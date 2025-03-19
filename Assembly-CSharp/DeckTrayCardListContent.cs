using System;
using System.Collections;
using System.Collections.Generic;
using Hearthstone.Core;
using PegasusShared;
using UnityEngine;

[CustomEditClass]
public class DeckTrayCardListContent : DeckTrayContent
{
	public delegate void CardTileHeld(DeckTrayDeckTileVisual cardTile);

	public delegate void CardTilePress(DeckTrayDeckTileVisual cardTile);

	public delegate void CardTileTap(DeckTrayDeckTileVisual cardTile);

	public delegate void CardTileOver(DeckTrayDeckTileVisual cardTile);

	public delegate void CardTileOut(DeckTrayDeckTileVisual cardTile);

	public delegate void CardTileRelease(DeckTrayDeckTileVisual cardTile);

	public delegate void CardTileRightClicked(DeckTrayDeckTileVisual cardTile);

	public delegate void CardCountChanged(int cardCount, int maxCount);

	[CustomEditField(Sections = "Card Tile Settings")]
	public float m_cardTileHeight = 2.45f;

	[CustomEditField(Sections = "Card Tile Settings")]
	public float m_cardHelpButtonHeight = 3f;

	[CustomEditField(Sections = "Card Tile Settings")]
	public float m_deckCardBarFlareUpInterval = 0.075f;

	[CustomEditField(Sections = "Card Tile Settings")]
	public GameObject m_phoneDeckTileBone;

	[CustomEditField(Sections = "Card Tile Settings")]
	public Vector3 m_cardTileOffset = Vector3.zero;

	[CustomEditField(Sections = "Card Tile Settings")]
	public float m_cardTileSlotLocalHeight;

	[CustomEditField(Sections = "Card Tile Settings")]
	public Vector3 m_cardTileSlotLocalScaleVec3 = new Vector3(0.01f, 0.02f, 0.01f);

	[CustomEditField(Sections = "Card Tile Settings")]
	public float m_cardTileSlotLocalHeightSideboard;

	[CustomEditField(Sections = "Card Tile Settings")]
	public bool m_forceUseFullScaleDeckTileActors;

	[CustomEditField(Sections = "Deck Help")]
	public UIBButton m_smartDeckCompleteButton;

	[CustomEditField(Sections = "Deck Help")]
	public UIBButton m_deckTemplateHelpButton;

	[CustomEditField(Sections = "Deck Help")]
	public float m_deckTemplateHelpButtonDeathKnightPosY = -0.13f;

	[CustomEditField(Sections = "Other Objects")]
	public GameObject m_deckCompleteHighlight;

	[CustomEditField(Sections = "Other Objects")]
	public GameObject m_runeIndicatorSpacer;

	[CustomEditField(Sections = "Scroll Settings")]
	public UIBScrollable m_scrollbar;

	[CustomEditField(Sections = "Scroll Settings")]
	public BoxCollider m_LockedScrollBounds;

	[CustomEditField(Sections = "Sideboard")]
	public UIBButton m_sideboardDoneButton;

	[CustomEditField(Sections = "Sideboard")]
	public bool m_isSideboardContent;

	[CustomEditField(Sections = "TwistRulesHeader")]
	public GameObject m_twistRulesIndicator;

	[CustomEditField(Sections = "TwistRulesHeader")]
	public float m_twistRulesHeaderOffsetPosY;

	[CustomEditField(Sections = "TwistRulesHeader")]
	public float m_twistRulesHeaderOffsetPosYDeathKnight;

	[CustomEditField(Sections = "TwistRulesHeader")]
	public float m_twistRulesHeaderOffsetPosYReplaceCard;

	[CustomEditField(Sections = "TwistRulesHeader")]
	public float m_twistRulesHeaderOffsetPosYReplaceCardAndDeathKnight;

	[CustomEditField(Sections = "TwistRulesHeader")]
	public UIBScrollable m_deckTrayScrollable;

	[CustomEditField(Sections = "TwistRulesHeader")]
	public float m_twistScrollBottomPaddingWithHeader = 8f;

	[CustomEditField(Sections = "TwistRulesHeader")]
	public float m_twistScrollBottomPaddingWithoutHeader = 1.2f;

	private const string ADD_CARD_TO_DECK_SOUND = "collection_manager_card_add_to_deck_instant.prefab:06df359c4026d7e47b06a4174f33e3ef";

	private const float CARD_MOVEMENT_TIME = 0.3f;

	private Vector3 m_originalLocalPosition;

	private List<DeckTrayDeckTileVisual> m_cardTiles = new List<DeckTrayDeckTileVisual>();

	private List<CardTileHeld> m_cardTileHeldListeners = new List<CardTileHeld>();

	private List<CardTilePress> m_cardTilePressListeners = new List<CardTilePress>();

	private List<CardTileTap> m_cardTileTapListeners = new List<CardTileTap>();

	private List<CardTileOver> m_cardTileOverListeners = new List<CardTileOver>();

	private List<CardTileOut> m_cardTileOutListeners = new List<CardTileOut>();

	private List<CardTileRelease> m_cardTileReleaseListeners = new List<CardTileRelease>();

	private List<CardTileRightClicked> m_cardTileRightClickedListeners = new List<CardTileRightClicked>();

	private List<CardCountChanged> m_cardCountChangedListeners = new List<CardCountChanged>();

	private List<DefLoader.DisposableCardDef> m_cardDefs = new List<DefLoader.DisposableCardDef>();

	private bool m_animating;

	private bool m_loading;

	private const float DECK_HELP_BUTTON_EMPTY_DECK_Y_LOCAL_POS = -0.01194457f;

	private const float DECK_HELP_BUTTON_Y_TILE_OFFSET = -0.04915909f;

	private bool m_inArena;

	private CollectionDeck m_templateFakeDeck = new CollectionDeck();

	private bool m_isShowingFakeDeck;

	private bool m_hasFinishedEntering;

	private bool m_hasFinishedExiting = true;

	private Notification m_deckHelpPopup;

	private Vector3 m_deckTemplateHelpButtonOriginalLocalPosition;

	private Vector3 m_originalTwistRulesIndicatorPosition;

	private float TemplateDeckHelpButtonHeight => m_deckTemplateHelpButton.GetComponent<UIBScrollableItem>().m_size.z * m_cardTileSlotLocalScaleVec3.z;

	private float RuneIndicatorSpacerHeight
	{
		get
		{
			if (!m_runeIndicatorSpacer)
			{
				return 0f;
			}
			return m_runeIndicatorSpacer.GetComponent<UIBScrollableItem>().m_size.z * m_cardTileSlotLocalScaleVec3.z;
		}
	}

	public static event Action DoneButtonPressed;

