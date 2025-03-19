using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets;
using Hearthstone.DataModels;
using Hearthstone.Progression;
using Hearthstone.UI;
using PegasusLettuce;
using UnityEngine;

public static class CollectionUtils
{
	public enum ViewMode
	{
		CARDS,
		HERO_SKINS,
		CARD_BACKS,
		DECK_TEMPLATE,
		MASS_DISENCHANT,
		COINS,
		BATTLEGROUNDS_GUIDE_SKINS,
		BATTLEGROUNDS_HERO_SKINS,
		HERO_PICKER,
		BATTLEGROUNDS_BOARD_SKINS,
		BATTLEGROUNDS_FINISHERS,
		BATTLEGROUNDS_EMOTES,
		COUNT
	}

	public enum BattlegroundsHeroSkinFilterMode
	{
		DEFAULT,
		ALL,
		COUNT
	}

	public enum MercenariesModeCardType
	{
		None,
		Mercenary,
		Ability,
		Equipment
	}

	public enum BattlegroundsModeDraggableType
	{
		None,
		CollectionEmote,
		TrayEmote
	}

	public enum ViewSubmode
	{
		DEFAULT,
		CARD_ZILLIAX_MODULES
	}

	public class ViewModeData
	{
		public TAG_CLASS? m_setPageByClass;

		public TAG_ROLE? m_setPageByRole;

		public string m_setPageByCard;

		public TAG_PREMIUM m_setPageByPremium;

		public BookPageManager.DelOnPageTransitionComplete m_pageTransitionCompleteCallback;

		public object m_pageTransitionCompleteData;
	}

	[Serializable]
	public class CollectionPageLayoutSettings
	{
		[Serializable]
		public class Variables
		{
			public ViewMode m_ViewMode;

			public int m_ColumnCount = 4;

			public int m_RowCount = 2;

			public float m_Scale;

			public float m_ColumnSpacing;

			public float m_RowSpacing;

			public Vector3 m_Offset;
		}

		[CustomEditField(ListTable = true)]
		public List<Variables> m_layoutVariables = new List<Variables>();

		public Variables GetVariables(ViewMode mode)
		{
			Variables result = m_layoutVariables.Find((Variables v) => mode == v.m_ViewMode);
			if (result == null)
			{
				return new Variables();
			}
			return result;
		}
	}

	[Flags]
	public enum MercenaryDataPopluateExtra
	{
		None = 0,
		Abilities = 1,
		Coin = 2,
		Appearances = 8,
		UpdateValuesWithSlottedEquipment = 0x10,
		MythicStats = 0x20,
		MythicLoadout = 0x40,
		MythicTreasure = 0x80,
		MythicAll = 0xE0,
		ShowMercCardText = 0x100
	}

	private class SlottedEquipmentBonusData
	{
		public LettuceEquipmentModifierDataDbfRecord BaseMods { get; set; }

		public List<MythicEquipmentScalingCardTagDbfRecord> MythicScalingTagList { get; set; }

		public CardDataModel EquipmentCardModel { get; set; }

		public ModifiedLettuceAbilityValueDbfRecord GetAbilityBaseMods(int AbilityId)
		{
			if (BaseMods != null)
			{
				foreach (ModifiedLettuceAbilityValueDbfRecord abilityMod in BaseMods.ModifiedLettuceAbilityValues)
				{
					if (abilityMod.LettuceAbilityId == AbilityId)
					{
						return abilityMod;
					}
				}
			}
			return null;
		}
	}

	public static bool IsHeroSkinDisplayMode(ViewMode viewMode)
	{
		if (viewMode != ViewMode.HERO_SKINS && viewMode != ViewMode.BATTLEGROUNDS_HERO_SKINS)
		{
			return viewMode == ViewMode.BATTLEGROUNDS_GUIDE_SKINS;
		}
		return true;
	}

	public static void PopulateMercenariesTeamListDataModel(LettuceTeamListDataModel dataModel, bool setAutoSelectedTeam, List<LettuceTeam> teamList = null, bool isRemote = false, bool showLevelInList = false, bool hideInvalidTeams = false, bool showMythicLevel = false)
	{
		if (dataModel == null)
		{
			return;
		}
		DataModelList<LettuceTeamDataModel> teamData = new DataModelList<LettuceTeamDataModel>();
		if (teamList == null)
		{
			teamList = CollectionManager.Get().GetTeams();
		}
		CollectionManager.SortTeams(teamList);
		foreach (LettuceTeam team in teamList)
		{
			LettuceTeamDataModel teamModel = new LettuceTeamDataModel();
			if (!hideInvalidTeams || team.IsValid())
			{
				PopulateMercenariesTeamDataModel(teamModel, team, isRemote, showLevelInList, showMythicLevel);
				teamData.Add(teamModel);
			}
		}
		dataModel.TeamList = teamData;
		if (setAutoSelectedTeam)
		{
			GameSaveDataManager.Get().GetSubkeyValue(GameSaveKeyId.MERCENARIES, GameSaveKeySubkeyId.MERCENARIES_LAST_SELECTED_PVP_TEAM, out long lastSelectedTeamId);
			dataModel.AutoSelectedTeamId = (int)lastSelectedTeamId;
		}
	}

	public static void PopulateMercenariesTeamDataModel(LettuceTeamDataModel teamModel, LettuceTeam team, bool isRemote = false, bool showLevelInList = false, bool showMythicTeamLevel = false, MercenaryDataPopluateExtra extraMercData = MercenaryDataPopluateExtra.None)
	{
		teamModel.TeamId = team.ID;
		teamModel.TeamName = team.Name;
		DataModelList<LettuceMercenaryDataModel> mercList = new DataModelList<LettuceMercenaryDataModel>();
		foreach (LettuceMercenary merc in team.GetMercs())
		{
			mercList.Add(GetMercDataModelForTeam(merc, team, isRemote, showLevelInList, extraMercData));
		}
		teamModel.MercenaryList = mercList;
		teamModel.Valid = team.IsValid();
		teamModel.IsDisabled = team.DoesContainDisabledMerc();
		teamModel.MythicLevel = (showMythicTeamLevel ? team.GetTeamMythicLevel() : (-1));
	}

	private static LettuceMercenaryDataModel GetMercDataModelForTeam(LettuceMercenary merc, LettuceTeam team, bool isRemote, bool showLevelInList, MercenaryDataPopluateExtra extraRequest = MercenaryDataPopluateExtra.None)
	{
		LettuceMercenary.Loadout teamLoadout = team.GetLoadout(merc);
		LettuceMercenaryDataModel mercData = ((!isRemote) ? MercenaryFactory.CreatePopulatedMercenaryDataModel(merc, extraRequest, new LettuceMercenary.ArtVariation(teamLoadout.m_artVariationRecord, teamLoadout.m_artVariationPremium, teamLoadout.m_artVariationRecord.DefaultVariation)) : MercenaryFactory.CreateMercenaryDataModel(merc.ID, teamLoadout.m_artVariationRecord.ID, teamLoadout.m_artVariationPremium, merc));
		mercData.IsRemote = isRemote;
		mercData.ShowLevelInList = showLevelInList;
		return mercData;
	}

	public static float CalculateXPBarFillAmountFromExp(int exp)
	{
		return GameUtils.GetExperiencePercentageFromExperienceValue(exp) % 1f;
	}

