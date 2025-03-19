using System;
using System.Collections.Generic;
using System.Linq;
using Blizzard.Commerce;
using Blizzard.GameService.SDK.Client.Integration;
using Blizzard.T5.Services;
using Game.Shop;
using Hearthstone.Commerce;
using Hearthstone.DataModels;
using Hearthstone.InGameMessage;
using Hearthstone.Store;
using Hearthstone.UI;

public class ProductCatalog
{
	private class VariantSortOrder : VariantUtils.ISortOrder
	{
		public int Grouped(ProductDataModel product)
		{
			return RewardUtils.GetSortOrderFromItems(product.Items);
		}

		public int Ungrouped(ProductDataModel product)
		{
			DataModelList<RewardItemDataModel> items = product.Items;
			int result = 0;
			if (items.Count > 0)
			{
				result = items[0].Quantity;
				if (product.Tags.Contains("mini_set"))
				{
					result = ((!product.Tags.Contains("golden")) ? 1 : 0);
				}
				else if (product.Tags.Contains("sellable_deck"))
				{
					bool hasClassVariantTag = false;
					foreach (ProductDataModel variant in product.Variants)
					{
						if (variant.Tags.Contains("show_class_variants"))
						{
							hasClassVariantTag = true;
							break;
						}
					}
					result = (hasClassVariantTag ? ((!product.Tags.Contains("show_class_variants")) ? items[0].Quantity : 0) : ((!product.Tags.Contains("golden")) ? 1 : 0));
				}
			}
			return result;
		}
	}

	private readonly List<ProductDataModel> m_products = new List<ProductDataModel>();

	private readonly HashSet<int> m_goldBoosterProductIds = new HashSet<int>();

	private readonly CatalogTabs m_catalogTabs = new CatalogTabs();

	private readonly Dictionary<ProductTierDataModel, Network.ShopSection> m_tierSectionMapping = new Dictionary<ProductTierDataModel, Network.ShopSection>();

	private ProductDataModel m_virtualCurrencyProduct;

	private ProductDataModel m_boosterCurrencyProduct;

	private ProductDataModel m_tavernTicketProduct;

	private BoosterDbId m_latestBoosterId;

	private AdventureDbId m_latestAdventureId;

	private int m_rotationWarningThreshold;

	private DateTime? m_nextCatalogChangeTimeUtc;

	private long m_tiersChangeCount;

	private bool m_hasUpdatedProductStatusOnce;

	private readonly VariantUtils.ISortOrder m_sortOrder = new VariantSortOrder();

	private StoreManager m_storeManager;

	private ProductDataService m_productDataStorageService;

	private bool m_isProductDataDirty;

	private bool m_isMercShopAvailableCache;

	private bool m_isWildAvailableInShopCache;

	private bool m_isApprenticeshipCompleteCache;

	private static Comparison<Tuple<long, uint>> SortProducts = (Tuple<long, uint> a, Tuple<long, uint> b) => a.Item2.CompareTo(b.Item2);

	private static Comparison<Network.ShopSection> SortSections = (Network.ShopSection a, Network.ShopSection b) => a.SortOrder.CompareTo(b.SortOrder);

	public List<ProductDataModel> Products => m_products;

	public long TiersChangeCount => m_tiersChangeCount;

	public ProductDataModel VirtualCurrencyProductItem => m_virtualCurrencyProduct;

	public ProductDataModel BoosterCurrencyProductItem => m_boosterCurrencyProduct;

	public bool HasData { get; private set; }

	public void MarkProductDataAsDirty()
	{
		m_isProductDataDirty = true;
	}

	public ProductCatalog(StoreManager storeManager, ProductDataService dataService)
	{
		if (storeManager == null)
		{
			throw new ArgumentNullException("Store manager cannot be null");
		}
		m_storeManager = storeManager;
		m_productDataStorageService = dataService;
	}

	public Network.ShopSection GetNetworkSection(ProductTierDataModel tier)
	{
		m_tierSectionMapping.TryGetValue(tier, out var section);
		return section;
	}

	public void PopulateWithNetData(List<ProductInfo> netBundles, List<Network.GoldCostBooster> netGoldBoosters, CatalogNetworkPages networkPages)
	{
		HasData = true;
		m_products.Capacity = Math.Max(m_products.Capacity, netBundles.Count() + netGoldBoosters.Count());
		AddNetGoldBoosterProducts(netGoldBoosters);
		AddNetBundleProducts(netBundles);
		UpdateProductStatus();
		PopulateTiers(networkPages);
		PopulateProductVariants();
		HealupProductTiers();
	}

	public ProductDataModel GetProductByPmtId(ProductId productId)
	{
		int i = 0;
		for (int iMax = m_products.Count; i < iMax; i++)
		{
			if (m_products[i].PmtId == productId.Value)
			{
				return m_products[i];
			}
		}
		return null;
	}

	public List<ProductTierDataModel> GetTiers_All()
	{
		return m_catalogTabs.GetTiers();
	}

	public List<ProductTierDataModel> GetTiers_Tab(string tabId)
	{
		return m_catalogTabs.GetTiers(tabId);
	}

	public List<ProductTierDataModel> GetTiers_SubTab(string parentTabId, string subTabId)
	{
		return m_catalogTabs.GetTiers(parentTabId, subTabId);
	}

	public List<ShopTabData> GetTabs_All()
	{
		return m_catalogTabs.GetAllTabData();
	}

