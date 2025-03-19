using System;
using System.Linq;
using Blizzard.Commerce;
using Blizzard.GameService.SDK.Client.Integration;
using Blizzard.T5.AssetManager;
using Blizzard.T5.Services;
using Hearthstone.DataModels;
using Hearthstone.Store;
using PegasusShared;
using UnityEngine;

public static class ShopUtils
{
	private static Dbf<BoosterDbfRecord> s_boosters;

	private static Dbf<AdventureDbfRecord> s_adventures;

	public static Dbf<BoosterDbfRecord> Boosters
	{
		get
		{
			if (GameDbf.Booster != null && Application.isPlaying)
			{
				return GameDbf.Booster;
			}
			return s_boosters ?? (s_boosters = Dbf<BoosterDbfRecord>.Load("BOOSTER", DbfFormat.XML));
		}
	}

	public static Dbf<AdventureDbfRecord> Adventures
	{
		get
		{
			if (GameDbf.Adventure != null && Application.isPlaying)
			{
				return GameDbf.Adventure;
			}
			return s_adventures ?? (s_adventures = Dbf<AdventureDbfRecord>.Load("ADVENTURE", DbfFormat.XML));
		}
	}

	public static bool ShouldDisplayTier(ProductTierDataModel tier)
	{
		if (tier.BrowserButtons.Count > 0 && ServiceManager.TryGet<IProductDataService>(out var dataService))
		{
			if (dataService.TierHasShowIfAllOwnedTag(tier))
			{
				return true;
			}
			foreach (ShopBrowserButtonDataModel browserButton in tier.BrowserButtons)
			{
				if (ShouldDisplayButton(browserButton))
				{
					return true;
				}
			}
		}
		return false;
	}

	public static bool ShouldDisplayButton(ShopBrowserButtonDataModel button)
	{
		if (!button.IsFiller)
		{
			return AreProductOrVariantsPurchasable(button.DisplayProduct);
		}
		return false;
	}

	public static AssetHandle<GameObject> LoadStorePackPrefab(BoosterDbId boosterId)
	{
		BoosterDbfRecord booster = Boosters.GetRecord((int)boosterId);
		if (booster == null || string.IsNullOrEmpty(booster.StorePrefab))
		{
			return null;
		}
		return AssetLoader.Get().LoadAsset<GameObject>(booster.StorePrefab);
	}

	public static AssetHandle<GameObject> LoadStoreAdventurePrefab(AdventureDbId adventureId)
	{
		AdventureDbfRecord adventure = Adventures.GetRecord((int)adventureId);
		if (adventure == null || string.IsNullOrEmpty(adventure.StorePrefab))
		{
			return null;
		}
		return AssetLoader.Get().LoadAsset<GameObject>(adventure.StorePrefab);
	}

	public static long GetDeficit(PriceDataModel price)
	{
		CurrencyType currency = price.Currency;
		if (currency == CurrencyType.NONE || currency == CurrencyType.REAL_MONEY)
		{
			return 0L;
		}
		CurrencyManager currencyManager;
		long currentBalance = (ServiceManager.TryGet<CurrencyManager>(out currencyManager) ? currencyManager.GetBalance(price.Currency) : 0);
		long required = (long)price.Amount;
		if (required > currentBalance)
		{
			return required - currentBalance;
		}
		return 0L;
	}

	public static ProductDataModel FindCurrencyProduct(CurrencyType currencyType)
	{
		ProductDataModel product = null;
		if (ServiceManager.TryGet<IProductDataService>(out var dataService) && IsCurrencyVirtual(currencyType) && IsVirtualCurrencyEnabled())
		{
			TryGetMainVirtualCurrencyType(out var vcType);
			if (currencyType == vcType)
			{
				product = dataService.VirtualCurrencyProductItem;
			}
			else
			{
				TryGetBoosterVirtualCurrencyType(out var bcType);
				if (currencyType == bcType)
				{
					product = dataService.BoosterCurrencyProductItem;
				}
			}
		}
		if (product == null)
		{
			Log.Store.PrintError($"Couldn't find product for currency type {currencyType}");
		}
		return product;
	}

