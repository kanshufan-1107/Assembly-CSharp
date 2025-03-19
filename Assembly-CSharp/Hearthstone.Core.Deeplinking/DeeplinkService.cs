using System;
using System.Collections.Generic;
using Blizzard.T5.Core;
using Blizzard.T5.Jobs;
using Blizzard.T5.Services;
using MiniJSON;

namespace Hearthstone.Core.Deeplinking;

public class DeeplinkService : IUnityMessageHandler
{
	private readonly Dictionary<string, HashSet<IDeeplinkCallback>> m_deeplinkHandlersPerScheme = new Dictionary<string, HashSet<IDeeplinkCallback>>();

	private readonly List<IDeeplinkCallback> m_deeplinkHandlersToFire = new List<IDeeplinkCallback>();

	private bool m_isInitialized;

	private bool m_isShuttingDown;

	private string m_lastHandledDeeplinkData;

	private static DeeplinkService s_instance;

	public Map<string, string> GetStartupDeepLinkArgs()
	{
		return DeeplinkUtils.GetDeepLinkArgs(GetStartupDeeplinkUrl());
	}

	public static DeeplinkService Get()
	{
		if (s_instance == null)
		{
			s_instance = new DeeplinkService();
		}
		return s_instance;
	}

	public void Initialize()
	{
		if (!m_isInitialized)
		{
			if (!AreDependenciesInitialized())
			{
				Log.DeepLink.PrintError("[DeeplinkService] An error while initializing HS Java application!");
				return;
			}
			HearthstoneApplication.Get().Unpaused += OnApplicationUnpaused;
			HearthstoneApplication.Get().OnShutdown += OnShutdown;
			Processor.QueueJob(new JobDefinition("DeeplinkService.PostFullLoginJobs", Job_PostFullLoginJobs(), new WaitForFullLoginFlowComplete()));
			m_isInitialized = true;
			Log.DeepLink.PrintDebug("[DeeplinkService] Initialization finished.");
		}
	}

	private void OnShutdown()
	{
		m_isShuttingDown = true;
		m_isInitialized = false;
		HearthstoneApplication.Get().OnShutdown -= OnShutdown;
		HearthstoneApplication.Get().Unpaused -= OnApplicationUnpaused;
		UnityMessageBroker.UnregisterHandler("deeplinkData", this);
	}

	private IEnumerator<IAsyncJobResult> Job_PostFullLoginJobs()
	{
		while (!ServiceManager.IsAvailable<LoginManager>() || !ServiceManager.IsAvailable<SceneMgr>())
		{
			yield return null;
		}
		SceneMgr sceneMgr = SceneMgr.Get();
		while (sceneMgr.IsTransitioning() || sceneMgr.GetMode() == SceneMgr.Mode.INVALID || sceneMgr.GetMode() == SceneMgr.Mode.LOGIN)
		{
			yield return null;
		}
		if (!m_isShuttingDown)
		{
			UnityMessageBroker.RegisterHandler("deeplinkData", this);
			EnsureCurrentDeeplinkProcessed();
		}
	}

	public bool RegisterDeeplinkHandler(string scheme, IDeeplinkCallback callback)
	{
		if (string.IsNullOrEmpty(scheme) || callback == null)
		{
			Log.DeepLink.PrintError(string.Format("[{0}] Unexpeted nulls - ignoring registering a '{1}' handler '{2}'", "DeeplinkService", scheme, callback));
			return false;
		}
		Log.DeepLink.PrintDebug(string.Format("[{0}] Registering a '{1}' handler '{2}'", "DeeplinkService", scheme, callback));
		if (!m_deeplinkHandlersPerScheme.TryGetValue(scheme, out var handlers))
		{
			handlers = (m_deeplinkHandlersPerScheme[scheme] = new HashSet<IDeeplinkCallback>());
		}
		handlers.Add(callback);
		return true;
	}

	public bool UnregisterDeeplinkHandler(string scheme, IDeeplinkCallback callback)
	{
		if (string.IsNullOrEmpty(scheme) || callback == null)
		{
			Log.DeepLink.PrintError(string.Format("[{0}] Unexpeted nulls - ignoring unregistering a '{1}' handler '{2}'", "DeeplinkService", scheme, callback));
			return false;
		}
		Log.DeepLink.PrintDebug(string.Format("[{0}] Unregistering a '{1}' handler '{2}'", "DeeplinkService", scheme, callback));
		if (!m_deeplinkHandlersPerScheme.TryGetValue(scheme, out var handlers))
		{
			return false;
		}
		return handlers.Remove(callback);
	}

