using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Blizzard.Commerce;
using Blizzard.GameService.SDK.Client.Integration;
using Blizzard.T5.Core;
using Blizzard.T5.Jobs;
using Blizzard.T5.Logging;
using Blizzard.T5.Services;
using Blizzard.Telemetry.WTCG.Client;
using Hearthstone;
using Hearthstone.Commerce;
using Hearthstone.Core;
using Hearthstone.Store;
using Hearthstone.UI;
using PegasusShared;
using PegasusUtil;
using UnityEngine;

public class StoreManager : IProductAvailabilityApi
{
	private enum PurchaseErrorSource
	{
		FROM_PURCHASE_METHOD_RESPONSE,
		FROM_STATUS_OR_PURCHASE_RESPONSE,
		FROM_PREVIOUS_PURCHASE
	}

	private enum TransactionStatus
	{
		UNKNOWN,
		IN_PROGRESS_MONEY,
		IN_PROGRESS_GOLD_GTAPP,
		IN_PROGRESS_GOLD_NO_GTAPP,
		READY,
		WAIT_ZERO_COST_LICENSE,
		WAIT_METHOD_OF_PAYMENT,
		WAIT_CONFIRM,
		WAIT_RISK,
		CHALLENGE_SUBMITTED,
		CHALLENGE_CANCELED,
		USER_CANCELING,
		AUTO_CANCELING,
		IN_PROGRESS_BLIZZARD_CHECKOUT,
		WAIT_BLIZZARD_CHECKOUT
	}

	private enum LicenseStatus
	{
		NOT_OWNED,
		OWNED,
		OWNED_AND_BLOCKING,
		UNDEFINED
	}

	private struct ShowStoreData
	{
		public bool isTotallyFake;

		public Store.ExitCallback exitCallback;

		public object exitCallbackUserData;

		public ProductType storeProduct;

		public int storeProductData;

		public int numItemsRequired;

		public bool useOverlayUI;

		public int pmtProductId;

		public bool closeOnTransactionComplete;

		public string TargetTabId;

		public IDataModel dataModel;
	}

	public static readonly int DEFAULT_SECONDS_BEFORE_AUTO_CANCEL = 600;

	public const int NO_ITEM_COUNT_REQUIREMENT = 0;

	public const int NO_PRODUCT_DATA_REQUIREMENT = 0;

	public const string DEFAULT_COMMERCE_CLIENT_ID = "df5787f96b2b46c49c66dd45bcb05490";

	private static readonly int UNKNOWN_TRANSACTION_ID = -1;

	private static readonly double CURRENCY_TRANSACTION_TIMEOUT_SECONDS = 30.0;

	private static readonly Map<AdventureDbId, ProductType> s_adventureToProductMap = new Map<AdventureDbId, ProductType>
	{
		{
			AdventureDbId.NAXXRAMAS,
			ProductType.PRODUCT_TYPE_NAXX
		},
		{
			AdventureDbId.BRM,
			ProductType.PRODUCT_TYPE_BRM
		},
		{
			AdventureDbId.LOE,
			ProductType.PRODUCT_TYPE_LOE
		}
	};

	private static StoreManager s_instance = null;

	private readonly ShopView m_view = new ShopView();

	private bool m_featuresReady;

	private bool m_initComplete;

	private bool m_battlePayAvailable;

	private bool m_hasAttemptedToInitBattlePay;

	private bool m_firstNoticesProcessed;

	private bool m_firstMoneyOrGTAPPTransactionSet;

	private bool m_isStatusRefreshPending;

	private bool m_isShowingStoreUnavailableAlert;

	private float m_secsBeforeAutoCancel = DEFAULT_SECONDS_BEFORE_AUTO_CANCEL;

	private float m_lastCancelRequestTime;

	private readonly Map<int, Network.GoldCostBooster> m_goldCostBooster = new Map<int, Network.GoldCostBooster>();

	private readonly Dictionary<int, Network.ShopSale> m_sales = new Dictionary<int, Network.ShopSale>();

	private long? m_goldCostArena;

	private Currency m_currency = new Currency();

	private readonly HashSet<long> m_transactionIDsConclusivelyHandled = new HashSet<long>();

	private readonly Map<ShopType, IStore> m_stores = new Map<ShopType, IStore>();

	private ShopType m_currentShopType;

	private bool m_ignoreProductTiming;

	private float m_showStoreStart;

	private Network.PurchaseMethod m_challengePurchaseMethod;

	private BnetRegion m_regionId;

	private StorePackId m_currentlySelectedId;

	private bool m_canCloseConfirmation = true;

	private bool m_openWhenLastEventFired;

	private TransactionStatus m_status;

	private bool m_waitingToShowStore;

	private ShowStoreData m_showStoreData;

	private MoneyOrGTAPPTransaction m_activeMoneyOrGTAPPTransaction;

	private BuyProductEventArgs m_pendingProductPurchaseArgs;

	private readonly HashSet<long> m_confirmedTransactionIDs = new HashSet<long>();

	private readonly List<NetCache.ProfileNoticePurchase> m_outstandingPurchaseNotices = new List<NetCache.ProfileNoticePurchase>();

	private List<Achievement> m_completedAchieves = new List<Achievement>();

	private bool m_licenseAchievesListenerRegistered;

	private TransactionStatus m_previousStatusBeforeAutoCancel;

	private static readonly PlatformDependentValue<bool> HAS_THIRD_PARTY_APP_STORE = new PlatformDependentValue<bool>(PlatformCategory.OS)
	{
		PC = false,
		Mac = false,
		iOS = true,
		Android = true
	};

	public bool IgnoreProductTiming => m_ignoreProductTiming;

	public Dictionary<int, Network.ShopSale> Sales => m_sales;

	public IEnumerable<Network.GoldCostBooster> AllGoldCostBoosters => m_goldCostBooster.Values;

	public ShopType CurrentShopType => m_currentShopType;

	public StorePackId CurrentlySelectedId => m_currentlySelectedId;

	public bool IsPromptShowing
	{
		get
		{
			if (!m_view.IsPromptShowing())
			{
				return IsInTransaction();
			}
			return true;
		}
	}

	private TransactionStatus Status
	{
		get
		{
			return m_status;
		}
		set
		{
			if (0f == m_lastCancelRequestTime && m_status == TransactionStatus.UNKNOWN)
			{
				m_lastCancelRequestTime = Time.realtimeSinceStartup;
			}
			m_status = value;
			FireStatusChangedEventIfNeeded();
		}
	}

	public bool FirstNoticesProcessed
	{
		get
		{
			return m_firstNoticesProcessed;
		}
		set
		{
			m_firstNoticesProcessed = value;
			FireStatusChangedEventIfNeeded();
		}
	}

	public bool BattlePayAvailable
	{
		get
		{
			return m_battlePayAvailable;
		}
		private set
		{
			m_battlePayAvailable = value;
			m_hasAttemptedToInitBattlePay = true;
			FireStatusChangedEventIfNeeded();
		}
	}

	private bool FeaturesReady
	{
		get
		{
			return m_featuresReady;
		}
		set
		{
			m_featuresReady = value;
			FireStatusChangedEventIfNeeded();
		}
	}

	public static bool HasExternalStore
	{
		get
		{
			if (!HAS_THIRD_PARTY_APP_STORE)
			{
				return false;
			}
			return true;
		}
	}

	private event Action<bool> OnStatusChanged = delegate
	{
	};

	private event Action<ProductInfo, PaymentMethod> OnSuccessfulPurchaseAck = delegate
	{
	};

	private event Action<ProductInfo, PaymentMethod> OnSuccessfulPurchase = delegate
	{
	};

	private event Action<ProductInfo, PaymentMethod> OnFailedPurchaseAck = delegate
	{
	};

	private event Action OnAuthorizationExit = delegate
	{
	};

	private event Action OnStoreShown = delegate
	{
	};

	private event Action OnStoreHidden = delegate
	{
	};

	public ItemOwnershipStatus GetProductItemOwnershipStatus(ProductType product, int productData, out string failReason)
	{
		return GetStaticProductItemOwnershipStatus(product, productData, out failReason);
	}

	private StoreManager()
	{
	}

	public static StoreManager Get()
	{
		return s_instance ?? (s_instance = new StoreManager());
	}

	public static bool IsInitialized()
	{
		return s_instance != null;
	}

	private static void DestroyInstance()
	{
		s_instance.GetStore(ShopType.GENERAL_STORE)?.Unload();
		if (AchieveManager.Get() != null && s_instance != null)
		{
			AchieveManager.Get().RemoveAchievesUpdatedListener(s_instance.OnAchievesUpdated);
			AchieveManager.Get().RemoveLicenseAddedAchievesUpdatedListener(s_instance.OnLicenseAddedAchievesUpdated);
		}
		s_instance = null;
	}

	private void NetworkRegistration()
	{
		Network network = Network.Get();
		network.RegisterNetHandler(BattlePayStatusResponse.PacketID.ID, OnBattlePayStatusResponse);
		network.RegisterNetHandler(BattlePayConfigResponse.PacketID.ID, OnBattlePayConfigResponse);
		network.RegisterNetHandler(PurchaseMethod.PacketID.ID, OnPurchaseMethod);
		network.RegisterNetHandler(PurchaseResponse.PacketID.ID, OnPurchaseResponse);
		network.RegisterNetHandler(CancelPurchaseResponse.PacketID.ID, OnPurchaseCanceledResponse);
		network.RegisterNetHandler(PurchaseWithGoldResponse.PacketID.ID, OnPurchaseViaGoldResponse);
		network.RegisterNetHandler(ThirdPartyPurchaseStatusResponse.PacketID.ID, OnThirdPartyPurchaseStatusResponse);
	}

	public void Init()
	{
		NetCache.Get().RegisterUpdatedListener(typeof(NetCache.NetCacheFeatures), OnNetCacheFeaturesReady);
		if (!m_initComplete)
		{
			SceneMgr.Get().RegisterSceneUnloadedEvent(OnSceneUnloaded);
			NetworkRegistration();
			NetCache.NetCacheProfileNotices notices = NetCache.Get().GetNetObject<NetCache.NetCacheProfileNotices>();
			Processor.QueueJob("StoreManager.Init", Job_CompleteInit(), JobFlags.StartImmediately);
			if (notices != null)
			{
				OnNewNotices(notices.Notices, isInitialNoticeList: false);
			}
			NetCache.Get().RegisterNewNoticesListener(OnNewNotices);
			LoginManager.Get().OnFullLoginFlowComplete += OnLoginCompleted;
			m_regionId = BattleNet.GetCurrentRegion();
			RegisterViewListeners();
			AccountLicenseMgr.Get().RegisterAccountLicensesChangedListener(OnAccountLicensesUpdate);
			m_initComplete = true;
			HearthstoneApplication.Get().WillReset += WillReset;
		}
	}

	private IEnumerator<IAsyncJobResult> Job_CompleteInit()
	{
		IProductDataService dataService;
		while (!ServiceManager.TryGet<IProductDataService>(out dataService))
		{
			yield return null;
		}
		NetCache.Get().OwnedBattlegroundsSkinsChanged += dataService.UpdateProductStatus;
	}

	private void WillReset()
	{
		ShopInitialization.Reset();
		HearthstoneApplication.Get().WillReset -= WillReset;
		AccountLicenseMgr.Get().RemoveAccountLicensesChangedListener(OnAccountLicensesUpdate);
		UnregisterViewListeners();
		Network network = Network.Get();
		network.RemoveNetHandler(BattlePayStatusResponse.PacketID.ID, OnBattlePayStatusResponse);
		network.RemoveNetHandler(BattlePayConfigResponse.PacketID.ID, OnBattlePayConfigResponse);
		network.RemoveNetHandler(PurchaseMethod.PacketID.ID, OnPurchaseMethod);
		network.RemoveNetHandler(PurchaseResponse.PacketID.ID, OnPurchaseResponse);
		network.RemoveNetHandler(CancelPurchaseResponse.PacketID.ID, OnPurchaseCanceledResponse);
		network.RemoveNetHandler(PurchaseWithGoldResponse.PacketID.ID, OnPurchaseViaGoldResponse);
		network.RemoveNetHandler(ThirdPartyPurchaseStatusResponse.PacketID.ID, OnThirdPartyPurchaseStatusResponse);
		NetCache.Get().RemoveUpdatedListener(typeof(NetCache.NetCacheFeatures), OnNetCacheFeaturesReady);
		if (ServiceManager.TryGet<IProductDataService>(out var dataService))
		{
			NetCache.Get().OwnedBattlegroundsSkinsChanged -= dataService.UpdateProductStatus;
		}
		DestroyInstance();
	}

	public void Heartbeat()
	{
		if (m_initComplete)
		{
			if (m_isStatusRefreshPending)
			{
				m_isStatusRefreshPending = false;
				HandleShopAvailabilityChange();
			}
			float now = Time.realtimeSinceStartup;
			AutoCancelPurchaseIfNeeded(now);
		}
	}

	public bool IsOpen()
	{
		return ShopInitialization.Status == ShopStatus.Ready;
	}

	public bool IsBattlePayFeatureEnabled()
	{
		NetCache.NetCacheFeatures features = GetNetCacheFeatures();
		if (features == null)
		{
			return false;
		}
		if (features.Store.Store)
		{
			return features.Store.BattlePay;
		}
		return false;
	}

	public bool IsBuyWithGoldFeatureEnabled()
	{
		NetCache.NetCacheFeatures features = GetNetCacheFeatures();
		if (features == null)
		{
			return false;
		}
		if (features.Store.Store)
		{
			return features.Store.BuyWithGold;
		}
		return false;
	}

	private void SetCanTapOutConfirmationUI(bool closeConfirmationUI)
	{
		m_canCloseConfirmation = closeConfirmationUI;
	}

	public bool CanTapOutConfirmationUI()
	{
		return m_canCloseConfirmation;
	}

	public bool IsSimpleCheckoutFeatureEnabled()
	{
		NetCache.NetCacheFeatures features = GetNetCacheFeatures();
		if (features == null)
		{
			return false;
		}
		bool isPlatformEnabled = false;
		switch (PlatformSettings.RuntimeOS)
		{
		case OSCategory.PC:
			isPlatformEnabled = features.Store.SimpleCheckoutWin;
			break;
		case OSCategory.Mac:
			isPlatformEnabled = features.Store.SimpleCheckoutMac;
			break;
		case OSCategory.iOS:
			isPlatformEnabled = features.Store.SimpleCheckoutIOS;
			break;
		case OSCategory.Android:
			switch (AndroidDeviceSettings.Get().GetAndroidStore())
			{
			case AndroidStore.AMAZON:
				isPlatformEnabled = features.Store.SimpleCheckoutAndroidAmazon;
				break;
			case AndroidStore.GOOGLE:
				isPlatformEnabled = features.Store.SimpleCheckoutAndroidGoogle;
				break;
			case AndroidStore.BLIZZARD:
			case AndroidStore.HUAWEI:
			case AndroidStore.ONE_STORE:
			case AndroidStore.DASHEN:
				isPlatformEnabled = features.Store.SimpleCheckoutAndroidGlobal;
				break;
			default:
				Log.Store.PrintError("The given store was not accounted for: {0}\nPlease check in '{1}.{2}' class and method for implementation.", AndroidDeviceSettings.Get().GetAndroidStore().ToString(), "StoreManager", "IsSimpleCheckoutFeatureEnabled");
				break;
			}
			break;
		}
		if (isPlatformEnabled && features.Store.Store && features.Store.SimpleCheckout)
		{
			return ServiceManager.Get<HearthstoneCheckout>()?.IsAvailable() ?? false;
		}
		return false;
	}

	private bool IsSoftAccountPurchasingEnabled()
	{
		NetCache.NetCacheFeatures features = GetNetCacheFeatures();
		if (features == null)
		{
			return false;
		}
		if (features.Store.Store)
		{
			return features.Store.SoftAccountPurchasing;
		}
		return false;
	}

	public bool IsBuyCardBacksFromCollectionManagerEnabled()
	{
		return GetNetCacheFeatures()?.Store.BuyCardBacksFromCollectionManager ?? true;
	}

	public bool IsBuyHeroSkinsFromCollectionManagerEnabled()
	{
		return GetNetCacheFeatures()?.Store.BuyHeroSkinsFromCollectionManager ?? true;
	}

	public bool IsLargeItemBundleDetailsEnabled()
	{
		return GetNetCacheFeatures()?.Store.LargeItemBundleDetailsEnabled ?? true;
	}

	public BattlePayProvider? ActiveTransactionProvider()
	{
		return m_activeMoneyOrGTAPPTransaction?.Provider;
	}

	public void RegisterStatusChangedListener(Action<bool> callback)
	{
		OnStatusChanged -= callback;
		OnStatusChanged += callback;
	}

	public void RemoveStatusChangedListener(Action<bool> callback)
	{
		OnStatusChanged -= callback;
	}

	public void RegisterSuccessfulPurchaseListener(Action<ProductInfo, PaymentMethod> callback)
	{
		OnSuccessfulPurchase -= callback;
		OnSuccessfulPurchase += callback;
	}

	public void RemoveSuccessfulPurchaseListener(Action<ProductInfo, PaymentMethod> callback)
	{
		OnSuccessfulPurchase -= callback;
	}

	public void RegisterSuccessfulPurchaseAckListener(Action<ProductInfo, PaymentMethod> callback)
	{
		OnSuccessfulPurchaseAck -= callback;
		OnSuccessfulPurchaseAck += callback;
	}

	public void RemoveSuccessfulPurchaseAckListener(Action<ProductInfo, PaymentMethod> callback)
	{
		OnSuccessfulPurchaseAck -= callback;
	}

	public void RegisterFailedPurchaseAckListener(Action<ProductInfo, PaymentMethod> callback)
	{
		OnFailedPurchaseAck -= callback;
		OnFailedPurchaseAck += callback;
	}

	public void RemoveFailedPurchaseAckListener(Action<ProductInfo, PaymentMethod> callback)
	{
		OnFailedPurchaseAck -= callback;
	}

	public void RegisterAuthorizationExitListener(Action callback)
	{
		OnAuthorizationExit -= callback;
		OnAuthorizationExit += callback;
	}

	public void RemoveAuthorizationExitListener(Action callback)
	{
		OnAuthorizationExit -= callback;
	}

	public void RegisterStoreShownListener(Action callback)
	{
		OnStoreShown -= callback;
		OnStoreShown += callback;
	}

	public void RemoveStoreShownListener(Action callback)
	{
		OnStoreShown -= callback;
	}

	public void RegisterStoreHiddenListener(Action callback)
	{
		OnStoreHidden -= callback;
		OnStoreHidden += callback;
	}

	public void RemoveStoreHiddenListener(Action callback)
	{
		OnStoreHidden -= callback;
	}