	public static void PopulateMercenaryDataModel(LettuceMercenaryDataModel dataModel, LettuceMercenary merc, MercenaryDataPopluateExtra extraRequests, LettuceMercenary.ArtVariation desiredArtVariation = null)
	{
		dataModel.MercenaryId = merc.ID;
		dataModel.MercenaryName = merc.m_mercName;
		dataModel.MercenaryShortName = (string.IsNullOrWhiteSpace(merc.m_mercShortName) ? merc.m_mercName : merc.m_mercShortName);
		dataModel.MercenaryRole = merc.m_role;
		dataModel.MercenaryRarity = merc.m_rarity;
		dataModel.MercenaryLevel = merc.m_level;
		dataModel.IsMaxLevel = merc.m_level >= GameUtils.GetMaxMercenaryLevel();
		dataModel.ReadyForCrafting = merc.IsReadyForCrafting();
		dataModel.ChildUpgradeAvailable = merc.CanAnyCardBeUpgraded();
		dataModel.MercenarySelected = false;
		dataModel.Owned = merc.m_owned;
		dataModel.ExperienceInitial = (int)merc.m_experience;
		dataModel.FullyUpgradedInitial = merc.m_isFullyUpgraded;
		dataModel.CraftingCost = merc.GetCraftingCost();
		dataModel.IsAcquiredByCrafting = merc.IsAcquiredByCrafting();
		dataModel.AcquireType = merc.m_acquireType;
		dataModel.CustomAcquireText = merc.m_customAcquireText;
		dataModel.ShowCustomAcquireText = !string.IsNullOrEmpty(merc.m_customAcquireText);
		dataModel.ShowAbilityText = false;
		dataModel.AbilityText = null;
		dataModel.MythicView = false;
		dataModel.MythicLevel = 0L;
		dataModel.IsAffectedBySlottedEquipment = false;
		dataModel.EquipmentSlotIndex = -1;
		dataModel.ShowLevelInList = true;
		dataModel.ShowAsNew = CollectionManager.Get().DoesMercenaryNeedToBeAcknowledged(merc);
		dataModel.NumNewPortraits = CollectionManager.Get().GetNumNewPortraitsToAcknowledgeForMercenary(merc);
		dataModel.XPBarPercentage = CalculateXPBarFillAmountFromExp((int)merc.m_experience);
		NetCache.NetCacheMercenariesPlayerInfo playerInfo = NetCache.Get()?.GetNetObject<NetCache.NetCacheMercenariesPlayerInfo>();
		if (playerInfo == null)
		{
			Log.Lettuce.PrintError("PopulateMercenaryDataModel - Can't access NetCacheMercenariesPlayerInfo");
		}
		else
		{
			dataModel.IsDisabled = playerInfo.DisabledMercenaryList.Contains(merc.ID);
		}
		if (desiredArtVariation == null)
		{
			desiredArtVariation = merc.GetEquippedArtVariation();
		}
		PopulateMercenaryCardDataModel(dataModel, desiredArtVariation);
		SetMercenaryStatsByLevel(dataModel, merc.ID, merc.m_level, merc.m_isFullyUpgraded);
		if (extraRequests.HasFlag(MercenaryDataPopluateExtra.ShowMercCardText))
		{
			EntityDef entityDef = DefLoader.Get().GetEntityDef(dataModel.Card.CardId);
			if (entityDef != null)
			{
				dataModel.AbilityText = entityDef.GetCardTextInHand();
				if (!string.IsNullOrEmpty(dataModel.AbilityText))
				{
					dataModel.ShowAbilityText = true;
				}
			}
		}
		if (extraRequests.HasFlag(MercenaryDataPopluateExtra.Coin))
		{
			dataModel.MercenaryCoin = new LettuceMercenaryCoinDataModel
			{
				MercenaryId = merc.ID,
				MercenaryName = merc.m_mercName,
				Quantity = (int)merc.m_currencyAmount,
				GlowActive = false
			};
		}
		if (extraRequests.HasFlag(MercenaryDataPopluateExtra.Appearances))
		{
			PopulateMercenaryDataModelArtVariations(dataModel, merc);
		}
		if (extraRequests.HasFlag(MercenaryDataPopluateExtra.Abilities))
		{
			PopulateMercenaryDataModelAbilities(dataModel, merc);
		}
		if ((extraRequests & MercenaryDataPopluateExtra.MythicAll) != 0)
		{
			PopulateMythicBonuses(dataModel, merc, extraRequests);
		}
		if (extraRequests.HasFlag(MercenaryDataPopluateExtra.Abilities) && extraRequests.HasFlag(MercenaryDataPopluateExtra.UpdateValuesWithSlottedEquipment))
		{
			ApplyEquipmentBonuses(dataModel, merc, extraRequests);
		}
	}

	private static void PopulateMythicBonuses(LettuceMercenaryDataModel mercDataModel, LettuceMercenary merc, MercenaryDataPopluateExtra extraRequests)
	{
		if (!merc.IsMythicUpgradable())
		{
			return;
		}
		long mythicmodifier = merc.GetMythicModifier();
		if (extraRequests.HasFlag(MercenaryDataPopluateExtra.MythicStats))
		{
			mercDataModel.MythicView = true;
			mercDataModel.MythicLevel = merc.m_level + mythicmodifier;
			mercDataModel.MythicModifier = mythicmodifier;
			(int, int) mythicBonus = GetMythicStatBonus(mythicmodifier);
			mercDataModel.Card.Attack += mythicBonus.Item1;
			mercDataModel.Card.Health += mythicBonus.Item2;
		}
		long currentRenown = NetCache.Get().GetRenownBalance();
		if (extraRequests.HasFlag(MercenaryDataPopluateExtra.Abilities))
		{
			foreach (LettuceAbilityDataModel abilityDataModel in mercDataModel.AbilityList)
			{
				if (abilityDataModel.CurrentTier == abilityDataModel.MaxTier)
				{
					LettuceAbility ability = merc.GetLettuceAbility(abilityDataModel.AbilityId);
					LettuceAbilityDbfRecord abilityRecord = GameDbf.LettuceAbility.GetRecord(abilityDataModel.AbilityId);
					LettuceAbilityTierDataModel lettuceAbilityTierDataModel = abilityDataModel.AbilityTiers[abilityDataModel.MaxTier - 1];
					int renownCost = LettuceMercenary.GetRenownCostOfMythicUpgrade(MERCENARY_MYTHIC_UPGRADE_TYPE.ABILITY, ability.m_mythicModifier, 1);
					abilityDataModel.MythicModifier = ability.m_mythicModifier;
					abilityDataModel.CanMythicScale = (abilityRecord?.ScalingTags?.Count).GetValueOrDefault() > 0;
					abilityDataModel.ReadyForUpgrade = abilityDataModel.CanMythicScale && renownCost <= currentRenown;
					PopluateMythicBonusesOnCardDataModel(lettuceAbilityTierDataModel.AbilityTierCard, abilityRecord, ability.m_mythicModifier);
				}
			}
			foreach (LettuceAbilityDataModel equipmentDataModel in mercDataModel.EquipmentList)
			{
				if (equipmentDataModel.CurrentTier == equipmentDataModel.MaxTier)
				{
					LettuceAbility equipment = merc.GetLettuceEquipment(equipmentDataModel.AbilityId);
					LettuceEquipmentDbfRecord equipmentRecord = GameDbf.LettuceEquipment.GetRecord(equipment.ID);
					LettuceAbilityTierDataModel lettuceAbilityTierDataModel2 = equipmentDataModel.AbilityTiers[equipmentDataModel.MaxTier - 1];
					int renownCost2 = LettuceMercenary.GetRenownCostOfMythicUpgrade(MERCENARY_MYTHIC_UPGRADE_TYPE.EQUIPMENT, equipment.m_mythicModifier, 1);
					equipmentDataModel.MythicModifier = equipment.m_mythicModifier;
					equipmentDataModel.CanMythicScale = (equipmentRecord?.ScalingTags?.Count).GetValueOrDefault() > 0;
					equipmentDataModel.ReadyForUpgrade = equipmentDataModel.CanMythicScale && renownCost2 <= currentRenown;
					PopluateMythicBonusesOnCardDataModel(lettuceAbilityTierDataModel2.AbilityTierCard, equipmentRecord, equipment.m_mythicModifier);
				}
			}
		}
		if (extraRequests.HasFlag(MercenaryDataPopluateExtra.MythicTreasure))
		{
			PopulateMercenaryDataModelMythicTreasureList(mercDataModel, merc);
		}
	}

	public static void UpdateMythicUpgradability(LettuceMercenaryDataModel mercDataModel, LettuceMercenary merc)
	{
		if (!merc.IsMythicUpgradable())
		{
			return;
		}
		long currentRenown = NetCache.Get().GetRenownBalance();
		foreach (LettuceAbilityDataModel abilityDataModel in mercDataModel.AbilityList)
		{
			if (abilityDataModel.CurrentTier == abilityDataModel.MaxTier)
			{
				int renownCost = LettuceMercenary.GetRenownCostOfMythicUpgrade(MERCENARY_MYTHIC_UPGRADE_TYPE.ABILITY, abilityDataModel.MythicModifier, 1);
				abilityDataModel.ReadyForUpgrade = abilityDataModel.CanMythicScale && renownCost <= currentRenown;
			}
		}
		foreach (LettuceAbilityDataModel equipmentDataModel in mercDataModel.EquipmentList)
		{
			if (equipmentDataModel.CurrentTier == equipmentDataModel.MaxTier)
			{
				int renownCost2 = LettuceMercenary.GetRenownCostOfMythicUpgrade(MERCENARY_MYTHIC_UPGRADE_TYPE.EQUIPMENT, equipmentDataModel.MythicModifier, 1);
				equipmentDataModel.ReadyForUpgrade = equipmentDataModel.CanMythicScale && renownCost2 <= currentRenown;
			}
		}
		foreach (MercenaryMythicTreasureDataModel treasureDataModel in mercDataModel.MythicTreasureList)
		{
			int renownCost3 = LettuceMercenary.GetRenownCostOfMythicUpgrade(MERCENARY_MYTHIC_UPGRADE_TYPE.EQUIPMENT, treasureDataModel.TreasureScalar, 1);
			treasureDataModel.ReadyForUpgrade = renownCost3 <= currentRenown;
		}
	}

