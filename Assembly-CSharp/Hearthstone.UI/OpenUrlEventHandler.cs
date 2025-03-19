using Hearthstone.DataModels;
using Hearthstone.UI.Core;
using UnityEngine;

namespace Hearthstone.UI;

[AddComponentMenu("")]
public class OpenUrlEventHandler : MonoBehaviour, IWidgetEventListener
{
	[SerializeField]
	private string m_url;

	public const string EventName = "OPEN_URL";

	private WidgetTemplate m_owningWidget;

	[Overridable]
	public string Url
	{
		get
		{
			return m_url;
		}
		set
		{
			m_url = value;
		}
	}

	public WidgetTemplate OwningWidget => m_owningWidget;

	private void Awake()
	{
		m_owningWidget = GetComponentInParent<WidgetTemplate>();
		if (m_owningWidget == null)
		{
			Log.UIFramework.PrintError("OpenUrlEventHandler unable to find its owning widget, so it won't be able to retrieve the URL payload!");
		}
	}

	public WidgetEventListenerResponse EventReceived(string eventName, TriggerEventParameters parameters)
	{
		WidgetEventListenerResponse response = default(WidgetEventListenerResponse);
		if (m_owningWidget == null || eventName != "OPEN_URL")
		{
			return response;
		}
		if (m_owningWidget.GetDataModel(120, base.gameObject, out var datamodel) && datamodel is EventDataModel { Payload: not null, Payload: string urlStr })
		{
			Log.UIFramework.PrintDebug("OpenUrlEventHandler received a valid URL from the URL payload.");
			Application.OpenURL(urlStr);
			return response;
		}
		if (!string.IsNullOrEmpty(Url))
		{
			Log.UIFramework.PrintDebug("OpenUrlEventHandler received a valid URL locally.");
			OpenLink(Url);
			return response;
		}
		Log.UIFramework.PrintError("OpenUrlEventHandler didn't receive a valid URL.");
		return response;
	}

	private void OpenLink(string url)
	{
		if (DeepLinkManager.TryParseUri(url, out var deepLinkArgs))
		{
			if (DeepLinkManager.ExecuteDeepLink(deepLinkArgs, DeepLinkManager.DeepLinkSource.IN_GAME_MESSAGE, 0))
			{
				Log.UIFramework.PrintDebug("OpenUrlEventHandler opened internal deeplink.");
			}
			else
			{
				Log.UIFramework.PrintError("OpenUrlEventHandler received malformed deeplink url scheme.");
			}
		}
		else
		{
			Log.UIFramework.PrintDebug("OpenUrlEventHandler opened external link.");
			Application.OpenURL(Url);
		}
	}
}