	private void RegisterViewListeners()
	{
		m_view.OnComponentReady += StoreViewReady;
		m_view.PurchaseAuth.OnPurchaseResultAcknowledged += OnPurchaseResultAcknowledged;
		m_view.PurchaseAuth.OnCancelButtonPressed += OnPurchaseAuthCancelButtonPressed;
		m_view.PurchaseAuth.OnAuthExit += OnAuthExit;
		m_view.Summary.OnSummaryConfirm += OnSummaryConfirm;
		m_view.Summary.OnSummaryCancel += OnSummaryCancel;
		m_view.Summary.OnSummaryInfo += OnSummaryInfo;
		m_view.Summary.OnSummaryPaymentAndTos += OnSummaryPaymentAndTOS;
		m_view.SendToBam.OnOkay += OnSendToBAMOkay;
		m_view.SendToBam.OnCancel += OnSendToBAMCancel;
		m_view.LegalBam.OnOkay += OnSendToBAMLegal;
		m_view.LegalBam.OnCancel += UnblockStoreInterface;
		m_view.DoneWithBam.OnOkay += UnblockStoreInterface;
		m_view.ChallengePrompt.OnComplete += OnChallengeComplete;
		m_view.ChallengePrompt.OnCancel += OnChallengeCancel;
	}

	private void UnregisterViewListeners()
	{
		m_view.OnComponentReady -= StoreViewReady;
		m_view.PurchaseAuth.OnPurchaseResultAcknowledged -= OnPurchaseResultAcknowledged;
		m_view.PurchaseAuth.OnCancelButtonPressed -= OnPurchaseAuthCancelButtonPressed;
		m_view.PurchaseAuth.OnAuthExit -= OnAuthExit;
		m_view.Summary.OnSummaryConfirm -= OnSummaryConfirm;
		m_view.Summary.OnSummaryCancel -= OnSummaryCancel;
		m_view.Summary.OnSummaryInfo -= OnSummaryInfo;
		m_view.Summary.OnSummaryPaymentAndTos -= OnSummaryPaymentAndTOS;
		m_view.SendToBam.OnOkay -= OnSendToBAMOkay;
		m_view.SendToBam.OnCancel -= OnSendToBAMCancel;
		m_view.LegalBam.OnOkay -= OnSendToBAMLegal;
		m_view.LegalBam.OnCancel -= UnblockStoreInterface;
		m_view.DoneWithBam.OnOkay -= UnblockStoreInterface;
		m_view.ChallengePrompt.OnComplete -= OnChallengeComplete;
		m_view.ChallengePrompt.OnCancel -= OnChallengeCancel;
	}

	private bool IsWaitingToShow()
	{
		return m_waitingToShowStore;
	}

	public IStore GetCurrentStore()
	{
		return GetStore(m_currentShopType);
	}

	private IStore GetStore(ShopType shopType)
	{
		m_stores.TryGetValue(shopType, out var store);
		return store;
	}

	public void BlockStoreInterface()
	{
		GetCurrentStore()?.BlockInterface(blocked: true);
	}

	public void UnblockStoreInterface()
	{
		GetCurrentStore()?.BlockInterface(blocked: false);
	}

	public bool IsShown()
	{
		return GetCurrentStore()?.IsOpen() ?? false;
	}

	public bool IsShownOrWaitingToShow()
	{
		if (IsWaitingToShow())
		{
			return true;
		}
		if (IsShown())
		{
			return true;
		}
		return false;
	}

	public bool IsPurchaseAuthViewShown()
	{
		PurchaseAuthView purchaseAuth = m_view.PurchaseAuth;
		if (purchaseAuth == null || !purchaseAuth.IsLoaded)
		{
			return false;
		}
		return purchaseAuth.IsShown;
	}

	public bool GetGoldCostNoGTAPP(NoGTAPPTransactionData noGTAPPTransactionData, out long cost)
	{
		cost = 0L;
		if (noGTAPPTransactionData == null)
		{
			return false;
		}
		long baseCost = 0L;
		switch (noGTAPPTransactionData.Product)
		{
		case ProductType.PRODUCT_TYPE_BOOSTER:
		case ProductType.PRODUCT_TYPE_MERCENARIES_BOOSTER:
			if (!GetBoosterGoldCostNoGTAPP(noGTAPPTransactionData.ProductData, out baseCost))
			{
				return false;
			}
			break;
		case ProductType.PRODUCT_TYPE_DRAFT:
			if (!GetArenaGoldCostNoGTAPP(out baseCost))
			{
				return false;
			}
			break;
		case ProductType.PRODUCT_TYPE_HIDDEN_LICENSE:
			return false;
		default:
			Log.Store.PrintWarning($"StoreManager.GetGoldCostNoGTAPP(): don't have a no-GTAPP gold price for product {noGTAPPTransactionData.Product} data {noGTAPPTransactionData.ProductData}");
			return false;
		}
		cost = baseCost * noGTAPPTransactionData.Quantity;
		return true;
	}

	private ProductInfo GetBundleFromPmtProductId(long? productId)
	{
		if (!ServiceManager.TryGet<IProductDataService>(out var dataService) || !dataService.TryGetProduct(productId, out var product))
		{
			return null;
		}
		return product;
	}

	private ProductInfo GetBundleFromPmtProductId(ProductId productId)
	{
		if (productId.IsValid() && ServiceManager.TryGet<IProductDataService>(out var dataService) && dataService.TryGetProduct(productId, out var bundle))
		{
			return bundle;
		}
		return null;
	}

	private HashSet<ProductType> GetProductsInItemList(IEnumerable<Network.BundleItem> items)
	{
		HashSet<ProductType> includedProducts = new HashSet<ProductType>();
		foreach (Network.BundleItem item in items)
		{
			includedProducts.Add(item.ItemType);
		}
		return includedProducts;
	}

	public ProductAvailability GetNetworkBundleProductAvailability(ProductInfo bundle, bool shouldSeeWild, bool hasFinishedApprenticeship, bool checkRange = true)
	{
		if (bundle == null)
		{
			return ProductAvailability.UNDEFINED;
		}
		bool isRestricted = false;
		int numOwned = 0;
		int numBuyable = 0;
		int numBlocked = 0;
		int numUnknown = 0;
		if (!hasFinishedApprenticeship && !isRestricted)
		{
			CatalogUtils.TagData tags = bundle.Attributes.GetTags();
			if (tags.Contains("requires_apprenticeship_complete") || tags.Contains("tavern_pass"))
			{
				isRestricted = true;
				ProductIssues.LogHidden(bundle.Id, "Hidden due to apprenticeship content");
			}
		}
		foreach (Network.BundleItem item in bundle.Items)
		{
			if (!shouldSeeWild && !isRestricted)
			{
				switch (item.ItemType)
				{
				case ProductType.PRODUCT_TYPE_MINI_SET:
					isRestricted = GameUtils.IsBoosterWild((BoosterDbId)GameDbf.MiniSet.GetRecord(item.ProductData).BoosterId);
					break;
				case ProductType.PRODUCT_TYPE_BOOSTER:
					isRestricted = GameUtils.IsBoosterWild((BoosterDbId)item.ProductData);
					break;
				case ProductType.PRODUCT_TYPE_WING:
					isRestricted = GameUtils.IsAdventureWild(GameUtils.GetAdventureIdByWingId(item.ProductData));
					break;
				case ProductType.PRODUCT_TYPE_NAXX:
				case ProductType.PRODUCT_TYPE_BRM:
				case ProductType.PRODUCT_TYPE_LOE:
					isRestricted = true;
					break;
				}
				if (isRestricted)
				{
					ProductIssues.LogHidden(bundle.Id, $"Hidden due to wild content: Type={item.ItemType}, ID = {item.ProductData}");
				}
			}
			ItemPurchaseRule purchRule = GetProductItemPurchaseRule(item);
			if (purchRule == ItemPurchaseRule.UNDEFINED)
			{
				ProductIssues.LogError(bundle.Id, $"Has license with undefined rule about re-purchase. Type={item.ItemType} Data={item.ProductData}");
				isRestricted = true;
			}
			string failReason;
			switch (GetProductItemOwnershipStatus(item.ItemType, item.ProductData, out failReason))
			{
			case ItemOwnershipStatus.OWNED:
				numOwned++;
				if (purchRule == ItemPurchaseRule.BLOCKING)
				{
					numBlocked++;
				}
				break;
			case ItemOwnershipStatus.IGNORED:
			case ItemOwnershipStatus.UNOWNED:
				if (item.ItemType != ProductType.PRODUCT_TYPE_HIDDEN_LICENSE)
				{
					numBuyable++;
				}
				break;
			default:
				ProductIssues.LogError(bundle.Id, failReason ?? $"Has license with unknown ownership status. Type={item.ItemType} Data={item.ProductData}");
				numUnknown++;
				break;
			}
		}
		if (numUnknown > 0)
		{
			return ProductAvailability.UNDEFINED;
		}
		if (isRestricted)
		{
			return ProductAvailability.RESTRICTED;
		}
		if ((numOwned > 0 && numBuyable == 0) || numBlocked > 0)
		{
			return ProductAvailability.ALREADY_OWNED;
		}
		if (numOwned == 0 && numBuyable == 0)
		{
			ProductIssues.LogError(bundle.Id, "Has no buyable or owned rewards. Availability status will remain UNDEFINED.");
			return ProductAvailability.UNDEFINED;
		}
		if (checkRange)
		{
			ProductAvailabilityRange range = bundle.GetBundleAvailabilityRange(this);
			if (range == null)
			{
				ProductIssues.LogError(bundle.Id, "Has unknown sale or event timing: event timing = " + bundle.ProductEvent + ", Sale ID = " + string.Join(",", bundle.SaleIds.Select((int id) => id.ToString()).ToArray()));
				return ProductAvailability.SALE_NOT_ACTIVE;
			}
			if (!range.IsVisibleAtTime(DateTime.UtcNow))
			{
				return ProductAvailability.SALE_NOT_ACTIVE;
			}
		}
		return ProductAvailability.CAN_PURCHASE;
	}

	public bool IsProductAlreadyOwned(ProductInfo bundle)
	{
		return GetNetworkBundleProductAvailability(bundle, shouldSeeWild: true, hasFinishedApprenticeship: true, checkRange: false) == ProductAvailability.ALREADY_OWNED;
	}

	public IEnumerable<ProductInfo> GetAvailableBundlesForProduct(ProductType productType, bool requireNonGoldPriceOption, int productData = 0, int numItemsRequired = 0)
	{
		if (!ServiceManager.TryGet<IProductDataService>(out var dataService))
		{
			yield break;
		}
		foreach (ProductInfo bundle in dataService.EnumerateBundlesForProductType(productType, requireNonGoldPriceOption, productData, numItemsRequired))
		{
			if (CanBuyBundle(bundle))
			{
				yield return bundle;
			}
		}
	}

	public void GetAvailableAdventureBundle(AdventureDbId adventureId, bool requireNonGoldOption, out ProductInfo bundle)
	{
		bundle = null;
		if (GetAdventureProductType(adventureId) == ProductType.PRODUCT_TYPE_UNKNOWN)
		{
			return;
		}
		IEnumerable<ProductInfo> bundleOptions = null;
		switch (adventureId)
		{
		case AdventureDbId.NAXXRAMAS:
			bundleOptions = GetAvailableBundlesForProduct(ProductType.PRODUCT_TYPE_NAXX, requireNonGoldOption, 5);
			break;
		case AdventureDbId.BRM:
			bundleOptions = GetAvailableBundlesForProduct(ProductType.PRODUCT_TYPE_BRM, requireNonGoldOption, 10);
			break;
		case AdventureDbId.LOE:
			bundleOptions = GetAvailableBundlesForProduct(ProductType.PRODUCT_TYPE_LOE, requireNonGoldOption, 14);
			break;
		default:
		{
			int finalWingId = AdventureUtils.GetFinalAdventureWing((int)adventureId, excludeOwnedWings: false);
			bundleOptions = GetAvailableBundlesForProduct(ProductType.PRODUCT_TYPE_WING, requireNonGoldOption, finalWingId);
			break;
		}
		}
		foreach (ProductInfo bundleOption in bundleOptions)
		{
			int numItems = bundleOption.Items.Count;
			if (numItems != 0 && (!requireNonGoldOption || bundleOption.HasNonGoldPrice()) && bundleOption.IsBundleAvailableNow(this))
			{
				if (bundle == null)
				{
					bundle = bundleOption;
				}
				else if (bundle.Items.Count <= numItems)
				{
					bundle = bundleOption;
				}
			}
		}
	}

	private bool CanBuyBoosterWithGold(int boosterDbId)
	{
		BoosterDbfRecord boosterDbfRecord = GameDbf.Booster.GetRecord(boosterDbId);
		if (boosterDbfRecord == null)
		{
			return false;
		}
		EventTimingType buyWithGoldEvent = boosterDbfRecord.BuyWithGoldEvent;
		return buyWithGoldEvent switch
		{
			EventTimingType.UNKNOWN => false, 
			EventTimingType.IGNORE => true, 
			_ => EventTimingManager.Get().IsEventActive(buyWithGoldEvent), 
		};
	}

	public bool GetHeroBundleByCardDbId(int heroCardDbId, out ProductInfo heroBundle)
	{
		if (!ServiceManager.TryGet<IProductDataService>(out var dataService))
		{
			heroBundle = null;
			return false;
		}
		foreach (ProductInfo bundle in dataService.EnumerateBundlesForProductType(ProductType.PRODUCT_TYPE_HERO, requireRealMoneyOption: false, heroCardDbId, 0, checkAvailability: false))
		{
			bool isHiddenLicenseBundle = false;
			foreach (Network.BundleItem item in bundle.Items)
			{
				if (item.ItemType == ProductType.PRODUCT_TYPE_HIDDEN_LICENSE)
				{
					isHiddenLicenseBundle = true;
				}
			}
			if (!isHiddenLicenseBundle)
			{
				heroBundle = bundle;
				return true;
			}
		}
		heroBundle = null;
		return false;
	}

	public bool IsKoreanCustomer()
	{
		return m_currency.SubRegion == 3;
	}

	public bool IsChineseCustomer()
	{
		return m_currency.SubRegion == 5;
	}

	public bool IsEuropeanCustomer()
	{
		if (m_currency.SubRegion == 2)
		{
			return true;
		}
		if (m_currency.SubRegion == 10)
		{
			return true;
		}
		return false;
	}

	public bool IsNorthAmericanCustomer()
	{
		if (m_currency.SubRegion == 1)
		{
			return true;
		}
		return false;
	}

	public string GetTaxText()
	{
		return m_currency.TaxText switch
		{
			Currency.Tax.TAX_ADDED => GameStrings.Get("GLUE_STORE_SUMMARY_TAX_DISCLAIMER_USD"), 
			Currency.Tax.NO_TAX => string.Empty, 
			_ => GameStrings.Get("GLUE_STORE_SUMMARY_TAX_DISCLAIMER"), 
		};
	}

	public string GetCurrencyCode()
	{
		return m_currency.Code;
	}

	public string FormatCost(double? costDisplay, Locale? locale = null)
	{
		string currencyFormat = m_currency.GetFormat();
		CultureInfo culture = Localization.GetCultureInfo();
		if (locale.HasValue)
		{
			culture = CultureInfo.CreateSpecificCulture(Localization.ConvertLocaleToDotNet(locale.Value.ToString()));
			currencyFormat = "{0:c}";
		}
		else
		{
			culture.NumberFormat.CurrencySymbol = " " + m_currency.Symbol + " ";
		}
		return string.Format(culture, currencyFormat, costDisplay).Replace("  ", " ").Trim();
	}

	public string GetProductName(ProductInfo bundle)
	{
		if (bundle == null)
		{
			return string.Empty;
		}
		string title = bundle.Title;
		if (!string.IsNullOrEmpty(title))
		{
			return title;
		}
		if (bundle.Items.Count == 1)
		{
			Network.BundleItem bundleItem = bundle.Items[0];
			return GetSingleItemProductName(bundleItem);
		}
		return GetMultiItemProductName(bundle);
	}

	public void StartGeneralTransaction(string targetTabId = "", Store.ExitCallback exitCallback = null, bool isTotallyFake = false)
	{
		if (m_waitingToShowStore)
		{
			Log.Store.Print("StoreManager.StartGeneralTransaction(): already waiting to show store");
			return;
		}
		m_currentShopType = ShopType.GENERAL_STORE;
		m_showStoreData.TargetTabId = targetTabId;
		m_showStoreData.exitCallback = exitCallback;
		m_showStoreData.exitCallbackUserData = null;
		m_showStoreData.isTotallyFake = isTotallyFake;
		m_showStoreData.storeProduct = ProductType.PRODUCT_TYPE_UNKNOWN;
		m_showStoreData.storeProductData = 0;
		m_showStoreData.useOverlayUI = true;
		m_showStoreData.closeOnTransactionComplete = false;
		ShowStoreWhenLoaded();
	}

	public void StartArenaTransaction(Store.ExitCallback exitCallback, object exitCallbackUserData, bool isTotallyFake)
	{
		if (m_waitingToShowStore)
		{
			Log.Store.Print("StoreManager.StartArenaTransaction(): already waiting to show store");
			return;
		}
		m_currentShopType = ShopType.ARENA_STORE;
		m_showStoreData.exitCallback = exitCallback;
		m_showStoreData.exitCallbackUserData = null;
		m_showStoreData.isTotallyFake = isTotallyFake;
		m_showStoreData.storeProduct = ProductType.PRODUCT_TYPE_UNKNOWN;
		m_showStoreData.storeProductData = 0;
		m_showStoreData.useOverlayUI = false;
		m_showStoreData.closeOnTransactionComplete = false;
		ShowStoreWhenLoaded();
	}

	public void StartTavernBrawlTransaction(Store.ExitCallback exitCallback, bool isTotallyFake)
	{
		if (m_waitingToShowStore)
		{
			Log.Store.Print("StoreManager.StartTavernBrawlTransaction(): already waiting to show store");
			return;
		}
		m_currentShopType = ShopType.TAVERN_BRAWL_STORE;
		m_showStoreData.exitCallback = exitCallback;
		m_showStoreData.exitCallbackUserData = null;
		m_showStoreData.isTotallyFake = isTotallyFake;
		m_showStoreData.storeProduct = ProductType.PRODUCT_TYPE_UNKNOWN;
		m_showStoreData.storeProductData = 0;
		m_showStoreData.useOverlayUI = false;
		m_showStoreData.closeOnTransactionComplete = false;
		ShowStoreWhenLoaded();
	}

