using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Blizzard.T5.Core.Utils;
using Blizzard.T5.Services;
using Hearthstone.DataModels;
using Hearthstone.UI;
using UnityEngine;

public class ProductPage : MonoBehaviour
{
	public const string SHOP_BUY_WITH_FIRST_CURRENCY = "SHOP_BUY_WITH_FIRST_CURRENCY";

	public const string SHOP_BUY_WITH_ALT_CURRENCY = "SHOP_BUY_WITH_ALT_CURRENCY";

	protected Widget m_widget;

	protected ProductPageContainer m_container;

	protected Shop m_parentShop;

	protected ProductDataModel m_productImmutable;

	protected ProductDataModel m_productMutable;

	protected Dictionary<int, int> m_variantQuantities = new Dictionary<int, int>();

	protected ProductSelectionDataModel m_productSelection = new ProductSelectionDataModel();

	protected AlertPopup.PopupInfo m_preBuyPopupInfo;

	private Coroutine m_openWhenReadyCoroutine;

	public Widget WidgetComponent => m_widget;

	public ProductDataModel Product => m_productMutable ?? m_productImmutable;

	public ProductSelectionDataModel Selection => m_productSelection;

	public bool IsOpen { get; private set; }

	public bool IsAnimating { get; set; }

	public event EventHandler OnOpened;

	public event EventHandler OnClosed;

	public event Action<ProductSelectionDataModel> OnProductVariantSet;

	protected virtual void Awake()
	{
		m_parentShop = GameObjectUtils.FindComponentInParents<Shop>(base.gameObject);
		m_container = GameObjectUtils.FindComponentInParents<ProductPageContainer>(base.gameObject);
		if (m_container != null)
		{
			m_container.RegisterProductPage(this);
		}
	}

	protected virtual void Start()
	{
		m_widget = GetComponent<Widget>();
		m_widget.RegisterEventListener(OnWidgetEvent);
	}

	protected virtual void OnEnable()
	{
		if (IsOpen && m_openWhenReadyCoroutine != null)
		{
			m_openWhenReadyCoroutine = StartCoroutine(OpenWhenReadyRoutine());
		}
	}

	protected virtual void OnDestroy()
	{
		if (m_container != null)
		{
			m_container.UnregisterProductPage(this);
		}
	}

	public virtual void SelectVariant(ProductDataModel product)
	{
		product = product ?? ProductFactory.CreateEmptyProductDataModel();
		Log.Store.PrintDebug("Selecting Product PMT ID = {0}, Name = {1}", product.PmtId, product.Name);
		if (m_productSelection.Variant != product)
		{
			m_productSelection.Variant = product;
			m_productSelection.VariantIndex = m_productImmutable.Variants.IndexOf(GetImmutableVariant(product));
			m_productSelection.Quantity = GetVariantQuantityByIndex(m_productSelection.VariantIndex);
		}
		if (m_container != null)
		{
			m_container.Variant = product;
		}
		m_productSelection.MaxQuantity = product.GetMaxBulkPurchaseCount();
		if (m_widget.GetDataModel<ProductSelectionDataModel>() != m_productSelection)
		{
			m_widget.BindDataModel(m_productSelection);
		}
		this.OnProductVariantSet?.Invoke(m_productSelection);
	}

	public ProductDataModel GetSelectedVariant()
	{
		return m_productSelection.Variant;
	}

	public int GetSelectedVariantIndex()
	{
		return m_productSelection.VariantIndex;
	}

	public ProductDataModel GetVariantByIndex(int index)
	{
		return Product?.Variants.ElementAtOrDefault(index);
	}

	public void SelectVariantByIndex(int index)
	{
		ProductDataModel variant = GetVariantByIndex(index);
		if (variant != null)
		{
			SelectVariant(variant);
			return;
		}
		Log.Store.PrintWarning("SelectVariantByIndex failed. Product missing variant index {0}", index);
	}

	public int GetVariantQuantityByIndex(int index)
	{
		if (m_variantQuantities.TryGetValue(index, out var quantity))
		{
			return quantity;
		}
		return 1;
	}

