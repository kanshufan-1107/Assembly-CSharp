using System.Collections.Generic;
using Blizzard.Commerce;
using Blizzard.T5.Services;
using Hearthstone.Store;
using Hearthstone.UI;
using PegasusUtil;
using UnityEngine;

[CustomEditClass]
public class AdventureStore : Store
{
	[CustomEditField(Sections = "UI")]
	public UIBButton m_BuyDungeonButton;

	[CustomEditField(Sections = "UI")]
	public UberText m_Headline;

	[CustomEditField(Sections = "UI")]
	public UberText m_DetailsText1;

	[CustomEditField(Sections = "UI")]
	public UberText m_DetailsText2;

	[CustomEditField(Sections = "UI")]
	public GameObject m_BuyWithMoneyButtonOpaqueCover;

	[CustomEditField(Sections = "UI")]
	public GameObject m_BuyWithGoldButtonOpaqueCover;

	[CustomEditField(Sections = "UI")]
	public GameObject m_BuyDungeonButtonOpaqueCover;

	[CustomEditField(Sections = "UI")]
	public UIBButton m_BackButton;

	[CustomEditField(Sections = "UI")]
	public WidgetInstance m_FullAdventureBundleCurrencyIcon;

	public const bool REQUIRE_REAL_MONEY_BUNDLE_OPTION = true;

	private bool m_animating;

	private ProductInfo m_bundle;

	private ProductInfo m_fullAdventureBundle;

	protected override void Start()
	{
		base.Start();
		if (m_BuyDungeonButton != null)
		{
			m_BuyDungeonButton.AddEventListener(UIEventType.RELEASE, OnBuyDungeonButtonReleased);
		}
		if (m_offClicker != null)
		{
			m_offClicker.AddEventListener(UIEventType.RELEASE, OnBackButtonReleased);
		}
		if (m_BackButton != null)
		{
			m_BackButton.AddEventListener(UIEventType.RELEASE, OnBackButtonReleased);
		}
	}

	public void SetAdventureProduct(ProductType productItemType, int productData, int numItemsRequired, ProductId productId)
	{
		if (productId.IsValid() && ServiceManager.TryGet<IProductDataService>(out var dataService))
		{
			m_bundle = null;
			if (!dataService.TryGetProduct(productId, out var bundle))
			{
				Log.Store.PrintWarning("AdventureStore.SetAdventureProduct(): could not find bundle with PMT Product ID {0}", productId);
			}
			else if (!bundle.DoesBundleContainProduct(productItemType, productData, numItemsRequired))
			{
				Log.Store.PrintWarning("AdventureStore.SetAdventureProduct(): bundle with PMT product ID {0} does not meet the expected criteria! productItemType: {1}  productData: {2}  numItemsRequired: {3}", productId, productItemType, productData, numItemsRequired);
			}
			else if (!bundle.IsBundleAvailableNow(StoreManager.Get()))
			{
				Log.Store.PrintWarning("AdventureStore.SetAdventureProduct(): bundle with PMT product ID {0} is not available now!", productId);
			}
			else if (StoreManager.Get().IsProductAlreadyOwned(bundle))
			{
				Log.Store.PrintWarning("AdventureStore.SetAdventureProduct(): bundle with PMT product ID {0} contains already owned content!", productId);
			}
			else
			{
				m_bundle = bundle;
			}
		}
		else
		{
			List<ProductInfo> bundleOptions = new List<ProductInfo>(StoreManager.Get().GetAvailableBundlesForProduct(productItemType, numItemsRequired > 1, productData, numItemsRequired));
			if (bundleOptions.Count == 1)
			{
				m_bundle = bundleOptions[0];
			}
			else
			{
				Debug.LogWarningFormat("AdventureStore.SetAdventureProduct(): expected to find 1 available bundle for productItemType {0} productData {1} numItemsRequired {2}, found {3}", productItemType, productData, numItemsRequired, bundleOptions.Count);
				m_bundle = null;
			}
		}
		string productName = StoreManager.Get().GetProductName(m_bundle);
		if (m_Headline != null)
		{
			m_Headline.Text = productName;
		}
		string productStringKey = string.Empty;
		switch (productItemType)
		{
		case ProductType.PRODUCT_TYPE_NAXX:
			productStringKey = "NAXX";
			break;
		case ProductType.PRODUCT_TYPE_BRM:
			productStringKey = "BRM";
			break;
		case ProductType.PRODUCT_TYPE_LOE:
			productStringKey = "LOE";
			break;
		case ProductType.PRODUCT_TYPE_WING:
			productStringKey = GameUtils.GetAdventureProductStringKey(productData);
			break;
		}
		string shortProductName = GameDbf.Wing.GetRecord(productData).NameShort;
		string adjustedProductName = (string.IsNullOrEmpty(shortProductName) ? productName : shortProductName);
		if (m_DetailsText1 != null)
		{
			string detailsKey1 = $"GLUE_STORE_PRODUCT_DETAILS_{productStringKey}_PART_1";
			m_DetailsText1.Text = GameStrings.Format(detailsKey1, adjustedProductName);
		}
		if (m_DetailsText2 != null)
		{
			string detailsKey2 = $"GLUE_STORE_PRODUCT_DETAILS_{productStringKey}_PART_2";
			m_DetailsText2.Text = GameStrings.Format(detailsKey2);
		}
		AdventureDbId adventureId = GameUtils.GetAdventureIdByWingId(productData);
		StoreManager.Get().GetAvailableAdventureBundle(adventureId, requireNonGoldOption: true, out m_fullAdventureBundle);
		if (m_fullAdventureBundle == null)
		{
			Log.Store.PrintWarning("Full adventure bundle not available.");
		}
		BindProductDataModel(m_bundle);
	}

