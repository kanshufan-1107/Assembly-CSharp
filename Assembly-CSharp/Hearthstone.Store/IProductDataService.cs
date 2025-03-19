using System.Collections.Generic;
using Blizzard.Commerce;
using Blizzard.T5.Services;
using com.blizzard.commerce.Model;
using Hearthstone.DataModels;
using PegasusUtil;

namespace Hearthstone.Store;

public interface IProductDataService : IService
{
	HashSet<string> ShopPlacementIds { get; }

	HashSet<long> ProductCatalog { get; }

	ProductDataModel VirtualCurrencyProductItem { get; }

	ProductDataModel BoosterCurrencyProductItem { get; }

	bool HasTierChangedSinceLastDataModelRetrievel { get; }

	void ResetProducts();

	void SetPersonalizedShopData(IList<Placement> placements, RpcError error);

	void RefreshShopPageRequests();

	void CreateDataModels();

	ProductDataModel Debug_GetProduct(ProductId productId, out string error);

	IEnumerable<ProductDataModel> Debug_GetProductsWithTag(string tag, out string error);

	bool SetProduct(Network.Bundle bundle);

	bool SetProduct(Product bundle);

	List<ProductTierDataModel> GetProductTierDataModels();

	bool TryGetSubTabs(string parentTabId, out List<ShopTabData> subTabs);

	bool TryFindTabWithId(string targetTabId, out ShopTabData parentTab, out ShopTabData subTab);

	bool TryGetSubTabFromProductId(long productId, out ShopTabData parentTab, out ShopTabData subTab);

	ProductDataModel GetProductDataModel(long productId);

	bool TryGetPmtIdWithTagContainingType(string identifyingTag, RewardItemType rewardItemType, out long pmtId);

	long GetDeeplinkProductId(ProductType productType, int productData);

	ProductDataModel GetBaseProductFromPmtProductId(long pmtId);

	KeyValuePair<int, string> GetSectionData(ProductTierDataModel productDataModel);

	bool GetCurrentCatalogSnapshot(ref ShopProductData shopData, ref long fakeLicenseId);

	bool TryGetProduct(long? productId, out ProductInfo product);

	bool TryGetProduct(long productId, out ProductInfo product);

	bool TryGetProduct(ProductId productId, out ProductInfo product);

	IEnumerable<ProductInfo> EnumerateBundlesForProductType(ProductType product, bool requireRealMoneyOption, int productData = 0, int numItemsRequired = 0, bool checkAvailability = true);

	List<ProductInfo> GetAllBundlesForProduct(ProductType product, bool requireRealMoneyOption, int productData = 0, int numItemsRequired = 0, bool checkAvailability = true);

	bool TierHasShowIfAllOwnedTag(ProductTierDataModel tier);

	bool HasStoreLoaded();

	bool HasPendingTierChanges();

	bool HasReceivedAllShopTypeSections();

	bool HasLoadedTiers();

	bool HasProductsAvailable();

	bool TryRefreshStaleProductAvailability();

	void UpdateProductStatus();

	void MarkProductAsSeen(ProductDataModel product);

	void MarkTabAsDisplayed(ShopTabDataModel tab, ShopTabDataModel subTab);

	void MarkShopAsVisited();

	void MarkAllProductsAsSeen();

	void CacheAllProductsInShop();

	bool CheckForNewItems();

	bool GetRefreshDataModel(ShopDataModel shopData);

	void ClearLatestDisplayedProducts();

	void ClearSeenProducts();

	IList<ProductInfo> ReconcileHearthstoneServerAndCommerceSdkProducts();
}
