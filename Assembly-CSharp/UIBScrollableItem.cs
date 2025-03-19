using Hearthstone.UI;
using UnityEngine;

public class UIBScrollableItem : MonoBehaviour
{
	public delegate bool ActiveStateCallback();

	public enum ActiveState
	{
		Active,
		Inactive,
		UseHierarchy
	}

	public enum BoundsMode
	{
		Fixed,
		WidgetBoundsLocal,
		WidgetBoundsIncludeChildren
	}

	[Tooltip("Fixed: Use values for Size and Offset defined at edit-time.\n\nWidgetBoundsLocal: Match the size and position of this object's Widget bounds.\n\nWidgetBoundsIncludeChildren: Encapsulate the Widget bounds defined on this object and its children.")]
	public BoundsMode m_boundsMode;

	public Vector3 m_offset = Vector3.zero;

	public Vector3 m_size = Vector3.one;

	public ActiveState m_active;

	private ActiveStateCallback m_activeStateCallback;

	private Vector3[] m_boundsPointTempVector = new Vector3[8];

	private UIBScrollable.IContent m_ScrollableParent;

	public void Awake()
	{
		UpdateScrollableParent();
	}

	public void OnEnable()
	{
		UpdateScrollableParent();
	}

	public void UpdateScrollableParent()
	{
		m_ScrollableParent = GetComponentInParent<UIBScrollable.IContent>();
		if (m_ScrollableParent != null)
		{
			m_ScrollableParent.Scrollable.RegisterScrollableItem(this);
		}
	}

	private void OnDestroy()
	{
		if (m_ScrollableParent != null)
		{
			m_ScrollableParent.Scrollable.RemoveScrollableItem(this);
		}
	}

	public void SetScrollableParent(UIBScrollable.IContent parent)
	{
		m_ScrollableParent = parent;
	}

	public bool IsActive()
	{
		if (m_activeStateCallback != null)
		{
			return m_activeStateCallback();
		}
		if (m_active != 0)
		{
			if (m_active == ActiveState.UseHierarchy)
			{
				return base.gameObject.activeInHierarchy;
			}
			return false;
		}
		return true;
	}

	public void SetCustomActiveState(ActiveStateCallback callback)
	{
		m_activeStateCallback = callback;
	}

	public OrientedBounds GetOrientedBounds()
	{
		Transform tran = base.transform;
		UpdateBounds(tran);
		Matrix4x4 mtx = tran.localToWorldMatrix;
		OrientedBounds orientedBounds = new OrientedBounds();
		orientedBounds.Origin = tran.position + (Vector3)(mtx * m_offset);
		orientedBounds.Extents = new Vector3[3]
		{
			mtx * new Vector3(m_size.x * 0.5f, 0f, 0f),
			mtx * new Vector3(0f, m_size.y * 0.5f, 0f),
			mtx * new Vector3(0f, 0f, m_size.z * 0.5f)
		};
		return orientedBounds;
	}

	public void GetWorldBounds(out Vector3 min, out Vector3 max)
	{
		min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
		max = new Vector3(float.MinValue, float.MinValue, float.MinValue);
		UpdateBoundsPoints();
		for (int i = 0; i < 8; i++)
		{
			Vector3 point = m_boundsPointTempVector[i];
			min.x = Mathf.Min(point.x, min.x);
			min.y = Mathf.Min(point.y, min.y);
			min.z = Mathf.Min(point.z, min.z);
			max.x = Mathf.Max(point.x, max.x);
			max.y = Mathf.Max(point.y, max.y);
			max.z = Mathf.Max(point.z, max.z);
		}
	}

	private void UpdateBoundsPoints()
	{
		Transform tran = base.transform;
		UpdateBounds(tran);
		Matrix4x4 mtx = tran.localToWorldMatrix;
		Vector3 extents0 = mtx * new Vector3(m_size.x * 0.5f, 0f, 0f);
		Vector3 vector = mtx * new Vector3(0f, m_size.y * 0.5f, 0f);
		Vector3 extents2 = mtx * new Vector3(0f, 0f, m_size.z * 0.5f);
		Vector3 vector2 = tran.position + (Vector3)(mtx * m_offset);
		Vector3 centerPlus = vector2 + extents0;
		Vector3 centerMinus = vector2 - extents0;
		Vector3 extentsPlus = vector + extents2;
		Vector3 extentsMinus = vector - extents2;
		m_boundsPointTempVector[0] = centerPlus + extentsPlus;
		m_boundsPointTempVector[1] = centerPlus + extentsMinus;
		m_boundsPointTempVector[2] = centerPlus - extentsPlus;
		m_boundsPointTempVector[3] = centerPlus - extentsMinus;
		m_boundsPointTempVector[4] = centerMinus + extentsPlus;
		m_boundsPointTempVector[5] = centerMinus + extentsMinus;
		m_boundsPointTempVector[6] = centerMinus - extentsPlus;
		m_boundsPointTempVector[7] = centerMinus - extentsMinus;
	}

	private void UpdateBounds(Transform transform)
	{
		if (m_boundsMode == BoundsMode.WidgetBoundsLocal)
		{
			if (GetComponent<WidgetTransform>() != null)
			{
				Bounds bounds = WidgetTransform.GetLocalBoundsOfWidgetTransform(transform);
				m_size = bounds.size;
				m_offset = bounds.center;
			}
		}
		else if (m_boundsMode == BoundsMode.WidgetBoundsIncludeChildren)
		{
			Bounds bounds2 = WidgetTransform.GetBoundsOfWidgetTransforms(transform, transform.worldToLocalMatrix);
			m_size = bounds2.size;
			m_offset = bounds2.center;
		}
	}
}