	protected override void Awake()
	{
		base.Awake();
		if (m_smartDeckCompleteButton != null)
		{
			m_smartDeckCompleteButton.AddEventListener(UIEventType.RELEASE, OnDeckCompleteButtonPress);
			m_smartDeckCompleteButton.AddEventListener(UIEventType.ROLLOVER, OnDeckCompleteButtonOver);
			m_smartDeckCompleteButton.AddEventListener(UIEventType.ROLLOUT, OnDeckCompleteButtonOut);
		}
		if (m_deckTemplateHelpButton != null)
		{
			m_deckTemplateHelpButton.AddEventListener(UIEventType.RELEASE, OnDeckTemplateHelpButtonPress);
			m_deckTemplateHelpButton.AddEventListener(UIEventType.ROLLOVER, OnDeckTemplateHelpButtonOver);
			m_deckTemplateHelpButton.AddEventListener(UIEventType.ROLLOUT, OnDeckTemplateHelpButtonOut);
			m_deckTemplateHelpButtonOriginalLocalPosition = m_deckTemplateHelpButton.transform.localPosition;
		}
		if (m_sideboardDoneButton != null)
		{
			m_sideboardDoneButton.AddEventListener(UIEventType.RELEASE, OnSideboardDoneButtonPress);
			m_sideboardDoneButton.AddEventListener(UIEventType.ROLLOVER, OnSideboardDoneButtonOver);
			m_sideboardDoneButton.AddEventListener(UIEventType.ROLLOUT, OnSideboardDoneButtonOut);
		}
		m_originalLocalPosition = base.transform.localPosition;
		if (m_twistRulesIndicator != null)
		{
			m_originalTwistRulesIndicatorPosition = m_twistRulesIndicator.transform.localPosition;
		}
		m_hasFinishedEntering = false;
	}

	protected override void OnDestroy()
	{
		m_cardDefs.DisposeValuesAndClear();
		base.OnDestroy();
	}

	public override bool AnimateContentEntranceStart()
	{
		if (m_loading)
		{
			return false;
		}
		m_animating = true;
		m_hasFinishedEntering = false;
		Action<object> setup = delegate
		{
			UpdateDeckCompleteHighlight();
			ShowDeckEditingTipsIfNeeded();
			m_animating = false;
		};
		CollectionDeck currentDeck = GetEditingDeck();
		if (currentDeck != null)
		{
			base.transform.localPosition = GetOffscreenLocalPosition();
			iTween.StopByName(base.gameObject, "position");
			iTween.MoveTo(base.gameObject, iTween.Hash("position", m_originalLocalPosition, "islocal", true, "time", 0.3f, "easetype", iTween.EaseType.easeOutQuad, "oncomplete", setup, "name", "position"));
			if (currentDeck.GetTotalCardCount() > 0)
			{
				SoundManager.Get().LoadAndPlay("collection_manager_new_deck_moves_up_tray.prefab:13650cd587089e14d9a297c8de6057f1", base.gameObject);
			}
			UpdateCardList(updateHighlight: false);
		}
		else
		{
			setup(null);
		}
		return true;
	}

	public override bool AnimateContentEntranceEnd()
	{
		if (m_animating)
		{
			return false;
		}
		m_hasFinishedEntering = true;
		FireCardCountChangedEvent();
		return true;
	}

	public override bool AnimateContentExitStart()
	{
		if (m_animating)
		{
			return false;
		}
		m_animating = true;
		m_hasFinishedExiting = false;
		if (m_deckCompleteHighlight != null)
		{
			m_deckCompleteHighlight.SetActive(value: false);
		}
		iTween.StopByName(base.gameObject, "position");
		iTween.MoveTo(base.gameObject, iTween.Hash("position", GetOffscreenLocalPosition(), "islocal", true, "time", 0.3f, "easetype", iTween.EaseType.easeInQuad, "name", "position"));
		if (HeroPickerDisplay.Get() == null || !HeroPickerDisplay.Get().IsShown())
		{
			SoundManager.Get().LoadAndPlay("panel_slide_off_deck_creation_screen.prefab:b0d25fc984ec05d4fbea7480b611e5ad", base.gameObject);
		}
		Processor.ScheduleCallback(0.5f, realTime: false, delegate
		{
			m_animating = false;
		});
		return true;
	}

	public override bool AnimateContentExitEnd()
	{
		m_hasFinishedExiting = true;
		return !m_animating;
	}

	public bool HasFinishedEntering()
	{
		return m_hasFinishedEntering;
	}

	public bool HasFinishedExiting()
	{
		return m_hasFinishedExiting;
	}

	public override void OnEditedDeckChanged(CollectionDeck newDeck, CollectionDeck oldDeck, bool isNewDeck)
	{
		if (newDeck == null)
		{
			return;
		}
		List<CollectionDeckSlot> collectionDeckSlots = newDeck.GetSlots();
		LoadCardPrefabs(collectionDeckSlots);
		if (IsModeActive())
		{
			ShowDeckHelpButtonIfNeeded();
		}
		CollectionManager cm = CollectionManager.Get();
		if (cm != null)
		{
			CollectionManagerDisplay collectionManagerDisplay = cm.GetCollectibleDisplay() as CollectionManagerDisplay;
			if (collectionManagerDisplay != null)
			{
				CollectionPageManager collectionPageManager = collectionManagerDisplay.GetPageManager() as CollectionPageManager;
				if (collectionPageManager != null)
				{
					collectionPageManager.OnCurrentClassChanged -= OnCollectionManagerClassChanged;
					if (newDeck != null)
					{
						collectionPageManager.OnCurrentClassChanged += OnCollectionManagerClassChanged;
					}
				}
			}
		}
		UpdateAttentionGlowSlots();
	}

	public void ShowDeckHelper(CollectionDeckSlot slotToReplace, bool replaceSingleSlotOnly)
	{
		if (!CollectionManager.Get().IsInEditMode() || (TavernBrawlDisplay.IsTavernBrawlOpen() && TavernBrawlDisplay.IsTavernBrawlViewing()) || !DeckHelper.Get())
		{
			return;
		}
		if (!Network.IsLoggedIn())
		{
			CollectionManager.ShowFeatureDisabledWhileOfflinePopup();
			return;
		}
		DeckHelper.DelCompleteCallback cb = delegate(List<EntityDef> chosenCards)
		{
			if (CollectionDeckTray.Get() != null)
			{
				CollectionDeckTray.Get().OnCardManuallyAddedByUser_CheckSuggestions(chosenCards);
			}
		};
		DeckHelper.Get().Show(slotToReplace, replaceSingleSlotOnly, cb);
	}

	public bool MouseIsOverDeckHelperButton(Camera camera)
	{
		if (m_smartDeckCompleteButton != null && m_smartDeckCompleteButton.gameObject.activeInHierarchy)
		{
			return UniversalInputManager.Get().InputIsOver(camera, m_smartDeckCompleteButton.gameObject);
		}
		return false;
	}

	public bool MouseIsOverDeckCardTile()
	{
		foreach (DeckTrayDeckTileVisual tile in m_cardTiles)
		{
			if (UniversalInputManager.Get().InputIsOver(tile.gameObject))
			{
				return true;
			}
		}
		return false;
	}

