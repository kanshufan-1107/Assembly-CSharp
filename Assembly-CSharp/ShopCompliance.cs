using Blizzard.GameService.SDK.Client.Integration;
using Blizzard.T5.Services;
using Hearthstone.DataModels;
using Hearthstone.Store;
using Hearthstone.UI;
using UnityEngine;

public class ShopCompliance : MonoBehaviour
{
	private Widget m_widget;

	private ProductPage m_productPage;

	private ShopLicenseTermsDataModel m_licenseTermsDataModel;

	private const string BlizzardTerms = "BLIZZARD_TERMS_CLICKED";

	private const string AppleTerms = "APPLE_TERMS_CLICKED";

	private const string GoogleTerms = "GOOGLE_TERMS_CLICKED";

	private void Awake()
	{
		m_widget = GetComponent<Widget>();
		m_productPage = GetComponentInParent<ProductPage>();
		RegisterEventListeners();
		InitDataModel();
	}

	private void OnDestroy()
	{
		UnregisterEventListeners();
	}

	private void RegisterEventListeners()
	{
		if (m_widget != null)
		{
			m_widget.RegisterEventListener(OnWidgetEvent);
		}
		if ((bool)m_productPage)
		{
			m_productPage.OnProductVariantSet += OnProductChanged;
		}
	}

	private void UnregisterEventListeners()
	{
		if (m_widget != null)
		{
			m_widget.RemoveEventListener(OnWidgetEvent);
		}
		if ((bool)m_productPage)
		{
			m_productPage.OnProductVariantSet -= OnProductChanged;
		}
	}

	private void InitDataModel()
	{
		if (!(m_widget == null) && !(m_productPage == null))
		{
			m_licenseTermsDataModel = new ShopLicenseTermsDataModel
			{
				Platform = PlatformSettings.RuntimeOS
			};
			SetCompliancePanelVisibility(m_productPage.Product);
			m_widget.BindDataModel(m_licenseTermsDataModel);
		}
	}

	private void SetCompliancePanelVisibility(ProductDataModel product)
	{
		if (m_licenseTermsDataModel != null && product != null)
		{
			IProductDataService dataService;
			ProductInfo productInfo;
			if (product.PmtId == 0L)
			{
				m_licenseTermsDataModel.Visible = false;
			}
			else if (ServiceManager.TryGet<IProductDataService>(out dataService) && dataService.TryGetProduct(product.PmtId, out productInfo))
			{
				m_licenseTermsDataModel.Visible = BattleNet.GetCurrentRegion() != BnetRegion.REGION_CN && PlatformSettings.IsMobile() && !productInfo.IsFree() && !productInfo.IsGoldOnly();
			}
		}
	}

	private void OpenUrl(string url)
	{
		if (string.IsNullOrEmpty(url))
		{
			Log.Store.Print("[ShopCompliance::OpenUrl] URL is null/empty.");
			return;
		}
		Log.Store.Print("[ShopCompliance::OpenUrl] Opening url: " + url);
		Application.OpenURL(url);
	}

	private void OnWidgetEvent(string eventName)
	{
		Log.Store.Print("[ShopCompliance::OnWidgetEvent] Received " + eventName + " event.");
		string url = string.Empty;
		switch (eventName)
		{
		case "BLIZZARD_TERMS_CLICKED":
			url = ExternalUrlService.Get().GetBlizzardLicenseTerms();
			break;
		case "APPLE_TERMS_CLICKED":
			url = ExternalUrlService.Get().GetAppleLicenseTerms();
			break;
		case "GOOGLE_TERMS_CLICKED":
			url = ExternalUrlService.Get().GetGoogleLicenseTerms();
			break;
		}
		OpenUrl(url);
	}

	private void OnProductChanged(ProductSelectionDataModel selection)
	{
		if (selection != null)
		{
			SetCompliancePanelVisibility(selection.Variant);
		}
	}
}
