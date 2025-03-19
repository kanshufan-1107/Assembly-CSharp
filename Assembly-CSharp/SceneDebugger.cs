using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Assets;
using Blizzard.T5.Core.Time;
using Blizzard.T5.Core.Utils;
using Blizzard.T5.Jobs;
using Blizzard.T5.Logging;
using Blizzard.T5.Services;
using Hearthstone;
using Hearthstone.Core;
using Hearthstone.Progression;
using Hearthstone.Streaming;
using PegasusGame;
using PegasusUtil;
using UnityEngine;
using UnityEngine.Networking;

public class SceneDebugger : IService, IHasUpdate
{
	public class ConsoleLogEntry : LoggerDebugWindow.LogEntry
	{
		public ConsoleLogEntry(LogLevel level, string message)
		{
			category = level;
			message = message.Trim();
			switch (level)
			{
			case LogLevel.Debug:
				message = $"<color=grey>{message}</color>";
				break;
			case LogLevel.Warning:
				message = $"<color=yellow>{message}</color>";
				break;
			case LogLevel.Error:
				message = $"<color=red>{message}</color>";
				break;
			}
			DateTime timestamp = DateTime.Now;
			string timeString = $"<color=grey>[{timestamp.Hour.ToString().PadLeft(2, '0')}:{timestamp.Minute.ToString().PadLeft(2, '0')}:{timestamp.Second.ToString().PadLeft(2, '0')}]</color>";
			text = $"{timeString} {message}";
		}
	}

	private class SlushTimeRecord : LoggerDebugWindow.LogEntry
	{
		public float ExpectedStart { get; set; }

		public float ExpectedEnd { get; set; }

		public int TaskId { get; set; }

		public float ActualStart { get; set; }

		public float ActualEnd { get; set; }

		public int EntityId { get; set; }

		public SlushTimeRecord(int taskId, float expectedStart, float expectedEnd, float actualStart = 0f, float actualEnd = 0f, int entityId = 0)
		{
			TaskId = taskId;
			ExpectedStart = expectedStart;
			ExpectedEnd = expectedEnd;
			ActualStart = actualStart;
			ActualEnd = actualEnd;
			EntityId = entityId;
			text = ToString();
		}

		private float GetDuration(float start, float end)
		{
			return end - start;
		}

		public override string ToString()
		{
			float expectedDuration = GetDuration(ExpectedStart, ExpectedEnd);
			float duration = GetDuration(ActualStart, ActualEnd);
			float startDiff = ActualStart - ExpectedStart;
			float diff = duration - expectedDuration;
			diff += startDiff;
			string sign = ((diff > 0f) ? "+" : "");
			string sourceEntity = "";
			if (EntityId != 0)
			{
				Entity source = GameState.Get().GetEntity(EntityId);
				if (source != null)
				{
					sourceEntity = source.GetName();
				}
			}
			return $"TaskId: {TaskId}, ({sourceEntity}) {sign}{diff}";
		}
	}

	private class ScriptWarning : LoggerDebugWindow.LogEntry
	{
		public string Source { get; private set; }

		public string Event { get; private set; }

		public string Message { get; private set; }

		public int Severity { get; set; }

		public string PowerDef { get; private set; }

		public int PC { get; private set; }

		public string IssueGUID { get; private set; }

		public ScriptWarning(string logSource, string logEvent, string logMessage)
		{
			Source = logSource;
			Event = logEvent;
			Message = logMessage;
			Severity = -1;
			PowerDef = "";
			PC = -1;
			IssueGUID = "";
			RebuildString();
		}

		public void SetPowerDefInfo(string powerDef, string pc)
		{
			if (powerDef.Length >= 0 && pc.Length >= 0 && int.TryParse(pc, out var parsedPC))
			{
				PowerDef = powerDef;
				PC = parsedPC;
				RebuildString();
			}
		}

		public string ComputeIssueGUID()
		{
			string hashString = "";
			if (PowerDef.Length > 0 && PC >= 0)
			{
				hashString = $"{Event}|{PowerDef}|{PC}";
			}
			else if (Source.Length > 0)
			{
				hashString = $"{Event}|{Source}";
			}
			if (hashString.Length <= 0)
			{
				return "";
			}
			byte[] hash = MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(hashString));
			IssueGUID = Convert.ToBase64String(hash);
			RebuildString();
			return IssueGUID;
		}

		public void RebuildString()
		{
			StringBuilder scriptWarningsBuilder = new StringBuilder();
			scriptWarningsBuilder.AppendLine($"<color=red>-> [{Event}]</color>");
			if (Source.Length > 0)
			{
				scriptWarningsBuilder.AppendLine($"    -source={Source}");
			}
			string[] array = Message.Split('|');
			foreach (string keyValue in array)
			{
				if (keyValue.Length > 0)
				{
					scriptWarningsBuilder.AppendLine($"    -{keyValue}");
				}
			}
			if (IssueGUID.Length > 0)
			{
				scriptWarningsBuilder.AppendLine($"    -(guid: {IssueGUID})");
			}
			text = scriptWarningsBuilder.ToString();
		}

