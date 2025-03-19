using System;
using System.Collections;
using Blizzard.GameService.SDK.Client.Integration;
using Blizzard.T5.MaterialService.Extensions;
using Hearthstone.Core;
using Hearthstone.DataModels;
using Hearthstone.Progression;
using Hearthstone.UI;
using PegasusShared;
using SpectatorProto;
using UnityEngine;

public class FriendlyChallengeDialog : DialogBase
{
	public delegate void ResponseCallback(bool accept);

	public class Info
	{
		public FormatType m_formatType;

		public BnetPlayer m_challenger;

		public PartyType m_partyType;

		public PartyQuestInfo m_questInfo;

		public ResponseCallback m_callback;
	}

	public UberText m_challengeText;

	public UberText m_challengerName;

	public UIBButton m_acceptButton;

	public UIBButton m_denyButton;

	public UberText m_nearbyPlayerNote;

	public float m_friendQuestSliderSoundDelay;

	public string m_friendQuestSliderSound;

	public float m_friendQuestSliderSoundDelay2;

	public string m_friendQuestSliderSound2;

	public GameObject m_friendQuestContainer;

	public GameObject m_dropShadow;

	public GameObject m_twistFormatIcon;

	public GameObject m_standardFormatIcon;

	public GameObject m_wildFormatIcon;

	public GameObject m_tavernBrawlIcon;

	private ResponseCallback m_responseCallback;

	private Achievement m_quest;

	private FriendlyChallengeQuestFrame m_friendlyQuestFrame;

	private PartyQuestInfo m_partyQuestInfo;

	private Widget m_questTileWidget;

	private const float NAME_LINE_PADDING = 0.01f;

	private void Start()
	{
		m_acceptButton.AddEventListener(UIEventType.RELEASE, ConfirmButtonPress);
		m_denyButton.AddEventListener(UIEventType.RELEASE, CancelButtonPress);
	}

	public override void Show()
	{
		base.Show();
		BnetBar.Get().DisableButtonsByDialog(this);
		if ((bool)UniversalInputManager.UsePhoneUI && m_nearbyPlayerNote.gameObject.activeSelf)
		{
			base.transform.localPosition = new Vector3(base.transform.localPosition.x, base.transform.localPosition.y + 50f, base.transform.localPosition.z);
		}
		DoShowAnimation();
		UniversalInputManager.Get().SetSystemDialogActive(active: true);
		SoundManager.Get().LoadAndPlay("friendly_challenge.prefab:649e070117bcd0d45bac691a03bf2dec");
		if (m_partyQuestInfo != null)
		{
			Processor.ScheduleCallback(m_friendQuestSliderSoundDelay, realTime: false, delegate
			{
				SoundManager.Get().LoadAndPlay(m_friendQuestSliderSound);
			});
			Processor.ScheduleCallback(m_friendQuestSliderSoundDelay2, realTime: false, delegate
			{
				SoundManager.Get().LoadAndPlay(m_friendQuestSliderSound2);
			});
		}
	}

	public override void Hide()
	{
		base.Hide();
		SoundManager.Get().LoadAndPlay("banner_shrink.prefab:d9de7386a7f2017429d126e972232123");
		Hashtable args = iTweenManager.Get().GetTweenHashTable();
		args.Add("amount", 0f);
		args.Add("time", 1f);
		iTween.FadeTo(m_dropShadow, args);
	}

	public override bool HandleKeyboardInput()
	{
		if (InputCollection.GetKeyUp(KeyCode.Escape))
		{
			CancelButtonPress(null);
			return true;
		}
		return false;
	}

