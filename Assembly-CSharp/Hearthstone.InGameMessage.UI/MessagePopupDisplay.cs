using System;
using System.Collections;
using System.Collections.Generic;
using Blizzard.BlizzardErrorMobile;
using Blizzard.T5.Jobs;
using Blizzard.T5.Services;
using Hearthstone.Core;
using Hearthstone.UI;
using Hearthstone.Util;
using UnityEngine;

namespace Hearthstone.InGameMessage.UI;

public class MessagePopupDisplay : IService
{
	private MessageUIData m_currentlyDisplayedMessage;

	private static readonly AssetReference m_modalMessageReference = new AssetReference("MessageModal.prefab:7d258ca7826c5ba4c8e86d37eb6e909d");

	private Widget m_modalWidget;

	private MessageModal m_messageModal;

	private List<MessageUIData> m_allMessageList = new List<MessageUIData>();

	private List<MessageUIData> m_messageDisplayList = new List<MessageUIData>();

	private List<MessageUIData> m_invalidMessages = new List<MessageUIData>();

	private PopupEvent m_lastEventId;

	private DateTime m_lastEventTriggerTime;

	private const float EVENT_TRIGGER_WINDOW_THRESHOLD = 5f;

	private const float BLUR_FADE_TIME = 1f;

	private Action m_onClosed;

	private Action m_onDialogInstanceClosed;

	private IInGameMessageEventControl m_inGameMessageEventControl;

	private InGameMessageExternalAssetDownloader m_inGameMessageExternalAssetDownloader;

	private MessageFeedRegistry m_feedRegistry;

	private InGameMessageScheduler m_IGMscheduler;

	private SceneMgr m_sceneMgr;

	private ScreenEffectsHandle m_screenEffectsHandle;

	public bool HasMessageToDisplay => m_messageDisplayList.Count > 0;

	public bool IsDisplayingMessage { get; private set; }

	public PopupEvent CurrentPopupEvent => m_lastEventId;

	public IEnumerator<IAsyncJobResult> Initialize(ServiceLocator serviceLocator)
	{
		m_feedRegistry = new MessageFeedRegistry();
		m_feedRegistry.RegisterDefaultFeeds();
		m_inGameMessageExternalAssetDownloader = new InGameMessageExternalAssetDownloader();
		m_inGameMessageEventControl = new InGameMessageEventControl();
		m_inGameMessageEventControl.Initialize(TriggerEvent);
		if (ServiceManager.TryGet<InGameMessageScheduler>(out var scheduler))
		{
			m_IGMscheduler = scheduler;
		}
		else
		{
			Log.InGameMessage.PrintError("Could not retrieve InGameMessageScheduler. Can't display IGM popup.");
		}
		serviceLocator.Get<NetCache>().RegisterUpdatedListener(typeof(NetCache.NetCacheFeatures), OnFeaturesUpdated);
		m_sceneMgr = serviceLocator.Get<SceneMgr>();
		m_sceneMgr.RegisterSceneLoadedEvent(OnSceneLoaded);
		ServiceManager.Get<HearthstoneCheckout>().RegisterReadyCallback(delegate
		{
			if (m_invalidMessages.Count > 0)
			{
				List<MessageUIData> messagesToAdd = new List<MessageUIData>(m_invalidMessages);
				AddMessages(messagesToAdd);
			}
		});
		m_screenEffectsHandle = new ScreenEffectsHandle(this);
		yield break;
	}

	public Type[] GetDependencies()
	{
		return new Type[5]
		{
			typeof(InGameMessageScheduler),
			typeof(LoginManager),
			typeof(NetCache),
			typeof(HearthstoneCheckout),
			typeof(SceneMgr)
		};
	}

	public void Shutdown()
	{
		m_sceneMgr.RegisterSceneLoadedEvent(OnSceneLoaded);
		m_inGameMessageEventControl.Dispose();
	}

	public void TriggerEvent(PopupEvent eventID)
	{
		TriggerEvent(eventID, null);
	}

	public void TriggerEvent(PopupEvent eventID, MessageUIData defaultMessage = null)
	{
		Log.InGameMessage.PrintDebug(($"Trigger event {eventID}" + defaultMessage == null) ? "" : " with defaultMessage");
		m_lastEventId = eventID;
		m_lastEventTriggerTime = DateTime.UtcNow;
		m_messageDisplayList.Clear();
		QueueMatchingMessagesForDisplay(eventID, defaultMessage);
	}

