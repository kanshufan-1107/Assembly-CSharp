using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets;
using Blizzard.T5.Core;
using Hearthstone;
using Hearthstone.DataModels;
using Hearthstone.UI;
using PegasusShared;
using UnityEngine;

[CustomEditClass]
public class LettuceCollectionPageManager : CollectiblePageManager
{
	public static readonly Map<TAG_ROLE, UnityEngine.Vector2> s_roleTextureOffsets = new Map<TAG_ROLE, UnityEngine.Vector2>
	{
		{
			TAG_ROLE.CASTER,
			new UnityEngine.Vector2(0f, 0f)
		},
		{
			TAG_ROLE.FIGHTER,
			new UnityEngine.Vector2(0.205f, -0.2f)
		},
		{
			TAG_ROLE.TANK,
			new UnityEngine.Vector2(0.205f, 0f)
		}
	};

	public static TAG_ROLE[] ROLE_TAB_ORDER = new TAG_ROLE[3]
	{
		TAG_ROLE.TANK,
		TAG_ROLE.FIGHTER,
		TAG_ROLE.CASTER
	};

	private static readonly float MOBILE_HIDDEN_TAB_LOCAL_Z_POS = -10f;

	[CustomEditField(Sections = "Widgets")]
	public AsyncReference m_lettuceCollectionPageManagerAudioReference;

	private VisualController m_lettuceCollectionPageManagerAudio;

	private List<LettuceRoleTab> m_roleTabs = new List<LettuceRoleTab>();

	private int m_numPageFlipsThisSession;

	protected TAG_ROLE m_currentRoleContext;

	private LettuceMercenary m_lastMercAnchor;

	private Coroutine m_animatingTabsCoroutine;

	private int[] m_activePages = new int[2];

	private int m_nextActivePageIndex;

	protected static readonly int MERCS_COLLECTION_NUM_PAGE_FLIPS_UNTIL_UNLOAD_UNUSED_ASSETS = 15;

	public TAG_ROLE CurrentRoleContext => m_currentRoleContext;

	private bool UsesTabs => m_tabContainer != null;

	private CollectibleCardRoleFilter m_roleCardsCollection => (CollectibleCardRoleFilter)m_cardsCollection;

	public event EventHandler PageTransitioned;

	protected override void Awake()
	{
		m_cardsCollection = new CollectibleCardRoleFilter();
		base.Awake();
		m_roleCardsCollection.Init(ROLE_TAB_ORDER, CollectiblePageDisplay.GetMaxCardsPerPage(CollectionUtils.ViewMode.CARDS));
		UpdateFilteredCards();
	}

	protected override void Start()
	{
		base.Start();
		m_lettuceCollectionPageManagerAudioReference.RegisterReadyListener(delegate(VisualController vc)
		{
			m_lettuceCollectionPageManagerAudio = vc;
		});
	}

	public override void OnDestroy()
	{
		base.OnDestroy();
	}

	public override bool JumpToPageWithCard(string cardID, TAG_PREMIUM premium, DelOnPageTransitionComplete callback, object callbackData)
	{
		LettuceMercenary merc = CollectionManager.Get().GetMercenary(cardID);
		if (m_roleCardsCollection.GetPageContentsForMercenary(merc, out var collectionPage).Count == 0)
		{
			return false;
		}
		if (m_currentPageNum == collectionPage)
		{
			return false;
		}
		FlipToPage(collectionPage, callback, callbackData);
		return true;
	}

	public void UpdateTabNewCardCounts()
	{
		foreach (LettuceRoleTab roleTab in m_roleTabs)
		{
			TAG_ROLE tabRole = roleTab.GetRole();
			int numNewCards = GetNumNewCardsForRole(tabRole);
			roleTab.UpdateNewItemCount(numNewCards);
		}
	}

	public int GetNumNewCardsForRole(TAG_ROLE tagRole)
	{
		return m_roleCardsCollection.GetNumNewCardsForRole(tagRole);
	}

	public List<LettuceMercenary> GetCurrentMercenaryResults()
	{
		return m_roleCardsCollection.FindMercenariesResult.m_mercenaries;
	}

	public List<LettuceMercenary> GetRoleSortedMercenaryResults()
	{
		return m_roleCardsCollection.GetAllRoleResults();
	}

