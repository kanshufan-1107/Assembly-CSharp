using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Hearthstone.UI;
using PegasusShared;
using UnityEngine;

public class CollectionDeckTray : EditableDeckTray
{
	[Serializable]
	public class CollectionCardEventHandlerData
	{
		public string CardID;

		public CollectionCardEventHandler CardHandlerPrefab;

		private CollectionCardEventHandler cardHandlerInstance;

		public CollectionCardEventHandler GetInstance()
		{
			return cardHandlerInstance;
		}

		public void SetInstance(CollectionCardEventHandler instance)
		{
			cardHandlerInstance = instance;
		}
	}

	[Serializable]
	public class CollectionTagEventHandlerData
	{
		public GAME_TAG Tag;

		public CollectionCardEventHandler cardHandlerInstance;
	}

	public delegate void PopuplateDeckCompleteCallback(List<EntityDef> addedCards, List<EntityDef> removedCards);

	public DeckTrayDeckListContent m_decksContent;

	[SerializeField]
	private WidgetInstance m_cosmeticCoinWidget;

	[SerializeField]
	private WidgetInstance m_cardBackWidget;

	[SerializeField]
	private WidgetInstance m_heroSkinWidget;

	[SerializeField]
	private DeckTrayTeamListContent m_teamsContent;

	[SerializeField]
	private DeckTrayMercListContent m_mercContent;

	[SerializeField]
	private RuneIndicatorVisual m_runeIndicatorVisual;

	[SerializeField]
	private WidgetInstance m_ETCSideboardTrayWidgetInstance;

	[SerializeField]
	private WidgetInstance m_zilliaxSideboardTrayWidgetInstance;

	private DeckSideboardTray m_ETCsideboardTray;

	private ZilliaxDeckSideboardTray m_ZilliaxsideboardTray;

	private DeckSideboardTray m_currentSideboardTray;

	[SerializeField]
	private List<CollectionCardEventHandlerData> m_cardEventHandlers;

	[SerializeField]
	private List<CollectionTagEventHandlerData> m_tagCardEventHandlers;

	private bool m_isAutoAddingCardsWithTiming;

	public GameObject TrayContentsContainer;

	public GameObject TrayContentsDuelsBone;

	public Transform m_removeCardTutorialBone;

	public PlayMakerFSM m_deckTemplateChosenGlow;

	private static CollectionDeckTray s_instance;

	private DeckTrayCosmeticCoinContent m_cosmeticCoinContent;

	private DeckTrayCardBackContent m_cardBackContent;

	private DeckTrayHeroSkinContent m_heroSkinContent;

	private CollectionCardEventHandler m_defaultCardEventHandler;

	private const string ETCCardId = "ETC_080";

	private const string SideboardTutorialVOLine = "GLUE_ETC_FTUE_QUOTE";

	public float SideboardPopupOffsetX = 16f;

	public float SideboardPopupScale = 10f;

	public float SideboardPopupDelay = 5f;

	private Notification m_sideboardCardPopup;

	public bool IsSideboardOpen
	{
		get
		{
			if ((bool)m_currentSideboardTray)
			{
				return m_currentSideboardTray.IsShowing;
			}
			return false;
		}
	}

	public bool IsAutoAddingCardsWithTiming => m_isAutoAddingCardsWithTiming;

	public bool IsShowingSideboardPopup => m_sideboardCardPopup != null;

	public static event Action<CollectionDeck, RunePattern> DeckTrayRunesAdded;

	public static event Action<DeckTrayDeckTileVisual> SideboardCardTileRightClicked;

	public static event Action<EntityDef> OnCardAddedEvent;

	private void Awake()
	{
		s_instance = this;
		if (base.gameObject.GetComponent<AudioSource>() == null)
		{
			base.gameObject.AddComponent<AudioSource>();
		}
		if (m_scrollbar != null)
		{
			if (SceneMgr.Get().IsInTavernBrawlMode() && !UniversalInputManager.UsePhoneUI)
			{
				Vector3 center = m_scrollbar.m_ScrollBounds.center;
				center.z = 3f;
				m_scrollbar.m_ScrollBounds.center = center;
				Vector3 size = m_scrollbar.m_ScrollBounds.size;
				size.z = 47.67f;
				m_scrollbar.m_ScrollBounds.size = size;
				if (m_cardsContent != null && m_cardsContent.m_deckCompleteHighlight != null)
				{
					Vector3 position = m_cardsContent.m_deckCompleteHighlight.transform.localPosition;
					position.z = -34.15f;
					m_cardsContent.m_deckCompleteHighlight.transform.localPosition = position;
				}
			}
			m_scrollbar.Enable(enable: false);
			m_scrollbar.AddTouchScrollStartedListener(base.OnTouchScrollStarted);
			m_scrollbar.AddTouchScrollEndedListener(base.OnTouchScrollEnded);
		}
		if (m_decksContent != null)
		{
			m_contents[DeckContentTypes.Decks] = m_decksContent;
			m_decksContent.RegisterBusyWithDeck(base.OnBusyWithDeck);
			if (!SceneMgr.Get().IsInTavernBrawlMode())
			{
				m_decksContent.RegisterDeckCountUpdated(OnDeckCountUpdated);
			}
		}
		if (m_cosmeticCoinWidget != null)
		{
			m_cosmeticCoinWidget.RegisterReadyListener(delegate
			{
				m_cosmeticCoinContent = m_cosmeticCoinWidget.gameObject.GetComponentInChildren<DeckTrayCosmeticCoinContent>();
				if (m_cosmeticCoinContent != null)
				{
					m_contents[DeckContentTypes.Coin] = m_cosmeticCoinContent;
				}
			});
		}
		if (m_cardBackWidget != null)
		{
			m_cardBackWidget.RegisterReadyListener(delegate
			{
				m_cardBackContent = m_cardBackWidget.gameObject.GetComponentInChildren<DeckTrayCardBackContent>();
				if (m_cardBackContent != null)
				{
					m_contents[DeckContentTypes.CardBack] = m_cardBackContent;
				}
			});
		}
		if (m_heroSkinWidget != null)
		{
			m_heroSkinWidget.RegisterReadyListener(delegate
			{
				m_heroSkinContent = m_heroSkinWidget.gameObject.GetComponentInChildren<DeckTrayHeroSkinContent>();
				if (m_heroSkinContent != null)
				{
					m_contents[DeckContentTypes.HeroSkin] = m_heroSkinContent;
					m_heroSkinContent.OnHeroChanged += OnHeroAssigned;
				}
			});
		}
		if (m_cardsContent != null)
		{
			m_contents[DeckContentTypes.Cards] = m_cardsContent;
			m_cardsContent.RegisterCardTileHeldListener(OnCardTileHeld);
			m_cardsContent.RegisterCardTilePressListener(OnCardTilePress);
			m_cardsContent.RegisterCardTileTapListener(OnCardTileTap);
			m_cardsContent.RegisterCardTileOverListener(OnCardTileOver);
			m_cardsContent.RegisterCardTileOutListener(OnCardTileOut);
			m_cardsContent.RegisterCardTileReleaseListener(OnCardTileRelease);
			m_cardsContent.RegisterCardCountUpdated(OnCardCountUpdated);
		}
		if (m_teamsContent != null)
		{
			m_contents[DeckContentTypes.Teams] = m_teamsContent;
			m_teamsContent.RegisterBusyWithTeam(base.OnBusyWithDeck);
			m_teamsContent.RegisterTeamCountUpdated(OnTeamCountUpdated);
		}
		if (m_mercContent != null)
		{
			m_contents[DeckContentTypes.Mercs] = m_mercContent;
			m_mercContent.RegisterMercCountUpdated(OnMercCountUpdated);
		}
		string myDecksLabelGlue = "GLUE_COLLECTION_MY_DECKS";
		if (SceneMgr.Get().IsInTavernBrawlMode())
		{
			myDecksLabelGlue = ((TavernBrawlManager.Get().CurrentSeasonBrawlMode == TavernBrawlMode.TB_MODE_HEROIC) ? "GLUE_HEROIC_BRAWL_DECK" : "GLUE_COLLECTION_DECK");
		}
		else if (SceneMgr.Get().IsInLettuceMode())
		{
			myDecksLabelGlue = "GLUE_COLLECTION_MY_TEAMS";
		}
		SetMyDecksLabelText(GameStrings.Get(myDecksLabelGlue));
		m_doneButton.SetText(GameStrings.Get("GLOBAL_BACK"));
		m_doneButton.AddEventListener(UIEventType.RELEASE, DoneButtonPress);
		if (SceneMgr.Get().IsInLettuceMode())
		{
			CollectionManager.Get().RegisterEditingTeamChanged(OnEditingTeamChanged);
		}
		else
		{
			CollectionManager.Get().RegisterEditedDeckChanged(OnEditedDeckChanged);
		}
		SceneMgr.Get().RegisterScenePreUnloadEvent(OnScenePreUnload);
		CollectionInputMgr.Get().SetScrollbar(m_scrollbar);
		foreach (DeckContentScroll scrollable in m_scrollables)
		{
			scrollable.SaveStartPosition();
		}
		m_defaultCardEventHandler = base.gameObject.AddComponent<CollectionCardEventHandler>();
	}

	protected override void Start()
	{
		CollectionManager cm = CollectionManager.Get();
		if (cm == null)
		{
			Log.CollectionManager.PrintError("CollectionDeckTray.Start - CollectionManager is null!");
			return;
		}
		CollectibleDisplay cd = cm.GetCollectibleDisplay();
		if (cd == null)
		{
			Log.CollectionManager.PrintError("CollectionDeckTray.Start - CollectibleDisplay is null!");
			return;
		}
		cd.UpdateCurrentPageCardLocks(playSound: true);
		cd.OnViewModeChanged += OnCMViewModeChanged;
		Navigation.Push(OnBackOutOfCollectionScreen);
		cm.RegisterDeckCreatedListener(OnDeckCreated);
		cm.RegisterTeamCreatedListener(OnTeamCreated);
		base.Start();
		InitializeETCSideboardTrayWidget();
		InitializeZilliaxSideboardTrayWidget();
	}

	private void InitializeETCSideboardTrayWidget()
	{
		if ((bool)m_ETCSideboardTrayWidgetInstance)
		{
			m_ETCSideboardTrayWidgetInstance.Initialize();
			m_ETCSideboardTrayWidgetInstance.RegisterReadyListener(delegate
			{
				m_ETCsideboardTray = m_ETCSideboardTrayWidgetInstance.GetComponentInChildren<DeckSideboardTray>();
				m_ETCsideboardTray.CardsContent.RegisterCardTileHeldListener(OnCardTileHeld);
				m_ETCsideboardTray.CardsContent.RegisterCardTilePressListener(OnCardTilePress);
				m_ETCsideboardTray.CardsContent.RegisterCardTileTapListener(OnCardTileTap);
				m_ETCsideboardTray.CardsContent.RegisterCardTileOverListener(OnCardTileOver);
				m_ETCsideboardTray.CardsContent.RegisterCardTileOutListener(OnCardTileOut);
				m_ETCsideboardTray.CardsContent.RegisterCardTileReleaseListener(OnCardTileRelease);
				m_ETCsideboardTray.CardsContent.RegisterCardCountUpdated(OnSideboardCardCountUpdated);
				m_ETCsideboardTray.CardsContent.RegisterCardTileRightClickedListener(OnSideboardCardTileRightClick);
			});
		}
	}

