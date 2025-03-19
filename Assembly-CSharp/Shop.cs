using System;
using System.Collections;
using System.Collections.Generic;
using Assets;
using Blizzard.Commerce;
using Blizzard.GameService.SDK.Client.Integration;
using Blizzard.T5.Services;
using Blizzard.Telemetry.WTCG.Client;
using Hearthstone;
using Hearthstone.DataModels;
using Hearthstone.Store;
using Hearthstone.UI;
using UnityEngine;

public class Shop : MonoBehaviour, IStore
{
	[SerializeField]
	private Widget m_widget;

	[SerializeField]
	private UIBScrollable m_browserScroller;

	[SerializeField]
	private VisualController m_shopStateController;

	[SerializeField]
	private AsyncReference m_shopBrowserRef;

	[SerializeField]
	private ProductPageController m_productPageController;

	[SerializeField]
	private ShopTabController m_shopTabController;

	private static Shop s_instance;

	private ShopDataModel m_shopData;

	private ShopBrowser m_browser;

	private WidgetTemplate m_browserWidgetTemplate;

	private bool m_isOpen;

	private bool m_isAnimatingOpenOrClose;

	private Maskable[] m_cameraMasks;

	private Coroutine m_moveToProductCoroutine;

	private readonly PlatformDependentValue<bool> m_unloadUnusedAssetsOnClose = new PlatformDependentValue<bool>(PlatformCategory.Memory)
	{
		LowMemory = true,
		MediumMemory = true,
		HighMemory = false
	};

	private const string OPEN = "OPEN";

	private const string CLOSED = "CLOSED";

	private const string SHOP_GO_BACK = "SHOP_GO_BACK";

	private const string SHOP_SHOW_INFO = "SHOP_SHOW_INFO";

	private const string SHOP_BUY_VC = "SHOP_BUY_VC";

	private const string SHOP_TOGGLE_AUTOCONVERT = "SHOP_TOGGLE_AUTOCONVERT";

	private const string SHOP_BLOCK_INTERFACE = "SHOP_BLOCK_INTERFACE";

	private const string SHOP_UNBLOCK_INTERFACE = "SHOP_UNBLOCK_INTERFACE";

	public ProductPageController ProductPageController => m_productPageController;

	public bool ProductPageTempInstancesInitialized
	{
		get
		{
			if (m_productPageController != null)
			{
				return m_productPageController.TempIntancesInitialized();
			}
			return false;
		}
	}

	public bool IsBrowserReady
	{
		get
		{
			if (m_browser != null)
			{
				return m_browser.IsReady();
			}
			return false;
		}
	}

	public bool IsBrowserDirty
	{
		get
		{
			if (m_browser != null)
			{
				return m_browser.IsDirty();
			}
			return false;
		}
	}

	public static bool DontFullyOpenShop { get; set; }

	public event Action OnOpenCompleted;

	public event Action OnCloseCompleted;

	public event Action OnTiersChanged;

	public event Action OnShopButtonStatusChanged;

	public event Action OnReady;

	public event Action OnOpened;

	public event Action<StoreClosedArgs> OnClosed;

	public event Action OnProductOpened;

	public event Action OnProductClosed;

	event Action<BuyProductEventArgs> IStore.OnProductPurchaseAttempt
	{
		add
		{
		}
		remove
		{
		}
	}

	public static Shop Get()
	{
		return s_instance;
	}

	public bool IsOpen()
	{
		return m_isOpen;
	}

	public bool IsReady()
	{
		return s_instance != null;
	}

