using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using UnityEngine;

namespace Blizzard.BlizzardErrorMobile;

public class ReportBuilder
{
	[CompilerGenerated]
	private bool _003CCaptureLogs_003Ek__BackingField;

	[CompilerGenerated]
	private bool _003CCaptureConfig_003Ek__BackingField;

	private static ExceptionSettings s_settings;

	public static string ApplicationUnityVersion { get; set; }

	public static bool ApplicationGenuine { get; set; }

	public ExceptionSettings.ReportType ReportType { get; set; }

	public int ProjectID { get; set; }

	public string ModuleName { get; set; }

	public string Summary { get; set; }

	public string StackTrace { get; protected set; }

	public string FullCrashReport { get; set; }

	public string Comment { get; protected set; }

	public string Hash { get; protected set; }

	public string Markup { get; protected set; }

	public string EnteredBy { get; set; }

	public int BuildNumber { get; set; }

	public string Branch { get; set; }

	public string Locale { get; set; }

	public string ReportUUID { get; set; }

	public string UserUUID { get; set; }

	public string DebugModules { get; set; }

	public bool CaptureLogs
	{
		[CompilerGenerated]
		set
		{
			_003CCaptureLogs_003Ek__BackingField = value;
		}
	}

	public bool CaptureConfig
	{
		[CompilerGenerated]
		set
		{
			_003CCaptureConfig_003Ek__BackingField = value;
		}
	}

	public bool HappenedBefore { get; set; }

	public bool IsANR => ReportType == ExceptionSettings.ReportType.ANR;

	public int SizeLimit { get; set; }

	public int LogLinesLimit { get; set; }

	public string[] ModifiedCodesInEditor { get; set; }

	public string[] LogPaths { get; set; }

	public string[] AttachableFiles { get; set; }

	public Dictionary<string, string> AddtionalInfo { get; set; }

	public static ExceptionSettings Settings
	{
		get
		{
			return s_settings;
		}
		set
		{
			if (string.IsNullOrEmpty(value.m_userUUID))
			{
				value.m_userUUID = SystemInfo.deviceUniqueIdentifier.ToString();
				ExceptionLogger.LogInfo("User UUID: {0}", value.m_userUUID);
			}
			if (value.m_projectID == int.MaxValue)
			{
				throw new Exception("Setting is invalid! - no projectID");
			}
			if (value.m_buildNumber == int.MaxValue)
			{
				throw new Exception("Setting is invalid! - no buildNumber");
			}
			if (string.IsNullOrEmpty(value.m_version))
			{
				throw new Exception("Setting is invalid! - no version");
			}
			if (string.IsNullOrEmpty(value.m_locale))
			{
				throw new Exception("Setting is invalid! - no locale");
			}
			if (value.m_logLineLimits[ExceptionSettings.ReportType.EXCEPTION] == 500)
			{
				value.m_logLineLimits[ExceptionSettings.ReportType.EXCEPTION] = value.m_logLineLimit;
			}
			if (value.m_logLineLimits[ExceptionSettings.ReportType.ASSERTION] == 500)
			{
				value.m_logLineLimits[ExceptionSettings.ReportType.ASSERTION] = value.m_logLineLimit;
			}
			if (value.m_maxZipSizeLimit > 10485760)
			{
				ExceptionLogger.LogWarning($"Zip max size for Exception board cannot exceed {10485760} bytes.");
				value.m_maxZipSizeLimit = 10485760;
			}
			if (value.m_maxZipSizeLimits[ExceptionSettings.ReportType.EXCEPTION] == 2097152)
			{
				value.m_maxZipSizeLimits[ExceptionSettings.ReportType.EXCEPTION] = value.m_maxZipSizeLimit;
			}
			if (value.m_maxZipSizeLimits[ExceptionSettings.ReportType.ASSERTION] == 2097152)
			{
				value.m_maxZipSizeLimits[ExceptionSettings.ReportType.ASSERTION] = value.m_maxZipSizeLimit;
			}
			if (value.m_jiraZipSizeLimit > 10485760)
			{
				ExceptionLogger.LogWarning($"Zip max size for JIRA system cannot exceed {10485760} bytes.");
				value.m_jiraZipSizeLimit = 10485760;
			}
			s_settings = value;
		}
	}

	private ReportBuilder()
	{
	}

