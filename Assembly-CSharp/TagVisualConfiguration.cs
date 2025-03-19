using System.Collections.Generic;
using UnityEngine;

[CustomEditClass(DefaultCollapsed = true)]
internal class TagVisualConfiguration : MonoBehaviour
{
	[CustomEditField(SearchField = "m_Tag")]
	public List<TagVisualConfigurationEntry> m_TagVisuals = new List<TagVisualConfigurationEntry>();

	private static TagVisualConfiguration s_instance;

	public void Awake()
	{
		s_instance = this;
	}

	private void OnDestroy()
	{
		s_instance = null;
	}

	public static TagVisualConfiguration Get()
	{
		return s_instance;
	}

	public void ActivateStateSpells(Card card)
	{
		if (card == null || card.GetActor() == null || card.GetEntity() == null)
		{
			return;
		}
		foreach (TagVisualConfigurationEntry entry in m_TagVisuals)
		{
			TagVisualConfigurationEntry comparisonEntry = entry;
			if (entry.m_ReferenceTag != 0)
			{
				comparisonEntry = FindTagEntry(entry.m_ReferenceTag);
			}
			if (comparisonEntry == null || !comparisonEntry.m_IsPlayStateSpell)
			{
				continue;
			}
			TagDelta change = new TagDelta();
			change.tag = (int)entry.m_Tag;
			change.oldValue = 0;
			change.newValue = card.GetEntity().GetTag(entry.m_Tag);
			if (comparisonEntry.m_BeforeAlways != null)
			{
				foreach (TagVisualActionConfiguration actionConfig in comparisonEntry.m_BeforeAlways.m_Actions)
				{
					ConditionallyExecuteAction(comparisonEntry, actionConfig, card, fromShowEntity: false, change, fromTagChange: false);
				}
			}
			if (change.newValue > 0 && comparisonEntry.m_TagAdded != null)
			{
				foreach (TagVisualActionConfiguration actionConfig2 in comparisonEntry.m_TagAdded.m_Actions)
				{
					ConditionallyExecuteAction(comparisonEntry, actionConfig2, card, fromShowEntity: false, change, fromTagChange: false);
				}
			}
			if (comparisonEntry.m_AfterAlways == null)
			{
				continue;
			}
			foreach (TagVisualActionConfiguration actionConfig3 in comparisonEntry.m_AfterAlways.m_Actions)
			{
				ConditionallyExecuteAction(comparisonEntry, actionConfig3, card, fromShowEntity: false, change, fromTagChange: false);
			}
		}
	}

	public void ActivateHandStateSpells(Card card, bool forceActivate = false)
	{
		if (card == null || card.GetActor() == null || card.GetEntity() == null || ((card.GetEntity() != null) ? card.GetEntity().GetZone() : TAG_ZONE.SETASIDE) != TAG_ZONE.HAND)
		{
			return;
		}
		foreach (TagVisualConfigurationEntry entry in m_TagVisuals)
		{
			TagVisualConfigurationEntry comparisonEntry = entry;
			if (entry.m_ReferenceTag != 0)
			{
				comparisonEntry = FindTagEntry(entry.m_ReferenceTag);
			}
			if (comparisonEntry == null || !comparisonEntry.m_IsHandStateSpell)
			{
				continue;
			}
			TagDelta change = new TagDelta();
			change.tag = (int)entry.m_Tag;
			change.oldValue = 0;
			change.newValue = card.GetEntity().GetTag(entry.m_Tag);
			if (comparisonEntry.m_BeforeAlways != null)
			{
				foreach (TagVisualActionConfiguration actionConfig in comparisonEntry.m_BeforeAlways.m_Actions)
				{
					ConditionallyExecuteAction(comparisonEntry, actionConfig, card, fromShowEntity: false, change, fromTagChange: false, forceActivate);
				}
			}
			if (change.newValue > 0 && comparisonEntry.m_TagAdded != null)
			{
				foreach (TagVisualActionConfiguration actionConfig2 in comparisonEntry.m_TagAdded.m_Actions)
				{
					ConditionallyExecuteAction(comparisonEntry, actionConfig2, card, fromShowEntity: false, change, fromTagChange: false, forceActivate);
				}
			}
			if (comparisonEntry.m_AfterAlways == null)
			{
				continue;
			}
			foreach (TagVisualActionConfiguration actionConfig3 in comparisonEntry.m_AfterAlways.m_Actions)
			{
				ConditionallyExecuteAction(comparisonEntry, actionConfig3, card, fromShowEntity: false, change, fromTagChange: false, forceActivate);
			}
		}
	}

