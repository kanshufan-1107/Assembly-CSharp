using System;
using System.Collections.Generic;

namespace Blizzard.BlizzardErrorMobile;

public class ExceptionSettings
{
	public enum ReportType
	{
		EXCEPTION,
		BUG,
		ANR,
		CAUGHT_EXCEPTION,
		CRASH,
		ASSERTION
	}

	public delegate string[] LogPathsHandler(ReportType type);

	public delegate string[] AttachableFilesHandler(ReportType type);

	public delegate Dictionary<string, string> AdditionalInfoHandler(ReportType type);

	public delegate byte[] ReadFileMethodHandler(string filePath);

	public int m_projectID = int.MaxValue;

	public int m_buildNumber = int.MaxValue;

	public string m_moduleName = "";

	public string m_branchName = "";

	public string m_version = "";

	public string m_locale = "";

	public string m_userUUID = "";

	public string m_jiraProjectName = "";

	public string m_jiraComponent = "";

	public string m_jiraVersion = "";

	public string m_debugModules = "";

	public string m_excetionHost = "";

	public string m_jiraHost = "";

	public bool m_cnRegion;

	[Obsolete("m_jiraZipSizeLimit is deprecated, please use m_maxZipSizeLimits instead.")]
	public int m_jiraZipSizeLimit = 7340032;

	[Obsolete("m_logLineLimit is deprecated, please use m_logLineLimits instead.")]
	public int m_logLineLimit = 500;

	[Obsolete("m_maxZipSizeLimit is deprecated, please use m_maxZipSizeLimits instead.")]
	public int m_maxZipSizeLimit = 2097152;

	public Dictionary<ReportType, int> m_logLineLimits = new Dictionary<ReportType, int>
	{
		{
			ReportType.EXCEPTION,
			500
		},
		{
			ReportType.BUG,
			0
		},
		{
			ReportType.ANR,
			0
		},
		{
			ReportType.CAUGHT_EXCEPTION,
			500
		},
		{
			ReportType.CRASH,
			500
		},
		{
			ReportType.ASSERTION,
			500
		}
	};

	public Dictionary<ReportType, int> m_maxZipSizeLimits = new Dictionary<ReportType, int>
	{
		{
			ReportType.EXCEPTION,
			2097152
		},
		{
			ReportType.BUG,
			7340032
		},
		{
			ReportType.ANR,
			2097152
		},
		{
			ReportType.CAUGHT_EXCEPTION,
			2097152
		},
		{
			ReportType.CRASH,
			2097152
		},
		{
			ReportType.ASSERTION,
			2097152
		}
	};

	public Dictionary<ReportType, int> m_maxScreenshotWidths = new Dictionary<ReportType, int>
	{
		{
			ReportType.EXCEPTION,
			-1
		},
		{
			ReportType.BUG,
			-1
		},
		{
			ReportType.ANR,
			-1
		},
		{
			ReportType.CAUGHT_EXCEPTION,
			-1
		},
		{
			ReportType.CRASH,
			-1
		},
		{
			ReportType.ASSERTION,
			-1
		}
	};

	public LogPathsHandler m_logPathsCallback;

	public AttachableFilesHandler m_attachableFilesCallback;

	public AdditionalInfoHandler m_additionalInfoCallback;

	public ReadFileMethodHandler m_readFileMethodCallback;
}
