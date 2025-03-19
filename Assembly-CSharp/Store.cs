using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Blizzard.T5.Core.Utils;
using Blizzard.T5.MaterialService.Extensions;
using Blizzard.T5.Services;
using Hearthstone.DataModels;
using Hearthstone.Store;
using Hearthstone.UI;
using UnityEngine;

[CustomEditClass]
public abstract class Store : UIBPopup, IStore
{
	protected enum BuyButtonState
	{
		ENABLED,
		DISABLED_NOT_ENOUGH_GOLD,
		DISABLED_NOT_ENOUGH_VC,
		DISABLED_NOT_ENOUGH_BC,
		DISABLED_FEATURE,
		DISABLED,
		DISABLED_OWNED,
		DISABLED_NO_TOOLTIP
	}

	private class BuyButtonInternal
	{
		public UIBButton m_button;

		public TooltipZone m_tooltipZone;

		public PegUIElement m_tooltipTrigger;

		public List<MeshRenderer> m_meshes;

		public Material m_enabledMaterial;

		public Material m_disabledMaterial;

		public string m_toolTipHeadlineStringId;

		public UIEvent.Handler m_buyHandler;

		public Func<string> m_getOwnedTooltipStringHandler;

		public bool m_silenceReleaseHandler;

		private BuyButtonState m_state;

		private bool m_forceDisabled;

		public BuyButtonState State
		{
			get
			{
				return m_state;
			}
			set
			{
				m_state = value;
				Refresh();
			}
		}

		public bool ForceDisabled
		{
			set
			{
				m_forceDisabled = value;
				Refresh();
			}
		}

		public void Init()
		{
			if (m_button != null)
			{
				m_button.AddEventListener(UIEventType.RELEASE, OnReleased);
			}
			if (m_tooltipTrigger != null)
			{
				m_tooltipTrigger.AddEventListener(UIEventType.ROLLOVER, OnShowTooltip);
				m_tooltipTrigger.AddEventListener(UIEventType.ROLLOUT, OnHideTooltip);
			}
		}

		public void OnDestroy()
		{
			if (m_button != null)
			{
				m_button.RemoveEventListener(UIEventType.RELEASE, OnReleased);
			}
			if (m_tooltipTrigger != null)
			{
				m_tooltipTrigger.RemoveEventListener(UIEventType.ROLLOVER, OnShowTooltip);
				m_tooltipTrigger.RemoveEventListener(UIEventType.ROLLOUT, OnHideTooltip);
			}
			m_enabledMaterial = null;
			m_disabledMaterial = null;
		}

		private void Refresh()
		{
			bool isEnabled = !m_forceDisabled && m_state == BuyButtonState.ENABLED;
			Material newMaterial = (isEnabled ? m_enabledMaterial : m_disabledMaterial);
			foreach (MeshRenderer mesh in m_meshes)
			{
				if (mesh != null)
				{
					mesh.SetSharedMaterial(newMaterial);
				}
			}
			if (m_button != null)
			{
				Collider collider = m_button.GetComponent<Collider>();
				if (collider != null)
				{
					collider.enabled = isEnabled;
				}
			}
			if (m_tooltipTrigger != null)
			{
				m_tooltipTrigger.gameObject.SetActive(!isEnabled && m_state != BuyButtonState.DISABLED_NO_TOOLTIP);
			}
		}

		private void OnReleased(UIEvent e)
		{
			if (!m_forceDisabled && m_state == BuyButtonState.ENABLED && !m_silenceReleaseHandler)
			{
				m_buyHandler(e);
			}
		}

		private void OnShowTooltip(UIEvent e)
		{
			if (m_state != 0)
			{
				m_tooltipZone.ShowLayerTooltip(GameStrings.Get(m_toolTipHeadlineStringId), GetBuyButtonTooltipMessage(m_state));
			}
		}

		private void OnHideTooltip(UIEvent e)
		{
			m_tooltipZone.HideTooltip();
		}

