using System;
using System.Collections.Generic;
using System.Linq;
using Blizzard.T5.Core;
using PegasusGame;
using UnityEngine;

public abstract class EntityBase
{
	protected static int DEFAULT_TAG_MAP_SIZE = 15;

	protected TagMap m_tags;

	protected TagMap m_cachedTagsForDormant;

	protected TagListMap m_tagLists;

	private string m_cardIdInternal;

	private List<TAG_RACE> m_entityRaces;

	private static List<TAG_CLASS> s_allowedClasses = new List<TAG_CLASS>();

	protected string m_cardId
	{
		get
		{
			return m_cardIdInternal;
		}
		set
		{
			m_cardIdInternal = value;
		}
	}

	public bool HasRuneCost => GetBloodCost() + GetFrostCost() + GetUnholyCost() > 0;

	public bool HasSideboard => GetMaxSideboardCards() > 0;

	public bool IsCollectionManagerFilterManaCostByEven => GetTag(GAME_TAG.COLLECTIONMANAGER_FILTER_MANA_EVEN) != 0;

	public bool IsCollectionManagerFilterManaCostByOdd => GetTag(GAME_TAG.COLLECTIONMANAGER_FILTER_MANA_ODD) != 0;

	public EntityBase()
	{
		m_tags = new TagMap(DEFAULT_TAG_MAP_SIZE);
		m_cachedTagsForDormant = new TagMap();
		m_tagLists = new TagListMap();
	}

	public EntityBase(int tagMapSize)
	{
		m_tags = new TagMap(tagMapSize);
		m_cachedTagsForDormant = new TagMap();
		m_tagLists = new TagListMap();
	}

	public bool HasTag(GAME_TAG tag)
	{
		return GetTag(tag) > 0;
	}

	public TagMap GetTags()
	{
		return m_tags;
	}

	public int GetTag(int tag)
	{
		return m_tags.GetTag(tag);
	}

	public int GetTag(GAME_TAG enumTag)
	{
		return m_tags.GetTag((int)enumTag);
	}

	public TagEnum GetTag<TagEnum>(GAME_TAG enumTag)
	{
		int val = GetTag(enumTag);
		return (TagEnum)Enum.ToObject(typeof(TagEnum), val);
	}

	public void SetTag(int tag, int tagValue)
	{
		m_tags.SetTag(tag, tagValue);
	}

	public void SetTag(GAME_TAG tag, int tagValue)
	{
		SetTag((int)tag, tagValue);
	}

	public void SetTag<TagEnum>(GAME_TAG tag, TagEnum tagValue)
	{
		SetTag((int)tag, Convert.ToInt32(tagValue));
	}

	public void SetTags(Map<GAME_TAG, int> tagMap)
	{
		m_tags.SetTags(tagMap);
	}

	public void SetTags(List<Network.Entity.Tag> tags)
	{
		m_tags.SetTags(tags);
	}

	public void SetTags(List<Tag> tags)
	{
		m_tags.SetTags(tags);
	}

	public void SetTagLists(List<Network.Entity.TagList> tagLists)
	{
		foreach (Network.Entity.TagList tagList in tagLists)
		{
			if (tagList != null)
			{
				m_tagLists.SetValues(tagList.Name, tagList.Values);
			}
		}
	}

	public void SetTagList(int tag, List<int> values)
	{
		m_tagLists.SetValues(tag, values);
	}

	public void ReplaceTags(TagMap tags)
	{
		m_tags.Replace(tags);
	}

	public bool HasReferencedTag(GAME_TAG enumTag)
	{
		return GetReferencedTag(enumTag) > 0;
	}

	public int GetReferencedTag(GAME_TAG enumTag)
	{
		return GetReferencedTag((int)enumTag);
	}

	public abstract int GetReferencedTag(int tag);

	public bool HasCachedTagForDormant(GAME_TAG tag)
	{
		return GetCachedTagForDormant(tag) > 0;
	}

