using Hearthstone.DataModels;
using Hearthstone.UI;
using UnityEngine;

public class LuckyDrawTile : MonoBehaviour
{
	private Widget m_widget;

	[SerializeField]
	private AsyncReference rewardItemDisplayAsyncReference;

	private void Awake()
	{
		m_widget = GetComponent<WidgetTemplate>();
		if (m_widget == null)
		{
			LuckyDrawManager.Get().LogError("Error [LuckyDrawTile] WidgetTemplate not found on " + base.gameObject.name);
		}
		else if (rewardItemDisplayAsyncReference == null)
		{
			LuckyDrawManager.Get().LogError("Error [LuckyDrawTile] rewardItemDisplayAsyncReference was not found on " + base.gameObject.name);
		}
		else
		{
			rewardItemDisplayAsyncReference.RegisterReadyListener<WidgetInstance>(OnWidgetReady);
		}
	}

	private void OnWidgetReady(WidgetInstance widget)
	{
		WidgetTemplate childTemplate = widget.Widget;
		IDataModel dataModel;
		if (childTemplate == null)
		{
			LuckyDrawManager.Get().LogError("Error [LuckyDrawTile] InitializeRewardItemWidget() Could not find WidgetTemplate child on m_widget! From object " + base.gameObject.name);
		}
		else if (m_widget.GetDataModel(34, out dataModel))
		{
			RewardListDataModel rewardList = dataModel as RewardListDataModel;
			childTemplate.BindDataModel(rewardList);
		}
	}

	public LuckyDrawRewardDataModel GetBoundRewardDataModel()
	{
		if (m_widget.GetDataModel(667, out var dataModel))
		{
			return dataModel as LuckyDrawRewardDataModel;
		}
		return null;
	}
}
