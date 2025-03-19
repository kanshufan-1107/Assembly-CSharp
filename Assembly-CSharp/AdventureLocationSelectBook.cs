using System.Collections;
using System.Collections.Generic;
using Assets;
using Blizzard.T5.Core;
using Blizzard.T5.MaterialService.Extensions;
using Hearthstone.DataModels;
using Hearthstone.Store;
using Hearthstone.UI;
using PegasusShared;
using UnityEngine;

[RequireComponent(typeof(WidgetTemplate))]
public class AdventureLocationSelectBook : MonoBehaviour
{
	public AdventureBookPageManager m_BookPageManager;

	public AsyncReference m_AdventureBookCoverReference;

	public Material m_anomalyModeCardHighlightMaterial;

	public float m_anomalyModeCardHideAnimTime = 0.25f;

	public float m_anomalyModeCardDriftScale = 2f;

	public float m_anomalyModeTooltipScale = 6f;

	private PlayButton m_playButton;

	private VisualController m_playButtonController;

	private Widget m_bookCover;

	private Widget m_anomalyModeButton;

	private Widget m_deckTrayWidget;

	private List<WingDbfRecord> m_wingRecords = new List<WingDbfRecord>();

	private Actor m_anomalyModeCardActor;

	private Transform m_anomalyModeCardSourceBone;

	private Transform m_anomalyModeCardBone;

	private bool m_anomalyModeCardShown;

	private bool m_justSawDungeonCrawlSubScene;

	private const string BOOK_COVER_OPEN_EVENT = "PlayBookCoverOpen";

	private const string ANOMALY_BUTTON_UNLOCKED_STATE = "UNLOCKED_ANOMALY";

	private const string ANOMALY_BUTTON_ACTIVATED_STATE = "ACTIVATED_ANOMALY";

	private const string ANOMALY_BUTTON_LOCKED_STATE = "LOCKED_ANOMALY";

	private const string PLAY_BUTTON_BURST_FX = "BURST";

	private const string ENABLE_INTERACTION_EVENT = "EnableInteraction";

	private const string DISABLE_INTERACTION_EVENT = "DisableInteraction";

	private const string SHOW_BOOK_COVER_EVENT = "ShowBookCover";

	private const string SHOW_ANOMALY_MODE_BIG_CARD_EVENT_NAME = "ShowAnomalyModeBigCard";

	private const string HIDE_ANOMALY_MODE_BIG_CARD_EVENT_NAME = "HideAnomalyModeBigCard";

	private static AdventureLocationSelectBook m_instance;

	private void Awake()
	{
		m_instance = this;
	}

	private void Start()
	{
		GetComponent<AdventureSubScene>().AddSubSceneTransitionFinishedListener(OnSubSceneTransitionFinished);
		GameMgr.Get().RegisterFindGameEvent(OnFindGameEvent);
		WidgetTemplate widget = GetComponent<WidgetTemplate>();
		widget.RegisterReadyListener(delegate
		{
			OnTopLevelWidgetReady(widget);
		});
		m_AdventureBookCoverReference.RegisterReadyListener<Widget>(OnBookCoverReady);
		m_BookPageManager.PageTurnStart += OnPageTurnStart;
		m_BookPageManager.PageTurnComplete += OnPageTurnComplete;
		m_BookPageManager.PageClicked += OnPageClicked;
		m_BookPageManager.SetEnableInteractionCallback(EnableInteraction);
		AdventureConfig.Get().AddAdventureMissionSetListener(OnMissionSet);
		StoreManager.Get().RegisterSuccessfulPurchaseAckListener(OnSuccessfulPurchaseAck);
		m_justSawDungeonCrawlSubScene = AdventureConfig.Get().PreviousSubScene == AdventureData.Adventuresubscene.DUNGEON_CRAWL;
		if (ShouldShowBookCoverOpeningAnim())
		{
			widget.TriggerEvent("ShowBookCover");
		}
		Navigation.PushUnique(OnNavigateBack);
		StartCoroutine(InitChapterDataWhenReady());
	}

	private void OnDestroy()
	{
		GameMgr.Get()?.UnregisterFindGameEvent(OnFindGameEvent);
		m_BookPageManager.PageTurnStart -= OnPageTurnStart;
		m_BookPageManager.PageTurnComplete -= OnPageTurnComplete;
		m_BookPageManager.PageClicked -= OnPageClicked;
		AdventureConfig.Get()?.RemoveAdventureMissionSetListener(OnMissionSet);
		StoreManager.Get()?.RemoveSuccessfulPurchaseAckListener(OnSuccessfulPurchaseAck);
		m_instance = null;
	}

	private void OnTopLevelWidgetReady(Widget topLevelWidget)
	{
		StartCoroutine(SetUpAdventureBookTrayOnceWidgetIsReady(topLevelWidget));
	}

	private IEnumerator SetUpAdventureBookTrayOnceWidgetIsReady(Widget topLevelWidget)
	{
		while (topLevelWidget.IsChangingStates)
		{
			yield return null;
		}
		AdventureBookDeckTray deckTray = topLevelWidget.GetComponentInChildren<AdventureBookDeckTray>(includeInactive: false);
		if (deckTray == null)
		{
			Error.AddDevWarning("UI Error!", "No AdventureBookDeckTray exists, or they're all hidden!");
		}
		else
		{
			SetUpAdventureBookTray(deckTray);
		}
	}

	private void OnSubSceneTransitionFinished()
	{
		StartCoroutine(StartAnimsWhenAllTransitionsComplete());
	}

