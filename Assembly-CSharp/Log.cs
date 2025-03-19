using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Blizzard.T5.Configuration;
using Blizzard.T5.Logging;
using Hearthstone;
using Hearthstone.Core;
using Hearthstone.Util;
using UnityEngine;

public static class Log
{
	private static readonly List<string> s_legacyHearthstoneLoggers = new List<string>
	{
		"All", "AchievementManager", "Achievements", "AdTracking", "Adventures", "Arena", "Asset", "AsyncLoading", "BattlegroundsAuthoring", "BattleNet",
		"BIReport", "Box", "BreakingNews", "BugReporter", "CardbackMgr", "ChangedCards", "ClientRequestManager", "CloudStorage", "CollectionDeckBox", "CollectionManager",
		"CoinManager", "ConfigFile", "ContentConnect", "Crafting", "CRM", "Dbf", "DeckHelper", "DeckRuleset", "Decks", "DeckTray",
		"DeepLink", "DelayedReporter", "DeviceEmulation", "Downloader", "EndOfGame", "ErrorReporter", "EventTable", "EventTiming", "ExceptionReporter", "FaceDownCard",
		"FlowPerformance", "Font", "FullScreenFX", "GameMgr", "Gameplay", "Graphics", "Hand", "InGameBrowser", "InGameMessage", "InnKeepersSpecial",
		"Jobs", "Lettuce", "LoadingScreen", "Login", "MinSpecManager", "MissingAssets", "MobileCallback", "MulliganManager", "NarrativeManager", "Net",
		"Notifications", "Offline", "Options", "Packet", "Party", "Performance", "PlayErrors", "PlayMaker", "PlayModeInvestigation", "Power",
		"Presence", "Privacy", "PVPDR", "RAF", "ReturningPlayer", "Replay", "Reset", "RewardBox", "Services", "SmartDiscover",
		"Spells", "Sound", "Spectator", "Store", "Tag", "TavernBrawl", "Telemetry", "TemporaryAccount", "UberText", "UIFramework",
		"UIStatus", "UserAttention", "W8Touch", "Zone"
	};

	private static readonly LogInfo[] DEFAULT_LOG_INFOS = new LogInfo[5]
	{
		new LogInfo
		{
			m_name = "Jobs",
			m_consolePrinting = true,
			m_minLevel = LogLevel.Error
		},
		new LogInfo
		{
			m_name = "Downloader",
			m_filePrinting = true,
			m_consolePrinting = true,
			m_minLevel = LogLevel.Info
		},
		new LogInfo
		{
			m_name = "Login",
			m_filePrinting = true,
			m_consolePrinting = true,
			m_minLevel = LogLevel.Info
		},
		new LogInfo
		{
			m_name = "ExceptionReporter",
			m_filePrinting = true,
			m_minLevel = LogLevel.Info
		},
		new LogInfo
		{
			m_name = "Offline",
			m_filePrinting = true,
			m_minLevel = LogLevel.Info
		}
	};

	public static Logger All => GetLoggerFromSystem("All");

	public static Logger Achievements => GetLoggerFromSystem("Achievements");

	public static Logger AdTracking => GetLoggerFromSystem("AdTracking");

	public static Logger Adventures => GetLoggerFromSystem("Adventures");

	public static Logger Arena => GetLoggerFromSystem("Arena");

	public static Logger Asset => GetLoggerFromSystem("Asset");

	public static Logger BattlegroundsAuthoring => GetLoggerFromSystem("BattlegroundsAuthoring");

	public static Logger BattleNet => GetLoggerFromSystem("BattleNet");

	public static Logger Box => GetLoggerFromSystem("Box");

	public static Logger BreakingNews => GetLoggerFromSystem("BreakingNews");

	public static Logger CardbackMgr => GetLoggerFromSystem("CardbackMgr");

	public static Logger CollectionDeckBox => GetLoggerFromSystem("CollectionDeckBox");

	public static Logger CollectionManager => GetLoggerFromSystem("CollectionManager");

	public static Logger CoinManager => GetLoggerFromSystem("CoinManager");

	public static Logger ConfigFile => GetLoggerFromSystem("ConfigFile");

	public static Logger ContentConnect => GetLoggerFromSystem("ContentConnect");

	public static Logger CosmeticPreview => GetLoggerFromSystem("CosmeticPreview");

	public static Logger Crafting => GetLoggerFromSystem("Crafting");

	public static Logger CRM => GetLoggerFromSystem("CRM");

	public static Logger Dbf => GetLoggerFromSystem("Dbf");

	public static Logger DeckHelper => GetLoggerFromSystem("DeckHelper");

	public static Logger DeckRuleset => GetLoggerFromSystem("DeckRuleset");

	public static Logger Decks => GetLoggerFromSystem("Decks");

	public static Logger DeckTray => GetLoggerFromSystem("DeckTray");