		private string GetBuyButtonTooltipMessage(BuyButtonState state)
		{
			switch (state)
			{
			case BuyButtonState.DISABLED_OWNED:
				return m_getOwnedTooltipStringHandler();
			case BuyButtonState.DISABLED_NOT_ENOUGH_GOLD:
				return GameStrings.Get("GLUE_STORE_FAIL_NOT_ENOUGH_GOLD");
			case BuyButtonState.DISABLED_NOT_ENOUGH_VC:
			{
				if (ShopUtils.TryGetMainVirtualCurrencyType(out var vcType) && (vcType == CurrencyType.CN_RUNESTONES || vcType == CurrencyType.ROW_RUNESTONES))
				{
					return GameStrings.Get("GLUE_STORE_FAIL_NOT_ENOUGH_RUNESTONES");
				}
				Log.Store.PrintError($"Cannot get relating button state for Not Enough VC as the currency type {vcType} isn't setup (or shouldn't be)");
				return GameStrings.Get("GLUE_TOOLTIP_BUTTON_DISABLED_DESC");
			}
			case BuyButtonState.DISABLED_NOT_ENOUGH_BC:
			{
				if (ShopUtils.TryGetBoosterVirtualCurrencyType(out var bcType) && bcType == CurrencyType.CN_ARCANE_ORBS)
				{
					return GameStrings.Get("GLUE_STORE_FAIL_NOT_ENOUGH_ARCANE_ORBS");
				}
				Log.Store.PrintError($"Cannot get relating button state for Not Enough VC as the currency type {bcType} isn't setup (or shouldn't be)");
				return GameStrings.Get("GLUE_TOOLTIP_BUTTON_DISABLED_DESC");
			}
			case BuyButtonState.DISABLED_FEATURE:
				return GameStrings.Get("GLUE_STORE_DISABLED");
			case BuyButtonState.DISABLED_NO_TOOLTIP:
				return string.Empty;
			default:
				return GameStrings.Get("GLUE_TOOLTIP_BUTTON_DISABLED_DESC");
			}
		}
	}

	public delegate void ExitCallback(bool authorizationBackButtonPressed, object userData);

	public delegate void InfoCallback(object userData);

	private class InfoListener : EventListener<InfoCallback>
	{
		public void Fire()
		{
			m_callback(m_userData);
		}
	}

	[CustomEditField(Sections = "Store/UI")]
	public GameObject m_root;

	[CustomEditField(Sections = "Store/UI")]
	public GameObject m_cover;

	[CustomEditField(Sections = "Store/UI")]
	public UIBButton m_buyWithMoneyButton;

	[CustomEditField(Sections = "Store/UI")]
	public TooltipZone m_buyWithMoneyTooltip;

	[CustomEditField(Sections = "Store/UI")]
	public PegUIElement m_buyWithMoneyTooltipTrigger;

	[CustomEditField(Sections = "Store/UI")]
	public UIBButton m_buyWithGoldButton;

	[CustomEditField(Sections = "Store/UI")]
	public TooltipZone m_buyWithGoldTooltip;

	[CustomEditField(Sections = "Store/UI")]
	public PegUIElement m_buyWithGoldTooltipTrigger;

	[CustomEditField(Sections = "Store/UI")]
	public UIBButton m_buyWithVCButton;

	[CustomEditField(Sections = "Store/UI")]
	public TooltipZone m_buyWithVCTooltip;

	[CustomEditField(Sections = "Store/UI")]
	public PegUIElement m_buyWithVCTooltipTrigger;

	[CustomEditField(Sections = "Store/UI")]
	public UIBButton m_infoButton;

	[CustomEditField(Sections = "Store/Materials")]
	public List<MeshRenderer> m_goldButtonMeshes = new List<MeshRenderer>();

	[CustomEditField(Sections = "Store/Materials")]
	public Material m_enabledGoldButtonMaterial;

	[CustomEditField(Sections = "Store/Materials")]
	public Material m_disabledGoldButtonMaterial;

	[CustomEditField(Sections = "Store/Materials")]
	public List<MeshRenderer> m_moneyButtonMeshes = new List<MeshRenderer>();

	[CustomEditField(Sections = "Store/Materials")]
	public Material m_enabledMoneyButtonMaterial;

	[CustomEditField(Sections = "Store/Materials")]
	public Material m_disabledMoneyButtonMaterial;