	private IEnumerator StartAnimsWhenAllTransitionsComplete()
	{
		while (GameUtils.IsAnyTransitionActive() || PopupDisplayManager.Get().IsShowing)
		{
			yield return null;
		}
		m_BookPageManager.OnBookOpening();
		if (ShouldShowBookCoverOpeningAnim() && m_bookCover != null)
		{
			m_bookCover.TriggerEvent("PlayBookCoverOpen");
			Log.Adventures.Print("Waiting for Book Cover Opening animation to complete...");
		}
		else
		{
			AllInitialTransitionsComplete();
		}
	}

	private bool ShouldShowBookCoverOpeningAnim()
	{
		if (AdventureConfig.Get().PreviousSubScene == AdventureData.Adventuresubscene.CHOOSER)
		{
			return SceneMgr.Get().GetPrevMode() != SceneMgr.Mode.GAMEPLAY;
		}
		return false;
	}

	private void OnCoverOpened(Object callbackData)
	{
		if (m_BookPageManager == null)
		{
			Log.Adventures.PrintError("OnCoverOpen: m_BookPageManager was null!");
			return;
		}
		Log.Adventures.Print("Book Cover Opening animation now complete!");
		PageData pageData = m_BookPageManager.GetPageDataForCurrentPage();
		if (pageData != null && pageData.PageType == AdventureBookPageType.MAP)
		{
			DungeonCrawlSubDef_VOLines.VOEventType eventType = ((AdventureConfig.Get().GetSelectedMode() == AdventureModeDbId.DUNGEON_CRAWL_HEROIC) ? DungeonCrawlSubDef_VOLines.VOEventType.BOOK_REVEAL_HEROIC : DungeonCrawlSubDef_VOLines.VOEventType.BOOK_REVEAL);
			DungeonCrawlSubDef_VOLines.PlayVOLine(AdventureConfig.Get().GetSelectedAdventure(), WingDbId.INVALID, 0, eventType);
		}
		AllInitialTransitionsComplete();
	}

	private void AllInitialTransitionsComplete()
	{
		EnableInteraction(enable: true);
		m_BookPageManager.AllInitialTransitionsComplete();
	}

