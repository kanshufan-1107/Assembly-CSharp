using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Networking;

namespace Blizzard.BlizzardErrorMobile;

public class ExceptionReporter : ILogHandler
{
	public delegate void ExceptionWithModifiedCodeHandler(string reportUUID, bool happenedBefore, bool isANR, string[] modifiedCodes, Action<bool> returnCallback);

	private const int DEFAULT_MAX_RECORDS = 20;

	private const float DEFAULT_ANR_THROTTLE = 0.05f;

	private static readonly string s_titleOfANRException = "ANR detected";

	private static readonly string s_titleOfCaughtException = "[Caught] ";

	private static readonly string s_titleOfAssertion = "[Assert] ";

	private static readonly string s_prefixOfZip = "exception-";

	private static readonly string s_prefixOfScreenshot = "Screenshot-";

	private static readonly string s_nameOfDataFile = "ExceptionReporter.json";

	[CompilerGenerated]
	private Action<ExceptionSettings.ReportType> AfterZipping;

	[CompilerGenerated]
	private Action<string, bool, bool> NotifyExceptionRecord;

	[CompilerGenerated]
	private Action<string, string> NotifyJiraIssueID;

	private static ExceptionReporter s_instance;

	private bool m_started;

	private bool m_processedPreviousExceptions;

	private string m_zipFileForANR;

	private int m_zipFileCount;

	private string m_exceptionDir;

	private Dictionary<string, bool> m_responseFromUser = new Dictionary<string, bool>();

	private Uri m_exceptionSubmitUrl;

	private MonoBehaviour m_monoBehaviour;

	private RecordedExceptions m_recordedExceptions = new RecordedExceptions();

	private HashSet<string> m_seenHashes = new HashSet<string>();

	private ANRMonitor m_monitorANR;

	private ILogHandler m_unityLogHandler;

	public string UnhandledExceptionContext;

	private string m_uniqueIDStr;

	private Uri m_jiraSubmitUrl;

	private Uri m_jiraUserUrl;

	public ExceptionWithModifiedCodeHandler ExceptionWithModifiedCodeCallback { get; }

	public bool Busy { get; private set; }

	public bool IsInDebugMode { get; set; }

	public bool IsFakeReport { get; }

	public bool IsEnabledANRMonitor
	{
		get
		{
			if (m_monitorANR == null)
			{
				return false;
			}
			return !m_monitorANR.IsTerminated;
		}
	}

	public bool ReportOnTheFly { get; } = true;

	public bool SendExceptions { get; set; } = true;

	public bool SendAsserts { get; set; }

	public bool SendErrors { get; set; }

	private Uri ExceptionSubmitURL
	{
		get
		{
			if (m_exceptionSubmitUrl == null)
			{
				if (string.IsNullOrEmpty(ReportBuilder.Settings.m_excetionHost))
				{
					ReportBuilder.Settings.m_excetionHost = (ReportBuilder.Settings.m_cnRegion ? "https://excp.battlenet.com.cn" : "https://excp.blizzard.com");
				}
				m_exceptionSubmitUrl = new Uri(new Uri(ReportBuilder.Settings.m_excetionHost), $"/submit/{ReportBuilder.Settings.m_projectID}");
			}
			return m_exceptionSubmitUrl;
		}
	}

	private string GetZipName => Path.Combine(m_exceptionDir, $"{s_prefixOfZip}{GetUniqueID}{m_zipFileCount++}.zip".Replace("\\", "/"));

	private string ScreenshotPathName => Path.Combine(m_exceptionDir, (s_prefixOfScreenshot + GetUniqueID + ".png").Replace("\\", "/"));

	private string GetUniqueID
	{
		get
		{
			if (string.IsNullOrEmpty(m_uniqueIDStr))
			{
				byte[] bytes = BitConverter.GetBytes(DateTime.Now.Ticks);
				m_uniqueIDStr = Convert.ToBase64String(bytes).Replace('+', '_').Replace('/', '-')
					.TrimEnd('=');
			}
			return m_uniqueIDStr;
		}
	}

