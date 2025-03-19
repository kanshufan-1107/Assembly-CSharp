using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets;
using Hearthstone.DataModels;
using Hearthstone.UI;
using UnityEngine;

public class LettuceBountyBoardDisplay : AbsSceneDisplay
{
	public int m_bountiesPerPage = 6;

	public WidgetTemplate m_widgetTemplate;

	public PlayMakerFSM[] m_fsms;

	public AsyncReference m_PlayButtonReference;

	public AsyncReference m_PlayButtonPhoneReference;

	public AsyncReference m_BackButtonReference;

	public AsyncReference m_BackButtonPhoneReference;

	public AsyncReference[] m_BountyBoardDisplays;

	public AsyncReference m_mythicScalerPopupReference;

	public AsyncReference m_bossRushPopupReference;

	private PlayButton m_playButton;

	private UIBButton m_backButton;

	private bool m_playButtonFinishedLoading;

	private bool m_backButtonFinishedLoading;

	private int m_bountyBoardDisplayFinishedLoadingCount;

	private bool m_mythicScalerFinishedLoading;

	private bool m_bossRushTutorialFinishedLoading;

	private LettuceBountyBoardDataModel m_bountyBoardDataModel = new LettuceBountyBoardDataModel();

	private static int m_lastSelectedBountySetIdThisSession;

	private static int m_lastSelectedBountyRecordIdThisSession;

	private List<DefLoader.DisposableCardDef> m_loadedCardDefs = new List<DefLoader.DisposableCardDef>();

	private List<DefLoader.DisposableFullDef> m_loadedBountyRewardFullDefs = new List<DefLoader.DisposableFullDef>();

	private List<int> m_BountyIdsToAcknowledge = new List<int>();

	private Dictionary<int, NetCache.NetCacheMercenariesPlayerInfo.BountyInfo> m_bountyInfos = new Dictionary<int, NetCache.NetCacheMercenariesPlayerInfo.BountyInfo>();

	private GameObject m_currentPopup;

	private LettuceBountyMythicLevelSelector m_mythicScalerPopup;

	private WidgetInstance m_bossRushTutorialPopup;

	public bool m_initialized;

	public int CurrentMythicLevel
	{
		get
		{
			return m_bountyBoardDataModel.CurrentMythicLevel;
		}
		private set
		{
			if (value != m_bountyBoardDataModel.CurrentMythicLevel)
			{
				m_bountyBoardDataModel.CurrentMythicLevel = value;
			}
		}
	}

	public override void Start()
	{
		base.Start();
		if ((bool)UniversalInputManager.UsePhoneUI)
		{
			m_PlayButtonPhoneReference.RegisterReadyListener<PlayButton>(OnPlayButtonReady);
			m_BackButtonPhoneReference.RegisterReadyListener<UIBButton>(OnBackButtonReady);
		}
		else
		{
			m_PlayButtonReference.RegisterReadyListener<PlayButton>(OnPlayButtonReady);
			m_BackButtonReference.RegisterReadyListener<UIBButton>(OnBackButtonReady);
		}
		AsyncReference[] bountyBoardDisplays = m_BountyBoardDisplays;
		for (int i = 0; i < bountyBoardDisplays.Length; i++)
		{
			bountyBoardDisplays[i].RegisterReadyListener<VisualController>(OnBountyBoardDisplayReady);
		}
		m_mythicScalerPopupReference.RegisterReadyListener<LettuceBountyMythicLevelSelector>(OnMythicLevelScalerReady);
		m_bossRushPopupReference.RegisterReadyListener<WidgetInstance>(OnBossRushTutorialPopupReady);
		NetCache.Get().RegisterUpdatedListener(typeof(NetCache.NetCacheMercenariesPlayerInfo), UpdateMercenariesPlayerInfo);
		MusicManager.Get().StartPlaylist(MusicPlaylistType.UI_MercenariesSubMenus);
		StartCoroutine(InitializeWhenReady());
	}

	private void Update()
	{
		if (m_initialized)
		{
			LettuceVillageDataUtil.RefreshIfPassedWeeklyGeneratedBountyReset();
		}
	}