	public DeckTrayDeckTileVisual GetCardTileVisual(string cardID)
	{
		foreach (DeckTrayDeckTileVisual tile in m_cardTiles)
		{
			if (!(tile == null) && !(tile.GetActor() == null) && tile.GetActor().GetEntityDef() != null && tile.GetActor().GetEntityDef().GetCardId() == cardID)
			{
				return tile;
			}
		}
		return null;
	}

	public DeckTrayDeckTileVisual GetCardTileVisual(int index)
	{
		if (index < m_cardTiles.Count)
		{
			return m_cardTiles[index];
		}
		return null;
	}

	public DeckTrayDeckTileVisual CreateCardTileVisual(string cardTileName, Transform parent)
	{
		string objName = cardTileName;
		if (string.IsNullOrEmpty(objName))
		{
			objName = "DeckTileVisual";
		}
		GameObject obj = new GameObject(objName);
		GameUtils.SetParent(obj, parent);
		obj.transform.localScale = m_cardTileSlotLocalScaleVec3;
		bool useFullScaleDeckTileActor = !UniversalInputManager.UsePhoneUI || m_forceUseFullScaleDeckTileActors;
		DeckTrayDeckTileVisual deckTrayDeckTileVisual = obj.AddComponent<DeckTrayDeckTileVisual>();
		deckTrayDeckTileVisual.Initialize(useFullScaleDeckTileActor);
		return deckTrayDeckTileVisual;
	}

	public DeckTrayDeckTileVisual GetOrAddCardTileVisual(int index)
	{
		DeckTrayDeckTileVisual newTileVisual = GetCardTileVisual(index);
		if (newTileVisual != null)
		{
			return newTileVisual;
		}
		newTileVisual = CreateCardTileVisual("DeckTileVisual" + index, base.transform);
		newTileVisual.AddEventListener(UIEventType.DRAG, delegate
		{
			FireCardTileDragEvent(newTileVisual);
		});
		newTileVisual.AddEventListener(UIEventType.PRESS, delegate
		{
			FireCardTilePressEvent(newTileVisual);
		});
		newTileVisual.AddEventListener(UIEventType.TAP, delegate
		{
			FireCardTileTapEvent(newTileVisual);
		});
		newTileVisual.AddEventListener(UIEventType.ROLLOVER, delegate
		{
			FireCardTileOverEvent(newTileVisual);
		});
		newTileVisual.AddEventListener(UIEventType.ROLLOUT, delegate
		{
			FireCardTileOutEvent(newTileVisual);
		});
		newTileVisual.AddEventListener(UIEventType.RELEASE, delegate
		{
			FireCardTileReleaseEvent(newTileVisual);
		});
		newTileVisual.AddEventListener(UIEventType.RIGHTCLICK, delegate
		{
			FireCardTileRightClickedEvent(newTileVisual);
		});
		m_cardTiles.Insert(index, newTileVisual);
		Vector3 extents = new Vector3(m_cardTileHeight, m_cardTileHeight, m_cardTileHeight);
		if (m_scrollbar != null)
		{
			m_scrollbar.AddVisibleAffectedObject(newTileVisual.gameObject, extents, visible: true, IsCardTileVisible);
		}
		return newTileVisual;
	}

	public List<string> GetCardIdsMatchingOrAboveRuneCost(RuneType runeType, int cost, List<EntityDef> remainingCards)
	{
		if (remainingCards == null)
		{
			remainingCards = new List<EntityDef>();
		}
		else
		{
			remainingCards.Clear();
		}
		List<string> results = new List<string>();
		foreach (DeckTrayDeckTileVisual tile in m_cardTiles)
		{
			CollectionDeckTileActor actor = tile.GetActor();
			if (actor == null)
			{
				continue;
			}
			EntityDef def = actor.GetEntityDef();
			if (def != null && tile.IsInUse())
			{
				int cardRuneCost = def.GetRuneCost().GetCost(runeType);
				if (cardRuneCost > 0 && cardRuneCost >= cost)
				{
					results.Add(tile.GetCardID());
				}
				else
				{
					remainingCards.Add(def);
				}
			}
		}
		return results;
	}

	public void UpdateTileVisuals()
	{
		foreach (DeckTrayDeckTileVisual cardTile in m_cardTiles)
		{
			cardTile.UpdateGhostedState();
		}
	}

	public void UpdateSideboardSlots(CollectionDeck deck)
	{
		if (deck == null)
		{
			return;
		}
		foreach (DeckTrayDeckTileVisual cardTile in m_cardTiles)
		{
			CollectionDeckTileActor actor = cardTile.GetActor();
			if (!(actor == null))
			{
				EntityDef entityDef = actor.GetEntityDef();
				if (entityDef != null)
				{
					SideboardDeck sideboard = deck.GetSideboard(entityDef.GetCardId());
					actor.SetupSideboard(sideboard);
				}
			}
		}
	}

	private void OnCollectionManagerClassChanged(TAG_CLASS _)
	{
		UpdateAttentionGlowSlots();
	}

	private void UpdateAttentionGlowSlots()
	{
		CollectionManager cm = CollectionManager.Get();
		CollectionDeck deck = null;
		TAG_CLASS currentCollectionManagerClass = TAG_CLASS.INVALID;
		if (cm != null)
		{
			deck = cm.GetEditedDeck();
			CollectionManagerDisplay collectionManagerDisplay = cm.GetCollectibleDisplay() as CollectionManagerDisplay;
			if (collectionManagerDisplay != null)
			{
				CollectionPageManager collectionPageManager = collectionManagerDisplay.GetPageManager() as CollectionPageManager;
				if (collectionPageManager != null)
				{
					currentCollectionManagerClass = collectionPageManager.GetCurrentClassContextClassTag();
				}
			}
		}
		foreach (DeckTrayDeckTileVisual cardTile in m_cardTiles)
		{
			CollectionDeckTileActor actor = cardTile.GetActor();
			if (actor == null)
			{
				continue;
			}
			EntityDef entityDef = actor.GetEntityDef();
			if (entityDef != null)
			{
				bool isTourist = entityDef.HasTag(GAME_TAG.TOURIST);
				TAG_CLASS touristClass = (isTourist ? entityDef.GetTag<TAG_CLASS>(GAME_TAG.TOURIST) : TAG_CLASS.INVALID);
				bool showGlow = deck != null && isTourist && currentCollectionManagerClass == touristClass;
				if (actor.m_attentionGlow != null)
				{
					actor.m_attentionGlow.SetActive(showGlow);
				}
			}
		}
	}

	public override void Show(bool showAll = false)
	{
		foreach (DeckTrayDeckTileVisual tile in m_cardTiles)
		{
			if (showAll || tile.IsInUse())
			{
				tile.Show();
			}
		}
	}

	public override void Hide(bool hideAll = false)
	{
		foreach (DeckTrayDeckTileVisual tile in m_cardTiles)
		{
			if (hideAll || !tile.IsInUse())
			{
				tile.Hide();
			}
		}
	}

