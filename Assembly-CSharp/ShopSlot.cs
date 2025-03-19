using System;
using System.Collections;
using System.Collections.Generic;
using Blizzard.Commerce;
using Blizzard.T5.Services;
using Blizzard.Telemetry.WTCG.Client;
using Game.Shop;
using Hearthstone.DataModels;
using Hearthstone.MarketingImages;
using Hearthstone.Store;
using Hearthstone.UI;
using UnityEngine;

public class ShopSlot : ShopBrowserElement
{
	[Header("Slot Data")]
	[SerializeField]
	private BoxCollider m_boxCollider;

	[SerializeField]
	private float m_boxColiderYSize;

	[SerializeField]
	private Clickable m_clickable;

	private bool m_isBlocked;

	private Coroutine m_disableColliderCoroutine;

	private ShopSection m_parentSection;

	private ShopCard m_shopCardTelemetry = new ShopCard();

	[SerializeField]
	private ShopMarketingImageFitter m_marketingImageFitter;

	private MarketingImagesService m_mimgService;

	public ShopBrowserButtonDataModel BrowserButtonDataModel { get; private set; }

	public bool IsEmpty => BrowserButtonDataModel?.DisplayProduct?.IsEmpty() == true;

	protected override void Awake()
	{
		base.Awake();
		if (m_clickable != null)
		{
			m_clickable.AddEventListener(UIEventType.RELEASE, OnRelease);
			m_clickable.AddEventListener(UIEventType.ROLLOVER, OnRollOver);
			m_clickable.AddEventListener(UIEventType.ROLLOUT, OnRollOut);
		}
		ServiceManager.TryGet<MarketingImagesService>(out m_mimgService);
		RefreshInput();
	}

	private void OnDestroy()
	{
		if (m_disableColliderCoroutine != null)
		{
			StopCoroutine(m_disableColliderCoroutine);
			m_disableColliderCoroutine = null;
		}
	}

	public bool HasProduct(ProductDataModel product)
	{
		if (!IsEmpty)
		{
			if (BrowserButtonDataModel.DisplayProduct != product)
			{
				return BrowserButtonDataModel.DisplayProduct.Variants.Contains(product);
			}
			return true;
		}
		return false;
	}

	public void SetParent(ShopSection shopSection)
	{
		m_parentSection = shopSection;
	}

	public void SetBlocked(bool isBlocked)
	{
		m_isBlocked = isBlocked;
		RefreshInput();
	}

	public void RefreshData()
	{
		BrowserButtonDataModel = base.Widget.GetDataModel<ShopBrowserButtonDataModel>();
		if (BrowserButtonDataModel != null)
		{
			ProductDataModel productDataModel = BrowserButtonDataModel.DisplayProduct;
			bool isEmpty = productDataModel.IsEmpty();
			IGamemodeAvailabilityService.Status status;
			bool isGamemodeAvailable = ShopUtils.IsProductGamemodesAvailable(productDataModel, out status);
			if (isEmpty || !isGamemodeAvailable)
			{
				ShopBlockingPlateDataModel blockingPlate = new ShopBlockingPlateDataModel
				{
					BlockingType = ShopBlockingPlateType.None
				};
				if (isEmpty)
				{
					productDataModel = ProductFactory.CreateEmptyProductDataModel();
					BrowserButtonDataModel.DisplayProduct = productDataModel;
					blockingPlate.BlockingType = ShopBlockingPlateType.Empty;
				}
				else if (!isGamemodeAvailable)
				{
					blockingPlate.BlockingType = GetBlockingPlateTypeFromGamemodeStatus(status);
				}
				base.Widget.BindDataModel(blockingPlate);
				BrowserButtonDataModel.BlockingPlate = blockingPlate;
			}
			else
			{
				base.Widget.UnbindDataModel(890);
				SetUpMarketingImage();
			}
			if (CheckEmoteFanLayout(productDataModel.RewardList?.Items))
			{
				productDataModel.Tags.Add("use_bgemote_fan_layout");
			}
			SetEnabled(isEnabled: true);
		}
		else
		{
			SetEnabled(isEnabled: false);
		}
		UpdateShopCardTelemetry();
	}

	public void SetData(ShopBrowserButtonDataModel browserButtonDataModel)
	{
		base.Widget.BindDataModel(browserButtonDataModel);
		RefreshData();
	}

