using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Blizzard.T5.Jobs;
using Blizzard.T5.Services;
using Hearthstone.Core;
using Hearthstone.DataModels;
using UnityEngine;

public class PurchaseManager : IService
{
	public struct PurchaseManagerOptions
	{
		public bool SuppressVCConfirmtation;

		public static readonly PurchaseManagerOptions DefaultOptions = new PurchaseManagerOptions
		{
			SuppressVCConfirmtation = false
		};

		public bool AutoconvertCurrency => Options.Get().GetBool(Option.AUTOCONVERT_VIRTUAL_CURRENCY);
	}

	private class PurchaseOrder
	{
		public ProductDataModel Product;

		public PriceDataModel Price;

		public int Quantity = 1;

		public bool HasFinished;

		public bool IsPurchasable()
		{
			if (Product != null && Price != null)
			{
				return Quantity > 0;
			}
			return false;
		}

		public long GetDeficit()
		{
			if (!IsPurchasable())
			{
				return 0L;
			}
			return ShopUtils.GetDeficit(new PriceDataModel
			{
				Currency = Price.Currency,
				Amount = Price.Amount * (float)Quantity
			});
		}

		public string GetPurchaseName()
		{
			if (Product == null)
			{
				return string.Empty;
			}
			string purchaseName = GameStrings.Get(Product.Name ?? string.Empty);
			if (Product.Items.Count == 1 && !Product.Tags.Contains("has_description"))
			{
				RewardItemType itemType = Product.Items[0].ItemType;
				if (itemType == RewardItemType.BOOSTER || itemType == RewardItemType.MERCENARY_BOOSTER)
				{
					BoosterDbId boosterId = Product.GetProductBoosterId();
					if (boosterId != 0)
					{
						BoosterDbfRecord boosterRecord = GameDbf.Booster.GetRecord((int)boosterId);
						if (boosterRecord != null)
						{
							int totalQuantity = Quantity * Product.Items[0].Quantity;
							if (totalQuantity > 0)
							{
								string boosterName = boosterRecord.Name;
								purchaseName = GameStrings.Format("GLUE_STORE_PRODUCT_NAME_PACK", totalQuantity, boosterName);
							}
						}
					}
				}
			}
			return purchaseName;
		}

		public bool DoesPurchaseContainDuplicateMercs()
		{
			List<RewardItemDataModel> knockoutMercs = Product.Items.Where((RewardItemDataModel item) => item.ItemType == RewardItemType.MERCENARY_KNOCKOUT_SPECIFIC).ToList();
			if (knockoutMercs.Count > 0)
			{
				for (int i = 0; i < knockoutMercs.Count; i++)
				{
					int mercId = knockoutMercs[i].Mercenary.MercenaryId;
					int num = GameDbf.LettuceMercenary.GetRecord(mercId).MercenaryArtVariations.SelectMany((MercenaryArtVariationDbfRecord art) => art.MercenaryArtVariationPremiums.Where((MercenaryArtVariationPremiumDbfRecord premium) => premium.Collectible)).Count();
					int numOwnedMercVariations = CollectionManager.Get().GetMercenary(mercId)?.m_artVariations.Count ?? 0;
					if (num == numOwnedMercVariations)
					{
						return true;
					}
				}
			}
			return false;
		}
	}

	[CompilerGenerated]
	private bool _003CIsPurchasing_003Ek__BackingField;

	private Stack<PurchaseOrder> m_autoPurchaseStack = new Stack<PurchaseOrder>();

	private PurchaseManagerOptions m_currentPurchaseOptions;

	private Coroutine m_currentPurchaseCoroutine;

	private Action m_onPurchaseStarted;

	private Action m_onPurchaseEnded;

	private bool IsPurchasing
	{
		[CompilerGenerated]
		set
		{
			_003CIsPurchasing_003Ek__BackingField = value;
		}
	}

	Type[] IService.GetDependencies()
	{
		return new Type[1] { typeof(CurrencyManager) };
	}

	IEnumerator<IAsyncJobResult> IService.Initialize(ServiceLocator serviceLocator)
	{
		if (ServiceManager.TryGet<CurrencyManager>(out var currencyManager))
		{
			currencyManager.AddCurrencyBalanceChangedCallback(OnCurrencyBalanceChanged);
		}
		yield break;
	}

