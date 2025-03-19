using Hearthstone.DataModels;
using Hearthstone.UI;
using UnityEngine;

public class ProductPriceController : MonoBehaviour
{
	[SerializeField]
	private Widget m_widget;

	[SerializeField]
	private AsyncReference m_priceTagIconsWidgetRef;

	private Widget m_priceTagIconsWidget;

	private CurrencyListDataModel m_currencyListDataModel;

	private void Awake()
	{
		m_widget.RegisterEventListener(HandleEvent);
		m_priceTagIconsWidgetRef.RegisterReadyListener<Widget>(OnPriceTagIconsWidgetReady);
	}

	private void HandleEvent(string eventName)
	{
		ProductDataModel productDataModel = m_widget.GetDataModel<ProductDataModel>();
		if (productDataModel == null)
		{
			return;
		}
		CurrencyListDataModel currencyListDataModel = null;
		switch (eventName)
		{
		case "ICON_ONLY_CODE":
			currencyListDataModel = productDataModel.GetAllCurrencies(includeVariants: true);
			break;
		case "SINGLE_PRICE_ICON_CODE":
			currencyListDataModel = productDataModel.GetAllCurrencies(includeVariants: true);
			if (productDataModel.Prices.Count >= 1)
			{
				RemoveCurrencyFromAvailable(currencyListDataModel, productDataModel.Prices[0]);
			}
			break;
		case "DOUBLE_PRICE_ICON_CODE":
			currencyListDataModel = productDataModel.GetAllCurrencies(includeVariants: true);
			if (productDataModel.Prices.Count >= 2)
			{
				RemoveCurrencyFromAvailable(currencyListDataModel, productDataModel.Prices[0]);
				RemoveCurrencyFromAvailable(currencyListDataModel, productDataModel.Prices[1]);
			}
			break;
		}
		if (currencyListDataModel != null)
		{
			m_currencyListDataModel = currencyListDataModel;
			if (m_priceTagIconsWidget != null)
			{
				m_priceTagIconsWidget.BindDataModel(currencyListDataModel);
			}
		}
	}

	private void RemoveCurrencyFromAvailable(CurrencyListDataModel currencyListDataModel, PriceDataModel priceToRemove)
	{
		if (currencyListDataModel != null)
		{
			if (priceToRemove.Currency == CurrencyType.REAL_MONEY)
			{
				currencyListDataModel.ISOCode = string.Empty;
			}
			else
			{
				currencyListDataModel.VCCurrency.Remove(priceToRemove.Currency);
			}
		}
	}

	private void OnPriceTagIconsWidgetReady(Widget priceTagIconsWidget)
	{
		if (!(priceTagIconsWidget == null))
		{
			m_priceTagIconsWidget = priceTagIconsWidget;
			if (m_currencyListDataModel != null)
			{
				m_priceTagIconsWidget.BindDataModel(m_currencyListDataModel);
			}
		}
	}
}