	public void DeactivateHandStateSpells(Card card, Actor actor)
	{
		if (card == null || actor == null || card.GetEntity() == null)
		{
			return;
		}
		foreach (TagVisualConfigurationEntry entry in m_TagVisuals)
		{
			TagVisualConfigurationEntry comparisonEntry = entry;
			if (entry.m_ReferenceTag != 0)
			{
				comparisonEntry = FindTagEntry(entry.m_ReferenceTag);
			}
			if (comparisonEntry == null || !comparisonEntry.m_IsHandStateSpell)
			{
				continue;
			}
			TagDelta change = new TagDelta();
			change.tag = (int)entry.m_Tag;
			change.oldValue = 0;
			change.newValue = card.GetEntity().GetTag(entry.m_Tag);
			if (comparisonEntry.m_BeforeAlways != null)
			{
				foreach (TagVisualActionConfiguration actionConfig in comparisonEntry.m_BeforeAlways.m_Actions)
				{
					ConditionallyExecuteAction(comparisonEntry, actionConfig, card, fromShowEntity: false, change, fromTagChange: false, forceActivate: false, actor);
				}
			}
			if (comparisonEntry.m_TagRemoved != null)
			{
				foreach (TagVisualActionConfiguration actionConfig2 in comparisonEntry.m_TagRemoved.m_Actions)
				{
					ConditionallyExecuteAction(comparisonEntry, actionConfig2, card, fromShowEntity: false, change, fromTagChange: false, forceActivate: false, actor);
				}
			}
			if (comparisonEntry.m_AfterAlways == null)
			{
				continue;
			}
			foreach (TagVisualActionConfiguration actionConfig3 in comparisonEntry.m_AfterAlways.m_Actions)
			{
				ConditionallyExecuteAction(comparisonEntry, actionConfig3, card, fromShowEntity: false, change, fromTagChange: false, forceActivate: false, actor);
			}
		}
	}

	public void ProcessTagChange(GAME_TAG tag, Card card, bool fromShowEntity, TagDelta change)
	{
		TagVisualConfigurationEntry entry = FindTagEntry(tag);
		if (entry == null || card == null || (!card.CanShowActorVisuals() && !entry.m_IgnoreCanShowActorVisuals))
		{
			return;
		}
		if (entry.m_ReferenceTag != 0)
		{
			entry = FindTagEntry(entry.m_ReferenceTag);
			if (entry == null)
			{
				return;
			}
		}
		if (entry.m_BeforeAlways != null)
		{
			foreach (TagVisualActionConfiguration actionConfig in entry.m_BeforeAlways.m_Actions)
			{
				ConditionallyExecuteAction(entry, actionConfig, card, fromShowEntity, change);
			}
		}
		if (change.newValue != 0 && change.oldValue == 0 && entry.m_TagAdded != null)
		{
			foreach (TagVisualActionConfiguration actionConfig2 in entry.m_TagAdded.m_Actions)
			{
				ConditionallyExecuteAction(entry, actionConfig2, card, fromShowEntity, change);
			}
		}
		else if (change.newValue == 0 && change.oldValue != 0 && entry.m_TagRemoved != null)
		{
			foreach (TagVisualActionConfiguration actionConfig3 in entry.m_TagRemoved.m_Actions)
			{
				ConditionallyExecuteAction(entry, actionConfig3, card, fromShowEntity, change);
			}
		}
		if (entry.m_AfterAlways == null)
		{
			return;
		}
		foreach (TagVisualActionConfiguration actionConfig4 in entry.m_AfterAlways.m_Actions)
		{
			ConditionallyExecuteAction(entry, actionConfig4, card, fromShowEntity, change);
		}
	}

