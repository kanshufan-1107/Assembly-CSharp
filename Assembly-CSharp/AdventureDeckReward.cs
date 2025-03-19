using System.Collections;
using Hearthstone.DataModels;
using UnityEngine;

public class AdventureDeckReward : WidgetReward
{
	private AdventureDeckDbfRecord m_deckRecord;

	protected override void Start()
	{
		base.Start();
		StartCoroutine(SetDeckPouchPropertiesWhenStateReady());
	}

	private IEnumerator SetDeckPouchPropertiesWhenStateReady()
	{
		while (m_rewardWidget == null)
		{
			yield return null;
		}
		while (m_deckRecord == null)
		{
			yield return null;
		}
		CollectionManager.Get().LoadDeckFromDBF(m_deckRecord.DeckId, out var deckName, out var _);
		AdventureLoadoutOptionDataModel dataModel = new AdventureLoadoutOptionDataModel();
		dataModel.Name = deckName;
		if (!string.IsNullOrEmpty(m_deckRecord.UnlockedDescriptionText))
		{
			dataModel.LockedText = string.Format(m_deckRecord.UnlockedDescriptionText, m_deckRecord.UnlockValue);
		}
		dataModel.DisplayColor = CollectionPageManager.ColorForClass((TAG_CLASS)m_deckRecord.ClassId);
		bool waitingForTexture = false;
		if (string.IsNullOrEmpty(m_deckRecord.DisplayTexture))
		{
			dataModel.DisplayTexture = null;
		}
		else
		{
			ObjectCallback onMaterialLoaded = delegate(AssetReference assetRef, Object materialObj, object data)
			{
				dataModel.DisplayTexture = materialObj as Material;
				waitingForTexture = false;
			};
			waitingForTexture = true;
			if (!AssetLoader.Get().LoadMaterial(m_deckRecord.DisplayTexture, onMaterialLoaded))
			{
				onMaterialLoaded(m_deckRecord.DisplayTexture, null, null);
			}
		}
		m_rewardWidget.BindDataModel(dataModel);
		while (waitingForTexture || m_rewardWidget.IsChangingStates)
		{
			yield return null;
		}
		SetReady(ready: true);
	}

	protected override void InitData()
	{
		SetData(new AdventureDeckRewardData(), updateVisuals: false);
	}

	protected override void OnDataSet(bool updateVisuals)
	{
		if (updateVisuals)
		{
			if (!(base.Data is AdventureDeckRewardData deckRewardData))
			{
				Debug.LogWarningFormat("AdventureDeckReward.OnDataSet() - Data {0} is not DeckRewardData", base.Data);
			}
			else
			{
				SetReady(ready: false);
				m_deckRecord = deckRewardData.DeckRecord;
			}
		}
	}
}
