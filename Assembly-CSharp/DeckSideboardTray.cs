using System.Collections.Generic;
using Hearthstone.UI;
using UnityEngine;

public class DeckSideboardTray : MonoBehaviour
{
	[SerializeField]
	private GameObject m_root;

	[SerializeField]
	private DeckTrayCardListContent m_cardsContent;

	[SerializeField]
	private VisualController m_sideboardTypeVC;

	[SerializeField]
	private VisualController m_sideboardVisibilityVC;

	[SerializeField]
	private SideboardRuneIndicator m_sideboardRuneIndicator;

	public DeckTrayCardListContent CardsContent => m_cardsContent;

	public bool IsShowing { get; private set; }

	public virtual void Show(SideboardDeck sideboardDeck)
	{
		if (IsShowing || sideboardDeck == null)
		{
			return;
		}
		UpdateCardList(sideboardDeck);
		foreach (IDataModel sideboardDataModel in sideboardDeck.UIDataModels)
		{
			m_sideboardTypeVC.BindDataModel(sideboardDataModel);
			m_sideboardVisibilityVC.BindDataModel(sideboardDataModel);
		}
		m_sideboardVisibilityVC.SetState("OPEN");
		IsShowing = true;
		if (m_sideboardRuneIndicator != null)
		{
			m_sideboardRuneIndicator.UpdateRunes(sideboardDeck.GetRuneOrder());
		}
	}

	public virtual void Hide()
	{
		m_sideboardVisibilityVC.SetState("CLOSE");
		IsShowing = false;
	}

	public virtual bool UpdateHeldCardVisual(CollectionDraggableCardVisual collectionDraggableCardVisual)
	{
		return false;
	}

	public virtual void StartDragWithActor(Actor actor, CollectionUtils.ViewMode viewMode, bool showVisual = true, CollectionDeckSlot slot = null)
	{
	}

	private void UpdateCardList(CollectionDeck sideboardDeck)
	{
		CardsContent.UpdateCardList(string.Empty, sideboardDeck);
	}

	public virtual bool UpdateCurrentPageCardLocks(IEnumerable<CollectionCardVisual> collectionCardVisuals)
	{
		return false;
	}

	public virtual bool OnSideboardDoneButtonPressed()
	{
		return true;
	}
}
