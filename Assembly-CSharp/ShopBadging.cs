using System;
using System.Collections.Generic;
using System.Linq;
using Hearthstone.DataModels;

public class ShopBadging
{
	public enum ProductStatus
	{
		Undefined,
		InShop,
		Displayed
	}

	private ShopCachedList m_seenProductsLocalCache = new ShopCachedList();

	private ShopCachedList m_seenProductsServerCache = new ShopCachedList();

	private ShopCachedList m_displayedProductsLocalCache = new ShopCachedList();

	private ShopCachedList m_displayedProductsServerCache = new ShopCachedList();

	private bool m_gsdInitialized;

	public HashSet<string> ListOfSeenProductIds => m_seenProductsLocalCache.ProductIds;

	public HashSet<string> ListOfDisplayedProductIds => m_displayedProductsLocalCache.GetProductsByState(ProductStatus.Displayed);

	public HashSet<string> ListOfAllProductsInShop => m_displayedProductsLocalCache.GetAllProductIds();

	public ShopBadging()
	{
		InitializeLocalCache();
		GameSaveDataManager.Get().OnGameSaveDataUpdate += OnGameSaveDataReceived;
		Log.Store.Print($"[ShopBadging::Initialize] Local Cache | {Option.LATEST_SEEN_SHOP_PRODUCT_LIST}: {SerializeCachedList(m_seenProductsLocalCache)}");
		Log.Store.Print($"[ShopBadging::Initialize] Local Cache | {Option.LATEST_DISPLAYED_SHOP_PRODUCT_LIST}: {SerializeCachedList(m_displayedProductsLocalCache)}");
	}

	public void Dispose()
	{
		GameSaveDataManager.Get().OnGameSaveDataUpdate -= OnGameSaveDataReceived;
	}

	public bool HasProductBeenSeen(long pmtId)
	{
		return m_seenProductsLocalCache.ProductIds.Contains(pmtId.ToString());
	}

	public void MarkProductAsSeen(ProductDataModel product)
	{
		if (!product.Tags.Remove("new"))
		{
			return;
		}
		if (product.PmtId == 0L)
		{
			foreach (ProductDataModel variant in product.Variants)
			{
				m_seenProductsLocalCache.ProductIds.Add(variant.PmtId.ToString());
			}
		}
		else if (!m_seenProductsLocalCache.ProductIds.Add(product.PmtId.ToString()))
		{
			return;
		}
		m_seenProductsLocalCache.Version++;
		SaveSeenProductList();
	}

	public void MarkTabAsDisplayed(IEnumerable<string> displayedProducts)
	{
		foreach (string productId in displayedProducts)
		{
			m_displayedProductsLocalCache.UpdateProductState(productId, ProductStatus.Displayed);
		}
		m_displayedProductsLocalCache.Version++;
		SaveDisplayedProductList();
	}

	public void CacheAllProductsInShop(List<string> products)
	{
		foreach (string productId in products)
		{
			ProductStatus? productStatus = m_displayedProductsLocalCache.GetProductStatus(productId);
			if (!productStatus.HasValue || productStatus.Value != ProductStatus.Displayed)
			{
				m_displayedProductsLocalCache.UpdateProductState(productId, ProductStatus.InShop);
			}
		}
		m_displayedProductsLocalCache.Version++;
		SaveDisplayedProductList();
	}

	public void MarkProductsAsSeen(List<string> products)
	{
		foreach (string product in products)
		{
			m_seenProductsLocalCache.ProductIds.Add(product);
		}
		m_seenProductsLocalCache.Version++;
		SaveSeenProductList();
	}

	public void ClearLatestDisplayedProducts()
	{
		m_displayedProductsLocalCache = new ShopCachedList
		{
			Version = ((m_displayedProductsLocalCache != null) ? m_displayedProductsLocalCache.Version++ : 0)
		};
		m_displayedProductsServerCache = new ShopCachedList
		{
			Version = ((m_displayedProductsLocalCache != null) ? m_displayedProductsLocalCache.Version++ : 0)
		};
		SaveDisplayedProductList();
	}

	public void ClearSeenProducts()
	{
		m_seenProductsLocalCache = new ShopCachedList
		{
			Version = ((m_seenProductsLocalCache != null) ? m_seenProductsLocalCache.Version++ : 0)
		};
		m_seenProductsServerCache = new ShopCachedList
		{
			Version = ((m_seenProductsLocalCache != null) ? m_seenProductsLocalCache.Version++ : 0)
		};
		SaveSeenProductList();
	}