	private IEnumerator InitChapterDataWhenReady()
	{
		while (!m_BookPageManager.IsFullyLoaded())
		{
			yield return null;
		}
		while (m_playButton == null)
		{
			yield return null;
		}
		AdventureConfig ac = AdventureConfig.Get();
		AdventureDbId selectedAdv = ac.GetSelectedAdventure();
		AdventureModeDbId selectedMode = ac.GetSelectedMode();
		List<ScenarioDbfRecord> records = GameDbf.Scenario.GetRecords((ScenarioDbfRecord r) => r.AdventureId == (int)selectedAdv && r.ModeId == (int)selectedMode && r.WingId != 0);
		int numChapters = 0;
		Map<int, List<ChapterPageData>> chapterDataMap = new Map<int, List<ChapterPageData>>();
		foreach (ScenarioDbfRecord scenarioRecord in records)
		{
			ChapterPageData chapterPage = null;
			foreach (List<ChapterPageData> value in chapterDataMap.Values)
			{
				chapterPage = value.Find((ChapterPageData x) => x.WingRecord.ID == scenarioRecord.WingId);
				if (chapterPage != null)
				{
					break;
				}
			}
			if (chapterPage == null)
			{
				WingDbfRecord wingRec = GameDbf.Wing.GetRecord(scenarioRecord.WingId);
				if (wingRec == null)
				{
					Log.Adventures.PrintError("No Wing record found for ID {0}, referenced by Scenario {1}", scenarioRecord.WingId, scenarioRecord.ID);
					continue;
				}
				chapterPage = new ChapterPageData
				{
					Adventure = selectedAdv,
					AdventureMode = selectedMode,
					WingRecord = wingRec,
					BookSection = wingRec.BookSection
				};
				if (!chapterDataMap.ContainsKey(wingRec.BookSection))
				{
					chapterDataMap.Add(wingRec.BookSection, new List<ChapterPageData>());
				}
				chapterDataMap[wingRec.BookSection].Add(chapterPage);
				m_wingRecords.Add(wingRec);
				numChapters++;
			}
			chapterPage.ScenarioRecords.Add(scenarioRecord);
		}
		int numSectionsInBook = chapterDataMap.Count;
		List<List<ChapterPageData>> chapterDataBySection = new List<List<ChapterPageData>>();
		foreach (int key in chapterDataMap.Keys)
		{
			chapterDataBySection.Add(chapterDataMap[key]);
			foreach (ChapterPageData item in chapterDataMap[key])
			{
				item.ScenarioRecords.Sort(GameUtils.MissionSortComparison);
			}
		}
		chapterDataBySection.Sort(delegate(List<ChapterPageData> a, List<ChapterPageData> b)
		{
			if (a.Count < 1 || b.Count < 1)
			{
				Debug.LogError("AdventureLocationSelectBook: chapterDataBySection has a section with 0 chapters in it!");
				return 0;
			}
			return a[0].WingRecord.BookSection - b[0].WingRecord.BookSection;
		});
		foreach (List<ChapterPageData> item2 in chapterDataBySection)
		{
			item2.Sort((ChapterPageData a, ChapterPageData b) => a.WingRecord.SortOrder - b.WingRecord.SortOrder);
		}
		List<PageNode> pageNodeList = new List<PageNode>();
		bool includeMapPage = true;
		bool includeRewardPage = true;
		AdventureDataDbfRecord adventureDataRecord = GameUtils.GetAdventureDataRecord((int)selectedAdv, (int)selectedMode);
		if (adventureDataRecord != null)
		{
			if (adventureDataRecord.AdventureBookMapPageLocation == AdventureData.Adventurebooklocation.END)
			{
				Debug.LogErrorFormat("Adventure {0} and Mode {1} has the Map Page Location at END, but that is not yet supported by the code!", selectedAdv, selectedMode);
			}
			includeMapPage = adventureDataRecord.AdventureBookMapPageLocation != AdventureData.Adventurebooklocation.NOWHERE;
			if (adventureDataRecord.AdventureBookRewardPageLocation == AdventureData.Adventurebooklocation.BEGINNING)
			{
				Debug.LogErrorFormat("Adventure {0} and Mode {1} has the Reward Page Location at BEGINNING, but that is not yet supported by the code!", selectedAdv, selectedMode);
			}
			includeRewardPage = adventureDataRecord.AdventureBookRewardPageLocation != AdventureData.Adventurebooklocation.NOWHERE;
		}
		Map<int, ChapterPageData> chapterNumberToChapterDataMap = new Map<int, ChapterPageData>();
		PageNode mapPageNode = null;
		if (includeMapPage)
		{
			mapPageNode = new PageNode(new MapPageData
			{
				Adventure = selectedAdv,
				AdventureMode = selectedMode,
				NumSectionsInBook = numSectionsInBook,
				BookSection = -1,
				ChapterData = chapterNumberToChapterDataMap
			});
			pageNodeList.Add(mapPageNode);
		}
		List<List<PageNode>> chapterNodeBySection = new List<List<PageNode>>();
		int chapterNumber = 1;
		foreach (List<ChapterPageData> item3 in chapterDataBySection)
		{
			List<PageNode> chapterNodeList = new List<PageNode>();
			chapterNodeBySection.Add(chapterNodeList);
			foreach (ChapterPageData chapterPageData in item3)
			{
				chapterPageData.ChapterNumber = chapterNumber++;
				chapterNodeList.Add(new PageNode(chapterPageData));
				chapterNumberToChapterDataMap.Add(chapterPageData.ChapterNumber, chapterPageData);
			}
			pageNodeList.AddRange(chapterNodeList.ToArray());
		}
		UpdateRelationalChapterData(chapterNumberToChapterDataMap);
		List<PageNode> rewardNodeList = new List<PageNode>();
		if (includeRewardPage)
		{
			for (int i = 0; i < numSectionsInBook; i++)
			{
				PageNode rewardNode = new PageNode(new RewardPageData
				{
					Adventure = selectedAdv,
					AdventureMode = selectedMode,
					BookSection = i,
					ChapterData = chapterNumberToChapterDataMap
				});
				rewardNodeList.Add(rewardNode);
				pageNodeList.Add(rewardNodeList[i]);
			}
		}
		if (numSectionsInBook == 1 && pageNodeList.Count > 1 && mapPageNode != null && pageNodeList[0] == mapPageNode)
		{
			mapPageNode.PageToRight = chapterNodeBySection[0][0];
		}
		for (int section = 0; section < chapterNodeBySection.Count; section++)
		{
			List<PageNode> chapterNodeSection = chapterNodeBySection[section];
			for (int indexInSection = 0; indexInSection < chapterNodeSection.Count; indexInSection++)
			{
				PageNode pageNode = chapterNodeSection[indexInSection];
				if (indexInSection == 0)
				{
					pageNode.PageToLeft = mapPageNode;
				}
				else
				{
					pageNode.PageToLeft = chapterNodeSection[indexInSection - 1];
				}
				if (indexInSection == chapterNodeSection.Count - 1)
				{
					if (section < rewardNodeList.Count)
					{
						pageNode.PageToRight = rewardNodeList[section];
						rewardNodeList[section].PageToLeft = pageNode;
					}
					else
					{
						pageNode.PageToRight = null;
					}
				}
				else if (indexInSection + 1 < chapterNodeSection.Count)
				{
					pageNode.PageToRight = chapterNodeSection[indexInSection + 1];
				}
				else
				{
					Log.Adventures.PrintWarning("No page to set for PageToRight for Chapter index {0} in section {1}!", indexInSection, section);
				}
			}
		}
		m_BookPageManager.Initialize(pageNodeList, numChapters);
		while (m_BookPageManager.ArePagesTurning())
		{
			yield return null;
		}
		AdventureDbId currentSelectedAdventure = AdventureConfig.Get().GetAdventureDataModel().SelectedAdventure;
		while (AchieveManager.Get().HasActiveLicenseForAdventure(currentSelectedAdventure))
		{
			Log.Adventures.Print("Waiting on active license added achieves before entering the current Adventure subscene!");
			yield return null;
		}
		GetComponent<AdventureSubScene>().SetIsLoaded(loaded: true);
	}

	private void UpdateRelationalChapterData(Map<int, ChapterPageData> chapterNumberToChapterDataMap)
	{
		foreach (ChapterPageData chapterData in chapterNumberToChapterDataMap.Values)
		{
			foreach (ScenarioDbfRecord scenarioRecord in chapterData.ScenarioRecords)
			{
				int missionReqProgress = 0;
				int requiredWingId = 0;
				if (!AdventureConfig.GetMissionPlayableParameters(scenarioRecord.ID, ref requiredWingId, ref missionReqProgress) || requiredWingId == chapterData.WingRecord.ID)
				{
					continue;
				}
				foreach (ChapterPageData otherChapter in chapterNumberToChapterDataMap.Values)
				{
					if (otherChapter.WingRecord.ID == requiredWingId)
					{
						if (otherChapter.ChapterToFlipToWhenCompleted != 0)
						{
							Debug.LogWarningFormat("Chapter {0} already had a ChapterToFlipToWhenCompleted value of {1}, setting it to {2}!  Having scenarios from multiple wings that rely on the progress of a single wing is not currently supported!", otherChapter.ChapterNumber, otherChapter.ChapterToFlipToWhenCompleted, chapterData.ChapterNumber);
						}
						otherChapter.ChapterToFlipToWhenCompleted = chapterData.ChapterNumber;
						Log.Adventures.Print("ChapterToFlipToWhenCompleted for Chapter {0} set to Chapter {1}", otherChapter.ChapterNumber, chapterData.ChapterNumber);
						break;
					}
				}
			}
		}
	}

