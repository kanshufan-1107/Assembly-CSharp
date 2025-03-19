using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Blizzard.T5.Core;
using Hearthstone;
using PegasusShared;
using UnityEngine;

[CustomEditClass]
public class CollectionPageManager : CollectiblePageManager
{
	public static readonly Map<TAG_CLASS, UnityEngine.Vector2> s_classTextureOffsets = new Map<TAG_CLASS, UnityEngine.Vector2>
	{
		{
			TAG_CLASS.MAGE,
			new UnityEngine.Vector2(0f, 0f)
		},
		{
			TAG_CLASS.PALADIN,
			new UnityEngine.Vector2(0.205f, 0f)
		},
		{
			TAG_CLASS.PRIEST,
			new UnityEngine.Vector2(0.392f, 0f)
		},
		{
			TAG_CLASS.ROGUE,
			new UnityEngine.Vector2(0.58f, 0f)
		},
		{
			TAG_CLASS.SHAMAN,
			new UnityEngine.Vector2(0.774f, 0f)
		},
		{
			TAG_CLASS.WARLOCK,
			new UnityEngine.Vector2(0f, -0.2f)
		},
		{
			TAG_CLASS.WARRIOR,
			new UnityEngine.Vector2(0.205f, -0.2f)
		},
		{
			TAG_CLASS.DRUID,
			new UnityEngine.Vector2(0.392f, -0.2f)
		},
		{
			TAG_CLASS.HUNTER,
			new UnityEngine.Vector2(0.58f, -0.2f)
		},
		{
			TAG_CLASS.NEUTRAL,
			new UnityEngine.Vector2(0.774f, -0.2f)
		},
		{
			TAG_CLASS.WHIZBANG,
			new UnityEngine.Vector2(0f, -0.395f)
		},
		{
			TAG_CLASS.DEMONHUNTER,
			new UnityEngine.Vector2(0.205f, -0.4f)
		},
		{
			TAG_CLASS.DEATHKNIGHT,
			new UnityEngine.Vector2(0.392f, -0.4f)
		}
	};

	private static readonly Map<TAG_CLASS, Color> s_classColors = new Map<TAG_CLASS, Color>
	{
		{
			TAG_CLASS.DEATHKNIGHT,
			new Color(1f / 15f, 0.5294118f, 0.58431375f)
		},
		{
			TAG_CLASS.MAGE,
			new Color(11f / 85f, 4f / 15f, 33f / 85f)
		},
		{
			TAG_CLASS.PALADIN,
			new Color(0.4392157f, 0.29411766f, 0.09019608f)
		},
		{
			TAG_CLASS.PRIEST,
			new Color(0.52156866f, 0.52156866f, 0.52156866f)
		},
		{
			TAG_CLASS.ROGUE,
			new Color(19f / 85f, 16f / 85f, 19f / 85f)
		},
		{
			TAG_CLASS.SHAMAN,
			new Color(11f / 85f, 0.17254902f, 19f / 51f)
		},
		{
			TAG_CLASS.WARLOCK,
			new Color(18f / 85f, 0.10980392f, 24f / 85f)
		},
		{
			TAG_CLASS.WARRIOR,
			new Color(14f / 51f, 0.050980393f, 7f / 85f)
		},
		{
			TAG_CLASS.DRUID,
			new Color(0.23137255f, 0.16078432f, 0.08627451f)
		},
		{
			TAG_CLASS.HUNTER,
			new Color(19f / 85f, 0.4627451f, 0.1764706f)
		},
		{
			TAG_CLASS.NEUTRAL,
			new Color(16f / 51f, 0.2784314f, 4f / 15f)
		},
		{
			TAG_CLASS.WHIZBANG,
			new Color(48f / 85f, 0.3019608f, 0.5372549f)
		},
		{
			TAG_CLASS.DEMONHUNTER,
			new Color(0.09019608f, 0.22745098f, 10f / 51f)
		}
	};

	public static TAG_CLASS[] CLASS_TAB_ORDER = new TAG_CLASS[12]
	{
		TAG_CLASS.DEATHKNIGHT,
		TAG_CLASS.DEMONHUNTER,
		TAG_CLASS.DRUID,
		TAG_CLASS.HUNTER,
		TAG_CLASS.MAGE,
		TAG_CLASS.PALADIN,
		TAG_CLASS.PRIEST,
		TAG_CLASS.ROGUE,
		TAG_CLASS.SHAMAN,
		TAG_CLASS.WARLOCK,
		TAG_CLASS.WARRIOR,
		TAG_CLASS.NEUTRAL
	};

	public CollectionClassTab m_heroSkinsTab;

	public CollectionClassTab m_cardBacksTab;

	public CollectionClassTab m_coinsTab;

	public ClassFilterHeaderButton m_classFilterHeader;

	public CollectionClassTab m_deckTemplateTab;

	public BookTab m_ZilliaxModulesTab;

	public BookTab m_ZilliaxBlueprintsTab;

	[CustomEditField(Sections = "Deck Template", T = EditType.GAME_OBJECT)]
	public string m_deckTemplatePickerPrefab;

	private static CollectionUtils.ViewMode[] TAG_ORDERING = new CollectionUtils.ViewMode[5]
	{
		CollectionUtils.ViewMode.CARDS,
		CollectionUtils.ViewMode.COINS,
		CollectionUtils.ViewMode.CARD_BACKS,
		CollectionUtils.ViewMode.HERO_PICKER,
		CollectionUtils.ViewMode.HERO_SKINS
	};

	private static readonly int NUM_PAGE_FLIPS_BEFORE_SET_FILTER_TUTORIAL = 3;

	protected static readonly string ANIMATE_TABS_COROUTINE_NAME = "AnimateTabs";

	private List<CollectionClassTab> m_classTabs = new List<CollectionClassTab>();

	private List<BookTab> m_zilliaxTabs = new List<BookTab>();

	private MassDisenchant m_massDisenchant;

	private DeckTemplatePicker m_deckTemplatePicker;

	private CollectibleCardHeroesFilter m_heroesCollection = new CollectibleCardHeroesFilter();

	private Vector3 m_heroSkinsTabPos;

	private Vector3 m_cardBacksTabPos;

	private Vector3 m_coinsTabPos;

	private bool m_hideNonDeckTemplateTabs;

	private int m_numPageFlipsThisSession;

	protected CollectionTabInfo m_currentClassContext;

	protected ICollectible m_lastCollectibleAnchor;

	private readonly List<CollectionCardVisual> m_ghostedRuneCards = new List<CollectionCardVisual>();

	private readonly List<CollectionCardVisual> m_ghostedTouristCards = new List<CollectionCardVisual>();

	private string m_searchText;

	private List<CollectibleCard> m_disenchantCards = new List<CollectibleCard>();

	private Dictionary<TAG_CLASS, bool> m_shouldClassHavePersistentGlow = new Dictionary<TAG_CLASS, bool>();

	private TAG_CLASS m_currentClass;

	private bool m_suppressTouristTooltip;

	private bool m_deckRunesWereUpdatedOnCurrentPage;

	private RunePattern m_originalDeckRunesForCurrentPage;

	private static Comparison<CollectibleCard> OrderedZilliaxModulesSort = delegate(CollectibleCard a, CollectibleCard b)
	{
		int num = a.ManaCost.CompareTo(b.ManaCost);
		if (num == 0)
		{
			num = string.Compare(a.Name, b.Name, ignoreCase: false, Localization.GetCultureInfo());
		}
		return num;
	};

	private static Comparison<CollectibleCard> OrderedZilliaxSavedVersionsSort = delegate(CollectibleCard a, CollectibleCard b)
	{
		int num2 = a.ManaCost.CompareTo(b.ManaCost);
		if (num2 == 0)
		{
			num2 = a.CardDbId.CompareTo(b.CardDbId);
		}
		if (num2 == 0)
		{
			num2 = a.GetEntityDef().GetTag(GAME_TAG.MODULAR_ENTITY_PART_1).CompareTo(b.GetEntityDef().GetTag(GAME_TAG.MODULAR_ENTITY_PART_1));
		}
		if (num2 == 0)
		{
			num2 = a.GetEntityDef().GetTag(GAME_TAG.MODULAR_ENTITY_PART_2).CompareTo(b.GetEntityDef().GetTag(GAME_TAG.MODULAR_ENTITY_PART_2));
		}
		return num2;
	};

	private const float DK_TUTORIAL_RUNE_POPUP_OFFSET_X_PC = 13f;

	private const float DK_TUTORIAL_RUNE_POPUP_OFFSET_X_PHONE = 14f;

	private const float DK_TUTORIAL_RUNE_POPUP_SCALE = 15f;

	private const float DK_TUTORIAL_RUNE_INDICATOR_ARROW_OFFSET_X_PC = -6f;

	private const float DK_TUTORIAL_RUNE_INDICATOR_ARROW_OFFSET_X_PHONE = -9f;

	private const float DK_TUTORIAL_RUNE_INDICATOR_ARROW_SCALE_PC = 7f;

	private const float DK_TUTORIAL_RUNE_INDICATOR_ARROW_SCALE_PHONE = 7f;

	private const float DK_TUTORIAL_RUNE_INDICATOR_ARROW_ROTATION = 90f;

	private Notification m_deathKnightRuneTutorialRunePopup;

	private Notification m_runeIndicatorArrow;

	private CollectibleCardClassFilter m_classCardsCollection => (CollectibleCardClassFilter)m_cardsCollection;

	public bool SuppressTouristTooltip
	{
		get
		{
			return m_suppressTouristTooltip;
		}
		set
		{
			if (value != m_suppressTouristTooltip)
			{
				m_suppressTouristTooltip = value;
				this.OnSuppressTouristTooltipChanged?.Invoke(m_suppressTouristTooltip);
			}
		}
	}

	public bool IsManaCostFilterActive
	{
		get
		{
			if (m_cardsCollection != null)
			{
				return m_cardsCollection.IsManaCostFilterActive;
			}
			return false;
		}
	}

	public static bool IsShowingLockedRuneCards { get; private set; }

	public event Action OnZilliaxTabPressed;

	public event Action<TAG_CLASS> OnCollectionClassTabHovered;

	public event Action<TAG_CLASS> OnCurrentClassChanged;

	public event Action<bool> OnSuppressTouristTooltipChanged;

	public static Color ColorForClass(TAG_CLASS tagClass)
	{
		return s_classColors[tagClass];
	}

	protected override void Awake()
	{
		base.Awake();
		m_cardsCollection = new CollectibleCardClassFilter();
		m_classCardsCollection.Init(CollectiblePageDisplay.GetMaxCardsPerPage(CollectionUtils.ViewMode.CARDS));
		m_heroesCollection.Init(CollectiblePageDisplay.GetMaxCardsPerPage(CollectionUtils.ViewMode.HERO_SKINS));
		UpdateFilteredCards();
		m_heroesCollection.UpdateResults();
		if ((bool)m_massDisenchant)
		{
			m_massDisenchant.Hide();
		}
		CollectionManager.Get()?.RegisterFavoriteHeroChangedListener(OnFavoriteHeroChanged);
		IsShowingLockedRuneCards = true;
		NetCache.Get().FavoriteCardBackChanged += OnFavoriteCardBackChanged;
		NetCache.Get().FavoriteBattlegroundsGuideSkinChanged += OnFavoriteBattlegroundsGuideSkinChanged;
		NetCache.Get().FavoriteCoinChanged += OnFavoriteCoinChanged;
	}

	private void OnEnable()
	{
		CollectionManagerDisplay.HideLockedRunesCheckboxToggled += OnHideLockedRunesCheckboxToggled;
		RuneIndicatorVisual.RunePatternChanged += RuneIndicatorVisualOnRunePatternChanged;
		CollectionDeckTray.DeckTrayRunesAdded += OnDeckTrayRunesAdded;
		CraftingTray.CraftingTrayShown += OnCraftingTrayShown;
		CraftingTray.CraftingTrayHidden += OnCraftingTrayHidden;
	}

	private void OnDisable()
	{
		CollectionManagerDisplay.HideLockedRunesCheckboxToggled -= OnHideLockedRunesCheckboxToggled;
		RuneIndicatorVisual.RunePatternChanged -= RuneIndicatorVisualOnRunePatternChanged;
		CollectionDeckTray.DeckTrayRunesAdded -= OnDeckTrayRunesAdded;
		CraftingTray.CraftingTrayShown -= OnCraftingTrayShown;
		CraftingTray.CraftingTrayHidden -= OnCraftingTrayHidden;
	}

	public override void OnDestroy()
	{
		base.OnDestroy();
		CollectionManager.Get()?.RemoveFavoriteHeroChangedListener(OnFavoriteHeroChanged);
		if (NetCache.Get() != null)
		{
			NetCache.Get().FavoriteCardBackChanged -= OnFavoriteCardBackChanged;
			NetCache.Get().FavoriteBattlegroundsGuideSkinChanged -= OnFavoriteBattlegroundsGuideSkinChanged;
			NetCache.Get().FavoriteCoinChanged -= OnFavoriteCoinChanged;
		}
	}

