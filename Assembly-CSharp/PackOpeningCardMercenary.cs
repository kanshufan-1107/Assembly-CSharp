using Blizzard.T5.Game.Spells;
using Hearthstone.UI;
using UnityEngine;

public class PackOpeningCardMercenary : MonoBehaviour
{
	public GameObject m_root;

	public Renderer[] m_CardBackRenderers;

	public AsyncReference m_mercenaryNameGlowReference;

	private ParticleSystem m_mercenaryNameGlow;

	private ISpellCallbackHandler<Spell>.FinishedCallback m_finishedCallback;

	private ISpellCallbackHandler<Spell>.StateFinishedCallback m_stateFinishedCallback;

	private void Start()
	{
		m_mercenaryNameGlowReference.RegisterReadyListener<ParticleSystem>(OnMercenaryNameGlowReady);
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

	private void OnMercenaryNameGlowReady(ParticleSystem nameGlow)
	{
		if (nameGlow == null)
		{
			Error.AddDevWarning("UI Error!", "MercenaryNameGlowReference could not be found!");
		}
		else
		{
			m_mercenaryNameGlow = nameGlow;
		}
	}

	public void ShowMercenaryNameGlow()
	{
		if (!(m_mercenaryNameGlow == null))
		{
			m_mercenaryNameGlow.Play();
			LayerUtils.SetLayer(m_mercenaryNameGlow.gameObject, GameLayer.Default);
		}
	}

	public void ActivateDeathVisuals(ISpellCallbackHandler<Spell>.FinishedCallback finishedCallback, ISpellCallbackHandler<Spell>.StateFinishedCallback stateFinishedCallback)
	{
		m_finishedCallback = finishedCallback;
		m_stateFinishedCallback = stateFinishedCallback;
		SendEventUpwardStateAction.SendEventUpward(base.gameObject, "FADE_OUT_MERC");
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