	[CustomEditField(Sections = "Store/Materials")]
	public List<MeshRenderer> m_vcButtonMeshes = new List<MeshRenderer>();

	[CustomEditField(Sections = "Store/Materials")]
	public Material m_enabledVCButtonMaterial;

	[CustomEditField(Sections = "Store/Materials")]
	public Material m_disabledVCButtonMaterial;

	[CustomEditField(Sections = "Store/UI")]
	public PegUIElement m_offClicker;

	protected bool m_hasRequestedFullscreenFX;

	protected bool m_restoreWhenShopHides;

	private BuyButtonInternal m_goldButtonInternal;

	private BuyButtonInternal m_moneyButtonInternal;

	private BuyButtonInternal m_vcButtonInternal;

	private readonly List<BuyButtonInternal> m_buyButtons = new List<BuyButtonInternal>();

	private readonly List<InfoListener> m_infoListeners = new List<InfoListener>();

	private ScreenEffectsHandle m_screenEffectsHandle;

	public event Action<BuyProductEventArgs> OnProductPurchaseAttempt;

	public event Action OnOpened;

	public event Action<StoreClosedArgs> OnClosed;

	public event Action OnReady;

	public event Action OnProductOpened;

	protected override void Awake()
	{
		m_goldButtonInternal = new BuyButtonInternal
		{
			m_button = m_buyWithGoldButton,
			m_tooltipZone = m_buyWithGoldTooltip,
			m_tooltipTrigger = m_buyWithGoldTooltipTrigger,
			m_meshes = m_goldButtonMeshes,
			m_enabledMaterial = m_enabledGoldButtonMaterial,
			m_disabledMaterial = m_disabledGoldButtonMaterial,
			m_toolTipHeadlineStringId = "GLUE_STORE_GOLD_BUTTON_TOOLTIP_HEADLINE",
			m_buyHandler = BuyWithGold,
			m_getOwnedTooltipStringHandler = GetOwnedTooltipString
		};
		m_buyButtons.Add(m_goldButtonInternal);
		m_moneyButtonInternal = new BuyButtonInternal
		{
			m_button = m_buyWithMoneyButton,
			m_tooltipZone = m_buyWithMoneyTooltip,
			m_tooltipTrigger = m_buyWithMoneyTooltipTrigger,
			m_meshes = m_moneyButtonMeshes,
			m_enabledMaterial = m_enabledMoneyButtonMaterial,
			m_disabledMaterial = m_disabledMoneyButtonMaterial,
			m_toolTipHeadlineStringId = "GLUE_STORE_MONEY_BUTTON_TOOLTIP_HEADLINE",
			m_buyHandler = BuyWithMoney,
			m_getOwnedTooltipStringHandler = GetOwnedTooltipString
		};
		m_buyButtons.Add(m_moneyButtonInternal);
		m_vcButtonInternal = new BuyButtonInternal
		{
			m_button = m_buyWithVCButton,
			m_tooltipZone = m_buyWithVCTooltip,
			m_tooltipTrigger = m_buyWithVCTooltipTrigger,
			m_meshes = m_vcButtonMeshes,
			m_enabledMaterial = m_enabledVCButtonMaterial,
			m_disabledMaterial = m_disabledVCButtonMaterial,
			m_toolTipHeadlineStringId = "GLUE_STORE_VC_BUTTON_TOOLTIP_HEADLINE",
			m_buyHandler = BuyWithVirtualCurrency,
			m_getOwnedTooltipStringHandler = GetOwnedTooltipString
		};
		m_buyButtons.Add(m_vcButtonInternal);
		base.Awake();
		m_infoButton.SetText(GameStrings.Get("GLUE_STORE_INFO_BUTTON_TEXT"));
		m_screenEffectsHandle = new ScreenEffectsHandle(this);
	}

