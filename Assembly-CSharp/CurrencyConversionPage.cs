using System;
using System.Linq;
using Blizzard.T5.Services;
using Hearthstone.DataModels;
using Hearthstone.Store;
using UnityEngine;

public class CurrencyConversionPage : ProductPage
{
	[SerializeField]
	private PegUIElement m_buttonIncrease;

	[SerializeField]
	private PegUIElement m_buttonDecrease;

	[SerializeField]
	private ScrollbarControl m_slider;

	[SerializeField]
	[Tooltip("Widget event when the player can afford to convert")]
	private string m_affordableEventName = "AFFORDABLE";

	[Tooltip("Widget event when the player cannot afford conversion")]
	[SerializeField]
	private string m_unaffordableEventName = "UNAFFORDABLE";

	private const int MINIMUM_SELECTION = 1;

	private float m_baseQuantity;

	private int m_selectedQuantity = 1;

	private int m_maxAffordable;

	private RangeInt m_sliderRange;

	protected override void Start()
	{
		base.Start();
		SetupIncrementerButton(m_buttonIncrease, 1);
		SetupIncrementerButton(m_buttonDecrease, -1);
		if (m_slider != null)
		{
			m_slider.SetUpdateHandler(OnSliderUpdated);
			m_slider.SetFinishHandler(OnSliderFinished);
		}
		if (ServiceManager.TryGet<CurrencyManager>(out var currencyManager))
		{
			currencyManager.AddCurrencyBalanceChangedCallback(HandleCurrencyBalanceChanged);
		}
		StoreManager.Get().RegisterSuccessfulPurchaseAckListener(HandleSuccessfulPurchaseAck);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (ServiceManager.TryGet<CurrencyManager>(out var currencyManager))
		{
			currencyManager.RemoveCurrencyBalanceChangedCallback(HandleCurrencyBalanceChanged);
		}
		StoreManager.Get().RemoveSuccessfulPurchaseAckListener(HandleSuccessfulPurchaseAck);
	}

	public void OpenToSKU(float desiredAmount)
	{
		Open();
		if (m_baseQuantity > 0f)
		{
			m_selectedQuantity = ClampSelection(Mathf.CeilToInt(desiredAmount / m_baseQuantity));
			UpdateQuantity();
		}
	}

	public override void Open()
	{
		base.Open();
		if (m_productImmutable == null)
		{
			ProductDataModel product = m_widget.GetDataModel<ProductDataModel>();
			SetProduct(product, product);
		}
		m_baseQuantity = GetCurrencyItem()?.Currency.Amount ?? 0f;
		UpdateConstraints();
	}

	private void OnSliderUpdated(float val)
	{
		int quantity = Mathf.RoundToInt((float)m_sliderRange.start + val * (float)m_sliderRange.length);
		if (m_selectedQuantity != quantity)
		{
			m_selectedQuantity = quantity;
			UpdateModel();
		}
	}

	private void OnSliderFinished()
	{
		UpdateSlider();
	}

	private void HandleCurrencyBalanceChanged(CurrencyBalanceChangedEventArgs args)
	{
		if (base.IsOpen)
		{
			RewardItemDataModel currencyItem = GetCurrencyItem();
			PriceDataModel price = GetPrice();
			if ((currencyItem != null && args.Currency == currencyItem.Currency.Currency) || (price != null && args.Currency == price.Currency))
			{
				UpdateConstraints();
			}
		}
	}

	private void HandleSuccessfulPurchaseAck(ProductInfo bundle, PaymentMethod purchaseMethod)
	{
		Close();
	}

	private RewardItemDataModel GetCurrencyItem()
	{
		ProductDataModel currencyProduct = m_productImmutable;
		if (currencyProduct == null)
		{
			Log.Store.PrintError("No currency conversion product set");
			return null;
		}
		RewardItemDataModel currencyItem = currencyProduct.Items.FirstOrDefault((RewardItemDataModel i) => i.Currency != null);
		if (currencyItem == null || currencyItem.Currency == null || currencyItem.Currency.Amount == 0f)
		{
			Log.Store.PrintError("No currency found on product {0}", currencyProduct.Name);
			return null;
		}
		return currencyItem;
	}

	private PriceDataModel GetPrice()
	{
		if (m_productImmutable == null)
		{
			return null;
		}
		return m_productImmutable.Prices.FirstOrDefault();
	}

	private void SetupIncrementerButton(PegUIElement ui, int increment)
	{
		if (!(ui == null))
		{
			ui.AddEventListener(UIEventType.PRESS, delegate
			{
				IncrementQuantity(increment);
			});
			ui.AddEventListener(UIEventType.HOLD, delegate
			{
				IncrementQuantity(increment);
			});
		}
	}

	private void IncrementQuantity(int delta)
	{
		m_selectedQuantity = ClampSelection(m_selectedQuantity + delta);
		UpdateQuantity();
	}

	private void UpdateConstraints()
	{
		ProductDataModel currencyProduct = m_productImmutable;
		if (currencyProduct == null)
		{
			Log.Store.PrintError("Unable to update VC conversion constraints; no product set");
			return;
		}
		PriceDataModel price = GetPrice();
		if (price == null || price.Amount == 0f)
		{
			Log.Store.PrintError("No price on currency product {0}", currencyProduct.Name);
			return;
		}
		if (!ServiceManager.TryGet<CurrencyManager>(out var currencyManager))
		{
			Log.Store.PrintError("Unable to update VC conversion constraints; no currency manager");
			return;
		}
		long balance = currencyManager.GetBalance(price.Currency);
		m_productSelection.MaxQuantity = currencyProduct.GetMaxBulkPurchaseCount();
		m_maxAffordable = Math.Min(Mathf.FloorToInt((float)balance / price.Amount), m_productSelection.MaxQuantity);
		m_sliderRange.start = Math.Min(1, m_maxAffordable);
		m_sliderRange.length = m_maxAffordable - m_sliderRange.start;
		m_selectedQuantity = ClampSelection(m_selectedQuantity);
		m_widget.TriggerEvent((m_maxAffordable > 0) ? m_affordableEventName : m_unaffordableEventName);
		UpdateQuantity();
	}

	private void UpdateQuantity()
	{
		UpdateModel();
		UpdateSlider();
	}

	private void UpdateModel()
	{
		SetVariantQuantityAndUpdateDataModel(m_productImmutable, m_selectedQuantity);
		base.Selection.MaxQuantity = m_maxAffordable;
	}

	private void UpdateSlider()
	{
		if (m_slider != null)
		{
			if (m_sliderRange.length > 0)
			{
				m_slider.SetValue((float)(m_selectedQuantity - m_sliderRange.start) / (float)m_sliderRange.length);
			}
			else
			{
				m_slider.SetValue((m_selectedQuantity != 0) ? 1 : 0);
			}
		}
	}

	private int ClampSelection(int amount)
	{
		return Math.Max(1, Mathf.Clamp(amount, m_sliderRange.start, m_sliderRange.end));
	}
}