	private void SetUpAdventureBookTray(AdventureBookDeckTray deckTray)
	{
		if (deckTray == null || deckTray.m_PlayButtonReference == null || deckTray.m_BackButton == null)
		{
			Error.AddDevWarning("UI Error!", "DeckTray was not properly configured!");
			return;
		}
		m_deckTrayWidget = deckTray.GetComponent<Widget>();
		if (m_deckTrayWidget != null)
		{
			m_deckTrayWidget.RegisterEventListener(DeckTrayEventListener);
		}
		deckTray.m_PlayButtonReference.RegisterReadyListener<VisualController>(OnPlayButtonReady);
		deckTray.m_AnomalyModeButtonReference.RegisterReadyListener<Widget>(OnAnomalyModeButtonReady);
		deckTray.m_BackButton.AddEventListener(UIEventType.RELEASE, delegate
		{
			OnBackButtonPress();
		});
		m_anomalyModeCardSourceBone = deckTray.m_anomalyModeCardSourceBone;
		m_anomalyModeCardBone = deckTray.m_anomalyModeCardBone;
		LoadAnomalyModeCard();
	}

	private void OnPlayButtonReady(VisualController buttonVisualController)
	{
		if (buttonVisualController == null)
		{
			Error.AddDevWarning("UI Error!", "PlayButton could not be found! You will not be able to click 'Play'!");
			return;
		}
		m_playButtonController = buttonVisualController;
		m_playButton = buttonVisualController.gameObject.GetComponent<PlayButton>();
		m_playButton.AddEventListener(UIEventType.RELEASE, PlayButtonRelease);
		SetPlayButtonStateForCurrentPage(showBurst: false);
	}

	private void SetPlayButtonEnabled(bool enable)
	{
		if (!(m_playButton != null))
		{
			return;
		}
		if (enable)
		{
			if (!m_playButton.IsEnabled())
			{
				m_playButton.Enable();
			}
		}
		else if (m_playButton.IsEnabled())
		{
			m_playButton.Disable();
		}
	}

	private void OnAnomalyModeButtonReady(Widget button)
	{
		m_anomalyModeButton = button;
		if (button == null)
		{
			return;
		}
		Clickable clickable = m_anomalyModeButton.GetComponentInChildren<Clickable>();
		if (clickable == null)
		{
			Error.AddDevWarning("UI Error!", "Anomaly Mode Button has no Clickable!  Unable to attach listeners.");
			return;
		}
		clickable.AddEventListener(UIEventType.RELEASE, AnomalyModeButtonRelease);
		TooltipZone tooltipZone = m_anomalyModeButton.GetComponentInChildren<TooltipZone>();
		if (tooltipZone == null)
		{
			Error.AddDevWarning("UI Error!", "Anomaly Mode Button has no TooltipZone!  Unable to attach tooltip.");
			return;
		}
		clickable.AddEventListener(UIEventType.ROLLOVER, delegate
		{
			AdventureDbId selectedAdventure = AdventureConfig.Get().GetSelectedAdventure();
			AdventureModeDbId selectedMode = AdventureConfig.Get().GetSelectedMode();
			WingDbId wingIdFromMissionId = GameUtils.GetWingIdFromMissionId(AdventureConfig.Get().GetMission());
			if (wingIdFromMissionId != 0)
			{
				if (!AdventureUtils.IsAnomalyModeAllowed(wingIdFromMissionId))
				{
					tooltipZone.ShowTooltip(GameStrings.Get("GLUE_ADVENTURE_DUNGEON_CRAWL_ANOMALY_MODE_BUTTON_LOCKED_TOOLTIP_HEADER"), GameStrings.Get("GLUE_ADVENTURE_DUNGEON_CRAWL_ANOMALY_MODE_UNAVAILABLE_TOOLTIP_BODY"), m_anomalyModeTooltipScale);
				}
				else if (AdventureUtils.IsAnomalyModeLocked(selectedAdventure, selectedMode))
				{
					tooltipZone.ShowTooltip(GameStrings.Get("GLUE_ADVENTURE_DUNGEON_CRAWL_ANOMALY_MODE_BUTTON_LOCKED_TOOLTIP_HEADER"), GameStrings.Get("GLUE_ADVENTURE_DUNGEON_CRAWL_ANOMALY_MODE_BUTTON_LOCKED_TOOLTIP_BODY"), m_anomalyModeTooltipScale);
				}
			}
		});
		clickable.AddEventListener(UIEventType.ROLLOUT, delegate
		{
			tooltipZone.HideTooltip();
		});
	}

	private void OnBookCoverReady(Widget bookCover)
	{
		m_bookCover = bookCover;
		StartCoroutine(SetUpBookCoverReferencesWhenResolved(bookCover));
	}

	private IEnumerator SetUpBookCoverReferencesWhenResolved(Widget bookCover)
	{
		if (bookCover == null)
		{
			Error.AddDevWarning("UI Issue!", "m_AdventureBookCover is not hooked up on AdventureLocationSelectBook, so things won't load!");
			yield break;
		}
		while (bookCover.IsChangingStates)
		{
			yield return null;
		}
		AnimationEventDispatcher animationEventDispatcher = bookCover.GetComponentInChildren<AnimationEventDispatcher>();
		if (animationEventDispatcher != null)
		{
			animationEventDispatcher.RegisterAnimationEventListener(OnCoverOpened);
		}
	}

