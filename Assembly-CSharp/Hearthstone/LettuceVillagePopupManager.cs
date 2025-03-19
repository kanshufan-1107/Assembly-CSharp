using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets;
using Blizzard.T5.Services;
using Hearthstone.InGameMessage;
using Hearthstone.InGameMessage.UI;
using Hearthstone.UI;
using PegasusLettuce;
using PegasusShared;
using UnityEngine;

namespace Hearthstone;

public class LettuceVillagePopupManager : MonoBehaviour
{
	public enum PopupType
	{
		INVALID,
		TASKBOARD,
		PVE,
		WORKSHOP,
		MAILBOX,
		TRAININGHALL,
		RENOWNCONVERSION
	}

	public const string OPEN_TASKS_EVENT = "OPEN_TASKS";

	public const string TASK_CLICKED_EVENT = "TASK_CLICKED";

	public const string OPEN_ZONE_PORTAL_EVENT = "OPEN_ZONE_PORTAL";

	public const string OPEN_WORKSHOP_EVENT = "OPEN_WORKSHOP";

	public const string OPEN_TRAINING_HALL_EVENT = "OPEN_TRAINING_HALL";

	public const string OPEN_MAILBOX_EVENT = "OPEN_MAILBOX";

	public const string OPEN_TASK_DETAIL = "OPEN_TASK_DETAIL";

	public const string OPEN_RENOWN_CONVERSION = "OPEN_RENOWN_CONVERSION";

	public const string CLOSE_TASK_DETAIL = "CLOSE_TASK_DETAIL";

	public const string SHRINK_TASK_DETAIL = "SHRINK_TASK_DETAIL";

	public const string SHOW_TASK_REWARD = "SHOW_TASK_REWARD";

	public const string TRANSITION_SCENE = "TRANSITION_SCENE";

	public const string HIDE_REQUEST = "TRY_HIDE";

	public const string SHRINK_ALL = "SHRINK_POPUPS";

	public const string HIDE_ALL = "HIDE_POPUPS";

	public const string PLAY_CURRENT_BARKVO = "PLAY_CURRENT_BARKVO";

	public const string RECHECK_POPUPS = "RECHECK_POPUPS";

	[SerializeField]
	private VisualController m_visualController;

	[SerializeField]
	private AsyncReference m_zonePortalReference;

	[SerializeField]
	private AsyncReference m_trainingHallReference;

	[SerializeField]
	private AsyncReference m_taskBoardReference;

	[SerializeField]
	private AsyncReference m_workshopReference;

	[SerializeField]
	private AsyncReference m_renownConversionReference;

	private static LettuceVillagePopupManager s_instance;

	private Widget m_widget;

	private LettuceVillageDisplay m_parentDisplay;

	private bool m_workshopIsVisible;

	private LettuceVillageZonePortal m_zonePortal;

	private LettuceVillageTrainingHall m_trainingHall;

	private LettuceVillageTaskBoardManager m_taskBoard;

	private LettuceVillageWorkshop m_workshop;

	private LettuceVillageRenownConversion m_renownConversion;

	private Enum[] m_prevPresence;

	private MusicPlaylistBookmark m_musicPlaylistBookmark;

	private GameObject m_currentPopup;

	private PopupType m_requestedPopup;

	public Action<PopupType> OnPopupShown { get; set; }

	public Action<PopupType> OnPopupClosed { get; set; }

	public bool VillageIsReady { get; set; }

	public int FocusedVisitorId { get; set; }

	public PopupType CurrentlyOpenPopup { get; private set; }

	public PopupType PreviouslyOpenPopup { get; private set; }

	public static LettuceVillagePopupManager Get()
	{
		return s_instance;
	}

	private void Awake()
	{
		s_instance = this;
		m_widget = GetComponent<Widget>();
		m_parentDisplay = GetComponentInParent<LettuceVillageDisplay>();
	}