	private static void ApplyEquipmentBonuses(LettuceMercenaryDataModel mercDataModel, LettuceMercenary merc, MercenaryDataPopluateExtra extraRequests)
	{
		LettuceAbility slottedEquipment = merc.GetSlottedEquipment();
		if (slottedEquipment == null)
		{
			return;
		}
		bool attemptMythic = mercDataModel.MythicView && extraRequests.HasFlag(MercenaryDataPopluateExtra.MythicLoadout);
		SlottedEquipmentBonusData equipmentBonusData = new SlottedEquipmentBonusData();
		foreach (LettuceAbilityDataModel equipmentDataModel in mercDataModel.EquipmentList)
		{
			if (slottedEquipment != null && slottedEquipment.ID == equipmentDataModel.AbilityId)
			{
				if (attemptMythic && equipmentDataModel.MaxTier == equipmentDataModel.CurrentTier)
				{
					equipmentBonusData.MythicScalingTagList = GameDbf.LettuceEquipment.GetRecord(equipmentDataModel.AbilityId)?.ScalingTags;
				}
				int tierIndex = equipmentDataModel.CurrentTier - 1;
				if (equipmentDataModel.AbilityTiers != null && equipmentDataModel.AbilityTiers.Count > tierIndex)
				{
					LettuceAbilityTierDataModel tierDataModel = equipmentDataModel.AbilityTiers[tierIndex];
					equipmentBonusData.EquipmentCardModel = tierDataModel?.AbilityTierCard;
					equipmentBonusData.BaseMods = GameDbf.LettuceEquipmentTier.GetRecord(tierDataModel.TierId)?.EquipmentModifierData;
				}
				break;
			}
		}
		for (int i = 0; i < mercDataModel.EquipmentList.Count; i++)
		{
			if (mercDataModel.EquipmentList[i].AbilityId == slottedEquipment.ID)
			{
				mercDataModel.EquipmentSlotIndex = i;
				break;
			}
		}
		if (equipmentBonusData.BaseMods != null && !attemptMythic)
		{
			mercDataModel.IsAffectedBySlottedEquipment = equipmentBonusData.BaseMods.ModifiedLettuceAbilityValues.Count == 0;
			mercDataModel.Card.Attack += equipmentBonusData.BaseMods.MercenaryAttackChange;
			mercDataModel.Card.Health += equipmentBonusData.BaseMods.MercenaryHealthChange;
		}
		if (attemptMythic && equipmentBonusData.MythicScalingTagList != null && equipmentBonusData.EquipmentCardModel != null)
		{
			foreach (MythicEquipmentScalingCardTagDbfRecord scaleTag in equipmentBonusData.MythicScalingTagList)
			{
				int equipmentValue = GetCardTagValue(equipmentBonusData.EquipmentCardModel, (GAME_TAG)scaleTag.TagId);
				List<MythicEquipmentScalingDestinationCardTagDbfRecord> destinationTagList = scaleTag.ScalingDestinationTags;
				if (destinationTagList == null)
				{
					continue;
				}
				foreach (MythicEquipmentScalingDestinationCardTagDbfRecord destinationTag in destinationTagList)
				{
					(CardDataModel, LettuceAbilityDataModel) abilityDataModels = FindDataModelsForAbility(mercDataModel, destinationTag.LettuceAbilityId);
					if (abilityDataModels.Item1 == null)
					{
						Log.Lettuce.PrintWarning($"CollectionUtils.PopulateMythicBonuses - unable to find card model affected by equipment = {slottedEquipment.ID}");
						continue;
					}
					EntityDef abilityEntityDef = DefLoader.Get().GetEntityDef(abilityDataModels.Item1.CardId);
					if (abilityDataModels.Item1 == null)
					{
						Log.Lettuce.PrintWarning($"CollectionUtils.PopulateMythicBonuses - unable to find entityDef affected by equipment = {slottedEquipment.ID}");
						continue;
					}
					UpdateCardTag(abilityDataModels.Item1, (GAME_TAG)destinationTag.TagId, abilityEntityDef.GetTag((GAME_TAG)destinationTag.TagId), equipmentValue, destinationTag.IsReferenceTag, destinationTag.IsPowerKeywordTag, onlyInitial: false);
					if (abilityDataModels.Item2 == null)
					{
						mercDataModel.IsAffectedBySlottedEquipment = true;
					}
					else
					{
						abilityDataModels.Item2.IsAffectedBySlottedEquipment = true;
					}
				}
			}
		}
		foreach (LettuceAbilityDataModel abilityDataModel in mercDataModel.AbilityList)
		{
			ModifiedLettuceAbilityValueDbfRecord abilityBaseMods = equipmentBonusData.GetAbilityBaseMods(abilityDataModel.AbilityId);
			abilityDataModel.IsAffectedBySlottedEquipment = abilityBaseMods != null;
			foreach (LettuceAbilityTierDataModel abilityTierDataModel in abilityDataModel.AbilityTiers)
			{
				if (abilityTierDataModel == null || equipmentBonusData.BaseMods == null)
				{
					continue;
				}
				CardDbfRecord cardRecord = GameDbf.GetIndex().GetCardRecord(abilityTierDataModel.AbilityTierCard.CardId);
				EntityDef entityDef = DefLoader.Get().GetEntityDef(abilityTierDataModel.AbilityTierCard.CardId);
				foreach (CardEquipmentAltTextDbfRecord altText in cardRecord.EquipmentAltText)
				{
					if (altText.AltTextIndex != 0 && altText.EquipmentCardRecord.NoteMiniGuid == slottedEquipment.GetCardId())
					{
						abilityTierDataModel.AbilityTierCard.GameTagOverrides.Add(new GameTagValueDataModel
						{
							GameTag = GAME_TAG.USE_ALTERNATE_CARD_TEXT,
							Value = altText.AltTextIndex
						});
						break;
					}
				}
				if (abilityBaseMods == null)
				{
					continue;
				}
				abilityTierDataModel.AbilityTierCard.Mana += abilityBaseMods.SpeedChange;
				abilityTierDataModel.AbilityTierCard.Cooldown += abilityBaseMods.CooldownChange;
				bool mythicstats = attemptMythic && equipmentBonusData.MythicScalingTagList != null;
				if (!mythicstats)
				{
					abilityTierDataModel.AbilityTierCard.Attack += abilityBaseMods.AttackChange;
					abilityTierDataModel.AbilityTierCard.Health += abilityBaseMods.HealthChange;
				}
				if (abilityBaseMods.ScriptDataNum1Change != 0)
				{
					UpdateCardTag(abilityTierDataModel.AbilityTierCard, GAME_TAG.TAG_SCRIPT_DATA_NUM_1, entityDef.GetTag(GAME_TAG.TAG_SCRIPT_DATA_NUM_1), abilityBaseMods.ScriptDataNum1Change, isReferenceTag: false, isPowerKeywordTag: false, mythicstats);
				}
				if (abilityBaseMods.ScriptDataNum2Change != 0)
				{
					UpdateCardTag(abilityTierDataModel.AbilityTierCard, GAME_TAG.TAG_SCRIPT_DATA_NUM_2, entityDef.GetTag(GAME_TAG.TAG_SCRIPT_DATA_NUM_2), abilityBaseMods.ScriptDataNum2Change, isReferenceTag: false, isPowerKeywordTag: false, mythicstats);
				}
				if (abilityBaseMods.ScriptDataNum3Change != 0)
				{
					UpdateCardTag(abilityTierDataModel.AbilityTierCard, GAME_TAG.TAG_SCRIPT_DATA_NUM_3, entityDef.GetTag(GAME_TAG.TAG_SCRIPT_DATA_NUM_3), abilityBaseMods.ScriptDataNum3Change, isReferenceTag: false, isPowerKeywordTag: false, mythicstats);
				}
				if (abilityBaseMods.Tags == null || abilityBaseMods.Tags.Count <= 0)
				{
					continue;
				}
				foreach (ModifiedLettuceAbilityCardTagDbfRecord tag in abilityBaseMods.Tags)
				{
					SetCardTag(abilityTierDataModel.AbilityTierCard, (GAME_TAG)tag.TagId, tag.TagValue, tag.IsReferenceTag, tag.IsPowerKeywordTag, mythicstats);
				}
			}
		}
		if (extraRequests.HasFlag(MercenaryDataPopluateExtra.ShowMercCardText))
		{
			UpdateMercCardText(mercDataModel, slottedEquipment, equipmentBonusData, attemptMythic);
		}
	}

	private static void UpdateMercCardText(LettuceMercenaryDataModel mercDataModel, LettuceAbility slottedEquipment, SlottedEquipmentBonusData equipmentBonusData, bool isMythic)
	{
		LettuceEquipmentTierDbfRecord equipmentTierRecord = slottedEquipment.GetCurrentEquipmentTierRecord();
		if (equipmentTierRecord == null)
		{
			Log.Lettuce.PrintWarning("CollectionUtils.PopulateMythicBonuses - slotted equipment tier record was null");
		}
		else
		{
			if (!equipmentTierRecord.ShowTextOnMerc)
			{
				return;
			}
			EntityDef cardTextSourceEntityDef = DefLoader.Get().GetEntityDef(equipmentBonusData?.EquipmentCardModel?.CardId);
			if (cardTextSourceEntityDef == null)
			{
				Log.Lettuce.PrintWarning("CollectionUtils.PopulateMythicBonuses - slotted equipment datamodel was null");
				return;
			}
			if (isMythic)
			{
				cardTextSourceEntityDef = cardTextSourceEntityDef.Clone();
				foreach (GameTagValueDataModel tag in equipmentBonusData.EquipmentCardModel.GameTagOverrides)
				{
					cardTextSourceEntityDef.SetTag(tag.GameTag, tag.Value);
				}
			}
			mercDataModel.IsAffectedBySlottedEquipment = true;
			mercDataModel.ShowAbilityText = true;
			mercDataModel.AbilityText = cardTextSourceEntityDef.GetCardTextInHand();
		}
	}

