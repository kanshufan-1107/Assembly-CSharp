using System;
using System.Text;
using Blizzard.T5.Configuration;
using Blizzard.T5.Core;
using UnityEngine;

public class AndroidDeviceSettings
{
	private static AndroidDeviceSettings s_instance;

	private string m_bestTexture = "";

	public bool m_determineSDCard;

	public string m_deviceModel = string.Empty;

	public int densityDpi = 300;

	public bool isExtraLarge = true;

	public bool isTablet = true;

	public string applicationStorageFolder;

	public string assetBundleFolder;

	public string externalStorageFolder;

	public string m_HSStore;

	public string m_HSPackageName;

	public int m_AndroidSDKVersion;

	public string InstalledTexture
	{
		get
		{
			if (!string.IsNullOrEmpty(m_bestTexture))
			{
				return m_bestTexture;
			}
			m_bestTexture = Vars.Key("Mobile.Texture").GetStr("");
			if (!string.IsNullOrEmpty(m_bestTexture))
			{
				Log.Downloader.PrintInfo("m_bestTexture is already set to " + m_bestTexture);
				return m_bestTexture;
			}
			m_bestTexture = "astc";
			return m_bestTexture;
		}
	}

	private AndroidDeviceSettings()
	{
		_ = Application.isEditor;
	}

	public string GetAssetPackPath(string assetBundleName)
	{
		return null;
	}

	public bool ExtractFromAssetPack(string assetBundlePath, string outDir)
	{
		return false;
	}

	public void AskForSDCard()
	{
		m_determineSDCard = true;
	}

	public bool IsCurrentTextureFormatSupported()
	{
		bool supported = SystemInfo.SupportsTextureFormat(new Map<string, TextureFormat>
		{
			{
				"",
				TextureFormat.ARGB32
			},
			{
				"etc1",
				TextureFormat.ETC_RGB4
			},
			{
				"etc2",
				TextureFormat.ETC2_RGBA8
			},
			{
				"astc",
				TextureFormat.ASTC_12x12
			}
		}[InstalledTexture]);
		Debug.Log("Checking whether texture format of build (" + InstalledTexture + ") is supported? " + supported);
		StringBuilder supportedFormats = new StringBuilder();
		supportedFormats.Append("All supported texture formats: ");
		foreach (TextureFormat format in Enum.GetValues(typeof(TextureFormat)))
		{
			try
			{
				if (SystemInfo.SupportsTextureFormat(format))
				{
					supportedFormats.Append(format.ToString() + ", ");
				}
			}
			catch (ArgumentException)
			{
			}
		}
		Log.Graphics.Print(supportedFormats.ToString());
		return supported;
	}

	public AndroidStore GetAndroidStore()
	{
		return AndroidStore.NONE;
	}

	public bool IsNonStoreAppAllowed()
	{
		return false;
	}

	public bool AllowUnknownApps()
	{
		return false;
	}

	public bool IsEssentialBundleInApk()
	{
		return false;
	}

	public void TriggerUnknownSources(string responseFuncName)
	{
	}

	public void ProcessInstallAPK(string apkPath, string installAPKFuncName)
	{
	}

	public void OpenAppStore(string deeplink, string url)
	{
	}

	public void DeleteOldNotificationChannels()
	{
	}

	public static AndroidDeviceSettings Get()
	{
		if (s_instance == null)
		{
			s_instance = new AndroidDeviceSettings();
		}
		return s_instance;
	}
}