	public void CommitFakeDeckChanges()
	{
		CollectionDeck editedDeck = CollectionManager.Get().GetEditedDeck();
		editedDeck.CopyContents(m_templateFakeDeck);
		editedDeck.Name = m_templateFakeDeck.Name;
	}

	public CollectionDeck GetEditingDeck()
	{
		if (m_isSideboardContent)
		{
			return CollectionManager.Get().GetEditedDeck()?.GetCurrentSideboardDeck();
		}
		if (m_isShowingFakeDeck)
		{
			if (CollectionManager.Get() != null)
			{
				CollectionDeck editedDeck = CollectionManager.Get().GetEditedDeck();
				if (editedDeck != null)
				{
					m_templateFakeDeck.FormatType = editedDeck.FormatType;
				}
			}
			if (m_templateFakeDeck.FormatType == FormatType.FT_UNKNOWN)
			{
				Debug.LogError("CollectionDeck.GetEditingDeck could not determine the format type for the fake deck " + m_templateFakeDeck.ToString());
			}
		}
		if (!m_isShowingFakeDeck)
		{
			return CollectionManager.Get().GetEditedDeck();
		}
		return m_templateFakeDeck;
	}

	public void ShowFakeDeck(bool show)
	{
		if (m_isShowingFakeDeck != show)
		{
			m_isShowingFakeDeck = show;
			UpdateCardList();
		}
	}

	public void ResetFakeDeck()
	{
		if (m_templateFakeDeck != null)
		{
			CollectionDeck copyFrom = CollectionManager.Get().GetEditedDeck();
			if (copyFrom != null)
			{
				m_templateFakeDeck.CopyContents(copyFrom);
				m_templateFakeDeck.Name = copyFrom.Name;
			}
		}
	}

	public void ShowDeckCompleteEffects()
	{
		StartCoroutine(ShowDeckCompleteEffectsWithInterval(m_deckCardBarFlareUpInterval));
	}

	public void SetInArena(bool inArena)
	{
		m_inArena = inArena;
	}

	public bool AddCard(EntityDef cardEntityDef, TAG_PREMIUM premium, bool playSound, Action<string> callback, Actor animateFromActor = null, bool updateVisuals = true, bool allowInvalid = false, params DeckRule.RuleType[] ignoreRules)
	{
		if (!IsModeActive())
		{
			return false;
		}
		if (cardEntityDef == null)
		{
			Debug.LogError("Trying to add card EntityDef that is null.");
			return false;
		}
		string cardID = cardEntityDef.GetCardId();
		CollectionDeck deck = GetEditingDeck();
		if (deck == null)
		{
			return false;
		}
		if (deck.GetTotalCardCount() >= CollectionManager.Get().GetDeckSizeWhileEditing(cardEntityDef))
		{
			GameplayErrorManager.Get().DisplayMessage(GameStrings.Get("GLUE_COLLECTION_MANAGER_ON_ADD_FULL_DECK_ERROR_TEXT"));
			return false;
		}
		if (playSound)
		{
			SoundManager.Get().LoadAndPlay("collection_manager_place_card_in_deck.prefab:df069ffaea9dfb24b96accc95bc434a7", base.gameObject);
		}
		if (CollectionManager.Get().GetEditedDeck() == null)
		{
			return false;
		}
		if (!deck.AddCard(cardID, premium, allowInvalid, cardEntityDef, ignoreRules))
		{
			Debug.LogWarningFormat("DeckTrayCardListContent.AddCard({0},{1}): deck.AddCard failed!", cardID, premium);
			return false;
		}
		if (updateVisuals)
		{
			UpdateCardList(cardEntityDef, updateHighlight: true, animateFromActor, delegate
			{
				callback?.Invoke(cardID);
			});
			CollectionManager.Get().GetCollectibleDisplay().UpdateCurrentPageCardLocks(playSound: true);
		}
		DeckHelper.Get().OnCardAdded(deck);
		if (!Options.Get().GetBool(Option.HAS_ADDED_CARDS_TO_DECK, defaultVal: false) && !m_isSideboardContent && deck.GetTotalCardCount() >= 2 && !DeckHelper.Get().IsActive() && deck.GetTotalCardCount() < 15 && UserAttentionManager.CanShowAttentionGrabber("DeckTrayCardListContent.AddCard:" + Option.HAS_ADDED_CARDS_TO_DECK))
		{
			NotificationManager.Get().CreateInnkeeperQuote(UserAttentionBlocker.NONE, GameStrings.Get("VO_INNKEEPER_CM_PAGEFLIP_28"), "VO_INNKEEPER_CM_PAGEFLIP_28.prefab:47bb7bdb89ad93443ab7d031bbe666fb");
			Options.Get().SetBool(Option.HAS_ADDED_CARDS_TO_DECK, val: true);
		}
		return true;
	}

	[ContextMenu("Update Card List")]
	public void UpdateCardList()
	{
		UpdateCardList(updateHighlight: true);
	}

	public void UpdateCardList(bool updateHighlight, Actor animateFromActor = null, Action onCompleteCallback = null)
	{
		UpdateCardList(string.Empty, updateHighlight, animateFromActor, onCompleteCallback);
	}

	public void UpdateCardList(EntityDef justChangedCardEntityDef, bool updateHighlight = true, Actor animateFromActor = null, Action onCompleteCallback = null)
	{
		UpdateCardList((justChangedCardEntityDef != null) ? justChangedCardEntityDef.GetCardId() : string.Empty, updateHighlight, animateFromActor, onCompleteCallback);
	}

	public void UpdateCardList(string justChangedCardID, bool updateHighlight = true, Actor animateFromActor = null, Action onCompleteCallback = null)
	{
		UpdateCardList(justChangedCardID, GetEditingDeck(), updateHighlight, animateFromActor, onCompleteCallback);
	}