	private void InitializeZilliaxSideboardTrayWidget()
	{
		if ((bool)m_zilliaxSideboardTrayWidgetInstance)
		{
			m_zilliaxSideboardTrayWidgetInstance.Initialize();
			m_zilliaxSideboardTrayWidgetInstance.RegisterReadyListener(delegate
			{
				m_ZilliaxsideboardTray = m_zilliaxSideboardTrayWidgetInstance.GetComponentInChildren<ZilliaxDeckSideboardTray>();
				m_ZilliaxsideboardTray.CardsContent.RegisterCardTileHeldListener(OnCardTileHeld);
				m_ZilliaxsideboardTray.CardsContent.RegisterCardTilePressListener(OnCardTilePress);
				m_ZilliaxsideboardTray.CardsContent.RegisterCardTileTapListener(OnCardTileTap);
				m_ZilliaxsideboardTray.CardsContent.RegisterCardTileOverListener(OnCardTileOver);
				m_ZilliaxsideboardTray.CardsContent.RegisterCardTileOutListener(OnCardTileOut);
				m_ZilliaxsideboardTray.CardsContent.RegisterCardTileReleaseListener(OnCardTileRelease);
				m_ZilliaxsideboardTray.CardsContent.RegisterCardCountUpdated(OnSideboardCardCountUpdated);
				m_ZilliaxsideboardTray.CardsContent.RegisterCardTileRightClickedListener(OnSideboardCardTileRightClick);
			});
		}
	}

	private void OnDestroy()
	{
		CollectionManager cm = CollectionManager.Get();
		if (cm != null && SceneMgr.Get() != null)
		{
			if (SceneMgr.Get().IsInLettuceMode())
			{
				cm.RemoveEditingTeamChanged(OnEditingTeamChanged);
			}
			else
			{
				cm.RemoveEditedDeckChanged(OnEditedDeckChanged);
			}
			cm.DoneEditing();
		}
		if (SceneMgr.Get() != null)
		{
			SceneMgr.Get().UnregisterScenePreUnloadEvent(OnScenePreUnload);
		}
		s_instance = null;
	}

	private void OnEnable()
	{
		RuneIndicatorVisual.RunePatternChanged += RuneIndicatorVisualOnRunePatternChanged;
		CollectionDeckTileActor.DeckTileSideboardButtonPressed += OnDeckTileSideboardButtonPressed;
		DeckTrayCardListContent.DoneButtonPressed += OnSideboardDoneButtonPressed;
	}

	private void OnDisable()
	{
		RuneIndicatorVisual.RunePatternChanged -= RuneIndicatorVisualOnRunePatternChanged;
		CollectionDeckTileActor.DeckTileSideboardButtonPressed -= OnDeckTileSideboardButtonPressed;
		DeckTrayCardListContent.DoneButtonPressed -= OnSideboardDoneButtonPressed;
	}

	public void OnSideboardDoneButtonPressed()
	{
		if (!(m_currentSideboardTray == null) && m_currentSideboardTray.OnSideboardDoneButtonPressed())
		{
			CloseSideboardTray();
		}
	}

	public void CloseSideboardTray()
	{
		if (m_currentSideboardTray != null)
		{
			m_currentSideboardTray.Hide();
			m_currentSideboardTray = null;
			CollectibleDisplay cd = CollectionManager.Get().GetCollectibleDisplay();
			if (cd.GetViewSubmode() == CollectionUtils.ViewSubmode.CARD_ZILLIAX_MODULES)
			{
				CollectiblePageManager pm = cd.GetPageManager();
				cd.UpdateCurrentPageCardLocks(playSound: true);
				cd.SetViewSubmode(CollectionUtils.ViewSubmode.DEFAULT);
				pm.UpdateVisibleTabs();
				pm.FlipToPage(1, null, null);
			}
			if (!UniversalInputManager.UsePhoneUI && m_scrollbar != null)
			{
				m_scrollbar.EnableIfNeeded();
			}
			CollectionManager.Get().GetCollectibleDisplay().UpdateCurrentPageCardLocks(playSound: true);
		}
	}

	private void OnDeckTileSideboardButtonPressed(CollectionDeckTileActor deckTile)
	{
		EntityDef entityDef = deckTile.GetEntityDef();
		if (entityDef == null)
		{
			return;
		}
		CollectionDeck editedDeck = CollectionManager.Get().GetEditedDeck();
		CollectionDeckSlot sideboardOwnerSlot = deckTile.GetSlot();
		if (!editedDeck.IsValidSlot(sideboardOwnerSlot, ignoreOwnership: false, ignoreGameplayEvent: false, enforceRemainingDeckRuleset: true, null))
		{
			m_cardsContent.ShowDeckHelper(sideboardOwnerSlot, replaceSingleSlotOnly: true);
			return;
		}
		switch (entityDef.GetTag<TAG_SIDEBOARD_TYPE>(GAME_TAG.SIDEBOARD_TYPE))
		{
		case TAG_SIDEBOARD_TYPE.ETC:
			if ((bool)m_ETCsideboardTray)
			{
				m_currentSideboardTray = m_ETCsideboardTray;
				SideboardDeck sideboardDeck2 = editedDeck.SetEditedSideboard(entityDef.GetCardId(), deckTile.GetPremium());
				sideboardDeck2.DataModel.HighlightEditButton = false;
				m_ETCsideboardTray.Show(sideboardDeck2);
				CollectionManager.Get().GetCollectibleDisplay().UpdateCurrentPageCardLocks(playSound: true);
				if (!UniversalInputManager.UsePhoneUI && m_scrollbar != null)
				{
					m_scrollbar.Enable(enable: false);
				}
			}
			break;
		case TAG_SIDEBOARD_TYPE.ZILLIAX:
			if ((bool)m_ZilliaxsideboardTray)
			{
				m_currentSideboardTray = m_ZilliaxsideboardTray;
				SideboardDeck sideboardDeck = editedDeck.SetEditedSideboard(entityDef.GetCardId(), deckTile.GetPremium());
				sideboardDeck.DataModel.HighlightEditButton = false;
				m_ZilliaxsideboardTray.Show(sideboardDeck);
				CollectibleDisplay collectibleDisplay = CollectionManager.Get().GetCollectibleDisplay();
				collectibleDisplay.SetViewSubmode(CollectionUtils.ViewSubmode.CARD_ZILLIAX_MODULES);
				collectibleDisplay.GetPageManager().UpdateVisibleTabs();
				collectibleDisplay.GetPageManager().FlipToPage(1, null, null);
				collectibleDisplay.UpdateCurrentPageCardLocks(playSound: true);
				if (!UniversalInputManager.UsePhoneUI && m_scrollbar != null)
				{
					m_scrollbar.Enable(enable: false);
				}
			}
			break;
		}
	}

	private void RuneIndicatorVisualOnRunePatternChanged(RunePattern runes)
	{
		m_cardsContent.UpdateTileVisuals();
	}

	protected override void OnEditedDeckChanged(CollectionDeck newDeck, CollectionDeck oldDeck, object callbackData)
	{
		CollectibleDisplay collectionDisplay = CollectionManager.Get().GetCollectibleDisplay();
		CollectionUtils.ViewMode viewMode = collectionDisplay.GetViewMode();
		CollectionPageManager pageManager = collectionDisplay.GetPageManager() as CollectionPageManager;
		bool isOpeningDeck = newDeck != null;
		if (viewMode == CollectionUtils.ViewMode.HERO_SKINS && !isOpeningDeck && !pageManager.IsSearching())
		{
			collectionDisplay.SetViewMode(CollectionUtils.ViewMode.HERO_PICKER, triggerResponse: false);
		}
		else if (viewMode == CollectionUtils.ViewMode.HERO_PICKER && isOpeningDeck)
		{
			collectionDisplay.SetViewMode(CollectionUtils.ViewMode.HERO_SKINS, triggerResponse: false);
		}
		if (isOpeningDeck)
		{
			newDeck.RemoveOrphanedSideboards();
		}
		base.OnEditedDeckChanged(newDeck, oldDeck, callbackData);
	}

	public bool CanPickupCard()
	{
		DeckContentTypes type = GetCurrentContentType();
		CollectionUtils.ViewMode viewMode = CollectionManager.Get().GetCollectibleDisplay().GetViewMode();
		if (((type != DeckContentTypes.Cards && type != DeckContentTypes.Mercs) || viewMode != 0) && (type != DeckContentTypes.CardBack || viewMode != CollectionUtils.ViewMode.CARD_BACKS) && (type != DeckContentTypes.HeroSkin || viewMode != CollectionUtils.ViewMode.HERO_SKINS))
		{
			if (type == DeckContentTypes.Coin)
			{
				return viewMode == CollectionUtils.ViewMode.COINS;
			}
			return false;
		}
		return true;
	}

	public static CollectionDeckTray Get()
	{
		return s_instance;
	}

	public void Unload()
	{
		CollectionManager.Get().RemoveDeckCreatedListener(OnDeckCreated);
		CollectionManager.Get().RemoveTeamCreatedListener(OnTeamCreated);
		CollectionInputMgr.Get().SetScrollbar(null);
	}

	public bool UpdateHeldCardVisual(CollectionDraggableCardVisual collectionDraggableCardVisual)
	{
		if (m_currentSideboardTray != null && m_currentSideboardTray.UpdateHeldCardVisual(collectionDraggableCardVisual))
		{
			return true;
		}
		return false;
	}

	public void StartDragWithActor(Actor actor, CollectionUtils.ViewMode viewMode, bool showVisual = true, CollectionDeckSlot slot = null)
	{
		if (m_currentSideboardTray != null)
		{
			m_currentSideboardTray.StartDragWithActor(actor, viewMode, showVisual, slot);
		}
	}

	public bool UpdateCurrentPageCardLocks(IEnumerable<CollectionCardVisual> collectionCardVisuals)
	{
		if (m_currentSideboardTray != null)
		{
			return m_currentSideboardTray.UpdateCurrentPageCardLocks(collectionCardVisuals);
		}
		return false;
	}

	private void OnScenePreUnload(SceneMgr.Mode prevMode, PegasusScene prevScene, object userData)
	{
		if (UniversalInputManager.Get() != null && UniversalInputManager.Get().IsTextInputActive())
		{
			UniversalInputManager.Get().CancelTextInput(base.gameObject, force: true);
		}
		if (CollectionManager.Get().IsInEditMode())
		{
			CollectionManager.Get().GetEditedDeck()?.SendChanges(CollectionDeck.ChangeSource.OnScenePreUnload);
		}
		else if (CollectionManager.Get().IsInEditTeamMode())
		{
			CollectionManager.Get().GetEditingTeam()?.SendChanges();
		}
		Exit();
	}

