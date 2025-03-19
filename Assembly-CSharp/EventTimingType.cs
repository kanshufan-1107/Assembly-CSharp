using System.ComponentModel;

public enum EventTimingType
{
	UNKNOWN = -1,
	[Description("none")]
	IGNORE = 0,
	[Description("gvg_promote")]
	GVG_PROMOTION = 7,
	[Description("lunar_new_year")]
	LUNAR_NEW_YEAR = 11,
	[Description("tb_pre_event")]
	SPECIAL_EVENT_PRE_TAVERN_BRAWL = 19,
	[Description("feast_of_winter_veil")]
	FEAST_OF_WINTER_VEIL = 85,
	[Description("never")]
	SPECIAL_EVENT_NEVER = 164,
	[Description("friend_week")]
	FRIEND_WEEK = 166,
	[Description("always")]
	SPECIAL_EVENT_ALWAYS = 203,
	[Description("event_happy_new_year")]
	SPECIAL_EVENT_HAPPY_NEW_YEAR = 219,
	[Description("fire_festival")]
	SPECIAL_EVENT_FIRE_FESTIVAL = 287,
	[Description("gold_doubled")]
	SPECIAL_EVENT_GOLD_DOUBLED = 289,
	[Description("frost_festival")]
	SPECIAL_EVENT_FROST_FESTIVAL = 292,
	[Description("icc_normal_sale")]
	SPECIAL_EVENT_ICC_NORMAL_SALE = 307,
	[Description("frost_fest_free_arena_win")]
	SPECIAL_EVENT_FROST_FESTIVAL_FREE_ARENA_WIN = 315,
	[Description("pirate_day")]
	SPECIAL_EVENT_PIRATE_DAY = 316,
	[Description("icc_launch_freepacks")]
	SPECIAL_EVENT_ICC_LAUNCH_FREEPACKS = 320,
	[Description("hearthstone_world_championship")]
	SPECIAL_EVENT_HEARTHSTONE_WORLD_CHAMPIONSHIP = 408,
	[Description("wild_week_2018")]
	SPECIAL_EVENT_WILD_WEEK_2018 = 410,
	[Description("road_to_raven")]
	SPECIAL_EVENT_ROAD_TO_RAVEN = 414,
	[Description("noblegarden_event")]
	SPECIAL_EVENT_NOBLEGARDEN = 473,
	[Description("taverns_of_time")]
	SPECIAL_EVENT_TAVERNS_OF_TIME = 490,
	[Description("fire_festival_v2")]
	SPECIAL_EVENT_FIRE_FESTIVAL_V2 = 499,
	[Description("days_of_the_frozen_throne")]
	SPECIAL_EVENT_DAYS_OF_THE_FROZEN_THRONE = 525,
	[Description("blizzcon_2018_flare")]
	SPECIAL_EVENT_BLIZZCON_2018_FLARE = 528,
	[Description("celebrate_the_players")]
	SPECIAL_EVENT_CELEBRATE_THE_PLAYERS = 541,
	[Description("feast_of_winter_veil_2018")]
	SPECIAL_EVENT_FEAST_OF_WINTER_VEIL_2018 = 567,
	[Description("rastakhan_season_week_1")]
	SPECIAL_EVENT_SEASON_OF_RASTAKHAN_WK1 = 580,
	[Description("rastakhan_season_week_2")]
	SPECIAL_EVENT_SEASON_OF_RASTAKHAN_WK2 = 581,
	[Description("rastakhan_season_week_3")]
	SPECIAL_EVENT_SEASON_OF_RASTAKHAN_WK3 = 582,
	[Description("henchmania_tb_quest")]
	SPECIAL_EVENT_HENCHMANIA_TB_SEASON = 583,
	[Description("fire_festival_v3")]
	SPECIAL_EVENT_FIRE_FESTIVAL_V3 = 584,
	[Description("tb_season_221")]
	SPECIAL_EVENT_TB_SEASON_221 = 585,
	[Description("tb_season_222")]
	SPECIAL_EVENT_TB_SEASON_222 = 586,
	[Description("uldum_launch_quest")]
	SPECIAL_EVENT_ULDUM_LAUNCH_QUEST = 587,
	[Description("post_hall_of_fame_2020")]
	SPECIAL_EVENT_POST_HALL_OF_FAME_2020 = 588,
	[Description("pre_hall_of_fame_2020")]
	SPECIAL_EVENT_PRE_HALL_OF_FAME_2020 = 589,
	[Description("fire_festival_emote_ever_green")]
	SPECIAL_EVENT_FIRE_FESTIVAL_EMOTES_EVERGREEN = 590,
	[Description("fire_festival_box_dressing_ever_green")]
	SPECIAL_EVENT_FIRE_FESTIVAL_BOX_DRESSING_EVERGREEN = 591,
	[Description("hearthstone_Anomalies_week1_27_4")]
	SPECIAL_EVENT_HEARTHSTONE_ANOMALIES = 1421,
	[Description("hearthstone_Anomalies_afterwards_27_4")]
	SPECIAL_EVENT_HEARTHSTONE_ANOMALIES_AFTERWARDS = 1422,
	[Description("cn_returning_player_experience")]
	CN_RPE = 1632,
	BASE_SPECIAL_EVENT_DATA_ID = 10000000
}
