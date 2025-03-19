using System;
using Hearthstone.UI;
using UnityEngine;

public class TemporaryBnetAccountPrompt : MonoBehaviour
{
	private GameObject m_owner;

	public Action OnNewAccountSelected { get; set; }

	public Action OnExistingAccountSelected { get; set; }

	private void Awake()
	{
		WidgetInstance tempAccountPopup = base.transform.parent.GetComponent<WidgetInstance>();
		if (!(tempAccountPopup != null))
		{
			return;
		}
		m_owner = tempAccountPopup.gameObject;
		tempAccountPopup.RegisterReadyListener(delegate
		{
			OverlayUI.Get().AddGameObject(m_owner);
			UIContext.GetRoot().ShowPopup(m_owner);
			tempAccountPopup.RegisterEventListener(delegate(string eventName)
			{
				switch (eventName)
				{
				case "SHRINK_DONE":
					UnityEngine.Object.Destroy(m_owner);
					break;
				case "Button_Framed_Double_Right_Clicked":
					OnNewAccountSelected?.Invoke();
					tempAccountPopup.TriggerEvent("DISMISS_TEMP_ALERT");
					break;
				case "Button_Framed_Double_Left_Clicked":
					OnExistingAccountSelected?.Invoke();
					tempAccountPopup.TriggerEvent("DISMISS_TEMP_ALERT");
					break;
				}
			});
		});
	}

	private void OnDestroy()
	{
		OnNewAccountSelected = null;
		OnExistingAccountSelected = null;
		UIContext.GetRoot().DismissPopup(m_owner);
	}
}
