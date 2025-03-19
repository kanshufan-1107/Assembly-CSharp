using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Blizzard.T5.Services;
using Hearthstone.DataModels;
using Hearthstone.Progression;
using Hearthstone.Store;
using Hearthstone.UI;
using PegasusLuckyDraw;
using PegasusUtil;
using UnityEngine;

[RequireComponent(typeof(WidgetTemplate))]
public class LuckyDrawWidget : MonoBehaviour, IStore
{
	public static AssetReference LUCKY_DRAW_MANAGER_POPUP_PREFAB = new AssetReference("LuckyDrawManagerPopup.prefab:7411ab66e5e09ed408bc291d20af76d6");

	public AsyncReference m_hammerManagerReference;

	public AsyncReference m_boardDetailsDisplayReference;

	public AsyncReference m_boardDetailsDisplayRenderedReference;

	public AsyncReference m_finisherDetailsDisplayReference;

	public AsyncReference m_finisherDetailsDisplayRenderedReference;

	public AsyncReference m_portraitDetailsDisplayReference;

	public AsyncReference m_emoteDetailsDisplayReference;

	[SerializeField]
	private AsyncReference m_LuckyDrawShopWidgetReference;

	private LuckyDrawBoardSkinDetails m_boardDetailsDisplay;

	private LuckyDrawBoardSkinDetails m_boardDetailsDisplayRendered;

	private LuckyDrawFinisherDetails m_finisherDetailsDisplay;

	private LuckyDrawFinisherDetails m_finisherDetailsDisplayRendered;

	private LuckyDrawPortraitDetails m_portraitDetailsDisplay;

	private LuckyDrawEmoteDetails m_emoteDetailsDisplay;

	private RewardPresenter m_rewardPresenter = new RewardPresenter();

	private LuckyDrawHammerSlot m_luckyDrawHammerSlot;

	private Widget m_LuckyDrawShopPopupWidget;

	[SerializeField]
	private WidgetInstance m_luckyDrawLayout;

	[SerializeField]
	private Renderer m_FrameRenderer;

	[SerializeField]
	private BoxCollider m_clickBlocker;

	private Widget m_widget;

	private GameObject m_owner;

	private int m_rewardTileSelected;

	private bool m_usingFirstHammer;

	private const string CLOSE_EVENT = "CODE_CLOSE";

	private const string SPEND_HAMMER = "CODE_HAMMER_USED";

	private const string TILE_ANTICIPATION = "CODE_TILE_ANTICIPATION";

	private const string TILE_ANTICIPATION_FINISHED = "CODE_TILE_ANTICIPATION_FINISHED";

	private const string MOUSE_OFF_HAMMER_BUTTON = "HAMMER_BUTTON_MOUSE_OFF";

	private const string HAMMER_SMASH_TILE = "HAMMER_FSM_SMASH_TILE";

	private const string USE_FIRST_HAMMER = "CODE_USE_FIRST_HAMMER";

	private const string START_FIRST_HAMMER_ANIM = "CODE_START_FIRST_HAMMER_ANIM";

	private const string kRewardDetailsEventName = "REWARD_clicked";

	private const string kRewardGrantedEventName = "CODE_SEND_REWARD_GRANTED";

	private const string kShowDetailViewEventName = "LUCKY_DRAW_SHOW";

	private const string kShowDetailRewardViewEventName = "LUCKY_DRAW_SHOW_REWARD";

	private const string HIDE_BATTLEPASS_SHOP = "CODE_HIDE_BATTLEBASH_SHOP";

	private const string SHOW_BATTLEBASH_SHOP = "CODE_BATTLEBASH_PURCHASE";

	private const string SHOP_SHOW_INFO = "SHOP_SHOW_INFO";

	private const string SHOW_NO_HAMMER_POPUP = "SHOW_NO_HAMMER_POPUP";

	private bool m_isStoreOpen;

	private PopupSwitcher m_popupSwitcher = new PopupSwitcher();

	private LuckyDrawManager m_luckyDrawManager;

	public event Action OnOpened;

	public event Action<StoreClosedArgs> OnClosed;

	public event Action OnReady;

	public event Action<BuyProductEventArgs> OnProductPurchaseAttempt;

	public event Action OnProductOpened;

	private void OnDestroy()
	{
		ShutDownLuckyDrawStore();
		m_luckyDrawManager?.UnregisterOnInitOrUpdateFinishedCallback(OnLuckyDrawDataInitializedOrUpdated);
	}