	private void Start()
	{
		m_zonePortalReference.RegisterReadyListener<LettuceVillageZonePortal>(OnZonePortalReady);
		m_trainingHallReference.RegisterReadyListener<LettuceVillageTrainingHall>(OnTrainingHallReady);
		m_taskBoardReference.RegisterReadyListener<LettuceVillageTaskBoardManager>(OnTaskBoardReady);
		m_workshopReference.RegisterReadyListener<LettuceVillageWorkshop>(OnWorkshopReady);
		m_renownConversionReference.RegisterReadyListener<LettuceVillageRenownConversion>(OnRenownVendorReady);
		m_widget.RegisterEventListener(HandleEvent);
		if (SceneMgr.Get().GetMode() == SceneMgr.Mode.LETTUCE_VILLAGE && SceneMgr.Get().GetPrevMode() == SceneMgr.Mode.HUB)
		{
			StartCoroutine(DisplayToasts());
		}
	}

	private void OnDestroy()
	{
		if (s_instance == this)
		{
			s_instance = null;
		}
	}

	public LettuceVillageDisplay GetDisplay()
	{
		return m_parentDisplay;
	}

	public LettuceVillageTaskBoardManager GetTaskBoardManager()
	{
		return m_taskBoard;
	}

	public void Show(PopupType popupType, bool force = false)
	{
		if (popupType == PopupType.INVALID)
		{
			Log.Lettuce.PrintError("Cannot show village popup with Invalid type");
		}
		else if (CurrentlyOpenPopup != popupType)
		{
			if (CurrentlyOpenPopup == PopupType.INVALID)
			{
				InternalShow(popupType);
				return;
			}
			Hide(CurrentlyOpenPopup, force);
			m_requestedPopup = popupType;
		}
	}

	public void Hide(PopupType popupType, bool force = false)
	{
		if (force || CanHidePopup(popupType))
		{
			if (popupType == PopupType.INVALID)
			{
				Log.Lettuce.PrintError("Cannot hide village popup with Invalid type");
			}
			else if (CurrentlyOpenPopup == popupType)
			{
				m_requestedPopup = PopupType.INVALID;
				InternalHide(CurrentlyOpenPopup);
			}
		}
	}

	public bool IsOpen(PopupType popupType)
	{
		return CurrentlyOpenPopup == popupType;
	}

	private void InternalShow(PopupType popupType)
	{
		m_visualController.RegisterDoneChangingStatesListener(delegate
		{
			OnShowComplete(popupType);
		}, null, callImmediatelyIfSet: true, doOnce: true);
		bool success = false;
		switch (popupType)
		{
		case PopupType.TASKBOARD:
			success = InternalOpenTaskboard();
			break;
		case PopupType.PVE:
			if (!m_zonePortal.TryNavigatingDirectlyToMap())
			{
				success = InternalOpenPVE();
			}
			break;
		case PopupType.WORKSHOP:
			success = InternalOpenWorkshop();
			break;
		case PopupType.MAILBOX:
			success = InternalOpenMailbox();
			break;
		case PopupType.TRAININGHALL:
			success = InternalTrainingHall();
			break;
		case PopupType.RENOWNCONVERSION:
			success = InternalOpenRenownConversion();
			break;
		default:
			Log.Lettuce.PrintError($"Cannot show village popup with invalid type - {popupType}");
			return;
		}
		if (success)
		{
			if (CurrentlyOpenPopup != 0 && CurrentlyOpenPopup != PreviouslyOpenPopup)
			{
				PreviouslyOpenPopup = CurrentlyOpenPopup;
			}
			CurrentlyOpenPopup = popupType;
		}
	}

	private void OnShowComplete(PopupType popupType)
	{
		OnPopupShown?.Invoke(popupType);
	}

