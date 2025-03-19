using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using Blizzard.Commerce;
using com.blizzard.commerce.Model;
using Hearthstone.Commerce;
using Hearthstone.DataModels;
using PegasusUtil;
using Shared.Scripts.Game.Shop.Product;

namespace Hearthstone.Store;

public class ProductInfo
{
	public const int NO_ITEM_COUNT_REQUIREMENT = 0;

	public const int NO_PRODUCT_DATA_REQUIREMENT = 0;

	[CompilerGenerated]
	private DateTime? _003CStartTime_003Ek__BackingField;

	private RealPriceInfo? m_realMoneyPrice;

	private IDictionary<CurrencyType, PriceInfo> m_vcPrices = new Dictionary<CurrencyType, PriceInfo>();

	private bool m_needsSetBundle = true;

	private bool m_needsSetProduct = true;

	public List<Network.BundleItem> Items { get; set; }

	public string Title { get; private set; }

	private DateTime? StartTime
	{
		[CompilerGenerated]
		set
		{
			_003CStartTime_003Ek__BackingField = value;
		}
	}

	public DateTime? EndTime { get; private set; }

	public string Description { get; private set; }

	public ProductId Id { get; private set; } = ProductId.InvalidProduct;

	public string ProductClaimToken { get; set; }

	public string ProductEvent { get; private set; }

	public bool VisibleOnSalePeriodOnly { get; private set; }

	public IList<int> SaleIds { get; private set; }

	public bool IsPrePurchase { get; private set; }

	public AttributeSet Attributes { get; private set; }

	public MobileShopType DisableRealMoneyShopFlags { get; private set; }

	public ProductInfo(Product product)
	{
		SetProduct(product);
	}

	public ProductInfo(Network.Bundle bundle)
	{
		SetBundle(bundle);
	}

	public ProductInfo(ProductId productId, string productClaimToken)
	{
		if (!Id.IsValid())
		{
			Id = productId;
		}
		ProductClaimToken = productClaimToken;
	}

	public void SetProduct(Product product)
	{
		if (product == null)
		{
			return;
		}
		m_needsSetProduct = false;
		if (!Id.IsValid())
		{
			Id = ProductId.CreateFrom(product.productId);
		}
		if (product.localization != null && !string.IsNullOrEmpty(product.localization.name))
		{
			Title = product.localization.name;
		}
		if (product.localization != null && !string.IsNullOrEmpty(product.localization.description))
		{
			Description = product.localization.description;
		}
		StartTime = null;
		EndTime = null;
		if (product.productAvailabilities != null && product.productAvailabilities.Count > 0)
		{
			com.blizzard.commerce.Model.ProductAvailability productAvailability = product.productAvailabilities[0];
			if (productAvailability.startTimeMs != 0L)
			{
				StartTime = DateTimeOffset.FromUnixTimeMilliseconds((long)productAvailability.startTimeMs).UtcDateTime;
			}
			if (productAvailability.displayEndTimeMs != 0L)
			{
				EndTime = DateTimeOffset.FromUnixTimeMilliseconds((long)productAvailability.displayEndTimeMs).UtcDateTime;
			}
		}
		UpdatePrices(product);
	}

	public void SetBundle(Network.Bundle bundle)
	{
		if (!(bundle == null))
		{
			m_needsSetBundle = false;
			Items = bundle.Items;
			if (bundle.DisplayName != null && string.IsNullOrEmpty(Title))
			{
				Title = bundle.DisplayName.GetString();
			}
			if (bundle.DisplayDescription != null && string.IsNullOrEmpty(Description))
			{
				Description = bundle.DisplayDescription.GetString();
			}
			if (bundle.PMTProductID.HasValue)
			{
				Id = ProductId.CreateFrom(bundle.PMTProductID.Value);
			}
			else
			{
				Id = ProductId.InvalidProduct;
			}
			ProductEvent = bundle.ProductEvent;
			VisibleOnSalePeriodOnly = bundle.VisibleOnSalePeriodOnly;
			SaleIds = bundle.SaleIds;
			IsPrePurchase = bundle.IsPrePurchase;
			Attributes = bundle.Attributes;
			UpdatePrices(bundle);
			if ((!bundle.Cost.HasValue || bundle.Cost.Value.CurrentCost == 0L) && !HasNonGoldPrice() && m_vcPrices.Count > 0)
			{
				m_needsSetProduct = false;
			}
			DisableRealMoneyShopFlags = bundle.DisableRealMoneyShopFlags;
		}
	}

