using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
[CustomEditClass]
public class NineSliceElement : MonoBehaviour
{
	[CustomEditField(Sections = "Top Row")]
	public MultiSliceElement.Slice m_topRow;

	[CustomEditField(Sections = "Middle Row")]
	public MultiSliceElement.Slice m_midRow;

	[CustomEditField(Sections = "Bottom Row")]
	public MultiSliceElement.Slice m_btmRow;

	[CustomEditField(Sections = "Top Row")]
	public MultiSliceElement.Slice m_topLeft;

	[CustomEditField(Sections = "Top Row")]
	public MultiSliceElement.Slice m_top;

	[CustomEditField(Sections = "Top Row")]
	public MultiSliceElement.Slice m_topRight;

	[CustomEditField(Sections = "Middle Row")]
	public MultiSliceElement.Slice m_left;

	[CustomEditField(Sections = "Middle Row")]
	public MultiSliceElement.Slice m_middle;

	[CustomEditField(Sections = "Middle Row")]
	public MultiSliceElement.Slice m_right;

	[CustomEditField(Sections = "Bottom Row")]
	public MultiSliceElement.Slice m_bottomLeft;

	[CustomEditField(Sections = "Bottom Row")]
	public MultiSliceElement.Slice m_bottom;

	[CustomEditField(Sections = "Bottom Row")]
	public MultiSliceElement.Slice m_bottomRight;

	public List<GameObject> m_ignore = new List<GameObject>();

	public MultiSliceElement.Direction m_WidthDirection;

	public MultiSliceElement.Direction m_HeightDirection = MultiSliceElement.Direction.Z;

	public Vector3 m_localPinnedPointOffset = Vector3.zero;

	public MultiSliceElement.XAxisAlign m_XAlign;

	public MultiSliceElement.YAxisAlign m_YAlign = MultiSliceElement.YAxisAlign.BOTTOM;

	public MultiSliceElement.ZAxisAlign m_ZAlign = MultiSliceElement.ZAxisAlign.BACK;

	public Vector3 m_localSliceSpacing = Vector3.zero;

	public bool m_reverse;

	public void SetEntireWidth(float width)
	{
		int dir = (int)m_WidthDirection;
		OrientedBounds topLeftBounds = GetSliceBounds(m_topLeft);
		OrientedBounds leftBounds = GetSliceBounds(m_left);
		OrientedBounds btmLeftBounds = GetSliceBounds(m_bottomLeft);
		OrientedBounds topRightBounds = GetSliceBounds(m_topRight);
		OrientedBounds rightBounds = GetSliceBounds(m_right);
		OrientedBounds btmRightBounds = GetSliceBounds(m_bottomRight);
		float leftWidth = Mathf.Max(topLeftBounds.Extents[dir].magnitude, leftBounds.Extents[dir].magnitude, btmLeftBounds.Extents[dir].magnitude) * 2f;
		float rightWidth = Mathf.Max(topRightBounds.Extents[dir].magnitude, rightBounds.Extents[dir].magnitude, btmRightBounds.Extents[dir].magnitude) * 2f;
		SetWidth(width - leftWidth - rightWidth);
	}

	public void SetEntireHeight(float height)
	{
		int dir = (int)m_HeightDirection;
		OrientedBounds topLeftBounds = GetSliceBounds(m_topLeft);
		OrientedBounds topBounds = GetSliceBounds(m_top);
		OrientedBounds topRightBounds = GetSliceBounds(m_topRight);
		OrientedBounds btmLeftBounds = GetSliceBounds(m_bottomLeft);
		OrientedBounds btmBounds = GetSliceBounds(m_bottom);
		OrientedBounds btmRightBounds = GetSliceBounds(m_bottomRight);
		float topHeight = Mathf.Max(topLeftBounds.Extents[dir].magnitude, topBounds.Extents[dir].magnitude, topRightBounds.Extents[dir].magnitude) * 2f;
		float btmHeight = Mathf.Max(btmLeftBounds.Extents[dir].magnitude, btmBounds.Extents[dir].magnitude, btmRightBounds.Extents[dir].magnitude) * 2f;
		SetHeight(height - topHeight - btmHeight);
	}

