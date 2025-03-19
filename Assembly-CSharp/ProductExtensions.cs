using System;
using System.Collections.Generic;
using System.Linq;
using Blizzard.Commerce;
using Blizzard.T5.Services;
using Hearthstone.Commerce;
using Hearthstone.DataModels;
using Hearthstone.Store;
using Hearthstone.UI;
using PegasusUtil;
using UnityEngine;

public static class ProductExtensions
{
	private static Dictionary<RewardItemType, string> s_rewardTypeTags = Enum.GetValues(typeof(RewardItemType)).Cast<RewardItemType>().ToDictionary((RewardItemType v) => v, (RewardItemType v) => v.ToString().ToLowerInvariant());

	public static ProductId GetProductId(this ProductDataModel product)
	{
		return ProductId.CreateFrom(product.PmtId);
	}

	public static bool IsEmpty(this ProductDataModel productDataModel)
	{
		if (productDataModel != null)
		{
			if (productDataModel.PmtId != 0L)
			{
				return false;
			}
			DataModelList<RewardItemDataModel> items = productDataModel.Items;
			if (items != null && items.Count > 0)
			{
				return false;
			}
			if (productDataModel.RewardList != null)
			{
				return false;
			}
		}
		return true;
	}

	public static int GetMaxBulkPurchaseCount(this ProductDataModel product)
	{
		if (!product.ProductSupportsQuantitySelect())
		{
			return 1;
		}
		if (product.Tags.Contains("cn_arcane_orbs"))
		{
			if (!ServiceManager.TryGet<CurrencyManager>(out var currencyManager))
			{
				return 0;
			}
			long balance = currencyManager.GetBalance(CurrencyType.CN_ARCANE_ORBS);
			if (balance >= 9999)
			{
				return 0;
			}
			float orbsPerUnit = ShopUtils.GetAmountOfCurrencyInProduct(product, CurrencyType.CN_ARCANE_ORBS);
			return Mathf.Min(900, Mathf.FloorToInt((float)(9999 - balance) / orbsPerUnit));
		}
		return 50;
	}

	public static bool ProductSupportsQuantitySelect(this ProductDataModel product)
	{
		if (product.Prices.Count == 1)
		{
			if (!product.Tags.Contains("cn_arcane_orbs"))
			{
				return product.GetBuyProductArgs(product.Prices.First(), 1) is BuyNoGTAPPEventArgs;
			}
			return true;
		}
		return false;
	}

	public static BuyProductEventArgs GetBuyProductArgs(this ProductDataModel product, PriceDataModel price, int quantity)
	{
		if (product.PmtId == 0L && price.Currency == CurrencyType.GOLD)
		{
			if (product.Items.Count != 1)
			{
				Log.Store.PrintError($"Cannot buy product for gold where item count != 1. Name = {product.Name}, Item Count = {product.Items.Count}");
				return null;
			}
			RewardItemDataModel item = product.Items.First();
			ProductType netProductType = RewardItemTypeToNetProductType(item.ItemType);
			if (netProductType == ProductType.PRODUCT_TYPE_UNKNOWN)
			{
				Log.Store.PrintError($"Cannot buy gold product with unsupported item type {item.ItemType}. Name = {product.Name}");
				return null;
			}
			return new BuyNoGTAPPEventArgs(new NoGTAPPTransactionData
			{
				Product = netProductType,
				ProductData = item.ItemId,
				Quantity = quantity
			});
		}
		if (!ProductId.IsValid(product.PmtId))
		{
			Log.Store.PrintError($"Product data model has invalid product ID. Name = {product.Name}, PmtId = {product.PmtId}");
			return null;
		}
		if (!ServiceManager.TryGet<IProductDataService>(out var dataService) || !dataService.TryGetProduct(product.PmtId, out var netBundle))
		{
			Log.Store.PrintError($"Cannot buy product with no matching ProductInfo PMT ID = {product.PmtId}, Name = {product.Name}");
			return null;
		}
		return new BuyPmtProductEventArgs(netBundle.Id.Value, price.Currency, quantity);
	}

	public static void GenerateRewardList(this ProductDataModel product)
	{
		if (product.HasMultiHeroSkins())
		{
			product.RewardList = PremiumHeroSkinUtil.GenerateMultiHeroSkinRewardList(product);
			return;
		}
		product.RewardList = new RewardListDataModel();
		product.RewardList.Items.AddRange(product.Items);
	}