	public int GetMessageCount(PopupEvent eventID)
	{
		return BuildMessageListToDisplay(eventID)?.Count ?? 0;
	}

	public int GetUnreadMessageCountForFeed(PopupEvent eventID)
	{
		List<MessageUIData> messagesToDisplay = BuildMessageListToDisplay(eventID);
		int numUnread = 0;
		if (ServiceManager.TryGet<InGameMessageScheduler>(out var scheduler))
		{
			foreach (MessageUIData message in messagesToDisplay)
			{
				if (scheduler.GetViewCount(message) <= 0 && MessageValidator.IsMessageValid(message))
				{
					numUnread++;
				}
			}
		}
		return numUnread;
	}

	private List<MessageUIData> BuildMessageListToDisplay(PopupEvent eventID)
	{
		List<MessageUIData> list = m_allMessageList.FindAll((MessageUIData x) => x.EventId == eventID.ToString());
		List<MessageUIData> messagesToDisplay = new List<MessageUIData>();
		foreach (MessageUIData message in list)
		{
			if (!InGameMessageUtils.IncludesRegion(message.Region, RegionUtils.CurrentRegion))
			{
				Log.InGameMessage.PrintDebug($"Message does not match region. mID: {message.UID}, mTitle: {message.MessageData.Title} , eventID: {eventID}");
			}
			else if (message.MaxViewCount > 0 && m_IGMscheduler.GetViewCount(message) >= message.MaxViewCount)
			{
				Log.InGameMessage.PrintDebug($"Message reached max view count.  mID: {message.UID}, mTitle:{message.MessageData.Title} , eventID: {eventID}");
			}
			else if (message.LayoutType == MessageLayoutType.SHOP && !StoreManager.Get().IsOpen())
			{
				Log.InGameMessage.PrintDebug("Shop data is not ready, shop message to be displayed later." + $" mID: {message.UID}, mTitle: {message.MessageData.Title} , eventID: {eventID}");
			}
			else
			{
				messagesToDisplay.Add(message);
			}
		}
		return messagesToDisplay;
	}

	private void QueueMatchingMessagesForDisplay(PopupEvent eventID, MessageUIData defaultMessage = null)
	{
		List<MessageUIData> messagesToDisplay = BuildMessageListToDisplay(eventID);
		if (messagesToDisplay == null)
		{
			Log.InGameMessage.PrintDebug($"No messages to display for eventID: {eventID}");
			return;
		}
		if (defaultMessage != null && messagesToDisplay.Count == 0)
		{
			m_messageDisplayList.AddRange(new List<MessageUIData> { defaultMessage });
			UIMessageCallbacks callbacks = defaultMessage.Callbacks;
			callbacks.OnClosed = (Action)Delegate.Remove(callbacks.OnClosed, new Action(OnMessageClosed));
			UIMessageCallbacks callbacks2 = defaultMessage.Callbacks;
			callbacks2.OnClosed = (Action)Delegate.Combine(callbacks2.OnClosed, new Action(OnMessageClosed));
			InGameMessageTelemetry.SendDisplayQueueCountForEvent(eventID.ToString(), 0);
			return;
		}
		foreach (MessageUIData message in SortMessages(messagesToDisplay))
		{
			if (!MessageExistIn(message, m_messageDisplayList))
			{
				m_messageDisplayList.Add(message);
			}
		}
		foreach (MessageUIData messageDisplay in m_messageDisplayList)
		{
			UIMessageCallbacks callbacks3 = messageDisplay.Callbacks;
			callbacks3.OnClosed = (Action)Delegate.Remove(callbacks3.OnClosed, new Action(OnMessageClosed));
			UIMessageCallbacks callbacks4 = messageDisplay.Callbacks;
			callbacks4.OnClosed = (Action)Delegate.Combine(callbacks4.OnClosed, new Action(OnMessageClosed));
		}
		InGameMessageTelemetry.SendDisplayQueueCountForEvent(m_lastEventId.ToString(), m_messageDisplayList.Count);
	}