	public bool TryGetBundlePrice(CurrencyType currencyType, out double value)
	{
		value = 0.0;
		switch (currencyType)
		{
		case CurrencyType.REAL_MONEY:
			return TryGetRMPrice(out value);
		case CurrencyType.GOLD:
		case CurrencyType.CN_RUNESTONES:
		case CurrencyType.CN_ARCANE_ORBS:
		case CurrencyType.ROW_RUNESTONES:
		{
			long vcPrice;
			bool result = TryGetVCPrice(currencyType, out vcPrice);
			value = vcPrice;
			return result;
		}
		default:
			return false;
		}
	}

	public bool TryGetVCPriceInfo(CurrencyType type, out PriceInfo value)
	{
		value = default(PriceInfo);
		if (type == CurrencyType.NONE || m_vcPrices == null || m_vcPrices.Count == 0)
		{
			return false;
		}
		return m_vcPrices.TryGetValue(type, out value);
	}

	public bool TryGetRMPriceInfo(out RealPriceInfo value)
	{
		if (m_realMoneyPrice.HasValue)
		{
			value = m_realMoneyPrice.Value;
			return true;
		}
		value = default(RealPriceInfo);
		return false;
	}

	public bool TryGetVCPrice(CurrencyType type, out long price)
	{
		price = 0L;
		if (m_vcPrices == null || m_vcPrices.Count == 0)
		{
			return false;
		}
		if (!m_vcPrices.TryGetValue(type, out var priceInfo))
		{
			return false;
		}
		price = priceInfo.CurrentPrice.RealPrice;
		return true;
	}

	public bool TryGetRMPrice(out double price)
	{
		price = 0.0;
		if (!m_realMoneyPrice.HasValue)
		{
			return false;
		}
		price = m_realMoneyPrice.Value.CurrentPrice.RealPrice;
		return true;
	}

	public CurrencyType GetFirstVirtualCurrencyPriceType()
	{
		foreach (CurrencyType currencyType in m_vcPrices.Keys)
		{
			if (ShopUtils.IsCurrencyVirtual(currencyType))
			{
				return currencyType;
			}
		}
		return CurrencyType.NONE;
	}

	public PriceDataModel GetPriceDataModel(CurrencyType type)
	{
		PriceInfo priceInfo2;
		if (type == CurrencyType.REAL_MONEY)
		{
			if (TryGetRMPriceInfo(out var priceInfo))
			{
				return new PriceDataModel
				{
					Currency = type,
					Amount = (float)priceInfo.CurrentPrice.RealPrice,
					DisplayText = priceInfo.CurrentPrice.DisplayPrice,
					OriginalAmount = (float)priceInfo.OriginalPrice.RealPrice,
					OriginalDisplayText = priceInfo.OriginalPrice.DisplayPrice,
					OnSale = IsOnSale()
				};
			}
		}
		else if (TryGetVCPriceInfo(type, out priceInfo2))
		{
			return new PriceDataModel
			{
				Currency = type,
				Amount = priceInfo2.CurrentPrice.RealPrice,
				DisplayText = priceInfo2.CurrentPrice.DisplayPrice,
				OriginalAmount = priceInfo2.OriginalPrice.RealPrice,
				OriginalDisplayText = priceInfo2.OriginalPrice.DisplayPrice,
				OnSale = IsOnSale()
			};
		}
		return new PriceDataModel();
	}

	public bool IsProductFirstPurchaseBundle()
	{
		if (GetProductsInItemList(Items).Contains(ProductType.PRODUCT_TYPE_HIDDEN_LICENSE) && Items != null && Items.Find((Network.BundleItem obj) => obj.ItemType == ProductType.PRODUCT_TYPE_HIDDEN_LICENSE && obj.ProductData == 40) != null)
		{
			return true;
		}
		return false;
	}

	public HashSet<ProductType> GetProductsInBundle()
	{
		return GetProductsInItemList(Items);
	}

