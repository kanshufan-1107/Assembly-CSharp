using System;
using System.Collections.Generic;
using Assets;
using Blizzard.T5.Core.Utils;
using Blizzard.T5.MaterialService.Extensions;
using Hearthstone.DataModels;
using Hearthstone.Store;
using Hearthstone.UI;
using PegasusShared;
using PegasusUtil;
using UnityEngine;

public class TavernBrawlStore : Store
{
	public UIBButton m_ContinueButton;

	public UIBButton m_backButton;

	public GameObject m_storeClosed;

	public PlayMakerFSM m_ButtonFlipper;

	public PlayMakerFSM m_PaperEffect;

	public UberText m_EndsInTextPaper;

	public UberText m_EndsInTextChalk;

	public UberText m_ChalkboardTitleText;

	public UberText m_ChalkboardDescriptionText;

	public MeshRenderer m_ChalkboardMesh;

	private static readonly int NUM_BUNDLE_ITEMS_REQUIRED = 1;

	private ProductInfo m_bundle;

	private static TavernBrawlStore s_instance;

	private bool m_canCheckNoGTAPPTransactionData;

	private NoGTAPPTransactionData m_goldTransactionData;

	protected override void Start()
	{
		base.Start();
		m_ContinueButton.AddEventListener(UIEventType.RELEASE, OnContinuePressed);
		m_backButton.AddEventListener(UIEventType.RELEASE, OnBackPressed);
	}

	protected override void Awake()
	{
		s_instance = this;
		m_destroyOnSceneLoad = false;
		base.Awake();
		m_backButton.SetText(GameStrings.Get("GLOBAL_BACK"));
		m_infoButton.GetComponent<BoxCollider>().enabled = false;
	}

	protected override void OnDestroy()
	{
		s_instance = null;
	}

	public static TavernBrawlStore Get()
	{
		return s_instance;
	}

	public override void Hide()
	{
		if (ShownUIMgr.Get() != null)
		{
			ShownUIMgr.Get().ClearShownUI();
		}
		FriendChallengeMgr.Get().OnStoreClosed();
		StoreManager.Get().RemoveAuthorizationExitListener(OnAuthExit);
		Navigation.RemoveHandler(OnNavigateBack);
		EnableFullScreenEffects(enable: false);
		base.Hide();
	}

	public override void OnMoneySpent()
	{
		UpdateMoneyButtonState();
	}

	public override void OnGoldBalanceChanged(NetCache.NetCacheGoldBalance balance)
	{
		UpdateGoldButtonState(balance);
	}

	protected override void ShowImpl(bool isTotallyFake)
	{
		m_shown = true;
		Navigation.Push(OnNavigateBack);
		StoreManager.Get().RegisterAuthorizationExitListener(OnAuthExit);
		EnableFullScreenEffects(enable: true);
		ScenarioDbfRecord scenarioDbf = GameDbf.Scenario.GetRecord(TavernBrawlManager.Get().CurrentMission().missionId);
		m_ChalkboardTitleText.Text = scenarioDbf.Name;
		m_ChalkboardDescriptionText.Text = (((bool)UniversalInputManager.UsePhoneUI && !string.IsNullOrEmpty(scenarioDbf.ShortDescription)) ? scenarioDbf.ShortDescription : scenarioDbf.Description);
		string endingTimeText = TavernBrawlManager.Get().EndingTimeText;
		m_EndsInTextPaper.Text = endingTimeText;
		m_EndsInTextChalk.Text = endingTimeText;
		if (m_ChalkboardMesh != null)
		{
			Material chalkboardMeshMaterial = m_ChalkboardMesh.GetSharedMaterial();
			if (chalkboardMeshMaterial != null)
			{
				chalkboardMeshMaterial.SetTexture("_MainTex", TavernBrawlDisplay.Get().m_chalkboardTexture);
			}
		}
		BindTavernBrawlData();
		BindTicketProduct();
		SetUpBuyButtons();
		ShownUIMgr.Get().SetShownUI(ShownUIMgr.UI_WINDOW.TAVERN_BRAWL_STORE);
		FriendChallengeMgr.Get().OnStoreOpened();
		DoShowAnimation(delegate
		{
			if (isTotallyFake)
			{
				SilenceBuyButtons();
				m_infoButton.RemoveEventListener(UIEventType.RELEASE, base.OnInfoPressed);
			}
			FireOpenedEvent();
		});
	}