	private void OnDestroy()
	{
		NetCache.Get()?.RemoveUpdatedListener(typeof(NetCache.NetCacheMercenariesPlayerInfo), UpdateMercenariesPlayerInfo);
		foreach (DefLoader.DisposableCardDef loadedCardDef in m_loadedCardDefs)
		{
			loadedCardDef.Dispose();
		}
		foreach (DefLoader.DisposableFullDef loadedBountyRewardFullDef in m_loadedBountyRewardFullDefs)
		{
			loadedBountyRewardFullDef.Dispose();
		}
	}

	private void BountyBoardEventListener(string eventName)
	{
		switch (eventName)
		{
		case "BOUNTY_RELEASED":
			OnBountySelected();
			break;
		case "PAGE_NEXT":
			if (m_bountyBoardDataModel.PageIndex < m_bountyBoardDataModel.PageCount - 1)
			{
				int currentIndex = m_bountyBoardDataModel.PageIndex++;
				m_fsms[currentIndex].FsmVariables.FindFsmBool("PlayAudioOnSlide").Value = m_slidingTray.m_playAudioOnSlide;
				m_fsms[currentIndex].SendEvent("Death");
				AcknowledgeBountiesOnCurrentPage();
			}
			break;
		case "PAGE_PREV":
			if (m_bountyBoardDataModel.PageIndex > 0)
			{
				LettuceBountyBoardDataModel bountyBoardDataModel = m_bountyBoardDataModel;
				int pageIndex = bountyBoardDataModel.PageIndex - 1;
				bountyBoardDataModel.PageIndex = pageIndex;
				m_fsms[m_bountyBoardDataModel.PageIndex].FsmVariables.FindFsmBool("PlayAudioOnSlide").Value = m_slidingTray.m_playAudioOnSlide;
				m_fsms[m_bountyBoardDataModel.PageIndex].gameObject.transform.localPosition = m_slidingTray.m_trayHiddenBone.localPosition;
				m_fsms[m_bountyBoardDataModel.PageIndex].SendEvent("Birth");
				AcknowledgeBountiesOnCurrentPage();
			}
			break;
		case "BOUNTY_HOVERED_CODE":
			OnBountyHovered();
			break;
		case "OPEN_MYTHIC_LEVEL_SELECTOR":
			m_mythicScalerPopup.SetMythicLevel(CurrentMythicLevel);
			m_currentPopup = UIContext.GetRoot().ShowPopup(m_mythicScalerPopup.gameObject).gameObject;
			break;
		case "OPEN_BOSS_RUSH_TUTORIAL":
			m_currentPopup = UIContext.GetRoot().ShowPopup(m_bossRushTutorialPopup.gameObject).gameObject;
			break;
		case "SET_MYTHIC_LEVEL":
		{
			int newMythicLevel = (CurrentMythicLevel = m_mythicScalerPopup.CurrentMythicLevel);
			LettuceVillageDataUtil.UpdateCurrentSavedMythicBountyLevel(newMythicLevel);
			m_widgetTemplate.TriggerEvent("HIDE_ALL");
			break;
		}
		case "HIDE_COMPLETE":
			UIContext.GetRoot().DismissPopup(m_currentPopup);
			m_currentPopup = null;
			break;
		}
	}

	public void OnPlayButtonReady(PlayButton playButton)
	{
		m_playButtonFinishedLoading = true;
		if (playButton == null)
		{
			Error.AddDevWarning("UI Error!", "PlayButton could not be found! You will not be able to click 'Play'!");
			return;
		}
		m_playButton = playButton;
		m_playButton.AddEventListener(UIEventType.RELEASE, OnPlayButtonRelease);
		m_playButton.Disable();
	}

	public void OnBackButtonReady(UIBButton backButton)
	{
		m_backButtonFinishedLoading = true;
		if (backButton == null)
		{
			Error.AddDevWarning("UI Error!", "BackButton could not be found! You will not be able to click 'Back'!");
			return;
		}
		m_backButton = backButton;
		m_backButton.AddEventListener(UIEventType.RELEASE, OnBackButtonRelease);
	}