	public void StartAdventureTransaction(ProductType product, int productData, Store.ExitCallback exitCallback, object exitCallbackUserData, ShopType shopType, int numItemsRequired = 0, bool useOverlayUI = false, IDataModel dataModel = null, int pmtProductId = 0)
	{
		if (m_waitingToShowStore)
		{
			Log.Store.Print("StoreManager.StartAdventureTransaction(): already waiting to show store");
			return;
		}
		if (!CanBuyProductItem(product, productData, InferProductItemPurchaseRuleFromProductType(product)))
		{
			Log.Store.PrintWarning("StoreManager.StartAdventureTransaction(): cannot buy product item");
			return;
		}
		m_currentShopType = shopType;
		m_showStoreData.exitCallback = exitCallback;
		m_showStoreData.exitCallbackUserData = exitCallbackUserData;
		m_showStoreData.isTotallyFake = false;
		m_showStoreData.storeProduct = product;
		m_showStoreData.storeProductData = productData;
		m_showStoreData.numItemsRequired = numItemsRequired;
		m_showStoreData.dataModel = dataModel;
		m_showStoreData.useOverlayUI = useOverlayUI;
		m_showStoreData.pmtProductId = pmtProductId;
		m_showStoreData.closeOnTransactionComplete = false;
		ShowStoreWhenLoaded();
	}

	public void StartFakeStoreForMercenariesWorkshop()
	{
		m_showStoreData.storeProductData = (int)m_currentShopType;
		m_currentShopType = ShopType.MERCENARIES_WORKSHOP;
		m_showStoreData.isTotallyFake = false;
		m_showStoreData.storeProduct = ProductType.PRODUCT_TYPE_UNKNOWN;
		m_showStoreData.storeProductData = 0;
		m_showStoreData.useOverlayUI = false;
		m_showStoreData.closeOnTransactionComplete = true;
	}

	public void StopFakeMercenariesWorkshopStoreAndRestorePrevious()
	{
		m_currentShopType = (ShopType)m_showStoreData.storeProductData;
	}

	public void SetupCardBackStore(CardBackInfoManager cardBackInfoManager, int productData)
	{
		m_currentShopType = ShopType.CARD_BACK_STORE;
		m_showStoreData.isTotallyFake = false;
		m_showStoreData.storeProduct = ProductType.PRODUCT_TYPE_CARD_BACK;
		m_showStoreData.storeProductData = productData;
		m_showStoreData.useOverlayUI = false;
		m_showStoreData.closeOnTransactionComplete = true;
		m_showStoreData.exitCallback = null;
		m_showStoreData.exitCallbackUserData = null;
		m_stores[ShopType.CARD_BACK_STORE] = cardBackInfoManager;
		SetupLoadedStore(cardBackInfoManager);
		if (m_view.HasStartedLoading)
		{
			ShowStore();
			return;
		}
		m_showStoreStart = Time.realtimeSinceStartup;
		m_waitingToShowStore = true;
		m_view.LoadAssets();
	}

	public void ShutDownCardBackStore()
	{
		if (m_stores.ContainsKey(ShopType.CARD_BACK_STORE))
		{
			m_stores.Remove(ShopType.CARD_BACK_STORE);
		}
	}

	public void SetupHeroSkinStore(HeroSkinInfoManager heroSkinInfoManager, int productData)
	{
		m_currentShopType = ShopType.HERO_SKIN_STORE;
		m_showStoreData.isTotallyFake = false;
		m_showStoreData.storeProduct = ProductType.PRODUCT_TYPE_HERO;
		m_showStoreData.storeProductData = productData;
		m_showStoreData.useOverlayUI = false;
		m_showStoreData.closeOnTransactionComplete = true;
		m_showStoreData.exitCallback = null;
		m_showStoreData.exitCallbackUserData = null;
		m_stores[ShopType.HERO_SKIN_STORE] = heroSkinInfoManager;
		SetupLoadedStore(heroSkinInfoManager);
		if (m_view.HasStartedLoading)
		{
			ShowStore();
			return;
		}
		m_showStoreStart = Time.realtimeSinceStartup;
		m_waitingToShowStore = true;
		m_view.LoadAssets();
	}

	public void ShutDownHeroSkinStore()
	{
		if (m_stores.ContainsKey(ShopType.HERO_SKIN_STORE))
		{
			m_stores.Remove(ShopType.HERO_SKIN_STORE);
		}
	}

	public void StartLuckyDrawStore(LuckyDrawWidget luckyDrawWidget)
	{
		m_currentShopType = ShopType.LUCKY_DRAW_STORE;
		m_showStoreData.isTotallyFake = false;
		m_showStoreData.storeProduct = ProductType.PRODUCT_TYPE_LUCKY_DRAW;
		m_showStoreData.useOverlayUI = true;
		m_showStoreData.closeOnTransactionComplete = false;
		m_showStoreData.exitCallback = null;
		m_showStoreData.exitCallbackUserData = null;
		m_stores[ShopType.LUCKY_DRAW_STORE] = luckyDrawWidget;
		SetupLoadedStore(luckyDrawWidget);
		Status = TransactionStatus.READY;
		if (m_view.HasStartedLoading)
		{
			ShowStore();
			return;
		}
		m_showStoreStart = Time.realtimeSinceStartup;
		m_waitingToShowStore = true;
		m_view.LoadAssets();
	}

	public void ShutDownLuckyDrawStore()
	{
		if (m_stores.ContainsKey(ShopType.LUCKY_DRAW_STORE))
		{
			m_stores.Remove(ShopType.LUCKY_DRAW_STORE);
		}
	}

	public void HandleDisconnect()
	{
		if (IsShown() && !TransactionInProgress())
		{
			while (IsPromptShowing)
			{
				Navigation.GoBack();
			}
			GetCurrentStore()?.Close();
			DialogManager.Get().ShowReconnectHelperDialog();
		}
		FireStatusChangedEventIfNeeded();
	}

	public void HideStore(ShopType shopType)
	{
		IStore store = GetStore(shopType);
		if (store != null)
		{
			store.Close();
			m_view.Hide();
			BnetBar.Get()?.RefreshCurrency();
		}
	}

	public bool TransactionInProgress()
	{
		if (Status != TransactionStatus.READY)
		{
			return true;
		}
		return false;
	}

	public bool HasOutstandingPurchaseNotices(ProductType product)
	{
		return (from bundle in (from notice in m_outstandingPurchaseNotices
				where notice.PMTProductID.HasValue
				select ProductId.CreateFrom(notice.PMTProductID.Value) into prodId
				where prodId.IsValid()
				select prodId).Select(GetBundleFromPmtProductId)
			where bundle != null
			select bundle).SelectMany((ProductInfo bundle) => bundle.Items).Any((Network.BundleItem item) => item.ItemType == product);
	}

	public static ProductType GetAdventureProductType(AdventureDbId adventureId)
	{
		if (s_adventureToProductMap.TryGetValue(adventureId, out var productType))
		{
			return productType;
		}
		if (GameUtils.IsExpansionAdventure(adventureId))
		{
			return ProductType.PRODUCT_TYPE_WING;
		}
		return ProductType.PRODUCT_TYPE_UNKNOWN;
	}

	public static bool IsFirstPurchaseBundleOwned()
	{
		HiddenLicenseDbfRecord hiddenLicenseRecord = GameDbf.HiddenLicense.GetRecord(40);
		if (hiddenLicenseRecord == null)
		{
			return false;
		}
		AccountLicenseDbfRecord accountLicenseRecord = GameDbf.AccountLicense.GetRecord(hiddenLicenseRecord.AccountLicenseId);
		if (accountLicenseRecord == null)
		{
			return false;
		}
		return AccountLicenseMgr.Get().OwnsAccountLicense(accountLicenseRecord.LicenseId);
	}

	private void OnAccountLicensesUpdate(List<AccountLicenseInfo> changedAccountLicenses, object userData)
	{
		if (ServiceManager.TryGet<IProductDataService>(out var dataService))
		{
			dataService.UpdateProductStatus();
		}
	}

	private static LicenseStatus GetHiddenLicenseStatus(int hiddenLicenseId)
	{
		HiddenLicenseDbfRecord hiddenLicenseRecord = GameDbf.HiddenLicense.GetRecord(hiddenLicenseId);
		if (hiddenLicenseRecord == null)
		{
			return LicenseStatus.UNDEFINED;
		}
		AccountLicenseDbfRecord accountLicenseRecord = GameDbf.AccountLicense.GetRecord(hiddenLicenseRecord.AccountLicenseId);
		if (accountLicenseRecord == null)
		{
			return LicenseStatus.UNDEFINED;
		}
		if (AccountLicenseMgr.Get().OwnsAccountLicense(accountLicenseRecord.LicenseId))
		{
			if (!hiddenLicenseRecord.IsBlocking)
			{
				return LicenseStatus.OWNED;
			}
			return LicenseStatus.OWNED_AND_BLOCKING;
		}
		return LicenseStatus.NOT_OWNED;
	}

	private void ShowStoreWhenLoaded()
	{
		m_showStoreStart = Time.realtimeSinceStartup;
		HearthstonePerformance.Get()?.StartPerformanceFlow(new FlowPerformanceShop.ShopSetupConfig
		{
			shopType = m_currentShopType
		});
		m_waitingToShowStore = true;
		if (!IsCurrentStoreLoaded())
		{
			Load(m_currentShopType);
		}
		else
		{
			ShowStore();
		}
	}

	private void ShowStore()
	{
		if (!m_licenseAchievesListenerRegistered)
		{
			AchieveManager.Get().RegisterLicenseAddedAchievesUpdatedListener(OnLicenseAddedAchievesUpdated);
			m_licenseAchievesListenerRegistered = true;
		}
		if (TransactionStatus.READY == Status && AchieveManager.Get().HasActiveLicenseAddedAchieves())
		{
			Status = TransactionStatus.WAIT_ZERO_COST_LICENSE;
		}
		IStore currentStore = GetCurrentStore();
		bool showStore = true;
		bool showStoreClosedAlert = false;
		switch (m_currentShopType)
		{
		case ShopType.GENERAL_STORE:
			if (!IsOpen())
			{
				Log.Store.PrintWarning("StoreManager.ShowStore(): Cannot show general store.. Store is not open");
				if (m_showStoreData.exitCallback != null)
				{
					m_showStoreData.exitCallback(authorizationBackButtonPressed: false, m_showStoreData.exitCallbackUserData);
				}
				showStore = false;
			}
			break;
		case ShopType.ADVENTURE_STORE:
		case ShopType.ADVENTURE_STORE_WING_PURCHASE_WIDGET:
		case ShopType.ADVENTURE_STORE_FULL_PURCHASE_WIDGET:
			if (IsOpen())
			{
				AdventureStore adventureStore = (AdventureStore)currentStore;
				if (adventureStore != null)
				{
					ProductId productId = ProductId.CreateFrom(m_showStoreData.pmtProductId);
					adventureStore.SetAdventureProduct(m_showStoreData.storeProduct, m_showStoreData.storeProductData, m_showStoreData.numItemsRequired, productId);
				}
				break;
			}
			Log.Store.PrintWarning("StoreManager.ShowStore(): Cannot show adventure store.. Store is not open");
			if (m_showStoreData.exitCallback != null)
			{
				m_showStoreData.exitCallback(authorizationBackButtonPressed: false, m_showStoreData.exitCallbackUserData);
			}
			showStore = false;
			showStoreClosedAlert = true;
			break;
		case ShopType.MERCENARIES_WORKSHOP:
			if (!IsOpen())
			{
				showStore = false;
			}
			break;
		case ShopType.LUCKY_DRAW_STORE:
			if (!IsOpen())
			{
				showStore = false;
			}
			break;
		}
		if (showStoreClosedAlert)
		{
			ShowStoreUnavailableAlert();
			m_waitingToShowStore = false;
			return;
		}
		bool shouldBlockInterface = false;
		if (showStore && currentStore != null)
		{
			if (currentStore is Store store)
			{
				store.Show(m_showStoreData.isTotallyFake, m_showStoreData.useOverlayUI, m_showStoreData.dataModel);
			}
			else
			{
				currentStore.Open();
				if (m_currentShopType == ShopType.GENERAL_STORE && currentStore is Shop shop)
				{
					if (ServiceManager.TryGet<IProductDataService>(out var pds) && pds.TryFindTabWithId(m_showStoreData.TargetTabId, out var parentTab, out var subTab))
					{
						if ((parentTab != null && !parentTab.Enabled) || parentTab.Locked || (subTab != null && !subTab.Enabled) || (subTab != null && subTab.Locked))
						{
							Log.Store.PrintWarning("StoreManager.ShowStore(): Attempted to open locked " + m_showStoreData.TargetTabId + "...");
						}
						else
						{
							shop.MoveToTab(parentTab, subTab, isImmediate: true);
						}
					}
					else
					{
						Log.Store.PrintWarning("StoreManager.ShowStore(): Cannot show tab " + m_showStoreData.TargetTabId + ".. opening to default...");
					}
				}
			}
		}
		currentStore?.BlockInterface(shouldBlockInterface);
		Log.Store.Print("{0} took {1}s to load", m_currentShopType, Time.realtimeSinceStartup - m_showStoreStart);
		m_waitingToShowStore = false;
	}

	private void ShowStoreUnavailableAlert()
	{
		if (!m_isShowingStoreUnavailableAlert)
		{
			AlertPopup.PopupInfo info = new AlertPopup.PopupInfo
			{
				m_headerText = GameStrings.Get("GLUE_SHOP_CLOSED_ALERT_HEADER"),
				m_text = GameStrings.Get("GLUE_SHOP_CLOSED_ALERT_TEXT"),
				m_showAlertIcon = false,
				m_responseDisplay = AlertPopup.ResponseDisplay.OK,
				m_responseCallback = delegate
				{
					m_isShowingStoreUnavailableAlert = false;
				}
			};
			DialogManager.Get().ShowPopup(info);
			m_isShowingStoreUnavailableAlert = true;
		}
	}

	private void OnLoginCompleted()
	{
		FireStatusChangedEventIfNeeded();
	}

	public void HandleShopAvailabilityChange()
	{
		if (!IsOpen() && IsShown() && !TransactionInProgress())
		{
			while (IsPromptShowing)
			{
				Navigation.GoBack();
			}
			GetCurrentStore()?.Close();
			ShowStoreUnavailableAlert();
		}
		FireStatusChangedEventIfNeeded();
	}

	private StorePurchaseAuth.ButtonStyle GetPurchaseAuthButtonStyle(ShopType shopType)
	{
		ShopType currentShopType = m_currentShopType;
		if (currentShopType == ShopType.ARENA_STORE || currentShopType == ShopType.TAVERN_BRAWL_STORE)
		{
			return StorePurchaseAuth.ButtonStyle.Back;
		}
		return StorePurchaseAuth.ButtonStyle.NoButton;
	}

	private bool IsCurrentStoreLoaded()
	{
		IStore currentStore = GetCurrentStore();
		if (currentStore == null || !currentStore.IsReady())
		{
			return false;
		}
		if (!m_view.IsLoaded())
		{
			return false;
		}
		return true;
	}

	private void Load(ShopType shopType)
	{
		bool loadAssets = true;
		if (GetCurrentStore() != null)
		{
			return;
		}
		switch (shopType)
		{
		case ShopType.GENERAL_STORE:
		{
			CollectionManager cm = CollectionManager.Get();
			if (cm.IsLettuceLoaded())
			{
				LoadGeneralStore();
				break;
			}
			cm.OnLettuceLoaded += OnLettuceCollectionLoaded;
			cm.StartInitialMercenaryLoadIfRequired();
			loadAssets = false;
			break;
		}
		case ShopType.ARENA_STORE:
		{
			WidgetInstance arenaStoreWidget = WidgetInstance.Create(ShopPrefabs.ArenaShopPrefab);
			arenaStoreWidget.RegisterReadyListener(delegate
			{
				OnArenaStoreLoaded(null, arenaStoreWidget.gameObject, null);
			});
			break;
		}
		case ShopType.TAVERN_BRAWL_STORE:
		{
			WidgetInstance brawlStoreWidget = WidgetInstance.Create(ShopPrefabs.TavernBrawlShopPrefab);
			brawlStoreWidget.RegisterReadyListener(delegate
			{
				OnBrawlStoreLoaded(null, brawlStoreWidget.gameObject, null);
			});
			break;
		}
		case ShopType.ADVENTURE_STORE:
		{
			WidgetInstance adventureStoreWidget = WidgetInstance.Create(ShopPrefabs.AdventureShopPrefab);
			adventureStoreWidget.RegisterReadyListener(delegate
			{
				OnAdventureStoreLoaded(null, adventureStoreWidget.gameObject, null);
			});
			break;
		}
		case ShopType.ADVENTURE_STORE_WING_PURCHASE_WIDGET:
		{
			WidgetInstance wingWidget = WidgetInstance.Create("AdventureStorymodeChapterStore.prefab:b797807e5c127af47badd08be121ea16");
			wingWidget.RegisterReadyListener(delegate
			{
				OnAdventureWingStoreLoaded(null, wingWidget.gameObject, null);
			});
			break;
		}
		case ShopType.ADVENTURE_STORE_FULL_PURCHASE_WIDGET:
		{
			WidgetInstance bookWidget = WidgetInstance.Create("AdventureStorymodeBookStore.prefab:922203a90d48c1d47b2f6813ff72f160");
			bookWidget.RegisterReadyListener(delegate
			{
				OnAdventureFullStoreLoaded(null, bookWidget.gameObject, null);
			});
			break;
		}
		}
		if (loadAssets)
		{
			m_view.LoadAssets();
		}
	}

	private void LoadGeneralStore()
	{
		Shop generalStore = Shop.Get();
		m_stores[ShopType.GENERAL_STORE] = generalStore;
	}

	public void UnloadAndFreeMemory()
	{
		if (Shop.Get() != null)
		{
			Shop.Get().Unload();
		}
		foreach (KeyValuePair<ShopType, IStore> store in m_stores)
		{
			store.Value?.Unload();
		}
		m_stores.Clear();
		m_view.UnloadAssets();
	}

	private void FireStatusChangedEventIfNeeded()
	{
		bool isOpen = IsOpen();
		if (m_openWhenLastEventFired != isOpen)
		{
			this.OnStatusChanged(isOpen);
			m_openWhenLastEventFired = isOpen;
		}
	}

	private NetCache.NetCacheFeatures GetNetCacheFeatures()
	{
		if (!FeaturesReady)
		{
			return null;
		}
		NetCache.NetCacheFeatures netObject = NetCache.Get().GetNetObject<NetCache.NetCacheFeatures>();
		if (netObject == null)
		{
			FeaturesReady = false;
		}
		return netObject;
	}

	private static ItemPurchaseRule GetProductItemPurchaseRule(Network.BundleItem item)
	{
		if (!item.IsBlocking)
		{
			return ItemPurchaseRule.NO_LIMIT;
		}
		return ItemPurchaseRule.BLOCKING;
	}

