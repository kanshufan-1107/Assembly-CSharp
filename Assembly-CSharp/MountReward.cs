using UnityEngine;

public class MountReward : Reward
{
	public GameObject m_mount;

	protected override void InitData()
	{
		SetData(new MountRewardData(), updateVisuals: false);
	}

	protected override void ShowReward(bool updateCacheValues)
	{
		if (!(base.Data is MountRewardData))
		{
			Debug.LogWarning($"MountReward.ShowReward() - Data {base.Data} is not MountRewardData");
			return;
		}
		m_root.SetActive(value: true);
		Vector3 endScale = m_mount.transform.localScale;
		m_mount.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
		iTween.ScaleTo(m_mount.gameObject, iTween.Hash("scale", endScale, "time", 0.5f, "easetype", iTween.EaseType.easeOutElastic));
	}

	protected override void HideReward()
	{
		base.HideReward();
		m_root.SetActive(value: false);
	}

	protected override void OnDataSet(bool updateVisuals)
	{
		if (updateVisuals && base.Data is MountRewardData mountRewardData)
		{
			string mountHeadline = string.Empty;
			switch (mountRewardData.Mount)
			{
			case MountRewardData.MountType.WOW_HEARTHSTEED:
				mountHeadline = GameStrings.Get("GLOBAL_REWARD_HEARTHSTEED_HEADLINE");
				break;
			case MountRewardData.MountType.HEROES_MAGIC_CARPET_CARD:
				mountHeadline = GameStrings.Get("GLOBAL_REWARD_HEROES_CARD_MOUNT_HEADLINE");
				break;
			case MountRewardData.MountType.WOW_SARGE_TALE:
				mountHeadline = GameStrings.Get("GLOBAL_REWARD_SARGE_TALE_HEADLINE");
				break;
			case MountRewardData.MountType.TEN_YEAR_MOUNT:
				mountHeadline = GameStrings.Get("GLOBAL_REWARD_FIERY_HEART_MOUNT_HEADLINE");
				break;
			}
			SetRewardText(mountHeadline, string.Empty, string.Empty);
		}
	}
}
