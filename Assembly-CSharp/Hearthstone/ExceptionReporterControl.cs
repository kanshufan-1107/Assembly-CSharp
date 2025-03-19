using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Blizzard.BlizzardErrorMobile;
using Blizzard.T5.Configuration;
using Blizzard.T5.Logging;
using Hearthstone.Util;
using UnityEngine;

namespace Hearthstone;

public class ExceptionReporterControl
{
	private class CrashLogger : IExceptionLogger
	{
		public void LogDebug(string format, params object[] args)
		{
			Log.ExceptionReporter.PrintDebug(format, args);
		}

		public void LogInfo(string format, params object[] args)
		{
			Log.ExceptionReporter.PrintInfo(format, args);
		}

		public void LogWarning(string format, params object[] args)
		{
			Log.ExceptionReporter.PrintWarning(format, args);
		}

		public void LogError(string format, params object[] args)
		{
			Log.ExceptionReporter.PrintError(format, args);
		}
	}

	private static ExceptionReporterControl s_instance;

	private bool m_ANRMonitorOn;

	private CrashLogger m_logger = new CrashLogger();

	public bool IsEnabledT5MobileReport { get; set; } = true;

	public bool IsRestrictedReport
	{
		get
		{
			return ExceptionReporter.Get().GetSettings().m_cnRegion;
		}
		set
		{
			ExceptionReporter.Get().GetSettings().m_cnRegion = value;
		}
	}

	public static ExceptionReporterControl Get()
	{
		if (s_instance == null)
		{
			s_instance = new ExceptionReporterControl();
		}
		return s_instance;
	}

	public void ExceptionReportInitialize()
	{
		ExceptionReporter.Get().Initialize(PlatformFilePaths.PersistentDataPath, m_logger, HearthstoneApplication.Get());
		ExceptionReporter.Get().IsInDebugMode = Options.Get().GetBool(Option.DELAYED_REPORTER_STOP);
		ExceptionReporter.Get().SendExceptions = Vars.Key("Application.SendExceptions").GetBool(def: true);
		ExceptionReporter.Get().SendAsserts = Vars.Key("Application.SendAsserts").GetBool(def: false);
		ExceptionReporter.Get().SendErrors = Vars.Key("Application.SendErrors").GetBool(def: false);
		if (!string.IsNullOrEmpty(""))
		{
			ExceptionReporter.Get().SendExceptions = false;
		}
		ExceptionSettings settings = new ExceptionSettings();
		settings.m_projectID = 70;
		settings.m_moduleName = "Hearthstone Client";
		settings.m_version = "32.0";
		settings.m_branchName = Network.BranchName;
		settings.m_buildNumber = 217964;
		settings.m_locale = Localization.GetLocaleName();
		settings.m_jiraProjectName = "HSTN";
		settings.m_jiraComponent = "T5 Needs Triage";
		settings.m_jiraVersion = "32.0 Patch";
		settings.m_cnRegion = RegionUtils.IsCNLegalRegion;
		settings.m_debugModules = "Branch=" + Network.BranchName + ",CN=" + (RegionUtils.IsCNLegalRegion ? "true" : "false") + ",Arch=" + SystemInfo.processorType;
		settings.m_logLineLimits[ExceptionSettings.ReportType.BUG] = -1;
		if (HearthstoneApplication.IsInternal())
		{
			settings.m_logLineLimits[ExceptionSettings.ReportType.EXCEPTION] = 0;
		}
		else
		{
			settings.m_logLineLimits[ExceptionSettings.ReportType.EXCEPTION] = 1000;
		}
		settings.m_logLineLimits[ExceptionSettings.ReportType.CAUGHT_EXCEPTION] = settings.m_logLineLimits[ExceptionSettings.ReportType.EXCEPTION];
		settings.m_logLineLimits[ExceptionSettings.ReportType.CRASH] = settings.m_logLineLimits[ExceptionSettings.ReportType.EXCEPTION];
		settings.m_logPathsCallback = GetLogPaths;
		settings.m_attachableFilesCallback = GetAttachableFiles;
		settings.m_additionalInfoCallback = GetAdditionalInfo;
		settings.m_readFileMethodCallback = ReadLogFileSharing;
		ExceptionReporter.Get().BeforeZipping += FlushAllLogs;
		ExceptionReporter.Get().SetSettings(settings);
		HearthstoneApplication.Get().Resetting += ExceptionReporter.Get().ClearExceptionHashes;
	}

	public void Pause(bool paused)
	{
		ExceptionReporter.Get().SendExceptions = !paused && Vars.Key("Application.SendExceptions").GetBool(def: true);
		ExceptionReporter.Get().SendAsserts = !paused && Vars.Key("Application.SendAsserts").GetBool(def: false);
		ExceptionReporter.Get().SendErrors = !paused && Vars.Key("Application.SendErrors").GetBool(def: false);
	}

	public void ControlANRMonitor(bool on)
	{
		if (PlatformSettings.IsMobileRuntimeOS && (on ^ m_ANRMonitorOn))
		{
			if (on)
			{
				float seconds = Options.Get().GetFloat(Option.ANR_WAIT_SECONDS);
				float throttle = Options.Get().GetFloat(Option.ANR_THROTTLE);
				Log.ExceptionReporter.PrintInfo("ANR Monitor starts with seconds({0}) and throttle({1})", seconds, throttle);
				ExceptionReporter.Get().EnableANRMonitor(seconds, throttle);
				m_ANRMonitorOn = true;
			}
			else
			{
				Log.ExceptionReporter.PrintInfo("ANR Monitor stopped");
				ExceptionReporter.Get().EnableANRMonitor(0f, 0f);
				m_ANRMonitorOn = false;
			}
		}
	}

