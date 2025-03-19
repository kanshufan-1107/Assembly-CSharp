using Blizzard.Commerce;
using Hearthstone.Store;

public class BuyPmtProductEventArgs : BuyProductEventArgs
{
	public readonly long pmtProductId;

	public readonly CurrencyType paymentCurrency;

	public override CurrencyType PaymentCurrency => paymentCurrency;

	public BuyPmtProductEventArgs(ProductId pmtProductId, CurrencyType paymentCurrency, int quantity)
		: this(pmtProductId.Value, paymentCurrency, quantity)
	{
	}

	public BuyPmtProductEventArgs(long pmtProductId, CurrencyType paymentCurrency, int quantity)
	{
		this.pmtProductId = pmtProductId;
		base.quantity = quantity;
		this.paymentCurrency = paymentCurrency;
	}

	public BuyPmtProductEventArgs(ProductInfo bundle, CurrencyType paymentCurrency, int quantity)
		: this(bundle.Id, paymentCurrency, quantity)
	{
	}
}
