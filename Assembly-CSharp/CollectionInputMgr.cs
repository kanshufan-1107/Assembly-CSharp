using System;
using System.Collections.Generic;
using Hearthstone;
using Hearthstone.DataModels;
using Hearthstone.UI;
using PegasusShared;
using UnityEngine;

public class CollectionInputMgr : InputMgr
{
	private static CollectionInputMgr s_instance;

	private UIBScrollable m_scrollBar;

	public static event Action<CollectionCardVisual> CollectionDraggableCardGrabbed;

	public static event Action CollectionDraggableCardDropped;

	public new static CollectionInputMgr Get()
	{
		return s_instance;
	}

	protected override void Awake()
	{
		base.Awake();
		s_instance = this;
	}

	protected override void OnDestroy()
	{
		s_instance = null;
		base.OnDestroy();
	}

	public override bool HandleKeyboardInput()
	{
		if (CollectionManager.Get() == null || SceneMgr.Get() == null)
		{
			return false;
		}
		if (SceneMgr.Get().IsInLettuceMode())
		{
			HandleMercTeamCopying();
		}
		else
		{
			HandleDeckCopying();
		}
		if (InputCollection.GetKeyUp(KeyCode.Escape))
		{
			if (CardBackInfoManager.IsLoadedAndShowingPreview())
			{
				CardBackInfoManager.Get().CancelPreview();
				return true;
			}
			if (CraftingManager.Get() != null && CraftingManager.Get().IsCardShowing() && !CraftingManager.Get().IsCancelling())
			{
				Navigation.GoBack();
				return true;
			}
		}
		if (HearthstoneApplication.GetMode() == ApplicationMode.INTERNAL && InputCollection.GetKeyUp(KeyCode.P))
		{
			TAG_PREMIUM otherPremium = TAG_PREMIUM.GOLDEN;
			if (CollectionManager.Get().GetPreferredPremium() == TAG_PREMIUM.GOLDEN)
			{
				otherPremium = TAG_PREMIUM.NORMAL;
			}
			Debug.Log("setting premium preference " + otherPremium);
			CollectionManager.Get().SetPremiumPreference(otherPremium);
			return true;
		}
		return false;
	}

	public static void PasteDeckFromClipboard()
	{
		ShareableDeck shareableDeck = ShareableDeck.DeserializeFromClipboard();
		if (shareableDeck != null)
		{
			PasteDeckInEditModeFromShareableDeck(shareableDeck);
		}
	}

	public static void PasteDeckInEditModeFromShareableDeck(ShareableDeck shareableDeck)
	{
		if (!CollectionManager.Get().IsInEditMode())
		{
			Debug.LogError("Error trying to paste deck. Collection Manager is not in edit mode.");
			return;
		}
		CollectionDeck editedDeck = CollectionManager.Get().GetEditedDeck();
		editedDeck.SetShareableDeckCreatedFrom(shareableDeck);
		DefLoader defLoader = DefLoader.Get();
		List<DeckMaker.DeckFill> pastedDeckFill = new List<DeckMaker.DeckFill>();
		for (int cardIndex = 0; cardIndex < shareableDeck.DeckContents.Cards.Count; cardIndex++)
		{
			DeckCardData item = shareableDeck.DeckContents.Cards[cardIndex];
			EntityDef cardToAdd = defLoader.GetEntityDef(item.Def.Asset);
			int totalOwnedCount = CollectionManager.Get().GetTotalOwnedCount(cardToAdd.GetCardId());
			int numToAdd = item.Qty;
			for (int i = 0; i < numToAdd && i < totalOwnedCount; i++)
			{
				pastedDeckFill.Add(new DeckMaker.DeckFill
				{
					m_addCard = cardToAdd
				});
			}
			numToAdd -= totalOwnedCount;
			if (numToAdd <= 0)
			{
				continue;
			}
			string ownedCounterpartCardID = GameUtils.GetCounterpartCardIDForFormat(cardToAdd, shareableDeck.FormatType);
			if (ownedCounterpartCardID != null)
			{
				EntityDef ownedCounterpartCardDef = defLoader.GetEntityDef(ownedCounterpartCardID);
				if (ownedCounterpartCardDef != null && editedDeck.CanAddCard(ownedCounterpartCardDef, (TAG_PREMIUM)item.Def.Premium))
				{
					int totalOwnedCounterpart = CollectionManager.Get().GetTotalOwnedCount(ownedCounterpartCardDef.GetCardId());
					for (int j = 0; j < numToAdd && j < totalOwnedCounterpart; j++)
					{
						pastedDeckFill.Add(new DeckMaker.DeckFill
						{
							m_addCard = ownedCounterpartCardDef
						});
					}
					numToAdd -= totalOwnedCounterpart;
				}
			}
			for (int k = 0; k < numToAdd; k++)
			{
				pastedDeckFill.Add(new DeckMaker.DeckFill
				{
					m_addCard = cardToAdd
				});
			}
		}
		CollectionDeckTray.PopuplateDeckCompleteCallback completedCallback = delegate(List<EntityDef> addedCards, List<EntityDef> removedCards)
		{
			CollectionDeck editedDeck2 = CollectionManager.Get().GetEditedDeck();
			int num = CollectionManager.Get().GetDeckRuleset()?.GetDeckSize(editedDeck2) ?? int.MinValue;
			if (editedDeck2 != null && (editedDeck2.HasReplaceableSlot() || editedDeck2.GetTotalCardCount() < num))
			{
				CollectionDeckTray.Get().OnCardManuallyAddedByUser_CheckSuggestions(addedCards);
			}
			editedDeck2?.RemoveOrphanedSideboards();
		};
		CollectionManager.Get().GetEditedDeck();
		CollectionDeckTray.Get().PopulateDeck(pastedDeckFill, completedCallback);
		editedDeck.ClearSideboards();
		foreach (SideBoardCardData sideboardCardData in shareableDeck.DeckContents.SideboardCards)
		{
			string cardId = GameUtils.TranslateDbIdToCardId(sideboardCardData.Def.Asset);
			for (int l = 0; l < sideboardCardData.Qty; l++)
			{
				editedDeck.AddCardToSideboardPreferredPremium(cardId, sideboardCardData.LinkedCardDbId, (TAG_PREMIUM)sideboardCardData.Def.Premium, allowInvalid: true);
			}
		}
	}

