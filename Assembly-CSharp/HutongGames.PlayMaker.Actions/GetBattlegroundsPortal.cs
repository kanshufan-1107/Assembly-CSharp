namespace HutongGames.PlayMaker.Actions;

[ActionCategory("Pegasus")]
[Tooltip("Get the game object of the Battlegrounds Portal")]
public class GetBattlegroundsPortal : FsmStateAction
{
	[UIHint(UIHint.Variable)]
	[Tooltip("Output variable.")]
	[RequiredField]
	public FsmGameObject m_Portal;

	public override void Reset()
	{
		m_Portal = null;
	}

	public override void OnEnter()
	{
		base.OnEnter();
		if (m_Portal == null)
		{
			global::Log.Gameplay.PrintError("{0}.OnEnter() - No variable hooked up to store portal position!", this);
			Finish();
			return;
		}
		if (GameState.Get() == null)
		{
			global::Log.Gameplay.PrintError("{0}.OnEnter() - GameState is null!", this);
			Finish();
			return;
		}
		GameEntity gameEntity = GameState.Get().GetGameEntity();
		if (gameEntity == null || !(gameEntity is TB_BaconShop))
		{
			global::Log.Gameplay.PrintError("{0}.OnEnter() - game entity is null or not BG!", this);
			Finish();
		}
		else
		{
			TB_BaconShop baconGameEntity = (TB_BaconShop)gameEntity;
			m_Portal.Value = baconGameEntity.GetPortal();
			Finish();
		}
	}
}
