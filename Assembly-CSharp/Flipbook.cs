using Blizzard.T5.MaterialService.Extensions;
using UnityEngine;

public class Flipbook : MonoBehaviour
{
	public float m_flipbookRate = 15f;

	public bool m_flipbookRandom;

	public Vector2[] m_flipbookOffsets = new Vector2[4]
	{
		new Vector2(0f, 0.5f),
		new Vector2(0.5f, 0.5f),
		new Vector2(0f, 0f),
		new Vector2(0.5f, 0f)
	};

	public bool m_animate = true;

	public bool m_reverse = true;

	public bool m_RandomRateRange;

	public float m_RandomRateMin;

	public float m_RandomRateMax;

	private float m_flipbookFrame;

	private bool m_flipbookReverse;

	private int m_flipbookLastOffset;

	private void Start()
	{
		if (m_RandomRateRange)
		{
			m_flipbookRate = Random.Range(m_RandomRateMin, m_RandomRateMax);
		}
		if (m_flipbookRandom)
		{
			m_flipbookLastOffset = Random.Range(0, m_flipbookOffsets.Length);
			SetIndex(m_flipbookLastOffset);
		}
	}

	private void Update()
	{
		float rate = m_flipbookRate;
		if (rate == 0f)
		{
			return;
		}
		bool playReverse = false;
		if (rate < 0f)
		{
			rate *= -1f;
			playReverse = true;
		}
		if (!m_animate)
		{
			return;
		}
		if (m_flipbookFrame > rate)
		{
			int offsetIdx = 0;
			if (m_flipbookRandom)
			{
				int i = 0;
				do
				{
					offsetIdx = Random.Range(0, m_flipbookOffsets.Length);
					i++;
				}
				while (offsetIdx == m_flipbookLastOffset && i < 100);
				m_flipbookLastOffset = offsetIdx;
			}
			else
			{
				if (playReverse)
				{
					m_flipbookLastOffset -= Mathf.FloorToInt(m_flipbookFrame / rate);
					if (m_flipbookLastOffset < 0)
					{
						m_flipbookLastOffset = Mathf.FloorToInt(m_flipbookOffsets.Length - Mathf.Abs(m_flipbookLastOffset));
						if (m_flipbookLastOffset < 0)
						{
							m_flipbookLastOffset = m_flipbookOffsets.Length - 1;
						}
					}
				}
				else if (!m_flipbookReverse)
				{
					if (m_reverse)
					{
						if (m_flipbookLastOffset >= m_flipbookOffsets.Length - 1)
						{
							m_flipbookLastOffset = m_flipbookOffsets.Length - 1;
							m_flipbookReverse = true;
						}
						else
						{
							m_flipbookLastOffset++;
						}
					}
					else
					{
						m_flipbookLastOffset += Mathf.FloorToInt(m_flipbookFrame / rate);
						if (m_flipbookLastOffset >= m_flipbookOffsets.Length)
						{
							m_flipbookLastOffset = Mathf.FloorToInt(m_flipbookLastOffset - m_flipbookOffsets.Length);
							if (m_flipbookLastOffset >= m_flipbookOffsets.Length)
							{
								m_flipbookLastOffset = 0;
							}
						}
					}
				}
				else if (m_flipbookLastOffset <= 0)
				{
					m_flipbookLastOffset = 1;
					m_flipbookReverse = false;
				}
				else
				{
					m_flipbookLastOffset -= Mathf.FloorToInt(m_flipbookFrame / rate);
					if (m_flipbookLastOffset < 0)
					{
						m_flipbookLastOffset = Mathf.FloorToInt(m_flipbookOffsets.Length - Mathf.Abs(m_flipbookLastOffset));
					}
					if (m_flipbookLastOffset < 0)
					{
						m_flipbookLastOffset = m_flipbookOffsets.Length - 1;
					}
				}
				offsetIdx = m_flipbookLastOffset;
			}
			m_flipbookFrame = 0f;
			SetIndex(offsetIdx);
		}
		m_flipbookFrame += Time.deltaTime * 60f;
	}

	public void SetIndex(int i)
	{
		if (i < 0 || i >= m_flipbookOffsets.Length)
		{
			if (i < 0)
			{
				m_flipbookLastOffset = 0;
			}
			else
			{
				m_flipbookLastOffset = m_flipbookOffsets.Length;
			}
		}
		else
		{
			GetComponent<Renderer>().GetMaterial().SetTextureOffset("_MainTex", m_flipbookOffsets[i]);
		}
	}
}
