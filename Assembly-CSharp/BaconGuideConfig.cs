using System;
using System.Collections.Generic;
using Blizzard.T5.Core.Utils;
using UnityEngine;

[CustomEditClass]
public class BaconGuideConfig : MonoBehaviour
{
	[Serializable]
	public class VOHeroSpecificLine
	{
		public string m_HeroCardId;

		[CustomEditField(T = EditType.SOUND_PREFAB)]
		public string m_VOLine;
	}

	public enum HumanReadableVOLineCategory
	{
		InvalidCategory,
		All,
		HeroSpecific,
		AFK,
		HighestShopTier,
		AfterFreezing,
		AfterSelling,
		AfterShopUpgrade,
		AfterTriple,
		Ahead,
		Behind,
		CombatLoss,
		CombatWin,
		FirstPlace,
		Flavor,
		General,
		Hire,
		Idle,
		NewGame,
		RecruitLargeMinion,
		RecruitMediumMinion,
		RecruitSmallMinion,
		ShopToCombat,
		PossibleTriple
	}

	[CustomEditField(Sections = "Guide ID")]
	public string m_GuideCardId;

	[CustomEditField(Sections = "VoiceOver")]
	public List<VOHeroSpecificLine> m_VOHeroSpecificLines;

	[CustomEditField(Sections = "VoiceOver", T = EditType.SOUND_PREFAB)]
	public string m_VOAFK;

	[CustomEditField(Sections = "VoiceOver", T = EditType.SOUND_PREFAB)]
	public string m_VOHighestTier;

	[CustomEditField(Sections = "VoiceOver", T = EditType.SOUND_PREFAB)]
	public List<string> m_VOFreezingLines;

	[CustomEditField(Sections = "VoiceOver", T = EditType.SOUND_PREFAB)]
	public List<string> m_VOSellingLines;

	[CustomEditField(Sections = "VoiceOver", T = EditType.SOUND_PREFAB)]
	public List<string> m_VOShopUpgradeLines;

	[CustomEditField(Sections = "VoiceOver", T = EditType.SOUND_PREFAB)]
	public List<string> m_VOTripleLines;

	[CustomEditField(Sections = "VoiceOver", T = EditType.SOUND_PREFAB)]
	public List<string> m_VOPostShopWinLines;

	[CustomEditField(Sections = "VoiceOver", T = EditType.SOUND_PREFAB)]
	public List<string> m_VOPostShopLoseLines;

	[CustomEditField(Sections = "VoiceOver", T = EditType.SOUND_PREFAB)]
	public List<string> m_VOPostCombatLoseLines;

	[CustomEditField(Sections = "VoiceOver", T = EditType.SOUND_PREFAB)]
	public List<string> m_VOPostCombatWinLines;

	[CustomEditField(Sections = "VoiceOver", T = EditType.SOUND_PREFAB)]
	public List<string> m_VOPostShopIsFirstLines;

	[CustomEditField(Sections = "VoiceOver", T = EditType.SOUND_PREFAB)]
	public List<string> m_VOSpecialIdleLines;

	[CustomEditField(Sections = "VoiceOver", T = EditType.SOUND_PREFAB)]
	public List<string> m_VOPostCombatGeneralLines;

	[CustomEditField(Sections = "VoiceOver", T = EditType.SOUND_PREFAB)]
	public List<string> m_VORefreshLines;

	[CustomEditField(Sections = "VoiceOver", T = EditType.SOUND_PREFAB)]
	public List<string> m_VOIdleLines;

	[CustomEditField(Sections = "VoiceOver", T = EditType.SOUND_PREFAB)]
	public List<string> m_VONewGameLines;

	[CustomEditField(Sections = "VoiceOver", T = EditType.SOUND_PREFAB)]
	public List<string> m_VORecruitLargeLines;

	[CustomEditField(Sections = "VoiceOver", T = EditType.SOUND_PREFAB)]
	public List<string> m_VORecruitMediumLines;

	[CustomEditField(Sections = "VoiceOver", T = EditType.SOUND_PREFAB)]
	public List<string> m_VORecruitSmallLines;

	[CustomEditField(Sections = "VoiceOver", T = EditType.SOUND_PREFAB)]
	public List<string> m_VOPostShopGeneralLines;

	[CustomEditField(Sections = "VoiceOver", T = EditType.SOUND_PREFAB)]
	public List<string> m_VOPossibleTripleLines;