	public static ProductDataModel FindCurrencyProduct(CurrencyType currencyType, float requiredAmount)
	{
		ProductDataModel product = FindCurrencyProduct(currencyType);
		if (product == null)
		{
			return null;
		}
		ProductDataModel bestMatch = product;
		ProductDataModel bestSpecialOffer = null;
		float bestDifference = float.MinValue;
		float bestSpecialOfferDifference = float.MinValue;
		foreach (ProductDataModel variant in product.Variants)
		{
			if (variant.Availability != ProductAvailability.CAN_PURCHASE)
			{
				continue;
			}
			float amount = GetAmountOfCurrencyInProduct(variant, currencyType);
			float difference = amount - requiredAmount;
			if (difference >= 0f && bestDifference < 0f)
			{
				bestDifference = difference;
				bestMatch = variant;
			}
			else if (Math.Abs(difference) < Math.Abs(bestDifference))
			{
				bestDifference = difference;
				bestMatch = variant;
			}
			if (variant.Tags.Contains("special_offer") && amount >= requiredAmount)
			{
				float specialOfferDifference = amount - requiredAmount;
				if (bestSpecialOffer == null || specialOfferDifference < bestSpecialOfferDifference)
				{
					bestSpecialOffer = variant;
					bestSpecialOfferDifference = specialOfferDifference;
				}
			}
		}
		return bestSpecialOffer ?? bestMatch;
	}

	public static float GetAmountOfCurrencyInProduct(ProductDataModel product, CurrencyType currencyType)
	{
		return product.Items.FirstOrDefault((RewardItemDataModel i) => i.Currency != null && i.Currency.Currency == currencyType)?.Currency.Amount ?? 0f;
	}

	public static string GetCurrencyCode(CurrencyType currency)
	{
		return currency switch
		{
			CurrencyType.CN_RUNESTONES => "XSA", 
			CurrencyType.CN_ARCANE_ORBS => "XSB", 
			CurrencyType.ROW_RUNESTONES => "XSR", 
			CurrencyType.GOLD => "XSG", 
			CurrencyType.REAL_MONEY => StoreManager.Get()?.GetCurrencyCode(), 
			_ => "invalid", 
		};
	}

	public static CurrencyType GetCurrencyTypeFromCode(string code)
	{
		if (code != null && (code == null || code.Length != 0))
		{
			return code switch
			{
				"XSA" => CurrencyType.CN_RUNESTONES, 
				"XSB" => CurrencyType.CN_ARCANE_ORBS, 
				"XSR" => CurrencyType.ROW_RUNESTONES, 
				"XSG" => CurrencyType.GOLD, 
				_ => CurrencyType.REAL_MONEY, 
			};
		}
		return CurrencyType.NONE;
	}

	public static VirtualCurrencyMode GetVirtualCurrencyMode()
	{
		if (BattleNet.GetCurrentRegion() == BnetRegion.REGION_CN)
		{
			return VirtualCurrencyMode.China;
		}
		return VirtualCurrencyMode.Default;
	}

	public static bool IsVirtualCurrencyEnabled()
	{
		NetCache netCache = NetCache.Get();
		if (netCache == null)
		{
			return false;
		}
		return netCache.GetNetObject<NetCache.NetCacheFeatures>()?.Store.VirtualCurrencyEnabled ?? false;
	}

	public static bool IsCurrencyVirtual(CurrencyType currency)
	{
		if (currency == CurrencyType.CN_RUNESTONES || (uint)(currency - 5) <= 1u)
		{
			return true;
		}
		return false;
	}

	public static bool IsVirtualCurrencyTypeEnabled(CurrencyType currencyType)
	{
		if (!IsCurrencyVirtual(currencyType))
		{
			return false;
		}
		VirtualCurrencyMode vcMode = GetVirtualCurrencyMode();
		switch (currencyType)
		{
		case CurrencyType.CN_RUNESTONES:
		case CurrencyType.CN_ARCANE_ORBS:
			return vcMode == VirtualCurrencyMode.China;
		case CurrencyType.ROW_RUNESTONES:
			return vcMode == VirtualCurrencyMode.Default;
		default:
			Log.Store.PrintError($"Cannot determine if Virtual Currency is enabled. Unknown Currency type - {currencyType}.");
			return false;
		}
	}