	private static void PopluateAbilityCardDataModel(CardDataModel cardDataModel, LettuceAbility.AbilityTier abilityTier)
	{
		if (cardDataModel == null)
		{
			Log.Lettuce.PrintWarning("CollectionUtils.PopluateAbilityCardDataModel - cardDataModel was null");
			return;
		}
		if (abilityTier == null)
		{
			Log.Lettuce.PrintWarning("CollectionUtils.PopluateAbilityCardDataModel - abilityTier was null");
			return;
		}
		EntityDef entityDef = DefLoader.Get().GetEntityDef(cardDataModel.CardId);
		if (entityDef == null)
		{
			Log.Lettuce.PrintWarning("CollectionUtils.PopluateAbilityCardDataModel - unable to load entity for cardID = " + abilityTier.m_cardId);
			return;
		}
		PopluateEntityStatsToCardDataModel(cardDataModel, entityDef);
		PopulateCardNameData(entityDef, cardDataModel);
	}

	private static void PopluateEntityStatsToCardDataModel(CardDataModel cardDataModel, EntityDef entityDef)
	{
		if (cardDataModel == null)
		{
			Log.Lettuce.PrintWarning("CollectionUtils.PopluateEntityStatsToCardDataModel - card data model was null");
			return;
		}
		if (entityDef == null)
		{
			Log.Lettuce.PrintWarning("CollectionUtils.PopluateEntityStatsToCardDataModel - entity was null for cardID = " + cardDataModel.CardId);
			return;
		}
		(bool, int, int) values = entityDef.GetSummonedMinionStats();
		if (!values.Item1)
		{
			values.Item2 = entityDef.GetATK();
			values.Item3 = entityDef.GetHealth();
		}
		cardDataModel.Attack = values.Item2;
		cardDataModel.Health = values.Item3;
		cardDataModel.Mana = entityDef.GetTag(GAME_TAG.COST);
		cardDataModel.Cooldown = entityDef.GetTag(GAME_TAG.LETTUCE_COOLDOWN_CONFIG);
		UpdateCardTag(cardDataModel, GAME_TAG.HIDE_ATTACK, 0, (!values.Item1) ? 1 : 0, isReferenceTag: false, isPowerKeywordTag: false, onlyInitial: false);
		UpdateCardTag(cardDataModel, GAME_TAG.HIDE_HEALTH, 0, (!values.Item1) ? 1 : 0, isReferenceTag: false, isPowerKeywordTag: false, onlyInitial: false);
	}

	private static void PopluateMythicBonusesOnCardDataModel(CardDataModel cardDataModel, LettuceAbilityDbfRecord abilityRecord, long mythicLevel)
	{
		if (abilityRecord == null)
		{
			Log.Lettuce.PrintError("CollectionUtils.PopluateMythicBonusesOnCardDataModel - ability record was null for ability cardID = " + cardDataModel?.CardId);
			return;
		}
		EntityDef entityDef = DefLoader.Get().GetEntityDef(cardDataModel?.CardId);
		if (entityDef == null)
		{
			Log.Lettuce.PrintError("CollectionUtils.PopluateMythicBonusesOnCardDataModel - unable to load entity for ability cardID = " + cardDataModel?.CardId);
			return;
		}
		UpdateCardTag(cardDataModel, GAME_TAG.CARD_NAME_DATA_1, 0, (int)mythicLevel, isReferenceTag: false, isPowerKeywordTag: false, onlyInitial: false);
		List<MythicAbilityScalingCardTagDbfRecord> scalingTagList = abilityRecord.ScalingTags;
		if (scalingTagList == null)
		{
			return;
		}
		foreach (MythicAbilityScalingCardTagDbfRecord scalingTag in scalingTagList)
		{
			int initialValue = 0;
			int value = (int)(scalingTag.TagScaleValue * mythicLevel);
			GAME_TAG tagId = (GAME_TAG)scalingTag.TagId;
			if (tagId != GAME_TAG.HEALTH && tagId != GAME_TAG.ATK)
			{
				initialValue = entityDef.GetTag(scalingTag.TagId);
			}
			UpdateCardTag(cardDataModel, (GAME_TAG)scalingTag.TagId, initialValue, value, scalingTag.IsReferenceTag, scalingTag.IsPowerKeywordTag, onlyInitial: false);
		}
	}

	private static void PopluateMythicBonusesOnCardDataModel(CardDataModel cardDataModel, LettuceEquipmentDbfRecord equipmentRecord, long mythicLevel)
	{
		if (equipmentRecord == null)
		{
			Log.Lettuce.PrintError("CollectionUtils.PopluateMythicBonusesOnCardDataModel - equipment record was null for ability cardID = " + cardDataModel?.CardId);
			return;
		}
		EntityDef entityDef = DefLoader.Get().GetEntityDef(cardDataModel?.CardId);
		if (entityDef == null)
		{
			Log.Lettuce.PrintError("CollectionUtils.PopluateMythicBonusesOnCardDataModel - unable to load entity for ability cardID = " + cardDataModel?.CardId);
			return;
		}
		UpdateCardTag(cardDataModel, GAME_TAG.CARD_NAME_DATA_1, 0, (int)mythicLevel, isReferenceTag: false, isPowerKeywordTag: false, onlyInitial: false);
		List<MythicEquipmentScalingCardTagDbfRecord> scalingTagList = equipmentRecord.ScalingTags;
		if (scalingTagList == null)
		{
			return;
		}
		foreach (MythicEquipmentScalingCardTagDbfRecord scalingTag in scalingTagList)
		{
			int value = scalingTag.TagScaleValue * (int)mythicLevel;
			UpdateCardTag(cardDataModel, (GAME_TAG)scalingTag.TagId, entityDef.GetTag(scalingTag.TagId), value, scalingTag.IsReferenceTag, scalingTag.IsPowerKeywordTag, onlyInitial: false);
		}
	}

	private static (CardDataModel card, LettuceAbilityDataModel ability) FindDataModelsForAbility(LettuceMercenaryDataModel mercDataModel, int abilityId)
	{
		if (mercDataModel == null)
		{
			Log.Lettuce.PrintError("CollectionUtils.FindCardDataModelForMythic - missing LettuceMercenaryDataModel");
		}
		if (abilityId == 0)
		{
			return (card: mercDataModel?.Card, ability: null);
		}
		foreach (LettuceAbilityDataModel abilityDataModel in mercDataModel.AbilityList)
		{
			if (abilityDataModel.AbilityId == abilityId)
			{
				return (card: abilityDataModel.AbilityTiers[abilityDataModel.CurrentTier - 1]?.AbilityTierCard, ability: abilityDataModel);
			}
		}
		Log.Lettuce.PrintError($"CollectionUtils.FindCardDataModelForMythic - unable to find ability = {abilityId} for merc = {mercDataModel.MercenaryId}");
		return (card: null, ability: null);
	}

	private static void UpdateCardTag(CardDataModel cardDataModel, GAME_TAG tag, int initialTag, int tagValue, bool isReferenceTag, bool isPowerKeywordTag, bool onlyInitial)
	{
		switch (tag)
		{
		case GAME_TAG.HEALTH:
			cardDataModel.Health += tagValue;
			return;
		case GAME_TAG.ATK:
			cardDataModel.Attack += tagValue;
			return;
		}
		GameTagValueDataModel foundTag = null;
		foreach (GameTagValueDataModel tagOverride in cardDataModel.GameTagOverrides)
		{
			if (tagOverride.GameTag == tag)
			{
				foundTag = tagOverride;
				break;
			}
		}
		if (foundTag == null)
		{
			cardDataModel.GameTagOverrides.Add(new GameTagValueDataModel
			{
				GameTag = tag,
				Value = initialTag + tagValue,
				IsReferenceValue = isReferenceTag,
				IsPowerKeywordTag = isPowerKeywordTag
			});
		}
		else if (!onlyInitial)
		{
			foundTag.Value += tagValue;
		}
	}

	private static void SetCardTag(CardDataModel cardDataModel, GAME_TAG tag, int tagValue, bool isReferenceTag, bool isPowerKeywordTag, bool onlyInitial)
	{
		GameTagValueDataModel foundTag = null;
		foreach (GameTagValueDataModel tagOverride in cardDataModel.GameTagOverrides)
		{
			if (tagOverride.GameTag == tag)
			{
				foundTag = tagOverride;
				break;
			}
		}
		if (foundTag == null)
		{
			cardDataModel.GameTagOverrides.Add(new GameTagValueDataModel
			{
				GameTag = tag,
				Value = tagValue,
				IsReferenceValue = isReferenceTag,
				IsPowerKeywordTag = isPowerKeywordTag
			});
		}
		else if (!onlyInitial)
		{
			foundTag.Value = tagValue;
		}
	}

	private static int GetCardTagValue(CardDataModel cardDataModel, GAME_TAG tag)
	{
		switch (tag)
		{
		case GAME_TAG.HEALTH:
			return cardDataModel.Health;
		case GAME_TAG.ATK:
			return cardDataModel.Attack;
		default:
			foreach (GameTagValueDataModel tagOverride in cardDataModel.GameTagOverrides)
			{
				if (tagOverride.GameTag == tag)
				{
					return tagOverride.Value;
				}
			}
			return 0;
		}
	}

