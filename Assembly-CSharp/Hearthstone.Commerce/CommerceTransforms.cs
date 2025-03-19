using System.Linq;
using PegasusUtil;

namespace Hearthstone.Commerce;

public static class CommerceTransforms
{
	public static Network.Bundle ToNetBundle(this Bundle pegBundle, Currency currency)
	{
		Network.Bundle bundle = new Network.Bundle
		{
			IsPrePurchase = pegBundle.IsPrePurchase,
			PMTProductID = pegBundle.PmtProductId,
			DisplayName = (pegBundle.HasDisplayName ? DbfUtils.ConvertFromProtobuf(pegBundle.DisplayName) : null),
			DisplayDescription = (pegBundle.HasDisplayDesc ? DbfUtils.ConvertFromProtobuf(pegBundle.DisplayDesc) : null),
			Attributes = CommerceUtils.ConvertAttributes(pegBundle.Attributes),
			SaleIds = pegBundle.SaleIds.ToList(),
			VisibleOnSalePeriodOnly = pegBundle.VisibleOnSalePeriodOnly,
			DisableRealMoneyShopFlags = (pegBundle.HasDisableRealMoneyShopFlags ? ((MobileShopType)pegBundle.DisableRealMoneyShopFlags) : MobileShopType.MOBILE_SHOP_TYPE_NONE)
		};
		bundle.DisplayName?.StripUnusedLocales();
		bundle.DisplayDescription?.StripUnusedLocales();
		bundle.IsPrePurchase = bundle.Attributes.GetTags().Contains("prepurchase");
		if (pegBundle.HasCost && pegBundle.Cost.CurrentCost != 0)
		{
			bundle.Cost = new Network.Bundle.CostInfo
			{
				CurrentCost = (long)pegBundle.Cost.CurrentCost,
				OriginalCost = (long)(pegBundle.Cost.HasOriginalCost ? pegBundle.Cost.OriginalCost : pegBundle.Cost.CurrentCost)
			};
		}
		if (pegBundle.HasGoldCost && pegBundle.GoldCost.CurrentCost != 0)
		{
			bundle.GtappGoldCost = new Network.Bundle.CostInfo
			{
				CurrentCost = (long)pegBundle.GoldCost.CurrentCost,
				OriginalCost = (long)(pegBundle.GoldCost.HasOriginalCost ? pegBundle.GoldCost.OriginalCost : pegBundle.GoldCost.CurrentCost)
			};
		}
		if (pegBundle.HasVirtualCurrencyCost && pegBundle.VirtualCurrencyCost.HasCost && pegBundle.VirtualCurrencyCost.Cost.CurrentCost != 0)
		{
			bundle.VirtualCurrencyCost = new Network.Bundle.CostInfo
			{
				CurrentCost = (long)pegBundle.VirtualCurrencyCost.Cost.CurrentCost,
				OriginalCost = (long)(pegBundle.VirtualCurrencyCost.Cost.HasOriginalCost ? pegBundle.VirtualCurrencyCost.Cost.OriginalCost : pegBundle.VirtualCurrencyCost.Cost.CurrentCost)
			};
			bundle.VirtualCurrencyCode = pegBundle.VirtualCurrencyCost.CurrencyCode;
		}
		if (pegBundle.HasProductEventName)
		{
			bundle.ProductEvent = pegBundle.ProductEventName;
		}
		bundle.Items = pegBundle.Items.Select(ToNetBundleItem).ToList();
		return bundle;
	}

	private static Network.BundleItem ToNetBundleItem(this BundleItem utilBundleItem)
	{
		return new Network.BundleItem
		{
			ItemType = utilBundleItem.ProductType,
			ProductData = utilBundleItem.Data,
			Quantity = utilBundleItem.Quantity,
			BaseQuantity = utilBundleItem.BaseQuantity,
			Attributes = CommerceUtils.ConvertAttributes(utilBundleItem.Attributes),
			IsBlocking = utilBundleItem.IsBlocking,
			HasShopAvailableDate = utilBundleItem.HasShopAvailableDate,
			ShopAvailableDate = utilBundleItem.ShopAvailableDate
		};
	}
}