	public static Logger DeepLink => GetLoggerFromSystem("DeepLink");

	public static Logger DeviceEmulation => GetLoggerFromSystem("DeviceEmulation");

	public static Logger Downloader => GetLoggerFromSystem("Downloader");

	public static Logger EndOfGame => GetLoggerFromSystem("EndOfGame");

	public static Logger ErrorReporter => GetLoggerFromSystem("ErrorReporter");

	public static Logger EventTable => GetLoggerFromSystem("EventTable");

	public static Logger EventTiming => GetLoggerFromSystem("EventTiming");

	public static Logger ExceptionReporter => GetLoggerFromSystem("ExceptionReporter");

	public static Logger FaceDownCard => GetLoggerFromSystem("FaceDownCard");

	public static Logger FlowPerformance => GetLoggerFromSystem("FlowPerformance");

	public static Logger Font => GetLoggerFromSystem("Font");

	public static Logger FullScreenFX => GetLoggerFromSystem("FullScreenFX");

	public static Logger GameMgr => GetLoggerFromSystem("GameMgr");

	public static Logger Gameplay => GetLoggerFromSystem("Gameplay");

	public static Logger Graphics => GetLoggerFromSystem("Graphics");

	public static Logger Hand => GetLoggerFromSystem("Hand");

	public static Logger InGameBrowser => GetLoggerFromSystem("InGameBrowser");

	public static Logger InGameMessage => GetLoggerFromSystem("InGameMessage");

	public static Logger InnKeepersSpecial => GetLoggerFromSystem("InnKeepersSpecial");

	public static Logger Jobs => GetLoggerFromSystem("Jobs");

	public static Logger Lettuce => GetLoggerFromSystem("Lettuce");

	public static Logger LoadingScreen => GetLoggerFromSystem("LoadingScreen");

	public static Logger Login => GetLoggerFromSystem("Login");

	public static Logger MinSpecManager => GetLoggerFromSystem("MinSpecManager");

	public static Logger MissingAssets => GetLoggerFromSystem("MissingAssets");

	public static Logger MobileCallback => GetLoggerFromSystem("MobileCallback");

	public static Logger MulliganManager => GetLoggerFromSystem("MulliganManager");

	public static Logger NarrativeManager => GetLoggerFromSystem("NarrativeManager");

	public static Logger Net => GetLoggerFromSystem("Net");

	public static Logger Notifications => GetLoggerFromSystem("Notifications");

	public static Logger Offline => GetLoggerFromSystem("Offline");

	public static Logger Options => GetLoggerFromSystem("Options");

	public static Logger Party => GetLoggerFromSystem("Party");

	public static Logger Performance => GetLoggerFromSystem("Performance");

	public static Logger PlayErrors => GetLoggerFromSystem("PlayErrors");

	public static Logger PlayMaker => GetLoggerFromSystem("PlayMaker");

	public static Logger PlayModeInvestigation => GetLoggerFromSystem("PlayModeInvestigation");

	public static Logger Power => GetLoggerFromSystem("Power");

	public static Logger Presence => GetLoggerFromSystem("Presence");

	public static Logger Privacy => GetLoggerFromSystem("Privacy");

	public static Logger RAF => GetLoggerFromSystem("RAF");

	public static Logger ReturningPlayer => GetLoggerFromSystem("ReturningPlayer");

	public static Logger Reset => GetLoggerFromSystem("Reset");

	public static Logger RewardBox => GetLoggerFromSystem("RewardBox");

	public static Logger Services => GetLoggerFromSystem("Services");

	public static Logger SmartDiscover => GetLoggerFromSystem("SmartDiscover");

	public static Logger Spells => GetLoggerFromSystem("Spells");

	public static Logger Sound => GetLoggerFromSystem("Sound");

	public static Logger Store => GetLoggerFromSystem("Store");

	public static Logger TavernBrawl => GetLoggerFromSystem("TavernBrawl");

	public static Logger Telemetry => GetLoggerFromSystem("Telemetry");

	public static Logger TemporaryAccount => GetLoggerFromSystem("TemporaryAccount");

	public static Logger TextUtils => GetLoggerFromSystem("TextUtils");

	public static Logger UberText => GetLoggerFromSystem("UberText");

	public static Logger UIFramework => GetLoggerFromSystem("UIFramework");

	public static Logger UIStatus => GetLoggerFromSystem("UIStatus");

	public static Logger UserAttention => GetLoggerFromSystem("UserAttention");

	public static Logger W8Touch => GetLoggerFromSystem("W8Touch");

	public static Logger Zone => GetLoggerFromSystem("Zone");

