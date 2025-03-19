using System;
using System.Collections;
using System.Collections.Generic;
using Blizzard.T5.Game.Spells;
using PegasusGame;
using UnityEngine;

public class HistoryManager : CardTileListDisplay
{
	public delegate void BigCardStartedCallback();

	public delegate void BigCardFinishedCallback();

	private class BigCardEntry
	{
		public HistoryInfo m_info;

		public BigCardStartedCallback m_startedCallback;

		public BigCardFinishedCallback m_finishedCallback;

		public bool m_fromMetaData;

		public bool m_countered;

		public bool m_waitForSecretSpell;

		public int m_displayTimeMS;
	}

	private enum BigCardTransformState
	{
		INVALID,
		PRE_TRANSFORM,
		TRANSFORM,
		POST_TRANSFORM
	}

	private class TileEntry
	{
		public HistoryInfo m_lastAttacker;

		public HistoryInfo m_lastDefender;

		public HistoryInfo m_lastCardPlayed;

		public HistoryInfo m_sourceOwner;

		public HistoryInfo m_lastCardTriggered;

		public HistoryInfo m_lastCardTargeted;

		public List<HistoryInfo> m_affectedCards = new List<HistoryInfo>();

		public HistoryInfo m_fatigueInfo;

		public HistoryInfo m_burnedCardsInfo;

		public bool m_usingMetaDataOverride;

		public bool m_complete;

		public void SetAttacker(Entity attacker)
		{
			m_lastAttacker = new HistoryInfo();
			m_lastAttacker.m_infoType = HistoryInfoType.ATTACK;
			m_lastAttacker.SetOriginalEntity(attacker);
		}

		public void SetDefender(Entity defender)
		{
			m_lastDefender = new HistoryInfo();
			m_lastDefender.SetOriginalEntity(defender);
		}

		public void SetCardPlayed(Entity entity, Entity suboption = null)
		{
			m_lastCardPlayed = new HistoryInfo();
			if (entity.IsWeapon())
			{
				m_lastCardPlayed.m_infoType = HistoryInfoType.WEAPON_PLAYED;
			}
			else
			{
				m_lastCardPlayed.m_infoType = HistoryInfoType.CARD_PLAYED;
			}
			if (suboption != null)
			{
				m_lastCardPlayed.SetOriginalEntity(suboption);
				SetSourceOwner(entity);
			}
			else
			{
				m_lastCardPlayed.SetOriginalEntity(entity);
			}
			if (entity.GetLettuceAbilityOwner() != null)
			{
				SetSourceOwner(entity.GetLettuceAbilityOwner());
			}
		}

		public void SetSourceOwner(Entity entity)
		{
			if (entity != null)
			{
				m_sourceOwner = new HistoryInfo();
				m_sourceOwner.SetOriginalEntity(entity);
			}
		}

		public void SetCardTargeted(Entity entity)
		{
			if (entity != null)
			{
				m_lastCardTargeted = new HistoryInfo();
				m_lastCardTargeted.SetOriginalEntity(entity);
			}
		}

		public void SetCardTriggered(Entity entity)
		{
			if (!entity.IsGame() && !entity.IsPlayer())
			{
				m_lastCardTriggered = new HistoryInfo();
				m_lastCardTriggered.m_infoType = HistoryInfoType.TRIGGER;
				m_lastCardTriggered.SetOriginalEntity(entity);
			}
		}

		public void SetFatigue()
		{
			m_fatigueInfo = new HistoryInfo();
			m_fatigueInfo.m_infoType = HistoryInfoType.FATIGUE;
		}

		public void SetBurnedCards()
		{
			m_burnedCardsInfo = new HistoryInfo();
			m_burnedCardsInfo.m_infoType = HistoryInfoType.BURNED_CARDS;
		}

		public bool CanDuplicateAllEntities(bool duplicateHiddenNonSecrets, bool isEndOfHistory = false)
		{
			HistoryInfo sourceInfo = GetSourceInfo();
			if (ShouldDuplicateEntity(sourceInfo) && !sourceInfo.CanDuplicateEntity(duplicateHiddenNonSecrets, isEndOfHistory))
			{
				return false;
			}
			HistoryInfo targetInfo = GetTargetInfo();
			if (ShouldDuplicateEntity(targetInfo) && !targetInfo.CanDuplicateEntity(duplicateHiddenNonSecrets, isEndOfHistory))
			{
				return false;
			}
			for (int i = 0; i < m_affectedCards.Count; i++)
			{
				HistoryInfo affectedInfo = m_affectedCards[i];
				if (ShouldDuplicateEntity(affectedInfo) && !affectedInfo.CanDuplicateEntity(duplicateHiddenNonSecrets, isEndOfHistory))
				{
					return false;
				}
			}
			return true;
		}

		public void DuplicateAllEntities(bool duplicateHiddenNonSecrets, bool isEndOfHistory = false)
		{
			HistoryInfo sourceInfo = GetSourceInfo();
			if (ShouldDuplicateEntity(sourceInfo))
			{
				sourceInfo.DuplicateEntity(duplicateHiddenNonSecrets, isEndOfHistory);
			}
			HistoryInfo targetInfo = GetTargetInfo();
			if (ShouldDuplicateEntity(targetInfo))
			{
				targetInfo.DuplicateEntity(duplicateHiddenNonSecrets, isEndOfHistory);
			}
			for (int i = 0; i < m_affectedCards.Count; i++)
			{
				HistoryInfo affectedInfo = m_affectedCards[i];
				if (ShouldDuplicateEntity(affectedInfo))
				{
					affectedInfo.DuplicateEntity(duplicateHiddenNonSecrets, isEndOfHistory);
				}
			}
		}

		public bool ShouldDuplicateEntity(HistoryInfo info)
		{
			if (info == null)
			{
				return false;
			}
			if (info == m_fatigueInfo)
			{
				return false;
			}
			if (info == m_burnedCardsInfo)
			{
				return false;
			}
			return true;
		}

		public HistoryInfo GetSourceInfo()
		{
			if (m_lastCardPlayed != null)
			{
				return m_lastCardPlayed;
			}
			if (m_lastAttacker != null)
			{
				return m_lastAttacker;
			}
			if (m_lastCardTriggered != null)
			{
				return m_lastCardTriggered;
			}
			if (m_fatigueInfo != null)
			{
				return m_fatigueInfo;
			}
			if (m_burnedCardsInfo != null)
			{
				return m_burnedCardsInfo;
			}
			Debug.LogError("HistoryEntry.GetSourceInfo() - no source info");
			return null;
		}

		public HistoryInfo GetTargetInfo()
		{
			if (m_lastCardPlayed != null && m_lastCardTargeted != null)
			{
				return m_lastCardTargeted;
			}
			if (m_lastAttacker != null && m_lastDefender != null)
			{
				return m_lastDefender;
			}
			return null;
		}
	}

	private class TileEntryBuffer
	{
		private const int MAX_PREVIOUS_TILE_ENTRIES = 5;

		private int m_queuedEntriesBufferIndex;

		private TileEntry[] m_queuedEntriesBuffer = new TileEntry[5];

		public int Length => m_queuedEntriesBuffer.Length;

		public void Clear()
		{
			for (int bufferIndex = 0; bufferIndex < 5; bufferIndex++)
			{
				m_queuedEntriesBuffer[bufferIndex] = null;
			}
		}

		public void AddHistoryEntry(ref TileEntry newEntry)
		{
			m_queuedEntriesBuffer[m_queuedEntriesBufferIndex] = newEntry;
			m_queuedEntriesBufferIndex++;
			m_queuedEntriesBufferIndex %= 5;
		}

		public TileEntry GetHistoryEntry(int index)
		{
			int tempIndex = m_queuedEntriesBufferIndex - 1 - index;
			tempIndex %= 5;
			if (tempIndex < 0)
			{
				tempIndex += 5;
			}
			return m_queuedEntriesBuffer[tempIndex];
		}
	}

	private class TileLoadedCallbackData
	{
		public HistoryInfo m_sourceInfo { get; set; }

		public List<HistoryInfo> m_childInfos { get; } = new List<HistoryInfo>();

		public HistoryInfo m_sourceOwnerInfo { get; set; }
	}

	public Texture m_mageSecretTexture;

	public Texture m_paladinSecretTexture;

	public Texture m_hunterSecretTexture;

	public Texture m_rogueSecretTexture;

	public Texture m_wandererSecretTexture;

	public Texture m_FatigueTexture;

	public Texture m_BurnedCardsTexture;

	public Texture m_forgeCardsTexture;

	public Spell[] m_TransformSpells;

	public BoxCollider m_baseHistoryCollider;

	public BoxCollider m_shortHistoryCollider;

	private const float BIG_CARD_POWER_PROCESSOR_DELAY_TIME = 1f;

	private const float BIG_CARD_SPELL_DISPLAY_TIME = 4f;

	private const float BIG_CARD_MINION_DISPLAY_TIME = 3f;

	private const float BIG_CARD_LETTUCE_ABILITY_DISPLAY_TIME = 1.65f;

	private const float BIG_CARD_HERO_POWER_DISPLAY_TIME = 4f;

	private const float BIG_CARD_SECRET_DISPLAY_TIME = 4f;

	private const float BIG_CARD_POST_TRANSFORM_DISPLAY_TIME = 2f;

	private const float BIG_CARD_META_DATA_DEFAULT_DISPLAY_TIME = 1.5f;

	private const float BIG_CARD_META_DATA_FAST_DISPLAY_TIME = 1f;

	private const float SPACE_BETWEEN_TILES = 0.15f;

	private const float MOVE_Y_DOWN_AMOUNT = -0.32f;

	private Entity m_lastTransformed;

	private static HistoryManager s_instance;

	private bool m_historyDisabled;

	private List<HistoryCard> m_historyTiles = new List<HistoryCard>();

	private HistoryCard m_currentlyMousedOverTile;

	private List<TileEntry> m_queuedEntries = new List<TileEntry>();