	public void OnBountyBoardDisplayReady(VisualController visualController)
	{
		m_bountyBoardDisplayFinishedLoadingCount++;
	}

	private void OnMythicLevelScalerReady(LettuceBountyMythicLevelSelector popup)
	{
		m_mythicScalerFinishedLoading = true;
		if (popup == null)
		{
			Error.AddDevWarning("UI Error!", "MythicScaler could not be found");
			return;
		}
		m_mythicScalerPopup = popup;
		OnMythicLevelUpdated();
	}

	private void OnBossRushTutorialPopupReady(WidgetInstance popup)
	{
		m_bossRushTutorialFinishedLoading = true;
		if (popup == null)
		{
			Error.AddDevWarning("UI Error!", "BossRushTutorial could not be found");
		}
		else
		{
			m_bossRushTutorialPopup = popup;
		}
	}

	public EventDataModel GetEventDataModel()
	{
		return m_widgetTemplate.GetDataModel<EventDataModel>();
	}

	public override bool IsFinishedLoading(out string failureMessage)
	{
		if (!m_playButtonFinishedLoading)
		{
			failureMessage = "LettuceBountyBoardDisplay - Play button never loaded.";
			return false;
		}
		if (!m_backButtonFinishedLoading)
		{
			failureMessage = "LettuceBountyBoardDisplay - Back button never loaded.";
			return false;
		}
		if (m_bountyBoardDisplayFinishedLoadingCount != m_BountyBoardDisplays.Length)
		{
			failureMessage = $"LettuceBountyBoardDisplay - Display loading count {m_bountyBoardDisplayFinishedLoadingCount} never reached expected count {m_BountyBoardDisplays.Length}.";
			return false;
		}
		if (!m_mythicScalerFinishedLoading)
		{
			failureMessage = "LettuceBountyBoardDisplay - Mythic scaler popup never loaded.";
			return false;
		}
		if (!m_bossRushTutorialFinishedLoading)
		{
			failureMessage = "LettuceBountyBoardDisplay - Boss rush tutorial popup never loaded.";
			return false;
		}
		failureMessage = string.Empty;
		return true;
	}

	private void OnPlayButtonRelease(UIEvent e)
	{
		((LettuceVillageDisplay.LettuceSceneTransitionPayload)m_sceneTransitionPayload).m_MythicLevel = CurrentMythicLevel;
		SendAcknowledgeRequestOnExit();
		SetNextModeAndHandleTransition(SceneMgr.Mode.LETTUCE_BOUNTY_TEAM_SELECT, m_sceneTransitionPayload);
		m_playButton.Disable();
	}

	private void OnBackButtonRelease(UIEvent e)
	{
		SendAcknowledgeRequestOnExit();
		SetNextModeAndHandleTransition(SceneMgr.Mode.LETTUCE_VILLAGE, SceneMgr.TransitionHandlerType.CURRENT_SCENE);
	}

	protected override bool ShouldStartShown()
	{
		if (SceneMgr.Get().GetPrevMode() != SceneMgr.Mode.LETTUCE_BOUNTY_TEAM_SELECT && SceneMgr.Get().GetPrevMode() != SceneMgr.Mode.LETTUCE_MAP)
		{
			return SceneMgr.Get().GetPrevMode() != SceneMgr.Mode.LETTUCE_VILLAGE;
		}
		return false;
	}

	protected override void OnSlidingTrayAnimationComplete()
	{
		base.OnSlidingTrayAnimationComplete();
		if (m_bountyBoardDataModel.BountySetShortGuid == "BR")
		{
			LettuceTutorialUtils.FireEvent(LettuceTutorialVo.LettuceTutorialEvent.VILLAGE_TUTORIAL_BOSS_RUSH_POPUP, base.gameObject);
		}
	}

	private IEnumerator InitializeWhenReady()
	{
		string failureMessage;
		while (!IsFinishedLoading(out failureMessage))
		{
			yield return null;
		}
		InitializeBountyBoardDataModel();
		m_widgetTemplate.RegisterEventListener(BountyBoardEventListener);
	}

