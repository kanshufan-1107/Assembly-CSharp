using System;
using System.Collections.Generic;
using Assets;
using Hearthstone.DataModels;
using Hearthstone.Progression;
using Hearthstone.UI;
using PegasusShared;
using PegasusUtil;
using UnityEngine;

[RequireComponent(typeof(WidgetTemplate))]
public class RewardTrackSeasonRoll : MonoBehaviour
{
	public static readonly AssetReference REWARD_TRACK_SEASON_ROLL_PREFAB = new AssetReference("RewardTrackSeasonRoll.prefab:896a446794e9b334d937e067e63613b0");

	private const string CODE_HIDE_AUTO_CLAIMED_REWARDS_POPUP = "CODE_HIDE_AUTO_CLAIMED_REWARDS_POPUP";

	private const string CODE_DISMISS = "CODE_DISMISS";

	public Widget m_forgotGlobalRewardsPopupWidget;

	public Widget m_forgotBattlegroundRewardsPopupWidget;

	public Widget m_forgotEventRewardsPopupWidget;

	public Widget m_chooseOneItemPopupWidget;

	private Widget m_widget;

	private GameObject m_owner;

	private Action m_callback;

	private RewardTrackUnclaimedRewards m_rewardTrackUnclaimedNotification;

	private RewardTrackDataModel m_rewardTrackDataModel = new RewardTrackDataModel();

	private Queue<RewardTrackNodeRewardsDataModel> m_unclaimedRewardTrackNodeDataModels = new Queue<RewardTrackNodeRewardsDataModel>();

	private bool m_hasPaidTrackUnlocked;

	private Widget m_currentForgotPopup;

	private void Awake()
	{
		m_widget = GetComponent<WidgetTemplate>();
		m_widget.RegisterEventListener(delegate(string eventName)
		{
			if (eventName == "CODE_HIDE_AUTO_CLAIMED_REWARDS_POPUP")
			{
				ShowChooseOneRewardPickerPopup();
			}
			else if (eventName == "CODE_DISMISS")
			{
				Hide();
			}
		});
		m_owner = base.gameObject;
		if (base.transform.parent != null && base.transform.parent.GetComponent<WidgetInstance>() != null)
		{
			m_owner = base.transform.parent.gameObject;
		}
	}

	public void Initialize(Action callback, RewardTrackUnclaimedRewards rewardTrackUnclaimedRewards)
	{
		m_callback = callback;
		m_rewardTrackUnclaimedNotification = rewardTrackUnclaimedRewards;
		RewardTrackDbfRecord rewardTrackAsset = GameDbf.RewardTrack.GetRecord(rewardTrackUnclaimedRewards.RewardTrackId);
		m_hasPaidTrackUnlocked = AccountLicenseMgr.Get().OwnsAccountLicense((rewardTrackAsset?.AccountLicenseRecord?.LicenseId).GetValueOrDefault());
		m_rewardTrackDataModel.RewardTrackId = rewardTrackUnclaimedRewards.RewardTrackId;
		m_rewardTrackDataModel.Name = rewardTrackAsset.Name?.GetString() ?? string.Empty;
		m_rewardTrackDataModel.RewardTrackType = (Global.RewardTrackType)rewardTrackAsset.RewardTrackType;
		m_rewardTrackDataModel.Level = int.MaxValue;
		foreach (PlayerRewardTrackLevelState levelState in rewardTrackUnclaimedRewards.UnclaimedLevel)
		{
			RewardTrackPaidType[] allValidRewardTrackPaidType = ProgressUtils.AllValidRewardTrackPaidType;
			foreach (RewardTrackPaidType paidType in allValidRewardTrackPaidType)
			{
				HandleUnclaimedRewardTracklevel(levelState, rewardTrackUnclaimedRewards.RewardTrackId, paidType);
			}
		}
		m_widget.BindDataModel(m_rewardTrackDataModel);
		if (m_rewardTrackDataModel.RewardTrackType == Global.RewardTrackType.BATTLEGROUNDS)
		{
			m_currentForgotPopup = m_forgotBattlegroundRewardsPopupWidget;
			return;
		}
		if (m_rewardTrackDataModel.RewardTrackType == Global.RewardTrackType.GLOBAL)
		{
			m_currentForgotPopup = m_forgotGlobalRewardsPopupWidget;
			return;
		}
		m_currentForgotPopup = m_forgotEventRewardsPopupWidget;
		SpecialEventDataModel specialEventDataModel = RewardTrackManager.Get().GetEventDataModelFromRewardTrack(m_rewardTrackDataModel);
		if (specialEventDataModel != null)
		{
			m_currentForgotPopup.BindDataModel(specialEventDataModel);
		}
	}