	public static void SetupProductStrings(this ProductDataModel product)
	{
		product.DescriptionHeader = null;
		product.VariantName = null;
		switch (product.GetPrimaryProductTag())
		{
		case "booster":
			product.SetupBoosterProductStrings();
			break;
		case "mercenary_booster":
			product.SetupMercenaryBoosterProductStrings();
			break;
		case "cn_runestones":
		case "row_runestones":
			product.SetupRunestoneProductString();
			break;
		case "bundle":
			if (product.IsSingleItemProduct() && (product.Items[0].ItemType == RewardItemType.CN_RUNESTONES || product.Items[0].ItemType == RewardItemType.ROW_RUNESTONES))
			{
				product.SetupRunestoneProductString();
			}
			break;
		case "mini_set":
			product.SetupMiniSetProductStrings();
			break;
		case "sellable_deck":
		case "sellable_deck_bundle":
			product.SetupSellableDeckProductStrings();
			break;
		}
		product.DescriptionHeader = product.DescriptionHeader ?? GameStrings.Format("GLUE_SHOP_DESCRIPTION_HEADER", product.Name);
		product.VariantName = product.VariantName ?? product.Name;
		if (string.IsNullOrWhiteSpace(product.Description) || !product.Description.Contains("~~~"))
		{
			if (!product.Tags.Contains("hide_full_description"))
			{
				product.Tags.Add("hide_full_description");
			}
		}
		else
		{
			string[] dividedDescription = product.Description.Split("~~~");
			string shortDescription;
			string fullDescription;
			if (dividedDescription.Length < 2 || string.IsNullOrWhiteSpace(dividedDescription[1]))
			{
				Log.Store.PrintWarning(string.Format("Product {0} had invalid description setup with delimiter ({1}) - hiding full description...", product.PmtId, "~~~"));
				if (!product.Tags.Contains("hide_full_description"))
				{
					product.Tags.Add("hide_full_description");
				}
				shortDescription = product.Description.Replace("~~~", string.Empty);
				fullDescription = string.Empty;
			}
			else
			{
				shortDescription = dividedDescription[0];
				fullDescription = dividedDescription[1].Replace("~~~", string.Empty);
			}
			product.Description = shortDescription;
			product.FullDescription = fullDescription;
		}
		string disclaimer = product.GetProductLegalDisclaimer();
		if (string.IsNullOrEmpty(disclaimer))
		{
			return;
		}
		if (string.IsNullOrWhiteSpace(product.Description))
		{
			product.Description = disclaimer;
		}
		else if (!product.Description.Contains(disclaimer))
		{
			product.Description = product.Description + "\n" + disclaimer;
		}
		if (StoreManager.Get().IsKoreanCustomer())
		{
			if (string.IsNullOrWhiteSpace(product.FullDescription))
			{
				product.FullDescription = disclaimer;
			}
			else if (!product.FullDescription.Contains(disclaimer))
			{
				product.FullDescription = product.FullDescription + "\n" + disclaimer;
			}
		}
	}

	public static void SetDescriptionOverride(this ProductDataModel product)
	{
		if (!string.IsNullOrEmpty(product.Description))
		{
			product.Tags.Add("has_pmtdescription");
		}
	}

	public static BoosterDbId GetProductBoosterId(this ProductDataModel product)
	{
		if (!product.Tags.Contains("booster") && !product.Tags.Contains("mercenary_booster"))
		{
			return BoosterDbId.INVALID;
		}
		BoosterDbId productBoosterType = BoosterDbId.INVALID;
		foreach (RewardItemDataModel itemData in product.Items)
		{
			if (itemData.ItemType != RewardItemType.DUST)
			{
				if (itemData.Booster == null)
				{
					return BoosterDbId.INVALID;
				}
				if (productBoosterType != 0 && itemData.Booster.Type != productBoosterType)
				{
					return BoosterDbId.INVALID;
				}
				productBoosterType = itemData.Booster.Type;
			}
		}
		return productBoosterType;
	}

