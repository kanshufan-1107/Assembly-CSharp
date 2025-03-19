using UnityEngine;

public class ForgeBanner : MonoBehaviour
{
	public MeshRenderer m_forgeHighlight_Green;

	public MeshRenderer m_forgeHighlight_Blue;

	public void SetHighlightState(DeckActionHighlightState state)
	{
		m_forgeHighlight_Green.enabled = state == DeckActionHighlightState.Green;
		m_forgeHighlight_Blue.enabled = state == DeckActionHighlightState.Blue;
	}
}