	private void Awake()
	{
		m_luckyDrawManager = LuckyDrawManager.Get();
		if (m_luckyDrawManager == null)
		{
			Error.AddDevWarning("Error", "[LuckyDrawWidget] Awake() LuckyDrawManager is null!");
			return;
		}
		m_widget = GetComponent<WidgetTemplate>();
		if (m_widget == null)
		{
			Error.AddDevWarning("UI Error", "[LuckyDrawWidget] Awake() WidgetTemplate not found on {0}", base.gameObject.name);
			return;
		}
		m_widget.RegisterEventListener(HandleEvent);
		m_popupSwitcher.RegisterPopupWidgetInstance(m_boardDetailsDisplayReference, "OffDialogClick_code", "LuckyDrawDismissPopup_code", delegate(IDataModel dataModel)
		{
			m_boardDetailsDisplay.OnShow(dataModel);
		}, delegate
		{
			m_boardDetailsDisplay.OnHide();
		});
		m_popupSwitcher.RegisterPopupWidgetInstance(m_boardDetailsDisplayRenderedReference, "OffDialogClick_code", "LuckyDrawDismissPopup_code", delegate(IDataModel dataModel)
		{
			m_boardDetailsDisplayRendered.OnShow(dataModel);
		}, delegate
		{
			m_boardDetailsDisplayRendered.OnHide();
		});
		m_popupSwitcher.RegisterPopupWidgetInstance(m_finisherDetailsDisplayReference, "OffDialogClick_code", "LuckyDrawDismissPopup_code", delegate(IDataModel dataModel)
		{
			m_finisherDetailsDisplay.OnShow(dataModel);
		}, delegate
		{
			m_finisherDetailsDisplay.OnHide();
		});
		m_popupSwitcher.RegisterPopupWidgetInstance(m_finisherDetailsDisplayRenderedReference, "OffDialogClick_code", "LuckyDrawDismissPopup_code", delegate(IDataModel dataModel)
		{
			m_finisherDetailsDisplayRendered.OnShow(dataModel);
		}, delegate
		{
			m_finisherDetailsDisplayRendered.OnHide();
		});
		m_popupSwitcher.RegisterPopupWidgetInstance(m_portraitDetailsDisplayReference, "OffDialogClick_code", "LuckyDrawDismissPopup_code", delegate
		{
			m_portraitDetailsDisplay.Show();
		}, delegate
		{
			m_portraitDetailsDisplay.Hide();
		});
		m_popupSwitcher.RegisterPopupWidgetInstance(m_emoteDetailsDisplayReference, "OffDialogClick_code", "LuckyDrawDismissPopup_code", delegate
		{
			m_emoteDetailsDisplay.Show();
		}, delegate
		{
			m_emoteDetailsDisplay.Hide();
		});
		m_popupSwitcher.RegisterPopupWidgetInstance(m_LuckyDrawShopWidgetReference, "CODE_HIDE_BATTLEBASH_SHOP", null, delegate(IDataModel dataModel)
		{
			m_LuckyDrawShopPopupWidget.BindDataModel(((ProductSelectionDataModel)dataModel).Variant);
		});
		m_owner = base.gameObject;
		if (base.transform.parent != null && base.transform.parent.GetComponent<WidgetInstance>() != null)
		{
			m_owner = base.transform.parent.gameObject;
		}
		if (m_hammerManagerReference == null)
		{
			Error.AddDevWarning("UI Error", "[LuckyDrawWidget] Awake() Hammer Manager not assigned! LuckyDrawHammerSlot instance should be assigned");
		}
	}

	private void Start()
	{
		m_boardDetailsDisplayReference.RegisterReadyListener<WidgetInstance>(OnBoardDetailsDisplayReady);
		m_boardDetailsDisplayRenderedReference.RegisterReadyListener<WidgetInstance>(OnBoardDetailsDisplayRenderedReady);
		m_finisherDetailsDisplayReference.RegisterReadyListener<WidgetInstance>(OnFinisherDetailsDisplayReady);
		m_finisherDetailsDisplayRenderedReference.RegisterReadyListener<WidgetInstance>(OnFinisherDetailsDisplayRenderedReady);
		m_portraitDetailsDisplayReference.RegisterReadyListener<WidgetInstance>(OnPortraitDetailsDisplayReady);
		m_emoteDetailsDisplayReference.RegisterReadyListener<WidgetInstance>(OnEmoteDetailsDisplayReady);
		m_hammerManagerReference.RegisterReadyListener<WidgetInstance>(OnHammerPlaymakerReady);
		m_LuckyDrawShopWidgetReference.RegisterReadyListener<WidgetInstance>(OnLuckyDrawShopWidgetReady);
		m_widget.RegisterEventListener(RewardDetailsEventListener);
		this.OnReady?.Invoke();
		this.OnProductOpened?.Invoke();
	}

