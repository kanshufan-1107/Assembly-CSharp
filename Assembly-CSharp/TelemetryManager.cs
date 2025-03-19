using System;
using System.Collections;
using System.Collections.Generic;
using Blizzard.GameService.SDK.Client.Integration;
using Blizzard.T5.Configuration;
using Blizzard.Telemetry;
using Hearthstone.Core;
using Hearthstone.Telemetry;
using HearthstoneTelemetry;
using UnityEngine;

public class TelemetryManager
{
	private readonly ITelemetryClient m_telemetryClient = new TelemetryClient();

	private static readonly TelemetryManager s_instance;

	private Service m_telemetryService;

	private bool m_auroraConnected;

	private readonly List<Action> m_shutdownListeners = new List<Action>();

	private readonly object m_listenerLock = new object();

	private BaseContextData m_contextData;

	private static List<ITelemetryManagerComponent> s_components;

	private readonly Dictionary<long, List<Action<long>>> m_messagesWaitingForCallback = new Dictionary<long, List<Action<long>>>();

	public static TelemetryManagerComponentNetwork NetworkComponent { get; private set; }

	public static string ProgramId
	{
		get
		{
			if (s_instance.m_telemetryService != null && s_instance.m_telemetryService.Context != null && s_instance.m_telemetryService.Context.Program != null && s_instance.m_telemetryService.Context.Program.Id != null)
			{
				return s_instance.m_telemetryService.Context.Program.Id;
			}
			return string.Empty;
		}
	}

	public static string ProgramName
	{
		get
		{
			if (s_instance.m_telemetryService != null && s_instance.m_telemetryService.Context != null && s_instance.m_telemetryService.Context.Program != null && s_instance.m_telemetryService.Context.Program.Name != null)
			{
				return s_instance.m_telemetryService.Context.Program.Name;
			}
			return string.Empty;
		}
	}

	public static string ProgramVersion
	{
		get
		{
			if (s_instance.m_telemetryService != null && s_instance.m_telemetryService.Context != null && s_instance.m_telemetryService.Context.Program != null && s_instance.m_telemetryService.Context.Program.Version != null)
			{
				return s_instance.m_telemetryService.Context.Program.Version;
			}
			return string.Empty;
		}
	}

	public static string SessionId
	{
		get
		{
			if (s_instance.m_telemetryService != null && s_instance.m_telemetryService.Context != null && s_instance.m_telemetryService.Context.SessionId != null)
			{
				return s_instance.m_telemetryService.Context.SessionId;
			}
			return string.Empty;
		}
	}

	public static string AppInstallId => s_instance?.m_telemetryService?.Context?.Identity?.AppInstallId ?? string.Empty;

	static TelemetryManager()
	{
		s_instance = new TelemetryManager();
		NetworkComponent = new TelemetryManagerComponentNetwork();
		s_components = new List<ITelemetryManagerComponent>
		{
			new TelemetryManagerComponentAudio(s_instance.m_telemetryClient),
			NetworkComponent
		};
	}

	private TelemetryManager()
	{
	}

	public static ITelemetryClient Client()
	{
		return s_instance.m_telemetryClient;
	}

	public static void RegisterMessageSentCallback(long messageId, Action<long> sentMessageCallback)
	{
		if (s_instance.m_messagesWaitingForCallback.ContainsKey(messageId))
		{
			if (s_instance.m_messagesWaitingForCallback[messageId] == null)
			{
				s_instance.m_messagesWaitingForCallback[messageId] = new List<Action<long>>();
			}
			s_instance.m_messagesWaitingForCallback[messageId].Add(sentMessageCallback);
		}
		else
		{
			s_instance.m_messagesWaitingForCallback.Add(messageId, new List<Action<long>> { sentMessageCallback });
		}
	}

	public static void RegisterShutdownListener(Action handler)
	{
		lock (s_instance.m_listenerLock)
		{
			if (!s_instance.m_shutdownListeners.Contains(handler))
			{
				s_instance.m_shutdownListeners.Add(handler);
			}
		}
	}