	public bool TryGetTab(string tabId, out ShopTabData tabData)
	{
		return m_catalogTabs.TryGetTabData(tabId, out tabData);
	}

	public bool TryGetSubTabs_All(string parentTabId, out List<ShopTabData> subTabs)
	{
		return m_catalogTabs.TryGetAllSubTabData(parentTabId, out subTabs);
	}

	public bool TryGetSubTab(string parentTabId, string subTabId, out ShopTabData tabData)
	{
		return m_catalogTabs.TryGetSubTabData(parentTabId, subTabId, out tabData);
	}

	public void UpdateProductStatus()
	{
		Log.Store.PrintDebug($"Updating Product Status at {DateTime.Now:g}");
		if (!CatalogUtils.CanUpdateProductStatus(out var cannotUpdateReason))
		{
			Log.Store.PrintWarning(cannotUpdateReason);
			return;
		}
		m_hasUpdatedProductStatusOnce = true;
		m_latestBoosterId = GameUtils.GetLatestRewardableBooster();
		m_latestAdventureId = GameUtils.GetLatestActiveAdventure();
		bool canSeeWild = CollectionManager.Get() != null && CollectionManager.Get().ShouldAccountSeeStandardWild();
		bool hasCompletedApprenticeship = GameUtils.HasCompletedApprentice();
		UpdateWarningThreshold(m_productDataStorageService.CatalogNetworkPages);
		IGamemodeAvailabilityService gamemodeAvailabilityService = ServiceManager.Get<IGamemodeAvailabilityService>();
		foreach (ProductDataModel product in m_products)
		{
			if (product.PmtId == 0L)
			{
				product.Availability = ProductAvailability.UNDEFINED;
				if (product.Prices.Count == 1 && product.GetBuyProductArgs(product.Prices[0], 1) is BuyNoGTAPPEventArgs eventArgs && m_storeManager.GetGoldCostNoGTAPP(eventArgs.transactionData, out var _))
				{
					product.Availability = ProductAvailability.CAN_PURCHASE;
				}
			}
			else
			{
				if (m_productDataStorageService.TryGetProduct(product.GetProductId(), out var netBundle))
				{
					product.Availability = m_storeManager.GetNetworkBundleProductAvailability(netBundle, canSeeWild, hasCompletedApprenticeship);
				}
				if (!product.Tags.Contains("booster_allow_no_gold") && product.Availability == ProductAvailability.CAN_PURCHASE && !m_storeManager.IgnoreProductTiming && product.GetPrimaryProductTag() == "booster")
				{
					BoosterDbId boosterId = product.GetProductBoosterId();
					if (boosterId != 0)
					{
						BoosterDbfRecord boosterRecord = GameDbf.Booster.GetRecord((int)boosterId);
						if (boosterRecord != null && boosterRecord.BuyWithGoldEvent != EventTimingType.UNKNOWN && !EventTimingManager.Get().IsEventActive(boosterRecord.BuyWithGoldEvent))
						{
							product.Availability = ProductAvailability.SALE_NOT_ACTIVE;
						}
					}
				}
			}
			if (gamemodeAvailabilityService != null)
			{
				foreach (IGamemodeAvailabilityService.Gamemode mode in product.RequiredGamemodes)
				{
					IGamemodeAvailabilityService.Status modeStatus = gamemodeAvailabilityService.GetGamemodeStatus(mode);
					if (modeStatus <= IGamemodeAvailabilityService.Status.NOT_DOWNLOADED)
					{
						product.Availability = ProductAvailability.ASSETS_NOT_DOWNLOADED;
						break;
					}
					if (modeStatus <= IGamemodeAvailabilityService.Status.TUTORIAL_INCOMPLETE && (mode != IGamemodeAvailabilityService.Gamemode.MERCENARIES || !MercenaryMessageUtils.IsMercenaryVillageShopAvailable()))
					{
						product.Availability = ProductAvailability.RESTRICTED;
						break;
					}
				}
			}
			UpdateProductFreshness(product);
		}
		BnetBar.Get().RefreshCurrency();
		UpdateNextCatalogChangeTime();
	}

	public bool TryRefreshStaleProductAvailability()
	{
		if (!CatalogUtils.CanUpdateProductStatus(out var _))
		{
			return false;
		}
		if (!m_isMercShopAvailableCache && ServiceManager.TryGet<IGamemodeAvailabilityService>(out var gas))
		{
			IGamemodeAvailabilityService.Status status = gas.GetGamemodeStatus(IGamemodeAvailabilityService.Gamemode.MERCENARIES);
			if (status > IGamemodeAvailabilityService.Status.NOT_DOWNLOADED && status < IGamemodeAvailabilityService.Status.READY)
			{
				if (MercenaryMessageUtils.IsMercenaryVillageShopAvailable())
				{
					m_isProductDataDirty = true;
					m_isMercShopAvailableCache = true;
				}
				else
				{
					m_isMercShopAvailableCache = false;
				}
			}
		}
		if (!m_isWildAvailableInShopCache && CollectionManager.Get() != null && CollectionManager.Get().ShouldAccountSeeStandardWild())
		{
			m_isWildAvailableInShopCache = true;
			m_isProductDataDirty = true;
		}
		if (!m_isApprenticeshipCompleteCache && GameUtils.HasCompletedApprentice())
		{
			m_isApprenticeshipCompleteCache = true;
			m_isProductDataDirty = true;
		}
		if (IsProductAvailabilityStale())
		{
			m_isProductDataDirty = false;
			m_nextCatalogChangeTimeUtc = null;
			UpdateProductStatus();
			PopulateTiers(m_productDataStorageService.CatalogNetworkPages);
			PopulateProductVariants();
			HealupProductTiers();
			return true;
		}
		return false;
	}

