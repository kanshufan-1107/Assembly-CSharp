using Shared.UI.Scripts.Carousel;
using UnityEngine;

public class CatchupPackSummaryCardCarouselItem : Carousel.Item
{
	private GameObject m_catchupPackSummaryCardGameObject;

	public CatchupPackSummaryCardCarouselItem(GameObject inGameObject)
	{
		m_catchupPackSummaryCardGameObject = inGameObject;
	}

	public void Show(Carousel carousel)
	{
	}

	public void Hide()
	{
	}

	public void Clear()
	{
		m_catchupPackSummaryCardGameObject = null;
	}

	public GameObject GetGameObject()
	{
		return m_catchupPackSummaryCardGameObject;
	}

	public bool IsLoaded()
	{
		return true;
	}
}