	private void ConditionallyExecuteAction(TagVisualConfigurationEntry entry, TagVisualActionConfiguration actionConfig, Card card, bool fromShowEntity, TagDelta change, bool fromTagChange = true, bool forceActivate = true, Actor overrideActor = null)
	{
		if (actionConfig != null && !(card == null) && IsActionConditionMet(actionConfig.m_Condition, card, fromShowEntity, fromTagChange, overrideActor))
		{
			ExecuteAction(actionConfig, card, change, forceActivate, overrideActor);
		}
	}

	private void ExecuteAction(TagVisualActionConfiguration actionConfig, Card card, TagDelta change, bool forceActivate, Actor overrideActor)
	{
		if (card == null)
		{
			return;
		}
		switch (actionConfig.m_Action)
		{
		case TagVisualActorFunction.ACTIVATE_SPELL_STATE:
			ActivateSpellState(actionConfig.m_SpellType, actionConfig.m_SpellState, card, forceActivate, overrideActor);
			break;
		case TagVisualActorFunction.PLAY_SOUND_PREFAB:
		{
			AssetReference soundSpellAssetRef = actionConfig.m_PlaySoundPrefabParameters;
			if (soundSpellAssetRef != null && AdditionalPlaySoundChecks(card))
			{
				SoundManager.Get().LoadAndPlay(soundSpellAssetRef);
			}
			break;
		}
		case TagVisualActorFunction.ACTIVATE_STATE_FUNCTION:
		{
			TagVisualActorStateFunction aFunction = actionConfig.m_StateFunctionParameters;
			ActivateStateFunction(aFunction, card, isActive: true, change);
			break;
		}
		case TagVisualActorFunction.DEACTIVATE_STATE_FUNCTION:
		{
			TagVisualActorStateFunction dFunction = actionConfig.m_StateFunctionParameters;
			ActivateStateFunction(dFunction, card, isActive: false, change);
			break;
		}
		case TagVisualActorFunction.UPDATE_ACTOR:
			card.UpdateActor();
			break;
		case TagVisualActorFunction.UPDATE_ACTOR_COMPONENTS:
			if (overrideActor != null)
			{
				overrideActor.UpdateAllComponents();
			}
			else
			{
				card.UpdateActorComponents();
			}
			break;
		case TagVisualActorFunction.UPDATE_ACTOR_STATE:
			card.UpdateActorState();
			break;
		case TagVisualActorFunction.UPDATE_SIDEQUEST_UI:
			card.UpdateSideQuestUI(allowQuestComplete: false);
			break;
		case TagVisualActorFunction.UPDATE_QUEST_UI:
			card.UpdateQuestUI();
			break;
		case TagVisualActorFunction.UPDATE_QUESTLINE_UI:
			card.UpdateQuestlineUI();
			break;
		case TagVisualActorFunction.UPDATE_OBJECTIVE_UI:
			card.UpdateObjectiveUI();
			break;
		case TagVisualActorFunction.UPDATE_PUZZLE_UI:
			card.UpdatePuzzleUI();
			break;
		case TagVisualActorFunction.UPDATE_HERO_POWER_VISUALS:
			card.UpdateHeroPowerRelatedVisual();
			break;
		case TagVisualActorFunction.UPDATE_TEXT_COMPONENTS:
		{
			Actor actor = card.GetActor();
			if (overrideActor != null)
			{
				actor = overrideActor;
			}
			if (actor != null)
			{
				actor.UpdateTextComponents();
			}
			break;
		}
		case TagVisualActorFunction.UPDATE_BAUBLE:
			card.UpdateBauble();
			break;
		case TagVisualActorFunction.UPDATE_ATTACHED_CARD_BAUBLE:
			if (card.GetEntity() != null)
			{
				Entity attachedEntity = GameState.Get().GetEntity(card.GetEntity().GetAttached());
				if (attachedEntity != null && attachedEntity.GetCard() != null)
				{
					attachedEntity.GetCard().UpdateBauble();
				}
			}
			break;
		case TagVisualActorFunction.ACTIVATE_LIFETIME_EFFECTS:
			card.ActivateLifetimeEffects();
			break;
		case TagVisualActorFunction.DEACTIVATE_LIFETIME_EFFECTS:
			card.DeactivateLifetimeEffects();
			break;
		case TagVisualActorFunction.CANCEL_ACTIVE_SPELLS:
			card.CancelActiveSpells();
			break;
		case TagVisualActorFunction.ACTIVATE_CUSTOM_KEYWORD_EFFECT:
			card.ActivateCustomKeywordEffect();
			break;
		case TagVisualActorFunction.DEACTIVATE_CUSTOM_KEYWORD_EFFECT:
			card.DeactivateCustomKeywordEffect();
			break;
		case TagVisualActorFunction.ACTIVATE_STATE_SPELLS:
			card.ActivateStateSpells();
			break;
		case TagVisualActorFunction.SPELL_POWER_MOUSE_OVER_EVENT:
		{
			Entity spellpowerEntity2 = card.GetEntity();
			if (spellpowerEntity2 == null)
			{
				ZoneMgr.Get().OnSpellPowerEntityMousedOver();
			}
			else
			{
				ZoneMgr.Get().OnSpellPowerEntityMousedOver(spellpowerEntity2.GetSpellPowerSchool());
			}
			break;
		}
		case TagVisualActorFunction.SPELL_POWER_MOUSE_OUT_EVENT:
		{
			Entity spellpowerEntity = card.GetEntity();
			if (spellpowerEntity == null)
			{
				ZoneMgr.Get().OnSpellPowerEntityMousedOut();
			}
			else
			{
				ZoneMgr.Get().OnSpellPowerEntityMousedOut(spellpowerEntity.GetSpellPowerSchool());
			}
			break;
		}
		case TagVisualActorFunction.HEALING_DOES_DAMAGE_MOUSE_OVER_EVENT:
			ZoneMgr.Get().OnHealingDoesDamageEntityMousedOver();
			break;
		case TagVisualActorFunction.HEALING_DOES_DAMAGE_MOUSE_OUT_EVENT:
			ZoneMgr.Get().OnHealingDoesDamageEntityMousedOut();
			break;
		case TagVisualActorFunction.LIFESTEAL_DOES_DAMAGE_MOUSE_OVER_EVENT:
			ZoneMgr.Get().OnLifestealDoesDamageEntityMousedOver();
			break;
		case TagVisualActorFunction.LIFESTEAL_DOES_DAMAGE_MOUSE_OUT_EVENT:
			ZoneMgr.Get().OnLifestealDoesDamageEntityMousedOut();
			break;
		case TagVisualActorFunction.UPDATE_WATERMARK:
		{
			Actor watermarkActor = card.GetActor();
			if (overrideActor != null)
			{
				watermarkActor = overrideActor;
			}
			Entity watermarkEntity = card.GetEntity();
			if (watermarkActor != null && watermarkEntity != null)
			{
				watermarkActor.SetWatermarkCardSetOverride(watermarkEntity.GetWatermarkCardSetOverride());
				watermarkActor.UpdateMeshComponents();
			}
			break;
		}
		}
	}