	public static bool OnNavigateBack()
	{
		if (m_instance == null)
		{
			Log.Adventures.PrintError("Trying to navigate back, but AdventureLocationSelectBook has been destroyed!");
			return false;
		}
		AdventureConfig.Get().SetMission(ScenarioDbId.INVALID);
		AdventureConfig.Get().SubSceneGoBack();
		AdventureBookPageManager bookPageManager = m_instance.m_BookPageManager;
		if (bookPageManager != null)
		{
			bookPageManager.HideAllPopups();
		}
		return true;
	}

	private void OnBackButtonPress()
	{
		Navigation.GoBack();
	}

	private void OnPageTurnStart(BookPageManager.PageTransitionType transitionType)
	{
		SetPlayButtonEnabled(enable: false);
		if (transitionType != 0)
		{
			AdventureConfig.Get().AnomalyModeActivated = false;
			AdventureConfig.Get().SetMission(ScenarioDbId.INVALID);
			ChapterPageData pageData = m_BookPageManager.GetPageDataForCurrentPage() as ChapterPageData;
			AdventureChapterState chapterState = AdventureChapterState.LOCKED;
			if (pageData != null && pageData.WingRecord != null)
			{
				chapterState = AdventureProgressMgr.Get().AdventureBookChapterStateForWing(pageData.WingRecord, pageData.AdventureMode);
			}
			if (pageData != null && chapterState != 0)
			{
				AdventureConfig.Get().SetHasSeenUnlockedChapterPage((WingDbId)pageData.WingRecord.ID, hasSeen: true);
			}
		}
	}

	private void OnPageTurnComplete(int currentPageNum)
	{
		AdventureBookPageDataModel adventurePageDataModel = m_BookPageManager.GetCurrentPageDataModel();
		if (m_deckTrayWidget != null && adventurePageDataModel != null)
		{
			m_deckTrayWidget.BindDataModel(adventurePageDataModel);
		}
		AdventureChapterDataModel chapterDataModel = adventurePageDataModel.ChapterData;
		ChapterPageData chapterPageData = m_BookPageManager.GetPageDataForCurrentPage() as ChapterPageData;
		AdventureDbfRecord adventureRecord = GameDbf.Adventure.GetRecord((int)AdventureConfig.Get().GetSelectedAdventure());
		if (chapterPageData != null && chapterPageData.PageType == AdventureBookPageType.CHAPTER && chapterDataModel.ChapterState != 0 && chapterPageData.WingRecord.PmtProductIdForSingleWingPurchase == 0 && AdventureConfig.Get().ShouldSeeFirstTimeFlow && adventureRecord != null && !adventureRecord.MapPageHasButtonsToChapters)
		{
			if (AdventureUtils.IsEntireAdventureFree((AdventureDbId)adventureRecord.ID))
			{
				if (chapterPageData.ChapterNumber == 1)
				{
					AdventureConfig.Get().MarkHasSeenFirstTimeFlowComplete();
				}
			}
			else
			{
				AdventureUtils.DisplayFirstChapterFreePopup(chapterPageData, OnFirstChapterFreePopupDisplayed);
			}
		}
		if (!m_justSawDungeonCrawlSubScene)
		{
			PlayPageSpecificVO();
		}
		m_justSawDungeonCrawlSubScene = false;
	}

	private void OnFirstChapterFreePopupDisplayed()
	{
		PlayPageSpecificVO();
	}

	private void OnMissionSet(ScenarioDbId mission, bool showDetails)
	{
		SetPlayButtonStateForCurrentPage(showBurst: true);
		if (m_deckTrayWidget != null)
		{
			ScenarioDbfRecord scenarioRecord = GameDbf.Scenario.GetRecord((int)mission);
			string bossCardId = GameUtils.GetMissionHeroCardId((int)mission);
			m_deckTrayWidget.GetDataModel(111, out var dataModel);
			HeroDataModel heroDataModel = dataModel as HeroDataModel;
			if (heroDataModel == null)
			{
				heroDataModel = new HeroDataModel();
				m_deckTrayWidget.BindDataModel(heroDataModel);
			}
			if (heroDataModel.HeroCard == null)
			{
				heroDataModel.HeroCard = new CardDataModel();
			}
			heroDataModel.HeroCard.CardId = bossCardId;
			string heroPowerId = GameUtils.GetMissionHeroPowerCardId((int)mission);
			if (heroDataModel.HeroPowerCard == null)
			{
				heroDataModel.HeroPowerCard = new CardDataModel();
			}
			heroDataModel.HeroPowerCard.CardId = heroPowerId;
			if (scenarioRecord == null)
			{
				heroDataModel.Name = null;
				heroDataModel.Description = null;
			}
			else
			{
				heroDataModel.Name = scenarioRecord.ShortName;
				heroDataModel.Description = (((bool)UniversalInputManager.UsePhoneUI && !string.IsNullOrEmpty(scenarioRecord.ShortDescription)) ? scenarioRecord.ShortDescription : scenarioRecord.Description);
			}
		}
		if (mission != 0 && AdventureProgressMgr.Get().CanPlayScenario((int)mission) && (SceneMgr.Get().GetPrevMode() != SceneMgr.Mode.GAMEPLAY || GameMgr.Get().GetPreviousMissionId() != (int)mission))
		{
			AdventureBossDef bossDef = AdventureConfig.Get().GetBossDef(mission);
			if (bossDef != null && bossDef.m_IntroLinePlayTime == AdventureBossDef.IntroLinePlayTime.MissionSelect)
			{
				AdventureUtils.PlayMissionQuote(bossDef, NotificationManager.DEFAULT_CHARACTER_POS);
			}
		}
	}