	private void HandleEvent(string eventName)
	{
		switch (eventName)
		{
		case "CODE_CLOSE":
			Close();
			break;
		case "CODE_HAMMER_USED":
			InitiateSpendHammerFlow();
			break;
		case "CODE_BATTLEBASH_PURCHASE":
			TelemetryManager.Client()?.SendLuckyDrawEventMessage("LuckyDrawBuyButtonClicked");
			ShowBattleBashShop();
			break;
		case "CODE_TILE_ANTICIPATION":
			PlayTileAnticipationAnim();
			break;
		case "CODE_TILE_ANTICIPATION_FINISHED":
			TileAnticipationFinished();
			break;
		case "HAMMER_BUTTON_MOUSE_OFF":
			TrySetHammerButtonIdleAnimation();
			break;
		case "HAMMER_FSM_SMASH_TILE":
			PlayTileSmashAnim();
			break;
		case "SHOW_NO_HAMMER_POPUP":
		{
			AlertPopup.PopupInfo info = new AlertPopup.PopupInfo
			{
				m_headerText = GameStrings.Get("GLUE_BATTLEBASH_ERROR_NO_HAMMERS"),
				m_text = GameStrings.Get("GLUE_BATTLEBASH_EARN_MORE_HAMMERS"),
				m_responseDisplay = AlertPopup.ResponseDisplay.OK,
				m_showAlertIcon = true,
				m_okText = GameStrings.Get("GLOBAL_OKAY")
			};
			DialogManager.Get().ShowPopup(info);
			break;
		}
		case "CODE_START_FIRST_HAMMER_ANIM":
			m_usingFirstHammer = LuckyDrawManager.Get().HasUnclamedFree();
			InitializeHammerFSMVariables();
			m_luckyDrawHammerSlot.GetComponent<Widget>().TriggerEvent("CODE_DO_FIRST_HAMMER_CLAIM_ANIMATION");
			break;
		case "CODE_USE_FIRST_HAMMER":
			LuckyDrawManager.Get().UseLuckyDrawHammer(this);
			break;
		}
	}

	private void HandleShopPopupEvent(string eventName)
	{
		switch (eventName)
		{
		case "SHOP_BUY_WITH_FIRST_CURRENCY":
			StartLuckyDrawTransaction(0);
			break;
		case "SHOP_BUY_WITH_ALT_CURRENCY":
			StartLuckyDrawTransaction(1);
			break;
		case "CODE_HIDE_BATTLEBASH_SHOP":
			m_popupSwitcher.DismissPopup(m_LuckyDrawShopWidgetReference);
			ShutDownLuckyDrawStore();
			break;
		case "SHOP_SHOW_INFO":
			StoreManager.Get().ShowStoreInfo();
			break;
		}
	}

	public void SetupEndOfLoadSceneObjects()
	{
		if (m_luckyDrawHammerSlot == null)
		{
			Error.AddDevWarning("Error", "[LuckyDrawWidget] SetupEndOfLoadSceneObjects() m_luckyDrawHammerSlot was null!");
			return;
		}
		m_clickBlocker.enabled = false;
		m_luckyDrawHammerSlot.DisplayFirstHammer();
	}

	public void Close()
	{
		UnityEngine.Object.Destroy(m_owner);
	}

	public void Show()
	{
		m_luckyDrawLayout.RegisterReadyListener(delegate
		{
			InitializeLuckyDrawLayoutWidget();
		});
	}

	private void ShowFinisherDetailDisplay(LuckyDrawRewardDataModel dataModel, bool showRewardGrantVFX)
	{
		if (dataModel == null)
		{
			Error.AddDevWarning("UI Error", "[LuckyDrawWidget] ShowFinisherDetailDisplay() dataModel was null!");
			return;
		}
		if (StandaloneCosmeticPreviewSceneLoader.CosmeticsRenderingEnabled() && !string.IsNullOrEmpty(dataModel.RewardList.Items[0].BGFinisher.DetailsRenderConfig))
		{
			m_finisherDetailsDisplayRendered.ShowingRewardGrantVFX = showRewardGrantVFX;
			m_popupSwitcher.ShowPopup(m_finisherDetailsDisplayRenderedReference, dataModel);
			return;
		}
		if (NetCache.Get() == null || NetCache.Get().GetNetObject<NetCache.NetCacheFeatures>() == null || NetCache.Get().GetNetObject<NetCache.NetCacheFeatures>().Collection == null || string.IsNullOrEmpty(dataModel.RewardList.Items[0].BGFinisher.DetailsRenderConfig))
		{
			TelemetryManager.Client()?.SendCosmeticsRenderingFallback($"LuckyDrawWidget::Finisher[{dataModel.RewardList.Items[0].BGFinisher.FinisherDbiId}]");
		}
		if (m_finisherDetailsDisplay != null)
		{
			m_finisherDetailsDisplay.ShowingRewardGrantVFX = showRewardGrantVFX;
			m_popupSwitcher.ShowPopup(m_finisherDetailsDisplayReference, dataModel);
		}
	}