	private void ActivateStateFunction(TagVisualActorStateFunction stateFunction, Card card, bool isActive, TagDelta change)
	{
		if (card == null || card.GetActor() == null)
		{
			return;
		}
		switch (stateFunction)
		{
		case TagVisualActorStateFunction.TAUNT:
			if (isActive)
			{
				card.GetActor().ActivateTaunt();
			}
			else
			{
				card.GetActor().DeactivateTaunt();
			}
			break;
		case TagVisualActorStateFunction.DEATHRATTLE:
			card.ToggleDeathrattle(isActive);
			break;
		case TagVisualActorStateFunction.EXHAUSTED:
			card.HandleCardExhaustedTagChanged(change);
			break;
		case TagVisualActorStateFunction.DORMANT:
			if (isActive)
			{
				card.ActivateDormantStateVisual();
			}
			else
			{
				card.DeactivateDormantStateVisual();
			}
			break;
		case TagVisualActorStateFunction.ARMS_DEALING:
			if (isActive)
			{
				card.ActivateActorArmsDealingSpell();
			}
			break;
		case TagVisualActorStateFunction.CARD_ALTERNATE_COST:
			if (isActive)
			{
				card.GetActor().ActivateAlternateCost();
			}
			else
			{
				card.GetActor().DeactivateAlternateCost();
			}
			break;
		case TagVisualActorStateFunction.COIN_MANA_GEM:
			if (isActive && card.CanShowActorVisuals() && card.CanShowCoinManaGem() && !card.GetActor().UseTechLevelManaGem())
			{
				Spell coinManaSpell = card.GetActor().GetSpell(SpellType.COIN_MANA_GEM);
				if (coinManaSpell != null)
				{
					coinManaSpell.ActivateState(SpellStateType.BIRTH);
				}
			}
			else
			{
				SpellUtils.ActivateDeathIfNecessary(card.GetActor().GetSpellIfLoaded(SpellType.COIN_MANA_GEM));
			}
			break;
		case TagVisualActorStateFunction.TECH_LEVEL_MANA_GEM:
			if (isActive && card.CanShowActorVisuals())
			{
				Spell techLevelSpell = card.GetActor().GetSpell(SpellType.TECH_LEVEL_MANA_GEM);
				if (techLevelSpell != null)
				{
					techLevelSpell.GetComponent<PlayMakerFSM>().FsmVariables.GetFsmInt("TechLevel").Value = card.GetEntity().GetTechLevel();
					techLevelSpell.ActivateState(SpellStateType.BIRTH);
				}
			}
			else
			{
				SpellUtils.ActivateDeathIfNecessary(card.GetActor().GetSpellIfLoaded(SpellType.TECH_LEVEL_MANA_GEM));
			}
			break;
		case TagVisualActorStateFunction.COIN_ON_ENEMY_MINIONS:
			if (isActive)
			{
				Spell baconShopMinionCoinSpell = card.GetActor().GetSpell(SpellType.BACON_SHOP_MINION_COIN);
				if (baconShopMinionCoinSpell != null)
				{
					baconShopMinionCoinSpell.GetComponent<PlayMakerFSM>().FsmVariables.GetFsmInt("TechLevel").Value = card.GetEntity().GetTechLevel();
					baconShopMinionCoinSpell.ActivateState(SpellStateType.BIRTH);
				}
			}
			else
			{
				SpellUtils.ActivateDeathIfNecessary(card.GetActor().GetSpellIfLoaded(SpellType.BACON_SHOP_MINION_COIN));
			}
			break;
		case TagVisualActorStateFunction.DECK_POWER_UP:
			if (isActive)
			{
				Spell powerUpSpell = card.GetActor().GetSpell(SpellType.DECK_POWER_UP);
				if (powerUpSpell != null && card.GetHeroCard() != null && card.GetHeroCard().gameObject != null)
				{
					powerUpSpell.SetSource(card.GetHeroCard().gameObject);
					powerUpSpell.ForceUpdateTransform();
					SpellUtils.ActivateBirthIfNecessary(powerUpSpell);
				}
			}
			else
			{
				SpellUtils.ActivateDeathIfNecessary(card.GetActor().GetSpellIfLoaded(SpellType.DECK_POWER_UP));
			}
			break;
		case TagVisualActorStateFunction.EVIL_TWIN_MUSTACHE:
			if (isActive)
			{
				card.GetActor().ActivateEvilTwinMustache();
			}
			else
			{
				card.GetActor().DeactivateEvilTwinMustache();
			}
			break;
		case TagVisualActorStateFunction.MINION_TYPE_MASK:
			if (isActive)
			{
				card.GetActor().ActivateMinionTypeMask();
			}
			else
			{
				card.GetActor().DeactivateMinionTypeMask();
			}
			break;
		case TagVisualActorStateFunction.STARSHIP_LAUNCHPAD:
			if (isActive)
			{
				card.GetActor().ActivateLaunchpadAnimations();
			}
			else
			{
				card.GetActor().DeactivateLaunchpadAnimations();
			}
			break;
		case TagVisualActorStateFunction.CARD_COST_HEALTH:
		case TagVisualActorStateFunction.CARD_COST_ARMOR:
			break;
		}
	}

