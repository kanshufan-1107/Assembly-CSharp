using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class LettuceMapLine : MonoBehaviour
{
	public enum ConnectionType
	{
		INVALID,
		NEXT_ROW,
		SAME_ROW
	}

	public LineRenderer m_GlowLineRenderer;

	public float m_VerticalOffsetFromStart = 7f;

	public float m_HorizontalOffsetPerLineConnectingToEndPoint = 2f;

	public float m_VerticalOffsetPerLineFromSameDirection = 2f;

	public float m_CornerRadius = 1f;

	public LineRenderer m_LineRenderer => GetComponent<LineRenderer>();

	public Transform m_StartBone { get; set; }

	public Transform m_EndBone { get; set; }

	public ConnectionType m_ConnectionType { get; set; }

	public int m_ConnectionIndex { get; set; }

	public int m_NumParentConnectionsComingFromLeft { get; set; }

	public int m_NumParentConnectionsComingFromRight { get; set; }

	public void RefreshLine()
	{
		Vector3[] linePoints = CalculateLinePoints();
		m_LineRenderer.positionCount = linePoints.Length;
		m_LineRenderer.SetPositions(linePoints);
		if (m_GlowLineRenderer != null)
		{
			m_GlowLineRenderer.positionCount = linePoints.Length;
			m_GlowLineRenderer.SetPositions(linePoints);
		}
	}

	private Vector3[] CalculateLinePoints()
	{
		int totalParentConnections = m_NumParentConnectionsComingFromLeft + m_NumParentConnectionsComingFromRight;
		Vector3 startPosition = base.transform.InverseTransformPoint(m_StartBone.position);
		Vector3 endPosition = base.transform.InverseTransformPoint(m_EndBone.position);
		Vector3[] linePoints = new Vector3[0];
		float horizontalOffset = ((totalParentConnections % 2 == 0) ? (m_HorizontalOffsetPerLineConnectingToEndPoint / 2f) : 0f);
		horizontalOffset += m_HorizontalOffsetPerLineConnectingToEndPoint * (float)(m_ConnectionIndex - totalParentConnections / 2);
		float verticalOffsetBetweenLines = 0f;
		verticalOffsetBetweenLines = ((m_ConnectionIndex >= m_NumParentConnectionsComingFromLeft) ? ((float)(m_ConnectionIndex - m_NumParentConnectionsComingFromLeft) * m_VerticalOffsetPerLineFromSameDirection) : ((float)(m_NumParentConnectionsComingFromLeft - 1 - m_ConnectionIndex) * m_VerticalOffsetPerLineFromSameDirection));
		switch (m_ConnectionType)
		{
		case ConnectionType.NEXT_ROW:
		{
			Vector3 endPositionWithOffset = endPosition + new Vector3(horizontalOffset, 0f, 0f);
			if (Mathf.Abs(startPosition.x - endPositionWithOffset.x) < m_CornerRadius)
			{
				linePoints = new Vector3[2] { startPosition, endPositionWithOffset };
				break;
			}
			int horizontalMultiplier = ((!(startPosition.x > endPositionWithOffset.x)) ? 1 : (-1));
			Vector3 firstCornerCenter = startPosition + new Vector3(0f, 0f, m_VerticalOffsetFromStart + verticalOffsetBetweenLines) + new Vector3(m_CornerRadius * (float)horizontalMultiplier, 0f, 0f - m_CornerRadius);
			Vector3 firstCornerStart = firstCornerCenter + Vector3.left * m_CornerRadius * horizontalMultiplier;
			Vector3 firstCornerEnd = firstCornerCenter + Vector3.forward * m_CornerRadius;
			Vector3 secondCornerCenter = new Vector3(endPositionWithOffset.x, startPosition.y, startPosition.z + m_VerticalOffsetFromStart + verticalOffsetBetweenLines) + new Vector3((0f - m_CornerRadius) * (float)horizontalMultiplier, 0f, m_CornerRadius);
			Vector3 secondCornerStart = secondCornerCenter + Vector3.back * m_CornerRadius;
			Vector3 secondCornerEnd = secondCornerCenter + Vector3.right * m_CornerRadius * horizontalMultiplier;
			linePoints = new Vector3[10]
			{
				startPosition,
				firstCornerCenter + Vector3.Slerp(firstCornerStart - firstCornerCenter, firstCornerEnd - firstCornerCenter, 0f),
				firstCornerCenter + Vector3.Slerp(firstCornerStart - firstCornerCenter, firstCornerEnd - firstCornerCenter, 0.33f),
				firstCornerCenter + Vector3.Slerp(firstCornerStart - firstCornerCenter, firstCornerEnd - firstCornerCenter, 0.66f),
				firstCornerCenter + Vector3.Slerp(firstCornerStart - firstCornerCenter, firstCornerEnd - firstCornerCenter, 1f),
				secondCornerCenter + Vector3.Slerp(secondCornerStart - secondCornerCenter, secondCornerEnd - secondCornerCenter, 0f),
				secondCornerCenter + Vector3.Slerp(secondCornerStart - secondCornerCenter, secondCornerEnd - secondCornerCenter, 0.33f),
				secondCornerCenter + Vector3.Slerp(secondCornerStart - secondCornerCenter, secondCornerEnd - secondCornerCenter, 0.66f),
				secondCornerCenter + Vector3.Slerp(secondCornerStart - secondCornerCenter, secondCornerEnd - secondCornerCenter, 1f),
				endPositionWithOffset
			};
			break;
		}
		case ConnectionType.SAME_ROW:
			linePoints = new Vector3[2] { startPosition, endPosition };
			break;
		}
		return linePoints;
	}
}