	private void InitializeBountyBoardDataModel()
	{
		m_widgetTemplate.BindDataModel(m_bountyBoardDataModel);
		if (m_sceneTransitionPayload == null)
		{
			Debug.LogError("LettuceBountyBoardDisplay: No scene transition payload was received.");
			return;
		}
		NetCache.NetCacheMercenariesPlayerInfo playerInfo = NetCache.Get().GetNetObject<NetCache.NetCacheMercenariesPlayerInfo>();
		LettuceVillageDisplay.LettuceSceneTransitionPayload payload = (LettuceVillageDisplay.LettuceSceneTransitionPayload)m_sceneTransitionPayload;
		LettuceBountySetDbfRecord bountySetRecord = payload.m_SelectedBountySet;
		List<LettuceBountyDbfRecord> records = (from r in GameDbf.LettuceBounty.GetRecords((LettuceBountyDbfRecord r) => r.BountySetId == bountySetRecord.ID && r.Enabled && r.DifficultyMode == payload.m_DifficultyMode)
			orderby r.SortOrder
			select r).ToList();
		m_bountyBoardDataModel.Bounties = new DataModelList<LettuceBountyDataModel>();
		m_bountyBoardDataModel.PageCount = 1 + (records.Count - 1) / m_bountiesPerPage;
		m_bountyBoardDataModel.PageIndex = 0;
		m_bountyBoardDataModel.AutoSelectedBountyRecordId = records[0].ID;
		int firstIncompleteBountyId = 0;
		m_bountyInfos.Clear();
		BnetBar bnetBar = BnetBar.Get();
		DateTime serverNow = DateTime.MinValue;
		if (bnetBar != null)
		{
			bnetBar.TryGetServerTimeUTC(out serverNow);
		}
		foreach (LettuceBountyDbfRecord bountyRecord in records)
		{
			NetCache.NetCacheMercenariesPlayerInfo.BountyInfo bountyInfo = ((playerInfo != null && playerInfo.BountyInfoMap.ContainsKey(bountyRecord.ID)) ? playerInfo.BountyInfoMap[bountyRecord.ID] : null);
			if (bountyInfo != null)
			{
				m_bountyInfos.Add(bountyRecord.ID, bountyInfo.Clone());
			}
			MercenariesDataUtil.MercenariesBountyLockedReason lockedReason = MercenariesDataUtil.GetBountyUnlockStatus(bountyRecord);
			bool lockedByCompleteRequirements = lockedReason != MercenariesDataUtil.MercenariesBountyLockedReason.UNLOCKED;
			bool bountyEventAvailable = lockedReason != MercenariesDataUtil.MercenariesBountyLockedReason.EVENT_NOT_ACTIVE && lockedReason != MercenariesDataUtil.MercenariesBountyLockedReason.EVENT_NOT_COMPLETE;
			bool isNew = (lockedReason == MercenariesDataUtil.MercenariesBountyLockedReason.UNLOCKED && bountyInfo == null) || (bountyInfo != null && !bountyInfo.IsAcknowledged);
			bool bountyComplete = MercenariesDataUtil.IsBountyComplete(bountyRecord.ID);
			bool isComingSoon = false;
			int comingSoonDaysRemaining = 0;
			if (bountyInfo != null && bountyInfo.UnlockTime.HasValue && serverNow < bountyInfo.UnlockTime.Value)
			{
				TimeSpan timeUntilUnlock = bountyInfo.UnlockTime.Value - serverNow;
				isComingSoon = true;
				comingSoonDaysRemaining = (int)timeUntilUnlock.TotalDays;
			}
			if (!bountyComplete && bountyEventAvailable && firstIncompleteBountyId == 0 && !isComingSoon)
			{
				firstIncompleteBountyId = bountyRecord.ID;
			}
			AdventureMissionDataModel coinData = new AdventureMissionDataModel
			{
				ScenarioId = ScenarioDbId.LETTUCE_TAVERN,
				MissionState = AdventureMissionState.UNLOCKED
			};
			LettuceVillageDataUtil.ApplyMercenaryBossCoinMaterials(coinData, LettuceVillageDataUtil.GetBountyBossIds(bountyRecord, bountyInfo), m_loadedCardDefs);
			m_bountyBoardDataModel.Bounties.Add(new LettuceBountyDataModel
			{
				BountyId = bountyRecord.ID,
				AdventureMission = coinData,
				Complete = bountyComplete,
				IsLocked = lockedByCompleteRequirements,
				IsEventLocked = !bountyEventAvailable,
				Available = true,
				ComingSoonText = bountyRecord.ComingSoonText,
				PosterText = LettuceVillageDataUtil.GeneratePosterName(bountyRecord),
				IsNew = isNew,
				BestMythicLevel = (bountyInfo?.MaxMythicLevel ?? 0),
				IsComingSoon = isComingSoon,
				ComingSoonInDays = comingSoonDaysRemaining
			});
		}
		if (firstIncompleteBountyId > 0)
		{
			m_bountyBoardDataModel.AutoSelectedBountyRecordId = firstIncompleteBountyId;
		}
		if (m_lastSelectedBountySetIdThisSession == bountySetRecord.ID && m_lastSelectedBountyRecordIdThisSession > 0)
		{
			m_bountyBoardDataModel.AutoSelectedBountyRecordId = m_lastSelectedBountyRecordIdThisSession;
			m_bountyBoardDataModel.CurrentSelectedBountyRecordId = m_lastSelectedBountyRecordIdThisSession;
		}
		m_bountyBoardDataModel.DifficultyMode = payload.m_DifficultyMode;
		m_bountyBoardDataModel.BountySetShortGuid = bountySetRecord.ShortGuid;
		m_bountyBoardDataModel.HeaderText = GameStrings.Format("GLUE_LETTUCE_BOUNTY_BOARD_HEADER", bountySetRecord.Name.GetString());
		if (!string.IsNullOrEmpty(bountySetRecord.WatermarkTexture))
		{
			AssetLoader.Get().LoadTexture(bountySetRecord.WatermarkTexture, delegate(AssetReference assetRef, UnityEngine.Object obj, object callbackData)
			{
				m_bountyBoardDataModel.BountySetWatermark = obj as Texture;
			});
		}
		AcknowledgeBountiesOnCurrentPage();
		m_initialized = true;
	}

