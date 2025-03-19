using System;
using System.Collections.Generic;
using Hearthstone;
using Hearthstone.Attribution.AppsFlyer;
using Hearthstone.CRM;
using Hearthstone.InGameMessage;
using Hearthstone.Notifications;
using HearthstoneTelemetry;
using MiniJSON;
using PegasusShared;
using UnityEngine;

public class PushNotificationManager : MonoBehaviour
{
	private class CampaignInfo
	{
		public string cid;
	}

	private enum OSNotificationStatus
	{
		Unknown,
		Disallowed,
		Allowed,
		AllowedProvisional
	}

	public const int UNASKED = 1;

	public const int DISALLOWED = 2;

	public const int ALLOWED = 3;

	public const int ALLOWED_PROVISIONAL = 4;

	private const string BRAZE_DEFAULT_NOTIFICATION_CHANNEL_ID = "com_appboy_default_notification_channel";

	private const string BRAZE_PUSH_ENABLED_ATTRIBUTE = "hs_c_account_push_enabled";

	private static PushNotificationManager s_instance;

	private static ITelemetryClient s_telemetryClient;

	private static Action s_dismissCallback = null;

	private static bool s_isShowingContext = false;

	private static bool s_isInitialized = false;

	private static string s_brazeAppName;

	private static string s_brazeUserId = string.Empty;

	private OSNotificationStatus m_OSNotificationStatus;

	private readonly List<IBrazeInGameMessageListener> m_brazeInGameMessageHandlers = new List<IBrazeInGameMessageListener>();

	private BrazeInGameMessage m_lastBrazeInGameMessage;

	private readonly List<int> m_iamCallbackRemovalCachedCollection = new List<int>();

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	private static void Initialize()
	{
		try
		{
			if (Options.Get().GetInt(Option.PUSH_NOTIFICATION_STATUS, 1) == 2)
			{
				HsBraze.SetCustomUserAttribute("hs_c_account_push_enabled", value: false);
			}
			s_telemetryClient = TelemetryManager.Client();
			GameObject gameObject = new GameObject("PushNotificationManager", typeof(HSDontDestroyOnLoad));
			s_instance = gameObject.AddComponent<PushNotificationManager>();
			if (HearthstoneApplication.IsCNMobileBinary)
			{
				Log.MobileCallback.Print("PushNotificationManager - Aborting Initialize due to lack of support for CN Region (Expect lack of functionality)");
				HsBraze.DisableSDK();
				return;
			}
			s_brazeAppName = (HearthstoneApplication.IsCNMobileBinary ? "Hearthstone Production - CN" : "Hearthstone Production - Global");
			s_telemetryClient.SendPushNotificationSystemAppInitialized(s_brazeAppName, s_brazeUserId);
			HsBraze.ConfigureListener_PushReceived(gameObject.name, "OnPushNotificationReceived");
			HsBraze.ConfigureListener_PushOpened(gameObject.name, "OnPushNotificationOpened");
			HsBraze.SetInAppMessageDisplayAction_IAM_Discard();
			s_isInitialized = true;
			Log.MobileCallback.Print(string.Format("{0} - Completed initialization? {1}", "PushNotificationManager", s_isInitialized));
		}
		catch (Exception ex)
		{
			Log.MobileCallback.PrintException(ex, "PushNotificationManager failed initialization due to: " + ex.Message);
		}
	}

	public static PushNotificationManager Get()
	{
		return s_instance;
	}

	public void OnLoginComplete()
	{
		Log.MobileCallback.PrintDebug($"Braze is supported for this build: {HsBraze.IsEnabled}");
		Debug.Log($"Braze is supported  for this build: {HsBraze.IsEnabled}");
	}

	public void SetPushNotificationFeatureStatus(bool isEnabled)
	{
		Log.MobileCallback.Print(string.Format("{0} - Setting push notification feature status to: {1}", "PushNotificationManager", isEnabled));
		if (!s_isInitialized)
		{
			Log.MobileCallback.Print("PushNotificationManager - Setting feature status aborted due to manager being uninitialized...");
		}
		else if (!isEnabled)
		{
			UnregisterPushNotifications();
		}
		else if (TemporaryAccountManager.IsTemporaryAccount())
		{
			HandleTemporaryAccountChecks();
		}
		else
		{
			ProcessDeviceNotificationStatus();
		}
	}

