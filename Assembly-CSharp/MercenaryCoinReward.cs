using Hearthstone.DataModels;
using Hearthstone.UI;
using UnityEngine;

public class MercenaryCoinReward : Reward
{
	public AsyncReference m_mercenaryCoinReference;

	protected Widget m_mercenaryCoinWidget;

	protected bool m_hidden;

	protected override void Start()
	{
		base.Start();
		m_mercenaryCoinReference.RegisterReadyListener<Widget>(OnWidgetReady);
	}

	private void OnWidgetReady(Widget widget)
	{
		m_mercenaryCoinWidget = widget;
		if (!(m_mercenaryCoinWidget == null) && m_hidden)
		{
			m_mercenaryCoinWidget.Hide();
		}
	}

	private LettuceMercenaryCoinDataModel GetMercenaryCoinDataModel()
	{
		if (m_mercenaryCoinWidget == null)
		{
			return null;
		}
		if (!m_mercenaryCoinWidget.GetDataModel(238, out var dataModel))
		{
			dataModel = new LettuceMercenaryCoinDataModel();
			m_mercenaryCoinWidget.BindDataModel(dataModel);
		}
		return dataModel as LettuceMercenaryCoinDataModel;
	}

	protected override void InitData()
	{
		SetData(new MercenaryCoinRewardData(), updateVisuals: false);
	}

	protected override void OnDataSet(bool updateVisuals)
	{
		if (!updateVisuals || m_mercenaryCoinWidget == null)
		{
			return;
		}
		if (!(base.Data is MercenaryCoinRewardData coinRewardData))
		{
			Debug.LogWarning($"MercenaryCoinReward.OnDataSet() - data {base.Data} is not MercenaryCoinRewardData");
			return;
		}
		LettuceMercenaryDbfRecord mercenaryRecord = GameDbf.LettuceMercenary.GetRecord(coinRewardData.MercenaryId);
		if (mercenaryRecord == null)
		{
			Debug.LogWarning($"MercenaryCoinReward.OnDataSet() - data {coinRewardData.MercenaryId} has invalid mercenary id");
			return;
		}
		SetReady(ready: false);
		string mercenaryCardId = GameUtils.GetCardIdFromMercenaryId(mercenaryRecord.ID);
		EntityDef entityDef = DefLoader.Get().GetEntityDef(mercenaryCardId);
		LettuceMercenaryCoinDataModel mercenaryCoinDataModel = GetMercenaryCoinDataModel();
		mercenaryCoinDataModel.MercenaryId = mercenaryCoinDataModel.MercenaryId;
		mercenaryCoinDataModel.MercenaryName = entityDef.GetName();
		mercenaryCoinDataModel.Quantity = mercenaryCoinDataModel.Quantity;
		mercenaryCoinDataModel.GlowActive = true;
	}

	protected override void ShowReward(bool updateCacheValues)
	{
		m_hidden = false;
		m_root.SetActive(value: true);
		if (m_mercenaryCoinWidget != null)
		{
			m_mercenaryCoinWidget.Show();
			OnDataSet(updateVisuals: true);
		}
	}

	protected override void HideReward()
	{
		base.HideReward();
		m_root.SetActive(value: false);
		m_hidden = true;
		if (m_mercenaryCoinWidget != null)
		{
			m_mercenaryCoinWidget.Hide();
		}
	}
}