	public void OnDoneEditingTeam()
	{
		LettuceTeamDataModel teamDataModel = CollectionDeckTray.Get().GetMercsContent().SelectedTeamDataModel;
		if (teamDataModel != null)
		{
			foreach (LettuceMercenaryDataModel mercenary in teamDataModel.MercenaryList)
			{
				mercenary.InCurrentTeam = false;
			}
		}
		LettuceCollectionPageDisplay lcpd = GetCurrentCollectiblePage() as LettuceCollectionPageDisplay;
		if (lcpd != null)
		{
			lcpd.ClearCurrentPageCardLocks();
		}
	}

	public override void NotifyOfCollectionChanged()
	{
	}

	public bool HasRoleCardsAvailable(TAG_ROLE roleTag)
	{
		return m_roleCardsCollection.GetNumPagesForRole(roleTag) > 0;
	}

	public void ShowCraftingModeMercs(DelOnPageTransitionComplete callback = null, object callbackData = null, bool showCraftableMercs = true, bool showOnlyPromotableMercs = false, bool updatePage = true, bool toggleChanged = false)
	{
		if (m_cardsCollection is CollectibleCardRoleFilter roleCollection)
		{
			roleCollection.FilterOnlyOwned(showCraftableMercs);
			roleCollection.FilterOnlyUpgradeableMercs(showOnlyPromotableMercs);
			UpdateFilteredCards();
			PageTransitionType transitionType = (toggleChanged ? PageTransitionType.MANY_PAGE_LEFT : PageTransitionType.NONE);
			if (toggleChanged)
			{
				m_lastMercAnchor = null;
			}
			if (updatePage)
			{
				TransitionPageWhenReady(transitionType, useCurrentPageNum: false, callback, callbackData);
			}
		}
	}

	public override void HideCraftingModeCards(PageTransitionType transitionType = PageTransitionType.NONE, bool updatePage = true)
	{
		if (m_cardsCollection is CollectibleCardRoleFilter roleCollection)
		{
			roleCollection.FilterOnlyUpgradeableMercs(onlyUpgradeable: false);
		}
		base.HideCraftingModeCards(transitionType);
	}

	public bool GetShowOnlyFullyUpgradedMercs()
	{
		if (m_cardsCollection is CollectibleCardRoleFilter { m_filterOnlyFulIyUpgraded: var filterOnlyFulIyUpgraded })
		{
			return filterOnlyFulIyUpgraded == true;
		}
		return false;
	}

	public void SetOnlyShowFullyUpgradedMercs(bool fullyUpgradedOnly, DelOnPageTransitionComplete callback, object callbackData, bool transitionPage = true)
	{
		if (m_cardsCollection is CollectibleCardRoleFilter roleCollection)
		{
			roleCollection.FilterOnlyFullyUpgraded(fullyUpgradedOnly);
		}
		UpdateFilteredCards();
		if (transitionPage)
		{
			TransitionPageWhenReady(PageTransitionType.MANY_PAGE_LEFT, useCurrentPageNum: false, callback, callbackData);
		}
	}

	public void UpdatePageMercenary(LettuceMercenaryDataModel dataModel)
	{
		LettuceCollectionPageDisplay lcpd = GetCurrentCollectiblePage() as LettuceCollectionPageDisplay;
		if (!(lcpd == null))
		{
			lcpd.UpdateMercenaryOnPage(dataModel);
		}
	}

	public void UpdateAcknowledgedStatusForPageMercenary(int mercID, bool status)
	{
		LettuceCollectionPageDisplay lcpd = GetCurrentCollectiblePage() as LettuceCollectionPageDisplay;
		if (!(lcpd == null))
		{
			lcpd.UpdateAcknowledgeStatusForMercenaryOnPage(mercID, status);
		}
	}

	public LettuceMercenaryDataModel GetMercenaryOnPage(int mercenaryId)
	{
		LettuceCollectionPageDisplay lcpd = GetCurrentCollectiblePage() as LettuceCollectionPageDisplay;
		if (lcpd == null)
		{
			return null;
		}
		return lcpd.GetMercenaryOnPage(mercenaryId);
	}

