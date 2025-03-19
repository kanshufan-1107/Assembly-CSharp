using System;
using System.Collections.Generic;
using System.Linq;
using Hearthstone.UI;
using Hearthstone.UI.Core;
using UnityEngine;
using UnityEngine.Serialization;

[ExecuteAlways]
public class MultiSliceElement : WidgetPositioningElement
{
	public enum Direction
	{
		X,
		Y,
		Z
	}

	public enum XAxisAlign
	{
		LEFT,
		MIDDLE,
		RIGHT
	}

	public enum YAxisAlign
	{
		TOP,
		MIDDLE,
		BOTTOM
	}

	public enum ZAxisAlign
	{
		FRONT,
		MIDDLE,
		BACK
	}

	[Serializable]
	public class Slice
	{
		public enum SliceLoadingMethod
		{
			GameObject,
			AsyncRef
		}

		[Header("Slice Options")]
		public SliceLoadingMethod m_loadingMethod;

		public GameObject m_slice;

		public AsyncReference m_sliceRef;

		public Vector3 m_minLocalPadding;

		public Vector3 m_maxLocalPadding;

		public bool m_reverse;

		public bool m_refreshOnTextChange;

		[Header("Cached Components")]
		public List<MeshFilter> m_cachedMeshFilters = new List<MeshFilter>();

		public List<WidgetTransform> m_cachedWidgetTransforms = new List<WidgetTransform>();

		public List<UberText> m_cachedUberTexts = new List<UberText>();

		[Header("Auto-Fetch Components - Expensive!")]
		public bool m_useMeshRenderers = true;

		public bool m_useWidgetTransform;

		public bool m_useTextSize;

		public bool m_includeAllChildren = true;

		public MultiSliceElement MultisliceOwner { get; set; }

		public void OnTextUpdated(object owner, EventArgs args)
		{
			if (MultisliceOwner != null)
			{
				MultisliceOwner.RefreshWithDelay();
			}
		}

		public static implicit operator GameObject(Slice slice)
		{
			return slice?.m_slice;
		}
	}

	public List<Slice> m_slices = new List<Slice>();

	public List<GameObject> m_ignore = new List<GameObject>();

	public Vector3 m_localPinnedPointOffset = Vector3.zero;

	[SerializeField]
	private XAxisAlign m_XAlign;

	[SerializeField]
	private YAxisAlign m_YAlign = YAxisAlign.BOTTOM;

	[SerializeField]
	private ZAxisAlign m_ZAlign = ZAxisAlign.BACK;

	public Vector3 m_localSliceSpacing = Vector3.zero;

	public Direction m_direction;

	public bool m_reverse;

	[FormerlySerializedAs("m_useUberText")]
	[Tooltip("Replaced by UseTextSize on the slice entries. Disable if true and tick individual components")]
	public bool m_legacy_UseUberText;

	public bool m_weldable;

	private GeometryWeld m_sliceWeld;

	[Overridable]
	public bool UpdateSliceToggle
	{
		get
		{
			return false;
		}
		set
		{
			if (value)
			{
				Refresh();
			}
		}
	}

	[Overridable]
	public XAxisAlign XAlign
	{
		get
		{
			return m_XAlign;
		}
		set
		{
			if (value != m_XAlign)
			{
				m_XAlign = value;
				Refresh();
			}
		}
	}

	[Overridable]
	public YAxisAlign YAlign
	{
		get
		{
			return m_YAlign;
		}
		set
		{
			if (value != m_YAlign)
			{
				m_YAlign = value;
				Refresh();
			}
		}
	}

	[Overridable]
	public ZAxisAlign ZAlign
	{
		get
		{
			return m_ZAlign;
		}
		set
		{
			if (value != m_ZAlign)
			{
				m_ZAlign = value;
				Refresh();
			}
		}
	}