	private static void PopulateMercenaryDataModelAbilities(LettuceMercenaryDataModel dataModel, LettuceMercenary merc)
	{
		DataModelList<LettuceAbilityDataModel> abilityList = new DataModelList<LettuceAbilityDataModel>();
		foreach (LettuceAbility ability in merc.m_abilityList)
		{
			LettuceAbilityDataModel abilityData = new LettuceAbilityDataModel();
			PopulateAbilityDataModel(abilityData, ability, merc);
			abilityList.Add(abilityData);
		}
		dataModel.AbilityList = abilityList;
		DataModelList<LettuceAbilityDataModel> equipmentList = new DataModelList<LettuceAbilityDataModel>();
		foreach (LettuceAbility equipment in merc.m_equipmentList)
		{
			LettuceAbilityDataModel equipmentData = new LettuceAbilityDataModel();
			PopulateAbilityDataModel(equipmentData, equipment, merc);
			equipmentData.IsEquipment = true;
			equipmentList.Add(equipmentData);
		}
		dataModel.EquipmentList = equipmentList;
	}

	private static void PopulateMercenaryDataModelArtVariations(LettuceMercenaryDataModel dataModel, LettuceMercenary merc)
	{
		dataModel.ArtVariationList = new DataModelList<LettuceMercenaryArtVariationDataModel>();
		foreach (MercenaryArtVariationDbfRecord variation in LettuceMercenary.GetArtVariations(merc.ID))
		{
			foreach (MercenaryArtVariationPremiumDbfRecord variationPremium in variation.MercenaryArtVariationPremiums)
			{
				if (!variationPremium.Collectible)
				{
					continue;
				}
				TAG_PREMIUM tagPremium = TAG_PREMIUM.NORMAL;
				switch (variationPremium.Premium)
				{
				case MercenaryArtVariationPremium.MercenariesPremium.PREMIUM_GOLDEN:
					tagPremium = TAG_PREMIUM.GOLDEN;
					break;
				case MercenaryArtVariationPremium.MercenariesPremium.PREMIUM_DIAMOND:
					tagPremium = TAG_PREMIUM.DIAMOND;
					break;
				}
				bool isUnlocked = merc.IsArtVariationUnlocked(variation.ID, tagPremium);
				string lockedText = null;
				if (!isUnlocked)
				{
					MercenaryUnlock unlock = GameDbf.GetIndex().GetArtVariationUnlock(variation.ID, tagPremium);
					switch (unlock.m_unlockType)
					{
					case MercenaryUnlock.UnlockType.Packs:
						lockedText = GameStrings.Get("GLUE_LETTUCE_VANITY_LOCKEDPLATE_MESSAGE");
						break;
					case MercenaryUnlock.UnlockType.RewardTrack:
						lockedText = GameStrings.Get("GLUE_LETTUCE_VANITY_LOCKEDPLATE_MESSAGE_REWARD_TRACK");
						break;
					case MercenaryUnlock.UnlockType.VisitorTask:
						lockedText = GameStrings.Format("GLUE_LETTUCE_VANITY_LOCKEDPLATE_MESSAGE_TASK", GameStrings.Format(unlock.m_visitorTask.TaskTitle.GetString()));
						break;
					case MercenaryUnlock.UnlockType.Custom:
						lockedText = unlock.m_customAcquireText;
						break;
					}
				}
				dataModel.ArtVariationList.Add(new LettuceMercenaryArtVariationDataModel
				{
					Card = new CardDataModel
					{
						CardId = variation.CardRecord.NoteMiniGuid,
						Premium = tagPremium,
						FlavorText = variation.CardRecord.FlavorText
					},
					ArtVariationId = variation.ID,
					Unlocked = isUnlocked,
					Selected = (dataModel.Card.CardId == variation.CardRecord.NoteMiniGuid && dataModel.Card.Premium == tagPremium),
					LockedText = lockedText,
					NewlyUnlocked = merc.IsArtVariationNew(variation.ID, tagPremium)
				});
			}
		}
		dataModel.ArtVariationList.Sort(delegate(LettuceMercenaryArtVariationDataModel a, LettuceMercenaryArtVariationDataModel b)
		{
			if (a.Card.Premium != b.Card.Premium)
			{
				if (a.Card.Premium <= b.Card.Premium)
				{
					return 1;
				}
				return -1;
			}
			return (a.ArtVariationId != b.ArtVariationId) ? ((a.ArtVariationId <= b.ArtVariationId) ? 1 : (-1)) : 0;
		});
		dataModel.ArtVariationPageList = new DataModelList<LettuceMercenaryArtVariationPageDataModel>();
		for (int remaining = dataModel.ArtVariationList.Count; remaining > 0; remaining -= 4)
		{
			dataModel.ArtVariationPageList.Add(new LettuceMercenaryArtVariationPageDataModel
			{
				ArtVatiationsOnPageCount = Mathf.Min(remaining, 4),
				ShowLeftArrow = true,
				ShowRightArrow = true
			});
		}
		dataModel.ArtVariationPageIndex = 0;
		if (dataModel.ArtVariationPageList.Count > 0)
		{
			LettuceMercenaryArtVariationPageDataModel lettuceMercenaryArtVariationPageDataModel = dataModel.ArtVariationPageList[0];
			lettuceMercenaryArtVariationPageDataModel.ShowLeftArrow = false;
			lettuceMercenaryArtVariationPageDataModel.ShowRightArrow = dataModel.ArtVariationPageList.Count > 1;
			dataModel.ArtVariationPageList[dataModel.ArtVariationPageList.Count - 1].ShowRightArrow = false;
		}
	}

	public static void PopulateMercenaryMythicUpgradeDisplay(MercenaryMythicUpgradeDisplayDataModel upgradeDataModel, LettuceMercenaryDataModel mercenaryDataModel, LettuceMercenary mercenary)
	{
		int[] upgradeOptions = new int[1] { 1 };
		if (upgradeDataModel == null || upgradeDataModel.UpgradeType == MERCENARY_MYTHIC_UPGRADE_TYPE.INVALID)
		{
			return;
		}
		upgradeDataModel.CurrentChoice = 0;
		upgradeDataModel.CurrentRenown = NetCache.Get().GetRenownBalance();
		upgradeDataModel.UpgradeChoices = new DataModelList<MercenaryMythicUpgradeChoiceDataModel>();
		if (upgradeDataModel.UpgradeType == MERCENARY_MYTHIC_UPGRADE_TYPE.TREASURE)
		{
			int[] array = upgradeOptions;
			foreach (int option in array)
			{
				MercenaryMythicUpgradeChoiceDataModel upgradeChoiceDataModel = new MercenaryMythicUpgradeChoiceDataModel
				{
					MythicLevel = upgradeDataModel.CurrentMythicLevel + option,
					LevelCount = option,
					Cost = LettuceMercenary.GetRenownCostOfMythicUpgrade(upgradeDataModel.UpgradeType, upgradeDataModel.CurrentMythicLevel, option),
					UpgradeCard = CreateTreasureCardDataModel(upgradeDataModel.UpgradeId, 0, upgradeDataModel.CurrentMythicLevel + option)
				};
				upgradeChoiceDataModel.NextTierAbilityChanges = PopulateAbilityModifiedValues(upgradeDataModel.CurrentCard, upgradeChoiceDataModel.UpgradeCard);
				upgradeDataModel.UpgradeChoices.Add(upgradeChoiceDataModel);
			}
		}
		else if (upgradeDataModel.UpgradeType == MERCENARY_MYTHIC_UPGRADE_TYPE.ABILITY)
		{
			LettuceAbility ability = mercenary.GetLettuceAbility(upgradeDataModel.UpgradeId);
			LettuceAbility.AbilityTier abilityTier = ability?.GetAbilityTier(ability.GetMaxTier());
			LettuceAbilityDbfRecord abilityRecord = GameDbf.LettuceAbility.GetRecord(upgradeDataModel.UpgradeId);
			PopluateAbilityCardDataModel(upgradeDataModel.CurrentCard, abilityTier);
			PopluateMythicBonusesOnCardDataModel(upgradeDataModel.CurrentCard, abilityRecord, upgradeDataModel.CurrentMythicLevel);
			int[] array = upgradeOptions;
			foreach (int option2 in array)
			{
				MercenaryMythicUpgradeChoiceDataModel upgradeChoiceDataModel2 = new MercenaryMythicUpgradeChoiceDataModel
				{
					MythicLevel = upgradeDataModel.CurrentMythicLevel + option2,
					LevelCount = option2,
					Cost = LettuceMercenary.GetRenownCostOfMythicUpgrade(upgradeDataModel.UpgradeType, upgradeDataModel.CurrentMythicLevel, option2),
					UpgradeCard = new CardDataModel
					{
						CardId = upgradeDataModel.CurrentCard.CardId,
						FlavorText = upgradeDataModel.CurrentCard.FlavorText,
						Premium = upgradeDataModel.CurrentCard.Premium
					}
				};
				PopluateAbilityCardDataModel(upgradeChoiceDataModel2.UpgradeCard, abilityTier);
				PopluateMythicBonusesOnCardDataModel(upgradeChoiceDataModel2.UpgradeCard, abilityRecord, (uint)(upgradeDataModel.CurrentMythicLevel + option2));
				upgradeChoiceDataModel2.NextTierAbilityChanges = PopulateAbilityModifiedValues(upgradeDataModel.CurrentCard, upgradeChoiceDataModel2.UpgradeCard);
				upgradeDataModel.UpgradeChoices.Add(upgradeChoiceDataModel2);
			}
		}
		else if (upgradeDataModel.UpgradeType == MERCENARY_MYTHIC_UPGRADE_TYPE.EQUIPMENT)
		{
			LettuceAbility equipment = mercenary.GetLettuceEquipment(upgradeDataModel.UpgradeId);
			LettuceAbility.AbilityTier equipmentTier = equipment?.GetAbilityTier(equipment.GetMaxTier());
			LettuceEquipmentDbfRecord equipmentRecord = GameDbf.LettuceEquipment.GetRecord(upgradeDataModel.UpgradeId);
			PopluateAbilityCardDataModel(upgradeDataModel.CurrentCard, equipmentTier);
			PopluateMythicBonusesOnCardDataModel(upgradeDataModel.CurrentCard, equipmentRecord, upgradeDataModel.CurrentMythicLevel);
			int[] array = upgradeOptions;
			foreach (int option3 in array)
			{
				MercenaryMythicUpgradeChoiceDataModel upgradeChoiceDataModel3 = new MercenaryMythicUpgradeChoiceDataModel
				{
					MythicLevel = upgradeDataModel.CurrentMythicLevel + option3,
					LevelCount = option3,
					Cost = LettuceMercenary.GetRenownCostOfMythicUpgrade(upgradeDataModel.UpgradeType, upgradeDataModel.CurrentMythicLevel, option3),
					UpgradeCard = new CardDataModel
					{
						CardId = upgradeDataModel.CurrentCard.CardId,
						FlavorText = upgradeDataModel.CurrentCard.FlavorText,
						Premium = upgradeDataModel.CurrentCard.Premium
					}
				};
				PopluateAbilityCardDataModel(upgradeChoiceDataModel3.UpgradeCard, equipmentTier);
				PopluateMythicBonusesOnCardDataModel(upgradeChoiceDataModel3.UpgradeCard, equipmentRecord, (uint)(upgradeDataModel.CurrentMythicLevel + option3));
				upgradeChoiceDataModel3.NextTierAbilityChanges = PopulateAbilityModifiedValues(upgradeDataModel.CurrentCard, upgradeChoiceDataModel3.UpgradeCard);
				upgradeDataModel.UpgradeChoices.Add(upgradeChoiceDataModel3);
			}
		}
		(int, int) preBonus = GetMythicStatBonus(mercenaryDataModel.MythicModifier);
		(int attack, int health) mythicStatBonus = GetMythicStatBonus(mercenaryDataModel.MythicModifier + 1);
		int attackChange = mythicStatBonus.attack - preBonus.Item1;
		int healthChange = mythicStatBonus.health - preBonus.Item2;
		StringBuilder str = new StringBuilder();
		if (attackChange > 0)
		{
			str.AppendFormat(GameStrings.Get("GLUE_LETTUCE_COLLECTION_MYTHIC_MERC_STATS_FORMAT_ATTACK"), attackChange);
		}
		if (healthChange > 0)
		{
			if (str.Length > 0)
			{
				str.Append(" ");
			}
			str.AppendFormat(GameStrings.Get("GLUE_LETTUCE_COLLECTION_MYTHIC_MERC_STATS_FORMAT_HEALTH"), healthChange);
		}
		upgradeDataModel.MercUpgradeText = str.ToString();
	}