	public static bool TryGetMainVirtualCurrencyType(out CurrencyType currencyType)
	{
		if (!BattleNet.IsConnected())
		{
			currencyType = CurrencyType.NONE;
			return false;
		}
		switch (GetVirtualCurrencyMode())
		{
		case VirtualCurrencyMode.Default:
			currencyType = CurrencyType.ROW_RUNESTONES;
			return true;
		case VirtualCurrencyMode.China:
			currencyType = CurrencyType.CN_RUNESTONES;
			return true;
		default:
			currencyType = CurrencyType.NONE;
			return false;
		}
	}

	public static bool IsMainVirtualCurrencyType(CurrencyType currencyType)
	{
		if (currencyType != CurrencyType.CN_RUNESTONES)
		{
			return currencyType == CurrencyType.ROW_RUNESTONES;
		}
		return true;
	}

	public static bool TryGetBoosterVirtualCurrencyType(out CurrencyType currencyType)
	{
		if (!BattleNet.IsConnected())
		{
			currencyType = CurrencyType.NONE;
			return false;
		}
		if (GetVirtualCurrencyMode() == VirtualCurrencyMode.China)
		{
			currencyType = CurrencyType.CN_ARCANE_ORBS;
			return true;
		}
		currencyType = CurrencyType.NONE;
		return false;
	}

	public static bool IsBoosterVirtualCurrencyType(CurrencyType currencyType)
	{
		return currencyType == CurrencyType.CN_ARCANE_ORBS;
	}

	public static bool IsVirtualCurrencyRewardItemType(RewardItemType rewardItemType)
	{
		if ((uint)(rewardItemType - 8) <= 1u || rewardItemType == RewardItemType.ROW_RUNESTONES)
		{
			return true;
		}
		return false;
	}

	public static RewardItemType GetRewardItemTypeFromCurrencyType(CurrencyType currencyType)
	{
		return currencyType switch
		{
			CurrencyType.GOLD => RewardItemType.GOLD, 
			CurrencyType.DUST => RewardItemType.DUST, 
			CurrencyType.ROW_RUNESTONES => RewardItemType.ROW_RUNESTONES, 
			CurrencyType.CN_RUNESTONES => RewardItemType.CN_RUNESTONES, 
			CurrencyType.CN_ARCANE_ORBS => RewardItemType.CN_ARCANE_ORBS, 
			CurrencyType.RENOWN => RewardItemType.MERCENARY_RENOWN, 
			CurrencyType.BG_TOKEN => RewardItemType.BATTLEGROUNDS_TOKEN, 
			_ => RewardItemType.UNDEFINED, 
		};
	}

	public static bool TryGetPriceFromBundleOrTransaction(ProductInfo bundle, NoGTAPPTransactionData transaction, CurrencyType currencyType, out long price)
	{
		price = 0L;
		if (transaction != null && currencyType == CurrencyType.GOLD)
		{
			return StoreManager.Get().GetGoldCostNoGTAPP(transaction, out price);
		}
		if (!bundle.TryGetBundlePrice(currencyType, out var bundlePrice))
		{
			Log.Store.PrintWarning($"Product {bundle.Id} does not have requested cost for {currencyType}");
			return false;
		}
		price = (long)bundlePrice;
		return true;
	}

