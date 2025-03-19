using System.Collections.Generic;
using Assets;
using PegasusShared;
using UnityEngine;

public class LettuceMercenary
{
	public class ArtVariation
	{
		public readonly MercenaryArtVariationDbfRecord m_record;

		public readonly TAG_PREMIUM m_premium;

		public readonly bool m_default;

		public bool m_acknowledged;

		public ArtVariation(MercenaryArtVariationDbfRecord record, TAG_PREMIUM premium, bool isDefault, bool acknowledged = true)
		{
			m_record = record;
			m_premium = premium;
			m_default = isDefault;
			m_acknowledged = acknowledged;
		}
	}

	public class Loadout
	{
		public LettuceEquipmentDbfRecord m_equipmentRecord;

		public MercenaryArtVariationDbfRecord m_artVariationRecord;

		public TAG_PREMIUM m_artVariationPremium;

		private bool m_dirty;

		public Loadout()
		{
		}

		public Loadout(Loadout src)
		{
			if (src != null)
			{
				m_equipmentRecord = src.m_equipmentRecord;
				m_artVariationRecord = src.m_artVariationRecord;
				m_artVariationPremium = src.m_artVariationPremium;
			}
		}

		public void SetArtVariation(MercenaryArtVariationDbfRecord record, TAG_PREMIUM premium, bool markDirty = false)
		{
			if (m_artVariationRecord != record || m_artVariationPremium != premium)
			{
				m_artVariationRecord = record;
				m_artVariationPremium = premium;
				m_dirty |= markDirty;
			}
		}

		public bool SetSlottedEquipment(LettuceEquipmentDbfRecord record, bool markDirty = false)
		{
			if (m_equipmentRecord != record)
			{
				m_equipmentRecord = record;
				m_dirty |= markDirty;
				return true;
			}
			return false;
		}

		public bool IsValid()
		{
			if (m_artVariationRecord == null)
			{
				return false;
			}
			return true;
		}

		public string GetCardId()
		{
			return m_artVariationRecord.CardRecord.NoteMiniGuid;
		}

		public bool IsDirty()
		{
			return m_dirty;
		}

		public void ClearDirty()
		{
			m_dirty = false;
		}
	}

	public int ID;

	public long m_experience;

	public int m_level = 1;

	public bool m_isFullyUpgraded;

	public int m_attack;

	public int m_health;

	public long m_currencyAmount;

	public string m_mercName;

	public string m_mercShortName;

	public bool m_owned;

	public TAG_ROLE m_role;

	public TAG_RARITY m_rarity;

	public TAG_PREMIUM m_premium;

	public TAG_ACQUIRE_TYPE m_acquireType;

	public string m_customAcquireText;

	public bool m_equipmentSelectionChanged;

	public List<string> m_abilitySpecializations = new List<string>();

	public List<LettuceAbility> m_abilityList = new List<LettuceAbility>();

	public List<LettuceAbility> m_equipmentList = new List<LettuceAbility>();

	public List<ArtVariation> m_artVariations = new List<ArtVariation>();

	private Loadout m_loadout = new Loadout();

	public Date m_trainingStartDate;

	public static MercenaryArtVariationDbfRecord GetDefaultArtVariationRecord(int mercId)
	{
		if (GameDbf.LettuceMercenary.GetRecord(mercId) != null)
		{
			foreach (MercenaryArtVariationDbfRecord variation in GameDbf.GetIndex().GetMercenaryArtVariationsByMercenaryID(mercId))
			{
				if (variation.DefaultVariation)
				{
					return variation;
				}
			}
		}
		return null;
	}

	public static ArtVariation CreateDefaultArtVariation(int mercId)
	{
		return new ArtVariation(GetDefaultArtVariationRecord(mercId), TAG_PREMIUM.NORMAL, isDefault: true);
	}