	public float GetEntireHeight()
	{
		int dir = (int)m_HeightDirection;
		OrientedBounds sliceBounds = GetSliceBounds(m_top);
		OrientedBounds midBounds = GetSliceBounds(m_middle);
		OrientedBounds btmBounds = GetSliceBounds(m_bottom);
		return sliceBounds.Extents[dir].magnitude + midBounds.Extents[dir].magnitude + btmBounds.Extents[dir].magnitude;
	}

	public void SetEntireSize(Vector2 size)
	{
		SetEntireSize(size.x, size.y);
	}

	public void SetEntireSize(float width, float height)
	{
		int widthDir = (int)m_WidthDirection;
		int heightDir = (int)m_HeightDirection;
		OrientedBounds topLeftBounds = GetSliceBounds(m_topLeft);
		OrientedBounds topBounds = GetSliceBounds(m_top);
		OrientedBounds topRightBounds = GetSliceBounds(m_topRight);
		OrientedBounds leftBounds = GetSliceBounds(m_left);
		OrientedBounds rightBounds = GetSliceBounds(m_right);
		OrientedBounds btmLeftBounds = GetSliceBounds(m_bottomLeft);
		OrientedBounds btmBounds = GetSliceBounds(m_bottom);
		OrientedBounds btmRightBounds = GetSliceBounds(m_bottomRight);
		float leftWidth = Mathf.Max(topLeftBounds.Extents[widthDir].magnitude, leftBounds.Extents[widthDir].magnitude, btmLeftBounds.Extents[widthDir].magnitude) * 2f;
		float rightWidth = Mathf.Max(topRightBounds.Extents[widthDir].magnitude, rightBounds.Extents[widthDir].magnitude, btmRightBounds.Extents[widthDir].magnitude) * 2f;
		float topHeight = Mathf.Max(topLeftBounds.Extents[heightDir].magnitude, topBounds.Extents[heightDir].magnitude, topRightBounds.Extents[heightDir].magnitude) * 2f;
		float btmHeight = Mathf.Max(btmLeftBounds.Extents[heightDir].magnitude, btmBounds.Extents[heightDir].magnitude, btmRightBounds.Extents[heightDir].magnitude) * 2f;
		SetSize(width - leftWidth - rightWidth, height - topHeight - btmHeight);
	}

	public void SetWidth(float width)
	{
		width = Mathf.Max(width, 0f);
		int dirWidth = (int)m_WidthDirection;
		SetSliceSize(m_top, new WorldDimensionIndex(width, dirWidth));
		SetSliceSize(m_bottom, new WorldDimensionIndex(width, dirWidth));
		SetSliceSize(m_middle, new WorldDimensionIndex(width, dirWidth));
		UpdateAllSlices();
	}

	public void SetHeight(float height)
	{
		height = Mathf.Max(height, 0f);
		int dirHeight = (int)m_HeightDirection;
		SetSliceSize(m_left, new WorldDimensionIndex(height, dirHeight));
		SetSliceSize(m_right, new WorldDimensionIndex(height, dirHeight));
		SetSliceSize(m_middle, new WorldDimensionIndex(height, dirHeight));
		UpdateAllSlices();
	}

	public void SetSize(Vector2 size)
	{
		SetSize(size.x, size.y);
	}

	public void SetSize(float width, float height)
	{
		width = Mathf.Max(width, 0f);
		height = Mathf.Max(height, 0f);
		int dirWidth = (int)m_WidthDirection;
		int dirHeight = (int)m_HeightDirection;
		SetSliceSize(m_top, new WorldDimensionIndex(width, dirWidth));
		SetSliceSize(m_bottom, new WorldDimensionIndex(width, dirWidth));
		SetSliceSize(m_left, new WorldDimensionIndex(height, dirHeight));
		SetSliceSize(m_right, new WorldDimensionIndex(height, dirHeight));
		SetSliceSize(m_middle, new WorldDimensionIndex(width, dirWidth), new WorldDimensionIndex(height, dirHeight));
		UpdateAllSlices();
	}

	public void SetMiddleScale(float scaleWidth, float scaleHeight)
	{
		Vector3 newScale = m_middle.m_slice.transform.localScale;
		newScale[(int)m_WidthDirection] = scaleWidth;
		newScale[(int)m_HeightDirection] = scaleHeight;
		m_middle.m_slice.transform.localScale = newScale;
		UpdateSegmentsToMatchMiddle();
		UpdateAllSlices();
	}