	private void Awake()
	{
		if (m_legacy_UseUberText)
		{
			for (int i = 0; i < m_slices.Count; i++)
			{
				Slice sliceData = m_slices[i];
				if (sliceData.m_slice != null && (bool)sliceData.m_slice.GetComponentInChildren<UberText>())
				{
					sliceData.m_useWidgetTransform = true;
					m_slices[i] = sliceData;
				}
			}
		}
		foreach (Slice slice in m_slices)
		{
			slice.MultisliceOwner = this;
			if (slice.m_refreshOnTextChange)
			{
				foreach (UberText text in slice.m_cachedUberTexts)
				{
					if (text != null)
					{
						text.TextUpdated -= slice.OnTextUpdated;
						text.TextUpdated += slice.OnTextUpdated;
					}
				}
			}
			if (slice.m_loadingMethod != Slice.SliceLoadingMethod.AsyncRef || slice.m_sliceRef.IsReady)
			{
				continue;
			}
			slice.m_slice = null;
			slice.m_sliceRef.RegisterReadyListener(delegate(Widget w)
			{
				if (w != null)
				{
					slice.m_slice = w.gameObject;
					RefreshWithDelay();
				}
			});
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		foreach (Slice slice in m_slices)
		{
			foreach (UberText text in slice.m_cachedUberTexts)
			{
				if (text != null)
				{
					text.TextUpdated -= slice.OnTextUpdated;
				}
			}
		}
	}

	public void AddSlice(GameObject obj, Vector3 minLocalPadding = default(Vector3), Vector3 maxLocalPadding = default(Vector3), bool reverse = false, bool useMeshRenderers = true, bool useWidgetTransform = false, bool useTextSize = false, bool includeAllChildren = true)
	{
		m_slices.Add(new Slice
		{
			m_slice = obj,
			m_minLocalPadding = minLocalPadding,
			m_maxLocalPadding = maxLocalPadding,
			m_reverse = reverse,
			m_includeAllChildren = includeAllChildren,
			m_useMeshRenderers = useMeshRenderers,
			m_useWidgetTransform = useWidgetTransform,
			m_useTextSize = useTextSize
		});
	}

	public void ClearSlices()
	{
		m_slices.Clear();
	}

	protected override void InternalRefresh()
	{
		UpdateSlices();
	}

	public void UpdateSlices()
	{
		if (m_sliceWeld != null)
		{
			m_sliceWeld.Unweld();
		}
		PositionSlices(base.transform, m_slices, m_reverse, m_direction, m_localSliceSpacing, m_localPinnedPointOffset, m_XAlign, m_YAlign, m_ZAlign, m_ignore);
		if (m_weldable)
		{
			m_sliceWeld = new GeometryWeld(base.gameObject, m_slices.Select((Slice x) => x.m_slice).ToArray());
		}
	}

	public static void PositionSlices(Transform root, List<Slice> slices, bool reverseDir, Direction dir, Vector3 localSliceSpacing, Vector3 localPinnedPointOffset, XAxisAlign xAlign, YAxisAlign yAlign, ZAxisAlign zAlign, List<GameObject> ignoreObjects = null)
	{
		if (slices.Count == 0)
		{
			return;
		}
		foreach (Slice slice3 in slices)
		{
			UpdateCachedComponents(slice3, ignoreObjects);
		}
		float reverse = (reverseDir ? (-1f) : 1f);
		List<Slice> activeSlices = new List<Slice>();
		foreach (Slice slice in slices)
		{
			if (TransformUtil.CanComputeOrientedWorldBoundsWithComponents(slice.m_slice, ignoreObjects, slice.m_cachedMeshFilters, slice.m_cachedUberTexts, slice.m_cachedWidgetTransforms))
			{
				activeSlices.Add(slice);
			}
		}
		if (activeSlices.Count == 0)
		{
			return;
		}
		Vector3 prevOffset = Vector3.zero;
		Matrix4x4 thisTransform = root.worldToLocalMatrix;
		Vector3 localMin = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
		Vector3 localMax = new Vector3(float.MinValue, float.MinValue, float.MinValue);
		Slice firstSlice = activeSlices[0];
		GameObject firstSliceObj = firstSlice.m_slice;
		firstSliceObj.transform.localPosition = Vector3.zero;
		OrientedBounds firstSliceBounds = TransformUtil.ComputeOrientedWorldBoundsWithComponents(firstSliceObj, firstSlice.m_minLocalPadding, firstSlice.m_maxLocalPadding, firstSlice.m_cachedMeshFilters, firstSlice.m_cachedWidgetTransforms, firstSlice.m_cachedUberTexts);
		float localReverse = reverse * (firstSlice.m_reverse ? (-1f) : 1f);
		Vector3 moveToCorner = (firstSliceBounds.Extents[0] + firstSliceBounds.Extents[1] + firstSliceBounds.Extents[2]) * localReverse;
		firstSliceObj.transform.position += firstSliceBounds.CenterOffset + moveToCorner;
		prevOffset = firstSliceBounds.Extents[(int)dir] * localReverse + moveToCorner;
		TransformUtil.GetBoundsMinMax(thisTransform * (firstSliceObj.transform.position - firstSliceBounds.CenterOffset), thisTransform * firstSliceBounds.Extents[0], thisTransform * firstSliceBounds.Extents[1], thisTransform * firstSliceBounds.Extents[2], ref localMin, ref localMax);
		Vector3 spacing = localSliceSpacing * reverse;
		for (int i = 1; i < activeSlices.Count; i++)
		{
			Slice slice2 = activeSlices[i];
			GameObject sliceObj = slice2.m_slice;
			float localReverse2 = reverse * (slice2.m_reverse ? (-1f) : 1f);
			sliceObj.transform.localPosition = Vector3.zero;
			OrientedBounds sliceBounds = TransformUtil.ComputeOrientedWorldBoundsWithComponents(sliceObj, slice2.m_minLocalPadding, slice2.m_maxLocalPadding, slice2.m_cachedMeshFilters, slice2.m_cachedWidgetTransforms, slice2.m_cachedUberTexts);
			Vector3 sliceSpacing = sliceObj.transform.localToWorldMatrix * spacing;
			Vector3 shift = sliceBounds.Extents[(int)dir] * localReverse2;
			sliceObj.transform.position += sliceBounds.CenterOffset + prevOffset + shift + sliceSpacing;
			prevOffset += shift * 2f + sliceSpacing;
			TransformUtil.GetBoundsMinMax(thisTransform * (sliceObj.transform.position - sliceBounds.CenterOffset), thisTransform * sliceBounds.Extents[0], thisTransform * sliceBounds.Extents[1], thisTransform * sliceBounds.Extents[2], ref localMin, ref localMax);
		}
		Vector3 startPt = new Vector3(localMin.x, localMax.y, localMin.z);
		Vector3 endPt = new Vector3(localMax.x, localMin.y, localMax.z);
		Vector3 targetPos = root.localToWorldMatrix * (startPt + GetAlignmentVector(endPt - startPt, xAlign, yAlign, zAlign));
		Vector3 vector = root.localToWorldMatrix * localPinnedPointOffset * reverse;
		Vector3 alignmentOffset = root.position - targetPos;
		Vector3 totalOffset = vector + alignmentOffset;
		foreach (Slice item in activeSlices)
		{
			item.m_slice.transform.position += totalOffset;
		}
	}

	private static Vector3 GetAlignmentVector(Vector3 interpolate, XAxisAlign x, YAxisAlign y, ZAxisAlign z)
	{
		return new Vector3(interpolate.x * ((float)x * 0.5f), interpolate.y * ((float)y * 0.5f), interpolate.z * ((float)z * 0.5f));
	}

	private static void UpdateCachedComponents(Slice slice, List<GameObject> ignore)
	{
		if (slice.m_useMeshRenderers)
		{
			slice.m_cachedMeshFilters = TransformUtil.GetComponentsWithIgnore<MeshFilter>(slice.m_slice, ignore, slice.m_includeAllChildren);
		}
		if (slice.m_useWidgetTransform)
		{
			slice.m_cachedWidgetTransforms = TransformUtil.GetComponentsWithIgnore<WidgetTransform>(slice.m_slice, ignore, slice.m_includeAllChildren);
		}
		if (!slice.m_useTextSize)
		{
			return;
		}
		if (slice.m_cachedUberTexts != null && slice.m_cachedUberTexts.Count > 0)
		{
			foreach (UberText cachedUberText in slice.m_cachedUberTexts)
			{
				cachedUberText.TextUpdated -= slice.OnTextUpdated;
			}
		}
		slice.m_cachedUberTexts = TransformUtil.GetComponentsWithIgnore<UberText>(slice.m_slice, ignore, slice.m_includeAllChildren);
		foreach (UberText currentUberText in slice.m_cachedUberTexts)
		{
			if (slice.m_refreshOnTextChange)
			{
				currentUberText.TextUpdated -= slice.OnTextUpdated;
				currentUberText.TextUpdated += slice.OnTextUpdated;
			}
		}
	}
}