	private void OnSuccessfulPurchaseAck(ProductInfo bundle, PaymentMethod purchaseMethod)
	{
		if (AdventureUtils.DoesBundleIncludeWingForAdventure(bundle, AdventureConfig.Get().GetSelectedAdventure()))
		{
			PlayPageSpecificVO();
		}
	}

	private void BurstPlayButton()
	{
		if (!(m_playButton == null) && m_playButton.IsEnabled())
		{
			if (m_playButtonController == null)
			{
				Log.Adventures.PrintError("Attempting to burst Play Button, but m_playButtonController is null!");
			}
			else
			{
				m_playButtonController.Owner.TriggerEvent("BURST");
			}
		}
	}

	private void OnPageClicked()
	{
		BurstPlayButton();
	}

	private void SetPlayButtonStateForCurrentPage(bool showBurst)
	{
		if (PlayButtonShouldBeEnabled())
		{
			SetPlayButtonEnabled(enable: true);
			if (showBurst)
			{
				BurstPlayButton();
			}
		}
		else
		{
			SetPlayButtonEnabled(enable: false);
		}
	}

	private void PlayButtonRelease(UIEvent e)
	{
		SetPlayButtonEnabled(enable: false);
		if (!PlayButtonShouldBeEnabled())
		{
			Log.Adventures.PrintError("Play Button should be disabled, but you clicked it anyway!");
			return;
		}
		ScenarioDbId selectedMission = AdventureConfig.Get().GetMission();
		AdventureBossDef bossDef = AdventureConfig.Get().GetBossDef(selectedMission);
		if (bossDef != null && bossDef.m_IntroLinePlayTime == AdventureBossDef.IntroLinePlayTime.MissionStart)
		{
			AdventureUtils.PlayMissionQuote(bossDef, NotificationManager.DEFAULT_CHARACTER_POS);
		}
		if (AdventureConfig.DoesMissionRequireDeck(AdventureConfig.Get().GetMission()))
		{
			AdventureData.Adventuresubscene nextSubScene = ((!GameUtils.DoesAdventureModeUseDungeonCrawlFormat(AdventureConfig.Get().GetSelectedMode()) || AdventureConfig.Get().IsHeroSelectedBeforeDungeonCrawlScreenForSelectedAdventure()) ? AdventureConfig.Get().SubSceneForPickingHeroForCurrentAdventure() : AdventureData.Adventuresubscene.DUNGEON_CRAWL);
			AdventureConfig.Get().ChangeSubScene(nextSubScene);
		}
		else
		{
			GameMgr.Get().FindGame(GameType.GT_VS_AI, FormatType.FT_WILD, (int)AdventureConfig.Get().GetMissionToPlay(), 0, 0L, null, null, restoreSavedGameState: false, null, null, 0L);
		}
	}

	private bool PlayButtonShouldBeEnabled()
	{
		if (AdventureConfig.Get().ShouldSeeFirstTimeFlow)
		{
			return false;
		}
		ScenarioDbId currentMission = AdventureConfig.Get().GetMission();
		if (currentMission == ScenarioDbId.INVALID)
		{
			return false;
		}
		if (!AdventureProgressMgr.Get().CanPlayScenario((int)currentMission))
		{
			return false;
		}
		return true;
	}

	private void AnomalyModeButtonRelease(UIEvent e)
	{
		if (IsAnomalyModeAvailable())
		{
			AdventureConfig.Get().AnomalyModeActivated = !AdventureConfig.Get().AnomalyModeActivated;
		}
	}

	private bool IsAnomalyModeAvailable()
	{
		AdventureDbId selectedAdventure = AdventureConfig.Get().GetSelectedAdventure();
		AdventureModeDbId modeDbId = AdventureConfig.Get().GetSelectedMode();
		WingDbId selectedWingDbId = GameUtils.GetWingIdFromMissionId(AdventureConfig.Get().GetMission());
		return AdventureUtils.IsAnomalyModeAvailable(selectedAdventure, modeDbId, selectedWingDbId);
	}

	private void LoadAnomalyModeCard()
	{
		AdventureConfig adventureConfig = AdventureConfig.Get();
		AdventureDbId selectedAdv = adventureConfig.GetSelectedAdventure();
		AdventureModeDbId selectedMode = adventureConfig.GetSelectedMode();
		AdventureDataDbfRecord dataRecord = GameUtils.GetAdventureDataRecord((int)selectedAdv, (int)selectedMode);
		long anomalyCardDbId = 0L;
		if (dataRecord.GameSaveDataServerKey > 0)
		{
			GameSaveDataManager.Get().GetSubkeyValue((GameSaveKeyId)dataRecord.GameSaveDataServerKey, GameSaveKeySubkeyId.DUNGEON_CRAWL_ANOMALY_MODE_CARD_PREVIEW, out anomalyCardDbId);
		}
		if (anomalyCardDbId <= 0)
		{
			if (dataRecord.AnomalyModeDefaultCardId == 0)
			{
				return;
			}
			anomalyCardDbId = dataRecord.AnomalyModeDefaultCardId;
		}
		string anomalyCardId = GameUtils.TranslateDbIdToCardId((int)anomalyCardDbId);
		DefLoader.Get().LoadFullDef(anomalyCardId, OnAnomalyModeFullDefLoaded);
	}

