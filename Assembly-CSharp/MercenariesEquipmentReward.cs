using System.Collections;
using Assets;
using Hearthstone.DataModels;
using Hearthstone.UI;
using UnityEngine;

public class MercenariesEquipmentReward : Reward
{
	public AsyncReference m_mercenaryCardReference;

	public AsyncReference m_equipmentCardReference;

	public AsyncReference m_rootWidgetReference;

	public AsyncReference m_unlockEquipmentSuperReference;

	protected Widget m_mercenaryCardWidget;

	protected Widget m_equipmentCardWidget;

	protected Widget m_rootWidget;

	protected bool m_hidden;

	protected PlayMakerFSM m_unlockEquipmentSuperFsm;

	private const string FsmDeathEvent = "Death";

	protected override void Start()
	{
		base.Start();
		m_mercenaryCardReference.RegisterReadyListener<Widget>(OnMercenaryCardReady);
		m_equipmentCardReference.RegisterReadyListener<Widget>(OnEquipmentCardReady);
		m_rootWidgetReference.RegisterReadyListener<Widget>(OnRootWidgetReady);
		m_unlockEquipmentSuperReference.RegisterReadyListener<PlayMakerFSM>(OnPlaymakerReady);
	}

	private void OnMercenaryCardReady(Widget widget)
	{
		m_mercenaryCardWidget = widget;
		if (!(m_mercenaryCardWidget == null))
		{
			m_mercenaryCardWidget.BindDataModel(MercenaryFactory.CreateEmptyMercenaryDataModel());
			if (m_hidden)
			{
				m_mercenaryCardWidget.Hide();
			}
		}
	}

	private void OnEquipmentCardReady(Widget widget)
	{
		m_equipmentCardWidget = widget;
		if (!(m_equipmentCardWidget == null))
		{
			m_equipmentCardWidget.BindDataModel(new LettuceAbilityDataModel());
			if (m_hidden)
			{
				m_equipmentCardWidget.Hide();
			}
		}
	}

	private void OnRootWidgetReady(Widget widget)
	{
		m_rootWidget = widget;
		if (!(m_rootWidget == null))
		{
			m_rootWidget.RegisterEventListener(RootWidgetEventListener);
			if (m_hidden)
			{
				m_rootWidget.Hide();
			}
		}
	}

	private void OnPlaymakerReady(PlayMakerFSM playmaker)
	{
		m_unlockEquipmentSuperFsm = playmaker;
		_ = m_unlockEquipmentSuperFsm == null;
	}

	private void RootWidgetEventListener(string eventName)
	{
		if (eventName == "PLAY_FTUE_EQUIPMENT_UNLOCK_code")
		{
			LettuceTutorialUtils.FireEvent(LettuceTutorialVo.LettuceTutorialEvent.FIRST_EQUIPMENT_UNLOCKED, base.gameObject);
		}
	}

	protected override void InitData()
	{
		SetData(new MercenariesEquipmentRewardData(), updateVisuals: false);
	}

	protected override void OnDataSet(bool updateVisuals)
	{
		if (!updateVisuals || m_hidden || m_mercenaryCardWidget == null || m_equipmentCardWidget == null)
		{
			return;
		}
		if (!(base.Data is MercenariesEquipmentRewardData rewardData))
		{
			Debug.LogWarning($"MercenariesEquipmentUnlockReward.OnDataSet() - data {base.Data} is not MercenariesEquipmentUnlockRewardData");
			return;
		}
		LettuceMercenary merc = CollectionManager.Get().GetMercenary(rewardData.MercenaryId);
		if (merc == null)
		{
			Debug.LogWarning($"MercenariesEquipmentUnlockReward.OnDataSet() - No mercenary with id {rewardData.MercenaryId}");
			return;
		}
		CollectionUtils.PopulateMercenaryDataModel(m_mercenaryCardWidget.GetDataModel<LettuceMercenaryDataModel>(), merc, CollectionUtils.MercenaryDataPopluateExtra.None);
		LettuceEquipmentDbfRecord equipmentRecord = GameDbf.LettuceEquipment.GetRecord(rewardData.EquipmentId);
		if (equipmentRecord == null)
		{
			Debug.LogWarning($"MercenariesEquipmentUnlockReward.OnDataSet() - No record found for equipment id={rewardData.EquipmentId}");
			return;
		}
		string equipmentCardId = null;
		foreach (LettuceEquipmentTierDbfRecord equipmentTierRecord in equipmentRecord.LettuceEquipmentTiers)
		{
			if (equipmentTierRecord.Tier == rewardData.EquipmentTier)
			{
				equipmentCardId = GameUtils.TranslateDbIdToCardId(equipmentTierRecord.CardId, showWarning: true);
				break;
			}
		}
		if (string.IsNullOrEmpty(equipmentCardId))
		{
			Debug.LogWarning($"MercenariesEquipmentUnlockReward.OnDataSet() - No card for equipment id={rewardData.EquipmentId}, tier={rewardData.EquipmentTier}");
			return;
		}
		LettuceAbility equip = merc.GetLettuceEquipment(rewardData.EquipmentId);
		CollectionUtils.PopulateDefaultAbilityDataModelWithTier(m_equipmentCardWidget.GetDataModel<LettuceAbilityDataModel>(), equip, merc, rewardData.EquipmentTier);
	}

	protected override void ShowReward(bool updateCacheValues)
	{
		m_hidden = false;
		m_root.SetActive(value: true);
		StartCoroutine(ShowRewardWhenReady());
	}

	private IEnumerator ShowRewardWhenReady()
	{
		while (m_mercenaryCardWidget == null || m_equipmentCardWidget == null)
		{
			yield return null;
		}
		m_mercenaryCardWidget.Show();
		m_equipmentCardWidget.Show();
		m_rootWidget.Show();
		m_rootWidget.TriggerEvent("SHOW");
		OnDataSet(updateVisuals: true);
		EnableClickCatcher(enabled: true);
		UIContext.GetRoot().ShowPopup(base.gameObject);
	}

	protected override void HideReward()
	{
		if (m_rootWidget != null)
		{
			UIContext.GetRoot().DismissPopup(base.gameObject);
		}
		base.HideReward();
		m_root.SetActive(value: false);
		m_hidden = true;
		if (m_mercenaryCardWidget != null)
		{
			m_mercenaryCardWidget.Hide();
		}
		if (m_equipmentCardWidget != null)
		{
			m_equipmentCardWidget.Hide();
		}
		if (m_unlockEquipmentSuperFsm != null)
		{
			m_unlockEquipmentSuperFsm.SendEvent("Death");
		}
	}
}