	private static ItemPurchaseRule InferProductItemPurchaseRuleFromProductType(ProductType product)
	{
		switch (product)
		{
		case ProductType.PRODUCT_TYPE_BOOSTER:
		case ProductType.PRODUCT_TYPE_DRAFT:
		case ProductType.PRODUCT_TYPE_RANDOM_CARD:
		case ProductType.PRODUCT_TYPE_TAVERN_BRAWL_TICKET:
		case ProductType.PRODUCT_TYPE_CURRENCY:
		case ProductType.PRODUCT_TYPE_MERCENARIES_MERCENARY:
		case ProductType.PRODUCT_TYPE_MERCENARIES_CURRENCY:
		case ProductType.PRODUCT_TYPE_MERCENARIES_BOOSTER:
		case ProductType.PRODUCT_TYPE_MERCENARIES_RANDOM_REWARD:
		case ProductType.PRODUCT_TYPE_MERCENARIES_KNOCKOUT_SPECIFIC:
		case ProductType.PRODUCT_TYPE_MERCENARIES_KNOCKOUT_RANDOM:
			return ItemPurchaseRule.NO_LIMIT;
		case ProductType.PRODUCT_TYPE_CARD_BACK:
		case ProductType.PRODUCT_TYPE_HERO:
		case ProductType.PRODUCT_TYPE_FIXED_LICENSE:
		case ProductType.PRODUCT_TYPE_MINI_SET:
		case ProductType.PRODUCT_TYPE_SELLABLE_DECK:
		case ProductType.PRODUCT_TYPE_BATTLEGROUNDS_BOARD_SKIN:
		case ProductType.PRODUCT_TYPE_BATTLEGROUNDS_FINISHER:
		case ProductType.PRODUCT_TYPE_DIAMOND_CARD:
		case ProductType.PRODUCT_TYPE_BATTLEGROUNDS_EMOTE:
		case ProductType.PRODUCT_TYPE_LUCKY_DRAW:
		case ProductType.PRODUCT_TYPE_SPECIFIC_CARD:
			return ItemPurchaseRule.NO_LIMIT;
		case ProductType.PRODUCT_TYPE_NAXX:
		case ProductType.PRODUCT_TYPE_BRM:
		case ProductType.PRODUCT_TYPE_LOE:
		case ProductType.PRODUCT_TYPE_WING:
		case ProductType.PRODUCT_TYPE_BATTLEGROUNDS_BONUS:
		case ProductType.PRODUCT_TYPE_PROGRESSION_BONUS:
			return ItemPurchaseRule.BLOCKING;
		case ProductType.PRODUCT_TYPE_HIDDEN_LICENSE:
			return ItemPurchaseRule.BLOCKING;
		default:
			return ItemPurchaseRule.UNDEFINED;
		}
	}

	public static ItemOwnershipStatus GetStaticProductItemOwnershipStatus(ProductType product, int productData, out string failReason)
	{
		failReason = null;
		switch (product)
		{
		case ProductType.PRODUCT_TYPE_BOOSTER:
		case ProductType.PRODUCT_TYPE_DRAFT:
		case ProductType.PRODUCT_TYPE_RANDOM_CARD:
		case ProductType.PRODUCT_TYPE_TAVERN_BRAWL_TICKET:
		case ProductType.PRODUCT_TYPE_CURRENCY:
		case ProductType.PRODUCT_TYPE_MINI_SET:
		case ProductType.PRODUCT_TYPE_SELLABLE_DECK:
		case ProductType.PRODUCT_TYPE_MERCENARIES_MERCENARY:
		case ProductType.PRODUCT_TYPE_MERCENARIES_CURRENCY:
		case ProductType.PRODUCT_TYPE_MERCENARIES_BOOSTER:
		case ProductType.PRODUCT_TYPE_MERCENARIES_RANDOM_REWARD:
		case ProductType.PRODUCT_TYPE_MERCENARIES_KNOCKOUT_SPECIFIC:
		case ProductType.PRODUCT_TYPE_MERCENARIES_KNOCKOUT_RANDOM:
		case ProductType.PRODUCT_TYPE_SPECIFIC_CARD:
			return ItemOwnershipStatus.IGNORED;
		case ProductType.PRODUCT_TYPE_CARD_BACK:
			if (!CardBackManager.Get().IsCardBackOwned(productData))
			{
				return ItemOwnershipStatus.UNOWNED;
			}
			return ItemOwnershipStatus.OWNED;
		case ProductType.PRODUCT_TYPE_NAXX:
		case ProductType.PRODUCT_TYPE_BRM:
		case ProductType.PRODUCT_TYPE_LOE:
		case ProductType.PRODUCT_TYPE_WING:
			if (!AdventureProgressMgr.Get().IsReady)
			{
				failReason = $"Adventure Progress Manager not ready to determine ownership of chapter Type={product}, ID={productData}";
				return ItemOwnershipStatus.UNDEFINED;
			}
			if (!AdventureProgressMgr.Get().OwnsWing(productData))
			{
				return ItemOwnershipStatus.UNOWNED;
			}
			return ItemOwnershipStatus.OWNED;
		case ProductType.PRODUCT_TYPE_DIAMOND_CARD:
		{
			if (NetCache.Get().GetNetObject<NetCache.NetCacheCollection>() == null)
			{
				failReason = $"Collection not received to determine ownership of diamond card ID={productData}";
				return ItemOwnershipStatus.UNDEFINED;
			}
			string cardID = GameUtils.TranslateDbIdToCardId(productData);
			if (cardID == null || !CollectionManager.Get().IsCardInCollection(cardID, TAG_PREMIUM.DIAMOND))
			{
				return ItemOwnershipStatus.UNOWNED;
			}
			return ItemOwnershipStatus.OWNED;
		}
		case ProductType.PRODUCT_TYPE_HERO:
		{
			if (NetCache.Get().GetNetObject<NetCache.NetCacheCollection>() == null)
			{
				failReason = $"Collection not received to determine ownership of hero card ID={productData}";
				return ItemOwnershipStatus.UNDEFINED;
			}
			string cardID2 = GameUtils.TranslateDbIdToCardId(productData);
			if (!(CollectionManager.Get().IsBattlegroundsHeroSkinCard(productData) ? CollectionManager.Get().OwnsBattlegroundsHeroSkin(cardID2) : ((!CollectionManager.Get().IsBattlegroundsGuideSkinCard(productData)) ? (cardID2 != null && CollectionManager.Get().IsCardInCollection(cardID2, TAG_PREMIUM.NORMAL)) : CollectionManager.Get().OwnsBattlegroundsGuideSkin(productData))))
			{
				return ItemOwnershipStatus.UNOWNED;
			}
			return ItemOwnershipStatus.OWNED;
		}
		case ProductType.PRODUCT_TYPE_HIDDEN_LICENSE:
		{
			if (AccountLicenseMgr.Get().FixedLicensesState != AccountLicenseMgr.LicenseUpdateState.SUCCESS)
			{
				failReason = $"Fixed licenses not received to determine ownership of hidden license ID={productData}";
				return ItemOwnershipStatus.UNDEFINED;
			}
			HiddenLicenseDbfRecord hiddenLicenseRecord = GameDbf.HiddenLicense.GetRecord(productData);
			if (hiddenLicenseRecord == null)
			{
				failReason = $"Hidden license has unknown ID in HIDDEN_LICENSE table record ID={productData}";
				return ItemOwnershipStatus.UNDEFINED;
			}
			if (GameDbf.AccountLicense.GetRecord(hiddenLicenseRecord.AccountLicenseId) == null)
			{
				failReason = $"HIDDEN_LICENSE record {productData} pointing to missing ACCOUNT_LICENSE record ID={hiddenLicenseRecord.AccountLicenseId}";
				return ItemOwnershipStatus.UNDEFINED;
			}
			switch (GetHiddenLicenseStatus(productData))
			{
			case LicenseStatus.NOT_OWNED:
				return ItemOwnershipStatus.UNOWNED;
			case LicenseStatus.OWNED:
			case LicenseStatus.OWNED_AND_BLOCKING:
				return ItemOwnershipStatus.OWNED;
			default:
				failReason = $"Hidden license has undefined ownership status. HIDDEN_LIENSE table record ID={productData}";
				return ItemOwnershipStatus.UNDEFINED;
			}
		}
		case ProductType.PRODUCT_TYPE_FIXED_LICENSE:
			if (AccountLicenseMgr.Get().FixedLicensesState != AccountLicenseMgr.LicenseUpdateState.SUCCESS)
			{
				failReason = $"Fixed licenses not received to determine ownership of ACCOUNT_LICENSE table record ID={productData} for license type={product}";
				return ItemOwnershipStatus.UNDEFINED;
			}
			if (!AccountLicenseMgr.Get().OwnsAccountLicense(productData))
			{
				return ItemOwnershipStatus.UNOWNED;
			}
			return ItemOwnershipStatus.OWNED;
		case ProductType.PRODUCT_TYPE_BATTLEGROUNDS_BONUS:
		case ProductType.PRODUCT_TYPE_PROGRESSION_BONUS:
		case ProductType.PRODUCT_TYPE_LUCKY_DRAW:
		{
			if (AccountLicenseMgr.Get().FixedLicensesState != AccountLicenseMgr.LicenseUpdateState.SUCCESS)
			{
				failReason = $"Fixed licenses not received to determine ownership of ACCOUNT_LICENSE table record ID={productData} for license type={product}";
				return ItemOwnershipStatus.UNDEFINED;
			}
			AccountLicenseDbfRecord accountLicenseRecord = GameDbf.AccountLicense.GetRecord(productData);
			if (accountLicenseRecord == null)
			{
				failReason = $"Fixed licenses not received to determine ownership of ACCOUNT_LICENSE table record ID={productData} for license type={product}";
				return ItemOwnershipStatus.UNDEFINED;
			}
			if (!AccountLicenseMgr.Get().OwnsAccountLicense(accountLicenseRecord.LicenseId))
			{
				return ItemOwnershipStatus.UNOWNED;
			}
			return ItemOwnershipStatus.OWNED;
		}
		case ProductType.PRODUCT_TYPE_BATTLEGROUNDS_BOARD_SKIN:
		{
			BattlegroundsBoardSkinId skinId = BattlegroundsBoardSkinId.FromTrustedValue(productData);
			if (CollectionManager.Get().IsValidBattlegroundsBoardSkinId(skinId))
			{
				if (!CollectionManager.Get().OwnsBattlegroundsBoardSkin(skinId))
				{
					return ItemOwnershipStatus.UNOWNED;
				}
				return ItemOwnershipStatus.OWNED;
			}
			return ItemOwnershipStatus.UNDEFINED;
		}
		case ProductType.PRODUCT_TYPE_BATTLEGROUNDS_FINISHER:
		{
			BattlegroundsFinisherId finisherId = BattlegroundsFinisherId.FromTrustedValue(productData);
			if (CollectionManager.Get().IsValidBattlegroundsFinisherId(finisherId))
			{
				if (!CollectionManager.Get().OwnsBattlegroundsFinisher(finisherId))
				{
					return ItemOwnershipStatus.UNOWNED;
				}
				return ItemOwnershipStatus.OWNED;
			}
			return ItemOwnershipStatus.UNDEFINED;
		}
		case ProductType.PRODUCT_TYPE_BATTLEGROUNDS_EMOTE:
		{
			BattlegroundsEmoteId emoteId = BattlegroundsEmoteId.FromTrustedValue(productData);
			if (CollectionManager.Get().IsValidBattlegroundsEmoteId(emoteId))
			{
				if (!CollectionManager.Get().OwnsBattlegroundsEmote(emoteId))
				{
					return ItemOwnershipStatus.UNOWNED;
				}
				return ItemOwnershipStatus.OWNED;
			}
			return ItemOwnershipStatus.UNDEFINED;
		}
		default:
			failReason = $"Ownership status cannot be determined from license type {product}";
			return ItemOwnershipStatus.UNDEFINED;
		}
	}

	private string GetSingleItemProductName(Network.BundleItem item)
	{
		string productName = string.Empty;
		switch (item.ItemType)
		{
		case ProductType.PRODUCT_TYPE_BOOSTER:
		{
			string boosterName = GameDbf.Booster.GetRecord(item.ProductData).Name;
			productName = GameStrings.Format("GLUE_STORE_PRODUCT_NAME_PACK", item.Quantity, boosterName);
			break;
		}
		case ProductType.PRODUCT_TYPE_DRAFT:
			productName = GameStrings.Get("GLUE_STORE_PRODUCT_NAME_FORGE_TICKET");
			break;
		case ProductType.PRODUCT_TYPE_BATTLEGROUNDS_BONUS:
			productName = GameStrings.Get("GLUE_STORE_PRODUCT_NAME_BATTLEGROUNDS_BONUS");
			break;
		case ProductType.PRODUCT_TYPE_PROGRESSION_BONUS:
			productName = GameStrings.Get("GLUE_STORE_PRODUCT_NAME_PROGRESSION_BONUS");
			break;
		case ProductType.PRODUCT_TYPE_NAXX:
		case ProductType.PRODUCT_TYPE_BRM:
		case ProductType.PRODUCT_TYPE_LOE:
		case ProductType.PRODUCT_TYPE_WING:
			productName = AdventureProgressMgr.GetWingName(item.ProductData);
			break;
		case ProductType.PRODUCT_TYPE_CARD_BACK:
		{
			CardBackDbfRecord cardBackDbfRecord = GameDbf.CardBack.GetRecord(item.ProductData);
			if (cardBackDbfRecord != null)
			{
				productName = cardBackDbfRecord.Name;
			}
			break;
		}
		case ProductType.PRODUCT_TYPE_HERO:
		{
			EntityDef heroEntityDef = DefLoader.Get().GetEntityDef(item.ProductData);
			if (heroEntityDef != null)
			{
				productName = heroEntityDef.GetName();
			}
			break;
		}
		case ProductType.PRODUCT_TYPE_TAVERN_BRAWL_TICKET:
		{
			TavernBrawlTicketDbfRecord tbTicketRecord = GameDbf.TavernBrawlTicket.GetRecord(item.ProductData);
			if (tbTicketRecord != null)
			{
				productName = tbTicketRecord.StoreName;
			}
			break;
		}
		case ProductType.PRODUCT_TYPE_LUCKY_DRAW:
			productName = "Battle Bash";
			break;
		default:
			Log.Store.PrintWarning($"StoreManager.GetSingleItemProductName(): don't know how to format name for bundle product {item.ItemType}");
			break;
		}
		return productName;
	}

	private string GetMultiItemProductName(ProductInfo bundle)
	{
		HashSet<ProductType> includedProducts = GetProductsInItemList(bundle.Items);
		if (includedProducts.Contains(ProductType.PRODUCT_TYPE_NAXX))
		{
			return GameStrings.Format("GLUE_STORE_PRODUCT_NAME_NAXX_WING_BUNDLE", bundle.Items.Count);
		}
		if (includedProducts.Contains(ProductType.PRODUCT_TYPE_BRM))
		{
			if (includedProducts.Contains(ProductType.PRODUCT_TYPE_CARD_BACK))
			{
				return GameStrings.Get("GLUE_STORE_PRODUCT_NAME_BRM_PRESALE_BUNDLE");
			}
			return GameStrings.Format("GLUE_STORE_PRODUCT_NAME_BRM_WING_BUNDLE", bundle.Items.Count);
		}
		if (includedProducts.Contains(ProductType.PRODUCT_TYPE_LOE))
		{
			return GameStrings.Format("GLUE_STORE_PRODUCT_NAME_LOE_WING_BUNDLE", bundle.Items.Count);
		}
		if (includedProducts.Contains(ProductType.PRODUCT_TYPE_WING))
		{
			int num = (from r in bundle.Items
				where r.ItemType == ProductType.PRODUCT_TYPE_WING
				select r.ProductData).FirstOrDefault();
			if (num == 0)
			{
				Log.Store.PrintError("StoreManager.GetMultiItemProductName: bundle with PRODUCT_TYPE_WING did not contain a valid wing ID in any of its product data.");
			}
			string productStringKey = GameUtils.GetAdventureProductStringKey(num);
			if (includedProducts.Contains(ProductType.PRODUCT_TYPE_CARD_BACK))
			{
				return GameStrings.Get("GLUE_STORE_PRODUCT_NAME_" + productStringKey + "_PRESALE_BUNDLE");
			}
			int itemCount = bundle.Items.Count((Network.BundleItem x) => x.ItemType == ProductType.PRODUCT_TYPE_WING);
			return GameStrings.Format("GLUE_STORE_PRODUCT_NAME_" + productStringKey + "_WING_BUNDLE", itemCount);
		}
		if (includedProducts.Contains(ProductType.PRODUCT_TYPE_HIDDEN_LICENSE))
		{
			Network.BundleItem hiddenLicenseItem = bundle.Items.Find((Network.BundleItem obj) => obj.ItemType == ProductType.PRODUCT_TYPE_HIDDEN_LICENSE);
			if (hiddenLicenseItem.ProductData == 40)
			{
				return GameStrings.Get("GLUE_STORE_PRODUCT_NAME_FIRST_PURCHASE_BUNDLE");
			}
			if (hiddenLicenseItem.ProductData == 27)
			{
				return GameStrings.Get("GLUE_STORE_PRODUCT_NAME_MAMMOTH_BUNDLE");
			}
		}
		if (includedProducts.Contains(ProductType.PRODUCT_TYPE_HERO))
		{
			Network.BundleItem heroBundleItem = bundle.Items.Find((Network.BundleItem obj) => obj.ItemType == ProductType.PRODUCT_TYPE_HERO);
			if (heroBundleItem != null)
			{
				return GetSingleItemProductName(heroBundleItem);
			}
		}
		else if (includedProducts.Contains(ProductType.PRODUCT_TYPE_BOOSTER) && includedProducts.Contains(ProductType.PRODUCT_TYPE_CARD_BACK))
		{
			if (bundle.Items.Find((Network.BundleItem obj) => obj.ItemType == ProductType.PRODUCT_TYPE_BOOSTER && obj.ProductData == 10) != null)
			{
				return GameStrings.Get("GLUE_STORE_PRODUCT_NAME_TGT_PRESALE_BUNDLE");
			}
			if (bundle.Items.Find((Network.BundleItem obj) => obj.ItemType == ProductType.PRODUCT_TYPE_BOOSTER && obj.ProductData == 11) != null)
			{
				return GameStrings.Get("GLUE_STORE_PRODUCT_NAME_OG_PRESALE_BUNDLE");
			}
			if (bundle.Items.Find((Network.BundleItem obj) => obj.ItemType == ProductType.PRODUCT_TYPE_BOOSTER && obj.ProductData == 20) != null)
			{
				return GameStrings.Get("GLUE_STORE_PRODUCT_NAME_GORO_PRESALE_BUNDLE");
			}
			if (bundle.Items.Find((Network.BundleItem obj) => obj.ItemType == ProductType.PRODUCT_TYPE_BOOSTER && obj.ProductData == 21) != null)
			{
				return GameStrings.Get("GLUE_STORE_PRODUCT_NAME_ICC_PRESALE_BUNDLE");
			}
			if (bundle.Items.Find((Network.BundleItem obj) => obj.ItemType == ProductType.PRODUCT_TYPE_BOOSTER && obj.ProductData == 30) != null)
			{
				return GameStrings.Get("GLUE_STORE_PRODUCT_NAME_LOOT_PRESALE_BUNDLE");
			}
			if (bundle.Items.Find((Network.BundleItem obj) => obj.ItemType == ProductType.PRODUCT_TYPE_BOOSTER && obj.ProductData == 31) != null)
			{
				return GameStrings.Get("GLUE_STORE_PRODUCT_NAME_GIL_PRESALE_BUNDLE");
			}
		}
		else if (includedProducts.Contains(ProductType.PRODUCT_TYPE_BOOSTER) && includedProducts.Contains(ProductType.PRODUCT_TYPE_CURRENCY))
		{
			Network.BundleItem boosterBundleItem = bundle.Items.Find((Network.BundleItem obj) => obj.ItemType == ProductType.PRODUCT_TYPE_BOOSTER);
			Network.BundleItem dustBundleItem = bundle.Items.Find((Network.BundleItem obj) => obj.ItemType == ProductType.PRODUCT_TYPE_CURRENCY && obj.ProductData == 2);
			if (boosterBundleItem != null && dustBundleItem != null)
			{
				string boosterName = GameDbf.Booster.GetRecord(boosterBundleItem.ProductData).Name;
				return GameStrings.Format("GLUE_STORE_PRODUCT_NAME_DUST", dustBundleItem.Quantity, boosterBundleItem.Quantity, boosterName);
			}
		}
		string itemsString = string.Empty;
		foreach (Network.BundleItem item in bundle.Items)
		{
			itemsString += $"[Product={item.ItemType},ProductData={item.ProductData},Quantity={item.Quantity}],";
		}
		Log.Store.PrintWarning("StoreManager.GetMultiItemProductName(): don't know how to format product name for items '" + itemsString + "'");
		return string.Empty;
	}

