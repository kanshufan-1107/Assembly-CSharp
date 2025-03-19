using System.ComponentModel;
using Blizzard.T5.Core.Utils;

namespace Assets;

public static class LettuceTutorialVo
{
	public enum LettuceTutorialEvent
	{
		[Description("invalid")]
		INVALID,
		[Description("lobby_entered")]
		LOBBY_ENTERED,
		[Description("map_started")]
		MAP_STARTED,
		[Description("map_node_completed_pre_merc_grant")]
		MAP_NODE_COMPLETED_PRE_MERC_GRANT,
		[Description("map_node_completed_post_merc_grant")]
		MAP_NODE_COMPLETED_POST_MERC_GRANT,
		[Description("map_active_coin_released")]
		MAP_ACTIVE_COIN_RELEASED,
		[Description("map_pre_treasure_selection")]
		MAP_PRE_TREASURE_SELECTION,
		[Description("first_equipment_unlocked")]
		FIRST_EQUIPMENT_UNLOCKED,
		[Description("village_tutorial_start")]
		VILLAGE_TUTORIAL_START,
		[Description("village_tutorial_workshop_intro_popup")]
		VILLAGE_TUTORIAL_WORKSHOP_INTRO_POPUP,
		[Description("village_tutorial_visit_tavern_popup")]
		VILLAGE_TUTORIAL_VISIT_TAVERN_POPUP,
		[Description("village_tutorial_view_merc_abilities")]
		VILLAGE_TUTORIAL_VIEW_MERC_ABILITIES,
		[Description("village_tutorial_choose_upgrade")]
		VILLAGE_TUTORIAL_CHOOSE_UPGRADE,
		[Description("village_tutorial_upgrade_ability_start")]
		VILLAGE_TUTORIAL_UPGRADE_ABILITY_START,
		[Description("village_tutorial_upgrade_ability_end")]
		VILLAGE_TUTORIAL_UPGRADE_ABILITY_END,
		[Description("village_tutorial_task_board_start")]
		VILLAGE_TUTORIAL_TASK_BOARD_START,
		[Description("village_tutorial_task_board_end")]
		VILLAGE_TUTORIAL_TASK_BOARD_END,
		[Description("village_tutorial_pve_build_start")]
		VILLAGE_TUTORIAL_PVE_BUILD_START,
		[Description("village_tutorial_pve_build_end")]
		VILLAGE_TUTORIAL_PVE_BUILD_END,
		[Description("village_tutorial_visit_travel")]
		VILLAGE_TUTORIAL_VISIT_TRAVEL,
		[Description("village_tutorial_shop_build_start")]
		VILLAGE_TUTORIAL_SHOP_BUILD_START,
		[Description("village_tutorial_shop_build_end")]
		VILLAGE_TUTORIAL_SHOP_BUILD_END,
		[Description("village_tutorial_visit_shop")]
		VILLAGE_TUTORIAL_VISIT_SHOP,
		[Description("village_tutorial_claim_gift")]
		VILLAGE_TUTORIAL_CLAIM_GIFT,
		[Description("village_tutorial_open_pack")]
		VILLAGE_TUTORIAL_OPEN_PACK,
		[Description("village_tutorial_end")]
		VILLAGE_TUTORIAL_END,
		[Description("village_tutorial_upgrade_ability_popup")]
		VILLAGE_TUTORIAL_UPGRADE_ABILITY_POPUP,
		[Description("village_tutorial_shop_claim_pack_popup")]
		VILLAGE_TUTORIAL_SHOP_CLAIM_PACK_POPUP,
		[Description("village_tutorial_open_task_detail")]
		VILLAGE_TUTORIAL_OPEN_TASK_DETAIL,
		[Description("village_tutorial_after_pack_opening")]
		VILLAGE_TUTORIAL_AFTER_PACK_OPENING,
		[Description("village_tutorial_mailbox_build_end")]
		VILLAGE_TUTORIAL_MAILBOX_BUILD_END,
		[Description("village_building_upgrade_available_taskboard")]
		VILLAGE_BUILDING_UPGRADE_AVAILABLE_TASKBOARD,
		[Description("village_building_upgrade_available_pvp")]
		VILLAGE_BUILDING_UPGRADE_AVAILABLE_PVP,
		[Description("village_building_upgrade_available_pvezones")]
		VILLAGE_BUILDING_UPGRADE_AVAILABLE_PVEZONES,
		[Description("village_building_upgrade_available_training_hall")]
		VILLAGE_BUILDING_UPGRADE_AVAILABLE_TRAINING_HALL,
		[Description("village_building_start_training")]
		VILLAGE_BUILDING_START_TRAINING,
		[Description("map_node_revealed")]
		MAP_NODE_REVEALED,
		[Description("map_node_completed_pre_rewards")]
		MAP_NODE_COMPLETED_PRE_REWARDS,
		[Description("village_tutorial_view_tasks_popup")]
		VILLAGE_TUTORIAL_VIEW_TASKS_POPUP,
		[Description("village_tutorial_task_collection_popup")]
		VILLAGE_TUTORIAL_TASK_COLLECTION_POPUP,
		[Description("village_tutorial_renown_popup")]
		VILLAGE_TUTORIAL_RENOWN_POPUP,
		[Description("village_tutorial_boss_rush_popup")]
		VILLAGE_TUTORIAL_BOSS_RUSH_POPUP,
		[Description("village_tutorial_visit_mythic_treasure_start")]
		VILLAGE_TUTORIAL_VISIT_MYTHIC_TREASURE_START,
		[Description("village_tutorial_visit_mythic_treasure_end")]
		VILLAGE_TUTORIAL_VISIT_MYTHIC_TREASURE_END
	}

	public static LettuceTutorialEvent ParseLettuceTutorialEventValue(string value)
	{
		EnumUtils.TryGetEnum<LettuceTutorialEvent>(value, out var e);
		return e;
	}
}
