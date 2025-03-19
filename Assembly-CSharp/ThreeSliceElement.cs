using UnityEngine;

[ExecuteAlways]
public class ThreeSliceElement : MonoBehaviour
{
	public enum PinnedPoint
	{
		LEFT,
		MIDDLE,
		RIGHT,
		TOP,
		BOTTOM
	}

	public enum Direction
	{
		X,
		Y,
		Z
	}

	public GameObject m_left;

	public GameObject m_middle;

	public GameObject m_right;

	public PinnedPoint m_pinnedPoint;

	public Vector3 m_pinnedPointOffset;

	public Direction m_direction;

	public float m_width;

	public float m_middleScale = 1f;

	public Vector3_MobileOverride m_leftOffset;

	public Vector3_MobileOverride m_middleOffset;

	public Vector3_MobileOverride m_rightOffset;

	private Bounds m_initialMiddleBounds;

	private Vector3 m_initialScale = Vector3.zero;

	private Renderer m_leftRenderer;

	private Renderer m_middleRenderer;

	private Renderer m_rightRenderer;

	private void Awake()
	{
		bool hasErrors = false;
		if (null == m_left)
		{
			hasErrors = true;
			Debug.LogError("m_left not set");
		}
		if (null == m_middle)
		{
			hasErrors = true;
			Debug.LogError("m_middle not set");
		}
		if (null == m_right)
		{
			hasErrors = true;
			Debug.LogError("m_right not set");
		}
		if (!hasErrors)
		{
			m_leftRenderer = m_left.GetComponent<Renderer>();
			m_middleRenderer = m_middle.GetComponent<Renderer>();
			m_rightRenderer = m_right.GetComponent<Renderer>();
			if ((bool)m_middle)
			{
				SetInitialValues();
			}
		}
	}

	public void UpdateDisplay()
	{
		if (base.enabled)
		{
			if (m_initialMiddleBounds.size == Vector3.zero)
			{
				m_initialMiddleBounds = m_middleRenderer.bounds;
			}
			float middleWidth = m_width - (m_leftRenderer.bounds.size.x + m_rightRenderer.bounds.size.x);
			switch (m_direction)
			{
			case Direction.X:
			{
				Vector3 worldScale = TransformUtil.ComputeWorldScale(m_middle.transform);
				worldScale.x = m_initialScale.x * middleWidth / m_initialMiddleBounds.size.x;
				TransformUtil.SetWorldScale(m_middle.transform, worldScale);
				break;
			}
			}
			switch (m_pinnedPoint)
			{
			case PinnedPoint.RIGHT:
				m_right.transform.localPosition = m_pinnedPointOffset;
				TransformUtil.SetPoint(m_middle, Anchor.RIGHT, m_right, Anchor.LEFT, m_middleOffset);
				TransformUtil.SetPoint(m_left, Anchor.RIGHT, m_middle, Anchor.LEFT, m_leftOffset);
				break;
			case PinnedPoint.LEFT:
				m_left.transform.localPosition = m_pinnedPointOffset;
				TransformUtil.SetPoint(m_middle, Anchor.LEFT, m_left, Anchor.RIGHT, m_middleOffset);
				TransformUtil.SetPoint(m_right, Anchor.LEFT, m_middle, Anchor.RIGHT, m_rightOffset);
				break;
			case PinnedPoint.MIDDLE:
				m_middle.transform.localPosition = m_pinnedPointOffset;
				TransformUtil.SetPoint(m_left, Anchor.RIGHT, m_middle, Anchor.LEFT, m_leftOffset);
				TransformUtil.SetPoint(m_right, Anchor.LEFT, m_middle, Anchor.RIGHT, m_rightOffset);
				break;
			}
		}
	}

	public void SetWidth(float globalWidth)
	{
		m_width = globalWidth;
		UpdateDisplay();
	}

	public void SetMiddleWidth(float globalWidth)
	{
		m_width = globalWidth + m_leftRenderer.bounds.size.x + m_rightRenderer.bounds.size.x;
		UpdateDisplay();
	}

	public Vector3 GetLeftSize()
	{
		return m_leftRenderer.bounds.size;
	}

	public Vector3 GetMiddleSize()
	{
		return m_middleRenderer.bounds.size;
	}

	public Vector3 GetRightSize()
	{
		return m_rightRenderer.bounds.size;
	}

	public Vector3 GetSize()
	{
		return GetSize(zIsHeight: true);
	}

	public Vector3 GetSize(bool zIsHeight)
	{
		Vector3 size = m_leftRenderer.bounds.size;
		Vector3 middleSize = m_middleRenderer.bounds.size;
		Vector3 rightSize = m_rightRenderer.bounds.size;
		float totalWidth = size.x + rightSize.x + middleSize.x;
		float totalHeight = Mathf.Max(Mathf.Max(size.z, middleSize.z), rightSize.z);
		float totalDepth = Mathf.Max(Mathf.Max(size.y, middleSize.y), rightSize.y);
		if (zIsHeight)
		{
			return new Vector3(totalWidth, totalHeight, totalDepth);
		}
		return new Vector3(totalWidth, totalDepth, totalHeight);
	}

	public void SetInitialValues()
	{
		m_initialMiddleBounds = m_middleRenderer.bounds;
		m_initialScale = m_middle.transform.lossyScale;
		m_width = m_middleRenderer.bounds.size.x + m_leftRenderer.bounds.size.x + m_rightRenderer.bounds.size.x;
	}
}
