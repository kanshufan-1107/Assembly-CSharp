using System.Collections;
using System.Collections.Generic;
using Assets;
using Blizzard.T5.Services;
using Game.Shop;
using Hearthstone;
using Hearthstone.Progression;
using Hearthstone.Store;
using Hearthstone.Streaming;
using Hearthstone.UI;
using UnityEngine;

public class CurrencyFrame : MonoBehaviour
{
	public enum State
	{
		ANIMATE_IN,
		ANIMATE_OUT,
		HIDDEN,
		SHOWN
	}

	public GameObject m_dustFX;

	public GameObject m_explodeFX_Common;

	public GameObject m_explodeFX_Rare;

	public GameObject m_explodeFX_Epic;

	public GameObject m_explodeFX_Legendary;

	[SerializeField]
	protected GameObject m_currencyIconContainer;

	[SerializeField]
	protected Clickable m_clickable;

	[SerializeField]
	protected Vector3 m_helperTipPopupOffsetPC;

	[SerializeField]
	protected Vector3 m_helperTipPopupOffsetMobile;

	private Widget m_widget;

	private State m_state = State.SHOWN;

	private bool m_isBlocked;

	private const float CURRENCY_FRAME_OFFSET_LOCAL_Y = -63f;

	private const float CURRENCY_FRAME_OFFSET_WORLD_Z = 7f;

	private Notification m_rechargeHelpPopup;

	public CurrencyType CurrentCurrencyType { get; private set; }

	public GameObject CurrencyIconContainer => m_currencyIconContainer;

	private void Awake()
	{
		m_widget = GetComponent<Widget>();
		m_widget.RegisterEventListener(OnWidgetEvent);
		if (m_clickable != null)
		{
			m_clickable.AddEventListener(UIEventType.ROLLOVER, OnFrameMouseOver);
			m_clickable.AddEventListener(UIEventType.ROLLOUT, OnFrameMouseOut);
		}
	}

	private void Start()
	{
		BnetBar bnetBar = BnetBar.Get();
		if (bnetBar != null)
		{
			bnetBar.RegisterCurrencyFrame(this);
		}
		Bind(CurrencyType.NONE);
	}

	public void Bind(CurrencyType currencyType)
	{
		CurrentCurrencyType = currencyType;
		if (Shop.Get() == null)
		{
			Hide(isImmediate: true);
			return;
		}
		TriggerEventParameters eventParams = new TriggerEventParameters(null, null, noDownwardPropagation: true);
		if (m_rechargeHelpPopup != null)
		{
			NotificationManager.Get().DestroyNotificationNowWithNoAnim(m_rechargeHelpPopup);
		}
		switch (currencyType)
		{
		case CurrencyType.CN_RUNESTONES:
		case CurrencyType.ROW_RUNESTONES:
			m_widget.TriggerEvent("VIRTUAL_CURRENCY", eventParams);
			break;
		case CurrencyType.CN_ARCANE_ORBS:
			m_widget.TriggerEvent("BOOSTER_CURRENCY", eventParams);
			break;
		case CurrencyType.GOLD:
			m_widget.TriggerEvent("GOLD", eventParams);
			break;
		case CurrencyType.DUST:
			m_widget.TriggerEvent("DUST", eventParams);
			break;
		case CurrencyType.BG_TOKEN:
			m_widget.TriggerEvent("BG_TOKEN", eventParams);
			if (AllowRecharge(currencyType))
			{
				m_widget.TriggerEvent("BG_TOKEN_RECHARGEABLE", eventParams);
			}
			else
			{
				m_widget.TriggerEvent("BG_TOKEN_NONRECHARGEABLE", eventParams);
			}
			break;
		case CurrencyType.RENOWN:
			m_widget.TriggerEvent("RENOWN", eventParams);
			if (!LettuceTutorialUtils.IsEventTypeComplete(LettuceTutorialVo.LettuceTutorialEvent.VILLAGE_TUTORIAL_RENOWN_POPUP))
			{
				Vector3 pos = base.transform.position;
				Notification.PopUpArrowDirection dir;
				float popupScale;
				if ((bool)UniversalInputManager.UsePhoneUI)
				{
					pos += m_helperTipPopupOffsetMobile;
					dir = Notification.PopUpArrowDirection.Up;
					popupScale = 0.9f;
				}
				else
				{
					pos += m_helperTipPopupOffsetPC;
					dir = Notification.PopUpArrowDirection.Down;
					popupScale = 1f;
				}
				m_rechargeHelpPopup = NotificationManager.Get().CreatePopupText(UserAttentionBlocker.NONE, Vector3.zero, TutorialEntity.GetTextScale() * popupScale, GameStrings.Get("GLUE_LETTUCE_RENOWN_CONVERSION_HELPER_POPUP"));
				m_rechargeHelpPopup.gameObject.transform.position = pos;
				m_rechargeHelpPopup.ShowPopUpArrow(dir);
				m_rechargeHelpPopup.PulseReminderEveryXSeconds(3f);
			}
			break;
		default:
			m_widget.TriggerEvent("NONE", eventParams);
			Hide(isImmediate: true);
			break;
		}
		bool fadeBackground = (SceneMgr.Get()?.GetMode() ?? SceneMgr.Mode.INVALID) != SceneMgr.Mode.HUB;
		m_widget.TriggerEvent(fadeBackground ? "FADE_BACKGROUND" : "SOLID_BACKGROUND");
	}

