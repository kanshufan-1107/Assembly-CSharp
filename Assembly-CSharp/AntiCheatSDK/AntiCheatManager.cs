using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using AntiCheatWrapperLib;
using Blizzard.BlizzardErrorMobile;
using Blizzard.T5.Core;
using Blizzard.T5.Jobs;
using Blizzard.T5.Logging;
using Blizzard.T5.Services;
using Blizzard.Telemetry.WTCG.Client;
using Hearthstone.Core;
using Hearthstone.Util;
using UnityEngine;

namespace AntiCheatSDK;

public class AntiCheatManager : IService
{
	private readonly PlatformDependentValue<string> GAMEID_IN_ACSDK = new PlatformDependentValue<string>(PlatformCategory.OS)
	{
		iOS = "ios_blzhs",
		Android = "android_blzhs",
		PC = "blzhs",
		Mac = ""
	};

	private Blizzard.T5.Core.ILogger m_logger;

	private Thread SDKWorkThread { get; set; }

	private string ReportId { get; set; }

	private bool IsCalledPreInit { get; set; }

	private bool IsCalledSetup { get; set; }

	private bool RetCodeSetExtraParams { get; set; }

	private int RetCodeSetupSDK { get; set; }

	private int RetCodeCallSDK { get; set; }

	private AntiCheatTimer CallSDKTimer { get; set; }

	public IEnumerator<IAsyncJobResult> Initialize(ServiceLocator serviceLocator)
	{
		InitLogger();
		if (PlatformSettings.RuntimeOS != OSCategory.Mac)
		{
			CallSDKTimer = new AntiCheatTimer(m_logger, delegate
			{
				TryCallSDK("hb");
			});
			LoginManager.Get().OnLoginCompleted += OnLoginComplete;
		}
		yield break;
	}

	public Type[] GetDependencies()
	{
		return new Type[2]
		{
			typeof(LoginManager),
			typeof(NetCache)
		};
	}

	public void Shutdown()
	{
		if (PlatformSettings.RuntimeOS == OSCategory.Mac)
		{
			return;
		}
		LoginManager.Get().OnLoginCompleted -= OnLoginComplete;
		if (IsCalledPreInit)
		{
			CallSDKTimer.Dispose();
			if (SDKWorkThread != null && SDKWorkThread.IsAlive)
			{
				SDKWorkThread.Abort();
			}
			Wrapper.d10e6b030e22d52c1d10552c0d92669();
			Wrapper.b8e609c7f5696e64cb190c814();
		}
	}

	public void TryCallSDK(string scriptId)
	{
		InnerSDKMethodCall(CallInterfaceCallSDK, scriptId);
	}

	public void ClearExtraParams()
	{
		try
		{
			if (IsCalledPreInit)
			{
				Wrapper.d10e6b030e22d52c1d10552c0d92669();
			}
		}
		catch (Exception ex)
		{
			m_logger.Log(Blizzard.T5.Core.LogLevel.Error, "Failed to call ClearExtracParams: {0}", ex.Message);
			ExceptionReporter.Get()?.ReportCaughtException(ex);
		}
	}

	private void InitLogger()
	{
		if (m_logger == null)
		{
			m_logger = LogSystem.Get().CreateLogger("AntiCheat");
		}
	}

	private void OnLoginComplete()
	{
		m_logger.Log(Blizzard.T5.Core.LogLevel.Debug, "Run AntiCheat feature at login completion.");
		if (!RegionUtils.IsCNLegalRegion || Application.isEditor)
		{
			m_logger.Log(Blizzard.T5.Core.LogLevel.Debug, "AntiCheat feature is disabled.");
			return;
		}
		if (!IsCalledPreInit)
		{
			Wrapper.cd527f3e733e2b7cc591f2ff(Application.dataPath);
			m_logger.Log(Blizzard.T5.Core.LogLevel.Debug, "preInit called");
			if (Wrapper.ExceptionExists)
			{
				m_logger.Log(Blizzard.T5.Core.LogLevel.Debug, "AntiCheat exception.");
			}
			ReportId = Wrapper.c46af54d51691b();
			m_logger.Log(Blizzard.T5.Core.LogLevel.Debug, "ReportId = " + ReportId);
			IsCalledPreInit = true;
		}
		if (IsCalledPreInit)
		{
			WriteUserInfo();
			if (!IsCalledSetup)
			{
				InnerSDKMethodCall(CallInterfaceSetupSDK);
				CallSDKTimer.Start();
				IsCalledSetup = true;
			}
		}
	}

