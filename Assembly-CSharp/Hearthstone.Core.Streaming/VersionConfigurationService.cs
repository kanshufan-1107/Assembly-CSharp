using System;
using System.Collections.Generic;
using System.IO;
using Blizzard.T5.Configuration;
using Blizzard.T5.Core;
using Blizzard.T5.Core.Utils;
using Blizzard.T5.Jobs;
using Blizzard.T5.Services;
using Hearthstone.Core.Deeplinking;
using Hearthstone.Util;

namespace Hearthstone.Core.Streaming;

public class VersionConfigurationService : IService
{
	private string m_tokenFromFile = string.Empty;

	private VersionPipeline m_pipelineFromFile;

	private string TokenOverride { get; set; }

	private VersionPipeline PipelineOverride { get; set; }

	private string ProductName { get; set; }

	private string EncrpytionKey { get; set; }

	public IEnumerator<IAsyncJobResult> Initialize(ServiceLocator serviceLocator)
	{
		if (!IsEnabledForCurrentPlatform())
		{
			Log.Downloader.PrintDebug("Not using VersionConfig, not used for platform");
			yield break;
		}
		Log.Downloader.PrintInfo("Initializing VersionConfigurationService");
		Map<string, string> args = DeeplinkService.Get()?.GetStartupDeepLinkArgs();
		string argsStr = ((args != null) ? string.Join(",", args) : "null");
		Log.Downloader.PrintDebug("[VersionConfigurationService] Using startup arguments: " + argsStr);
		if (args == null)
		{
			Log.Downloader.PrintInfo("Failed to pull deeplink args for VersionConfigurationService");
		}
		else
		{
			if (args.ContainsKey("pipeline"))
			{
				string pipelineString = args["pipeline"];
				if (EnumUtils.TryGetEnum<VersionPipeline>(pipelineString, out var pipeline))
				{
					Log.Downloader.PrintInfo("Setting pipeline override from deeplink args {0}", pipelineString);
					PipelineOverride = pipeline;
				}
				else
				{
					Log.Downloader.PrintInfo("Pipeline override deeplink arg was found but it was invalid: {0}", pipelineString);
				}
			}
			if (args.ContainsKey("version_token"))
			{
				string tokenString = args["version_token"];
				if (tokenString.Length == 36)
				{
					Log.Downloader.PrintInfo("Setting token override from deeplink args {0}", tokenString);
					TokenOverride = tokenString;
				}
				else
				{
					Log.Downloader.PrintInfo("Token override deeplink arg was found but it was invalid: {0}", tokenString);
				}
			}
		}
		yield return new JobDefinition("VersionConfigurationService.LoadTokenConfigFile", Job_LoadConfigFiles());
	}

	public Type[] GetDependencies()
	{
		return new Type[0];
	}

	public void Shutdown()
	{
	}

	public string GetClientToken()
	{
		if (!string.IsNullOrEmpty(TokenOverride))
		{
			return TokenOverride;
		}
		return m_tokenFromFile;
	}

	public VersionPipeline GetPipeline()
	{
		if (PipelineOverride != 0)
		{
			return PipelineOverride;
		}
		return m_pipelineFromFile;
	}

	public string GetProductName()
	{
		if (!string.IsNullOrEmpty(ProductName))
		{
			return ProductName;
		}
		switch (GetPipeline())
		{
		case VersionPipeline.LIVERC:
			ProductName = "hsrc";
			break;
		case VersionPipeline.LIVEDEV:
			ProductName = "hsdev";
			break;
		default:
			ProductName = "hsb";
			break;
		}
		return ProductName;
	}

	public string GetEncrpytionKey()
	{
		return EncrpytionKey;
	}

	public bool IsInternalVersionService()
	{
		VersionPipeline pipeline = GetPipeline();
		if (pipeline == VersionPipeline.LIVE || pipeline == VersionPipeline.LIVERC || pipeline == VersionPipeline.LIVEDEV)
		{
			return Vars.Key("Mobile.LiveOverride").GetBool(def: false);
		}
		return pipeline != VersionPipeline.UNKNOWN;
	}

	public static bool IsEnabledForCurrentPlatform()
	{
		return UpdateUtils.AreUpdatesEnabledForCurrentPlatform;
	}

	private void ProcessingTokenConfig(string tokenData)
	{
		if (string.IsNullOrEmpty(tokenData))
		{
			Log.Downloader.PrintInfo("Token information was missing from token.config!");
			return;
		}
		VersionPipeline pipeline = VersionPipeline.UNKNOWN;
		string token = string.Empty;
		if (!TryParseTokenString(tokenData, out pipeline, out token))
		{
			Log.Downloader.PrintError("Could not parse token string from token.config: {0}", tokenData);
		}
		Log.Downloader.PrintInfo("Succesfully loaded information from token.confg. {0}:{1}", EnumUtils.GetString(pipeline), token);
		m_pipelineFromFile = pipeline;
		m_tokenFromFile = token;
	}

	private void ProcessingEncryptionKeyConfig(string keyData)
	{
		if (string.IsNullOrEmpty(keyData))
		{
			Log.Downloader.PrintDebug("Key information was missing!");
		}
		else
		{
			EncrpytionKey = keyData;
		}
	}

	private IEnumerator<IAsyncJobResult> Job_LoadConfigFiles()
	{
		ClearLoadedFileValues();
		if (!IsEnabledForCurrentPlatform())
		{
			Log.Downloader.PrintError("Attempted to load token.config on a platform that does not support it");
			yield break;
		}
		ReadDataFromConfigFile(PlatformFilePaths.GetTokenConfigPath(), ProcessingTokenConfig);
		if (File.Exists(PlatformFilePaths.GetEncryptionKeyConfigPath()))
		{
			ReadDataFromConfigFile(PlatformFilePaths.GetEncryptionKeyConfigPath(), ProcessingEncryptionKeyConfig);
		}
	}

	private static bool TryParseTokenString(string tokenString, out VersionPipeline pipeline, out string token)
	{
		pipeline = VersionPipeline.UNKNOWN;
		token = string.Empty;
		string[] tokenParts = tokenString.Split(':');
		if (tokenParts.Length != 2)
		{
			Log.Downloader.PrintError("Malformed token loaded from token.config: {0}", tokenString);
			return false;
		}
		string pipelineString = tokenParts[0];
		if (!EnumUtils.TryGetEnum<VersionPipeline>(pipelineString, out pipeline))
		{
			Log.Downloader.PrintError("Failed to parse pipeline enum: {0}", pipelineString);
			return false;
		}
		token = tokenParts[1];
		if (token.Length != 36)
		{
			Log.Downloader.PrintError("Unexpected length of token guid {0}", token);
			return false;
		}
		return true;
	}

	private void ClearLoadedFileValues()
	{
		m_tokenFromFile = string.Empty;
		m_pipelineFromFile = VersionPipeline.UNKNOWN;
	}

	private static void ReadDataFromConfigFile(string filePath, Action<string> callback)
	{
		if (!File.Exists(filePath))
		{
			Log.Downloader.PrintInfo("Config file does not exist at path {0}", filePath);
			return;
		}
		try
		{
			string tokenData = null;
			using (StreamReader tokenFileStream = File.OpenText(filePath))
			{
				tokenData = tokenFileStream.ReadLine();
				if (!tokenFileStream.EndOfStream)
				{
					Log.Downloader.PrintWarning("Config file had more lines than expected");
				}
			}
			callback(tokenData);
		}
		catch (Exception ex)
		{
			Log.Downloader.PrintError("Could not read Config file({0}): {1}", filePath, ex.ToString());
		}
	}
}
