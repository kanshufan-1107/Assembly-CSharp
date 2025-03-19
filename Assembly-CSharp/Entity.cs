using System;
using System.Collections.Generic;
using System.Linq;
using PegasusGame;
using Unity.Profiling;
using UnityEngine;

public class Entity : EntityBase
{
	public enum LoadState
	{
		INVALID,
		PENDING,
		LOADING,
		DONE
	}

	public class LoadCardData
	{
		public bool updateActor;

		public bool restartStateSpells;

		public bool fromChangeEntity;
	}

	private struct CachedDebugName
	{
		public bool Dirty;

		public string Name;
	}

	public enum RerollButtonEnableResult
	{
		FREE = 0,
		REROLL = 1,
		FREE_UNLOCK = 2,
		UNLOCK = 3,
		ENABLED = 3,
		OUT_OF_CURRENCY = 4,
		HERO_REROLL_LIMITATION_REACHED = 5,
		INSUFFICIENT_MULLIGAN_TIME_LEFT = 6,
		MULLIGAN_NOT_ACTIVE = 7,
		LOCKED = 8
	}

	private class EnchantmentComparer : IEqualityComparer<Entity>
	{
		public bool Equals(Entity a, Entity b)
		{
			if (a == b)
			{
				return true;
			}
			if (a == null || b == null)
			{
				return false;
			}
			if (a.GetCardId() != b.GetCardId())
			{
				return false;
			}
			string cardTextInHand = a.GetCardTextInHand();
			string textB = b.GetCardTextInHand();
			return cardTextInHand == textB;
		}

		public int GetHashCode(Entity entity)
		{
			if (entity == null)
			{
				return 0;
			}
			if (entity.GetCardId() == null)
			{
				return 0;
			}
			return entity.GetCardId().GetHashCode();
		}
	}

	private EntityDef m_staticEntityDef = new EntityDef();

	private EntityDef m_dynamicEntityDef;

	protected Card m_card;

	private LoadState m_loadState;

	private int m_cardAssetLoadCount;

	private bool m_useBattlecryPower;

	private bool m_duplicateForHistory;

	private CardTextHistoryData m_cardTextHistoryData;

	private List<Entity> m_attachments = new List<Entity>();

	private List<int> m_subCardIDs = new List<int>();

	private List<int> m_lettuceAbilityEntityIDs = new List<int>();

	private int m_realTimeCost;

	private int m_realTimeAttack;

	private int m_realTimeHealth;

	private int m_realTimeDamage;

	private int m_realTimeArmor;

	private int m_realTimeZone;

	private int m_realTimeZonePosition;

	private int m_realTimeLinkedEntityId;

	private int m_realTimeParentEntityId;

	private bool m_realTimePoweredUp;

	private bool m_realTimeDivineShield;

	private bool m_realTimeIsImmune;

	private bool m_realTimeIsImmuneWhileAttacking;

	private bool m_realTimeIsPoisonous;

	private bool m_realTimeIsVenomous;

	private bool m_realTimeIsDormant;

	private int m_realTimeSpellpower;

	private bool m_realTimeSpellpowerDouble;

	private bool m_realTimeHealingDoesDamageHint;

	private bool m_realTimeLifestealDoesDamageHint;

	private bool m_realTimeCardCostsHealth;

	private bool m_realTimeCardCostsArmor;

	private bool m_realTimeCardCostsCorpses;

	private bool m_realTimeAttackableByRush;

	private bool m_realTimeBaconCombatPhaseHero;

	private bool m_realTimeBaconDamageCapEnabled;

	private int m_realTimeDeckActionCost;

	private bool m_realTimeIsTitan;

	private bool m_realTimeTitanAbilityUsed1;

	private bool m_realTimeTitanAbilityUsed2;

	private bool m_realTimeTitanAbilityUsed3;

	private TAG_CARDTYPE m_realTimeCardType;

	private TAG_PREMIUM m_realTimePremium;

	private int m_realTimePlayerLeaderboardPlace;

	private int m_realTimePlayerTechLevel;

	private int m_realTimePlayerFightsFirst;

	private int m_realTimeBaconDuoPassable;

	private int m_queuedRealTimeControllerTagChangeCount;

	private int m_queuedChangeEntityCount;

	private List<Network.HistChangeEntity> m_transformPowersProcessed = new List<Network.HistChangeEntity>();

	private string m_displayedCreatorName;

	private string m_enchantmentCreatorCardIDForPortrait;

	private CachedDebugName m_cachedDebugName;

	private static ProfilerMarker s_cardInitMarker = new ProfilerMarker("Entity.CardInit");

	public override string ToString()
	{
		return GetDebugName();
	}

	public virtual void OnRealTimeFullEntity(Network.HistFullEntity fullEntity)
	{
		SetTags(fullEntity.Entity.Tags);
		SetTagLists(fullEntity.Entity.TagLists);
		InitRealTimeValues(fullEntity.Entity.Tags);
		InitCard();
		LoadEntityDef(fullEntity.Entity.CardID);
	}

	public void OnFullEntity(Network.HistFullEntity fullEntity)
	{
		m_loadState = LoadState.PENDING;
		LoadCard(fullEntity.Entity.CardID);
		int attachedToId = GetTag(GAME_TAG.ATTACHED);
		if (attachedToId != 0)
		{
			GameState.Get().GetEntity(attachedToId).AddAttachment(this);
		}
		int parentId = GetTag(GAME_TAG.PARENT_CARD);
		if (parentId != 0)
		{
			Entity parent = GameState.Get().GetEntity(parentId);
			if (parent != null)
			{
				parent.AddSubCard(this);
			}
			else
			{
				Log.Gameplay.PrintError("Unable to find parent entity id={0}", parentId);
			}
		}
		int lettuceabilityOwnerID = GetTag(GAME_TAG.LETTUCE_ABILITY_OWNER);
		if (lettuceabilityOwnerID != 0 && IsLettuceAbility())
		{
			GameState.Get().GetEntity(lettuceabilityOwnerID)?.AddLettuceAbilityEntityID(GetEntityId());
		}
		if (GetZone() == TAG_ZONE.PLAY)
		{
			if (IsHero())
			{
				GetController().SetHero(this);
				if (HasTag(GAME_TAG.BACON_IS_KEL_THUZAD))
				{
					PlayerLeaderboardManager.Get().SetOddManOutOpponentHero(this);
				}
			}
			else if (IsHeroPower())
			{
				GetController().SetHeroPower(this);
			}
		}
		if (fullEntity.Entity.DefTags.Count > 0)
		{
			EntityDef entDef = GetOrCreateDynamicDefinition();
			for (int i = 0; i < fullEntity.Entity.DefTags.Count; i++)
			{
				entDef.SetTag(fullEntity.Entity.DefTags[i].Name, fullEntity.Entity.DefTags[i].Value);
			}
		}
		if (HasTag(GAME_TAG.DISPLAYED_CREATOR))
		{
			SetDisplayedCreatorName(GetTag(GAME_TAG.DISPLAYED_CREATOR));
		}
		if (HasTag(GAME_TAG.CREATOR_DBID))
		{
			ResolveEnchantmentPortraitCardID(GetTag(GAME_TAG.CREATOR_DBID));
		}
		if (HasTag(GAME_TAG.PLAYER_LEADERBOARD_PLACE) && GetRealTimeZone() != TAG_ZONE.GRAVEYARD)
		{
			PlayerLeaderboardManager.Get().CreatePlayerTile(this);
			int playerId = GetTag(GAME_TAG.PLAYER_ID);
			if (GameState.Get().GetPlayerInfoMap().ContainsKey(playerId))
			{
				GameState.Get().GetPlayerInfoMap()[playerId].SetPlayerHero(this);
			}
			if (HasTag(GAME_TAG.REPLACEMENT_ENTITY))
			{
				PlayerLeaderboardManager.Get().ApplyEntityReplacement(playerId, this);
			}
		}
	}

	public virtual void OnRealTimeShowEntity(Network.HistShowEntity showEntity)
	{
		HandleRealTimeEntityChange(showEntity.Entity);
	}

	public void OnShowEntity(Network.HistShowEntity showEntity)
	{
		HandleEntityChange(showEntity.Entity, new LoadCardData
		{
			updateActor = false,
			restartStateSpells = false,
			fromChangeEntity = false
		}, fromShowEntity: true);
	}

	public void OnHideEntity(Network.HistHideEntity hideEntity)
	{
		SetTagAndHandleChange(GAME_TAG.ZONE, hideEntity.Zone);
		EntityDef entityDef = GetEntityDef();
		SetTag(GAME_TAG.ATK, entityDef.GetATK());
		SetTag(GAME_TAG.HEALTH, entityDef.GetHealth());
		SetTag(GAME_TAG.COST, entityDef.GetCost());
		SetTag(GAME_TAG.DAMAGE, 0);
		SetCardId(null);
	}

	public virtual void OnRealTimeChangeEntity(List<Network.PowerHistory> powerList, int index, Network.HistChangeEntity changeEntity)
	{
		m_queuedChangeEntityCount++;
		HandleRealTimeEntityChange(changeEntity.Entity);
		CheckRealTimeTransform(powerList, index, changeEntity);
	}

	public void OnChangeEntity(Network.HistChangeEntity changeEntity)
	{
		if (m_transformPowersProcessed.Contains(changeEntity))
		{
			m_transformPowersProcessed.Remove(changeEntity);
			return;
		}
		m_subCardIDs.Clear();
		m_queuedChangeEntityCount--;
		LoadCardData data = new LoadCardData
		{
			updateActor = ShouldUpdateActorOnChangeEntity(changeEntity),
			restartStateSpells = ShouldRestartStateSpellsOnChangeEntity(changeEntity),
			fromChangeEntity = true
		};
		HandleEntityChange(changeEntity.Entity, data, fromShowEntity: false);
	}

	private bool IsTagChanged(Network.HistChangeEntity changeEntity, GAME_TAG tag)
	{
		Network.Entity.Tag newTag = changeEntity.Entity.Tags.Find((Network.Entity.Tag currTag) => currTag.Name == (int)tag);
		if (newTag != null)
		{
			return GetTag(tag) != newTag.Value;
		}
		return false;
	}

	private bool SignatureFrameWillChange(string cardId, string changeId)
	{
		if (string.IsNullOrEmpty(cardId) || string.IsNullOrEmpty(changeId))
		{
			return false;
		}
		if (GetTag<TAG_PREMIUM>(GAME_TAG.PREMIUM) == TAG_PREMIUM.SIGNATURE)
		{
			return ActorNames.GetSignatureFrameId(cardId) != ActorNames.GetSignatureFrameId(changeId);
		}
		return false;
	}

	private bool ShouldUpdateActorOnChangeEntity(Network.HistChangeEntity changeEntity)
	{
		if (!IsTagChanged(changeEntity, GAME_TAG.CARDTYPE) && GetTag(GAME_TAG.CARDTYPE) == (int)m_realTimeCardType && !IsTagChanged(changeEntity, GAME_TAG.PREMIUM) && GetTag(GAME_TAG.PREMIUM) == (int)m_realTimePremium && !IsTagChanged(changeEntity, GAME_TAG.LETTUCE_MERCENARY))
		{
			return SignatureFrameWillChange(base.m_cardId, changeEntity.Entity.CardID);
		}
		return true;
	}

	private bool ShouldRestartStateSpellsOnChangeEntity(Network.HistChangeEntity changeEntity)
	{
		if (GameState.Get() != null && GameState.Get().GetGameEntity().HasTag(GAME_TAG.BACON_COIN_ON_ENEMY_MINIONS) && !IsControlledByFriendlySidePlayer() && IsMinion() && GetZone() == TAG_ZONE.PLAY)
		{
			return true;
		}
		return IsTagChanged(changeEntity, GAME_TAG.ELITE);
	}