	public static AdventureDbId GetProductAdventureId(this ProductDataModel product)
	{
		RewardItemDataModel firstWing = product.Items.FirstOrDefault((RewardItemDataModel i) => i.ItemType == RewardItemType.ADVENTURE_WING);
		if (firstWing != null)
		{
			return GameUtils.GetAdventureIdByWingId(firstWing.ItemId);
		}
		return AdventureDbId.INVALID;
	}

	public static string GetPrimaryProductTag(this ProductDataModel product)
	{
		DataModelList<string> tags = product.Tags;
		int i = 0;
		for (int iMax = tags.Count; i < iMax; i++)
		{
			string tag = tags[i];
			if (CatalogUtils.IsPrimaryProductTag(tag))
			{
				return tag;
			}
		}
		return null;
	}

	public static ShopBrowserButtonDataModel ToButton(this ProductDataModel product, bool isFiller = false)
	{
		return new ShopBrowserButtonDataModel
		{
			DisplayProduct = product,
			DisplayText = product.Name,
			IsFiller = isFiller,
			Hovered = false
		};
	}

	public static void SetProductTagPresence(this ProductDataModel product, string tag, bool shouldHave)
	{
		bool hasTag = product.Tags.Contains(tag);
		if (!hasTag && shouldHave)
		{
			product.Tags.Add(tag);
		}
		else if (hasTag && !shouldHave)
		{
			product.Tags.Remove(tag);
		}
	}

	public static bool AddAutomaticTagsAndItems(this ProductDataModel product, ProductInfo netBundle)
	{
		if (product.Tags.Contains("collapse_wings"))
		{
			List<RewardItemDataModel> items = product.Items.ToList();
			while (true)
			{
				RewardItemDataModel firstWingItem = items.FirstOrDefault((RewardItemDataModel i) => i.ItemType == RewardItemType.ADVENTURE_WING);
				if (firstWingItem == null)
				{
					break;
				}
				AdventureDbId adventureId = GameUtils.GetAdventureIdByWingId(firstWingItem.ItemId);
				items.RemoveAll((RewardItemDataModel i) => i.ItemType == RewardItemType.ADVENTURE_WING && GameUtils.GetAdventureIdByWingId(i.ItemId) == adventureId);
				if (adventureId != 0)
				{
					items.Add(new RewardItemDataModel
					{
						ItemType = RewardItemType.ADVENTURE,
						ItemId = (int)adventureId,
						Quantity = 1
					});
				}
			}
			items.Sort(RewardUtils.CompareItemsForSort);
			product.Items.Clear();
			product.Items.AddRange(items);
		}
		else if (product.Items.Count > 1)
		{
			AdventureDbId adventureId2 = product.GetProductAdventureId();
			if (adventureId2 != 0 && product.Items.All((RewardItemDataModel item) => item.ItemType == RewardItemType.ADVENTURE_WING && GameUtils.GetAdventureIdByWingId(item.ItemId) == adventureId2))
			{
				product.Items.Insert(0, new RewardItemDataModel
				{
					ItemType = RewardItemType.ADVENTURE,
					ItemId = (int)adventureId2,
					Quantity = 1
				});
			}
		}
		string primaryTag = product.GetPrimaryProductTag();
		if (primaryTag == null)
		{
			primaryTag = product.DetermineProductPrimaryTagFromItems(netBundle);
			if (primaryTag == null)
			{
				ProductIssues.LogError(product, "Could not determine a primary tag");
				return false;
			}
			product.Tags.Add(primaryTag);
		}
		if (product.IsSingleItemProduct())
		{
			RewardItemType itemType = product.Items[0].ItemType;
			if (ShopUtils.IsVirtualCurrencyRewardItemType(itemType))
			{
				product.Tags.Add("vc");
				if (primaryTag == "bundle" && s_rewardTypeTags.TryGetValue(itemType, out var rewardTypeTag) && !product.Tags.Contains(rewardTypeTag))
				{
					product.Tags.Add(rewardTypeTag);
				}
			}
		}
		if (netBundle.IsPrePurchase && !product.Tags.Contains("prepurchase"))
		{
			product.Tags.Add("prepurchase");
		}
		return true;
	}

	public static bool IsSingleItemProduct(this ProductDataModel product)
	{
		if (product.Items.Count == 1 && !product.Tags.Contains("bundle"))
		{
			return true;
		}
		if (product.Items.Count == 1 && product.Tags.Contains("bundle") && ShopUtils.IsVirtualCurrencyRewardItemType(product.Items[0].ItemType))
		{
			return true;
		}
		return false;
	}