	protected override void Start()
	{
		base.Start();
		foreach (BuyButtonInternal buyButton in m_buyButtons)
		{
			buyButton.Init();
		}
		m_infoButton.AddEventListener(UIEventType.RELEASE, OnInfoPressed);
		NetCache.Get().RegisterGoldBalanceListener(OnStoreGoldBalanceChanged);
		StoreManager.Get().RegisterSuccessfulPurchaseListener(OnSuccessfulPurchase);
		StartCoroutine(NotifyListenersWhenReady());
		if (!m_shown)
		{
			Hide();
		}
		if (ServiceManager.TryGet<CurrencyManager>(out var currencyManager))
		{
			currencyManager.AddCurrencyBalanceChangedCallback(OnCurrencyBalanceChangedInternal);
		}
		Shop shop = Shop.Get();
		if (shop != null)
		{
			ProductPageController productPageController = shop.ProductPageController;
			productPageController.OnProductPageOpened = (Action<ProductPage>)Delegate.Combine(productPageController.OnProductPageOpened, new Action<ProductPage>(OnShopProductPageOpened));
			ProductPageController productPageController2 = shop.ProductPageController;
			productPageController2.OnProductPageClosed = (Action<ProductPage>)Delegate.Combine(productPageController2.OnProductPageClosed, new Action<ProductPage>(OnShopProductPageClosed));
		}
	}

	private void OnStoreGoldBalanceChanged(NetCache.NetCacheGoldBalance balance)
	{
		if (IsOpen())
		{
			OnGoldBalanceChanged(balance);
		}
	}

	private void OnCurrencyBalanceChangedInternal(CurrencyBalanceChangedEventArgs args)
	{
		if (IsOpen())
		{
			OnCurrencyBalanceChanged(args);
		}
	}

	private void OnSuccessfulPurchase(ProductInfo bundle, PaymentMethod paymentMethod)
	{
		if (paymentMethod == PaymentMethod.MONEY && IsOpen())
		{
			OnMoneySpent();
		}
	}

	protected virtual void OnDestroy()
	{
		if (NetCache.Get() != null)
		{
			NetCache.Get().RemoveGoldBalanceListener(OnStoreGoldBalanceChanged);
		}
		if (ServiceManager.TryGet<CurrencyManager>(out var currencyManager))
		{
			currencyManager.RemoveCurrencyBalanceChangedCallback(OnCurrencyBalanceChangedInternal);
		}
		Shop shop = Shop.Get();
		if (shop != null)
		{
			ProductPageController productPageController = shop.ProductPageController;
			productPageController.OnProductPageOpened = (Action<ProductPage>)Delegate.Remove(productPageController.OnProductPageOpened, new Action<ProductPage>(OnShopProductPageOpened));
			ProductPageController productPageController2 = shop.ProductPageController;
			productPageController2.OnProductPageClosed = (Action<ProductPage>)Delegate.Remove(productPageController2.OnProductPageClosed, new Action<ProductPage>(OnShopProductPageClosed));
		}
		StoreManager.Get().RemoveSuccessfulPurchaseListener(OnSuccessfulPurchase);
		foreach (BuyButtonInternal buyButton in m_buyButtons)
		{
			buyButton.OnDestroy();
		}
		m_buyButtons.Clear();
		m_enabledGoldButtonMaterial = null;
		m_disabledGoldButtonMaterial = null;
		m_enabledMoneyButtonMaterial = null;
		m_disabledMoneyButtonMaterial = null;
		m_enabledVCButtonMaterial = null;
		m_disabledVCButtonMaterial = null;
		if (FullScreenFXMgr.Get() != null && m_hasRequestedFullscreenFX)
		{
			EnableFullScreenEffects(enable: false);
		}
	}

	public virtual void OnGoldBalanceChanged(NetCache.NetCacheGoldBalance balance)
	{
	}

	public virtual void OnMoneySpent()
	{
	}

	public virtual void OnCurrencyBalanceChanged(CurrencyBalanceChangedEventArgs args)
	{
	}

	public void Show(bool isTotallyFake, bool useOverlayUI, IDataModel dataModel)
	{
		if (dataModel != null)
		{
			WidgetTemplate widget = GetComponentInParent<WidgetTemplate>();
			if (widget != null)
			{
				widget.BindDataModel(dataModel, base.gameObject);
			}
		}
		m_useOverlayUI = useOverlayUI;
		StartCoroutine(ShowWhenReady(isTotallyFake));
	}

	public void Open()
	{
		StartCoroutine(ShowWhenReady(isTotallyFake: false));
	}

