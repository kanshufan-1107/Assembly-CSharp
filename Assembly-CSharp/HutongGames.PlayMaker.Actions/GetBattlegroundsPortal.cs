namespace HutongGames.PlayMaker.Actions;

[ActionCategory("Pegasus")]
[Tooltip("Get the game object of the Battlegrounds Portal")]
public class GetBattlegroundsPortal : FsmStateAction
{
	[RequiredField]
	[UIHint(UIHint.Variable)]
	[Tooltip("Output variable.")]
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
		}
		else if (GameState.Get() == null)
		{
			global::Log.Gameplay.PrintError("{0}.OnEnter() - GameState is null!", this);
			Finish();
		}
		else if (TeammateBoardViewer.Get() == null)
		{
			global::Log.Gameplay.PrintError("{0}.OnEnter() - TeammateBoardViewer is null!", this);
			Finish();
		}
		else
		{
			m_Portal.Value = TeammateBoardViewer.Get().GetPortal();
			Finish();
		}
	}
}
