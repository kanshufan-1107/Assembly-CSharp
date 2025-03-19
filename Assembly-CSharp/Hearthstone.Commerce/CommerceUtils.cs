using System.Collections.Generic;
using System.Linq;
using com.blizzard.commerce.Model;
using PegasusUtil;
using Shared.Scripts.Game.Shop.Product;

namespace Hearthstone.Commerce;

public static class CommerceUtils
{
	public static bool TryGetRealMoneyPrice(this IEnumerable<ProductPrice> prices, out ProductPrice price)
	{
		price = null;
		if (prices == null)
		{
			return false;
		}
		int numFound = 0;
		ProductPrice foundPrice = null;
		foreach (ProductPrice p in prices)
		{
			if (p != null)
			{
				if (p.currencyTypeId == 1)
				{
					numFound++;
					foundPrice = p;
				}
				if (numFound == 1)
				{
					price = foundPrice;
					return true;
				}
			}
		}
		return false;
	}

	public static bool TryGetRealMoneyPrice(this IEnumerable<ProductPrice> prices, IList<string> currencyCodes, out ProductPrice price)
	{
		price = null;
		if (prices == null)
		{
			return false;
		}
		int lastFoundIndex = -1;
		foreach (ProductPrice p in prices)
		{
			if (p == null || p.currencyTypeId != 1)
			{
				continue;
			}
			if (currencyCodes == null || currencyCodes.Count() == 0)
			{
				price = p;
				return true;
			}
			if (!string.IsNullOrEmpty(p.currencyCode))
			{
				int foundItemIndex = currencyCodes.IndexOf(p.currencyCode);
				switch (foundItemIndex)
				{
				case 0:
					price = p;
					return true;
				case -1:
					Log.Store.PrintDebug($"[TryGetRealMoneyPrice] Found price with no matching code {p}");
					if (lastFoundIndex == -1)
					{
						price = p;
					}
					break;
				default:
					if (lastFoundIndex == -1 || lastFoundIndex > foundItemIndex)
					{
						price = p;
						lastFoundIndex = foundItemIndex;
					}
					break;
				}
			}
			else
			{
				Log.Store.PrintDebug($"[TryGetRealMoneyPrice] Not currency code on this price {p}");
				if (lastFoundIndex == -1)
				{
					price = p;
				}
			}
		}
		return price != null;
	}

	public static bool TryGetGoldPrice(this IEnumerable<ProductPrice> prices, out ProductPrice price)
	{
		price = null;
		if (prices == null)
		{
			return false;
		}
		foreach (ProductPrice p in prices)
		{
			if (p != null && p.currencyTypeId == 2)
			{
				price = p;
				return true;
			}
		}
		return false;
	}

	public static bool TryGetVirtualPrice(this IEnumerable<ProductPrice> prices, out ProductPrice price)
	{
		price = null;
		if (prices == null)
		{
			return false;
		}
		foreach (ProductPrice p in prices)
		{
			if (p != null && p.currencyTypeId == 3)
			{
				price = p;
				return true;
			}
		}
		return false;
	}

	public static AttributeSet ConvertAttributes(List<ProductAttribute> attributes)
	{
		AttributeSet attributeSet = new AttributeSet();
		foreach (ProductAttribute productStoreAttribute in attributes)
		{
			attributeSet.AddAttributeIfValid(Attribute.CreateFrom(productStoreAttribute.Name, productStoreAttribute.Value));
		}
		return attributeSet;
	}

	public static AttributeSet ConvertAttributes(List<SectionAttribute> attributes)
	{
		AttributeSet attributeSet = new AttributeSet();
		foreach (SectionAttribute sectionAttribute in attributes)
		{
			attributeSet.AddAttributeIfValid(Attribute.CreateFrom(sectionAttribute.sectionAttributeKey, sectionAttribute.sectionAttributeValue));
		}
		return attributeSet;
	}
}