	protected override bool ShouldShowTab(BookTab tab)
	{
		if (!m_initializedTabPositions)
		{
			return true;
		}
		LettuceRoleTab roleTab = tab as LettuceRoleTab;
		if (roleTab == null)
		{
			Log.CollectionManager.PrintError("CollectionPageManager.ShouldShowTab passed a non-LettuceRoleTab object.");
			return false;
		}
		return HasRoleCardsAvailable(roleTab.GetRole());
	}

	protected override void SetUpBookTabs()
	{
		if (!UsesTabs)
		{
			return;
		}
		bool isTouch = UniversalInputManager.Get().IsTouchMode();
		for (int i = 0; i < ROLE_TAB_ORDER.Length; i++)
		{
			TAG_ROLE roleTag = ROLE_TAB_ORDER[i];
			LettuceRoleTab roleTab = (LettuceRoleTab)GameUtils.Instantiate(m_tabPrefab, m_tabContainer);
			roleTab.Init(roleTag);
			roleTab.transform.localScale = (UniversalInputManager.UsePhoneUI ? roleTab.m_MobileDeselectedLocalScale : roleTab.m_DeselectedLocalScale);
			roleTab.transform.localEulerAngles = CollectiblePageManager.TAB_LOCAL_EULERS;
			roleTab.AddEventListener(UIEventType.RELEASE, OnRoleTabPressed);
			roleTab.AddEventListener(UIEventType.ROLLOVER, OnTabOver);
			roleTab.AddEventListener(UIEventType.ROLLOUT, base.OnTabOut);
			roleTab.AddEventListener(UIEventType.ROLLOVER, base.OnTabOver_Touch);
			roleTab.AddEventListener(UIEventType.ROLLOUT, base.OnTabOut_Touch);
			roleTab.SetReceiveReleaseWithoutMouseDown(isTouch);
			roleTab.gameObject.name = roleTag.ToString();
			m_allTabs.Add(roleTab);
			m_roleTabs.Add(roleTab);
			m_tabVisibility[roleTab] = true;
			if (i <= 0)
			{
				m_deselectedTabHalfWidth = roleTab.GetComponent<BoxCollider>().bounds.extents.x;
			}
		}
		PositionBookTabs(animate: false);
		m_initializedTabPositions = true;
	}

	private void SetupBookTab(bool show, bool animate)
	{
		if (!UsesTabs)
		{
			return;
		}
		Vector3 visibleTabPosition = m_tabContainer.transform.position;
		int tabCount = ROLE_TAB_ORDER.Length;
		for (int i = 0; i < tabCount; i++)
		{
			LettuceRoleTab tab = m_roleTabs[i];
			Vector3 tabLocalPos;
			if (show && ShouldShowTab(tab))
			{
				tab.SetTargetVisibility(visible: true);
				visibleTabPosition.x += m_spaceBetweenTabs;
				visibleTabPosition.x += m_deselectedTabHalfWidth;
				tabLocalPos = m_tabContainer.transform.InverseTransformPoint(visibleTabPosition);
				if (tab == m_currentTab)
				{
					tabLocalPos.y = tab.m_SelectedLocalYPos;
					tabLocalPos += tab.m_SelectedLocalOffset;
				}
				visibleTabPosition.x += m_deselectedTabHalfWidth;
			}
			else
			{
				tab.SetTargetVisibility(visible: false);
				tabLocalPos = tab.transform.localPosition;
				tabLocalPos.z = (UniversalInputManager.UsePhoneUI ? MOBILE_HIDDEN_TAB_LOCAL_Z_POS : CollectiblePageManager.HIDDEN_TAB_LOCAL_Z_POS);
			}
			if (animate)
			{
				tab.SetTargetLocalPosition(tabLocalPos);
				continue;
			}
			tab.SetIsVisible(tab.ShouldBeVisible());
			tab.transform.localPosition = tabLocalPos;
		}
	}

	protected override void PositionBookTabs(bool animate)
	{
		SetupBookTab(show: true, animate);
		if (animate)
		{
			if (m_animatingTabsCoroutine != null)
			{
				StopCoroutine(m_animatingTabsCoroutine);
			}
			m_animatingTabsCoroutine = StartCoroutine(AnimateTabs());
		}
	}