	private void ShowBoardDetailDisplay(LuckyDrawRewardDataModel dataModel, bool showRewardVFX)
	{
		if (dataModel == null)
		{
			Error.AddDevWarning("UI Error", "[LuckyDrawWidget] ShowBoardDetailDisplay() dataModel was null!");
			return;
		}
		if (StandaloneCosmeticPreviewSceneLoader.CosmeticsRenderingEnabled() && !string.IsNullOrEmpty(dataModel.RewardList.Items[0].BGBoardSkin.DetailsRenderConfig))
		{
			m_boardDetailsDisplayRendered.ShowingRewardGrantVFX = showRewardVFX;
			m_popupSwitcher.ShowPopup(m_boardDetailsDisplayRenderedReference, dataModel);
			return;
		}
		if (NetCache.Get() == null || NetCache.Get().GetNetObject<NetCache.NetCacheFeatures>() == null || NetCache.Get().GetNetObject<NetCache.NetCacheFeatures>().Collection == null || string.IsNullOrEmpty(dataModel.RewardList.Items[0].BGBoardSkin.DetailsRenderConfig))
		{
			TelemetryManager.Client()?.SendCosmeticsRenderingFallback($"LuckyDrawWidget::Boardskin[{dataModel.RewardList.Items[0].BGBoardSkin.BoardDbiId}]");
		}
		if (m_boardDetailsDisplay != null)
		{
			m_boardDetailsDisplay.ShowingRewardGrantVFX = showRewardVFX;
			m_popupSwitcher.ShowPopup(m_boardDetailsDisplayReference, dataModel);
		}
	}

	private void ShowPortraitDetailsDisplay(LuckyDrawRewardDataModel dataModel, bool showRewardGrantVFX)
	{
		if (dataModel == null)
		{
			Error.AddDevWarning("UI Error", "[LuckyDrawWidget] ShowPortraitDetailDisplay() datamodel was null!");
			return;
		}
		m_popupSwitcher.ShowPopup(m_portraitDetailsDisplayReference, dataModel);
		if (showRewardGrantVFX)
		{
			EventFunctions.TriggerEvent(m_portraitDetailsDisplay.transform.parent, "LUCKY_DRAW_SHOW_REWARD");
		}
		else
		{
			EventFunctions.TriggerEvent(m_portraitDetailsDisplay.transform.parent, "LUCKY_DRAW_SHOW");
		}
	}

	private void ShowEmoteDetailsDisplay(LuckyDrawRewardDataModel dataModel, bool showRewardGrantVFX)
	{
		if (dataModel == null)
		{
			Error.AddDevWarning("UI Error", "[LuckyDrawWidget] ShowEmoteDetailsDisplay() datamodel was null!");
			return;
		}
		m_popupSwitcher.ShowPopup(m_emoteDetailsDisplayReference, dataModel);
		if (showRewardGrantVFX)
		{
			EventFunctions.TriggerEvent(m_emoteDetailsDisplay.transform.parent, "LUCKY_DRAW_SHOW_REWARD");
		}
		else
		{
			EventFunctions.TriggerEvent(m_emoteDetailsDisplay.transform.parent, "LUCKY_DRAW_SHOW");
		}
	}

	private void ShowBattleBashShop()
	{
		if (m_LuckyDrawShopPopupWidget == null)
		{
			Error.AddDevWarning("UI Error", "[LuckyDrawWidget] ShowBattleBashShop() Cannot open Lucky Draw shop popup. m_LuckyDrawShopPopupWidget was null.");
			return;
		}
		ProductInfo luckyDrawBundle = m_luckyDrawManager.GetProduct();
		if (luckyDrawBundle == null)
		{
			Error.AddDevWarning("UI Error", "[LuckyDrawWidget] ShowBattleBashShop() Cannot open Lucky Draw shop popup. luckyDrawBundle was null.");
			return;
		}
		SetUpLuckyDrawStore();
		ProductDataModel productDataModel;
		if (!ServiceManager.TryGet<IProductDataService>(out var dataService) || (productDataModel = dataService.GetProductDataModel(luckyDrawBundle.Id.Value)) == null)
		{
			AlertPopup.PopupInfo shopUnavailableAlert = new AlertPopup.PopupInfo
			{
				m_showAlertIcon = false,
				m_responseDisplay = AlertPopup.ResponseDisplay.CONFIRM,
				m_confirmText = GameStrings.Get("GLOBAL_BUTTON_OK"),
				m_alertTextAlignment = UberText.AlignmentOptions.Center,
				m_alertTextAlignmentAnchor = UberText.AnchorOptions.Middle,
				m_headerText = GameStrings.Get("GLUE_BATTLEBASH_SHOP_UNAVAILABLE_HEADER"),
				m_text = GameStrings.Get("GLUE_BATTLEBASH_SHOP_UNAVAILABLE_BODY")
			};
			DialogManager.Get().ShowPopup(shopUnavailableAlert);
		}
		else
		{
			ProductSelectionDataModel productSelection = new ProductSelectionDataModel();
			productSelection.MaxQuantity = 1;
			productSelection.Quantity = 1;
			productSelection.Variant = productDataModel;
			m_popupSwitcher.ShowPopup(m_LuckyDrawShopWidgetReference, productSelection);
			EventFunctions.TriggerEvent(m_LuckyDrawShopPopupWidget.transform, "LUCKY_DRAW_SHOW");
		}
	}

