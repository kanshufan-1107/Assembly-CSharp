namespace Hearthstone.Store;

public struct PriceInfo
{
	public struct Price
	{
		public long RealPrice;

		public string DisplayPrice;
	}

	public Price CurrentPrice;

	public Price OriginalPrice;
}
