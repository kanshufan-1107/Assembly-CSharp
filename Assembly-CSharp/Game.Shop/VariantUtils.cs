using System.Collections.Generic;
using System.Linq;
using Hearthstone.DataModels;

namespace Game.Shop;

public static class VariantUtils
{
	public interface ISortOrder
	{
		int Grouped(ProductDataModel product);

		int Ungrouped(ProductDataModel product);
	}

	public static List<ProductDataModel> GetVariantsByItemType(RewardItemType itemType, int itemId, IEnumerable<ProductDataModel> products, ISortOrder sortOrder)
	{
		return GetUngroupedVariants(itemType, itemId, products, sortOrder);
	}

	public static List<ProductDataModel> GetGroupedVariants(RewardItemType itemType, IEnumerable<ProductDataModel> products, ISortOrder sortOrder)
	{
		return products.Where((ProductDataModel product) => IsSingleItemProductOfType(product, itemType)).OrderBy(sortOrder.Grouped).ToList();
	}

	public static List<ProductDataModel> GetUngroupedVariants(RewardItemType itemType, int itemId, IEnumerable<ProductDataModel> products, ISortOrder sortOrder)
	{
		return products.Where((ProductDataModel product) => IsSingleItemProductOfTypeWithId(product, itemType, itemId)).OrderBy(sortOrder.Ungrouped).ToList();
	}

	public static bool IsSingleItemProductOfType(ProductDataModel product, RewardItemType itemType)
	{
		if (product.IsSingleItemProduct())
		{
			return product.MatchesItemType(itemType);
		}
		return false;
	}

	public static bool IsSingleItemProductOfTypeWithId(ProductDataModel product, RewardItemType itemType, int itemId)
	{
		if (product.IsSingleItemProduct() && product.MatchesItemType(itemType))
		{
			return product.MatchesItemId(itemId);
		}
		return false;
	}

	public static bool TryFindSpecialOfferVariant(ProductDataModel product, out ProductDataModel specialOfferVariant)
	{
		specialOfferVariant = null;
		if (product != null)
		{
			foreach (ProductDataModel variant in product.Variants)
			{
				if (variant.Tags.Contains("special_offer") && variant.Availability == ProductAvailability.CAN_PURCHASE)
				{
					specialOfferVariant = variant;
					return true;
				}
			}
		}
		return false;
	}

	internal static IEnumerable<ProductDataModel> GetMiniSetVariants(ProductDataModel primaryProduct, IEnumerable<ProductDataModel> m_products)
	{
		if (!primaryProduct.Tags.Contains("mini_set"))
		{
			Log.Store.PrintWarning("This API is only for Mini Sets");
			yield break;
		}
		IEnumerable<int> primaryMiniSetItems = (from i in primaryProduct.Items
			where i.ItemType == RewardItemType.MINI_SET
			select i into x
			select x.ItemId).Distinct();
		if (!primaryMiniSetItems.Any())
		{
			Log.Store.PrintError($"Found mini set with no mini set items {primaryProduct.PmtId}");
			yield break;
		}
		foreach (ProductDataModel product in m_products)
		{
			if (product.PmtId == 0L)
			{
				continue;
			}
			if (product.PmtId == primaryProduct.PmtId)
			{
				yield return product;
			}
			else if (product.Tags.Contains("mini_set"))
			{
				IEnumerable<int> miniSetItems = (from i in product.Items
					where i.ItemType == RewardItemType.MINI_SET
					select i into x
					select x.ItemId).Distinct();
				if (primaryMiniSetItems.Count() == miniSetItems.Count() && !miniSetItems.Where((int x) => !primaryMiniSetItems.Contains(x)).Any())
				{
					yield return product;
				}
			}
		}
	}

	internal static List<ProductDataModel> GetVariantsWithAllItemsMatching(ProductDataModel primaryProduct, List<ProductDataModel> m_products)
	{
		List<ProductDataModel> result = null;
		int primaryItemCount = primaryProduct.Items.Count;
		foreach (ProductDataModel product in m_products)
		{
			if (product.PmtId != 0L && product.PmtId == primaryProduct.PmtId)
			{
				if (result == null)
				{
					result = new List<ProductDataModel>();
				}
				result.Add(product);
			}
			else if (primaryProduct.Tags.Contains("sellable_deck_bundle") && product.Tags.Contains("sellable_deck_bundle"))
			{
				if (result == null)
				{
					result = new List<ProductDataModel>();
				}
				result.Add(product);
			}
			else
			{
				if (product.Items.Count != primaryItemCount)
				{
					continue;
				}
				bool foundIt = false;
				foreach (RewardItemDataModel primaryItem in primaryProduct.Items)
				{
					foundIt = false;
					foreach (RewardItemDataModel item in product.Items)
					{
						if (item.ItemType == primaryItem.ItemType && item.ItemId == primaryItem.ItemId)
						{
							foundIt = true;
							break;
						}
					}
					if (!foundIt)
					{
						break;
					}
				}
				if (foundIt)
				{
					if (result == null)
					{
						result = new List<ProductDataModel>();
					}
					result.Add(product);
				}
			}
		}
		result.Sort(delegate(ProductDataModel a, ProductDataModel b)
		{
			if (a.Tags.Contains("golden"))
			{
				return -1;
			}
			return b.Tags.Contains("golden") ? 1 : a.Items[0].ItemId.CompareTo(b.Items[0].ItemId);
		});
		return result;
	}
}
