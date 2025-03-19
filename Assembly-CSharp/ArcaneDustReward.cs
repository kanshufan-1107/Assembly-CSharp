using UnityEngine;

public class ArcaneDustReward : Reward
{
	public GameObject m_dustJar;

	public UberText m_dustCount;

	protected override void InitData()
	{
		SetData(new ArcaneDustRewardData(), updateVisuals: false);
	}

	protected override void ShowReward(bool updateCacheValues)
	{
		if (!(base.Data is ArcaneDustRewardData dustRewardData))
		{
			Debug.LogWarning($"ArcaneDustReward.ShowReward() - Data {base.Data} is not ArcaneDustRewardData");
			return;
		}
		m_root.SetActive(value: true);
		m_dustCount.Text = dustRewardData.Amount.ToString();
		Vector3 endScale = m_dustJar.transform.localScale;
		m_dustJar.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
		iTween.ScaleTo(m_dustJar.gameObject, iTween.Hash("scale", endScale, "time", 0.5f, "easetype", iTween.EaseType.easeOutElastic));
	}

	protected override void HideReward()
	{
		base.HideReward();
		m_root.SetActive(value: false);
	}

	protected override void OnDataSet(bool updateVisuals)
	{
		if (updateVisuals)
		{
			string rewardHeadline = GameStrings.Get("GLOBAL_REWARD_ARCANE_DUST_HEADLINE");
			SetRewardText(rewardHeadline, "", "");
		}
	}
}
