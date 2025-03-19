using System;
using System.Collections;
using System.Collections.Generic;
using Blizzard.T5.Services;
using Blizzard.Telemetry.WTCG.Client;
using Hearthstone.DataModels;
using Hearthstone.Store;
using Hearthstone.UI;
using Hearthstone.UI.Core;
using UnityEngine;

public class ShopSection : ShopBrowserElement, IPopupRendering
{
	[SerializeField]
	[Header("Section Data")]
	private WidgetTransform m_slotRenderArea;

	[SerializeField]
	private float m_slotYOffset;

	[SerializeField]
	private Vector2 m_stackingMargins = Vector2.zero;

	[SerializeField]
	private Vector2 m_slotOffsets = Vector2.zero;

	[SerializeField]
	private UIBScrollableItem m_scrollableItem;

	[SerializeField]
	private bool m_loadWidgetInstancesSynchronously;

	private float m_startingHeight;

	private LayoutMapping m_layoutMapping;

	private List<ShopRow> m_rows = new List<ShopRow>();

	private List<ShopSlot> m_slots = new List<ShopSlot>();

	private List<WidgetInstance> m_instances = new List<WidgetInstance>();

	private Coroutine m_loadingCoroutine;

	private FrameTimer m_sectionLoadTimer = new FrameTimer();

	private IPopupRoot m_popupRoot;

	private HashSet<IPopupRendering> m_popupRenderingComponents = new HashSet<IPopupRendering>();

	private Coroutine m_popupRootEnabledCoroutine;

	private const string SLOT_WIDGET_PREFAB = "ProductButton.prefab:66515ec996bf56f4d89a5946b20ffa60";

	public ProductTierDataModel ProductTierData { get; private set; }

	protected override void Awake()
	{
		base.Awake();
		m_startingHeight = base.WidgetRect.height;
	}

	private void OnDestroy()
	{
		StopSlotLoading();
		if (m_popupRootEnabledCoroutine != null)
		{
			StopCoroutine(m_popupRootEnabledCoroutine);
			m_popupRootEnabledCoroutine = null;
		}
	}

	public bool IsReady()
	{
		if (m_loadingCoroutine != null)
		{
			return false;
		}
		return true;
	}

	public void RefreshData()
	{
		ProductTierData = base.Widget.GetDataModel<ProductTierDataModel>();
		if (ProductTierData != null)
		{
			DataModelList<ShopBrowserButtonDataModel> browserButtons = ProductTierData.BrowserButtons;
			if (browserButtons == null || browserButtons.Count != 0)
			{
				List<Tuple<int, int>> slots = ProductTierData.GetSlots();
				if (slots == null)
				{
					SetEnabled(isEnabled: false);
					return;
				}
				SetupShopRows(slots.Count);
				if (m_rows.Count == 0)
				{
					SetEnabled(isEnabled: false);
					return;
				}
				m_layoutMapping = ProductTierData.GetLayoutMapping(slots, GetTotalNumberOfProductsToDisplay());
				if (m_layoutMapping == null)
				{
					SetEnabled(isEnabled: false);
					return;
				}
				float height = m_startingHeight * (float)m_layoutMapping.LayoutCount;
				SetHeight(height);
				SetEnabled(isEnabled: true);
				return;
			}
		}
		SetEnabled(isEnabled: false);
	}

	public void SetData(ProductTierDataModel productTierDataModel)
	{
		base.Widget.BindDataModel(productTierDataModel);
		RefreshData();
	}

	public void ClearData()
	{
		base.Widget.UnbindDataModel(23);
		RefreshData();
	}

	public void ClearElements()
	{
		ClearInstances();
	}

	public void RefreshContent()
	{
		if (!base.IsElementEnabled)
		{
			Log.Store.PrintWarning("Cannot refresh content of " + base.name + " as it is not enabled");
		}
		else
		{
			StartSlotLoading();
		}
	}

	public void SetScrollableState(bool isActive)
	{
		if (!base.IsElementEnabled)
		{
			Log.Store.PrintWarning("Cannot enabled scrolling state for " + base.name + " as it is not enabled");
		}
		else if (m_scrollableItem != null)
		{
			m_scrollableItem.m_active = ((!isActive) ? UIBScrollableItem.ActiveState.Inactive : UIBScrollableItem.ActiveState.Active);
		}
	}

