using System.Collections;
using System.Linq;
using Assets;
using Hearthstone.Progression;
using PegasusShared;
using UnityEngine;

public class VictoryScreen : EndGameScreen
{
	public GamesWonIndicator m_gamesWonIndicator;

	public Transform m_goldenHeroEventBone;

	private bool m_showWinProgress;

	private bool m_showHeroRewardEvent;

	private bool m_heroRewardCardDefReady;

	private string m_heroRewardCardID;

	private HeroRewardEvent m_heroRewardEvent;

	private DefLoader.DisposableCardDef m_heroRewardCardDef;

	protected int m_heroRewardAchievementID;

	private const string NO_HERO_REWARD = "none";

	public bool hasCheckedForNewlyEarnedHeroRewards;

	private int? m_newlyCompletedHeroSkinRewardAchievementId;

	private bool m_hasAlreadyCheckedForAppRating;

	protected override void Awake()
	{
		base.Awake();
		m_gamesWonIndicator?.Hide();
		if (ShouldMakeUtilRequests())
		{
			if (GameMgr.Get().IsTraditionalTutorial())
			{
				NetCache.Get().RegisterTutorialEndGameScreen(OnNetCacheReady);
			}
			else
			{
				NetCache.Get().RegisterScreenEndOfGame(OnNetCacheReady);
			}
			AchievementManager.Get().OnStatusChanged += OnAchievementStatusChanged;
			QueueCompletedHeroSkinAchievements();
		}
		if (GameMgr.Get().IsTraditionalTutorial())
		{
			BnetBar bnetBar = BnetBar.Get();
			bnetBar.HideSkipTutorialButton();
			bnetBar.Dim();
		}
	}

	protected override void OnDestroy()
	{
		m_heroRewardCardDef?.Dispose();
		m_heroRewardCardDef = null;
		base.OnDestroy();
	}

	protected override void ShowStandardFlow()
	{
		base.ShowStandardFlow();
		if (BattlegroundsEmoteHandler.TryGetActiveInstance(out var battlegroundsEmoteHandler))
		{
			battlegroundsEmoteHandler.HideEmotes();
		}
		else if (EmoteHandler.Get() != null)
		{
			EmoteHandler.Get().HideEmotes();
		}
		if (TargetReticleManager.Get() != null)
		{
			TargetReticleManager.Get().DestroyEnemyTargetArrow();
			TargetReticleManager.Get().DestroyFriendlyTargetArrow(isLocallyCanceled: false);
		}
		if (!GameMgr.Get().IsTraditionalTutorial() || GameMgr.Get().IsSpectator())
		{
			m_hitbox.AddEventListener(UIEventType.RELEASE, base.ContinueButtonPress_PrevMode);
		}
		else if (GameMgr.Get().GetMissionId() == 5290 && GameUtils.IsTraditionalTutorialComplete() && TutorialProgressScreen.HasEverOpenedRewardChest())
		{
			LoadingScreen.Get().SetFadeColor(Color.white);
			m_hitbox.AddEventListener(UIEventType.RELEASE, base.ContinueButtonPress_FirstTimeHub);
		}
		else if (DemoMgr.Get().GetMode() == DemoMode.BLIZZ_MUSEUM && GameUtils.GetNextTutorial() == 0)
		{
			StartCoroutine(DemoMgr.Get().CompleteBlizzMuseumDemo());
		}
		else
		{
			m_hitbox.AddEventListener(UIEventType.RELEASE, base.ContinueButtonPress_TutorialProgress);
		}
	}

	protected override bool ShowHeroRewardEvent()
	{
		if (m_heroRewardEvent == null)
		{
			return false;
		}
		if (m_heroRewardEvent.gameObject.activeInHierarchy)
		{
			m_heroRewardEvent.Hide();
			m_showHeroRewardEvent = false;
			return false;
		}
		AchievementManager.Get().ClaimAchievementReward(m_heroRewardAchievementID);
		SetPlayingBlockingAnim(set: true);
		m_heroRewardEvent.RegisterAnimationDoneListener(NotifyOfGoldenHeroAnimComplete);
		m_twoScoop.StopAnimating();
		m_heroRewardEvent.Show();
		m_twoScoop.m_heroActor.transform.parent = m_heroRewardEvent.m_heroBone;
		m_twoScoop.m_heroActor.transform.localPosition = Vector3.zero;
		m_twoScoop.m_heroActor.transform.localScale = new Vector3(1.375f, 1.375f, 1.375f);
		return true;
	}

