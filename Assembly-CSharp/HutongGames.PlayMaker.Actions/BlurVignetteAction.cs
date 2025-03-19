namespace HutongGames.PlayMaker.Actions;

[Tooltip("Trigger blur+vignette effects action.")]
[ActionCategory("Pegasus")]
public class BlurVignetteAction : FsmStateAction
{
	public enum ActionType
	{
		Start,
		End
	}

	[Tooltip("Blur or Vignette time that it takes to get to full value")]
	public FsmFloat m_blurTime;

	public FsmFloat m_vignetteVal = 0.8f;

	public FsmBool m_isBlurred;

	public ActionType m_actionType;

	[Tooltip("If multiple FSMs are going to turn on and off a blur effect, create one ScreenEffectsHandleComponent and add it here.")]
	public FsmGameObject m_handleComponentOverride;

	private ScreenEffectsHandleComponent m_handleComponent;

	public override void Awake()
	{
		base.Awake();
		if (m_handleComponentOverride != null && m_handleComponentOverride.Value != null && m_handleComponentOverride.Value.TryGetComponent<ScreenEffectsHandleComponent>(out var handle))
		{
			m_handleComponent = handle;
		}
		if (m_handleComponent == null)
		{
			if (!base.Fsm.GameObject.TryGetComponent<ScreenEffectsHandleComponent>(out var handle2))
			{
				m_handleComponent = base.Fsm.GameObject.AddComponent<ScreenEffectsHandleComponent>();
			}
			else
			{
				m_handleComponent = handle2;
			}
		}
	}

	public override void OnEnter()
	{
		if (m_isBlurred.Value)
		{
			ScreenEffectParameters screenEffectParameters = ScreenEffectParameters.BlurVignettePerspective;
			screenEffectParameters.Time = m_blurTime.Value;
			switch (m_actionType)
			{
			case ActionType.Start:
				m_handleComponent.StartEffect(screenEffectParameters);
				break;
			case ActionType.End:
				m_handleComponent.StopEffect(screenEffectParameters);
				break;
			}
		}
		else
		{
			ScreenEffectParameters screenEffectParameters2 = ScreenEffectParameters.VignettePerspective;
			screenEffectParameters2.Time = m_blurTime.Value;
			screenEffectParameters2.Vignette = new VignetteParameters(m_vignetteVal.Value);
			switch (m_actionType)
			{
			case ActionType.Start:
				m_handleComponent.StartEffect(screenEffectParameters2);
				break;
			case ActionType.End:
				m_handleComponent.StopEffect(screenEffectParameters2);
				break;
			}
		}
		Finish();
	}
}