	public bool ShowQuantityPromptForVariant(int variantIndex)
	{
		ProductDataModel variant = GetVariantByIndex(variantIndex);
		if (variant == null)
		{
			Log.Store.PrintError("ShowQuantityPromptForVariant failed. No variant at index {0}.", variantIndex);
			return false;
		}
		if (!variant.ProductSupportsQuantitySelect())
		{
			Log.Store.Print("ShowQuantityPromptForVariant failed. Product {0} [{1}] does not support quantity select.", variant.PmtId, variant.Name);
			return false;
		}
		if (m_parentShop == null || m_parentShop.ProductPageController == null)
		{
			Log.Store.PrintError("ShowQuantityPromptForVariant failed. Shop is null.");
			return false;
		}
		m_parentShop.ProductPageController.OpenQuantityPrompt(m_productSelection.MaxQuantity, delegate(int quantity)
		{
			SetVariantQuantityAndUpdateDataModel(variant, quantity);
		});
		return true;
	}

	protected virtual void OnProductSet()
	{
	}

	protected virtual ProductDataModel GetFirstVariantToDisplay(ProductDataModel chosenProduct, ProductDataModel chosenVariant)
	{
		return chosenVariant;
	}

	protected void SetProduct(ProductDataModel product, ProductDataModel variant)
	{
		m_productImmutable = product ?? ProductFactory.CreateEmptyProductDataModel();
		m_productMutable = null;
		TryCreateMutableProduct();
		m_variantQuantities.Clear();
		BindProductDataModel();
		OnProductSet();
		SelectVariant(variant ?? m_productImmutable);
	}

	protected bool OnNavigateBack()
	{
		if (IsAnimating)
		{
			return false;
		}
		Close();
		return true;
	}

	protected void OnWidgetEvent(string eventName)
	{
		switch (eventName)
		{
		case "SHOP_BUY_WITH_FIRST_CURRENCY":
			TryBuy(0);
			break;
		case "SHOP_BUY_WITH_ALT_CURRENCY":
			TryBuy(1);
			break;
		case "SHOP_SKU_CLICKED_0":
			SelectVariantByIndex(0);
			break;
		case "SHOP_SKU_CLICKED_1":
			SelectVariantByIndex(1);
			break;
		case "SHOP_SKU_CLICKED_2":
			SelectVariantByIndex(2);
			break;
		case "SHOP_SKU_CLICKED_3":
			SelectVariantByIndex(3);
			break;
		case "SHOP_SKU_CLICKED_4":
			SelectVariantByIndex(4);
			break;
		case "SHOP_SKU_CLICKED_5":
			SelectVariantByIndex(5);
			break;
		case "SHOP_SKU_CLICKED_6":
			SelectVariantByIndex(6);
			break;
		case "SHOP_SKU_DOUBLE_CLICKED_0":
			ShowQuantityPromptForVariant(0);
			break;
		}
	}

	public virtual void Open()
	{
		if (!IsOpen)
		{
			IsOpen = true;
			if (m_container != null)
			{
				ProductDataModel variantToDisplay = GetFirstVariantToDisplay(m_container.Product, m_container.Variant);
				SetProduct(m_container.Product, variantToDisplay);
			}
			Navigation.Push(OnNavigateBack);
			m_openWhenReadyCoroutine = StartCoroutine(OpenWhenReadyRoutine());
		}
	}

	public virtual void Close()
	{
		if (IsOpen)
		{
			IsOpen = false;
			Navigation.RemoveHandler(OnNavigateBack);
			if ((bool)m_widget)
			{
				m_widget.TriggerEvent("CLOSED");
			}
			if (m_openWhenReadyCoroutine != null)
			{
				StopCoroutine(m_openWhenReadyCoroutine);
				m_openWhenReadyCoroutine = null;
			}
			if (this.OnClosed != null)
			{
				this.OnClosed(this, new EventArgs());
			}
		}
	}

