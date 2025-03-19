using UnityEngine;

public class SpawnMinionsToHandFromRitualSpell : SpawnToHandSpell
{
	public string m_friendlyInvokeBoneName = "FriendlyRitual";

	public string m_opponentInvokeBoneName = "OpponentRitual";

	protected override Vector3 GetOriginForTarget(int targetIndex = 0)
	{
		Entity sourceEntity = GetSourceCard().GetEntity();
		Player controller = sourceEntity.GetController();
		if (controller.GetTag(GAME_TAG.MAIN_GALAKROND) == controller.GetHero().GetEntityId())
		{
			return base.GetOriginForTarget(targetIndex);
		}
		string invokeBoneName = ((sourceEntity.GetControllerSide() == Player.Side.FRIENDLY) ? m_friendlyInvokeBoneName : m_opponentInvokeBoneName);
		return Board.Get().FindBone(invokeBoneName).position;
	}
}
