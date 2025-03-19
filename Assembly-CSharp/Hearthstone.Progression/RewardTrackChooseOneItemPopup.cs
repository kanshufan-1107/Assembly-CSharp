using System.Linq;
using Hearthstone.DataModels;
using Hearthstone.UI;
using UnityEngine;

namespace Hearthstone.Progression;

[RequireComponent(typeof(WidgetTemplate))]
public class RewardTrackChooseOneItemPopup : MonoBehaviour
{
	public const string TURN_PAGE_LEFT = "CODE_TURN_PAGE_LEFT";

	public const string TURN_PAGE_RIGHT = "CODE_TURN_PAGE_RIGHT";

	public const string FROM_ACHIEVE = "FROM_ACHIEVE";

	public const string FROM_TRACK = "FROM_TRACK";

	[SerializeField]
	private int m_numberOfItemsPerPage = 4;

	private WidgetTemplate m_widget;

	private PageInfoDataModel m_pageInfo = new PageInfoDataModel();

	private RewardListDataModel m_rewardListDataModel;

	private void Awake()
	{
		m_widget = GetComponent<WidgetTemplate>();
		m_widget.BindDataModel(m_pageInfo);
		m_widget.RegisterEventListener(HandleEvent);
		m_numberOfItemsPerPage = Mathf.Max(1, m_numberOfItemsPerPage);
	}

	private void HandleEvent(string eventName)
	{
		switch (eventName)
		{
		case "CODE_TURN_PAGE_LEFT":
			SetPageData(m_pageInfo.PageNumber - 1);
			break;
		case "CODE_TURN_PAGE_RIGHT":
			SetPageData(m_pageInfo.PageNumber + 1);
			break;
		case "FROM_ACHIEVE":
			HandleFromAchieve();
			break;
		case "FROM_TRACK":
			m_rewardListDataModel = m_widget.GetDataModel<RewardTrackNodeRewardsDataModel>().Items.CloneDataModel();
			SetPageData(1);
			break;
		}
	}

	private void SetPageData(int pageNumber)
	{
		if (m_rewardListDataModel == null)
		{
			Debug.LogError("RewardTrackChooseOneItemPopup.SetPageData: RewardTrackNodeRewardsDataModel was not bound!");
			return;
		}
		m_pageInfo.ItemsPerPage = m_numberOfItemsPerPage;
		m_pageInfo.TotalPages = Mathf.CeilToInt((float)m_rewardListDataModel.Items.Count / (float)m_numberOfItemsPerPage);
		m_pageInfo.PageNumber = Mathf.Clamp(pageNumber, 1, m_pageInfo.TotalPages);
		if (m_pageInfo.PageNumber == pageNumber)
		{
			int startIndex = m_numberOfItemsPerPage * (pageNumber - 1);
			int endIndex = startIndex + m_numberOfItemsPerPage - 1;
			RewardListDataModel tempRewardListDataModel = new RewardListDataModel();
			tempRewardListDataModel.Items = m_rewardListDataModel.Items.Where((RewardItemDataModel item, int index) => index >= startIndex && index <= endIndex).ToDataModelList();
			int delta = m_numberOfItemsPerPage - tempRewardListDataModel.Items.Count;
			tempRewardListDataModel.Items.AddRange(Enumerable.Repeat(new RewardItemDataModel(), delta).ToArray());
			m_widget.BindDataModel(tempRewardListDataModel);
		}
	}

	private void HandleFromAchieve()
	{
		EventDataModel eventDataModel = m_widget.GetDataModel<EventDataModel>();
		if (eventDataModel.Payload is AchievementDataModel)
		{
			AchievementDataModel achievement = eventDataModel.Payload as AchievementDataModel;
			UpdateRewardModelWithOwnership(achievement.ID);
			SetPageData(1);
		}
	}

	private void UpdateRewardModelWithOwnership(int achievementId)
	{
		AchievementDbfRecord achievementRecord = GameDbf.Achievement.GetRecord(achievementId);
		if (achievementRecord == null)
		{
			return;
		}
		RewardListDataModel updatedRewardListModel = RewardUtils.CreateRewardListDataModelFromRewardListId(achievementRecord.RewardList);
		if (updatedRewardListModel != null)
		{
			m_rewardListDataModel = updatedRewardListModel;
			m_rewardListDataModel.Items = updatedRewardListModel.Items.OrderBy((RewardItemDataModel item) => item, new RewardUtils.RewardOwnedItemComparer()).ToDataModelList();
		}
	}
}