	public bool HideNonDeckTemplateTabs(bool hide, bool updateTabs = false)
	{
		if (m_hideNonDeckTemplateTabs == hide)
		{
			return false;
		}
		m_hideNonDeckTemplateTabs = hide;
		if (updateTabs)
		{
			UpdateVisibleTabs();
		}
		return true;
	}

	public bool IsNonDeckTemplateTabsHidden()
	{
		return m_hideNonDeckTemplateTabs;
	}

	public bool ShouldShowAllClassCards(CollectionDeck deck)
	{
		if (deck == null)
		{
			return false;
		}
		DeckRuleset ruleset = deck.GetRuleset(null);
		if (ruleset == null)
		{
			return false;
		}
		bool shouldShowAllClassCards = false;
		if (ruleset.EntityInDeckIgnoresRuleset(deck))
		{
			shouldShowAllClassCards = true;
		}
		else if (!ruleset.Rules.Where((DeckRule r) => r.Type == DeckRule.RuleType.IS_CLASS_CARD_OR_NEUTRAL).Any())
		{
			shouldShowAllClassCards = true;
		}
		return shouldShowAllClassCards;
	}

	public void UpdateFiltersForDeck(CollectionDeck deck, List<TAG_CLASS> deckClasses, bool skipPageTurn, DelOnPageTransitionComplete callback = null, object callbackData = null)
	{
		m_skipNextPageTurn = skipPageTurn;
		bool showAllClassCards = false;
		bool showOnlyOtherClassCards = false;
		if (deck != null && deck.GetRuleset(null) != null)
		{
			DeckRuleset ruleset = deck.GetRuleset(null);
			if (ruleset.EntityInDeckIgnoresRuleset(deck))
			{
				showAllClassCards = true;
			}
			else
			{
				IEnumerable<DeckRule> classOrNeutralRules = ruleset.Rules.Where((DeckRule r) => r.Type == DeckRule.RuleType.IS_CLASS_CARD_OR_NEUTRAL);
				if (classOrNeutralRules.Any((DeckRule r) => r.RuleIsNot))
				{
					showOnlyOtherClassCards = true;
				}
				else if (!classOrNeutralRules.Any())
				{
					showAllClassCards = true;
				}
			}
		}
		if (showAllClassCards)
		{
			m_classCardsCollection.FilterTheseClasses(null);
		}
		else if (showOnlyOtherClassCards)
		{
			m_classCardsCollection.FilterTheseClasses(CLASS_TAB_ORDER.Where((TAG_CLASS tag) => !deckClasses.Contains(tag)).ToArray());
		}
		else
		{
			List<TAG_CLASS> filterClasses = new List<TAG_CLASS>(deckClasses);
			filterClasses.Add(TAG_CLASS.NEUTRAL);
			m_classCardsCollection.FilterTheseClasses(filterClasses.ToArray());
		}
		m_heroesCollection.FilterOnlyOwned(owned: true);
		m_heroesCollection.UpdateResults();
		UpdateFilteredCards();
		UpdateVisibleTabs();
		bool updatePages = true;
		CollectionUtils.ViewMode viewMode = CollectionManager.Get().GetCollectibleDisplay().GetViewMode();
		if (viewMode == CollectionUtils.ViewMode.DECK_TEMPLATE || viewMode == CollectionUtils.ViewMode.MASS_DISENCHANT)
		{
			updatePages = false;
		}
		if (updatePages)
		{
			switch (viewMode)
			{
			case CollectionUtils.ViewMode.CARDS:
				JumpToCollectionClassPage(new CollectionTabInfo
				{
					tagClass = deckClasses.First()
				}, callback, callbackData);
				break;
			case CollectionUtils.ViewMode.HERO_SKINS:
			case CollectionUtils.ViewMode.CARD_BACKS:
			case CollectionUtils.ViewMode.COINS:
				m_currentPageNum = 1;
				TransitionPageWhenReady(PageTransitionType.NONE, useCurrentPageNum: false, callback, callbackData);
				break;
			}
		}
	}

	public override bool JumpToPageWithCard(string cardID, TAG_PREMIUM premium, DelOnPageTransitionComplete callback, object callbackData)
	{
		return JumpToPageWithCard(cardID, premium, callback, callbackData, tryClearFilters: true);
	}

	private bool JumpToPageWithCard(string cardID, TAG_PREMIUM premium, DelOnPageTransitionComplete callback, object callbackData, bool tryClearFilters)
	{
		CollectionDeck currentEditingDeck = CollectionManager.Get().GetEditedDeck();
		CollectionTabInfo collectionTabInfo = default(CollectionTabInfo);
		collectionTabInfo.tagClass = TAG_CLASS.INVALID;
		CollectionTabInfo currentClassContext = collectionTabInfo;
		if (currentEditingDeck != null)
		{
			currentClassContext.tagClass = currentEditingDeck.GetClass();
		}
		if (m_classCardsCollection.GetPageContentsForCard(cardID, premium, out var collectionPage, currentClassContext).Count == 0)
		{
			if (tryClearFilters)
			{
				CollectionManagerDisplay cmd = CollectionManager.Get().GetCollectibleDisplay() as CollectionManagerDisplay;
				if (cmd != null)
				{
					cmd.ResetFilters(updateVisuals: false);
				}
				return JumpToPageWithCard(cardID, premium, callback, callbackData, tryClearFilters: false);
			}
			return false;
		}
		if (m_currentPageNum == collectionPage)
		{
			return false;
		}
		FlipToPage(collectionPage, callback, callbackData);
		return true;
	}

	public void FilterByManaCost(int cost, bool transitionPage = true)
	{
		if (cost == -1)
		{
			m_cardsCollection.FilterManaCost(null);
		}
		else
		{
			m_cardsCollection.FilterManaCost(cost);
		}
		UpdateFilteredCards();
		if (transitionPage)
		{
			TransitionPageWhenReady(PageTransitionType.NONE, useCurrentPageNum: false, null, null);
		}
	}

	public override void ChangeSearchTextFilter(string newSearchText, DelOnPageTransitionComplete callback, object callbackData, bool transitionPage = true)
	{
		if (newSearchText == "")
		{
			RemoveSearchTextFilter(callback, callbackData, transitionPage);
			return;
		}
		m_searchText = newSearchText;
		UpdateNonCardSearchTextFilters();
		base.ChangeSearchTextFilter(m_searchText, callback, callbackData, transitionPage);
	}

	public override void RemoveSearchTextFilter(DelOnPageTransitionComplete callback, object callbackData, bool transitionPage = true)
	{
		m_searchText = null;
		UpdateNonCardSearchTextFilters();
		base.RemoveSearchTextFilter(callback, callbackData, transitionPage);
	}

	private void UpdateNonCardSearchTextFilters()
	{
		CardBackManager.Get().SetSearchText(m_searchText);
		CosmeticCoinManager.Get().SetSearchText(m_searchText);
		CollectionManagerDisplay cmd = CollectionManager.Get().GetCollectibleDisplay() as CollectionManagerDisplay;
		if (cmd != null)
		{
			switch (cmd.GetViewMode())
			{
			case CollectionUtils.ViewMode.HERO_SKINS:
				if (!IsSearching() && !CollectionManager.Get().IsInEditMode())
				{
					cmd.SetViewMode(CollectionUtils.ViewMode.HERO_PICKER, triggerResponse: false);
				}
				else
				{
					cmd.SetHeroSkinClass(null);
				}
				break;
			case CollectionUtils.ViewMode.HERO_PICKER:
				if (IsSearching())
				{
					cmd.SetViewMode(CollectionUtils.ViewMode.HERO_SKINS, triggerResponse: false);
				}
				break;
			}
		}
		m_heroesCollection.FilterSearchText(m_searchText);
		m_heroesCollection.UpdateResults();
	}

	public bool IsSearching()
	{
		return m_searchText != null;
	}

	public string GetSearchText()
	{
		return m_searchText;
	}

	public void UpdateClassTabNewCardCounts()
	{
		foreach (CollectionClassTab classTab in m_classTabs)
		{
			TAG_CLASS tabClass = classTab.TabInfo.tagClass;
			int numNewCardsForClass = ((classTab.m_tabViewMode != CollectionUtils.ViewMode.DECK_TEMPLATE) ? GetNumNewCardsForClass(tabClass) : 0);
			classTab.UpdateNewItemCount(numNewCardsForClass);
		}
	}

	public int GetNumNewCardsForClass(TAG_CLASS tagClass)
	{
		return m_classCardsCollection.GetNumNewCardsForTab(new CollectionTabInfo
		{
			tagClass = tagClass
		});
	}

	public override void NotifyOfCollectionChanged()
	{
		UpdateMassDisenchant();
	}

	public void OnDoneEditingDeck()
	{
		FormatType formatType = CollectionManager.Get().GetThemeShowing();
		List<TAG_CLASS> deckClasses = RankMgr.GetExcludedClassesForFormat(formatType);
		m_cardsCollection.FilterTheseClasses(CLASS_TAB_ORDER.Where((TAG_CLASS tag) => !deckClasses.Contains(tag)).ToArray());
		UpdateFilteredCards();
		m_heroesCollection.FilterTheseClasses(null);
		m_heroesCollection.FilterOnlyOwned(owned: false);
		m_heroesCollection.UpdateResults();
		PageTransitionType transitionType = ((CollectionManager.Get().GetCollectibleDisplay().GetViewMode() == CollectionUtils.ViewMode.CARDS) ? PageTransitionType.SINGLE_PAGE_LEFT : PageTransitionType.NONE);
		TransitionPageWhenReady(transitionType, useCurrentPageNum: false, null, null);
		UpdateCraftingModeButtonDustBottleVisibility(CollectionManager.Get().GetCardsToMassDisenchantCount());
		NotificationManager.Get().DestroyNotificationWithText(GameStrings.Get("GLUE_COLLECTION_TUTORIAL_TEMPLATE_REPLACE_1"));
		NotificationManager.Get().DestroyNotificationWithText(GameStrings.Get("GLUE_COLLECTION_TUTORIAL_TEMPLATE_REPLACE_2"));
		NotificationManager.Get().DestroyNotificationWithText(GameStrings.Get("GLUE_COLLECTION_TUTORIAL_REPLACE_WILD_CARDS"));
		NotificationManager.Get().DestroyNotificationWithText(GameStrings.Get("GLUE_COLLECTION_TUTORIAL_REPLACE_WILD_CARDS_NPR"));
		CollectionDeckTray.Get().GetCardsContent().HideDeckHelpPopup();
	}

	public void UpdateCraftingModeButtonDustBottleVisibility(int disenchantCount)
	{
		CollectibleDisplay collectibleDisplay = CollectionManager.Get().GetCollectibleDisplay();
		bool isCardsMode = collectibleDisplay.GetViewMode() == CollectionUtils.ViewMode.CARDS;
		bool isOnPhoneDisenchantView = (bool)UniversalInputManager.UsePhoneUI && collectibleDisplay.GetViewMode() == CollectionUtils.ViewMode.MASS_DISENCHANT;
		bool showBottle = (isCardsMode || isOnPhoneDisenchantView) && (disenchantCount > 0 || (bool)UniversalInputManager.UsePhoneUI);
		CollectionManagerDisplay cmd = collectibleDisplay as CollectionManagerDisplay;
		if (cmd != null)
		{
			cmd.m_craftingModeButton.ShowDustBottle(showBottle, isCardsMode);
		}
	}

	public int GetMassDisenchantAmount()
	{
		return CollectionManager.Get().GetCardsToMassDisenchantCount();
	}

	public void LoadMassDisenchantScreen()
	{
		if (!(m_massDisenchant != null))
		{
			GameObject screen = AssetLoader.Get().InstantiatePrefab("MassDisenchant.prefab:0bfb8a7db15d748b291be3096753ca24");
			m_massDisenchant = screen.GetComponent<MassDisenchant>();
			m_massDisenchant.Hide();
		}
	}

	public bool HasClassCardsAvailable(TAG_CLASS classTag)
	{
		return m_classCardsCollection.GetNumPagesForTab(new CollectionTabInfo
		{
			tagClass = classTag
		}) > 0;
	}

	public bool HasAnyCardsAvailable()
	{
		return m_classCardsCollection.GetTotalNumPages() > 0;
	}

	public TAG_CLASS GetCurrentClassContextClassTag()
	{
		return m_currentClassContext.tagClass;
	}

