using System;
using System.Linq;
using Assets;
using Blizzard.T5.Services;
using Blizzard.Telemetry.WTCG.Client;
using Hearthstone.DataModels;
using Hearthstone.Store;
using Hearthstone.UI;
using UnityEngine;

namespace Hearthstone.Progression;

[RequireComponent(typeof(WidgetTemplate))]
public class JournalPopup : MonoBehaviour
{
	public static bool s_isShowing;

	[SerializeField]
	private Global.RewardTrackType m_rewardTrackType = Global.RewardTrackType.GLOBAL;

	[SerializeField]
	private int m_eventComingSoonDays;

	[SerializeField]
	private WidgetInstance m_tavernGuideButton;

	public const string ACHIEVEMENT_SELECTED = "ACHIEVEMENT_SELECTED";

	public const string REWARD_SELECTED = "REWARD_SELECTED";

	public const string TAVERN_GUIDE_SELECTED = "TAVERN_GUIDE_SELECTED";

	public const string CODE_SHOW_SEASON_PASS = "CODE_SHOW_SEASON_PASS";

	public const string CODE_SHOW_APPRENTICE_TAVERN_PASS_POPUP = "CODE_SHOW_APPRENTICE_TAVERN_PASS_POPUP";

	public const string SHOW_APPRENTICE_TAVERN_PASS_POPUP = "SHOW_APPRENTICE_TAVERN_PASS_POPUP";

	public const string CODE_SKIP_APPRENTICE = "CODE_SKIP_APPRENTICE";

	public const string CODE_UPDATE_JOURNAL_META = "CODE_UPDATE_JOURNAL_META";

	public const string CODE_CLOSE = "CODE_CLOSE";

	public const string CODE_MARK_WELCOME_SEEN = "MARK_WELCOME_SEEN";

	private const string GLUE_APPRENTICE_INNKEEPER_TAVERN_GUIDE_INTRO = "GLUE_PROGRESSION_APPRENTICE_TUTORIALS_INNKEEEPER_LEARN_ROPES";

	private const string GLUE_APPRENTICE_INNKEEPER_BONUS_XP = "GLUE_PROGRESSION_APPRENTICE_TUTORIALS_INNKEEEPER_BONUS_XP";

	private Widget m_widget;

	private JournalMetaDataModel m_journalMetaDatamodel;

	private GameObject m_owner;

	private Enum[] m_presencePrevStatus;

	private Renderer m_tavernGuideArrow;

	protected virtual void Awake()
	{
		m_widget = GetComponent<WidgetTemplate>();
		m_widget.RegisterEventListener(HandleEvent);
		if (m_rewardTrackType == Global.RewardTrackType.BATTLEGROUNDS)
		{
			RewardTrack rewardTrack = RewardTrackManager.Get().GetRewardTrack(m_rewardTrackType);
			if (rewardTrack == null || !rewardTrack.IsValid)
			{
				Debug.LogError("JournalPopup: no RewardTrackType.BATTLEGROUNDS reward track found.");
			}
			else
			{
				m_widget.BindDataModel(rewardTrack.TrackDataModel);
			}
		}
		BindCurrentSpecialEventDatamodel();
		m_widget.BindDataModel(AchievementManager.Get().Categories);
		m_owner = base.gameObject;
		if (base.transform.parent != null && base.transform.parent.GetComponent<WidgetInstance>() != null)
		{
			m_owner = base.transform.parent.gameObject;
		}
		if (m_tavernGuideButton != null)
		{
			m_tavernGuideButton.RegisterReadyListener(delegate
			{
				Renderer[] componentsInChildren = m_tavernGuideButton.GetComponentsInChildren<Renderer>(includeInactive: true);
				m_tavernGuideArrow = componentsInChildren.FirstOrDefault((Renderer r) => r.name == "Arrow");
			});
		}
		SceneMgr.Get().RegisterScenePreLoadEvent(OnPreLoadNextScene);
		FatalErrorMgr.Get().AddErrorListener(OnFatalError);
	}