	public static ReportBuilder BuildExceptionReport(string summary, string stackTrace, string comment, ExceptionSettings.ReportType reportType, bool happenedBefore, string hash = null, string fullCrashReport = null)
	{
		if (s_settings == null)
		{
			throw new Exception("Setting should be set!");
		}
		ReportBuilder builder = new ReportBuilder();
		builder.Summary = summary;
		builder.StackTrace = stackTrace;
		builder.Comment = comment;
		builder.Hash = hash ?? CreateHash(summary, stackTrace);
		builder.FullCrashReport = fullCrashReport;
		builder.CaptureLogs = true;
		builder.CaptureConfig = true;
		builder.SizeLimit = Settings.m_maxZipSizeLimits[reportType];
		builder.LogLinesLimit = Settings.m_logLineLimits[reportType];
		builder.EnteredBy = "0";
		builder.ReportUUID = Guid.NewGuid().ToString().ToUpper();
		builder.UserUUID = Settings.m_userUUID;
		builder.ProjectID = Settings.m_projectID;
		builder.ModuleName = Settings.m_moduleName;
		builder.BuildNumber = Settings.m_buildNumber;
		builder.Branch = Settings.m_branchName;
		builder.Locale = Settings.m_locale;
		builder.ReportType = reportType;
		builder.HappenedBefore = happenedBefore;
		builder.DebugModules = Settings.m_debugModules;
		if (TryFindModifiedCodesInEditor(stackTrace, out var modifiedCodes))
		{
			builder.ModifiedCodesInEditor = modifiedCodes.ToArray();
		}
		if (Settings.m_logPathsCallback != null && !Settings.m_cnRegion)
		{
			builder.LogPaths = Settings.m_logPathsCallback(builder.ReportType);
		}
		if (Settings.m_attachableFilesCallback != null && !Settings.m_cnRegion)
		{
			builder.AttachableFiles = Settings.m_attachableFilesCallback(builder.ReportType);
		}
		if (Settings.m_additionalInfoCallback != null && !Settings.m_cnRegion)
		{
			builder.AddtionalInfo = Settings.m_additionalInfoCallback(builder.ReportType);
		}
		builder.BuildExceptionMarkup();
		return builder;
	}

	private static bool TryFindModifiedCodesInEditor(string stackTrace, out List<string> modifiedCodes)
	{
		modifiedCodes = new List<string>();
		foreach (Match m in Regex.Matches(stackTrace, "at Assets/([\\w\\d\\s/_]*\\.cs):\\d+"))
		{
			string src = stackTrace.Substring(m.Groups[1].Index, m.Groups[1].Length);
			FileInfo fi = new FileInfo(Path.Combine(Application.dataPath, src));
			if (!modifiedCodes.Contains(src) && (!fi.Exists || !fi.IsReadOnly))
			{
				modifiedCodes.Add(src);
			}
		}
		return modifiedCodes.Count > 0;
	}

	public static string CreateHash(string summary, string stackTrace)
	{
		if (summary.Split('\n').Length > 2)
		{
			return SHA1Calc(ConvertBlob(stackTrace));
		}
		return SHA1Calc(ConvertBlob(summary + stackTrace));
	}

	public static string ConvertBlob(string blob)
	{
		string[] variablesPatterns = new string[8] { "at [\\w\\d\\s/\\._]*:\\d+", "at <[0-9a-fA-F]{32}>:0", "\\bBuildId: ([0-9a-f]{32}|[0-9a-f]{40})\\b", "\\b0x[0-9a-f]{16}\\b", "/data/(app|data)/com\\.blizzard\\..*\\.(so|apk)\\s+", "_m[0-9a-fA-F]{40}\\b", "\\s+/.*/libc.so\\s+", "at (libunity|libil2cpp|split_config)\\." };
		string pattern = string.Join("|", variablesPatterns);
		return Regex.Replace(blob, pattern, delegate(Match m)
		{
			if (m.Value.StartsWith("at "))
			{
				return "at <0>:0";
			}
			if (m.Value.StartsWith("BuildId:"))
			{
				return "BuildId: 0";
			}
			if (m.Value.StartsWith("0x"))
			{
				return "0x0";
			}
			if (m.Value.StartsWith("/data/app/") || m.Value.StartsWith("/data/data/"))
			{
				return "/data/app/com.blizzard./";
			}
			if (m.Value.StartsWith("_m"))
			{
				return "_m0";
			}
			return m.Value.EndsWith("libc.so ") ? " libc.so " : "0";
		});
	}

