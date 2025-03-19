using System;
using System.Collections.Generic;
using Hearthstone.DataModels;
using Hearthstone.UI;
using UnityEngine;

public class MercenariesTrainingHallExpRewardPopup : MonoBehaviour
{
	public const string DISMISS_EXP_REWARD = "DISMISS_EXP_REWARD";

	private GameObject m_rewardContainer;

	private Widget m_rootWidget;

	private List<MercenaryExpRewardData> m_mercenaryExpRewards;

	private Action m_onReadyCallback;

	private Action m_onClosedCallback;

	private bool m_isLoading;

	private void LoadWidgetPrefab()
	{
		if (!m_isLoading)
		{
			m_isLoading = true;
			AssetReference assetReference = "MercenariesExperienceTrainingHall.prefab:a3944e4a53a98714b8dbb33907caeba6";
			if (!AssetLoader.Get().InstantiatePrefab(assetReference, OnExperienceWidgetLoadAttempted))
			{
				OnExperienceWidgetLoadAttempted(assetReference, null, null);
			}
		}
	}

	private void OnExperienceWidgetLoadAttempted(AssetReference assetRef, GameObject go, object callbackData)
	{
		if (go == null)
		{
			m_isLoading = false;
			Log.Lettuce.PrintError("Failed to load mercenaries experience reward prefab");
			return;
		}
		go.transform.parent = base.transform;
		m_rewardContainer = go;
		m_rewardContainer.transform.localScale = Vector3.one;
		m_rewardContainer.transform.localPosition = Vector3.zero;
		m_rootWidget = go.GetComponentInChildren<WidgetInstance>();
		m_rootWidget.RegisterEventListener(WidgetEventListener);
		BindMercenariesExperienceTwoScoopDataModel();
		if (m_onReadyCallback != null)
		{
			m_onReadyCallback();
		}
		m_isLoading = false;
	}

	private void WidgetEventListener(string eventName)
	{
		if (eventName == "DISMISS_EXP_REWARD")
		{
			OnClosed();
		}
	}

	private void BindMercenariesExperienceTwoScoopDataModel()
	{
		if (m_mercenaryExpRewards == null || m_mercenaryExpRewards.Count == 0 || m_rootWidget == null)
		{
			return;
		}
		LettuceExperienceTwoScoopDataModel twoScoopDataModel = new LettuceExperienceTwoScoopDataModel();
		m_rootWidget.BindDataModel(twoScoopDataModel);
		twoScoopDataModel.ExpRewards = new DataModelList<LettuceMercenaryExpRewardDataModel>();
		foreach (MercenaryExpRewardData expReward in m_mercenaryExpRewards)
		{
			LettuceMercenaryExpRewardDataModel expRewardDataModel = new LettuceMercenaryExpRewardDataModel();
			LettuceMercenary merc = CollectionManager.Get().GetMercenary(expReward.MercenaryId);
			expRewardDataModel.Mercenary = MercenaryFactory.CreateMercenaryDataModel(merc);
			expRewardDataModel.Mercenary.ExperienceFinal = expReward.FinalExperience;
			expRewardDataModel.Mercenary.ExperienceInitial = expReward.InitialExperience;
			expRewardDataModel.Mercenary.Owned = true;
			expRewardDataModel.Mercenary.Label = GameStrings.Get("MERCENARY_CARD_LABEL_XP");
			int level = GameUtils.GetMercenaryLevelFromExperience(expReward.InitialExperience);
			CollectionUtils.PopulateMercenaryCardDataModel(expRewardDataModel.Mercenary, merc.GetEquippedArtVariation());
			CollectionUtils.SetMercenaryStatsByLevel(expRewardDataModel.Mercenary, merc.ID, level, merc.m_isFullyUpgraded);
			expRewardDataModel.ExperienceDeltaText = GameStrings.Format("GLUE_LETTUCE_MERCENARY_EXP_GAIN", expReward.Amount);
			expRewardDataModel.LeveledUp = expReward.NumberOfLevelUps > 0;
			twoScoopDataModel.ExpRewards.Add(expRewardDataModel);
		}
	}

	public void Initialize(List<MercenaryExpRewardData> mercenaryExpRewards, Action onReadyCallback, Action onClosedCallback)
	{
		m_mercenaryExpRewards = mercenaryExpRewards;
		m_onClosedCallback = onClosedCallback;
		m_onReadyCallback = onReadyCallback;
		LoadWidgetPrefab();
	}

	private void OnClosed()
	{
		m_onClosedCallback?.Invoke();
		UnityEngine.Object.Destroy(m_rewardContainer, 1f);
	}
}
