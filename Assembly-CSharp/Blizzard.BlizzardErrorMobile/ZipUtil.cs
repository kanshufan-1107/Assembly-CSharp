using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Unity.SharpZipLib.Zip;

namespace Blizzard.BlizzardErrorMobile;

internal static class ZipUtil
{
	private static long PositionBeforeSizeLimit { get; set; } = 0L;

	private static long ExpectedPosition { get; set; } = long.MaxValue;

	public static byte[] TryBuildZipArchive(ReportBuilder builder)
	{
		PositionBeforeSizeLimit = 0L;
		MemoryStream memoryStream = new MemoryStream();
		ZipConstants.DefaultCodePage = 0;
		ZipOutputStream zipStream = new ZipOutputStream(memoryStream, 4096);
		zipStream.SetLevel(6);
		byte[] dataBytes = new byte[0];
		if (!string.IsNullOrEmpty(builder.Markup))
		{
			dataBytes = Encoding.UTF8.GetBytes(builder.Markup);
			SafeAddToZip(zipStream, dataBytes, "ReportedIssue.xml");
		}
		if (!string.IsNullOrEmpty(builder.FullCrashReport))
		{
			dataBytes = Encoding.UTF8.GetBytes(builder.FullCrashReport);
			SafeAddToZip(zipStream, dataBytes, "FullCrashReport.txt");
		}
		AddLogFiles(zipStream, builder);
		AddAttachableFiles(zipStream, builder);
		if (memoryStream.Length > builder.SizeLimit)
		{
			return null;
		}
		zipStream.IsStreamOwner = false;
		zipStream.Close();
		memoryStream.Flush();
		return memoryStream.ToArray();
	}

	public static byte[] BuildZipArchive(ReportBuilder builder)
	{
		ExpectedPosition = long.MaxValue;
		byte[] ret = TryBuildZipArchive(builder);
		if (ret == null && PositionBeforeSizeLimit > 0 && PositionBeforeSizeLimit < builder.SizeLimit)
		{
			ExceptionLogger.LogInfo("Zip file is too big. Attempting to make a smaller zip.");
			ExpectedPosition = PositionBeforeSizeLimit;
			ret = TryBuildZipArchive(builder);
		}
		if (ret == null)
		{
			throw new InsufficientMemoryException("The zip file is too large");
		}
		return ret;
	}

	public static void AddFileToZip(string filename, string zipFile, int sizeLimit)
	{
		MemoryStream memoryStream = new MemoryStream();
		using (FileStream zipToOpen = new FileStream(zipFile, FileMode.Open))
		{
			ZipInputStream zipInputStream = new ZipInputStream(zipToOpen);
			ZipConstants.DefaultCodePage = 0;
			ZipOutputStream zipOutputStream = new ZipOutputStream(memoryStream, 4096);
			byte[] buffer = new byte[4096];
			ZipEntry currentEntry;
			while ((currentEntry = zipInputStream.GetNextEntry()) != null)
			{
				zipOutputStream.PutNextEntry(new ZipEntry(currentEntry.Name));
				int length;
				while ((length = zipInputStream.Read(buffer, 0, buffer.Length)) > 0)
				{
					zipOutputStream.Write(buffer, 0, length);
				}
				zipOutputStream.CloseEntry();
			}
			SafeAddToZip(zipOutputStream, File.ReadAllBytes(filename), new FileInfo(filename).Name);
			zipOutputStream.IsStreamOwner = false;
			zipOutputStream.Close();
			zipInputStream.Close();
			memoryStream.Flush();
			if (memoryStream.Length > sizeLimit)
			{
				memoryStream.Close();
				throw new InsufficientMemoryException("The zip file is too large");
			}
		}
		try
		{
			File.Delete(zipFile);
			File.WriteAllBytes(zipFile, memoryStream.ToArray());
			ExceptionLogger.LogDebug("Added " + filename + " to " + zipFile + "!");
		}
		catch (Exception ex)
		{
			ExceptionLogger.LogError("Failed to add " + filename + " to " + zipFile + ": " + ex.Message);
		}
	}

	private static bool IsValidZipSize(long curLength, long sizeLimit)
	{
		if (curLength >= ExpectedPosition)
		{
			ExceptionLogger.LogInfo($"Meet the expected max zip size. {curLength} >= {ExpectedPosition}");
			return false;
		}
		if (curLength > sizeLimit)
		{
			ExceptionLogger.LogWarning($"Zip file is too big to add a folder. '{curLength}'");
			return false;
		}
		return true;
	}

	private static void SafeAddFolderToZip(ZipOutputStream zipStream, string folder, string name, long sizeLimit)
	{
		if (string.IsNullOrEmpty(folder) || !Directory.Exists(folder))
		{
			return;
		}
		string[] array = Directory.GetFiles(folder).ToArray();
		foreach (string logFile in array)
		{
			if (!IsValidZipSize(zipStream.Length, sizeLimit))
			{
				return;
			}
			try
			{
				PositionBeforeSizeLimit = zipStream.Length;
				SafeAddToZip(zipStream, logFile, name + "/" + new FileInfo(logFile).Name);
			}
			catch (Exception ex)
			{
				ExceptionLogger.LogError("Failed to add '" + logFile + "' from the folder '" + folder + "' to zip: " + ex.Message);
			}
		}
		array = Directory.GetDirectories(folder).ToArray();
		foreach (string dir in array)
		{
			string dirName = new DirectoryInfo(dir).Name;
			SafeAddFolderToZip(zipStream, dir, name + "/" + dirName, sizeLimit);
		}
	}

