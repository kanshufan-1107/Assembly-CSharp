namespace Hearthstone.Store;

public struct RealPriceInfo
{
	public struct Price
	{
		public double RealPrice;

		public string DisplayPrice;
	}

	public Price CurrentPrice;

	public Price OriginalPrice;

	public string CurrencyCode;
}