	private TileEntryBuffer m_queuedEntriesPrevious = new TileEntryBuffer();

	private Vector3[] m_bigCardPath;

	private Vector3[] m_lettuceAbilityBigCardPath;

	private BigCardEntry m_pendingBigCardEntry;

	private HistoryCard m_currentBigCard;

	private bool m_showingBigCard;

	private bool m_bigCardWaitingForSecret;

	private bool m_bigCardWaitingForLettuceSpeedTile;

	private BigCardTransformState m_bigCardTransformState;

	private Spell m_bigCardTransformSpell;

	public static event Action<bool> HistoryBarHoverEvent;

	protected override void Awake()
	{
		base.Awake();
		s_instance = this;
		m_queuedEntriesPrevious.Clear();
	}

	protected override void OnDestroy()
	{
		s_instance = null;
		base.OnDestroy();
	}

	protected override void Start()
	{
		base.Start();
		StartCoroutine(WaitForBoardLoadedAndSetPaths());
	}

	public static HistoryManager Get()
	{
		return s_instance;
	}

	public bool IsHistoryEnabled()
	{
		return !m_historyDisabled;
	}

	public void DisableHistory()
	{
		m_historyDisabled = true;
		UpdateHistoryCollider();
	}

	public void EnableHistory()
	{
		m_historyDisabled = false;
		UpdateHistoryCollider();
	}

	private Entity CreatePreTransformedEntity(Entity entity)
	{
		int cardDbId = entity.GetTag(GAME_TAG.TRANSFORMED_FROM_CARD);
		if (cardDbId == 0)
		{
			return null;
		}
		string cardId = GameUtils.TranslateDbIdToCardId(cardDbId);
		if (string.IsNullOrEmpty(cardId))
		{
			return null;
		}
		Entity entity2 = new Entity();
		EntityDef preTransformEntityDef = DefLoader.Get().GetEntityDef(cardId);
		entity2.InitCard();
		entity2.ReplaceTags(preTransformEntityDef.GetTags());
		entity2.LoadCard(cardId);
		entity2.SetTag(GAME_TAG.CONTROLLER, entity.GetControllerId());
		entity2.SetTag(GAME_TAG.ZONE, TAG_ZONE.HAND);
		entity2.SetTag(GAME_TAG.PREMIUM, entity.GetPremiumType());
		entity2.SetTag(GAME_TAG.WATERMARK_OVERRIDE_CARD_SET, entity.GetWatermarkCardSetOverride());
		return entity2;
	}

	private Entity CreateFakeEntity(Entity entity)
	{
		string cardId = entity.GetCardId();
		if (string.IsNullOrEmpty(cardId))
		{
			return null;
		}
		Entity fakeEntity = new Entity();
		EntityDef fakeEntityDef = DefLoader.Get().GetEntityDef(cardId);
		fakeEntity.InitCard();
		fakeEntity.ReplaceTags(fakeEntityDef.GetTags());
		fakeEntity.LoadCard(cardId);
		fakeEntity.SetTag(GAME_TAG.CONTROLLER, entity.GetControllerId());
		fakeEntity.SetTag(GAME_TAG.ZONE, TAG_ZONE.HAND);
		fakeEntity.SetTag(GAME_TAG.PREMIUM, entity.GetPremiumType());
		fakeEntity.SetTag(GAME_TAG.WATERMARK_OVERRIDE_CARD_SET, entity.GetWatermarkCardSetOverride());
		if (cardId == "TTN_724t")
		{
			fakeEntity.SetTag(GAME_TAG.COST, entity.GetDefCost());
		}
		return fakeEntity;
	}

	public Entity CreateFakeHiddenEntity()
	{
		string cardId = "";
		Entity entity = new Entity();
		entity.InitCard();
		entity.LoadCard(cardId);
		entity.SetTag(GAME_TAG.CONTROLLER, GameState.Get().GetOpposingPlayer().GetEntityId());
		entity.SetTag(GAME_TAG.ZONE, TAG_ZONE.HAND);
		entity.SetTag(GAME_TAG.PREMIUM, TAG_PREMIUM.NORMAL);
		return entity;
	}

	private Entity CreateForgeTransformedEntity(Entity entity)
	{
		int cardDbId = entity.GetTag(GAME_TAG.FORGES_INTO);
		if (cardDbId == 0)
		{
			return null;
		}
		string cardId = GameUtils.TranslateDbIdToCardId(cardDbId);
		if (string.IsNullOrEmpty(cardId))
		{
			return null;
		}
		Entity postTransformEntity = new Entity();
		EntityDef postTransformEntityDef = DefLoader.Get().GetEntityDef(cardId);
		postTransformEntity.InitCard();
		postTransformEntity.ReplaceTags(postTransformEntityDef.GetTags());
		postTransformEntity.LoadCard(cardId);
		postTransformEntity.SetTag(GAME_TAG.CONTROLLER, entity.GetControllerId());
		postTransformEntity.SetTag(GAME_TAG.ZONE, TAG_ZONE.HAND);
		postTransformEntity.SetTag(GAME_TAG.PREMIUM, entity.GetPremiumType());
		postTransformEntity.SetTag(GAME_TAG.WATERMARK_OVERRIDE_CARD_SET, entity.GetWatermarkCardSetOverride());
		if (cardId == "TTN_724t")
		{
			int cost = Mathf.Max(entity.GetDefCost() - 2, 0);
			postTransformEntity.SetTag(GAME_TAG.COST, cost);
		}
		return postTransformEntity;
	}

	private Entity CreateSecretDeathrattleEntity(Entity entity)
	{
		if (!entity.HasSecretDeathrattle())
		{
			return null;
		}
		string cardId = "GIL_222t";
		if (string.IsNullOrEmpty(cardId))
		{
			return null;
		}
		Entity entity2 = new Entity();
		EntityDef secretDeathrattleEntDef = DefLoader.Get().GetEntityDef(cardId);
		entity2.InitCard();
		entity2.ReplaceTags(secretDeathrattleEntDef.GetTags());
		entity2.LoadCard(cardId);
		entity2.SetTag(GAME_TAG.CONTROLLER, entity.GetControllerId());
		entity2.SetTag(GAME_TAG.ZONE, TAG_ZONE.HAND);
		entity2.SetTag(GAME_TAG.PREMIUM, entity.GetPremiumType());
		entity2.SetTag(GAME_TAG.WATERMARK_OVERRIDE_CARD_SET, entity.GetWatermarkCardSetOverride());
		return entity2;
	}

	public void CreatePlayedTile(Entity playedEntity, Entity targetedEntity, Entity playedSubcard = null)
	{
		if (!m_historyDisabled)
		{
			TileEntry newEntry = new TileEntry();
			m_queuedEntries.Add(newEntry);
			newEntry.SetCardPlayed(playedEntity, playedSubcard);
			newEntry.SetCardTargeted(targetedEntity);
			if (newEntry.m_lastCardPlayed.GetDuplicatedEntity() == null)
			{
				StartCoroutine("WaitForCardLoadedAndDuplicateInfo", newEntry.m_lastCardPlayed);
			}
		}
	}

	public void CreateTriggerTile(Entity triggeredEntity)
	{
		if (!m_historyDisabled)
		{
			TileEntry newEntry = new TileEntry();
			m_queuedEntries.Add(newEntry);
			newEntry.SetCardTriggered(triggeredEntity);
		}
	}

	public void CreateAttackTile(Entity attacker, Entity defender, PowerTaskList taskList)
	{
		if (m_historyDisabled)
		{
			return;
		}
		TileEntry newEntry = new TileEntry();
		m_queuedEntries.Add(newEntry);
		newEntry.SetAttacker(attacker);
		newEntry.SetDefender(defender);
		Entity attackerDuplicate = newEntry.m_lastAttacker.GetDuplicatedEntity();
		Entity defenderDuplicate = newEntry.m_lastDefender.GetDuplicatedEntity();
		int attackerId = attacker.GetEntityId();
		int defenderId = defender.GetEntityId();
		int damageTaskIndex = -1;
		List<PowerTask> tasks = taskList.GetTaskList();
		for (int i = 0; i < tasks.Count; i++)
		{
			Network.PowerHistory power = tasks[i].GetPower();
			if (power.Type == Network.PowerType.META_DATA)
			{
				Network.HistMetaData metaData = (Network.HistMetaData)power;
				if (metaData.MetaType == HistoryMeta.Type.DAMAGE && metaData.Info.Contains(defenderId))
				{
					damageTaskIndex = i;
					break;
				}
			}
		}
		for (int j = 0; j < damageTaskIndex; j++)
		{
			Network.PowerHistory power2 = tasks[j].GetPower();
			switch (power2.Type)
			{
			case Network.PowerType.SHOW_ENTITY:
			{
				Network.HistShowEntity showEntity = (Network.HistShowEntity)power2;
				if (attackerId == showEntity.Entity.ID)
				{
					GameUtils.ApplyShowEntity(attackerDuplicate, showEntity);
				}
				if (defenderId == showEntity.Entity.ID)
				{
					GameUtils.ApplyShowEntity(defenderDuplicate, showEntity);
				}
				break;
			}
			case Network.PowerType.HIDE_ENTITY:
			{
				Network.HistHideEntity hideEntity = (Network.HistHideEntity)power2;
				if (attackerId == hideEntity.Entity)
				{
					GameUtils.ApplyHideEntity(attackerDuplicate, hideEntity);
				}
				if (defenderId == hideEntity.Entity)
				{
					GameUtils.ApplyHideEntity(defenderDuplicate, hideEntity);
				}
				break;
			}
			case Network.PowerType.TAG_CHANGE:
			{
				Network.HistTagChange tagChange = (Network.HistTagChange)power2;
				if (attackerId == tagChange.Entity)
				{
					GameUtils.ApplyTagChange(attackerDuplicate, tagChange);
				}
				if (defenderId == tagChange.Entity)
				{
					GameUtils.ApplyTagChange(defenderDuplicate, tagChange);
				}
				break;
			}
			}
		}
	}