	public static bool MatchesItemType(this ProductDataModel product, RewardItemType itemType)
	{
		return product.Items.First().ItemType == itemType;
	}

	public static bool MatchesItemId(this ProductDataModel product, int itemId)
	{
		return product.Items.First().ItemId == itemId;
	}

	public static int CountPacks(this ProductDataModel product)
	{
		int returnVal = 0;
		DataModelList<RewardItemDataModel> productItems = product.Items;
		int i = 0;
		for (int iMax = productItems.Count; i < iMax; i++)
		{
			RewardItemDataModel rewardItemDataModel = productItems[i];
			if (rewardItemDataModel.ItemType == RewardItemType.BOOSTER || rewardItemDataModel.ItemType == RewardItemType.MERCENARY_BOOSTER)
			{
				returnVal += rewardItemDataModel.Quantity;
			}
		}
		return returnVal;
	}

	public static CurrencyListDataModel GetAllCurrencies(this ProductDataModel product, bool includeVariants = false)
	{
		CurrencyListDataModel currencyListDataModel = new CurrencyListDataModel();
		if (product != null)
		{
			foreach (PriceDataModel price in product.Prices)
			{
				switch (price.Currency)
				{
				case CurrencyType.REAL_MONEY:
					currencyListDataModel.ISOCode = ShopUtils.GetCurrencyCode(CurrencyType.REAL_MONEY);
					break;
				default:
					currencyListDataModel.VCCurrency.Add(price.Currency);
					break;
				case CurrencyType.NONE:
					break;
				}
			}
			if (includeVariants)
			{
				foreach (ProductDataModel variant in product.Variants)
				{
					currencyListDataModel.Append(variant);
				}
			}
		}
		return currencyListDataModel;
	}

	public static void Append(this CurrencyListDataModel currencyList, ProductDataModel targetProduct, bool includeVariants = false)
	{
		if (targetProduct == null)
		{
			return;
		}
		foreach (PriceDataModel price in targetProduct.Prices)
		{
			switch (price.Currency)
			{
			case CurrencyType.REAL_MONEY:
				if (string.IsNullOrEmpty(currencyList.ISOCode))
				{
					currencyList.ISOCode = ShopUtils.GetCurrencyCode(CurrencyType.REAL_MONEY);
				}
				break;
			default:
				if (!currencyList.VCCurrency.Contains(price.Currency))
				{
					currencyList.VCCurrency.Add(price.Currency);
				}
				break;
			case CurrencyType.NONE:
				break;
			}
		}
		if (!includeVariants)
		{
			return;
		}
		foreach (ProductDataModel variant in targetProduct.Variants)
		{
			currencyList.Append(variant);
		}
	}

	public static bool HasMultiHeroSkins(this ProductDataModel product)
	{
		int count = 0;
		foreach (RewardItemDataModel item in product.Items)
		{
			if (item.ItemType == RewardItemType.HERO_SKIN)
			{
				count++;
			}
		}
		return count > 1;
	}

	private static string DetermineProductPrimaryTagFromItems(this ProductDataModel product, ProductInfo netBundle)
	{
		if (product.IsEmptyProduct())
		{
			return "bundle";
		}
		if (product.IsAdventureProduct())
		{
			return "adventure";
		}
		if (product.ContainsSellableDeck())
		{
			return "sellable_deck";
		}
		if (product.ContainsSellableDeckBundle())
		{
			return "sellable_deck_bundle";
		}
		if (netBundle != null && netBundle.ContainsHiddenLicense())
		{
			return "bundle";
		}
		if (product.IsSingleItemProduct())
		{
			RewardItemDataModel item = product.Items[0];
			if (item.ItemType == RewardItemType.UNDEFINED)
			{
				ProductIssues.LogError(product, "Single-item product has reward of undefined type");
				return null;
			}
			if (!Enum.IsDefined(typeof(RewardItemType), item.ItemType))
			{
				ProductIssues.LogError(product, $"Single-item product has reward of unsupported type {item.ItemType}");
				return null;
			}
			s_rewardTypeTags.TryGetValue(item.ItemType, out var rewardTag);
			return rewardTag;
		}
		return "bundle";
	}