	private bool GetBoosterGoldCostNoGTAPP(int boosterID, out long cost)
	{
		cost = 0L;
		if (!m_goldCostBooster.ContainsKey(boosterID))
		{
			return false;
		}
		if (!CanBuyBoosterWithGold(boosterID))
		{
			return false;
		}
		Network.GoldCostBooster goldCostBooster = m_goldCostBooster[boosterID];
		if (!goldCostBooster.Cost.HasValue)
		{
			return false;
		}
		if (goldCostBooster.Cost.Value <= 0)
		{
			return false;
		}
		cost = goldCostBooster.Cost.Value;
		return true;
	}

	private bool GetArenaGoldCostNoGTAPP(out long cost)
	{
		cost = 0L;
		if (!m_goldCostArena.HasValue)
		{
			return false;
		}
		cost = m_goldCostArena.Value;
		return true;
	}

	private bool AutoCancelPurchaseIfNeeded(float now)
	{
		if (now - m_lastCancelRequestTime < m_secsBeforeAutoCancel)
		{
			return false;
		}
		return AutoCancelPurchaseIfPossible();
	}

	private bool AutoCancelPurchaseIfPossible()
	{
		MoneyOrGTAPPTransaction activeMoneyOrGTAPPTransaction = m_activeMoneyOrGTAPPTransaction;
		if (activeMoneyOrGTAPPTransaction == null || !activeMoneyOrGTAPPTransaction.Provider.HasValue)
		{
			return false;
		}
		if (BattlePayProvider.BP_PROVIDER_BLIZZARD == m_activeMoneyOrGTAPPTransaction.Provider.Value)
		{
			if (!IsSimpleCheckoutFeatureEnabled() || m_activeMoneyOrGTAPPTransaction.IsGTAPP)
			{
				TransactionStatus status = Status;
				if ((uint)(status - 1) <= 1u || (uint)(status - 6) <= 4u)
				{
					Log.Store.Print("StoreManager.AutoCancelPurchaseIfPossible() canceling Blizzard purchase, status={0}", Status);
					Status = TransactionStatus.AUTO_CANCELING;
					m_lastCancelRequestTime = Time.realtimeSinceStartup;
					Network.Get().CancelBlizzardPurchase(isAutoCanceled: true, null, null);
					return true;
				}
			}
			else if (Status != TransactionStatus.IN_PROGRESS_BLIZZARD_CHECKOUT)
			{
				if (ServiceManager.TryGet<HearthstoneCheckout>(out var commerce))
				{
					commerce.RequestClose();
				}
				Status = TransactionStatus.READY;
				m_lastCancelRequestTime = Time.realtimeSinceStartup;
				m_activeMoneyOrGTAPPTransaction = null;
				return true;
			}
		}
		return false;
	}

	private void CancelBlizzardPurchase(CancelPurchase.CancelReason? reason = null, string errorMessage = null)
	{
		Log.Store.Print("StoreManager.CancelBlizzardPurchase() reason=", reason.HasValue ? reason.Value.ToString() : "null");
		Status = TransactionStatus.USER_CANCELING;
		m_lastCancelRequestTime = Time.realtimeSinceStartup;
		Network.Get().CancelBlizzardPurchase(isAutoCanceled: false, reason, errorMessage);
	}

	public bool HaveProductsToSell()
	{
		if ((!ServiceManager.TryGet<IProductDataService>(out var dataService) || !dataService.HasProductsAvailable()) && (m_goldCostBooster == null || m_goldCostBooster.Count <= 0))
		{
			if (m_goldCostArena.HasValue)
			{
				return m_goldCostArena.HasValue;
			}
			return false;
		}
		return true;
	}

	private void OnStoreOpen()
	{
		if (BnetBar.Get() != null)
		{
			BnetBar.Get().RefreshCurrency();
		}
		this.OnStoreShown?.Invoke();
	}

	private void OnStoreExit(bool authorizationBackButtonPressed, object userData)
	{
		m_showStoreData.exitCallback?.Invoke(authorizationBackButtonPressed, userData);
		if (m_activeMoneyOrGTAPPTransaction != null)
		{
			m_activeMoneyOrGTAPPTransaction.ClosedStore = true;
		}
		if (m_view.ChallengePrompt.IsLoaded && !m_view.ChallengePrompt.Cancel(OnChallengeCancel))
		{
			AutoCancelPurchaseIfPossible();
		}
		UnblockStoreInterface();
		m_view.Hide();
		this.OnStoreHidden();
		if (BnetBar.Get() != null)
		{
			BnetBar.Get().RefreshCurrency();
		}
		HearthstonePerformance.Get()?.StopCurrentFlow();
	}

	private void OnStoreInfo(object userData)
	{
		ShowStoreInfo();
	}

	public void ShowStoreInfo()
	{
		BlockStoreInterface();
		m_view.SendToBam.Show(null, StoreSendToBAM.BAMReason.PAYMENT_INFO, "", fromPreviousPurchase: false);
	}

	public bool CanBuyBundle(ProductInfo bundleToBuy)
	{
		if (bundleToBuy == null)
		{
			Log.Store.PrintWarning("[StoreManager.CanBuyBundle] Null bundle passed to CanBuyBundle!");
			return false;
		}
		if (AchieveManager.Get() == null)
		{
			Log.Store.PrintError("[StoreManager.CanBuyBundle] Achieve Manager is null");
			return false;
		}
		if (!AchieveManager.Get().IsReady())
		{
			Log.Store.PrintError("[StoreManager.CanBuyBundle] Achieve Manager not ready");
			return false;
		}
		if (bundleToBuy.Items.Count < 1)
		{
			Log.Store.PrintWarning($"[StoreManager.CanBuyBundle]Attempting to buy bundle {bundleToBuy.Id}, which does not contain any items!");
			return false;
		}
		if (!bundleToBuy.IsBundleAvailableNow(this))
		{
			return false;
		}
		foreach (Network.BundleItem item in bundleToBuy.Items)
		{
			if (!CanBuyProductItem(item.ItemType, item.ProductData, GetProductItemPurchaseRule(item)))
			{
				return false;
			}
		}
		return true;
	}

	private bool CanBuyProductItem(ProductType product, int productData, ItemPurchaseRule purchaseRule)
	{
		if (AchieveManager.Get() == null)
		{
			Log.Store.PrintError("[StoreManager.CanBuyBundle] Achieve Manager is null");
			return false;
		}
		if (!AchieveManager.Get().IsReady())
		{
			Log.Store.PrintError("[StoreManager.CanBuyBundle] Achieve Manager not ready");
			return false;
		}
		switch (purchaseRule)
		{
		case ItemPurchaseRule.NO_LIMIT:
			return true;
		case ItemPurchaseRule.BLOCKING:
		{
			if (GetProductItemOwnershipStatus(product, productData, out var failReason) == ItemOwnershipStatus.UNOWNED)
			{
				return true;
			}
			Log.Store.Print(Blizzard.T5.Logging.LogLevel.Debug, verbose: true, $"[StoreManager.CanBuyProductItem] Product data {productData} already owned. Reason: {failReason}");
			return false;
		}
		default:
			Log.Store.PrintDebug($"[StoreManager.CanBuyProductItem] Product data {productData} undefined purchase rule.");
			return false;
		}
	}

	public void StartStoreBuy(BuyProductEventArgs args)
	{
		if (args == null)
		{
			Log.Store.PrintError("Cannot attempt purchase due to null BuyProductEventArgs");
			return;
		}
		BuyPmtProductEventArgs productArgs = args as BuyPmtProductEventArgs;
		m_pendingProductPurchaseArgs = args;
		if (!args.BeginPurchaseTelemetryFired)
		{
			SendShopPurchaseEventTelemetry(isComplete: false, m_pendingProductPurchaseArgs);
		}
		switch (args.PaymentCurrency)
		{
		case CurrencyType.GOLD:
			if (args is BuyNoGTAPPEventArgs noGtappArgs)
			{
				OnStoreBuyWithGoldNoGTAPP(noGtappArgs.transactionData);
			}
			else
			{
				OnStoreBuyWithGTAPP(productArgs);
			}
			return;
		case CurrencyType.REAL_MONEY:
			OnStoreBuyWithMoney(productArgs);
			return;
		}
		if (ShopUtils.IsCurrencyVirtual(args.PaymentCurrency))
		{
			OnStoreBuyWithCheckout(productArgs);
			return;
		}
		Log.Store.PrintError("Attempted purchase with invalid currency type {0}", args.PaymentCurrency);
	}

	private void OnStoreBuyWithMoney(BuyPmtProductEventArgs args)
	{
		if (TemporaryAccountManager.IsTemporaryAccount() && !IsSoftAccountPurchasingEnabled())
		{
			TemporaryAccountManager.Get().ShowHealUpDialog(GameStrings.Get("GLUE_TEMPORARY_ACCOUNT_DIALOG_HEADER_01"), GameStrings.Get("GLUE_TEMPORARY_ACCOUNT_DIALOG_BODY_02"), TemporaryAccountManager.HealUpReason.REAL_MONEY, userTriggered: true, null);
			return;
		}
		ProductInfo bundle = GetBundleFromPmtProductId(args.pmtProductId);
		if (bundle == null)
		{
			Log.Store.PrintError("OnStoreBuyWithMoney failed: bundle not found for pmtProductID = {0}.", args.pmtProductId);
		}
		else if (!CanBuyBundle(bundle))
		{
			Log.Store.PrintError("OnStoreBuyWithMoney failed: CanBuyBundle is false for pmtProductID = {0}.", args.pmtProductId);
		}
		else if (IsSimpleCheckoutFeatureEnabled())
		{
			OnStoreBuyWithCheckout(args);
		}
	}

	private void OnStoreBuyWithGTAPP(BuyPmtProductEventArgs args)
	{
		ProductInfo bundle = GetBundleFromPmtProductId(args.pmtProductId);
		if (!CanBuyBundle(bundle))
		{
			Log.Store.PrintError("Purchase with GTAPP failed (PMT product ID = {0}): CanBuyProductItem is false.", args.pmtProductId);
			return;
		}
		SetCanTapOutConfirmationUI(closeConfirmationUI: true);
		BlockStoreInterface();
		SetActiveMoneyOrGTAPPTransaction(UNKNOWN_TRANSACTION_ID, args.pmtProductId, BattlePayProvider.BP_PROVIDER_BLIZZARD, isGTAPP: true, tryToResolvePreviousTransactionNotices: false);
		Status = TransactionStatus.WAIT_METHOD_OF_PAYMENT;
		m_lastCancelRequestTime = Time.realtimeSinceStartup;
		m_view.PurchaseAuth.Show(m_activeMoneyOrGTAPPTransaction, isZeroCostLicense: false);
		Network.Get().GetPurchaseMethod(args.pmtProductId, args.quantity, Currency.GTAPP);
	}

	private void OnStoreBuyWithGoldNoGTAPP(NoGTAPPTransactionData noGTAPPtransactionData)
	{
		if (noGTAPPtransactionData == null)
		{
			Log.Store.PrintError("Purchase failed: null transaction data.");
			return;
		}
		if (!CanBuyProductItem(noGTAPPtransactionData.Product, noGTAPPtransactionData.ProductData, InferProductItemPurchaseRuleFromProductType(noGTAPPtransactionData.Product)))
		{
			Log.Store.PrintError("Purchase direct with gold (no GTAPP) failed: CanBuyProductItem is false.");
			return;
		}
		BlockStoreInterface();
		m_view.PurchaseAuth.Show(null, isZeroCostLicense: false, GetPurchaseAuthButtonStyle(m_currentShopType));
		Status = TransactionStatus.IN_PROGRESS_GOLD_NO_GTAPP;
		Network.Get().PurchaseViaGold(noGTAPPtransactionData.Quantity, noGTAPPtransactionData.Product, noGTAPPtransactionData.ProductData);
	}

	private void OnStoreBuyWithCheckout(BuyPmtProductEventArgs args)
	{
		ProductId productId = ProductId.CreateFrom(args.pmtProductId);
		HearthstoneCheckout commerce;
		if (GetBundleFromPmtProductId(productId) == null)
		{
			Log.Store.PrintError("Cannot buy product PMT ID = {0}. Bundle not found.", args.pmtProductId);
		}
		else if (!IsSimpleCheckoutFeatureEnabled())
		{
			Log.Store.PrintError("Purchase failed: Checkout feature is disabled.");
		}
		else if (!ServiceManager.TryGet<HearthstoneCheckout>(out commerce))
		{
			Log.Store.PrintError("Purchase failed: Commerce service is not available.");
		}
		else if (args.paymentCurrency == CurrencyType.REAL_MONEY)
		{
			if (TemporaryAccountManager.IsTemporaryAccount() && !IsSoftAccountPurchasingEnabled())
			{
				TemporaryAccountManager.Get().ShowHealUpDialog(GameStrings.Get("GLUE_TEMPORARY_ACCOUNT_DIALOG_HEADER_01"), GameStrings.Get("GLUE_TEMPORARY_ACCOUNT_DIALOG_BODY_02"), TemporaryAccountManager.HealUpReason.REAL_MONEY, userTriggered: true, null);
				return;
			}
			Status = TransactionStatus.WAIT_BLIZZARD_CHECKOUT;
			SetActiveMoneyOrGTAPPTransaction(UNKNOWN_TRANSACTION_ID, productId.Value, BattlePayProvider.BP_PROVIDER_BLIZZARD, isGTAPP: false, tryToResolvePreviousTransactionNotices: false);
			m_lastCancelRequestTime = Time.realtimeSinceStartup;
			SetCanTapOutConfirmationUI(closeConfirmationUI: true);
			BlockStoreInterface();
			commerce.ShowCheckout(productId, ShopUtils.GetCurrencyCode(args.paymentCurrency), (uint)args.quantity);
			if (HasExternalStore || (HearthstoneApplication.IsCNMobileBinary && Application.platform == RuntimePlatform.Android))
			{
				m_view.PurchaseAuth.Show(m_activeMoneyOrGTAPPTransaction, isZeroCostLicense: false, StorePurchaseAuth.ButtonStyle.Cancel);
			}
		}
		else if (ShopUtils.IsCurrencyVirtual(args.paymentCurrency))
		{
			Status = TransactionStatus.WAIT_BLIZZARD_CHECKOUT;
			SetActiveMoneyOrGTAPPTransaction(UNKNOWN_TRANSACTION_ID, args.pmtProductId, BattlePayProvider.BP_PROVIDER_BLIZZARD, isGTAPP: false, tryToResolvePreviousTransactionNotices: false);
			m_lastCancelRequestTime = Time.realtimeSinceStartup;
			SetCanTapOutConfirmationUI(closeConfirmationUI: true);
			BlockStoreInterface();
			if (m_view.PurchaseAuth.IsShown)
			{
				m_view.PurchaseAuth.StartNewTransaction(m_activeMoneyOrGTAPPTransaction, isZeroCostLicense: false);
			}
			else
			{
				m_view.PurchaseAuth.Show(m_activeMoneyOrGTAPPTransaction, isZeroCostLicense: false);
			}
			commerce.PurchaseWithVirtualCurrency(productId, ShopUtils.GetCurrencyCode(args.paymentCurrency), (uint)args.quantity);
		}
		else
		{
			Log.Store.PrintError("Buy with checkout failed: Invalid currency type {0}", args.paymentCurrency);
		}
	}

	private void OnSummaryConfirm(int quantity, object userData)
	{
		m_view.PurchaseAuth.Show(m_activeMoneyOrGTAPPTransaction, isZeroCostLicense: false, GetPurchaseAuthButtonStyle(m_currentShopType));
		if (m_challengePurchaseMethod != null)
		{
			m_view.ChallengePrompt.StartChallenge(m_challengePurchaseMethod.ChallengeURL);
		}
		else
		{
			ConfirmPurchase();
		}
	}

	private void ConfirmPurchase()
	{
		Status = ((!m_activeMoneyOrGTAPPTransaction.IsGTAPP) ? TransactionStatus.IN_PROGRESS_MONEY : TransactionStatus.IN_PROGRESS_GOLD_GTAPP);
		Network.Get().ConfirmPurchase();
	}

	private void OnSummaryCancel(object userData)
	{
		CancelBlizzardPurchase(null);
		UnblockStoreInterface();
	}

	private void OnSummaryInfo(object userData)
	{
		BlockStoreInterface();
		AutoCancelPurchaseIfPossible();
		m_view.SendToBam.Show(null, StoreSendToBAM.BAMReason.EULA_AND_TOS, string.Empty, fromPreviousPurchase: false);
	}

	private void OnSummaryPaymentAndTOS(object userData)
	{
		AutoCancelPurchaseIfPossible();
		m_view.LegalBam.Show();
	}

	private void OnChallengeComplete(string challengeID, bool isSuccess, CancelPurchase.CancelReason? reason, string internalErrorInfo)
	{
		if (!isSuccess)
		{
			OnChallengeCancel_Internal(challengeID, reason, internalErrorInfo);
			return;
		}
		m_view.PurchaseAuth.Show(m_activeMoneyOrGTAPPTransaction, isZeroCostLicense: false, GetPurchaseAuthButtonStyle(m_currentShopType));
		Status = TransactionStatus.CHALLENGE_SUBMITTED;
		ConfirmPurchase();
	}

	private void OnChallengeCancel(string challengeID)
	{
		OnChallengeCancel_Internal(challengeID, null, null);
	}