	private void TryToShowETCSideboardTutorial(string cardId)
	{
		if (cardId != "ETC_080")
		{
			return;
		}
		GameSaveDataManager.Get().GetSubkeyValue(GameSaveKeyId.FTUE, GameSaveKeySubkeyId.HAS_SEEN_ETC_SIDEBOARD_TUTORIAL, out long hasSeenETCTutorial);
		if (hasSeenETCTutorial > 0)
		{
			return;
		}
		UIVoiceLinesManager voiceLineManager = UIVoiceLinesManager.Get();
		if (voiceLineManager == null)
		{
			return;
		}
		voiceLineManager.ExecuteTrigger(UIVoiceLinesManager.UIVoiceLineCategory.COLLECTION_EVENT, UIVoiceLinesManager.TriggerType.CARD_ADDED_TO_DECK, -1, "ETC_080");
		CollectionDeck editedDeck = CollectionManager.Get().GetEditedDeck();
		DeckTrayDeckTileVisual tileVisual = SnapToCardInDeckTray("ETC_080", editedDeck);
		if ((bool)tileVisual)
		{
			UIVoiceLinesManager.VoiceLineFinished += delegate(UIVoiceLineItem vo, bool complete)
			{
				if (vo.m_StringToLocalize == "GLUE_ETC_FTUE_QUOTE")
				{
					ShowSideboardPopupForTutorial(tileVisual, GameStrings.Get("GLUE_ETC_FTUE_SIDEBOARD"));
				}
			};
		}
		GameSaveDataManager.Get().SaveSubkey(new GameSaveDataManager.SubkeySaveRequest(GameSaveKeyId.FTUE, GameSaveKeySubkeyId.HAS_SEEN_ETC_SIDEBOARD_TUTORIAL, 1L));
		SideboardDeck etcSideboard = editedDeck?.GetSideboard("ETC_080");
		if (etcSideboard != null)
		{
			etcSideboard.DataModel.HighlightEditButton = true;
		}
	}

	public DeckTrayDeckTileVisual SnapToCardInDeckTray(string cardId, CollectionDeck deck)
	{
		if (deck == null || cardId == string.Empty)
		{
			return null;
		}
		DeckTrayDeckTileVisual cardTileVisual = GetCardsContent().GetCardTileVisual(cardId);
		float totalSlotsCount = deck.GetSlotCount();
		float requiredScrollPosition = (float)cardTileVisual.GetSlot().Index / totalSlotsCount + 0.05f;
		m_scrollbar.SetScrollSnap(requiredScrollPosition);
		return cardTileVisual;
	}

	public void ShowSideboardPopupForTutorial(DeckTrayDeckTileVisual tileVisual, string popupText)
	{
		if ((bool)tileVisual)
		{
			if (m_sideboardCardPopup != null)
			{
				NotificationManager.Get().DestroyNotificationNowWithNoAnim(m_sideboardCardPopup);
			}
			Vector3 tilePosition = tileVisual.transform.position;
			tilePosition.x -= SideboardPopupOffsetX;
			NotificationManager notificationManager = NotificationManager.Get();
			m_sideboardCardPopup = notificationManager.CreatePopupText(UserAttentionBlocker.NONE, tilePosition, SideboardPopupScale * Vector3.one, popupText);
			m_sideboardCardPopup.ShowPopUpArrow(Notification.PopUpArrowDirection.Right);
			NotificationManager.Get().DestroyNotification(m_sideboardCardPopup, SideboardPopupDelay);
		}
	}

	private void OnCardAdded(string cardId)
	{
		TryToShowETCSideboardTutorial(cardId);
	}

	private bool AddCard(EntityDef cardEntityDef, TAG_PREMIUM premium, CollectionDeck deck, bool playSound, bool allowInvalid = false, Actor animateActor = null, params DeckRule.RuleType[] ignoreRules)
	{
		if (SceneMgr.Get().IsInLettuceMode())
		{
			return AddCardToTeam(cardEntityDef, playSound);
		}
		return AddCardToDeck(cardEntityDef, premium, playSound, deck, animateActor, allowInvalid, ignoreRules);
	}

	public bool AddCard(EntityDef cardEntityDef, TAG_PREMIUM premium, bool playSound, Actor animateActor = null, params DeckRule.RuleType[] ignoreRules)
	{
		return AddCard(cardEntityDef, premium, GetCurrentDeckContext(), playSound, allowInvalid: false, animateActor, ignoreRules);
	}

	public bool AddCard(EntityDef cardEntityDef, TAG_PREMIUM premium, bool playSound, bool allowInvalid, Actor animateActor = null, params DeckRule.RuleType[] ignoreRules)
	{
		return AddCard(cardEntityDef, premium, GetCurrentDeckContext(), playSound, allowInvalid, animateActor, ignoreRules);
	}

	private bool AddCardToDeck(EntityDef cardEntityDef, TAG_PREMIUM premium, bool playSound, CollectionDeck deck, Actor animateActor = null, bool allowInvalid = false, params DeckRule.RuleType[] ignoreRules)
	{
		if (deck == null)
		{
			return false;
		}
		string cardId = cardEntityDef.GetCardId();
		CollectionCardEventHandler addHandler = GetCardEventHandler(cardId);
		bool updateVisuals = addHandler.ShouldUpdateVisuals();
		DeckTrayCardListContent cardListContent = GetCardsContent();
		SideboardDeck sideboardDeck = deck as SideboardDeck;
		if (sideboardDeck != null)
		{
			DeckSideboardTray sideboardTray = GetSideboardTrayForSideboardType(sideboardDeck.SideboardType);
			if (sideboardTray != null)
			{
				cardListContent = sideboardTray.CardsContent;
			}
		}
		bool cardAdded = cardListContent.AddCard(cardEntityDef, premium, playSound, OnCardAdded, animateActor, updateVisuals, allowInvalid, ignoreRules);
		if (cardAdded)
		{
			addHandler.OnCardAdded(this, deck, cardEntityDef, premium, animateActor);
			if (sideboardDeck != null)
			{
				sideboardDeck.UpdateInvalidCardsData();
			}
			else
			{
				CollectionDeckTray.DeckTrayRunesAdded?.Invoke(deck, cardEntityDef.GetRuneCost());
			}
			CollectionDeckTray.OnCardAddedEvent?.Invoke(cardEntityDef);
		}
		if (!IsSideboardOpen && deck.CreatedFromShareableDeck == null && !deck.IsCreatedWithDeckComplete)
		{
			ShowExtraCardsPopupIfNeeded(deck, cardEntityDef);
			ShowExtraRunesPopupIfNeeded(deck, cardEntityDef);
			if (!cardAdded && cardEntityDef.HasTag(GAME_TAG.TOURIST) && deck.GetRuleset(null).HasMaxTouristsRule(out var maxTourists) && deck.GetCardCountHasTag(GAME_TAG.TOURIST) >= maxTourists)
			{
				GameplayErrorManager.Get().DisplayMessage(GameStrings.Format("GLUE_COLLECTION_MANAGER_ON_ADD_TOURIST_LIMIT_ERROR_TEXT", maxTourists));
			}
		}
		return cardAdded;
	}

	private void ShowExtraCardsPopupIfNeeded(CollectionDeck deck, EntityBase cardBeingAdded)
	{
		NetCache.NetCacheFeatures netObject = NetCache.Get().GetNetObject<NetCache.NetCacheFeatures>();
		if (netObject == null || !netObject.OvercappedDecksEnabled || CollectionManager.Get().HasSeenOvercappedDeckInfoPopup)
		{
			return;
		}
		int maxDeckSizeAfterAddingCard = ((!cardBeingAdded.HasTag(GAME_TAG.DECK_RULE_MOD_DECK_SIZE)) ? CollectionManager.Get().GetDeckSize() : cardBeingAdded.GetTag(GAME_TAG.DECK_RULE_MOD_DECK_SIZE));
		if (maxDeckSizeAfterAddingCard >= 30 && deck.GetTotalCardCount() == maxDeckSizeAfterAddingCard + 1)
		{
			AlertPopup.PopupInfo info = new AlertPopup.PopupInfo();
			info.m_headerText = GameStrings.Get("GLUE_COLLECTION_DECK_EXTRA_CARD_WARNING_HEADER");
			info.m_showAlertIcon = true;
			info.m_text = GameStrings.Format("GLUE_COLLECTION_DECK_EXTRA_CARD_WARNING_BODY", maxDeckSizeAfterAddingCard);
			info.m_alertTextAlignment = UberText.AlignmentOptions.Center;
			info.m_responseDisplay = AlertPopup.ResponseDisplay.CONFIRM;
			info.m_confirmText = GameStrings.Get("GLUE_COLLECTION_DECK_EXTRA_CARD_WARNING_CONFIRM");
			info.m_responseCallback = delegate
			{
				CollectionManager.Get().HasSeenOvercappedDeckInfoPopup = true;
			};
			DialogManager.Get().ShowPopup(info);
		}
	}

	private static void ShowExtraRunesPopupIfNeeded(CollectionDeck deck, EntityBase cardBeingAdded)
	{
		if (!CollectionManager.Get().HasSeenExtraRunesDeckInfoPopup && !deck.CanAddRunes(cardBeingAdded.GetRuneCost(), DeckRule_DeathKnightRuneLimit.MaxRuneSlots))
		{
			AlertPopup.PopupInfo popupInfo = new AlertPopup.PopupInfo();
			popupInfo.m_headerText = GameStrings.Get("GLUE_COLLECTION_DECK_EXTRA_RUNES_WARNING_HEADER");
			popupInfo.m_showAlertIcon = true;
			popupInfo.m_text = GameStrings.Format("GLUE_COLLECTION_DECK_EXTRA_RUNES_WARNING_BODY", DeckRule_DeathKnightRuneLimit.MaxRuneSlots);
			popupInfo.m_alertTextAlignment = UberText.AlignmentOptions.Center;
			popupInfo.m_responseDisplay = AlertPopup.ResponseDisplay.CONFIRM;
			popupInfo.m_confirmText = GameStrings.Get("GLUE_COLLECTION_DECK_EXTRA_RUNES_WARNING_CONFIRM");
			popupInfo.m_responseCallback = delegate
			{
				CollectionManager.Get().HasSeenExtraRunesDeckInfoPopup = true;
			};
			AlertPopup.PopupInfo info = popupInfo;
			DialogManager.Get().ShowPopup(info);
		}
	}

	public bool AddCardToTeam(EntityDef cardEntityDef, bool playSound, int index = -1)
	{
		return GetMercsContent().AddMerc(cardEntityDef, playSound, null, updateVisuals: true, index);
	}

	private bool AddCardWithPreferredPremium(EntityDef cardEntityDef, bool playSound)
	{
		CollectionDeck deck = CollectionManager.Get().GetEditedDeck();
		TAG_PREMIUM? premium = deck.GetPreferredPremiumThatCanBeAdded(cardEntityDef.GetCardId()).GetValueOrDefault();
		return AddCard(cardEntityDef, premium.Value, playSound, true, null);
	}

	public void OnCardManuallyAddedByUser_CheckSuggestions(EntityDef cardEntityDef)
	{
		OnCardManuallyAddedByUser_CheckSuggestions(new EntityDef[1] { cardEntityDef });
	}

