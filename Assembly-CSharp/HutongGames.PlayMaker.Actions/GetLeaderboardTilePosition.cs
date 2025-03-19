namespace HutongGames.PlayMaker.Actions;

[ActionCategory("Pegasus")]
[Tooltip("Stores the position of a player's leaderboard tile.")]
public class GetLeaderboardTilePosition : FsmStateAction
{
	[RequiredField]
	[UIHint(UIHint.Variable)]
	[Tooltip("Which player's tile are we looking for?")]
	public FsmInt m_PlayerId;

	[UIHint(UIHint.Variable)]
	[Tooltip("Output variable.")]
	[RequiredField]
	public FsmVector3 m_TilePosition;

	public override void Reset()
	{
		m_PlayerId = 1;
		m_TilePosition = null;
	}

	public override void OnEnter()
	{
		base.OnEnter();
		if (m_TilePosition == null)
		{
			global::Log.Gameplay.PrintError("{0}.OnEnter() - No variable hooked up to store tile position!", this);
			Finish();
			return;
		}
		if (GameState.Get() == null)
		{
			global::Log.Gameplay.PrintError("{0}.OnEnter() - GameState is null!", this);
			Finish();
			return;
		}
		if (PlayerLeaderboardManager.Get() == null)
		{
			global::Log.Gameplay.PrintError("{0}.OnEnter() - PlayerLeaderboardManager is null!", this);
			Finish();
			return;
		}
		PlayerLeaderboardCard playerCard = PlayerLeaderboardManager.Get().GetTileForPlayerId(m_PlayerId.Value);
		if (playerCard == null)
		{
			global::Log.Gameplay.PrintError("{0}.OnEnter() - No player card exists for player id {1}!", this, m_PlayerId.Value);
			Finish();
		}
		else if (playerCard.m_tileActor == null)
		{
			global::Log.Gameplay.PrintError("{0}.OnEnter() - No player tile exists for player id {1}!", this, m_PlayerId.Value);
			Finish();
		}
		else
		{
			m_TilePosition.Value = playerCard.m_tileActor.transform.position;
			Finish();
		}
	}
}