	public void UpdateCardList(string justChangedCardID, CollectionDeck currentDeck, bool updateHighlight = true, Actor animateFromActor = null, Action onCompleteCallback = null)
	{
		if (currentDeck == null)
		{
			return;
		}
		foreach (DeckTrayDeckTileVisual cardTile in m_cardTiles)
		{
			cardTile.MarkAsUnused();
		}
		List<CollectionDeckSlot> deckSlots = currentDeck.GetSlots();
		int cardCount = 0;
		Vector3 cardTileOffset = GetCardTileOffset(currentDeck);
		Vector3 lastCardPosition = cardTileOffset;
		bool offsetCardNameForRunes = currentDeck.ContainsDeathKnightRuneCards();
		List<CollectionDeckSlot> slotsToForceSort = new List<CollectionDeckSlot>();
		foreach (CollectionDeckSlot currentSlot in deckSlots)
		{
			if (currentDeck.ProcessForDynamicallySortingSlot(currentSlot))
			{
				slotsToForceSort.Add(currentSlot);
			}
		}
		foreach (CollectionDeckSlot slotToSort in slotsToForceSort)
		{
			currentDeck.ForceUpdateSlotPosition(slotToSort);
		}
		int currentDeckTileIndex = 0;
		for (int currentDeckSlotIndex = 0; currentDeckSlotIndex < deckSlots.Count; currentDeckSlotIndex++)
		{
			CollectionDeckSlot slot = deckSlots[currentDeckSlotIndex];
			if (slot.Count == 0)
			{
				Log.DeckTray.Print($"DeckTrayCardListContent.UpdateCardList(): Slot {currentDeckSlotIndex} of deck is empty! Skipping...");
			}
			else
			{
				if (slot.GetEntityDef().HasTag(GAME_TAG.ZILLIAX_CUSTOMIZABLE_COSMETICMODULE))
				{
					continue;
				}
				cardCount += slot.Count;
				DeckTrayDeckTileVisual orAddCardTileVisual = GetOrAddCardTileVisual(currentDeckTileIndex);
				orAddCardTileVisual.SetInArena(m_inArena);
				Vector3 newPosition = cardTileOffset;
				if (currentDeckTileIndex > 0)
				{
					CollectionDeckTileActor actor = GetOrAddCardTileVisual(currentDeckTileIndex - 1).GetActor();
					float localHeightOffset = m_cardTileSlotLocalHeight;
					if ((bool)actor)
					{
						EntityDef entity = actor.GetEntityDef();
						if (entity != null && entity.HasSideboard)
						{
							localHeightOffset = m_cardTileSlotLocalHeightSideboard;
						}
					}
					newPosition = lastCardPosition + Vector3.down * localHeightOffset;
				}
				lastCardPosition = newPosition;
				orAddCardTileVisual.gameObject.transform.localPosition = newPosition;
				orAddCardTileVisual.MarkAsUsed();
				orAddCardTileVisual.Show();
				orAddCardTileVisual.SetSlot(currentDeck, slot, justChangedCardID.Equals(slot.CardID), offsetCardNameForRunes);
				currentDeckTileIndex++;
			}
		}
		Hide();
		if (currentDeck.FormatType == FormatType.FT_TWIST)
		{
			SetTwistRulesIndicatorVisible(visible: true);
		}
		else
		{
			SetTwistRulesIndicatorVisible(visible: false);
		}
		ShowDeckHelpButtonIfNeeded();
		FireCardCountChangedEvent();
		if (m_scrollbar != null)
		{
			m_scrollbar.UpdateScroll();
		}
		if (updateHighlight && !m_isSideboardContent)
		{
			UpdateDeckCompleteHighlight();
		}
		UpdateAttentionGlowSlots();
		if (animateFromActor != null && base.gameObject.activeInHierarchy)
		{
			StartCoroutine(ShowAddCardAnimationAfterTrayLoads(animateFromActor, onCompleteCallback));
		}
		else
		{
			onCompleteCallback?.Invoke();
		}
	}

	private Vector3 GetCardTileOffset(CollectionDeck currentDeck)
	{
		_ = Vector3.zero;
		float flatOffset = 0f;
		if (currentDeck != null && CollectionManager.Get().IsEditingDeathKnightDeck() && !m_isSideboardContent)
		{
			flatOffset += RuneIndicatorSpacerHeight;
		}
		if (!m_isShowingFakeDeck && currentDeck != null && m_deckTemplateHelpButton != null)
		{
			bool isInEditDeckMode = true;
			if (SceneMgr.Get().IsInTavernBrawlMode())
			{
				isInEditDeckMode = TavernBrawlDisplay.IsTavernBrawlEditing();
			}
			if (isInEditDeckMode && currentDeck.GetTotalInvalidCardCount(null) > 0 && !RankMgr.IsTwistDeckWithNoSeason(currentDeck) && (!RankMgr.IsCurrentTwistSeasonUsingHeroicDecks() || currentDeck.FormatType != FormatType.FT_TWIST))
			{
				flatOffset += TemplateDeckHelpButtonHeight;
			}
		}
		if (currentDeck.FormatType == FormatType.FT_TWIST)
		{
			flatOffset += m_twistRulesHeaderOffsetPosY;
		}
		return Vector3.down * flatOffset + m_cardTileOffset;
	}

	public void TriggerCardCountUpdate()
	{
		FireCardCountChangedEvent();
	}

	public void SetRuneIndicatorSpacerVisible(bool visible)
	{
		if (m_runeIndicatorSpacer != null)
		{
			m_runeIndicatorSpacer.SetActive(visible);
		}
	}

	public void HideDeckHelpPopup()
	{
		if (m_deckHelpPopup != null)
		{
			NotificationManager.Get().DestroyNotificationNowWithNoAnim(m_deckHelpPopup);
		}
	}

	public CollectionDeckSlot FindInvalidSlot()
	{
		return GetEditingDeck()?.FindInvalidSlot(null);
	}

	private static bool ShouldShowDeckCompleteHighlight(CollectionDeck deck)
	{
		DeckType type = deck.Type;
		if (type == DeckType.CLIENT_ONLY_DECK || type == DeckType.DRAFT_DECK)
		{
			return false;
		}
		return true;
	}

	public void UpdateDeckCompleteHighlight()
	{
		CollectionDeck currentDeck = GetEditingDeck();
		if (currentDeck == null || !ShouldShowDeckCompleteHighlight(currentDeck))
		{
			return;
		}
		CollectionDeck.CardCountByStatus cardCount = currentDeck.CountCardsByStatus(null);
		bool isValidDeck = cardCount.Valid == cardCount.Max && cardCount.Extra == 0;
		if (m_scrollbar != null && m_LockedScrollBounds != null && currentDeck.Locked)
		{
			m_scrollbar.m_ScrollBounds.center = m_LockedScrollBounds.center;
			m_scrollbar.m_ScrollBounds.size = m_LockedScrollBounds.size;
		}
		if (m_deckCompleteHighlight != null)
		{
			if (currentDeck.Locked)
			{
				m_deckCompleteHighlight.SetActive(value: false);
			}
			else
			{
				m_deckCompleteHighlight.SetActive(isValidDeck);
			}
		}
		if (isValidDeck && !Options.Get().GetBool(Option.HAS_FINISHED_A_DECK, defaultVal: false))
		{
			Options.Get().SetBool(Option.HAS_FINISHED_A_DECK, val: true);
		}
	}

	public Notification GetDeckHelpPopup()
	{
		return m_deckHelpPopup;
	}

	public void SetTwistRulesIndicatorVisible(bool visible)
	{
		if (m_twistRulesIndicator == null)
		{
			return;
		}
		if (visible)
		{
			Vector3 templateOffset = Vector3.zero;
			if (CollectionManager.Get().IsEditingDeathKnightDeck() && !m_isSideboardContent && m_runeIndicatorSpacer != null)
			{
				templateOffset = m_twistRulesHeaderOffsetPosYDeathKnight * Vector3.down;
			}
			m_twistRulesIndicator.gameObject.transform.localPosition = m_originalTwistRulesIndicatorPosition + templateOffset;
		}
		if ((bool)UniversalInputManager.UsePhoneUI)
		{
			AdjustScrollSettingsForTwist(visible);
		}
		m_twistRulesIndicator.SetActive(visible);
	}