	public virtual void OnRealTimeTagChanged(Network.HistTagChange change)
	{
		switch ((GAME_TAG)change.Tag)
		{
		case GAME_TAG.COST:
			SetRealTimeCost(change.Value);
			break;
		case GAME_TAG.ATK:
			SetRealTimeAttack(change.Value);
			break;
		case GAME_TAG.HEALTH:
		case GAME_TAG.DURABILITY:
			SetRealTimeHealth(change.Value);
			break;
		case GAME_TAG.DAMAGE:
			SetRealTimeDamage(change.Value);
			break;
		case GAME_TAG.ARMOR:
			SetRealTimeArmor(change.Value);
			break;
		case GAME_TAG.ZONE:
			SetRealTimeZone(change.Value);
			break;
		case GAME_TAG.ZONE_POSITION:
			SetRealTimeZonePosition(change.Value);
			ZoneMgr.Get().OnRealTimeZonePosChange(this);
			break;
		case GAME_TAG.POWERED_UP:
			SetRealTimePoweredUp(change.Value);
			break;
		case GAME_TAG.LINKED_ENTITY:
			SetRealTimeLinkedEntityId(change.Value);
			break;
		case GAME_TAG.PARENT_CARD:
			SetRealTimeParentEntityId(change.Value);
			break;
		case GAME_TAG.DIVINE_SHIELD:
			SetRealTimeDivineShield(change.Value);
			break;
		case GAME_TAG.IMMUNE:
			SetRealTimeIsImmune(change.Value);
			break;
		case GAME_TAG.IMMUNE_WHILE_ATTACKING:
			SetRealTimeIsImmuneWhileAttacking(change.Value);
			break;
		case GAME_TAG.POISONOUS:
		case GAME_TAG.NON_KEYWORD_POISONOUS:
			SetRealTimeIsPoisonous(change.Value);
			break;
		case GAME_TAG.VENOMOUS:
			SetRealTimeIsVenomous(change.Value);
			break;
		case GAME_TAG.SPELLPOWER:
			SetRealTimeHasSpellpower(change.Value);
			break;
		case GAME_TAG.SPELLPOWER_DOUBLE:
			SetRealTimeSpellpowerDouble(change.Value);
			break;
		case GAME_TAG.HEALING_DOES_DAMAGE_HINT:
			SetRealTimeHealingDoesDamageHint(change.Value);
			break;
		case GAME_TAG.LIFESTEAL_DOES_DAMAGE_HINT:
			SetRealTimeLifestealDoesDamageHint(change.Value);
			break;
		case GAME_TAG.DORMANT:
			SetRealTimeIsDormant(change.Value);
			break;
		case GAME_TAG.CARDTYPE:
			SetRealTimeCardType((TAG_CARDTYPE)change.Value);
			break;
		case GAME_TAG.CARD_ALTERNATE_COST:
		{
			TAG_CARD_ALTERNATE_COST cost = (TAG_CARD_ALTERNATE_COST)change.Value;
			SetRealTimeCardCostsHealth(cost == TAG_CARD_ALTERNATE_COST.HEALTH);
			SetRealTimeCardCostsArmor(cost == TAG_CARD_ALTERNATE_COST.ARMOR);
			SetRealTimeCardCostsCorpses(cost == TAG_CARD_ALTERNATE_COST.CORPSES);
			break;
		}
		case GAME_TAG.ATTACKABLE_BY_RUSH:
			SetRealTimeAttackableByRush(change.Value);
			break;
		case GAME_TAG.PUZZLE_COMPLETED:
			OnRealTimePuzzleCompleted(change.Value);
			break;
		case GAME_TAG.PREMIUM:
			SetRealTimePremium((TAG_PREMIUM)change.Value);
			break;
		case GAME_TAG.PLAYER_LEADERBOARD_PLACE:
			SetRealTimePlayerLeaderboardPlace(change.Value);
			if (!(GameState.Get().GetGameEntity() is TB_BaconShop_Tutorial) || GetControllerSide() != Player.Side.OPPOSING)
			{
				UpdateSharedPlayer();
			}
			break;
		case GAME_TAG.BACON_DUO_PLAYER_FIGHTS_FIRST_NEXT_COMBAT:
			SetRealTimePlayerFightsFirst(change.Value);
			break;
		case GAME_TAG.PLAYER_TECH_LEVEL:
			if (!GetRealTimeBaconCombatPhaseHero())
			{
				SetRealTimePlayerTechLevel(change.Value);
				PlayerLeaderboardManager.Get().NotifyPlayerTileEvent(GetTag(GAME_TAG.PLAYER_ID), PlayerLeaderboardManager.PlayerTileEvent.TECH_LEVEL);
			}
			break;
		case GAME_TAG.BACON_DUO_PASSABLE:
			SetRealTimeBaconDuoPassable(change.Value);
			break;
		case GAME_TAG.PLAYER_TRIPLES:
			if (change.Value != 0 && !GetRealTimeBaconCombatPhaseHero())
			{
				PlayerLeaderboardManager.Get().NotifyPlayerTileEvent(GetTag(GAME_TAG.PLAYER_ID), PlayerLeaderboardManager.PlayerTileEvent.TRIPLE);
			}
			break;
		case GAME_TAG.BACON_PLAYER_NUM_HERO_BUDDIES_GAINED:
			switch (change.Value)
			{
			case 1:
				PlayerLeaderboardManager.Get().NotifyPlayerTileEvent(GetTag(GAME_TAG.PLAYER_ID), PlayerLeaderboardManager.PlayerTileEvent.HERO_BUDDY);
				break;
			case 2:
				PlayerLeaderboardManager.Get().NotifyPlayerTileEvent(GetTag(GAME_TAG.PLAYER_ID), PlayerLeaderboardManager.PlayerTileEvent.DOUBLE_HERO_BUDDY);
				break;
			default:
				PlayerLeaderboardManager.Get().NotifyPlayerTileEvent(GetTag(GAME_TAG.PLAYER_ID), PlayerLeaderboardManager.PlayerTileEvent.DOUBLE_HERO_BUDDY);
				Debug.LogWarning($"Unexpected Number of Hero Buddies gained: {change.Value}");
				break;
			case 0:
				break;
			}
			break;
		case GAME_TAG.BACON_MUKLA_BANANA_SPAWN_COUNT:
			if (!GetRealTimeBaconCombatPhaseHero())
			{
				PlayerLeaderboardManager.Get().NotifyPlayerTileEvent(GetTag(GAME_TAG.PLAYER_ID), PlayerLeaderboardManager.PlayerTileEvent.BANANA);
			}
			break;
		case GAME_TAG.BACON_QUEST_COMPLETED:
			if (!GetRealTimeBaconCombatPhaseHero())
			{
				PlayerLeaderboardManager.Get().NotifyPlayerTileEvent(GetTag(GAME_TAG.PLAYER_ID), PlayerLeaderboardManager.PlayerTileEvent.QUEST_COMPLETE);
			}
			break;
		case GAME_TAG.BACON_COMBAT_PHASE_HERO:
			SetRealTimeBaconCombatPhaseHero(change.Value);
			break;
		case GAME_TAG.CONTROLLER:
			m_queuedRealTimeControllerTagChangeCount++;
			break;
		case GAME_TAG.TITAN:
			SetRealTimeIsTitan(change.Value);
			break;
		case GAME_TAG.TITAN_ABILITY_USED_1:
			SetRealTimeTitanAbilityUsed1(change.Value);
			break;
		case GAME_TAG.TITAN_ABILITY_USED_2:
			SetRealTimeTitanAbilityUsed2(change.Value);
			break;
		case GAME_TAG.TITAN_ABILITY_USED_3:
			SetRealTimeTitanAbilityUsed3(change.Value);
			break;
		case GAME_TAG.DECK_ACTION_COST:
			SetRealTimeDeckActionCost(change.Value);
			break;
		}
	}

	private void UpdateSharedPlayer()
	{
		PlayerLeaderboardManager.Get().CreatePlayerTile(this);
		int playerId = GetTag(GAME_TAG.PLAYER_ID);
		if (playerId == 0)
		{
			playerId = GetTag(GAME_TAG.CONTROLLER);
		}
		if (GameState.Get().GetPlayerInfoMap().ContainsKey(playerId) && GameState.Get().GetPlayerInfoMap()[playerId].GetPlayerHero() == null)
		{
			GameState.Get().GetPlayerInfoMap()[playerId].SetPlayerHero(this);
		}
	}

	public void OnRealTimePuzzleCompleted(int newValue)
	{
		if (IsPuzzle() && !(m_card == null) && !(m_card.GetActor() == null))
		{
			PuzzleController puzzleController = m_card.GetActor().GetComponent<PuzzleController>();
			if (puzzleController == null)
			{
				Log.Gameplay.PrintError("Puzzle card {0} does not have a PuzzleController component.", this);
			}
			else
			{
				puzzleController.OnRealTimePuzzleCompleted(newValue);
			}
		}
	}

	public virtual void HandlePreTransformTagChanges(TagDeltaList changeList)
	{
		if (m_card != null)
		{
			m_card.DeactivateCustomKeywordEffect();
		}
	}

	public virtual void OnTagsChanged(TagDeltaList changeList, bool fromShowEntity)
	{
		bool nameDirty = false;
		for (int i = 0; i < changeList.Count; i++)
		{
			TagDelta change = changeList[i];
			if (IsNameChange(change))
			{
				nameDirty = true;
			}
			HandleTagChange(change);
		}
		if (!(m_card == null))
		{
			if (nameDirty)
			{
				UpdateCardName();
			}
			m_card.OnTagsChanged(changeList, fromShowEntity);
		}
	}

	public virtual void OnTagChanged(TagDelta change)
	{
		HandleTagChange(change);
		if (!(m_card == null))
		{
			if (IsNameChange(change))
			{
				UpdateCardName();
			}
			m_card.OnTagChanged(change, fromShowEntity: false);
		}
	}

	public void OnTagListChanged(int tag, List<int> values)
	{
		if (!(m_card == null))
		{
			m_tagLists.SetValues(tag, values);
		}
	}

	public virtual void OnCachedTagForDormantChanged(TagDelta change)
	{
		SetCachedTagForDormant(change.tag, change.newValue);
	}

	protected override void OnUpdateCardId()
	{
		UpdateCardName();
	}

	public virtual void OnMetaData(Network.HistMetaData metaData)
	{
		if (!(m_card == null))
		{
			m_card.OnMetaData(metaData);
		}
	}

	private void HandleRealTimeEntityChange(Network.Entity netEntity)
	{
		InitRealTimeValues(netEntity.Tags);
	}

	private bool HasRealTimeTransformTag(Network.Entity netEntity)
	{
		foreach (Network.Entity.Tag tag in netEntity.Tags)
		{
			if (tag.Name == 859 && tag.Value == 1)
			{
				return true;
			}
		}
		return false;
	}

	private void CheckRealTimeTransform(List<Network.PowerHistory> powerList, int index, Network.HistChangeEntity changeEntity)
	{
		if (HasRealTimeTransformTag(changeEntity.Entity) && CanRealTimeTransform(powerList, index))
		{
			OnChangeEntity(changeEntity);
			m_transformPowersProcessed.Add(changeEntity);
		}
	}

	private bool CanRealTimeTransform(List<Network.PowerHistory> powerList, int index)
	{
		for (int i = 0; i < index; i++)
		{
			Network.PowerHistory power = powerList[i];
			if (!CheckPowerHistoryForRealTimeTransform(power))
			{
				return false;
			}
		}
		foreach (PowerTaskList powerTaskList in GameState.Get().GetPowerProcessor().GetPowerQueue())
		{
			if (!CheckPowerTaskListForRealTimeTransform(powerTaskList))
			{
				return false;
			}
		}
		PowerTaskList currentTaskList = GameState.Get().GetPowerProcessor().GetCurrentTaskList();
		if (!CheckPowerTaskListForRealTimeTransform(currentTaskList))
		{
			return false;
		}
		return true;
	}

	private bool CheckPowerHistoryForRealTimeTransform(Network.PowerHistory power)
	{
		switch (power.Type)
		{
		case Network.PowerType.FULL_ENTITY:
			if (((Network.HistFullEntity)power).Entity.ID == GetEntityId())
			{
				return false;
			}
			break;
		case Network.PowerType.SHOW_ENTITY:
			if (((Network.HistShowEntity)power).Entity.ID == GetEntityId())
			{
				return false;
			}
			break;
		case Network.PowerType.HIDE_ENTITY:
			if (((Network.HistHideEntity)power).Entity == GetEntityId())
			{
				return false;
			}
			break;
		case Network.PowerType.TAG_CHANGE:
		{
			Network.HistTagChange tagChangeEntity = (Network.HistTagChange)power;
			if (tagChangeEntity.Entity == GetEntityId() && tagChangeEntity.Tag != 263 && tagChangeEntity.Tag != 385 && tagChangeEntity.Tag != 466)
			{
				return false;
			}
			break;
		}
		case Network.PowerType.CHANGE_ENTITY:
		{
			Network.HistChangeEntity changeEntity = (Network.HistChangeEntity)power;
			if (changeEntity.Entity.ID == GetEntityId() && !m_transformPowersProcessed.Contains(changeEntity))
			{
				return false;
			}
			break;
		}
		case Network.PowerType.META_DATA:
		{
			Network.HistMetaData metaDataEntity = (Network.HistMetaData)power;
			if (!CheckPowerHistoryMetaDataForRealTimeTransform(metaDataEntity))
			{
				return false;
			}
			break;
		}
		}
		return true;
	}