	private string ExceptionReporterDataPath => Path.Combine(m_exceptionDir, s_nameOfDataFile ?? "").Replace("\\", "/");

	public event Action<ExceptionSettings.ReportType> BeforeZipping;

	public static ExceptionReporter Get()
	{
		if (s_instance == null)
		{
			s_instance = new ExceptionReporter();
		}
		return s_instance;
	}

	public void Initialize(string installPath, IExceptionLogger logger, MonoBehaviour monoBehaviour)
	{
		ExceptionLogger.SetLogger(logger);
		m_monoBehaviour = monoBehaviour;
		ExceptionLogger.LogInfo("Version " + VersionInfo.VERSION);
		m_exceptionDir = Path.Combine(installPath, "Exceptions").Replace("\\", "/");
		if (!Directory.Exists(m_exceptionDir))
		{
			try
			{
				Directory.CreateDirectory(m_exceptionDir);
			}
			catch (Exception ex)
			{
				ExceptionLogger.LogError("Failed to create the folder '{0}' for exceptions: {1}", m_exceptionDir, ex.Message);
				UnregisterLogsCallback();
				return;
			}
		}
		ExceptionLogger.LogInfo("ScreenshotPath: {0}", string.IsNullOrEmpty(ScreenshotPathName) ? Screenshot.ScreenshotPath : ScreenshotPathName);
		CleanScreenshotFiles();
		DeserializeRecordedExceptions();
		CallbackManager.RegisterExceptionHandler();
		RegisterLogsCallback();
		ReportBuilder.ApplicationUnityVersion = Application.unityVersion;
		ReportBuilder.ApplicationGenuine = Application.genuine;
		CreateCoroutine(TryReportPreviousExceptions());
	}

	public bool SetSettings(ExceptionSettings settings)
	{
		try
		{
			ReportBuilder.Settings = settings;
		}
		catch (Exception ex)
		{
			ExceptionLogger.LogError("Failed to set Settings: " + ex.Message);
			return false;
		}
		return true;
	}

	public ExceptionSettings GetSettings()
	{
		return ReportBuilder.Settings;
	}

	public void ReportCaughtException(Exception exception)
	{
		if (string.IsNullOrEmpty(exception.StackTrace))
		{
			try
			{
				throw exception;
			}
			catch (Exception exception2)
			{
				ReportCaughtException(exception2);
				return;
			}
		}
		if (UseUnityLogHandler())
		{
			Il2CppProcessException.Process(exception, ExceptionSettings.ReportType.CAUGHT_EXCEPTION);
		}
		else
		{
			RecordException(exception.Message, exception.StackTrace, recordOnly: false, ExceptionSettings.ReportType.CAUGHT_EXCEPTION, happenedBefore: false);
		}
	}

	public void ReportAssertion(Exception assertion)
	{
		if (string.IsNullOrEmpty(assertion.StackTrace))
		{
			try
			{
				throw assertion;
			}
			catch (Exception assertion2)
			{
				ReportAssertion(assertion2);
				return;
			}
		}
		if (UseUnityLogHandler())
		{
			Il2CppProcessException.Process(assertion, ExceptionSettings.ReportType.ASSERTION);
		}
		else
		{
			RecordException(assertion.Message, assertion.StackTrace, recordOnly: false, ExceptionSettings.ReportType.ASSERTION, happenedBefore: false);
		}
	}

	public void RecordException(string message, string stackTrace)
	{
		RecordException(message, stackTrace, recordOnly: false, ExceptionSettings.ReportType.EXCEPTION, happenedBefore: false);
	}

	public bool HasHashBeenSeenBefore(string hash)
	{
		return m_seenHashes.Contains(hash);
	}