	public void OnCardManuallyAddedByUser_CheckSuggestions(IEnumerable<EntityDef> cardEntityDefs)
	{
		EntityDef cardEntityDef = cardEntityDefs.FirstOrDefault((EntityDef def) => def.IsCollectionManagerFilterManaCostByEven || def.IsCollectionManagerFilterManaCostByOdd);
		if (cardEntityDef == null)
		{
			return;
		}
		CollectionManagerDisplay cmDisplay = CollectionManager.Get().GetCollectibleDisplay() as CollectionManagerDisplay;
		bool suggestEvenManaCostFilter = cardEntityDef.IsCollectionManagerFilterManaCostByEven && cmDisplay != null && !cmDisplay.IsManaFilterEvenValues;
		bool suggestOddManaCostFilter = cardEntityDef.IsCollectionManagerFilterManaCostByOdd && cmDisplay != null && !cmDisplay.IsManaFilterOddValues;
		if (!(suggestEvenManaCostFilter || suggestOddManaCostFilter))
		{
			return;
		}
		string stringKey = (suggestEvenManaCostFilter ? "GLUE_COLLECTION_MANAGER_MANA_FILTER_PROMPT_BODY_EVEN_CARDS" : "GLUE_COLLECTION_MANAGER_MANA_FILTER_PROMPT_BODY_ODD_CARDS");
		AlertPopup.PopupInfo info = new AlertPopup.PopupInfo
		{
			m_headerText = GameStrings.Get("GLUE_COLLECTION_MANAGER_MANA_FILTER_PROMPT_HEADER"),
			m_text = GameStrings.Get(stringKey),
			m_confirmText = GameStrings.Get("GLOBAL_BUTTON_YES"),
			m_cancelText = GameStrings.Get("GLOBAL_BUTTON_NO"),
			m_showAlertIcon = false,
			m_responseDisplay = AlertPopup.ResponseDisplay.CONFIRM_CANCEL,
			m_responseCallback = delegate(AlertPopup.Response response, object userData)
			{
				if (response == AlertPopup.Response.CONFIRM && !(cmDisplay == null))
				{
					string newSearchText = CollectibleCardFilter.CreateSearchTerm_Mana_OddEven((bool)userData);
					cmDisplay.FilterBySearchText(newSearchText);
				}
			},
			m_responseUserData = suggestOddManaCostFilter
		};
		DialogManager.Get().ShowPopup(info);
	}

	public bool AnimateInCardBack(Actor actor)
	{
		CollectionCardBack cardBack = actor.gameObject.GetComponent<CollectionCardBack>();
		if (cardBack == null)
		{
			return false;
		}
		return GetCardBackContent().AnimateInCardBack(cardBack.GetCardBackId(), actor.gameObject);
	}

	public void AnimateInCosmeticCoin(Actor actor)
	{
		GetCosmeticCoinContent().AnimateInCosmeticCoin(actor);
	}

	public void SetHeroSkin(Actor actor)
	{
		GetHeroSkinContent().AnimateInHeroSkin(actor);
	}

	public void FlashDeckTemplateHighlight()
	{
		if (m_deckTemplateChosenGlow != null)
		{
			m_deckTemplateChosenGlow.SendEvent("Flash");
		}
	}

	public void HandleAddedCardDeckUpdate(EntityDef entityDef, TAG_PREMIUM premium, int newCount)
	{
		if (!IsShowingDeckContents())
		{
			return;
		}
		CollectionDeck editedDeck = CollectionManager.Get().GetEditedDeck();
		if (editedDeck == null)
		{
			Debug.LogWarning("null editing deck returned during HandleAddedCardDeckUpdate");
			return;
		}
		string cardId = entityDef.GetCardId();
		CollectionDeck deckToAddCard = editedDeck;
		CollectionDeckSlot unownedSlot = editedDeck.FindFirstOwnedSlotByCardId(cardId, owned: false);
		if (unownedSlot == null)
		{
			unownedSlot = FindFirstOwnedSlotByCardIdFromSideboards(cardId, owned: false, editedDeck, out deckToAddCard);
		}
		int numCardsAdded = 0;
		while (unownedSlot != null && numCardsAdded < newCount)
		{
			AddCard(entityDef, premium, deckToAddCard, true, true, null);
			unownedSlot = deckToAddCard.FindFirstOwnedSlotByCardId(cardId, owned: false);
			if (unownedSlot == null)
			{
				unownedSlot = FindFirstOwnedSlotByCardIdFromSideboards(cardId, owned: false, editedDeck, out deckToAddCard);
			}
			numCardsAdded++;
		}
	}

	private CollectionDeckSlot FindFirstOwnedSlotByCardIdFromSideboards(string cardId, bool owned, CollectionDeck deck, out CollectionDeck sideboard)
	{
		CollectionDeckSlot result = null;
		sideboard = null;
		foreach (KeyValuePair<string, SideboardDeck> kvp in deck.GetAllSideboards())
		{
			result = kvp.Value.FindFirstOwnedSlotByCardId(cardId, owned: false);
			if (result != null)
			{
				sideboard = kvp.Value;
				break;
			}
		}
		return result;
	}

	private void UpdateCardList(string justChangedCardId)
	{
		if (IsSideboardOpen)
		{
			CollectionDeck sideboardDeck = GetCurrentEditedSideboard();
			m_currentSideboardTray.CardsContent.UpdateCardList(justChangedCardId, sideboardDeck);
			GetCardsContent().UpdateCardList();
		}
		else
		{
			GetCardsContent().UpdateCardList(justChangedCardId);
		}
	}

	public bool HandleDeletedCardDeckUpdate(string cardID)
	{
		if (!IsShowingDeckContents())
		{
			return false;
		}
		CollectionDeck deck = GetCurrentDeckContext();
		GetCardEventHandler(cardID).OnCardRemoved(this, deck);
		CollectionDeck editedDeck = CollectionManager.Get().GetEditedDeck();
		if (editedDeck != null && editedDeck.HasSideboard(cardID) && editedDeck.GetCardIdCount(cardID, includeUnowned: true, includeSideboards: false) == 0)
		{
			editedDeck.RemoveSideboard(cardID);
			if (editedDeck.IsEditingSideboardFor(cardID))
			{
				editedDeck.ClearEditedSideboard();
			}
		}
		UpdateCardList(cardID);
		CollectionManager.Get().GetCollectibleDisplay().UpdateCurrentPageCardLocks(playSound: true);
		return true;
	}

	public bool RemoveCard(string cardID, TAG_PREMIUM premium, bool valid, bool enforceRemainingDeckRuleset = false)
	{
		bool removed = false;
		CollectionDeck deck = GetCurrentDeckContext();
		if (deck != null)
		{
			removed = deck.RemoveCard(cardID, premium, valid, enforceRemainingDeckRuleset);
			if (removed)
			{
				HandleDeletedCardDeckUpdate(cardID);
			}
		}
		return removed;
	}

	public bool RemoveAllCopiesOfCard(string cardID)
	{
		bool removed = false;
		CollectionDeck deck = CollectionManager.Get().GetEditedDeck();
		for (int slotIndex = deck.GetSlots().Count - 1; slotIndex >= 0; slotIndex--)
		{
			CollectionDeckSlot slot = deck.GetSlots()[slotIndex];
			if (!(slot.CardID != cardID))
			{
				while (slot.GetCount(TAG_PREMIUM.NORMAL) > 0)
				{
					removed |= RemoveCard(slot.CardID, TAG_PREMIUM.NORMAL, valid: true);
					removed |= RemoveCard(slot.CardID, TAG_PREMIUM.NORMAL, valid: false);
				}
				while (slot.GetCount(TAG_PREMIUM.GOLDEN) > 0)
				{
					removed |= RemoveCard(slot.CardID, TAG_PREMIUM.GOLDEN, valid: true);
					removed |= RemoveCard(slot.CardID, TAG_PREMIUM.GOLDEN, valid: false);
				}
				while (slot.GetCount(TAG_PREMIUM.SIGNATURE) > 0)
				{
					removed |= RemoveCard(slot.CardID, TAG_PREMIUM.SIGNATURE, valid: true);
					removed |= RemoveCard(slot.CardID, TAG_PREMIUM.SIGNATURE, valid: false);
				}
				while (slot.GetCount(TAG_PREMIUM.DIAMOND) > 0)
				{
					removed |= RemoveCard(slot.CardID, TAG_PREMIUM.DIAMOND, valid: true);
					removed |= RemoveCard(slot.CardID, TAG_PREMIUM.DIAMOND, valid: false);
				}
			}
		}
		return removed;
	}

	public void ShowDeck(CollectionUtils.ViewMode viewMode)
	{
		Log.CollectionManager.Print("mode={0}", viewMode);
		DeckContentTypes contentType = GetContentTypeFromViewMode(viewMode);
		SetTrayMode(contentType);
		if (!CollectionManagerDisplay.IsSpecialOneDeckMode())
		{
			Navigation.PushUnique(OnBackOutOfContainerContents);
		}
		if (CollectionManager.Get().ShouldShowWildToStandardTutorial(checkPrevSceneIsPlayMode: false) && CollectionManager.Get().GetEditedDeck().FormatType == FormatType.FT_WILD)
		{
			CollectionManagerDisplay cmd = CollectionManager.Get().GetCollectibleDisplay() as CollectionManagerDisplay;
			if (cmd != null && cmd.ViewModeHasVisibleDeckList())
			{
				cmd.ShowConvertTutorial(UserAttentionBlocker.SET_ROTATION_CM_TUTORIALS);
			}
		}
		if (!UniversalInputManager.UsePhoneUI)
		{
			UpdateEditedDeckBoxColliderHeightForDeathKnight();
		}
		HideReadyForStandardTutorial();
	}

	public void ShowTeam(CollectionUtils.ViewMode viewMode)
	{
		Log.CollectionManager.Print("mode={0}", viewMode);
		LettuceCollectionDisplay lcd = CollectionManager.Get().GetCollectibleDisplay() as LettuceCollectionDisplay;
		if (lcd != null && viewMode != 0)
		{
			viewMode = CollectionUtils.ViewMode.CARDS;
			lcd.SetViewMode(viewMode);
		}
		SetTrayMode(DeckContentTypes.Mercs);
		Navigation.PushUnique(OnBackOutOfContainerContents);
	}

	public void EnterEditDeckModeForTavernBrawl(CollectionDeck deck, bool isNewDeck)
	{
		Navigation.Push(OnBackOutOfContainerContents);
		UpdateDoneButtonText();
		UpdateRuneIndicatorVisual(deck);
		CollectionDeckBoxVisual deckBox = GetEditingDeckBox();
		if (deckBox != null)
		{
			deckBox.UpdateRuneSlotVisual(deck);
		}
		m_runeIndicatorVisual.EnableRuneButtons();
		m_cardsContent.UpdateCardList();
		CheckNumCardsNeededToBuildDeck(deck);
		CollectionManager.Get().StartEditingDeck(deck, isNewDeck);
	}

	public void ExitEditDeckModeForTavernBrawl()
	{
		UpdateDoneButtonText();
	}

	public void EnterDeckEditForPVPDR(CollectionDeck deck)
	{
		CollectionManager.Get().SetEditedDeck(deck);
		CollectionManagerDisplay obj = CollectionManager.Get().GetCollectibleDisplay() as CollectionManagerDisplay;
		obj.ShowDuelsDeckHeader();
		obj.ShowCurrentEditedDeck();
		UpdateDoneButtonText();
		UpdateRuneIndicatorVisual(deck);
		CollectionDeckBoxVisual deckBox = GetEditingDeckBox();
		if (deckBox != null)
		{
			deckBox.UpdateRuneSlotVisual(deck);
		}
	}

