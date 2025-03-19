using System;
using System.Collections.Generic;
using AFMiniJSON;
using Blizzard.BlizzardErrorMobile;

namespace Hearthstone.Attribution.AppsFlyer;

internal static class HsAppsFlyer
{
	private interface IAppsFlyerSDK
	{
		bool IsEnabled { get; }

		void initSDK(string devKey, string appID);

		void startSDK(string callbackObjectName = null);

		void waitForATTUserAuthorizationWithTimeoutInterval(int timeoutInterval);

		void setCustomerUserId(string id);

		void setIsDebug(bool shouldEnable);

		void sendEvent(string eventName, Dictionary<string, string> eventValues);

		void setHost(string hostPrefixName, string hostName);

		void setSharingFilterAll();

		void RegisterUninstall(byte[] deviceToken);

		void UpdateServerUninstallToken(string token);

		void stopSdk(bool isSdkStopped);
	}

	private class NoopAppsFlyerSDK : IAppsFlyerSDK
	{
		public bool IsEnabled => false;

		public void initSDK(string devKey, string appID)
		{
		}

		public void startSDK(string callbackObjectName = null)
		{
		}

		public void waitForATTUserAuthorizationWithTimeoutInterval(int timeoutInterval)
		{
		}

		public void sendEvent(string eventName, Dictionary<string, string> eventValues)
		{
		}

		public void setCustomerUserId(string id)
		{
		}

		public void setIsDebug(bool shouldEnable)
		{
		}

		public void setHost(string hostPrefixName, string hostName)
		{
		}

		public void setSharingFilterAll()
		{
		}

		public void RegisterUninstall(byte[] deviceToken)
		{
		}

		public void UpdateServerUninstallToken(string token)
		{
		}

		public void stopSdk(bool isSdkStopped)
		{
		}
	}

	private static bool s_enableDebugLogs;

	private static IAppsFlyerSDK s_sdk;

	private static string s_notificationToken;

	private static bool s_registerUninstallRequired;

	public static void Initialize(int attUserAuthorizationTimeoutSeconds, bool optOutOfSharing)
	{
		if (s_sdk != null)
		{
			Log.AdTracking.PrintWarning("AFSDK already initialized");
			return;
		}
		IAppsFlyerSDK afsdk = null;
		afsdk = new NoopAppsFlyerSDK();
		if (!afsdk.IsEnabled)
		{
			Log.AdTracking.PrintInfo("AFSDK disabled for this platform");
			return;
		}
		Log.AdTracking.PrintInfo("AFSDK enabled for this platform");
		Log.AdTracking.PrintInfo("Initializing AFSDK");
		try
		{
			afsdk.setIsDebug(s_enableDebugLogs);
			string appID = string.Empty;
			if (HearthstoneApplication.IsCNMobileBinary)
			{
				afsdk.setHost("", "appsflyer-cn.com");
			}
			afsdk.initSDK("biU9Lo4fZQJRMhPK4VVZjP", appID);
			if (attUserAuthorizationTimeoutSeconds > 0)
			{
				afsdk.waitForATTUserAuthorizationWithTimeoutInterval(attUserAuthorizationTimeoutSeconds);
			}
			StartSDK(afsdk, optOutOfSharing);
			s_sdk = afsdk;
			Log.AdTracking.PrintInfo("AFSDK initialized");
		}
		catch (Exception ex)
		{
			Log.AdTracking.PrintError("Failed to initialize AFSDK: " + ex);
			ExceptionReporter.Get().ReportCaughtException(ex);
			return;
		}
		_ = s_registerUninstallRequired;
	}

	private static void StartSDK(IAppsFlyerSDK afsdk, bool isOptedOutOfSharing)
	{
		if (isOptedOutOfSharing)
		{
			Log.AdTracking.PrintDebug("Sharing is opted out. Setting sharing filter to all");
			afsdk.setSharingFilterAll();
		}
		string callbackObjectName = null;
		if (s_enableDebugLogs)
		{
			HsAppsFlyerMessageReceiver callbackObj = HsAppsFlyerMessageReceiver.Instance;
			callbackObjectName = (callbackObj ? callbackObj.name : null);
		}
		afsdk.startSDK(callbackObjectName);
	}

	public static void Stop(bool isSdkStopped)
	{
		s_sdk?.stopSdk(isSdkStopped);
	}

	public static void OptOutOfSharing()
	{
		if (s_sdk != null)
		{
			s_sdk.setSharingFilterAll();
			s_sdk.sendEvent("do not share", new Dictionary<string, string>());
			Log.AdTracking.PrintDebug("Sent sharing filter and opt out event to appsflyer sdk");
		}
	}

	public static void SetCustomerUserId(string id)
	{
		if (s_sdk == null)
		{
			Log.AdTracking.PrintWarning("AF not initialized. Can't set id.");
			return;
		}
		Log.AdTracking.PrintInfo("Applying AF customer user id");
		try
		{
			if (s_enableDebugLogs)
			{
				Log.AdTracking.PrintInfo("AF customer user id set to " + id);
			}
			s_sdk.setCustomerUserId(id);
		}
		catch (Exception ex)
		{
			Log.AdTracking.PrintError("Failed set AF customer user id: " + ex);
			ExceptionReporter.Get().ReportCaughtException(ex);
		}
	}

	public static void SendEvent(string eventName, Dictionary<string, string> eventValues)
	{
		if (s_sdk == null)
		{
			Log.AdTracking.PrintWarning("AF not initialized. Can't log event " + eventName + ".");
			return;
		}
		Log.AdTracking.PrintInfo("Logging AF event: " + eventName);
		try
		{
			if (s_enableDebugLogs)
			{
				Log.AdTracking.PrintInfo("    eventValues=" + Json.Serialize(eventValues));
			}
			s_sdk.sendEvent(eventName, eventValues);
		}
		catch (Exception ex)
		{
			Log.AdTracking.PrintError("Failed to log AF event: " + ex);
			ExceptionReporter.Get().ReportCaughtException(ex);
		}
	}

	internal static void OniOSRegisterForRemoteNotification(string deviceToken)
	{
		if (s_sdk == null)
		{
			s_notificationToken = deviceToken;
			s_registerUninstallRequired = true;
		}
		else
		{
			s_registerUninstallRequired = false;
			byte[] tokenBytes = ConvertHexStringToByteArray(deviceToken);
			s_sdk.RegisterUninstall(tokenBytes);
		}
	}

	internal static void OnAndroidRegisterForRemoteNotification(string token)
	{
		if (s_sdk == null)
		{
			s_notificationToken = token;
			s_registerUninstallRequired = true;
		}
		else
		{
			s_registerUninstallRequired = false;
			s_sdk.UpdateServerUninstallToken(token);
		}
	}

	private static byte[] ConvertHexStringToByteArray(string hexString)
	{
		byte[] data = new byte[hexString.Length / 2];
		for (int index = 0; index < data.Length; index++)
		{
			string byteValue = hexString.Substring(index * 2, 2);
			data[index] = Convert.ToByte(byteValue, 16);
		}
		return data;
	}
}
