using System;
using System.Collections.Generic;
using Hearthstone.DataModels;
using Hearthstone.UI;

namespace Hearthstone.Progression;

public class RewardPresenter
{
	public static readonly AssetReference REWARD_PREFAB = new AssetReference("RewardScroll.prefab:ccd55a2608a26544da63232e330ad1d5");

	private const string HIDE = "CODE_HIDE";

	private const int MAX_REWARDS_PER_SCROLL = 4;

	public AssetReference m_rewardPrefab = REWARD_PREFAB;

	private readonly Queue<(RewardScrollDataModel, Action)> m_rewardsToShow = new Queue<(RewardScrollDataModel, Action)>();

	private bool m_isShowingReward;

	public event Action<int> OnRewardItemQueued;

	public bool HasReward()
	{
		return m_rewardsToShow.Count > 0;
	}

	public bool ShowNextReward(Action onHiddenCallback)
	{
		if (m_rewardsToShow.Count == 0)
		{
			return false;
		}
		m_isShowingReward = true;
		(RewardScrollDataModel, Action) rewardParams = m_rewardsToShow.Dequeue();
		CreateRewardPrefab(rewardParams.Item1, delegate
		{
			rewardParams.Item2?.Invoke();
			onHiddenCallback?.Invoke();
			m_isShowingReward = false;
		});
		return true;
	}

	public void EnqueueReward(RewardScrollDataModel dataModel, Action acknowledge)
	{
		if (dataModel == null || dataModel.RewardList == null)
		{
			return;
		}
		if (dataModel.RewardList.Items.Count > 4)
		{
			List<RewardScrollDataModel> rewardScrollDataModelList = SplitRewardScrollDataModel(dataModel, 4);
			for (int i = 0; i < rewardScrollDataModelList.Count; i++)
			{
				(RewardScrollDataModel, Action) rewardParams = (rewardScrollDataModelList[i], acknowledge);
				m_rewardsToShow.Enqueue(rewardParams);
			}
		}
		else
		{
			(RewardScrollDataModel, Action) rewardParams2 = (dataModel, acknowledge);
			m_rewardsToShow.Enqueue(rewardParams2);
		}
		if (this.OnRewardItemQueued == null)
		{
			return;
		}
		foreach (RewardItemDataModel rewardItem in dataModel.RewardList.Items)
		{
			this.OnRewardItemQueued(rewardItem.AssetId);
		}
	}

	public void Clear()
	{
		m_rewardsToShow.Clear();
	}

	public bool IsShowingReward()
	{
		return m_isShowingReward;
	}

	private List<RewardScrollDataModel> SplitRewardScrollDataModel(RewardScrollDataModel dataModel, int size)
	{
		DataModelList<RewardItemDataModel> dataModelList = dataModel.RewardList.Items;
		DataModelList<RewardItemDataModel> temp = new DataModelList<RewardItemDataModel>();
		List<RewardScrollDataModel> rewardScrollDataModelList = new List<RewardScrollDataModel>();
		int i = 0;
		foreach (RewardItemDataModel rewardItem in dataModelList)
		{
			temp.Add(rewardItem);
			if (++i == size || dataModelList.IndexOf(rewardItem) == dataModelList.Count - 1)
			{
				RewardScrollDataModel cloneModel = dataModel.CloneDataModel();
				cloneModel.RewardList = dataModel.RewardList.CloneDataModel();
				cloneModel.RewardList.Items = temp;
				rewardScrollDataModelList.Add(cloneModel);
				temp = new DataModelList<RewardItemDataModel>();
				i = 0;
			}
		}
		return rewardScrollDataModelList;
	}

	private void CreateRewardPrefab(RewardScrollDataModel rewardScrollDataModel, Action onHiddenCallback)
	{
		Widget rewardWidget = WidgetInstance.Create(m_rewardPrefab);
		rewardWidget.BindDataModel(rewardScrollDataModel);
		rewardWidget.RegisterDoneChangingStatesListener(delegate
		{
			RewardScroll componentInChildren = rewardWidget.GetComponentInChildren<RewardScroll>();
			componentInChildren.Initialize(delegate
			{
				onHiddenCallback?.Invoke();
			});
			componentInChildren.Show();
		}, null, callImmediatelyIfSet: true, doOnce: true);
	}
}