	private IEnumerator AnimateTabs(bool allowSFX = true)
	{
		bool playSounds = allowSFX && (HeroPickerDisplay.Get() == null || !HeroPickerDisplay.Get().IsShown());
		List<LettuceRoleTab> tabsToHide = new List<LettuceRoleTab>();
		List<LettuceRoleTab> tabsToShow = new List<LettuceRoleTab>();
		List<LettuceRoleTab> tabsToMove = new List<LettuceRoleTab>();
		foreach (LettuceRoleTab tab in m_roleTabs)
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
		m_tabsAreAnimating = true;
		if (tabsToHide.Count > 0)
		{
			foreach (LettuceRoleTab tab2 in tabsToHide)
			{
				if (playSounds)
				{
					SoundManager.Get().LoadAndPlay("class_tab_retract.prefab:da79957be76b10343999d6fa92a6a2f0", tab2.gameObject);
				}
				yield return new WaitForSeconds(0.03f);
				tab2.AnimateToTargetPosition(0.1f, iTween.EaseType.easeOutQuad);
			}
			yield return new WaitForSeconds(0.1f);
		}
		if (tabsToMove.Count > 0)
		{
			foreach (LettuceRoleTab tab3 in tabsToMove)
			{
				if (tab3.WillSlide() && playSounds)
				{
					SoundManager.Get().LoadAndPlay("class_tab_slides_across_top.prefab:04482bc6f531b76468ff92a5b4e979b6", tab3.gameObject);
				}
				tab3.AnimateToTargetPosition(0.25f, iTween.EaseType.easeOutQuad);
			}
			yield return new WaitForSeconds(0.25f);
		}
		if (tabsToShow.Count > 0)
		{
			foreach (LettuceRoleTab tab4 in tabsToShow)
			{
				if (playSounds)
				{
					SoundManager.Get().LoadAndPlay("class_tab_retract.prefab:da79957be76b10343999d6fa92a6a2f0", tab4.gameObject);
				}
				tab4.AnimateToTargetPosition(0.4f, iTween.EaseType.easeOutBounce);
			}
			yield return new WaitForSeconds(0.4f);
		}
		foreach (LettuceRoleTab roleTab in m_roleTabs)
		{
			roleTab.SetIsVisible(roleTab.ShouldBeVisible());
		}
		m_tabsAreAnimating = false;
	}

	public void PlayTabTuckAnimation(bool forward, bool animate = true, bool allowSFX = true)
	{
		SetupBookTab(!forward, animate);
		if (animate)
		{
			if (m_animatingTabsCoroutine != null)
			{
				StopCoroutine(m_animatingTabsCoroutine);
			}
			m_animatingTabsCoroutine = StartCoroutine(AnimateTabs(allowSFX));
		}
	}

	private void SetCurrentRoleTab(TAG_ROLE? tabRole)
	{
		LettuceRoleTab selectNewTab = null;
		if (CollectionManager.Get().GetCollectibleDisplay().GetViewMode() == CollectionUtils.ViewMode.CARDS)
		{
			if (tabRole.HasValue)
			{
				selectNewTab = m_roleTabs.Find((LettuceRoleTab obj) => obj.GetRole() == tabRole.Value && obj.m_tabViewMode != CollectionUtils.ViewMode.DECK_TEMPLATE);
			}
		}
		else
		{
			selectNewTab = null;
		}
		if (selectNewTab == m_currentTab)
		{
			return;
		}
		DeselectCurrentTab();
		if (tabRole.HasValue)
		{
			switch (tabRole.GetValueOrDefault())
			{
			case TAG_ROLE.CASTER:
				MusicManager.Get().StartPlaylist(MusicPlaylistType.UI_MercenariesCMCaster);
				break;
			case TAG_ROLE.FIGHTER:
				MusicManager.Get().StartPlaylist(MusicPlaylistType.UI_MercenariesCMFighter);
				break;
			case TAG_ROLE.TANK:
				MusicManager.Get().StartPlaylist(MusicPlaylistType.UI_MercenariesCMTank);
				break;
			}
		}
		m_currentTab = selectNewTab;
		if (m_currentTab != null)
		{
			StopCoroutine(CollectiblePageManager.SELECT_TAB_COROUTINE_NAME);
			StartCoroutine(CollectiblePageManager.SELECT_TAB_COROUTINE_NAME, m_currentTab);
		}
	}