	public int GetCachedTagForDormant(GAME_TAG enumTag)
	{
		return m_cachedTagsForDormant.GetTag((int)enumTag);
	}

	public void SetCachedTagForDormant(int tag, int tagValue)
	{
		m_cachedTagsForDormant.SetTag(tag, tagValue);
	}

	public bool HasAvenge()
	{
		return HasTag(GAME_TAG.AVENGE);
	}

	public bool HasCharge()
	{
		return HasTag(GAME_TAG.CHARGE);
	}

	public bool HasBattlecry()
	{
		return HasTag(GAME_TAG.BATTLECRY);
	}

	public bool CanBeTargetedBySpells()
	{
		if (!HasTag(GAME_TAG.CANT_BE_TARGETED_BY_SPELLS))
		{
			return !HasTag(GAME_TAG.UNTOUCHABLE);
		}
		return false;
	}

	public bool CanBeTargetedByHeroPowers()
	{
		if (!HasTag(GAME_TAG.CANT_BE_TARGETED_BY_HERO_POWERS))
		{
			return !HasTag(GAME_TAG.UNTOUCHABLE);
		}
		return false;
	}

	public bool HasTriggerVisual()
	{
		return HasTag(GAME_TAG.TRIGGER_VISUAL);
	}

	public bool HasInspire()
	{
		return HasTag(GAME_TAG.INSPIRE);
	}

	public bool HasOverKill()
	{
		return HasTag(GAME_TAG.OVERKILL);
	}

	public bool HasSpellburst()
	{
		if (!HasTag(GAME_TAG.SPELLBURST))
		{
			return HasTag(GAME_TAG.NON_KEYWORD_SPELLBURST);
		}
		return true;
	}

	public bool HasFrenzy()
	{
		return HasTag(GAME_TAG.FRENZY);
	}

	public bool HasHonorableKill()
	{
		return HasTag(GAME_TAG.HONORABLE_KILL);
	}

	public bool HasCounter()
	{
		return HasTag(GAME_TAG.COUNTER);
	}

	public bool IsImmune()
	{
		if (!HasTag(GAME_TAG.IMMUNE))
		{
			return HasTag(GAME_TAG.UNTOUCHABLE);
		}
		return true;
	}

	public bool IsPoisonous()
	{
		if (!HasTag(GAME_TAG.POISONOUS))
		{
			return HasTag(GAME_TAG.NON_KEYWORD_POISONOUS);
		}
		return true;
	}

	public bool IsVenomous()
	{
		return HasTag(GAME_TAG.VENOMOUS);
	}

	public bool HasLifesteal()
	{
		return HasTag(GAME_TAG.LIFESTEAL);
	}

	public bool HasOverheal()
	{
		return HasTag(GAME_TAG.OVERHEAL);
	}

	public bool HasEndOfTurnTrigger()
	{
		return HasTag(GAME_TAG.END_OF_TURN_TRIGGER);
	}

	public bool IsEnraged()
	{
		if (HasTag(GAME_TAG.ENRAGED))
		{
			return GetDamage() > 0;
		}
		return false;
	}

	public int GetDamage()
	{
		return GetTag(GAME_TAG.DAMAGE);
	}

	public bool IsFrozen()
	{
		return HasTag(GAME_TAG.FROZEN);
	}

	public bool IsDormant()
	{
		return HasTag(GAME_TAG.DORMANT);
	}

	public bool IsAsleep()
	{
		if (HasTag(GAME_TAG.SHOW_SLEEP_ZZZ_OVERRIDE))
		{
			return true;
		}
		if (GetNumTurnsInPlay() != 0)
		{
			return false;
		}
		if (GetNumAttacksThisTurn() != 0)
		{
			return false;
		}
		if (HasCharge())
		{
			return false;
		}
		if (HasRush())
		{
			return false;
		}
		if (ReferencesAutoAttack())
		{
			return false;
		}
		if (HasTag(GAME_TAG.UNTOUCHABLE))
		{
			return false;
		}
		if (IsLocation())
		{
			return false;
		}
		if (GameState.Get() != null && GameState.Get().GetGameEntity() != null && !GameState.Get().GetBooleanGameOption(GameEntityOption.ALLOW_SLEEP_FX))
		{
			return false;
		}
		if (IsTitan())
		{
			return false;
		}
		return true;
	}

