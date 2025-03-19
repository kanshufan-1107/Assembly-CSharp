using System;
using System.Collections.Generic;
using Blizzard.Telemetry.WTCG.Client;
using Hearthstone.DataModels;
using Hearthstone.UI;
using UnityEngine;

namespace Hearthstone.InGameMessage.UI;

public class MessageModal : MonoBehaviour
{
	private Widget m_contentWidget;

	private UIMessageCallbacks m_callbacks;

	private long m_productId;

	private bool m_openFullShop;

	private string m_shopDeepLink;

	private List<MessageUIData> m_messageDataList;

	private Action<MessageUIData> m_onSetMessage;

	private MessageUIData m_currentMessage;

	private int m_currentMessageIndex;

	private List<string> m_viewedMessageUIDs;

	private MessageModalContentPager m_messageModalContentPager;

	private const string SHOP_BUTTON_EVENT = "ShopButtonPressed";

	private const string FINAL_CLOSE_BUTTON_EVENT = "FinalalizeCloseButtonPress";

	private const string PREVIOUS_BUTTON_EVENT = "PreviousButtonPressed";

	private const string NEXT_BUTTON_EVENT = "NextButtonPressed";

	public static event Action ShopButtonPressed;

	private void Awake()
	{
		m_contentWidget = GetComponent<Widget>();
		m_contentWidget.RegisterEventListener(OnEventMessage);
		m_messageModalContentPager = GetComponent<MessageModalContentPager>();
	}

	public void SetMessageList(List<MessageUIData> dataList, Action<MessageUIData> onSetMessageCallback)
	{
		if (dataList == null)
		{
			Log.InGameMessage.PrintWarning("Failed to set In game message, data list was null");
			return;
		}
		m_viewedMessageUIDs = new List<string>();
		m_currentMessageIndex = 0;
		m_messageModalContentPager?.SetMessageUIData(dataList);
		m_messageDataList = dataList;
		m_onSetMessage = onSetMessageCallback;
		SetMessage(m_messageDataList[m_currentMessageIndex]);
	}

	private void SetMessage(MessageUIData data)
	{
		if (data == null)
		{
			Log.InGameMessage.PrintWarning("Failed to set In game message, data was null");
			return;
		}
		m_currentMessage = data;
		SetModalDataModel(data);
		SetShopSettingsIfAvailable(data);
		m_callbacks = data.Callbacks;
		m_callbacks.OnDisplayed?.Invoke();
		m_onSetMessage?.Invoke(data);
	}

	private void SetShopSettingsIfAvailable(MessageUIData data)
	{
		if (data.LayoutType == MessageLayoutType.SHOP)
		{
			ShopMessageContent content = data.MessageData as ShopMessageContent;
			m_productId = content.ProductID;
			m_openFullShop = content.OpenFullShop;
			m_shopDeepLink = content.ShopDeepLink;
		}
	}

	private void SetModalDataModel(MessageUIData data)
	{
		MessageModalDataModel modalDataModel = new MessageModalDataModel();
		modalDataModel.LayoutType = MessageModalUtils.GetLayoutTypeID(data.LayoutType);
		modalDataModel.PageArrowPrevious = GetPageArrowPrevious();
		modalDataModel.PageArrowNext = GetPageArrowNext();
		m_contentWidget.BindDataModel(modalDataModel);
	}

	private void SetMessageViewed(InGameMessageAction.ActionType action)
	{
		if (m_currentMessage == null)
		{
			Log.InGameMessage.PrintError("Current message is null. Something went wrong.");
		}
		else if (!m_viewedMessageUIDs.Contains(m_currentMessage.UID))
		{
			m_viewedMessageUIDs.Add(m_currentMessage.UID);
			m_callbacks?.OnViewed?.Invoke(action);
		}
	}

	public void OnEventMessage(string eventName)
	{
		if (eventName.Equals("FinalalizeCloseButtonPress", StringComparison.OrdinalIgnoreCase))
		{
			OnClosePressed();
		}
		else if (eventName.Equals("ShopButtonPressed", StringComparison.OrdinalIgnoreCase))
		{
			OnOpenStoreButton();
		}
		else if (eventName.Equals("PreviousButtonPressed", StringComparison.OrdinalIgnoreCase))
		{
			OnPreviousButtonPressed();
		}
		else if (eventName.Equals("NextButtonPressed", StringComparison.OrdinalIgnoreCase))
		{
			OnNextButtonPressed();
		}
	}

	public void OnClosePressed()
	{
		Log.InGameMessage.PrintDebug("Modal close button pressed");
		SetMessageViewed(InGameMessageAction.ActionType.CLOSE);
		m_callbacks?.OnClosed();
	}

	public void ForceClose()
	{
		Log.InGameMessage.PrintDebug("Modal forced closed");
		m_callbacks?.OnClosed();
	}

	private void OnOpenStoreButton()
	{
		Log.InGameMessage.PrintDebug($"Opening shop page for product ID {m_productId}");
		if (!string.IsNullOrEmpty(m_shopDeepLink))
		{
			DeepLinkManager.ExecuteDeepLink(new string[2]
			{
				m_shopDeepLink,
				m_productId.ToString()
			}, DeepLinkManager.DeepLinkSource.IN_GAME_MESSAGE, 0);
		}
		else
		{
			ProductPageJobs.OpenToProductPageWhenReady(m_productId, !m_openFullShop);
		}
		m_callbacks?.OnStoreOpened();
		OnClosePressed();
		MessageModal.ShopButtonPressed?.Invoke();
	}

	private void OnPreviousButtonPressed()
	{
		Log.InGameMessage.PrintDebug("Modal previous button pressed");
		SetMessageViewed(InGameMessageAction.ActionType.SCROLL_TO_NEXT);
		m_currentMessageIndex = GetPreviousMessageIndex();
		SetMessage(m_messageDataList[m_currentMessageIndex]);
		m_messageModalContentPager?.OnPageButtonPressed(m_currentMessageIndex, nextPressed: false);
	}

	private void OnNextButtonPressed()
	{
		Log.InGameMessage.PrintDebug("Modal next button pressed");
		SetMessageViewed(InGameMessageAction.ActionType.SCROLL_TO_NEXT);
		m_currentMessageIndex = GetNextMessageIndex();
		SetMessage(m_messageDataList[m_currentMessageIndex]);
		m_messageModalContentPager?.OnPageButtonPressed(m_currentMessageIndex, nextPressed: true);
	}

	private int GetNextMessageIndex()
	{
		return Math.Min(m_currentMessageIndex + 1, m_messageDataList.Count - 1);
	}

	private int GetPreviousMessageIndex()
	{
		return Math.Max(m_currentMessageIndex - 1, 0);
	}

	private MessagePageArrowDataModel GetPageArrowPrevious()
	{
		return new MessagePageArrowDataModel
		{
			Active = (m_currentMessageIndex > 0),
			IsNew = false
		};
	}

	private MessagePageArrowDataModel GetPageArrowNext()
	{
		bool active = m_currentMessageIndex < m_messageDataList.Count - 1;
		bool isNextMessageNew = false;
		if (active)
		{
			isNextMessageNew = !m_viewedMessageUIDs.Contains(m_messageDataList[GetNextMessageIndex()].UID);
		}
		return new MessagePageArrowDataModel
		{
			Active = active,
			IsNew = isNextMessageNew
		};
	}

	internal void OnMessageRemoved()
	{
		int index = ((m_currentMessage != null) ? m_messageDataList.IndexOf(m_currentMessage) : (-1));
		m_currentMessageIndex = ((index != -1) ? index : Math.Max(0, m_currentMessageIndex - 1));
	}
}