	private void InitializeLuckyDrawLayoutWidget()
	{
		LuckyDrawLayout layout = m_luckyDrawLayout.Widget.GetComponent<LuckyDrawLayout>();
		if (layout == null)
		{
			Error.AddDevWarning("UI Error", "[LuckyDrawWidget] InitializeLuckyDrawLayoutWidget() could not find LuckydrawLayout on object {0}", m_luckyDrawLayout.name);
		}
		else if (!m_luckyDrawManager.IsIntialized())
		{
			Error.AddDevWarning("UI Error", "[LuckyDrawWidget] InitializeLuckyDrawLayoutWidget() Lucky Draw Data not initialized!");
		}
		else
		{
			m_luckyDrawManager.BindLuckyDrawDataModelToWidget(m_luckyDrawLayout);
			layout.InitializeRewardTileWidgets(m_luckyDrawManager.GetBattlegroundsLuckyDrawDataModel().Rewards);
		}
	}

	private void InitiateSpendHammerFlow()
	{
		if (m_luckyDrawManager.GetBattlegroundsLuckyDrawDataModel().Hammers <= 0)
		{
			LuckyDrawManager.Get().LogError("Error [LuckyDrawWidget] InitiateSpendHammerFlow() The player attempted to spend a hammer when no hammers are available!");
			LuckyDrawUtils.ShowErrorAndReturnToLobby();
		}
		else
		{
			InitializeHammerFSMVariables();
			m_luckyDrawManager.UseLuckyDrawHammer(this);
		}
	}

	private void InitializeHammerFSMVariables()
	{
		m_luckyDrawHammerSlot.GetComponent<Widget>().TriggerEvent("CODE_INITIALIZE_HAMMER", new TriggerEventParameters(null, m_FrameRenderer.transform.localPosition));
	}

	private void PlayTileAnticipationAnim()
	{
		m_luckyDrawLayout.Widget.GetComponent<LuckyDrawLayout>().AnimateTiles();
	}

	private void TileAnticipationFinished()
	{
		m_luckyDrawHammerSlot.GetComponent<Widget>().TriggerEvent("CODE_ANTICIPATION_FINISHED");
	}

	private void PlayTileSmashAnim()
	{
		m_luckyDrawLayout.Widget.GetComponent<LuckyDrawLayout>().PlayTileSmashAnim(m_rewardTileSelected);
	}

	private void OnBoardDetailsDisplayReady(WidgetInstance widget)
	{
		m_boardDetailsDisplay = widget.Widget.GetComponent<LuckyDrawBoardSkinDetails>();
	}

	private void OnBoardDetailsDisplayRenderedReady(WidgetInstance widget)
	{
		m_boardDetailsDisplayRendered = widget.Widget.GetComponent<LuckyDrawBoardSkinDetails>();
	}

	private void OnFinisherDetailsDisplayReady(WidgetInstance widget)
	{
		m_finisherDetailsDisplay = widget.Widget.GetComponent<LuckyDrawFinisherDetails>();
	}

	private void OnFinisherDetailsDisplayRenderedReady(WidgetInstance widget)
	{
		m_finisherDetailsDisplayRendered = widget.Widget.GetComponent<LuckyDrawFinisherDetails>();
	}

	private void OnPortraitDetailsDisplayReady(WidgetInstance widget)
	{
		m_portraitDetailsDisplay = widget.Widget.GetComponent<LuckyDrawPortraitDetails>();
	}

	private void OnEmoteDetailsDisplayReady(WidgetInstance widget)
	{
		m_emoteDetailsDisplay = widget.Widget.GetComponent<LuckyDrawEmoteDetails>();
	}