	public bool HasCurrency(CurrencyType type)
	{
		if (type == CurrencyType.NONE || m_vcPrices == null)
		{
			return false;
		}
		if (type == CurrencyType.REAL_MONEY)
		{
			return m_realMoneyPrice.HasValue;
		}
		if (m_vcPrices != null)
		{
			return m_vcPrices.ContainsKey(type);
		}
		return false;
	}

	public bool IsFree()
	{
		return Attributes.GetTags().Contains("free");
	}

	public bool IsOnSale()
	{
		return Attributes.GetTags().Contains("on_sale");
	}

	public bool IsGoldOnly()
	{
		if (!Id.IsValid())
		{
			return false;
		}
		if (IsFree())
		{
			return false;
		}
		return !HasNonGoldPrice();
	}

	public bool HasValidPrices()
	{
		if (!Id.IsValid())
		{
			return false;
		}
		if (IsFree())
		{
			return true;
		}
		if (m_vcPrices.Count <= 0)
		{
			return m_realMoneyPrice.HasValue;
		}
		return true;
	}

	public bool HasNonGoldPrice()
	{
		if (!m_realMoneyPrice.HasValue && m_vcPrices.Count <= 1)
		{
			if (m_vcPrices.Count == 1)
			{
				return !m_vcPrices.ContainsKey(CurrencyType.GOLD);
			}
			return false;
		}
		return true;
	}

	public bool IsValid()
	{
		if (Id.IsValid() && !m_needsSetProduct && !m_needsSetBundle)
		{
			if (!IsGoldOnly())
			{
				return !string.IsNullOrEmpty(Title);
			}
			return true;
		}
		return false;
	}

	public bool StringLogSetStatus()
	{
		if (m_needsSetBundle && m_needsSetProduct)
		{
			ProductIssues.LogError(Id, "Product data missing from both BPAY util server and CSDK.");
			return false;
		}
		if (m_needsSetProduct)
		{
			ProductIssues.LogError(Id, "Missing from CSDK. Check if this productId is requested through LoadProduct call.");
			return false;
		}
		if (m_needsSetBundle)
		{
			ProductIssues.LogError(Id, "Missing from BPAY Util Server. Check if BattlePayConfigResponse includes this product.");
			return false;
		}
		return true;
	}

	public ProductAvailabilityRange GetBundleAvailabilityRange(IProductAvailabilityApi api)
	{
		if (api.IgnoreProductTiming)
		{
			return new ProductAvailabilityRange();
		}
		ProductAvailabilityRange eventTimingRange = null;
		if (!string.IsNullOrEmpty(ProductEvent))
		{
			EventTimingManager specialEventManager = EventTimingManager.Get();
			EventTimingType eventTimingType = specialEventManager.GetEventType(ProductEvent);
			switch (eventTimingType)
			{
			case EventTimingType.UNKNOWN:
				return null;
			case EventTimingType.SPECIAL_EVENT_NEVER:
				return new ProductAvailabilityRange(ProductEvent, null, null)
				{
					IsNever = true
				};
			default:
			{
				if (specialEventManager.GetEventRangeUtc(eventTimingType, out var eventTimingStartUtc, out var eventTimingEndUtc))
				{
					eventTimingRange = new ProductAvailabilityRange(ProductEvent, eventTimingStartUtc, eventTimingEndUtc);
					if (eventTimingRange.IsNever)
					{
						return eventTimingRange;
					}
					break;
				}
				return null;
			}
			case EventTimingType.IGNORE:
				break;
			}
		}
		ProductAvailabilityRange range = null;
		if (!VisibleOnSalePeriodOnly)
		{
			range = new ProductAvailabilityRange();
		}
		else if (SaleIds != null)
		{
			DateTime nowUtc = DateTime.UtcNow;
			foreach (int saleId in SaleIds)
			{
				api.Sales.TryGetValue(saleId, out var sale);
				if (!(sale == null))
				{
					ProductAvailabilityRange saleRange = new ProductAvailabilityRange(sale);
					TimeSpan displacementToCurrentRange;
					TimeSpan displacementToSaleRange;
					if (range == null)
					{
						range = saleRange;
					}
					else if (ProductAvailabilityRange.AreOverlapping(range, saleRange))
					{
						range.UnionWith(saleRange);
					}
					else if (!range.TryGetTimeDisplacementRequiredToBeBuyable(nowUtc, out displacementToCurrentRange))
					{
						range = saleRange;
					}
					else if (saleRange.TryGetTimeDisplacementRequiredToBeBuyable(nowUtc, out displacementToSaleRange) && Math.Abs(displacementToSaleRange.Ticks) <= Math.Abs(displacementToCurrentRange.Ticks))
					{
						range = saleRange;
					}
				}
			}
		}
		if (eventTimingRange != null)
		{
			if (range == null)
			{
				range = eventTimingRange;
			}
			else
			{
				range.IntersectWith(eventTimingRange);
			}
		}
		return range;
	}