	public void CreateFatigueTile()
	{
		if (!m_historyDisabled)
		{
			TileEntry newEntry = new TileEntry();
			m_queuedEntries.Add(newEntry);
			newEntry.SetFatigue();
		}
	}

	public void CreateBurnedCardsTile()
	{
		if (!m_historyDisabled)
		{
			TileEntry newEntry = new TileEntry();
			m_queuedEntries.Add(newEntry);
			newEntry.SetBurnedCards();
		}
	}

	public void MarkCurrentHistoryEntryAsCompleted()
	{
		if (!m_historyDisabled)
		{
			TileEntry entry = GetCurrentHistoryEntry();
			if (entry == null)
			{
				Log.Power.Print("HistoryManager.MarkCurrentHistoryEntryAsCompleted: There is no current History Entry!");
				return;
			}
			entry.m_complete = true;
			m_queuedEntriesPrevious.AddHistoryEntry(ref entry);
			LoadNextHistoryEntry();
		}
	}

	public bool HasHistoryEntry()
	{
		return GetCurrentHistoryEntry() != null;
	}

	public void NotifyDamageChanged(Entity entity, int damage)
	{
		if (entity == null || m_historyDisabled)
		{
			return;
		}
		TileEntry entry = GetCurrentHistoryEntry();
		if (entry == null)
		{
			Log.Power.Print("HistoryManager.NotifyDamageChanged: There is no current History Entry!");
			return;
		}
		if (IsEntityTheLastCardPlayed(entity))
		{
			Entity duplicatedEntity = entry.m_lastCardPlayed.GetDuplicatedEntity();
			if (duplicatedEntity != null)
			{
				int changeAmount = damage - duplicatedEntity.GetDamage();
				entry.m_lastCardPlayed.m_damageChangeAmount = changeAmount;
				entry.m_lastCardPlayed.m_splatType = duplicatedEntity.GetAlternateCost();
			}
			return;
		}
		if (IsEntityTheLastAttacker(entity))
		{
			Entity duplicatedEntity2 = entry.m_lastAttacker.GetDuplicatedEntity();
			if (duplicatedEntity2 != null)
			{
				int changeAmount2 = damage - duplicatedEntity2.GetDamage();
				entry.m_lastAttacker.m_damageChangeAmount = changeAmount2;
			}
			return;
		}
		if (IsEntityTheLastDefender(entity))
		{
			Entity duplicatedEntity3 = entry.m_lastDefender.GetDuplicatedEntity();
			if (duplicatedEntity3 != null)
			{
				int changeAmount3 = damage - duplicatedEntity3.GetDamage();
				entry.m_lastDefender.m_damageChangeAmount = changeAmount3;
			}
			return;
		}
		if (IsEntityTheLastCardTargeted(entity))
		{
			Entity duplicatedEntity4 = entry.m_lastCardTargeted.GetDuplicatedEntity();
			if (duplicatedEntity4 != null)
			{
				int changeAmount4 = damage - duplicatedEntity4.GetDamage();
				entry.m_lastCardTargeted.m_damageChangeAmount = changeAmount4;
			}
			return;
		}
		for (int i = 0; i < entry.m_affectedCards.Count; i++)
		{
			if (!IsEntityTheAffectedCard(entity, i))
			{
				continue;
			}
			Entity duplicatedEntity5 = entry.m_affectedCards[i].GetDuplicatedEntity();
			if (duplicatedEntity5 != null)
			{
				int changeAmount5 = damage - duplicatedEntity5.GetDamage();
				entry.m_affectedCards[i].m_damageChangeAmount = changeAmount5;
				if (entry.m_lastCardPlayed != null)
				{
					entry.m_affectedCards[i].m_splatType = entry.m_lastCardPlayed.GetSplatType();
				}
			}
			return;
		}
		if (NotifyEntityAffected(entity, allowDuplicates: false, fromMetaData: false))
		{
			NotifyDamageChanged(entity, damage);
		}
	}

	public void NotifyArmorChanged(Entity entity, int newArmor)
	{
		if (entity == null || m_historyDisabled || entity.GetArmor() - newArmor <= 0 || IsEntityTheLastCardPlayed(entity))
		{
			return;
		}
		TileEntry entry = GetCurrentHistoryEntry();
		if (entry == null)
		{
			Log.Power.Print("HistoryManager.NotifyArmorChanged: There is no current History Entry!");
			return;
		}
		if (IsEntityTheLastAttacker(entity))
		{
			Entity duplicatedEntity = entry.m_lastAttacker.GetDuplicatedEntity();
			if (duplicatedEntity != null)
			{
				int changeAmount = duplicatedEntity.GetArmor() - newArmor;
				entry.m_lastAttacker.m_armorChangeAmount = Mathf.Max(entry.m_lastAttacker.m_armorChangeAmount, changeAmount);
			}
			return;
		}
		if (IsEntityTheLastDefender(entity))
		{
			Entity duplicatedEntity2 = entry.m_lastDefender.GetDuplicatedEntity();
			if (duplicatedEntity2 != null)
			{
				int changeAmount2 = duplicatedEntity2.GetArmor() - newArmor;
				entry.m_lastDefender.m_armorChangeAmount = Mathf.Max(entry.m_lastDefender.m_armorChangeAmount, changeAmount2);
			}
			return;
		}
		if (IsEntityTheLastCardTargeted(entity))
		{
			Entity duplicatedEntity3 = entry.m_lastCardTargeted.GetDuplicatedEntity();
			if (duplicatedEntity3 != null)
			{
				int changeAmount3 = duplicatedEntity3.GetArmor() - newArmor;
				entry.m_lastCardTargeted.m_armorChangeAmount = Mathf.Max(entry.m_lastCardTargeted.m_armorChangeAmount, changeAmount3);
			}
			return;
		}
		for (int i = 0; i < entry.m_affectedCards.Count; i++)
		{
			if (!IsEntityTheAffectedCard(entity, i))
			{
				continue;
			}
			Entity duplicatedEntity4 = entry.m_affectedCards[i].GetDuplicatedEntity();
			if (duplicatedEntity4 != null)
			{
				int changeAmount4 = duplicatedEntity4.GetArmor() - newArmor;
				entry.m_affectedCards[i].m_armorChangeAmount = Mathf.Max(entry.m_affectedCards[i].m_armorChangeAmount, changeAmount4);
				if (entry.m_lastCardPlayed != null)
				{
					entry.m_affectedCards[i].m_splatType = entry.m_lastCardPlayed.GetSplatType();
				}
			}
			return;
		}
		if (NotifyEntityAffected(entity, allowDuplicates: false, fromMetaData: false))
		{
			NotifyArmorChanged(entity, newArmor);
		}
	}

	public void NotifyHealthChanged(Entity entity, int health)
	{
		if (entity == null || m_historyDisabled)
		{
			return;
		}
		TileEntry entry = GetCurrentHistoryEntry();
		if (entry == null)
		{
			Log.Power.Print("HistoryManager.NotifyHealthChanged: There is no current History Entry!");
			return;
		}
		if (IsEntityTheLastCardPlayed(entity))
		{
			Entity duplicatedEntity = entry.m_lastCardPlayed.GetDuplicatedEntity();
			if (duplicatedEntity != null)
			{
				int delta = health - duplicatedEntity.GetHealth();
				entry.m_lastCardPlayed.m_maxHealthChangeAmount = delta;
			}
			return;
		}
		if (IsEntityTheLastAttacker(entity))
		{
			Entity duplicatedEntity2 = entry.m_lastAttacker.GetDuplicatedEntity();
			if (duplicatedEntity2 != null)
			{
				int delta2 = health - duplicatedEntity2.GetHealth();
				entry.m_lastAttacker.m_maxHealthChangeAmount = delta2;
			}
			return;
		}
		if (IsEntityTheLastDefender(entity))
		{
			Entity duplicatedEntity3 = entry.m_lastDefender.GetDuplicatedEntity();
			if (duplicatedEntity3 != null)
			{
				int delta3 = health - duplicatedEntity3.GetHealth();
				entry.m_lastDefender.m_maxHealthChangeAmount = delta3;
			}
			return;
		}
		if (IsEntityTheLastCardTargeted(entity))
		{
			Entity duplicatedEntity4 = entry.m_lastCardTargeted.GetDuplicatedEntity();
			if (duplicatedEntity4 != null)
			{
				int delta4 = health - duplicatedEntity4.GetHealth();
				entry.m_lastCardTargeted.m_maxHealthChangeAmount = delta4;
			}
			return;
		}
		for (int i = 0; i < entry.m_affectedCards.Count; i++)
		{
			if (IsEntityTheAffectedCard(entity, i))
			{
				Entity duplicatedEntity5 = entry.m_affectedCards[i].GetDuplicatedEntity();
				if (duplicatedEntity5 != null)
				{
					int delta5 = health - duplicatedEntity5.GetHealth();
					entry.m_affectedCards[i].m_maxHealthChangeAmount = delta5;
				}
				return;
			}
		}
		if (NotifyEntityAffected(entity, allowDuplicates: false, fromMetaData: false))
		{
			NotifyHealthChanged(entity, health);
		}
	}

	public void OverrideCurrentHistoryEntryWithMetaData()
	{
		TileEntry entry = GetCurrentHistoryEntry();
		if (entry != null && !entry.m_usingMetaDataOverride)
		{
			entry.m_usingMetaDataOverride = true;
			entry.m_affectedCards.Clear();
		}
	}

	public void OverrideCurrentHistoryTriggerSource(Entity entity)
	{
		if (entity != null)
		{
			GetCurrentHistoryEntry()?.SetCardTriggered(entity);
		}
	}

	public void OverrideCurrentHistorySourceOwner(Entity entity)
	{
		if (entity != null)
		{
			GetCurrentHistoryEntry()?.SetSourceOwner(entity);
		}
	}

