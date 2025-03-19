using Hearthstone.UI;
using UnityEngine;

public class PushNotificationSoftAskController : MonoBehaviour
{
	private const string RESPONSE_YES = "RESPONSE_YES";

	private const string RESPONSE_NO = "RESPONSE_NO";

	private Widget m_widget;

	private static PushNotificationSoftAskController s_instance;

	private void Awake()
	{
		m_widget = GetComponent<Widget>();
		if (m_widget == null)
		{
			Log.MobileCallback.Print("PushNotificationSoftAskController - Widget null in Awake");
		}
		else
		{
			m_widget.RegisterEventListener(OnSoftAskWidgetEventReceived);
		}
	}

	private void OnDestroy()
	{
		if (m_widget != null)
		{
			m_widget.RemoveEventListener(OnSoftAskWidgetEventReceived);
		}
	}

	private void OnSoftAskWidgetEventReceived(string widgetEventResponse)
	{
		if (!(widgetEventResponse == "RESPONSE_YES"))
		{
			if (widgetEventResponse == "RESPONSE_NO")
			{
				Hide();
			}
			else
			{
				Log.MobileCallback.Print("PushNotificationSoftAskController - Event response not received");
			}
		}
		else
		{
			Show();
		}
	}

	public void Show()
	{
		m_widget.Show();
	}

	public void Hide()
	{
		m_widget.Hide();
	}

	public static bool ShowPushNotificationSoftAskController(bool shouldShow)
	{
		PushNotificationSoftAskController instance = GetInstance();
		if (instance == null)
		{
			return false;
		}
		if (shouldShow)
		{
			instance.Show();
			return true;
		}
		instance.Hide();
		return false;
	}

	private static PushNotificationSoftAskController GetInstance()
	{
		if (s_instance == null)
		{
			GameObject pushNotificationSoftAskControllerInstance = AssetLoader.Get().InstantiatePrefab("SoftAskWidget.prefab:1d06faa5053f1cc44bcc3c8d9b8c9082");
			if (pushNotificationSoftAskControllerInstance == null)
			{
				Debug.LogError("Push Notification Soft Ask Controller prefab is null in GetInstance");
				return s_instance;
			}
			s_instance = pushNotificationSoftAskControllerInstance.GetComponentInChildren<PushNotificationSoftAskController>();
			if (s_instance == null)
			{
				Debug.LogError($"{pushNotificationSoftAskControllerInstance} doesn't have {{[PushNotificationSoftAskController]}}");
				Object.Destroy(pushNotificationSoftAskControllerInstance);
				return s_instance;
			}
			Object.DontDestroyOnLoad(pushNotificationSoftAskControllerInstance);
		}
		return s_instance;
	}
}
