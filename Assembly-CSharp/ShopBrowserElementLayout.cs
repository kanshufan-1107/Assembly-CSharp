using System;
using System.Collections.Generic;
using Hearthstone.UI;
using UnityEngine;

public class ShopBrowserElementLayout : MonoBehaviour
{
	[Serializable]
	private struct BoundData
	{
		public GameObject Root;

		public WidgetTransform WidgetTransform;

		public Rect WidgetRect => WidgetTransform.Rect;

		public bool IsValid
		{
			get
			{
				if (Root != null)
				{
					return WidgetTransform != null;
				}
				return false;
			}
		}
	}

	[Header("Center")]
	[SerializeField]
	private GameObject m_centerRoot;

	[SerializeField]
	private WidgetTransform m_centerWidgetTransform;

	[SerializeField]
	[Header("Corners")]
	private BoundData m_topLeftCorner;

	[SerializeField]
	private BoundData m_topRightCorner;

	[SerializeField]
	private BoundData m_bottomRightCorner;

	[SerializeField]
	private BoundData m_bottomLeftCorner;

	[Header("Edges")]
	[SerializeField]
	private BoundData m_topEdge;

	[SerializeField]
	private BoundData m_rightEdge;

	[SerializeField]
	private BoundData m_bottomEdge;

	[SerializeField]
	private BoundData m_leftEdge;

	[SerializeField]
	[Header("Positioning")]
	private List<WidgetPositioningElement> m_positioningElements;