	public bool IsOpen()
	{
		return IsShown();
	}

	public virtual void Close()
	{
		Hide();
	}

	public virtual bool IsReady()
	{
		return true;
	}

	public bool IsCovered()
	{
		return m_cover.activeSelf;
	}

	public void BlockInterface(bool blocked)
	{
		if (m_cover != null)
		{
			m_cover.SetActive(blocked);
		}
		ForceDisableBuyButtons(blocked);
	}

	public void EnableClickCatcher(bool enabled)
	{
		m_offClicker.gameObject.SetActive(enabled);
	}

	public bool RegisterInfoListener(InfoCallback callback)
	{
		return RegisterInfoListener(callback, null);
	}

	public bool RegisterInfoListener(InfoCallback callback, object userData)
	{
		InfoListener listener = new InfoListener();
		listener.SetCallback(callback);
		listener.SetUserData(userData);
		if (m_infoListeners.Contains(listener))
		{
			return false;
		}
		m_infoListeners.Add(listener);
		return true;
	}

	public bool RemoveInfoListener(InfoCallback callback, object userData = null)
	{
		InfoListener listener = new InfoListener();
		listener.SetCallback(callback);
		listener.SetUserData(userData);
		return m_infoListeners.Remove(listener);
	}

	public void Unload()
	{
		UnityEngine.Object.Destroy(base.gameObject);
	}

	protected virtual void BuyWithGold(UIEvent e)
	{
	}

	protected virtual void BuyWithMoney(UIEvent e)
	{
	}

	protected virtual void BuyWithVirtualCurrency(UIEvent e)
	{
	}

	protected abstract void ShowImpl(bool isTotallyFake);

	protected void FireOpenedEvent()
	{
		if (this.OnOpened != null)
		{
			this.OnOpened();
		}
	}

	public void FireExitEvent(bool authorizationBackButtonPressed)
	{
		if (this.OnClosed != null)
		{
			this.OnClosed(new StoreClosedArgs(authorizationBackButtonPressed));
		}
	}

	protected void FireProductOpenedEvent()
	{
		this.OnProductOpened?.Invoke();
	}

	protected void EnableFullScreenEffects(bool enable)
	{
		if (enable)
		{
			m_hasRequestedFullscreenFX = true;
			ScreenEffectParameters screenEffectParameters = ScreenEffectParameters.BlurVignetteDesaturatePerspective;
			screenEffectParameters.Time = 1f;
			m_screenEffectsHandle.StartEffect(screenEffectParameters);
		}
		else
		{
			m_screenEffectsHandle.StopEffect();
		}
	}

	protected void FireBuyWithMoneyEvent(ProductInfo bundle, int quantity)
	{
		if (this.OnProductPurchaseAttempt != null)
		{
			this.OnProductPurchaseAttempt(new BuyPmtProductEventArgs(bundle.Id.Value, CurrencyType.REAL_MONEY, quantity));
		}
	}

	protected void FireBuyWithGoldEventGTAPP(ProductInfo bundle, int quantity)
	{
		if (this.OnProductPurchaseAttempt != null)
		{
			this.OnProductPurchaseAttempt(new BuyPmtProductEventArgs(bundle.Id.Value, CurrencyType.GOLD, quantity));
		}
	}

	protected void FireBuyWithGoldEventNoGTAPP(NoGTAPPTransactionData noGTAPPTransactionData)
	{
		if (this.OnProductPurchaseAttempt != null)
		{
			this.OnProductPurchaseAttempt(new BuyNoGTAPPEventArgs(noGTAPPTransactionData));
		}
	}

