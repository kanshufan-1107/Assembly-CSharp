using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Blizzard.GameService.SDK.Client.Integration;
using Blizzard.T5.Configuration;
using Content.Delivery;
using Hearthstone.Core;
using Hearthstone.Util;
using MiniJSON;
using UnityEngine;

namespace Hearthstone.InGameMessage;

public class MessageType : IBrazeInGameMessageListener
{
	public const int UPDATE_INTERVAL_NO_AUTOMATIC_UPDATE = -1;

	public const int UPDATE_INTERVAL_USE_HTTP_CACHE_AGE = 0;

	private MessageContentFeed m_messageFeed;

	private string m_cachedJsonPath;

	private int m_nextUpdateInterval;

	private bool m_initialized;

	private bool m_deleted;

	private List<GameMessage> m_allMessages = new List<GameMessage>();

	private List<UpdateMessageHandler> m_updateMessageHandlers = new List<UpdateMessageHandler>();

	private ContentStackConnect m_connect = new ContentStackConnect();

	private byte[] m_latestHash;

	private byte[] m_currentHash;

	private string m_entryTitles;

	private List<string> m_reportedQualifiedContentTypes = new List<string>();

	public bool IsReadyToUpdate
	{
		get
		{
			if (!m_initialized)
			{
				Init(force: false);
			}
			if (m_initialized && !m_deleted)
			{
				if (m_messageFeed.SourceType == MessageSourceType.ContentStack)
				{
					return m_connect.Ready;
				}
				return true;
			}
			return false;
		}
	}

	public bool IsAutomaticUpdate => m_nextUpdateInterval != -1;

	public int ValidSeconds => m_connect.ValidSeconds;

	public GameMessage[] Messages
	{
		get
		{
			if (m_allMessages.Count <= 0)
			{
				return null;
			}
			return m_allMessages.ToArray();
		}
	}

	private byte[] HashValues
	{
		get
		{
			if (m_currentHash == null && m_allMessages.Count > 0)
			{
				int oneLength = m_allMessages[0].HashValue.Length;
				m_currentHash = new byte[oneLength * m_allMessages.Count];
				int offset = 0;
				foreach (GameMessage allMessage in m_allMessages)
				{
					Buffer.BlockCopy(allMessage.HashValue, 0, m_currentHash, offset, oneLength);
					offset += oneLength;
				}
			}
			return m_currentHash;
		}
	}

	public MessageType(MessageContentFeed messageFeed, int nextUpdateInterval)
	{
		m_messageFeed = messageFeed;
		m_cachedJsonPath = Path.Combine(PlatformFilePaths.PersistentDataPath, m_messageFeed.ContentType + ".igm.json");
		m_nextUpdateInterval = nextUpdateInterval;
		Init(force: false);
	}

	public void Update(object param)
	{
		string query = m_messageFeed.GetQuery?.Invoke();
		if (string.IsNullOrEmpty(query))
		{
			Log.InGameMessage.PrintError("Count ne get query for " + m_messageFeed.ContentType);
		}
		else if (m_messageFeed.SourceType == MessageSourceType.ContentStack)
		{
			Processor.RunCoroutine(m_connect.Query(UpdateAllMessagesFromJson, param, query, force: false));
		}
		else if (m_messageFeed.SourceType == MessageSourceType.Braze && File.Exists(m_cachedJsonPath))
		{
			string json = File.ReadAllText(m_cachedJsonPath);
			UpdateAllMessagesFromJson(json, param);
		}
	}

	public void Init(bool force)
	{
		if (force)
		{
			m_initialized = false;
			m_deleted = false;
		}
		if (BattleNet.GetCurrentRegion() == BnetRegion.REGION_UNINITIALIZED)
		{
			Log.InGameMessage.PrintDebug("Failed to initialize '{0}': No region", m_messageFeed.ContentType);
		}
		else if (!m_initialized)
		{
			int configCacheAge = Vars.Key("ContentStack.CacheAge").GetInt(0);
			if (m_messageFeed.SourceType == MessageSourceType.ContentStack)
			{
				m_connect.InitializeURL(m_messageFeed.ContentType, Vars.Key("ContentStack.Env").GetStr("production"), Localization.GetBnetLocaleName(), BattleNet.GetCurrentRegion() == BnetRegion.REGION_CN, m_cachedJsonPath, (configCacheAge != 0) ? configCacheAge : m_nextUpdateInterval);
			}
			else if (m_messageFeed.SourceType == MessageSourceType.Braze)
			{
				RegisterForBrazeMessaging();
			}
			m_initialized = true;
		}
	}