	private HistoryInfo GetHistoryInfoForEntity(TileEntry entry, Entity entity)
	{
		if (IsEntityTheLastAttacker(entity))
		{
			return entry.m_lastAttacker;
		}
		if (IsEntityTheLastDefender(entity))
		{
			return entry.m_lastDefender;
		}
		if (IsEntityTheLastCardTargeted(entity))
		{
			return entry.m_lastCardTargeted;
		}
		if (entry.m_lastCardPlayed != null && entity == entry.m_lastCardPlayed.GetOriginalEntity())
		{
			return entry.m_lastCardPlayed;
		}
		for (int i = 0; i < entry.m_affectedCards.Count; i++)
		{
			if (IsEntityTheAffectedCard(entry, entity, i))
			{
				return entry.m_affectedCards[i];
			}
		}
		return null;
	}

	public bool NotifyEntityAffected(int entityId, bool allowDuplicates, bool fromMetaData, bool dontDuplicateUntilEnd = false, bool isBurnedCard = false, bool isPoisonous = false, bool isCriticalHit = false)
	{
		Entity entity = GameState.Get().GetEntity(entityId);
		return NotifyEntityAffected(entity, allowDuplicates, fromMetaData, dontDuplicateUntilEnd, isBurnedCard, isPoisonous, isCriticalHit);
	}

	public bool NotifyEntityAffected(Entity entity, bool allowDuplicates, bool fromMetaData, bool dontDuplicateUntilEnd = false, bool isBurnedCard = false, bool isPoisonous = false, bool isCriticalHit = false)
	{
		if (entity == null)
		{
			return false;
		}
		if (m_historyDisabled)
		{
			return false;
		}
		if (entity.IsEnchantment())
		{
			return false;
		}
		TileEntry entry = GetCurrentHistoryEntry();
		if (entry != null)
		{
			if (!fromMetaData && entry.m_usingMetaDataOverride)
			{
				return false;
			}
			if (!allowDuplicates)
			{
				HistoryInfo info = GetHistoryInfoForEntity(entry, entity);
				if (info != null)
				{
					if (dontDuplicateUntilEnd)
					{
						info.m_dontDuplicateUntilEnd = dontDuplicateUntilEnd;
					}
					if (isBurnedCard)
					{
						info.m_isBurnedCard = isBurnedCard;
					}
					if (isPoisonous)
					{
						info.m_isPoisonous = isPoisonous;
					}
					if (isCriticalHit)
					{
						info.m_isCriticalHit = isCriticalHit;
					}
					return false;
				}
			}
			HistoryInfo newAffectedCard = new HistoryInfo();
			newAffectedCard.m_dontDuplicateUntilEnd = dontDuplicateUntilEnd;
			newAffectedCard.m_isBurnedCard = isBurnedCard;
			newAffectedCard.m_isPoisonous = isPoisonous;
			newAffectedCard.m_isCriticalHit = isCriticalHit;
			newAffectedCard.SetOriginalEntity(entity);
			entry.m_affectedCards.Add(newAffectedCard);
			return true;
		}
		for (int bufferIndex = 0; bufferIndex < m_queuedEntriesPrevious.Length; bufferIndex++)
		{
			entry = m_queuedEntriesPrevious.GetHistoryEntry(bufferIndex);
			if (entry == null)
			{
				Log.Power.Print("HistoryManager.NotifyEntityAffected(): There is no current History Entry!");
				return false;
			}
			if ((!fromMetaData && entry.m_usingMetaDataOverride) || allowDuplicates)
			{
				continue;
			}
			HistoryInfo info2 = GetHistoryInfoForEntity(entry, entity);
			if (info2 != null)
			{
				if (dontDuplicateUntilEnd)
				{
					info2.m_dontDuplicateUntilEnd = dontDuplicateUntilEnd;
				}
				if (isBurnedCard)
				{
					info2.m_isBurnedCard = isBurnedCard;
				}
				if (isPoisonous)
				{
					info2.m_isPoisonous = isPoisonous;
				}
				return false;
			}
		}
		return false;
	}

	public void NotifyEntityDied(int entityId)
	{
		Entity entity = GameState.Get().GetEntity(entityId);
		NotifyEntityDied(entity);
	}

	public void NotifyEntityDied(Entity entity)
	{
		if (m_historyDisabled || entity.IsEnchantment() || IsEntityTheLastCardPlayed(entity))
		{
			return;
		}
		TileEntry entry = GetCurrentHistoryEntry();
		if (IsEntityTheLastAttacker(entity))
		{
			entry.m_lastAttacker.SetDied(set: true);
			return;
		}
		if (IsEntityTheLastDefender(entity))
		{
			entry.m_lastDefender.SetDied(set: true);
			return;
		}
		if (IsEntityTheLastCardTargeted(entity))
		{
			entry.m_lastCardTargeted.SetDied(set: true);
			return;
		}
		if (entry != null)
		{
			for (int i = 0; i < entry.m_affectedCards.Count; i++)
			{
				if (IsEntityTheAffectedCard(entity, i))
				{
					entry.m_affectedCards[i].SetDied(set: true);
					return;
				}
			}
		}
		if (!IsDeadInLaterHistoryEntry(entity) && NotifyEntityAffected(entity, allowDuplicates: false, fromMetaData: false))
		{
			NotifyEntityDied(entity);
		}
	}

	public bool NotifyRemoveEntityFromAffectedList(int entityID)
	{
		TileEntry entry = GetCurrentHistoryEntry();
		if (entry != null)
		{
			foreach (HistoryInfo info in entry.m_affectedCards)
			{
				if (info.GetOriginalEntity().GetEntityId() == entityID)
				{
					entry.m_affectedCards.Remove(info);
					return true;
				}
			}
		}
		return false;
	}

	public void NotifyOfInput(float zPosition)
	{
		if (m_historyTiles.Count == 0)
		{
			CheckForMouseOff();
			return;
		}
		if (GameState.Get().GetGameEntity().ShouldSuppressHistoryMouseOver())
		{
			CheckForMouseOff();
			return;
		}
		HistoryManager.HistoryBarHoverEvent?.Invoke(obj: true);
		float lowestBottom = 1000f;
		float highestTop = -1000f;
		float closestTileDistance = 1000f;
		HistoryCard closestTile = null;
		foreach (HistoryCard card in m_historyTiles)
		{
			if (!card.HasBeenShown())
			{
				continue;
			}
			Collider tileCollider = card.GetTileCollider();
			if (!(tileCollider == null))
			{
				float bottom = tileCollider.bounds.center.z - tileCollider.bounds.extents.z;
				float top = tileCollider.bounds.center.z + tileCollider.bounds.extents.z;
				if (bottom < lowestBottom)
				{
					lowestBottom = bottom;
				}
				if (top > highestTop)
				{
					highestTop = top;
				}
				float distanceToBottom = Mathf.Abs(zPosition - bottom);
				if (distanceToBottom < closestTileDistance)
				{
					closestTileDistance = distanceToBottom;
					closestTile = card;
				}
				float distanceToTop = Mathf.Abs(zPosition - top);
				if (distanceToTop < closestTileDistance)
				{
					closestTileDistance = distanceToTop;
					closestTile = card;
				}
			}
		}
		if (zPosition < lowestBottom || zPosition > highestTop)
		{
			CheckForMouseOff();
			return;
		}
		if (closestTile == null)
		{
			CheckForMouseOff();
			return;
		}
		m_SoundDucker.StartDucking();
		if (!(closestTile == m_currentlyMousedOverTile))
		{
			if (m_currentlyMousedOverTile != null)
			{
				m_currentlyMousedOverTile.NotifyMousedOut();
			}
			else
			{
				FadeVignetteIn();
			}
			m_currentlyMousedOverTile = closestTile;
			closestTile.NotifyMousedOver();
		}
	}

	public void NotifyOfMouseOff()
	{
		HistoryManager.HistoryBarHoverEvent?.Invoke(obj: false);
		CheckForMouseOff();
	}

	public void UpdateLayout()
	{
		if (UserIsMousedOverAHistoryTile())
		{
			return;
		}
		float zOffset = 0f;
		Vector3 topPosition = GetTopTilePosition();
		for (int i = m_historyTiles.Count - 1; i >= 0; i--)
		{
			int useHalfSizeOffset = 0;
			if (m_historyTiles[i].IsHalfSize())
			{
				useHalfSizeOffset = 1;
			}
			Collider tileCollider = m_historyTiles[i].GetTileCollider();
			float halfSizeOffset = 0f;
			if (tileCollider != null)
			{
				halfSizeOffset = tileCollider.bounds.size.z / 2f;
			}
			Vector3 newPosition = new Vector3(topPosition.x, topPosition.y, topPosition.z - zOffset + (float)useHalfSizeOffset * halfSizeOffset);
			m_historyTiles[i].MarkAsShown();
			iTween.MoveTo(m_historyTiles[i].gameObject, newPosition, 1f);
			if (tileCollider != null)
			{
				zOffset += tileCollider.bounds.size.z + 0.15f;
			}
		}
		DestroyHistoryTilesThatFallOffTheEnd();
	}

	public int GetNumHistoryTiles()
	{
		return m_historyTiles.Count;
	}

	public int GetIndexForTile(HistoryCard tile)
	{
		for (int i = 0; i < m_historyTiles.Count; i++)
		{
			if (m_historyTiles[i] == tile)
			{
				return i;
			}
		}
		Debug.LogWarning("HistoryManager.GetIndexForTile() - that Tile doesn't exist!");
		return -1;
	}

	public void OnEntityRevealed()
	{
		GetCurrentHistoryEntry()?.DuplicateAllEntities(duplicateHiddenNonSecrets: false);
	}

	private void LoadNextHistoryEntry()
	{
		if (m_queuedEntries.Count != 0 && m_queuedEntries[0].m_complete)
		{
			StartCoroutine(LoadNextHistoryEntryWhenLoaded());
		}
	}

