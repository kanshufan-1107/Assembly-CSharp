using System;
using System.IO;
using Blizzard.T5.Core.Utils;
using Blizzard.T5.Logging;
using UnityEngine;

namespace Hearthstone.Util;

public static class PlatformFilePaths
{
	private static string s_configNameOverride = string.Empty;

	private static string s_optionsNameOverride = string.Empty;

	private static string m_applicationPersistentDataPath = null;

	private static string m_ExternalDataPath = null;

	private static string s_workingDir;

	private static string s_persistentDataPath = Application.persistentDataPath;

	private static string WorkingDir
	{
		get
		{
			if (s_workingDir == null)
			{
				s_workingDir = Directory.GetCurrentDirectory().Replace("\\", "/");
			}
			return s_workingDir;
		}
	}

	public static string ClientConfigName => "client.config";

	private static bool NeedToExtractEssentialBundles => Options.Get().GetBool(Option.EXTRACT_ESSENTIAL_BUNDLES, AndroidDeviceSettings.Get().IsEssentialBundleInApk());

	public static string BasePersistentDataPath
	{
		get
		{
			if (PlatformSettings.RuntimeOS == OSCategory.Mac)
			{
				return string.Format("{0}/Library/Preferences/Blizzard/Hearthstone", Environment.GetEnvironmentVariable("HOME"));
			}
			if (PlatformSettings.RuntimeOS == OSCategory.PC)
			{
				string prefix = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
				prefix = prefix.Replace('\\', '/');
				return $"{prefix}/Blizzard/Hearthstone";
			}
			if (PlatformSettings.RuntimeOS == OSCategory.iOS)
			{
				return s_persistentDataPath;
			}
			if (PlatformSettings.RuntimeOS == OSCategory.Android)
			{
				if (m_applicationPersistentDataPath == null)
				{
					m_applicationPersistentDataPath = AndroidDeviceSettings.Get().applicationStorageFolder;
				}
				return m_applicationPersistentDataPath;
			}
			throw new NotImplementedException("Unknown persistent data path on this platform");
		}
	}

	public static string PublicPersistentDataPath => BasePersistentDataPath;

	public static string InternalPersistentDataPath => $"{BasePersistentDataPath}/Dev";

	public static string OldAgentLogPath => Path.Combine(Log.LogsPath, "AgentLogs");

	public static string AgentLogPath => Path.Combine(LogSystem.Get().Configuration.SessionConfig.LogSessionDirectory, "AgentLogs");

	public static string PersistentDataPath
	{
		get
		{
			string path = null;
			path = ((!HearthstoneApplication.IsInternal()) ? PublicPersistentDataPath : InternalPersistentDataPath);
			if (!Directory.Exists(path))
			{
				try
				{
					Directory.CreateDirectory(path);
				}
				catch (Exception ex)
				{
					Debug.LogError($"FileUtils.PersistentDataPath - Error creating {path}. Exception={ex.Message}");
					Error.AddFatal(FatalErrorReason.CREATE_DATA_FOLDER, "GLOBAL_ERROR_ASSET_CREATE_PERSISTENT_DATA_PATH");
				}
			}
			return path;
		}
	}

	public static string BaseExternalDataPath
	{
		get
		{
			if (PlatformSettings.RuntimeOS == OSCategory.Android)
			{
				return AndroidDeviceSettings.Get().externalStorageFolder;
			}
			return BasePersistentDataPath;
		}
	}

	public static string ExternalDataPath
	{
		get
		{
			if (m_ExternalDataPath == null)
			{
				if (PlatformSettings.RuntimeOS == OSCategory.Android)
				{
					if (HearthstoneApplication.IsInternal())
					{
						m_ExternalDataPath = $"{BaseExternalDataPath}/Dev";
					}
					else
					{
						m_ExternalDataPath = BaseExternalDataPath;
					}
				}
				else
				{
					m_ExternalDataPath = PersistentDataPath;
				}
			}
			return m_ExternalDataPath;
		}
	}

	public static string CachePath
	{
		get
		{
			string path = $"{PersistentDataPath}/Cache";
			if (!Directory.Exists(path))
			{
				try
				{
					Directory.CreateDirectory(path);
				}
				catch (Exception ex)
				{
					Debug.LogError($"FileUtils.CachePath - Error creating {path}. Exception={ex.Message}");
				}
			}
			return path;
		}
	}

