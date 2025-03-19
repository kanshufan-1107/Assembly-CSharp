using System.Collections.Generic;
using Assets;
using Blizzard.Commerce;
using Blizzard.T5.Services;
using Hearthstone.DataModels;
using Hearthstone.Store;
using PegasusUtil;
using UnityEngine;

[CustomEditClass]
public static class HeroSkinUtils
{
	public struct HeroSkinProductData
	{
		public int ProductId { get; set; }

		public CardHero.PortraitCurrency CurrencyType { get; set; }

		public bool IsDelayedRelease { get; set; }
	}

	public static bool IsHeroSkinOwned(string cardId)
	{
		return CollectionManager.Get()?.IsCardOwned(cardId) ?? false;
	}

	public static bool CanToggleFavoriteHeroSkin(TAG_CLASS heroClass, string cardId)
	{
		CollectionManager collectionManager = CollectionManager.Get();
		if (collectionManager == null)
		{
			return false;
		}
		bool canFavorite = collectionManager.GetCountOfOwnedHeroesForClass(heroClass) > 1 && !collectionManager.IsFavoriteHero(cardId);
		bool canUnfavorite = collectionManager.GetFavoriteHeroesForClass(heroClass).Count > 1;
		if (IsHeroSkinOwned(cardId))
		{
			return canFavorite || canUnfavorite;
		}
		return false;
	}

	public static HeroSkinProductData GetCollectionManagerHeroSkinPurchaseProductData(string cardId)
	{
		CardDbfRecord cardRecord = GameUtils.GetCardRecord(cardId);
		CardHeroDbfRecord cardHeroRecord = GameUtils.GetCardHeroRecordForCardId(cardRecord.ID);
		HeroSkinProductData result;
		if (cardHeroRecord == null)
		{
			string logErrorSource = ((cardRecord == null) ? "to be found in card database" : "to get hero card from card record");
			Debug.LogError("HeroSkinUtils:GetCollectionManagerHeroSkinPurchaseProductData card " + cardId + " failed " + logErrorSource);
			result = default(HeroSkinProductData);
			result.CurrencyType = CardHero.PortraitCurrency.UNKNOWN;
			result.ProductId = 0;
			return result;
		}
		result = default(HeroSkinProductData);
		result.CurrencyType = cardHeroRecord.CollectionManagerPurchaseCurrency;
		result.ProductId = cardHeroRecord.CollectionManagerPurchaseProductId;
		result.IsDelayedRelease = cardHeroRecord.IsCollectionManagerPurchaseDelayed;
		return result;
	}

	public static bool CanBuyHeroSkinFromCollectionManager(string cardId)
	{
		PriceDataModel priceDataModel = GetCollectionManagerHeroSkinPriceDataModel(cardId);
		if (priceDataModel == null)
		{
			return false;
		}
		return CanBuyHeroSkinFromCollectionManager(cardId, priceDataModel.Currency, priceDataModel);
	}

	public static bool CanBuyHeroSkinFromCollectionManager(string cardId, CurrencyType currencyType, PriceDataModel priceDataModel = null)
	{
		if (IsHeroSkinOwned(cardId))
		{
			return false;
		}
		if (!IsHeroSkinPurchasableFromCollectionManager(cardId, priceDataModel))
		{
			return false;
		}
		return HasEnoughBalanceForHeroSkin(cardId, currencyType);
	}

	public static bool IsHeroSkinPurchasableFromCollectionManager(string cardId, PriceDataModel priceDataModel = null)
	{
		StoreManager storeManager = StoreManager.Get();
		if (!storeManager.IsOpen())
		{
			return false;
		}
		if (!storeManager.IsBuyHeroSkinsFromCollectionManagerEnabled())
		{
			return false;
		}
		HeroSkinProductData productData = GetCollectionManagerHeroSkinPurchaseProductData(cardId);
		if (!ProductId.IsValid(productData.ProductId))
		{
			return false;
		}
		if (priceDataModel == null && GetCollectionManagerHeroSkinPriceDataModel(cardId, productData) == null)
		{
			if (productData.IsDelayedRelease)
			{
				Debug.LogWarning("GetCollectionManagerHeroSkinProductBundle failed to get price data model for " + cardId + " - Error skipped as delayed released was true...");
			}
			else
			{
				Debug.LogError("HeroSkinUtils:IsHeroSkinPurchasableFromCollectionManager failed to get the price data model for Hero card " + cardId);
			}
			return false;
		}
		return true;
	}

