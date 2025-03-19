using System;
using Assets;
using Hearthstone;
using Hearthstone.DataModels;
using Hearthstone.UI;
using PegasusLettuce;

public class CollectionMercenaryDetailDisplay : MercenaryDetailDisplay
{
	protected override void Start()
	{
		m_mercDetailsDisplayVisualController = base.gameObject.GetComponent<VisualController>();
		m_mercDetailsDisplayVisualController.GetComponent<Widget>().RegisterEventListener(MercDetailsEventListener);
		m_abilityUpgradePopupReference.RegisterReadyListener<VisualController>(base.OnAbilityUpgradePopupReady);
		m_abilityInfoPopupReference.RegisterReadyListener(delegate(Widget w)
		{
			m_abilityInfoPopupWidget = w;
		});
		m_popupHandlerReference.RegisterReadyListener(delegate(VisualController vc)
		{
			m_popupHandlerVisualController = vc;
		});
		m_equipmentExplanationPopupReference.RegisterReadyListener<Widget>(base.OnEquipmentExplanationPopupReady);
		m_abilityUpgradeCardReference.RegisterReadyListener(delegate(Hearthstone.UI.Card card)
		{
			m_abilityUpgradeCard = card;
		});
		m_mythicTreasureInfoPopupReference.RegisterReadyListener<Widget>(base.OnMythicTreasureInfoPopupReady);
		Network network = Network.Get();
		network.RegisterNetHandler(UpgradeMercenaryAbilityResponse.PacketID.ID, OnAbilityUpgradeNetworkResponse);
		network.RegisterNetHandler(UpgradeMercenaryEquipmentResponse.PacketID.ID, base.OnEquipmentUpgradeNetworkResponse);
		network.RegisterNetHandler(CraftMercenaryEquipmentResponse.PacketID.ID, base.OnCraftEquipmentNetworkResponse);
		MercenaryDetailDisplay.s_instance = this;
	}

	public override void Unload()
	{
		if (MercenaryDetailDisplay.s_instance == this)
		{
			MercenaryDetailDisplay.s_instance = null;
		}
		Network network = Network.Get();
		if (network != null)
		{
			network.RemoveNetHandler(UpgradeMercenaryAbilityResponse.PacketID.ID, OnAbilityUpgradeNetworkResponse);
			network.RemoveNetHandler(UpgradeMercenaryEquipmentResponse.PacketID.ID, base.OnEquipmentUpgradeNetworkResponse);
			network.RemoveNetHandler(CraftMercenaryEquipmentResponse.PacketID.ID, base.OnCraftEquipmentNetworkResponse);
		}
	}

	public override void Show(LettuceMercenary merc, string showEvent = "SHOW_FULL", LettuceTeam editingTeam = null)
	{
		if (m_mercDetailsDisplayVisualController == null || merc == null)
		{
			return;
		}
		if (CollectionManager.Get().GetCollectibleDisplay().GetPageManager() as LettuceCollectionPageManager == null)
		{
			Log.Lettuce.PrintWarning("MercenaryDetailDisplay.Show - no LettuceCollectionPageManager found!");
			return;
		}
		if (editingTeam == null)
		{
			editingTeam = CollectionManager.Get().GetEditingTeam();
		}
		if (editingTeam != null && editingTeam.IsMercInTeam(merc.ID))
		{
			m_currentTeam = editingTeam;
			showEvent = "SHOW_PARTIAL";
			if (m_mercIdBeingViewed == -1)
			{
				Navigation.Push(CollectionDeckTray.Get().OnBackOutOfContainerContents);
			}
			CollectionDeckTray.Get().GetMercsContent().ChangeCurrentlySelectedMercenary(merc.ID, selected: true);
		}
		m_equipmentSlotCollider.SetActive(value: false);
		m_mercIdBeingViewed = merc.ID;
		SetupActiveMercDataModel(GetMercenaryDisplayDataModel(), merc);
		ShowRequiredTutorialIfNeeded();
		LettuceVillagePopupManager lettuceVillagePopupManager = LettuceVillagePopupManager.Get();
		lettuceVillagePopupManager.OnPopupShown = (Action<LettuceVillagePopupManager.PopupType>)Delegate.Combine(lettuceVillagePopupManager.OnPopupShown, new Action<LettuceVillagePopupManager.PopupType>(base.OnVillagePopupShown));
		LettuceVillagePopupManager lettuceVillagePopupManager2 = LettuceVillagePopupManager.Get();
		lettuceVillagePopupManager2.OnPopupClosed = (Action<LettuceVillagePopupManager.PopupType>)Delegate.Combine(lettuceVillagePopupManager2.OnPopupClosed, new Action<LettuceVillagePopupManager.PopupType>(base.OnVillagePopupClosed));
		if (!string.IsNullOrEmpty(showEvent))
		{
			m_mercDetailsDisplayVisualController.OwningWidget.TriggerEvent(showEvent, TriggerEventParameters.Standard);
		}
		CollectiblePageManager cpm = CollectionManager.Get().GetCollectibleDisplay().GetPageManager();
		if (cpm != null)
		{
			cpm.EnablePageTurn(enable: false);
			cpm.EnablePageTurnArrows(enable: false);
		}
	}