	private void OnBountyHovered()
	{
		EventDataModel eventDataModel = GetEventDataModel();
		if (eventDataModel == null)
		{
			Log.All.PrintError("No bounty attached to the event.");
			return;
		}
		int selectedBountyID = (int)eventDataModel.Payload;
		AcknowledgeBountyAsSeen(selectedBountyID);
	}

	private void OnBountySelected()
	{
		EventDataModel eventDataModel = GetEventDataModel();
		if (eventDataModel == null)
		{
			Log.All.PrintError("No event data model attached to the LettuceBountyBoardDisplay.");
			return;
		}
		LettuceBountyDataModel selectedBounty = (LettuceBountyDataModel)eventDataModel.Payload;
		LettuceBountyDbfRecord lettuceBountyRecord = (((LettuceVillageDisplay.LettuceSceneTransitionPayload)m_sceneTransitionPayload).m_SelectedBounty = GameDbf.LettuceBounty.GetRecord(selectedBounty.BountyId));
		AcknowledgeBountyAsSeen(selectedBounty.BountyId);
		m_lastSelectedBountySetIdThisSession = lettuceBountyRecord.BountySetId;
		m_lastSelectedBountyRecordIdThisSession = lettuceBountyRecord.ID;
		m_bountyBoardDataModel.CurrentSelectedBountyRecordId = lettuceBountyRecord.ID;
		bool bountyEnabled = !selectedBounty.IsDisabled && !selectedBounty.IsLocked && !selectedBounty.IsComingSoon && selectedBounty.Available;
		NetCache.NetCacheFeatures features = NetCache.Get().GetNetObject<NetCache.NetCacheFeatures>();
		if (bountyEnabled && features.Games.MercenariesAI)
		{
			m_playButton.Enable();
		}
		else
		{
			m_playButton.Disable();
		}
		m_bountyBoardDataModel.IsSelectedBountyLocked = !bountyEnabled;
		if (selectedBounty.IsDisabled)
		{
			m_bountyBoardDataModel.BountyLockedText = GameStrings.Get("GLUE_LETTUCE_BOUNTY_BOARD_BOUNTY_DISABLED");
		}
		else if (selectedBounty.IsEventLocked)
		{
			if (!string.IsNullOrEmpty(selectedBounty.ComingSoonText))
			{
				m_bountyBoardDataModel.BountyLockedText = selectedBounty.ComingSoonText;
			}
			else
			{
				m_bountyBoardDataModel.BountyLockedText = GameStrings.Get("GLUE_LETTUCE_BOUNTY_BOARD_COMING_SOON");
			}
		}
		else if (selectedBounty.IsLocked)
		{
			m_bountyBoardDataModel.BountyLockedText = GameStrings.Get("GLUE_LETTUCE_BOUNTY_BOARD_BOUNTY_LOCKED");
		}
		m_bountyBoardDataModel.BossCard.Clear();
		if (LettuceVillageDataUtil.TryGetBountyBossData(lettuceBountyRecord, out var bossName, out var bossCards))
		{
			m_bountyBoardDataModel.BossName = bossName;
			foreach (string bossCard in bossCards)
			{
				m_bountyBoardDataModel.BossCard.Add(new CardDataModel
				{
					CardId = bossCard
				});
			}
		}
		else
		{
			m_bountyBoardDataModel.BossName = string.Empty;
		}
		m_bountyBoardDataModel.BossDescription = GameStrings.Format("GLUE_LETTUCE_BOUNTY_BOSS_DESCRIPTION", lettuceBountyRecord.BountyLevel);
		foreach (DefLoader.DisposableFullDef loadedBountyRewardFullDef in m_loadedBountyRewardFullDefs)
		{
			loadedBountyRewardFullDef.Dispose();
		}
		m_loadedBountyRewardFullDefs.Clear();
		m_bountyBoardDataModel.SelectedBountyRewardList = new RewardListDataModel();
		m_bountyBoardDataModel.SelectedBountyRewardList.Items.AddRange(LettuceVillageDataUtil.GetFinalBossRewards(lettuceBountyRecord, out var rewardDescription, out var additionalRewardDescription));
		m_bountyBoardDataModel.SelectedBountyRewardList.Description = rewardDescription;
		m_bountyBoardDataModel.SelectedBountyRewardList.AdditionalDescription = additionalRewardDescription;
	}