	private bool CheckPowerHistoryMetaDataForRealTimeTransform(Network.HistMetaData metaDataEntity)
	{
		switch (metaDataEntity.MetaType)
		{
		case HistoryMeta.Type.TARGET:
		case HistoryMeta.Type.DAMAGE:
		case HistoryMeta.Type.HEALING:
		case HistoryMeta.Type.JOUST:
		case HistoryMeta.Type.HISTORY_TARGET:
			foreach (int item in metaDataEntity.Info)
			{
				if (item == GetEntityId())
				{
					return false;
				}
			}
			break;
		case HistoryMeta.Type.SHOW_BIG_CARD:
		case HistoryMeta.Type.EFFECT_TIMING:
		case HistoryMeta.Type.OVERRIDE_HISTORY:
		case HistoryMeta.Type.HISTORY_TARGET_DONT_DUPLICATE_UNTIL_END:
		case HistoryMeta.Type.BEGIN_ARTIFICIAL_HISTORY_TILE:
		case HistoryMeta.Type.BEGIN_ARTIFICIAL_HISTORY_TRIGGER_TILE:
		case HistoryMeta.Type.BURNED_CARD:
			if (metaDataEntity.Info.Count > 0 && metaDataEntity.Info[0] == GetEntityId())
			{
				return false;
			}
			break;
		}
		return true;
	}

	private bool CheckPowerTaskListForRealTimeTransform(PowerTaskList powerTaskList)
	{
		if (powerTaskList == null)
		{
			return true;
		}
		foreach (PowerTask task in powerTaskList.GetTaskList())
		{
			Network.PowerHistory power = task.GetPower();
			if (!task.IsCompleted() && !CheckPowerHistoryForRealTimeTransform(power))
			{
				return false;
			}
		}
		return true;
	}

	private void HandleEntityChange(Network.Entity netEntity, LoadCardData data, bool fromShowEntity)
	{
		TagDeltaList changeSet = m_tags.CreateDeltas(netEntity.Tags);
		SetTags(netEntity.Tags);
		SetTagLists(netEntity.TagLists);
		HandlePreTransformTagChanges(changeSet);
		if (m_card != null)
		{
			m_card.DestroyCardDefAssetsOnEntityChanged();
		}
		LoadCard(netEntity.CardID, data);
		if (GetZone() == TAG_ZONE.HAND && GetCard() != null && GetCard().GetZone() != null)
		{
			if (data.updateActor)
			{
				GetCard().GetZone().UpdateLayout();
			}
			GetCard().UpdateActorState(forceHighlightRefresh: true);
		}
		if (netEntity.DefTags.Count > 0)
		{
			EntityDef entDef = GetOrCreateDynamicDefinition();
			for (int i = 0; i < netEntity.DefTags.Count; i++)
			{
				entDef.SetTag(netEntity.DefTags[i].Name, netEntity.DefTags[i].Value);
			}
		}
		RemoveCachedEntityRaces();
		OnTagsChanged(changeSet, fromShowEntity);
		CorpseCounter.UpdateTextAll();
	}

	private void HandleTagChange(TagDelta change)
	{
		switch ((GAME_TAG)change.tag)
		{
		case GAME_TAG.ZONE:
			UpdateUseBattlecryFlag(fromGameState: false);
			if (GameState.Get().IsTurnStartManagerActive() && change.oldValue == 2 && change.newValue == 3)
			{
				PowerTaskList currentTaskList = GameState.Get().GetPowerProcessor().GetCurrentTaskList();
				if (currentTaskList != null && currentTaskList.GetSourceEntity() == GameState.Get().GetFriendlySidePlayer())
				{
					TurnStartManager.Get().NotifyOfCardDrawn(this);
				}
			}
			if (change.newValue == 1)
			{
				if (IsHero())
				{
					GetController().SetHero(this);
					if (HasTag(GAME_TAG.BACON_IS_KEL_THUZAD))
					{
						PlayerLeaderboardManager.Get().SetOddManOutOpponentHero(this);
					}
				}
				else if (IsHeroPower())
				{
					GetController().SetHeroPower(this);
				}
			}
			if (change.newValue == 4 && IsLettuceAbility())
			{
				GetLettuceAbilityOwner()?.RemoveLettuceAbilityEntityID(GetEntityId());
			}
			CheckZoneChangeForEnchantment(change);
			if (change.newValue == 5)
			{
				GameState.Get().GetGameEntity().QueueEntityForRemoval(this);
			}
			break;
		case GAME_TAG.ATTACHED:
			GameState.Get().GetEntity(change.oldValue)?.RemoveAttachment(this);
			GameState.Get().GetEntity(change.newValue)?.AddAttachment(this);
			break;
		case GAME_TAG.PARENT_CARD:
		{
			GameState.Get().GetEntity(change.oldValue)?.RemoveSubCard(this);
			Entity newParent = GameState.Get().GetEntity(change.newValue);
			if (newParent != null)
			{
				newParent.AddSubCard(this);
			}
			else if (change.newValue != 0)
			{
				Log.Gameplay.PrintError("Unable to find parent entity id={0}", change.newValue);
			}
			break;
		}
		case GAME_TAG.CONTROLLER:
		{
			Entity parent = GetParentEntity();
			if (parent != null)
			{
				if (GameState.Get().GetFriendlyPlayerId() != change.newValue)
				{
					if (parent.GetZone() != TAG_ZONE.PLAY)
					{
						parent.RemoveSubCard(this);
					}
				}
				else
				{
					parent.AddSubCard(this);
				}
			}
			if (IsHeroPower())
			{
				GetController().SetHeroPower(this);
			}
			m_queuedRealTimeControllerTagChangeCount--;
			break;
		}
		case GAME_TAG.DISPLAYED_CREATOR:
			SetDisplayedCreatorName(change.newValue);
			break;
		case GAME_TAG.CREATOR_DBID:
			ResolveEnchantmentPortraitCardID(change.newValue);
			break;
		case GAME_TAG.HERO_POWER:
		case GAME_TAG.HERO_POWER_ENTITY:
		{
			PlayerLeaderboardManager plm = PlayerLeaderboardManager.Get();
			if (plm != null && plm.IsEnabled() && !GetRealTimeBaconCombatPhaseHero())
			{
				plm.UpdatePlayerTileHeroPower(this, change.newValue);
			}
			break;
		}
		case GAME_TAG.LETTUCE_ABILITY_OWNER:
		{
			GameState.Get().GetEntity(change.oldValue)?.RemoveLettuceAbilityEntityID(GetEntityId());
			Entity newOwner = GameState.Get().GetEntity(change.newValue);
			if (newOwner != null && IsLettuceAbility())
			{
				newOwner.AddLettuceAbilityEntityID(GetEntityId());
			}
			break;
		}
		case GAME_TAG.LETTUCE_SELECTED_ABILITY_QUEUE_ORDER:
			if (GameState.Get().GetGameEntity() is LettuceMissionEntity lettuceMissionEnt)
			{
				lettuceMissionEnt.UpdateAllMercenaryAbilityOrderBubbleText();
			}
			break;
		case GAME_TAG.BACON_HERO_BUDDY_PROGRESS:
		{
			Actor heroBuddyCardActor = TB_BaconShop.GetHeroBuddyCard(IsControlledByFriendlySidePlayer() ? Player.Side.FRIENDLY : Player.Side.OPPOSING)?.GetActor();
			if (!(heroBuddyCardActor != null))
			{
				break;
			}
			HeroBuddyWidgetProgressBar heroBuddyWidgetProgressBar = heroBuddyCardActor.GetComponent<HeroBuddyWidgetProgressBar>();
			if (!(heroBuddyWidgetProgressBar != null))
			{
				break;
			}
			Entity hero = GetController()?.GetHero();
			int numBuddiesGained = 0;
			if (hero != null)
			{
				numBuddiesGained = hero.GetTag(GAME_TAG.BACON_PLAYER_NUM_HERO_BUDDIES_GAINED);
			}
			if (heroBuddyWidgetProgressBar != null)
			{
				int progress = 100 * numBuddiesGained + change.newValue;
				if (progress > 200)
				{
					progress = 200;
				}
				heroBuddyWidgetProgressBar.UpdateProgressBar((float)progress / 200f);
			}
			break;
		}
		case GAME_TAG.BACON_PLAYER_NUM_HERO_BUDDIES_GAINED:
		{
			Card heroBuddyCard = TB_BaconShop.GetHeroBuddyCard(IsControlledByFriendlySidePlayer() ? Player.Side.FRIENDLY : Player.Side.OPPOSING);
			Actor heroBuddyCardActor2 = heroBuddyCard?.GetActor();
			if (!(heroBuddyCardActor2 != null) || !heroBuddyCardActor2.IsShown())
			{
				break;
			}
			switch (change.newValue)
			{
			case 1:
			{
				SpellUtils.ActivateBirthIfNecessary(heroBuddyCardActor2.GetLoadedSpell(SpellType.HERO_BUDDY_SINGLE));
				HeroBuddyWidgetCoinBased heroBuddyWidgetCoinBased = heroBuddyCardActor2.GetComponent<HeroBuddyWidgetCoinBased>();
				if (heroBuddyWidgetCoinBased != null)
				{
					heroBuddyWidgetCoinBased.EnterStage2();
					heroBuddyCard.GetEntity()?.SetTagAndHandleChange(GAME_TAG.EXHAUSTED, 0);
				}
				break;
			}
			case 2:
				SpellUtils.ActivateBirthIfNecessary(heroBuddyCardActor2.GetLoadedSpell(SpellType.HERO_BUDDY_DOUBLE));
				break;
			default:
				PlayerLeaderboardManager.Get().NotifyPlayerTileEvent(GetTag(GAME_TAG.PLAYER_ID), PlayerLeaderboardManager.PlayerTileEvent.DOUBLE_HERO_BUDDY);
				Debug.LogWarning($"Unexpected Number of Hero Buddies gained: {change.newValue}");
				break;
			case 0:
				break;
			}
			break;
		}
		case GAME_TAG.BACON_HERO_QUEST_REWARD_DATABASE_ID:
		case GAME_TAG.BACON_HERO_HEROPOWER_QUEST_REWARD_DATABASE_ID:
		case GAME_TAG.BACON_HERO_QUEST_REWARD_COMPLETED:
		case GAME_TAG.BACON_HERO_HEROPOWER_QUEST_REWARD_COMPLETED:
			PlayerLeaderboardManager.Get()?.NotifyPlayerTileEvent(GetTag(GAME_TAG.PLAYER_ID), PlayerLeaderboardManager.PlayerTileEvent.QUEST_UPDATE);
			break;
		case GAME_TAG.BACON_HERO_FIRST_TRINKET_LEADERBOARD_SDN1:
		case GAME_TAG.BACON_HERO_SECOND_TRINKET_LEADERBOARD_SDN1:
		case GAME_TAG.BACON_HERO_HEROPOWER_TRINKET_LEADERBOARD_SDN1:
		case GAME_TAG.BACON_HERO_FIRST_TRINKET_LEADERBOARD_SDN2:
		case GAME_TAG.BACON_HERO_SECOND_TRINKET_LEADERBOARD_SDN2:
		case GAME_TAG.BACON_HERO_HEROPOWER_TRINKET_LEADERBOARD_SDN2:
		case GAME_TAG.BACON_HERO_FIRST_TRINKET_LEADERBOARD_ALT_TEXT:
		case GAME_TAG.BACON_HERO_SECOND_TRINKET_LEADERBOARD_ALT_TEXT:
		case GAME_TAG.BACON_HERO_HEROPOWER_TRINKET_LEADERBOARD_ALT_TEXT:
			PlayerLeaderboardManager.Get()?.NotifyPlayerTileEvent(GetTag(GAME_TAG.PLAYER_ID), PlayerLeaderboardManager.PlayerTileEvent.TRINKET_TAG_UPDATE);
			break;
		case GAME_TAG.TITAN_ABILITY_USED_1:
		case GAME_TAG.TITAN_ABILITY_USED_2:
		case GAME_TAG.TITAN_ABILITY_USED_3:
		{
			Card card = GetCard();
			if (card != null)
			{
				Actor actor = card.GetActor();
				if (actor != null)
				{
					actor.UpdateTitanPips((GAME_TAG)change.tag, change.newValue);
				}
			}
			break;
		}
		}
	}