	private void CheckNumCardsNeededToBuildDeck(CollectionDeck deck)
	{
		int numCardsNeededToBuildMinimumSizeDeck = CalculateNumCardsNeededToCraftToReachMinimumDeckSize(deck);
		if (numCardsNeededToBuildMinimumSizeDeck > 0)
		{
			AlertPopup.PopupInfo popupInfo = new AlertPopup.PopupInfo();
			popupInfo.m_headerText = GameStrings.Get("GLUE_COLLECTION_DECK_INVALID_POPUP_HEADER");
			popupInfo.m_text = GameStrings.Format("GLUE_COLLECTION_DECK_RULE_NOT_ENOUGH_CARDS", numCardsNeededToBuildMinimumSizeDeck);
			popupInfo.m_okText = GameStrings.Get("GLOBAL_OKAY");
			popupInfo.m_showAlertIcon = true;
			popupInfo.m_responseDisplay = AlertPopup.ResponseDisplay.OK;
			AlertPopup.PopupInfo info = popupInfo;
			DialogManager.Get().ShowPopup(info);
		}
	}

	public bool IsWaitingToDeleteDeck()
	{
		if (!m_decksContent)
		{
			return false;
		}
		return m_decksContent.IsWaitingToDeleteDeck();
	}

	public void DeleteEditingDeck(bool popNavigation = true)
	{
		if (popNavigation)
		{
			Navigation.Pop();
		}
		m_decksContent.DeleteEditingDeck();
		SetTrayMode(DeckContentTypes.Decks);
	}

	public void CancelRenamingDeck()
	{
		m_decksContent.CancelRenameEditingDeck();
	}

	public void SetMyDecksLabelText(string text)
	{
		m_myDecksLabel.Text = text;
	}

	public DeckTrayDeckListContent GetDecksContent()
	{
		return m_decksContent;
	}

	public DeckTrayCosmeticCoinContent GetCosmeticCoinContent()
	{
		return m_cosmeticCoinContent;
	}

	public DeckTrayCardBackContent GetCardBackContent()
	{
		return m_cardBackContent;
	}

	public DeckTrayHeroSkinContent GetHeroSkinContent()
	{
		return m_heroSkinContent;
	}

	public DeckTrayTeamListContent GetTeamsContent()
	{
		return m_teamsContent;
	}

	public DeckTrayMercListContent GetMercsContent()
	{
		return m_mercContent;
	}

	public DeckTrayReorderableContent GetReorderableContent()
	{
		if ((bool)m_decksContent)
		{
			return m_decksContent;
		}
		if ((bool)m_teamsContent)
		{
			return m_teamsContent;
		}
		return null;
	}

	public void Exit()
	{
		if (!UniversalInputManager.UsePhoneUI)
		{
			HideUnseenDeckTrays();
		}
	}

	public CollectionDeckBoxVisual GetEditingDeckBox()
	{
		TraySection traySection = GetDecksContent().GetEditingTraySection();
		if (traySection == null)
		{
			return null;
		}
		return traySection.m_deckBox;
	}

	public void InitializeRuneIndicatorVisual(CollectionDeck deck)
	{
		if (deck != null && !(m_runeIndicatorVisual == null))
		{
			m_runeIndicatorVisual.Initialize(deck, this);
		}
	}

	public void DisableRuneIndicatorVisualButtons()
	{
		m_runeIndicatorVisual.DisableRuneButtons();
	}

	public void EnableRuneIndicatorVisualButtons()
	{
		m_runeIndicatorVisual.EnableRuneButtons();
	}

	private void DoneButtonPress(UIEvent e)
	{
		if (m_cardBackContent != null && m_cardBackContent.WaitingForCardbackAnimation)
		{
			StartCoroutine(CompleteDoneButtonPressAfterAnimations(e));
			return;
		}
		SideboardDeck incompleteSideboard = GetIncompleteSideboard();
		if (incompleteSideboard != null)
		{
			ShowIncompleteSideboardPopup(incompleteSideboard);
			return;
		}
		ResetDeathKnightTrayChanges();
		Navigation.GoBack();
	}

	private void ShowIncompleteSideboardPopup(SideboardDeck incompleteSideboard)
	{
		AlertPopup.PopupInfo popup = new AlertPopup.PopupInfo
		{
			m_responseDisplay = AlertPopup.ResponseDisplay.CONFIRM_CANCEL,
			m_responseCallback = delegate(AlertPopup.Response response, object userData)
			{
				if (response == AlertPopup.Response.CONFIRM)
				{
					ResetDeathKnightTrayChanges();
					Navigation.GoBack();
				}
			}
		};
		if (incompleteSideboard.SideboardType == TAG_SIDEBOARD_TYPE.ZILLIAX)
		{
			popup.m_headerText = GameStrings.Get("GLUE_COLLECTION_INCOMPLETE_ZILLIAX_HEADER");
			popup.m_text = GameStrings.Get("GLUE_COLLECTION_INCOMPLETE_ZILLIAX_DESCRIPTION");
		}
		else
		{
			popup.m_headerText = GameStrings.Get("GLUE_COLLECTION_INCOMPLETE_SIDEBOARD_HEADER");
			popup.m_text = GameStrings.Get("GLUE_COLLECTION_INCOMPLETE_SIDEBOARD_DESCRIPTION");
		}
		DialogManager.Get().ShowPopup(popup);
	}

	private void ResetDeathKnightTrayChanges()
	{
		if (m_runeIndicatorVisual != null)
		{
			if (TavernBrawlDisplay.IsTavernBrawlOpen())
			{
				m_runeIndicatorVisual.DisableRuneButtons();
			}
			else
			{
				m_runeIndicatorVisual.Hide();
				m_cardsContent.SetRuneIndicatorSpacerVisible(visible: false);
				m_runeIndicatorVisual.EnableRuneButtons();
			}
		}
		if (!UniversalInputManager.UsePhoneUI && CollectionManager.Get().IsEditingDeathKnightDeck())
		{
			CollectionDeckBoxVisual editedDeckBox = GetEditingDeckBox();
			if ((bool)editedDeckBox)
			{
				editedDeckBox.ResetColliderHeight();
			}
		}
	}

	private SideboardDeck GetIncompleteSideboard()
	{
		CollectionDeck editedDeck = CollectionManager.Get().GetEditedDeck();
		if (editedDeck == null)
		{
			return null;
		}
		List<SideboardDeck> incompleteSideboards = editedDeck.GetIncompleteSideboards();
		if (incompleteSideboards.Count <= 0)
		{
			return null;
		}
		return incompleteSideboards[0];
	}

	private IEnumerator CompleteDoneButtonPressAfterAnimations(UIEvent e)
	{
		while (m_cardBackContent.WaitingForCardbackAnimation)
		{
			yield return null;
		}
		DoneButtonPress(e);
	}

	public override bool OnBackOutOfContainerContents()
	{
		if (SceneMgr.Get().IsInLettuceMode())
		{
			return OnBackOutOfMercenariesContents();
		}
		return OnBackOutOfDeckContentsImpl(deleteDeck: false);
	}

	public bool OnBackOutOfDeckContentsImpl(bool deleteDeck)
	{
		if (GetCurrentContentType() != DeckContentTypes.INVALID && GetCurrentContent() != null && !GetCurrentContent().IsModeActive())
		{
			return false;
		}
		if (!IsShowingDeckContents())
		{
			return false;
		}
		Log.DeckTray.Print("backing out of deck contents " + deleteDeck);
		DeckHelper.Get().Hide();
		CollectionManagerDisplay cmd = CollectionManager.Get().GetCollectibleDisplay() as CollectionManagerDisplay;
		if (cmd != null)
		{
			cmd.HideConvertTutorial();
		}
		CollectionDeck deck = CollectionManager.Get().GetEditedDeck();
		if (deleteDeck)
		{
			m_decksContent.DeleteDeck(deck.ID);
		}
		DeckRuleset deckRuleset = CollectionManager.Get().GetDeckRuleset();
		(CollectionManager.Get().GetCollectibleDisplay().GetPageManager() as CollectionPageManager).HideNonDeckTemplateTabs(hide: false);
		bool isValid = true;
		if (deckRuleset != null)
		{
			isValid = deckRuleset.IsDeckValid(deck);
		}
		if (deck.FormatType == FormatType.FT_STANDARD && isValid && CollectionManager.Get().ShouldShowWildToStandardTutorial(checkPrevSceneIsPlayMode: false) && UserAttentionManager.CanShowAttentionGrabber(UserAttentionBlocker.SET_ROTATION_CM_TUTORIALS, "CollectionDeckTray.OnBackOutOfDeckContentsImpl:ShowSetRotationTutorial"))
		{
			Options.Get().SetBool(Option.NEEDS_TO_MAKE_STANDARD_DECK, val: false);
			Options.Get().SetLong(Option.LAST_CUSTOM_DECK_CHOSEN, deck.ID);
			Vector3 tipPosition = OverlayUI.Get().GetRelativePosition(m_doneButton.transform.position);
			tipPosition += (UniversalInputManager.UsePhoneUI ? new Vector3(-56.5f, 0f, 35f) : new Vector3(-30.8f, 0f, 17.8f));
			Notification notification = NotificationManager.Get().CreatePopupText(UserAttentionBlocker.SET_ROTATION_CM_TUTORIALS, tipPosition, NotificationManager.NOTIFICATITON_WORLD_SCALE, GameStrings.Get("GLUE_COLLECTION_TUTORIAL16"), convertLegacyPosition: false);
			notification.ShowPopUpArrow(Notification.PopUpArrowDirection.RightDown);
			notification.PulseReminderEveryXSeconds(3f);
			UserAttentionManager.StopBlocking(UserAttentionBlocker.SET_ROTATION_CM_TUTORIALS);
			m_doneButton.GetComponentInChildren<HighlightState>().ChangeState(ActorStateType.HIGHLIGHT_PRIMARY_ACTIVE);
		}
		SaveCurrentDeckAndEnterDeckListMode();
		return true;
	}