	private void OnHammerPlaymakerReady(WidgetInstance widget)
	{
		LuckyDrawHammerSlot hammerSlot = widget.Widget.GetComponent<LuckyDrawHammerSlot>();
		if (hammerSlot == null)
		{
			Error.AddDevWarning("UI Error", "[LuckyDrawWidget] OnHammerPlaymakerReady() hammerSlot was null!");
			return;
		}
		m_luckyDrawHammerSlot = hammerSlot;
		if (!m_luckyDrawManager.IsIntialized())
		{
			Error.AddDevWarning("UI Error", "[LuckyDrawWidget] OnHammerPlaymakerReady() luckyDrawManager not initialized!");
		}
		else
		{
			m_luckyDrawManager.BindAllLuckyDrawDataModelToWidget(widget);
		}
	}

	private void OnLuckyDrawShopWidgetReady(WidgetInstance widget)
	{
		m_LuckyDrawShopPopupWidget = widget;
		widget.RegisterEventListener(HandleShopPopupEvent);
	}

	private void RewardDetailsEventListener(string eventName)
	{
		if (eventName == "REWARD_clicked")
		{
			ShowDetailDisplay(playRewardGrantVFX: false);
		}
		else if (eventName == "CODE_SEND_REWARD_GRANTED")
		{
			ShowDetailDisplay(playRewardGrantVFX: true);
		}
	}

	private void ShowDetailDisplay(bool playRewardGrantVFX)
	{
		EventDataModel eventDataModel = m_widget.GetDataModel<EventDataModel>();
		if (eventDataModel == null)
		{
			Error.AddDevWarning("Error", "[LuckyDrawWidget] ShowDetailDisplay() No eventDataModel found from event call");
			return;
		}
		LuckyDrawRewardDataModel rewardData = (LuckyDrawRewardDataModel)eventDataModel.Payload;
		if (rewardData == null)
		{
			Error.AddDevWarning("Error", "[LuckyDrawWidget] ShowDetailDisplay() No eventDataModel Payload in event call");
			return;
		}
		RewardListDataModel rewardList = rewardData.RewardList;
		if (rewardList == null || rewardList.Items.Count <= 0)
		{
			Error.AddDevWarning("Error", "[LuckyDrawWidget] ShowDetailDisplay() Reward list has no valid data!");
			return;
		}
		switch (rewardList.Items[0].ItemType)
		{
		case RewardItemType.BATTLEGROUNDS_BOARD_SKIN:
			ShowBoardDetailDisplay(rewardData, playRewardGrantVFX);
			break;
		case RewardItemType.BATTLEGROUNDS_FINISHER:
			ShowFinisherDetailDisplay(rewardData, playRewardGrantVFX);
			break;
		case RewardItemType.BATTLEGROUNDS_HERO_SKIN:
		case RewardItemType.BATTLEGROUNDS_GUIDE_SKIN:
			ShowPortraitDetailsDisplay(rewardData, playRewardGrantVFX);
			break;
		case RewardItemType.BATTLEGROUNDS_EMOTE:
		case RewardItemType.BATTLEGROUNDS_EMOTE_PILE:
			ShowEmoteDetailsDisplay(rewardData, playRewardGrantVFX);
			break;
		case RewardItemType.MERCENARY_RANDOM_MERCENARY:
		case RewardItemType.MERCENARY_KNOCKOUT_SPECIFIC:
		case RewardItemType.MERCENARY_KNOCKOUT_RANDOM:
			break;
		}
	}

	private void TrySetHammerButtonIdleAnimation()
	{
		if (!m_luckyDrawHammerSlot.HammerPlaymaker.FsmVariables.GetFsmBool("HammerAnimationInProgress").Value)
		{
			m_luckyDrawHammerSlot.HammerPlaymaker.SendEvent("Button_MouseOff");
		}
	}

	public void OnRewardResponseReceived(LuckyDrawUseHammerResponse rewardResponse)
	{
		if (rewardResponse.HasErrorCode && rewardResponse.ErrorCode != 0)
		{
			LuckyDrawManager.Get().LogError($"Error [LuckyDrawWidget] OnRewardResponseReceived() response had error {rewardResponse.ErrorCode}");
			LuckyDrawUtils.ShowErrorAndReturnToLobby();
			return;
		}
		m_rewardTileSelected = m_luckyDrawLayout.Widget.GetComponent<LuckyDrawLayout>().GetTileFromRewardID(rewardResponse.GrantedRewardId);
		if (m_rewardTileSelected < 0)
		{
			LuckyDrawManager.Get().LogError("Error [LuckyDrawWidget] OnRewardResponseReceived() selected reward not found!");
			LuckyDrawUtils.ShowErrorAndReturnToLobby();
		}
		SetupHammerSmashAnimation();
		if (m_usingFirstHammer && rewardResponse.NumUnusedFreeHammersRemaining < 1)
		{
			m_usingFirstHammer = false;
			m_luckyDrawManager.UsedFreeHammer(rewardResponse);
		}
	}

