using Blizzard.T5.Core.Utils;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions;

[Tooltip("Create a dummy hand actor from a database ID.")]
[ActionCategory("Pegasus")]
public class CreateHandActorFromDatabaseIDAction : FsmStateAction
{
	[RequiredField]
	[Tooltip("GameObject(Spell) to retrieve a target info.")]
	public FsmOwnerDefault m_OwnerObject;

	[Tooltip("Database ID of the card definition to load. 0 = cardback")]
	[RequiredField]
	public FsmInt m_DatabaseID;

	[Tooltip("Variable to store the newly created dummy actor.")]
	[UIHint(UIHint.Variable)]
	[RequiredField]
	public FsmGameObject m_DummyActor;

	[Tooltip("Match the premium of the owner. Set to false to use the next field instead.")]
	public bool m_UseOwnerPremium;

	[Tooltip("Premium state for the card.")]
	public FsmInt m_PremiumType;

	[Tooltip("Show as an in-play actor or not.")]
	public bool m_ShowPlayActor;

	[Tooltip("Whether it appears or not.")]
	public bool m_Show;

	[Tooltip("FSM event to fire after actor finishes loading")]
	public FsmEvent m_FinishEvent;

	private TAG_PREMIUM m_premiumValue;

	public override void Reset()
	{
		m_UseOwnerPremium = true;
		m_DatabaseID = 0;
		m_DummyActor = null;
		m_premiumValue = TAG_PREMIUM.NORMAL;
	}

	public override void OnEnter()
	{
		AssetReference assetRef = null;
		if (m_DatabaseID.Value == 0)
		{
			assetRef = "Card_Hidden.prefab:1a94649d257bc284ca6e2962f634a8b9";
		}
		else
		{
			EntityDef entityDef = DefLoader.Get().GetEntityDef(m_DatabaseID.Value);
			TAG_PREMIUM entityPremium = (TAG_PREMIUM)m_PremiumType.Value;
			if (m_UseOwnerPremium)
			{
				entityPremium = GetOwnerPremium();
			}
			m_premiumValue = entityPremium;
			assetRef = ((!m_ShowPlayActor) ? ((AssetReference)ActorNames.GetHandActor(entityDef, entityPremium)) : ((AssetReference)ActorNames.GetPlayActor(entityDef, entityPremium)));
		}
		AssetLoader.Get().InstantiatePrefab(assetRef, OnActorLoaded, null, AssetLoadingOptions.IgnorePrefabPosition);
	}

	private void OnActorLoaded(AssetReference assetRef, GameObject go, object callbackData)
	{
		Actor actor = go.GetComponent<Actor>();
		if (m_DatabaseID.Value != 0)
		{
			using DefLoader.DisposableFullDef entityfullDef = DefLoader.Get().GetFullDef(m_DatabaseID.Value);
			actor.SetFullDef(entityfullDef);
		}
		actor.SetPremium(m_premiumValue);
		actor.SetCardBackSideOverride(GetOwnerControllerSide());
		actor.UpdateAllComponents();
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

	private Player.Side GetOwnerControllerSide()
	{
		return GetOwnerEntity()?.GetControllerSide() ?? Player.Side.FRIENDLY;
	}

	private TAG_PREMIUM GetOwnerPremium()
	{
		return (TAG_PREMIUM)(((int?)GetOwnerEntity()?.GetPremiumType()) ?? m_PremiumType.Value);
	}

	private Entity GetOwnerEntity()
	{
		GameObject owner = base.Fsm.GetOwnerDefaultTarget(m_OwnerObject);
		if (owner == null)
		{
			return null;
		}
		Spell spell = GameObjectUtils.FindComponentInThisOrParents<Spell>(owner);
		if (spell == null)
		{
			return null;
		}
		Card card = spell.GetTargetCard();
		if (card == null)
		{
			return null;
		}
		return card.GetEntity();
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
