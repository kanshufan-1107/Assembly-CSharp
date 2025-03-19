using System;
using System.Collections.Generic;
using System.IO;
using Assets;
using Hearthstone;
using Hearthstone.Streaming;
using UnityEngine;

public class PreloadScreen : MonoBehaviour
{
	private IGameDownloadManager DownloadManager
	{
		get
		{
			return GameDownloadManagerProvider.Get();
		}
		set
		{
			throw new NotImplementedException();
		}
	}

	private void Start()
	{
		Screen.sleepTimeout = -1;
	}

	private int GetInstalledLocaleCount()
	{
		int installedLocales = 0;
		List<string> detectedLocales = new List<string>();
		foreach (Locale loc in Enum.GetValues(typeof(Locale)))
		{
			if (loc != Locale.UNKNOWN && File.Exists(GameStrings.GetAssetPath(loc, Global.GameStringCategory.GLOBAL.ToString() + ".txt")))
			{
				installedLocales++;
				detectedLocales.Add(loc.ToString());
			}
		}
		Options.Get().SetString(Option.INSTALLED_LOCALES, string.Join(",", detectedLocales));
		return installedLocales;
	}

	private void Update()
	{
		if (DownloadManager != null && SplashScreen.Get() != null && SplashScreen.Get().gameObject.activeInHierarchy)
		{
			if (PlatformSettings.IsMobileRuntimeOS)
			{
				GetInstalledLocaleCount();
			}
			HearthstoneApplication.SendStartupTimeTelemetry("GameDownloadManager.EnteringSplashScreen");
			Log.Downloader.Print("killing preloadscreen");
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}
}
