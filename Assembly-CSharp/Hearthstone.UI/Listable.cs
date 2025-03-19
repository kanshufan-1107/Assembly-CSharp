using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Hearthstone.DataModels;
using Hearthstone.UI.Core;
using Hearthstone.UI.Logging;
using Hearthstone.UI.Scripting;
using Hearthstone.Util;
using Unity.Profiling;
using UnityEngine;

namespace Hearthstone.UI;

[AddComponentMenu("")]
[DisallowMultipleComponent]
[HelpURL("https://confluence.blizzard.com/x/PxZVJg")]
[ExecuteAlways]
public class Listable : WidgetBehavior, IBoundsDependent, IPopupRendering, ILayerOverridable, IVisibleWidgetComponent
{
	public enum LayoutMode
	{
		Vertical,
		Horizontal
	}

	[Tooltip("A mode that determines which direction the list should be laid out in.")]
	[SerializeField]
	private LayoutMode m_layoutMode;

	[SerializeField]
	[Tooltip("A reference to the Widget to be used as the item template.")]
	private WeakAssetReference m_itemTemplate;

	[Tooltip("A script that needs to evaluate to a data model list that is used to generate the list of items.")]
	[SerializeField]
	private ScriptString m_valueScript;

	[SerializeField]
	private int m_maxWidgetsToShow = 9999;

	[SerializeField]
	private float m_spacing;

	[Header("Virtual Scrolling")]
	[SerializeField]
	private bool m_useVirtualScrollingList;

	[SerializeField]
	private float m_virtualScrollingBuffer = 0.5f;

	private readonly List<WidgetInstance> m_widgetItems = new List<WidgetInstance>();

	private readonly HashSet<string> m_widgetsDoneChangingStates = new HashSet<string>();

	private BoxCollider m_listableBounds;

	private bool m_initialized;

	private HashSet<int> m_dataModelIDs;

	private string m_lastScriptString;

	private int m_lastDataVersion;

	private GameLayer? m_overrideLayer;

	private bool m_isOverrideLayerApplied;

	private bool m_isVisible = true;

	private IPopupRoot m_popupRoot;

	private HashSet<IPopupRendering> m_popupComponents = new HashSet<IPopupRendering>();

	private Action m_onDataChanged;

	private Vector3? m_cachedLayoutDirection;

	private const int MinimumScrollingListAmount = 1;

	private bool m_virtualScrollingInitialized;

	private bool m_virtualScrollingIsDirty;

	private bool m_virtualScrollingVisible;

	private int m_firstVisible = -1;

	private int m_virtualAmountInBounds = 1;

	private IDataModelList m_cachedMetaDataModels;

	private readonly List<Vector3> m_widgetSizes = new List<Vector3>();

	private ProfilerMarker m_visibilityUpdatedMarker = new ProfilerMarker("Listable.OnVirtualScrollingVisible");

	private UIBScrollableItem m_virtualScrollingFiller;

	private IScrollingVisibilityProvider m_visibilityProvider;

	private static ProfilerMarker s_onUpdateProfilerMarker = new ProfilerMarker("Listable.OnUpdate");

	private bool ShouldUseVirtualScrollingList
	{
		get
		{
			if (m_useVirtualScrollingList && Application.IsPlaying(this))
			{
				return VisibilityProvider != null;
			}
			return false;
		}
	}

	private UIBScrollableItem VirtualScrollingFiller
	{
		get
		{
			if (m_virtualScrollingFiller == null)
			{
				m_virtualScrollingFiller = new GameObject("Bounds Filler").AddComponent<UIBScrollableItem>();
				m_virtualScrollingFiller.transform.SetParent(base.transform);
				m_virtualScrollingFiller.transform.localScale = Vector3.one;
			}
			return m_virtualScrollingFiller;
		}
	}

	private IScrollingVisibilityProvider VisibilityProvider
	{
		get
		{
			if (m_visibilityProvider == null)
			{
				m_visibilityProvider = base.gameObject.GetComponentInParent<IScrollingVisibilityProvider>();
			}
			return m_visibilityProvider;
		}
	}

	public bool HandlesChildLayers => true;

	public IEnumerable<WidgetInstance> WidgetItems => m_widgetItems;

	public int WidgetItemsCount => m_widgetItems.Count;