	public void ShowCraftingModeCards(DelOnPageTransitionComplete callback = null, object callbackData = null, bool showUncraftable = false, bool showNormalOwned = false, bool showNormalMissing = false, bool showPremiumOwned = false, bool showPremiumMissing = false, bool updatePage = true, bool toggleChanged = false)
	{
		List<CollectibleCardFilter.FilterMask> filterMasks = new List<CollectibleCardFilter.FilterMask>();
		if (showNormalOwned)
		{
			filterMasks.Add(CollectibleCardFilter.FilterMask.PREMIUM_NORMAL | CollectibleCardFilter.FilterMask.OWNED);
		}
		if (showNormalMissing)
		{
			filterMasks.Add(CollectibleCardFilter.FilterMask.PREMIUM_NORMAL | CollectibleCardFilter.FilterMask.UNOWNED);
		}
		if (showPremiumOwned)
		{
			filterMasks.Add(CollectibleCardFilter.FilterMask.PREMIUM_GOLDEN | CollectibleCardFilter.FilterMask.OWNED);
			filterMasks.Add(CollectibleCardFilter.FilterMask.PREMIUM_DIAMOND | CollectibleCardFilter.FilterMask.OWNED);
			filterMasks.Add(CollectibleCardFilter.FilterMask.PREMIUM_SIGNATURE | CollectibleCardFilter.FilterMask.OWNED);
		}
		if (showPremiumMissing)
		{
			filterMasks.Add(CollectibleCardFilter.FilterMask.PREMIUM_GOLDEN | CollectibleCardFilter.FilterMask.UNOWNED);
			filterMasks.Add(CollectibleCardFilter.FilterMask.PREMIUM_DIAMOND | CollectibleCardFilter.FilterMask.UNOWNED);
			filterMasks.Add(CollectibleCardFilter.FilterMask.PREMIUM_SIGNATURE | CollectibleCardFilter.FilterMask.UNOWNED);
		}
		bool? craftableFilter = null;
		if (!showUncraftable)
		{
			craftableFilter = true;
		}
		m_cardsCollection.FilterOnlyOwned(owned: false);
		m_cardsCollection.FilterByMask(filterMasks);
		m_cardsCollection.FilterByCraftability(craftableFilter);
		m_cardsCollection.FilterLeagueBannedCardsSubset(RankMgr.Get().GetBannedCardsInCurrentLeague());
		UpdateFilteredCards();
		if (toggleChanged)
		{
			m_lastCollectibleAnchor = null;
		}
		if (updatePage)
		{
			PageTransitionType transitionType = (toggleChanged ? PageTransitionType.MANY_PAGE_LEFT : PageTransitionType.NONE);
			TransitionPageWhenReady(transitionType, useCurrentPageNum: false, callback, callbackData);
		}
	}

	protected override bool CanUserTurnPages()
	{
		if (CraftingManager.GetIsInCraftingMode())
		{
			return false;
		}
		CardBackInfoManager cardBackInfoManager = CardBackInfoManager.Get();
		if (cardBackInfoManager != null && cardBackInfoManager.IsPreviewing)
		{
			return false;
		}
		HeroSkinInfoManager heroSkinInfoManager = HeroSkinInfoManager.Get();
		if (heroSkinInfoManager != null && heroSkinInfoManager.IsShowingPreview)
		{
			return false;
		}
		return base.CanUserTurnPages();
	}

	private CollectionPageDisplay PageAsCollectionPage(BookPageDisplay page)
	{
		if (!(page is CollectionPageDisplay))
		{
			Log.CollectionManager.PrintError("Page in CollectionPageManager is not a CollectionPageDisplay!  This should not happen!");
		}
		return page as CollectionPageDisplay;
	}

	protected override bool ShouldShowTab(BookTab tab)
	{
		if (!m_initializedTabPositions)
		{
			return true;
		}
		if (m_hideNonDeckTemplateTabs)
		{
			return tab.m_tabViewMode == CollectionUtils.ViewMode.DECK_TEMPLATE;
		}
		CollectionDeck currentEditingDeck = CollectionManager.Get().GetEditedDeck();
		bool isEditingDeck = currentEditingDeck != null;
		CollectionUtils.ViewSubmode currentSubmode = CollectionManager.Get().GetCollectibleDisplay().GetViewSubmode();
		switch (tab.m_tabViewMode)
		{
		case CollectionUtils.ViewMode.CARDS:
		{
			if (m_zilliaxTabs.Contains(tab))
			{
				return currentSubmode == CollectionUtils.ViewSubmode.CARD_ZILLIAX_MODULES;
			}
			CollectionClassTab classTab = tab as CollectionClassTab;
			if (classTab == null)
			{
				Log.CollectionManager.PrintError("CollectionPageManager.ShouldShowTab passed a non-CollectionClassTab object.");
				return false;
			}
			if (currentSubmode == CollectionUtils.ViewSubmode.CARD_ZILLIAX_MODULES)
			{
				return false;
			}
			TAG_CLASS currentClass = classTab.TabInfo.tagClass;
			if (HasClassCardsAvailable(currentClass))
			{
				return true;
			}
			if (isEditingDeck && !HasAnyCardsAvailable())
			{
				return currentEditingDeck.GetHeroClasses().Contains(currentClass);
			}
			return false;
		}
		case CollectionUtils.ViewMode.DECK_TEMPLATE:
			if (isEditingDeck)
			{
				return !SceneMgr.Get().IsInTavernBrawlMode();
			}
			return false;
		case CollectionUtils.ViewMode.HERO_SKINS:
		{
			if (isEditingDeck)
			{
				if (currentEditingDeck.HasUIHeroOverride())
				{
					return false;
				}
				if (currentSubmode == CollectionUtils.ViewSubmode.CARD_ZILLIAX_MODULES)
				{
					return false;
				}
				return CollectionManager.Get().GetCountOfOwnedHeroesForClass(currentEditingDeck.GetClass()) > 1;
			}
			List<CollectibleCard> matchingSkins = m_heroesCollection.GetAllResults();
			if (matchingSkins != null && matchingSkins.Count > 0 && !m_classCardsCollection.IsManaCostFilterActive && !m_classCardsCollection.IsSingleSetFilterActive && IsSearching())
			{
				return true;
			}
			return HasAnyCardsAvailable();
		}
		case CollectionUtils.ViewMode.CARD_BACKS:
		{
			if (isEditingDeck)
			{
				if (currentSubmode == CollectionUtils.ViewSubmode.CARD_ZILLIAX_MODULES)
				{
					return false;
				}
				HashSet<int> cardBacksOwned = CardBackManager.Get().GetCardBacksOwned();
				if (cardBacksOwned != null)
				{
					return cardBacksOwned.Count > 1;
				}
				return false;
			}
			HashSet<int> cardBacks = GetCurrentDeckTrayModeCardBackIds();
			if (cardBacks != null && cardBacks.Count > 0 && !m_classCardsCollection.IsManaCostFilterActive && !m_classCardsCollection.IsSingleSetFilterActive && IsSearching())
			{
				return true;
			}
			return HasAnyCardsAvailable();
		}
		case CollectionUtils.ViewMode.COINS:
		{
			if (isEditingDeck)
			{
				if (currentSubmode == CollectionUtils.ViewSubmode.CARD_ZILLIAX_MODULES)
				{
					return false;
				}
				return CosmeticCoinManager.Get().GetTotalCoinsOwned() > 1;
			}
			List<CollectibleCard> coinCards = GetCurrentDeckTrayModeCosmeticCoins();
			if (coinCards != null && coinCards.Count > 0 && !m_classCardsCollection.IsManaCostFilterActive && !m_classCardsCollection.IsSingleSetFilterActive && IsSearching())
			{
				return true;
			}
			return HasAnyCardsAvailable();
		}
		default:
			return true;
		}
	}

	private void SetupClassTab(CollectionClassTab classTab, TAG_CLASS classTag, string tabName, bool isTouch)
	{
		classTab.Init(classTag);
		classTab.transform.localScale = classTab.m_DeselectedLocalScale;
		classTab.transform.localEulerAngles = CollectiblePageManager.TAB_LOCAL_EULERS;
		classTab.AddEventListener(UIEventType.RELEASE, OnClassTabPressed);
		classTab.AddEventListener(UIEventType.ROLLOVER, OnTabOver);
		classTab.AddEventListener(UIEventType.ROLLOUT, base.OnTabOut);
		classTab.AddEventListener(UIEventType.ROLLOVER, base.OnTabOver_Touch);
		classTab.AddEventListener(UIEventType.ROLLOUT, base.OnTabOut_Touch);
		classTab.SetReceiveReleaseWithoutMouseDown(isTouch);
		classTab.gameObject.name = tabName;
	}

	private void SetupZilliaxTab(BookTab zilliaxTab, string tabName, bool isTouch, UIEvent.Handler onReleaseHandler)
	{
		zilliaxTab.Init();
		zilliaxTab.transform.localScale = zilliaxTab.m_DeselectedLocalScale;
		zilliaxTab.transform.localEulerAngles = CollectiblePageManager.TAB_LOCAL_EULERS;
		zilliaxTab.AddEventListener(UIEventType.RELEASE, onReleaseHandler);
		zilliaxTab.AddEventListener(UIEventType.ROLLOVER, OnTabOver);
		zilliaxTab.AddEventListener(UIEventType.ROLLOUT, base.OnTabOut);
		zilliaxTab.AddEventListener(UIEventType.ROLLOVER, base.OnTabOver_Touch);
		zilliaxTab.AddEventListener(UIEventType.ROLLOUT, base.OnTabOut_Touch);
		zilliaxTab.SetReceiveReleaseWithoutMouseDown(isTouch);
		zilliaxTab.gameObject.name = tabName;
		m_allTabs.Add(zilliaxTab);
		m_zilliaxTabs.Add(zilliaxTab);
		m_tabVisibility[zilliaxTab] = false;
	}

	protected override void SetUpBookTabs()
	{
		if ((bool)UniversalInputManager.UsePhoneUI)
		{
			return;
		}
		bool isTouch = UniversalInputManager.Get().IsTouchMode();
		if (m_deckTemplateTab != null && m_deckTemplateTab.gameObject.activeSelf)
		{
			m_allTabs.Add(m_deckTemplateTab);
			m_classTabs.Add(m_deckTemplateTab);
			m_deckTemplateTab.AddEventListener(UIEventType.RELEASE, OnDeckTemplateTabPressed);
			m_deckTemplateTab.AddEventListener(UIEventType.ROLLOVER, OnTabOver);
			m_deckTemplateTab.AddEventListener(UIEventType.ROLLOUT, base.OnTabOut);
			m_deckTemplateTab.AddEventListener(UIEventType.ROLLOVER, base.OnTabOver_Touch);
			m_deckTemplateTab.AddEventListener(UIEventType.ROLLOUT, base.OnTabOut_Touch);
			m_deckTemplateTab.SetReceiveReleaseWithoutMouseDown(isTouch);
			m_tabVisibility[m_deckTemplateTab] = true;
		}
		for (int i = 0; i < CLASS_TAB_ORDER.Length; i++)
		{
			TAG_CLASS classTag = CLASS_TAB_ORDER[i];
			CollectionClassTab classTab = (CollectionClassTab)GameUtils.Instantiate(m_tabPrefab, m_tabContainer);
			SetupClassTab(classTab, classTag, classTag.ToString(), isTouch);
			m_allTabs.Add(classTab);
			m_classTabs.Add(classTab);
			m_tabVisibility[classTab] = true;
			if (i <= 0)
			{
				m_deselectedTabHalfWidth = classTab.GetComponent<BoxCollider>().bounds.extents.x;
			}
		}
		SetupZilliaxTab(m_ZilliaxModulesTab, "ZILLIAX_FUNCTIONAL", isTouch, OnZilliaxFunctionalTabPressed);
		SetupZilliaxTab(m_ZilliaxBlueprintsTab, "ZILLIAX_VERSIONS", isTouch, OnZilliaxSavedVersionsTabPressed);
		if (m_heroSkinsTab != null)
		{
			m_heroSkinsTab.Init(TAG_CLASS.NEUTRAL);
			m_heroSkinsTab.AddEventListener(UIEventType.RELEASE, OnHeroSkinsTabPressed);
			m_heroSkinsTab.AddEventListener(UIEventType.ROLLOVER, OnTabOver);
			m_heroSkinsTab.AddEventListener(UIEventType.ROLLOUT, base.OnTabOut);
			m_heroSkinsTab.AddEventListener(UIEventType.ROLLOVER, base.OnTabOver_Touch);
			m_heroSkinsTab.AddEventListener(UIEventType.ROLLOUT, base.OnTabOut_Touch);
			m_heroSkinsTab.SetReceiveReleaseWithoutMouseDown(isTouch);
			m_allTabs.Add(m_heroSkinsTab);
			m_tabVisibility[m_heroSkinsTab] = true;
			m_heroSkinsTabPos = m_heroSkinsTab.transform.localPosition;
		}
		if (m_cardBacksTab != null)
		{
			m_cardBacksTab.Init(TAG_CLASS.NEUTRAL);
			m_cardBacksTab.AddEventListener(UIEventType.RELEASE, OnCardBacksTabPressed);
			m_cardBacksTab.AddEventListener(UIEventType.ROLLOVER, OnTabOver);
			m_cardBacksTab.AddEventListener(UIEventType.ROLLOUT, base.OnTabOut);
			m_cardBacksTab.AddEventListener(UIEventType.ROLLOVER, base.OnTabOver_Touch);
			m_cardBacksTab.AddEventListener(UIEventType.ROLLOUT, base.OnTabOut_Touch);
			m_cardBacksTab.SetReceiveReleaseWithoutMouseDown(isTouch);
			m_allTabs.Add(m_cardBacksTab);
			m_tabVisibility[m_cardBacksTab] = true;
			m_cardBacksTabPos = m_cardBacksTab.transform.localPosition;
		}
		if (m_coinsTab != null)
		{
			m_coinsTab.Init(TAG_CLASS.NEUTRAL);
			m_coinsTab.AddEventListener(UIEventType.RELEASE, OnCoinsTabPressed);
			m_coinsTab.AddEventListener(UIEventType.ROLLOVER, OnTabOver);
			m_coinsTab.AddEventListener(UIEventType.ROLLOUT, base.OnTabOut);
			m_coinsTab.AddEventListener(UIEventType.ROLLOVER, base.OnTabOver_Touch);
			m_coinsTab.AddEventListener(UIEventType.ROLLOUT, base.OnTabOut_Touch);
			m_coinsTab.SetReceiveReleaseWithoutMouseDown(isTouch);
			m_allTabs.Add(m_coinsTab);
			m_tabVisibility[m_coinsTab] = true;
			m_coinsTabPos = m_coinsTab.transform.localPosition;
		}
		PositionBookTabs(animate: false);
		m_initializedTabPositions = true;
	}

