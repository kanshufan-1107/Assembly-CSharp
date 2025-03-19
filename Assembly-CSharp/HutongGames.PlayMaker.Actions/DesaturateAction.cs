namespace HutongGames.PlayMaker.Actions;

[Tooltip("Trigger desaturate effects action.")]
[ActionCategory("Pegasus")]
public class DesaturateAction : FsmStateAction
{
	public enum ActionType
	{
		Start,
		End
	}

	[Tooltip("Blur or Vignette time that it takes to get to full value")]
	public FsmFloat m_desaturateTime;

	public FsmFloat m_desaturateAmount = 0.8f;

	public ActionType m_actionType;

	private ScreenEffectsHandleComponent m_handleComponent;

	public override void Awake()
	{
		base.Awake();
		if (!base.Fsm.GameObject.TryGetComponent<ScreenEffectsHandleComponent>(out var handle))
		{
			m_handleComponent = base.Fsm.GameObject.AddComponent<ScreenEffectsHandleComponent>();
		}
		else
		{
			m_handleComponent = handle;
		}
	}

	public override void OnEnter()
	{
		ScreenEffectParameters screenEffectParameters = ScreenEffectParameters.DesaturatePerspective;
		screenEffectParameters.Desaturate = new DesaturateParameters(m_desaturateAmount.Value);
		screenEffectParameters.Time = m_desaturateTime.Value;
		switch (m_actionType)
		{
		case ActionType.Start:
			m_handleComponent.StartEffect(screenEffectParameters);
			break;
		case ActionType.End:
			screenEffectParameters.EaseType = iTween.EaseType.easeInOutCubic;
			m_handleComponent.StopEffect(screenEffectParameters);
			break;
		}
		Finish();
	}
}
