using System.Collections;
using Hearthstone.DataModels;
using Hearthstone.UI;
using UnityEngine;

public class MercenariesAbilityUnlockReward : Reward
{
	public AsyncReference m_mercenaryCardReference;

	public AsyncReference m_abilityCardReference;

	public AsyncReference m_rootWidgetReference;

	public AsyncReference m_unlockAbilitySuperReference;

	public AsyncReference m_blackBGReference;

	protected Widget m_mercenaryCardWidget;

	protected Widget m_abilityCardWidget;

	protected Widget m_rootWidget;

	protected bool m_hidden;

	protected PlayMakerFSM m_unlockAbilitySuperFsm;

	private ScreenEffectsHandle m_screenEffectsHandle;

	private const string FsmDeathEvent = "Death";

	protected override void Start()
	{
		m_screenEffectsHandle = new ScreenEffectsHandle(this);
		base.Start();
		m_mercenaryCardReference.RegisterReadyListener<Widget>(OnMercenaryCardReady);
		m_abilityCardReference.RegisterReadyListener<Widget>(OnAbilityCardReady);
		m_rootWidgetReference.RegisterReadyListener<Widget>(OnRootWidgetReady);
		m_unlockAbilitySuperReference.RegisterReadyListener<PlayMakerFSM>(OnPlaymakerReady);
		m_blackBGReference.RegisterReadyListener<Transform>(OnBlackBGReady);
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

	private void OnAbilityCardReady(Widget widget)
	{
		m_abilityCardWidget = widget;
		if (!(m_abilityCardWidget == null))
		{
			m_abilityCardWidget.BindDataModel(new LettuceAbilityDataModel());
			if (m_hidden)
			{
				m_abilityCardWidget.Hide();
			}
		}
	}

	private void OnRootWidgetReady(Widget widget)
	{
		m_rootWidget = widget;
		if (!(m_rootWidget == null) && m_hidden)
		{
			m_rootWidget.Hide();
		}
	}

	private void OnPlaymakerReady(PlayMakerFSM playMaker)
	{
		m_unlockAbilitySuperFsm = playMaker;
		_ = m_unlockAbilitySuperFsm == null;
	}

	private void OnBlackBGReady(Transform bg)
	{
		if (bg != null && SceneMgr.Get().GetMode() == SceneMgr.Mode.LETTUCE_VILLAGE)
		{
			bg.gameObject.SetActive(value: true);
		}
	}

	protected override void InitData()
	{
		SetData(new MercenariesAbilityUnlockRewardData(), updateVisuals: false);
	}

	protected override void OnDataSet(bool updateVisuals)
	{
		if (!updateVisuals || m_hidden || m_mercenaryCardWidget == null || m_abilityCardWidget == null)
		{
			return;
		}
		if (!(base.Data is MercenariesAbilityUnlockRewardData rewardData))
		{
			Debug.LogWarning($"MercenariesAbilityUnlockReward.OnDataSet() - data {base.Data} is not MercenariesAbilityUnlockRewardData");
			return;
		}
		LettuceMercenary merc = CollectionManager.Get().GetMercenary(rewardData.MercenaryId);
		if (merc == null)
		{
			Debug.LogWarning($"MercenariesAbilityUnlockReward.OnDataSet() - No mercenary with id {rewardData.MercenaryId}");
			return;
		}
		CollectionUtils.PopulateMercenaryDataModel(m_mercenaryCardWidget.GetDataModel<LettuceMercenaryDataModel>(), merc, CollectionUtils.MercenaryDataPopluateExtra.None);
		LettuceAbility ability = merc.GetLettuceAbility(rewardData.AbilityId);
		if (ability == null)
		{
			Debug.LogWarning($"MercenariesAbilityUnlockReward.OnDataSet() - No lettuce ability found for ability id={rewardData.AbilityId}");
		}
		else
		{
			CollectionUtils.PopulateAbilityDataModel(m_abilityCardWidget.GetDataModel<LettuceAbilityDataModel>(), ability, merc);
		}
	}

	protected override void ShowReward(bool updateCacheValues)
	{
		m_hidden = false;
		m_root.SetActive(value: true);
		StartCoroutine(ShowRewardWhenReady());
	}

	private IEnumerator ShowRewardWhenReady()
	{
		while (m_mercenaryCardWidget == null || m_abilityCardWidget == null)
		{
			yield return null;
		}
		m_mercenaryCardWidget.Show();
		m_abilityCardWidget.Show();
		m_rootWidget.Show();
		m_rootWidget.TriggerEvent("SHOW");
		OnDataSet(updateVisuals: true);
		ScreenEffectParameters screenEffectParameters = ScreenEffectParameters.BlurVignetteDesaturatePerspective;
		screenEffectParameters.Blur = new BlurParameters(1f, 1f);
		screenEffectParameters.Desaturate = new DesaturateParameters(0f);
		screenEffectParameters.Time = 0.4f;
		m_screenEffectsHandle.StartEffect(screenEffectParameters);
	}

	protected override void HideReward()
	{
		base.HideReward();
		m_root.SetActive(value: false);
		m_hidden = true;
		if (m_mercenaryCardWidget != null)
		{
			m_mercenaryCardWidget.Hide();
		}
		if (m_abilityCardWidget != null)
		{
			m_abilityCardWidget.Hide();
		}
		if (m_unlockAbilitySuperFsm != null)
		{
			m_unlockAbilitySuperFsm.SendEvent("Death");
		}
		m_screenEffectsHandle.StopEffect(RewardUtils.MercRewardEndBlurTime);
	}
}