	public void Open()
	{
		if (m_isOpen)
		{
			return;
		}
		m_isOpen = true;
		ShownUIMgr.Get().SetShownUI(ShownUIMgr.UI_WINDOW.GENERAL_STORE);
		Navigation.Push(OnNavigateBack);
		PresenceMgr.Get().SetStatus(Global.PresenceStatus.STORE);
		if (ServiceManager.TryGet<IGamemodeAvailabilityService>(out var gas))
		{
			gas.GetGamemodeStatus(IGamemodeAvailabilityService.Gamemode.NONE);
		}
		ServiceManager.Get<IProductDataService>()?.TryRefreshStaleProductAvailability();
		RefreshDataModel();
		base.gameObject.SetActive(value: true);
		m_browser.gameObject.SetActive(!DontFullyOpenShop);
		EnsureShopMovedToOverlayUI();
		if (!DontFullyOpenShop)
		{
			m_isAnimatingOpenOrClose = true;
			UIContext.GetRoot().ShowPopup(m_widget.gameObject, UIContext.BlurType.Layered, UIContext.ProjectionType.Perspective);
			SetMasking(maskingEnabled: true);
			m_shopStateController.SetState("OPEN");
			UpdateScrollerEnabled();
			m_shopTabController.RegisterTabChangedListener(OnTabChanged);
			m_shopTabController.ResetTabKnowledge();
			m_shopTabController.SelectTab(0, 0, instant: true);
			if (ServiceManager.TryGet<IProductDataService>(out var dataService))
			{
				dataService.CacheAllProductsInShop();
			}
		}
		m_productPageController.HandleShopOpen();
		this.OnOpened?.Invoke();
	}

	public void Close()
	{
		Close(forceClose: false);
	}

	public void Close(bool forceClose)
	{
		if (m_isOpen && (forceClose || ((!ServiceManager.TryGet<HearthstoneCheckout>(out var commerce) || !commerce.IsInProgress) && StoreManager.Get().CanTapOutConfirmationUI())))
		{
			m_isOpen = false;
			if (ServiceManager.TryGet<PurchaseManager>(out var purchaseManager))
			{
				purchaseManager.CancelPendingAutoPurchases();
			}
			if (ServiceManager.TryGet<CurrencyManager>(out var currencyManager))
			{
				currencyManager.MarkVirtualCurrencyDirty();
			}
			if (StoreManager.Get() != null && StoreManager.Get().CurrentShopType == ShopType.GENERAL_STORE && SceneMgr.Get() != null && SceneMgr.Get().GetMode() == SceneMgr.Mode.HUB)
			{
				Box.Get()?.GetBoxCamera()?.ChangeState(BoxCamera.State.CLOSED_WITH_DRAWER, force: true);
			}
			Navigation.RemoveHandler(OnNavigateBack);
			if (ShownUIMgr.Get() != null)
			{
				ShownUIMgr.Get().ClearShownUI();
			}
			PresenceMgr.Get().SetPrevStatus();
			if (!DontFullyOpenShop)
			{
				m_isAnimatingOpenOrClose = true;
				m_shopStateController.SetState("CLOSED");
				OnCloseCompleted -= DismissPopup;
				OnCloseCompleted += DismissPopup;
			}
			m_productPageController.HandleShopClosed();
			DontFullyOpenShop = false;
			UpdateScrollerEnabled();
			this.OnClosed?.Invoke(new StoreClosedArgs());
		}
	}

	public void RaiseShopButtonStatusEvent()
	{
		this.OnShopButtonStatusChanged?.Invoke();
	}

	public void BlockInterface(bool blocked)
	{
		if (blocked)
		{
			m_widget.TriggerEvent("SHOP_BLOCK_INTERFACE", TriggerEventParameters.StandardPropagateDownward);
		}
		else
		{
			m_widget.TriggerEvent("SHOP_UNBLOCK_INTERFACE", TriggerEventParameters.StandardPropagateDownward);
		}
	}

	public void Unload()
	{
		Close(forceClose: true);
	}

	IEnumerable<CurrencyType> IStore.GetVisibleCurrencies()
	{
		HashSet<CurrencyType> currencies = new HashSet<CurrencyType> { CurrencyType.GOLD };
		if (IsTavernTicketProductPageOpen())
		{
			currencies.Add(CurrencyType.TAVERN_TICKET);
		}
		if (TryGetCurrentTabs(out var tab, out var subTab))
		{
			if (tab.Id == "battlegrounds")
			{
				currencies.Add(CurrencyType.BG_TOKEN);
			}
			if (tab.Id == "other" && subTab.Id == "tickets")
			{
				currencies.Add(CurrencyType.TAVERN_TICKET);
			}
		}
		if (ShopUtils.IsVirtualCurrencyEnabled())
		{
			if (ShopUtils.TryGetMainVirtualCurrencyType(out var vcType))
			{
				currencies.Add(vcType);
			}
			if (ShopUtils.TryGetBoosterVirtualCurrencyType(out var bcType))
			{
				currencies.Add(bcType);
			}
		}
		return currencies;
	}

