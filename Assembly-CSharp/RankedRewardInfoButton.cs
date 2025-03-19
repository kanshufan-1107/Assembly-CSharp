using System.Collections;
using Hearthstone.DataModels;
using Hearthstone.UI;
using UnityEngine;

[CustomEditClass]
[RequireComponent(typeof(WidgetTemplate))]
public class RankedRewardInfoButton : MonoBehaviour
{
	public Clickable m_buttonClickable;

	public UberText m_buttonText;

	[CustomEditField(Sections = "Reward List")]
	public Vector3_MobileOverride m_rewardListPos;

	[CustomEditField(Sections = "Reward List")]
	public Float_MobileOverride m_rewardListDeviceScale = new Float_MobileOverride(1f);

	[CustomEditField(Sections = "Reward List")]
	public float m_rewardListScaleSmall;

	[CustomEditField(Sections = "Reward List")]
	public float m_rewardListScaleWide;

	[CustomEditField(Sections = "Reward List")]
	public float m_rewardListScaleExtraWide;

	private Widget m_widget;

	private WidgetInstance m_rankedRewardListWidget;

	private MedalInfoTranslator m_medalInfo;

	private TranslatedMedalInfo m_currentMedal;

	private long m_lastRewardsVersionSeen;

	private bool m_isShowingRewardsList;

	private TooltipZone m_tooltipZone;

	private bool IsReady
	{
		get
		{
			if (m_widget != null && m_widget.IsReady)
			{
				return !m_widget.IsChangingStates;
			}
			return false;
		}
	}

	private void Awake()
	{
		m_widget = GetComponent<WidgetTemplate>();
		m_widget.RegisterEventListener(WidgetEventListener);
		m_tooltipZone = GetComponent<TooltipZone>();
	}

	private void OnDestroy()
	{
		DestroyRankedRewardsList();
	}

	public void Initialize(MedalInfoTranslator mit)
	{
		if (mit != null)
		{
			m_medalInfo = mit;
			m_currentMedal = m_medalInfo.GetCurrentMedal(m_medalInfo.GetBestCurrentRankFormatType());
			bool isTooltipEnabled = false;
			bool hasEarnedCardBack = m_medalInfo.GetSeasonCardBackWinsRemaining() == 0;
			m_currentMedal.starLevel = m_currentMedal.bestStarLevel;
			RankedPlayDataModel rankedPlayDataModel = m_currentMedal.CreateDataModel(RankedMedal.DisplayMode.Chest, isTooltipEnabled, hasEarnedCardBack);
			m_widget.BindDataModel(rankedPlayDataModel);
			InitButtonText(rankedPlayDataModel);
			GameSaveDataManager.Get().GetSubkeyValue(GameSaveKeyId.RANKED_PLAY, GameSaveKeySubkeyId.RANKED_PLAY_LAST_REWARDS_VERSION_SEEN, out m_lastRewardsVersionSeen);
		}
	}

	public void Show()
	{
		StartCoroutine(ShowWhenReady());
	}

	private IEnumerator ShowWhenReady()
	{
		while (!IsReady)
		{
			yield return null;
		}
		m_widget.Show();
	}

	private bool HasSeenLatestRewardsVersion()
	{
		return m_lastRewardsVersionSeen >= m_currentMedal.LeagueConfig.RewardsVersion;
	}

	private void WidgetEventListener(string eventName)
	{
		if (eventName.Equals("OnClickRewardQuestLogButton"))
		{
			ShowRankedRewardList();
		}
		else if (eventName.Equals("RollOver"))
		{
			OnRollOver();
		}
		else if (eventName.Equals("RollOut"))
		{
			OnRollOut();
		}
	}

	private void WidgetEventListener_RewardsList(string eventName)
	{
		if (eventName.Equals("HIDE"))
		{
			HideRankedRewardsList();
		}
	}

	private void HideRankedRewardsList()
	{
		if (m_rankedRewardListWidget != null)
		{
			UIContext.GetRoot().DismissPopup(m_rankedRewardListWidget.gameObject);
		}
		m_isShowingRewardsList = false;
		m_buttonClickable.Active = true;
	}