	[Overridable]
	public LayoutMode Layout
	{
		get
		{
			return m_layoutMode;
		}
		set
		{
			m_layoutMode = value;
		}
	}

	[Overridable]
	public float Spacing
	{
		get
		{
			return m_spacing;
		}
		set
		{
			m_spacing = value;
			HandleLayout();
		}
	}

	public bool IsDirty => m_lastDataVersion != GetLocalDataVersion();

	public bool NeedsBounds => false;

	public override bool IsChangingStates
	{
		get
		{
			if (m_initialized && !IsDirty)
			{
				return m_widgetsDoneChangingStates.Count < m_widgetItems.Count;
			}
			return true;
		}
	}

	public bool IsDesiredHidden => base.Owner.IsDesiredHiddenInHierarchy;

	public bool IsDesiredHiddenInHierarchy => base.Owner.IsDesiredHiddenInHierarchy;

	public bool HandlesChildVisibility => true;

	public BoxCollider GetOrCreateColliderFromItemBounds(bool isEnabled = true)
	{
		Bounds bounds = default(Bounds);
		for (int i = 0; i < m_widgetItems.Count; i++)
		{
			Bounds temp = WidgetTransform.GetBoundsOfWidgetTransforms(m_widgetItems[i].transform, base.transform.worldToLocalMatrix);
			bounds.Encapsulate(temp);
		}
		if (m_listableBounds == null)
		{
			m_listableBounds = base.gameObject.AddComponent<BoxCollider>();
		}
		m_listableBounds.center = bounds.center;
		m_listableBounds.size = bounds.size;
		m_listableBounds.enabled = isEnabled;
		return m_listableBounds;
	}

	public void SetMaxWidgetCount(int maxWidgets)
	{
		m_maxWidgetsToShow = ((maxWidgets > 0) ? maxWidgets : m_maxWidgetsToShow);
	}

	public void UpdatePositions()
	{
		HandleLayout();
	}

	public void RegisterDataChangedListener(Action onDataChanged)
	{
		m_onDataChanged = (Action)Delegate.Combine(m_onDataChanged, onDataChanged);
	}

	public void UnregisterDataChangedListener(Action onDataChanged)
	{
		m_onDataChanged = (Action)Delegate.Remove(m_onDataChanged, onDataChanged);
	}

	public bool HandlesChildPropagation()
	{
		return true;
	}

	public void EnablePopupRendering(IPopupRoot popupRoot)
	{
		if (m_popupRoot != popupRoot && base.Owner != null)
		{
			base.Owner.RemoveReadyListener(ApplyPopupRendering);
			base.Owner.RegisterReadyListener(ApplyPopupRendering, popupRoot);
		}
		m_popupRoot = popupRoot;
	}

	public void DisablePopupRendering()
	{
		if (m_popupRoot != null)
		{
			m_popupRoot.CleanupPopupRendering(m_popupComponents);
		}
		if (base.Owner != null)
		{
			base.Owner.RemoveReadyListener(ApplyPopupRendering);
		}
		m_popupRoot = null;
		m_popupComponents.Clear();
	}

	public void ApplyPopupRendering(object payload)
	{
		if (!(payload is IPopupRoot popupRoot))
		{
			return;
		}
		if (m_popupComponents == null)
		{
			m_popupComponents = new HashSet<IPopupRendering>();
		}
		foreach (WidgetInstance itemWidget in m_widgetItems)
		{
			if (m_widgetsDoneChangingStates.Contains(itemWidget.name))
			{
				popupRoot.ApplyPopupRendering(itemWidget.transform, m_popupComponents, m_isOverrideLayerApplied, (int)m_overrideLayer.Value, m_isVisible);
			}
		}
	}

	private void RemovePopupRendering(Transform objectRoot)
	{
		IPopupRendering[] popupComponents = objectRoot.GetComponentsInChildren<IPopupRendering>(includeInactive: true);
		if (popupComponents == null)
		{
			return;
		}
		IPopupRendering[] array = popupComponents;
		foreach (IPopupRendering component in array)
		{
			if (component != null && component as UnityEngine.Object != null)
			{
				component.DisablePopupRendering();
				m_popupComponents.Remove(component);
			}
		}
	}

	protected override void OnInitialize()
	{
	}

