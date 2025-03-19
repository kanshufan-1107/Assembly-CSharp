using System;
using Blizzard.GameService.SDK.Client.Integration;
using Blizzard.T5.Configuration;
using Hearthstone.Dev;
using UnityEngine;

namespace Hearthstone.Devices;

public class DeviceLocaleHelper
{
	public static string GetTargetServer()
	{
		string targetServer = null;
		if (PlatformSettings.IsMobile() || PlatformSettings.IsSteam)
		{
			bool dev;
			if (PlatformSettings.IsMobileRuntimeOS)
			{
				try
				{
					targetServer = GetStoredBNetIP();
				}
				catch (Exception ex)
				{
					Debug.LogError("Exception while loading settings: " + ex.Message);
				}
				dev = HearthstoneApplication.GetMobileEnvironment() == MobileEnv.DEVELOPMENT;
			}
			else
			{
				dev = HearthstoneApplication.IsInternal();
			}
			if (!ClientConfigHelper.UseClientConfigForEnv())
			{
				targetServer = GetConnectionDataFromRegionId(GetCurrentRegionId(), dev).address;
			}
			string defaultEnv = (dev ? "bn11-01.battle.net" : "us.actual.battle.net");
			if (targetServer == null)
			{
				targetServer = Vars.Key("Aurora.Env").GetStr(defaultEnv);
			}
			if (targetServer == "default")
			{
				targetServer = defaultEnv;
			}
		}
		else
		{
			bool num = Vars.Key("Aurora.Env.Override").GetInt(0) != 0;
			string defaultEnv2 = "default";
			string front = null;
			if (num)
			{
				front = Vars.Key("Aurora.Env").GetStr(defaultEnv2);
				if (string.IsNullOrEmpty(front))
				{
					front = null;
				}
			}
			if (front == null)
			{
				front = BattleNet.GetConnectionString();
			}
			if (front == null)
			{
				string regionLaunchOption = BattleNet.GetLaunchOption("REGION", encrypted: false);
				if (!string.IsNullOrEmpty(regionLaunchOption))
				{
					front = regionLaunchOption switch
					{
						"US" => "us.actual.battle.net", 
						"XX" => "beta.actual.battle.net", 
						"EU" => "eu.actual.battle.net", 
						"CN" => "cn.actual.battlenet.com.cn", 
						"KR" => "kr.actual.battle.net", 
						_ => defaultEnv2, 
					};
				}
			}
			if (front.ToLower() == defaultEnv2)
			{
				front = "bn11-01.battle.net";
			}
			targetServer = front;
		}
		return targetServer;
	}

	public static uint GetPort()
	{
		uint port = 0u;
		if (PlatformSettings.IsMobile() || PlatformSettings.IsSteam)
		{
			bool dev = ((!PlatformSettings.IsMobileRuntimeOS) ? HearthstoneApplication.IsInternal() : (HearthstoneApplication.GetMobileEnvironment() == MobileEnv.DEVELOPMENT));
			if (!ClientConfigHelper.UseClientConfigForEnv())
			{
				port = (uint)GetConnectionDataFromRegionId(GetCurrentRegionId(), dev).port;
			}
			if (port == 0)
			{
				port = Vars.Key("Aurora.Port").GetUInt(0u);
			}
		}
		else if (Vars.Key("Aurora.Env.Override").GetUInt(0u) != 0)
		{
			port = Vars.Key("Aurora.Port").GetUInt(0u);
		}
		if (port == 0)
		{
			port = 1119u;
		}
		return port;
	}

	public static string GetVersion()
	{
		string version = null;
		if (PlatformSettings.IsMobile() || PlatformSettings.IsSteam)
		{
			bool dev;
			if (PlatformSettings.IsMobileRuntimeOS)
			{
				try
				{
					version = GetStoredVersion();
				}
				catch (Exception ex)
				{
					Debug.LogError("Exception while loading settings: " + ex.Message);
				}
				dev = HearthstoneApplication.GetMobileEnvironment() == MobileEnv.DEVELOPMENT;
			}
			else
			{
				dev = HearthstoneApplication.IsInternal();
			}
			if (!ClientConfigHelper.UseClientConfigForEnv())
			{
				version = GetConnectionDataFromRegionId(GetCurrentRegionId(), dev).version;
				if (version == "product")
				{
					version = Network.ProductVersion();
				}
			}
			else
			{
				version = ClientConfigHelper.GetVersionFromConfig();
			}
		}
		else
		{
			version = ClientConfigHelper.GetVersionFromConfig();
		}
		return version;
	}