	private void SetDisplayedCreatorName(int entityID)
	{
		Entity creator = GameState.Get().GetEntity(entityID);
		if (creator == null)
		{
			m_displayedCreatorName = null;
		}
		else if (string.IsNullOrEmpty(creator.m_cardId))
		{
			m_displayedCreatorName = GameStrings.Get("GAMEPLAY_UNKNOWN_CREATED_BY");
		}
		else
		{
			m_displayedCreatorName = creator.GetName();
		}
		foreach (int subEntityID in GetSubCardIDs())
		{
			GameState.Get().GetEntity(subEntityID).SetDisplayedCreatorName(entityID);
		}
	}

	private bool HasEnchantmentPortrait(string enchantmentPortraitCardID)
	{
		if (string.IsNullOrEmpty(enchantmentPortraitCardID))
		{
			return false;
		}
		using DefLoader.DisposableCardDef def = DefLoader.Get().GetCardDef(enchantmentPortraitCardID);
		if (def == null)
		{
			return false;
		}
		EntityDef entityDef = DefLoader.Get().GetEntityDef(enchantmentPortraitCardID);
		if (entityDef == null)
		{
			return false;
		}
		TAG_PREMIUM premium = TAG_PREMIUM.NORMAL;
		Material enchantmentPortraitMat;
		if (entityDef.GetCardType() == TAG_CARDTYPE.ENCHANTMENT)
		{
			if (def.CardDef.TryGetEnchantmentPortrait(out enchantmentPortraitMat))
			{
				return true;
			}
			if (def.CardDef.GetPortraitTexture(premium) != null)
			{
				return true;
			}
		}
		else
		{
			if (def.CardDef.TryGetHistoryTileFullPortrait(premium, out enchantmentPortraitMat))
			{
				return true;
			}
			if (def.CardDef.GetPortraitTexture(premium) != null)
			{
				return true;
			}
		}
		return false;
	}

	public string GetEnchantmentCreatorCardIDForPortrait()
	{
		return m_enchantmentCreatorCardIDForPortrait;
	}

	public void SetEnchantmentPortraitCardID(int creatorDBID)
	{
		m_enchantmentCreatorCardIDForPortrait = null;
		EntityDef entityDef = DefLoader.Get().GetEntityDef(creatorDBID);
		if (entityDef != null)
		{
			if (!HasEnchantmentPortrait(entityDef.GetCardId()) && entityDef.HasTag(GAME_TAG.FALLBACK_ENCHANTMENT_PORTRAIT_DBID))
			{
				SetEnchantmentPortraitCardID(entityDef.GetTag(GAME_TAG.FALLBACK_ENCHANTMENT_PORTRAIT_DBID));
			}
			else
			{
				m_enchantmentCreatorCardIDForPortrait = entityDef.GetCardId();
			}
		}
	}

	private void ResolveEnchantmentPortraitCardID(int creatorDBID)
	{
		m_enchantmentCreatorCardIDForPortrait = null;
		if (!IsEnchantment())
		{
			return;
		}
		EntityDef entityDef = DefLoader.Get().GetEntityDef(creatorDBID);
		if (entityDef == null)
		{
			return;
		}
		m_enchantmentCreatorCardIDForPortrait = entityDef.GetCardId();
		Entity creator = GetCreator();
		while (!HasEnchantmentPortrait(m_enchantmentCreatorCardIDForPortrait))
		{
			if (creator == null || (!creator.IsEnchantment() && creator.GetCardType() != 0))
			{
				m_enchantmentCreatorCardIDForPortrait = null;
				return;
			}
			entityDef = creator.GetCreatorDef();
			creator = creator.GetCreator();
			if (entityDef == null)
			{
				m_enchantmentCreatorCardIDForPortrait = null;
				return;
			}
			m_enchantmentCreatorCardIDForPortrait = entityDef.GetCardId();
		}
		Entity attachee = GameState.Get().GetEntity(GetAttached());
		if (attachee != null && attachee.m_card != null)
		{
			attachee.m_card.UpdateTooltip();
		}
	}

	private void CheckZoneChangeForEnchantment(TagDelta change)
	{
		if (change.tag == 49 && IsEnchantment() && change.oldValue != change.newValue && (change.newValue == 5 || change.newValue == 4))
		{
			GameState.Get().GetEntity(GetAttached())?.RemoveAttachment(this);
			if (m_card != null)
			{
				m_card.Destroy();
				m_card = null;
			}
		}
	}

	private bool IsNameChange(TagDelta change)
	{
		switch ((GAME_TAG)change.tag)
		{
		case GAME_TAG.ZONE:
		case GAME_TAG.CONTROLLER:
		case GAME_TAG.ENTITY_ID:
		case GAME_TAG.CARDTYPE:
		case GAME_TAG.ZONE_POSITION:
		case GAME_TAG.OVERRIDECARDNAME:
		case GAME_TAG.CARD_NAME_DATA_1:
			return true;
		default:
			return false;
		}
	}

	public EntityDef GetEntityDef()
	{
		if (m_dynamicEntityDef == null)
		{
			return m_staticEntityDef;
		}
		return m_dynamicEntityDef;
	}

	public EntityDef GetOrCreateDynamicDefinition()
	{
		if (m_dynamicEntityDef == null)
		{
			m_dynamicEntityDef = m_staticEntityDef.Clone();
			m_staticEntityDef = null;
		}
		return m_dynamicEntityDef;
	}

	public Card InitCard()
	{
		using (s_cardInitMarker.Auto())
		{
			GameObject cardObject = AssetLoader.Get().InstantiatePrefab("BaseCard.prefab:465d44bb92c351f48ba03163aa012389");
			m_card = cardObject.GetComponent<Card>();
			m_card.SetEntity(this);
			UpdateCardName();
			return m_card;
		}
	}

	public DefLoader.DisposableCardDef ShareDisposableCardDef()
	{
		if (m_duplicateForHistory)
		{
			return GetCardDefForHistory();
		}
		if (m_card != null)
		{
			return m_card.ShareDisposableCardDef();
		}
		if (!string.IsNullOrEmpty(base.m_cardId))
		{
			return DefLoader.Get().GetCardDef(base.m_cardId);
		}
		return null;
	}

	private DefLoader.DisposableCardDef GetCardDefForHistory()
	{
		if (m_card != null)
		{
			if (IsHidden() && !m_card.HasHiddenCardDef)
			{
				return DefLoader.Get().GetCardDef("HiddenCard");
			}
			if (base.m_cardId == m_card.GetEntity().GetCardId())
			{
				return m_card.ShareDisposableCardDef();
			}
		}
		if (!string.IsNullOrEmpty(base.m_cardId))
		{
			return DefLoader.Get().GetCardDef(base.m_cardId);
		}
		return DefLoader.Get().GetCardDef("HiddenCard");
	}

	public Card GetCard()
	{
		return m_card;
	}

	public void SetCard(Card card)
	{
		m_card = card;
	}

	public void Destroy()
	{
		if (m_card != null)
		{
			m_card.Destroy();
			m_card = null;
		}
	}

	public LoadState GetLoadState()
	{
		return m_loadState;
	}

	public bool IsLoadingAssets()
	{
		return m_loadState == LoadState.LOADING;
	}

	public bool IsBusy()
	{
		if (IsLoadingAssets())
		{
			return true;
		}
		if (m_card != null && !m_card.IsActorReady())
		{
			return true;
		}
		return false;
	}

	public bool IsHidden()
	{
		return string.IsNullOrEmpty(base.m_cardId);
	}

	public bool HasQueuedChangeEntity()
	{
		return m_queuedChangeEntityCount > 0;
	}

	public bool HasQueuedControllerTagChange()
	{
		return m_queuedRealTimeControllerTagChangeCount > 0;
	}

	public void SetTagAndHandleChange<TagEnum>(GAME_TAG tag, TagEnum tagValue)
	{
		SetTagAndHandleChange((int)tag, Convert.ToInt32(tagValue));
	}

	public TagDelta SetTagAndHandleChange(int tag, int tagValue)
	{
		int prevTagValue = m_tags.GetTag(tag);
		SetTag(tag, tagValue);
		TagDelta change = new TagDelta();
		change.tag = tag;
		change.oldValue = prevTagValue;
		change.newValue = tagValue;
		OnTagChanged(change);
		return change;
	}

	public override int GetReferencedTag(int tag)
	{
		return GetEntityDef().GetReferencedTag(tag);
	}

	public int GetDefCost()
	{
		return GetEntityDef().GetCost();
	}

	public int GetDefATK()
	{
		return GetEntityDef().GetATK();
	}

	public int GetDefHealth()
	{
		return GetEntityDef().GetHealth();
	}

	public int GetDefDurability()
	{
		return GetEntityDef().GetDurability();
	}

	public bool HasRace(TAG_RACE race)
	{
		if ((HasTag(GAME_TAG.CARDRACE) ? GetTag<TAG_RACE>(GAME_TAG.CARDRACE) : GetEntityDef().GetTag<TAG_RACE>(GAME_TAG.CARDRACE)) == TAG_RACE.ALL && race != 0)
		{
			return true;
		}
		return GetRaces().Contains(race);
	}

	public override TAG_CLASS GetClass()
	{
		if (IsSecret())
		{
			return base.GetClass();
		}
		return GetEntityDef().GetClass();
	}

	public override void GetClasses(IList<TAG_CLASS> classes)
	{
		base.GetClasses(classes);
		if (classes.Count == 0)
		{
			GetEntityDef().GetClasses(classes);
		}
		if (classes.Count == 0)
		{
			classes.Add(TAG_CLASS.INVALID);
		}
	}

	public TAG_ENCHANTMENT_VISUAL GetEnchantmentBirthVisual()
	{
		return GetEntityDef().GetEnchantmentBirthVisual();
	}

	public TAG_ENCHANTMENT_VISUAL GetEnchantmentIdleVisual()
	{
		return GetEntityDef().GetEnchantmentIdleVisual();
	}

	public string GetCustomEnchantmentBannerText()
	{
		return GameDbf.GetIndex().GetClientString(GetTag(GAME_TAG.ENCHANTMENT_BANNER_TEXT));
	}

	public string GetCustomSideQuestBannerText()
	{
		return GameStrings.Format("GLUE_SIDEQUEST_PROGRESS_BANNER", GetTag(GAME_TAG.QUEST_PROGRESS), GetTag(GAME_TAG.QUEST_PROGRESS_TOTAL));
	}

	public string GetCustomObjectiveBannerText()
	{
		string cardId = GetCardId();
		int questProgressTotal = GetTag(GAME_TAG.QUEST_PROGRESS_TOTAL);
		if (cardId == "ETC_387t")
		{
			switch (GetTag(GAME_TAG.HIDDEN_CHOICE))
			{
			case 1:
				questProgressTotal = 2;
				break;
			case 2:
				questProgressTotal = 4;
				break;
			}
		}
		else if (cardId == "ETC_335")
		{
			if (GetTag(GAME_TAG.QUEST_PROGRESS) > 0)
			{
				return GameStrings.Format("GLUE_OBJECTIVES_BANNER_SPELL_PLAYED");
			}
			return GameStrings.Format("GLUE_OBJECTIVES_BANNER_NO_SPELL_PLAYED");
		}
		if (HasTag(GAME_TAG.QUEST_HIDE_PROGRESS))
		{
			return "";
		}
		string customBannerTextString = "";
		if (questProgressTotal > 0)
		{
			int turnsLeft = questProgressTotal - GetTag(GAME_TAG.QUEST_PROGRESS);
			return (GetEntityDef() != null && GetEntityDef().HasTag(GAME_TAG.BACON_SHOW_REFRESH_LEFT_BANNER)) ? GameStrings.Format("GLUE_OBJECTIVES_BANNER_REFRESH", turnsLeft) : GameStrings.Format("GLUE_OBJECTIVES_BANNER", turnsLeft);
		}
		return GameStrings.Format("GLUE_OBJECTIVES_BANNER_UNKNOWN");
	}

	public EnchantmentBanner.Icon GetCustomObjectiveBannerIcon()
	{
		if (GetCardId() == "ETC_335")
		{
			if (GetTag(GAME_TAG.QUEST_PROGRESS) > 0)
			{
				return EnchantmentBanner.Icon.Checkmark;
			}
			return EnchantmentBanner.Icon.Xmark;
		}
		return EnchantmentBanner.Icon.Nothing;
	}