	public static void UnregisterShutdownListener(Action handler)
	{
		lock (s_instance.m_listenerLock)
		{
			s_instance.m_shutdownListeners.Remove(handler);
		}
	}

	public static string GetApplicationId()
	{
		if (s_instance.m_contextData != null)
		{
			return s_instance.m_contextData.ApplicationID;
		}
		return string.Empty;
	}

	public static void RebuildContext()
	{
		s_instance.SetServiceContext();
	}

	public static void Flush()
	{
		s_instance.m_telemetryService.Flush();
	}

	public static void FlushSync()
	{
		if (s_instance.m_telemetryClient.IsInitialized())
		{
			s_instance.m_telemetryService.Stop(new TimeSpan(0, 0, 0, 0, 2000), null);
			Processor.RunCoroutine(s_instance.m_telemetryService.Run());
		}
	}

	public static void Reset()
	{
		if (s_instance.m_telemetryClient.IsInitialized())
		{
			s_instance.m_auroraConnected = false;
			PresenceMgr.Get().ResetTelemetry();
			s_instance.m_telemetryService.Stop(new TimeSpan(0, 0, 0, 0, 2000), null);
			s_instance.SetTelemetryServiceData();
			Processor.RunCoroutine(s_instance.m_telemetryService.Run());
			Processor.RunCoroutine(s_instance.SetTelemetryAppUserContext());
		}
	}

	private IEnumerator SetTelemetryAppUserContext()
	{
		yield return new WaitUntil(() => s_instance.m_telemetryService.Running);
		SetAppUserContext(ProgramId, ProgramName, ProgramVersion, SessionId);
	}

	public static void OnBattleNetConnect(string host, int port, BattleNetErrors error)
	{
		if (error != 0)
		{
			s_instance.m_telemetryClient.SendConnectFail("AURORA", error.ToString(), host, (uint)port);
			return;
		}
		s_instance.m_auroraConnected = true;
		s_instance.m_telemetryClient.SendConnectSuccess("AURORA", host, (uint)port);
		RegisterShutdownListener(delegate
		{
			OnBattleNetDisconnect(host, port, BattleNetErrors.ERROR_OK);
		});
	}

	public static void OnBattleNetDisconnect(string host, int port, BattleNetErrors error)
	{
		if (s_instance.m_auroraConnected)
		{
			s_instance.m_auroraConnected = false;
			s_instance.m_telemetryClient.SendDisconnect("AURORA", TelemetryUtil.GetReasonFromBnetError(error), (error == BattleNetErrors.ERROR_OK) ? null : error.ToString(), null, null);
		}
	}

	public static void Shutdown()
	{
		if (!s_instance.m_telemetryClient.IsInitialized())
		{
			return;
		}
		Log.Telemetry.Print("Shutting down telemetry");
		foreach (ITelemetryManagerComponent s_component in s_components)
		{
			s_component.Shutdown();
		}
		Processor.UnregisterUpdateDelegate(s_instance.OnUpdate);
		ProcessShutdownListeners();
		s_instance.m_telemetryService.Stop(new TimeSpan(0, 0, 0, 0, 1000), null);
		s_instance.m_telemetryClient.Shutdown();
	}

	public static void Initialize()
	{
		if (!Vars.Key("Telemetry.Enabled").GetBool(def: true))
		{
			return;
		}
		s_instance.SetTelemetryServiceData();
		Log.Telemetry.Print("Sending telemetry messages to TDK instance: {0}, SSL={1} IngestPort={2}", s_instance.m_contextData.IngestUri.AbsoluteUri, s_instance.m_contextData.IngestUri.Scheme == "https", s_instance.m_contextData.IngestUri.Port);
		Processor.RunCoroutine(s_instance.m_telemetryService.Run());
		Processor.RunCoroutine(s_instance.SetTelemetryAppUserContext());
		foreach (ITelemetryManagerComponent s_component in s_components)
		{
			s_component.Initialize();
		}
		Processor.RegisterUpdateDelegate(s_instance.OnUpdate);
	}

	public static void SetTelemetryFeatureStatus(bool isEnabled)
	{
		if (isEnabled)
		{
			s_instance.m_telemetryClient.Enable();
		}
		else
		{
			s_instance.m_telemetryClient.Disable();
		}
	}

