using Hearthstone.DataModels;
using Hearthstone.Progression;
using Hearthstone.UI;
using UnityEngine;

[RequireComponent(typeof(WidgetTemplate))]
public class RewardTrackForgotRewardsPopup : MonoBehaviour
{
	public UberText m_headerText;

	public UberText m_bodyText;

	private Widget m_widget;

	private readonly string CODE_HIDE = "CODE_HIDE";

	private void Awake()
	{
		m_widget = GetComponent<WidgetTemplate>();
		m_widget.RegisterEventListener(delegate(string eventName)
		{
			if (eventName == CODE_HIDE)
			{
				m_widget.Hide();
			}
		});
	}

	public void Show()
	{
		RewardTrackDataModel rewardTrackDataModel = m_widget.GetDataModel<RewardTrackDataModel>();
		if (rewardTrackDataModel == null)
		{
			Debug.LogWarning("Unexpected state: no bound RewardTrackDataModel");
			return;
		}
		m_headerText.Text = GameStrings.FormatPlurals("GLUE_PROGRESSION_REWARD_TRACK_POPUP_FORGOT_REWARDS_TITLE", GameStrings.MakePlurals(rewardTrackDataModel.Unclaimed));
		string body = (ProgressUtils.IsEventRewardTrackType(rewardTrackDataModel.RewardTrackType) ? "GLUE_PROGRESSION_EVENT_TAB_POPUP_FORGOT_REWARDS_BODY" : "GLUE_PROGRESSION_REWARD_TRACK_POPUP_FORGOT_REWARDS_BODY");
		m_bodyText.Text = GameStrings.Format(body, rewardTrackDataModel.Unclaimed, rewardTrackDataModel.Name);
		m_widget.Show();
	}
}
