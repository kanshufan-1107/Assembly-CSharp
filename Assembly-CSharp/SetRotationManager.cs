using System;
using System.Collections;
using System.Collections.Generic;
using Assets;
using Blizzard.T5.Jobs;
using Blizzard.T5.Services;
using Hearthstone;
using Hearthstone.Core;
using Hearthstone.Progression;
using Hearthstone.Streaming;
using UnityEngine;

public class SetRotationManager : IService
{
	private bool? m_currentSetRotationActive;

	public EventTimingType CurrentSetRotationEvent { get; private set; }

	public bool IsShowingSetRotationRelogPopup { get; private set; }

	private int CurrentSetRotationYear
	{
		get
		{
			EventTimingManager eventTimingManager = EventTimingManager.Get();
			return eventTimingManager.GetEventStartTimeUtc(CurrentSetRotationEvent).GetValueOrDefault().AddSeconds(eventTimingManager.DevTimeOffsetSeconds)
				.Year;
		}
	}

	public IEnumerator<IAsyncJobResult> Initialize(ServiceLocator serviceLocator)
	{
		serviceLocator.Get<EventTimingManager>().OnReceivedEventTimingsFromServer += OnReceivedEventTimingsFromServer;
		yield break;
	}

	public Type[] GetDependencies()
	{
		return new Type[3]
		{
			typeof(EventTimingManager),
			typeof(GameDownloadManager),
			typeof(RewardTrackManager)
		};
	}

	public void Shutdown()
	{
	}

	public static SetRotationManager Get()
	{
		return ServiceManager.Get<SetRotationManager>();
	}

	public static bool HasSeenStandardModeTutorial()
	{
		return Options.Get().GetBool(Option.HAS_SEEN_STANDARD_MODE_TUTORIAL, defaultVal: false);
	}

	public bool ShowNewPlayerSetRotationPopupIfNeeded()
	{
		if (Options.Get().GetInt(Option.SET_ROTATION_INTRO_PROGRESS_NEW_PLAYER, 0) >= GetActiveSetRotationYear())
		{
			return false;
		}
		if (!RankMgr.Get().IsNewPlayer())
		{
			return false;
		}
		if (!CollectionManager.Get().AccountHasRotatedBoosters(DateTime.UtcNow) && !CollectionManager.Get().AccountHasWildCards())
		{
			return false;
		}
		if (GameSaveDataManager.Get().IsDataReady(GameSaveKeyId.FTUE) && !GameUtils.IsGSDFlagSet(GameSaveKeyId.FTUE, GameSaveKeySubkeyId.FTUE_HAS_SEEN_WELCOME_APPRENTICE))
		{
			Hearthstone.Progression.RewardTrack apprenticeTrack = RewardTrackManager.Get().GetRewardTrack(Assets.Achievement.RewardTrackType.APPRENTICE);
			bool num = apprenticeTrack != null && apprenticeTrack.IsActive && apprenticeTrack.TrackDataModel.TotalXp == 0;
			bool isInNewPlayerRailroading = PopupDisplayManager.SuppressPopupsForNewPlayer;
			if (num || isInNewPlayerRailroading)
			{
				Options.Get().SetInt(Option.SET_ROTATION_INTRO_PROGRESS_NEW_PLAYER, GetActiveSetRotationYear());
				return false;
			}
		}
		BasicPopup.PopupInfo info = new BasicPopup.PopupInfo();
		info.m_prefabAssetRefs.Add("SetRotationNewPlayerPopup.prefab:ed707c931e185924eab67aa36770f8ec");
		info.m_blurWhenShown = true;
		info.m_responseCallback = delegate
		{
			SetRotationIntroProgress();
		};
		DialogManager.Get().ShowBasicPopup(UserAttentionBlocker.NONE, info);
		return true;
	}

	public bool ShouldShowSetRotationIntro()
	{
		if (IsShowingSetRotationRelogPopup)
		{
			return false;
		}
		if (!EventTimingManager.Get().HasReceivedEventTimingsFromServer)
		{
			return false;
		}
		if (Options.Get().GetInt(Option.SET_ROTATION_INTRO_PROGRESS, 0) == GetActiveSetRotationYear() && HasSeenStandardModeTutorial())
		{
			return false;
		}
		if (!GameDownloadManagerProvider.Get().IsReadyToPlay)
		{
			return false;
		}
		if (!GameUtils.HasCompletedApprentice())
		{
			return false;
		}
		CollectionManager collectionMgr = CollectionManager.Get();
		if (collectionMgr == null)
		{
			Debug.LogWarning("ShouldShowSetRotationIntro: CollectionManager is NULL!");
			return false;
		}
		if (!collectionMgr.ShouldAccountSeeStandardWild())
		{
			return false;
		}
		if (Cheat_AutoCompleteSetRotationIntro())
		{
			return false;
		}
		return true;
	}