	public void ApplyCustomBannerTextOffset(UberText enchantmentBannerText)
	{
		if (!(enchantmentBannerText == null) && GetCardId() == "ETC_335")
		{
			enchantmentBannerText.transform.localScale *= 0.8f;
			if (UniversalInputManager.Get().IsTouchMode())
			{
				enchantmentBannerText.transform.localPosition += new Vector3(0.14f, 0.084f, 0f);
			}
			else if (IsControlledByFriendlySidePlayer())
			{
				enchantmentBannerText.transform.position += new Vector3(0.14f, 0.084f, 0.05f);
			}
			else
			{
				enchantmentBannerText.transform.position += new Vector3(0.14f, 0.084f, 0.08f);
			}
		}
	}

	public TAG_RARITY GetRarity()
	{
		return GetEntityDef().GetRarity();
	}

	public new TAG_CARD_SET GetCardSet()
	{
		return GetEntityDef().GetCardSet();
	}

	public TAG_PREMIUM GetPremiumType()
	{
		TAG_PREMIUM premium = (TAG_PREMIUM)GetTag(GAME_TAG.PREMIUM);
		if (premium == TAG_PREMIUM.DIAMOND && !HasTag(GAME_TAG.HAS_DIAMOND_QUALITY))
		{
			premium = TAG_PREMIUM.SIGNATURE;
		}
		if (premium == TAG_PREMIUM.SIGNATURE && !HasTag(GAME_TAG.HAS_SIGNATURE_QUALITY))
		{
			premium = TAG_PREMIUM.GOLDEN;
		}
		return premium;
	}

	public bool CanBeDamagedRealTime()
	{
		if (GetRealTimeDivineShield() || GetRealTimeIsImmune())
		{
			return false;
		}
		if (GetRealTimeIsImmuneWhileAttacking() && (bool)TargetReticleManager.Get() && TargetReticleManager.Get().ArrowSourceEntityID == GetEntityId())
		{
			return false;
		}
		return true;
	}

	public int GetCurrentHealth()
	{
		return GetTag(GAME_TAG.HEALTH) - GetTag(GAME_TAG.DAMAGE) - GetTag(GAME_TAG.PREDAMAGE);
	}

	public int GetCurrentDurability()
	{
		return GetTag(GAME_TAG.DURABILITY) - GetTag(GAME_TAG.DAMAGE) - GetTag(GAME_TAG.PREDAMAGE);
	}

	public int GetCurrentDefense()
	{
		return GetCurrentHealth() + GetArmor();
	}

	public int GetCurrentVitality()
	{
		if (IsCharacter())
		{
			return GetCurrentDefense();
		}
		if (IsWeapon())
		{
			return GetCurrentDurability();
		}
		Error.AddDevFatal("Entity.GetCurrentVitality() should not be called on {0}. This entity is neither a character nor a weapon.", this);
		return 0;
	}

	public virtual Player GetController()
	{
		return GameState.Get()?.GetPlayer(GetControllerId());
	}

	public Player.Side GetControllerSide()
	{
		return GetController()?.GetSide() ?? Player.Side.NEUTRAL;
	}

	public bool IsControlledByLocalUser()
	{
		Player controller = GetController();
		if (controller != null)
		{
			return controller.IsLocalUser();
		}
		Log.Gameplay.PrintError("Failed to find controller for entity: {0}", ToString());
		return false;
	}

	public bool IsControlledByFriendlySidePlayer()
	{
		Player controller = GetController();
		if (controller != null)
		{
			return controller.IsFriendlySide();
		}
		Log.Gameplay.PrintError("Failed to find controller for entity: {0}", ToString());
		return false;
	}

	public bool IsControlledByOpposingSidePlayer()
	{
		Player controller = GetController();
		if (controller != null)
		{
			return controller.IsOpposingSide();
		}
		Log.Gameplay.PrintError("Failed to find controller for entity: {0}", ToString());
		return false;
	}

	public bool IsControlledByRevealedPlayer()
	{
		Player controller = GetController();
		if (controller != null)
		{
			return controller.IsRevealed();
		}
		Log.Gameplay.PrintError("Failed to find controller for entity: {0}", ToString());
		return false;
	}

	public bool IsControlledByConcealedPlayer()
	{
		return !IsControlledByRevealedPlayer();
	}

	public Entity GetCreator()
	{
		return GameState.Get().GetEntity(GetCreatorId());
	}

	public EntityDef GetCreatorDef()
	{
		return DefLoader.Get().GetEntityDef(GetCreatorDBID());
	}

	public string GetDisplayedCreatorName()
	{
		return m_displayedCreatorName;
	}

	public virtual Entity GetHero()
	{
		return GetController()?.GetHero();
	}

	public virtual Card GetHeroCard()
	{
		return GetController()?.GetHeroCard();
	}

	public virtual Entity GetHeroPower()
	{
		if (IsHeroPower())
		{
			return this;
		}
		return GetController()?.GetHeroPower();
	}

	public virtual Card GetHeroPowerCard()
	{
		if (IsHeroPower())
		{
			return GetCard();
		}
		return GetController()?.GetHeroPowerCard();
	}

	public virtual Card GetWeaponCard()
	{
		if (IsWeapon())
		{
			return GetCard();
		}
		return GetController()?.GetWeaponCard();
	}

	public virtual Card GetHeroBuddyCard()
	{
		if (IsBattlegroundHeroBuddy())
		{
			return GetCard();
		}
		return GetController()?.GetHeroBuddyCard();
	}

	public virtual Card GetQuestRewardFromHeroPowerCard()
	{
		if (IsBattlegroundQuestReward())
		{
			return GetCard();
		}
		return GetController()?.GetQuestRewardFromHeroPowerCard();
	}

	public virtual Card GetQuestRewardCard()
	{
		if (IsBattlegroundQuestReward())
		{
			return GetCard();
		}
		return GetController()?.GetQuestRewardCard();
	}

	public virtual List<Card> GetQuestRewardCards()
	{
		if (IsBattlegroundQuestReward())
		{
			return new List<Card> { GetCard() };
		}
		Player controller = GetController();
		if (controller == null)
		{
			return new List<Card>();
		}
		return controller.GetQuestRewardCards();
	}

	public virtual int GetHeroBuddyCardId()
	{
		if (HasTag(GAME_TAG.BACON_SKIN) && HasTag(GAME_TAG.BACON_SKIN_PARENT_ID))
		{
			int baseCardId = GetTag(GAME_TAG.BACON_SKIN_PARENT_ID);
			using DefLoader.DisposableFullDef disposableDef = DefLoader.Get().GetFullDef(baseCardId);
			if (disposableDef?.EntityDef == null || disposableDef?.CardDef == null)
			{
				Log.Gameplay.PrintError("GetHeroBuddyId(): Unable to load def for card ID {0}.", baseCardId);
				return 0;
			}
			return disposableDef.EntityDef.GetTag(GAME_TAG.BACON_COMPANION_ID);
		}
		return GetTag(GAME_TAG.BACON_COMPANION_ID);
	}

	public virtual bool HasValidDisplayName()
	{
		return GetEntityDef().HasValidDisplayName();
	}

	public virtual string GetName()
	{
		int overrideNameValue = GetTag(GAME_TAG.OVERRIDECARDNAME);
		EntityDef entityDef;
		if (overrideNameValue > 0)
		{
			entityDef = DefLoader.Get().GetEntityDef(overrideNameValue);
			if (entityDef != null)
			{
				return entityDef.GetName();
			}
		}
		entityDef = GetEntityDef();
		if (entityDef != null && entityDef.GetCardTextBuilder() != null)
		{
			return entityDef.GetCardTextBuilder().BuildCardName(this);
		}
		if (!string.IsNullOrEmpty(base.m_cardId))
		{
			Debug.LogWarning($"Entity.GetName: No textbuilder found for {base.m_cardId}, returning default name");
		}
		return CardTextBuilder.GetDefaultCardName(GetEntityDef());
	}

	public virtual string GetDebugName()
	{
		if (m_cachedDebugName.Name == null)
		{
			m_cachedDebugName.Dirty = true;
		}
		if (m_cachedDebugName.Dirty)
		{
			string entityName = GetEntityDef().GetName();
			if (entityName != null)
			{
				m_cachedDebugName.Name = $"[entityName={entityName} id={GetEntityId()} zone={GetZone()} zonePos={GetZonePosition()} cardId={base.m_cardId} player={GetControllerId()}]";
			}
			else if (base.m_cardId != null)
			{
				m_cachedDebugName.Name = $"[id={GetEntityId()} cardId={base.m_cardId} type={GetCardType()} zone={GetZone()} zonePos={GetZonePosition()} player={GetControllerId()}]";
			}
			else
			{
				m_cachedDebugName.Name = $"UNKNOWN ENTITY [id={GetEntityId()} type={GetCardType()} zone={GetZone()} zonePos={GetZonePosition()}]";
			}
			m_cachedDebugName.Dirty = false;
		}
		return m_cachedDebugName.Name;
	}

	public void UpdateCardName()
	{
		m_cachedDebugName.Dirty = true;
		if (m_card == null)
		{
			return;
		}
		string entityName = GetEntityDef().GetName();
		if (entityName != null)
		{
			if (string.IsNullOrEmpty(base.m_cardId))
			{
				m_card.gameObject.name = $"{entityName} [id={GetEntityId()} zone={GetZone()} zonePos={GetZonePosition()}]";
			}
			else
			{
				m_card.gameObject.name = $"{entityName} [id={GetEntityId()} cardId={GetCardId()} zone={GetZone()} zonePos={GetZonePosition()} player={GetControllerId()}]";
			}
		}
		else
		{
			m_card.gameObject.name = $"Hidden Entity [id={GetEntityId()} zone={GetZone()} zonePos={GetZonePosition()}]";
		}
		if (m_card.GetActor() != null)
		{
			m_card.GetActor().UpdateNameText();
		}
	}

	public string GetCardTextInHand()
	{
		using DefLoader.DisposableCardDef disposableDef = ShareDisposableCardDef();
		if (disposableDef?.CardDef == null)
		{
			Log.All.PrintError("Entity.GetCardTextInHand(): entity {0} does not have a CardDef", GetEntityId());
			return string.Empty;
		}
		return GetCardTextBuilder().BuildCardTextInHand(this);
	}

	public string GetCardTextInHistory()
	{
		using DefLoader.DisposableCardDef disposableDef = ShareDisposableCardDef();
		if (disposableDef?.CardDef == null)
		{
			Log.All.PrintError("Entity.GetCardTextInHand(): entity {0} does not have a CardDef", GetEntityId());
			return string.Empty;
		}
		return GetCardTextBuilder().BuildCardTextInHistory(this);
	}

	public string GetTargetingArrowText()
	{
		using DefLoader.DisposableCardDef disposableDef = ShareDisposableCardDef();
		if (disposableDef?.CardDef == null)
		{
			Log.All.PrintError("Entity.GetTargetingArrowText(): entity {0} does not have a CardDef", GetEntityId());
			return string.Empty;
		}
		return GetCardTextBuilder().GetTargetingArrowText(this);
	}

	public void AddAttachment(Entity entity)
	{
		int oldCount = m_attachments.Count;
		if (m_attachments.Contains(entity))
		{
			Log.Gameplay.Print($"Entity.AddAttachment() - {entity} is already an attachment of {this}");
			return;
		}
		m_attachments.Add(entity);
		if (!(m_card == null))
		{
			m_card.OnEnchantmentAdded(oldCount, entity);
		}
	}

	public void RemoveAttachment(Entity entity)
	{
		int oldCount = m_attachments.Count;
		if (!m_attachments.Remove(entity))
		{
			Log.Gameplay.Print("Entity.RemoveAttachment() - {0} is not an attachment of {1}", entity, this);
		}
		else if (!(m_card == null))
		{
			m_card.OnEnchantmentRemoved(oldCount, entity);
		}
	}

	public bool AddSubCard(Entity entity)
	{
		if (m_subCardIDs.Contains(entity.GetEntityId()))
		{
			return false;
		}
		m_subCardIDs.Add(entity.GetEntityId());
		return true;
	}

	private void RemoveSubCard(Entity entity)
	{
		m_subCardIDs.Remove(entity.GetEntityId());
	}

	private void RemoveLettuceAbilityEntityID(int entityID)
	{
		m_lettuceAbilityEntityIDs.Remove(entityID);
	}

