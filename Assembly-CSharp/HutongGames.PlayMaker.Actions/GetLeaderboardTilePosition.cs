namespace HutongGames.PlayMaker.Actions;

[Tooltip("Stores the position of a player's leaderboard tile.")]
[ActionCategory("Pegasus")]
public class GetLeaderboardTilePosition : FsmStateAction
{
	[RequiredField]
	[Tooltip("Which player's tile are we looking for?")]
	[UIHint(UIHint.Variable)]
	public FsmInt m_PlayerId;

	[Tooltip("Output variable.")]
	[UIHint(UIHint.Variable)]
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
