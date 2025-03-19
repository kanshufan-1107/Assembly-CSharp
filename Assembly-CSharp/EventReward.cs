using UnityEngine;

public class EventReward : Reward
{
	protected override void InitData()
	{
		SetData(new EventRewardData(), updateVisuals: false);
	}

	protected override void ShowReward(bool updateCacheValues)
	{
		m_root.SetActive(value: true);
	}

	protected override void HideReward()
	{
		base.HideReward();
		m_root.SetActive(value: false);
	}

	protected override void OnDataSet(bool updateVisuals)
	{
		if (!updateVisuals)
		{
			return;
		}
		if (!(base.Data is EventRewardData eventRewardData))
		{
			Debug.LogWarning($"EventRewardData.SetData() - data {base.Data} is not EventRewardData");
			return;
		}
		string rewardHeadline = string.Empty;
		if (eventRewardData.EventType == 0)
		{
			rewardHeadline = GameStrings.Get("GLUE_2X_GOLD_EVENT_BANNER_HEADLINE");
		}
		string rewardDetails = string.Empty;
		string rewardSource = string.Empty;
		SetRewardText(rewardHeadline, rewardDetails, rewardSource);
	}
}
