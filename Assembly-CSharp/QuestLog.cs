using System;
using System.Collections;
using System.Collections.Generic;
using Assets;
using Blizzard.T5.Configuration;
using Hearthstone.DataModels;
using Hearthstone.UI;
using PegasusShared;
using UnityEngine;

[CustomEditClass]
public class QuestLog : UIBPopup
{
	public const int QUEST_LOG_MAX_COUNT = 3;

	public GameObject m_root;

	public UberText m_winsCountText;

	public UberText m_forgeRecordCountText;

	public UberText m_totalLevelsText;

	public Transform m_arenaMedalBone;

	public ArenaMedal m_arenaMedalPrefab;

	public PegUIElement m_offClickCatcher;

	public List<ClassProgressBar> m_classProgressBars;

	public List<ClassProgressInfo> m_classProgressInfos;

	public AsyncReference m_rankedMedalWidgetReference;

	public AsyncReference m_rankedRewardInfoButtonWidgetReference;

	public GameObject m_questTilePrefab;

	public List<Transform> m_questBones;

	public UberText m_noQuestText;

	public UIBButton m_closeButton;

	[CustomEditField(Sections = "Aspect Ratio Positioning")]
	public float m_extraWideScale = 150f;

	private List<QuestTile> m_currentQuests;

	private static QuestLog s_instance;

	private int m_justCanceledQuestID;

	private Widget m_rankedMedalWidget;

	private RankedMedal m_rankedMedal;

	private Widget m_rankedRewardInfoButtonWidget;

	private RankedRewardInfoButton m_rankedRewardInfoButton;

	private ArenaMedal m_arenaMedal;

	private Enum[] m_presencePrevStatus;

	private ScreenEffectsHandle m_screenEffectsHandle;

	protected override void Awake()
	{
		base.Awake();
		s_instance = this;
		AchieveManager.Get().RegisterAchievesUpdatedListener(OnAchievesUpdated);
		if (m_closeButton != null)
		{
			m_closeButton.AddEventListener(UIEventType.RELEASE, OnCloseButtonReleased);
		}
		m_screenEffectsHandle = new ScreenEffectsHandle(this);
	}

	protected override void Start()
	{
		base.Start();
		m_rankedMedalWidgetReference.RegisterReadyListener<Widget>(OnRankedMedalWidgetReady);
		m_rankedRewardInfoButtonWidgetReference.RegisterReadyListener<Widget>(OnRankedRewardInfoButtonWidgetReady);
		for (int i = 0; i < m_classProgressInfos.Count; i++)
		{
			ClassProgressInfo info = m_classProgressInfos[i];
			TAG_CLASS tagClass = info.m_class;
			ClassProgressBar classProgressBar = info.m_frame;
			LayerUtils.SetLayer(classProgressBar, info.m_frame.gameObject.layer);
			classProgressBar.m_class = tagClass;
			m_classProgressBars.Add(classProgressBar);
		}
		m_offClickCatcher.AddEventListener(UIEventType.RELEASE, OnQuestLogCloseEvent);
		BnetBar bnetBar = BnetBar.Get();
		if (bnetBar != null)
		{
			bnetBar.OnMenuOpened += OnMenuOpened;
		}
	}

	private void OnDestroy()
	{
		if (ShownUIMgr.Get() != null)
		{
			ShownUIMgr.Get().ClearShownUI();
		}
		if (AchieveManager.Get() != null)
		{
			AchieveManager.Get().RemoveAchievesUpdatedListener(OnAchievesUpdated);
			AchieveManager.Get().RemoveQuestCanceledListener(OnQuestCanceled);
		}
		m_screenEffectsHandle.StopEffect();
		Navigation.RemoveHandler(OnNavigateBack);
		Hide(animate: false);
		BnetBar bnetBar = BnetBar.Get();
		if (bnetBar != null)
		{
			bnetBar.OnMenuOpened -= OnMenuOpened;
		}
		s_instance = null;
	}

	public static QuestLog Get()
	{
		return s_instance;
	}

	public void StartHidden()
	{
		DoHideAnimation(disableAnimation: true);
	}

	public void SetCloseButtonActive(bool active)
	{
		if (m_closeButton != null)
		{
			m_closeButton.gameObject.SetActive(active);
		}
	}

