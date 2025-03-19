using System;
using Blizzard.T5.Core;

public class DeeplinkUtils
{
	public static Map<string, string> GetDeepLinkArgs(string deeplink)
	{
		Map<string, string> args = new Map<string, string>();
		if (!string.IsNullOrEmpty(deeplink))
		{
			string[] array = deeplink.Substring(deeplink.LastIndexOf('?') + 1).Split('&');
			foreach (string argTuple in array)
			{
				string[] argPair = argTuple.Split('=');
				if (argPair.Length != 2 || argPair[0].Length == 0)
				{
					Log.DeepLink.PrintInfo("Skipping invalid formed arg {0}", argTuple);
					continue;
				}
				string key = argPair[0];
				string value = Uri.UnescapeDataString(argPair[1]);
				if (args.ContainsKey(key))
				{
					Log.DeepLink.PrintInfo("Duplicate arg {0} in deeplink, overwritting previous value {1} with {2}", key, args[key], value);
				}
				Log.DeepLink.PrintDebug("Found deeplink arg {0} = {1}", key, value);
				args[key] = value;
			}
		}
		return args;
	}
}
