using System;
using System.Collections.Generic;
using Blizzard.T5.Configuration;
using Blizzard.T5.Core;
using Blizzard.T5.Core.Utils;
using Blizzard.T5.Logging;
using Blizzard.T5.Services;
using Hearthstone.Core.Deeplinking;

namespace Hearthstone.Core.Streaming;

public class LaunchArguments
{
	private enum LaunchArgumentType
	{
		Pipeline,
		Token,
		Version,
		DOP,
		Env,
		StartLocale,
		TDK,
		ContentStackEnv,
		Logs,
		StopDownloadAfter,
		Config,
		UnsaveCfg,
		BaseVersion,
		OpenMode,
		RunCheats
	}

	private static Dictionary<LaunchArgumentType, string> Arguments = new Dictionary<LaunchArgumentType, string>
	{
		{
			LaunchArgumentType.Pipeline,
			"pipeline"
		},
		{
			LaunchArgumentType.Token,
			"token"
		},
		{
			LaunchArgumentType.Version,
			"version_string"
		},
		{
			LaunchArgumentType.DOP,
			"dop"
		},
		{
			LaunchArgumentType.Env,
			"env"
		},
		{
			LaunchArgumentType.StartLocale,
			"start_locale"
		},
		{
			LaunchArgumentType.TDK,
			"tdk"
		},
		{
			LaunchArgumentType.ContentStackEnv,
			"cenv"
		},
		{
			LaunchArgumentType.Logs,
			"logs"
		},
		{
			LaunchArgumentType.StopDownloadAfter,
			"stopafter"
		},
		{
			LaunchArgumentType.Config,
			"cfg"
		},
		{
			LaunchArgumentType.UnsaveCfg,
			"un"
		},
		{
			LaunchArgumentType.BaseVersion,
			"base"
		},
		{
			LaunchArgumentType.OpenMode,
			"mode"
		},
		{
			LaunchArgumentType.RunCheats,
			"cheats"
		}
	};

	public static string LiveBuildVersion { get; set; } = string.Empty;

	public static string PipelineName { get; set; } = string.Empty;

	public static string TokenStr { get; set; } = string.Empty;

