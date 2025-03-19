using System.Collections.Generic;
using System.Linq;
using Assets;
using Blizzard.T5.Services;
using Hearthstone.Store;
using PegasusUtil;
using UnityEngine;

public class ShopProductDataConverter : MonoBehaviour
{
	[SerializeField]
	private ShopProductData m_data;

	[SerializeField]
	private bool m_captureCurrentCatalog;

	[SerializeField]
	private bool m_generateTestData;

	[SerializeField]
	private bool m_flushData;

	private List<ShopProductData.ProductItemData> m_itemCatalog;

	private List<ShopProductData.ProductData> m_productCatalog;

	private long m_fakeLicenseId;

	private void Update()
	{
		if (m_captureCurrentCatalog && ServiceManager.TryGet<IProductDataService>(out var dataService) && dataService.HasStoreLoaded())
		{
			m_captureCurrentCatalog = false;
			SnapshotCurrentCatalog();
		}
		if (m_generateTestData)
		{
			m_generateTestData = false;
			BuildTestData();
		}
		if (m_flushData)
		{
			m_flushData = false;
			FlushData();
		}
	}

	private void SnapshotCurrentCatalog()
	{
		if (ServiceManager.TryGet<IProductDataService>(out var dataService))
		{
			dataService.GetCurrentCatalogSnapshot(ref m_data, ref m_fakeLicenseId);
			m_productCatalog = new List<ShopProductData.ProductData>(m_data.productCatalog);
			m_itemCatalog = new List<ShopProductData.ProductItemData>(m_data.productItemCatalog);
		}
	}

	private void BuildTestData()
	{
		m_itemCatalog = new List<ShopProductData.ProductItemData>();
		m_productCatalog = new List<ShopProductData.ProductData>();
		m_fakeLicenseId = 404000L;
		foreach (BoosterDbfRecord boosterRec in GameDbf.Booster.GetRecords())
		{
			if (!(boosterRec.Name == "") && boosterRec.StorePrefab != null)
			{
				StorePackId storePackId = default(StorePackId);
				storePackId.Type = StorePackType.BOOSTER;
				storePackId.Id = boosterRec.ID;
				if (!AddItemsAndProductsFromStorePack(storePackId))
				{
					Log.Store.PrintWarning("Could not add test data from Network.Bundles for booster '{0}' (storePackId: {1})", boosterRec.Name, storePackId);
					AddDummyItemAndProduct(RewardItemType.BOOSTER, boosterRec.ID, boosterRec.Name);
				}
			}
		}
		int cardbackQuota = 10;
		foreach (CardBackDbfRecord cardbackRec in GameDbf.CardBack.GetRecords())
		{
			if (!(cardbackRec.Name == ""))
			{
				AddDummyItemAndProduct(RewardItemType.CARD_BACK, cardbackRec.ID, cardbackRec.Name);
				if (--cardbackQuota <= 0)
				{
					break;
				}
			}
		}
		foreach (CardHeroDbfRecord heroRec in GameDbf.CardHero.GetRecords())
		{
			CardDbfRecord cardRec = GameDbf.Card.GetRecord(heroRec.CardId);
			if (cardRec != null && !(cardRec.Name == "") && heroRec.HeroType != CardHero.HeroType.BATTLEGROUNDS_GUIDE && heroRec.HeroType != CardHero.HeroType.BATTLEGROUNDS_HERO)
			{
				StoreManager.Get().GetHeroBundleByCardDbId(heroRec.CardId, out var heroBundle);
				if (heroBundle == null || !AddItemsAndProductsFromNetBundle(heroBundle, cardRec.Name))
				{
					Log.Store.PrintWarning("Could not add test data from Network.Bundles for card '{0}' (hero CardId: {1})", cardRec.Name, heroRec.CardId);
					AddDummyItemAndProduct(RewardItemType.HERO_SKIN, cardRec.ID, cardRec.Name);
				}
			}
		}
		m_data.productItemCatalog = m_itemCatalog.ToArray();
		m_data.productCatalog = m_productCatalog.ToArray();
	}

	private bool AddItemsAndProductsFromStorePack(StorePackId storePackId)
	{
		ProductType productType = StorePackId.GetProductTypeFromStorePackType(storePackId);
		int productCount = GameUtils.GetProductDataCountFromStorePackId(storePackId);
		List<ProductInfo> netBundles = new List<ProductInfo>();
		if (ServiceManager.TryGet<IProductDataService>(out var dataService))
		{
			for (int bundleIndex = 0; bundleIndex < productCount; bundleIndex++)
			{
				List<ProductInfo> productBundles = dataService.GetAllBundlesForProduct(productType, requireRealMoneyOption: false, GameUtils.GetProductDataFromStorePackId(storePackId, bundleIndex), 0, checkAvailability: false);
				netBundles = netBundles.Concat(productBundles).ToList();
			}
		}
		int newProductCount = 0;
		foreach (ProductInfo netBundle in netBundles)
		{
			string debugName = $"(DEBUG) Type: {storePackId.Type}-{storePackId.Id}; productId:{(netBundle.Id.IsValid() ? netBundle.Id.Value : (-1))}";
			if (AddItemsAndProductsFromNetBundle(netBundle, debugName))
			{
				newProductCount++;
			}
		}
		return newProductCount > 0;
	}