	private void OnChallengeCancel_Internal(string challengeID, CancelPurchase.CancelReason? reason, string errorMessage)
	{
		Debug.LogFormat("Canceling purchase from challengeId={0} reason={1} msg={2}", challengeID, reason.HasValue ? reason.Value.ToString() : "null", errorMessage);
		Status = TransactionStatus.CHALLENGE_CANCELED;
		CancelBlizzardPurchase(reason, errorMessage);
		UnblockStoreInterface();
		m_view.Hide();
	}

	private void OnSendToBAMOkay(MoneyOrGTAPPTransaction moneyOrGTAPPTransaction, StoreSendToBAM.BAMReason reason)
	{
		if (moneyOrGTAPPTransaction != null)
		{
			ConfirmActiveMoneyTransaction(moneyOrGTAPPTransaction.ID);
		}
		if (reason == StoreSendToBAM.BAMReason.PAYMENT_INFO)
		{
			UnblockStoreInterface();
		}
		else
		{
			m_view.DoneWithBam.Show();
		}
	}

	private void OnSendToBAMCancel(MoneyOrGTAPPTransaction moneyOrGTAPPTransaction)
	{
		if (moneyOrGTAPPTransaction != null)
		{
			ConfirmActiveMoneyTransaction(moneyOrGTAPPTransaction.ID);
		}
		UnblockStoreInterface();
	}

	private void OnSendToBAMLegal(StoreLegalBAMLinks.BAMReason reason)
	{
		UnblockStoreInterface();
	}

	private void OnAchievesUpdated(List<Achievement> updatedAchives, List<Achievement> completedAchives, object userData)
	{
		m_completedAchieves = AchieveManager.Get().GetNewCompletedAchievesToShow();
		ShowCompletedAchieve();
	}

	private void OnLicenseAddedAchievesUpdated(List<Achievement> activeLicenseAddedAchieves, object userData)
	{
		if (TransactionStatus.WAIT_ZERO_COST_LICENSE == Status && activeLicenseAddedAchieves.Count <= 0)
		{
			Log.Store.Print("StoreManager.OnLicenseAddedAchievesUpdated(): done waiting for licenses!");
			if (IsCurrentStoreLoaded())
			{
				RemovePurchaseAuthCancelButton();
				Processor.QueueJob("StoreManager.ShowCompletePurchaseSuccessWhenReady", Job_ShowCompletePurchaseSuccessWhenReady(null));
			}
			Status = TransactionStatus.READY;
		}
	}

	private void ShowCompletedAchieve()
	{
		if (m_completedAchieves.Count != 0)
		{
			Achievement completedAchieve = m_completedAchieves[0];
			m_completedAchieves.RemoveAt(0);
			QuestToast.ShowQuestToast(UserAttentionBlocker.NONE, delegate
			{
				ShowCompletedAchieve();
			}, updateCacheValues: true, completedAchieve, fullScreenEffects: false);
		}
	}

	private void OnPurchaseResultAcknowledged(bool success, MoneyOrGTAPPTransaction moneyOrGTAPPTransaction)
	{
		ProductInfo bundle = null;
		PaymentMethod paymentMethod;
		if (moneyOrGTAPPTransaction == null)
		{
			paymentMethod = PaymentMethod.GOLD_NO_GTAPP;
		}
		else
		{
			if (moneyOrGTAPPTransaction.ID > 0)
			{
				m_transactionIDsConclusivelyHandled.Add(moneyOrGTAPPTransaction.ID);
			}
			paymentMethod = (moneyOrGTAPPTransaction.IsGTAPP ? PaymentMethod.GOLD_GTAPP : PaymentMethod.MONEY);
			bundle = GetBundleFromPmtProductId(moneyOrGTAPPTransaction.PMTProductID.GetValueOrDefault());
		}
		if (PaymentMethod.GOLD_NO_GTAPP != paymentMethod)
		{
			ConfirmActiveMoneyTransaction(moneyOrGTAPPTransaction.ID);
		}
		if (success)
		{
			this.OnSuccessfulPurchaseAck(bundle, paymentMethod);
		}
		else
		{
			this.OnFailedPurchaseAck(bundle, paymentMethod);
		}
		SetCanTapOutConfirmationUI(closeConfirmationUI: true);
		UnblockStoreInterface();
		IStore currentStore = GetCurrentStore();
		if (m_currentShopType == ShopType.ADVENTURE_STORE || m_currentShopType == ShopType.ADVENTURE_STORE_WING_PURCHASE_WIDGET || m_currentShopType == ShopType.ADVENTURE_STORE_FULL_PURCHASE_WIDGET)
		{
			currentStore.Close();
		}
		if (!BattlePayAvailable && m_currentShopType == ShopType.GENERAL_STORE)
		{
			currentStore.Close();
		}
	}

	private void OnAuthExit()
	{
		this.OnAuthorizationExit();
	}

	private void OnPurchaseAuthCancelButtonPressed()
	{
		UnblockStoreInterface();
		if (ServiceManager.TryGet<HearthstoneCheckout>(out var commerce))
		{
			commerce.CancelCurrentTransaction();
		}
	}

	private void RemovePurchaseAuthCancelButton()
	{
		if (m_view.IsLoaded())
		{
			m_view.PurchaseAuth.HideCancelButton();
		}
	}

	private void HandlePurchaseSuccess(PurchaseErrorSource? source, MoneyOrGTAPPTransaction moneyOrGTAPPTransaction, string thirdPartyID, TransactionData checkoutTransactionData)
	{
		Status = TransactionStatus.READY;
		SendShopPurchaseEventTelemetry(isComplete: true, m_pendingProductPurchaseArgs);
		m_pendingProductPurchaseArgs = null;
		ProductInfo bundle = null;
		PaymentMethod paymentMethod;
		if (moneyOrGTAPPTransaction == null)
		{
			paymentMethod = PaymentMethod.GOLD_NO_GTAPP;
		}
		else
		{
			paymentMethod = ((checkoutTransactionData == null || !ShopUtils.IsCurrencyVirtual(ShopUtils.GetCurrencyTypeFromCode(checkoutTransactionData.CurrencyCode))) ? (moneyOrGTAPPTransaction.IsGTAPP ? PaymentMethod.GOLD_GTAPP : PaymentMethod.MONEY) : PaymentMethod.VIRTUAL_CURRENCY);
			ProductId productId = ProductId.CreateFrom(moneyOrGTAPPTransaction.PMTProductID.GetValueOrDefault());
			bundle = GetBundleFromPmtProductId(productId);
		}
		this.OnSuccessfulPurchase(bundle, paymentMethod);
		if (!IsCurrentStoreLoaded())
		{
			return;
		}
		if (source == PurchaseErrorSource.FROM_PREVIOUS_PURCHASE)
		{
			BlockStoreInterface();
			m_view.PurchaseAuth.ShowPreviousPurchaseSuccess(moneyOrGTAPPTransaction, GetPurchaseAuthButtonStyle(m_currentShopType));
			return;
		}
		if (ServiceManager.TryGet<CurrencyManager>(out var currencyManager))
		{
			switch (paymentMethod)
			{
			case PaymentMethod.GOLD_GTAPP:
			case PaymentMethod.GOLD_NO_GTAPP:
				currencyManager.MarkCurrencyDirty(CurrencyType.GOLD);
				break;
			case PaymentMethod.VIRTUAL_CURRENCY:
				if (bundle != null)
				{
					CurrencyType vcPriceType = bundle.GetFirstVirtualCurrencyPriceType();
					if (vcPriceType != 0)
					{
						currencyManager.MarkCurrencyDirty(vcPriceType);
					}
				}
				break;
			}
			if (bundle != null)
			{
				foreach (Network.BundleItem item in bundle.Items)
				{
					if (item.ItemType == ProductType.PRODUCT_TYPE_CURRENCY)
					{
						CurrencyType currencyGrantType = ShopUtils.GetCurrencyTypeFromProto((PegasusShared.CurrencyType)item.ProductData);
						if (currencyGrantType != 0)
						{
							currencyManager.MarkCurrencyDirty(currencyGrantType);
						}
					}
				}
			}
		}
		RemovePurchaseAuthCancelButton();
		Processor.QueueJob("StoreManager.ShowCompletePurchaseSuccessWhenReady", Job_ShowCompletePurchaseSuccessWhenReady(moneyOrGTAPPTransaction));
	}

	private IEnumerator<IAsyncJobResult> Job_ShowCompletePurchaseSuccessWhenReady(MoneyOrGTAPPTransaction moneyOrGTAPPTransaction)
	{
		DateTime startTime = DateTime.Now;
		double elapsedSeconds = 0.0;
		string currencyError = string.Empty;
		if (ServiceManager.TryGet<CurrencyManager>(out var currencyManager) && ServiceManager.TryGet<PurchaseManager>(out var purchaseManager))
		{
			bool willAutoPurchase = purchaseManager.HasAutoPurchase();
			while (Status != TransactionStatus.READY || willAutoPurchase || (string.IsNullOrEmpty(currencyError) && currencyManager.IsAnyCurrencyCacheRefreshing()))
			{
				elapsedSeconds = DateTime.Now.Subtract(startTime).TotalSeconds;
				if (string.IsNullOrEmpty(currencyError) && elapsedSeconds > CURRENCY_TRANSACTION_TIMEOUT_SECONDS)
				{
					currencyError = $"Gave up on waiting for currency balance after {elapsedSeconds} seconds";
				}
				yield return null;
			}
		}
		else
		{
			currencyError = "Could not get currency manager or purchase manager";
		}
		if (!string.IsNullOrEmpty(currencyError))
		{
			Log.Store.PrintError("[StoreManager.ShowCompletePurchaseSuccessWhenReady] gave up on waiting for currency balance after {0} seconds", elapsedSeconds);
			if (DialogManager.Get() != null)
			{
				AlertPopup.PopupInfo info = new AlertPopup.PopupInfo
				{
					m_text = GameStrings.Format("GLUE_STORE_FAIL_CURRENCY_BALANCE"),
					m_showAlertIcon = true,
					m_responseDisplay = AlertPopup.ResponseDisplay.OK
				};
				DialogManager.Get().ShowPopup(info);
			}
		}
		SetCanTapOutConfirmationUI(closeConfirmationUI: false);
		if (m_view.IsLoaded())
		{
			m_view.PurchaseAuth.CompletePurchaseSuccess(moneyOrGTAPPTransaction);
		}
	}

	private void HandleFailedRiskError(PurchaseErrorSource source)
	{
		bool num = TransactionStatus.CHALLENGE_CANCELED == Status;
		Status = TransactionStatus.READY;
		if (num)
		{
			Log.Store.Print("HandleFailedRiskError for canceled transaction");
			if (m_activeMoneyOrGTAPPTransaction != null)
			{
				ConfirmActiveMoneyTransaction(m_activeMoneyOrGTAPPTransaction.ID);
			}
			UnblockStoreInterface();
		}
		else if (IsCurrentStoreLoaded() && GetCurrentStore().IsOpen())
		{
			m_view.PurchaseAuth.Hide();
			m_view.Summary.Hide();
			BlockStoreInterface();
			m_view.SendToBam.Show(m_activeMoneyOrGTAPPTransaction, StoreSendToBAM.BAMReason.NEED_PASSWORD_RESET, string.Empty, source == PurchaseErrorSource.FROM_PREVIOUS_PURCHASE);
		}
	}

	private void HandleSendToBAMError(PurchaseErrorSource source, StoreSendToBAM.BAMReason reason, string errorCode)
	{
		Status = TransactionStatus.READY;
		if (IsCurrentStoreLoaded() && GetCurrentStore().IsOpen())
		{
			m_view.PurchaseAuth.Hide();
			m_view.Summary.Hide();
			BlockStoreInterface();
			m_view.SendToBam.Show(m_activeMoneyOrGTAPPTransaction, reason, errorCode, source == PurchaseErrorSource.FROM_PREVIOUS_PURCHASE);
		}
	}

	private void CompletePurchaseFailure(PurchaseErrorSource source, MoneyOrGTAPPTransaction moneyOrGTAPPTransaction, string failDetails, string thirdPartyID, Network.PurchaseErrorInfo.ErrorType error)
	{
		if (!IsCurrentStoreLoaded())
		{
			return;
		}
		switch (source)
		{
		case PurchaseErrorSource.FROM_PURCHASE_METHOD_RESPONSE:
			if (!m_view.SendToBam.IsShown)
			{
				BlockStoreInterface();
				m_view.PurchaseAuth.ShowPreviousPurchaseFailure(moneyOrGTAPPTransaction, failDetails, GetPurchaseAuthButtonStyle(m_currentShopType), error);
			}
			break;
		case PurchaseErrorSource.FROM_PREVIOUS_PURCHASE:
			BlockStoreInterface();
			m_view.PurchaseAuth.ShowPreviousPurchaseFailure(moneyOrGTAPPTransaction, failDetails, GetPurchaseAuthButtonStyle(m_currentShopType), error);
			break;
		default:
			if (!m_view.PurchaseAuth.CompletePurchaseFailure(moneyOrGTAPPTransaction, failDetails, error))
			{
				Log.Store.PrintWarning("StoreManager.CompletePurchaseFailure(): purchased failed (" + failDetails + ") but the store authorization window has been closed.");
				UnblockStoreInterface();
			}
			break;
		}
	}

