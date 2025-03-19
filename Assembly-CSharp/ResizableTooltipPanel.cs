using UnityEngine;

public class ResizableTooltipPanel : TooltipPanel
{
	protected float m_heightPadding = 1f;

	protected float m_bodyTextHeight;

	protected float m_bodyPadding = 0.1f;

	protected float m_totalHeight;

	private string m_tooltipName;

	private string m_tooltipBody;

	protected NewThreeSliceElement m_threeSliceElement;

	public override void Initialize(string keywordName, string keywordText)
	{
		m_tooltipName = keywordName;
		m_tooltipBody = keywordText;
		m_threeSliceElement = m_background.GetComponent<NewThreeSliceElement>();
		if (m_threeSliceElement == null)
		{
			Error.AddDevFatal("Prefab expecting m_background to have a NewThreeSliceElement!");
		}
		base.Initialize(m_tooltipName, m_tooltipBody);
		m_bodyTextHeight = CalculateBodyHeight();
		if (m_initialBackgroundHeight == 0f || m_initialBackgroundScale == Vector3.zero)
		{
			m_initialBackgroundHeight = m_threeSliceElement?.m_middle?.GetComponent<Renderer>()?.bounds.size.z ?? 1f;
			m_initialBackgroundScale = m_threeSliceElement?.m_middle?.transform.localScale ?? Vector3.one;
		}
		CalculateTotalHeight();
		SetBackgroundSize(m_totalHeight);
	}

	protected virtual float CalculateBodyHeight()
	{
		if (m_tooltipBody == "")
		{
			return -0.2f;
		}
		return m_body.GetTextBounds().size.y;
	}

	protected virtual void CalculateTotalHeight()
	{
		if (string.IsNullOrEmpty(m_tooltipName))
		{
			m_totalHeight = (m_bodyTextHeight + m_bodyPadding) * m_heightPadding;
		}
		else
		{
			m_totalHeight = (m_name.Height + m_bodyTextHeight) * m_heightPadding;
		}
	}

	protected void SetBackgroundSize(float totalHeight)
	{
		if (m_threeSliceElement == null)
		{
			Error.AddDevFatal("Prefab expecting a NewThreeSliceElement.");
		}
		else
		{
			m_threeSliceElement.SetSize(new Vector3(m_initialBackgroundScale.x, m_initialBackgroundScale.y * totalHeight / m_initialBackgroundHeight, m_initialBackgroundScale.z));
		}
	}

	public override float GetHeight()
	{
		if (m_threeSliceElement == null)
		{
			Error.AddDevFatal("Prefab expecting a NewThreeSliceElement.");
		}
		if (m_threeSliceElement.m_leftOrTop == null || m_threeSliceElement.m_middle == null || m_threeSliceElement.m_rightOrBottom == null)
		{
			Error.AddDevFatal("Resizable Tooltip Panel on " + base.gameObject.name + " has an undefined element within its parts.");
		}
		float num = m_threeSliceElement?.m_leftOrTop?.GetComponent<Renderer>()?.bounds.size.z ?? 1f;
		float middleSize = m_threeSliceElement?.m_middle?.GetComponent<Renderer>()?.bounds.size.z ?? 1f;
		float bottomSize = m_threeSliceElement?.m_rightOrBottom?.GetComponent<Renderer>()?.bounds.size.z ?? 1f;
		return num + middleSize + bottomSize;
	}

	public override float GetWidth()
	{
		if (m_threeSliceElement == null || m_threeSliceElement.m_leftOrTop == null)
		{
			Error.AddDevFatal("Missing Three Slice Element information on " + base.gameObject.name + ".");
			return 1f;
		}
		return m_threeSliceElement.m_leftOrTop.GetComponent<Renderer>()?.bounds.size.x ?? 1f;
	}
}
