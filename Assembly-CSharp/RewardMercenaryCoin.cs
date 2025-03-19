using Hearthstone.DataModels;
using Hearthstone.UI;
using UnityEngine;

public class RewardMercenaryCoin : MonoBehaviour
{
	public AsyncReference m_mercenaryCoinReference;

	private Widget m_mercenaryCoinWidget;

	private MercenaryCoinRewardData m_rewardData;

	public void Initialize(MercenaryCoinRewardData rewardData)
	{
		m_rewardData = rewardData;
	}

	private void Start()
	{
		m_mercenaryCoinReference.RegisterReadyListener<Widget>(OnWidgetReady);
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

	private void OnWidgetReady(Widget widget)
	{
		m_mercenaryCoinWidget = widget;
		if (!(m_mercenaryCoinWidget == null))
		{
			string mercenaryCardId = GameUtils.GetCardIdFromMercenaryId(m_rewardData.MercenaryId);
			EntityDef entityDef = DefLoader.Get().GetEntityDef(mercenaryCardId);
			LettuceMercenaryCoinDataModel mercenaryCoinDataModel = GetMercenaryCoinDataModel();
			mercenaryCoinDataModel.MercenaryId = m_rewardData.MercenaryId;
			mercenaryCoinDataModel.MercenaryName = entityDef.GetName();
			mercenaryCoinDataModel.Quantity = m_rewardData.Quantity;
			mercenaryCoinDataModel.GlowActive = true;
			mercenaryCoinDataModel.NameActive = true;
			LayerUtils.SetLayer(widget.gameObject, base.gameObject.layer, null);
		}
	}
}