	private void Awake()
	{
		s_instance = this;
		m_cameraMasks = GetComponentsInChildren<Maskable>(includeInactive: true);
		StoreManager.Get().RegisterSuccessfulPurchaseAckListener(OnSuccessfulPurchaseAck);
		m_shopBrowserRef.RegisterReadyListener(delegate(ShopBrowser browser)
		{
			if (browser == null)
			{
				Log.Store.PrintError("Browser is null");
			}
			else
			{
				m_browser = browser;
				m_browserWidgetTemplate = browser.GetComponent<WidgetTemplate>();
				if (m_widget != null)
				{
					m_widget.BindDataModel(m_browser.ShopBrowserData);
				}
			}
		});
		CurrencyManager currencyManager = ServiceManager.Get<CurrencyManager>();
		m_shopData = new ShopDataModel
		{
			VirtualCurrency = ProductFactory.CreateEmptyProductDataModel(),
			BoosterCurrency = ProductFactory.CreateEmptyProductDataModel(),
			GoldBalance = currencyManager.GetPriceDataModel(CurrencyType.GOLD),
			DustBalance = currencyManager.GetPriceDataModel(CurrencyType.DUST),
			RenownBalance = currencyManager.GetPriceDataModel(CurrencyType.RENOWN),
			BattlegroundsTokenBalance = currencyManager.GetPriceDataModel(CurrencyType.BG_TOKEN),
			TavernTicketCurrencyBalance = currencyManager.GetPriceDataModel(CurrencyType.TAVERN_TICKET)
		};
		Network.Get().OnConnectedToBattleNet += OnBattleNetConnectionStateChanged;
		OnBattleNetConnectionStateChanged(BattleNetErrors.ERROR_OK);
		GlobalDataContext.Get().BindDataModel(m_shopData);
		m_widget.RegisterEventListener(HandleWidgetEvent);
		m_widget.BindDataModel(m_shopData);
		StoreManager.Get().RegisterAmazingNewShop(this);
		ProductPageController productPageController = m_productPageController;
		productPageController.OnProductPageOpening = (Action<ProductPage>)Delegate.Combine(productPageController.OnProductPageOpening, new Action<ProductPage>(OnPageOpening));
		ProductPageController productPageController2 = m_productPageController;
		productPageController2.OnProductPageOpened = (Action<ProductPage>)Delegate.Combine(productPageController2.OnProductPageOpened, new Action<ProductPage>(OnPageOpened));
		ProductPageController productPageController3 = m_productPageController;
		productPageController3.OnProductPageClosed = (Action<ProductPage>)Delegate.Combine(productPageController3.OnProductPageClosed, new Action<ProductPage>(OnPageClosed));
		this.OnReady?.Invoke();
	}

	private void OnDestroy()
	{
		if (s_instance == this)
		{
			s_instance = null;
		}
		StoreManager.Get()?.RemoveSuccessfulPurchaseAckListener(OnSuccessfulPurchaseAck);
		GlobalDataContext.Get().UnbindDataModel(m_shopData.DataModelId);
		Network network = Network.Get();
		if (network != null)
		{
			network.OnConnectedToBattleNet -= OnBattleNetConnectionStateChanged;
			network.OnDisconnectedFromBattleNet -= OnBattleNetConnectionStateChanged;
		}
	}

	private void Update()
	{
		if (ServiceManager.TryGet<IProductDataService>(out var productDataService) && productDataService.HasTierChangedSinceLastDataModelRetrievel && StoreManager.Get().IsOpen())
		{
			RefreshDataModel();
			RefreshContent();
			this.OnTiersChanged?.Invoke();
		}
	}

	public void RefreshDataModel()
	{
		if (!ServiceManager.TryGet<IProductDataService>(out var dataService))
		{
			Log.Store.PrintError("[Shop.RefreshDataModel] Unable to update store data model");
			return;
		}
		if (!dataService.GetRefreshDataModel(m_shopData))
		{
			Log.Store.PrintError("[Shop.RefreshDataModel] Unable to update store data model");
		}
		if (ServiceManager.TryGet<CurrencyManager>(out var currencyManager))
		{
			currencyManager.RefreshWallet();
		}
		m_shopTabController.SetTabData(m_shopData);
	}

