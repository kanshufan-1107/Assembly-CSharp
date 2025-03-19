namespace HutongGames.PlayMaker.Actions;

[ActionCategory("Pegasus")]
[Tooltip("Trigger blend to color effects action.")]
public class BlendToColorAction : FsmStateAction
{
	public enum ActionType
	{
		Start,
		End
	}

	[Tooltip("Blur or Vignette time that it takes to get to full value")]
	public FsmFloat m_blendTime;

	public FsmFloat m_finalBlendAmount = 0.8f;

	public FsmColor m_blendColor;

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
		ScreenEffectParameters screenEffectParameters = ScreenEffectParameters.BlendToColorPerspective;
		screenEffectParameters.BlendToColor = new BlendToColorParameters(m_blendColor.Value, m_finalBlendAmount.Value);
		screenEffectParameters.Time = m_blendTime.Value;
		switch (m_actionType)
		{
		case ActionType.Start:
			m_handleComponent.StartEffect(screenEffectParameters);
			break;
		case ActionType.End:
			m_handleComponent.StopEffect(screenEffectParameters);
			break;
		}
		Finish();
	}
}
