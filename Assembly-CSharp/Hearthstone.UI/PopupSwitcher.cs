using System;
using System.Collections.Generic;
using Blizzard.T5.Core.Utils;

namespace Hearthstone.UI;

public class PopupSwitcher
{
	private class PopupHandle
	{
		private PopupRoot m_popupRoot;

		private string m_hideEventName;

		private string m_dismissEventName;

		private Action<IDataModel> m_onShow;

		private Action m_onHide;

		public bool IsShown { get; private set; }

		public AsyncReference AsyncReference { get; }

		public WidgetInstance WidgetInstance => AsyncReference?.Object as WidgetInstance;

		public PopupHandle(AsyncReference asyncReference, string hideEventName, string dismissEventName, Action<IDataModel> onShow, Action onHide)
		{
			AsyncReference = asyncReference;
			m_hideEventName = hideEventName;
			m_dismissEventName = dismissEventName;
			m_onShow = onShow;
			m_onHide = onHide;
			asyncReference.RegisterReadyListener<WidgetInstance>(OnWidgetInstanceReady);
		}

		private void OnWidgetInstanceReady(WidgetInstance widgetInstance)
		{
			widgetInstance.RegisterEventListener(OnWidgetEvent);
		}

		public void Show(IDataModel dataModel)
		{
			WidgetInstance widgetInstance = WidgetInstance;
			if (widgetInstance == null)
			{
				Log.All.PrintError("Cannot show Lucky Draw popup. Widget not ready.");
				return;
			}
			IsShown = true;
			if (dataModel != null)
			{
				widgetInstance.BindDataModel(dataModel);
			}
			widgetInstance.gameObject.SetActive(value: true);
			if (!OverlayUI.Get().HasObject(widgetInstance.gameObject))
			{
				OverlayUI.Get().AddGameObject(widgetInstance.gameObject, CanvasAnchor.CENTER, destroyOnSceneLoad: true);
			}
			m_popupRoot = UIContext.GetRoot().ShowPopup(widgetInstance.gameObject);
			if (m_popupRoot != null)
			{
				m_popupRoot.OnDestroyed += OnPopupDestroyed;
			}
			if (GeneralUtils.IsCallbackValid(m_onShow))
			{
				m_onShow(dataModel);
			}
		}

		public void Hide()
		{
			if (IsShown && GeneralUtils.IsCallbackValid(m_onHide))
			{
				m_onHide();
			}
		}

		public void DismissPopup()
		{
			if (IsShown)
			{
				IsShown = false;
				WidgetInstance widgetInstance = WidgetInstance;
				if (widgetInstance != null)
				{
					UIContext.GetRoot().DismissPopup(widgetInstance.gameObject);
					widgetInstance.gameObject.SetActive(value: false);
				}
			}
		}

		private void OnPopupDestroyed(PopupRoot popupRoot)
		{
			if (!(popupRoot == m_popupRoot))
			{
				return;
			}
			m_popupRoot = null;
			if (IsShown)
			{
				IsShown = false;
				if (GeneralUtils.IsCallbackValid(m_onHide))
				{
					m_onHide();
				}
			}
		}

		private void OnWidgetEvent(string eventName)
		{
			if (m_hideEventName != null && eventName == m_hideEventName)
			{
				Hide();
			}
			if (m_dismissEventName != null && eventName == m_dismissEventName)
			{
				DismissPopup();
			}
		}
	}

	private Dictionary<AsyncReference, PopupHandle> m_popupHandles = new Dictionary<AsyncReference, PopupHandle>();

	public void RegisterPopupWidgetInstance(AsyncReference widgetInstanceReference, string hideEventName = null, string dismissEventName = null, Action<IDataModel> onShow = null, Action onHide = null)
	{
		m_popupHandles[widgetInstanceReference] = new PopupHandle(widgetInstanceReference, hideEventName, dismissEventName, onShow, onHide);
	}

	public void ShowPopup(AsyncReference widgetInstanceReference, IDataModel dataModel)
	{
		foreach (KeyValuePair<AsyncReference, PopupHandle> pair in m_popupHandles)
		{
			if (pair.Key == widgetInstanceReference)
			{
				pair.Value.Show(dataModel);
			}
			else
			{
				pair.Value.Hide();
			}
		}
	}

	public void HidePopup(AsyncReference widgetInstanceReference)
	{
		if (m_popupHandles.TryGetValue(widgetInstanceReference, out var popupHandle))
		{
			popupHandle.Hide();
		}
	}

	public void DismissPopup(AsyncReference widgetInstanceReference)
	{
		if (m_popupHandles.TryGetValue(widgetInstanceReference, out var popupHandle))
		{
			popupHandle.DismissPopup();
		}
	}
}