	protected ProductDataModel GetImmutableVariant(ProductDataModel variant)
	{
		if (Product == variant)
		{
			return m_productImmutable;
		}
		foreach (ProductDataModel immutableVariant in m_productImmutable.Variants)
		{
			if (variant.PmtId == immutableVariant.PmtId)
			{
				return immutableVariant;
			}
		}
		if (m_productMutable != null)
		{
			int index = m_productMutable.Variants.IndexOf(variant);
			if (index >= 0)
			{
				return m_productImmutable.Variants.ElementAtOrDefault(index);
			}
		}
		return null;
	}

	protected virtual void TryBuy(int priceOption)
	{
		if (Product == null)
		{
			Log.Store.PrintError("TryBuy failed where no Product is bound to ProductPage");
			return;
		}
		ProductDataModel selectedVariant = GetSelectedVariant();
		if (selectedVariant == null)
		{
			Log.Store.PrintError("Attempted to purchase, but no selected variant found.");
			return;
		}
		if (!ValidateMutableProduct())
		{
			Log.Store.PrintError("Attempted to purchase, but mutable product mismatches immutable product on ProductPage. PMT ID = {0}, Name = {1}", selectedVariant.PmtId, selectedVariant.Name);
			return;
		}
		ProductDataModel immuatableSelectedProduct = GetImmutableVariant(selectedVariant);
		if (immuatableSelectedProduct == null)
		{
			Log.Store.PrintError("Attempted to purchase but failed to get immutable version of product. PMT ID = {0}, Name = {1}", selectedVariant.PmtId, selectedVariant.Name);
			return;
		}
		int quantity = 1;
		if (immuatableSelectedProduct != selectedVariant)
		{
			int selectedIndex = m_productImmutable.Variants.IndexOf(immuatableSelectedProduct);
			if (selectedIndex < 0)
			{
				Log.Store.PrintError("Attempted to purchase but failed to get index of product. PMT ID = {0}, Name = {1}", selectedVariant.PmtId, selectedVariant.Name);
				return;
			}
			quantity = GetVariantQuantityByIndex(selectedIndex);
			if (quantity < 1)
			{
				Log.Store.PrintError("Attempted to purchase, but selected product quantity is invalid. PMT ID = {0}, Name = {1}, Quantity = {2}", selectedVariant.PmtId, selectedVariant.Name, quantity);
				return;
			}
		}
		if (priceOption < 0 || priceOption >= immuatableSelectedProduct.Prices.Count)
		{
			Log.Store.PrintError("Attempted to purchase, but price index {0} is out of bounds. Num Prices = {1}, PMT ID = {2}, Name = {3}", priceOption, immuatableSelectedProduct.Prices.Count, immuatableSelectedProduct.PmtId, immuatableSelectedProduct.Name);
			return;
		}
		PriceDataModel price = immuatableSelectedProduct.Prices[priceOption];
		if (price == null)
		{
			Log.Store.PrintError("Attempted to purchase, but PriceDataModel is null at index {0}. PMT ID = {1}, Name = {2}", priceOption, immuatableSelectedProduct.PmtId, immuatableSelectedProduct.Name);
			return;
		}
		if (!ServiceManager.TryGet<PurchaseManager>(out var purchaseManager))
		{
			Log.Store.PrintError("Attempted to purchase, but purchase manager does not exist");
			return;
		}
		if (m_preBuyPopupInfo == null)
		{
			purchaseManager.PurchaseProduct(immuatableSelectedProduct, price, quantity);
			return;
		}
		m_preBuyPopupInfo.m_responseCallback = delegate(AlertPopup.Response response, object userData)
		{
			if (response == AlertPopup.Response.CONFIRM || response == AlertPopup.Response.OK)
			{
				purchaseManager.PurchaseProduct(immuatableSelectedProduct, price, quantity);
			}
		};
		DialogManager.Get().ShowPopup(m_preBuyPopupInfo);
	}