	private bool AddItemsAndProductsFromNetBundle(ProductInfo netBundle, string debugName, List<string> overrideTags = null)
	{
		long productId = (netBundle.Id.IsValid() ? netBundle.Id.Value : GetUniqueFakeId());
		if (m_productCatalog.Exists((ShopProductData.ProductData p) => p.productId == productId))
		{
			return false;
		}
		ShopProductData.ProductData productData = default(ShopProductData.ProductData);
		productData.name = StoreManager.Get().GetProductName(netBundle);
		productData.description = debugName;
		List<string> tags = new List<string>();
		List<long> bundledLicenses = new List<long>();
		foreach (Network.BundleItem item in netBundle.Items)
		{
			ShopProductData.ProductItemData itemData = GenerateProductItemData(item);
			if (itemData.itemType != 0)
			{
				if (!m_itemCatalog.Exists((ShopProductData.ProductItemData i) => i.licenseId == itemData.licenseId))
				{
					m_itemCatalog.Add(itemData);
				}
				bundledLicenses.Add(itemData.licenseId);
				GetTags(itemData, ref tags);
			}
		}
		productData.licenseIds = bundledLicenses.ToArray();
		if (overrideTags != null)
		{
			tags = overrideTags;
		}
		productData.tags = SerializeTags(tags);
		productData.productId = productId;
		AddPricesFromNetBundle(ref productData, netBundle);
		m_productCatalog.Add(productData);
		return true;
	}

	private ShopProductData.ProductItemData GenerateProductItemData(Network.BundleItem bundleItem)
	{
		ShopProductData.ProductItemData itemData = default(ShopProductData.ProductItemData);
		switch (bundleItem.ItemType)
		{
		case ProductType.PRODUCT_TYPE_BOOSTER:
			itemData.itemType = RewardItemType.BOOSTER;
			break;
		case ProductType.PRODUCT_TYPE_CARD_BACK:
			itemData.itemType = RewardItemType.CARD_BACK;
			break;
		case ProductType.PRODUCT_TYPE_HERO:
		{
			RewardItemType itemType = RewardItemType.HERO_SKIN;
			int cardDbiId = bundleItem.ProductData;
			if (CollectionManager.Get().IsBattlegroundsHeroSkinCard(cardDbiId))
			{
				itemType = RewardItemType.BATTLEGROUNDS_HERO_SKIN;
			}
			else if (CollectionManager.Get().IsBattlegroundsGuideSkinCard(cardDbiId))
			{
				itemType = RewardItemType.BATTLEGROUNDS_GUIDE_SKIN;
			}
			itemData.itemType = itemType;
			break;
		}
		case ProductType.PRODUCT_TYPE_BATTLEGROUNDS_BOARD_SKIN:
			itemData.itemType = RewardItemType.BATTLEGROUNDS_BOARD_SKIN;
			break;
		case ProductType.PRODUCT_TYPE_BATTLEGROUNDS_FINISHER:
			itemData.itemType = RewardItemType.BATTLEGROUNDS_FINISHER;
			break;
		case ProductType.PRODUCT_TYPE_BATTLEGROUNDS_EMOTE:
			itemData.itemType = RewardItemType.BATTLEGROUNDS_EMOTE;
			break;
		case ProductType.PRODUCT_TYPE_LUCKY_DRAW:
			itemData.itemType = RewardItemType.LUCKY_DRAW;
			break;
		default:
			itemData.itemType = RewardItemType.UNDEFINED;
			break;
		}
		itemData.itemId = bundleItem.ProductData;
		itemData.quantity = bundleItem.Quantity;
		itemData.licenseId = GetUniqueFakeId();
		FillInDebugItemName(ref itemData);
		return itemData;
	}

	public static void FillInDebugItemName(ref ShopProductData.ProductItemData itemData)
	{
		string productName;
		switch (itemData.itemType)
		{
		case RewardItemType.BOOSTER:
			productName = GameDbf.Booster.GetRecord(itemData.itemId).Name;
			break;
		case RewardItemType.CARD_BACK:
			productName = GameDbf.CardBack.GetRecord(itemData.itemId).Name;
			break;
		case RewardItemType.HERO_SKIN:
		case RewardItemType.CARD:
		case RewardItemType.BATTLEGROUNDS_HERO_SKIN:
		case RewardItemType.BATTLEGROUNDS_GUIDE_SKIN:
			productName = GameDbf.Card.GetRecord(itemData.itemId).Name;
			break;
		case RewardItemType.BATTLEGROUNDS_BOARD_SKIN:
			productName = GameDbf.BattlegroundsBoardSkin.GetRecord(itemData.itemId).CollectionName;
			break;
		case RewardItemType.BATTLEGROUNDS_FINISHER:
			productName = GameDbf.BattlegroundsFinisher.GetRecord(itemData.itemId).CollectionName;
			break;
		case RewardItemType.BATTLEGROUNDS_EMOTE:
			productName = GameDbf.BattlegroundsEmote.GetRecord(itemData.itemId).CollectionShortName;
			break;
		default:
			productName = $"{itemData.itemType}-{itemData.itemId}";
			break;
		}
		if (itemData.quantity == 1)
		{
			itemData.debugName = $"{productName} ({itemData.itemType})";
		}
		else
		{
			itemData.debugName = $"{productName} x{itemData.quantity}";
		}
	}