	private void Update()
	{
		if (m_tavernGuideArrow != null && m_tavernGuideArrow.gameObject.activeSelf && m_tavernGuideArrow.gameObject.layer != 31 && !m_widget.IsChangingStates)
		{
			m_tavernGuideArrow.gameObject.layer = 31;
		}
	}

	private void HandleEvent(string eventName)
	{
		switch (eventName)
		{
		case "ACHIEVEMENT_SELECTED":
			CollectionManager.Get().StartInitialMercenaryLoadIfRequired();
			break;
		case "TAVERN_GUIDE_SELECTED":
			if (Network.IsLoggedIn())
			{
				HandleMarkTavernGuideSeen();
				HandleShowTavernQuestsIntro();
			}
			else
			{
				ProgressUtils.ShowOfflinePopup();
			}
			break;
		case "CODE_SHOW_SEASON_PASS":
			OpenSeasonPassProductPage((RewardTrackManager.Get().GetRewardTrack(m_rewardTrackType)?.RewardTrackAsset?.SeasonPassProductId).GetValueOrDefault(), eventName);
			break;
		case "CODE_SHOW_APPRENTICE_TAVERN_PASS_POPUP":
			if (Network.IsLoggedIn())
			{
				HandleShowingApprenticeTavernPassInfo();
			}
			else
			{
				ProgressUtils.ShowOfflinePopup();
			}
			break;
		case "CODE_SKIP_APPRENTICE":
			DialogManager.Get().ShowLeaguePromoteSelfManuallyDialog(delegate
			{
				if (!Network.IsLoggedIn())
				{
					DialogManager.Get().ShowReconnectHelperDialog(RequestSkipApprentice);
				}
				else
				{
					RequestSkipApprentice();
				}
			});
			break;
		case "CODE_UPDATE_JOURNAL_META":
			UpdateJournalMeta();
			break;
		case "CODE_CLOSE":
			Close();
			break;
		case "MARK_WELCOME_SEEN":
			MarkWelcomeApprenticeAsSeen();
			break;
		}
	}

	private void OnDestroy()
	{
		s_isShowing = false;
		Navigation.RemoveHandler(OnNavigateBack);
		if (PresenceMgr.IsInitialized())
		{
			PresenceMgr.Get().SetStatus(m_presencePrevStatus);
		}
		UIContext.GetRoot().DismissPopup(m_owner);
		if (SceneMgr.IsInitialized())
		{
			SceneMgr.Get().UnregisterScenePreLoadEvent(OnPreLoadNextScene);
		}
		if (FatalErrorMgr.IsInitialized())
		{
			FatalErrorMgr.Get().RemoveErrorListener(OnFatalError);
		}
	}

	public virtual void Show()
	{
		s_isShowing = true;
		HearthstonePerformance.Get()?.StartPerformanceFlow(new FlowPerformance.SetupConfig
		{
			FlowType = Blizzard.Telemetry.WTCG.Client.FlowPerformance.FlowType.JOURNAL
		});
		Navigation.Push(OnNavigateBack);
		m_presencePrevStatus = PresenceMgr.Get().GetStatus();
		PresenceMgr.Get().SetStatus(Global.PresenceStatus.VIEWING_JOURNAL);
		m_widget.RegisterDoneChangingStatesListener(delegate
		{
			MusicManager.Get().StartPlaylist(MusicPlaylistType.UI_Journal);
			OverlayUI.Get().AddGameObject(m_owner);
			UIContext.GetRoot().ShowPopup(m_owner);
			m_widget.TriggerEvent("SHOW");
			if (m_journalMetaDatamodel != null)
			{
				m_journalMetaDatamodel.DoneChangingTabs = true;
			}
		}, null, callImmediatelyIfSet: true, doOnce: true);
	}

	public virtual void Close()
	{
		if (m_rewardTrackType == Global.RewardTrackType.BATTLEGROUNDS)
		{
			MusicManager.Get()?.StartPlaylist(MusicPlaylistType.UI_Battlegrounds);
		}
		else
		{
			Box box = Box.Get();
			if (box != null)
			{
				box.PlayBoxMusic();
			}
		}
		HearthstonePerformance.Get()?.StopCurrentFlow();
		UnityEngine.Object.Destroy(m_owner);
	}