	public void SetShopBrowserHidden(bool hidden)
	{
		if (m_browser == null)
		{
			return;
		}
		if (!hidden)
		{
			m_browserWidgetTemplate.Show();
			if (m_browser.IsReady())
			{
				m_browserWidgetTemplate.TriggerEvent("TIER_SLOTS_LOADED");
			}
		}
		else
		{
			m_browserWidgetTemplate.Hide();
		}
	}

	public void MoveToTab(ShopTabData parentTab, ShopTabData subTab, bool isImmediate = false)
	{
		if (parentTab != null && !(m_shopTabController == null))
		{
			m_shopTabController.SelectTab(parentTab.Id, subTab?.Id, isImmediate);
		}
	}

	public void MoveToProduct(ProductDataModel product)
	{
		if (product != null && m_isOpen)
		{
			if (m_moveToProductCoroutine != null)
			{
				StopCoroutine(m_moveToProductCoroutine);
				m_moveToProductCoroutine = null;
			}
			m_moveToProductCoroutine = StartCoroutine(MoveToProductCoroutine(product));
		}
	}

	public void Debug_FillBrowser(IEnumerable<ProductTierDataModel> tiers)
	{
		m_browser.SetData(tiers);
	}

	public bool TryGetCurrentTabIds(out string mainTabId, out string subTabId)
	{
		mainTabId = string.Empty;
		subTabId = string.Empty;
		if (m_shopData != null && m_shopData.CurrentTab != null)
		{
			mainTabId = m_shopData.CurrentTab.Id;
			if (m_shopData.CurrentSubTab != null)
			{
				subTabId = m_shopData.CurrentSubTab.Id;
				return true;
			}
		}
		return false;
	}

	public bool TryGetCurrentTabs(out ShopTabDataModel tab, out ShopTabDataModel subTab)
	{
		tab = null;
		subTab = null;
		if (m_shopData == null || m_shopData.CurrentTab == null || m_shopData.CurrentSubTab == null)
		{
			return false;
		}
		tab = m_shopData.CurrentTab;
		subTab = m_shopData.CurrentSubTab;
		return true;
	}

	private void HandleWidgetEvent(string eventName)
	{
		switch (eventName)
		{
		case "SHOP_GO_BACK":
			if (m_isOpen && !Navigation.BackStackContainsHandler(OnNavigateBack))
			{
				Close();
			}
			else
			{
				Navigation.GoBack();
			}
			break;
		case "SHOP_SHOW_INFO":
			StoreManager.Get().ShowStoreInfo();
			break;
		case "SHOP_TOGGLE_AUTOCONVERT":
			m_shopData.AutoconvertCurrency = !m_shopData.AutoconvertCurrency;
			Options.Get().SetBool(Option.AUTOCONVERT_VIRTUAL_CURRENCY, m_shopData.AutoconvertCurrency);
			break;
		case "SHOP_BUY_VC":
			m_productPageController.OpenVirtualCurrencyPurchase(0f, rememberLastPage: true);
			break;
		}
	}

	public bool IsTavernTicketProduct(ProductInfo bundle)
	{
		if (bundle == null)
		{
			return false;
		}
		if (!ServiceManager.TryGet<IProductDataService>(out var dataService))
		{
			return false;
		}
		if (!bundle.Id.IsValid())
		{
			return false;
		}
		ProductDataModel productDataModel = dataService.GetProductDataModel(bundle.Id.Value);
		if (productDataModel != null && productDataModel.Tags.Contains("arena_ticket"))
		{
			return true;
		}
		return false;
	}

	public bool IsTavernTicketProductPageOpen()
	{
		if (m_productPageController == null)
		{
			return false;
		}
		if (IsTavernTicketProductPage(m_productPageController.CurrentProductPage) && m_productPageController.CurrentProductPage.IsOpen)
		{
			return true;
		}
		return false;
	}

	private bool IsTavernTicketProductPage(ProductPage page)
	{
		if (page != null && page.Product != null)
		{
			return page.Product.Tags.Contains("arena_ticket");
		}
		return false;
	}

	private void RefreshContent()
	{
		if (m_shopTabController.ActiveTabIndex >= 0 && m_shopTabController.ActiveSubTabIndex >= 0)
		{
			RefreshContent(m_shopTabController.ActiveTabIndex, m_shopTabController.ActiveSubTabIndex);
		}
	}