	public void RegisterBrazeInGameMessageHandler(IBrazeInGameMessageListener handler)
	{
		if (handler != null && !m_brazeInGameMessageHandlers.Contains(handler))
		{
			m_brazeInGameMessageHandlers.Add(handler);
			if (m_lastBrazeInGameMessage != null)
			{
				handler.OnBrazeInGameMessageReceived(m_lastBrazeInGameMessage);
			}
		}
	}

	public void UnregisterBrazeInGameMessageHandler(IBrazeInGameMessageListener handler)
	{
		if (handler != null)
		{
			m_brazeInGameMessageHandlers.Remove(handler);
		}
	}

	private void HandleTemporaryAccountChecks()
	{
		ProcessDeviceNotificationStatus();
	}

	public bool ShowPushNotificationContext(Action dismissCallback)
	{
		Log.MobileCallback.Print(string.Format("{0} - Showing push notifications context. DismissCallback: {1}", "PushNotificationManager", dismissCallback));
		if (!s_isInitialized)
		{
			return false;
		}
		if (PlatformSettings.RuntimeOS == OSCategory.PC || PlatformSettings.RuntimeOS == OSCategory.Mac)
		{
			return false;
		}
		if (!IsNotificationOptInRequired())
		{
			return false;
		}
		if (SpectatorManager.Get().IsSpectatingOrWatching)
		{
			return false;
		}
		if (!PrivacyGate.Get().FeatureEnabled(PrivacyFeatures.PUSH_NOTIFICATIONS))
		{
			return false;
		}
		if (m_OSNotificationStatus == OSNotificationStatus.Allowed)
		{
			return false;
		}
		if (Options.Get().GetInt(Option.PUSH_NOTIFICATION_STATUS, 1) != 1)
		{
			return false;
		}
		if (GetGamesWon() < 3)
		{
			return false;
		}
		ShowPushNotificationOptInPrompt(dismissCallback);
		return true;
	}

	private void ShowPushNotificationOptInPrompt(Action onDismissCallback = null)
	{
		if (s_isShowingContext)
		{
			Log.MobileCallback.PrintError("PushNotificationManager attempted to display opt-in prompt while flag for show status is already true!");
			return;
		}
		AlertPopup.PopupInfo info = new AlertPopup.PopupInfo
		{
			m_headerText = GameStrings.Get("GLUE_PUSH_NOTIFICATION_CONTEXT_HEADER"),
			m_text = GameStrings.Get("GLUE_PUSH_NOTIFICATION_CONTEXT_BODY"),
			m_alertTextAlignment = UberText.AlignmentOptions.Center,
			m_showAlertIcon = false,
			m_responseDisplay = AlertPopup.ResponseDisplay.CONFIRM_CANCEL,
			m_confirmText = GameStrings.Get("GLUE_PUSH_NOTIFICATION_CONTEXT_CONFIRM"),
			m_cancelText = GameStrings.Get("GLUE_PUSH_NOTIFICATION_CONTEXT_CANCEL"),
			m_responseCallback = OnPushNotificationContextResponse
		};
		s_dismissCallback = onDismissCallback;
		DialogManager.Get().ShowPopup(info);
		s_isShowingContext = true;
	}

	private void RegisterPushNotifications()
	{
		if (!s_isInitialized)
		{
			Log.MobileCallback.PrintError("PushNotificationManager - Attempting to register while not initialized!");
		}
		else
		{
			Log.MobileCallback.PrintWarning("PushNotificationManager - Ignoring register push notifications request - platform not supported.");
		}
	}

	private void UnregisterPushNotifications()
	{
		if (s_isInitialized)
		{
			Log.MobileCallback.PrintWarning("PushNotificationManager - Ignoring unregister push notifications request - platform not supported.");
		}
	}

	private void ProcessDeviceNotificationStatus()
	{
		Log.MobileCallback.Print("PushNotificationManager - Getting device push notification status.");
		if (!PrivacyGate.Get().FeatureEnabled(PrivacyFeatures.PUSH_NOTIFICATIONS))
		{
			OnHandleDevicePushNotificationStatus(2.ToString());
		}
	}

