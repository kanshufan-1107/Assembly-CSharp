using Hearthstone.DataModels;
using Hearthstone.UI;
using UnityEngine;

public class ProductButtonUnavailabilityController : MonoBehaviour
{
	[SerializeField]
	private Widget m_widget;

	private ProductDataModel m_productDataModel;

	private bool m_hasSetup;

	private const string OWNED_EVENT = "OWNED";

	private int m_changedDataVersion;

	private void Start()
	{
		Setup();
		m_changedDataVersion = GetWidgetDataVersion();
	}

	private void Update()
	{
		int currentDataVersion = GetWidgetDataVersion();
		if (base.gameObject.activeSelf && m_changedDataVersion != currentDataVersion)
		{
			m_changedDataVersion = currentDataVersion;
			m_productDataModel = m_widget.GetDataModel<ProductDataModel>();
			OnProductDataChanged(null);
		}
	}

	private int GetWidgetDataVersion()
	{
		if (m_widget == null)
		{
			return 0;
		}
		WidgetTemplate widgetTemplate = m_widget as WidgetTemplate;
		if (!(widgetTemplate != null))
		{
			return 0;
		}
		return widgetTemplate.DataVersion;
	}

	private void Setup()
	{
		if (m_hasSetup)
		{
			return;
		}
		m_productDataModel = ((m_widget != null) ? m_widget.GetDataModel<ProductDataModel>() : null);
		if (m_productDataModel == null)
		{
			return;
		}
		if (IsProductWithSeperateVariants(m_productDataModel))
		{
			foreach (ProductDataModel variant in m_productDataModel.Variants)
			{
				variant.RegisterChangedListener(OnProductDataChanged);
			}
		}
		else
		{
			m_productDataModel.RegisterChangedListener(OnProductDataChanged);
		}
		OnProductDataChanged(null);
		m_hasSetup = true;
	}

	private void OnProductDataChanged(object _)
	{
		if (m_widget == null)
		{
			return;
		}
		if (IsProductWithSeperateVariants(m_productDataModel))
		{
			if (m_productDataModel.Availability != ProductAvailability.ALREADY_OWNED)
			{
				return;
			}
			foreach (ProductDataModel variant in m_productDataModel.Variants)
			{
				if (variant.Availability != ProductAvailability.ALREADY_OWNED)
				{
					return;
				}
			}
			m_widget.TriggerEvent("OWNED");
			return;
		}
		foreach (ProductDataModel variant2 in m_productDataModel.Variants)
		{
			if (variant2.Availability == ProductAvailability.ALREADY_OWNED)
			{
				m_widget.TriggerEvent("OWNED");
				break;
			}
		}
	}

	private bool IsProductWithSeperateVariants(ProductDataModel dataModel)
	{
		if (dataModel.Variants.Count > 1)
		{
			return dataModel.Tags.Contains("mini_set");
		}
		return false;
	}
}
