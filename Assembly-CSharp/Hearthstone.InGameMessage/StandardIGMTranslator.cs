using System;
using System.Collections.Generic;
using Hearthstone.InGameMessage.UI;

namespace Hearthstone.InGameMessage;

public sealed class StandardIGMTranslator : IDataTranslator
{
	public const string TEXT_LAYOUT_NAME = "Simple Text";

	public const string SHOP_LAYOUT_NAME = "Shop";

	public const string LAUNCH_LAYOUT_NAME = "Launch";

	public const string CHANGE_LAYOUT_NAME = "Card Changes";

	public const string MERCENARY_LAYOUT_NAME = "Mercenary";

	public const string MERCENARY_BOUNTY_LAYOUT_NAME = "Mercenary Bounty";

	public MessageUIData CreateData(GameMessage message)
	{
		if (string.IsNullOrEmpty(message.LayoutType))
		{
			Log.InGameMessage.PrintInfo("Could not translate IGM data, missing layout type");
			return null;
		}
		Log.InGameMessage.PrintDebug("Translating message type {0}", message.LayoutType);
		if (message.LayoutType.Equals("Simple Text", StringComparison.OrdinalIgnoreCase))
		{
			return TranslateTextMessage(message);
		}
		if (message.LayoutType.Equals("Shop", StringComparison.OrdinalIgnoreCase))
		{
			return TranslateShopMessage(message);
		}
		if (message.LayoutType.Equals("Launch", StringComparison.OrdinalIgnoreCase))
		{
			return TranslateLaunchMessage(message);
		}
		if (message.LayoutType.Equals("Card Changes", StringComparison.OrdinalIgnoreCase))
		{
			return TranslateChangeMessage(message);
		}
		if (message.LayoutType.Equals("Mercenary", StringComparison.OrdinalIgnoreCase))
		{
			return TranslateMercenaryItemsMessage(message);
		}
		if (message.LayoutType.Equals("Mercenary Bounty", StringComparison.OrdinalIgnoreCase))
		{
			return TranslateMercenaryBountyItemsMessage(message);
		}
		Log.InGameMessage.PrintInfo("Could not find data translator for IGM layout {0}", message.LayoutType);
		return null;
	}

	private static MessageUIData TranslateChangeMessage(GameMessage message)
	{
		if (string.IsNullOrEmpty(message.Title))
		{
			Log.InGameMessage.PrintInfo("IGM Change message was missing a title and is not valid");
			return null;
		}
		if (string.IsNullOrEmpty(message.TextBody))
		{
			Log.InGameMessage.PrintInfo("IGM Change message was missing the body text and is not valid");
			return null;
		}
		ChangeMessageContent messageData = new ChangeMessageContent
		{
			Title = message.Title,
			BodyText = message.TextBody,
			Url = AddDynamicItemsToUrl(message.Link)
		};
		if (message.ItemDisplay != null && message.ItemDisplay.Count > 0)
		{
			messageData.ChangeItems = new List<ChangeMessageItemInformation>(message.ItemDisplay.Count);
			foreach (InGameMessageItemDisplayContent curItem in message.ItemDisplay)
			{
				messageData.ChangeItems.Add(new ChangeMessageItemInformation
				{
					ItemType = curItem.ChangeType,
					ItemId = curItem.itemChangeId,
					itemPremiumType = curItem.itemPremiumType
				});
			}
		}
		return CreateUIData(message, MessageLayoutType.CHANGE, messageData);
	}

	private static MessageUIData TranslateTextMessage(GameMessage message)
	{
		if (string.IsNullOrEmpty(message.Title))
		{
			Log.InGameMessage.PrintInfo("IGM Text message was missing a title and is not valid");
			return null;
		}
		if (string.IsNullOrEmpty(message.DisplayImageType))
		{
			Log.InGameMessage.PrintInfo("IGM Text message was missing a display image type and is not valid");
			return null;
		}
		if (string.IsNullOrEmpty(message.TextBody))
		{
			Log.InGameMessage.PrintInfo("IGM Text message was missing the body text and is not valid");
			return null;
		}
		TextMessageContent messageData = new TextMessageContent
		{
			Title = message.Title,
			ImageType = message.DisplayImageType,
			TextBody = message.TextBody,
			ImageMaterial = (string.IsNullOrEmpty(message.ImageMaterial) ? null : message.ImageMaterial),
			ImageTexture = (string.IsNullOrEmpty(message.TextureReference) ? null : message.TextureReference),
			Url = AddDynamicItemsToUrl(message.Link)
		};
		if (message.ItemDisplay != null && message.ItemDisplay.Count > 0)
		{
			messageData.DisplayItems = new List<TextMessageItemInformation>(message.ItemDisplay.Count);
			foreach (InGameMessageItemDisplayContent curItem in message.ItemDisplay)
			{
				messageData.DisplayItems.Add(new TextMessageItemInformation
				{
					ItemType = curItem.ChangeType,
					ItemId = curItem.itemChangeId,
					itemPremiumType = curItem.itemPremiumType,
					mercArtVariationId = curItem.mercArtVariationId
				});
			}
		}
		return CreateUIData(message, MessageLayoutType.TEXT, messageData);
	}