	private List<MessageUIData> SortMessages(List<MessageUIData> messages)
	{
		InGameMessageScheduler scheduler;
		bool schedulerExists = ServiceManager.TryGet<InGameMessageScheduler>(out scheduler);
		messages.Sort(delegate(MessageUIData x, MessageUIData y)
		{
			int num = (schedulerExists ? scheduler.GetViewCount(x) : 0);
			int num2 = (schedulerExists ? scheduler.GetViewCount(y) : 0);
			if (num <= 0 && 0 < num2)
			{
				return -1;
			}
			return (0 < num && num2 <= 0) ? 1 : x.Priority.CompareTo(y.Priority);
		});
		return messages;
	}

	public void AddMessages(List<MessageUIData> messagesToAdd, bool downloadExternalAssets = false)
	{
		if (downloadExternalAssets)
		{
			m_inGameMessageExternalAssetDownloader.DownloadMessagesExternalAssets(messagesToAdd, delegate
			{
				AddAndValidateMessages(messagesToAdd);
			});
		}
		else
		{
			AddAndValidateMessages(messagesToAdd);
		}
	}

	private void AddAndValidateMessages(List<MessageUIData> messagesToAdd)
	{
		foreach (MessageUIData data in messagesToAdd)
		{
			if (!string.IsNullOrEmpty(data.UID))
			{
				if (MessageIsBeingDisplayed(data))
				{
					Log.InGameMessage.PrintInfo("Message " + data.UID + " is already being display. Ignoring the message");
					continue;
				}
				if (MessageExistIn(data, m_allMessageList, updateValuesIfExist: true) || MessageExistIn(data, m_messageDisplayList, updateValuesIfExist: true))
				{
					continue;
				}
			}
			if (IsValidMessage(data))
			{
				m_allMessageList.Add(data);
			}
		}
		if (messagesToAdd.Count > 0 && m_lastEventId != 0 && !TryDisplayingDelayedMessages())
		{
			InGameMessageTelemetry.SendDelayedMessageData(messagesToAdd[0].ContentType, m_lastEventId.ToString());
		}
	}

	public void RemoveMessage(MessageUIData messgeToRemove)
	{
		m_allMessageList.Remove(messgeToRemove);
		if (IsDisplayingMessage)
		{
			m_messageModal?.OnMessageRemoved();
		}
	}

	private bool TryDisplayingDelayedMessages()
	{
		if ((DateTime.UtcNow - m_lastEventTriggerTime).TotalSeconds <= 5.0)
		{
			Log.InGameMessage.PrintDebug("Messages received within last event trigger window. Matching messages will be displayed.");
			QueueMatchingMessagesForDisplay(m_lastEventId);
			return true;
		}
		Log.InGameMessage.PrintWarning("Messages received after last event trigger window. They will be displayed later.");
		return false;
	}

	private bool IsValidMessage(MessageUIData data)
	{
		if (!MessageValidator.IsMessageValid(data))
		{
			if (!MessageExistIn(data, m_invalidMessages))
			{
				InGameMessageTelemetry.SendMessageValidationError(data.ContentType, data.MessageData.Title, data.UID);
				Log.InGameMessage.PrintWarning("Invalid message given with: Content Type " + data.ContentType + ", Title " + data.MessageData.Title + " UID  " + data.UID + ". Ignoring message");
				m_invalidMessages.Add(data);
			}
			return false;
		}
		if (MessageExistIn(data, m_invalidMessages))
		{
			m_invalidMessages.Remove(data);
		}
		return true;
	}

	private void OnFeaturesUpdated()
	{
		if (ServiceManager.TryGet<NetCache>(out var netCache))
		{
			NetCache.NetCacheFeatures features = netCache.GetNetObject<NetCache.NetCacheFeatures>();
			if (features != null && features.PersonalizedMessagesEnabled)
			{
				m_feedRegistry.EnablePersonalizedMessages();
			}
			else
			{
				m_feedRegistry.DisablePersonalizedMessages();
			}
		}
	}

	public void DisplayIGMMessage(Action onClosed)
	{
		if (IsDisplayingMessage)
		{
			Log.InGameMessage.PrintWarning("Attempted to display the an in game message while one is already being shown");
			return;
		}
		IsDisplayingMessage = true;
		if (onClosed != null)
		{
			m_onClosed = (Action)Delegate.Remove(m_onClosed, onClosed);
			m_onClosed = (Action)Delegate.Combine(m_onClosed, onClosed);
		}
		Processor.RunCoroutine(DisplayMessagesWhenReady(m_messageDisplayList));
	}

