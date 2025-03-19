using System.Collections;
using UnityEngine;

public class StoreButton : PegUIElement
{
	public enum State
	{
		SHOWN,
		HIDDEN
	}

	public GameObject m_storeClosed;

	public UberText m_storeClosedText;

	public UberText m_storeText;

	public HighlightState m_highlightState;

	public GameObject m_highlight;

	private State m_state;

	private Coroutine m_storeStatusPollCoroutine;

	private bool m_isLoaded;

	private readonly AssetReference m_storeButtonMouseOver = new AssetReference("store_button_mouse_over.prefab:11c9392d3449f064cb60420a61398732");

	private readonly AssetReference m_storeWindowShrink = new AssetReference("Store_window_shrink.prefab:b68247126e211224e8a904142d2a9895");

	private readonly AssetReference m_smallClick = new AssetReference("Small_Click.prefab:2a1c5335bf08dc84eb6e04fc58160681");

	private readonly AssetReference m_storeClosedButtonClick = new AssetReference("Store_closed_button_click.prefab:a6b74848e2c7e5748a20524b40fe6c1e");

	protected override void Awake()
	{
		base.Awake();
		m_storeText.Text = GameStrings.Get("GLUE_STORE_OPEN_BUTTON_TEXT");
		m_storeClosedText.Text = GameStrings.Get("GLUE_STORE_CLOSED_BUTTON_TEXT");
		if (SoundManager.Get() != null)
		{
			SoundManager.Get().Load(m_storeButtonMouseOver);
			SoundManager.Get().Load(m_storeWindowShrink);
		}
	}

	private void OnEnable()
	{
		Load();
	}

	private void OnDisable()
	{
		Unload();
	}

	public void Load()
	{
		if (!m_isLoaded && base.gameObject.activeInHierarchy)
		{
			SetEnabled(enabled: true);
			m_storeClosed.SetActive(!StoreManager.Get().IsOpen());
			UnregisterEventListeners();
			RegisterEventListeners();
			if (m_storeStatusPollCoroutine != null)
			{
				StopCoroutine(m_storeStatusPollCoroutine);
			}
			m_storeStatusPollCoroutine = StartCoroutine(PollShopStatusForTelemetry());
			m_isLoaded = true;
		}
	}

	public void Unload()
	{
		m_isLoaded = false;
		SetEnabled(enabled: false);
		UnregisterEventListeners();
		if (m_storeStatusPollCoroutine != null)
		{
			StopCoroutine(m_storeStatusPollCoroutine);
			m_storeStatusPollCoroutine = null;
		}
	}

	public bool IsVisualClosed()
	{
		if (m_storeClosed != null)
		{
			return m_storeClosed.activeInHierarchy;
		}
		return false;
	}

	public State GetState()
	{
		return m_state;
	}

	public void UpdateState(State state)
	{
		m_state = state;
		switch (state)
		{
		case State.SHOWN:
			base.gameObject.SetActive(value: true);
			break;
		case State.HIDDEN:
			base.gameObject.SetActive(value: false);
			break;
		}
	}

	private void OnButtonOver(UIEvent e)
	{
		BoxRailroadManager railroadManager = Box.Get().GetRailroadManager();
		if (!(railroadManager != null) || !railroadManager.ShouldDisableButtonType(Box.ButtonType.STORE))
		{
			if (!IsVisualClosed())
			{
				SoundManager.Get().LoadAndPlay(m_storeButtonMouseOver, base.gameObject);
			}
			if (m_highlightState != null)
			{
				m_highlightState.ChangeState(ActorStateType.HIGHLIGHT_MOUSE_OVER);
			}
			if (m_highlight != null)
			{
				m_highlight.SetActive(value: true);
			}
			TooltipZone tooltipZone = GetComponent<TooltipZone>();
			if (tooltipZone != null)
			{
				tooltipZone.ShowBoxTooltip(GameStrings.Get("GLUE_TOOLTIP_BUTTON_STORE_HEADLINE"), GameStrings.Get("GLUE_TOOLTIP_BUTTON_STORE_DESC"));
			}
		}
	}

	private void OnButtonOut(UIEvent e)
	{
		if (m_highlightState != null)
		{
			m_highlightState.ChangeState(ActorStateType.HIGHLIGHT_OFF);
		}
		if (m_highlight != null)
		{
			m_highlight.SetActive(value: false);
		}
		TooltipZone tooltipZone = GetComponent<TooltipZone>();
		if (tooltipZone != null)
		{
			tooltipZone.HideTooltip();
		}
	}

	private void OnButtonRelease(UIEvent e)
	{
		if (!IsVisualClosed())
		{
			SoundManager.Get().LoadAndPlay(m_smallClick, base.gameObject);
		}
		else
		{
			SoundManager.Get().LoadAndPlay(m_storeClosedButtonClick, base.gameObject);
		}
	}

	private void OnStoreStatusChanged(bool isOpen)
	{
		if (m_storeClosed != null)
		{
			m_storeClosed.SetActive(!isOpen);
		}
	}

	private IEnumerator PollShopStatusForTelemetry()
	{
		while (SceneMgr.Get().GetMode() != SceneMgr.Mode.HUB)
		{
			yield return null;
		}
		while (true)
		{
			ShopInitialization.SendShopStatusTelemetryIfStalled();
			yield return null;
		}
	}

	private void RegisterEventListeners()
	{
		StoreManager.Get().RegisterStatusChangedListener(OnStoreStatusChanged);
		AddEventListener(UIEventType.ROLLOUT, OnButtonOut);
		AddEventListener(UIEventType.ROLLOVER, OnButtonOver);
		AddEventListener(UIEventType.RELEASE, OnButtonRelease);
	}

	private void UnregisterEventListeners()
	{
		StoreManager.Get().RemoveStatusChangedListener(OnStoreStatusChanged);
		RemoveEventListener(UIEventType.ROLLOUT, OnButtonOut);
		RemoveEventListener(UIEventType.ROLLOVER, OnButtonOver);
		RemoveEventListener(UIEventType.RELEASE, OnButtonRelease);
	}
}
