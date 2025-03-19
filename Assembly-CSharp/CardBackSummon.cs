using Blizzard.T5.Core.Utils;
using Blizzard.T5.MaterialService.Extensions;
using UnityEngine;

public class CardBackSummon : MonoBehaviour
{
	private CardBackManager m_CardBackManager;

	private Actor m_Actor;

	private Spell m_Spell;

	private void OnEnable()
	{
		CardBackManager.Get()?.RegisterUpdateCardbacksListener(UpdateCardBack);
		m_Spell = GameObjectUtils.FindComponentInParents<Spell>(base.gameObject);
		if (m_Spell == null)
		{
			Debug.LogWarning("Failed to find Spell on CardBackSummon");
			UpdateEchoTexture();
		}
		else
		{
			m_Spell.AddStateStartedCallback(OnStateStarted);
		}
	}

	private void OnDisable()
	{
		CardBackManager.Get()?.UnregisterUpdateCardbacksListener(UpdateCardBack);
	}

	private void OnStateStarted(Spell spell, SpellStateType spellStateType, object userData)
	{
		switch (spell.GetActiveState())
		{
		case SpellStateType.BIRTH:
			if (spell.GetSourceCard() != null)
			{
				UpdateEchoTexture();
			}
			break;
		case SpellStateType.NONE:
			m_Actor = null;
			break;
		}
	}

	public void UpdateEffectWithCardBack(CardBack cardBack)
	{
		UpdateEchoTexture(cardBack);
	}

	private void UpdateEchoTexture(CardBack cardBackOverride = null)
	{
		if (m_Actor == null)
		{
			m_Actor = GameObjectUtils.FindComponentInParents<Actor>(base.gameObject);
			if (m_Actor == null)
			{
				Debug.LogError("CardBackSummonIn failed to get Actor!");
			}
		}
		Renderer echoRenderer = GetComponent<Renderer>();
		Texture echo = echoRenderer.GetMaterial().mainTexture;
		if (cardBackOverride != null)
		{
			echo = cardBackOverride.m_HiddenCardEchoTexture;
		}
		else
		{
			if (m_CardBackManager == null)
			{
				m_CardBackManager = CardBackManager.Get();
				if (m_CardBackManager == null)
				{
					Debug.LogError("CardBackSummonIn failed to get CardBackManager!");
					base.enabled = false;
					return;
				}
			}
			if (m_CardBackManager.IsActorFriendly(m_Actor))
			{
				CardBack fcb = m_CardBackManager.GetFriendlyCardBack();
				if (fcb != null)
				{
					echo = fcb.m_HiddenCardEchoTexture;
				}
			}
			else
			{
				CardBack ocb = m_CardBackManager.GetOpponentCardBack();
				if (ocb != null)
				{
					echo = ocb.m_HiddenCardEchoTexture;
				}
			}
		}
		if (echo != null)
		{
			echoRenderer.GetMaterial().mainTexture = echo;
		}
	}

	private void UpdateCardBack()
	{
		UpdateEchoTexture();
	}
}
