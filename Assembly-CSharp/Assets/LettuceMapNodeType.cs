using System.ComponentModel;
using Blizzard.T5.Core.Utils;

namespace Assets;

public static class LettuceMapNodeType
{
	public enum LettuceMapBossType
	{
		NONE,
		NORMAL_BOSS,
		ELITE_BOSS,
		FINAL_BOSS,
		SIMPLE_BOSS
	}

	public enum Visitlogictype
	{
		[Description("none")]
		NONE = 0,
		[Description("heal_team")]
		HEAL_TEAM = 1,
		[Description("skip_to_final_boss")]
		SKIP_TO_FINAL_BOSS = 3,
		[Description("reassign_map_role")]
		REASSIGN_MAP_ROLE = 4,
		[Description("view_task_list")]
		VIEW_TASK_LIST = 5
	}

	public static LettuceMapBossType ParseLettuceMapBossTypeValue(string value)
	{
		EnumUtils.TryGetEnum<LettuceMapBossType>(value, out var e);
		return e;
	}

	public static Visitlogictype ParseVisitlogictypeValue(string value)
	{
		EnumUtils.TryGetEnum<Visitlogictype>(value, out var e);
		return e;
	}
}
