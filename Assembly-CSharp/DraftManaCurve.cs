using System.Collections.Generic;
using UnityEngine;

public class DraftManaCurve : MonoBehaviour
{
	public List<ManaCostBar> m_bars;

	private List<int> m_manaCosts;

	private const int MAX_CARDS = 10;

	private const float SIZE_PER_CARD = 0.1f;

	private void Awake()
	{
		ResetBars();
	}

	public void UpdateBars()
	{
		int highestCount = 0;
		foreach (int count in m_manaCosts)
		{
			if (count > highestCount)
			{
				highestCount = count;
			}
		}
		if (highestCount < 10)
		{
			highestCount = 10;
		}
		for (int i = 0; i < m_bars.Count; i++)
		{
			m_bars[i].m_maxValue = highestCount;
			m_bars[i].AnimateBar(m_manaCosts[i]);
		}
	}

	public void AddCardOfCost(int cost)
	{
		if (m_manaCosts != null)
		{
			cost = Mathf.Clamp(cost, 0, m_manaCosts.Count - 1);
			m_manaCosts[cost]++;
			UpdateBars();
		}
	}

	public void ResetBars()
	{
		m_manaCosts = new List<int>();
		for (int i = 0; i < m_bars.Count; i++)
		{
			m_manaCosts.Add(0);
		}
		UpdateBars();
	}

	public void AddCardToManaCurve(EntityDef entityDef)
	{
		if (entityDef == null)
		{
			Debug.LogWarning("DraftManaCurve.AddCardToManaCurve() - entityDef is null");
		}
		else
		{
			AddCardOfCost(entityDef.GetCost());
		}
	}
}