	private bool CheckForNewlyEarnedHeroReward(out bool showHeroRewardEvent, out string heroRewardCardID, out int heroRewardAchievementID)
	{
		showHeroRewardEvent = m_showHeroRewardEvent;
		heroRewardCardID = string.Empty;
		heroRewardAchievementID = 0;
		if (hasCheckedForNewlyEarnedHeroRewards)
		{
			return false;
		}
		if (!GetNewHeroRewardCardIdAndAchievement(out heroRewardCardID, out heroRewardAchievementID))
		{
			return false;
		}
		hasCheckedForNewlyEarnedHeroRewards = true;
		showHeroRewardEvent = heroRewardCardID != "none";
		return true;
	}

	private void LoadAnimatedPrefabsForHeroSkins(string heroRewardCardID, int heroRewardAchievementID)
	{
		if (heroRewardCardID != "none")
		{
			CardPortraitQuality portraitQuality = new CardPortraitQuality(3, TAG_PREMIUM.GOLDEN);
			DefLoader.Get().LoadCardDef(heroRewardCardID, OnHeroRewardEventLoaded, null, portraitQuality);
			if (GameUtils.IsHonored1KHeroSkinAchievement(heroRewardAchievementID))
			{
				AssetLoader.Get().InstantiatePrefab("Hero2PremiumHero.prefab:1115650b4bc229d49a8d45470424f5cd", OnHeroRewardEventLoaded);
			}
			else if (GameUtils.IsGolden500HeroSkinAchievement(heroRewardAchievementID))
			{
				AssetLoader.Get().InstantiatePrefab("Hero2GoldHero.prefab:a83a85837f828844caba16593ea3c1d0", OnHeroRewardEventLoaded);
			}
		}
	}

	private bool TryGetLatestHeroRewardCardIdAndLoadPrefabs()
	{
		if (!CheckForNewlyEarnedHeroReward(out var showHeroRewardEvent, out var heroRewardCardID, out var heroRewardAchievementID))
		{
			return false;
		}
		m_showHeroRewardEvent = showHeroRewardEvent;
		m_heroRewardCardID = heroRewardCardID;
		m_heroRewardAchievementID = heroRewardAchievementID;
		LoadAnimatedPrefabsForHeroSkins(heroRewardCardID, heroRewardAchievementID);
		return true;
	}

	protected override bool JustEarnedHeroReward()
	{
		return TryGetLatestHeroRewardCardIdAndLoadPrefabs();
	}

	protected override bool ShowHealUpDialog()
	{
		return TemporaryAccountManager.Get().ShowHealUpDialog(GameStrings.Get("GLUE_TEMPORARY_ACCOUNT_DIALOG_HEADER_02"), GameStrings.Get("GLUE_TEMPORARY_ACCOUNT_DIALOG_BODY_04"), TemporaryAccountManager.HealUpReason.WIN_GAME, userTriggered: false, OnHealUpDialogDismissed);
	}

	private void OnHealUpDialogDismissed()
	{
		ContinueEvents();
	}

	protected override bool ShowPushNotificationPrompt()
	{
		return PushNotificationManager.Get().ShowPushNotificationContext(OnPushNotificationDialogDismissed);
	}

	private void OnPushNotificationDialogDismissed()
	{
		ContinueEvents();
	}

	protected override void OnTwoScoopShown()
	{
		if (BnetBar.Get() != null)
		{
			BnetBar.Get().SuppressLoginTooltip(val: true);
		}
		if (m_showWinProgress)
		{
			m_gamesWonIndicator?.Show();
		}
	}

	protected override void OnTwoScoopHidden()
	{
		if (m_showWinProgress)
		{
			m_gamesWonIndicator?.Hide();
		}
	}

	private void OnHeroRewardEventLoaded(AssetReference assetRef, GameObject go, object callbackData)
	{
		m_heroRewardEvent = go.GetComponent<HeroRewardEvent>();
		m_heroRewardEvent.LoadHeroCardDefs(m_heroRewardCardID);
		StartCoroutine(WaitUntilTwoScoopLoaded(base.name, go));
	}

	public void NotifyOfGoldenHeroAnimComplete()
	{
		SetPlayingBlockingAnim(set: false);
		m_heroRewardEvent.RemoveAnimationDoneListener(NotifyOfGoldenHeroAnimComplete);
	}

