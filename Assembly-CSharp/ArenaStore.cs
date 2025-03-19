using System.Linq;
using Blizzard.T5.Services;
using Hearthstone.Store;
using PegasusUtil;
using UnityEngine;

public class ArenaStore : Store
{
	public UIBButton m_backButton;

	public GameObject m_storeClosed;

	private static readonly int NUM_BUNDLE_ITEMS_REQUIRED = 1;

	private const long DRAFT_TICKET_PMT_ID = 327L;

	private NoGTAPPTransactionData m_goldTransactionData;

	private ProductInfo m_bundle;

	private static ArenaStore s_instance;

	protected override void Start()
	{
		base.Start();
		m_backButton.AddEventListener(UIEventType.RELEASE, OnBackPressed);
	}

	protected override void Awake()
	{
		s_instance = this;
		m_destroyOnSceneLoad = false;
		base.Awake();
		m_backButton.SetText(GameStrings.Get("GLOBAL_BACK"));
	}

	protected override void OnDestroy()
	{
		s_instance = null;
		Navigation.RemoveHandler(OnNavigateBack);
	}

	public static ArenaStore Get()
	{
		return s_instance;
	}

	public static ProductInfo GetDraftTicketProduct()
	{
		if (!ServiceManager.TryGet<IProductDataService>(out var dataService))
		{
			return null;
		}
		ProductInfo arenaTicket = dataService.EnumerateBundlesForProductType(ProductType.PRODUCT_TYPE_DRAFT, requireRealMoneyOption: false, 0, NUM_BUNDLE_ITEMS_REQUIRED).FirstOrDefault((ProductInfo b) => b.Id.Value == 327);
		if (arenaTicket != null)
		{
			Log.Store.PrintDebug("Arena Ticket Product found. PMT ID = {0}, Name = {1}", arenaTicket.Id, arenaTicket.Title);
			return arenaTicket;
		}
		Log.Store.PrintError("Arena Ticket Product not found!");
		return null;
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
		RefreshBuyButtonStates(m_bundle, m_goldTransactionData);
	}

	public override void OnGoldBalanceChanged(NetCache.NetCacheGoldBalance balance)
	{
		RefreshBuyButtonStates(m_bundle, m_goldTransactionData);
	}

	public override void OnCurrencyBalanceChanged(CurrencyBalanceChangedEventArgs args)
	{
		if (ShopUtils.IsCurrencyVirtual(args.Currency))
		{
			RefreshBuyButtonStates(m_bundle, m_goldTransactionData);
		}
	}

	protected override void ShowImpl(bool isTotallyFake)
	{
		m_shown = true;
		Navigation.Push(OnNavigateBack);
		StoreManager.Get().RegisterAuthorizationExitListener(OnAuthExit);
		EnableFullScreenEffects(enable: true);
		FindTicketProduct();
		SetUpBuyButtons();
		ShownUIMgr.Get().SetShownUI(ShownUIMgr.UI_WINDOW.ARENA_STORE);
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
		if (m_goldTransactionData != null)
		{
			FireBuyWithGoldEventNoGTAPP(m_goldTransactionData);
		}
	}

	protected override void BuyWithMoney(UIEvent e)
	{
		if (m_bundle == null)
		{
			Log.Store.PrintError("ArenaStore.BuyWithMoney failed. Arena ticket product not found");
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
			Log.Store.PrintError("ArenaStore.BuyWithVirtualCurrency failed. Arena ticket product not found");
		}
		else
		{
			FireBuyWithVirtualCurrencyEvent(m_bundle, m_bundle.GetFirstVirtualCurrencyPriceType());
		}
	}

	private void OnAuthExit()
	{
		Navigation.Pop();
		ExitForgeStore(authorizationBackButtonPressed: true);
	}

	private void OnBackPressed(UIEvent e)
	{
		Navigation.GoBack();
	}

	private bool OnNavigateBack()
	{
		ExitForgeStore(authorizationBackButtonPressed: false);
		return true;
	}

	private void ExitForgeStore(bool authorizationBackButtonPressed)
	{
		BlockInterface(blocked: false);
		LayerUtils.SetLayer(base.gameObject, GameLayer.Default);
		EnableFullScreenEffects(enable: false);
		StoreManager.Get().RemoveAuthorizationExitListener(OnAuthExit);
		FireExitEvent(authorizationBackButtonPressed);
	}

	private void SetUpBuyButtons()
	{
		SetUpBuyWithGoldButton();
		SetUpBuyWithMoneyButton();
		RefreshBuyButtonStates(m_bundle, m_goldTransactionData);
	}

	private void SetUpBuyWithGoldButton()
	{
		string buttonText = "";
		NoGTAPPTransactionData noGTAPPTransactionData = new NoGTAPPTransactionData
		{
			Product = ProductType.PRODUCT_TYPE_DRAFT,
			ProductData = 0,
			Quantity = 1
		};
		if (StoreManager.Get().GetGoldCostNoGTAPP(noGTAPPTransactionData, out var goldCost))
		{
			m_goldTransactionData = noGTAPPTransactionData;
			buttonText = goldCost.ToString();
		}
		else
		{
			Debug.LogWarning("ForgeStore.SetUpBuyWithGoldButton(): no gold price for purchase Arena without GTAPP");
			buttonText = GameStrings.Get("GLUE_STORE_PRODUCT_PRICE_NA");
		}
		m_buyWithGoldButton.SetText(buttonText);
	}

	private void FindTicketProduct()
	{
		m_bundle = GetDraftTicketProduct();
		if (m_bundle != null)
		{
			BindProductDataModel(m_bundle);
		}
	}

	private void SetUpBuyWithMoneyButton()
	{
		string buttonText = "";
		buttonText = ((m_bundle == null || !m_bundle.TryGetRMPriceInfo(out var priceInfo)) ? GameStrings.Get("GLUE_STORE_PRODUCT_PRICE_NA") : priceInfo.CurrentPrice.DisplayPrice);
		m_buyWithMoneyButton.SetText(buttonText);
	}
}
