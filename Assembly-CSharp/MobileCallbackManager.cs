using System;
using System.Collections.Generic;
using Blizzard.T5.Core.Utils;
using Blizzard.T5.Jobs;
using Blizzard.T5.Services;
using Hearthstone;
using UnityEngine;
using UnityEngine.Profiling;

public class MobileCallbackManager : MonoBehaviour, IService
{
	private const string CHINESE_CURRENCY_CODE = "CNY";

	private const string CHINESE_COUNTRY_CODE = "CN";

	private const char RECEIPT_DATA_DELIMITER = '|';

	private const int LARGE_RECEIPT_CHAR_THRESHOLD = 9788;

	private const int MAX_TRIM_DELAY_SECONDS = 10;

	private ulong m_nextTrimAvailableTime;

	private ulong m_trimDelay = 1uL;

	private static bool m_isAppRatingPromptLoading = false;

	public static Action m_onAppRatingPromptHidden;

	public static string VersionCodeInStore { get; protected set; } = string.Empty;

	public static bool IsReadyVersionCodeInStore
	{
		get
		{
			if (AndroidDeviceSettings.Get().GetAndroidStore() == AndroidStore.GOOGLE)
			{
				return !string.IsNullOrEmpty(VersionCodeInStore);
			}
			return true;
		}
	}

	public static bool IsAppRatingPromptQueued { get; private set; }

	public static bool IgnorePlatformForAppRating { get; set; }

	public IEnumerator<IAsyncJobResult> Initialize(ServiceLocator serviceLocator)
	{
		CheckVersionInStore();
		yield break;
	}

	public Type[] GetDependencies()
	{
		return null;
	}

	public void Shutdown()
	{
	}

	public static MobileCallbackManager Get()
	{
		return ServiceManager.Get<MobileCallbackManager>();
	}

	public void ClearCaches(LowMemorySeverity severity)
	{
		if (severity == LowMemorySeverity.CRITICAL && ServiceManager.TryGet<SpellManager>(out var spellManager))
		{
			Debug.LogWarning("Clearing SpellCache");
			spellManager.Clear();
		}
	}

	public void LowMemoryWarning(string msg)
	{
		ulong epochNow = TimeUtils.DateTimeToUnixTimeStamp(DateTime.Now);
		if (epochNow < m_nextTrimAvailableTime)
		{
			Debug.Log("Ignored because it didn't pass max time(" + m_nextTrimAvailableTime + ")");
			return;
		}
		if (epochNow - m_nextTrimAvailableTime > 10)
		{
			m_trimDelay = 1uL;
		}
		m_nextTrimAvailableTime = epochNow + m_trimDelay;
		m_trimDelay *= 2uL;
		if (!EnumUtils.TryGetEnum<LowMemorySeverity>(msg, out var severity))
		{
			severity = LowMemorySeverity.MODERATE;
		}
		Debug.LogWarningFormat("Receiving LowMemoryWarning severity={0}", severity);
		ClearCaches(severity);
		HearthstoneApplication hearthstoneApplication = HearthstoneApplication.Get();
		if (hearthstoneApplication != null)
		{
			hearthstoneApplication.UnloadUnusedAssets();
		}
		PreviousInstanceStatus.LowMemoryCount++;
	}

	public static bool IsAndroidDeviceTabletSized()
	{
		if (Application.isEditor)
		{
			return true;
		}
		return false;
	}

	public static bool RequestAppReview(AppRatingPromptTrigger triggerReason, bool forcePopupToShow = false)
	{
		Log.MobileCallback.Print("RequestAppReview()");
		if (m_isAppRatingPromptLoading)
		{
			return false;
		}
		if (IsAppRatingPromptQueued)
		{
			return false;
		}
		if (!forcePopupToShow)
		{
			if (!IgnorePlatformForAppRating && PlatformSettings.RuntimeOS != OSCategory.iOS && (PlatformSettings.RuntimeOS != OSCategory.Android || AndroidDeviceSettings.Get().GetAndroidStore() != AndroidStore.GOOGLE))
			{
				Log.MobileCallback.PrintInfo("No applicable storefront for rating app found.");
				return false;
			}
			if (NetCache.Get() != null)
			{
				NetCache.NetCacheFeatures features = NetCache.Get().GetNetObject<NetCache.NetCacheFeatures>();
				if (!features.AppRatingEnabled)
				{
					return false;
				}
				if (((float?)BnetUtils.TryGetGameAccountId()).HasValue && !BnetUtils.IsPlayerPartOfSamplingPercentage(features.AppRatingSamplingPercentage))
				{
					return false;
				}
			}
			if (Options.Get().GetBool(Option.APP_RATING_AGREED))
			{
				return false;
			}
			if (Options.Get().GetInt(Option.APP_RATING_PROMPT_LAST_MAJOR_VERSION_SEEN) == 31)
			{
				return false;
			}
		}
		else
		{
			Log.MobileCallback.Print("Forcing app rating popup to show, bypassing popup limitations.");
		}
		IsAppRatingPromptQueued = true;
		return true;
	}

	public static bool DisplayAppRatingPopup()
	{
		if (m_isAppRatingPromptLoading)
		{
			return false;
		}
		m_isAppRatingPromptLoading = true;
		AssetLoader.Get().InstantiatePrefab("RatingsPrompt.prefab:9b2e45ccc2e3add4dabd6bbd3fcd9ba1", OnAppRatingPopupShown);
		Options.Get().SetInt(Option.APP_RATING_PROMPT_LAST_MAJOR_VERSION_SEEN, 31);
		return true;
	}

	private static void OnAppRatingPopupShown(AssetReference assetRef, GameObject go, object callbackData)
	{
		m_isAppRatingPromptLoading = false;
		IsAppRatingPromptQueued = false;
		AppRatingsPopup component = go.GetComponent<AppRatingsPopup>();
		component.m_OnHideCallback = (Action)Delegate.Combine(component.m_OnHideCallback, new Action(OnAppRatingPopupHidden));
		component.Show();
	}

	private static void OnAppRatingPopupHidden()
	{
		m_onAppRatingPromptHidden?.Invoke();
		m_onAppRatingPromptHidden = null;
	}

	public static float GetSystemTotalMemoryMB()
	{
		return (float)GetSystemTotalMemoryBytes() / 1048576f;
	}

	public static float GetSystemOSSpec()
	{
		return 0f;
	}

	private void CheckVersionInStore()
	{
	}

	private void CheckVersionInStoreListener(string versionCode)
	{
		Log.Downloader.PrintInfo("Version in Store: " + versionCode);
		string[] values = versionCode.Split(',');
		VersionCodeInStore = values[0];
		TelemetryManager.Client().SendVersionCodeInStore(VersionCodeInStore, (values.Length > 1) ? values[1] : "");
	}

	public static int GetMemoryUsage()
	{
		return (int)Profiler.GetTotalAllocatedMemoryLong();
	}

	public static void CreateCrashPlugInLayer(string desc)
	{
	}

	public static void CreateCrashInNativeLayer(string desc)
	{
	}

	public static bool AreMotionEffectsEnabled()
	{
		return true;
	}

	private static bool IsDevice(string deviceModel)
	{
		return false;
	}

	public static string GetSharedKeychainIdentifier()
	{
		return string.Empty;
	}

	public static void ShowNativeAppRatingPopup()
	{
	}

	public static string GetAlpha2CountryCode()
	{
		return "US";
	}

	public static ulong GetSystemTotalMemoryBytes()
	{
		return (ulong)SystemInfo.systemMemorySize;
	}

	public static void SetUpdateCompleted(bool finished)
	{
	}
}