	protected void FireBuyWithVirtualCurrencyEvent(ProductInfo bundle, CurrencyType currencyType, int quantity = 1)
	{
		if (this.OnProductPurchaseAttempt == null)
		{
			return;
		}
		if (bundle == null)
		{
			Log.Store.PrintError("FireBuyWithVirtualCurrencyEvent error: bundle is null");
			return;
		}
		if (currencyType == CurrencyType.NONE)
		{
			Log.Store.PrintError("FireBuyWithVirtualCurrencyEvent error: currency type is None, does the bundle have VC?");
			return;
		}
		if (!ShopUtils.IsVirtualCurrencyEnabled())
		{
			Log.Store.PrintError("FireBuyWithVirtualCurrencyEvent error: Virtual Currency is not enabled.");
			return;
		}
		if (!ShopUtils.IsVirtualCurrencyTypeEnabled(currencyType))
		{
			Log.Store.PrintError("FireBuyWithVirtualCurrencyEvent error: Currency Type is not enabled via VC: {0}", currencyType);
			return;
		}
		if (!ServiceManager.TryGet<IProductDataService>(out var dataService))
		{
			Log.Store.PrintError("FireBuyWithVirtualCurrencyEvent error: Service is not available");
			return;
		}
		ProductDataModel product = dataService.GetProductDataModel(bundle.Id.Value);
		PriceDataModel price = product.Prices.FirstOrDefault((PriceDataModel p) => p.Currency == currencyType);
		ServiceManager.Get<PurchaseManager>().PurchaseProduct(product, price, quantity);
	}

	protected void SetGoldButtonState(BuyButtonState state)
	{
		m_goldButtonInternal.State = state;
	}

	protected BuyButtonState GetGoldButtonState()
	{
		return m_goldButtonInternal.State;
	}

	protected void SetMoneyButtonState(BuyButtonState state)
	{
		m_moneyButtonInternal.State = state;
	}

	protected BuyButtonState GetMoneyButtonState()
	{
		return m_moneyButtonInternal.State;
	}

	protected void SetVCButtonState(BuyButtonState state)
	{
		m_vcButtonInternal.State = state;
	}

	protected BuyButtonState GetVCButtonState()
	{
		return m_vcButtonInternal.State;
	}

	private IEnumerator ShowWhenReady(bool isTotallyFake)
	{
		VisualController visualController = GetComponent<VisualController>();
		while (visualController != null && visualController.IsChangingStates)
		{
			yield return null;
		}
		ShowImpl(isTotallyFake);
	}

	private void ForceDisableBuyButtons(bool forceDisable)
	{
		foreach (BuyButtonInternal buyButton in m_buyButtons)
		{
			buyButton.ForceDisabled = forceDisable;
		}
	}

	private IEnumerator NotifyListenersWhenReady()
	{
		while (!IsReady())
		{
			yield return null;
		}
		this.OnReady?.Invoke();
	}