	private static void PopulateMercenaryDataModelMythicTreasureList(LettuceMercenaryDataModel dataModel, LettuceMercenary merc)
	{
		dataModel.MythicTreasureList.Clear();
		LettuceMercenaryDbfRecord mercenaryRecord = GameDbf.LettuceMercenary.GetRecord(merc.ID);
		if (mercenaryRecord == null)
		{
			return;
		}
		long currentRenown = NetCache.Get().GetRenownBalance();
		NetCache.NetCacheMercenariesMythicTreasureInfo netcacheTreasureInfo = NetCache.Get().GetNetObject<NetCache.NetCacheMercenariesMythicTreasureInfo>();
		if (netcacheTreasureInfo == null)
		{
			return;
		}
		foreach (MercenaryAllowedTreasureDbfRecord allowedTreasureRecord in mercenaryRecord.MercenaryTreasure)
		{
			int mythicScalar = 0;
			if (netcacheTreasureInfo.MythicTreasureScalarMap.TryGetValue(allowedTreasureRecord.TreasureId, out var treasureScalar))
			{
				mythicScalar = treasureScalar.Scalar;
			}
			int renownCost = LettuceMercenary.GetRenownCostOfMythicUpgrade(MERCENARY_MYTHIC_UPGRADE_TYPE.EQUIPMENT, mythicScalar, 1);
			dataModel.MythicTreasureList.Add(new MercenaryMythicTreasureDataModel
			{
				TreasureId = allowedTreasureRecord.TreasureId,
				TreasureScalar = mythicScalar,
				MyticTreasure = CreateTreasureCardDataModel(allowedTreasureRecord.TreasureId, 0, mythicScalar),
				ReadyForUpgrade = (renownCost <= currentRenown)
			});
		}
	}

	public static LettuceAbilityModifiedValuesDataModel PopulateAbilityModifiedValues(string baseCardId, string upgradeCardId)
	{
		if (string.IsNullOrEmpty(baseCardId))
		{
			Log.Lettuce.PrintWarning("PopulateAbilityModifiedValues - invalid base card Id = " + baseCardId);
			return null;
		}
		if (string.IsNullOrEmpty(upgradeCardId))
		{
			Log.Lettuce.PrintWarning("PopulateAbilityModifiedValues - invalid upgrade card Id = " + upgradeCardId);
			return null;
		}
		EntityDef baseEntityDef = DefLoader.Get().GetEntityDef(baseCardId);
		if (baseEntityDef == null)
		{
			Log.Lettuce.PrintWarning("PopulateAbilityModifiedValues - failed to load baseEntityDef for cardID = " + baseCardId);
			return null;
		}
		EntityDef upgradeEntityDef = DefLoader.Get().GetEntityDef(upgradeCardId);
		if (upgradeEntityDef == null)
		{
			Log.Lettuce.PrintWarning("PopulateAbilityModifiedValues - failed to load upgradeEntityDef for cardID = " + upgradeCardId);
			return null;
		}
		return new LettuceAbilityModifiedValuesDataModel
		{
			IsAttackChanging = (baseEntityDef.GetATK() != upgradeEntityDef.GetATK()),
			IsHealthChanging = (baseEntityDef.GetHealth() != upgradeEntityDef.GetHealth()),
			IsSpeedChanging = (baseEntityDef.GetCost() != upgradeEntityDef.GetCost()),
			IsCooldownChanging = (baseEntityDef.GetTag(GAME_TAG.LETTUCE_COOLDOWN_CONFIG) != upgradeEntityDef.GetTag(GAME_TAG.LETTUCE_COOLDOWN_CONFIG)),
			IsDescriptionChanging = (baseEntityDef.GetCardTextInHand() != upgradeEntityDef.GetCardTextInHand())
		};
	}

	public static LettuceAbilityModifiedValuesDataModel PopulateAbilityModifiedValues(CardDataModel baseCard, CardDataModel upgradeCard)
	{
		if (baseCard == null)
		{
			Log.Lettuce.PrintWarning("PopulateAbilityModifiedValues - invalid base card Id");
			return null;
		}
		if (upgradeCard == null)
		{
			Log.Lettuce.PrintWarning("PopulateAbilityModifiedValues - invalid upgrade card");
			return null;
		}
		LettuceAbilityModifiedValuesDataModel lettuceAbilityModifiedValuesDataModel = new LettuceAbilityModifiedValuesDataModel();
		lettuceAbilityModifiedValuesDataModel.IsHealthChanging = GetCardTagValue(baseCard, GAME_TAG.HEALTH) != GetCardTagValue(upgradeCard, GAME_TAG.HEALTH);
		lettuceAbilityModifiedValuesDataModel.IsHealthChanging = GetCardTagValue(baseCard, GAME_TAG.ATK) != GetCardTagValue(upgradeCard, GAME_TAG.ATK);
		lettuceAbilityModifiedValuesDataModel.IsHealthChanging = GetCardTagValue(baseCard, GAME_TAG.COST) != GetCardTagValue(upgradeCard, GAME_TAG.COST);
		lettuceAbilityModifiedValuesDataModel.IsHealthChanging = GetCardTagValue(baseCard, GAME_TAG.LETTUCE_COOLDOWN_CONFIG) != GetCardTagValue(upgradeCard, GAME_TAG.LETTUCE_COOLDOWN_CONFIG);
		lettuceAbilityModifiedValuesDataModel.IsDescriptionChanging = GetCardTagValue(baseCard, GAME_TAG.TAG_SCRIPT_DATA_NUM_1) != GetCardTagValue(upgradeCard, GAME_TAG.TAG_SCRIPT_DATA_NUM_1);
		lettuceAbilityModifiedValuesDataModel.IsDescriptionChanging |= GetCardTagValue(baseCard, GAME_TAG.TAG_SCRIPT_DATA_NUM_2) != GetCardTagValue(upgradeCard, GAME_TAG.TAG_SCRIPT_DATA_NUM_2);
		lettuceAbilityModifiedValuesDataModel.IsDescriptionChanging |= GetCardTagValue(baseCard, GAME_TAG.TAG_SCRIPT_DATA_NUM_3) != GetCardTagValue(upgradeCard, GAME_TAG.TAG_SCRIPT_DATA_NUM_3);
		lettuceAbilityModifiedValuesDataModel.IsDescriptionChanging |= GetCardTagValue(baseCard, GAME_TAG.TAG_SCRIPT_DATA_NUM_4) != GetCardTagValue(upgradeCard, GAME_TAG.TAG_SCRIPT_DATA_NUM_4);
		lettuceAbilityModifiedValuesDataModel.IsDescriptionChanging |= GetCardTagValue(baseCard, GAME_TAG.TAG_SCRIPT_DATA_NUM_5) != GetCardTagValue(upgradeCard, GAME_TAG.TAG_SCRIPT_DATA_NUM_5);
		lettuceAbilityModifiedValuesDataModel.IsDescriptionChanging |= GetCardTagValue(baseCard, GAME_TAG.TAG_SCRIPT_DATA_NUM_6) != GetCardTagValue(upgradeCard, GAME_TAG.TAG_SCRIPT_DATA_NUM_6);
		return lettuceAbilityModifiedValuesDataModel;
	}