	public static string ConfigPath
	{
		get
		{
			string configPath = null;
			if (1 == 3)
			{
				configPath = Application.persistentDataPath + "/log.config";
				if (!File.Exists(configPath))
				{
					configPath = PlatformFilePaths.GetAssetPath("log.config", useAssetBundleFolder: false);
				}
			}
			else
			{
				configPath = PlatformFilePaths.ExternalDataPath + "/log.config";
				if (!File.Exists(configPath))
				{
					configPath = PlatformFilePaths.PersistentDataPath + "/log.config";
					if (!File.Exists(configPath))
					{
						configPath = PlatformFilePaths.GetAssetPath("log.config", useAssetBundleFolder: false);
					}
				}
			}
			return configPath;
		}
	}

	public static string LogsPath
	{
		get
		{
			string directoryPath = null;
			if (PlatformSettings.RuntimeOS == OSCategory.Android)
			{
				return PlatformFilePaths.ExternalDataPath + "/Logs";
			}
			if (PlatformSettings.RuntimeOS == OSCategory.iOS)
			{
				return PlatformFilePaths.PersistentDataPath + "/Logs";
			}
			if (PlatformSettings.RuntimeOS == OSCategory.Mac && !Application.isEditor)
			{
				return Application.dataPath + "/../../Logs";
			}
			return Application.dataPath + "/../Logs";
		}
	}

	private static Logger GetLoggerFromSystem(string name)
	{
		Initialize();
		return LogSystem.Get().GetFullLogger(name);
	}

	public static IEnumerable<string> GetEnabledLogNames()
	{
		IEnumerable<KeyValuePair<string, Logger>> allLoggers = LogSystem.Get().GetAllLoggers();
		List<string> enabledLogs = new List<string>(allLoggers.Count());
		foreach (KeyValuePair<string, Logger> item in allLoggers)
		{
			Logger logger = item.Value;
			if (LogSystem.Get().GetLogInfo(logger.GetName()).m_filePrinting)
			{
				enabledLogs.Add(logger.GetName());
			}
		}
		return enabledLogs;
	}

	public static void SetStandardLogInfo(string logName, LogLevel level = LogLevel.Info)
	{
		LogInfo info = LogSystem.Get().GetLogInfo(logName);
		if (info == null)
		{
			info = new LogInfo
			{
				m_name = logName
			};
		}
		info.m_filePrinting = true;
		info.m_screenPrinting = true;
		info.m_minLevel = level;
		LogSystem.Get().SetLogInfo(logName, info);
	}

	public static void Initialize()
	{
		if (!LogSystem.Get().IsConfigured)
		{
			LogConfig logConfig = BuildRuntimeLogConfig();
			PopulateLogSessionConfigOptions(logConfig.SessionConfig);
			ConfigureLogSystem(logConfig);
		}
	}

	private static LogConfig BuildRuntimeLogConfig()
	{
		int numberLogSessionsToRetain = 5;
		LogSessionConfig sessionConfig = new LogSessionConfig(LogsPath, "Hearthstone", "Hearthstone_")
		{
			MaxLogSessionsRetained = numberLogSessionsToRetain
		};
		return new LogConfig
		{
			Printers = new List<ILogPrinter>
			{
				new StandardFileLogPrinter(sessionConfig, delegate(Action onMainThreadFunc)
				{
					Processor.ScheduleCallback(0f, realTime: false, delegate
					{
						onMainThreadFunc();
					});
				}, () => HearthstoneApplication.IsMainThread),
				new UnityConsoleLogPrinter(),
				new ScreenLogPrinter()
			},
			IsMainThreadFunc = () => HearthstoneApplication.IsMainThread,
			LogInfoConfigDirectory = ConfigPath,
			SessionConfig = sessionConfig
		};
	}

	private static void ConfigureLogSystem(LogConfig config)
	{
		LogSystem.Get().SetConfiguration(config);
		LogInfo[] dEFAULT_LOG_INFOS = DEFAULT_LOG_INFOS;
		foreach (LogInfo info in dEFAULT_LOG_INFOS)
		{
			if (LogSystem.Get().GetLogInfo(info.m_name) == null)
			{
				LogSystem.Get().SetLogInfo(info.m_name, info);
			}
		}
		foreach (string legacyLogger in s_legacyHearthstoneLoggers)
		{
			LogSystem.Get().CreateFullLogger(legacyLogger);
		}
		LogInfo logInfo = LogSystem.Get().GetLogInfo("BattleNet");
		if (logInfo != null && logInfo.m_truncatePos == 0)
		{
			logInfo.m_truncatePos = 300;
			LogSystem.Get().SetLogInfo("BattleNet", logInfo);
		}
	}

	private static void PopulateLogSessionConfigOptions(LogSessionConfig sessionConfig)
	{
		ConfigFile config = new ConfigFile();
		if (config.FullLoad(PlatformFilePaths.GetClientConfigPath()))
		{
			sessionConfig.MaxFileSizeKilobytes = config.Get("Log.FileSizeLimit.Int", 10000);
		}
	}
}