	public bool TryGetPmtIdWithTagContainingType(string tag, RewardItemType type, out long pmtId)
	{
		pmtId = 0L;
		IEnumerable<long> matchingIds = from product in m_products
			where product.Tags.Contains(tag)
			where product.Items.Any((RewardItemDataModel item) => item.ItemType == type)
			select product.PmtId;
		if (matchingIds.Count() == 0)
		{
			return false;
		}
		pmtId = matchingIds.First();
		return true;
	}

	public ProductDataModel Debug_GetProduct(ProductId pmtProductId, out string error)
	{
		IEnumerable<ProductDataModel> miniSetProducts = GetMiniSetProducts(m_products);
		ProductDataModel product = GetProductByPmtId(pmtProductId);
		if (product == null)
		{
			if (!m_productDataStorageService.TryGetProduct(pmtProductId, out var _))
			{
				error = $"Product {pmtProductId} not received from server.";
				return null;
			}
			error = $"Product {pmtProductId} failed client validation. See Store log.";
			return null;
		}
		PopulateVariantsForProduct(product, miniSetProducts);
		error = string.Empty;
		return product;
	}

	public IEnumerable<ProductDataModel> Debug_TryGetProductsWithTag(string tag, out string error)
	{
		IEnumerable<ProductDataModel> miniSetProducts = GetMiniSetProducts(m_products);
		IEnumerable<ProductDataModel> productsByTag = m_products.Where((ProductDataModel p) => p.Tags.Contains(tag));
		foreach (ProductDataModel product in productsByTag)
		{
			PopulateVariantsForProduct(product, miniSetProducts);
		}
		error = string.Empty;
		return productsByTag;
	}

	private void ClearTiers()
	{
		if (m_catalogTabs.HasTiers())
		{
			m_tiersChangeCount++;
			m_catalogTabs.Clear();
		}
		m_tierSectionMapping.Clear();
	}

	public void CreateDataModels()
	{
		PopulateWithNetData(m_productDataStorageService.ProductMap.Values.ToList(), m_storeManager.AllGoldCostBoosters.ToList(), m_productDataStorageService.CatalogNetworkPages);
		Log.Store.PrintDebug("ProductCatalog initial population complete");
	}

	private bool IsProductAvailabilityStale()
	{
		if (m_hasUpdatedProductStatusOnce && !m_isProductDataDirty)
		{
			if (m_nextCatalogChangeTimeUtc.HasValue)
			{
				return m_nextCatalogChangeTimeUtc.Value < DateTime.UtcNow;
			}
			return false;
		}
		return true;
	}

	private void AddNetGoldBoosterProducts(IEnumerable<Network.GoldCostBooster> netGoldBoosters)
	{
		foreach (Network.GoldCostBooster goldCostBooster in netGoldBoosters)
		{
			if (!m_goldBoosterProductIds.Contains(goldCostBooster.ID))
			{
				ProductDataModel product = CatalogUtils.NetGoldCostBoosterToProduct(goldCostBooster);
				if (product != null)
				{
					m_products.Add(product);
					m_goldBoosterProductIds.Add(goldCostBooster.ID);
				}
			}
		}
	}

	private void AddNetBundleProducts(IEnumerable<ProductInfo> netBundles)
	{
		foreach (ProductInfo netBundle in netBundles)
		{
			if (netBundle.Id.IsValid())
			{
				ProductId productId = netBundle.Id;
				if (GetProductByPmtId(productId) != null)
				{
					continue;
				}
			}
			ProductDataModel product = ProductFactory.CreateProductDataModel(netBundle);
			if (product != null)
			{
				m_products.Add(product);
			}
		}
	}

