using Hearthstone.UI.Core;
using UnityEngine;

public class TexturePanAndZoom : MonoBehaviour
{
	private Vector2 m_startScale;

	private Vector2 m_targetScale;

	private Vector2 m_startOffset;

	private Vector2 m_targetOffset;

	private float m_startTime = -1f;

	public float m_transitionTime;

	public AnimationCurve m_transitionEase = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

	public Material m_targetMaterial;

	[Overridable]
	public float TargetScaleX
	{
		get
		{
			return m_targetScale.x;
		}
		set
		{
			m_targetScale = new Vector2(value, m_targetScale.y);
			m_startScale = m_targetMaterial.mainTextureScale;
			m_startTime = Time.time;
		}
	}

	[Overridable]
	public float TargetScaleY
	{
		get
		{
			return m_targetScale.y;
		}
		set
		{
			m_targetScale = new Vector2(m_targetScale.x, value);
			m_startScale = m_targetMaterial.mainTextureScale;
			m_startTime = Time.time;
		}
	}

	[Overridable]
	public float TargetOffsetX
	{
		get
		{
			return m_targetOffset.x;
		}
		set
		{
			m_targetOffset = new Vector2(value, m_targetOffset.y);
			m_startOffset = m_targetMaterial.mainTextureOffset;
			m_startTime = Time.time;
		}
	}

	[Overridable]
	public float TargetOffsetY
	{
		get
		{
			return m_targetOffset.y;
		}
		set
		{
			m_targetOffset = new Vector2(m_targetOffset.x, value);
			m_startOffset = m_targetMaterial.mainTextureOffset;
			m_startTime = Time.time;
		}
	}

	private bool IsPanAndZooming()
	{
		return m_startTime >= 0f;
	}

	private void Update()
	{
		if (IsPanAndZooming())
		{
			float elapsedTime = Time.time - m_startTime;
			float elapsedFraction = ((m_transitionTime <= Mathf.Epsilon) ? 1f : (elapsedTime / m_transitionTime));
			if (elapsedFraction >= 1f)
			{
				elapsedFraction = 1f;
				m_startTime = -1f;
			}
			float transitionFraction = m_transitionEase.Evaluate(elapsedFraction);
			m_targetMaterial.mainTextureScale = Vector2.LerpUnclamped(m_startScale, m_targetScale, transitionFraction);
			m_targetMaterial.mainTextureOffset = Vector2.LerpUnclamped(m_startOffset, m_targetOffset, transitionFraction);
		}
	}
}