	public static int GetFirstOwnedEquipmentIndex(LettuceMercenaryDataModel mercData)
	{
		for (int i = 0; i < mercData.EquipmentList.Count; i++)
		{
			if (mercData.EquipmentList[i].Owned)
			{
				return i;
			}
		}
		return -1;
	}

	public static void PopulateMercenaryCardDataModel(LettuceMercenaryDataModel dataModel, LettuceMercenary.ArtVariation artVariation)
	{
		if (dataModel == null)
		{
			Log.Lettuce.PrintError("PopulateMercenaryCardDataModel - Data model was null");
			return;
		}
		if (dataModel.Card == null)
		{
			dataModel.Card = new CardDataModel();
		}
		if (artVariation != null)
		{
			CardDbfRecord record = artVariation.m_record.CardRecord;
			string cardId = record.NoteMiniGuid;
			dataModel.Card.CardId = cardId;
			dataModel.Card.Premium = artVariation.m_premium;
			dataModel.Card.FlavorText = record.FlavorText;
		}
		else
		{
			Log.Lettuce.PrintError("PopulateMercenaryCardDataModel - art variation was null");
		}
		dataModel.HideXp = false;
		dataModel.HideWatermark = true;
		dataModel.HideStats = false;
		dataModel.Label = string.Empty;
	}

	public static void SetMercenaryStatsByLevel(LettuceMercenaryDataModel dataModel, int mercenaryId, int level, bool isFullyUpgraded)
	{
		GetMercenaryStatsByLevel(mercenaryId, level, isFullyUpgraded, out var attack, out var health);
		dataModel.Card.Attack = attack;
		dataModel.Card.Health = health;
	}

	public static void GetMercenaryStatsByLevel(int mercenaryId, int level, bool isFullyUpgraded, out int attack, out int health)
	{
		attack = 0;
		health = 0;
		bool isMaxLevel;
		LettuceMercenaryLevelStatsDbfRecord levelStatsRecord = GameUtils.GetMercenaryStatsByLevel(mercenaryId, level, out isMaxLevel);
		if (levelStatsRecord != null)
		{
			attack = levelStatsRecord.Attack;
			health = levelStatsRecord.Health;
			if (isFullyUpgraded && isMaxLevel)
			{
				NetCache.NetCacheFeatures features = NetCache.Get().GetNetObject<NetCache.NetCacheFeatures>();
				attack += features.Mercenaries.FullyUpgradedStatBoostAttack;
				health += features.Mercenaries.FullyUpgradedStatBoostHealth;
			}
		}
	}

	public static (int attack, int health) GetMythicStatBonus(long mythicLevel)
	{
		NetCache.NetCacheFeatures features = NetCache.Get().GetNetObject<NetCache.NetCacheFeatures>();
		if (features == null)
		{
			Log.Lettuce.PrintError("CollectionUtils.PopulateMythicBonuses - NetCache.NetCacheFeatures was null");
			return default((int, int));
		}
		return (attack: Mathf.FloorToInt((float)mythicLevel * features.Mercenaries.AttackBoostPerMythicLevel), health: Mathf.FloorToInt((float)mythicLevel * features.Mercenaries.HealthBoostPerMythicLevel));
	}

	public static void PopulateAbilityTierDataModel(LettuceAbilityTierDataModel dataModel, LettuceAbility.AbilityTier abilityTier, LettuceMercenary parentMerc, int parentAbilityId)
	{
		dataModel.Tier = abilityTier.m_tier;
		dataModel.ParentAbilityId = parentAbilityId;
		dataModel.ValidTier = abilityTier.m_validTier;
		dataModel.TierId = abilityTier.ID;
		if (abilityTier.m_validTier)
		{
			dataModel.CoinCraftCost = $"{abilityTier.m_coinCost}";
			CardDbfRecord record = GameDbf.GetIndex().GetCardRecord(abilityTier.m_cardId);
			if (record == null)
			{
				Log.Lettuce.PrintWarning("CollectionUtils.PopulateLettuceAbilityTierDataModel - unable to load card record for cardID = {0}", abilityTier.m_cardId);
				return;
			}
			dataModel.AbilityName = record.Name;
			dataModel.AbilityTierCard = new CardDataModel
			{
				CardId = abilityTier.m_cardId,
				Premium = TAG_PREMIUM.NORMAL,
				FlavorText = record.FlavorText
			};
			PopluateAbilityCardDataModel(dataModel.AbilityTierCard, abilityTier);
		}
	}

	private static void FillInAbilityTierData(LettuceAbilityDataModel dataModel, LettuceAbility ability, LettuceMercenary parentMerc, bool fillInAllTiers = false)
	{
		DataModelList<LettuceAbilityTierDataModel> tierList = new DataModelList<LettuceAbilityTierDataModel>();
		if (ability != null)
		{
			LettuceAbility.AbilityTier[] tierList2 = ability.m_tierList;
			foreach (LettuceAbility.AbilityTier abilityTier in tierList2)
			{
				LettuceAbilityTierDataModel tierData = null;
				if (fillInAllTiers || ability.m_tier <= abilityTier.m_tier)
				{
					tierData = new LettuceAbilityTierDataModel();
					PopulateAbilityTierDataModel(tierData, abilityTier, parentMerc, ability.ID);
				}
				tierList.Add(tierData);
				dataModel.MaxTier = Mathf.Max(dataModel.MaxTier, abilityTier.m_tier);
			}
		}
		dataModel.AbilityTiers = tierList;
	}

	public static void PopulateDefaultAbilityDataModelWithTier(LettuceAbilityDataModel dataModel, LettuceAbility ability, LettuceMercenary parentMerc, int desiredTier = 1)
	{
		if (ability == null)
		{
			Debug.LogErrorFormat("PopulateDefaultAbilityDataModelWithTier has null ability param.  parentMerc = {0}", (parentMerc != null) ? parentMerc.m_mercName : "None");
		}
		else
		{
			dataModel.AbilityId = ability.ID;
			dataModel.AbilityName = ability.m_abilityName;
		}
		dataModel.CurrentTier = desiredTier;
		dataModel.ParentMercId = parentMerc.ID;
		dataModel.AbilityRole = parentMerc.m_role;
		dataModel.ReadyForUpgrade = false;
		if (ability != null && ability.m_cardType == MercenariesModeCardType.Equipment)
		{
			dataModel.IsEquipment = true;
			dataModel.IsEquipped = false;
			dataModel.Owned = true;
		}
		FillInAbilityTierData(dataModel, ability, parentMerc, fillInAllTiers: true);
	}

	public static void PopulateAbilityDataModel(LettuceAbilityDataModel dataModel, LettuceAbility ability, LettuceMercenary parentMerc)
	{
		if (ability == null)
		{
			Debug.LogErrorFormat("PopulateAbilityDataModel has null ability param.  parentMerc = {0}", (parentMerc != null) ? parentMerc.m_mercName : "None");
		}
		else
		{
			dataModel.AbilityId = ability.ID;
			dataModel.AbilityName = ability.m_abilityName;
			dataModel.CurrentTier = ability.m_tier;
			dataModel.MythicModifier = ability.m_mythicModifier;
		}
		dataModel.ParentMercId = parentMerc.ID;
		dataModel.AbilityRole = parentMerc.m_role;
		dataModel.ReadyForUpgrade = parentMerc.IsCardReadyForUpgrade(ability);
		LettuceMercenary.Loadout loadout = parentMerc.GetCurrentLoadout();
		if (ability != null)
		{
			dataModel.IsNew = !ability.IsAcknowledged(parentMerc);
			if (ability.m_cardType == MercenariesModeCardType.Ability)
			{
				dataModel.UnlockLevel = ability.m_unlockLevel;
				dataModel.LockPlateText = GameStrings.Format("GLUE_LETTUCE_ABILITY_REACH_LEVEL", ability.m_unlockLevel);
			}
			else
			{
				dataModel.IsEquipment = true;
				dataModel.IsEquipped = loadout.m_equipmentRecord != null && loadout.m_equipmentRecord.ID == ability.ID;
				dataModel.Owned = ability.Owned;
				if (!ability.Owned)
				{
					MercenaryUnlock equipmentUnlock = GameDbf.GetIndex().GetEquipmentUnlockFromEquipmentID(ability.ID);
					if (equipmentUnlock != null)
					{
						switch (equipmentUnlock.m_unlockType)
						{
						case MercenaryUnlock.UnlockType.VisitorTask:
							dataModel.LockPlateText = GameStrings.Format("GLUE_LETTUCE_COLLECTION_EQUIPMENT_UNLOCK_DESC", equipmentUnlock.m_visitorTaskIndex + 1);
							break;
						case MercenaryUnlock.UnlockType.Achievement:
						{
							AchievementDataModel achievementDataModel = AchievementManager.Get().GetAchievementDataModel(equipmentUnlock.m_achievement.ID);
							if (achievementDataModel.Progress >= achievementDataModel.Quota)
							{
								dataModel.LockPlateText = GameStrings.Get("GLUE_LETTUCE_COLLECTION_EQUIPMENT_REDEEM_ACHIEVEMENT");
								break;
							}
							string achievementDescription = achievementDataModel.Description;
							if (achievementDataModel.Quota > 1)
							{
								achievementDescription = ProgressUtils.FormatDescription(achievementDescription, achievementDataModel.Quota);
								dataModel.LockPlateText = achievementDescription + "\n" + achievementDataModel.Progress + "/" + achievementDataModel.Quota;
							}
							else
							{
								dataModel.LockPlateText = achievementDescription;
							}
							break;
						}
						case MercenaryUnlock.UnlockType.Bounty:
							dataModel.LockPlateText = GameStrings.Format("GLUE_LETTUCE_COLLECTION_EQUIPMENT_UNLOCK_BY_BOUNTY_DESC", LettuceVillageDataUtil.GenerateBountyName(equipmentUnlock.m_bounty));
							break;
						default:
							Log.CollectionManager.PrintError("CollectionManager_Lettuce.RegisterMercenary(): Ability [{0}] missing required unlock info!", ability.m_abilityName);
							break;
						}
					}
				}
			}
		}
		FillInAbilityTierData(dataModel, ability, parentMerc);
	}

