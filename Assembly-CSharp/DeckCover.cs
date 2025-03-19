using UnityEngine;

public class DeckCover : MonoBehaviour
{
	public Spell m_OpenDeckCoverSpell;

	public MeshRenderer m_HighlightMeshRenderer;

	public void SetDeckVisualRootObject(GameObject deckVisualRootObject)
	{
		if (!(m_OpenDeckCoverSpell == null) && !(deckVisualRootObject == null))
		{
			PlayMakerFSM fsm = m_OpenDeckCoverSpell.GetComponent<PlayMakerFSM>();
			if (!(fsm == null))
			{
				fsm.FsmVariables.GetFsmGameObject("DeckVisual").Value = deckVisualRootObject;
			}
		}
	}

	public void OpenDeckCover()
	{
		if (!(m_OpenDeckCoverSpell == null) && m_OpenDeckCoverSpell.GetActiveState() != SpellStateType.BIRTH && m_OpenDeckCoverSpell.GetActiveState() != SpellStateType.IDLE)
		{
			m_OpenDeckCoverSpell.ActivateState(SpellStateType.BIRTH);
		}
	}

	public void CloseDeckCover()
	{
		if (!(m_OpenDeckCoverSpell == null) && m_OpenDeckCoverSpell.GetActiveState() != SpellStateType.DEATH && m_OpenDeckCoverSpell.GetActiveState() != 0)
		{
			m_OpenDeckCoverSpell.ActivateState(SpellStateType.DEATH);
		}
	}

	public void SetDeckCoverHighlightVisibility(bool isVisible)
	{
		if (m_HighlightMeshRenderer != null)
		{
			m_HighlightMeshRenderer.enabled = isVisible;
		}
	}

	public virtual void UpdateVisual(Player.Side side)
	{
	}
}
