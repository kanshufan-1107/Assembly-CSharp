using System.Collections.Generic;
using UnityEngine;

[CustomEditClass]
public class CardColorSwitcher : MonoBehaviour
{
	public enum CardColorType
	{
		TYPE_GENERIC = 0,
		TYPE_WARLOCK = 1,
		TYPE_ROGUE = 2,
		TYPE_DRUID = 3,
		TYPE_SHAMAN = 4,
		TYPE_HUNTER = 5,
		TYPE_MAGE = 6,
		TYPE_PALADIN = 7,
		TYPE_PRIEST = 8,
		TYPE_WARRIOR = 9,
		TYPE_DEATHKNIGHT = 10,
		TYPE_DEMONHUNTER = 11,
		TYPE_PALADIN_PRIEST = 12,
		TYPE_WARLOCK_PRIEST = 13,
		TYPE_WARLOCK_DEMONHUNTER = 14,
		TYPE_HUNTER_DEMONHUNTER = 15,
		TYPE_DRUID_HUNTER = 16,
		TYPE_DRUID_SHAMAN = 17,
		TYPE_SHAMAN_MAGE = 18,
		TYPE_MAGE_ROGUE = 19,
		TYPE_WARRIOR_ROGUE = 20,
		TYPE_WARRIOR_PALADIN = 21,
		TYPE_MERCENARIES_CASTER_TIER_1 = 22,
		TYPE_MERCENARIES_CASTER_TIER_2 = 23,
		TYPE_MERCENARIES_CASTER_TIER_3 = 24,
		TYPE_MERCENARIES_FIGHTER_TIER_1 = 25,
		TYPE_MERCENARIES_FIGHTER_TIER_2 = 26,
		TYPE_MERCENARIES_FIGHTER_TIER_3 = 27,
		TYPE_MERCENARIES_TANK_TIER_1 = 28,
		TYPE_MERCENARIES_TANK_TIER_2 = 29,
		TYPE_MERCENARIES_TANK_TIER_3 = 30,
		TYPE_MERCENARIES_NEUTRAL_TIER_1 = 31,
		TYPE_MERCENARIES_NEUTRAL_TIER_2 = 32,
		TYPE_MERCENARIES_NEUTRAL_TIER_3 = 33,
		TYPE_MERCENARIES_ABILITY_CASTER_SPELL = 31,
		TYPE_MERCENARIES_ABILITY_CASTER_MINION = 32,
		TYPE_MERCENARIES_ABILITY_FIGHTER_SPELL = 33,
		TYPE_MERCENARIES_ABILITY_FIGHTER_MINION = 34,
		TYPE_MERCENARIES_ABILITY_TANK_SPELL = 35,
		TYPE_MERCENARIES_ABILITY_TANK_MINION = 36,
		TYPE_MERCENARIES_ABILITY_NEUTRAL_SPELL = 37,
		TYPE_MERCENARIES_ABILITY_NEUTRAL_MINION = 38,
		TYPE_MAGE_HUNTER = 39,
		TYPE_HUNTER_DEATHKNIGHT = 40,
		TYPE_DEATHKNIGHT_PALADIN = 41,
		TYPE_PALADIN_SHAMAN = 42,
		TYPE_SHAMAN_WARRIOR = 43,
		TYPE_WARRIOR_DEMONHUNTER = 44,
		TYPE_DEMONHUNTER_ROGUE = 45,
		TYPE_ROGUE_PRIEST = 46,
		TYPE_PRIEST_DRUID = 47,
		TYPE_DRUID_WARLOCK = 48,
		TYPE_WARLOCK_MAGE = 49,
		TYPE_WARRIOR_WARLOCK = 50,
		TYPE_BATTLEGROUNDS_TRINKET_FRIENDLY = 51,
		TYPE_BATTLEGROUNDS_TRINKET_OPPONENT = 52
	}

	public enum FactionColorType
	{
		TYPE_GENERIC,
		TYPE_GRIMY_GOONS,
		TYPE_KABAL,
		TYPE_JADE_LOTUS,
		TYPE_ZERG,
		TYPE_TERRAN,
		TYPE_PROTOSS
	}