	public void UpdateLayout(WidgetTransform target)
	{
		Rect targetRect = target.Rect;
		if (m_topLeftCorner.IsValid)
		{
			float cornerX = target.Left + m_topLeftCorner.WidgetTransform.Right;
			float cornerZ = target.Top + m_topLeftCorner.WidgetTransform.Bottom;
			float yPos = m_topLeftCorner.Root.transform.localPosition.y;
			m_topLeftCorner.Root.transform.localPosition = new Vector3(cornerX, yPos, cornerZ);
		}
		if (m_topRightCorner.IsValid)
		{
			float cornerX2 = target.Right + m_topRightCorner.WidgetTransform.Left;
			float cornerZ2 = target.Top + m_topRightCorner.WidgetTransform.Bottom;
			float yPos2 = m_topRightCorner.Root.transform.localPosition.y;
			m_topRightCorner.Root.transform.localPosition = new Vector3(cornerX2, yPos2, cornerZ2);
		}
		if (m_bottomRightCorner.IsValid)
		{
			float cornerX3 = target.Right + m_bottomRightCorner.WidgetTransform.Left;
			float cornerZ3 = target.Bottom + m_bottomRightCorner.WidgetTransform.Top;
			float yPos3 = m_bottomRightCorner.Root.transform.localPosition.y;
			m_bottomRightCorner.Root.transform.localPosition = new Vector3(cornerX3, yPos3, cornerZ3);
		}
		if (m_bottomLeftCorner.IsValid)
		{
			float cornerX4 = target.Left + m_bottomLeftCorner.WidgetTransform.Right;
			float cornerZ4 = target.Bottom + m_bottomLeftCorner.WidgetTransform.Top;
			float yPos4 = m_bottomLeftCorner.Root.transform.localPosition.y;
			m_bottomLeftCorner.Root.transform.localPosition = new Vector3(cornerX4, yPos4, cornerZ4);
		}
		if (m_topEdge.IsValid)
		{
			float zPos = target.Top + m_topEdge.WidgetTransform.Bottom;
			float yPos5 = m_topEdge.Root.transform.localPosition.y;
			m_topEdge.Root.transform.localPosition = new Vector3(0f, yPos5, zPos);
			float width = targetRect.width;
			if (m_topLeftCorner.IsValid)
			{
				width -= m_topLeftCorner.WidgetRect.width;
			}
			if (m_topRightCorner.IsValid)
			{
				width -= m_topRightCorner.WidgetRect.width;
			}
			m_topEdge.WidgetTransform.Left = (0f - width) / 2f;
			m_topEdge.WidgetTransform.Right = width / 2f;
		}
		if (m_rightEdge.IsValid)
		{
			float xPos = target.Right + m_rightEdge.WidgetTransform.Left;
			float yPos6 = m_rightEdge.Root.transform.localPosition.y;
			m_rightEdge.Root.transform.localPosition = new Vector3(xPos, yPos6, 0f);
			float height = targetRect.height;
			if (m_topRightCorner.IsValid)
			{
				height -= m_topRightCorner.WidgetRect.height;
			}
			if (m_bottomRightCorner.IsValid)
			{
				height -= m_bottomRightCorner.WidgetRect.height;
			}
			m_rightEdge.WidgetTransform.Top = height / 2f;
			m_rightEdge.WidgetTransform.Bottom = (0f - height) / 2f;
		}
		if (m_bottomEdge.IsValid)
		{
			float zPos2 = target.Bottom + m_bottomEdge.WidgetTransform.Top;
			float yPos7 = m_bottomEdge.Root.transform.localPosition.y;
			m_bottomEdge.Root.transform.localPosition = new Vector3(0f, yPos7, zPos2);
			float width2 = targetRect.width;
			if (m_bottomLeftCorner.IsValid)
			{
				width2 -= m_bottomLeftCorner.WidgetRect.width;
			}
			if (m_bottomRightCorner.IsValid)
			{
				width2 -= m_bottomRightCorner.WidgetRect.width;
			}
			m_bottomEdge.WidgetTransform.Left = (0f - width2) / 2f;
			m_bottomEdge.WidgetTransform.Right = width2 / 2f;
		}
		if (m_leftEdge.IsValid)
		{
			float xPos2 = target.Left + m_leftEdge.WidgetTransform.Right;
			float yPos8 = m_leftEdge.Root.transform.localPosition.y;
			m_leftEdge.Root.transform.localPosition = new Vector3(xPos2, yPos8, 0f);
			float height2 = targetRect.height;
			if (m_topLeftCorner.IsValid)
			{
				height2 -= m_topLeftCorner.WidgetRect.height;
			}
			if (m_bottomLeftCorner.IsValid)
			{
				height2 -= m_bottomLeftCorner.WidgetRect.height;
			}
			m_leftEdge.WidgetTransform.Top = height2 / 2f;
			m_leftEdge.WidgetTransform.Bottom = (0f - height2) / 2f;
		}
		if (m_centerRoot != null && m_centerWidgetTransform != null)
		{
			float yPos9 = m_centerRoot.transform.localPosition.y;
			m_centerRoot.transform.localPosition = new Vector3(0f, yPos9, 0f);
		}
		if (m_centerWidgetTransform != null)
		{
			float height3 = targetRect.height;
			float width3 = targetRect.width;
			float top = height3 / 2f;
			if (m_topEdge.IsValid)
			{
				top -= m_topEdge.WidgetRect.height;
			}
			m_centerWidgetTransform.Top = top;
			float right = width3 / 2f;
			if (m_rightEdge.IsValid)
			{
				right -= m_rightEdge.WidgetRect.width;
			}
			m_centerWidgetTransform.Right = right;
			float bottom = (0f - height3) / 2f;
			if (m_bottomEdge.IsValid)
			{
				bottom += m_bottomEdge.WidgetRect.height;
			}
			m_centerWidgetTransform.Bottom = bottom;
			float left = (0f - width3) / 2f;
			if (m_leftEdge.IsValid)
			{
				left += m_leftEdge.WidgetRect.width;
			}
			m_centerWidgetTransform.Left = left;
		}
		foreach (WidgetPositioningElement positioningElement in m_positioningElements)
		{
			if (positioningElement != null)
			{
				positioningElement.Refresh();
			}
		}
	}
}
