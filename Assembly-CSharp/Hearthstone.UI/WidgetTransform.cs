using System;
using Blizzard.T5.Core.Utils;
using Hearthstone.UI.Core;
using UnityEngine;

namespace Hearthstone.UI;

[DisallowMultipleComponent]
[ExecuteAlways]
public sealed class WidgetTransform : MonoBehaviour, IWidgetTransform
{
	public enum LayoutBoundsMode
	{
		Recursive,
		NonRecursive,
		Excluded
	}

	[SerializeField]
	private Rect m_rect;

	[SerializeField]
	private bool m_hasBounds;

	[SerializeField]
	private FacingDirection m_facingDirection;

	[SerializeField]
	private LayoutBoundsMode m_layoutBoundsMode;

	public Rect Rect
	{
		get
		{
			return m_rect;
		}
		set
		{
			m_rect = value;
			this.OnBoundsChanged?.Invoke();
		}
	}

	[Overridable]
	public float Left
	{
		get
		{
			return m_rect.xMin;
		}
		set
		{
			m_rect.xMin = value;
			this.OnBoundsChanged?.Invoke();
		}
	}

	[Overridable]
	public float Right
	{
		get
		{
			return m_rect.xMax;
		}
		set
		{
			m_rect.xMax = value;
			this.OnBoundsChanged?.Invoke();
		}
	}

	[Overridable]
	public float Top
	{
		get
		{
			return m_rect.yMax;
		}
		set
		{
			m_rect.yMax = value;
			this.OnBoundsChanged?.Invoke();
		}
	}

	[Overridable]
	public float Bottom
	{
		get
		{
			return m_rect.yMin;
		}
		set
		{
			m_rect.yMin = value;
			this.OnBoundsChanged?.Invoke();
		}
	}

	public bool HasBounds => m_hasBounds;

	public FacingDirection Facing
	{
		get
		{
			return m_facingDirection;
		}
		set
		{
			m_facingDirection = value;
		}
	}

	public LayoutBoundsMode BoundsMode
	{
		get
		{
			return m_layoutBoundsMode;
		}
		set
		{
			m_layoutBoundsMode = value;
		}
	}

	public event Action OnBoundsChanged;

	public static Bounds GetLocalBoundsOfWidgetTransform(Transform transform)
	{
		return GetBoundsOfWidgetTransform(transform, Matrix4x4.identity);
	}

	public static Bounds GetBoundsOfWidgetTransform(Transform transform, Matrix4x4 localToTargetMatrix)
	{
		Bounds bounds = new Bounds(Vector3.zero, Vector3.zero);
		WidgetTransform widgetTransform = transform.GetComponent<WidgetTransform>();
		if (widgetTransform == null || widgetTransform.BoundsMode == LayoutBoundsMode.Excluded)
		{
			return bounds;
		}
		Matrix4x4 rotationMatrix = WidgetTransformUtils.GetRotationMatrixFromZNegativeToDesiredFacing(widgetTransform.Facing);
		Matrix4x4 transformMatrix = localToTargetMatrix * rotationMatrix;
		bounds.size = transformMatrix.MultiplyPoint(new Vector3(widgetTransform.Rect.width, widgetTransform.Rect.height, 0f));
		float rectCenterX = (widgetTransform.Rect.xMax + widgetTransform.Rect.xMin) * 0.5f;
		float rectCenterY = (widgetTransform.Rect.yMax + widgetTransform.Rect.yMin) * 0.5f;
		bounds.center = transformMatrix.MultiplyPoint(new Vector3(rectCenterX, rectCenterY, 0f));
		return bounds;
	}

	public static Bounds GetWorldBoundsOfWidgetTransforms(Transform transform)
	{
		return GetBoundsOfWidgetTransforms(transform, Matrix4x4.identity);
	}

	public static Bounds GetBoundsOfWidgetTransforms(Transform transform, Matrix4x4 worldToTargetMatrix)
	{
		Bounds bounds = new Bounds(Vector3.zero, Vector3.zero);
		bool hasInitBounds = false;
		GameObjectUtils.WalkSelfAndChildren(transform, delegate(Transform current)
		{
			WidgetTransform component = current.GetComponent<WidgetTransform>();
			if (component == null || component.BoundsMode == LayoutBoundsMode.Excluded)
			{
				return true;
			}
			if (!component.enabled || !component.gameObject.activeInHierarchy)
			{
				return false;
			}
			Matrix4x4 rotationMatrixFromZNegativeToDesiredFacing = WidgetTransformUtils.GetRotationMatrixFromZNegativeToDesiredFacing(component.Facing);
			Matrix4x4 localToWorldMatrix = component.gameObject.transform.localToWorldMatrix;
			Matrix4x4 matrix4x = worldToTargetMatrix * localToWorldMatrix * rotationMatrixFromZNegativeToDesiredFacing;
			Rect rect = component.Rect;
			Vector3 vector = matrix4x.MultiplyPoint(new Vector3(rect.xMin, rect.yMin, 0f));
			Vector3 point = matrix4x.MultiplyPoint(new Vector3(rect.xMax, rect.yMin, 0f));
			Vector3 point2 = matrix4x.MultiplyPoint(new Vector3(rect.xMin, rect.yMax, 0f));
			Vector3 point3 = matrix4x.MultiplyPoint(new Vector3(rect.xMax, rect.yMax, 0f));
			if (!hasInitBounds)
			{
				bounds = new Bounds(vector, Vector3.zero);
				hasInitBounds = true;
			}
			bounds.Encapsulate(vector);
			bounds.Encapsulate(point);
			bounds.Encapsulate(point2);
			bounds.Encapsulate(point3);
			return component.BoundsMode != LayoutBoundsMode.NonRecursive;
		});
		return bounds;
	}