	protected override void PositionBookTabs(bool animate)
	{
		Vector3 visibleTabPosition = m_tabContainer.transform.position;
		int totalTabCount = CLASS_TAB_ORDER.Length;
		if (m_deckTemplateTab != null && m_deckTemplateTab.gameObject.activeSelf)
		{
			totalTabCount++;
		}
		for (int i = 0; i < totalTabCount; i++)
		{
			CollectionClassTab tab = m_classTabs[i];
			Vector3 tabLocalPos;
			if (ShouldShowTab(tab))
			{
				tab.SetTargetVisibility(visible: true);
				visibleTabPosition.x += m_spaceBetweenTabs;
				visibleTabPosition.x += m_deselectedTabHalfWidth;
				tabLocalPos = m_tabContainer.transform.InverseTransformPoint(visibleTabPosition);
				if (tab == m_currentTab)
				{
					tabLocalPos.y = tab.m_SelectedLocalYPos;
					tabLocalPos.z = tab.GetOriginalLocalPosition().z;
				}
				visibleTabPosition.x += m_deselectedTabHalfWidth;
			}
			else
			{
				tab.SetTargetVisibility(visible: false);
				tabLocalPos = tab.transform.localPosition;
				tabLocalPos.z = CollectiblePageManager.HIDDEN_TAB_LOCAL_Z_POS;
			}
			if (animate)
			{
				tab.SetTargetLocalPosition(tabLocalPos);
				continue;
			}
			tab.SetIsVisible(tab.ShouldBeVisible());
			tab.transform.localPosition = tabLocalPos;
		}
		bool showHeroSkins = ShouldShowTab(m_heroSkinsTab);
		PositionFixedTab(showHeroSkins, m_heroSkinsTab, m_heroSkinsTabPos, animate);
		bool showCardBacks = ShouldShowTab(m_cardBacksTab);
		PositionFixedTab(showCardBacks, m_cardBacksTab, m_cardBacksTabPos, animate);
		bool showCoins = ShouldShowTab(m_coinsTab);
		PositionFixedTab(showCoins, m_coinsTab, m_coinsTabPos, animate);
		bool showZilliaxTabs = m_initializedTabPositions && m_zilliaxTabs.Count > 0 && ShouldShowTab(m_zilliaxTabs[0]);
		Vector3 visibleZilliaxTabPosition = m_tabContainer.transform.position;
		foreach (BookTab zilliaxTab in m_zilliaxTabs)
		{
			Vector3 zilliaxTabLocalPos;
			if (showZilliaxTabs)
			{
				zilliaxTab.SetTargetVisibility(visible: true);
				visibleZilliaxTabPosition.x += m_spaceBetweenTabs;
				visibleZilliaxTabPosition.x += m_deselectedTabHalfWidth;
				zilliaxTabLocalPos = m_tabContainer.transform.InverseTransformPoint(visibleZilliaxTabPosition);
				if (zilliaxTab == m_currentTab)
				{
					zilliaxTabLocalPos.y = zilliaxTab.m_SelectedLocalYPos;
					zilliaxTabLocalPos.z = zilliaxTab.GetOriginalLocalPosition().z;
				}
				visibleZilliaxTabPosition.x += m_deselectedTabHalfWidth;
			}
			else
			{
				zilliaxTab.SetTargetVisibility(visible: false);
				zilliaxTabLocalPos = zilliaxTab.transform.localPosition;
				zilliaxTabLocalPos.z = CollectiblePageManager.HIDDEN_TAB_LOCAL_Z_POS;
			}
			if (animate)
			{
				zilliaxTab.SetTargetLocalPosition(zilliaxTabLocalPos);
				continue;
			}
			zilliaxTab.SetIsVisible(zilliaxTab.ShouldBeVisible());
			zilliaxTab.transform.localPosition = zilliaxTabLocalPos;
		}
		if (animate)
		{
			StopCoroutine(ANIMATE_TABS_COROUTINE_NAME);
			StartCoroutine(ANIMATE_TABS_COROUTINE_NAME);
		}
	}

	private IEnumerator AnimateTabs()
	{
		bool playSounds = HeroPickerDisplay.Get() == null || !HeroPickerDisplay.Get().IsShown();
		List<BookTab> tabsToHide = new List<BookTab>();
		List<BookTab> tabsToShow = new List<BookTab>();
		List<BookTab> tabsToMove = new List<BookTab>();
		foreach (CollectionClassTab tab in m_classTabs)
		{
			if (tab.IsVisible() || tab.ShouldBeVisible())
			{
				if (tab.IsVisible() && tab.ShouldBeVisible())
				{
					tabsToMove.Add(tab);
				}
				else if (tab.IsVisible() && !tab.ShouldBeVisible())
				{
					tabsToHide.Add(tab);
				}
				else
				{
					tabsToShow.Add(tab);
				}
			}
		}
		foreach (BookTab tab2 in m_zilliaxTabs)
		{
			if (tab2.IsVisible() || tab2.ShouldBeVisible())
			{
				if (tab2.IsVisible() && tab2.ShouldBeVisible())
				{
					tabsToMove.Add(tab2);
				}
				else if (tab2.IsVisible() && !tab2.ShouldBeVisible())
				{
					tabsToHide.Add(tab2);
				}
				else
				{
					tabsToShow.Add(tab2);
				}
			}
		}
		m_tabsAreAnimating = true;
		if (tabsToHide.Count > 0)
		{
			foreach (BookTab tab3 in tabsToHide)
			{
				if (playSounds)
				{
					SoundManager.Get().LoadAndPlay("class_tab_retract.prefab:da79957be76b10343999d6fa92a6a2f0", tab3.gameObject);
				}
				yield return new WaitForSeconds(0.03f);
				tab3.AnimateToTargetPosition(0.1f, iTween.EaseType.easeOutQuad);
			}
			yield return new WaitForSeconds(0.1f);
		}
		if (tabsToMove.Count > 0)
		{
			foreach (BookTab tab4 in tabsToMove)
			{
				if (tab4.WillSlide() && playSounds)
				{
					SoundManager.Get().LoadAndPlay("class_tab_slides_across_top.prefab:04482bc6f531b76468ff92a5b4e979b6", tab4.gameObject);
				}
				tab4.AnimateToTargetPosition(0.25f, iTween.EaseType.easeOutQuad);
			}
			yield return new WaitForSeconds(0.25f);
		}
		if (tabsToShow.Count > 0)
		{
			foreach (BookTab tab5 in tabsToShow)
			{
				if (playSounds)
				{
					SoundManager.Get().LoadAndPlay("class_tab_retract.prefab:da79957be76b10343999d6fa92a6a2f0", tab5.gameObject);
				}
				tab5.AnimateToTargetPosition(0.4f, iTween.EaseType.easeOutBounce);
			}
			yield return new WaitForSeconds(0.4f);
		}
		foreach (CollectionClassTab classTab in m_classTabs)
		{
			classTab.SetIsVisible(classTab.ShouldBeVisible());
		}
		foreach (BookTab zilliaxTab in m_zilliaxTabs)
		{
			zilliaxTab.SetIsVisible(zilliaxTab.ShouldBeVisible());
		}
		m_tabsAreAnimating = false;
	}

	public void SetClassTabShouldShowPersistentGlow(TAG_CLASS tabClass, bool shouldShowGlow)
	{
		foreach (CollectionClassTab collectionClassTab in m_classTabs)
		{
			if (collectionClassTab.TabInfo.tagClass == tabClass)
			{
				collectionClassTab.ShouldShowPersistentClassGlow = shouldShowGlow;
				break;
			}
		}
		m_shouldClassHavePersistentGlow[tabClass] = shouldShowGlow;
	}

	public bool ShouldClassTabShowPersistentGlow(TAG_CLASS tabClass)
	{
		bool shouldShow = false;
		if (!m_shouldClassHavePersistentGlow.TryGetValue(tabClass, out shouldShow))
		{
			return false;
		}
		return shouldShow;
	}

	public void PlaySpawnVFXOnClassTab(TAG_CLASS tabClass)
	{
		foreach (CollectionClassTab collectionClassTab in m_classTabs)
		{
			if (collectionClassTab.TabInfo.tagClass == tabClass)
			{
				collectionClassTab.PlaySpawnVFX();
				break;
			}
		}
		if (m_classFilterHeader != null)
		{
			m_classFilterHeader.PlaySparkleVFX();
		}
	}

	private void RemoveGhostingEffectForRuneCards()
	{
		List<CollectionCardVisual> list = new List<CollectionCardVisual>(m_ghostedRuneCards);
		m_ghostedRuneCards.Clear();
		foreach (CollectionCardVisual collectionCardVisual in list)
		{
			if (!ShouldCollectionCardVisualHaveGhostingEffect(collectionCardVisual))
			{
				Actor actor = collectionCardVisual.GetActor();
				if (actor != null)
				{
					actor.GhostCardEffect(GhostCard.Type.NONE, actor.GetPremium());
				}
			}
		}
	}

	public void AddGhostedRuneCards(List<CollectionCardVisual> runeCards)
	{
		foreach (CollectionCardVisual card in runeCards)
		{
			if (!m_ghostedRuneCards.Contains(card))
			{
				m_ghostedRuneCards.Add(card);
			}
		}
	}

	private void OnDeckTrayRunesAdded(CollectionDeck deck, RunePattern cardRunesAdded)
	{
		m_deckRunesWereUpdatedOnCurrentPage = !deck.Runes.Matches(m_originalDeckRunesForCurrentPage);
		RemoveGhostingEffectForRuneCards();
		UpdatePageGhostingForInvalidRunes(deck.Runes);
	}

	public void RemoveGhostingEffectForTouristCards()
	{
		List<CollectionCardVisual> list = new List<CollectionCardVisual>(m_ghostedTouristCards);
		m_ghostedTouristCards.Clear();
		foreach (CollectionCardVisual collectionCardVisual in list)
		{
			if (!ShouldCollectionCardVisualHaveGhostingEffect(collectionCardVisual))
			{
				Actor actor = collectionCardVisual.GetActor();
				if (actor != null)
				{
					actor.GhostCardEffect(GhostCard.Type.NONE, actor.GetPremium());
				}
			}
		}
	}

	public void AddGhostedTouristCards(List<CollectionCardVisual> touristCards)
	{
		foreach (CollectionCardVisual card in touristCards)
		{
			if (!m_ghostedTouristCards.Contains(card))
			{
				m_ghostedTouristCards.Add(card);
			}
		}
	}

	private bool ShouldCollectionCardVisualHaveGhostingEffect(CollectionCardVisual cardVisual)
	{
		if (!m_ghostedRuneCards.Contains(cardVisual))
		{
			return m_ghostedTouristCards.Contains(cardVisual);
		}
		return true;
	}

	private void OnCraftingTrayShown()
	{
		RemoveGhostingEffectForRuneCards();
	}

	private void OnCraftingTrayHidden()
	{
		CollectionDeck deck = CollectionManager.Get().GetEditedDeck();
		if (deck != null && deck.HasClass(TAG_CLASS.DEATHKNIGHT))
		{
			UpdatePageGhostingForInvalidRunes(deck.Runes);
		}
	}

	private void OnHideLockedRunesCheckboxToggled(bool isChecked)
	{
		IsShowingLockedRuneCards = isChecked;
		if (IsShowingLockedRuneCards)
		{
			if (m_deckRunesWereUpdatedOnCurrentPage || m_classCardsCollection.HasHiddenDeathKnightCards)
			{
				m_classCardsCollection.UpdateResults();
				FlipToPage(1, null, null, PageTransitionType.MANY_PAGE_LEFT);
			}
		}
		else
		{
			FlipToNextFilteredDeathKnightPage(PageTransitionType.NONE);
		}
	}

	private void UpdatePageGhostingForInvalidRunes(RunePattern runes)
	{
		CollectiblePageDisplay cpd = GetCurrentCollectiblePage();
		if (!(cpd == null))
		{
			AddGhostedRuneCards(cpd.ApplyRuneCardGhostEffectsForCurrentPage(runes));
		}
	}

