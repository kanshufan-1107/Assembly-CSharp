using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Blizzard.T5.Core.Utils;

public class LogUtils
{
	public static string[] GetLatestLogFiles(int nLatestLogs = int.MaxValue)
	{
		return GetLatestLogFolders(nLatestLogs).SelectMany((string folder) => Directory.EnumerateFiles(folder, "*.log", SearchOption.TopDirectoryOnly)).ToArray();
	}

	public static string[] GetLatestLogFolders(int nLatestLogs = int.MaxValue)
	{
		string[] list = Directory.GetDirectories(Log.LogsPath, "Hearthstone_*", SearchOption.TopDirectoryOnly);
		Array.Reverse(list);
		return new ArraySegment<string>(list, 0, Math.Min(list.Length, nLatestLogs)).ToArray();
	}

	public static string[] GetAllLogFiles(bool attachFolderName = false, Func<string, bool> filter = null)
	{
		List<string> files = new List<string>();
		if (Directory.Exists(Log.LogsPath))
		{
			string[] latestLogFolders = GetLatestLogFolders();
			for (int i = 0; i < latestLogFolders.Length; i++)
			{
				string normalizePath = FileUtils.NormalizePath(latestLogFolders[i]);
				string folderName = Path.GetFileName(normalizePath);
				string[] files2 = Directory.GetFiles(normalizePath, "*.log", SearchOption.AllDirectories);
				foreach (string logName in files2)
				{
					string subfolder = Path.GetDirectoryName(logName);
					string folderNameInZip = folderName;
					if (!subfolder.Equals(normalizePath))
					{
						folderNameInZip = folderName + subfolder.Substring(normalizePath.Length);
					}
					if (filter == null || filter(logName))
					{
						files.Add(string.Format("{0}{1}", Path.GetFullPath(logName), attachFolderName ? ("|" + folderNameInZip) : ""));
					}
				}
			}
		}
		return files.ToArray();
	}
}