	public bool CheckForSetRotationRollover()
	{
		if (!m_currentSetRotationActive.HasValue)
		{
			return false;
		}
		if (m_currentSetRotationActive.Value)
		{
			return false;
		}
		if (SceneMgr.Get() == null || SceneMgr.Get().IsInGame())
		{
			return false;
		}
		if (!IsThisYearsSetRotationEventActive())
		{
			return false;
		}
		if (ServiceManager.TryGet<GameMgr>(out var gameMgr) && gameMgr.IsFindingGame())
		{
			GameMgr.Get().CancelFindGame();
		}
		Log.All.Print("Set Rotation has just occurred!  Forcing the client to restart.");
		AlertPopup.PopupInfo info = new AlertPopup.PopupInfo();
		info.m_headerText = GameStrings.Get("GLOBAL_SET_ROTATION_ROLLOVER_HEADER");
		info.m_text = GameStrings.Get(HearthstoneApplication.AllowResetFromFatalError ? "GLOBAL_SET_ROTATION_ROLLOVER_BODY_MOBILE" : "GLOBAL_SET_ROTATION_ROLLOVER_BODY_DESKTOP");
		info.m_alertTextAlignment = UberText.AlignmentOptions.Center;
		info.m_showAlertIcon = true;
		info.m_disableBnetBar = true;
		info.m_blurWhenShown = true;
		info.m_responseDisplay = AlertPopup.ResponseDisplay.OK;
		info.m_responseCallback = delegate
		{
			if ((bool)HearthstoneApplication.AllowResetFromFatalError)
			{
				HearthstoneApplication.Get().Reset();
			}
			else
			{
				HearthstoneApplication.Get().Exit();
			}
		};
		DialogManager.Get().ShowPopup(info);
		IsShowingSetRotationRelogPopup = true;
		m_currentSetRotationActive = true;
		return true;
	}

	public void SetRotationIntroProgress()
	{
		Options.Get().SetInt(Option.SET_ROTATION_INTRO_PROGRESS, GetActiveSetRotationYear());
		Options.Get().SetInt(Option.SET_ROTATION_INTRO_PROGRESS_NEW_PLAYER, GetActiveSetRotationYear());
	}

	public int GetActiveSetRotationYear()
	{
		if (!EventTimingManager.Get().IsEventActive(CurrentSetRotationEvent))
		{
			return CurrentSetRotationYear - 1;
		}
		return CurrentSetRotationYear;
	}

	public string GetActiveSetRotationYearLocalizedString()
	{
		if (GetActiveSetRotationYear() % 2 != 0)
		{
			return GameStrings.Get("GLUE_SET_ROTATION_ODD_YEAR");
		}
		return GameStrings.Get("GLUE_SET_ROTATION_EVEN_YEAR");
	}

	private IEnumerator PollForSetRotationRollover(float interval)
	{
		while (!CheckForSetRotationRollover())
		{
			yield return new WaitForSeconds(interval);
		}
	}

	private void OnThisYearsSetRotationEventAdded(object userData)
	{
		m_currentSetRotationActive = IsThisYearsSetRotationEventActive();
		if (!m_currentSetRotationActive.Value)
		{
			Processor.RunCoroutine(PollForSetRotationRollover(1f));
		}
	}

	private void FindCurrentSetRotationEvent()
	{
		EventTimingType? firstSetRotationOfThisYearEvent = null;
		DateTime? firstSetRotationOfThisYearStartDate = null;
		EventTimingType? lastYearsLatestSetRotationEvent = null;
		DateTime? lastYearsLatestSetRotationStartDate = null;
		long secondsOffset = EventTimingManager.Get().DevTimeOffsetSeconds;
		DateTime currentServerTime = DateTime.Now.AddSeconds(secondsOffset);
		foreach (SetRotationEventDbfRecord record in GameDbf.SetRotationEvent.GetRecords())
		{
			EventTimingType eventType = record.EventTiming;
			DateTime? startDate = EventTimingManager.Get().GetEventStartTimeUtc(eventType)?.AddSeconds(secondsOffset);
			if (startDate.HasValue && startDate.Value.Year == currentServerTime.Year && (!firstSetRotationOfThisYearStartDate.HasValue || startDate < firstSetRotationOfThisYearStartDate.Value))
			{
				firstSetRotationOfThisYearStartDate = startDate;
				firstSetRotationOfThisYearEvent = eventType;
			}
			if (startDate.HasValue && startDate.Value.Year == currentServerTime.Year - 1 && (!lastYearsLatestSetRotationStartDate.HasValue || startDate > lastYearsLatestSetRotationStartDate.Value))
			{
				lastYearsLatestSetRotationStartDate = startDate;
				lastYearsLatestSetRotationEvent = eventType;
			}
		}
		if (firstSetRotationOfThisYearEvent.HasValue)
		{
			CurrentSetRotationEvent = firstSetRotationOfThisYearEvent.Value;
		}
		else if (lastYearsLatestSetRotationEvent.HasValue)
		{
			CurrentSetRotationEvent = lastYearsLatestSetRotationEvent.Value;
		}
		else
		{
			Debug.LogWarning("Unable to find either first content launch event of year, or, latest content launch event");
		}
	}

	private void OnReceivedEventTimingsFromServer()
	{
		FindCurrentSetRotationEvent();
		EventTimingManager.Get().AddEventAddedListener(OnThisYearsSetRotationEventAdded, CurrentSetRotationEvent);
	}

	public bool IsThisYearsSetRotationEventActive()
	{
		return EventTimingManager.Get().IsEventActive(CurrentSetRotationEvent);
	}

	public bool Cheat_AutoCompleteSetRotationIntro()
	{
		if (!HearthstoneApplication.IsInternal())
		{
			return false;
		}
		if (!Options.Get().GetBool(Option.DISABLE_SET_ROTATION_INTRO, defaultVal: false))
		{
			return false;
		}
		SetRotationIntroProgress();
		Options.Get().SetBool(Option.HAS_SEEN_STANDARD_MODE_TUTORIAL, val: true);
		string message = "Set Rotation intro skipped due to disableSetRotationIntro=true";
		UIStatus.Get().AddInfo(message, 10f);
		return true;
	}
}