	public override void Hide()
	{
		AcknowledgeAbilityorEquipment(0, acknowledgeAll: true);
		SendAcknowledgements();
		CollectionManager.Get().TriggerNewCardSeenListeners();
		if (!(m_mercDetailsDisplayVisualController == null))
		{
			string hideEvent = "HIDE_FULL";
			LettuceTeam editingTeam = CollectionManager.Get().GetEditingTeam();
			if (m_currentTeam != null || (editingTeam != null && editingTeam.IsMercInTeam(m_mercIdBeingViewed)))
			{
				hideEvent = "HIDE_PARTIAL";
				CollectionDeckTray.Get().GetMercsContent().ChangeCurrentlySelectedMercenary(m_mercIdBeingViewed, selected: false);
			}
			m_currentTeam = null;
			m_mercDetailsDisplayVisualController.OwningWidget.TriggerEvent(hideEvent, TriggerEventParameters.Standard);
			m_mercIdBeingViewed = -1;
			HideHelpPopups();
			LettuceVillagePopupManager lettuceVillagePopupManager = LettuceVillagePopupManager.Get();
			lettuceVillagePopupManager.OnPopupShown = (Action<LettuceVillagePopupManager.PopupType>)Delegate.Remove(lettuceVillagePopupManager.OnPopupShown, new Action<LettuceVillagePopupManager.PopupType>(base.OnVillagePopupShown));
			LettuceVillagePopupManager lettuceVillagePopupManager2 = LettuceVillagePopupManager.Get();
			lettuceVillagePopupManager2.OnPopupClosed = (Action<LettuceVillagePopupManager.PopupType>)Delegate.Remove(lettuceVillagePopupManager2.OnPopupClosed, new Action<LettuceVillagePopupManager.PopupType>(base.OnVillagePopupClosed));
			CollectiblePageManager cpm = CollectionManager.Get().GetCollectibleDisplay().GetPageManager();
			if (cpm != null)
			{
				cpm.EnablePageTurn(enable: true);
				cpm.EnablePageTurnArrows(enable: true);
			}
			LettuceCollectionDisplay collectionDisplay = CollectionManager.Get().GetCollectibleDisplay() as LettuceCollectionDisplay;
			if (collectionDisplay != null)
			{
				collectionDisplay.OnReturnFromMercenaryDetailsDisplay();
			}
		}
	}