	private IEnumerator WaitUntilTwoScoopLoaded(AssetReference assetRef, GameObject go)
	{
		while (m_twoScoop == null || !m_twoScoop.IsLoaded())
		{
			yield return null;
		}
		while (!m_heroRewardCardDefReady)
		{
			yield return null;
		}
		go.SetActive(value: false);
		TransformUtil.AttachAndPreserveLocalTransform(go.transform, m_goldenHeroEventBone);
		Texture heroTexture = m_heroRewardCardDef.CardDef.GetPortraitTexture(TAG_PREMIUM.NORMAL);
		m_heroRewardEvent.SetHeroBurnAwayTexture(heroTexture);
		m_heroRewardEvent.SetVictoryTwoScoop((VictoryTwoScoop)m_twoScoop);
		SetHeroRewardEventReady(isReady: true);
	}

	protected override void InitGoldRewardUI()
	{
		m_showWinProgress = true;
	}

	private bool GetNewHeroRewardCardIdAndAchievement(out string heroRewardCardId, out int heroRewardAchievementID)
	{
		heroRewardCardId = "none";
		heroRewardAchievementID = 0;
		if (m_newlyCompletedHeroSkinRewardAchievementId.HasValue)
		{
			RewardListDbfRecord rewardList = GameDbf.Achievement.GetRecord(m_newlyCompletedHeroSkinRewardAchievementId.Value)?.RewardListRecord;
			if (rewardList == null)
			{
				Log.Gameplay.PrintError("GetNewHeroRewardCardIdAndAchievement no achievement data model for {0}.", m_newlyCompletedHeroSkinRewardAchievementId.Value);
				return false;
			}
			foreach (RewardItemDbfRecord rewardItem in rewardList.RewardItems)
			{
				if (rewardItem.RewardType == RewardItem.RewardType.HERO_SKIN)
				{
					heroRewardCardId = rewardItem.CardRecord.NoteMiniGuid;
					heroRewardAchievementID = m_newlyCompletedHeroSkinRewardAchievementId.Value;
					return true;
				}
			}
		}
		return false;
	}

	private void OnHeroRewardEventLoaded(string cardId, DefLoader.DisposableCardDef def, object userData)
	{
		m_heroRewardCardDef?.Dispose();
		m_heroRewardCardDef = def;
		m_heroRewardCardDefReady = true;
	}

	public void OnAchievementStatusChanged(int achievementId, AchievementManager.AchievementStatus status)
	{
		if (status == AchievementManager.AchievementStatus.COMPLETED && (GameUtils.IsGolden500HeroSkinAchievement(achievementId) || GameUtils.IsHonored1KHeroSkinAchievement(achievementId)))
		{
			AchievementDbfRecord achievementAsset = GameDbf.Achievement.GetRecord(achievementId);
			if (achievementAsset?.RewardListRecord != null && achievementAsset.RewardListRecord.RewardItems.FirstOrDefault((RewardItemDbfRecord x) => x.RewardType == RewardItem.RewardType.HERO_SKIN) != null)
			{
				m_newlyCompletedHeroSkinRewardAchievementId = achievementId;
				hasCheckedForNewlyEarnedHeroRewards = false;
			}
		}
	}

	private void QueueCompletedHeroSkinAchievements()
	{
		if (SpectatorManager.Get().IsSpectatingOrWatching)
		{
			return;
		}
		Entity myHero = GameState.Get().GetLocalSidePlayer()?.GetHero();
		if (myHero == null)
		{
			return;
		}
		TAG_CLASS myHeroClass = myHero.GetClass();
		if (GameUtils.HERO_SKIN_ACHIEVEMENTS.TryGetValue(myHeroClass, out var achievements))
		{
			if (AchievementManager.Get().GetAchievementDataModel(achievements.Golden500Win).Status == AchievementManager.AchievementStatus.COMPLETED)
			{
				m_newlyCompletedHeroSkinRewardAchievementId = achievements.Golden500Win;
			}
			else if (AchievementManager.Get().GetAchievementDataModel(achievements.Honored1kWin).Status == AchievementManager.AchievementStatus.COMPLETED)
			{
				m_newlyCompletedHeroSkinRewardAchievementId = achievements.Honored1kWin;
			}
		}
	}

	protected override void ShowAppRatingPrompt()
	{
		if (m_hasAlreadyCheckedForAppRating)
		{
			return;
		}
		m_hasAlreadyCheckedForAppRating = true;
		if (GameMgr.Get().IsRankedPlay())
		{
			FormatType currentFormatType = Options.GetFormatType();
			MedalInfoTranslator medalInfoTranslator = RankMgr.Get().GetLocalPlayerMedalInfo();
			if (medalInfoTranslator != null && medalInfoTranslator.IsOnWinStreak(currentFormatType))
			{
				MobileCallbackManager.RequestAppReview(AppRatingPromptTrigger.RANKED_WIN_STREAK);
			}
		}
	}
}
