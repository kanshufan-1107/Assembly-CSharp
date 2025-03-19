using System;
using System.Collections;
using Blizzard.T5.Services;
using Hearthstone.DataModels;
using Hearthstone.Store;
using Hearthstone.UI;
using Hearthstone.UI.Core;
using UnityEngine;

public class ProductPageController : MonoBehaviour
{
	private struct PageReopenInfo
	{
		private ProductDataModel m_product;

		private ProductDataModel m_variant;

		private CurrencyType m_relatedCurrencyType;

		public bool IsValid
		{
			get
			{
				if (m_relatedCurrencyType == CurrencyType.NONE)
				{
					return m_product != null;
				}
				return true;
			}
		}

		public string ProductId
		{
			get
			{
				if (IsValid && m_product != null)
				{
					if (m_product.PmtId == 0L)
					{
						return m_variant.PmtId.ToString();
					}
					return m_product.PmtId.ToString();
				}
				return string.Empty;
			}
		}

		public PageReopenInfo(ProductDataModel product, ProductDataModel variant = null)
		{
			m_product = product;
			m_variant = variant;
			m_relatedCurrencyType = CurrencyType.NONE;
		}

		public PageReopenInfo(CurrencyType currencyType)
		{
			m_relatedCurrencyType = currencyType;
			m_product = null;
			m_variant = null;
		}

		public void Clear()
		{
			m_relatedCurrencyType = CurrencyType.NONE;
			m_product = null;
			m_variant = null;
		}

		public void ReopenPage()
		{
			if (!IsValid)
			{
				return;
			}
			Shop shop = Shop.Get();
			if (!(shop != null))
			{
				return;
			}
			if (m_relatedCurrencyType != 0)
			{
				switch (m_relatedCurrencyType)
				{
				case CurrencyType.CN_RUNESTONES:
				case CurrencyType.ROW_RUNESTONES:
					shop.ProductPageController.OpenVirtualCurrencyPurchase();
					break;
				case CurrencyType.CN_ARCANE_ORBS:
					shop.ProductPageController.OpenBoosterCurrencyPurchase();
					break;
				case CurrencyType.REAL_MONEY:
					break;
				}
			}
			else if (m_product != null)
			{
				shop.ProductPageController.OpenProductPage(m_product, m_variant);
			}
		}
	}

	[SerializeField]
	protected AsyncReference m_vcPageRef;

	[SerializeField]
	protected AsyncReference m_bcPageRef;

	[SerializeField]
	protected AsyncReference m_productPageContainerRef;

	[SerializeField]
	protected AsyncReference m_quantityPromptRef;

	[SerializeField]
	protected AsyncReference m_cosmeticPreviewSceneLoaderRef;

	private VirtualCurrencyPurchasePage m_vcPage;

	private CurrencyConversionPage m_bcPage;

	private ProductPageContainer m_productPageContainer;

	private StoreQuantityPrompt m_quantityPrompt;

	private CosmeticPreviewSceneLoader m_cosmeticPreviewSceneLoader;

	private PageReopenInfo m_pageReopenInfo;

	private Coroutine m_pageReopenCoroutine;

	public Action<ProductPage> OnProductPageOpening;

	public Action<ProductPage> OnProductPageOpened;

	public Action<ProductPage> OnProductPageClosed;

	private bool m_resetRedirectedProductId;

	public ProductPage CurrentProductPage { get; private set; }

	public string RedirectedProductId { get; private set; }

	public bool IsOpen
	{
		get
		{
			if ((!(m_bcPage != null) || !m_bcPage.IsOpen) && (!(m_vcPage != null) || !m_vcPage.IsOpen))
			{
				if (m_productPageContainer != null)
				{
					return m_productPageContainer.IsOpen;
				}
				return false;
			}
			return true;
		}
	}

	public bool IsOpening { get; private set; }

	public bool IsReopening => m_pageReopenCoroutine != null;

	public bool IsVCPage(ProductPage page)
	{
		if (m_vcPage != null)
		{
			return page == m_vcPage;
		}
		return false;
	}