	public override void OnUpdate()
	{
		using (s_onUpdateProfilerMarker.Auto())
		{
			int dataVersion = GetLocalDataVersion();
			if (m_lastDataVersion != dataVersion)
			{
				HandleDataChanged();
				m_lastDataVersion = dataVersion;
				m_onDataChanged?.Invoke();
				if (base.IsChangingStatesInternally)
				{
					CheckIfDoneChangingStates();
				}
			}
			if (m_virtualScrollingIsDirty)
			{
				VisibilityProvider.UpdateVisibility(base.gameObject);
				m_virtualScrollingIsDirty = false;
			}
		}
	}

	private void OnLayerOverride(GameLayer obj)
	{
	}

	private void OnLayerClear(GameLayer obj)
	{
	}

	public override bool TryIncrementDataVersion(int id)
	{
		if (m_dataModelIDs == null || m_lastScriptString != m_valueScript.Script)
		{
			m_dataModelIDs = m_valueScript.GetDataModelIDs();
			m_lastScriptString = m_valueScript.Script;
		}
		if (!m_dataModelIDs.Contains(id))
		{
			return false;
		}
		IncrementLocalDataVersion();
		return true;
	}

	public override bool GetIsChangingStates(Func<GameObject, bool> includeGameObject)
	{
		if (!includeGameObject(base.gameObject))
		{
			return false;
		}
		return IsChangingStates;
	}

	public void SetVisibility(bool isVisible, bool isInternal)
	{
		if (isVisible == m_isVisible)
		{
			return;
		}
		m_isVisible = isVisible;
		if (m_isVisible && !IsChangingStates)
		{
			foreach (WidgetInstance widgetInstance in m_widgetItems)
			{
				if (!(widgetInstance == null))
				{
					widgetInstance.Show();
					if (m_overrideLayer.HasValue)
					{
						widgetInstance.SetLayerOverride(m_overrideLayer.Value);
						m_isOverrideLayerApplied = true;
					}
				}
			}
		}
		else if (!m_isVisible)
		{
			foreach (WidgetInstance widgetInstance2 in m_widgetItems)
			{
				if (!(widgetInstance2 == null))
				{
					widgetInstance2.Hide();
				}
			}
		}
		SetVirtualScrollingDirty();
	}

	public void SetLayerOverride(GameLayer layer)
	{
		m_overrideLayer = layer;
		if (!m_isVisible)
		{
			return;
		}
		foreach (WidgetInstance widgetItem in m_widgetItems)
		{
			widgetItem.SetLayerOverride(layer);
		}
		m_isOverrideLayerApplied = true;
	}

	public void ClearLayerOverride()
	{
		if (m_overrideLayer.HasValue && m_isOverrideLayerApplied)
		{
			foreach (WidgetInstance widgetItem in m_widgetItems)
			{
				widgetItem.ClearLayerOverride();
			}
		}
		m_isOverrideLayerApplied = false;
		m_overrideLayer = null;
	}

	private void AddListItem()
	{
		WidgetInstance itemWidget = WidgetInstance.Create(m_itemTemplate.AssetString);
		GameObject itemGo = itemWidget.gameObject;
		itemGo.name = $"ListItem_{m_widgetItems.Count}";
		itemGo.hideFlags = HideFlags.DontSave;
		if (!Application.IsPlaying(this))
		{
			itemGo.hideFlags |= HideFlags.NotEditable;
		}
		GameUtils.SetParent(itemGo, base.gameObject, withRotation: true);
		itemWidget.RegisterStartChangingStatesListener(delegate
		{
			HandleStartChangingStates();
			m_widgetsDoneChangingStates.Remove(itemWidget.name);
		});
		itemWidget.RegisterDoneChangingStatesListener(delegate
		{
			HandleDoneChangingStates(itemWidget);
		});
		itemWidget.RegisterDoneChangingStatesListener(delegate
		{
			if (m_popupRoot != null)
			{
				m_popupRoot.ApplyPopupRendering(itemWidget.transform, m_popupComponents, m_isOverrideLayerApplied, (int)m_overrideLayer.Value, m_isVisible);
			}
		}, true);
		base.Owner.AddNestedInstance(itemWidget, base.gameObject);
		m_widgetItems.Add(itemWidget);
		itemWidget.Initialize();
		itemWidget.Hide();
	}