	public override void Hide()
	{
		m_shown = false;
		Navigation.RemoveHandler(OnNavigateBack);
		StoreManager.Get().RemoveAuthorizationExitListener(OnAuthExit);
		StoreManager.Get().RemoveSuccessfulPurchaseAckListener(OnSuccessfulPurchase);
		EnableFullScreenEffects(enable: false);
		DoHideAnimation();
	}

	public override void OnMoneySpent()
	{
		RefreshBuyButtonStates(m_bundle, null);
		RefreshBuyFullAdventureButton();
	}

	public override void OnGoldBalanceChanged(NetCache.NetCacheGoldBalance balance)
	{
		RefreshBuyButtonStates(m_bundle, null);
	}

	public override void OnCurrencyBalanceChanged(CurrencyBalanceChangedEventArgs args)
	{
		if (ShopUtils.IsCurrencyVirtual(args.Currency))
		{
			RefreshBuyButtonStates(m_bundle, null);
			RefreshBuyFullAdventureButton();
		}
	}

	public override void Close()
	{
		Hide();
		FireExitEvent(authorizationBackButtonPressed: false);
	}

	protected override void ShowImpl(bool isTotallyFake)
	{
		if (!m_shown)
		{
			m_shown = true;
			Navigation.Push(OnNavigateBack);
			StoreManager.Get().RegisterAuthorizationExitListener(OnAuthExit);
			StoreManager.Get().RegisterSuccessfulPurchaseAckListener(OnSuccessfulPurchase);
			EnableFullScreenEffects(enable: true);
			SetUpBuyButtons();
			m_animating = true;
			DoShowAnimation(delegate
			{
				m_animating = false;
				FireOpenedEvent();
			});
		}
	}

	protected override void BuyWithGold(UIEvent e)
	{
		if (m_animating)
		{
			Log.Store.Print("AdventureStore.BuyWithGold failed: still animating");
		}
		else if (m_bundle == null)
		{
			Log.Store.PrintError("AdventureStore.BuyWithGold failed: Bundle is null");
		}
		else
		{
			FireBuyWithGoldEventGTAPP(m_bundle, 1);
		}
	}

	protected override void BuyWithMoney(UIEvent e)
	{
		if (m_animating)
		{
			Log.Store.Print("AdventureStore.BuyWithMoney failed: still animating");
		}
		else if (m_bundle == null)
		{
			Log.Store.PrintError("AdventureStore.BuyWithMoney failed: Bundle is null");
		}
		else
		{
			FireBuyWithMoneyEvent(m_bundle, 1);
		}
	}