	private void AddLettuceAbilityEntityID(int entityID)
	{
		if (!m_lettuceAbilityEntityIDs.Contains(entityID))
		{
			m_lettuceAbilityEntityIDs.Add(entityID);
		}
	}

	public List<int> GetLettuceAbilityEntityIDs()
	{
		return m_lettuceAbilityEntityIDs;
	}

	public int GetSelectedLettuceAbilityID()
	{
		int selected = GetTag(GAME_TAG.LETTUCE_ABILITY_TILE_VISUAL_SELF_ONLY);
		if (selected > 0)
		{
			return selected;
		}
		return GetTag(GAME_TAG.LETTUCE_ABILITY_TILE_VISUAL_ALL_VISIBLE);
	}

	public List<Entity> GetAttachments()
	{
		return m_attachments;
	}

	public bool DoEnchantmentsHaveTag(GAME_TAG tag)
	{
		foreach (Entity attachment in m_attachments)
		{
			if (attachment.HasTag(tag))
			{
				return true;
			}
		}
		return false;
	}

	public bool DoEnchantmentsHaveTriggerVisuals()
	{
		foreach (Entity attachment in m_attachments)
		{
			if (attachment.HasTriggerVisual())
			{
				return true;
			}
		}
		return false;
	}

	public bool DoEnchantmentsHaveOverKill()
	{
		foreach (Entity attachment in m_attachments)
		{
			if (attachment.HasTag(GAME_TAG.OVERKILL))
			{
				return true;
			}
		}
		return false;
	}

	public bool DoEnchantmentsHaveSpellburst()
	{
		foreach (Entity attachment in m_attachments)
		{
			if (attachment.HasSpellburst())
			{
				return true;
			}
		}
		return false;
	}

	public bool DoEnchantmentsHaveCounter()
	{
		foreach (Entity attachment in m_attachments)
		{
			if (attachment.HasCounter())
			{
				return true;
			}
		}
		return false;
	}

	public bool DoEnchantmentsHaveHonorableKill()
	{
		foreach (Entity attachment in m_attachments)
		{
			if (attachment.HasTag(GAME_TAG.HONORABLE_KILL))
			{
				return true;
			}
		}
		return false;
	}

	public bool IsEnchanted()
	{
		return m_attachments.Count > 0;
	}

	public bool IsEnchantment()
	{
		return GetRealTimeCardType() == TAG_CARDTYPE.ENCHANTMENT;
	}

	public bool IsDarkWandererSecret()
	{
		if (IsSecret())
		{
			return GetClass() == TAG_CLASS.WARRIOR;
		}
		return false;
	}

	public bool IsDeathrattleDisabled()
	{
		return HasTag(GAME_TAG.CANT_TRIGGER_DEATHRATTLE);
	}

	public bool IsToBeDestroyed()
	{
		return HasTag(GAME_TAG.TO_BE_DESTROYED);
	}

	public List<Entity> GetEnchantments()
	{
		return GetAttachments();
	}

	public List<Entity> GetDisplayedEnchantments(bool unique = false)
	{
		List<Entity> displayedEnchantments = new List<Entity>(GetAttachments());
		displayedEnchantments.RemoveAll((Entity enchant) => enchant.HasTag(GAME_TAG.ENCHANTMENT_INVISIBLE));
		if (!unique)
		{
			return displayedEnchantments;
		}
		return displayedEnchantments.Distinct(new EnchantmentComparer()).ToList();
	}

	public bool HasSubCards()
	{
		if (m_subCardIDs != null)
		{
			return m_subCardIDs.Count > 0;
		}
		return false;
	}

	public List<int> GetSubCardIDs()
	{
		return m_subCardIDs;
	}

	public Entity GetSubCard(int suboption)
	{
		if (m_subCardIDs != null && suboption < m_subCardIDs.Count)
		{
			return GameState.Get()?.GetEntity(m_subCardIDs[suboption]);
		}
		return null;
	}

	public int GetSubCardIndex(Entity entity)
	{
		if (entity == null)
		{
			return -1;
		}
		int entityId = entity.GetEntityId();
		for (int i = 0; i < m_subCardIDs.Count; i++)
		{
			if (m_subCardIDs[i] == entityId)
			{
				return i;
			}
		}
		return -1;
	}

	public Entity GetParentEntity()
	{
		int parentId = GetTag(GAME_TAG.PARENT_CARD);
		return GameState.Get()?.GetEntity(parentId);
	}

	public CardTextBuilder GetCardTextBuilder()
	{
		if (GetEntityDef() != null && GetEntityDef().GetCardTextBuilder() != null)
		{
			return GetEntityDef().GetCardTextBuilder();
		}
		if (!string.IsNullOrEmpty(base.m_cardId))
		{
			Debug.LogWarning($"Entity.GetCardTextBuilder: No textbuilder found for {base.m_cardId}, returning fallback text builder");
		}
		return CardTextBuilder.GetFallbackCardTextBuilder();
	}

	public Entity CloneForZoneMgr()
	{
		Entity entity = new Entity();
		entity.m_staticEntityDef = GetEntityDef();
		entity.m_dynamicEntityDef = null;
		entity.m_card = m_card;
		entity.m_cardId = base.m_cardId;
		entity.ReplaceTags(m_tags);
		entity.m_loadState = m_loadState;
		entity.m_cachedDebugName.Dirty = true;
		return entity;
	}

	public Entity CloneForHistory(HistoryInfo historyInfo)
	{
		Entity clone = new Entity();
		clone.m_duplicateForHistory = true;
		clone.m_staticEntityDef = GetEntityDef();
		clone.m_dynamicEntityDef = null;
		clone.m_card = m_card;
		clone.m_cardId = base.m_cardId;
		clone.ReplaceTags(m_tags);
		clone.m_cardTextHistoryData = GetCardTextBuilder().CreateCardTextHistoryData();
		clone.m_cardTextHistoryData.SetHistoryData(this, historyInfo);
		clone.m_subCardIDs = m_subCardIDs;
		if (!IsHero())
		{
			clone.SetTag(GAME_TAG.ZONE, TAG_ZONE.HAND);
		}
		clone.m_loadState = m_loadState;
		clone.m_displayedCreatorName = m_displayedCreatorName;
		clone.m_enchantmentCreatorCardIDForPortrait = m_enchantmentCreatorCardIDForPortrait;
		return clone;
	}

	public bool IsHistoryDupe()
	{
		return m_duplicateForHistory;
	}

	public int GetJadeGolem()
	{
		Player controller = GetController();
		if (controller != null)
		{
			return Mathf.Min(controller.GetTag(GAME_TAG.JADE_GOLEM) + 1, 30);
		}
		Log.Gameplay.PrintError("Failed to find controller for entity: {0}", ToString());
		return 1;
	}

	private bool IsEnchantmentAffectedBySpellPower()
	{
		if (IsEnchantment())
		{
			return IsAffectedBySpellPower();
		}
		return false;
	}

	private Player GetControllerForDamageOrHealingBonus()
	{
		if (HasTag(GAME_TAG.SOURCE_OVERRIDE_FOR_MODIFIER_TEXT))
		{
			return GameState.Get().GetEntity(GetTag(GAME_TAG.SOURCE_OVERRIDE_FOR_MODIFIER_TEXT)).GetController();
		}
		if (IsLettuceAbility())
		{
			Entity owner = GetLettuceAbilityOwner();
			if (owner != null)
			{
				return owner.GetController();
			}
		}
		Entity parentEntity = GetParentEntity();
		if (parentEntity != null && parentEntity.IsLettuceAbility())
		{
			Entity parent = GetParentEntity();
			if (parent != null)
			{
				Entity owner2 = parent.GetLettuceAbilityOwner();
				if (owner2 != null)
				{
					return owner2.GetController();
				}
			}
		}
		return GetController();
	}

	public int GetDamageBonus()
	{
		Player controller = GetControllerForDamageOrHealingBonus();
		if (controller == null)
		{
			return 0;
		}
		if (IsSpell() || IsMinion() || IsLettuceAbilitySpellCasting() || IsEnchantmentAffectedBySpellPower())
		{
			int spellDamage = controller.TotalSpellpower(this, GetSpellSchool());
			if (HasTag(GAME_TAG.RECEIVES_DOUBLE_SPELLDAMAGE_BONUS))
			{
				spellDamage *= 2;
			}
			return spellDamage;
		}
		if (IsHeroPower())
		{
			int bonus = controller.GetTag(GAME_TAG.CURRENT_HEROPOWER_DAMAGE_BONUS);
			if (GetCardTextBuilder() is SpellDamageOnlyCardTextBuilder)
			{
				int spellDamage2 = controller.TotalSpellpower(this, GetSpellSchool());
				if (HasTag(GAME_TAG.RECEIVES_DOUBLE_SPELLDAMAGE_BONUS))
				{
					spellDamage2 *= 2;
				}
				bonus += spellDamage2;
			}
			return bonus;
		}
		return 0;
	}

	public int GetDamageBonusDouble()
	{
		Player controller = GetControllerForDamageOrHealingBonus();
		if (controller == null)
		{
			return 0;
		}
		if (IsSpell() || IsLettuceAbilitySpellCasting() || IsEnchantmentAffectedBySpellPower())
		{
			return controller.GetTag(GAME_TAG.SPELLPOWER_DOUBLE);
		}
		if (IsHeroPower())
		{
			return controller.GetTag(GAME_TAG.HERO_POWER_DOUBLE);
		}
		return 0;
	}

	public int GetHealingBonus()
	{
		Player controller = GetControllerForDamageOrHealingBonus();
		if (controller == null)
		{
			return 0;
		}
		if (IsSpell() || IsLettuceAbilitySpellCasting() || IsEnchantmentAffectedBySpellPower())
		{
			return controller.GetTag(GAME_TAG.CURRENT_HEALING_POWER);
		}
		return 0;
	}

	public int GetHealingDouble()
	{
		Player controller = GetControllerForDamageOrHealingBonus();
		if (controller == null)
		{
			return 0;
		}
		int allHealingDouble = controller.GetTag(GAME_TAG.ALL_HEALING_DOUBLE);
		if (IsSpell() || IsLettuceAbilitySpellCasting() || IsEnchantmentAffectedBySpellPower())
		{
			return controller.GetTag(GAME_TAG.SPELL_HEALING_DOUBLE) + allHealingDouble;
		}
		if (IsHeroPower())
		{
			return controller.GetTag(GAME_TAG.HERO_POWER_DOUBLE) + allHealingDouble;
		}
		return allHealingDouble;
	}

	public int GetAttackBonus()
	{
		int entityBonus = GetTag(GAME_TAG.HERO_ATTACK_GIVEN_ADDITIONAL);
		Player controller = GetControllerForDamageOrHealingBonus();
		if (controller == null)
		{
			return entityBonus;
		}
		int playerBonus = controller.GetTag(GAME_TAG.HERO_ATTACK_GIVEN_ADDITIONAL);
		return entityBonus + playerBonus;
	}

	public int GetArmorBonus()
	{
		int entityBonus = GetTag(GAME_TAG.HERO_ARMOR_GIVEN_ADDITIONAL);
		Player controller = GetControllerForDamageOrHealingBonus();
		if (controller == null)
		{
			return entityBonus;
		}
		int playerBonus = controller.GetTag(GAME_TAG.HERO_ARMOR_GIVEN_ADDITIONAL);
		return entityBonus + playerBonus;
	}

	public void ClearBattlecryFlag()
	{
		m_useBattlecryPower = false;
	}

	public void UpdateUseBattlecryFlag(bool fromGameState)
	{
		if (IsMinion() && !IsCutsceneEntity())
		{
			bool entityHasTargets = fromGameState || GameState.Get().EntityHasTargets(this);
			if (TAG_ZONE.HAND == GetZone() && entityHasTargets)
			{
				m_useBattlecryPower = true;
			}
		}
	}