	public static void AlertPlayerOnInvalidDeckPaste(string errorReason)
	{
		AlertPopup.PopupInfo popup = new AlertPopup.PopupInfo
		{
			m_headerText = GameStrings.Get("GLUE_COLLECTION_DECK_INVALID_POPUP_HEADER"),
			m_text = errorReason,
			m_okText = GameStrings.Get("GLOBAL_OKAY"),
			m_showAlertIcon = true,
			m_responseDisplay = AlertPopup.ResponseDisplay.OK
		};
		DialogManager.Get().ShowPopup(popup);
	}

	public bool StartDragWithActor(Actor actor, CollectionUtils.ViewMode viewMode, bool showVisual = true, CollectionDeckSlot slot = null)
	{
		if (!CanGrabItem(actor) || m_heldCardVisual == null)
		{
			return false;
		}
		m_heldCardVisual.SetSlot(slot);
		CollectionDeckTray collectionDeckTray = CollectionDeckTray.Get();
		if (collectionDeckTray != null)
		{
			collectionDeckTray.StartDragWithActor(actor, viewMode, showVisual, slot);
		}
		TAG_PREMIUM premium = slot?.UnPreferredPremium ?? actor.GetPremium();
		if (!m_heldCardVisual.ChangeActor(actor, viewMode, premium))
		{
			return false;
		}
		if (m_scrollBar != null)
		{
			m_scrollBar.Pause(pause: true);
		}
		PegCursor.Get().SetMode(PegCursor.Mode.DRAG);
		m_heldCardVisual.transform.position = actor.transform.position;
		m_heldCardVisual.Show(showVisual);
		SoundManager.Get().LoadAndPlay("collection_manager_pick_up_card.prefab:f7fb595cdc26f2f4997b4a10eaf1d0e1", m_heldCardVisual.gameObject);
		return true;
	}

	public bool GrabCardVisual(CollectionCardVisual cardVisual)
	{
		Actor actor = cardVisual.GetCollectionCardActors().GetPreferredActor();
		CollectionUtils.ViewMode viewMode = cardVisual.GetVisualType();
		if (!StartDragWithActor(actor, cardVisual.GetVisualType(), MouseIsOverDeck))
		{
			return false;
		}
		switch (viewMode)
		{
		case CollectionUtils.ViewMode.CARDS:
		{
			CollectionManagerDisplay cmd = CollectionManager.Get().GetCollectibleDisplay() as CollectionManagerDisplay;
			if (cmd != null)
			{
				cmd.HideFilterTrayOnStartDragCard();
			}
			CollectionInputMgr.CollectionDraggableCardGrabbed?.Invoke(cardVisual);
			break;
		}
		case CollectionUtils.ViewMode.COINS:
			CollectionDeckTray.Get()?.GetCosmeticCoinContent()?.ToggleSparkleEffects(enabled: true);
			break;
		case CollectionUtils.ViewMode.CARD_BACKS:
		{
			CollectionCardBack cardBackComponent = actor.GetComponent<CollectionCardBack>();
			if (cardBackComponent != null)
			{
				m_heldCardVisual.SetCardBackId(cardBackComponent.GetCardBackId());
				CollectionDeckTray.Get()?.GetCardBackContent()?.ToggleSparkleEffects(enabled: true);
			}
			break;
		}
		case CollectionUtils.ViewMode.HERO_SKINS:
			CollectionDeckTray.Get()?.GetHeroSkinContent()?.ToggleSparkleEffects(enabled: true);
			break;
		}
		return true;
	}

