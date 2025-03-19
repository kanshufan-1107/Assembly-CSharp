using UnityEngine;

namespace HutongGames.PlayMaker.Actions;

[Tooltip("Set an actor to the appropriate ritual fx position")]
[ActionCategory("Pegasus")]
public class PositionRitualActorAction : FsmStateAction
{
	public const string FRIENDLY_BONE_NAME = "FriendlyRitual";

	public const string OPPONENT_BONE_NAME = "OpponentRitual";

	[RequiredField]
	[Tooltip("GameObject to retrieve controller side.")]
	public FsmInt m_OwnerEntityId;

	[Tooltip("GameObject to reposition")]
	[RequiredField]
	public FsmGameObject m_actorObject;

	[Tooltip("Progress text for DMF C'thun")]
	public UberText m_progressText;

	[Tooltip("Show progress text")]
	public bool m_showDMFCount;

	public override void Reset()
	{
		m_actorObject = null;
	}

	public override void OnEnter()
	{
		string ritualBoneName = ((GetOwnerControllerSide() == Player.Side.FRIENDLY) ? "FriendlyRitual" : "OpponentRitual");
		Transform ritualBone = Board.Get().FindBone(ritualBoneName);
		m_actorObject.Value.transform.parent = ritualBone;
		m_actorObject.Value.transform.localPosition = Vector3.zero;
		if (m_showDMFCount)
		{
			m_progressText.Text = $"{GetNumCthunPiecesPlayed()}/4";
			m_progressText.gameObject.SetActive(value: true);
		}
		Finish();
	}

	private Player.Side GetOwnerControllerSide()
	{
		return GetOwnerEntity()?.GetControllerSide() ?? Player.Side.FRIENDLY;
	}

	private Entity GetOwnerEntity()
	{
		return GameState.Get().GetEntity(m_OwnerEntityId.Value);
	}

	protected int GetNumCthunPiecesPlayed()
	{
		Entity controller = GetOwnerEntity();
		if (controller == null)
		{
			return 0;
		}
		int sum = 0;
		if (controller.GetTag(GAME_TAG.PLAYED_CTHUN_EYE) != 0)
		{
			sum++;
		}
		if (controller.GetTag(GAME_TAG.PLAYED_CTHUN_BODY) != 0)
		{
			sum++;
		}
		if (controller.GetTag(GAME_TAG.PLAYED_CTHUN_MAW) != 0)
		{
			sum++;
		}
		if (controller.GetTag(GAME_TAG.PLAYED_CTHUN_HEART) != 0)
		{
			sum++;
		}
		return sum;
	}
}
