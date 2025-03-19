using Hearthstone.DataModels;
using Hearthstone.UI;
using UnityEngine;

public class LuckyDrawBoardSkinDetails : MonoBehaviour
{
	[SerializeField]
	private GameObject m_scaleParentOverride;

	[SerializeField]
	private AsyncReference m_baconBoardSkinCollectionDetailsReference;

	private BaconBoardCollectionDetails m_baconBoardSkinCollectionDetails;

	public bool ShowingRewardGrantVFX { get; set; }

	private void Start()
	{
		m_baconBoardSkinCollectionDetailsReference.RegisterReadyListener<BaconBoardCollectionDetails>(OnBoardDetailsReady);
	}

	private void OnBoardDetailsReady(BaconBoardCollectionDetails collectionDetails)
	{
		m_baconBoardSkinCollectionDetails = collectionDetails;
		m_baconBoardSkinCollectionDetails.SetScaleParentOverride(m_scaleParentOverride);
	}

	private void AssignDataModel(LuckyDrawRewardDataModel boardSkinData)
	{
		if (boardSkinData == null)
		{
			LuckyDrawManager.Get().LogError("Error [LuckyDrawBoardSkinDetails] AssignDataModel boardSkindData was null!");
			return;
		}
		if (boardSkinData.RewardList == null)
		{
			LuckyDrawManager.Get().LogError("Error [LuckyDrawBoardSkinDetails] AssignDataModel rewardList was null!");
			return;
		}
		if (boardSkinData.RewardList.Items == null || boardSkinData.RewardList.Items.Count <= 0)
		{
			LuckyDrawManager.Get().LogError("Error [LuckyDrawBoardSkinDetails] AssignDataModel rewardList items were null or empty!");
			return;
		}
		if (boardSkinData.RewardList.Items[0].BGBoardSkin == null)
		{
			LuckyDrawManager.Get().LogError("Error [LuckyDrawBoardSkinDetails] AssignDataModel rewardList Item was not a boardSkin!");
			return;
		}
		if (boardSkinData.RewardList.Items.Count > 1)
		{
			LuckyDrawManager.Get().LogWarning("Warning [LuckyDrawBoardSkinDetails] Only 1 reward item is expected, multiple reward items will not be displayed!");
		}
		m_baconBoardSkinCollectionDetails.AssignDataModels(boardSkinData.RewardList.Items[0].BGBoardSkin, null);
	}

	public void OnShow(IDataModel dataModel)
	{
		AssignDataModel(dataModel as LuckyDrawRewardDataModel);
		m_baconBoardSkinCollectionDetails.Show();
		if (ShowingRewardGrantVFX)
		{
			EventFunctions.TriggerEvent(m_baconBoardSkinCollectionDetails.transform.parent, "LUCKY_DRAW_SHOW_REWARD", TriggerEventParameters.Standard);
		}
		else
		{
			EventFunctions.TriggerEvent(m_baconBoardSkinCollectionDetails.transform.parent, "LUCKY_DRAW_SHOW", TriggerEventParameters.Standard);
		}
	}

	public void OnHide()
	{
		if (m_baconBoardSkinCollectionDetails.CanHide())
		{
			m_baconBoardSkinCollectionDetails.Hide();
		}
	}
}