	public static int GetPortraitIdFromArtVariation(int artVariationId, TAG_PREMIUM premium)
	{
		MercenaryArtVariationDbfRecord variationRecord = GameDbf.MercenaryArtVariation.GetRecord(artVariationId);
		if (variationRecord != null)
		{
			foreach (MercenaryArtVariationPremiumDbfRecord portraitRecord in variationRecord.MercenaryArtVariationPremiums)
			{
				if (portraitRecord.Premium == (MercenaryArtVariationPremium.MercenariesPremium)premium)
				{
					return portraitRecord.ID;
				}
			}
		}
		return 0;
	}

	public ArtVariation GetDefaultOrFirstAvailableArtVariation()
	{
		foreach (ArtVariation variation in m_artVariations)
		{
			if (variation.m_default)
			{
				return variation;
			}
		}
		if (m_artVariations.Count > 0)
		{
			return m_artVariations[0];
		}
		Debug.LogWarning("GetDefaultOrFirstAvailableArtVariation: Unable to find any art variations on this mercenary, generating the default variation as a fallback");
		return CreateDefaultArtVariation(ID);
	}

	public ArtVariation GetEquippedArtVariation()
	{
		Loadout loadout = GetCurrentLoadout();
		foreach (ArtVariation variation in m_artVariations)
		{
			if (variation.m_record == loadout.m_artVariationRecord && variation.m_premium == loadout.m_artVariationPremium)
			{
				return variation;
			}
		}
		return GetDefaultOrFirstAvailableArtVariation();
	}

	public ArtVariation GetOwnedArtVariation(int ArtVariationId, TAG_PREMIUM premium)
	{
		ArtVariation requestedVariation = null;
		foreach (ArtVariation variation in m_artVariations)
		{
			if (variation.m_record.ID == ArtVariationId && variation.m_premium == premium)
			{
				requestedVariation = variation;
			}
		}
		if (requestedVariation == null)
		{
			requestedVariation = GetDefaultOrFirstAvailableArtVariation();
		}
		return requestedVariation;
	}

	public void SetEquippedArtVariation(int ArtVariationId, TAG_PREMIUM premium)
	{
		ArtVariation requestedVariation = GetOwnedArtVariation(ArtVariationId, premium);
		m_loadout.SetArtVariation(requestedVariation.m_record, premium, markDirty: true);
		CollectionManager.Get().GetEditingTeam()?.GetLoadout(this)?.SetArtVariation(requestedVariation.m_record, premium, markDirty: true);
	}

	public bool IsArtVariationUnlocked(int ArtVariationId, TAG_PREMIUM premium)
	{
		foreach (ArtVariation variation in m_artVariations)
		{
			if (variation.m_record.ID == ArtVariationId && variation.m_premium == premium)
			{
				return true;
			}
		}
		return false;
	}

	public bool IsArtVariationNew(int ArtVariationId, TAG_PREMIUM premium)
	{
		foreach (ArtVariation variation in m_artVariations)
		{
			if (variation.m_record.ID == ArtVariationId && variation.m_premium == premium && !variation.m_acknowledged)
			{
				return true;
			}
		}
		return false;
	}

	public static List<MercenaryArtVariationDbfRecord> GetArtVariations(int mercId)
	{
		return GameDbf.GetIndex().GetMercenaryArtVariationsByMercenaryID(mercId);
	}

	public bool HasUnlockedGoldenOrBetter()
	{
		foreach (ArtVariation artVariation in m_artVariations)
		{
			if (artVariation.m_premium >= TAG_PREMIUM.GOLDEN)
			{
				return true;
			}
		}
		return false;
	}

