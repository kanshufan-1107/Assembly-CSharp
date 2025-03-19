using System.Collections;
using UnityEngine;

public class BattlegroundsTokenReward : Reward
{
	public GameObject m_coin;

	public bool m_RotateIn = true;

	protected override void InitData()
	{
		SetData(new BattlegroundsTokenRewardData(), updateVisuals: false);
	}

	protected override void ShowReward(bool updateCacheValues)
	{
		m_root.SetActive(value: true);
		Vector3 endScale = m_coin.transform.localScale;
		m_coin.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
		iTween.ScaleTo(m_coin.gameObject, iTween.Hash("scale", endScale, "time", 0.5f, "easetype", iTween.EaseType.easeOutElastic));
		if (m_RotateIn)
		{
			m_coin.transform.localEulerAngles = new Vector3(0f, 180f, 180f);
			Hashtable args = iTweenManager.Get().GetTweenHashTable();
			args.Add("amount", new Vector3(0f, 0f, 540f));
			args.Add("time", 1.5f);
			args.Add("easetype", iTween.EaseType.easeOutElastic);
			args.Add("space", Space.Self);
			iTween.RotateAdd(m_coin.gameObject, args);
		}
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
		if (!(base.Data is BattlegroundsTokenRewardData battlegroundsTokenRewardData))
		{
			Debug.LogWarning($"BattlegroundsTokenRewardData.SetData() - data {base.Data} is not GoldRewardData");
			return;
		}
		string rewardHeadline = GameStrings.Get("GLOBAL_REWARD_BG_TOKEN_HEADLINE");
		string rewardDetails = battlegroundsTokenRewardData.Amount.ToString();
		string rewardSource = string.Empty;
		UberText sackText = m_coin.GetComponentInChildren<UberText>(includeInactive: true);
		if (sackText != null)
		{
			m_rewardBanner.m_detailsText = sackText;
			m_rewardBanner.AlignHeadlineToCenterBone();
		}
		switch (base.Data.Origin)
		{
		case NetCache.ProfileNotice.NoticeOrigin.BETA_REIMBURSE:
			rewardHeadline = GameStrings.Get("GLOBAL_BETA_REIMBURSEMENT_HEADLINE");
			rewardSource = GameStrings.Get("GLOBAL_BETA_REIMBURSEMENT_DETAILS");
			break;
		case NetCache.ProfileNotice.NoticeOrigin.IGR:
			if (battlegroundsTokenRewardData.Date.HasValue)
			{
				string date = GameStrings.Format("GLOBAL_CURRENT_DATE", battlegroundsTokenRewardData.Date);
				rewardSource = GameStrings.Format("GLOBAL_REWARD_BG_TOKEN_SOURCE_IGR_DATED", date);
			}
			else
			{
				rewardSource = GameStrings.Get("GLOBAL_REWARD_BG_TOKEN_SOURCE_IGR");
			}
			break;
		}
		SetRewardText(rewardHeadline, rewardDetails, rewardSource);
	}
}