	public override void Show()
	{
		if (this == null)
		{
			Debug.Log("QuestLog: Attempting to Show after the QuestLog component has already been destroyed.");
			return;
		}
		m_presencePrevStatus = PresenceMgr.Get().GetStatus();
		PresenceMgr.Get().SetStatus(Global.PresenceStatus.QUESTLOG);
		AchieveManager.Get().RegisterQuestCanceledListener(OnQuestCanceled);
		ScreenEffectParameters screenEffectParameters = ScreenEffectParameters.BlurVignetteDesaturatePerspective;
		screenEffectParameters.Time = 0.1f;
		m_screenEffectsHandle.StartEffect(screenEffectParameters);
		Navigation.Push(OnNavigateBack);
		if ((bool)UniversalInputManager.UsePhoneUI && m_root != null)
		{
			float localScale = 1f;
			m_scaleMode = CanvasScaleMode.WIDTH;
			if (TransformUtil.IsExtraWideAspectRatio())
			{
				localScale = m_extraWideScale;
				m_scaleMode = CanvasScaleMode.HEIGHT;
			}
			OverlayUI.Get().AddGameObject(base.gameObject, CanvasAnchor.CENTER, destroyOnSceneLoad: false, m_scaleMode);
			m_root.transform.localScale = Vector3.one * localScale;
		}
		StartCoroutine(ShowWhenReady());
	}

	private IEnumerator ShowWhenReady()
	{
		while (m_rankedMedalWidget == null || m_rankedRewardInfoButtonWidget == null)
		{
			yield return null;
		}
		UpdateData();
		while (m_rankedMedalWidget.IsChangingStates || m_rankedRewardInfoButtonWidget.IsChangingStates)
		{
			yield return null;
		}
		base.Show();
	}

	protected override void Hide(bool animate)
	{
		if (this == null)
		{
			Debug.Log("QuestLog: Attempting to Hide after the QuestLog component has already been destroyed.");
			return;
		}
		if (m_presencePrevStatus == null)
		{
			m_presencePrevStatus = new Enum[1] { Global.PresenceStatus.HUB };
		}
		PresenceMgr.Get().SetStatus(m_presencePrevStatus);
		if (ShownUIMgr.Get() != null)
		{
			ShownUIMgr.Get().ClearShownUI();
		}
		foreach (QuestTile questTile in m_currentQuests)
		{
			if (questTile != null)
			{
				questTile.OnClose();
			}
		}
		DoHideAnimation(!animate, delegate
		{
			if (AchieveManager.Get() != null)
			{
				AchieveManager.Get().RemoveQuestCanceledListener(OnQuestCanceled);
			}
			DeleteQuests();
			if (FullScreenFXMgr.Get() != null)
			{
				m_screenEffectsHandle.StopEffect();
			}
			m_shown = false;
		});
	}

	private void DeleteQuests()
	{
		if (m_currentQuests == null || m_currentQuests.Count == 0)
		{
			return;
		}
		foreach (QuestTile questTile in m_currentQuests)
		{
			if (questTile != null)
			{
				UnityEngine.Object.Destroy(questTile.gameObject);
			}
		}
	}

	private void OnQuestLogCloseEvent(UIEvent e)
	{
		Navigation.GoBack();
	}

	private bool OnNavigateBack()
	{
		Hide(animate: true);
		return true;
	}

	private void UpdateData()
	{
		UpdateClassProgress();
		UpdateActiveQuests();
		UpdateRankedMedal();
		UpdateRankedRewardInfo();
		UpdateBestArenaMedal();
		UpdateTotalWins();
	}

	private void UpdateTotalWins()
	{
		int ammSum = 0;
		int arenaSum = 0;
		NetCache.NetCachePlayerRecords cachedPlayerRecords = NetCache.Get()?.GetNetObject<NetCache.NetCachePlayerRecords>();
		if (cachedPlayerRecords?.Records == null)
		{
			return;
		}
		foreach (NetCache.PlayerRecord record in cachedPlayerRecords.Records)
		{
			if (record.Data == 0)
			{
				switch (record.RecordType)
				{
				case GameType.GT_ARENA:
					arenaSum += record.Wins;
					break;
				case GameType.GT_RANKED:
				case GameType.GT_CASUAL:
				case GameType.GT_TAVERNBRAWL:
					ammSum += record.Wins;
					break;
				}
			}
		}
		m_winsCountText.Text = ammSum.ToString();
		m_forgeRecordCountText.Text = arenaSum.ToString();
	}

	private void UpdateBestArenaMedal()
	{
		NetCache.NetCacheProfileProgress netCacheProfileProgress = NetCache.Get().GetNetObject<NetCache.NetCacheProfileProgress>();
		if (m_arenaMedal == null)
		{
			m_arenaMedal = (ArenaMedal)GameUtils.Instantiate(m_arenaMedalPrefab, m_arenaMedalBone.gameObject, withRotation: true);
			LayerUtils.SetLayer(m_arenaMedal, m_arenaMedalBone.gameObject.layer);
			m_arenaMedal.transform.localScale = Vector3.one;
		}
		if (netCacheProfileProgress.LastForgeDate != 0L)
		{
			m_arenaMedal.gameObject.SetActive(value: true);
			m_arenaMedal.SetMedal(netCacheProfileProgress.BestForgeWins);
		}
		else
		{
			m_arenaMedal.gameObject.SetActive(value: false);
		}
	}