	private void RuneIndicatorVisualOnRunePatternChanged(RunePattern currentDeckRunes)
	{
		m_deckRunesWereUpdatedOnCurrentPage = !currentDeckRunes.Matches(m_originalDeckRunesForCurrentPage);
		RemoveGhostingEffectForRuneCards();
		UpdatePageGhostingForInvalidRunes(currentDeckRunes);
	}

	public void UpdatePageGhostingForInvalidTourists(CollectionDeck deck)
	{
		CollectiblePageDisplay cpd = GetCurrentCollectiblePage();
		if (!(cpd == null))
		{
			AddGhostedTouristCards(cpd.ApplyTouristCardGhostEffectsForCurrentPage(deck));
		}
	}

	private void SetCurrentClassTabInfo(CollectionTabInfo tabInfo)
	{
		CollectibleDisplay collectibleDisplay = CollectionManager.Get().GetCollectibleDisplay();
		CollectionUtils.ViewMode currentMode = collectibleDisplay.GetViewMode();
		if (m_currentClass != tabInfo.tagClass)
		{
			this.OnCurrentClassChanged?.Invoke(tabInfo.tagClass);
			m_currentClass = tabInfo.tagClass;
		}
		if ((bool)UniversalInputManager.UsePhoneUI)
		{
			if (m_classFilterHeader == null)
			{
				Debug.LogError("CollectionPageManager:SetCurrentClassTab: m_classFilterHeader should not be null when UniversalInputManager.UsePhoneUI is true");
				return;
			}
			if (!ShouldClassFilterBeVisible())
			{
				m_classFilterHeader.gameObject.SetActive(value: false);
				return;
			}
			m_classFilterHeader.gameObject.SetActive(value: true);
			m_classFilterHeader.SetMode(currentMode, tabInfo.tagClass, tabInfo.stringOverride);
			return;
		}
		BookTab selectNewTab = null;
		switch (currentMode)
		{
		case CollectionUtils.ViewMode.CARDS:
			if (collectibleDisplay.GetViewSubmode() != CollectionUtils.ViewSubmode.CARD_ZILLIAX_MODULES)
			{
				if (tabInfo.tagClass != 0)
				{
					selectNewTab = m_classTabs.Find((CollectionClassTab obj) => obj.TabInfo.tagClass == tabInfo.tagClass && obj.m_tabViewMode != CollectionUtils.ViewMode.DECK_TEMPLATE);
				}
			}
			else
			{
				selectNewTab = ((m_currentPageNum == 1) ? m_ZilliaxModulesTab : m_ZilliaxBlueprintsTab);
			}
			break;
		case CollectionUtils.ViewMode.HERO_SKINS:
		case CollectionUtils.ViewMode.HERO_PICKER:
		{
			bool pickerActive = currentMode == CollectionUtils.ViewMode.HERO_PICKER;
			bool isFiltered = IsSearching() || CollectionManager.Get().GetEditedDeck() != null;
			if (pickerActive || (isFiltered && !pickerActive))
			{
				selectNewTab = m_heroSkinsTab;
			}
			break;
		}
		case CollectionUtils.ViewMode.CARD_BACKS:
			selectNewTab = m_cardBacksTab;
			break;
		case CollectionUtils.ViewMode.COINS:
			selectNewTab = m_coinsTab;
			break;
		default:
			selectNewTab = null;
			break;
		}
		if (!(selectNewTab == m_currentTab))
		{
			DeselectCurrentTab();
			_ = m_currentTab;
			m_currentTab = selectNewTab;
			if (m_currentTab != null)
			{
				StopCoroutine(CollectiblePageManager.SELECT_TAB_COROUTINE_NAME);
				StartCoroutine(CollectiblePageManager.SELECT_TAB_COROUTINE_NAME, m_currentTab);
			}
		}
	}

	public void SetDeckRuleset(DeckRuleset deckRuleset, bool refresh = false)
	{
		m_cardsCollection.SetDeckRuleset(deckRuleset);
		if (refresh)
		{
			UpdateFilteredCards();
			TransitionPageWhenReady(PageTransitionType.NONE, useCurrentPageNum: false, null, null);
		}
	}

	private void OnClassTabPressed(UIEvent e)
	{
		if (CanUserTurnPages())
		{
			CollectionClassTab classTab = e.GetElement() as CollectionClassTab;
			if (!(classTab == null) && !(classTab == m_currentTab))
			{
				JumpToCollectionClassPage(classTab.TabInfo);
			}
		}
	}

	protected override void OnTabOver(UIEvent e)
	{
		base.OnTabOver(e);
		CollectionClassTab classTab = e.GetElement() as CollectionClassTab;
		if (!(classTab == null))
		{
			this.OnCollectionClassTabHovered?.Invoke(classTab.TabInfo.tagClass);
		}
	}

	private void OnZilliaxFunctionalTabPressed(UIEvent e)
	{
		if (CanUserTurnPages() && (CollectionManager.Get().GetCollectibleDisplay().GetViewSubmode() != CollectionUtils.ViewSubmode.CARD_ZILLIAX_MODULES || base.CurrentPageNum != 1))
		{
			FlipToPage(1, null, null);
			this.OnZilliaxTabPressed?.Invoke();
		}
	}

	private void OnZilliaxSavedVersionsTabPressed(UIEvent e)
	{
		if (CanUserTurnPages() && (CollectionManager.Get().GetCollectibleDisplay().GetViewSubmode() != CollectionUtils.ViewSubmode.CARD_ZILLIAX_MODULES || base.CurrentPageNum != 2))
		{
			FlipToPage(2, null, null);
			this.OnZilliaxTabPressed?.Invoke();
		}
	}

	private void OnDeckTemplateTabPressed(UIEvent e)
	{
		if (CanUserTurnPages())
		{
			CollectionManager.Get().GetCollectibleDisplay().SetViewMode(CollectionUtils.ViewMode.DECK_TEMPLATE);
		}
	}

	private void OnHeroSkinsTabPressed(UIEvent e)
	{
		if (!CanUserTurnPages())
		{
			return;
		}
		CollectionClassTab classTab = e.GetElement() as CollectionClassTab;
		if (!(classTab == null) && !(classTab == m_currentTab) && ShouldShowTab(m_heroSkinsTab))
		{
			CollectionPageDisplay collectionPageDisplay = GetCurrentCollectiblePage() as CollectionPageDisplay;
			if (collectionPageDisplay != null)
			{
				_ = collectionPageDisplay.m_pageFormatType;
			}
			if (IsSearching() || CollectionManager.Get().GetEditedDeck() != null)
			{
				OnHeroClassButtonPressed(e);
			}
			else
			{
				CollectionManager.Get().GetCollectibleDisplay().SetViewMode(CollectionUtils.ViewMode.HERO_PICKER);
			}
		}
	}

	private void OnHeroClassButtonPressed(UIEvent e)
	{
		CollectionManager.Get().GetCollectibleDisplay().SetViewMode(CollectionUtils.ViewMode.HERO_SKINS);
	}

	private void OnCardBacksTabPressed(UIEvent e)
	{
		if (CanUserTurnPages())
		{
			CollectionClassTab classTab = e.GetElement() as CollectionClassTab;
			if (!(classTab == null) && !(classTab == m_currentTab))
			{
				CollectionManager.Get().GetCollectibleDisplay().SetViewMode(CollectionUtils.ViewMode.CARD_BACKS);
			}
		}
	}

	private void OnCoinsTabPressed(UIEvent e)
	{
		if (CanUserTurnPages())
		{
			CollectionClassTab classTab = e.GetElement() as CollectionClassTab;
			if (!(classTab == null) && !(classTab == m_currentTab) && ShouldShowTab(m_coinsTab))
			{
				CollectionManager.Get().GetCollectibleDisplay().SetViewMode(CollectionUtils.ViewMode.COINS);
			}
		}
	}

	public void UpdateMassDisenchant()
	{
		CraftingTray.Get()?.SetMassDisenchantAmount();
		CollectionManager collectionManager = CollectionManager.Get();
		if (collectionManager != null)
		{
			int disenchantCount = 0;
			collectionManager.GetMassDisenchantCardsAndCount(m_disenchantCards, out disenchantCount);
			UpdateCraftingModeButtonDustBottleVisibility(disenchantCount);
			MassDisenchant massDisenchant = MassDisenchant.Get();
			if (massDisenchant != null)
			{
				massDisenchant.UpdateContents(m_disenchantCards);
			}
		}
	}

	public void JumpToCollectionClassPage(CollectionTabInfo pageTabInfo)
	{
		JumpToCollectionClassPage(pageTabInfo, null, null);
	}

	public void JumpToCollectionClassPage(TAG_CLASS pageClass, DelOnPageTransitionComplete callback = null, object callbackData = null)
	{
		JumpToCollectionClassPage(new CollectionTabInfo
		{
			tagClass = pageClass
		}, callback, callbackData);
	}

	public void JumpToCollectionClassPage(CollectionTabInfo pageTabInfo, DelOnPageTransitionComplete callback, object callbackData)
	{
		CollectibleDisplay cd = CollectionManager.Get().GetCollectibleDisplay();
		if (cd != null && cd.GetViewMode() != 0)
		{
			cd.SetViewMode(CollectionUtils.ViewMode.CARDS, new CollectionUtils.ViewModeData
			{
				m_setPageByClass = pageTabInfo.tagClass
			});
			return;
		}
		CollectionDeck deck = CollectionManager.Get().GetEditedDeck();
		if (deck != null && deck.HasClass(TAG_CLASS.DEATHKNIGHT))
		{
			m_classCardsCollection.UpdateResults();
			m_deckRunesWereUpdatedOnCurrentPage = false;
		}
		m_classCardsCollection.GetPageContentsForTab(pageTabInfo, 1, calculateCollectionPage: true, out var newCollectionPage);
		FlipToPage(newCollectionPage, callback, callbackData);
	}

	protected override void AssembleEmptyPageUI(BookPageDisplay page)
	{
		base.AssembleEmptyPageUI(page);
		AssembleEmptyPageUI(page as CollectiblePageDisplay, displayNoMatchesText: false);
	}

	protected override void AssembleEmptyPageUI(CollectiblePageDisplay page, bool displayNoMatchesText)
	{
		CollectionPageDisplay cpd = PageAsCollectionPage(page);
		if (cpd == null)
		{
			Log.CollectionManager.PrintError("Page in CollectionPageManager is not a CollectionPageDisplay!  This should not happen!");
			return;
		}
		cpd.SetClass(default(CollectionTabInfo));
		bool showNoMatchHints = CollectionManager.Get().GetCollectibleDisplay().GetViewMode() == CollectionUtils.ViewMode.CARDS;
		cpd.ShowNoMatchesFound(displayNoMatchesText, m_classCardsCollection.FindCardsResult, showNoMatchHints);
		if (CollectionManager.Get().GetCollectibleDisplay().GetViewMode() == CollectionUtils.ViewMode.CARDS)
		{
			DeselectCurrentTab();
		}
		cpd.SetPageCountText(GameStrings.Get("GLUE_COLLECTION_EMPTY_PAGE"));
		cpd.SetPageTextColor();
	}

	private void AssembleMassDisenchantPage(TransitionReadyCallbackData transitionReadyCallbackData, FormatType formatType)
	{
		CollectionPageDisplay page = PageAsCollectionPage(transitionReadyCallbackData.m_assembledPage);
		page.ActivatePageCountText(active: false);
		page.SetPageType(formatType);
		AssembleEmptyPageUI(page, displayNoMatchesText: false);
		SetHasPreviousAndNextPages(hasPreviousPage: false, hasNextPage: false);
		page.SetMassDisenchant();
		CollectionManager.Get().GetCollectibleDisplay().CollectionPageContentsChanged<ICollectible>(null, delegate
		{
			page.UpdatePageWithMassDisenchant();
			TransitionPage(transitionReadyCallbackData);
		}, null);
	}

	private List<CollectibleCard> GetFilteredDeathKnightCards<TCollectible>(ICollection<TCollectible> collectiblesToDisplay)
	{
		if (!(collectiblesToDisplay is List<CollectibleCard> collectibleCards))
		{
			return null;
		}
		if (collectibleCards.Count == 0)
		{
			return collectibleCards;
		}
		if (collectibleCards[0].GetEntityDef().GetClass() != TAG_CLASS.DEATHKNIGHT)
		{
			return null;
		}
		CollectionDeck deck = CollectionManager.Get().GetEditedDeck();
		if (deck == null)
		{
			return null;
		}
		List<CollectibleCard> result = new List<CollectibleCard>();
		RunePattern runePattern = default(RunePattern);
		foreach (CollectibleCard card in collectibleCards)
		{
			runePattern.SetCostsFromEntity(card.GetEntityDef());
			if (deck.CanAddRunes(runePattern, DeckRule_DeathKnightRuneLimit.MaxRuneSlots))
			{
				result.Add(card);
			}
		}
		return result;
	}