	public void RecordException(string message, string stackTrace, bool recordOnly, ExceptionSettings.ReportType reportType, bool happenedBefore, string hash = null, string fullCrashReport = null)
	{
		message = reportType switch
		{
			ExceptionSettings.ReportType.CAUGHT_EXCEPTION => s_titleOfCaughtException + message, 
			ExceptionSettings.ReportType.ASSERTION => s_titleOfAssertion + message, 
			_ => message, 
		};
		if (!SendExceptions)
		{
			ExceptionLogger.LogInfo("Exception has been reported but skipped because SendException is off.");
			return;
		}
		ReportBuilder builder;
		try
		{
			builder = ReportBuilder.BuildExceptionReport(message, stackTrace, string.Empty, reportType, happenedBefore, hash, fullCrashReport);
		}
		catch (Exception ex)
		{
			ExceptionLogger.LogError("Failed to create ExceptionReport: " + ex.Message);
			return;
		}
		if (HasHashBeenSeenBefore(builder.Hash))
		{
			ExceptionLogger.LogDebug("Skipped same Exception...");
			return;
		}
		ExceptionLogger.LogInfo("Record an exception Hash {0}, ReportUUID {1}\n{2}\nAt:\n{3}", builder.Hash, builder.ReportUUID, message, stackTrace);
		lock (m_recordedExceptions)
		{
			if (m_recordedExceptions.m_records.Count + m_recordedExceptions.m_backupRecords.Count >= 20)
			{
				ExceptionLogger.LogWarning("It reached the maximum records. Skipped.");
				return;
			}
		}
		m_seenHashes.Add(builder.Hash);
		if (!ReportOnTheFly || recordOnly)
		{
			MakeZipAndRecordInner(builder);
		}
		else if (CreateCoroutine(MakeZipAndRecord(builder, ReportOnTheFly)) == null)
		{
			MakeZipAndRecordInner(builder);
		}
	}

	public void ClearExceptionHashes()
	{
		ReportExceptions();
		m_seenHashes.Clear();
	}

	public bool EnableANRMonitor(float waitLimitSeconds, float throttle)
	{
		if (m_monoBehaviour == null)
		{
			ExceptionLogger.LogError("EnableANRMonitor can be used after initialization only.");
			return false;
		}
		if (m_monitorANR == null)
		{
			m_monitorANR = new ANRMonitor(waitLimitSeconds, throttle, m_monoBehaviour);
			m_monitorANR.Detected += RecordANRException;
			m_monitorANR.FirstUpdateAfterANR += ReportANRException;
		}
		else
		{
			m_monitorANR.SetWaitSeconds(waitLimitSeconds, throttle);
		}
		return true;
	}

	public void OnApplicationPause(bool pauseStatus)
	{
		m_monitorANR?.OnPause(pauseStatus);
	}

	private Coroutine CreateCoroutine(IEnumerator routine)
	{
		try
		{
			return m_monoBehaviour.StartCoroutine(routine);
		}
		catch (Exception ex)
		{
			ExceptionLogger.LogError("Failed to start coroutine: " + ex.Message);
			return null;
		}
	}

	private bool UseUnityLogHandler()
	{
		RuntimePlatform platform = Application.platform;
		return platform == RuntimePlatform.IPhonePlayer || platform == RuntimePlatform.Android;
	}

	private void RegisterLogsCallback()
	{
		if (m_started)
		{
			return;
		}
		ExceptionLogger.LogDebug("Registering error reporter callback");
		if (UseUnityLogHandler())
		{
			if (Debug.unityLogger.logHandler == this)
			{
				ExceptionLogger.LogWarning("Debug.unityLogger.logHandler has already been registered.");
				return;
			}
			m_unityLogHandler = Debug.unityLogger.logHandler;
			Debug.unityLogger.logHandler = this;
		}
		Application.logMessageReceivedThreaded += LogMessageCallback;
		m_started = true;
	}

	private void UnregisterLogsCallback()
	{
		if (!m_started)
		{
			return;
		}
		ExceptionLogger.LogDebug("Unregistering error reporter callback");
		if (UseUnityLogHandler())
		{
			if (m_unityLogHandler == null)
			{
				ExceptionLogger.LogError("UnregisterLogsCallback called when we are not registered.");
				return;
			}
			Debug.unityLogger.logHandler = m_unityLogHandler;
			m_unityLogHandler = null;
		}
		Application.logMessageReceivedThreaded -= LogMessageCallback;
		m_started = false;
	}

