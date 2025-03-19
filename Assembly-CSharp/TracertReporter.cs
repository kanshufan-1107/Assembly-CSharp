using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Blizzard.T5.Core.Utils;
using Hearthstone;
using tracert;
using UnityEngine;

public class TracertReporter
{
	private static NetCache.NetCacheFeatures.CacheTraceroute s_settings;

	private static bool IsElevateNeeded => false;

	private static bool IsReady { get; set; } = false;

	private static string InitStatus { get; set; } = "Undefined";

	private static bool TracerouteEnabled
	{
		get
		{
			NetCache.NetCacheFeatures features = NetCache.Get()?.GetNetObject<NetCache.NetCacheFeatures>();
			if (features != null)
			{
				s_settings = features.Traceroute;
				if (features.TracerouteEnabled)
				{
					return IsReady;
				}
				return false;
			}
			return IsReady;
		}
	}

	private static int MaxHops
	{
		get
		{
			if (s_settings != null)
			{
				return s_settings.MaxHops;
			}
			return 30;
		}
	}

	private static int MessageSize
	{
		get
		{
			if (s_settings != null)
			{
				return s_settings.MessageSize;
			}
			return 32;
		}
	}

	private static int MaxRetries
	{
		get
		{
			if (s_settings != null)
			{
				return s_settings.MaxRetries;
			}
			return 3;
		}
	}

	private static int TimeoutMs
	{
		get
		{
			if (s_settings != null)
			{
				return s_settings.TimeoutMs;
			}
			return 3000;
		}
	}

	private static bool ResolveHost
	{
		get
		{
			if (s_settings != null)
			{
				return s_settings.ResolveHost;
			}
			return false;
		}
	}

	private static void ReportTracertInfoInternal(string host, string hopStr)
	{
		if (string.IsNullOrEmpty(hopStr))
		{
			return;
		}
		List<string> hops = new List<string>();
		string[] resultAndLog = hopStr.Split(new string[1] { "%%%" }, StringSplitOptions.None);
		Debug.LogFormat("Tracert: " + hopStr);
		if (resultAndLog.Length > 1)
		{
			Debug.LogFormat(resultAndLog[1]);
		}
		hops.AddRange(resultAndLog[0].TrimEnd().Split(new string[3] { "\r\n", "\r", "\n" }, StringSplitOptions.None));
		int nFailedHops = 0;
		hops.ForEach(delegate(string h)
		{
			if (h.EndsWith("*\t"))
			{
				int num = nFailedHops + 1;
				nFailedHops = num;
			}
		});
		TelemetryManager.Client().SendTraceroute(host, hops, hops.Count, nFailedHops, hops.Count - nFailedHops);
	}

	private static async Task<string> RunTracertAsync(string host)
	{
		string resultLines = string.Empty;
		await Task.Run(delegate
		{
			try
			{
				resultLines = TracertAPI.GetTraceRouteStrWrapper(host, MaxHops, MessageSize, MaxRetries, TimeoutMs, ResolveHost, verbose: true);
			}
			catch (Exception ex)
			{
				Log.All.PrintWarning("Failed to get tracert information with " + host + ": " + ex.Message);
			}
		});
		return resultLines;
	}

	public static void SendTelemetry()
	{
		TelemetryManager.Client().SendInitTraceroute(IsReady, InitStatus);
	}

	public static void ReportTracertInfo(string host)
	{
		if (TracerouteEnabled && !string.IsNullOrEmpty(host))
		{
			Task.Run(async delegate
			{
				string resultLines = await RunTracertAsync(host);
				ReportTracertInfoInternal(host, resultLines);
			});
		}
	}

	public static void Initialize()
	{
		try
		{
			if (TracertAPI.IsAvailableICMP())
			{
				Debug.Log("Tracert: ICMP protocol is ready.");
				IsReady = true;
				InitStatus = "OK";
				return;
			}
			string args = string.Join(" ", HearthstoneApplication.CommandLineArgs);
			if (TracertAPI.PrepareICMPRule(IsElevateNeeded, args))
			{
				if (IsElevateNeeded)
				{
					Debug.Log("Tracert: It's elevated. Closing the current exe.");
					GeneralUtils.ExitApplication();
				}
				IsReady = true;
				InitStatus = "OK";
			}
			else if (IsElevateNeeded)
			{
				if (TracertAPI.IsRunAsAdministrator())
				{
					IsReady = true;
					InitStatus = "OK";
				}
				else
				{
					Debug.Log("Tracert: Need to admin permission!");
					InitStatus = "NotAdmin";
				}
			}
			else
			{
				Debug.Log("Tracert: failed to add ICMP rule.");
				InitStatus = "ICMPRuleAddFailure";
			}
		}
		catch (Exception ex)
		{
			Debug.Log("Failed to initialize tracert: " + ex.Message);
			InitStatus = "Exception-" + ex.Message;
		}
	}
}
