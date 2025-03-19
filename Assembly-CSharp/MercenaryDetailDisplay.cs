using System;
using System.Collections;
using System.Collections.Generic;
using Assets;
using Hearthstone;
using Hearthstone.DataModels;
using Hearthstone.UI;
using Hearthstone.UI.Core;
using PegasusLettuce;
using UnityEngine;

public class MercenaryDetailDisplay : MonoBehaviour
{
	public delegate void OnHideDelegate();

	public const string SHOW_EVENT_FULL = "SHOW_FULL";

	public const string SHOW_EVENT_PARTIAL = "SHOW_PARTIAL";

	[CustomEditField(Sections = "Widgets")]
	public WidgetTemplate m_widgetTemplate;

	[CustomEditField(Sections = "Widgets")]
	public AsyncReference m_abilityUpgradePopupReference;

	[CustomEditField(Sections = "Widgets")]
	public AsyncReference m_abilityInfoPopupReference;

	[CustomEditField(Sections = "Widgets")]
	public AsyncReference m_popupHandlerReference;

	[CustomEditField(Sections = "Widgets")]
	public AsyncReference m_equipmentExplanationPopupReference;

	[CustomEditField(Sections = "Widgets")]
	public AsyncReference m_abilityUpgradeCardReference;

	[CustomEditField(Sections = "Widgets")]
	public AsyncReference m_mercenariesListReference;

	[CustomEditField(Sections = "Widgets")]
	public AsyncReference m_mythicTreasureInfoPopupReference;

	[CustomEditField(Sections = "Objects")]
	public TooltipZone m_tooltipZone;

	[CustomEditField(Sections = "Objects")]
	public GameObject m_equipmentSlotCollider;

	[CustomEditField(Sections = "Bones")]
	public List<Transform> m_showLoadEquipmentInSlotTutorialBones;

	[CustomEditField(Sections = "Bones")]
	public Transform m_upgradeAbilityTutorialBone;

	[CustomEditField(Sections = "Bones")]
	public Transform m_AppearanceMercTutorialBone;

	[CustomEditField(Sections = "Bones")]
	public Transform m_AppearanceTutorialBone;

	[CustomEditField(Sections = "Bones")]
	public Transform m_showUnlockMythicBone;

	[CustomEditField(Sections = "Settings")]
	public float m_secondsDelayBeforeTutorialPopups = 1f;

	protected VisualController m_mercDetailsDisplayVisualController;

	protected VisualController m_abilityUpgradePopupVisualController;

	protected Widget m_abilityInfoPopupWidget;

	protected Widget m_equipmentExplanationPopupWidget;

	protected VisualController m_popupHandlerVisualController;

	protected Hearthstone.UI.Card m_abilityUpgradeCard;

	protected MaterialDataModel m_abilityMaterialData = new MaterialDataModel();

	protected VisualController m_mercenaryListVisualController;

	protected Widget m_mythicTreasureInfoPopup;

	protected GameObject m_currentInfoPopup;

	protected Notification m_helpPopup;

	protected Option m_helpPopupType;

	protected LettuceAbilityDataModel m_currentlyDisplayedAbility;

	protected LettuceAbilityDataModel m_draggingEquipmentDataModel;

	protected List<MercenaryAcknowledgeData> m_mercenaryAcknowledgements = new List<MercenaryAcknowledgeData>();

	protected LettuceTeam m_currentTeam;

	protected int m_mercIdBeingViewed = -1;

	protected const string STOP_DRAG_EQUIPMENT_EVENT = "STOP_DRAG_EQUIPMENT";

	protected const string STOP_DRAG_EQUIPMENT_SLOTTED_EVENT = "STOP_DRAG_EQUIPMENT_SLOTTED";

	protected const string START_DRAG_EQUIPMENT_EVENT = "START_DRAG_EQUIPMENT";

	protected const float TUTORIAL_PULSE_RATE = 3f;

	protected Coroutine m_tutorialCoroutine;

	protected bool m_isTreasureTrayOpen;

	protected HashSet<string> m_blockTooltipRequests = new HashSet<string>();

	protected string DetailPopupBlockId = "DETAIL_POPUP";

	protected string VillagePopupBlockId = "VILLAGE_POPUP";

	protected static MercenaryDetailDisplay s_instance;

	private readonly List<OnHideDelegate> m_onHideCallbacks = new List<OnHideDelegate>();

	[CustomEditField(Sections = "Settings")]
	[Overridable]
	public bool EnableMythicMode { get; set; } = true;

	protected bool IsTutorialPopupBlocked => m_blockTooltipRequests.Count > 0;

	public bool DisplayVisible => m_mercIdBeingViewed != -1;

	protected virtual void Start()
	{
		m_mercDetailsDisplayVisualController = base.gameObject.GetComponent<VisualController>();
		m_mercDetailsDisplayVisualController.GetComponent<Widget>().RegisterEventListener(MercDetailsEventListener);
		m_abilityUpgradePopupReference.RegisterReadyListener<VisualController>(OnAbilityUpgradePopupReady);
		m_abilityInfoPopupReference.RegisterReadyListener(delegate(Widget w)
		{
			m_abilityInfoPopupWidget = w;
		});
		m_popupHandlerReference.RegisterReadyListener(delegate(VisualController vc)
		{
			m_popupHandlerVisualController = vc;
		});
		m_equipmentExplanationPopupReference.RegisterReadyListener<Widget>(OnEquipmentExplanationPopupReady);
		m_abilityUpgradeCardReference.RegisterReadyListener(delegate(Hearthstone.UI.Card card)
		{
			m_abilityUpgradeCard = card;
		});
		m_mercenariesListReference.RegisterReadyListener<VisualController>(OnMercenaryListReady);
		m_mythicTreasureInfoPopupReference.RegisterReadyListener<Widget>(OnMythicTreasureInfoPopupReady);
		Network network = Network.Get();
		network.RegisterNetHandler(UpgradeMercenaryAbilityResponse.PacketID.ID, OnAbilityUpgradeNetworkResponse);
		network.RegisterNetHandler(UpgradeMercenaryEquipmentResponse.PacketID.ID, OnEquipmentUpgradeNetworkResponse);
		network.RegisterNetHandler(CraftMercenaryEquipmentResponse.PacketID.ID, OnCraftEquipmentNetworkResponse);
		InputMgr.Get().OnDropMercenariesModeCard += OnDropMercenariesModeCard;
		CollectionManager.Get().MercenaryArtVariationChangedEvent += OnMercenaryArtVariationChanged;
		s_instance = this;
	}

	public virtual void Unload()
	{
		if (s_instance == this)
		{
			s_instance = null;
		}
		InputMgr inputManager = InputMgr.Get();
		if (inputManager != null)
		{
			inputManager.OnDropMercenariesModeCard -= OnDropMercenariesModeCard;
		}
		CollectionManager collectionMgr = CollectionManager.Get();
		if (collectionMgr != null)
		{
			collectionMgr.MercenaryArtVariationChangedEvent -= OnMercenaryArtVariationChanged;
		}
		Network network = Network.Get();
		if (network != null)
		{
			network.RemoveNetHandler(UpgradeMercenaryAbilityResponse.PacketID.ID, OnAbilityUpgradeNetworkResponse);
			network.RemoveNetHandler(UpgradeMercenaryEquipmentResponse.PacketID.ID, OnEquipmentUpgradeNetworkResponse);
			network.RemoveNetHandler(CraftMercenaryEquipmentResponse.PacketID.ID, OnCraftEquipmentNetworkResponse);
			Network.Get().RemoveNetHandler(MercenariesCollectionAcknowledgeResponse.PacketID.ID, OnCollectionAcknowledgeResponse);
		}
	}

	public void OnDestroy()
	{
		Unload();
		HideHelpPopups();
	}

