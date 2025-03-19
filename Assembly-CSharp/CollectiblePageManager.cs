using System.Collections.Generic;
using Hearthstone.Core;
using PegasusShared;
using UnityEngine;

[CustomEditClass]
public abstract class CollectiblePageManager : TabbedBookPageManager
{
	public int m_numPlageFlipsBeforeStopShowingArrows;

	public static readonly float SELECT_TAB_ANIM_TIME = 0.2f;

	protected static readonly int NUM_PAGE_FLIPS_UNTIL_UNLOAD_UNUSED_ASSETS = 15;

	protected static readonly Vector3 TAB_LOCAL_EULERS = new Vector3(0f, 180f, 0f);

	protected static readonly float HIDDEN_TAB_LOCAL_Z_POS = -0.42f;

	protected static readonly string SELECT_TAB_COROUTINE_NAME = "SelectTabWhenReady";

	protected Coroutine m_turnPageCoroutine;

	protected bool m_initializedTabPositions;

	protected float m_deselectedTabHalfWidth;

	protected CollectibleCardFilter m_cardsCollection;

	protected override void Awake()
	{
		base.Awake();
		CollectibleDisplay cd = CollectionManager.Get()?.GetCollectibleDisplay();
		if (cd != null)
		{
			cd.OnViewModeChanged += OnCollectionManagerViewModeChanged;
		}
	}

	public virtual void OnDestroy()
	{
		CollectibleDisplay cd = CollectionManager.Get()?.GetCollectibleDisplay();
		if (cd != null)
		{
			cd.OnViewModeChanged -= OnCollectionManagerViewModeChanged;
		}
	}

	protected override void Update()
	{
		base.Update();
		UpdateMouseWheel();
	}

	public virtual void Exit()
	{
		CollectiblePageDisplay currentPage = GetCurrentCollectiblePage();
		if (!(currentPage == null))
		{
			currentPage.MarkAllShownCardsSeen();
		}
	}

	public void OnCollectionLoaded()
	{
		ShowOnlyCardsIOwn(PageTransitionType.NONE);
	}

	public void UpdateCurrentPageCardLocks(bool playSound)
	{
		GetCurrentCollectiblePage().UpdateCurrentPageCardLocks(playSound);
	}

	public void RefreshCurrentPageContents()
	{
		RefreshCurrentPageContents(PageTransitionType.NONE, null, null);
	}

	public void RefreshCurrentPageContents(PageTransitionType transition)
	{
		RefreshCurrentPageContents(transition, null, null);
	}

	public void RefreshCurrentPageContents(DelOnPageTransitionComplete callback, object callbackData)
	{
		RefreshCurrentPageContents(PageTransitionType.NONE, null, null);
	}

	public void RefreshCurrentPageContents(PageTransitionType transition, DelOnPageTransitionComplete callback, object callbackData)
	{
		UpdateFilteredCards();
		TransitionPageWhenReady(transition, useCurrentPageNum: true, callback, callbackData);
	}

	public CollectionCardVisual GetCardVisual(string cardID, TAG_PREMIUM premium)
	{
		return GetCurrentCollectiblePage().GetCardVisual(cardID, premium);
	}

	public void FilterByCardSets(List<TAG_CARD_SET> cardSets, bool transitionPage = true)
	{
		FilterByCardSets(cardSets, null, null, transitionPage);
	}

	public void FilterByCardSets(List<TAG_CARD_SET> cardSets, DelOnPageTransitionComplete callback, object callbackData, bool transitionPage = true)
	{
		TAG_CARD_SET[] cardSetsCasted = null;
		if (cardSets != null && cardSets.Count > 0)
		{
			cardSetsCasted = cardSets.ToArray();
		}
		m_cardsCollection.ClearOutFiltersFromSetFilterDropdown();
		m_cardsCollection.FilterTheseCardSets(cardSetsCasted);
		UpdateFilteredCards();
		if (transitionPage)
		{
			PageTransitionType transition = ((!SceneMgr.Get().IsTransitioning()) ? PageTransitionType.SINGLE_PAGE_RIGHT : PageTransitionType.NONE);
			TransitionPageWhenReady(transition, useCurrentPageNum: false, callback, callbackData);
		}
	}

	public void FilterBySpecificCards(List<int> specificCards)
	{
		m_cardsCollection.ClearOutFiltersFromSetFilterDropdown();
		m_cardsCollection.FilterSpecificCards(specificCards);
		UpdateFilteredCards();
		PageTransitionType transition = ((!SceneMgr.Get().IsTransitioning()) ? PageTransitionType.SINGLE_PAGE_RIGHT : PageTransitionType.NONE);
		TransitionPageWhenReady(transition, useCurrentPageNum: false, null, null);
	}