	public Vector2 GetWorldDimensions()
	{
		GameObject go = m_middle;
		List<GameObject> ignore = m_ignore;
		bool includeAllChildren = m_middle.m_includeAllChildren;
		OrientedBounds middleBounds = TransformUtil.ComputeOrientedWorldBounds(go, default(Vector3), default(Vector3), ignore, includeAllChildren, includeMeshRenderers: true, includeWidgetTransformBounds: true);
		return new Vector2(middleBounds.Extents[(int)m_WidthDirection].magnitude * 2f, middleBounds.Extents[(int)m_HeightDirection].magnitude * 2f);
	}

	private OrientedBounds GetSliceBounds(GameObject slice)
	{
		if (slice != null)
		{
			List<GameObject> ignore = m_ignore;
			return TransformUtil.ComputeOrientedWorldBounds(slice, default(Vector3), default(Vector3), ignore, includeAllChildren: true, includeMeshRenderers: true, includeWidgetTransformBounds: true);
		}
		OrientedBounds orientedBounds = new OrientedBounds();
		orientedBounds.Extents = new Vector3[3]
		{
			Vector3.zero,
			Vector3.zero,
			Vector3.zero
		};
		orientedBounds.Origin = Vector3.zero;
		orientedBounds.CenterOffset = Vector3.zero;
		return orientedBounds;
	}

	private void SetSliceSize(GameObject slice, params WorldDimensionIndex[] dimensions)
	{
		if (slice != null)
		{
			TransformUtil.SetLocalScaleToWorldDimension(slice, m_ignore, dimensions);
		}
	}

	private void UpdateAllSlices()
	{
		if (m_topRow.m_slice == null || m_midRow.m_slice == null || m_btmRow.m_slice == null)
		{
			Debug.LogError("Nine Slice elements for one of these object is null: top Row, mid Row, btm Row");
			return;
		}
		UpdateRowSlices(m_topRow.m_slice.transform, new List<MultiSliceElement.Slice> { m_topLeft, m_top, m_topRight }, m_WidthDirection);
		UpdateRowSlices(m_midRow.m_slice.transform, new List<MultiSliceElement.Slice> { m_left, m_middle, m_right }, m_WidthDirection);
		UpdateRowSlices(m_btmRow.m_slice.transform, new List<MultiSliceElement.Slice> { m_bottomLeft, m_bottom, m_bottomRight }, m_WidthDirection);
		UpdateRowSlices(base.transform, new List<MultiSliceElement.Slice> { m_topRow, m_midRow, m_btmRow }, m_HeightDirection);
	}

	private void UpdateRowSlices(Transform parent, List<MultiSliceElement.Slice> slices, MultiSliceElement.Direction direction)
	{
		Vector3 localSpacing = Vector3.zero;
		Vector3 localOffset = Vector3.zero;
		localSpacing[(int)direction] = m_localSliceSpacing[(int)direction];
		localOffset[(int)direction] = m_localPinnedPointOffset[(int)direction];
		MultiSliceElement.PositionSlices(parent, slices, m_reverse, direction, localSpacing, localOffset, m_XAlign, m_YAlign, m_ZAlign, m_ignore);
	}

	private void UpdateSegmentsToMatchMiddle()
	{
		GameObject go = m_middle;
		List<GameObject> ignore = m_ignore;
		bool includeAllChildren = m_middle.m_includeAllChildren;
		bool useMeshRenderers = m_middle.m_useMeshRenderers;
		bool useWidgetTransform = m_middle.m_useWidgetTransform;
		bool useTextSize = m_middle.m_useTextSize;
		OrientedBounds middleBounds = TransformUtil.ComputeOrientedWorldBounds(go, default(Vector3), default(Vector3), ignore, includeAllChildren, useMeshRenderers, useWidgetTransform, useTextSize);
		if (middleBounds != null)
		{
			float width = middleBounds.Extents[(int)m_WidthDirection].magnitude * 2f;
			float height = middleBounds.Extents[(int)m_HeightDirection].magnitude * 2f;
			int dirWidth = (int)m_WidthDirection;
			int dirHeight = (int)m_HeightDirection;
			SetSliceSize(m_top, new WorldDimensionIndex(width, dirWidth));
			SetSliceSize(m_bottom, new WorldDimensionIndex(width, dirWidth));
			SetSliceSize(m_left, new WorldDimensionIndex(height, dirHeight));
			SetSliceSize(m_right, new WorldDimensionIndex(height, dirHeight));
		}
	}
}