	public void SelectRole(TAG_ROLE role)
	{
		if (CanUserTurnPages())
		{
			OnRoleSelected(role);
		}
	}

	private void OnRoleTabPressed(UIEvent e)
	{
		if (CanUserTurnPages())
		{
			LettuceRoleTab roleTab = e.GetElement() as LettuceRoleTab;
			if (!(roleTab == null) && !(roleTab == m_currentTab))
			{
				roleTab.PlayClickFX();
				TAG_ROLE selectedRole = roleTab.GetRole();
				OnRoleSelected(selectedRole);
			}
		}
	}

	private void OnRoleSelected(TAG_ROLE role)
	{
		m_lettuceCollectionPageManagerAudio.SetState(role.ToString() + "_TAB_CLICKED_code");
		JumpToCollectionRolePage(role);
	}

	public void JumpToCollectionRolePage(TAG_ROLE pageRole)
	{
		JumpToCollectionRolePage(pageRole, null, null);
	}

	public void JumpToCollectionRolePage(TAG_ROLE pageRole, DelOnPageTransitionComplete callback, object callbackData)
	{
		CollectibleDisplay cd = CollectionManager.Get().GetCollectibleDisplay();
		if (cd != null && cd.GetViewMode() != 0)
		{
			cd.SetViewMode(CollectionUtils.ViewMode.CARDS, new CollectionUtils.ViewModeData
			{
				m_setPageByRole = pageRole
			});
		}
		else
		{
			int newCollectionPage = 0;
			m_roleCardsCollection.GetPageContentsForRole(pageRole, 1, calculateCollectionPage: true, out newCollectionPage);
			FlipToPage(newCollectionPage, callback, callbackData);
		}
	}

	protected override void AssembleEmptyPageUI(BookPageDisplay page)
	{
		base.AssembleEmptyPageUI(page);
		AssembleEmptyPageUI(page as LettuceCollectionPageDisplay, displayNoMatchesText: false);
	}

	protected override void AssembleEmptyPageUI(CollectiblePageDisplay page, bool displayNoMatchesText)
	{
		LettuceCollectionPageDisplay lcpd = page as LettuceCollectionPageDisplay;
		if (lcpd == null)
		{
			Log.CollectionManager.PrintError("Page in LettuceCollectionPageManager is not a LettuceCollectionPageDisplay! This should not happen!");
			return;
		}
		lcpd.SetRole(null);
		lcpd.ShowNoMatchesFound(displayNoMatchesText, m_roleCardsCollection.FindCardsResult);
		lcpd.UpdateCollectionMercs(null);
		DeselectCurrentTab();
		lcpd.SetPageCountText(GameStrings.Get("GLUE_COLLECTION_EMPTY_PAGE"));
	}

	protected bool AssembleMercenaryPage(TransitionReadyCallbackData transitionReadyCallbackData, List<LettuceMercenary> cardsToDisplay, int totalNumPages)
	{
		bool emptyPage = cardsToDisplay == null || cardsToDisplay.Count == 0;
		CollectionUtils.ViewMode viewMode = CollectionManager.Get().GetCollectibleDisplay().GetViewMode();
		Log.CollectionManager.Print("transitionPageId={0} pagesTurning={1} currentPageIsPageA={2} emptyPage={3} viewMode={4}", m_transitionPageId, m_pagesCurrentlyTurning, m_currentPageIsPageA, emptyPage, viewMode);
		if (AssembleCollectionBasePage(transitionReadyCallbackData, emptyPage, FormatType.FT_STANDARD))
		{
			return true;
		}
		LettuceCollectionPageDisplay page = transitionReadyCallbackData.m_assembledPage as LettuceCollectionPageDisplay;
		m_lastMercAnchor = cardsToDisplay[0];
		TAG_ROLE currentRole = m_roleCardsCollection.GetCurrentRoleFromPage(m_currentPageNum);
		page.SetRole(currentRole);
		m_currentRoleContext = currentRole;
		page.SetPageCountText(GameStrings.Format("GLUE_COLLECTION_PAGE_NUM", m_currentPageNum));
		page.ShowNoMatchesFound(show: false);
		SetHasPreviousAndNextPages(m_currentPageNum > 1, m_currentPageNum < totalNumPages);
		page.UpdateCollectionMercs(cardsToDisplay, transitionReadyCallbackData.m_transitionType);
		if (transitionReadyCallbackData.m_transitionType == PageTransitionType.NONE)
		{
			page.WaitForPageUpdate(TransitionPage, transitionReadyCallbackData);
		}
		else
		{
			TransitionPageNextFrame(transitionReadyCallbackData);
		}
		return true;
	}