	public bool IsStealthed()
	{
		return HasTag(GAME_TAG.STEALTH);
	}

	public bool HasTaunt()
	{
		return HasTag(GAME_TAG.TAUNT);
	}

	public bool HasDivineShield()
	{
		return HasTag(GAME_TAG.DIVINE_SHIELD);
	}

	public bool ReferencesAutoAttack()
	{
		return HasReferencedTag(GAME_TAG.AUTO_ATTACK);
	}

	public bool IsHero()
	{
		return GetTag(GAME_TAG.CARDTYPE) == 3;
	}

	public bool IsHeroPower()
	{
		return GetTag(GAME_TAG.CARDTYPE) == 10;
	}

	public bool IsGameModeButton()
	{
		return GetTag(GAME_TAG.CARDTYPE) == 12;
	}

	public bool IsLettuceAbility()
	{
		return GetTag(GAME_TAG.CARDTYPE) == 23;
	}

	public bool IsLettuceEquipment()
	{
		if (IsLettuceAbility())
		{
			return HasTag(GAME_TAG.LETTUCE_IS_EQUPIMENT);
		}
		return false;
	}

	public bool IsLettuceAbilitySpellCasting()
	{
		if (IsLettuceAbility())
		{
			return !HasTag(GAME_TAG.LETTUCE_ABILITY_SUMMONED_MINION);
		}
		return false;
	}

	public bool IsLettuceAbilityMinionSummoning()
	{
		if (IsLettuceAbility())
		{
			return HasTag(GAME_TAG.LETTUCE_ABILITY_SUMMONED_MINION);
		}
		return false;
	}

	public bool IsLettuceMercenary()
	{
		return GetTag(GAME_TAG.LETTUCE_MERCENARY) > 0;
	}

	public bool IsMinion()
	{
		return GetTag(GAME_TAG.CARDTYPE) == 4;
	}

	public bool IsTitan()
	{
		return GetTag(GAME_TAG.TITAN) != 0;
	}

	public bool IsLaunchpad()
	{
		return GetTag(GAME_TAG.LAUNCHPAD) != 0;
	}

	public bool IsStarship()
	{
		return GetTag(GAME_TAG.STARSHIP) != 0;
	}

	public bool IsStarshipLaunchAbility()
	{
		return GetTag(GAME_TAG.TAG_LAUNCHPAD_ABILITY) != 0;
	}

	public bool IsSpell()
	{
		return GetTag(GAME_TAG.CARDTYPE) == 5;
	}

	public bool IsWeapon()
	{
		return GetTag(GAME_TAG.CARDTYPE) == 7;
	}

	public bool IsLocation()
	{
		return GetTag(GAME_TAG.CARDTYPE) == 39;
	}

	public bool IsAnomaly()
	{
		return GetTag(GAME_TAG.CARDTYPE) == 43;
	}

	public bool IsBaconSpell()
	{
		return GetTag(GAME_TAG.CARDTYPE) == 42;
	}

	public bool IsCutsceneEntity()
	{
		return HasTag(GAME_TAG.CUTSCENE_CARD_TYPE);
	}

	public int GetLocationCooldown()
	{
		return GetTag(GAME_TAG.EXHAUSTED) + GetTag(GAME_TAG.LOCATION_ACTION_COOLDOWN);
	}

	public bool IsElite()
	{
		return GetTag(GAME_TAG.ELITE) > 0;
	}