	public static string GetAssetPath(string fileName, bool useAssetBundleFolder = true)
	{
		if (PlatformSettings.RuntimeOS == OSCategory.iOS)
		{
			string streamingPath = Application.persistentDataPath + "/" + fileName;
			if (File.Exists(streamingPath))
			{
				return streamingPath;
			}
			string ipaPath = Application.dataPath + "/Raw/" + fileName;
			if (File.Exists(ipaPath))
			{
				if (fileName.EndsWith("dbf.unity3d") && FileUtils.SafeCopy(ipaPath, streamingPath))
				{
					return streamingPath;
				}
				return ipaPath;
			}
			return streamingPath;
		}
		if (PlatformSettings.RuntimeOS == OSCategory.Android)
		{
			string streamingPath2 = (useAssetBundleFolder ? AndroidDeviceSettings.Get().assetBundleFolder : AndroidDeviceSettings.Get().applicationStorageFolder) + "/" + fileName;
			if (useAssetBundleFolder && IsBundleInBinary(Path.GetFileName(fileName)) && !File.Exists(streamingPath2))
			{
				return Path.Combine(ExtractBundleFromAssetPack(fileName), fileName);
			}
			return streamingPath2;
		}
		return fileName;
	}

	private static string ExtractBundleFromAssetPack(string bundlePath)
	{
		string bundleName = Path.GetFileName(bundlePath);
		string apkPath = AndroidDeviceSettings.Get().GetAssetPackPath(bundleName) ?? Application.streamingAssetsPath;
		if (!apkPath.StartsWith("jar:file://") || (!bundleName.StartsWith("essential") && !bundleName.StartsWith("dbf")) || !NeedToExtractEssentialBundles)
		{
			Log.Asset.PrintDebug(bundleName + " is in " + apkPath);
			return apkPath;
		}
		Log.Asset.PrintDebug("Copy " + bundlePath + " to " + AndroidDeviceSettings.Get().assetBundleFolder + ".");
		if (AndroidDeviceSettings.Get().ExtractFromAssetPack(bundlePath, AndroidDeviceSettings.Get().assetBundleFolder))
		{
			Log.Asset.PrintDebug("Extracted: " + bundleName);
			return AndroidDeviceSettings.Get().assetBundleFolder;
		}
		return apkPath;
	}

	public static bool IsBundleInBinary(string bundleName)
	{
		return false;
	}

	public static string CreateLocalFilePath(string relPath, bool useAssetBundleFolder = true)
	{
		return $"{WorkingDir}/{relPath}";
	}

	public static string GetPathForConfigFile(string filename)
	{
		string configPath = GetSavePathForConfigFile(filename);
		if (PlatformSettings.RuntimeOS == OSCategory.iOS)
		{
			if (!File.Exists(configPath))
			{
				configPath = GetAssetPath(filename, useAssetBundleFolder: false);
			}
		}
		else if (PlatformSettings.RuntimeOS == OSCategory.Android && !File.Exists(configPath))
		{
			configPath = $"{BasePersistentDataPath}/{filename}";
			if (!File.Exists(configPath))
			{
				configPath = GetAssetPath(filename, useAssetBundleFolder: false);
			}
		}
		return configPath;
	}

	public static string GetSavePathForConfigFile(string filename)
	{
		if (PlatformSettings.RuntimeOS == OSCategory.iOS)
		{
			return $"{Application.persistentDataPath}/{filename}";
		}
		if (PlatformSettings.RuntimeOS == OSCategory.Android)
		{
			return $"{BaseExternalDataPath}/{filename}";
		}
		return filename;
	}

	public static string GetOptionsFileName()
	{
		return "options.txt";
	}

	public static bool IsOptionsFileOverridden()
	{
		return GetOptionsFileName() != "options.txt";
	}

	public static string GetClientConfigPath()
	{
		return GetPathForConfigFile("client.config");
	}

	public static string GetTokenConfigPath()
	{
		return GetPathForConfigFile("token.config");
	}

	public static string GetEncryptionKeyConfigPath()
	{
		return GetPathForConfigFile("akey.config");
	}

	public static void SetConfigNameOverride(string overrideName)
	{
		s_configNameOverride = overrideName;
	}

	public static void SetOptionsNameOverride(string overrideName)
	{
		s_optionsNameOverride = overrideName;
	}
}