	void IService.Shutdown()
	{
		if (ServiceManager.TryGet<CurrencyManager>(out var currencyManager))
		{
			currencyManager.RemoveCurrencyBalanceChangedCallback(OnCurrencyBalanceChanged);
		}
		CancelCurrentPurchase("Purchase Manager has been shutdown");
	}

	public void PurchaseProduct(ProductDataModel product, PriceDataModel price, PurchaseManagerOptions purchaseOptions)
	{
		PurchaseProduct(product, price, 1, purchaseOptions);
	}

	public void PurchaseProduct(ProductDataModel product, PriceDataModel price, int quantity)
	{
		PurchaseProduct(product, price, quantity, PurchaseManagerOptions.DefaultOptions);
	}

	public void PurchaseProduct(ProductDataModel product, PriceDataModel price, int quantity, PurchaseManagerOptions purchaseOptions)
	{
		if (m_currentPurchaseCoroutine != null)
		{
			Log.Store.PrintError("Stopping purchase execution - A purchase order has already been executed");
			return;
		}
		if (quantity <= 0)
		{
			Log.Store.PrintError($"Cannot start purchase for an item with quantity - {quantity}");
			return;
		}
		if (ShopUtils.IsCurrencyVirtual(price.Currency) && !ShopUtils.IsVirtualCurrencyTypeEnabled(price.Currency))
		{
			Log.Store.PrintError($"Cannot start purchase for item with VC price when it is not enabled - {price.Currency}");
			return;
		}
		CancelPendingAutoPurchases();
		PurchaseOrder purchase = new PurchaseOrder
		{
			Product = product,
			Price = price,
			Quantity = quantity
		};
		BuyProductEventArgs args = purchase.Product.GetBuyProductArgs(purchase.Price, purchase.Quantity);
		StoreManager.Get().SendShopPurchaseEventTelemetry(isComplete: false, args);
		while (purchase != null)
		{
			m_autoPurchaseStack.Push(purchase);
			if (!TryGetPrerequisitePurchase(purchase, purchaseOptions, out var prerequisiteOrder))
			{
				Log.Store.PrintError("Purchase could not be started");
				return;
			}
			purchase = prerequisiteOrder;
		}
		purchase = m_autoPurchaseStack.Pop();
		m_currentPurchaseOptions = purchaseOptions;
		TriggerOnPurchaseStarted();
		Internal_PurchaseProduct(purchase, purchaseOptions);
	}

	public void CancelPendingAutoPurchases()
	{
		m_autoPurchaseStack.Clear();
	}

	public bool HasAutoPurchase()
	{
		return m_autoPurchaseStack.Count > 0;
	}

	private void Internal_PurchaseProduct(PurchaseOrder purchase, PurchaseManagerOptions purchaseOptions)
	{
		if (m_currentPurchaseCoroutine == null)
		{
			IEnumerator enumerator = Internal_PurchaseProductCoroutine(purchase, purchaseOptions);
			if (!purchase.HasFinished)
			{
				m_currentPurchaseCoroutine = Processor.RunCoroutine(enumerator);
			}
		}
	}

