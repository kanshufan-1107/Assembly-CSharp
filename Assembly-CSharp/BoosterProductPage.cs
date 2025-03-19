using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Blizzard.T5.AssetManager;
using Hearthstone.DataModels;
using Hearthstone.UI;
using UnityEngine;

public class BoosterProductPage : ProductPage
{
	[SerializeField]
	private AsyncReference[] m_boosterStackRefs;

	[SerializeField]
	private int m_variableQuantityMax;

	[SerializeField]
	protected float m_packShakeTime = 2f;

	[SerializeField]
	protected float m_packLandShakeDelay = 0.25f;

	[SerializeField]
	protected float m_packLandWeight = 2f;

	[SerializeField]
	protected float m_packLiftWeight = 1f;

	private int m_lastSelectedQuantity;

	private List<BoosterStack> m_boosterStacks;

	private ShakePane m_shakePane;

	private bool m_pendingDistributePacks = true;

	protected override void Awake()
	{
		base.Awake();
		base.OnProductVariantSet += HandleProductVariantSet;
	}

	protected override void Start()
	{
		m_boosterStacks = new List<BoosterStack>(m_boosterStackRefs.Length);
		AsyncReference[] boosterStackRefs = m_boosterStackRefs;
		for (int i = 0; i < boosterStackRefs.Length; i++)
		{
			boosterStackRefs[i].RegisterReadyListener<BoosterStack>(delegate
			{
				if (AreBoosterStacksReady())
				{
					AsyncReference[] boosterStackRefs2 = m_boosterStackRefs;
					foreach (AsyncReference asyncReference in boosterStackRefs2)
					{
						m_boosterStacks.Add(asyncReference.Object as BoosterStack);
					}
				}
			});
		}
		m_shakePane = GetComponentInParent<ShakePane>();
		base.Start();
	}

	protected void Update()
	{
		if (m_pendingDistributePacks && AreBoosterStacksReady() && base.IsOpen && !m_widget.IsChangingStates)
		{
			m_pendingDistributePacks = false;
			DistributeStacks();
		}
	}

	public override void Open()
	{
		if (m_container != null)
		{
			m_container.OverrideMusic(MusicPlaylistType.Invalid);
		}
		base.Open();
	}

	protected override void OnProductSet()
	{
		base.OnProductSet();
		RewardItemDataModel boosterItem = base.Product.Items.FirstOrDefault((RewardItemDataModel item) => item.Booster != null);
		if (boosterItem == null)
		{
			Log.Store.PrintError("No Boosters in Product \"{0}\"", base.Product.Name);
			return;
		}
		using (AssetHandle<GameObject> storePackDefPrefab = ShopUtils.LoadStorePackPrefab(boosterItem.Booster.Type))
		{
			StorePackDef storePackDef = (storePackDefPrefab ? storePackDefPrefab.Asset.GetComponent<StorePackDef>() : null);
			if (m_container != null)
			{
				m_container.OverrideMusic(storePackDef ? storePackDef.GetPlaylist() : MusicPlaylistType.Invalid);
			}
		}
		foreach (BoosterStack boosterStack in m_boosterStacks)
		{
			boosterStack.SetStacks(0);
		}
		if (m_variableQuantityMax > 0)
		{
			TEST_PopulateVariantsRange(base.Product, 1, m_variableQuantityMax);
		}
		List<IDataModel> selectables = base.Product.Variants.Cast<IDataModel>().ToList();
		selectables.Sort(SortProducts);
		int startingSelection = selectables.IndexOf(m_productSelection.Variant);
		if (startingSelection < 0)
		{
			startingSelection = 0;
		}
		SelectVariant(selectables.ElementAtOrDefault(startingSelection) as ProductDataModel);
	}

	private void HandleProductVariantSet(ProductSelectionDataModel _)
	{
		m_pendingDistributePacks = true;
	}

	private bool AreBoosterStacksReady()
	{
		return m_boosterStackRefs.All((AsyncReference r) => r.IsReady);
	}

	private void DistributeStacks()
	{
		ProductDataModel selectedVariant = GetSelectedVariant();
		int startingQuantity = 0;
		int quantity = selectedVariant?.CountPacks() ?? 0;
		int numStacks = m_boosterStacks.Count();
		int minPacksPerStack = quantity / numStacks;
		int leftovers = quantity % numStacks;
		bool isAdding = quantity > m_lastSelectedQuantity;
		int startingStackIdx = (m_lastSelectedQuantity + ((!isAdding) ? (-1) : 0)) % numStacks;
		m_lastSelectedQuantity = quantity;
		for (int i = 0; i < numStacks; i++)
		{
			int stackIdx = (startingStackIdx + (isAdding ? i : (-i)) + numStacks) % numStacks;
			bool getsLeftovers = stackIdx < leftovers;
			int stackSize = minPacksPerStack + (getsLeftovers ? 1 : 0);
			BoosterStack stack = m_boosterStacks[stackIdx];
			startingQuantity += stack.CurrentStackSize;
			stack.StackingDelay = (float)i * stack.StackingBaseDuration / (float)numStacks;
			stack.SetStacks(stackSize, instantaneous: false);
		}
		if ((bool)m_shakePane)
		{
			int deltaStack = quantity - startingQuantity;
			float weight = ((deltaStack > 0) ? ((float)deltaStack * m_packLandWeight) : ((float)deltaStack * m_packLiftWeight));
			float delay = ((deltaStack > 0) ? m_packLandShakeDelay : 0f);
			m_shakePane.Shake(weight, m_packShakeTime, delay);
		}
	}

	private static int SortProducts(IDataModel a, IDataModel b)
	{
		if (!(a is ProductDataModel productA) || !(b is ProductDataModel productB))
		{
			return 0;
		}
		int quantityA = productA.CountPacks();
		int quantityB = productB.CountPacks();
		if (quantityA > quantityB)
		{
			return 1;
		}
		if (quantityA < quantityB)
		{
			return -1;
		}
		return 0;
	}

	private void TEST_PopulateVariantsRange(ProductDataModel product, int minQuantity = 1, int maxQuantity = 100)
	{
		if (product.Variants.Count >= maxQuantity)
		{
			return;
		}
		ProductDataModel templateProduct = product.Variants.First();
		RewardItemDataModel templateItem = templateProduct.Items.First((RewardItemDataModel i) => i.Booster != null);
		while (minQuantity <= maxQuantity)
		{
			if (!product.Variants.Any((ProductDataModel p) => p != null && p.CountPacks() == minQuantity))
			{
				RewardItemDataModel itemDataModel = new RewardItemDataModel
				{
					Booster = templateItem.Booster,
					ItemId = templateItem.ItemId,
					ItemType = templateItem.ItemType,
					Quantity = minQuantity
				};
				PriceDataModel priceDataModel = new PriceDataModel
				{
					Currency = CurrencyType.GOLD,
					Amount = minQuantity * 100
				};
				priceDataModel.DisplayText = priceDataModel.Amount.ToString(CultureInfo.InvariantCulture);
				ProductDataModel productDataModel = new ProductDataModel
				{
					Name = $"TESTDATA {templateItem.Booster.Type}x{minQuantity}"
				};
				productDataModel.Items.Add(itemDataModel);
				productDataModel.Prices.Add(priceDataModel);
				productDataModel.Tags = templateProduct.Tags;
				product.Variants.Add(productDataModel);
			}
			minQuantity++;
		}
	}
}
