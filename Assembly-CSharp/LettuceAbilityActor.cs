using System.Collections.Generic;
using Blizzard.T5.MaterialService.Extensions;
using UnityEngine;

public class LettuceAbilityActor : Actor
{
	public bool m_updateVisualsOnCooldown;

	public UberText m_currentCooldownText;

	public UberText m_cooldownConfigText;

	public GameObject m_hourglassObject;

	public GameObject m_checkMarkObject;

	public MeshRenderer m_mercenaryAbilityBannerMesh;

	public GameObject m_speedWing;

	public Color m_cooldownFrameColor;

	public AudioSource m_hoverSound;

	public List<AudioSource> m_clickSounds;

	public override void UpdateMeshComponents()
	{
		base.UpdateMeshComponents();
		UpdateMedallionCooldownState();
		UpdateSpeedWingState();
	}

	public void PlayMousedOverSound()
	{
		if (m_hoverSound != null)
		{
			SoundManager.Get().Play(m_hoverSound);
		}
	}

	public void PlayMouseClickedSound()
	{
		if (m_clickSounds == null)
		{
			return;
		}
		foreach (AudioSource sound in m_clickSounds)
		{
			if (sound != null)
			{
				SoundManager.Get().Play(sound);
			}
		}
	}

	private void UpdateMedallionCooldownState()
	{
		if (m_entity == null || !m_updateVisualsOnCooldown)
		{
			return;
		}
		Renderer portraitMeshRenderer = m_portraitMesh.GetComponent<Renderer>();
		if (m_entity.HasTag(GAME_TAG.LETTUCE_CURRENT_COOLDOWN))
		{
			if (m_hourglassObject != null)
			{
				m_hourglassObject.SetActive(value: true);
			}
			m_costTextMesh.gameObject.SetActive(value: false);
			portraitMeshRenderer.GetMaterial(m_portraitFrameMatIdx).color = m_cooldownFrameColor;
		}
		else
		{
			if (m_hourglassObject != null)
			{
				m_hourglassObject.SetActive(value: false);
			}
			m_costTextMesh.gameObject.SetActive(value: true);
			portraitMeshRenderer.GetMaterial(m_portraitFrameMatIdx).color = Color.white;
		}
	}

	private void UpdateSpeedWingState()
	{
		if (!(m_speedWing == null))
		{
			bool speedWingActive = true;
			if (m_updateVisualsOnCooldown && m_entity != null && m_entity.HasTag(GAME_TAG.LETTUCE_CURRENT_COOLDOWN))
			{
				speedWingActive = false;
			}
			if (AbilityIsPassiveOrStartOfGame())
			{
				speedWingActive = false;
			}
			m_speedWing.SetActive(speedWingActive);
		}
	}

	private bool AbilityIsPassiveOrStartOfGame()
	{
		if (m_entityDef != null && (m_entityDef.HasTag(GAME_TAG.LETTUCE_PASSIVE_ABILITY) || m_entityDef.HasTag(GAME_TAG.LETTUCE_START_OF_GAME_ABILITY)))
		{
			return true;
		}
		if (m_entity != null && (m_entity.HasTag(GAME_TAG.LETTUCE_PASSIVE_ABILITY) || m_entity.HasTag(GAME_TAG.LETTUCE_START_OF_GAME_ABILITY)))
		{
			return true;
		}
		return false;
	}

	public override void UpdateTextComponentsDef(EntityDef entityDef)
	{
		if (entityDef != null)
		{
			base.UpdateTextComponentsDef(entityDef);
			UpdateCurrentCooldownText(entityDef);
			UpdateCooldownConfigText(entityDef);
			UpdateHourglassObject();
		}
	}

	public override void UpdateTextComponents(Entity entity)
	{
		if (entity != null)
		{
			base.UpdateTextComponents(entity);
			UpdateCurrentCooldownText(entity);
			UpdateCooldownConfigText(entity);
			UpdateHourglassObject();
			UpdateCheckMarkObject();
		}
	}

	protected override void SetMaterialWithTexture(TAG_CARDTYPE cardType, CardColorSwitcher.CardColorType colorType)
	{
		base.SetMaterialWithTexture(cardType, colorType);
		if (m_mercenaryAbilityBannerMesh != null && m_cardColorTex != null)
		{
			m_mercenaryAbilityBannerMesh.GetMaterial(0).mainTexture = m_cardColorTex;
		}
	}

	private void UpdateCurrentCooldownText(Entity entity)
	{
		if (!(m_currentCooldownText == null))
		{
			int currentCooldown = entity.GetTag(GAME_TAG.LETTUCE_CURRENT_COOLDOWN);
			if (currentCooldown == 0)
			{
				m_currentCooldownText.Text = string.Empty;
			}
			else
			{
				m_currentCooldownText.Text = currentCooldown.ToString();
			}
		}
	}

	private void UpdateCurrentCooldownText(EntityDef entityDef)
	{
		if (!(m_currentCooldownText == null))
		{
			int currentCooldown = entityDef.GetTag(GAME_TAG.LETTUCE_CURRENT_COOLDOWN);
			if (currentCooldown == 0)
			{
				m_currentCooldownText.Text = string.Empty;
			}
			else
			{
				m_currentCooldownText.Text = currentCooldown.ToString();
			}
		}
	}

	private void UpdateCooldownConfigText(Entity entity)
	{
		if (!(m_cooldownConfigText == null))
		{
			int cooldownConfig = entity.GetTag(GAME_TAG.LETTUCE_COOLDOWN_CONFIG);
			if (cooldownConfig == 0)
			{
				m_cooldownConfigText.Text = string.Empty;
			}
			else
			{
				m_cooldownConfigText.Text = cooldownConfig.ToString();
			}
		}
	}

	private void UpdateCooldownConfigText(EntityDef entityDef)
	{
		if (!(m_cooldownConfigText == null))
		{
			int cooldownConfig = entityDef.GetTag(GAME_TAG.LETTUCE_COOLDOWN_CONFIG);
			if (cooldownConfig == 0)
			{
				m_cooldownConfigText.Text = string.Empty;
			}
			else
			{
				m_cooldownConfigText.Text = cooldownConfig.ToString();
			}
		}
	}

	private void UpdateHourglassObject()
	{
		if (m_hourglassObject == null)
		{
			return;
		}
		if (string.IsNullOrEmpty(m_cooldownConfigText?.Text) && string.IsNullOrEmpty(m_currentCooldownText?.Text))
		{
			if (m_hourglassObject.activeSelf)
			{
				m_hourglassObject.SetActive(value: false);
			}
		}
		else if (!m_hourglassObject.activeSelf)
		{
			m_hourglassObject.SetActive(value: true);
		}
	}

	public void UpdateCheckMarkObject()
	{
		if (m_checkMarkObject == null)
		{
			return;
		}
		m_checkMarkObject.SetActive(value: false);
		if (m_entity != null)
		{
			Entity abilityOwner = m_entity.GetLettuceAbilityOwner();
			if (abilityOwner != null && abilityOwner.GetSelectedLettuceAbilityID() == m_entity.GetEntityId())
			{
				m_checkMarkObject.SetActive(value: true);
			}
		}
	}
}
