using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemonicProjectPlaySpell : Spell
{
	private class ActorLoadData
	{
		public EntityDef entityDef;

		public Player.Side playerSide;

		public TAG_PREMIUM premium;
	}

	[SerializeField]
	private string m_FriendlyBoneName = "FriendlyJoust";

	[SerializeField]
	private string m_OpponentBoneName = "OpponentJoust";

	[SerializeField]
	private float m_MoveOldCardTime = 1f;

	[SerializeField]
	private float m_ShowNewCardTime = 1f;

	[SerializeField]
	private Spell m_TransformSpell;

	private List<int> m_newEntityIDs = new List<int>();

	private Actor[] m_newActors = new Actor[2];

	private int m_numNewActorsInLoading;

	private int m_numOldActorsInMoving;

	private List<Spell> m_activeSpells = new List<Spell>();

	protected override void OnAction(SpellStateType prevStateType)
	{
		InputManager.Get().DisableInput();
		StartCoroutine(DoEffectWithTiming());
		base.OnAction(prevStateType);
	}

	public override void OnSpellFinished()
	{
		InputManager.Get().EnableInput();
		base.OnSpellFinished();
	}

	private IEnumerator DoEffectWithTiming()
	{
		AddNewEntities();
		yield return StartCoroutine(CompleteTasksBeforeSetAside());
		yield return StartCoroutine(LoadAssets());
		yield return StartCoroutine(MoveOldCards());
		yield return StartCoroutine(PlayTransformFX());
		yield return StartCoroutine(ShowNewCards());
		yield return StartCoroutine(SwitchToRealCards());
		yield return StartCoroutine(WaitAndDeactivate());
	}

	private void AddNewEntities()
	{
		List<PowerTask> taskList = m_taskList.GetTaskList();
		for (int i = 0; i < taskList.Count; i++)
		{
			Network.PowerHistory power = taskList[i].GetPower();
			if (power.Type == Network.PowerType.FULL_ENTITY)
			{
				Network.HistFullEntity fullEntity = (Network.HistFullEntity)power;
				Network.Entity.Tag tag = fullEntity.Entity.Tags.Find((Network.Entity.Tag item) => item.Name == 49);
				if (tag != null && tag.Value == 6)
				{
					m_newEntityIDs.Add(fullEntity.Entity.ID);
				}
			}
		}
	}

	private int FindLastFullEntityTaskIndex()
	{
		List<PowerTask> taskList = m_taskList.GetTaskList();
		for (int i = taskList.Count - 1; i >= 0; i--)
		{
			if (taskList[i].GetPower().Type == Network.PowerType.FULL_ENTITY)
			{
				return i;
			}
		}
		return -1;
	}

	private IEnumerator CompleteTasksBeforeSetAside()
	{
		int taskIndex = FindLastFullEntityTaskIndex();
		if (taskIndex == -1)
		{
			OnSpellFinished();
			yield break;
		}
		int total = taskIndex + 1;
		m_taskList.DoTasks(0, total);
		List<PowerTask> powerTaskList = m_taskList.GetTaskList();
		int i = 0;
		while (i < total)
		{
			PowerTask task = powerTaskList[i];
			while (!task.IsCompleted())
			{
				yield return null;
			}
			int num = i + 1;
			i = num;
		}
	}

	private IEnumerator LoadAssets()
	{
		Entity sourceEntity = GetSourceCard().GetEntity();
		LoadActor(GAME_TAG.TAG_SCRIPT_DATA_ENT_1, GAME_TAG.TAG_SCRIPT_DATA_NUM_1, sourceEntity.IsControlledByFriendlySidePlayer());
		LoadActor(GAME_TAG.TAG_SCRIPT_DATA_ENT_2, GAME_TAG.TAG_SCRIPT_DATA_NUM_2, !sourceEntity.IsControlledByFriendlySidePlayer());
		if (m_numNewActorsInLoading == 0)
		{
			OnSpellFinished();
			yield break;
		}
		while (m_numNewActorsInLoading > 0)
		{
			yield return null;
		}
	}

	private void LoadActor(GAME_TAG tagDataEntity, GAME_TAG tagDataNum, bool friendly)
	{
		Entity sourceEntity = GetSourceCard().GetEntity();
		if (sourceEntity.HasTag(tagDataNum))
		{
			m_numNewActorsInLoading++;
			Entity entity = GameState.Get().GetEntity(sourceEntity.GetTag(tagDataEntity));
			int databaseID = sourceEntity.GetTag(tagDataNum);
			TAG_PREMIUM premium = ((entity.HasTag(GAME_TAG.PREMIUM) || sourceEntity.HasTag(GAME_TAG.PREMIUM)) ? TAG_PREMIUM.GOLDEN : TAG_PREMIUM.NORMAL);
			Player.Side playerSide = (friendly ? Player.Side.FRIENDLY : Player.Side.OPPOSING);
			EntityDef entityDef = DefLoader.Get().GetEntityDef(databaseID);
			string assetPath = ActorNames.GetHandActor(entityDef, premium);
			ActorLoadData data = new ActorLoadData
			{
				entityDef = entityDef,
				playerSide = playerSide,
				premium = premium
			};
			AssetLoader.Get().InstantiatePrefab(assetPath, OnActorLoaded, data, AssetLoadingOptions.IgnorePrefabPosition);
		}
	}

	private void OnActorLoaded(AssetReference assetRef, GameObject go, object callbackData)
	{
		m_numNewActorsInLoading--;
		Actor actor = go.GetComponent<Actor>();
		ActorLoadData data = (ActorLoadData)callbackData;
		actor.SetEntityDef(data.entityDef);
		actor.SetPremium(data.premium);
		actor.SetCardBackSideOverride(data.playerSide);
		actor.UpdateAllComponents();
		actor.Hide();
		m_newActors[(int)(data.playerSide - 1)] = actor;
	}

	private IEnumerator MoveOldCards()
	{
		MoveOldCard(GAME_TAG.TAG_SCRIPT_DATA_ENT_1);
		MoveOldCard(GAME_TAG.TAG_SCRIPT_DATA_ENT_2);
		if (m_numOldActorsInMoving == 0)
		{
			OnSpellFinished();
			yield break;
		}
		while (m_numOldActorsInMoving > 0)
		{
			yield return null;
		}
	}

	private void MoveOldCard(GAME_TAG tag)
	{
		Entity sourceEntity = GetSourceCard().GetEntity();
		if (sourceEntity.HasTag(tag))
		{
			m_numOldActorsInMoving++;
			Entity entity = GameState.Get().GetEntity(sourceEntity.GetTag(tag));
			Card card = entity.GetCard();
			if (entity.IsControlledByOpposingSidePlayer())
			{
				string assetPath = ActorNames.GetHandActor(entity);
				card.UpdateActor(forceIfNullZone: false, assetPath);
			}
			string boneName = ((tag != GAME_TAG.TAG_SCRIPT_DATA_ENT_1) ? (sourceEntity.IsControlledByFriendlySidePlayer() ? m_OpponentBoneName : m_FriendlyBoneName) : (sourceEntity.IsControlledByFriendlySidePlayer() ? m_FriendlyBoneName : m_OpponentBoneName));
			if ((bool)UniversalInputManager.UsePhoneUI)
			{
				boneName += "_phone";
			}
			Transform obj = Board.Get().FindBone(boneName);
			Vector3 localScale = obj.localScale;
			Vector3 position = obj.position;
			Quaternion rotation = obj.rotation;
			Action<object> tweensCompleteCallback = delegate
			{
				m_numOldActorsInMoving--;
			};
			iTween.MoveTo(card.gameObject, iTween.Hash("position", position, "time", m_MoveOldCardTime, "easetype", iTween.EaseType.easeInOutQuart, "oncomplete", tweensCompleteCallback));
			iTween.RotateTo(card.gameObject, iTween.Hash("rotation", rotation.eulerAngles, "time", m_MoveOldCardTime, "easetype", iTween.EaseType.easeInOutCubic));
			iTween.ScaleTo(card.gameObject, iTween.Hash("scale", localScale, "time", m_MoveOldCardTime, "easetype", iTween.EaseType.easeInOutQuint));
		}
	}

	private IEnumerator PlayTransformFX()
	{
		ActivateTransformSpell(GAME_TAG.TAG_SCRIPT_DATA_ENT_1);
		ActivateTransformSpell(GAME_TAG.TAG_SCRIPT_DATA_ENT_2);
		foreach (Spell spell in m_activeSpells)
		{
			while (!spell.IsFinished())
			{
				yield return null;
			}
		}
	}

	private void ActivateTransformSpell(GAME_TAG tag)
	{
		Entity sourceEntity = GetSourceCard().GetEntity();
		if (sourceEntity.HasTag(tag))
		{
			Spell spell = SpellManager.Get().GetSpell(m_TransformSpell);
			Entity entity = GameState.Get().GetEntity(sourceEntity.GetTag(tag));
			spell.SetSource(entity.GetCard().gameObject);
			spell.AddStateFinishedCallback(OnSpellStateFinished);
			spell.ActivateState(SpellStateType.ACTION);
			m_activeSpells.Add(spell);
		}
	}

	private void OnSpellStateFinished(Spell spell, SpellStateType prevStateType, object userData)
	{
		if (spell.GetActiveState() == SpellStateType.NONE)
		{
			m_activeSpells.Remove(spell);
			UnityEngine.Object.Destroy(spell);
		}
	}

	private IEnumerator ShowNewCards()
	{
		ShowNewCard(Player.Side.FRIENDLY);
		ShowNewCard(Player.Side.OPPOSING);
		yield return new WaitForSeconds(m_ShowNewCardTime);
	}

	private void ShowNewCard(Player.Side side)
	{
		Actor newActor = m_newActors[(int)(side - 1)];
		if (!(newActor == null))
		{
			Entity sourceEntity = GetSourceCard().GetEntity();
			GAME_TAG scriptEntTag = ((!sourceEntity.IsControlledByFriendlySidePlayer()) ? ((side == Player.Side.FRIENDLY) ? GAME_TAG.TAG_SCRIPT_DATA_ENT_2 : GAME_TAG.TAG_SCRIPT_DATA_ENT_1) : ((side == Player.Side.FRIENDLY) ? GAME_TAG.TAG_SCRIPT_DATA_ENT_1 : GAME_TAG.TAG_SCRIPT_DATA_ENT_2));
			Entity entity = GameState.Get().GetEntity(sourceEntity.GetTag(scriptEntTag));
			TransformUtil.CopyWorld(newActor.gameObject, entity.GetCard().gameObject);
			entity.GetCard().TransitionToZone(null);
			newActor.Show();
		}
	}

	private IEnumerator SwitchToRealCards()
	{
		foreach (int entityID in m_newEntityIDs)
		{
			Entity entity = GameState.Get().GetEntity(entityID);
			Card card = entity.GetCard();
			card.SetDoNotSort(on: true);
			card.SetDoNotWarpToNewZone(on: true);
			card.TransitionToZone(entity.GetController().GetHandZone());
			int actorIndex = ((!entity.IsControlledByFriendlySidePlayer()) ? 1 : 0);
			Actor actor = m_newActors[actorIndex];
			while (card.IsActorLoading())
			{
				yield return null;
			}
			actor.Hide();
			TransformUtil.CopyWorld(card.gameObject, actor.gameObject);
			card.SetDoNotSort(on: false);
			card.SetDoNotWarpToNewZone(on: false);
		}
		OnSpellFinished();
	}

	private IEnumerator WaitAndDeactivate()
	{
		while (m_activeSpells.Count > 0)
		{
			yield return null;
		}
		Actor[] newActors = m_newActors;
		for (int i = 0; i < newActors.Length; i++)
		{
			UnityEngine.Object.Destroy(newActors[i]);
		}
		if (m_newEntityIDs != null)
		{
			m_newEntityIDs.Clear();
		}
		Deactivate();
	}
}
