using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnToDeckSpell : SuperSpell
{
	public enum HandActorSource
	{
		CHOSEN_TARGET,
		OVERRIDE,
		SPELL_TARGET,
		ENTITY_TARGET
	}

	public enum SpreadType
	{
		STACK,
		SEQUENCE,
		CUSTOM_SPELL
	}

	[Serializable]
	public class StackData
	{
		public float m_RevealTime = 1f;

		public float m_StaggerTime = 1.2f;
	}

	[Serializable]
	public class SequenceData
	{
		public float m_Spacing = 2.1f;

		public float m_RevealTime = 0.6f;

		public float m_NextCardRevealTimeMin = 0.1f;

		public float m_NextCardRevealTimeMax = 0.2f;

		public float m_HoldTime = 0.3f;

		public float m_NextCardHoldTime = 0.4f;
	}

	private struct RevealSpellFinishedCallbackData
	{
		public Actor actor;

		public Card card;

		public int id;
	}

	private const float PHONE_HAND_OFFSET = 1.5f;

	private const int SEQUENCE_BATCH_SIZE = 5;

	private const float SEQUENCE_BATCH_REVEAL_TIME = 0.3f;

	private const float SEQUENCE_BATCH_HOLD_TIME = 0f;

	private const float SEQUENCE_BATCH_NEXT_CARD_HOLD_TIME = 0.2f;

	public HandActorSource m_HandActorSource;

	public string m_OverrideCardId;

	public List<string> m_OverrideCardIds = new List<string>();

	public float m_CardDelay;

	public float m_CardAnimatePlayToDeckTimeScale = 1f;

	public float m_RevealStartScale = 0.1f;

	public float m_RevealYOffsetMin = 5f;

	public float m_RevealYOffsetMax = 5f;

	public float m_RevealFriendlySideZOffset;

	public float m_RevealOpponentSideZOffset;

	public Vector3 m_RevealBaseOffset = Vector3.zero;

	public SpreadType m_SpreadType;

	public StackData m_StackData = new StackData();

	public SequenceData m_SequenceData = new SequenceData();

	public Spell m_customRevealSpell;

	public bool m_VisibleByDefault = true;

	private List<DefLoader.DisposableCardDef> m_overrideCardDefs = new List<DefLoader.DisposableCardDef>();

	private List<Actor> m_loadedActors;

	[HideInInspector]
	public bool m_finishedLoading;

	protected override void OnDestroy()
	{
		m_overrideCardDefs.DisposeValuesAndClear();
		base.OnDestroy();
	}

	public override bool AddPowerTargets()
	{
		m_visualToTargetIndexMap.Clear();
		m_targetToMetaDataMap.Clear();
		m_targets.Clear();
		List<PowerTask> tasks = m_taskList.GetTaskList();
		for (int i = 0; i < tasks.Count; i++)
		{
			PowerTask task = tasks[i];
			Card card = GetTargetCardFromPowerTask(i, task);
			if (!(card == null) && IsValidSpellTarget(card.GetEntity()))
			{
				AddTarget(card.gameObject);
			}
		}
		if (m_targets.Count > 0)
		{
			return true;
		}
		return false;
	}

	protected override Card GetTargetCardFromPowerTask(int index, PowerTask task)
	{
		Network.PowerHistory power = task.GetPower();
		if (power.Type != Network.PowerType.FULL_ENTITY)
		{
			return null;
		}
		Network.Entity netEnt = (power as Network.HistFullEntity).Entity;
		Entity entity = GameState.Get().GetEntity(netEnt.ID);
		if (entity == null)
		{
			Debug.LogWarning($"{this}.GetTargetCardFromPowerTask() - WARNING trying to target entity with id {netEnt.ID} but there is no entity with that id");
			return null;
		}
		if (entity.GetZone() != TAG_ZONE.DECK)
		{
			return null;
		}
		return entity.GetCard();
	}

	protected override void OnAction(SpellStateType prevStateType)
	{
		m_effectsPendingFinish++;
		base.OnAction(prevStateType);
		StartCoroutine(DoActionWithTiming());
	}

	private IEnumerator ProcessShowEntityForTargets()
	{
		PowerTaskList taskList = GetPowerTaskList();
		foreach (PowerTask task in taskList.GetTaskList())
		{
			if (!(task.GetPower() is Network.HistShowEntity { Entity: var netEnt }))
			{
				continue;
			}
			Entity target = FindTargetEntity(netEnt.ID);
			if (target == null)
			{
				continue;
			}
			foreach (Network.Entity.Tag netTag in netEnt.Tags)
			{
				target.SetTag(netTag.Name, netTag.Value);
			}
			target.LoadCard(netEnt.CardID);
			while (target.IsLoadingAssets())
			{
				yield return null;
			}
		}
	}

	private Entity FindTargetEntity(int entityID)
	{
		foreach (GameObject target in m_targets)
		{
			Card card = target.GetComponent<Card>();
			if (!(card == null))
			{
				Entity entity = card.GetEntity();
				if (entity != null && entity.GetEntityId() == entityID)
				{
					return entity;
				}
			}
		}
		Entity chosenEntity = GetPowerTarget();
		if (chosenEntity != null && chosenEntity.GetEntityId() == entityID)
		{
			return chosenEntity;
		}
		return null;
	}

	private IEnumerator DoActionWithTiming()
	{
		m_loadedActors = new List<Actor>(m_targets.Count);
		yield return StartCoroutine(ProcessShowEntityForTargets());
		yield return StartCoroutine(LoadAssets(m_loadedActors));
		yield return new WaitForSeconds(m_CardDelay);
		yield return StartCoroutine(DoEffects(m_loadedActors));
	}

	private IEnumerator LoadAssets(List<Actor> actors)
	{
		if (base.transform.position == Vector3.zero)
		{
			Player.Side sourceSide = GetSourceCard().GetControllerSide();
			base.transform.position = ZoneMgr.Get().FindZoneOfType<ZoneHero>(sourceSide).transform.position;
		}
		bool loadingOverrideCardDef = false;
		if (m_OverrideCardIds.Count == 0)
		{
			m_OverrideCardIds.Add(m_OverrideCardId);
		}
		int j = 0;
		while (j < m_OverrideCardIds.Count)
		{
			if (!string.IsNullOrEmpty(m_OverrideCardIds[j]))
			{
				loadingOverrideCardDef = true;
				DefLoader.LoadDefCallback<DefLoader.DisposableCardDef> cardDefCallback = delegate(string cardId, DefLoader.DisposableCardDef def, object userData)
				{
					loadingOverrideCardDef = false;
					if (def == null)
					{
						Error.AddDevFatal("SpawnToDeckSpell.LoadAssets() - FAILED to load CardDef for {0}", cardId);
					}
					else
					{
						m_overrideCardDefs.Add(def);
					}
				};
				DefLoader.Get().LoadCardDef(m_OverrideCardIds[j], cardDefCallback);
				while (loadingOverrideCardDef)
				{
					yield return null;
				}
			}
			int assetsLoading = 1;
			if (j == m_OverrideCardIds.Count - 1 && m_targets.Count > m_OverrideCardIds.Count)
			{
				assetsLoading = m_targets.Count - m_OverrideCardIds.Count + 1;
			}
			PrefabCallback<GameObject> actorCallback = delegate(AssetReference assetRef, GameObject go, object callbackData)
			{
				int num = assetsLoading - 1;
				assetsLoading = num;
				int num2 = (int)callbackData;
				if (num2 <= m_targets.Count - 1)
				{
					if (go == null)
					{
						Error.AddDevFatal("SpawnToDeckSpell.LoadAssets() - FAILED to load actor {0} (targetIndex {1})", base.name, num2);
					}
					else
					{
						Actor component = go.GetComponent<Actor>();
						Card component2 = m_targets[num2].GetComponent<Card>();
						Entity entity = component2.GetEntity();
						if (entity.GetLoadState() == Entity.LoadState.DONE)
						{
							component.SetEntity(entity);
						}
						else
						{
							component.SetPremium(GetPremium(entity));
							if (m_HandActorSource == HandActorSource.CHOSEN_TARGET)
							{
								Entity powerTarget = GetPowerTarget();
								if (powerTarget != null)
								{
									string cardTextInHand = powerTarget.GetCardTextInHand();
									component.SetCardDefPowerTextOverride(cardTextInHand);
								}
							}
						}
						if (m_HandActorSource != HandActorSource.ENTITY_TARGET)
						{
							component.SetEntityDef(GetEntityDef(entity, num2));
						}
						using (DefLoader.DisposableCardDef cardDef = ShareDisposableCardDef(component2, num2))
						{
							component.SetCardDef(cardDef);
						}
						component.SetCardBackSideOverride(entity.GetControllerSide());
						component.UpdateAllComponents();
						component.Hide();
						actors[num2] = component;
						OnActorLoaded(component);
					}
				}
			};
			int targetRounds = assetsLoading;
			for (int i = 0; i < targetRounds; i++)
			{
				Entity entity2 = m_targets[Math.Min(j + i, m_targets.Count - 1)].GetComponent<Card>().GetEntity();
				TAG_PREMIUM premium = GetPremium(entity2);
				string assetRef2 = GetAssetRef(entity2, premium, j + i);
				if (actors.Count < m_targets.Count)
				{
					actors.Add(null);
				}
				object callbackData2 = j + i;
				if (!AssetLoader.Get().InstantiatePrefab(assetRef2, actorCallback, callbackData2, AssetLoadingOptions.IgnorePrefabPosition))
				{
					actorCallback(assetRef2, null, callbackData2);
				}
			}
			while (assetsLoading > 0)
			{
				yield return null;
			}
			int num3 = j + 1;
			j = num3;
		}
		m_finishedLoading = true;
	}

	protected virtual void OnActorLoaded(Actor actor)
	{
	}

	private IEnumerator DoEffects(List<Actor> actors)
	{
		StartCoroutine(AnimateSpread(actors));
		Actor livingActor;
		do
		{
			livingActor = actors.Find((Actor currActor) => currActor);
			if ((bool)livingActor)
			{
				yield return null;
			}
		}
		while ((bool)livingActor);
		m_effectsPendingFinish--;
		FinishIfPossible();
	}

	private float GetRevealSec(int iterationCount)
	{
		if (iterationCount > 0)
		{
			return 0.3f;
		}
		return m_SequenceData.m_RevealTime;
	}

	private float GetHoldSec(int iterationCount)
	{
		if (iterationCount > 0)
		{
			return 0f;
		}
		return m_SequenceData.m_HoldTime;
	}

	private float GetNextCardHoldSec(int iterationCount)
	{
		if (iterationCount > 0)
		{
			return 0.2f;
		}
		return m_SequenceData.m_NextCardHoldTime;
	}

	private IEnumerator WaitForBatchToAnimate(int batchSize, int iterationCount)
	{
		float batchPauseSec = (m_SequenceData.m_NextCardRevealTimeMax + GetNextCardHoldSec(iterationCount)) * (float)(batchSize - 1) + GetRevealSec(iterationCount) + GetHoldSec(iterationCount);
		yield return new WaitForSeconds(batchPauseSec);
	}

	private void AnimateSequence(List<Actor> actors, int iterationCount)
	{
		List<Vector3> revealPositions = new List<Vector3>();
		float startOffset = -0.5f * (float)(actors.Count - 1) * m_SequenceData.m_Spacing;
		for (int i = 0; i < actors.Count; i++)
		{
			float currOffset = (float)i * m_SequenceData.m_Spacing;
			Vector3 offset = new Vector3(startOffset + currOffset, 0f, 0f);
			Vector3 revealPos = ComputeRevealPosition(offset);
			revealPositions.Add(revealPos);
		}
		BoundRevealPositions(actors, revealPositions);
		PreventHandOverlapPhone(actors, revealPositions);
		float baseRevealSec = GetRevealSec(iterationCount);
		float baseHoldTimeSec = GetHoldSec(iterationCount);
		float baseNextCardHoldSec = GetNextCardHoldSec(iterationCount);
		List<float> revealSecs = RandomizeRevealTimes(actors.Count, baseRevealSec, m_SequenceData.m_NextCardRevealTimeMin, m_SequenceData.m_NextCardRevealTimeMax);
		float maxRevealSec = Mathf.Max(revealSecs.ToArray());
		for (int j = 0; j < actors.Count; j++)
		{
			Vector3 revealPos2 = revealPositions[j];
			float revealSec = revealSecs[j];
			float currHoldSec = (float)(actors.Count - 1 - j) * baseNextCardHoldSec;
			float holdSec = baseHoldTimeSec + currHoldSec;
			float waitSec = maxRevealSec + holdSec;
			StartCoroutine(AnimateActor(actors, j, revealSec, revealPos2, waitSec));
		}
	}

	private IEnumerator AnimateSpread(List<Actor> actors)
	{
		if (m_SpreadType == SpreadType.SEQUENCE)
		{
			List<Actor> batchedActors = new List<Actor>();
			int iterationCount = 0;
			foreach (Actor actor in actors)
			{
				if (batchedActors.Count == 5)
				{
					AnimateSequence(batchedActors, iterationCount);
					yield return WaitForBatchToAnimate(batchedActors.Count, iterationCount);
					iterationCount++;
					batchedActors.Clear();
				}
				batchedActors.Add(actor);
			}
			if (batchedActors.Count > 0)
			{
				AnimateSequence(batchedActors, iterationCount);
				yield return WaitForBatchToAnimate(batchedActors.Count, iterationCount);
			}
		}
		else if (m_SpreadType == SpreadType.STACK)
		{
			int iterationCount = 0;
			while (iterationCount < actors.Count)
			{
				Vector3 revealPos = ComputeRevealPosition(Vector3.zero);
				StartCoroutine(AnimateActor(actors, iterationCount, m_StackData.m_RevealTime, revealPos, m_StackData.m_RevealTime));
				if (iterationCount < actors.Count - 1)
				{
					yield return new WaitForSeconds(m_StackData.m_StaggerTime);
				}
				int num = iterationCount + 1;
				iterationCount = num;
			}
		}
		else if (m_SpreadType == SpreadType.CUSTOM_SPELL)
		{
			for (int i = 0; i < actors.Count; i++)
			{
				AnimateActorUsingSpell(actors, i);
			}
		}
	}

	private void AnimateActorUsingSpell(List<Actor> actors, int index)
	{
		Actor actor = actors[index];
		GameObject targetGameObject = m_targets[index];
		Card card = targetGameObject.GetComponent<Card>();
		actor.transform.localScale = new Vector3(m_RevealStartScale, m_RevealStartScale, m_RevealStartScale);
		actor.transform.rotation = base.transform.rotation;
		actor.transform.position = base.transform.position;
		if (m_VisibleByDefault)
		{
			actor.Show();
		}
		RevealSpellFinishedCallbackData callbackData = default(RevealSpellFinishedCallbackData);
		callbackData.actor = actor;
		callbackData.card = card;
		callbackData.id = index;
		if (m_customRevealSpell == null)
		{
			Log.Spells.PrintError("SpawnToDeckSpell.AnimateSpread(): m_SpreadType is set to CUSTOM_SPELL, but m_customRevealSpell is null!");
			OnRevealSpellFinished(null, callbackData);
			return;
		}
		Spell spell = SpellManager.Get().GetSpell(m_customRevealSpell);
		SpellUtils.SetCustomSpellParent(spell, actor);
		spell.AddFinishedCallback(OnRevealSpellFinished, callbackData);
		spell.SetSource(GetSource());
		spell.AddTarget(targetGameObject);
		spell.Activate();
	}

	public void OnRevealSpellFinished(Spell spell, object userData)
	{
		RevealSpellFinishedCallbackData callbackData = (RevealSpellFinishedCallbackData)userData;
		Actor actor = callbackData.actor;
		Card card = callbackData.card;
		Entity entity = card.GetEntity();
		ZoneDeck deck = entity.GetController().GetDeckZone();
		bool hiddenActor = GetEntityDef(entity, callbackData.id).GetCardType() == TAG_CARDTYPE.INVALID;
		StartCoroutine(AnimateRevealedCardToDeck(actor, card, deck, hiddenActor));
	}

	public IEnumerator AnimateRevealedCardToDeck(Actor actor, Card card, ZoneDeck deck, bool hideBackSide)
	{
		yield return StartCoroutine(card.AnimatePlayToDeck(actor.gameObject, deck, hideBackSide, m_CardAnimatePlayToDeckTimeScale));
		actor.Destroy();
	}

	private Vector3 ComputeRevealPosition(Vector3 offset)
	{
		Vector3 revealPos = base.transform.position;
		float yOffset = UnityEngine.Random.Range(m_RevealYOffsetMin, m_RevealYOffsetMax);
		revealPos.y += yOffset;
		switch (GetSourceCard().GetControllerSide())
		{
		case Player.Side.FRIENDLY:
			revealPos.z += m_RevealFriendlySideZOffset;
			break;
		case Player.Side.OPPOSING:
			revealPos.z += m_RevealOpponentSideZOffset;
			break;
		}
		revealPos += m_RevealBaseOffset;
		return revealPos + offset;
	}

	private void PreventHandOverlapPhone(List<Actor> actors, List<Vector3> revealPositions)
	{
		if (!UniversalInputManager.UsePhoneUI)
		{
			return;
		}
		Entity entity = GetPowerTarget();
		if (entity != null)
		{
			if (entity.GetControllerSide() == Player.Side.OPPOSING)
			{
				return;
			}
		}
		else
		{
			Card sourceCard = GetSourceCard();
			if (sourceCard != null && sourceCard.GetControllerSide() == Player.Side.OPPOSING)
			{
				return;
			}
		}
		for (int i = 0; i < revealPositions.Count; i++)
		{
			Vector3 revealPos = revealPositions[i];
			revealPos = new Vector3(revealPos.x, revealPos.y, revealPos.z + 1.5f);
			revealPositions[i] = revealPos;
		}
	}

	private void BoundRevealPositions(List<Actor> actors, List<Vector3> revealPositions)
	{
		float maxScreenOverlapX = float.MinValue;
		float minScreenOverlapX = float.MaxValue;
		for (int i = 0; i < revealPositions.Count; i++)
		{
			ZoneDeck deckZone = m_targets[i].GetComponent<Card>().GetEntity().GetController()
				.GetDeckZone();
			float deckThickness = 0f;
			Actor deckActor = deckZone.GetActiveThickness();
			if (deckActor != null)
			{
				deckThickness = deckActor.GetMeshRenderer().bounds.extents.x;
			}
			Vector3 deckWorldLeftPos = deckZone.transform.position;
			deckWorldLeftPos.x -= deckThickness;
			Vector3 revealWorldRightPos = revealPositions[i];
			revealWorldRightPos.x += actors[i].GetMeshRenderer().bounds.extents.x;
			Vector3 revealWorldLeftPos = revealPositions[i];
			revealWorldLeftPos.x -= actors[i].GetMeshRenderer().bounds.extents.x;
			Vector3 deckScreenLeftPos = Camera.main.WorldToScreenPoint(deckWorldLeftPos);
			Vector3 vector = Camera.main.WorldToScreenPoint(revealWorldRightPos);
			Vector3 revealScreenLeftPos = Camera.main.WorldToScreenPoint(revealWorldLeftPos);
			float screenOverlapDeckX = vector.x - deckScreenLeftPos.x;
			if (screenOverlapDeckX > maxScreenOverlapX)
			{
				maxScreenOverlapX = screenOverlapDeckX;
			}
			float screenOverlapLeftsideX = revealScreenLeftPos.x;
			if (screenOverlapLeftsideX < minScreenOverlapX)
			{
				minScreenOverlapX = screenOverlapLeftsideX;
			}
		}
		if (!(minScreenOverlapX >= 0f) || !(maxScreenOverlapX <= 0f))
		{
			float screenOffsetX = 0f;
			float worldOffsetX = CameraUtils.ScreenToWorldDist(screenDist: (!(maxScreenOverlapX > 0f)) ? Math.Max(minScreenOverlapX, maxScreenOverlapX) : maxScreenOverlapX, camera: Camera.main, worldPoint: revealPositions[0]);
			for (int j = 0; j < revealPositions.Count; j++)
			{
				Vector3 revealPos = revealPositions[j];
				revealPos.x -= worldOffsetX;
				revealPositions[j] = revealPos;
			}
		}
	}

	private List<float> RandomizeRevealTimes(int count, float revealSec, float nextRevealSecMin, float nextRevealSecMax)
	{
		List<float> secs = new List<float>(count);
		List<int> randomIndexes = new List<int>(count);
		for (int i = 0; i < count; i++)
		{
			secs.Add(0f);
			randomIndexes.Add(i);
		}
		float runningSec = revealSec;
		for (int j = 0; j < count; j++)
		{
			int randomIndex = UnityEngine.Random.Range(0, randomIndexes.Count);
			int secIndex = randomIndexes[randomIndex];
			randomIndexes.RemoveAt(randomIndex);
			secs[secIndex] = runningSec;
			float currSec = UnityEngine.Random.Range(nextRevealSecMin, nextRevealSecMax);
			runningSec += currSec;
		}
		return secs;
	}

	private IEnumerator AnimateActor(List<Actor> actors, int index, float revealSec, Vector3 revealPos, float waitSec)
	{
		Actor actor = actors[index];
		GameObject targetObject = m_targets[index];
		Card card = targetObject.GetComponent<Card>();
		Entity entity = card.GetEntity();
		Player controller = entity.GetController();
		ZonePlay battlefieldZone = controller.GetBattlefieldZone();
		ZoneDeck deck = controller.GetDeckZone();
		actor.transform.localScale = new Vector3(m_RevealStartScale, m_RevealStartScale, m_RevealStartScale);
		actor.transform.rotation = base.transform.rotation;
		actor.transform.position = base.transform.position;
		if (m_VisibleByDefault)
		{
			actor.Show();
		}
		Vector3 revealScale = Vector3.one;
		Vector3 revealAngles = battlefieldZone.transform.rotation.eulerAngles;
		iTween.MoveTo(actor.gameObject, iTween.Hash("position", revealPos, "time", revealSec, "easetype", iTween.EaseType.easeOutExpo));
		iTween.RotateTo(actor.gameObject, iTween.Hash("rotation", revealAngles, "time", revealSec, "easetype", iTween.EaseType.easeOutExpo));
		iTween.ScaleTo(actor.gameObject, iTween.Hash("scale", revealScale, "time", revealSec, "easetype", iTween.EaseType.easeOutExpo));
		if (waitSec > 0f)
		{
			yield return new WaitForSeconds(waitSec);
		}
		bool hiddenActor = GetEntityDef(entity, index).GetCardType() == TAG_CARDTYPE.INVALID;
		yield return StartCoroutine(card.AnimatePlayToDeck(actor.gameObject, deck, hiddenActor, m_CardAnimatePlayToDeckTimeScale));
		actor.Destroy();
	}

	private TAG_PREMIUM GetPremium(Entity entity)
	{
		TAG_PREMIUM sourcePremium = GetSourceCard().GetEntity().GetPremiumType();
		switch (m_HandActorSource)
		{
		case HandActorSource.CHOSEN_TARGET:
		{
			TAG_PREMIUM chosenPremium = GetPowerTarget().GetPremiumType();
			if (sourcePremium <= chosenPremium)
			{
				return chosenPremium;
			}
			return sourcePremium;
		}
		case HandActorSource.OVERRIDE:
			if (m_overrideCardDefs.Count > 0)
			{
				sourcePremium = m_overrideCardDefs[m_overrideCardDefs.Count - 1].CardDef.GetPortraitQuality().PremiumType;
			}
			return sourcePremium;
		default:
			return entity.GetPremiumType();
		}
	}

	private string GetAssetRef(Entity entity, TAG_PREMIUM premium, int index = 0)
	{
		string assetRef = null;
		return m_HandActorSource switch
		{
			HandActorSource.CHOSEN_TARGET => ActorNames.GetHandActor(GetPowerTarget().GetEntityDef(), premium), 
			HandActorSource.OVERRIDE => ActorNames.GetHandActor(DefLoader.Get().GetEntityDef(m_OverrideCardIds[Math.Min(index, m_OverrideCardIds.Count - 1)]), premium), 
			HandActorSource.SPELL_TARGET => ActorNames.GetHandActor(entity.GetEntityDef(), premium), 
			HandActorSource.ENTITY_TARGET => ActorNames.GetHandActor(entity), 
			_ => ActorNames.GetHandActor(entity.GetEntityDef(), premium), 
		};
	}

	private EntityDef GetEntityDef(Entity entity, int index = 0)
	{
		return m_HandActorSource switch
		{
			HandActorSource.CHOSEN_TARGET => GetPowerTarget().GetEntityDef(), 
			HandActorSource.OVERRIDE => DefLoader.Get().GetEntityDef(m_OverrideCardIds[Math.Min(index, m_OverrideCardIds.Count - 1)]), 
			_ => entity.GetEntityDef(), 
		};
	}

	private DefLoader.DisposableCardDef ShareDisposableCardDef(Card card, int index = 0)
	{
		return m_HandActorSource switch
		{
			HandActorSource.CHOSEN_TARGET => GetPowerTargetCard().ShareDisposableCardDef(), 
			HandActorSource.OVERRIDE => m_overrideCardDefs[Math.Min(index, m_overrideCardDefs.Count - 1)]?.Share(), 
			_ => card.ShareDisposableCardDef(), 
		};
	}

	public List<Actor> GetLoadedActors()
	{
		return m_loadedActors;
	}
}