	protected void OnInfoPressed(UIEvent e)
	{
		InfoListener[] array = m_infoListeners.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Fire();
		}
	}

	protected virtual string GetOwnedTooltipString()
	{
		return GameStrings.Get("GLUE_STORE_DUNGEON_BUTTON_TEXT_PURCHASED");
	}

	protected void SilenceBuyButtons()
	{
		foreach (BuyButtonInternal buyButton in m_buyButtons)
		{
			buyButton.m_silenceReleaseHandler = true;
		}
	}

	protected virtual void RefreshBuyButtonStates(ProductInfo bundle, NoGTAPPTransactionData transaction)
	{
		if (bundle == null)
		{
			Log.Store.PrintError("Store.RefreshBuyButtonStates failed. Product does not exist.");
			return;
		}
		BuyButtonState buyWithMoneyState = DetermineBuyButtonState(bundle, transaction, CurrencyType.REAL_MONEY);
		SetMoneyButtonState(buyWithMoneyState);
		BuyButtonState buyWithGoldState = DetermineBuyButtonState(bundle, transaction, CurrencyType.GOLD);
		SetGoldButtonState(buyWithGoldState);
		CurrencyType vcType = bundle.GetFirstVirtualCurrencyPriceType();
		BuyButtonState buyWithVCState = DetermineBuyButtonState(bundle, transaction, vcType);
		SetVCButtonState(buyWithVCState);
	}

	protected static BuyButtonState DetermineBuyButtonState(ProductInfo bundle, NoGTAPPTransactionData transaction, CurrencyType currencyType)
	{
		if (!ServiceManager.TryGet<IProductDataService>(out var dataService))
		{
			return BuyButtonState.DISABLED;
		}
		if (currencyType == CurrencyType.NONE)
		{
			return BuyButtonState.DISABLED;
		}
		if (!StoreManager.Get().IsOpen())
		{
			return BuyButtonState.DISABLED;
		}
		if (!ServiceManager.TryGet<CurrencyManager>(out var currencyManager))
		{
			return BuyButtonState.DISABLED;
		}
		if (currencyType == CurrencyType.REAL_MONEY && !StoreManager.Get().IsBattlePayFeatureEnabled())
		{
			return BuyButtonState.DISABLED_FEATURE;
		}
		if (currencyType == CurrencyType.GOLD && !StoreManager.Get().IsBuyWithGoldFeatureEnabled())
		{
			return BuyButtonState.DISABLED_FEATURE;
		}
		if (ShopUtils.IsCurrencyVirtual(currencyType) && !ShopUtils.IsVirtualCurrencyEnabled())
		{
			return BuyButtonState.DISABLED_FEATURE;
		}
		if (!ShopUtils.TryGetPriceFromBundleOrTransaction(bundle, transaction, currencyType, out var price))
		{
			return BuyButtonState.DISABLED_NO_TOOLTIP;
		}
		if (StoreManager.Get().IsProductAlreadyOwned(bundle))
		{
			return BuyButtonState.DISABLED_OWNED;
		}
		if (!bundle.IsBundleAvailableNow(StoreManager.Get()))
		{
			return BuyButtonState.DISABLED_NO_TOOLTIP;
		}
		if (currencyType != CurrencyType.REAL_MONEY && currencyManager.GetBalance(currencyType) < price)
		{
			switch (currencyType)
			{
			case CurrencyType.GOLD:
				return BuyButtonState.DISABLED_NOT_ENOUGH_GOLD;
			case CurrencyType.CN_RUNESTONES:
			case CurrencyType.ROW_RUNESTONES:
			{
				ProductDataModel currencyProduct2 = dataService.VirtualCurrencyProductItem;
				if (currencyProduct2 != null && currencyProduct2.Availability == ProductAvailability.CAN_PURCHASE)
				{
					return BuyButtonState.ENABLED;
				}
				return BuyButtonState.DISABLED_NOT_ENOUGH_VC;
			}
			case CurrencyType.CN_ARCANE_ORBS:
			{
				ProductDataModel currencyProduct = dataService.BoosterCurrencyProductItem;
				if (currencyProduct != null && currencyProduct.Availability == ProductAvailability.CAN_PURCHASE)
				{
					return BuyButtonState.ENABLED;
				}
				return BuyButtonState.DISABLED_NOT_ENOUGH_BC;
			}
			default:
				return BuyButtonState.DISABLED;
			}
		}
		return BuyButtonState.ENABLED;
	}

	protected void BindProductDataModel(ProductInfo bundle)
	{
		WidgetTemplate widget = GameObjectUtils.GetComponentOnSelfOrParent<WidgetTemplate>(base.transform);
		if (widget != null && ServiceManager.TryGet<IProductDataService>(out var dataService))
		{
			ProductDataModel product = null;
			if (bundle != null && bundle.Id.IsValid())
			{
				product = dataService.GetProductDataModel(bundle.Id.Value);
			}
			product = product ?? ProductFactory.CreateEmptyProductDataModel();
			widget.BindDataModel(product, base.gameObject, propagateToChildren: true, overrideChildren: true);
		}
	}

	private void OnShopProductPageOpened(ProductPage page)
	{
		m_restoreWhenShopHides = true;
		Hide();
	}

	private void OnShopProductPageClosed(ProductPage page)
	{
		if (this != null && m_restoreWhenShopHides)
		{
			m_restoreWhenShopHides = false;
			StartCoroutine(ShowWhenReady(isTotallyFake: false));
		}
	}

	public virtual IEnumerable<CurrencyType> GetVisibleCurrencies()
	{
		return GetVisibleCurrenciesImpl();
	}

	protected HashSet<CurrencyType> GetVisibleCurrenciesImpl()
	{
		HashSet<CurrencyType> currencies = new HashSet<CurrencyType> { CurrencyType.GOLD };
		if (ShopUtils.IsVirtualCurrencyEnabled() && ShopUtils.TryGetMainVirtualCurrencyType(out var vcType))
		{
			currencies.Add(vcType);
		}
		return currencies;
	}
}
