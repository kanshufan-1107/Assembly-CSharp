using System;
using System.Collections.Generic;
using Assets;
using Blizzard.T5.Services;
using Hearthstone;
using Hearthstone.DataModels;
using Hearthstone.Store;
using Hearthstone.UI;
using UnityEngine;

public class BoxProductBanner : MonoBehaviour
{
	private static bool s_hasSeenBannerThisSession;

	private const string ANIMATE_IN_EVENT = "ANIMATE_IN";

	private const string ANIMATE_OUT_EVENT = "ANIMATE_OUT";

	private const string SINGLE_IMAGE_EVENT = "SINGLE_IMAGE";

	private const string DUAL_IMAGE_EVENT = "DUAL_IMAGE";

	private WidgetTemplate m_boxProductBannerWidget;

	private Box m_box;

	private StoreManager m_storeManager;

	private EventTimingManager m_eventTimingManager;

	private BoxProductBannerDbfRecord m_productBannerDbfRecord;

	private bool? m_productAlreadyOwned;

	private bool m_triedFindingEventTiming;

	private bool m_isShopReady;

	private bool m_isBoxInCorrectState;

	private bool m_bannerImageLoadedCorrectly;

	private bool m_bannerSecondaryImageLoadedCorrectly;

	private bool m_isBannerUnfurled;

	private BoxProductBannerDataModel m_bannerDataModel;

	private void Start()
	{
		if (Init())
		{
			SetupCallbacks();
			if (HearthstoneApplication.IsInternal())
			{
				CheatMgr.Get().RegisterCategory("box");
				CheatMgr.Get().RegisterCheatHandler("showboxproductbanner", OnProcessCheat_ShowBoxProductBanner, "Show the box product banner with data specified by HE2 row id");
				CheatMgr.Get().RegisterCheatHandler("hideboxproductbanner", OnProcessCheat_HideBoxProductBanner, "Hide the box product banner");
			}
		}
	}

	public bool Init()
	{
		m_boxProductBannerWidget = GetComponent<WidgetTemplate>();
		if (m_boxProductBannerWidget == null)
		{
			Log.Box.PrintWarning("BoxProductBanner: Unable to find banner widget");
			return false;
		}
		m_boxProductBannerWidget.RegisterEventListener(OnWidgetEvent);
		m_box = Box.Get();
		if (m_box == null)
		{
			Log.Box.PrintWarning("BoxProductBanner: Unable to get box component");
			return false;
		}
		m_storeManager = StoreManager.Get();
		if (m_storeManager == null)
		{
			Log.Box.PrintWarning("BoxProductBanner: Unable to get store manager");
			return false;
		}
		m_isShopReady = m_storeManager.IsOpen();
		if (!ServiceManager.TryGet<EventTimingManager>(out var eventTimingManager))
		{
			Log.Box.PrintWarning("BoxProductBanner: Unable to get event timing manager");
			return false;
		}
		m_eventTimingManager = eventTimingManager;
		return true;
	}

	private bool IsBoxProductBannerEnabledOnGuardian()
	{
		return NetCache.Get().GetNetObject<NetCache.NetCacheFeatures>().BoxProductBannerEnabled;
	}

	private void SetupCallbacks()
	{
		m_box.AddTransitionStartedListener(OnBoxTransitionStarted);
		m_box.AddTransitionFinishedListener(OnBoxTranstionFinished);
		m_storeManager.RegisterStoreShownListener(OnShopOpened);
		m_storeManager.RegisterStatusChangedListener(OnShopAvailable);
		if (m_eventTimingManager.HasReceivedEventTimingsFromServer)
		{
			OnReceivedEventTimingsFromServer();
		}
		else
		{
			m_eventTimingManager.OnReceivedEventTimingsFromServer += OnReceivedEventTimingsFromServer;
		}
	}

	private Color GetColorFromBannerHex(string bannerColorHex)
	{
		if (!ColorUtility.TryParseHtmlString(bannerColorHex, out var color))
		{
			Log.Box.PrintWarning("BoxProductBanner: Unable to parse banner color");
			return Color.white;
		}
		return color;
	}

	private BoxProductBannerDbfRecord GetActiveProductBannerRecord()
	{
		EventTimingManager eventTimingManager = EventTimingManager.Get();
		List<BoxProductBannerDbfRecord> productBannerRecords = GameDbf.BoxProductBanner.GetRecords();
		if (productBannerRecords == null)
		{
			return null;
		}
		productBannerRecords.Sort((BoxProductBannerDbfRecord a, BoxProductBannerDbfRecord b) => b.ID.CompareTo(a.ID));
		foreach (BoxProductBannerDbfRecord product in productBannerRecords)
		{
			if (eventTimingManager.IsEventActive(product.EventTiming))
			{
				return product;
			}
		}
		return null;
	}