	private void RefreshContent(int tabIndex, int subTabIndex)
	{
		m_shopData.CurrentTab = (m_shopTabController.TryGetActiveMainTabDataModel(out var tabDataModel) ? tabDataModel : new ShopTabDataModel());
		m_shopData.CurrentSubTab = (m_shopTabController.TryGetActiveSubTabDataModel(out var subTabDataModel) ? subTabDataModel : new ShopTabDataModel());
		m_browserScroller.SetScroll(0f);
		if (tabIndex < 0 || tabIndex >= m_shopData.Pages.Count)
		{
			SetContentError($"Tab index {tabIndex} is out of range of pages");
			return;
		}
		ShopPageDataModel page = m_shopData.Pages[tabIndex];
		if (page?.ShopSubPages == null)
		{
			SetContentError($"Page at index {tabIndex} is null");
			return;
		}
		if (page.ShopSubPages.Count > 0)
		{
			if (subTabIndex < 0 || subTabIndex >= page.ShopSubPages.Count)
			{
				SetContentError($"Sub tab index {subTabIndex} within page at index {tabIndex} is out of range of sub pages");
				return;
			}
			ShopSubPageDataModel subPage = page.ShopSubPages[subTabIndex];
			if (subPage?.Tiers == null)
			{
				SetContentError($"Sub page at index {subTabIndex} within page at index {tabIndex} is null");
				return;
			}
			if (m_browser != null)
			{
				m_browser.SetData(subPage.Tiers);
			}
		}
		else if (m_browser != null)
		{
			m_browser.ClearData();
		}
		BnetBar.Get()?.RefreshCurrency();
	}

	private void ForceRefreshAfterPurchase(ProductId productId)
	{
		if (ServiceManager.TryGet<IProductDataService>(out var dataService) && productId.IsValid())
		{
			ProductDataModel productDataModel = dataService.GetProductDataModel(productId.Value);
			if (productDataModel != null && productDataModel.Tags.Contains("refresh_after_purchase"))
			{
				m_browser.ForceRefresh();
			}
		}
	}

	private void SetContentError(string error)
	{
		Log.Store.PrintError("Failed to load content - " + (string.IsNullOrEmpty(error) ? "Unknown" : error));
		if (m_browser != null)
		{
			m_browser.ClearData();
		}
	}

	private void DismissPopup()
	{
		SetMasking(maskingEnabled: false);
		UIContext.GetRoot().DismissPopup(m_widget.gameObject);
		OnCloseCompleted -= DismissPopup;
	}

	private void SetMasking(bool maskingEnabled)
	{
		if (m_cameraMasks != null)
		{
			Maskable[] cameraMasks = m_cameraMasks;
			foreach (Maskable obj in cameraMasks)
			{
				obj.enabled = maskingEnabled;
				obj.SetVisibility(maskingEnabled, isInternal: false);
			}
		}
	}

	private void UpdateScrollerEnabled()
	{
		bool enableScroll = m_isOpen && !m_productPageController.IsOpen && !m_isAnimatingOpenOrClose;
		m_browserScroller.enabled = enableScroll;
		m_browserScroller.SetHideThumb(!enableScroll);
	}

	private void EnsureShopMovedToOverlayUI()
	{
		OverlayUI overlayUI = OverlayUI.Get();
		if ((bool)overlayUI && (bool)m_widget && !overlayUI.HasObject(m_widget.gameObject))
		{
			overlayUI.AddGameObject(m_widget.gameObject);
		}
	}

	private void OnBattleNetConnectionStateChanged(BattleNetErrors bnetErrors)
	{
		if (ServiceManager.TryGet<CurrencyManager>(out var currencyManager))
		{
			if (ShopUtils.TryGetMainVirtualCurrencyType(out var vcType))
			{
				m_shopData.VirtualCurrencyBalance = currencyManager.GetPriceDataModel(vcType);
			}
			else
			{
				m_shopData.VirtualCurrencyBalance = currencyManager.GetPriceDataModel(CurrencyType.NONE);
			}
			if (ShopUtils.TryGetBoosterVirtualCurrencyType(out var bcType))
			{
				m_shopData.BoosterCurrencyBalance = currencyManager.GetPriceDataModel(bcType);
			}
			else
			{
				m_shopData.BoosterCurrencyBalance = currencyManager.GetPriceDataModel(CurrencyType.NONE);
			}
		}
	}

	private void OnPageOpening(ProductPage page)
	{
		this.OnProductOpened?.Invoke();
		EnsureShopMovedToOverlayUI();
	}