		public override string ToString()
		{
			return $"Received script warning from '{Source}'!  event:[{Event}]  message:\"{Message}\"  guid:({IssueGUID})";
		}
	}

	private readonly Vector2 m_GUISize;

	private float m_UpdateInterval;

	private double m_LastInterval;

	private int m_frames;

	private string m_fpsText;

	private static readonly LogLevel[] LOG_LEVELS_TO_DISPLAY = new LogLevel[3]
	{
		LogLevel.Info,
		LogLevel.Warning,
		LogLevel.Error
	};

	private bool m_enableSceneDebugger;

	private bool m_testMessaging;

	private DebuggerGuiWindow m_guiWindow;

	private DebuggerGuiWindow m_ratingWindow;

	private DebuggerGuiWindow m_assetsWindow;

	private DebuggerGuiWindow m_gameplayWindow;

	private DebuggerGuiWindow m_presenceWindow;

	private DebuggerGuiWindow m_questWindow;

	private DebuggerGuiWindow m_achievementWindow;

	private DebuggerGuiWindow m_rewardTrackWindow;

	private LoggerDebugWindow m_messageWindow;

	private LoggerDebugWindow m_serverLogWindow;

	private CheatsDebugWindow m_cheatsWindow;

	private LoggerDebugWindow m_slushTrackerWindow;

	private DebuggerGuiWindow m_notepadWindow;

	private string m_notepadContents;

	private bool m_notepadFirstRun;

	private Vector2 scrollViewVector;

	private DebuggerGui m_timeSection;

	private DebuggerGui m_qualitySection;

	private DebuggerGui m_statsSection;

	private bool m_showGuiCustomization;

	private int m_guiSaveTimer;

	private List<DebuggerGui> m_debuggerGui;

	private long? m_playerId;

	private MedalInfoData m_debugMedalInfo;

	private float m_lastMedalInfoRequestTime;

	private IGraphicsManager m_graphicsManager;

	public IEnumerator<IAsyncJobResult> Initialize(ServiceLocator serviceLocator)
	{
		m_graphicsManager = ServiceManager.Get<IGraphicsManager>();
		TimeScaleMgr.Get().SetTimeScaleMultiplier(GetDevTimescaleMultiplier());
		Vector2 screenDims = GetScaledScreen();
		m_guiWindow = new DebuggerGuiWindow("Scene Debugger", LayoutGuiControls, canClose: false, canResize: false);
		m_guiWindow.Position = new Vector2(screenDims.x * 0.05f, screenDims.y * 0.125f);
		m_timeSection = new DebuggerGui("Time Scale", LayoutTimeControls);
		m_qualitySection = new DebuggerGui("Quality", LayoutQualityControls);
		m_statsSection = new DebuggerGui("Stats", LayoutStats);
		m_cheatsWindow = new CheatsDebugWindow(m_GUISize);
		m_cheatsWindow.Position = new Vector2(screenDims.x * 0.5f, 0f);
		m_cheatsWindow.ResizeToFit(screenDims.x * 0.5f, screenDims.y * 0.5f);
		m_cheatsWindow.collapsedWidth = m_GUISize.x;
		m_messageWindow = new LoggerDebugWindow("Messages", m_GUISize, Enum.GetValues(typeof(LogLevel)).Cast<object>());
		m_messageWindow.CustomLayout = LayoutMessages;
		m_messageWindow.collapsedWidth = m_GUISize.x;
		m_messageWindow.Position = new Vector2(0f, 0.65f * screenDims.y - 35f);
		m_messageWindow.ResizeToFit(screenDims.x, screenDims.y * 0.35f);
		m_serverLogWindow = new LoggerDebugWindow("Server Script Log", m_GUISize, Enum.GetValues(typeof(ServerLogs.ServerLogLevel)).Cast<object>());
		m_serverLogWindow.CustomLayout = LayoutScriptWarnings;
		m_serverLogWindow.collapsedWidth = m_GUISize.x;
		m_serverLogWindow.Position = new Vector2(0f, 0.65f * screenDims.y - 35f);
		m_serverLogWindow.ResizeToFit(screenDims.x, screenDims.y * 0.35f);
		m_ratingWindow = new DebuggerGuiWindow("Rating", LayoutRatingDebug, canClose: true, canResize: false);
		m_ratingWindow.collapsedWidth = m_GUISize.x;
		m_ratingWindow.Position = new Vector2(screenDims.x - m_GUISize.x, 0.5f * screenDims.y);
		m_ratingWindow.ResizeToFit(m_GUISize.x, m_GUISize.y);
		serviceLocator.Get<Network>().RegisterNetHandler(DebugRatingInfoResponse.PacketID.ID, OnDebugRatingInfoResponse);
		m_assetsWindow = new DebuggerGuiWindow("Assets", LayoutAssetsDebug);
		m_assetsWindow.collapsedWidth = m_GUISize.x;
		m_assetsWindow.Position = new Vector2(screenDims.x - m_GUISize.x, 0.5f * screenDims.y);
		m_assetsWindow.ResizeToFit(m_GUISize.x, m_GUISize.y);
		m_gameplayWindow = new DebuggerGuiWindow("Gameplay", LayoutGameplayDebug);
		m_gameplayWindow.collapsedWidth = m_GUISize.x;
		m_gameplayWindow.Position = new Vector2(screenDims.x - m_GUISize.x, 0.5f * screenDims.y);
		m_gameplayWindow.ResizeToFit(m_GUISize.x, m_GUISize.y);
		m_presenceWindow = new DebuggerGuiWindow("Presence", LayoutPresenceDebug);
		m_presenceWindow.collapsedWidth = m_GUISize.x;
		m_presenceWindow.Position = new Vector2(screenDims.x - m_GUISize.x, 0.5f * screenDims.y);
		m_presenceWindow.ResizeToFit(m_GUISize.x, m_GUISize.y);
		float questWidth = m_GUISize.x * 2f;
		m_questWindow = new DebuggerGuiWindow("Quest", LayoutQuestDebug, canClose: true, canResize: false);
		m_questWindow.collapsedWidth = m_GUISize.x;
		m_questWindow.Position = new Vector2(screenDims.x - questWidth, 0.5f * screenDims.y);
		m_questWindow.ResizeToFit(questWidth, m_GUISize.y);
		float achievementWidth = m_GUISize.x * 2.5f;
		m_achievementWindow = new DebuggerGuiWindow("Achievement", LayoutAchievementDebug, canClose: true, canResize: false);
		m_achievementWindow.collapsedWidth = m_GUISize.x;
		m_achievementWindow.Position = new Vector2(screenDims.x - achievementWidth, 0.5f * screenDims.y);
		m_achievementWindow.ResizeToFit(achievementWidth, m_GUISize.y);
		float rewardTrackWidth = m_GUISize.x * 2f;
		m_rewardTrackWindow = new DebuggerGuiWindow("Reward Track", LayoutRewardTrackDebug, canClose: true, canResize: false);
		m_rewardTrackWindow.collapsedWidth = m_GUISize.x;
		m_rewardTrackWindow.Position = new Vector2(screenDims.x - rewardTrackWidth, 0.5f * screenDims.y);
		m_rewardTrackWindow.ResizeToFit(rewardTrackWidth, m_GUISize.y);
		m_slushTrackerWindow = new LoggerDebugWindow("Slush Time Log", m_GUISize, Enum.GetValues(typeof(LogLevel)).Cast<object>());
		m_slushTrackerWindow.collapsedWidth = m_GUISize.x;
		m_slushTrackerWindow.Position = new Vector2(0f, 0.65f * screenDims.y - 35f);
		m_slushTrackerWindow.ResizeToFit(screenDims.x, screenDims.y * 0.35f);
		float notepadWidth = m_GUISize.x * 2f;
		m_notepadWindow = new DebuggerGuiWindow("Notepad", LayoutNotepadDebug);
		m_notepadWindow.collapsedWidth = m_GUISize.x;
		m_notepadWindow.Position = new Vector2(screenDims.x - notepadWidth, 0.5f * screenDims.y);
		m_notepadWindow.ResizeToFit(notepadWidth, m_GUISize.y);
		m_debuggerGui = new List<DebuggerGui>();
		m_debuggerGui.Add(m_guiWindow);
		m_debuggerGui.Add(m_cheatsWindow);
		if (m_messageWindow != null)
		{
			m_debuggerGui.Add(m_messageWindow);
		}
		m_debuggerGui.Add(m_serverLogWindow);
		m_debuggerGui.Add(m_ratingWindow);
		m_debuggerGui.Add(m_assetsWindow);
		m_debuggerGui.Add(m_questWindow);
		m_debuggerGui.Add(m_achievementWindow);
		m_debuggerGui.Add(m_rewardTrackWindow);
		m_debuggerGui.Add(m_timeSection);
		m_debuggerGui.Add(m_qualitySection);
		m_debuggerGui.Add(m_statsSection);
		m_debuggerGui.Add(m_slushTrackerWindow);
		m_debuggerGui.Add(m_notepadWindow);
		m_debuggerGui.Add(m_gameplayWindow);
		m_debuggerGui.Add(m_presenceWindow);
		foreach (DebuggerGui item in m_debuggerGui)
		{
			item.OnChanged += HandleGuiChanged;
		}
		m_guiWindow.IsShown = true;
		m_cheatsWindow.IsShown = false;
		if (m_messageWindow != null)
		{
			m_messageWindow.IsShown = false;
		}
		m_serverLogWindow.IsShown = false;
		m_ratingWindow.IsShown = false;
		m_assetsWindow.IsShown = false;
		m_gameplayWindow.IsShown = false;
		m_presenceWindow.IsShown = false;
		m_questWindow.IsShown = false;
		m_achievementWindow.IsShown = false;
		m_rewardTrackWindow.IsShown = false;
		m_slushTrackerWindow.IsShown = false;
		m_notepadWindow.IsShown = false;
		DebuggerGui.LoadConfig(m_debuggerGui);
		OnGUIDelegateComponent.CreateGUIDelegate(OnGUI);
		yield break;
	}

	public Type[] GetDependencies()
	{
		return new Type[2]
		{
			typeof(IGraphicsManager),
			typeof(Network)
		};
	}

	public void Shutdown()
	{
	}

	public bool IsMouseOverGui()
	{
		if (!Options.Get().GetBool(Option.HUD))
		{
			return false;
		}
		foreach (DebuggerGui gui in m_debuggerGui)
		{
			if (gui is DebuggerGuiWindow window && gui.IsShown && window.IsMouseOver())
			{
				return true;
			}
		}
		return false;
	}

	public void Update()
	{
		m_frames++;
		float currentTime = Time.realtimeSinceStartup;
		if ((double)currentTime > m_LastInterval + (double)m_UpdateInterval)
		{
			float fps = (float)m_frames / (float)((double)currentTime - m_LastInterval);
			m_fpsText = $"{SystemInfo.graphicsDeviceType}  FPS: {fps:f2}\nFrame Time:{1000f / fps:f2}ms\nScreen: {Screen.width} x {Screen.height}\n";
			m_frames = 0;
			m_LastInterval = Time.realtimeSinceStartup;
		}
		if (m_testMessaging)
		{
			string baseChars = "abcdefghijklmnopqrstuvwxyz0123456789";
			StringBuilder sb = new StringBuilder();
			for (int i = 0; i < 5000; i++)
			{
				char ch = baseChars[UnityEngine.Random.Range(0, baseChars.Length)];
				sb.Append(ch);
			}
			AddErrorMessage(sb.ToString());
		}
	}

	private void OnGUI()
	{
		if (ScriptDebugDisplay.Get().m_isDisplayed || !m_enableSceneDebugger || !Options.Get().GetBool(Option.HUD))
		{
			return;
		}
		float guiScaling = GetGuiScaling();
		GUI.matrix = Matrix4x4.Scale(new Vector3(guiScaling, guiScaling, guiScaling));
		if (GameState.Get() != null && GameState.Get().GetSlushTimeTracker().GetAccruedLostTimeInSeconds() > (float)GameplayDebug.LOST_SLUSH_TIME_ERROR_THRESHOLD_SECONDS)
		{
			m_gameplayWindow.IsShown = true;
		}
		m_guiWindow.Layout();
		m_cheatsWindow.Layout();
		if (m_messageWindow != null)
		{
			m_messageWindow.Layout();
		}
		m_serverLogWindow.Layout();
		m_ratingWindow.Layout();
		m_assetsWindow.Layout();
		m_gameplayWindow.Layout();
		m_presenceWindow.Layout();
		m_questWindow.Layout();
		m_achievementWindow.Layout();
		m_rewardTrackWindow.Layout();
		m_slushTrackerWindow.Layout();
		m_notepadWindow.Layout();
		LayoutCursorDebug();
		if (m_guiSaveTimer > 0)
		{
			m_guiSaveTimer--;
			if (m_guiSaveTimer == 0)
			{
				DebuggerGui.SaveConfig(m_debuggerGui);
			}
		}
	}

	private float GetGuiScaling()
	{
		float hudScale = 1f;
		GeneralUtils.TryParseFloat(Options.Get().GetOption(Option.HUD_SCALE).ToString(), out hudScale);
		float scalingHeight = Screen.height;
		switch (PlatformSettings.Screen)
		{
		case ScreenCategory.Phone:
			scalingHeight = 480f;
			break;
		case ScreenCategory.MiniTablet:
			scalingHeight = 576f;
			break;
		case ScreenCategory.Tablet:
			scalingHeight = 640f;
			break;
		}
		return Mathf.Max(0.1f, hudScale) * Mathf.Max(1f, (float)Screen.height / scalingHeight);
	}

	private Vector2 GetScaledScreen()
	{
		return new Vector2(Screen.width, Screen.height) / GetGuiScaling();
	}

	public static SceneDebugger Get()
	{
		return ServiceManager.Get<SceneDebugger>();
	}

	public static float GetDevTimescaleMultiplier()
	{
		if (HearthstoneApplication.IsPublic())
		{
			return 1f;
		}
		return Options.Get().GetFloat(Option.DEV_TIMESCALE, 1f);
	}

	public static void SetDevTimescaleMultiplier(float f)
	{
		if (!HearthstoneApplication.IsPublic() && f != TimeScaleMgr.Get().GetTimeScaleMultiplier())
		{
			if (f == 0f)
			{
				f = 0.0001f;
			}
			Options.Get().SetFloat(Option.DEV_TIMESCALE, f);
			TimeScaleMgr.Get().SetTimeScaleMultiplier(f);
		}
	}

	public void SetPlayerId(long? playerId)
	{
		m_playerId = playerId;
	}

	public long? GetPlayerId_DebugOnly()
	{
		return m_playerId;
	}

	public void AddMessage(string message)
	{
		AddMessage(LogLevel.Info, message);
	}

	public void AddMessage(LogLevel level, string message, bool autoShow = false)
	{
		if (m_messageWindow != null && Array.Exists(LOG_LEVELS_TO_DISPLAY, (LogLevel l) => l == level))
		{
			m_messageWindow.AddEntry(new ConsoleLogEntry(level, message), autoShow);
		}
		if (message.Contains("spawncard"))
		{
			m_messageWindow.IsShown = true;
			m_messageWindow.IsExpanded = true;
		}
	}

	public void AddErrorMessage(string message)
	{
		AddMessage(LogLevel.Error, message);
	}

	public void AddSlushTimeEntry(int taskId, float expectedStart, float expectedEnd, float actualStart = 0f, float actualEnd = 0f, int entityId = 0)
	{
		m_slushTrackerWindow.AddEntry(new SlushTimeRecord(taskId, expectedStart, expectedEnd, actualStart, actualEnd, entityId));
	}

	public void AddServerScriptLogMessage(ScriptLogMessage message)
	{
		int minSeverity = 3;
		if (message.Severity >= minSeverity && m_serverLogWindow.GetEntries().Count((LoggerDebugWindow.LogEntry m) => (m as ScriptWarning).Severity >= minSeverity) == 0)
		{
			m_serverLogWindow.IsShown = true;
			m_serverLogWindow.IsExpanded = true;
		}
		string source = "";
		string powerDef = "";
		string pc = "";
		string entity = "";
		StringBuilder modifiedMessageBuilder = new StringBuilder();
		string[] array = message.Message.Split('|');
		foreach (string keyValue in array)
		{
			if (keyValue.Length <= 0)
			{
				continue;
			}
			if (keyValue.StartsWith("source="))
			{
				Match messageMatch = Regex.Match(keyValue, ".*source=(?<source>[^\\(]+) \\(ID=(?<entityId>[0-9]+)( CardID=(?<cardId>[^\\)]*))?\\).*");
				source = ((!messageMatch.Success) ? keyValue.Substring(7) : ((messageMatch.Groups["cardId"].Length <= 0) ? string.Format("{0}", messageMatch.Groups["source"]) : string.Format("{0} ({1})", messageMatch.Groups["source"], messageMatch.Groups["cardId"])));
				continue;
			}
			if (keyValue.StartsWith("powerDef="))
			{
				powerDef = keyValue.Substring(9);
			}
			else if (keyValue.StartsWith("pc="))
			{
				pc = keyValue.Substring(3);
			}
			else if (keyValue.StartsWith("entity="))
			{
				Match messageMatch2 = Regex.Match(keyValue, ".*entity=(?<source>[^\\(]+) \\(ID=(?<entityId>[0-9]+)( CardID=(?<cardId>[^\\)]*))?\\).*");
				if (messageMatch2.Success)
				{
					entity = ((messageMatch2.Groups["cardId"].Length <= 0) ? string.Format("{0}", messageMatch2.Groups["source"]) : string.Format("{0} ({1})", messageMatch2.Groups["source"], messageMatch2.Groups["cardId"]));
				}
			}
			modifiedMessageBuilder.AppendFormat("{0}|", keyValue);
		}
		ScriptWarning newScriptWarning = new ScriptWarning((source.Length > 0) ? source : entity, message.Event, modifiedMessageBuilder.ToString());
		if (message.HasSeverity)
		{
			newScriptWarning.Severity = message.Severity;
		}
		newScriptWarning.SetPowerDefInfo(powerDef, pc);
		newScriptWarning.ComputeIssueGUID();
		m_serverLogWindow.AddEntry(newScriptWarning);
		string logLine = newScriptWarning.ToString();
		Log.Gameplay.PrintWarning(logLine);
		Debug.LogWarning(logLine);
	}

	private Rect LayoutGuiControls(Rect space)
	{
		space.width = m_GUISize.x;
		space.yMax = GetScaledScreen().y;
		float top = space.yMin;
		Rect header = m_guiWindow.GetHeaderRect();
		if (GUI.Button(new Rect(header.xMax - header.height, header.y, header.height, header.height), "☰"))
		{
			m_showGuiCustomization = !m_showGuiCustomization;
		}
		if (m_showGuiCustomization)
		{
			space = LayoutCustomizeMenu(space);
		}
		else
		{
			space = m_timeSection.Layout(space);
			space = m_qualitySection.Layout(space);
			space = m_statsSection.Layout(space);
		}
		m_guiWindow.ResizeToFit(space.width, space.yMin - top);
		return new Rect(space.xMin, space.yMax, space.width, 0f);
	}

	private void LayoutCursorDebug()
	{
		if (Options.Get() == null || !(PegUI.Get() != null) || !Options.Get().GetBool(Option.DEBUG_CURSOR) || !HearthstoneApplication.IsInternal())
		{
			return;
		}
		RaycastHit raycastHit;
		PegUIElement hitElement = PegUI.Get().FindHitElement(out raycastHit);
		string hitElementAsString = "none";
		UnityEngine.Object hitObj = raycastHit.collider;
		if (hitObj != null)
		{
			string cameraHitStr = string.Empty;
			if (PegUI.Get().IsUsingRenderPassPriorityHitTest)
			{
				cameraHitStr = ", HitTestCamera=" + ((PegUI.Get().LastCameraPriorityHitCamera != null) ? PegUI.Get().LastCameraPriorityHitCamera.name : "none");
			}
			hitElementAsString = $"<color=#FFFFFF>{hitObj.GetType().ToString()}: {DebugUtils.GetHierarchyPath(hitObj, '/')}\nObjLayer={(GameLayer)raycastHit.collider.gameObject.layer}, HasPegUI={hitElement != null}, RenderPassPriority={PegUI.Get().IsUsingRenderPassPriorityHitTest}{cameraHitStr}</color>";
		}
		Vector2 screenDims = GetScaledScreen();
		GUIStyle rightStyle = new GUIStyle("box")
		{
			fontSize = GUI.skin.button.fontSize,
			fontStyle = GUI.skin.button.fontStyle,
			alignment = TextAnchor.UpperLeft,
			wordWrap = true,
			clipping = TextClipping.Overflow,
			stretchWidth = true,
			richText = true
		};
		GUI.Box(new Rect(screenDims.x / 2f, 0f, screenDims.x / 2f, m_GUISize.y * 3f), hitElementAsString, rightStyle);
	}

	private Rect LayoutTimeControls(Rect space)
	{
		SetDevTimescaleMultiplier(GUI.HorizontalSlider(new Rect(space.min, m_GUISize), TimeScaleMgr.Get().GetTimeScaleMultiplier(), 0.01f, 4f));
		space.yMin += 0.5f * m_GUISize.y;
		GUI.Box(new Rect(space.min, m_GUISize), $"Time Scale: {TimeScaleMgr.Get().GetTimeScaleMultiplier()}");
		space.yMin += 0.75f * m_GUISize.y;
		if (GUI.Button(new Rect(space.min, m_GUISize), "Reset Time Scale"))
		{
			SetDevTimescaleMultiplier(1f);
		}
		space.yMin += 1.1f * m_GUISize.y;
		return space;
	}

	private Rect LayoutQualityControls(Rect space)
	{
		if (m_graphicsManager == null)
		{
			return space;
		}
		string low = "Low";
		if (m_graphicsManager.RenderQualityLevel == GraphicsQuality.Low)
		{
			low = "<color=cyan>Low</color>";
		}
		string medium = "Medium";
		if (m_graphicsManager.RenderQualityLevel == GraphicsQuality.Medium)
		{
			medium = "<color=cyan>Medium</color>";
		}
		string high = "High";
		if (m_graphicsManager.RenderQualityLevel == GraphicsQuality.High)
		{
			high = "<color=cyan>High</color>";
		}
		float width = space.width / 3f;
		if (GUI.Button(new Rect(space.xMin, space.yMin, width, m_GUISize.y), low))
		{
			m_graphicsManager.RenderQualityLevel = GraphicsQuality.Low;
		}
		if (GUI.Button(new Rect(space.xMin + width, space.yMin, width, m_GUISize.y), medium))
		{
			m_graphicsManager.RenderQualityLevel = GraphicsQuality.Medium;
		}
		if (GUI.Button(new Rect(space.xMin + width * 2f, space.yMin, width, m_GUISize.y), high))
		{
			m_graphicsManager.RenderQualityLevel = GraphicsQuality.High;
		}
		space.yMin += m_GUISize.y;
		return space;
	}

	private Rect LayoutStats(Rect space)
	{
		float lineHeight = GUI.skin.box.lineHeight;
		float boxPadding = GUI.skin.box.border.vertical;
		float height = 3f * lineHeight + boxPadding;
		GUI.Box(new Rect(space.xMin, space.yMin, m_GUISize.x, height), m_fpsText);
		space.yMin += height;
		string statStr = string.Format("Build: {0}.{1}\nServer: {2}", "31.4", 214839, Network.GetVersion());
		height = 2f * lineHeight + boxPadding;
		IGameDownloadManager downloadMgr = GameDownloadManagerProvider.Get();
		if ((PlatformSettings.IsMobileRuntimeOS || (Application.isEditor && PlatformSettings.IsEmulating)) && downloadMgr != null)
		{
			string downloaderOverrides = GetDownloadOverrideString(downloadMgr);
			statStr += downloaderOverrides;
			height += lineHeight;
		}
		if (HearthstoneApplication.IsInternal() && m_playerId.HasValue)
		{
			statStr += $"\nPlayer Id: {m_playerId}";
			height += lineHeight;
		}
		if (!string.IsNullOrEmpty(Network.GetUsername()))
		{
			statStr += $"\nAccount: {Network.GetUsername().Split('@')[0]}";
			height += lineHeight;
		}
		GUI.Box(new Rect(space.xMin, space.yMin, m_GUISize.x, height), statStr);
		space.yMin += height;
		if (Application.isEditor && AssetLoaderPrefs.AssetLoadingMethod == AssetLoaderPrefs.ASSET_LOADING_METHOD.ASSET_BUNDLES)
		{
			GUI.Box(new Rect(space.min, m_GUISize), "<color=red>Using Asset Bundles</color>");
			space.yMin += m_GUISize.y;
		}
		return space;
	}

	private string GetDownloadOverrideString(IGameDownloadManager downloadMgr)
	{
		string versionOverride = downloadMgr.VersionOverrideUrl;
		bool versionIsLive = versionOverride.Equals("Live");
		if (versionIsLive)
		{
			return "\nVerSrv: Live";
		}
		string returnValue = "";
		if (!versionIsLive)
		{
			returnValue += $"\nVersionSrv: {versionOverride}";
		}
		return returnValue;
	}

	private Rect LayoutMessages(Rect space)
	{
		Rect guiRect = new Rect(space.min, m_GUISize);
		if (GUI.Button(guiRect, $"Clear ({m_messageWindow.GetEntries().Count((LoggerDebugWindow.LogEntry m) => m_messageWindow.AreLogsDisplayed(m.category))})"))
		{
			m_messageWindow.Clear();
		}
		guiRect.xMin = guiRect.xMax + 10f;
		guiRect.width = 40f;
		GUI.Label(new Rect(guiRect), "Filter:");
		guiRect.xMin = guiRect.xMax;
		guiRect.xMax = space.xMax - 100f * (float)LOG_LEVELS_TO_DISPLAY.Count();
		m_messageWindow.FilterString = GUI.TextField(guiRect, m_messageWindow.FilterString);
		LogLevel[] lOG_LEVELS_TO_DISPLAY = LOG_LEVELS_TO_DISPLAY;
		for (int i = 0; i < lOG_LEVELS_TO_DISPLAY.Length; i++)
		{
			LogLevel logLevel = lOG_LEVELS_TO_DISPLAY[i];
			guiRect.xMin = guiRect.xMax;
			guiRect.width = 100f;
			bool isShown = m_messageWindow.AreLogsDisplayed(logLevel);
			int count = m_messageWindow.GetCount(logLevel);
			string label = string.Format("<color={0}>{1} ({2})</color>", isShown ? "white" : "grey", logLevel.ToString(), count);
			if (GUI.Button(guiRect, label))
			{
				m_messageWindow.ToggleLogsDisplay(logLevel, !isShown);
			}
		}
		space.yMin = guiRect.yMax;
		return m_messageWindow.LayoutLog(space);
	}

	private Rect LayoutRatingDebug(Rect space)
	{
		StringBuilder sb = new StringBuilder();
		if (m_debugMedalInfo != null)
		{
			sb.AppendLine($"{(RatingDebugOption)m_debugMedalInfo.RatingId}");
			sb.AppendLine($"Rating ID: {m_debugMedalInfo.RatingId}");
			sb.AppendLine($"Rating: {m_debugMedalInfo.Rating}");
			sb.AppendLine($"Variance: {m_debugMedalInfo.Variance}");
			sb.Append($"Public Rating: {m_debugMedalInfo.PublicRating}");
			if (m_debugMedalInfo.LeagueId != 0)
			{
				sb.AppendLine("\n");
				sb.AppendLine($"League ID: {m_debugMedalInfo.LeagueId}");
				sb.AppendLine($"Season ID: {m_debugMedalInfo.SeasonId}");
				sb.AppendLine($"Games: {m_debugMedalInfo.SeasonGames}");
				sb.AppendLine($"Wins: {m_debugMedalInfo.SeasonWins}");
				sb.AppendLine($"Streak: {m_debugMedalInfo.Streak}");
				sb.AppendLine(string.Empty);
				sb.AppendLine($"Stars Per Win: {m_debugMedalInfo.StarsPerWin}");
				sb.AppendLine($"Star Level: {m_debugMedalInfo.StarLevel}");
				sb.AppendLine($"Stars: {m_debugMedalInfo.Stars}");
				sb.AppendLine($"LegendRank: {m_debugMedalInfo.LegendRank}");
				sb.AppendLine(string.Empty);
				sb.AppendLine($"Best Star Level: {m_debugMedalInfo.BestStarLevel}");
				sb.AppendLine($"Best Stars: {m_debugMedalInfo.BestStars}");
				sb.AppendLine($"Best Rating: {m_debugMedalInfo.BestRating}");
				sb.AppendLine(string.Empty);
				sb.AppendLine($"Best Ever League ID: {m_debugMedalInfo.BestEverLeagueId}");
				sb.Append($"Best Ever Star Level: {m_debugMedalInfo.BestEverStarLevel}");
			}
		}
		string statStr = sb.ToString();
		GUIStyle style = new GUIStyle(GUI.skin.box);
		style.alignment = TextAnchor.MiddleLeft;
		GUIContent content = new GUIContent(statStr);
		space.height = style.CalcHeight(content, space.width);
		GUI.Box(space, statStr, style);
		float buttonHeight = m_GUISize.y;
		if (GUI.Button(new Rect(space.xMin, space.yMax, space.width, buttonHeight), "Refresh") || Time.realtimeSinceStartup - m_lastMedalInfoRequestTime >= 5f)
		{
			RequestDebugRatingInfo();
		}
		space.yMax += buttonHeight;
		m_ratingWindow.ResizeToFit(new Vector2(space.width, space.height));
		space.yMin = space.yMax;
		return space;
	}

	public void RequestDebugRatingInfo()
	{
		int ratingId = (int)Options.Get().GetEnum<RatingDebugOption>(Option.RATING_DEBUG);
		Network.Get().SetDebugRatingInfo(ratingId);
		m_lastMedalInfoRequestTime = Time.realtimeSinceStartup;
	}

	private void OnDebugRatingInfoResponse()
	{
		DebugRatingInfoResponse response = Network.Get().GetDebugRatingInfoResponse();
		if (response != null)
		{
			m_debugMedalInfo = response.MedalData;
		}
	}

	private Rect LayoutAssetsDebug(Rect space)
	{
		space = AssetLoaderDebug.LayoutUI(space);
		m_assetsWindow.ResizeToFit(new Vector2(space.width, space.height));
		return space;
	}

	private Rect LayoutGameplayDebug(Rect space)
	{
		space = GameplayDebug.LayoutUI(space);
		m_gameplayWindow.ResizeToFit(new Vector2(space.width, space.height));
		return space;
	}

	private Rect LayoutPresenceDebug(Rect space)
	{
		space = PresenceDebug.LayoutUI(space);
		m_gameplayWindow.ResizeToFit(new Vector2(space.width, space.height));
		return space;
	}

	private Rect LayoutQuestDebug(Rect space)
	{
		string contentStr = QuestManager.Get()?.GetQuestDebugHudString() ?? string.Empty;
		GUIStyle style = new GUIStyle(GUI.skin.box);
		style.alignment = TextAnchor.MiddleLeft;
		GUIContent content = new GUIContent(contentStr);
		space.height = style.CalcHeight(content, space.width);
		GUI.Box(space, contentStr, style);
		float buttonHeight = m_GUISize.y;
		if (GUI.Button(new Rect(space.xMin, space.yMax, space.width, buttonHeight), "Copy to Clipboard"))
		{
			ClipboardUtils.CopyToClipboard(contentStr);
		}
		space.yMax += buttonHeight;
		m_questWindow.ResizeToFit(new Vector2(space.width, space.height));
		space.yMin = space.yMax;
		return space;
	}

	private Rect LayoutAchievementDebug(Rect space)
	{
		string contentStr = AchievementManager.Get()?.Debug_GetAchievementHudString() ?? string.Empty;
		GUIStyle style = new GUIStyle(GUI.skin.box);
		style.alignment = TextAnchor.MiddleLeft;
		GUIContent content = new GUIContent(contentStr);
		space.height = style.CalcHeight(content, space.width);
		GUI.Box(space, contentStr, style);
		float buttonHeight = m_GUISize.y;
		if (GUI.Button(new Rect(space.xMin, space.yMax, space.width, buttonHeight), "Copy to Clipboard"))
		{
			ClipboardUtils.CopyToClipboard(contentStr);
		}
		space.yMax += buttonHeight;
		m_achievementWindow.ResizeToFit(new Vector2(space.width, space.height));
		space.yMin = space.yMax;
		return space;
	}

	private Rect LayoutRewardTrackDebug(Rect space)
	{
		string contentStr = RewardTrackManager.Get()?.GetRewardTrack(Global.RewardTrackType.GLOBAL)?.GetRewardTrackDebugHudString() ?? string.Empty;
		GUIStyle style = new GUIStyle(GUI.skin.box);
		style.alignment = TextAnchor.MiddleLeft;
		GUIContent content = new GUIContent(contentStr);
		space.height = style.CalcHeight(content, space.width);
		GUI.Box(space, contentStr, style);
		float buttonHeight = m_GUISize.y;
		if (GUI.Button(new Rect(space.xMin, space.yMax, space.width, buttonHeight), "Copy to Clipboard"))
		{
			ClipboardUtils.CopyToClipboard(contentStr);
		}
		space.yMax += buttonHeight;
		m_rewardTrackWindow.ResizeToFit(new Vector2(space.width, space.height));
		space.yMin = space.yMax;
		return space;
	}

	private Rect LayoutNotepadDebug(Rect space)
	{
		GUIStyle style = new GUIStyle(GUI.skin.box);
		style.alignment = TextAnchor.MiddleLeft;
		GUIContent content = new GUIContent("");
		space.height = style.CalcHeight(content, space.width);
		string notepadFileString = Directory.GetCurrentDirectory() + "\\notepad.txt";
		if (m_notepadFirstRun)
		{
			if (!File.Exists(notepadFileString))
			{
				File.Create(notepadFileString).Close();
			}
			else
			{
				m_notepadContents = File.ReadAllText(notepadFileString);
			}
			m_notepadFirstRun = false;
		}
		GUILayout.BeginArea(new Rect(space.xMin, space.yMax, space.width, 300f));
		GUILayout.BeginVertical();
		scrollViewVector = GUILayout.BeginScrollView(scrollViewVector);
		m_notepadContents = GUILayout.TextArea(m_notepadContents, GUILayout.ExpandHeight(expand: true));
		GUILayout.EndScrollView();
		GUILayout.BeginHorizontal();
		float buttonHeight = m_GUISize.y;
		if (GUILayout.Button("Copy to Clipboard"))
		{
			ClipboardUtils.CopyToClipboard(m_notepadContents);
		}
		if (GUILayout.Button("Save Contents"))
		{
			File.WriteAllText(notepadFileString, m_notepadContents);
		}
		space.yMax += buttonHeight;
		space.yMin = space.yMax;
		GUILayout.EndHorizontal();
		GUILayout.EndVertical();
		GUILayout.EndArea();
		return space;
	}

	private Rect LayoutScriptWarnings(Rect space)
	{
		Vector2 offset = space.min;
		Vector2 size = m_GUISize;
		if (GUI.Button(new Rect(offset.x, offset.y, size.x, size.y), "Clear Script Warnings"))
		{
			m_serverLogWindow.Clear();
		}
		offset.x += size.x;
		if (GUI.Button(new Rect(offset.x, offset.y, size.x, size.y), "Search JIRA for GUID") && m_serverLogWindow.GetEntries().LastOrDefault() is ScriptWarning entry)
		{
			string guid = UnityWebRequest.EscapeURL(entry.IssueGUID);
			Application.OpenURL($"https://jira.blizzard.com/issues/?jql=text~%22{guid}%22");
		}
		offset.x += size.x;
		offset.y += size.y;
		space.yMin = offset.y;
		return m_serverLogWindow.LayoutLog(space);
	}

	private void LayoutButton(ref Vector2 offset, float top, Vector2 size, string label, Action action)
	{
		if (offset.y + size.y > GetScaledScreen().y)
		{
			offset.y = top + size.y;
			offset.x += 1.1f * size.x;
		}
		if (GUI.Button(new Rect(offset.x, offset.y, size.x, size.y), label))
		{
			action();
		}
		offset.y += 1.1f * size.y;
	}

	private Rect LayoutCustomizeMenu(Rect space)
	{
		List<DebuggerGui> sections = new List<DebuggerGui>();
		sections.Add(m_cheatsWindow);
		if (m_messageWindow != null)
		{
			sections.Add(m_messageWindow);
		}
		sections.Add(m_serverLogWindow);
		sections.Add(m_ratingWindow);
		sections.Add(m_assetsWindow);
		sections.Add(m_questWindow);
		sections.Add(m_achievementWindow);
		sections.Add(m_rewardTrackWindow);
		sections.Add(m_timeSection);
		sections.Add(m_qualitySection);
		sections.Add(m_statsSection);
		sections.Add(m_slushTrackerWindow);
		sections.Add(m_notepadWindow);
		sections.Add(m_gameplayWindow);
		sections.Add(m_presenceWindow);
		Vector2 minOffset = space.min;
		Vector2 offset = space.min;
		foreach (DebuggerGui section in sections)
		{
			string label = (section.IsShown ? "☑" : "☐") + " " + section.Title;
			LayoutButton(ref offset, 0f, m_GUISize, label, delegate
			{
				section.IsShown = !section.IsShown;
			});
			if (offset.x > minOffset.x)
			{
				minOffset.x = offset.x;
				space.width += minOffset.x;
			}
			if (offset.y > minOffset.y)
			{
				minOffset.y = offset.y;
			}
		}
		space.yMin = minOffset.y;
		return space;
	}

	private void HandleGuiChanged()
	{
		m_guiSaveTimer = 3;
	}
}