	private void SetTelemetryServiceData()
	{
		s_instance.m_contextData = new StandaloneContext();
		ServiceOptions serviceOptions = new ServiceOptions(m_contextData.ProgramId);
		serviceOptions.IngestBaseUrl = m_contextData.IngestUri.AbsoluteUri;
		serviceOptions.SendStartAndFinishMessages = true;
		serviceOptions.MaxQueueSize = 1500;
		serviceOptions.MaxBatchSize = 10;
		serviceOptions.OnMessageSent = TelemetryMessageSent;
		if (Vars.Key("Telemetry.LogEnabled").GetBool(def: false))
		{
			Blizzard.Telemetry.Log.Logger = new TelemetryLogWrapper();
			serviceOptions.LogEnqueue = true;
			serviceOptions.LogMessageRequest = true;
		}
		m_telemetryService = new Service(serviceOptions);
		SetServiceContext();
		m_telemetryClient.Initialize(m_telemetryService);
	}

	public static void SetAppUserContext(string appId, string appName, string appVersion, string sessionId)
	{
	}

	public static void SetAccountUserContext(string tassadarAuthToken, ulong bnetId, string bnetRegion, string bnetLocale)
	{
	}

	private void SetServiceContext()
	{
		List<string> hostTags = new List<string>();
		if (m_contextData.ConnectionType == TelemetryConnectionType.Internal)
		{
			hostTags.Add("dev");
			Log.Telemetry.Print("Sending telemetry to production endpoint. Messages will be tagged as dev");
		}
		else
		{
			hostTags.Add("prod");
			Log.Telemetry.Print("Sending telemetry to production endpoint. Messages will be tagged as prod");
		}
		if (PlatformSettings.IsSteam)
		{
			hostTags.Add("Steam");
		}
		m_telemetryService.UserContext = new Context
		{
			Account = BnetUtils.TryGetGameAccountId(),
			BnetId = BnetUtils.TryGetBnetAccountId(),
			GameLocation = new Context.LocationInfo
			{
				BnetRegion = (int?)BnetUtils.TryGetGameRegion()
			},
			Program = new Context.ProgramInfo
			{
				Id = s_instance.m_contextData.ProgramId,
				Name = s_instance.m_contextData.ProgramName,
				Version = s_instance.m_contextData.ProgramVersion
			},
			PlayerLocation = new Context.LocationInfo
			{
				BnetRegion = s_instance.m_contextData.BattleNetRegion
			},
			Host = new Context.HostInfo
			{
				Tag = hostTags,
				Arch = SystemInfo.deviceModel
			}
		};
	}

	private void OnUpdate()
	{
		if (!m_telemetryClient.IsInitialized())
		{
			return;
		}
		foreach (ITelemetryManagerComponent s_component in s_components)
		{
			s_component.Update();
		}
		m_telemetryClient.OnUpdate();
	}

	private void TelemetryMessageSent(long messageId)
	{
		if (!m_messagesWaitingForCallback.ContainsKey(messageId))
		{
			return;
		}
		foreach (Action<long> item in m_messagesWaitingForCallback[messageId])
		{
			item?.Invoke(messageId);
		}
		m_messagesWaitingForCallback.Remove(messageId);
	}

	private static void ProcessShutdownListeners()
	{
		if (s_instance.m_shutdownListeners.Count != 0)
		{
			Action[] handlers;
			lock (s_instance.m_listenerLock)
			{
				handlers = s_instance.m_shutdownListeners.ToArray();
				s_instance.m_shutdownListeners.Clear();
			}
			Log.Telemetry.Print("Processing {0} shutdown listeners", handlers.Length);
			Action[] array = handlers;
			for (int i = 0; i < array.Length; i++)
			{
				array[i]();
			}
		}
	}

	public static void Stop()
	{
		if (s_instance.m_telemetryClient.IsInitialized())
		{
			s_instance.m_telemetryService.Stop(new TimeSpan(0, 0, 0, 0, 2000), null);
		}
	}
}