	public void ResetBadging()
	{
		m_displayedProductsLocalCache = new ShopCachedList();
		m_displayedProductsServerCache = new ShopCachedList();
		SaveDisplayedProductList();
		m_seenProductsLocalCache = new ShopCachedList();
		m_seenProductsServerCache = new ShopCachedList();
		SaveSeenProductList();
	}

	private void InitializeLocalCache()
	{
		m_seenProductsLocalCache = LoadCacheFromLocalFile(Option.LATEST_SEEN_SHOP_PRODUCT_LIST);
		m_displayedProductsLocalCache = LoadCacheFromLocalFile(Option.LATEST_DISPLAYED_SHOP_PRODUCT_LIST);
		m_displayedProductsLocalCache.UpdateProductList(ProductStatus.Displayed);
		SaveToLocalFile(Option.LATEST_DISPLAYED_SHOP_PRODUCT_LIST, m_displayedProductsLocalCache);
	}

	private void ReconcileCache()
	{
		ReconcileProductListCache(ref m_seenProductsLocalCache, ref m_seenProductsServerCache, Option.LATEST_SEEN_SHOP_PRODUCT_LIST, GameSaveKeySubkeyId.LIST_OF_SEEN_SHOP_PRODUCTS, GameSaveKeySubkeyId.SEEN_SHOP_PRODUCTS_LIST_VERSION);
		ReconcileProductListCache(ref m_displayedProductsLocalCache, ref m_displayedProductsServerCache, Option.LATEST_DISPLAYED_SHOP_PRODUCT_LIST, GameSaveKeySubkeyId.LIST_OF_LATEST_DISPLAYED_SHOP_PRODUCTS, GameSaveKeySubkeyId.DISPLAYED_SHOP_PRODUCTS_LIST_VERSION);
		m_displayedProductsLocalCache.UpdateProductList(ProductStatus.Displayed);
		SaveDisplayedProductList();
	}

	private void ReconcileProductListCache(ref ShopCachedList localCache, ref ShopCachedList serverCache, Option localOption, GameSaveKeySubkeyId serverListKey, GameSaveKeySubkeyId serverVersionKey)
	{
		localCache = LoadCacheFromLocalFile(localOption);
		serverCache = LoadCacheFromGameSaveData(serverListKey, serverVersionKey);
		Log.Store.Print("[ShopBadging] Local Cache | " + localOption.ToString() + ": " + SerializeCachedList(localCache));
		Log.Store.Print("[ShopBadging] Server Cache | " + serverListKey.ToString() + ": " + SerializeCachedList(serverCache));
		if (localOption == Option.LATEST_SEEN_SHOP_PRODUCT_LIST)
		{
			MergeLists(ref localCache, ref serverCache, localOption, serverListKey, serverVersionKey);
		}
		else if (localCache.Version > serverCache.Version)
		{
			SaveToServer(serverListKey, serverVersionKey, localCache);
			serverCache = localCache;
		}
		else if (localCache.Version < serverCache.Version)
		{
			MergeLists(ref localCache, ref serverCache, localOption, serverListKey, serverVersionKey);
		}
	}

	private ShopCachedList LoadCacheFromLocalFile(Option option)
	{
		if (option != Option.LATEST_DISPLAYED_SHOP_PRODUCT_LIST && option != Option.LATEST_SEEN_SHOP_PRODUCT_LIST)
		{
			return new ShopCachedList();
		}
		string optionStr = Options.Get().GetString(option);
		return ParseCachedList(optionStr);
	}

	private ShopCachedList LoadCacheFromGameSaveData(GameSaveKeySubkeyId listKey, GameSaveKeySubkeyId version)
	{
		if (listKey != GameSaveKeySubkeyId.LIST_OF_LATEST_DISPLAYED_SHOP_PRODUCTS && listKey != GameSaveKeySubkeyId.LIST_OF_SEEN_SHOP_PRODUCTS && version != GameSaveKeySubkeyId.DISPLAYED_SHOP_PRODUCTS_LIST_VERSION && version != GameSaveKeySubkeyId.SEEN_SHOP_PRODUCTS_LIST_VERSION)
		{
			return new ShopCachedList();
		}
		if (!GameSaveDataManager.Get().GetSubkeyValue(GameSaveKeyId.PLAYER_OPTIONS, listKey, out List<string> shopProducts))
		{
			shopProducts = new List<string>();
		}
		if (!GameSaveDataManager.Get().GetSubkeyValue(GameSaveKeyId.PLAYER_OPTIONS, version, out long listVersion))
		{
			listVersion = 0L;
		}
		return new ShopCachedList(TryParseVersion(listVersion), shopProducts.Select((string id) => id.ToString()).ToList());
	}