	public bool AddHandler(UpdateMessageHandler handler)
	{
		if (m_updateMessageHandlers.Contains(handler))
		{
			return false;
		}
		m_updateMessageHandlers.Add(handler);
		return true;
	}

	public bool RemoveHandler(UpdateMessageHandler handler)
	{
		if (!m_updateMessageHandlers.Contains(handler))
		{
			return false;
		}
		m_updateMessageHandlers.Remove(handler);
		return true;
	}

	public void RunUpdateMessageHandlers()
	{
		if (m_allMessages.Count == 0 || m_updateMessageHandlers.Count == 0 || StructuralComparisons.StructuralEqualityComparer.Equals(m_latestHash, HashValues))
		{
			return;
		}
		foreach (UpdateMessageHandler updateMessageHandler in m_updateMessageHandlers)
		{
			updateMessageHandler(Messages);
		}
		m_latestHash = HashValues;
	}

	public void ResetWithNewUpdateInterval(int interval)
	{
		if (m_nextUpdateInterval != interval || m_deleted)
		{
			m_nextUpdateInterval = interval;
			Init(force: true);
		}
	}

	public void SetDeleted()
	{
		if (!m_deleted)
		{
			m_initialized = false;
			m_deleted = true;
			m_allMessages.Clear();
			m_latestHash = null;
			m_currentHash = null;
			m_entryTitles = string.Empty;
		}
		if (m_messageFeed.SourceType == MessageSourceType.Braze)
		{
			UnregisterForBrazeMessaging();
		}
	}

	private void RegisterForBrazeMessaging()
	{
		PushNotificationManager pnm = PushNotificationManager.Get();
		if (pnm == null)
		{
			Log.InGameMessage.PrintError("MessageFeedRegistry failed to register for Braze in-game messages!");
		}
		else
		{
			pnm.RegisterBrazeInGameMessageHandler(this);
		}
	}

	private void UnregisterForBrazeMessaging()
	{
		PushNotificationManager pnm = PushNotificationManager.Get();
		if (pnm == null)
		{
			Log.InGameMessage.PrintError("MessageFeedRegistry failed to unregister for Braze in-game messages!");
		}
		else
		{
			pnm.UnregisterBrazeInGameMessageHandler(this);
		}
	}

	void IBrazeInGameMessageListener.OnBrazeInGameMessageReceived(BrazeInGameMessage message)
	{
		if (message == null)
		{
			Log.InGameMessage.PrintWarning("MessageFeedRegistry received a null IGM from Braze, ignoring...");
			return;
		}
		GameMessage gm = message.ConvertBrazeIgmToGameMessage();
		if (gm == null)
		{
			Log.InGameMessage.PrintWarning("MessageFeedRegistry couldn't convert Braze IGM to GameMessage!");
			return;
		}
		m_allMessages.Add(gm);
		InGameMessageUtils.SaveGameMessagesToDisk(m_allMessages, m_cachedJsonPath);
	}

	private void UpdateAllMessagesFromJson(string jsonResponse, object param)
	{
		Log.InGameMessage.PrintDebug("Content Stack response : " + m_messageFeed.ContentType + " " + jsonResponse);
		if (string.IsNullOrEmpty(jsonResponse))
		{
			return;
		}
		JsonNode jsonNode;
		try
		{
			jsonNode = Json.Deserialize(jsonResponse) as JsonNode;
		}
		catch (Exception ex)
		{
			jsonNode = null;
			Log.InGameMessage.PrintWarning("Aborting because of an invalid json response:\n{0}", jsonResponse);
			Debug.LogError(ex.StackTrace);
		}
		try
		{
			ViewCountController viewCountController = param as ViewCountController;
			m_allMessages = InGameMessageUtils.GetAllMessagesFromJsonResponse(jsonNode, viewCountController);
			m_currentHash = null;
			m_entryTitles = string.Empty;
			List<string> uids = new List<string>();
			foreach (GameMessage msg in m_allMessages)
			{
				msg.ContentType = m_messageFeed.ContentType;
				uids.Add(msg.UID);
			}
			if (!m_reportedQualifiedContentTypes.Contains(m_messageFeed.ContentType))
			{
				TelemetryManager.Client().SendInGameMessageQualified(m_messageFeed.ContentType, uids);
				m_reportedQualifiedContentTypes.Add(m_messageFeed.ContentType);
			}
			RunUpdateMessageHandlers();
		}
		catch (Exception ex2)
		{
			Debug.LogErrorFormat("Failed to convert JSON response to message: {0}", ex2.Message);
		}
	}
}
