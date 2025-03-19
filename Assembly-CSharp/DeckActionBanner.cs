using UnityEngine;

public class DeckActionBanner : MonoBehaviour
{
	public MeshRenderer m_deckActionHighlight_Green;

	public MeshRenderer m_deckActionHighlight_Blue;

	public void SetHighlightState(DeckActionHighlightState state)
	{
		m_deckActionHighlight_Green.enabled = state == DeckActionHighlightState.Green;
		m_deckActionHighlight_Blue.enabled = state == DeckActionHighlightState.Blue;
	}
}
