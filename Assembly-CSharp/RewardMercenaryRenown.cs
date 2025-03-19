using Hearthstone.DataModels;
using Hearthstone.UI;
using UnityEngine;

public class RewardMercenaryRenown : MonoBehaviour
{
	[SerializeField]
	private AsyncReference m_mercenaryRenownReference;

	private Widget m_mercenaryRenownWidget;

	private MercenaryRenownRewardData m_rewardData;

	public void Initialize(MercenaryRenownRewardData rewardData)
	{
		m_rewardData = rewardData;
	}

	private void Start()
	{
		m_mercenaryRenownReference.RegisterReadyListener<Widget>(OnWidgetReady);
	}

	private PriceDataModel GetPriceDataModel()
	{
		if (m_mercenaryRenownWidget == null)
		{
			return null;
		}
		if (!m_mercenaryRenownWidget.GetDataModel(238, out var dataModel))
		{
			dataModel = new PriceDataModel();
			m_mercenaryRenownWidget.BindDataModel(dataModel);
		}
		return dataModel as PriceDataModel;
	}

	private void OnWidgetReady(Widget widget)
	{
		m_mercenaryRenownWidget = widget;
		if (!(m_mercenaryRenownWidget == null))
		{
			PriceDataModel priceDataModel = GetPriceDataModel();
			priceDataModel.Currency = CurrencyType.RENOWN;
			priceDataModel.Amount = m_rewardData.Amount;
			LayerUtils.SetLayer(widget.gameObject, base.gameObject.layer, null);
		}
	}
}