	private void SetupHammerSmashAnimation()
	{
		Vector3 targetVector = m_luckyDrawLayout.Widget.GetComponent<LuckyDrawLayout>().GetWorldPositionOfTile(m_rewardTileSelected);
		m_luckyDrawHammerSlot.GetComponent<Widget>().TriggerEvent("CODE_HAMMER_SMASH_READY", new TriggerEventParameters(null, targetVector));
	}

	private void SetUpLuckyDrawStore()
	{
		StoreManager storeManager = StoreManager.Get();
		storeManager.StartLuckyDrawStore(this);
		storeManager.RegisterSuccessfulPurchaseAckListener(OnSuccessfulPurchaseAck);
		storeManager.RegisterFailedPurchaseAckListener(OnFailedPurchaseAck);
		storeManager.RegisterSuccessfulPurchaseListener(OnSuccessfulPurchase);
		m_isStoreOpen = true;
		this.OnOpened?.Invoke();
		BnetBar.Get()?.RefreshCurrency();
	}

	private void ShutDownLuckyDrawStore()
	{
		this.OnClosed?.Invoke(new StoreClosedArgs());
		StoreManager storeManager = StoreManager.Get();
		storeManager.RemoveFailedPurchaseAckListener(OnFailedPurchaseAck);
		storeManager.RemoveSuccessfulPurchaseAckListener(OnSuccessfulPurchaseAck);
		storeManager.ShutDownLuckyDrawStore();
		m_isStoreOpen = false;
		BnetBar.Get()?.RefreshCurrency();
	}

	private void StartLuckyDrawTransaction(int priceIndex)
	{
		LuckyDrawManager luckyDrawManager = LuckyDrawManager.Get();
		ProductInfo bundle = m_luckyDrawManager.GetProduct();
		if (bundle == null)
		{
			Error.AddDevWarning("Error", "[LuckyDrawWidget] StartLuckyDrawTransaction() Cannot start Lucky Draw transaction. No product found. bundle was null");
			return;
		}
		ProductDataModel productDataModel;
		if (!ServiceManager.TryGet<IProductDataService>(out var dataService) || (productDataModel = dataService.GetProductDataModel(bundle.Id.Value)) == null)
		{
			Error.AddDevWarning("UI Error", "[LuckyDrawWidget] StartLuckyDrawTransaction() Cannot start Lucky Draw transaction. No product data model found. productDataModel was null");
			return;
		}
		if (!ServiceManager.TryGet<PurchaseManager>(out var purchaseManager))
		{
			Error.AddDevWarning("UI Error", "[LuckyDrawWidget] StartLuckyDrawTransaction() Cannot start Lucky Draw transaction. No purchase manager found.");
			return;
		}
		PriceDataModel priceDataModel = productDataModel.Prices.ElementAtOrDefault(priceIndex);
		if (priceDataModel == null)
		{
			Error.AddDevWarning("UI Error", "[LuckyDrawWidget] StartLuckyDrawTransaction() Cannot start Lucky Draw transaction. Product data model has no price at index {0}.", priceIndex);
			return;
		}
		TimeSpan timeRemaining = LuckyDrawUtils.GetLuckyDrawTimeRemaining(luckyDrawManager.GetActiveLuckyDrawBoxID());
		AlertPopup.PopupInfo popupInfo = new AlertPopup.PopupInfo();
		popupInfo.m_showAlertIcon = false;
		popupInfo.m_responseDisplay = AlertPopup.ResponseDisplay.CONFIRM_CANCEL;
		popupInfo.m_confirmText = GameStrings.Get("GLOBAL_BUTTON_OK");
		popupInfo.m_cancelText = GameStrings.Get("GLOBAL_CANCEL");
		popupInfo.m_alertTextAlignment = UberText.AlignmentOptions.Center;
		popupInfo.m_alertTextAlignmentAnchor = UberText.AnchorOptions.Middle;
		popupInfo.m_headerText = GameStrings.Get("GLUE_BACON_BATTLEBASH_PURCHASE_HEADER");
		popupInfo.m_text = GameStrings.Format("GLUE_BACON_BATTLEBASH_PURCHASE_BODY", timeRemaining.Days);
		popupInfo.m_responseCallback = delegate(AlertPopup.Response response, object userData)
		{
			if (response == AlertPopup.Response.CONFIRM)
			{
				purchaseManager.PurchaseProduct(productDataModel, priceDataModel, new PurchaseManager.PurchaseManagerOptions
				{
					SuppressVCConfirmtation = true
				});
			}
		};
		AlertPopup.PopupInfo daysRemainingAlert = popupInfo;
		DialogManager.Get().ShowPopup(daysRemainingAlert);
	}