	public virtual void Show(LettuceMercenary merc, string showEvent = "SHOW_FULL", LettuceTeam editingTeam = null)
	{
		if (!(m_mercDetailsDisplayVisualController == null) && merc != null && m_mercIdBeingViewed != merc.ID)
		{
			m_equipmentSlotCollider.SetActive(value: false);
			if (editingTeam != null)
			{
				m_currentTeam = editingTeam;
				CollectionManager.Get().SetEditingTeam(editingTeam);
			}
			m_mercIdBeingViewed = merc.ID;
			SetupActiveMercDataModel(GetMercenaryDisplayDataModel(), merc);
			ShowRequiredTutorialIfNeeded();
			LettuceVillagePopupManager popupManager = LettuceVillagePopupManager.Get();
			if (popupManager != null)
			{
				popupManager.OnPopupShown = (Action<LettuceVillagePopupManager.PopupType>)Delegate.Combine(popupManager.OnPopupShown, new Action<LettuceVillagePopupManager.PopupType>(OnVillagePopupShown));
				popupManager.OnPopupClosed = (Action<LettuceVillagePopupManager.PopupType>)Delegate.Combine(popupManager.OnPopupClosed, new Action<LettuceVillagePopupManager.PopupType>(OnVillagePopupClosed));
			}
			m_mercDetailsDisplayVisualController.OwningWidget.TriggerEvent(showEvent, TriggerEventParameters.Standard);
			MercenaryInputMgr.Get().MouseOverTargetEvaluator = IsMouseOverEquipmentSlot;
		}
	}

	public virtual void Hide()
	{
		AcknowledgeAbilityorEquipment(0, acknowledgeAll: true);
		SendAcknowledgements();
		MercenaryInputMgr.Get().MouseOverTargetEvaluator = null;
		OnHide();
		if (m_currentTeam != null)
		{
			m_currentTeam = null;
			CollectionManager.Get().ClearEditingTeam();
		}
		if (!(m_mercDetailsDisplayVisualController == null))
		{
			m_mercDetailsDisplayVisualController.OwningWidget.TriggerEvent("HIDE_FULL", TriggerEventParameters.Standard);
			m_mercIdBeingViewed = -1;
			HideHelpPopups();
			if (LettuceVillagePopupManager.Get() != null)
			{
				LettuceVillagePopupManager lettuceVillagePopupManager = LettuceVillagePopupManager.Get();
				lettuceVillagePopupManager.OnPopupShown = (Action<LettuceVillagePopupManager.PopupType>)Delegate.Remove(lettuceVillagePopupManager.OnPopupShown, new Action<LettuceVillagePopupManager.PopupType>(OnVillagePopupShown));
				LettuceVillagePopupManager lettuceVillagePopupManager2 = LettuceVillagePopupManager.Get();
				lettuceVillagePopupManager2.OnPopupClosed = (Action<LettuceVillagePopupManager.PopupType>)Delegate.Remove(lettuceVillagePopupManager2.OnPopupClosed, new Action<LettuceVillagePopupManager.PopupType>(OnVillagePopupClosed));
			}
		}
	}

	public void HideHelpPopups()
	{
		if (m_tutorialCoroutine != null)
		{
			StopCoroutine(m_tutorialCoroutine);
			m_tutorialCoroutine = null;
		}
		if (m_helpPopup != null)
		{
			NotificationManager.Get()?.DestroyNotificationNowWithNoAnim(m_helpPopup);
		}
		m_helpPopupType = Option.INVALID;
	}

	public void UpdateMercenaryData(LettuceMercenary merc)
	{
		LettuceMercenary currentMerc = GetCurrentlyDisplayedMercenary();
		if (merc != null && currentMerc != null && currentMerc.ID == merc.ID)
		{
			LettuceMercenaryDataModel mercData = GetMercenaryDisplayDataModel();
			SetupActiveMercDataModel(mercData, merc);
		}
	}

	public static MercenaryDetailDisplay Get()
	{
		return s_instance;
	}

	protected void OnAbilityUpgradePopupReady(VisualController visualController)
	{
		m_abilityUpgradePopupVisualController = visualController;
		m_abilityUpgradePopupVisualController.BindDataModel(m_abilityMaterialData);
	}

	protected void OnEquipmentExplanationPopupReady(Widget widget)
	{
		m_equipmentExplanationPopupWidget = widget;
	}

	protected void OnMercenaryListReady(VisualController visualController)
	{
		m_mercenaryListVisualController = visualController;
		m_mercenaryListVisualController.SetState("SHOW");
	}

	protected void OnMythicTreasureInfoPopupReady(Widget popup)
	{
		m_mythicTreasureInfoPopup = popup;
	}

	protected virtual void MercDetailsEventListener(string eventName)
	{
		switch (eventName)
		{
		case "MERC_LOADOUT_RELEASED":
			if (WidgetUtils.GetEventDataModel(m_mercenaryListVisualController).Payload is LettuceMercenaryDataModel mercenaryDataModel)
			{
				Show(CollectionManager.Get().GetMercenary(mercenaryDataModel.MercenaryId), "SHOW_PARTIAL", m_currentTeam);
			}
			break;
		case "BACK_BUTTON_PRESSED":
			Hide();
			break;
		case "ABILITY_UPGRADE_code":
			OnUpgradeAbility();
			break;
		case "POPUP_ACTIVATED_code":
			BlockTutorialPopups(isBlocked: true, DetailPopupBlockId);
			break;
		case "ABILITY_HOVERED_code":
		{
			LettuceAbilityDataModel abilityDataModel = WidgetUtils.GetEventDataModel(m_mercDetailsDisplayVisualController).Payload as LettuceAbilityDataModel;
			AcknowledgeAbilityorEquipment(abilityDataModel.AbilityId);
			break;
		}
		case "ABILITY_drag_started":
			OnAbilityDragStart();
			break;
		case "LOADOUT_ABILITY_drag_started":
			OnEquipmentLoadoutDragStart();
			break;
		case "ABILITY_CLICKED_code":
			OnAbilityClicked();
			break;
		case "MERC_COIN_OVER":
			ShowCoinTooltip();
			break;
		case "MERC_COIN_OUT":
			HideCoinTooltip();
			break;
		case "POPUP_DEACTIVATED_code":
			if (!ShowFullyUpgradeMercIfNeeded())
			{
				BlockTutorialPopups(isBlocked: false, DetailPopupBlockId);
			}
			break;
		case "ABILITY_INFO_POPUP_REVEAL_COMPLETED_code":
			ShowExplanationPopup();
			break;
		case "APPEARANCE_SELECTED_code":
			OnAppearanceClicked();
			break;
		case "MERCENARY_released":
			OnAppearanceResetPage();
			break;
		case "APPEARANCE_SHOW_code":
			ShowAppearancePart2TutorialIfNeeded();
			break;
		case "APPEARANCE_HIDE_code":
			AcknowledgeArtVariation(0, TAG_PREMIUM.NORMAL, acknowledgeAll: true);
			HideHelpPopups();
			break;
		case "ART_VARIATION_PREV_code":
			OnAppearanceArrowClicked(-1);
			break;
		case "ART_VARIATION_NEXT_code":
			OnAppearanceArrowClicked(1);
			break;
		case "OnUninspectMerc":
			Hide();
			break;
		case "ART_VARIATION_HOVERED_code":
		{
			LettuceMercenaryArtVariationDataModel portraitDataModel = WidgetUtils.GetEventDataModel(m_mercDetailsDisplayVisualController).Payload as LettuceMercenaryArtVariationDataModel;
			AcknowledgeArtVariation(portraitDataModel.ArtVariationId, portraitDataModel.Card.Premium);
			break;
		}
		case "MYTHIC_TOGGLE_code":
			OnMythicToggle();
			break;
		case "TREASURE_TRAY_OPEN_code":
			m_isTreasureTrayOpen = true;
			HideHelpPopups();
			ShowRequiredTutorialIfNeeded();
			break;
		case "TREASURE_TRAY_CLOSE_code":
			m_isTreasureTrayOpen = false;
			HideHelpPopups();
			ShowRequiredTutorialIfNeeded();
			break;
		case "OPEN_MYTHIC_TREASURE_INFO_POPUP_REQUEST":
			m_widgetTemplate.TriggerEvent("OPEN_MYTHIC_TREASURE_INFO_POPUP");
			break;
		case "OPEN_MYTHIC_TREASURE_INFO_POPUP":
			m_currentInfoPopup = UIContext.GetRoot().ShowPopup(m_mythicTreasureInfoPopup.gameObject).gameObject;
			BnetBar.Get().SetBlockCurrencyFrames(isBlocked: true);
			break;
		case "SEEN_MYTHIC_TREASURE_INFO_POPUP":
			if (LettuceTutorialUtils.IsEventTypeComplete(LettuceTutorialVo.LettuceTutorialEvent.VILLAGE_TUTORIAL_VISIT_MYTHIC_TREASURE_START) && !LettuceTutorialUtils.IsEventTypeComplete(LettuceTutorialVo.LettuceTutorialEvent.VILLAGE_TUTORIAL_VISIT_MYTHIC_TREASURE_END))
			{
				LettuceTutorialUtils.FireEvent(LettuceTutorialVo.LettuceTutorialEvent.VILLAGE_TUTORIAL_VISIT_MYTHIC_TREASURE_END, base.gameObject);
			}
			break;
		case "HIDE_COMPLETE":
			UIContext.GetRoot().DismissPopup(m_currentInfoPopup);
			m_currentInfoPopup = null;
			BnetBar.Get().SetBlockCurrencyFrames(isBlocked: false);
			break;
		}
	}