	private IEnumerator Internal_PurchaseProductCoroutine(PurchaseOrder purchase, PurchaseManagerOptions purchaseOptions)
	{
		if (purchase == null)
		{
			CancelCurrentPurchase("PurchaseOrder null.");
			yield break;
		}
		if (!purchase.IsPurchasable())
		{
			purchase.HasFinished = true;
			CancelCurrentPurchase("PurchaseOrder invalid.");
			yield break;
		}
		BuyProductEventArgs args = purchase.Product.GetBuyProductArgs(purchase.Price, purchase.Quantity);
		if (args == null)
		{
			purchase.HasFinished = true;
			CancelCurrentPurchase("No valid BuyProductEventArgs for product");
			yield break;
		}
		if ((purchase.Product.Tags.Contains("row_runestones") || purchase.Product.Tags.Contains("cn_runestones")) && m_autoPurchaseStack.Count > 0)
		{
			PurchaseOrder nextBuyWithVC = m_autoPurchaseStack.LastOrDefault((PurchaseOrder p) => p.Price != null && ShopUtils.IsMainVirtualCurrencyType(p.Price.Currency));
			if (nextBuyWithVC == null)
			{
				Log.Store.PrintError("Unnecessary VC purchase planned; skipping");
				yield return Internal_PurchaseProductCoroutine(m_autoPurchaseStack.Pop(), purchaseOptions);
				yield break;
			}
			StoreManager.Get().BlockStoreInterface();
			AlertPopup.Response? nullableResponse = null;
			DialogManager.Get().ShowPopup(new AlertPopup.PopupInfo
			{
				m_showAlertIcon = false,
				m_responseDisplay = AlertPopup.ResponseDisplay.CONFIRM_CANCEL,
				m_confirmText = GameStrings.Get("GLOBAL_BUTTON_YES"),
				m_cancelText = GameStrings.Get("GLOBAL_BUTTON_NOT_NOW"),
				m_alertTextAlignment = UberText.AlignmentOptions.Center,
				m_alertTextAlignmentAnchor = UberText.AnchorOptions.Middle,
				m_headerText = GameStrings.Get("GLUE_SHOP_GET_MORE_RUNESTONES_HEADER"),
				m_text = GameStrings.Get("GLUE_SHOP_GET_MORE_RUNESTONES"),
				m_responseCallback = delegate(AlertPopup.Response response, object userData)
				{
					nullableResponse = response;
				}
			});
			StoreManager.Get().SendShopPurchaseEventTelemetry(isComplete: false, args);
			yield return new WaitUntil(() => nullableResponse.HasValue);
			StoreManager.Get().UnblockStoreInterface();
			if (nullableResponse.Value == AlertPopup.Response.CONFIRM)
			{
				StoreManager.Get().SendShopPurchaseEventTelemetry(isComplete: false, args);
				Shop shop = Shop.Get();
				if (shop != null)
				{
					shop.ProductPageController.OpenVirtualCurrencyPurchase(nextBuyWithVC.GetDeficit(), rememberLastPage: true);
					if (BnetBar.Get() != null)
					{
						BnetBar.Get().RefreshCurrency();
					}
				}
			}
			purchase.HasFinished = true;
			CancelCurrentPurchase((nullableResponse.Value != AlertPopup.Response.CONFIRM && nullableResponse.Value != AlertPopup.Response.CANCEL) ? $"Unknown response value - {nullableResponse.Value} at Get More VC confirmation popup" : null);
			yield break;
		}
		if ((purchase.Price.Currency == CurrencyType.ROW_RUNESTONES || purchase.Price.Currency == CurrencyType.GOLD) && !purchaseOptions.SuppressVCConfirmtation)
		{
			StoreManager.Get().BlockStoreInterface();
			AlertPopup.Response? nullableResponse2 = null;
			ShopPopupSpawner.CreatePurchaseConfirmPopup(purchase.GetPurchaseName(), purchase.Price.Currency, delegate(AlertPopup.Response response, object userData)
			{
				nullableResponse2 = response;
			});
			StoreManager.Get().SendShopPurchaseEventTelemetry(isComplete: false, args);
			yield return new WaitUntil(() => nullableResponse2.HasValue);
			StoreManager.Get().UnblockStoreInterface();
			if (nullableResponse2.Value != AlertPopup.Response.CONFIRM)
			{
				purchase.HasFinished = true;
				CancelCurrentPurchase((nullableResponse2.Value != AlertPopup.Response.CANCEL) ? $"Unknown response value - {nullableResponse2.Value} at VC/Gold confirmation popup" : null);
				yield break;
			}
		}
		if (purchase.Product.Items.Any((RewardItemDataModel item) => item.Mercenary != null) && purchase.DoesPurchaseContainDuplicateMercs())
		{
			StoreManager.Get().BlockStoreInterface();
			AlertPopup.Response? nullableResponse3 = null;
			DialogManager.Get().ShowPopup(new AlertPopup.PopupInfo
			{
				m_showAlertIcon = false,
				m_responseDisplay = AlertPopup.ResponseDisplay.CONFIRM_CANCEL,
				m_confirmText = GameStrings.Get("GLOBAL_CONFIRM"),
				m_cancelText = GameStrings.Get("GLOBAL_CANCEL"),
				m_alertTextAlignment = UberText.AlignmentOptions.Center,
				m_alertTextAlignmentAnchor = UberText.AnchorOptions.Middle,
				m_headerText = GameStrings.Get("GLUE_LETTUCE_OWNED_MERCENARY_WARNING_TITLE"),
				m_text = GameStrings.Get("GLUE_LETTUCE_OWNED_MERCENARY_WARNING_DESC"),
				m_responseCallback = delegate(AlertPopup.Response response, object userData)
				{
					nullableResponse3 = response;
				}
			});
			yield return new WaitUntil(() => nullableResponse3.HasValue);
			StoreManager.Get().UnblockStoreInterface();
			if (nullableResponse3.Value != AlertPopup.Response.CONFIRM)
			{
				purchase.HasFinished = true;
				CancelCurrentPurchase((nullableResponse3.Value != AlertPopup.Response.CANCEL) ? $"Unknown response value - {nullableResponse3.Value} at duplicate merc popup" : null);
				yield break;
			}
			StoreManager.Get().SendShopPurchaseEventTelemetry(isComplete: false, args);
		}
		args.BeginPurchaseTelemetryFired = true;
		purchase.HasFinished = true;
		m_currentPurchaseCoroutine = null;
		StoreManager.Get().StartStoreBuy(args);
		TriggerOnPurchaseEnded();
	}

