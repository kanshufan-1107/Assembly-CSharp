using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeadManCopyHandToDeckSpell : SuperSpell
{
	private struct ActorCallbackData
	{
		public int targetIndex;

		public int handIndex;

		public int handSize;
	}

	public float m_MoveUpTime;

	public float m_MoveUpOffsetZ;

	public float m_MoveUpScale;

	public float m_MoveToDeckInterval;

	public bool m_ShuffleRealHandToDeck;

	[Tooltip("Used to prevent this spell from waiting on card draw, when card draw is waiting on this spell.")]
	public bool m_dontWaitForFriendlyCardDraw;

	private int m_taskCountToRunFirst;

	private bool m_waitForTasksToComplete;

	private List<Entity> m_entitiesToDrawBeforeFX = new List<Entity>();

	private List<Actor> m_actors = new List<Actor>();

	private List<Actor> m_friendlyActors = new List<Actor>();

	private List<Actor> m_opposingActors = new List<Actor>();

	private int m_numActorsInLoading;

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
			if (card == null)
			{
				continue;
			}
			Entity entity = card.GetEntity();
			if (entity != null && GameState.Get()?.GetEntity(entity.GetRealTimeParentEntityId()) == null && IsValidSpellTarget(entity) && !m_targets.Contains(card.gameObject))
			{
				AddTarget(card.gameObject);
				card.SuppressHandToDeckTransition();
				if (m_targets.Count == 1)
				{
					m_taskCountToRunFirst = i;
				}
			}
		}
		return m_targets.Count != 0;
	}

	protected override Card GetTargetCardFromPowerTask(int index, PowerTask task)
	{
		Network.PowerHistory power = task.GetPower();
		if (power.Type == Network.PowerType.TAG_CHANGE)
		{
			Network.HistTagChange tagChange = (Network.HistTagChange)power;
			if (tagChange.Tag != 49)
			{
				return null;
			}
			if (tagChange.Value != 2)
			{
				return null;
			}
			Entity entity = GameState.Get().GetEntity(tagChange.Entity);
			if (entity == null)
			{
				return null;
			}
			if (entity.GetZone() != TAG_ZONE.HAND && entity.GetZone() != TAG_ZONE.SETASIDE)
			{
				return null;
			}
			return entity.GetCard();
		}
		if (power.Type == Network.PowerType.HIDE_ENTITY)
		{
			Network.HistHideEntity hideEntity = (Network.HistHideEntity)power;
			if (hideEntity.Zone != 2)
			{
				return null;
			}
			Entity entity2 = GameState.Get().GetEntity(hideEntity.Entity);
			if (entity2 == null)
			{
				return null;
			}
			if (entity2.GetZone() != TAG_ZONE.HAND)
			{
				return null;
			}
			return entity2.GetCard();
		}
		if (power.Type == Network.PowerType.FULL_ENTITY)
		{
			Network.Entity netEnt = ((Network.HistFullEntity)power).Entity;
			if (netEnt == null)
			{
				return null;
			}
			return GameState.Get().GetEntity(netEnt.ID)?.GetCard();
		}
		return null;
	}

	protected override void OnAction(SpellStateType prevStateType)
	{
		m_effectsPendingFinish++;
		base.OnAction(prevStateType);
		FindEntitiesToDrawBeforeFX();
		DoTasks();
		StartCoroutine(DoActionWithTiming());
	}

	protected override void DoActionNow()
	{
	}

	private IEnumerator DoActionWithTiming()
	{
		if (m_ShuffleRealHandToDeck)
		{
			yield return StartCoroutine(WaitForPendingCardDraw());
		}
		yield return StartCoroutine(WaitForTasksAndDrawing());
		yield return StartCoroutine(LoadAssets());
		yield return StartCoroutine(DoEffects());
	}

	private void FindEntitiesToDrawBeforeFX()
	{
		Card source = GetSourceCard();
		m_entitiesToDrawBeforeFX.Clear();
		List<PowerTask> tasks = m_taskList.GetTaskList();
		for (int i = 0; i < m_taskCountToRunFirst; i++)
		{
			PowerTask task = tasks[i];
			if (source.GetControllerSide() == Player.Side.FRIENDLY)
			{
				FindRevealedEntitiesToDrawBeforeFX(task.GetPower());
				continue;
			}
			FindRevealedEntitiesToDrawBeforeFX(task.GetPower());
			FindHiddenEntitiesToDrawBeforeFX(task.GetPower());
		}
	}

	private void FindRevealedEntitiesToDrawBeforeFX(Network.PowerHistory power)
	{
		if (power.Type != Network.PowerType.SHOW_ENTITY)
		{
			return;
		}
		Network.HistShowEntity showEntity = (Network.HistShowEntity)power;
		Entity entity = GameState.Get().GetEntity(showEntity.Entity.ID);
		if (entity != null && entity.GetZone() == TAG_ZONE.DECK)
		{
			if (showEntity.Entity.Tags.Exists((Network.Entity.Tag tag) => tag.Name == 49 && tag.Value == 3))
			{
				m_entitiesToDrawBeforeFX.Add(entity);
			}
			else if (showEntity.Entity.Tags.Exists((Network.Entity.Tag tag) => tag.Name == 49 && tag.Value == 4))
			{
				m_entitiesToDrawBeforeFX.Add(entity);
			}
		}
	}

	private void FindHiddenEntitiesToDrawBeforeFX(Network.PowerHistory power)
	{
		if (power.Type != Network.PowerType.TAG_CHANGE)
		{
			return;
		}
		Network.HistTagChange tagChange = (Network.HistTagChange)power;
		if (tagChange.Tag == 49 && (tagChange.Value == 3 || tagChange.Value == 4))
		{
			Entity entity = GameState.Get().GetEntity(tagChange.Entity);
			if (entity == null)
			{
				Debug.LogWarningFormat("{0}.FindOpponentEntitiesToDrawBeforeFX() - WARNING trying to target entity with id {1} but there is no entity with that id", this, tagChange.Entity);
			}
			else if (entity.GetZone() == TAG_ZONE.DECK)
			{
				m_entitiesToDrawBeforeFX.Add(entity);
			}
		}
	}

	private void DoTasks()
	{
		if (m_taskCountToRunFirst <= 0)
		{
			m_waitForTasksToComplete = false;
			return;
		}
		m_waitForTasksToComplete = true;
		m_taskList.DoTasks(0, m_taskCountToRunFirst, delegate
		{
			m_waitForTasksToComplete = false;
		});
	}

	private IEnumerator LoadAssets()
	{
		m_numActorsInLoading = m_targets.Count;
		m_actors.Clear();
		m_friendlyActors.Clear();
		m_opposingActors.Clear();
		int friendlyHandSize = 0;
		int opposingHandSize = 0;
		for (int i = 0; i < m_targets.Count; i++)
		{
			if (m_targets[i].GetComponent<Card>().GetEntity().IsControlledByFriendlySidePlayer())
			{
				friendlyHandSize++;
			}
			else
			{
				opposingHandSize++;
			}
		}
		int friendlyCardIndex = 0;
		int opposingCardIndex = 0;
		for (int j = 0; j < m_targets.Count; j++)
		{
			Entity entity = m_targets[j].GetComponent<Card>().GetEntity();
			bool isFriendly = entity.IsControlledByFriendlySidePlayer();
			m_actors.Add(null);
			if (isFriendly)
			{
				m_friendlyActors.Add(null);
			}
			else
			{
				m_opposingActors.Add(null);
			}
			string assetPath = ActorNames.GetZoneActor(entity, TAG_ZONE.HAND);
			ActorCallbackData actorCallbackData = default(ActorCallbackData);
			actorCallbackData.targetIndex = j;
			actorCallbackData.handIndex = (isFriendly ? friendlyCardIndex++ : opposingCardIndex++);
			actorCallbackData.handSize = (isFriendly ? friendlyHandSize : opposingHandSize);
			ActorCallbackData callbackData = actorCallbackData;
			if (!AssetLoader.Get().InstantiatePrefab(assetPath, OnActorLoadAttempted, callbackData, AssetLoadingOptions.IgnorePrefabPosition))
			{
				OnActorLoadAttempted(assetPath, null, callbackData);
			}
		}
		while (m_numActorsInLoading > 0)
		{
			yield return null;
		}
	}

	private void OnActorLoadAttempted(AssetReference assetRef, GameObject go, object callbackData)
	{
		m_numActorsInLoading--;
		ActorCallbackData obj = (ActorCallbackData)callbackData;
		int targetIndex = obj.targetIndex;
		int handIndex = obj.handIndex;
		int handSize = obj.handSize;
		if (go == null)
		{
			Error.AddDevFatal("DeadManCopyHandToDeckSpell.OnActorLoadAttempted() - FAILED to load actor {0} (targetIndex {1})", assetRef, targetIndex);
			return;
		}
		Actor actor = go.GetComponent<Actor>();
		Card card = m_targets[targetIndex].GetComponent<Card>();
		Entity entity = card.GetEntity();
		ZoneHand hand = card.GetController().GetHandZone();
		if (card != null && !card.HasCardDef && entity != null)
		{
			string cardId = entity.GetCardId();
			if (!string.IsNullOrEmpty(cardId))
			{
				card.SetCardDef(DefLoader.Get().GetCardDef(cardId), updateActor: false);
			}
		}
		card.SetDoNotSort(on: true);
		actor.SetCard(card);
		actor.SetCardDefFromCard(card);
		actor.SetEntity(entity);
		actor.SetEntityDef(entity.GetEntityDef());
		actor.SetCardBackSideOverride(entity.GetControllerSide());
		actor.UpdateAllComponents();
		card.transform.position = hand.GetCardPosition(handIndex, handSize);
		card.transform.localEulerAngles = hand.GetCardRotation(handIndex, handSize);
		card.transform.localScale = hand.GetCardScale();
		actor.Hide();
		m_actors[targetIndex] = actor;
		if (entity.IsControlledByFriendlySidePlayer())
		{
			m_friendlyActors[handIndex] = actor;
		}
		else
		{
			m_opposingActors[handIndex] = actor;
		}
	}

	private IEnumerator WaitForPendingCardDraw()
	{
		Card source = GetSourceCard();
		if (source == null)
		{
			yield break;
		}
		Entity entity = source.GetEntity();
		if (entity == null)
		{
			yield break;
		}
		if (entity.IsControlledByFriendlySidePlayer())
		{
			while ((bool)GameState.Get().GetFriendlyCardBeingDrawn() && !m_dontWaitForFriendlyCardDraw)
			{
				yield return null;
			}
			while (GameState.Get().GetFriendlySidePlayer().GetHandZone()
				.IsUpdatingLayout())
			{
				yield return null;
			}
		}
		else
		{
			while ((bool)GameState.Get().GetOpponentCardBeingDrawn())
			{
				yield return null;
			}
			while (GameState.Get().GetOpposingSidePlayer().GetHandZone()
				.IsUpdatingLayout())
			{
				yield return null;
			}
		}
	}

	private bool IsDrawing()
	{
		foreach (Entity entity in m_entitiesToDrawBeforeFX)
		{
			Card card = entity.GetCard();
			switch (entity.GetZone())
			{
			case TAG_ZONE.GRAVEYARD:
				if (!card.IsActorReady())
				{
					return true;
				}
				break;
			case TAG_ZONE.HAND:
				if (!(card.GetZone() is ZoneHand))
				{
					return true;
				}
				if (card.IsDoNotSort())
				{
					return true;
				}
				if (entity.IsControlledByFriendlySidePlayer())
				{
					if (!card.CardStandInIsInteractive())
					{
						return true;
					}
				}
				else if (card.IsBeingDrawnByOpponent())
				{
					return true;
				}
				break;
			}
		}
		return false;
	}

	private IEnumerator WaitForTasksAndDrawing()
	{
		while (m_waitForTasksToComplete)
		{
			yield return null;
		}
		while (IsDrawing())
		{
			yield return null;
		}
	}

	private void CheckHideOriginalHandActors()
	{
		if (!m_ShuffleRealHandToDeck)
		{
			return;
		}
		if (m_opposingActors.Count > 0)
		{
			foreach (Card card in GameState.Get().GetOpposingSidePlayer().GetHandZone()
				.GetCards())
			{
				Actor originalActor = card.GetActor();
				if (!(originalActor == null) && CardInActorList(card, m_opposingActors))
				{
					originalActor.Hide();
				}
			}
		}
		if (m_friendlyActors.Count <= 0)
		{
			return;
		}
		foreach (Card card2 in GameState.Get().GetFriendlySidePlayer().GetHandZone()
			.GetCards())
		{
			Actor originalActor2 = card2.GetActor();
			if (!(originalActor2 == null) && CardInActorList(card2, m_friendlyActors))
			{
				originalActor2.Hide();
			}
		}
	}

	private bool CardInActorList(Card card, List<Actor> actorList)
	{
		int originalCardId = card.GetEntity().GetEntityId();
		foreach (Actor actor in actorList)
		{
			if (!(actor == null) && actor.GetEntity().GetEntityId() == originalCardId)
			{
				return true;
			}
		}
		return false;
	}

	private IEnumerator DoEffects()
	{
		base.DoActionNow();
		CheckHideOriginalHandActors();
		AnimateSpread();
		Actor livingActor;
		do
		{
			livingActor = m_actors.Find((Actor currActor) => currActor);
			if ((bool)livingActor)
			{
				yield return null;
			}
		}
		while ((bool)livingActor);
		m_effectsPendingFinish--;
		FinishIfPossible();
	}

	private void AnimateSpread()
	{
		for (int i = 0; i < m_opposingActors.Count || i < m_friendlyActors.Count; i++)
		{
			if (i < m_opposingActors.Count)
			{
				float waitSec = (float)(m_opposingActors.Count - i - 1) * m_MoveToDeckInterval;
				StartCoroutine(AnimateActor(m_opposingActors[i], waitSec));
			}
			if (i < m_friendlyActors.Count)
			{
				float waitSec2 = (float)(m_friendlyActors.Count - i - 1) * m_MoveToDeckInterval;
				StartCoroutine(AnimateActor(m_friendlyActors[i], waitSec2));
			}
		}
	}

	private IEnumerator AnimateActor(Actor actor, float waitSec)
	{
		Card card = actor.GetCard();
		Player player = card.GetController();
		ZoneDeck deck = player.GetDeckZone();
		actor.Show();
		float slideAmount = (player.IsFriendlySide() ? m_MoveUpOffsetZ : (0f - m_MoveUpOffsetZ));
		iTween.MoveTo(position: new Vector3(card.transform.position.x, card.transform.position.y, card.transform.position.z + slideAmount), target: card.gameObject, time: m_MoveUpTime);
		iTween.ScaleTo(card.gameObject, card.transform.localScale * m_MoveUpScale, m_MoveUpTime);
		yield return new WaitForSeconds(m_MoveUpTime + waitSec);
		bool hiddenActor = actor.GetEntityDef().GetCardType() == TAG_CARDTYPE.INVALID;
		yield return StartCoroutine(actor.GetCard().AnimatePlayToDeck(actor.gameObject, deck, hiddenActor));
		actor.Destroy();
		card.SetDoNotSort(on: false);
	}
}