	public static string SHA1Calc(string message)
	{
		SHA1 hasher = SHA1.Create();
		byte[] array = hasher.ComputeHash(Encoding.ASCII.GetBytes(message));
		StringBuilder sb = new StringBuilder();
		byte[] array2 = array;
		foreach (byte b in array2)
		{
			sb.Append(b.ToString("x2"));
		}
		hasher.Dispose();
		return sb.ToString();
	}

	private void BuildExceptionMarkup()
	{
		string escapeSummary = CreateEscapedSGML(Summary);
		string escapedStackTrace = CreateEscapedSGML(StackTrace);
		string addtional = "";
		if (AddtionalInfo != null)
		{
			foreach (KeyValuePair<string, string> i in AddtionalInfo)
			{
				addtional = addtional + "\t\t<NameValuePair><Name>" + CreateEscapedSGML(i.Key) + "</Name><Value>" + CreateEscapedSGML(i.Value) + "</Value></NameValuePair>\n";
			}
		}
		addtional = addtional + "\t\t<NameValuePair><Name>Architecture</Name><Value>" + SystemInfo.processorType + "</Value></NameValuePair>\n";
		Markup = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n<ReportedIssue xmlns=\"http://schemas.datacontract.org/2004/07/Inspector.Models\" xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\">\n\t<ReportType>Detailed</ReportType>\n\t<EnteredBy>" + EnteredBy + "</EnteredBy>\n\t<IssueType>Exception</IssueType>\n\t<Summary>" + escapeSummary + "</Summary>\n\t<Assertion>" + escapedStackTrace + "</Assertion>\n\t<HashBlock>" + Hash + "</HashBlock>\n\t<Comments>" + Comment + "</Comments >\n\t<BuildNumber>" + BuildNumber + "</BuildNumber>\n\t<DebugModules>" + DebugModules + "</DebugModules>\n\t<Module>" + ModuleName + "</Module>\n\t<ProjectId>" + ProjectID + "</ProjectId>\n\t<IssueFields>\n   <IssueField><Name>ReportType</Name><Value>Detailed</Value></IssueField>\n   <IssueField><Name>EnteredBy</Name><Value>" + EnteredBy + "</Value></IssueField>\n\t<IssueField><Name>IssueType</Name><Value>Exception</Value></IssueField>\n\t<IssueField><Name>BuildNumber</Name><Value>" + BuildNumber + "</Value></IssueField>\n\t<IssueField><Name>Module</Name><Value>" + ModuleName + "</Value></IssueField>\n\t<IssueField><Name>ProjectId</Name><Value>" + ProjectID + "</Value></IssueField>\n\t</IssueFields>\n\t<Metadata><NameValuePairs>\n\t\t<NameValuePair><Name>Build</Name><Value>" + BuildNumber + "</Value></NameValuePair>\n\t\t<NameValuePair><Name>OS.Platform</Name><Value>" + Application.platform.ToString() + "</Value></NameValuePair>\n\t\t<NameValuePair><Name>Unity.Version</Name><Value>" + ApplicationUnityVersion + "</Value></NameValuePair>\n\t\t<NameValuePair><Name>Unity.Genuine</Name><Value>" + ApplicationGenuine + "</Value></NameValuePair>\n\t\t<NameValuePair><Name>Locale</Name><Value>" + Locale + "</Value></NameValuePair>\n\t\t<NameValuePair><Name>Branch</Name><Value>" + Branch + "</Value></NameValuePair>\n\t\t<NameValuePair><Name>Report.UUID</Name><Value>" + ReportUUID + "</Value></NameValuePair>\n\t\t<NameValuePair><Name>User.UUID</Name><Value>" + UserUUID + "</Value></NameValuePair>\n\t\t<NameValuePair><Name>FailureType</Name><Value>" + ReportType.ToString() + "</Value></NameValuePair>\n" + addtional + "\t</NameValuePairs></Metadata>\n</ReportedIssue>\n";
	}

	private static string CreateEscapedSGML(string blob)
	{
		XmlElement xmlElement = new XmlDocument().CreateElement("root");
		xmlElement.InnerText = blob;
		return xmlElement.InnerXml;
	}
}