	public static Bounds GetBoundsOfWidgetTransforms(Transform transform, Transform targetParentTransform)
	{
		Bounds bounds = new Bounds(Vector3.zero, Vector3.zero);
		bool hasInitBounds = false;
		GameObjectUtils.WalkSelfAndChildren(transform, delegate(Transform current)
		{
			WidgetTransform component = current.GetComponent<WidgetTransform>();
			if (component == null || component.BoundsMode == LayoutBoundsMode.Excluded)
			{
				return true;
			}
			if (!component.enabled || !component.gameObject.activeInHierarchy)
			{
				return false;
			}
			Matrix4x4 transformRelativeToTargetParent = GetTransformRelativeToTargetParent(component.gameObject.transform, targetParentTransform);
			Matrix4x4 rotationMatrixFromZNegativeToDesiredFacing = WidgetTransformUtils.GetRotationMatrixFromZNegativeToDesiredFacing(component.Facing);
			Matrix4x4 matrix4x = transformRelativeToTargetParent * rotationMatrixFromZNegativeToDesiredFacing;
			Rect rect = component.Rect;
			Vector3 vector = matrix4x.MultiplyPoint(new Vector3(rect.xMin, rect.yMin, 0f));
			Vector3 point = matrix4x.MultiplyPoint(new Vector3(rect.xMax, rect.yMin, 0f));
			Vector3 point2 = matrix4x.MultiplyPoint(new Vector3(rect.xMin, rect.yMax, 0f));
			Vector3 point3 = matrix4x.MultiplyPoint(new Vector3(rect.xMax, rect.yMax, 0f));
			if (!hasInitBounds)
			{
				bounds = new Bounds(vector, Vector3.zero);
				hasInitBounds = true;
			}
			bounds.Encapsulate(vector);
			bounds.Encapsulate(point);
			bounds.Encapsulate(point2);
			bounds.Encapsulate(point3);
			return component.BoundsMode != LayoutBoundsMode.NonRecursive;
		});
		return bounds;
	}

	private static Matrix4x4 GetTransformRelativeToTargetParent(Transform child, Transform parent)
	{
		Matrix4x4 matrix = Matrix4x4.TRS(child.localPosition, child.localRotation, child.localScale);
		Transform iter = child.parent;
		while (iter != parent)
		{
			if (iter.parent == null)
			{
				throw new Exception("Attempted to generate a matrix from object " + child.name + " to target object " + parent.name + ", but object is not child of parent object!");
			}
			matrix = Matrix4x4.TRS(iter.localPosition, iter.localRotation, iter.localScale) * matrix;
			iter = iter.parent;
		}
		return matrix;
	}

	public static WidgetTransform AddOrUpdateWidgetTransform(GameObject obj, Rect bounds, FacingDirection direction = FacingDirection.YPositive, LayoutBoundsMode boundsMode = LayoutBoundsMode.Recursive)
	{
		WidgetTransform widgetTransform = obj.GetComponent<WidgetTransform>();
		if (widgetTransform == null)
		{
			widgetTransform = obj.AddComponent<WidgetTransform>();
		}
		widgetTransform.m_hasBounds = true;
		widgetTransform.m_layoutBoundsMode = boundsMode;
		widgetTransform.m_rect = bounds;
		return widgetTransform;
	}

	private void OnDrawGizmosSelected()
	{
		DrawGizmos(selected: true);
	}

	private void OnDrawGizmos()
	{
		DrawGizmos(selected: false);
	}

	public void SetWidth(float value)
	{
		m_rect.width = value;
		this.OnBoundsChanged?.Invoke();
	}

	public void SetHeight(float value)
	{
		m_rect.height = value;
		this.OnBoundsChanged?.Invoke();
	}

	private void DrawGizmos(bool selected)
	{
		if (m_hasBounds && base.transform.gameObject.activeInHierarchy)
		{
			Gizmos.matrix = base.transform.localToWorldMatrix;
			Color baseColor = ((base.transform.GetComponent<Clickable>() != null) ? Color.magenta : Color.cyan);
			Gizmos.color = (selected ? baseColor : (baseColor * 0.5f));
			Matrix4x4 rotMatrix = WidgetTransformUtils.GetRotationMatrixFromZNegativeToDesiredFacing(m_facingDirection);
			Gizmos.DrawWireCube(rotMatrix * m_rect.center, rotMatrix * m_rect.size);
		}
	}

	public BoxCollider CreateBoxCollider(GameObject target)
	{
		BoxCollider collider = target.GetComponent<BoxCollider>();
		if (collider == null)
		{
			collider = target.AddComponent<BoxCollider>();
			collider.hideFlags = HideFlags.DontSave;
			SetBoxColliderDimensionsToBounds(collider);
		}
		return collider;
	}

	public void SetBoxColliderDimensionsToBounds(BoxCollider boxCollider)
	{
		Matrix4x4 rotMatrix = WidgetTransformUtils.GetRotationMatrixFromZNegativeToDesiredFacing(m_facingDirection);
		boxCollider.center = rotMatrix * Rect.center;
		boxCollider.size = rotMatrix * Rect.size;
	}
}