	private void OnPageOpened(ProductPage page)
	{
		UpdateScrollerEnabled();
	}

	private void OnPageClosed(ProductPage page)
	{
		UpdateScrollerEnabled();
		this.OnProductClosed?.Invoke();
		if (m_productPageController != null && DontFullyOpenShop && (!m_productPageController.IsVCPage(m_productPageController.CurrentProductPage) || !m_productPageController.IsOpening) && !m_productPageController.IsReopening)
		{
			SoundManager.Get().LoadAndPlay("Store_window_shrink.prefab:b68247126e211224e8a904142d2a9895", base.gameObject);
			Close();
		}
	}

	private void OnTabChanged(int tabIndex, int subTabIndex)
	{
		RefreshContent(tabIndex, subTabIndex);
	}

	private bool OnNavigateBack()
	{
		Close();
		return true;
	}

	private void OnSuccessfulPurchaseAck(ProductInfo bundle, PaymentMethod paymentMethod)
	{
		if (ServiceManager.TryGet<IProductDataService>(out var dataService))
		{
			dataService.UpdateProductStatus();
			if (bundle != null)
			{
				ForceRefreshAfterPurchase(bundle.Id);
			}
		}
	}

	private void CompleteOpen()
	{
		m_isAnimatingOpenOrClose = false;
		this.OnOpenCompleted?.Invoke();
		UpdateScrollerEnabled();
		MusicManager.Get().StartPlaylist(MusicPlaylistType.UI_Store);
	}

	private void CompleteClose()
	{
		m_isAnimatingOpenOrClose = false;
		this.OnCloseCompleted?.Invoke();
		UpdateScrollerEnabled();
		SetMasking(maskingEnabled: false);
		if ((bool)m_unloadUnusedAssetsOnClose && HearthstoneApplication.Get() != null)
		{
			HearthstoneApplication.Get().UnloadUnusedAssets();
		}
		Box box = Box.Get();
		if (box != null)
		{
			box.PlayBoxMusic();
		}
	}

	private IEnumerator MoveToProductCoroutine(ProductDataModel product)
	{
		while (!m_browser.IsReady() || m_browser.IsDirty())
		{
			yield return null;
		}
		if (ServiceManager.TryGet<IProductDataService>(out var pas) && pas.TryGetSubTabFromProductId(product.PmtId, out var parentTab, out var subTab))
		{
			if (!parentTab.Enabled || parentTab.Locked || !subTab.Enabled || subTab.Locked)
			{
				Log.Store.PrintWarning($"Attempted to open product {product.PmtId} but Tab({parentTab.Id})/SubTab({subTab.Id}) is unavailable! Skipping...");
				yield break;
			}
			m_shopTabController.SelectTab(parentTab.Id, subTab.Id, instant: true);
		}
		if (TryGetActiveShopSlot(product, out var slot))
		{
			m_browserScroller.CenterObjectInView(slot.gameObject, 0f, null, iTween.EaseType.easeInExpo, 0.2f, blockInputWhileScrolling: true);
		}
		m_moveToProductCoroutine = null;
	}

	public bool TryGetActiveShopSlot(ProductDataModel product, out ShopSlot shopSlot)
	{
		shopSlot = null;
		foreach (ShopSection activeSection in m_browser.GetActiveSections())
		{
			foreach (ShopSlot activeSlot in activeSection.GetActiveSlots())
			{
				if (activeSlot.HasProduct(product))
				{
					shopSlot = activeSlot;
					return true;
				}
			}
		}
		Log.Store.PrintWarning($"[Shop::TryGetActiveShopSlot] Product {product.PmtId} not found");
		return false;
	}

	public ShopVisit GetShopVisitTelemetryData()
	{
		if (m_browser == null)
		{
			return null;
		}
		if (StoreManager.Get() == null)
		{
			return null;
		}
		if (m_shopData == null || m_shopData.CurrentTab == null || m_shopData.CurrentSubTab == null)
		{
			return null;
		}
		return new ShopVisit
		{
			Cards = m_browser.GetShopCardTelemetry(),
			StoreType = StoreManager.Get().CurrentShopType.ToString(),
			ShopTab = m_shopData.CurrentTab.Id,
			ShopSubTab = m_shopData.CurrentSubTab.Id,
			LoadTimeSeconds = 0f
		};
	}
}