	private int GetGamesWon()
	{
		int totalWins = 0;
		NetCache.NetCachePlayerRecords cachedPlayerRecords = NetCache.Get()?.GetNetObject<NetCache.NetCachePlayerRecords>();
		if (cachedPlayerRecords?.Records == null)
		{
			return totalWins;
		}
		foreach (NetCache.PlayerRecord record in cachedPlayerRecords.Records)
		{
			if (record.Data == 0)
			{
				switch (record.RecordType)
				{
				case GameType.GT_VS_AI:
				case GameType.GT_ARENA:
				case GameType.GT_RANKED:
				case GameType.GT_CASUAL:
				case GameType.GT_TAVERNBRAWL:
				case GameType.GT_FSG_BRAWL:
				case GameType.GT_FSG_BRAWL_2P_COOP:
					totalWins += record.Wins;
					break;
				}
			}
		}
		return totalWins;
	}

	private void OnPushNotificationContextResponse(AlertPopup.Response response, object userData)
	{
		Log.MobileCallback.Print(string.Format("{0}, OnPushNotificationContextResponse({1}, {2})", "PushNotificationManager", response, userData));
		if (!PrivacyGate.Get().FeatureEnabled(PrivacyFeatures.PUSH_NOTIFICATIONS))
		{
			Log.MobileCallback.PrintError("PushNotificationManager - Attempted to perform " + response.ToString() + " action from opt-in prompt while BNet notification privacy setting is disabled and should never have got this far! Disabling notifications...");
			s_dismissCallback?.Invoke();
			s_isShowingContext = false;
			return;
		}
		if (response == AlertPopup.Response.CANCEL)
		{
			Log.MobileCallback.Print("PushNotificationManager - In-app prompt: Push Notification permission denied.");
			Options.Get().SetInt(Option.PUSH_NOTIFICATION_STATUS, 2);
			UnregisterPushNotifications();
		}
		else
		{
			Log.MobileCallback.Print("PushNotificationManager - In-app prompt: Push Notification permission authorized.");
			Options.Get().SetInt(Option.PUSH_NOTIFICATION_STATUS, 3);
			RegisterPushNotifications();
		}
		s_dismissCallback?.Invoke();
		s_isShowingContext = false;
	}

	public void OnPushTokenReceived(string token)
	{
		if (string.IsNullOrEmpty(token))
		{
			Log.MobileCallback.PrintError("OnPushTokenReceived push token in null. Push notifications will not work.");
			return;
		}
		Log.MobileCallback.Print("OnPushTokenReceived(" + token + ")");
		BlizzardCRMManager.Get().SendEvent_PushRegistration(token);
		s_telemetryClient.SendPushNotificationSystemDeviceTokenObtained(s_brazeAppName, s_brazeUserId, token);
		HsAppsFlyer.OnAndroidRegisterForRemoteNotification(token);
	}

	private void OnPushNotificationReceived(string message)
	{
		if (string.IsNullOrEmpty(message))
		{
			Log.MobileCallback.Print("OnPushNotificationReceived message received from push notification was empty.");
			return;
		}
		if (message.Contains("af-uinstall-tracking"))
		{
			Log.MobileCallback.Print("OnPushNotificationReceived message received from push notification was for af-uinstall-tracking.");
			return;
		}
		CampaignInfo info = new CampaignInfo();
		try
		{
			info = JsonUtility.FromJson<CampaignInfo>(message);
		}
		catch (Exception ex)
		{
			Log.MobileCallback.PrintException(ex, string.Format("{0} while parsing {1} hit an exception: {2}", "PushNotificationManager", "CampaignInfo", ex.InnerException));
		}
		BlizzardCRMManager.Get().SendEvent_PushEvent(info.cid, null);
		s_telemetryClient.SendPushNotificationSystemNotificationReceived(s_brazeAppName, s_brazeUserId);
	}