	public void ClearData()
	{
		ShopBrowserButtonDataModel blankButton = new ShopBrowserButtonDataModel();
		if (BrowserButtonDataModel != null)
		{
			blankButton.SlotWidth = BrowserButtonDataModel.SlotWidth;
			blankButton.SlotWidthPercentage = BrowserButtonDataModel.SlotWidthPercentage;
			blankButton.SlotHeight = BrowserButtonDataModel.SlotHeight;
			blankButton.SlotHeightPercentage = BrowserButtonDataModel.SlotHeightPercentage;
			blankButton.SlotPositionX = BrowserButtonDataModel.SlotPositionX;
			blankButton.SlotPositionY = BrowserButtonDataModel.SlotPositionY;
		}
		base.Widget.BindDataModel(blankButton);
	}

	public void RefreshContent()
	{
		if (BrowserButtonDataModel != null)
		{
			ProductDataModel productDataModel = BrowserButtonDataModel.DisplayProduct;
			if (productDataModel != null)
			{
				PileEmotes(productDataModel.RewardList?.Items);
				base.Widget.BindDataModel(productDataModel);
			}
			else
			{
				base.Widget.UnbindDataModel(15);
			}
		}
		m_disableColliderCoroutine = StartCoroutine(DisableChildCollidersCoroutine());
		RefreshInput();
	}

	public ShopCard GetShopCardTelemetry()
	{
		UpdateShopCardTelemetryTimeRemaining();
		return m_shopCardTelemetry;
	}

	protected override void OnEnabledStateChanged(bool isEnabled)
	{
		base.gameObject.SetActive(isEnabled);
	}

	protected override void OnBoundsChanged()
	{
		base.OnBoundsChanged();
		if (m_boxCollider != null)
		{
			Rect rect = base.WidgetTransform.Rect;
			m_boxCollider.transform.localPosition = new Vector3(rect.center.x, m_boxCollider.center.y, rect.center.y);
			m_boxCollider.size = new Vector3(rect.width, m_boxColiderYSize, rect.height);
		}
	}

	private void RefreshInput()
	{
		bool isClickable = !m_isBlocked && !IsEmpty;
		if (m_boxCollider != null)
		{
			m_boxCollider.enabled = isClickable;
		}
		if (m_clickable != null)
		{
			m_clickable.enabled = isClickable;
		}
	}

	private IEnumerator DisableChildCollidersCoroutine()
	{
		while (base.Widget.IsChangingStates)
		{
			yield return null;
		}
		Collider[] componentsInChildren = GetComponentsInChildren<Collider>();
		foreach (Collider collider in componentsInChildren)
		{
			if (!(collider == m_boxCollider))
			{
				collider.enabled = false;
			}
		}
		m_disableColliderCoroutine = null;
	}

	private void OnRelease(UIEvent e)
	{
		if (IsEmpty)
		{
			Log.Store.PrintWarning("Ignoring click on shop slot that is not filled. The clickable for this ShopSlot should be disabled.");
			return;
		}
		Shop.Get().TryGetCurrentTabIds(out var currentTabId, out var currentSubTabId);
		TelemetryManager.Client().SendShopCardClick(GetShopCardTelemetry(), StoreManager.Get().CurrentShopType.ToString(), currentTabId, currentSubTabId);
		if (BrowserButtonDataModel == null)
		{
			return;
		}
		ProductDataModel product = BrowserButtonDataModel.DisplayProduct;
		if (product == null)
		{
			return;
		}
		if (product.Tags.Contains("vc") && product.Variants.Count > 1)
		{
			ProductDataModel productToPreSelect = product;
			if (VariantUtils.TryFindSpecialOfferVariant(product, out var specialOfferVariant))
			{
				productToPreSelect = specialOfferVariant;
			}
			Shop.Get().ProductPageController.OpenVirtualCurrencyPurchase(productToPreSelect);
			if (product != null && ServiceManager.TryGet<IProductDataService>(out var productService))
			{
				productService.MarkProductAsSeen(product);
			}
		}
		else
		{
			Shop.Get().ProductPageController.OpenProductPage(product);
		}
	}

	private void OnRollOver(UIEvent e)
	{
		if (BrowserButtonDataModel != null)
		{
			BrowserButtonDataModel.Hovered = true;
		}
	}

	private void OnRollOut(UIEvent e)
	{
		if (BrowserButtonDataModel != null)
		{
			BrowserButtonDataModel.Hovered = false;
		}
	}

	private void PileEmotes(DataModelList<RewardItemDataModel> items)
	{
		if (items == null)
		{
			return;
		}
		DataModelList<BattlegroundsEmoteDataModel> emotes = new DataModelList<BattlegroundsEmoteDataModel>();
		List<int> indicesToRemove = new List<int>();
		for (int i = 0; i < items.Count; i++)
		{
			RewardItemDataModel item = items[i];
			if (item.ItemType == RewardItemType.BATTLEGROUNDS_EMOTE)
			{
				emotes.Add(item.BGEmote);
				indicesToRemove.Add(i);
			}
		}
		if (emotes.Count > 1 && emotes.Count != items.Count)
		{
			RewardItemDataModel rewardItemDataModel = new RewardItemDataModel
			{
				ItemType = RewardItemType.BATTLEGROUNDS_EMOTE_PILE,
				ItemId = 0,
				BGEmotePile = emotes
			};
			items.Insert(indicesToRemove[0], rewardItemDataModel);
			for (int i2 = indicesToRemove.Count - 1; i2 >= 0; i2--)
			{
				items.RemoveAt(indicesToRemove[i2] + 1);
			}
		}
	}