	private static void SetupBoosterProductStrings(this ProductDataModel product)
	{
		BoosterDbId boosterId = product.GetProductBoosterId();
		if (boosterId != 0)
		{
			BoosterDbfRecord boosterRecord = GameDbf.Booster.GetRecord((int)boosterId);
			if (boosterRecord != null)
			{
				string boosterName = boosterRecord.Name;
				string boosterShortName = boosterRecord.ShortName;
				if (!product.Tags.Contains("has_description") && !product.Tags.Contains("has_pmtdescription"))
				{
					if (boosterId == BoosterDbId.MERCENARIES)
					{
						product.Description = GameStrings.Format("GLUE_STORE_PRODUCT_DETAILS_MERCENARY_PACK");
					}
					else
					{
						product.Description = GameStrings.Format("GLUE_STORE_PRODUCT_DETAILS_PACK", boosterName);
					}
					if (GameUtils.IsBoosterWild(boosterRecord))
					{
						product.Description = product.Description + "\n" + GameStrings.Get("GLUE_SHOP_WILD_CARDS_DISCLAIMER");
					}
				}
				if (!product.Tags.Contains("has_description"))
				{
					product.Name = ((!string.IsNullOrEmpty(boosterName)) ? boosterName : boosterShortName);
					product.ShortName = ((!string.IsNullOrEmpty(boosterShortName)) ? boosterShortName : boosterName);
					product.DescriptionHeader = GameStrings.Get("GLUE_STORE_PRODUCT_DETAILS_HEADLINE_PACK");
				}
			}
		}
		product.VariantName = GameStrings.Format("GLUE_SHOP_BOOSTER_SKU_BUTTON", product.CountPacks());
	}

	private static void SetupMercenaryBoosterProductStrings(this ProductDataModel product)
	{
		BoosterDbId boosterId = product.GetProductBoosterId();
		if (boosterId != 0)
		{
			string boosterName = GameDbf.Booster.GetRecord((int)boosterId)?.Name;
			product.Name = boosterName;
			product.DescriptionHeader = GameStrings.Get("GLUE_STORE_PRODUCT_DETAILS_HEADLINE_PACK");
			product.Description = GameStrings.Get("GLUE_STORE_PRODUCT_DETAILS_MERCENARY_PACK");
		}
		product.VariantName = GameStrings.Format("GLUE_SHOP_BOOSTER_SKU_BUTTON", product.CountPacks());
	}

	private static void SetupRunestoneProductString(this ProductDataModel product)
	{
		RewardItemDataModel runestoneItem = product.Items.FirstOrDefault((RewardItemDataModel item) => item.ItemType == RewardItemType.CN_RUNESTONES || item.ItemType == RewardItemType.ROW_RUNESTONES);
		if (runestoneItem != null)
		{
			product.VariantName = GameStrings.Format("GLUE_SHOP_RUNESTONE_SKU_BUTTON", runestoneItem.Quantity);
		}
		product.DescriptionHeader = GameStrings.Get("GLUE_SHOP_RUNESTONES_DETAILS_HEADER");
	}

	private static void SetupMiniSetProductStrings(this ProductDataModel product)
	{
		int itemCount = 0;
		foreach (RewardItemDataModel item in product.Items)
		{
			if (item.ItemType == RewardItemType.CARD)
			{
				itemCount++;
			}
			else if (item.ItemType == RewardItemType.MINI_SET)
			{
				MiniSetDbfRecord miniSet = GameDbf.MiniSet.GetRecord(item.ItemId);
				itemCount += miniSet.DeckRecord.Cards.Count;
			}
		}
		product.ShortName = GameStrings.Get("GLUE_STORE_MINI_SET_LABEL");
		product.FlavorText = GameStrings.FormatPlurals("GLUE_STORE_MINI_SET_CARD_COUNT", GameStrings.MakePlurals(itemCount), itemCount);
	}

	private static void SetupSellableDeckProductStrings(this ProductDataModel product)
	{
		RewardItemDataModel sellableDeckItem = product.Items.FirstOrDefault((RewardItemDataModel item) => item.ItemType == RewardItemType.SELLABLE_DECK);
		if (sellableDeckItem != null)
		{
			DeckTemplateDbfRecord deckTemplate = GameDbf.SellableDeck.GetRecord(sellableDeckItem.ItemId)?.DeckTemplateRecord;
			if (deckTemplate != null)
			{
				product.FlavorText = string.Format(GameStrings.Get("GLUE_STORE_SELLABLEDECKS_FLAVOR"), GameStrings.GetClassName((TAG_CLASS)deckTemplate.ClassId));
			}
		}
	}