	private BoxProductBannerDbfRecord GetProductRecordByID(int id)
	{
		List<BoxProductBannerDbfRecord> productBannerRecords = GameDbf.BoxProductBanner.GetRecords();
		if (productBannerRecords == null)
		{
			return null;
		}
		foreach (BoxProductBannerDbfRecord product in productBannerRecords)
		{
			if (product.ID == id)
			{
				return product;
			}
		}
		return null;
	}

	private void OnDestroy()
	{
		if (m_boxProductBannerWidget != null)
		{
			m_boxProductBannerWidget.RemoveEventListener(OnWidgetEvent);
		}
		if (m_box != null)
		{
			m_box.RemoveTransitionStartedListener(OnBoxTransitionStarted);
			m_box.RemoveTransitionFinishedListener(OnBoxTranstionFinished);
		}
		if (m_storeManager != null)
		{
			m_storeManager.RemoveStoreShownListener(OnShopOpened);
			m_storeManager.RemoveStatusChangedListener(OnShopAvailable);
		}
		if (m_eventTimingManager != null)
		{
			m_eventTimingManager.OnReceivedEventTimingsFromServer -= OnReceivedEventTimingsFromServer;
		}
	}

	private void OnBoxTransitionStarted()
	{
		m_isBoxInCorrectState = false;
	}

	private void OnBoxTranstionFinished(object userData)
	{
		if (m_box.GetState() == Box.State.HUB_WITH_DRAWER)
		{
			m_isBoxInCorrectState = true;
			TryUnfurlBanner();
		}
	}

	private void OnReceivedEventTimingsFromServer()
	{
		m_productBannerDbfRecord = GetActiveProductBannerRecord();
		if (m_productBannerDbfRecord != null)
		{
			InitBannerDataModelFromRecord(m_productBannerDbfRecord);
			if (!m_productAlreadyOwned.HasValue && m_isShopReady)
			{
				m_productAlreadyOwned = IsProductOwned(m_productBannerDbfRecord.PmtProductId);
			}
		}
		m_triedFindingEventTiming = true;
		TryUnfurlBanner();
	}

	private void InitBannerDataModelFromRecord(BoxProductBannerDbfRecord record, Action OnImageLoaded = null)
	{
		m_bannerDataModel = new BoxProductBannerDataModel();
		m_bannerDataModel.ButtonText = record.ButtonText;
		m_bannerDataModel.HeaderText = record.Text;
		m_bannerDataModel.BannerColor = GetColorFromBannerHex(record.BannerColorHex);
		if (!string.IsNullOrEmpty(record.Texture))
		{
			ObjectCallback callback = delegate(AssetReference assetRef, UnityEngine.Object obj, object callbackData)
			{
				m_bannerDataModel.ImageTexture = obj as Texture;
				m_bannerImageLoadedCorrectly = true;
				if (m_bannerSecondaryImageLoadedCorrectly)
				{
					m_boxProductBannerWidget.BindDataModel(m_bannerDataModel);
					TryUnfurlBanner();
					if (OnImageLoaded != null)
					{
						OnImageLoaded();
					}
				}
			};
			AssetLoader.Get().LoadTexture(record.Texture, callback);
		}
		if (!string.IsNullOrEmpty(record.SecondaryTexture))
		{
			ObjectCallback secondaryCallback = delegate(AssetReference assetRef, UnityEngine.Object obj, object callbackData)
			{
				m_bannerDataModel.SecondaryImageTexture = obj as Texture;
				m_bannerSecondaryImageLoadedCorrectly = true;
				if (m_bannerImageLoadedCorrectly)
				{
					m_boxProductBannerWidget.BindDataModel(m_bannerDataModel);
					TryUnfurlBanner();
					if (OnImageLoaded != null)
					{
						OnImageLoaded();
					}
				}
			};
			AssetLoader.Get().LoadTexture(record.SecondaryTexture, secondaryCallback);
		}
		else
		{
			m_bannerSecondaryImageLoadedCorrectly = true;
		}
		if (!string.IsNullOrEmpty(record.SecondaryTexture) && !string.IsNullOrEmpty(record.Texture))
		{
			m_boxProductBannerWidget.TriggerEvent("DUAL_IMAGE");
		}
		else
		{
			m_boxProductBannerWidget.TriggerEvent("SINGLE_IMAGE");
		}
	}

	private void OnShopAvailable(bool isAvailable)
	{
		if (!m_productAlreadyOwned.HasValue && m_productBannerDbfRecord != null)
		{
			m_productAlreadyOwned = IsProductOwned(m_productBannerDbfRecord.PmtProductId);
		}
		m_isShopReady = isAvailable;
		if (m_isShopReady)
		{
			TryUnfurlBanner();
		}
		else
		{
			FurlBanner();
		}
	}

	private bool IsProductOwned(long pmtId)
	{
		if (!ServiceManager.TryGet<IProductDataService>(out var dataService))
		{
			return true;
		}
		if (!dataService.TryGetProduct(pmtId, out var bundle))
		{
			Log.Box.PrintWarning("BoxProductBanner: Unable to retieve the product bundle for " + pmtId);
			return true;
		}
		return m_storeManager.IsProductAlreadyOwned(bundle);
	}

