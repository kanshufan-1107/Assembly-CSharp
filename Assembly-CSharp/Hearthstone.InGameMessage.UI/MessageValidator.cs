using System.Collections.Generic;
using Blizzard.Commerce;
using Blizzard.T5.Configuration;
using Blizzard.T5.Services;
using Hearthstone.DataModels;
using Hearthstone.Store;

namespace Hearthstone.InGameMessage.UI;

public class MessageValidator
{
	private const bool REQUIRE_PRODUCTS_PURCHASABLE_DEFAULT = true;

	public static bool IsMessageValid(MessageUIData messageData)
	{
		if (messageData == null)
		{
			return false;
		}
		return messageData.LayoutType switch
		{
			MessageLayoutType.TEXT => IsTextMessageValid(messageData.MessageData as TextMessageContent), 
			MessageLayoutType.SHOP => IsShopMessageValid(messageData.MessageData as ShopMessageContent), 
			MessageLayoutType.LAUNCH => IsLaunchMessageValid(messageData.MessageData as LaunchMessageContent), 
			MessageLayoutType.CHANGE => IsChangeMessageValid(messageData.MessageData as ChangeMessageContent), 
			MessageLayoutType.MERCENARY => IsMercenaryMessageValid(messageData.MessageData as MercenaryMessageContent), 
			MessageLayoutType.MERCENARY_BOUNTY => IsMercenaryMessageBountyValid(messageData.MessageData as MercenaryMessageBountyContent), 
			MessageLayoutType.EMPTY => IsEmptyMailboxMessageValid(messageData.MessageData as TextMessageContent), 
			_ => false, 
		};
	}

	private static bool IsEmptyMailboxMessageValid(TextMessageContent content)
	{
		if (IsRequiredStringValid(content.ImageType, "Image Type"))
		{
			return IsRequiredStringValid(content.Title, "Title");
		}
		return false;
	}

	private static bool IsMercenaryMessageBountyValid(MercenaryMessageBountyContent content)
	{
		if (IsRequiredStringValid(content.Title, "Title") && IsRequiredStringValid(content.BodyText, "Text Body") && content.Items.Count > 0)
		{
			return ValidateMercenaryMessageBountyIDs(content.Items);
		}
		return false;
	}

	private static bool ValidateMercenaryMessageBountyIDs(List<MercenaryMessageBountyItemInformation> items)
	{
		List<LettuceBountyDbfRecord> bountyRecords = GameDbf.LettuceBounty.GetRecords((LettuceBountyDbfRecord r) => r.Enabled);
		if (bountyRecords == null)
		{
			Log.InGameBrowser.PrintInfo("No bounty records found from GameDbf. Considering message invalid");
			return false;
		}
		foreach (MercenaryMessageBountyItemInformation item in items)
		{
			if (bountyRecords.Find((LettuceBountyDbfRecord r) => r.FinalBossCardId != item.ItemId) == null)
			{
				return false;
			}
		}
		return true;
	}

	private static bool IsMercenaryMessageValid(MercenaryMessageContent content)
	{
		if (IsRequiredStringValid(content.Title, "Title") && IsRequiredStringValid(content.BodyText, "Text Body"))
		{
			return content.Items.Count > 0;
		}
		return false;
	}

	private static bool IsChangeMessageValid(ChangeMessageContent content)
	{
		if (IsRequiredStringValid(content.Title, "Title") && IsRequiredStringValid(content.BodyText, "Text Body"))
		{
			return content.ChangeItems.Count > 0;
		}
		return false;
	}

	private static bool IsTextMessageValid(TextMessageContent content)
	{
		if (IsRequiredStringValid(content.ImageType, "Image Type") && IsRequiredStringValid(content.TextBody, "Text Body"))
		{
			return IsRequiredStringValid(content.TextBody, "Title");
		}
		return false;
	}

	private static bool IsShopMessageValid(ShopMessageContent content)
	{
		if (IsRequiredStringValid(content.TextBody, "Text Body") && IsRequiredStringValid(content.Title, "Title"))
		{
			return IsShopProductValid(content.ProductID);
		}
		return false;
	}

	private static bool IsLaunchMessageValid(LaunchMessageContent content)
	{
		return IsRequiredStringValid(content.Title, "Title");
	}

	private static bool IsRequiredStringValid(string stringField, string fieldName)
	{
		if (string.IsNullOrEmpty(stringField))
		{
			Log.InGameMessage.PrintInfo("Message was missing " + fieldName + " and is invalid");
			return false;
		}
		return true;
	}

	private static bool IsShopProductValid(long pmtId)
	{
		StoreManager storeMgr = StoreManager.Get();
		if (storeMgr == null || !storeMgr.IsOpen())
		{
			Log.InGameMessage.PrintInfo("Shop product is considered invalid as shop is not ready");
			return false;
		}
		if (!ServiceManager.TryGet<IProductDataService>(out var dataService) || !dataService.HasStoreLoaded())
		{
			Log.InGameMessage.PrintInfo("Shop product is considered invalid as products not loaded");
			return false;
		}
		if (!ProductId.IsValid(pmtId))
		{
			Log.InGameMessage.PrintInfo($"Shop product {pmtId} is invalid");
		}
		ProductDataModel product = dataService.GetProductDataModel(pmtId);
		if (product == null)
		{
			Log.InGameMessage.PrintInfo($"Shop product {pmtId} is invalid as shop data was missing");
			return false;
		}
		if (Shop.Get() != null && !Shop.Get().TryGetActiveShopSlot(product, out var _))
		{
			Log.InGameMessage.PrintInfo($"Shop product {pmtId} is not present in store. Considering message invalid");
			return false;
		}
		if (AllowOnlyPurchaseableProduct() && product.Availability != ProductAvailability.CAN_PURCHASE)
		{
			Log.InGameMessage.PrintInfo($"Shop product {pmtId} cannot be purchased. Considering message invalid");
			return false;
		}
		if (product.RewardList == null || product.RewardList.Items == null || product.RewardList.Items.Count < 1)
		{
			Log.InGameBrowser.PrintInfo($"Product {pmtId} has no reward items to display. Considering message invalid");
			return false;
		}
		if (!MercenaryMessageUtils.HasCompletedMercenaryVillageShopTutorial() && product.Items != null)
		{
			foreach (RewardItemDataModel item in product.Items)
			{
				if (MercenaryMessageUtils.IsMercenaryOnlyShopItem(item.ItemType))
				{
					Log.InGameMessage.PrintInfo($"Mercenary only Shop product {pmtId} cannot be viewed. Mercenary shop tutorial not completed");
					return false;
				}
			}
		}
		return true;
	}

	private static bool AllowOnlyPurchaseableProduct()
	{
		return Vars.Key("IGM.RequirePurchasable").GetBool(def: true);
	}
}