	private void WriteUserInfo()
	{
		Dictionary<string, string> dicInfo = new Dictionary<string, string>();
		dicInfo.Add("account_id", $"{BnetUtils.TryGetGameAccountId().GetValueOrDefault()}");
		dicInfo.Add("bnet_id", $"{BnetUtils.TryGetBnetAccountId().GetValueOrDefault()}");
		RetCodeSetExtraParams = Wrapper.d7a4dbab18cc6f1ddb(dicInfo);
		if (!RetCodeSetExtraParams)
		{
			m_logger.Log(Blizzard.T5.Core.LogLevel.Error, "SetExtraParams failed");
		}
	}

	private void InnerSDKMethodCall(Action<string> handler, string args = null)
	{
		if (IsCalledPreInit)
		{
			if (SDKWorkThread != null && SDKWorkThread.IsAlive)
			{
				m_logger.Log(Blizzard.T5.Core.LogLevel.Warning, "Cancel the previous SDK method method.");
				SDKWorkThread.Abort();
			}
			SDKWorkThread = new Thread(delegate
			{
				handler(args);
			});
			SDKWorkThread.Start();
		}
	}

	private void CallInterfaceSetupSDK(string scriptId = "")
	{
		string[] messages = new string[1] { "" };
		for (int i = 0; i < 10; i++)
		{
			RetCodeSetupSDK = Wrapper.c2a0ebc18a7097f0b7e(GAMEID_IN_ACSDK, "zo6w8gik7sko8kpc", "acsdk-hs.gameyw.netease.com", "", out messages);
			if (RetCodeSetupSDK == 200)
			{
				m_logger.Log(Blizzard.T5.Core.LogLevel.Debug, "SetupSDK succeeded.");
				break;
			}
		}
		if (RetCodeSetupSDK != 200)
		{
			m_logger.Log(Blizzard.T5.Core.LogLevel.Error, $"SetupSDK failed: {RetCodeSetupSDK}");
			string[] array = messages;
			foreach (string message in array)
			{
				m_logger.Log(Blizzard.T5.Core.LogLevel.Error, "SetupSDK msg: " + message);
			}
		}
		Processor.ScheduleCallback(0f, realTime: false, delegate
		{
			TelemetryManager.Client().SendACSdkResult(ReportId, ACSdkResult.CommandType.SetupSDK, scriptId, RetCodeSetExtraParams, RetCodeSetupSDK, 0, messages.ToList());
			if (RetCodeSetupSDK != 200)
			{
				Error.AddFatal(FatalErrorReason.UNKNOWN, "GLOBAL_ERROR_GAME_DENIED");
			}
		});
	}

	private void CallInterfaceCallSDK(string scriptId)
	{
		RetCodeCallSDK = Wrapper.b29709adc0b1e61d54f5e(scriptId, "", out var messages);
		if (RetCodeCallSDK != 200)
		{
			m_logger.Log(Blizzard.T5.Core.LogLevel.Error, $"CallSDK failed: {RetCodeCallSDK}");
			string[] array = messages;
			foreach (string message in array)
			{
				m_logger.Log(Blizzard.T5.Core.LogLevel.Error, "CallSDK msg: " + message);
			}
		}
		Processor.ScheduleCallback(0f, realTime: false, delegate
		{
			TelemetryManager.Client().SendACSdkResult(ReportId, ACSdkResult.CommandType.CallSDK, scriptId, RetCodeSetExtraParams, RetCodeSetupSDK, RetCodeCallSDK, messages.ToList());
		});
	}
}
