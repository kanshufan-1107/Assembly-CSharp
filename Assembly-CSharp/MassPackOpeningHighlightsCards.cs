using System.Collections.Generic;
using UnityEngine;

public class MassPackOpeningHighlightsCards : MonoBehaviour
{
	[SerializeField]
	private List<PackOpeningCard> m_packOpeningCards = new List<PackOpeningCard>();

	public List<PackOpeningCard> GetPackOpeningCards()
	{
		return m_packOpeningCards;
	}
}
