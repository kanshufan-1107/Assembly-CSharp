using System;
using System.Collections.Generic;
using System.Globalization;
using Hearthstone.Commerce;
using Hearthstone.DataModels;
using Hearthstone.Store;
using Hearthstone.UI;
using PegasusUtil;
using UnityEngine;

public static class ProductFactory
{
	public static ProductDataModel CreateEmptyProductDataModel()
	{
		return new ProductDataModel();
	}

	public static ProductDataModel CreateProductDataModel(ProductInfo netBundle)
	{
		if (netBundle == null || !netBundle.Id.IsValid())
		{
			Log.Store.PrintError("A product Network.Bundle has no PMTProductID");
			return null;
		}
		ProductDataModel product = new ProductDataModel
		{
			PmtId = netBundle.Id.Value,
			Name = netBundle.Title,
			Description = netBundle.Description
		};
		CatalogUtils.TagData tagData = netBundle.Attributes.GetTags();
		product.Tags = tagData.Tags.ToDataModelList();
		if (!StoreManager.Get().IsLargeItemBundleDetailsEnabled())
		{
			product.Tags.Remove("large_item_bundle_details");
		}
		if (!netBundle.HasValidPrices())
		{
			ProductIssues.LogError(product, "No prices and no free tag");
			return null;
		}
		if (netBundle.Items == null || netBundle.Items.Count == 0)
		{
			ProductIssues.LogError(product, "No licenses and no VC grants");
			return null;
		}
		if (PlatformSettings.IsMobile() && product.Tags.Contains("hide_on_mobile"))
		{
			ProductIssues.LogHidden(product, "Tagged to be hidden from mobile");
			return null;
		}
		if (!ShopUtils.ShouldShowPriceStyles())
		{
			product.Tags.Remove("promote_price");
			product.Tags.Remove("pricestyle_single");
			product.Tags.Remove("pricestyle_singleicon");
			product.Tags.Remove("pricestyle_double");
			product.Tags.Remove("pricestyle_doubleicon");
			product.Tags.Remove("pricestyle_icon");
		}
		if (netBundle.IsFree())
		{
			PriceDataModel price = new PriceDataModel
			{
				Currency = CurrencyType.GOLD,
				Amount = 0f
			};
			product.Prices.Add(price);
		}
		else
		{
			CurrencyType vcCurrencyType = netBundle.GetFirstVirtualCurrencyPriceType();
			if (!netBundle.HasCurrency(vcCurrencyType))
			{
				ProductIssues.LogError(product, $"VC price with unrecognized currency code {vcCurrencyType}");
				vcCurrencyType = CurrencyType.NONE;
			}
			Queue<CurrencyType> currencyTypes = new Queue<CurrencyType>();
			if (tagData.TryGetData("promote_price", out var promotedPrices))
			{
				string[] array = promotedPrices.Split("-");
				foreach (string price2 in array)
				{
					if (price2.ToLower() == "gold")
					{
						currencyTypes.Enqueue(CurrencyType.GOLD);
					}
					else if (price2.ToLower() == "vc")
					{
						if (vcCurrencyType != 0)
						{
							currencyTypes.Enqueue(vcCurrencyType);
						}
					}
					else if (price2.ToLower() == "rm")
					{
						currencyTypes.Enqueue(CurrencyType.REAL_MONEY);
					}
				}
			}
			if (!currencyTypes.Contains(CurrencyType.REAL_MONEY))
			{
				currencyTypes.Enqueue(CurrencyType.REAL_MONEY);
			}
			if (vcCurrencyType != 0 && !currencyTypes.Contains(vcCurrencyType))
			{
				currencyTypes.Enqueue(vcCurrencyType);
			}
			if (!currencyTypes.Contains(CurrencyType.GOLD))
			{
				currencyTypes.Enqueue(CurrencyType.GOLD);
			}
			while (currencyTypes.Count > 0)
			{
				CurrencyType currencyType = currencyTypes.Dequeue();
				if ((currencyType != CurrencyType.REAL_MONEY || ShouldShowRealMoneyPrice(netBundle)) && netBundle.HasCurrency(currencyType))
				{
					product.Prices.Add(netBundle.GetPriceDataModel(currencyType));
				}
			}
		}
		if (product.Prices.Count == 0)
		{
			ProductIssues.LogError(product, "No valid prices");
			return null;
		}
		ParseProductTagData(product, tagData);
		bool productHasInvalidItems = false;
		List<RewardItemDataModel> items = new List<RewardItemDataModel>();
		product.RequiredGamemodes = new DataModelList<IGamemodeAvailabilityService.Gamemode>();
		foreach (Network.BundleItem netBundleItem in netBundle.Items)
		{
			if (!IsLicenseAvailable(netBundleItem))
			{
				continue;
			}
			if (netBundleItem.ItemType == ProductType.PRODUCT_TYPE_MINI_SET)
			{
				MiniSetDbfRecord record = GameDbf.MiniSet.GetRecord(netBundleItem.ProductData);
				if (record != null && record.HideOnClient)
				{
					if (netBundle.Items.Count == 1)
					{
						productHasInvalidItems = true;
						ProductIssues.LogError(product, $"Hidden Mini-Set Cannot be the only item in a product!! ProductId={product.PmtId}");
						break;
					}
					continue;
				}
			}
			bool isValidItem;
			RewardItemDataModel item = RewardFactory.CreateShopRewardItemDataModel(netBundle, netBundleItem, out isValidItem);
			if (!isValidItem)
			{
				productHasInvalidItems = true;
				ProductIssues.LogError(product, $"Invalid reward Type={netBundleItem.ItemType}, ID={netBundleItem.ProductData}");
			}
			if (item != null)
			{
				IGamemodeAvailabilityService.Gamemode requiredGamemode = RewardUtils.GetGamemodeFromRewardItemType(item.ItemType);
				if (requiredGamemode != 0 && !product.RequiredGamemodes.Contains(requiredGamemode))
				{
					product.RequiredGamemodes.Add(requiredGamemode);
				}
				items.Add(item);
			}
		}
		if (product.Tags.Contains("battlegrounds_season_pass"))
		{
			product.RequiredGamemodes.Add(IGamemodeAvailabilityService.Gamemode.BATTLEGROUNDS);
		}
		if (product.RequiredGamemodes.Count == 0)
		{
			product.RequiredGamemodes.Add(IGamemodeAvailabilityService.Gamemode.NONE);
		}
		if (productHasInvalidItems)
		{
			return null;
		}
		if (items.Count == 0)
		{
			ProductIssues.LogError(product, "No valid reward items");
			return null;
		}
		items.Sort(RewardUtils.CompareItemsForSort);
		product.Items.AddRange(items);
		if (!product.AddAutomaticTagsAndItems(netBundle))
		{
			ProductIssues.LogError(product, "Failed to add automatic tags and reward items");
			return null;
		}
		DateTime? endDate = netBundle.EndTime;
		product.IsScheduled = endDate.HasValue;
		if (endDate.HasValue && BnetBar.Get().TryGetServerTimeUTC(out var serverTime))
		{
			TimeSpan timeUntilEnd = endDate.Value - serverTime;
			product.DaysBeforeEnd = Mathf.Max(0, timeUntilEnd.Days);
			product.HoursBeforeEnd = Mathf.Max(0, timeUntilEnd.Hours);
		}
		product.GenerateRewardList();
		product.SetupProductStrings();
		return product;
	}