	public static void ReadLaunchArgumentsFromDeeplink()
	{
		DeeplinkService deeplinkService = DeeplinkService.Get();
		if (deeplinkService == null)
		{
			Log.All.PrintError("Failed to ready launch arguments as DeeplinkService wasn't found!");
			return;
		}
		string deeplink = deeplinkService.GetStartupDeeplinkUrl();
		Log.All.PrintInfo("Retrieved launched deeplink: " + deeplink);
		bool saveClientConfig = true;
		bool changedConfigValue = false;
		if (!string.IsNullOrEmpty(deeplink))
		{
			Map<string, string> args = DeeplinkUtils.GetDeepLinkArgs(deeplink);
			if (args.ContainsKey(Arguments[LaunchArgumentType.UnsaveCfg]))
			{
				saveClientConfig = false;
			}
			if (args.TryGetValue(Arguments[LaunchArgumentType.Pipeline], out var pipelineString) && pipelineString.Equals(VersionPipeline.LIVE.ToString()))
			{
				Log.All.PrintInfo("Live pipeline is overridden through proxy service");
				Vars.Key("Mobile.LiveOverride").Set("1", permanent: false);
			}
			if (args.TryGetValue(Arguments[LaunchArgumentType.Version], out var versionString))
			{
				if (!versionString.Equals("#"))
				{
					Vars.Key("Aurora.Version.Source").Set("string", saveClientConfig);
				}
				else
				{
					Vars.Key("Aurora.Version.Source").Clear();
				}
				SetClientConfig(versionString, "Aurora.Version.String", saveClientConfig);
				changedConfigValue = true;
			}
			if (args.ContainsKey(Arguments[LaunchArgumentType.DOP]))
			{
				Log.All.PrintInfo("DOP is on from deeplink args");
				Vars.Key("Aurora.Version.Source").Set("product", saveClientConfig);
				changedConfigValue = true;
			}
			if (args.TryGetValue(Arguments[LaunchArgumentType.Env], out var envString))
			{
				if (!envString.Equals("#"))
				{
					Vars.Key("Aurora.Env.Override").Set("1", saveClientConfig);
				}
				else
				{
					Vars.Key("Aurora.Env.Override").Clear();
				}
				SetClientConfig(envString, "Aurora.Env", saveClientConfig);
				changedConfigValue = true;
			}
			if (args.TryGetValue(Arguments[LaunchArgumentType.StartLocale], out var localeString))
			{
				Locale startLocale;
				try
				{
					startLocale = (Locale)Enum.Parse(typeof(Locale), localeString);
					Log.All.PrintInfo("Setting locale override from deeplink args {0}", localeString);
				}
				catch (ArgumentException ex)
				{
					Log.All.PrintError("Invalid locale from deeplink args {0}", localeString, ex);
					startLocale = Locale.enUS;
				}
				Localization.SetLocale(startLocale);
			}
			if (args.TryGetValue(Arguments[LaunchArgumentType.TDK], out var tdkString))
			{
				if (!tdkString.Equals("#"))
				{
					tdkString = $"https://{tdkString}-in.tdk.blizzard.net";
				}
				SetClientConfig(tdkString, "Telemetry.Host", saveClientConfig);
				changedConfigValue = true;
			}
			if (args.TryGetValue(Arguments[LaunchArgumentType.ContentStackEnv], out var cEnvString))
			{
				SetClientConfig(cEnvString, "ContentStack.Env", saveClientConfig);
				changedConfigValue = true;
			}
			if (args.TryGetValue(Arguments[LaunchArgumentType.StopDownloadAfter], out var stopAfter))
			{
				SetClientConfig(stopAfter, "Mobile.StopDownloadAfter", saveClientConfig);
				changedConfigValue = true;
			}
			if (args.TryGetValue(Arguments[LaunchArgumentType.BaseVersion], out var baseVersion))
			{
				SetClientConfig(baseVersion, "Mobile.BinaryVersion", saveClientConfig);
				changedConfigValue = true;
			}
			if (args.TryGetValue(Arguments[LaunchArgumentType.Logs], out var logsString))
			{
				if (!logsString.Equals("#"))
				{
					Log.All.PrintInfo("Setting logs override from deeplink args {0}", logsString);
					AddEnabledLogInOptions(logsString);
					if (saveClientConfig)
					{
						Options.Get().SetString(Option.ENABLED_LOG_LIST, logsString);
					}
				}
				else
				{
					Log.All.PrintInfo("Setting logs override has been cleared");
					Options.Get().DeleteOption(Option.ENABLED_LOG_LIST);
				}
			}
			if (args.TryGetValue(Arguments[LaunchArgumentType.Config], out var cfgString))
			{
				if (!cfgString.Equals("#"))
				{
					Log.All.PrintInfo("Setting additional config override from deeplink args {0}", cfgString);
					ProcessConfigArgument(cfgString, delegate(string key, string value)
					{
						SetClientConfig(value, key, saveClientConfig);
					}, delegate(string line)
					{
						SetOptionsTxt(line, saveClientConfig);
					});
				}
				SetClientConfig(cfgString, "Debug.Cfg", saveClientConfig);
				changedConfigValue = true;
			}
			if (args.TryGetValue(Arguments[LaunchArgumentType.OpenMode], out var modeString) && !envString.Equals("#"))
			{
				Vars.Key("Debug.OpenMode").Set(modeString, permanent: false);
			}
			if (args.TryGetValue(Arguments[LaunchArgumentType.RunCheats], out var cheatsString) && !envString.Equals("#"))
			{
				Vars.Key("Debug.RunCheats").Set(Uri.UnescapeDataString(cheatsString), permanent: false);
			}
		}
		if (saveClientConfig && changedConfigValue)
		{
			Vars.SaveConfig();
		}
	}

	public static void AddEnabledLogInOptions(string logList)
	{
		if (string.IsNullOrEmpty(logList))
		{
			logList = Options.Get().GetString(Option.ENABLED_LOG_LIST);
			if (string.IsNullOrEmpty(logList))
			{
				return;
			}
		}
		Log.All.PrintDebug("Enable logs: {0}", logList);
		logList.Split(',').ForEach(delegate(string l)
		{
			Log.SetStandardLogInfo(l, Blizzard.T5.Logging.LogLevel.Debug);
		});
	}

	public static void ProcessConfigArgument(string cfgString, Action<string, string> clientConfigCallback, Action<string> optionsCallback)
	{
		string[] array = cfgString.Split(new string[1] { ",," }, StringSplitOptions.None);
		foreach (string cfg in array)
		{
			int dotPos = cfg.IndexOf(".");
			if (dotPos < 0)
			{
				continue;
			}
			string location = cfg.Substring(0, dotPos).Trim().ToLower();
			string line = cfg.Substring(dotPos + 1);
			switch (location)
			{
			case "client":
			case "c":
			{
				int assignPos = line.IndexOf("=");
				if (assignPos >= 0)
				{
					string keyname = line.Substring(0, assignPos).Trim();
					string value = line.Substring(assignPos + 1).Trim();
					clientConfigCallback?.Invoke(keyname, value);
				}
				break;
			}
			case "option":
			case "o":
				optionsCallback?.Invoke(line);
				break;
			default:
				Log.All.PrintError("Unknown location string '{0}' is used in {1}", location, cfgString);
				break;
			}
		}
	}