	private void OnSuccessfulPurchase(ProductInfo bundle, PaymentMethod paymentMethod)
	{
		if (bundle.Items != null && bundle.Items.Count((Network.BundleItem item) => item.ItemType == ProductType.PRODUCT_TYPE_LUCKY_DRAW) > 0)
		{
			TelemetryManager.Client()?.SendLuckyDrawEventMessage("LuckyDrawPurchaseSucceeded");
		}
	}

	private void OnSuccessfulPurchaseAck(ProductInfo bundle, PaymentMethod paymentMethod)
	{
		if (bundle.Items != null && bundle.Items.Count((Network.BundleItem item) => item.ItemType == ProductType.PRODUCT_TYPE_LUCKY_DRAW) > 0)
		{
			StartCoroutine(WaitForLicenseUpdate(15f, UpdateDataAndShowUnacknowledgedHammersPopup));
		}
	}

	private void OnFailedPurchaseAck(ProductInfo bundle, PaymentMethod paymentMethod)
	{
		if (bundle.Items != null && bundle.Items.Count((Network.BundleItem item) => item.ItemType == ProductType.PRODUCT_TYPE_LUCKY_DRAW) > 0)
		{
			TelemetryManager.Client()?.SendLuckyDrawEventMessage("LuckyDrawPurchaseFailed");
		}
	}

	private IEnumerator WaitForLicenseUpdate(float timeout = 15f, Action onFinishedCallback = null)
	{
		float cancelTime = Time.time + timeout;
		while (!m_luckyDrawManager.GetLuckyDrawButtonDataModel().BattlePassPurchased)
		{
			if (Time.time > cancelTime)
			{
				LuckyDrawManager.Get().LogError("Error [LuckyDrawWidget] WaitForLicenseUpdate() timeout triggered while waiting for license after successful purchase.");
				LuckyDrawUtils.ShowErrorAndReturnToLobby();
				yield break;
			}
			yield return null;
		}
		m_popupSwitcher.HidePopup(m_LuckyDrawShopWidgetReference);
		onFinishedCallback?.Invoke();
	}

	private void UpdateDataAndShowUnacknowledgedHammersPopup()
	{
		m_luckyDrawManager.InitializeOrUpdateData(OnLuckyDrawDataInitializedOrUpdated);
	}

	private void OnLuckyDrawDataInitializedOrUpdated()
	{
		StartCoroutine(ShowBattlegroundsUnacknowledgedBonusHammersPopUp());
	}

	private IEnumerator ShowBattlegroundsUnacknowledgedBonusHammersPopUp()
	{
		if (m_luckyDrawManager.NumUnacknowledgedBonusHammers() > 0 && m_rewardPresenter != null)
		{
			while (m_rewardPresenter.IsShowingReward())
			{
				yield return new WaitForSeconds(0.1f);
			}
			RewardScrollDataModel dataModel = new RewardScrollDataModel
			{
				DisplayName = GameStrings.Get("GLUE_BACON_REWARD_BATTLE_BASH_BONUS_HAMMERS"),
				Description = GameStrings.Get("GLUE_BACON_REWARD_BATTLE_BASH_EARN_MORE_HAMMERS_DESC"),
				RewardList = new RewardListDataModel
				{
					Items = new DataModelList<RewardItemDataModel>
					{
						new RewardItemDataModel
						{
							Quantity = 1,
							ItemType = RewardItemType.BATTLEGROUNDS_BATTLE_BASH_HAMMER,
							BattlegroundsBattleBashHammer = new BattlegroundsBattleBashHammerDataModel
							{
								NumHammers = m_luckyDrawManager.NumUnacknowledgedBonusHammers()
							}
						}
					}
				}
			};
			m_rewardPresenter.EnqueueReward(dataModel, delegate
			{
			});
			m_rewardPresenter.ShowNextReward(OnLuckyDrawPopupDismissed);
		}
	}

	private void OnLuckyDrawPopupDismissed()
	{
		m_luckyDrawManager.AcknowledgeAllHammers();
	}

	public void Open()
	{
		Shop.Get().RefreshDataModel();
	}

	public void BlockInterface(bool blocked)
	{
	}

	public bool IsReady()
	{
		return true;
	}

	public bool IsOpen()
	{
		return m_isStoreOpen;
	}

	public void Unload()
	{
	}

	public IEnumerable<CurrencyType> GetVisibleCurrencies()
	{
		if (ShopUtils.TryGetMainVirtualCurrencyType(out var vcType))
		{
			return new CurrencyType[1] { vcType };
		}
		return new CurrencyType[0];
	}
}