	protected override void BuyWithGold(UIEvent e)
	{
		if (m_bundle == null)
		{
			Log.Store.PrintError("TavernBrawlStore.BuyWithGold failed. Brawl ticket product not found");
		}
		else if (m_canCheckNoGTAPPTransactionData && m_goldTransactionData != null)
		{
			FireBuyWithGoldEventNoGTAPP(m_goldTransactionData);
		}
		else
		{
			FireBuyWithGoldEventGTAPP(m_bundle, 1);
		}
	}

	protected override void BuyWithMoney(UIEvent e)
	{
		if (m_bundle == null)
		{
			Log.Store.PrintError("TavernBrawlStore.BuyWithMoney failed. Brawl ticket product not found");
		}
		else
		{
			FireBuyWithMoneyEvent(m_bundle, 1);
		}
	}

	protected override void BuyWithVirtualCurrency(UIEvent e)
	{
		if (m_bundle == null)
		{
			Log.Store.PrintError("TavernBrawlStore.BuyWithVirtualCurrency failed. Brawl ticket product not found");
		}
		else
		{
			FireBuyWithVirtualCurrencyEvent(m_bundle, m_bundle.GetFirstVirtualCurrencyPriceType());
		}
	}

	private void OnAuthExit()
	{
		Navigation.Pop();
		ExitTavernBrawlStore(authorizationBackButtonPressed: true);
	}

	private void OnBackPressed(UIEvent e)
	{
		Navigation.GoBack();
	}

	private void OnContinuePressed(UIEvent e)
	{
		m_ButtonFlipper.SendEvent("Flip");
		m_PaperEffect.SendEvent("BurnAway");
		m_ContinueButton.SetEnabled(enabled: false);
		SetUpBuyButtons();
		m_infoButton.GetComponent<BoxCollider>().enabled = true;
		_ = TavernBrawlManager.Get().CurrentSession.SessionCount;
		_ = TavernBrawlManager.Get().CurrentMission().FreeSessions;
		if (TavernBrawlManager.Get().IsEligibleForFreeTicket())
		{
			SetMoneyButtonState(BuyButtonState.DISABLED);
			SetGoldButtonState(BuyButtonState.DISABLED);
			AlertPopup.PopupInfo info = new AlertPopup.PopupInfo();
			info.m_headerText = GameStrings.Get("GLOBAL_BRAWLISEUM");
			info.m_text = GameStrings.Get("GLUE_BRAWLISEUM_FREE_TICKET_BODY");
			info.m_responseDisplay = AlertPopup.ResponseDisplay.OK;
			info.m_alertTextAlignment = UberText.AlignmentOptions.Center;
			info.m_responseCallback = OnFreePopupClosed;
			DialogManager.Get().ShowPopup(info);
		}
		else
		{
			TavernBrawlMission currentMission = TavernBrawlManager.Get().CurrentMission();
			int ticketType = currentMission.ticketType;
			if (GameDbf.TavernBrawlTicket.GetRecord(ticketType).TicketBehaviorType == TavernBrawlTicket.TicketBehaviorType.ARENA_TAVERN_TICKET && NetCache.Get().GetNetObject<NetCache.NetPlayerArenaTickets>().Balance >= currentMission.ticketAmount)
			{
				TavernBrawlManager.Get().RequestSessionBegin();
			}
		}
	}

	private void OnFreePopupClosed(AlertPopup.Response response, object userData)
	{
		TavernBrawlManager.Get().RequestSessionBegin();
	}

	private bool OnNavigateBack()
	{
		ExitTavernBrawlStore(authorizationBackButtonPressed: false);
		return true;
	}

	private void ExitTavernBrawlStore(bool authorizationBackButtonPressed)
	{
		BlockInterface(blocked: false);
		LayerUtils.SetLayer(base.gameObject, GameLayer.Default);
		EnableFullScreenEffects(enable: false);
		StoreManager.Get().RemoveAuthorizationExitListener(OnAuthExit);
		FireExitEvent(authorizationBackButtonPressed);
	}

