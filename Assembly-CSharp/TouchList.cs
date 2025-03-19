using System;
using System.Collections.Generic;
using System.Linq;
using Blizzard.T5.Core;
using Blizzard.T5.Core.Utils;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class TouchList : PegUIElement
{
	public enum Orientation
	{
		Horizontal,
		Vertical
	}

	public enum Alignment
	{
		Min,
		Mid,
		Max
	}

	public enum LayoutPlane
	{
		XY,
		XZ
	}

	public delegate bool SelectedIndexChangingEvent(int index);

	public delegate void ScrollingEnabledChangedEvent(bool canScroll);

	public delegate bool ItemDragEvent(ITouchListItem item, float dragAmount);

	public interface ILongListBehavior
	{
		int AllItemsCount { get; }

		int MinBuffer { get; }

		int MaxAcquiredItems { get; }

		void ReleaseAllItems();

		void ReleaseItem(ITouchListItem item);

		ITouchListItem AcquireItem(int index);

		bool IsItemShowable(int allItemsIndex);

		Vector3 GetItemSize(int allItemsIndex);
	}

	private class ItemInfo
	{
		private readonly ITouchListItem item;

		public Vector3 Size { get; private set; }

		public Vector3 Offset { get; private set; }

		public int LongListIndex { get; set; }

		public Vector3 Min => item.transform.localPosition + Vector3.Scale(item.LocalBounds.min, VectorUtils.Abs(item.transform.localScale));

		public Vector3 Max => item.transform.localPosition + Vector3.Scale(item.LocalBounds.max, VectorUtils.Abs(item.transform.localScale));

		public ItemInfo(ITouchListItem item, LayoutPlane layoutPlane)
		{
			this.item = item;
			CalculateSizeAndOffset(layoutPlane);
		}

		public void CalculateSizeAndOffset(LayoutPlane layoutPlane, bool ignoreCurrentPosition = false)
		{
			Vector3 min = Vector3.Scale(item.LocalBounds.min, VectorUtils.Abs(item.transform.localScale));
			Vector3 max = Vector3.Scale(item.LocalBounds.max, VectorUtils.Abs(item.transform.localScale));
			if (!ignoreCurrentPosition)
			{
				min -= item.transform.localPosition;
				max -= item.transform.localPosition;
			}
			Size = max - min;
			Vector3 localOffset = min;
			if (layoutPlane == LayoutPlane.XZ)
			{
				localOffset.y = max.y;
			}
			Offset = -localOffset;
			if (!ignoreCurrentPosition)
			{
				Offset += item.transform.localPosition;
			}
		}

		public bool Contains(Vector3 point, LayoutPlane layoutPlane)
		{
			Vector3 min = Min;
			Vector3 max = Max;
			int verticalIndex = ((layoutPlane == LayoutPlane.XY) ? 1 : 2);
			if (point.x > min.x && point[verticalIndex] > min[verticalIndex] && point.x < max.x)
			{
				return point[verticalIndex] < max[verticalIndex];
			}
			return false;
		}
	}

	public Orientation orientation;

	public Alignment alignment = Alignment.Mid;

	public LayoutPlane layoutPlane;

	public float elementSpacing;

	public Vector2 padding = Vector2.zero;

	public int breadth = 1;

	public float itemDragFinishDistance;

	public TiledBackground background;

	public float scrollWheelIncrement = 30f;

	public Float_MobileOverride maxKineticScrollSpeed = new Float_MobileOverride();

	private GameObject content;

	private List<ITouchListItem> renderedItems = new List<ITouchListItem>();

	private Map<ITouchListItem, ItemInfo> itemInfos = new Map<ITouchListItem, ItemInfo>();

	private int layoutDimension1;

	private int layoutDimension2;

	private int layoutDimension3;

	private float contentSize;

	private float excessContentSize;

	private float m_fullListContentSize;

	private Vector2? touchBeginScreenPosition;

	private Vector3? dragBeginOffsetFromContent;

	private Vector3 dragBeginContentPosition = Vector3.zero;

	private Vector3 lastTouchPosition = Vector3.zero;

	private float lastContentPosition;

	private ITouchListItem touchBeginItem;

	private bool m_isHoveredOverTouchList;

	private PegUIElement m_hoveredOverItem;

	private ILongListBehavior longListBehavior;

	private bool allowModification = true;

	private Vector3? dragItemBegin;

	private bool layoutSuspended;

	private int? selection;

	private bool scrollEnabled = true;

	private const float ScrollDragThreshold = 0.05f;

	private const float ItemDragThreshold = 0.05f;

	private const float KineticScrollFriction = 10000f;

	private const float MinKineticScrollSpeed = 0.01f;

	private const float ScrollBoundsSpringK = 400f;

	private static readonly float ScrollBoundsSpringB = Mathf.Sqrt(1600f);

	private const float MinOutOfBoundsDistance = 0.05f;

	private static readonly Func<float, float> OutOfBoundsDistReducer = (float dist) => 30f * (Mathf.Log(dist + 30f) - Mathf.Log(30f));

	private const float CLIPSIZE_EPSILON = 0.0001f;

	public IEnumerable<ITouchListItem> RenderedItems
	{
		get
		{
			EnforceInitialized();
			return renderedItems;
		}
	}

	public bool IsReadOnly
	{
		get
		{
			EnforceInitialized();
			return false;
		}
	}

	public bool IsInitialized => content != null;

	public ILongListBehavior LongListBehavior
	{
		get
		{
			EnforceInitialized();
			return longListBehavior;
		}
		set
		{
			EnforceInitialized();
			if (value != longListBehavior)
			{
				allowModification = true;
				Clear();
				if (longListBehavior != null)
				{
					longListBehavior.ReleaseAllItems();
				}
				longListBehavior = value;
				if (longListBehavior != null)
				{
					RefreshList(0, preserveScrolling: false);
					allowModification = false;
				}
			}
		}
	}

	public float ScrollValue
	{
		get
		{
			EnforceInitialized();
			float scrollableAmount = ScrollableAmount;
			float result = ((scrollableAmount > 0f) ? Mathf.Clamp01((0f - content.transform.localPosition[layoutDimension1]) / scrollableAmount) : 0f);
			if (result == 0f || result == 1f)
			{
				return (0f - GetOutOfBoundsDist(content.transform.localPosition[layoutDimension1])) / Mathf.Max(contentSize, ClipSize[GetVector2Dimension(layoutDimension1)]) + result;
			}
			return result;
		}
		set
		{
			EnforceInitialized();
			if (!dragBeginOffsetFromContent.HasValue && !Mathf.Approximately(ScrollValue, value))
			{
				float scrollableAmount = ScrollableAmount;
				Vector3 localPosition = content.transform.localPosition;
				localPosition[layoutDimension1] = (0f - Mathf.Clamp01(value)) * scrollableAmount;
				content.transform.localPosition = localPosition;
				float posDelta = localPosition[layoutDimension1] - lastContentPosition;
				if (posDelta != 0f)
				{
					PreBufferLongListItems(posDelta < 0f);
				}
				lastContentPosition = localPosition[layoutDimension1];
				FixUpScrolling();
				OnScrolled();
			}
		}
	}

	public float ScrollableAmount
	{
		get
		{
			if (longListBehavior == null)
			{
				return excessContentSize;
			}
			return Mathf.Max(0f, m_fullListContentSize - ClipSize[GetVector2Dimension(layoutDimension1)]);
		}
	}

	public bool CanScrollAhead
	{
		get
		{
			if (!scrollEnabled)
			{
				return false;
			}
			if (ScrollValue < 1f)
			{
				return true;
			}
			if (longListBehavior != null && renderedItems.Count > 0)
			{
				ITouchListItem listItem = renderedItems.Last();
				for (int i = itemInfos[listItem].LongListIndex + 1; i < longListBehavior.AllItemsCount; i++)
				{
					if (longListBehavior.IsItemShowable(i))
					{
						return true;
					}
				}
			}
			return false;
		}
	}

	public bool CanScrollBehind
	{
		get
		{
			if (!scrollEnabled)
			{
				return false;
			}
			if (ScrollValue > 0f)
			{
				return true;
			}
			if (longListBehavior != null && renderedItems.Count > 0)
			{
				ITouchListItem listItem = renderedItems.First();
				ItemInfo info = itemInfos[listItem];
				if (longListBehavior.AllItemsCount > 0)
				{
					for (int i = info.LongListIndex - 1; i >= 0; i--)
					{
						if (longListBehavior.IsItemShowable(i))
						{
							return true;
						}
					}
				}
			}
			return false;
		}
	}

	public bool CanScroll
	{
		get
		{
			if (!CanScrollAhead)
			{
				return CanScrollBehind;
			}
			return true;
		}
	}

	public float ViewWindowMinValue
	{
		get
		{
			return (0f - content.transform.localPosition[layoutDimension1]) / contentSize;
		}
		set
		{
			Vector3 localPosition = content.transform.localPosition;
			localPosition[layoutDimension1] = (0f - Mathf.Clamp01(value)) * contentSize;
			content.transform.localPosition = localPosition;
			float posDelta = content.transform.localPosition[layoutDimension1] - lastContentPosition;
			if (posDelta != 0f)
			{
				PreBufferLongListItems(posDelta < 0f);
			}
			lastContentPosition = localPosition[layoutDimension1];
			OnScrolled();
		}
	}

	public float ViewWindowMaxValue
	{
		get
		{
			float num = content.transform.localPosition[layoutDimension1];
			float clipSize = ClipSize[GetVector2Dimension(layoutDimension1)];
			return (0f - num + clipSize) / contentSize;
		}
		set
		{
			Vector3 localPosition = content.transform.localPosition;
			localPosition[layoutDimension1] = (0f - Mathf.Clamp01(value)) * contentSize + ClipSize[GetVector2Dimension(layoutDimension1)];
			content.transform.localPosition = localPosition;
			float posDelta = content.transform.localPosition[layoutDimension1] - lastContentPosition;
			if (posDelta != 0f)
			{
				PreBufferLongListItems(posDelta < 0f);
			}
			lastContentPosition = localPosition[layoutDimension1];
			OnScrolled();
		}
	}

	public Vector2 ClipSize
	{
		get
		{
			EnforceInitialized();
			BoxCollider boxCollider = GetComponent<Collider>() as BoxCollider;
			return new Vector2(boxCollider.size.x, (layoutPlane == LayoutPlane.XY) ? boxCollider.size.y : boxCollider.size.z);
		}
		set
		{
			EnforceInitialized();
			BoxCollider boxCollider = GetComponent<Collider>() as BoxCollider;
			Vector3 size = new Vector3(value.x, 0f, 0f);
			size[1] = ((layoutPlane == LayoutPlane.XY) ? value.y : boxCollider.size.y);
			size[2] = ((layoutPlane == LayoutPlane.XZ) ? value.y : boxCollider.size.z);
			Vector3 deltaSize = VectorUtils.Abs(boxCollider.size - size);
			if (!(deltaSize.x <= 0.0001f) || !(deltaSize.y <= 0.0001f) || !(deltaSize.z <= 0.0001f))
			{
				boxCollider.size = size;
				UpdateBackgroundBounds();
				if (longListBehavior == null)
				{
					RepositionItems(0, null);
				}
				else
				{
					RefreshList(0, preserveScrolling: true);
				}
				if (this.ClipSizeChanged != null)
				{
					this.ClipSizeChanged();
				}
			}
		}
	}

	public bool SelectionEnabled
	{
		get
		{
			EnforceInitialized();
			return selection.HasValue;
		}
		set
		{
			EnforceInitialized();
			if (value != SelectionEnabled)
			{
				if (value)
				{
					selection = -1;
				}
				else
				{
					selection = null;
				}
			}
		}
	}

	public int SelectedIndex
	{
		get
		{
			EnforceInitialized();
			if (!selection.HasValue)
			{
				return -1;
			}
			return selection.Value;
		}
		set
		{
			EnforceInitialized();
			if (SelectionEnabled && value != selection && (this.SelectedIndexChanging == null || this.SelectedIndexChanging(value)))
			{
				ISelectableTouchListItem unselectingItem = SelectedItem as ISelectableTouchListItem;
				ISelectableTouchListItem selectingItem = ((value != -1) ? renderedItems[value] : null) as ISelectableTouchListItem;
				if (value == -1 || (selectingItem != null && selectingItem.Selectable))
				{
					selection = value;
				}
				if (unselectingItem != null && selection == value)
				{
					unselectingItem.Unselected();
				}
				if (selection == value && selectingItem != null)
				{
					selectingItem.Selected();
					ScrollToItem_Internal(selectingItem);
				}
			}
		}
	}

	public ITouchListItem SelectedItem
	{
		get
		{
			EnforceInitialized();
			if (!selection.HasValue || selection.Value == -1)
			{
				return null;
			}
			return renderedItems[selection.Value];
		}
		set
		{
			EnforceInitialized();
			int index = renderedItems.IndexOf(value);
			if (index != -1)
			{
				SelectedIndex = index;
			}
		}
	}

	public bool IsLayoutSuspended => layoutSuspended;

	public event Action Scrolled;

	public event SelectedIndexChangingEvent SelectedIndexChanging;

	public event ScrollingEnabledChangedEvent ScrollingEnabledChanged;

	public event Action ClipSizeChanged;

	public event ItemDragEvent ItemDragStarted;

	public event ItemDragEvent ItemDragged;

	public event ItemDragEvent ItemDragFinished;

	private void FixUpScrolling()
	{
		if (longListBehavior == null || renderedItems.Count <= 0 || !CanScroll)
		{
			return;
		}
		Bounds localClipBounds = CalculateLocalClipBounds();
		ITouchListItem item = renderedItems[0];
		ItemInfo info = itemInfos[item];
		if (info.LongListIndex == 0 && !CanScrollBehind)
		{
			float clipMin = localClipBounds.min[layoutDimension1];
			Vector3 itemMin = info.Min;
			if (Mathf.Abs(itemMin[layoutDimension1] - clipMin) > 0.0001f)
			{
				Vector3 delta = Vector3.zero;
				delta[layoutDimension1] = clipMin - itemMin[layoutDimension1];
				delta /= 4f;
				for (int i = 0; i < renderedItems.Count; i++)
				{
					item = renderedItems[i];
					item.gameObject.transform.Translate(delta);
				}
			}
		}
		else
		{
			if (renderedItems.Count <= 1 || CanScrollAhead)
			{
				return;
			}
			float clipMax = localClipBounds.max[layoutDimension1];
			item = renderedItems[renderedItems.Count - 1];
			info = itemInfos[item];
			if (info.LongListIndex < longListBehavior.AllItemsCount - 1)
			{
				return;
			}
			Vector3 itemMax = info.Max;
			if (Mathf.Abs(itemMax[layoutDimension1] - clipMax) > 0.0001f)
			{
				Vector3 delta2 = Vector3.zero;
				delta2[layoutDimension1] = clipMax - itemMax[layoutDimension1];
				delta2 /= 4f;
				for (int j = 0; j < renderedItems.Count; j++)
				{
					item = renderedItems[j];
					item.gameObject.transform.Translate(delta2);
				}
			}
		}
	}

	public void Add(ITouchListItem item)
	{
		Add(item, repositionItems: true);
	}

	public void Add(ITouchListItem item, bool repositionItems)
	{
		EnforceInitialized();
		if (allowModification)
		{
			renderedItems.Add(item);
			Vector3 negatedScale = GetNegatedScale(item.transform.localScale);
			item.transform.parent = content.transform;
			item.transform.localPosition = Vector3.zero;
			item.transform.localRotation = Quaternion.identity;
			if (orientation == Orientation.Vertical)
			{
				item.transform.localScale = negatedScale;
			}
			itemInfos[item] = new ItemInfo(item, layoutPlane);
			item.gameObject.SetActive(value: false);
			if (selection == -1 && item is ISelectableTouchListItem && ((ISelectableTouchListItem)item).IsSelected())
			{
				selection = renderedItems.Count - 1;
			}
			if (repositionItems)
			{
				RepositionItems(renderedItems.Count - 1, null);
				RecalculateLongListContentSize();
			}
		}
	}

	public void Clear()
	{
		EnforceInitialized();
		if (!allowModification)
		{
			return;
		}
		foreach (ITouchListItem item in renderedItems)
		{
			Vector3 negatedScale = GetNegatedScale(item.transform.localScale);
			item.transform.parent = null;
			if (orientation == Orientation.Vertical)
			{
				item.transform.localScale = negatedScale;
			}
		}
		content.transform.localPosition = Vector3.zero;
		lastContentPosition = 0f;
		renderedItems.Clear();
		RecalculateSize();
		UpdateBackgroundScroll();
		RecalculateLongListContentSize();
		if (SelectionEnabled)
		{
			SelectedIndex = -1;
		}
		if (m_hoveredOverItem != null)
		{
			m_hoveredOverItem.TriggerOut();
			m_hoveredOverItem = null;
		}
	}

	public bool Contains(ITouchListItem item)
	{
		EnforceInitialized();
		return renderedItems.Contains(item);
	}

	public void CopyTo(ITouchListItem[] array, int arrayIndex)
	{
		EnforceInitialized();
		renderedItems.CopyTo(array, arrayIndex);
	}

	private List<ITouchListItem> GetItemsInView()
	{
		EnforceInitialized();
		List<ITouchListItem> result = new List<ITouchListItem>();
		float clipMax = CalculateLocalClipBounds().max[layoutDimension1];
		for (int i = GetNumItemsBehindView(); i < renderedItems.Count && !((itemInfos[renderedItems[i]].Min - content.transform.localPosition)[layoutDimension1] >= clipMax); i++)
		{
			result.Add(renderedItems[i]);
		}
		return result;
	}

	public void SetVisibilityOfAllItems()
	{
		if (layoutSuspended)
		{
			return;
		}
		EnforceInitialized();
		Bounds localClipBounds = CalculateLocalClipBounds();
		for (int i = 0; i < renderedItems.Count; i++)
		{
			ITouchListItem item = renderedItems[i];
			bool visible = IsItemVisible_Internal(i, ref localClipBounds);
			if (visible != item.gameObject.activeSelf)
			{
				item.gameObject.SetActive(visible);
				if (!visible)
				{
					item.OnScrollOutOfView();
				}
			}
		}
	}

	private bool IsItemVisible_Internal(int visualizedListIndex, ref Bounds localClipBounds)
	{
		ITouchListItem item = renderedItems[visualizedListIndex];
		ItemInfo itemInfo = itemInfos[item];
		Vector3 itemMin = itemInfo.Min;
		Vector3 itemMax = itemInfo.Max;
		if (!IsWithinClipBounds(itemMin, itemMax, ref localClipBounds))
		{
			return false;
		}
		return true;
	}

	private bool IsWithinClipBounds(Vector3 localBoundsMin, Vector3 localBoundsMax, ref Bounds localClipBounds)
	{
		float clipMin = localClipBounds.min[layoutDimension1];
		float clipMax = localClipBounds.max[layoutDimension1];
		if (localBoundsMax[layoutDimension1] < clipMin)
		{
			return false;
		}
		if (localBoundsMin[layoutDimension1] > clipMax)
		{
			return false;
		}
		return true;
	}

	private bool IsItemVisible(int visualizedListIndex)
	{
		Bounds localClipBounds = CalculateLocalClipBounds();
		return IsItemVisible_Internal(visualizedListIndex, ref localClipBounds);
	}

	public int IndexOf(ITouchListItem item)
	{
		EnforceInitialized();
		return renderedItems.IndexOf(item);
	}

	public void Insert(int index, ITouchListItem item)
	{
		Insert(index, item, repositionItems: true);
	}

	public void Insert(int index, ITouchListItem item, bool repositionItems)
	{
		EnforceInitialized();
		if (allowModification)
		{
			renderedItems.Insert(index, item);
			Vector3 negatedScale = GetNegatedScale(item.transform.localScale);
			item.transform.parent = content.transform;
			item.transform.localPosition = Vector3.zero;
			item.transform.localRotation = Quaternion.identity;
			if (orientation == Orientation.Vertical)
			{
				item.transform.localScale = negatedScale;
			}
			itemInfos[item] = new ItemInfo(item, layoutPlane);
			if (selection == -1 && item is ISelectableTouchListItem && ((ISelectableTouchListItem)item).IsSelected())
			{
				selection = index;
			}
			if (repositionItems)
			{
				RepositionItems(index, null);
				RecalculateLongListContentSize();
			}
		}
	}

	public bool Remove(ITouchListItem item)
	{
		EnforceInitialized();
		if (!allowModification)
		{
			return false;
		}
		int index = renderedItems.IndexOf(item);
		if (index != -1)
		{
			RemoveAt(index, repositionItems: true);
			return true;
		}
		return false;
	}

	public void RemoveAt(int index)
	{
		RemoveAt(index, repositionItems: true);
	}

	public void RemoveAt(int index, bool repositionItems)
	{
		EnforceInitialized();
		if (allowModification)
		{
			Vector3 negatedScale = GetNegatedScale(renderedItems[index].transform.localScale);
			ITouchListItem item = renderedItems[index];
			item.transform.parent = base.transform;
			if (orientation == Orientation.Vertical)
			{
				renderedItems[index].transform.localScale = negatedScale;
			}
			itemInfos.Remove(renderedItems[index]);
			renderedItems.RemoveAt(index);
			if (index == selection)
			{
				selection = -1;
			}
			else if (index < selection)
			{
				selection--;
			}
			if (m_hoveredOverItem != null && item.GetComponent<PegUIElement>() == m_hoveredOverItem)
			{
				m_hoveredOverItem.TriggerOut();
				m_hoveredOverItem = null;
			}
			if (repositionItems)
			{
				RepositionItems(index, null);
				RecalculateLongListContentSize();
			}
		}
	}

	public int FindIndex(Predicate<ITouchListItem> match)
	{
		EnforceInitialized();
		return renderedItems.FindIndex(match);
	}

	public void Sort(Comparison<ITouchListItem> comparison)
	{
		EnforceInitialized();
		ITouchListItem selectedItem = SelectedItem;
		renderedItems.Sort(comparison);
		RepositionItems(0, null);
		selection = renderedItems.IndexOf(selectedItem);
	}

	public void SuspendLayout()
	{
		EnforceInitialized();
		layoutSuspended = true;
	}

	public void ResumeLayout(bool repositionItems = true)
	{
		EnforceInitialized();
		layoutSuspended = false;
		if (repositionItems)
		{
			RepositionItems(0, null);
		}
	}

	public void ResetState()
	{
		touchBeginScreenPosition = null;
		dragBeginOffsetFromContent = null;
		dragBeginContentPosition = Vector3.zero;
		lastTouchPosition = Vector3.zero;
		lastContentPosition = 0f;
		dragItemBegin = null;
		if (content != null)
		{
			content.transform.localPosition = Vector3.zero;
		}
	}

	public void SetScrollingEnabled(bool enable)
	{
		scrollEnabled = enable;
		OnScrollingEnabledChanged();
	}

	public void ScrollToItem(ITouchListItem item)
	{
		ScrollToItem_Internal(item);
	}

	protected override void Awake()
	{
		base.Awake();
		content = new GameObject("Content");
		content.transform.parent = base.transform;
		TransformUtil.Identity(content.transform);
		layoutDimension1 = 0;
		layoutDimension2 = ((layoutPlane == LayoutPlane.XY) ? 1 : 2);
		layoutDimension3 = 3 - layoutDimension2;
		if (orientation == Orientation.Vertical)
		{
			GeneralUtils.Swap(ref layoutDimension1, ref layoutDimension2);
			Vector3 localScale = Vector3.one;
			localScale[layoutDimension1] = -1f;
			base.transform.localScale = localScale;
		}
		if (background != null)
		{
			if (orientation == Orientation.Vertical)
			{
				background.transform.localScale = GetNegatedScale(background.transform.localScale);
			}
			UpdateBackgroundBounds();
		}
	}

	protected override void OnOver(InteractionState oldState)
	{
		m_isHoveredOverTouchList = true;
		OnHover(isKnownOver: true);
	}

	protected override void OnOut(InteractionState oldState)
	{
		m_isHoveredOverTouchList = false;
		if (m_hoveredOverItem != null)
		{
			m_hoveredOverItem.TriggerOut();
			m_hoveredOverItem = null;
		}
	}

	private void OnHover(bool isKnownOver)
	{
		if (UniversalInputManager.Get().IsTouchMode())
		{
			return;
		}
		if (!isKnownOver && (!PegUI.Get().FindHitElement(out var hitInfo) || hitInfo.transform != base.transform) && m_hoveredOverItem != null)
		{
			m_hoveredOverItem.TriggerOut();
			m_hoveredOverItem = null;
		}
		Collider component = GetComponent<Collider>();
		component.enabled = false;
		PegUIElement element = GetChildHitElement(out hitInfo);
		component.enabled = true;
		if (m_hoveredOverItem != element)
		{
			if (m_hoveredOverItem != null)
			{
				m_hoveredOverItem.TriggerOut();
			}
			if (element != null)
			{
				element.TriggerOver();
			}
			m_hoveredOverItem = element;
		}
	}

	protected override void OnPress()
	{
		touchBeginScreenPosition = InputCollection.GetMousePosition();
		if (lastContentPosition != content.transform.localPosition[layoutDimension1])
		{
			return;
		}
		Vector3 touchContentPos = GetTouchPosition() - content.transform.localPosition;
		for (int i = 0; i < renderedItems.Count; i++)
		{
			ITouchListItem item = renderedItems[i];
			if ((item.IsHeader || item.Visible) && itemInfos[item].Contains(touchContentPos, layoutPlane))
			{
				touchBeginItem = item;
				break;
			}
		}
		Collider component = GetComponent<Collider>();
		component.enabled = false;
		RaycastHit hitInfo;
		PegUIElement element = GetChildHitElement(out hitInfo);
		component.enabled = true;
		if (element != null)
		{
			element.TriggerPress();
		}
	}

	protected override void OnRelease()
	{
		if (touchBeginItem != null && !dragItemBegin.HasValue)
		{
			touchBeginScreenPosition = null;
			Collider component = GetComponent<Collider>();
			component.enabled = false;
			RaycastHit hitInfo;
			PegUIElement element = GetChildHitElement(out hitInfo);
			component.enabled = true;
			if (element != null)
			{
				element.TriggerRelease();
				touchBeginItem = null;
			}
		}
	}

	private void EnforceInitialized()
	{
		if (!IsInitialized)
		{
			throw new InvalidOperationException("TouchList must be initialized before using it. Please wait for Awake to finish.");
		}
	}

	private void Update()
	{
		if (UniversalInputManager.Get().IsTouchMode())
		{
			UpdateTouchInput();
		}
		else
		{
			UpdateMouseInput();
		}
		if (m_isHoveredOverTouchList)
		{
			OnHover(isKnownOver: false);
		}
	}

	private void UpdateTouchInput()
	{
		Vector3 touchPosition = GetTouchPosition();
		if (InputCollection.GetMouseButtonUp(0))
		{
			if (dragItemBegin.HasValue && this.ItemDragFinished != null)
			{
				this.ItemDragFinished(touchBeginItem, GetItemDragDelta(touchPosition));
				dragItemBegin = null;
			}
			touchBeginItem = null;
			touchBeginScreenPosition = null;
			dragBeginOffsetFromContent = null;
		}
		if (touchBeginScreenPosition.HasValue)
		{
			Func<int, float, bool> testThreshold = delegate(int dimension, float inchThreshold)
			{
				int vector2Dimension = GetVector2Dimension(dimension);
				float f = touchBeginScreenPosition.Value[vector2Dimension] - InputCollection.GetMousePosition()[vector2Dimension];
				float num = inchThreshold * ((Screen.dpi > 0f) ? Screen.dpi : 96f);
				return Mathf.Abs(f) > num;
			};
			if (this.ItemDragStarted != null && testThreshold(layoutDimension2, 0.05f) && this.ItemDragStarted(touchBeginItem, GetItemDragDelta(touchPosition)))
			{
				dragItemBegin = GetTouchPosition();
				touchBeginScreenPosition = null;
			}
			else if (testThreshold(layoutDimension1, 0.05f))
			{
				dragBeginContentPosition = content.transform.localPosition;
				dragBeginOffsetFromContent = dragBeginContentPosition - lastTouchPosition;
				touchBeginItem = null;
				touchBeginScreenPosition = null;
			}
		}
		float posDelta;
		if (dragItemBegin.HasValue)
		{
			if (!this.ItemDragged(touchBeginItem, GetItemDragDelta(touchPosition)))
			{
				dragItemBegin = null;
				touchBeginItem = null;
			}
		}
		else if (dragBeginOffsetFromContent.HasValue)
		{
			float contentPosition = touchPosition[layoutDimension1] + dragBeginOffsetFromContent.Value[layoutDimension1];
			float outOfBoundsDist = GetOutOfBoundsDist(contentPosition);
			if (outOfBoundsDist != 0f)
			{
				outOfBoundsDist = OutOfBoundsDistReducer(Mathf.Abs(outOfBoundsDist)) * Mathf.Sign(outOfBoundsDist);
				contentPosition = ((outOfBoundsDist < 0f) ? (0f - excessContentSize + outOfBoundsDist) : outOfBoundsDist);
			}
			Vector3 position = content.transform.localPosition;
			lastContentPosition = position[layoutDimension1];
			position[layoutDimension1] = contentPosition;
			content.transform.localPosition = position;
			if (lastContentPosition != position[layoutDimension1])
			{
				OnScrolled();
			}
		}
		else
		{
			float contentPosition2 = content.transform.localPosition[layoutDimension1];
			float outOfBoundsDist2 = GetOutOfBoundsDist(contentPosition2);
			posDelta = content.transform.localPosition[layoutDimension1] - lastContentPosition;
			float lastVelocity = posDelta / Time.fixedDeltaTime;
			if ((float)maxKineticScrollSpeed > Mathf.Epsilon)
			{
				lastVelocity = ((!(lastVelocity > 0f)) ? Mathf.Max(lastVelocity, 0f - (float)maxKineticScrollSpeed) : Mathf.Min(lastVelocity, maxKineticScrollSpeed));
			}
			if (outOfBoundsDist2 != 0f)
			{
				Vector3 nextPos = content.transform.localPosition;
				lastContentPosition = contentPosition2;
				float acceleration = -400f * outOfBoundsDist2 - ScrollBoundsSpringB * lastVelocity;
				float nextVelocity = lastVelocity + acceleration * Time.fixedDeltaTime;
				nextPos[layoutDimension1] += nextVelocity * Time.fixedDeltaTime;
				if (Mathf.Abs(GetOutOfBoundsDist(nextPos[layoutDimension1])) < 0.05f)
				{
					float boundary = ((Mathf.Abs(nextPos[layoutDimension1] + excessContentSize) < Mathf.Abs(nextPos[layoutDimension1])) ? (0f - excessContentSize) : 0f);
					nextPos[layoutDimension1] = boundary;
					lastContentPosition = boundary;
				}
				content.transform.localPosition = nextPos;
				OnScrolled();
			}
			else if (lastVelocity != 0f)
			{
				lastContentPosition = content.transform.localPosition[layoutDimension1];
				float acceleration2 = (0f - Mathf.Sign(lastVelocity)) * 10000f;
				float nextVelocity2 = lastVelocity + acceleration2 * Time.fixedDeltaTime;
				if (Mathf.Abs(nextVelocity2) >= 0.01f && Mathf.Sign(nextVelocity2) == Mathf.Sign(lastVelocity))
				{
					Vector3 nextPos2 = content.transform.localPosition;
					nextPos2[layoutDimension1] += nextVelocity2 * Time.fixedDeltaTime;
					content.transform.localPosition = nextPos2;
					OnScrolled();
				}
			}
			else
			{
				FixUpScrolling();
			}
		}
		posDelta = content.transform.localPosition[layoutDimension1] - lastContentPosition;
		if (posDelta != 0f)
		{
			PreBufferLongListItems(posDelta < 0f);
		}
		lastTouchPosition = touchPosition;
	}

	private void PreBufferLongListItems(bool scrolledAhead)
	{
		if (LongListBehavior == null)
		{
			return;
		}
		allowModification = true;
		if (scrolledAhead && GetNumItemsAheadOfView() < longListBehavior.MinBuffer)
		{
			bool loadAhead = CanScrollAhead;
			if (renderedItems.Count > 0)
			{
				Bounds localClipBounds = CalculateLocalClipBounds();
				ITouchListItem item = renderedItems[renderedItems.Count - 1];
				Vector3 itemMax = itemInfos[item].Max;
				float clipMin = localClipBounds.min[layoutDimension1];
				if (itemMax[layoutDimension1] < clipMin)
				{
					RefreshList(0, preserveScrolling: true);
					loadAhead = false;
				}
			}
			if (loadAhead)
			{
				LoadAhead();
			}
		}
		else if (!scrolledAhead && GetNumItemsBehindView() < longListBehavior.MinBuffer)
		{
			bool loadBehind = CanScrollBehind;
			if (renderedItems.Count > 0)
			{
				Bounds localClipBounds2 = CalculateLocalClipBounds();
				ITouchListItem item2 = renderedItems[0];
				Vector3 itemMin = itemInfos[item2].Min;
				float clipMax = localClipBounds2.max[layoutDimension1];
				if (itemMin[layoutDimension1] > clipMax)
				{
					RefreshList(0, preserveScrolling: true);
					loadBehind = false;
				}
			}
			if (loadBehind)
			{
				LoadBehind();
			}
		}
		allowModification = false;
	}

	private void UpdateMouseInput()
	{
		Camera camera = CameraUtils.FindFirstByLayer(base.gameObject.layer);
		if (camera == null)
		{
			return;
		}
		Ray mouseRay = camera.ScreenPointToRay(InputCollection.GetMousePosition());
		if (!GetComponent<Collider>().Raycast(mouseRay, out var _, camera.farClipPlane))
		{
			return;
		}
		float scrollDelta = 0f;
		if (Input.GetAxis("Mouse ScrollWheel") < 0f && CanScrollAhead)
		{
			scrollDelta -= scrollWheelIncrement;
		}
		if (Input.GetAxis("Mouse ScrollWheel") > 0f && CanScrollBehind)
		{
			scrollDelta += scrollWheelIncrement;
		}
		if (Mathf.Abs(scrollDelta) > Mathf.Epsilon)
		{
			float contentPosition = content.transform.localPosition[layoutDimension1] + scrollDelta;
			if (contentPosition <= 0f - excessContentSize)
			{
				contentPosition = 0f - excessContentSize;
			}
			else if (contentPosition >= 0f)
			{
				contentPosition = 0f;
			}
			Vector3 position = content.transform.localPosition;
			lastContentPosition = position[layoutDimension1];
			position[layoutDimension1] = contentPosition;
			content.transform.localPosition = position;
			float posDelta = content.transform.localPosition[layoutDimension1] - lastContentPosition;
			lastContentPosition = content.transform.localPosition[layoutDimension1];
			if (posDelta != 0f)
			{
				PreBufferLongListItems(posDelta < 0f);
			}
			FixUpScrolling();
			OnScrolled();
		}
	}

	private float GetOutOfBoundsDist(float contentPosition)
	{
		float outOfBoundsDist = 0f;
		if (contentPosition < 0f - excessContentSize)
		{
			outOfBoundsDist = contentPosition + excessContentSize;
		}
		else if (contentPosition > 0f)
		{
			outOfBoundsDist = contentPosition;
		}
		return outOfBoundsDist;
	}

	private void ScrollToItem_Internal(ITouchListItem item)
	{
		Bounds clipBounds = CalculateLocalClipBounds();
		ItemInfo itemInfo = itemInfos[item];
		float distPastMax = itemInfo.Max[layoutDimension1] - clipBounds.max[layoutDimension1];
		if (distPastMax > 0f)
		{
			Vector3 translation = Vector3.zero;
			translation[layoutDimension1] = distPastMax;
			content.transform.Translate(translation);
			lastContentPosition = content.transform.localPosition[layoutDimension1];
			PreBufferLongListItems(scrolledAhead: true);
			OnScrolled();
		}
		float distPastMin = clipBounds.min[layoutDimension1] - itemInfo.Min[layoutDimension1];
		if (distPastMin > 0f)
		{
			Vector3 translation2 = Vector3.zero;
			translation2[layoutDimension1] = 0f - distPastMin;
			content.transform.Translate(translation2);
			lastContentPosition = content.transform.localPosition[layoutDimension1];
			PreBufferLongListItems(scrolledAhead: false);
			OnScrolled();
		}
	}

	private void OnScrolled()
	{
		UpdateBackgroundScroll();
		SetVisibilityOfAllItems();
		if (this.Scrolled != null)
		{
			this.Scrolled();
		}
	}

	private Vector3 GetTouchPosition()
	{
		Camera camera = CameraUtils.FindFirstByLayer(base.gameObject.layer);
		if (camera == null)
		{
			return Vector3.zero;
		}
		Bounds bounds = GetComponent<Collider>().bounds;
		Transform cameraTransform = camera.transform;
		float num = Vector3.Distance(cameraTransform.position, bounds.min);
		float maxDist = Vector3.Distance(cameraTransform.position, bounds.max);
		Vector3 planePoint = ((num < maxDist) ? bounds.min : bounds.max);
		Plane plane = new Plane(-cameraTransform.forward, planePoint);
		Ray ray = camera.ScreenPointToRay(InputCollection.GetMousePosition());
		plane.Raycast(ray, out var distance);
		return base.transform.InverseTransformPoint(ray.GetPoint(distance));
	}

	private float GetItemDragDelta(Vector3 touchPosition)
	{
		if (dragItemBegin.HasValue)
		{
			return touchPosition[layoutDimension2] - dragItemBegin.Value[layoutDimension2];
		}
		return 0f;
	}

	private void LoadAhead()
	{
		bool prevAllowModification = allowModification;
		bool prevSuspendLayout = layoutSuspended;
		allowModification = true;
		int repositionStartingIndex = -1;
		int countAdded = 0;
		int itemsBehind = GetNumItemsBehindView();
		for (int i = 0; i < itemsBehind - longListBehavior.MinBuffer; i++)
		{
			ITouchListItem removedItem = renderedItems[0];
			RemoveAt(0, repositionItems: false);
			longListBehavior.ReleaseItem(removedItem);
		}
		float clipMax = CalculateLocalClipBounds().max[layoutDimension1];
		int countAddedAhead = 0;
		for (int indexToAcquire = ((renderedItems.Count != 0) ? (itemInfos[renderedItems.Last()].LongListIndex + 1) : 0); indexToAcquire < longListBehavior.AllItemsCount; indexToAcquire++)
		{
			if (renderedItems.Count >= longListBehavior.MaxAcquiredItems)
			{
				break;
			}
			if (countAddedAhead >= longListBehavior.MinBuffer)
			{
				break;
			}
			if (longListBehavior.IsItemShowable(indexToAcquire))
			{
				if (repositionStartingIndex < 0)
				{
					repositionStartingIndex = renderedItems.Count;
				}
				ITouchListItem newItem = longListBehavior.AcquireItem(indexToAcquire);
				Add(newItem, repositionItems: false);
				ItemInfo itemInfo = itemInfos[newItem];
				itemInfo.LongListIndex = indexToAcquire;
				countAdded++;
				if (itemInfo.Min[layoutDimension1] > clipMax)
				{
					countAddedAhead++;
				}
			}
		}
		if (repositionStartingIndex >= 0)
		{
			layoutSuspended = false;
			RepositionItems(repositionStartingIndex, null);
		}
		allowModification = prevAllowModification;
		if (prevSuspendLayout != layoutSuspended)
		{
			layoutSuspended = prevSuspendLayout;
		}
	}

	private void LoadBehind()
	{
		bool prevAllowModification = allowModification;
		allowModification = true;
		int countAdded = 0;
		int itemsAhead = GetNumItemsAheadOfView();
		for (int i = 0; i < itemsAhead - longListBehavior.MinBuffer; i++)
		{
			ITouchListItem removedItem = renderedItems[renderedItems.Count - 1];
			RemoveAt(renderedItems.Count - 1, repositionItems: false);
			longListBehavior.ReleaseItem(removedItem);
		}
		float clipMin = CalculateLocalClipBounds().min[layoutDimension1];
		int countAddedBehind = 0;
		int indexToAcquire = ((renderedItems.Count == 0) ? (longListBehavior.AllItemsCount - 1) : (itemInfos[renderedItems.First()].LongListIndex - 1));
		while (indexToAcquire >= 0 && renderedItems.Count < longListBehavior.MaxAcquiredItems && countAddedBehind < longListBehavior.MinBuffer)
		{
			if (longListBehavior.IsItemShowable(indexToAcquire))
			{
				ITouchListItem newItem = longListBehavior.AcquireItem(indexToAcquire);
				InsertAndPositionBehind(newItem, indexToAcquire);
				ItemInfo itemInfo = itemInfos[newItem];
				itemInfo.LongListIndex = indexToAcquire;
				countAdded++;
				if (itemInfo.Max[layoutDimension1] < clipMin)
				{
					countAddedBehind++;
				}
			}
			indexToAcquire--;
		}
		allowModification = prevAllowModification;
	}

	private int GetNumItemsBehindView()
	{
		float clipMin = CalculateLocalClipBounds().min[layoutDimension1];
		for (int i = 0; i < renderedItems.Count; i++)
		{
			ITouchListItem listItem = renderedItems[i];
			if (itemInfos[listItem].Max[layoutDimension1] > clipMin)
			{
				return i;
			}
		}
		return renderedItems.Count;
	}

	private int GetNumItemsAheadOfView()
	{
		float clipMax = CalculateLocalClipBounds().max[layoutDimension1];
		for (int i = renderedItems.Count - 1; i >= 0; i--)
		{
			ITouchListItem listItem = renderedItems[i];
			if (itemInfos[listItem].Min[layoutDimension1] < clipMax)
			{
				return renderedItems.Count - 1 - i;
			}
		}
		return renderedItems.Count;
	}

	public void RefreshList(int startingLongListIndex, bool preserveScrolling)
	{
		if (longListBehavior == null)
		{
			return;
		}
		bool prevAllowModification = allowModification;
		allowModification = true;
		int selectedLongListItem = ((SelectedItem == null) ? (-1) : itemInfos[SelectedItem].LongListIndex);
		int lastIndexLessThanStarting = -2;
		int visualizedListIndex = -1;
		if (startingLongListIndex > 0)
		{
			for (int i = 0; i < renderedItems.Count; i++)
			{
				ITouchListItem item = renderedItems[i];
				if (itemInfos[item].LongListIndex < startingLongListIndex)
				{
					lastIndexLessThanStarting = i;
					continue;
				}
				visualizedListIndex = i;
				break;
			}
		}
		else
		{
			visualizedListIndex = 0;
		}
		int indexToRemove = ((visualizedListIndex == -1) ? (lastIndexLessThanStarting + 1) : visualizedListIndex);
		Bounds globalClipBounds = GetComponent<Collider>().bounds;
		Vector3? initialItemPosition = null;
		Vector3 nextItemPosition = Vector3.zero;
		int axisDirection = ((orientation != Orientation.Vertical) ? 1 : (-1));
		if (preserveScrolling)
		{
			nextItemPosition = content.transform.position;
			nextItemPosition[layoutDimension1] -= (float)axisDirection * globalClipBounds.extents[layoutDimension1];
			nextItemPosition[layoutDimension1] += (float)axisDirection * padding[GetVector2Dimension(layoutDimension1)];
			nextItemPosition[layoutDimension2] = globalClipBounds.center[layoutDimension2];
			nextItemPosition[layoutDimension3] = globalClipBounds.center[layoutDimension3];
			Vector3 contentPos = content.transform.localPosition;
			content.transform.localPosition = Vector3.zero;
			Bounds localClipBounds = CalculateLocalClipBounds();
			Vector3 initialPos = localClipBounds.min;
			initialPos[layoutDimension1] = 0f - contentPos[layoutDimension1] + localClipBounds.min[layoutDimension1];
			content.transform.localPosition = contentPos;
			initialItemPosition = initialPos;
			if (lastIndexLessThanStarting >= 0)
			{
				ITouchListItem item2 = renderedItems[lastIndexLessThanStarting];
				ItemInfo info = itemInfos[item2];
				nextItemPosition = item2.transform.position - info.Offset;
				nextItemPosition[layoutDimension1] += (float)axisDirection * elementSpacing;
				ITouchListItem firstItem = renderedItems[0];
				ItemInfo firstInfo = itemInfos[firstItem];
				initialItemPosition = firstItem.transform.localPosition - firstInfo.Offset;
			}
		}
		int countRemoved = 0;
		if (indexToRemove >= 0)
		{
			for (int i2 = renderedItems.Count - 1; i2 >= indexToRemove; i2--)
			{
				countRemoved++;
				ITouchListItem removedItem = renderedItems[i2];
				RemoveAt(i2, repositionItems: false);
				longListBehavior.ReleaseItem(removedItem);
			}
		}
		if (visualizedListIndex < 0)
		{
			visualizedListIndex = lastIndexLessThanStarting + 1;
			if (visualizedListIndex < 0)
			{
				visualizedListIndex = 0;
			}
		}
		int countAdded = 0;
		Vector3 itemSizeScaleFactor = new Vector3(Math.Abs(content.transform.lossyScale.x), Math.Abs(content.transform.lossyScale.y), Math.Abs(content.transform.lossyScale.z));
		float scaledElementSpacing = elementSpacing * Math.Abs(content.transform.lossyScale[layoutDimension1]);
		for (int j = startingLongListIndex; j < longListBehavior.AllItemsCount; j++)
		{
			if (renderedItems.Count >= longListBehavior.MaxAcquiredItems)
			{
				break;
			}
			if (!longListBehavior.IsItemShowable(j))
			{
				continue;
			}
			bool addItem = true;
			if (preserveScrolling)
			{
				addItem = false;
				Vector3 itemSize = Vector3.Scale(longListBehavior.GetItemSize(j), itemSizeScaleFactor);
				Vector3 itemPoint = nextItemPosition;
				itemPoint[layoutDimension1] += (float)axisDirection * itemSize[layoutDimension1];
				if (globalClipBounds.Contains(nextItemPosition) || globalClipBounds.Contains(itemPoint))
				{
					addItem = true;
				}
				nextItemPosition = itemPoint;
				nextItemPosition[layoutDimension1] += (float)axisDirection * scaledElementSpacing;
			}
			if (addItem)
			{
				countAdded++;
				ITouchListItem item3 = longListBehavior.AcquireItem(j);
				Add(item3, repositionItems: false);
				itemInfos[item3].LongListIndex = j;
			}
		}
		RepositionItems(visualizedListIndex, initialItemPosition);
		if (visualizedListIndex == 0)
		{
			LoadBehind();
		}
		if (indexToRemove >= 0)
		{
			LoadAhead();
		}
		bool hasScrolled = false;
		float outOfBoundsDist = GetOutOfBoundsDist(content.transform.localPosition[layoutDimension1]);
		if (outOfBoundsDist != 0f && excessContentSize > 0f)
		{
			Vector3 nextPos = content.transform.localPosition;
			nextPos[layoutDimension1] -= outOfBoundsDist;
			float num = nextPos[layoutDimension1] - content.transform.localPosition[layoutDimension1];
			content.transform.localPosition = nextPos;
			lastContentPosition = content.transform.localPosition[layoutDimension1];
			if (num < 0f)
			{
				LoadAhead();
			}
			else
			{
				LoadBehind();
			}
			hasScrolled = true;
		}
		if (selectedLongListItem >= 0 && renderedItems.Count > 0 && selectedLongListItem >= itemInfos[renderedItems.First()].LongListIndex && selectedLongListItem <= itemInfos[renderedItems.Last()].LongListIndex)
		{
			for (int k = 0; k < renderedItems.Count; k++)
			{
				if (renderedItems[k] is ISelectableTouchListItem item4 && itemInfos[item4].LongListIndex == selectedLongListItem)
				{
					selection = k;
					item4.Selected();
					break;
				}
			}
		}
		hasScrolled = RecalculateLongListContentSize(fireOnScroll: false) || hasScrolled;
		allowModification = prevAllowModification;
		if (hasScrolled)
		{
			OnScrolled();
			OnScrollingEnabledChanged();
		}
	}

	private void OnScrollingEnabledChanged()
	{
		if (this.ScrollingEnabledChanged != null)
		{
			if (longListBehavior == null)
			{
				this.ScrollingEnabledChanged(excessContentSize > 0f && scrollEnabled);
			}
			else
			{
				this.ScrollingEnabledChanged(m_fullListContentSize > ClipSize[GetVector2Dimension(layoutDimension1)] && scrollEnabled);
			}
		}
	}

	public void RecalculateItemSizeAndOffsets(bool ignoreCurrentPosition)
	{
		for (int i = 0; i < renderedItems.Count; i++)
		{
			itemInfos[renderedItems[i]].CalculateSizeAndOffset(layoutPlane, ignoreCurrentPosition);
		}
		RepositionItems(0, null);
	}

	private void RepositionItems(int startingIndex, Vector3? initialItemPosition = null)
	{
		if (layoutSuspended)
		{
			return;
		}
		if (orientation == Orientation.Vertical)
		{
			base.transform.localScale = Vector3.one;
		}
		Vector3 contentPos = content.transform.localPosition;
		content.transform.localPosition = Vector3.zero;
		Vector3 nextItemPosition = CalculateLocalClipBounds().min;
		if (initialItemPosition.HasValue)
		{
			nextItemPosition = initialItemPosition.Value;
		}
		nextItemPosition[layoutDimension1] += padding[GetVector2Dimension(layoutDimension1)];
		nextItemPosition[layoutDimension3] = 0f;
		content.transform.localPosition = contentPos;
		ValidateBreadth();
		startingIndex -= startingIndex % breadth;
		if (startingIndex > 0)
		{
			int num = startingIndex - breadth;
			float lastCellMax = float.MinValue;
			for (int i = num; i < startingIndex && i < renderedItems.Count; i++)
			{
				ITouchListItem item = renderedItems[i];
				lastCellMax = Mathf.Max(itemInfos[item].Max[layoutDimension1], lastCellMax);
			}
			nextItemPosition[layoutDimension1] = lastCellMax + elementSpacing;
		}
		Vector3 layoutDirection = Vector3.zero;
		layoutDirection[layoutDimension1] = 1f;
		for (int j = startingIndex; j < renderedItems.Count; j++)
		{
			ITouchListItem listItem = renderedItems[j];
			if (!listItem.IsHeader && !listItem.Visible)
			{
				renderedItems[j].Visible = false;
				renderedItems[j].gameObject.SetActive(value: false);
				continue;
			}
			ItemInfo item2 = itemInfos[renderedItems[j]];
			Vector3 position = nextItemPosition + item2.Offset;
			position[layoutDimension2] = GetBreadthPosition(j) + item2.Offset[layoutDimension2];
			renderedItems[j].transform.localPosition = position;
			renderedItems[j].OnPositionUpdate();
			if ((j + 1) % breadth == 0)
			{
				nextItemPosition = (item2.Max[layoutDimension1] + elementSpacing) * layoutDirection;
			}
		}
		RecalculateSize();
		UpdateBackgroundScroll();
		if (orientation == Orientation.Vertical)
		{
			base.transform.localScale = GetNegatedScale(Vector3.one);
		}
		SetVisibilityOfAllItems();
	}

	private void InsertAndPositionBehind(ITouchListItem item, int longListIndex)
	{
		if (renderedItems.Count == 0)
		{
			Add(item, repositionItems: true);
			return;
		}
		ITouchListItem prevFirstItem = renderedItems.FirstOrDefault();
		if (prevFirstItem == null)
		{
			Insert(0, item, repositionItems: true);
			return;
		}
		if (orientation == Orientation.Vertical)
		{
			base.transform.localScale = Vector3.one;
		}
		ItemInfo prevFirstItemInfo = itemInfos[prevFirstItem];
		Vector3 vector = prevFirstItem.transform.localPosition - prevFirstItemInfo.Offset;
		Insert(0, item, repositionItems: false);
		itemInfos[item].LongListIndex = longListIndex;
		ItemInfo info = itemInfos[item];
		Vector3 newItemPosition = vector;
		float itemSize = info.Size[layoutDimension1] + elementSpacing;
		newItemPosition[layoutDimension1] -= itemSize;
		newItemPosition += info.Offset;
		item.transform.localPosition = newItemPosition;
		if (selection == -1 && item is ISelectableTouchListItem && ((ISelectableTouchListItem)item).IsSelected())
		{
			selection = 0;
		}
		RecalculateSize();
		UpdateBackgroundScroll();
		if (orientation == Orientation.Vertical)
		{
			base.transform.localScale = GetNegatedScale(Vector3.one);
		}
		bool visible = IsItemVisible(0);
		item.gameObject.SetActive(visible);
	}

	private void RecalculateSize()
	{
		float clipSize = Math.Abs((GetComponent<Collider>() as BoxCollider).size[layoutDimension1]);
		float min = (0f - clipSize) / 2f;
		float max = min;
		if (renderedItems.Any())
		{
			ValidateBreadth();
			int num = renderedItems.Count - 1;
			int num2 = num - num % breadth;
			int end = Math.Min(num2 + breadth, renderedItems.Count);
			for (int i = num2; i < end; i++)
			{
				ITouchListItem item = renderedItems[i];
				max = Math.Max(itemInfos[item].Max[layoutDimension1], max);
			}
			contentSize = max - min + padding[GetVector2Dimension(layoutDimension1)];
			excessContentSize = Math.Max(contentSize - clipSize, 0f);
		}
		else
		{
			contentSize = 0f;
			excessContentSize = 0f;
		}
		OnScrollingEnabledChanged();
	}

	public bool RecalculateLongListContentSize(bool fireOnScroll = true)
	{
		if (longListBehavior == null)
		{
			return false;
		}
		float prevSize = m_fullListContentSize;
		m_fullListContentSize = 0f;
		bool first = true;
		for (int i = 0; i < longListBehavior.AllItemsCount; i++)
		{
			if (longListBehavior.IsItemShowable(i))
			{
				m_fullListContentSize += longListBehavior.GetItemSize(i)[layoutDimension1];
				if (first)
				{
					first = false;
				}
				else
				{
					m_fullListContentSize += elementSpacing;
				}
			}
		}
		if (m_fullListContentSize > 0f)
		{
			m_fullListContentSize += 2f * padding[GetVector2Dimension(layoutDimension1)];
		}
		bool num = prevSize != m_fullListContentSize;
		if (num && fireOnScroll)
		{
			OnScrolled();
			OnScrollingEnabledChanged();
		}
		return num;
	}

	private void UpdateBackgroundBounds()
	{
		if (!(background == null))
		{
			Collider touchListCollider = GetComponent<Collider>();
			Vector3 size = (touchListCollider as BoxCollider).size;
			size[layoutDimension1] = Math.Abs(size[layoutDimension1]);
			size[layoutDimension3] = 0f;
			Camera camera = CameraUtils.FindFirstByLayer((GameLayer)base.gameObject.layer);
			if (!(camera == null))
			{
				float minDist = Vector3.Distance(camera.transform.position, touchListCollider.bounds.min);
				float maxDist = Vector3.Distance(camera.transform.position, touchListCollider.bounds.max);
				Vector3 planePoint = ((minDist > maxDist) ? touchListCollider.bounds.min : touchListCollider.bounds.max);
				Vector3 position = Vector3.zero;
				position[layoutDimension3] = content.transform.InverseTransformPoint(planePoint)[layoutDimension3];
				background.SetBounds(new Bounds(position, size));
				UpdateBackgroundScroll();
			}
		}
	}

	private void UpdateBackgroundScroll()
	{
		if (!(background == null))
		{
			float clipSize = Math.Abs((GetComponent<Collider>() as BoxCollider).size[layoutDimension1]);
			float contentPosition = content.transform.localPosition[layoutDimension1];
			if (orientation == Orientation.Vertical)
			{
				contentPosition *= -1f;
			}
			Vector2 curOffset = background.Offset;
			curOffset[GetVector2Dimension(layoutDimension1)] = contentPosition / clipSize;
			background.Offset = curOffset;
		}
	}

	private float GetBreadthPosition(int itemIndex)
	{
		float totalBreadth = padding[GetVector2Dimension(layoutDimension2)];
		float breadthOffset = 0f;
		int num = itemIndex - itemIndex % breadth;
		int end = Math.Min(num + breadth, renderedItems.Count);
		for (int i = num; i < end; i++)
		{
			if (i == itemIndex)
			{
				breadthOffset = totalBreadth;
			}
			totalBreadth += itemInfos[renderedItems[i]].Size[layoutDimension2];
		}
		totalBreadth += padding[GetVector2Dimension(layoutDimension2)];
		float breadthBegin = 0f;
		float clipSize = (GetComponent<Collider>() as BoxCollider).size[layoutDimension2];
		Alignment pseudoAlign = alignment;
		if (orientation == Orientation.Horizontal && alignment != Alignment.Mid)
		{
			pseudoAlign = alignment ^ Alignment.Max;
		}
		switch (pseudoAlign)
		{
		case Alignment.Min:
			breadthBegin = (0f - clipSize) / 2f;
			break;
		case Alignment.Mid:
			breadthBegin = (0f - totalBreadth) / 2f;
			break;
		case Alignment.Max:
			breadthBegin = clipSize / 2f - totalBreadth;
			break;
		}
		return breadthBegin + breadthOffset;
	}

	private Vector3 GetNegatedScale(Vector3 scale)
	{
		scale[(layoutPlane == LayoutPlane.XY) ? 1 : 2] *= -1f;
		return scale;
	}

	private int GetVector2Dimension(int vec3Dimension)
	{
		if (vec3Dimension != 0)
		{
			return 1;
		}
		return vec3Dimension;
	}

	private int GetVector3Dimension(int vec2Dimension)
	{
		if (vec2Dimension == 0 || layoutPlane == LayoutPlane.XY)
		{
			return vec2Dimension;
		}
		return 2;
	}

	private void ValidateBreadth()
	{
		if (longListBehavior != null)
		{
			breadth = 1;
		}
		else
		{
			breadth = Math.Max(breadth, 1);
		}
	}

	private Bounds CalculateLocalClipBounds()
	{
		Collider touchListCollider = GetComponent<Collider>();
		Vector3 min = content.transform.InverseTransformPoint(touchListCollider.bounds.min);
		Vector3 vector = content.transform.InverseTransformPoint(touchListCollider.bounds.max);
		Vector3 center = (vector + min) / 2f;
		Vector3 size = VectorUtils.Abs(vector - min);
		return new Bounds(center, size);
	}

	private PegUIElement GetChildHitElement(out RaycastHit hitInfo)
	{
		if ((bool)PegUI.Get().FindHitElement(out hitInfo) && hitInfo.transform.IsChildOf(base.transform))
		{
			return hitInfo.transform.GetComponent<PegUIElement>();
		}
		return null;
	}
}