	private IEnumerator LoadNextHistoryEntryWhenLoaded()
	{
		TileEntry currentEntry = m_queuedEntries[0];
		m_queuedEntries.RemoveAt(0);
		while (!currentEntry.CanDuplicateAllEntities(duplicateHiddenNonSecrets: true, isEndOfHistory: true))
		{
			yield return null;
		}
		if (currentEntry.GetSourceInfo() != null && currentEntry.GetSourceInfo().GetOriginalEntity() != null && currentEntry.GetSourceInfo().GetOriginalEntity().IsEnchantment())
		{
			LoadNextHistoryEntry();
			yield break;
		}
		currentEntry.DuplicateAllEntities(duplicateHiddenNonSecrets: true, isEndOfHistory: true);
		HistoryInfo sourceInfo = currentEntry.GetSourceInfo();
		if (sourceInfo == null || !sourceInfo.HasValidDisplayEntity())
		{
			LoadNextHistoryEntry();
			yield break;
		}
		Entity originalEntity = sourceInfo.GetOriginalEntity();
		bool skipTransformHistory = false;
		if (originalEntity != null && originalEntity.IsControlledByFriendlySidePlayer() && originalEntity.HasTag(GAME_TAG.FORGED))
		{
			skipTransformHistory = true;
		}
		if (originalEntity != null && !originalEntity.HasTag(GAME_TAG.JUST_PLAYED))
		{
			skipTransformHistory = true;
		}
		if (originalEntity != null && originalEntity.HasTag(GAME_TAG.COLOSSAL) && m_lastTransformed == originalEntity)
		{
			skipTransformHistory = true;
		}
		if (sourceInfo.m_infoType != HistoryInfoType.FATIGUE && sourceInfo.m_infoType != HistoryInfoType.BURNED_CARDS && !skipTransformHistory)
		{
			CreateTransformTile(sourceInfo.GetDuplicatedEntity(), originalEntity);
			m_lastTransformed = originalEntity;
		}
		else
		{
			m_lastTransformed = null;
		}
		TileLoadedCallbackData callbackData = new TileLoadedCallbackData
		{
			m_sourceInfo = sourceInfo,
			m_sourceOwnerInfo = currentEntry.m_sourceOwner
		};
		HistoryInfo targetInfo = currentEntry.GetTargetInfo();
		if (targetInfo != null)
		{
			callbackData.m_childInfos.Add(targetInfo);
		}
		if (currentEntry.m_affectedCards.Count > 0)
		{
			callbackData.m_childInfos.AddRange(currentEntry.m_affectedCards);
		}
		else if (originalEntity != null && originalEntity.IsTradeable() && originalEntity.IsControlledByOpposingSidePlayer() && sourceInfo.GetDuplicatedEntity().HasTag(GAME_TAG.IS_USING_TRADE_OPTION))
		{
			Entity fakeEntity = CreateFakeHiddenEntity();
			HistoryInfo fakeInfo = new HistoryInfo();
			fakeInfo.SetOriginalEntity(fakeEntity);
			fakeInfo.DuplicateEntity(duplicateHiddenNonSecret: true, isEndOfHistory: true);
			callbackData.m_childInfos.Add(fakeInfo);
		}
		AssetLoader.Get().InstantiatePrefab("HistoryCard.prefab:f8193c3e146b62342b8fb2c0494ec447", TileLoadedCallback, callbackData, AssetLoadingOptions.IgnorePrefabPosition);
	}

	public void CreateForgeTransformTile(Entity originalEntity)
	{
		if (originalEntity != null)
		{
			int cardDbId = originalEntity.GetTag(GAME_TAG.FORGES_INTO);
			if (cardDbId != 0 && !string.IsNullOrEmpty(GameUtils.TranslateDbIdToCardId(cardDbId)))
			{
				Entity preTransformedEntity = CreateFakeEntity(originalEntity);
				HistoryInfo preTransformInfo = new HistoryInfo();
				preTransformInfo.SetOriginalEntity(preTransformedEntity);
				preTransformInfo.DuplicateEntity(duplicateHiddenNonSecret: true, isEndOfHistory: true);
				Entity postTransformedEntity = CreateForgeTransformedEntity(originalEntity);
				HistoryInfo postTransformInfo = new HistoryInfo();
				postTransformInfo.SetOriginalEntity(postTransformedEntity);
				postTransformInfo.DuplicateEntity(duplicateHiddenNonSecret: true, isEndOfHistory: true);
				TileLoadedCallbackData callbackData = new TileLoadedCallbackData
				{
					m_sourceInfo = preTransformInfo
				};
				callbackData.m_childInfos.Add(postTransformInfo);
				AssetLoader.Get().InstantiatePrefab("HistoryCard.prefab:f8193c3e146b62342b8fb2c0494ec447", TileLoadedCallback, callbackData, AssetLoadingOptions.IgnorePrefabPosition);
			}
		}
	}

	public void CreateTransformTile(Entity duplicatedEntity, Entity originalEntity)
	{
		if (duplicatedEntity != null && originalEntity != null)
		{
			int cardDbId = duplicatedEntity.GetTag(GAME_TAG.TRANSFORMED_FROM_CARD);
			if (cardDbId != 0 && !string.IsNullOrEmpty(GameUtils.TranslateDbIdToCardId(cardDbId)))
			{
				Entity preTransformedEntity = CreatePreTransformedEntity(duplicatedEntity);
				HistoryInfo preTransformInfo = new HistoryInfo();
				preTransformInfo.SetOriginalEntity(preTransformedEntity);
				preTransformInfo.DuplicateEntity(duplicateHiddenNonSecret: true, isEndOfHistory: true);
				Entity postTransformedEntity = CreateFakeEntity(originalEntity);
				HistoryInfo postTransformInfo = new HistoryInfo();
				postTransformInfo.SetOriginalEntity(postTransformedEntity);
				postTransformInfo.DuplicateEntity(duplicateHiddenNonSecret: true, isEndOfHistory: true);
				TileLoadedCallbackData callbackData = new TileLoadedCallbackData
				{
					m_sourceInfo = preTransformInfo
				};
				callbackData.m_childInfos.Add(postTransformInfo);
				AssetLoader.Get().InstantiatePrefab("HistoryCard.prefab:f8193c3e146b62342b8fb2c0494ec447", TileLoadedCallback, callbackData, AssetLoadingOptions.IgnorePrefabPosition);
			}
		}
	}

	private IEnumerator WaitForCardLoadedAndDuplicateInfo(HistoryInfo info)
	{
		while (!info.CanDuplicateEntity(duplicateHiddenNonSecret: false))
		{
			yield return null;
		}
		info.DuplicateEntity(duplicateHiddenNonSecret: false, isEndOfHistory: false);
	}

	private bool IsEntityTheLastCardTargeted(Entity entity)
	{
		TileEntry entry = GetCurrentHistoryEntry();
		if (entry == null)
		{
			Log.Power.Print("HistoryManager.IsEntityTheLastCardTargeted: There is no current History Entry!");
			return false;
		}
		if (entry.m_lastCardTargeted != null)
		{
			return entity == entry.m_lastCardTargeted.GetOriginalEntity();
		}
		return false;
	}

	private bool IsEntityTheLastAttacker(Entity entity)
	{
		TileEntry entry = GetCurrentHistoryEntry();
		if (entry == null)
		{
			Log.Power.Print("HistoryManager.IsEntityTheLastAttacker: There is no current History Entry!");
			return false;
		}
		if (entry.m_lastAttacker != null)
		{
			return entity == entry.m_lastAttacker.GetOriginalEntity();
		}
		return false;
	}

	private bool IsEntityTheLastCardPlayed(Entity entity)
	{
		TileEntry entry = GetCurrentHistoryEntry();
		if (entry == null)
		{
			Log.Power.Print("HistoryManager.IsEntityTheLastCardPlayed: There is no current History Entry!");
			return false;
		}
		if (entry.m_lastCardPlayed != null)
		{
			return entity == entry.m_lastCardPlayed.GetOriginalEntity();
		}
		return false;
	}

	private bool IsEntityTheLastDefender(Entity entity)
	{
		TileEntry entry = GetCurrentHistoryEntry();
		if (entry == null)
		{
			Log.Power.Print("HistoryManager.IsEntityTheLastDefender: There is no current History Entry!");
			return false;
		}
		if (entry.m_lastDefender != null)
		{
			return entity == entry.m_lastDefender.GetOriginalEntity();
		}
		return false;
	}

	private bool IsEntityTheAffectedCard(Entity entity, int index)
	{
		return IsEntityTheAffectedCard(GetCurrentHistoryEntry(), entity, index);
	}

	private bool IsEntityTheAffectedCard(TileEntry entry, Entity entity, int index)
	{
		if (entry == null)
		{
			Log.Power.Print("HistoryManager.IsEntityTheAffectedCard: There is no current History Entry!");
			return false;
		}
		if (entry.m_affectedCards[index] != null)
		{
			return entity == entry.m_affectedCards[index].GetOriginalEntity();
		}
		return false;
	}

	private TileEntry GetCurrentHistoryEntry()
	{
		if (m_queuedEntries.Count == 0)
		{
			return null;
		}
		for (int i = m_queuedEntries.Count - 1; i >= 0; i--)
		{
			if (!m_queuedEntries[i].m_complete)
			{
				return m_queuedEntries[i];
			}
		}
		return null;
	}

	private bool IsDeadInLaterHistoryEntry(Entity entity)
	{
		bool isDeadInLaterEntry = false;
		for (int entryIndex = m_queuedEntries.Count - 1; entryIndex >= 0; entryIndex--)
		{
			TileEntry entry = m_queuedEntries[entryIndex];
			if (!entry.m_complete)
			{
				return isDeadInLaterEntry;
			}
			for (int affectedIndex = 0; affectedIndex < entry.m_affectedCards.Count; affectedIndex++)
			{
				HistoryInfo info = entry.m_affectedCards[affectedIndex];
				if (info.GetOriginalEntity() == entity && info.HasDied())
				{
					isDeadInLaterEntry = true;
				}
			}
		}
		return false;
	}