	public int GetMythicModifier()
	{
		if (!m_isFullyUpgraded)
		{
			return 0;
		}
		if (GameDbf.LettuceMercenary.GetRecord(ID) == null)
		{
			Debug.LogWarning($"GetMythicLevel: Unable to load mercenary record {ID}");
			return 0;
		}
		int mythicModifier = 0;
		foreach (LettuceAbility ability in m_abilityList)
		{
			mythicModifier += ability.m_mythicModifier;
		}
		foreach (LettuceAbility equipment in m_equipmentList)
		{
			mythicModifier += equipment.m_mythicModifier;
		}
		NetCache.NetCacheMercenariesMythicTreasureInfo netcacheTreasureInfo = NetCache.Get().GetNetObject<NetCache.NetCacheMercenariesMythicTreasureInfo>();
		if (netcacheTreasureInfo != null)
		{
			foreach (MercenaryAllowedTreasureDbfRecord allowedTreasureRecord in GameDbf.GetIndex().GetMercenaryTreasureByMercenaryID(ID))
			{
				if (netcacheTreasureInfo.MythicTreasureScalarMap.TryGetValue(allowedTreasureRecord.TreasureId, out var treasureScalar))
				{
					mythicModifier += treasureScalar.Scalar;
				}
			}
		}
		return mythicModifier;
	}

	public Loadout GetBaseLoadout()
	{
		return m_loadout;
	}

	public Loadout GetTeamLoadout(LettuceTeam team)
	{
		if (team != null)
		{
			Loadout teamLoadout = team.GetLoadout(this);
			if (teamLoadout != null)
			{
				return teamLoadout;
			}
		}
		return GetBaseLoadout();
	}

	public Loadout GetCurrentLoadout()
	{
		LettuceTeam team = CollectionManager.Get().GetEditingTeam();
		return GetTeamLoadout(team);
	}

	public CardDbfRecord GetCardRecord()
	{
		return GetEquippedArtVariation().m_record.CardRecord;
	}

	public string GetCardId()
	{
		return GetCardRecord().NoteMiniGuid;
	}

	public CollectibleCard GetCollectibleCard()
	{
		return CollectionManager.Get().GetCard(GetCardId(), TAG_PREMIUM.NORMAL);
	}

	public LettuceAbility GetLettuceAbility(int abilityId)
	{
		int abilityIndex = GetAbilityIndex(abilityId);
		if (abilityIndex >= 0)
		{
			return m_abilityList[abilityIndex];
		}
		Log.Lettuce.PrintWarning("No ability found on mercenary {0} with ability Id {1}.", ID, abilityId);
		return null;
	}

	public LettuceAbility GetLettuceEquipment(int equipmentId)
	{
		int equipmentIndex = GetEquipmentIndex(equipmentId);
		if (equipmentIndex >= 0)
		{
			return m_equipmentList[equipmentIndex];
		}
		Log.Lettuce.PrintWarning("No equipment found on mercenary {0} with equipment Id {1}.", ID, equipmentId);
		return null;
	}

	public LettuceAbility GetLettuceEquipment(string cardId)
	{
		int equipmentIndex = GetEquipmentIndex(cardId);
		if (equipmentIndex >= 0)
		{
			return m_equipmentList[equipmentIndex];
		}
		Log.Lettuce.PrintWarning("No equipment found on mercenary {0} at equipment index {1}.", ID, equipmentIndex);
		return null;
	}

	public int GetAbilityIndex(int abilityDbId)
	{
		return m_abilityList.FindIndex((LettuceAbility a) => a.ID == abilityDbId);
	}

	public int GetEquipmentIndex(int equipmentDbId)
	{
		return m_equipmentList.FindIndex((LettuceAbility e) => e.ID == equipmentDbId);
	}

	public int GetEquipmentIndex(string cardId)
	{
		return m_equipmentList.FindIndex((LettuceAbility e) => e.ContainsCardId(cardId));
	}

	public bool IsEquipmentSlotUnassigned()
	{
		return GetCurrentLoadout().m_equipmentRecord == null;
	}

	public LettuceAbility GetSlottedEquipment()
	{
		if (IsEquipmentSlotUnassigned())
		{
			return null;
		}
		return m_equipmentList[GetEquipmentIndex(GetCurrentLoadout().m_equipmentRecord.ID)];
	}