	public void LogException(Exception exception, UnityEngine.Object context)
	{
		try
		{
			Il2CppProcessException.Process(exception, ExceptionSettings.ReportType.EXCEPTION);
		}
		finally
		{
			m_unityLogHandler.LogException(exception, context);
		}
	}

	public void LogFormat(LogType logType, UnityEngine.Object context, string format, params object[] args)
	{
		try
		{
		}
		finally
		{
			m_unityLogHandler.LogFormat(logType, context, format, args);
		}
	}

	private void LogMessageCallback(string message, string stackTrace, LogType logType)
	{
		switch (logType)
		{
		case LogType.Assert:
			if (SendAsserts)
			{
				RecordException(message, stackTrace);
			}
			break;
		case LogType.Error:
			if (SendErrors && !ExceptionLogger.IsExceptionLoggerError(message))
			{
				RecordException(message, stackTrace);
			}
			break;
		case LogType.Exception:
			if (!UseUnityLogHandler() && SendExceptions)
			{
				if (UnhandledExceptionContext == null)
				{
					RecordException(message, stackTrace);
					break;
				}
				RecordException(message + " Context: " + UnhandledExceptionContext, stackTrace);
				UnhandledExceptionContext = null;
			}
			break;
		case LogType.Warning:
		case LogType.Log:
			break;
		}
	}

	private IEnumerator TryReportPreviousExceptions()
	{
		yield return new WaitUntil(() => ReportBuilder.Settings != null);
		ClearExceptionHashes();
	}

	private void RecordANRException()
	{
		RecordException(s_titleOfANRException, "", recordOnly: true, ExceptionSettings.ReportType.ANR, happenedBefore: false);
	}

	private void ReportANRException()
	{
		m_monoBehaviour.StartCoroutine(AddScreenshotAndReport());
	}

	private IEnumerator AddScreenshotAndReport()
	{
		if (ReportBuilder.Settings.m_cnRegion)
		{
			ExceptionLogger.LogDebug("Skip creating a screenshot because it's restricted.");
		}
		else if (!string.IsNullOrEmpty(m_zipFileForANR) && File.Exists(m_zipFileForANR))
		{
			ExceptionLogger.LogDebug("ANR occurred before");
			yield return new WaitForEndOfFrame();
			if (!Screenshot.CaptureScreenshot(ReportBuilder.Settings.m_maxScreenshotWidths[ExceptionSettings.ReportType.ANR], ScreenshotPathName))
			{
				m_zipFileForANR = string.Empty;
				yield break;
			}
			ZipUtil.AddFileToZip(Screenshot.ScreenshotPath, m_zipFileForANR, ReportBuilder.Settings.m_maxZipSizeLimits[ExceptionSettings.ReportType.ANR]);
			m_zipFileForANR = string.Empty;
			Screenshot.RemoveScreenshot();
			ReportExceptions();
		}
	}

	private void ReportExceptions()
	{
		lock (m_recordedExceptions)
		{
			ProcessPreviousExceptions();
			if (!m_started || m_recordedExceptions.m_records.Count + m_recordedExceptions.m_backupRecords.Count == 0)
			{
				return;
			}
		}
		m_monoBehaviour.StartCoroutine(SendInner());
	}

	private void ProcessPreviousExceptions()
	{
		if (!m_processedPreviousExceptions)
		{
			ExceptionLogger.LogInfo("Checking the exceptions from previous launch.");
			m_recordedExceptions.m_lastReadTimeLog = CallbackManager.CatchCrashCaptureFromLog(m_recordedExceptions.m_lastReadTimeLog, Application.identifier);
			SerializeRecordedExceptions();
			m_processedPreviousExceptions = true;
		}
	}

	private void DeserializeRecordedExceptions()
	{
		try
		{
			if (IsInDebugMode || !File.Exists(ExceptionReporterDataPath))
			{
				return;
			}
			lock (m_recordedExceptions)
			{
				string json = File.ReadAllText(ExceptionReporterDataPath);
				m_recordedExceptions = JsonUtility.FromJson<RecordedExceptions>(json);
				m_recordedExceptions.m_records.ForEach(delegate(ExceptionStruct x)
				{
					x.m_happenedBefore = true;
				});
				m_recordedExceptions.m_backupRecords.ForEach(delegate(ExceptionStruct x)
				{
					x.m_happenedBefore = true;
				});
				File.Delete(ExceptionReporterDataPath);
				MergeRecordedExceptions();
				ExceptionLogger.LogInfo("Loaded exception data from '" + ExceptionReporterDataPath + "'");
			}
		}
		catch (Exception ex)
		{
			ExceptionLogger.LogError("Failed to read exception data: " + ex.Message);
		}
	}

