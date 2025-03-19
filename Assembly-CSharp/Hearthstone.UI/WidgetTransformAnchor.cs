using Hearthstone.UI.Core;
using UnityEngine;

namespace Hearthstone.UI;

public class WidgetTransformAnchor : WidgetPositioningElement
{
	public enum AnchorPoint
	{
		None,
		Center,
		Left,
		Right,
		Top,
		Bottom,
		TopLeft,
		TopRight,
		BottomLeft,
		BottomRight
	}

	[SerializeField]
	[Header("Components")]
	private WidgetTransform m_targetWidgetTransform;

	[SerializeField]
	private WidgetTransform m_parentWidgetTransform;

	[SerializeField]
	[Header("Anchor")]
	private AnchorPoint m_anchorPoint;

	[SerializeField]
	private bool m_stretchX;

	[SerializeField]
	private bool m_stretchY;

	[SerializeField]
	[Header("Anchor Offsets")]
	private Vector2 m_originOffset;

	[SerializeField]
	[Header("Stretch Offsets")]
	private float m_leftPercentage = 1f;

	[SerializeField]
	private float m_rightPercentage = 1f;

	[SerializeField]
	private float m_topPercentage = 1f;

	[SerializeField]
	private float m_bottomPercentage = 1f;

	[SerializeField]
	[Header("Max Stretch")]
	private float m_leftMax = -1000f;

	[SerializeField]
	private float m_rightMax = 1000f;

	[SerializeField]
	private float m_topMax = 1000f;

	[SerializeField]
	private float m_bottomMax = -1000f;

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
		if (m_parentWidgetTransform != null)
		{
			m_parentWidgetTransform.OnBoundsChanged += base.Refresh;
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (m_parentWidgetTransform != null)
		{
			m_parentWidgetTransform.OnBoundsChanged -= base.Refresh;
		}
	}

	protected override void InternalRefresh()
	{
		if (m_parentWidgetTransform == null || m_targetWidgetTransform == null)
		{
			return;
		}
		Transform parentTransform = m_parentWidgetTransform.transform;
		Transform targetTransform = m_targetWidgetTransform.transform;
		Vector3 targetLocalPosition = parentTransform.InverseTransformPoint(targetTransform.position);
		float localX = targetLocalPosition.x;
		float localY = targetLocalPosition.z;
		float left = m_targetWidgetTransform.Left;
		float right = m_targetWidgetTransform.Right;
		float top = m_targetWidgetTransform.Top;
		float bottom = m_targetWidgetTransform.Bottom;
		if (IsMatchingAnchorPoint(m_anchorPoint, AnchorPoint.Left))
		{
			localX = m_parentWidgetTransform.Left;
			if (m_stretchX)
			{
				left = 0f;
				right = m_parentWidgetTransform.Rect.width;
			}
		}
		else if (IsMatchingAnchorPoint(m_anchorPoint, AnchorPoint.Right))
		{
			localX = m_parentWidgetTransform.Right;
			if (m_stretchX)
			{
				right = 0f;
				left = 0f - m_parentWidgetTransform.Rect.width;
			}
		}
		else
		{
			if (m_anchorPoint == AnchorPoint.Center)
			{
				localX = 0f;
			}
			if (m_stretchX)
			{
				right = m_parentWidgetTransform.Right;
				left = m_parentWidgetTransform.Left;
			}
		}
		if (IsMatchingAnchorPoint(m_anchorPoint, AnchorPoint.Top))
		{
			localY = m_parentWidgetTransform.Top;
			if (m_stretchY)
			{
				top = 0f;
				bottom = 0f - m_parentWidgetTransform.Rect.height;
			}
		}
		else if (IsMatchingAnchorPoint(m_anchorPoint, AnchorPoint.Bottom))
		{
			localY = m_parentWidgetTransform.Bottom;
			if (m_stretchY)
			{
				bottom = 0f;
				top = m_parentWidgetTransform.Rect.height;
			}
		}
		else
		{
			if (m_anchorPoint == AnchorPoint.Center)
			{
				localY = 0f;
			}
			if (m_stretchY)
			{
				top = m_parentWidgetTransform.Top;
				bottom = m_parentWidgetTransform.Bottom;
			}
		}
		if (m_stretchX)
		{
			m_targetWidgetTransform.Left = Mathf.Max(left * m_leftPercentage, m_leftMax);
			m_targetWidgetTransform.Right = Mathf.Min(right * m_rightPercentage, m_rightMax);
		}
		if (m_stretchY)
		{
			m_targetWidgetTransform.Top = Mathf.Min(top * m_topPercentage, m_topMax);
			m_targetWidgetTransform.Bottom = Mathf.Max(bottom * m_bottomPercentage, m_bottomMax);
		}
		if (m_anchorPoint != 0)
		{
			localX += m_originOffset.x;
			localY += m_originOffset.y;
			targetTransform.position = parentTransform.TransformPoint(new Vector3(localX, targetLocalPosition.y, localY));
		}
	}

	private static bool IsMatchingAnchorPoint(AnchorPoint target, AnchorPoint check)
	{
		switch (check)
		{
		case AnchorPoint.Left:
			if (target != AnchorPoint.BottomLeft && target != AnchorPoint.Left)
			{
				return target == AnchorPoint.TopLeft;
			}
			return true;
		case AnchorPoint.Right:
			if (target != AnchorPoint.BottomRight && target != AnchorPoint.Right)
			{
				return target == AnchorPoint.TopRight;
			}
			return true;
		case AnchorPoint.Top:
			if (target != AnchorPoint.TopLeft && target != AnchorPoint.Top)
			{
				return target == AnchorPoint.TopRight;
			}
			return true;
		case AnchorPoint.Bottom:
			if (target != AnchorPoint.BottomLeft && target != AnchorPoint.Bottom)
			{
				return target == AnchorPoint.BottomRight;
			}
			return true;
		default:
			return true;
		}
	}
}