	private void Awake()
	{
		m_vcPageRef.RegisterReadyListener(delegate(VirtualCurrencyPurchasePage page)
		{
			if (page == null)
			{
				Log.Store.PrintError("Virtual currency page is null");
			}
			else
			{
				m_vcPage = page;
				m_vcPage.OnOpened += OnPageOpened;
				m_vcPage.OnClosed += OnPageClosed;
				VirtualCurrencyPurchasePage vcPage = m_vcPage;
				vcPage.OnVCPurchaseSuccessful = (EventHandler<VirtualCurrencyPurchasePage.VirtualCurrencyPurchaseArgs>)Delegate.Combine(vcPage.OnVCPurchaseSuccessful, new EventHandler<VirtualCurrencyPurchasePage.VirtualCurrencyPurchaseArgs>(OnVCPurchaseSuccessful));
			}
		});
		m_bcPageRef.RegisterReadyListener(delegate(CurrencyConversionPage page)
		{
			if (page == null)
			{
				Log.Store.PrintError("Virtual currency page is null");
			}
			else
			{
				m_bcPage = page;
				m_bcPage.OnOpened += OnPageOpened;
				m_bcPage.OnClosed += OnPageClosed;
			}
		});
		m_productPageContainerRef.RegisterReadyListener(delegate(ProductPageContainer page)
		{
			m_productPageContainer = page;
			m_productPageContainer.OnOpened += OnPageOpened;
			m_productPageContainer.OnClosed += OnPageClosed;
		});
		m_quantityPromptRef.RegisterReadyListener(delegate(StoreQuantityPrompt page)
		{
			m_quantityPrompt = page;
		});
		if (NetCache.Get().IsNetObjectAvailable<NetCache.NetCacheFeatures>())
		{
			OnNetCacheReady();
		}
		else
		{
			NetCache.Get().RegisterUpdatedListener(typeof(NetCache.NetCacheFeatures), OnNetCacheReady);
		}
	}

	private void OnDestroy()
	{
		if (m_vcPage != null)
		{
			VirtualCurrencyPurchasePage vcPage = m_vcPage;
			vcPage.OnVCPurchaseSuccessful = (EventHandler<VirtualCurrencyPurchasePage.VirtualCurrencyPurchaseArgs>)Delegate.Remove(vcPage.OnVCPurchaseSuccessful, new EventHandler<VirtualCurrencyPurchasePage.VirtualCurrencyPurchaseArgs>(OnVCPurchaseSuccessful));
		}
	}

	public void HandleShopOpen()
	{
		if (m_cosmeticPreviewSceneLoader != null && m_cosmeticPreviewSceneLoader.CosmeticsRenderingEnabled())
		{
			m_cosmeticPreviewSceneLoader.LoadScene();
		}
	}

	public void HandleShopClosed()
	{
		if (m_productPageContainer != null)
		{
			m_productPageContainer.Close();
		}
		if (m_vcPage != null && m_vcPage.IsOpen)
		{
			m_vcPage.Close();
		}
		if (m_bcPage != null && m_bcPage.IsOpen)
		{
			m_bcPage.Close();
		}
		if (m_quantityPrompt != null && m_quantityPrompt.IsShown())
		{
			m_quantityPrompt.Cancel();
		}
		if (m_cosmeticPreviewSceneLoader != null && m_cosmeticPreviewSceneLoader.CosmeticsRenderingEnabled())
		{
			m_cosmeticPreviewSceneLoader.UnloadScene();
		}
	}

	public void OpenProductPage(ProductDataModel product, ProductDataModel variant = null)
	{
		if (product == null || product == ProductFactory.CreateEmptyProductDataModel())
		{
			Log.Store.PrintError("Cannot open Product page - Shop cannot open null or empty product");
			return;
		}
		IsOpening = true;
		OnProductPageOpening?.Invoke(null);
		if (m_productPageContainer != null)
		{
			m_productPageContainer.InitializeTempInstances();
			m_productPageContainer.Open(product, variant);
		}
		if (ServiceManager.TryGet<IProductDataService>(out var productService))
		{
			productService.MarkProductAsSeen(product);
		}
		OnProductPageOpened?.Invoke(m_vcPage);
		if (NetCache.Get() == null || NetCache.Get().GetNetObject<NetCache.NetCacheFeatures>() == null || NetCache.Get().GetNetObject<NetCache.NetCacheFeatures>().Collection == null)
		{
			TelemetryManager.Client()?.SendCosmeticsRenderingFallback("Shop::NetCache Failed");
		}
		if (product.Items[0]?.BGBoardSkin != null && string.IsNullOrEmpty(product.Items[0]?.BGBoardSkin.DetailsRenderConfig))
		{
			TelemetryManager.Client()?.SendCosmeticsRenderingFallback($"Shop::Boardskin[{product.Items[0].BGBoardSkin.BoardDbiId}]");
		}
		else if (product.Items[0]?.BGFinisher != null && string.IsNullOrEmpty(product.Items[0].BGFinisher.DetailsRenderConfig))
		{
			TelemetryManager.Client()?.SendCosmeticsRenderingFallback($"Shop::Finisher[{product.Items[0].BGFinisher.FinisherDbiId}]");
		}
	}

