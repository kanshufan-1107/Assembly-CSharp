using Blizzard.T5.Core;

public class TB_AutoBrawl : MissionEntity
{
	private static Map<GameEntityOption, bool> s_booleanOptions = InitBooleanOptions();

	private static Map<GameEntityOption, string> s_stringOptions = InitStringOptions();

	private static Map<GameEntityOption, bool> InitBooleanOptions()
	{
		return new Map<GameEntityOption, bool> { 
		{
			GameEntityOption.HANDLE_COIN,
			false
		} };
	}

	private static Map<GameEntityOption, string> InitStringOptions()
	{
		return new Map<GameEntityOption, string>();
	}

	public TB_AutoBrawl()
	{
		m_gameOptions.AddOptions(s_booleanOptions, s_stringOptions);
	}

	public override bool ShouldDoAlternateMulliganIntro()
	{
		return true;
	}
}
