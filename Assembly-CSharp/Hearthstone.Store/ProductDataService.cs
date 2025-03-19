using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Blizzard.Commerce;
using Blizzard.T5.Jobs;
using Blizzard.T5.Logging;
using Blizzard.T5.Services;
using com.blizzard.commerce.Model;
using Hearthstone.Commerce;
using Hearthstone.Core;
using Hearthstone.DataModels;
using Hearthstone.UI;
using PegasusUtil;

namespace Hearthstone.Store;

public class ProductDataService : IProductDataService, IService
{
	private ShopBadging m_shopBadging;

	private bool m_hasSetPages;

	private bool m_hasValidatedProducts;

	private long m_tiersChangeCountAtLastRefresh = -1L;

	private HashSet<string> m_requestedShopPlacementIds = new HashSet<string>();

	private HashSet<string> m_receivedShopPlacementIds = new HashSet<string>();

	public ProductCatalog Catalog { get; private set; }

	public CatalogNetworkPages CatalogNetworkPages { get; } = new CatalogNetworkPages();

	public HashSet<string> ShopPlacementIds { get; } = new HashSet<string>();

	public HashSet<long> ProductCatalog { get; } = new HashSet<long>();

	public Dictionary<ProductId, ProductInfo> ProductMap { get; } = new Dictionary<ProductId, ProductInfo>();

	public Dictionary<(ProductId, PlatformId), string> ExternalIds { get; } = new Dictionary<(ProductId, PlatformId), string>();

	public ProductDataModel VirtualCurrencyProductItem => Catalog.VirtualCurrencyProductItem;

	public ProductDataModel BoosterCurrencyProductItem => Catalog.BoosterCurrencyProductItem;

	public bool HasTierChangedSinceLastDataModelRetrievel
	{
		get
		{
			if (HasStoreLoaded())
			{
				return Catalog.TiersChangeCount != m_tiersChangeCountAtLastRefresh;
			}
			return true;
		}
	}

	public Type[] GetDependencies()
	{
		return new Type[2]
		{
			typeof(Network),
			typeof(HearthstoneCheckout)
		};
	}

	public IEnumerator<IAsyncJobResult> Initialize(ServiceLocator serviceLocator)
	{
		GamemodeAvailabilityService.GamemodeStateChange -= OnGamemodeAvailablityChanged;
		GamemodeAvailabilityService.GamemodeStateChange += OnGamemodeAvailablityChanged;
		StoreManager store;
		do
		{
			store = StoreManager.Get();
			yield return null;
		}
		while (store == null);
		Catalog = new ProductCatalog(store, this);
		m_shopBadging = new ShopBadging();
	}

	public void Shutdown()
	{
		GamemodeAvailabilityService.GamemodeStateChange -= OnGamemodeAvailablityChanged;
		Catalog = null;
		if (m_shopBadging != null)
		{
			m_shopBadging.Dispose();
			m_shopBadging = null;
		}
	}

	private void OnGamemodeAvailablityChanged(IGamemodeAvailabilityService.Gamemode mode, IGamemodeAvailabilityService.Status newStatus, IGamemodeAvailabilityService.Status oldStatus)
	{
		if (newStatus >= IGamemodeAvailabilityService.Status.READY || oldStatus >= IGamemodeAvailabilityService.Status.READY)
		{
			Catalog?.MarkProductDataAsDirty();
		}
	}

	private IEnumerable<ProductInfo> GetAllProductsContainingItem(ProductType itemType, int productData)
	{
		foreach (ProductInfo product in ProductMap.Values)
		{
			if (DoesProductContainProduct(product, itemType, productData))
			{
				yield return product;
			}
		}
	}

	public static bool DoesProductContainProduct(ProductInfo product, ProductType itemType, int productData = 0, int numItemsRequired = 0)
	{
		if (numItemsRequired != 0 && product.Items.Count != numItemsRequired)
		{
			return false;
		}
		foreach (Network.BundleItem item in product.Items)
		{
			if (item.ItemType == itemType && (productData == 0 || item.ProductData == productData))
			{
				return true;
			}
		}
		return false;
	}

	public bool IsBundleValid(ProductInfo bundle)
	{
		if (!bundle.StringLogSetStatus())
		{
			return false;
		}
		if (!bundle.IsValid())
		{
			string productName = bundle.Title;
			Log.Store.Print((!string.IsNullOrWhiteSpace(productName)) ? ("StoreManager::IsBundleValid - Bundle Is Not Valid (" + productName + ".") : "StoreManager::IsBundleValid - Bundle has no Product ID.");
			return false;
		}
		if (bundle.IsGoldOnly() || bundle.IsFree())
		{
			return true;
		}
		if (!bundle.HasValidPrices())
		{
			ProductIssues.LogError(bundle.Id, "Missing price from Commerce service.");
			return false;
		}
		return true;
	}