	private void InternalHide(PopupType popupType)
	{
		HideHelpPopupsInVillage();
		RestorePresenceAndMusic();
		BnetBar.Get().RefreshCurrency();
		switch (popupType)
		{
		case PopupType.TASKBOARD:
			InternalHideTaskBoard();
			m_visualController.SetState("SHRINK_POPUPS");
			break;
		case PopupType.PVE:
		case PopupType.WORKSHOP:
		case PopupType.TRAININGHALL:
		case PopupType.RENOWNCONVERSION:
			m_visualController.SetState("SHRINK_POPUPS");
			break;
		case PopupType.MAILBOX:
			m_visualController.SetState("HIDE_POPUPS");
			PopupDisplayManager.Get().SetVillageMailboxMessageShouldShow(show: false);
			OnHideComplete(popupType);
			break;
		}
	}

	private void OnHideComplete(PopupType popupType)
	{
		if (CurrentlyOpenPopup != 0 && CurrentlyOpenPopup != PreviouslyOpenPopup)
		{
			PreviouslyOpenPopup = CurrentlyOpenPopup;
		}
		CurrentlyOpenPopup = PopupType.INVALID;
		OnPopupClosed?.Invoke(popupType);
		if (m_currentPopup != null)
		{
			UIContext.GetRoot().DismissPopup(m_currentPopup);
			m_currentPopup = null;
		}
		switch (popupType)
		{
		case PopupType.TASKBOARD:
			m_taskBoard.ResetPopup();
			break;
		}
		if (m_requestedPopup != 0)
		{
			InternalShow(m_requestedPopup);
			m_requestedPopup = PopupType.INVALID;
		}
	}

	private bool CanHidePopup(PopupType popupType)
	{
		switch (popupType)
		{
		case PopupType.TASKBOARD:
			if (m_taskBoard != null && !m_taskBoard.IsProcessing)
			{
				return !m_taskBoard.IsShowingRewards;
			}
			return false;
		case PopupType.PVE:
			return m_zonePortal != null;
		case PopupType.WORKSHOP:
			return m_workshop != null;
		case PopupType.MAILBOX:
			return true;
		case PopupType.TRAININGHALL:
			if (m_trainingHall != null)
			{
				return !m_trainingHall.IsProcessingUpdate();
			}
			return false;
		case PopupType.RENOWNCONVERSION:
			if (m_renownConversion != null)
			{
				return !m_renownConversion.IsProcessing;
			}
			return false;
		default:
			return true;
		}
	}

	private bool InternalOpenTaskboard()
	{
		m_taskBoard.TryFocusVisitor(FocusedVisitorId);
		FocusedVisitorId = 0;
		m_visualController.SetState("OPEN_TASKS");
		m_currentPopup = UIContext.GetRoot().ShowPopup(m_taskBoard.gameObject).gameObject;
		m_taskBoard.HandleTutorialPromptsOnOpen();
		SetPresenceAndMusic(PopupType.TASKBOARD);
		BnetBar.Get()?.RefreshCurrency();
		return true;
	}

	private bool InternalOpenPVE()
	{
		NetCache.ProfileNoticeMercenariesRewards rewardNotice = PopupDisplayManager.Get().RewardPopups.GetNextMercenariesRewardToShow((NetCache.ProfileNoticeMercenariesRewards notice) => notice.RewardType == ProfileNoticeMercenariesRewards.RewardType.REWARD_TYPE_PVE_AUTO_RETIRE);
		if (!PopupDisplayManager.Get().RewardPopups.ShowMercenariesRewards(autoOpenChest: true, rewardNotice, null, OnReadyToOpen))
		{
			OnReadyToOpen();
		}
		return true;
		void OnReadyToOpen()
		{
			if (NetCache.Get().GetNetObject<NetCache.NetCacheFeatures>().Games.MercenariesAI)
			{
				m_visualController.SetState("OPEN_ZONE_PORTAL");
				m_currentPopup = UIContext.GetRoot().ShowPopup(m_zonePortal.gameObject).gameObject;
				SetPresenceAndMusic(PopupType.PVE);
			}
		}
	}

	private bool InternalOpenWorkshop()
	{
		m_visualController.SetState("OPEN_WORKSHOP");
		m_currentPopup = UIContext.GetRoot().ShowPopup(m_workshop.gameObject).gameObject;
		SetPresenceAndMusic(PopupType.WORKSHOP);
		return true;
	}