	public void OpenVirtualCurrencyPurchase(float desiredAmount = 0f, bool rememberLastPage = false)
	{
		ProductDataModel variant = null;
		if (ShopUtils.TryGetMainVirtualCurrencyType(out var currencyType))
		{
			variant = ShopUtils.FindCurrencyProduct(currencyType, desiredAmount);
		}
		OpenVirtualCurrencyPurchase(variant, rememberLastPage);
	}

	public void OpenVirtualCurrencyPurchase(ProductDataModel vcVariant, bool rememberLastPage = false)
	{
		if (!ShopUtils.IsVirtualCurrencyEnabled())
		{
			Log.Store.PrintError("Cannot open VC page - VC is not enabled.");
			return;
		}
		if (!ShopUtils.TryGetMainVirtualCurrencyType(out var _))
		{
			Log.Store.PrintError("Cannot open VC page - VC is not used in this region.");
			return;
		}
		if (m_vcPage == null)
		{
			Log.Store.PrintError("Cannot open VC page - VC page is null.");
			return;
		}
		if (m_vcPage.IsOpen || m_vcPage.IsAnimating)
		{
			Log.Store.PrintDebug("Cannot open VC page - VC page is still animating.");
			return;
		}
		if (vcVariant == null)
		{
			Log.Store.PrintError("Cannot open VC page - Null variant provided.");
			return;
		}
		if (!ServiceManager.TryGet<IProductDataService>(out var productService))
		{
			Log.Store.PrintError("Cannot show VC Page - No product data service");
			return;
		}
		ProductDataModel vcRootProduct = productService.VirtualCurrencyProductItem;
		if (vcRootProduct == null || vcRootProduct == ProductFactory.CreateEmptyProductDataModel())
		{
			Log.Store.PrintError("Cannot open VC page - No valid VC products received.");
			return;
		}
		if (vcRootProduct.Availability != ProductAvailability.CAN_PURCHASE)
		{
			Log.Store.PrintError("Cannot open VC page - VC product is not available for purchase.");
			return;
		}
		if (!ServiceManager.TryGet<CurrencyManager>(out var _))
		{
			Log.Store.PrintError("Cannot show VC Page - No currency manager available");
			return;
		}
		if (!IsOpen && StoreManager.Get() != null && StoreManager.Get().IsPromptShowing)
		{
			Log.Store.PrintDebug("Cannot open VC page - StoreManager is showing popup.");
			return;
		}
		CleanUpPagesForCurrencyPage(rememberLastPage);
		CurrentProductPage = m_vcPage;
		IsOpening = true;
		OnProductPageOpening?.Invoke(m_vcPage);
		UIContext.GetRoot().ShowPopup(m_vcPage.gameObject, UIContext.BlurType.Layered, UIContext.ProjectionType.Perspective);
		m_vcPage.OpenToSKU(vcVariant, rememberLastPage);
	}

