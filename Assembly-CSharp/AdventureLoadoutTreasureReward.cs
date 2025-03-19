using System.Collections;
using Hearthstone.DataModels;
using Hearthstone.Progression;
using UnityEngine;

public class AdventureLoadoutTreasureReward : WidgetReward
{
	private AdventureLoadoutTreasuresDbfRecord m_loadoutTreasureRecord;

	protected override void Start()
	{
		base.Start();
	}

	private IEnumerator SetDataWhenLoaded()
	{
		while (m_rewardWidget == null)
		{
			yield return null;
		}
		AdventureLoadoutTreasureRewardData loadoutTreasureRewardData = base.Data as AdventureLoadoutTreasureRewardData;
		AdventureLoadoutOptionDataModel dataModel = new AdventureLoadoutOptionDataModel();
		bool isUpgrade = false;
		if (loadoutTreasureRewardData != null && loadoutTreasureRewardData.IsUpgrade)
		{
			isUpgrade = true;
			if (!string.IsNullOrEmpty(m_loadoutTreasureRecord.UpgradedDescriptionText))
			{
				dataModel.LockedText = string.Format(m_loadoutTreasureRecord.UpgradedDescriptionText, m_loadoutTreasureRecord.UpgradeValue);
			}
			dataModel.IsUpgraded = true;
		}
		else
		{
			if (!string.IsNullOrEmpty(m_loadoutTreasureRecord.UnlockedDescriptionText))
			{
				int achQuota = 0;
				if (m_loadoutTreasureRecord.UnlockAchievement > 0)
				{
					achQuota = AchievementManager.Get().GetAchievementDataModel(m_loadoutTreasureRecord.UnlockAchievement).Quota;
				}
				int unlockValue = m_loadoutTreasureRecord.UnlockValue + achQuota;
				dataModel.LockedText = string.Format(m_loadoutTreasureRecord.UnlockedDescriptionText, unlockValue);
			}
			dataModel.IsUpgraded = false;
		}
		m_rewardWidget.BindDataModel(dataModel);
		string cardId = GameUtils.TranslateDbIdToCardId(isUpgrade ? m_loadoutTreasureRecord.UpgradedCardId : m_loadoutTreasureRecord.CardId);
		if (cardId == null)
		{
			Debug.LogWarningFormat("AdventureLoadoutTreasureReward.SetLoadoutTreasureWhenReady() - No CardId found for DbId {0}!", m_loadoutTreasureRecord.CardId);
		}
		CardDataModel cardDataModel = new CardDataModel();
		CardDbfRecord record = GameDbf.GetIndex().GetCardRecord(cardId);
		cardDataModel.CardId = cardId;
		cardDataModel.FlavorText = record?.FlavorText;
		m_rewardWidget.BindDataModel(cardDataModel);
		while (m_rewardWidget.IsChangingStates)
		{
			yield return null;
		}
		SetReady(ready: true);
	}

	protected override void InitData()
	{
		SetData(new AdventureLoadoutTreasureRewardData(), updateVisuals: false);
	}

	protected override void OnDataSet(bool updateVisuals)
	{
		if (!updateVisuals)
		{
			return;
		}
		if (!(base.Data is AdventureLoadoutTreasureRewardData loadoutTreasureRewardData))
		{
			Debug.LogWarningFormat("AdventureLoadoutTreasureReward.OnDataSet() - Data {0} is not LoadoutTreasureRewardData", base.Data);
			return;
		}
		m_loadoutTreasureRecord = loadoutTreasureRewardData.LoadoutTreasureRecord;
		if (m_loadoutTreasureRecord == null)
		{
			Debug.LogWarningFormat("AdventureLoadoutTreasureReward.OnDataSet() - LoadoutTreasureRecord is null!");
			return;
		}
		SetReady(ready: false);
		StartCoroutine(SetDataWhenLoaded());
	}
}