	public void ResetProducts()
	{
		m_hasSetPages = false;
		m_hasValidatedProducts = false;
		ProductMap?.Clear();
		ProductCatalog?.Clear();
	}

	public void SetPersonalizedShopData(IList<Placement> placements, RpcError error)
	{
		Log.Store.PrintDebug("[ProductDataService.SetPersonalizedShopDat] Received responses");
		if (placements == null || placements.Count == 0)
		{
			string placementIdsLogString = string.Join(", ", ShopPlacementIds);
			Log.Store.PrintError("No page data was found for page ids \"{0}\"", placementIdsLogString);
			if (error != null && !string.IsNullOrEmpty(error.code))
			{
				Log.Store.PrintError("[ProductDataService.SetPersonalizedShopData] GetCurrentPagesResponse Error: code:'" + error.code + "', message:'" + error.message + "'");
			}
			return;
		}
		StringBuilder logString = new StringBuilder(32);
		foreach (Placement placement in placements)
		{
			string placementId = placement.placementId;
			if (!m_requestedShopPlacementIds.Contains(placementId))
			{
				Log.Store.PrintError("GetCurrentPageResponse Error: Shop type {0} has no pending request. Placement IDs = {1}", placementId, string.Join(",", placements.Select((Placement x) => x.placementId)));
				continue;
			}
			m_requestedShopPlacementIds.Remove(placementId);
			if (!m_receivedShopPlacementIds.Add(placementId))
			{
				Log.Store.PrintError("GetCurrentPageResponse Error: Shop type {0} received page response more than once. Placement IDs = {1}", placementId, string.Join(",", placements.Select((Placement x) => x.placementId)));
			}
			Log.Store.PrintDebug("Section Data (page Ids {0}):", string.Join(",", placements.Select((Placement x) => x.page.pageId)));
			CatalogNetworkPage netPage = CatalogNetworkPages.GetOrCreatePage(placement);
			netPage.Clear();
			foreach (Section responseSection in placement.page.sections)
			{
				logString.Clear();
				logString.Append($"section {netPage.Sections.Count + 1}: {responseSection.name}");
				Network.ShopSection netSection = new Network.ShopSection
				{
					InternalName = responseSection.name
				};
				if (responseSection.localization != null)
				{
					netSection.Label = new DbfLocValue();
					netSection.Label.SetString(responseSection.localization.name);
				}
				if (responseSection.orderInPage >= 0)
				{
					netSection.SortOrder = responseSection.orderInPage;
					Network.ShopSection sameOrderNetSection = netPage.GetSectionBySortOrder(netSection.SortOrder);
					if (sameOrderNetSection != null)
					{
						Log.Store.PrintError("section {0} has the same SortOrder as {1}: {2}. Order may be inconsistent", responseSection.name, sameOrderNetSection.InternalName, netSection.SortOrder);
					}
					logString.Append($"\n  sortOrder={netSection.SortOrder}");
				}
				else
				{
					Log.Store.PrintError("section {0} missing OrderInPage", responseSection.name);
				}
				netSection.ProductOrder = new List<Tuple<long, uint>>();
				foreach (ProductCollection productCollection in responseSection.productCollections)
				{
					foreach (ProductCollectionItem item in productCollection.items)
					{
						long pmtId = item.productCollectionItemValue;
						uint order = item.orderInProductCollection;
						UpdateProductClaimToken(pmtId, item.productClaimsToken);
						netSection.ProductOrder.Add(new Tuple<long, uint>(pmtId, order));
						logString.Append($"\n   [{pmtId}]={order}");
					}
				}
				netSection.Attributes = CommerceUtils.ConvertAttributes(responseSection.attributes);
				netPage.AddSection(netSection);
				Log.Store.PrintDebug(logString.ToString());
			}
		}
		m_hasSetPages = true;
		if (m_requestedShopPlacementIds.IsSubsetOf(m_receivedShopPlacementIds))
		{
			List<uint> productIds = ProductCatalog.ToList().ConvertAll((long id) => (uint)id);
			Processor.QueueJob("Load_Products", ServiceManager.Get<HearthstoneCheckout>().LoadProducts(productIds), JobFlags.StartImmediately);
		}
	}

