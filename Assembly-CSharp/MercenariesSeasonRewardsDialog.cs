using System;
using System.Collections;
using System.Collections.Generic;
using Hearthstone.DataModels;
using Hearthstone.UI;
using HutongGames.PlayMaker;
using UnityEngine;

public class MercenariesSeasonRewardsDialog : DialogBase
{
	public class Info
	{
		public long m_noticeId;

		public List<RewardData> m_rewards;

		public int m_rewardAssetId;

		public Action m_doneCallback;
	}

	public AsyncReference m_chestWidgetReference;

	public Transform m_rewardBoxesBone;

	public UberText m_footer;

	private Info m_info;

	private Widget m_chestWidget;

	private bool m_chestOpened;

	private List<RewardData> m_boxRewards;

	private Queue<RewardData> m_bannerRewards;

	private void Start()
	{
		m_chestWidgetReference.RegisterReadyListener(delegate(Widget w)
		{
			m_chestWidget = w;
		});
	}

	public void SetInfo(Info info)
	{
		m_info = info;
	}

	public override void Show()
	{
		StartCoroutine(ShowWhenReady());
	}

	private IEnumerator ShowWhenReady()
	{
		while (m_chestWidget == null || !m_chestWidget.IsReady)
		{
			yield return null;
		}
		m_chestWidget.RegisterEventListener(ChestEventListener);
		List<MercenariesRankedSeasonRewardRankDbfRecord> sortedRewardRecords = LettucePlayDisplay.SortedRewardRecords;
		int highIndex = sortedRewardRecords.FindIndex((MercenariesRankedSeasonRewardRankDbfRecord r) => r.ID == m_info.m_rewardAssetId);
		int threshold = sortedRewardRecords[highIndex].MinPublicRatingUnlock;
		m_chestWidget.BindDataModel(new LettucePlayDisplayDataModel
		{
			HighRatingTierIndex = highIndex,
			Rating = threshold
		});
		int nextHighIndex = highIndex + 1;
		if (nextHighIndex == sortedRewardRecords.Count)
		{
			m_footer.gameObject.SetActive(value: false);
		}
		else
		{
			int nextRating = sortedRewardRecords[nextHighIndex].MinPublicRatingUnlock;
			m_footer.Text = GameStrings.Format("GLUE_LETTUCE_SEASON_RATING_REWARD_FOOTER", nextRating);
		}
		while (m_chestWidget.IsChangingStates)
		{
			yield return null;
		}
		LayerUtils.SetLayer(m_chestWidget.gameObject, base.gameObject.layer, null);
		SeasonEndDialog.FadeEffectsIn();
		base.Show();
		DoShowAnimation();
		UniversalInputManager.Get().SetGameDialogActive(active: true);
		SeasonEndDialog.PlayShowSound();
	}

	public override void Hide()
	{
		base.Hide();
		SeasonEndDialog.FadeEffectsOut();
		SeasonEndDialog.PlayHideSound();
	}

	private void ChestEventListener(string eventName)
	{
		if (eventName == "MERC_REWARD_3D_CHEST_CLICKED" && !m_chestOpened)
		{
			m_chestOpened = true;
			PlayMakerFSM componentInChildren = m_chestWidget.GetComponentInChildren<PlayMakerFSM>();
			FsmGameObject ownerObjectVar = componentInChildren.FsmVariables.GetFsmGameObject("OwnerObject");
			if (ownerObjectVar != null)
			{
				ownerObjectVar.Value = base.gameObject;
			}
			componentInChildren.SendEvent("StartAnim");
		}
	}

	private void OpenRewards()
	{
		m_boxRewards = new List<RewardData>(m_info.m_rewards.Count);
		m_bannerRewards = new Queue<RewardData>();
		foreach (RewardData rewardData in m_info.m_rewards)
		{
			if (rewardData.RewardType != Reward.Type.REWARD_ITEM && rewardData.RewardType != Reward.Type.MERCENARY_MERCENARY && rewardData.RewardType != Reward.Type.MERCENARY_KNOCKOUT && rewardData.RewardType != Reward.Type.MERCENARY_RANDOM_MERCENARY && rewardData.RewardType != Reward.Type.MERCENARY_EQUIPMENT)
			{
				m_boxRewards.Add(rewardData);
			}
			else
			{
				m_bannerRewards.Enqueue(rewardData);
			}
		}
		RewardUtils.ShowRewardBoxes(m_boxRewards, RewardBoxesDoneCallback, m_rewardBoxesBone, useLocalPosition: true, GameLayer.PerspectiveUI, useDarkeningClickCatcher: true);
	}

	private void RewardBoxesDoneCallback()
	{
		if (m_bannerRewards.Count > 0)
		{
			RewardData bannerRewardData = m_bannerRewards.Dequeue();
			QuestToast.ShowGenericRewardQuestToast(UserAttentionBlocker.NONE, delegate
			{
				RewardBoxesDoneCallback();
			}, null, bannerRewardData, bannerRewardData.NameOverride, bannerRewardData.DescriptionOverride, fullscreenEffects: false);
		}
		else
		{
			AckAndHide();
		}
	}

	private void AckAndHide()
	{
		Network.Get().AckNotice(m_info.m_noticeId);
		Hide();
	}

	protected override void DoShowAnimation()
	{
		m_showAnimState = ShowAnimState.IN_PROGRESS;
		AnimationUtil.ShowWithPunch(base.gameObject, START_SCALE, Vector3.Scale(PUNCH_SCALE, m_originalScale), m_originalScale, "OnShowAnimFinished", noFade: true);
	}

	protected override void OnHideAnimFinished()
	{
		UniversalInputManager.Get().SetGameDialogActive(active: false);
		base.OnHideAnimFinished();
		m_info?.m_doneCallback?.Invoke();
	}
}