	private bool IsActionConditionMet(bool invertCondition, TagVisualActorCondition condition, GAME_TAG tag, TagVisualActorConditionComparisonOperator tagComparisonOperator, int tagValue, TagVisualActorConditionEntity tagComparisonEntity, SpellType spellType, SpellStateType spellState, Card card, bool fromShowEntity, bool fromTagChange, Actor overrideActor)
	{
		bool result = false;
		if (card == null)
		{
			return false;
		}
		Actor actor = card.GetActor();
		if (overrideActor != null)
		{
			actor = overrideActor;
		}
		switch (condition)
		{
		case TagVisualActorCondition.ALWAYS:
			result = true;
			break;
		case TagVisualActorCondition.DOES_TAG_HAVE_VALUE:
		{
			Entity comparisonEntity = card.GetEntity();
			switch (tagComparisonEntity)
			{
			case TagVisualActorConditionEntity.CONTROLLER:
				comparisonEntity = card.GetController();
				break;
			case TagVisualActorConditionEntity.HERO:
				comparisonEntity = card.GetHero();
				break;
			case TagVisualActorConditionEntity.GAME:
				comparisonEntity = ((GameState.Get() != null) ? GameState.Get().GetGameEntity() : null);
				break;
			}
			result = CompareTagValue(tagComparisonOperator, tag, tagValue, comparisonEntity);
			break;
		}
		case TagVisualActorCondition.DOES_SPELL_HAVE_STATE:
			result = CompareSpellState(spellType, spellState, card, overrideActor);
			break;
		case TagVisualActorCondition.IS_ENRAGED:
			result = card.GetEntity() != null && card.GetEntity().IsEnraged();
			break;
		case TagVisualActorCondition.IS_ASLEEP:
			result = card.GetEntity() != null && card.GetEntity().IsAsleep();
			break;
		case TagVisualActorCondition.IS_FRIENDLY:
			result = card.GetEntity() != null && card.GetEntity().GetController() != null && card.GetEntity().GetController().IsFriendlySide();
			break;
		case TagVisualActorCondition.IS_MOUSED_OVER:
			result = card.IsMousedOver();
			break;
		case TagVisualActorCondition.IS_ENCHANTMENT:
			result = card.GetEntity() != null && card.GetEntity().IsEnchantment();
			break;
		case TagVisualActorCondition.IS_DISABLED_HERO_POWER:
			result = card.GetEntity() != null && card.GetEntity().IsDisabledHeroPower();
			break;
		case TagVisualActorCondition.IS_FROM_SHOW_ENTITY:
			result = fromShowEntity;
			break;
		case TagVisualActorCondition.IS_FROM_TAG_CHANGE:
			result = fromTagChange;
			break;
		case TagVisualActorCondition.IS_REAL_TIME_DORMANT:
			result = card.GetEntity() != null && card.GetEntity().GetRealTimeIsDormant();
			break;
		case TagVisualActorCondition.IS_AI_CONTROLLER:
			result = card.GetEntity() != null && card.GetEntity().GetController() != null && card.GetEntity().GetController().IsAI();
			break;
		case TagVisualActorCondition.IS_VALID_OPTION:
			result = GameState.Get() != null && GameState.Get().IsValidOption(card.GetEntity(), null);
			break;
		case TagVisualActorCondition.SHOULD_SHOW_IMMUNE_VISUALS:
			result = card.ShouldShowImmuneVisuals();
			break;
		case TagVisualActorCondition.CAN_SHOW_ACTOR_VISUALS:
			result = card.CanShowActorVisuals();
			break;
		case TagVisualActorCondition.ATTACHED_CARD_CAN_SHOW_ACTOR_VISUALS:
			if (card.GetEntity() != null)
			{
				Entity attachedEntity = GameState.Get().GetEntity(card.GetEntity().GetAttached());
				result = attachedEntity != null && attachedEntity.GetCard() != null && attachedEntity.GetCard().CanShowActorVisuals();
			}
			break;
		case TagVisualActorCondition.SHOULD_USE_TECH_LEVEL_MANA_GEM:
			result = actor != null && actor.UseTechLevelManaGem();
			break;
		case TagVisualActorCondition.SHOULD_USE_COIN_ON_ENEMY_MINIONS:
			result = actor != null && actor.GetEntity() != null && !actor.GetEntity().IsControlledByFriendlySidePlayer() && GameState.Get() != null && GameState.Get().GetGameEntity() != null && GameState.Get().GetGameEntity().HasTag(GAME_TAG.BACON_COIN_ON_ENEMY_MINIONS);
			break;
		case TagVisualActorCondition.SHOULD_USE_COST_HEALTH_TO_BUY:
			if (actor != null && actor.GetEntity() != null)
			{
				bool num = !actor.GetEntity().IsControlledByFriendlySidePlayer();
				bool hasBaconCostsHealthToBuyTag = actor.GetEntity().HasTag(GAME_TAG.BACON_COSTS_HEALTH_TO_BUY);
				bool isInCombat = ZoneMgr.Get() != null && ZoneMgr.Get().IsBattlegroundBattlePhase();
				result = num && !isInCombat && hasBaconCostsHealthToBuyTag;
			}
			break;
		case TagVisualActorCondition.HAS_USABLE_TITAN_ABILITY:
		{
			result = false;
			Entity entity = card.GetEntity();
			if (entity != null && entity.IsTitan() && (!entity.HasTag(GAME_TAG.TITAN_ABILITY_USED_1) || !entity.HasTag(GAME_TAG.TITAN_ABILITY_USED_2) || !entity.HasTag(GAME_TAG.TITAN_ABILITY_USED_3)))
			{
				result = true;
			}
			break;
		}
		case TagVisualActorCondition.IS_SPELL:
			result = card.GetEntity() != null && card.GetEntity().IsSpell();
			break;
		}
		if (invertCondition)
		{
			result = !result;
		}
		return result;
	}