	private static string GetProductLegalDisclaimer(this ProductDataModel product)
	{
		if (StoreManager.Get().IsKoreanCustomer())
		{
			if (product.Tags.Contains("non_refundable"))
			{
				return GetGenericKoreanAgreementString();
			}
			if (product.Tags.Contains("non_refundable_pack"))
			{
				return GameStrings.Get("GLUE_STORE_SUMMARY_KOREAN_AGREEMENT_EXPERT_PACK");
			}
			if (product.Tags.Contains("prepurchase"))
			{
				return GameStrings.Get("GLUE_STORE_SUMMARY_KOREAN_AGREEMENT_PACK_PREORDER");
			}
			if (product.ContainsRunestones())
			{
				return GameStrings.Get("GLUE_STORE_SUMMARY_KOREAN_AGREEMENT_RUNESTONES");
			}
			if (product.ContainsAdventureChapter())
			{
				if (product.Items.Count == 1)
				{
					return GameStrings.Get("GLUE_STORE_SUMMARY_KOREAN_AGREEMENT_ADVENTURE_SINGLE");
				}
				return GameStrings.Get("GLUE_STORE_SUMMARY_KOREAN_AGREEMENT_ADVENTURE_BUNDLE");
			}
			if (product.ContainsAnyBoosterPack())
			{
				if (product.IsWelcomeBundle())
				{
					return GameStrings.Get("GLUE_STORE_SUMMARY_KOREAN_AGREEMENT_FIRST_PURCHASE_BUNDLE");
				}
				return GameStrings.Get("GLUE_STORE_SUMMARY_KOREAN_AGREEMENT_EXPERT_PACK");
			}
			if (product.ContainsBattlegroundsPerk())
			{
				return GameStrings.Get("GLUE_STORE_SUMMARY_KOREAN_AGREEMENT_BATTLEGROUNDS_PERKS");
			}
			if (product.ContainsProgressionBonus())
			{
				return GameStrings.Get("GLUE_STORE_SUMMARY_KOREAN_AGREEMENT_PROGRESSION_BONUS");
			}
			if (product.ContainsArenaTicket())
			{
				return GameStrings.Get("GLUE_STORE_SUMMARY_KOREAN_AGREEMENT_FORGE_TICKET");
			}
		}
		else if (StoreManager.Get().IsChineseCustomer() && product.Tags.Contains("cn_disclaimer_one_dust"))
		{
			return GameStrings.Get("GLUE_STORE_CN_DISCLAIMER_ONE_DUST");
		}
		return null;
	}

	private static bool IsEmptyProduct(this ProductDataModel product)
	{
		return product.Items.Count == 0;
	}

	private static bool IsAdventureProduct(this ProductDataModel product)
	{
		if (product.IsEmptyProduct())
		{
			return false;
		}
		DataModelList<RewardItemDataModel> productItems = product.Items;
		if (productItems[0].ItemType != RewardItemType.ADVENTURE)
		{
			return false;
		}
		int i = 1;
		for (int iMax = productItems.Count; i < iMax; i++)
		{
			RewardItemType rewardItemDataModelType = productItems[i].ItemType;
			if (rewardItemDataModelType != RewardItemType.ADVENTURE && rewardItemDataModelType != RewardItemType.ADVENTURE_WING)
			{
				return false;
			}
		}
		return true;
	}

	private static bool ContainsRunestones(this ProductDataModel product)
	{
		if (!ProductContainsItemType(product, RewardItemType.CN_RUNESTONES))
		{
			return ProductContainsItemType(product, RewardItemType.ROW_RUNESTONES);
		}
		return true;
	}

	private static bool ContainsAdventureChapter(this ProductDataModel product)
	{
		return ProductContainsItemType(product, RewardItemType.ADVENTURE_WING);
	}

	private static bool ContainsAnyBoosterPack(this ProductDataModel product)
	{
		if (!product.Tags.Contains("booster"))
		{
			return product.Tags.Contains("mercenary_booster");
		}
		return true;
	}