	public void FilterBySubsetsCardsetsAndClasses(List<HashSet<string>> subsets, List<TAG_CARD_SET> cardSets, List<TAG_CLASS> classes, bool transitionPage)
	{
		m_cardsCollection.ClearOutFiltersFromSetFilterDropdown();
		m_cardsCollection.FilterTheseSubsets(subsets);
		TAG_CARD_SET[] cardSetsCasted = null;
		if (cardSets != null && cardSets.Count > 0)
		{
			cardSetsCasted = cardSets.ToArray();
		}
		m_cardsCollection.FilterTheseCardSets(cardSetsCasted);
		if (classes != null && classes.Count > 0)
		{
			TAG_CLASS[] classesCasted = classes.ToArray();
			m_cardsCollection.FilterTheseClasses(classesCasted);
		}
		UpdateFilteredCards();
		if (transitionPage)
		{
			PageTransitionType transition = ((!SceneMgr.Get().IsTransitioning()) ? PageTransitionType.SINGLE_PAGE_RIGHT : PageTransitionType.NONE);
			TransitionPageWhenReady(transition, useCurrentPageNum: false, null, null);
		}
	}

	public void FilterByClasses(List<TAG_CLASS> classes)
	{
		TAG_CLASS[] classesCasted = null;
		if (classes != null && classes.Count > 0)
		{
			classesCasted = classes.ToArray();
		}
		m_cardsCollection.FilterTheseClasses(classesCasted);
		UpdateFilteredCards();
	}

	public bool CardSetFilterIncludesWild()
	{
		return m_cardsCollection.CardSetFilterIncludesWild();
	}

	public bool CardSetFilterIsClassic()
	{
		return m_cardsCollection.CardSetFilterIsClassicSet();
	}

	public bool CardSetFilterIsTwist()
	{
		return m_cardsCollection.CardSetFilterIsTwistSet();
	}

	public bool HasSearchTextFilter()
	{
		return m_cardsCollection.HasSearchText();
	}

	public void ChangeSearchTextFilter(string newSearchText, bool transitionPage = true)
	{
		ChangeSearchTextFilter(newSearchText, null, null, transitionPage);
	}

	public virtual void ChangeSearchTextFilter(string newSearchText, DelOnPageTransitionComplete callback, object callbackData, bool transitionPage = true)
	{
		m_cardsCollection.FilterSearchText(newSearchText);
		UpdateFilteredCards();
		if (transitionPage)
		{
			TransitionPageWhenReady(PageTransitionType.MANY_PAGE_LEFT, useCurrentPageNum: false, callback, callbackData);
		}
	}

	public void RemoveSearchTextFilter()
	{
		RemoveSearchTextFilter(null, null);
	}

	public virtual void RemoveSearchTextFilter(DelOnPageTransitionComplete callback, object callbackData, bool transitionPage = true)
	{
		m_cardsCollection.FilterSearchText(null);
		UpdateFilteredCards();
		if (transitionPage)
		{
			TransitionPageWhenReady(PageTransitionType.NONE, useCurrentPageNum: false, callback, callbackData);
		}
	}

	public void ShowOnlyCardsIOwn(PageTransitionType? pageTransition = null)
	{
		ShowOnlyCardsIOwn(null, null, pageTransition);
	}

	public void ShowOnlyCardsIOwn(DelOnPageTransitionComplete callback, object callbackData, PageTransitionType? pageTransition = null)
	{
		m_cardsCollection.FilterOnlyOwned(owned: true);
		m_cardsCollection.FilterByMask(null);
		m_cardsCollection.FilterByCraftability(null);
		UpdateFilteredCards();
		if (pageTransition.HasValue)
		{
			TransitionPageWhenReady(pageTransition.Value, useCurrentPageNum: false, callback, callbackData);
		}
	}

	public void ShowCardsNotOwned(bool includePremiums, PageTransitionType? pageTransition = null)
	{
		ShowCardsNotOwned(includePremiums, null, null, pageTransition);
	}

	public void ShowCardsNotOwned(bool includePremiums, DelOnPageTransitionComplete callback, object callbackData, PageTransitionType? pageTransition = null)
	{
		m_cardsCollection.FilterOnlyOwned(owned: false);
		m_cardsCollection.FilterByMask(null);
		UpdateFilteredCards();
		if (pageTransition.HasValue)
		{
			TransitionPageWhenReady(pageTransition.Value, useCurrentPageNum: false, callback, callbackData);
		}
	}