	public List<ShopSlot> GetActiveSlots()
	{
		List<ShopSlot> slots = new List<ShopSlot>();
		foreach (ShopSlot slot in m_slots)
		{
			if (slot != null)
			{
				slots.Add(slot);
			}
		}
		return slots;
	}

	public void UpdateShopCardTelemetry(ShopCard shopCard, ShopSlot slot)
	{
		if (ProductTierData != null && ServiceManager.TryGet<IProductDataService>(out var dataService))
		{
			KeyValuePair<int, string> sectionInfo = dataService.GetSectionData(ProductTierData);
			shopCard.SectionIndex = sectionInfo.Key;
			shopCard.SectionName = sectionInfo.Value;
		}
		shopCard.SlotIndex = ((slot != null) ? m_slots.IndexOf(slot) : (-1));
	}

	protected override void OnEnabledStateChanged(bool isEnabled)
	{
		base.gameObject.SetActive(isEnabled);
		if (!isEnabled)
		{
			SetScrollableState(isActive: false);
		}
	}

	protected override void OnBoundsChanged()
	{
		base.OnBoundsChanged();
		if (m_scrollableItem != null)
		{
			m_scrollableItem.m_offset.x = base.WidgetRect.x + base.WidgetRect.width / 2f;
			m_scrollableItem.m_offset.z = base.WidgetRect.y + base.WidgetRect.height / 2f;
			m_scrollableItem.m_size.x = base.WidgetRect.width;
			m_scrollableItem.m_size.z = base.WidgetRect.height;
		}
	}

	private void StartSlotLoading()
	{
		StopSlotLoading();
		ProductTierDataModel productTierData = ProductTierData;
		if (productTierData != null && productTierData.LayoutMap?.Count == 0)
		{
			ClearElements();
		}
		else
		{
			m_loadingCoroutine = StartCoroutine(LoadSlotsCoroutine());
		}
	}

	private void StopSlotLoading(string error = null)
	{
		if (m_loadingCoroutine != null)
		{
			StopCoroutine(m_loadingCoroutine);
			m_loadingCoroutine = null;
			if (!string.IsNullOrEmpty(error))
			{
				Log.Store.PrintError("Section loading was unexpectedly stopped - " + error);
			}
			m_sectionLoadTimer.StopRecording();
		}
	}