	public bool IsBundleAvailableNow(IProductAvailabilityApi api)
	{
		ProductAvailabilityRange range = GetBundleAvailabilityRange(api);
		if (range != null && range.IsBuyableAtTime(DateTime.UtcNow))
		{
			return true;
		}
		return false;
	}

	public bool DoesBundleContainProduct(ProductType product, int productData = 0, int numItemsRequired = 0)
	{
		if (Items == null || Items.Count == 0)
		{
			return false;
		}
		if (numItemsRequired != 0 && Items.Count != numItemsRequired)
		{
			return false;
		}
		foreach (Network.BundleItem item in Items)
		{
			if (item.ItemType == product && (productData == 0 || item.ProductData == productData))
			{
				return true;
			}
		}
		return false;
	}

	private void UpdatePrices(Product product)
	{
		if (IsFree())
		{
			return;
		}
		ProductPrice extRmPrice = null;
		ProductPrice rmPrice = null;
		List<string> expectedCurrencyCodes = null;
		if (product.externalPlatformSetting != null && product.externalPlatformSetting.prices != null)
		{
			if (expectedCurrencyCodes != null)
			{
				product.externalPlatformSetting.prices.TryGetRealMoneyPrice(expectedCurrencyCodes, out extRmPrice);
			}
			else
			{
				product.externalPlatformSetting.prices.TryGetRealMoneyPrice(out extRmPrice);
			}
		}
		if (product.prices != null)
		{
			if (expectedCurrencyCodes != null)
			{
				product.prices.TryGetRealMoneyPrice(expectedCurrencyCodes, out rmPrice);
			}
			else
			{
				product.prices.TryGetRealMoneyPrice(out rmPrice);
			}
		}
		if (rmPrice != null)
		{
			if (double.TryParse(rmPrice.currentPrice, NumberStyles.Currency, CultureInfo.InvariantCulture, out var currentPrice))
			{
				string localizedCurrentPrice = extRmPrice?.localizedCurrentPrice ?? rmPrice.localizedCurrentPrice;
				string localizedOriginalPrice = extRmPrice?.localizedOriginalPrice ?? rmPrice.localizedOriginalPrice;
				string currencyCode = extRmPrice?.currencyCode ?? rmPrice.currencyCode;
				if (!double.TryParse(rmPrice.originalPrice, NumberStyles.Currency, CultureInfo.InvariantCulture, out var originalPrice) || originalPrice <= 0.0)
				{
					originalPrice = currentPrice;
					localizedOriginalPrice = localizedCurrentPrice;
				}
				m_realMoneyPrice = new RealPriceInfo
				{
					CurrentPrice = new RealPriceInfo.Price
					{
						RealPrice = currentPrice,
						DisplayPrice = localizedCurrentPrice
					},
					OriginalPrice = new RealPriceInfo.Price
					{
						RealPrice = originalPrice,
						DisplayPrice = localizedOriginalPrice
					},
					CurrencyCode = currencyCode
				};
			}
			else
			{
				m_realMoneyPrice = new RealPriceInfo
				{
					CurrentPrice = new RealPriceInfo.Price
					{
						RealPrice = 0.0,
						DisplayPrice = (extRmPrice?.localizedCurrentPrice ?? rmPrice.localizedCurrentPrice)
					},
					OriginalPrice = new RealPriceInfo.Price
					{
						RealPrice = 0.0,
						DisplayPrice = (extRmPrice?.localizedOriginalPrice ?? rmPrice.localizedOriginalPrice)
					},
					CurrencyCode = (extRmPrice?.currencyCode ?? rmPrice.currencyCode)
				};
				Log.Store.PrintError($"[ProductInfo.UpdatePrices] Failed to parse product prices. Using 0s as parsed values as a fallback. ProductID {product?.productId}");
			}
		}
		if (product.prices != null && product.prices.TryGetGoldPrice(out var goldPrice))
		{
			if (long.TryParse(goldPrice.currentPrice, out var currentPrice2) && currentPrice2 > 0)
			{
				if (!long.TryParse(goldPrice.originalPrice, out var originalPrice2) || originalPrice2 <= 0)
				{
					originalPrice2 = currentPrice2;
				}
				m_vcPrices[CurrencyType.GOLD] = new PriceInfo
				{
					CurrentPrice = new PriceInfo.Price
					{
						RealPrice = currentPrice2,
						DisplayPrice = currentPrice2.ToString()
					},
					OriginalPrice = new PriceInfo.Price
					{
						RealPrice = originalPrice2,
						DisplayPrice = originalPrice2.ToString()
					}
				};
			}
			else
			{
				Log.Store.PrintWarning($"[ProductInfo.SetProduct] Unable to parse gold price of {goldPrice.currentPrice} for {product.productId}");
			}
		}
		if (product.prices == null || !product.prices.TryGetVirtualPrice(out var vcPrice))
		{
			return;
		}
		CurrencyType type = ShopUtils.GetCurrencyTypeFromCode(vcPrice.currencyCode);
		if (type == CurrencyType.NONE)
		{
			return;
		}
		if (long.TryParse(vcPrice.currentPrice, out var currentPrice3) && currentPrice3 > 0)
		{
			if (!long.TryParse(vcPrice.originalPrice, out var originalPrice3) || currentPrice3 <= 0)
			{
				originalPrice3 = currentPrice3;
			}
			m_vcPrices[type] = new PriceInfo
			{
				CurrentPrice = new PriceInfo.Price
				{
					RealPrice = currentPrice3,
					DisplayPrice = currentPrice3.ToString()
				},
				OriginalPrice = new PriceInfo.Price
				{
					RealPrice = originalPrice3,
					DisplayPrice = originalPrice3.ToString()
				}
			};
		}
		else
		{
			Log.Store.PrintWarning($"[ProductInfo.SetProduct] Unable to parse {type} price of {vcPrice.currentPrice} for {product.productId}");
		}
	}

