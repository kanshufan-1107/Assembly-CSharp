using PegasusShared;
using UnityEngine;

public class DeckTrayReorderableContent : DeckTrayContent
{
	[CustomEditField(Sections = "Deck Button Settings")]
	public Vector3 m_rearrangeWiggleAxis = new Vector3(0f, 1f, 0f);

	[CustomEditField(Sections = "Deck Button Settings")]
	public float m_rearrangeWiggleAmplitude = 0.85f;

	[CustomEditField(Sections = "Deck Button Settings")]
	public float m_rearrangeWiggleFrequency = 15f;

	[CustomEditField(Sections = "Deck Button Settings")]
	public float m_rearrangeStartStopTweenDuration = 0.1f;

	[CustomEditField(Sections = "Deck Button Settings")]
	public float m_rearrangeEnlargeScale = 1.05f;

	protected IDraggableCollectionVisual m_draggingDeckBox;

	[CustomEditField(Sections = "Scroll Settings")]
	public UIBScrollable m_scrollbar;

	public bool IsTouchDragging
	{
		get
		{
			if (m_scrollbar != null)
			{
				return m_scrollbar.IsTouchDragging();
			}
			return false;
		}
	}

	public IDraggableCollectionVisual DraggingDeckBox => m_draggingDeckBox;

	public virtual void StartDragToReorder(IDraggableCollectionVisual draggingDeckBox)
	{
		if (m_draggingDeckBox != draggingDeckBox)
		{
			if (m_draggingDeckBox != null)
			{
				StopDragToReorder();
			}
			m_draggingDeckBox = draggingDeckBox;
			m_scrollbar.Pause(pause: true);
			m_scrollbar.PauseUpdateScrollHeight(pause: true);
		}
	}

	public virtual void StopDragToReorder()
	{
		if (m_draggingDeckBox != null)
		{
			foreach (CollectionDeck deck in CollectionManager.Get().GetDecks(DeckType.NORMAL_DECK))
			{
				deck.SendChanges(CollectionDeck.ChangeSource.StopDragToReorder);
			}
			m_draggingDeckBox.OnStopDragToReorder();
		}
		m_draggingDeckBox = null;
		m_scrollbar.Pause(pause: false);
		m_scrollbar.PauseUpdateScrollHeight(pause: false);
	}

	protected virtual void UpdateDragToReorder()
	{
	}
}