	protected override bool AssembleCollectiblePage<TCollectible>(TransitionReadyCallbackData transitionReadyCallbackData, ICollection<TCollectible> collectiblesToDisplay, int totalNumPages)
	{
		CollectionUtils.ViewMode viewMode = CollectionManager.Get().GetCollectibleDisplay().GetViewMode();
		CollectionDeck deck = CollectionManager.Get().GetEditedDeck();
		if (m_currentClassContext.tagClass == TAG_CLASS.DEATHKNIGHT && deck != null)
		{
			m_originalDeckRunesForCurrentPage = deck.Runes;
		}
		CollectionTabInfo newClassContext = m_classCardsCollection.GetCurrentTabInfoFromPage(m_currentPageNum);
		bool isEmptyTouristClassPage = deck != null && deck.GetTouristClasses().Contains(newClassContext.tagClass) && collectiblesToDisplay.Count == 0;
		if (isEmptyTouristClassPage)
		{
			isEmptyTouristClassPage = m_classCardsCollection.GetDoesAnyCardExistIgnoreOwnership(newClassContext.tagClass);
		}
		bool wasPageConstructed = false;
		if (isEmptyTouristClassPage)
		{
			FormatType formatType = CollectionManager.Get().GetThemeShowing();
			if (!AssembleCollectionBasePage(transitionReadyCallbackData, emptyPage: false, formatType))
			{
				wasPageConstructed = true;
			}
		}
		if (!wasPageConstructed && base.AssembleCollectiblePage(transitionReadyCallbackData, collectiblesToDisplay, totalNumPages))
		{
			CollectionPageDisplay pageToHideTouristStamp = PageAsCollectionPage(transitionReadyCallbackData.m_assembledPage);
			if (pageToHideTouristStamp != null)
			{
				pageToHideTouristStamp.HideTouristStamp();
			}
			if (!CollectionManager.Get().IsInEditMode() || viewMode != 0)
			{
				return true;
			}
			if (deck == null)
			{
				return true;
			}
			List<TAG_CLASS> deckClasses = deck.GetClasses();
			if (deckClasses.Count <= 0)
			{
				return true;
			}
			TAG_CLASS deckClass = deckClasses[0];
			m_currentClassContext = new CollectionTabInfo
			{
				tagClass = deckClass
			};
			SetCurrentClassTabInfo(m_currentClassContext);
			return true;
		}
		CollectionPageDisplay page = PageAsCollectionPage(transitionReadyCallbackData.m_assembledPage);
		m_lastCollectibleAnchor = collectiblesToDisplay.FirstOrDefault();
		if (viewMode == CollectionUtils.ViewMode.HERO_SKINS)
		{
			CollectionDeck editedDeck = CollectionManager.Get().GetEditedDeck();
			if (editedDeck != null)
			{
				page.SetHeroSkins(editedDeck.GetClass());
			}
			else
			{
				CollectionManagerDisplay cmd = CollectionManager.Get().GetCollectibleDisplay() as CollectionManagerDisplay;
				TAG_CLASS? heroSkinClass = ((cmd != null) ? cmd.GetHeroSkinClass() : ((TAG_CLASS?)null));
				page.SetHeroSkins(heroSkinClass);
			}
		}
		else if (viewMode == CollectionUtils.ViewMode.COINS)
		{
			page.SetCoins();
		}
		else if (CollectionManager.Get().GetCollectibleDisplay().GetViewSubmode() != CollectionUtils.ViewSubmode.CARD_ZILLIAX_MODULES)
		{
			CollectionTabInfo currentTabInfo = m_classCardsCollection.GetCurrentTabInfoFromPage(m_currentPageNum);
			page.SetClass(currentTabInfo);
			m_currentClassContext = currentTabInfo;
		}
		m_deckRunesWereUpdatedOnCurrentPage = false;
		page.SetPageCountText(GameStrings.Format("GLUE_COLLECTION_PAGE_NUM", m_currentPageNum));
		page.SetPageTextColor();
		page.ShowNoMatchesFound(collectiblesToDisplay.Count == 0, m_classCardsCollection.FindCardsResult);
		SetHasPreviousAndNextPages(m_currentPageNum > 1, m_currentPageNum < totalNumPages);
		CollectionManager.Get().GetCollectibleDisplay().CollectionPageContentsChanged(collectiblesToDisplay, delegate(List<CollectionCardActors> actorList, List<ICollectible> nonActorCollectibleList, object data)
		{
			page.UpdateCollectionItems(actorList, nonActorCollectibleList, viewMode);
			TransitionPageNextFrame(transitionReadyCallbackData);
			if (m_deckTemplatePicker != null)
			{
				StartCoroutine(m_deckTemplatePicker.Show(show: false));
			}
		}, null);
		return true;
	}

	private void AssembleDeckTemplatePage(TransitionReadyCallbackData transitionReadyCallbackData)
	{
		FormatType pageFormatType = ((m_deckTemplatePicker != null && m_deckTemplatePicker.CurrentSelectedFormat != 0) ? m_deckTemplatePicker.CurrentSelectedFormat : FormatType.FT_STANDARD);
		if (AssembleCollectionBasePage(transitionReadyCallbackData, emptyPage: false, pageFormatType))
		{
			return;
		}
		CollectionPageDisplay page = PageAsCollectionPage(transitionReadyCallbackData.m_assembledPage);
		if (m_deckTemplatePicker == null && !string.IsNullOrEmpty(m_deckTemplatePickerPrefab))
		{
			m_deckTemplatePicker = GameUtils.LoadGameObjectWithComponent<DeckTemplatePicker>(m_deckTemplatePickerPrefab);
			if (m_deckTemplatePicker == null)
			{
				Debug.LogWarning("Failed to instantiate deck template picker prefab " + m_deckTemplatePickerPrefab);
				return;
			}
			m_deckTemplatePicker.RegisterOnTemplateDeckChosen(delegate
			{
				HideNonDeckTemplateTabs(hide: false, updateTabs: true);
				CollectionManager.Get().GetCollectibleDisplay().SetViewMode(CollectionUtils.ViewMode.CARDS);
			});
		}
		page.UpdateDeckTemplateHeader(m_deckTemplatePicker?.m_pageHeader, pageFormatType);
		page.UpdateDeckTemplatePage(m_deckTemplatePicker);
		page.SetDeckTemplates();
		page.ShowNoMatchesFound(show: false);
		page.SetPageCountText(string.Empty);
		SetHasPreviousAndNextPages(hasPreviousPage: false, hasNextPage: false);
		UpdateDeckTemplate(m_deckTemplatePicker);
		TransitionPage(transitionReadyCallbackData);
	}

	public DeckTemplatePicker GetDeckTemplatePicker()
	{
		return m_deckTemplatePicker;
	}

	public void UpdateDeckTemplate(DeckTemplatePicker deckTemplatePicker)
	{
		if (deckTemplatePicker != null)
		{
			CollectionDeck editingDeck = CollectionManager.Get().GetEditedDeck();
			if (editingDeck != null)
			{
				deckTemplatePicker.SetDeckFormatAndClass(editingDeck.FormatType, editingDeck.GetClass());
			}
			StartCoroutine(deckTemplatePicker.Show(show: true));
		}
	}

	private void AssembleCardBackPage(TransitionReadyCallbackData transitionReadyCallbackData, bool useCurrentPageNum)
	{
		Log.CollectionManager.Print("transitionPageId={0} pagesTurning={1} currentPageIsPageA={2} currentPageNum={3}", m_transitionPageId, m_pagesCurrentlyTurning, m_currentPageIsPageA, m_currentPageNum);
		int cardBackCount = GetCurrentDeckTrayModeCardBackIds().Count;
		bool emptyPage = cardBackCount == 0;
		if (AssembleCollectionBasePage(transitionReadyCallbackData, emptyPage, FormatType.FT_STANDARD))
		{
			return;
		}
		CollectionPageDisplay page = PageAsCollectionPage(transitionReadyCallbackData.m_assembledPage);
		int cardsPerPage = CollectiblePageDisplay.GetMaxCardsPerPage();
		int numCardBackPages = cardBackCount / cardsPerPage + ((cardBackCount % cardsPerPage > 0) ? 1 : 0);
		m_currentPageNum = Mathf.Clamp(m_currentPageNum, 1, numCardBackPages);
		page.SetCardBacks();
		page.ShowNoMatchesFound(cardBackCount == 0);
		page.SetPageCountText(GameStrings.Format("GLUE_COLLECTION_PAGE_NUM", m_currentPageNum));
		SetHasPreviousAndNextPages(m_currentPageNum > 1, m_currentPageNum < numCardBackPages);
		bool showAll = !CollectionManager.Get().IsInEditMode();
		List<CardBackManager.OwnedCardBack> cardBacksToDisplay = CardBackManager.Get()?.GetPageOfCardBacks(!showAll, m_currentPageNum);
		CollectionManagerDisplay cmd = CollectionManager.Get().GetCollectibleDisplay() as CollectionManagerDisplay;
		if (!(cmd != null))
		{
			return;
		}
		cmd.CollectionPageContentsChangedToCardBacks(cardBacksToDisplay, delegate(List<CollectionCardActors> actorList, List<ICollectible> nonActorCollectibleList, object data)
		{
			page.UpdateCollectionItems(actorList, nonActorCollectibleList, CollectionUtils.ViewMode.CARD_BACKS);
			foreach (CollectionCardActors current in actorList)
			{
				CardBackManager.Get().UpdateCardBackWithInternalCardBack(current.GetPreferredActor());
			}
			TransitionPage(transitionReadyCallbackData);
			if (m_deckTemplatePicker != null)
			{
				StartCoroutine(m_deckTemplatePicker.Show(show: false));
			}
		});
	}

	protected void AssembleHeroPickerPage(TransitionReadyCallbackData transitionReadyCallbackData)
	{
		CollectionPageDisplay page = PageAsCollectionPage(transitionReadyCallbackData.m_assembledPage);
		page.ActivatePageCountText(active: false);
		page.SetPageType(FormatType.FT_STANDARD);
		AssembleEmptyPageUI(page, displayNoMatchesText: false);
		MassDisenchant massDisenchant = MassDisenchant.Get();
		if (massDisenchant != null)
		{
			massDisenchant.Hide();
		}
		SetHasPreviousAndNextPages(hasPreviousPage: false, hasNextPage: false);
		page.SetHeroPicker();
		page.SetPageTextColor();
		CollectionManagerDisplay cmd = CollectionManager.Get().GetCollectibleDisplay() as CollectionManagerDisplay;
		if (!(cmd == null))
		{
			m_heroesCollection.SortResults();
			m_heroesCollection.FilterHeroesByActiveClass();
			cmd.SetHeroSkinClass(null);
			cmd.CollectionPageContentsChanged<ICollectible>(null, delegate
			{
				CountClassHeroTotals(out var allHeroCounts, out var ownedHeroCounts);
				page.UpdatePageWithHeroPicker(allHeroCounts, ownedHeroCounts);
				TransitionPage(transitionReadyCallbackData);
			}, null);
		}
	}

	protected void CountClassHeroTotals(out int[] allHeroCounts, out int[] ownedHeroCounts)
	{
		List<TAG_CLASS> validClasses = new List<TAG_CLASS>(GameUtils.ORDERED_HERO_CLASSES);
		allHeroCounts = new int[validClasses.Count];
		ownedHeroCounts = new int[validClasses.Count];
		List<CollectibleCard> rawHeroes = m_heroesCollection.GetAllResults();
		for (int i = 0; i < rawHeroes.Count; i++)
		{
			CollectibleCard heroCard = rawHeroes[i];
			TAG_CLASS heroClass = heroCard.Class;
			int indexOfClass = validClasses.IndexOf(heroClass);
			if (indexOfClass < 0 || indexOfClass >= validClasses.Count)
			{
				Log.CollectionManager.PrintError($"Hero count failed to increment from hero: {heroCard.Name} ({heroCard.CardDbId})) due to invalid class! " + $"(Expected: 0 to {validClasses.Count - 1}, Actual: {indexOfClass})");
				continue;
			}
			allHeroCounts[indexOfClass]++;
			if (heroCard.OwnedCount >= 1)
			{
				ownedHeroCounts[indexOfClass]++;
			}
		}
	}

	protected void AssembleHeroSkinsPage(TransitionReadyCallbackData transitionReadyCallbackData, bool useCurrentPageNum)
	{
		m_heroesCollection.FilterHeroesByActiveClass();
		List<CollectibleCard> heroesToDisplay = m_heroesCollection.GetHeroesContents(m_currentPageNum);
		heroesToDisplay = m_heroesCollection.GetHeroesContents(m_currentPageNum);
		AssembleCollectiblePage(transitionReadyCallbackData, heroesToDisplay, m_heroesCollection.GetTotalNumPages());
	}

	protected void AssembleCoinPage(TransitionReadyCallbackData transitionReadyCallbackData, bool useCurrentPageNum)
	{
		int coinsPerPage = CollectiblePageDisplay.GetMaxCardsPerPage();
		List<CollectibleCard> coinsToDisplay = CosmeticCoinManager.Get().GetPageOfCoinCards(m_currentPageNum, coinsPerPage);
		AssembleCollectiblePage(transitionReadyCallbackData, coinsToDisplay, CosmeticCoinManager.Get().GetCoinPageCount(coinsPerPage));
	}