	private bool TryCreateMutableProduct()
	{
		bool needsMutableProduct = false;
		if (!CanVariantBeMutable(m_productImmutable))
		{
			foreach (ProductDataModel product in m_productImmutable.Variants)
			{
				if (CanVariantBeMutable(product))
				{
					needsMutableProduct = true;
					break;
				}
			}
		}
		else
		{
			needsMutableProduct = true;
		}
		if (needsMutableProduct)
		{
			m_productMutable = m_productImmutable.CloneDataModel();
			m_productMutable.Variants = new DataModelList<ProductDataModel>();
			m_productMutable.Variants.AddRange(m_productImmutable.Variants.Select((ProductDataModel v) => CreateMutableVariant(v)));
			m_variantQuantities.Clear();
			return true;
		}
		return false;
	}

	private ProductDataModel CreateMutableVariant(ProductDataModel immutableVariant)
	{
		ProductDataModel mutableVariant = immutableVariant.CloneDataModel();
		mutableVariant.Items = new DataModelList<RewardItemDataModel>();
		mutableVariant.Items.AddRange(immutableVariant.Items.Select((RewardItemDataModel i) => i.CloneDataModel()));
		mutableVariant.Prices = new DataModelList<PriceDataModel>();
		mutableVariant.Prices.AddRange(immutableVariant.Prices.Select((PriceDataModel p) => p.CloneDataModel()));
		RefreshVariant(mutableVariant);
		return mutableVariant;
	}

	private bool ValidateMutableProduct()
	{
		if (m_productMutable != null)
		{
			if (m_productImmutable == null)
			{
				Log.Store.PrintError("ProductPage has a m_productMutable but no m_productImmutable. Mutable Product PMT ID = {0}, Name = {1}", m_productMutable.PmtId, m_productMutable.Name);
				return false;
			}
			if (m_productMutable.PmtId != m_productImmutable.PmtId)
			{
				Log.Store.PrintError("ProductPage Mutable and Immutable products have mismatching PMT id's. Mutable Product PMT ID = {0}, Name = {1}", m_productMutable.PmtId, m_productMutable.Name);
				return false;
			}
			if (m_productMutable.Variants.Count != m_productImmutable.Variants.Count)
			{
				Log.Store.PrintError("ProductPage Mutable and Immutable products have mismatching variant counts. Mutable Product PMT ID = {0}, Name = {1}", m_productMutable.PmtId, m_productMutable.Name);
				return false;
			}
			for (int i = 0; i < m_productMutable.Variants.Count; i++)
			{
				if (m_productMutable.Variants.ElementAt(i).PmtId != m_productImmutable.Variants.ElementAt(i).PmtId)
				{
					Log.Store.PrintError("ProductPage Mutable and Immutable products have mismatching variant. Mutable Product PMT ID = {0}, Name = {1}", m_productMutable.PmtId, m_productMutable.Name);
					return false;
				}
			}
		}
		return true;
	}