	private static CardColorSwitcher s_instance;

	[CustomEditField(Sections = "Spells", T = EditType.TEXTURE)]
	public List<string> spellCardTextures;

	[CustomEditField(Sections = "Minions", T = EditType.TEXTURE)]
	public List<string> minionCardTextures;

	[CustomEditField(Sections = "Heroes", T = EditType.TEXTURE)]
	public List<string> heroCardTextures;

	[CustomEditField(Sections = "Weapons", T = EditType.TEXTURE)]
	public List<string> weaponCardTextures;

	[CustomEditField(Sections = "Factions", T = EditType.MATERIAL)]
	public List<string> factionIconMaterials;

	[CustomEditField(Sections = "Factions", T = EditType.MATERIAL)]
	public List<string> factionIconMaterialsSignature;

	[CustomEditField(Sections = "Factions", T = EditType.MATERIAL)]
	public List<string> factionBannerMaterials;

	[CustomEditField(Sections = "Mercenaries Abilities", T = EditType.TEXTURE)]
	public List<string> mercenariesAbilityCardTextures;

	[CustomEditField(Sections = "Locations", T = EditType.TEXTURE)]
	public List<string> locationCardTextures;

	[CustomEditField(Sections = "Battlegrounds Spells", T = EditType.TEXTURE)]
	public List<string> battlegroundsSpellCardTextures;

	[CustomEditField(Sections = "Battlegrounds Trinkets", T = EditType.TEXTURE)]
	public List<string> battlegroundsTrinketCardTextures;

	[CustomEditField(Sections = "Battlegrounds Anomalies", T = EditType.TEXTURE)]
	public List<string> battlegroundsAnomalyCardTextures;

	private void Awake()
	{
		s_instance = this;
		base.gameObject.AddComponent<HSDontDestroyOnLoad>();
	}

	private void OnDestroy()
	{
		s_instance = null;
	}

	public static CardColorSwitcher Get()
	{
		return s_instance;
	}

