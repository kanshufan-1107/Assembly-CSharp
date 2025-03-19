using System;
using System.Collections;
using Hearthstone.UI;
using UnityEngine;

public class RedundantNDEPopup : MonoBehaviour
{
	public UIBButton m_rerollButton;

	public UIBButton m_refuseButton;

	public Widget m_rootWidget;

	private GameObject m_owner;

	public event Action RerollSelected;

	public event Action RefuseSelected;

	public event Action OnDismissAnimationComplete;

	private void Awake()
	{
		if ((bool)m_rerollButton)
		{
			m_rerollButton.AddEventListener(UIEventType.RELEASE, OnRerollSelected);
		}
		if ((bool)m_refuseButton)
		{
			m_refuseButton.AddEventListener(UIEventType.RELEASE, OnRefuseSelected);
		}
		m_owner = base.gameObject;
		if (base.transform.parent != null && base.transform.parent.GetComponent<WidgetInstance>() != null)
		{
			m_owner = base.transform.parent.gameObject;
		}
		OverlayUI.Get().AddGameObject(m_owner);
	}

	private void OnDestroy()
	{
		UIContext.GetRoot().DismissPopup(m_owner);
	}

	private void OnRerollSelected(UIEvent e)
	{
		this.RerollSelected?.Invoke();
	}

	private void OnRefuseSelected(UIEvent e)
	{
		this.RefuseSelected?.Invoke();
	}

	public void Show()
	{
		UIContext.GetRoot().ShowPopup(m_owner);
	}

	public IEnumerator Hide()
	{
		m_rootWidget.TriggerEvent("Popup_Outro");
		yield return new WaitForSeconds(0.4f);
		UnityEngine.Object.Destroy(m_owner);
		this.OnDismissAnimationComplete?.Invoke();
	}
}