	private ShopCachedList ParseCachedList(string data)
	{
		if (string.IsNullOrEmpty(data))
		{
			return new ShopCachedList();
		}
		string[] parts = data.Split('|');
		if (parts.Length == 2)
		{
			int version = TryParseVersion(parts[0]);
			List<string> productIds = SplitAndFilter(parts[1], ':');
			return new ShopCachedList(version, productIds);
		}
		List<string> productIds2 = new List<string>();
		return new ShopCachedList(0, productIds2);
	}

	private List<string> SplitAndFilter(string data, char delimiter)
	{
		List<string> result = new List<string>();
		string[] array = data.Split(delimiter);
		foreach (string part in array)
		{
			if (!string.IsNullOrEmpty(part))
			{
				result.Add(part);
			}
		}
		return result;
	}

	private void SaveToLocalFile(Option option, ShopCachedList cachedList)
	{
		if (cachedList != null && (option == Option.LATEST_DISPLAYED_SHOP_PRODUCT_LIST || option == Option.LATEST_SEEN_SHOP_PRODUCT_LIST))
		{
			string serializedList = SerializeCachedList(cachedList);
			Options.Get().SetString(option, serializedList);
		}
	}

	private void SaveToServer(GameSaveKeySubkeyId listKey, GameSaveKeySubkeyId versionKey, ShopCachedList cachedList)
	{
		if (cachedList != null && m_gsdInitialized && (listKey == GameSaveKeySubkeyId.LIST_OF_LATEST_DISPLAYED_SHOP_PRODUCTS || listKey == GameSaveKeySubkeyId.LIST_OF_SEEN_SHOP_PRODUCTS))
		{
			string[] latestProductIds = cachedList.GetXLatestProducts(100).ToArray();
			GameSaveDataManager.Get().SaveSubkey(new GameSaveDataManager.SubkeySaveRequest(GameSaveKeyId.PLAYER_OPTIONS, listKey, latestProductIds));
			GameSaveDataManager.Get().SaveSubkey(new GameSaveDataManager.SubkeySaveRequest(GameSaveKeyId.PLAYER_OPTIONS, versionKey, cachedList.Version));
		}
	}

	private void SaveDisplayedProductList()
	{
		SaveToLocalFile(Option.LATEST_DISPLAYED_SHOP_PRODUCT_LIST, m_displayedProductsLocalCache);
		SaveToServer(GameSaveKeySubkeyId.LIST_OF_LATEST_DISPLAYED_SHOP_PRODUCTS, GameSaveKeySubkeyId.DISPLAYED_SHOP_PRODUCTS_LIST_VERSION, m_displayedProductsLocalCache);
	}

	private void SaveSeenProductList()
	{
		SaveToLocalFile(Option.LATEST_SEEN_SHOP_PRODUCT_LIST, m_seenProductsLocalCache);
		SaveToServer(GameSaveKeySubkeyId.LIST_OF_SEEN_SHOP_PRODUCTS, GameSaveKeySubkeyId.SEEN_SHOP_PRODUCTS_LIST_VERSION, m_seenProductsLocalCache);
	}

	private void MergeLists(ref ShopCachedList localList, ref ShopCachedList serverList, Option localOption, GameSaveKeySubkeyId serverListKey, GameSaveKeySubkeyId serverVersionKey)
	{
		HashSet<string> mergedSet = new HashSet<string>(localList.ProductIds);
		foreach (string productId in serverList.ProductIds)
		{
			mergedSet.Add(productId);
		}
		ShopCachedList mergedList = new ShopCachedList(Math.Max(localList.Version, serverList.Version) + 1, new List<string>(mergedSet));
		SaveToLocalFile(localOption, mergedList);
		SaveToServer(serverListKey, serverVersionKey, mergedList);
		localList = mergedList;
		serverList = mergedList;
	}

	private string SerializeCachedList(ShopCachedList cachedList)
	{
		return string.Format("{0}|{1}", cachedList.Version, string.Join(":", cachedList.ProductIds));
	}

	private void OnGameSaveDataReceived(GameSaveKeyId key)
	{
		if (key == GameSaveKeyId.PLAYER_OPTIONS)
		{
			Log.Store.Print("[ShopBadging::OnGameSaveDataReceived] Shop Badging cache received");
			m_gsdInitialized = true;
			ReconcileCache();
		}
	}

	private int TryParseVersion(string versionString)
	{
		if (!int.TryParse(versionString, out var version) || version < 0 || version == int.MaxValue)
		{
			Log.Store.PrintError("[ShopBadging::TryParseVersion] Version exceeds max value. Resetting to 0");
			return 0;
		}
		return version;
	}

	private int TryParseVersion(long version)
	{
		return TryParseVersion(version.ToString());
	}
}