	public void OpenBoosterCurrencyPurchase(float desiredPurchaseAmount = 0f, bool rememberLastPage = false)
	{
		if (!ShopUtils.IsVirtualCurrencyEnabled())
		{
			Log.Store.PrintError("Cannot open BC page - BC is not enabled.");
			return;
		}
		if (!ShopUtils.TryGetBoosterVirtualCurrencyType(out var bcType))
		{
			Log.Store.PrintError("Cannot open BC page - BC is not used in this region.");
			return;
		}
		if (m_bcPage == null)
		{
			Log.Store.PrintError("Cannot open BC page - BC page is null.");
			return;
		}
		if (m_bcPage.IsOpen || m_bcPage.IsAnimating)
		{
			Log.Store.PrintDebug("Cannot open BC page - BC page is still animating.");
			return;
		}
		if (!ServiceManager.TryGet<IProductDataService>(out var productService))
		{
			Log.Store.PrintError("Cannot show BC Page - No product data service");
			return;
		}
		ProductDataModel bcRootProduct = productService.VirtualCurrencyProductItem;
		if (bcRootProduct == null || bcRootProduct == ProductFactory.CreateEmptyProductDataModel())
		{
			Log.Store.PrintError("Cannot Open BC Page - No valid BC product received.");
			return;
		}
		if (bcRootProduct.Availability != ProductAvailability.CAN_PURCHASE)
		{
			Log.Store.PrintError("Cannot Open BC Page - BC not available for purchase");
			return;
		}
		if (!ServiceManager.TryGet<CurrencyManager>(out var currencyManager))
		{
			Log.Store.PrintError("Cannot Open BC Page - No currency manager available");
			return;
		}
		if (currencyManager.DoesCurrencyNeedRefresh(bcType))
		{
			if (DialogManager.Get() != null)
			{
				AlertPopup.PopupInfo info = new AlertPopup.PopupInfo
				{
					m_text = (currencyManager.DoesCurrencyHaveError(bcType) ? GameStrings.Format("GLUE_STORE_FAIL_CURRENCY_BALANCE") : GameStrings.Format("GLUE_STORE_UPDATING_CURRENCY_BALANCE")),
					m_showAlertIcon = true,
					m_responseDisplay = AlertPopup.ResponseDisplay.OK
				};
				DialogManager.Get().ShowPopup(info);
			}
			return;
		}
		if (!IsOpen && StoreManager.Get() != null && StoreManager.Get().IsPromptShowing)
		{
			Log.Store.PrintDebug("Cannot open BC page - StoreManager is showing popup.");
			return;
		}
		CleanUpPagesForCurrencyPage(rememberLastPage);
		if (bcType == CurrencyType.CN_ARCANE_ORBS)
		{
			long balance = currencyManager.GetBalance(CurrencyType.CN_ARCANE_ORBS);
			float orbsPerSale = ShopUtils.GetAmountOfCurrencyInProduct(bcRootProduct, CurrencyType.CN_ARCANE_ORBS);
			if ((float)balance + orbsPerSale > 9999f)
			{
				if (DialogManager.Get() != null)
				{
					AlertPopup.PopupInfo popupInfo = new AlertPopup.PopupInfo();
					popupInfo.m_headerText = GameStrings.Format("GLUE_ARCANE_ORBS_CAP_HEADER");
					popupInfo.m_text = GameStrings.Format("GLUE_ARCANE_ORBS_CAP_BODY", 9999);
					popupInfo.m_showAlertIcon = true;
					popupInfo.m_responseDisplay = AlertPopup.ResponseDisplay.OK;
					AlertPopup.PopupInfo info2 = popupInfo;
					DialogManager.Get().ShowPopup(info2);
				}
				return;
			}
		}
		CurrentProductPage = m_bcPage;
		IsOpening = true;
		OnProductPageOpening?.Invoke(m_bcPage);
		UIContext.GetRoot().ShowPopup(m_bcPage.gameObject, UIContext.BlurType.Layered, UIContext.ProjectionType.Perspective);
		m_bcPage.OpenToSKU(desiredPurchaseAmount);
	}

	public void OpenQuantityPrompt(int maxQuantity, Action<int> onQuantitySet, Action onCancelSet = null)
	{
		if (!(m_quantityPrompt == null))
		{
			StoreManager.Get().BlockStoreInterface();
			m_quantityPrompt.Show(maxQuantity, delegate(int quantity)
			{
				StoreManager.Get().UnblockStoreInterface();
				onQuantitySet?.Invoke(quantity);
			}, delegate
			{
				StoreManager.Get().UnblockStoreInterface();
				onCancelSet?.Invoke();
			});
		}
	}

