using Shared.UI.Scripts.Carousel;
using UnityEngine;

namespace Game.PackOpening;

public class PackOpeningCardCarouselItem : Carousel.Item
{
	private PackOpeningCard m_card;

	public PackOpeningCardCarouselItem(PackOpeningCard card)
	{
		m_card = card;
	}

	public void Show(Carousel card)
	{
	}

	public void Hide()
	{
	}

	public void Clear()
	{
		m_card = null;
	}

	public GameObject GetGameObject()
	{
		if (!(m_card != null))
		{
			return null;
		}
		return m_card.gameObject;
	}

	public bool IsLoaded()
	{
		return true;
	}
}