	private bool IsActionConditionMet(TagVisualActorConditionConfiguration condition, Card card, bool fromShowEntity, bool fromTagChange, Actor overrideActor)
	{
		bool result = false;
		switch (condition.m_Condition)
		{
		case TagVisualActorCondition.ALWAYS:
			result = true;
			break;
		case TagVisualActorCondition.AND:
			result = IsActionConditionMet(condition.m_Parameters.m_InvertConditionLHS, condition.m_Parameters.m_ConditionLHS, condition.m_Parameters.m_TagLHS, condition.m_Parameters.m_ComparisonOperatorLHS, condition.m_Parameters.m_ValueLHS, condition.m_Parameters.m_TagComparisonEntityLHS, condition.m_Parameters.m_SpellTypeLHS, condition.m_Parameters.m_SpellStateLHS, card, fromShowEntity, fromTagChange, overrideActor) && IsActionConditionMet(condition.m_Parameters.m_InvertConditionRHS, condition.m_Parameters.m_ConditionRHS, condition.m_Parameters.m_TagRHS, condition.m_Parameters.m_ComparisonOperatorRHS, condition.m_Parameters.m_ValueRHS, condition.m_Parameters.m_TagComparisonEntityRHS, condition.m_Parameters.m_SpellTypeRHS, condition.m_Parameters.m_SpellStateRHS, card, fromShowEntity, fromTagChange, overrideActor);
			if (condition.m_InvertCondition)
			{
				result = !result;
			}
			break;
		case TagVisualActorCondition.OR:
			result = IsActionConditionMet(condition.m_Parameters.m_InvertConditionLHS, condition.m_Parameters.m_ConditionLHS, condition.m_Parameters.m_TagLHS, condition.m_Parameters.m_ComparisonOperatorLHS, condition.m_Parameters.m_ValueLHS, condition.m_Parameters.m_TagComparisonEntityLHS, condition.m_Parameters.m_SpellTypeLHS, condition.m_Parameters.m_SpellStateLHS, card, fromShowEntity, fromTagChange, overrideActor) || IsActionConditionMet(condition.m_Parameters.m_InvertConditionRHS, condition.m_Parameters.m_ConditionRHS, condition.m_Parameters.m_TagRHS, condition.m_Parameters.m_ComparisonOperatorRHS, condition.m_Parameters.m_ValueRHS, condition.m_Parameters.m_TagComparisonEntityRHS, condition.m_Parameters.m_SpellTypeRHS, condition.m_Parameters.m_SpellStateRHS, card, fromShowEntity, fromTagChange, overrideActor);
			if (condition.m_InvertCondition)
			{
				result = !result;
			}
			break;
		default:
			result = IsActionConditionMet(condition.m_InvertCondition, condition.m_Condition, condition.m_Parameters.m_Tag, condition.m_Parameters.m_ComparisonOperator, condition.m_Parameters.m_Value, condition.m_Parameters.m_TagComparisonEntity, condition.m_Parameters.m_SpellType, condition.m_Parameters.m_SpellState, card, fromShowEntity, fromTagChange, overrideActor);
			break;
		}
		return result;
	}

