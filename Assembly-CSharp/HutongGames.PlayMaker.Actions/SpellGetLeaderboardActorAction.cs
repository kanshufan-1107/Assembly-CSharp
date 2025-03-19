using UnityEngine;

namespace HutongGames.PlayMaker.Actions;

[Tooltip("Put a Spell's Source or Target Actor (that is on the leaderboard) into a GameObject variable.")]
[ActionCategory("Pegasus")]
public class SpellGetLeaderboardActorAction : SpellAction
{
	public FsmOwnerDefault m_SpellObject;

	public Which m_WhichActor;

	public FsmGameObject m_GameObject;

	protected override GameObject GetSpellOwner()
	{
		return base.Fsm.GetOwnerDefaultTarget(m_SpellObject);
	}

	public override void Reset()
	{
		m_SpellObject = null;
		m_WhichActor = Which.SOURCE;
		m_GameObject = null;
	}

	public override void OnEnter()
	{
		base.OnEnter();
		Entity entity = GetEntity(m_WhichActor);
		PlayerLeaderboardManager playerLeaderboardManager = PlayerLeaderboardManager.Get();
		if (playerLeaderboardManager == null)
		{
			Error.AddDevFatal("SpellGetLeaderboardActorAction.OnEnter() - No PlayerLeaderboardManager exists!");
			Finish();
			return;
		}
		PlayerLeaderboardCard playerTile = playerLeaderboardManager.GetTileForPlayerId(entity.GetTag(GAME_TAG.PLAYER_ID));
		if (playerTile == null)
		{
			Error.AddDevFatal("SpellGetLeaderboardActorAction.OnEnter() - PlayerLeaderboardCard not found!");
			Finish();
			return;
		}
		Actor actor = playerTile.m_tileActor;
		if (actor == null)
		{
			Error.AddDevFatal("SpellGetLeaderboardActorAction.OnEnter() - Actor not found!");
			Finish();
			return;
		}
		if (!m_GameObject.IsNone)
		{
			m_GameObject.Value = actor.gameObject;
		}
		Finish();
	}
}