	private static void ParseProductTagData(ProductDataModel product, CatalogUtils.TagData tagData)
	{
		string multiplier;
		string isDetailed;
		if (tagData.TryGetData("value_percent", out var percentage))
		{
			product.AdditionalBannerData = percentage;
		}
		else if (tagData.TryGetData("value_multiply", out multiplier))
		{
			product.AdditionalBannerData = multiplier;
		}
		else if (tagData.TryGetData("on_sale", out isDetailed))
		{
			_ = string.Empty;
			PriceDataModel priceDataModel = ((product.Prices.Count == 1) ? product.Prices[0] : null);
			if (priceDataModel != null && priceDataModel.OnSale && isDetailed == "percent" && priceDataModel.Amount > 0f && priceDataModel.OriginalAmount > 0f)
			{
				float percentageSale = 1f - priceDataModel.Amount / priceDataModel.OriginalAmount;
				if (percentageSale > 0f && percentageSale < 1f)
				{
					percentageSale *= 100f;
					if (percentageSale % 5f == 0f)
					{
						product.Tags.Add("sale_percentage_whole");
						product.AdditionalBannerData = percentageSale.ToString("0");
					}
					else
					{
						product.Tags.Add("sale_percentage_average");
						product.AdditionalBannerData = (Mathf.FloorToInt(percentageSale / 5f) * 5).ToString();
					}
				}
			}
		}
		if (tagData.TryGetData("shop_swipe", out var animDelay))
		{
			product.ShopSwipeAnimDelay = animDelay;
		}
	}

	private static bool IsLicenseAvailable(Network.BundleItem bundleItem)
	{
		if (!bundleItem.HasShopAvailableDate || string.IsNullOrEmpty(bundleItem.ShopAvailableDate))
		{
			return true;
		}
		string format = "MM/dd/yyyy hh:mm:ss tt zzz";
		if (DateTime.TryParseExact(bundleItem.ShopAvailableDate, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out var licenseAvailableTime) && BnetBar.Get() != null && BnetBar.Get().TryGetServerTimeUTC(out var serverTime))
		{
			return serverTime > licenseAvailableTime.ToUniversalTime();
		}
		return EventTimingManager.Get().IsEventActive(bundleItem.ShopAvailableDate);
	}

	public static ProductTierDataModel CreateEmptyProductTier()
	{
		return new ProductTierDataModel();
	}

	private static bool ShouldShowRealMoneyPrice(ProductInfo netBundle)
	{
		if (netBundle.HasValidPrices() && (!PlatformSettings.IsMobile() || netBundle.DisableRealMoneyShopFlags != (MobileShopType)23))
		{
			return true;
		}
		return false;
	}
}