	public static ProductInfo GetCollectionManagerHeroSkinProductBundle(string cardId)
	{
		HeroSkinProductData productData = GetCollectionManagerHeroSkinPurchaseProductData(cardId);
		if (!ProductId.IsValid(productData.ProductId) || !ServiceManager.TryGet<IProductDataService>(out var dataService))
		{
			return null;
		}
		int cardDbId = GameUtils.TranslateCardIdToDbId(cardId);
		if (!dataService.TryGetProduct(productData.ProductId, out var bundle))
		{
			if (productData.IsDelayedRelease)
			{
				Debug.LogWarning("GetCollectionManagerHeroSkinProductBundle failed to get bundle for Hero card " + cardId + " - Error skipped as delayed released was true...");
				return null;
			}
			Debug.LogError($"HeroSkinUtils:GetCollectionManagerHeroSkinProductBundle: Did not find a bundle with pmtProductId {productData.ProductId} for Hero card {cardId}");
			return null;
		}
		List<Network.BundleItem> items = bundle.Items;
		int i = 0;
		for (int iMax = items.Count; i < iMax; i++)
		{
			Network.BundleItem item = items[i];
			if (item.ItemType == ProductType.PRODUCT_TYPE_HERO && item.ProductData == cardDbId)
			{
				return bundle;
			}
		}
		Debug.LogError($"HeroSkinUtils:GetCollectionManagerHeroSkinProductBundle: Did not find any items with type PRODUCT_TYPE_HERO for bundle with pmtProductId {productData.ProductId} for Hero card {cardId}");
		return null;
	}

	public static PriceDataModel GetCollectionManagerHeroSkinPriceDataModel(string cardId)
	{
		HeroSkinProductData productData = GetCollectionManagerHeroSkinPurchaseProductData(cardId);
		if (!ProductId.IsValid(productData.ProductId))
		{
			return null;
		}
		return GetCollectionManagerHeroSkinPriceDataModel(cardId, productData);
	}

	public static PriceDataModel GetCollectionManagerHeroSkinPriceDataModel(string cardId, HeroSkinProductData productData)
	{
		ProductInfo bundle = GetCollectionManagerHeroSkinProductBundle(cardId);
		if (bundle == null)
		{
			if (productData.IsDelayedRelease)
			{
				Debug.LogWarning("HeroSkinUtils:GetCollectionManagerHeroSkinPriceDataModel failed to get bundle for Hero card " + cardId + " - Error skipped as delayed released was true...");
				return null;
			}
			Debug.LogError("HeroSkinUtils:GetCollectionManagerHeroSkinPriceDataModel failed to get bundle for Hero card " + cardId);
			return null;
		}
		CurrencyType currencyType;
		switch (productData.CurrencyType)
		{
		case CardHero.PortraitCurrency.GOLD:
			currencyType = CurrencyType.GOLD;
			if (!bundle.HasCurrency(currencyType))
			{
				Debug.LogError("HeroSkinUtils:GetCollectionManagerHeroSkinPriceDataModel bundle for Hero card " + cardId + " has no GTAPP gold cost");
				return null;
			}
			break;
		case CardHero.PortraitCurrency.REAL_MONEY:
			currencyType = CurrencyType.REAL_MONEY;
			if (!bundle.HasCurrency(currencyType))
			{
				Debug.LogError("HeroSkinUtils:GetCollectionManagerHeroSkinPriceDataModel bundle for Hero card " + cardId + " has no real money cost");
				return null;
			}
			break;
		case CardHero.PortraitCurrency.VIRTUAL_CURRENCY:
			currencyType = bundle.GetFirstVirtualCurrencyPriceType();
			if (currencyType == CurrencyType.NONE)
			{
				Debug.LogError("HeroSkinUtils:GetCollectionManagerHeroSkinPriceDataModel failed to pull VC type for card " + cardId + ".");
				return null;
			}
			if (!bundle.HasCurrency(currencyType))
			{
				Debug.LogError("HeroSkinUtils:GetCollectionManagerHeroSkinPriceDataModel bundle for Hero card " + cardId + " has no virtual currency cost");
				return null;
			}
			break;
		default:
			Debug.LogError(string.Format("{0} bundle for Hero card {1} do to unhandled currency type {2}!", "HeroSkinUtils:GetCollectionManagerHeroSkinPriceDataModel", cardId, productData.CurrencyType));
			return null;
		}
		return bundle.GetPriceDataModel(currencyType);
	}

	public static bool HasEnoughBalanceForHeroSkin(string cardId, CurrencyType currencyType = CurrencyType.GOLD)
	{
		ProductInfo bundle = GetCollectionManagerHeroSkinProductBundle(cardId);
		if (bundle == null)
		{
			Debug.LogError("HeroSkinUtils:HasEnoughBalanceForHeroSkin called for a card with no valid product bundle. Hero card Id = " + cardId);
			return false;
		}
		if (currencyType == CurrencyType.REAL_MONEY)
		{
			return true;
		}
		if (!bundle.TryGetVCPrice(currencyType, out var amount))
		{
			return false;
		}
		if (!ServiceManager.TryGet<CurrencyManager>(out var currencyManager))
		{
			return false;
		}
		return currencyManager.GetBalance(currencyType) >= amount;
	}
}
