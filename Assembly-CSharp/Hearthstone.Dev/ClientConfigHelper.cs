using Blizzard.T5.Configuration;
using UnityEngine;

namespace Hearthstone.Dev;

public class ClientConfigHelper
{
	public static bool UseClientConfigForEnv()
	{
		bool overrideWithConfig = Vars.Key("Aurora.Env.Override").GetInt(0) != 0;
		if (Vars.Key("Aurora.Env.DisableOverrideOnDevices").GetInt(0) != 0)
		{
			overrideWithConfig = false;
		}
		string configServer = Vars.Key("Aurora.Env").GetStr("");
		bool configServerExists = configServer != null && !(configServer == "");
		return overrideWithConfig && configServerExists;
	}

	public static string GetVersionFromConfig()
	{
		string version = null;
		string auroraVersionSource = Vars.Key("Aurora.Version.Source").GetStr("undefined");
		if (auroraVersionSource == "undefined")
		{
			auroraVersionSource = "product";
		}
		if (auroraVersionSource == "product")
		{
			version = Network.ProductVersion();
		}
		else if (auroraVersionSource == "string")
		{
			string versionUndefinedStr = "undefined";
			version = Vars.Key("Aurora.Version.String").GetStr(versionUndefinedStr);
			if (version == versionUndefinedStr)
			{
				Debug.LogError("Aurora.Version.String undefined");
			}
		}
		else
		{
			Debug.LogError("unknown version source: " + auroraVersionSource);
			version = "0";
		}
		string[] commandLineArgs = HearthstoneApplication.CommandLineArgs;
		foreach (string arg in commandLineArgs)
		{
			if (arg.Equals("hsc") || arg.Equals("-hsc"))
			{
				version = "6969ef511a6cabbc24c5";
				break;
			}
			if (arg.Equals("hse1") || arg.Equals("-hse1"))
			{
				version = "707cb136922d1f294c4f";
				break;
			}
		}
		return version;
	}
}