	public void SetInfo(Info info)
	{
		string challengeTextKey = "GLOBAL_FRIEND_CHALLENGE_BODY1";
		ResetChallengeFormatIcon();
		if (FriendChallengeMgr.Get().IsChallengeTavernBrawl())
		{
			if (m_tavernBrawlIcon != null)
			{
				m_tavernBrawlIcon.SetActive(value: true);
			}
			challengeTextKey = "GLOBAL_FRIEND_CHALLENGE_TAVERN_BRAWL_BODY1";
		}
		else if (FriendChallengeMgr.Get().IsChallengeBacon() || info.m_partyType == PartyType.BATTLEGROUNDS_PARTY)
		{
			challengeTextKey = "GLOBAL_FRIEND_CHALLENGE_BODY_BACON";
		}
		else if (CollectionManager.Get().ShouldAccountSeeStandardWild())
		{
			if (info.m_formatType == FormatType.FT_STANDARD)
			{
				challengeTextKey = "GLOBAL_FRIEND_CHALLENGE_BODY1_STANDARD";
				if (m_standardFormatIcon != null)
				{
					m_standardFormatIcon.SetActive(value: true);
				}
			}
			else if (info.m_formatType == FormatType.FT_WILD)
			{
				challengeTextKey = "GLOBAL_FRIEND_CHALLENGE_BODY1_WILD";
				if (m_wildFormatIcon != null)
				{
					m_wildFormatIcon.SetActive(value: true);
				}
			}
			else if (info.m_formatType == FormatType.FT_TWIST)
			{
				if (m_twistFormatIcon != null)
				{
					m_twistFormatIcon.SetActive(value: true);
				}
				challengeTextKey = "GLOBAL_FRIEND_CHALLENGE_BODY1_TWIST";
			}
		}
		m_challengeText.Text = GameStrings.Get(challengeTextKey);
		m_challengerName.Text = FriendUtils.GetUniqueName(info.m_challenger);
		m_responseCallback = info.m_callback;
		bool isNearbyStranger = BnetNearbyPlayerMgr.Get().IsNearbyStranger(info.m_challenger);
		m_nearbyPlayerNote.gameObject.SetActive(isNearbyStranger);
		if (info.m_partyType != PartyType.BATTLEGROUNDS_PARTY && info.m_questInfo != null)
		{
			SetQuestInfo(info.m_questInfo);
		}
	}

	public void ResetChallengeFormatIcon()
	{
		if (m_twistFormatIcon != null)
		{
			m_twistFormatIcon.SetActive(value: false);
		}
		if (m_standardFormatIcon != null)
		{
			m_standardFormatIcon.SetActive(value: false);
		}
		if (m_wildFormatIcon != null)
		{
			m_wildFormatIcon.SetActive(value: false);
		}
		if (m_tavernBrawlIcon != null)
		{
			m_tavernBrawlIcon.SetActive(value: false);
		}
	}

	public Achievement GetQuest()
	{
		return m_quest;
	}

	public void SetQuestInfo(PartyQuestInfo info)
	{
		if (m_friendQuestContainer == null)
		{
			return;
		}
		m_partyQuestInfo = info;
		if (info == null || info.QuestIds.Count == 0)
		{
			m_friendQuestContainer.gameObject.SetActive(value: false);
			return;
		}
		bool rewardLimitReached = false;
		foreach (int questId in info.QuestIds)
		{
			AchieveDbfRecord achieveRecord = GameDbf.Achieve.GetRecord(questId);
			if (achieveRecord == null || achieveRecord.SharedAchieveId == 0)
			{
				continue;
			}
			Achievement sharedAchieve = AchieveManager.Get().GetAchievement(achieveRecord.SharedAchieveId);
			if (sharedAchieve == null)
			{
				continue;
			}
			AchieveRegionDataDbfRecord regionData = sharedAchieve.GetCurrentRegionData();
			if (regionData != null && regionData.RewardableLimit > 0 && sharedAchieve.IntervalRewardStartDate > 0)
			{
				DateTime intervalStart = DateTime.FromFileTimeUtc(sharedAchieve.IntervalRewardStartDate);
				if ((DateTime.UtcNow - intervalStart).TotalDays < regionData.RewardableInterval && sharedAchieve.IntervalRewardCount >= regionData.RewardableLimit)
				{
					rewardLimitReached = true;
				}
			}
		}
		m_friendlyQuestFrame = ((m_friendQuestContainer != null) ? m_friendQuestContainer.GetComponentInChildren<FriendlyChallengeQuestFrame>() : null);
		if (m_friendlyQuestFrame != null && m_friendlyQuestFrame.m_questTileBone != null)
		{
			if (rewardLimitReached)
			{
				m_friendlyQuestFrame.m_noGoldRewardText.Text = GameStrings.Get("GLOBAL_FRIENDLYCHALLENGE_QUEST_REWARD_AT_LIMIT");
				m_friendlyQuestFrame.m_questName.Hide();
				m_friendlyQuestFrame.m_questDesc.Hide();
				m_friendlyQuestFrame.m_nameLine.gameObject.SetActive(value: false);
				m_friendlyQuestFrame.m_rewardMesh.gameObject.SetActive(value: false);
				m_friendlyQuestFrame.m_rewardAmountLabel.Hide();
				m_friendQuestContainer.gameObject.SetActive(value: true);
				SlidingTray slidingTray = m_friendQuestContainer.GetComponent<SlidingTray>();
				if (slidingTray != null)
				{
					slidingTray.ShowTray();
				}
			}
			else
			{
				m_questTileWidget = WidgetInstance.Create(Hearthstone.Progression.QuestTile.QUEST_TILE_WIDGET_ASSET);
				GameUtils.SetParent(m_questTileWidget, m_friendlyQuestFrame.m_questTileBone);
				m_questTileWidget.SetLayerOverride((GameLayer)base.gameObject.layer);
				StartCoroutine(ShowWhenReady(info));
			}
		}
		else
		{
			Debug.LogError("FriendlyChallegeDialog.Start - QuestTileWidget is not set!");
		}
	}