	private void OnAnomalyModeFullDefLoaded(string cardId, DefLoader.DisposableFullDef fullDef, object userData)
	{
		if (fullDef == null)
		{
			Debug.LogWarningFormat("OnAnomalyModeFullDefLoaded: No FullDef found for cardId {0}!", cardId);
		}
		else
		{
			AssetLoader.Get().InstantiatePrefab(ActorNames.GetHandActor(fullDef.EntityDef, TAG_PREMIUM.NORMAL), OnAnomalyModeActorLoaded, fullDef, AssetLoadingOptions.IgnorePrefabPosition);
		}
	}

	private void OnAnomalyModeActorLoaded(AssetReference assetRef, GameObject go, object callbackData)
	{
		DefLoader.DisposableFullDef fullDef = callbackData as DefLoader.DisposableFullDef;
		using (fullDef)
		{
			Actor actor = (m_anomalyModeCardActor = go.GetComponent<Actor>());
			if (fullDef == null)
			{
				Debug.LogWarning("OnAnomalyModeActorLoaded: no FullDef passed in!");
				return;
			}
			if (actor == null)
			{
				Debug.LogWarningFormat("OnAnomalyModeActorLoaded: actor \"{0}\" has no Actor component", assetRef);
				return;
			}
			GameUtils.SetParent(actor, base.gameObject);
			LayerUtils.SetLayer(actor, base.gameObject.layer);
			actor.TurnOffCollider();
			actor.SetFullDef(fullDef);
			actor.UpdateAllComponents();
			actor.SetUnlit();
			actor.Hide();
		}
	}

	private void DeckTrayEventListener(string eventName)
	{
		if (eventName.Equals("ShowAnomalyModeBigCard") && IsAnomalyModeAvailable())
		{
			ShowAnomalyModeBigCard();
		}
		else if (eventName.Equals("HideAnomalyModeBigCard"))
		{
			HideAnomalyModeBigCard();
		}
	}

	private void ShowAnomalyModeBigCard()
	{
		if (m_anomalyModeCardActor == null)
		{
			Debug.LogWarning("ShowAnomalyModeBigCard: m_anomalyModeCardActor is not loaded!");
		}
		else if (!m_anomalyModeCardShown)
		{
			m_anomalyModeCardShown = true;
			iTween.Stop(m_anomalyModeCardActor.gameObject);
			m_anomalyModeCardActor.Show();
			HighlightRender highlight = m_anomalyModeCardActor.GetComponentInChildren<HighlightRender>();
			MeshRenderer meshRenderer = ((highlight != null) ? highlight.GetComponent<MeshRenderer>() : null);
			if (meshRenderer != null && m_anomalyModeCardHighlightMaterial != null)
			{
				meshRenderer.SetSharedMaterial(m_anomalyModeCardHighlightMaterial);
				meshRenderer.enabled = true;
			}
			m_anomalyModeCardActor.gameObject.transform.position = m_anomalyModeCardBone.position;
			m_anomalyModeCardActor.gameObject.transform.localScale = m_anomalyModeCardBone.localScale;
			AnimationUtil.GrowThenDrift(m_anomalyModeCardActor.gameObject, m_anomalyModeCardSourceBone.position, m_anomalyModeCardDriftScale);
		}
	}

	private void HideAnomalyModeBigCard()
	{
		if (m_anomalyModeCardActor == null)
		{
			Debug.LogWarning("ShowAnomalyModeBigCard: m_anomalyModeCardActor is not loaded!");
		}
		else if (m_anomalyModeCardShown)
		{
			m_anomalyModeCardShown = false;
			iTween.Stop(m_anomalyModeCardActor.gameObject);
			Hashtable moveArgs = iTweenManager.Get().GetTweenHashTable();
			moveArgs.Add("position", m_anomalyModeCardSourceBone.position);
			moveArgs.Add("time", m_anomalyModeCardHideAnimTime);
			moveArgs.Add("easetype", iTween.EaseType.easeOutQuart);
			iTween.MoveTo(m_anomalyModeCardActor.gameObject, moveArgs);
			Hashtable scaleArgs = iTweenManager.Get().GetTweenHashTable();
			scaleArgs.Add("scale", Vector3.one * 0.05f);
			scaleArgs.Add("time", m_anomalyModeCardHideAnimTime);
			scaleArgs.Add("oncomplete", "AnomalyModeCardShrinkComplete");
			scaleArgs.Add("oncompletetarget", base.gameObject);
			iTween.ScaleTo(m_anomalyModeCardActor.gameObject, scaleArgs);
		}
	}

	private void AnomalyModeCardShrinkComplete()
	{
		m_anomalyModeCardActor.Hide();
	}

	private void EnableInteraction(bool enable)
	{
		Widget widget = GetComponent<Widget>();
		if (widget == null)
		{
			Error.AddDevWarning("UI Issue!", "The component AdventureLocationSelectBook is not attached to a Widget!");
		}
		else
		{
			widget.TriggerEvent(enable ? "EnableInteraction" : "DisableInteraction");
		}
	}