	private void OnInAppMessageReceived(string response)
	{
		Log.MobileCallback.Print("[PushNotificationManager] Received InAppMessage from Braze: " + response);
		m_lastBrazeInGameMessage = null;
		if (string.IsNullOrEmpty(response) || response == "{}")
		{
			return;
		}
		try
		{
			if (Json.Deserialize(response) is JsonNode { Count: not 0 } igmJsonNode)
			{
				BrazeInGameMessage igm = BrazeInGameMessageExtensions.GetBrazeIgmFromJsonNode(igmJsonNode);
				if (igm == null)
				{
					Log.InGameMessage.PrintError("[PushNotificationManager] received IAM but failed to deserialize as: BrazeInGameMessage!");
					return;
				}
				m_lastBrazeInGameMessage = igm;
				for (int i = 0; i < m_brazeInGameMessageHandlers.Count; i++)
				{
					IBrazeInGameMessageListener handler = m_brazeInGameMessageHandlers[i];
					m_iamCallbackRemovalCachedCollection.Clear();
					if (handler == null)
					{
						m_iamCallbackRemovalCachedCollection.Add(i);
						continue;
					}
					try
					{
						handler.OnBrazeInGameMessageReceived(igm);
					}
					catch (Exception ex)
					{
						Log.InGameMessage.PrintException(ex, string.Format("[{0}] Braze in-app message receiver handler {1} failed due to: {2}", "PushNotificationManager", handler, ex.Message));
					}
				}
				for (int i2 = m_iamCallbackRemovalCachedCollection.Count - 1; i2 >= 0; i2--)
				{
					m_brazeInGameMessageHandlers.RemoveAt(i2);
				}
			}
			else
			{
				Log.InGameMessage.PrintError("[PushNotificationManager] received IAM but failed to deserialize as: JsonNode!");
			}
		}
		catch (Exception ex2)
		{
			Log.InGameMessage.PrintException(ex2, "[PushNotificationManager] Failed to process in-app message due to " + ex2.Message);
		}
	}

	public void OnPushNotificationOpened()
	{
		s_telemetryClient.SendPushNotificationSystemNotificationOpened(s_brazeAppName, s_brazeUserId);
	}

	public void OnPushNotificationDeleted()
	{
		s_telemetryClient.SendPushNotificationSystemNotificationDeleted(s_brazeAppName, s_brazeUserId);
	}

	private void OnDidRegisterForRemoteNotificationsWithDeviceToken(string deviceToken)
	{
		Log.MobileCallback.Print("OnDidRegisterForRemoteNotificationsWithDeviceToken(" + deviceToken + ")");
		s_telemetryClient.SendPushRegistrationSucceeded(deviceToken);
		HsAppsFlyer.OniOSRegisterForRemoteNotification(deviceToken);
	}

	private void OnDidFailToRegisterForRemoteNotificationsWithError(string error)
	{
		Log.MobileCallback.Print("OnDidFailToRegisterForRemoteNotificationsWithError(" + error + ")");
		s_telemetryClient.SendPushRegistrationFailed(error);
	}

	private void OnHandleDevicePushNotificationStatus(string status)
	{
		Log.MobileCallback.Print("PushNotificationManager - OnHandleDevicePushNotificationStatus(" + status + ")");
		if (status == 2.ToString())
		{
			Log.MobileCallback.Print("PushNotificationManager - Native OS prompt: Push Notification permission denied.");
			m_OSNotificationStatus = OSNotificationStatus.Disallowed;
			UnregisterPushNotifications();
		}
		else if (status == 1.ToString())
		{
			Log.MobileCallback.Print("PushNotificationManager - Native OS prompt status: Push Notification ignored, device hasn't been asked yet.");
			m_OSNotificationStatus = OSNotificationStatus.Disallowed;
			UnregisterPushNotifications();
		}
		else if (status == 4.ToString())
		{
			Log.MobileCallback.Print("PushNotificationManager - Native OS prompt status: Push Notification ignored, device hasn't been asked yet and allows provisional/silent notifications only.");
			m_OSNotificationStatus = OSNotificationStatus.AllowedProvisional;
			UnregisterPushNotifications();
		}
		else
		{
			Log.MobileCallback.Print("PushNotificationManager - Native OS prompt: Push Notification permission authorized.");
			m_OSNotificationStatus = OSNotificationStatus.Allowed;
			RegisterPushNotifications();
		}
	}

	private static bool IsNotificationOptInRequired()
	{
		return false;
	}
}