	private void TileLoadedCallback(AssetReference assetRef, GameObject go, object callbackData)
	{
		TileLoadedCallbackData data = (TileLoadedCallbackData)callbackData;
		if (data == null)
		{
			return;
		}
		HistoryInfo sourceInfo = data.m_sourceInfo;
		if (sourceInfo == null)
		{
			return;
		}
		HistoryTileInitInfo initInfo = new HistoryTileInitInfo
		{
			m_type = sourceInfo.m_infoType,
			m_ownerInfo = data.m_sourceOwnerInfo,
			m_childInfos = data.m_childInfos
		};
		if (initInfo.m_type == HistoryInfoType.FATIGUE)
		{
			initInfo.m_fatigueTexture = m_FatigueTexture;
		}
		else if (initInfo.m_type == HistoryInfoType.BURNED_CARDS)
		{
			initInfo.m_burnedCardsTexture = m_BurnedCardsTexture;
		}
		else
		{
			Entity entity = sourceInfo.GetDuplicatedEntity();
			if (entity == null)
			{
				return;
			}
			initInfo.m_cardDef = entity.ShareDisposableCardDef();
			initInfo.m_entity = entity;
			initInfo.m_portraitTexture = DeterminePortraitTextureForTiles(entity, initInfo.m_cardDef.CardDef);
			if (initInfo.m_cardDef != null && initInfo.m_cardDef.CardDef != null)
			{
				int parentId = entity.GetTag(GAME_TAG.PARENT_CARD);
				Entity parent = null;
				if (parentId != 0)
				{
					Entity found = GameState.Get().GetEntity(parentId);
					if (found != null && !found.IsTitan())
					{
						parent = found;
					}
				}
				if (parent != null)
				{
					CardDef cardDef = parent.ShareDisposableCardDef().CardDef;
					initInfo.m_portraitGoldenMaterial = cardDef.GetPremiumPortraitMaterial();
					cardDef.TryGetHistoryTileFullPortrait(parent.GetPremiumType(), out initInfo.m_fullTileMaterial);
					cardDef.TryGetHistoryTileHalfPortrait(parent.GetPremiumType(), out initInfo.m_halfTileMaterial);
				}
				else
				{
					initInfo.m_portraitGoldenMaterial = initInfo.m_cardDef.CardDef.GetPremiumPortraitMaterial();
					initInfo.m_cardDef.CardDef.TryGetHistoryTileFullPortrait(entity.GetPremiumType(), out initInfo.m_fullTileMaterial);
					initInfo.m_cardDef.CardDef.TryGetHistoryTileHalfPortrait(entity.GetPremiumType(), out initInfo.m_halfTileMaterial);
				}
			}
			initInfo.m_splatAmount = sourceInfo.GetSplatAmount();
			initInfo.m_dead = sourceInfo.HasDied();
			initInfo.m_burned = sourceInfo.m_isBurnedCard;
			initInfo.m_isPoisonous = sourceInfo.m_isPoisonous;
			initInfo.m_isCriticalHit = sourceInfo.m_isCriticalHit;
			initInfo.m_splatType = sourceInfo.GetSplatType();
		}
		using (initInfo.m_cardDef)
		{
			HistoryCard historyCard = go.GetComponent<HistoryCard>();
			m_historyTiles.Add(historyCard);
			historyCard.LoadTile(initInfo);
			SetAsideTileAndTryToUpdate(historyCard);
			LoadNextHistoryEntry();
		}
	}

	public Texture DeterminePortraitTextureForTiles(Entity entity, CardDef cardDef)
	{
		Texture portraitTexture = null;
		if (entity.IsHiddenSecret())
		{
			if (entity.GetClass() == TAG_CLASS.PALADIN)
			{
				return m_paladinSecretTexture;
			}
			if (entity.GetClass() == TAG_CLASS.HUNTER)
			{
				return m_hunterSecretTexture;
			}
			if (entity.GetClass() == TAG_CLASS.ROGUE)
			{
				return m_rogueSecretTexture;
			}
			if (entity.IsDarkWandererSecret())
			{
				return m_wandererSecretTexture;
			}
			return m_mageSecretTexture;
		}
		if (entity.IsHiddenForge())
		{
			return m_forgeCardsTexture;
		}
		return cardDef.GetPortraitTexture(entity.GetPremiumType());
	}

	private void CheckForMouseOff()
	{
		if (!(m_currentlyMousedOverTile == null))
		{
			m_currentlyMousedOverTile.NotifyMousedOut();
			m_currentlyMousedOverTile = null;
			m_SoundDucker.StopDucking();
			FadeVignetteOut();
		}
	}

	private void DestroyHistoryTilesThatFallOffTheEnd()
	{
		if (m_historyTiles.Count != 0)
		{
			float lengthOfAllTiles = 0f;
			float maxHistoryLength = GetHistoryCollider().bounds.size.z;
			for (int i = 0; i < m_historyTiles.Count; i++)
			{
				lengthOfAllTiles += m_historyTiles[i].GetTileSize();
			}
			lengthOfAllTiles += 0.15f * (float)(m_historyTiles.Count - 1);
			while (lengthOfAllTiles > maxHistoryLength)
			{
				lengthOfAllTiles -= m_historyTiles[0].GetTileSize();
				lengthOfAllTiles -= 0.15f;
				UnityEngine.Object.Destroy(m_historyTiles[0].gameObject);
				m_historyTiles.RemoveAt(0);
			}
		}
	}

	private void SetAsideTileAndTryToUpdate(HistoryCard tile)
	{
		Vector3 topPosition = GetTopTilePosition();
		tile.transform.position = new Vector3(topPosition.x - 20f, topPosition.y, topPosition.z);
		UpdateLayout();
	}

	private void UpdateHistoryCollider()
	{
		if (AnomalyMedallion.IsShown())
		{
			m_baseHistoryCollider.enabled = false;
			m_shortHistoryCollider.enabled = !m_historyDisabled;
		}
		else
		{
			m_baseHistoryCollider.enabled = !m_historyDisabled;
			m_shortHistoryCollider.enabled = false;
		}
	}

	private BoxCollider GetHistoryCollider()
	{
		UpdateHistoryCollider();
		if (!AnomalyMedallion.IsShown())
		{
			return m_baseHistoryCollider;
		}
		return m_shortHistoryCollider;
	}

	private Vector3 GetTopTilePosition()
	{
		BoxCollider collider = GetHistoryCollider();
		return base.transform.position + collider.center + new Vector3(0f, -0.32f, collider.size.z / 2f / base.transform.localScale.z);
	}

	private bool UserIsMousedOverAHistoryTile()
	{
		if (UniversalInputManager.Get().IsTouchMode() && !InputCollection.GetMouseButton(0))
		{
			return false;
		}
		if (UniversalInputManager.Get().GetInputHitInfo(GameLayer.Default.LayerBit(), out var hitInfo) && hitInfo.transform.GetComponentInChildren<HistoryManager>() == null && hitInfo.transform.GetComponentInChildren<HistoryCard>() == null)
		{
			return false;
		}
		float zPosition = hitInfo.point.z;
		float lowestBottom = 1000f;
		float highestTop = -1000f;
		foreach (HistoryCard card in m_historyTiles)
		{
			if (!card.HasBeenShown())
			{
				continue;
			}
			Collider tileCollider = card.GetTileCollider();
			if (!(tileCollider == null))
			{
				float bottom = tileCollider.bounds.center.z - tileCollider.bounds.extents.z;
				float top = tileCollider.bounds.center.z + tileCollider.bounds.extents.z;
				if (bottom < lowestBottom)
				{
					lowestBottom = bottom;
				}
				if (top > highestTop)
				{
					highestTop = top;
				}
			}
		}
		if (zPosition < lowestBottom || zPosition > highestTop)
		{
			return false;
		}
		return true;
	}

	private void FadeVignetteIn()
	{
		foreach (HistoryCard card in m_historyTiles)
		{
			if (!(card.m_tileActor == null))
			{
				LayerUtils.SetLayer(card.m_tileActor.gameObject, GameLayer.IgnoreFullScreenEffects);
			}
		}
		LayerUtils.SetLayer(base.gameObject, GameLayer.IgnoreFullScreenEffects);
		AnimateVignetteIn();
	}

	private void FadeVignetteOut()
	{
		foreach (HistoryCard card in m_historyTiles)
		{
			if (!(card.m_tileActor == null))
			{
				LayerUtils.SetLayer(card.GetTileCollider().gameObject, GameLayer.Default);
			}
		}
		LayerUtils.SetLayer(base.gameObject, GameLayer.CardRaycast);
		AnimateVignetteOut();
	}

	protected override void OnFullScreenEffectOutFinished()
	{
		foreach (HistoryCard card in m_historyTiles)
		{
			if (!(card.m_tileActor == null))
			{
				LayerUtils.SetLayer(card.m_tileActor.gameObject, GameLayer.Default);
			}
		}
	}

	public bool IsShowingBigCard()
	{
		return m_showingBigCard;
	}

	public bool HasBigCard()
	{
		return m_currentBigCard != null;
	}

	public HistoryCard GetCurrentBigCard()
	{
		return m_currentBigCard;
	}

	public Entity GetPendingBigCardEntity()
	{
		if (m_pendingBigCardEntry == null)
		{
			return null;
		}
		return m_pendingBigCardEntry.m_info.GetOriginalEntity();
	}

	public void CreateFastBigCardFromMetaData(Entity entity)
	{
		int displayTimeMS = 1000;
		CreatePlayedBigCard(entity, delegate
		{
		}, delegate
		{
		}, fromMetaData: true, countered: false, displayTimeMS);
	}