	private static MessageUIData TranslateShopMessage(GameMessage message)
	{
		if (string.IsNullOrEmpty(message.Title))
		{
			Log.InGameMessage.PrintInfo("IGM Shop message was missing a title and is not valid");
			return null;
		}
		if (string.IsNullOrEmpty(message.TextBody))
		{
			Log.InGameMessage.PrintInfo("IGM Shop message was missing the body text and is not valid");
			return null;
		}
		if (message.ProductID == 0L)
		{
			Log.InGameMessage.PrintInfo("IGM Shop message was missing a product id and is not valid");
			return null;
		}
		ShopMessageContent messageContent = new ShopMessageContent
		{
			Title = message.Title,
			TextBody = message.TextBody,
			ProductID = message.ProductID,
			OpenFullShop = message.OpenFullShop,
			ShopDeepLink = message.ShopDeepLink,
			TextureAssetUrl = message.TextureAssetUrl
		};
		return CreateUIData(message, MessageLayoutType.SHOP, messageContent);
	}

	private static MessageUIData TranslateLaunchMessage(GameMessage message)
	{
		if (string.IsNullOrEmpty(message.Title))
		{
			Log.InGameMessage.PrintInfo("IGM Launch message was missing a title and is not valid");
			return null;
		}
		LaunchMessageContent messageContent = new LaunchMessageContent
		{
			Title = message.Title,
			IconType = message.DisplayImageType,
			ImageMaterial = (string.IsNullOrEmpty(message.ImageMaterial) ? null : message.ImageMaterial),
			ImageTexture = (string.IsNullOrEmpty(message.TextureReference) ? null : message.TextureReference),
			SubLayout = (string.IsNullOrEmpty(message.LaunchSubLayoutType) ? "mode" : message.LaunchSubLayoutType),
			Effect = new LaunchMessageEffectContent
			{
				EffectId = message.LaunchEffect.EffectId,
				EffectColor = message.LaunchEffect.EffectColor,
				EffectSoundId = message.LaunchEffect.EffectSoundId
			},
			Url = AddDynamicItemsToUrl(message.Link)
		};
		return CreateUIData(message, MessageLayoutType.LAUNCH, messageContent);
	}

	private static MessageUIData TranslateMercenaryItemsMessage(GameMessage message)
	{
		if (string.IsNullOrEmpty(message.Title))
		{
			Log.InGameMessage.PrintInfo("IGM Items message was missing a title and is not valid");
			return null;
		}
		if (string.IsNullOrEmpty(message.TextBody))
		{
			Log.InGameMessage.PrintInfo("IGM Items message was missing the body text and is not valid");
			return null;
		}
		MercenaryMessageContent messageData = new MercenaryMessageContent
		{
			Title = message.Title,
			BodyText = message.TextBody,
			Url = AddDynamicItemsToUrl(message.Link)
		};
		if (message.ItemDisplay != null && message.ItemDisplay.Count > 0)
		{
			messageData.Items = new List<MercenaryMessageItemInformation>(message.ItemDisplay.Count);
			foreach (InGameMessageItemDisplayContent curItem in message.ItemDisplay)
			{
				messageData.Items.Add(new MercenaryMessageItemInformation
				{
					ItemType = curItem.ChangeType,
					ItemId = curItem.itemChangeId,
					itemPremiumType = curItem.itemPremiumType,
					mercArtVariationId = curItem.mercArtVariationId
				});
			}
		}
		return CreateUIData(message, MessageLayoutType.MERCENARY, messageData);
	}

	private static MessageUIData TranslateMercenaryBountyItemsMessage(GameMessage message)
	{
		if (string.IsNullOrEmpty(message.Title))
		{
			Log.InGameMessage.PrintInfo("IGM Items message was missing a title and is not valid");
			return null;
		}
		if (string.IsNullOrEmpty(message.TextBody))
		{
			Log.InGameMessage.PrintInfo("IGM Items message was missing the body text and is not valid");
			return null;
		}
		MercenaryMessageBountyContent messageData = new MercenaryMessageBountyContent
		{
			Title = message.Title,
			BodyText = message.TextBody,
			Url = AddDynamicItemsToUrl(message.Link)
		};
		if (message.ItemDisplay != null && message.ItemDisplay.Count > 0)
		{
			messageData.Items = new List<MercenaryMessageBountyItemInformation>(message.ItemDisplay.Count);
			foreach (InGameMessageItemDisplayContent curItem in message.ItemDisplay)
			{
				messageData.Items.Add(new MercenaryMessageBountyItemInformation
				{
					ItemType = curItem.ChangeType,
					ItemId = int.Parse(curItem.itemChangeId)
				});
			}
		}
		return CreateUIData(message, MessageLayoutType.MERCENARY_BOUNTY, messageData);
	}

	private static MessageUIData CreateUIData(GameMessage message, MessageLayoutType type, IMessageContent content)
	{
		return new MessageUIData
		{
			UID = message.UID,
			EventId = message.EventId,
			Region = message.Region,
			ViewCountId = message.ViewCountId,
			Priority = message.PriorityLevel,
			ContentType = message.ContentType,
			LayoutType = type,
			MaxViewCount = message.MaxViewCount,
			MessageData = content
		};
	}

	private static string AddDynamicItemsToUrl(string url)
	{
		ulong? bnetId = BnetUtils.TryGetBnetAccountId();
		bool hasValidBnetId = bnetId.HasValue && bnetId.Value != 0;
		if (string.IsNullOrEmpty(url) || !hasValidBnetId)
		{
			return url;
		}
		return url.Replace("{{bnet_id}}", bnetId.Value.ToString());
	}
}