	public bool JumpToPageWithCard(string cardID, TAG_PREMIUM premium)
	{
		return JumpToPageWithCard(cardID, premium, null, null);
	}

	public virtual void HideCraftingModeCards(PageTransitionType transitionType = PageTransitionType.NONE, bool updatePage = true)
	{
		m_cardsCollection.FilterByCraftability(null);
		m_cardsCollection.FilterByMask(null);
		m_cardsCollection.FilterOnlyOwned(owned: true);
		m_cardsCollection.FilterLeagueBannedCardsSubset(null);
		UpdateFilteredCards();
		if (updatePage)
		{
			TransitionPageWhenReady(transitionType, useCurrentPageNum: false, null, null);
		}
	}

	public abstract bool JumpToPageWithCard(string cardID, TAG_PREMIUM premium, DelOnPageTransitionComplete callback, object callbackData);

	public abstract void NotifyOfCollectionChanged();

	protected CollectiblePageDisplay GetCurrentCollectiblePage()
	{
		return GetCurrentPage() as CollectiblePageDisplay;
	}

	protected void TransitionPageNextFrame(TransitionReadyCallbackData transitionReadyCallbackData)
	{
		Processor.ScheduleCallback(0f, realTime: false, delegate
		{
			TransitionPage(transitionReadyCallbackData);
		});
	}

	protected bool AssembleCollectionBasePage(TransitionReadyCallbackData transitionReadyCallbackData, bool emptyPage, FormatType formatType)
	{
		CollectiblePageDisplay page = transitionReadyCallbackData.m_assembledPage as CollectiblePageDisplay;
		if (page == null)
		{
			Log.CollectionManager.PrintError("CollectiblePageManager.AssembleCollectionBasePage - page is null!");
			return false;
		}
		page.UpdateBasePage();
		page.SetPageType(formatType);
		page.ActivatePageCountText(active: true);
		if (emptyPage)
		{
			SetHasPreviousAndNextPages(hasPreviousPage: false, hasNextPage: false);
			AssembleEmptyPageUI(page, displayNoMatchesText: true);
			CollectionManager.Get().GetCollectibleDisplay().CollectionPageContentsChanged<ICollectible>(null, delegate(List<CollectionCardActors> actorList, List<ICollectible> nonActorCollectibleList, object data)
			{
				page.UpdateCollectionItems(actorList, nonActorCollectibleList, CollectionManager.Get().GetCollectibleDisplay().GetViewMode());
				TransitionPage(transitionReadyCallbackData);
			}, null);
			return true;
		}
		return false;
	}

	protected virtual bool AssembleCollectiblePage<TCollectible>(TransitionReadyCallbackData transitionReadyCallbackData, ICollection<TCollectible> collectiblesToDisplay, int totalNumPages) where TCollectible : ICollectible
	{
		bool emptyPage = collectiblesToDisplay == null || collectiblesToDisplay.Count == 0;
		Log.CollectionManager.Print("transitionPageId={0} pagesTurning={1} currentPageIsPageA={2} emptyPage={3}", m_transitionPageId, m_pagesCurrentlyTurning, m_currentPageIsPageA, emptyPage);
		FormatType formatType = CollectionManager.Get().GetThemeShowing();
		if (AssembleCollectionBasePage(transitionReadyCallbackData, emptyPage, formatType))
		{
			return true;
		}
		return false;
	}

	protected virtual void UpdateFilteredCards()
	{
		m_cardsCollection.UpdateResults();
	}

	protected virtual void UpdateMouseWheel()
	{
		if (UniversalInputManager.Get().IsTouchMode() || !CanUserTurnPages())
		{
			return;
		}
		Input.GetAxis("Mouse ScrollWheel");
		if (m_hasNextPage && Input.GetAxis("Mouse ScrollWheel") > 0f)
		{
			if (UniversalInputManager.Get().InputIsOver(GetCurrentPage().gameObject))
			{
				PageRight(null, null);
			}
		}
		else if (m_hasPreviousPage && Input.GetAxis("Mouse ScrollWheel") < 0f && UniversalInputManager.Get().InputIsOver(GetCurrentPage().gameObject))
		{
			PageLeft(null, null);
		}
	}

	protected abstract void AssembleEmptyPageUI(CollectiblePageDisplay page, bool displayNoMatchesText);

	protected abstract void OnCollectionManagerViewModeChanged(CollectionUtils.ViewMode prevMode, CollectionUtils.ViewMode mode, CollectionUtils.ViewModeData userdata, bool triggerResponse);
}