	public void RefreshShopPageRequests()
	{
		List<string> shopIdsToRequest = new List<string>(ShopPlacementIds);
		m_receivedShopPlacementIds.Clear();
		m_requestedShopPlacementIds.Clear();
		m_requestedShopPlacementIds.UnionWith(shopIdsToRequest);
		if (shopIdsToRequest.Count == 0)
		{
			Log.Store.PrintDebug("Ignoring RefreshShopPageRequests request as no valid/new shop types requested...");
			return;
		}
		Logger store = Log.Store;
		object[] args = shopIdsToRequest.ToArray();
		store.Print(LogLevel.Debug, verbose: true, "[ProductDataService.RefreshShopPageRequests] Attempting to load the following pages: {0}, {1}, {2}", args);
		ServiceManager.Get<HearthstoneCheckout>().LoadPersonalizedShopData(shopIdsToRequest, SetPersonalizedShopData);
	}

	public bool SetProduct(Network.Bundle bundle)
	{
		if (bundle.PMTProductID.HasValue && ProductId.IsValid(bundle.PMTProductID.Value))
		{
			ProductId id = ProductId.CreateFrom(bundle.PMTProductID.Value);
			if (!ProductCatalog.Contains(bundle.PMTProductID.Value))
			{
				ProductCatalog.Add(bundle.PMTProductID.Value);
			}
			if (ProductMap.TryGetValue(id, out var existingProductInfo))
			{
				existingProductInfo.SetBundle(bundle);
			}
			else
			{
				ProductMap.Add(id, new ProductInfo(bundle));
			}
			return true;
		}
		return false;
	}

	public bool SetProduct(Product curProduct)
	{
		if (curProduct == null)
		{
			return false;
		}
		ProductId productId = ProductId.CreateFrom(curProduct.productId);
		if (!productId.IsValid())
		{
			return false;
		}
		if (!ProductCatalog.Contains(productId.Value))
		{
			ProductCatalog.Add(productId.Value);
		}
		if (ProductMap.TryGetValue(productId, out var existingProductInfo))
		{
			existingProductInfo.SetProduct(curProduct);
		}
		else
		{
			ProductMap.Add(productId, new ProductInfo(curProduct));
		}
		PlatformId.CreateFrom((int)curProduct.externalPlatformSetting.externalPlatformId).Match(delegate(PlatformId platformId)
		{
			ExternalIds[(productId, platformId)] = curProduct.externalPlatformSetting.externalPlatformProductId;
		});
		return true;
	}

	public void UpdateProductClaimToken(long pmtId, string productClaimToken)
	{
		ProductId productId = ProductId.CreateFrom(pmtId);
		ProductInfo existingProductInfo;
		if (!productId.IsValid())
		{
			Log.Store.PrintError("UpdateProductClaimToken Error: Unable to update product claim token due to invalid product id.");
		}
		else if (ProductMap.TryGetValue(productId, out existingProductInfo))
		{
			existingProductInfo.ProductClaimToken = productClaimToken;
		}
		else
		{
			ProductMap.Add(productId, new ProductInfo(productId, productClaimToken));
		}
	}

	public ProductDataModel Debug_GetProduct(ProductId productId, out string error)
	{
		return Catalog.Debug_GetProduct(productId, out error);
	}

	public IEnumerable<ProductDataModel> Debug_GetProductsWithTag(string tag, out string error)
	{
		return Catalog.Debug_TryGetProductsWithTag(tag, out error);
	}

	public List<ProductTierDataModel> GetProductTierDataModels()
	{
		return Catalog.GetTiers_All();
	}

	public List<ProductTierDataModel> GetProductTierDataModels(string parentTabId, string subTabId)
	{
		return Catalog.GetTiers_SubTab(parentTabId, subTabId);
	}

	public List<ShopTabData> GetTabs()
	{
		return Catalog.GetTabs_All();
	}

	public bool TryGetSubTabs(string parentTabId, out List<ShopTabData> subTabs)
	{
		return Catalog.TryGetSubTabs_All(parentTabId, out subTabs);
	}

	public bool TryFindTabWithId(string targetTabId, out ShopTabData parentTab, out ShopTabData subTab)
	{
		parentTab = null;
		subTab = null;
		if (string.IsNullOrEmpty(targetTabId) || Catalog == null)
		{
			return false;
		}
		if (Catalog.TryGetTab(targetTabId, out parentTab))
		{
			return true;
		}
		List<ShopTabData> allParentTabs = GetTabs();
		if (allParentTabs == null)
		{
			return false;
		}
		foreach (ShopTabData pTab in allParentTabs)
		{
			if (Catalog.TryGetSubTab(pTab.Id, targetTabId, out subTab))
			{
				parentTab = pTab;
				return true;
			}
		}
		return false;
	}