	protected void OnMythicToggle()
	{
		LettuceMercenaryDataModel mercenaryDataModel = GetMercenaryDisplayDataModel();
		if (mercenaryDataModel != null)
		{
			ToggleMythicToggle();
			SetupActiveMercDataModel(mercenaryDataModel, GetCurrentlyDisplayedMercenary());
		}
	}

	protected void HideHoverCards()
	{
		LettuceCollectionDisplay lcd = CollectionManager.Get().GetCollectibleDisplay() as LettuceCollectionDisplay;
		if (lcd == null)
		{
			Log.Lettuce.PrintWarning("MercenaryDetailDisplay.HideHoverCards - LettuceCollectionDisplay is null!");
		}
		else
		{
			lcd.HideHoverCards();
		}
	}

	public void SlotSelectedEquipment(string cardId)
	{
		LettuceMercenaryDataModel mercData = GetMercenaryDisplayDataModel();
		LettuceMercenary collectionMerc = CollectionManager.Get().GetMercenary(mercData.MercenaryId);
		if (collectionMerc == null)
		{
			return;
		}
		LettuceAbility selectedEquipment = collectionMerc.GetLettuceEquipment(cardId);
		if (selectedEquipment == null)
		{
			Log.Lettuce.PrintError("MercenaryDetailDisplay.SlotSelectedEquipment - equipment with card ID {0} not found on Mercenary ID {1}", cardId, collectionMerc.ID);
			return;
		}
		collectionMerc.SlotEquipment(selectedEquipment.ID);
		if (collectionMerc.m_equipmentSelectionChanged)
		{
			CollectionManager.Get().SendEquippedMercenaryEquipment(collectionMerc.ID);
		}
		m_equipmentSlotCollider.SetActive(value: false);
		if (m_helpPopupType == Option.HAS_SEEN_LOAD_EQUIPMENT_IN_SLOT_TUTORIAL)
		{
			HideHelpPopups();
			Options.Get().SetBool(Option.HAS_SEEN_LOAD_EQUIPMENT_IN_SLOT_TUTORIAL, val: true);
		}
		SetupActiveMercDataModel(mercData, collectionMerc);
		m_mercDetailsDisplayVisualController.OwningWidget.TriggerEvent("STOP_DRAG_EQUIPMENT_SLOTTED");
	}

	protected virtual void OnEquipmentLoadoutDragStart()
	{
		HideHoverCards();
		EventDataModel eventDataModel = WidgetUtils.GetEventDataModel(m_mercDetailsDisplayVisualController);
		if (eventDataModel == null)
		{
			Log.Lettuce.PrintError("No event data model attached to the LettuceAbilitySlot");
			return;
		}
		LettuceAbilityDataModel equipmentData = (LettuceAbilityDataModel)eventDataModel.Payload;
		if (equipmentData == null)
		{
			return;
		}
		LettuceMercenary collectionMerc = CollectionManager.Get().GetMercenary(GetMercenaryDisplayDataModel().MercenaryId);
		if (collectionMerc == null)
		{
			return;
		}
		LettuceAbility grabbedEquipment = collectionMerc.GetLettuceEquipment(equipmentData.AbilityId);
		if (CanPickUpAbility(collectionMerc, grabbedEquipment))
		{
			m_equipmentSlotCollider.SetActive(value: true);
			InputMgr.Get().GrabMercenariesModeCard(equipmentData, grabbedEquipment.m_cardType, OnEquipmentDropped);
			if (collectionMerc.CanUnslotEquipment(equipmentData.AbilityId))
			{
				m_draggingEquipmentDataModel = equipmentData;
			}
			m_mercDetailsDisplayVisualController.OwningWidget.TriggerEvent("START_DRAG_EQUIPMENT");
		}
	}

	protected void OnAbilityClicked()
	{
		EventDataModel eventDataModel = WidgetUtils.GetEventDataModel(m_mercDetailsDisplayVisualController);
		if (eventDataModel == null)
		{
			Log.Lettuce.PrintError("MercenaryDetailDisplay.OnAbilityClicked - no event data model attached to clicked ability!");
		}
		else
		{
			OnPreMythicAbilityClicked(eventDataModel);
		}
	}

	protected void OnPreMythicAbilityClicked(EventDataModel eventDataModel)
	{
		if (!(eventDataModel.Payload is LettuceAbilityDataModel abilityData))
		{
			return;
		}
		m_currentlyDisplayedAbility = abilityData;
		LettuceMercenaryDataModel mercenaryDataModel = GetMercenaryDisplayDataModel();
		if (mercenaryDataModel == null)
		{
			Log.Lettuce.PrintError("OnPreMythicAbilityClicked - failed to get mercenary data model!");
		}
		else
		{
			if (MythicUpgradeDisplay.ShouldBeHandledByMythic(mercenaryDataModel, abilityData))
			{
				return;
			}
			LettuceMercenary parentMercenary = CollectionManager.Get().GetMercenary(mercenaryDataModel.MercenaryId);
			if (parentMercenary == null)
			{
				Log.Lettuce.PrintError($"OnPreMythicAbilityClicked - could not load mercenary with id {mercenaryDataModel.MercenaryId}");
				return;
			}
			if (ShouldShowAbilityInfoPopup(abilityData, parentMercenary))
			{
				LettuceAbilityDataModel boundData = (abilityData.IsEquipment ? abilityData.CloneDataModel() : abilityData);
				if (boundData.IsEquipment)
				{
					boundData.IsEquipped = false;
				}
				m_abilityInfoPopupWidget.BindDataModel(boundData);
				m_popupHandlerVisualController.SetState("SHOW_ABILITY_INFO_POPUP");
				return;
			}
			LettuceAbilityUpgradeDisplayDataModel upgradeData = new LettuceAbilityUpgradeDisplayDataModel();
			upgradeData.CurrentTierAbility = abilityData.CloneDataModel();
			upgradeData.NextTierAbility = abilityData.CloneDataModel();
			int nextTier = upgradeData.NextTierAbility.CurrentTier + 1;
			upgradeData.NextTierAbility.CurrentTier = nextTier;
			CardDataModel cardDataModel = abilityData.AbilityTiers[nextTier - 1].AbilityTierCard;
			if (cardDataModel != null)
			{
				EntityDef entityDef = DefLoader.Get().GetEntityDef(cardDataModel.CardId);
				upgradeData.IsMinion = entityDef.HasTag(GAME_TAG.LETTUCE_ABILITY_SUMMONED_MINION);
				CollectionUtils.PopulateCardNameData(entityDef, cardDataModel);
			}
			upgradeData.CurrentTierAbility.IsEquipped = false;
			upgradeData.NextTierAbility.IsEquipped = false;
			if (abilityData.AbilityTiers.Count < upgradeData.CurrentTierAbility.CurrentTier || abilityData.AbilityTiers.Count < upgradeData.NextTierAbility.CurrentTier)
			{
				Log.Lettuce.PrintError($"MercenaryDetailDisplay.OnAbilityClicked - current tier {upgradeData.CurrentTierAbility.CurrentTier} or {upgradeData.NextTierAbility.CurrentTier}" + $" are greater than the number of tiers {abilityData.AbilityTiers.Count}");
				return;
			}
			upgradeData.NextTierAbilityChanges = CollectionUtils.PopulateAbilityModifiedValues(abilityData.AbilityTiers[upgradeData.CurrentTierAbility.CurrentTier - 1].AbilityTierCard?.CardId, abilityData.AbilityTiers[upgradeData.NextTierAbility.CurrentTier - 1].AbilityTierCard?.CardId);
			m_abilityUpgradePopupVisualController.Owner.BindDataModel(upgradeData);
			m_popupHandlerVisualController.SetState("SHOW_ABILITY_UPGRADE_POPUP");
		}
	}

