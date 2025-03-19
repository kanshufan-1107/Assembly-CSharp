using UnityEngine;

[ExecuteAlways]
public class NewNineSliceElement : MonoBehaviour
{
	public enum PinnedPoint
	{
		TOPLEFT,
		TOP,
		TOPRIGHT,
		LEFT,
		MIDDLE,
		RIGHT,
		BOTTOMLEFT,
		BOTTOM,
		BOTTOMRIGHT
	}

	public enum PlaneAxis
	{
		XY,
		XZ
	}

	public enum Mode
	{
		UseMiddleScale,
		UseSize
	}

	public GameObject m_topLeft;

	public GameObject m_top;

	public GameObject m_topRight;

	public GameObject m_left;

	public GameObject m_right;

	public GameObject m_middle;

	public GameObject m_bottomLeft;

	public GameObject m_bottom;

	public GameObject m_bottomRight;

	public GameObject m_anchorBone;

	public PinnedPoint m_pinnedPoint = PinnedPoint.TOP;

	public PlaneAxis m_planeAxis = PlaneAxis.XZ;

	public Vector3 m_pinnedPointOffset;

	public Vector2 m_middleScale;

	public Vector2 m_size;

	public Mode m_mode;

	public virtual void SetSize(float width, float height)
	{
		if (m_mode == Mode.UseSize)
		{
			m_size = new Vector2(width, height);
			Vector3 topLeftSize = m_topLeft.GetComponent<Renderer>().bounds.size;
			Vector3 bottomRightSize = m_bottomLeft.GetComponent<Renderer>().bounds.size;
			width = Mathf.Max(width - (topLeftSize.x + bottomRightSize.x), 1f);
			height = Mathf.Max(height - (topLeftSize.y + bottomRightSize.y), 1f);
			SetPieceWidth(m_middle, width);
			SetPieceHeight(m_middle, height);
			SetPieceWidth(m_top, width);
			SetPieceWidth(m_bottom, width);
			SetPieceHeight(m_left, height);
			SetPieceHeight(m_right, height);
		}
		else
		{
			TransformUtil.SetLocalScaleXZ(m_middle, new Vector2(width, height));
			TransformUtil.SetLocalScaleX(m_top, width);
			TransformUtil.SetLocalScaleX(m_bottom, width);
			TransformUtil.SetLocalScaleZ(m_left, height);
			TransformUtil.SetLocalScaleZ(m_right, height);
		}
		Vector3 topLeftAnchor;
		Vector3 bottomLeftAnchor;
		Vector3 leftAnchor;
		Vector3 rightAnchor;
		Vector3 centerAnchor;
		if (m_planeAxis == PlaneAxis.XZ)
		{
			topLeftAnchor = new Vector3(0f, 0f, 1f);
			bottomLeftAnchor = new Vector3(0f, 0f, 0f);
			leftAnchor = new Vector3(0f, 0f, 0.5f);
			rightAnchor = new Vector3(1f, 0f, 0.5f);
			centerAnchor = new Vector3(0.5f, 0f, 0.5f);
		}
		else
		{
			topLeftAnchor = new Vector3(0f, 1f, 0f);
			bottomLeftAnchor = new Vector3(0f, 0f, 0f);
			leftAnchor = new Vector3(0f, 0.5f, 0f);
			rightAnchor = new Vector3(1f, 0.5f, 0f);
			centerAnchor = new Vector3(0.5f, 0.5f, 0f);
		}
		switch (m_pinnedPoint)
		{
		case PinnedPoint.TOPLEFT:
			TransformUtil.SetPoint(m_topLeft, Anchor.TOP_LEFT, m_anchorBone, Anchor.TOP_LEFT);
			TransformUtil.SetPoint(m_top, Anchor.LEFT, m_topLeft, Anchor.RIGHT);
			TransformUtil.SetPoint(m_topRight, Anchor.LEFT, m_top, Anchor.RIGHT);
			TransformUtil.SetPoint(m_left, topLeftAnchor, m_topLeft, bottomLeftAnchor);
			TransformUtil.SetPoint(m_middle, Anchor.LEFT, m_left, Anchor.RIGHT);
			TransformUtil.SetPoint(m_right, Anchor.LEFT, m_middle, Anchor.RIGHT);
			TransformUtil.SetPoint(m_bottomLeft, topLeftAnchor, m_left, bottomLeftAnchor);
			TransformUtil.SetPoint(m_bottom, Anchor.LEFT, m_bottomLeft, Anchor.RIGHT);
			TransformUtil.SetPoint(m_bottomRight, Anchor.LEFT, m_bottom, Anchor.RIGHT);
			break;
		case PinnedPoint.TOP:
			TransformUtil.SetPoint(m_top, Anchor.TOP, m_anchorBone, Anchor.TOP);
			TransformUtil.SetPoint(m_topLeft, Anchor.RIGHT, m_top, Anchor.LEFT);
			TransformUtil.SetPoint(m_topRight, Anchor.LEFT, m_top, Anchor.RIGHT);
			TransformUtil.SetPoint(m_left, topLeftAnchor, m_topLeft, bottomLeftAnchor);
			TransformUtil.SetPoint(m_middle, Anchor.LEFT, m_left, Anchor.RIGHT);
			TransformUtil.SetPoint(m_right, Anchor.LEFT, m_middle, Anchor.RIGHT);
			TransformUtil.SetPoint(m_bottomLeft, topLeftAnchor, m_left, bottomLeftAnchor);
			TransformUtil.SetPoint(m_bottom, Anchor.LEFT, m_bottomLeft, Anchor.RIGHT);
			TransformUtil.SetPoint(m_bottomRight, Anchor.LEFT, m_bottom, Anchor.RIGHT);
			break;
		case PinnedPoint.TOPRIGHT:
			TransformUtil.SetPoint(m_topRight, Anchor.TOP_RIGHT, m_anchorBone, Anchor.TOP_RIGHT);
			TransformUtil.SetPoint(m_top, Anchor.RIGHT, m_topRight, Anchor.LEFT);
			TransformUtil.SetPoint(m_topLeft, Anchor.RIGHT, m_top, Anchor.LEFT);
			TransformUtil.SetPoint(m_left, topLeftAnchor, m_topLeft, bottomLeftAnchor);
			TransformUtil.SetPoint(m_middle, Anchor.LEFT, m_left, Anchor.RIGHT);
			TransformUtil.SetPoint(m_right, Anchor.LEFT, m_middle, Anchor.RIGHT);
			TransformUtil.SetPoint(m_bottomLeft, topLeftAnchor, m_left, bottomLeftAnchor);
			TransformUtil.SetPoint(m_bottom, Anchor.LEFT, m_bottomLeft, Anchor.RIGHT);
			TransformUtil.SetPoint(m_bottomRight, Anchor.LEFT, m_bottom, Anchor.RIGHT);
			break;
		case PinnedPoint.LEFT:
			TransformUtil.SetPoint(m_left, leftAnchor, m_anchorBone, leftAnchor);
			TransformUtil.SetPoint(m_middle, Anchor.LEFT, m_left, Anchor.RIGHT);
			TransformUtil.SetPoint(m_right, Anchor.LEFT, m_middle, Anchor.RIGHT);
			TransformUtil.SetPoint(m_topLeft, bottomLeftAnchor, m_left, topLeftAnchor);
			TransformUtil.SetPoint(m_top, Anchor.LEFT, m_topLeft, Anchor.RIGHT);
			TransformUtil.SetPoint(m_topRight, Anchor.LEFT, m_top, Anchor.RIGHT);
			TransformUtil.SetPoint(m_bottomLeft, topLeftAnchor, m_left, bottomLeftAnchor);
			TransformUtil.SetPoint(m_bottom, Anchor.LEFT, m_bottomLeft, Anchor.RIGHT);
			TransformUtil.SetPoint(m_bottomRight, Anchor.LEFT, m_bottom, Anchor.RIGHT);
			break;
		case PinnedPoint.MIDDLE:
			TransformUtil.SetPoint(m_middle, centerAnchor, m_anchorBone, centerAnchor);
			TransformUtil.SetPoint(m_left, Anchor.RIGHT, m_middle, Anchor.LEFT);
			TransformUtil.SetPoint(m_right, Anchor.LEFT, m_middle, Anchor.RIGHT);
			TransformUtil.SetPoint(m_topLeft, bottomLeftAnchor, m_left, topLeftAnchor);
			TransformUtil.SetPoint(m_top, Anchor.LEFT, m_topLeft, Anchor.RIGHT);
			TransformUtil.SetPoint(m_topRight, Anchor.LEFT, m_top, Anchor.RIGHT);
			TransformUtil.SetPoint(m_bottomLeft, topLeftAnchor, m_left, bottomLeftAnchor);
			TransformUtil.SetPoint(m_bottom, Anchor.LEFT, m_bottomLeft, Anchor.RIGHT);
			TransformUtil.SetPoint(m_bottomRight, Anchor.LEFT, m_bottom, Anchor.RIGHT);
			break;
		case PinnedPoint.RIGHT:
			TransformUtil.SetPoint(m_right, rightAnchor, m_anchorBone, rightAnchor);
			TransformUtil.SetPoint(m_middle, Anchor.RIGHT, m_right, Anchor.LEFT);
			TransformUtil.SetPoint(m_left, Anchor.RIGHT, m_middle, Anchor.LEFT);
			TransformUtil.SetPoint(m_topLeft, bottomLeftAnchor, m_left, topLeftAnchor);
			TransformUtil.SetPoint(m_top, Anchor.LEFT, m_topLeft, Anchor.RIGHT);
			TransformUtil.SetPoint(m_topRight, Anchor.LEFT, m_top, Anchor.RIGHT);
			TransformUtil.SetPoint(m_bottomLeft, topLeftAnchor, m_left, bottomLeftAnchor);
			TransformUtil.SetPoint(m_bottom, Anchor.LEFT, m_bottomLeft, Anchor.RIGHT);
			TransformUtil.SetPoint(m_bottomRight, Anchor.LEFT, m_bottom, Anchor.RIGHT);
			break;
		case PinnedPoint.BOTTOMLEFT:
			TransformUtil.SetPoint(m_bottomLeft, Anchor.BOTTOM_LEFT, m_anchorBone, Anchor.BOTTOM_LEFT);
			TransformUtil.SetPoint(m_bottom, Anchor.LEFT, m_bottomLeft, Anchor.RIGHT);
			TransformUtil.SetPoint(m_bottomRight, Anchor.LEFT, m_bottom, Anchor.RIGHT);
			TransformUtil.SetPoint(m_left, bottomLeftAnchor, m_bottomLeft, topLeftAnchor);
			TransformUtil.SetPoint(m_middle, Anchor.LEFT, m_left, Anchor.RIGHT);
			TransformUtil.SetPoint(m_right, Anchor.LEFT, m_middle, Anchor.RIGHT);
			TransformUtil.SetPoint(m_topLeft, bottomLeftAnchor, m_left, topLeftAnchor);
			TransformUtil.SetPoint(m_top, Anchor.LEFT, m_topLeft, Anchor.RIGHT);
			TransformUtil.SetPoint(m_topRight, Anchor.LEFT, m_top, Anchor.RIGHT);
			break;
		case PinnedPoint.BOTTOM:
			TransformUtil.SetPoint(m_bottom, Anchor.BOTTOM, m_anchorBone, Anchor.BOTTOM);
			TransformUtil.SetPoint(m_bottomLeft, Anchor.RIGHT, m_bottom, Anchor.LEFT);
			TransformUtil.SetPoint(m_bottomRight, Anchor.LEFT, m_bottom, Anchor.RIGHT);
			TransformUtil.SetPoint(m_left, bottomLeftAnchor, m_bottomLeft, topLeftAnchor);
			TransformUtil.SetPoint(m_middle, Anchor.LEFT, m_left, Anchor.RIGHT);
			TransformUtil.SetPoint(m_right, Anchor.LEFT, m_middle, Anchor.RIGHT);
			TransformUtil.SetPoint(m_topLeft, bottomLeftAnchor, m_left, topLeftAnchor);
			TransformUtil.SetPoint(m_top, Anchor.LEFT, m_topLeft, Anchor.RIGHT);
			TransformUtil.SetPoint(m_topRight, Anchor.LEFT, m_top, Anchor.RIGHT);
			break;
		case PinnedPoint.BOTTOMRIGHT:
			TransformUtil.SetPoint(m_bottomRight, Anchor.BOTTOM_RIGHT, m_anchorBone, Anchor.BOTTOM_RIGHT);
			TransformUtil.SetPoint(m_bottom, Anchor.RIGHT, m_bottomRight, Anchor.LEFT);
			TransformUtil.SetPoint(m_bottomLeft, Anchor.RIGHT, m_bottom, Anchor.LEFT);
			TransformUtil.SetPoint(m_left, bottomLeftAnchor, m_bottomLeft, topLeftAnchor);
			TransformUtil.SetPoint(m_middle, Anchor.LEFT, m_left, Anchor.RIGHT);
			TransformUtil.SetPoint(m_right, Anchor.LEFT, m_middle, Anchor.RIGHT);
			TransformUtil.SetPoint(m_topLeft, bottomLeftAnchor, m_left, topLeftAnchor);
			TransformUtil.SetPoint(m_top, Anchor.LEFT, m_topLeft, Anchor.RIGHT);
			TransformUtil.SetPoint(m_topRight, Anchor.LEFT, m_top, Anchor.RIGHT);
			break;
		}
	}

	private void SetPieceWidth(GameObject piece, float width)
	{
		Vector3 localScale = piece.transform.localScale;
		localScale.x = width * piece.transform.localScale.x / piece.GetComponent<Renderer>().bounds.size.x;
		piece.transform.localScale = localScale;
	}

	private void SetPieceHeight(GameObject piece, float height)
	{
		Vector3 localScale = piece.transform.localScale;
		localScale.z = height * piece.transform.localScale.z / piece.GetComponent<Renderer>().bounds.size.y;
		piece.transform.localScale = localScale;
	}
}
