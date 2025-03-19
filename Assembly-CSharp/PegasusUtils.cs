using System.IO;
using System.Linq;
using System.Xml.Linq;
using UnityEngine;

public static class PegasusUtils
{
	public static string GetPatchDir()
	{
		string patchDir = Directory.GetCurrentDirectory();
		patchDir = patchDir.Substring(0, patchDir.LastIndexOf(Path.DirectorySeparatorChar));
		return patchDir.Substring(0, patchDir.LastIndexOf(Path.DirectorySeparatorChar));
	}

	public static string GetBranchName()
	{
		return XDocument.Load(Path.Combine(GetPatchDir(), "branch_info.xml")).Descendants("BranchName").ElementAt(0)
			.Value;
	}

	private static void UseStackTraceLoggingMinimum()
	{
		Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
		Application.SetStackTraceLogType(LogType.Assert, StackTraceLogType.None);
		Application.SetStackTraceLogType(LogType.Warning, StackTraceLogType.None);
	}

	public static void SetStackTraceLoggingOptions(bool forceUseMinimumLogging)
	{
		if (forceUseMinimumLogging)
		{
			UseStackTraceLoggingMinimum();
		}
		else
		{
			UseStackTraceLoggingMinimum();
		}
	}
}
