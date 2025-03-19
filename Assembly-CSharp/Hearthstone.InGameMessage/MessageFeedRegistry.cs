using System;
using System.Collections.Generic;
using Blizzard.T5.Services;
using Blizzard.Telemetry.WTCG.Client;
using Hearthstone.InGameMessage.Personalization;
using Hearthstone.InGameMessage.UI;

namespace Hearthstone.InGameMessage;

public class MessageFeedRegistry
{
	private readonly List<MessageContentFeed> m_feeds = new List<MessageContentFeed>();

	private GIGateway m_GIGateway;

	private List<string> m_GIData;

	private bool m_personalizedMessagesEnabled;

	private const string PERSONALIZED_MESSAGE_CONTENT_TYPE = "in_game_message_personalized";

	public const string BRAZE_MESSAGE_CONTENT_TYPE = "in_game_message_braze";

	private readonly List<MessageUIData> m_messageQueueBacklog = new List<MessageUIData>();

	public void EnablePersonalizedMessages()
	{
		Log.InGameMessage.PrintDebug("Personalized Messaging Enabled");
		if (!m_personalizedMessagesEnabled)
		{
			m_personalizedMessagesEnabled = true;
			RegisterPersonalizedMessageFeed();
		}
	}

	public void DisablePersonalizedMessages()
	{
		Log.InGameMessage.PrintInfo("Personalized Messaging Disabled");
		if (m_personalizedMessagesEnabled)
		{
			m_personalizedMessagesEnabled = false;
			ClearPersonalizedMessageFeed();
		}
	}

	private void RegisterPersonalizedMessageFeed()
	{
		if (m_GIGateway == null)
		{
			m_GIGateway = new GIGateway(Log.InGameMessage);
		}
		m_GIGateway.GetMessages(OnGIDataReceived);
	}

	private void ClearPersonalizedMessageFeed()
	{
		m_GIGateway = null;
	}

	private void OnGIDataReceived(string[] data)
	{
		if (!m_personalizedMessagesEnabled)
		{
			return;
		}
		if (data == null)
		{
			Log.InGameMessage.PrintWarning("GI personalization data is null. Personalization feed will not be registered.");
			return;
		}
		m_GIData = new List<string>(data);
		if (m_GIData.Count == 0)
		{
			Log.InGameMessage.PrintWarning("GI personalization data is empty. Personalization feed will not be registered.");
			return;
		}
		MessageContentFeed personalizedFeed = new MessageContentFeed("in_game_message_personalized", new StandardIGMTranslator(), () => InGameMessageUtils.MakePersonalizedQueryString(m_GIData), MessageSourceType.ContentStack);
		RegisterFeed(personalizedFeed);
	}

	public void RegisterDefaultFeeds()
	{
		MessageContentFeed boxFeed = new MessageContentFeed("in_game_message", new StandardIGMTranslator(), () => InGameMessageUtils.MakeStandardQueryString(), MessageSourceType.ContentStack);
		RegisterFeed(boxFeed);
		MessageContentFeed mercFeed = new MessageContentFeed("in_game_message_mercenaries", new StandardIGMTranslator(), () => InGameMessageUtils.MakeStandardQueryString(), MessageSourceType.ContentStack);
		RegisterFeed(mercFeed);
		MessageContentFeed brazeFeed = new MessageContentFeed("in_game_message_braze", new StandardIGMTranslator(), () => InGameMessageUtils.MakeStandardQueryString(), MessageSourceType.Braze);
		RegisterFeed(brazeFeed);
	}

	public void RegisterFeed(MessageContentFeed feed)
	{
		if (feed == null || string.IsNullOrEmpty(feed.ContentType))
		{
			Log.InGameMessage.PrintWarning("Attempted to register a feed with invalid/missing arguments");
			return;
		}
		if (m_feeds.Exists((MessageContentFeed x) => x.ContentType.Equals(feed.ContentType)))
		{
			Log.InGameMessage.PrintWarning("Attempted to re-register an existing feed");
			return;
		}
		m_feeds.Add(feed);
		InGameMessageTelemetry.SendRegisterContentType(feed.ContentType);
		RegisterFeedListener(feed);
	}

	private void RegisterFeedListener(MessageContentFeed feed)
	{
		if (ServiceManager.TryGet<InGameMessageScheduler>(out var scheduler))
		{
			scheduler.RegisterMessage(feed, 0, OnFeedUpdated);
		}
		else
		{
			Log.InGameMessage.PrintWarning("Failed to register feed listener, scheduler is not ready or missing.");
		}
	}