	private void AddDummyItemAndProduct(RewardItemType itemType, int itemId, string debugName, string[] tagsOverride = null)
	{
		ShopProductData.ProductItemData itemData = default(ShopProductData.ProductItemData);
		itemData.itemType = itemType;
		itemData.itemId = itemId;
		itemData.debugName = "[PH] " + debugName;
		itemData.licenseId = GetUniqueFakeId();
		itemData.quantity = 1;
		m_itemCatalog.Add(itemData);
		ShopProductData.ProductData productData = default(ShopProductData.ProductData);
		productData.name = itemData.debugName;
		productData.description = itemData.ToString();
		productData.licenseIds = new long[1] { itemData.licenseId };
		productData.productId = GetUniqueFakeId();
		ShopProductData.PriceData debugPrice = default(ShopProductData.PriceData);
		debugPrice.currencyType = CurrencyType.GOLD;
		debugPrice.amount = 404.0;
		productData.prices = new ShopProductData.PriceData[1] { debugPrice };
		List<string> tags = new List<string>();
		if (tagsOverride != null)
		{
			tags = tagsOverride.ToList();
		}
		else
		{
			GetTags(itemData, ref tags);
		}
		productData.tags = SerializeTags(tags);
		m_productCatalog.Add(productData);
	}

	private void AddPricesFromNetBundle(ref ShopProductData.ProductData productData, ProductInfo netBundle)
	{
		List<ShopProductData.PriceData> prices = new List<ShopProductData.PriceData>();
		ShopProductData.PriceData priceData = default(ShopProductData.PriceData);
		if (netBundle.TryGetRMPrice(out var rmAmount))
		{
			priceData.currencyType = CurrencyType.REAL_MONEY;
			priceData.amount = rmAmount;
			prices.Add(priceData);
		}
		if (netBundle.TryGetVCPrice(CurrencyType.GOLD, out var vcAmount))
		{
			priceData.currencyType = CurrencyType.GOLD;
			priceData.amount = vcAmount;
			prices.Add(priceData);
		}
		productData.prices = prices.ToArray();
	}

	private void FlushData()
	{
	}

	private string GetTags(ShopProductData.ProductItemData itemData)
	{
		List<string> tags = new List<string>();
		GetTags(itemData, ref tags);
		return SerializeTags(tags);
	}

	private void GetTags(ShopProductData.ProductItemData itemData, ref List<string> tags)
	{
		List<string> addedTags = new List<string>();
		switch (itemData.itemType)
		{
		case RewardItemType.BOOSTER:
			addedTags.Add("booster");
			break;
		case RewardItemType.CARD_BACK:
			addedTags.Add("cardback");
			break;
		case RewardItemType.HERO_SKIN:
		case RewardItemType.BATTLEGROUNDS_HERO_SKIN:
		case RewardItemType.BATTLEGROUNDS_GUIDE_SKIN:
		case RewardItemType.BATTLEGROUNDS_BOARD_SKIN:
		case RewardItemType.BATTLEGROUNDS_FINISHER:
			addedTags.Add("skin");
			break;
		case RewardItemType.BATTLEGROUNDS_EMOTE:
		case RewardItemType.BATTLEGROUNDS_EMOTE_PILE:
			addedTags.Add("emote");
			break;
		}
		if (itemData.itemType == RewardItemType.BOOSTER)
		{
			switch (itemData.itemId)
			{
			case 1:
				addedTags.Add("classic");
				break;
			case 9:
			case 11:
			case 19:
			case 20:
			case 21:
			case 30:
				addedTags.Add("wild");
				break;
			case 17:
				addedTags.Add("welcome_bundle");
				addedTags.Add("bad_prefab");
				break;
			case 256:
				addedTags.Add("theme_pack");
				addedTags.Add("rogue_theme");
				break;
			}
		}
		foreach (string tag in addedTags)
		{
			if (!tags.Contains(tag))
			{
				tags.Add(tag);
			}
		}
	}

	private string SerializeTags(List<string> tags)
	{
		return string.Join(",", tags.ToArray());
	}

	private long GetUniqueFakeId()
	{
		return m_fakeLicenseId++;
	}
}