	public bool IsHeroSkin()
	{
		if (!IsHero())
		{
			return false;
		}
		CardDbfRecord cardRecord = GameDbf.GetIndex().GetCardRecord(m_cardIdInternal);
		if (cardRecord == null)
		{
			return false;
		}
		return GameUtils.GetCardHeroRecordForCardId(cardRecord.ID) != null;
	}

	public bool IsCoinBasedHeroBuddy()
	{
		GameEntity gameEntity = GameState.Get()?.GetGameEntity();
		if (IsBattlegroundHeroBuddy() && gameEntity != null)
		{
			return gameEntity.GetTag(GAME_TAG.BACON_USE_COIN_BASED_BUDDY_METER) != 0;
		}
		return false;
	}

	public bool IsCardButton()
	{
		if (!IsHeroPower() && !IsLocation() && !IsGameModeButton() && !IsLettuceAbility())
		{
			return IsCoinBasedHeroBuddy();
		}
		return true;
	}

	public bool IsMoveMinionHoverTarget()
	{
		return GetTag(GAME_TAG.CARDTYPE) == 22;
	}

	public bool IsBattlegroundHeroBuddy()
	{
		return GetTag(GAME_TAG.CARDTYPE) == 24;
	}

	public bool IsBattlegroundTrinket()
	{
		return GetTag(GAME_TAG.CARDTYPE) == 44;
	}

	public bool IsBattlegroundQuestReward()
	{
		return GetTag(GAME_TAG.CARDTYPE) == 40;
	}

	public bool IsCustomCoin()
	{
		CardDbfRecord cardRecord = GameDbf.GetIndex().GetCardRecord(m_cardIdInternal);
		return GameDbf.CosmeticCoin.HasRecord((CosmeticCoinDbfRecord coin) => coin.CardId == cardRecord.ID);
	}

	public TAG_CARDTYPE GetCardType()
	{
		return (TAG_CARDTYPE)GetTag(GAME_TAG.CARDTYPE);
	}

	public TAG_PUZZLE_TYPE GetPuzzleType()
	{
		return (TAG_PUZZLE_TYPE)GetTag(GAME_TAG.PUZZLE_TYPE);
	}

	public bool IsGame()
	{
		return GetTag(GAME_TAG.CARDTYPE) == 1;
	}

	public bool IsPlayer()
	{
		return GetTag(GAME_TAG.CARDTYPE) == 2;
	}

	public bool IsExhausted()
	{
		return HasTag(GAME_TAG.EXHAUSTED);
	}

	public bool IsAttached()
	{
		return HasTag(GAME_TAG.ATTACHED);
	}

	public bool HasSecretDeathrattle()
	{
		return HasTag(GAME_TAG.SECRET_DEATHRATTLE);
	}

	public bool IsSecret()
	{
		if (HasTag(GAME_TAG.SECRET))
		{
			return !IsBaconSpell();
		}
		return false;
	}

	public bool IsBobQuest()
	{
		return HasTag(GAME_TAG.BACON_IS_BOB_QUEST);
	}

	public bool IsQuest()
	{
		if (HasTag(GAME_TAG.QUEST))
		{
			return !IsBaconSpell();
		}
		return false;
	}

	public bool IsQuestline()
	{
		if (HasTag(GAME_TAG.QUESTLINE))
		{
			return !IsBaconSpell();
		}
		return false;
	}

	public bool IsQuestOrQuestline()
	{
		if (IsBaconSpell())
		{
			return false;
		}
		if (!HasTag(GAME_TAG.QUEST))
		{
			return HasTag(GAME_TAG.QUESTLINE);
		}
		return true;
	}

	public bool IsSideQuest()
	{
		if (HasTag(GAME_TAG.SIDE_QUEST))
		{
			return !IsBaconSpell();
		}
		return false;
	}

	public bool IsSigil()
	{
		if (HasTag(GAME_TAG.SIGIL))
		{
			return !IsBaconSpell();
		}
		return false;
	}