	protected override void AssemblePage(TransitionReadyCallbackData transitionReadyCallbackData, bool useCurrentPageNum)
	{
		if (CollectionManager.Get().GetCollectibleDisplay() == null)
		{
			return;
		}
		CollectionUtils.ViewMode viewMode = CollectionManager.Get().GetCollectibleDisplay().GetViewMode();
		if (m_roleCardsCollection == null)
		{
			Log.Lettuce.PrintError("LettuceCollectionPageManager.AssemblePage - card collection is null!");
		}
		else
		{
			if (viewMode != 0)
			{
				return;
			}
			List<LettuceMercenary> cardsToDisplay = null;
			if (useCurrentPageNum)
			{
				cardsToDisplay = m_roleCardsCollection.GetMercenariesPageContents(m_currentPageNum);
			}
			else
			{
				if (!LettuceTutorialUtils.IsEventTypeComplete(LettuceTutorialVo.LettuceTutorialEvent.VILLAGE_TUTORIAL_UPGRADE_ABILITY_END))
				{
					List<LettuceMercenary> mercs = CollectionManager.Get().FindMercenaries(null, true, null, null, null, ordered: true, null).m_mercenaries;
					m_lastMercAnchor = mercs.FirstOrDefault((LettuceMercenary m) => m.CanAnyAbilityBeUpgraded());
				}
				if (m_lastMercAnchor == null)
				{
					m_currentPageNum = 1;
					cardsToDisplay = m_roleCardsCollection.GetMercenariesPageContents(m_currentPageNum);
				}
				else
				{
					if (m_roleCardsCollection == null)
					{
						Log.Lettuce.PrintError("LettuceCollectionPageManager.AssemblePage - role collection is null!");
						return;
					}
					cardsToDisplay = m_roleCardsCollection.GetPageContentsForMercenary(m_lastMercAnchor, out var currentPageNum);
					if (cardsToDisplay.Count == 0)
					{
						cardsToDisplay = m_roleCardsCollection.GetPageContentsForRole(m_currentRoleContext, 1, calculateCollectionPage: true, out currentPageNum);
					}
					if (cardsToDisplay.Count > 1)
					{
						foreach (LettuceMercenary merc in cardsToDisplay)
						{
							if (merc.ID == 69)
							{
								cardsToDisplay.Remove(merc);
								cardsToDisplay.Insert(0, merc);
								break;
							}
						}
					}
					if (cardsToDisplay.Count == 0)
					{
						cardsToDisplay = m_roleCardsCollection.GetMercenariesPageContents(1);
						currentPageNum = 1;
					}
					m_currentPageNum = ((cardsToDisplay.Count != 0) ? currentPageNum : 0);
				}
				LettuceCollectionDisplay lcd = CollectionManager.Get().GetCollectibleDisplay() as LettuceCollectionDisplay;
				if (lcd != null && lcd.CanShowAppearanceTip(checkForMercOnPage: false) && cardsToDisplay.Count > 1)
				{
					foreach (LettuceMercenary merc2 in cardsToDisplay)
					{
						if (merc2.ID == 18)
						{
							cardsToDisplay.Remove(merc2);
							cardsToDisplay.Insert(0, merc2);
							break;
						}
					}
				}
			}
			if (cardsToDisplay == null || cardsToDisplay.Count == 0)
			{
				cardsToDisplay = m_roleCardsCollection.GetFirstNonEmptyMercenaryPage(out var tmpPageNum);
				if (cardsToDisplay.Count > 0)
				{
					m_currentPageNum = tmpPageNum;
				}
			}
			AssembleMercenaryPage(transitionReadyCallbackData, cardsToDisplay, m_roleCardsCollection.GetTotalNumPages());
			UpdateCurrentPageCardLocks(playSound: false);
		}
	}

	protected override void UpdateFilteredCards()
	{
		base.UpdateFilteredCards();
		UpdateTabNewCardCounts();
	}