	public static bool TryDecomposeBuyProductEventArgs(BuyProductEventArgs args, out ProductId productId, out string currencyCode, out double totalPrice, out int quantity, out string productItemType, out int productItemId)
	{
		productId = ProductId.InvalidProduct;
		currencyCode = null;
		totalPrice = 0.0;
		quantity = 0;
		productItemType = "";
		productItemId = 0;
		ProductInfo bundle = null;
		if (args == null || !ServiceManager.TryGet<IProductDataService>(out var dataService))
		{
			return false;
		}
		quantity = args.quantity;
		if (args is BuyPmtProductEventArgs pmtProductArgs)
		{
			productId = ProductId.CreateFrom(pmtProductArgs.pmtProductId);
			currencyCode = GetCurrencyCode(pmtProductArgs.paymentCurrency);
			dataService.TryGetProduct(productId, out bundle);
		}
		else
		{
			if (!(args is BuyNoGTAPPEventArgs noGtappArgs))
			{
				return false;
			}
			productItemType = noGtappArgs.transactionData.Product.ToString().ToLowerInvariant();
			productItemId = noGtappArgs.transactionData.ProductData;
			currencyCode = GetCurrencyCode(CurrencyType.GOLD);
			StoreManager.Get().GetGoldCostNoGTAPP(noGtappArgs.transactionData, out var goldPrice);
			totalPrice = goldPrice;
		}
		if (bundle != null)
		{
			productId = bundle.Id;
			CurrencyType currencyType = GetCurrencyTypeFromCode(currencyCode);
			if (!bundle.TryGetBundlePrice(currencyType, out totalPrice))
			{
				Log.Store.PrintWarning($"Product {bundle.Id} does not have requested cost for {currencyType}");
			}
			if (bundle.Items.Count == 1)
			{
				productItemType = bundle.Items[0].ItemType.ToString().ToLowerInvariant();
				productItemId = bundle.Items[0].ProductData;
			}
			totalPrice *= quantity;
		}
		return true;
	}

	public static CurrencyType GetCurrencyTypeFromProto(PegasusShared.CurrencyType protoCurrencyType)
	{
		return protoCurrencyType switch
		{
			PegasusShared.CurrencyType.CURRENCY_TYPE_DUST => CurrencyType.DUST, 
			PegasusShared.CurrencyType.CURRENCY_TYPE_ROW_RUNESTONES => CurrencyType.ROW_RUNESTONES, 
			PegasusShared.CurrencyType.CURRENCY_TYPE_CN_RUNESTONES => CurrencyType.CN_RUNESTONES, 
			PegasusShared.CurrencyType.CURRENCY_TYPE_CN_ARCANE_ORBS => CurrencyType.CN_ARCANE_ORBS, 
			PegasusShared.CurrencyType.CURRENCY_TYPE_GOLD => CurrencyType.GOLD, 
			PegasusShared.CurrencyType.CURRENCY_TYPE_RENOWN => CurrencyType.RENOWN, 
			_ => CurrencyType.NONE, 
		};
	}

	public static bool ShouldShowPriceStyles()
	{
		return true;
	}

	private static bool AreProductOrVariantsPurchasable(ProductDataModel product)
	{
		if (product.Availability != ProductAvailability.CAN_PURCHASE)
		{
			return product.Variants.Any((ProductDataModel p) => p.Availability == ProductAvailability.CAN_PURCHASE);
		}
		return true;
	}

	public static bool IsProductGamemodesAvailable(ProductDataModel product, out IGamemodeAvailabilityService.Status status)
	{
		status = IGamemodeAvailabilityService.Status.NONE;
		if (product == null || product.Items == null || product.Items.Count == 0)
		{
			return true;
		}
		if (product.RequiredGamemodes == null)
		{
			Log.Store.PrintWarning($"Product {product.PmtId} had null required gamemodes. Failed to determine availability...");
			return true;
		}
		if (!ServiceManager.TryGet<IGamemodeAvailabilityService>(out var gas))
		{
			return true;
		}
		foreach (IGamemodeAvailabilityService.Gamemode gm in product.RequiredGamemodes)
		{
			IGamemodeAvailabilityService.Status gmStatus = gas.GetGamemodeStatus(gm);
			if (gmStatus != IGamemodeAvailabilityService.Status.READY)
			{
				status = gmStatus;
				return false;
			}
		}
		status = IGamemodeAvailabilityService.Status.READY;
		return true;
	}
}
