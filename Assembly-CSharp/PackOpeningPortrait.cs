using Blizzard.T5.Game.Spells;
using Hearthstone.UI;
using UnityEngine;

public class PackOpeningPortrait : MonoBehaviour
{
	public GameObject m_root;

	public Renderer[] m_CardBackRenderers;

	private ISpellCallbackHandler<Spell>.FinishedCallback m_finishedCallback;

	private ISpellCallbackHandler<Spell>.StateFinishedCallback m_stateFinishedCallback;

	private void Start()
	{
		SetCardbackVisability(visible: false);
	}

	private void SetCardbackVisability(bool visible)
	{
		Renderer[] cardBackRenderers = m_CardBackRenderers;
		for (int i = 0; i < cardBackRenderers.Length; i++)
		{
			cardBackRenderers[i].enabled = visible;
		}
	}

	public void ActivateDeathVisuals(ISpellCallbackHandler<Spell>.FinishedCallback finishedCallback, ISpellCallbackHandler<Spell>.StateFinishedCallback stateFinishedCallback)
	{
		m_finishedCallback = finishedCallback;
		m_stateFinishedCallback = stateFinishedCallback;
		SendEventUpwardStateAction.SendEventUpward(base.gameObject, "FADE_OUT");
	}

	public void OnDeathVisualsFadedIn()
	{
		m_finishedCallback(null, null);
		m_finishedCallback = null;
	}

	public void OnDeathVisualsFinished()
	{
		m_stateFinishedCallback(null, SpellStateType.NONE, null);
		m_stateFinishedCallback = null;
	}

	public void SetActive(bool active)
	{
		m_root.SetActive(active);
	}
}