	private bool ProcessDeeplink(string url)
	{
		try
		{
			Log.DeepLink.PrintDebug("[DeeplinkService] Processing deeplink '" + url + "'");
			Uri uri = new Uri(url);
			if (!url.StartsWith("hearthstone://", StringComparison.OrdinalIgnoreCase) && !uri.Host.Equals("WTCG", StringComparison.OrdinalIgnoreCase))
			{
				Log.DeepLink.PrintError("Received deeplink from unknown host " + uri.Host);
				return false;
			}
			if (m_deeplinkHandlersPerScheme.TryGetValue(uri.Scheme, out var handlers))
			{
				m_deeplinkHandlersToFire.Clear();
				m_deeplinkHandlersToFire.AddRange(handlers);
				foreach (IDeeplinkCallback item in m_deeplinkHandlersToFire)
				{
					item.ProcessDeeplink(url);
				}
				return true;
			}
		}
		catch (Exception exception)
		{
			Log.DeepLink.PrintException(exception, "[DeeplinkService] Processing deeplink '" + url + "' failed due to exception.");
		}
		return false;
	}

	void IUnityMessageHandler.HandleUnityMessage(string rawJson, JsonNode deeplinkData)
	{
		Log.DeepLink.PrintDebug("[DeeplinkService] Handling Unity Message '" + rawJson + "'");
		if (rawJson == m_lastHandledDeeplinkData)
		{
			Log.DeepLink.PrintDebug("[DeeplinkService] Ignoring Unity Message '" + rawJson + "' - already handled.");
			return;
		}
		m_lastHandledDeeplinkData = rawJson;
		if (!deeplinkData.TryGetValueAs<int>("version", out var _) || !deeplinkData.TryGetValueAs<string>("url", out var deeplinkUrl))
		{
			Log.DeepLink.PrintError("[DeeplinkService] Ignoring unexpected message '" + rawJson + "'");
		}
		else if (!string.IsNullOrEmpty(deeplinkUrl))
		{
			ProcessDeeplink(deeplinkUrl);
		}
	}

	private void OnApplicationUnpaused()
	{
		Log.DeepLink.PrintDebug("Application unpaused, refreshing deeplink");
		EnsureCurrentDeeplinkProcessed();
	}

	public string GetCurrentDeeplinkUrl()
	{
		string deeplinkDataJson = GetCurrentDeeplinkData();
		if (!string.IsNullOrEmpty(deeplinkDataJson))
		{
			if (TryParseDeeplinkData(deeplinkDataJson, out var deeplinkData, out var errorMsg))
			{
				return deeplinkData.GetValueAs("url", string.Empty);
			}
			Log.DeepLink.PrintError(errorMsg);
		}
		return string.Empty;
	}

	private void EnsureCurrentDeeplinkProcessed()
	{
		string deeplinkDataJson = GetCurrentDeeplinkData();
		if (!string.IsNullOrEmpty(deeplinkDataJson) && deeplinkDataJson != m_lastHandledDeeplinkData)
		{
			if (TryParseDeeplinkData(deeplinkDataJson, out var deeplinkData, out var errorMsg))
			{
				((IUnityMessageHandler)this).HandleUnityMessage(deeplinkDataJson, deeplinkData);
			}
			else
			{
				Log.DeepLink.PrintError(errorMsg);
			}
		}
	}

	private static bool TryParseDeeplinkData(string json, out JsonNode deeplinkData, out string errorMessage)
	{
		deeplinkData = null;
		errorMessage = null;
		try
		{
			deeplinkData = Json.Deserialize(json) as JsonNode;
			if (deeplinkData == null)
			{
				errorMessage = "[DeeplinkService] Received null json message";
				return false;
			}
			return true;
		}
		catch (Exception ex)
		{
			errorMessage = "[DeeplinkService] Failed to parse json: " + ex.Message;
			return false;
		}
	}

	private string GetCurrentDeeplinkData()
	{
		return null;
	}

	public string GetStartupDeeplinkUrl()
	{
		return null ?? string.Empty;
	}

	private bool AreDependenciesInitialized()
	{
		return true;
	}
}
