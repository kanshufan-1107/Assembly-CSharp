using System.Collections.Generic;
using UnityEngine;

public class TutorialKeywordManager : MonoBehaviour
{
	public TutorialKeywordTooltip m_keywordPanelPrefab;

	private static TutorialKeywordManager s_instance;

	private List<TutorialKeywordTooltip> m_keywordPanels;

	private Actor m_actor;

	private Card m_card;

	private void Awake()
	{
		s_instance = this;
	}

	private void OnDestroy()
	{
		s_instance = null;
	}

	public static TutorialKeywordManager Get()
	{
		return s_instance;
	}

	public void UpdateKeywordHelp(Card c, Actor a)
	{
		UpdateKeywordHelp(c, a, showOnRight: true, null);
	}

	public void UpdateKeywordHelp(Card card, Actor actor, bool showOnRight, float? overrideScale = null)
	{
		m_card = card;
		UpdateKeywordHelp(card.GetEntity(), actor, showOnRight, overrideScale);
	}

	public void UpdateKeywordHelp(Entity entity, Actor actor, bool showOnRight, float? overrideScale = null)
	{
		float scaleToUse = 1f;
		if (overrideScale.HasValue)
		{
			scaleToUse = overrideScale.Value;
		}
		PrepareToUpdateKeywordHelp(actor);
		string[] customKeywordData = GameState.Get().GetGameEntity().NotifyOfKeywordHelpPanelDisplay(entity);
		if (customKeywordData != null)
		{
			SetupKeywordPanel(customKeywordData[0], customKeywordData[1]);
		}
		SetUpPanels(entity);
		TutorialKeywordTooltip prevPanel = null;
		float xPaddingMultiplier = 0f;
		float zPadding = 0f;
		for (int i = 0; i < m_keywordPanels.Count; i++)
		{
			TutorialKeywordTooltip k = m_keywordPanels[i];
			xPaddingMultiplier = 1.05f;
			if (entity.IsHero())
			{
				xPaddingMultiplier = 1.2f;
			}
			else if (entity.GetZone() == TAG_ZONE.PLAY)
			{
				if ((bool)UniversalInputManager.UsePhoneUI)
				{
					scaleToUse = 1.7f;
				}
				xPaddingMultiplier = 1.45f * scaleToUse;
			}
			k.transform.localScale = new Vector3(scaleToUse, scaleToUse, scaleToUse);
			zPadding = -0.2f * m_actor.GetMeshRenderer().bounds.size.z;
			if ((bool)UniversalInputManager.UsePhoneUI && entity.GetZone() == TAG_ZONE.PLAY)
			{
				zPadding += 1.5f;
			}
			if (i == 0)
			{
				if (showOnRight)
				{
					k.transform.position = m_actor.transform.position + new Vector3(m_actor.GetMeshRenderer().bounds.size.x * xPaddingMultiplier, 0f, m_actor.GetMeshRenderer().bounds.extents.z + zPadding);
				}
				else
				{
					k.transform.position = m_actor.transform.position + new Vector3((0f - m_actor.GetMeshRenderer().bounds.size.x) * xPaddingMultiplier, 0f, m_actor.GetMeshRenderer().bounds.extents.z + zPadding);
				}
			}
			else
			{
				k.transform.position = prevPanel.transform.position - new Vector3(0f, 0f, prevPanel.GetHeight() * 0.35f + k.GetHeight() * 0.35f);
			}
			prevPanel = k;
		}
		GameState.Get().GetGameEntity().NotifyOfHelpPanelDisplay(m_keywordPanels.Count);
	}

	private void PrepareToUpdateKeywordHelp(Actor actor)
	{
		HideKeywordHelp();
		m_actor = actor;
		m_keywordPanels = new List<TutorialKeywordTooltip>();
	}

	private void SetUpPanels(EntityBase entityInfo)
	{
		SetupKeywordPanelIfNecessary(entityInfo, GAME_TAG.TAUNT);
		SetupKeywordPanelIfNecessary(entityInfo, GAME_TAG.STEALTH);
		SetupKeywordPanelIfNecessary(entityInfo, GAME_TAG.DIVINE_SHIELD);
		SetupKeywordPanelIfNecessary(entityInfo, GAME_TAG.SPELLPOWER);
		SetupKeywordPanelIfNecessary(entityInfo, GAME_TAG.ENRAGED);
		SetupKeywordPanelIfNecessary(entityInfo, GAME_TAG.CHARGE);
		SetupKeywordPanelIfNecessary(entityInfo, GAME_TAG.BATTLECRY);
		SetupKeywordPanelIfNecessary(entityInfo, GAME_TAG.FROZEN);
		SetupKeywordPanelIfNecessary(entityInfo, GAME_TAG.FREEZE);
		SetupKeywordPanelIfNecessary(entityInfo, GAME_TAG.WINDFURY);
		SetupKeywordPanelIfNecessary(entityInfo, GAME_TAG.SECRET);
		SetupKeywordPanelIfNecessary(entityInfo, GAME_TAG.DEATHRATTLE);
		SetupKeywordPanelIfNecessary(entityInfo, GAME_TAG.OVERLOAD);
		SetupKeywordPanelIfNecessary(entityInfo, GAME_TAG.COMBO);
	}

	private bool SetupKeywordPanelIfNecessary(EntityBase entityInfo, GAME_TAG tag)
	{
		if (entityInfo.HasTag(tag))
		{
			SetupKeywordPanel(tag);
			return true;
		}
		if (entityInfo.HasReferencedTag(tag))
		{
			SetupKeywordRefPanel(tag);
			return true;
		}
		return false;
	}

	public void SetupKeywordPanel(GAME_TAG tag)
	{
		string name = GameStrings.GetKeywordName(tag);
		string text = GameStrings.GetKeywordText(tag);
		SetupKeywordPanel(name, text);
	}

	public void SetupKeywordRefPanel(GAME_TAG tag)
	{
		string name = GameStrings.GetKeywordName(tag);
		string text = GameStrings.GetRefKeywordText(tag);
		SetupKeywordPanel(name, text);
	}

	public void SetupKeywordPanel(string headline, string description)
	{
		TutorialKeywordTooltip k = Object.Instantiate(m_keywordPanelPrefab.gameObject).GetComponent<TutorialKeywordTooltip>();
		if (!(k == null))
		{
			k.Initialize(headline, description);
			m_keywordPanels.Add(k);
		}
	}

	public void HideKeywordHelp()
	{
		if (m_keywordPanels == null)
		{
			return;
		}
		foreach (TutorialKeywordTooltip k in m_keywordPanels)
		{
			if (!(k == null))
			{
				Object.Destroy(k.gameObject);
			}
		}
	}

	public Card GetCard()
	{
		return m_card;
	}

	public Vector3 GetPositionOfTopPanel()
	{
		if (m_keywordPanels == null || m_keywordPanels.Count == 0)
		{
			return new Vector3(0f, 0f, 0f);
		}
		return m_keywordPanels[0].transform.position;
	}
}