	protected override void BuyWithVirtualCurrency(UIEvent e)
	{
		if (m_animating)
		{
			Log.Store.Print("AdventureStore.BuyWithVirtualCurrency failed: still animating");
		}
		else if (m_bundle == null)
		{
			Log.Store.PrintError("AdventureStore.BuyWithVirtualCurrency failed: Bundle is null");
		}
		else
		{
			FireBuyWithVirtualCurrencyEvent(m_bundle, m_bundle.GetFirstVirtualCurrencyPriceType());
		}
	}

	protected override void RefreshBuyButtonStates(ProductInfo bundle, NoGTAPPTransactionData transaction)
	{
		base.RefreshBuyButtonStates(bundle, transaction);
		if (m_BuyWithMoneyButtonOpaqueCover != null)
		{
			bool num = m_buyWithMoneyButton != null && m_buyWithMoneyButton.gameObject.activeInHierarchy;
			bool isVCButtonActive = m_buyWithVCButton != null && m_buyWithVCButton.gameObject.activeInHierarchy;
			bool activateCover = false;
			if (num && GetMoneyButtonState() == BuyButtonState.DISABLED_NO_TOOLTIP)
			{
				activateCover = true;
			}
			if (isVCButtonActive && GetVCButtonState() == BuyButtonState.DISABLED_NO_TOOLTIP)
			{
				activateCover = true;
			}
			m_BuyWithMoneyButtonOpaqueCover.SetActive(activateCover);
		}
		if (m_BuyWithGoldButtonOpaqueCover != null)
		{
			bool activateCover2 = GetGoldButtonState() == BuyButtonState.DISABLED_NO_TOOLTIP;
			m_BuyWithGoldButtonOpaqueCover.SetActive(activateCover2);
		}
		RefreshBuyFullAdventureButton();
	}

	private void OnAuthExit()
	{
		BlockInterface(blocked: false);
		LayerUtils.SetLayer(base.gameObject, GameLayer.Default);
		EnableFullScreenEffects(enable: false);
		StoreManager.Get().RemoveAuthorizationExitListener(OnAuthExit);
		FireExitEvent(authorizationBackButtonPressed: true);
		Hide();
	}

	private void OnSuccessfulPurchase(ProductInfo bundle, PaymentMethod method)
	{
		BlockInterface(blocked: false);
		EnableFullScreenEffects(enable: false);
		FireExitEvent(authorizationBackButtonPressed: true);
		Hide();
	}

	private void SetUpBuyButtons()
	{
		SetUpBuyWithGoldButton();
		SetUpBuyWithMoneyButton();
		SetUpBuyFullAdventureButton();
		RefreshBuyButtonStates(m_bundle, null);
	}

	private void SetUpBuyWithGoldButton()
	{
		string buttonText = string.Empty;
		long amount;
		if (m_bundle == null)
		{
			Debug.LogWarning("AdventureStore.SetUpBuyWithGoldButton(): m_bundle is null");
			buttonText = GameStrings.Get("GLUE_STORE_PRODUCT_PRICE_NA");
		}
		else if (!m_bundle.TryGetVCPrice(CurrencyType.GOLD, out amount))
		{
			Debug.LogWarning("AdventureStore.SetUpBuyWithGoldButton(): " + m_bundle.Title + " does not have gold cost");
			buttonText = GameStrings.Get("GLUE_STORE_PRODUCT_PRICE_NA");
		}
		else
		{
			buttonText = amount.ToString();
		}
		if (m_buyWithGoldButton != null)
		{
			m_buyWithGoldButton.SetText(buttonText);
		}
	}

	private void SetUpBuyWithMoneyButton()
	{
		string buttonText = string.Empty;
		if (m_bundle != null && m_bundle.TryGetRMPriceInfo(out var priceInfo))
		{
			buttonText = priceInfo.CurrentPrice.DisplayPrice;
		}
		else
		{
			Debug.LogWarning("AdventureStore.SetUpBuyWithMoneyButton(): m_bundle is null");
			buttonText = GameStrings.Get("GLUE_STORE_PRODUCT_PRICE_NA");
		}
		m_buyWithMoneyButton.SetText(buttonText);
	}

	private void SetUpBuyFullAdventureButton()
	{
		RefreshBuyFullAdventureButton();
	}