	private static void SafeAddToZip(ZipOutputStream zipStream, byte[] bytes, string name)
	{
		try
		{
			if (bytes != null && bytes.Length != 0)
			{
				zipStream.PutNextEntry(new ZipEntry(name));
				zipStream.Write(bytes, 0, bytes.Length);
				zipStream.CloseEntry();
			}
		}
		catch (NotSupportedException ex)
		{
			ExceptionLogger.LogError("Tried to add " + name + " to zip:\n" + ex.Message);
		}
	}

	private static void SafeAddToZip(ZipOutputStream zipStream, string filePath, string name)
	{
		if (ReportBuilder.Settings.m_readFileMethodCallback != null)
		{
			SafeAddToZip(zipStream, ReportBuilder.Settings.m_readFileMethodCallback(filePath), name);
		}
		else
		{
			SafeAddToZip(zipStream, File.ReadAllBytes(filePath), name);
		}
	}

	private static void AddLogFile(ZipOutputStream zipStream, ReportBuilder builder, string log, string folder)
	{
		try
		{
			if (File.Exists(log) && new FileInfo(log).Length <= ReportBuilder.Settings.m_maxZipSizeLimits[builder.ReportType])
			{
				string[] lines = new string[0];
				if (ReportBuilder.Settings.m_readFileMethodCallback != null)
				{
					lines = Encoding.UTF8.GetString(ReportBuilder.Settings.m_readFileMethodCallback(log)).Split(new string[3] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
				}
				if (builder.LogLinesLimit > 0)
				{
					lines = lines.Reverse().Take(builder.LogLinesLimit).Reverse()
						.ToArray();
				}
				SafeAddToZip(zipStream, Encoding.UTF8.GetBytes(string.Join("\r\n", lines)), (string.IsNullOrEmpty(folder) ? string.Empty : (folder + "/")) + Path.GetFileName(log));
				ExceptionLogger.LogInfo("Attached the log file: " + log);
			}
		}
		catch (Exception ex)
		{
			ExceptionLogger.LogError("Failed to retrieve logs from '" + log + "' with Error: " + ex.Message);
		}
	}

	private static void AddLogFiles(ZipOutputStream zipStream, ReportBuilder builder)
	{
		if (builder.LogPaths == null)
		{
			return;
		}
		string[] logPaths = builder.LogPaths;
		foreach (string log in logPaths)
		{
			if (IsValidZipSize(zipStream.Length, builder.SizeLimit))
			{
				PositionBeforeSizeLimit = zipStream.Position;
				string[] values = log.Split('|');
				AddLogFile(zipStream, builder, values[0], (values.Count() > 1) ? values[1] : null);
				continue;
			}
			break;
		}
	}

	private static void AddScreenshotFile(ReportBuilder builder)
	{
		List<string> files = new List<string>();
		if (builder.AttachableFiles != null)
		{
			files.AddRange(builder.AttachableFiles);
		}
		if (File.Exists(Screenshot.ScreenshotPath))
		{
			files.Add(string.Format("{0}|{1}|{2}|image/png", Screenshot.ScreenshotPath, "screenshot.png", (builder.ReportType == ExceptionSettings.ReportType.BUG) ? "1" : "0"));
			builder.AttachableFiles = files.ToArray();
		}
	}

	private static void AddAttachableFiles(ZipOutputStream zipStream, ReportBuilder builder)
	{
		AddScreenshotFile(builder);
		if (builder.AttachableFiles == null)
		{
			return;
		}
		string[] attachableFiles = builder.AttachableFiles;
		for (int i = 0; i < attachableFiles.Length; i++)
		{
			string[] values = attachableFiles[i].Split('|');
			if (values.Count() > 2 && values[2] == "1")
			{
				continue;
			}
			string file = values[0];
			string name = null;
			if (values.Count() > 1)
			{
				name = values[1];
			}
			if (!IsValidZipSize(zipStream.Length, builder.SizeLimit))
			{
				break;
			}
			try
			{
				if (Directory.Exists(file))
				{
					SafeAddFolderToZip(zipStream, file, string.IsNullOrEmpty(name) ? Path.GetFileName(file.TrimEnd(Path.DirectorySeparatorChar)) : name, builder.SizeLimit);
					ExceptionLogger.LogInfo("Attached the folder '" + file + "'");
				}
				else if (File.Exists(file) || ReportBuilder.Settings.m_readFileMethodCallback != null)
				{
					PositionBeforeSizeLimit = zipStream.Length;
					SafeAddToZip(zipStream, file, string.IsNullOrEmpty(name) ? Path.GetFileName(file) : name);
					ExceptionLogger.LogInfo("Attached the file '" + file + "'");
				}
			}
			catch (Exception ex)
			{
				ExceptionLogger.LogError("Failed to attach '" + file + "' with Error: " + ex.Message);
			}
		}
	}
}