	public void Show(bool isImmediate = false)
	{
		if (m_isBlocked || CurrentCurrencyType == CurrencyType.NONE || m_state == State.SHOWN || m_state == State.ANIMATE_IN || (Shop.Get() == null && (GameMgr.Get() == null || !GameMgr.Get().IsBattlegrounds())) || GameDownloadManagerProvider.Get() == null || !GameDownloadManagerProvider.Get().IsReadyToPlay)
		{
			return;
		}
		if (DemoMgr.Get() != null && !DemoMgr.Get().IsCurrencyEnabled())
		{
			Hide(isImmediate: true);
			return;
		}
		if ((bool)UniversalInputManager.UsePhoneUI)
		{
			isImmediate = true;
		}
		base.gameObject.SetActive(value: true);
		m_state = State.ANIMATE_IN;
		Hashtable args = iTweenManager.Get().GetTweenHashTable();
		args.Add("amount", 1f);
		args.Add("delay", 0f);
		args.Add("time", isImmediate ? 0f : 0.25f);
		args.Add("easetype", iTween.EaseType.easeOutCubic);
		args.Add("oncomplete", "FinaliseShow");
		args.Add("oncompletetarget", base.gameObject);
		iTween.Stop(base.gameObject);
		iTween.FadeTo(base.gameObject, args);
	}

	public void Hide(bool isImmediate = false)
	{
		if (m_state != State.HIDDEN && m_state != State.ANIMATE_OUT)
		{
			m_state = State.ANIMATE_OUT;
			if (iTweenManager.Get() != null)
			{
				Hashtable args = iTweenManager.Get().GetTweenHashTable();
				args.Add("amount", 0f);
				args.Add("delay", 0f);
				args.Add("time", isImmediate ? 0f : 0.25f);
				args.Add("easetype", iTween.EaseType.easeOutCubic);
				args.Add("oncomplete", "FinaliseHide");
				args.Add("oncompletetarget", base.gameObject);
				iTween.Stop(base.gameObject);
				iTween.FadeTo(base.gameObject, args);
			}
		}
	}

	public void SetBlocked(bool isBlocked)
	{
		m_isBlocked = isBlocked;
		if (!isBlocked)
		{
			Bind(CurrentCurrencyType);
		}
	}

	public GameObject GetTooltipObject()
	{
		TooltipZone tooltip = GetComponent<TooltipZone>();
		if (tooltip != null)
		{
			return tooltip.GetTooltipObject();
		}
		return null;
	}

	public bool IsShown()
	{
		if (m_state != 0)
		{
			return m_state == State.SHOWN;
		}
		return true;
	}

	private void FinaliseShow()
	{
		iTween.Stop(base.gameObject, includechildren: true);
		m_state = State.SHOWN;
	}

	private void FinaliseHide()
	{
		iTween.Stop(base.gameObject, includechildren: true);
		base.gameObject.SetActive(value: false);
		m_state = State.HIDDEN;
	}