	public static string GetStoredUserName()
	{
		return null;
	}

	public static string GetStoredBNetIP()
	{
		return null;
	}

	public static string GetStoredVersion()
	{
		return null;
	}

	public static BnetRegion FindDevRegionByServerVersion(string version)
	{
		foreach (BnetRegion region in DeviceLocaleData.s_regionIdToDevIP.Keys)
		{
			if (version == DeviceLocaleData.s_regionIdToDevIP[region].version)
			{
				return region;
			}
		}
		if (version.Contains("cn-dev"))
		{
			return BnetRegion.REGION_CN;
		}
		return BnetRegion.REGION_UNINITIALIZED;
	}

	public static BnetRegion GetCurrentRegionId()
	{
		if (PlatformSettings.LocaleVariant == LocaleVariant.China)
		{
			return BnetRegion.REGION_CN;
		}
		int currentRegion = Options.Get().GetInt(Option.PREFERRED_REGION, -1);
		if (currentRegion < 0 && ClientConfigHelper.UseClientConfigForEnv())
		{
			BnetRegion clientConfigRegion = FindDevRegionByServerVersion(Vars.Key("Aurora.Version.String").GetStr(""));
			Log.BattleNet.Print("Battle.net region from client.config version: " + clientConfigRegion);
			if (clientConfigRegion != BnetRegion.REGION_UNINITIALIZED)
			{
				return clientConfigRegion;
			}
		}
		return (BnetRegion)currentRegion;
	}

	public static DeviceLocaleData.ConnectionData GetConnectionDataFromRegionId(BnetRegion region, bool isDev)
	{
		DeviceLocaleData.ConnectionData data;
		if (isDev)
		{
			if (!DeviceLocaleData.s_regionIdToDevIP.TryGetValue(region, out data) && !DeviceLocaleData.s_regionIdToDevIP.TryGetValue(DeviceLocaleData.s_defaultDevRegion, out data))
			{
				Debug.LogError("Invalid region set for s_defaultDevRegion!  This should never happen!!!");
			}
		}
		else if (!DeviceLocaleData.s_regionIdToProdIP.TryGetValue(region, out data))
		{
			return DeviceLocaleData.s_defaultProdIP;
		}
		return data;
	}

	public static Locale GetBestGuessForLocale()
	{
		Locale guessLocale = Locale.enUS;
		string language = GetLanguageCode();
		Debug.Log("Device locale: " + language);
		if (PlatformSettings.LocaleVariant == LocaleVariant.China)
		{
			guessLocale = ((!(language == "en")) ? Locale.zhCN : Locale.enUS);
		}
		else
		{
			bool foundLocale = false;
			try
			{
				foundLocale = DeviceLocaleData.s_languageCodeToLocale.TryGetValue(language, out guessLocale);
			}
			catch (Exception)
			{
			}
			if (!foundLocale)
			{
				language = language.Substring(0, 2);
				try
				{
					foundLocale = DeviceLocaleData.s_languageCodeToLocale.TryGetValue(language, out guessLocale);
				}
				catch (Exception)
				{
				}
			}
			if (!foundLocale)
			{
				int regionValue = 1;
				string countryCode = GetCountryCode();
				try
				{
					DeviceLocaleData.s_countryCodeToRegionId.TryGetValue(countryCode, out regionValue);
				}
				catch (Exception)
				{
				}
				guessLocale = language switch
				{
					"es" => (regionValue != 1) ? Locale.esES : Locale.esMX, 
					"zh" => (!(countryCode == "CN")) ? Locale.zhTW : Locale.zhCN, 
					"en" => (regionValue == 2) ? Locale.enGB : Locale.enUS, 
					_ => Locale.enUS, 
				};
			}
		}
		return guessLocale;
	}

	public static string GetCountryCode()
	{
		return GetLocaleCountryCode();
	}

	public static string GetLanguageCode()
	{
		return GetLocaleLanguageCode();
	}

	private static string GetLocaleCountryCode()
	{
		return "";
	}

	private static string GetLocaleLanguageCode()
	{
		return "";
	}
}