	private void UpdateMoneyButtonState()
	{
		BuyButtonState buyButtonState = BuyButtonState.ENABLED;
		if (m_bundle == null || !StoreManager.Get().IsOpen())
		{
			buyButtonState = BuyButtonState.DISABLED;
			m_storeClosed.SetActive(value: true);
		}
		else if (!StoreManager.Get().IsBattlePayFeatureEnabled())
		{
			buyButtonState = BuyButtonState.DISABLED_FEATURE;
		}
		else if (StoreManager.Get().IsPromptShowing)
		{
			buyButtonState = BuyButtonState.DISABLED;
			SetGoldButtonState(buyButtonState);
		}
		else
		{
			m_storeClosed.SetActive(value: false);
		}
		SetMoneyButtonState(buyButtonState);
	}

	private void UpdateGoldButtonState(NetCache.NetCacheGoldBalance balance)
	{
		BuyButtonState buyButtonState = BuyButtonState.ENABLED;
		long goldTotal = 0L;
		if (StoreManager.Get().IsPromptShowing)
		{
			buyButtonState = BuyButtonState.DISABLED;
			SetMoneyButtonState(buyButtonState);
		}
		else if (m_bundle == null)
		{
			buyButtonState = BuyButtonState.DISABLED;
		}
		else if (!StoreManager.Get().IsOpen())
		{
			buyButtonState = BuyButtonState.DISABLED;
		}
		else if (!StoreManager.Get().IsBuyWithGoldFeatureEnabled())
		{
			buyButtonState = BuyButtonState.DISABLED_FEATURE;
		}
		else if (m_canCheckNoGTAPPTransactionData && m_goldTransactionData != null)
		{
			StoreManager.Get().GetGoldCostNoGTAPP(m_goldTransactionData, out goldTotal);
		}
		else if (goldTotal == 0L && (!m_bundle.TryGetVCPrice(CurrencyType.GOLD, out goldTotal) || !m_bundle.IsBundleAvailableNow(StoreManager.Get())))
		{
			buyButtonState = BuyButtonState.DISABLED_NO_TOOLTIP;
		}
		else if (balance == null)
		{
			buyButtonState = BuyButtonState.DISABLED;
		}
		if (buyButtonState == BuyButtonState.ENABLED && balance.GetTotal() < goldTotal)
		{
			buyButtonState = BuyButtonState.DISABLED_NOT_ENOUGH_GOLD;
		}
		SetGoldButtonState(buyButtonState);
	}

	private void BindTavernBrawlData()
	{
		WidgetTemplate widget = GameObjectUtils.GetComponentOnSelfOrParent<WidgetTemplate>(base.transform);
		if (widget != null)
		{
			ScenarioDbfRecord scenarioDbf = GameDbf.Scenario.GetRecord(TavernBrawlManager.Get().CurrentMission().missionId);
			TavernBrawlMission mission = TavernBrawlManager.Get().GetMission(BrawlType.BRAWL_TYPE_TAVERN_BRAWL);
			TavernBrawlDetailsDataModel model = new TavernBrawlDetailsDataModel
			{
				BrawlType = mission.BrawlType,
				BrawlMode = mission.brawlMode,
				FormatType = mission.formatType,
				TicketType = mission.ticketType,
				MaxWins = mission.maxWins,
				MaxLosses = mission.maxLosses,
				PopupType = mission.tavernBrawlSpec.StorePopupType,
				Title = scenarioDbf.Name,
				RulesDesc = (((bool)UniversalInputManager.UsePhoneUI && !string.IsNullOrEmpty(scenarioDbf.ShortDescription)) ? scenarioDbf.ShortDescription : scenarioDbf.Description),
				RewardDesc = mission.tavernBrawlSpec.RewardDesc,
				MinRewardDesc = mission.tavernBrawlSpec.MinRewardDesc,
				MaxRewardDesc = mission.tavernBrawlSpec.MaxRewardDesc,
				EndConditionDesc = mission.tavernBrawlSpec.EndConditionDesc
			};
			widget.BindDataModel(model);
		}
	}