	public bool TryGetSubTabFromProductId(long productId, out ShopTabData parentTab, out ShopTabData subTab)
	{
		parentTab = null;
		subTab = null;
		if (Catalog == null)
		{
			return false;
		}
		List<ShopTabData> parentTabs = Catalog.GetTabs_All();
		if (parentTabs == null)
		{
			return false;
		}
		foreach (ShopTabData pTab in parentTabs)
		{
			string pTabId = pTab.Id;
			if (!Catalog.TryGetSubTabs_All(pTabId, out var subTabs))
			{
				continue;
			}
			foreach (ShopTabData sTab in subTabs)
			{
				List<ProductTierDataModel> tierData = Catalog.GetTiers_SubTab(pTabId, sTab.Id);
				if (DoesTierDataContainProductId(productId, tierData))
				{
					parentTab = pTab;
					subTab = sTab;
					return true;
				}
			}
		}
		return false;
	}

	private bool DoesTierDataContainProductId(long productId, List<ProductTierDataModel> tiers)
	{
		if (!ProductId.IsValid(productId) || tiers == null || tiers.Count == 0)
		{
			return false;
		}
		foreach (ProductTierDataModel data in tiers)
		{
			if (data.BrowserButtons == null)
			{
				continue;
			}
			foreach (ShopBrowserButtonDataModel browserButton in data.BrowserButtons)
			{
				if (browserButton.DisplayProduct.PmtId == productId)
				{
					return true;
				}
			}
		}
		return false;
	}

	public IEnumerable<string> GetUndisplayedProducts(ShopTabDataModel tab, ShopTabDataModel subTab)
	{
		if (!HasStoreLoaded())
		{
			yield break;
		}
		List<ProductTierDataModel> tiers = GetProductTierDataModels(tab.Id, subTab.Id);
		IEnumerable<string> cachedDisplayedProducts = GetLatestDisplayedProducts();
		foreach (ProductTierDataModel tier in tiers)
		{
			if (!ShopUtils.ShouldDisplayTier(tier))
			{
				continue;
			}
			for (int i = 0; i < tier.BrowserButtons.Count; i++)
			{
				string productId = tier.BrowserButtons[i].DisplayProduct.PmtId.ToString();
				if (!cachedDisplayedProducts.Contains(productId))
				{
					yield return productId;
				}
			}
		}
	}

	public bool HasProductBeenSeen(long pmtId)
	{
		if (m_shopBadging == null)
		{
			return false;
		}
		return m_shopBadging.HasProductBeenSeen(pmtId);
	}

	private HashSet<string> GetLatestDisplayedProducts()
	{
		if (m_shopBadging == null)
		{
			return new HashSet<string>();
		}
		return m_shopBadging.ListOfDisplayedProductIds;
	}

	public ProductDataModel GetProductDataModel(long productId)
	{
		if (!HasStoreLoaded())
		{
			return null;
		}
		return Catalog.GetProductByPmtId(ProductId.CreateFrom(productId));
	}

	public long GetDeeplinkProductId(ProductType productType, int productData)
	{
		IEnumerable<ProductInfo> match = from product in GetAllProductsContainingItem(productType, productData)
			where Catalog.GetTiers_All().Any((ProductTierDataModel tier) => tier.BrowserButtons.Any((ShopBrowserButtonDataModel button) => button.DisplayProduct.PmtId == product.Id.Value))
			select product;
		if (!match.Any())
		{
			return 0L;
		}
		return match.First().Id.Value;
	}

	public ProductDataModel GetBaseProductFromPmtProductId(long pmtProductId)
	{
		ProductDataModel baseProduct = null;
		foreach (ProductTierDataModel item in Catalog.GetTiers_All())
		{
			baseProduct = item.BrowserButtons.FirstOrDefault((ShopBrowserButtonDataModel b) => b.DisplayProduct.Variants.Any((ProductDataModel v) => v.PmtId == pmtProductId))?.DisplayProduct;
			if (baseProduct != null)
			{
				break;
			}
		}
		if (baseProduct == null)
		{
			foreach (ProductDataModel currencyProduct in new List<ProductDataModel> { VirtualCurrencyProductItem, BoosterCurrencyProductItem })
			{
				if (currencyProduct != null && currencyProduct.Variants.Any((ProductDataModel v) => v.PmtId == pmtProductId))
				{
					baseProduct = currencyProduct;
					break;
				}
			}
		}
		return baseProduct;
	}

