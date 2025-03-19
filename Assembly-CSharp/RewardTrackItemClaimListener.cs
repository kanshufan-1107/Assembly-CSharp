using Assets;
using Hearthstone.DataModels;
using Hearthstone.Progression;
using Hearthstone.UI;
using UnityEngine;

[RequireComponent(typeof(WidgetTemplate))]
public class RewardTrackItemClaimListener : MonoBehaviour
{
	public const string CLAIM_INDIVIDUAL_REWARD = "CODE_CLAIM_INDIVIDUAL_REWARD";

	public const string CLAIM_CHOOSE_ONE_REWARD = "CODE_CLAIM_CHOOSE_ONE_REWARD";

	private WidgetTemplate m_widget;

	private void Awake()
	{
		m_widget = GetComponent<WidgetTemplate>();
		m_widget.RegisterEventListener(delegate(string eventName)
		{
			if (!(eventName == "CODE_CLAIM_INDIVIDUAL_REWARD"))
			{
				if (eventName == "CODE_CLAIM_CHOOSE_ONE_REWARD")
				{
					ClaimReward(chooseOne: true);
					m_widget.TriggerEvent("CLEANUP_POPUP_AFTER_CONFIRM");
				}
			}
			else
			{
				ClaimReward(chooseOne: false);
			}
		});
	}

	private void ClaimReward(bool chooseOne)
	{
		if (!Network.IsLoggedIn())
		{
			ProgressUtils.ShowOfflinePopup();
			return;
		}
		RewardTrackNodeRewardsDataModel nodeDataModel = m_widget.GetDataModel<RewardTrackNodeRewardsDataModel>();
		if (nodeDataModel == null)
		{
			Debug.LogError("RewardTrackItemClaimListener: Failed to get reward track node rewards data model!");
			return;
		}
		int chooseOneRewardItemId = 0;
		if (chooseOne)
		{
			if (!(m_widget.GetDataModel<EventDataModel>()?.Payload is RewardItemDataModel rewardItemDataModel))
			{
				Debug.LogError("RewardTrackItemClaimListener: failed to get reward item data model from event payload!");
				return;
			}
			chooseOneRewardItemId = rewardItemDataModel.AssetId;
		}
		Global.RewardTrackType rewardTrackType = (Global.RewardTrackType)nodeDataModel.RewardTrackType;
		RewardTrackManager.Get().GetRewardTrack(rewardTrackType)?.ClaimReward(nodeDataModel.RewardTrackId, nodeDataModel.Level, nodeDataModel.PaidType, chooseOneRewardItemId);
	}
}