	protected void AssembleZilliaxModulesPage(TransitionReadyCallbackData transitionReadyCallbackData, bool useCurrentPageNum)
	{
		int maxCardsPerPage = CollectiblePageDisplay.GetMaxCardsPerPage();
		int pageToDisplay = ((!useCurrentPageNum) ? 1 : m_currentPageNum);
		TAG_PREMIUM premiumToUse = CollectionManager.Get().GetEditedDeck().GetCurrentSideboardDeck()
			.DataModel.Premium;
		List<CollectibleCard> modulesToDisplay = new List<CollectibleCard>(maxCardsPerPage);
		if (pageToDisplay == 1)
		{
			GAME_TAG tagToUse = GAME_TAG.ZILLIAX_CUSTOMIZABLE_FUNCTIONALMODULE;
			foreach (CardDbfRecord moduleCardRecord in GameDbf.Card.GetRecords((CardDbfRecord record) => GameUtils.GetCardTagValue(record.ID, tagToUse) == 1))
			{
				EntityDef defToUse = DefLoader.Get().GetEntityDef(moduleCardRecord.NoteMiniGuid).Clone();
				modulesToDisplay.Add(new CollectibleCard(moduleCardRecord, defToUse, premiumToUse));
			}
			modulesToDisplay.Sort(OrderedZilliaxModulesSort);
		}
		else if (CollectionDeckTray.Get() != null && CollectionDeckTray.Get().GetSavedZilliaxVersions() != null)
		{
			modulesToDisplay = CollectionDeckTray.Get().GetSavedZilliaxVersions();
			modulesToDisplay.Sort(OrderedZilliaxSavedVersionsSort);
		}
		AssembleCollectiblePage(transitionReadyCallbackData, modulesToDisplay, 2);
		if (transitionReadyCallbackData?.m_assembledPage != null)
		{
			CollectionPageDisplay collectionPageDisplay = PageAsCollectionPage(transitionReadyCallbackData.m_assembledPage);
			CollectionTabInfo zilliaxTabInfo = (m_currentClassContext = new CollectionTabInfo
			{
				tagClass = TAG_CLASS.INVALID,
				stringOverride = GameStrings.Get((pageToDisplay == 1) ? "GLUE_TAB_NAME_ZILLIAX_MODULES" : "GLUE_TAB_NAME_ZILLIAX_SAVED_VERSIONS")
			});
			collectionPageDisplay.SetClass(zilliaxTabInfo, zilliaxTabInfo.stringOverride);
			SetCurrentClassTabInfo(zilliaxTabInfo);
		}
		EnablePageTurnClickableRegions(enable: false);
		EnablePageTurnArrows(enable: false);
	}

	protected override void AssemblePage(TransitionReadyCallbackData transitionReadyCallbackData, bool useCurrentPageNum)
	{
		CollectibleDisplay collectibleDisplay = CollectionManager.Get().GetCollectibleDisplay();
		if (null == collectibleDisplay)
		{
			return;
		}
		if (!useCurrentPageNum)
		{
			m_currentPageNum = 1;
		}
		switch (collectibleDisplay.GetViewMode())
		{
		case CollectionUtils.ViewMode.CARD_BACKS:
			AssembleCardBackPage(transitionReadyCallbackData, useCurrentPageNum);
			break;
		case CollectionUtils.ViewMode.DECK_TEMPLATE:
			AssembleDeckTemplatePage(transitionReadyCallbackData);
			break;
		case CollectionUtils.ViewMode.HERO_PICKER:
			AssembleHeroPickerPage(transitionReadyCallbackData);
			break;
		case CollectionUtils.ViewMode.HERO_SKINS:
			AssembleHeroSkinsPage(transitionReadyCallbackData, useCurrentPageNum);
			break;
		case CollectionUtils.ViewMode.MASS_DISENCHANT:
		{
			FormatType formatType = CollectionManager.Get().GetThemeShowing();
			AssembleMassDisenchantPage(transitionReadyCallbackData, formatType);
			break;
		}
		case CollectionUtils.ViewMode.COINS:
			AssembleCoinPage(transitionReadyCallbackData, useCurrentPageNum);
			break;
		case CollectionUtils.ViewMode.CARDS:
		{
			CollectionDeck deck = CollectionManager.Get().GetEditedDeck();
			if (collectibleDisplay.GetViewSubmode() == CollectionUtils.ViewSubmode.CARD_ZILLIAX_MODULES)
			{
				AssembleZilliaxModulesPage(transitionReadyCallbackData, useCurrentPageNum);
			}
			else
			{
				if (m_classCardsCollection == null)
				{
					break;
				}
				List<CollectibleCard> cardsToDisplay = new List<CollectibleCard>();
				if (useCurrentPageNum)
				{
					cardsToDisplay = m_classCardsCollection.GetPageContents(m_currentPageNum);
				}
				else if (!(m_lastCollectibleAnchor is CollectibleCard lastCardAnchor))
				{
					cardsToDisplay = m_classCardsCollection.GetPageContentsForTab(m_currentClassContext, 1, calculateCollectionPage: true, out m_currentPageNum);
					if (cardsToDisplay.Count == 0 && m_classCardsCollection.GetNumPagesForTab(m_currentClassContext) == 0)
					{
						m_currentPageNum = 1;
						cardsToDisplay = m_cardsCollection.GetPageContents(m_currentPageNum);
					}
				}
				else
				{
					cardsToDisplay = m_classCardsCollection.GetPageContentsForCard(lastCardAnchor.CardId, lastCardAnchor.PremiumType, out var currentPageNum, m_currentClassContext);
					if (cardsToDisplay.Count == 0)
					{
						cardsToDisplay = m_classCardsCollection.GetPageContentsForTab(m_currentClassContext, 1, calculateCollectionPage: true, out currentPageNum);
					}
					if (cardsToDisplay.Count == 0 && m_classCardsCollection.GetNumPagesForTab(m_currentClassContext) == 0)
					{
						cardsToDisplay = m_cardsCollection.GetPageContents(1);
						currentPageNum = 1;
						m_currentPageNum = ((cardsToDisplay.Count != 0) ? currentPageNum : 0);
					}
					else
					{
						m_currentPageNum = currentPageNum;
					}
				}
				if (cardsToDisplay.Count == 0)
				{
					CollectionTabInfo newClassContext = m_classCardsCollection.GetCurrentTabInfoFromPage(m_currentPageNum);
					if (deck == null || !deck.GetTouristClasses().Contains(newClassContext.tagClass))
					{
						cardsToDisplay = m_cardsCollection.GetFirstNonEmptyPage(out var tmpPageNum);
						if (cardsToDisplay.Count > 0)
						{
							m_currentPageNum = tmpPageNum;
						}
					}
				}
				AssembleCollectiblePage(transitionReadyCallbackData, cardsToDisplay, m_cardsCollection.GetTotalNumPages());
			}
			CollectionManagerDisplay cmd = collectibleDisplay as CollectionManagerDisplay;
			if (cmd != null)
			{
				bool shouldShowAllClassCards = false;
				if (deck != null)
				{
					shouldShowAllClassCards = ShouldShowAllClassCards(deck);
				}
				cmd.SetRuneLockedCheckboxVisible(CollectionManager.Get().IsEditingDeathKnightDeck() && !shouldShowAllClassCards);
			}
			if (!CollectionManager.Get().GetCollectibleDisplay().InCraftingMode() && deck != null)
			{
				if (deck.HasClass(TAG_CLASS.DEATHKNIGHT))
				{
					UpdatePageGhostingForInvalidRunes(deck.Runes);
				}
				UpdatePageGhostingForInvalidTourists(deck);
			}
			break;
		}
		}
	}

	protected override void UpdateFilteredCards()
	{
		base.UpdateFilteredCards();
		UpdateClassTabNewCardCounts();
	}

	protected override void TransitionPage(object callbackData)
	{
		base.TransitionPage(callbackData);
		if (CollectionManager.Get().GetCollectibleDisplay().GetViewMode() == CollectionUtils.ViewMode.MASS_DISENCHANT)
		{
			DeselectCurrentTab();
		}
		else
		{
			SetCurrentClassTabInfo(m_currentClassContext);
		}
	}

	protected override void PageRight(DelOnPageTransitionComplete callback, object callbackData)
	{
		if (CollectionManager.Get().GetCollectibleDisplay().GetViewSubmode() == CollectionUtils.ViewSubmode.CARD_ZILLIAX_MODULES)
		{
			return;
		}
		if (!IsEditingDeathKnightDeck(out var _))
		{
			base.PageRight(callback, callbackData);
			return;
		}
		if (IsShowingLockedRuneCards)
		{
			base.PageRight(callback, callbackData);
			return;
		}
		if (m_currentClassContext.tagClass == TAG_CLASS.DEATHKNIGHT)
		{
			if (m_deckRunesWereUpdatedOnCurrentPage)
			{
				FlipToNextFilteredDeathKnightPage(PageTransitionType.MANY_PAGE_RIGHT);
				return;
			}
		}
		else if (IsRightPageInDeathKnightTab())
		{
			m_classCardsCollection.UpdateResults();
			m_classCardsCollection.GetPageContentsForTab(new CollectionTabInfo
			{
				tagClass = TAG_CLASS.DEATHKNIGHT
			}, 1, calculateCollectionPage: true, out var rightPage);
			FlipToPage(rightPage, null, null, PageTransitionType.MANY_PAGE_RIGHT);
			return;
		}
		base.PageRight(callback, callbackData);
	}

	protected override void PageLeft(DelOnPageTransitionComplete callback, object callbackData)
	{
		if (CollectionManager.Get().GetCollectibleDisplay().GetViewSubmode() == CollectionUtils.ViewSubmode.CARD_ZILLIAX_MODULES)
		{
			return;
		}
		if (!IsEditingDeathKnightDeck(out var _))
		{
			base.PageLeft(callback, callbackData);
			return;
		}
		if (IsShowingLockedRuneCards)
		{
			base.PageLeft(callback, callbackData);
			return;
		}
		if (m_currentClassContext.tagClass == TAG_CLASS.DEATHKNIGHT)
		{
			if (m_deckRunesWereUpdatedOnCurrentPage)
			{
				FlipToNextFilteredDeathKnightPage(PageTransitionType.MANY_PAGE_LEFT);
				return;
			}
		}
		else if (IsLeftPageInDeathKnightTab())
		{
			m_classCardsCollection.UpdateResults();
			m_classCardsCollection.GetPageContentsForTab(m_currentClassContext, 1, calculateCollectionPage: true, out var page);
			int leftPage = page - 1;
			FlipToPage(leftPage, null, null, PageTransitionType.MANY_PAGE_LEFT);
			return;
		}
		base.PageLeft(callback, callbackData);
	}

	public void ShowRuneCardPopupForTutorial()
	{
		CollectibleCard firstRuneCard = m_classCardsCollection.GetFirstRuneCard();
		if (firstRuneCard == null)
		{
			Debug.LogWarning("CollectionPageManager.ShowRuneCardPopupForTutorial: There is no valid rune card.");
			return;
		}
		m_classCardsCollection.GetPageContentsForCard(firstRuneCard.CardId, firstRuneCard.PremiumType, out var page, new CollectionTabInfo
		{
			tagClass = TAG_CLASS.DEATHKNIGHT
		});
		if (page != m_currentPageNum)
		{
			FlipToPage(page, null, null);
		}
		CollectionCardVisual cardVisual = GetCurrentCollectiblePage().GetCardVisual(firstRuneCard.CardId, firstRuneCard.PremiumType);
		Vector3 runeBannerPosition = cardVisual.GetRuneBannerPosition();
		runeBannerPosition.x += (UniversalInputManager.UsePhoneUI ? 14f : 13f);
		NotificationManager notificationManager = NotificationManager.Get();
		m_deathKnightRuneTutorialRunePopup = notificationManager.CreatePopupText(UserAttentionBlocker.NONE, runeBannerPosition, 15f * Vector3.one, GameStrings.Get("GLOBAL_RUNE_REQUIREMENT_POPUP_TEXT"));
		m_deathKnightRuneTutorialRunePopup.ShowPopUpArrow(Notification.PopUpArrowDirection.Left);
		cardVisual.SetRuneBannerHighlighted(highlight: true);
	}

	public void ShowRuneIndicatorArrowForTutorial()
	{
		CollectionDeckTray collectionDeckTray = CollectionDeckTray.Get();
		Vector3 arrowPosition = collectionDeckTray.GetFirstRuneIndicatorButtonPosition();
		float arrowScale;
		if ((bool)UniversalInputManager.UsePhoneUI)
		{
			arrowPosition.x += -9f;
			arrowScale = 7f;
		}
		else
		{
			arrowPosition.x += -6f;
			arrowScale = 7f;
		}
		NotificationManager notificationManager = NotificationManager.Get();
		m_runeIndicatorArrow = notificationManager.CreateBouncingArrow(UserAttentionBlocker.NONE, arrowPosition, Vector3.down * 90f, addToList: false, arrowScale);
		collectionDeckTray.SetRuneIndicatorHighlighted(highlighted: true);
	}