	public KeyValuePair<int, string> GetSectionData(ProductTierDataModel productDataModel)
	{
		string name = null;
		int sectionIndex = -1;
		foreach (ShopTabData tabData in Catalog.GetTabs_All())
		{
			sectionIndex = Catalog.GetTiers_Tab(tabData.Id).IndexOf(productDataModel);
			if (sectionIndex >= 0)
			{
				break;
			}
		}
		Network.ShopSection netSection = Catalog.GetNetworkSection(productDataModel);
		if (netSection != null)
		{
			name = netSection.InternalName;
		}
		return new KeyValuePair<int, string>(sectionIndex, name);
	}

	public bool TryGetPmtIdWithTagContainingType(string identifyingTag, RewardItemType rewardItemType, out long pmtId)
	{
		return Catalog.TryGetPmtIdWithTagContainingType(identifyingTag, rewardItemType, out pmtId);
	}

	public bool GetCurrentCatalogSnapshot(ref ShopProductData shopData, ref long fakeLicenseId)
	{
		List<ShopProductData.ProductItemData> itemCatalog = new List<ShopProductData.ProductItemData>();
		List<ShopProductData.ProductData> productCatalog = new List<ShopProductData.ProductData>();
		fakeLicenseId = 404000L;
		List<ShopProductData.ProductTierData> tierList = new List<ShopProductData.ProductTierData>();
		foreach (ProductTierDataModel sourceTier in Catalog.GetTiers_All())
		{
			List<long> productIds = new List<long>();
			foreach (ShopBrowserButtonDataModel sourceButton in sourceTier.BrowserButtons)
			{
				productIds.Add(sourceButton.DisplayProduct.PmtId);
			}
			ShopProductData.ProductTierData productTierData = default(ShopProductData.ProductTierData);
			productTierData.tierId = sourceTier.GetDebugName();
			productTierData.tags = string.Join(",", sourceTier.Tags.ToArray());
			productTierData.header = sourceTier.Header;
			productTierData.productIds = productIds.ToArray();
			ShopProductData.ProductTierData capturedTier = productTierData;
			tierList.Add(capturedTier);
		}
		foreach (ProductDataModel sourceProduct in Catalog.Products)
		{
			ShopProductData.ProductData productData = default(ShopProductData.ProductData);
			productData.name = sourceProduct.Name;
			productData.description = sourceProduct.Description;
			productData.productId = sourceProduct.PmtId;
			productData.tags = string.Join(",", sourceProduct.Tags.ToArray());
			ShopProductData.ProductData capturedProduct = productData;
			List<ShopProductData.PriceData> priceList = new List<ShopProductData.PriceData>();
			foreach (PriceDataModel sourcePrice in sourceProduct.Prices)
			{
				ShopProductData.PriceData priceData = default(ShopProductData.PriceData);
				priceData.amount = sourcePrice.Amount;
				priceData.currencyType = sourcePrice.Currency;
				ShopProductData.PriceData capturedPrice = priceData;
				priceList.Add(capturedPrice);
			}
			capturedProduct.prices = priceList.ToArray();
			List<long> licenseIds = new List<long>();
			foreach (RewardItemDataModel sourceItem in sourceProduct.Items)
			{
				ShopProductData.ProductItemData productItemData = default(ShopProductData.ProductItemData);
				productItemData.itemId = sourceItem.ItemId;
				productItemData.itemType = sourceItem.ItemType;
				productItemData.licenseId = ((sourceItem.PmtLicenseId == 0L) ? fakeLicenseId++ : sourceItem.PmtLicenseId);
				productItemData.quantity = sourceItem.Quantity;
				ShopProductData.ProductItemData capturedItem = productItemData;
				ShopProductDataConverter.FillInDebugItemName(ref capturedItem);
				itemCatalog.Add(capturedItem);
				licenseIds.Add(capturedItem.licenseId);
			}
			capturedProduct.licenseIds = licenseIds.ToArray();
			productCatalog.Add(capturedProduct);
		}
		shopData.productTierCatalog = tierList.ToArray();
		shopData.productCatalog = productCatalog.ToArray();
		shopData.productItemCatalog = itemCatalog.ToArray();
		return true;
	}

	public bool TryGetProduct(long? productId, out ProductInfo product)
	{
		if (!productId.HasValue)
		{
			product = null;
			return false;
		}
		return TryGetProduct(productId.Value, out product);
	}