	public bool CanSlotEquipment(int equipmentId)
	{
		LettuceAbility equip = GetLettuceEquipment(equipmentId);
		if (equip == null)
		{
			Log.Lettuce.PrintWarning($"LettuceMercenary.CanSlotEquipment: Equipment ID {equipmentId} is not in Equipment list for Mercenary {ID}");
			return false;
		}
		return equip.Owned;
	}

	public bool SlotEquipment(int equipmentId)
	{
		bool slotted = false;
		if (!CanSlotEquipment(equipmentId))
		{
			return false;
		}
		LettuceEquipmentDbfRecord record = GameDbf.LettuceEquipment.GetRecord(equipmentId);
		if (m_loadout.SetSlottedEquipment(record, markDirty: true))
		{
			m_equipmentSelectionChanged = true;
			slotted = true;
		}
		LettuceTeam team = CollectionManager.Get().GetEditingTeam();
		if (team != null)
		{
			Loadout teamLoadout = team.GetLoadout(this);
			if (teamLoadout != null && teamLoadout.SetSlottedEquipment(record, markDirty: true))
			{
				m_equipmentSelectionChanged = true;
				slotted = true;
			}
		}
		return slotted;
	}

	public bool CanUnslotEquipment(int equipmentId)
	{
		if (GetLettuceEquipment(equipmentId) == null)
		{
			Log.Lettuce.PrintWarning($"LettuceMercenary.UnslotEquipment: Equipment ID {equipmentId} is not in Equipment list for Mercenary {ID}");
			return false;
		}
		if (GetCurrentLoadout().m_equipmentRecord.ID != equipmentId)
		{
			return false;
		}
		return true;
	}

	public bool UnslotEquipment(int equipmentId)
	{
		if (!CanUnslotEquipment(equipmentId))
		{
			return false;
		}
		m_loadout.SetSlottedEquipment(null, markDirty: true);
		CollectionManager.Get().GetEditingTeam()?.GetLoadout(this)?.SetSlottedEquipment(null, markDirty: true);
		return true;
	}

	public void SetExperience(long experience)
	{
		m_experience = experience;
		_ = m_level;
		m_level = GameUtils.GetMercenaryLevelFromExperience((int)experience);
		GetCurrentMercStats(out m_attack, out m_health);
	}

	public bool IsAcquiredByCrafting()
	{
		return GameDbf.LettuceMercenary.GetRecord(ID).Craftable;
	}

	public int GetCraftingCost()
	{
		return GameDbf.LettuceMercenary.GetRecord(ID).CoinCraftCost;
	}

	public bool IsReadyForCrafting()
	{
		if (IsAcquiredByCrafting() && !m_owned)
		{
			return m_currencyAmount >= GetCraftingCost();
		}
		return false;
	}

	public bool CanAnyAbilityBeUpgraded()
	{
		foreach (LettuceAbility ability in m_abilityList)
		{
			if (IsLettuceAbilityUpgradeable(ability))
			{
				return true;
			}
		}
		return false;
	}

	public bool CanAnyCardBeUpgraded()
	{
		if (CanAnyAbilityBeUpgraded())
		{
			return true;
		}
		foreach (LettuceAbility equipment in m_equipmentList)
		{
			if (equipment.Owned && IsLettuceAbilityUpgradeable(equipment))
			{
				return true;
			}
		}
		return false;
	}

	public bool IsCardReadyForUpgrade(int abilityId, CollectionUtils.MercenariesModeCardType cardType)
	{
		List<LettuceAbility> cardList = null;
		switch (cardType)
		{
		case CollectionUtils.MercenariesModeCardType.Ability:
			cardList = m_abilityList;
			break;
		case CollectionUtils.MercenariesModeCardType.Equipment:
			cardList = m_equipmentList;
			break;
		}
		int index = cardList.FindIndex((LettuceAbility a) => a.ID == abilityId);
		LettuceAbility ability = ((index >= 0) ? cardList[index] : null);
		if (ability == null)
		{
			Log.Lettuce.PrintWarning("LettuceMercenary.IsCardReadyForUpgrade - Ability type {0} with ID {1} does not belong to mercenary ID {2}", cardType, abilityId, ID);
			return false;
		}
		return IsLettuceAbilityUpgradeable(ability);
	}

