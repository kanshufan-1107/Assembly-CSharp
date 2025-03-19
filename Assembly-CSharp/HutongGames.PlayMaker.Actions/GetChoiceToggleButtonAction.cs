namespace HutongGames.PlayMaker.Actions;

[Tooltip("Get the toggle button game object when a choice is happening")]
[ActionCategory("Pegasus")]
public class GetChoiceToggleButtonAction : FsmStateAction
{
	public FsmGameObject m_toggleButtonGameObject;

	public override void Reset()
	{
		m_toggleButtonGameObject = null;
	}

	public override void OnEnter()
	{
		base.OnEnter();
		NormalButton button = ChoiceCardMgr.Get()?.GetToggleButton();
		if (button == null)
		{
			global::Log.PlayMaker.PrintError("{0}.OnEnter(): button object is null", this);
			Finish();
			return;
		}
		if (!m_toggleButtonGameObject.IsNone)
		{
			m_toggleButtonGameObject.Value = button.gameObject;
		}
		Finish();
	}
}