	protected override void TransitionPage(object callbackData)
	{
		base.TransitionPage(callbackData);
		if (CollectionManager.Get().GetCollectibleDisplay().GetViewMode() == CollectionUtils.ViewMode.MASS_DISENCHANT)
		{
			DeselectCurrentTab();
			return;
		}
		SetCurrentRoleTab(m_currentRoleContext);
		this.PageTransitioned?.Invoke(this, new EventArgs());
	}

	protected override void TransitionPageWhenReady(PageTransitionType transitionType, bool useCurrentPageNum, DelOnPageTransitionComplete callback, object callbackData)
	{
		if (transitionType == PageTransitionType.NONE || LettuceTutorialUtils.IsEventTypeComplete(LettuceTutorialVo.LettuceTutorialEvent.VILLAGE_TUTORIAL_UPGRADE_ABILITY_END))
		{
			base.TransitionPageWhenReady(transitionType, useCurrentPageNum, callback, callbackData);
		}
	}

	protected override void OnPageTransitionRequested()
	{
		int @int = Options.Get().GetInt(Option.PAGE_MOUSE_OVERS);
		int updatedPageFlips = @int + 1;
		if (@int < m_numPlageFlipsBeforeStopShowingArrows)
		{
			Options.Get().SetInt(Option.PAGE_MOUSE_OVERS, updatedPageFlips);
		}
		(CollectionManager.Get().GetCollectibleDisplay() as LettuceCollectionDisplay).HideHelpPopups();
		if (!m_activePages.Contains(base.CurrentPageNum))
		{
			m_numPageFlipsThisSession++;
		}
		m_activePages[m_nextActivePageIndex] = base.CurrentPageNum;
		m_nextActivePageIndex = (m_nextActivePageIndex + 1) % m_activePages.Length;
	}

	protected override void OnPageTurnComplete(object callbackData, int operationId)
	{
		if (m_numPageFlipsThisSession % MERCS_COLLECTION_NUM_PAGE_FLIPS_UNTIL_UNLOAD_UNUSED_ASSETS == 0)
		{
			m_numPageFlipsThisSession++;
			HearthstoneApplication hearthstoneApplication = HearthstoneApplication.Get();
			if (hearthstoneApplication != null)
			{
				hearthstoneApplication.UnloadUnusedAssets();
			}
		}
		base.OnPageTurnComplete(callbackData, operationId);
		(CollectionManager.Get().GetCollectibleDisplay() as LettuceCollectionDisplay).TryShowCollectionTips();
	}

	protected override void OnCollectionManagerViewModeChanged(CollectionUtils.ViewMode prevMode, CollectionUtils.ViewMode mode, CollectionUtils.ViewModeData userdata, bool triggerResponse)
	{
		if (!triggerResponse)
		{
			return;
		}
		Log.CollectionManager.Print("transitionPageId={0} pagesTurning={1} mode={2}-->{3} triggerResponse={4}", m_transitionPageId, m_pagesCurrentlyTurning, prevMode, mode, triggerResponse);
		if (mode != 0)
		{
			CollectionDeckTray.Get().GetCardsContent().HideDeckHelpPopup();
		}
		m_currentPageNum = 1;
		if (userdata != null)
		{
			if (userdata.m_setPageByRole.HasValue)
			{
				m_roleCardsCollection.GetPageContentsForRole(userdata.m_setPageByRole.Value, 1, calculateCollectionPage: true, out m_currentPageNum);
			}
			else if (userdata.m_setPageByCard != null)
			{
				LettuceMercenary merc = CollectionManager.Get().GetMercenary(userdata.m_setPageByCard);
				m_roleCardsCollection.GetPageContentsForMercenary(merc, out m_currentPageNum);
			}
		}
		int prevId = 0;
		PageTransitionType transition = ((-prevId >= 0) ? PageTransitionType.SINGLE_PAGE_RIGHT : PageTransitionType.SINGLE_PAGE_LEFT);
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
		TransitionPageWhenReady(transition, useCurrentPageNum: true, callback, callbackData);
	}

	private HashSet<int> GetCurrentDeckTrayModeCardBackIds()
	{
		return CardBackManager.Get().GetCardBackIds(!CollectionManager.Get().IsInEditMode());
	}
}
