using UnityEngine;

public class SimpleReward : Reward
{
	public QuestTileRewardIcon m_icon;

	protected override void InitData()
	{
		SetData(new SimpleRewardData(Type.NONE), updateVisuals: false);
	}

	protected override void ShowReward(bool updateCacheValues)
	{
		if (!(base.Data is SimpleRewardData))
		{
			Debug.LogWarning($"SimpleReward.ShowReward() - Data {base.Data} is not SimpleRewardData");
			return;
		}
		m_root.SetActive(value: true);
		Vector3 endScale = m_icon.transform.localScale;
		m_icon.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
		iTween.ScaleTo(m_icon.gameObject, iTween.Hash("scale", endScale, "time", 0.5f, "easetype", iTween.EaseType.easeOutElastic));
	}

	protected override void HideReward()
	{
		base.HideReward();
		m_root.SetActive(value: false);
	}

	protected override void OnDataSet(bool updateVisuals)
	{
		if (updateVisuals && base.Data is SimpleRewardData { RewardType: not Type.NONE } rewardData)
		{
			SetRewardText(rewardData.RewardHeadlineText, "", "");
			if (m_icon != null)
			{
				m_icon.InitWithRewardData(rewardData, isDoubleGoldEnabled: false, 3000);
			}
		}
	}
}
