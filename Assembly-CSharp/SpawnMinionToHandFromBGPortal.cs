using UnityEngine;

public class SpawnMinionToHandFromBGPortal : SpawnToHandSpell
{
	protected override Vector3 GetOriginForTarget(int targetIndex = 0)
	{
		if (TeammateBoardViewer.Get() == null)
		{
			if (GameState.Get() == null)
			{
				return Vector3.zero;
			}
			GameEntity gameEntity = GameState.Get().GetGameEntity();
			if (gameEntity == null || !(gameEntity is TB_BaconShop))
			{
				return Vector3.zero;
			}
			return ((TB_BaconShop)gameEntity).GetPortalPosition();
		}
		return TeammateBoardViewer.Get().GetPortalOriginalPosition();
	}

	protected override void OnDeath(SpellStateType prevStateType)
	{
		if (m_targets.Count >= 1)
		{
			(GameState.Get().GetGameEntity() as TB_BaconShop).OnCardRecievedFromTeammate(m_targets[0].GetComponent<Card>());
		}
		base.OnDeath(prevStateType);
	}
}