	private void SerializeRecordedExceptions()
	{
		try
		{
			lock (m_recordedExceptions)
			{
				string json = JsonUtility.ToJson(m_recordedExceptions);
				File.WriteAllText(ExceptionReporterDataPath, json);
				ExceptionLogger.LogInfo("Saved exception data(Count: {0}) to '{1}'", m_recordedExceptions.m_records.Count, ExceptionReporterDataPath);
			}
		}
		catch (Exception ex)
		{
			ExceptionLogger.LogError("Failed to write exception data to '{0}': {1}", ExceptionReporterDataPath, ex.Message);
		}
	}

	private void MakeZipAndRecordInner(ReportBuilder builder)
	{
		string zipFileName = GetZipName;
		try
		{
			this.BeforeZipping?.Invoke(builder.ReportType);
			byte[] zipBytes = ZipUtil.BuildZipArchive(builder);
			AfterZipping?.Invoke(builder.ReportType);
			File.WriteAllBytes(zipFileName, zipBytes);
		}
		catch (InsufficientMemoryException ex)
		{
			ExceptionLogger.LogError("Failed to zip because the file size is too big: " + ex.Message);
			zipFileName = string.Empty;
		}
		catch (Exception ex2)
		{
			ExceptionLogger.LogError("Failed to zip: " + ex2.Message);
		}
		Screenshot.RemoveScreenshot();
		lock (m_recordedExceptions)
		{
			ExceptionStruct record = new ExceptionStruct(builder.Hash, builder.Summary, builder.StackTrace, builder.ReportUUID, zipFileName, builder.ModifiedCodesInEditor, builder.HappenedBefore, !IsFakeReport && IsInDebugMode, builder.IsANR);
			if (!Busy)
			{
				m_recordedExceptions.m_records.Add(record);
			}
			else
			{
				m_recordedExceptions.m_backupRecords.Add(record);
			}
			if (builder.IsANR)
			{
				m_zipFileForANR = zipFileName;
			}
		}
		SerializeRecordedExceptions();
	}

	private IEnumerator MakeZipAndRecord(ReportBuilder builder, bool reportNow)
	{
		yield return new WaitForEndOfFrame();
		if (!builder.IsANR)
		{
			if (ReportBuilder.Settings.m_cnRegion)
			{
				ExceptionLogger.LogDebug("Skip creating a screenshot because it's restricted.");
			}
			else
			{
				Screenshot.CaptureScreenshot(ReportBuilder.Settings.m_maxScreenshotWidths[builder.ReportType], ScreenshotPathName);
			}
		}
		MakeZipAndRecordInner(builder);
		if (reportNow)
		{
			while (Busy)
			{
				yield return new WaitForSecondsRealtime(0.5f);
			}
			ReportExceptions();
		}
	}