	private IEnumerator ShowAddCardAnimationAfterTrayLoads(Actor cardToAnimate, Action onCompleteCallback)
	{
		string cardID = cardToAnimate.GetEntityDef().GetCardId();
		DeckTrayDeckTileVisual tile = GetCardTileVisual(cardID);
		Vector3 cardPos = cardToAnimate.transform.position;
		while (tile == null)
		{
			yield return null;
			tile = GetCardTileVisual(cardID);
		}
		GameObject cardTileObject = UnityEngine.Object.Instantiate(tile.GetActor().gameObject);
		Actor movingCardTile = cardTileObject.GetComponent<Actor>();
		if (GetEditingDeck().GetCardCountAllMatchingSlots(cardID) == 1)
		{
			tile.Hide();
		}
		else
		{
			tile.Show();
		}
		movingCardTile.transform.position = new Vector3(cardPos.x, cardPos.y + 2.5f, cardPos.z);
		if ((bool)UniversalInputManager.UsePhoneUI)
		{
			movingCardTile.transform.localScale = new Vector3(1.4f, 1.4f, 1.4f);
		}
		else
		{
			movingCardTile.transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);
		}
		movingCardTile.ActivateAllSpellsDeathStates();
		movingCardTile.ActivateSpellBirthState(SpellType.SUMMON_IN_LARGE);
		if ((bool)UniversalInputManager.UsePhoneUI && m_phoneDeckTileBone != null)
		{
			iTween.MoveTo(cardTileObject, iTween.Hash("position", m_phoneDeckTileBone.transform.position, "time", 0.5f, "easetype", iTween.EaseType.easeInCubic, "oncomplete", (Action<object>)delegate
			{
				tile.ShowAndSetupActor();
				tile.GetActor().GetSpell(SpellType.SUMMON_IN).ActivateState(SpellStateType.BIRTH);
				StartCoroutine(FinishPhoneMovingCardTile(cardTileObject, movingCardTile, 1f));
				if (onCompleteCallback != null)
				{
					onCompleteCallback();
				}
			}));
			iTween.ScaleTo(cardTileObject, iTween.Hash("scale", new Vector3(0.5f, 1.1f, 1.1f), "time", 0.5f, "easetype", iTween.EaseType.easeInCubic));
		}
		else
		{
			Vector3[] newPath = new Vector3[3];
			Vector3 startSpot = movingCardTile.transform.position;
			newPath[0] = startSpot;
			iTween.ValueTo(cardTileObject, iTween.Hash("from", 0f, "to", 1f, "time", 0.75f, "easetype", iTween.EaseType.easeOutCirc, "onupdate", (Action<object>)delegate(object val)
			{
				Vector3 position = tile.transform.position;
				newPath[1] = new Vector3((startSpot.x + position.x) * 0.5f, (startSpot.y + position.y) * 0.5f + 60f, (startSpot.z + position.z) * 0.5f);
				newPath[2] = position;
				iTween.PutOnPath(cardTileObject, newPath, (float)val);
			}, "oncomplete", (Action<object>)delegate
			{
				tile.ShowAndSetupActor();
				tile.GetActor().GetSpell(SpellType.SUMMON_IN).ActivateState(SpellStateType.BIRTH);
				movingCardTile.Hide();
				UnityEngine.Object.Destroy(cardTileObject);
				if (onCompleteCallback != null)
				{
					onCompleteCallback();
				}
			}));
		}
		SoundManager.Get().LoadAndPlay("collection_manager_card_add_to_deck_instant.prefab:06df359c4026d7e47b06a4174f33e3ef", base.gameObject);
	}

	private IEnumerator FinishPhoneMovingCardTile(GameObject obj, Actor movingCardTile, float delay)
	{
		yield return new WaitForSeconds(delay);
		movingCardTile.Hide();
		UnityEngine.Object.Destroy(obj);
	}

	private IEnumerator ShowDeckCompleteEffectsWithInterval(float interval)
	{
		if (m_scrollbar == null)
		{
			yield break;
		}
		bool needScroll = m_scrollbar.IsScrollNeeded();
		if (needScroll)
		{
			m_scrollbar.Enable(enable: false);
			m_scrollbar.ForceVisibleAffectedObjectsShow(show: true);
			m_scrollbar.SetScroll(0f, iTween.EaseType.easeOutSine, 0.25f, blockInputWhileScrolling: true);
			yield return new WaitForSeconds(0.3f);
			m_scrollbar.SetScroll(1f, iTween.EaseType.easeInOutQuart, interval * (float)m_cardTiles.Count, blockInputWhileScrolling: true);
		}
		List<DeckTrayDeckTileVisual> cardTiles = new List<DeckTrayDeckTileVisual>(m_cardTiles);
		foreach (DeckTrayDeckTileVisual tile in cardTiles)
		{
			if (!(tile == null) && tile.IsInUse())
			{
				tile.GetActor().ActivateSpellBirthState(SpellType.SUMMON_IN_FORGE);
				yield return new WaitForSeconds(interval);
			}
		}
		foreach (DeckTrayDeckTileVisual tile2 in cardTiles)
		{
			if (!(tile2 == null) && tile2.IsInUse())
			{
				yield return new WaitForSeconds(interval);
				tile2.GetActor().DeactivateAllSpells();
			}
		}
		if (needScroll)
		{
			m_scrollbar.ForceVisibleAffectedObjectsShow(show: false);
			m_scrollbar.EnableIfNeeded();
		}
	}

	private void IsCardTileVisible(GameObject obj, bool visible)
	{
		if (obj.activeSelf != visible && obj.TryGetComponent<DeckTrayDeckTileVisual>(out var tile))
		{
			if (visible && tile.IsInUse())
			{
				tile.ShowAndSetupActor();
			}
			else
			{
				tile.Hide();
			}
		}
	}

	private void ShowDeckEditingTipsIfNeeded()
	{
		if (Options.Get().GetBool(Option.HAS_REMOVED_CARD_FROM_DECK, defaultVal: false) || SceneMgr.Get().IsInTavernBrawlMode() || CollectionManager.Get().GetCollectibleDisplay() == null || CollectionManager.Get().GetCollectibleDisplay().GetViewMode() != 0 || m_cardTiles.Count <= 0)
		{
			return;
		}
		Transform bone = CollectionDeckTray.Get().m_removeCardTutorialBone;
		if (m_deckHelpPopup == null)
		{
			m_deckHelpPopup = NotificationManager.Get().CreatePopupText(UserAttentionBlocker.NONE, bone.position, bone.localScale, GameStrings.Get("GLUE_COLLECTION_TUTORIAL08"));
			if (m_deckHelpPopup != null)
			{
				m_deckHelpPopup.PulseReminderEveryXSeconds(3f);
			}
		}
	}

	private void ShowDeckHelpButtonIfNeeded()
	{
		bool showTemplateButton = false;
		bool showCompleteButton = false;
		if (CollectionManager.Get().GetCollectibleDisplay() == null)
		{
			return;
		}
		CollectionDeck currentDeck = GetEditingDeck();
		if (currentDeck == null || (Options.GetFormatType() == FormatType.FT_TWIST && (RankMgr.IsTwistDeckWithNoSeason(currentDeck) || RankMgr.IsCurrentTwistSeasonUsingHeroicDecks())))
		{
			return;
		}
		if (DeckHelper.Get() != null && currentDeck.GetTotalValidCardCount(null) < CollectionManager.Get().GetDeckSize())
		{
			showCompleteButton = true;
		}
		if (currentDeck.GetTotalInvalidCardCount(null) > 0)
		{
			showCompleteButton = false;
			showTemplateButton = true;
		}
		else
		{
			showTemplateButton = false;
		}
		if (CollectionManager.Get().GetCollectibleDisplay().GetViewMode() == CollectionUtils.ViewMode.DECK_TEMPLATE)
		{
			showTemplateButton = false;
			showCompleteButton = false;
		}
		if (TavernBrawlDisplay.IsTavernBrawlViewing())
		{
			showTemplateButton = false;
			showCompleteButton = false;
		}
		if (!DeckHelper.HasChoicesToOffer(currentDeck))
		{
			showCompleteButton = false;
		}
		showCompleteButton = showCompleteButton && !m_isSideboardContent;
		if (m_smartDeckCompleteButton != null)
		{
			m_smartDeckCompleteButton.gameObject.SetActive(showCompleteButton);
			if (showCompleteButton)
			{
				m_smartDeckCompleteButton.transform.localPosition = GetPositionOfLastCardTile(currentDeck);
			}
		}
		if (m_deckTemplateHelpButton != null)
		{
			m_deckTemplateHelpButton.gameObject.SetActive(showTemplateButton);
		}
		if (showTemplateButton)
		{
			Vector3 newPosition = m_deckTemplateHelpButtonOriginalLocalPosition;
			if (!m_isSideboardContent)
			{
				if (CollectionManager.Get().IsEditingDeathKnightDeck())
				{
					newPosition.y = m_deckTemplateHelpButtonDeathKnightPosY;
				}
				else
				{
					newPosition = m_deckTemplateHelpButtonOriginalLocalPosition;
				}
				if (currentDeck.FormatType == FormatType.FT_TWIST)
				{
					if (CollectionManager.Get().IsEditingDeathKnightDeck())
					{
						newPosition.y += m_twistRulesHeaderOffsetPosYReplaceCardAndDeathKnight;
					}
					else
					{
						newPosition.y += m_twistRulesHeaderOffsetPosYReplaceCard;
					}
				}
			}
			m_deckTemplateHelpButton.transform.localPosition = newPosition;
		}
		if (Options.Get().GetBool(Option.HAS_FINISHED_A_DECK, defaultVal: false))
		{
			return;
		}
		if (m_smartDeckCompleteButton != null)
		{
			HighlightState highlight = m_smartDeckCompleteButton.GetComponentInChildren<HighlightState>();
			if (highlight != null)
			{
				highlight.ChangeState(ActorStateType.HIGHLIGHT_PRIMARY_ACTIVE);
			}
		}
		if (m_deckTemplateHelpButton != null)
		{
			HighlightState highlight2 = m_deckTemplateHelpButton.GetComponentInChildren<HighlightState>();
			if (highlight2 != null)
			{
				highlight2.ChangeState(ActorStateType.HIGHLIGHT_PRIMARY_ACTIVE);
			}
		}
	}

	private Vector3 GetPositionOfLastCardTile(CollectionDeck deck)
	{
		Vector3 setPos = GetCardTileOffset(deck);
		foreach (CollectionDeckSlot slot in deck.GetSlots())
		{
			EntityDef entityDef = slot.GetEntityDef();
			if (entityDef != null && entityDef.HasSideboard)
			{
				setPos.y -= m_cardTileSlotLocalHeightSideboard;
			}
			else
			{
				setPos.y -= m_cardTileSlotLocalHeight;
			}
		}
		return setPos;
	}

	private void OnDeckTemplateHelpButtonPress(UIEvent e)
	{
		Options.Get().SetBool(Option.HAS_CLICKED_DECK_TEMPLATE_REPLACE, val: true);
		CollectionDeckSlot slotToReplace = FindInvalidSlot();
		ShowDeckHelper(slotToReplace, replaceSingleSlotOnly: false);
	}

	private void OnDeckTemplateHelpButtonOver(UIEvent e)
	{
		HighlightState highlight = m_deckTemplateHelpButton.GetComponentInChildren<HighlightState>();
		if (highlight != null)
		{
			if (!Options.Get().GetBool(Option.HAS_FINISHED_A_DECK, defaultVal: false))
			{
				highlight.ChangeState(ActorStateType.HIGHLIGHT_PRIMARY_ACTIVE);
			}
			else
			{
				highlight.ChangeState(ActorStateType.HIGHLIGHT_MOUSE_OVER);
			}
		}
		SoundManager.Get().LoadAndPlay("Small_Mouseover.prefab:692610296028713458ea58bc34adb4c9", base.gameObject);
	}

	private void OnDeckTemplateHelpButtonOut(UIEvent e)
	{
		HighlightState highlight = m_deckTemplateHelpButton.GetComponentInChildren<HighlightState>();
		if (highlight != null)
		{
			if (!Options.Get().GetBool(Option.HAS_CLICKED_DECK_TEMPLATE_REPLACE, defaultVal: false))
			{
				highlight.ChangeState(ActorStateType.HIGHLIGHT_PRIMARY_ACTIVE);
			}
			else
			{
				highlight.ChangeState(ActorStateType.NONE);
			}
		}
	}

	private void OnDeckCompleteButtonPress(UIEvent e)
	{
		SoundManager.Get().LoadAndPlay("Small_Click.prefab:2a1c5335bf08dc84eb6e04fc58160681", base.gameObject);
		if (CollectionDeckTray.Get() != null)
		{
			CollectionDeckTray.Get().CompleteMyDeckButtonPress();
		}
	}

	private void OnDeckCompleteButtonOver(UIEvent e)
	{
		if (!CollectionInputMgr.Get().HasHeldCard())
		{
			HighlightState highlight = m_smartDeckCompleteButton.GetComponentInChildren<HighlightState>();
			if (highlight != null)
			{
				highlight.ChangeState(ActorStateType.HIGHLIGHT_MOUSE_OVER);
			}
			SoundManager.Get().LoadAndPlay("Small_Mouseover.prefab:692610296028713458ea58bc34adb4c9", base.gameObject);
		}
	}

	private void OnDeckCompleteButtonOut(UIEvent e)
	{
		HighlightState highlight = m_smartDeckCompleteButton.GetComponentInChildren<HighlightState>();
		if (highlight != null)
		{
			highlight.ChangeState(ActorStateType.NONE);
		}
	}

	private void OnSideboardDoneButtonPress(UIEvent e)
	{
		DeckTrayCardListContent.DoneButtonPressed?.Invoke();
	}

	private void OnSideboardDoneButtonOver(UIEvent e)
	{
		if (!CollectionInputMgr.Get().HasHeldCard())
		{
			HighlightState highlight = m_sideboardDoneButton.GetComponentInChildren<HighlightState>();
			if (highlight != null)
			{
				highlight.ChangeState(ActorStateType.HIGHLIGHT_MOUSE_OVER);
			}
			SoundManager.Get().LoadAndPlay("Small_Mouseover.prefab:692610296028713458ea58bc34adb4c9", base.gameObject);
		}
	}

	private void OnSideboardDoneButtonOut(UIEvent e)
	{
		HighlightState highlight = m_sideboardDoneButton.GetComponentInChildren<HighlightState>();
		if (highlight != null)
		{
			highlight.ChangeState(ActorStateType.NONE);
		}
	}

	private void LoadCardPrefabs(List<CollectionDeckSlot> deckSlots)
	{
		if (deckSlots.Count == 0)
		{
			return;
		}
		int prefabsToLoad = deckSlots.Count;
		m_loading = true;
		m_cardDefs.DisposeValuesAndClear();
		for (int i = 0; i < deckSlots.Count; i++)
		{
			CollectionDeckSlot slot = deckSlots[i];
			if (slot.Count == 0)
			{
				Log.DeckTray.Print($"DeckTrayCardListContent.LoadCardPrefabs(): Slot {i} of deck is empty! Skipping...");
				continue;
			}
			DefLoader.Get().LoadCardDef(slot.CardID, delegate(string cardId, DefLoader.DisposableCardDef def, object userData)
			{
				m_cardDefs.Add(def);
				int num = prefabsToLoad - 1;
				prefabsToLoad = num;
				if (prefabsToLoad == 0)
				{
					m_loading = false;
				}
			}, null, new CardPortraitQuality(1, TAG_PREMIUM.NORMAL));
		}
	}

	private Vector3 GetOffscreenLocalPosition()
	{
		Vector3 newLocalPos = m_originalLocalPosition;
		CollectionDeck currDeck = GetEditingDeck();
		int cardCount = ((currDeck != null) ? (currDeck.GetSlotCount() + 2) : 0);
		int sideboardCards = currDeck?.GetNumberOfCardsWithSideboards() ?? 0;
		float cardListHeight = m_cardTileHeight * (float)(cardCount + sideboardCards);
		newLocalPos.z -= cardListHeight - GetCardTileOffset(currDeck).y / m_cardTileSlotLocalScaleVec3.y;
		return newLocalPos;
	}

	private void AdjustScrollSettingsForTwist(bool isTwistHeaderVisible)
	{
		if (!(m_deckTrayScrollable == null))
		{
			if (isTwistHeaderVisible)
			{
				m_deckTrayScrollable.ScrollBottomPadding = m_twistScrollBottomPaddingWithHeader;
			}
			else
			{
				m_deckTrayScrollable.ScrollBottomPadding = m_twistScrollBottomPaddingWithoutHeader;
			}
		}
	}

	public void RegisterCardTileHeldListener(CardTileHeld dlg)
	{
		m_cardTileHeldListeners.Add(dlg);
	}

	public void RegisterCardTilePressListener(CardTilePress dlg)
	{
		m_cardTilePressListeners.Add(dlg);
	}

	public void RegisterCardTileTapListener(CardTileTap dlg)
	{
		m_cardTileTapListeners.Add(dlg);
	}

	public void RegisterCardTileOverListener(CardTileOver dlg)
	{
		m_cardTileOverListeners.Add(dlg);
	}

	public void RegisterCardTileOutListener(CardTileOut dlg)
	{
		m_cardTileOutListeners.Add(dlg);
	}

	public void RegisterCardTileReleaseListener(CardTileRelease dlg)
	{
		m_cardTileReleaseListeners.Add(dlg);
	}

	public void RegisterCardTileRightClickedListener(CardTileRightClicked dlg)
	{
		m_cardTileRightClickedListeners.Add(dlg);
	}

	public void RegisterCardCountUpdated(CardCountChanged dlg)
	{
		m_cardCountChangedListeners.Add(dlg);
	}

	public void UnregisterCardTileHeldListener(CardTileHeld dlg)
	{
		m_cardTileHeldListeners.Remove(dlg);
	}

	public void UnregisterCardTileTapListener(CardTileTap dlg)
	{
		m_cardTileTapListeners.Remove(dlg);
	}

	public void UnregisterCardTilePressListener(CardTilePress dlg)
	{
		m_cardTilePressListeners.Remove(dlg);
	}

	public void UnregisterCardTileOverListener(CardTileOver dlg)
	{
		m_cardTileOverListeners.Remove(dlg);
	}

	public void UnregisterCardTileOutListener(CardTileOut dlg)
	{
		m_cardTileOutListeners.Remove(dlg);
	}

	public void UnregisterCardTileReleaseListener(CardTileRelease dlg)
	{
		m_cardTileReleaseListeners.Remove(dlg);
	}

	public void UnregisterCardTileRightClickedListener(CardTileRightClicked dlg)
	{
		m_cardTileRightClickedListeners.Remove(dlg);
	}

	public void UnregisterCardCountUpdated(CardCountChanged dlg)
	{
		m_cardCountChangedListeners.Remove(dlg);
	}

	private void FireCardTileDragEvent(DeckTrayDeckTileVisual cardTile)
	{
		CardTileHeld[] array = m_cardTileHeldListeners.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			array[i](cardTile);
		}
	}

	private void FireCardTilePressEvent(DeckTrayDeckTileVisual cardTile)
	{
		CardTilePress[] array = m_cardTilePressListeners.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			array[i](cardTile);
		}
	}

	private void FireCardTileTapEvent(DeckTrayDeckTileVisual cardTile)
	{
		CardTileTap[] array = m_cardTileTapListeners.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			array[i](cardTile);
		}
	}

	private void FireCardTileOverEvent(DeckTrayDeckTileVisual cardTile)
	{
		CardTileOver[] array = m_cardTileOverListeners.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			array[i](cardTile);
		}
	}

	private void FireCardTileOutEvent(DeckTrayDeckTileVisual cardTile)
	{
		CardTileOut[] array = m_cardTileOutListeners.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			array[i](cardTile);
		}
	}

	private void FireCardTileReleaseEvent(DeckTrayDeckTileVisual cardTile)
	{
		CardTileRelease[] array = m_cardTileReleaseListeners.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			array[i](cardTile);
		}
	}

	private void FireCardTileRightClickedEvent(DeckTrayDeckTileVisual cardTile)
	{
		CardTileRightClicked[] array = m_cardTileRightClickedListeners.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			array[i](cardTile);
		}
	}

	private void FireCardCountChangedEvent()
	{
		int currentCardCount = GetEditingDeck()?.GetTotalCardCount() ?? 0;
		int maxCardCount = GetEditingDeck()?.GetMaxCardCount() ?? 0;
		CardCountChanged[] array = m_cardCountChangedListeners.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			array[i](currentCardCount, maxCardCount);
		}
	}
}