	public bool TryGetProduct(long productId, out ProductInfo product)
	{
		if (!ProductId.IsValid(productId))
		{
			product = null;
			return false;
		}
		return TryGetProduct(ProductId.CreateFrom(productId), out product);
	}

	public bool TryGetProduct(ProductId productId, out ProductInfo product)
	{
		if (ProductMap.ContainsKey(productId))
		{
			product = ProductMap[productId];
			return true;
		}
		product = null;
		return false;
	}

	public IEnumerable<ProductInfo> EnumerateBundlesForProductType(ProductType product, bool requireRealMoneyOption, int productData = 0, int numItemsRequired = 0, bool checkAvailability = true)
	{
		foreach (ProductInfo bundle in ProductMap.Values)
		{
			if ((!requireRealMoneyOption || !bundle.HasCurrency(CurrencyType.REAL_MONEY)) && bundle.DoesBundleContainProduct(product, productData, numItemsRequired) && (!checkAvailability || bundle.IsBundleAvailableNow(StoreManager.Get())))
			{
				yield return bundle;
			}
		}
	}

	public List<ProductInfo> GetAllBundlesForProduct(ProductType product, bool requireRealMoneyOption, int productData = 0, int numItemsRequired = 0, bool checkAvailability = true)
	{
		return EnumerateBundlesForProductType(product, requireRealMoneyOption, productData, numItemsRequired, checkAvailability).ToList();
	}

	public bool TierHasShowIfAllOwnedTag(ProductTierDataModel tier)
	{
		return Catalog.GetNetworkSection(tier)?.Attributes.GetTags().Contains("show_if_all_owned") ?? false;
	}

	public bool HasStoreLoaded()
	{
		if (Catalog != null && Catalog.HasData && ProductMap.Any() && m_hasSetPages)
		{
			return m_hasValidatedProducts;
		}
		return false;
	}

	public bool HasLoadedTiers()
	{
		if (ShopPlacementIds.Any())
		{
			return m_hasSetPages;
		}
		return false;
	}

	public bool HasPendingTierChanges()
	{
		return Catalog.TiersChangeCount > 0;
	}

	public bool HasReceivedAllShopTypeSections()
	{
		foreach (string requestShopPageId in m_requestedShopPlacementIds)
		{
			if (!CatalogNetworkPages.Contains(requestShopPageId))
			{
				return false;
			}
		}
		return true;
	}

	public bool HasProductsAvailable()
	{
		return ProductMap.Count > 0;
	}

	public void CreateDataModels()
	{
		Catalog.CreateDataModels();
	}

	public bool TryRefreshStaleProductAvailability()
	{
		return Catalog?.TryRefreshStaleProductAvailability() ?? false;
	}

	public void UpdateProductStatus()
	{
		Catalog?.UpdateProductStatus();
	}

	public void MarkProductAsSeen(ProductDataModel product)
	{
		if (m_shopBadging != null)
		{
			m_shopBadging.MarkProductAsSeen(product);
			ClearShopTabBadgeIfAllProductsSeen(m_shopBadging.ListOfSeenProductIds);
		}
	}

	private void ClearShopTabBadgeIfAllProductsSeen(HashSet<string> productsSeen)
	{
		if (Shop.Get() == null || !Shop.Get().TryGetCurrentTabs(out var tab, out var subTab))
		{
			return;
		}
		foreach (string product in GetUnseenProducts(tab, subTab))
		{
			if (!productsSeen.Contains(product))
			{
				return;
			}
		}
		MarkTabAsDisplayed(tab, subTab);
	}

	private List<string> GetUnseenProducts(ShopTabDataModel tab, ShopTabDataModel subTab)
	{
		List<ProductTierDataModel> productTierDataModels = GetProductTierDataModels(tab.Id, subTab.Id);
		List<string> listOfUnseenProducts = new List<string>();
		foreach (ProductTierDataModel tier in productTierDataModels)
		{
			if (tier.Tags.Contains("expansion_row"))
			{
				continue;
			}
			for (int i = 0; i < tier.BrowserButtons.Count; i++)
			{
				ProductDataModel product = tier.BrowserButtons[i].DisplayProduct;
				if (product.Tags.Contains("new"))
				{
					listOfUnseenProducts.Add(product.PmtId.ToString());
				}
			}
		}
		return listOfUnseenProducts;
	}

	public void MarkTabAsDisplayed(ShopTabDataModel tab, ShopTabDataModel subTab)
	{
		if (m_shopBadging != null)
		{
			IEnumerable<string> productsToMarkAsSeen = GetUndisplayedProducts(tab, subTab);
			IEnumerable<string> combinedList = GetLatestDisplayedProducts().Concat(productsToMarkAsSeen);
			m_shopBadging.MarkTabAsDisplayed(combinedList);
			UpdateTabNotificationStates();
		}
	}

