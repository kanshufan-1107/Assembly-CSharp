using Blizzard.T5.Core.Utils;

namespace Assets;

public static class BoxProductBanner
{
	public enum ProductActionType
	{
		OPEN_SHOP,
		OPEN_SHOP_TO_PRODUCT
	}

	public static ProductActionType ParseProductActionTypeValue(string value)
	{
		EnumUtils.TryGetEnum<ProductActionType>(value, out var e);
		return e;
	}
}