	public void CreatePlayedBigCard(Entity entity, BigCardStartedCallback startedCallback, BigCardFinishedCallback finishedCallback, bool fromMetaData, bool countered, int displayTimeMS)
	{
		if (!GameState.Get().GetGameEntity().ShouldShowBigCard())
		{
			finishedCallback();
			return;
		}
		m_showingBigCard = true;
		StopCoroutine("WaitForCardLoadedAndCreateBigCard");
		BigCardEntry bigCardEntry = new BigCardEntry();
		bigCardEntry.m_info = new HistoryInfo();
		bigCardEntry.m_info.SetOriginalEntity(entity);
		if (entity.IsWeapon())
		{
			bigCardEntry.m_info.m_infoType = HistoryInfoType.WEAPON_PLAYED;
		}
		else
		{
			bigCardEntry.m_info.m_infoType = HistoryInfoType.CARD_PLAYED;
		}
		bigCardEntry.m_startedCallback = startedCallback;
		bigCardEntry.m_finishedCallback = finishedCallback;
		bigCardEntry.m_fromMetaData = fromMetaData;
		bigCardEntry.m_countered = countered;
		bigCardEntry.m_displayTimeMS = displayTimeMS;
		StartCoroutine("WaitForCardLoadedAndCreateBigCard", bigCardEntry);
	}

	public void CreateTriggeredBigCard(Entity entity, BigCardStartedCallback startedCallback, BigCardFinishedCallback finishedCallback, bool fromMetaData, bool isSecret)
	{
		if (!GameState.Get().GetGameEntity().ShouldShowBigCard() || entity.IsBobQuest())
		{
			finishedCallback();
			return;
		}
		m_showingBigCard = true;
		StopCoroutine("WaitForCardLoadedAndCreateBigCard");
		BigCardEntry bigCardEntry = new BigCardEntry();
		bigCardEntry.m_info = new HistoryInfo();
		bigCardEntry.m_info.SetOriginalEntity(entity);
		bigCardEntry.m_info.m_infoType = HistoryInfoType.TRIGGER;
		bigCardEntry.m_fromMetaData = fromMetaData;
		bigCardEntry.m_startedCallback = startedCallback;
		bigCardEntry.m_finishedCallback = finishedCallback;
		bigCardEntry.m_waitForSecretSpell = isSecret;
		StartCoroutine("WaitForCardLoadedAndCreateBigCard", bigCardEntry);
	}

	public void NotifyOfSecretSpellFinished()
	{
		m_bigCardWaitingForSecret = false;
	}

	public void NotifyOfLettuceSpeedTileSpellFinished()
	{
		m_bigCardWaitingForLettuceSpeedTile = false;
	}

	public void HandleClickOnBigCard(HistoryCard card)
	{
		if (m_currentBigCard != null && m_currentBigCard == card)
		{
			OnCurrentBigCardClicked();
		}
	}

	public string GetBigCardBoneName()
	{
		string boneName = "BigCardPosition";
		if ((bool)UniversalInputManager.UsePhoneUI)
		{
			boneName += "_phone";
		}
		return boneName;
	}

	private IEnumerator WaitForBoardLoadedAndSetPaths()
	{
		while (ZoneMgr.Get() == null)
		{
			yield return null;
		}
		while (Gameplay.Get()?.GetBoardLayout() == null)
		{
			yield return null;
		}
		Transform bigCardPathPoint = Board.Get()?.FindBone("BigCardPathPoint");
		if (!(bigCardPathPoint == null))
		{
			Vector3 bigCardPosition = GetBigCardPosition();
			m_bigCardPath = new Vector3[3];
			m_bigCardPath[1] = bigCardPathPoint.position;
			m_bigCardPath[2] = bigCardPosition;
			m_lettuceAbilityBigCardPath = new Vector3[2];
			m_lettuceAbilityBigCardPath[1] = bigCardPosition;
		}
	}

	private Vector3 GetBigCardPosition()
	{
		if (PlatformSettings.IsTablet)
		{
			Transform tabletTransform = Board.Get().FindBone("BigCardPosition_tablet");
			if (tabletTransform != null)
			{
				return tabletTransform.position;
			}
		}
		return Board.Get().FindBone(GetBigCardBoneName()).position;
	}

	private IEnumerator WaitForCardLoadedAndCreateBigCard(BigCardEntry bigCardEntry)
	{
		m_pendingBigCardEntry = bigCardEntry;
		HistoryInfo info = bigCardEntry.m_info;
		while (!info.CanDuplicateEntity(duplicateHiddenNonSecret: false))
		{
			yield return null;
		}
		bigCardEntry.m_startedCallback();
		info.DuplicateEntity(duplicateHiddenNonSecret: false, isEndOfHistory: false);
		m_pendingBigCardEntry = null;
		AssetLoader.Get().InstantiatePrefab("HistoryCard.prefab:f8193c3e146b62342b8fb2c0494ec447", BigCardLoadedCallback, bigCardEntry, AssetLoadingOptions.IgnorePrefabPosition);
	}