	public bool IsCardReadyForUpgrade(LettuceAbility ability)
	{
		List<LettuceAbility> cardList = null;
		switch (ability.m_cardType)
		{
		case CollectionUtils.MercenariesModeCardType.Ability:
			cardList = m_abilityList;
			break;
		case CollectionUtils.MercenariesModeCardType.Equipment:
			cardList = m_equipmentList;
			break;
		default:
			Log.Lettuce.PrintWarning("LettuceMercenary.Unexpected card type: {0}", ability.m_cardType);
			return false;
		}
		if (!cardList.Contains(ability))
		{
			Log.Lettuce.PrintWarning("LettuceMercenary.IsAbilityReadyForUpgrade: Ability ID {0} of type {1} does not belong to Merc ID {2}!", ability.ID, ability.m_cardType, ID);
			return false;
		}
		return IsLettuceAbilityUpgradeable(ability);
	}

	public bool IsMaxLevel()
	{
		return m_level == GameUtils.GetMaxMercenaryLevel();
	}

	public bool IsMythicUpgradable()
	{
		return m_isFullyUpgraded;
	}

	public bool IsAbilityLocked(LettuceAbility ability)
	{
		if (ability.m_cardType != CollectionUtils.MercenariesModeCardType.Equipment)
		{
			return m_level < ability.m_unlockLevel;
		}
		return false;
	}