	private IEnumerator LoadSlotsCoroutine()
	{
		Log.Store.PrintDebug("Section " + base.name + " started loading slots");
		m_sectionLoadTimer.StartRecording();
		DataModelList<ShopBrowserButtonDataModel> browserButtons = GetAllProducts();
		SetupInstances(m_layoutMapping.GetNumberOfTotalSlots());
		foreach (WidgetInstance instance in m_instances)
		{
			instance.gameObject.SetActive(value: false);
		}
		int dataIndex = 0;
		for (int i = 0; i < m_layoutMapping.GetNumberOfTotalSlots() && i < m_instances.Count; i++)
		{
			WidgetInstance inst = m_instances[i];
			if (inst == null)
			{
				SetEnabled(isEnabled: false);
				StopSlotLoading("Instance is null");
				yield break;
			}
			LayoutMapEntry slotData = m_layoutMapping.SlotData[i];
			ShopBrowserButtonDataModel button = null;
			if (ProductTierData.DisplayTierData && i % m_layoutMapping.GetNumberOfSingleSlots() == 0)
			{
				button = new ShopBrowserButtonDataModel();
				if (i == 0)
				{
					button.ShowTierData = true;
				}
			}
			else if (dataIndex < browserButtons.Count)
			{
				button = browserButtons[dataIndex];
				dataIndex++;
			}
			if (button == null)
			{
				button = new ShopBrowserButtonDataModel();
			}
			button.SlotWidth = slotData.UnitWidth;
			button.SlotWidthPercentage = (float)slotData.UnitWidth / (float)m_layoutMapping.TotalUnitWidth;
			button.SlotHeight = slotData.UnitHeight;
			button.SlotHeightPercentage = (float)slotData.UnitHeight / (float)m_layoutMapping.TotalUnitHeight;
			button.SlotPositionX = slotData.OriginX;
			button.SlotPositionY = slotData.OriginY;
			Log.Store.PrintDebug("Started loading section " + button.GetName());
			inst.BindDataModel(button);
			inst.BindDataModel(ProductTierData);
			inst.gameObject.SetActive(value: true);
			if (inst.WillLoadSynchronously)
			{
				inst.Initialize();
			}
		}
		for (int j = 0; j < m_layoutMapping.GetNumberOfTotalSlots() && j < m_instances.Count; j++)
		{
			WidgetInstance inst2 = m_instances[j];
			if (inst2 == null)
			{
				SetEnabled(isEnabled: false);
				StopSlotLoading("Initialized instance is null");
				yield break;
			}
			if (inst2.IsReady)
			{
				if (inst2.Widget == null)
				{
					SetEnabled(isEnabled: false);
					StopSlotLoading("Instance failed to load template");
					yield break;
				}
				ShopSlot slot = inst2.Widget.GetComponent<ShopSlot>();
				if (slot == null)
				{
					SetEnabled(isEnabled: false);
					StopSlotLoading("Instance does not contain shop slot");
					yield break;
				}
				if (!m_slots.Contains(slot))
				{
					m_slots.Add(slot);
				}
			}
			yield return null;
		}
		Rect slotRenderRect = m_slotRenderArea.Rect;
		float singleUnitWidth = (slotRenderRect.width - m_stackingMargins.x * (float)(m_layoutMapping.TotalUnitWidth - 1)) / (float)m_layoutMapping.TotalUnitWidth;
		float singleUnitHeight = (slotRenderRect.height - m_stackingMargins.y * (float)(m_layoutMapping.TotalUnitHeight - 1)) / (float)m_layoutMapping.TotalUnitHeight;
		float slotRenderOffsetX = (Mathf.Abs(m_slotRenderArea.Right) - Mathf.Abs(m_slotRenderArea.Left)) / 2f;
		float slotRenderOffsetZ = (Mathf.Abs(m_slotRenderArea.Top) - Mathf.Abs(m_slotRenderArea.Bottom)) / 2f;
		foreach (ShopSlot slot2 in m_slots)
		{
			slot2.SetParent(this);
			slot2.RefreshData();
			if (slot2.IsElementEnabled)
			{
				ShopBrowserButtonDataModel slotData2 = slot2.BrowserButtonDataModel;
				if (slotData2 == null)
				{
					SetEnabled(isEnabled: false);
					StopSlotLoading(base.transform.parent.name + " doesn't contain data but isn't disabled");
					yield break;
				}
				slot2.RefreshContent();
				float slotWidth = singleUnitWidth * (float)slotData2.SlotWidth + m_stackingMargins.x * (float)(slotData2.SlotWidth - 1);
				float slotHeight = singleUnitHeight * (float)slotData2.SlotHeight + m_stackingMargins.y * (float)(slotData2.SlotHeight - 1);
				slot2.SuppressBoundsChangedTrigger("SECTION_REFRESH", isSuppressed: true);
				slot2.SetWidth(slotWidth);
				slot2.SetHeight(slotHeight);
				float slotX = (float)slotData2.SlotPositionX * singleUnitWidth + (float)slotData2.SlotPositionX * m_stackingMargins.x + (m_slotOffsets.x + slotRenderOffsetX);
				slotX -= slotRenderRect.width / 2f - slot2.WidgetRect.width / 2f;
				float slotZ = m_slotOffsets.y + slotRenderOffsetZ - ((float)slotData2.SlotPositionY * singleUnitHeight + (float)slotData2.SlotPositionY * m_stackingMargins.y);
				slotZ += slotRenderRect.height / 2f - slot2.WidgetRect.height / 2f;
				slot2.ElementLocalPosition = new Vector3(slotX, m_slotYOffset, slotZ);
				slot2.SuppressBoundsChangedTrigger("SECTION_REFRESH", isSuppressed: false);
			}
		}
		bool slotsReady = false;
		base.Widget.RegisterDoneChangingStatesListener(delegate
		{
			slotsReady = true;
		}, null, callImmediatelyIfSet: true, doOnce: true);
		while (!slotsReady)
		{
			yield return null;
		}
		m_loadingCoroutine = null;
		m_sectionLoadTimer.StopRecording();
		Log.Store.PrintDebug($"Completed loading {m_slots.Count} slots - {m_sectionLoadTimer.TimeTakenInfo}");
	}