	private static bool IsWelcomeBundle(this ProductDataModel product)
	{
		return product.Tags.Contains("welcome_bundle");
	}

	private static bool ContainsBattlegroundsPerk(this ProductDataModel product)
	{
		return ProductContainsItemType(product, RewardItemType.BATTLEGROUNDS_BONUS);
	}

	private static bool ContainsProgressionBonus(this ProductDataModel product)
	{
		return ProductContainsItemType(product, RewardItemType.PROGRESSION_BONUS);
	}

	private static bool ContainsArenaTicket(this ProductDataModel product)
	{
		return ProductContainsItemType(product, RewardItemType.ARENA_TICKET);
	}

	private static bool ContainsHiddenLicense(this ProductInfo bundle)
	{
		List<Network.BundleItem> bundleItems = bundle.Items;
		int i = 0;
		for (int iMax = bundleItems.Count; i < iMax; i++)
		{
			if (bundleItems[i].ItemType == ProductType.PRODUCT_TYPE_HIDDEN_LICENSE)
			{
				return true;
			}
		}
		return false;
	}

	private static bool ContainsSellableDeck(this ProductDataModel product)
	{
		return ProductContainsItemType(product, RewardItemType.SELLABLE_DECK);
	}

	private static bool ContainsSellableDeckBundle(this ProductDataModel product)
	{
		return ProductContainsItemType(product, RewardItemType.SELLABLE_DECK_BUNDLE);
	}

	private static bool ProductContainsItemType(ProductDataModel product, RewardItemType rewardItemType)
	{
		DataModelList<RewardItemDataModel> productItems = product.Items;
		int i = 0;
		for (int iMax = productItems.Count; i < iMax; i++)
		{
			if (productItems[i].ItemType == rewardItemType)
			{
				return true;
			}
		}
		return false;
	}

	private static string GetGenericKoreanAgreementString()
	{
		return GameStrings.Get("GLUE_STORE_SUMMARY_KOREAN_AGREEMENT_HERO");
	}

	private static ProductType RewardItemTypeToNetProductType(RewardItemType itemType)
	{
		return itemType switch
		{
			RewardItemType.BOOSTER => ProductType.PRODUCT_TYPE_BOOSTER, 
			RewardItemType.DUST => ProductType.PRODUCT_TYPE_CURRENCY, 
			RewardItemType.HERO_SKIN => ProductType.PRODUCT_TYPE_HERO, 
			RewardItemType.BATTLEGROUNDS_HERO_SKIN => ProductType.PRODUCT_TYPE_HERO, 
			RewardItemType.BATTLEGROUNDS_GUIDE_SKIN => ProductType.PRODUCT_TYPE_HERO, 
			RewardItemType.CARD_BACK => ProductType.PRODUCT_TYPE_CARD_BACK, 
			RewardItemType.ADVENTURE_WING => ProductType.PRODUCT_TYPE_WING, 
			RewardItemType.ARENA_TICKET => ProductType.PRODUCT_TYPE_DRAFT, 
			RewardItemType.RANDOM_CARD => ProductType.PRODUCT_TYPE_RANDOM_CARD, 
			RewardItemType.BATTLEGROUNDS_BONUS => ProductType.PRODUCT_TYPE_BATTLEGROUNDS_BONUS, 
			RewardItemType.TAVERN_BRAWL_TICKET => ProductType.PRODUCT_TYPE_TAVERN_BRAWL_TICKET, 
			RewardItemType.PROGRESSION_BONUS => ProductType.PRODUCT_TYPE_PROGRESSION_BONUS, 
			RewardItemType.MINI_SET => ProductType.PRODUCT_TYPE_MINI_SET, 
			RewardItemType.SELLABLE_DECK => ProductType.PRODUCT_TYPE_SELLABLE_DECK, 
			RewardItemType.SELLABLE_DECK_BUNDLE => ProductType.PRODUCT_TYPE_SELLABLE_DECK, 
			RewardItemType.MERCENARY_BOOSTER => ProductType.PRODUCT_TYPE_MERCENARIES_BOOSTER, 
			RewardItemType.LUCKY_DRAW => ProductType.PRODUCT_TYPE_LUCKY_DRAW, 
			_ => ProductType.PRODUCT_TYPE_UNKNOWN, 
		};
	}
}