	private void RemoveListItem()
	{
		if (m_widgetItems.Count != 0)
		{
			WidgetInstance itemWidget = m_widgetItems.Last();
			m_widgetItems.Remove(itemWidget);
			m_widgetsDoneChangingStates.Remove(itemWidget.name);
			base.Owner.RemoveNestedInstance(itemWidget);
			if (m_popupRoot != null)
			{
				RemovePopupRendering(itemWidget.transform);
			}
			if (Application.IsPlaying(this))
			{
				UnityEngine.Object.Destroy(itemWidget.gameObject);
			}
			else
			{
				UnityEngine.Object.DestroyImmediate(itemWidget.gameObject);
			}
			if (m_widgetItems.Count == 0)
			{
				m_virtualScrollingInitialized = false;
			}
		}
	}

	private void HandleDataChanged()
	{
		IDataModelList lValue = UpdateDataModels();
		int a = lValue?.Count ?? 0;
		if (ShouldUseVirtualScrollingList)
		{
			SetVirtualScrollingDirty();
			m_cachedMetaDataModels = lValue;
		}
		int widgetAmountToUse = (ShouldUseVirtualScrollingList ? Mathf.Max(1, m_virtualAmountInBounds) : m_maxWidgetsToShow);
		int maxWidgetsToShow = Mathf.Min(a, widgetAmountToUse);
		bool isChangingStates = false;
		if (m_widgetItems.Count < maxWidgetsToShow)
		{
			isChangingStates = true;
		}
		else if (m_widgetItems.Count > maxWidgetsToShow)
		{
			isChangingStates = true;
		}
		CreateAndCleanupWidgetItems(maxWidgetsToShow);
		if (m_widgetItems.Count > 0)
		{
			if (!ShouldUseVirtualScrollingList)
			{
				BindWidgetDataModels(m_widgetItems, lValue?.Cast<IDataModel>(), maxWidgetsToShow);
			}
			else
			{
				BindVirtualScrollingDataModels(m_widgetItems, lValue?.Cast<IDataModel>().ToList(), maxWidgetsToShow);
			}
			foreach (WidgetInstance widget in m_widgetItems)
			{
				isChangingStates |= widget.IsChangingStates;
			}
		}
		if (isChangingStates)
		{
			HandleStartChangingStates();
		}
	}

	private IDataModelList UpdateDataModels()
	{
		ScriptContext.EvaluationResults valueResults = new ScriptContext().Evaluate(m_valueScript.Script, this);
		IDataModelList lValue = valueResults.Value as IDataModelList;
		ArrayList lValueArrayList = ((lValue == null) ? (valueResults.Value as ArrayList) : null);
		if (lValueArrayList != null)
		{
			lValue = new DataModelList<IDataModel>();
			foreach (object value in lValueArrayList)
			{
				lValue.Add(value as IDataModel);
			}
		}
		return lValue;
	}

	private void CreateAndCleanupWidgetItems(int widgetsToShow)
	{
		while (m_widgetItems.Count < widgetsToShow)
		{
			AddListItem();
		}
		while (m_widgetItems.Count > widgetsToShow)
		{
			RemoveListItem();
		}
	}

	private void BindWidgetDataModels(IEnumerable<Widget> widgets, IEnumerable<IDataModel> dataModels, int count)
	{
		FunctionalUtil.Zip(widgets, dataModels, Enumerable.Range(0, count), Enumerable.Repeat(count, count), BindWidget);
	}

	private void BindWidget(Widget widget, IDataModel dataModel, int index, int count)
	{
		if (!(widget == null))
		{
			if (m_widgetsDoneChangingStates.Contains(widget.name) && widget.GetDataModel(dataModel.DataModelId, out var model) && dataModel.GetPropertiesHashCode() != model.GetPropertiesHashCode())
			{
				m_widgetsDoneChangingStates.Remove(widget.name);
			}
			widget.BindDataModel(GetMetaDataModel(widget, index, count));
			if (dataModel != null)
			{
				widget.BindDataModel(dataModel);
			}
		}
	}