	private DataModelList<ShopBrowserButtonDataModel> GetAllProducts()
	{
		DataModelList<ShopBrowserButtonDataModel> buttons = new DataModelList<ShopBrowserButtonDataModel>();
		foreach (ShopRow row in m_rows)
		{
			if (row.BrowserButtons != null)
			{
				buttons.AddRange(row.BrowserButtons);
			}
		}
		return buttons;
	}

	private void SetupShopRows(int itemsPerRow)
	{
		DataModelList<ShopBrowserButtonDataModel> browserButtons = ProductTierData.BrowserButtons;
		m_rows = new List<ShopRow>();
		IProductDataService dataService;
		bool showAllRows = ServiceManager.TryGet<IProductDataService>(out dataService) && dataService.TierHasShowIfAllOwnedTag(ProductTierData);
		int maxItemsInSection = Mathf.CeilToInt((float)browserButtons.Count / (float)itemsPerRow) * itemsPerRow;
		int slotIndex = 0;
		int buttonIndex = 0;
		for (; slotIndex < maxItemsInSection; slotIndex += itemsPerRow)
		{
			ShopRow row = new ShopRow();
			row.SetData(browserButtons, itemsPerRow, ref buttonIndex);
			if (row.IsEnabled || showAllRows)
			{
				m_rows.Add(row);
			}
		}
	}

	private int GetTotalNumberOfProductsToDisplay()
	{
		int total = 0;
		foreach (ShopRow row in m_rows)
		{
			if (row.BrowserButtons != null)
			{
				total += row.BrowserButtons.Count;
			}
		}
		return total;
	}

	private void SetupInstances(int targetCount)
	{
		if (targetCount <= 0)
		{
			ClearInstances();
			return;
		}
		targetCount = Mathf.Max(targetCount, 0);
		while (m_instances.Count > targetCount)
		{
			int lastIndex = m_instances.Count - 1;
			if (lastIndex < 0)
			{
				continue;
			}
			WidgetInstance instance = m_instances[lastIndex];
			m_instances.RemoveAt(lastIndex);
			if (instance != null)
			{
				m_slots.RemoveAll((ShopSlot s) => s == null || s.transform.IsChildOf(instance.transform));
				UnityEngine.Object.Destroy(instance.gameObject);
			}
		}
		while (m_instances.Count < targetCount)
		{
			WidgetInstance instance2 = WidgetInstance.Create("ProductButton.prefab:66515ec996bf56f4d89a5946b20ffa60");
			instance2.SetLayerOverride((GameLayer)base.gameObject.layer);
			instance2.transform.SetParent(base.transform, worldPositionStays: false);
			instance2.name = string.Format($"Slot {m_instances.Count}");
			instance2.WillLoadSynchronously = m_loadWidgetInstancesSynchronously;
			m_instances.Add(instance2);
		}
		foreach (ShopSlot slot in m_slots)
		{
			slot.ClearData();
		}
	}

	public void ClearInstances()
	{
		foreach (WidgetInstance inst in m_instances)
		{
			if (inst != null)
			{
				UnityEngine.Object.Destroy(inst.gameObject);
			}
		}
		m_instances.Clear();
		m_slots.Clear();
	}

	void IPopupRendering.EnablePopupRendering(IPopupRoot popupRoot)
	{
		m_popupRoot = popupRoot;
		if (m_popupRootEnabledCoroutine != null)
		{
			StopCoroutine(m_popupRootEnabledCoroutine);
			m_popupRootEnabledCoroutine = null;
		}
		m_popupRootEnabledCoroutine = StartCoroutine(EnablePopupRenderingInternal());
	}

	private IEnumerator EnablePopupRenderingInternal()
	{
		while (!IsReady())
		{
			yield return null;
		}
		m_popupRoot.ApplyPopupRendering(base.transform, m_popupRenderingComponents);
	}

	void IPopupRendering.DisablePopupRendering()
	{
		if (m_popupRoot != null)
		{
			m_popupRoot.CleanupPopupRendering(m_popupRenderingComponents);
			m_popupRoot = null;
		}
	}

	bool IPopupRendering.HandlesChildPropagation()
	{
		return true;
	}
}