	public void Show()
	{
		OverlayUI.Get().AddGameObject(base.transform.parent.gameObject);
		m_currentForgotPopup.RegisterDoneChangingStatesListener(delegate
		{
			UIContext.GetRoot().ShowPopup(m_owner);
			m_currentForgotPopup.GetComponentInChildren<RewardTrackForgotRewardsPopup>().Show();
		}, null, callImmediatelyIfSet: true, doOnce: true);
	}

	public void ShowChooseOneRewardPickerPopup()
	{
		if (m_unclaimedRewardTrackNodeDataModels.Count == 0)
		{
			Hide();
			return;
		}
		m_chooseOneItemPopupWidget.gameObject.SetActive(value: true);
		RewardTrackNodeRewardsDataModel unclaimedRewardTrackDataModel = m_unclaimedRewardTrackNodeDataModels.Dequeue();
		m_chooseOneItemPopupWidget.BindDataModel(unclaimedRewardTrackDataModel);
		m_chooseOneItemPopupWidget.BindDataModel(unclaimedRewardTrackDataModel.Items);
		m_chooseOneItemPopupWidget.RegisterDoneChangingStatesListener(delegate
		{
			m_widget.GetComponentInChildren<RewardTrackForgotRewardsPopup>().Show();
		}, null, callImmediatelyIfSet: true, doOnce: true);
	}

	public void Hide()
	{
		m_widget.Hide();
		UnityEngine.Object.Destroy(m_owner);
	}

	private void OnDestroy()
	{
		UIContext.GetRoot().DismissPopup(m_owner);
		m_callback?.Invoke();
	}

	private void HandleUnclaimedRewardTracklevel(PlayerRewardTrackLevelState levelState, int rewardTrackId, RewardTrackPaidType paidType)
	{
		RewardTrackDbfRecord rewardTrackRecord = GameDbf.RewardTrack.GetRecord(rewardTrackId);
		if (rewardTrackRecord != null || !ProgressUtils.HasOwnedRewardTrackPaidType(rewardTrackRecord, paidType) || ProgressUtils.HasClaimedRewardTrackReward(ProgressUtils.GetRewardStatus(levelState, paidType)))
		{
			return;
		}
		RewardTrackLevelDbfRecord rewardTrackLevelAsset = GameUtils.GetRewardTrackLevelsForRewardTrack(rewardTrackId).Find((RewardTrackLevelDbfRecord r) => r.Level == levelState.Level);
		if (rewardTrackLevelAsset == null)
		{
			Debug.LogError($"Reward track level asset not found for track id {rewardTrackId} level {levelState.Level}");
			return;
		}
		RewardListDbfRecord rewardListDbfRecord = ProgressUtils.GetRewardListRecord(rewardTrackLevelAsset, paidType);
		if (rewardListDbfRecord == null)
		{
			return;
		}
		if (rewardListDbfRecord.ChooseOne)
		{
			RewardTrackNodeRewardsDataModel unclaimedTrackRewards = RewardTrackFactory.CreateRewardTrackNodeRewardsDataModel(rewardTrackRecord, rewardListDbfRecord, m_rewardTrackDataModel, paidType, levelState, 0);
			m_unclaimedRewardTrackNodeDataModels.Enqueue(unclaimedTrackRewards);
			return;
		}
		foreach (RewardItemDbfRecord rewardItem in rewardListDbfRecord.RewardItems)
		{
			if (rewardItem.RewardType != RewardItem.RewardType.REWARD_TRACK_XP_BOOST)
			{
				RewardTrackDataModel rewardTrackDataModel = m_rewardTrackDataModel;
				int unclaimed = rewardTrackDataModel.Unclaimed + 1;
				rewardTrackDataModel.Unclaimed = unclaimed;
			}
		}
	}

	public static void DebugShowFakeForgotTrackRewards(int trackId = 2, int trackLevel = 50)
	{
		Widget widget = WidgetInstance.Create(REWARD_TRACK_SEASON_ROLL_PREFAB);
		widget.RegisterReadyListener(delegate
		{
			RewardTrackSeasonRoll componentInChildren = widget.GetComponentInChildren<RewardTrackSeasonRoll>();
			RewardTrackUnclaimedRewards rewardTrackUnclaimedRewards = new RewardTrackUnclaimedRewards
			{
				RewardTrackId = trackId
			};
			PlayerRewardTrackLevelState item = new PlayerRewardTrackLevelState
			{
				Level = trackLevel,
				FreeRewardStatus = 0,
				PaidRewardStatus = 2
			};
			rewardTrackUnclaimedRewards.UnclaimedLevel.Add(item);
			componentInChildren.Initialize(null, rewardTrackUnclaimedRewards);
			componentInChildren.Show();
		});
	}
}