	private void RefreshBuyFullAdventureButton()
	{
		if (m_fullAdventureBundle != null && !StoreManager.Get().CanBuyBundle(m_fullAdventureBundle))
		{
			Log.Store.PrintWarning("CanBuyBundle is false for m_fullAdventureBundle, PMTProductID = {0}", m_fullAdventureBundle.Id.Value);
			m_fullAdventureBundle = null;
		}
		string buttonText = string.Empty;
		bool opaqueCoverActive = false;
		string formattedCostText = null;
		CurrencyType fullAdventureVCPriceType = CurrencyType.NONE;
		if (m_fullAdventureBundle != null)
		{
			fullAdventureVCPriceType = m_fullAdventureBundle.GetFirstVirtualCurrencyPriceType();
			RealPriceInfo priceInfo;
			if (fullAdventureVCPriceType != 0 && m_fullAdventureBundle.TryGetVCPrice(fullAdventureVCPriceType, out var fullAdventureVCCost))
			{
				switch (fullAdventureVCPriceType)
				{
				case CurrencyType.CN_RUNESTONES:
				case CurrencyType.ROW_RUNESTONES:
					formattedCostText = GameStrings.Format("GLUE_SHOP_PRICE_RUNESTONES", fullAdventureVCCost);
					break;
				case CurrencyType.CN_ARCANE_ORBS:
					formattedCostText = GameStrings.Format("GLUE_SHOP_PRICE_ARCANE_ORBS", fullAdventureVCCost);
					break;
				}
			}
			else if (m_fullAdventureBundle.TryGetRMPriceInfo(out priceInfo))
			{
				formattedCostText = priceInfo.CurrentPrice.DisplayPrice;
			}
		}
		if (formattedCostText != null)
		{
			string line1Text = GameStrings.Get("GLUE_STORE_DUNGEON_BUTTON_TEXT");
			string line2Text = GameStrings.Format("GLUE_STORE_DUNGEON_BUTTON_COST_TEXT", m_fullAdventureBundle.Items?.Count, formattedCostText);
			buttonText = $"{line1Text}\n{line2Text}";
		}
		else
		{
			opaqueCoverActive = true;
			buttonText = string.Empty;
		}
		if (m_FullAdventureBundleCurrencyIcon != null)
		{
			m_FullAdventureBundleCurrencyIcon.BindDataModel(m_fullAdventureBundle.GetPriceDataModel(fullAdventureVCPriceType));
		}
		if (m_BuyDungeonButton != null)
		{
			m_BuyDungeonButton.SetText(buttonText);
		}
		if (m_BuyDungeonButtonOpaqueCover != null)
		{
			m_BuyDungeonButtonOpaqueCover.SetActive(opaqueCoverActive);
		}
		if (m_BuyDungeonButton != null)
		{
			m_BuyDungeonButton.SetEnabled(!opaqueCoverActive);
		}
	}

	private void OnBuyDungeonButtonReleased(UIEvent e)
	{
		if (m_animating)
		{
			Log.Store.Print("AdventureStore.OnBuyDungeonButtonReleased failed: still animating");
			return;
		}
		if (m_fullAdventureBundle == null)
		{
			Log.Store.PrintError("AdventureStore.OnBuyDungeonButtonReleased failed: m_fullAdventureBundle is null");
			return;
		}
		CurrencyType vcCurrencyType = m_fullAdventureBundle.GetFirstVirtualCurrencyPriceType();
		if (vcCurrencyType != 0)
		{
			FireBuyWithVirtualCurrencyEvent(m_fullAdventureBundle, vcCurrencyType);
			return;
		}
		if (m_fullAdventureBundle.HasCurrency(CurrencyType.REAL_MONEY))
		{
			FireBuyWithMoneyEvent(m_fullAdventureBundle, 1);
			return;
		}
		Log.Store.PrintError("AdventureStore.OnBuyDungeonButtonReleased failed: no valid price on m_fullAdventureBundle. PMT ID = {0}", m_fullAdventureBundle.Id);
	}

	private void OnBackButtonReleased(UIEvent e)
	{
		if (!ServiceManager.TryGet<HearthstoneCheckout>(out var commerce) || !commerce.IsInProgress)
		{
			Close();
		}
	}

	private bool OnNavigateBack()
	{
		Close();
		return true;
	}
}
