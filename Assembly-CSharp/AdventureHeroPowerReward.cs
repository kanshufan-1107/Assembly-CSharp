using System.Collections;
using Hearthstone.DataModels;
using Hearthstone.Progression;
using UnityEngine;

public class AdventureHeroPowerReward : WidgetReward
{
	private AdventureHeroPowerDbfRecord m_heroPowerRecord;

	private DefLoader.DisposableFullDef m_heroPowerFullDef;

	protected override void Start()
	{
		base.Start();
		StartCoroutine(SetHeroPowerWhenReady());
	}

	protected override void OnDestroy()
	{
		m_heroPowerFullDef?.Dispose();
		m_heroPowerFullDef = null;
		base.OnDestroy();
	}

	private IEnumerator SetHeroPowerWhenReady()
	{
		while (m_rewardWidget == null)
		{
			yield return null;
		}
		while (m_heroPowerRecord == null)
		{
			yield return null;
		}
		AdventureLoadoutOptionDataModel dataModel = new AdventureLoadoutOptionDataModel();
		if (!string.IsNullOrEmpty(m_heroPowerRecord.UnlockedDescriptionText))
		{
			int achQuota = 0;
			if (m_heroPowerRecord.UnlockAchievement > 0)
			{
				achQuota = AchievementManager.Get().GetAchievementDataModel(m_heroPowerRecord.UnlockAchievement).Quota;
			}
			int unlockValue = m_heroPowerRecord.UnlockValue + achQuota;
			dataModel.LockedText = string.Format(m_heroPowerRecord.UnlockedDescriptionText, unlockValue);
		}
		m_rewardWidget.BindDataModel(dataModel);
		while (m_rewardWidget.IsChangingStates)
		{
			yield return null;
		}
		while (m_heroPowerFullDef == null)
		{
			yield return null;
		}
		Actor componentInChildren = m_rewardWidget.GetComponentInChildren<Actor>();
		componentInChildren.SetFullDef(m_heroPowerFullDef);
		componentInChildren.UpdateAllComponents();
		SetReady(ready: true);
	}

	private void OnFullDefLoaded(string cardID, DefLoader.DisposableFullDef def, object userData)
	{
		if (def == null)
		{
			Debug.LogErrorFormat("Unable to load FullDef for cardID={0}", cardID);
		}
		else
		{
			m_heroPowerFullDef?.Dispose();
			m_heroPowerFullDef = def.Share();
		}
	}

	protected override void InitData()
	{
		SetData(new AdventureHeroPowerRewardData(), updateVisuals: false);
	}

	protected override void OnDataSet(bool updateVisuals)
	{
		if (!updateVisuals)
		{
			return;
		}
		if (!(base.Data is AdventureHeroPowerRewardData heroPowerRewardData))
		{
			Debug.LogWarningFormat("AdventureHeroPowerReward.OnDataSet() - Data {0} is not HeroPowerRewardData", base.Data);
			return;
		}
		m_heroPowerRecord = heroPowerRewardData.HeroPowerRecord;
		if (m_heroPowerRecord == null)
		{
			Debug.LogWarningFormat("AdventureHeroPowerReward.OnDataSet() - HeroPowerRecord is null!");
			return;
		}
		string cardId = GameUtils.TranslateDbIdToCardId(m_heroPowerRecord.CardId);
		if (cardId == null)
		{
			Debug.LogWarningFormat("AdventureHeroPowerReward.OnDataSet() - No CardId found for DbId {0}!", m_heroPowerRecord.CardId);
		}
		else
		{
			SetReady(ready: false);
			DefLoader.Get().LoadFullDef(cardId, OnFullDefLoaded);
		}
	}
}