	public virtual void InitRealTimeValues(List<Network.Entity.Tag> tags)
	{
		foreach (Network.Entity.Tag tag in tags)
		{
			switch ((GAME_TAG)tag.Name)
			{
			case GAME_TAG.COST:
				SetRealTimeCost(tag.Value);
				break;
			case GAME_TAG.ATK:
				SetRealTimeAttack(tag.Value);
				break;
			case GAME_TAG.HEALTH:
			case GAME_TAG.DURABILITY:
				SetRealTimeHealth(tag.Value);
				break;
			case GAME_TAG.DAMAGE:
				SetRealTimeDamage(tag.Value);
				break;
			case GAME_TAG.ARMOR:
				SetRealTimeArmor(tag.Value);
				break;
			case GAME_TAG.ZONE:
				SetRealTimeZone(tag.Value);
				break;
			case GAME_TAG.ZONE_POSITION:
				SetRealTimeZonePosition(tag.Value);
				break;
			case GAME_TAG.POWERED_UP:
				SetRealTimePoweredUp(tag.Value);
				break;
			case GAME_TAG.LINKED_ENTITY:
				SetRealTimeLinkedEntityId(tag.Value);
				break;
			case GAME_TAG.PARENT_CARD:
				SetRealTimeParentEntityId(tag.Value);
				break;
			case GAME_TAG.DIVINE_SHIELD:
				SetRealTimeDivineShield(tag.Value);
				break;
			case GAME_TAG.IMMUNE:
				SetRealTimeIsImmune(tag.Value);
				break;
			case GAME_TAG.IMMUNE_WHILE_ATTACKING:
				SetRealTimeIsImmuneWhileAttacking(tag.Value);
				break;
			case GAME_TAG.POISONOUS:
			case GAME_TAG.NON_KEYWORD_POISONOUS:
				SetRealTimeIsPoisonous(tag.Value);
				break;
			case GAME_TAG.VENOMOUS:
				SetRealTimeIsVenomous(tag.Value);
				break;
			case GAME_TAG.CARDTYPE:
				SetRealTimeCardType((TAG_CARDTYPE)tag.Value);
				break;
			case GAME_TAG.CARD_ALTERNATE_COST:
			{
				TAG_CARD_ALTERNATE_COST cost = (TAG_CARD_ALTERNATE_COST)tag.Value;
				SetRealTimeCardCostsHealth(cost == TAG_CARD_ALTERNATE_COST.HEALTH);
				SetRealTimeCardCostsArmor(cost == TAG_CARD_ALTERNATE_COST.ARMOR);
				SetRealTimeCardCostsCorpses(cost == TAG_CARD_ALTERNATE_COST.CORPSES);
				break;
			}
			case GAME_TAG.ATTACKABLE_BY_RUSH:
				SetRealTimeAttackableByRush(tag.Value);
				break;
			case GAME_TAG.PREMIUM:
				SetRealTimePremium((TAG_PREMIUM)tag.Value);
				break;
			case GAME_TAG.PLAYER_LEADERBOARD_PLACE:
				SetRealTimePlayerLeaderboardPlace(tag.Value);
				break;
			case GAME_TAG.BACON_DUO_PASSABLE:
				SetRealTimeBaconDuoPassable(tag.Value);
				break;
			case GAME_TAG.PLAYER_TECH_LEVEL:
				SetRealTimePlayerTechLevel(tag.Value);
				break;
			case GAME_TAG.BACON_DUO_PLAYER_FIGHTS_FIRST_NEXT_COMBAT:
				SetRealTimePlayerFightsFirst(tag.Value);
				break;
			case GAME_TAG.TITAN:
				SetRealTimeIsTitan(tag.Value);
				break;
			case GAME_TAG.TITAN_ABILITY_USED_1:
				SetRealTimeTitanAbilityUsed1(tag.Value);
				break;
			case GAME_TAG.TITAN_ABILITY_USED_2:
				SetRealTimeTitanAbilityUsed2(tag.Value);
				break;
			case GAME_TAG.TITAN_ABILITY_USED_3:
				SetRealTimeTitanAbilityUsed3(tag.Value);
				break;
			case GAME_TAG.DECK_ACTION_COST:
				SetRealTimeDeckActionCost(tag.Value);
				break;
			}
		}
	}

	public void SetRealTimeCost(int newCost)
	{
		m_realTimeCost = newCost;
	}

	public int GetRealTimeCost()
	{
		if (m_realTimeCost == -1)
		{
			return GetCost();
		}
		return m_realTimeCost;
	}

	public void SetRealTimeAttack(int newAttack)
	{
		m_realTimeAttack = newAttack;
	}

	public int GetRealTimeAttack()
	{
		return m_realTimeAttack;
	}

	public void SetRealTimeBaconCombatPhaseHero(int value)
	{
		m_realTimeBaconCombatPhaseHero = value > 0;
	}

	public bool GetRealTimeBaconCombatPhaseHero()
	{
		return m_realTimeBaconCombatPhaseHero;
	}

	public void SetRealtimeBaconDamageCapEnabled(int value)
	{
		m_realTimeBaconDamageCapEnabled = value > 0;
	}

	public bool GetRealtimeBaconDamageCapEnabled()
	{
		return m_realTimeBaconDamageCapEnabled;
	}

	public void SetRealTimeDeckActionCost(int cost)
	{
		m_realTimeDeckActionCost = cost;
	}

	public int GetRealTimeDeckActionCost()
	{
		return m_realTimeDeckActionCost;
	}

	public void SetRealTimeHealth(int newHealth)
	{
		m_realTimeHealth = newHealth;
	}

	public void SetRealTimeDamage(int newDamage)
	{
		m_realTimeDamage = newDamage;
	}

	public void SetRealTimeArmor(int newArmor)
	{
		m_realTimeArmor = newArmor;
	}

	public int GetRealTimeRemainingHP()
	{
		return m_realTimeHealth + m_realTimeArmor - m_realTimeDamage;
	}

	public int GetRealTimeArmor()
	{
		return m_realTimeArmor;
	}

	public void SetRealTimeZone(int zone)
	{
		m_realTimeZone = zone;
	}

	public TAG_ZONE GetRealTimeZone()
	{
		return (TAG_ZONE)m_realTimeZone;
	}

	public void SetRealTimeZonePosition(int zonePosition)
	{
		m_realTimeZonePosition = zonePosition;
	}

	public int GetRealTimeZonePosition()
	{
		return m_realTimeZonePosition;
	}

	public void SetRealTimeLinkedEntityId(int linkedEntityId)
	{
		m_realTimeLinkedEntityId = linkedEntityId;
	}

	public int GetRealTimeLinkedEntityId()
	{
		return m_realTimeLinkedEntityId;
	}

	public void SetRealTimeParentEntityId(int parentEntityId)
	{
		m_realTimeParentEntityId = parentEntityId;
	}

	public int GetRealTimeParentEntityId()
	{
		return m_realTimeParentEntityId;
	}

	public void SetRealTimePoweredUp(int poweredUp)
	{
		m_realTimePoweredUp = poweredUp > 0;
	}

	public bool GetRealTimePoweredUp()
	{
		return m_realTimePoweredUp;
	}

	public void SetRealTimeDivineShield(int divineShield)
	{
		m_realTimeDivineShield = divineShield > 0;
	}

	public bool GetRealTimeDivineShield()
	{
		return m_realTimeDivineShield;
	}

	public void SetRealTimeIsImmune(int immune)
	{
		m_realTimeIsImmune = immune > 0;
	}

	public bool GetRealTimeIsImmune()
	{
		return m_realTimeIsImmune;
	}

	public void SetRealTimeIsImmuneWhileAttacking(int immune)
	{
		m_realTimeIsImmuneWhileAttacking = immune > 0;
	}

	public bool GetRealTimeIsImmuneWhileAttacking()
	{
		return m_realTimeIsImmuneWhileAttacking;
	}

	public void SetRealTimeIsPoisonous(int poisonous)
	{
		m_realTimeIsPoisonous = poisonous > 0;
	}

	public bool GetRealTimeIsPoisonous()
	{
		return m_realTimeIsPoisonous;
	}

	public void SetRealTimeIsVenomous(int venomous)
	{
		m_realTimeIsVenomous = venomous > 0;
	}

	public bool GetRealTimeIsVenomous()
	{
		return m_realTimeIsVenomous;
	}

	public void SetRealTimeIsDormant(int dormant)
	{
		m_realTimeIsDormant = dormant > 0;
	}

	public bool GetRealTimeIsDormant()
	{
		return m_realTimeIsDormant;
	}

	public void SetRealTimeHasSpellpower(int spellpower)
	{
		m_realTimeSpellpower = spellpower;
	}

	public int GetRealTimeSpellpower()
	{
		return m_realTimeSpellpower;
	}

	public void SetRealTimeSpellpowerDouble(int powerDouble)
	{
		m_realTimeSpellpowerDouble = powerDouble > 0;
	}

	public bool GetRealTimeSpellpowerDouble()
	{
		return m_realTimeSpellpowerDouble;
	}

	public void SetRealTimeHealingDoesDamageHint(int healingDoesDamageHint)
	{
		m_realTimeHealingDoesDamageHint = healingDoesDamageHint > 0;
	}

	public bool GetRealTimeHealingDoeDamageHint()
	{
		return m_realTimeHealingDoesDamageHint;
	}

	public void SetRealTimeLifestealDoesDamageHint(int lifestealDoesDamageHint)
	{
		m_realTimeLifestealDoesDamageHint = lifestealDoesDamageHint > 0;
	}

	public bool GetRealTimeLifestealDoesDamageHint()
	{
		return m_realTimeLifestealDoesDamageHint;
	}

	public void SetRealTimeCardCostsHealth(bool value)
	{
		m_realTimeCardCostsHealth = value;
	}

	public bool GetRealTimeCardCostsHealth()
	{
		return m_realTimeCardCostsHealth;
	}

	public void SetRealTimeCardCostsArmor(bool value)
	{
		m_realTimeCardCostsArmor = value;
	}

	public bool GetRealTimeCardCostsArmor()
	{
		return m_realTimeCardCostsArmor;
	}

	public void SetRealTimeCardCostsCorpses(bool value)
	{
		m_realTimeCardCostsCorpses = value;
	}

	public bool GetRealTimeCardCostsCorpses()
	{
		return m_realTimeCardCostsCorpses;
	}

	public void SetRealTimeAttackableByRush(int value)
	{
		m_realTimeAttackableByRush = value > 0;
	}

	public bool GetRealTimeAttackableByRush()
	{
		return m_realTimeAttackableByRush;
	}

	public void SetRealTimeCardType(TAG_CARDTYPE cardType)
	{
		m_realTimeCardType = cardType;
	}

	public TAG_CARDTYPE GetRealTimeCardType()
	{
		return m_realTimeCardType;
	}

	public void SetRealTimePremium(TAG_PREMIUM premium)
	{
		m_realTimePremium = premium;
	}

	public void SetRealTimePlayerLeaderboardPlace(int playerLeaderboardPlace)
	{
		m_realTimePlayerLeaderboardPlace = playerLeaderboardPlace;
	}

	public int GetRealTimePlayerLeaderboardPlace()
	{
		return m_realTimePlayerLeaderboardPlace;
	}

	public void SetRealTimePlayerFightsFirst(int value)
	{
		m_realTimePlayerFightsFirst = value;
	}

	public void SetRealTimePlayerTechLevel(int playerTechLevel)
	{
		m_realTimePlayerTechLevel = playerTechLevel;
	}

	public int GetRealTimePlayerFightsFirst()
	{
		return m_realTimePlayerFightsFirst;
	}

	public int GetRealTimePlayerTechLevel()
	{
		return m_realTimePlayerTechLevel;
	}

	public void SetRealTimeBaconDuoPassable(int baconDuoPassable)
	{
		m_realTimeBaconDuoPassable = baconDuoPassable;
	}

	public void SetRealTimeIsTitan(int isTitan)
	{
		m_realTimeIsTitan = isTitan > 0;
	}

	public void SetRealTimeTitanAbilityUsed1(int used)
	{
		m_realTimeTitanAbilityUsed1 = used > 0;
	}

	public void SetRealTimeTitanAbilityUsed2(int used)
	{
		m_realTimeTitanAbilityUsed2 = used > 0;
	}

	public void SetRealTimeTitanAbilityUsed3(int used)
	{
		m_realTimeTitanAbilityUsed3 = used > 0;
	}

	public bool GetRealTimeTitanAbilityUsable()
	{
		if (m_realTimeIsTitan)
		{
			if (m_realTimeTitanAbilityUsed1 && m_realTimeTitanAbilityUsed2)
			{
				return !m_realTimeTitanAbilityUsed3;
			}
			return true;
		}
		return false;
	}

	public CardTextHistoryData GetCardTextHistoryData()
	{
		return m_cardTextHistoryData;
	}

	private void LoadEntityDef(string cardId)
	{
		if (base.m_cardId != cardId)
		{
			base.m_cardId = cardId;
		}
		if (!string.IsNullOrEmpty(cardId))
		{
			m_dynamicEntityDef = null;
			m_staticEntityDef = DefLoader.Get().GetEntityDef(cardId);
			if (m_staticEntityDef == null)
			{
				Error.AddDevFatal("Failed to load a card xml for {0}", cardId);
			}
			else
			{
				UpdateCardName();
			}
		}
	}