	private void BigCardLoadedCallback(AssetReference assetRef, GameObject go, object callbackData)
	{
		BigCardEntry entry = (BigCardEntry)callbackData;
		Entity entity = entry.m_info.GetDuplicatedEntity();
		Card card = entity.GetCard();
		DefLoader.DisposableCardDef cardDef = card.ShareDisposableCardDef();
		if (entity.IsLettuceAbility())
		{
			Card abilityOwnerCard = entity.GetLettuceAbilityOwner()?.GetCard();
			if (abilityOwnerCard != null)
			{
				go.transform.position = abilityOwnerCard.transform.position;
				go.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
			}
			else
			{
				go.transform.position = GetBigCardPosition();
			}
		}
		else if (entity.IsSpell() || entity.IsHeroPower() || entry.m_fromMetaData)
		{
			go.transform.position = card.transform.position;
			go.transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);
		}
		else
		{
			go.transform.position = GetBigCardPosition();
		}
		Entity preTransformedEntity = CreatePreTransformedEntity(entity);
		Entity postTransformedEntity = null;
		if (preTransformedEntity != null)
		{
			postTransformedEntity = entity;
			entity = preTransformedEntity;
			card = entity.GetCard();
			cardDef?.Dispose();
			cardDef = card.ShareDisposableCardDef();
		}
		Entity secretDeathrattleEntity = CreateSecretDeathrattleEntity(entity);
		if (secretDeathrattleEntity != null)
		{
			entity = secretDeathrattleEntity;
			card = entity.GetCard();
			cardDef?.Dispose();
			cardDef = card.ShareDisposableCardDef();
		}
		using (cardDef)
		{
			HistoryBigCardInitInfo initInfo = new HistoryBigCardInitInfo();
			initInfo.m_historyInfoType = entry.m_info.m_infoType;
			initInfo.m_entity = entity;
			initInfo.m_portraitTexture = cardDef.CardDef.GetPortraitTexture(entity.GetPremiumType());
			initInfo.m_portraitGoldenMaterial = cardDef.CardDef.GetPremiumPortraitMaterial();
			initInfo.m_cardDef = cardDef;
			initInfo.m_finishedCallback = entry.m_finishedCallback;
			initInfo.m_countered = entry.m_countered;
			initInfo.m_waitForSecretSpell = entry.m_waitForSecretSpell;
			initInfo.m_fromMetaData = entry.m_fromMetaData;
			initInfo.m_postTransformedEntity = postTransformedEntity;
			initInfo.m_displayTimeMS = entry.m_displayTimeMS;
			HistoryCard historyCard = go.GetComponent<HistoryCard>();
			historyCard.LoadBigCard(initInfo);
			if ((bool)m_currentBigCard)
			{
				InterruptCurrentBigCard();
			}
			m_currentBigCard = historyCard;
			StartCoroutine("WaitThenShowBigCard");
		}
	}

	private IEnumerator WaitThenShowBigCard()
	{
		if (m_currentBigCard.IsBigCardWaitingForSecret())
		{
			m_bigCardWaitingForSecret = true;
			m_currentBigCard.transform.localScale = new Vector3(0.001f, 0.001f, 0.001f);
			while (m_bigCardWaitingForSecret)
			{
				yield return null;
			}
			if (m_currentBigCard.HasBigCardPostTransformedEntity())
			{
				m_bigCardTransformState = BigCardTransformState.PRE_TRANSFORM;
			}
			m_currentBigCard.ShowBigCard(m_bigCardPath);
			StartCoroutine("WaitThenDestroyBigCard");
			if (m_currentBigCard.HasBigCardPostTransformedEntity())
			{
				while (m_bigCardTransformState == BigCardTransformState.PRE_TRANSFORM || m_bigCardTransformState == BigCardTransformState.TRANSFORM)
				{
					yield return null;
				}
			}
		}
		else if (m_currentBigCard.HasBigCardPostTransformedEntity())
		{
			m_bigCardTransformState = BigCardTransformState.PRE_TRANSFORM;
			m_currentBigCard.ShowBigCard(m_bigCardPath);
			StartCoroutine("WaitThenDestroyBigCard");
			while (m_bigCardTransformState == BigCardTransformState.PRE_TRANSFORM || m_bigCardTransformState == BigCardTransformState.TRANSFORM)
			{
				yield return null;
			}
		}
		else if (m_currentBigCard.IsCastedByLettuceCharacter())
		{
			if (!m_currentBigCard.IsBigCardFromMetaData())
			{
				m_bigCardWaitingForLettuceSpeedTile = true;
				while (m_bigCardWaitingForLettuceSpeedTile)
				{
					yield return null;
				}
			}
			m_currentBigCard.ShowBigCard(m_lettuceAbilityBigCardPath);
			StartCoroutine("WaitThenDestroyBigCard");
		}
		else
		{
			m_currentBigCard.ShowBigCard(m_bigCardPath);
			StartCoroutine("WaitThenDestroyBigCard");
		}
		Entity bigCardEntity = m_currentBigCard.GetEntity();
		if (bigCardEntity.HasSubCards() && !bigCardEntity.IsLettuceAbility() && !bigCardEntity.IsStarship())
		{
			Network.HistBlockStart blockStart = GameState.Get().GetPowerProcessor().GetHistoryBlockingTaskList()?.GetBlockStart();
			if (blockStart.SubOption != -1)
			{
				Card subOptionCard = bigCardEntity.GetCard();
				if (!bigCardEntity.IsTitan() || bigCardEntity.IsControlledByFriendlySidePlayer())
				{
					ChoiceCardMgr.Get().ShowSubOptions(subOptionCard);
					StartCoroutine(FinishSpectatorSubOption(bigCardEntity, blockStart.SubOption));
				}
			}
		}
		if (!GameState.Get().GetBooleanGameOption(GameEntityOption.DISABLE_DELAY_BETWEEN_BIG_CARD_DISPLAY_AND_POWER_PROCESSING))
		{
			yield return new WaitForSeconds(1f);
		}
		m_currentBigCard.RunBigCardFinishedCallback();
	}

	private IEnumerator FinishSpectatorSubOption(Entity mainEntity, int chosenSubOption)
	{
		while (ChoiceCardMgr.Get().IsWaitingToShowSubOptions())
		{
			yield return null;
			if (ChoiceCardMgr.Get() == null || !ChoiceCardMgr.Get().HasSubOption())
			{
				yield break;
			}
		}
		List<Card> actualChoiceCards = ChoiceCardMgr.Get().GetFriendlyCards();
		List<Card> choiceCards;
		if (actualChoiceCards == null)
		{
			Log.All.PrintError("actualChoiceCards is NULL. Attempting workaround.");
			choiceCards = new List<Card>();
		}
		else
		{
			choiceCards = new List<Card>(actualChoiceCards);
		}
		Card subCard = ((chosenSubOption < choiceCards.Count) ? choiceCards[chosenSubOption] : null);
		Entity subEntity = (subCard ? subCard.GetEntity() : null);
		if (subCard != null)
		{
			subCard.SetInputEnabled(enabled: false);
		}
		yield return new WaitForSeconds(1f);
		if (subCard != null)
		{
			subCard.SetInputEnabled(enabled: true);
		}
		GameState state = GameState.Get();
		if (state == null || state.IsGameOver())
		{
			foreach (Card item in choiceCards)
			{
				item.HideCard();
			}
			yield break;
		}
		InputManager.Get().HandleClickOnSubOption(subEntity, isSimulated: true);
	}

	private IEnumerator WaitThenDestroyBigCard()
	{
		float timeToWait = (float)m_currentBigCard.GetDisplayTimeMS() / 1000f;
		if (timeToWait <= 0f)
		{
			if (m_currentBigCard.IsBigCardFromMetaData())
			{
				timeToWait = 1.5f;
			}
			else
			{
				timeToWait = ((m_currentBigCard.GetEntity() == null) ? 4f : (m_currentBigCard.GetEntity().GetCardType() switch
				{
					TAG_CARDTYPE.SPELL => 4f + GameState.Get().GetGameEntity().GetAdditionalTimeToWaitForSpells(), 
					TAG_CARDTYPE.HERO_POWER => 4f + GameState.Get().GetGameEntity().GetAdditionalTimeToWaitForSpells(), 
					TAG_CARDTYPE.LETTUCE_ABILITY => 1.65f, 
					_ => 3f, 
				}));
				if (m_currentBigCard.HasBigCardPostTransformedEntity())
				{
					timeToWait *= 0.5f;
				}
			}
		}
		yield return new WaitForSeconds(timeToWait);
		DestroyBigCard();
	}

	private void DestroyBigCard()
	{
		if (!(m_currentBigCard == null))
		{
			if (m_currentBigCard.m_mainCardActor == null)
			{
				RunFinishedCallbackAndDestroyBigCard();
			}
			else if (m_currentBigCard.HasBigCardPostTransformedEntity())
			{
				PlayBigCardTransformEffects();
			}
			else if (m_currentBigCard.WasBigCardCountered())
			{
				PlayBigCardCounteredEffects();
			}
			else
			{
				RunFinishedCallbackAndDestroyBigCard();
			}
		}
	}

	private void RunFinishedCallbackAndDestroyBigCard()
	{
		if (!(m_currentBigCard == null))
		{
			m_currentBigCard.RunBigCardFinishedCallback();
			m_showingBigCard = false;
			UnityEngine.Object.Destroy(m_currentBigCard.gameObject);
			TooltipPanelManager.Get().HideTooltipPanels();
		}
	}

	private void PlayBigCardCounteredEffects()
	{
		ISpellCallbackHandler<Spell>.StateFinishedCallback stateFinishedCallback = delegate(Spell s, SpellStateType prevStateType, object userData)
		{
			if (s.GetActiveState() == SpellStateType.NONE)
			{
				HistoryCard obj = (HistoryCard)userData;
				m_showingBigCard = false;
				UnityEngine.Object.Destroy(obj.gameObject);
			}
		};
		Spell spell = m_currentBigCard.m_mainCardActor.GetSpell(SpellType.DEATH);
		if (spell == null)
		{
			RunFinishedCallbackAndDestroyBigCard();
			return;
		}
		spell.AddStateFinishedCallback(stateFinishedCallback, m_currentBigCard);
		m_currentBigCard.RunBigCardFinishedCallback();
		m_currentBigCard = null;
		spell.Activate();
	}

	private void PlayBigCardTransformEffects()
	{
		StartCoroutine("PlayBigCardTransformEffectsWithTiming");
	}

	private IEnumerator PlayBigCardTransformEffectsWithTiming()
	{
		if (m_bigCardTransformState == BigCardTransformState.INVALID)
		{
			RunFinishedCallbackAndDestroyBigCard();
			yield break;
		}
		if (m_bigCardTransformState == BigCardTransformState.PRE_TRANSFORM)
		{
			m_bigCardTransformState = BigCardTransformState.TRANSFORM;
			yield return StartCoroutine(PlayBigCardTransformSpell());
		}
		if (m_bigCardTransformState == BigCardTransformState.TRANSFORM)
		{
			m_bigCardTransformState = BigCardTransformState.POST_TRANSFORM;
			yield return StartCoroutine(WaitForBigCardPostTransform());
		}
		if (m_bigCardTransformState == BigCardTransformState.POST_TRANSFORM)
		{
			m_bigCardTransformState = BigCardTransformState.INVALID;
			RunFinishedCallbackAndDestroyBigCard();
		}
	}

	private IEnumerator PlayBigCardTransformSpell()
	{
		if (m_TransformSpells == null || m_TransformSpells.Length == 0)
		{
			yield break;
		}
		Entity entity = m_currentBigCard.GetEntity();
		int transformSpellIndex = entity.GetTag(GAME_TAG.TRANSFORMED_FROM_CARD_VISUAL_TYPE);
		if (transformSpellIndex < 0 || transformSpellIndex >= m_TransformSpells.Length)
		{
			transformSpellIndex = 0;
		}
		m_bigCardTransformSpell = SpellManager.Get().GetSpell(m_TransformSpells[transformSpellIndex]);
		if (m_bigCardTransformSpell == null)
		{
			yield break;
		}
		Card card = entity.GetCard();
		m_bigCardTransformSpell.SetSource(card.gameObject);
		m_bigCardTransformSpell.AddTarget(card.gameObject);
		m_bigCardTransformSpell.SetParentToLocation = true;
		m_bigCardTransformSpell.UpdateTransform();
		m_bigCardTransformSpell.SetPosition(m_currentBigCard.m_mainCardActor.transform.position);
		ISpellCallbackHandler<Spell>.StateFinishedCallback stateFinishedCallback = delegate(Spell s, SpellStateType prevStateType, object userData)
		{
			if (s.GetActiveState() == SpellStateType.NONE)
			{
				SpellManager.Get().ReleaseSpell(s);
			}
		};
		m_bigCardTransformSpell.AddStateFinishedCallback(stateFinishedCallback);
		m_bigCardTransformSpell.Activate();
		while ((bool)m_bigCardTransformSpell && !m_bigCardTransformSpell.IsFinished())
		{
			yield return null;
		}
	}

	private IEnumerator WaitForBigCardPostTransform()
	{
		Actor preTransformedActor = m_currentBigCard.m_mainCardActor;
		preTransformedActor.Hide(ignoreSpells: true);
		m_currentBigCard.LoadBigCardPostTransformedEntity();
		TransformUtil.CopyLocal(m_currentBigCard.m_mainCardActor, preTransformedActor);
		yield return new WaitForSeconds(2f);
	}

	private void OnCurrentBigCardClicked()
	{
		if (m_currentBigCard.HasBigCardPostTransformedEntity())
		{
			ForceNextBigCardTransformState();
		}
		else
		{
			InterruptCurrentBigCard();
		}
	}

	private void ForceNextBigCardTransformState()
	{
		switch (m_bigCardTransformState)
		{
		case BigCardTransformState.PRE_TRANSFORM:
			m_bigCardTransformState = BigCardTransformState.TRANSFORM;
			StopWaitingThenDestroyBigCard();
			break;
		case BigCardTransformState.TRANSFORM:
			if ((bool)m_bigCardTransformSpell)
			{
				UnityEngine.Object.Destroy(m_bigCardTransformSpell.gameObject);
			}
			break;
		case BigCardTransformState.POST_TRANSFORM:
			InterruptCurrentBigCard();
			break;
		}
	}

	private void StopWaitingThenDestroyBigCard()
	{
		StopCoroutine("WaitThenDestroyBigCard");
		DestroyBigCard();
	}

	private void InterruptCurrentBigCard()
	{
		StopCoroutine("WaitThenShowBigCard");
		if (m_currentBigCard.HasBigCardPostTransformedEntity())
		{
			CutoffBigCardTransformEffects();
		}
		else
		{
			StopWaitingThenDestroyBigCard();
		}
	}

	private void CutoffBigCardTransformEffects()
	{
		if ((bool)m_bigCardTransformSpell)
		{
			UnityEngine.Object.Destroy(m_bigCardTransformSpell.gameObject);
		}
		StopCoroutine("PlayBigCardTransformEffectsWithTiming");
		m_bigCardTransformState = BigCardTransformState.INVALID;
		RunFinishedCallbackAndDestroyBigCard();
	}
}