	public bool GrabCardTile(DeckTrayDeckTileVisual deckTileVisual, OnCardDroppedCallback callback, bool removeCard = true)
	{
		m_cardDroppedCallback = callback;
		return GrabCardTile(deckTileVisual, removeCard);
	}

	public bool GrabCardTile(DeckTrayDeckTileVisual deckTileVisual, bool removeCard = true)
	{
		Actor actor = deckTileVisual.GetActor();
		CollectionDeckSlot slot = deckTileVisual.GetSlot();
		if (!StartDragWithActor(actor, CollectionUtils.ViewMode.CARDS, MouseIsOverDeck, slot))
		{
			return false;
		}
		if (removeCard)
		{
			CollectionDeckTray collectionDeckTray = CollectionDeckTray.Get();
			collectionDeckTray.RemoveCard(valid: collectionDeckTray.GetCurrentDeckContext().IsValidSlot(slot, ignoreOwnership: false, ignoreGameplayEvent: false, enforceRemainingDeckRuleset: false, null), cardID: m_heldCardVisual.GetCardID(), premium: slot.UnPreferredPremium);
			if (!collectionDeckTray.IsSideboardOpen && !Options.Get().GetBool(Option.HAS_REMOVED_CARD_FROM_DECK, defaultVal: false))
			{
				CollectionDeckTray.Get().GetCardsContent().HideDeckHelpPopup();
				Options.Get().SetBool(Option.HAS_REMOVED_CARD_FROM_DECK, val: true);
			}
		}
		return true;
	}

	public bool GrabCardBackFromSlot(Actor actor, int cardBackId)
	{
		if (!StartDragWithActor(actor, CollectionUtils.ViewMode.CARD_BACKS))
		{
			return false;
		}
		m_heldCardVisual.SetCardBackId(cardBackId);
		return true;
	}

	public bool GrabCosmeticCoinFromSlot(Actor actor)
	{
		if (!StartDragWithActor(actor, CollectionUtils.ViewMode.COINS))
		{
			return false;
		}
		return true;
	}

	public bool GrabHeroSkinFromSlot(Actor actor)
	{
		if (!StartDragWithActor(actor, CollectionUtils.ViewMode.HERO_SKINS))
		{
			return false;
		}
		return true;
	}

	public override bool GrabMercenariesModeCard(IDataModel dataModel, CollectionUtils.MercenariesModeCardType cardType, OnCardDroppedCallback callback = null)
	{
		if (dataModel == null)
		{
			return false;
		}
		if (!CanGrabMercenariesModeItem(cardType))
		{
			return false;
		}
		if (m_mercenariesDraggablesWidget == null)
		{
			return false;
		}
		if (!UniversalInputManager.Get().GetInputHitInfo(Box.Get().GetCamera(), GameLayer.DragPlane.LayerBit(), out var hit))
		{
			return false;
		}
		m_cardDroppedCallback = callback;
		m_mercenariesDraggablesWidget.BindDataModel(dataModel);
		if (m_scrollBar != null)
		{
			m_scrollBar.Pause(pause: true);
		}
		PegCursor.Get().SetMode(PegCursor.Mode.DRAG);
		string holdOverCollectionEvent = null;
		string holdOverTeamTrayEvent = null;
		switch (cardType)
		{
		case CollectionUtils.MercenariesModeCardType.Mercenary:
			holdOverCollectionEvent = "START_MERC_OVER_COLLECTION_code";
			holdOverTeamTrayEvent = "HOLD_MERC_OVER_TEAM_TRAY_code";
			break;
		case CollectionUtils.MercenariesModeCardType.Equipment:
			holdOverCollectionEvent = "HOLD_ABILITY_OVER_COLLECTION_code";
			holdOverTeamTrayEvent = "HOLD_ABILITY_OVER_TEAM_TRAY_code";
			break;
		}
		SetHeldMercenaryCard(dataModel, cardType);
		m_mercenariesDraggablesWidget.TriggerEvent(holdOverCollectionEvent);
		DisableDraggableColliders();
		bool mouseCurrentlyOverDeck = CollectionDeckTray.Get().MouseIsOver(Box.Get().GetCamera());
		if (mouseCurrentlyOverDeck)
		{
			m_mercenariesDraggablesWidget.TriggerEvent(holdOverTeamTrayEvent);
		}
		else
		{
			m_mercenariesDraggablesWidget.TriggerEvent(holdOverCollectionEvent);
		}
		MouseIsOverDeck = mouseCurrentlyOverDeck;
		m_offScreenPosition = m_mercenariesDraggablesWidget.gameObject.transform.position;
		m_mercenariesDraggablesWidget.gameObject.transform.position = hit.point;
		CollectionManagerDisplay cmd = CollectionManager.Get().GetCollectibleDisplay() as CollectionManagerDisplay;
		if (cmd != null)
		{
			cmd.HideFilterTrayOnStartDragCard();
		}
		return true;
	}

