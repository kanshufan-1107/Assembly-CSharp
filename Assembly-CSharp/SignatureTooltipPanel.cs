using UnityEngine;

public class SignatureTooltipPanel : ResizableTooltipPanel
{
	[SerializeField]
	private GameObject m_silenceIcon;

	[SerializeField]
	private GameObject m_tribePlateContainer;

	[SerializeField]
	private GameObject m_singleTribePlate;

	[SerializeField]
	private GameObject m_multiTribePlate;

	[SerializeField]
	private float m_minimumBodyTextHeight = 1f;

	private UberText m_tribeText;

	private bool m_showTribe;

	private bool m_multiTribe;

	private const float TRIBE_OVERLAP_DISTANCE = 0.147f;

	public void SetTribe(string text, bool isMulti)
	{
		m_showTribe = !string.IsNullOrEmpty(text);
		m_singleTribePlate.SetActive(value: false);
		m_multiTribePlate.SetActive(value: false);
		if (m_showTribe)
		{
			m_multiTribe = isMulti;
			if (m_multiTribe)
			{
				m_multiTribePlate.SetActive(value: true);
				m_tribeText = m_multiTribePlate.GetComponentInChildren<UberText>();
			}
			else
			{
				m_singleTribePlate.SetActive(value: true);
				m_tribeText = m_singleTribePlate.GetComponentInChildren<UberText>();
			}
			m_tribeText.Text = text;
		}
	}

	public void SetSilenced(bool silenced)
	{
		m_silenceIcon.SetActive(silenced);
	}

	public override void Initialize(string name, string description)
	{
		if (m_tribeText != null)
		{
			m_tribeText.UpdateNow();
		}
		base.Initialize(name, description);
	}

	protected override float CalculateBodyHeight()
	{
		float bodyHeight = base.CalculateBodyHeight();
		if (!string.IsNullOrEmpty(m_body.Text) && bodyHeight < m_minimumBodyTextHeight)
		{
			bodyHeight = m_minimumBodyTextHeight;
		}
		return bodyHeight;
	}

	protected override void CalculateTotalHeight()
	{
		base.CalculateTotalHeight();
		if (m_showTribe && m_tribeText != null)
		{
			float tribeHeight = m_tribeText.GetTextBounds().size.y * m_tribeText.transform.parent.transform.localScale.z;
			m_totalHeight = m_totalHeight + tribeHeight + 0.147f;
		}
	}
}