	private void ActivateSpellState(SpellType spellType, SpellStateType spellState, Card card, bool forceActivate, Actor overrideActor)
	{
		if (card == null)
		{
			return;
		}
		Actor actor = card.GetActor();
		if (overrideActor != null)
		{
			actor = overrideActor;
		}
		if (!(actor != null))
		{
			return;
		}
		Spell spell = ((spellState == SpellStateType.BIRTH) ? actor.GetSpell(spellType) : actor.GetSpellIfLoaded(spellType));
		if (!(spell != null))
		{
			return;
		}
		if (forceActivate)
		{
			spell.ActivateState(spellState);
			if (spell.GetSource() == null && card != null)
			{
				spell.SetSource(card.gameObject);
			}
		}
		else if (SpellUtils.ActivateStateIfNecessary(spell, spellState) && spell.GetSource() == null && card != null)
		{
			spell.SetSource(card.gameObject);
		}
	}

	private bool CompareSpellState(SpellType spellType, SpellStateType spellState, Card card, Actor overrideActor)
	{
		bool result = false;
		if (card == null)
		{
			return false;
		}
		Actor actor = card.GetActor();
		if (overrideActor != null)
		{
			actor = overrideActor;
		}
		if (actor != null)
		{
			Spell spell = actor.GetSpellIfLoaded(spellType);
			if (spell != null)
			{
				return spell.GetActiveState() == spellState;
			}
			return spellState == SpellStateType.NONE;
		}
		return result;
	}

