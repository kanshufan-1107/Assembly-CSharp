using Blizzard.T5.Core.Utils;

namespace Assets;

public static class TavernGuideQuestSet
{
	public enum TavernGuideQuestDisplayType
	{
		UNKNOWN,
		QUEST_BOARD,
		CIRCULAR
	}

	public enum TavernGuideCategory
	{
		UNKNOWN,
		PROGRESSION,
		GAMEPLAY,
		COLLECTION
	}

	public static TavernGuideQuestDisplayType ParseTavernGuideQuestDisplayTypeValue(string value)
	{
		EnumUtils.TryGetEnum<TavernGuideQuestDisplayType>(value, out var e);
		return e;
	}

	public static TavernGuideCategory ParseTavernGuideCategoryValue(string value)
	{
		EnumUtils.TryGetEnum<TavernGuideCategory>(value, out var e);
		return e;
	}
}