	public bool IsObjective()
	{
		if (HasTag(GAME_TAG.OBJECTIVE))
		{
			return !IsBaconSpell();
		}
		return false;
	}

	public bool IsPuzzle()
	{
		if (HasTag(GAME_TAG.PUZZLE))
		{
			return !IsBaconSpell();
		}
		return false;
	}

	public bool IsRulebook()
	{
		return HasTag(GAME_TAG.RULEBOOK);
	}

	public bool IsSecretLike()
	{
		if (!IsSecret() && !IsQuest() && !IsQuestline() && !IsSideQuest() && !IsSigil())
		{
			return IsObjective();
		}
		return true;
	}

	public bool IsRevealed()
	{
		return HasTag(GAME_TAG.REVEALED);
	}

	public bool HasReplacementsWhenPlayed()
	{
		if (m_tagLists == null)
		{
			return false;
		}
		List<int> replacementsWhenPlayed = GetReplacementsWhenPlayed();
		if (replacementsWhenPlayed == null)
		{
			return false;
		}
		return replacementsWhenPlayed.Count > 0;
	}

	public List<int> GetReplacementsWhenPlayed()
	{
		if (!m_tagLists.TryGetValue(3677, out var replacementsWhenPlayed))
		{
			return null;
		}
		if (replacementsWhenPlayed == null)
		{
			return null;
		}
		return replacementsWhenPlayed;
	}

	public int GetNumTurnsInPlay()
	{
		return GetTag(GAME_TAG.NUM_TURNS_IN_PLAY);
	}

	public int GetNumAttacksThisTurn()
	{
		return GetTag(GAME_TAG.NUM_ATTACKS_THIS_TURN);
	}

	public TAG_SPELL_SCHOOL GetSpellPowerSchool()
	{
		if (HasTag(GAME_TAG.SPELLPOWER))
		{
			return TAG_SPELL_SCHOOL.NONE;
		}
		if (HasTag(GAME_TAG.SPELLPOWER_ARCANE))
		{
			return TAG_SPELL_SCHOOL.ARCANE;
		}
		if (HasTag(GAME_TAG.SPELLPOWER_FIRE))
		{
			return TAG_SPELL_SCHOOL.FIRE;
		}
		if (HasTag(GAME_TAG.SPELLPOWER_FROST))
		{
			return TAG_SPELL_SCHOOL.FROST;
		}
		if (HasTag(GAME_TAG.SPELLPOWER_NATURE))
		{
			return TAG_SPELL_SCHOOL.NATURE;
		}
		if (HasTag(GAME_TAG.SPELLPOWER_HOLY))
		{
			return TAG_SPELL_SCHOOL.HOLY;
		}
		if (HasTag(GAME_TAG.SPELLPOWER_SHADOW))
		{
			return TAG_SPELL_SCHOOL.SHADOW;
		}
		if (HasTag(GAME_TAG.SPELLPOWER_FEL))
		{
			return TAG_SPELL_SCHOOL.FEL;
		}
		if (HasTag(GAME_TAG.SPELLPOWER_PHYSICAL))
		{
			return TAG_SPELL_SCHOOL.PHYSICAL_COMBAT;
		}
		return TAG_SPELL_SCHOOL.NONE;
	}

	public bool HasSpellPower()
	{
		if (!HasTag(GAME_TAG.SPELLPOWER) && !HasTag(GAME_TAG.SPELLPOWER_ARCANE) && !HasTag(GAME_TAG.SPELLPOWER_FIRE) && !HasTag(GAME_TAG.SPELLPOWER_FROST) && !HasTag(GAME_TAG.SPELLPOWER_NATURE) && !HasTag(GAME_TAG.SPELLPOWER_HOLY) && !HasTag(GAME_TAG.SPELLPOWER_SHADOW) && !HasTag(GAME_TAG.SPELLPOWER_FEL))
		{
			return HasTag(GAME_TAG.SPELLPOWER_PHYSICAL);
		}
		return true;
	}

