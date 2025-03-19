using System;

namespace Blizzard.BlizzardErrorMobile;

[Serializable]
public class ExceptionStruct
{
	public string m_hash;

	public string m_message;

	public string m_stackTrace;

	public string m_reportUUID;

	public string m_zipName;

	public string[] m_modifiedCodes;

	public bool m_happenedBefore;

	public bool m_skipReport;

	public bool m_isANR;

	public ExceptionStruct(string hash, string message, string stackTrace, string reportUUID, string zipName, string[] modifiedCodes, bool happenedBefore, bool skipReport, bool isANR)
	{
		m_hash = hash;
		m_message = message;
		m_stackTrace = stackTrace;
		m_reportUUID = reportUUID;
		m_zipName = zipName;
		m_modifiedCodes = modifiedCodes;
		m_happenedBefore = happenedBefore;
		m_skipReport = skipReport;
		m_isANR = isANR;
	}
}