	protected override void OnEquipmentLoadoutDragStart()
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
			CollectionInputMgr.Get().GrabMercenariesModeCard(equipmentData, grabbedEquipment.m_cardType, base.OnEquipmentDropped);
			if (collectionMerc.CanUnslotEquipment(equipmentData.AbilityId))
			{
				m_draggingEquipmentDataModel = equipmentData;
			}
			CollectionDeckTray.Get().GetMercsContent().UpdateMercList();
			m_mercDetailsDisplayVisualController.OwningWidget.TriggerEvent("START_DRAG_EQUIPMENT");
		}
	}

	protected override void OnAbilityUpgradeNetworkResponse()
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
				CollectionDeckTray.Get().HighlightBackButton();
				LettuceTutorialUtils.FireEvent(LettuceTutorialVo.LettuceTutorialEvent.VILLAGE_TUTORIAL_UPGRADE_ABILITY_END, base.gameObject);
			}
		}
	}

	protected override LettuceMercenaryDataModel UpdateDataModelsAfterTransaction(LettuceAbility ability, LettuceMercenary merc)
	{
		LettuceMercenaryDataModel mercData = base.UpdateDataModelsAfterTransaction(ability, merc);
		if (mercData == null)
		{
			Log.Lettuce.PrintWarning("MercenaryDetailDisplay.UpdateDataModelsAfterTransaction - Unable to get Mercenary Data Model!");
			return null;
		}
		LettuceCollectionPageManager lcpm = CollectionManager.Get()?.GetCollectibleDisplay()?.GetPageManager() as LettuceCollectionPageManager;
		if (lcpm == null)
		{
			Log.Lettuce.PrintWarning("MercenaryDetailDisplay.UpdateDataModelsAfterTransaction - Unable to retrieve LettuceCollectionPageManager!");
		}
		else
		{
			LettuceMercenaryDataModel pageMerc = lcpm.GetMercenaryOnPage(mercData.MercenaryId);
			if (pageMerc != null)
			{
				pageMerc.ChildUpgradeAvailable = mercData.ChildUpgradeAvailable;
			}
		}
		LettuceMercenaryDataModel trayMerc = CollectionDeckTray.Get().GetMercsContent().GetMercenaryDataModel(merc.ID);
		if (trayMerc != null)
		{
			trayMerc.ChildUpgradeAvailable = mercData.ChildUpgradeAvailable;
		}
		return mercData;
	}

	protected override void MercDetailsEventListener(string eventName)
	{
		switch (eventName)
		{
		case "OnPress":
			Hide();
			return;
		case "ABILITY_UPGRADE_code":
			OnUpgradeAbility();
			return;
		case "POPUP_ACTIVATED_code":
			BlockTutorialPopups(isBlocked: true, DetailPopupBlockId);
			return;
		case "ABILITY_drag_started":
			OnAbilityDragStart();
			return;
		case "LOADOUT_ABILITY_drag_started":
			OnEquipmentLoadoutDragStart();
			return;
		case "ABILITY_CLICKED_code":
			OnAbilityClicked();
			return;
		case "ABILITY_HOVERED_code":
		{
			LettuceAbilityDataModel abilityDataModel = WidgetUtils.GetEventDataModel(m_mercDetailsDisplayVisualController).Payload as LettuceAbilityDataModel;
			AcknowledgeAbilityorEquipment(abilityDataModel.AbilityId);
			return;
		}
		case "MERC_COIN_OVER":
			ShowCoinTooltip();
			return;
		case "MERC_COIN_OUT":
			HideCoinTooltip();
			return;
		case "POPUP_DEACTIVATED_code":
			if (!ShowFullyUpgradeMercIfNeeded())
			{
				BlockTutorialPopups(isBlocked: false, DetailPopupBlockId);
			}
			return;
		case "ABILITY_INFO_POPUP_REVEAL_COMPLETED_code":
			ShowExplanationPopup();
			return;
		case "APPEARANCE_SELECTED_code":
			OnAppearanceClicked();
			return;
		case "MERCENARY_released":
			OnAppearanceResetPage();
			return;
		case "APPEARANCE_SHOW_code":
			ShowAppearancePart2TutorialIfNeeded();
			return;
		case "APPEARANCE_HIDE_code":
			AcknowledgeArtVariation(0, TAG_PREMIUM.NORMAL, acknowledgeAll: true);
			HideHelpPopups();
			return;
		case "ART_VARIATION_PREV_code":
			OnAppearanceArrowClicked(-1);
			return;
		case "ART_VARIATION_NEXT_code":
			OnAppearanceArrowClicked(1);
			return;
		case "ART_VARIATION_HOVERED_code":
			if (WidgetUtils.GetEventDataModel(m_mercDetailsDisplayVisualController).Payload is LettuceMercenaryArtVariationDataModel portraitDataModel)
			{
				AcknowledgeArtVariation(portraitDataModel.ArtVariationId, portraitDataModel.Card.Premium);
			}
			return;
		case "MYTHIC_TOGGLE_code":
			OnMythicToggle();
			return;
		case "TREASURE_TRAY_OPEN_code":
			m_isTreasureTrayOpen = true;
			HideHelpPopups();
			ShowRequiredTutorialIfNeeded();
			return;
		case "TREASURE_TRAY_CLOSE_code":
			m_isTreasureTrayOpen = false;
			HideHelpPopups();
			ShowRequiredTutorialIfNeeded();
			return;
		case "OPEN_MYTHIC_TREASURE_INFO_POPUP_REQUEST":
			m_widgetTemplate.TriggerEvent("OPEN_MYTHIC_TREASURE_INFO_POPUP");
			return;
		case "OPEN_MYTHIC_TREASURE_INFO_POPUP":
			m_currentInfoPopup = UIContext.GetRoot().ShowPopup(m_mythicTreasureInfoPopup.gameObject).gameObject;
			BnetBar.Get().SetBlockCurrencyFrames(isBlocked: true);
			return;
		case "SEEN_MYTHIC_TREASURE_INFO_POPUP":
			if (LettuceTutorialUtils.IsEventTypeComplete(LettuceTutorialVo.LettuceTutorialEvent.VILLAGE_TUTORIAL_VISIT_MYTHIC_TREASURE_START) && !LettuceTutorialUtils.IsEventTypeComplete(LettuceTutorialVo.LettuceTutorialEvent.VILLAGE_TUTORIAL_VISIT_MYTHIC_TREASURE_END))
			{
				LettuceTutorialUtils.FireEvent(LettuceTutorialVo.LettuceTutorialEvent.VILLAGE_TUTORIAL_VISIT_MYTHIC_TREASURE_END, base.gameObject);
			}
			return;
		case "HIDE_COMPLETE":
			UIContext.GetRoot().DismissPopup(m_currentInfoPopup);
			m_currentInfoPopup = null;
			BnetBar.Get().SetBlockCurrencyFrames(isBlocked: false);
			return;
		}
		LettuceCollectionDisplay lcd = CollectionManager.Get().GetCollectibleDisplay() as LettuceCollectionDisplay;
		if (lcd == null)
		{
			Log.Lettuce.PrintWarning("MercenaryDetailDisplay.MercDetailsEventListener - LettuceCollectionDisplay is null!");
		}
		else
		{
			lcd.HandleTileHoverEvents(eventName, m_mercDetailsDisplayVisualController);
		}
	}

	public override bool ShowFullyUpgradeMercIfNeeded()
	{
		return PopupDisplayManager.Get().RewardPopups.ShowMercenariesFullyUpgraded(delegate
		{
			LettuceMercenaryDataModel mercenaryDisplayDataModel = GetMercenaryDisplayDataModel();
			LettuceMercenary mercenary = CollectionManager.Get().GetMercenary(mercenaryDisplayDataModel.MercenaryId);
			SetupActiveMercDataModel(mercenaryDisplayDataModel, mercenary);
			LettuceCollectionPageManager lettuceCollectionPageManager = CollectionManager.Get().GetCollectibleDisplay().GetPageManager() as LettuceCollectionPageManager;
			if (lettuceCollectionPageManager != null)
			{
				lettuceCollectionPageManager.UpdatePageMercenary(MercenaryFactory.CreateMercenaryDataModelWithCoin(mercenary));
			}
			BlockTutorialPopups(isBlocked: false, DetailPopupBlockId);
			BnetBar.Get().RefreshCurrency();
		});
	}

	protected override bool CanPickUpAbility(LettuceMercenary merc, LettuceAbility ability)
	{
		if (CollectionManager.Get().GetCollectibleDisplay().GetPageManager()
			.ArePagesTurning())
		{
			return true;
		}
		return base.CanPickUpAbility(merc, ability);
	}
}