	private void OnFrameMouseOver(UIEvent e)
	{
		if (m_isBlocked)
		{
			return;
		}
		string header = "";
		string description = "";
		switch (CurrentCurrencyType)
		{
		case CurrencyType.DUST:
			header = "GLUE_CRAFTING_ARCANEDUST";
			description = "GLUE_CRAFTING_ARCANEDUST_DESCRIPTION";
			break;
		case CurrencyType.GOLD:
			header = "GLUE_TOOLTIP_GOLD_HEADER";
			description = "GLUE_TOOLTIP_GOLD_DESCRIPTION";
			break;
		case CurrencyType.CN_RUNESTONES:
			header = "GLUE_TOOLTIP_VIRTUAL_CURRENCY_HEADER";
			description = "GLUE_TOOLTIP_VIRTUAL_CURRENCY_DESCRIPTION";
			break;
		case CurrencyType.ROW_RUNESTONES:
			header = "GLUE_TOOLTIP_VIRTUAL_CURRENCY_HEADER";
			description = "GLUE_TOOLTIP_VIRTUAL_CURRENCY_ROW_DESCRIPTION";
			break;
		case CurrencyType.CN_ARCANE_ORBS:
			header = "GLUE_TOOLTIP_BOOSTER_CURRENCY_HEADER";
			description = "GLUE_TOOLTIP_BOOSTER_CURRENCY_DESCRIPTION";
			break;
		case CurrencyType.RENOWN:
			header = "GLUE_TOOLTIP_RENOWN_HEADER";
			description = "GLUE_TOOLTIP_RENOWN_DESCRIPTION";
			break;
		case CurrencyType.BG_TOKEN:
			header = "GLUE_TOOLTIP_BG_TOKEN_HEADER";
			description = "GLUE_TOOLTIP_BG_TOKEN_DESCRIPTION";
			break;
		}
		if (!(header == ""))
		{
			TooltipPanel tooltip = GetComponent<TooltipZone>().ShowTooltip(GameStrings.Get(header), GameStrings.Get(description), 0.7f);
			LayerUtils.SetLayer(tooltip.gameObject, GameLayer.BattleNet);
			tooltip.transform.localEulerAngles = new Vector3(270f, 0f, 0f);
			tooltip.transform.localScale = new Vector3(70f, 70f, 70f);
			if ((bool)UniversalInputManager.UsePhoneUI)
			{
				TransformUtil.SetPoint(tooltip, Anchor.TOP, m_clickable, Anchor.BOTTOM, Vector3.zero);
				TransformUtil.SetLocalPosY(tooltip, -63f);
			}
			else
			{
				TransformUtil.SetPoint(tooltip, Anchor.BOTTOM, m_clickable, Anchor.TOP, new Vector3(0f, 0f, 7f));
			}
		}
	}

	private void OnFrameMouseOut(UIEvent e)
	{
		GetComponent<TooltipZone>().HideTooltip();
	}

	private void OnWidgetEvent(string eventName)
	{
		if (eventName == "RECHARGE")
		{
			OnAttemptRecharge();
		}
	}

	private bool AllowRecharge(CurrencyType currencyType)
	{
		if (currencyType == CurrencyType.BG_TOKEN)
		{
			SceneMgr.Mode num = SceneMgr.Get()?.GetMode() ?? SceneMgr.Mode.INVALID;
			bool storeOpen = StoreManager.Get()?.GetCurrentStore()?.IsOpen() == true;
			if (num == SceneMgr.Mode.BACON && !storeOpen)
			{
				return true;
			}
		}
		return false;
	}

	private void OnAttemptRecharge()
	{
		if (m_isBlocked)
		{
			return;
		}
		if (ShopUtils.IsCurrencyVirtual(CurrentCurrencyType))
		{
			Shop shop = Shop.Get();
			if (shop == null || !ShopUtils.IsVirtualCurrencyEnabled())
			{
				return;
			}
			if (ShopUtils.IsMainVirtualCurrencyType(CurrentCurrencyType))
			{
				if (ServiceManager.TryGet<IProductDataService>(out var dataService) && VariantUtils.TryFindSpecialOfferVariant(dataService.VirtualCurrencyProductItem, out var preferredVariant))
				{
					shop.ProductPageController.OpenVirtualCurrencyPurchase(preferredVariant);
				}
				else
				{
					shop.ProductPageController.OpenVirtualCurrencyPurchase();
				}
			}
			else if (ShopUtils.IsBoosterVirtualCurrencyType(CurrentCurrencyType))
			{
				shop.ProductPageController.OpenBoosterCurrencyPurchase();
			}
		}
		else if (CurrentCurrencyType == CurrencyType.RENOWN)
		{
			if (PresenceMgr.Get().CurrentStatus != Global.PresenceStatus.MERCENARIES_VILLAGE_RENOWN_CONVERSION)
			{
				LettuceVillagePopupManager popupManager = LettuceVillagePopupManager.Get();
				if (popupManager != null)
				{
					popupManager.Show(LettuceVillagePopupManager.PopupType.RENOWNCONVERSION);
				}
			}
		}
		else if (CurrentCurrencyType == CurrencyType.BG_TOKEN && AllowRecharge(CurrencyType.BG_TOKEN))
		{
			if (Network.IsLoggedIn())
			{
				BaconDisplay.Get()?.OpenBattlegroundsShop("tokens");
			}
			else
			{
				ProgressUtils.ShowOfflinePopup();
			}
		}
	}