	public void SetVariantQuantityAndUpdateDataModel(ProductDataModel variant, int quantity)
	{
		if (variant == null)
		{
			Log.Store.PrintError("Cannot set product quantity. variant is null.");
			return;
		}
		if (!ValidateMutableProduct())
		{
			Log.Store.PrintError("Cannot set product quantity. ProductPage has an invalid mutable product.");
			return;
		}
		ProductDataModel immutableVariant = GetImmutableVariant(variant);
		if (immutableVariant == null)
		{
			Log.Store.PrintError("Cannot set product quantity. No matching immutable variant found. PMT ID = {0}, Name = {1}.", variant.PmtId, variant.Name);
			return;
		}
		if (quantity < 1 || quantity > m_productSelection.MaxQuantity)
		{
			Log.Store.PrintError("Cannot set product quantity. Invalid input {0}", quantity);
			return;
		}
		int variantIndex = m_productImmutable.Variants.IndexOf(immutableVariant);
		if (variantIndex < 0)
		{
			Log.Store.PrintError("Cannot set product quantity. Variant not found in product. PMT ID = {0}, Name = {1}.", variant.PmtId, variant.Name);
		}
		else if (GetVariantQuantityByIndex(variantIndex) == quantity)
		{
			Log.Store.Print("SetVariantQuantityAndUpdateDataModel value matches current quantity. Quantity = {0}, ", quantity);
		}
		else if (!immutableVariant.ProductSupportsQuantitySelect())
		{
			Log.Store.PrintError("Cannot set product quantity. Product does not support variable quantity. PMT ID = {0}, Name = {1}", immutableVariant.PmtId, immutableVariant.Name);
		}
		else
		{
			if (m_productMutable == null && quantity == 1)
			{
				return;
			}
			if (m_productMutable == null && !TryCreateMutableProduct())
			{
				Log.Store.PrintError("Cannot create mutable product for quantity set. Product does not support mutability. PMT ID = {0}, Name = {1}", immutableVariant.PmtId, immutableVariant.Name);
				return;
			}
			m_variantQuantities[variantIndex] = quantity;
			m_productSelection.Quantity = quantity;
			ProductDataModel mutableVariant = m_productMutable.Variants.ElementAt(variantIndex);
			for (int i = 0; i < mutableVariant.Items.Count; i++)
			{
				RewardItemDataModel immutableItem = immutableVariant.Items.ElementAtOrDefault(i);
				RewardItemDataModel mutableItem = mutableVariant.Items.ElementAtOrDefault(i);
				if (immutableItem != null && mutableItem != null)
				{
					mutableItem.Quantity = immutableItem.Quantity * quantity;
					RewardUtils.InitializeRewardItemDataModelForShop(mutableItem, null, null);
					continue;
				}
				Log.Store.PrintError("Error modifying product item {0}, where immutable product = [{1}], mutable product = [{2}]", i, immutableVariant.Name, mutableVariant.Name);
			}
			for (int j = 0; j < mutableVariant.Prices.Count; j++)
			{
				PriceDataModel immutablePrice = immutableVariant.Prices.ElementAtOrDefault(j);
				PriceDataModel mutablePrice = mutableVariant.Prices.ElementAtOrDefault(j);
				if (immutablePrice != null && mutablePrice != null && immutablePrice.Currency == mutablePrice.Currency)
				{
					mutablePrice.Amount = immutablePrice.Amount * (float)quantity;
					mutablePrice.DisplayText = mutablePrice.Amount.ToString();
					continue;
				}
				Log.Store.PrintError("Error modifying product price {0}, where immutable product = [{1}], mutable product = [{2}]", j, immutableVariant.Name, mutableVariant.Name);
			}
			RefreshVariant(mutableVariant);
			BindProductDataModel();
			SelectVariant(mutableVariant);
		}
	}

	protected void RefreshVariant(ProductDataModel variant)
	{
		if (!CanVariantBeMutable(variant))
		{
			return;
		}
		variant.SetupProductStrings();
		if (!(variant.GetPrimaryProductTag() == "booster"))
		{
			return;
		}
		variant.VariantName = GameStrings.Format("GLUE_STORE_PACK_MUTABLE_NAME", variant.GetMaxBulkPurchaseCount());
		foreach (PriceDataModel price in variant.Prices)
		{
			price.OverrideSkuDisplayText = GameStrings.Format("GLUE_STORE_PACK_MUTABLE_PRICE", price.Amount);
		}
	}

	protected bool CanVariantBeMutable(ProductDataModel variant)
	{
		return variant.ProductSupportsQuantitySelect();
	}

	protected void BindProductDataModel()
	{
		ProductDataModel productToBind = Product;
		m_widget.BindDataModel(productToBind);
	}

	private IEnumerator OpenWhenReadyRoutine()
	{
		try
		{
			while ((bool)m_widget && m_widget.IsChangingStates)
			{
				yield return null;
			}
			if ((bool)m_widget)
			{
				m_widget.TriggerEvent("OPEN");
			}
			this.OnOpened(this, EventArgs.Empty);
		}
		finally
		{
			m_openWhenReadyCoroutine = null;
		}
	}
}
