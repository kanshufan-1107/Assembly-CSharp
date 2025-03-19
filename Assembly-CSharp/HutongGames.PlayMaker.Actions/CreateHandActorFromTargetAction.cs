using System.Collections.Generic;
using Blizzard.T5.Core.Utils;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions;

[Tooltip("Create a dummy hand actor from a spell target.")]
[ActionCategory("Pegasus")]
public class CreateHandActorFromTargetAction : FsmStateAction
{
	[RequiredField]
	[Tooltip("GameObject(Spell) to retrieve a target info.")]
	public FsmOwnerDefault m_OwnerObject;

	[UIHint(UIHint.Variable)]
	[RequiredField]
	[Tooltip("Variable to store the newly created dummy actor.")]
	public FsmGameObject m_DummyActor;

	[Tooltip("Bone name for setting its initial transform. (Friendly side)")]
	public string m_FriendlyBoneName = "FriendlyJoust";

	[Tooltip("Bone name for setting its initial transform. (Opponent Side)")]
	public string m_OpponentBoneName = "OpponentJoust";

	[Tooltip("Whether it appears or not.")]
	public bool m_Show;

	[Tooltip("FSM event to fire after actor finishes loading")]
	public FsmEvent m_FinishEvent;

	public override void Reset()
	{
		m_OwnerObject = null;
		m_DummyActor = null;
	}

	public override void OnEnter()
	{
		GameObject owner = base.Fsm.GetOwnerDefaultTarget(m_OwnerObject);
		if (owner == null)
		{
			Error.AddDevWarning("CreateHandActorFromTargetAction", "Null owner");
			FinishAction();
			return;
		}
		Spell spell = GameObjectUtils.FindComponentInThisOrParents<Spell>(owner);
		if (spell == null)
		{
			Error.AddDevWarning("CreateHandActorFromTargetAction", "Invalid owner spell. owner:{0}", owner);
			FinishAction();
			return;
		}
		Card card = spell.GetTargetCard();
		if (card == null)
		{
			FinishAction();
			return;
		}
		Entity entity = card.GetEntity();
		if (entity == null)
		{
			Error.AddDevWarning("CreateHandActorFromTargetAction", "Invalid target entity. card:{0}, spell:{1}", card, spell);
			FinishAction();
		}
		else if (!entity.IsRevealed())
		{
			ParseShowEntityAndLoadDummyActor(spell, entity);
		}
		else
		{
			LoadDummyActor(entity);
		}
	}

	private void ParseShowEntityAndLoadDummyActor(Spell spell, Entity entity)
	{
		PowerTaskList taskList = spell.GetPowerTaskList();
		if (taskList == null)
		{
			Error.AddDevWarning("CreateHandActorFromTargetAction", "Invalid PowerTaskList. entity:{0}, spell:{1}", entity, spell);
			FinishAction();
			return;
		}
		Entity dummyEntity = new Entity();
		dummyEntity.SetCardId(entity.GetCardId());
		foreach (KeyValuePair<int, int> tagPair in entity.GetTags().GetMap())
		{
			dummyEntity.SetTag(tagPair.Key, tagPair.Value);
		}
		foreach (PowerTask task in taskList.GetTaskList())
		{
			Network.PowerHistory power = task.GetPower();
			if (power.Type != Network.PowerType.SHOW_ENTITY)
			{
				continue;
			}
			Network.Entity netEnt = (power as Network.HistShowEntity).Entity;
			if (netEnt.ID != entity.GetEntityId())
			{
				continue;
			}
			dummyEntity.LoadCard(netEnt.CardID);
			foreach (Network.Entity.Tag netTag in netEnt.Tags)
			{
				dummyEntity.SetTag(netTag.Name, netTag.Value);
			}
			break;
		}
		LoadDummyActor(dummyEntity);
	}

	private void LoadDummyActor(Entity entity)
	{
		AssetLoader.Get().InstantiatePrefab(ActorNames.GetHandActor(entity.GetEntityDef(), entity.GetPremiumType()), OnActorLoaded, entity, AssetLoadingOptions.IgnorePrefabPosition);
	}

	private void OnActorLoaded(AssetReference assetRef, GameObject go, object callbackData)
	{
		Actor actor = go.GetComponent<Actor>();
		Entity entity = (Entity)callbackData;
		actor.SetEntity(entity);
		actor.SetCardDefFromEntity(entity);
		actor.SetCardBackSideOverride(entity.GetControllerSide());
		actor.UpdateAllComponents();
		Transform transform = GetIntialTransform(entity.GetControllerSide());
		if (transform != null)
		{
			TransformUtil.CopyWorld(actor, transform);
		}
		else
		{
			TransformUtil.Identity(actor);
		}
		if (m_Show)
		{
			actor.Show();
		}
		else
		{
			actor.Hide();
		}
		m_DummyActor.Value = actor.gameObject;
		FinishAction();
	}

	private Transform GetIntialTransform(Player.Side playerSide)
	{
		string boneName = ((playerSide == Player.Side.FRIENDLY) ? m_FriendlyBoneName : m_OpponentBoneName);
		if (string.IsNullOrEmpty(boneName))
		{
			return null;
		}
		if ((bool)UniversalInputManager.UsePhoneUI)
		{
			boneName += "_phone";
		}
		return Board.Get().FindBone(boneName);
	}

	private void FinishAction()
	{
		if (m_FinishEvent != null)
		{
			base.Fsm.Event(m_FinishEvent);
		}
		Finish();
	}
}