	public void LoadCard(string cardId, LoadCardData data = null)
	{
		LoadEntityDef(cardId);
		m_loadState = LoadState.LOADING;
		if (string.IsNullOrEmpty(cardId))
		{
			DefLoader.Get().LoadCardDef("HiddenCard", OnCardDefLoaded);
			return;
		}
		CardPortraitQuality quality = CardPortraitQuality.GetDefault();
		quality.PremiumType = m_realTimePremium;
		DefLoader.Get().LoadCardDef(cardId, OnCardDefLoaded, data, quality);
	}

	private void OnCardDefLoaded(string cardId, DefLoader.DisposableCardDef cardDef, object callbackData)
	{
		using (cardDef)
		{
			if (cardDef == null)
			{
				Debug.LogErrorFormat("Entity.OnCardDefLoaded() - {0} does not have an asset!", cardId);
				m_loadState = LoadState.DONE;
				return;
			}
			LoadCardData data = new LoadCardData
			{
				updateActor = false,
				restartStateSpells = false,
				fromChangeEntity = false
			};
			if (callbackData is LoadCardData)
			{
				data = (LoadCardData)callbackData;
			}
			if (m_card != null)
			{
				m_card.SetCardDef(cardDef, data.updateActor);
				if (data.updateActor)
				{
					m_card.UpdateActor();
					m_card.ActivateStateSpells();
				}
				else if (data.restartStateSpells)
				{
					m_card.ActivateStateSpells(forceActivate: true);
				}
				m_card.RefreshCardsInTooltip();
				if (data.fromChangeEntity && IsMinion() && m_card.GetZone() is ZonePlay)
				{
					m_card.ActivateCharacterPlayEffects();
				}
			}
			UpdateUseBattlecryFlag(fromGameState: false);
			m_loadState = LoadState.DONE;
			if (m_card != null)
			{
				m_card.RefreshActor();
			}
		}
	}

	public SpellType GetTriggerSpellType()
	{
		GameState gameState = GameState.Get();
		GameMgr gameMgr = GameMgr.Get();
		if (gameMgr != null && gameMgr.IsBattlegrounds() && gameState != null)
		{
			if (HasTag(GAME_TAG.BACON_TRIGGER_XY_STAY))
			{
				return SpellType.TRIGGER_XY_STAY;
			}
			if (HasTag(GAME_TAG.BACON_TRIGGER_XY))
			{
				return SpellType.TRIGGER_XY;
			}
			if (HasTag(GAME_TAG.BACON_TRIGGER_UPBEAT))
			{
				return SpellType.TRIGGER_UPBEAT;
			}
			if (gameState.IsUsingFastActorTriggers() && !IsHeroPower())
			{
				return SpellType.FAST_TRIGGER;
			}
		}
		return SpellType.TRIGGER;
	}

	public SpellType GetPrioritizedBaubleSpellType()
	{
		if (IsPoisonous())
		{
			return SpellType.POISONOUS;
		}
		if (IsVenomous())
		{
			return SpellType.VENOMOUS;
		}
		bool wantsSpellburstBauble = HasSpellburst() || DoEnchantmentsHaveSpellburst() || DoEnchantmentsHaveCounter();
		bool wantsTriggerBauble = HasTriggerVisual() || DoEnchantmentsHaveTriggerVisuals() || HasOverheal() || HasTag(GAME_TAG.FORGETFUL);
		bool wantsLifestealBauble = HasLifesteal();
		if (wantsSpellburstBauble && wantsTriggerBauble)
		{
			return SpellType.TRIGGER_AND_SPELLBURST;
		}
		if (wantsLifestealBauble && wantsSpellburstBauble)
		{
			return SpellType.LIFESTEAL_AND_SPELLBURST;
		}
		if (HasTriggerVisual() || DoEnchantmentsHaveTriggerVisuals())
		{
			return GetTriggerSpellType();
		}
		if (HasLifesteal())
		{
			return SpellType.LIFESTEAL;
		}
		if (HasInspire())
		{
			return SpellType.INSPIRE;
		}
		if (HasOverKill() || DoEnchantmentsHaveOverKill())
		{
			return SpellType.OVERKILL;
		}
		if (HasSpellburst() || DoEnchantmentsHaveSpellburst())
		{
			return SpellType.SPELLBURST;
		}
		if (DoEnchantmentsHaveCounter())
		{
			return SpellType.SPELLBURST;
		}
		if (HasFrenzy())
		{
			return SpellType.FRENZY;
		}
		if (HasAvenge())
		{
			return SpellType.AVENGE;
		}
		if (HasHonorableKill() || DoEnchantmentsHaveHonorableKill())
		{
			return SpellType.HONORABLEKILL;
		}
		if (HasOverheal() || HasEndOfTurnTrigger())
		{
			return GetTriggerSpellType();
		}
		if (HasTag(GAME_TAG.FORGETFUL))
		{
			return GetTriggerSpellType();
		}
		return SpellType.NONE;
	}

	public TAG_CARD_SET GetWatermarkCardSetOverride()
	{
		if (HasTag(GAME_TAG.WATERMARK_OVERRIDE_CARD_SET))
		{
			return (TAG_CARD_SET)GetTag(GAME_TAG.WATERMARK_OVERRIDE_CARD_SET);
		}
		EntityDef entityDef = GetEntityDef();
		if (entityDef != null && entityDef.HasTag(GAME_TAG.WATERMARK_OVERRIDE_CARD_SET))
		{
			return (TAG_CARD_SET)entityDef.GetTag(GAME_TAG.WATERMARK_OVERRIDE_CARD_SET);
		}
		return TAG_CARD_SET.INVALID;
	}

	public bool IsTauntIgnored()
	{
		if (GameState.Get().GetFirstOpponentPlayer(GetController()).HasTag(GAME_TAG.IGNORE_TAUNT))
		{
			return true;
		}
		return false;
	}

	public Entity GetLettuceAbilityOwner()
	{
		return GameState.Get().GetEntity(GetTag(GAME_TAG.LETTUCE_ABILITY_OWNER));
	}

	public bool IsMyLettuceRoleStrongAgainst(Entity otherEntity)
	{
		if (otherEntity == null)
		{
			return false;
		}
		TAG_ROLE myRole = GetTag<TAG_ROLE>(GAME_TAG.LETTUCE_ROLE);
		TAG_ROLE theirRole = otherEntity.GetTag<TAG_ROLE>(GAME_TAG.LETTUCE_ROLE);
		if (myRole == TAG_ROLE.CASTER && theirRole == TAG_ROLE.TANK)
		{
			return true;
		}
		if (myRole == TAG_ROLE.TANK && theirRole == TAG_ROLE.FIGHTER)
		{
			return true;
		}
		if (myRole == TAG_ROLE.FIGHTER && theirRole == TAG_ROLE.CASTER)
		{
			return true;
		}
		return false;
	}

	public bool HasSelectedLettuceAbility()
	{
		if (!HasTag(GAME_TAG.LETTUCE_ABILITY_TILE_VISUAL_ALL_VISIBLE))
		{
			return HasTag(GAME_TAG.LETTUCE_ABILITY_TILE_VISUAL_SELF_ONLY);
		}
		return true;
	}

	public bool IsMercenary()
	{
		return HasTag(GAME_TAG.LETTUCE_MERCENARY);
	}

	public bool ShouldShowEquipmentTextOnMerc()
	{
		int equipmentCardId = GetTag(GAME_TAG.LETTUCE_EQUIPMENT_ID);
		if (equipmentCardId == 0)
		{
			return false;
		}
		return GameDbf.GetIndex().GetEquipmentTierFromCardID(equipmentCardId)?.ShowTextOnMerc ?? false;
	}

	public bool ShouldShowHeroRerollButton()
	{
		Entity gameEntity = GameState.Get().GetGameEntity();
		if (gameEntity == null)
		{
			return false;
		}
		bool num = !HasTag(GAME_TAG.BACON_LOCKED_MULLIGAN_HERO);
		bool lockedHeroShouldShow = HasTag(GAME_TAG.BACON_LOCKED_MULLIGAN_HERO) && gameEntity.HasTag(GAME_TAG.BACON_UNLOCK_MULLIGAN_HERO_ENABLED);
		return num || lockedHeroShouldShow;
	}

	public RerollButtonEnableResult ShouldEnableRerollButton(bool? hasFreeReroll = null, bool? hasPaidReroll = null)
	{
		MulliganManager manager = MulliganManager.Get();
		if (manager != null)
		{
			_ = !manager.IsMulliganTimerLeftExceedBaconRerollButtonCutoff();
		}
		else
			_ = 0;
		if (manager == null)
		{
			return RerollButtonEnableResult.MULLIGAN_NOT_ACTIVE;
		}
		if (manager.IsMulliganTimerLeftExceedBaconRerollButtonCutoff())
		{
			return RerollButtonEnableResult.INSUFFICIENT_MULLIGAN_TIME_LEFT;
		}
		if (BaconNumRerollLeft() <= 0)
		{
			return RerollButtonEnableResult.HERO_REROLL_LIMITATION_REACHED;
		}
		if ((hasFreeReroll.HasValue && hasFreeReroll.Value) || BaconFreeRerollLeft() > 0)
		{
			if (!HasTag(GAME_TAG.BACON_LOCKED_MULLIGAN_HERO))
			{
				return RerollButtonEnableResult.FREE;
			}
			return RerollButtonEnableResult.FREE_UNLOCK;
		}
		if ((hasPaidReroll.HasValue && hasPaidReroll.Value) || NetCache.Get().GetBattlegroundsTokenBalance() > 0)
		{
			if (!HasTag(GAME_TAG.BACON_LOCKED_MULLIGAN_HERO))
			{
				return RerollButtonEnableResult.REROLL;
			}
			return RerollButtonEnableResult.UNLOCK;
		}
		return RerollButtonEnableResult.OUT_OF_CURRENCY;
	}

	public int BaconNumRerollLeft()
	{
		Entity gameEntity = GameState.Get().GetGameEntity();
		if (gameEntity == null)
		{
			return 0;
		}
		return gameEntity.GetTag(GAME_TAG.BACON_NUM_MAX_REROLL_PER_HERO) - GetTag(GAME_TAG.BACON_NUM_MULLIGAN_REFRESH_USED);
	}

	public virtual int BaconFreeRerollLeft()
	{
		Player player = GetController();
		if (player == null)
		{
			return 0;
		}
		return player.GetTag(GAME_TAG.BACON_PREMIUM_FREE_REROLLS) - player.GetTag(GAME_TAG.BACON_NUM_FREE_REROLLS_USED);
	}

	public Entity GetEquipmentEntity()
	{
		int equipmentCardId = GetTag(GAME_TAG.LETTUCE_EQUIPMENT_ID);
		if (equipmentCardId == 0)
		{
			return null;
		}
		foreach (int ability in GetLettuceAbilityEntityIDs())
		{
			Entity entity = GameState.Get().GetEntity(ability);
			if (GameUtils.TranslateCardIdToDbId(entity.GetCardId()) == equipmentCardId)
			{
				return entity;
			}
		}
		return null;
	}

	public bool CanBeMagnitizedBy(Entity entity)
	{
		if (HasRace(TAG_RACE.MECHANICAL))
		{
			return true;
		}
		if (entity.HasTag(GAME_TAG.MAGNETIC_TO_RACE))
		{
			return HasRace((TAG_RACE)entity.GetTag(GAME_TAG.MAGNETIC_TO_RACE));
		}
		return false;
	}

	public int GetUsableTitanAbilityCount()
	{
		if (!IsTitan())
		{
			return 0;
		}
		int sum = 0;
		foreach (int entityID in m_subCardIDs)
		{
			if (!GameState.Get().GetEntity(entityID).HasTag(GAME_TAG.LITERALLY_UNPLAYABLE))
			{
				sum++;
			}
		}
		return sum;
	}

	public bool HasUsableTitanAbilities()
	{
		return GetUsableTitanAbilityCount() > 0;
	}

	public bool IsHiddenSecret()
	{
		if (IsSecret() && IsHidden())
		{
			return IsControlledByConcealedPlayer();
		}
		return false;
	}

	public bool IsHiddenForge()
	{
		if ((IsForgeable() || HasTag(GAME_TAG.FORGE_REVEALED)) && IsHidden())
		{
			return IsControlledByConcealedPlayer();
		}
		return false;
	}
}