	private bool InternalOpenMailbox()
	{
		if (ServiceManager.TryGet<MessagePopupDisplay>(out var popupDisplay))
		{
			PopupDisplayManager.Get().SetVillageMailboxMessageShouldShow(show: true);
			popupDisplay.TriggerEvent(PopupEvent.OnMercInbox, MercenaryMessageUtils.GetEmptyMailboxMessage());
			popupDisplay.RegisterOnDialogInstanceClosedAction(delegate
			{
				InternalHide(PopupType.MAILBOX);
			});
			m_visualController.SetState("OPEN_MAILBOX");
			SetPresenceAndMusic(PopupType.MAILBOX);
			return true;
		}
		return false;
	}

	private bool InternalTrainingHall()
	{
		m_visualController.SetState("OPEN_TRAINING_HALL");
		m_currentPopup = UIContext.GetRoot().ShowPopup(m_trainingHall.gameObject).gameObject;
		SetPresenceAndMusic(PopupType.TRAININGHALL);
		return true;
	}

	private bool InternalOpenRenownConversion()
	{
		m_visualController.SetState("OPEN_RENOWN_CONVERSION");
		m_currentPopup = UIContext.GetRoot().ShowPopup(m_renownConversion.gameObject).gameObject;
		SetPresenceAndMusic(PopupType.RENOWNCONVERSION);
		LettuceTutorialUtils.ForceCompleteEvent(LettuceTutorialVo.LettuceTutorialEvent.VILLAGE_TUTORIAL_RENOWN_POPUP);
		BnetBar.Get()?.RefreshCurrency();
		return true;
	}

	private bool InternalHideTaskBoard()
	{
		if (LettuceVillageDataUtil.RecentlyClaimedTaskId == 28035)
		{
			LettuceVillageDataUtil.RecentlyClaimedTaskId = 0;
			UIContext.GetRoot().DismissPopup(m_currentPopup);
			LettuceTutorialUtils.FireEvent(LettuceTutorialVo.LettuceTutorialEvent.VILLAGE_TUTORIAL_SHOP_BUILD_START, GetTutorialGameObject());
		}
		else if (LettuceTutorialUtils.FireEvent(LettuceTutorialVo.LettuceTutorialEvent.VILLAGE_TUTORIAL_PVE_BUILD_START, GetTutorialGameObject()))
		{
			m_parentDisplay.GetVillage()?.UpdateBuildingStates();
		}
		return true;
	}

	private void HandleEvent(string eventName)
	{
		switch (eventName)
		{
		case "TRY_HIDE":
			Hide(CurrentlyOpenPopup);
			break;
		case "HIDE_POPUPS":
			OnHideComplete(CurrentlyOpenPopup);
			break;
		case "RECHECK_POPUPS":
			if (CurrentlyOpenPopup == PopupType.PVE)
			{
				m_zonePortal.CheckForAndMaybeShowFanfare();
			}
			break;
		}
	}

	private void OnZonePortalReady(LettuceVillageZonePortal obj)
	{
		m_zonePortal = obj;
	}

	private void OnTrainingHallReady(LettuceVillageTrainingHall obj)
	{
		m_trainingHall = obj;
	}

	private void OnTaskBoardReady(LettuceVillageTaskBoardManager obj)
	{
		m_taskBoard = obj;
	}

	private void OnWorkshopReady(LettuceVillageWorkshop obj)
	{
		m_workshop = obj;
	}

	private void OnRenownVendorReady(LettuceVillageRenownConversion obj)
	{
		m_renownConversion = obj;
	}