	private void ShowRankedRewardList()
	{
		if (m_isShowingRewardsList)
		{
			return;
		}
		m_isShowingRewardsList = true;
		m_buttonClickable.Active = false;
		if (m_rankedRewardListWidget == null)
		{
			m_rankedRewardListWidget = WidgetInstance.Create(RankMgr.RANKED_REWARD_LIST_POPUP);
			m_rankedRewardListWidget.WillLoadSynchronously = true;
			OverlayUI.Get().AddGameObject(m_rankedRewardListWidget.gameObject);
			UIContext.GetRoot().ShowPopup(m_rankedRewardListWidget.gameObject);
			m_rankedRewardListWidget.transform.localPosition = m_rewardListPos;
			float rewardListScale = TransformUtil.GetAspectRatioDependentValue(m_rewardListScaleSmall, m_rewardListScaleWide, m_rewardListScaleExtraWide);
			m_rankedRewardListWidget.transform.localScale = Vector3.one * rewardListScale * m_rewardListDeviceScale;
			m_rankedRewardListWidget.RegisterReadyListener(delegate
			{
				RankedRewardList componentInChildren = m_rankedRewardListWidget.GetComponentInChildren<RankedRewardList>();
				if (componentInChildren != null)
				{
					componentInChildren.Initialize(m_medalInfo);
					m_rankedRewardListWidget.TriggerEvent("SHOW");
				}
			});
			m_rankedRewardListWidget.RegisterEventListener(WidgetEventListener_RewardsList);
		}
		else
		{
			UIContext.GetRoot().ShowPopup(m_rankedRewardListWidget.gameObject);
			m_rankedRewardListWidget.TriggerEvent("SHOW");
		}
		if (!HasSeenLatestRewardsVersion())
		{
			m_lastRewardsVersionSeen = m_currentMedal.LeagueConfig.RewardsVersion;
			GameSaveDataManager.Get().SaveSubkey(new GameSaveDataManager.SubkeySaveRequest(GameSaveKeyId.RANKED_PLAY, GameSaveKeySubkeyId.RANKED_PLAY_LAST_REWARDS_VERSION_SEEN, m_lastRewardsVersionSeen));
		}
	}

	private void DestroyRankedRewardsList()
	{
		if (m_rankedRewardListWidget != null)
		{
			UIContext.GetRoot().DismissPopup(m_rankedRewardListWidget.gameObject);
			Object.Destroy(m_rankedRewardListWidget.gameObject);
		}
		m_isShowingRewardsList = false;
	}

	private void InitButtonText(RankedPlayDataModel rankedPlayDataModel)
	{
		if (!rankedPlayDataModel.HasEarnedCardBack)
		{
			m_buttonText.Text = GameStrings.Format("GLUE_RANKED_REWARD_QUEST_LOG_CARDBACK_PROGRESS", m_medalInfo.GetSeasonCardBackWinsRemaining());
			return;
		}
		if (rankedPlayDataModel.IsNewPlayer)
		{
			int leagueId = m_currentMedal.leagueId;
			int nextNewPlayerRewardStarLevel = rankedPlayDataModel.StarLevel - 1;
			nextNewPlayerRewardStarLevel -= nextNewPlayerRewardStarLevel % 5;
			nextNewPlayerRewardStarLevel += 5;
			nextNewPlayerRewardStarLevel++;
			if (nextNewPlayerRewardStarLevel < RankMgr.Get().GetMaxStarLevel(leagueId))
			{
				LeagueRankDbfRecord nextRewardRankRecord = RankMgr.Get().GetLeagueRankRecord(leagueId, nextNewPlayerRewardStarLevel);
				m_buttonText.Text = GameStrings.Format("GLUE_RANKED_REWARD_QUEST_LOG_LABEL_RANK_REWARD", nextRewardRankRecord.MedalText.GetString());
			}
			else
			{
				m_buttonText.Text = "";
			}
			return;
		}
		int leagueId2 = m_currentMedal.leagueId;
		int starLevelMax = RankMgr.Get().GetMaxStarLevel(leagueId2);
		int starLevel = 1;
		LeagueRankDbfRecord firstRewardRankRecord = null;
		bool hasAlreadyEarnedFirstReward = false;
		for (; starLevel < starLevelMax; starLevel++)
		{
			LeagueRankDbfRecord rankRecord = RankMgr.Get().GetLeagueRankRecord(leagueId2, starLevel);
			if (rankRecord.RewardBagId != 0)
			{
				if (rankedPlayDataModel.StarLevel >= starLevel)
				{
					hasAlreadyEarnedFirstReward = true;
				}
				else
				{
					firstRewardRankRecord = rankRecord;
				}
				break;
			}
		}
		if (!hasAlreadyEarnedFirstReward && firstRewardRankRecord != null)
		{
			m_buttonText.Text = GameStrings.Format("GLUE_RANKED_REWARD_QUEST_LOG_LABEL_RANK_REQUIRED", firstRewardRankRecord.RankName.GetString());
		}
		else
		{
			m_buttonText.Text = "";
		}
	}

	private void OnRollOver()
	{
		m_tooltipZone.ShowLayerTooltip(GameStrings.Get("GLOBAL_PROGRESSION_RANKED_REWARDS_TOOLTIP_TITLE"), GameStrings.Get("GLOBAL_PROGRESSION_RANKED_REWARDS_TOOLTIP"));
	}

	private void OnRollOut()
	{
		m_tooltipZone.HideTooltip();
	}
}