	private IEnumerator SendInner()
	{
		ExceptionLogger.LogDebug("SendInner started");
		while (Busy)
		{
			yield return new WaitForSecondsRealtime(0.5f);
		}
		MergeRecordedExceptions();
		lock (m_recordedExceptions)
		{
			Busy = true;
			foreach (ExceptionStruct item in m_recordedExceptions.m_records)
			{
				if (m_started && File.Exists(item.m_zipName))
				{
					if (item.m_skipReport)
					{
						continue;
					}
					if (!item.m_message.StartsWith(s_titleOfCaughtException) && item.m_modifiedCodes != null)
					{
						if (ExceptionWithModifiedCodeCallback == null)
						{
							continue;
						}
						if (!item.m_happenedBefore && !item.m_isANR)
						{
							ExceptionWithModifiedCodeCallback(item.m_reportUUID, item.m_happenedBefore, item.m_isANR, item.m_modifiedCodes, delegate(bool report)
							{
								m_responseFromUser.Add(item.m_reportUUID, report);
							});
							bool wantToReport = false;
							while (!m_responseFromUser.TryGetValue(item.m_reportUUID, out wantToReport))
							{
								yield return new WaitForSecondsRealtime(0.5f);
							}
							m_responseFromUser.Remove(item.m_reportUUID);
							if (!wantToReport)
							{
								continue;
							}
						}
					}
					bool isCaughtException = item.m_message.StartsWith(s_titleOfCaughtException);
					bool isAssertion = item.m_message.StartsWith(s_titleOfAssertion);
					if (!IsFakeReport)
					{
						UnityWebRequest unityRequest = CreatePostWebRequest(item.m_zipName);
						yield return unityRequest.SendWebRequest();
						int statusCode = (int)unityRequest.responseCode;
						ExceptionLogger.LogDebug($"Response code: {statusCode}");
						if ((unityRequest.result == UnityWebRequest.Result.ProtocolError && statusCode != 404 && statusCode != 403) || unityRequest.result == UnityWebRequest.Result.ConnectionError)
						{
							UnregisterLogsCallback();
							ExceptionLogger.LogError("Unable to send error report: " + unityRequest.error);
						}
						else if (!isCaughtException && !isAssertion)
						{
							NotifyExceptionRecord?.Invoke(item.m_reportUUID, item.m_happenedBefore, item.m_isANR);
						}
						unityRequest.Dispose();
					}
					else if (!isAssertion)
					{
						NotifyExceptionRecord?.Invoke(item.m_reportUUID, item.m_happenedBefore, item.m_isANR);
					}
				}
				SafeDeleteFile(item.m_zipName);
			}
			m_recordedExceptions.m_records.Clear();
			SafeDeleteFile(ExceptionReporterDataPath);
			CleanSavedFiles();
			Busy = false;
		}
	}

	private UnityWebRequest CreatePostWebRequest(string zipFileName)
	{
		WWWForm form = new WWWForm();
		form.AddBinaryData("file", File.ReadAllBytes(zipFileName), "ReportedIssue.zip", "application/zip");
		UnityWebRequest unityWebRequest = UnityWebRequest.Post(ExceptionSubmitURL, form);
		unityWebRequest.useHttpContinue = false;
		return unityWebRequest;
	}

	private void MergeRecordedExceptions()
	{
		lock (m_recordedExceptions)
		{
			if (m_recordedExceptions.m_backupRecords.Count > 0)
			{
				m_recordedExceptions.m_records.AddRange(m_recordedExceptions.m_backupRecords);
				m_recordedExceptions.m_backupRecords.Clear();
			}
		}
	}

	private void CleanSavedFiles()
	{
		CleanAnyPrefixFiles(s_prefixOfZip, "zip");
	}

	private void CleanScreenshotFiles()
	{
		CleanAnyPrefixFiles(s_prefixOfScreenshot, "screenshot");
	}

	private void CleanAnyPrefixFiles(string prefix, string kind)
	{
		ExceptionLogger.LogInfo("Trying to clean up the old " + kind + " files.");
		string[] files;
		try
		{
			files = Directory.GetFiles(m_exceptionDir, prefix + "*");
		}
		catch (Exception ex)
		{
			ExceptionLogger.LogError("Failed to get the file list from " + m_exceptionDir + ": " + ex.Message);
			return;
		}
		string[] array = files;
		foreach (string f in array)
		{
			try
			{
				File.Delete(f);
			}
			catch (Exception ex2)
			{
				ExceptionLogger.LogError("Failed to delete the " + kind + " file '" + f + "': " + ex2.Message);
			}
		}
	}

	private void SafeDeleteFile(string filepath)
	{
		if (string.IsNullOrEmpty(filepath))
		{
			return;
		}
		try
		{
			if (File.Exists(filepath))
			{
				File.Delete(filepath);
			}
		}
		catch (Exception ex)
		{
			ExceptionLogger.LogError("Failed to delete the file '" + filepath + "': " + ex.Message);
		}
	}
}
