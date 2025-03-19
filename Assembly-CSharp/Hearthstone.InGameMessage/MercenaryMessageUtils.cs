using System;
using System.Collections.Generic;
using Assets;
using Blizzard.T5.Services;
using Blizzard.Telemetry.WTCG.Client;
using Hearthstone.DataModels;
using Hearthstone.InGameMessage.UI;

namespace Hearthstone.InGameMessage;

public class MercenaryMessageUtils
{
	public static readonly string MERC_WELCOME_MESSAGE_UID = "MERC_WELCOME_MESSAGE_UID";

	public static void AddMercenaryWelcomeMessage()
	{
		MessagePopupDisplay popupDisplay = ServiceManager.Get<MessagePopupDisplay>();
		if (popupDisplay == null)
		{
			UIStatus.Get().AddError("Message Popup Display was not available to add Mercenary welcome message");
			return;
		}
		MessageUIData welcomeMessage = new MessageUIData
		{
			UID = MERC_WELCOME_MESSAGE_UID,
			EventId = "OnMercInbox",
			LayoutType = MessageLayoutType.TEXT,
			Priority = 0,
			MessageData = new TextMessageContent
			{
				ImageType = "mercs",
				Title = GameStrings.Get("GLUE_MERCENARY_WELCOME_MESSAGE_TITLE"),
				TextBody = GameStrings.Get("GLUE_MERCENARY_WELCOME_MESSAGE_BODY")
			}
		};
		UIMessageCallbacks callbacks = welcomeMessage.Callbacks;
		callbacks.OnViewed = (Action<InGameMessageAction.ActionType>)Delegate.Combine(callbacks.OnViewed, (Action<InGameMessageAction.ActionType>)delegate
		{
			popupDisplay.RemoveMessage(welcomeMessage);
		});
		popupDisplay.AddMessages(new List<MessageUIData> { welcomeMessage });
	}

	public static MessageUIData GetEmptyMailboxMessage()
	{
		return new MessageUIData
		{
			LayoutType = MessageLayoutType.EMPTY,
			MessageData = new TextMessageContent
			{
				ImageType = "merc_empty",
				Title = GameStrings.Get("GLUE_INGAME_MESSAGING_MAILBOX_EMPTY")
			}
		};
	}

	public static bool HasCompletedMercenaryVillageShopTutorial()
	{
		return LettuceTutorialUtils.IsEventTypeComplete(LettuceTutorialVo.LettuceTutorialEvent.VILLAGE_TUTORIAL_VISIT_SHOP);
	}

	public static bool IsMercenaryVillageShopAvailable()
	{
		if (!HasCompletedMercenaryVillageShopTutorial())
		{
			return LettuceTutorialUtils.IsEventTypeComplete(LettuceTutorialVo.LettuceTutorialEvent.VILLAGE_TUTORIAL_SHOP_BUILD_START);
		}
		return true;
	}

	public static bool IsMercenaryOnlyShopItem(RewardItemType rewardItemType)
	{
		switch (rewardItemType)
		{
		case RewardItemType.MERCENARY_COIN:
		case RewardItemType.MERCENARY:
		case RewardItemType.MERCENARY_XP:
		case RewardItemType.MERCENARY_EQUIPMENT:
		case RewardItemType.MERCENARY_EQUIPMENT_ICON:
		case RewardItemType.MERCENARY_BOOSTER:
		case RewardItemType.MERCENARY_RANDOM_MERCENARY:
		case RewardItemType.MERCENARY_KNOCKOUT_SPECIFIC:
		case RewardItemType.MERCENARY_KNOCKOUT_RANDOM:
		case RewardItemType.MERCENARY_RENOWN:
			return true;
		default:
			return false;
		}
	}

	public static bool TryGetMercenaryRewardItemDataModel(string itemId, TAG_PREMIUM itemPremiumType, int mercArtVariationId, out RewardItemDataModel mercRewardData)
	{
		try
		{
			int.TryParse(itemId, out var mercId);
			mercRewardData = RewardUtils.CreateMercenaryRewardItemDataModel(mercId, mercArtVariationId, itemPremiumType);
			mercRewardData.Mercenary.HideStats = itemPremiumType != TAG_PREMIUM.NORMAL;
			return true;
		}
		catch
		{
			Log.InGameMessage.PrintError("Unable to create mercenary reward item data model: " + itemId);
			mercRewardData = null;
			return false;
		}
	}
}