	public static void PopulateTeamPreviewData(LettuceTeamDataModel dataModel, LettuceTeam team, List<int> deadMercs, bool populateCards, bool isRemote = false, bool showMythicTeamLevel = false, MercenaryDataPopluateExtra extraMercData = MercenaryDataPopluateExtra.None)
	{
		dataModel.MercenaryList = new DataModelList<LettuceMercenaryDataModel>();
		if (team == null)
		{
			return;
		}
		foreach (LettuceMercenary merc in team.GetMercs())
		{
			if (merc == null)
			{
				Log.CollectionManager.PrintError($"CollectionManager_Lettuce.PopulateTeamPreviewData(): There was an error displaying a mercenary for team {team.ID}");
				continue;
			}
			LettuceMercenaryDataModel mercData = GetMercDataModelForTeam(merc, team, isRemote, showLevelInList: true, extraMercData);
			if (populateCards)
			{
				mercData.ExperienceInitial = (int)merc.m_experience;
				mercData.ExperienceFinal = (int)merc.m_experience;
				mercData.FullyUpgradedInitial = merc.m_isFullyUpgraded;
				mercData.FullyUpgradedFinal = merc.m_isFullyUpgraded;
			}
			if (deadMercs != null)
			{
				mercData.DeadInMapRun = deadMercs.Contains(merc.ID);
			}
			dataModel.MercenaryList.Add(mercData);
		}
		dataModel.MythicLevel = (showMythicTeamLevel ? team.GetTeamMythicLevel() : (-1));
	}

	public static void PopulateTeamTreasures(LettuceTeamDataModel dataModel, List<LettuceMapTreasureAssignment> treasureList, bool inMythicMode)
	{
		if (treasureList == null)
		{
			return;
		}
		foreach (LettuceMercenaryDataModel merc in dataModel.MercenaryList)
		{
			LettuceMapTreasureAssignment choiceMercenaryTreasureAssignment = treasureList.FirstOrDefault((LettuceMapTreasureAssignment e) => e.AssignedMercenary == merc.MercenaryId);
			if (choiceMercenaryTreasureAssignment != null)
			{
				merc.TreasureCard = CreateTreasureCardDataModel(choiceMercenaryTreasureAssignment.Treasure, inMythicMode);
			}
		}
	}

	public static CardDataModel CreateTreasureCardDataModel(LettuceMapTreasure MapTreasure, bool inMythicMode)
	{
		CardDataModel treasureCard = null;
		if (MapTreasure.HasTreasureId)
		{
			int mythicScalar = 0;
			if (inMythicMode)
			{
				NetCache.NetCacheMercenariesMythicTreasureInfo mythicTreasureInfo = NetCache.Get().GetNetObject<NetCache.NetCacheMercenariesMythicTreasureInfo>();
				if (mythicTreasureInfo != null && mythicTreasureInfo.MythicTreasureScalarMap.TryGetValue(MapTreasure.TreasureId, out var treasureScalar))
				{
					mythicScalar = treasureScalar.Scalar;
				}
			}
			treasureCard = CreateTreasureCardDataModel(MapTreasure.TreasureId, (int)MapTreasure.Scalar, mythicScalar);
		}
		if (treasureCard == null)
		{
			treasureCard = new CardDataModel();
		}
		return treasureCard;
	}

	public static CardDataModel CreateTreasureCardDataModel(int treasureId, int baseScalar, int mythicScalar)
	{
		CardDataModel treasureCard = new CardDataModel();
		LettuceTreasureDbfRecord treasureRecord = GameDbf.LettuceTreasure.GetRecord(treasureId);
		if (treasureRecord == null)
		{
			Log.Lettuce.PrintWarning($"CollectionUtils.CreateTreasureCardDataModel - unable to load treasure record for ID = {treasureId}");
			return null;
		}
		treasureCard.CardId = GameUtils.TranslateDbIdToCardId(treasureRecord.CardId);
		EntityDef entityDef = DefLoader.Get().GetEntityDef(treasureCard.CardId);
		if (entityDef == null)
		{
			Log.Lettuce.PrintWarning("CollectionUtils.CreateTreasureCardDataModel - unable to load entity for cardID = " + treasureCard.CardId);
			return null;
		}
		PopluateEntityStatsToCardDataModel(treasureCard, entityDef);
		int finalScalar = baseScalar + mythicScalar;
		if (finalScalar > 0)
		{
			treasureCard.GameTagOverrides.Add(new GameTagValueDataModel
			{
				GameTag = GAME_TAG.MERCENARIES_TREASURE_SCALE_LEVEL,
				Value = finalScalar
			});
			foreach (ScalingTreasureCardTagDbfRecord scalingTag in treasureRecord.Tags)
			{
				treasureCard.GameTagOverrides.Add(new GameTagValueDataModel
				{
					GameTag = (GAME_TAG)scalingTag.TagId,
					Value = entityDef.GetTag((GAME_TAG)scalingTag.TagId) + scalingTag.TagScaleValue * finalScalar,
					IsReferenceValue = scalingTag.IsReferenceTag,
					IsPowerKeywordTag = scalingTag.IsPowerKeywordTag
				});
			}
		}
		UpdateCardTag(treasureCard, GAME_TAG.CARD_NAME_DATA_1, 0, finalScalar + 1, isReferenceTag: false, isPowerKeywordTag: false, onlyInitial: false);
		return treasureCard;
	}

	public static void UpdateAbilityAffectedBySlottedEquipment(LettuceMercenaryDataModel dataModel, LettuceMercenary merc)
	{
		PopulateMercenaryDataModelAbilities(dataModel, merc);
		ApplyEquipmentBonuses(dataModel, merc, MercenaryDataPopluateExtra.None);
	}

	public static void UpdateReadyForUpgradeStatus(LettuceMercenaryDataModel dataModel, LettuceMercenary merc)
	{
		dataModel.ReadyForCrafting = merc.IsReadyForCrafting();
		bool upgradeAvailable = false;
		foreach (LettuceAbilityDataModel abilityData in dataModel.AbilityList)
		{
			abilityData.ReadyForUpgrade = merc.IsCardReadyForUpgrade(abilityData.AbilityId, MercenariesModeCardType.Ability);
			upgradeAvailable = abilityData.ReadyForUpgrade || upgradeAvailable;
		}
		foreach (LettuceAbilityDataModel equipmentData in dataModel.EquipmentList)
		{
			equipmentData.ReadyForUpgrade = merc.IsCardReadyForUpgrade(equipmentData.AbilityId, MercenariesModeCardType.Equipment);
			upgradeAvailable = equipmentData.ReadyForUpgrade || upgradeAvailable;
		}
		dataModel.ChildUpgradeAvailable = upgradeAvailable;
	}

	public static bool FindTextInCollectible(ICollectible collectible, string searchText)
	{
		searchText = searchText.Trim();
		if (collectible.GetSearchableTokens().Contains(searchText))
		{
			return true;
		}
		return collectible.GetSearchableString().Search(searchText);
	}

	public static void PopulateCardNameData(EntityDef entityDef, CardDataModel cardDataModel)
	{
		int tier = ((!entityDef.IsLettuceEquipment()) ? entityDef.GetTag(GAME_TAG.LETTUCE_ABILITY_TIER) : entityDef.GetTag(GAME_TAG.LETTUCE_EQUIPMENT_TIER));
		cardDataModel.GameTagOverrides.Add(new GameTagValueDataModel
		{
			GameTag = GAME_TAG.CARD_NAME_DATA_1,
			Value = tier
		});
	}
}
