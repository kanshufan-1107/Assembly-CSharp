using System.Collections;
using Hearthstone.DataModels;
using Hearthstone.UI;
using UnityEngine;

public class MercenaryActorLevelObject : MonoBehaviour
{
	public UberText m_levelText;

	public ProgressBar m_xpBar;

	public GameObject m_xpBarCover;

	public GameObject m_xpBarBacking;

	public PlayMakerFSM m_fsm;

	public float m_randomStartOffsetMin;

	public float m_randomStartOffsetMax = 0.5f;

	private const float DELAY_BEFORE_ANIMATION_COMPLETE_EVENT = 0.5f;

	private const float FULLY_UPGRADED_HOLD_TIME = 1f;

	private int m_displayedLevel;

	private bool m_FullyUpgraded;

	private LettuceMercenaryDataModel m_mercenaryDataModel;

	private bool m_isAnimating;

	private void Awake()
	{
		m_xpBar.OnProgressBarFilled += OnBarFilled;
	}

	public void SetLevelInfo(int initialExperience, int finalExperience, bool fullyUpgradedFinal, Hearthstone.UI.Card mercenaryCardWidget = null)
	{
		if (m_isAnimating)
		{
			return;
		}
		if (mercenaryCardWidget != null && mercenaryCardWidget.Owner.GetDataModel(216, out var mercenaryDataModel))
		{
			m_mercenaryDataModel = mercenaryDataModel as LettuceMercenaryDataModel;
		}
		if (initialExperience < 0)
		{
			initialExperience = 0;
		}
		if (initialExperience == finalExperience || finalExperience == 0)
		{
			float xpBarFillAmount = GameUtils.GetExperiencePercentageFromExperienceValue(initialExperience);
			xpBarFillAmount = GetAutoWrappedProgressBarValue(xpBarFillAmount);
			m_xpBar.SetProgressBar(xpBarFillAmount);
			if (fullyUpgradedFinal)
			{
				m_FullyUpgraded = fullyUpgradedFinal;
				StartCoroutine(AnimateFullyUpgraded());
			}
			else if (base.isActiveAndEnabled)
			{
				StartCoroutine(SendAnimationCompleteEventAfterDelay());
			}
		}
		else
		{
			int experienceDelta = finalExperience - initialExperience;
			float initialPercentValue = GameUtils.GetExperiencePercentageFromExperienceValue(initialExperience);
			float percentValueDelta = GameUtils.GetExperiencePercentageDelta(initialExperience, experienceDelta);
			initialPercentValue = GetAutoWrappedProgressBarValue(initialPercentValue);
			StartCoroutine(AnimateBar(initialPercentValue, percentValueDelta));
		}
		SetLevelText(GameUtils.GetMercenaryLevelFromExperience(initialExperience));
	}

	public void SetLevelText(int level)
	{
		m_displayedLevel = Mathf.Min(level, GameUtils.GetMaxMercenaryLevel());
		m_levelText.Text = m_displayedLevel.ToString();
	}

	private float GetAutoWrappedProgressBarValue(float value)
	{
		if (value % 1f == 0f)
		{
			value += 0.0001f;
		}
		return value;
	}

	private IEnumerator AnimateBar(float initialValue, float delta)
	{
		m_isAnimating = true;
		m_xpBar.SetProgressBar(initialValue);
		float randomWaitTime = Random.Range(m_randomStartOffsetMin, m_randomStartOffsetMax);
		yield return new WaitForSeconds(randomWaitTime);
		m_fsm.SendEvent("Birth");
		float finalValue = GetAutoWrappedProgressBarValue(initialValue + delta);
		m_xpBar.AnimateProgress(initialValue, finalValue);
		yield return new WaitForSeconds(m_xpBar.GetAnimationTime());
		yield return SendAnimationCompleteEventAfterDelay();
		m_fsm.SendEvent("Death");
		m_isAnimating = false;
	}

	private IEnumerator SendAnimationCompleteEventAfterDelay()
	{
		yield return new WaitForSeconds(0.5f);
		SendEventUpwardStateAction.SendEventUpward(base.gameObject, "XP_BAR_ANIMATION_COMPLETE_FROM_CODE");
	}

	private void OnBarFilled()
	{
		int attackIncrease = 0;
		int healthIncrease = 0;
		EventDataModel eventData = new EventDataModel();
		if (m_mercenaryDataModel != null)
		{
			int mercenaryId = m_mercenaryDataModel.MercenaryId;
			LettuceMercenaryLevelUpDataModel levelUpData = new LettuceMercenaryLevelUpDataModel();
			CollectionUtils.GetMercenaryStatsByLevel(mercenaryId, m_displayedLevel, isFullyUpgraded: false, out var preAttack, out var preHealth);
			CollectionUtils.GetMercenaryStatsByLevel(mercenaryId, m_displayedLevel + 1, m_FullyUpgraded, out var postAttack, out var postHealth);
			attackIncrease = postAttack - preAttack;
			healthIncrease = postHealth - preHealth;
			levelUpData.AttackIncrease = attackIncrease;
			levelUpData.HealthIncrease = healthIncrease;
			levelUpData.NewAttackValue = postAttack;
			levelUpData.NewHealthValue = postHealth;
			levelUpData.NewIsMaxLevel = m_displayedLevel + 1 >= GameUtils.GetMaxMercenaryLevel();
			eventData.Payload = levelUpData;
		}
		SendEventUpwardStateAction.SendEventUpward(base.gameObject, "LEVEL_UP_FROM_CODE", eventData);
		m_fsm.FsmVariables.GetFsmInt("AttackIncrease").Value = attackIncrease;
		m_fsm.FsmVariables.GetFsmInt("HealthIncrease").Value = healthIncrease;
		m_fsm.SendEvent("LEVEL UP");
		SetLevelText(m_displayedLevel + 1);
	}

	private IEnumerator AnimateFullyUpgraded()
	{
		m_isAnimating = true;
		float randomWaitTime = Random.Range(m_randomStartOffsetMin, m_randomStartOffsetMax);
		yield return new WaitForSeconds(randomWaitTime);
		m_fsm.SendEvent("Birth");
		OnBarFilled();
		yield return new WaitForSeconds(1f);
		yield return SendAnimationCompleteEventAfterDelay();
		m_fsm.SendEvent("Death");
		m_isAnimating = false;
	}
}