	private static void BindVirtualScrollingDataModels(List<WidgetInstance> widgetItems, List<IDataModel> dataModels, int firstVisible)
	{
		for (int i = 0; i < widgetItems.Count; i++)
		{
			int min = Math.Min(dataModels.Count - widgetItems.Count + i, firstVisible + i);
			int adjustedIndex = min % widgetItems.Count;
			if (adjustedIndex >= 0 && !(widgetItems[adjustedIndex].Widget == null))
			{
				widgetItems[adjustedIndex].BindDataModel(GetMetaDataModel(widgetItems[adjustedIndex], min, dataModels.Count));
				if (i < dataModels.Count && dataModels[i] != null)
				{
					widgetItems[adjustedIndex].BindDataModel(dataModels[min]);
				}
			}
		}
	}

	private void HandleDoneChangingStates(WidgetInstance itemWidget)
	{
		m_widgetsDoneChangingStates.Add(itemWidget.name);
		CheckIfDoneChangingStates();
		if (ShouldUseVirtualScrollingList)
		{
			HandleVirtualScrollingDoneChangingStates();
		}
	}

	private void HandleVirtualScrollingDoneChangingStates()
	{
		if (m_virtualScrollingInitialized || m_widgetItems.Count == 0 || m_widgetItems[0] == null)
		{
			return;
		}
		VisibilityProvider.RemoveFastVisibleAffectedObject(base.gameObject);
		m_widgetItems[0].RegisterDoneChangingStatesListener(delegate
		{
			if (m_widgetItems.Count != 0 && !(m_widgetItems[0] == null))
			{
				InitializeVirtualScrollingOnWidget(m_widgetItems[0]);
			}
		}, null, callImmediatelyIfSet: true, doOnce: true);
		m_virtualScrollingInitialized = true;
	}

	private void InitializeVirtualScrollingOnWidget(WidgetInstance widgetInstance)
	{
		if (!(widgetInstance.Widget == null))
		{
			Vector3 extents = WidgetTransform.GetWorldBoundsOfWidgetTransforms(widgetInstance.Widget.transform).extents;
			VisibilityProvider.AddFastVisibleAffectedObject(base.gameObject, extents, visible: false, m_virtualScrollingBuffer, OnVirtualScrollingVisible);
			SetVirtualScrollingDirty();
		}
	}

	private void SetVirtualScrollingDirty()
	{
		if (ShouldUseVirtualScrollingList)
		{
			m_virtualScrollingIsDirty = true;
		}
	}

	private void CheckIfDoneChangingStates()
	{
		if (m_widgetsDoneChangingStates.Count >= m_widgetItems.Count && !IsDirty)
		{
			if (!ShouldUseVirtualScrollingList)
			{
				HandleLayout();
				HandleVisibility();
			}
			else
			{
				SetVirtualScrollingDirty();
			}
			m_initialized = true;
			HandleDoneChangingStates();
		}
		_ = m_initialized;
	}

	private void HandleLayout()
	{
		Vector3 layoutDirection = GetLayoutDirection();
		if (ShouldUseVirtualScrollingList)
		{
			int listCount = m_cachedMetaDataModels?.Count ?? 0;
			Bounds localBounds = GetLargestBounds(local: true);
			if (m_widgetSizes.Count < listCount)
			{
				m_widgetSizes.AddRange(Enumerable.Repeat(localBounds.size, listCount - m_widgetSizes.Count));
			}
			else if (m_widgetSizes.Count > listCount)
			{
				m_widgetSizes.RemoveRange(listCount, m_widgetSizes.Count - listCount);
			}
			for (int i = 0; i < m_widgetItems.Count; i++)
			{
				int min = Math.Min(listCount - m_widgetItems.Count + i, m_firstVisible + i);
				int adjustedIndex = min % m_widgetItems.Count;
				Transform obj = m_widgetItems[adjustedIndex].transform;
				Bounds b = WidgetTransform.GetBoundsOfWidgetTransforms(obj, base.transform);
				m_widgetSizes[min] = b.size;
				float sum = GetSumAlongLayoutDirection(m_widgetSizes, 0, min, layoutDirection);
				Vector3 startPos = Vector3.Scale(b.extents, layoutDirection);
				if (min != 0)
				{
					startPos += layoutDirection * sum;
				}
				obj.localPosition = startPos;
			}
			Vector3 sizeInDirection = Vector3.Scale(localBounds.size * listCount, layoutDirection);
			float totalSize = sizeInDirection.magnitude;
			VirtualScrollingFiller.m_size = sizeInDirection;
			VirtualScrollingFiller.m_offset = sizeInDirection * 0.5f;
			VirtualScrollingFiller.transform.SetSiblingIndex(m_widgetItems.Count);
			Vector3 orientation = ((m_layoutMode == LayoutMode.Vertical) ? Vector3.right : Vector3.up);
			WidgetTransform obj2 = WidgetTransform.AddOrUpdateWidgetTransform(bounds: new Rect(Vector2.zero, Vector3.Scale(orientation, localBounds.size)), obj: VirtualScrollingFiller.gameObject);
			obj2.transform.localPosition = Vector3.Scale(localBounds.extents, -orientation);
			obj2.Bottom = 0f - totalSize;
			return;
		}
		float startOfNextItemInLayoutDirection = 0f;
		foreach (WidgetInstance widgetItem in m_widgetItems)
		{
			Bounds bounds2 = WidgetTransform.GetBoundsOfWidgetTransforms(widgetItem.transform, base.transform);
			Vector3 layoutPosition = GetNextLayoutPosition(bounds2, layoutDirection, ref startOfNextItemInLayoutDirection);
			widgetItem.transform.localPosition += layoutPosition;
		}
	}