	public static CardColorType GetCardColorTypeForClasses(List<TAG_CLASS> classes)
	{
		if (classes == null)
		{
			return CardColorType.TYPE_GENERIC;
		}
		if (classes.Count == 0)
		{
			return CardColorType.TYPE_GENERIC;
		}
		if (classes.Count == 1)
		{
			return GetCardColorTypeForClass(classes[0]);
		}
		if (classes.Count >= 3)
		{
			return CardColorType.TYPE_GENERIC;
		}
		if (classes.Contains(TAG_CLASS.PALADIN) && classes.Contains(TAG_CLASS.PRIEST))
		{
			return CardColorType.TYPE_PALADIN_PRIEST;
		}
		if (classes.Contains(TAG_CLASS.WARLOCK) && classes.Contains(TAG_CLASS.PRIEST))
		{
			return CardColorType.TYPE_WARLOCK_PRIEST;
		}
		if (classes.Contains(TAG_CLASS.WARLOCK) && classes.Contains(TAG_CLASS.DEMONHUNTER))
		{
			return CardColorType.TYPE_WARLOCK_DEMONHUNTER;
		}
		if (classes.Contains(TAG_CLASS.HUNTER) && classes.Contains(TAG_CLASS.DEMONHUNTER))
		{
			return CardColorType.TYPE_HUNTER_DEMONHUNTER;
		}
		if (classes.Contains(TAG_CLASS.DRUID) && classes.Contains(TAG_CLASS.HUNTER))
		{
			return CardColorType.TYPE_DRUID_HUNTER;
		}
		if (classes.Contains(TAG_CLASS.DRUID) && classes.Contains(TAG_CLASS.SHAMAN))
		{
			return CardColorType.TYPE_DRUID_SHAMAN;
		}
		if (classes.Contains(TAG_CLASS.SHAMAN) && classes.Contains(TAG_CLASS.MAGE))
		{
			return CardColorType.TYPE_SHAMAN_MAGE;
		}
		if (classes.Contains(TAG_CLASS.MAGE) && classes.Contains(TAG_CLASS.ROGUE))
		{
			return CardColorType.TYPE_MAGE_ROGUE;
		}
		if (classes.Contains(TAG_CLASS.WARRIOR) && classes.Contains(TAG_CLASS.ROGUE))
		{
			return CardColorType.TYPE_WARRIOR_ROGUE;
		}
		if (classes.Contains(TAG_CLASS.WARRIOR) && classes.Contains(TAG_CLASS.PALADIN))
		{
			return CardColorType.TYPE_WARRIOR_PALADIN;
		}
		if (classes.Contains(TAG_CLASS.MAGE) && classes.Contains(TAG_CLASS.HUNTER))
		{
			return CardColorType.TYPE_MAGE_HUNTER;
		}
		if (classes.Contains(TAG_CLASS.HUNTER) && classes.Contains(TAG_CLASS.DEATHKNIGHT))
		{
			return CardColorType.TYPE_HUNTER_DEATHKNIGHT;
		}
		if (classes.Contains(TAG_CLASS.DEATHKNIGHT) && classes.Contains(TAG_CLASS.PALADIN))
		{
			return CardColorType.TYPE_DEATHKNIGHT_PALADIN;
		}
		if (classes.Contains(TAG_CLASS.PALADIN) && classes.Contains(TAG_CLASS.SHAMAN))
		{
			return CardColorType.TYPE_PALADIN_SHAMAN;
		}
		if (classes.Contains(TAG_CLASS.SHAMAN) && classes.Contains(TAG_CLASS.WARRIOR))
		{
			return CardColorType.TYPE_SHAMAN_WARRIOR;
		}
		if (classes.Contains(TAG_CLASS.WARRIOR) && classes.Contains(TAG_CLASS.DEMONHUNTER))
		{
			return CardColorType.TYPE_WARRIOR_DEMONHUNTER;
		}
		if (classes.Contains(TAG_CLASS.DEMONHUNTER) && classes.Contains(TAG_CLASS.ROGUE))
		{
			return CardColorType.TYPE_DEMONHUNTER_ROGUE;
		}
		if (classes.Contains(TAG_CLASS.ROGUE) && classes.Contains(TAG_CLASS.PRIEST))
		{
			return CardColorType.TYPE_ROGUE_PRIEST;
		}
		if (classes.Contains(TAG_CLASS.PRIEST) && classes.Contains(TAG_CLASS.DRUID))
		{
			return CardColorType.TYPE_PRIEST_DRUID;
		}
		if (classes.Contains(TAG_CLASS.DRUID) && classes.Contains(TAG_CLASS.WARLOCK))
		{
			return CardColorType.TYPE_DRUID_WARLOCK;
		}
		if (classes.Contains(TAG_CLASS.WARLOCK) && classes.Contains(TAG_CLASS.MAGE))
		{
			return CardColorType.TYPE_WARLOCK_MAGE;
		}
		if (classes.Contains(TAG_CLASS.WARRIOR) && classes.Contains(TAG_CLASS.WARLOCK))
		{
			return CardColorType.TYPE_WARRIOR_WARLOCK;
		}
		return CardColorType.TYPE_GENERIC;
	}