	private void BindTicketProduct()
	{
		m_canCheckNoGTAPPTransactionData = false;
		m_goldTransactionData = null;
		TavernBrawlMission tavernBrawlMission = TavernBrawlManager.Get().CurrentMission();
		int ticketType = tavernBrawlMission.ticketType;
		int ticketAmount = tavernBrawlMission.ticketAmount;
		int ticketsRequired = ticketAmount;
		TavernBrawlTicketDbfRecord ticketRecord = GameDbf.TavernBrawlTicket.GetRecord(ticketType);
		List<ProductInfo> bundles = new List<ProductInfo>();
		if (ticketRecord != null)
		{
			switch (ticketRecord.TicketBehaviorType)
			{
			case TavernBrawlTicket.TicketBehaviorType.DEFAULT:
			{
				bundles = new List<ProductInfo>(StoreManager.Get().GetAvailableBundlesForProduct(ProductType.PRODUCT_TYPE_TAVERN_BRAWL_TICKET, requireNonGoldPriceOption: true, ticketType, NUM_BUNDLE_ITEMS_REQUIRED));
				int currentBrawlTicketsOwned = TavernBrawlManager.Get().NumTicketsOwned;
				ticketsRequired = Math.Max(ticketsRequired - currentBrawlTicketsOwned, 0);
				break;
			}
			case TavernBrawlTicket.TicketBehaviorType.ARENA_TAVERN_TICKET:
			{
				m_canCheckNoGTAPPTransactionData = true;
				bundles = new List<ProductInfo>(StoreManager.Get().GetAvailableBundlesForProduct(ProductType.PRODUCT_TYPE_DRAFT, requireNonGoldPriceOption: true, 0, NUM_BUNDLE_ITEMS_REQUIRED));
				int currentDraftTicketsOwned = NetCache.Get().GetNetObject<NetCache.NetPlayerArenaTickets>().Balance;
				ticketsRequired = Math.Max(ticketsRequired - currentDraftTicketsOwned, 0);
				break;
			}
			default:
				Log.TavernBrawl.PrintError("Ticket type " + ticketRecord.NoteDesc + " has unknown ticket behavior type TicketBehaviorType");
				break;
			}
		}
		bundles.RemoveAll((ProductInfo productInfo) => productInfo.Items.Count == 0 || productInfo.Items[0].Quantity != ticketsRequired);
		if (bundles.Count == 0)
		{
			m_bundle = null;
			return;
		}
		m_bundle = bundles[0];
		BindProductDataModel(m_bundle);
		if (!m_canCheckNoGTAPPTransactionData || m_bundle.HasCurrency(CurrencyType.GOLD))
		{
			return;
		}
		m_goldTransactionData = new NoGTAPPTransactionData
		{
			Product = ProductType.PRODUCT_TYPE_DRAFT,
			ProductData = 0,
			Quantity = ticketsRequired
		};
		WidgetTemplate widget = GameObjectUtils.GetComponentOnSelfOrParent<WidgetTemplate>(base.transform);
		if (!(widget != null))
		{
			return;
		}
		ProductDataModel productDataModel = widget.GetDataModel<ProductDataModel>();
		bool hasGoldAlready = false;
		foreach (PriceDataModel price in productDataModel.Prices)
		{
			if (price.Currency == CurrencyType.GOLD)
			{
				hasGoldAlready = true;
				break;
			}
		}
		if (!hasGoldAlready && StoreManager.Get().GetGoldCostNoGTAPP(m_goldTransactionData, out var goldCost))
		{
			productDataModel.Prices.Add(new PriceDataModel
			{
				Currency = CurrencyType.GOLD,
				Amount = goldCost,
				DisplayText = goldCost.ToString(),
				OriginalAmount = goldCost,
				OriginalDisplayText = goldCost.ToString(),
				OnSale = m_bundle.IsOnSale()
			});
		}
	}

	private void SetUpBuyButtons()
	{
		SetUpBuyWithGoldButton();
		SetUpBuyWithMoneyButton();
		SetVCButtonState(BuyButtonState.ENABLED);
	}

	private void SetUpBuyWithGoldButton()
	{
		if (m_bundle != null && (m_bundle.HasCurrency(CurrencyType.GOLD) || m_canCheckNoGTAPPTransactionData))
		{
			NetCache.NetCacheGoldBalance netGold = NetCache.Get().GetNetObject<NetCache.NetCacheGoldBalance>();
			UpdateGoldButtonState(netGold);
		}
		else
		{
			Debug.LogWarningFormat("TavernBrawlStore.SetUpBuyWithGoldButton(): no gold cost (bundle={0} hasGoldCost=<no value>)", (m_bundle == null) ? "<null>" : "<not null>");
			SetGoldButtonState(BuyButtonState.DISABLED);
		}
	}

	private void SetUpBuyWithMoneyButton()
	{
		if (m_bundle != null)
		{
			UpdateMoneyButtonState();
			return;
		}
		Debug.LogWarning("TavernBrawlStore.SetUpBuyWithMoneyButton(): m_bundle is null");
		SetMoneyButtonState(BuyButtonState.DISABLED);
	}
}
