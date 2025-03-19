using Hearthstone.DataModels;

public static class ShopBrowserButtonExtensions
{
	public static string GetName(this ShopBrowserButtonDataModel shopBrowserButtonDataModel)
	{
		string productName = "Empty";
		string size = "Unknown";
		if (shopBrowserButtonDataModel != null)
		{
			if (shopBrowserButtonDataModel.DisplayProduct != null)
			{
				productName = shopBrowserButtonDataModel.DisplayProduct.Name;
			}
			if (shopBrowserButtonDataModel.SlotWidth > 0 && shopBrowserButtonDataModel.SlotHeight > 0)
			{
				size = $"{shopBrowserButtonDataModel.SlotWidth}x{shopBrowserButtonDataModel.SlotHeight}";
			}
		}
		return "Product: " + productName + ", Size: " + size;
	}
}