	private void OnFeedUpdated(GameMessage[] messages)
	{
		List<MessageUIData> messagesToAdd = new List<MessageUIData>();
		foreach (GameMessage message in messages)
		{
			if (message == null)
			{
				Log.InGameMessage.PrintWarning("MessageFeedRegistry got a null message on feed update - ignoring and continuing to the next...");
				continue;
			}
			MessageContentFeed feed = FindFeedForMessage(message);
			if (feed == null)
			{
				Log.InGameMessage.PrintWarning("Got message for unregistered feed " + message.ContentType);
				return;
			}
			if (feed.ContentType.Equals("in_game_message_personalized") && !m_personalizedMessagesEnabled)
			{
				Log.InGameMessage.PrintInfo("Got a personalized message but ignoring due to feature being disabled");
				return;
			}
			MessageUIData messageData = feed.DataTranslator.CreateData(message);
			if (messageData == null)
			{
				InGameMessageTelemetry.SendMessageTranslationError(message.ContentType, message.Title, message.UID);
				Log.InGameMessage.PrintWarning("Could not translate data for message {0} in feed {1}. Message will be ignored", message, feed.ContentType);
				continue;
			}
			UIMessageCallbacks callbacks = messageData.Callbacks;
			callbacks.OnViewed = (Action<InGameMessageAction.ActionType>)Delegate.Combine(callbacks.OnViewed, (Action<InGameMessageAction.ActionType>)delegate(InGameMessageAction.ActionType action)
			{
				if (ServiceManager.TryGet<InGameMessageScheduler>(out var service))
				{
					InGameMessageTelemetry.SendMessageAction(message, service.GetViewCount(message), action);
					service.IncreaseViewCount(message);
				}
			});
			UIMessageCallbacks callbacks2 = messageData.Callbacks;
			callbacks2.OnStoreOpened = (Action)Delegate.Combine(callbacks2.OnStoreOpened, (Action)delegate
			{
				if (ServiceManager.TryGet<InGameMessageScheduler>(out var service2))
				{
					InGameMessageTelemetry.SendMessageAction(message, service2.GetViewCount(message), InGameMessageAction.ActionType.OPENED_SHOP);
				}
			});
			UIMessageCallbacks callbacks3 = messageData.Callbacks;
			callbacks3.OnDisplayed = (Action)Delegate.Combine(callbacks3.OnDisplayed, (Action)delegate
			{
				InGameMessageTelemetry.SendMessageDisplayed(message.ContentType, message.UID, message.Title);
			});
			messagesToAdd.Add(messageData);
		}
		AddMessagesForDisplay(messagesToAdd);
	}

	private void AddMessagesForDisplay(List<MessageUIData> messagesToAdd)
	{
		if (ServiceManager.TryGet<MessagePopupDisplay>(out var popupDisplay))
		{
			popupDisplay.AddMessages(messagesToAdd, downloadExternalAssets: true);
			return;
		}
		if (ServiceManager.GetServiceState<MessagePopupDisplay>() == ServiceLocator.ServiceState.Error)
		{
			Log.InGameMessage.PrintWarning("MessagePopupDisplay service failed to initalize so IGM will fail and be ignored...");
			return;
		}
		m_messageQueueBacklog.AddRange(messagesToAdd);
		ServiceManager.CurrentServiceLocator.RemoveServiceStateChangedListener(OnServiceStateChanged);
		ServiceManager.CurrentServiceLocator.AddServiceStateChangedListener(OnServiceStateChanged);
		Log.InGameMessage.PrintWarning("Could not get MessagePopupDisplay service to display in game message");
	}

	private void OnServiceStateChanged(Type serviceType, ServiceLocator.ServiceState state)
	{
		if (serviceType != typeof(MessagePopupDisplay))
		{
			return;
		}
		switch (state)
		{
		case ServiceLocator.ServiceState.Error:
			Log.InGameMessage.PrintWarning("MessagePopupDisplay service failed to initalize so IGM will fail and be ignored...");
			ServiceManager.CurrentServiceLocator.RemoveServiceStateChangedListener(OnServiceStateChanged);
			break;
		case ServiceLocator.ServiceState.Ready:
		{
			ServiceManager.CurrentServiceLocator.RemoveServiceStateChangedListener(OnServiceStateChanged);
			if (!ServiceManager.TryGet<MessagePopupDisplay>(out var popupDisplay))
			{
				Log.InGameMessage.PrintError("Could not get MessagePopupDisplay service to display in game message after waiting for it to be ready!");
				break;
			}
			List<MessageUIData> copiedData = new List<MessageUIData>(m_messageQueueBacklog);
			m_messageQueueBacklog.Clear();
			popupDisplay.AddMessages(copiedData, downloadExternalAssets: true);
			break;
		}
		}
	}

	private MessageContentFeed FindFeedForMessage(GameMessage message)
	{
		if (message == null)
		{
			return null;
		}
		return FindFeedForContentType(message.ContentType);
	}

	private MessageContentFeed FindFeedForContentType(string contentType)
	{
		return m_feeds.Find((MessageContentFeed x) => x.ContentType.Equals(contentType));
	}
}