	private void PopulateTiersFromNetSections(CatalogNetworkPages networkPages)
	{
		ClearTiers();
		if (!HasData)
		{
			return;
		}
		List<string> fillerTags = new List<string>();
		bool isInCnRegion = BattleNet.GetCurrentRegion() == BnetRegion.REGION_CN;
		foreach (CatalogNetworkPage page in networkPages.Pages)
		{
			List<Network.ShopSection> sections = page.Sections;
			sections.Sort(SortSections);
			string currentTabId = null;
			string currentSubTabId = null;
			foreach (Network.ShopSection netSection in sections)
			{
				if (netSection.Attributes.GetValue("tab_id").TryGetValue(out var tabId))
				{
					if (string.IsNullOrEmpty(tabId))
					{
						Log.Store.PrintError("Cannot handle tab section - " + netSection.InternalName + " - Null/Empty tab ID");
						continue;
					}
					currentTabId = tabId;
					m_catalogTabs.AddTab(GetShopTabData(tabId, netSection));
					continue;
				}
				if (currentTabId == null)
				{
					Log.Store.PrintError("Cannot handle section - " + netSection.InternalName + " - No parent tab. Ensure the section above has tab_id");
					continue;
				}
				if (netSection.Attributes.GetValue("subtab_id").TryGetValue(out var subTabId))
				{
					if (string.IsNullOrEmpty(subTabId))
					{
						Log.Store.PrintError("Cannot handle sub tab section - " + netSection.InternalName + " - Null/Empty sub tab ID");
						continue;
					}
					currentSubTabId = subTabId;
					m_catalogTabs.AddSubTab(currentTabId, GetShopTabData(subTabId, netSection));
					continue;
				}
				if (currentSubTabId == null)
				{
					Log.Store.PrintError("Cannot handle section - " + netSection.InternalName + " - No parent sub tab. Ensure the section above has subtab_id");
					continue;
				}
				fillerTags.Clear();
				if (netSection.Attributes.GetValue("TreatTagsAsFiller").TryGetValue(out var fillerTagsStr) && !string.IsNullOrEmpty(fillerTagsStr))
				{
					fillerTags.AddRange(CatalogUtils.ParseTagsString(fillerTagsStr).Tags);
				}
				bool isPersonalizedOfferSection = false;
				if (isInCnRegion && netSection.Attributes.GetValue("ispersonalizedoffer").TryGetValue(out var isPersonalizedString))
				{
					bool.TryParse(isPersonalizedString, out isPersonalizedOfferSection);
				}
				bool showGoldPriceBooster = false;
				List<string> showGoldPriceBoosterPmtIds = null;
				if (ShopUtils.ShouldShowPriceStyles() && netSection.Attributes.GetValue("override_gold_price_booster").TryGetValue(out var pmtIds) && !string.IsNullOrEmpty(pmtIds))
				{
					if (pmtIds != "true")
					{
						showGoldPriceBoosterPmtIds = pmtIds.Split(",").ToList();
					}
					showGoldPriceBooster = pmtIds == "true" || showGoldPriceBoosterPmtIds.Count > 0;
				}
				List<ShopBrowserButtonDataModel> buttons = new List<ShopBrowserButtonDataModel>();
				List<Tuple<long, uint>> sortedProducts = netSection.ProductOrder;
				sortedProducts.Sort(SortProducts);
				int i = 0;
				for (int iMax = sortedProducts.Count; i < iMax; i++)
				{
					long pmtId = sortedProducts[i].Item1;
					if (!ProductId.IsValid(pmtId))
					{
						continue;
					}
					ProductId productId = ProductId.CreateFrom(pmtId);
					ProductDataModel productDataModel = GetProductByPmtId(productId);
					if (productDataModel == null)
					{
						ProductIssues.LogError(productId, "Referenced in section [" + netSection.InternalName + "] but client has no valid product data model.");
						continue;
					}
					switch (productDataModel.Availability)
					{
					case ProductAvailability.ALREADY_OWNED:
						if (productDataModel.Tags.Contains("hide_owned"))
						{
							ProductIssues.LogHidden(productDataModel, "Hidden due to hide_owned tag and status is ALREADY_OWNED");
							continue;
						}
						break;
					case ProductAvailability.SALE_NOT_ACTIVE:
					{
						if (m_productDataStorageService.TryGetProduct(productDataModel.PmtId, out var bundle))
						{
							ProductAvailabilityRange range = bundle.GetBundleAvailabilityRange(m_storeManager);
							string rangeAsString = ((range != null) ? range.ToString() : "<unknown sale>");
							ProductIssues.LogHidden(productDataModel, "Hidden because sale is not active. Range = " + rangeAsString + " (May be shifted by server cheats)");
						}
						continue;
					}
					default:
						ProductIssues.LogHidden(productDataModel, $"Hidden because status is {productDataModel.Availability}");
						continue;
					case ProductAvailability.CAN_PURCHASE:
						break;
					}
					if (showGoldPriceBooster && (showGoldPriceBoosterPmtIds == null || showGoldPriceBoosterPmtIds.Contains(productDataModel.PmtId.ToString())) && productDataModel.Tags.Contains("booster"))
					{
						BoosterDbId boosterDbId = productDataModel.GetProductBoosterId();
						if (boosterDbId != 0)
						{
							foreach (ProductDataModel product in m_products)
							{
								if (product.PmtId != 0L || boosterDbId != product.GetProductBoosterId())
								{
									continue;
								}
								DataModelList<string> tags = productDataModel.Tags;
								productDataModel = product;
								productDataModel.Tags.Add("show_price");
								productDataModel.Tags.Add("pricestyle_single");
								foreach (string tag in tags)
								{
									if (!(tag == "hide_price") && !CatalogTags.PRICESTYLES.Contains(tag) && !productDataModel.Tags.Contains(tag))
									{
										productDataModel.Tags.Add(tag);
									}
								}
								break;
							}
						}
					}
					if (isPersonalizedOfferSection)
					{
						productDataModel.Tags.Add("personalized");
					}
					bool isFiller = false;
					foreach (string fillStr in fillerTags)
					{
						if (productDataModel.Tags.Contains(fillStr))
						{
							isFiller = true;
							break;
						}
					}
					ShopBrowserButtonDataModel button = productDataModel.ToButton(isFiller);
					buttons.Add(button);
				}
				if (buttons.Count == 0)
				{
					Log.Store.Print("Tier [" + netSection.InternalName + "] is hidden because it has no products");
					continue;
				}
				TierAttributes.TryGetLayoutStyleData(TierAttributes.LayoutStyle.Default, out var defaultStyleData);
				List<string> layoutMap = defaultStyleData.LayoutMap;
				int tierWidth = defaultStyleData.LayoutWidth;
				int tierHeight = defaultStyleData.LayoutHeight;
				int maxLayoutCount = defaultStyleData.MaxLayoutCount;
				List<string> dividerOptionTags = defaultStyleData.DividerOptionTags;
				List<string> tierOptionTags = defaultStyleData.TierOptionTags;
				List<string> additionalTags = defaultStyleData.AdditionalTags;
				if (netSection.Attributes.GetValue("style").TryGetValue(out var styleStr))
				{
					if (!TierAttributes.TryParseLayoutStyle(styleStr, out var style))
					{
						Log.Store.PrintWarning("Tier [" + netSection.InternalName + "] has unknown tier layout style - " + styleStr + ", reverting to Default");
						style = TierAttributes.LayoutStyle.Default;
					}
					if (!TierAttributes.TryGetLayoutStyleData(style, out var styleData))
					{
						Log.Store.PrintWarning("Tier [" + netSection.InternalName + "] has no matching layout data with style - " + styleStr + ", using Default value");
					}
					layoutMap = styleData.LayoutMap;
					tierWidth = styleData.LayoutWidth;
					tierHeight = styleData.LayoutHeight;
					maxLayoutCount = styleData.MaxLayoutCount;
					dividerOptionTags = styleData.DividerOptionTags;
					tierOptionTags = styleData.TierOptionTags;
					additionalTags = new List<string>(styleData.AdditionalTags);
					if (style == TierAttributes.LayoutStyle.YearExpansion && styleStr != "year_expansion")
					{
						additionalTags.Add("logo_" + styleStr);
					}
				}
				if (netSection.Attributes.GetValue("layout_map").TryGetValue(out var layoutMapFlat))
				{
					if (!string.IsNullOrEmpty(layoutMapFlat))
					{
						layoutMap = layoutMapFlat.Split(',').ToList();
					}
					else
					{
						Log.Store.PrintError("Tier [" + netSection.InternalName + "] Unable to parse layout map from " + layoutMapFlat);
						layoutMap = defaultStyleData.LayoutMap;
					}
				}
				if (netSection.Attributes.GetValue("layout_size").TryGetValue(out var layoutMapSizeStr))
				{
					string[] size = layoutMapSizeStr.Split("x");
					if (size.Length == 2 && int.TryParse(size[0], out var tierWidthParsed) && int.TryParse(size[1], out var tierHeightParsed))
					{
						tierWidth = tierWidthParsed;
						tierHeight = tierHeightParsed;
					}
					else
					{
						Log.Store.PrintError("Tier [" + netSection.InternalName + "] Unable to parse layout map size from " + layoutMapSizeStr);
						tierWidth = defaultStyleData.LayoutWidth;
						tierHeight = defaultStyleData.LayoutHeight;
					}
				}
				if (netSection.Attributes.GetValue("max_layout_count").TryGetValue(out var maxLayoutCountStr))
				{
					if (int.TryParse(maxLayoutCountStr, out var maxLayoutCountParsed) && maxLayoutCountParsed > 0)
					{
						maxLayoutCount = maxLayoutCountParsed;
					}
					else
					{
						Log.Store.PrintError("Tier [" + netSection.InternalName + "] Unable to parse max layout count from " + maxLayoutCountStr);
						maxLayoutCount = defaultStyleData.MaxLayoutCount;
					}
				}
				if (netSection.Attributes.GetValue("divider_options").TryGetValue(out var dividerTagsStr))
				{
					dividerOptionTags = dividerTagsStr.Split(",").ToList();
				}
				if (netSection.Attributes.GetValue("tier_options").TryGetValue(out var tierTagsStr))
				{
					tierOptionTags = tierTagsStr.Split(",").ToList();
				}
				if (netSection.Attributes.GetValue("additional_options").TryGetValue(out var additionalOptionsStr))
				{
					additionalTags.AddRange(additionalOptionsStr.Split(",").ToList());
				}
				DataModelList<string> tags2 = netSection.Attributes.GetTags().Tags.ToDataModelList();
				if (isPersonalizedOfferSection)
				{
					tags2.Add("personalized");
				}
				foreach (string dividerTag in dividerOptionTags)
				{
					tags2.Add("show_divider_" + dividerTag);
				}
				foreach (string tierTag in tierOptionTags)
				{
					tags2.Add("show_tier_" + tierTag);
				}
				foreach (string additionalTag in additionalTags)
				{
					tags2.Add(additionalTag);
				}
				ProductTierDataModel productTierDataModel = new ProductTierDataModel
				{
					Header = netSection.Label.GetString(),
					LayoutMap = layoutMap.ToDataModelList(),
					LayoutWidth = tierWidth,
					LayoutHeight = tierHeight,
					MaxLayoutCount = maxLayoutCount,
					DisplayDivder = (dividerOptionTags.Count > 0),
					DisplayTierData = (tierOptionTags.Count > 0),
					Tags = tags2
				};
				productTierDataModel.BrowserButtons.AddRange(buttons);
				productTierDataModel.CurrenciesAvailable = new CurrencyListDataModel();
				foreach (ShopBrowserButtonDataModel button2 in productTierDataModel.BrowserButtons)
				{
					productTierDataModel.CurrenciesAvailable.Append(button2.DisplayProduct);
				}
				m_catalogTabs.AddTier(currentTabId, currentSubTabId, productTierDataModel);
				m_tierSectionMapping.Add(productTierDataModel, netSection);
			}
			m_tiersChangeCount++;
		}
	}