	public bool OnBackOutOfMercenariesContents()
	{
		if (GetCurrentContentType() != DeckContentTypes.INVALID && !GetCurrentContent().IsModeActive())
		{
			return false;
		}
		if (!IsShowingTeamContents())
		{
			return false;
		}
		LettuceCollectionDisplay lcd = CollectionManager.Get().GetCollectibleDisplay() as LettuceCollectionDisplay;
		if (lcd != null && lcd.IsMercenaryDetailsDisplayActive())
		{
			Log.DeckTray.Print("backing out of merc detail display");
			lcd.HideMercenaryDetailsDisplay();
			return true;
		}
		Log.DeckTray.Print("backing out of team contents");
		LettuceTeam team = CollectionManager.Get().GetEditingTeam();
		if (!team.IsBeingDeleted())
		{
			if (!team.IsValid())
			{
				AlertPopup.PopupInfo info = new AlertPopup.PopupInfo
				{
					m_headerText = GameStrings.Get("GLUE_LETTUCE_COLLECTION_INCOMPLETE_TEAM_HEADER"),
					m_text = GameStrings.Get("GLUE_LETTUCE_COLLECTION_INCOMPLETE_TEAM_DESC"),
					m_confirmText = GameStrings.Get("GLOBAL_CONTINUE"),
					m_cancelText = GameStrings.Get("GLOBAL_BUTTON_NO"),
					m_showAlertIcon = true,
					m_responseDisplay = AlertPopup.ResponseDisplay.CONFIRM_CANCEL,
					m_responseCallback = delegate(AlertPopup.Response response, object o)
					{
						if (response == AlertPopup.Response.CONFIRM)
						{
							SaveCurrentTeamAndEnterTeamListMode();
						}
						else
						{
							Navigation.Push(OnBackOutOfContainerContents);
						}
					}
				};
				DialogManager.Get().ShowPopup(info);
				return true;
			}
			LettuceMercenary unequippedMerc = team.GetMercs().FirstOrDefault((LettuceMercenary m) => m.IsEquipmentSlotUnassigned() && m.m_equipmentList.Any((LettuceAbility e) => e.Owned));
			if (unequippedMerc != null)
			{
				AlertPopup.PopupInfo info2 = new AlertPopup.PopupInfo
				{
					m_headerText = GameStrings.Get("GLUE_LETTUCE_COLLECTION_EQUIPMENT_AVAILABLE_HEADER"),
					m_text = GameStrings.Get("GLUE_LETTUCE_COLLECTION_EQUIPMENT_AVAILABLE_DESC"),
					m_confirmText = GameStrings.Get("GLOBAL_CONTINUE"),
					m_cancelText = GameStrings.Get("GLUE_LETTUCE_COLLECTION_EQUIPMENT_AVAILABLE_BUTTON_EQUIP"),
					m_showAlertIcon = true,
					m_responseDisplay = AlertPopup.ResponseDisplay.CONFIRM_CANCEL,
					m_responseCallback = delegate(AlertPopup.Response response, object o)
					{
						if (response == AlertPopup.Response.CONFIRM)
						{
							SaveCurrentTeamAndEnterTeamListMode();
						}
						else
						{
							Navigation.Push(OnBackOutOfContainerContents);
							lcd.ShowMercenaryDetailsDisplay(unequippedMerc);
						}
					}
				};
				DialogManager.Get().ShowPopup(info2);
				return true;
			}
		}
		SaveCurrentTeamAndEnterTeamListMode();
		return true;
	}

	private void HideReadyForStandardTutorial()
	{
		if (NotificationManager.Get() != null)
		{
			NotificationManager.Get().DestroyNotificationWithText(GameStrings.Get("GLUE_COLLECTION_TUTORIAL16"));
		}
		if (m_doneButton != null)
		{
			m_doneButton.GetComponentInChildren<HighlightState>().ChangeState(ActorStateType.HIGHLIGHT_OFF);
		}
	}

	private bool OnBackOutOfCollectionScreen()
	{
		if (this == null || base.gameObject == null)
		{
			return true;
		}
		HideReadyForStandardTutorial();
		if (GetCurrentContentType() != DeckContentTypes.INVALID && GetCurrentContent() != null && !GetCurrentContent().IsModeActive())
		{
			return false;
		}
		if (SceneMgr.Get() != null && !SceneMgr.Get().IsInTavernBrawlMode() && !SceneMgr.Get().IsInLettuceMode() && IsShowingDeckContents())
		{
			return false;
		}
		if ((!SceneMgr.Get().IsInTavernBrawlMode() || SceneMgr.Get().GetPrevMode() != SceneMgr.Mode.GAME_MODE) && !SceneMgr.Get().IsInLettuceMode())
		{
			AnimationUtil.DelayedActivate(base.gameObject, 0.25f, activate: false);
		}
		if (CollectionManager.Get().GetCollectibleDisplay() != null)
		{
			CollectionManager.Get().GetCollectibleDisplay().Exit();
		}
		return true;
	}

	public static void SaveCurrentDeck()
	{
		CollectionDeck deck = CollectionManager.Get().GetEditedDeck();
		if (deck != null)
		{
			deck.RemoveOrphanedSideboards();
			deck.ClearRemovedSideboards();
			deck.SendChanges(CollectionDeck.ChangeSource.SaveCurrentDeck);
			if (Network.IsLoggedIn())
			{
				CollectionManager.Get().SetTimeOfLastPlayerDeckSave(DateTime.Now);
			}
			Log.Decks.PrintInfo("Finished Editing Deck:");
			deck.LogDeckStringInformation();
		}
	}

	private void SaveCurrentDeckAndEnterDeckListMode()
	{
		SaveCurrentDeck();
		if (SceneMgr.Get().IsInTavernBrawlMode())
		{
			if (TavernBrawlDisplay.Get() != null)
			{
				TavernBrawlDisplay.Get().BackFromDeckEdit(animate: true);
			}
			m_cardsContent.UpdateCardList();
			return;
		}
		SetTrayMode(DeckContentTypes.Decks);
		CollectionManager.Get().DoneEditing();
		UpdateDoneButtonText();
		CollectionManagerDisplay cmd = CollectionManager.Get().GetCollectibleDisplay() as CollectionManagerDisplay;
		if (cmd != null)
		{
			cmd.OnDoneEditingDeck();
		}
	}

	private void SaveCurrentTeamAndEnterTeamListMode()
	{
		LettuceTeam team = CollectionManager.Get().GetEditingTeam();
		if (team != null)
		{
			team.SendChanges();
			if (Network.IsLoggedIn())
			{
				CollectionManager.Get().SetTimeOfLastPlayerDeckSave(DateTime.Now);
			}
		}
		SetTrayMode(DeckContentTypes.Teams);
		CollectionManager.Get().DoneEditingTeam();
		UpdateDoneButtonText();
		LettuceCollectionDisplay lcd = CollectionManager.Get().GetCollectibleDisplay() as LettuceCollectionDisplay;
		if (lcd != null)
		{
			lcd.OnDoneEditingTeam();
		}
	}

	public void CompleteMyDeckButtonPress()
	{
		if (!Network.IsLoggedIn())
		{
			CollectionManager.ShowFeatureDisabledWhileOfflinePopup();
			return;
		}
		AlertPopup.PopupInfo info = new AlertPopup.PopupInfo
		{
			m_headerText = GameStrings.Get("GLUE_COLLECTION_DECK_COMPLETE_POPUP_HEADER"),
			m_text = GameStrings.Get("GLUE_COLLECTION_DECK_RULE_FINISH_AUTOMATICALLY"),
			m_confirmText = GameStrings.Get("GLUE_COLLECTION_DECK_COMPLETE_POPUP_CONFIRM"),
			m_cancelText = GameStrings.Get("GLUE_COLLECTION_DECK_COMPLETE_POPUP_CANCEL"),
			m_showAlertIcon = true,
			m_responseDisplay = AlertPopup.ResponseDisplay.CONFIRM_CANCEL,
			m_responseCallback = delegate(AlertPopup.Response response, object userData)
			{
				if (response == AlertPopup.Response.CONFIRM)
				{
					FinishMyDeck(backOutWhenComplete: false);
				}
			}
		};
		DialogManager.Get().ShowPopup(info);
	}

	public void FinishMyDeck(bool backOutWhenComplete)
	{
		CollectionDeck deck = CollectionManager.Get().GetEditedDeck();
		bool allowSmartDeckCompletion = SceneMgr.Get().GetMode() == SceneMgr.Mode.COLLECTIONMANAGER;
		CollectionManager.Get().AutoFillDeck(deck, allowSmartDeckCompletion, delegate(CollectionDeck filledDeck, IEnumerable<DeckMaker.DeckFill> fillCards)
		{
			AutoAddCardsAndTryToBackOut(fillCards, filledDeck.GetRuleset(null), backOutWhenComplete);
			foreach (SideboardDeck value in deck.GetAllSideboards().Values)
			{
				value.AutoFillSideboard();
			}
		});
	}

	private void AutoAddCardsAndTryToBackOut(IEnumerable<DeckMaker.DeckFill> fillCards, DeckRuleset deckRuleset, bool backOutWhenComplete)
	{
		PopuplateDeckCompleteCallback onAutoAddCardsCompleteCallback = null;
		if (backOutWhenComplete)
		{
			onAutoAddCardsCompleteCallback = delegate
			{
				OnBackOutOfContainerContents();
			};
		}
		StartCoroutine(AutoAddCardsWithTiming(fillCards, deckRuleset, allowInvalid: false, onAutoAddCardsCompleteCallback));
	}

	public void PopulateDeck(IEnumerable<DeckMaker.DeckFill> fillCards, PopuplateDeckCompleteCallback completedCallback)
	{
		CollectionDeck deck = CollectionManager.Get().GetEditedDeck();
		if (deck.HasClass(TAG_CLASS.DEATHKNIGHT))
		{
			deck.ClearRuneOrder();
			m_runeIndicatorVisual.ResetRuneButtons();
		}
		deck.ClearSlotContents();
		GetCardsContent().UpdateCardList();
		StartCoroutine(AutoAddCardsWithTiming(fillCards, null, allowInvalid: true, completedCallback));
	}

	private IEnumerator AutoAddCardsWithTiming(IEnumerable<DeckMaker.DeckFill> fillCards, DeckRuleset deckRuleset, bool allowInvalid, PopuplateDeckCompleteCallback completedCallback)
	{
		m_isAutoAddingCardsWithTiming = true;
		AllowInput(allowed: false);
		CollectionManager.Get().GetCollectibleDisplay().EnableInput(enable: false);
		List<EntityDef> addedCards = null;
		List<EntityDef> removedCards = null;
		if (completedCallback != null)
		{
			addedCards = new List<EntityDef>();
			removedCards = new List<EntityDef>();
		}
		if (CollectionManager.Get().IsInEditMode())
		{
			CollectionDeck deck = CollectionManager.Get().GetEditedDeck();
			bool isDeckRulesetNull = deckRuleset == null;
			foreach (DeckMaker.DeckFill fillCard in fillCards)
			{
				if (deck == null || !deck.IsBeingEdited())
				{
					break;
				}
				if (!deck.HasReplaceableSlot())
				{
					int totalCardCount = deck.GetTotalCardCount();
					int maxDeckSize = (isDeckRulesetNull ? int.MaxValue : deckRuleset.GetDeckSize(deck));
					if (totalCardCount >= maxDeckSize)
					{
						break;
					}
				}
				EntityDef addCard = fillCard.m_addCard;
				EntityDef removeCard = fillCard.m_removeTemplate;
				if (removeCard != null)
				{
					bool removed = RemoveCard(removeCard.GetCardId(), TAG_PREMIUM.NORMAL, valid: false);
					if (!removed)
					{
						removed = RemoveCard(removeCard.GetCardId(), TAG_PREMIUM.GOLDEN, valid: false);
					}
					if (!removed)
					{
						removed = RemoveCard(removeCard.GetCardId(), TAG_PREMIUM.SIGNATURE, valid: false);
					}
					if (!removed)
					{
						removed = RemoveCard(removeCard.GetCardId(), TAG_PREMIUM.DIAMOND, valid: false);
					}
					if (removed)
					{
						removedCards?.Add(removeCard);
					}
				}
				if (addCard != null && deck.IsEntityDefValid(addCard, null, ignoreOwnership: false, ignoreGameplayEvent: false, enforceRemainingDeckRuleset: false, null) && (0u | (AddCardWithPreferredPremium(addCard, playSound: true) ? 1u : 0u)) != 0)
				{
					addedCards?.Add(addCard);
					yield return new WaitForSeconds(0.2f);
				}
			}
		}
		CollectionManager.Get().GetCollectibleDisplay().EnableInput(enable: true);
		AllowInput(allowed: true);
		m_isAutoAddingCardsWithTiming = false;
		completedCallback?.Invoke(addedCards, removedCards);
	}