	public override void SetHeldMercenaryCard(IDataModel dataModel, CollectionUtils.MercenariesModeCardType cardType)
	{
		if (dataModel == null)
		{
			Log.Lettuce.PrintWarning("CollectionInputMgr.SetHeldMercenaryCard - input data model is not valid!");
			return;
		}
		m_heldType = cardType;
		switch (cardType)
		{
		case CollectionUtils.MercenariesModeCardType.Mercenary:
			if (!(dataModel is LettuceMercenaryDataModel mercenaryData))
			{
				Log.Lettuce.PrintWarning("CollectionInputMgr.SetHeldMercenaryCard - mercenary data model is not valid!");
			}
			else
			{
				m_heldMercenariesModeCardId = CollectionManager.Get().GetMercenary(mercenaryData.MercenaryId).GetCardId();
			}
			break;
		case CollectionUtils.MercenariesModeCardType.Equipment:
			if (dataModel is LettuceAbilityDataModel { AbilityTiers: not null } abilityData)
			{
				LettuceAbilityTierDataModel tierData = abilityData.AbilityTiers[abilityData.CurrentTier - 1];
				if (tierData == null || tierData.AbilityTierCard == null)
				{
					Log.Lettuce.PrintWarning("CollectionInputMgr.SetHeldMercenaryCard - ability tier data model is not valid!");
				}
				else
				{
					m_heldMercenariesModeCardId = tierData.AbilityTierCard.CardId;
				}
			}
			else
			{
				Log.Lettuce.PrintWarning("CollectionInputMgr.SetHeldMercenaryCard - ability data model is not valid!");
			}
			break;
		}
	}

	public bool GrabBattlegroundsEmote(IDataModel dataModel, CollectionUtils.BattlegroundsModeDraggableType bgType, OnCardDroppedCallback callback = null, Widget sourceWidget = null)
	{
		if (dataModel == null)
		{
			return false;
		}
		if (bgType == CollectionUtils.BattlegroundsModeDraggableType.None)
		{
			return false;
		}
		if (m_battlegroundsDraggablesWidget == null)
		{
			return false;
		}
		if (!UniversalInputManager.Get().GetInputHitInfo(Box.Get().GetCamera(), GameLayer.DragPlane.LayerBit(), out var hit))
		{
			return false;
		}
		m_cardDroppedCallback = callback;
		if (!SetHeldBattlegroundsEmote(dataModel, bgType))
		{
			return false;
		}
		m_battlegroundsDraggablesWidget.Hide();
		m_battlegroundsDraggablesWidget.BindDataModel(dataModel);
		m_battlegroundsDraggablesWidget.TriggerEvent("START_EMOTE_DRAG_code");
		m_battlegroundsDraggablesWidget.RegisterDoneChangingStatesListener(delegate
		{
			BattlegroundsEmoteDataModel battlegroundsEmoteDataModel = dataModel as BattlegroundsEmoteDataModel;
			if (m_heldBattlegroundsEmoteCardId == battlegroundsEmoteDataModel.EmoteDbiId.ToString())
			{
				if (bgType == CollectionUtils.BattlegroundsModeDraggableType.TrayEmote && sourceWidget != null)
				{
					sourceWidget.Hide();
				}
				m_battlegroundsDraggablesWidget.Show();
			}
		}, null, callImmediatelyIfSet: true, doOnce: true);
		if (m_scrollBar != null)
		{
			m_scrollBar.Pause(pause: true);
		}
		PegCursor.Get().SetMode(PegCursor.Mode.DRAG);
		DisableBattlegroundsDraggableColliders();
		m_offScreenPosition = m_battlegroundsDraggablesWidget.gameObject.transform.position;
		m_battlegroundsDraggablesWidget.gameObject.transform.position = hit.point;
		CollectionManagerDisplay cmd = CollectionManager.Get().GetCollectibleDisplay() as CollectionManagerDisplay;
		if (cmd != null)
		{
			cmd.HideFilterTrayOnStartDragCard();
		}
		BaconCollectionDisplay bcd = CollectionManager.Get().GetCollectibleDisplay() as BaconCollectionDisplay;
		if (bcd == null)
		{
			Log.CollectionManager.PrintError("Unable to access BaconCollectionDisplay");
			return false;
		}
		bcd.m_pageManager.EnableEmoteHoverHighlights(enable: false);
		SoundManager.Get().LoadAndPlay("collection_manager_pick_up_card.prefab:f7fb595cdc26f2f4997b4a10eaf1d0e1", m_battlegroundsDraggablesWidget.gameObject);
		return true;
	}

