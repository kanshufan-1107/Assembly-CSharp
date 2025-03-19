using System.Collections.Generic;
using UnityEngine;

public class CollectionTeamInfo : MonoBehaviour
{
	public delegate void ShowListener();

	public delegate void HideListener();

	public GameObject m_root;

	public PegUIElement m_offClicker;

	private bool m_wasTouchModeEnabled;

	protected bool m_shown = true;

	private List<ShowListener> m_showListeners = new List<ShowListener>();

	private List<HideListener> m_hideListeners = new List<HideListener>();

	private void Awake()
	{
		m_wasTouchModeEnabled = true;
	}

	private void Start()
	{
		m_offClicker.AddEventListener(UIEventType.RELEASE, OnClosePressed);
		m_offClicker.AddEventListener(UIEventType.ROLLOVER, OverOffClicker);
	}

	private void Update()
	{
		if (m_wasTouchModeEnabled != UniversalInputManager.Get().IsTouchMode())
		{
			m_wasTouchModeEnabled = UniversalInputManager.Get().IsTouchMode();
			m_offClicker.gameObject.SetActive(value: true);
		}
	}

	public void Show()
	{
		if (!m_shown)
		{
			m_root.SetActive(value: true);
			m_shown = true;
			if (UniversalInputManager.Get().IsTouchMode())
			{
				Navigation.Push(GoBackImpl);
			}
			ShowListener[] array = m_showListeners.ToArray();
			for (int i = 0; i < array.Length; i++)
			{
				array[i]();
			}
		}
	}

	private bool GoBackImpl()
	{
		Hide();
		return true;
	}

	public void Hide()
	{
		Navigation.RemoveHandler(GoBackImpl);
		if (m_shown)
		{
			m_root.SetActive(value: false);
			m_shown = false;
			HideListener[] array = m_hideListeners.ToArray();
			for (int i = 0; i < array.Length; i++)
			{
				array[i]();
			}
		}
	}

	public void RegisterShowListener(ShowListener dlg)
	{
		m_showListeners.Add(dlg);
	}

	public void UnregisterShowListener(ShowListener dlg)
	{
		m_showListeners.Remove(dlg);
	}

	public void RegisterHideListener(HideListener dlg)
	{
		m_hideListeners.Add(dlg);
	}

	public void UnregisterHideListener(HideListener dlg)
	{
		m_hideListeners.Remove(dlg);
	}

	public bool IsShown()
	{
		return m_shown;
	}

	private void OnClosePressed(UIEvent e)
	{
		Hide();
	}

	private void OverOffClicker(UIEvent e)
	{
		Hide();
	}
}