	public bool HasHeroPowerDamage()
	{
		return HasTag(GAME_TAG.HEROPOWER_DAMAGE);
	}

	public bool IsAffectedBySpellPower()
	{
		return HasTag(GAME_TAG.AFFECTED_BY_SPELL_POWER);
	}

	public bool HasSpellPowerDouble()
	{
		return HasTag(GAME_TAG.SPELLPOWER_DOUBLE);
	}

	public bool HasHealingDoesDamageHint()
	{
		return HasTag(GAME_TAG.HEALING_DOES_DAMAGE_HINT);
	}

	public bool HasLifestealDoesDamageHint()
	{
		return HasTag(GAME_TAG.LIFESTEAL_DOES_DAMAGE_HINT);
	}

	public int GetCost()
	{
		return GetTag(GAME_TAG.COST);
	}

	public int GetATK()
	{
		return GetTag(GAME_TAG.ATK);
	}

	public int GetHealth()
	{
		return GetTag(GAME_TAG.HEALTH);
	}

	public int GetDurability()
	{
		return GetTag(GAME_TAG.DURABILITY);
	}

	public int GetArmor()
	{
		return GetTag(GAME_TAG.ARMOR);
	}

	public int GetAttached()
	{
		return GetTag(GAME_TAG.ATTACHED);
	}

	public int GetBloodCost()
	{
		return GetTag(GAME_TAG.COST_BLOOD);
	}

	public int GetFrostCost()
	{
		return GetTag(GAME_TAG.COST_FROST);
	}

	public int GetUnholyCost()
	{
		return GetTag(GAME_TAG.COST_UNHOLY);
	}

	public RunePattern GetRuneCost()
	{
		return new RunePattern(GetBloodCost(), GetFrostCost(), GetUnholyCost());
	}

	public int GetMaxSideboardCards()
	{
		return GetTag(GAME_TAG.MAX_SIDEBOARD_CARDS);
	}

	public TAG_ZONE GetZone()
	{
		TAG_ZONE fakeZone = (TAG_ZONE)GetTag(GAME_TAG.FAKE_ZONE);
		if (fakeZone != 0)
		{
			return fakeZone;
		}
		return (TAG_ZONE)GetTag(GAME_TAG.ZONE);
	}

	public int GetZonePosition()
	{
		int fakePosition = GetTag(GAME_TAG.FAKE_ZONE_POSITION);
		if (fakePosition > 0)
		{
			return fakePosition;
		}
		return GetTag(GAME_TAG.ZONE_POSITION);
	}

	public int GetCreatorId()
	{
		return GetTag(GAME_TAG.CREATOR);
	}

	public int GetCreatorDBID()
	{
		return GetTag(GAME_TAG.CREATOR_DBID);
	}

	public int GetControllerId()
	{
		int fakeController = GetTag(GAME_TAG.FAKE_CONTROLLER);
		if (fakeController > 0)
		{
			return fakeController;
		}
		return GetTag(GAME_TAG.CONTROLLER);
	}

	public bool HasWindfury()
	{
		return GetTag(GAME_TAG.WINDFURY) > 0;
	}

	public bool HasCombo()
	{
		return HasTag(GAME_TAG.COMBO);
	}

	public bool HasDeathrattle()
	{
		return HasTag(GAME_TAG.DEATHRATTLE);
	}

	public bool IsSilenced()
	{
		return HasTag(GAME_TAG.SILENCED);
	}

	public int GetEntityId()
	{
		return GetTag(GAME_TAG.ENTITY_ID);
	}

	public bool IsCharacter()
	{
		if (!IsHero())
		{
			return IsMinion();
		}
		return true;
	}

	public bool HasRush()
	{
		return HasTag(GAME_TAG.RUSH);
	}

	public int GetTechLevel()
	{
		return GetTag(GAME_TAG.TECH_LEVEL);
	}

