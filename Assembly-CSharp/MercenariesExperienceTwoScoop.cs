using System;
using System.Collections.Generic;
using Hearthstone.DataModels;
using Hearthstone.UI;
using UnityEngine;

public class MercenariesExperienceTwoScoop : MonoBehaviour
{
	public AsyncReference m_rootWidgetReference;

	private Widget m_rootWidget;

	private List<MercenaryExpRewardData> m_mercenaryExpRewards;

	private Action m_onClosedCallback;

	private bool m_initialized;

	private void Start()
	{
		m_rootWidgetReference.RegisterReadyListener<Widget>(OnRootWidgetReady);
	}

	private void WidgetEventListener(string eventName)
	{
		if (eventName == "DISMISS_TWO_SCOOP")
		{
			OnClosed();
		}
	}

	public void OnRootWidgetReady(Widget widget)
	{
		if (widget == null)
		{
			Error.AddDevWarning("UI Error!", "Root Widget could not be found!");
			return;
		}
		m_rootWidget = widget;
		m_rootWidget.RegisterEventListener(WidgetEventListener);
		BindMercenariesExperienceTwoScoopDataModel();
	}

	private void BindMercenariesExperienceTwoScoopDataModel()
	{
		if (m_initialized || m_mercenaryExpRewards == null || m_mercenaryExpRewards.Count == 0 || m_rootWidget == null)
		{
			return;
		}
		m_initialized = true;
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
			expRewardDataModel.Mercenary.ShowAbilityText = false;
			expRewardDataModel.Mercenary.AbilityText = null;
			int level = GameUtils.GetMercenaryLevelFromExperience(expReward.InitialExperience);
			CollectionUtils.PopulateMercenaryCardDataModel(expRewardDataModel.Mercenary, merc.GetEquippedArtVariation());
			CollectionUtils.SetMercenaryStatsByLevel(expRewardDataModel.Mercenary, merc.ID, level, merc.m_isFullyUpgraded);
			expRewardDataModel.Mercenary.IsMaxLevel = level >= GameUtils.GetMaxMercenaryLevel();
			expRewardDataModel.ExperienceDeltaText = GameStrings.Format("GLUE_LETTUCE_MERCENARY_EXP_GAIN", expReward.Amount);
			expRewardDataModel.LeveledUp = expReward.NumberOfLevelUps > 0;
			twoScoopDataModel.ExpRewards.Add(expRewardDataModel);
		}
	}

	private void OnClosed()
	{
		m_onClosedCallback?.Invoke();
		UnityEngine.Object.Destroy(base.gameObject, 5f);
	}

	public void Initialize(List<MercenaryExpRewardData> mercenaryExpRewards, Action onClosedCallback)
	{
		m_mercenaryExpRewards = mercenaryExpRewards;
		m_onClosedCallback = onClosedCallback;
		BindMercenariesExperienceTwoScoopDataModel();
	}

	public void ResetData()
	{
		m_mercenaryExpRewards = null;
		m_onClosedCallback = null;
		m_initialized = false;
		m_rootWidget.TriggerEvent("RESET_DATA");
	}
}