	public void OnLoginCompleted()
	{
	}

	private string[] GetLogPaths(ExceptionSettings.ReportType type)
	{
		if (type != 0 && type != ExceptionSettings.ReportType.CAUGHT_EXCEPTION && type != ExceptionSettings.ReportType.CRASH && type != ExceptionSettings.ReportType.ASSERTION)
		{
			return null;
		}
		return LogUtils.GetAllLogFiles(attachFolderName: true, (string l) => !l.EndsWith("Power.log") && !l.EndsWith("Zone.log"));
	}

	private string[] GetAttachableFiles(ExceptionSettings.ReportType type)
	{
		List<string> files = new List<string>
		{
			PlatformFilePaths.GetClientConfigPath(),
			LocalOptions.OptionsPath
		};
		if (type != 0 && type != ExceptionSettings.ReportType.CAUGHT_EXCEPTION && type != ExceptionSettings.ReportType.CRASH && type != ExceptionSettings.ReportType.ASSERTION)
		{
			string[] latestLogFiles = LogUtils.GetLatestLogFiles(3);
			foreach (string file in latestLogFiles)
			{
				files.Add(file);
			}
			_ = 1;
		}
		return files.ToArray();
	}

	private Dictionary<string, string> GetAdditionalInfo(ExceptionSettings.ReportType type)
	{
		Dictionary<string, string> dict = new Dictionary<string, string>();
		if (type == ExceptionSettings.ReportType.BUG)
		{
			dict.Add("Aurora.Env", Vars.Key("Aurora.Env").GetStr(""));
			dict.Add("Aurora.Version.Source", Vars.Key("Aurora.Version.Source").GetStr(""));
			dict.Add("Aurora.Version.String", Vars.Key("Aurora.Version.String").GetStr(""));
			dict.Add("Aurora.Version.Int", Vars.Key("Aurora.Version.Int").GetStr(""));
			dict.Add("Aurora.Version", Vars.Key("Aurora.Version").GetStr(""));
			dict.Add("Mode", HearthstoneApplication.GetMode().ToString());
			dict.Add("MEnv", HearthstoneApplication.GetMobileEnvironment().ToString());
		}
		else
		{
			dict.Add("GameAccountID", BnetUtils.TryGetGameAccountId().GetValueOrDefault().ToString());
			if (PlatformSettings.RuntimeOS == OSCategory.Android && !string.IsNullOrEmpty(AndroidDeviceSettings.Get().m_HSStore))
			{
				dict.Add("AndroidStore", AndroidDeviceSettings.Get().m_HSStore);
			}
		}
		if (BnetBar.Get() != null && BnetBar.Get().TryGetServerTime(out var serverTimeNow))
		{
			string currentServerTime = GameStrings.Format("GLOBAL_CURRENT_TIME_AND_DATE_DEV", GameStrings.Format("GLOBAL_CURRENT_TIME", DateTime.Now), GameStrings.Format("GLOBAL_CURRENT_DATE", serverTimeNow), GameStrings.Format("GLOBAL_CURRENT_TIME", serverTimeNow));
			dict.Add("ServerTime", currentServerTime);
		}
		return dict;
	}

	private byte[] ReadLogFileSharing(string filepath)
	{
		byte[] dataBytes = null;
		try
		{
			if (File.Exists(filepath))
			{
				string loggerName = Path.GetFileNameWithoutExtension(new FileInfo(filepath).Name);
				Logger targetLogger = LogSystem.Get().GetFullLogger(loggerName);
				dataBytes = ((targetLogger == null || !Path.GetFullPath(targetLogger.FilePath).Equals(Path.GetFullPath(filepath))) ? File.ReadAllBytes(filepath) : Encoding.ASCII.GetBytes(targetLogger.GetContent()));
			}
		}
		catch (Exception ex)
		{
			Log.ExceptionReporter.PrintError("Failed to read log file '{0}': {2}", filepath, ex.Message);
		}
		return dataBytes;
	}

	private void FlushAllLogs(ExceptionSettings.ReportType type)
	{
		if (type != ExceptionSettings.ReportType.ANR && type != ExceptionSettings.ReportType.CRASH)
		{
			Log.ExceptionReporter.PrintInfo(AssetLoader.Get()?.PrintRecordedAssets());
		}
		string[] files = Directory.GetFiles(Log.LogsPath);
		foreach (string logFile in files)
		{
			try
			{
				string loggerName = Path.GetFileNameWithoutExtension(new FileInfo(logFile).Name);
				Logger targetLogger = LogSystem.Get().GetFullLogger(loggerName);
				if (targetLogger != null)
				{
					targetLogger?.FlushContent();
				}
			}
			catch (Exception ex)
			{
				Log.ExceptionReporter.PrintError("Failed to flush '{0}' from the folder '{1}'\n: {2}", logFile, Log.LogsPath, ex.Message);
			}
		}
	}
}