	private void OnShopOpened()
	{
		if (!Shop.DontFullyOpenShop)
		{
			s_hasSeenBannerThisSession = true;
			FurlBanner();
		}
	}

	private void TryUnfurlBanner()
	{
		if (IsBannerReady())
		{
			UnfurlBanner();
		}
	}

	private bool IsBannerReady()
	{
		if (m_productBannerDbfRecord == null)
		{
			Log.Box.Print("BoxProductBanner::IsBannerReady - No Record");
			return false;
		}
		if (!m_isShopReady)
		{
			Log.Box.Print("BoxProductBanner::IsBannerReady - Shop not ready");
			return false;
		}
		if (!m_productAlreadyOwned.HasValue)
		{
			Log.Box.Print("BoxProductBanner::IsBannerReady - Product ownership not established");
			return false;
		}
		if (!m_triedFindingEventTiming)
		{
			Log.Box.Print("BoxProductBanner::IsBannerReady - Event timings not checked");
			return false;
		}
		if (!m_isBoxInCorrectState)
		{
			Log.Box.Print("BoxProductBanner::IsBannerReady - Box is not in the correct state");
			return false;
		}
		if (!m_bannerImageLoadedCorrectly)
		{
			Log.Box.Print("BoxProductBanner::IsBannerReady - Banner image not loaded");
			return false;
		}
		if (!m_bannerSecondaryImageLoadedCorrectly)
		{
			Log.Box.Print("BoxProductBanner::IsBannerReady - Banner secondary image not loaded");
			return false;
		}
		if (!IsBoxProductBannerEnabledOnGuardian())
		{
			Log.Box.Print("BoxProductBanner::IsBannerReady - Banner blocked on guardian");
			return false;
		}
		return true;
	}

	private void UnfurlBanner(bool forced = false)
	{
		if (CanShowBanner(forced))
		{
			TelemetryManager.Client().SendBoxProductBannerDisplayed(m_productBannerDbfRecord.CampaignName, m_productBannerDbfRecord.Texture, m_productBannerDbfRecord.PmtProductId);
			m_boxProductBannerWidget.TriggerEvent("ANIMATE_IN");
			m_isBannerUnfurled = true;
		}
	}

	private bool CanShowBanner(bool forced)
	{
		if (s_hasSeenBannerThisSession && !forced)
		{
			Log.Box.Print("BoxProductBanner::CanShowBanner - Banner seen already in this session");
			return false;
		}
		if ((!m_productAlreadyOwned.HasValue || m_productAlreadyOwned.Value) && !forced)
		{
			Log.Box.Print("BoxProductBanner::CanShowBanner - Product is owned or ownership can't be established");
			return false;
		}
		return !m_isBannerUnfurled;
	}

	private void FurlBanner()
	{
		if (m_isBannerUnfurled)
		{
			m_boxProductBannerWidget.TriggerEvent("ANIMATE_OUT");
			m_isBannerUnfurled = false;
		}
	}

	private void OnWidgetEvent(string eventName)
	{
		if (eventName == "BOX_PRODUCT_PRESSED")
		{
			TelemetryManager.Client().SendBoxProductBannerClicked(m_productBannerDbfRecord.CampaignName, m_productBannerDbfRecord.Texture, m_productBannerDbfRecord.PmtProductId);
			switch (m_productBannerDbfRecord.ProductActionType)
			{
			case Assets.BoxProductBanner.ProductActionType.OPEN_SHOP:
				m_storeManager.StartGeneralTransaction();
				break;
			case Assets.BoxProductBanner.ProductActionType.OPEN_SHOP_TO_PRODUCT:
				ProductPageJobs.OpenToProductPageWhenReady(m_productBannerDbfRecord.PmtProductId);
				break;
			}
		}
	}

	private bool OnProcessCheat_ShowBoxProductBanner(string func, string[] args, string rawArgs)
	{
		if (args.Length < 1)
		{
			UIStatus.Get().AddInfo("Please specify a id from HE2 Box product banner table", 10f);
			return true;
		}
		if (!int.TryParse(args[0], out var parsedID))
		{
			UIStatus.Get().AddInfo("Unable to parse ID, please input a valid integer", 10f);
			return true;
		}
		BoxProductBannerDbfRecord productBannerRecord = GetProductRecordByID(parsedID);
		if (productBannerRecord == null)
		{
			UIStatus.Get().AddInfo("Unable to get record, please verify the id is a valid record id", 10f);
			return true;
		}
		m_productBannerDbfRecord = productBannerRecord;
		InitBannerDataModelFromRecord(productBannerRecord, delegate
		{
			UnfurlBanner(forced: true);
		});
		return true;
	}

	private bool OnProcessCheat_HideBoxProductBanner(string func, string[] args, string rawArgs)
	{
		FurlBanner();
		return true;
	}
}
