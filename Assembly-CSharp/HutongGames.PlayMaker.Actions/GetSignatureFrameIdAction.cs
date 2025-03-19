using Blizzard.T5.Core.Utils;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions;

[Tooltip("Store the requested signature frameId in an FSM int variable for later use.")]
[ActionCategory("Pegasus")]
public class GetSignatureFrameIdAction : FsmStateAction
{
	[Title("FrameIdInt")]
	[Tooltip("Store the requested frameId in specified int variable.")]
	[RequiredField]
	[UIHint(UIHint.Variable)]
	public FsmInt targetFrameId;

	public override void Reset()
	{
	}

	public override void OnEnter()
	{
		Actor actorInst = GameObjectUtils.FindComponentInThisOrParents<Actor>(base.Owner);
		if (actorInst == null)
		{
			Debug.LogWarningFormat("Failed to find Actor component in GetSignatureFrameIdAction: " + base.Owner?.name);
			Finish();
			return;
		}
		string cardId = "";
		Entity cardEntity = actorInst.GetEntity();
		if (cardEntity != null)
		{
			cardId = cardEntity.GetCardId();
		}
		Card card = actorInst.GetCard();
		if (cardId == "" && card != null)
		{
			Entity cardEntity2 = card.GetEntity();
			if (cardEntity2 != null)
			{
				cardId = cardEntity2.GetCardId();
			}
		}
		if (cardId == "")
		{
			EntityDef entityDef = actorInst.GetEntityDef();
			if (entityDef != null)
			{
				cardId = entityDef.GetCardId();
			}
		}
		if (cardId == "")
		{
			Debug.LogErrorFormat("Failed to get entity in GetSignatureFrameIdAction: " + base.Owner?.name);
			Finish();
		}
		else
		{
			targetFrameId.Value = ActorNames.GetSignatureFrameId(cardId);
			Finish();
		}
	}
}