	public bool IsCoreCard()
	{
		TAG_CARD_SET cardSetId = GetCardSet();
		if (cardSetId == TAG_CARD_SET.INVALID)
		{
			return false;
		}
		CardSetDbfRecord cardSet = GameDbf.GetIndex().GetCardSet(cardSetId);
		if (cardSet == null)
		{
			Debug.LogWarning($"Got null card set ID: {cardSetId}");
			return false;
		}
		return cardSet.IsCoreCardSet;
	}

	public virtual TAG_CLASS GetClass()
	{
		s_allowedClasses.Clear();
		GetClasses(s_allowedClasses);
		int count = s_allowedClasses.Count;
		if (count == 0)
		{
			return TAG_CLASS.INVALID;
		}
		if (1 == count)
		{
			return s_allowedClasses[0];
		}
		return TAG_CLASS.NEUTRAL;
	}

	public virtual void GetClasses(IList<TAG_CLASS> classes)
	{
		classes.Clear();
		uint multiClassMap = (uint)GetTag(GAME_TAG.MULTIPLE_CLASSES);
		if (multiClassMap == 0)
		{
			TAG_CLASS classTag = (TAG_CLASS)GetTag(GAME_TAG.CLASS);
			if (classTag != 0)
			{
				classes.Add(classTag);
			}
			return;
		}
		int classAsInt = 1;
		while (multiClassMap != 0)
		{
			if (1 == (multiClassMap & 1))
			{
				classes.Add((TAG_CLASS)classAsInt);
			}
			multiClassMap >>= 1;
			classAsInt++;
		}
	}

	public bool HasClass(TAG_CLASS tagClass)
	{
		GetClasses(s_allowedClasses);
		return s_allowedClasses.Contains(tagClass);
	}

	public bool IsMultiClass()
	{
		GetClasses(s_allowedClasses);
		return s_allowedClasses.Count > 1;
	}

	public bool HasFaction()
	{
		if (!HasTag(GAME_TAG.PROTOSS) && !HasTag(GAME_TAG.TERRAN) && !HasTag(GAME_TAG.ZERG) && !HasTag(GAME_TAG.GRIMY_GOONS) && !HasTag(GAME_TAG.KABAL))
		{
			return HasTag(GAME_TAG.JADE_LOTUS);
		}
		return true;
	}

	public List<TAG_RACE> GetRaces()
	{
		if (m_entityRaces != null)
		{
			return m_entityRaces;
		}
		m_entityRaces = new List<TAG_RACE>();
		if (GetTag(GAME_TAG.CARDRACE) != 0)
		{
			m_entityRaces.Add((TAG_RACE)GetTag(GAME_TAG.CARDRACE));
		}
		foreach (CardRaceDbfRecord raceRecord in GameDbf.CardRace.GetRecords())
		{
			if (HasTag((GAME_TAG)raceRecord.IsRaceTag))
			{
				m_entityRaces.Add((TAG_RACE)raceRecord.ID);
			}
		}
		m_entityRaces = m_entityRaces.Distinct().ToList();
		if (m_entityRaces.Count > 1)
		{
			TAG_RACE[] order = new TAG_RACE[12]
			{
				TAG_RACE.UNDEAD,
				TAG_RACE.ELEMENTAL,
				TAG_RACE.MECHANICAL,
				TAG_RACE.DEMON,
				TAG_RACE.MURLOC,
				TAG_RACE.QUILBOAR,
				TAG_RACE.NAGA,
				TAG_RACE.PET,
				TAG_RACE.DRAGON,
				TAG_RACE.DRAENEI,
				TAG_RACE.TOTEM,
				TAG_RACE.PIRATE
			};
			m_entityRaces.Sort((TAG_RACE r1, TAG_RACE r2) => Array.IndexOf(order, r1).CompareTo(Array.IndexOf(order, r2)));
		}
		return m_entityRaces;
	}