	public void CloseCurrentPage(bool reopenLater)
	{
		ProductPage page = CurrentProductPage;
		if (page == null)
		{
			return;
		}
		if (m_quantityPrompt != null && m_quantityPrompt.IsShown())
		{
			m_quantityPrompt.Cancel();
		}
		if (page == m_vcPage || page == m_bcPage)
		{
			CurrencyType currentCurrencyType = CurrencyType.NONE;
			if (page == m_vcPage)
			{
				ShopUtils.TryGetMainVirtualCurrencyType(out currentCurrencyType);
			}
			else if (page == m_bcPage)
			{
				ShopUtils.TryGetBoosterVirtualCurrencyType(out currentCurrencyType);
			}
			PageReopenInfo reopenInfo = new PageReopenInfo(currentCurrencyType);
			page.Close();
			if (page.GetComponent<IPopupRoot>() != null)
			{
				UIContext.GetRoot().DismissPopup(page.gameObject);
			}
			if (reopenLater)
			{
				m_pageReopenInfo = reopenInfo;
				RedirectedProductId = m_pageReopenInfo.ProductId;
			}
		}
		else if (page == m_productPageContainer.GetCurrentProductPage())
		{
			PageReopenInfo reopenInfo2 = new PageReopenInfo(m_productPageContainer.Product, m_productPageContainer.Variant);
			m_productPageContainer.Close();
			if (reopenLater)
			{
				m_pageReopenInfo = reopenInfo2;
				RedirectedProductId = m_pageReopenInfo.ProductId;
			}
		}
	}

	public bool TempIntancesInitialized()
	{
		if (m_productPageContainer != null)
		{
			return m_productPageContainer.TempInstancesInitialized();
		}
		return false;
	}

	private void OnPageOpened(object sender, EventArgs args)
	{
		ProductPage page;
		if (sender is ProductPage senderPage)
		{
			page = senderPage;
		}
		else
		{
			if (!(sender is ProductPageContainer productPageContainer))
			{
				Log.Store.PrintError("Unknown sender type on page opened");
				return;
			}
			page = productPageContainer.GetCurrentProductPage();
		}
		if (page == null)
		{
			Log.Store.PrintError("Unknown page from sender on page opened");
			return;
		}
		CurrentProductPage = page;
		IsOpening = false;
		OnProductPageOpened?.Invoke(page);
		if (BnetBar.Get() != null)
		{
			BnetBar.Get().RefreshCurrency();
		}
		if (m_vcPage != page && m_resetRedirectedProductId)
		{
			RedirectedProductId = string.Empty;
		}
		m_resetRedirectedProductId = true;
	}

	private void OnPageClosed(object sender, EventArgs args)
	{
		if (!(CurrentProductPage == null))
		{
			OnProductPageClosed?.Invoke(CurrentProductPage);
			CurrentProductPage = null;
			if (ServiceManager.TryGet<PurchaseManager>(out var purchaseManager))
			{
				purchaseManager.CancelPendingAutoPurchases();
			}
			if (ServiceManager.TryGet<CurrencyManager>(out var currencyManager))
			{
				currencyManager.MarkVirtualCurrencyDirty();
			}
			if (BnetBar.Get() != null)
			{
				BnetBar.Get().RefreshCurrency();
			}
			ReopenClosedPage();
		}
	}

	private void ReopenClosedPage()
	{
		if (m_pageReopenInfo.IsValid)
		{
			if (m_pageReopenCoroutine != null)
			{
				StopCoroutine(m_pageReopenCoroutine);
				m_pageReopenCoroutine = null;
			}
			m_pageReopenCoroutine = StartCoroutine(ReopenClosedPageCoroutine(m_pageReopenInfo));
			m_pageReopenInfo.Clear();
		}
	}

	private IEnumerator ReopenClosedPageCoroutine(PageReopenInfo pageReopeInfo)
	{
		try
		{
			yield return new WaitForEndOfFrame();
			if (pageReopeInfo.IsValid)
			{
				pageReopeInfo.ReopenPage();
			}
		}
		finally
		{
			m_pageReopenCoroutine = null;
		}
	}

	private void CleanUpPagesForCurrencyPage(bool rememberLastPage)
	{
		if (!rememberLastPage)
		{
			m_pageReopenInfo.Clear();
		}
		CloseCurrentPage(rememberLastPage);
	}

	private void OnNetCacheReady()
	{
		NetCache.Get().UnregisterNetCacheHandler(OnNetCacheReady);
		if (NetCache.Get().GetNetObject<NetCache.NetCacheFeatures>().Collection.CosmeticsRenderingEnabled)
		{
			m_cosmeticPreviewSceneLoaderRef.RegisterReadyListener(delegate(CosmeticPreviewSceneLoader page)
			{
				m_cosmeticPreviewSceneLoader = page;
			});
		}
	}

	private void OnVCPurchaseSuccessful(object sender, VirtualCurrencyPurchasePage.VirtualCurrencyPurchaseArgs e)
	{
		RedirectedProductId = e.ProductId;
		m_resetRedirectedProductId = false;
	}
}