	public List<string> GetAllVOLines()
	{
		List<string> ret = new List<string>();
		Action<string> addIfNotNull = delegate(string x)
		{
			if (!string.IsNullOrEmpty(x))
			{
				ret.Add(x);
			}
		};
		Action<List<string>> addRangeIfNotNull = delegate(List<string> x)
		{
			if (x != null)
			{
				ret.AddRange(x);
			}
		};
		if (m_VOHeroSpecificLines != null)
		{
			foreach (VOHeroSpecificLine line in m_VOHeroSpecificLines)
			{
				addIfNotNull(line.m_VOLine);
			}
		}
		addIfNotNull(m_VOAFK);
		addIfNotNull(m_VOHighestTier);
		addRangeIfNotNull(m_VOFreezingLines);
		addRangeIfNotNull(m_VOSellingLines);
		addRangeIfNotNull(m_VOShopUpgradeLines);
		addRangeIfNotNull(m_VOTripleLines);
		addRangeIfNotNull(m_VOPostShopWinLines);
		addRangeIfNotNull(m_VOPostShopLoseLines);
		addRangeIfNotNull(m_VOPostCombatLoseLines);
		addRangeIfNotNull(m_VOPostCombatWinLines);
		addRangeIfNotNull(m_VOPostShopIsFirstLines);
		addRangeIfNotNull(m_VOSpecialIdleLines);
		addRangeIfNotNull(m_VOPostCombatGeneralLines);
		addRangeIfNotNull(m_VORefreshLines);
		addRangeIfNotNull(m_VOIdleLines);
		addRangeIfNotNull(m_VONewGameLines);
		addRangeIfNotNull(m_VORecruitLargeLines);
		addRangeIfNotNull(m_VORecruitMediumLines);
		addRangeIfNotNull(m_VORecruitSmallLines);
		addRangeIfNotNull(m_VOPostShopGeneralLines);
		addRangeIfNotNull(m_VOPossibleTripleLines);
		return ret;
	}

	public string PopRandomSpecialIdleLine()
	{
		if (m_VOSpecialIdleLines.Count == 0 || m_VOSpecialIdleLines == null)
		{
			return null;
		}
		string randomLine = TryGetRandomLine(m_VOSpecialIdleLines);
		if (m_VOSpecialIdleLines != null && randomLine != null)
		{
			m_VOSpecialIdleLines.Remove(randomLine);
		}
		return randomLine;
	}

	public bool CheckHeroSpecificLine(string heroCardId, out string voHeroSpecificLine)
	{
		foreach (VOHeroSpecificLine line in m_VOHeroSpecificLines)
		{
			if (line.m_HeroCardId == heroCardId)
			{
				voHeroSpecificLine = line.m_VOLine;
				return true;
			}
		}
		voHeroSpecificLine = null;
		return false;
	}

	public string GetHighestTierLine()
	{
		return m_VOHighestTier;
	}

	public string GetAFKLine()
	{
		return m_VOAFK;
	}

	public string GetRandomFreezingLine()
	{
		return TryGetRandomLine(m_VOFreezingLines);
	}

	public string GetRandomSellingLine()
	{
		return TryGetRandomLine(m_VOSellingLines);
	}

	public string GetRandomShopUpgradeLine()
	{
		return TryGetRandomLine(m_VOShopUpgradeLines);
	}

	public string GetRandomTripleLine()
	{
		return TryGetRandomLine(m_VOTripleLines);
	}

	public string GetRandomPostShopWinLine()
	{
		return TryGetRandomLine(m_VOPostShopWinLines);
	}

	public string GetRandomPostShopLoseLine()
	{
		return TryGetRandomLine(m_VOPostShopLoseLines);
	}

	public string GetRandomPostCombatLoseLine()
	{
		return TryGetRandomLine(m_VOPostCombatLoseLines);
	}

	public string GetRandomPostCombatWinLine()
	{
		return TryGetRandomLine(m_VOPostCombatWinLines);
	}

	public string GetRandomPostShopIsFirstLine()
	{
		return TryGetRandomLine(m_VOPostShopIsFirstLines);
	}

	public string GetRandomSpecialIdleLine()
	{
		return TryGetRandomLine(m_VOSpecialIdleLines);
	}

	public string GetRandomPostCombatGeneralLine()
	{
		return TryGetRandomLine(m_VOPostCombatGeneralLines);
	}

	public string GetRandomRefreshLine()
	{
		return TryGetRandomLine(m_VORefreshLines);
	}

	public string GetRandomIdleLine()
	{
		return TryGetRandomLine(m_VOIdleLines);
	}

	public string GetRandomNewGameLine()
	{
		return TryGetRandomLine(m_VONewGameLines);
	}