	public static string CreateLaunchArgument(bool usingT5Mobile)
	{
		VersionConfigurationService versionConfigService = ServiceManager.Get<VersionConfigurationService>();
		if (versionConfigService != null)
		{
			VersionPipeline pipeline = versionConfigService.GetPipeline();
			if (pipeline != 0)
			{
				PipelineName = EnumUtils.GetString(pipeline);
			}
			string token = versionConfigService.GetClientToken();
			if (!string.IsNullOrEmpty(token))
			{
				TokenStr = token;
			}
		}
		if (string.IsNullOrEmpty(LiveBuildVersion) || string.IsNullOrEmpty(PipelineName) || string.IsNullOrEmpty(TokenStr))
		{
			Log.All.PrintWarning("Skipped to create a launch argument, Version: {0}, Pipeline: {1}, Token: {2}", LiveBuildVersion, PipelineName, TokenStr);
			return null;
		}
		string returnURI = string.Format("{0}?{1}={2}&{3}={4}", "hearthstone://", Arguments[LaunchArgumentType.Pipeline], PipelineName, Arguments[LaunchArgumentType.Token], TokenStr);
		if (Vars.Key("Aurora.Version.Source").GetStr(string.Empty) == "string")
		{
			returnURI += string.Format("&{0}={1}", Arguments[LaunchArgumentType.Version], Vars.Key("Aurora.Version.String").GetStr(string.Empty));
		}
		if (Vars.Key("Aurora.Env.Override").GetInt(0) != 0)
		{
			returnURI += string.Format("&{0}={1}", Arguments[LaunchArgumentType.Env], Vars.Key("Aurora.Env").GetStr(string.Empty));
		}
		returnURI += $"&{Arguments[LaunchArgumentType.StartLocale]}={Localization.GetLocaleName()}";
		string tdk = Vars.Key("Telemetry.Host").GetStr(string.Empty);
		if (!string.IsNullOrEmpty(tdk))
		{
			returnURI += string.Format("&{0}={1}", Arguments[LaunchArgumentType.TDK], tdk.Replace("-in.tdk.blizzard.net", "").Replace("https://", ""));
		}
		string cenv = Vars.Key("ContentStack.Env").GetStr(string.Empty);
		if (!string.IsNullOrEmpty(cenv))
		{
			returnURI += $"&{Arguments[LaunchArgumentType.ContentStackEnv]}={cenv}";
		}
		returnURI += string.Format("&{0}={1}", Arguments[LaunchArgumentType.Logs], string.Join(",", Log.GetEnabledLogNames()));
		string stopAfter = Vars.Key("Mobile.StopDownloadAfter").GetStr(string.Empty);
		if (!string.IsNullOrEmpty(stopAfter))
		{
			returnURI += $"&{Arguments[LaunchArgumentType.StopDownloadAfter]}={stopAfter}";
		}
		string cfg = Vars.Key("Debug.cfg").GetStr(string.Empty);
		if (!string.IsNullOrEmpty(cfg))
		{
			returnURI += $"&{Arguments[LaunchArgumentType.Config]}={cfg}";
		}
		string mode = Vars.Key("Debug.OpenMode").GetStr(string.Empty);
		if (!string.IsNullOrEmpty(mode))
		{
			returnURI += $"&{Arguments[LaunchArgumentType.OpenMode]}={mode}";
		}
		string cheats = Vars.Key("Debug.RunCheats").GetStr(string.Empty);
		if (!string.IsNullOrEmpty(cheats))
		{
			returnURI += $"&{Arguments[LaunchArgumentType.RunCheats]}={Uri.EscapeDataString(cheats)}";
		}
		return returnURI;
	}

	private static void SetClientConfig(string input, string keyName, bool permenant)
	{
		if (!input.Equals("#"))
		{
			Log.All.PrintInfo("Setting {0} from deeplink args {1}", keyName, input);
			Vars.Key(keyName).Set(input, permenant);
		}
		else
		{
			Log.All.PrintInfo("Setting {0} has been cleared", keyName);
			Vars.Key(keyName).Clear();
		}
	}

	private static void SetOptionsTxt(string input, bool permenant)
	{
		int assignChPos = input.IndexOf("=#");
		if (assignChPos == -1)
		{
			Log.All.PrintInfo("Option override from deeplink args {0}", input);
			LocalOptions.Get().SetByLine(input, permenant);
		}
		else
		{
			string key = input.Substring(0, assignChPos);
			Log.All.PrintInfo("Option '{0}' has been cleared", key);
			Options.Get().DeleteOption(key);
		}
	}
}
