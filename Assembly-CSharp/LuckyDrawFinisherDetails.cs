using Hearthstone.DataModels;
using Hearthstone.UI;
using UnityEngine;

public class LuckyDrawFinisherDetails : MonoBehaviour
{
	[SerializeField]
	private GameObject m_scaleParentOverride;

	[SerializeField]
	private AsyncReference m_baconFinisherCollectionDetailsReference;

	private BaconFinisherCollectionDetails m_baconFinisherCollectionDetails;

	public bool ShowingRewardGrantVFX { get; set; }

	private void Start()
	{
		m_baconFinisherCollectionDetailsReference.RegisterReadyListener<BaconFinisherCollectionDetails>(OnFinisherDetailsDisplayReady);
	}

	private void OnFinisherDetailsDisplayReady(BaconFinisherCollectionDetails collectionDetails)
	{
		m_baconFinisherCollectionDetails = collectionDetails;
		m_baconFinisherCollectionDetails.SetScaleParentOverride(m_scaleParentOverride);
	}

	private void AssignDataModel(LuckyDrawRewardDataModel finisherData)
	{
		if (finisherData == null)
		{
			LuckyDrawManager.Get().LogError("Error [LuckyDrawFinisherDetails] AssignDataModel finisherData was null!");
		}
		else if (finisherData.RewardList == null)
		{
			LuckyDrawManager.Get().LogError("Error [LuckyDrawFinisherDetails] AssignDataModel rewardList was null!");
		}
		else if (finisherData.RewardList.Items == null || finisherData.RewardList.Items.Count <= 0)
		{
			LuckyDrawManager.Get().LogError("Error [LuckyDrawFinisherDetails] AssignDataModel rewardList items were null or empty!");
		}
		else if (finisherData.RewardList.Items[0].BGFinisher == null)
		{
			LuckyDrawManager.Get().LogError("Error [LuckyDrawFinisherDetails] AssignDataModel rewardList Item was not a finisher!");
		}
		else
		{
			m_baconFinisherCollectionDetails.AssignDataModels(finisherData.RewardList.Items[0].BGFinisher, null);
		}
	}

	public void OnShow(IDataModel dataModel)
	{
		AssignDataModel(dataModel as LuckyDrawRewardDataModel);
		m_baconFinisherCollectionDetails.Show();
		if (ShowingRewardGrantVFX)
		{
			EventFunctions.TriggerEvent(m_baconFinisherCollectionDetails.transform.parent, "LUCKY_DRAW_SHOW_REWARD", TriggerEventParameters.Standard);
		}
		else
		{
			EventFunctions.TriggerEvent(m_baconFinisherCollectionDetails.transform.parent, "LUCKY_DRAW_SHOW", TriggerEventParameters.Standard);
		}
	}

	public void OnHide()
	{
		if (m_baconFinisherCollectionDetails.CanHide())
		{
			m_baconFinisherCollectionDetails.Hide();
		}
	}
}
