using System;
using System.Collections.Generic;
using Hearthstone.UI;
using Hearthstone.UI.Core;
using UnityEngine;
using UnityEngine.Serialization;

public class ContentFitter : WidgetPositioningElement
{
	[Serializable]
	private struct ScaleDirection
	{
		public enum PaddingMode
		{
			Additive,
			Multiply
		}

		public bool X;

		public bool Y;

		public bool Z;

		public bool MatchXAndZ;

		public bool MatchXAndY;

		public bool MatchYAndZ;

		public Vector3 Padding;

		public PaddingMode Mode;

		public Vector3 GetScale(Vector3 original, Vector3 target)
		{
			float xScale = original.x;
			float yScale = original.y;
			float zScale = original.z;
			switch (Mode)
			{
			case PaddingMode.Additive:
				xScale = (X ? (target.x + Padding.x) : original.x);
				yScale = (Y ? (target.y + Padding.y) : original.y);
				zScale = (Z ? (target.z + Padding.z) : original.z);
				break;
			case PaddingMode.Multiply:
				xScale = (X ? (target.x * Padding.x) : original.x);
				yScale = (Y ? (target.y * Padding.y) : original.y);
				zScale = (Z ? (target.z * Padding.z) : original.z);
				break;
			}
			if (MatchXAndZ && (X ^ Z))
			{
				if (X)
				{
					zScale = xScale;
				}
				else if (Z)
				{
					xScale = zScale;
				}
			}
			if (MatchYAndZ && (Y ^ Z))
			{
				if (Y)
				{
					zScale = yScale;
				}
				else if (Z)
				{
					yScale = zScale;
				}
			}
			if (MatchXAndY && (X ^ Y))
			{
				if (X)
				{
					yScale = xScale;
				}
				else if (Y)
				{
					xScale = yScale;
				}
			}
			return new Vector3(xScale, yScale, zScale);
		}
	}

	[SerializeField]
	[FormerlySerializedAs("m_target")]
	[Tooltip("The target which the attached gameobject will scale to")]
	[Header("Fitter Data")]
	private GameObject m_parent;

	[Tooltip("The transform that will scale")]
	[SerializeField]
	private Transform m_transform;

	[SerializeField]
	private ScaleDirection m_scaleDirection;

	[SerializeField]
	[Header("Find Components")]
	private bool m_useChildren;

	[SerializeField]
	private bool m_useMeshRenderers;

	[SerializeField]
	private bool m_useWidgetTransform;

	[SerializeField]
	private bool m_useTextSize;

	[SerializeField]
	private List<GameObject> m_ignoreObjects = new List<GameObject>();

	[SerializeField]
	[Header("Specified Components")]
	private List<MeshFilter> m_targetMeshFilters;

	[SerializeField]
	private List<WidgetTransform> m_targetWidgetTransforms;

	[SerializeField]
	private List<UberText> m_targetUberText;

	[SerializeField]
	private List<MeshFilter> m_thisMeshFilters;

	[SerializeField]
	private List<WidgetTransform> m_thisWidgetTransforms;

	[SerializeField]
	private List<UberText> m_thisUberText;

	[Overridable]
	public bool UpdateResizeToggle
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

	private void Awake()
	{
		if (m_transform == null)
		{
			m_transform = base.transform;
		}
	}

	protected override void InternalRefresh()
	{
		if (m_parent == null || m_transform == null)
		{
			return;
		}
		UpdateCachedComponents();
		GameObject parent = m_parent;
		List<MeshFilter> targetMeshFilters = m_targetMeshFilters;
		List<WidgetTransform> targetWidgetTransforms = m_targetWidgetTransforms;
		List<UberText> targetUberText = m_targetUberText;
		OrientedBounds targetBounds = TransformUtil.ComputeOrientedWorldBoundsWithComponents(parent, default(Vector3), default(Vector3), targetMeshFilters, targetWidgetTransforms, targetUberText);
		if (targetBounds == null)
		{
			return;
		}
		Vector3 moddedSize = Vector3.zero;
		Vector3 targetSize = targetBounds.GetSize();
		if (targetSize != Vector3.zero)
		{
			m_transform.localScale = Vector3.one;
			GameObject go = m_transform.gameObject;
			targetMeshFilters = m_thisMeshFilters;
			targetWidgetTransforms = m_thisWidgetTransforms;
			targetUberText = m_thisUberText;
			OrientedBounds currentBounds = TransformUtil.ComputeOrientedWorldBoundsWithComponents(go, default(Vector3), default(Vector3), targetMeshFilters, targetWidgetTransforms, targetUberText);
			if (currentBounds == null)
			{
				return;
			}
			Vector3 currentSize = currentBounds.GetSize();
			moddedSize = new Vector3((currentSize.x != 0f) ? (targetSize.x / currentSize.x) : 0f, (currentSize.y != 0f) ? (targetSize.y / currentSize.y) : 0f, (currentSize.z != 0f) ? (targetSize.z / currentSize.z) : 0f);
		}
		m_transform.localScale = m_scaleDirection.GetScale(base.gameObject.transform.localScale, moddedSize);
	}

	private void UpdateCachedComponents()
	{
		if (m_useMeshRenderers)
		{
			foreach (MeshFilter meshFilter in TransformUtil.GetComponentsWithIgnore<MeshFilter>(base.gameObject, m_ignoreObjects, m_useChildren))
			{
				if (!m_thisMeshFilters.Contains(meshFilter))
				{
					m_thisMeshFilters.Add(meshFilter);
				}
			}
			if (m_parent != null)
			{
				foreach (MeshFilter meshFilter2 in TransformUtil.GetComponentsWithIgnore<MeshFilter>(m_parent, m_ignoreObjects, m_useChildren))
				{
					if (!m_targetMeshFilters.Contains(meshFilter2))
					{
						m_targetMeshFilters.Add(meshFilter2);
					}
				}
			}
		}
		if (m_useWidgetTransform)
		{
			foreach (WidgetTransform widgetTransform in TransformUtil.GetComponentsWithIgnore<WidgetTransform>(base.gameObject, m_ignoreObjects, m_useChildren))
			{
				if (!m_thisWidgetTransforms.Contains(widgetTransform))
				{
					m_thisWidgetTransforms.Add(widgetTransform);
				}
			}
			if (m_parent != null)
			{
				foreach (WidgetTransform widgetTransform2 in TransformUtil.GetComponentsWithIgnore<WidgetTransform>(m_parent, m_ignoreObjects, m_useChildren))
				{
					if (!m_targetWidgetTransforms.Contains(widgetTransform2))
					{
						m_targetWidgetTransforms.Add(widgetTransform2);
					}
				}
			}
		}
		if (!m_useTextSize)
		{
			return;
		}
		foreach (UberText uberText in TransformUtil.GetComponentsWithIgnore<UberText>(base.gameObject, m_ignoreObjects, m_useChildren))
		{
			if (!m_thisUberText.Contains(uberText))
			{
				m_thisUberText.Add(uberText);
			}
		}
		if (!(m_parent != null))
		{
			return;
		}
		foreach (UberText uberText2 in TransformUtil.GetComponentsWithIgnore<UberText>(m_parent, m_ignoreObjects, m_useChildren))
		{
			if (!m_targetUberText.Contains(uberText2))
			{
				m_targetUberText.Add(uberText2);
			}
		}
	}
}