	public bool SetHeldBattlegroundsEmote(IDataModel dataModel, CollectionUtils.BattlegroundsModeDraggableType bgType)
	{
		if (!(dataModel is BattlegroundsEmoteDataModel emoteData))
		{
			Log.CollectionManager.PrintWarning("CollectionInputMgr.SetHeldBattlegroundsEmote - emote data model is not valid!");
			return false;
		}
		if (bgType == CollectionUtils.BattlegroundsModeDraggableType.CollectionEmote && m_battlegroundsEmoteTray.IsEmoteInLoadout(emoteData.EmoteDbiId))
		{
			return false;
		}
		m_bgHeldType = bgType;
		m_heldBattlegroundsEmoteCardId = emoteData.EmoteDbiId.ToString();
		return true;
	}

	public void SetScrollbar(UIBScrollable scrollbar)
	{
		m_scrollBar = scrollbar;
	}

	public bool IsDraggingScrollbar()
	{
		if (m_scrollBar != null)
		{
			return m_scrollBar.IsDragging();
		}
		return false;
	}

	public bool HasHeldCard()
	{
		if ((m_heldCardVisual != null && m_heldCardVisual.IsShown()) || (m_mercenariesDraggablesWidget != null && m_heldType != 0))
		{
			return true;
		}
		return false;
	}

	public bool HasHeldEmote()
	{
		if (m_battlegroundsDraggablesWidget != null && m_bgHeldType != 0)
		{
			return true;
		}
		return false;
	}

	private bool CanGrabItem(Actor actor)
	{
		if (IsDraggingScrollbar())
		{
			return false;
		}
		if (m_heldCardVisual == null || m_heldCardVisual.IsShown())
		{
			return false;
		}
		if (actor == null)
		{
			return false;
		}
		return true;
	}

	protected override bool CanGrabMercenariesModeItem(CollectionUtils.MercenariesModeCardType itemType)
	{
		if (IsDraggingScrollbar())
		{
			return false;
		}
		if (m_heldType != 0)
		{
			return false;
		}
		return true;
	}