	private void PopulateTiers(CatalogNetworkPages networkPages)
	{
		if (m_hasUpdatedProductStatusOnce)
		{
			PopulateTiersFromNetSections(networkPages);
		}
	}

	private void SortItemsOfProductAndVariants(ProductDataModel product)
	{
		foreach (ProductDataModel variant in product.Variants)
		{
			variant.Items.Sort((RewardItemDataModel a, RewardItemDataModel b) => a.ItemId.CompareTo(b.ItemId));
		}
	}

	private static IEnumerable<ProductDataModel> GetMiniSetProducts(IEnumerable<ProductDataModel> productList)
	{
		HashSet<long> returnedProducts = new HashSet<long>();
		foreach (ProductDataModel product in productList)
		{
			if (product.Tags.Contains("mini_set") && !returnedProducts.Contains(product.PmtId))
			{
				returnedProducts.Add(product.PmtId);
				yield return product;
			}
		}
	}

	private void PopulateProductVariants()
	{
		IEnumerable<ProductDataModel> miniSetProducts = GetMiniSetProducts(m_products);
		foreach (ProductTierDataModel item in GetTiers_All())
		{
			foreach (ShopBrowserButtonDataModel browserButton in item.BrowserButtons)
			{
				ProductDataModel product = browserButton.DisplayProduct;
				PopulateVariantsForProduct(product, miniSetProducts);
				if ((product.Variants?.Count ?? 0) > 1 && (product.Items?.Count ?? 0) > 1)
				{
					DataModelList<string> tags = product.Tags;
					if (tags != null && tags.Contains("sellable_deck"))
					{
						SortItemsOfProductAndVariants(product);
					}
				}
			}
		}
		m_virtualCurrencyProduct = null;
		m_boosterCurrencyProduct = null;
		m_tavernTicketProduct = null;
		if (m_products.Count == 0)
		{
			return;
		}
		m_tavernTicketProduct = GetPrimaryProductForItemAndPopulateVariants(RewardItemType.ARENA_TICKET, 0);
		if (!ShopUtils.IsVirtualCurrencyEnabled())
		{
			return;
		}
		if (ShopUtils.TryGetMainVirtualCurrencyType(out var vcType))
		{
			RewardItemType currencyRewardItemType = ShopUtils.GetRewardItemTypeFromCurrencyType(vcType);
			m_virtualCurrencyProduct = GetPrimaryProductForItemAndPopulateVariants(currencyRewardItemType, 0);
			if (m_virtualCurrencyProduct == null)
			{
				Log.Store.PrintError($"Failed to find any Virtual Currency products for Currency Type - {vcType}.");
			}
		}
		else
		{
			Log.Store.PrintError("Failed to find any Virtual Currency products due to no related Currency Type found while Virtual Currency is enabled.");
		}
		if (ShopUtils.TryGetBoosterVirtualCurrencyType(out var bcType))
		{
			RewardItemType currencyRewardItemType2 = ShopUtils.GetRewardItemTypeFromCurrencyType(bcType);
			m_boosterCurrencyProduct = GetPrimaryProductForItemAndPopulateVariants(currencyRewardItemType2, 0);
			if (m_boosterCurrencyProduct == null)
			{
				Log.Store.PrintError($"Failed to find any Booster Currency products for Currency Type - {bcType}.");
			}
		}
	}

