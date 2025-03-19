using System;
using Blizzard.T5.Services;
using Hearthstone.DataModels;
using Hearthstone.Store;

public class VirtualCurrencyPurchasePage : ProductPage
{
	public class VirtualCurrencyPurchaseArgs : EventArgs
	{
		public string ProductId { get; set; }
	}

	public EventHandler<VirtualCurrencyPurchaseArgs> OnVCPurchaseSuccessful;

	private bool m_closeOnPurchase;

	protected override void Start()
	{
		base.Start();
		StoreManager.Get()?.RegisterSuccessfulPurchaseAckListener(HandleSuccessfulPurchaseAck);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		StoreManager.Get()?.RemoveSuccessfulPurchaseAckListener(HandleSuccessfulPurchaseAck);
	}

	public void OpenToSKU(float desiredAmount, bool closeOnPurchase = false)
	{
		ProductDataModel variant = null;
		if (ShopUtils.TryGetMainVirtualCurrencyType(out var currencyType))
		{
			variant = ShopUtils.FindCurrencyProduct(currencyType, desiredAmount);
		}
		OpenToSKU(variant, closeOnPurchase);
	}

	public void OpenToSKU(ProductDataModel vcVariant, bool closeOnPurchase = false)
	{
		base.Open();
		m_closeOnPurchase = closeOnPurchase;
		if (vcVariant == null)
		{
			Log.Store.PrintError("Invalid Virtual Currency variant was provided.");
			return;
		}
		if (!ServiceManager.TryGet<IProductDataService>(out var dataService))
		{
			Log.Store.PrintError("Product service not initialized.");
			return;
		}
		ProductDataModel vcRootProduct = dataService.VirtualCurrencyProductItem;
		if (!vcRootProduct.Variants.Contains(vcVariant))
		{
			Log.Store.PrintError("Attempted to display Product PMT ID = {0}, Name = {1} as Virtual Currency", vcVariant.PmtId, vcVariant.Name);
		}
		else
		{
			SetProduct(vcRootProduct, vcVariant);
		}
	}

	private void HandleSuccessfulPurchaseAck(ProductInfo bundle, PaymentMethod paymentMethod)
	{
		if (base.IsOpen && m_closeOnPurchase)
		{
			Close();
			OnVCPurchaseSuccessful?.Invoke(this, new VirtualCurrencyPurchaseArgs
			{
				ProductId = bundle.Id.ToString()
			});
		}
	}
}