	private void SetPresenceAndMusic(PopupType popupType)
	{
		Global.PresenceStatus presence = Global.PresenceStatus.UNKNOWN;
		MusicPlaylistType playlist = MusicPlaylistType.Invalid;
		switch (popupType)
		{
		case PopupType.TASKBOARD:
			presence = Global.PresenceStatus.MERCENARIES_VILLAGE_TASKBOARD;
			playlist = MusicPlaylistType.UI_MercenariesTaskboard;
			break;
		case PopupType.PVE:
			presence = Global.PresenceStatus.MERCENARIES_VILLAGE_PVE_ZONES;
			playlist = MusicPlaylistType.UI_MercenariesZoneSelection;
			break;
		case PopupType.WORKSHOP:
			presence = Global.PresenceStatus.MERCENARIES_VILLAGE_BUILDING_MANAGER;
			playlist = MusicPlaylistType.UI_MercenariesWorkshop;
			break;
		case PopupType.MAILBOX:
			presence = Global.PresenceStatus.MERCENARIES_VILLAGE_MAILBOX;
			playlist = MusicPlaylistType.UI_MercenariesMailbox;
			break;
		case PopupType.RENOWNCONVERSION:
			presence = Global.PresenceStatus.MERCENARIES_VILLAGE_RENOWN_CONVERSION;
			playlist = MusicPlaylistType.UI_MercenariesRenownConversion;
			break;
		default:
			Log.Lettuce.PrintError($"Cannot set Presence/Music for unknown popup type - {popupType}");
			break;
		case PopupType.TRAININGHALL:
			break;
		}
		if (playlist != 0)
		{
			MusicManager musicMgr = MusicManager.Get();
			if (musicMgr != null)
			{
				m_musicPlaylistBookmark = musicMgr.CreateBookmarkOfCurrentPlaylist();
				musicMgr.StartPlaylist(playlist);
			}
		}
		if (presence != Global.PresenceStatus.UNKNOWN)
		{
			PresenceMgr presenceMgr = PresenceMgr.Get();
			if (presenceMgr != null)
			{
				m_prevPresence = presenceMgr.GetStatus();
				presenceMgr.SetStatus(presence);
			}
		}
	}

	private void RestorePresenceAndMusic()
	{
		MusicManager musicMgr = MusicManager.Get();
		if (musicMgr != null)
		{
			if (m_musicPlaylistBookmark == null || m_musicPlaylistBookmark.m_currentTrack == null)
			{
				musicMgr.StartPlaylist(MusicPlaylistType.UI_MercenariesVillage);
			}
			else
			{
				musicMgr.StopPlaylist();
				musicMgr.PlayFromBookmark(m_musicPlaylistBookmark);
				m_musicPlaylistBookmark = null;
			}
		}
		PresenceMgr presenceMgr = PresenceMgr.Get();
		if (presenceMgr != null)
		{
			if (m_prevPresence == null)
			{
				presenceMgr.SetStatus(Global.PresenceStatus.MERCENARIES_VILLAGE);
			}
			else
			{
				presenceMgr.SetStatus(m_prevPresence);
				m_prevPresence = null;
			}
		}
	}

	public GameObject GetTutorialGameObject()
	{
		if (m_parentDisplay != null && m_parentDisplay.GetVillage() != null)
		{
			return m_parentDisplay.GetVillage().gameObject;
		}
		return base.gameObject;
	}

	public void HideHelpPopupsInVillage()
	{
		if (m_parentDisplay != null && m_parentDisplay.GetVillage() != null)
		{
			m_parentDisplay.GetVillage().HideHelpPopups();
		}
	}

	private IEnumerator DisplayToasts()
	{
		NetCache.NetCacheMercenariesVillageVisitorInfo visitorInfo = NetCache.Get().GetNetObject<NetCache.NetCacheMercenariesVillageVisitorInfo>();
		if (visitorInfo != null && visitorInfo.CompletedTasks != null && visitorInfo.CompletedTasks.Count > 0)
		{
			while (!VillageIsReady)
			{
				yield return null;
			}
			List<MercenariesTaskState> copyTasks = visitorInfo.CompletedTasks.ToList();
			visitorInfo.CompletedTasks.Clear();
			yield return LettuceVillageDataUtil.ShowTaskToast(copyTasks);
		}
	}
}
