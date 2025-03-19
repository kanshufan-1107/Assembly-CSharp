using System.Collections;
using Blizzard.T5.Services;
using Hearthstone.Store;
using UnityEngine;

public class CardBackReward : Reward
{
	public GameObject m_cardbackBone;

	private int m_numCardBacksLoaded;

	protected override void InitData()
	{
		SetData(new CardBackRewardData(), updateVisuals: false);
	}

	protected override void ShowReward(bool updateCacheValues)
	{
		if (!(base.Data is CardBackRewardData cardBackRewardData))
		{
			Debug.LogWarning($"CardBackReward.ShowReward() - Data {base.Data} is not CardBackRewardData");
			return;
		}
		if (!cardBackRewardData.IsDummyReward && updateCacheValues)
		{
			CardBackManager.Get().AddNewCardBack(cardBackRewardData.CardBackID);
			if (ServiceManager.TryGet<IProductDataService>(out var dataService))
			{
				dataService.UpdateProductStatus();
			}
		}
		m_root.SetActive(value: true);
		m_cardbackBone.transform.localEulerAngles = new Vector3(0f, 0f, 180f);
		Hashtable args = iTweenManager.Get().GetTweenHashTable();
		args.Add("amount", new Vector3(0f, 0f, 540f));
		args.Add("time", 1.5f);
		args.Add("easetype", iTween.EaseType.easeOutElastic);
		args.Add("space", Space.Self);
		iTween.RotateAdd(m_cardbackBone.gameObject, args);
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
			CardBackRewardData cardBackRewardData = base.Data as CardBackRewardData;
			string rewardHeadline = GameStrings.Get("GLOBAL_REWARD_CARD_BACK_HEADLINE");
			SetRewardText(rewardHeadline, string.Empty, string.Empty);
			if (cardBackRewardData == null)
			{
				Debug.LogWarning($"CardBackReward.OnDataSet() - Data {base.Data} is not CardBackRewardData");
				return;
			}
			SetReady(ready: false);
			CardBackManager.Get().LoadCardBackByIndex(cardBackRewardData.CardBackID, OnFrontCardBackLoaded, unlit: true);
			CardBackManager.Get().LoadCardBackByIndex(cardBackRewardData.CardBackID, OnBackCardBackLoaded, unlit: true);
		}
	}

	private void OnFrontCardBackLoaded(CardBackManager.LoadCardBackData cardbackData)
	{
		GameObject obj = cardbackData.m_GameObject;
		obj.transform.parent = m_cardbackBone.transform;
		obj.transform.localPosition = Vector3.zero;
		obj.transform.localRotation = Quaternion.Euler(Vector3.zero);
		obj.transform.localScale = Vector3.one;
		LayerUtils.SetLayer(obj, base.gameObject.layer, null);
		m_numCardBacksLoaded++;
		if (2 == m_numCardBacksLoaded)
		{
			SetReady(ready: true);
		}
	}

	private void OnBackCardBackLoaded(CardBackManager.LoadCardBackData cardbackData)
	{
		GameObject obj = cardbackData.m_GameObject;
		obj.transform.parent = m_cardbackBone.transform;
		obj.transform.localPosition = Vector3.zero;
		obj.transform.localRotation = Quaternion.Euler(new Vector3(0f, 0f, 180f));
		obj.transform.localScale = Vector3.one;
		LayerUtils.SetLayer(obj, base.gameObject.layer, null);
		m_numCardBacksLoaded++;
		if (2 == m_numCardBacksLoaded)
		{
			SetReady(ready: true);
		}
	}
}
