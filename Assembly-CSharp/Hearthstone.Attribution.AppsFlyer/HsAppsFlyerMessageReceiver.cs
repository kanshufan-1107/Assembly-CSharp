using System;
using AppsFlyerSDK;
using UnityEngine;

namespace Hearthstone.Attribution.AppsFlyer;

public class HsAppsFlyerMessageReceiver : MonoBehaviour, IAppsFlyerValidateReceipt, IAppsFlyerConversionData, IAppsFlyerUserInvite
{
	private const string LOG_TAG = "[AppsFlyer]";

	private static HsAppsFlyerMessageReceiver s_instance;

	private static EventHandler s_noopEventHandler = delegate
	{
	};

	public static HsAppsFlyerMessageReceiver Instance
	{
		get
		{
			if (!s_instance)
			{
				GameObject obj = new GameObject("HsAppsFlyerMessageReceiver", typeof(HsAppsFlyerMessageReceiver));
				obj.hideFlags |= HideFlags.HideAndDontSave;
				UnityEngine.Object.DontDestroyOnLoad(obj);
				s_instance = obj.GetComponent<HsAppsFlyerMessageReceiver>();
				AppsFlyerSDK.AppsFlyer.OnRequestResponse -= s_noopEventHandler;
				AppsFlyerSDK.AppsFlyer.OnRequestResponse += s_noopEventHandler;
				AppsFlyerSDK.AppsFlyer.OnInAppResponse -= s_noopEventHandler;
				AppsFlyerSDK.AppsFlyer.OnInAppResponse += s_noopEventHandler;
				AppsFlyerSDK.AppsFlyer.OnDeepLinkReceived -= s_noopEventHandler;
				AppsFlyerSDK.AppsFlyer.OnDeepLinkReceived += s_noopEventHandler;
			}
			return s_instance;
		}
	}

	private void Awake()
	{
		if ((bool)s_instance)
		{
			Debug.LogError("Invalid usage of HsAppsFlyerMessageReceiver: This class is supposed to be a singleton, multiple instances detected.");
			UnityEngine.Object.DestroyImmediate(base.gameObject);
		}
	}

	private void LogIfError(string message)
	{
		if (message.Contains("errorDescription"))
		{
			Log.AdTracking.PrintError("AppsFlyer error: " + message);
		}
	}

	void IAppsFlyerValidateReceipt.didFinishValidateReceipt(string msg)
	{
		Log.AdTracking.PrintInfo("[AppsFlyer] didFinishValidateReceipt: " + msg);
	}

	void IAppsFlyerValidateReceipt.didFinishValidateReceiptWithError(string err)
	{
		Log.AdTracking.PrintError("[AppsFlyer] didFinishValidateReceiptWithError: " + err);
	}

	void IAppsFlyerConversionData.onConversionDataSuccess(string msg)
	{
		Log.AdTracking.PrintInfo("[AppsFlyer] onConversionDataSuccess: " + msg);
	}

	void IAppsFlyerConversionData.onConversionDataFail(string err)
	{
		Log.AdTracking.PrintError("[AppsFlyer] onConversionDataFail: " + err);
	}

	void IAppsFlyerConversionData.onAppOpenAttribution(string msg)
	{
		Log.AdTracking.PrintInfo("[AppsFlyer] onAppOpenAttribution: " + msg);
	}

	void IAppsFlyerConversionData.onAppOpenAttributionFailure(string err)
	{
		Log.AdTracking.PrintError("[AppsFlyer] onAppOpenAttributionFailure: " + err);
	}

	void IAppsFlyerUserInvite.onInviteLinkGenerated(string msg)
	{
		Log.AdTracking.PrintInfo("[AppsFlyer] onInviteLinkGenerated: " + msg);
	}

	void IAppsFlyerUserInvite.onInviteLinkGeneratedFailure(string err)
	{
		Log.AdTracking.PrintError("[AppsFlyer] onInviteLinkGeneratedFailure: " + err);
	}

	void IAppsFlyerUserInvite.onOpenStoreLinkGenerated(string msg)
	{
		Log.AdTracking.PrintInfo("[AppsFlyer] onOpenStoreLinkGenerated: " + msg);
	}

	private void requestResponseReceived(string msg)
	{
		LogIfError("requestResponseReceived " + msg);
		Log.AdTracking.PrintInfo("[AppsFlyer] requestResponseReceived: " + msg);
	}

	private void inAppResponseReceived(string msg)
	{
		LogIfError("inAppResponseReceived " + msg);
		Log.AdTracking.PrintInfo("[AppsFlyer] inAppResponseReceived: " + msg);
	}

	private void onDeepLinking(string msg)
	{
		LogIfError("onDeepLinking " + msg);
		Log.AdTracking.PrintInfo("[AppsFlyer] onDeepLinking: " + msg);
	}
}
