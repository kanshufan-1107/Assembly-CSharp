using System;
using System.Collections.Generic;
using Blizzard.T5.Services;
using Hearthstone.DataModels;
using Hearthstone.Store;

public static class CatalogUtils
{
	public struct TagData
	{
		public HashSet<string> Tags;

		public Dictionary<string, string> ExtraData;

		public bool Contains(string tag)
		{
			if (Tags != null)
			{
				return Tags.Contains(tag);
			}
			return false;
		}

		public bool TryGetData(string tag, out string data)
		{
			data = null;
			if (ExtraData != null && Contains(tag))
			{
				return ExtraData.TryGetValue(tag, out data);
			}
			return false;
		}
	}

	public static TagData ParseTagsString(string tagsString)
	{
		TagData tagData = default(TagData);
		tagData.Tags = new HashSet<string>();
		tagData.ExtraData = new Dictionary<string, string>();
		TagData tagData2 = tagData;
		if (!string.IsNullOrEmpty(tagsString))
		{
			string[] array = tagsString.Split(',');
			for (int i = 0; i < array.Length; i++)
			{
				string[] tagSplit = array[i].Split('+');
				if (tagSplit.Length != 0)
				{
					string tag = tagSplit[0].Trim().ToLowerInvariant();
					if (tagData2.Tags.Add(tag) && tagSplit.Length > 1)
					{
						tagData2.ExtraData[tag] = tagSplit[1];
					}
				}
			}
		}
		return tagData2;
	}

	public static bool CanUpdateProductStatus(out string reason)
	{
		if (!ServiceManager.TryGet<IProductDataService>(out var dataService) || !dataService.HasLoadedTiers())
		{
			reason = "Cannot update product status before populating sections";
			return false;
		}
		if (EventTimingManager.Get() == null || !EventTimingManager.Get().HasReceivedEventTimingsFromServer)
		{
			reason = "Cannot update product status before HasReceivedEventTimingsFromServer";
			return false;
		}
		if (NetCache.Get().GetNetObject<NetCache.NetCacheCardBacks>() == null)
		{
			reason = "Cannot update product status before NetCacheCardBacks received";
			return false;
		}
		if (NetCache.Get().GetNetObject<NetCache.NetCacheCollection>() == null)
		{
			reason = "Cannot update product status before NetCacheCollection received";
			return false;
		}
		if (CollectionManager.Get() == null)
		{
			reason = "Cannot update product status before CollectionManager initialized";
			return false;
		}
		if (FixedRewardsMgr.Get() == null || !FixedRewardsMgr.Get().IsStartupFinished())
		{
			reason = "Cannot update product status before FixedRewardsMgr initialized";
			return false;
		}
		AccountLicenseMgr.LicenseUpdateState fixedLicensesState = ((AccountLicenseMgr.Get() != null) ? AccountLicenseMgr.Get().FixedLicensesState : AccountLicenseMgr.LicenseUpdateState.UNKNOWN);
		if (fixedLicensesState != AccountLicenseMgr.LicenseUpdateState.SUCCESS)
		{
			reason = $"Cannot update product status when AccountLicenseMgr FixedLicensesState is {fixedLicensesState}.";
			return false;
		}
		reason = null;
		return true;
	}

	public static ProductDataModel NetGoldCostBoosterToProduct(Network.GoldCostBooster goldCostBooster)
	{
		if (!goldCostBooster.Cost.HasValue)
		{
			Log.Store.PrintError("GoldCostBooster has no cost value. Booster ID = {0}", goldCostBooster.ID);
			return null;
		}
		if (goldCostBooster.Cost.Value < 0)
		{
			Log.Store.PrintError("GoldCostBooster has invalid cost value {0}. Booster ID = {1}", goldCostBooster.Cost.Value, goldCostBooster.ID);
			return null;
		}
		BoosterDbfRecord boosterRecord = GameDbf.Booster.GetRecord(goldCostBooster.ID);
		if (boosterRecord == null)
		{
			Log.Store.PrintError("GoldCostBooster has unknown booster ID {0}", goldCostBooster.ID);
			return null;
		}
		ProductDataModel product = new ProductDataModel
		{
			Name = boosterRecord.Name,
			Availability = ProductAvailability.CAN_PURCHASE,
			Description = boosterRecord.DescriptionOverride
		};
		product.Prices.Add(new PriceDataModel
		{
			Currency = CurrencyType.GOLD,
			Amount = goldCostBooster.Cost.Value,
			DisplayText = goldCostBooster.Cost.Value.ToString()
		});
		RewardItemType boosterType = ((goldCostBooster.ID != 629) ? RewardItemType.BOOSTER : RewardItemType.MERCENARY_BOOSTER);
		product.Items.Add(new RewardItemDataModel
		{
			ItemType = boosterType,
			ItemId = goldCostBooster.ID,
			Quantity = 1,
			Booster = new PackDataModel
			{
				Type = (BoosterDbId)goldCostBooster.ID,
				Quantity = 1
			}
		});
		product.Tags.Add("booster");
		product.RewardList = new RewardListDataModel();
		product.RewardList.Items.AddRange(product.Items);
		product.SetDescriptionOverride();
		product.SetupProductStrings();
		return product;
	}

	public static BoosterDbId GetAlternateBoosterId(BoosterDbId id)
	{
		if (id == BoosterDbId.INVALID)
		{
			return BoosterDbId.INVALID;
		}
		if (Enum.IsDefined(typeof(BoosterDbId), id))
		{
			return id;
		}
		BoosterDbfRecord currentRecord = GameDbf.Booster.GetRecord((int)id);
		if (currentRecord == null)
		{
			return BoosterDbId.INVALID;
		}
		foreach (BoosterDbId boosterId in Enum.GetValues(typeof(BoosterDbId)))
		{
			BoosterDbfRecord altRecord = GameDbf.Booster.GetRecord((int)boosterId);
			if (altRecord != null && altRecord.CardSetId == currentRecord.CardSetId)
			{
				return boosterId;
			}
		}
		return BoosterDbId.INVALID;
	}

	public static bool IsPrimaryProductTag(string tag)
	{
		if (!(tag == "bundle") && !(tag == "adventure"))
		{
			RewardItemType result;
			if (tag != "undefined")
			{
				return Enum.TryParse<RewardItemType>(tag, ignoreCase: true, out result);
			}
			return false;
		}
		return true;
	}
}
