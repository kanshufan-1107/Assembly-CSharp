using System.Collections.Generic;
using System.IO;
using Hearthstone.Util;

public class AssetBundleInfo
{
	private static Dictionary<string, string> s_bundleNameToPath = new Dictionary<string, string>();

	public static string BundlePathPlatformModifier()
	{
		return "Win/";
	}

	public static string GetAssetBundlePath(string bundleName)
	{
		string path = string.Empty;
		if (bundleName != null && !s_bundleNameToPath.TryGetValue(bundleName, out path))
		{
			path = PlatformFilePaths.CreateLocalFilePath($"Data/{BundlePathPlatformModifier()}{bundleName}");
			s_bundleNameToPath[bundleName] = path;
		}
		return path;
	}

	public static bool Exists(string bundlePath)
	{
		return File.Exists(bundlePath);
	}
}