	private void PlayPageSpecificVO()
	{
		PageData pageData = m_BookPageManager.GetPageDataForCurrentPage();
		if (pageData == null)
		{
			Log.Adventures.PrintWarning("AdventureBookPageManager.PlayPageSpecificVO was called but no page data exists.");
			return;
		}
		AdventureDbId adventureId = AdventureConfig.Get().GetSelectedAdventure();
		AdventureModeDbId modeId = AdventureConfig.Get().GetSelectedMode();
		DungeonCrawlSubDef_VOLines.VOEventType eventType = DungeonCrawlSubDef_VOLines.VOEventType.INVALID;
		WingDbfRecord wingRecord = ((pageData is ChapterPageData chapterData) ? chapterData.WingRecord : null);
		AdventureChapterState chapterState = ((wingRecord != null) ? AdventureProgressMgr.Get().AdventureBookChapterStateForWing(wingRecord, modeId) : AdventureChapterState.LOCKED);
		WingDbId wingId = (WingDbId)(wingRecord?.ID ?? 0);
		StopCoroutine("PlayChapterPageQuoteAfterDelay");
		switch (pageData.PageType)
		{
		case AdventureBookPageType.CHAPTER:
		{
			int numChaptersOwned = m_BookPageManager.GetNumChaptersOwned();
			if (chapterState == AdventureChapterState.LOCKED || AdventureConfig.Get().ShouldSeeFirstTimeFlow)
			{
				eventType = DungeonCrawlSubDef_VOLines.GetNextValidEventType(adventureId, wingId, 0, new DungeonCrawlSubDef_VOLines.VOEventType[1] { DungeonCrawlSubDef_VOLines.VOEventType.WING_UNLOCK });
			}
			if (eventType == DungeonCrawlSubDef_VOLines.VOEventType.INVALID && numChaptersOwned == m_BookPageManager.NumChapters)
			{
				eventType = DungeonCrawlSubDef_VOLines.GetNextValidEventType(adventureId, wingId, 0, new DungeonCrawlSubDef_VOLines.VOEventType[1] { DungeonCrawlSubDef_VOLines.VOEventType.ANOMALY_UNLOCK });
			}
			if (eventType == DungeonCrawlSubDef_VOLines.VOEventType.INVALID && !AdventureConfig.Get().ShouldSeeFirstTimeFlow && chapterState == AdventureChapterState.LOCKED)
			{
				eventType = DungeonCrawlSubDef_VOLines.VOEventType.CALL_TO_ACTION;
			}
			if (eventType == DungeonCrawlSubDef_VOLines.VOEventType.INVALID && !AdventureConfig.Get().ShouldSeeFirstTimeFlow && chapterState != 0)
			{
				AdventureMission.WingProgress wingProgress = AdventureProgressMgr.Get().GetProgress((int)wingId);
				if (wingProgress != null && wingProgress.IsOwned())
				{
					StartCoroutine("PlayChapterPageQuoteAfterDelay", AdventureScene.Get().GetWingDef(wingId));
					return;
				}
			}
			break;
		}
		case AdventureBookPageType.REWARD:
			eventType = DungeonCrawlSubDef_VOLines.GetNextValidEventType(adventureId, WingDbId.INVALID, 0, new DungeonCrawlSubDef_VOLines.VOEventType[1] { DungeonCrawlSubDef_VOLines.VOEventType.REWARD_PAGE_REVEAL });
			break;
		}
		if (eventType != 0)
		{
			DungeonCrawlSubDef_VOLines.PlayVOLine(AdventureConfig.Get().GetSelectedAdventure(), wingId, 0, eventType);
		}
	}

	private IEnumerator PlayChapterPageQuoteAfterDelay(AdventureWingDef wingDef)
	{
		if (wingDef == null)
		{
			Debug.LogError("AdventureLocationSelectBook.PlayChapterPageQuoteAfterDelay() called with no AdventureWingDef passed in!");
			yield break;
		}
		yield return new WaitForSeconds(wingDef.m_OpenQuoteDelay);
		if (NotificationManager.Get().IsQuotePlaying)
		{
			if (AdventureUtils.CanPlayWingOpenQuote(wingDef))
			{
				NotificationManager.Get().ForceAddSoundToPlayedList(wingDef.m_OpenQuoteVOLine);
			}
			yield break;
		}
		WingDbId wingId = wingDef.GetWingId();
		ScenarioDbId lastSelectedMission = AdventureConfig.Get().GetMission();
		if (wingId != 0)
		{
			if (DungeonCrawlSubDef_VOLines.PlayVOLine(AdventureConfig.Get().GetSelectedAdventure(), wingId, 0, DungeonCrawlSubDef_VOLines.VOEventType.CHAPTER_PAGE))
			{
				while (NotificationManager.Get().IsQuotePlaying)
				{
					yield return null;
				}
			}
			else if (SceneMgr.Get().GetPrevMode() == SceneMgr.Mode.GAMEPLAY && lastSelectedMission != 0 && !AdventureProgressMgr.Get().HasDefeatedScenario((int)lastSelectedMission))
			{
				DungeonCrawlSubDef_VOLines.VOEventType voEventType = (((m_BookPageManager.GetPageDataForCurrentPage() as ChapterPageData).BookSection == 0) ? DungeonCrawlSubDef_VOLines.VOEventType.BOSS_LOSS_1 : DungeonCrawlSubDef_VOLines.VOEventType.BOSS_LOSS_1_SECOND_BOOK_SECTION);
				if (DungeonCrawlSubDef_VOLines.PlayVOLine(AdventureConfig.Get().GetSelectedAdventure(), wingId, 0, voEventType))
				{
					yield break;
				}
			}
		}
		if (AdventureUtils.CanPlayWingOpenQuote(wingDef))
		{
			string gameString = new AssetReference(wingDef.m_OpenQuoteVOLine).GetLegacyAssetName();
			NotificationManager.Get().CreateCharacterQuote(wingDef.m_OpenQuotePrefab, GameStrings.Get(gameString), wingDef.m_OpenQuoteVOLine, allowRepeatDuringSession: false);
		}
	}

	private bool OnFindGameEvent(FindGameEventData eventData, object userData)
	{
		FindGameState state = eventData.m_state;
		if ((uint)(state - 2) <= 1u || (uint)(state - 7) <= 1u || state == FindGameState.SERVER_GAME_CANCELED)
		{
			HandleGameStartupFailure();
		}
		return false;
	}

	private void HandleGameStartupFailure()
	{
		SetPlayButtonEnabled(PlayButtonShouldBeEnabled());
	}
}