	protected bool ShouldShowAbilityInfoPopup(LettuceAbilityDataModel abilityData, LettuceMercenary parentMercenary)
	{
		if (abilityData.IsEquipment)
		{
			if (abilityData.Owned)
			{
				return abilityData.CurrentTier >= abilityData.MaxTier;
			}
			return true;
		}
		if (parentMercenary.m_level >= abilityData.UnlockLevel)
		{
			return abilityData.CurrentTier >= abilityData.MaxTier;
		}
		return true;
	}

	protected void ShowCoinTooltip()
	{
		if (m_tooltipZone != null)
		{
			m_tooltipZone.ShowTooltip(GameStrings.Get("GLUE_LETTUCE_COIN_TOOLTIP_HEADER"), GameStrings.Format("GLUE_LETTUCE_COIN_TOOLTIP_BODY"), 4f);
		}
	}

	protected void HideCoinTooltip()
	{
		if (m_tooltipZone != null)
		{
			m_tooltipZone.HideTooltip();
		}
	}

	protected void OnUpgradeAbility()
	{
		EventDataModel eventDataModel = WidgetUtils.GetEventDataModel(m_mercDetailsDisplayVisualController);
		if (eventDataModel == null)
		{
			Log.Lettuce.PrintError("No event data model attached to the LettuceMercDetailAbilityUpdragePopup");
			return;
		}
		LettuceAbilityDataModel abilityData = (LettuceAbilityDataModel)eventDataModel.Payload;
		if (abilityData == null)
		{
			return;
		}
		AlertPopup.PopupInfo info = new AlertPopup.PopupInfo();
		if (!abilityData.ReadyForUpgrade)
		{
			info.m_headerText = GameStrings.Get("GLUE_LETTUCE_ABILITY_UPGRADE_NOT_ENOUGH_COINS_HEADER");
			info.m_text = GameStrings.Get("GLUE_LETTUCE_ABILITY_UPGRADE_NOT_ENOUGH_COINS_BODY");
			info.m_showAlertIcon = true;
			info.m_alertTextAlignmentAnchor = UberText.AnchorOptions.Middle;
			info.m_responseDisplay = AlertPopup.ResponseDisplay.OK;
		}
		else
		{
			info.m_headerText = GameStrings.Get("GLUE_LETTUCE_ABILITY_UPGRADE_CONFIRMATION_HEADER");
			if (abilityData.IsEquipment)
			{
				info.m_text = GameStrings.Get("GLUE_LETTUCE_EQUIPMENT_UPGRADE_CONFIRMATION_BODY");
			}
			else
			{
				info.m_text = GameStrings.Get("GLUE_LETTUCE_ABILITY_UPGRADE_CONFIRMATION_BODY");
			}
			info.m_showAlertIcon = false;
			info.m_alertTextAlignmentAnchor = UberText.AnchorOptions.Middle;
			info.m_responseDisplay = AlertPopup.ResponseDisplay.CONFIRM_CANCEL;
			info.m_confirmText = GameStrings.Get("GLOBAL_BUTTON_YES");
			info.m_cancelText = GameStrings.Get("GLOBAL_BUTTON_NO");
			info.m_responseCallback = OnAbilityUpgradePopupResponse;
			info.m_responseUserData = abilityData;
		}
		DialogManager.Get().ShowPopup(info);
	}

	protected void OnAbilityUpgradePopupResponse(AlertPopup.Response response, object userData)
	{
		if (response == AlertPopup.Response.CANCEL || !(userData is LettuceAbilityDataModel abilityData))
		{
			return;
		}
		LettuceMercenary merc = GetCurrentlyDisplayedMercenary();
		if (merc == null)
		{
			Log.Lettuce.PrintWarning("MercenaryDetailDisplay.OnAbilityUpgradePopupResponse - no currently displayed mercenary!");
			return;
		}
		LettuceAbility ability = (abilityData.IsEquipment ? merc.GetLettuceEquipment(abilityData.AbilityId) : merc.GetLettuceAbility(abilityData.AbilityId));
		if (ability == null)
		{
			Log.Lettuce.PrintWarning("MercenaryDetailDisplay.OnAbilityUpgradePopupResponse - No ability found on merc {0} for ability Id {1}.", merc.ID, abilityData.AbilityId);
			return;
		}
		uint desiredTier = (uint)(abilityData.CurrentTier + 1);
		if (ability.m_tier != abilityData.CurrentTier)
		{
			Log.Lettuce.PrintWarning("MercenaryDetailDisplay.OnAbilityUpgradePopupResponse - Ability {0} is currently tier {1}, and cannot upgrade to tier {2}", ability.ID, ability.m_tier, desiredTier);
			return;
		}
		if (merc.m_currencyAmount < ability.GetNextUpgradeCost())
		{
			Log.Lettuce.PrintWarning("MercenaryDetailDisplay.OnAbilityUpgradePopupResponse - Ability {0} requires {1} coins to upgrade to tier {2}, but only has {3} coins.", ability.ID, ability.GetNextUpgradeCost(), ability.m_tier + 1, merc.m_currencyAmount);
			return;
		}
		if (Network.IsLoggedIn())
		{
			if (ability.m_cardType == CollectionUtils.MercenariesModeCardType.Ability)
			{
				Network.Get().UpgradeMercenaryAbility(merc.ID, ability.ID, desiredTier);
			}
			else
			{
				Network.Get().UpgradeMercenaryEquipment(merc.ID, ability.ID, desiredTier);
			}
		}
		m_abilityMaterialData.Material = m_abilityUpgradeCard.CardActor.GetPortraitMaterial();
		if (m_abilityUpgradePopupVisualController != null)
		{
			m_abilityUpgradePopupVisualController.OwningWidget.TriggerEvent("PLAY_ANIMATION");
		}
	}