	public int GetRaceCount()
	{
		return GetRaces().Count;
	}

	public bool HasDeckAction()
	{
		if (!IsTradeable() && !IsForgeable())
		{
			return IsPassable();
		}
		return true;
	}

	public string GetRaceText()
	{
		if (IsMinion() && HasTag(GAME_TAG.CARDRACE))
		{
			List<TAG_RACE> races = GetRaces();
			if (races.Count() > 0)
			{
				string raceString = "";
				foreach (TAG_RACE race in races)
				{
					raceString += GameStrings.GetRaceName(race);
					raceString += "\n";
				}
				return raceString.Remove(raceString.Length - 1);
			}
		}
		if ((IsSpell() || IsLettuceAbility() || IsBaconSpell() || IsBattlegroundTrinket()) && HasTag(GAME_TAG.SPELL_SCHOOL))
		{
			return GameStrings.GetSpellSchoolName(GetSpellSchool());
		}
		if (IsBattlegroundTrinket() && HasTag(GAME_TAG.TECH_LEVEL) && !HasTag(GAME_TAG.BACON_IS_POTENTIAL_TRINKET))
		{
			Log.All.PrintWarning($"Trinket missing spell school: {this}");
			int techLevel = GetTag(GAME_TAG.TECH_LEVEL);
			switch (techLevel)
			{
			case 4:
				return GameStrings.Get("GLOBAL_SPELL_SCHOOL_LESSER_TRINKET");
			case 6:
				return GameStrings.Get("GLOBAL_SPELL_SCHOOL_GREATER_TRINKET");
			}
			Log.All.PrintWarning($"Unsupported Battletrounds Trinket Tech Level: {techLevel}");
		}
		return "";
	}

	public bool IsTradeable()
	{
		return HasTag(GAME_TAG.TRADEABLE);
	}

	public bool IsForgeable()
	{
		return HasTag(GAME_TAG.FORGE);
	}

	public bool IsPassable()
	{
		return HasTag(GAME_TAG.BACON_DUO_PASSABLE);
	}

	public TAG_ROLE GetMercenaryRole()
	{
		return GetTag<TAG_ROLE>(GAME_TAG.LETTUCE_ROLE);
	}

	public string GetCardId()
	{
		return m_cardId;
	}

	public void SetCardId(string cardId)
	{
		m_cardId = cardId;
		OnUpdateCardId();
	}

	protected virtual void OnUpdateCardId()
	{
	}

	public TAG_CARD_SET GetCardSet()
	{
		TAG_CARD_SET cardSet = (TAG_CARD_SET)GetTag(GAME_TAG.CARD_SET);
		if (cardSet != 0)
		{
			return cardSet;
		}
		CardDbfRecord cardRecord = GameDbf.GetIndex().GetCardRecord(m_cardIdInternal);
		if (cardRecord != null)
		{
			List<CardSetTimingDbfRecord> cardSetTimingsForCard = GameUtils.GetCardSetTimingsForCard(cardRecord.ID);
			EventTimingManager eventTimingManager = EventTimingManager.Get();
			foreach (CardSetTimingDbfRecord record in cardSetTimingsForCard)
			{
				if (eventTimingManager == null || eventTimingManager.IsEventActive(record.EventTimingEvent))
				{
					return (TAG_CARD_SET)record.CardSetId;
				}
			}
		}
		return TAG_CARD_SET.INVALID;
	}

	public TAG_SPELL_SCHOOL GetSpellSchool()
	{
		return GetTag<TAG_SPELL_SCHOOL>(GAME_TAG.SPELL_SCHOOL);
	}

	public TAG_CARD_ALTERNATE_COST GetAlternateCost()
	{
		return GetTag<TAG_CARD_ALTERNATE_COST>(GAME_TAG.CARD_ALTERNATE_COST);
	}

	protected void RemoveCachedEntityRaces()
	{
		m_entityRaces = null;
	}
}