	private void UpdatePrices(Network.Bundle bundle)
	{
		if (IsFree())
		{
			return;
		}
		if (bundle.GtappGoldCost.HasValue && bundle.GtappGoldCost.HasValue && bundle.GtappGoldCost.Value.CurrentCost > 0)
		{
			m_vcPrices[CurrencyType.GOLD] = new PriceInfo
			{
				CurrentPrice = new PriceInfo.Price
				{
					RealPrice = bundle.GtappGoldCost.Value.CurrentCost,
					DisplayPrice = bundle.GtappGoldCost.Value.CurrentCost.ToString()
				},
				OriginalPrice = new PriceInfo.Price
				{
					RealPrice = bundle.GtappGoldCost.Value.OriginalCost,
					DisplayPrice = bundle.GtappGoldCost.Value.OriginalCost.ToString()
				}
			};
		}
		if (bundle.VirtualCurrencyCost.HasValue && bundle.VirtualCurrencyCost.HasValue && bundle.VirtualCurrencyCost.Value.CurrentCost > 0 && !string.IsNullOrEmpty(bundle.VirtualCurrencyCode))
		{
			CurrencyType type = ShopUtils.GetCurrencyTypeFromCode(bundle.VirtualCurrencyCode);
			if (type != 0)
			{
				m_vcPrices[type] = new PriceInfo
				{
					CurrentPrice = new PriceInfo.Price
					{
						RealPrice = bundle.VirtualCurrencyCost.Value.CurrentCost,
						DisplayPrice = bundle.VirtualCurrencyCost.Value.CurrentCost.ToString()
					},
					OriginalPrice = new PriceInfo.Price
					{
						RealPrice = bundle.VirtualCurrencyCost.Value.OriginalCost,
						DisplayPrice = bundle.VirtualCurrencyCost.Value.OriginalCost.ToString()
					}
				};
			}
		}
	}

	private HashSet<ProductType> GetProductsInItemList(IEnumerable<Network.BundleItem> items)
	{
		HashSet<ProductType> includedProducts = new HashSet<ProductType>();
		foreach (Network.BundleItem item in items)
		{
			includedProducts.Add(item.ItemType);
		}
		return includedProducts;
	}
}