	private void OnRankedMedalWidgetReady(Widget widget)
	{
		m_rankedMedalWidget = widget;
		m_rankedMedal = m_rankedMedalWidget.GetComponentInChildren<RankedMedal>();
	}

	private void OnRankedRewardInfoButtonWidgetReady(Widget widget)
	{
		m_rankedRewardInfoButtonWidget = widget;
		m_rankedRewardInfoButtonWidget.Hide();
		m_rankedRewardInfoButton = m_rankedRewardInfoButtonWidget.GetComponentInChildren<RankedRewardInfoButton>();
	}

	private void UpdateRankedMedal()
	{
		if (!(m_rankedMedalWidget == null) && !(m_rankedMedal == null))
		{
			MedalInfoTranslator localPlayerMedalInfo = RankMgr.Get().GetLocalPlayerMedalInfo();
			RankedPlayDataModel rankedDataModel = localPlayerMedalInfo.CreateDataModel(localPlayerMedalInfo.GetBestCurrentRankFormatType(), RankedMedal.DisplayMode.Default, isTooltipEnabled: true);
			m_rankedMedal.BindRankedPlayDataModel(rankedDataModel);
		}
	}

	private void UpdateRankedRewardInfo()
	{
		MedalInfoTranslator mit = RankMgr.Get().GetLocalPlayerMedalInfo();
		if (m_rankedRewardInfoButton != null)
		{
			m_rankedRewardInfoButton.Initialize(mit);
			m_rankedRewardInfoButton.Show();
		}
	}

	private void UpdateClassProgress()
	{
		if (m_classProgressBars.Count == 0)
		{
			return;
		}
		int totalLevels = 0;
		List<Achievement> goldHeroAchieves = AchieveManager.Get().GetAchievesInGroup(Achieve.Type.GOLDHERO, isComplete: true);
		NetCache.NetCacheHeroLevels heroLevels = NetCache.Get().GetNetObject<NetCache.NetCacheHeroLevels>();
		foreach (ClassProgressBar classProgress in m_classProgressBars)
		{
			NetCache.HeroLevel heroLevel = heroLevels.Levels.Find((NetCache.HeroLevel obj) => obj.Class == classProgress.m_class);
			Achievement goldHero = goldHeroAchieves.Find((Achievement obj) => obj.MyHeroClassRequirement.HasValue && obj.MyHeroClassRequirement.Value == classProgress.m_class);
			classProgress.SetPremium(goldHero != null);
			if (heroLevel != null)
			{
				classProgress.m_classLockedGO.SetActive(value: false);
				classProgress.m_levelText.Text = heroLevel.CurrentLevel.Level.ToString();
				int nextRewardLevel = 0;
				RewardData nextReward = FixedRewardsMgr.Get().GetNextHeroLevelReward(heroLevel.Class, heroLevel.CurrentLevel.Level, out nextRewardLevel);
				if (nextReward != null)
				{
					classProgress.SetTooltipText(GameStrings.Format("GLOBAL_HERO_LEVEL_NEXT_REWARD_TITLE", nextRewardLevel), RewardUtils.GetRewardText(nextReward), heroLevel.CurrentLevel.Level.ToString());
				}
				totalLevels += heroLevel.CurrentLevel.Level;
				if (heroLevel.CurrentLevel.IsMaxLevel())
				{
					classProgress.m_progressBar.SetProgressBar(1f);
				}
				else
				{
					classProgress.m_progressBar.SetProgressBar((float)heroLevel.CurrentLevel.XP / (float)heroLevel.CurrentLevel.MaxXP);
				}
			}
			else
			{
				classProgress.m_levelText.Text = "0";
				classProgress.Lock();
			}
		}
		if (m_totalLevelsText != null)
		{
			m_totalLevelsText.Text = string.Format(GameStrings.Get("GLUE_QUEST_LOG_TOTAL_LEVELS"), totalLevels);
		}
	}

