using System;
using System.Collections;
using UnityEngine;

[Obsolete("use ArenaTicketReward")]
public class ForgeTicketReward : Reward
{
	public GameObject m_rotateParent;

	protected override void InitData()
	{
		SetData(new ForgeTicketRewardData(), updateVisuals: false);
	}

	protected override void ShowReward(bool updateCacheValues)
	{
		string rewardHeadline = string.Empty;
		string rewardDetails = string.Empty;
		string rewardSource = string.Empty;
		if (base.Data.Origin == NetCache.ProfileNotice.NoticeOrigin.OUT_OF_BAND_LICENSE)
		{
			ForgeTicketRewardData forgeRewardData = base.Data as ForgeTicketRewardData;
			rewardHeadline = GameStrings.Get("GLOBAL_REWARD_FORGE_HEADLINE");
			rewardSource = GameStrings.Format("GLOBAL_REWARD_BOOSTER_DETAILS_OUT_OF_BAND", forgeRewardData.Quantity);
		}
		else
		{
			rewardHeadline = GameStrings.Get("GLOBAL_REWARD_FORGE_UNLOCKED_HEADLINE");
			rewardSource = GameStrings.Get("GLOBAL_REWARD_FORGE_UNLOCKED_SOURCE");
		}
		SetRewardText(rewardHeadline, rewardDetails, rewardSource);
		m_root.SetActive(value: true);
		m_rotateParent.transform.localEulerAngles = new Vector3(0f, 0f, 180f);
		Hashtable args = iTweenManager.Get().GetTweenHashTable();
		args.Add("amount", new Vector3(0f, 0f, 540f));
		args.Add("time", 1.5f);
		args.Add("easetype", iTween.EaseType.easeOutElastic);
		args.Add("space", Space.Self);
		iTween.RotateAdd(m_rotateParent, args);
	}

	protected override void HideReward()
	{
		base.HideReward();
		m_root.SetActive(value: false);
	}
}