	public void PopulateTeam(IEnumerable<LettuceCollectionDisplay.TeamCopyingModule.TeamFill> fillCards, PopuplateDeckCompleteCallback completedCallback)
	{
		StartCoroutine(AutoAddMercenariesToTeamWithTiming(fillCards, completedCallback));
	}

	private IEnumerator AutoAddMercenariesToTeamWithTiming(IEnumerable<LettuceCollectionDisplay.TeamCopyingModule.TeamFill> fillCards, PopuplateDeckCompleteCallback completedCallback)
	{
		AllowInput(allowed: false);
		CollectionManager.Get().GetCollectibleDisplay().EnableInput(enable: false);
		List<EntityDef> addedCards = null;
		List<EntityDef> removedCards = null;
		if (completedCallback != null)
		{
			addedCards = new List<EntityDef>();
			removedCards = new List<EntityDef>();
		}
		if (CollectionManager.Get().IsInEditTeamMode())
		{
			LettuceTeam team = CollectionManager.Get().GetEditingTeam();
			if (team != null && team.IsBeingEdited())
			{
				team.ClearContents();
				int maxTeamSize = CollectionManager.Get().GetTeamSize();
				foreach (LettuceCollectionDisplay.TeamCopyingModule.TeamFill fillCard in fillCards)
				{
					if (team.GetMercCount() >= maxTeamSize)
					{
						break;
					}
					EntityDef addCard = fillCard.m_addCard;
					if (addCard != null && GetMercsContent().AddMerc(addCard, playSound: true, null, updateVisuals: true, -1, fillCard.m_addLoadout))
					{
						addedCards?.Add(addCard);
						yield return new WaitForSeconds(0.2f);
					}
				}
			}
		}
		CollectionManager.Get().GetCollectibleDisplay().EnableInput(enable: true);
		AllowInput(allowed: true);
		completedCallback?.Invoke(addedCards, removedCards);
	}

	public override void UpdateDoneButtonText()
	{
		bool showBackText = (!CollectionManager.Get().IsInEditMode() && !CollectionManager.Get().IsInEditTeamMode()) || CollectionManager.Get().GetCollectibleDisplay().GetViewMode() == CollectionUtils.ViewMode.DECK_TEMPLATE;
		if (SceneMgr.Get().IsInTavernBrawlMode())
		{
			TavernBrawlDisplay tavernBrawlDisplay = TavernBrawlDisplay.Get();
			showBackText = tavernBrawlDisplay != null && !tavernBrawlDisplay.IsInDeckEditMode() && !UniversalInputManager.UsePhoneUI;
		}
		if (SceneMgr.Get().IsInLettuceMode() && CollectionManager.Get().GetEditingTeam() != null)
		{
			showBackText = false;
		}
		bool hasBackArrow = m_backArrow != null;
		if (showBackText)
		{
			m_doneButton.SetText(hasBackArrow ? "" : GameStrings.Get("GLOBAL_BACK"));
			if (hasBackArrow)
			{
				m_backArrow.gameObject.SetActive(value: true);
			}
		}
		else
		{
			m_doneButton.SetText(GameStrings.Get("GLOBAL_DONE"));
			if (hasBackArrow)
			{
				m_backArrow.gameObject.SetActive(value: false);
			}
		}
	}

	protected override void HideUnseenDeckTrays()
	{
		base.HideUnseenDeckTrays();
		if (m_decksContent != null)
		{
			m_decksContent.HideTraySectionsNotInBounds(m_scrollbar.m_ScrollBounds.bounds);
		}
	}

	protected override void OnCardTilePress(DeckTrayDeckTileVisual cardTile)
	{
		if (UniversalInputManager.Get().IsTouchMode())
		{
			ShowDeckBigCard(cardTile, 0.2f);
		}
		else if (CollectionInputMgr.Get() != null)
		{
			HideDeckBigCard(cardTile);
		}
	}

	protected override IEnumerator UpdateTrayMode()
	{
		yield return base.UpdateTrayMode();
		UpdateRuneIndicatorVisual();
		if ((bool)m_ETCsideboardTray)
		{
			m_ETCsideboardTray.CardsContent.SetModeActive(active: true);
		}
		if ((bool)m_ZilliaxsideboardTray)
		{
			m_ZilliaxsideboardTray.CardsContent.SetModeActive(active: true);
		}
	}

	public void UpdateRuneIndicatorVisual(CollectionDeck deck)
	{
		if (m_currentContent == DeckContentTypes.Cards && deck.ShouldShowDeathKnightRunes())
		{
			if (deck == null)
			{
				Log.ErrorReporter.PrintError("UpdateTrayMode::CollectionDeckTray deck is null!");
				return;
			}
			m_runeIndicatorVisual.Show();
			m_runeIndicatorVisual.InitializeWithTilePool(deck, this);
			m_cardsContent.SetRuneIndicatorSpacerVisible(visible: true);
			if (TavernBrawlDisplay.IsTavernBrawlViewing())
			{
				m_runeIndicatorVisual.DisableRuneButtons();
			}
			else
			{
				TryToShowDeathKnightDeckBuildingTutorial();
			}
		}
		else
		{
			m_runeIndicatorVisual.Hide();
			m_cardsContent.SetRuneIndicatorSpacerVisible(visible: false);
		}
	}

	private void UpdateRuneIndicatorVisual()
	{
		if (!(m_runeIndicatorVisual == null))
		{
			CollectionManager collectionManager = CollectionManager.Get();
			bool isInDeckTemplateViewMode = false;
			CollectionDeck deck = null;
			CollectibleDisplay collectibleDisplay = collectionManager.GetCollectibleDisplay();
			if (collectibleDisplay != null && collectibleDisplay.GetViewMode() == CollectionUtils.ViewMode.DECK_TEMPLATE)
			{
				isInDeckTemplateViewMode = true;
			}
			deck = ((!isInDeckTemplateViewMode) ? collectionManager.GetEditedDeck() : m_cardsContent.GetEditingDeck());
			UpdateRuneIndicatorVisual(deck);
		}
	}

	private void UpdateEditedDeckBoxColliderHeightForDeathKnight()
	{
		CollectionDeckBoxVisual editedDeckBox = GetEditingDeckBox();
		if ((bool)editedDeckBox)
		{
			editedDeckBox.UpdateColliderHeightForDeathKnight();
		}
	}

	private static void TryToShowDeathKnightDeckBuildingTutorial()
	{
		CollectionManagerDisplay cmd = CollectionManager.Get().GetCollectibleDisplay() as CollectionManagerDisplay;
		bool isShowingDeckTemplatePicker = false;
		if (cmd != null)
		{
			DeckTemplatePicker deckTemplatePicker = (UniversalInputManager.UsePhoneUI ? cmd.GetPhoneDeckTemplateTray() : cmd.m_pageManager.GetDeckTemplatePicker());
			if (deckTemplatePicker != null)
			{
				isShowingDeckTemplatePicker = deckTemplatePicker.IsShowingPacks();
			}
		}
		if (!isShowingDeckTemplatePicker)
		{
			TutorialDeathKnightDeckBuilding.ShowTutorial(UIVoiceLinesManager.TriggerType.STARTED_EDITING_DEATH_KNIGHT_DECK);
		}
	}

	private void OnCardTileTap(DeckTrayDeckTileVisual cardTile)
	{
		if (cardTile == null || m_cardsContent == null)
		{
			return;
		}
		UniversalInputManager universalInputManager = UniversalInputManager.Get();
		if (universalInputManager == null || !universalInputManager.IsTouchMode())
		{
			return;
		}
		CollectionManager collectionManager = CollectionManager.Get();
		if (collectionManager == null)
		{
			return;
		}
		CollectibleDisplay collectibleDisplay = collectionManager.GetCollectibleDisplay();
		if (collectibleDisplay == null || collectibleDisplay.GetViewMode() == CollectionUtils.ViewMode.DECK_TEMPLATE)
		{
			return;
		}
		CollectionDeck deck = GetCurrentDeckContext();
		if (deck != null)
		{
			CollectionDeckSlot slot = cardTile.GetSlot();
			if (!deck.IsValidSlot(slot, ignoreOwnership: false, ignoreGameplayEvent: false, enforceRemainingDeckRuleset: true, null))
			{
				m_cardsContent.ShowDeckHelper(slot, replaceSingleSlotOnly: true);
			}
		}
	}

	protected override void OnCardTileOver(DeckTrayDeckTileVisual cardTile)
	{
		if (!UniversalInputManager.Get().IsTouchMode() && (CollectionInputMgr.Get() == null || !CollectionInputMgr.Get().HasHeldCard()))
		{
			ShowDeckBigCard(cardTile);
		}
	}

	private void OnCardTileHeld(DeckTrayDeckTileVisual cardTile)
	{
		if (CollectionInputMgr.Get() != null && !TavernBrawlDisplay.IsTavernBrawlViewing() && CollectionManager.Get().GetCollectibleDisplay().GetViewMode() != CollectionUtils.ViewMode.DECK_TEMPLATE && CollectionInputMgr.Get().GrabCardTile(cardTile) && m_deckBigCard != null)
		{
			HideDeckBigCard(cardTile, force: true);
		}
	}

	protected override void OnCardTileRelease(DeckTrayDeckTileVisual cardTile)
	{
		RemoveCardTile(cardTile);
	}

	public void RemoveCardTile(DeckTrayDeckTileVisual cardTile)
	{
		if (CollectionManager.Get().GetCollectibleDisplay().GetViewMode() == CollectionUtils.ViewMode.DECK_TEMPLATE || CollectionInputMgr.Get().HasHeldCard())
		{
			return;
		}
		CollectionDeck deck = GetCurrentDeckContext();
		if (UniversalInputManager.Get().IsTouchMode())
		{
			HideDeckBigCard(cardTile);
			return;
		}
		CollectionDeckSlot slot = cardTile.GetSlot();
		if (!deck.IsValidSlot(slot, ignoreOwnership: false, ignoreGameplayEvent: false, enforceRemainingDeckRuleset: true, null) && !deck.Locked)
		{
			m_cardsContent.ShowDeckHelper(slot, replaceSingleSlotOnly: true);
		}
		else if (!(CollectionInputMgr.Get() == null) && !TavernBrawlDisplay.IsTavernBrawlViewing())
		{
			CollectionDeckTileActor actor = cardTile.GetActor();
			Spell oldSpell = actor.GetSpell(SpellType.SUMMON_IN);
			Spell newSpell = SpellManager.Get().GetSpell(oldSpell);
			Transform newSpellTransform = newSpell.transform;
			newSpellTransform.position = actor.transform.position + new Vector3(-2f, 0f, 0f);
			newSpellTransform.localScale = new Vector3(0.6f, 0.6f, 0.6f);
			newSpell.ActivateState(SpellStateType.BIRTH);
			DestroySpellAfterSeconds(newSpell).Forget();
			if (Get() != null)
			{
				Get().RemoveCard(cardTile.GetCardID(), cardTile.GetSlot().UnPreferredPremium, deck.IsValidSlot(cardTile.GetSlot(), ignoreOwnership: false, ignoreGameplayEvent: false, enforceRemainingDeckRuleset: false, null));
			}
			iTween.MoveTo(newSpell.gameObject, new Vector3(newSpellTransform.position.x - 10f, newSpellTransform.position.y + 10f, newSpellTransform.position.z), 4f);
			SoundManager.Get().LoadAndPlay("collection_manager_card_remove_from_deck_instant.prefab:bcee588ddfc73844ea3a24beb63bc53f", base.gameObject);
		}
	}