	public void UpdateJournalMeta()
	{
		if (m_journalMetaDatamodel == null)
		{
			m_journalMetaDatamodel = m_widget.GetDataModel<JournalMetaDataModel>();
		}
		Global.RewardTrackType trackType = ((!GameUtils.HasCompletedApprentice() && RewardTrackManager.Get().IsApprenticeTrackReady()) ? Global.RewardTrackType.APPRENTICE : m_rewardTrackType);
		RewardTrackManager.Get().UpdateJournalMetaWithRewardTrack(m_journalMetaDatamodel, trackType);
		if (m_rewardTrackType == Global.RewardTrackType.GLOBAL)
		{
			SpecialEventManager.Get().UpdateJournalMetaWithSpecialEvent(m_journalMetaDatamodel);
			BindCurrentSpecialEventDatamodel();
		}
		TavernGuideManager.Get().UpdateJournalMetaForTavernGuide(m_journalMetaDatamodel);
	}

	private void MarkWelcomeApprenticeAsSeen()
	{
		if (m_journalMetaDatamodel == null)
		{
			m_journalMetaDatamodel = m_widget.GetDataModel<JournalMetaDataModel>();
		}
		m_journalMetaDatamodel.HasSeenWelcomeApprentice = true;
		GameSaveDataManager.Get().SaveSubkey(new GameSaveDataManager.SubkeySaveRequest(GameSaveKeyId.FTUE, GameSaveKeySubkeyId.FTUE_HAS_SEEN_WELCOME_APPRENTICE, 1L));
	}

	public TimeSpan GetEventComingSoonTimespan()
	{
		return new TimeSpan(m_eventComingSoonDays, 0, 0, 0);
	}

	private void OpenSeasonPassProductPage(int productId, string eventName)
	{
		if (Network.IsLoggedIn())
		{
			if (m_rewardTrackType == Global.RewardTrackType.BATTLEGROUNDS)
			{
				BaconDisplay.Get()?.OpenBattlegroundsShop();
				return;
			}
			RewardTrackManager.Get().GetRewardTrack(m_rewardTrackType);
			Log.Store.Print("[JournalPopup::HandleEvent] " + eventName + " - Network ONLINE");
			ProductPageJobs.OpenToSeasonPassPageWhenReady(m_rewardTrackType, productId);
		}
		else
		{
			Log.Store.Print("[JournalPopup::HandleEvent] " + eventName + " - Network OFFLINE");
			ProgressUtils.ShowOfflinePopup();
		}
	}

	private void BindCurrentSpecialEventDatamodel()
	{
		if (m_widget.GetDataModel<SpecialEventDataModel>() == null)
		{
			SpecialEventManager specialEventManager = SpecialEventManager.Get();
			SpecialEventDataModel specialEventDataModel = specialEventManager.GetEventDataModelForCurrentEvent();
			if (specialEventDataModel == null && m_eventComingSoonDays > 0)
			{
				TimeSpan timeSpanToCheck = GetEventComingSoonTimespan();
				SpecialEventDbfRecord upcomingEvent = specialEventManager.GetUpcomingSpecialEvent(timeSpanToCheck);
				specialEventDataModel = specialEventManager.GetEventDataModelFromSpecialEvent(upcomingEvent);
			}
			if (specialEventDataModel != null)
			{
				m_widget.BindDataModel(specialEventDataModel);
			}
		}
	}