	private float GetSumAlongLayoutDirection(List<Vector3> list, int startIndex, int endIndex, Vector3 layoutDirection)
	{
		float sum = 0f;
		for (int i = startIndex; i < endIndex; i++)
		{
			sum += Mathf.Abs(Vector3.Dot(list[i], layoutDirection)) + m_spacing;
		}
		return sum;
	}

	private void HandleVisibility()
	{
		foreach (WidgetInstance widget in m_widgetItems)
		{
			if (m_isVisible)
			{
				widget.Show();
				if (m_overrideLayer.HasValue)
				{
					widget.SetLayerOverride(m_overrideLayer.Value);
					m_isOverrideLayerApplied = true;
				}
			}
		}
	}

	private void OnVirtualScrollingVisible(int startIndex, int endIndex, bool isVisible)
	{
		using (m_visibilityUpdatedMarker.Auto())
		{
			if (isVisible)
			{
				if (m_widgetItems.Count != 0)
				{
					IDataModelList cachedMetaDataModels = m_cachedMetaDataModels;
					if (cachedMetaDataModels == null || cachedMetaDataModels.Count != 0)
					{
						goto IL_003f;
					}
				}
				m_cachedMetaDataModels = UpdateDataModels();
			}
			goto IL_003f;
			IL_003f:
			int listCount = m_cachedMetaDataModels?.Count ?? 0;
			int adjustedStartIndex = Mathf.Clamp(startIndex, 0, listCount - m_widgetItems.Count);
			int num = Mathf.Clamp(endIndex, adjustedStartIndex, listCount);
			int minimumAmount = Mathf.Max(m_widgetItems.Count, 1);
			int minOrListCount = Mathf.Min(listCount, minimumAmount);
			int desiredVirtualAmountInBounds = Mathf.Clamp(num - adjustedStartIndex, minOrListCount, listCount);
			if (listCount == 0)
			{
				desiredVirtualAmountInBounds = 0;
			}
			if (startIndex > listCount)
			{
				isVisible = false;
			}
			if (!m_virtualScrollingIsDirty && m_firstVisible == adjustedStartIndex && m_virtualAmountInBounds == desiredVirtualAmountInBounds && m_widgetItems.Count == desiredVirtualAmountInBounds && m_virtualScrollingVisible == isVisible)
			{
				return;
			}
			m_firstVisible = Mathf.Max(0, adjustedStartIndex);
			CreateAndCleanupWidgetItems(desiredVirtualAmountInBounds);
			BindVirtualScrollingDataModels(m_widgetItems, m_cachedMetaDataModels?.Cast<IDataModel>().ToList(), m_firstVisible);
			HandleLayout();
			HandleVisibility();
			foreach (WidgetInstance widgetItem in m_widgetItems)
			{
				if (widgetItem.IsChangingStates)
				{
					HandleStartChangingStates();
					break;
				}
			}
			m_virtualAmountInBounds = desiredVirtualAmountInBounds;
			m_virtualScrollingVisible = isVisible;
			m_virtualScrollingIsDirty = false;
			if (m_widgetItems.Count != 0)
			{
				Vector3 uniformExtent = GetLargestBounds(local: false).extents;
				VisibilityProvider.ChangeExtentsOnFastVisibleObject(base.gameObject, uniformExtent);
			}
		}
	}