	public static CardColorType GetCardColorTypeForClass(TAG_CLASS classType)
	{
		CardColorType colorType = CardColorType.TYPE_GENERIC;
		switch (classType)
		{
		case TAG_CLASS.WARLOCK:
			colorType = CardColorType.TYPE_WARLOCK;
			break;
		case TAG_CLASS.ROGUE:
			colorType = CardColorType.TYPE_ROGUE;
			break;
		case TAG_CLASS.DRUID:
			colorType = CardColorType.TYPE_DRUID;
			break;
		case TAG_CLASS.HUNTER:
			colorType = CardColorType.TYPE_HUNTER;
			break;
		case TAG_CLASS.MAGE:
			colorType = CardColorType.TYPE_MAGE;
			break;
		case TAG_CLASS.PALADIN:
			colorType = CardColorType.TYPE_PALADIN;
			break;
		case TAG_CLASS.PRIEST:
			colorType = CardColorType.TYPE_PRIEST;
			break;
		case TAG_CLASS.SHAMAN:
			colorType = CardColorType.TYPE_SHAMAN;
			break;
		case TAG_CLASS.WARRIOR:
			colorType = CardColorType.TYPE_WARRIOR;
			break;
		case TAG_CLASS.DREAM:
			colorType = CardColorType.TYPE_HUNTER;
			break;
		case TAG_CLASS.DEATHKNIGHT:
			colorType = CardColorType.TYPE_DEATHKNIGHT;
			break;
		case TAG_CLASS.DEMONHUNTER:
			colorType = CardColorType.TYPE_DEMONHUNTER;
			break;
		}
		return colorType;
	}

	public static FactionColorType GetFactionColorTypeForTag(GAME_TAG tag)
	{
		FactionColorType colorType = FactionColorType.TYPE_GENERIC;
		return tag switch
		{
			GAME_TAG.GRIMY_GOONS => FactionColorType.TYPE_GRIMY_GOONS, 
			GAME_TAG.KABAL => FactionColorType.TYPE_KABAL, 
			GAME_TAG.JADE_LOTUS => FactionColorType.TYPE_JADE_LOTUS, 
			GAME_TAG.ZERG => FactionColorType.TYPE_ZERG, 
			GAME_TAG.TERRAN => FactionColorType.TYPE_TERRAN, 
			GAME_TAG.PROTOSS => FactionColorType.TYPE_PROTOSS, 
			_ => colorType, 
		};
	}

	public AssetReference GetTexture(TAG_CARDTYPE cardType, CardColorType colorType)
	{
		List<string> textures;
		switch (cardType)
		{
		case TAG_CARDTYPE.MINION:
		case TAG_CARDTYPE.BATTLEGROUND_HERO_BUDDY:
			textures = minionCardTextures;
			break;
		case TAG_CARDTYPE.LOCATION:
			textures = locationCardTextures;
			break;
		case TAG_CARDTYPE.SPELL:
			textures = spellCardTextures;
			break;
		case TAG_CARDTYPE.HERO:
			textures = heroCardTextures;
			break;
		case TAG_CARDTYPE.WEAPON:
			textures = weaponCardTextures;
			break;
		case TAG_CARDTYPE.LETTUCE_ABILITY:
			textures = mercenariesAbilityCardTextures;
			break;
		case TAG_CARDTYPE.BATTLEGROUND_SPELL:
			textures = battlegroundsSpellCardTextures;
			break;
		case TAG_CARDTYPE.BATTLEGROUND_TRINKET:
			textures = battlegroundsTrinketCardTextures;
			break;
		case TAG_CARDTYPE.BATTLEGROUND_ANOMALY:
			textures = battlegroundsAnomalyCardTextures;
			break;
		case TAG_CARDTYPE.INVALID:
		case TAG_CARDTYPE.BATTLEGROUND_QUEST_REWARD:
			return null;
		default:
			Debug.LogErrorFormat("Wrong cardType {0}", cardType);
			textures = minionCardTextures;
			break;
		}
		if (textures.Count <= (int)colorType)
		{
			return null;
		}
		return textures[(int)colorType];
	}

	public AssetReference GetMaterialIcon(TAG_PREMIUM premium, FactionColorType colorType)
	{
		List<string> textures = ((premium != TAG_PREMIUM.SIGNATURE) ? factionIconMaterials : factionIconMaterialsSignature);
		if ((int)colorType >= textures.Count)
		{
			return null;
		}
		return textures[(int)colorType];
	}

	public AssetReference GetMaterialBanner(FactionColorType colorType)
	{
		if ((int)colorType >= factionBannerMaterials.Count)
		{
			return null;
		}
		return factionBannerMaterials[(int)colorType];
	}
}
