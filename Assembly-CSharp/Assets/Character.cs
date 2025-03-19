using Blizzard.T5.Core.Utils;

namespace Assets;

public static class Character
{
	public enum CharacterGender
	{
		MALE,
		FEMALE,
		NON_BINARY
	}

	public enum CharacterRace
	{
		INVALID,
		BLOOD_ELF,
		DEMON,
		DRAENEI,
		DRAGON,
		DWARF,
		GNOME,
		GOBLIN,
		HUMAN,
		NIGHT_ELF,
		ORC,
		PANDAREN,
		TAUREN,
		TROLL,
		UNDEAD,
		WORGEN,
		VOID_ELF,
		DARK_IRON_DWARF,
		HIGHMOUNTAIN_TAUREN,
		KUL_TIRAN,
		LIGHTFORGED_DRAENEI,
		MAGHAR_ORC,
		MECHAGNOME,
		NIGHTBORNE,
		VULPERA,
		ZANDALARI_TROLL
	}

	public enum CharacterClass
	{
		INVALID,
		WARRIOR,
		SHAMAN,
		ROGUE,
		PALADIN,
		HUNTER,
		DRUID,
		WARLOCK,
		MAGE,
		PRIEST,
		DEMON_HUNTER,
		DEATH_KNIGHT
	}

	public static CharacterGender ParseCharacterGenderValue(string value)
	{
		EnumUtils.TryGetEnum<CharacterGender>(value, out var e);
		return e;
	}

	public static CharacterRace ParseCharacterRaceValue(string value)
	{
		EnumUtils.TryGetEnum<CharacterRace>(value, out var e);
		return e;
	}

	public static CharacterClass ParseCharacterClassValue(string value)
	{
		EnumUtils.TryGetEnum<CharacterClass>(value, out var e);
		return e;
	}
}
