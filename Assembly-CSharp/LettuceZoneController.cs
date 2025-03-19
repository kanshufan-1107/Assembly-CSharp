using System.Collections.Generic;
using UnityEngine;

public class LettuceZoneController
{
	private const string ABILITY_TRAY_PREFAB = "MercenariesAbilityTray.prefab:bf65ec0d425616a40a16734ff75c32b1";

	private const string ABILITY_TRAY_BONE = "MercenariesAbilityTray";

	private GameState m_gameState;

	private InputManager m_inputManager;

	private Entity m_lettuceAbilitiesSourceEntity;

	private Card m_previouslySelectedMercenaryCard;

	private List<Card> m_displayedAbilityCards = new List<Card>();

	private MercenariesAbilityTray m_abilityTray;

	public LettuceZoneController(GameState gameState, InputManager inputManager)
	{
		m_gameState = gameState;
		m_inputManager = inputManager;
	}

	public Entity GetLettuceAbilitiesSourceEntity()
	{
		return m_lettuceAbilitiesSourceEntity;
	}

	public List<Card> GetDisplayedLettuceAbilityCards()
	{
		return m_displayedAbilityCards;
	}

	public void DisplayLettuceAbilitiesForPreviouslySelectedCard()
	{
		if (m_previouslySelectedMercenaryCard != null)
		{
			DisplayLettuceAbilitiesForEntity(m_previouslySelectedMercenaryCard.GetEntity());
		}
	}

	public void DisplayLettuceAbilitiesForEntity(Entity entity)
	{
		if (m_lettuceAbilitiesSourceEntity == entity || !(m_gameState.GetGameEntity() is LettuceMissionEntity lettuceGameEntity))
		{
			return;
		}
		if (entity != null)
		{
			lettuceGameEntity.SetPrevSelectedCharacterZonePosition(entity.GetZonePosition());
		}
		ClearDisplayedLettuceAbilities();
		if (entity == null || !IsAllowedToShowAbilityTray())
		{
			return;
		}
		lettuceGameEntity.ShowWeaknessSplatsForMercenary(entity);
		Card previouslySelectedAbilityCard = null;
		foreach (int entityID in entity.GetLettuceAbilityEntityIDs())
		{
			Card abilityCard = m_gameState.GetEntity(entityID)?.GetCard();
			if (entity.GetSelectedLettuceAbilityID() == abilityCard.GetEntity().GetEntityId())
			{
				previouslySelectedAbilityCard = abilityCard;
				break;
			}
		}
		m_displayedAbilityCards = new List<Card>();
		foreach (int entityID2 in entity.GetLettuceAbilityEntityIDs())
		{
			Entity abilityEntity = m_gameState.GetEntity(entityID2);
			if (abilityEntity != null && !abilityEntity.IsLettuceEquipment())
			{
				Card abilityCard2 = abilityEntity.GetCard();
				if (abilityCard2 != null)
				{
					m_displayedAbilityCards.Add(abilityCard2);
				}
			}
		}
		ShowAbilityTray(entity, m_displayedAbilityCards);
		if (previouslySelectedAbilityCard != null)
		{
			int targetEntityId = entity.GetTag(GAME_TAG.LETTUCE_SELECTED_TARGET);
			Entity targetEntity = m_gameState.GetEntity(targetEntityId);
			if (targetEntity != null)
			{
				TargetReticleManager.Get().CreateStaticTargetArrow(entity, targetEntity);
				TargetReticleManager.Get().SetTargetArrowLinkLayer(GameLayer.Default);
				TargetReticleManager.Get().SetParabolaHeight(0.4f);
			}
		}
		m_lettuceAbilitiesSourceEntity = entity;
		entity.GetCard().UpdateSelectedLettuceCharacterVisual();
	}

	public void ClearDisplayedLettuceAbilities(bool hideWeaknessSplats = true, bool cachePreviouslySelected = false)
	{
		Card prevSelectedCard = null;
		m_previouslySelectedMercenaryCard = null;
		if (m_lettuceAbilitiesSourceEntity != null)
		{
			prevSelectedCard = m_lettuceAbilitiesSourceEntity.GetCard();
			if (cachePreviouslySelected)
			{
				m_previouslySelectedMercenaryCard = prevSelectedCard;
			}
			HideAbilityTray();
			TargetReticleManager.Get().DestroyStaticTargetArrow();
		}
		if (hideWeaknessSplats && m_gameState.GetGameEntity() is LettuceMissionEntity lettuceGameEntity)
		{
			lettuceGameEntity.HideWeaknessSplats();
		}
		m_lettuceAbilitiesSourceEntity = null;
		prevSelectedCard?.UpdateSelectedLettuceCharacterVisual();
	}

	private void CreateAbilityTray()
	{
		GameObject abilityTrayObject = AssetLoader.Get().InstantiatePrefab("MercenariesAbilityTray.prefab:bf65ec0d425616a40a16734ff75c32b1");
		m_abilityTray = abilityTrayObject.GetComponent<MercenariesAbilityTray>();
		Transform abilityTrayBone = Gameplay.Get().GetBoardLayout().FindBone("MercenariesAbilityTray");
		if (abilityTrayBone != null)
		{
			m_abilityTray.transform.position = abilityTrayBone.position;
		}
	}

	private void ShowAbilityTray(Entity entity, List<Card> abilityCards)
	{
		if (m_abilityTray == null)
		{
			CreateAbilityTray();
		}
		m_abilityTray.SetupForMercenary(entity, abilityCards);
		if (m_gameState.GetGameEntity() is LettuceMissionEntity lettuceGameEntity)
		{
			lettuceGameEntity.UpdateAllMercenaryAbilityOrderBubbleText(hideUnselectedAbilityBubbles: true);
			lettuceGameEntity.OnAbilityTrayShown(entity);
		}
		m_abilityTray.Show();
	}

	private void HideAbilityTray()
	{
		if (!(m_abilityTray == null))
		{
			if (m_gameState.GetGameEntity() is LettuceMissionEntity lettuceGameEntity)
			{
				lettuceGameEntity.ShowAllMercenaryAbilityOrderBubbles();
				lettuceGameEntity.UpdateAllMercenaryAbilityOrderBubbleText();
				lettuceGameEntity.OnAbilityTrayDismissed();
			}
			m_abilityTray.Hide();
		}
	}

	private bool IsAllowedToShowAbilityTray()
	{
		if (m_gameState.IsResponsePacketBlocked())
		{
			return false;
		}
		return true;
	}

	public MercenariesAbilityTray GetAbilityTray()
	{
		return m_abilityTray;
	}
}