	private void HandlePurchaseError(PurchaseErrorSource source, Network.PurchaseErrorInfo.ErrorType purchaseErrorType, string purchaseErrorCode, string thirdPartyID, bool isGTAPP)
	{
		if (IsConclusiveState(purchaseErrorType) && m_activeMoneyOrGTAPPTransaction != null && m_transactionIDsConclusivelyHandled.Contains(m_activeMoneyOrGTAPPTransaction.ID))
		{
			Log.Store.Print("HandlePurchaseError already handled purchase error for conclusive state on transaction (Transaction: {0}, current purchaseErrorType = {1})", m_activeMoneyOrGTAPPTransaction, purchaseErrorType);
			return;
		}
		Log.Store.Print($"HandlePurchaseError source={source} purchaseErrorType={purchaseErrorType} purchaseErrorCode={purchaseErrorCode} thirdPartyID={thirdPartyID}");
		string failDetails = "";
		switch (purchaseErrorType)
		{
		case Network.PurchaseErrorInfo.ErrorType.UNKNOWN:
			Log.Store.PrintWarning("StoreManager.HandlePurchaseError: purchase error is UNKNOWN, taking no action on this purchase");
			return;
		case Network.PurchaseErrorInfo.ErrorType.SUCCESS:
			if (source == PurchaseErrorSource.FROM_PURCHASE_METHOD_RESPONSE)
			{
				Log.Store.PrintWarning("StoreManager.HandlePurchaseError: received SUCCESS from payment method purchase error.");
			}
			else
			{
				HandlePurchaseSuccess(source, m_activeMoneyOrGTAPPTransaction, thirdPartyID, null);
			}
			return;
		case Network.PurchaseErrorInfo.ErrorType.STILL_IN_PROGRESS:
			switch (source)
			{
			case PurchaseErrorSource.FROM_PURCHASE_METHOD_RESPONSE:
				Log.Store.PrintWarning("StoreManager.HandlePurchaseError: received STILL_IN_PROGRESS from payment method purchase error.");
				break;
			default:
				Status = ((!isGTAPP) ? TransactionStatus.IN_PROGRESS_MONEY : TransactionStatus.IN_PROGRESS_GOLD_GTAPP);
				break;
			case PurchaseErrorSource.FROM_PREVIOUS_PURCHASE:
				break;
			}
			return;
		case Network.PurchaseErrorInfo.ErrorType.INVALID_BNET:
			failDetails = GameStrings.Get("GLUE_STORE_FAIL_BNET_ID");
			break;
		case Network.PurchaseErrorInfo.ErrorType.SERVICE_NA:
			if (source != PurchaseErrorSource.FROM_PREVIOUS_PURCHASE)
			{
				if (Status != 0)
				{
					BattlePayAvailable = false;
				}
				Status = TransactionStatus.UNKNOWN;
			}
			failDetails = GameStrings.Get("GLUE_STORE_FAIL_NO_BATTLEPAY");
			CompletePurchaseFailure(source, m_activeMoneyOrGTAPPTransaction, failDetails, thirdPartyID, purchaseErrorType);
			return;
		case Network.PurchaseErrorInfo.ErrorType.PURCHASE_IN_PROGRESS:
			if (source != PurchaseErrorSource.FROM_PREVIOUS_PURCHASE)
			{
				Status = ((!isGTAPP) ? TransactionStatus.IN_PROGRESS_MONEY : TransactionStatus.IN_PROGRESS_GOLD_GTAPP);
			}
			failDetails = GameStrings.Get("GLUE_STORE_FAIL_IN_PROGRESS");
			CompletePurchaseFailure(source, m_activeMoneyOrGTAPPTransaction, failDetails, thirdPartyID, purchaseErrorType);
			return;
		case Network.PurchaseErrorInfo.ErrorType.DATABASE:
			failDetails = GameStrings.Get("GLUE_STORE_FAIL_DATABASE");
			break;
		case Network.PurchaseErrorInfo.ErrorType.INVALID_QUANTITY:
			failDetails = GameStrings.Get("GLUE_STORE_FAIL_QUANTITY");
			break;
		case Network.PurchaseErrorInfo.ErrorType.DUPLICATE_LICENSE:
			failDetails = GameStrings.Get("GLUE_STORE_FAIL_LICENSE");
			break;
		case Network.PurchaseErrorInfo.ErrorType.REQUEST_NOT_SENT:
			if (source != PurchaseErrorSource.FROM_PREVIOUS_PURCHASE && Status != 0)
			{
				BattlePayAvailable = false;
			}
			failDetails = GameStrings.Get("GLUE_STORE_FAIL_NO_BATTLEPAY");
			break;
		case Network.PurchaseErrorInfo.ErrorType.NO_ACTIVE_BPAY:
			failDetails = GameStrings.Get("GLUE_STORE_FAIL_NO_ACTIVE_BPAY");
			break;
		case Network.PurchaseErrorInfo.ErrorType.FAILED_RISK:
			HandleFailedRiskError(source);
			return;
		case Network.PurchaseErrorInfo.ErrorType.CANCELED:
			if (source != PurchaseErrorSource.FROM_PREVIOUS_PURCHASE)
			{
				Status = TransactionStatus.READY;
			}
			return;
		case Network.PurchaseErrorInfo.ErrorType.WAIT_MOP:
			Log.Store.Print("StoreManager.HandlePurchaseError: Status is WAIT_MOP.. this probably shouldn't be happening.");
			if (source != PurchaseErrorSource.FROM_PREVIOUS_PURCHASE)
			{
				if (Status == TransactionStatus.UNKNOWN)
				{
					Log.Store.Print($"StoreManager.HandlePurchaseError: Status is WAIT_MOP, previous Status was UNKNOWN, source = {source}");
				}
				else
				{
					Status = TransactionStatus.WAIT_METHOD_OF_PAYMENT;
				}
			}
			return;
		case Network.PurchaseErrorInfo.ErrorType.WAIT_CONFIRM:
			if (source != PurchaseErrorSource.FROM_PREVIOUS_PURCHASE && Status == TransactionStatus.UNKNOWN)
			{
				Log.Store.Print($"StoreManager.HandlePurchaseError: Status is WAIT_CONFIRM, previous Status was UNKNOWN, source = {source}. Going to try to cancel the purchase.");
				CancelBlizzardPurchase(null);
			}
			return;
		case Network.PurchaseErrorInfo.ErrorType.WAIT_RISK:
			if (source != PurchaseErrorSource.FROM_PREVIOUS_PURCHASE)
			{
				Log.Store.Print("StoreManager.HandlePurchaseError: Waiting for client to respond to Risk challenge");
				if (Status == TransactionStatus.UNKNOWN)
				{
					Log.Store.Print($"StoreManager.HandlePurchaseError: Status is WAIT_RISK, previous Status was UNKNOWN, source = {source}");
				}
				else if (TransactionStatus.CHALLENGE_SUBMITTED == Status || TransactionStatus.CHALLENGE_CANCELED == Status)
				{
					Log.Store.Print($"StoreManager.HandlePurchaseError: Status = {Status}; ignoring WAIT_RISK purchase error info");
				}
				else
				{
					Status = TransactionStatus.WAIT_RISK;
				}
			}
			return;
		case Network.PurchaseErrorInfo.ErrorType.PRODUCT_NA:
			failDetails = GameStrings.Get("GLUE_STORE_FAIL_PRODUCT_NA");
			break;
		case Network.PurchaseErrorInfo.ErrorType.PRODUCT_EVENT_HAS_ENDED:
		{
			ProductId productId = ProductId.CreateFrom(m_activeMoneyOrGTAPPTransaction.PMTProductID.GetValueOrDefault());
			failDetails = ((m_activeMoneyOrGTAPPTransaction == null || !GetBundleFromPmtProductId(productId).IsPrePurchase) ? GameStrings.Get("GLUE_STORE_PRODUCT_EVENT_HAS_ENDED") : GameStrings.Get("GLUE_STORE_PRE_PURCHASE_HAS_ENDED"));
			break;
		}
		case Network.PurchaseErrorInfo.ErrorType.RISK_TIMEOUT:
			failDetails = GameStrings.Get("GLUE_STORE_FAIL_CHALLENGE_TIMEOUT");
			break;
		case Network.PurchaseErrorInfo.ErrorType.PRODUCT_ALREADY_OWNED:
			failDetails = GameStrings.Get("GLUE_STORE_FAIL_PRODUCT_ALREADY_OWNED");
			break;
		case Network.PurchaseErrorInfo.ErrorType.WAIT_THIRD_PARTY_RECEIPT:
			Log.Store.PrintWarning("StoreManager.HandlePurchaseError: Received WAIT_THIRD_PARTY_RECEIPT response, even though legacy third party purchasing is removed.");
			return;
		case Network.PurchaseErrorInfo.ErrorType.BP_GENERIC_FAIL:
		case Network.PurchaseErrorInfo.ErrorType.BP_RISK_ERROR:
		case Network.PurchaseErrorInfo.ErrorType.BP_PAYMENT_AUTH:
		case Network.PurchaseErrorInfo.ErrorType.BP_PROVIDER_DENIED:
		case Network.PurchaseErrorInfo.ErrorType.E_BP_GENERIC_FAIL_RETRY_CONTACT_CS_IF_PERSISTS:
			if (!isGTAPP)
			{
				StoreSendToBAM.BAMReason reason = StoreSendToBAM.BAMReason.GENERIC_PAYMENT_FAIL;
				if (purchaseErrorType == Network.PurchaseErrorInfo.ErrorType.E_BP_GENERIC_FAIL_RETRY_CONTACT_CS_IF_PERSISTS)
				{
					reason = StoreSendToBAM.BAMReason.GENERIC_PURCHASE_FAIL_RETRY_CONTACT_CS_IF_PERSISTS;
				}
				HandleSendToBAMError(source, reason, purchaseErrorCode);
				if (HasExternalStore)
				{
					CompletePurchaseFailure(source, m_activeMoneyOrGTAPPTransaction, failDetails, thirdPartyID, purchaseErrorType);
				}
				return;
			}
			failDetails = GameStrings.Get("GLUE_STORE_FAIL_GOLD_GENERIC");
			break;
		case Network.PurchaseErrorInfo.ErrorType.BP_INVALID_CC_EXPIRY:
			if (!isGTAPP)
			{
				HandleSendToBAMError(source, StoreSendToBAM.BAMReason.CREDIT_CARD_EXPIRED, string.Empty);
				return;
			}
			failDetails = GameStrings.Get("GLUE_STORE_FAIL_GOLD_GENERIC");
			break;
		case Network.PurchaseErrorInfo.ErrorType.BP_NO_VALID_PAYMENT:
			if (source == PurchaseErrorSource.FROM_PURCHASE_METHOD_RESPONSE)
			{
				Log.Store.PrintWarning("StoreManager.HandlePurchaseError: received BP_NO_VALID_PAYMENT from payment method purchase error.");
				break;
			}
			if (!isGTAPP)
			{
				HandleSendToBAMError(source, StoreSendToBAM.BAMReason.NO_VALID_PAYMENT_METHOD, string.Empty);
				return;
			}
			failDetails = GameStrings.Get("GLUE_STORE_FAIL_GOLD_GENERIC");
			break;
		case Network.PurchaseErrorInfo.ErrorType.BP_PURCHASE_BAN:
			failDetails = GameStrings.Get("GLUE_STORE_FAIL_PURCHASE_BAN");
			break;
		case Network.PurchaseErrorInfo.ErrorType.BP_SPENDING_LIMIT:
			failDetails = (isGTAPP ? GameStrings.Get("GLUE_STORE_FAIL_GOLD_GENERIC") : GameStrings.Get("GLUE_STORE_FAIL_SPENDING_LIMIT"));
			break;
		case Network.PurchaseErrorInfo.ErrorType.BP_PARENTAL_CONTROL:
			failDetails = GameStrings.Get("GLUE_STORE_FAIL_PARENTAL_CONTROL");
			break;
		case Network.PurchaseErrorInfo.ErrorType.BP_THROTTLED:
			failDetails = GameStrings.Get("GLUE_STORE_FAIL_THROTTLED");
			break;
		case Network.PurchaseErrorInfo.ErrorType.BP_THIRD_PARTY_BAD_RECEIPT:
		case Network.PurchaseErrorInfo.ErrorType.BP_THIRD_PARTY_RECEIPT_USED:
			failDetails = GameStrings.Get("GLUE_STORE_FAIL_THIRD_PARTY_BAD_RECEIPT");
			break;
		case Network.PurchaseErrorInfo.ErrorType.BP_PRODUCT_UNIQUENESS_VIOLATED:
			HandleSendToBAMError(source, StoreSendToBAM.BAMReason.PRODUCT_UNIQUENESS_VIOLATED, string.Empty);
			return;
		case Network.PurchaseErrorInfo.ErrorType.BP_REGION_IS_DOWN:
			failDetails = GameStrings.Get("GLUE_STORE_FAIL_REGION_IS_DOWN");
			break;
		case Network.PurchaseErrorInfo.ErrorType.E_BP_CHALLENGE_ID_FAILED_VERIFICATION:
			failDetails = GameStrings.Get("GLUE_STORE_FAIL_CHALLENGE_ID_FAILED_VERIFICATION");
			break;
		default:
			failDetails = GameStrings.Get("GLUE_STORE_FAIL_GENERAL");
			break;
		}
		if (source != PurchaseErrorSource.FROM_PREVIOUS_PURCHASE)
		{
			Status = TransactionStatus.READY;
		}
		CompletePurchaseFailure(source, m_activeMoneyOrGTAPPTransaction, failDetails, thirdPartyID, purchaseErrorType);
	}

	private void SetActiveMoneyOrGTAPPTransaction(long id, long? pmtProductID, BattlePayProvider? provider, bool isGTAPP, bool tryToResolvePreviousTransactionNotices)
	{
		MoneyOrGTAPPTransaction moneyOrGTAPPTransaction = new MoneyOrGTAPPTransaction(id, pmtProductID, provider, isGTAPP);
		bool overrideOldActiveTransaction = true;
		if (m_activeMoneyOrGTAPPTransaction != null)
		{
			if (moneyOrGTAPPTransaction.Equals(m_activeMoneyOrGTAPPTransaction))
			{
				overrideOldActiveTransaction = !m_activeMoneyOrGTAPPTransaction.Provider.HasValue && provider.HasValue;
			}
			else if (UNKNOWN_TRANSACTION_ID != m_activeMoneyOrGTAPPTransaction.ID)
			{
				Log.Store.PrintWarning(string.Format("StoreManager.SetActiveMoneyOrGTAPPTransaction(id={0}, pmtProductId={1}, isGTAPP={2}, provider={3}) does not match active money or GTAPP transaction '{4}'", id, pmtProductID, isGTAPP, provider.HasValue ? provider.Value.ToString() : "UNKNOWN", m_activeMoneyOrGTAPPTransaction));
			}
		}
		if (overrideOldActiveTransaction)
		{
			Log.Store.Print($"SetActiveMoneyOrGTAPPTransaction() {moneyOrGTAPPTransaction}");
			m_activeMoneyOrGTAPPTransaction = moneyOrGTAPPTransaction;
		}
		if (!m_firstMoneyOrGTAPPTransactionSet)
		{
			m_firstMoneyOrGTAPPTransactionSet = true;
			if (tryToResolvePreviousTransactionNotices)
			{
				ResolveFirstMoneyOrGTAPPTransactionIfPossible();
			}
		}
	}

	private void ResolveFirstMoneyOrGTAPPTransactionIfPossible()
	{
		if (m_firstMoneyOrGTAPPTransactionSet && FirstNoticesProcessed && m_activeMoneyOrGTAPPTransaction != null && m_outstandingPurchaseNotices.Find((NetCache.ProfileNoticePurchase obj) => obj.OriginData == m_activeMoneyOrGTAPPTransaction.ID) == null)
		{
			Log.Store.Print($"StoreManager.ResolveFirstMoneyTransactionIfPossible(): no outstanding notices for transaction {m_activeMoneyOrGTAPPTransaction}; setting m_activeMoneyOrGTAPPTransaction = null");
			m_activeMoneyOrGTAPPTransaction = null;
		}
	}

	private void ConfirmActiveMoneyTransaction(long id)
	{
		if (m_activeMoneyOrGTAPPTransaction == null || (m_activeMoneyOrGTAPPTransaction.ID != UNKNOWN_TRANSACTION_ID && m_activeMoneyOrGTAPPTransaction.ID != id))
		{
			Log.Store.PrintWarning($"StoreManager.ConfirmActiveMoneyTransaction(id={id}) does not match active money transaction '{m_activeMoneyOrGTAPPTransaction}'");
		}
		Log.Store.Print($"ConfirmActiveMoneyTransaction() {id}");
		List<NetCache.ProfileNoticePurchase> list = m_outstandingPurchaseNotices.FindAll(Predicate);
		m_outstandingPurchaseNotices.RemoveAll(Predicate);
		foreach (NetCache.ProfileNoticePurchase notice in list)
		{
			Network.Get().AckNotice(notice.NoticeID);
		}
		m_confirmedTransactionIDs.Add(id);
		m_activeMoneyOrGTAPPTransaction = null;
		bool Predicate(NetCache.ProfileNoticePurchase obj)
		{
			return obj.OriginData == id;
		}
	}

	private void OnNewNotices(List<NetCache.ProfileNotice> newNotices, bool isInitialNoticeList)
	{
		Log.Store.Print("StoreManager.OnNewNotices() New Notice");
		List<long> noticeIDsToAck = new List<long>();
		foreach (NetCache.ProfileNotice notice in newNotices)
		{
			if (notice.Type == NetCache.ProfileNotice.NoticeType.PURCHASE)
			{
				if (notice.Origin == NetCache.ProfileNotice.NoticeOrigin.PURCHASE_CANCELED)
				{
					Log.Store.Print($"StoreManager.OnNewNotices() ack'ing purchase canceled notice for bpay ID {notice.OriginData}");
					noticeIDsToAck.Add(notice.NoticeID);
				}
				else if (m_confirmedTransactionIDs.Contains(notice.OriginData))
				{
					Log.Store.Print($"StoreManager.OnNewNotices() ack'ing purchase notice for already confirmed bpay ID {notice.OriginData}");
					noticeIDsToAck.Add(notice.NoticeID);
				}
				else
				{
					NetCache.ProfileNoticePurchase purchaseNotice = notice as NetCache.ProfileNoticePurchase;
					Log.Store.Print($"StoreManager.OnNewNotices() adding outstanding purchase notice for bpay ID {notice.OriginData}");
					m_outstandingPurchaseNotices.Add(purchaseNotice);
				}
			}
		}
		Network network = Network.Get();
		foreach (long noticeID in noticeIDsToAck)
		{
			network.AckNotice(noticeID);
		}
		if (!FirstNoticesProcessed)
		{
			FirstNoticesProcessed = true;
			if (Status == TransactionStatus.READY)
			{
				ResolveFirstMoneyOrGTAPPTransactionIfPossible();
			}
		}
	}

	private void OnNetCacheFeaturesReady()
	{
		NetCache.NetCacheFeatures guardianVars = NetCache.Get().GetNetObject<NetCache.NetCacheFeatures>();
		FeaturesReady = guardianVars != null;
	}

	private void OnPurchaseCanceledResponse()
	{
		Network.PurchaseCanceledResponse response = Network.Get().GetPurchaseCanceledResponse();
		switch (response.Result)
		{
		case Network.PurchaseCanceledResponse.CancelResult.NOT_ALLOWED:
		{
			Log.Store.PrintWarning("StoreManager.OnPurchaseCanceledResponse(): cancel purchase is not allowed right now.");
			bool isGTAPP = Currency.IsGTAPP(response.CurrencyCode);
			SetActiveMoneyOrGTAPPTransaction(response.TransactionID, response.PMTProductID, MoneyOrGTAPPTransaction.UNKNOWN_PROVIDER, isGTAPP, tryToResolvePreviousTransactionNotices: true);
			Status = ((!isGTAPP) ? TransactionStatus.IN_PROGRESS_MONEY : TransactionStatus.IN_PROGRESS_GOLD_GTAPP);
			if (m_previousStatusBeforeAutoCancel != 0)
			{
				Status = m_previousStatusBeforeAutoCancel;
				m_previousStatusBeforeAutoCancel = TransactionStatus.UNKNOWN;
			}
			break;
		}
		case Network.PurchaseCanceledResponse.CancelResult.NOTHING_TO_CANCEL:
			m_previousStatusBeforeAutoCancel = TransactionStatus.UNKNOWN;
			if (m_activeMoneyOrGTAPPTransaction != null && UNKNOWN_TRANSACTION_ID != m_activeMoneyOrGTAPPTransaction.ID)
			{
				ConfirmActiveMoneyTransaction(m_activeMoneyOrGTAPPTransaction.ID);
			}
			Status = TransactionStatus.READY;
			break;
		case Network.PurchaseCanceledResponse.CancelResult.SUCCESS:
			Log.Store.Print("StoreManager.OnPurchaseCanceledResponse(): purchase successfully canceled.");
			ConfirmActiveMoneyTransaction(response.TransactionID);
			Status = TransactionStatus.READY;
			m_previousStatusBeforeAutoCancel = TransactionStatus.UNKNOWN;
			break;
		}
	}

	private bool IsConclusiveState(Network.PurchaseErrorInfo.ErrorType errorType)
	{
		switch (errorType)
		{
		case Network.PurchaseErrorInfo.ErrorType.UNKNOWN:
		case Network.PurchaseErrorInfo.ErrorType.STILL_IN_PROGRESS:
		case Network.PurchaseErrorInfo.ErrorType.WAIT_MOP:
		case Network.PurchaseErrorInfo.ErrorType.WAIT_CONFIRM:
		case Network.PurchaseErrorInfo.ErrorType.WAIT_RISK:
		case Network.PurchaseErrorInfo.ErrorType.WAIT_THIRD_PARTY_RECEIPT:
			return false;
		default:
			return true;
		}
	}

	private void OnBattlePayStatusResponse()
	{
		Network.BattlePayStatus response = Network.Get().GetBattlePayStatusResponse();
		if (response.BattlePayAvailable != BattlePayAvailable)
		{
			BattlePayAvailable = response.BattlePayAvailable;
			Log.Store.Print("Store server status is now {0}", BattlePayAvailable ? "available" : "unavailable");
		}
		switch (response.State)
		{
		case Network.BattlePayStatus.PurchaseState.READY:
			Status = TransactionStatus.READY;
			Log.Store.Print("Store PurchaseState is READY.");
			break;
		case Network.BattlePayStatus.PurchaseState.CHECK_RESULTS:
		{
			Log.Store.Print("Store PurchaseState is CHECK_RESULTS.");
			bool isGTAPP = Currency.IsGTAPP(response.CurrencyCode);
			bool tryToResolveNotices = IsConclusiveState(response.PurchaseError.Error);
			SetActiveMoneyOrGTAPPTransaction(response.TransactionID, response.PMTProductID, response.Provider, isGTAPP, tryToResolveNotices);
			HandlePurchaseError(PurchaseErrorSource.FROM_STATUS_OR_PURCHASE_RESPONSE, response.PurchaseError.Error, response.PurchaseError.ErrorCode, response.ThirdPartyID, isGTAPP);
			break;
		}
		case Network.BattlePayStatus.PurchaseState.ERROR:
			Log.Store.PrintError("Store PurchaseState is ERROR.");
			break;
		default:
			Log.Store.PrintError("Store PurchaseState is unknown value {0}.", response.State);
			break;
		}
	}

	private static string GetExternalStoreProductId(ProductInfo bundle)
	{
		if (bundle == null)
		{
			Log.Store.PrintError("[GetExternalStoreProductId] There was no bundle object properly sent.");
			return null;
		}
		if (!bundle.Id.IsValid())
		{
			return null;
		}
		if (!ServiceManager.TryGet<IProductDataService>(out var _))
		{
			return null;
		}
		_ = bundle.Id;
		return null;
	}

	private void OnBattlePayConfigResponse()
	{
		Network.BattlePayConfig response = Network.Get().GetBattlePayConfigResponse();
		if (response.Available)
		{
			m_secsBeforeAutoCancel = response.SecondsBeforeAutoCancel;
			m_ignoreProductTiming = response.IgnoreProductTiming;
			m_currency = response.Currency;
			m_goldCostBooster.Clear();
			foreach (Network.GoldCostBooster goldCostBooster in response.GoldCostBoosters)
			{
				m_goldCostBooster.Add(goldCostBooster.ID, goldCostBooster);
			}
			m_goldCostArena = response.GoldCostArena;
			m_sales.Clear();
			foreach (Network.ShopSale sale in response.SaleList)
			{
				m_sales[sale.SaleId] = sale;
			}
			Log.Store.Print("Server responds that store is available.");
			BattlePayAvailable = true;
		}
		else
		{
			Log.Store.PrintWarning("Server responds that store is unavailable.");
			BattlePayAvailable = false;
		}
		ShopInitialization.StartInitializing(response);
	}