	private void SetQuestInfo_OnLoadRewardObject(Reward reward, object callbackData)
	{
		if (!(m_friendlyQuestFrame.m_rewardBone == null))
		{
			reward.transform.SetParent(m_friendlyQuestFrame.m_rewardBone.transform);
			reward.transform.localPosition = Vector3.zero;
			bool doubleGoldEventActive = m_quest != null && m_quest.IsAffectedByDoubleGold && EventTimingManager.Get().IsEventActive(EventTimingType.SPECIAL_EVENT_GOLD_DOUBLED);
			RewardUtils.SetupRewardIcon(m_quest.Rewards[0], m_friendlyQuestFrame.m_rewardMesh, m_friendlyQuestFrame.m_rewardAmountLabel, out var amountToScaleReward, doubleGoldEventActive);
			m_friendlyQuestFrame.m_rewardMesh.transform.localScale *= amountToScaleReward;
			m_friendlyQuestFrame.m_rewardAmountLabel.RenderQueue = m_friendlyQuestFrame.m_rewardMesh.GetMaterial().renderQueue;
		}
	}

	private void ConfirmButtonPress(UIEvent e)
	{
		SoundManager.Get().LoadAndPlay("Small_Click.prefab:2a1c5335bf08dc84eb6e04fc58160681");
		if (m_responseCallback != null)
		{
			m_responseCallback(accept: true);
		}
		Hide();
	}

	private void CancelButtonPress(UIEvent e)
	{
		SoundManager.Get().LoadAndPlay("Small_Click.prefab:2a1c5335bf08dc84eb6e04fc58160681");
		if (m_responseCallback != null)
		{
			m_responseCallback(accept: false);
		}
		Hide();
	}

	private IEnumerator ShowWhenReady(PartyQuestInfo info)
	{
		int friendlyChallengeeQuestId = 114;
		QuestDataModel dataModel = QuestManager.Get().CreateQuestDataModelById(friendlyChallengeeQuestId);
		dataModel.RerollCount = 0;
		if (m_questTileWidget != null)
		{
			m_questTileWidget.BindDataModel(dataModel);
			m_questTileWidget.TriggerEvent("DISABLE_INTERACTION");
		}
		m_friendQuestContainer.gameObject.SetActive(value: true);
		while (m_questTileWidget != null && (!m_questTileWidget.IsReady || m_questTileWidget.IsChangingStates))
		{
			yield return null;
		}
		SlidingTray slidingTray = m_friendQuestContainer.GetComponent<SlidingTray>();
		if (slidingTray != null)
		{
			slidingTray.ShowTray();
		}
		Hashtable args = iTweenManager.Get().GetTweenHashTable();
		args.Add("amount", 1f);
		args.Add("time", 1f);
		iTween.FadeTo(m_dropShadow, args);
		if (m_friendlyQuestFrame != null)
		{
			m_friendlyQuestFrame.m_nameLine.SetActive(value: false);
			m_friendlyQuestFrame.m_questDesc.Hide();
			m_friendlyQuestFrame.m_rewardAmountLabel.Hide();
			m_friendlyQuestFrame.m_rewardMesh.gameObject.SetActive(value: false);
		}
	}
}
