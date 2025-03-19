using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Blizzard.GameService.SDK.Client.Integration;
using Blizzard.T5.Jobs;
using Blizzard.T5.Services;
using Blizzard.Telemetry.WTCG.Client;
using Content.Delivery;
using Hearthstone.Core;
using Hearthstone.Util;

namespace Hearthstone.InGameMessage;

public class InGameMessageScheduler : IService
{
	private class WaitForBnetRegionAndNetCacheFeatures : IJobDependency, IAsyncJobResult
	{
		public bool IsReady()
		{
			if (NetCache.Get().GetNetObject<NetCache.NetCacheFeatures>() != null)
			{
				return BattleNet.GetCurrentRegion() != BnetRegion.REGION_UNINITIALIZED;
			}
			return false;
		}
	}

	public const string IGM_JSON_EXTENSION = ".igm.json";

	private const string IGM_UPDATE_JOB_NAME = "InGameMessage.UpdateMessages";

	private const double IGM_VALID_MESSAGE_HOURS = 672.0;

	private const int IGM_MAX_WAIT_TIME = 60;

	private Dictionary<string, MessageType> m_messageTypes = new Dictionary<string, MessageType>();

	private List<string> m_deletedMessageTypes = new List<string>();

	private ViewCountController m_viewCountController = new ViewCountController();

	public bool IsTerminated { get; set; }

	public bool HasNewRegisteredType { get; set; }

	public IEnumerator<IAsyncJobResult> Initialize(ServiceLocator serviceLocator)
	{
		CleanupOldMessages();
		Processor.QueueJobIfNotExist("InGameMessage.InitialUpdateMessages", InitialUpdate(), new WaitForBnetRegionAndNetCacheFeatures());
		CreateUpdateMessagesJob();
		HearthstoneApplication.Get().Resetting += CreateUpdateMessagesJob;
		yield break;
	}

	public Type[] GetDependencies()
	{
		return new Type[2]
		{
			typeof(SceneMgr),
			typeof(NetCache)
		};
	}

	public void Shutdown()
	{
		IsTerminated = true;
	}

	public static InGameMessageScheduler Get()
	{
		return ServiceManager.Get<InGameMessageScheduler>();
	}

	public void OnLoginCompleted()
	{
		foreach (KeyValuePair<string, MessageType> messageType in m_messageTypes)
		{
			messageType.Value.Init(force: true);
		}
	}

	public void RegisterMessage(MessageContentFeed messageFeed, int nextUpdateInterval, UpdateMessageHandler handler)
	{
		if (string.IsNullOrEmpty(messageFeed.ContentType))
		{
			Log.InGameMessage.PrintError("Invalid message feed. Will not be registered");
		}
		Log.InGameMessage.PrintInfo("Registered message: {0}, refresh time: {1}s", messageFeed.ContentType, nextUpdateInterval);
		if (!m_messageTypes.TryGetValue(messageFeed.ContentType, out var mtype))
		{
			mtype = new MessageType(messageFeed, nextUpdateInterval);
			m_messageTypes[messageFeed.ContentType] = mtype;
		}
		else
		{
			mtype.ResetWithNewUpdateInterval(nextUpdateInterval);
			if (m_deletedMessageTypes.Contains(messageFeed.ContentType))
			{
				m_deletedMessageTypes.Remove(messageFeed.ContentType);
			}
		}
		if (handler != null)
		{
			mtype.AddHandler(handler);
		}
		HasNewRegisteredType = true;
	}

	public void UnregisterMessage(MessageContentFeed messageFeed)
	{
		Log.InGameMessage.PrintInfo("Unregistered message: {0}", messageFeed.ContentType);
		if (m_messageTypes.TryGetValue(messageFeed.ContentType, out var mtype))
		{
			mtype.SetDeleted();
			if (!m_deletedMessageTypes.Contains(messageFeed.ContentType))
			{
				m_deletedMessageTypes.Add(messageFeed.ContentType);
			}
		}
	}

	public bool AddHandler(string contentType, UpdateMessageHandler handler)
	{
		if (!m_messageTypes.TryGetValue(contentType, out var mtype))
		{
			Log.InGameMessage.PrintError("Failed to add a handler for '{0}'", contentType);
			return false;
		}
		return mtype.AddHandler(handler);
	}

	public bool RemoveHandler(string contentType, UpdateMessageHandler handler)
	{
		if (!m_messageTypes.TryGetValue(contentType, out var mtype))
		{
			Log.InGameMessage.PrintError("Failed to remove a handler for '{0}'", contentType);
			return false;
		}
		return mtype.RemoveHandler(handler);
	}