	private void OnMythicLevelUpdated(NetCache.NetCacheMercenariesPlayerInfo playerInfo = null)
	{
		if (!LettuceVillageDataUtil.TryGetCurrentSavedMythicBountyLevel(out var mythicLevel))
		{
			return;
		}
		if (playerInfo == null)
		{
			playerInfo = NetCache.Get().GetNetObject<NetCache.NetCacheMercenariesPlayerInfo>();
			if (playerInfo == null)
			{
				return;
			}
		}
		mythicLevel = Mathf.Clamp(mythicLevel, playerInfo.MinMythicBountyLevel, playerInfo.MaxMythicBountyLevel);
		CurrentMythicLevel = mythicLevel;
		if (m_mythicScalerPopup != null)
		{
			m_mythicScalerPopup.SetMythicLevelLimits(playerInfo.MinMythicBountyLevel, playerInfo.MaxMythicBountyLevel);
		}
	}

	private void AcknowledgeBountiesOnCurrentPage()
	{
		if (m_bountyBoardDataModel == null)
		{
			return;
		}
		int end = Math.Min((m_bountyBoardDataModel.PageIndex + 1) * m_bountiesPerPage, m_bountyBoardDataModel.Bounties.Count);
		for (int start = m_bountyBoardDataModel.PageIndex * m_bountiesPerPage; start < end; start++)
		{
			LettuceBountyDataModel bountyDM = m_bountyBoardDataModel.Bounties[start];
			if (!m_BountyIdsToAcknowledge.Contains(bountyDM.BountyId) && MercenariesDataUtil.GetBountyUnlockStatus(bountyDM.BountyId) == MercenariesDataUtil.MercenariesBountyLockedReason.UNLOCKED)
			{
				m_BountyIdsToAcknowledge.Add(bountyDM.BountyId);
			}
		}
	}