	private bool CompareTagValue(TagVisualActorConditionComparisonOperator op, GAME_TAG tag, int value, Entity entity)
	{
		bool result = false;
		if (entity == null)
		{
			return false;
		}
		switch (op)
		{
		case TagVisualActorConditionComparisonOperator.EQUAL:
			result = entity.GetTag(tag) == value;
			break;
		case TagVisualActorConditionComparisonOperator.GREATER_THAN:
			result = entity.GetTag(tag) > value;
			break;
		case TagVisualActorConditionComparisonOperator.GREATER_THAN_OR_EQUAL:
			result = entity.GetTag(tag) >= value;
			break;
		case TagVisualActorConditionComparisonOperator.LESS_THAN:
			result = entity.GetTag(tag) < value;
			break;
		case TagVisualActorConditionComparisonOperator.LESS_THAN_OR_EQUAL:
			result = entity.GetTag(tag) <= value;
			break;
		}
		return result;
	}

	private TagVisualConfigurationEntry FindTagEntry(GAME_TAG tag)
	{
		foreach (TagVisualConfigurationEntry entry in m_TagVisuals)
		{
			if (entry.m_Tag == tag)
			{
				return entry;
			}
		}
		return null;
	}

	private bool AdditionalPlaySoundChecks(Card card)
	{
		if (card == null || card.GetActor() == null)
		{
			return true;
		}
		if (TeammateBoardViewer.Get() != null && card.GetActor().IsTeammateActor() && !TeammateBoardViewer.Get().IsViewingTeammate())
		{
			return false;
		}
		return true;
	}
}