	public static IEnumerable<CurrencyType> GetVisibleCurrencies()
	{
		IStore currentStore = StoreManager.Get().GetCurrentStore();
		if (currentStore != null && currentStore.IsOpen())
		{
			return currentStore.GetVisibleCurrencies();
		}
		List<CurrencyType> currencies = new List<CurrencyType>();
		switch (SceneMgr.Get()?.GetMode() ?? SceneMgr.Mode.INVALID)
		{
		case SceneMgr.Mode.LOGIN:
		case SceneMgr.Mode.FATAL_ERROR:
		case SceneMgr.Mode.RESET:
			return currencies;
		case SceneMgr.Mode.HUB:
		{
			Shop shop = Shop.Get();
			if (shop != null && currentStore != null && shop.ProductPageController.CurrentProductPage != null)
			{
				return currentStore.GetVisibleCurrencies();
			}
			currencies.Add(CurrencyType.GOLD);
			break;
		}
		case SceneMgr.Mode.COLLECTIONMANAGER:
			currencies.Add(CurrencyType.DUST);
			break;
		case SceneMgr.Mode.TAVERN_BRAWL:
			if (!UniversalInputManager.UsePhoneUI)
			{
				TavernBrawlDisplay tavernBrawlDisplay = TavernBrawlDisplay.Get();
				if (tavernBrawlDisplay != null && tavernBrawlDisplay.IsInDeckEditMode())
				{
					currencies.Add(CurrencyType.DUST);
				}
				else
				{
					currencies.Add(CurrencyType.GOLD);
				}
			}
			break;
		case SceneMgr.Mode.LETTUCE_VILLAGE:
			if (StoreManager.Get().CurrentShopType == ShopType.MERCENARIES_WORKSHOP)
			{
				currencies.Add(CurrencyType.GOLD);
			}
			else if ((PresenceMgr.Get().CurrentStatus == Global.PresenceStatus.MERCENARIES_VILLAGE_TASKBOARD || PresenceMgr.Get().CurrentStatus == Global.PresenceStatus.MERCENARIES_VILLAGE_RENOWN_CONVERSION) && LettuceRenownUtil.HasUnlockedRenownOffers())
			{
				currencies.Add(CurrencyType.RENOWN);
			}
			break;
		case SceneMgr.Mode.LETTUCE_COLLECTION:
			if (LettuceRenownUtil.HasUnlockedRenownOffers())
			{
				currencies.Add(CurrencyType.RENOWN);
			}
			break;
		case SceneMgr.Mode.BACON:
			if (!UniversalInputManager.UsePhoneUI)
			{
				currencies.Add(CurrencyType.GOLD);
				currencies.Add(CurrencyType.BG_TOKEN);
			}
			break;
		case SceneMgr.Mode.GAMEPLAY:
			if (GameMgr.Get().IsBattlegrounds() && MulliganManager.Get() != null && MulliganManager.Get().IsMulliganActive())
			{
				currencies.Add(CurrencyType.BG_TOKEN);
			}
			break;
		}
		if ((bool)UniversalInputManager.UsePhoneUI)
		{
			currencies.Remove(CurrencyType.DUST);
		}
		GameSaveDataManager gsd = GameSaveDataManager.Get();
		if (gsd.IsDataReady(GameSaveKeyId.FTUE))
		{
			gsd.GetSubkeyValue(GameSaveKeyId.FTUE, GameSaveKeySubkeyId.FTUE_HAS_SEEN_GOLD, out long hasSeenGoldFlag);
			if (hasSeenGoldFlag == 0L)
			{
				if (!GameUtils.HasCompletedApprentice() && NetCache.Get().GetGoldBalance() == 0L)
				{
					currencies.Remove(CurrencyType.GOLD);
				}
				else
				{
					gsd.SaveSubkey(new GameSaveDataManager.SubkeySaveRequest(GameSaveKeyId.FTUE, GameSaveKeySubkeyId.FTUE_HAS_SEEN_GOLD, 1L));
				}
			}
		}
		return currencies;
	}
}