	private void HandleZeroCostLicensePurchaseMethod(Network.PurchaseMethod method)
	{
		if (Network.PurchaseErrorInfo.ErrorType.STILL_IN_PROGRESS != method.PurchaseError.Error)
		{
			Log.Store.PrintWarning($"StoreManager.HandleZeroCostLicensePurchaseMethod() FAILED error={method.PurchaseError.Error}");
			Status = TransactionStatus.READY;
		}
		else
		{
			Log.Store.Print("StoreManager.HandleZeroCostLicensePurchaseMethod succeeded, refreshing achieves");
		}
	}

	private void OnPurchaseMethod()
	{
		Network.PurchaseMethod method = Network.Get().GetPurchaseMethodResponse();
		if (method.IsZeroCostLicense)
		{
			HandleZeroCostLicensePurchaseMethod(method);
			return;
		}
		if (!string.IsNullOrEmpty(method.ChallengeID) && !string.IsNullOrEmpty(method.ChallengeURL))
		{
			m_challengePurchaseMethod = method;
		}
		else
		{
			m_challengePurchaseMethod = null;
		}
		bool isGTAPP = Currency.IsGTAPP(method.CurrencyCode);
		SetActiveMoneyOrGTAPPTransaction(method.TransactionID, method.PMTProductID, BattlePayProvider.BP_PROVIDER_BLIZZARD, isGTAPP, tryToResolvePreviousTransactionNotices: false);
		if (method.PurchaseError != null)
		{
			HandlePurchaseError(PurchaseErrorSource.FROM_PURCHASE_METHOD_RESPONSE, method.PurchaseError.Error, method.PurchaseError.ErrorCode, string.Empty, isGTAPP);
			return;
		}
		BlockStoreInterface();
		if (isGTAPP)
		{
			OnSummaryConfirm(method.Quantity, null);
			return;
		}
		string paymentMethodName = ((!method.UseEBalance) ? method.WalletName : GameStrings.Get("GLUE_STORE_BNET_BALANCE"));
		IStore currentStore = GetCurrentStore();
		if (currentStore == null || !currentStore.IsOpen())
		{
			AutoCancelPurchaseIfPossible();
			return;
		}
		m_view.PurchaseAuth.Hide();
		Status = TransactionStatus.WAIT_CONFIRM;
		ProductId prodId = ProductId.CreateFrom(method.PMTProductID ?? (-1));
		m_view.Summary.Show(prodId, method.Quantity, paymentMethodName);
	}

	private void OnPurchaseResponse()
	{
		Network.PurchaseResponse response = Network.Get().GetPurchaseResponse();
		bool isGTAPP = Currency.IsGTAPP(response.CurrencyCode);
		SetActiveMoneyOrGTAPPTransaction(response.TransactionID, response.PMTProductID, MoneyOrGTAPPTransaction.UNKNOWN_PROVIDER, isGTAPP, tryToResolvePreviousTransactionNotices: false);
		HandlePurchaseError(PurchaseErrorSource.FROM_STATUS_OR_PURCHASE_RESPONSE, response.PurchaseError.Error, response.PurchaseError.ErrorCode, response.ThirdPartyID, isGTAPP);
	}

	private void OnPurchaseViaGoldResponse()
	{
		Network.PurchaseViaGoldResponse purchaseWithGoldResponse = Network.Get().GetPurchaseWithGoldResponse();
		string failDetails = "";
		switch (purchaseWithGoldResponse.Error)
		{
		case Network.PurchaseViaGoldResponse.ErrorType.SUCCESS:
			HandlePurchaseSuccess(null, null, string.Empty, null);
			return;
		case Network.PurchaseViaGoldResponse.ErrorType.INSUFFICIENT_GOLD:
			failDetails = GameStrings.Get("GLUE_STORE_FAIL_NOT_ENOUGH_GOLD");
			break;
		case Network.PurchaseViaGoldResponse.ErrorType.PRODUCT_NA:
			failDetails = GameStrings.Get("GLUE_STORE_FAIL_PRODUCT_NA");
			break;
		case Network.PurchaseViaGoldResponse.ErrorType.FEATURE_NA:
			failDetails = GameStrings.Get("GLUE_TOOLTIP_BUTTON_DISABLED_DESC");
			break;
		case Network.PurchaseViaGoldResponse.ErrorType.INVALID_QUANTITY:
			failDetails = GameStrings.Get("GLUE_STORE_FAIL_QUANTITY");
			break;
		default:
			failDetails = GameStrings.Get("GLUE_STORE_FAIL_GENERAL");
			break;
		}
		Status = TransactionStatus.READY;
		m_view.PurchaseAuth.CompletePurchaseFailure(null, failDetails, Network.PurchaseErrorInfo.ErrorType.BP_GENERIC_FAIL);
	}

	private void OnThirdPartyPurchaseStatusResponse()
	{
		Log.Store.PrintWarning("[StoreManager.OnThirdPartyPurchaseStatusResponse] Received OnThirdPartyPurchaseStatusResponse packet.  Legacy third party purchasing has been removed.");
	}

	private void StoreViewReady()
	{
		if (m_waitingToShowStore && IsCurrentStoreLoaded())
		{
			ShowStore();
		}
	}

	private void OnLettuceCollectionLoaded()
	{
		LoadGeneralStore();
		m_view.LoadAssets();
	}

	private void OnArenaStoreLoaded(AssetReference assetRef, GameObject go, object callbackData)
	{
		ArenaStore store = OnStoreLoaded<ArenaStore>(go, ShopType.ARENA_STORE);
		if (store != null)
		{
			SetupLoadedStore(store);
		}
	}

	private void OnBrawlStoreLoaded(AssetReference assetRef, GameObject go, object callbackData)
	{
		TavernBrawlStore store = OnStoreLoaded<TavernBrawlStore>(go, ShopType.TAVERN_BRAWL_STORE);
		if (store != null)
		{
			SetupLoadedStore(store);
		}
	}

	private void OnAdventureStoreLoaded(AssetReference assetRef, GameObject go, object callbackData)
	{
		AdventureStore store = OnStoreLoaded<AdventureStore>(go, ShopType.ADVENTURE_STORE);
		if (store != null)
		{
			SetupLoadedStore(store);
		}
	}

	private void OnAdventureWingStoreLoaded(AssetReference assetRef, GameObject go, object callbackData)
	{
		AdventureStore store = OnStoreLoaded<AdventureStore>(go, ShopType.ADVENTURE_STORE_WING_PURCHASE_WIDGET);
		if (store != null)
		{
			SetupLoadedStore(store);
		}
	}

	private void OnAdventureFullStoreLoaded(AssetReference assetRef, GameObject go, object callbackData)
	{
		AdventureStore store = OnStoreLoaded<AdventureStore>(go, ShopType.ADVENTURE_STORE_FULL_PURCHASE_WIDGET);
		if (store != null)
		{
			SetupLoadedStore(store);
		}
	}

	private T OnStoreLoaded<T>(GameObject go, ShopType shopType) where T : Store
	{
		if (go == null)
		{
			Debug.LogError($"StoreManager.OnStoreLoaded<{typeof(T)}>(): go is null!");
			return null;
		}
		T store = go.GetComponent<T>();
		if (store == null)
		{
			store = go.GetComponentInChildren<T>();
		}
		if (store == null)
		{
			Debug.LogError($"StoreManager.OnStoreLoaded<{typeof(T)}>(): go has no {typeof(T)} component!");
			return null;
		}
		m_stores[shopType] = store;
		return store;
	}

	public void SendShopPurchaseEventTelemetry(bool isComplete, BuyProductEventArgs productArgs)
	{
		if (productArgs == null)
		{
			Log.Store.PrintWarning("No active transaction in progress");
			return;
		}
		Product product = new Product();
		if (!ShopUtils.TryDecomposeBuyProductEventArgs(productArgs, out var productId, out var currencyCode, out var totalPrice, out var quantity, out var productItemType, out var productItemId))
		{
			Log.Store.PrintError("Failed to decompose pending product purchase args for telemetry.");
			return;
		}
		BattlePayProvider? provider = ActiveTransactionProvider();
		string storefront = (provider.HasValue ? provider.Value.ToString().ToLowerInvariant() : "");
		product.ProductId = (productId.IsValid() ? productId.Value : (-1));
		product.HsProductType = productItemType;
		product.HsProductId = productItemId;
		string currentTabId = string.Empty;
		string currentSubTabId = string.Empty;
		if (m_currentShopType == ShopType.GENERAL_STORE)
		{
			Shop.Get().TryGetCurrentTabIds(out currentTabId, out currentSubTabId);
		}
		Shop shop = Shop.Get();
		TelemetryManager.Client().SendShopPurchaseEvent(product, quantity, currencyCode, totalPrice, isGift: false, storefront, isComplete, m_currentShopType.ToString(), (shop != null) ? shop.ProductPageController.RedirectedProductId : string.Empty, currentTabId, currentSubTabId);
	}

	public void RegisterAmazingNewShop(Shop amazingNewShop)
	{
		SetupLoadedStore(amazingNewShop);
	}

	private void SetupLoadedStore(IStore store)
	{
		if (store != null)
		{
			store.OnProductPurchaseAttempt += StartStoreBuy;
			store.OnOpened += OnStoreOpen;
			store.OnClosed += delegate(StoreClosedArgs e)
			{
				OnStoreExit(e.authorizationBackButtonPressed ?? false, null);
			};
			store.OnReady += StoreViewReady;
			(store as Store)?.RegisterInfoListener(OnStoreInfo);
			StoreViewReady();
		}
	}

	private void OnSceneUnloaded(SceneMgr.Mode prevMode, PegasusScene prevScene, object userData)
	{
		UnloadAndFreeMemory();
	}

	public void SetPersonalizedShopPageAndRefreshCatalog(List<string> pageIds)
	{
		if (pageIds.Count == 0)
		{
			Log.Store.PrintError("No page id data was found.");
			return;
		}
		if (!ServiceManager.TryGet<IProductDataService>(out var dataService))
		{
			Log.Store.PrintError("Data storage not available.");
			return;
		}
		dataService.ShopPlacementIds.Clear();
		foreach (string pageId in pageIds)
		{
			if (!string.IsNullOrEmpty(pageId))
			{
				dataService.ShopPlacementIds.Add(pageId);
			}
		}
		dataService.RefreshShopPageRequests();
	}

	public void HandleCommerceCancelEvent()
	{
		Status = TransactionStatus.READY;
		if (m_activeMoneyOrGTAPPTransaction != null)
		{
			ConfirmActiveMoneyTransaction(m_activeMoneyOrGTAPPTransaction.ID);
		}
		m_view.PurchaseAuth.Hide();
		Network.Get().ReportBlizzardCheckoutStatus(BlizzardCheckoutStatus.BLIZZARD_CHECKOUT_STATUS_CANCELED);
		TelemetryManager.Client().SendBlizzardCheckoutPurchaseCancel();
	}

	public void HandleCommerceCloseEvent()
	{
		if (!m_view.PurchaseAuth.IsShown)
		{
			SetCanTapOutConfirmationUI(closeConfirmationUI: true);
			UnblockStoreInterface();
			if (Status == TransactionStatus.IN_PROGRESS_BLIZZARD_CHECKOUT || m_showStoreData.closeOnTransactionComplete)
			{
				GetCurrentStore()?.Close();
			}
		}
		else if (Status != TransactionStatus.READY)
		{
			m_view.PurchaseAuth.Hide();
			UnblockStoreInterface();
		}
	}

	public void HandleCommerceOrderPending(TransactionData data)
	{
		if (IsInTransaction() || m_view.PurchaseAuth.IsShown)
		{
			m_view.PurchaseAuth.Show(m_activeMoneyOrGTAPPTransaction, isZeroCostLicense: false);
		}
		Status = TransactionStatus.IN_PROGRESS_BLIZZARD_CHECKOUT;
		if (data != null)
		{
			Network.Get().ReportBlizzardCheckoutStatus(BlizzardCheckoutStatus.BLIZZARD_CHECKOUT_STATUS_START, data);
			TelemetryManager.Client().SendBlizzardCheckoutPurchaseStart(data.TransactionID, data.ProductID.ToString(), data.CurrencyCode);
		}
	}

	public void HandleCommerceOrderFailure(TransactionData data)
	{
		if (!m_view.PurchaseAuth.IsShown)
		{
			m_view.PurchaseAuth.Show(m_activeMoneyOrGTAPPTransaction, isZeroCostLicense: false);
		}
		string errorString = GetHearthstoneCheckoutErrorString(data?.ErrorCodes);
		m_view.PurchaseAuth.CompletePurchaseFailure(m_activeMoneyOrGTAPPTransaction, errorString, Network.PurchaseErrorInfo.ErrorType.BP_GENERIC_FAIL);
		Status = TransactionStatus.READY;
		if (data != null)
		{
			Network.Get().ReportBlizzardCheckoutStatus(BlizzardCheckoutStatus.BLIZZARD_CHECKOUT_STATUS_COMPLETED_FAILED, data);
			TelemetryManager.Client().SendBlizzardCheckoutPurchaseCompletedFailure(data.TransactionID, data.ProductID.ToString(), data.CurrencyCode, new List<string> { data.ErrorCodes ?? string.Empty });
			Log.Store.PrintError("Checkout Order Failure: TransactionID={0}, ProductID={1}, CurrencyCode={2}, ErrorCodes={3}", data.TransactionID, data.ProductID, data.CurrencyCode, data.ErrorCodes);
		}
	}

	public void HandleCommerceSubmitFailure()
	{
		if (!m_view.PurchaseAuth.IsShown)
		{
			m_view.PurchaseAuth.Show(m_activeMoneyOrGTAPPTransaction, isZeroCostLicense: false);
		}
		string errorString = GameStrings.Get("GLUE_CHECKOUT_ERROR_GENERIC_FAILURE");
		m_view.PurchaseAuth.CompletePurchaseFailure(m_activeMoneyOrGTAPPTransaction, errorString, Network.PurchaseErrorInfo.ErrorType.BP_GENERIC_FAIL);
		Status = TransactionStatus.READY;
	}

	private string GetHearthstoneCheckoutErrorString(string errorCode)
	{
		switch (errorCode)
		{
		case "HICKORY402":
			return GameStrings.Get("GLUE_CHECKOUT_HICKORY_ACCOUNT_FAILURE");
		case "HICKORY403":
			return GameStrings.Get("GLUE_CHECKOUT_STEAM_PENDING_FAILURE");
		case "HICKORY404":
			return GameStrings.Get("GLUE_CHECKOUT_STEAM_BLOCKED_FAILURE");
		case "HICKORY408":
			return GameStrings.Get("GLUE_CHECKOUT_HICKORY_TIMEOUT_FAILURE");
		case "HICKORY412":
			return GameStrings.Get("GLUE_CHECKOUT_HICKORY_RECONCILE_FAILURE");
		case "HICKORY424":
			return GameStrings.Get("GLUE_CHECKOUT_HICKORY_OVERLAY_FAILURE");
		case "BLZBNTPURJNL42203":
			return GameStrings.Get("GLUE_STORE_FAIL_PRODUCT_ALREADY_OWNED");
		case "BLZBNTPURJNL42208":
			return GameStrings.Get("GLUE_STORE_FAIL_SPENDING_LIMIT");
		case "BLZBNTPUR3000003":
		case "10201001":
			return GameStrings.Get("GLUE_CHECKOUT_ERROR_INSUFFICIENT_FUNDS");
		case "30000101":
			return GameStrings.Get("GLUE_CHECKOUT_ERROR_PRODUCT_UNAVAILABLE");
		default:
			Log.Store.PrintWarning("Unhandled checkout error: {0}", errorCode);
			return GameStrings.Get("GLUE_CHECKOUT_ERROR_GENERIC_FAILURE");
		}
	}

	public void HandleCommerceOrderComplete(TransactionData data)
	{
		if (IsInTransaction() && !m_view.PurchaseAuth.IsShown)
		{
			m_view.PurchaseAuth.Show(m_activeMoneyOrGTAPPTransaction, isZeroCostLicense: false);
		}
		try
		{
			SendAttributionPurchaseMessage(data);
		}
		catch (Exception arg)
		{
			Log.Store.PrintError($"[SendAttributionPurchaseMessage] Error during purchase attribution message send.\nException: {arg}");
		}
		AdventureStore adventureStore = GetCurrentStore() as AdventureStore;
		if (adventureStore != null)
		{
			adventureStore.Hide();
		}
		HandlePurchaseSuccess(null, m_activeMoneyOrGTAPPTransaction, string.Empty, data);
		if (data != null)
		{
			Network.Get().ReportBlizzardCheckoutStatus(BlizzardCheckoutStatus.BLIZZARD_CHECKOUT_STATUS_COMPLETED_SUCCESS, data);
			TelemetryManager.Client().SendBlizzardCheckoutPurchaseCompletedSuccess(data.TransactionID, data.ProductID.ToString(), data.CurrencyCode);
		}
	}

	private void SendAttributionPurchaseMessage(TransactionData transactionData)
	{
		if (transactionData == null)
		{
			Log.Store.PrintWarning("[SendAttributionPurchaseMessage] No transaction data provided, skipping attribution message.");
		}
		else
		{
			if (transactionData.IsVCPurchase)
			{
				return;
			}
			if (!ServiceManager.TryGet<AdTrackingManager>(out var adTrackingManager))
			{
				Log.Store.PrintWarning("[SendAttributionPurchaseMessage] AdTrackingManager unavailable, skipping attribution message.");
				return;
			}
			ProductInfo bundle = GetBundleFromPmtProductId(transactionData.ProductID);
			if (bundle == null)
			{
				Log.Store.PrintWarning("[SendAttributionPurchaseMessage] Unable to find bundle for PMT Product ID {0}, skipping attribution message.", transactionData.ProductID);
				return;
			}
			if (!bundle.TryGetRMPrice(out var price))
			{
				Log.Store.PrintWarning("[SendAttributionPurchaseMessage] Unable to find RM price for PMT Product ID {0}, skipping attribution message.", transactionData.ProductID);
				return;
			}
			string currencyCode = transactionData.CurrencyCode;
			string transactionId = transactionData.TransactionID;
			string productId = GetExternalStoreProductId(bundle) ?? bundle.Id.ToString();
			adTrackingManager.TrackSale(price, currencyCode, productId, transactionId);
		}
	}

	private bool IsInTransaction()
	{
		if (ServiceManager.TryGet<HearthstoneCheckout>(out var commerce))
		{
			return commerce.IsInProgress;
		}
		return false;
	}

	public bool WillStoreDisplayNotice(NetCache.ProfileNotice.NoticeOrigin noticeOrigin, NetCache.ProfileNotice.NoticeType noticeType, long noticeOriginData)
	{
		return false;
	}
}