	public bool FindTextInCard(string searchStr)
	{
		if (GetCollectibleCard().FindTextInCard(searchStr))
		{
			return true;
		}
		foreach (LettuceAbility ability in m_abilityList)
		{
			string cardId = ability.GetCardId();
			if (!string.IsNullOrEmpty(cardId))
			{
				CollectibleCard collectibleCard = CollectionManager.Get().GetCard(cardId, TAG_PREMIUM.NORMAL);
				if (collectibleCard != null && collectibleCard.FindTextInCard(searchStr))
				{
					return true;
				}
			}
		}
		foreach (LettuceAbility equipment in m_equipmentList)
		{
			string cardId2 = equipment.GetCardId();
			if (!string.IsNullOrEmpty(cardId2))
			{
				CollectibleCard collectibleCard = CollectionManager.Get().GetCard(cardId2, TAG_PREMIUM.NORMAL);
				if (collectibleCard != null && collectibleCard.FindTextInCard(searchStr))
				{
					return true;
				}
			}
		}
		if (m_isFullyUpgraded && GameDbf.LettuceMercenary.GetRecord(ID) != null)
		{
			foreach (MercenaryAllowedTreasureDbfRecord item in GameDbf.GetIndex().GetMercenaryTreasureByMercenaryID(ID))
			{
				string cardId3 = GameUtils.TranslateDbIdToCardId((item?.TreasureRecord?.CardId).GetValueOrDefault());
				if (!string.IsNullOrEmpty(cardId3))
				{
					CollectibleCard collectibleCard = CollectionManager.Get().GetCard(cardId3, TAG_PREMIUM.NORMAL);
					if (collectibleCard != null && collectibleCard.FindTextInCard(searchStr))
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	private static int GetMythicRenownCostScaleAssetId(MERCENARY_MYTHIC_UPGRADE_TYPE upgradeType)
	{
		NetCache.NetCacheFeatures features = NetCache.Get()?.GetNetObject<NetCache.NetCacheFeatures>();
		if (features == null)
		{
			Log.Lettuce.PrintWarning("LettuceMercenary.GetMythicRenownCostScaleAssetId: NetCache.NetCacheFeatures was null!");
			return 0;
		}
		return upgradeType switch
		{
			MERCENARY_MYTHIC_UPGRADE_TYPE.ABILITY => features.Mercenaries.MythicAbilityRenownScaleAssetId, 
			MERCENARY_MYTHIC_UPGRADE_TYPE.EQUIPMENT => features.Mercenaries.MythicEquipmentRenownScaleAssetId, 
			MERCENARY_MYTHIC_UPGRADE_TYPE.TREASURE => features.Mercenaries.MythicTreasureRenownScaleAssetId, 
			_ => 0, 
		};
	}

	private static int GetRenownCostOfMythicUpgrade(FormulaDbfRecord scaleRecord, int currentUpgradeCount)
	{
		if (scaleRecord == null)
		{
			Log.Lettuce.PrintWarning("LettuceMercenary.GetRenownCostOfMythicUpgrade: invalid scale record.");
			return int.MaxValue;
		}
		FormulaChangePointDbfRecord lastValidScalePoint = null;
		foreach (FormulaChangePointDbfRecord scalePointRecord in scaleRecord.FormulaChangePoint)
		{
			if (scalePointRecord.Level > currentUpgradeCount)
			{
				break;
			}
			lastValidScalePoint = scalePointRecord;
		}
		if (lastValidScalePoint == null)
		{
			Log.Lettuce.PrintWarning($"LettuceMercenary.GetRenownCostOfMythicUpgrade: couldn't find valid scale point! Scale Id {scaleRecord.ID} Mythic Level {currentUpgradeCount}");
			return int.MaxValue;
		}
		switch (lastValidScalePoint.Function)
		{
		case FormulaChangePoint.Function.CONSTANT:
			return lastValidScalePoint.BaseValue;
		case FormulaChangePoint.Function.LINEAR:
			return lastValidScalePoint.BaseValue + (currentUpgradeCount - lastValidScalePoint.Level) * lastValidScalePoint.Rate;
		default:
			Log.Lettuce.PrintWarning($"LettuceMercenary.GetRenownCostOfMythicUpgrade: invalid cost formula! Scale Id {scaleRecord.ID}, Scale point Id {lastValidScalePoint.ID}, Mythic Level {currentUpgradeCount}");
			return int.MaxValue;
		}
	}

	public static int GetRenownCostOfMythicUpgrade(MERCENARY_MYTHIC_UPGRADE_TYPE upgradeType, int currentMythicLevel, int upgradeCount)
	{
		int scaleId = GetMythicRenownCostScaleAssetId(upgradeType);
		FormulaDbfRecord scaleRecord = GameDbf.Formula.GetRecord(scaleId);
		if (scaleRecord == null)
		{
			Log.Lettuce.PrintWarning($"LettuceMercenary.GetRenownCostOfMythicUpgrade: invalid scale! Scale Id {scaleRecord.ID}");
			return int.MaxValue;
		}
		int totalCost = 0;
		for (int i = 0; i < upgradeCount; i++)
		{
			totalCost += GetRenownCostOfMythicUpgrade(scaleRecord, currentMythicLevel + i);
		}
		return totalCost;
	}

	private bool IsLettuceAbilityUpgradeable(LettuceAbility ability)
	{
		bool requireOwnership = true;
		switch (ability.m_cardType)
		{
		case CollectionUtils.MercenariesModeCardType.Ability:
			requireOwnership = false;
			break;
		case CollectionUtils.MercenariesModeCardType.Equipment:
			requireOwnership = true;
			break;
		default:
			Log.Lettuce.PrintWarning("LettuceMercenary.Unexpected card type: {0}", ability.m_cardType);
			return false;
		}
		if (m_owned && (!requireOwnership || ability.Owned) && !IsAbilityLocked(ability) && ability.m_tier < ability.GetMaxTier())
		{
			return m_currencyAmount >= ability.GetNextUpgradeCost();
		}
		return false;
	}

	public void GetCurrentMercStats(out int attack, out int health)
	{
		CollectionUtils.GetMercenaryStatsByLevel(ID, m_level, m_isFullyUpgraded, out attack, out health);
	}
}