	protected override void UpdateHeldCardVisual()
	{
		if (!UniversalInputManager.Get().GetInputHitInfo(GameLayer.DragPlane.LayerBit(), out var hit))
		{
			return;
		}
		if (m_heldCardVisual != null && (bool)UniversalInputManager.UsePhoneUI)
		{
			Transform[] componentsInChildren = m_heldCardVisual.GetComponentsInChildren<Transform>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].gameObject.layer = 19;
			}
		}
		Vector3 newPos = hit.point;
		if ((bool)UniversalInputManager.UsePhoneUI)
		{
			newPos.y += InputMgr.PHONE_HEIGHT_OFFSET;
		}
		m_heldCardVisual.transform.position = newPos;
		CollectionDeckTray collectionDeckTray = CollectionDeckTray.Get();
		CollectionDraggableCardVisual.ActorVisualMode draggedVisualMode = ((!MouseIsOverDeck) ? CollectionDraggableCardVisual.ActorVisualMode.BIG_CARD : CollectionDraggableCardVisual.ActorVisualMode.DECK_TILE);
		if (collectionDeckTray != null)
		{
			MouseIsOverDeck = CollectionDeckTray.Get().MouseIsOver(Box.Get().GetCamera());
			if (!collectionDeckTray.UpdateHeldCardVisual(m_heldCardVisual))
			{
				m_heldCardVisual.UpdateVisual(draggedVisualMode);
			}
		}
		if (DraftPhoneDeckTray.Get() != null)
		{
			MouseIsOverDeck = DraftPhoneDeckTray.Get().MouseIsOver();
			m_heldCardVisual.UpdateVisual(draggedVisualMode);
		}
		if (InputCollection.GetMouseButtonUp(0))
		{
			DropCard(dragCanceled: false);
		}
	}

	protected override void UpdateMercenariesHeldVisual(CollectionUtils.MercenariesModeCardType heldType)
	{
		string eventName = "";
		bool isOverTarget = false;
		switch (heldType)
		{
		case CollectionUtils.MercenariesModeCardType.Equipment:
			isOverTarget = (CollectionManager.Get().GetCollectibleDisplay() as LettuceCollectionDisplay).GetMercenaryDetailsDisplay().IsMouseOverEquipmentSlot();
			if (isOverTarget && !MouseIsOverDeck)
			{
				eventName = "HOLD_ABILITY_OVER_TEAM_TRAY_code";
			}
			else if (!isOverTarget && MouseIsOverDeck)
			{
				eventName = "HOLD_ABILITY_OVER_COLLECTION_code";
			}
			break;
		case CollectionUtils.MercenariesModeCardType.Mercenary:
			if ((bool)CollectionDeckTray.Get())
			{
				isOverTarget = CollectionDeckTray.Get().MouseIsOver(Box.Get().GetCamera());
			}
			if (isOverTarget && !MouseIsOverDeck)
			{
				eventName = "HOLD_MERC_OVER_TEAM_TRAY_code";
			}
			else if (!isOverTarget && MouseIsOverDeck)
			{
				eventName = "HOLD_MERC_OVER_COLLECTION_code";
			}
			break;
		}
		if (!string.IsNullOrEmpty(eventName))
		{
			m_mercenariesDraggablesWidget.TriggerEvent(eventName);
		}
		MouseIsOverDeck = isOverTarget;
	}

	protected override void DropCard(bool dragCanceled)
	{
		PegCursor.Get().SetMode(PegCursor.Mode.STOPDRAG);
		if (m_heldCardVisual == null)
		{
			return;
		}
		CollectionUtils.ViewMode vtype = m_heldCardVisual.GetVisualType();
		if (!dragCanceled)
		{
			if (MouseIsOverDeck)
			{
				switch (vtype)
				{
				case CollectionUtils.ViewMode.CARDS:
					if (CollectionDeckTray.Get() != null && CollectionDeckTray.Get().AddCard(m_heldCardVisual.GetCurrentlyShowingEntityDef(), m_heldCardVisual.GetPremium(), true, null, DeckRule.RuleType.DEATHKNIGHT_RUNE_LIMIT))
					{
						CollectionDeckTray deckTray3 = CollectionDeckTray.Get();
						if (deckTray3 != null)
						{
							deckTray3.OnCardManuallyAddedByUser_CheckSuggestions(m_heldCardVisual.GetEntityDef());
						}
					}
					break;
				case CollectionUtils.ViewMode.COINS:
				{
					EntityDef coinCardDef = m_heldCardVisual.GetEntityDef();
					int coinId = CosmeticCoinManager.Get().GetCoinIdFromCoinCard(coinCardDef.GetCardId());
					if (coinCardDef != null)
					{
						CollectionDeckTray deckTray4 = CollectionDeckTray.Get();
						if (deckTray4 != null)
						{
							deckTray4.GetCosmeticCoinContent()?.UpdateCosmeticCoin(coinId, assigning: true);
						}
					}
					break;
				}
				case CollectionUtils.ViewMode.CARD_BACKS:
				{
					int cardBackId = m_heldCardVisual.GetCardBackId();
					if (cardBackId != -1)
					{
						CollectionDeckTray deckTray2 = CollectionDeckTray.Get();
						if (deckTray2 != null)
						{
							deckTray2.GetCardBackContent()?.UpdateCardBack(cardBackId, assigning: true);
						}
					}
					else
					{
						Debug.LogWarning("Cardback ID not set for dragging card back.");
					}
					break;
				}
				case CollectionUtils.ViewMode.HERO_SKINS:
				{
					EntityDef skinDef = m_heldCardVisual.GetEntityDef();
					TAG_PREMIUM skinPremium = m_heldCardVisual.GetPremium();
					if (skinDef != null)
					{
						CollectionDeckTray deckTray = CollectionDeckTray.Get();
						if (deckTray != null)
						{
							deckTray.GetHeroSkinContent()?.UpdateHeroSkin(skinDef.GetCardId(), skinPremium, assigning: true);
						}
					}
					break;
				}
				case CollectionUtils.ViewMode.BATTLEGROUNDS_HERO_SKINS:
					Debug.LogWarning("DropCard called in battlegrounds hero skins view mode. Should not be possible to pick up card in this mode.");
					break;
				case CollectionUtils.ViewMode.BATTLEGROUNDS_GUIDE_SKINS:
					Debug.LogWarning("DropCard called in guide skins view mode. Should not be possible to pick up card in this mode.");
					break;
				}
			}
			else
			{
				SoundManager.Get().LoadAndPlay("collection_manager_drop_card.prefab:8275e45efb8280347b35c2548e706d84", m_heldCardVisual.gameObject);
				switch (vtype)
				{
				case CollectionUtils.ViewMode.COINS:
					CollectionDeckTray.Get()?.GetCosmeticCoinContent()?.ToggleSparkleEffects(enabled: false);
					break;
				case CollectionUtils.ViewMode.CARD_BACKS:
					CollectionDeckTray.Get()?.GetCardBackContent()?.ToggleSparkleEffects(enabled: false);
					break;
				case CollectionUtils.ViewMode.HERO_SKINS:
					CollectionDeckTray.Get()?.GetHeroSkinContent()?.ToggleSparkleEffects(enabled: false);
					break;
				}
				if (m_cardDroppedCallback != null)
				{
					m_cardDroppedCallback();
					m_cardDroppedCallback = null;
				}
			}
		}
		m_heldCardVisual.Hide();
		if (m_scrollBar != null)
		{
			m_scrollBar.Pause(pause: false);
		}
		CollectionManagerDisplay cmd = CollectionManager.Get().GetCollectibleDisplay() as CollectionManagerDisplay;
		if (cmd != null)
		{
			if (!dragCanceled && MouseIsOverDeck)
			{
				cmd.WaitThenUnhideFilterTrayOnStopDragCard();
			}
			else
			{
				cmd.UnhideFilterTrayOnStopDragCard();
			}
		}
		if (vtype == CollectionUtils.ViewMode.CARDS)
		{
			CollectionInputMgr.CollectionDraggableCardDropped?.Invoke();
		}
	}

	public override void DropMercenariesModeCard(bool dragCanceled)
	{
		if (m_heldType == CollectionUtils.MercenariesModeCardType.None)
		{
			return;
		}
		PegCursor.Get().SetMode(PegCursor.Mode.STOPDRAG);
		if (m_mercenariesDraggablesWidget == null)
		{
			return;
		}
		if (!dragCanceled)
		{
			if (MouseIsOverDeck)
			{
				if (m_heldType == CollectionUtils.MercenariesModeCardType.Mercenary)
				{
					if (CollectionDeckTray.Get() != null)
					{
						EntityDef entityDef = DefLoader.Get().GetEntityDef(m_heldMercenariesModeCardId);
						int index = -1;
						bool mouseOverMercListable = false;
						DeckTrayMercListContent mercsContent = CollectionDeckTray.Get().GetMercsContent();
						if (mercsContent != null && mercsContent.MercListable != null && mercsContent.MercListable.WidgetItems != null)
						{
							foreach (WidgetInstance widgetItem in mercsContent.MercListable.WidgetItems)
							{
								index++;
								GameObject collider = widgetItem.GetComponentInChildren<BoxCollider>(includeInactive: false).gameObject;
								if ((bool)collider && UniversalInputManager.Get().ForcedUnblockableInputIsOver(Camera.main, collider.gameObject, out var _))
								{
									mouseOverMercListable = true;
									break;
								}
							}
						}
						if (!mouseOverMercListable)
						{
							index = -1;
						}
						if (CollectionDeckTray.Get().AddCardToTeam(entityDef, playSound: true, index))
						{
							CollectionDeckTray.Get().OnCardManuallyAddedByUser_CheckSuggestions(entityDef);
						}
					}
				}
				else if (m_heldType == CollectionUtils.MercenariesModeCardType.Equipment)
				{
					LettuceCollectionDisplay lcd = CollectionManager.Get().GetCollectibleDisplay() as LettuceCollectionDisplay;
					if (lcd == null)
					{
						Log.Lettuce.PrintWarning("CollectionInputMgr.DropMercenariesModeCard - unable to find LettuceCollectionDisplay!");
						return;
					}
					lcd.SlotEquipmentOnActiveMercenary(m_heldMercenariesModeCardId);
				}
			}
			else
			{
				m_mercenariesDraggablesWidget.TriggerEvent("END_MERC_OVER_COLLECTION_code");
				if (m_cardDroppedCallback != null)
				{
					m_cardDroppedCallback();
					m_cardDroppedCallback = null;
				}
			}
		}
		m_mercenariesDraggablesWidget.gameObject.transform.position = m_offScreenPosition;
		m_heldMercenariesModeCardId = string.Empty;
		m_heldType = CollectionUtils.MercenariesModeCardType.None;
		if (m_scrollBar != null)
		{
			m_scrollBar.Pause(pause: false);
		}
		CollectionManagerDisplay cmd = CollectionManager.Get().GetCollectibleDisplay() as CollectionManagerDisplay;
		if (cmd != null)
		{
			if (!dragCanceled && MouseIsOverDeck)
			{
				cmd.WaitThenUnhideFilterTrayOnStopDragCard();
			}
			else
			{
				cmd.UnhideFilterTrayOnStopDragCard();
			}
		}
	}

	public override void DropBattlegroundsEmote(bool dragCanceled, bool trayDropCanceled = false)
	{
		if (m_bgHeldType == CollectionUtils.BattlegroundsModeDraggableType.None)
		{
			return;
		}
		PegCursor.Get().SetMode(PegCursor.Mode.STOPDRAG);
		if (m_battlegroundsDraggablesWidget == null)
		{
			return;
		}
		m_battlegroundsDraggablesWidget.TriggerEvent("END_EMOTE_DRAG_code");
		m_battlegroundsDraggablesWidget.gameObject.transform.position = m_offScreenPosition;
		if (!dragCanceled)
		{
			BattlegroundsEmoteDataModel emoteDataModel = m_battlegroundsDraggablesWidget.GetDataModel<BattlegroundsEmoteDataModel>();
			if (m_battlegroundsEmoteTray.IsEmoteOverTray() && !trayDropCanceled)
			{
				m_battlegroundsEmoteTray.DropOverEmoteTray(emoteDataModel);
			}
			else if (m_bgHeldType == CollectionUtils.BattlegroundsModeDraggableType.TrayEmote)
			{
				m_battlegroundsEmoteTray.RemoveEmote(emoteDataModel);
			}
			else if (m_cardDroppedCallback != null)
			{
				m_cardDroppedCallback();
				m_cardDroppedCallback = null;
			}
		}
		m_heldBattlegroundsEmoteCardId = string.Empty;
		m_bgHeldType = CollectionUtils.BattlegroundsModeDraggableType.None;
		if (m_scrollBar != null)
		{
			m_scrollBar.Pause(pause: false);
		}
		BaconCollectionDisplay bcd = CollectionManager.Get().GetCollectibleDisplay() as BaconCollectionDisplay;
		if (bcd == null)
		{
			Log.CollectionManager.PrintError("Unable to access BaconCollectionDisplay");
			return;
		}
		bcd.m_pageManager.EnableEmoteHoverHighlights(enable: true);
		SoundManager.Get().LoadAndPlay("collection_manager_drop_card.prefab:8275e45efb8280347b35c2548e706d84", m_battlegroundsDraggablesWidget.gameObject);
		m_battlegroundsEmoteTray.UpdateTrayHighlight(trayHovered: false);
	}

	protected override void OnMouseOnOrOffScreen(bool onScreen)
	{
		if (m_heldCardVisual == null || m_heldCardVisual.gameObject == null)
		{
			return;
		}
		if (onScreen)
		{
			if (m_heldCardOffscreen)
			{
				m_heldCardOffscreen = false;
				if (InputCollection.GetMouseButton(0))
				{
					m_heldCardVisual.Show(MouseIsOverDeck);
				}
				else
				{
					DropCard(dragCanceled: true);
				}
			}
		}
		else if (m_heldCardVisual.IsShown())
		{
			m_heldCardVisual.Hide();
			m_heldCardOffscreen = true;
		}
	}

	private void HandleDeckCopying()
	{
		if (!(CollectionDeckTray.Get() != null) || (!InputCollection.GetKey(KeyCode.LeftMeta) && !InputCollection.GetKey(KeyCode.LeftControl) && !InputCollection.GetKey(KeyCode.RightMeta) && !InputCollection.GetKey(KeyCode.RightControl)))
		{
			return;
		}
		bool showingDeckContents = CollectionDeckTray.Get().IsShowingDeckContents();
		if (InputCollection.GetKeyDown(KeyCode.C) && showingDeckContents)
		{
			CollectionDeck deck = CollectionManager.Get().GetEditedDeck();
			if (deck != null && UIStatus.Get() != null)
			{
				DeckRuleViolation topViolation = null;
				bool num = deck.CanCopyAsShareableDeck(out topViolation);
				if (topViolation != null)
				{
					string topViolationMessage = CollectionDeck.GetUserFriendlyCopyErrorMessageFromDeckRuleViolation(topViolation);
					if (!string.IsNullOrEmpty(topViolationMessage))
					{
						UIStatus.Get().AddInfo(topViolationMessage);
					}
				}
				if (num)
				{
					ClipboardUtils.CopyToClipboard(deck.GetShareableDeck().Serialize());
					UIStatus.Get().AddInfo(GameStrings.Get("GLUE_COLLECTION_DECK_COPIED_TOAST"));
				}
			}
		}
		if (!InputCollection.GetKeyDown(KeyCode.V))
		{
			return;
		}
		CollectionManagerDisplay cmd = CollectionManager.Get().GetCollectibleDisplay() as CollectionManagerDisplay;
		if (cmd != null && DialogManager.Get() != null)
		{
			bool num2 = DialogManager.Get().ShowingDialog();
			bool isSelectingDeckHero = cmd.IsSelectingNewDeckHero();
			if (!num2 && !showingDeckContents && !isSelectingDeckHero)
			{
				cmd.PasteFromClipboardIfValidOrShowStatusMessage();
			}
		}
	}

	private void HandleMercTeamCopying()
	{
		CollectionDeckTray.Get().GetTeamsContent();
		if (!InputCollection.GetKey(KeyCode.LeftMeta) && !InputCollection.GetKey(KeyCode.LeftControl) && !InputCollection.GetKey(KeyCode.RightMeta) && !InputCollection.GetKey(KeyCode.RightControl))
		{
			return;
		}
		if (CollectionDeckTray.Get().IsShowingTeamContents() && InputCollection.GetKeyDown(KeyCode.C))
		{
			LettuceTeam team = CollectionManager.Get().GetEditingTeam();
			if (team != null && UIStatus.Get() != null)
			{
				ClipboardUtils.CopyToClipboard(new ShareableMercenariesTeam(team).Serialize());
				UIStatus.Get().AddInfo(GameStrings.Get("GLUE_COLLECTION_DECK_COPIED_TOAST"));
			}
		}
		if (!InputCollection.GetKeyDown(KeyCode.V))
		{
			return;
		}
		LettuceCollectionDisplay collectionDisplay = CollectionManager.Get().GetCollectibleDisplay() as LettuceCollectionDisplay;
		if (collectionDisplay != null)
		{
			DialogManager dialogManager = DialogManager.Get();
			if (dialogManager != null && !dialogManager.ShowingDialog())
			{
				collectionDisplay.TeamCopying.CheckClipboardAndPromptPlayerToPaste();
			}
		}
	}
}