	private Bounds GetLargestBounds(bool local)
	{
		Bounds largestBounds = default(Bounds);
		Vector3 layoutDirection = GetLayoutDirection();
		foreach (WidgetInstance widgetItem in m_widgetItems)
		{
			Transform widgetTransform = widgetItem.transform;
			Bounds extents = (local ? WidgetTransform.GetBoundsOfWidgetTransforms(widgetTransform, base.transform) : WidgetTransform.GetWorldBoundsOfWidgetTransforms(widgetTransform));
			if (Mathf.Abs(Vector3.Dot(extents.size, layoutDirection)) > Mathf.Abs(Vector3.Dot(largestBounds.size, layoutDirection)))
			{
				largestBounds = extents;
			}
		}
		return largestBounds;
	}

	private static ListableDataModel GetMetaDataModel(Widget widget, int i, int size)
	{
		ListableDataModel metaDataModel = null;
		metaDataModel = ((!widget.GetDataModel(258, out var dataModel)) ? new ListableDataModel() : (dataModel as ListableDataModel));
		metaDataModel.ItemIndex = i;
		metaDataModel.ListSize = size;
		metaDataModel.IsFirstItem = i == 0;
		metaDataModel.IsLastItem = i == size - 1;
		return metaDataModel;
	}

	private Vector3 GetNextLayoutPosition(Bounds bounds, Vector3 layoutDirection, ref float startOfNextItem)
	{
		float a = Vector3.Dot(bounds.min, layoutDirection);
		float maxInLayoutDirection = Vector3.Dot(bounds.max, layoutDirection);
		float startInLayoutDirection = Mathf.Min(a, maxInLayoutDirection);
		float deltaInLayoutDirection = startOfNextItem - startInLayoutDirection;
		float sizeInLayoutDirection = Mathf.Abs(Vector3.Dot(bounds.size, layoutDirection));
		startOfNextItem += sizeInLayoutDirection + m_spacing;
		return layoutDirection * deltaInLayoutDirection;
	}

	private Vector3 GetLayoutDirection()
	{
		if (m_cachedLayoutDirection.HasValue)
		{
			return m_cachedLayoutDirection.Value;
		}
		Vector3 direction = new Vector3(0f, -1f, 0f);
		LayoutMode layoutMode = m_layoutMode;
		if (layoutMode != 0 && layoutMode == LayoutMode.Horizontal)
		{
			direction = new Vector3(1f, 0f, 0f);
		}
		WidgetTransform widgetTransform = GetComponent<WidgetTransform>();
		direction = WidgetTransformUtils.GetRotationFromZNegativeToDesiredFacing((widgetTransform != null) ? widgetTransform.Facing : FacingDirection.YPositive) * direction;
		m_cachedLayoutDirection = direction;
		return direction;
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		if (ShouldUseVirtualScrollingList && VisibilityProvider is UIBScrollable.IContent scrollable)
		{
			scrollable.Scrollable.SetScrollSnap(0f);
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		if (ShouldUseVirtualScrollingList)
		{
			CleanupVirtualScrolling();
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (ShouldUseVirtualScrollingList)
		{
			CleanupVirtualScrolling();
		}
	}

	private void CleanupVirtualScrolling()
	{
		VisibilityProvider.RemoveFastVisibleAffectedObject(base.gameObject);
		m_virtualScrollingInitialized = false;
		m_virtualScrollingIsDirty = false;
		m_virtualScrollingVisible = false;
		m_firstVisible = -1;
		m_virtualAmountInBounds = 1;
		m_cachedMetaDataModels = null;
		m_lastDataVersion = 0;
		if (VisibilityProvider is UIBScrollable.IContent scrollable)
		{
			scrollable.Scrollable.SetScrollSnap(0f);
		}
		if (m_virtualScrollingFiller != null)
		{
			UnityEngine.Object.Destroy(m_virtualScrollingFiller.gameObject);
		}
	}

	[Conditional("UNITY_EDITOR")]
	private void Log(string message, string type)
	{
		Hearthstone.UI.Logging.Log.Get().AddMessage(message, this, LogLevel.Info, type);
	}
}
