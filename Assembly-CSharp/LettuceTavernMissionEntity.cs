using Blizzard.T5.Core;

public class LettuceTavernMissionEntity : MissionEntity
{
	private static Map<GameEntityOption, bool> s_booleanOptions = InitBooleanOptions();

	private static Map<GameEntityOption, string> s_stringOptions = InitStringOptions();

	private static Map<GameEntityOption, bool> InitBooleanOptions()
	{
		return new Map<GameEntityOption, bool>
		{
			{
				GameEntityOption.DIM_OPPOSING_HERO_DURING_MULLIGAN,
				true
			},
			{
				GameEntityOption.HANDLE_COIN,
				false
			},
			{
				GameEntityOption.DO_OPENING_TAUNTS,
				false
			},
			{
				GameEntityOption.SUPPRESS_CLASS_NAMES,
				true
			},
			{
				GameEntityOption.ALLOW_NAME_BANNER_MODE_ICONS,
				false
			},
			{
				GameEntityOption.DISABLE_RESTART_BUTTON,
				true
			},
			{
				GameEntityOption.USE_COMPACT_ENCHANTMENT_BANNERS,
				true
			},
			{
				GameEntityOption.ALLOW_FATIGUE,
				false
			},
			{
				GameEntityOption.ALLOW_ENCHANTMENT_SPARKLES,
				false
			},
			{
				GameEntityOption.ALLOW_SLEEP_FX,
				false
			},
			{
				GameEntityOption.WAIT_FOR_RATING_INFO,
				false
			}
		};
	}

	private static Map<GameEntityOption, string> InitStringOptions()
	{
		return new Map<GameEntityOption, string>();
	}

	public LettuceTavernMissionEntity()
	{
		m_gameOptions.AddOptions(s_booleanOptions, s_stringOptions);
	}
}