	public string GetRandomRecruitLargeLine()
	{
		return TryGetRandomLine(m_VORecruitLargeLines);
	}

	public string GetRandomRecruitMediumLine()
	{
		return TryGetRandomLine(m_VORecruitMediumLines);
	}

	public string GetRandomRecruitSmallLine()
	{
		return TryGetRandomLine(m_VORecruitSmallLines);
	}

	public string GetRandomPostShopGeneralLine()
	{
		return TryGetRandomLine(m_VOPostShopGeneralLines);
	}

	public string GetRandomPossibleTripleLine()
	{
		return TryGetRandomLine(m_VOPossibleTripleLines);
	}

	protected string TryGetRandomLine(List<string> lines)
	{
		if (lines == null)
		{
			return null;
		}
		if (lines.Count == 0)
		{
			return null;
		}
		return lines[UnityEngine.Random.Range(0, lines.Count)];
	}

	public List<string> GetLinesByHumanReadableName(string humanReadableName)
	{
		HumanReadableVOLineCategory enumValue = EnumUtils.SafeParse(humanReadableName, HumanReadableVOLineCategory.InvalidCategory);
		if (enumValue == HumanReadableVOLineCategory.InvalidCategory)
		{
			Log.Gameplay.PrintError("BaconGuideConfig.GetLinesByHumanReadableName() - Invalid category name given: " + humanReadableName);
			return new List<string>();
		}
		return GetLinesByHumanReadableCategory(enumValue);
	}

	public List<string> GetLinesByHumanReadableCategory(HumanReadableVOLineCategory category)
	{
		switch (category)
		{
		case HumanReadableVOLineCategory.InvalidCategory:
			Log.Gameplay.PrintError($"BaconGuideConfig.GetLinesByHumanReadableName() - Invalid category given: {category}");
			return new List<string>();
		case HumanReadableVOLineCategory.All:
			return GetAllVOLines();
		case HumanReadableVOLineCategory.HeroSpecific:
		{
			List<string> heroSpecificLines = new List<string>();
			{
				foreach (VOHeroSpecificLine item in m_VOHeroSpecificLines)
				{
					heroSpecificLines.Add(item.m_VOLine);
				}
				return heroSpecificLines;
			}
		}
		case HumanReadableVOLineCategory.AFK:
			return new List<string> { m_VOAFK };
		case HumanReadableVOLineCategory.HighestShopTier:
			return new List<string> { m_VOHighestTier };
		case HumanReadableVOLineCategory.AfterFreezing:
			return m_VOFreezingLines;
		case HumanReadableVOLineCategory.AfterSelling:
			return m_VOSellingLines;
		case HumanReadableVOLineCategory.AfterShopUpgrade:
			return m_VOShopUpgradeLines;
		case HumanReadableVOLineCategory.AfterTriple:
			return m_VOTripleLines;
		case HumanReadableVOLineCategory.Ahead:
			return m_VOPostShopWinLines;
		case HumanReadableVOLineCategory.Behind:
			return m_VOPostShopLoseLines;
		case HumanReadableVOLineCategory.CombatLoss:
			return m_VOPostCombatLoseLines;
		case HumanReadableVOLineCategory.CombatWin:
			return m_VOPostCombatWinLines;
		case HumanReadableVOLineCategory.FirstPlace:
			return m_VOPostShopIsFirstLines;
		case HumanReadableVOLineCategory.Flavor:
			return m_VOSpecialIdleLines;
		case HumanReadableVOLineCategory.General:
			return m_VOPostCombatGeneralLines;
		case HumanReadableVOLineCategory.Hire:
			return m_VORefreshLines;
		case HumanReadableVOLineCategory.Idle:
			return m_VOIdleLines;
		case HumanReadableVOLineCategory.NewGame:
			return m_VONewGameLines;
		case HumanReadableVOLineCategory.RecruitLargeMinion:
			return m_VORecruitLargeLines;
		case HumanReadableVOLineCategory.RecruitMediumMinion:
			return m_VORecruitMediumLines;
		case HumanReadableVOLineCategory.RecruitSmallMinion:
			return m_VORecruitSmallLines;
		case HumanReadableVOLineCategory.ShopToCombat:
			return m_VOPostShopGeneralLines;
		case HumanReadableVOLineCategory.PossibleTriple:
			return m_VOPossibleTripleLines;
		default:
			Log.Gameplay.PrintError($"BaconGuideConfig.GetLinesByHumanReadableName() - Unable to parse category given: {category}");
			return new List<string>();
		}
	}
}
