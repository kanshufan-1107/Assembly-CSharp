using System.Collections;
using Hearthstone.DataModels;
using Hearthstone.UI;
using UnityEngine;

namespace Hearthstone.Progression;

public class RewardListPopup : MonoBehaviour
{
	public const string SHOW_REWARDS = "CODE_SHOW_REWARDS";

	public const string SHOW_REWARD_LAYOUT = "CODE_SHOW_REWARD_LAYOUT";

	public const string SHOW_CHOOSE_ONE_LAYOUT = "CODE_SHOW_CHOOSE_ONE_LAYOUT";

	public const string ANIMATE_REWARD = "CODE_ANIMATE_REWARD";

	public const string FROM_ACHIEVE = "FROM_ACHIEVE";

	[SerializeField]
	private GameObject m_ChooseReward;

	[SerializeField]
	private GameObject m_SingleReward;

	private Widget m_widget;

	private AchievementDataModel m_achievementDataModel;

	private void Awake()
	{
		m_widget = GetComponent<WidgetTemplate>();
		m_widget.RegisterEventListener(HandleEvent);
		m_ChooseReward.SetActive(value: false);
		m_SingleReward.SetActive(value: false);
	}

	public void OnDisable()
	{
		m_widget.UnbindDataModel(34);
		m_ChooseReward.SetActive(value: false);
		m_SingleReward.SetActive(value: false);
	}

	private void HandleDoneChangingStates(object _)
	{
		StartCoroutine(AnimateRewardAfterShortDelay());
	}

	private IEnumerator AnimateRewardAfterShortDelay()
	{
		yield return new WaitForSeconds(0.1f);
		m_widget.TriggerEvent("CODE_ANIMATE_REWARD");
	}

	private void HandleEvent(string eventName)
	{
		if (eventName == "CODE_SHOW_REWARDS")
		{
			HandleShowRewards();
		}
	}

	private void HandleShowRewards()
	{
		AchievementDataModel achievementDataModel = m_widget.GetDataModel<AchievementDataModel>();
		if (achievementDataModel != null)
		{
			m_achievementDataModel = achievementDataModel;
			m_widget.BindDataModel(m_achievementDataModel.RewardList);
			if (m_achievementDataModel.RewardList.ChooseOne)
			{
				m_widget.TriggerEvent("CODE_SHOW_CHOOSE_ONE_LAYOUT", new TriggerEventParameters(null, m_achievementDataModel));
			}
			else
			{
				m_widget.TriggerEvent("CODE_SHOW_REWARD_LAYOUT");
			}
			m_widget.RegisterDoneChangingStatesListener(HandleDoneChangingStates, null, callImmediatelyIfSet: false, doOnce: true);
		}
	}
}
