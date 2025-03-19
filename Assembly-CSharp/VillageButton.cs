using Hearthstone.UI;

public class VillageButton : PegUIElement
{
	private PlayMakerFSM m_fsm;

	private bool m_isStarted;

	private VisualController m_visualController;

	private const string PLAY_BUTTON_ENABLED_STATE = "ENABLED";

	private const string PLAY_BUTTON_DISABLED_STATE = "DISABLED";

	private const string PLAY_BUTTON_PRESSED_STATE = "PRESSED";

	private const string PLAY_BUTTON_RELEASED_STATE = "RELEASED";

	protected override void Awake()
	{
		base.Awake();
		SetOriginalLocalPosition();
	}

	protected void Start()
	{
		m_isStarted = true;
		m_fsm = GetComponent<PlayMakerFSM>();
		m_visualController = GetComponent<VisualController>();
		if (IsEnabled())
		{
			Enable();
		}
		else
		{
			Disable();
		}
	}

	protected override void OnOut(InteractionState oldState)
	{
		if (m_visualController != null)
		{
			m_visualController.SetState("RELEASED");
		}
	}

	public void Disable(bool keepLabelTextVisible = false)
	{
		SetEnabled(enabled: false);
		if (m_isStarted)
		{
			if (m_fsm != null && !keepLabelTextVisible)
			{
				m_fsm.SendEvent("Cancel");
			}
			if (m_visualController != null)
			{
				m_visualController.SetState("DISABLED");
			}
		}
	}

	public void Enable()
	{
		SetEnabled(enabled: true);
		if (m_isStarted)
		{
			if (m_fsm != null)
			{
				m_fsm.SendEvent("Birth");
			}
			if (m_visualController != null)
			{
				m_visualController.SetState("ENABLED");
			}
		}
	}

	protected override void OnPress()
	{
		if (m_visualController != null)
		{
			m_visualController.SetState("PRESSED");
		}
	}

	protected override void OnRelease()
	{
		if (m_visualController != null)
		{
			m_visualController.SetState("RELEASED");
		}
	}
}