	private void TryNextAutoPurchase()
	{
		if (m_currentPurchaseCoroutine == null && m_autoPurchaseStack.Count != 0)
		{
			PurchaseOrder nextPurchase = m_autoPurchaseStack.Peek();
			if (nextPurchase != null && nextPurchase.GetDeficit() == 0L)
			{
				m_autoPurchaseStack.Pop();
				Internal_PurchaseProduct(nextPurchase, m_currentPurchaseOptions);
			}
		}
	}

	private void CancelCurrentPurchase(string error = "")
	{
		if (m_currentPurchaseCoroutine != null)
		{
			Processor.CancelCoroutine(m_currentPurchaseCoroutine);
			m_currentPurchaseCoroutine = null;
			if (error != null)
			{
				Log.Store.PrintError("Executed purchase was stopped unexpectedly - Reason: " + error);
			}
		}
		CancelPendingAutoPurchases();
		TriggerOnPurchaseEnded();
	}

	private bool TryGetPrerequisitePurchase(PurchaseOrder pendingPurchase, PurchaseManagerOptions purchaseOptions, out PurchaseOrder prerequisiteOrder)
	{
		prerequisiteOrder = null;
		CurrencyType pendingPurchaseCurrencyType = pendingPurchase.Price.Currency;
		if (pendingPurchaseCurrencyType == CurrencyType.REAL_MONEY)
		{
			return true;
		}
		long deficit = pendingPurchase.GetDeficit();
		if (deficit <= 0)
		{
			return true;
		}
		ProductDataModel currencyProduct = ShopUtils.FindCurrencyProduct(pendingPurchaseCurrencyType, deficit);
		if (currencyProduct == null)
		{
			Log.Store.PrintError("Unable to find product with {0} of currency {1}", deficit, pendingPurchaseCurrencyType.ToString());
			return false;
		}
		if (currencyProduct.Items.Count == 0)
		{
			Log.Store.PrintError("Invalid currency product '" + currencyProduct.Name + "': No items found.");
			return false;
		}
		if (currencyProduct.Prices.Count == 0)
		{
			Log.Store.PrintError("Invalid currency product '" + currencyProduct.Name + "': No prices found.");
			return false;
		}
		if (currencyProduct.Items[0].ItemType == RewardItemType.CN_ARCANE_ORBS && !purchaseOptions.AutoconvertCurrency)
		{
			Log.Store.PrintError("Unable to convert Booster Currency; autoconversion required");
			return false;
		}
		float amount = ShopUtils.GetAmountOfCurrencyInProduct(currencyProduct, pendingPurchaseCurrencyType);
		if (amount <= 0f)
		{
			Log.Store.PrintError("Invalid currency product; contains no currency");
			return false;
		}
		prerequisiteOrder = new PurchaseOrder
		{
			Product = currencyProduct,
			Price = currencyProduct.Prices[0],
			Quantity = Mathf.CeilToInt((float)deficit / amount)
		};
		return true;
	}

	private void TriggerOnPurchaseStarted()
	{
		IsPurchasing = true;
		m_onPurchaseStarted?.Invoke();
	}

	private void TriggerOnPurchaseEnded()
	{
		IsPurchasing = false;
		m_onPurchaseEnded?.Invoke();
	}

	private void OnCurrencyBalanceChanged(CurrencyBalanceChangedEventArgs args)
	{
		if (args.Currency == CurrencyType.CN_ARCANE_ORBS)
		{
			TryNextAutoPurchase();
		}
	}
}