	public void DismissRuneCardPopupForTutorial()
	{
		NotificationManager.Get().DestroyNotification(m_deathKnightRuneTutorialRunePopup, 0f);
		CollectibleCard firstRuneCard = m_classCardsCollection.GetFirstRuneCard();
		if (firstRuneCard == null)
		{
			Debug.LogWarning("CollectionPageManager.ShowRuneCardPopupForTutorial: There is no valid rune card.");
		}
		else
		{
			GetCurrentCollectiblePage().GetCardVisual(firstRuneCard.CardId, firstRuneCard.PremiumType).SetRuneBannerHighlighted(highlight: false);
		}
	}

	public void DismissRuneIndicatorArrowForTutorial()
	{
		NotificationManager.Get().DestroyNotification(m_runeIndicatorArrow, 0f);
		CollectionDeckTray.Get().SetRuneIndicatorHighlighted(highlighted: false);
	}

	private void FlipToNextFilteredDeathKnightPage(PageTransitionType transitionType)
	{
		if (m_lastCollectibleAnchor is CollectibleCard lastCardAnchor)
		{
			int page;
			List<CollectibleCard> cardsOnThisPage = m_classCardsCollection.GetPageContentsForCard(lastCardAnchor.CardId, lastCardAnchor.PremiumType, out page, m_currentClassContext);
			if (cardsOnThisPage.Count > 0)
			{
				m_classCardsCollection.UpdateResults();
				List<CollectibleCard> pageContentsForCard = m_classCardsCollection.GetPageContentsForCard(lastCardAnchor.CardId, lastCardAnchor.PremiumType, out page, m_currentClassContext);
				int nextPage = page;
				if (pageContentsForCard.Count > 0)
				{
					switch (transitionType)
					{
					case PageTransitionType.MANY_PAGE_RIGHT:
						nextPage++;
						break;
					case PageTransitionType.MANY_PAGE_LEFT:
						nextPage--;
						break;
					}
					FlipToPage(nextPage, null, null, transitionType);
					return;
				}
				CollectibleCard newCardAnchor = ((transitionType != PageTransitionType.MANY_PAGE_LEFT) ? m_classCardsCollection.GetNextValidDeathKnightCardRight(lastCardAnchor) : m_classCardsCollection.GetNextValidDeathKnightCardLeft(lastCardAnchor));
				if (newCardAnchor != null)
				{
					nextPage = m_classCardsCollection.GetPageNumberForCard(newCardAnchor, m_currentClassContext);
					if (cardsOnThisPage.Contains(newCardAnchor))
					{
						switch (transitionType)
						{
						case PageTransitionType.MANY_PAGE_RIGHT:
							nextPage++;
							break;
						case PageTransitionType.MANY_PAGE_LEFT:
							nextPage--;
							break;
						}
					}
					FlipToPage(nextPage, null, null, transitionType);
					return;
				}
				if (transitionType == PageTransitionType.MANY_PAGE_LEFT)
				{
					nextPage = m_classCardsCollection.GetFirstPageForTab(m_currentClassContext);
					if (nextPage > 0)
					{
						nextPage--;
						FlipToPage(nextPage, null, null, transitionType);
						return;
					}
				}
				else
				{
					nextPage = m_classCardsCollection.GetLastPageForTab(m_currentClassContext);
					if (nextPage > 0)
					{
						nextPage++;
						FlipToPage(nextPage, null, null, transitionType);
						return;
					}
				}
			}
		}
		TransitionPageWhenReady(PageTransitionType.MANY_PAGE_LEFT, useCurrentPageNum: false, null, null);
		m_deckRunesWereUpdatedOnCurrentPage = false;
	}

	private bool IsLeftPageInDeathKnightTab()
	{
		return m_classCardsCollection.GetCurrentTabInfoFromPage(m_currentPageNum - 1).tagClass == TAG_CLASS.DEATHKNIGHT;
	}

	private bool IsRightPageInDeathKnightTab()
	{
		return m_classCardsCollection.GetCurrentTabInfoFromPage(m_currentPageNum + 1).tagClass == TAG_CLASS.DEATHKNIGHT;
	}

	private static bool IsEditingDeathKnightDeck(out RunePattern deckRunes)
	{
		CollectionDeck deck = CollectionManager.Get().GetEditedDeck();
		bool isEditingDeathKnightDeck = deck?.HasClass(TAG_CLASS.DEATHKNIGHT) ?? false;
		deckRunes = (isEditingDeathKnightDeck ? deck.Runes : default(RunePattern));
		return isEditingDeathKnightDeck;
	}

	protected override void OnPageTransitionRequested()
	{
		m_numPageFlipsThisSession++;
		int @int = Options.Get().GetInt(Option.PAGE_MOUSE_OVERS);
		int updatedPageFlips = @int + 1;
		if (@int < m_numPlageFlipsBeforeStopShowingArrows)
		{
			Options.Get().SetInt(Option.PAGE_MOUSE_OVERS, updatedPageFlips);
		}
		ShowSetFilterTutorialIfNeeded();
	}

	protected override void OnPageTurnComplete(object callbackData, int operationId)
	{
		if (m_numPageFlipsThisSession % CollectiblePageManager.NUM_PAGE_FLIPS_UNTIL_UNLOAD_UNUSED_ASSETS == 0)
		{
			HearthstoneApplication hearthstoneApplication = HearthstoneApplication.Get();
			if (hearthstoneApplication != null)
			{
				hearthstoneApplication.UnloadUnusedAssets();
			}
		}
		TransitionReadyCallbackData transitionReadyCallbackData = callbackData as TransitionReadyCallbackData;
		CollectionPageDisplay oldPage = PageAsCollectionPage(transitionReadyCallbackData.m_otherPage);
		CollectionUtils.ViewMode viewMode = CollectionManager.Get().GetCollectibleDisplay().GetViewMode();
		bool num = oldPage != PageAsCollectionPage(GetCurrentPage());
		if (num || viewMode != CollectionUtils.ViewMode.HERO_SKINS)
		{
			oldPage.HideHeroSkinsDecor();
		}
		if (num || viewMode != CollectionUtils.ViewMode.HERO_PICKER)
		{
			oldPage.HideHeroPicker();
		}
		base.OnPageTurnComplete(callbackData, operationId);
	}

	private void ShowSetFilterTutorialIfNeeded()
	{
		if (!Options.Get().GetBool(Option.HAS_SEEN_SET_FILTER_TUTORIAL) && !CollectionManager.Get().IsInEditMode() && CollectionManager.Get().GetCollectibleDisplay().GetViewMode() == CollectionUtils.ViewMode.CARDS && m_cardsCollection.CardSetFilterIsAllStandardSets())
		{
			CollectionManagerDisplay cmd = CollectionManager.Get().GetCollectibleDisplay() as CollectionManagerDisplay;
			if (!(cmd == null) && !cmd.IsShowingSetFilterTray() && CollectionManager.Get().AccountHasWildCards() && RankMgr.Get().WildCardsAllowedInCurrentLeague() && m_numPageFlipsThisSession >= NUM_PAGE_FLIPS_BEFORE_SET_FILTER_TUTORIAL && cmd != null)
			{
				cmd.ShowSetFilterTutorial(UserAttentionBlocker.SET_ROTATION_CM_TUTORIALS);
				Options.Get().SetBool(Option.HAS_SEEN_SET_FILTER_TUTORIAL, val: true);
			}
		}
	}

	protected override void OnCollectionManagerViewModeChanged(CollectionUtils.ViewMode prevMode, CollectionUtils.ViewMode mode, CollectionUtils.ViewModeData userdata, bool triggerResponse)
	{
		if (!triggerResponse)
		{
			return;
		}
		Log.CollectionManager.Print("transitionPageId={0} pagesTurning={1} mode={2}-->{3} triggerResponse={4}", m_transitionPageId, m_pagesCurrentlyTurning, prevMode, mode, triggerResponse);
		UpdateCraftingModeButtonDustBottleVisibility(CollectionManager.Get().GetCardsToMassDisenchantCount());
		if (mode == CollectionUtils.ViewMode.DECK_TEMPLATE)
		{
			HideNonDeckTemplateTabs(hide: true);
		}
		if (mode != 0)
		{
			CollectionDeckTray.Get().GetCardsContent().HideDeckHelpPopup();
		}
		if (mode != CollectionUtils.ViewMode.HERO_SKINS)
		{
			CollectionManagerDisplay cmd = CollectionManager.Get().GetCollectibleDisplay() as CollectionManagerDisplay;
			if (cmd != null)
			{
				cmd.SetHeroSkinClass(null);
			}
		}
		m_currentPageNum = 1;
		if (userdata != null)
		{
			if (userdata.m_setPageByClass.HasValue)
			{
				TAG_CLASS tagClass = userdata.m_setPageByClass.GetValueOrDefault();
				CollectionTabInfo collectionTabInfo = default(CollectionTabInfo);
				collectionTabInfo.tagClass = tagClass;
				CollectionTabInfo tabInfo = collectionTabInfo;
				m_classCardsCollection.GetPageContentsForTab(tabInfo, 1, calculateCollectionPage: true, out m_currentPageNum);
			}
			else if (userdata.m_setPageByCard != null)
			{
				m_classCardsCollection.GetPageContentsForCard(userdata.m_setPageByCard, userdata.m_setPageByPremium, out m_currentPageNum, m_currentClassContext);
			}
		}
		int prevId = 0;
		int newId = 0;
		for (int i = 0; i < TAG_ORDERING.Length; i++)
		{
			if (prevMode == TAG_ORDERING[i])
			{
				prevId = i;
			}
			if (mode == TAG_ORDERING[i])
			{
				newId = i;
			}
		}
		PageTransitionType transition = ((newId - prevId >= 0) ? PageTransitionType.SINGLE_PAGE_RIGHT : PageTransitionType.SINGLE_PAGE_LEFT);
		DelOnPageTransitionComplete callback = null;
		object callbackData = null;
		if (userdata != null)
		{
			callback = userdata.m_pageTransitionCompleteCallback;
			callbackData = userdata.m_pageTransitionCompleteData;
		}
		if (m_turnPageCoroutine != null)
		{
			StopCoroutine(m_turnPageCoroutine);
		}
		CollectionDeckTray.Get().m_decksContent.UpdateDeckName(null, shouldValidateDeckName: false);
		CollectionDeckTray.Get().UpdateDoneButtonText();
		m_turnPageCoroutine = StartCoroutine(ViewModeChangedWaitToTurnPage(transition, prevMode == CollectionUtils.ViewMode.DECK_TEMPLATE, callback, callbackData));
	}

	private IEnumerator ViewModeChangedWaitToTurnPage(PageTransitionType transition, bool hideDeckTemplateBottomPanel, DelOnPageTransitionComplete callback, object callbackData)
	{
		if (m_deckTemplatePicker != null && hideDeckTemplateBottomPanel)
		{
			CollectionManager.Get().GetCollectibleDisplay().m_inputBlocker.gameObject.SetActive(value: true);
			m_deckTemplatePicker.ShowBottomPanel(show: false);
			while (m_deckTemplatePicker.IsShowingBottomPanel())
			{
				yield return null;
			}
			yield return StartCoroutine(m_deckTemplatePicker.ShowPacks(show: false));
			CollectionManager.Get().GetCollectibleDisplay().m_inputBlocker.gameObject.SetActive(value: false);
		}
		TransitionPageWhenReady(transition, useCurrentPageNum: true, callback, callbackData);
	}

	public void OnFavoriteHeroChanged(TAG_CLASS heroClass, NetCache.CardDefinition favoriteHero, bool isFavorite, object userData)
	{
		PageAsCollectionPage(GetCurrentPage()).UpdateFavoriteHeroSkins(CollectionManager.Get().GetCollectibleDisplay().GetViewMode());
	}

	public void OnFavoriteCardBackChanged(int newFavoriteCardBackID, bool isFavorite)
	{
		PageAsCollectionPage(GetCurrentPage()).UpdateFavoriteCardBacks(CollectionManager.Get().GetCollectibleDisplay().GetViewMode());
	}

	public void OnFavoriteBattlegroundsGuideSkinChanged(BattlegroundsGuideSkinId? newFavoriteBattlegroundsGuideSkinID)
	{
		PageAsCollectionPage(GetCurrentPage()).UpdateFavoriteBattlegroundsGuideSkin(CollectionManager.Get().GetCollectibleDisplay().GetViewMode());
	}

	public void OnFavoriteCoinChanged(int newFavoriteCoinId, bool isFavorite)
	{
		PageAsCollectionPage(GetCurrentPage()).UpdateFavoriteCoin(CollectionManager.Get().GetCollectibleDisplay().GetViewMode());
	}

	private HashSet<int> GetCurrentDeckTrayModeCardBackIds()
	{
		return CardBackManager.Get().GetCardBackIds(CollectionManager.Get().IsInEditMode());
	}

	private List<CollectibleCard> GetCurrentDeckTrayModeCosmeticCoins()
	{
		return CosmeticCoinManager.Get().GetFilteredCoins();
	}

	private bool ShouldClassFilterBeVisible()
	{
		return CollectionManager.Get().OwnsAnyCollectible();
	}
}