	protected virtual void OnAbilityUpgradeNetworkResponse()
	{
		UpgradeMercenaryAbilityResponse response = Network.Get().UpgradeMercenaryAbilityResponse();
		if (response.ErrorCode != 0)
		{
			Log.Lettuce.PrintError("MercenaryDetailDisplay.OnAbilityUpgradeNetworkResponse() - Error upgrading ability: {0} for ability {1} on mercenary {2}", response.ErrorCode, response.AbilityId, response.MercenaryId);
			return;
		}
		LettuceMercenary merc = CollectionManager.Get().GetMercenary(response.MercenaryId);
		if (merc == null)
		{
			return;
		}
		LettuceAbility ability = merc.GetLettuceAbility(response.AbilityId);
		if (ability != null)
		{
			UpdateDataModelsAfterTransaction(ability, merc);
			if (!LettuceTutorialUtils.IsEventTypeComplete(LettuceTutorialVo.LettuceTutorialEvent.VILLAGE_TUTORIAL_UPGRADE_ABILITY_END))
			{
				EventFunctions.TriggerEvent(base.transform, "SHOW_TUTORIAL_DONE_BUTTON_HIGHLIGHT");
				LettuceTutorialUtils.FireEvent(LettuceTutorialVo.LettuceTutorialEvent.VILLAGE_TUTORIAL_UPGRADE_ABILITY_END, base.gameObject);
			}
		}
	}

	protected void OnEquipmentUpgradeNetworkResponse()
	{
		UpgradeMercenaryEquipmentResponse response = Network.Get().UpgradeMercenaryEquipmentResponse();
		if (response.ErrorCode != 0)
		{
			Log.Lettuce.PrintError("MercenaryDetailDisplay.OnEquipmentUpgradeNetworkResponse() - Error upgrading equipment: {0} for equipment {1} on mercenary {2}", response.ErrorCode, response.EquipmentId, response.MercenaryId);
			return;
		}
		LettuceMercenary merc = CollectionManager.Get().GetMercenary(response.MercenaryId);
		if (merc == null)
		{
			Log.Lettuce.PrintWarning("MercenaryDetailDisplay.OnEquipmentUpgradeNetworkResponse - No mercenary found with Id {0}.", response.MercenaryId);
			return;
		}
		LettuceAbility equipment = merc.GetLettuceEquipment(response.EquipmentId);
		if (equipment == null)
		{
			Log.Lettuce.PrintWarning("MercenaryDetailDisplay.OnEquipmentUpgradeNetworkResponse - No ability found on mercenary {0} with equipment Id {1}.", response.MercenaryId, response.EquipmentId);
		}
		else
		{
			UpdateDataModelsAfterTransaction(equipment, merc);
		}
	}

	protected virtual LettuceMercenaryDataModel UpdateDataModelsAfterTransaction(LettuceAbility ability, LettuceMercenary merc)
	{
		LettuceMercenaryDataModel mercenaryDisplayDataModel = GetMercenaryDisplayDataModel();
		MercenariesDataUtil.UpdateMercenaryDataModelWithNewData(mercenaryDisplayDataModel, ability, merc);
		return mercenaryDisplayDataModel;
	}

	public LettuceMercenary GetCurrentlyDisplayedMercenary()
	{
		if (!DisplayVisible)
		{
			return null;
		}
		return CollectionManager.Get().GetMercenary(m_mercIdBeingViewed);
	}

	public bool IsMouseOverEquipmentSlot()
	{
		RaycastHit hit;
		return UniversalInputManager.Get().ForcedUnblockableInputIsOver(Camera.main, m_equipmentSlotCollider, out hit);
	}

	public virtual bool ShowFullyUpgradeMercIfNeeded()
	{
		return PopupDisplayManager.Get().RewardPopups.ShowMercenariesFullyUpgraded(delegate
		{
			LettuceMercenaryDataModel mercenaryDisplayDataModel = GetMercenaryDisplayDataModel();
			LettuceMercenary mercenary = CollectionManager.Get().GetMercenary(mercenaryDisplayDataModel.MercenaryId);
			SetupActiveMercDataModel(mercenaryDisplayDataModel, mercenary);
			BlockTutorialPopups(isBlocked: false, DetailPopupBlockId);
			BnetBar.Get().RefreshCurrency();
		});
	}

	public void ShowExplanationPopup()
	{
		if (m_currentlyDisplayedAbility == null || !m_currentlyDisplayedAbility.IsEquipment)
		{
			return;
		}
		CollectionManager lcm = CollectionManager.Get();
		if (!lcm.GetHasOpenedDetailsDisplay() && !Options.Get().GetBool(Option.HAS_UNLOCKED_FIRST_EQUIPMENT))
		{
			if ((bool)m_equipmentExplanationPopupWidget)
			{
				lcm.SetHasVisitedDetailsDisplayTrue();
				m_equipmentExplanationPopupWidget.TriggerEvent("SHOW");
			}
			else
			{
				Log.Lettuce.PrintWarning("MercenaryDetailDisplay.ShowExplanationPopup - Equipment explanation popup widget not loaded on request");
			}
		}
	}

	protected void OnAppearanceClicked()
	{
		EventDataModel eventDataModel = WidgetUtils.GetEventDataModel(m_mercDetailsDisplayVisualController);
		if (eventDataModel == null)
		{
			Log.Lettuce.PrintError("MercenaryDetailDisplay.OnAppearanceClicked - no event data model attached to clicked ability!");
			return;
		}
		LettuceMercenaryArtVariationDataModel choice = (LettuceMercenaryArtVariationDataModel)eventDataModel.Payload;
		if (choice == null)
		{
			Log.Lettuce.PrintError("MercenaryDetailDisplay.OnAppearanceClicked - data model attached to appearance was not correct type!");
			return;
		}
		LettuceMercenaryDataModel mercDataModel = GetMercenaryDisplayDataModel();
		for (int i = 0; i < mercDataModel.ArtVariationList.Count; i++)
		{
			LettuceMercenaryArtVariationDataModel lettuceMercenaryArtVariationDataModel = mercDataModel.ArtVariationList[i];
			lettuceMercenaryArtVariationDataModel.Selected = lettuceMercenaryArtVariationDataModel == choice;
		}
		CollectionManager.Get().SendSelectedMercenaryArtVariation(mercDataModel.MercenaryId, choice.ArtVariationId, choice.Card.Premium);
		if (!Options.Get().GetBool(Option.HAS_SEEN_MERC_APPEARANCE_TUTORIAL, defaultVal: false))
		{
			HideHelpPopups();
			Options.Get().SetBool(Option.HAS_SEEN_MERC_APPEARANCE_TUTORIAL, val: true);
		}
	}

	protected void OnAppearanceResetPage()
	{
		GetMercenaryDisplayDataModel().ArtVariationPageIndex = 0;
	}

	protected void OnAppearanceArrowClicked(int dir)
	{
		LettuceMercenaryDataModel lettuceMercenary = GetMercenaryDisplayDataModel();
		int newIndex = lettuceMercenary.ArtVariationPageIndex + dir;
		if (newIndex < 0)
		{
			newIndex = 0;
		}
		else if (newIndex >= lettuceMercenary.ArtVariationPageList.Count)
		{
			newIndex = lettuceMercenary.ArtVariationPageList.Count - 1;
		}
		lettuceMercenary.ArtVariationPageIndex = newIndex;
	}

	public LettuceMercenaryDataModel GetMercenaryDisplayDataModel()
	{
		if (m_mercDetailsDisplayVisualController == null)
		{
			Log.Lettuce.PrintError("MercenaryDetailDisplay.GetMercenaryDisplayDataModel - Missing required VisualController reference");
			return null;
		}
		Widget owner = m_mercDetailsDisplayVisualController.Owner;
		if (!owner.GetDataModel(216, out var dataModel))
		{
			dataModel = MercenaryFactory.CreateEmptyMercenaryDataModel();
			owner.BindDataModel(dataModel);
		}
		return dataModel as LettuceMercenaryDataModel;
	}

	public void RegisterOnHideEvent(OnHideDelegate callback)
	{
		if (!m_onHideCallbacks.Contains(callback))
		{
			m_onHideCallbacks.Add(callback);
		}
	}