	public void Refresh(string contentType)
	{
		Processor.RunCoroutine(TryToUpdate(contentType));
	}

	private IEnumerator TryToUpdate(string contentType)
	{
		while (NetCache.Get().GetNetObject<NetCache.NetCacheFeatures>() == null)
		{
			yield return null;
		}
		MessageType mtype;
		if (!ContentConnect.ContentstackEnabled)
		{
			Log.InGameMessage.PrintInfo("Refresh is ignored because system is turned off.");
		}
		else if (!m_messageTypes.TryGetValue(contentType, out mtype))
		{
			Log.InGameMessage.PrintError("Failed to refresh: {0}", contentType);
		}
		else if (mtype.IsReadyToUpdate)
		{
			mtype.Update(m_viewCountController);
		}
		else
		{
			mtype.RunUpdateMessageHandlers();
		}
	}

	public GameMessage[] GetMessage(string contentType)
	{
		if (!m_messageTypes.TryGetValue(contentType, out var mtype))
		{
			Log.InGameMessage.PrintError("Failed to get a message: {0}", contentType);
			return null;
		}
		return mtype.Messages;
	}

	public void IncreaseViewCount(GameMessage message)
	{
		if (message != null && message.MaxViewCount > 0)
		{
			m_viewCountController.IncreaseViewCount(message);
		}
	}

	public void SendTelemetryMessageAction(GameMessage message, InGameMessageAction.ActionType action)
	{
		TelemetryManager.Client().SendInGameMessageAction(message.ContentType, message.Title, action, m_viewCountController.GetViewCount(message), message.UID);
	}

	public int GetViewCount(string UID)
	{
		return m_viewCountController.GetViewCount(UID);
	}

	public void ResetViewCount()
	{
		m_viewCountController.ClearViewCounts();
	}

	public void ForceRefreshAllContents()
	{
		foreach (KeyValuePair<string, MessageType> msgType in m_messageTypes)
		{
			msgType.Value.SetDeleted();
			msgType.Value.Init(force: true);
			msgType.Value.Update(m_viewCountController);
		}
	}

	private void CreateUpdateMessagesJob()
	{
		Processor.QueueJobIfNotExist("InGameMessage.UpdateMessages", Job_UpdateMessages(), new WaitForBnetRegionAndNetCacheFeatures());
	}

	private void CleanupOldMessages()
	{
		string[] files = Directory.GetFiles(PlatformFilePaths.PersistentDataPath, "*.igm.json");
		foreach (string cachedMessage in files)
		{
			new FileInfo(cachedMessage);
			if (DateTime.UtcNow.Subtract(new FileInfo(cachedMessage).LastAccessTimeUtc).TotalHours > 672.0)
			{
				File.Delete(cachedMessage);
			}
		}
	}

	private IEnumerator<IAsyncJobResult> InitialUpdate()
	{
		foreach (KeyValuePair<string, MessageType> messageType in m_messageTypes)
		{
			messageType.Value.Update(m_viewCountController);
		}
		yield break;
	}

	private IEnumerator<IAsyncJobResult> Job_UpdateMessages()
	{
		while (!IsTerminated)
		{
			HasNewRegisteredType = false;
			int minWaitSeconds = 60;
			if (ContentConnect.ContentstackEnabled)
			{
				foreach (KeyValuePair<string, MessageType> mtype in m_messageTypes)
				{
					if (IsTerminated)
					{
						yield break;
					}
					if (mtype.Value.IsReadyToUpdate && mtype.Value.IsAutomaticUpdate)
					{
						InGameMessageTelemetry.SendMessageUpdate(mtype.Key);
						mtype.Value.Update(m_viewCountController);
						continue;
					}
					int waitSeconds = mtype.Value.ValidSeconds;
					if (waitSeconds > 0 && waitSeconds < minWaitSeconds)
					{
						minWaitSeconds = waitSeconds;
					}
				}
				if (m_deletedMessageTypes.Count > 0)
				{
					m_deletedMessageTypes.ForEach(delegate(string key)
					{
						m_messageTypes.Remove(key);
					});
					m_deletedMessageTypes.Clear();
				}
			}
			else
			{
				Log.InGameMessage.PrintDebug("The system is turned off by server configuration.");
			}
			if (IsTerminated)
			{
				break;
			}
			Log.InGameMessage.PrintDebug("Wait for {0}s before next update", minWaitSeconds);
			yield return new WaitForDurationForNextUpdate(minWaitSeconds);
		}
	}
}
