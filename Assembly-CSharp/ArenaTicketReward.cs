using System.Collections;
using UnityEngine;

public class ArenaTicketReward : Reward
{
	public GameObject m_ticketVisual;

	public GameObject m_plusSign;

	public UberText m_countLabel;

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
			rewardSource = GameStrings.Format("GLOBAL_REWARD_FORGE_DETAILS_OUT_OF_BAND", forgeRewardData.Quantity);
		}
		else if (base.Data.Origin == NetCache.ProfileNotice.NoticeOrigin.ACHIEVEMENT && base.Data.OriginData == 56)
		{
			rewardHeadline = GameStrings.Get("GLOBAL_REWARD_FORGE_UNLOCKED_HEADLINE");
			rewardSource = GameStrings.Get("GLOBAL_REWARD_FORGE_UNLOCKED_SOURCE");
		}
		else
		{
			rewardHeadline = GameStrings.Get("GLOBAL_REWARD_ARENA_TICKET_HEADLINE");
		}
		SetRewardText(rewardHeadline, rewardDetails, rewardSource);
		bool showPlus = false;
		if (m_countLabel != null)
		{
			ForgeTicketRewardData forgeRewardData2 = base.Data as ForgeTicketRewardData;
			if (forgeRewardData2.Quantity > 9)
			{
				m_countLabel.Text = "9";
				showPlus = true;
			}
			else
			{
				m_countLabel.Text = forgeRewardData2.Quantity.ToString();
			}
		}
		m_root.SetActive(value: true);
		if (m_plusSign != null)
		{
			m_plusSign.SetActive(showPlus);
		}
		m_ticketVisual.transform.localEulerAngles = new Vector3(m_ticketVisual.transform.localEulerAngles.x, m_ticketVisual.transform.localEulerAngles.y, 180f);
		Hashtable args = iTweenManager.Get().GetTweenHashTable();
		args.Add("amount", new Vector3(0f, 0f, 540f));
		args.Add("time", 1.5f);
		args.Add("easetype", iTween.EaseType.easeOutElastic);
		args.Add("space", Space.Self);
		iTween.RotateAdd(m_ticketVisual, args);
	}

	protected override void HideReward()
	{
		base.HideReward();
		m_root.SetActive(value: false);
	}
}