	public void UnregisterOnHideEvent(OnHideDelegate callback)
	{
		m_onHideCallbacks.Remove(callback);
	}

	private void OnHide()
	{
		OnHideDelegate[] array = m_onHideCallbacks.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			array[i]();
		}
	}

	protected void OnAbilityDragStart()
	{
		HideHoverCards();
		EventDataModel eventDataModel = WidgetUtils.GetEventDataModel(m_mercDetailsDisplayVisualController);
		if (eventDataModel == null)
		{
			Log.Lettuce.PrintError("No event data model attached to LettuceMercDetailDisplay");
			return;
		}
		LettuceAbilityDataModel abilityData = (LettuceAbilityDataModel)eventDataModel.Payload;
		LettuceMercenary merc = CollectionManager.Get().GetMercenary(m_mercIdBeingViewed);
		LettuceAbility ability = (abilityData.IsEquipment ? merc.GetLettuceEquipment(abilityData.AbilityId) : merc.GetLettuceAbility(abilityData.AbilityId));
		if (CanPickUpAbility(merc, ability))
		{
			m_equipmentSlotCollider.SetActive(value: true);
			InputMgr.Get().GrabMercenariesModeCard(abilityData, ability.m_cardType, OnEquipmentDropped);
			m_mercDetailsDisplayVisualController.OwningWidget.TriggerEvent("START_DRAG_EQUIPMENT");
		}
	}

	protected virtual bool CanPickUpAbility(LettuceMercenary merc, LettuceAbility ability)
	{
		if (m_mercIdBeingViewed == -1)
		{
			return true;
		}
		if (ability.m_cardType != CollectionUtils.MercenariesModeCardType.Equipment)
		{
			return false;
		}
		return ability.Owned;
	}

	protected void OnEquipmentDropped()
	{
		if (m_draggingEquipmentDataModel != null)
		{
			LettuceMercenaryDataModel mercData = GetMercenaryDisplayDataModel();
			LettuceMercenary collectionMerc = CollectionManager.Get().GetMercenary(mercData.MercenaryId);
			if (collectionMerc.UnslotEquipment(m_draggingEquipmentDataModel.AbilityId))
			{
				m_draggingEquipmentDataModel.IsEquipped = false;
				CollectionManager.Get().SendEquippedMercenaryEquipment(collectionMerc.ID);
				SetupActiveMercDataModel(mercData, collectionMerc);
			}
		}
		m_equipmentSlotCollider.SetActive(value: false);
		m_mercDetailsDisplayVisualController.OwningWidget.TriggerEvent("STOP_DRAG_EQUIPMENT");
		m_draggingEquipmentDataModel = null;
	}

	protected void SetupActiveMercDataModel(LettuceMercenaryDataModel mercData, LettuceMercenary collectionMerc)
	{
		CollectionUtils.MercenaryDataPopluateExtra populateOptions = CollectionUtils.MercenaryDataPopluateExtra.Abilities | CollectionUtils.MercenaryDataPopluateExtra.Coin | CollectionUtils.MercenaryDataPopluateExtra.Appearances | CollectionUtils.MercenaryDataPopluateExtra.UpdateValuesWithSlottedEquipment | CollectionUtils.MercenaryDataPopluateExtra.ShowMercCardText;
		if (EnableMythicMode && IsMythicFeatureEnabled() && collectionMerc.IsMythicUpgradable())
		{
			mercData.MythicToggle = GetMythicToggle();
			mercData.MythicToggleEnable = true;
			if (mercData.MythicToggle)
			{
				populateOptions |= CollectionUtils.MercenaryDataPopluateExtra.MythicAll;
				mercData.IsMythicFirstUnlock = !Options.Get().GetBool(Option.HAS_SEEN_MYTHIC_UNLOCK, defaultVal: false);
			}
		}
		else
		{
			mercData.MythicToggleEnable = false;
		}
		CollectionUtils.PopulateMercenaryDataModel(mercData, collectionMerc, populateOptions);
		mercData.MercenarySelected = true;
		mercData.ChildUpgradeAvailable = false;
	}

	protected void OnCraftEquipmentNetworkResponse()
	{
		CraftMercenaryEquipmentResponse response = Network.Get().CraftMercenaryEquipmentResponse();
		if (response.ErrorCode != 0)
		{
			Log.Lettuce.PrintError("MercenaryDetailDisplay.OnCraftEquipmentNetworkResponse - Error Code {0} crafting equipment ID {1} on mercenary ID {2}", response.ErrorCode, response.EquipmentId, response.MercenaryId);
			return;
		}
		LettuceMercenary merc = CollectionManager.Get().GetMercenary(response.MercenaryId);
		if (merc == null)
		{
			Log.Lettuce.PrintWarning("MercenaryDetailDisplay.OnCraftEquipmentNetworkResponse - No mercenary found with ID {0}.", response.MercenaryId);
			return;
		}
		LettuceAbility equipment = merc.GetLettuceEquipment(response.EquipmentId);
		if (equipment == null)
		{
			Log.Lettuce.PrintWarning("MercenaryDetailDisplay.OnCraftEquipmentNetworkResponse - No equipment found with ID {0} on Mercenary ID {1}.", response.EquipmentId, response.MercenaryId);
		}
		else
		{
			UpdateDataModelsAfterTransaction(equipment, merc);
		}
	}

	protected bool GetMythicToggle()
	{
		if (GameSaveDataManager.Get().GetSubkeyValue(GameSaveKeyId.MERCENARIES, GameSaveKeySubkeyId.MERCENARIES_MYTHIC_TOGGLE, out long value))
		{
			return value != 0;
		}
		return true;
	}

	protected bool ToggleMythicToggle()
	{
		bool newValue = !GetMythicToggle();
		bool result = GameSaveDataManager.Get().SaveSubkey(new GameSaveDataManager.SubkeySaveRequest(GameSaveKeyId.MERCENARIES, GameSaveKeySubkeyId.MERCENARIES_MYTHIC_TOGGLE, newValue ? 1 : 0));
		ShowRequiredTutorialIfNeeded();
		return result;
	}

	protected bool TutorialShouldShowAbilityUpgrade()
	{
		LettuceCollectionDisplay lcd = CollectionManager.Get().GetCollectibleDisplay() as LettuceCollectionDisplay;
		if (!(lcd != null))
		{
			return false;
		}
		return lcd.TutorialShouldShowAbilityUpgrade();
	}

	protected void ShowRequiredTutorialIfNeeded()
	{
		if (IsTutorialPopupBlocked)
		{
			return;
		}
		LettuceMercenary merc = GetCurrentlyDisplayedMercenary();
		if (m_isTreasureTrayOpen)
		{
			if (GetMythicToggle() && LettuceTutorialUtils.IsEventTypeComplete(LettuceTutorialVo.LettuceTutorialEvent.VILLAGE_TUTORIAL_VISIT_MYTHIC_TREASURE_START) && !LettuceTutorialUtils.IsEventTypeComplete(LettuceTutorialVo.LettuceTutorialEvent.VILLAGE_TUTORIAL_VISIT_MYTHIC_TREASURE_END) && merc.m_isFullyUpgraded)
			{
				m_widgetTemplate.TriggerEvent("OPEN_MYTHIC_TREASURE_INFO_POPUP");
			}
			return;
		}
		if (GetMythicToggle() && LettuceTutorialUtils.IsEventTypeComplete(LettuceTutorialVo.LettuceTutorialEvent.VILLAGE_TUTORIAL_VISIT_MYTHIC_TREASURE_START) && !LettuceTutorialUtils.IsEventTypeComplete(LettuceTutorialVo.LettuceTutorialEvent.VILLAGE_TUTORIAL_VISIT_MYTHIC_TREASURE_END) && merc.m_isFullyUpgraded)
		{
			HideHelpPopups();
			m_tutorialCoroutine = StartCoroutine(ShowUnlockMythicWhenReady());
			return;
		}
		if (!Options.Get().GetBool(Option.HAS_SEEN_MERC_APPEARANCE_TUTORIAL, defaultVal: false) && UserAttentionManager.CanShowAttentionGrabber("MercenaryDetailDisplay.ShowAppearanceTutorialIfNeeded:" + Option.HAS_SEEN_MERC_APPEARANCE_TUTORIAL) && merc != null && (long)merc.ID == 18 && merc.HasUnlockedGoldenOrBetter())
		{
			HideHelpPopups();
			m_helpPopupType = Option.HAS_SEEN_MERC_APPEARANCE_TUTORIAL;
			m_tutorialCoroutine = StartCoroutine(ShowAppearancePart1TutorialWhenReady());
			return;
		}
		bool canAbilityBeUpgraded = merc?.CanAnyAbilityBeUpgraded() ?? false;
		if (canAbilityBeUpgraded || (merc != null && (long)merc.ID == 69))
		{
			LettuceTutorialUtils.FireEvent(LettuceTutorialVo.LettuceTutorialEvent.VILLAGE_TUTORIAL_UPGRADE_ABILITY_START, base.gameObject);
		}
		if (TutorialShouldShowAbilityUpgrade() && canAbilityBeUpgraded && UserAttentionManager.CanShowAttentionGrabber("MercenaryDetailDisplay.ShowEquipmentSlotTutorialIfNeeded:HAS_SEEN_ABILITY_UPGRADE"))
		{
			HideHelpPopups();
			m_tutorialCoroutine = StartCoroutine(ShowUpgradeAbilityTutorialWhenReady(merc));
		}
		else if (!Options.Get().GetBool(Option.HAS_SEEN_LOAD_EQUIPMENT_IN_SLOT_TUTORIAL, defaultVal: false) && UserAttentionManager.CanShowAttentionGrabber("MercenaryDetailDisplay.ShowEquipmentSlotTutorialIfNeeded:" + Option.HAS_SEEN_LOAD_EQUIPMENT_IN_SLOT_TUTORIAL))
		{
			HideHelpPopups();
			m_helpPopupType = Option.HAS_SEEN_LOAD_EQUIPMENT_IN_SLOT_TUTORIAL;
			m_tutorialCoroutine = StartCoroutine(ShowEquipmentSlotTutorialWhenReady());
		}
	}

	protected void ShowAppearancePart2TutorialIfNeeded()
	{
		if (!Options.Get().GetBool(Option.HAS_SEEN_MERC_APPEARANCE_TUTORIAL, defaultVal: false) && UserAttentionManager.CanShowAttentionGrabber("MercenaryDetailDisplay.ShowAppearanceTutorialIfNeeded:" + Option.HAS_SEEN_MERC_APPEARANCE_TUTORIAL) && (long)GetCurrentlyDisplayedMercenary().ID == 18 && CollectionManager.Get().GetMercenary(18L).HasUnlockedGoldenOrBetter())
		{
			HideHelpPopups();
			m_helpPopupType = Option.HAS_SEEN_MERC_APPEARANCE_TUTORIAL;
			m_tutorialCoroutine = StartCoroutine(ShowAppearancePart2TutorialWhenReady());
		}
	}

	protected void BlockTutorialPopups(bool isBlocked, string blockId)
	{
		if (isBlocked)
		{
			if (m_blockTooltipRequests.Add(blockId) && m_blockTooltipRequests.Count == 1)
			{
				HideHelpPopups();
			}
		}
		else if (m_blockTooltipRequests.Remove(blockId) && m_blockTooltipRequests.Count == 0)
		{
			ShowRequiredTutorialIfNeeded();
		}
	}

	protected IEnumerator ShowEquipmentSlotTutorialWhenReady()
	{
		yield return new WaitForSeconds(m_secondsDelayBeforeTutorialPopups);
		LettuceMercenaryDataModel mercData = GetMercenaryDisplayDataModel();
		if (mercData != null)
		{
			int firstOwnedEquipmentIndex = CollectionUtils.GetFirstOwnedEquipmentIndex(mercData);
			if (firstOwnedEquipmentIndex != -1)
			{
				m_helpPopup = NotificationManager.Get().CreatePopupText(UserAttentionBlocker.NONE, m_showLoadEquipmentInSlotTutorialBones[firstOwnedEquipmentIndex].position, m_showLoadEquipmentInSlotTutorialBones[firstOwnedEquipmentIndex].localScale, GameStrings.Get("GLUE_LETTUCE_COLLECTION_TUTORIAL02"));
				m_helpPopup.ShowPopUpArrow(Notification.PopUpArrowDirection.Down);
				m_helpPopup.PulseReminderEveryXSeconds(3f);
			}
		}
	}

	protected IEnumerator ShowUpgradeAbilityTutorialWhenReady(LettuceMercenary merc)
	{
		yield return new WaitForSeconds(m_secondsDelayBeforeTutorialPopups);
		m_helpPopup = NotificationManager.Get().CreatePopupText(UserAttentionBlocker.NONE, m_upgradeAbilityTutorialBone.position, m_upgradeAbilityTutorialBone.localScale, GameStrings.Get("GLUE_LETTUCE_COLLECTION_UPGRADE_ABILITY_TOOLTIP"));
		for (int i = 0; i < merc.m_abilityList.Count; i++)
		{
			if (merc.IsCardReadyForUpgrade(merc.m_abilityList[i]))
			{
				switch (i)
				{
				case 0:
					m_helpPopup.ShowPopUpArrow(Notification.PopUpArrowDirection.LeftUp);
					break;
				case 1:
					m_helpPopup.ShowPopUpArrow(Notification.PopUpArrowDirection.Up);
					break;
				case 2:
					m_helpPopup.ShowPopUpArrow(Notification.PopUpArrowDirection.RightUp);
					break;
				}
			}
		}
		m_helpPopup.PulseReminderEveryXSeconds(3f);
	}

	protected IEnumerator ShowAppearancePart1TutorialWhenReady()
	{
		yield return new WaitForSeconds(m_secondsDelayBeforeTutorialPopups);
		m_helpPopup = NotificationManager.Get().CreatePopupText(UserAttentionBlocker.NONE, m_AppearanceMercTutorialBone.position, m_AppearanceMercTutorialBone.localScale, GameStrings.Get("GLUE_LETTUCE_COLLECTION_TUTORIAL_PORTRAIT_02"));
		m_helpPopup.ShowPopUpArrow(Notification.PopUpArrowDirection.Left);
		m_helpPopup.PulseReminderEveryXSeconds(3f);
	}

	protected IEnumerator ShowAppearancePart2TutorialWhenReady()
	{
		yield return new WaitForSeconds(m_secondsDelayBeforeTutorialPopups);
		m_helpPopup = NotificationManager.Get().CreatePopupText(UserAttentionBlocker.NONE, m_AppearanceTutorialBone.position, m_AppearanceMercTutorialBone.localScale, GameStrings.Get("GLUE_LETTUCE_COLLECTION_TUTORIAL_PORTRAIT_03"));
		m_helpPopup.ShowPopUpArrow(Notification.PopUpArrowDirection.Up);
		m_helpPopup.PulseReminderEveryXSeconds(3f);
	}

	protected IEnumerator ShowUnlockMythicWhenReady()
	{
		yield return new WaitForSeconds(m_secondsDelayBeforeTutorialPopups);
		m_helpPopup = NotificationManager.Get().CreatePopupText(UserAttentionBlocker.NONE, m_showUnlockMythicBone.position, m_showUnlockMythicBone.localScale, GameStrings.Get("GLUE_LETTUCE_COLLECTION_TUTORIAL_MAXED_OUT_UNLOCK"));
		m_helpPopup.ShowPopUpArrow(Notification.PopUpArrowDirection.Right);
		m_helpPopup.PulseReminderEveryXSeconds(3f);
	}

	public void Dev_ShowTutorialPopups()
	{
		List<Transform> list = new List<Transform>();
		list.AddRange(m_showLoadEquipmentInSlotTutorialBones);
		list.Add(m_upgradeAbilityTutorialBone);
		list.Add(m_AppearanceMercTutorialBone);
		list.Add(m_showUnlockMythicBone);
		foreach (Transform bone in list)
		{
			NotificationManager.Get().CreatePopupText(UserAttentionBlocker.NONE, bone.position, bone.localScale, bone.name);
		}
	}

	private void OnDropMercenariesModeCard(CollectionUtils.MercenariesModeCardType cardType, string mercenariesCardID)
	{
		if (cardType == CollectionUtils.MercenariesModeCardType.Equipment)
		{
			if (IsMouseOverEquipmentSlot())
			{
				SlotSelectedEquipment(mercenariesCardID);
			}
		}
		else
		{
			Debug.LogError(string.Format("{0}.{1} could not handle type {2}", "MercenaryDetailDisplay", "OnDropMercenariesModeCard", cardType));
		}
	}

	private void OnMercenaryArtVariationChanged(int mercenaryDbId, int artVariationId, TAG_PREMIUM premium)
	{
		LettuceMercenary mercenary = CollectionManager.Get().GetMercenary(mercenaryDbId);
		CollectionUtils.PopulateMercenaryCardDataModel(GetMercenaryDisplayDataModel(), mercenary.GetEquippedArtVariation());
	}

	protected void AcknowledgeAbilityorEquipment(int itemID, bool acknowledgeAll = false)
	{
		LettuceMercenaryDataModel mercData = GetMercenaryDisplayDataModel();
		LettuceMercenary mercenary = CollectionManager.Get().GetMercenary(mercData.MercenaryId);
		if (mercenary == null)
		{
			return;
		}
		foreach (LettuceAbilityDataModel abilityData in mercData.AbilityList)
		{
			LettuceAbility ability = mercenary.GetLettuceAbility(abilityData.AbilityId);
			if (ability != null && !mercenary.IsAbilityLocked(ability) && (itemID == abilityData.AbilityId || acknowledgeAll) && m_mercenaryAcknowledgements.FindIndex((MercenaryAcknowledgeData i) => i.AssetId == abilityData.AbilityId) <= 0)
			{
				abilityData.IsNew = false;
				MercenaryAcknowledgeData newAcknowledge = new MercenaryAcknowledgeData();
				newAcknowledge.Type = MercenaryAcknowledgeData.AcknowledgeType.ACKNOWLEDGE_MERC_ABILITY_ALL;
				newAcknowledge.AssetId = abilityData.AbilityId;
				newAcknowledge.Acknowledged = true;
				newAcknowledge.MercenaryId = mercData.MercenaryId;
				m_mercenaryAcknowledgements.Add(newAcknowledge);
				CollectionManager.Get().MarkMercenaryAsAcknowledgedinCollection(newAcknowledge);
			}
		}
		foreach (LettuceAbilityDataModel equipmentData in mercData.EquipmentList)
		{
			LettuceAbility equipment = mercenary.GetLettuceEquipment(equipmentData.AbilityId);
			if (equipment != null && equipment.Owned && (itemID == equipmentData.AbilityId || acknowledgeAll) && m_mercenaryAcknowledgements.FindIndex((MercenaryAcknowledgeData i) => i.AssetId == equipmentData.AbilityId) <= 0)
			{
				equipmentData.IsNew = false;
				MercenaryAcknowledgeData newAcknowledge2 = new MercenaryAcknowledgeData();
				newAcknowledge2.Type = MercenaryAcknowledgeData.AcknowledgeType.ACKNOWLEDGE_MERC_EQUIPMENT_ALL;
				newAcknowledge2.AssetId = equipmentData.AbilityId;
				newAcknowledge2.Acknowledged = true;
				newAcknowledge2.MercenaryId = mercData.MercenaryId;
				m_mercenaryAcknowledgements.Add(newAcknowledge2);
				CollectionManager.Get().MarkMercenaryAsAcknowledgedinCollection(newAcknowledge2);
			}
		}
	}

	protected void AcknowledgeArtVariation(int artID, TAG_PREMIUM premium, bool acknowledgeAll = false)
	{
		LettuceMercenaryDataModel mercData = GetMercenaryDisplayDataModel();
		LettuceMercenary mercenary = CollectionManager.Get().GetMercenary(mercData.MercenaryId);
		if (mercenary == null)
		{
			return;
		}
		foreach (LettuceMercenaryArtVariationDataModel artVariation in mercData.ArtVariationList)
		{
			if (artVariation.Unlocked && ((artVariation.ArtVariationId == artID && artVariation.Card.Premium == premium) || acknowledgeAll))
			{
				int portraitId = LettuceMercenary.GetPortraitIdFromArtVariation(artVariation.ArtVariationId, artVariation.Card.Premium);
				if (m_mercenaryAcknowledgements.FindIndex((MercenaryAcknowledgeData i) => i.AssetId == portraitId) < 0)
				{
					artVariation.NewlyUnlocked = false;
					MercenaryAcknowledgeData newAcknowledge = new MercenaryAcknowledgeData();
					newAcknowledge.Type = MercenaryAcknowledgeData.AcknowledgeType.ACKNOWLEDGE_MERC_PORTRAIT_ACQUIRED;
					newAcknowledge.MercenaryId = mercData.MercenaryId;
					newAcknowledge.AssetId = portraitId;
					newAcknowledge.Acknowledged = true;
					newAcknowledge.MercenaryId = mercData.MercenaryId;
					m_mercenaryAcknowledgements.Add(newAcknowledge);
					CollectionManager.Get().MarkMercenaryAsAcknowledgedinCollection(newAcknowledge);
				}
			}
		}
		mercData.NumNewPortraits = CollectionManager.Get().GetNumNewPortraitsToAcknowledgeForMercenary(mercenary);
	}

	protected void SendAcknowledgements()
	{
		MercenariesDataUtil.UpdateMercenaryDataModelNewStatus(GetMercenaryDisplayDataModel());
		Network.Get().RegisterNetHandler(MercenariesCollectionAcknowledgeResponse.PacketID.ID, OnCollectionAcknowledgeResponse);
		Network.Get().AcknowledgeMercenaryCollection(m_mercenaryAcknowledgements);
		m_mercenaryAcknowledgements.Clear();
	}

	protected void OnCollectionAcknowledgeResponse()
	{
		Network.Get().RemoveNetHandler(MercenariesCollectionAcknowledgeResponse.PacketID.ID, OnCollectionAcknowledgeResponse);
		if (!Network.Get().AcknowledgeMercenaryCollectionResponse().Success)
		{
			Debug.LogWarning("Error acknowledging collection");
		}
	}

	protected bool IsMythicFeatureEnabled()
	{
		return (NetCache.Get()?.GetNetObject<NetCache.NetCacheFeatures>())?.Games.MercenariesMythic ?? true;
	}

	protected void OnVillagePopupShown(LettuceVillagePopupManager.PopupType type)
	{
		if (type == LettuceVillagePopupManager.PopupType.TASKBOARD || type == LettuceVillagePopupManager.PopupType.RENOWNCONVERSION)
		{
			BlockTutorialPopups(isBlocked: true, VillagePopupBlockId);
		}
	}

	protected void OnVillagePopupClosed(LettuceVillagePopupManager.PopupType type)
	{
		if (type == LettuceVillagePopupManager.PopupType.TASKBOARD || type == LettuceVillagePopupManager.PopupType.RENOWNCONVERSION)
		{
			BlockTutorialPopups(isBlocked: false, VillagePopupBlockId);
		}
	}
}