	public void MarkShopAsVisited()
	{
		foreach (ShopTabData tab in GetTabs())
		{
			if (!TryGetSubTabs(tab.Id, out var subTabs))
			{
				continue;
			}
			foreach (ShopTabData subTab in subTabs)
			{
				MarkTabAsDisplayed(tab.GetTabDataModel(), subTab.GetTabDataModel());
			}
		}
	}

	public void MarkAllProductsAsSeen()
	{
		if (m_shopBadging != null)
		{
			List<string> productsToCache = GetAvailableProductsInShop();
			m_shopBadging.MarkProductsAsSeen(productsToCache);
			ClearShopTabBadgeIfAllProductsSeen(m_shopBadging.ListOfSeenProductIds);
			Catalog.UpdateProductStatus();
		}
	}

	public void CacheAllProductsInShop()
	{
		if (m_shopBadging != null)
		{
			List<string> productsToCache = GetAvailableProductsInShop();
			m_shopBadging.CacheAllProductsInShop(productsToCache);
		}
	}

	public bool CheckForNewItems()
	{
		if (m_shopBadging == null)
		{
			Log.Store.Print("[ShopBadging::CheckForNewItems] Shop Badging is not available.");
			return false;
		}
		HashSet<string> allProductsInCache = m_shopBadging.ListOfAllProductsInShop;
		List<string> undisplayedProducts = new List<string>();
		foreach (ShopTabData tab in GetTabs())
		{
			if (tab.Locked || !TryGetSubTabs(tab.Id, out var subTabs))
			{
				continue;
			}
			foreach (ShopTabData subTab in subTabs)
			{
				if (!subTab.Locked)
				{
					undisplayedProducts.AddRange(GetUndisplayedProducts(tab.GetTabDataModel(), subTab.GetTabDataModel()));
				}
			}
		}
		Log.Store.Print("[ShopBadging::CheckForNewItems] Cached Products: " + string.Join(":", allProductsInCache));
		Log.Store.Print("[ShopBadging::CheckForNewItems] Undisplayed Products: " + string.Join(":", undisplayedProducts));
		foreach (string product in undisplayedProducts)
		{
			if (!allProductsInCache.Contains(product))
			{
				return true;
			}
		}
		return false;
	}

	public bool GetRefreshDataModel(ShopDataModel shopData)
	{
		shopData.Pages.Clear();
		List<ProductTierDataModel> tiersToAdd = new List<ProductTierDataModel>();
		List<ShopTabData> tabs = GetTabs();
		tabs.Sort((ShopTabData x, ShopTabData y) => x.Order.CompareTo(y.Order));
		foreach (ShopTabData tab in tabs)
		{
			if (!tab.Enabled)
			{
				continue;
			}
			ShopPageDataModel shopPageDataModel = new ShopPageDataModel
			{
				Tab = tab.GetTabDataModel(),
				ShopSubPages = new DataModelList<ShopSubPageDataModel>()
			};
			shopData.Pages.Add(shopPageDataModel);
			string tabId = tab.Id;
			if (!TryGetSubTabs(tabId, out var subTabs) || subTabs.Count == 0)
			{
				continue;
			}
			subTabs.Sort(delegate(ShopTabData x, ShopTabData y)
			{
				int num = x.Locked.CompareTo(y.Locked);
				return (num != 0) ? num : x.Order.CompareTo(y.Order);
			});
			foreach (ShopTabData subTab in subTabs)
			{
				if (!subTab.Enabled)
				{
					continue;
				}
				tiersToAdd.Clear();
				string subTabId = subTab.Id;
				List<ProductTierDataModel> tiers = GetProductTierDataModels(tabId, subTabId);
				if (tiers == null || tiers.Count == 0)
				{
					continue;
				}
				foreach (ProductTierDataModel tier in tiers)
				{
					if (ShopUtils.ShouldDisplayTier(tier))
					{
						tiersToAdd.Add(tier);
					}
				}
				if (tiersToAdd.Count > 0)
				{
					ShopSubPageDataModel shopSubPageDataModel = new ShopSubPageDataModel();
					shopPageDataModel.ShopSubPages.Add(shopSubPageDataModel);
					shopSubPageDataModel.Tab = subTab.GetTabDataModel();
					shopSubPageDataModel.Tiers = new DataModelList<ProductTierDataModel>();
					shopSubPageDataModel.Tiers.AddRange(tiersToAdd);
				}
			}
		}
		shopData.VirtualCurrency = VirtualCurrencyProductItem ?? ProductFactory.CreateEmptyProductDataModel();
		shopData.BoosterCurrency = BoosterCurrencyProductItem ?? ProductFactory.CreateEmptyProductDataModel();
		shopData.AutoconvertCurrency = Options.Get().GetBool(Option.AUTOCONVERT_VIRTUAL_CURRENCY);
		shopData.DebugShowProductIds = Options.Get().GetBool(Option.DEBUG_SHOW_PRODUCT_IDS);
		shopData.IsWild = CollectionManager.Get() != null && CollectionManager.Get().ShouldAccountSeeStandardWild();
		shopData.TavernTicketBalance = NetCache.Get().GetArenaTicketBalance();
		m_tiersChangeCountAtLastRefresh = Catalog.TiersChangeCount;
		if (ServiceManager.TryGet<HearthstoneCheckout>(out var checkout))
		{
			shopData.AllowRealMoneyPurchases = checkout.CanMakeRealMoneyPurchase;
		}
		else
		{
			shopData.AllowRealMoneyPurchases = false;
			shopData.DisableRealMoneyPurchaseMessage = string.Empty;
		}
		UpdateTabNotificationStates();
		return true;
	}