	private async UniTaskVoid DestroySpellAfterSeconds(Spell spell)
	{
		await UniTask.Delay(TimeSpan.FromSeconds(5.0));
		SpellManager.Get().ReleaseSpell(spell);
	}

	public CollectionDeck GetCurrentDeckContext()
	{
		CollectionDeck deck = CollectionManager.Get().GetEditedDeck();
		if (!IsSideboardOpen)
		{
			return deck;
		}
		return deck.GetCurrentSideboardDeck();
	}

	public DeckTrayCardListContent GetCurrentCardListContext()
	{
		if (IsSideboardOpen && m_currentSideboardTray != null)
		{
			return m_currentSideboardTray.CardsContent;
		}
		return m_cardsContent;
	}

	private SideboardDeck GetCurrentEditedSideboard()
	{
		return CollectionManager.Get().GetEditedDeck()?.GetCurrentSideboardDeck();
	}

	protected override void ShowDeckBigCard(DeckTrayDeckTileVisual cardTile, float delay = 0f)
	{
		if (m_deckBigCard == null)
		{
			return;
		}
		CollectionDeckTileActor actor = cardTile.GetActor();
		EntityDef entityDef = actor.GetEntityDef();
		if (cardTile.CardDefHandleOverride != null)
		{
			using (DefLoader.DisposableCardDef cardDefOverride = cardTile.CardDefHandleOverride.Share())
			{
				ShowDeckBigCardForCardDef(cardDefOverride, entityDef, actor, cardTile, delay);
				return;
			}
		}
		using DefLoader.DisposableCardDef cardDef = DefLoader.Get().GetCardDef(entityDef.GetCardId());
		ShowDeckBigCardForCardDef(cardDef, entityDef, actor, cardTile, delay);
	}

	private void ShowDeckBigCardForCardDef(DefLoader.DisposableCardDef cardDef, EntityDef entityDef, CollectionDeckTileActor actor, DeckTrayDeckTileVisual cardTile, float delay)
	{
		CollectionDeck deck = GetCurrentDeckContext();
		DeckTrayCardListContent listContent = GetCurrentCardListContext();
		if (listContent != null)
		{
			deck = listContent.GetEditingDeck();
		}
		GhostCard.Type ghostType = GhostCard.GetGhostTypeFromSlot(deck, cardTile.GetSlot());
		m_deckBigCard.Show(entityDef, actor.GetPremium(), cardDef, actor.gameObject.transform.position, ghostType, delay);
		if (UniversalInputManager.Get().IsTouchMode())
		{
			cardTile.SetHighlight(highlight: true);
		}
		CollectionManagerDisplay cmd = CollectionManager.Get().GetCollectibleDisplay() as CollectionManagerDisplay;
		if (cmd != null && cmd.m_deckTemplateCardReplacePopup != null)
		{
			cmd.m_deckTemplateCardReplacePopup.Shrink(0.1f);
		}
	}

	protected override void HideDeckBigCard(DeckTrayDeckTileVisual cardTile, bool force = false)
	{
		CollectionDeckTileActor actor = cardTile.GetActor();
		if (m_deckBigCard != null)
		{
			if (force)
			{
				m_deckBigCard.ForceHide();
			}
			else
			{
				m_deckBigCard.Hide(actor.GetEntityDef(), actor.GetPremium());
			}
			if (UniversalInputManager.Get().IsTouchMode())
			{
				cardTile.SetHighlight(highlight: false);
			}
			CollectionManagerDisplay cmd = CollectionManager.Get().GetCollectibleDisplay() as CollectionManagerDisplay;
			if (cmd != null && cmd.m_deckTemplateCardReplacePopup != null)
			{
				cmd.m_deckTemplateCardReplacePopup.Unshrink(0.1f);
			}
		}
	}

	private void OnCardCountUpdated(int cardCount, int maxCount)
	{
		string cardLabel = GameStrings.Get("GLUE_DECK_TRAY_CARD_COUNT_LABEL");
		string cardCountText = GameStrings.Format("GLUE_DECK_TRAY_COUNT", cardCount, maxCount);
		m_countLabelText.Text = cardLabel;
		m_countText.Text = cardCountText;
	}

	private void OnSideboardCardCountUpdated(int cardCount, int maxCount)
	{
		SideboardDeck sideboard = GetCurrentEditedSideboard();
		if (sideboard != null)
		{
			sideboard.DataModel.CardCount = cardCount;
			sideboard.UpdateInvalidCardsData();
		}
	}

	private void OnSideboardCardTileRightClick(DeckTrayDeckTileVisual cardTile)
	{
		CollectionDeckTray.SideboardCardTileRightClicked?.Invoke(cardTile);
	}

	private void OnDeckCountUpdated(int deckCount)
	{
		string deckLabel = GameStrings.Get("GLUE_DECK_TRAY_DECK_COUNT_LABEL");
		string deckCountText = GameStrings.Format("GLUE_DECK_TRAY_COUNT", deckCount, 27);
		m_countLabelText.Text = deckLabel;
		m_countText.Text = deckCountText;
	}

	private void OnTeamCountUpdated(int teamCount)
	{
		string teamLabel = GameStrings.Get("GLUE_DECK_TRAY_TEAM_COUNT_LABEL");
		string teamCountText = GameStrings.Format("GLUE_DECK_TRAY_COUNT", teamCount, 9);
		m_countLabelText.Text = teamLabel;
		m_countText.Text = teamCountText;
	}

	private void OnMercCountUpdated(int mercCount)
	{
		string cardLabel = GameStrings.Get("GLUE_DECK_TRAY_MERC_COUNT_LABEL");
		string cardCountText = GameStrings.Format("GLUE_DECK_TRAY_COUNT", mercCount, CollectionManager.Get().GetTeamSize());
		m_countLabelText.Text = cardLabel;
		m_countText.Text = cardCountText;
	}

	private void OnDeckCreated(long deckID, string name)
	{
		ResetDeckTrayScroll();
	}

	private void OnTeamCreated(long deckID)
	{
		ResetDeckTrayScroll();
	}

	private void OnCMViewModeChanged(CollectionUtils.ViewMode prevMode, CollectionUtils.ViewMode mode, CollectionUtils.ViewModeData userdata, bool triggerResponse)
	{
		DeckContentTypes contentType = GetContentTypeFromViewMode(mode);
		m_cardsContent.ShowFakeDeck(mode == CollectionUtils.ViewMode.DECK_TEMPLATE);
		if (triggerResponse)
		{
			m_decksContent.UpdateDeckName(null, shouldValidateDeckName: false);
			if (m_currentContent != 0)
			{
				SetTrayMode(contentType);
			}
		}
	}

	private DeckContentTypes GetContentTypeFromViewMode(CollectionUtils.ViewMode viewMode)
	{
		return viewMode switch
		{
			CollectionUtils.ViewMode.CARD_BACKS => DeckContentTypes.CardBack, 
			CollectionUtils.ViewMode.HERO_SKINS => DeckContentTypes.HeroSkin, 
			CollectionUtils.ViewMode.COINS => DeckContentTypes.Coin, 
			_ => DeckContentTypes.Cards, 
		};
	}

	private void OnHeroAssigned(string cardID)
	{
		m_decksContent.UpdateEditingDeckBoxVisual(cardID, null);
	}

	private CollectionCardEventHandler GetCardEventHandler(string cardID)
	{
		CollectionCardEventHandlerData handlerData = m_cardEventHandlers.Find((CollectionCardEventHandlerData data) => data.CardID == cardID);
		if (handlerData != null)
		{
			if (handlerData.GetInstance() == null)
			{
				CollectionCardEventHandler handler = UnityEngine.Object.Instantiate(handlerData.CardHandlerPrefab);
				handler.transform.parent = base.transform;
				TransformUtil.Identity(handler);
				handlerData.SetInstance(handler);
			}
			return handlerData.GetInstance();
		}
		int cardDBID = GameUtils.TranslateCardIdToDbId(cardID);
		for (int i = 0; i < m_tagCardEventHandlers.Count; i++)
		{
			CollectionTagEventHandlerData data2 = m_tagCardEventHandlers[i];
			if (GameUtils.GetCardTagValue(cardDBID, data2.Tag) != 0)
			{
				return data2.cardHandlerInstance;
			}
		}
		return m_defaultCardEventHandler;
	}

	private int CalculateNumCardsNeededToCraftToReachMinimumDeckSize(CollectionDeck deck)
	{
		if (deck == null)
		{
			Log.CollectionManager.PrintWarning("GetNumCardsNeededToCraftToReachMinimumDeckSize - No deck to check ruleset against.");
			return 0;
		}
		CollectionDeck wipDeck = new CollectionDeck();
		wipDeck.CopyFrom(deck);
		wipDeck.ClearSlotContents();
		int minimumAllowedDeckSize = wipDeck.GetRuleset(null).GetMinimumAllowedDeckSize(wipDeck);
		IEnumerable<DeckMaker.DeckFill> fillCards = DeckMaker.GetFillCards(wipDeck, wipDeck.GetRuleset(null));
		int countAdded = 0;
		foreach (DeckMaker.DeckFill card in fillCards)
		{
			TAG_PREMIUM? premium = wipDeck.GetPreferredPremiumThatCanBeAdded(card.m_addCard.GetCardId());
			if (premium.HasValue)
			{
				wipDeck.AddCard(card.m_addCard.GetCardId(), premium.Value, false, null);
				countAdded++;
				if (countAdded >= minimumAllowedDeckSize)
				{
					return 0;
				}
			}
		}
		return minimumAllowedDeckSize - countAdded;
	}

	public void HighlightBackButton()
	{
		m_doneButton.GetComponentInChildren<HighlightState>().ChangeState(ActorStateType.HIGHLIGHT_PRIMARY_ACTIVE);
	}

	public Vector3 GetFirstRuneIndicatorButtonPosition()
	{
		return m_runeIndicatorVisual.runeButtons[0].transform.position;
	}

	public void SetRuneIndicatorHighlighted(bool highlighted)
	{
		m_runeIndicatorVisual.HighlightAllRunes(highlighted);
	}

	public DeckTrayCardListContent GetSideboardCardsContent()
	{
		if (!(m_currentSideboardTray != null))
		{
			return null;
		}
		return m_currentSideboardTray.CardsContent;
	}

	public List<CollectibleCard> GetSavedZilliaxVersions()
	{
		if (!(m_ZilliaxsideboardTray != null))
		{
			return null;
		}
		return m_ZilliaxsideboardTray.SavedZilliaxVersions;
	}

	private DeckSideboardTray GetSideboardTrayForSideboardType(TAG_SIDEBOARD_TYPE sideboardType)
	{
		return sideboardType switch
		{
			TAG_SIDEBOARD_TYPE.ETC => m_ETCsideboardTray, 
			TAG_SIDEBOARD_TYPE.ZILLIAX => m_ZilliaxsideboardTray, 
			_ => null, 
		};
	}
}