	private IEnumerator DisplayMessagesWhenReady(List<MessageUIData> messagesToDisplay)
	{
		if (m_modalWidget == null)
		{
			CreateModal();
		}
		while (m_modalWidget == null || !m_modalWidget.IsReady || m_messageModal == null)
		{
			yield return null;
		}
		try
		{
			m_messageModal.SetMessageList(messagesToDisplay, OnSetCurrentMessage);
			m_modalWidget.Show();
			UIContext.GetRoot().ShowPopup(m_modalWidget.gameObject);
		}
		catch (Exception ex)
		{
			Log.InGameMessage.PrintError("Exception showing IGM. Forcing close: " + ex.Message);
			m_currentlyDisplayedMessage.Callbacks.OnClosed?.Invoke();
			OnMessageClosed();
			ExceptionReporter.Get()?.ReportCaughtException(ex);
		}
	}

	private void OnSetCurrentMessage(MessageUIData message)
	{
		m_currentlyDisplayedMessage = message;
	}

	private void OnModalWidgetReady(object _)
	{
		if (m_modalWidget != null)
		{
			InitializeModalWidget(m_modalWidget);
		}
	}

	private void InitializeModalWidget(Widget modalWidget)
	{
		m_messageModal = modalWidget.GetComponentInChildren<MessageModal>();
		if (m_messageModal == null)
		{
			Log.InGameMessage.PrintError("Could not find Message Modal component. IGM will not function!");
		}
	}

	private void CreateModal()
	{
		m_modalWidget = WidgetInstance.Create(m_modalMessageReference);
		m_modalWidget.RegisterReadyListener(OnModalWidgetReady);
		OverlayUI.Get().AddGameObject(m_modalWidget.gameObject);
	}

	public void RegisterOnDialogInstanceClosedAction(Action onClosed)
	{
		if (onClosed != null)
		{
			m_onDialogInstanceClosed = (Action)Delegate.Combine(m_onDialogInstanceClosed, onClosed);
		}
	}

	private void OnMessageClosed()
	{
		m_messageDisplayList.Clear();
		m_currentlyDisplayedMessage = null;
		m_onClosed?.Invoke();
		m_onDialogInstanceClosed?.Invoke();
		m_onDialogInstanceClosed = null;
		DestroyModal();
		IsDisplayingMessage = false;
	}

	private void DestroyModal()
	{
		if ((bool)m_modalWidget)
		{
			UIContext.GetRoot().DismissPopup(m_modalWidget.gameObject);
			UnityEngine.Object.Destroy(m_modalWidget);
			m_modalWidget = null;
			m_messageModal = null;
		}
	}

	private void ActivateFullscreenBlur()
	{
		BnetBar.Get()?.RequestDisableButtons();
		ScreenEffectParameters screenEffectParameters = ScreenEffectParameters.BlurVignetteDesaturatePerspective;
		screenEffectParameters.Time = 1f;
		m_screenEffectsHandle.StartEffect(screenEffectParameters);
	}

	private void DeactivateFullscreenBlur()
	{
		BnetBar.Get()?.CancelRequestToDisableButtons();
		m_screenEffectsHandle.StopEffect();
	}

	private bool MessageExistIn(MessageUIData data, IEnumerable<MessageUIData> collection, bool updateValuesIfExist = false)
	{
		foreach (MessageUIData listedMessage in collection)
		{
			if (MessagesHaveSameUID(listedMessage, data))
			{
				if (updateValuesIfExist)
				{
					listedMessage.CopyValues(data);
					Log.InGameMessage.PrintInfo("Message was queued to display. Updating the stored values. Content Type " + data.ContentType + ", Title " + data.MessageData.Title + " UID  " + data.UID + ".");
				}
				return true;
			}
		}
		return false;
	}

	private bool MessageIsBeingDisplayed(MessageUIData data)
	{
		if (m_currentlyDisplayedMessage != null)
		{
			return MessagesHaveSameUID(m_currentlyDisplayedMessage, data);
		}
		return false;
	}

	private bool MessagesHaveSameUID(MessageUIData first, MessageUIData second)
	{
		if (!string.IsNullOrEmpty(first.UID) && !string.IsNullOrEmpty(second.UID))
		{
			return first.UID.Equals(second.UID);
		}
		return false;
	}

	private void OnSceneLoaded(SceneMgr.Mode prevMode, PegasusScene prevScene, object userData)
	{
		if (IsDisplayingMessage)
		{
			m_messageModal.ForceClose();
		}
	}
}