	public void UpdateTabNotificationStates()
	{
		GlobalDataContext.Get().GetDataModel(24, out var dataModel);
		ShopDataModel shopData = (ShopDataModel)dataModel;
		List<ShopTabData> tabs = GetTabs();
		HashSet<string> tabsWithUndisplayedProducts = new HashSet<string>();
		shopData?.TabsWithUndisplayedProducts.Clear();
		foreach (ShopTabData tab in tabs)
		{
			tab.HasUndisplayedProducts = false;
			tab.NotificationEnabled = tab.Name == "Hearthstone" || tab.Name == "Battlegrounds" || tab.Name == "Featured";
			if (!TryGetSubTabs(tab.Id, out var subTabs))
			{
				continue;
			}
			foreach (ShopTabData subTab in subTabs)
			{
				subTab.HasUndisplayedProducts = false;
				if (tab.NotificationEnabled && GetUndisplayedProducts(tab.GetTabDataModel(), subTab.GetTabDataModel()).Count() > 0)
				{
					tabsWithUndisplayedProducts.Add(tab.Id);
					tabsWithUndisplayedProducts.Add(subTab.Id);
					subTab.HasUndisplayedProducts = true;
					tab.HasUndisplayedProducts = true;
				}
			}
		}
		foreach (string id in tabsWithUndisplayedProducts)
		{
			shopData?.TabsWithUndisplayedProducts.Add(id);
		}
	}

	public void ClearLatestDisplayedProducts()
	{
		if (m_shopBadging != null)
		{
			m_shopBadging.ClearLatestDisplayedProducts();
		}
		UpdateTabNotificationStates();
	}

	public void ClearSeenProducts()
	{
		if (m_shopBadging != null)
		{
			m_shopBadging.ClearSeenProducts();
		}
	}

	public IList<ProductInfo> ReconcileHearthstoneServerAndCommerceSdkProducts()
	{
		List<ProductId> list = (from kvp in ProductMap
			where !IsBundleValid(kvp.Value)
			select kvp.Key).ToList();
		IList<ProductInfo> removedProducts = new List<ProductInfo>();
		foreach (ProductId key in list)
		{
			ProductInfo product = ProductMap[key];
			if (ProductMap.Remove(key))
			{
				removedProducts.Add(product);
			}
		}
		m_hasValidatedProducts = true;
		return removedProducts;
	}

	private List<string> GetAvailableProductsInShop()
	{
		HashSet<string> productsToCache = new HashSet<string>();
		foreach (ShopTabData tab in GetTabs())
		{
			if (tab.Locked || !TryGetSubTabs(tab.Id, out var subTabs))
			{
				continue;
			}
			foreach (ShopTabData subTab in subTabs)
			{
				if (subTab.Locked)
				{
					continue;
				}
				foreach (ProductTierDataModel tier in GetProductTierDataModels(tab.Id, subTab.Id))
				{
					for (int i = 0; i < tier.BrowserButtons.Count; i++)
					{
						string productId = tier.BrowserButtons[i].DisplayProduct.PmtId.ToString();
						productsToCache.Add(productId);
					}
				}
			}
		}
		return productsToCache.ToList();
	}
}
