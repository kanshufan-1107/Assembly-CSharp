using Hearthstone.UI;
using UnityEngine;

public class DeepLinkEventController : MonoBehaviour
{
	public string m_deepLink = "hearthstone://ranked";

	private Widget m_widget;

	private const string GoToDeepLink = "GoToDeepLink";

	private void Awake()
	{
		m_widget = GetComponent<WidgetTemplate>();
		if (m_widget != null)
		{
			m_widget.RegisterEventListener(OnEvent);
		}
	}

	private void OnEvent(string eventName)
	{
		if (eventName == "GoToDeepLink" && m_deepLink != null && m_deepLink.Length > 0)
		{
			DeepLinkManager.ExecuteDeepLink(m_deepLink.Substring("hearthstone://".Length).Split('/'), DeepLinkManager.DeepLinkSource.IN_GAME_MESSAGE, 0);
		}
	}
}