	private void HandleShowingApprenticeTavernPassInfo()
	{
		if (StoreManager.Get() == null || !ServiceManager.TryGet<IProductDataService>(out var dataService))
		{
			Log.All.PrintError("[JournalPopup] Cannot open Tavern Pass info popup because shop services are unavailable");
			return;
		}
		RewardTrack rewardTrack = RewardTrackManager.Get().GetRewardTrack(Global.RewardTrackType.APPRENTICE);
		if (rewardTrack.RewardTrackAsset == null)
		{
			Log.All.PrintError("[JournalPopup] Cannot show Tavern Pass info because RewardTrack is invalid");
			return;
		}
		if (rewardTrack.RewardTrackAsset.SeasonPassProductId == 0)
		{
			Log.All.PrintError("[JournalPopup] Cannot open Tavern Pass info because its product is missing");
			return;
		}
		ProductDataModel productDataModel = dataService.GetProductDataModel(rewardTrack.RewardTrackAsset.SeasonPassProductId);
		if (productDataModel == null)
		{
			Log.All.PrintError("[JournalPopup] Cannot open Tavern Pass info because its product is missing");
		}
		else
		{
			m_widget.TriggerEvent("SHOW_APPRENTICE_TAVERN_PASS_POPUP", new TriggerEventParameters(ToString(), productDataModel, noDownwardPropagation: true));
		}
	}

	private void RequestSkipApprentice()
	{
		RankMgr.Get().RequestSkipApprentice();
		Close();
	}

	private void HandleMarkTavernGuideSeen()
	{
		GameSaveDataManager.Get().GetSubkeyValue(GameSaveKeyId.FTUE, GameSaveKeySubkeyId.FTUE_HAS_SEEN_TAVERN_GUIDE_INTRODUCTION, out long hasSeenTavernGuide);
		if (hasSeenTavernGuide != 1)
		{
			NotificationManager.Get().CreateInnkeeperQuote(UserAttentionBlocker.NONE, NotificationManager.DEFAULT_CHARACTER_POS, GameStrings.Get("GLUE_PROGRESSION_APPRENTICE_TUTORIALS_INNKEEEPER_LEARN_ROPES"), "");
			GameSaveDataManager.Get().SaveSubkey(new GameSaveDataManager.SubkeySaveRequest(GameSaveKeyId.FTUE, GameSaveKeySubkeyId.FTUE_HAS_SEEN_TAVERN_GUIDE_INTRODUCTION, 1L));
			if (m_journalMetaDatamodel == null)
			{
				m_journalMetaDatamodel = m_widget.GetDataModel<JournalMetaDataModel>();
			}
			TavernGuideManager.Get().UpdateJournalMetaForTavernGuide(m_journalMetaDatamodel);
			Box box = Box.Get();
			if (box != null && box.GetRailroadManager() != null)
			{
				box.GetRailroadManager().UpdateRailroadingOnBox();
			}
		}
	}

	private void HandleShowTavernQuestsIntro()
	{
		if (TavernGuideManager.Get() != null && TavernGuideManager.Get().IsTavernGuideActive() && TavernGuideManager.Get().CanShowAllQuestSets())
		{
			GameSaveDataManager.Get().GetSubkeyValue(GameSaveKeyId.FTUE, GameSaveKeySubkeyId.FTUE_HAS_SEEN_COMPLETE_QUESTS_BRASS_RING, out long hasSeenCompleteQuestsDialogue);
			if (hasSeenCompleteQuestsDialogue <= 0)
			{
				NotificationManager.Get().CreateInnkeeperQuote(UserAttentionBlocker.NONE, NotificationManager.DEFAULT_CHARACTER_POS, GameStrings.Get("GLUE_PROGRESSION_APPRENTICE_TUTORIALS_INNKEEEPER_BONUS_XP"), "");
				GameSaveDataManager.Get().SaveSubkey(new GameSaveDataManager.SubkeySaveRequest(GameSaveKeyId.FTUE, GameSaveKeySubkeyId.FTUE_HAS_SEEN_COMPLETE_QUESTS_BRASS_RING, 1L));
			}
		}
	}

	private bool OnNavigateBack()
	{
		m_widget.TriggerEvent("HIDE");
		return true;
	}

	private void OnPreLoadNextScene(SceneMgr.Mode prevMode, SceneMgr.Mode mode, object userData)
	{
		Close();
	}

	private void OnFatalError(FatalErrorMessage message, object userData)
	{
		Close();
	}
}
