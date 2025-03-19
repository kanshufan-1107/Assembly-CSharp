using Blizzard.T5.Core;

public class HM_Puzzle_04 : StandardGameEntity
{
	private static Map<GameEntityOption, bool> s_booleanOptions = InitBooleanOptions();

	private static Map<GameEntityOption, string> s_stringOptions = new Map<GameEntityOption, string>();

	public HM_Puzzle_04()
	{
		m_gameOptions.AddOptions(s_booleanOptions, s_stringOptions);
	}

	private static Map<GameEntityOption, bool> InitBooleanOptions()
	{
		return new Map<GameEntityOption, bool> { 
		{
			GameEntityOption.DISABLE_POWER_LOGGING,
			true
		} };
	}
}