	private void AcknowledgeBountyAsSeen(int bountyRecordID)
	{
		foreach (LettuceBountyDataModel bountyModel in m_bountyBoardDataModel.Bounties)
		{
			if (bountyModel.BountyId == bountyRecordID && bountyModel.IsNew)
			{
				if (!m_BountyIdsToAcknowledge.Contains(bountyRecordID) && MercenariesDataUtil.GetBountyUnlockStatus(bountyRecordID) == MercenariesDataUtil.MercenariesBountyLockedReason.UNLOCKED)
				{
					m_BountyIdsToAcknowledge.Add(bountyRecordID);
				}
				bountyModel.IsNew = false;
				break;
			}
		}
	}

	private void SendAcknowledgeRequestOnExit()
	{
		if (m_BountyIdsToAcknowledge.Count > 0)
		{
			Network.Get().AcknowledgeBounties(m_BountyIdsToAcknowledge);
			NetCache.Get().UpdateNetCachePlayerInfoAcknowledgedBounties(m_BountyIdsToAcknowledge);
		}
	}

	private bool ShouldReinitializeBountyBoardDataModel(Dictionary<int, NetCache.NetCacheMercenariesPlayerInfo.BountyInfo> bountyInfos)
	{
		foreach (LettuceBountyDataModel bountyDataModel in m_bountyBoardDataModel.Bounties)
		{
			m_bountyInfos.TryGetValue(bountyDataModel.BountyId, out var cachedBountyInfo);
			bountyInfos.TryGetValue(bountyDataModel.BountyId, out var bountyInfo);
			if (bountyInfo == null)
			{
				continue;
			}
			if (cachedBountyInfo == null)
			{
				if (bountyInfo.IsComplete || bountyInfo.UnlockTime.HasValue || bountyInfo.BossCardIds != null)
				{
					return true;
				}
				continue;
			}
			if (bountyInfo.IsComplete == cachedBountyInfo.IsComplete)
			{
				DateTime? unlockTime = bountyInfo.UnlockTime;
				DateTime? unlockTime2 = cachedBountyInfo.UnlockTime;
				if (unlockTime.HasValue == unlockTime2.HasValue && (!unlockTime.HasValue || !(unlockTime.GetValueOrDefault() != unlockTime2.GetValueOrDefault())) && !((bountyInfo.BossCardIds != null && cachedBountyInfo.BossCardIds != null) ? (!bountyInfo.BossCardIds.SequenceEqual(cachedBountyInfo.BossCardIds)) : (bountyInfo.BossCardIds != cachedBountyInfo.BossCardIds)))
				{
					continue;
				}
			}
			return true;
		}
		return false;
	}

	private void UpdateMercenariesPlayerInfo()
	{
		NetCache.NetCacheMercenariesPlayerInfo playerInfo = NetCache.Get().GetNetObject<NetCache.NetCacheMercenariesPlayerInfo>();
		if (playerInfo == null)
		{
			return;
		}
		if (ShouldReinitializeBountyBoardDataModel(playerInfo.BountyInfoMap))
		{
			m_lastSelectedBountyRecordIdThisSession = 0;
			InitializeBountyBoardDataModel();
			LettuceBountyDataModel modelToSelect = m_bountyBoardDataModel.Bounties.First((LettuceBountyDataModel x) => x.BountyId == m_bountyBoardDataModel.AutoSelectedBountyRecordId);
			if (modelToSelect != null)
			{
				m_widgetTemplate.TriggerEvent("BOUNTY_RELEASED", new TriggerEventParameters(null, modelToSelect));
			}
		}
		OnMythicLevelUpdated(playerInfo);
	}
}