	private void UpdateActiveQuests()
	{
		List<Achievement> activeQuests = AchieveManager.Get().GetActiveQuests();
		m_currentQuests = new List<QuestTile>();
		for (int i = 0; i < activeQuests.Count; i++)
		{
			if (i < 3)
			{
				AddCurrentQuestTile(activeQuests[i], i);
			}
		}
		if (m_currentQuests.Count == 0)
		{
			m_noQuestText.gameObject.SetActive(value: true);
			if (AchieveManager.Get().HasUnlockedFeature(Achieve.Unlocks.DAILY))
			{
				m_noQuestText.Text = GameStrings.Get("GLUE_QUEST_LOG_NO_QUESTS_DAILIES_UNLOCKED");
				if (!Options.Get().GetBool(Option.HAS_RUN_OUT_OF_QUESTS, defaultVal: false) && UserAttentionManager.CanShowAttentionGrabber("QuestLog.UpdateActiveQuests:" + Option.HAS_RUN_OUT_OF_QUESTS))
				{
					NotificationManager.Get().CreateInnkeeperQuote(UserAttentionBlocker.NONE, new Vector3(155.3f, 0f, 34.5f), GameStrings.Get("VO_INNKEEPER_OUT_OF_QUESTS"), "VO_INNKEEPER_OUT_OF_QUESTS.prefab:b0073c56bf38c664dab532ad92f3baf9");
					Options.Get().SetBool(Option.HAS_RUN_OUT_OF_QUESTS, val: true);
				}
			}
			else
			{
				m_noQuestText.Text = GameStrings.Get("GLUE_QUEST_LOG_NO_QUESTS");
			}
		}
		else
		{
			m_noQuestText.gameObject.SetActive(value: false);
		}
	}

	private void AddCurrentQuestTile(Achievement achieveQuest, int slot)
	{
		if (m_questTilePrefab == null || m_questBones == null || m_questBones[slot] == null || m_currentQuests == null)
		{
			Debug.Log("QuestLog: AddCurrentQuestTile failed, because a required object is null.");
			return;
		}
		GameObject obj = (GameObject)GameUtils.Instantiate(m_questTilePrefab, m_questBones[slot].gameObject, withRotation: true);
		LayerUtils.SetLayer(obj, m_questBones[slot].gameObject.layer, null);
		obj.transform.localScale = Vector3.one;
		QuestTile quest = obj.GetComponent<QuestTile>();
		quest.SetupTile(achieveQuest, QuestTile.FsmEvent.QuestShownInQuestLog);
		quest.SetCanShowCancelButton(canShowCancel: true);
		m_currentQuests.Add(quest);
	}

	private void OnQuestCanceled(int achieveID, bool canceled, object userData)
	{
		if (canceled)
		{
			m_justCanceledQuestID = achieveID;
		}
	}

	private void OnAchievesUpdated(List<Achievement> updatedAchieves, List<Achievement> completedAchieves, object userData)
	{
		if (m_justCanceledQuestID == 0)
		{
			return;
		}
		List<Achievement> newActiveQuests = AchieveManager.Get().GetActiveQuests(onlyNewlyActive: true);
		if (newActiveQuests.Count <= 0)
		{
			return;
		}
		if (newActiveQuests.Count > 1 && !Vars.Key("Quests.CanCancelManyTimes").GetBool(def: false) && !Vars.Key("Quests.CancelGivesManyNewQuests").GetBool(def: false))
		{
			Debug.LogError($"QuestLog.OnActiveAchievesUpdated(): expecting ONE new active quest after a quest cancel but received {newActiveQuests.Count}");
			Hide();
			return;
		}
		int justCanceledQuest = m_justCanceledQuestID;
		m_justCanceledQuestID = 0;
		QuestTile questTile = m_currentQuests.Find((QuestTile obj) => obj.GetQuestID() == justCanceledQuest);
		if (questTile == null)
		{
			Debug.LogError($"QuestLog.OnActiveAchievesUpdated(): could not find tile for just canceled quest (quest ID {justCanceledQuest})");
			Hide();
			return;
		}
		Log.Achievements.Print("Adding QuestLog tile for: {0}", newActiveQuests[0]);
		questTile.SetupTile(newActiveQuests[0], QuestTile.FsmEvent.QuestRerolled);
		for (int i = 1; i < newActiveQuests.Count; i++)
		{
			int slot = m_currentQuests.Count;
			if (slot >= m_questBones.Count)
			{
				break;
			}
			AddCurrentQuestTile(newActiveQuests[i], slot);
		}
		foreach (QuestTile currentQuest in m_currentQuests)
		{
			currentQuest.UpdateCancelButtonVisibility();
		}
	}

	private void OnCloseButtonReleased(UIEvent e)
	{
		OnNavigateBack();
	}

	private void OnMenuOpened()
	{
		if (m_shown)
		{
			Hide(animate: false);
		}
	}
}
