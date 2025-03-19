using System.Collections.Generic;
using Assets;
using Blizzard.T5.Jobs;
using Blizzard.T5.Services;
using Hearthstone.Core;
using Hearthstone.DataModels;
using Hearthstone.Progression;
using Hearthstone.Store;

public static class ProductPageJobs
{
	public static void OpenToProductPageWhenReady(long pmtProductId, bool dontFullyOpenShop = false)
	{
		Processor.QueueJob("OpenToProductPage", Job_OpenToProductPage(pmtProductId, dontFullyOpenShop));
	}

	public static void OpenToSeasonPassPageWhenReady(Global.RewardTrackType trackType, long pmtId)
	{
		Log.Store.Print($"[ProuctPageJobs::OpenToSeasonPassPageWhenReady] TrackType - {trackType} PMTID - {pmtId}");
		string identifyingTag;
		switch (trackType)
		{
		case Global.RewardTrackType.GLOBAL:
			identifyingTag = "tavern_pass";
			break;
		case Global.RewardTrackType.BATTLEGROUNDS:
			identifyingTag = "battlegrounds_season_pass";
			break;
		default:
			Log.Store.PrintError("Attempted to open invalid season pass.");
			RewardTrackManager.OpenTavernPassErrorPopup();
			return;
		}
		long fallbackPmtId = 0L;
		IProductDataService dataService;
		if (pmtId != 0L)
		{
			OpenToProductPageWhenReady(pmtId, dontFullyOpenShop: true);
			Log.Store.Print($"[ProuctPageJobs::OpenToSeasonPassPageWhenReady] Opening Season Pass Page {pmtId}");
		}
		else if (ServiceManager.TryGet<IProductDataService>(out dataService) && dataService.TryGetPmtIdWithTagContainingType(identifyingTag, RewardItemType.PROGRESSION_BONUS, out fallbackPmtId))
		{
			OpenToProductPageWhenReady(fallbackPmtId, dontFullyOpenShop: true);
			Log.Store.Print($"[ProuctPageJobs::OpenToSeasonPassPageWhenReady] Opening Season Pass Page with fallback ID: {fallbackPmtId}");
		}
		else
		{
			Log.Store.Print($"[ProuctPageJobs::OpenToSeasonPassPageWhenReady] Unable to open Season Pass Page. Data Service: {dataService == null}, Fallback PMTID: {fallbackPmtId}");
			RewardTrackManager.OpenTavernPassErrorPopup(trackType);
		}
	}

	private static IEnumerator<IAsyncJobResult> Job_OpenToProductPage(long pmtProductId, bool dontFullyOpenShop)
	{
		StoreManager storeManager = StoreManager.Get();
		if (storeManager == null)
		{
			yield return new JobFailedResult("[Shop.OpenToProductPage] Cannot open product because StoreManager is unavailable");
		}
		if (!ServiceManager.TryGet<IProductDataService>(out var dataService))
		{
			yield return new JobFailedResult("[Shop.OpenToProductPage] Product service not initialized.");
		}
		while (!storeManager.IsOpen())
		{
			yield return null;
		}
		Shop.DontFullyOpenShop = dontFullyOpenShop;
		storeManager.StartGeneralTransaction();
		if (pmtProductId == 0L)
		{
			yield return new JobFailedResult("[Shop.OpenToProductPage] Must provide a PMT product Id");
		}
		while (!dataService.HasPendingTierChanges())
		{
			yield return null;
		}
		while (Shop.Get() == null)
		{
			yield return null;
		}
		Shop shop = Shop.Get();
		SoundManager.Get().LoadAndPlay("Store_window_expand.prefab:050bf879a3e32d04999427c262baaf09", shop.gameObject);
		ProductDataModel product = dataService.GetProductDataModel(pmtProductId);
		if (product == null)
		{
			yield return new JobFailedResult("[Shop.OpenToProductPage] Unable to find product {0} in catalog", pmtProductId);
		}
		while (!shop.IsOpen())
		{
			yield return null;
		}
		if (dataService.TryGetSubTabFromProductId(product.PmtId, out var parentTab, out var subTab) && (!parentTab.Enabled || parentTab.Locked || !subTab.Enabled || subTab.Locked))
		{
			yield return new JobFailedResult($"[Shop.OpenToProductPage] Attempted to open product {product.PmtId} but Tab({parentTab.Id})/SubTab({subTab.Id}) is unavailable!");
		}
		ProductDataModel baseProduct = dataService.GetBaseProductFromPmtProductId(pmtProductId);
		if (baseProduct != null)
		{
			CurrencyType vcType;
			if (dataService.VirtualCurrencyProductItem == baseProduct)
			{
				if (!ShopUtils.IsVirtualCurrencyEnabled())
				{
					yield return new JobFailedResult("[Shop.OpenToProductPage] Cannot handle VC product when VC mode is disabled");
				}
				if (!ShopUtils.TryGetMainVirtualCurrencyType(out vcType))
				{
					yield return new JobFailedResult("[Shop.OpenToProductPage] Cannot handle VC product with no valid Currency Type");
				}
				shop.ProductPageController.OpenVirtualCurrencyPurchase(ShopUtils.GetAmountOfCurrencyInProduct(product, vcType));
			}
			else if (dataService.BoosterCurrencyProductItem == baseProduct)
			{
				if (!ShopUtils.IsVirtualCurrencyEnabled())
				{
					yield return new JobFailedResult("[Shop.OpenToProductPage] Cannot handle BC product when VC mode is disabled");
				}
				if (!ShopUtils.TryGetBoosterVirtualCurrencyType(out vcType))
				{
					yield return new JobFailedResult("[Shop.OpenToProductPage] Cannot handle BC product with no valid Currency Type");
				}
				shop.ProductPageController.OpenBoosterCurrencyPurchase(ShopUtils.GetAmountOfCurrencyInProduct(product, vcType));
			}
			else
			{
				shop.ProductPageController.OpenProductPage(baseProduct, product);
			}
		}
		else
		{
			shop.ProductPageController.OpenProductPage(product);
		}
		if (!dontFullyOpenShop)
		{
			shop.MoveToProduct(product);
		}
	}
}