	private void PopulateVariantsForProduct(ProductDataModel product, IEnumerable<ProductDataModel> minisetProducts)
	{
		product.Variants.Clear();
		bool isSellableDeck = false;
		bool isBundle = false;
		bool isStandaloneVariant = false;
		bool isMiniSet = false;
		bool hasMultipleItems = product.Items.Count > 1;
		using (IEnumerator<string> enumerator = product.Tags.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				switch (enumerator.Current)
				{
				case "bundle":
				case "large_item_bundle_details":
					isBundle = true;
					break;
				case "sellable_deck":
				case "sellable_deck_bundle":
					isSellableDeck = true;
					break;
				case "standalone_variant":
					isStandaloneVariant = true;
					break;
				case "mini_set":
					isMiniSet = true;
					break;
				}
			}
		}
		if ((hasMultipleItems && !isSellableDeck && !isMiniSet) || isBundle || isStandaloneVariant)
		{
			product.Variants.Add(product);
			return;
		}
		IEnumerable<ProductDataModel> listOfVariants;
		if (isMiniSet)
		{
			listOfVariants = VariantUtils.GetMiniSetVariants(product, minisetProducts).OrderBy(m_sortOrder.Ungrouped);
		}
		else if (hasMultipleItems)
		{
			listOfVariants = VariantUtils.GetVariantsWithAllItemsMatching(product, m_products);
		}
		else
		{
			RewardItemDataModel item = product.Items[0];
			listOfVariants = VariantUtils.GetVariantsByItemType(item.ItemType, item.ItemId, m_products, m_sortOrder);
		}
		if (listOfVariants != null)
		{
			product.Variants.AddRange(listOfVariants);
		}
	}

	private ProductDataModel GetPrimaryProductForItemAndPopulateVariants(RewardItemType itemType, int itemId)
	{
		List<ProductDataModel> variants = VariantUtils.GetVariantsByItemType(itemType, itemId, m_products, m_sortOrder);
		if (variants.Count == 0)
		{
			return null;
		}
		ProductDataModel primary = variants[0];
		primary.Variants.Clear();
		if (primary.Tags.Contains("standalone_variant"))
		{
			primary.Variants.Add(primary);
		}
		else
		{
			primary.Variants.AddRange(variants);
		}
		return primary;
	}

	private ShopTabData GetShopTabData(string id, Network.ShopSection section)
	{
		ShopTabData shopTabData = new ShopTabData(id, section.Label.GetString());
		bool tabEnabled = true;
		if (section.Attributes.GetValue("tab_enabled").TryGetValue(out var tabEnabledStr) && !bool.TryParse(tabEnabledStr, out tabEnabled))
		{
			tabEnabled = true;
			Log.Store.PrintError("Cannot parse tab_enabled value as bool - " + tabEnabledStr);
		}
		shopTabData.Enabled = tabEnabled;
		int tabOrder = -1;
		if (section.Attributes.GetValue("tab_order").TryGetValue(out var tabOrderStr) && !int.TryParse(tabOrderStr, out tabOrder))
		{
			tabOrder = -1;
			Log.Store.PrintError("Cannot parse tab_order value as int - " + tabOrderStr);
		}
		shopTabData.Order = tabOrder;
		if (section.Attributes.GetValue("tab_icon").TryGetValue(out var tabIconId))
		{
			shopTabData.Icon = tabIconId.ToUpper();
		}
		if (section.Attributes.GetValue("tab_gamemode").TryGetValue(out var tabGameMode))
		{
			shopTabData.Locked = !HasTabGameModeAvailability(id, tabGameMode, out var mode, out var status);
			shopTabData.LockedMode = (shopTabData.Locked ? mode : IGamemodeAvailabilityService.Gamemode.NONE);
			shopTabData.LockedReason = (shopTabData.Locked ? status : IGamemodeAvailabilityService.Status.NONE);
		}
		return shopTabData;
	}

	private bool HasTabGameModeAvailability(string tabId, string tabGameMode, out IGamemodeAvailabilityService.Gamemode mode, out IGamemodeAvailabilityService.Status status)
	{
		mode = IGamemodeAvailabilityService.Gamemode.NONE;
		status = IGamemodeAvailabilityService.Status.NONE;
		if (string.IsNullOrEmpty(tabGameMode))
		{
			return true;
		}
		if (!ServiceManager.TryGet<IGamemodeAvailabilityService>(out var gas))
		{
			return true;
		}
		tabGameMode = tabGameMode.ToLower();
		switch (tabGameMode)
		{
		case "hs":
		case "hearthstone":
			mode = IGamemodeAvailabilityService.Gamemode.HEARTHSTONE;
			break;
		case "bg":
		case "battlegrounds":
		case "bacon":
			mode = IGamemodeAvailabilityService.Gamemode.BATTLEGROUNDS;
			break;
		case "merc":
		case "mercs":
		case "mercenaries":
		case "lettuce":
			mode = IGamemodeAvailabilityService.Gamemode.MERCENARIES;
			break;
		case "tb":
		case "tavernbrawl":
			mode = IGamemodeAvailabilityService.Gamemode.TAVERN_BRAWL;
			break;
		case "adventures":
			mode = IGamemodeAvailabilityService.Gamemode.SOLO_ADVENTURE;
			break;
		case "arena":
			mode = IGamemodeAvailabilityService.Gamemode.ARENA;
			break;
		case "dual":
			mode = IGamemodeAvailabilityService.Gamemode.DUEL;
			break;
		default:
			Log.Store.PrintWarning("Unable to determine Gamemode availability for tab " + tabId + " due to unhandled case: " + tabGameMode + "...");
			return true;
		}
		status = gas.GetGamemodeStatus(mode);
		bool isAvailable = status == IGamemodeAvailabilityService.Status.READY;
		if (!isAvailable && mode == IGamemodeAvailabilityService.Gamemode.MERCENARIES && MercenaryMessageUtils.IsMercenaryVillageShopAvailable())
		{
			status = IGamemodeAvailabilityService.Status.READY;
			isAvailable = true;
		}
		return isAvailable;
	}

	private void UpdateWarningThreshold(CatalogNetworkPages networkPages)
	{
		NetCache.NetCacheFeatures features = NetCache.Get().GetNetObject<NetCache.NetCacheFeatures>();
		if (features == null)
		{
			return;
		}
		bool prepurchaseListed = false;
		IEnumerator<CatalogNetworkPage> pageValueEnumerator = networkPages.Pages.GetEnumerator();
		while (!prepurchaseListed && pageValueEnumerator.MoveNext())
		{
			List<Network.ShopSection>.Enumerator sectionEnumerator = pageValueEnumerator.Current.Sections.GetEnumerator();
			while (!prepurchaseListed && sectionEnumerator.MoveNext())
			{
				foreach (Tuple<long, uint> item in sectionEnumerator.Current.ProductOrder)
				{
					long pmtId = item.Item1;
					if (!ProductId.IsValid(pmtId))
					{
						continue;
					}
					ProductId productId = ProductId.CreateFrom(pmtId);
					ProductDataModel product = GetProductByPmtId(productId);
					if (product != null && product.Tags.Contains("prepurchase"))
					{
						ProductInfo bundle;
						ProductAvailabilityRange range = (m_productDataStorageService.TryGetProduct(productId, out bundle) ? bundle.GetBundleAvailabilityRange(m_storeManager) : null);
						if (range != null && range.IsVisibleAtTime(DateTime.Now))
						{
							prepurchaseListed = true;
							break;
						}
					}
				}
			}
		}
		if (prepurchaseListed)
		{
			m_rotationWarningThreshold = features.Store.BoosterRotatingSoonWarnDaysWithSale;
		}
		else
		{
			m_rotationWarningThreshold = features.Store.BoosterRotatingSoonWarnDaysWithoutSale;
		}
	}

	private bool IsLatestExpansionBooster(int boosterId)
	{
		BoosterDbfRecord lastestBoosterRecord = GameDbf.Booster.GetRecord((int)m_latestBoosterId);
		if (lastestBoosterRecord == null)
		{
			return false;
		}
		BoosterDbfRecord boosterRecord = GameDbf.Booster.GetRecord(boosterId);
		if (boosterRecord == null)
		{
			return false;
		}
		if (boosterRecord.CardSetId == lastestBoosterRecord.CardSetId)
		{
			return true;
		}
		return false;
	}

	private void UpdateProductFreshness(ProductDataModel product)
	{
		bool isLatestExpansion = product.Tags.Contains("latest_expansion");
		bool isNew = product.Tags.Contains("new");
		switch (product.GetPrimaryProductTag())
		{
		case "booster":
		{
			BoosterDbId boosterId = product.GetProductBoosterId();
			if (IsLatestExpansionBooster((int)boosterId))
			{
				isLatestExpansion = true;
				isNew = true;
			}
			else if (!isNew)
			{
				BoosterDbfRecord boosterRecord = GameDbf.Booster.GetRecord((int)boosterId);
				isNew = boosterRecord != null && boosterRecord.LatestExpansionOrder == 0;
			}
			bool isWild = GameUtils.IsBoosterWild(boosterId);
			if (!product.Tags.Contains("wild") && isWild)
			{
				product.Tags.Add("wild");
			}
			else if (!product.Tags.Contains("rotating_soon") && !isWild && GameUtils.IsBoosterRotated(boosterId, DateTime.UtcNow.AddDays(m_rotationWarningThreshold)))
			{
				product.Tags.Add("rotating_soon");
			}
			break;
		}
		case "adventure":
		{
			AdventureDbId adventureId = product.GetProductAdventureId();
			isLatestExpansion |= adventureId == m_latestAdventureId;
			isNew = isNew || isLatestExpansion;
			if (!product.Tags.Contains("wild") && GameUtils.IsAdventureWild(adventureId))
			{
				product.Tags.Add("wild");
			}
			break;
		}
		case "row_runestones":
			isNew = false;
			break;
		default:
			isNew = true;
			break;
		}
		if (isNew && product.Availability == ProductAvailability.ALREADY_OWNED)
		{
			isNew = false;
		}
		if (isNew)
		{
			isNew = !m_productDataStorageService.HasProductBeenSeen(product.PmtId);
		}
		product.SetProductTagPresence("new", isNew);
		product.SetProductTagPresence("latest_expansion", isLatestExpansion);
	}

	private void UpdateNextCatalogChangeTime()
	{
		DateTime nowUtc = DateTime.UtcNow;
		if (m_nextCatalogChangeTimeUtc.HasValue && m_nextCatalogChangeTimeUtc.Value <= nowUtc)
		{
			return;
		}
		ProductDataModel productFound = null;
		ProductAvailabilityRange rangeFound = null;
		foreach (ProductDataModel product in m_products)
		{
			if (product.PmtId == 0L || !m_productDataStorageService.TryGetProduct(product.PmtId, out var bundle))
			{
				continue;
			}
			ProductAvailabilityRange range = bundle.GetBundleAvailabilityRange(m_storeManager);
			if (range != null && !range.IsNever)
			{
				if (range.StartDateTime.HasValue && range.StartDateTime.Value > nowUtc && (!m_nextCatalogChangeTimeUtc.HasValue || range.StartDateTime.Value < m_nextCatalogChangeTimeUtc.Value))
				{
					m_nextCatalogChangeTimeUtc = range.StartDateTime.Value;
					productFound = product;
					rangeFound = range;
				}
				if (range.SoftEndDateTime.HasValue && range.SoftEndDateTime.Value > nowUtc && (!m_nextCatalogChangeTimeUtc.HasValue || range.SoftEndDateTime.Value < m_nextCatalogChangeTimeUtc.Value))
				{
					m_nextCatalogChangeTimeUtc = range.SoftEndDateTime.Value;
					productFound = product;
					rangeFound = range;
				}
			}
		}
		if (m_nextCatalogChangeTimeUtc.HasValue)
		{
			Log.Store.PrintDebug($"Next product availability change at {m_nextCatalogChangeTimeUtc.Value.ToLocalTime():g}");
			if (productFound != null)
			{
				Log.Store.PrintDebug($"Next product to change availability is PMT ID = {productFound.PmtId}, Name = [{productFound.Name}], range = {rangeFound}");
			}
		}
		else
		{
			Log.Store.PrintDebug("No known incoming product availability changes");
		}
	}

	private void HealupProductTiers()
	{
		foreach (ProductTierDataModel productTierDataModel in GetTiers_All())
		{
			if (productTierDataModel == null || productTierDataModel.CurrenciesAvailable == null)
			{
				continue;
			}
			foreach (ShopBrowserButtonDataModel button in productTierDataModel.BrowserButtons)
			{
				productTierDataModel.CurrenciesAvailable.Append(button.DisplayProduct, includeVariants: true);
			}
		}
	}
}