	private bool CheckEmoteFanLayout(DataModelList<RewardItemDataModel> items)
	{
		if (items == null)
		{
			return false;
		}
		int emoteCount = 0;
		foreach (RewardItemDataModel item in items)
		{
			if (item.ItemType == RewardItemType.BATTLEGROUNDS_EMOTE)
			{
				emoteCount++;
			}
		}
		if (emoteCount == 6)
		{
			return items.Count == emoteCount;
		}
		return false;
	}

	private void UpdateShopCardTelemetry()
	{
		if (!(m_parentSection == null))
		{
			m_shopCardTelemetry = new ShopCard();
			m_parentSection.UpdateShopCardTelemetry(m_shopCardTelemetry, this);
			ShopBrowserButtonDataModel browserButtonDataModel = BrowserButtonDataModel;
			bool? obj;
			if (browserButtonDataModel == null)
			{
				obj = null;
			}
			else
			{
				ProductDataModel displayProduct = browserButtonDataModel.DisplayProduct;
				obj = ((displayProduct != null) ? new bool?(!displayProduct.IsEmpty()) : ((bool?)null));
			}
			bool? flag = obj;
			if (flag == true)
			{
				m_shopCardTelemetry.Product = new Product
				{
					ProductId = BrowserButtonDataModel.DisplayProduct.PmtId
				};
			}
			UpdateShopCardTelemetryTimeRemaining();
		}
	}

	private void UpdateShopCardTelemetryTimeRemaining()
	{
		if (!ServiceManager.TryGet<IProductDataService>(out var dataService) || BrowserButtonDataModel == null || BrowserButtonDataModel.DisplayProduct == null || !ProductId.IsValid(BrowserButtonDataModel.DisplayProduct.PmtId) || !dataService.TryGetProduct(BrowserButtonDataModel.DisplayProduct.PmtId, out var bundle))
		{
			return;
		}
		ProductAvailabilityRange range = bundle.GetBundleAvailabilityRange(StoreManager.Get());
		if (range != null)
		{
			DateTime? endDateTime = range.EndDateTime;
			if (endDateTime.HasValue)
			{
				DateTime nowUtc = DateTime.UtcNow;
				m_shopCardTelemetry.SecondsRemaining = (int)Math.Min((endDateTime.Value - nowUtc).TotalSeconds, 2147483647.0);
			}
		}
	}

	private static ShopBlockingPlateType GetBlockingPlateTypeFromGamemodeStatus(IGamemodeAvailabilityService.Status status)
	{
		if (status <= IGamemodeAvailabilityService.Status.NOT_DOWNLOADED)
		{
			return ShopBlockingPlateType.ModularDownload;
		}
		if (status <= IGamemodeAvailabilityService.Status.TUTORIAL_INCOMPLETE)
		{
			return ShopBlockingPlateType.Tutorial;
		}
		return ShopBlockingPlateType.None;
	}

	private void SetUpMarketingImage()
	{
		if (BrowserButtonDataModel == null || BrowserButtonDataModel.DisplayProduct == null)
		{
			return;
		}
		ProductDataModel productDataModel = BrowserButtonDataModel.DisplayProduct;
		if (!m_marketingImageFitter || m_mimgService == null)
		{
			productDataModel.ShowMarketingImage = false;
			return;
		}
		long productId = productDataModel.PmtId;
		if (productId <= 0)
		{
			productDataModel.ShowMarketingImage = false;
			return;
		}
		if ((bool)m_parentSection && m_parentSection.ProductTierData != null && m_parentSection.ProductTierData.Tags.Contains("hide_marketing_images"))
		{
			productDataModel.ShowMarketingImage = false;
			return;
		}
		MarketingImageSlot preferredSlotSize = ((BrowserButtonDataModel.SlotWidth > 6) ? MarketingImageSlot.FullRow : MarketingImageSlot.HalfRow);
		if (!m_mimgService.TryGetConfig(productId, preferredSlotSize, out var mimgConfig))
		{
			productDataModel.ShowMarketingImage = false;
			return;
		}
		m_marketingImageFitter.SetTexture(mimgConfig);
		productDataModel.ShowMarketingImage = true;
	}
}
