using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using Assets;
using Blizzard.BlizzardErrorMobile;
using Blizzard.Commerce;
using Blizzard.GameService.Protocol.V2.Client;
using Blizzard.GameService.SDK.Client.Integration;
using Blizzard.T5.AssetManager;
using Blizzard.T5.Configuration;
using Blizzard.T5.Core;
using Blizzard.T5.Core.Time;
using Blizzard.T5.Core.Utils;
using Blizzard.T5.Jobs;
using Blizzard.T5.Logging;
using Blizzard.T5.MaterialService;
using Blizzard.T5.Services;
using CSharpZombieDetector;
using Hearthstone;
using Hearthstone.APIGateway;
using Hearthstone.Attribution;
using Hearthstone.Core;
using Hearthstone.CRM;
using Hearthstone.DataModels;
using Hearthstone.Http;
using Hearthstone.InGameMessage;
using Hearthstone.InGameMessage.UI;
using Hearthstone.Login;
using Hearthstone.Progression;
using Hearthstone.Store;
using Hearthstone.Streaming;
using Hearthstone.UI;
using Hearthstone.Util;
using MiniJSON;
using PegasusGame;
using PegasusLettuce;
using PegasusShared;
using PegasusUtil;
using SpectatorProto;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Cheats : IService
{
	private enum QuickLaunchAvailability
	{
		OK,
		FINDING_GAME,
		ACTIVE_GAME,
		SCENE_TRANSITION,
		COLLECTION_NOT_READY
	}

	private enum FriendListType
	{
		FRIEND,
		RECENT,
		NEARBY
	}

	private class QuickLaunchState
	{
		public bool m_launching;

		public bool m_skipMulligan;

		public bool m_flipHeroes;

		public bool m_mirrorHeroes;

		public string m_opponentHeroCardId;
	}

	private struct NamedParam
	{
		public string Text { get; private set; }

		public int Number { get; private set; }

		public bool HasNumber => Number > 0;

		public NamedParam(string param)
		{
			Text = param;
			Number = 0;
			if (GeneralUtils.TryParseInt(param, out var val))
			{
				Number = val;
			}
		}
	}

	public delegate void LogFormatFunc(string format, params object[] args);

	private enum SetAdventureProgressMode
	{
		Victory,
		Defeat,
		Progress
	}

	public readonly Vector3 SPEECH_BUBBLE_HIDDEN_POSITION = new Vector3(15000f, 0f, 0f);

	private static Cheats s_instance;

	private bool m_isInGameplayScene;

	private int m_boardId;

	private string m_playerTags;

	private bool m_speechBubblesEnabled = true;

	private bool m_cardTextEnabled = true;

	private bool m_cardNamesEnabled = true;

	private bool m_cardRaceTextEnabled = true;

	private bool m_playerNamesEnabled = true;

	private bool m_forcingApprenticeGameEnabled;

	private bool m_battlegroundHeroBuddyEnabled = true;

	private Map<Global.SoundCategory, bool> m_audioChannelEnabled = InitAudioChannelMap();

	private Queue<int> m_pvpdrTreasureIds = new Queue<int>();

	private Queue<int> m_pvpdrLootIds = new Queue<int>();

	private Map<string, List<Global.SoundCategory>> m_audioChannelGroups = new Map<string, List<Global.SoundCategory>>
	{
		{
			"VO",
			new List<Global.SoundCategory>
			{
				Global.SoundCategory.VO,
				Global.SoundCategory.SPECIAL_VO,
				Global.SoundCategory.BOSS_VO,
				Global.SoundCategory.TRIGGER_VO
			}
		},
		{
			"MUSIC",
			new List<Global.SoundCategory>
			{
				Global.SoundCategory.MUSIC,
				Global.SoundCategory.SPECIAL_MUSIC,
				Global.SoundCategory.HERO_MUSIC
			}
		},
		{
			"FX",
			new List<Global.SoundCategory>
			{
				Global.SoundCategory.FX,
				Global.SoundCategory.NONE,
				Global.SoundCategory.SPECIAL_CARD
			}
		},
		{
			"BACKGROUND",
			new List<Global.SoundCategory>
			{
				Global.SoundCategory.AMBIENCE,
				Global.SoundCategory.RESET_GAME
			}
		}
	};

	private bool m_loadingStoreChallengePrompt;

	private StoreChallengePrompt m_storeChallengePrompt;

	private bool m_isNewCardInPackOpeningEnabled;

	private AlertPopup m_alert;

	private static readonly Map<KeyCode, ScenarioDbId> s_quickPlayKeyMap = new Map<KeyCode, ScenarioDbId>
	{
		{
			KeyCode.F1,
			ScenarioDbId.PRACTICE_EXPERT_MAGE
		},
		{
			KeyCode.F2,
			ScenarioDbId.PRACTICE_EXPERT_HUNTER
		},
		{
			KeyCode.F3,
			ScenarioDbId.PRACTICE_EXPERT_WARRIOR
		},
		{
			KeyCode.F4,
			ScenarioDbId.PRACTICE_EXPERT_SHAMAN
		},
		{
			KeyCode.F5,
			ScenarioDbId.PRACTICE_EXPERT_DRUID
		},
		{
			KeyCode.F6,
			ScenarioDbId.PRACTICE_EXPERT_PRIEST
		},
		{
			KeyCode.F7,
			ScenarioDbId.PRACTICE_EXPERT_ROGUE
		},
		{
			KeyCode.F8,
			ScenarioDbId.PRACTICE_EXPERT_PALADIN
		},
		{
			KeyCode.F9,
			ScenarioDbId.PRACTICE_EXPERT_WARLOCK
		},
		{
			KeyCode.F10,
			ScenarioDbId.PRACTICE_EXPERT_DEMONHUNTER
		},
		{
			KeyCode.F11,
			ScenarioDbId.PRACTICE_EXPERT_DEATHKNIGHT
		},
		{
			KeyCode.T,
			ScenarioDbId.TEST_BLANK_STATE
		},
		{
			KeyCode.M,
			ScenarioDbId.LETTUCE_DEV_TEST_VS_AI
		},
		{
			KeyCode.B,
			ScenarioDbId.TB_BACONSHOP_VS_AI
		},
		{
			KeyCode.D,
			ScenarioDbId.TB_BACONSHOP_DUOS_VS_AI
		},
		{
			KeyCode.F,
			ScenarioDbId.TB_BACONSHOP_DUOS_1_PLAYER_VS_AI
		}
	};

	private static readonly Map<ScenarioDbId, GameType> s_scenarioToGameTypeMap = new Map<ScenarioDbId, GameType>
	{
		{
			ScenarioDbId.TB_BACONSHOP_VS_AI,
			GameType.GT_BATTLEGROUNDS_PLAYER_VS_AI
		},
		{
			ScenarioDbId.TB_BACONSHOP_DUOS_VS_AI,
			GameType.GT_BATTLEGROUNDS_DUO_VS_AI
		},
		{
			ScenarioDbId.TB_BACONSHOP_DUOS_1_PLAYER_VS_AI,
			GameType.GT_BATTLEGROUNDS_DUO_1_PLAYER_VS_AI
		}
	};

	private static readonly List<ScenarioDbId> s_quickPlayNotSkipMulligan = new List<ScenarioDbId>
	{
		ScenarioDbId.TB_BACONSHOP_VS_AI,
		ScenarioDbId.TB_BACONSHOP_DUOS_VS_AI,
		ScenarioDbId.TB_BACONSHOP_DUOS_1_PLAYER_VS_AI
	};

	private static readonly Map<KeyCode, string> s_opponentHeroKeyMap = new Map<KeyCode, string>
	{
		{
			KeyCode.F1,
			"HERO_08"
		},
		{
			KeyCode.F2,
			"HERO_05"
		},
		{
			KeyCode.F3,
			"HERO_01"
		},
		{
			KeyCode.F4,
			"HERO_02"
		},
		{
			KeyCode.F5,
			"HERO_06"
		},
		{
			KeyCode.F6,
			"HERO_09"
		},
		{
			KeyCode.F7,
			"HERO_03"
		},
		{
			KeyCode.F8,
			"HERO_04"
		},
		{
			KeyCode.F9,
			"HERO_07"
		},
		{
			KeyCode.F10,
			"HERO_10"
		},
		{
			KeyCode.F11,
			"HERO_11"
		},
		{
			KeyCode.T,
			"HERO_01"
		},
		{
			KeyCode.M,
			string.Empty
		},
		{
			KeyCode.B,
			"TB_BaconShop_HERO_PH"
		},
		{
			KeyCode.D,
			"TB_BaconShop_HERO_PH"
		},
		{
			KeyCode.F,
			"TB_BaconShop_HERO_PH"
		}
	};

	private QuickLaunchState m_quickLaunchState = new QuickLaunchState();

	private bool m_skipSendingGetGameState;

	public static float VOChanceOverride = -1f;

	private float m_waitTime = 10f;

	private bool m_showedMessage;

	private static readonly ChangeMessageItemInformation[] m_changeMessageCardsExamples = new ChangeMessageItemInformation[5]
	{
		new ChangeMessageItemInformation
		{
			ItemType = InGameMessageItemDisplayContent.ItemType.Card,
			ItemId = "BAR_024"
		},
		new ChangeMessageItemInformation
		{
			ItemType = InGameMessageItemDisplayContent.ItemType.Card,
			ItemId = "BAR_745"
		},
		new ChangeMessageItemInformation
		{
			ItemType = InGameMessageItemDisplayContent.ItemType.Card,
			ItemId = "BAR_327"
		},
		new ChangeMessageItemInformation
		{
			ItemType = InGameMessageItemDisplayContent.ItemType.Card,
			ItemId = "BAR_082"
		},
		new ChangeMessageItemInformation
		{
			ItemType = InGameMessageItemDisplayContent.ItemType.Card,
			ItemId = "BAR_025"
		}
	};

	private static readonly ChangeMessageItemInformation[] m_changeMessageHeroExamples = new ChangeMessageItemInformation[1]
	{
		new ChangeMessageItemInformation
		{
			ItemType = InGameMessageItemDisplayContent.ItemType.Hero,
			ItemId = "HERO_09a"
		}
	};

	private List<WidgetInstance> s_createdWidgets = new List<WidgetInstance>();

	[CompilerGenerated]
	private static Action<string[]> PlayAudioByName;

	private static bool s_hasSubscribedToPartyEvents = false;

	private string[] m_lastMercsServerCmd;

	private string[] m_lastUtilServerCmd;

	private static WidgetInstance exampleUI = null;

	private IGameDownloadManager DownloadManager => GameDownloadManagerProvider.Get();

	public static bool ShowFakeBreakingNews => Vars.Key("Cheats.ShowFakeBreakingNews").GetBool(def: false);

	private static Map<Global.SoundCategory, bool> InitAudioChannelMap()
	{
		Map<Global.SoundCategory, bool> ac = new Map<Global.SoundCategory, bool>();
		foreach (int item in Enum.GetValues(typeof(Global.SoundCategory)))
		{
			ac.Add((Global.SoundCategory)item, value: true);
		}
		return ac;
	}

	public static Cheats Get()
	{
		if (s_instance == null)
		{
			s_instance = ServiceManager.Get<Cheats>();
		}
		return s_instance;
	}

	public IEnumerator<IAsyncJobResult> Initialize(ServiceLocator serviceLocator)
	{
		CheatMgr cheatMgr = serviceLocator.Get<CheatMgr>();
		if (HearthstoneApplication.IsInternal())
		{
			cheatMgr.RegisterCategory("help");
			cheatMgr.RegisterCheatHandler("help", OnProcessCheat_help, "Get help for a specific command or list of commands", "<command name>", "");
			cheatMgr.RegisterCheatHandler("example", OnProcessCheat_example, "Run an example of this command if one exists", "<command name>");
			cheatMgr.RegisterCheatHandler("error", OnProcessCheat_error, "Make the client throw an arbitrary error.", "<warning | fatal | exception> <optional error message>", "warning This is an example warning message.");
			cheatMgr.RegisterCategory("bug");
			if (!RegionUtils.IsCNLegalRegion || Vars.Key("Debug.EnableBugCheat").GetBool(def: false))
			{
				cheatMgr.RegisterCheatHandler("bug", On_ProcessCheat_bug);
				cheatMgr.RegisterCheatHandler("Bug", On_ProcessCheat_bug);
				cheatMgr.RegisterCheatHandler("assert", On_ProcessCheat_assert);
				cheatMgr.RegisterCheatHandler("Assert", On_ProcessCheat_assert);
			}
			cheatMgr.RegisterCheatHandler("crash", On_ProcessCheat_crash);
			cheatMgr.RegisterCheatHandler("anr", On_ProcessCheat_ANR);
			cheatMgr.RegisterCategory("general");
			cheatMgr.RegisterCheatHandler("cheat", OnProcessCheat_cheat, "Send a cheat command to the server", "<command> <arguments>");
			cheatMgr.RegisterCheatAlias("cheat", "c");
			cheatMgr.RegisterCheatHandler("timescale", OnProcessCheat_timescale, "Cheat to change the timescale", "<timescale>", "0.5");
			cheatMgr.RegisterCheatHandler("util", OnProcessCheat_utilservercmd, "Run a cheat on the UTIL server you're connected to.", "[subcommand] [subcommand args]", "help");
			cheatMgr.RegisterCheatHandler("game", OnProcessCheat_gameservercmd, "[NYI] Run a cheat on the GAME server you're connected to.", "[subcommand] [subcommand args]", "help");
			Network.Get().RegisterNetHandler(DebugCommandResponse.PacketID.ID, OnProcessCheat_utilservercmd_OnResponse);
			cheatMgr.RegisterCheatHandler("event", OnProcessCheat_EventTiming, "View event timings to see if they're active.", "[event=event_name]", "");
			cheatMgr.RegisterCheatHandler("audiochannel", OnProcessCheat_audioChannel, "Turn on/off an audio channel.", "[audio channel name] [on/off]", "fx off");
			cheatMgr.RegisterCheatHandler("audiochannelgroup", OnProcessCheat_audioChannelGroup, "Turn on/off a group of audio channels.", "[audio channel group name] [on/off]", "vo off");
			cheatMgr.RegisterCheatHandler("tracert", OnProcessCheat_tracert);
			cheatMgr.RegisterCheatHandler("setservertimeoffset", OnProcessCheat_setservertimeoffset, "Set the time offset on the client when requesting server time. This does not modify server time", "<time_offset_ms>");
			cheatMgr.RegisterCheatHandler("rundeeplink", OnProcessCheat_RunDeeplink, "Attempts to run provided deeplink (e.g. hearthstone://store/[pmtId])");
			cheatMgr.RegisterCategory("igm");
			cheatMgr.RegisterCheatHandler("igm", OnProcessCheat_igm, "Register the content type and show it by using the debug UI", "<content_type>");
			cheatMgr.RegisterCheatHandler("msgui", OnProcessCheat_msgui, "Message popup ui", "<register|show> [<text|shop|launch|change>]");
			cheatMgr.RegisterCheatHandler("askpushnotification", OnProcessCheat_askpushnotification, "Ask push notification", "<show|hide>");
			cheatMgr.RegisterCategory("program");
			cheatMgr.RegisterCheatHandler("reset", OnProcessCheat_reset, "Reset the client");
			cheatMgr.RegisterCategory("gameplay");
			cheatMgr.RegisterCheatHandler("board", OnProcessCheat_board, "Set which board will be loaded on the next game", "<BRM|STW|GVG>", "BRM");
			cheatMgr.RegisterCheatHandler("playertags", OnProcessCheat_playerTags, "Set these tags on your player in the next game (limit 20)", "<TagId1=TagValue1,TagId2=TagValue2,...,TagIdN=TagValueN>", "427=10,419=1");
			cheatMgr.RegisterCheatHandler("togglespeechbubbles", OnProcessCheat_speechBubbles, "Toggle on/off speech bubbles.", "", "");
			cheatMgr.RegisterCheatHandler("disconnect", OnProcessCheat_disconnect, "Disconnects you from a game in progress (disconnects from game server only). If you want to disconnect from just battle.net, use 'disconnect bnet'.");
			cheatMgr.RegisterCheatHandler("reconnect", OnProcessCheat_reconnect, "Reconnect to the util server using 'reconnect bnet'.");
			cheatMgr.RegisterCheatHandler("restart", OnProcessCheat_restart, "Restarts any non-PvP game.");
			cheatMgr.RegisterCheatHandler("autohand", OnProcessCheat_autohand, "Set whether PhoneUI automatically hides your hand after playing a card", "<true/false>", "true");
			cheatMgr.RegisterCheatHandler("endturn", OnProcessCheat_endturn, "End your turn");
			cheatMgr.RegisterCheatHandler("scenario", OnProcessCheat_scenario, "Launch a scenario.", "<scenario_id> [<game_type_id>] [<deck_name>|<deck_id>] [<game_format>]");
			cheatMgr.RegisterCheatAlias("scenario", "mission");
			cheatMgr.RegisterCheatHandler("aigame", OnProcessCheat_aigame, "Launch a game vs an AI using specified deck code.", "<deck_code_string> [<game_format>]");
			cheatMgr.RegisterCheatHandler("loadsnapshot", OnProcessCheat_loadSnapshot, "Load a snapshot file from local disk.", "<replayfilename>");
			cheatMgr.RegisterCheatHandler("skipgetgamestate", OnProcessCheat_SkipSendingGetGameState, "Skip sending GetGameState packet in Gameplay.Start().");
			cheatMgr.RegisterCheatHandler("sendgetgamestate", OnProcessCheat_SendGetGameState, "Send GetGameState packet.");
			cheatMgr.RegisterCheatHandler("auto_exportgamestate", OnProcessCheat_autoexportgamestate, "Save JSON file serializing some of GameState");
			cheatMgr.RegisterCheatHandler("opponentname", OnProcessCheat_OpponentName, "Set the Opponent name", "", "The Innkeeper");
			cheatMgr.RegisterCheatHandler("history", OnProcessCheat_History, "disable/enable history", "", "true");
			cheatMgr.RegisterCheatHandler("settag", OnProcessCheat_settag, "Sets a tag on an entity to a value", "<tag_id> <entity_id> <tag_value>");
			cheatMgr.RegisterCheatHandler("thinkemotes", OnProcessCheat_playAllThinkEmotes, "Plays all of the think lines for the specified player's hero");
			cheatMgr.RegisterCheatHandler("playemote", OnProcessCheat_playEmote, "Play the emote for the specified player's hero", "<emote_type> <player>");
			cheatMgr.RegisterCheatHandler("heropowervo", OnProcessCheat_playAllMissionHeroPowerLines, "Plays all the hero power lines associated with this mission");
			cheatMgr.RegisterCheatHandler("idlevo", OnProcessCheat_playAllMissionIdleLines, "Plays all idle lines associated with this mission");
			cheatMgr.RegisterCheatHandler("playbgguidevo", OnProcessCheat_playBattlegroundsGuideVO, "Play a guide vo line");
			cheatMgr.RegisterCheatHandler("playbglegendaryherovfx", OnProcessCheat_playLegendaryHeroVFX, "Play a legendary hero VFX");
			cheatMgr.RegisterCheatHandler("playbglegendaryherovo", OnProcessCheat_playLegendaryHeroVO, "Play a legendary hero vo line");
			cheatMgr.RegisterCheatHandler("debugscript", OnProcessCheat_debugscript, "Toggles script debugging for a specific power", "<power_guid>");
			cheatMgr.RegisterCheatHandler("scriptdebug", OnProcessCheat_debugscript, "Toggles script debugging for a specific power", "<power_guid>");
			cheatMgr.RegisterCheatHandler("disablescriptdebug", OnProcessCheat_disablescriptdebug, "Disables all script debugging on the server");
			cheatMgr.RegisterCheatHandler("disabledebugscript", OnProcessCheat_disablescriptdebug, "Disables all script debugging on the server");
			cheatMgr.RegisterCheatHandler("printpersistentlist", OnProcessCheat_printpersistentlist, "Prints all persistent lists for a particular entity. Call it with no entity to print ALL persistent lists on ALL entities", "[entity_id]");
			cheatMgr.RegisterCheatHandler("printpersistentlists", OnProcessCheat_printpersistentlist, "Prints all persistent lists for a particular entity. Call it with no entity to print ALL persistent lists on ALL entities", "[entity_id]");
			cheatMgr.RegisterCheatHandler("togglecardtext", OnProcessCheat_togglecardtext, "Enables/Disables all card powers text", "togglecardtext");
			cheatMgr.RegisterCheatHandler("togglecardnames", OnProcessCheat_togglecardnames, "Enables/Disables all card names", "togglecardnames");
			cheatMgr.RegisterCheatHandler("toggleracetext", OnProcessCheat_toggleracetext, "Enables/Disables all race and spell school text", "toggleracetext");
			cheatMgr.RegisterCheatHandler("removeplayernames", OnProcessCheat_removeplayernames, "Disables player name banners", "removeplayernames");
			cheatMgr.RegisterCheatHandler("toggleForcingApprenticeGame", OnProcessCheat_toggleForcingApprenticeGame, "Enables/Disables standard games starting as Apprentice Casual", "toggleForcingApprenticeGame");
			cheatMgr.RegisterCategory("collection");
			cheatMgr.RegisterCheatHandler("collectionfirstxp", OnProcessCheat_collectionfirstxp, "Set the number of page and cover flips to zero", "", "");
			cheatMgr.RegisterCheatHandler("resethasseencollectionmanager", OnProcessCheat_HasSeenCollectionManager, "Resets Innkeeper tips for collection manager", "", "");
			cheatMgr.RegisterCheatHandler("cardchangereset", OnProcessCheat_cardchangereset, "Reset the record of which changed cards have already been seen.", "<event_name>");
			cheatMgr.RegisterCheatHandler("showcardchangeevent", OnProcessCheat_showcardchangeevent, "Force show a card change event.", "<event_name>");
			cheatMgr.RegisterCheatHandler("loginpopupsequence", OnProcessCheat_loginpopupsequence, "Show any active login popup sequences.");
			cheatMgr.RegisterCheatHandler("loginpopupreset", OnProcessCheat_loginpopupreset, "Reset game save data for login popup sequences.");
			cheatMgr.RegisterCheatHandler("onlygold", OnProcessCheat_onlygold, "In collection manager, do you want to see gold, nogold, or both?", "<command name>", "");
			cheatMgr.RegisterCheatHandler("exportcards", OnProcessCheat_exportcards, "Export images of cards");
			cheatMgr.RegisterCategory("cosmetics");
			cheatMgr.RegisterCheatHandler("defaultcardback", OnProcessCheat_favoritecardback, "Set your favorite cardback as if through the collection manager", "<cardback id>");
			cheatMgr.RegisterCheatHandler("favoritecardback", OnProcessCheat_favoritecardback, "Set your favorite cardback as if through the collection manager", "<cardback id>");
			cheatMgr.RegisterCheatHandler("favoritehero", OnProcessCheat_favoritehero, "Change your favorite hero for a class (only works from CollectionManager)", "<class_id> <hero_card_id> <hero_premium>");
			cheatMgr.RegisterCheatHandler("exportcardbacks", OnProcessCheat_exportcardbacks, "Export images of card backs");
			cheatMgr.RegisterCheatHandler("finisher", OnProcessCheat_PlayFinisher, "Requests a specific finisher to play.");
			cheatMgr.RegisterCategory("legacy quests and rewards");
			cheatMgr.RegisterCheatHandler("questcompletepopup", OnProcessCheat_questcompletepopup, "Shows the quest complete achievement screen", "<quest_id>", "58");
			cheatMgr.RegisterCheatHandler("questprogresspopup", OnProcessCheat_questprogresspopup, "Pop up a quest progress toast", "<title> <description> <progress> <maxprogress>", "Hello World 3 10");
			cheatMgr.RegisterCheatHandler("questwelcome", OnProcessCheat_questwelcome, "Open list of daily quests", "<fromLogin>", "true");
			cheatMgr.RegisterCheatHandler("newquestvisual", OnProcessCheat_newquestvisual, "Shows a new quest tile, only usable while a quest popup is active");
			cheatMgr.RegisterCheatHandler("fixedrewardcomplete", OnProcessCheat_fixedrewardcomplete, "Shows the visual for a fixed reward", "<fixed_reward_map_id>");
			cheatMgr.RegisterCheatHandler("rewardboxes", OnProcessCheat_rewardboxes, "Open the reward box screen with example rewards", "<card|cardback|gold|dust|random> <num_boxes>", "");
			cheatMgr.RegisterCheatHandler("questchangereset", OnProcessCheat_questchangereset, "Reset the record of which changed quests have already been seen.", "");
			RegisterShopCommands(cheatMgr);
			cheatMgr.RegisterCheatHandler("refreshcurrency", OnProcessCheat_refreshcurrency, "Refresh currency balance", "<runestones|arcane_orbs>");
			cheatMgr.RegisterCheatHandler("loadpersonalizedshop", OnProcessCheat_loadpersonalizedshop, "Load personalized shop", "<page_id>");
			cheatMgr.RegisterCheatHandler("mercpackgrantdiamondcard", OnProcessCheat_mercpackgrantdiamondcard, "Grant a specific diamond card in a merc pack", "<merc_id> <art_variant_id>");
			cheatMgr.RegisterCheatHandler("mercpackduplicate", OnProcessCheat_mercpackduplicate, "Force a specific merc duplicate in a merc pack", "<merc_id> <amount>");
			cheatMgr.RegisterCheatHandler("mercpackforcemercskin", OnProcessCheat_mercpackforcemercskin, "Force a specific merc skin to appear, regardless if it is owned", "<merc_id> <art_variant_id> <premium_type>");
			cheatMgr.RegisterCheatHandler("refreshproductmodels", OnProcessCheat_refreshproductmodels, "Refresh the product data models (widget) list with net bundles stored. Close store before running");
			cheatMgr.RegisterCheatHandler("addproducttag", OnProcessCheat_addproducttag, "Forces a tag onto a product bundle. If force shown, this may modify the product if showing is blocked by other elements", "<pmt_product_id> <tag> <force_show>");
			cheatMgr.RegisterCheatHandler("addproducttiertag", OnProcessCheat_addproducttiertag, "Forces a tag onto a product tier that contains PMT product ID. If force shown, this may modify the product if showing is blocked by other elements", "<pmt_product_id> <tag> <force_show>");
			cheatMgr.RegisterCategory("iks");
			cheatMgr.RegisterCheatHandler("iks", OnProcessCheat_iks, "Open InnKeepersSpecial with a custom url", "<url>");
			cheatMgr.RegisterCheatHandler("iksaction", OnProcessCheat_iksgameaction, "Execute a game action as if IKS was clicked.");
			cheatMgr.RegisterCheatHandler("iksseen", OnProcessCheat_iksseen, "Determine if an IKS message should be seen by its game action.");
			cheatMgr.RegisterCategory("rank");
			cheatMgr.RegisterCheatHandler("seasondialog", OnProcessCheat_seasondialog, "Open the season end dialog", "<rank> [standard|wild|classic]", "bronze5 wild");
			cheatMgr.RegisterCheatHandler("rankrefresh", OnProcessCheat_rankrefresh, "Request medalinfo from server and show rankchange twoscoop after receiving it");
			cheatMgr.RegisterCheatHandler("rankchange", OnProcessCheat_rankchange, "Show a fake rankchange twoscoop", "[rank] [up|down|win|loss] [wild] [winstreak] [chest]", "bronze5 up chest");
			cheatMgr.RegisterCheatHandler("rankreward", OnProcessCheat_rankreward, "Show a fake RankedRewardDisplay for rank (or all ranks up to a rank)", "<rank> [standard|wild|classic|all]", "bronze5 all");
			cheatMgr.RegisterCheatHandler("rankcardback", OnProcessCheat_rankcardback, "Show a fake RankedCardBackProgressDisplay", "<wins> [season_id]", "5 75");
			cheatMgr.RegisterCheatHandler("easyrank", OnProcessCheat_easyrank, "Easier cheat command to set your rank on the util server", "<rank>", "16");
			cheatMgr.RegisterCheatHandler("resetrotationtutorial", OnProcessCheat_resetrotationtutorial, "Cause the user to see the Set Rotation Tutorial again.", "<newbie|veteran>", "newbie|veteran");
			cheatMgr.RegisterCheatHandler("ratingdebug", OnProcessCheat_ratingdebug, "Display debug information regarding rating", "<#> or <standard/wild/classic>", "standard");
			cheatMgr.RegisterCheatHandler("resetrankedintro", OnProcessCheat_resetrankedintro, "Reset game save data values for various tutorial elements for ranked play.");
			cheatMgr.RegisterCheatHandler("localmedaloverride", OnProcessCheat_localmedaloverride, "Sets LOCAL ONLY medal data for a given format type to specified value", "[ft_standard|ft_wild|ft_classic] legend_rank=9001", "off");
			cheatMgr.RegisterCategory("sound/vo");
			cheatMgr.RegisterCheatHandler("playnullsound", OnProcessCheat_playnullsound, "Tell SoundManager to play a null sound.");
			cheatMgr.RegisterCheatHandler("playaudio", OnProcessCheat_playaudio, "Play an audio file by name");
			cheatMgr.RegisterCheatHandler("quote", OnProcessCheat_quote, "", "<character> <line> [sound]", "Innkeeper VO_INNKEEPER_FORGE_COMPLETE_22 VO_INNKEEPER_ARENA_COMPLETE");
			cheatMgr.RegisterCheatHandler("narrative", OnProcessCheat_narrative, "Show a narrative popup from an achievement");
			cheatMgr.RegisterCheatHandler("narrativedialog", OnProcessCheat_narrativedialog, "Show a narrative dialog sequence popup");
			cheatMgr.RegisterCategory("game modes");
			cheatMgr.RegisterCheatHandler("arena", OnProcessCheat_arena, "Runs various arena cheats.", "[subcommand] [subcommand args]", "help");
			cheatMgr.RegisterCheatHandler("retire", OnProcessCheat_retire, "Retires your draft deck", "", "");
			cheatMgr.RegisterCheatHandler("tb_retire", OnProcessCheat_tbRetire, "Retires your Tavern Brawl Session", "", "");
			cheatMgr.RegisterCheatHandler("battlegrounds", OnProcessCheat_battlegrounds, "Queue for a game of Battlegrounds.");
			cheatMgr.RegisterCheatHandler("battlegroundsDuo", OnProcessCheat_battlegroundsDuo, "Queue for a game of Battlegrounds Duo.");
			cheatMgr.RegisterCheatHandler("tb", OnProcessCheat_tavernbrawl, "Run a variety of Tavern Brawl related commands", "[subcommand] [subcommand args]", "view");
			cheatMgr.RegisterCheatHandler("resetTavernBrawlAdventure", OnProcessCheat_ResetTavernBrawlAdventure, "Reset the current Tavern Brawl Adventure progress");
			cheatMgr.RegisterCheatHandler("randomizemercenariesboard", OnProcessCheat_randomizemercenariesboard, "Randomize the mercenaries board visuals", "<isFinalBoss> [seed]", "false 1");
			cheatMgr.RegisterCheatHandler("mercs", OnProcessCheat_mercs, "Run a variety of mercenaries commands.", "[subcommand] [subcommand args]", "help");
			Network.Get().RegisterNetHandler(MercenariesDebugCommandResponse.PacketID.ID, OnProcessCheat_mercs_OnResponse);
			cheatMgr.RegisterCategory("ui");
			cheatMgr.RegisterCheatHandler("demotext", OnProcessCheat_demotext, "", "<line>", "HelloWorld!");
			cheatMgr.RegisterCheatHandler("popuptext", OnProcessCheat_popuptext, "show a popup notification", "<line>", "HelloWorld!");
			cheatMgr.RegisterCheatHandler("alerttext", OnProcessCheat_alerttext, "show a popup alert", "<line>", "HelloWorld!");
			cheatMgr.RegisterCheatHandler("fatalmgrtext", OnProcessCheat_FatalMgrText, "show a fatal error message", "<line>", "HelloWorld!");
			cheatMgr.RegisterCheatHandler("logtext", OnProcessCheat_logtext, "log a line of text", "<level> <line>", "warning WatchOutWorld!");
			cheatMgr.RegisterCheatHandler("logenable", OnProcessCheat_logenable, "temporarily enables a logger", "<logger> <subtype> <enabled>", "Store file/screen/console true");
			cheatMgr.RegisterCheatHandler("loglevel", OnProcessCheat_loglevel, "temporarily sets the min level of a logger", "<logger> <level>", "Store debug");
			cheatMgr.RegisterCheatHandler("reloadgamestrings", OnProcessCheat_reloadgamestrings, "Reload all game strings from GLUE/GLOBAL/etc.");
			cheatMgr.RegisterCheatHandler("attn", OnProcessCheat_userattentionmanager, "Prints out what UserAttentionBlockers, if any, are currently active.");
			cheatMgr.RegisterCheatHandler("banner", OnProcessCheat_banner, "Shows the specified wooden banner (supply a banner_id). If none is supplied, it'll show the latest known banner. Use 'banner list' to view all known banners.", "<banner_id> | list", "33");
			cheatMgr.RegisterCheatHandler("notice", OnProcessCheat_notice, "Show a notice", "<gold|runestones|arcane_orbs|dust|booster|card|cardback|tavern_brawl_rewards|event|license> [data]");
			cheatMgr.RegisterCheatHandler("load_widget", OnProcessCheat_LoadWidget, "Show a widget given a specific guid. If `CHEATED_STATE` exists on a visual controller in the widget, it will be triggered and should be used to help get the widget into the proper location on the screen or any other special test only setup that is needed.");
			cheatMgr.RegisterCheatHandler("clear_widgets", OnProcessCheat_ClearWidgets, "Remove any widgets that were created via the load_widget cheat");
			cheatMgr.RegisterCheatHandler("serverlog", OnProcessCheat_ServerLog, "Log a ServerScript message");
			cheatMgr.RegisterCheatHandler("dialogevent", OnProcessCheat_dialogEvent, "Choose a category of dialog event, and force it to be run again.", "<event_type> or \"reset\"");
			cheatMgr.RegisterCheatHandler("showtip", OnProcessCheat_ShowTip, "Shows a tip from a chosen category (or default)", "[category] [index(optional)]", "4 25");
			cheatMgr.RegisterCategory("social");
			cheatMgr.RegisterCheatHandler("spectate", OnProcessCheat_spectate, "Connects to a game server to spectate", "<ip_address> <port> <game_handle> <spectator_password> [gameType] [missionId]");
			cheatMgr.RegisterCheatHandler("party", OnProcessCheat_party, "Run a variety of party related commands", "[sub command] [subcommand args]", "list");
			cheatMgr.RegisterCheatHandler("raf", OnProcessCheat_raf, "Run a RAF UI related commands", "[subcommand]", "showprogress");
			cheatMgr.RegisterCheatHandler("flist", OnProcessCheat_friendlist, "Run various friends list cheats.", "[subcommand] [subcommand args]", "add remove");
			cheatMgr.RegisterCheatHandler("social", OnProcessCheat_social, "View information about the social list (friends, nearby players, etc)", "[subcommand] [subcommand args]", "list");
			cheatMgr.RegisterCheatHandler("playstartemote", OnProcessCheat_playStartEmote, " the appropriate start, mirror start, or custom start emote on first the enemy hero, then the friendly hero");
			cheatMgr.RegisterCheatHandler("getbgdenylist", OnProcessCheat_getBattlegroundDenyList, "Get Battleground deny list");
			cheatMgr.RegisterCheatHandler("getbgminionpool", OnProcessCheat_getBattlegroundMinionPool, "Get Battleground minion pool");
			cheatMgr.RegisterCheatHandler("getbgheroarmortierlist", OnProcessCheat_getBattlegroundHeroArmorTierList, "Get Battleground Hero Armor Tier List");
			cheatMgr.RegisterCheatHandler("getbgplayeranomaly", OnProcessCheat_getBattlegroundsPlayerAnomaly, "Get Battleground Anomaly list for all players");
			cheatMgr.RegisterCheatHandler("bgduosresettutorial", OnProcessCheat_resetBGDuosTutroialFlags, "Reset Battlegrounds Tutorial Flags");
			cheatMgr.RegisterCheatHandler("combatTimer", OnProcessCheat_toggleBGCombatTimerDisplay, "Toggle BG combat timer debug display");
			cheatMgr.RegisterCheatHandler("setbgbuddyprog", OnProcessCheat_SetBattlegroundHeroBuddyProgress, "Set the progress of Battleground Hero Buddy");
			cheatMgr.RegisterCheatHandler("setbgbuddygained", OnProcessCheat_SetBattlegroundHeroBuddyGained, "Set number Battleground Hero Buddy Gained");
			cheatMgr.RegisterCheatHandler("replacebghero", OnProcessCheat_ReplaceBattlegroundHero, "Replace Battleground Hero");
			cheatMgr.RegisterCheatHandler("enablebgherobuddy", OnProcessCheat_EnableBattlegroundHeroBuddy, "Enable Battleground Hero Buddy Locally");
			cheatMgr.RegisterCheatHandler("bgboard", OnProcessCheat_BattlegroundsBoardFSMManipulate, "Manipulate FSMs for Battlegrounds Board cosmetic effects");
			cheatMgr.RegisterCheatHandler("setbgluckydrawendtime", OnProcessCheat_SetBattlegroundsLuckyDrawEndTime, "Set the end time when lucky draw will ends (in seconds)");
			cheatMgr.RegisterCheatHandler("toggleaiplayer", OnProcessCheat_ToggleAIPlayer, "Toggle if you want to enable/disable AI play for player");
			cheatMgr.RegisterCheatHandler("fakedefeat", OnProcessCheat_FakeDefeat, "Mimic the behavior when player is defeated");
			cheatMgr.RegisterCategory("device");
			cheatMgr.RegisterCheatHandler("lowmemorywarning", OnProcessCheat_lowmemorywarning, "Simulate a low memory warning from mobile.");
			cheatMgr.RegisterCheatHandler("mobile", OnProcessCheat_mobile, "Run Mobile related commands", "subcommand [subcommand args]", "subcommand:login|push|ngdp subcommand args:clear|show|register|unregister");
			cheatMgr.RegisterCheatHandler("edittextdebug", OnProcessCheat_edittextdebug, "Toggle EditText debugging");
			cheatMgr.RegisterCategory("streaming");
			cheatMgr.RegisterCheatHandler("setupdateintention", OnProcessCheat_UpdateIntention, "Set the next \"goal\" for the runtime update manager", "[UpdateIntention]");
			cheatMgr.RegisterCheatHandler("updater", OnProcessCheat_Updater, "Modify the properties of Updater", "[subcommand] [subcommand args]", "speed");
			cheatMgr.RegisterCategory("assets");
			cheatMgr.RegisterCheatHandler("printassethandles", OnProcessCheat_Assets, "Prints outstanding AssetHandles", "[filter]");
			cheatMgr.RegisterCheatHandler("printassetbundles", OnProcessCheat_Assets, "Prints open AssetBundles", "[filter]");
			cheatMgr.RegisterCheatHandler("dumpassets", OnProcessCheat_Assets, "Dumps AssetHandles and AssetBundles to CSV files", "[filter]");
			cheatMgr.RegisterCheatHandler("orphanasset", OnProcessCheat_Assets, "Orphans an AssetHandle");
			cheatMgr.RegisterCheatHandler("orphanprefab", OnProcessCheat_Assets, "Orphans a shared prefab");
			cheatMgr.RegisterCategory("account data");
			cheatMgr.RegisterCheatHandler("account", OnProcessCheat_account, "Account management cheat");
			cheatMgr.RegisterCheatHandler("cloud", OnProcessCheat_cloud, "Run Cloud Storage related commands", "[subcommand]", "set");
			cheatMgr.RegisterCheatHandler("tempaccount", OnProcessCheat_tempaccount, "Run Temporary Account related commands", "[subcommand]", "dialog");
			cheatMgr.RegisterCheatHandler("getgsd", OnProcessCheat_GetGameSaveData, "Request the value of a particular Game Save Data subkey.", "[key] [subkey]", "24 13");
			cheatMgr.RegisterCheatHandler("gsd", OnProcessCheat_GetGameSaveData, "Request the value of a particular Game Save Data subkey.", "[key] [subkey]", "24 13");
			cheatMgr.RegisterCheatHandler("setgsd", OnProcessCheat_SetGameSaveData, "Set the value(s) of a Game Save Data subkey. Can provide multiple values to set a list.", "[key] [subkey] [int_value]", "24 13 2");
			cheatMgr.RegisterCategory("adventure");
			cheatMgr.RegisterCheatHandler("adventureChallengeUnlock", OnProcessCheat_adventureChallengeUnlock, "Show adventure challenge unlock", "<wing number>");
			cheatMgr.RegisterCheatHandler("advevent", OnProcessCheat_advevent, "Trigger an AdventureWingEventTable event.", "<event name>", "PlateOpen");
			cheatMgr.RegisterCheatHandler("showadventureloadingpopup", OnProcessCheat_ShowAdventureLoadingPopup, "Show the popup for loading into the currently-set Adventure mission.");
			cheatMgr.RegisterCheatHandler("hidegametransitionpopup", OnProcessCheat_HideGameTransitionPopup, "Hide any currently shown game transition popup.");
			cheatMgr.RegisterCheatHandler("setallpuzzlesinprogress", OnProcessCheat_SetAllPuzzlesInProgress, "Set the sub-puzzle progress for each puzzle to be on the final puzzle.");
			cheatMgr.RegisterCheatHandler("unlockhagatha", OnProcessCheat_UnlockHagatha, "Set up the hagatha unlock flow. After running the cheat, complete a monster hunt to unlock.");
			cheatMgr.RegisterCheatHandler("setadventurecomingsoon", OnProcessCheat_SetAdventureComingSoon, "Set the Coming Soon state of an adventure.");
			cheatMgr.RegisterCheatHandler("resetsessionvo", OnProcessCheat_ResetSession_VO, "Reset the fact that you've seen once per session related VO, to be able to hear it again.");
			cheatMgr.RegisterCheatHandler("setvochance", OnProcessCheat_SetVOChance_VO, "Set an override on the chance to play a VO line in the adventure. This will only override the chance on VO that won't always play.", "<chance>", "0.1");
			cheatMgr.RegisterCategory("adventure:dungeon run");
			cheatMgr.RegisterCheatHandler("setdrprogress", OnProcessCheat_SetDungeonRunProgress, "Set how many bosses you've defeated during an active run in the provided Adventure.", "[adventure abbreviation] [num bosses] [next boss id (optional)]", "uld 7 46589");
			cheatMgr.RegisterCheatHandler("setdrvictory", OnProcessCheat_SetDungeonRunVictory, "Set victory in the provided Adventure.", "<adventure abbreviation>", "uld");
			cheatMgr.RegisterCheatHandler("setdrdefeat", OnProcessCheat_SetDungeonRunDefeat, "Set defeat and how many bosses you've defeated in the provided Adventure.", "[adventure abbreviation] [num bosses]", "uld 7");
			cheatMgr.RegisterCheatHandler("resetdradventure", OnProcessCheat_ResetDungeonRunAdventure, "Reset the current run for the provided Adventure.", "[adventure abbreviation]", "uld");
			cheatMgr.RegisterCheatAlias("resetdradventure", "resetdrrun");
			cheatMgr.RegisterCheatHandler("resetdrvo", OnProcessCheat_ResetDungeonRun_VO, "Reset the fact that you've seen all VO related to the provided Adventure, to be able to hear it again.", "[adventure abbreviation] [optional:value to set subkeys to]", "uld 1");
			cheatMgr.RegisterCheatHandler("unlockloadout", OnProcessCheat_UnlockLoadout, "Unlock all loadout options for the provided Adventure.", "[adventure abbreviation]", "uld");
			cheatMgr.RegisterCheatHandler("lockloadout", OnProcessCheat_LockLoadout, "Lock all loadout options for the provided Adventure.", "[adventure abbreviation]", "uld");
			cheatMgr.RegisterCategory("adventure:k&c");
			cheatMgr.RegisterCheatHandler("setkcprogress", OnProcessCheat_SetKCProgress, "Set how many bosses you've defeated during an active run in Kobolds & Catacombs.", "[num bosses] [next boss id (optional)]", "7 46589");
			cheatMgr.RegisterCheatHandler("setkcvictory", OnProcessCheat_SetKCVictory, "Set victory in Kobolds & Catacombs.");
			cheatMgr.RegisterCheatHandler("setkcdefeat", OnProcessCheat_SetKCDefeat, "Set defeat and how many bosses you've defeated in Kobolds & Catacombs.", "<num bosses>", "7");
			cheatMgr.RegisterCheatHandler("resetkcvo", OnProcessCheat_ResetKC_VO, "Reset the fact that you've seen all K&C related VO, to be able to hear it again.");
			cheatMgr.RegisterCategory("adventure:witchwood");
			cheatMgr.RegisterCheatHandler("setgilprogress", OnProcessCheat_SetGILProgress, "Set how many bosses you've defeated during an active run in Witchwood.", "[num bosses] [next boss id (optional)]", "7 46589");
			cheatMgr.RegisterCheatHandler("setgilvictory", OnProcessCheat_SetGILVictory, "Set victory in Witchwood.");
			cheatMgr.RegisterCheatHandler("setgildefeat", OnProcessCheat_SetGILDefeat, "Set defeat and how many bosses you've defeated in Witchwood.", "<num bosses>", "7");
			cheatMgr.RegisterCheatHandler("setgilbonus", OnProcessCheat_SetGILBonus, "Set the Witchwood bonus challenge to be active.");
			cheatMgr.RegisterCheatHandler("resetGilAdventure", OnProcessCheat_ResetGILAdventure, "Reset the current Witchwood Adventure run.");
			cheatMgr.RegisterCheatHandler("resetgilvo", OnProcessCheat_ResetGIL_VO, "Reset the fact that you've seen all Witchwood related VO, to be able to hear it again.");
			cheatMgr.RegisterCategory("adventure:rastakhan");
			cheatMgr.RegisterCheatHandler("settrlprogress", OnProcessCheat_SetTRLProgress, "Set how many bosses you've defeated during an active run in Rastakhan.", "[num bosses] [next boss id (optional)]", "7 46589");
			cheatMgr.RegisterCheatHandler("settrlvictory", OnProcessCheat_SetTRLVictory, "Set victory in Rastakhan.");
			cheatMgr.RegisterCheatHandler("settrldefeat", OnProcessCheat_SetTRLDefeat, "Set defeat and how many bosses you've defeated in Rastakhan.", "<num bosses>", "7");
			cheatMgr.RegisterCheatHandler("resettrlvo", OnProcessCheat_ResetTRL_VO, "Reset the fact that you've seen all Rastakhan related VO, to be able to hear it again.");
			cheatMgr.RegisterCategory("adventure:dalaran");
			cheatMgr.RegisterCheatHandler("setdalprogress", OnProcessCheat_SetDALProgress, "Set how many bosses you've defeated during an active run in Dalaran.", "[num bosses] [next boss id (optional)]", "7 46589");
			cheatMgr.RegisterCheatHandler("setdalvictory", OnProcessCheat_SetDALVictory, "Set victory in Dalaran.");
			cheatMgr.RegisterCheatHandler("setdaldefeat", OnProcessCheat_SetDALDefeat, "Set defeat and how many bosses you've defeated in Dalaran.", "<num bosses>", "7");
			cheatMgr.RegisterCheatHandler("resetDalaranAdventure", OnProcessCheat_ResetDalaranAdventure, "Reset the current Dalaran Adventure run, so you can start at the location selection again.");
			cheatMgr.RegisterCheatHandler("setdalheroicprogress", OnProcessCheat_SetDALHeroicProgress, "Set how many bosses you've defeated during an active run in Dalaran Heroic.", "[num bosses] [next boss id (optional)]", "7 46589");
			cheatMgr.RegisterCheatHandler("setdalheroicvictory", OnProcessCheat_SetDALHeroicVictory, "Set victory in Dalaran Heroic.");
			cheatMgr.RegisterCheatHandler("setdalheroicdefeat", OnProcessCheat_SetDALHeroicDefeat, "Set defeat and how many bosses you've defeated in Dalaran Heroic.", "<num bosses>", "7");
			cheatMgr.RegisterCheatHandler("resetDalaranHeroicAdventure", OnProcessCheat_ResetDalaranHeroicAdventure, "Reset the current Dalaran Heroic Adventure run, so you can start at the location selection again.");
			cheatMgr.RegisterCheatHandler("resetdalvo", OnProcessCheat_ResetDAL_VO, "Reset the fact that you've seen all Dalaran related VO, to be able to hear it again.");
			cheatMgr.RegisterCategory("adventure:uldum");
			cheatMgr.RegisterCheatHandler("setuldprogress", OnProcessCheat_SetULDProgress, "Set how many bosses you've defeated during an active run in Uldum.", "[num bosses] [next boss id (optional)]", "7 46589");
			cheatMgr.RegisterCheatHandler("setuldvictory", OnProcessCheat_SetULDVictory, "Set victory in Uldum.");
			cheatMgr.RegisterCheatHandler("setulddefeat", OnProcessCheat_SetULDDefeat, "Set defeat and how many bosses you've defeated in Uldum.", "<num bosses>", "7");
			cheatMgr.RegisterCheatHandler("resetuldrun", OnProcessCheat_ResetUldumAdventure, "Reset the current Uldum Adventure run, so you can start at the location selection again.");
			cheatMgr.RegisterCheatAlias("resetuldrun", "resetUldumAdventure");
			cheatMgr.RegisterCheatHandler("setuldheroicprogress", OnProcessCheat_SetULDHeroicProgress, "Set how many bosses you've defeated during an active run in Uldum Heroic.", "[num bosses] [next boss id (optional)]", "7 46589");
			cheatMgr.RegisterCheatHandler("setuldheroicvictory", OnProcessCheat_SetULDHeroicVictory, "Set victory in Uldum Heroic.");
			cheatMgr.RegisterCheatHandler("setuldheroicdefeat", OnProcessCheat_SetULDHeroicDefeat, "Set defeat and how many bosses you've defeated in Uldum Heroic.", "<num bosses>", "7");
			cheatMgr.RegisterCheatHandler("resetuldheroicrun", OnProcessCheat_ResetUldumHeroicAdventure, "Reset the current Uldum Heroic Adventure run, so you can start at the location selection again.");
			cheatMgr.RegisterCheatAlias("resetuldheroicrun", "resetUldumHeroicAdventure");
			cheatMgr.RegisterCheatHandler("resetuldvo", OnProcessCheat_ResetULD_VO, "Reset the fact that you've seen all Uldum related VO, to be able to hear it again.");
			cheatMgr.DefaultCategory();
			cheatMgr.RegisterCheatHandler("brode", OnProcessCheat_brode, "Brode's personal cheat", "", "");
			cheatMgr.RegisterCheatHandler("freeyourmind", OnProcessCheat_freeyourmind, "And the rest will follow");
		}
		cheatMgr.RegisterCategory("config");
		cheatMgr.RegisterCheatHandler("has", OnProcessCheat_HasOption, "Query whether a Game Option exists.");
		cheatMgr.RegisterCheatHandler("get", OnProcessCheat_GetOption, "Get the value of a Game Option.");
		cheatMgr.RegisterCheatHandler("set", OnProcessCheat_SetOption, "Set the value of a Game Option.");
		cheatMgr.RegisterCheatHandler("getvar", OnProcessCheat_GetVar, "Get the value of a client.config var.");
		cheatMgr.RegisterCheatHandler("setvar", OnProcessCheat_SetVar, "Set the value of a client.config var.");
		cheatMgr.RegisterCheatHandler("delete", OnProcessCheat_DeleteOption, "Delete a Game Option; the absence of option may trigger default behavior");
		cheatMgr.RegisterCheatAlias("delete", "del");
		cheatMgr.RegisterCheatHandler("getregion", OnProcessCheat_GetRegionInfo, "Get Region debug info for the client.");
		cheatMgr.RegisterCheatHandler("resetpreferredregion", OnProcessCheat_ResetPreferredRegion, "Reset the \"PREFERRED_REGION\" Game Option to fallback on client's set Launch Region");
		cheatMgr.RegisterCategory("ui");
		cheatMgr.RegisterCheatHandler("nav", OnProcessCheat_navigation, "Debug Navigation.GoBack");
		cheatMgr.RegisterCheatAlias("nav", "navigate");
		cheatMgr.RegisterCheatHandler("warning", OnProcessCheat_warning, "Show a warning message", "<message>", "Test You're a cheater and you've been warned!");
		cheatMgr.RegisterCheatHandler("fatal", OnProcessCheat_fatal, "Brings up the Fatal Error screen", "<error to display>", "Hearthstone cheated and failed!");
		cheatMgr.RegisterCheatHandler("alert", OnProcessCheat_alert, "Show a popup alert", "header=<string> text=<string> icon=<bool> response=<ok|confirm|cancel|confirm_cancel> oktext=<string> confirmtext=<string>", "header=header text=body text icon=true response=confirm");
		cheatMgr.RegisterCheatAlias("alert", "popup", "dialog");
		cheatMgr.RegisterCheatHandler("exampleui", OnProcessCheat_ExampleUI);
		cheatMgr.RegisterCheatHandler("rankedintropopup", OnProcessCheat_rankedIntroPopup, "Show the Ranked Intro Popup");
		cheatMgr.RegisterCheatHandler("setrotationrotatedboosterspopup", OnProcessCheat_setRotationRotatedBoostersPopup, "Show the Set Rotation Tutorial Popup");
		cheatMgr.RegisterCategory("game modes");
		cheatMgr.RegisterCheatHandler("autodraft", OnProcessCheat_autodraft, "Sets Arena autodraft on/off.", "<on | off>", "on");
		cheatMgr.RegisterCategory("program");
		cheatMgr.RegisterCheatHandler("exit", OnProcessCheat_exit, "Exit the application", "", "");
		cheatMgr.RegisterCheatAlias("exit", "quit");
		cheatMgr.RegisterCheatHandler("pause", (CheatMgr.ProcessCheatCallback)delegate
		{
			HearthstoneApplication.Get().OnApplicationPause(pauseStatus: true);
			return true;
		}, (string)null, (string)null, (string)null);
		cheatMgr.RegisterCheatHandler("unpause", (CheatMgr.ProcessCheatCallback)delegate
		{
			HearthstoneApplication.Get().OnApplicationPause(pauseStatus: false);
			return true;
		}, (string)null, (string)null, (string)null);
		cheatMgr.RegisterCategory("account data");
		cheatMgr.RegisterCheatHandler("clearofflinelocalcache", (CheatMgr.ProcessCheatCallback)delegate
		{
			OfflineDataCache.ClearLocalCacheFile();
			return true;
		}, (string)null, (string)null, (string)null);
		cheatMgr.RegisterCheatHandler("herocount", OnProcessCheat_HeroCount, "Set the hero picker count and reload UI", "number of heroes to display 1-12", "12");
		cheatMgr.DefaultCategory();
		cheatMgr.RegisterCheatHandler("attribution", OnProcessCheat_Attribution);
		cheatMgr.RegisterCheatHandler("crm", OnProcessCheat_CRM);
		cheatMgr.RegisterCategory("progression");
		cheatMgr.RegisterCheatHandler("checkfornewquests", OnProcessCheat_checkfornewquests, "Trigger a check for next quests after n secs (default 0)", "[delaySecs]", "1");
		cheatMgr.RegisterCheatHandler("checkforexpiredquests", OnProcessCheat_checkforexpiredquests, "Trigger a check for expired quests after n secs (default 0)", "[delaySecs]", "1");
		cheatMgr.RegisterCheatHandler("showachievementreward", OnProcessCheat_showachievementreward, "show a fake achievement reward scroll");
		cheatMgr.RegisterCheatHandler("showquestreward", OnProcessCheat_showquestreward, "show a fake quest reward scroll");
		cheatMgr.RegisterCheatHandler("showtrackreward", OnProcessCheat_showtrackreward, "show a fake track reward scroll");
		cheatMgr.RegisterCheatHandler("showquestprogresstoast", OnProcessCheat_showquestprogresstoast, "Pop up a quest progress toast widget", "<quest id>", "2");
		cheatMgr.RegisterCheatHandler("showquestnotification", OnProcessCheat_showquestnotification, "Shows the quest notification popup widget", "<daily|weekly>", "daily");
		cheatMgr.RegisterCheatHandler("showachievementtoast", OnProcessCheat_showachievementtoast, "Pop up a achievement complete toast widget", "<achieve id>", "2");
		cheatMgr.RegisterCheatHandler("showprogtileids", OnProcessCheat_showprogtileids, "Show the quest id or achievement id on quest and achievement tiles");
		cheatMgr.RegisterCheatHandler("showhiddenachievements", OnProcessCheat_showhiddenachievements, "Show hidden achievements in the UI");
		cheatMgr.RegisterCheatHandler("earlyconcedeconfirmationdisabled", OnProcessCheat_earlyConcedeConfirmationDisabled, "Disable the early concede confirmation popup warning");
		cheatMgr.RegisterCheatHandler("simendofgamexp", OnProcessCheat_simendofgamexp, "Simulate different end of game situations and show end of game xp screen.", "<scenario id>", "1");
		cheatMgr.RegisterCheatHandler("terminateendofgamexp", OnProcessCheat_terminateendofgamexp, "Terminate the current end of game xp or simulation");
		cheatMgr.RegisterCheatHandler("showunclaimedtrackrewards", OnProcessCheat_showunclaimedtrackrewards, "Show the reward track's unclaimed rewards popup.");
		cheatMgr.RegisterCategory("tutorials");
		cheatMgr.RegisterCheatHandler("resetdkdecktutorials", OnProcessCheat_resetdkdecktutorials, "Resets all Death Knight deck building tutorials as if the player had not seen them before");
		cheatMgr.RegisterCategory("general");
		cheatMgr.RegisterCheatHandler("log", OnProcessCheat_log);
		cheatMgr.RegisterCheatHandler("ip", OnProcessCheat_IPAddress);
		cheatMgr.RegisterCheatHandler("shownotavernpasswarning", OnProcessCheat_shownotavernpasswarning, "Shows the warning popup when no tavern pass is available");
		cheatMgr.RegisterCheatHandler("setlastrewardtrackseasonseen", OnProcessCheat_setlastrewardtrackseasonseen, "Sets the GSD value of Rewards Track: Season Last Seen");
		cheatMgr.RegisterCheatHandler("apprating", OnProcessCheat_ShowAppRatingPrompt, "Shows the app review popup (Android and iOS only).");
		cheatMgr.RegisterCheatHandler("optin", OnProcessCheat_UpdateAADCSetting, "Gets and sets AADC opt-ins.");
		cheatMgr.RegisterCheatHandler("showpresence", OnProcessCheat_ShowPresence, "Shows the current presence string for the local player");
		cheatMgr.RegisterCheatHandler("showvillagehelppopups", OnProcessCheat_ShowVillageHelpPopups, "Shows all help popups that appear during the village tutorial at once");
		cheatMgr.RegisterCheatHandler("merctraining", OnProcessCheat_MercTraining, "Client side merc training commands to circumvent UI");
		cheatMgr.RegisterCheatHandler("showmercenariestaskcompletetoasts", OnProcessCheat_ShowMercenariesTaskToasts, "Shows X num of task complete toasts for testing UI");
		cheatMgr.RegisterCheatHandler("dumpmaterials", OnProcessCheat_LogMaterialService, "Logs important information regarding the material service");
		cheatMgr.RegisterCheatHandler("logzombies", OnProcessCheat_LogZombies, "Logs zombie objects");
		cheatMgr.RegisterCheatHandler("sendreport", OnProcessCheat_SendReport, "Sends a CS report", "<account id> <issue type> <user source>");
		cheatMgr.RegisterCheatHandler("mercdetails", OnProgressCheat_MercDetails, "Show the Merc Details for a specific merc");
		cheatMgr.RegisterCheatHandler("ackallnotices", OnProcessCheat_AckAllNotices, "Acks all pending notices. Requires client restart to dismiss popups in queue.");
		cheatMgr.RegisterCheatHandler("appsflyer", OnProcessCheat_AppsFlyer, "Apps flyer cheats");
		cheatMgr.RegisterCheatHandler("soundmono", OnProcessCheat_SoundMono, "Sound Mono");
		cheatMgr.RegisterCheatHandler("task_board", OnProgressCheat_TaskBoardCheat, "Cheats related to the task board UI", "<nav_type|search_all>");
		cheatMgr.RegisterCheatHandler("enabledebugmpo", OnProcessCheat_MPOEnable, "Enable/Disable Naive MPO implementation", "<true|false>");
		cheatMgr.RegisterCheatHandler("openfakecatchupbooster", OnProcessCheat_OpenFakeCatchupBooster, "Open a fake catchup booster on the pack opening screen", "<numCards>");
		cheatMgr.RegisterCheatHandler("addcardtosideboard", OnProcessCheat_AddCardToSideboard, "Add a card to the currently edited sideboard", "<cardId>");
		cheatMgr.RegisterCheatHandler("addcardtodeck", OnProcessCheat_AddCardToDeck, "Add a card to the currently edited deck", "<cardId>");
		cheatMgr.RegisterCheatHandler("forceaddcardtodeck", OnProcessCheat_ForceAddCardToDeck, "Add a card to the currently edited deck, even if invalid", "<cardId>");
		yield break;
	}

	private bool OnProcessCheat_askpushnotification(string func, string[] args, string rawArgs)
	{
		bool showWidgetToggle = false;
		if (args.Length != 0)
		{
			string arg = (args[0] ?? string.Empty).Trim();
			if (string.Equals(arg, "show", StringComparison.OrdinalIgnoreCase))
			{
				showWidgetToggle = true;
			}
			else if (string.Equals(arg, "hide", StringComparison.OrdinalIgnoreCase))
			{
				showWidgetToggle = false;
			}
			PushNotificationSoftAskController.ShowPushNotificationSoftAskController(showWidgetToggle);
		}
		return showWidgetToggle;
	}

	public Type[] GetDependencies()
	{
		return new Type[2]
		{
			typeof(CheatMgr),
			typeof(Network)
		};
	}

	public void Shutdown()
	{
		s_instance = null;
	}

	public int GetBoardId()
	{
		return m_boardId;
	}

	public void ClearBoardId()
	{
		m_boardId = 0;
	}

	public bool IsForcingApprenticeGameEnabled()
	{
		return m_forcingApprenticeGameEnabled;
	}

	public bool IsSpeechBubbleEnabled()
	{
		return m_speechBubblesEnabled;
	}

	public bool IsSoundCategoryEnabled(Global.SoundCategory sc)
	{
		if (m_audioChannelEnabled.ContainsKey(sc))
		{
			return m_audioChannelEnabled[sc];
		}
		return true;
	}

	public string GetPlayerTags()
	{
		return m_playerTags;
	}

	public void ClearAllPlayerTags()
	{
		m_playerTags = "";
	}

	public bool IsLaunchingQuickGame()
	{
		return m_quickLaunchState.m_launching;
	}

	public bool ShouldSkipMulligan()
	{
		if (Options.Get().GetBool(Option.SKIP_ALL_MULLIGANS))
		{
			return true;
		}
		return m_quickLaunchState.m_skipMulligan;
	}

	public bool ShouldSkipSendingGetGameState()
	{
		return m_skipSendingGetGameState;
	}

	public bool HandleKeyboardInput()
	{
		if (HearthstoneApplication.IsInternal() && HandleQuickPlayInput())
		{
			return true;
		}
		return false;
	}

	private void ParseErrorText(string[] args, string rawArgs, out string header, out string message)
	{
		header = ((args.Length == 0) ? "[PH] Header" : args[0]);
		if (args.Length <= 1)
		{
			message = "[PH] Message";
			return;
		}
		int messageStartIndex = 0;
		bool insideWord = false;
		for (int i = 0; i < rawArgs.Length; i++)
		{
			if (char.IsWhiteSpace(rawArgs[i]))
			{
				if (insideWord)
				{
					messageStartIndex = i;
					break;
				}
			}
			else
			{
				insideWord = true;
			}
		}
		message = rawArgs.Substring(messageStartIndex).Trim();
	}

	private AlertPopup.PopupInfo GenerateAlertInfo(string rawArgs)
	{
		Map<string, string> map = ParseAlertArgs(rawArgs);
		AlertPopup.PopupInfo info = new AlertPopup.PopupInfo
		{
			m_showAlertIcon = false,
			m_headerText = "Header",
			m_text = "Message",
			m_responseDisplay = AlertPopup.ResponseDisplay.OK,
			m_okText = "OK",
			m_confirmText = "Confirm",
			m_cancelText = "Cancel"
		};
		foreach (KeyValuePair<string, string> pair in map)
		{
			string key = pair.Key;
			string val = pair.Value;
			if (key.Equals("header"))
			{
				info.m_headerText = val;
			}
			else if (key.Equals("text"))
			{
				info.m_text = val;
			}
			else if (key.Equals("response"))
			{
				val = val.ToLowerInvariant();
				if (val.Equals("ok"))
				{
					info.m_responseDisplay = AlertPopup.ResponseDisplay.OK;
				}
				else if (val.Equals("confirm"))
				{
					info.m_responseDisplay = AlertPopup.ResponseDisplay.CONFIRM;
				}
				else if (val.Equals("cancel"))
				{
					info.m_responseDisplay = AlertPopup.ResponseDisplay.CANCEL;
				}
				else if (val.Equals("confirm_cancel") || val.Equals("cancel_confirm"))
				{
					info.m_responseDisplay = AlertPopup.ResponseDisplay.CONFIRM_CANCEL;
				}
			}
			else if (key.Equals("icon"))
			{
				info.m_showAlertIcon = GeneralUtils.ForceBool(val);
			}
			else if (key.Equals("oktext"))
			{
				info.m_okText = val;
			}
			else if (key.Equals("confirmtext"))
			{
				info.m_confirmText = val;
			}
			else if (key.Equals("canceltext"))
			{
				info.m_cancelText = val;
			}
			else if (key.Equals("offset"))
			{
				string[] valSplit = val.Split();
				Vector3 offset = default(Vector3);
				if (valSplit.Length % 2 == 0)
				{
					for (int i = 0; i < valSplit.Length; i += 2)
					{
						string axisKeyStr = valSplit[i].ToLowerInvariant();
						string axisValStr = valSplit[i + 1];
						if (axisKeyStr.Equals("x"))
						{
							offset.x = GeneralUtils.ForceFloat(axisValStr);
						}
						else if (axisKeyStr.Equals("y"))
						{
							offset.y = GeneralUtils.ForceFloat(axisValStr);
						}
						else if (axisKeyStr.Equals("z"))
						{
							offset.z = GeneralUtils.ForceFloat(axisValStr);
						}
					}
				}
				info.m_offset = offset;
			}
			else if (key.Equals("padding"))
			{
				info.m_padding = GeneralUtils.ForceFloat(val);
			}
			else
			{
				if (!key.Equals("align"))
				{
					continue;
				}
				string[] array = val.Split('|');
				for (int j = 0; j < array.Length; j++)
				{
					switch (array[j].ToLower())
					{
					case "left":
						info.m_alertTextAlignment = UberText.AlignmentOptions.Left;
						break;
					case "center":
						info.m_alertTextAlignment = UberText.AlignmentOptions.Center;
						break;
					case "right":
						info.m_alertTextAlignment = UberText.AlignmentOptions.Right;
						break;
					case "top":
						info.m_alertTextAlignmentAnchor = UberText.AnchorOptions.Upper;
						break;
					case "middle":
						info.m_alertTextAlignmentAnchor = UberText.AnchorOptions.Middle;
						break;
					case "bottom":
						info.m_alertTextAlignmentAnchor = UberText.AnchorOptions.Lower;
						break;
					}
				}
			}
		}
		return info;
	}

	private Map<string, string> ParseAlertArgs(string rawArgs)
	{
		Map<string, string> argMap = new Map<string, string>();
		int prevValueStartIndex = -1;
		int prevValueEndIndex = -1;
		string prevKey = null;
		for (int i = 0; i < rawArgs.Length; i++)
		{
			if (rawArgs[i] != '=')
			{
				continue;
			}
			int keyStartIndex = -1;
			for (int j = i - 1; j >= 0; j--)
			{
				char c = rawArgs[j];
				char rightChar = rawArgs[j + 1];
				if (!char.IsWhiteSpace(c))
				{
					keyStartIndex = j;
				}
				if (char.IsWhiteSpace(c) && !char.IsWhiteSpace(rightChar))
				{
					break;
				}
			}
			if (keyStartIndex >= 0)
			{
				prevValueEndIndex = keyStartIndex - 2;
				if (prevKey != null)
				{
					argMap[prevKey] = rawArgs.Substring(prevValueStartIndex, prevValueEndIndex - prevValueStartIndex + 1);
				}
				prevValueStartIndex = i + 1;
				prevKey = rawArgs.Substring(keyStartIndex, i - keyStartIndex).Trim().ToLowerInvariant()
					.Replace("\\n", "\n");
			}
		}
		prevValueEndIndex = rawArgs.Length - 1;
		if (prevKey != null)
		{
			argMap[prevKey] = rawArgs.Substring(prevValueStartIndex, prevValueEndIndex - prevValueStartIndex + 1).Replace("\\n", "\n");
		}
		return argMap;
	}

	private bool OnAlertProcessed(DialogBase dialog, object userData)
	{
		m_alert = (AlertPopup)dialog;
		return true;
	}

	private void HideAlert()
	{
		if (m_alert != null)
		{
			m_alert.Hide();
			m_alert = null;
		}
	}

	private bool HandleQuickPlayInput()
	{
		if (!ServiceManager.IsAvailable<SceneMgr>())
		{
			return false;
		}
		if (!InputCollection.GetKey(KeyCode.LeftShift) && !InputCollection.GetKey(KeyCode.RightShift))
		{
			return false;
		}
		if (InputCollection.GetKeyDown(KeyCode.F12))
		{
			PrintQuickPlayLegend();
			return false;
		}
		if (GetQuickLaunchAvailability() != 0)
		{
			return false;
		}
		ScenarioDbId missionId = ScenarioDbId.INVALID;
		string heroCardId = null;
		foreach (KeyValuePair<KeyCode, ScenarioDbId> pair in s_quickPlayKeyMap)
		{
			KeyCode currKeyCode = pair.Key;
			ScenarioDbId currMissionId = pair.Value;
			if (InputCollection.GetKeyDown(currKeyCode))
			{
				missionId = currMissionId;
				heroCardId = s_opponentHeroKeyMap[currKeyCode];
				break;
			}
		}
		if (missionId == ScenarioDbId.INVALID)
		{
			return false;
		}
		m_quickLaunchState.m_mirrorHeroes = false;
		m_quickLaunchState.m_flipHeroes = false;
		m_quickLaunchState.m_skipMulligan = true;
		m_quickLaunchState.m_opponentHeroCardId = heroCardId;
		if ((InputCollection.GetKey(KeyCode.RightAlt) || InputCollection.GetKey(KeyCode.LeftAlt)) && (InputCollection.GetKey(KeyCode.RightControl) || InputCollection.GetKey(KeyCode.LeftControl)))
		{
			m_quickLaunchState.m_mirrorHeroes = true;
			m_quickLaunchState.m_skipMulligan = false;
			m_quickLaunchState.m_flipHeroes = false;
		}
		else if (InputCollection.GetKey(KeyCode.RightControl) || InputCollection.GetKey(KeyCode.LeftControl))
		{
			m_quickLaunchState.m_flipHeroes = false;
			m_quickLaunchState.m_skipMulligan = false;
			m_quickLaunchState.m_mirrorHeroes = false;
		}
		else if (InputCollection.GetKey(KeyCode.RightAlt) || InputCollection.GetKey(KeyCode.LeftAlt))
		{
			m_quickLaunchState.m_flipHeroes = true;
			m_quickLaunchState.m_skipMulligan = false;
			m_quickLaunchState.m_mirrorHeroes = false;
		}
		if (s_quickPlayNotSkipMulligan.Contains(missionId))
		{
			m_quickLaunchState.m_skipMulligan = false;
		}
		GameType gameType = GameType.GT_VS_AI;
		if (s_scenarioToGameTypeMap.ContainsKey(missionId))
		{
			gameType = s_scenarioToGameTypeMap[missionId];
		}
		LaunchQuickGame((int)missionId, gameType);
		return true;
	}

	private void PrintQuickPlayLegend()
	{
		string statusMessage = $"F1: {GetQuickPlayMissionName(KeyCode.F1)}\nF2: {GetQuickPlayMissionName(KeyCode.F2)}\nF3: {GetQuickPlayMissionName(KeyCode.F3)}\nF4: {GetQuickPlayMissionName(KeyCode.F4)}\nF5: {GetQuickPlayMissionName(KeyCode.F5)}\nF6: {GetQuickPlayMissionName(KeyCode.F6)}\nF7: {GetQuickPlayMissionName(KeyCode.F7)}\nF8: {GetQuickPlayMissionName(KeyCode.F8)}\nF9: {GetQuickPlayMissionName(KeyCode.F9)}\nF10: {GetQuickPlayMissionName(KeyCode.F10)}\nF11: {GetQuickPlayMissionName(KeyCode.F11)}\n(CTRL and ALT will Show mulligan)\nSHIFT + CTRL = Hero on players side\nSHIFT + ALT = Hero on opponent side\nSHIFT + ALT + CTRL = Hero on both sides";
		if (UIStatus.Get() != null)
		{
			UIStatus.Get().AddInfo(statusMessage);
		}
		Debug.Log($"F1: {GetQuickPlayMissionShortName(KeyCode.F1)}  F2: {GetQuickPlayMissionShortName(KeyCode.F2)}  F3: {GetQuickPlayMissionShortName(KeyCode.F3)}  F4: {GetQuickPlayMissionShortName(KeyCode.F4)}  F5: {GetQuickPlayMissionShortName(KeyCode.F5)}  F6: {GetQuickPlayMissionShortName(KeyCode.F6)}  F7: {GetQuickPlayMissionShortName(KeyCode.F7)}  F8: {GetQuickPlayMissionShortName(KeyCode.F8)}  F9: {GetQuickPlayMissionShortName(KeyCode.F9)}\nF10: {GetQuickPlayMissionShortName(KeyCode.F10)}\nF11: {GetQuickPlayMissionShortName(KeyCode.F11)}\n(CTRL and ALT will Show mulligan) -- SHIFT + CTRL = Hero on players side -- SHIFT + ALT = Hero on opponent side -- SHIFT + ALT + CTRL = Hero on both sides");
	}

	private string GetQuickPlayMissionName(KeyCode keyCode)
	{
		return GetQuickPlayMissionName((int)s_quickPlayKeyMap[keyCode]);
	}

	private string GetQuickPlayMissionShortName(KeyCode keyCode)
	{
		return GetQuickPlayMissionShortName((int)s_quickPlayKeyMap[keyCode]);
	}

	private string GetQuickPlayMissionName(int missionId)
	{
		return GetQuickPlayMissionNameImpl(missionId, "NAME");
	}

	private string GetQuickPlayMissionShortName(int missionId)
	{
		return GetQuickPlayMissionNameImpl(missionId, "SHORT_NAME");
	}

	private string GetQuickPlayMissionNameImpl(int missionId, string columnName)
	{
		ScenarioDbfRecord scenarioRecord = GameDbf.Scenario.GetRecord(missionId);
		if (scenarioRecord != null)
		{
			DbfLocValue locString = (DbfLocValue)scenarioRecord.GetVar(columnName);
			if (locString != null)
			{
				return locString.GetString();
			}
		}
		string missionName = missionId.ToString();
		try
		{
			ScenarioDbId enumId = (ScenarioDbId)missionId;
			missionName = enumId.ToString();
		}
		catch (Exception)
		{
		}
		return missionName;
	}

	private QuickLaunchAvailability GetQuickLaunchAvailability()
	{
		if (m_quickLaunchState.m_launching)
		{
			return QuickLaunchAvailability.ACTIVE_GAME;
		}
		if (SceneMgr.Get().IsInGame())
		{
			return QuickLaunchAvailability.ACTIVE_GAME;
		}
		if (GameMgr.Get().IsFindingGame())
		{
			return QuickLaunchAvailability.FINDING_GAME;
		}
		if (SceneMgr.Get().GetNextMode() != 0)
		{
			return QuickLaunchAvailability.SCENE_TRANSITION;
		}
		if (!SceneMgr.Get().IsSceneLoaded())
		{
			return QuickLaunchAvailability.SCENE_TRANSITION;
		}
		if (LoadingScreen.Get().IsTransitioning())
		{
			return QuickLaunchAvailability.ACTIVE_GAME;
		}
		if (CollectionManager.Get() == null || !CollectionManager.Get().IsFullyLoaded())
		{
			return QuickLaunchAvailability.COLLECTION_NOT_READY;
		}
		return QuickLaunchAvailability.OK;
	}

	private void LaunchQuickGame(int missionId, GameType gameType = GameType.GT_VS_AI, FormatType formatType = FormatType.FT_WILD, CollectionDeck deck = null, string aiDeck = null, GameType progFilterOverride = GameType.GT_UNKNOWN)
	{
		string deckName = "";
		long deckId = 0L;
		if (gameType != GameType.GT_BATTLEGROUNDS_PLAYER_VS_AI && gameType != GameType.GT_BATTLEGROUNDS_DUO_VS_AI && gameType != GameType.GT_BATTLEGROUNDS_DUO_1_PLAYER_VS_AI)
		{
			if (deck == null)
			{
				CollectionManager collectionManager = CollectionManager.Get();
				deckId = Options.Get().GetLong(Option.LAST_CUSTOM_DECK_CHOSEN);
				deck = collectionManager.GetDeck(deckId);
				if (deck == null)
				{
					TAG_CLASS defaultClass = TAG_CLASS.MAGE;
					List<CollectionDeck> normalDecks = collectionManager.GetDecks(DeckType.NORMAL_DECK);
					deck = normalDecks.Where((CollectionDeck x) => x.GetClass() == defaultClass).FirstOrDefault();
					if (deck == null)
					{
						deck = normalDecks.FirstOrDefault();
						if (deck == null)
						{
							Debug.LogError("Could not launch quick game because the account has no decks. Please add at least one deck to your account");
							return;
						}
					}
					deckId = deck.ID;
					deckName = deck.Name;
				}
				else
				{
					deckName = deck.Name;
				}
			}
			else
			{
				deckId = deck.ID;
				deckName = deck.Name;
			}
		}
		ReconnectMgr.Get().SetBypassGameReconnect(shouldBypass: true);
		m_quickLaunchState.m_launching = true;
		string missionName = GetQuickPlayMissionName(missionId);
		string message = $"Launching {missionName}\nDeck: {deckName}";
		UIStatus.Get().AddInfo(message);
		TimeScaleMgr.Get().PushTemporarySpeedIncrease(4f);
		SceneMgr.Get().RegisterSceneLoadedEvent(OnSceneLoaded);
		GameMgr.Get().SetPendingAutoConcede(pendingAutoConcede: true);
		GameMgr.Get().RegisterFindGameEvent(OnFindGameEvent);
		GameMgr.Get().FindGame(gameType, formatType, missionId, 0, deckId, aiDeck, null, restoreSavedGameState: false, null, null, 0L, progFilterOverride);
	}

	private void OnSceneLoaded(SceneMgr.Mode mode, PegasusScene scene, object userData)
	{
		if (mode == SceneMgr.Mode.GAMEPLAY)
		{
			HideAlert();
			m_isInGameplayScene = true;
		}
		if (m_isInGameplayScene && mode != SceneMgr.Mode.GAMEPLAY)
		{
			SceneMgr.Get().UnregisterSceneLoadedEvent(OnSceneLoaded);
			m_quickLaunchState = new QuickLaunchState();
			m_isInGameplayScene = false;
		}
	}

	private bool OnFindGameEvent(FindGameEventData eventData, object userData)
	{
		FindGameState state = eventData.m_state;
		if ((uint)(state - 2) <= 1u || (uint)(state - 7) <= 1u || state == FindGameState.SERVER_GAME_CANCELED)
		{
			GameMgr.Get().UnregisterFindGameEvent(OnFindGameEvent);
			m_quickLaunchState = new QuickLaunchState();
		}
		return false;
	}

	private JsonList GetCardlistJson(List<Card> list)
	{
		JsonList cardarray = new JsonList();
		for (int card = 0; card < list.Count; card++)
		{
			JsonNode cardobj = GetCardJson(list[card].GetEntity());
			cardarray.Add(cardobj);
		}
		return cardarray;
	}

	private JsonNode GetCardJson(Entity card)
	{
		if (card == null)
		{
			return null;
		}
		JsonNode cardobj = new JsonNode();
		cardobj["cardName"] = card.GetName();
		cardobj["cardID"] = card.GetCardId();
		cardobj["entityID"] = (long)card.GetEntityId();
		JsonList tagarray = new JsonList();
		if (card.GetTags() != null)
		{
			foreach (KeyValuePair<int, int> tag in card.GetTags().GetMap())
			{
				JsonNode tagobj = new JsonNode();
				string tagname = Enum.GetName(typeof(GAME_TAG), tag.Key);
				if (tagname == null)
				{
					tagname = "NOTAG_" + tag.Key;
				}
				tagobj[tagname] = (long)tag.Value;
				tagarray.Add(tagobj);
			}
			cardobj["tags"] = tagarray;
		}
		JsonList enchantarray = new JsonList();
		List<Entity> enchantments = card.GetEnchantments();
		for (int i = 0; i < enchantments.Count(); i++)
		{
			JsonNode enchantobj = GetCardJson(enchantments[i]);
			enchantarray.Add(enchantobj);
		}
		cardobj["enchantments"] = enchantarray;
		return cardobj;
	}

	private bool OnProcessCheat_error(string func, string[] args, string rawArgs)
	{
		string arg0Lower = ((args == null || args.Length == 0) ? null : args[0]?.ToLower());
		bool num = arg0Lower == "ex" || arg0Lower == "except" || arg0Lower == "exception";
		bool isFatal = arg0Lower == "f" || arg0Lower == "fatal";
		string msg = ((args.Length <= 1) ? null : string.Join(" ", args.Skip(1).ToArray()));
		if (num)
		{
			if (msg == null)
			{
				msg = "This is a simulated Exception.";
			}
			throw new Exception(msg);
		}
		if (isFatal)
		{
			if (msg == null)
			{
				msg = "This is a simulated Fatal Error.";
			}
			Error.AddFatal(FatalErrorReason.CHEAT, msg);
		}
		else
		{
			if (msg == null)
			{
				msg = "This is a simulated Warning message.";
			}
			Error.AddWarning("Warning", msg);
		}
		return true;
	}

	public static bool ProcessAutofillParam(IEnumerable<string> values, string searchTerm, AutofillData autofillData)
	{
		values = values.OrderBy((string v) => v);
		string prefix = autofillData.m_lastAutofillParamPrefix ?? searchTerm ?? string.Empty;
		List<string> matches = ((!string.IsNullOrEmpty(prefix.Trim())) ? values.Where((string v) => v.StartsWith(prefix, StringComparison.InvariantCultureIgnoreCase)).ToList() : values.ToList());
		int index = 0;
		if (autofillData.m_lastAutofillParamMatch != null)
		{
			index = matches.IndexOf(autofillData.m_lastAutofillParamMatch);
			if (index >= 0)
			{
				index += ((!autofillData.m_isShiftTab) ? 1 : (-1));
				if (index >= matches.Count)
				{
					index = 0;
				}
				else if (index < 0)
				{
					index = matches.Count - 1;
				}
			}
		}
		if (index < 0)
		{
			index = 0;
		}
		else if (index >= matches.Count)
		{
			autofillData.m_lastAutofillParamPrefix = null;
			autofillData.m_lastAutofillParamMatch = null;
			float duration = 5f + Mathf.Max(0f, matches.Count - 3);
			duration *= Time.timeScale;
			string availableStr = string.Join("   ", values.ToArray());
			UIStatus.Get().AddError($"No match for '{searchTerm}'. Available params:\n{availableStr}", duration);
			return false;
		}
		autofillData.m_lastAutofillParamPrefix = prefix;
		autofillData.m_lastAutofillParamMatch = matches[index];
		if (matches.Count > 0)
		{
			float duration2 = 5f + Mathf.Max(0f, matches.Count - 3);
			duration2 *= Time.timeScale;
			string availableStr2 = string.Join("   ", matches.ToArray());
			UIStatus.Get().AddInfoNoRichText("Available params:\n" + availableStr2, duration2);
		}
		return true;
	}

	private bool OnProcessCheat_HasOption(string func, string[] args, string rawArgs, AutofillData autofillData)
	{
		string optionString = args[0];
		if (autofillData != null)
		{
			if (args.Length != 1)
			{
				return false;
			}
			return ProcessAutofillParam(from Option v in Enum.GetValues(typeof(Option))
				select EnumUtils.GetString(v), optionString, autofillData);
		}
		Option option;
		try
		{
			option = EnumUtils.GetEnum<Option>(optionString, StringComparison.OrdinalIgnoreCase);
		}
		catch (ArgumentException)
		{
			return false;
		}
		string status = $"HasOption: {EnumUtils.GetString(option)} = {Options.Get().HasOption(option)}";
		Debug.Log(status);
		UIStatus.Get().AddInfo(status);
		return true;
	}

	private bool OnProcessCheat_GetOption(string func, string[] args, string rawArgs, AutofillData autofillData)
	{
		string optionString = args[0];
		if (autofillData != null)
		{
			if (args.Length != 1)
			{
				return false;
			}
			return ProcessAutofillParam(from Option v in Enum.GetValues(typeof(Option))
				select EnumUtils.GetString(v), optionString, autofillData);
		}
		Option option;
		try
		{
			option = EnumUtils.GetEnum<Option>(optionString, StringComparison.OrdinalIgnoreCase);
		}
		catch (ArgumentException)
		{
			return false;
		}
		string status = $"GetOption: {EnumUtils.GetString(option)} = {Options.Get().GetOption(option)}";
		Debug.Log(status);
		UIStatus.Get().AddInfo(status);
		return true;
	}

	private bool OnProcessCheat_SetOption(string func, string[] args, string rawArgs, AutofillData autofillData)
	{
		string optionString = args[0];
		if (autofillData != null)
		{
			if (args.Length != 1)
			{
				return false;
			}
			return ProcessAutofillParam(from Option v in Enum.GetValues(typeof(Option))
				select EnumUtils.GetString(v), optionString, autofillData);
		}
		Option option;
		try
		{
			option = EnumUtils.GetEnum<Option>(optionString, StringComparison.OrdinalIgnoreCase);
		}
		catch (ArgumentException)
		{
			return false;
		}
		if (args.Length < 2)
		{
			return false;
		}
		string prevValue = (Options.Get().HasOption(option) ? Options.Get().GetOption(option).ToString() : "<null>");
		string optionName = EnumUtils.GetString(option);
		string valString = args[1];
		Type type = Options.Get().GetOptionType(option);
		if (type == typeof(bool))
		{
			if (!GeneralUtils.TryParseBool(valString, out var boolVal))
			{
				return false;
			}
			Options.Get().SetBool(option, boolVal);
		}
		else if (type == typeof(int))
		{
			if (!GeneralUtils.TryParseInt(valString, out var intVal))
			{
				return false;
			}
			Options.Get().SetInt(option, intVal);
		}
		else if (type == typeof(long))
		{
			if (!GeneralUtils.TryParseLong(valString, out var longVal))
			{
				return false;
			}
			Options.Get().SetLong(option, longVal);
		}
		else if (type == typeof(float))
		{
			if (!GeneralUtils.TryParseFloat(valString, out var floatVal))
			{
				return false;
			}
			Options.Get().SetFloat(option, floatVal);
		}
		else
		{
			if (!(type == typeof(string)))
			{
				string errorMsg = $"SetOption: {optionName} has unsupported underlying type {type}";
				UIStatus.Get().AddError(errorMsg);
				return true;
			}
			valString = rawArgs.Remove(0, optionString.Length + 1);
			Options.Get().SetString(option, valString);
		}
		switch (option)
		{
		case Option.CURSOR:
			Cursor.visible = Options.Get().GetBool(Option.CURSOR);
			break;
		case Option.GFX_TARGET_FRAME_RATE:
			ServiceManager.Get<IGraphicsManager>().UpdateTargetFramerate(Options.Get().GetInt(Option.GFX_TARGET_FRAME_RATE));
			break;
		}
		string newValue = (Options.Get().HasOption(option) ? Options.Get().GetOption(option).ToString() : "<null>");
		string status = $"SetOption: {optionName} to {valString}.\nPrevious value: {prevValue}\nNew GetOption: {newValue}";
		Debug.Log(status);
		NetCache.Get().DispatchClientOptionsToServer();
		UIStatus.Get().AddInfo(status);
		return true;
	}

	private bool OnProcessCheat_GetVar(string func, string[] args, string rawArgs, AutofillData autofillData)
	{
		string key = args[0];
		if (autofillData != null)
		{
			if (args.Length != 1)
			{
				return false;
			}
			return ProcessAutofillParam(Vars.AllKeys, key, autofillData);
		}
		string status = string.Format("Var: {0} = {1}", key, Vars.Key(key).GetStr(null) ?? "(null)");
		Debug.Log(status);
		UIStatus.Get().AddInfo(status);
		return true;
	}

	private bool OnProcessCheat_SetVar(string func, string[] args, string rawArgs, AutofillData autofillData)
	{
		string key = args[0];
		if (autofillData != null)
		{
			if (args.Length != 1)
			{
				return false;
			}
			return ProcessAutofillParam(Vars.AllKeys, key, autofillData);
		}
		string value = ((args.Length < 2) ? null : args[1]);
		Vars.Key(key).Set(value, permanent: false);
		string status = string.Format("Var: {0} = {1}", key, value ?? "(null)");
		Debug.Log(status);
		UIStatus.Get().AddInfo(status);
		if (key.Equals("Arena.AutoDraft", StringComparison.InvariantCultureIgnoreCase) && DraftDisplay.Get() != null)
		{
			DraftDisplay.Get().StartCoroutine(DraftDisplay.Get().RunAutoDraftCheat());
		}
		return true;
	}

	private bool OnProcessCheat_autodraft(string func, string[] args, string rawArgs)
	{
		string param = args[0];
		bool isOn = string.IsNullOrEmpty(param) || GeneralUtils.ForceBool(param);
		Vars.Key("Arena.AutoDraft").Set(isOn ? "true" : "false", permanent: false);
		if (isOn && DraftDisplay.Get() != null)
		{
			TimeScaleMgr.Get().PushTemporarySpeedIncrease(4f);
			DraftDisplay.Get().StartCoroutine(DraftDisplay.Get().RunAutoDraftCheat());
		}
		else if (!isOn)
		{
			TimeScaleMgr.Get().PopTemporarySpeedIncrease();
		}
		string status = string.Format("Arena autodraft turned {0}.", isOn ? "on" : "off");
		Debug.Log(status);
		UIStatus.Get().AddInfo(status);
		return true;
	}

	private bool OnProcessCheat_HeroCount(string func, string[] args, string rawArgs, AutofillData autofillData)
	{
		try
		{
			int.TryParse(args[0], out var amount);
			switch (SceneMgr.Get().GetMode())
			{
			case SceneMgr.Mode.ADVENTURE:
				GuestHeroPickerTrayDisplay.Get().CheatLoadHeroButtons(amount);
				break;
			case SceneMgr.Mode.TAVERN_BRAWL:
				DeckPickerTrayDisplay.Get().CheatLoadHeroButtons(amount);
				break;
			case SceneMgr.Mode.COLLECTIONMANAGER:
				HeroPickerDisplay.Get().CheatLoadHeroButtons(amount);
				break;
			default:
				return false;
			}
		}
		catch (ArgumentException)
		{
			return false;
		}
		return true;
	}

	private bool OnProcessCheat_onlygold(string func, string[] args, string rawArgs)
	{
		string cmd = args[0].ToLowerInvariant();
		switch (cmd)
		{
		case "gold":
		case "normal":
		case "standard":
			Options.Get().SetString(Option.COLLECTION_PREMIUM_TYPE, cmd);
			break;
		case "both":
			Options.Get().DeleteOption(Option.COLLECTION_PREMIUM_TYPE);
			break;
		default:
			UIStatus.Get().AddError("Unknown cmd: " + (string.IsNullOrEmpty(cmd) ? "(blank)" : cmd) + "\nValid cmds: gold, standard, both");
			return false;
		}
		return true;
	}

	private bool OnProcessCheat_navigation(string func, string[] args, string rawArgs, AutofillData autofillData)
	{
		if (args.Length == 0 || string.IsNullOrEmpty(args[0]))
		{
			return true;
		}
		string[] validCmds = new string[7] { "debug", "dump", "back", "pop", "stack", "history", "show" };
		string cmd = args[0].ToLowerInvariant();
		if (autofillData != null)
		{
			if (!HearthstoneApplication.IsInternal())
			{
				return false;
			}
			return ProcessAutofillParam(validCmds, cmd, autofillData);
		}
		switch (cmd)
		{
		case "debug":
			Navigation.NAVIGATION_DEBUG = args.Length < 2 || GeneralUtils.ForceBool(args[1]);
			if (Navigation.NAVIGATION_DEBUG)
			{
				Navigation.DumpStack();
				UIStatus.Get().AddInfo("Navigation debugging turned on - see Console or output log for nav dump.");
			}
			else
			{
				UIStatus.Get().AddInfo("Navigation debugging turned off.");
			}
			break;
		case "dump":
			Navigation.DumpStack();
			UIStatus.Get().AddInfo("Navigation dumped, see Console or output log.");
			break;
		case "back":
		case "pop":
			if (!HearthstoneApplication.IsInternal())
			{
				return false;
			}
			if (!Navigation.CanGoBack)
			{
				string historyEmptyMsg = (Navigation.IsEmpty ? " Stack is empty." : string.Empty);
				UIStatus.Get().AddInfo("Cannot go back at this time." + historyEmptyMsg);
				return true;
			}
			Navigation.GoBack();
			break;
		case "stack":
		case "history":
		case "show":
		{
			if (!HearthstoneApplication.IsInternal())
			{
				return false;
			}
			string stackString = Navigation.StackDumpString;
			int countNewlines = stackString.Count((char c) => c == '\n');
			float duration = 5 + 3 * countNewlines;
			duration *= Time.timeScale;
			UIStatus.Get().AddInfo(Navigation.IsEmpty ? "Stack is empty." : stackString, duration);
			break;
		}
		default:
		{
			string msg = "Unknown cmd: " + (string.IsNullOrEmpty(cmd) ? "(blank)" : cmd);
			if (HearthstoneApplication.IsInternal())
			{
				msg = msg + "\nValid cmds: " + string.Join(", ", validCmds);
			}
			UIStatus.Get().AddError(msg);
			break;
		}
		}
		return true;
	}

	private bool OnProcessCheat_DeleteOption(string func, string[] args, string rawArgs, AutofillData autofillData)
	{
		string optionString = args[0];
		if (autofillData != null)
		{
			if (args.Length != 1)
			{
				return false;
			}
			return ProcessAutofillParam(from Option v in Enum.GetValues(typeof(Option))
				select EnumUtils.GetString(v), optionString, autofillData);
		}
		Option option;
		try
		{
			option = EnumUtils.GetEnum<Option>(optionString, StringComparison.OrdinalIgnoreCase);
		}
		catch (ArgumentException)
		{
			return false;
		}
		string prevValue = (Options.Get().HasOption(option) ? Options.Get().GetOption(option).ToString() : "<null>");
		Options.Get().DeleteOption(option);
		string newValue = (Options.Get().HasOption(option) ? Options.Get().GetOption(option).ToString() : "<null>");
		string status = $"DeleteOption: {EnumUtils.GetString(option)}\nPrevious Value: {prevValue}\nNew Value: {newValue}";
		Debug.Log(status);
		UIStatus.Get().AddInfo(status);
		return true;
	}

	private bool OnProcessCheat_collectionfirstxp(string func, string[] args, string rawArgs)
	{
		Options.Get().SetInt(Option.COVER_MOUSE_OVERS, 0);
		Options.Get().SetInt(Option.PAGE_MOUSE_OVERS, 0);
		return true;
	}

	private bool OnProcessCheat_board(string func, string[] args, string rawArgs)
	{
		int parsedValue = 0;
		m_boardId = (int.TryParse(args[0], out parsedValue) ? parsedValue : 0);
		UIStatus.Get().AddInfo($"Board for next game set to id {m_boardId}.");
		return true;
	}

	private bool OnProcessCheat_playerTags(string func, string[] args, string rawArgs)
	{
		TryParsePlayerTags(args[0], out m_playerTags);
		if (PartyManager.Get().IsInBattlegroundsParty() && !string.IsNullOrEmpty(m_playerTags))
		{
			PartyManager.Get().SetMyPlayerTagsAttribute();
		}
		return true;
	}

	private bool OnProcessCheat_speechBubbles(string func, string[] args, string rawArgs)
	{
		m_speechBubblesEnabled = !m_speechBubblesEnabled;
		UIStatus.Get().AddInfo(string.Format("Speech bubbles {0}.", m_speechBubblesEnabled ? "enabled" : "disabled"));
		return true;
	}

	private bool OnProcessCheat_playAllThinkEmotes(string func, string[] args, string rawArgs)
	{
		if (args.Length != 1)
		{
			UIStatus.Get().AddError("Invalid params for " + func);
			Log.Gameplay.PrintError("Unrecognized number of arguments. Expected \"" + func + " <player>\"");
			return false;
		}
		int playerId;
		switch (args[0].ToLower())
		{
		case "1":
		case "friendly":
			playerId = 1;
			break;
		case "2":
		case "opponent":
			playerId = 2;
			break;
		default:
			UIStatus.Get().AddError("Invalid params for " + func);
			Log.Gameplay.PrintError("Unrecognized player: \"" + args[0] + "\". Expected \"1\", \"2\", \"friendly\", or \"opponent\"");
			return false;
		}
		Entity hero = GameState.Get()?.GetPlayer(playerId)?.GetHero();
		if (hero == null)
		{
			Log.Gameplay.PrintError($"Unable to find Hero for player {playerId}");
			return false;
		}
		Card heroCard = hero.GetCard();
		Processor.RunCoroutine(PlayEmotesInOrder(heroCard, EmoteType.THINK1, EmoteType.THINK2, EmoteType.THINK3));
		return true;
	}

	private IEnumerator PlayEmotesInOrder(Card heroCard, params EmoteType[] emoteTypes)
	{
		if (heroCard == null || emoteTypes == null)
		{
			yield break;
		}
		int i = 0;
		while (i < emoteTypes.Length)
		{
			if (heroCard.GetEmoteEntry(emoteTypes[i]) == null)
			{
				string error = $"Unable to locate {emoteTypes[i]} emote for {heroCard}";
				UIStatus.Get().AddError(error);
				Log.Gameplay.PrintError(error);
			}
			else
			{
				heroCard.PlayEmote(emoteTypes[i]);
				if (i < emoteTypes.Length - 1)
				{
					yield return new WaitForSeconds(5f);
				}
			}
			int num = i + 1;
			i = num;
		}
	}

	private bool OnProcessCheat_playEmote(string func, string[] args, string rawArgs)
	{
		if (args.Length != 1 && args.Length != 2)
		{
			UIStatus.Get().AddError("Provide 1 to 2 params for " + func + ".");
			Log.Gameplay.PrintError("Unrecognized number of arguments. Expected \"" + func + " <enum_type> <player>\"");
			return true;
		}
		EmoteType selectedEmote = EmoteType.INVALID;
		Enum.TryParse<EmoteType>(args[0], ignoreCase: true, out selectedEmote);
		if (!Enum.IsDefined(typeof(EmoteType), selectedEmote) || selectedEmote == EmoteType.INVALID)
		{
			if (GameMgr.Get().IsBattlegrounds())
			{
				int selectedEvent = 0;
				int.TryParse(args[0], out selectedEvent);
				if (selectedEvent >= 101 && selectedEvent <= 119)
				{
					GameState.Get().GetGameEntity().SendCustomEvent(selectedEvent);
					return true;
				}
			}
			Array enumNames = Enum.GetNames(typeof(EmoteType));
			StringBuilder builder = new StringBuilder();
			int index = 0;
			foreach (string name in enumNames)
			{
				if (index != 0)
				{
					builder.Append(index);
					builder.Append(" = ");
					builder.Append(name);
					builder.Append('\n');
				}
				index++;
			}
			string enumNameString = builder.ToString();
			UIStatus.Get().AddError("Invalid first param for " + func + ". See \"Messages\".");
			Log.Gameplay.PrintError("Unrecognized <enum_type>.\nFor Battlegrounds, try a num [101-119]. Some don't play every time you call it.\n" + $"Try a num [1-{enumNames.Length - 1}] or a string:\n" + enumNameString);
			return true;
		}
		int playerId = 1;
		if (args.Length == 2)
		{
			if (GameMgr.Get().IsBattlegrounds())
			{
				int.TryParse(args[1], out playerId);
				GameState.Get().GetGameEntity().PlayAlternateEnemyEmote(playerId, selectedEmote);
				return true;
			}
			switch (args[1].ToLower())
			{
			case "1":
			case "friendly":
				playerId = 1;
				break;
			case "2":
			case "opponent":
				playerId = 2;
				break;
			default:
				UIStatus.Get().AddError("Invalid second param for " + func + ". See \"Messages\".");
				Log.Gameplay.PrintError("Unrecognized player: \"" + args[1] + "\". Expected \"1\", \"2\", \"friendly\", or \"opponent\"");
				return true;
			}
		}
		Card heroCard = GameState.Get()?.GetPlayer(playerId)?.GetHero()?.GetCard();
		if (heroCard == null)
		{
			Log.Gameplay.PrintError("Unable to find Hero for current player");
			return false;
		}
		heroCard.PlayEmote(selectedEmote);
		return true;
	}

	private bool OnProcessCheat_playAllMissionHeroPowerLines(string func, string[] args, string rawArgs)
	{
		if (args.Length > 1 || args[0] != string.Empty)
		{
			UIStatus.Get().AddError("Invalid params for " + func);
			Log.Gameplay.PrintError("Unrecognized number of arguments. Expected 0 arguments.");
			return false;
		}
		GameEntity game = GameState.Get().GetGameEntity();
		if (game == null)
		{
			return false;
		}
		string methodName = "GetBossHeroPowerRandomLines";
		MethodInfo method = game.GetType().GetMethod(methodName);
		if (method == null)
		{
			Log.Gameplay.PrintError("This game mode lacks hero power lines.");
			return false;
		}
		if (!(method.Invoke(game, null) is List<string> heroPowerLines))
		{
			return false;
		}
		Gameplay.Get().StartCoroutine(LoadAndPlayVO(heroPowerLines));
		return true;
	}

	private bool OnProcessCheat_playAllMissionIdleLines(string func, string[] args, string rawArgs)
	{
		if (args.Length > 1 || args[0] != string.Empty)
		{
			UIStatus.Get().AddError("Invalid params for " + func);
			Log.Gameplay.PrintError("Unrecognized number of arguments. Expected 0 arguments.");
			return false;
		}
		GameEntity game = GameState.Get().GetGameEntity();
		if (game == null)
		{
			return false;
		}
		string methodName = "GetIdleLines";
		MethodInfo method = game.GetType().GetMethod(methodName);
		if (method == null)
		{
			Log.Gameplay.PrintError("This game mode lacks idle lines.");
			return false;
		}
		if (!(method.Invoke(game, null) is List<string> idleLines))
		{
			return false;
		}
		Gameplay.Get().StartCoroutine(LoadAndPlayVO(idleLines));
		return true;
	}

	private bool OnProcessCheat_playLegendaryHeroVFX(string func, string[] args, string rawArgs)
	{
		Player player = GameState.Get().GetFriendlySidePlayer();
		if (player == null)
		{
			Log.Gameplay.PrintError("Player doesn't exist.  Make sure to run from within battlegrounds match");
			return false;
		}
		Entity hero = player.GetHero();
		if (hero == null)
		{
			Log.Gameplay.PrintError("Hero doesn't exist.  Make sure to run from within battlegrounds match");
			return false;
		}
		Card c = hero.GetCard();
		BaconLHSConfig skinconfig = null;
		if (c != null)
		{
			skinconfig = c.LegendaryHeroSkinConfig;
		}
		if (skinconfig == null)
		{
			Log.Gameplay.PrintError("Unable to load legendary hero skin config");
			return false;
		}
		if (args.Length == 0)
		{
			Log.Gameplay.PrintError("playLegendaryHeroVFX must have at least one argument");
			return false;
		}
		switch (args[0].ToLower())
		{
		case "socketin":
			if (!skinconfig.TryActivateVFX_SocketIn())
			{
				Log.Gameplay.PrintError("Could not play socket in VFX");
				return false;
			}
			return true;
		case "combatstart":
			if (!skinconfig.TryActivateVFX_CombatStart())
			{
				Log.Gameplay.PrintError("Could not play combat start in VFX");
				return false;
			}
			return true;
		case "winstreak":
		{
			if (int.TryParse(args[1], out var winstreaknum))
			{
				if (!skinconfig.TryActivateVFX_WinStreak(winstreaknum))
				{
					Log.Gameplay.PrintError($"Could not activate winstreak with id {winstreaknum}");
					return false;
				}
				return true;
			}
			Log.Gameplay.PrintError("second argument of winstreak not a number:  " + args[1]);
			return false;
		}
		default:
			Log.Gameplay.PrintError("invalid vfx " + args[0] + ".  valid vfx options are socketin, combatstart, winstreak");
			return false;
		}
	}

	private bool OnProcessCheat_playLegendaryHeroVO(string func, string[] args, string rawArgs)
	{
		Player player = GameState.Get().GetFriendlySidePlayer();
		if (player == null)
		{
			Log.Gameplay.PrintError("Player doesn't exist.  Make sure to run from within battlegrounds match");
			return false;
		}
		Entity hero = player.GetHero();
		if (hero == null)
		{
			Log.Gameplay.PrintError("Hero doesn't exist.  Make sure to run from within battlegrounds match");
			return false;
		}
		Card c = hero.GetCard();
		BaconLHSConfig skinconfig = null;
		if (c != null)
		{
			skinconfig = c.LegendaryHeroSkinConfig;
		}
		if (skinconfig == null)
		{
			Log.Gameplay.PrintError("Unable to load legendary hero skin config");
			return false;
		}
		List<string> voLines = new List<string>();
		if (args.Length == 0 || args[0] == "")
		{
			voLines = skinconfig.GetAllVOLines();
		}
		int linenum;
		switch (args[0].ToLower())
		{
		case "picked":
			voLines.Add(skinconfig.m_VOPicked);
			break;
		case "startgame":
			voLines.Add(skinconfig.m_VOStartOfGame);
			break;
		case "winstreak":
			if (args.Length == 1)
			{
				foreach (BaconLHSConfig.ValueLine line3 in skinconfig.m_VOWinStreak)
				{
					voLines.Add(line3.m_VOLine);
				}
				break;
			}
			if (int.TryParse(args[1], out linenum))
			{
				if (skinconfig.CheckWinStreakLine(linenum, out var line4))
				{
					voLines.Add(line4);
					break;
				}
				Log.Gameplay.PrintError("No VO available for winstreak of " + args[1]);
				return false;
			}
			Log.Gameplay.PrintError("second argument of winstreak not a number:  " + args[1]);
			return false;
		case "greet":
			if (args.Length == 1)
			{
				voLines = skinconfig.m_VOGreet;
				break;
			}
			if (int.TryParse(args[1], out linenum))
			{
				if (linenum >= 0 && linenum < skinconfig.m_VOGreet.Count)
				{
					voLines.Add(skinconfig.m_VOGreet[linenum]);
					break;
				}
				Log.Gameplay.PrintError($"attempt to access greet VO number {args[1]} but max is {skinconfig.m_VOGreet.Count - 1}");
				return false;
			}
			Log.Gameplay.PrintError("second argument of greet not a number:  " + args[1]);
			return false;
		case "bartendergreet":
		{
			if (args.Length == 1)
			{
				foreach (BaconLHSConfig.CardSpecificLine line2 in skinconfig.m_VOBartenderGreet)
				{
					voLines.Add(line2.m_VOLine);
				}
				break;
			}
			if (skinconfig.TryGetAllBartenderGreet(args[1], out var bartenderVO))
			{
				if (args.Length == 2)
				{
					voLines = bartenderVO;
					break;
				}
				if (int.TryParse(args[2], out linenum))
				{
					if (linenum >= 0 && linenum < bartenderVO.Count)
					{
						voLines.Add(bartenderVO[linenum]);
						break;
					}
					Log.Gameplay.PrintError($"attempt to access bartendergreet VO number {args[2]} for bartender {args[1]} but max is {bartenderVO.Count - 1}");
					return false;
				}
				Log.Gameplay.PrintError("third argument of bartendergreet not a number:  " + args[2]);
				return false;
			}
			Log.Gameplay.PrintError("second argument of bartendergreet not in dictionary:  " + args[1]);
			return false;
		}
		case "herogreet":
		{
			if (args.Length == 1)
			{
				foreach (BaconLHSConfig.CardSpecificLine line in skinconfig.m_VOHeroGreet)
				{
					voLines.Add(line.m_VOLine);
				}
				break;
			}
			if (skinconfig.TryGetAllHeroGreet(args[1], out var heroVO))
			{
				if (args.Length == 2)
				{
					voLines = heroVO;
					break;
				}
				if (int.TryParse(args[2], out linenum))
				{
					if (linenum >= 0 && linenum < heroVO.Count)
					{
						voLines.Add(heroVO[linenum]);
						break;
					}
					Log.Gameplay.PrintError($"attempt to access herogreet VO number {args[2]} for hero {args[1]} but max is {heroVO.Count - 1}");
					return false;
				}
				Log.Gameplay.PrintError("third argument of herogreet not a number:  " + args[2]);
				return false;
			}
			Log.Gameplay.PrintError("second argument of herogreet not in dictionary:  " + args[1]);
			return false;
		}
		default:
			Log.Gameplay.PrintError("invalid sfx " + args[0] + ".  valid sfx options are picked, startgame, winstreak, greet, bartendergreet, herogreet");
			break;
		}
		Gameplay.Get().StartCoroutine(LoadAndPlayVO(voLines, 3f));
		return true;
	}

	private bool OnProcessCheat_playBattlegroundsGuideVO(string func, string[] args, string rawArgs, AutofillData autofillData)
	{
		if (autofillData != null)
		{
			if ((rawArgs.EndsWith(" ") && args.Length == 1) || args.Length == 2)
			{
				string subCmd = ((args.Length == 1) ? string.Empty : args[1]);
				return ProcessAutofillParam(Enum.GetNames(typeof(BaconGuideConfig.HumanReadableVOLineCategory)), subCmd, autofillData);
			}
			return false;
		}
		if (args.Length > 3 || args.Length < 2)
		{
			UIStatus.Get().AddError("Invalid params for " + func);
			Log.Gameplay.PrintError("Unrecognized number of arguments. Expected 2 or 3 arguments.");
			return false;
		}
		BaconGuideConfig guideConfig = TB_BaconShop.LoadGuideConfig(args[0]);
		if (null == guideConfig)
		{
			Log.Gameplay.PrintError("Unable to load guide config for " + args[0]);
			return false;
		}
		List<string> voLines = guideConfig.GetLinesByHumanReadableName(args[1]);
		if (voLines.Count == 0)
		{
			Log.Gameplay.PrintError("No VO lines found for category " + args[1]);
			return false;
		}
		if (args.Count() == 3)
		{
			int voLineIndex = 0;
			if (!int.TryParse(args[2], out voLineIndex))
			{
				Log.Gameplay.PrintError("Unable to parse index from third argument " + args[2]);
				return false;
			}
			voLineIndex--;
			if (voLineIndex < 0 || voLineIndex >= voLines.Count)
			{
				Log.Gameplay.PrintError("Invalid index in third argument " + args[2]);
				return false;
			}
			string selectedVOLine = voLines[voLineIndex];
			voLines = new List<string>();
			voLines.Add(selectedVOLine);
		}
		Gameplay.Get().StartCoroutine(LoadAndPlayVO(voLines));
		return true;
	}

	private IEnumerator LoadAndPlayVO(List<string> assets, float delayBetweenVo = 10f)
	{
		if (assets == null || assets.Count == 0)
		{
			yield break;
		}
		foreach (string asset in assets)
		{
			if (SoundLoader.LoadSound(asset, OnVoLoaded))
			{
				if (asset != assets.Last())
				{
					yield return new WaitForSeconds(delayBetweenVo);
				}
			}
			else
			{
				string error = "Error loading asset " + asset.ToString();
				Log.Gameplay.PrintError(error);
				UIStatus.Get().AddError(error);
			}
		}
	}

	private void OnVoLoaded(AssetReference assetRef, GameObject go, object callbackData)
	{
		if (!(go == null) && !string.IsNullOrEmpty(assetRef))
		{
			Debug.LogFormat("Now playing \"{0}\"", assetRef.ToString());
			AudioSource sound = go.GetComponent<AudioSource>();
			SoundManager.Get().PlayPreloaded(sound);
			string[] prefabGuidSplit = assetRef.ToString().Split(':');
			string localizedTextKey = prefabGuidSplit[0].Substring(0, prefabGuidSplit[0].Length - ".prefab".Length);
			Actor opposingHero = GameState.Get().GetOpposingSidePlayer().GetHero()
				.GetCard()
				.GetActor();
			NotificationManager notificationManager = NotificationManager.Get();
			Notification notification = notificationManager.CreateSpeechBubble(GameStrings.Get(localizedTextKey), Notification.SpeechBubbleDirection.TopRight, opposingHero, bDestroyWhenNewCreated: false);
			notificationManager.DestroyNotification(notification, sound.clip.length);
		}
	}

	private bool OnProcessCheat_audioChannel(string func, string[] args, string rawArgs)
	{
		if (args.Length == 0 || string.IsNullOrEmpty(args[0]))
		{
			StringBuilder allSoundCategories = new StringBuilder();
			foreach (Global.SoundCategory sc in Enum.GetValues(typeof(Global.SoundCategory)))
			{
				allSoundCategories.Append(string.Format("\n{0}: {1}", sc, (!m_audioChannelEnabled.ContainsKey(sc) || m_audioChannelEnabled[sc]) ? "enabled" : "disabled"));
			}
			UIStatus.Get().AddInfo($"Audio channels:{allSoundCategories.ToString()}", 5f);
			return true;
		}
		if (args.Length > 2)
		{
			UIStatus.Get().AddError($"Argument format: [audio channel name] [on/off]");
			return true;
		}
		try
		{
			Global.SoundCategory sc2 = (Global.SoundCategory)Enum.Parse(typeof(Global.SoundCategory), args[0], ignoreCase: true);
			if (args.Length == 1 || string.IsNullOrEmpty(args[1]))
			{
				UIStatus.Get().AddInfo(string.Format("Audio channel {0} is {1}", sc2, m_audioChannelEnabled[sc2] ? "on" : "off"));
				return true;
			}
			if (args[1].ToLower() != "on" && args[1].ToLower() != "off")
			{
				UIStatus.Get().AddError($"Second argument must be \"on\" or \"off\"");
				return true;
			}
			m_audioChannelEnabled[sc2] = args[1].ToLower() == "on";
			SoundManager.Get().UpdateCategoryVolume(sc2);
			UIStatus.Get().AddInfo(string.Format("Audio channel {0} has been {1}", sc2, m_audioChannelEnabled[sc2] ? "enabled" : "disabled"));
		}
		catch (ArgumentException)
		{
			UIStatus.Get().AddError($"{args[0]} is not an audio channel. Type audiochannel to see a list of channels.");
		}
		return true;
	}

	private bool OnProcessCheat_audioChannelGroup(string func, string[] args, string rawArgs)
	{
		if (args.Length == 0 || string.IsNullOrEmpty(args[0]))
		{
			StringBuilder allSoundCategories = new StringBuilder();
			foreach (string group in m_audioChannelGroups.Keys)
			{
				allSoundCategories.Append($"\n{group}");
			}
			UIStatus.Get().AddInfo($"Audio channel groups:{allSoundCategories.ToString()}", 5f);
			return true;
		}
		if (args.Length != 2)
		{
			UIStatus.Get().AddError($"Argument format: [audio channel group name] [on/off]");
			return true;
		}
		if (!m_audioChannelGroups.ContainsKey(args[0].ToUpper()))
		{
			UIStatus.Get().AddError($"{args[0]} is not an audio channel group. Type audiochannelgroup to see a list of channel groups.");
			return true;
		}
		if (args[1].ToLower() != "on" && args[1].ToLower() != "off")
		{
			UIStatus.Get().AddError($"Second argument must be \"on\" or \"off\"");
			return true;
		}
		foreach (Global.SoundCategory sc in m_audioChannelGroups[args[0].ToUpper()])
		{
			if (m_audioChannelEnabled.ContainsKey(sc))
			{
				m_audioChannelEnabled[sc] = args[1].ToLower() == "on";
				SoundManager.Get().UpdateCategoryVolume(sc);
			}
		}
		UIStatus.Get().AddInfo(string.Format("Audio channel group {0} has been {1}", args[0], (args[1].ToLower() == "on") ? "enabled" : "disabled"));
		return true;
	}

	private bool OnProcessCheat_tracert(string func, string[] args, string rawArgs)
	{
		string host = string.Empty;
		if (args.Length < 1 || string.IsNullOrEmpty(rawArgs))
		{
			if (Network.Get() != null)
			{
				GameServerInfo serverInfo = Network.Get().GetLastGameServerJoined();
				if (serverInfo != null)
				{
					host = serverInfo.Address;
				}
			}
			if (string.IsNullOrEmpty(host))
			{
				UIStatus.Get().AddError("No host is defined yet! Please make a game first or set host argument!");
				return true;
			}
		}
		else
		{
			host = args[0];
		}
		if (host.Equals("help"))
		{
			UIStatus.Get().AddInfo("USAGE: tracert [host]\n 'host' can be omitted if game is connected to server");
		}
		else
		{
			TracertReporter.ReportTracertInfo(host);
			UIStatus.Get().AddInfo("It's called with '" + host + "'.");
		}
		return true;
	}

	private bool TryParsePlayerTags(string input, out string output)
	{
		if (string.IsNullOrEmpty(input))
		{
			UIStatus.Get().AddInfo($"Player tags cleared.");
			output = input;
			return true;
		}
		string[] tagValues = input.Split(',');
		if (tagValues.Length > 20)
		{
			output = "";
			UIStatus.Get().AddError($"{tagValues.Length} tag values found, but only {20} tag values can be passed.");
			return false;
		}
		string[] array = tagValues;
		foreach (string tagValue in array)
		{
			if (string.IsNullOrEmpty(tagValue))
			{
				continue;
			}
			string[] tagValueArr = tagValue.Split('=');
			if (tagValueArr.Length != 2)
			{
				output = "";
				UIStatus.Get().AddError($"Invalid tag/value entry: \"{tagValue}\". Format is \"TagId=Value\".");
				return false;
			}
			int tag = 0;
			int value = 0;
			if (!int.TryParse(tagValueArr[0], out tag))
			{
				output = "";
				UIStatus.Get().AddError($"Invalid tagId: \"{tagValueArr[0]}\". Must be an integer.");
				return false;
			}
			if (!int.TryParse(tagValueArr[1], out value))
			{
				value = GameUtils.TranslateCardIdToDbId(tagValueArr[1], showWarning: true);
				if (value == 0)
				{
					output = "";
					UIStatus.Get().AddError($"Invalid tagValue: \"{tagValueArr[1]}\". Must be an integer.");
					return false;
				}
			}
			if (tag > 999999)
			{
				output = "";
				UIStatus.Get().AddError($"Invalid tagId: \"{tag}\". Must be < {999999}.");
				return false;
			}
			if (tag <= 0)
			{
				output = "";
				UIStatus.Get().AddError($"Invalid tagId: \"{tag}\". Must be > 0.");
				return false;
			}
			if (value > 999999)
			{
				output = "";
				UIStatus.Get().AddError($"Invalid tagValue: \"{value}\". Must be < {999999}.");
				return false;
			}
		}
		UIStatus.Get().AddInfo($"Player tags set for next game.");
		output = input;
		return true;
	}

	private bool TryParseArenaChoices(string[] input, out string[] output)
	{
		List<string> discoveredValues = new List<string>();
		bool success = input.Length != 0;
		for (int i = 0; i < input.Length; i++)
		{
			string idValue = input[i].Replace(",", "");
			int cardId = 0;
			if (!int.TryParse(idValue, out cardId))
			{
				cardId = GameUtils.TranslateCardIdToDbId(idValue);
				if (cardId == 0)
				{
					UIStatus.Get().AddError($"Invalid tagValue: \"{idValue}\". Must be an integer or valid card Id.");
					success = false;
					break;
				}
				idValue = cardId.ToString();
			}
			if (cardId > 999999)
			{
				UIStatus.Get().AddError($"Invalid card ID: \"{cardId}\". Must be < {999999}.");
				success = false;
				break;
			}
			if (cardId <= 0)
			{
				UIStatus.Get().AddError($"Invalid card ID: \"{cardId}\". Must be > 0.");
				success = false;
				break;
			}
			discoveredValues.Add(idValue);
		}
		output = discoveredValues.ToArray();
		return success;
	}

	private bool TryParseNamedArgs(string[] args, out Map<string, NamedParam> values)
	{
		values = new Map<string, NamedParam>();
		for (int i = 0; i < args.Length; i++)
		{
			string[] split = args[i].Trim().Split('=');
			if (split.Length > 1)
			{
				values.Add(split[0], new NamedParam(split[1]));
			}
		}
		return values.Count > 0;
	}

	private bool OnProcessCheat_HasSeenCollectionManager(string func, string[] args, string rawArgs)
	{
		Options.Get().SetBool(Option.HAS_SEEN_COLLECTIONMANAGER, val: false);
		return true;
	}

	private bool OnProcessCheat_brode(string func, string[] args, string rawArgs)
	{
		NotificationManager.Get().CreateInnkeeperQuote(UserAttentionBlocker.ALL, new Vector3(133.1f, NotificationManager.DEPTH, 54.2f), GameStrings.Get("VO_INNKEEPER_FORGE_1WIN"), "VO_INNKEEPER_ARENA_1WIN.prefab:31bb13e800c74c0439ee1a7bfc1e3499");
		return true;
	}

	private bool On_ProcessCheat_bug(string func, string[] args, string rawArgs)
	{
		return true;
	}

	private bool On_ProcessCheat_ANR(string func, string[] args, string rawArgs)
	{
		if (!ExceptionReporter.Get().IsEnabledANRMonitor)
		{
			UIStatus.Get().AddInfo("ANR Monitor of ExceptionReporter is disabled");
			return true;
		}
		try
		{
			m_waitTime = float.Parse(args[0]);
		}
		catch
		{
		}
		m_showedMessage = false;
		Processor.RegisterUpdateDelegate(SimulatorPauseUpdate);
		return true;
	}

	private void SimulatorPauseUpdate()
	{
		UIStatus.Get().AddInfo("Wait for " + m_waitTime + " seconds");
		if (m_showedMessage)
		{
			Thread.Sleep((int)(m_waitTime * 1000f));
			Processor.UnregisterUpdateDelegate(SimulatorPauseUpdate);
		}
		m_showedMessage = true;
	}

	private bool OnProcessCheat_igm(string func, string[] args, string rawArgs)
	{
		return true;
	}

	private bool OnProcessCheat_msgui(string func, string[] args, string rawArgs)
	{
		string cmd = "show";
		if (args.Length != 0 && !string.IsNullOrEmpty(args[0]))
		{
			cmd = args[0];
		}
		if ("add".StartsWith(cmd))
		{
			AddMessagePopupForArgs(args);
		}
		else if ("help".StartsWith(cmd))
		{
			UIStatus.Get().AddInfo("USAGE: msgui [add] [text|shop|launch|change|empty] [imageType|pid|launchEffectId|launchEffectColor|launchEffectSoundId|url|changeObjecTtype|cardChangeCount]");
		}
		return true;
	}

	private void AddMessagePopupForArgs(string[] args)
	{
		MessageUIData data = ConstructUIDataFromArgs(args);
		if (data == null)
		{
			Log.InGameMessage.PrintDebug("Failed to construct UI Data for test IGM");
			return;
		}
		MessagePopupDisplay popupDisplay = ServiceManager.Get<MessagePopupDisplay>();
		if (popupDisplay == null)
		{
			UIStatus.Get().AddError("Message Popup Display was not available to show a message");
			return;
		}
		List<MessageUIData> dataList = new List<MessageUIData>();
		dataList.Add(data);
		popupDisplay.AddMessages(dataList);
	}

	private static MessageUIData ConstructUIDataFromArgs(string[] args)
	{
		MessageLayoutType layoutType = GetLayoutTypeIfAvailable(args);
		if (layoutType == MessageLayoutType.INVALID)
		{
			return null;
		}
		MessageUIData data = new MessageUIData
		{
			LayoutType = layoutType,
			MessageData = ConstructContentDataForMessage(layoutType, args)
		};
		if (data.MessageData == null)
		{
			return null;
		}
		return data;
	}

	private static IMessageContent ConstructContentDataForMessage(MessageLayoutType layoutType, string[] args)
	{
		switch (layoutType)
		{
		case MessageLayoutType.TEXT:
			return ConstructTestTextMsg(args);
		case MessageLayoutType.SHOP:
			return ConstructTestShopMsg(args);
		case MessageLayoutType.LAUNCH:
			return ConstructTestLaunchMessage(args);
		case MessageLayoutType.CHANGE:
			return ConstructTestChangeMessage(args);
		case MessageLayoutType.EMPTY:
			return ConstructEmptyMailboxMessage(args);
		default:
			UIStatus.Get().AddError($"Unsupported content type {layoutType}");
			return null;
		}
	}

	private static ChangeMessageContent ConstructTestChangeMessage(string[] args)
	{
		string url = string.Empty;
		int cardCount = 0;
		string changeObjectType = string.Empty;
		if (args.Length > 2 && !string.IsNullOrEmpty(args[2]))
		{
			url = args[2];
		}
		if (args.Length > 3 && !string.IsNullOrEmpty(args[3]))
		{
			changeObjectType = args[3].ToLower();
			if (changeObjectType != "card" && changeObjectType != "hero")
			{
				UIStatus.Get().AddError("The third argument for a change in game message should be either 'card' or 'hero' to determine what object type we want to display.");
				return null;
			}
		}
		List<ChangeMessageItemInformation> changeItems = new List<ChangeMessageItemInformation>();
		if (args.Length > 4 && !string.IsNullOrEmpty(args[4]))
		{
			cardCount = int.Parse(args[4]);
		}
		if ((changeObjectType.Equals("hero") && cardCount > 1) || (changeObjectType.Equals("card") && cardCount > 5))
		{
			UIStatus.Get().AddError($"The card count given ({cardCount}) is too high for the object type given({changeObjectType})");
		}
		for (int i = 0; i < cardCount; i++)
		{
			if (!(changeObjectType == "hero"))
			{
				if (changeObjectType == "card")
				{
					changeItems.Add(m_changeMessageCardsExamples[i]);
				}
				else
				{
					UIStatus.Get().AddError("Unrecognized change object type passed in for in game message.");
				}
			}
			else
			{
				changeItems.Add(m_changeMessageHeroExamples[i]);
			}
		}
		return new ChangeMessageContent
		{
			Title = "Lorem Ipsum",
			BodyText = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Donec ut rhoncus ante. Donec in pretium felis. Duis mollis purus a ante mollis luctus. Nulla hendrerit gravida nulla non convallis. Vivamus vel ligula a mi porta porta et at magna. Nulla euismod diam eget arcu pharetra scelerisque. In id sem a ipsum maximus cursus. In pulvinar fermentum dolor, at ultrices ipsum congue nec.",
			Url = url,
			ChangeItems = changeItems
		};
	}

	private static LaunchMessageContent ConstructTestLaunchMessage(string[] args)
	{
		string image = "Logo";
		string effectId = string.Empty;
		string effectColor = string.Empty;
		string effectSoundId = string.Empty;
		if (args.Length > 2 && !string.IsNullOrEmpty(args[2]))
		{
			image = args[2];
		}
		if (args.Length > 3 && !string.IsNullOrEmpty(args[3]))
		{
			effectId = args[3];
		}
		if (args.Length > 4 && !string.IsNullOrEmpty(args[4]))
		{
			effectColor = args[4];
		}
		if (args.Length > 5 && !string.IsNullOrEmpty(args[5]))
		{
			effectSoundId = args[5];
		}
		return new LaunchMessageContent
		{
			IconType = image,
			Title = "Lorem Ipsum",
			TextBody = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Donec ut rhoncus ante. Donec in pretium felis. Duis mollis purus a ante mollis luctus. Nulla hendrerit gravida nulla non convallis. Vivamus vel ligula a mi porta porta et at magna. Nulla euismod diam eget arcu pharetra scelerisque. In id sem a ipsum maximus cursus. In pulvinar fermentum dolor, at ultrices ipsum congue nec.",
			Effect = new LaunchMessageEffectContent
			{
				EffectId = effectId,
				EffectColor = effectColor,
				EffectSoundId = effectSoundId
			}
		};
	}

	private static TextMessageContent ConstructTestTextMsg(string[] args)
	{
		string image = "Logo";
		if (args.Length > 2 && !string.IsNullOrEmpty(args[2]))
		{
			image = args[2];
		}
		return new TextMessageContent
		{
			ImageType = image,
			ImageMaterial = null,
			Title = "Lorem Ipsum",
			TextBody = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Donec ut rhoncus ante. Donec in pretium felis. Duis mollis purus a ante mollis luctus. Nulla hendrerit gravida nulla non convallis. Vivamus vel ligula a mi porta porta et at magna. Nulla euismod diam eget arcu pharetra scelerisque. In id sem a ipsum maximus cursus. In pulvinar fermentum dolor, at ultrices ipsum congue nec."
		};
	}

	private static ShopMessageContent ConstructTestShopMsg(string[] args)
	{
		long pid = 10747L;
		if (args.Length > 2 && !string.IsNullOrEmpty(args[2]) && !long.TryParse(args[2], out pid))
		{
			UIStatus.Get().AddError("Invalid product id for show igm: " + args[2]);
			return null;
		}
		return new ShopMessageContent
		{
			Title = "Lorem Ipsum",
			TextBody = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Donec ut rhoncus ante. Donec in pretium felis. Duis mollis purus a ante mollis luctus. Nulla hendrerit gravida nulla non convallis. Vivamus vel ligula a mi porta porta et at magna. Nulla euismod diam eget arcu pharetra scelerisque. In id sem a ipsum maximus cursus. In pulvinar fermentum dolor, at ultrices ipsum congue nec.",
			ProductID = pid
		};
	}

	private static MessageLayoutType GetLayoutTypeIfAvailable(string[] args)
	{
		MessageLayoutType layoutType = MessageLayoutType.TEXT;
		if (args.Length > 1 && !string.IsNullOrEmpty(args[1]))
		{
			string contentString = args[1].ToLower();
			switch (contentString)
			{
			case "text":
				layoutType = MessageLayoutType.TEXT;
				break;
			case "shop":
				layoutType = MessageLayoutType.SHOP;
				break;
			case "launch":
				layoutType = MessageLayoutType.LAUNCH;
				break;
			case "change":
				layoutType = MessageLayoutType.CHANGE;
				break;
			case "empty":
				layoutType = MessageLayoutType.EMPTY;
				break;
			default:
				layoutType = MessageLayoutType.INVALID;
				UIStatus.Get().AddError("Invalid message type to show " + contentString);
				break;
			}
		}
		return layoutType;
	}

	private static TextMessageContent ConstructEmptyMailboxMessage(string[] args)
	{
		string image = "mercs";
		if (args.Length > 2 && !string.IsNullOrEmpty(args[2]))
		{
			image = args[2];
		}
		return new TextMessageContent
		{
			ImageType = image,
			Title = "Lorem Ipsum"
		};
	}

	private bool On_ProcessCheat_assert(string func, string[] args, string rawArgs)
	{
		string[] allCmds = new string[6] { "help", "invalid", "initial", "adventures", "battlegrounds", "mercenaries" };
		if (args.Length < 1 || string.IsNullOrEmpty(rawArgs))
		{
			ExceptionReporter.Get().ReportAssertion(new Exception("Test assertion cheat"));
			return true;
		}
		string cmd = args[0].ToLower();
		string desc = null;
		if (1 < args.Length)
		{
			desc = args[1];
			_ = args[1];
		}
		if (string.IsNullOrEmpty(desc))
		{
			desc = "User requested assertion";
		}
		IAssetLoader assetLoader = AssetLoader.Get();
		if ("invalid".StartsWith(cmd))
		{
			HearthstoneApplication.Get().StartCoroutine(Co_On_ProcessCheat_assert_LoadInvalidAssets());
		}
		else if ("initial".StartsWith(cmd))
		{
			assetLoader.LoadAsset<AudioClip>(new AssetReference("HeroSkin_JusticeJaina.wav:944c543bdda2aff44b33a6b6fd367473"));
		}
		else if ("adventures".StartsWith(cmd))
		{
			assetLoader.LoadAsset<GameObject>(new AssetReference("AdventureWingDef_BOH_11_Faelin.prefab:3469f00be3b03f147a90c7bf8b92e2dd"));
		}
		else if ("battlegrounds".StartsWith(cmd))
		{
			assetLoader.LoadAsset<GameObject>(new AssetReference("VO_TB_BaconShopBob_SKIN_AH_Zephrys_Elemental_Emote_Flavor_01.prefab:49a46b24ae0a2014990f762229953905"));
		}
		else if ("mercenaries".StartsWith(cmd))
		{
			assetLoader.LoadAsset<GameObject>(new AssetReference("CollectionPageMercenary.prefab:755d5774f7e26ad47ac7e99c7d904858"));
		}
		else if (cmd == "help")
		{
			UIStatus.Get().AddInfo("USAGE: assert [test name]\n Where(substring): " + string.Join(" | ", allCmds) + "\nassert assetloader");
		}
		return true;
	}

	private IEnumerator Co_On_ProcessCheat_assert_LoadInvalidAssets()
	{
		IAssetLoader assetLoader = AssetLoader.Get();
		List<Action> loadingActions = new List<Action>
		{
			delegate
			{
				assetLoader.LoadAsset<GameObject>("Cheat_LoadAsset1:00000000000000000000000000000000");
			},
			delegate
			{
				AssetHandle<GameObject> assetHandle = null;
				assetLoader.LoadAsset(ref assetHandle, "Cheat_LoadAsset2:00000000000000000000000000000000");
			},
			delegate
			{
				assetLoader.LoadAsset<GameObject>("Cheat_LoadAsset3:00000000000000000000000000000000", null);
			},
			delegate
			{
				assetLoader.InstantiatePrefab("Cheat_InstantiatePrefab1:00000000000000000000000000000000");
			},
			delegate
			{
				assetLoader.InstantiatePrefab("Cheat_InstantiatePrefab2:00000000000000000000000000000000", null);
			},
			delegate
			{
				assetLoader.GetOrInstantiateSharedPrefab("Cheat_GetOrInstantiateSharedPrefab1:00000000000000000000000000000000");
			},
			delegate
			{
				assetLoader.GetOrInstantiateSharedPrefab("Cheat_GetOrInstantiateSharedPrefab2:00000000000000000000000000000000", null);
			},
			delegate
			{
				assetLoader.LoadMaterial("Cheat_LoadMaterial1:00000000000000000000000000000000", null);
			},
			delegate
			{
				assetLoader.LoadMaterial("Cheat_LoadMaterial2:00000000000000000000000000000000");
			},
			delegate
			{
				assetLoader.LoadTexture("Cheat_LoadTexture1:00000000000000000000000000000000", null);
			},
			delegate
			{
				assetLoader.LoadTexture("Cheat_LoadTexture2:00000000000000000000000000000000");
			},
			delegate
			{
				assetLoader.LoadMesh("Cheat_LoadMesh1:00000000000000000000000000000000", null);
			},
			delegate
			{
				assetLoader.LoadMesh("Cheat_LoadMesh2:00000000000000000000000000000000");
			},
			delegate
			{
				assetLoader.LoadGameObject("Cheat_LoadGameObject:00000000000000000000000000000000", null);
			}
		};
		float waitBetweenLoads = 5f + UnityEngine.Random.value;
		int i = 0;
		while (i < loadingActions.Count)
		{
			UIStatus.Get().AddInfo($"Loading invalid asset {i + 1} / {loadingActions.Count}. Please wait.");
			loadingActions[i]();
			if (i < loadingActions.Count - 1)
			{
				yield return new WaitForSecondsRealtime(waitBetweenLoads);
			}
			int num = i + 1;
			i = num;
		}
		UIStatus.Get().AddInfo("Done.");
	}

	private bool On_ProcessCheat_crash(string func, string[] args, string rawArgs)
	{
		string[] allCmds = new string[6] { "help", "cs", "plugin", "nativeinlib", "javainlib", "report" };
		if (args.Length < 1 || string.IsNullOrEmpty(rawArgs))
		{
			throw new Exception("User requested exception");
		}
		string cmd = args[0].ToLower();
		string desc = null;
		string secondArg = null;
		if (args.Length > 1)
		{
			desc = args[1];
			secondArg = args[1];
		}
		if (string.IsNullOrEmpty(desc))
		{
			desc = "User requested exception";
		}
		if ("plugin".StartsWith(cmd))
		{
			if (PlatformSettings.IsMobileRuntimeOS)
			{
				MobileCallbackManager.CreateCrashPlugInLayer(desc);
			}
			else
			{
				UIStatus.Get().AddInfo("Plug-in crash is only for Android platform");
			}
		}
		else if ("javainlib".StartsWith(cmd))
		{
			if (PlatformSettings.RuntimeOS == OSCategory.Android)
			{
				MobileCallbackManager.CreateCrashInNativeLayer("java:" + desc);
			}
			else
			{
				UIStatus.Get().AddInfo("Java crash is only for Android platforms");
			}
		}
		else if ("nativeinlib".StartsWith(cmd))
		{
			if (PlatformSettings.IsMobileRuntimeOS)
			{
				MobileCallbackManager.CreateCrashInNativeLayer(desc);
			}
			else
			{
				UIStatus.Get().AddInfo("Native crash is only for mobile platforms");
			}
		}
		else
		{
			if ("cs".StartsWith(cmd))
			{
				throw new Exception(desc);
			}
			if ("restricted".StartsWith(cmd))
			{
				if (string.IsNullOrEmpty(secondArg))
				{
					secondArg = (ExceptionReporterControl.Get().IsRestrictedReport ? "off" : "on");
				}
				ExceptionReporterControl.Get().IsRestrictedReport = string.Equals(secondArg, "on", StringComparison.OrdinalIgnoreCase);
				UIStatus.Get().AddInfo("Exception report restriction: " + secondArg);
			}
			else if ("report".StartsWith(cmd))
			{
				if (desc.Length < 36)
				{
					UIStatus.Get().AddInfo(desc + " seems not UUID format!");
				}
			}
			else if ("t5report".StartsWith(cmd))
			{
				if (string.IsNullOrEmpty(secondArg))
				{
					secondArg = (ExceptionReporterControl.Get().IsEnabledT5MobileReport ? "off" : "on");
				}
				ExceptionReporterControl.Get().IsEnabledT5MobileReport = string.Equals(secondArg, "on", StringComparison.OrdinalIgnoreCase);
				UIStatus.Get().AddInfo("Register as exception in t5/mobile: " + secondArg);
			}
			else if (cmd == "help")
			{
				UIStatus.Get().AddInfo("USAGE: crash [where] [exception title]\n Where(substring): " + string.Join(" | ", allCmds) + "\ncrash t5report on/off\ncrash restricted on/off");
			}
		}
		return true;
	}

	private bool OnProcessCheat_questcompletepopup(string func, string[] args, string rawArgs)
	{
		int achieveId = 0;
		Achievement achieve = (int.TryParse(rawArgs, out achieveId) ? AchieveManager.Get().GetAchievement(achieveId) : null);
		if (achieve == null)
		{
			UIStatus.Get().AddError($"{func}: please specify a valid Quest ID");
			return true;
		}
		QuestToast.ShowQuestToast(UserAttentionBlocker.ALL, null, updateCacheValues: false, achieve);
		return true;
	}

	private bool OnProcessCheat_narrative(string func, string[] args, string rawArgs)
	{
		if (args.Length == 1 && string.Equals(args[0], "clear", StringComparison.OrdinalIgnoreCase))
		{
			List<Option> options = NarrativeManager.Get().Cheat_ClearAllSeen();
			string msg = string.Format("Narrative seen options cleared:\n{0}", string.Join(", ", options.Select((Option o) => EnumUtils.GetString(o)).ToArray()));
			UIStatus.Get().AddInfo(msg);
			return true;
		}
		int achieveId = 0;
		if ((int.TryParse(rawArgs, out achieveId) ? AchieveManager.Get().GetAchievement(achieveId) : null) == null)
		{
			UIStatus.Get().AddError($"{func}: please specify a valid Quest ID");
			return true;
		}
		NarrativeManager.Get().OnQuestCompleteShown(achieveId);
		NarrativeManager.Get().ShowOutstandingQuestDialogs();
		return true;
	}

	private bool OnProcessCheat_narrativedialog(string func, string[] args, string rawArgs)
	{
		int dialogID = 0;
		CharacterDialogSequence sequence = (int.TryParse(rawArgs, out dialogID) ? new CharacterDialogSequence(dialogID) : null);
		if (sequence == null)
		{
			UIStatus.Get().AddError($"{func}: please specify a valid Dialog ID");
			return true;
		}
		NarrativeManager.Get().PushDialogSequence(sequence);
		return true;
	}

	private bool OnProcessCheat_questwelcome(string func, string[] args, string rawArgs)
	{
		bool fromLogin = true;
		if (args.Length != 0 && !string.IsNullOrEmpty(args[0]))
		{
			GeneralUtils.TryParseBool(args[0], out fromLogin);
		}
		WelcomeQuests.Show(UserAttentionBlocker.ALL, fromLogin);
		return true;
	}

	private bool OnProcessCheat_newquestvisual(string func, string[] args, string rawArgs)
	{
		if (WelcomeQuests.Get() == null)
		{
			UIStatus.Get().AddError("WelcomeQuests object is not active - try using 'questwelcome' cheat first.");
			return true;
		}
		int achieveId = 0;
		Achievement achieve = (int.TryParse(rawArgs, out achieveId) ? AchieveManager.Get().GetAchievement(achieveId) : null);
		if (achieve == null)
		{
			UIStatus.Get().AddError($"{func}: please specify a valid Quest ID");
			return true;
		}
		WelcomeQuests.Get().GetFirstQuestTile().SetupTile(achieve, QuestTile.FsmEvent.QuestGranted);
		return true;
	}

	private bool OnProcessCheat_questprogresspopup(string func, string[] args, string rawArgs)
	{
		int achieveId = 0;
		Achievement achieve = ((args.Length != 0 && int.TryParse(args[0], out achieveId)) ? AchieveManager.Get().GetAchievement(achieveId) : null);
		int count = 1;
		string questName;
		string questDescription;
		int progress;
		int maxProgress;
		if (achieve == null)
		{
			if (achieveId != 0)
			{
				UIStatus.Get().AddError("unknown Achieve with ID " + achieveId);
				return true;
			}
			if (args.Length < 4)
			{
				UIStatus.Get().AddError("please specify an Achieve ID or the following params:\n<title> <description> <progress> <maxprogress>");
				return true;
			}
			questName = args[0];
			questDescription = args[1];
			int.TryParse(args[2], out progress);
			int.TryParse(args[3], out maxProgress);
		}
		else
		{
			questName = achieve.Name;
			questDescription = achieve.Description;
			progress = achieve.Progress;
			maxProgress = achieve.MaxProgress;
		}
		for (int i = 0; i < args.Length; i++)
		{
			string[] argParts = args[i].Split('=');
			if (argParts.Length >= 2)
			{
				string argName = argParts[0]?.ToLower();
				string argVal = argParts[1];
				if (argName == "count" && !int.TryParse(argVal, out count))
				{
					UIStatus.Get().AddError($"Unable to parse parameter #{i + 1} as integer: {argVal}");
					return true;
				}
			}
		}
		if (GameToastMgr.Get() != null)
		{
			if (progress >= maxProgress)
			{
				progress = maxProgress - 1;
			}
			for (int j = 0; j < count; j++)
			{
				GameToastMgr.Get().AddQuestProgressToast(achieveId, questName, questDescription, progress, maxProgress);
			}
			return true;
		}
		UIStatus.Get().AddError("GameToastMgr is null!");
		return true;
	}

	private bool OnProcessCheat_retire(string func, string[] args, string rawArgs)
	{
		if (DemoMgr.Get().GetMode() != DemoMode.BLIZZCON_2013)
		{
			return false;
		}
		DraftManager draft = DraftManager.Get();
		if (draft == null)
		{
			return false;
		}
		Network.Get().DraftRetire(draft.GetDraftDeck().ID, draft.GetSlot(), draft.CurrentSeasonId);
		return true;
	}

	private bool OnProcessCheat_tbRetire(string func, string[] args, string rawArgs)
	{
		Network.Get().TavernBrawlRetire();
		return true;
	}

	private bool OnProcessCheat_notice(string func, string[] args, string rawArgs)
	{
		if (args.Count() < 2)
		{
			UIStatus.Get().AddError("notice cheat requires 2 params: [string]type [int]data [OPTIONAL int]data2 [OPTIONAL bool]quest toast?");
			return true;
		}
		int data = -1;
		int.TryParse(args[1], out data);
		if (data < 0)
		{
			UIStatus.Get().AddError($"{data}: please specify a valid Notice Data Value");
			return true;
		}
		string data2 = null;
		if (args.Length > 2)
		{
			data2 = args[2];
		}
		bool questToast = false;
		if (args.Length > 3)
		{
			questToast = GeneralUtils.ForceBool(args[3]);
		}
		NetCache.ProfileNotice notice = null;
		Achievement achieve = new Achievement();
		List<RewardData> rewards = achieve.Rewards;
		switch (args[0]?.ToLower())
		{
		case "gold":
			if (questToast)
			{
				GoldRewardData goldReward = new GoldRewardData();
				goldReward.Amount = data;
				rewards.Add(goldReward);
			}
			else
			{
				notice = CreateCurrencyNotice(PegasusShared.CurrencyType.CURRENCY_TYPE_GOLD);
			}
			break;
		case "runestones":
			notice = (ShopUtils.IsMainVirtualCurrencyType(CurrencyType.CN_RUNESTONES) ? CreateCurrencyNotice(PegasusShared.CurrencyType.CURRENCY_TYPE_CN_RUNESTONES) : CreateCurrencyNotice(PegasusShared.CurrencyType.CURRENCY_TYPE_ROW_RUNESTONES));
			break;
		case "arcane_orbs":
			if (questToast)
			{
				rewards.Add(RewardUtils.CreateArcaneOrbRewardData(data));
			}
			else
			{
				notice = CreateCurrencyNotice(PegasusShared.CurrencyType.CURRENCY_TYPE_CN_ARCANE_ORBS);
			}
			break;
		case "dust":
			if (questToast)
			{
				ArcaneDustRewardData dustReward = new ArcaneDustRewardData();
				dustReward.Amount = data;
				rewards.Add(dustReward);
			}
			else
			{
				notice = new NetCache.ProfileNoticeRewardDust
				{
					Amount = data
				};
			}
			break;
		case "bgtoken":
			if (questToast)
			{
				BattlegroundsTokenRewardData tokenReward = new BattlegroundsTokenRewardData();
				tokenReward.Amount = data;
				rewards.Add(tokenReward);
			}
			else
			{
				notice = CreateCurrencyNotice(PegasusShared.CurrencyType.CURRENCY_TYPE_BG_TOKEN);
			}
			break;
		case "booster":
		{
			int boosterID = 1;
			if (!string.IsNullOrEmpty(data2))
			{
				int.TryParse(data2, out boosterID);
			}
			if (GameDbf.Booster.GetRecord(boosterID) == null)
			{
				UIStatus.Get().AddError($"Booster ID is invalid: {boosterID}");
				return true;
			}
			if (questToast)
			{
				BoosterPackRewardData boosterReward = new BoosterPackRewardData();
				boosterReward.Id = boosterID;
				boosterReward.Count = data;
				rewards.Add(boosterReward);
			}
			else
			{
				notice = new NetCache.ProfileNoticeRewardBooster
				{
					Count = data,
					Id = boosterID
				};
			}
			break;
		}
		case "card":
		{
			string cardID = "NEW1_040";
			if (!string.IsNullOrEmpty(data2))
			{
				int cardDbId = -1;
				int.TryParse(data2, out cardDbId);
				cardID = ((cardDbId <= 0) ? data2 : GameUtils.TranslateDbIdToCardId(cardDbId));
			}
			if (GameUtils.GetCardRecord(cardID) == null)
			{
				UIStatus.Get().AddError($"Card ID is invalid: {cardID}");
				return true;
			}
			if (questToast)
			{
				CardRewardData cardReward = new CardRewardData();
				cardReward.CardID = cardID;
				cardReward.Count = Mathf.Clamp(data, 1, 2);
				rewards.Add(cardReward);
			}
			else
			{
				notice = new NetCache.ProfileNoticeRewardCard
				{
					CardID = cardID,
					Quantity = Mathf.Clamp(data, 1, 2)
				};
			}
			break;
		}
		case "cardback":
			if (GameDbf.CardBack.GetRecord(data) == null)
			{
				UIStatus.Get().AddError($"Cardback ID is invalid: {data}");
				return true;
			}
			if (questToast)
			{
				CardBackRewardData cardBackRewardData = new CardBackRewardData();
				cardBackRewardData.CardBackID = data;
				rewards.Add(cardBackRewardData);
			}
			else
			{
				notice = new NetCache.ProfileNoticeRewardCardBack
				{
					CardBackID = data
				};
			}
			break;
		case "tavern_brawl_rewards":
		{
			NetCache.ProfileNoticeTavernBrawlRewards profileNoticeTavernBrawlRewards = new NetCache.ProfileNoticeTavernBrawlRewards();
			profileNoticeTavernBrawlRewards.Wins = data;
			profileNoticeTavernBrawlRewards.Chest = RewardUtils.GenerateTavernBrawlRewardChest_CHEAT(mode: profileNoticeTavernBrawlRewards.Mode = (data2.Equals("heroic") ? TavernBrawlMode.TB_MODE_HEROIC : TavernBrawlMode.TB_MODE_NORMAL), wins: data);
			notice = profileNoticeTavernBrawlRewards;
			break;
		}
		case "mercenaries_map_chest":
			notice = new NetCache.ProfileNoticeMercenariesRewards
			{
				Chest = RewardUtils.GenerateMercenariesMapRewardChest_CHEAT(),
				Origin = NetCache.ProfileNotice.NoticeOrigin.NOTICE_ORIGIN_MERCENARIES
			};
			break;
		case "mercenaries_consolation_reward":
			notice = new NetCache.ProfileNoticeMercenariesRewards
			{
				Chest = RewardUtils.GenerateMercenariesConsolationReward_CHEAT(),
				Origin = NetCache.ProfileNotice.NoticeOrigin.NOTICE_ORIGIN_MERCENARIES,
				RewardType = ProfileNoticeMercenariesRewards.RewardType.REWARD_TYPE_PVE_CONSOLATION
			};
			break;
		case "mercenaries_autoretire_reward":
			notice = new NetCache.ProfileNoticeMercenariesRewards
			{
				Chest = RewardUtils.GenerateMercenariesConsolationReward_CHEAT(),
				Origin = NetCache.ProfileNotice.NoticeOrigin.NOTICE_ORIGIN_MERCENARIES,
				RewardType = ProfileNoticeMercenariesRewards.RewardType.REWARD_TYPE_PVE_AUTO_RETIRE
			};
			break;
		case "mercenaries_season_reward":
			notice = new NetCache.ProfileNoticeMercenariesSeasonRewards
			{
				Chest = RewardUtils.GenerateMercenariesSeasonReward_CHEAT(),
				Origin = NetCache.ProfileNotice.NoticeOrigin.NOTICE_ORIGIN_MERCENARIES,
				RewardAssetId = LettucePlayDisplay.SortedRewardRecords[data].ID
			};
			break;
		case "mercenaries_ability_unlock_notice":
		{
			NetCache.ProfileNoticeMercenariesAbilityUnlock obj = new NetCache.ProfileNoticeMercenariesAbilityUnlock
			{
				Origin = NetCache.ProfileNotice.NoticeOrigin.NOTICE_ORIGIN_MERCENARIES,
				MercenaryId = data
			};
			int abilityId2 = 1;
			if (!string.IsNullOrEmpty(data2))
			{
				int.TryParse(data2, out abilityId2);
			}
			obj.AbilityId = abilityId2;
			notice = obj;
			break;
		}
		case "mercenaries_ability_unlock":
		{
			NetCache.ProfileNoticeMercenariesAbilityUnlock rewardNotice = new NetCache.ProfileNoticeMercenariesAbilityUnlock();
			rewardNotice.Origin = NetCache.ProfileNotice.NoticeOrigin.NOTICE_ORIGIN_MERCENARIES;
			rewardNotice.MercenaryId = data;
			int abilityId = 1;
			if (!string.IsNullOrEmpty(data2))
			{
				int.TryParse(data2, out abilityId);
			}
			rewardNotice.AbilityId = abilityId;
			if (rewardNotice != null)
			{
				RewardUtils.LoadAndDisplayRewards(RewardUtils.GetRewards(new List<NetCache.ProfileNotice> { rewardNotice }));
			}
			break;
		}
		case "mercenaries_equipment_unlock":
		{
			List<RewardData> rewardsToShow = new List<RewardData>();
			int equipmentId = 1;
			if (!string.IsNullOrEmpty(data2))
			{
				int.TryParse(data2, out equipmentId);
			}
			LettuceEquipmentDbfRecord equipmentRecord = GameDbf.LettuceEquipment.GetRecord((LettuceEquipmentDbfRecord r) => r.ID == equipmentId);
			if (equipmentRecord != null && equipmentRecord.LettuceEquipmentTiers.Count > 0)
			{
				rewardsToShow.Add(new MercenariesEquipmentRewardData(data, equipmentId, equipmentRecord.LettuceEquipmentTiers[0].Tier));
				RewardUtils.LoadAndDisplayRewards(rewardsToShow);
			}
			break;
		}
		case "mercenaries_zone_unlock":
			notice = new NetCache.ProfileNoticeMercenariesZoneUnlock
			{
				ZoneId = data
			};
			break;
		case "mercenaries_booster":
			notice = new NetCache.ProfileNoticeMercenariesBoosterLicense
			{
				Count = data
			};
			break;
		case "mercenaries_coin":
			notice = new NetCache.ProfileNoticeMercenariesCurrencyLicense
			{
				MercenaryId = 1,
				CurrencyAmount = 100L
			};
			break;
		case "mercenaries_mercenary":
			notice = new NetCache.ProfileNoticeMercenariesMercenaryLicense
			{
				MercenaryId = 1,
				ArtVariationId = 0,
				ArtVariationPremium = 2u,
				CurrencyAmount = 100L
			};
			break;
		case "event":
		{
			questToast = true;
			EventRewardData eventReward = new EventRewardData();
			eventReward.EventType = data;
			rewards.Add(eventReward);
			break;
		}
		case "license":
		{
			questToast = false;
			NetCache.NetCacheAccountLicenses licenses = NetCache.Get().GetNetObject<NetCache.NetCacheAccountLicenses>();
			NetCache.ProfileNoticeAcccountLicense accountLicenseNotice = new NetCache.ProfileNoticeAcccountLicense();
			accountLicenseNotice.License = data;
			accountLicenseNotice.Origin = NetCache.ProfileNotice.NoticeOrigin.ACCOUNT_LICENSE_FLAGS;
			accountLicenseNotice.OriginData = 1L;
			if (licenses.AccountLicenses.ContainsKey(accountLicenseNotice.License))
			{
				accountLicenseNotice.CasID = licenses.AccountLicenses[accountLicenseNotice.License].CasId + 1;
			}
			notice = accountLicenseNotice;
			break;
		}
		case "renown":
			if (questToast)
			{
				rewards.Add(new MercenaryRenownRewardData(data));
			}
			else
			{
				notice = CreateCurrencyNotice(PegasusShared.CurrencyType.CURRENCY_TYPE_RENOWN);
			}
			break;
		default:
			UIStatus.Get().AddError($"{args[0]}: please specify a valid Notice Type.\nValid Types are: 'gold','arcane_orbs','dust','booster','card','cardback','tavern_brawl_rewards','event','license','renown'");
			return true;
		}
		if (questToast)
		{
			achieve.SetDescription("Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.", "");
			achieve.SetName("Title Text", "");
			QuestToast.ShowQuestToast(UserAttentionBlocker.ALL, null, updateCacheValues: false, achieve);
		}
		else if (notice != null)
		{
			NetCache.Get().Cheat_AddNotice(notice);
		}
		return true;
		NetCache.ProfileNoticeRewardCurrency CreateCurrencyNotice(PegasusShared.CurrencyType currency)
		{
			return new NetCache.ProfileNoticeRewardCurrency
			{
				CurrencyType = currency,
				Amount = data
			};
		}
	}

	private bool OnProcessCheat_LoadWidget(string func, string[] args, string rawArgs)
	{
		string guid = args[0];
		if (string.IsNullOrEmpty(guid))
		{
			UIStatus.Get().AddError("First parameter must be the GUID of a valid widget template.");
			return false;
		}
		WidgetInstance instance = WidgetInstance.Create(guid);
		if (instance == null)
		{
			UIStatus.Get().AddError("First parameter must be the GUID of a valid widget template.");
			return false;
		}
		s_createdWidgets.Add(instance);
		instance.TriggerEvent("CHEATED_STATE");
		return true;
	}

	private bool OnProcessCheat_ClearWidgets(string func, string[] args, string rawArgs)
	{
		foreach (WidgetInstance s_createdWidget in s_createdWidgets)
		{
			UnityEngine.Object.Destroy(s_createdWidget.gameObject);
		}
		s_createdWidgets.Clear();
		return true;
	}

	private bool OnProcessCheat_ServerLog(string func, string[] args, string rawArgs)
	{
		ScriptLogMessage message = new ScriptLogMessage();
		message.Message = rawArgs;
		message.Event = "Cheat";
		message.Severity = 1;
		SceneDebugger.Get().AddServerScriptLogMessage(message);
		return true;
	}

	private bool OnProcessCheat_dialogEvent(string func, string[] args, string rawArgs)
	{
		if (args.Length != 1)
		{
			UIStatus.Get().AddError("Provide 1 param for " + func + ".");
			return true;
		}
		NarrativeManager narrativeMgr = NarrativeManager.Get();
		if (narrativeMgr == null)
		{
			return false;
		}
		if (string.Equals(args[0], "reset", StringComparison.OrdinalIgnoreCase))
		{
			UIStatus.Get().AddInfo("All ScheduledCharacterDialogEvent's have been reset.");
			narrativeMgr.ResetScheduledCharacterDialogEvent_Debug();
			return true;
		}
		ScheduledCharacterDialogEvent selectedEventType = ScheduledCharacterDialogEvent.INVALID;
		Enum.TryParse<ScheduledCharacterDialogEvent>(args[0], ignoreCase: true, out selectedEventType);
		if (!Enum.IsDefined(typeof(ScheduledCharacterDialogEvent), selectedEventType) || selectedEventType == ScheduledCharacterDialogEvent.INVALID)
		{
			Array enumNames = Enum.GetNames(typeof(ScheduledCharacterDialogEvent));
			StringBuilder builder = new StringBuilder();
			builder.Append("reset -- this allows events to run again");
			builder.Append('\n');
			int index = 0;
			foreach (string name in enumNames)
			{
				if (index != 0)
				{
					builder.Append(index);
					builder.Append(" = ");
					builder.Append(name);
					builder.Append('\n');
				}
				index++;
			}
			string enumNameString = builder.ToString();
			UIStatus.Get().AddError("Invalid param for " + func + ". See \"Messages\".");
			Log.Gameplay.PrintError("Unrecognized <event_type>.\n" + $"Try a num [1-{enumNames.Length - 1}] or a string:\n" + enumNameString);
			return true;
		}
		narrativeMgr.TriggerScheduledCharacterDialogEvent_Debug(selectedEventType);
		return true;
	}

	private bool OnProcessCheat_account(string func, string[] args, string rawArgs, AutofillData autofillData)
	{
		string subCmdsCsv = "add, remove, set, skip, unlock";
		if (autofillData != null)
		{
			if ((rawArgs.EndsWith(" ") && args.Length == 0) || args.Length == 1)
			{
				string[] values = subCmdsCsv.Split(new char[2] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);
				string subCmd = ((args.Length == 0) ? string.Empty : args[0]);
				return ProcessAutofillParam(values, subCmd, autofillData);
			}
			return false;
		}
		string commandError = "account cheat requires one of the following valid sub-commands: " + subCmdsCsv;
		if (args.Length == 0)
		{
			UIStatus.Get().AddError(commandError);
			return true;
		}
		string subCommand = args[0].ToLower();
		string[] subArgs = args.Skip(1).ToArray();
		switch (subCommand)
		{
		case "add":
			HttpCheater.Get().RunAddResourceCommand(subArgs);
			break;
		case "remove":
			HttpCheater.Get().RunRemoveResourceCommand(subArgs);
			break;
		case "set":
			HttpCheater.Get().RunSetResourceCommand(subArgs);
			break;
		case "skip":
			HttpCheater.Get().RunSkipResourceCommand(subArgs);
			break;
		case "unlock":
			HttpCheater.Get().RunUnlockResourceCommand(subArgs);
			break;
		default:
			UIStatus.Get().AddError(commandError);
			break;
		}
		return true;
	}

	private bool OnProcessCheat_SkipSendingGetGameState(string func, string[] args, string rawArgs)
	{
		int arg = 0;
		if (args.Length != 0 && int.TryParse(args[0], out arg))
		{
			m_skipSendingGetGameState = arg != 0;
			return true;
		}
		return false;
	}

	private bool OnProcessCheat_SendGetGameState(string func, string[] args, string rawArgs)
	{
		if (m_skipSendingGetGameState)
		{
			Network.Get().GetGameState();
			return true;
		}
		return false;
	}

	private string GetChallengeUrl(string type)
	{
		string baseUrl = $"https://login-qa-us.web.blizzard.net/login/admin/challenge/create/ct_{type.ToLower()}";
		string email = "joe_balance@zmail.blizzard.com";
		string programId = "wtcg";
		string platformId = "*";
		string redirectUrl = "none";
		string messageKey = "";
		bool notifyRisk = false;
		bool chooseChallenge = false;
		string challengeTypes = "";
		string riskTransId = "";
		return $"{baseUrl}?email={email}&programId={programId}&platformId={platformId}&redirectUrl={redirectUrl}&messageKey={messageKey}&notifyRisk={notifyRisk}&chooseChallenge={chooseChallenge}&challengeType={challengeTypes}&riskTransId={riskTransId}";
	}

	private IEnumerator StorePasswordCoroutine(AssetReference assetRef, GameObject go, object callbackData)
	{
		m_loadingStoreChallengePrompt = false;
		m_storeChallengePrompt = go.GetComponent<StoreChallengePrompt>();
		m_storeChallengePrompt.Hide();
		Dictionary<string, string> headers = new Dictionary<string, string>();
		headers["Accept"] = "application/json;charset=UTF-8";
		headers["Accept-Language"] = Localization.GetBnetLocaleName();
		string createUrl = GetChallengeUrl("cvv");
		Debug.Log("creating challenge with url " + createUrl);
		IHttpRequest createChallenge = HttpRequestFactory.Get().CreateGetRequest(createUrl);
		createChallenge.SetRequestHeaders(headers);
		yield return createChallenge.SendRequest();
		Debug.Log("challenge response is " + createChallenge.ResponseAsString);
		string challengeUrl = (string)(Json.Deserialize(createChallenge.ResponseAsString) as JsonNode)["challenge_url"];
		Debug.Log("challenge url is " + challengeUrl);
		yield return m_storeChallengePrompt.StartCoroutine(m_storeChallengePrompt.Show(challengeUrl));
	}

	private bool OnProcessCheat_favoritecardback(string func, string[] args, string rawArgs)
	{
		if (args.Length == 0)
		{
			return false;
		}
		if (!int.TryParse(args[0].ToLowerInvariant(), out var cardBackID))
		{
			return false;
		}
		Network.Get().SetFavoriteCardBack(cardBackID);
		return true;
	}

	private bool OnProcessCheat_disconnect(string func, string[] args, string rawArgs)
	{
		string arg0Lower = ((args != null && args.Length >= 1) ? args[0].ToLower() : null);
		if (arg0Lower == "bnet")
		{
			if (Network.BattleNetStatus() != ConnectionState.Ready)
			{
				UIStatus.Get().AddError("Not connected to Battle.net, status=" + Network.BattleNetStatus());
				return true;
			}
			bool shouldStayDisconnected = default(bool);
			if (args.Length >= 2 && GeneralUtils.TryParseBool(args[1], out shouldStayDisconnected) && shouldStayDisconnected)
			{
				ReconnectMgr.Get().SetSuppressUtilReconnect(value: true);
			}
			BattleNet.RequestCloseAurora();
			UIStatus.Get().AddInfo("Disconnecting from Battle.net.");
			return true;
		}
		if (!Network.Get().IsConnectedToGameServer())
		{
			UIStatus.Get().AddError("Not connected to game server.");
			return true;
		}
		if (arg0Lower == "pong")
		{
			UIStatus.Get().AddInfo("Pong responses now being ignored.");
			Network.Get().SetShouldIgnorePong(value: true);
			return true;
		}
		bool num = arg0Lower == "internet";
		NetworkReachabilityManager networkReachabilityManager = ServiceManager.Get<NetworkReachabilityManager>();
		if (num)
		{
			networkReachabilityManager?.SetForceUnreachable(!networkReachabilityManager.GetForceUnreachable());
			UIStatus.Get().AddInfo(networkReachabilityManager.GetForceUnreachable() ? "Forcing unreachable network." : "Network reachable.");
			return true;
		}
		if (args != null && args.Length >= 2 && arg0Lower == "duration")
		{
			int duration = int.Parse(args[1]);
			networkReachabilityManager?.SetForceUnreachable(value: true);
			Network.Get().SetSpoofDisconnected(value: true);
			Network.Get().OverrideKeepAliveSeconds(5u);
			UIStatus.Get().AddInfo($"All network disconnected for {duration} seconds");
			HearthstoneApplication.Get().StartCoroutine(EnableNetworkAfterDelay(duration));
			return true;
		}
		bool num2 = args == null || args.Length == 0 || arg0Lower != "clean";
		Log.LoadingScreen.Print("Cheats.OnProcessCheat_disconnect() - reconnect=true");
		if (num2)
		{
			Network.Get().SimulateUncleanDisconnectFromGameServer();
		}
		else
		{
			Network.Get().QueueDispatcher.SetDebugGameConnectionState(canConnect: false, SocketError.ConnectionAborted);
		}
		return true;
	}

	private bool OnProcessCheat_reconnect(string func, string[] args, string rawArgs)
	{
		if (((args != null && args.Length >= 1) ? args[0].ToLower() : null) == "bnet")
		{
			if (Network.BattleNetStatus() == ConnectionState.Ready)
			{
				UIStatus.Get().AddError("Already connected to Battle.net");
				return true;
			}
			ReconnectMgr.Get().SetSuppressUtilReconnect(value: false);
			ReconnectMgr.Get().StartUtilReconnect();
			UIStatus.Get().AddInfo("Reconnecting to Battle.net.");
			return true;
		}
		return false;
	}

	private IEnumerator EnableNetworkAfterDelay(int delay)
	{
		yield return new WaitForSeconds(delay);
		ServiceManager.Get<NetworkReachabilityManager>()?.SetForceUnreachable(value: false);
		Network.Get().SetSpoofDisconnected(value: false);
		Network.Get().OverrideKeepAliveSeconds(0u);
	}

	private bool OnProcessCheat_restart(string func, string[] args, string rawArgs)
	{
		if (!Network.Get().IsConnectedToGameServer())
		{
			UIStatus.Get().AddError("Not connected to game server.");
			return true;
		}
		if (!GameUtils.CanRestartCurrentMission(checkTutorial: false))
		{
			UIStatus.Get().AddError("This game cannot be restarted.");
			return true;
		}
		GameState.Get().Restart();
		return true;
	}

	private bool OnProcessCheat_warning(string func, string[] args, string rawArgs)
	{
		ParseErrorText(args, rawArgs, out var header, out var message);
		Error.AddWarning(header, message);
		return true;
	}

	private bool OnProcessCheat_fatal(string func, string[] args, string rawArgs)
	{
		Error.AddFatal(FatalErrorReason.CHEAT, rawArgs);
		return true;
	}

	private bool OnProcessCheat_exit(string func, string[] args, string rawArgs)
	{
		GeneralUtils.ExitApplication();
		return true;
	}

	private bool OnProcessCheat_resetdkdecktutorials(string func, string[] args, string rawArgs)
	{
		List<GameSaveDataManager.SubkeySaveRequest> list = new List<GameSaveDataManager.SubkeySaveRequest>();
		list.Add(new GameSaveDataManager.SubkeySaveRequest(GameSaveKeyId.FTUE, GameSaveKeySubkeyId.HAS_SEEN_DK_DECK_BUILDING_INTRO_TUTORIAL, default(long)));
		list.Add(new GameSaveDataManager.SubkeySaveRequest(GameSaveKeyId.FTUE, GameSaveKeySubkeyId.HAS_SEEN_DK_DECK_BUILDING_TRIPLE_RUNES_POPUP, default(long)));
		list.Add(new GameSaveDataManager.SubkeySaveRequest(GameSaveKeyId.FTUE, GameSaveKeySubkeyId.HAS_SEEN_DK_DECK_BUILDING_RUNE_SLOT_AVAILABLE_POPUP, default(long)));
		list.Add(new GameSaveDataManager.SubkeySaveRequest(GameSaveKeyId.FTUE, GameSaveKeySubkeyId.HAS_SEEN_DK_DECK_BUILDING_CANNOT_ADD_RUNES_POPUP, default(long)));
		List<GameSaveDataManager.SubkeySaveRequest> saveRequests = list;
		if (GameSaveDataManager.Get().SaveSubkeys(saveRequests))
		{
			UIStatus.Get().AddInfo("Death Knight deck building game save data keys reset.");
			return true;
		}
		UIStatus.Get().AddInfo("Failed to reset Death Knight deck building game save data keys!");
		return false;
	}

	private bool OnProcessCheat_log(string func, string[] args, string rawArgs)
	{
		string msg = "unknown log command, please use 'log help'";
		float msgDuration = 5f;
		string command = args[0].ToLowerInvariant();
		string arg1 = ((args.Length < 2) ? string.Empty : args[1]?.ToLower());
		switch (command)
		{
		case "help":
			msg = "available log commands: load reload line";
			switch (arg1)
			{
			case "load":
			case "reload":
				msg = "reloads the log.config";
				break;
			case "line":
				msg = "prints a simple long line to log, useful for debugging\nto visually differentiate between test results.\nyou can specify a parameter like\n'log warn' to call Debug.LogWarning. you can\nalso add a note/context to your line\nby adding words afterwards, like 'log test 2 start'\nor 'log error (test 3 starting)'.";
				msgDuration = 10f;
				break;
			}
			break;
		case "load":
		case "reload":
			LogSystem.Get().ReloadLogConfig();
			break;
		case "line":
		{
			LogFormatFunc logFunc = Debug.LogFormat;
			string note = string.Empty;
			int noteIndex = 1;
			switch (arg1)
			{
			case "warn":
			case "warning":
				logFunc = Debug.LogWarningFormat;
				noteIndex++;
				break;
			case "err":
			case "error":
				logFunc = Debug.LogErrorFormat;
				noteIndex++;
				break;
			}
			note = string.Join(" ", args.Skip(noteIndex).ToArray());
			if (note.Length > 0)
			{
				note = $" {note} ";
			}
			logFunc("====={0}{1}", note, new string('=', Mathf.Max(5, 75 - note.Length)));
			msg = "printed line to " + logFunc.Method.Name;
			msgDuration = 2f;
			break;
		}
		}
		UIStatus.Get().AddInfo(msg, msgDuration);
		return true;
	}

	private bool OnProcessCheat_alert(string func, string[] args, string rawArgs)
	{
		AlertPopup.PopupInfo info = GenerateAlertInfo(rawArgs);
		if (m_alert == null)
		{
			DialogManager.Get().ShowPopup(info, OnAlertProcessed);
		}
		else
		{
			m_alert.UpdateInfo(info);
		}
		return true;
	}

	private bool OnProcessCheat_rankedIntroPopup(string func, string[] args, string rawArgs)
	{
		DialogManager.Get().ShowRankedIntroPopUp(null);
		MedalInfoTranslator mit = RankMgr.Get().GetLocalPlayerMedalInfo();
		DialogManager.Get().ShowBonusStarsPopup(mit.CreateDataModel(FormatType.FT_STANDARD, RankedMedal.DisplayMode.Default), null);
		return true;
	}

	private bool OnProcessCheat_setRotationRotatedBoostersPopup(string func, string[] args, string rawArgs)
	{
		SetRotationRotatedBoostersPopup.SetRotationRotatedBoostersPopupInfo info = new SetRotationRotatedBoostersPopup.SetRotationRotatedBoostersPopupInfo();
		DialogManager.Get().ShowSetRotationTutorialPopup(UserAttentionBlocker.SET_ROTATION_INTRO, info);
		return true;
	}

	private bool OnProcessCheat_seasondialog(string func, string[] args, string rawArgs)
	{
		string rankCheatName = "bronze10";
		if (args.Length != 0 && !string.IsNullOrEmpty(args[0]))
		{
			rankCheatName = args[0];
		}
		LeagueRankDbfRecord rankRecord = RankMgr.Get().GetLeagueRankRecordByCheatName(rankCheatName);
		if (rankRecord == null)
		{
			return false;
		}
		FormatType formatType = FormatType.FT_STANDARD;
		if (args.Length >= 2)
		{
			switch (args[1].ToLower())
			{
			case "1":
			case "wild":
				formatType = FormatType.FT_WILD;
				break;
			case "2":
			case "standard":
				formatType = FormatType.FT_STANDARD;
				break;
			case "3":
			case "classic":
				formatType = FormatType.FT_CLASSIC;
				break;
			default:
				UIStatus.Get().AddInfo("please enter a valid value for 2nd parameter <format type>");
				return true;
			}
		}
		SeasonEndDialog.SeasonEndInfo seasonEndInfo = new SeasonEndDialog.SeasonEndInfo();
		seasonEndInfo.m_leagueId = rankRecord.LeagueId;
		seasonEndInfo.m_starLevelAtEndOfSeason = rankRecord.StarLevel;
		seasonEndInfo.m_bestStarLevelAtEndOfSeason = rankRecord.StarLevel;
		seasonEndInfo.m_formatType = formatType;
		MedalInfoTranslator medalInfoTranslator = MedalInfoTranslator.CreateMedalInfoForLeagueId(rankRecord.LeagueId, rankRecord.StarLevel, 0);
		medalInfoTranslator.GetPreviousMedal(formatType).starLevel = 1;
		medalInfoTranslator.GetCurrentMedal(formatType).bestStarLevel = rankRecord.StarLevel;
		seasonEndInfo.m_rankedRewards = new List<RewardData>();
		List<List<RewardData>> rewardDataLists = new List<List<RewardData>>();
		if (!medalInfoTranslator.GetRankedRewardsEarned(formatType, ref rewardDataLists))
		{
			return false;
		}
		foreach (List<RewardData> rewardDataList in rewardDataLists)
		{
			seasonEndInfo.m_rankedRewards.AddRange(rewardDataList);
		}
		for (int i = 0; i < seasonEndInfo.m_rankedRewards.Count; i++)
		{
			if (seasonEndInfo.m_rankedRewards[i] is RandomCardRewardData randomCardRewardData)
			{
				string cardIdForRarity = "GAME_005";
				switch (randomCardRewardData.Rarity)
				{
				case TAG_RARITY.COMMON:
					cardIdForRarity = "EX1_096";
					break;
				case TAG_RARITY.RARE:
					cardIdForRarity = "EX1_274";
					break;
				case TAG_RARITY.EPIC:
					cardIdForRarity = "EX1_586";
					break;
				case TAG_RARITY.LEGENDARY:
					cardIdForRarity = "EX1_562";
					break;
				}
				seasonEndInfo.m_rankedRewards[i] = new CardRewardData(cardIdForRarity, randomCardRewardData.Premium, 1);
			}
		}
		NetCache.NetCacheRewardProgress rewardProgress = NetCache.Get().GetNetObject<NetCache.NetCacheRewardProgress>();
		if (rewardProgress != null)
		{
			seasonEndInfo.m_seasonID = rewardProgress.Season;
		}
		DialogManager.DialogRequest dialogRequest = new DialogManager.DialogRequest();
		dialogRequest.m_type = DialogManager.DialogType.SEASON_END;
		dialogRequest.m_info = seasonEndInfo;
		dialogRequest.m_isFake = true;
		DialogManager.Get().AddToQueue(dialogRequest);
		return true;
	}

	private bool OnProcessCheat_playnullsound(string func, string[] args, string rawArgs)
	{
		SoundManager.Get().Play(null);
		return true;
	}

	private bool OnProcessCheat_playaudio(string func, string[] args, string rawArgs)
	{
		if (PlayAudioByName != null)
		{
			PlayAudioByName(args);
		}
		return true;
	}

	private bool OnProcessCheat_spectate(string func, string[] args, string rawArgs)
	{
		if (args.Length >= 1 && string.Equals(args[0], "waiting", StringComparison.OrdinalIgnoreCase))
		{
			SpectatorManager.Get().ShowWaitingForNextGameDialog();
			return true;
		}
		if (args.Length < 4 || args.Any((string a) => string.IsNullOrEmpty(a)))
		{
			Error.AddWarning("Spectate Cheat Error", "spectate cheat must have the following args:\n\nspectate ipaddress port game_handle spectator_password [gameType] [missionId]");
			return false;
		}
		JoinInfo builder = new JoinInfo();
		builder.ServerIpAddress = args[0];
		builder.SecretKey = args[3];
		if (!uint.TryParse(args[1], out var uintVal))
		{
			Error.AddWarning("Spectate Cheat Error", "error parsing the port # (uint) argument: " + args[1]);
			return false;
		}
		builder.ServerPort = uintVal;
		if (!int.TryParse(args[2], out var intVal))
		{
			Error.AddWarning("Spectate Cheat Error", "error parsing the game_handle (int) argument: " + args[2]);
			return false;
		}
		builder.GameHandle = intVal;
		builder.GameType = GameType.GT_UNKNOWN;
		builder.MissionId = 2;
		if (args.Length >= 5 && int.TryParse(args[4], out intVal))
		{
			builder.GameType = (GameType)intVal;
		}
		if (args.Length >= 6 && int.TryParse(args[5], out intVal))
		{
			builder.MissionId = intVal;
		}
		GameMgr.Get().SpectateGame(builder);
		return true;
	}

	private static void SubscribePartyEvents()
	{
		if (s_hasSubscribedToPartyEvents)
		{
			return;
		}
		BnetParty.OnError += delegate(PartyError error)
		{
			Log.Party.Print("{0} code={1} feature={2} party={3} str={4}", error.DebugContext, error.ErrorCode, error.FeatureEvent.ToString(), new PartyInfo(error.PartyId, error.PartyType), error.StringData);
		};
		BnetParty.OnJoined += delegate(OnlineEventType e, PartyInfo party, LeaveReason? reason)
		{
			Log.Party.Print("Party.OnJoined {0} party={1} reason={2}", e, party, reason.HasValue ? reason.Value.ToString() : "null");
		};
		BnetParty.OnPrivacyLevelChanged += delegate(PartyInfo party, ChannelApi.PartyPrivacyLevel privacy)
		{
			Log.Party.Print("Party.OnPrivacyLevelChanged party={0} privacy={1}", party, privacy);
		};
		BnetParty.OnMemberEvent += delegate(OnlineEventType e, PartyInfo party, BnetGameAccountId memberId, bool isRolesUpdate, LeaveReason? reason)
		{
			Log.Party.Print("Party.OnMemberEvent {0} party={1} memberId={2} isRolesUpdate={3} reason={4}", e, party, memberId, isRolesUpdate, reason.HasValue ? reason.Value.ToString() : "null");
		};
		BnetParty.OnReceivedInvite += delegate(OnlineEventType e, PartyInfo party, ulong inviteId, BnetGameAccountId inviter, string inviterBattletag, BnetGameAccountId invitee, InviteRemoveReason? reason)
		{
			Log.Party.Print("Party.OnReceivedInvite {0} party={1} inviteId={2} reason={3}", e, party, inviteId, reason.HasValue ? reason.Value.ToString() : "null");
		};
		BnetParty.OnSentInvite += delegate(OnlineEventType e, PartyInfo party, ulong inviteId, BnetGameAccountId inviter, BnetGameAccountId invitee, bool senderIsMyself, InviteRemoveReason? reason)
		{
			PartyInvite sentInvite = BnetParty.GetSentInvite(party.Id, inviteId);
			Log.Party.Print("Party.OnSentInvite {0} party={1} inviteId={2} senderIsMyself={3} isRejoin={4} reason={5}", e, party, inviteId, senderIsMyself, (sentInvite == null) ? "null" : sentInvite.IsRejoin.ToString(), reason.HasValue ? reason.Value.ToString() : "null");
		};
		BnetParty.OnReceivedInviteRequest += delegate(OnlineEventType e, PartyInfo party, InviteRequest request, InviteRequestRemovedReason? reason)
		{
			Log.Party.Print("Party.OnReceivedInviteRequest {0} party={1} target={2} {3} requester={4} {5} reason={6}", e, party, request.TargetName, request.TargetId, request.RequesterName, request.RequesterId, reason.HasValue ? reason.Value.ToString() : "null");
		};
		BnetParty.OnChatMessage += delegate(PartyInfo party, BnetGameAccountId speakerId, string msg)
		{
			Log.Party.Print("Party.OnChatMessage party={0} speakerId={1} msg={2}", party, speakerId, msg);
		};
		BnetParty.OnPartyAttributeChanged += delegate(PartyInfo party, Blizzard.GameService.Protocol.V2.Client.Attribute attr)
		{
			string text = "null";
			if (attr.Value.HasIntValue)
			{
				text = "[long]" + attr.Value.IntValue;
			}
			else if (attr.Value.HasStringValue)
			{
				text = "[string]" + attr.Value.StringValue;
			}
			else if (attr.Value.HasBlobValue)
			{
				byte[] array = attr.Value.BlobValue.ToByteArray();
				if (array != null)
				{
					text = "blobLength=" + array.Length;
					try
					{
						string @string = Encoding.UTF8.GetString(array);
						if (@string != null)
						{
							text = text + " decodedUtf8=" + @string;
						}
					}
					catch (ArgumentException)
					{
					}
				}
			}
			Log.Party.Print("BnetParty.OnPartyAttributeChanged party={0} key={1} value={2}", party, attr.Name, text);
		};
		BnetParty.OnMemberAttributeChanged += delegate(PartyInfo party, BnetGameAccountId partyMember, Blizzard.GameService.Protocol.V2.Client.Attribute attr)
		{
			string text2 = "null";
			if (attr.Value.HasIntValue)
			{
				text2 = "[long]" + attr.Value.IntValue;
			}
			else if (attr.Value.HasStringValue)
			{
				text2 = "[string]" + attr.Value.StringValue;
			}
			else if (attr.Value.HasBlobValue)
			{
				byte[] array2 = attr.Value.BlobValue.ToByteArray();
				if (array2 != null)
				{
					text2 = "blobLength=" + array2.Length;
					try
					{
						string string2 = Encoding.UTF8.GetString(array2);
						if (string2 != null)
						{
							text2 = text2 + " decodedUtf8=" + string2;
						}
					}
					catch (ArgumentException)
					{
					}
				}
			}
			Log.Party.Print("BnetParty.OnMemberAttributeChanged party={0} member={1} key={2} value={3}", party, partyMember, attr.Name, text2);
		};
		s_hasSubscribedToPartyEvents = true;
	}

	private static BnetPartyId ParsePartyId(string cmd, string arg, int argIndex, ref string errorMsg)
	{
		BnetPartyId partyId = null;
		PartyType type;
		if (ulong.TryParse(arg, out var low))
		{
			BnetPartyId[] partyIds = BnetParty.GetJoinedPartyIds();
			partyId = ((low < 0 || partyIds.Length == 0 || low >= (ulong)partyIds.LongLength) ? partyIds.FirstOrDefault((BnetPartyId p) => p.ChannelId.Id == low) : partyIds[low]);
			if (partyId == null)
			{
				errorMsg = "party " + cmd + ": couldn't find party at index, or with PartyId low bits: " + low;
			}
		}
		else if (!EnumUtils.TryGetEnum<PartyType>(arg, out type))
		{
			errorMsg = "party " + cmd + ": unable to parse party (index or LowBits or type)" + ((argIndex >= 0) ? (" at arg index=" + argIndex) : "") + " (" + arg + "), please specify the Low bits of a PartyId or a PartyType.";
		}
		else
		{
			partyId = (from info in BnetParty.GetJoinedParties()
				where info.Type == type
				select info.Id).FirstOrDefault();
			if (partyId == null)
			{
				errorMsg = "party " + cmd + ": no joined party with PartyType: " + arg;
			}
		}
		return partyId;
	}

	private bool OnProcessCheat_party(string func, string[] args, string rawArgs)
	{
		if (args.Length < 1 || args.Any((string a) => string.IsNullOrEmpty(a)))
		{
			string usage = "USAGE: party [cmd] [args]\nCommands: create | join | leave | dissolve | list | invite | accept | decline | revoke | requestinvite | ignorerequest | setleader | kick | chat | setprivacy | setlong | setstring | setblob | clearattr | subscribe | unsubscribe";
			Error.AddWarning("Party Cheat Error", usage);
			return false;
		}
		string cmd = args[0]?.ToLower();
		if (cmd == "unsubscribe")
		{
			BnetParty.RemoveFromAllEventHandlers(this);
			s_hasSubscribedToPartyEvents = false;
			Log.Party.Print("party {0}: unsubscribed.", cmd);
			return true;
		}
		bool success = true;
		string[] cmdArgs = args.Skip(1).ToArray();
		string errorMsg = null;
		SubscribePartyEvents();
		switch (cmd)
		{
		case "create":
		{
			if (cmdArgs.Length < 1)
			{
				errorMsg = "party create: requires a PartyType: " + string.Join(" | ", Enum.GetValues(typeof(PartyType)).Cast<PartyType>().Select(delegate(PartyType v)
				{
					string text2 = v.ToString();
					int num8 = (int)v;
					return text2 + " (" + num8 + ")";
				})
					.ToArray());
				break;
			}
			PartyType type;
			if (int.TryParse(cmdArgs[0], out var intVal2))
			{
				type = (PartyType)intVal2;
			}
			else if (!EnumUtils.TryGetEnum<PartyType>(cmdArgs[0], out type))
			{
				errorMsg = "party create: unknown PartyType specified: " + cmdArgs[0];
			}
			if (errorMsg == null)
			{
				byte[] creatorBlob = ProtobufUtil.ToByteArray(BnetUtils.CreatePegasusBnetId(BnetPresenceMgr.Get().GetMyGameAccountId()));
				BnetParty.CreateParty(type, ChannelApi.PartyPrivacyLevel.OpenInvitation, creatorBlob, delegate(PartyType t, BnetPartyId partyId)
				{
					Log.Party.Print("BnetParty.CreateSuccessCallback type={0} partyId={1}", t, partyId);
				});
			}
			break;
		}
		case "leave":
		case "dissolve":
		{
			bool isDissolve = cmd == "dissolve";
			if (cmdArgs.Length == 0)
			{
				Log.Party.Print("NOTE: party {0} without any arguments will {0} all joined parties.", cmd);
				PartyInfo[] joinedParties4 = BnetParty.GetJoinedParties();
				if (joinedParties4.Length == 0)
				{
					Log.Party.Print("No joined parties.");
				}
				PartyInfo[] array3 = joinedParties4;
				foreach (PartyInfo info3 in array3)
				{
					Log.Party.Print("party {0}: {1} party {2}", cmd, isDissolve ? "dissolving" : "leaving", info3);
					if (isDissolve)
					{
						BnetParty.DissolveParty(info3.Id);
					}
					else
					{
						BnetParty.Leave(info3.Id);
					}
				}
				break;
			}
			for (int num7 = 0; num7 < cmdArgs.Length; num7++)
			{
				string arg4 = cmdArgs[num7];
				string logMsg = null;
				BnetPartyId partyId11 = ParsePartyId(cmd, arg4, num7, ref logMsg);
				if (logMsg != null)
				{
					Log.Party.Print(logMsg);
				}
				if (partyId11 != null)
				{
					Log.Party.Print("party {0}: {1} party {2}", cmd, isDissolve ? "dissolving" : "leaving", BnetParty.GetJoinedParty(partyId11));
					if (isDissolve)
					{
						BnetParty.DissolveParty(partyId11);
					}
					else
					{
						BnetParty.Leave(partyId11);
					}
				}
			}
			break;
		}
		case "join":
		{
			if (cmdArgs.Length < 1)
			{
				errorMsg = "party " + cmd + ": must specify an online friend index or a partyId (Hi-Lo format)";
				break;
			}
			PartyType partyType = PartyType.DEFAULT;
			string[] array2 = cmdArgs;
			foreach (string arg2 in array2)
			{
				int hyphenIndex = arg2.IndexOf('-');
				int friendIndex2 = -1;
				BnetPartyId partyIdToJoin = null;
				if (hyphenIndex >= 0)
				{
					string s = arg2.Substring(0, hyphenIndex);
					string loPart = ((arg2.Length > hyphenIndex) ? arg2.Substring(hyphenIndex + 1) : "");
					if (ulong.TryParse(s, out var hiBits) && ulong.TryParse(loPart, out var loBits))
					{
						partyIdToJoin = new BnetPartyId(hiBits, loBits);
					}
					else
					{
						errorMsg = "party " + cmd + ": unable to parse partyId (in format Hi-Lo).";
					}
				}
				else if (int.TryParse(arg2, out friendIndex2))
				{
					BnetPlayer[] friends2 = (from p in BnetFriendMgr.Get().GetFriends()
						where p.IsOnline() && p.GetHearthstoneGameAccount() != null
						select p).ToArray();
					errorMsg = ((friendIndex2 >= 0 && friendIndex2 < friends2.Length) ? ("party " + cmd + ": Not-Yet-Implemented: find partyId from online friend's presence.") : ("party " + cmd + ": no online friend at index " + friendIndex2));
				}
				else
				{
					errorMsg = "party " + cmd + ": unable to parse online friend index.";
				}
				if (partyIdToJoin != null)
				{
					BnetParty.JoinParty(partyIdToJoin, partyType);
				}
			}
			break;
		}
		case "chat":
		{
			BnetPartyId[] joinedParties3 = BnetParty.GetJoinedPartyIds();
			if (cmdArgs.Length < 1)
			{
				errorMsg = "party chat: must specify 1-2 arguments: party (index or LowBits or type) or a message to send.";
				break;
			}
			int argSkipAmount2 = 1;
			BnetPartyId partyId6 = ParsePartyId(cmd, cmdArgs[0], -1, ref errorMsg);
			if (partyId6 == null && joinedParties3.Length != 0)
			{
				errorMsg = null;
				partyId6 = joinedParties3[0];
				argSkipAmount2 = 0;
			}
			if (partyId6 != null)
			{
				BnetParty.SendChatMessage(partyId6, string.Join(" ", cmdArgs.Skip(argSkipAmount2).ToArray()));
			}
			break;
		}
		case "invite":
		{
			BnetPartyId partyId4 = null;
			int argSkipAmount = 1;
			if (cmdArgs.Length == 0)
			{
				BnetPartyId[] joinedParties2 = BnetParty.GetJoinedPartyIds();
				if (joinedParties2.Length != 0)
				{
					partyId4 = joinedParties2[0];
					argSkipAmount = 0;
				}
				else
				{
					errorMsg = "party invite: no joined parties to invite to.";
				}
			}
			else
			{
				partyId4 = ParsePartyId(cmd, cmdArgs[0], -1, ref errorMsg);
			}
			if (!(partyId4 != null))
			{
				break;
			}
			string[] targetArgs = cmdArgs.Skip(argSkipAmount).ToArray();
			HashSet<BnetPlayer> targets = new HashSet<BnetPlayer>();
			IEnumerable<BnetPlayer> friends = from p in BnetFriendMgr.Get().GetFriends()
				where p.IsOnline() && p.GetHearthstoneGameAccount() != null
				select p;
			if (targetArgs.Length == 0)
			{
				Log.Party.Print("NOTE: party invite without any arguments will pick the first online friend.");
				BnetPlayer target = null;
				target = friends.FirstOrDefault();
				if (target == null)
				{
					errorMsg = "party invite: no online Hearthstone friend found.";
				}
				else
				{
					targets.Add(target);
				}
			}
			else
			{
				for (int num2 = 0; num2 < targetArgs.Length; num2++)
				{
					string arg = targetArgs[num2];
					if (int.TryParse(arg, out var friendIndex))
					{
						BnetPlayer f = friends.ElementAtOrDefault(friendIndex);
						if (f == null)
						{
							errorMsg = "party invite: no online Hearthstone friend index " + friendIndex;
						}
						else
						{
							targets.Add(f);
						}
						continue;
					}
					IEnumerable<BnetPlayer> matches = friends.Where((BnetPlayer p) => p.GetBattleTag().ToString().Contains(arg, StringComparison.OrdinalIgnoreCase) || (p.GetFullName() != null && p.GetFullName().Contains(arg, StringComparison.OrdinalIgnoreCase)));
					if (!matches.Any())
					{
						errorMsg = "party invite: no online Hearthstone friend matching name " + arg + " (arg index " + num2 + ")";
						continue;
					}
					foreach (BnetPlayer f2 in matches)
					{
						if (!targets.Contains(f2))
						{
							targets.Add(f2);
							break;
						}
					}
				}
			}
			foreach (BnetPlayer target2 in targets)
			{
				BnetGameAccountId targetId = target2.GetHearthstoneGameAccountId();
				if (BnetParty.IsMember(partyId4, targetId))
				{
					Log.Party.Print("party invite: already a party member of {0}: {1}", target2, BnetParty.GetJoinedParty(partyId4));
				}
				else
				{
					Log.Party.Print("party invite: inviting {0} {1} to party {2}", targetId, target2, BnetParty.GetJoinedParty(partyId4));
					BnetParty.SendInvite(partyId4, targetId, isReservation: true);
				}
			}
			break;
		}
		case "accept":
		case "decline":
		{
			bool isAccept = cmd == "accept";
			PartyInvite[] invites2 = BnetParty.GetReceivedInvites();
			if (invites2.Length == 0)
			{
				errorMsg = "party " + cmd + ": no received party invites.";
				break;
			}
			if (cmdArgs.Length == 0)
			{
				Log.Party.Print("NOTE: party {0} without any arguments will {0} all received invites.", cmd);
				PartyInvite[] sentInvites2 = invites2;
				foreach (PartyInvite invite7 in sentInvites2)
				{
					Log.Party.Print("party {0}: {1} inviteId={2} from {3} for party {4}.", cmd, isAccept ? "accepting" : "declining", invite7.InviteId, invite7.InviterName, new PartyInfo(invite7.PartyId, invite7.PartyType));
					if (isAccept)
					{
						BnetParty.AcceptReceivedInvite(invite7.InviteId);
					}
					else
					{
						BnetParty.DeclineReceivedInvite(invite7.InviteId);
					}
				}
				break;
			}
			for (int num10 = 0; num10 < cmdArgs.Length; num10++)
			{
				if (ulong.TryParse(cmdArgs[num10], out var indexOrId3))
				{
					PartyInvite invite8 = null;
					if (indexOrId3 < (ulong)invites2.LongLength)
					{
						invite8 = invites2[indexOrId3];
					}
					else
					{
						invite8 = invites2.FirstOrDefault((PartyInvite inv) => inv.InviteId == indexOrId3);
						if (invite8 == null)
						{
							Log.Party.Print("party {0}: unable to find received invite (id or index): {1}", cmd, cmdArgs[num10]);
						}
					}
					if (invite8 != null)
					{
						Log.Party.Print("party {0}: {1} inviteId={2} from {3} for party {4}.", cmd, isAccept ? "accepting" : "declining", invite8.InviteId, invite8.InviterName, new PartyInfo(invite8.PartyId, invite8.PartyType));
						if (isAccept)
						{
							BnetParty.AcceptReceivedInvite(invite8.InviteId);
						}
						else
						{
							BnetParty.DeclineReceivedInvite(invite8.InviteId);
						}
					}
				}
				else
				{
					Log.Party.Print("party {0}: unable to parse invite (id or index): {1}", cmd, cmdArgs[num10]);
				}
			}
			break;
		}
		case "revoke":
		{
			BnetPartyId partyId9 = null;
			if (cmdArgs.Length == 0)
			{
				Log.Party.Print("NOTE: party {0} without any arguments will {0} all sent invites for all parties.", cmd);
				BnetPartyId[] partyIds4 = BnetParty.GetJoinedPartyIds();
				if (partyIds4.Length == 0)
				{
					Log.Party.Print("party {0}: no joined parties.", cmd);
				}
				BnetPartyId[] array = partyIds4;
				foreach (BnetPartyId pId2 in array)
				{
					PartyInvite[] sentInvites2 = BnetParty.GetSentInvites(pId2);
					foreach (PartyInvite invite4 in sentInvites2)
					{
						Log.Party.Print("party {0}: revoking inviteId={1} from {2} for party {3}.", cmd, invite4.InviteId, invite4.InviterName, BnetParty.GetJoinedParty(pId2));
						BnetParty.RevokeSentInvite(pId2, invite4.InviteId);
					}
				}
			}
			else
			{
				partyId9 = ParsePartyId(cmd, cmdArgs[0], -1, ref errorMsg);
			}
			if (!(partyId9 != null))
			{
				break;
			}
			PartyInfo info2 = BnetParty.GetJoinedParty(partyId9);
			PartyInvite[] invites = BnetParty.GetSentInvites(partyId9);
			if (invites.Length == 0)
			{
				errorMsg = "party " + cmd + ": no sent invites for party " + info2;
				break;
			}
			string[] revokeArgs = cmdArgs.Skip(1).ToArray();
			if (revokeArgs.Length == 0)
			{
				Log.Party.Print("NOTE: party {0} without specifying InviteId (or index) will {0} all sent invites.", cmd);
				PartyInvite[] sentInvites2 = invites;
				foreach (PartyInvite invite5 in sentInvites2)
				{
					Log.Party.Print("party {0}: revoking inviteId={1} from {2} for party {3}.", cmd, invite5.InviteId, invite5.InviterName, info2);
					BnetParty.RevokeSentInvite(partyId9, invite5.InviteId);
				}
				break;
			}
			for (int num5 = 0; num5 < revokeArgs.Length; num5++)
			{
				if (ulong.TryParse(revokeArgs[num5], out var indexOrId))
				{
					PartyInvite invite6 = null;
					if (indexOrId < (ulong)invites.LongLength)
					{
						invite6 = invites[indexOrId];
					}
					else
					{
						invite6 = invites.FirstOrDefault((PartyInvite inv) => inv.InviteId == indexOrId);
						if (invite6 == null)
						{
							Log.Party.Print("party {0}: unable to find sent invite (id or index): {1} for party {2}", cmd, revokeArgs[num5], info2);
						}
					}
					if (invite6 != null)
					{
						Log.Party.Print("party {0}: revoking inviteId={1} from {2} for party {3}.", cmd, invite6.InviteId, invite6.InviterName, info2);
						BnetParty.RevokeSentInvite(partyId9, invite6.InviteId);
					}
				}
				else
				{
					Log.Party.Print("party {0}: unable to parse invite (id or index): {1}", cmd, revokeArgs[num5]);
				}
			}
			break;
		}
		case "requestinvite":
		{
			if (cmdArgs.Length < 2)
			{
				errorMsg = "party " + cmd + ": must specify a partyId (Hi-Lo format) and an online friend index";
				break;
			}
			PartyType partyType2 = PartyType.DEFAULT;
			string[] array2 = cmdArgs;
			foreach (string arg3 in array2)
			{
				int hyphenIndex2 = arg3.IndexOf('-');
				int friendIndex3 = -1;
				BnetPartyId partyId7 = null;
				BnetGameAccountId whomToAskForApproval = null;
				if (hyphenIndex2 >= 0)
				{
					string s2 = arg3.Substring(0, hyphenIndex2);
					string loPart2 = ((arg3.Length > hyphenIndex2) ? arg3.Substring(hyphenIndex2 + 1) : "");
					if (ulong.TryParse(s2, out var hiBits2) && ulong.TryParse(loPart2, out var loBits2))
					{
						partyId7 = new BnetPartyId(hiBits2, loBits2);
					}
					else
					{
						errorMsg = "party " + cmd + ": unable to parse partyId (in format Hi-Lo).";
					}
				}
				else if (int.TryParse(arg3, out friendIndex3))
				{
					BnetPlayer[] friends3 = (from p in BnetFriendMgr.Get().GetFriends()
						where p.IsOnline() && p.GetHearthstoneGameAccount() != null
						select p).ToArray();
					if (friendIndex3 < 0 || friendIndex3 >= friends3.Length)
					{
						errorMsg = "party " + cmd + ": no online friend at index " + friendIndex3;
					}
					else
					{
						whomToAskForApproval = friends3[friendIndex3].GetHearthstoneGameAccountId();
					}
				}
				else
				{
					errorMsg = "party " + cmd + ": unable to parse online friend index.";
				}
				if (partyId7 != null && whomToAskForApproval != null)
				{
					BnetParty.RequestInvite(partyId7, whomToAskForApproval, BnetPresenceMgr.Get().GetMyGameAccountId(), partyType2);
				}
			}
			break;
		}
		case "ignorerequest":
		{
			BnetPartyId[] partyIds2 = BnetParty.GetJoinedPartyIds();
			if (partyIds2.Length == 0)
			{
				Log.Party.Print("party {0}: no joined parties.", cmd);
				break;
			}
			BnetPartyId[] array = partyIds2;
			foreach (BnetPartyId partyId5 in array)
			{
				InviteRequest[] inviteRequests = BnetParty.GetInviteRequests(partyId5);
				foreach (InviteRequest invite3 in inviteRequests)
				{
					Log.Party.Print("party {0}: ignoring request to invite {0} {1} from {2} {3}.", invite3.TargetName, invite3.TargetId, invite3.RequesterName, invite3.RequesterId);
					BnetParty.IgnoreInviteRequest(partyId5, invite3.TargetId);
				}
			}
			break;
		}
		case "setleader":
		{
			IEnumerable<BnetPartyId> partyIds3 = null;
			int memberIndex = -1;
			if (cmdArgs.Length >= 2 && (!int.TryParse(cmdArgs[1], out memberIndex) || memberIndex < 0))
			{
				errorMsg = $"party {cmd}: invalid memberIndex={cmdArgs[1]}";
			}
			if (cmdArgs.Length == 0)
			{
				Log.Party.Print("NOTE: party {0} without any arguments will {0} to first member in all parties.", cmd);
				BnetPartyId[] joinedPartyIds = BnetParty.GetJoinedPartyIds();
				if (joinedPartyIds.Length == 0)
				{
					Log.Party.Print("party {0}: no joined parties.", cmd);
				}
				else
				{
					partyIds3 = joinedPartyIds;
				}
			}
			else
			{
				BnetPartyId partyId8 = ParsePartyId(cmd, cmdArgs[0], -1, ref errorMsg);
				if (partyId8 != null)
				{
					partyIds3 = new BnetPartyId[1] { partyId8 };
				}
			}
			if (partyIds3 == null)
			{
				break;
			}
			foreach (BnetPartyId pId in partyIds3)
			{
				BnetParty.PartyMember[] members2 = BnetParty.GetMembers(pId);
				if (memberIndex >= 0)
				{
					if (memberIndex >= members2.Length)
					{
						Log.Party.Print("party {0}: party={1} has no member at index={2}", cmd, BnetParty.GetJoinedParty(pId), memberIndex);
					}
					else
					{
						BnetParty.PartyMember member = members2[memberIndex];
						BnetParty.SetLeader(pId, member.GameAccountId);
					}
				}
				else if (members2.Any((BnetParty.PartyMember m) => m.GameAccountId != BnetPresenceMgr.Get().GetMyGameAccountId()))
				{
					BnetParty.SetLeader(pId, members2.First((BnetParty.PartyMember m) => m.GameAccountId != BnetPresenceMgr.Get().GetMyGameAccountId()).GameAccountId);
				}
				else
				{
					Log.Party.Print("party {0}: party={1} has no member not myself to set as leader.", cmd, BnetParty.GetJoinedParty(pId));
				}
			}
			break;
		}
		case "kick":
		{
			BnetPartyId partyId13 = null;
			if (cmdArgs.Length == 0)
			{
				Log.Party.Print("NOTE: party {0} without any arguments will {0} all members for all parties (other than self).", cmd);
				BnetPartyId[] partyIds5 = BnetParty.GetJoinedPartyIds();
				if (partyIds5.Length == 0)
				{
					Log.Party.Print("party {0}: no joined parties.", cmd);
				}
				BnetPartyId[] array = partyIds5;
				foreach (BnetPartyId pId3 in array)
				{
					BnetParty.PartyMember[] members3 = BnetParty.GetMembers(pId3);
					foreach (BnetParty.PartyMember member2 in members3)
					{
						if (!(member2.GameAccountId == BnetPresenceMgr.Get().GetMyGameAccountId()))
						{
							Log.Party.Print("party {0}: kicking memberId={1} from party {2}.", cmd, member2.GameAccountId, BnetParty.GetJoinedParty(pId3));
							BnetParty.KickMember(pId3, member2.GameAccountId);
						}
					}
				}
			}
			else
			{
				partyId13 = ParsePartyId(cmd, cmdArgs[0], -1, ref errorMsg);
			}
			if (!(partyId13 != null))
			{
				break;
			}
			PartyInfo info4 = BnetParty.GetJoinedParty(partyId13);
			BnetParty.PartyMember[] members4 = BnetParty.GetMembers(partyId13);
			if (members4.Length == 1)
			{
				errorMsg = "party " + cmd + ": no members (other than self) for party " + info4;
				break;
			}
			string[] kickArgs = cmdArgs.Skip(1).ToArray();
			if (kickArgs.Length == 0)
			{
				Log.Party.Print("NOTE: party {0} without specifying member index will {0} all members (other than self).", cmd);
				BnetParty.PartyMember[] members3 = members4;
				foreach (BnetParty.PartyMember member3 in members3)
				{
					if (!(member3.GameAccountId == BnetPresenceMgr.Get().GetMyGameAccountId()))
					{
						Log.Party.Print("party {0}: kicking memberId={1} from party {2}.", cmd, member3.GameAccountId, info4);
						BnetParty.KickMember(partyId13, member3.GameAccountId);
					}
				}
				break;
			}
			for (int num9 = 0; num9 < kickArgs.Length; num9++)
			{
				if (ulong.TryParse(kickArgs[num9], out var indexOrId2))
				{
					BnetParty.PartyMember member4 = null;
					if (indexOrId2 < (ulong)members4.LongLength)
					{
						member4 = members4[indexOrId2];
					}
					else
					{
						member4 = members4.FirstOrDefault((BnetParty.PartyMember m) => m.GameAccountId.Low == indexOrId2);
						if (member4 == null)
						{
							Log.Party.Print("party {0}: unable to find member (id or index): {1} for party {2}", cmd, kickArgs[num9], info4);
						}
					}
					if (member4 != null)
					{
						if (member4.GameAccountId == BnetPresenceMgr.Get().GetMyGameAccountId())
						{
							Log.Party.Print("party {0}: cannot kick yourself (argIndex={1}); party={2}", cmd, num9, info4);
						}
						else
						{
							Log.Party.Print("party {0}: kicking memberId={1} from party {2}.", cmd, member4.GameAccountId, info4);
							BnetParty.KickMember(partyId13, member4.GameAccountId);
						}
					}
				}
				else
				{
					Log.Party.Print("party {0}: unable to parse member (id or index): {1}", cmd, kickArgs[num9]);
				}
			}
			break;
		}
		case "setprivacy":
		{
			BnetPartyId partyId10 = null;
			if (cmdArgs.Length < 2)
			{
				errorMsg = "party setprivacy: must specify a party (index or LowBits or type) and a PrivacyLevel: " + string.Join(" | ", Enum.GetValues(typeof(ChannelApi.PartyPrivacyLevel)).Cast<ChannelApi.PartyPrivacyLevel>().Select(delegate(ChannelApi.PartyPrivacyLevel v)
				{
					string text = v.ToString();
					int num6 = (int)v;
					return text + " (" + num6 + ")";
				})
					.ToArray());
			}
			else
			{
				partyId10 = ParsePartyId(cmd, cmdArgs[0], -1, ref errorMsg);
			}
			if (partyId10 != null)
			{
				ChannelApi.PartyPrivacyLevel? privacy = null;
				ChannelApi.PartyPrivacyLevel p2;
				if (int.TryParse(cmdArgs[1], out var intVal))
				{
					privacy = (ChannelApi.PartyPrivacyLevel)intVal;
				}
				else if (!EnumUtils.TryGetEnum<ChannelApi.PartyPrivacyLevel>(cmdArgs[1], out p2))
				{
					errorMsg = "party setprivacy: unknown PrivacyLevel specified: " + cmdArgs[1];
				}
				else
				{
					privacy = p2;
				}
				if (privacy.HasValue)
				{
					Log.Party.Print("party setprivacy: setting PrivacyLevel={0} for party {1}.", privacy.Value, BnetParty.GetJoinedParty(partyId10));
					BnetParty.SetPrivacy(partyId10, privacy.Value);
				}
			}
			break;
		}
		case "setlong":
		case "setstring":
		case "setblob":
		{
			bool isLong = cmd == "setlong";
			bool isString = cmd == "setstring";
			bool isBlob = cmd == "setblob";
			int keyArgIndex = 1;
			BnetPartyId partyId12 = null;
			if (cmdArgs.Length < 2)
			{
				errorMsg = "party " + cmd + ": must specify attributeKey and a value.";
			}
			else
			{
				partyId12 = ParsePartyId(cmd, cmdArgs[0], -1, ref errorMsg);
				if (partyId12 == null)
				{
					BnetPartyId[] joinedParties5 = BnetParty.GetJoinedPartyIds();
					if (joinedParties5.Length != 0)
					{
						Log.Party.Print("party {0}: treating first argument as attributeKey (and not PartyId) - will use PartyId at index 0", cmd);
						errorMsg = null;
						partyId12 = joinedParties5[0];
					}
				}
				else
				{
					Log.Party.Print("party {0}: treating first argument as PartyId (second argument will be attributeKey)", cmd);
				}
			}
			if (!(partyId12 != null))
			{
				break;
			}
			bool complete = false;
			string key2 = cmdArgs[keyArgIndex];
			string val2 = string.Join(" ", cmdArgs.Skip(keyArgIndex + 1).ToArray());
			if (isLong)
			{
				if (long.TryParse(val2, out var longVal))
				{
					BattleNet.SetPartyAttributes(partyId12, BnetAttribute.CreateAttribute(key2, longVal));
					complete = true;
				}
			}
			else if (isString)
			{
				BattleNet.SetPartyAttributes(partyId12, BnetAttribute.CreateAttribute(key2, val2));
				complete = true;
			}
			else if (isBlob)
			{
				byte[] blob2 = Encoding.UTF8.GetBytes(val2);
				BattleNet.SetPartyAttributes(partyId12, BnetAttribute.CreateAttribute(key2, blob2));
				complete = true;
			}
			else
			{
				errorMsg = "party " + cmd + ": unhandled attribute type!";
			}
			if (complete)
			{
				Log.Party.Print("party {0}: complete key={1} val={2} party={3}", cmd, key2, val2, BnetParty.GetJoinedParty(partyId12));
			}
			break;
		}
		case "clearattr":
		{
			BnetPartyId partyId3 = null;
			if (cmdArgs.Length < 2)
			{
				errorMsg = "party " + cmd + ": must specify attributeKey.";
			}
			else
			{
				partyId3 = ParsePartyId(cmd, cmdArgs[0], -1, ref errorMsg);
				if (partyId3 == null)
				{
					BnetPartyId[] joinedParties = BnetParty.GetJoinedPartyIds();
					if (joinedParties.Length != 0)
					{
						Log.Party.Print("party {0}: treating first argument as attributeKey (and not PartyId) - will use PartyId at index 0", cmd);
						errorMsg = null;
						partyId3 = joinedParties[0];
					}
				}
				else
				{
					Log.Party.Print("party {0}: treating first argument as PartyId (second argument will be attributeKey)", cmd);
				}
			}
			if (partyId3 != null)
			{
				string key = cmdArgs[1];
				BattleNet.ClearPartyAttribute(partyId3, key);
				Log.Party.Print("party {0}: cleared key={1} party={2}", cmd, key, BnetParty.GetJoinedParty(partyId3));
			}
			break;
		}
		case "subscribe":
		case "list":
		{
			IEnumerable<BnetPartyId> partyIds = null;
			if (cmdArgs.Length == 0)
			{
				PartyInfo[] infos = BnetParty.GetJoinedParties();
				if (infos.Length == 0)
				{
					Log.Party.Print("party list: no joined parties.");
				}
				else
				{
					Log.Party.Print("party list: listing all joined parties and the details of the party at index 0.");
					partyIds = new BnetPartyId[1] { infos[0].Id };
				}
				for (int j = 0; j < infos.Length; j++)
				{
					Log.Party.Print("   {0}", GetPartySummary(infos[j], j));
				}
			}
			else
			{
				partyIds = from p in cmdArgs.Select(delegate(string a, int i)
					{
						string errorMsg2 = null;
						BnetPartyId result = ParsePartyId(cmd, a, i, ref errorMsg2);
						if (errorMsg2 != null)
						{
							Log.Party.Print(errorMsg2);
						}
						return result;
					})
					where p != null
					select p;
			}
			if (partyIds != null)
			{
				int i2 = -1;
				foreach (BnetPartyId partyId2 in partyIds)
				{
					i2++;
					PartyInfo info = BnetParty.GetJoinedParty(partyId2);
					Log.Party.Print("party {0}: {1}", cmd, GetPartySummary(BnetParty.GetJoinedParty(partyId2), i2));
					BnetParty.PartyMember[] members = BnetParty.GetMembers(partyId2);
					if (members.Length == 0)
					{
						Log.Party.Print("   no members.");
					}
					else
					{
						Log.Party.Print("   members:");
					}
					for (int k = 0; k < members.Length; k++)
					{
						bool isMyself = members[k].GameAccountId == BnetPresenceMgr.Get().GetMyGameAccountId();
						Log.Party.Print("      [{0}] {1} isMyself={2} isLeader={3} roleIds={4}", k, members[k].GameAccountId, isMyself, members[k].IsLeader(info.Type), string.Join(",", members[k].RoleIds.Select((uint r) => r.ToString()).ToArray()));
					}
					PartyInvite[] sentInvites = BnetParty.GetSentInvites(partyId2);
					if (sentInvites.Length == 0)
					{
						Log.Party.Print("   no sent invites.");
					}
					else
					{
						Log.Party.Print("   sent invites:");
					}
					for (int l = 0; l < sentInvites.Length; l++)
					{
						PartyInvite invite = sentInvites[l];
						Log.Party.Print("      {0}", GetPartyInviteSummary(invite, l));
					}
					BattleNet.GetAllPartyAttributes(partyId2, out var attrs);
					if (attrs.Length == 0)
					{
						Log.Party.Print("   no party attributes.");
					}
					else
					{
						Log.Party.Print("   party attributes:");
					}
					foreach (Blizzard.GameService.Protocol.V2.Client.Attribute attr in attrs)
					{
						string val = ((attr.Value == null) ? "<null>" : $"[{attr.Value.GetType().Name}]{attr.Value.ToString()}");
						if (attr.Value.HasBlobValue)
						{
							byte[] blob = attr.Value.BlobValue.ToByteArray();
							val = "blobLength=" + blob.Length;
							try
							{
								string decodedVal = Encoding.UTF8.GetString(blob);
								if (decodedVal != null)
								{
									val = val + " decodedUtf8=" + decodedVal;
								}
							}
							catch (ArgumentException)
							{
							}
						}
						Log.Party.Print("      {0}={1}", attr.Name ?? "<null>", val);
					}
				}
			}
			PartyInvite[] receivedInvites = BnetParty.GetReceivedInvites();
			if (receivedInvites.Length == 0)
			{
				Log.Party.Print("party list: no received party invites.");
			}
			else
			{
				Log.Party.Print("party list: received party invites:");
			}
			for (int num = 0; num < receivedInvites.Length; num++)
			{
				PartyInvite invite2 = receivedInvites[num];
				Log.Party.Print("   {0}", GetPartyInviteSummary(invite2, num));
			}
			break;
		}
		default:
			errorMsg = "party: unknown party cmd: " + cmd;
			break;
		}
		if (errorMsg != null)
		{
			Log.Party.Print(errorMsg);
			Error.AddWarning("Party Cheat Error", errorMsg);
			success = false;
		}
		return success;
	}

	private static string GetPartyInviteSummary(PartyInvite invite, int index)
	{
		return string.Format("{0}: inviteId={1} sender={2} recipient={3} party={4}", (index >= 0) ? $"[{index}] " : "", invite.InviteId, invite.InviterId?.ToString() + " " + invite.InviterName, invite.InviteeId, new PartyInfo(invite.PartyId, invite.PartyType));
	}

	private static string GetPartySummary(PartyInfo info, int index)
	{
		BnetParty.PartyMember leader = BnetParty.GetLeader(info.Id);
		return string.Format("{0}{1}: members={2} invites={3} privacy={4} leader={5}", (index >= 0) ? $"[{index}] " : "", info, BnetParty.CountMembers(info.Id) + (BnetParty.IsPartyFull(info.Id) ? "(full)" : ""), BnetParty.GetSentInvites(info.Id).Length, BnetParty.GetPrivacyLevel(info.Id), (leader == null) ? "null" : leader.GameAccountId.ToString());
	}

	private bool OnProcessCheat_cheat(string func, string[] args, string rawArgs, AutofillData autofillData)
	{
		string allCmdsCsv = "spawncard, drawcard, loadcard, cyclehand, shuffle, addmana, readymana, maxmana, nocosts, healhero, healentity, nuke, damage, settag, ready, exhaust, freeze, move, undo, destroyhero, tiegame, getgsd, aiplaylastspawnedcard, forcestallingprevention, endturn, logrelay";
		if (autofillData != null)
		{
			string[] allCmds = null;
			string[] paramsPlayer = new string[2] { "friendly", "opponent" };
			string[] paramsZoneName = new string[7] { "InPlay", "InDeck", "InHand", "InGraveyard", "InRemovedFromGame", "InSetAside", "InSecret" };
			Func<string[]> paramsCardIds = () => GameDbf.GetIndex().GetAllCardIds().ToArray();
			string searchTerm = autofillData.m_lastAutofillParamPrefix ?? ((args.Length == 0) ? string.Empty : args.Last());
			int argsLength = args.Length;
			if (rawArgs.EndsWith(" "))
			{
				searchTerm = string.Empty;
				argsLength++;
			}
			if (argsLength > 1 && !string.IsNullOrEmpty(args[0]))
			{
				allCmdsCsv = null;
				switch (args[0]?.ToLower())
				{
				case "spawncard":
					switch (argsLength)
					{
					case 4:
						allCmds = new string[2] { "1", "0" };
						break;
					case 3:
						allCmds = paramsZoneName;
						break;
					case 2:
						allCmds = paramsCardIds();
						break;
					}
					break;
				case "loadcard":
					if (argsLength == 2)
					{
						allCmds = paramsCardIds();
					}
					break;
				case "drawcard":
				case "cyclehand":
				case "shuffle":
				case "addmana":
				case "readymana":
				case "maxmana":
				case "healhero":
				case "nuke":
				case "destroyhero":
					if (argsLength == 2)
					{
						allCmds = paramsPlayer;
					}
					break;
				case "move":
					if (argsLength == 3)
					{
						allCmds = paramsZoneName;
					}
					break;
				case "getgsd":
					if (argsLength == 2)
					{
						allCmds = paramsPlayer;
					}
					break;
				}
			}
			if (allCmds == null)
			{
				if (allCmdsCsv == null)
				{
					return false;
				}
				allCmds = allCmdsCsv.Split(new char[2] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);
			}
			return ProcessAutofillParam(allCmds, searchTerm, autofillData);
		}
		if (!Network.Get().IsConnectedToGameServer())
		{
			UIStatus.Get().AddInfoNoRichText("Not connected to a game. Cannot send cheat command.");
			return true;
		}
		Network.Get().SendDebugConsoleCommand(rawArgs);
		return true;
	}

	private bool OnProcessCheat_autohand(string func, string[] args, string rawArgs)
	{
		if (args.Length == 0)
		{
			return false;
		}
		if (!GeneralUtils.TryParseBool(args[0], out var set))
		{
			return false;
		}
		if (InputManager.Get() == null)
		{
			return false;
		}
		string message = ((!set) ? "auto hand hiding is off" : "auto hand hiding is on");
		Debug.Log(message);
		UIStatus.Get().AddInfo(message);
		InputManager.Get().SetHideHandAfterPlayingCard(set);
		return true;
	}

	private bool OnProcessCheat_adventureChallengeUnlock(string func, string[] args, string rawArgs)
	{
		if (args.Length < 1)
		{
			return false;
		}
		if (!int.TryParse(args[0].ToLowerInvariant(), out var wingId))
		{
			return false;
		}
		List<int> wings = new List<int>();
		wings.Add(wingId);
		AdventureMissionDisplay.Get().ShowClassChallengeUnlock(wings);
		return true;
	}

	private bool OnProcessCheat_iks(string func, string[] args, string rawArgs)
	{
		InnKeepersSpecial.Get().InitializeJsonURL(args[0]);
		InnKeepersSpecial.Get().ResetAdUrl();
		Processor.RunCoroutine(TriggerWelcomeQuestShow());
		return true;
	}

	private IEnumerator TriggerWelcomeQuestShow()
	{
		yield return new WaitForSeconds(1f);
		while (InnKeepersSpecial.Get().ProcessingResponse)
		{
			yield return new WaitForSeconds(1f);
		}
		QuestManager.Get().SimulateQuestNotificationPopup(QuestPool.QuestPoolType.DAILY);
	}

	private bool OnProcessCheat_iksgameaction(string func, string[] args, string rawArgs)
	{
		if (string.IsNullOrEmpty(rawArgs))
		{
			UIStatus.Get().AddError("Please specify a game action.");
			return true;
		}
		DeepLinkManager.ExecuteDeepLink(args, DeepLinkManager.DeepLinkSource.INNKEEPERS_SPECIAL, 0);
		return true;
	}

	private bool OnProcessCheat_iksseen(string func, string[] args, string rawArgs)
	{
		if (string.IsNullOrEmpty(rawArgs))
		{
			UIStatus.Get().AddError("Please specify a game action.");
			return true;
		}
		string gameAction = string.Join(" ", args);
		UIStatus.Get().AddInfo("Has Interacted With Product: " + InnKeepersSpecial.Get().HasInteractedWithAdvertisedProduct(gameAction));
		return true;
	}

	private bool OnProcessCheat_quote(string func, string[] args, string rawArgs)
	{
		string characterPrefab = "innkeeper";
		string gameStringKey = "VO_INNKEEPER_FIRST_100_GOLD";
		string soundPrefab = "VO_INNKEEPER_FIRST_100_GOLD.prefab:c6a50337099a454488acd96d2f37320f";
		if (args.Length < 1 || !string.Equals(args[0], "default", StringComparison.OrdinalIgnoreCase))
		{
			if (args.Length < 2)
			{
				UIStatus.Get().AddError("Please specify 2 arguments: CharacterPrefabAssetRef GameStringsKey [AudioAssetRef]\nExamples:\nquote default\nquote innkeeper VO_TUTORIAL_01_ANNOUNCER_05 VO_TUTORIAL_01_ANNOUNCER_05.prefab:635b33010e4704a42a87c7625b5b5ada\nquote Barnes_Quote.prefab:2e7e9f28b5bc37149a12b2e5feaa244a VO_Barnes_Male_Human_JulianneWin_01 VO_Barnes_Male_Human_JulianneWin_01.prefab:09d4c4aaf43ac634aaf325c2badc72a8", 5f * Time.timeScale);
				return true;
			}
			characterPrefab = args[0];
			gameStringKey = args[1];
			soundPrefab = gameStringKey;
			if (args.Length > 2)
			{
				soundPrefab = args[2];
			}
		}
		if (characterPrefab.ToLowerInvariant().Contains("innkeeper"))
		{
			NotificationManager.Get().CreateInnkeeperQuote(UserAttentionBlocker.ALL, NotificationManager.DEFAULT_CHARACTER_POS, GameStrings.Get(gameStringKey), soundPrefab);
		}
		else
		{
			NotificationManager.Get().CreateCharacterQuote(characterPrefab, NotificationManager.DEFAULT_CHARACTER_POS, GameStrings.Get(gameStringKey), soundPrefab);
		}
		return true;
	}

	private bool OnProcessCheat_popuptext(string func, string[] args, string rawArgs)
	{
		if (args.Length < 1)
		{
			return false;
		}
		string text = args[0];
		NotificationManager.Get().CreatePopupText(UserAttentionBlocker.ALL, Box.Get().m_LeftDoor.transform.position, TutorialEntity.GetTextScale(), text);
		return true;
	}

	private bool OnProcessCheat_demotext(string func, string[] args, string rawArgs)
	{
		if (args.Length < 1)
		{
			return false;
		}
		string text = args[0];
		DemoMgr.Get().CreateDemoText(text);
		return true;
	}

	private bool OnProcessCheat_alerttext(string func, string[] args, string rawArgs)
	{
		if (args.Length < 1)
		{
			return false;
		}
		AlertPopup.PopupInfo info = new AlertPopup.PopupInfo();
		info.m_text = rawArgs;
		info.m_showAlertIcon = true;
		info.m_responseDisplay = AlertPopup.ResponseDisplay.NONE;
		DialogManager.Get().ShowPopup(info);
		return true;
	}

	private bool OnProcessCheat_FatalMgrText(string func, string[] args, string rawArgs)
	{
		if (args.Length < 1)
		{
			return false;
		}
		FatalErrorMessage message = new FatalErrorMessage();
		string text = GameStrings.Get(rawArgs);
		if (text == null && !string.IsNullOrEmpty(rawArgs))
		{
			Debug.LogWarning("Could not find game strings for " + rawArgs + ", will display it as is.");
			text = rawArgs;
		}
		if (text == null)
		{
			Debug.LogWarning("Invalid value");
			return false;
		}
		message.m_text = text;
		FatalErrorMgr.Get().Add(message);
		typeof(SceneMgr).GetMethod("GoToFatalErrorScreen", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(SceneMgr.Get(), new object[1] { message });
		return true;
	}

	private bool OnProcessCheat_logtext(string func, string[] args, string rawArgs)
	{
		if (args.Length < 1)
		{
			return false;
		}
		if (args.Length > 1)
		{
			string text = rawArgs.Substring(rawArgs.IndexOf(' ') + 1);
			switch (args[0]?.ToLower())
			{
			case "debug":
				Log.All.PrintDebug(text);
				return true;
			case "info":
				Log.All.PrintInfo(text);
				return true;
			case "warning":
				Log.All.PrintWarning(text);
				return true;
			case "error":
				Log.All.PrintError(text);
				return true;
			}
		}
		Log.All.Print(rawArgs);
		return true;
	}

	private bool OnProcessCheat_logenable(string func, string[] args, string rawArgs)
	{
		if (args.Count() < 3)
		{
			return false;
		}
		string logName = args[0];
		LogInfo logInfo = LogSystem.Get().GetLogInfo(logName);
		if (logInfo == null)
		{
			return false;
		}
		string obj = args[1];
		string enabledStr = args[2];
		bool enabled = !enabledStr.Equals("false", StringComparison.OrdinalIgnoreCase) && enabledStr != "0";
		switch (obj?.ToLower())
		{
		case "file":
			logInfo.m_filePrinting = enabled;
			break;
		case "screen":
			logInfo.m_screenPrinting = enabled;
			break;
		case "console":
			logInfo.m_consolePrinting = enabled;
			break;
		default:
			return false;
		}
		LogSystem.Get().SetLogInfo(logName, logInfo);
		return true;
	}

	private bool OnProcessCheat_loglevel(string func, string[] args, string rawArgs)
	{
		if (args.Count() < 2)
		{
			return false;
		}
		string logName = args.ElementAtOrDefault(0);
		if (!EnumUtils.TryGetEnum<Blizzard.T5.Logging.LogLevel>(args.ElementAtOrDefault(1), StringComparison.OrdinalIgnoreCase, out var logLevel))
		{
			return false;
		}
		LogInfo logInfo = LogSystem.Get().GetLogInfo(logName);
		if (logInfo == null)
		{
			return false;
		}
		logInfo.m_minLevel = logLevel;
		LogSystem.Get().SetLogInfo(logName, logInfo);
		return true;
	}

	private bool OnProcessCheat_cardchangereset(string func, string[] args, string rawArgs)
	{
		if (args.Length == 1)
		{
			string eventName = args[0];
			if (!string.IsNullOrWhiteSpace(eventName))
			{
				long eventId = EventTimingManager.Get().GetEventIdFromEventName(eventName);
				GameSaveDataManager.Get().GetSubkeyValue(GameSaveKeyId.PLAYER_OPTIONS, GameSaveKeySubkeyId.LIST_OF_SEEN_CARD_CHANGES, out List<long> seenCardChangeEventIds);
				if (seenCardChangeEventIds != null && seenCardChangeEventIds.Contains(eventId))
				{
					seenCardChangeEventIds.Remove(eventId);
					GameSaveDataManager.Get().SaveSubkey(new GameSaveDataManager.SubkeySaveRequest(GameSaveKeyId.PLAYER_OPTIONS, GameSaveKeySubkeyId.LIST_OF_SEEN_CARD_CHANGES, seenCardChangeEventIds.ToArray()));
					UIStatus.Get().AddInfo("Card Change popup for " + eventName + " will be displayed on next login", 10f);
					return true;
				}
				UIStatus.Get().AddInfo("Error: ${eventName} does not exist or subkey is empty", 10f);
				return false;
			}
		}
		GameSaveDataManager.Get().SaveSubkey(new GameSaveDataManager.SubkeySaveRequest(GameSaveKeyId.PLAYER_OPTIONS, GameSaveKeySubkeyId.LIST_OF_SEEN_CARD_CHANGES, new long[0]));
		UIStatus.Get().AddInfo("Card Change popup for all events will be displayed on next login", 10f);
		return true;
	}

	private bool OnProcessCheat_questchangereset(string func, string[] args, string rawArgs)
	{
		GameSaveDataManager.Get().SaveSubkey(new GameSaveDataManager.SubkeySaveRequest(GameSaveKeyId.PLAYER_OPTIONS, GameSaveKeySubkeyId.LIST_OF_SEEN_QUEST_CHANGES_ON_LOGIN, new long[0]));
		GameSaveDataManager.Get().SaveSubkey(new GameSaveDataManager.SubkeySaveRequest(GameSaveKeyId.PLAYER_OPTIONS, GameSaveKeySubkeyId.LIST_OF_SEEN_QUEST_CHANGES_ON_JOURNAL, new long[0]));
		UIStatus.Get().AddInfo("Quest Change popups for all events will be displayed on next login", 10f);
		return true;
	}

	private bool OnProcessCheat_showcardchangeevent(string func, string[] args, string rawArgs)
	{
		if (args.Length > 2)
		{
			UIStatus.Get().AddInfo($"Error: Too many arguments, expected at most 2, passed {args.Length}", 10f);
			return false;
		}
		if (string.IsNullOrEmpty(args[0]) && !PopupDisplayManager.Get().CardPopups.ForceShowChangedCardEvent())
		{
			UIStatus.Get().AddInfo("No active card change events to show", 10f);
			return true;
		}
		string eventName = args[0];
		if (!string.IsNullOrEmpty(args[1]))
		{
			eventName = eventName + " " + args[1];
		}
		EventTimingType eventToShow = EventTimingManager.Get().GetEventType(eventName);
		if (eventToShow != EventTimingType.UNKNOWN)
		{
			if (!PopupDisplayManager.Get().CardPopups.ForceShowChangedCardEvent(eventToShow))
			{
				UIStatus.Get().AddInfo("Error: ForceShowChangeCardEvent returned false", 10f);
				return false;
			}
		}
		else
		{
			UIStatus.Get().AddInfo("Error: Unknown eventName", 10f);
		}
		return true;
	}

	private bool OnProcessCheat_loginpopupsequence(string func, string[] args, string rawArgs)
	{
		bool suppress = PopupDisplayManager.SuppressPopupsForNewPlayer;
		bool disableNotifications = PopupDisplayManager.ShouldDisableNotificationOnLogin();
		PopupDisplayManager.Get().LoginPopups.ShowLoginPopupSequence(suppress, disableNotifications, PopupDisplayManager.Get().CardPopups);
		return true;
	}

	private bool OnProcessCheat_loginpopupreset(string func, string[] args, string rawArgs)
	{
		GameSaveDataManager.Get().SaveSubkey(new GameSaveDataManager.SubkeySaveRequest(GameSaveKeyId.PLAYER_OPTIONS, GameSaveKeySubkeyId.LOGIN_POPUP_SEQUENCE_SEEN_POPUPS, new long[0]));
		return true;
	}

	private bool OnProcessCheat_favoritehero(string func, string[] args, string rawArgs)
	{
		if (!(SceneMgr.Get().GetScene() is CollectionManagerScene))
		{
			Debug.LogWarning("OnProcessCheat_favoritehero must be used from the CollectionManagaer!");
			return false;
		}
		if (args.Length != 3)
		{
			return false;
		}
		if (!int.TryParse(args[0].ToLowerInvariant(), out var classIDAsInt))
		{
			return false;
		}
		if (!EnumUtils.TryCast<TAG_CLASS>(classIDAsInt, out var heroClass))
		{
			return false;
		}
		string heroCardID = args[1];
		if (!int.TryParse(args[2].ToLowerInvariant(), out var heroPremiumAsInt))
		{
			return false;
		}
		if (!EnumUtils.TryCast<TAG_PREMIUM>(heroPremiumAsInt, out var heroPremium))
		{
			return false;
		}
		NetCache.CardDefinition favoriteHero = new NetCache.CardDefinition
		{
			Name = heroCardID,
			Premium = heroPremium
		};
		Log.All.Print("OnProcessCheat_favoritehero setting favorite hero to {0} for class {1}", favoriteHero, heroClass);
		Network.Get().SetFavoriteHero(heroClass, favoriteHero, isFavorite: true);
		return true;
	}

	private bool OnProcessCheat_PlayFinisher(string func, string[] args, string rawArgs, AutofillData autofillData)
	{
		string[] allCmds = new string[3] { "help", "player", "opponent" };
		if (args.Length < 1 || string.IsNullOrEmpty(rawArgs))
		{
			if (autofillData != null)
			{
				return ProcessAutofillParam(allCmds, string.Empty, autofillData);
			}
			UIStatus.Get().AddError("Must specify a sub-command.");
			return true;
		}
		string cmd = args[0].ToLower();
		string[] cmdArgs = args.Skip(1).ToArray();
		if (autofillData != null && args.Length == 1 && !rawArgs.EndsWith(" "))
		{
			string prefix = cmd;
			return ProcessAutofillParam(allCmds, prefix, autofillData);
		}
		switch (cmd?.ToLower())
		{
		default:
		{
			StringBuilder stringBuilder = new StringBuilder("finisher help - show finisher cheats");
			stringBuilder.AppendLine("finisher player id=X large - player does large finisher X");
			stringBuilder.AppendLine("finisher opponent id=X small - opponent does small finisher X");
			UIStatus.Get().AddInfo(stringBuilder.ToString(), 10f);
			return true;
		}
		case "player":
		case "opponent":
		{
			GameState gameState = GameState.Get();
			if (gameState == null)
			{
				UIStatus.Get().AddError("Cannot play a finisher. GameState is null. Are you in a game?", 10f);
				return true;
			}
			GameMgr gameMgr = GameMgr.Get();
			if (gameMgr == null)
			{
				UIStatus.Get().AddError("Cannot play a finisher. GameMgr is somehow null.", 10f);
				return true;
			}
			if (!gameMgr.IsBattlegrounds() && !gameMgr.IsBattlegroundsTutorial())
			{
				UIStatus.Get().AddError("Cannot play a finisher. Not in a Battlegrounds game or Battlegrounds tutorial.", 10f);
				return true;
			}
			Actor friendlyActor = gameState.GetFriendlySidePlayer().GetHeroCard().GetActor();
			Actor opposingActor = gameState.GetOpposingSidePlayer().GetHeroCard().GetActor();
			Actor sourceActor = ((cmd == "player") ? friendlyActor : opposingActor);
			Actor targetActor = ((cmd == "player") ? opposingActor : friendlyActor);
			string[] allSubCmds = new string[3] { "id", "small", "large" };
			bool useLarge = false;
			int finisherId = 0;
			string[] array = cmdArgs;
			foreach (string arg in array)
			{
				string argName = arg;
				string argValue = null;
				int equalsIndex = arg.IndexOf('=');
				if (equalsIndex >= 0)
				{
					argName = arg.Substring(0, equalsIndex);
					argValue = arg.Substring(equalsIndex + 1);
				}
				if (!allSubCmds.Contains(argName))
				{
					UIStatus.Get().AddError("Unrecognized sub command \"" + argName + "\". Enter cheat \"finisher help\" for more information.", 10f);
					continue;
				}
				switch (argName?.ToLower())
				{
				case "id":
					if (!int.TryParse(argValue, out finisherId))
					{
						UIStatus.Get().AddError("Could not parse \"" + argValue + "\" as an integer. Enter cheat \"finisher help\" for more information.");
						return true;
					}
					if (GameDbf.BattlegroundsFinisher.GetRecord(finisherId) == null)
					{
						UIStatus.Get().AddError($"No finisher with ID:\"{finisherId}\". Enter cheat \"finisher help\" for more information.");
						return true;
					}
					break;
				case "small":
					useLarge = false;
					break;
				case "large":
					useLarge = true;
					break;
				}
			}
			PowerTaskList powerTaskList = new PowerTaskList();
			Network.HistBlockStart blockStart = new Network.HistBlockStart(HistoryBlock.Type.ATTACK);
			Network.HistBlockEnd blockEnd = new Network.HistBlockEnd();
			Network.HistTagChange defendTagChange = new Network.HistTagChange();
			defendTagChange.Tag = 36;
			defendTagChange.Value = 1;
			defendTagChange.Entity = targetActor.GetEntity().GetEntityId();
			Network.HistTagChange attackTagChange = new Network.HistTagChange();
			attackTagChange.Tag = 38;
			attackTagChange.Value = 1;
			attackTagChange.Entity = sourceActor.GetEntity().GetEntityId();
			sourceActor.GetEntity().SetTag(GAME_TAG.BATTLEGROUNDS_FAVORITE_FINISHER, finisherId);
			int atkDamage = ((!useLarge) ? 1 : 999);
			sourceActor.GetEntity().SetTag(GAME_TAG.ATK, atkDamage);
			targetActor.GetEntity().SetTag(GAME_TAG.DAMAGE, atkDamage);
			powerTaskList.SetBlockStart(blockStart);
			powerTaskList.SetBlockEnd(blockEnd);
			powerTaskList.CreateTask(defendTagChange);
			powerTaskList.CreateTask(attackTagChange);
			GameState.Get().GetPowerProcessor().PerformTaskListOnCurrentGameState(powerTaskList);
			return true;
		}
		}
	}

	private bool OnProcessCheat_settag(string func, string[] args, string rawArgs)
	{
		if (args.Length != 3)
		{
			return false;
		}
		int tagID = int.Parse(args[0]);
		if (tagID <= 0)
		{
			return false;
		}
		int tagValue = int.Parse(args[2]);
		if (tagValue < 0)
		{
			return false;
		}
		int entityID = 0;
		if (!int.TryParse(args[1], out entityID))
		{
			string entityIdentifier = args[1];
			Network.Get().SetTag(tagID, entityIdentifier, tagValue);
			return true;
		}
		Network.Get().SetTag(tagID, entityID, tagValue);
		return true;
	}

	private bool OnProcessCheat_debugscript(string func, string[] args, string rawArgs)
	{
		ScriptDebugDisplay.Get().ToggleDebugDisplay(shouldDisplay: true);
		if (args.Length != 1)
		{
			return false;
		}
		string powerGUID = args[0];
		Network.Get().DebugScript(powerGUID);
		return true;
	}

	private bool OnProcessCheat_disablescriptdebug(string func, string[] args, string rawArgs)
	{
		ScriptDebugDisplay.Get().ToggleDebugDisplay(shouldDisplay: false);
		Network.Get().DisableScriptDebug();
		return true;
	}

	private bool OnProcessCheat_printpersistentlist(string func, string[] args, string rawArgs)
	{
		if (args.Length == 0 || args[0] == "")
		{
			Network.Get().PrintPersistentList(0);
			return true;
		}
		for (int i = 0; i < args.Length; i++)
		{
			int parameter = int.Parse(args[i]);
			Network.Get().PrintPersistentList(parameter);
		}
		return true;
	}

	private bool OnProcessCheat_togglecardtext(string func, string[] args, string rawArgs)
	{
		m_cardTextEnabled = !m_cardTextEnabled;
		if (SceneMgr.Get().IsInGame())
		{
			foreach (Zone zone in ZoneMgr.Get().GetZones())
			{
				foreach (Card card in zone.GetCards())
				{
					if (card.GetActor() != null)
					{
						card.GetActor().UpdatePowersText();
					}
				}
			}
		}
		return true;
	}

	private bool OnProcessCheat_togglecardnames(string func, string[] args, string rawArgs)
	{
		m_cardNamesEnabled = !m_cardNamesEnabled;
		if (SceneMgr.Get().IsInGame())
		{
			foreach (Zone zone in ZoneMgr.Get().GetZones())
			{
				foreach (Card card in zone.GetCards())
				{
					if (card.GetActor() != null)
					{
						card.GetActor().UpdateNameText();
					}
				}
			}
		}
		return true;
	}

	private bool OnProcessCheat_toggleracetext(string func, string[] args, string rawArgs)
	{
		m_cardRaceTextEnabled = !m_cardRaceTextEnabled;
		if (SceneMgr.Get().IsInGame())
		{
			foreach (Zone zone in ZoneMgr.Get().GetZones())
			{
				foreach (Card card in zone.GetCards())
				{
					if (card.GetActor() != null && card.GetEntity() != null)
					{
						card.GetActor().UpdateTextComponents(card.GetEntity());
					}
				}
			}
		}
		return true;
	}

	private bool OnProcessCheat_removeplayernames(string func, string[] args, string rawArgs)
	{
		m_playerNamesEnabled = false;
		if (SceneMgr.Get().IsInGame())
		{
			Gameplay.Get().RemoveNameBanners();
		}
		return true;
	}

	private bool OnProcessCheat_toggleForcingApprenticeGame(string func, string[] args, string rawArgs)
	{
		m_forcingApprenticeGameEnabled = !m_forcingApprenticeGameEnabled;
		UIStatus.Get().AddInfo($"Apprentice game override set to {m_forcingApprenticeGameEnabled}.", 10f);
		return true;
	}

	private bool OnProcessCheat_help(string func, string[] args, string rawArgs)
	{
		StringBuilder helpText = new StringBuilder();
		string command = null;
		if (args.Length != 0 && !string.IsNullOrEmpty(args[0]))
		{
			command = args[0];
		}
		List<string> commands = new List<string>();
		if (command != null)
		{
			foreach (string cheat in CheatMgr.Get().GetCheatCommands())
			{
				if (cheat.Contains(command))
				{
					commands.Add(cheat);
				}
			}
		}
		else
		{
			foreach (string cheat2 in CheatMgr.Get().GetCheatCommands())
			{
				commands.Add(cheat2);
			}
		}
		if (commands.Count == 1)
		{
			command = commands[0];
		}
		if (command == null || commands.Count != 1)
		{
			if (command == null)
			{
				helpText.Append("All available cheat commands:\n");
			}
			else
			{
				helpText.Append("Cheat commands containing: \"" + command + "\"\n");
			}
			int cheatCount = 0;
			string helpLine = "";
			foreach (string cheatCommand in commands)
			{
				helpLine = helpLine + cheatCommand + ", ";
				cheatCount++;
				if (cheatCount > 4)
				{
					cheatCount = 0;
					helpText.Append(helpLine);
					helpLine = "";
				}
			}
			if (!string.IsNullOrEmpty(helpLine))
			{
				helpText.Append(helpLine);
			}
			UIStatus.Get().AddInfo("look at console for help info", 10f);
		}
		else
		{
			string desc = "";
			CheatMgr.Get().cheatDesc.TryGetValue(command, out desc);
			string cheatArgs = "";
			CheatMgr.Get().cheatArgs.TryGetValue(command, out cheatArgs);
			helpText.Append("Usage: ");
			helpText.Append(command);
			if (!string.IsNullOrEmpty(cheatArgs))
			{
				helpText.Append(" " + cheatArgs);
			}
			if (!string.IsNullOrEmpty(desc))
			{
				helpText.Append("\n(" + desc + ")");
			}
			UIStatus.Get().AddInfo(helpText.ToString(), 20f);
		}
		Debug.Log("found commands " + commands.Count + "\n" + helpText);
		return true;
	}

	private bool OnProcessCheat_fixedrewardcomplete(string func, string[] args, string rawArgs)
	{
		if (args.Length < 1 || string.IsNullOrEmpty(args[0]))
		{
			return false;
		}
		if (!GeneralUtils.TryParseInt(args[0], out var fixedRewardMapID))
		{
			return false;
		}
		return FixedRewardsMgr.Get().Cheat_ShowFixedReward(fixedRewardMapID, PositionLoginFixedReward);
	}

	private void PositionLoginFixedReward(Reward reward)
	{
		PegasusScene currentScene = SceneMgr.Get().GetScene();
		reward.transform.parent = currentScene.transform;
		reward.transform.localRotation = Quaternion.identity;
		reward.transform.localPosition = PopupDisplayManager.Get().RewardPopups.GetRewardLocalPos();
	}

	private bool OnProcessCheat_example(string func, string[] args, string rawArgs)
	{
		if (args.Length < 1 || string.IsNullOrEmpty(args[0]))
		{
			return false;
		}
		string command = args[0];
		string exampleArgs = "";
		if (!CheatMgr.Get().cheatExamples.TryGetValue(command, out exampleArgs))
		{
			return false;
		}
		CheatMgr.Get().ProcessCheat(command + " " + exampleArgs);
		return true;
	}

	private bool OnProcessCheat_tavernbrawl(string func, string[] args, string rawArgs)
	{
		string usage = "USAGE: tb [cmd] [args]\nCommands: view, get, set, refresh, scenario, reset";
		if (args.Length < 1 || args.Any((string a) => string.IsNullOrEmpty(a)))
		{
			UIStatus.Get().AddInfo(usage, 10f);
			return true;
		}
		string cmd = args[0]?.ToLower();
		string[] cmdArgs = args.Skip(1).ToArray();
		string status = null;
		switch (cmd)
		{
		case "help":
			status = "usage";
			break;
		case "reset":
			if (cmdArgs.Length == 0)
			{
				status = "Please specify what to reset: seen, toserver";
			}
			else if ("toserver".Equals(cmdArgs[0], StringComparison.InvariantCultureIgnoreCase))
			{
				if (TavernBrawlManager.Get().IsCheated)
				{
					TavernBrawlManager.Get().Cheat_ResetToServerData();
					TavernBrawlMission cm = TavernBrawlManager.Get().CurrentMission();
					status = ((cm != null) ? ("TB settings reset to server-specified Scenario ID " + cm.missionId) : "TB settings reset to server-specified Scenario ID <null>");
				}
				else
				{
					status = "TB not locally cheated. Already using server-specified data.";
				}
			}
			else if ("seen".Equals(cmdArgs[0], StringComparison.InvariantCultureIgnoreCase))
			{
				int newValue = 0;
				if (cmdArgs.Length > 1 && !int.TryParse(cmdArgs[1], out newValue))
				{
					status = "Error parsing new seen value: " + cmdArgs[1];
				}
				if (status == null)
				{
					TavernBrawlManager.Get().Cheat_ResetSeenStuff(newValue);
					status = "all \"seentb*\" client-options reset to " + newValue;
				}
			}
			else
			{
				status = "Unknown reset parameter: " + cmdArgs[0];
			}
			break;
		case "refresh":
		{
			for (BrawlType brawlType2 = BrawlType.BRAWL_TYPE_TAVERN_BRAWL; brawlType2 < BrawlType.BRAWL_TYPE_COUNT; brawlType2++)
			{
				TavernBrawlManager.Get().RefreshServerData(brawlType2);
			}
			status = "TB refreshing";
			break;
		}
		case "fake_active_session":
		{
			int sessionStatus = 0;
			int.TryParse(args[1], out sessionStatus);
			TavernBrawlManager.Get().Cheat_SetActiveSession(sessionStatus);
			status = "Fake Tavern Brawl Session set.";
			break;
		}
		case "do_rewards":
		{
			int wins = 0;
			int.TryParse(cmdArgs[0], out wins);
			TavernBrawlMode mode = TavernBrawlMode.TB_MODE_NORMAL;
			if (cmdArgs.Length > 1)
			{
				mode = (cmdArgs[1].Equals("heroic") ? TavernBrawlMode.TB_MODE_HEROIC : TavernBrawlMode.TB_MODE_NORMAL);
			}
			TavernBrawlManager.Get().Cheat_DoHeroicRewards(wins, mode);
			status = "Doing reward animation and ending fake session if one exists.";
			break;
		}
		case "get":
		case "set":
		{
			bool isSet = cmd == "set";
			string varName = cmdArgs.FirstOrDefault();
			if (string.IsNullOrEmpty(varName))
			{
				status = $"Please specify a TB variable to {cmd}. Variables:RefreshTime";
				break;
			}
			string varValue = null;
			switch (varName.ToLower())
			{
			case "refreshtime":
				if (isSet)
				{
					status = "cannot set RefreshTime";
				}
				else
				{
					varValue = TavernBrawlManager.Get().CurrentScheduledSecondsToRefresh + " secs";
				}
				break;
			case "wins":
			{
				int numWins = 0;
				int.TryParse(args[2], out numWins);
				TavernBrawlManager.Get().Cheat_SetWins(numWins);
				status = $"tb set wins {numWins} successful";
				break;
			}
			case "losses":
			{
				int numLosses = 0;
				int.TryParse(args[2], out numLosses);
				TavernBrawlManager.Get().Cheat_SetLosses(numLosses);
				status = $"tb set losses {numLosses} successful";
				break;
			}
			}
			if (isSet)
			{
				status = string.Format("tb set {0} {1} successful.", varName, (cmdArgs.Length >= 2) ? cmdArgs[1] : "null");
			}
			else if (string.IsNullOrEmpty(status))
			{
				status = string.Format("tb variable {0}: {1}", varName, varValue ?? "null");
			}
			break;
		}
		case "view":
		{
			TavernBrawlMission mission = TavernBrawlManager.Get().CurrentMission();
			if (mission == null)
			{
				status = "No active Tavern Brawl at this time.";
				break;
			}
			string missionName = "";
			string missionDesc = "";
			ScenarioDbfRecord scenarioDbf = GameDbf.Scenario.GetRecord(mission.missionId);
			if (scenarioDbf != null)
			{
				missionName = scenarioDbf.Name;
				missionDesc = scenarioDbf.Description;
			}
			status = $"Active TB: [{mission.missionId}] {missionName}\n{missionDesc}";
			break;
		}
		case "scen":
		case "scenario":
		{
			if (cmdArgs.Length < 1)
			{
				status = "tb scenario: requires an ID parameter";
				break;
			}
			BrawlType brawlType = BrawlType.BRAWL_TYPE_TAVERN_BRAWL;
			if (cmdArgs.Length > 1)
			{
				int brawlTypeInt = -1;
				if (int.TryParse(cmdArgs[1], out brawlTypeInt) && brawlTypeInt >= 1 && brawlTypeInt < 3)
				{
					brawlType = (BrawlType)brawlTypeInt;
				}
			}
			if (!int.TryParse(cmdArgs[0], out var scenarioId))
			{
				status = "tb scenario: invalid non-integer Scenario ID " + cmdArgs[0];
			}
			if (status == null)
			{
				TavernBrawlManager.Get().Cheat_SetScenario(scenarioId, brawlType);
				status = "tb scenario: set on client to ID: " + scenarioId + " for type: " + brawlType;
			}
			break;
		}
		}
		if (status != null)
		{
			UIStatus.Get().AddInfo(status, 5f);
		}
		return true;
	}

	private bool OnProcessCheat_randomizemercenariesboard(string func, string[] args, string rawArgs)
	{
		bool isFinalBoss = false;
		if (args.Length != 0 && !string.IsNullOrEmpty(args[0]))
		{
			isFinalBoss = Convert.ToBoolean(args[0]);
		}
		int seed = 0;
		if (args.Length > 1 && !string.IsNullOrEmpty(args[0]))
		{
			seed = Convert.ToInt32(args[1]);
		}
		if (Board.Get() is MercenariesBoard board)
		{
			board.RandomizeVisuals(isFinalBoss, allowLightingChanges: true, seed);
		}
		return true;
	}

	private bool OnProcessCheat_mercs(string func, string[] args, string rawArgs)
	{
		m_lastMercsServerCmd = args;
		MercenariesDebugCommandRequest request = new MercenariesDebugCommandRequest();
		request.Args.AddRange(args.ToArray());
		Network.Get().SendMercenariesDebugCommandRequest(request);
		return true;
	}

	private void OnProcessCheat_mercs_OnResponse()
	{
		MercenariesDebugCommandResponse response = Network.Get().MercenariesDebugCommandResponse();
		bool success = false;
		string reply = "null response";
		if (m_lastMercsServerCmd != null && m_lastMercsServerCmd.Length != 0)
		{
			_ = m_lastMercsServerCmd[0];
		}
		string[] cmdArgs = ((m_lastMercsServerCmd == null) ? new string[0] : m_lastMercsServerCmd.Skip(1).ToArray());
		if (cmdArgs.Length != 0)
		{
			_ = cmdArgs[0];
		}
		if (cmdArgs.Length >= 2)
		{
			cmdArgs[1].ToLower();
		}
		m_lastMercsServerCmd = null;
		if (response != null)
		{
			success = response.Success;
			reply = string.Format("{0} {1}", response.Success ? "" : "FAILED:", response.HasMessage ? response.Message : "reply=<blank>");
		}
		Blizzard.T5.Logging.LogLevel logLevel = (success ? Blizzard.T5.Logging.LogLevel.Info : Blizzard.T5.Logging.LogLevel.Error);
		Log.Net.Print(logLevel, reply);
		if (success)
		{
			float popupTextDuration = 5f;
			UIStatus.Get().AddInfo(reply, popupTextDuration);
		}
		else
		{
			UIStatus.Get().AddError(reply);
		}
	}

	private bool OnProcessCheat_utilservercmd(string func, string[] args, string rawArgs, AutofillData autofillData)
	{
		string[] allCmds = new string[21]
		{
			"help", "tb", "arena", "ranked", "deck", "freedeck", "banner", "quest", "legacyachieve", "prog",
			"setgsd", "returningplayer", "curl", "coin", "bgheroskin", "bgguideskin", "bgboardskin", "bgfinisher", "bgemote", "reward",
			"playerflag"
		};
		if (args.Length < 1 || string.IsNullOrEmpty(rawArgs))
		{
			if (autofillData != null)
			{
				return ProcessAutofillParam(allCmds, string.Empty, autofillData);
			}
			UIStatus.Get().AddError("Must specify a sub-command.");
			return true;
		}
		string[] argsOverride = OnProcessCheat_utilservercmd_OverwriteArgsForAliasing(args);
		string cmd = argsOverride[0].ToLower();
		string[] cmdArgs = argsOverride.Skip(1).ToArray();
		string subCmd = ((cmdArgs.Length == 0) ? null : cmdArgs[0].ToLower());
		if (autofillData != null && argsOverride.Length == 1 && !rawArgs.EndsWith(" "))
		{
			string prefix = cmd;
			return ProcessAutofillParam(allCmds, prefix, autofillData);
		}
		bool requiresConfirm = true;
		switch (cmd)
		{
		case "help":
			requiresConfirm = false;
			break;
		case "tb":
		{
			int subSubCmdIndex = 1;
			if (autofillData != null)
			{
				bool isEmpty = rawArgs.EndsWith(" ") && cmdArgs.Length == 0;
				if (cmd == "tb" && (isEmpty || cmdArgs.Length == 1))
				{
					return ProcessAutofillParam("view, list, season, scenario, end_offset, start_offset, active, dormant, ticket, reset_ticket, reset, wins, losses, reward".Split(new char[2] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries), subCmd, autofillData);
				}
				return false;
			}
			switch (subCmd)
			{
			case "help":
			case "view":
			case "list":
				requiresConfirm = false;
				break;
			case "reset":
				requiresConfirm = ((cmdArgs.Length < subSubCmdIndex + 1) ? null : cmdArgs[subSubCmdIndex].ToLower()) != "help";
				break;
			}
			break;
		}
		case "arena":
			requiresConfirm = false;
			subCmd = ((cmdArgs.Length < 2) ? null : cmdArgs[1].ToLower());
			if (autofillData != null)
			{
				if ((rawArgs.EndsWith(" ") && cmdArgs.Length == 0) || cmdArgs.Length == 1)
				{
					return ProcessAutofillParam("view_player, reward, ticket, set, view, list, season, scenario, end_offset, start_offset, active, dormant, choices".Split(new char[2] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries), subCmd, autofillData);
				}
				return false;
			}
			if (subCmd == "reward" && !cmdArgs.Any((string arg) => "justids".Equals(arg)))
			{
				List<string> additionalArgs = cmdArgs.ToList();
				additionalArgs.Add("justids");
				cmdArgs = additionalArgs.ToArray();
			}
			break;
		case "ranked":
			requiresConfirm = false;
			if (autofillData != null)
			{
				if ((rawArgs.EndsWith(" ") && cmdArgs.Length == 0) || cmdArgs.Length == 1)
				{
					return ProcessAutofillParam("view, season, set, reward, medal, win, lose, games, seasonroll".Split(new char[2] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries), subCmd, autofillData);
				}
				return false;
			}
			if (subCmd == "seasonroll" || subCmd == "settwist")
			{
				requiresConfirm = true;
			}
			break;
		case "deck":
			requiresConfirm = false;
			if (autofillData != null)
			{
				if ((rawArgs.EndsWith(" ") && cmdArgs.Length == 0) || cmdArgs.Length == 1)
				{
					return ProcessAutofillParam("view, test, grant, modify".Split(new char[2] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries), subCmd, autofillData);
				}
				return false;
			}
			if (subCmd == "view" && !cmdArgs.Any((string arg) => arg.StartsWith("details=", StringComparison.InvariantCultureIgnoreCase)))
			{
				List<string> newArgs = new List<string>(cmdArgs);
				newArgs.Add("details=0");
				cmdArgs = newArgs.ToArray();
			}
			break;
		case "freedeck":
			requiresConfirm = false;
			if (autofillData != null)
			{
				if ((rawArgs.EndsWith(" ") && cmdArgs.Length == 0) || cmdArgs.Length == 1)
				{
					return ProcessAutofillParam("start, end, claim, view, reset".Split(new char[2] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries), subCmd, autofillData);
				}
				return false;
			}
			break;
		case "banner":
			requiresConfirm = false;
			if (autofillData != null)
			{
				if ((rawArgs.EndsWith(" ") && cmdArgs.Length == 0) || cmdArgs.Length == 1)
				{
					return ProcessAutofillParam("list, reset".Split(new char[2] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries), subCmd, autofillData);
				}
				return false;
			}
			if (string.IsNullOrEmpty(subCmd) || subCmd == "help")
			{
				UIStatus.Get().AddInfo("Usage: util banner <list | reset bannerId=#>\n\nClear seen banners (wooden signs at login) with IDs >= bannerId arg. If no parameters, clears out just latest known bannerId. If bannerId=0, all seen banners are cleared.", 5f);
				return true;
			}
			if (subCmd == "list")
			{
				Cheat_ShowBannerList();
				return true;
			}
			break;
		case "prog":
		{
			bool autoFillResult = false;
			if (ProcessAutofillParam_util_prog(rawArgs, cmdArgs, autofillData, ref autoFillResult, ref requiresConfirm))
			{
				return autoFillResult;
			}
			break;
		}
		case "legacyachieve":
		{
			requiresConfirm = false;
			if (autofillData != null)
			{
				if ((rawArgs.EndsWith(" ") && cmdArgs.Length == 0) || cmdArgs.Length == 1)
				{
					return ProcessAutofillParam("cancel, resetdaily, resetreroll, grant, complete, progress".Split(new char[2] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries), subCmd, autofillData);
				}
				return false;
			}
			OnProcessCheat_util_achieves_ReplaceSlotWithAchieve(cmdArgs);
			int achieveId = OnProcessCheat_util_achieves_GetAchievementFromArgs(cmdArgs);
			if (subCmd == "grant")
			{
				Achievement achievement = AchieveManager.Get().GetAchievement(achieveId);
				if (achievement != null && AchieveManager.Get().GetActiveQuests().Count >= 3 && achievement.CanShowInQuestLog)
				{
					UIStatus.Get().AddInfo($"{func} {cmd}: Quest log is full.", 5f);
					return true;
				}
			}
			switch (subCmd)
			{
			case "cancel":
				OnProcessCheat_util_achieves_ShowQuestLog();
				break;
			case "grant":
			case "complete":
			case "progress":
				OnProcessCheat_util_achieves_ShowQuestPopupsWhenAchieveUpdated(achieveId);
				break;
			default:
				UIStatus.Get().AddInfo("USAGE: quest [subcmd] [subcmd args]\nCommands: grant, complete, progress, cancel, resetdaily\n Subcommands: achieve=[achieveId] (required for grant), slot=[slot#] (Either achieveId or slot required for complete, progress, cancel), amount=[X] (for progress only- optional), offset=[X] (in hours from current time, for resetdaily and resetreroll", 10f);
				return true;
			case "resetdaily":
			case "resetreroll":
				break;
			}
			break;
		}
		case "curl":
		case "grant":
		case "getgsd":
		case "setgsd":
			requiresConfirm = false;
			break;
		case "returningplayer":
			requiresConfirm = false;
			if (autofillData != null)
			{
				if ((rawArgs.EndsWith(" ") && cmdArgs.Length == 0) || cmdArgs.Length == 1)
				{
					return ProcessAutofillParam("start, complete, reset, view".Split(new char[2] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries), subCmd, autofillData);
				}
				return false;
			}
			break;
		case "hero":
			requiresConfirm = false;
			break;
		case "logrelay":
			if (string.IsNullOrEmpty(subCmd))
			{
				UIStatus.Get().AddInfo("USAGE: logrelay [logName]", 10f);
				return true;
			}
			requiresConfirm = false;
			break;
		case "coin":
			requiresConfirm = false;
			if (subCmd == "quickfavorite")
			{
				string coinId = argsOverride.FirstOrDefault((string x) => x.StartsWith("id="));
				int id = 1;
				if (coinId != null)
				{
					id = Convert.ToInt32(coinId.Substring("id=".Length));
				}
				CosmeticCoinManager.Get().RequestSetFavoriteCosmeticCoin(id, isFavorite: true);
				return true;
			}
			break;
		case "bgheroskin":
			requiresConfirm = false;
			if (autofillData != null)
			{
				if ((rawArgs.EndsWith(" ") && cmdArgs.Length == 0) || cmdArgs.Length == 1)
				{
					return ProcessAutofillParam(new string[13]
					{
						"help", "view", "favorite", "grant", "grantall", "clear", "remove", "grantall", "removeall", "setseen",
						"setallseen", "clearseen", "clearallseen"
					}, subCmd, autofillData);
				}
				return false;
			}
			break;
		case "bgguideskin":
			requiresConfirm = false;
			if (autofillData != null)
			{
				if ((rawArgs.EndsWith(" ") && cmdArgs.Length == 0) || cmdArgs.Length == 1)
				{
					return ProcessAutofillParam(new string[12]
					{
						"help", "view", "favorite", "grant", "clear", "remove", "grantall", "removeall", "setseen", "setallseen",
						"clearseen", "clearallseen"
					}, subCmd, autofillData);
				}
				return false;
			}
			break;
		case "bgboardskin":
			requiresConfirm = false;
			if (autofillData != null)
			{
				if ((rawArgs.EndsWith(" ") && cmdArgs.Length == 0) || cmdArgs.Length == 1)
				{
					return ProcessAutofillParam(new string[12]
					{
						"help", "view", "favorite", "grant", "clear", "remove", "grantall", "removeall", "setseen", "setallseen",
						"clearseen", "clearallseen"
					}, subCmd, autofillData);
				}
				return false;
			}
			break;
		case "bgfinisher":
			requiresConfirm = false;
			if (autofillData != null)
			{
				if ((rawArgs.EndsWith(" ") && cmdArgs.Length == 0) || cmdArgs.Length == 1)
				{
					return ProcessAutofillParam(new string[12]
					{
						"help", "view", "favorite", "grant", "clear", "remove", "grantall", "removeall", "setseen", "clearseen",
						"setallseen", "clearallseen"
					}, subCmd, autofillData);
				}
				return false;
			}
			break;
		case "bgemote":
			requiresConfirm = false;
			if (autofillData != null && ((rawArgs.EndsWith(" ") && cmdArgs.Length == 0) || cmdArgs.Length == 1))
			{
				return ProcessAutofillParam(new string[13]
				{
					"help", "view", "grant", "remove", "grantall", "removeall", "setseen", "clearseen", "setallseen", "clearallseen",
					"loadout", "setloadoutslot", "clearloadoutslot"
				}, subCmd, autofillData);
			}
			break;
		case "reward":
			requiresConfirm = false;
			if (autofillData != null)
			{
				if ((rawArgs.EndsWith(" ") && cmdArgs.Length == 0) || cmdArgs.Length == 1)
				{
					return ProcessAutofillParam(new string[12]
					{
						"grantlist", "grantitem", "gold", "dust", "orbs", "booster", "card", "randomcard", "tavernticket", "cardback",
						"heroskin", "customcoin"
					}, subCmd, autofillData);
				}
				return false;
			}
			break;
		case "playerflag":
			requiresConfirm = false;
			if (autofillData != null)
			{
				if ((rawArgs.EndsWith(" ") && cmdArgs.Length == 0) || cmdArgs.Length == 1)
				{
					return ProcessAutofillParam(new string[2] { "check", "set" }, subCmd, autofillData);
				}
				return false;
			}
			break;
		}
		if (autofillData != null)
		{
			return false;
		}
		AlertPopup.ResponseCallback cb = delegate(AlertPopup.Response response, object userData)
		{
			if (response == AlertPopup.Response.CONFIRM || response == AlertPopup.Response.OK)
			{
				DebugCommandRequest debugCommandRequest = new DebugCommandRequest();
				debugCommandRequest.Command = cmd;
				debugCommandRequest.Args.AddRange(cmdArgs);
				Network.Get().SendDebugCommandRequest(debugCommandRequest);
			}
		};
		m_lastUtilServerCmd = argsOverride;
		if (requiresConfirm)
		{
			AlertPopup.PopupInfo popup = new AlertPopup.PopupInfo();
			popup.m_headerText = "Run UTIL server command?";
			popup.m_text = "You are about to run a UTIL Server command - this may affect other players on this environment and possibly change configuration on this environment.\n\nPlease confirm you want to do this.";
			popup.m_responseDisplay = AlertPopup.ResponseDisplay.CONFIRM_CANCEL;
			popup.m_responseCallback = cb;
			DialogManager.Get().ShowPopup(popup);
		}
		else
		{
			cb(AlertPopup.Response.OK, null);
		}
		return true;
	}

	private string[] OnProcessCheat_utilservercmd_OverwriteArgsForAliasing(string[] args)
	{
		string text = args[0]?.ToLower();
		if (text == "quest" || text == "achieve")
		{
			string[] progArgs = new string[1 + args.Length];
			progArgs[0] = "prog";
			args.CopyTo(progArgs, 1);
			return progArgs;
		}
		return args;
	}

	private void OnProcessCheat_utilservercmd_OnResponse()
	{
		DebugCommandResponse res = Network.Get().GetDebugCommandResponse();
		bool success = false;
		string reply = "null response";
		string cmd = ((m_lastUtilServerCmd == null || m_lastUtilServerCmd.Length == 0) ? "" : m_lastUtilServerCmd[0]);
		string[] cmdArgs = ((m_lastUtilServerCmd == null) ? new string[0] : m_lastUtilServerCmd.Skip(1).ToArray());
		string subCmd = ((cmdArgs.Length == 0) ? null : cmdArgs[0]);
		string subSubCmd = ((cmdArgs.Length < 2) ? null : cmdArgs[1].ToLower());
		m_lastUtilServerCmd = null;
		if (res != null)
		{
			success = res.Success;
			reply = string.Format("{0} {1}", res.Success ? "" : "FAILED:", res.HasResponse ? res.Response : "reply=<blank>");
		}
		Blizzard.T5.Logging.LogLevel logLevel = (success ? Blizzard.T5.Logging.LogLevel.Info : Blizzard.T5.Logging.LogLevel.Error);
		Log.Net.Print(logLevel, reply);
		bool showPopupText = true;
		float popupTextDuration = 5f;
		if (success)
		{
			switch (cmd)
			{
			case "tb":
				switch (subCmd)
				{
				case "reset":
					if (!(subSubCmd != "help"))
					{
						break;
					}
					goto case "scenario";
				case "scenario":
				case "scen":
				case "season":
				case "end_offset":
				case "start_offset":
				case "wins":
				case "losses":
				case "ticket":
				{
					for (BrawlType brawlType = BrawlType.BRAWL_TYPE_TAVERN_BRAWL; brawlType < BrawlType.BRAWL_TYPE_COUNT; brawlType++)
					{
						if (brawlType != BrawlType.BRAWL_TYPE_FIRESIDE_GATHERING)
						{
							TavernBrawlManager.Get().RefreshServerData(brawlType);
						}
					}
					break;
				}
				}
				break;
			case "ranked":
				if (subCmd == "medal" || subCmd == "seasonroll")
				{
					success = success && (!res.HasResponse || !res.Response.StartsWith("Error"));
					if (success)
					{
						reply = "Success";
						popupTextDuration = 0.5f;
					}
					else if (res.HasResponse)
					{
						reply = res.Response;
					}
				}
				switch (subCmd)
				{
				case "set":
				case "win":
				case "lose":
				case "games":
					NetCache.Get().RefreshNetObject<NetCache.NetCacheMedalInfo>();
					break;
				}
				break;
			case "hero":
				if (subCmd == "addxp")
				{
					NetCache.Get().RefreshNetObject<NetCache.NetCacheHeroLevels>();
				}
				break;
			case "banner":
			{
				if (!(subCmd == "reset"))
				{
					break;
				}
				NetCache.Get().ReloadNetObject<NetCache.NetCacheProfileProgress>();
				bool specifiedBannerId = false;
				int bannerId = 0;
				string[] array = cmdArgs;
				for (int i = 0; i < array.Length; i++)
				{
					string[] parts2 = array[i]?.Split('=');
					if (parts2 != null && parts2.Length >= 2 && (parts2[0].Equals("banner", StringComparison.InvariantCultureIgnoreCase) || parts2[0].Equals("bannerId", StringComparison.InvariantCultureIgnoreCase)))
					{
						specifiedBannerId = true;
						int.TryParse(parts2[1], out bannerId);
					}
				}
				if (specifiedBannerId)
				{
					BannerManager.Get().Cheat_ClearSeenBannersNewerThan(bannerId);
				}
				else
				{
					BannerManager.Get().Cheat_ClearSeenBanners();
				}
				break;
			}
			case "logrelay":
				if (subCmd == "*")
				{
					showPopupText = false;
				}
				break;
			case "prog":
				switch (subCmd)
				{
				case "achieve":
				case "quest":
				case "task":
				{
					if (!(subSubCmd == "listen"))
					{
						break;
					}
					if (cmdArgs.Length < 3)
					{
						return;
					}
					string subSubSubCmd = cmdArgs[2].ToLower();
					LuaLogs luaLogService = ServiceManager.Get<LuaLogs>();
					if (luaLogService == null)
					{
						return;
					}
					int playerId = (int)SceneDebugger.Get().GetPlayerId_DebugOnly().GetValueOrDefault();
					if (subSubSubCmd == "all")
					{
						luaLogService.ClearListenOnGameServer(playerId);
						return;
					}
					int scriptId = 0;
					string[] array = cmdArgs;
					for (int i = 0; i < array.Length; i++)
					{
						string[] parts = array[i]?.Split('=');
						if (parts != null && parts.Length >= 2 && parts[0].Equals("id", StringComparison.InvariantCultureIgnoreCase))
						{
							int.TryParse(parts[1], out scriptId);
						}
					}
					try
					{
						LuaLogs.ListenableScriptType scriptType = EnumUtils.GetEnum<LuaLogs.ListenableScriptType>(subCmd.ToUpper());
						luaLogService.ListenOnGameServer(playerId, scriptId, scriptType);
					}
					catch (ArgumentException arg)
					{
						Error.AddWarning("Prog listen Cheat Error", $"Type is not configured to be listenable {subCmd.ToUpper()}. Error Message: {arg}");
					}
					break;
				}
				}
				break;
			case "bgemote":
				NetCache.Get().RefreshNetObject<NetCache.NetCacheBattlegroundsEmotes>();
				break;
			case "bgheroskin":
			case "bgguideskin":
			case "bgboardskin":
			case "bgfinisher":
				if (subCmd != null && subCmd.Contains("seen"))
				{
					switch (cmd)
					{
					case "bgheroskin":
						NetCache.Get().RefreshNetObject<NetCache.NetCacheBattlegroundsHeroSkins>();
						break;
					case "bgguideskin":
						NetCache.Get().RefreshNetObject<NetCache.NetCacheBattlegroundsGuideSkins>();
						break;
					case "bgboardskin":
						NetCache.Get().RefreshNetObject<NetCache.NetCacheBattlegroundsBoardSkins>();
						break;
					case "bgfinisher":
						NetCache.Get().RefreshNetObject<NetCache.NetCacheBattlegroundsFinishers>();
						break;
					}
				}
				break;
			}
			if ((cmd == "ranked" || cmd == "arena") && (subCmd == "reward" || subCmd == "rewards"))
			{
				success = success && (!res.HasResponse || !res.Response.StartsWith("Error"));
				if (success)
				{
					reply = Cheat_ShowRewardBoxes(reply);
					if (reply == null)
					{
						popupTextDuration = 0.5f;
						reply = "Success";
					}
					else
					{
						success = false;
					}
				}
			}
			if (cmd == "arena" && subCmd == "season")
			{
				DraftManager.Get().RefreshCurrentSeasonFromServer();
			}
		}
		if (showPopupText)
		{
			if (success)
			{
				UIStatus.Get().AddInfo(reply, popupTextDuration);
			}
			else
			{
				UIStatus.Get().AddError(reply);
			}
		}
	}

	private int OnProcessCheat_util_achieves_GetAchievementFromArgs(string[] args)
	{
		string achieve = args.FirstOrDefault((string x) => x.StartsWith("achieve="));
		if (achieve != null)
		{
			return Convert.ToInt32(achieve.Substring("achieve=".Length));
		}
		return 0;
	}

	private int OnProcessCheat_util_achieves_GetAchieveFromSlotId(int slotId)
	{
		List<Achievement> activeQuests = AchieveManager.Get().GetActiveQuests();
		if (slotId > 0 && slotId <= activeQuests.Count)
		{
			return activeQuests[slotId - 1].ID;
		}
		return 0;
	}

	private void OnProcessCheat_util_achieves_ReplaceSlotWithAchieve(string[] args)
	{
		for (int i = 0; i < args.Length; i++)
		{
			if (args[i].StartsWith("slot=", ignoreCase: true, CultureInfo.CurrentCulture))
			{
				int slotId = Convert.ToInt32(args[i].Substring("slot=".Length));
				int achieveId = OnProcessCheat_util_achieves_GetAchieveFromSlotId(slotId);
				args[i] = $"achieve={achieveId}";
			}
		}
	}

	private void OnProcessCheat_util_achieves_ShowQuestPopupsWhenAchieveUpdated(int achieveId)
	{
		AchieveManager.AchievesUpdatedCallback action = null;
		AchieveManager.Get().RegisterAchievesUpdatedListener(action = delegate(List<Achievement> updatedAchieves, List<Achievement> completedAchieves, object userdata)
		{
			if (achieveId == 0 || updatedAchieves.Any((Achievement x) => x.ID == achieveId) || completedAchieves.Any((Achievement x) => x.ID == achieveId))
			{
				if (AchieveManager.Get().HasQuestsToShow(onlyNewlyActive: true))
				{
					WelcomeQuests.Show(UserAttentionBlocker.ALL, fromLogin: true);
				}
				else if (GameToastMgr.Get() != null)
				{
					GameToastMgr.Get().UpdateQuestProgressToasts();
				}
				AchieveManager.Get().RemoveAchievesUpdatedListener(action);
			}
		});
	}

	private void OnProcessCheat_util_achieves_ShowQuestLog()
	{
		if (QuestLog.Get() != null && !QuestLog.Get().IsShown())
		{
			QuestLog.Get().Show();
		}
	}

	private bool ProcessAutofillParam_util_prog(string rawArgs, string[] cmdArgs, AutofillData autofillData, ref bool autoFillResult, ref bool requiresConfirm)
	{
		requiresConfirm = false;
		if (autofillData == null)
		{
			return false;
		}
		if (cmdArgs.Length > 2)
		{
			return false;
		}
		string subCmd = ((cmdArgs.Length < 1) ? null : cmdArgs[0].ToLower());
		string subSubCmd = ((cmdArgs.Length < 2) ? null : cmdArgs[1].ToLower());
		bool endsWithSpace = rawArgs.EndsWith(" ");
		if ((subCmd == null && endsWithSpace) || (subCmd != null && subSubCmd == null && !endsWithSpace))
		{
			string[] subCommands = new string[5] { "quest", "pool", "achieve", "track", "task" };
			autoFillResult = ProcessAutofillParam(subCommands, subCmd, autofillData);
			return true;
		}
		if (subCmd == null)
		{
			return false;
		}
		if (((subSubCmd == null && endsWithSpace) || (subSubCmd != null && !endsWithSpace)) && subCmd == "quest")
		{
			string[] subSubCommands = new string[8] { "help", "view", "grant", "ack", "advance", "complete", "reset", "listen" };
			autoFillResult = ProcessAutofillParam(subSubCommands, subSubCmd, autofillData);
			return true;
		}
		if (((subSubCmd == null && endsWithSpace) || (subSubCmd != null && !endsWithSpace)) && subCmd == "pool")
		{
			string[] subSubCommands2 = new string[11]
			{
				"help", "view", "grant", "login", "lastcheckdate", "lastgrantdate", "reroll", "reset", "set", "testcalcnumquests",
				"testcalctimeuntil"
			};
			autoFillResult = ProcessAutofillParam(subSubCommands2, subSubCmd, autofillData);
			return true;
		}
		if (((subSubCmd == null && endsWithSpace) || (subSubCmd != null && !endsWithSpace)) && subCmd == "achieve")
		{
			string[] subSubCommands3 = new string[9] { "help", "view", "score", "advance", "complete", "claim", "ack", "reset", "listen" };
			autoFillResult = ProcessAutofillParam(subSubCommands3, subSubCmd, autofillData);
			return true;
		}
		if (((subSubCmd == null && endsWithSpace) || (subSubCmd != null && !endsWithSpace)) && subCmd == "track")
		{
			string[] subSubCommands4 = new string[9] { "help", "view", "set", "gamexp", "addxp", "levelup", "claim", "ack", "reset" };
			autoFillResult = ProcessAutofillParam(subSubCommands4, subSubCmd, autofillData);
			return true;
		}
		return false;
	}

	private static string Cheat_ShowRewardBoxes(string parsableRewardBags)
	{
		if (SceneMgr.Get().IsInGame())
		{
			return "Cannot display reward boxes in gameplay.";
		}
		string[] parts = parsableRewardBags.Trim().Split(new char[1] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
		if (parts.Length < 2)
		{
			return "Error parsing reply, should start with 'Success:' then player_id: " + parsableRewardBags;
		}
		if (parts.Length < 3)
		{
			return "No rewards returned by server: reply=" + parsableRewardBags;
		}
		List<NetCache.ProfileNotice> simulatedNotices = new List<NetCache.ProfileNotice>();
		parts = parts.Skip(1).ToArray();
		for (int nthReward = 0; nthReward < parts.Length; nthReward++)
		{
			int rewardBagType = 0;
			int index = nthReward * 2;
			if (index >= parts.Length)
			{
				break;
			}
			if (!int.TryParse(parts[index], out rewardBagType))
			{
				return "Reward at index " + index + " (" + parts[index] + ") is not an int: reply=" + parsableRewardBags;
			}
			if (rewardBagType != 0)
			{
				index++;
				if (index >= parts.Length)
				{
					return "No reward bag data at index " + index + ": reply=" + parsableRewardBags;
				}
				long rewardBagData = 0L;
				if (!long.TryParse(parts[index], out rewardBagData))
				{
					return "Reward Data at index " + index + " (" + parts[index] + ") is not a long int: reply=" + parsableRewardBags;
				}
				NetCache.ProfileNotice notice = null;
				TAG_PREMIUM premium = TAG_PREMIUM.NORMAL;
				switch (rewardBagType)
				{
				case 1:
				case 12:
				case 14:
				case 15:
				case 24:
					notice = new NetCache.ProfileNoticeRewardBooster
					{
						Id = (int)rewardBagData,
						Count = 1
					};
					break;
				case 2:
					notice = new NetCache.ProfileNoticeRewardCurrency
					{
						CurrencyType = PegasusShared.CurrencyType.CURRENCY_TYPE_GOLD,
						Amount = (int)rewardBagData
					};
					break;
				case 3:
					notice = new NetCache.ProfileNoticeRewardDust
					{
						Amount = (int)rewardBagData
					};
					break;
				case 8:
				case 9:
				case 10:
				case 11:
					premium = TAG_PREMIUM.GOLDEN;
					goto case 4;
				case 4:
				case 5:
				case 6:
				case 7:
					notice = new NetCache.ProfileNoticeRewardCard
					{
						CardID = GameUtils.TranslateDbIdToCardId((int)rewardBagData),
						Premium = premium
					};
					break;
				case 13:
					notice = new NetCache.ProfileNoticeRewardCardBack
					{
						CardBackID = (int)rewardBagData
					};
					break;
				default:
					Debug.LogError("Unknown Reward Bag Type: " + rewardBagType + " (data=" + rewardBagData + ") at index " + index + ": reply=" + parsableRewardBags);
					break;
				}
				if (notice != null)
				{
					simulatedNotices.Add(notice);
				}
			}
		}
		RewardBoxesDisplay existingDisplay = UnityEngine.Object.FindObjectOfType<RewardBoxesDisplay>();
		if (existingDisplay != null)
		{
			float timeToWait = 0f;
			if (existingDisplay.IsClosing)
			{
				timeToWait = 0.1f;
			}
			else
			{
				existingDisplay.Close();
			}
			Processor.ScheduleCallback(timeToWait, realTime: false, delegate
			{
				Cheat_ShowRewardBoxes(parsableRewardBags);
			});
			return null;
		}
		List<RewardData> rewardList = RewardUtils.GetRewards(simulatedNotices);
		PrefabCallback<GameObject> cbRewardBoxesPrefabLoaded = delegate(AssetReference assetRef, GameObject go, object callbackData)
		{
			RewardBoxesDisplay component = go.GetComponent<RewardBoxesDisplay>();
			component.SetRewards(callbackData as List<RewardData>);
			component.m_Root.transform.position = (UniversalInputManager.UsePhoneUI ? new Vector3(0f, 14.7f, 3f) : new Vector3(0f, 131.2f, -3.2f));
			if (Box.Get() != null && Box.Get().GetBoxCamera() != null && Box.Get().GetBoxCamera().GetState() == BoxCamera.State.OPENED)
			{
				component.m_Root.transform.position += new Vector3(-3f, 0f, 4.6f);
				if ((bool)UniversalInputManager.UsePhoneUI)
				{
					component.m_Root.transform.position += new Vector3(0f, 0f, -7f);
				}
				else
				{
					component.transform.localScale = Vector3.one * 0.6f;
				}
			}
			component.AnimateRewards();
		};
		AssetLoader.Get().InstantiatePrefab(RewardBoxesDisplay.GetPrefab(rewardList), cbRewardBoxesPrefabLoaded, rewardList);
		return null;
	}

	private bool OnProcessCheat_gameservercmd(string func, string[] args, string rawArgs)
	{
		return true;
	}

	private bool OnProcessCheat_rewardboxes(string func, string[] args, string rawArgs)
	{
		string.IsNullOrEmpty(args[0].ToLower());
		int numRewards = 5;
		if (args.Length > 1)
		{
			GeneralUtils.TryParseInt(args[1], out numRewards);
		}
		BoosterDbId boosterDbId = BoosterDbId.THE_GRAND_TOURNAMENT;
		BoosterDbId[] boosters = (from BoosterDbId i in Enum.GetValues(typeof(BoosterDbId))
			where i != BoosterDbId.INVALID
			select i).ToArray();
		boosterDbId = boosters[UnityEngine.Random.Range(0, boosters.Length)];
		string text = string.Concat(string.Concat("Success: 123456" + " " + 13, " ", UnityEngine.Random.Range(1, 34).ToString()), " ", 1.ToString());
		int num = (int)boosterDbId;
		string reply = Cheat_ShowRewardBoxes(string.Concat(string.Concat(string.Concat(string.Concat(string.Concat(string.Concat(text + " " + num, " ", 3.ToString()), " ", (UnityEngine.Random.Range(1, 31) * 5).ToString()), " ", 2.ToString()), " ", (UnityEngine.Random.Range(1, 31) * 5).ToString()), " ", ((UnityEngine.Random.Range(0, 2) == 0) ? 6 : 10).ToString()), " ", GameUtils.TranslateCardIdToDbId("EX1_279").ToString()));
		if (reply != null)
		{
			UIStatus.Get().AddError(reply);
		}
		return true;
	}

	private bool OnProcessCheat_rankrefresh(string func, string[] args, string rawArgs)
	{
		NetCache.Get().RegisterScreenEndOfGame(OnNetCacheReady_CallRankChangeTwoScoopDebugShow);
		return true;
	}

	private void OnNetCacheReady_CallRankChangeTwoScoopDebugShow()
	{
		NetCache.Get().UnregisterNetCacheHandler(OnNetCacheReady_CallRankChangeTwoScoopDebugShow);
		RankChangeTwoScoop_NEW.DebugShowHelper(RankMgr.Get().GetLocalPlayerMedalInfo(), Options.GetFormatType());
	}

	private bool OnProcessCheat_rankchange(string func, string[] args, string rawArgs)
	{
		string rankCheatName = "bronze10";
		if (args.Length != 0 && !string.IsNullOrEmpty(args[0]))
		{
			rankCheatName = args[0];
		}
		LeagueRankDbfRecord rankRecord = RankMgr.Get().GetLeagueRankRecordByCheatName(rankCheatName);
		if (rankRecord == null)
		{
			return false;
		}
		FormatType formatType = FormatType.FT_STANDARD;
		bool isWinStreak = false;
		int currStars = 0;
		int starsPerWin = 1;
		bool showWin = true;
		for (int i = 0; i < args.Length; i++)
		{
			string lowerArg = args[i].ToLower();
			switch (lowerArg)
			{
			case "winstreak":
			case "streak":
				isWinStreak = true;
				continue;
			case "win":
				showWin = true;
				continue;
			case "loss":
				showWin = false;
				continue;
			case "wild":
				formatType = FormatType.FT_WILD;
				continue;
			case "classic":
				formatType = FormatType.FT_CLASSIC;
				continue;
			}
			if (lowerArg.StartsWith("x") || lowerArg.EndsWith("x"))
			{
				lowerArg = lowerArg.Trim('x');
				starsPerWin = int.Parse(lowerArg);
			}
			else if (lowerArg.StartsWith("*") || lowerArg.EndsWith("*"))
			{
				lowerArg = lowerArg.Trim('*');
				currStars = int.Parse(lowerArg);
			}
		}
		RankChangeTwoScoop_NEW.DebugShowFake(rankRecord.LeagueId, rankRecord.StarLevel, currStars, starsPerWin, formatType, isWinStreak, showWin);
		return true;
	}

	private bool OnProcessCheat_rankreward(string func, string[] args, string rawArgs)
	{
		string rankCheatName = "bronze5";
		if (args.Length != 0 && !string.IsNullOrEmpty(args[0]))
		{
			rankCheatName = args[0];
		}
		LeagueRankDbfRecord rankRecord = RankMgr.Get().GetLeagueRankRecordByCheatName(rankCheatName);
		if (rankRecord == null)
		{
			return false;
		}
		FormatType formatType = FormatType.FT_STANDARD;
		bool includeAllRewards = false;
		for (int i = 0; i < args.Length; i++)
		{
			switch (args[i].ToLower())
			{
			case "standard":
				formatType = FormatType.FT_STANDARD;
				break;
			case "wild":
				formatType = FormatType.FT_WILD;
				break;
			case "classic":
				formatType = FormatType.FT_CLASSIC;
				break;
			case "all":
				includeAllRewards = true;
				break;
			}
		}
		MedalInfoTranslator medalInfoTranslator = MedalInfoTranslator.CreateMedalInfoForLeagueId(rankRecord.LeagueId, rankRecord.StarLevel, 1337);
		medalInfoTranslator.GetPreviousMedal(formatType).starLevel = (includeAllRewards ? 1 : (rankRecord.StarLevel - 1));
		TranslatedMedalInfo currMedal = medalInfoTranslator.GetCurrentMedal(formatType);
		currMedal.bestStarLevel = rankRecord.StarLevel;
		NetCache.NetCacheRewardProgress rewardProgress = NetCache.Get().GetNetObject<NetCache.NetCacheRewardProgress>();
		if (rewardProgress != null)
		{
			currMedal.seasonId = rewardProgress.Season;
		}
		List<List<RewardData>> rankedRewards = new List<List<RewardData>>();
		if (!medalInfoTranslator.GetRankedRewardsEarned(formatType, ref rankedRewards) || rankedRewards.Count == 0)
		{
			return false;
		}
		RankedRewardDisplay.DebugShowFake(rankRecord.LeagueId, rankRecord.StarLevel, formatType, rankedRewards);
		return true;
	}

	private bool OnProcessCheat_rankcardback(string func, string[] args, string rawArgs)
	{
		string rankCheatName = "bronze10";
		LeagueRankDbfRecord rankRecord = RankMgr.Get().GetLeagueRankRecordByCheatName(rankCheatName);
		if (rankRecord == null)
		{
			return false;
		}
		int wins = 0;
		if (args.Length != 0 && !string.IsNullOrEmpty(args[0]) && !GeneralUtils.TryParseInt(args[0], out wins))
		{
			UIStatus.Get().AddInfo("please enter a valid int value for 1st parameter <wins>");
			return true;
		}
		int seasonId = 0;
		if (args.Length >= 2 && !GeneralUtils.TryParseInt(args[1], out seasonId))
		{
			UIStatus.Get().AddInfo("please enter a valid int value for 2nd parameter <season_id>");
			return true;
		}
		if (seasonId == 0)
		{
			NetCache.NetCacheRewardProgress rewardProgress = NetCache.Get().GetNetObject<NetCache.NetCacheRewardProgress>();
			if (rewardProgress != null)
			{
				seasonId = rewardProgress.Season;
			}
		}
		MedalInfoTranslator medalInfoTranslator = MedalInfoTranslator.CreateMedalInfoForLeagueId(rankRecord.LeagueId, rankRecord.StarLevel, 1337);
		TranslatedMedalInfo prevMedal = medalInfoTranslator.GetPreviousMedal(FormatType.FT_STANDARD);
		TranslatedMedalInfo currentMedal = medalInfoTranslator.GetCurrentMedal(FormatType.FT_STANDARD);
		prevMedal.seasonWins = Mathf.Max(0, wins - 1);
		currentMedal.seasonWins = wins;
		currentMedal.seasonId = seasonId;
		RankedCardBackProgressDisplay.DebugShowFake(medalInfoTranslator);
		return true;
	}

	private bool OnProcessCheat_easyrank(string func, string[] args, string rawArgs)
	{
		string rankChangeArg = args[0].ToLower();
		CheatMgr.Get().ProcessCheat($"util ranked set rank={rankChangeArg}");
		return true;
	}

	private bool OnProcessCheat_localmedaloverride(string func, string[] args, string rawArgs)
	{
		string cheatName = "localmedaloverride";
		if (args.Length < 1 || string.IsNullOrEmpty(args[0]))
		{
			UIStatus.Get().AddError("expected use: " + cheatName + " [ft_standard|ft_wild|ft_classic] star_level=# legend_rank=# OR off");
			return true;
		}
		if (args[0].ToLower() == "off")
		{
			NetCache.NetCacheMedalInfo.CheatLocalOverrideClear();
			return true;
		}
		string formatTypeArg = args[0].ToUpper();
		if (!Enum.TryParse<FormatType>(formatTypeArg, out var formatType))
		{
			UIStatus.Get().AddError(cheatName + " error: Unknown FormatType '" + formatTypeArg + "'");
			return true;
		}
		if (formatType == FormatType.FT_UNKNOWN)
		{
			UIStatus.Get().AddError(cheatName + " error: Cannot use FormatType 'FT_UNKNOWN'");
			return true;
		}
		string[] argNames = new string[2] { "star_level", "legend_rank" };
		TryParseNamedArgs(args, out var namedArgs);
		foreach (string argName in argNames)
		{
			if (!namedArgs.TryGetValue(argName, out var value))
			{
				Map<string, NamedParam> map = namedArgs;
				value = default(NamedParam);
				map.Add(argName, value);
			}
		}
		NetCache.NetCacheMedalInfo medalInfo = NetCache.Get().GetNetObject<NetCache.NetCacheMedalInfo>();
		NamedParam legendRankParam = namedArgs["legend_rank"];
		int legendRank = 0;
		if (legendRankParam.HasNumber)
		{
			legendRank = legendRankParam.Number;
			medalInfo.CheatLocalOverrideLegendRank(formatType, legendRank);
		}
		NamedParam starLevelParam = namedArgs["star_level"];
		int starLevel = 0;
		if (starLevelParam.HasNumber || legendRankParam.HasNumber)
		{
			starLevel = (starLevelParam.HasNumber ? starLevelParam.Number : 51);
			medalInfo.CheatLocalOverrideStarLevel(formatType, starLevel);
		}
		if (legendRankParam.HasNumber)
		{
			UIStatus.Get().AddInfo($"Setting local medal {formatType} to star_level={starLevel} and legend_rank={legendRank}");
		}
		else if (starLevelParam.HasNumber)
		{
			UIStatus.Get().AddInfo($"Setting local medal {formatType} to star_level={starLevel}");
		}
		OnProcessCheat_rankrefresh(func, args, rawArgs);
		return true;
	}

	private bool OnProcessCheat_timescale(string func, string[] args, string rawArgs)
	{
		string timeScaleArg = args[0].ToLower();
		if (string.IsNullOrEmpty(timeScaleArg))
		{
			float effectiveTimescale = Time.timeScale;
			float devTimescale = SceneDebugger.GetDevTimescaleMultiplier();
			string msg = ((effectiveTimescale != devTimescale) ? $"Current timeScale is: {effectiveTimescale}\nDev timescale: {devTimescale}\nGame timescale: {TimeScaleMgr.Get().GetGameTimeScale()}" : $"Current timeScale is: {effectiveTimescale}");
			UIStatus.Get().AddInfo(msg, 3f * SceneDebugger.GetDevTimescaleMultiplier());
			return true;
		}
		float timeScale = 1f;
		if (!float.TryParse(timeScaleArg, out timeScale))
		{
			return false;
		}
		SceneDebugger.SetDevTimescaleMultiplier(timeScale);
		UIStatus.Get().AddInfo($"Setting timescale to: {timeScale}", 3f * timeScale);
		return true;
	}

	private bool OnProcessCheat_reset(string func, string[] args, string rawArgs)
	{
		HearthstoneApplication.Get().Reset();
		return true;
	}

	private bool OnProcessCheat_endturn(string func, string[] args, string rawArgs)
	{
		UIStatus.Get().AddError("Deprecated. Use \"cheat endturn\" instead.");
		return true;
	}

	private bool OnProcessCheat_battlegrounds(string func, string[] args, string rawArgs)
	{
		if (SceneMgr.Get().IsInGame())
		{
			UIStatus.Get().AddError("Cannot queue for a battlegrounds game while in gameplay.");
			return true;
		}
		if (DialogManager.Get().ShowingDialog())
		{
			UIStatus.Get().AddError("Cannot queue for a battlegrounds game while a dialog is active.");
			return true;
		}
		GameMgr.Get().FindGame(GameType.GT_BATTLEGROUNDS, FormatType.FT_WILD, 3459, 0, 0L, null, null, restoreSavedGameState: false, null, null, 0L);
		return true;
	}

	private bool OnProcessCheat_battlegroundsDuo(string func, string[] args, string rawArgs)
	{
		if (SceneMgr.Get().IsInGame())
		{
			UIStatus.Get().AddError("Cannot queue for a battlegrounds game while in gameplay.");
			return true;
		}
		if (DialogManager.Get().ShowingDialog())
		{
			UIStatus.Get().AddError("Cannot queue for a battlegrounds game while a dialog is active.");
			return true;
		}
		GameMgr.Get().FindGame(GameType.GT_BATTLEGROUNDS_DUO, FormatType.FT_WILD, 5173, 0, 0L, null, null, restoreSavedGameState: false, null, null, 0L);
		return true;
	}

	private bool OnProcessCheat_scenario(string func, string[] args, string rawArgs)
	{
		string[] argNames = new string[5] { "id", "game_type", "deck_id", "format_type", "prog_override" };
		Map<string, NamedParam> namedArgs;
		bool hasNamedParams = TryParseNamedArgs(args, out namedArgs);
		for (int i = 0; i < argNames.Length; i++)
		{
			string argName = argNames[i];
			if (!namedArgs.TryGetValue(argName, out var value))
			{
				if (!hasNamedParams && args.Length > i)
				{
					namedArgs.Add(argName, new NamedParam(args[i]));
					continue;
				}
				Map<string, NamedParam> map = namedArgs;
				value = default(NamedParam);
				map.Add(argName, value);
			}
		}
		NamedParam idParam = namedArgs["id"];
		int scenarioId = 260;
		if (idParam.HasNumber)
		{
			scenarioId = idParam.Number;
			if (GameDbf.Scenario.GetRecord(scenarioId) == null)
			{
				Error.AddWarning("scenario Cheat Error", "Error reading a scenario id from \"{0}\"", scenarioId);
				return false;
			}
		}
		NamedParam gtParam = namedArgs["game_type"];
		GameType gameType = GameType.GT_VS_AI;
		if (gtParam.HasNumber)
		{
			gameType = (GameType)gtParam.Number;
			if (gameType == GameType.GT_UNKNOWN)
			{
				Error.AddWarning("scenario Cheat Error", "Error reading a game type from \"{0}\"", gameType);
				return false;
			}
		}
		else if (s_scenarioToGameTypeMap.ContainsKey((ScenarioDbId)scenarioId))
		{
			gameType = s_scenarioToGameTypeMap[(ScenarioDbId)scenarioId];
		}
		NamedParam deckParam = namedArgs["deck_id"];
		CollectionDeck deck = null;
		if (deckParam.HasNumber)
		{
			deck = CollectionManager.Get().GetDeck(deckParam.Number);
		}
		if (deckParam.HasNumber && deck == null)
		{
			deck = (from x in CollectionManager.Get().GetDecks()
				where x.Value.Name.Equals(deckParam.Text, StringComparison.CurrentCultureIgnoreCase)
				select x).FirstOrDefault().Value;
			if (deck == null)
			{
				Error.AddWarning("scenario Cheat Error", "Error reading a deck id from \"{0}\"", deck);
				return false;
			}
		}
		NamedParam ftParam = namedArgs["format_type"];
		FormatType formatType = FormatType.FT_WILD;
		if (ftParam.HasNumber)
		{
			formatType = (FormatType)ftParam.Number;
			if (formatType == FormatType.FT_UNKNOWN)
			{
				Error.AddWarning("scenario Cheat Error", "Error reading a format type from \"{0}\"", formatType);
				return false;
			}
		}
		NamedParam progOverrideParam = namedArgs["prog_override"];
		GameType progFilterOverride = GameType.GT_UNKNOWN;
		if (progOverrideParam.HasNumber)
		{
			progFilterOverride = (GameType)progOverrideParam.Number;
			if (progFilterOverride == GameType.GT_UNKNOWN)
			{
				Error.AddWarning("scenario Cheat Error", "Error reading a prog override from \"{0}\"", progFilterOverride);
				return false;
			}
		}
		QuickLaunchAvailability availability = GetQuickLaunchAvailability();
		switch (availability)
		{
		case QuickLaunchAvailability.FINDING_GAME:
			Error.AddDevWarning("scenario Cheat Error", "You are already finding a game.");
			goto IL_030e;
		case QuickLaunchAvailability.ACTIVE_GAME:
			Error.AddDevWarning("scenario Cheat Error", "You are already in a game.");
			goto IL_030e;
		case QuickLaunchAvailability.SCENE_TRANSITION:
			Error.AddDevWarning("scenario Cheat Error", "Can't start a game because a scene transition is active.");
			goto IL_030e;
		case QuickLaunchAvailability.COLLECTION_NOT_READY:
			Error.AddDevWarning("scenario Cheat Error", "Can't start a game because your collection is not fully loaded.");
			goto IL_030e;
		default:
			Error.AddDevWarning("scenario Cheat Error", "Can't start a game: {0}", availability);
			goto IL_030e;
		case QuickLaunchAvailability.OK:
			{
				LaunchQuickGame(scenarioId, gameType, formatType, deck, null, progFilterOverride);
				return true;
			}
			IL_030e:
			return false;
		}
	}

	private bool OnProcessCheat_aigame(string func, string[] args, string rawArgs)
	{
		int scenarioId = 3680;
		GameType gameType = GameType.GT_VS_AI;
		string strDeck = args[0];
		if (string.IsNullOrEmpty(strDeck))
		{
			Error.AddWarning("aigame Cheat Error", "No deck string supplied");
			return false;
		}
		if (ShareableDeck.Deserialize(strDeck) == null)
		{
			Error.AddWarning("aigame Cheat Error", "Invalid deck string supplied \"{0}\"", strDeck);
			return false;
		}
		FormatType formatType = FormatType.FT_WILD;
		if (args.Length > 1)
		{
			string strFormatType = args[1];
			if (int.TryParse(strFormatType, out var intFormatType))
			{
				formatType = (FormatType)intFormatType;
			}
			else if (!EnumUtils.TryGetEnum<FormatType>(strFormatType, out formatType))
			{
				switch (strFormatType.ToLower())
				{
				case "wild":
					formatType = FormatType.FT_WILD;
					break;
				case "standard":
				case "std":
					formatType = FormatType.FT_STANDARD;
					break;
				default:
					Error.AddWarning("scenario Cheat Error", "Error reading a parameter for FormatType \"{0}\", please use \"wild\" or \"standard\"", strFormatType);
					return false;
				}
			}
		}
		QuickLaunchAvailability availability = GetQuickLaunchAvailability();
		switch (availability)
		{
		case QuickLaunchAvailability.FINDING_GAME:
			Error.AddDevWarning("scenario Cheat Error", "You are already finding a game.");
			goto IL_016f;
		case QuickLaunchAvailability.ACTIVE_GAME:
			Error.AddDevWarning("scenario Cheat Error", "You are already in a game.");
			goto IL_016f;
		case QuickLaunchAvailability.SCENE_TRANSITION:
			Error.AddDevWarning("scenario Cheat Error", "Can't start a game because a scene transition is active.");
			goto IL_016f;
		case QuickLaunchAvailability.COLLECTION_NOT_READY:
			Error.AddDevWarning("scenario Cheat Error", "Can't start a game because your collection is not fully loaded.");
			goto IL_016f;
		default:
			Error.AddDevWarning("scenario Cheat Error", "Can't start a game: {0}", availability);
			goto IL_016f;
		case QuickLaunchAvailability.OK:
			{
				LaunchQuickGame(scenarioId, gameType, formatType, null, strDeck);
				return true;
			}
			IL_016f:
			return false;
		}
	}

	private bool OnProcessCheat_loadSnapshot(string func, string[] args, string rawArgs)
	{
		if (args.Length < 1)
		{
			return false;
		}
		string snapshotFileStr = args[0];
		if (!snapshotFileStr.EndsWith(".replay"))
		{
			snapshotFileStr += ".replay";
		}
		if (!File.Exists(snapshotFileStr))
		{
			Error.AddDevWarning("loadsnapshot Cheat Error", $"Replay file {snapshotFileStr}\nnot found!");
			return false;
		}
		byte[] bytes = File.ReadAllBytes(snapshotFileStr);
		GameSnapshot snapshot = new GameSnapshot();
		snapshot.Deserialize(new MemoryStream(bytes));
		QuickLaunchAvailability availability = GetQuickLaunchAvailability();
		switch (availability)
		{
		case QuickLaunchAvailability.FINDING_GAME:
			Error.AddDevWarning("loadsnapshot Cheat Error", "You are already finding a game.");
			goto IL_00ff;
		case QuickLaunchAvailability.ACTIVE_GAME:
			Error.AddDevWarning("loadsnapshot Cheat Error", "You are already in a game.");
			goto IL_00ff;
		case QuickLaunchAvailability.SCENE_TRANSITION:
			Error.AddDevWarning("loadsnapshot Cheat Error", "Can't start a game because a scene transition is active.");
			goto IL_00ff;
		case QuickLaunchAvailability.COLLECTION_NOT_READY:
			Error.AddDevWarning("loadsnapshot Cheat Error", "Can't start a game because your collection is not fully loaded.");
			goto IL_00ff;
		default:
			Error.AddDevWarning("loadsnapshot Cheat Error", "Can't start a game: {0}", availability);
			goto IL_00ff;
		case QuickLaunchAvailability.OK:
			{
				GameType gameType = snapshot.GameType;
				FormatType formatType = snapshot.FormatType;
				int scenario = snapshot.ScenarioId;
				m_quickLaunchState.m_launching = true;
				string message = $"Launching game from replay file\n{snapshotFileStr}";
				UIStatus.Get().AddInfo(message);
				SceneMgr.Get().RegisterSceneLoadedEvent(OnSceneLoaded);
				GameMgr.Get().SetPendingAutoConcede(pendingAutoConcede: true);
				GameMgr.Get().RegisterFindGameEvent(OnFindGameEvent);
				GameMgr gameMgr = GameMgr.Get();
				long deckId = 0L;
				byte[] snapshot2 = bytes;
				gameMgr.FindGame(gameType, formatType, scenario, 0, deckId, null, null, restoreSavedGameState: false, snapshot2, null, 0L);
				return true;
			}
			IL_00ff:
			return false;
		}
	}

	private bool OnProcessCheat_exportcards(string func, string[] args, string rawArgs)
	{
		SceneManager.LoadScene("ExportCards");
		return true;
	}

	private bool OnProcessCheat_exportcardbacks(string func, string[] args, string rawArgs)
	{
		SceneManager.LoadScene("ExportCardBacks");
		return true;
	}

	private bool OnProcessCheat_freeyourmind(string func, string[] args, string rawArgs)
	{
		m_isNewCardInPackOpeningEnabled = true;
		return true;
	}

	private bool OnProcessCheat_reloadgamestrings(string func, string[] args, string rawArgs)
	{
		GameStrings.ReloadAll();
		return true;
	}

	private bool OnProcessCheat_userattentionmanager(string func, string[] args, string rawArgs)
	{
		string currentBlockers = UserAttentionManager.DumpUserAttentionBlockers("OnProcessCheat_userattentionmanager");
		UIStatus.Get().AddInfo($"Current UserAttentionBlockers: {currentBlockers}");
		return true;
	}

	private void Cheat_ShowBannerList()
	{
		StringBuilder builder = new StringBuilder();
		bool first = true;
		foreach (BannerDbfRecord record in from r in GameDbf.Banner.GetRecords()
			orderby r.ID descending
			select r)
		{
			if (!first)
			{
				builder.Append("\n");
			}
			first = false;
			builder.AppendFormat("{0}. {1}", record.ID, record.NoteDesc);
		}
		UIStatus.Get().AddInfo(builder.ToString(), 5f);
	}

	private bool OnProcessCheat_banner(string func, string[] args, string rawArgs)
	{
		int bannerId = 0;
		if (args.Length < 1 || string.IsNullOrEmpty(args[0]))
		{
			bannerId = GameDbf.Banner.GetRecords().Max((BannerDbfRecord r) => r.ID);
		}
		else
		{
			if (!int.TryParse(args[0], out bannerId))
			{
				if (args[0].Equals("list", StringComparison.InvariantCultureIgnoreCase))
				{
					Cheat_ShowBannerList();
					return true;
				}
				UIStatus.Get().AddInfo($"Unknown parameter: {args[0]}");
				return true;
			}
			if (GameDbf.Banner.GetRecord(bannerId) == null)
			{
				UIStatus.Get().AddInfo($"Unknown bannerId: {bannerId}");
				return true;
			}
		}
		BannerManager.Get().ShowBanner(bannerId);
		return true;
	}

	private bool OnProcessCheat_raf(string func, string[] args, string rawArgs)
	{
		string rafArg = args[0].ToLower();
		if (string.Equals(rafArg, "showhero"))
		{
			RAFManager.Get().ShowRAFHeroFrame();
		}
		else if (string.Equals(rafArg, "showprogress"))
		{
			RAFManager.Get().ShowRAFProgressFrame();
		}
		else if (string.Equals(rafArg, "setprogress"))
		{
			if (args.Length > 1)
			{
				int count = Convert.ToInt32(args[1]);
				RAFManager.Get().SetRAFProgress(count);
			}
		}
		else if (string.Equals(rafArg, "showglows"))
		{
			Options.Get().SetBool(Option.HAS_SEEN_RAF, val: false);
			Options.Get().SetBool(Option.HAS_SEEN_RAF_RECRUIT_URL, val: false);
			FriendListFrame friendListFrame = ChatMgr.Get().FriendListFrame;
			if (friendListFrame != null)
			{
				friendListFrame.UpdateRAFButtonGlow();
			}
			RAFFrame rafFrame = RAFManager.Get().GetRAFFrame();
			if (rafFrame != null)
			{
				rafFrame.UpdateRecruitFriendsButtonGlow();
			}
			RAFManager.Get().ShowRAFProgressFrame();
		}
		return true;
	}

	private bool OnProcessCheat_ratingdebug(string func, string[] args, string rawArgs)
	{
		if (args.Length < 1 || string.IsNullOrEmpty(args[0]))
		{
			UIStatus.Get().AddError("ratingdebug cheat must have rating id # or [standard/wild/classic/bg/mercs]");
			return true;
		}
		string arg = args[0].ToLower();
		if (!GeneralUtils.TryParseInt(arg, out var ratingId))
		{
			if (string.Equals(arg, "standard"))
			{
				ratingId = 1;
			}
			else if (string.Equals(arg, "wild"))
			{
				ratingId = 5;
			}
			else if (string.Equals(arg, "classic"))
			{
				ratingId = 12;
			}
			else if (string.Equals(arg, "bg"))
			{
				ratingId = 8;
			}
			else
			{
				if (!string.Equals(arg, "mercs"))
				{
					UIStatus.Get().AddError("ratingdebug error: Unknown argument '" + arg + "'");
					return true;
				}
				ratingId = 14;
			}
		}
		if (!Enum.IsDefined(typeof(RatingDebugOption), ratingId) || ratingId == 0)
		{
			UIStatus.Get().AddError($"ratingdebug error: Unknown rating id '{ratingId}'");
			return true;
		}
		Options.Get().SetEnum(Option.RATING_DEBUG, (RatingDebugOption)ratingId);
		SceneDebugger.Get().RequestDebugRatingInfo();
		return true;
	}

	private bool OnProcessCheat_resetrankedintro(string func, string[] args, string rawArgs)
	{
		List<GameSaveDataManager.SubkeySaveRequest> list = new List<GameSaveDataManager.SubkeySaveRequest>();
		list.Add(new GameSaveDataManager.SubkeySaveRequest(GameSaveKeyId.RANKED_PLAY, GameSaveKeySubkeyId.RANKED_PLAY_INTRO_SEEN_COUNT, default(long)));
		list.Add(new GameSaveDataManager.SubkeySaveRequest(GameSaveKeyId.RANKED_PLAY, GameSaveKeySubkeyId.RANKED_PLAY_LAST_SEASON_BONUS_STARS_POPUP_SEEN, default(long)));
		list.Add(new GameSaveDataManager.SubkeySaveRequest(GameSaveKeyId.RANKED_PLAY, GameSaveKeySubkeyId.RANKED_PLAY_BONUS_STARS_POPUP_SEEN_COUNT, default(long)));
		list.Add(new GameSaveDataManager.SubkeySaveRequest(GameSaveKeyId.RANKED_PLAY, GameSaveKeySubkeyId.RANKED_PLAY_LAST_REWARDS_VERSION_SEEN, default(long)));
		List<GameSaveDataManager.SubkeySaveRequest> saveRequests = list;
		if (GameSaveDataManager.Get().SaveSubkeys(saveRequests))
		{
			UIStatus.Get().AddInfo("Ranked intro game save data keys reset.");
			return true;
		}
		UIStatus.Get().AddInfo("Failed to reset ranked intro game save data keys!");
		return false;
	}

	private bool OnProcessCheat_advevent(string func, string[] args, string rawArgs)
	{
		if (AdventureScene.Get() == null || AdventureMissionDisplay.Get() == null || SceneMgr.Get().GetMode() != SceneMgr.Mode.ADVENTURE)
		{
			UIStatus.Get().AddError("You must be viewing an Adventure to use this cheat!");
			return true;
		}
		if (args.Length < 1 || string.IsNullOrEmpty(args[0]))
		{
			UIStatus.Get().AddError("You must provide an event from AdventureWingEventTable as a parameter!");
			return true;
		}
		if (AdventureMissionDisplay.Get().Cheat_AdventureEvent(args[0]))
		{
			UIStatus.Get().AddInfo($"Triggered event {args[0]} on each wing's AdventureWingEventTable.");
		}
		else
		{
			UIStatus.Get().AddInfo("Could not activate cheat 'advevent', perhaps 'advdev' has not been enabled yet?");
		}
		return true;
	}

	private bool OnProcessCheat_lowmemorywarning(string func, string[] args, string rawArgs)
	{
		if (args.Length < 1)
		{
			MobileCallbackManager.Get().LowMemoryWarning("");
		}
		else
		{
			MobileCallbackManager.Get().LowMemoryWarning(args[0]);
		}
		return true;
	}

	private bool OnProcessCheat_mobile(string func, string[] args, string rawArgs)
	{
		string mobileArg = args[0].ToLower();
		if (string.Equals(mobileArg, "login"))
		{
			if (args.Length > 1 && string.Equals(args[1].ToLower(), "clear"))
			{
				ServiceManager.Get<ILoginService>()?.WipeAllAuthenticationData();
				UIStatus.Get().AddInfo("Mobile Login Cleared!");
			}
		}
		else if (string.Equals(mobileArg, "push"))
		{
			if (args.Length <= 1)
			{
			}
		}
		else if (string.Equals(mobileArg, "ngdp") && args.Length > 1 && string.Equals(args[1].ToLower(), "clear"))
		{
			AlertPopup.PopupInfo info = new AlertPopup.PopupInfo();
			info.m_headerText = "GameDownloadManager";
			info.m_text = "Hearthstone can crash after clearing data. Do you still want to clear the data? Please re-launch Hearthstone after clearing data.";
			info.m_showAlertIcon = true;
			info.m_responseDisplay = AlertPopup.ResponseDisplay.CONFIRM_CANCEL;
			info.m_responseCallback = delegate(AlertPopup.Response response, object userData)
			{
				if (response != AlertPopup.Response.CANCEL && DownloadManager != null)
				{
					DownloadManager.DeleteDownloadedData();
				}
			};
			DialogManager.Get().ShowPopup(info);
		}
		return true;
	}

	private bool OnProcessCheat_edittextdebug(string func, string[] args, string rawArgs)
	{
		if (PlatformSettings.RuntimeOS == OSCategory.Android)
		{
			TextField.ToggleDebug();
		}
		else
		{
			UIStatus.Get().AddInfo("EditText debug is only for Android platforms");
		}
		return true;
	}

	private bool OnProcessCheat_resetrotationtutorial(string func, string[] args, string rawArgs)
	{
		bool newbie = true;
		if (args.Length != 0)
		{
			string arg = args[0].ToLower();
			if (string.Equals(arg, "veteran"))
			{
				newbie = false;
			}
			else if (!string.IsNullOrEmpty(arg) && !string.Equals(arg, "newbie"))
			{
				string errorMsg = $"resetrotationtutorial: {arg} is not a valid parameter!";
				UIStatus.Get().AddError(errorMsg);
				return true;
			}
		}
		if (newbie)
		{
			Options.Get().SetBool(Option.HAS_SEEN_STANDARD_MODE_TUTORIAL, val: false);
			Options.Get().SetInt(Option.SET_ROTATION_INTRO_PROGRESS, 0);
			Options.Get().SetInt(Option.SET_ROTATION_INTRO_PROGRESS_NEW_PLAYER, 0);
			Options.Get().SetBool(Option.NEEDS_TO_MAKE_STANDARD_DECK, val: true);
		}
		else
		{
			Options.Get().SetBool(Option.HAS_SEEN_STANDARD_MODE_TUTORIAL, val: true);
			Options.Get().SetInt(Option.SET_ROTATION_INTRO_PROGRESS, DateTime.Now.Year - 1);
			Options.Get().SetInt(Option.SET_ROTATION_INTRO_PROGRESS_NEW_PLAYER, DateTime.Now.Year - 1);
		}
		Options.Get().SetBool(Option.DISABLE_SET_ROTATION_INTRO, val: false);
		string completeMsg = string.Format("Set Rotation tutorial progress reset as a {0}!\nReset disableSetRotationIntro to false. Restart client to trigger the flow.", newbie ? "newbie" : "veteran");
		UIStatus.Get().AddInfo(completeMsg);
		return true;
	}

	private bool OnProcessCheat_cloud(string func, string[] args, string rawArgs)
	{
		string cloudArg = args[0].ToLower();
		if (string.Equals(cloudArg, "set"))
		{
			if (args.Length > 2)
			{
				string keyArg = args[1];
				string valueArg = args[2];
				if (string.Equals(valueArg.ToLower(), "blank"))
				{
					valueArg = "";
				}
				CloudStorageManager.SetString(keyArg, valueArg);
				UIStatus.Get().AddInfo("Cloud Storage Set: (" + keyArg + ", " + valueArg + ")");
			}
		}
		else if (string.Equals(cloudArg, "get") && args.Length > 1)
		{
			string keyArg2 = args[1];
			string value = CloudStorageManager.GetString(keyArg2);
			UIStatus.Get().AddInfo("Cloud Storage Get: Value for " + keyArg2 + " is " + ((value == null) ? "NULL" : value));
		}
		return true;
	}

	private bool OnProcessCheat_tempaccount(string func, string[] args, string rawArgs)
	{
		string tempAccountArg = args[0].ToLower();
		if (string.Equals(tempAccountArg, "dialog"))
		{
			if (args.Length > 1)
			{
				string actionArg = args[1].ToLower();
				if (string.Equals(actionArg, "skip"))
				{
					CreateSkipHelper.ShowCreateSkipDialog(null);
				}
				else if (string.Equals(actionArg, "clear"))
				{
					Options.Get().SetBool(Option.HAS_SEEN_HEAL_UP_POPUP_AFTER_TUTORIAL_TRADITIONAL, val: false);
					Options.Get().SetBool(Option.HAS_SEEN_HEAL_UP_POPUP_AFTER_TUTORIAL_BATTLEGROUNDS, val: false);
					Options.Get().SetBool(Option.HAS_SEEN_HEAL_UP_POPUP_AFTER_TUTORIAL_MERCENARIES, val: false);
					UIStatus.Get().AddInfo("Create Skip Helper Options cleared");
				}
				else if (string.Equals(actionArg, "tutorialpack"))
				{
					if (!TemporaryAccountManager.IsTemporaryAccount() || !GameUtils.IsAnyTutorialComplete())
					{
						UIStatus.Get().AddError("Can't show account healup dialog!\nBe sure that this is a temporary account that has finished a tutorial!\n'tempaccount cheat on' can be used to fake this as a temp account!");
					}
					else
					{
						TemporaryAccountManager.Get().ShowHealUpDialogWithReminderOnSkip(TemporaryAccountManager.HealUpReason.TUTORIAL_PACK_OPEN);
					}
				}
			}
			else
			{
				TemporaryAccountManager.Get().ShowHealUpDialog(GameStrings.Get("GLUE_TEMPORARY_ACCOUNT_DIALOG_HEADER_01"), GameStrings.Get("GLUE_TEMPORARY_ACCOUNT_DIALOG_BODY_03"), TemporaryAccountManager.HealUpReason.UNKNOWN, userTriggered: true, null);
			}
		}
		else if (string.Equals(tempAccountArg, "cheat"))
		{
			if (args.Length > 1)
			{
				string actionArg2 = args[1].ToLower();
				if (string.Equals(actionArg2, "on"))
				{
					Options.Get().SetBool(Option.IS_TEMPORARY_ACCOUNT_CHEAT, val: true);
					UIStatus.Get().AddInfo("Temporary Account CHEAT is now ON");
				}
				else if (string.Equals(actionArg2, "off"))
				{
					Options.Get().SetBool(Option.IS_TEMPORARY_ACCOUNT_CHEAT, val: false);
					UIStatus.Get().AddInfo("Temporary Account CHEAT is now OFF");
				}
				else if (string.Equals(actionArg2, "clear"))
				{
					Options.Get().DeleteOption(Option.IS_TEMPORARY_ACCOUNT_CHEAT);
					UIStatus.Get().AddInfo("Temporary Account CHEAT is now CLEARED");
				}
			}
		}
		else if (string.Equals(tempAccountArg, "status"))
		{
			string info = "Temporary Account status is " + (BattleNet.IsHeadlessAccount() ? "ON" : "OFF") + " Cheat is ";
			info = ((!Options.Get().HasOption(Option.IS_TEMPORARY_ACCOUNT_CHEAT)) ? (info + "CLEARED") : (info + (Options.Get().GetBool(Option.IS_TEMPORARY_ACCOUNT_CHEAT) ? "ON" : "OFF")));
			UIStatus.Get().AddInfo(info);
		}
		else if (string.Equals(tempAccountArg, "tutorial"))
		{
			if (args.Length > 1)
			{
				string actionArg3 = args[1].ToLower();
				if (string.Equals(actionArg3, "skip"))
				{
					Options.Get().SetBool(Option.CONNECT_TO_AURORA, val: true);
					Options.Get().SetEnum(Option.LOCAL_TUTORIAL_PROGRESS, TutorialProgress.LICH_KING_COMPLETE);
					UIStatus.Get().AddInfo("Set to Skip No Account Tutorial");
				}
				else if (string.Equals(actionArg3, "reset"))
				{
					Options.Get().SetBool(Option.CONNECT_TO_AURORA, val: false);
					Options.Get().SetEnum(Option.LOCAL_TUTORIAL_PROGRESS, TutorialProgress.NOTHING_COMPLETE);
					UIStatus.Get().AddInfo("Set to Reset No Account Tutorial");
				}
			}
		}
		else if (string.Equals(tempAccountArg, "id"))
		{
			string temporaryAccountId = TemporaryAccountManager.Get().GetSelectedTemporaryAccountId();
			UIStatus.Get().AddInfo("Selected Temporary Account ID is " + ((temporaryAccountId == null) ? "NULL" : temporaryAccountId));
		}
		else if (string.Equals(tempAccountArg, "healupachievement"))
		{
			AchieveManager.Get().NotifyOfAccountCreation();
		}
		else if (string.Equals(tempAccountArg, "data"))
		{
			if (args.Length > 1 && string.Equals(args[1].ToLower(), "clear"))
			{
				TemporaryAccountManager.Get().DeleteTemporaryAccountData();
				UIStatus.Get().AddInfo("Temporary Account Data Deleted");
			}
		}
		else if (string.Equals(tempAccountArg, "nag"))
		{
			if (args.Length > 1 && string.Equals(args[1].ToLower(), "clear"))
			{
				Options.Get().DeleteOption(Option.LAST_HEAL_UP_EVENT_DATE);
				UIStatus.Get().AddInfo("Last Heal Up Event Time Cleared!");
			}
		}
		else if (string.Equals(tempAccountArg, "lazy"))
		{
			ServiceManager.Get<ILoginService>()?.ClearAuthentication();
			TemporaryAccountManager.Get().DeleteTemporaryAccountData();
			Options.Get().SetBool(Option.CONNECT_TO_AURORA, val: true);
			Options.Get().SetEnum(Option.LOCAL_TUTORIAL_PROGRESS, TutorialProgress.LICH_KING_COMPLETE);
		}
		return true;
	}

	private bool OnProcessCheat_arena(string func, string[] args, string rawArgs)
	{
		string cmd = ((args.Length < 1) ? null : args[0]?.ToLower());
		string subCmd = ((args.Length < 2) ? null : args[1]?.ToLower());
		string arg1 = ((args.Length >= 3) ? args[2] : null);
		float msgDuration = 5f * Time.timeScale;
		if (string.IsNullOrEmpty(cmd) || cmd == "help")
		{
			string helpMsg = ((subCmd == "popup") ? "Valid arena popup args: clear, comingsoon [#days], endingsoon [#days]" : ((!(subCmd == "refresh")) ? "Valid arena commands: popup refresh\n\nUse 'util arena' to execute cheats on server, e.g. 'util arena season x' to switch season to x." : "refreshes Arena season info from server"));
			UIStatus.Get().AddInfo(helpMsg, msgDuration);
			return true;
		}
		string msg = null;
		switch (cmd)
		{
		case "popup":
			switch (subCmd)
			{
			case "help":
			case null:
				UIStatus.Get().AddInfo("Valid arena popup args: clear, comingsoon [#days], endingsoon [#days]", msgDuration);
				return true;
			case "clear":
			case "clearpopups":
			case "clearseen":
				if (string.Equals(arg1, "innkeeper", StringComparison.OrdinalIgnoreCase))
				{
					DraftManager.Get().ClearAllInnkeeperPopups();
					msg = "All arena innkeeper popups cleared.";
				}
				else
				{
					DraftManager.Get().ClearAllSeenPopups();
					msg = "All arena popups cleared.";
				}
				NetCache.Get().DispatchClientOptionsToServer();
				break;
			case "1":
			case "comingsoon":
			{
				if (!double.TryParse(arg1, out var days2))
				{
					days2 = 13.0;
				}
				DraftManager.Get().ShowArenaPopup_SeasonComingSoon((long)(days2 * 86400.0), null);
				msg = string.Empty;
				break;
			}
			case "2":
			case "endingsoon":
			{
				if (!double.TryParse(arg1, out var days))
				{
					days = 5.0;
				}
				DraftManager.Get().ShowArenaPopup_SeasonEndingSoon((long)(days * 86400.0), null);
				msg = string.Empty;
				break;
			}
			}
			break;
		case "refresh":
			DraftManager.Get().RefreshCurrentSeasonFromServer();
			msg = "Refreshing Arena season info from server.";
			break;
		case "season":
			msg = $"Please use 'util arena {rawArgs}' instead.";
			break;
		case "choices":
		{
			List<string> choices = new List<string>();
			for (int i = 1; i < args.Length; i++)
			{
				choices.Add(args[i]);
			}
			if (TryParseArenaChoices(choices.ToArray(), out var outputStrings))
			{
				List<string> utilArgs = new List<string>();
				utilArgs.Add("arena");
				utilArgs.Add("choices");
				string[] array = outputStrings;
				foreach (string value in array)
				{
					utilArgs.Add(value);
				}
				OnProcessCheat_utilservercmd("util", utilArgs.ToArray(), rawArgs, null);
			}
			msg = string.Empty;
			break;
		}
		}
		NetCache.Get().DispatchClientOptionsToServer();
		if (msg == null)
		{
			msg = $"Unknown subcmd: {rawArgs}";
		}
		UIStatus.Get().AddInfo(msg, msgDuration);
		return true;
	}

	private bool OnProcessCheat_EventTiming(string func, string[] args, string rawArgs, AutofillData autofillData)
	{
		args = args.Where((string a) => !string.IsNullOrEmpty(a.Trim())).ToArray();
		if (autofillData != null)
		{
			List<string> values = EventTimingManager.Get().AllKnownEvents.Select((EventTimingType e) => EventTimingManager.Get().GetName(e)).ToList();
			if (args.Length <= 1)
			{
				values.InsertRange(0, new string[3] { "list", "listall", "help" });
			}
			return ProcessAutofillParam(values, (args.Length == 0) ? string.Empty : args.Last(), autofillData);
		}
		if (args.Length != 0 && string.Equals(args[0], "help", StringComparison.OrdinalIgnoreCase))
		{
			UIStatus.Get().AddInfoNoRichText("Lists events and whether or not they're Active.\nValid args: list | listall | [event names]\n", 5f * Time.timeScale);
			return true;
		}
		List<EventTimingType> eventsToShow = new List<EventTimingType>();
		bool showOnlyRecentEvents = false;
		bool showDefaultEvents = true;
		bool listNames = false;
		string[] array = args;
		foreach (string arg in array)
		{
			if (string.Equals(arg, "list", StringComparison.OrdinalIgnoreCase))
			{
				listNames = true;
				continue;
			}
			if (string.Equals(arg, "listall", StringComparison.OrdinalIgnoreCase))
			{
				listNames = true;
				eventsToShow.AddRange(EventTimingManager.Get().AllKnownEvents);
				showDefaultEvents = false;
				continue;
			}
			string eventNamesFromArg = arg;
			if (arg.StartsWith("event=", StringComparison.OrdinalIgnoreCase) && arg.Length > 6)
			{
				eventNamesFromArg = arg.Substring(6);
			}
			Func<string, string, bool> fnSubstringMatch = (string evtName, string userInput) => evtName.Contains(userInput, StringComparison.InvariantCultureIgnoreCase);
			Func<string, string, bool> fnStartsWithMatch = (string evtName, string userInput) => evtName.StartsWith(userInput, StringComparison.InvariantCultureIgnoreCase);
			Func<string, string, bool> fnEndsWithMatch = (string evtName, string userInput) => evtName.EndsWith(userInput, StringComparison.InvariantCultureIgnoreCase);
			Func<string, string, bool> fnExactMatch = (string evtName, string userInput) => evtName.Equals(userInput, StringComparison.InvariantCultureIgnoreCase);
			string[] names = eventNamesFromArg.Split(',');
			Func<string, bool> fnIsMatch = (string evtName) => names.Any(delegate(string userInput)
			{
				Func<string, string, bool> func2 = fnSubstringMatch;
				bool flag = false;
				bool flag2 = false;
				if (userInput.StartsWith("^"))
				{
					userInput = userInput.Substring(1);
					flag = true;
				}
				if (userInput.EndsWith("$"))
				{
					userInput = userInput.Substring(0, userInput.Length - 1);
					flag2 = true;
				}
				if (userInput.Length == 0)
				{
					return false;
				}
				if (flag && flag2)
				{
					func2 = fnExactMatch;
				}
				else if (flag)
				{
					func2 = fnStartsWithMatch;
				}
				else if (flag2)
				{
					func2 = fnEndsWithMatch;
				}
				return func2(evtName, userInput);
			});
			IEnumerable<EventTimingType> parsedEvents = from evt in EventTimingManager.Get().AllKnownEvents
				let evtName = EventTimingManager.Get().GetName(evt)
				where fnIsMatch(evtName)
				select evt;
			eventsToShow.AddRange(parsedEvents);
			showDefaultEvents = false;
		}
		if (showDefaultEvents)
		{
			eventsToShow = EventTimingManager.Get().AllKnownEvents.Where((EventTimingType e) => EventTimingManager.Get().IsEventActive(e)).ToList();
			showOnlyRecentEvents = true;
		}
		DateTime utcNow = DateTime.UtcNow;
		if (showOnlyRecentEvents)
		{
			eventsToShow.RemoveAll(delegate(EventTimingType e)
			{
				DateTime? eventStartTimeUtc = EventTimingManager.Get().GetEventStartTimeUtc(e);
				DateTime? eventEndTimeUtc = EventTimingManager.Get().GetEventEndTimeUtc(e);
				TimeSpan timeSpan = ((!eventStartTimeUtc.HasValue) ? TimeSpan.MaxValue : ((eventStartTimeUtc.Value > utcNow) ? (eventStartTimeUtc.Value - utcNow) : (utcNow - eventStartTimeUtc.Value)));
				TimeSpan timeSpan2 = ((!eventEndTimeUtc.HasValue) ? TimeSpan.MaxValue : ((eventEndTimeUtc.Value > utcNow) ? (eventEndTimeUtc.Value - utcNow) : (utcNow - eventEndTimeUtc.Value)));
				bool num = timeSpan.TotalDays <= 120.0;
				bool flag3 = timeSpan2.TotalDays <= 120.0;
				return !num && !flag3;
			});
		}
		if (eventsToShow.Count <= 0)
		{
			UIStatus.Get().AddInfoNoRichText("No events to show (check event names).");
			return true;
		}
		eventsToShow.Sort(delegate(EventTimingType lhs, EventTimingType rhs)
		{
			bool flag4 = EventTimingManager.Get().IsEventActive(lhs);
			bool flag5 = EventTimingManager.Get().IsEventActive(rhs);
			if (flag4 != flag5)
			{
				if (!flag4)
				{
					return 1;
				}
				return -1;
			}
			DateTime? eventStartTimeUtc2 = EventTimingManager.Get().GetEventStartTimeUtc(lhs);
			DateTime? eventStartTimeUtc3 = EventTimingManager.Get().GetEventStartTimeUtc(rhs);
			if (eventStartTimeUtc2 != eventStartTimeUtc3)
			{
				if (eventStartTimeUtc2.HasValue)
				{
					if (eventStartTimeUtc3.HasValue)
					{
						return eventStartTimeUtc2.Value.CompareTo(eventStartTimeUtc3.Value);
					}
					return 1;
				}
				return -1;
			}
			DateTime? eventEndTimeUtc2 = EventTimingManager.Get().GetEventEndTimeUtc(lhs);
			DateTime? eventEndTimeUtc3 = EventTimingManager.Get().GetEventEndTimeUtc(rhs);
			if (eventEndTimeUtc2 != eventEndTimeUtc3)
			{
				if (eventEndTimeUtc2.HasValue)
				{
					if (eventEndTimeUtc3.HasValue)
					{
						return eventEndTimeUtc2.Value.CompareTo(eventEndTimeUtc3.Value);
					}
					return -1;
				}
				return 1;
			}
			string name = EventTimingManager.Get().GetName(lhs);
			string name2 = EventTimingManager.Get().GetName(rhs);
			return name.CompareTo(name2);
		});
		StringBuilder builder = new StringBuilder();
		foreach (EventTimingType eventType in eventsToShow)
		{
			if (listNames)
			{
				if (builder.Length != 0)
				{
					builder.Append(", ");
				}
				builder.Append(EventTimingManager.Get().GetName(eventType));
				continue;
			}
			bool active = EventTimingManager.Get().IsEventActive(eventType);
			DateTime? start = EventTimingManager.Get().GetEventStartTimeUtc(eventType);
			DateTime? end = EventTimingManager.Get().GetEventEndTimeUtc(eventType);
			DateTime? displayStart = start;
			DateTime? displayEnd = end;
			if (displayStart.HasValue)
			{
				displayStart = displayStart.Value.AddSeconds(EventTimingManager.Get().DevTimeOffsetSeconds).ToLocalTime();
			}
			if (displayEnd.HasValue)
			{
				displayEnd = displayEnd.Value.AddSeconds(EventTimingManager.Get().DevTimeOffsetSeconds).ToLocalTime();
			}
			if (builder.Length != 0)
			{
				builder.Append("\n");
			}
			string startStr = (displayStart.HasValue ? displayStart.Value.ToString("yyyy/MM/dd") : "<always>");
			string endStr = (displayEnd.HasValue ? displayEnd.Value.ToString("yyyy/MM/dd") : "<forever>");
			builder.AppendFormat("{0} {1} {2}-{3}", EventTimingManager.Get().GetName(eventType), active ? "Active" : "Inactive", startStr, endStr);
			if (active)
			{
				TimeSpan? tilEnd = ((!end.HasValue || end.Value < utcNow) ? ((TimeSpan?)null) : new TimeSpan?(end.Value - utcNow));
				if (tilEnd.HasValue && tilEnd.Value.TotalDays < 3.0)
				{
					builder.AppendFormat(" ends in {0}", TimeUtils.GetElapsedTimeString((int)tilEnd.Value.TotalSeconds, TimeUtils.SPLASHSCREEN_DATETIME_STRINGSET, roundUp: true));
				}
			}
			else
			{
				TimeSpan? tilStart = ((!start.HasValue || start.Value < utcNow) ? ((TimeSpan?)null) : new TimeSpan?(start.Value - utcNow));
				if (tilStart.HasValue && tilStart.Value.TotalDays < 3.0)
				{
					builder.AppendFormat(" starts in {0}", TimeUtils.GetElapsedTimeString((int)tilStart.Value.TotalSeconds, TimeUtils.SPLASHSCREEN_DATETIME_STRINGSET, roundUp: true));
				}
			}
		}
		builder.Append("\n");
		float duration = (float)Mathf.Max(5, 2 * Mathf.Min(20, eventsToShow.Count)) * Time.timeScale;
		string msg = builder.ToString();
		Log.EventTiming.PrintInfo(msg);
		UIStatus.Get().AddInfoNoRichText(msg, duration);
		return true;
	}

	private bool OnProcessCheat_UpdateIntention(string func, string[] args, string rawArgs)
	{
		Options.Get().SetInt(Option.UPDATE_STATE, int.Parse(args[0]));
		return true;
	}

	private bool OnProcessCheat_autoexportgamestate(string func, string[] args, string rawArgs)
	{
		if (SceneMgr.Get().GetMode() != SceneMgr.Mode.GAMEPLAY)
		{
			return false;
		}
		string filename = (string.IsNullOrEmpty(args[0]) ? "GameStateExportFile" : args[0]);
		JsonNode output = new JsonNode();
		foreach (KeyValuePair<int, Player> player in GameState.Get().GetPlayerMap())
		{
			string identifier = "Player" + player.Key;
			JsonNode identifierNode = new JsonNode();
			output.Add(identifier, identifierNode);
			identifierNode["Hero"] = GetCardJson(player.Value.GetHero());
			identifierNode["HeroPower"] = GetCardJson(player.Value.GetHeroPower());
			if (player.Value.HasWeapon())
			{
				identifierNode["Weapon"] = GetCardJson(player.Value.GetWeaponCard().GetEntity());
			}
			identifierNode["CardsInBattlefield"] = GetCardlistJson(player.Value.GetBattlefieldZone().GetCards());
			if (player.Value.GetSide() == Player.Side.FRIENDLY)
			{
				identifierNode["CardsInHand"] = GetCardlistJson(player.Value.GetHandZone().GetCards());
				identifierNode["ActiveSecrets"] = GetCardlistJson(player.Value.GetSecretZone().GetCards());
			}
		}
		File.WriteAllText($"{System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop)}\\{filename}.json", Json.Serialize(output));
		return true;
	}

	private bool OnProcessCheat_social(string func, string[] args, string rawArgs)
	{
		List<BnetPlayer> friends = BnetFriendMgr.Get().GetFriends();
		List<BnetPlayer> recentPlayers = BnetRecentPlayerMgr.Get().GetRecentPlayers();
		List<BnetPlayer> nearbyPlayers = BnetNearbyPlayerMgr.Get().GetNearbyPlayers();
		friends.Sort(FriendUtils.FriendSortCompare);
		recentPlayers.Sort(FriendUtils.RecentFriendSortCompare);
		nearbyPlayers.Sort(FriendUtils.FriendSortCompare);
		bool printFriends = false;
		bool printRecentPlayers = false;
		bool printNearbyPlayers = false;
		bool printFullPresence = false;
		string usage = "USAGE: social [cmd] [args]\nCommands: help, list";
		float statusTextTime = 5f;
		string cmd = ((args == null || args.Length == 0) ? "list" : args[0]?.ToLower());
		string status = null;
		if (!(cmd == "help"))
		{
			if (cmd == "list")
			{
				if (args.Length >= 2 && string.Equals(args[1], "help", StringComparison.OrdinalIgnoreCase))
				{
					status = "Lists all players in the various social lists. Can specific specific lists: friend, nearby";
				}
				else
				{
					for (int i = 1; i < args.Length; i++)
					{
						switch ((args[i] == null) ? "" : args[i].ToLower())
						{
						case "friend":
						case "friends":
							printFriends = true;
							break;
						case "recent":
						case "recentplayer":
						case "recentplayers":
							printRecentPlayers = true;
							break;
						case "nearby":
						case "nearbyplayer":
						case "nearbyplayers":
						case "subnet":
						case "local":
						case "localplayer":
						case "localplayers":
							printNearbyPlayers = true;
							break;
						case "full":
						case "all":
						case "presence":
							printFullPresence = true;
							break;
						}
					}
					if (!printFriends && !printRecentPlayers && !printNearbyPlayers)
					{
						printFriends = (printRecentPlayers = (printNearbyPlayers = true));
					}
					Log.Presence.PrintInfo("Cheat: print social list executed.");
					if (printFriends)
					{
						Log.Presence.PrintInfo("Friends: {0}", friends.Count);
						foreach (BnetPlayer player in friends)
						{
							OnProcessCheat_social_PrintPlayer(printFullPresence, player);
						}
					}
					if (printRecentPlayers)
					{
						Log.Presence.PrintInfo("Recent Players: {0}", recentPlayers.Count);
						foreach (BnetPlayer player2 in recentPlayers)
						{
							OnProcessCheat_social_PrintPlayer(printFullPresence, player2);
						}
					}
					if (printNearbyPlayers)
					{
						Log.Presence.PrintInfo("Nearby Players: {0}", nearbyPlayers.Count);
						foreach (BnetPlayer player3 in nearbyPlayers)
						{
							OnProcessCheat_social_PrintPlayer(printFullPresence, player3);
						}
					}
					status = "Printed to Presence Log.";
				}
			}
		}
		else
		{
			status = usage;
		}
		if (status != null)
		{
			UIStatus.Get().AddInfo(status, statusTextTime);
		}
		return true;
	}

	private bool OnProcessCheat_playStartEmote(string func, string[] args, string rawArgs)
	{
		Gameplay gameplay = Gameplay.Get();
		if (gameplay == null)
		{
			return false;
		}
		gameplay.StartCoroutine(PlayStartingTaunts());
		return true;
	}

	private bool OnProcessCheat_getBattlegroundHeroArmorTierList(string func, string[] args, string rawArgs)
	{
		Network.Get().UpdateBattlegroundHeroArmorTierList();
		GameState gamestate = GameState.Get();
		if (gamestate == null)
		{
			return false;
		}
		gamestate.SetPrintBattlegroundHeroArmorTierListOnUpdate(isPrinting: true);
		return true;
	}

	private bool OnProcessCheat_getBattlegroundsPlayerAnomaly(string func, string[] args, string rawArgs)
	{
		Network.Get().UpdateBattlegroundsPlayerAnomaly();
		GameState gamestate = GameState.Get();
		if (gamestate == null)
		{
			return false;
		}
		gamestate.SetPrintBattlegroundAnomalyOnUpdate(isPrinting: true);
		return true;
	}

	private bool OnProcessCheat_toggleBGCombatTimerDisplay(string func, string[] args, string rawArgs)
	{
		if (GameState.Get() == null)
		{
			return false;
		}
		if (!(GameState.Get().GetGameEntity() is TB_BaconShop gameEnttiy))
		{
			return false;
		}
		gameEnttiy.ToggleDebugCombatTimer();
		return true;
	}

	private bool OnProcessCheat_resetBGDuosTutroialFlags(string func, string[] args, string rawArgs)
	{
		if (Options.Get() == null)
		{
			return false;
		}
		BaconDisplay.ClearDuosTutorialFlags();
		Options.Get().SetBool(Option.BG_DUOS_LOBBY_TUTORIAL_PLAYED_TOGGLE, val: false);
		Options.Get().SetBool(Option.BG_DUOS_LOBBY_TUTORIAL_PLAYED_QUEUE, val: false);
		Options.Get().SetBool(Option.BG_DUOS_LOBBY_TUTORIAL_PLAYED_ARRANGE_PARTY, val: false);
		Options.Get().SetBool(Option.BG_NEW_RULES_VIEWED, val: false);
		UIStatus.Get().AddInfo("Cleared Duos Tutorial flags");
		return true;
	}

	private bool OnProcessCheat_ToggleAIPlayer(string func, string[] args, string rawArgs)
	{
		Network.Get().ToggleAIPlayer();
		return true;
	}

	private bool OnProcessCheat_FakeDefeat(string func, string[] args, string rawArgs)
	{
		GameState gameState = GameState.Get();
		if (gameState == null)
		{
			return false;
		}
		gameState.FakeConceded();
		return true;
	}

	private bool OnProcessCheat_SetBattlegroundHeroBuddyProgress(string func, string[] args, string rawArgs)
	{
		int heroBuddyProgress = 0;
		if (args.Length >= 1 && !int.TryParse(args[0], out heroBuddyProgress))
		{
			Log.Gameplay.PrintError("[OnProcessCheat_SetBattlegroundHeroBuddyProgress] Unable to parse buddy progress " + args[0]);
			return false;
		}
		int playerID = 0;
		if (args.Length >= 2 && !int.TryParse(args[1], out playerID))
		{
			playerID = 0;
		}
		Network.Get().SetBattlegroundHeroBuddyProgress(heroBuddyProgress, playerID);
		return true;
	}

	private bool OnProcessCheat_EnableBattlegroundHeroBuddy(string func, string[] args, string rawArgs)
	{
		int enabled = 0;
		if (!int.TryParse(args[0], out enabled))
		{
			return false;
		}
		bool newEnabled = enabled != 0;
		bool num = m_battlegroundHeroBuddyEnabled != newEnabled;
		m_battlegroundHeroBuddyEnabled = newEnabled;
		if (num)
		{
			PlayerLeaderboardManager.Get().NotifyBattlegroundHeroBuddyEnabledDirty();
		}
		return true;
	}

	private bool OnProcessCheat_ReplaceBattlegroundHero(string func, string[] args, string rawArgs)
	{
		int newHeroID = 0;
		if (!int.TryParse(args[0], out newHeroID))
		{
			Log.Gameplay.PrintError("[OnProcessCheat_ReplaceBattlegroundHero] Unable to parse new hero " + args[0]);
			return false;
		}
		int playerID = 0;
		if (!int.TryParse(args[1], out playerID))
		{
			playerID = 0;
		}
		Network.Get().ReplaceBattlegroundHero(newHeroID, playerID);
		return true;
	}

	private bool OnProcessCheat_SetBattlegroundsLuckyDrawEndTime(string func, string[] args, string rawArgs)
	{
		return false;
	}

	private bool OnProcessCheat_BattlegroundsBoardFSMManipulate(string func, string[] args, string rawArgs)
	{
		BaconBoard board = BaconBoard.Get();
		if (board == null)
		{
			Log.Gameplay.PrintError("BaconBoard not available");
			return false;
		}
		string cmd = args[0]?.ToLower();
		switch (cmd)
		{
		case "setwinstreak":
		{
			int newStreak = 0;
			if (args.Length < 2 || !int.TryParse(args[1], out newStreak))
			{
				Log.Gameplay.PrintError("[OnProcessCheat_BattlegroundsBoardFSMManipulate] Unable to parse new streak");
				return false;
			}
			board.CheatSetWinstreak(newStreak);
			break;
		}
		case "setopponentdefeated":
			board.CheatSetHasDefeatedOpponent();
			break;
		case "setracedefeated":
		{
			int race = 0;
			if (args.Length < 2 || !int.TryParse(args[1], out race))
			{
				Log.Gameplay.PrintError("[OnProcessCheat_BattlegroundsBoardFSMManipulate] Unable to parse race enum number");
				return false;
			}
			if (!Enum.IsDefined(typeof(TAG_RACE), race))
			{
				Log.Gameplay.PrintError("[OnProcessCheat_BattlegroundsBoardFSMManipulate] " + args[1] + " does not correspond to a valid race");
				return false;
			}
			board.CheatAddDefeatedRace((TAG_RACE)race);
			break;
		}
		case "setdefeatedminioncount":
		{
			int count = 0;
			if (args.Length < 2 || !int.TryParse(args[1], out count))
			{
				Log.Gameplay.PrintError("[OnProcessCheat_BattlegroundsBoardFSMManipulate] Unable to parse count argument");
				return false;
			}
			board.CheatSetDefeatedMinionCount(count);
			break;
		}
		case "adddefeatedminion":
			if (args.Length < 2)
			{
				Log.Gameplay.PrintError("[OnProcessCheat_BattlegroundsBoardFSMManipulate] No minion CardId provided");
				break;
			}
			if (GameUtils.GetCardRecord(args[1]) == null)
			{
				Log.Gameplay.PrintError("[OnProcessCheat_BattlegroundsBoardFSMManipulate] Provided invalid CardId");
				return false;
			}
			board.CheatAddDefeatedMinion(args[1]);
			break;
		case "triggerdefeatopponenthero":
			if (!board.SetOpponentHeroDefeated())
			{
				Log.Gameplay.PrintError("Attempted to trigger board effect without an instance");
				return false;
			}
			break;
		case "triggerdefeatminion":
		{
			if (args.Length < 2)
			{
				Log.Gameplay.PrintError("[OnProcessCheat_BattlegroundsBoardFSMManipulate] No minion CardId provided");
				break;
			}
			string cardID = args[1];
			if (GameUtils.GetCardRecord(cardID) == null)
			{
				Log.Gameplay.PrintError("[OnProcessCheat_BattlegroundsBoardFSMManipulate] Provided invalid CardId");
				return false;
			}
			if (board.CheatTriggerDefeatedMinion(cardID))
			{
				break;
			}
			Log.Gameplay.PrintError("Attempted to trigger board effect without an instance");
			return false;
		}
		case "triggerheavyhit":
			if (!board.CheatTriggerHeroHeavyHitEffects())
			{
				Log.Gameplay.PrintError("Attempted to trigger board effect without an instance");
				return false;
			}
			break;
		case "triggerminionheavyhit":
			if (!board.CheatTriggerMinionHeavyHitEffects())
			{
				Log.Gameplay.PrintError("Attempted to trigger board effect without an instance");
				return false;
			}
			break;
		case "triggerall":
			if (args.Length > 1 && args[1].Contains("finisher"))
			{
				string cheat = "finisher player ";
				cheat = ((args.Length <= 2 || !args[2].Contains("id=")) ? (cheat + "id=2") : (cheat + args[2]));
				cheat += " large";
				CheatMgr.Get().ProcessCheat(cheat, doNotSaveToHistory: true);
			}
			if (!board.CheatTriggerAllBoardEffects())
			{
				Log.Gameplay.PrintError("Attempted to trigger all board effects without an instance");
				return false;
			}
			break;
		default:
			UIStatus.Get().AddError(cmd + " is not a valid command");
			break;
		}
		return true;
	}

	private bool OnProcessCheat_SetBattlegroundHeroBuddyGained(string func, string[] args, string rawArgs)
	{
		int heroBuddyGained = 0;
		if (!int.TryParse(args[0], out heroBuddyGained))
		{
			Log.Gameplay.PrintError("[OnProcessCheat_SetBattlegroundHeroBuddyGained] Unable to parse buddy progress " + args[0]);
			return false;
		}
		int playerID = 0;
		if (!int.TryParse(args[1], out playerID))
		{
			playerID = 0;
		}
		Network.Get().SetBattlegroundHeroBuddyGained(heroBuddyGained, playerID);
		return true;
	}

	private bool OnProcessCheat_getBattlegroundDenyList(string func, string[] args, string rawArgs)
	{
		Network.Get().UpdateBattlegroundInfo();
		GameState gamestate = GameState.Get();
		if (gamestate == null)
		{
			return false;
		}
		gamestate.SetPrintBattlegroundDenyListOnUpdate(isPrinting: true);
		return true;
	}

	private bool OnProcessCheat_getBattlegroundMinionPool(string func, string[] args, string rawArgs)
	{
		Network.Get().UpdateBattlegroundInfo();
		GameState gamestate = GameState.Get();
		if (gamestate == null)
		{
			return false;
		}
		gamestate.SetPrintBattlegroundMinionPoolOnUpdate(isPrinting: true);
		return true;
	}

	private IEnumerator PlayStartingTaunts()
	{
		return EmoteHandler.Get().PlayStartingTaunts(null);
	}

	private static void OnProcessCheat_social_PrintPlayer(bool printFullPresence, BnetPlayer player)
	{
		string strPlayer = ((player == null) ? "<null>" : (printFullPresence ? player.FullPresenceSummary : player.ShortSummary));
		SortedList<string, bool> relationships = new SortedList<string, bool>();
		if (BnetRecentPlayerMgr.Get().IsRecentPlayer(player))
		{
			relationships["recent"] = true;
		}
		if (BnetNearbyPlayerMgr.Get().IsNearbyPlayer(player))
		{
			relationships["nearby"] = true;
		}
		if (BnetFriendMgr.Get().IsFriend(player))
		{
			relationships["friend"] = true;
		}
		string strRelationships = string.Join(", ", relationships.Keys.ToArray());
		if (!string.IsNullOrEmpty(strRelationships))
		{
			strRelationships = $"[{strRelationships}]";
		}
		Log.Presence.PrintInfo("    {0} {1}", strPlayer, strRelationships);
	}

	private bool OnProcessCheat_OpponentName(string func, string[] args, string rawArgs)
	{
		Gameplay gameplay = Gameplay.Get();
		if (gameplay == null)
		{
			return false;
		}
		NameBanner nameBanner = gameplay.GetNameBannerForSide(Player.Side.OPPOSING);
		if (nameBanner == null)
		{
			return false;
		}
		nameBanner.m_playerName.Text = args[0];
		return true;
	}

	private bool OnProcessCheat_friendlist(string func, string[] args, string rawArgs)
	{
		string usage = "USAGE: flist [cmd] [args]\nCommands: fill, add, remove";
		if (args.Length < 1 || args.Any((string a) => string.IsNullOrEmpty(a)))
		{
			UIStatus.Get().AddInfo(usage, 10f);
			return true;
		}
		float statusTextTime = 5f;
		string status = null;
		switch (args[0]?.ToLower())
		{
		case "fill":
		{
			int seasonId2 = NetCache.Get().GetNetObject<NetCache.NetCacheRewardProgress>().Season;
			int leagueId2 = RankMgr.Get().GetLeagueRecordForType(League.LeagueType.NORMAL, seasonId2).ID;
			int maxStarLevel = RankMgr.Get().GetMaxStarLevel(leagueId2);
			foreach (FriendListType type2 in Enum.GetValues(typeof(FriendListType)))
			{
				for (int k = 1; k < maxStarLevel; k++)
				{
					string name2 = $"{type2} friend{k}";
					CreateCheatFriendlistItem(name2, type2, leagueId2, k, BnetProgramId.HEARTHSTONE, isFriend: true, isOnline: true, isAway: false);
				}
			}
			status = $"Filled friend list";
			break;
		}
		case "add":
		{
			int num = 1;
			string name = "Player";
			FriendListType type = FriendListType.FRIEND;
			int seasonId = NetCache.Get().GetNetObject<NetCache.NetCacheRewardProgress>().Season;
			int leagueId = RankMgr.Get().GetLeagueRecordForType(League.LeagueType.NORMAL, seasonId).ID;
			int starLevel = 1;
			BnetProgramId programID = BnetProgramId.HEARTHSTONE;
			bool isFriend = true;
			bool isOnline = true;
			bool isAway = false;
			for (int i = 0; i < args.Length; i++)
			{
				string[] parts = args[i]?.Split('=');
				if (parts == null || parts.Length < 2)
				{
					continue;
				}
				if (parts[0].Equals("num", StringComparison.InvariantCultureIgnoreCase))
				{
					int.TryParse(parts[1], out num);
					if (num < 1)
					{
						num = 1;
					}
				}
				else if (parts[0].Equals("name", StringComparison.InvariantCultureIgnoreCase))
				{
					name = parts[1];
				}
				else if (parts[0].Equals("type", StringComparison.InvariantCultureIgnoreCase))
				{
					string typeString = parts[1];
					if (!string.IsNullOrEmpty(typeString))
					{
						type = EnumUtils.SafeParse(typeString, FriendListType.FRIEND, ignoreCase: true);
					}
				}
				else if (parts[0].Equals("rank", StringComparison.InvariantCultureIgnoreCase))
				{
					LeagueRankDbfRecord rankRecord = RankMgr.Get().GetLeagueRankRecordByCheatName(parts[1]);
					if (rankRecord != null)
					{
						leagueId = rankRecord.LeagueId;
						starLevel = rankRecord.StarLevel;
					}
				}
				else if (parts[0].Equals("program", StringComparison.InvariantCultureIgnoreCase))
				{
					string programIDString = parts[1];
					if (!string.IsNullOrEmpty(programIDString))
					{
						programID = new BnetProgramId(programIDString);
						leagueId = 0;
						starLevel = 0;
					}
				}
				else if (parts[0].Equals("friend", StringComparison.InvariantCultureIgnoreCase))
				{
					GeneralUtils.TryParseBool(parts[1], out isFriend);
				}
				else if (parts[0].Equals("online", StringComparison.InvariantCultureIgnoreCase))
				{
					GeneralUtils.TryParseBool(parts[1], out isOnline);
				}
				else if (parts[0].Equals("away", StringComparison.InvariantCultureIgnoreCase))
				{
					GeneralUtils.TryParseBool(parts[1], out isAway);
				}
			}
			for (int j = 0; j < num; j++)
			{
				CreateCheatFriendlistItem(name + j, type, leagueId, starLevel, programID, isFriend, isOnline, isAway);
			}
			status = $"Created {num} players";
			break;
		}
		case "remove":
			BnetRecentPlayerMgr.Get().Cheat_RemoveCheatFriends();
			BnetNearbyPlayerMgr.Get().Cheat_RemoveCheatFriends();
			BnetFriendMgr.Get().Cheat_RemoveCheatFriends();
			status = $"Removed cheat friends";
			break;
		}
		BnetBarFriendButton.Get().UpdateOnlineCount();
		if (status != null)
		{
			UIStatus.Get().AddInfo(status, statusTextTime);
		}
		return true;
	}

	private bool OnProcessCheat_SetGameSaveData(string func, string[] args, string rawArgs)
	{
		GameSaveKeyId key = (GameSaveKeyId)0;
		GameSaveKeySubkeyId subkey = (GameSaveKeySubkeyId)0;
		if (!ValidateAndParseGameSaveDataKeyAndSubkey(args, out key, out subkey))
		{
			UIStatus.Get().AddError("You must provide valid key and subkeys!");
			return true;
		}
		long subkeyValue = 0L;
		int valueIndex = 2;
		string valuesString = string.Empty;
		List<long> valuesList = new List<long>();
		for (; valueIndex < args.Count(); valueIndex++)
		{
			if (!ValidateAndParseLongAtIndex(valueIndex, args, out subkeyValue))
			{
				subkeyValue = GameUtils.TranslateCardIdToDbId(args[valueIndex], showWarning: true);
				if (subkeyValue == 0L)
				{
					break;
				}
			}
			valuesList.Add(subkeyValue);
			valuesString = valuesString + subkeyValue + ";";
		}
		args = new string[4]
		{
			"setgsd",
			"key=" + args[0],
			"subkey=" + args[1],
			"values=" + valuesString
		};
		GameSaveDataManager.Get().Cheat_SaveSubkeyToLocalCache(key, subkey, valuesList.ToArray());
		UIStatus.Get().AddInfo($"Set key {key} subkey {subkey} to {valuesString}");
		return OnProcessCheat_utilservercmd("util", args, rawArgs, null);
	}

	private bool OnProcessCheat_ShowTip(string func, string[] args, string rawArgs)
	{
		TipCategory tipCategory = TipCategory.DEFAULT;
		int? index = null;
		if (args[0].Length > 0)
		{
			switch (args[0].ToUpper())
			{
			case "PRACTICE":
				tipCategory = TipCategory.PRACTICE;
				break;
			case "PLAY":
				tipCategory = TipCategory.PLAY;
				break;
			case "FORGE":
				tipCategory = TipCategory.FORGE;
				break;
			case "DEFAULT":
				tipCategory = TipCategory.DEFAULT;
				break;
			case "QUEST_LOG":
				tipCategory = TipCategory.QUEST_LOG;
				break;
			case "QUEST_LOG_RANDOM":
				tipCategory = TipCategory.QUEST_LOG_RANDOM;
				break;
			case "ADVENTURE":
				tipCategory = TipCategory.ADVENTURE;
				break;
			case "TAVERNBRAWL":
				tipCategory = TipCategory.TAVERNBRAWL;
				break;
			case "HEROICBRAWL":
				tipCategory = TipCategory.HEROICBRAWL;
				break;
			case "BACON":
				tipCategory = TipCategory.BACON;
				break;
			case "BACON_SOLO":
				tipCategory = TipCategory.BACON_SOLO;
				break;
			case "BACON_DUO":
				tipCategory = TipCategory.BACON_DUO;
				break;
			case "LETTUCE":
				tipCategory = TipCategory.LETTUCE;
				break;
			default:
				UIStatus.Get().AddInfo("Valid Categories: INVALID, PRACTICE, PLAY, FORGE, DEFAULT, QUEST_LOG, QUEST_LOG_RANDOM, ADVENTURE, TAVERNBRAWL, HEROICBRAWL, BACON, DUELS, LETTUCE");
				return true;
			}
			if (args.Length == 2 && int.TryParse(args[1], out var result))
			{
				index = result;
			}
		}
		UIStatus.Get().AddInfo(GameStrings.GetTip(tipCategory, index));
		return true;
	}

	private bool OnProcessCheat_SetDungeonRunProgress(string func, string[] args, string rawArgs)
	{
		return ParseAdventureThenSetProgress(args, SetAdventureProgressMode.Progress);
	}

	private bool OnProcessCheat_SetDungeonRunVictory(string func, string[] args, string rawArgs)
	{
		return ParseAdventureThenSetProgress(args, SetAdventureProgressMode.Victory);
	}

	private bool OnProcessCheat_SetDungeonRunDefeat(string func, string[] args, string rawArgs)
	{
		return ParseAdventureThenSetProgress(args, SetAdventureProgressMode.Defeat);
	}

	private bool OnProcessCheat_ResetDungeonRunAdventure(string func, string[] args, string rawArgs)
	{
		AdventureDbId adventure = ParseAdventureDbIdFromArgs(args, 0);
		if (adventure == AdventureDbId.INVALID)
		{
			return true;
		}
		return ResetDungeonRunAdventure(adventure, AdventureModeDbId.DUNGEON_CRAWL);
	}

	private bool ResetDungeonRunAdventure(AdventureDbId adventure, AdventureModeDbId mode)
	{
		if (adventure == AdventureDbId.INVALID)
		{
			return true;
		}
		AdventureDataDbfRecord dataRecord = GameUtils.GetAdventureDataRecord((int)adventure, (int)mode);
		if (dataRecord == null)
		{
			UIStatus.Get().AddError($"No Adventure data found for Adventure {adventure} Mode {mode}");
			return true;
		}
		if (dataRecord.GameSaveDataServerKey == 0)
		{
			UIStatus.Get().AddError($"No GameSaveDataServerKey for Adventure {adventure} Mode {mode}!");
			return true;
		}
		ResetAdventureRunCommon_Server(dataRecord.GameSaveDataServerKey);
		if (dataRecord.GameSaveDataClientKey != 0)
		{
			ResetAdventureRunCommon_Client(dataRecord.GameSaveDataClientKey);
		}
		UIStatus.Get().AddInfo($"Reset current run for Adventure {adventure} Mode {mode}");
		return true;
	}

	private bool OnProcessCheat_ResetDungeonRun_VO(string func, string[] args, string rawArgs)
	{
		AdventureDbId adventure = ParseAdventureDbIdFromArgs(args, 0);
		if (adventure == AdventureDbId.INVALID)
		{
			return true;
		}
		long subkeyValue = 0L;
		ValidateAndParseLongAtIndex(1, args, out subkeyValue);
		return ResetDungeonRun_VO(adventure, subkeyValue);
	}

	private bool ResetDungeonRun_VO(AdventureDbId adventure, long subkeyValue)
	{
		AdventureDungeonCrawlDisplay.s_shouldShowWelcomeBanner = true;
		switch (adventure)
		{
		case AdventureDbId.LOOT:
			Options.Get().SetBool(Option.HAS_JUST_SEEN_LOOT_NO_TAKE_CANDLE_VO, val: false);
			break;
		case AdventureDbId.GIL:
			Options.Get().SetBool(Option.HAS_SEEN_PLAYED_TESS, val: false);
			Options.Get().SetBool(Option.HAS_SEEN_PLAYED_DARIUS, val: false);
			Options.Get().SetBool(Option.HAS_SEEN_PLAYED_SHAW, val: false);
			Options.Get().SetBool(Option.HAS_SEEN_PLAYED_TOKI, val: false);
			break;
		}
		AdventureModeDbId mode = AdventureModeDbId.DUNGEON_CRAWL;
		AdventureDataDbfRecord dataRecord = GameUtils.GetAdventureDataRecord((int)adventure, (int)mode);
		if (dataRecord == null)
		{
			UIStatus.Get().AddError($"No Adventure data found for Adventure {adventure} Mode {mode}");
			return true;
		}
		if (dataRecord.GameSaveDataClientKey == 0)
		{
			UIStatus.Get().AddError($"No GameSaveDataClientKey for Adventure {adventure} Mode {mode}!");
			return true;
		}
		ResetVOSubkeysForAdventure((GameSaveKeyId)dataRecord.GameSaveDataClientKey, subkeyValue);
		if (dataRecord.GameSaveDataServerKey != 0)
		{
			ResetVOSubkeysForAdventure((GameSaveKeyId)dataRecord.GameSaveDataServerKey, subkeyValue);
		}
		UIStatus.Get().AddInfo($"You can now see all {adventure} VO again.");
		return true;
	}

	private bool ParseAdventureThenSetProgress(string[] args, SetAdventureProgressMode progressMode)
	{
		AdventureDbId adventure = ParseAdventureDbIdFromArgs(args, 0);
		if (adventure == AdventureDbId.INVALID)
		{
			return true;
		}
		string[] prunedArgs = new string[args.Length - 1];
		Array.Copy(args, 1, prunedArgs, 0, args.Length - 1);
		if (SetAdventureProgressCommon(adventure, AdventureModeDbId.DUNGEON_CRAWL, prunedArgs, progressMode))
		{
			UIStatus.Get().AddInfo($"Set Dungeon Run {progressMode} for {adventure}");
		}
		return true;
	}

	private bool OnProcessCheat_SetKCVictory(string func, string[] args, string rawArgs)
	{
		if (SetAdventureProgressCommon(AdventureDbId.LOOT, AdventureModeDbId.DUNGEON_CRAWL, args, SetAdventureProgressMode.Victory))
		{
			UIStatus.Get().AddInfo($"Set KC victory");
		}
		return true;
	}

	private bool OnProcessCheat_SetKCProgress(string func, string[] args, string rawArgs)
	{
		if (SetAdventureProgressCommon(AdventureDbId.LOOT, AdventureModeDbId.DUNGEON_CRAWL, args, SetAdventureProgressMode.Progress))
		{
			UIStatus.Get().AddInfo($"Set KC progress");
		}
		return true;
	}

	private bool OnProcessCheat_SetKCDefeat(string func, string[] args, string rawArgs)
	{
		if (SetAdventureProgressCommon(AdventureDbId.LOOT, AdventureModeDbId.DUNGEON_CRAWL, args, SetAdventureProgressMode.Defeat))
		{
			UIStatus.Get().AddInfo($"Set KC defeat");
		}
		return true;
	}

	private bool OnProcessCheat_SetGILVictory(string func, string[] args, string rawArgs)
	{
		if (SetAdventureProgressCommon(AdventureDbId.GIL, AdventureModeDbId.DUNGEON_CRAWL, args, SetAdventureProgressMode.Victory))
		{
			UIStatus.Get().AddInfo($"Set Witchwood victory");
		}
		return true;
	}

	private bool OnProcessCheat_SetGILProgress(string func, string[] args, string rawArgs)
	{
		if (SetAdventureProgressCommon(AdventureDbId.GIL, AdventureModeDbId.DUNGEON_CRAWL, args, SetAdventureProgressMode.Progress))
		{
			UIStatus.Get().AddInfo($"Set Witchwood progress");
		}
		return true;
	}

	private bool OnProcessCheat_SetGILDefeat(string func, string[] args, string rawArgs)
	{
		if (SetAdventureProgressCommon(AdventureDbId.GIL, AdventureModeDbId.DUNGEON_CRAWL, args, SetAdventureProgressMode.Defeat))
		{
			UIStatus.Get().AddInfo($"Set Witchwood defeat");
		}
		return true;
	}

	private bool OnProcessCheat_SetGILBonus(string func, string[] args, string rawArgs)
	{
		OnProcessCheat_utilservercmd("util", new string[4] { "quest", "progress", "achieve=1010", "amount=4" }, "util quest progress achieve=1010 amount=4", null);
		UIStatus.Get().AddInfo($"Set Witchwood Bonus Challenge Active");
		Options.Get().SetBool(Option.HAS_SEEN_GIL_BONUS_CHALLENGE, val: false);
		return true;
	}

	private bool OnProcessCheat_SetTRLVictory(string func, string[] args, string rawArgs)
	{
		if (SetAdventureProgressCommon(AdventureDbId.TRL, AdventureModeDbId.DUNGEON_CRAWL, args, SetAdventureProgressMode.Victory))
		{
			UIStatus.Get().AddInfo($"Set Rastakhan's Rumble victory");
		}
		return true;
	}

	private bool OnProcessCheat_SetTRLProgress(string func, string[] args, string rawArgs)
	{
		if (SetAdventureProgressCommon(AdventureDbId.TRL, AdventureModeDbId.DUNGEON_CRAWL, args, SetAdventureProgressMode.Progress))
		{
			UIStatus.Get().AddInfo($"Set Rastakhan's Rumble progress");
		}
		return true;
	}

	private bool OnProcessCheat_SetTRLDefeat(string func, string[] args, string rawArgs)
	{
		if (SetAdventureProgressCommon(AdventureDbId.TRL, AdventureModeDbId.DUNGEON_CRAWL, args, SetAdventureProgressMode.Defeat))
		{
			UIStatus.Get().AddInfo($"Set Rastakhan's Rumble defeat");
		}
		return true;
	}

	private bool OnProcessCheat_SetDALProgress(string func, string[] args, string rawArgs)
	{
		if (SetAdventureProgressCommon(AdventureDbId.DALARAN, AdventureModeDbId.DUNGEON_CRAWL, args, SetAdventureProgressMode.Progress))
		{
			UIStatus.Get().AddInfo($"Set Dalaran progress");
		}
		return true;
	}

	private bool OnProcessCheat_SetDALVictory(string func, string[] args, string rawArgs)
	{
		if (SetAdventureProgressCommon(AdventureDbId.DALARAN, AdventureModeDbId.DUNGEON_CRAWL, args, SetAdventureProgressMode.Victory))
		{
			UIStatus.Get().AddInfo($"Set Dalaran victory");
		}
		return true;
	}

	private bool OnProcessCheat_SetDALDefeat(string func, string[] args, string rawArgs)
	{
		if (SetAdventureProgressCommon(AdventureDbId.DALARAN, AdventureModeDbId.DUNGEON_CRAWL, args, SetAdventureProgressMode.Defeat))
		{
			UIStatus.Get().AddInfo($"Set Dalaran defeat");
		}
		return true;
	}

	private bool OnProcessCheat_ResetDalaranAdventure(string func, string[] args, string rawArgs)
	{
		return ResetDungeonRunAdventure(AdventureDbId.DALARAN, AdventureModeDbId.DUNGEON_CRAWL);
	}

	private bool OnProcessCheat_ResetTavernBrawlAdventure(string func, string[] args, string rawArgs)
	{
		if (TavernBrawlManager.Get() == null)
		{
			UIStatus.Get().AddError("TavernBrawlManager is not initialized!");
			return true;
		}
		TavernBrawlMission mission = TavernBrawlManager.Get().GetMission(BrawlType.BRAWL_TYPE_TAVERN_BRAWL);
		if (mission == null)
		{
			UIStatus.Get().AddError("No Tavern Brawl Mission found");
			return true;
		}
		ScenarioDbfRecord scen = GameDbf.Scenario.GetRecord(mission.missionId);
		if (scen == null)
		{
			UIStatus.Get().AddError("Could not find scenario for current tavern brawl mission");
			return true;
		}
		AdventureDataDbfRecord adventureDataRecord = GameUtils.GetAdventureDataRecord(scen.AdventureId, scen.ModeId);
		if (adventureDataRecord == null)
		{
			UIStatus.Get().AddError("Could not find adventure data for current tavern brawl mission");
			return true;
		}
		ResetAdventureRunCommon_Server(adventureDataRecord.GameSaveDataServerKey);
		ResetAdventureRunCommon_Client(adventureDataRecord.GameSaveDataClientKey);
		UIStatus.Get().AddInfo($"Reset Tavern Brawl Adventure Progress");
		return true;
	}

	private bool OnProcessCheat_SetDALHeroicProgress(string func, string[] args, string rawArgs)
	{
		if (SetAdventureProgressCommon(AdventureDbId.DALARAN, AdventureModeDbId.DUNGEON_CRAWL_HEROIC, args, SetAdventureProgressMode.Progress))
		{
			UIStatus.Get().AddInfo($"Set Dalaran Heroic progress");
		}
		return true;
	}

	private bool OnProcessCheat_SetDALHeroicVictory(string func, string[] args, string rawArgs)
	{
		if (SetAdventureProgressCommon(AdventureDbId.DALARAN, AdventureModeDbId.DUNGEON_CRAWL_HEROIC, args, SetAdventureProgressMode.Victory))
		{
			UIStatus.Get().AddInfo($"Set Dalaran Heroic victory");
		}
		return true;
	}

	private bool OnProcessCheat_SetDALHeroicDefeat(string func, string[] args, string rawArgs)
	{
		if (SetAdventureProgressCommon(AdventureDbId.DALARAN, AdventureModeDbId.DUNGEON_CRAWL_HEROIC, args, SetAdventureProgressMode.Defeat))
		{
			UIStatus.Get().AddInfo($"Set Dalaran Heroic defeat");
		}
		return true;
	}

	private bool OnProcessCheat_ResetDalaranHeroicAdventure(string func, string[] args, string rawArgs)
	{
		return ResetDungeonRunAdventure(AdventureDbId.DALARAN, AdventureModeDbId.DUNGEON_CRAWL_HEROIC);
	}

	private bool OnProcessCheat_SetULDProgress(string func, string[] args, string rawArgs)
	{
		if (SetAdventureProgressCommon(AdventureDbId.ULDUM, AdventureModeDbId.DUNGEON_CRAWL, args, SetAdventureProgressMode.Progress))
		{
			UIStatus.Get().AddInfo($"Set Uldum progress");
		}
		return true;
	}

	private bool OnProcessCheat_SetULDVictory(string func, string[] args, string rawArgs)
	{
		if (SetAdventureProgressCommon(AdventureDbId.ULDUM, AdventureModeDbId.DUNGEON_CRAWL, args, SetAdventureProgressMode.Victory))
		{
			UIStatus.Get().AddInfo($"Set Uldum victory");
		}
		return true;
	}

	private bool OnProcessCheat_SetULDDefeat(string func, string[] args, string rawArgs)
	{
		if (SetAdventureProgressCommon(AdventureDbId.ULDUM, AdventureModeDbId.DUNGEON_CRAWL, args, SetAdventureProgressMode.Defeat))
		{
			UIStatus.Get().AddInfo($"Set Uldum defeat");
		}
		return true;
	}

	private bool OnProcessCheat_ResetUldumAdventure(string func, string[] args, string rawArgs)
	{
		return ResetDungeonRunAdventure(AdventureDbId.ULDUM, AdventureModeDbId.DUNGEON_CRAWL);
	}

	private bool OnProcessCheat_SetULDHeroicProgress(string func, string[] args, string rawArgs)
	{
		if (SetAdventureProgressCommon(AdventureDbId.ULDUM, AdventureModeDbId.DUNGEON_CRAWL_HEROIC, args, SetAdventureProgressMode.Progress))
		{
			UIStatus.Get().AddInfo($"Set Uldum Heroic progress");
		}
		return true;
	}

	private bool OnProcessCheat_SetULDHeroicVictory(string func, string[] args, string rawArgs)
	{
		if (SetAdventureProgressCommon(AdventureDbId.ULDUM, AdventureModeDbId.DUNGEON_CRAWL_HEROIC, args, SetAdventureProgressMode.Victory))
		{
			UIStatus.Get().AddInfo($"Set Uldum Heroic victory");
		}
		return true;
	}

	private bool OnProcessCheat_SetULDHeroicDefeat(string func, string[] args, string rawArgs)
	{
		if (SetAdventureProgressCommon(AdventureDbId.ULDUM, AdventureModeDbId.DUNGEON_CRAWL_HEROIC, args, SetAdventureProgressMode.Defeat))
		{
			UIStatus.Get().AddInfo($"Set Uldum Heroic defeat");
		}
		return true;
	}

	private bool OnProcessCheat_ResetUldumHeroicAdventure(string func, string[] args, string rawArgs)
	{
		return ResetDungeonRunAdventure(AdventureDbId.ULDUM, AdventureModeDbId.DUNGEON_CRAWL_HEROIC);
	}

	private bool OnProcessCheat_ResetGILAdventure(string func, string[] args, string rawArgs)
	{
		return ResetDungeonRunAdventure(AdventureDbId.GIL, AdventureModeDbId.DUNGEON_CRAWL);
	}

	private static AdventureDbId ParseAdventureDbIdFromArgs(string[] args, int index)
	{
		AdventureDbId adventure = AdventureDbId.INVALID;
		if (args.Length <= index || string.IsNullOrEmpty(args[index]))
		{
			UIStatus.Get().AddError("You must provide an Adventure to operate on!  Ex: 'uld'");
			return adventure;
		}
		adventure = GetAdventureDbIdFromString(args[index]);
		if (adventure == AdventureDbId.INVALID)
		{
			UIStatus.Get().AddError($"{args[index]} does not map to a valid Adventure!");
			return adventure;
		}
		return adventure;
	}

	private static AdventureDbId GetAdventureDbIdFromString(string adventureString)
	{
		if (string.IsNullOrEmpty(adventureString))
		{
			return AdventureDbId.INVALID;
		}
		AdventureDbId adventure = AdventureDbId.INVALID;
		try
		{
			adventure = (AdventureDbId)Enum.Parse(typeof(AdventureDbId), adventureString, ignoreCase: true);
		}
		catch (ArgumentException)
		{
		}
		if (adventure != 0)
		{
			return adventure;
		}
		switch (adventureString.ToLower())
		{
		case "nax":
		case "naxx":
			return AdventureDbId.NAXXRAMAS;
		case "league":
			return AdventureDbId.LOE;
		case "karazhan":
			return AdventureDbId.KARA;
		case "icecrown":
			return AdventureDbId.ICC;
		case "kc":
		case "k&c":
		case "knc":
			return AdventureDbId.LOOT;
		case "witchwood":
			return AdventureDbId.GIL;
		case "rastakhan":
			return AdventureDbId.TRL;
		case "dal":
			return AdventureDbId.DALARAN;
		case "uld":
		case "tot":
			return AdventureDbId.ULDUM;
		case "ga":
		case "drg":
			return AdventureDbId.DRAGONS;
		default:
			return AdventureDbId.INVALID;
		}
	}

	private bool OnProcessCheat_UnlockLoadout(string func, string[] args, string rawArgs)
	{
		return UpdateAdventureLoadoutOptionsLockStateFromArgs(args, shouldLock: false);
	}

	private bool OnProcessCheat_LockLoadout(string func, string[] args, string rawArgs)
	{
		return UpdateAdventureLoadoutOptionsLockStateFromArgs(args, shouldLock: true);
	}

	private bool OnProcessCheat_ShowAdventureLoadingPopup(string func, string[] args, string rawArgs)
	{
		GameMgr.Get().Cheat_ShowTransitionPopup(GameType.GT_VS_AI, FormatType.FT_WILD, (int)AdventureConfig.Get().GetMission());
		if (AdventureConfig.Get().GetMission() == ScenarioDbId.INVALID)
		{
			UIStatus.Get().AddInfo("Showing generic popup, navigate to an Adventure scenario to customize the popup");
		}
		else
		{
			UIStatus.Get().AddInfo($"Showing loading popup for scenario {(int)AdventureConfig.Get().GetMission()}");
		}
		return true;
	}

	private bool OnProcessCheat_HideGameTransitionPopup(string func, string[] args, string rawArgs)
	{
		GameMgr.Get().HideTransitionPopup();
		UIStatus.Get().AddInfo("Hiding Transition Popup");
		return true;
	}

	private static GameSaveKeyId GetGameSaveServerKeyForAdventure(AdventureDbId adventureDbId, AdventureModeDbId adventureMode)
	{
		AdventureDataDbfRecord adventureDataRecord = GameDbf.AdventureData.GetRecord((AdventureDataDbfRecord r) => r.AdventureId == (int)adventureDbId && r.ModeId == (int)adventureMode);
		if (adventureDataRecord == null)
		{
			Debug.LogErrorFormat("No AdventureDataDbfRecord found for Adventure {0} Mode {1}, unable to unlock loadout options!", adventureDbId, adventureMode);
			return GameSaveKeyId.INVALID;
		}
		return (GameSaveKeyId)adventureDataRecord.GameSaveDataServerKey;
	}

	private bool UpdateAdventureLoadoutOptionsLockStateFromArgs(string[] args, bool shouldLock)
	{
		AdventureDbId adventure = ParseAdventureDbIdFromArgs(args, 0);
		if (adventure == AdventureDbId.INVALID)
		{
			return true;
		}
		GameSaveKeyId normalServerKey = GetGameSaveServerKeyForAdventure(adventure, AdventureModeDbId.DUNGEON_CRAWL);
		if (normalServerKey == (GameSaveKeyId)0)
		{
			UIStatus.Get().AddError("No ServerKey found for Adventure " + adventure.ToString() + " Mode " + AdventureModeDbId.DUNGEON_CRAWL.ToString() + ", unable to unlock loadout options!");
			return true;
		}
		List<GameSaveKeyId> keys = new List<GameSaveKeyId> { normalServerKey };
		GameSaveKeyId heroicServerKey = GetGameSaveServerKeyForAdventure(adventure, AdventureModeDbId.DUNGEON_CRAWL_HEROIC);
		if (heroicServerKey != 0)
		{
			keys.Add(heroicServerKey);
		}
		GameSaveDataManager.Get().Request(keys, delegate(bool success)
		{
			UpdateAdventureLoadoutOptionsLockStateCommon(adventure, normalServerKey, shouldLock);
			if (heroicServerKey != 0)
			{
				UpdateAdventureLoadoutOptionsLockStateCommon(adventure, heroicServerKey, shouldLock);
			}
			if (!success)
			{
				UIStatus.Get().AddInfo("Failed to request ServerKeys for Adventure " + adventure.ToString() + ", not all loadout options may be unlocked properly!");
			}
			else
			{
				UIStatus.Get().AddInfo(string.Format("{0} Loadout {1}", shouldLock ? "Lock" : "Unlock", adventure));
			}
		});
		return true;
	}

	private void UpdateLockSubkey(GameSaveKeyId serverKey, GameSaveKeySubkeyId subkey, long unlockValue, bool shouldLock)
	{
		if (serverKey != 0 && subkey != 0)
		{
			GameSaveDataManager.Get().GetSubkeyValue(serverKey, subkey, out long currentValue);
			if (shouldLock && currentValue != 0L)
			{
				InvokeSetGameSaveDataCheat(serverKey, subkey, 0L);
			}
			else if (!shouldLock && currentValue < unlockValue)
			{
				InvokeSetGameSaveDataCheat(serverKey, subkey, unlockValue);
			}
		}
	}

	private void UpdateAdventureLoadoutOptionsLockStateCommon(AdventureDbId adventureDbId, GameSaveKeyId serverKey, bool shouldLock)
	{
		foreach (AdventureHeroPowerDbfRecord record in GameDbf.AdventureHeroPower.GetRecords((AdventureHeroPowerDbfRecord r) => r.AdventureId == (int)adventureDbId))
		{
			UpdateLockSubkey(serverKey, (GameSaveKeySubkeyId)record.UnlockGameSaveSubkey, record.UnlockValue, shouldLock);
		}
		foreach (AdventureDeckDbfRecord record2 in GameDbf.AdventureDeck.GetRecords((AdventureDeckDbfRecord r) => r.AdventureId == (int)adventureDbId))
		{
			UpdateLockSubkey(serverKey, (GameSaveKeySubkeyId)record2.UnlockGameSaveSubkey, record2.UnlockValue, shouldLock);
		}
		foreach (AdventureLoadoutTreasuresDbfRecord record3 in GameDbf.AdventureLoadoutTreasures.GetRecords((AdventureLoadoutTreasuresDbfRecord r) => r.AdventureId == (int)adventureDbId))
		{
			UpdateLockSubkey(serverKey, (GameSaveKeySubkeyId)record3.UnlockGameSaveSubkey, record3.UnlockValue, shouldLock);
			UpdateLockSubkey(serverKey, (GameSaveKeySubkeyId)record3.UpgradeGameSaveSubkey, record3.UpgradeValue, shouldLock);
		}
	}

	private void ResetAdventureRunCommon_Server(int key)
	{
		InvokeSetGameSaveDataCheat(key, GameSaveKeySubkeyId.DUNGEON_CRAWL_PLAYER_SELECTED_SCENARIO_ID, new long[0]);
		InvokeSetGameSaveDataCheat(key, GameSaveKeySubkeyId.DUNGEON_CRAWL_SCENARIO_ID, new long[0]);
		InvokeSetGameSaveDataCheat(key, GameSaveKeySubkeyId.DUNGEON_CRAWL_PLAYER_SELECTED_LOADOUT_TREASURE_ID, new long[0]);
		InvokeSetGameSaveDataCheat(key, GameSaveKeySubkeyId.DUNGEON_CRAWL_PLAYER_SELECTED_HERO_POWER, new long[0]);
		InvokeSetGameSaveDataCheat(key, GameSaveKeySubkeyId.DUNGEON_CRAWL_HERO_POWER, new long[0]);
		InvokeSetGameSaveDataCheat(key, GameSaveKeySubkeyId.DUNGEON_CRAWL_PLAYER_SELECTED_DECK, new long[0]);
		InvokeSetGameSaveDataCheat(key, GameSaveKeySubkeyId.DUNGEON_CRAWL_PLAYER_SELECTED_ANOMALY_MODE, new long[0]);
		InvokeSetGameSaveDataCheat(key, GameSaveKeySubkeyId.DUNGEON_CRAWL_IS_ANOMALY_MODE, new long[0]);
		InvokeSetGameSaveDataCheat(key, GameSaveKeySubkeyId.DUNGEON_CRAWL_BOSS_LOST_TO, new long[0]);
		InvokeSetGameSaveDataCheat(key, GameSaveKeySubkeyId.DUNGEON_CRAWL_BOSSES_DEFEATED, new long[0]);
		InvokeSetGameSaveDataCheat(key, GameSaveKeySubkeyId.DUNGEON_CRAWL_DECK_CARD_LIST, new long[0]);
		InvokeSetGameSaveDataCheat(key, GameSaveKeySubkeyId.DUNGEON_CRAWL_DECK_CLASS, new long[0]);
		InvokeSetGameSaveDataCheat(key, GameSaveKeySubkeyId.DUNGEON_CRAWL_BOSSES_FOUGHT_LIST, new long[0]);
		InvokeSetGameSaveDataCheat(key, GameSaveKeySubkeyId.DUNGEON_CRAWL_NEXT_BOSS_FIGHT, new long[0]);
		InvokeSetGameSaveDataCheat(key, GameSaveKeySubkeyId.DUNGEON_CRAWL_IS_NEXT_BOSS_FIGHT_UNDEFEATED, new long[0]);
		InvokeSetGameSaveDataCheat(key, GameSaveKeySubkeyId.DUNGEON_CRAWL_LOOT_OPTION_A, new long[0]);
		InvokeSetGameSaveDataCheat(key, GameSaveKeySubkeyId.DUNGEON_CRAWL_LOOT_OPTION_B, new long[0]);
		InvokeSetGameSaveDataCheat(key, GameSaveKeySubkeyId.DUNGEON_CRAWL_LOOT_OPTION_C, new long[0]);
		InvokeSetGameSaveDataCheat(key, GameSaveKeySubkeyId.DUNGEON_CRAWL_TREASURE_OPTION, new long[0]);
		InvokeSetGameSaveDataCheat(key, GameSaveKeySubkeyId.DUNGEON_CRAWL_SHRINE_OPTIONS, new long[0]);
		InvokeSetGameSaveDataCheat(key, GameSaveKeySubkeyId.DUNGEON_CRAWL_LOOT_HISTORY, new long[0]);
		InvokeSetGameSaveDataCheat(key, GameSaveKeySubkeyId.DUNGEON_CRAWL_IS_RUN_ACTIVE, new long[0]);
		InvokeSetGameSaveDataCheat(key, GameSaveKeySubkeyId.DUNGEON_CRAWL_IS_RUN_RETIRED, new long[0]);
		InvokeSetGameSaveDataCheat(key, GameSaveKeySubkeyId.DUNGEON_CRAWL_PLAYER_CHOSEN_LOOT, new long[0]);
		InvokeSetGameSaveDataCheat(key, GameSaveKeySubkeyId.DUNGEON_CRAWL_PLAYER_CHOSEN_TREASURE, new long[0]);
		InvokeSetGameSaveDataCheat(key, GameSaveKeySubkeyId.DUNGEON_CRAWL_PLAYER_CHOSEN_SHRINE, new long[0]);
		InvokeSetGameSaveDataCheat(key, GameSaveKeySubkeyId.DUNGEON_CRAWL_NEXT_BOSS_HEALTH, new long[0]);
		InvokeSetGameSaveDataCheat(key, GameSaveKeySubkeyId.DUNGEON_CRAWL_HERO_HEALTH, new long[0]);
		InvokeSetGameSaveDataCheat(key, GameSaveKeySubkeyId.DUNGEON_CRAWL_HAS_SEEN_EVENT_1, new long[0]);
		InvokeSetGameSaveDataCheat(key, GameSaveKeySubkeyId.DUNGEON_CRAWL_HAS_SEEN_EVENT_2, new long[0]);
		InvokeSetGameSaveDataCheat(key, GameSaveKeySubkeyId.DUNGEON_CRAWL_SCENARIO_OVERRIDE, new long[0]);
		InvokeSetGameSaveDataCheat(key, GameSaveKeySubkeyId.DUNGEON_CRAWL_PLAYER_SELECTED_HERO_CLASS, new long[0]);
		InvokeSetGameSaveDataCheat(key, GameSaveKeySubkeyId.DUNGEON_CRAWL_PLAYER_SELECTED_LOADOUT_TREASURE_ID, new long[0]);
		InvokeSetGameSaveDataCheat(key, GameSaveKeySubkeyId.DUELS_DRAFT_HERO_CHOICES, new long[0]);
	}

	private void ResetAdventureRunCommon_Client(int key)
	{
	}

	private bool SetAdventureProgressCommon(AdventureDbId adventureDbId, AdventureModeDbId adventureMode, string[] args, SetAdventureProgressMode mode)
	{
		AdventureDataDbfRecord adventureDataRecord = GameDbf.AdventureData.GetRecord((AdventureDataDbfRecord r) => r.AdventureId == (int)adventureDbId && r.ModeId == (int)adventureMode);
		if (adventureDataRecord == null)
		{
			UIStatus.Get().AddError("No AdventureDataDbfRecord found for Adventure " + adventureDbId.ToString() + " Mode " + adventureMode.ToString() + ", unable to set Adventure progress!");
			return false;
		}
		long numDesiredBosses = 0L;
		if (mode != 0 && !ValidateAndParseLongAtIndex(0, args, out numDesiredBosses))
		{
			UIStatus.Get().AddError("You must provide a valid number of bosses defeated!");
			return false;
		}
		GameSaveKeyId serverKey = (GameSaveKeyId)adventureDataRecord.GameSaveDataServerKey;
		long isRunActive = 0L;
		GameSaveDataManager.Get().GetSubkeyValue(serverKey, GameSaveKeySubkeyId.DUNGEON_CRAWL_IS_RUN_ACTIVE, out isRunActive);
		bool useLoadoutOfActiveRun = isRunActive > 0;
		if (!useLoadoutOfActiveRun)
		{
			long heroClass = 0L;
			if (GameSaveDataManager.Get().GetSubkeyValue(serverKey, GameSaveKeySubkeyId.DUNGEON_CRAWL_PLAYER_SELECTED_HERO_CLASS, out heroClass) && (int)heroClass != 0)
			{
				InvokeSetGameSaveDataCheat(serverKey, GameSaveKeySubkeyId.DUNGEON_CRAWL_DECK_CLASS, new long[1] { heroClass });
			}
		}
		long deckClass = 0L;
		if (!GameSaveDataManager.Get().GetSubkeyValue(serverKey, GameSaveKeySubkeyId.DUNGEON_CRAWL_DECK_CLASS, out deckClass) || (int)deckClass == 0)
		{
			deckClass = 4L;
			HashSet<TAG_CLASS> heroClasses = new HashSet<TAG_CLASS>(GameUtils.ORDERED_HERO_CLASSES);
			List<AdventureGuestHeroesDbfRecord> records = GameDbf.AdventureGuestHeroes.GetRecords((AdventureGuestHeroesDbfRecord r) => r.AdventureId == (int)adventureDbId);
			GuestHeroDbfRecord guestHeroRecord = null;
			foreach (AdventureGuestHeroesDbfRecord advGuestHeroRecord in records)
			{
				GuestHeroDbfRecord record = GameDbf.GuestHero.GetRecord(advGuestHeroRecord.GuestHeroId);
				if (heroClasses.Contains(GameUtils.GetTagClassFromCardDbId(record.CardId)))
				{
					guestHeroRecord = record;
					break;
				}
			}
			if (guestHeroRecord != null)
			{
				TAG_CLASS heroTag = GameUtils.GetTagClassFromCardDbId(guestHeroRecord.CardId);
				if (heroTag != 0)
				{
					deckClass = (long)heroTag;
				}
			}
			InvokeSetGameSaveDataCheat(serverKey, GameSaveKeySubkeyId.DUNGEON_CRAWL_DECK_CLASS, new long[1] { deckClass });
		}
		GameSaveDataManager.Get().GetSubkeyValue((GameSaveKeyId)adventureDataRecord.GameSaveDataServerKey, GameSaveKeySubkeyId.DUNGEON_CRAWL_SCENARIO_ID, out long scenarioID);
		WingDbId wingId = GameUtils.GetWingIdFromMissionId((ScenarioDbId)scenarioID);
		long adventureScenarioId = 0L;
		if (adventureDataRecord != null && adventureDataRecord.DungeonCrawlSelectChapter)
		{
			if (!useLoadoutOfActiveRun)
			{
				adventureScenarioId = (long)AdventureConfig.Get().GetMission();
				if (adventureScenarioId <= 0)
				{
					GameSaveDataManager.Get().GetSubkeyValue(serverKey, GameSaveKeySubkeyId.DUNGEON_CRAWL_PLAYER_SELECTED_SCENARIO_ID, out adventureScenarioId);
				}
				if (adventureScenarioId > 0)
				{
					InvokeSetGameSaveDataCheat(serverKey, GameSaveKeySubkeyId.DUNGEON_CRAWL_SCENARIO_ID, new long[1] { adventureScenarioId });
				}
			}
		}
		else if (adventureDbId == AdventureDbId.BOH || adventureDbId == AdventureDbId.BOM)
		{
			ScenarioDbId[] scenariosForClass = ((adventureDbId == AdventureDbId.BOH) ? (wingId switch
			{
				WingDbId.BOH_REXXAR => new ScenarioDbId[8]
				{
					ScenarioDbId.BOH_REXXAR_01,
					ScenarioDbId.BOH_REXXAR_02,
					ScenarioDbId.BOH_REXXAR_03,
					ScenarioDbId.BOH_REXXAR_04,
					ScenarioDbId.BOH_REXXAR_05,
					ScenarioDbId.BOH_REXXAR_06,
					ScenarioDbId.BOH_REXXAR_07,
					ScenarioDbId.BOH_REXXAR_08
				}, 
				WingDbId.BOH_GARROSH => new ScenarioDbId[8]
				{
					ScenarioDbId.BOH_GARROSH_01,
					ScenarioDbId.BOH_GARROSH_02,
					ScenarioDbId.BOH_GARROSH_03,
					ScenarioDbId.BOH_GARROSH_04,
					ScenarioDbId.BOH_GARROSH_05,
					ScenarioDbId.BOH_GARROSH_06,
					ScenarioDbId.BOH_GARROSH_07,
					ScenarioDbId.BOH_GARROSH_08
				}, 
				WingDbId.BOH_UTHER => new ScenarioDbId[8]
				{
					ScenarioDbId.BOH_UTHER_01,
					ScenarioDbId.BOH_UTHER_02,
					ScenarioDbId.BOH_UTHER_03,
					ScenarioDbId.BOH_UTHER_04,
					ScenarioDbId.BOH_UTHER_05,
					ScenarioDbId.BOH_UTHER_06,
					ScenarioDbId.BOH_UTHER_07,
					ScenarioDbId.BOH_UTHER_08
				}, 
				WingDbId.BOH_ANDUIN => new ScenarioDbId[8]
				{
					ScenarioDbId.BOH_ANDUIN_01,
					ScenarioDbId.BOH_ANDUIN_02,
					ScenarioDbId.BOH_ANDUIN_03,
					ScenarioDbId.BOH_ANDUIN_04,
					ScenarioDbId.BOH_ANDUIN_05,
					ScenarioDbId.BOH_ANDUIN_06,
					ScenarioDbId.BOH_ANDUIN_07,
					ScenarioDbId.BOH_ANDUIN_08
				}, 
				WingDbId.BOH_VALEERA => new ScenarioDbId[8]
				{
					ScenarioDbId.BOH_VALEERA_01,
					ScenarioDbId.BOH_VALEERA_02,
					ScenarioDbId.BOH_VALEERA_03,
					ScenarioDbId.BOH_VALEERA_04,
					ScenarioDbId.BOH_VALEERA_05,
					ScenarioDbId.BOH_VALEERA_06,
					ScenarioDbId.BOH_VALEERA_07,
					ScenarioDbId.BOH_VALEERA_08
				}, 
				WingDbId.BOH_THRALL => new ScenarioDbId[8]
				{
					ScenarioDbId.BOH_THRALL_01,
					ScenarioDbId.BOH_THRALL_02,
					ScenarioDbId.BOH_THRALL_03,
					ScenarioDbId.BOH_THRALL_04,
					ScenarioDbId.BOH_THRALL_05,
					ScenarioDbId.BOH_THRALL_06,
					ScenarioDbId.BOH_THRALL_07,
					ScenarioDbId.BOH_THRALL_08
				}, 
				WingDbId.BOH_MALFURION => new ScenarioDbId[8]
				{
					ScenarioDbId.BOH_MALFURION_01,
					ScenarioDbId.BOH_MALFURION_02,
					ScenarioDbId.BOH_MALFURION_03,
					ScenarioDbId.BOH_MALFURION_04,
					ScenarioDbId.BOH_MALFURION_05,
					ScenarioDbId.BOH_MALFURION_06,
					ScenarioDbId.BOH_MALFURION_07,
					ScenarioDbId.BOH_MALFURION_08
				}, 
				WingDbId.BOH_GULDAN => new ScenarioDbId[8]
				{
					ScenarioDbId.BOH_GULDAN_01,
					ScenarioDbId.BOH_GULDAN_02,
					ScenarioDbId.BOH_GULDAN_03,
					ScenarioDbId.BOH_GULDAN_04,
					ScenarioDbId.BOH_GULDAN_05,
					ScenarioDbId.BOH_GULDAN_06,
					ScenarioDbId.BOH_GULDAN_07,
					ScenarioDbId.BOH_GULDAN_08
				}, 
				WingDbId.BOH_ILLIDAN => new ScenarioDbId[8]
				{
					ScenarioDbId.BOH_ILLIDAN_01,
					ScenarioDbId.BOH_ILLIDAN_02,
					ScenarioDbId.BOH_ILLIDAN_03,
					ScenarioDbId.BOH_ILLIDAN_04,
					ScenarioDbId.BOH_ILLIDAN_05,
					ScenarioDbId.BOH_ILLIDAN_06,
					ScenarioDbId.BOH_ILLIDAN_07,
					ScenarioDbId.BOH_ILLIDAN_08
				}, 
				WingDbId.BOH_FAELIN => new ScenarioDbId[19]
				{
					ScenarioDbId.BOH_FAELIN_01,
					ScenarioDbId.BOH_FAELIN_02,
					ScenarioDbId.BOH_FAELIN_03,
					ScenarioDbId.BOH_FAELIN_04,
					ScenarioDbId.BOH_FAELIN_05A,
					ScenarioDbId.BOH_FAELIN_05B,
					ScenarioDbId.BOH_FAELIN_06,
					ScenarioDbId.BOH_FAELIN_07,
					ScenarioDbId.BOH_FAELIN_08,
					ScenarioDbId.BOH_FAELIN_09A,
					ScenarioDbId.BOH_FAELIN_09B,
					ScenarioDbId.BOH_FAELIN_10A,
					ScenarioDbId.BOH_FAELIN_10B,
					ScenarioDbId.BOH_FAELIN_11,
					ScenarioDbId.BOH_FAELIN_12,
					ScenarioDbId.BOH_FAELIN_13,
					ScenarioDbId.BOH_FAELIN_14,
					ScenarioDbId.BOH_FAELIN_15,
					ScenarioDbId.BOH_FAELIN_16
				}, 
				_ => new ScenarioDbId[8]
				{
					ScenarioDbId.BOH_JAINA_01,
					ScenarioDbId.BOH_JAINA_02,
					ScenarioDbId.BOH_JAINA_03,
					ScenarioDbId.BOH_JAINA_04,
					ScenarioDbId.BOH_JAINA_05,
					ScenarioDbId.BOH_JAINA_06,
					ScenarioDbId.BOH_JAINA_07,
					ScenarioDbId.BOH_JAINA_08
				}, 
			}) : (wingId switch
			{
				WingDbId.BOM_Xyrella => new ScenarioDbId[8]
				{
					ScenarioDbId.BOM_02_Xyrella_01,
					ScenarioDbId.BOM_02_Xyrella_02,
					ScenarioDbId.BOM_02_Xyrella_03,
					ScenarioDbId.BOM_02_Xyrella_04,
					ScenarioDbId.BOM_02_Xyrella_05,
					ScenarioDbId.BOM_02_Xyrella_06,
					ScenarioDbId.BOM_02_Xyrella_07,
					ScenarioDbId.BOM_02_Xyrella_08
				}, 
				WingDbId.BOM_Guff => new ScenarioDbId[8]
				{
					ScenarioDbId.BOM_03_Guff_01,
					ScenarioDbId.BOM_03_Guff_02,
					ScenarioDbId.BOM_03_Guff_03,
					ScenarioDbId.BOM_03_Guff_04,
					ScenarioDbId.BOM_03_Guff_05,
					ScenarioDbId.BOM_03_Guff_06,
					ScenarioDbId.BOM_03_Guff_07,
					ScenarioDbId.BOM_03_Guff_08
				}, 
				WingDbId.BOM_Kurtrus => new ScenarioDbId[8]
				{
					ScenarioDbId.BOM_04_Kurtrus_01,
					ScenarioDbId.BOM_04_Kurtrus_02,
					ScenarioDbId.BOM_04_Kurtrus_03,
					ScenarioDbId.BOM_04_Kurtrus_04,
					ScenarioDbId.BOM_04_Kurtrus_05,
					ScenarioDbId.BOM_04_Kurtrus_06,
					ScenarioDbId.BOM_04_Kurtrus_07,
					ScenarioDbId.BOM_04_Kurtrus_08
				}, 
				WingDbId.BOM_Tamsin => new ScenarioDbId[8]
				{
					ScenarioDbId.BOM_05_Tamsin_001,
					ScenarioDbId.BOM_05_Tamsin_002,
					ScenarioDbId.BOM_05_Tamsin_003,
					ScenarioDbId.BOM_05_Tamsin_004,
					ScenarioDbId.BOM_05_Tamsin_005,
					ScenarioDbId.BOM_05_Tamsin_006,
					ScenarioDbId.BOM_05_Tamsin_007,
					ScenarioDbId.BOM_05_Tamsin_008
				}, 
				WingDbId.BOM_Cariel => new ScenarioDbId[8]
				{
					ScenarioDbId.BOM_06_Cariel_001,
					ScenarioDbId.BOM_06_Cariel_002,
					ScenarioDbId.BOM_06_Cariel_003,
					ScenarioDbId.BOM_06_Cariel_004,
					ScenarioDbId.BOM_06_Cariel_005,
					ScenarioDbId.BOM_06_Cariel_006,
					ScenarioDbId.BOM_06_Cariel_007,
					ScenarioDbId.BOM_06_Cariel_008
				}, 
				WingDbId.BOM_Scabbs => new ScenarioDbId[8]
				{
					ScenarioDbId.BOM_07_Scabbs_Fight_001,
					ScenarioDbId.BOM_07_Scabbs_Fight_002,
					ScenarioDbId.BOM_07_Scabbs_Fight_003,
					ScenarioDbId.BOM_07_Scabbs_Fight_004,
					ScenarioDbId.BOM_07_Scabbs_Fight_005,
					ScenarioDbId.BOM_07_Scabbs_Fight_006,
					ScenarioDbId.BOM_07_Scabbs_Fight_007,
					ScenarioDbId.BOM_07_Scabbs_Fight_008
				}, 
				WingDbId.BOM_Tavish => new ScenarioDbId[8]
				{
					ScenarioDbId.BOM_08_Tavish_Fight_001,
					ScenarioDbId.BOM_08_Tavish_Fight_002,
					ScenarioDbId.BOM_08_Tavish_Fight_003,
					ScenarioDbId.BOM_08_Tavish_Fight_004,
					ScenarioDbId.BOM_08_Tavish_Fight_005,
					ScenarioDbId.BOM_08_Tavish_Fight_006,
					ScenarioDbId.BOM_08_Tavish_Fight_007,
					ScenarioDbId.BOM_08_Tavish_Fight_008
				}, 
				WingDbId.BOM_Brukan => new ScenarioDbId[8]
				{
					ScenarioDbId.BOM_09_Brukan_Fight_001,
					ScenarioDbId.BOM_09_Brukan_Fight_002,
					ScenarioDbId.BOM_09_Brukan_Fight_003,
					ScenarioDbId.BOM_09_Brukan_Fight_004,
					ScenarioDbId.BOM_09_Brukan_Fight_005,
					ScenarioDbId.BOM_09_Brukan_Fight_006,
					ScenarioDbId.BOM_09_Brukan_Fight_007,
					ScenarioDbId.BOM_09_Brukan_Fight_008
				}, 
				WingDbId.BOM_Dawngrasp => new ScenarioDbId[8]
				{
					ScenarioDbId.BOM_10_Dawngrasp_Fight_001,
					ScenarioDbId.BOM_10_Dawngrasp_Fight_002,
					ScenarioDbId.BOM_10_Dawngrasp_Fight_003,
					ScenarioDbId.BOM_10_Dawngrasp_Fight_004,
					ScenarioDbId.BOM_10_Dawngrasp_Fight_005,
					ScenarioDbId.BOM_10_Dawngrasp_Fight_006,
					ScenarioDbId.BOM_10_Dawngrasp_Fight_007,
					ScenarioDbId.BOM_10_Dawngrasp_Fight_008
				}, 
				_ => new ScenarioDbId[8]
				{
					ScenarioDbId.BOM_01_Rokara_01,
					ScenarioDbId.BOM_01_Rokara_02,
					ScenarioDbId.BOM_01_Rokara_03,
					ScenarioDbId.BOM_01_Rokara_04,
					ScenarioDbId.BOM_01_Rokara_05,
					ScenarioDbId.BOM_01_Rokara_06,
					ScenarioDbId.BOM_01_Rokara_07,
					ScenarioDbId.BOM_01_Rokara_08
				}, 
			}));
			if (numDesiredBosses >= 0 && numDesiredBosses < scenariosForClass.Length)
			{
				adventureScenarioId = (long)scenariosForClass[numDesiredBosses];
			}
			if (adventureScenarioId > 0)
			{
				InvokeSetGameSaveDataCheat(serverKey, GameSaveKeySubkeyId.DUNGEON_CRAWL_SCENARIO_ID, new long[1] { adventureScenarioId });
			}
		}
		if (!GameSaveDataManager.Get().GetSubkeyValue(serverKey, GameSaveKeySubkeyId.DUNGEON_CRAWL_SCENARIO_ID, out adventureScenarioId) || adventureScenarioId <= 0)
		{
			ScenarioDbfRecord scenarioRecord = GameDbf.Scenario.GetRecord((ScenarioDbfRecord r) => r.AdventureId == (int)adventureDbId && r.ModeId == (int)adventureMode);
			if (scenarioRecord != null && scenarioRecord.ID > 0)
			{
				adventureScenarioId = scenarioRecord.ID;
				InvokeSetGameSaveDataCheat(serverKey, GameSaveKeySubkeyId.DUNGEON_CRAWL_SCENARIO_ID, new long[1] { adventureScenarioId });
			}
		}
		if (AdventureUtils.SelectableHeroPowersExistForAdventure(adventureDbId))
		{
			long adventureHeroPowerDbId = 0L;
			if (!useLoadoutOfActiveRun && GameSaveDataManager.Get().GetSubkeyValue(serverKey, GameSaveKeySubkeyId.DUNGEON_CRAWL_PLAYER_SELECTED_HERO_POWER, out adventureHeroPowerDbId) && adventureHeroPowerDbId > 0)
			{
				InvokeSetGameSaveDataCheat(serverKey, GameSaveKeySubkeyId.DUNGEON_CRAWL_HERO_POWER, new long[1] { adventureHeroPowerDbId });
			}
			if (!GameSaveDataManager.Get().GetSubkeyValue(serverKey, GameSaveKeySubkeyId.DUNGEON_CRAWL_HERO_POWER, out adventureHeroPowerDbId) || adventureHeroPowerDbId <= 0)
			{
				AdventureHeroPowerDbfRecord adventureHeroPower = GameDbf.AdventureHeroPower.GetRecord((AdventureHeroPowerDbfRecord r) => r.AdventureId == (int)adventureDbId && r.ClassId == deckClass);
				if (adventureHeroPower != null)
				{
					adventureHeroPowerDbId = adventureHeroPower.CardId;
					InvokeSetGameSaveDataCheat(serverKey, GameSaveKeySubkeyId.DUNGEON_CRAWL_HERO_POWER, new long[1] { adventureHeroPowerDbId });
				}
			}
		}
		if (AdventureUtils.SelectableDecksExistForAdventure(adventureDbId))
		{
			long adventureDeckId = 0L;
			List<long> adventureCardIds = null;
			if (!useLoadoutOfActiveRun && GameSaveDataManager.Get().GetSubkeyValue(serverKey, GameSaveKeySubkeyId.DUNGEON_CRAWL_PLAYER_SELECTED_DECK, out adventureDeckId) && adventureDeckId > 0)
			{
				adventureCardIds = ((IEnumerable<DeckCardDbfRecord>)GameDbf.DeckCard.GetRecords((DeckCardDbfRecord r) => r.DeckId == adventureDeckId)).Select((Func<DeckCardDbfRecord, long>)((DeckCardDbfRecord r) => r.CardId)).ToList();
				InvokeSetGameSaveDataCheat(serverKey, GameSaveKeySubkeyId.DUNGEON_CRAWL_DECK_CARD_LIST, adventureCardIds.ToArray());
			}
			if (!GameSaveDataManager.Get().GetSubkeyValue(serverKey, GameSaveKeySubkeyId.DUNGEON_CRAWL_DECK_CARD_LIST, out adventureCardIds) || adventureCardIds == null || adventureCardIds.Count <= 0)
			{
				AdventureDeckDbfRecord adventureDeck = GameDbf.AdventureDeck.GetRecord((AdventureDeckDbfRecord r) => r.AdventureId == (int)adventureDbId && r.ClassId == deckClass);
				if (adventureDeck != null)
				{
					adventureDeckId = adventureDeck.DeckId;
					if (adventureDeckId > 0)
					{
						adventureCardIds = ((IEnumerable<DeckCardDbfRecord>)GameDbf.DeckCard.GetRecords((DeckCardDbfRecord r) => r.DeckId == adventureDeckId)).Select((Func<DeckCardDbfRecord, long>)((DeckCardDbfRecord r) => r.CardId)).ToList();
						InvokeSetGameSaveDataCheat(serverKey, GameSaveKeySubkeyId.DUNGEON_CRAWL_DECK_CARD_LIST, adventureCardIds.ToArray());
					}
				}
			}
		}
		if (!useLoadoutOfActiveRun && AdventureUtils.SelectableLoadoutTreasuresExistForAdventure(adventureDbId))
		{
			long loadoutTreasureDbId = 0L;
			if (GameSaveDataManager.Get().GetSubkeyValue(serverKey, GameSaveKeySubkeyId.DUNGEON_CRAWL_PLAYER_SELECTED_LOADOUT_TREASURE_ID, out loadoutTreasureDbId) && loadoutTreasureDbId > 0)
			{
				GameSaveDataManager.Get().GetSubkeyValue(serverKey, GameSaveKeySubkeyId.DUNGEON_CRAWL_DECK_CARD_LIST, out List<long> adventureCardIds2);
				if (adventureCardIds2 == null)
				{
					adventureCardIds2 = new List<long>();
				}
				adventureCardIds2.Add(loadoutTreasureDbId);
				InvokeSetGameSaveDataCheat(serverKey, GameSaveKeySubkeyId.DUNGEON_CRAWL_DECK_CARD_LIST, adventureCardIds2.ToArray());
			}
		}
		long[] defaultDefeatedBosses = null;
		defaultDefeatedBosses = adventureDbId switch
		{
			AdventureDbId.LOOT => new long[8] { 47316L, 46311L, 46915L, 46338L, 46371L, 47307L, 47001L, 47210L }, 
			AdventureDbId.GIL => new long[8] { 47903L, 48311L, 48182L, 48151L, 48196L, 48600L, 48942L, 48315L }, 
			AdventureDbId.TRL => new long[8] { 53222L, 53223L, 53224L, 53225L, 53226L, 53227L, 53228L, 53229L }, 
			AdventureDbId.DALARAN => new long[12]
			{
				53750L, 53779L, 53667L, 53558L, 53572L, 53636L, 53607L, 53309L, 53562L, 53483L,
				53714L, 53783L
			}, 
			AdventureDbId.BOH => wingId switch
			{
				WingDbId.BOH_REXXAR => new long[8] { 63834L, 63835L, 63836L, 61384L, 63837L, 61385L, 63838L, 63839L }, 
				WingDbId.BOH_GARROSH => new long[8] { 61390L, 64757L, 64758L, 64759L, 64760L, 64761L, 64762L, 64763L }, 
				WingDbId.BOH_UTHER => new long[8] { 61388L, 65557L, 65558L, 65559L, 61389L, 65560L, 65561L, 65562L }, 
				WingDbId.BOH_ANDUIN => new long[8] { 66904L, 66902L, 66903L, 66904L, 66905L, 66906L, 66908L, 66909L }, 
				WingDbId.BOH_VALEERA => new long[8] { 68015L, 68016L, 68017L, 68018L, 68019L, 68020L, 68021L, 68022L }, 
				WingDbId.BOH_THRALL => new long[8] { 71187L, 71188L, 71189L, 71190L, 71191L, 71192L, 71193L, 71194L }, 
				WingDbId.BOH_MALFURION => new long[8] { 71857L, 71865L, 71866L, 71867L, 71868L, 71869L, 71870L, 71871L }, 
				WingDbId.BOH_GULDAN => new long[8] { 73910L, 73918L, 73919L, 73920L, 73921L, 73922L, 73923L, 73924L }, 
				WingDbId.BOH_ILLIDAN => new long[8] { 75649L, 75657L, 75658L, 75659L, 75661L, 75662L, 75663L, 75664L }, 
				WingDbId.BOH_FAELIN => new long[19]
				{
					79358L, 79364L, 79365L, 79359L, 79366L, 79367L, 79368L, 79369L, 79360L, 79371L,
					79370L, 79372L, 79377L, 79373L, 80032L, 79376L, 79361L, 79379L, 79380L
				}, 
				_ => new long[8] { 63199L, 63201L, 63204L, 63205L, 63206L, 63207L, 63208L, 61382L }, 
			}, 
			AdventureDbId.BOM => wingId switch
			{
				WingDbId.BOM_Xyrella => new long[8] { 71943L, 71946L, 71947L, 71948L, 71951L, 71955L, 71957L, 71958L }, 
				WingDbId.BOM_Guff => new long[8] { 73323L, 73324L, 73325L, 73326L, 73327L, 73328L, 73329L, 73330L }, 
				WingDbId.BOM_Kurtrus => new long[8] { 74770L, 74771L, 74772L, 74773L, 74774L, 74775L, 74777L, 74778L }, 
				WingDbId.BOM_Tamsin => new long[8] { 76424L, 76425L, 76426L, 76427L, 76430L, 76431L, 76432L, 76433L }, 
				WingDbId.BOM_Cariel => new long[8] { 78435L, 78437L, 78438L, 78439L, 78440L, 78441L, 78442L, 78443L }, 
				WingDbId.BOM_Scabbs => new long[8] { 80896L, 80897L, 80898L, 80899L, 80900L, 80901L, 80902L, 80903L }, 
				WingDbId.BOM_Tavish => new long[8] { 82407L, 82409L, 82412L, 82430L, 82416L, 82417L, 82419L, 82420L }, 
				WingDbId.BOM_Brukan => new long[8] { 85837L, 85838L, 85839L, 85840L, 85841L, 85842L, 85843L, 85844L }, 
				WingDbId.BOM_Dawngrasp => new long[8] { 86336L, 86337L, 86338L, 68339L, 86340L, 86341L, 86342L, 86343L }, 
				_ => new long[8] { 67655L, 67656L, 67657L, 67658L, 67659L, 67660L, 67661L, 67662L }, 
			}, 
			_ => new long[8] { 57319L, 57378L, 57322L, 57397L, 57573L, 53810L, 57387L, 56176L }, 
		};
		switch (mode)
		{
		case SetAdventureProgressMode.Victory:
			numDesiredBosses = defaultDefeatedBosses.Length;
			break;
		case SetAdventureProgressMode.Progress:
			numDesiredBosses = Math.Min(numDesiredBosses, defaultDefeatedBosses.Length - 1);
			break;
		}
		int numBossesInRun = AdventureConfig.GetAdventureBossesInRun(GameUtils.GetWingRecordFromMissionId((int)adventureScenarioId));
		if (numBossesInRun > 0)
		{
			numDesiredBosses = Math.Min(numDesiredBosses, numBossesInRun - 1);
		}
		long nextBoss = 0L;
		ValidateAndParseLongAtIndex(1, args, out nextBoss);
		GameSaveDataManager.Get().GetSubkeyValue(serverKey, GameSaveKeySubkeyId.DUNGEON_CRAWL_BOSSES_DEFEATED, out List<long> defeatedBosses);
		if (defeatedBosses == null)
		{
			defeatedBosses = new List<long>();
		}
		if (defeatedBosses.Count > numDesiredBosses)
		{
			int numToRemove = defeatedBosses.Count - (int)numDesiredBosses;
			defeatedBosses.RemoveRange(defeatedBosses.Count - numToRemove, numToRemove);
		}
		else
		{
			while (defeatedBosses.Count < numDesiredBosses)
			{
				defeatedBosses.Add(defaultDefeatedBosses[defeatedBosses.Count]);
			}
		}
		InvokeSetGameSaveDataCheat(serverKey, GameSaveKeySubkeyId.DUNGEON_CRAWL_BOSSES_DEFEATED, defeatedBosses.ToArray());
		InvokeSetGameSaveDataCheat(serverKey, GameSaveKeySubkeyId.DUNGEON_CRAWL_IS_RUN_ACTIVE, new long[1] { (mode == SetAdventureProgressMode.Progress) ? 1 : 0 });
		InvokeSetGameSaveDataCheat(serverKey, GameSaveKeySubkeyId.DUNGEON_CRAWL_IS_RUN_RETIRED, new long[0]);
		if (mode == SetAdventureProgressMode.Victory || mode == SetAdventureProgressMode.Defeat)
		{
			InvokeSetGameSaveDataCheat(serverKey, GameSaveKeySubkeyId.DUNGEON_CRAWL_PLAYER_CHOSEN_TREASURE, new long[0]);
			InvokeSetGameSaveDataCheat(serverKey, GameSaveKeySubkeyId.DUNGEON_CRAWL_PLAYER_CHOSEN_LOOT, new long[0]);
		}
		switch (mode)
		{
		case SetAdventureProgressMode.Victory:
			InvokeSetGameSaveDataCheat(serverKey, GameSaveKeySubkeyId.DUNGEON_CRAWL_BOSS_LOST_TO, new long[0]);
			InvokeSetGameSaveDataCheat(serverKey, GameSaveKeySubkeyId.DUNGEON_CRAWL_NEXT_BOSS_FIGHT, new long[0]);
			break;
		case SetAdventureProgressMode.Defeat:
			InvokeSetGameSaveDataCheat(serverKey, GameSaveKeySubkeyId.DUNGEON_CRAWL_BOSS_LOST_TO, new long[1] { defaultDefeatedBosses[defeatedBosses.Count] });
			InvokeSetGameSaveDataCheat(serverKey, GameSaveKeySubkeyId.DUNGEON_CRAWL_NEXT_BOSS_FIGHT, new long[0]);
			break;
		default:
			if (nextBoss == 0L && defeatedBosses.Count < defaultDefeatedBosses.Length)
			{
				nextBoss = defaultDefeatedBosses[defeatedBosses.Count];
			}
			InvokeSetGameSaveDataCheat(serverKey, GameSaveKeySubkeyId.DUNGEON_CRAWL_NEXT_BOSS_FIGHT, new long[1] { nextBoss });
			break;
		}
		InvokeSetGameSaveDataCheat(serverKey, GameSaveKeySubkeyId.DUNGEON_CRAWL_HAS_SEEN_LATEST_DUNGEON_RUN_COMPLETE, new long[1]);
		return true;
	}

	private bool OnProcessCheat_SetAllPuzzlesInProgress(string func, string[] args, string rawArgs)
	{
		int gameSaveDataKey = GameDbf.AdventureData.GetRecord((AdventureDataDbfRecord r) => r.AdventureId == 429).GameSaveDataServerKey;
		foreach (ScenarioDbfRecord record in GameDbf.Scenario.GetRecords((ScenarioDbfRecord r) => r.AdventureId == 429))
		{
			int subkey = record.GameSaveDataProgressSubkey;
			int maxProgress = record.GameSaveDataProgressMax;
			InvokeSetGameSaveDataCheat((GameSaveKeyId)gameSaveDataKey, (GameSaveKeySubkeyId)subkey, new long[1] { maxProgress });
		}
		UIStatus.Get().AddInfo($"Set All Boomsday Puzzles To Their Last Sub-Puzzle");
		return true;
	}

	private void InvokeSetGameSaveDataCheat(GameSaveKeyId key, GameSaveKeySubkeyId subkey, long value)
	{
		InvokeSetGameSaveDataCheat(key, subkey, new long[1] { value });
	}

	private void InvokeSetGameSaveDataCheat(GameSaveKeyId key, GameSaveKeySubkeyId subkey, long[] values)
	{
		InvokeSetGameSaveDataCheat((int)key, subkey, values);
	}

	private void InvokeSetGameSaveDataCheat(int key, GameSaveKeySubkeyId subkey, long[] values)
	{
		List<string> obj = new List<string> { key.ToString() };
		int num = (int)subkey;
		obj.Add(num.ToString());
		List<string> args = obj;
		if (values != null)
		{
			foreach (long value in values)
			{
				args.Add(value.ToString());
			}
		}
		OnProcessCheat_SetGameSaveData("setgsd", args.ToArray(), string.Join(" ", args.ToArray()));
	}

	private bool OnProcessCheat_GetGameSaveData(string func, string[] args, string rawArgs)
	{
		GameSaveKeyId key = (GameSaveKeyId)0;
		GameSaveKeySubkeyId subkey = (GameSaveKeySubkeyId)0;
		if (!ValidateAndParseGameSaveDataKeyAndSubkey(args, out key, out subkey))
		{
			UIStatus.Get().AddError("You must provide valid key and subkeys!");
			return true;
		}
		args = new string[3]
		{
			"getgsd",
			"key=" + args[0],
			"subkey=" + args[1]
		};
		return OnProcessCheat_utilservercmd("util", args, rawArgs, null);
	}

	private bool ValidateAndParseLongAtIndex(int index, string[] args, out long value)
	{
		value = 0L;
		long v = 0L;
		if (args.Length <= index || !long.TryParse(args[index], out v))
		{
			return false;
		}
		value = v;
		return true;
	}

	private bool ValidateAndParseGameSaveDataKeyAndSubkey(string[] args, out GameSaveKeyId key, out GameSaveKeySubkeyId subkey)
	{
		key = (GameSaveKeyId)0;
		subkey = (GameSaveKeySubkeyId)0;
		long parsedKey = 0L;
		if (args.Length < 1 || !long.TryParse(args[0], out parsedKey) || parsedKey == 0L)
		{
			UIStatus.Get().AddError("You must provide a valid non-zero id for the key!");
			return false;
		}
		key = (GameSaveKeyId)parsedKey;
		long parsedSubkey = 0L;
		if (args.Length < 2 || !long.TryParse(args[1], out parsedSubkey) || parsedSubkey == 0L)
		{
			UIStatus.Get().AddError("You must provide a valid non-zero id for the key!");
			return false;
		}
		subkey = (GameSaveKeySubkeyId)parsedSubkey;
		return true;
	}

	private bool OnProcessCheat_ResetKC_VO(string func, string[] args, string rawArgs)
	{
		ValidateAndParseLongAtIndex(0, args, out var subkeyValue);
		ResetDungeonRun_VO(AdventureDbId.LOOT, subkeyValue);
		return true;
	}

	private bool OnProcessCheat_ResetGIL_VO(string func, string[] args, string rawArgs)
	{
		ValidateAndParseLongAtIndex(0, args, out var subkeyValue);
		ResetDungeonRun_VO(AdventureDbId.GIL, subkeyValue);
		return true;
	}

	private bool OnProcessCheat_UnlockHagatha(string func, string[] args, string rawArgs)
	{
		InvokeSetGameSaveDataCheat(GameSaveKeyId.ADVENTURE_DATA_SERVER_GIL, GameSaveKeySubkeyId.DUNGEON_CRAWL_HUNTER_RUN_WINS, new long[1] { 1L });
		InvokeSetGameSaveDataCheat(GameSaveKeyId.ADVENTURE_DATA_SERVER_GIL, GameSaveKeySubkeyId.DUNGEON_CRAWL_WARRIOR_RUN_WINS, new long[1] { 1L });
		InvokeSetGameSaveDataCheat(GameSaveKeyId.ADVENTURE_DATA_SERVER_GIL, GameSaveKeySubkeyId.DUNGEON_CRAWL_ROGUE_RUN_WINS, new long[1] { 1L });
		OnProcessCheat_utilservercmd("util", new string[4] { "quest", "progress", "achieve=1010", "amount=3" }, "util quest progress achieve=1010 amount=3", null);
		SetAdventureProgressCommon(AdventureDbId.GIL, AdventureModeDbId.DUNGEON_CRAWL, new string[1] { "7" }, SetAdventureProgressMode.Progress);
		InvokeSetGameSaveDataCheat(GameSaveKeyId.ADVENTURE_DATA_CLIENT_GIL, GameSaveKeySubkeyId.DUNGEON_CRAWL_HAS_SEEN_CHARACTER_SELECT_VO, new long[1] { 1L });
		InvokeSetGameSaveDataCheat(GameSaveKeyId.ADVENTURE_DATA_CLIENT_GIL, GameSaveKeySubkeyId.DUNGEON_CRAWL_HAS_SEEN_BOSS_FLIP_1_VO, new long[1] { 1L });
		InvokeSetGameSaveDataCheat(GameSaveKeyId.ADVENTURE_DATA_CLIENT_GIL, GameSaveKeySubkeyId.DUNGEON_CRAWL_HAS_SEEN_BOSS_FLIP_2_VO, new long[1] { 1L });
		InvokeSetGameSaveDataCheat(GameSaveKeyId.ADVENTURE_DATA_CLIENT_GIL, GameSaveKeySubkeyId.DUNGEON_CRAWL_HAS_SEEN_BOSS_FLIP_3_VO, new long[1] { 1L });
		InvokeSetGameSaveDataCheat(GameSaveKeyId.ADVENTURE_DATA_CLIENT_GIL, GameSaveKeySubkeyId.DUNGEON_CRAWL_HAS_SEEN_OFFER_TREASURE_1_VO, new long[1] { 1L });
		InvokeSetGameSaveDataCheat(GameSaveKeyId.ADVENTURE_DATA_CLIENT_GIL, GameSaveKeySubkeyId.DUNGEON_CRAWL_HAS_SEEN_OFFER_LOOT_PACKS_1_VO, new long[1] { 1L });
		InvokeSetGameSaveDataCheat(GameSaveKeyId.ADVENTURE_DATA_CLIENT_GIL, GameSaveKeySubkeyId.DUNGEON_CRAWL_HAS_SEEN_OFFER_LOOT_PACKS_2_VO, new long[1] { 1L });
		return true;
	}

	private bool OnProcessCheat_ResetTRL_VO(string func, string[] args, string rawArgs)
	{
		ValidateAndParseLongAtIndex(0, args, out var subkeyValue);
		ResetDungeonRun_VO(AdventureDbId.TRL, subkeyValue);
		return true;
	}

	private bool OnProcessCheat_ResetDAL_VO(string func, string[] args, string rawArgs)
	{
		ValidateAndParseLongAtIndex(0, args, out var subkeyValue);
		ResetDungeonRun_VO(AdventureDbId.DALARAN, subkeyValue);
		return true;
	}

	private bool OnProcessCheat_ResetULD_VO(string func, string[] args, string rawArgs)
	{
		ValidateAndParseLongAtIndex(0, args, out var subkeyValue);
		ResetDungeonRun_VO(AdventureDbId.ULDUM, subkeyValue);
		return true;
	}

	private void ResetVOSubkeysForAdventure(GameSaveKeyId adventureGameSaveKey, long subkeyValue = 0L)
	{
		List<GameSaveKeySubkeyId> obj = new List<GameSaveKeySubkeyId>
		{
			GameSaveKeySubkeyId.DUNGEON_CRAWL_HAS_SEEN_WING_COMPLETE_VO,
			GameSaveKeySubkeyId.DUNGEON_CRAWL_HAS_SEEN_COMPLETE_ALL_CLASSES_VO
		};
		List<GameSaveKeySubkeyId> subkeysToSet = new List<GameSaveKeySubkeyId>
		{
			GameSaveKeySubkeyId.DUNGEON_CRAWL_BOSS_HERO_POWER_TUTORIAL_PROGRESS,
			GameSaveKeySubkeyId.DUNGEON_CRAWL_HAS_SEEN_CHARACTER_SELECT_VO,
			GameSaveKeySubkeyId.DUNGEON_CRAWL_HAS_SEEN_WELCOME_BANNER_VO,
			GameSaveKeySubkeyId.DUNGEON_CRAWL_HAS_SEEN_BOSS_FLIP_1_VO,
			GameSaveKeySubkeyId.DUNGEON_CRAWL_HAS_SEEN_BOSS_FLIP_2_VO,
			GameSaveKeySubkeyId.DUNGEON_CRAWL_HAS_SEEN_BOSS_FLIP_3_VO,
			GameSaveKeySubkeyId.DUNGEON_CRAWL_HAS_SEEN_BOSS_FLIP_4_VO,
			GameSaveKeySubkeyId.DUNGEON_CRAWL_HAS_SEEN_BOSS_FLIP_5_VO,
			GameSaveKeySubkeyId.DUNGEON_CRAWL_HAS_SEEN_OFFER_TREASURE_1_VO,
			GameSaveKeySubkeyId.DUNGEON_CRAWL_HAS_SEEN_OFFER_TREASURE_2_VO,
			GameSaveKeySubkeyId.DUNGEON_CRAWL_HAS_SEEN_OFFER_TREASURE_3_VO,
			GameSaveKeySubkeyId.DUNGEON_CRAWL_HAS_SEEN_OFFER_TREASURE_4_VO,
			GameSaveKeySubkeyId.DUNGEON_CRAWL_HAS_SEEN_OFFER_HERO_POWER_1_VO,
			GameSaveKeySubkeyId.DUNGEON_CRAWL_HAS_SEEN_OFFER_DECK_1_VO,
			GameSaveKeySubkeyId.DUNGEON_CRAWL_HAS_SEEN_OFFER_LOOT_PACKS_1_VO,
			GameSaveKeySubkeyId.DUNGEON_CRAWL_HAS_SEEN_OFFER_LOOT_PACKS_2_VO,
			GameSaveKeySubkeyId.DUNGEON_CRAWL_HAS_SEEN_BOOK_REVEAL_VO,
			GameSaveKeySubkeyId.DUNGEON_CRAWL_HAS_SEEN_BOOK_REVEAL_HEROIC_VO,
			GameSaveKeySubkeyId.DUNGEON_CRAWL_HAS_SEEN_WING_UNLOCK_VO,
			GameSaveKeySubkeyId.DUNGEON_CRAWL_HAS_SEEN_COMPLETE_ALL_WINGS_VO,
			GameSaveKeySubkeyId.DUNGEON_CRAWL_HAS_SEEN_COMPLETE_ALL_WINGS_HEROIC_VO,
			GameSaveKeySubkeyId.DUNGEON_CRAWL_HAS_SEEN_ANOMALY_UNLOCK_VO,
			GameSaveKeySubkeyId.DUNGEON_CRAWL_HAS_SEEN_REWARD_PAGE_REVEAL_VO,
			GameSaveKeySubkeyId.DUNGEON_CRAWL_HAS_SEEN_FINAL_BOSS_LOSS_1_VO,
			GameSaveKeySubkeyId.DUNGEON_CRAWL_HAS_SEEN_FINAL_BOSS_LOSS_2_VO,
			GameSaveKeySubkeyId.DUNGEON_CRAWL_HAS_SEEN_BOSS_LOSS_1_VO,
			GameSaveKeySubkeyId.DUNGEON_CRAWL_HAS_SEEN_IN_GAME_WIN_VO,
			GameSaveKeySubkeyId.DUNGEON_CRAWL_HAS_SEEN_IN_GAME_LOSE_VO,
			GameSaveKeySubkeyId.DUNGEON_CRAWL_HAS_SEEN_IN_GAME_LOSE_2_VO,
			GameSaveKeySubkeyId.DUNGEON_CRAWL_HAS_SEEN_IN_GAME_MULLIGAN_1_VO,
			GameSaveKeySubkeyId.DUNGEON_CRAWL_HAS_SEEN_IN_GAME_MULLIGAN_2_VO,
			GameSaveKeySubkeyId.ADVENTURE_HAS_SEEN_ADVENTURE,
			GameSaveKeySubkeyId.TRL_DUNGEON_HAS_SEEN_SHRINE_TUTORIAL_1_VO,
			GameSaveKeySubkeyId.TRL_DUNGEON_HAS_SEEN_SHRINE_TUTORIAL_2_VO,
			GameSaveKeySubkeyId.TRL_DUNGEON_HAS_SEEN_ENEMY_SHRINE_DIES_TUTORIAL_VO,
			GameSaveKeySubkeyId.TRL_DUNGEON_HAS_SEEN_ENEMY_SHRINE_REVIVES_TUTORIAL_VO,
			GameSaveKeySubkeyId.TRL_DUNGEON_HAS_SEEN_PLAYER_SHRINE_DIES_TUTORIAL_VO,
			GameSaveKeySubkeyId.TRL_DUNGEON_HAS_SEEN_PLAYER_SHRINE_TIMER_TICK_TUTORIAL_VO,
			GameSaveKeySubkeyId.TRL_DUNGEON_HAS_SEEN_PLAYER_SHRINE_LOST_TUTORIAL_VO,
			GameSaveKeySubkeyId.TRL_DUNGEON_HAS_SEEN_PLAYER_SHRINE_TRANSFORMED_TUTORIAL_VO,
			GameSaveKeySubkeyId.TRL_DUNGEON_HAS_SEEN_PLAYER_SHRINE_BOUNCED_TUTORIAL_VO
		};
		foreach (GameSaveKeySubkeyId subkey in obj)
		{
			long[] subkeyValues = null;
			InvokeSetGameSaveDataCheat(adventureGameSaveKey, subkey, subkeyValues);
		}
		foreach (GameSaveKeySubkeyId subkey2 in subkeysToSet)
		{
			long[] subkeyValues2 = null;
			if (subkeyValue != 0L)
			{
				subkeyValues2 = new long[1] { subkeyValue };
			}
			InvokeSetGameSaveDataCheat(adventureGameSaveKey, subkey2, subkeyValues2);
		}
	}

	private bool OnProcessCheat_SetAdventureComingSoon(string func, string[] args, string rawArgs)
	{
		if (args.Length < 2)
		{
			UIStatus.Get().AddInfo("Usage: setadventurecomingsoon [ADVENTURE] [TRUE/FALSE]\nExample: setadventurecomingsoon GIL true");
			return false;
		}
		AdventureDbId adventureId = ParseAdventureDbIdFromArgs(args, 0);
		if (adventureId == AdventureDbId.INVALID)
		{
			return false;
		}
		bool comingSoon = false;
		if (!bool.TryParse(args[1], out comingSoon))
		{
			UIStatus.Get().AddError($"Unable to parse \"{args[1]}\". Please enter True or False.");
			return false;
		}
		AdventureDbfRecord record = GameDbf.Adventure.GetRecord((int)adventureId);
		record.SetVar("COMING_SOON_EVENT", comingSoon ? "always" : "never");
		GameDbf.Adventure.ReplaceRecordByRecordId(record);
		string successMessage = ((AdventureScene.Get() == null) ? "Success!" : "Success!\nBack out and re-enter to see the change.");
		UIStatus.Get().AddInfo(successMessage);
		return true;
	}

	private bool OnProcessCheat_ResetSession_VO(string func, string[] args, string rawArgs)
	{
		NotificationManager.Get().ResetSoundsPlayedThisSession();
		return true;
	}

	private bool OnProcessCheat_SetVOChance_VO(string func, string[] args, string rawArgs)
	{
		float parsedKey = -1f;
		if (args.Length != 0 && float.TryParse(args[0], out parsedKey) && parsedKey >= 0f)
		{
			parsedKey = Mathf.Clamp(parsedKey, 0f, 1f);
		}
		VOChanceOverride = parsedKey;
		return true;
	}

	private BnetPlayer CreateCheatFriendlistItem(string name, FriendListType type, int leagueId, int starLevel, BnetProgramId programID, bool isFriend, bool isOnline, bool isAway)
	{
		return type switch
		{
			FriendListType.FRIEND => BnetFriendMgr.Get().Cheat_CreateFriend(name, leagueId, starLevel, programID, isOnline, isAway), 
			FriendListType.RECENT => BnetRecentPlayerMgr.Get().Cheat_CreateRecentPlayer(name, leagueId, starLevel, programID, isFriend, isOnline), 
			FriendListType.NEARBY => BnetNearbyPlayerMgr.Get().Cheat_CreateNearbyPlayer(name, leagueId, starLevel, programID, isFriend, isOnline), 
			_ => null, 
		};
	}

	private bool OnProcessCheat_History(string func, string[] args, string rawArgs)
	{
		HistoryManager history = HistoryManager.Get();
		if (history == null)
		{
			return false;
		}
		if (args[0].ToLower() == "true" || args[0].ToLower() == "on" || args[0] == "1")
		{
			history.EnableHistory();
		}
		if (args[0].ToLower() == "false" || args[0].ToLower() == "off" || args[0] == "0")
		{
			history.DisableHistory();
		}
		return true;
	}

	private bool OnProcessCheat_IPAddress(string func, string[] args, string rawArgs)
	{
		IPHostEntry hostEntry = Dns.GetHostEntry(Dns.GetHostName());
		if (hostEntry.AddressList.Length != 0)
		{
			string addresses = "";
			IPAddress[] addressList = hostEntry.AddressList;
			foreach (IPAddress address in addressList)
			{
				addresses = addresses + address.ToString() + "\n";
			}
			UIStatus.Get().AddInfo(addresses, 10f);
		}
		return true;
	}

	private bool OnProcessCheat_Attribution(string func, string[] args, string rawArgs)
	{
		string message = BlizzardAttributionManagerDebug.HandleCheat(func, args, rawArgs);
		UIStatus.Get().AddInfo(message);
		return true;
	}

	private bool OnProcessCheat_CRM(string func, string[] args, string rawArgs)
	{
		BlizzardCRMManager.Get().SendAllEventsForTest();
		UIStatus.Get().AddInfo("Test CRM telemetry sent!");
		return true;
	}

	private bool OnProcessCheat_Updater(string func, string[] args, string rawArgs)
	{
		string usage = "USAGE: updater [cmd] [args]\\nCommands: speed, gamespeed\\nNotice: Unit of speed is bytes per second.\\n\n\\t0 = unlimited, -1 = turn off game streaming\\n\\tStore the speed permanently: speed 0 store";
		if (args.Length < 1 || args.Any((string a) => string.IsNullOrEmpty(a)))
		{
			UIStatus.Get().AddInfo(usage, 10f);
			return true;
		}
		if (DownloadManager == null)
		{
			UIStatus.Get().AddInfo("DownloadManager is not ready yet!");
			return true;
		}
		string cmd = args[0]?.ToLower();
		bool setSpeed = true;
		bool store = false;
		int speed = 0;
		if (args.Length > 1)
		{
			speed = int.Parse(args[1]);
			store = args.Length > 2 && args[2].Equals("store");
		}
		else
		{
			setSpeed = false;
		}
		string status = null;
		switch (cmd)
		{
		case "help":
			status = usage;
			break;
		case "speed":
			if (speed < 0)
			{
				status = "Error: Cannot use the negative value!";
				break;
			}
			if (setSpeed)
			{
				DownloadManager.MaxDownloadSpeed = speed;
				status = "Set the download speed to " + speed;
			}
			else
			{
				status = "The current speed is " + DownloadManager.MaxDownloadSpeed;
			}
			if (store)
			{
				Options.Get().SetInt(Option.MAX_DOWNLOAD_SPEED, speed);
			}
			break;
		case "gamespeed":
			if (setSpeed)
			{
				if (speed < 0)
				{
					DownloadManager.InGameStreamingDefaultSpeed = speed;
					status = "Turned off in game streaming";
				}
				else
				{
					DownloadManager.DownloadSpeedInGame = speed;
					status = "Set the download speed in game to " + speed;
				}
			}
			else
			{
				status = "The current speed in game is " + DownloadManager.DownloadSpeedInGame;
			}
			if (store && speed >= 0)
			{
				Options.Get().SetInt(Option.STREAMING_SPEED_IN_GAME, speed);
			}
			break;
		}
		if (status != null)
		{
			UIStatus.Get().AddInfo(status, 5f);
		}
		return true;
	}

	private bool OnProcessCheat_Assets(string func, string[] args, string rawArgs)
	{
		string message = AssetLoaderDebug.HandleCheat(func, args, rawArgs);
		UIStatus.Get().AddInfo(message);
		return true;
	}

	private bool OnProcessCheat_refreshcurrency(string func, string[] args, string rawArgs)
	{
		CurrencyType currencyType = CurrencyType.NONE;
		if (args.Length != 0)
		{
			string arg = (args[0] ?? string.Empty).Trim();
			if (string.Equals(arg, "runestones", StringComparison.OrdinalIgnoreCase))
			{
				currencyType = (ShopUtils.IsMainVirtualCurrencyType(CurrencyType.CN_RUNESTONES) ? CurrencyType.CN_RUNESTONES : CurrencyType.ROW_RUNESTONES);
			}
			else if (string.Equals(arg, "arcane_orbs", StringComparison.OrdinalIgnoreCase))
			{
				currencyType = CurrencyType.CN_ARCANE_ORBS;
			}
		}
		if (currencyType == CurrencyType.NONE)
		{
			string usage = "USAGE: refreshcurrency <runestones|arcane_orbs>";
			UIStatus.Get().AddInfo(usage, 10f);
			return true;
		}
		if (ServiceManager.TryGet<CurrencyManager>(out var currencyManager))
		{
			currencyManager.MarkCurrencyDirty(currencyType);
			currencyManager.RefreshWallet();
		}
		return true;
	}

	private bool OnProcessCheat_mercpackgrantdiamondcard(string func, string[] args, string rawArgs)
	{
		int mercId = 0;
		int artVariantId = 0;
		if (args.Length != 2 || !int.TryParse(args[0], out mercId) || !int.TryParse(args[1], out artVariantId))
		{
			string usage = "USAGE: mercpackgrantdiamondcard <merc_id> <mercenary_art_variation id>";
			UIStatus.Get().AddInfo(usage, 10f);
			return true;
		}
		if (!PackOpening.Get().CreateMockLettucePackComponent(mercId, artVariantId, 0, acquired: false, TAG_PREMIUM.DIAMOND))
		{
			string usage2 = "Invalid Mercenary Art Variation Id";
			UIStatus.Get().AddInfo(usage2, 10f);
		}
		return true;
	}

	private bool OnProcessCheat_mercpackforcemercskin(string func, string[] args, string rawArgs)
	{
		int mercId = 0;
		int artVariantId = 0;
		int premium = 0;
		if (args.Length != 3 || !int.TryParse(args[0], out mercId) || !int.TryParse(args[1], out artVariantId) || !int.TryParse(args[2], out premium))
		{
			string usage = "USAGE: mercpackforcemercskin <merc_id> <mercenary_art_variation id> <premium_type>";
			UIStatus.Get().AddInfo(usage, 10f);
			return true;
		}
		if (!PackOpening.Get().CreateMockLettucePackComponent(mercId, artVariantId, 0, acquired: false, (TAG_PREMIUM)premium))
		{
			string usage2 = "Invalid Mercenary Art Variation Id";
			UIStatus.Get().AddInfo(usage2, 10f);
		}
		return true;
	}

	private bool OnProcessCheat_mercpackduplicate(string func, string[] args, string rawArgs)
	{
		int mercId = 0;
		int amount = 0;
		if (args.Length != 2 || !int.TryParse(args[0], out mercId) || !int.TryParse(args[1], out amount))
		{
			string usage = "USAGE: mercpackduplicate <merc_id> <amount>";
			UIStatus.Get().AddInfo(usage, 10f);
			return true;
		}
		PackOpening.Get().CreateMockLettucePackComponent(mercId, 0, amount, acquired: true, TAG_PREMIUM.NORMAL);
		return true;
	}

	private bool OnProcessCheat_loadpersonalizedshop(string func, string[] args, string rawArgs)
	{
		if (args.Length < 1)
		{
			string usage = "USAGE: loadpersonalizedshop <page_ids>";
			UIStatus.Get().AddInfo(usage, 10f);
			return true;
		}
		List<string> pageIds = args[0].Split(",").ToList();
		StoreManager.Get().SetPersonalizedShopPageAndRefreshCatalog(pageIds);
		return true;
	}

	private bool OnProcessCheat_checkfornewquests(string func, string[] args, string rawArgs)
	{
		float delaySeconds = 0f;
		if (args.Length != 0 && !string.IsNullOrEmpty(args[0]) && !float.TryParse(args[0], out delaySeconds))
		{
			UIStatus.Get().AddInfo("checkfornewquests [delaySeconds]");
			return true;
		}
		QuestManager.Get().DebugScheduleCheckForNewQuests(delaySeconds);
		return true;
	}

	private bool OnProcessCheat_checkforexpiredquests(string func, string[] args, string rawArgs)
	{
		float delaySeconds = 0f;
		if (args.Length != 0 && !string.IsNullOrEmpty(args[0]) && !float.TryParse(args[0], out delaySeconds))
		{
			UIStatus.Get().AddInfo("checkforexpiredquests [delaySeconds]");
			return true;
		}
		QuestManager.Get().DebugScheduleCheckForExpiredQuests(delaySeconds);
		return true;
	}

	private bool OnProcessCheat_showquestnotification(string func, string[] args, string rawArgs)
	{
		QuestPool.QuestPoolType poolType = QuestPool.QuestPoolType.DAILY;
		if (args.Length != 0)
		{
			poolType = EnumUtils.SafeParse(args[0], QuestPool.QuestPoolType.DAILY, ignoreCase: true);
		}
		QuestManager.Get().SimulateQuestNotificationPopup(poolType);
		return true;
	}

	private bool OnProcessCheat_showquestprogresstoast(string func, string[] args, string rawArgs)
	{
		string invalidArgsReply = "showquestprogresstoast <quest_id>";
		if (!int.TryParse(args[0], out var questId))
		{
			UIStatus.Get().AddInfo(invalidArgsReply);
			return true;
		}
		if (GameDbf.Quest.GetRecord(questId) == null)
		{
			UIStatus.Get().AddInfo(invalidArgsReply);
			return true;
		}
		QuestManager.Get().SimulateQuestProgress(questId);
		return true;
	}

	private bool OnProcessCheat_showachievementtoast(string func, string[] args, string rawArgs)
	{
		string invalidArgsReply = "showachievementtoast <achieve_id>";
		if (!int.TryParse(args[0], out var achieveId))
		{
			UIStatus.Get().AddInfo(invalidArgsReply);
			return true;
		}
		if (AchievementManager.Get().Debug_GetAchievementDataModel(achieveId) == null)
		{
			UIStatus.Get().AddInfo(invalidArgsReply);
			return true;
		}
		AchievementManager.Get().ShowAchievementComplete(achieveId);
		return true;
	}

	private bool OnProcessCheat_showachievementreward(string func, string[] args, string rawArgs)
	{
		string invalidArgsReply = "showachievementeward <achievement_id>";
		if (!int.TryParse(args[0], out var achievementId))
		{
			UIStatus.Get().AddInfo(invalidArgsReply);
			return true;
		}
		RewardScrollDataModel achievement = AchievementFactory.CreateRewardScrollDataModel(achievementId);
		if (achievement == null)
		{
			UIStatus.Get().AddInfo(invalidArgsReply);
			return true;
		}
		RewardScroll.DebugShowFake(achievement);
		return true;
	}

	private bool OnProcessCheat_showquestreward(string func, string[] args, string rawArgs)
	{
		string invalidArgsReply = "showquestreward <quest_id>";
		if (!int.TryParse(args[0], out var questId))
		{
			UIStatus.Get().AddInfo(invalidArgsReply);
			return true;
		}
		if (GameDbf.Quest.GetRecord(questId) == null)
		{
			UIStatus.Get().AddInfo(invalidArgsReply);
			return true;
		}
		RewardScroll.DebugShowFake(QuestManager.Get().CreateRewardScrollDataModelByQuestId(questId));
		return true;
	}

	private bool OnProcessCheat_showtrackreward(string func, string[] args, string rawArgs)
	{
		string invalidArgsReply = "showtrackreward <level> <forPaidTrack>";
		if (!int.TryParse(args[0], out var level))
		{
			UIStatus.Get().AddInfo(invalidArgsReply);
			return true;
		}
		bool forPaidTrack = false;
		if (args.Length > 1)
		{
			bool.TryParse(args[1], out forPaidTrack);
		}
		Hearthstone.Progression.RewardTrack rewardTrack = RewardTrackManager.Get().GetRewardTrack(Assets.Achievement.RewardTrackType.GLOBAL);
		if (rewardTrack == null)
		{
			UIStatus.Get().AddInfo("No Reward Track Found");
			return false;
		}
		RewardTrackLevelDbfRecord nodeRecord = (from r in GameUtils.GetRewardTrackLevelsForRewardTrack(rewardTrack.RewardTrackId)
			where r.Level == level
			select r).FirstOrDefault();
		if (nodeRecord == null)
		{
			UIStatus.Get().AddInfo(invalidArgsReply);
			return true;
		}
		int rewardListId = (forPaidTrack ? nodeRecord.PaidRewardList : nodeRecord.FreeRewardList);
		if (rewardListId <= 0)
		{
			if (forPaidTrack)
			{
				UIStatus.Get().AddInfo($"No paid rewards for level {level}.");
			}
			else
			{
				UIStatus.Get().AddInfo($"No free rewards for level {level}.");
			}
			return true;
		}
		RewardTrackManager.Get().Cheat_ShowRewardScroll(rewardListId, level);
		return true;
	}

	private bool OnProcessCheat_showprogtileids(string func, string[] args, string rawArgs)
	{
		ProgressUtils.ShowDebugIds = !ProgressUtils.ShowDebugIds;
		return true;
	}

	private bool OnProcessCheat_showhiddenachievements(string func, string[] args, string rawArgs)
	{
		ProgressUtils.ShowHiddenAchievements = !ProgressUtils.ShowHiddenAchievements;
		return true;
	}

	private bool OnProcessCheat_earlyConcedeConfirmationDisabled(string func, string[] args, string rawArgs)
	{
		ProgressUtils.EarlyConcedeConfirmationDisabled = !ProgressUtils.EarlyConcedeConfirmationDisabled;
		return true;
	}

	private bool OnProcessCheat_simendofgamexp(string func, string[] args, string rawArgs)
	{
		string invalidArgsReply = "simendofgamexp <scenario_id>";
		if (args.Length != 1 || !int.TryParse(args[0], out var scenarioId))
		{
			UIStatus.Get().AddInfo(invalidArgsReply);
			return true;
		}
		RewardXpNotificationManager.Get().DebugSimScenario(scenarioId);
		return true;
	}

	private bool OnProcessCheat_terminateendofgamexp(string func, string[] args, string rawArgs)
	{
		RewardXpNotificationManager.Get().TerminateEndOfGameXp();
		return true;
	}

	private bool OnProcessCheat_shownotavernpasswarning(string func, string[] args, string rawArgs)
	{
		RewardTrackManager.OpenTavernPassErrorPopup();
		return true;
	}

	private bool OnProcessCheat_showunclaimedtrackrewards(string func, string[] args, string rawArgs)
	{
		int trackId = 2;
		int trackLevel = 50;
		if (args.Length >= 1 && args[0] != "" && int.TryParse(args[0], out var tmpTrackId))
		{
			trackId = tmpTrackId;
		}
		if (args.Length >= 2 && int.TryParse(args[1], out var tmpTrackLevel))
		{
			trackLevel = tmpTrackLevel;
		}
		RewardTrackSeasonRoll.DebugShowFakeForgotTrackRewards(trackId, trackLevel);
		return true;
	}

	private bool OnProcessCheat_setlastrewardtrackseasonseen(string func, string[] args, string rawArgs)
	{
		if (args.Length < 2)
		{
			UIStatus.Get().AddInfo("setlastrewardtrackseasonseen <reward track type>('global' or 'battlegrounds')  <season_number>");
			return true;
		}
		Global.RewardTrackType rewardTrackType = Global.RewardTrackType.NONE;
		if (!Enum.TryParse<Global.RewardTrackType>(args[0], ignoreCase: true, out rewardTrackType))
		{
			UIStatus.Get().AddInfo("setlastrewardtrackseasonseen <reward track type>('global' or 'battlegrounds')  <season_number>");
			return true;
		}
		int lastSeasonSeen = 0;
		if (!int.TryParse(args[1], out lastSeasonSeen))
		{
			UIStatus.Get().AddInfo("setlastrewardtrackseasonseen <reward track type>('global' or 'battlegrounds')  <season_number>");
			return true;
		}
		Hearthstone.Progression.RewardTrack rewardTrack = RewardTrackManager.Get().GetRewardTrack(rewardTrackType);
		if (rewardTrack == null)
		{
			UIStatus.Get().AddInfo("Reward Track not found");
			return false;
		}
		if (!rewardTrack.SetRewardTrackSeasonLastSeen(lastSeasonSeen))
		{
			UIStatus.Get().AddInfo("setlastrewardtrackseasonseen failed to set GSD value");
			return true;
		}
		UIStatus.Get().AddInfo($"Last reward track season seen = {lastSeasonSeen}");
		return true;
	}

	private bool OnProcessCheat_ShowAppRatingPrompt(string func, string[] args, string rawArgs)
	{
		string usage = "USAGE: apprating [cmd] \nCommands: clear, show, toggleplatformignore";
		if (args.Length < 1 || args.Any((string a) => string.IsNullOrEmpty(a)))
		{
			UIStatus.Get().AddInfo(usage, 10f);
			return true;
		}
		switch (args[0]?.ToLower())
		{
		case "clear":
			Options.Get().SetBool(Option.APP_RATING_AGREED, val: false);
			Options.Get().SetInt(Option.APP_RATING_PROMPT_LAST_MAJOR_VERSION_SEEN, 0);
			UIStatus.Get().AddInfo("Resetting app rating prompt count.");
			break;
		case "show":
			MobileCallbackManager.RequestAppReview(AppRatingPromptTrigger.CHEAT, forcePopupToShow: true);
			UIStatus.Get().AddInfo("Requesting app rating prompt.");
			break;
		case "toggleplatformignore":
			MobileCallbackManager.IgnorePlatformForAppRating = !MobileCallbackManager.IgnorePlatformForAppRating;
			UIStatus.Get().AddInfo(MobileCallbackManager.IgnorePlatformForAppRating ? "Now ignoring platform for app rating purposes" : "No longer ignoring platform for app rating purposes");
			break;
		}
		return true;
	}

	private bool OnProcessCheat_UpdateAADCSetting(string func, string[] args, string rawArgs)
	{
		string usage = "USAGE: optin set <optInId> <value>\noptin get";
		if (args.Length < 1 || args.Any((string a) => string.IsNullOrEmpty(a)))
		{
			UIStatus.Get().AddInfo(usage, 10f);
			return true;
		}
		string cmd = args[0]?.ToLower();
		int optInTypeIdValue;
		bool isOptedIn;
		if (!(cmd == "set"))
		{
			if (cmd == "get")
			{
				if (LoginManager.Get() != null && LoginManager.Get().OptInsReceivedDependency.IsReady())
				{
					string optInReport = "";
					foreach (OptInApi.OptInType type in Enum.GetValues(typeof(OptInApi.OptInType)))
					{
						optInReport += $"{type}({(int)type}):{LoginManager.Get().OptInApi.GetAccountOptIn(type)}\n";
					}
					UIStatus.Get().AddError(optInReport, 10f);
				}
				else
				{
					UIStatus.Get().AddError("Error: Opt-ins not ready yet, wait until login is complete.", 10f);
				}
			}
		}
		else if (int.TryParse(args[1], out optInTypeIdValue) && bool.TryParse(args[2], out isOptedIn))
		{
			if (!Enum.IsDefined(typeof(OptInApi.OptInType), optInTypeIdValue))
			{
				UIStatus.Get().AddError($"Error: No opt-in with id {optInTypeIdValue} found.", 10f);
				return true;
			}
			OptInApi.OptInType optInType = (OptInApi.OptInType)optInTypeIdValue;
			if (LoginManager.Get() != null && LoginManager.Get().OptInsReceivedDependency.IsReady())
			{
				LoginManager.Get().OptInApi.SetAccountOptIn(optInType, isOptedIn);
				UIStatus.Get().AddInfo($"Account opt in {optInType} set to {isOptedIn}.", 10f);
				PrivacyGate.Get().RefreshPrivacySettings();
			}
			else
			{
				UIStatus.Get().AddError("Error: Opt-ins not ready yet, wait until login is complete.", 10f);
			}
		}
		return true;
	}

	private bool OnProcessCheat_ShowPresence(string func, string[] args, string rawArgs)
	{
		BnetPresenceMgr bnetMgr = BnetPresenceMgr.Get();
		if (bnetMgr != null)
		{
			BnetPlayer bnetPlayer = bnetMgr.GetMyPlayer();
			string msg = PresenceMgr.Get().GetStatusText(bnetPlayer) ?? "";
			if (!string.IsNullOrEmpty(msg))
			{
				UIStatus.Get().AddInfo(msg, 2f * SceneDebugger.GetDevTimescaleMultiplier());
				return true;
			}
		}
		return false;
	}

	private bool OnProcessCheat_ShowVillageHelpPopups(string func, string[] args, string rawArgs)
	{
		LettuceVillage village = UnityEngine.Object.FindObjectOfType<LettuceVillage>();
		if (village != null)
		{
			village.Dev_ShowTutorialPopups();
			return true;
		}
		UIStatus.Get().AddError("Village does not exist in scene, cannot show popups");
		return false;
	}

	private bool OnProcessCheat_ShowMercenariesTaskToasts(string func, string[] args, string rawArgs)
	{
		int numTasksToShow;
		bool numTasksArg = int.TryParse(args[0], out numTasksToShow);
		if (args.Length < 1 || !numTasksArg || numTasksToShow == 0)
		{
			numTasksToShow = 1;
		}
		int taskID = 27868;
		if (args.Length > 1)
		{
			int.TryParse(args[1], out taskID);
		}
		int taskProgress = 1;
		if (args.Length > 2)
		{
			int.TryParse(args[1], out taskProgress);
		}
		LettuceVillage village = UnityEngine.Object.FindObjectOfType<LettuceVillage>();
		LettuceMapDisplay mapDisplay = UnityEngine.Object.FindObjectOfType<LettuceMapDisplay>();
		if (village == null && mapDisplay == null)
		{
			UIStatus.Get().AddError("Village and map display do not exist in scene, cannot show toasts");
			return false;
		}
		List<MercenariesTaskState> fakeTasks = new List<MercenariesTaskState>();
		for (int i = 0; i < numTasksToShow; i++)
		{
			MercenariesTaskState fakeTask = new MercenariesTaskState();
			fakeTask.Progress = taskProgress;
			fakeTask.TaskId = taskID;
			fakeTasks.Add(fakeTask);
		}
		if (village != null)
		{
			village.StartCoroutine(LettuceVillageDataUtil.ShowTaskToast(fakeTasks, useGeneric: true));
			return true;
		}
		if (mapDisplay != null)
		{
			mapDisplay.StartCoroutine(LettuceVillageDataUtil.ShowTaskToast(fakeTasks, useGeneric: true));
			return true;
		}
		return false;
	}

	private bool OnProcessCheat_MercTraining(string func, string[] args, string rawArgs)
	{
		StringBuilder usage = new StringBuilder();
		usage.Append("USAGE: merctraining ").AppendLine("add <mercId>").AppendLine("remove <mercId>")
			.AppendLine("claim <mercId>")
			.AppendLine("debug");
		if (args.Length < 1 || args.Any((string a) => string.IsNullOrEmpty(a)))
		{
			UIStatus.Get().AddInfo(usage.ToString(), 10f);
			return true;
		}
		int mercId = 0;
		if (args.Length > 1 && !int.TryParse(args[1], out mercId))
		{
			UIStatus.Get().AddError("Invalid merc ID");
			return false;
		}
		string cmd = args[0]?.ToLower();
		switch (cmd)
		{
		case "add":
			Network.Get().MercenariesTrainingAddRequest(mercId);
			break;
		case "remove":
			Network.Get().MercenariesTrainingRemoveRequest(mercId);
			break;
		case "claim":
			Network.Get().MercenariesTrainingCollectRequest(mercId);
			break;
		case "debug":
		{
			(LettuceMercenary, LettuceMercenary) mercenariesInTraining = CollectionManager.Get().GetMercenariesInTraining();
			StringBuilder sb = new StringBuilder();
			sb.AppendLine("Mercenaries in training ...");
			if (mercenariesInTraining.Item1 == null)
			{
				sb.AppendLine("  Slot 1: Empty");
			}
			else
			{
				sb.AppendFormat("  Slot 1: {0} ID:{1}\n", mercenariesInTraining.Item1.m_mercName, mercenariesInTraining.Item1.ID);
			}
			if (mercenariesInTraining.Item2 == null)
			{
				sb.AppendLine("  Slot 2: Empty");
			}
			else
			{
				sb.AppendFormat("  Slot 2: {0} ID:{1}\n", mercenariesInTraining.Item2.m_mercName, mercenariesInTraining.Item2.ID);
			}
			Debug.Log(sb);
			break;
		}
		default:
			UIStatus.Get().AddError(cmd + " is not a valid command for merctraining");
			break;
		}
		return true;
	}

	private bool OnProcessCheat_ExampleUI(string func, string[] args, string rawArgs)
	{
		if (exampleUI != null)
		{
			return true;
		}
		exampleUI = WidgetInstance.Create("UIFExamples.prefab:bce429027ad32fc4da9efe26c5362d6e");
		if (exampleUI == null)
		{
			return false;
		}
		OverlayUI overlayUI = OverlayUI.Get();
		if (overlayUI != null)
		{
			overlayUI.AddGameObject(exampleUI.gameObject);
		}
		return true;
	}

	private bool OnProcessCheat_LogMaterialService(string func, string[] args, string rawArgs)
	{
		DateTime utcNow = DateTime.UtcNow;
		string meterialServiceDumpDirectory = Path.Combine(Log.LogsPath, "MaterialService");
		try
		{
			Directory.CreateDirectory(meterialServiceDumpDirectory);
		}
		catch (Exception ex)
		{
			Log.Asset.PrintInfo("Error creating CSV file directory: '" + meterialServiceDumpDirectory + "'\nError message: " + ex.Message);
			return false;
		}
		string timestamp = utcNow.ToString("yy_MM_dd_hh_mm_ss");
		string materialServiceHierarchyDump = Path.Combine(meterialServiceDumpDirectory, timestamp + "_hierarchyDump.csv");
		string materialServiceStatsDump = Path.Combine(meterialServiceDumpDirectory, timestamp + "_stats.csv");
		string materialServiceMaterialsDump = Path.Combine(meterialServiceDumpDirectory, timestamp + "_materials.csv");
		DumpRendererHierarchyToCsv(materialServiceHierarchyDump);
		DumpMaterialStatsToCsv(materialServiceStatsDump);
		DumpMaterialsToCsv(materialServiceMaterialsDump);
		string cheatToastMessage = "Wrotedebug material service logs to: " + meterialServiceDumpDirectory + ".";
		Log.Asset.PrintInfo(cheatToastMessage);
		return true;
	}

	private void DumpRendererHierarchyToCsv(string path)
	{
		using FileStream fileStream = File.Open(path, FileMode.Create);
		using StreamWriter streamWriter = new StreamWriter(fileStream);
		IMaterialService materialService = ServiceManager.Get<IMaterialService>();
		streamWriter.WriteLine("Renderer,Materials Count,PathToRoot");
		foreach (KeyValuePair<int, RegisteredRenderer> registeredRenderer in materialService.GetRegisteredRenderers())
		{
			RegisteredRenderer renderer = registeredRenderer.Value;
			Transform current = renderer.Renderer.transform;
			if (current != null)
			{
				streamWriter.Write(current.name);
			}
			else
			{
				streamWriter.Write("null");
			}
			streamWriter.Write(",");
			streamWriter.Write(renderer.Materials.Count);
			streamWriter.Write(",");
			while (current != null)
			{
				streamWriter.Write(current.name);
				streamWriter.Write("->");
				current = current.parent;
			}
			streamWriter.WriteLine("null");
		}
	}

	private void DumpMaterialsToCsv(string path)
	{
		using FileStream fileStream = File.Open(path, FileMode.Create);
		using StreamWriter streamWriter = new StreamWriter(fileStream);
		IMaterialService materialService = ServiceManager.Get<IMaterialService>();
		streamWriter.WriteLine("Material,HashCode,TimesUsed");
		foreach (KeyValuePair<int, MaterialUsages> registeredMaterial in materialService.GetRegisteredMaterials())
		{
			MaterialUsages material = registeredMaterial.Value;
			streamWriter.WriteLine(string.Format("{0},{1},{2},{3}", material.Material ? material.Material.name : "NULL", material.HashCode, material.TimesUsed, material.TimeToRemove));
		}
	}

	private void DumpMaterialStatsToCsv(string path)
	{
		using FileStream fileStream = File.Open(path, FileMode.Create);
		using StreamWriter streamWriter = new StreamWriter(fileStream);
		IMaterialService materialService = ServiceManager.Get<IMaterialService>();
		Dictionary<int, RegisteredRenderer> registeredRenderers = materialService.GetRegisteredRenderers();
		Dictionary<int, MaterialUsages> registeredMaterials = materialService.GetRegisteredMaterials();
		streamWriter.WriteLine("Custom Renderer Count,Custom Material Count,Unused Materials,Unused Renderers");
		streamWriter.WriteLine($"{registeredRenderers.Count},{registeredMaterials.Count},{materialService.GetUnusedMaterials().Count},{materialService.GetUnusedRenderers().Count}");
	}

	private bool OnProcessCheat_LogZombies(string func, string[] args, string rawArgs)
	{
		GameObject go = UnityEngine.Object.FindObjectOfType<Processor>().gameObject;
		if (!go.TryGetComponent<ZombieObjectDetector>(out var zombieObjectDetector))
		{
			zombieObjectDetector = go.AddComponent<ZombieObjectDetector>();
		}
		if (!go.TryGetComponent<ZombieObjectDetector_Report_TTY>(out var zombieObjectDetector_Report_TTY))
		{
			zombieObjectDetector_Report_TTY = go.AddComponent<ZombieObjectDetector_Report_TTY>();
		}
		string zombieDumpDirectory = Path.Combine(Log.LogsPath, "Zombies");
		DateTime utcNow = DateTime.UtcNow;
		try
		{
			Directory.CreateDirectory(zombieDumpDirectory);
		}
		catch (Exception ex)
		{
			Log.Asset.PrintInfo("Error creating CSV file directory: '" + zombieDumpDirectory + "'\nError message: " + ex.Message);
			return false;
		}
		string timestamp = utcNow.ToString("yy_MM_dd_hh_mm_ss");
		string zombieDumpPath = Path.Combine(zombieDumpDirectory, timestamp + "_zombies.csv");
		zombieObjectDetector_Report_TTY.InitOutputFile(zombieDumpPath);
		zombieObjectDetector.RunZombieObjectDetection();
		return true;
	}

	private bool OnProcessCheat_SendReport(string func, string[] args, string rawArgs)
	{
		string usage = "USAGE: sendreport <account id> <complaint type> <subcomplaint type>";
		if (args.Length < 3 || args.Any((string a) => string.IsNullOrEmpty(a)))
		{
			UIStatus.Get().AddInfo(usage, 5f);
			return true;
		}
		string s = args[0];
		string complaintTypeStr = args[1];
		string subcomplaintTypeStr = args[2];
		if (ulong.TryParse(s, out var accountId) && Enum.TryParse<ReportType.ComplaintType>(complaintTypeStr, out var complaintType) && Enum.TryParse<ReportType.SubcomplaintType>(subcomplaintTypeStr, out var subComplaintType))
		{
			BattleNet.SubmitReport(new BnetAccountId(0uL, accountId), complaintType, new List<ReportType.SubcomplaintType> { subComplaintType });
		}
		else
		{
			UIStatus.Get().AddInfo("Invalid arguments provided.", 5f);
		}
		return true;
	}

	private bool OnProgressCheat_MercDetails(string func, string[] args, string rawArgs)
	{
		MercenaryDetailDisplay mercDetailDisplay = UnityEngine.Object.FindObjectOfType<MercenaryDetailDisplay>();
		if (mercDetailDisplay == null)
		{
			UIStatus.Get().AddInfo("MercenaryDetailDisplay component not found. Please add a LettuceMercDetailsDisplay object into the scene", 5f);
			return false;
		}
		LettuceTeam team = null;
		LettuceMercenary mercenary = null;
		CollectionManager collectionManager = CollectionManager.Get();
		int num = args.Length;
		string teamId = ((num >= 1) ? args[0] : null);
		string mercId = ((num >= 2) ? args[1] : null);
		if (string.IsNullOrEmpty(teamId))
		{
			team = collectionManager.GetTeams().FirstOrDefault();
		}
		else
		{
			if (!long.TryParse(teamId, out var result))
			{
				UIStatus.Get().AddInfo("Failed to parse team ID as a number.", 5f);
				return false;
			}
			team = collectionManager.GetTeam(result);
		}
		mercenary = ((!string.IsNullOrEmpty(mercId)) ? collectionManager.GetMercenary(mercId) : team.GetMercs().FirstOrDefault());
		if (mercenary == null)
		{
			UIStatus.Get().AddInfo("Could not find a valid Mercenary to display", 5f);
			return false;
		}
		LettuceTeamDataModel selectedTeamDataModel = new LettuceTeamDataModel();
		CollectionUtils.PopulateMercenariesTeamDataModel(selectedTeamDataModel, team);
		mercDetailDisplay.GetComponent<Widget>().BindDataModel(selectedTeamDataModel);
		mercDetailDisplay.Show(mercenary, "SHOW_FULL", team);
		return true;
	}

	private bool OnProcessCheat_AckAllNotices(string func, string[] args, string rawArgs)
	{
		NetCache.NetCacheProfileNotices notices = ((NetCache.Get() != null) ? NetCache.Get().GetNetObject<NetCache.NetCacheProfileNotices>() : null);
		if (notices == null || notices.Notices == null)
		{
			return false;
		}
		UIStatus.Get().AddInfo("Acknowledging all notices, restart client to dismiss.");
		try
		{
			Network net = Network.Get();
			foreach (NetCache.ProfileNotice notice in notices.Notices)
			{
				net.AckNotice(notice.NoticeID);
			}
		}
		catch (Exception ex)
		{
			Log.All.PrintWarning("Error acknowledging notices: " + ex.Message);
		}
		return true;
	}

	private bool OnProcessCheat_AppsFlyer(string func, string[] args, string rawArgs)
	{
		if (args[0] == "resetoptions")
		{
			UIStatus.Get().AddInfo("Resetting Apps Flyer Options");
			Options.Get().SetBool(Option.AF_FIRST_BOX_AFTER_TUTORIAL, val: false);
			Options.Get().SetBool(Option.AF_FIRST_PACK_OPENED, val: false);
			Options.Get().SetBool(Option.AF_FIRST_SHOP_VISIT, val: false);
			Options.Get().SetBool(Option.AF_FIRST_NON_TUTORIAL_GAME_START_TRADITIONAL, val: false);
			Options.Get().SetBool(Option.AF_FIRST_NON_TUTORIAL_GAME_START_BATTLEGROUNDS, val: false);
			Options.Get().SetBool(Option.AF_FIRST_NON_TUTORIAL_GAME_START_MERCENARIES, val: false);
			Options.Get().SetBool(Option.AF_REWARD_TRACK_EVENT, val: false);
		}
		return true;
	}

	private bool OnProcessCheat_SoundMono(string func, string[] args, string rawArgs)
	{
		if (args[0].ToLower() == "on")
		{
			if (Options.Get().GetBool(Option.SOUND_MONO_ENABLED))
			{
				UIStatus.Get().AddInfo("Mono sound already enabled");
				return true;
			}
			Options.Get().SetBool(Option.SOUND_MONO_ENABLED, val: true);
			UIStatus.Get().AddInfo("Mono: ON | Stereo: OFF");
		}
		else if (args[0].ToLower() == "off")
		{
			if (!Options.Get().GetBool(Option.SOUND_MONO_ENABLED))
			{
				UIStatus.Get().AddInfo("Mono sound already disabled");
				return true;
			}
			Options.Get().SetBool(Option.SOUND_MONO_ENABLED, val: false);
			UIStatus.Get().AddInfo("Mono: OFF | Stereo: ON");
		}
		return true;
	}

	private bool OnProgressCheat_TaskBoardCheat(string func, string[] args, string rawArgs)
	{
		int argLength = args.Length;
		string obj = ((argLength > 0) ? args[0].ToLower() : string.Empty);
		List<string> cheatArgs = ((argLength > 1) ? args.ToList().GetRange(1, argLength - 1) : new List<string>());
		if (obj == "search_all")
		{
			return SetTaskBoardSearchAll(cheatArgs);
		}
		UIStatus.Get().AddInfo("Task board cheat not known", 5f);
		return false;
	}

	private bool OnProcessCheat_MPOEnable(string func, string[] args, string rawArgs)
	{
		string cheat = ((args.Length > 0) ? args[0].ToLower() : string.Empty);
		if (cheat == "true")
		{
			Options.Get().SetBool(Option.MPO_DEBUG, val: true);
			UIStatus.Get().AddInfo("DEBUG_UseMassPackOpening set to true", 5f);
			return true;
		}
		if (cheat == "false")
		{
			Options.Get().SetBool(Option.MPO_DEBUG, val: false);
			UIStatus.Get().AddInfo("DEBUG_UseMassPackOpening set to false", 5f);
			return true;
		}
		UIStatus.Get().AddInfo("unknown input", 5f);
		return false;
	}

	private bool OnProcessCheat_OpenFakeCatchupBooster(string func, string[] args, string rawArgs)
	{
		int num = args.Length;
		int numCards = 0;
		if (num > 0 && !string.IsNullOrEmpty(args[0]) && !int.TryParse(args[0], out numCards))
		{
			UIStatus.Get().AddError("OpenFakeCatchupBooster Error: Unknown input. Usage OpenFakeCatchupBooster [numberOfCardsInBooster]");
			return false;
		}
		if (PackOpening.Get() != null)
		{
			PackOpening.Get().OpenFakeCatchupPack(numCards);
			return true;
		}
		UIStatus.Get().AddError("Pack opening is not active!");
		return false;
	}

	private bool OnProcessCheat_AddCardToSideboard(string func, string[] args, string rawArgs)
	{
		if (args.Length != 1)
		{
			UIStatus.Get().AddError("AddCardToSideboard error: Unknown input. Usage addcardtosideboard [cardId]");
			return false;
		}
		if (CollectionManager.Get().GetEditedDeck().GetCurrentSideboardDeck() != null)
		{
			EntityDef entityDef = DefLoader.Get().GetEntityDef(args[0]);
			if (entityDef == null)
			{
				UIStatus.Get().AddError("Card with ID " + args[0] + " not found");
				return false;
			}
			CollectionDeckTray.Get().AddCard(entityDef, TAG_PREMIUM.NORMAL, false, null, DeckRule.RuleType.DEATHKNIGHT_RUNE_LIMIT, DeckRule.RuleType.COUNT_COPIES_OF_EACH_CARD);
			return true;
		}
		UIStatus.Get().AddError("No sideboard is currently being edited");
		return false;
	}

	private bool OnProcessCheat_AddCardToDeck(string func, string[] args, string rawArgs)
	{
		if (args.Length != 1)
		{
			UIStatus.Get().AddError("AddCardToDeck error: Unknown input. Usage addcardtosideboard [cardId]");
			return false;
		}
		if (CollectionManager.Get().GetEditedDeck() != null)
		{
			EntityDef entityDef = DefLoader.Get().GetEntityDef(args[0]);
			if (entityDef == null)
			{
				UIStatus.Get().AddError("Card with ID " + args[0] + " not found");
				return false;
			}
			CollectionDeckTray.Get().AddCard(entityDef, TAG_PREMIUM.NORMAL, false, null, DeckRule.RuleType.DEATHKNIGHT_RUNE_LIMIT, DeckRule.RuleType.COUNT_COPIES_OF_EACH_CARD);
			return true;
		}
		UIStatus.Get().AddError("No deck is currently being edited");
		return false;
	}

	private bool OnProcessCheat_ForceAddCardToDeck(string func, string[] args, string rawArgs)
	{
		if (args.Length != 1)
		{
			UIStatus.Get().AddError("AddCardToDeck error: Unknown input. Usage addcardtosideboard [cardId]");
			return false;
		}
		if (CollectionManager.Get().GetEditedDeck() != null)
		{
			EntityDef entityDef = DefLoader.Get().GetEntityDef(args[0]);
			if (entityDef == null)
			{
				UIStatus.Get().AddError("Card with ID " + args[0] + " not found");
				return false;
			}
			CollectionDeckTray.Get().AddCard(entityDef, TAG_PREMIUM.NORMAL, false, true, null, DeckRule.RuleType.DEATHKNIGHT_RUNE_LIMIT, DeckRule.RuleType.COUNT_COPIES_OF_EACH_CARD);
			return true;
		}
		UIStatus.Get().AddError("No deck is currently being edited");
		return false;
	}

	private bool SetTaskBoardSearchAll(List<string> args)
	{
		LettuceVillageTaskCollection taskCollection = UnityEngine.Object.FindObjectOfType<LettuceVillageTaskCollection>();
		if (taskCollection == null)
		{
			UIStatus.Get().AddInfo("LettuceVillageTaskCollection component not found. Please load into the merc scene", 5f);
			return false;
		}
		if (bool.TryParse((args.Count >= 1) ? args[0].ToLower() : string.Empty, out var result))
		{
			taskCollection.DoSearchAllMercData = result;
			taskCollection.RefreshVisuals();
			return true;
		}
		UIStatus.Get().AddInfo("Param not known. Use True or False", 5f);
		return false;
	}

	private bool OnProcessCheat_setservertimeoffset(string func, string[] args, string rawArgs)
	{
		if (!double.TryParse((args.Length >= 1) ? args[0].ToLower() : string.Empty, out var offset))
		{
			UIStatus.Get().AddInfo("Failed to parse offset as a double.", 5f);
			return false;
		}
		BnetBar bnetBar = BnetBar.Get();
		if (bnetBar == null)
		{
			UIStatus.Get().AddInfo("BNet bar not loaded.", 5f);
			return false;
		}
		bnetBar.Cheat_SetServerTimeOffset(offset);
		return true;
	}

	private bool OnProcessCheat_RunDeeplink(string func, string[] args, string rawArgs)
	{
		if (args.Length < 1 || args.Length > 1)
		{
			UIStatus.Get().AddInfo("Unknown format, expected: " + func + " <url>", 5f);
			return true;
		}
		return true;
	}

	private bool OnProcessCheat_refreshproductmodels(string func, string[] args, string rawArgs)
	{
		IProductDataService productDataService = ServiceManager.Get<IProductDataService>();
		if (productDataService == null)
		{
			UIStatus.Get().AddInfo("Cannot find Product Data Service", 5f);
			return false;
		}
		productDataService.CreateDataModels();
		Shop shop = Shop.Get();
		if (shop != null)
		{
			shop.RefreshDataModel();
		}
		return true;
	}

	private bool OnProcessCheat_addproducttag(string func, string[] args, string rawArgs)
	{
		string usage = "USAGE: addproducttag <pmt_product_id> <tag> <force_show>";
		int argLength = args.Length;
		if (argLength < 2 || !long.TryParse(args[0], out var pmtProductId))
		{
			UIStatus.Get().AddInfo(usage, 5f);
			return false;
		}
		string tagToAdd = args[1];
		if (string.IsNullOrWhiteSpace(tagToAdd))
		{
			UIStatus.Get().AddInfo("Cannot add blank tag to product", 5f);
			return false;
		}
		if (!ProductId.IsValid(pmtProductId))
		{
			UIStatus.Get().AddInfo($"Product ID: {pmtProductId} is out of range.", 5f);
			return false;
		}
		if (!ServiceManager.TryGet<IProductDataService>(out var dataService))
		{
			Log.Store.PrintError("Product service not initialized.");
			return false;
		}
		ProductDataModel productDataModel = dataService.GetProductDataModel(pmtProductId);
		if (productDataModel == null)
		{
			UIStatus.Get().AddInfo($"Cannot find bundle with pmt id - {pmtProductId}", 5f);
			return false;
		}
		if (productDataModel.Tags.Contains(tagToAdd))
		{
			UIStatus.Get().AddInfo($"Tags {tagToAdd} already exists on product {pmtProductId}", 5f);
			return false;
		}
		if (tagToAdd.Contains("value_percent"))
		{
			string[] split = tagToAdd.Split('+');
			productDataModel.Tags.Add("value_percent");
			productDataModel.AdditionalBannerData = ((split.Length > 1) ? split[1] : "0");
		}
		else if (tagToAdd.Contains("value_multiply"))
		{
			string[] split2 = tagToAdd.Split('+');
			productDataModel.Tags.Add("value_multiply");
			productDataModel.AdditionalBannerData = ((split2.Length > 1) ? split2[1] : "0");
		}
		else
		{
			productDataModel.Tags.Add(tagToAdd);
		}
		bool forceShow = false;
		if (argLength >= 3 && !bool.TryParse(args[2], out forceShow))
		{
			UIStatus.Get().AddError($"Unable to parse \"{args[2]}\". Please enter True or False.");
			return false;
		}
		if (forceShow)
		{
			switch (tagToAdd)
			{
			case "timer_min":
				productDataModel.IsScheduled = true;
				productDataModel.HoursBeforeEnd = 7;
				productDataModel.DaysBeforeEnd = 0;
				break;
			case "timer_max":
				productDataModel.IsScheduled = true;
				productDataModel.HoursBeforeEnd = 0;
				productDataModel.DaysBeforeEnd = 7;
				break;
			case "lto_min":
				productDataModel.IsScheduled = true;
				productDataModel.HoursBeforeEnd = 1;
				productDataModel.DaysBeforeEnd = 0;
				break;
			case "lto_max":
				productDataModel.IsScheduled = true;
				productDataModel.DaysBeforeEnd = 6;
				break;
			case "on_sale":
			{
				CurrencyType priceCurrency = CurrencyType.GOLD;
				float priceAmount = 100f;
				if (productDataModel.Prices.Count > 0)
				{
					PriceDataModel priceDataModel2 = productDataModel.Prices[0];
					priceAmount = priceDataModel2.Amount;
					priceCurrency = priceDataModel2.Currency;
				}
				productDataModel.Prices.Clear();
				PriceDataModel priceDataModel3 = new PriceDataModel
				{
					Currency = priceCurrency,
					Amount = priceAmount,
					OnSale = true
				};
				if (priceCurrency == CurrencyType.REAL_MONEY)
				{
					priceDataModel3.DisplayText = StoreManager.Get().FormatCost(priceAmount, null);
					priceDataModel3.OriginalAmount = priceAmount + 10f;
					priceDataModel3.OriginalDisplayText = StoreManager.Get().FormatCost(priceDataModel3.OriginalAmount, null);
				}
				else
				{
					priceDataModel3.DisplayText = priceAmount.ToString();
					priceDataModel3.OriginalAmount = priceAmount + 500f;
					priceDataModel3.OriginalDisplayText = priceDataModel3.OriginalAmount.ToString();
				}
				productDataModel.Prices.Add(priceDataModel3);
				productDataModel.Tags.Add("show_price");
				break;
			}
			case "pricestyle_icon":
			{
				productDataModel.Prices.Clear();
				List<PriceDataModel> priceDataModels4 = new List<PriceDataModel>
				{
					new PriceDataModel
					{
						Currency = CurrencyType.ROW_RUNESTONES,
						Amount = 1f,
						DisplayText = "1"
					},
					new PriceDataModel
					{
						Currency = CurrencyType.GOLD,
						Amount = 1f,
						DisplayText = "1"
					},
					new PriceDataModel
					{
						Currency = CurrencyType.REAL_MONEY,
						Amount = 1f,
						DisplayText = "1"
					}
				};
				productDataModel.Prices.AddRange(priceDataModels4);
				productDataModel.Tags.Add("show_price");
				break;
			}
			case "pricestyle_single":
			{
				productDataModel.Prices.Clear();
				PriceDataModel priceDataModel = new PriceDataModel
				{
					Currency = CurrencyType.ROW_RUNESTONES,
					Amount = 1000f,
					DisplayText = "1000"
				};
				productDataModel.Prices.Add(priceDataModel);
				productDataModel.Tags.Add("show_price");
				break;
			}
			case "pricestyle_singleicon":
			{
				productDataModel.Prices.Clear();
				List<PriceDataModel> priceDataModels3 = new List<PriceDataModel>
				{
					new PriceDataModel
					{
						Currency = CurrencyType.ROW_RUNESTONES,
						Amount = 1000f,
						DisplayText = "1000"
					},
					new PriceDataModel
					{
						Currency = CurrencyType.GOLD,
						Amount = 1f,
						DisplayText = "1"
					},
					new PriceDataModel
					{
						Currency = CurrencyType.REAL_MONEY,
						Amount = 1f,
						DisplayText = "1"
					}
				};
				productDataModel.Prices.AddRange(priceDataModels3);
				productDataModel.Tags.Add("show_price");
				productDataModel.Tags.Add("promote_price+vc");
				break;
			}
			case "pricestyle_double":
			{
				productDataModel.Prices.Clear();
				List<PriceDataModel> priceDataModels2 = new List<PriceDataModel>
				{
					new PriceDataModel
					{
						Currency = CurrencyType.ROW_RUNESTONES,
						Amount = 1000f,
						DisplayText = "1000"
					},
					new PriceDataModel
					{
						Currency = CurrencyType.GOLD,
						Amount = 100f,
						DisplayText = "100"
					}
				};
				productDataModel.Prices.AddRange(priceDataModels2);
				productDataModel.Tags.Add("show_price");
				productDataModel.Tags.Add("promote_price+vc-gold");
				break;
			}
			case "pricestyle_doubleicon":
			{
				productDataModel.Prices.Clear();
				List<PriceDataModel> priceDataModels = new List<PriceDataModel>
				{
					new PriceDataModel
					{
						Currency = CurrencyType.ROW_RUNESTONES,
						Amount = 1000f,
						DisplayText = "1000"
					},
					new PriceDataModel
					{
						Currency = CurrencyType.GOLD,
						Amount = 100f,
						DisplayText = "100"
					},
					new PriceDataModel
					{
						Currency = CurrencyType.REAL_MONEY,
						Amount = 1f,
						DisplayText = "1"
					}
				};
				productDataModel.Prices.AddRange(priceDataModels);
				productDataModel.Tags.Add("show_price");
				productDataModel.Tags.Add("promote_price+vc-gold");
				break;
			}
			}
		}
		return true;
	}

	private bool OnProcessCheat_addproducttiertag(string func, string[] args, string rawArgs)
	{
		string usage = "USAGE: addproducttiertag <pmt_product_id> <tag> <force_show>";
		int argLength = args.Length;
		if (argLength < 2 || !long.TryParse(args[0], out var pmtProductId))
		{
			UIStatus.Get().AddInfo(usage, 5f);
			return false;
		}
		string tagToAdd = args[1];
		if (string.IsNullOrWhiteSpace(tagToAdd))
		{
			UIStatus.Get().AddInfo("Cannot add blank tag to product tier", 5f);
			return false;
		}
		if (!ProductId.IsValid(pmtProductId))
		{
			UIStatus.Get().AddInfo($"Product ID: {pmtProductId} is out of range.", 5f);
			return false;
		}
		if (!ServiceManager.TryGet<IProductDataService>(out var dataService))
		{
			Log.Store.PrintError("Product service not initialized.");
			return false;
		}
		foreach (ProductTierDataModel productTierDataModel in dataService.GetProductTierDataModels())
		{
			foreach (ShopBrowserButtonDataModel browserButton in productTierDataModel.BrowserButtons)
			{
				if (browserButton.DisplayProduct.PmtId == pmtProductId)
				{
					if (!productTierDataModel.Tags.Contains(tagToAdd))
					{
						productTierDataModel.Tags.Add(tagToAdd);
					}
					break;
				}
			}
		}
		bool forceShow = false;
		if (argLength >= 3 && !bool.TryParse(args[2], out forceShow))
		{
			UIStatus.Get().AddError($"Unable to parse \"{args[2]}\". Please enter True or False.");
			return false;
		}
		return true;
	}

	private bool OnProcessCheat_GetRegionInfo(string func, string[] args, string rawArgs)
	{
		string launchRegion = BattleNet.GetLaunchOption("REGION", encrypted: false);
		Region prevRegion = RegionUtils.ConvertBNetRegionToGlobalRegion((BnetRegion)Options.Get().GetInt(Option.PREFERRED_REGION, -1));
		UIStatus.Get().AddInfo($"Launch Region: {launchRegion}, Preferred Region: {prevRegion}" + $"\nCurrent Region used: {RegionUtils.CurrentRegion}" + "\n\nOn dev builds Region may be pulled from Preferred. Use \"resetpreferredregion\" to clear it.", 7f);
		return true;
	}

	private bool OnProcessCheat_ResetPreferredRegion(string func, string[] args, string rawArgs)
	{
		Region prevRegion = RegionUtils.ConvertBNetRegionToGlobalRegion((BnetRegion)Options.Get().GetInt(Option.PREFERRED_REGION, -1));
		Options.Get().SetInt(Option.PREFERRED_REGION, -1);
		UIStatus.Get().AddInfo($"Clearing PREFERRED_REGION Option. Was \"{prevRegion}\"", 5f);
		return true;
	}

	private void RegisterShopCommands(CheatMgr cheatMgr)
	{
		cheatMgr.RegisterCategory("shop");
		cheatMgr.RegisterCheatHandler("storepassword", OnProcessCheat_storepassword, "Show store challenge popup", "", "");
		cheatMgr.RegisterCheatHandler("testproduct", OnProcessCheat_testproduct, "Fill Shop with a product", "<pmt_product_id>");
		cheatMgr.RegisterCheatHandler("testproducttag", OnProcessCheat_testproducttag, "Fill Shop with products matching a tag", "<tag name>");
		cheatMgr.RegisterCheatHandler("testadventurestore", OnProcessCheat_testadventurestore, "Open adventure store for a wing", "<wing_id> <is_full_adventure>");
		cheatMgr.RegisterCheatHandler("shopbadging", OnProcessCheat_shopbadging, "Show/Hide shop badging", "<show/hide>");
		cheatMgr.RegisterCheatHandler("shopnewlabel", OnProcessCheat_shopnewlabel, "Show/Hide the NEW! label", "<show/hide>");
	}

	private bool OnProcessCheat_storepassword(string func, string[] args, string rawArgs)
	{
		if (m_loadingStoreChallengePrompt)
		{
			return true;
		}
		if (m_storeChallengePrompt == null)
		{
			m_loadingStoreChallengePrompt = true;
			PrefabCallback<GameObject> onPrefabInstantiated = delegate(AssetReference assetRef, GameObject go, object callbackData)
			{
				Processor.RunCoroutine(StorePasswordCoroutine(assetRef, go, callbackData));
			};
			AssetLoader.Get().InstantiatePrefab("StoreChallengePrompt.prefab:43f02a51d311c214aa25232228ccefef", onPrefabInstantiated);
		}
		else if (m_storeChallengePrompt.IsShown())
		{
			m_storeChallengePrompt.Hide();
		}
		else
		{
			Processor.RunCoroutine(StorePasswordCoroutine(m_storeChallengePrompt.name, m_storeChallengePrompt.gameObject, null));
		}
		return true;
	}

	private bool OnProcessCheat_testproducttag(string func, string[] args, string rawArgs)
	{
		string usage = "USAGE: testproducttag <tag_name>";
		if (args.Length < 1)
		{
			UIStatus.Get().AddInfo(usage, 10f);
			return true;
		}
		if (string.IsNullOrEmpty(args[0]))
		{
			UIStatus.Get().AddInfo("Tag is null or empty", 10f);
			return false;
		}
		if (!ServiceManager.TryGet<IProductDataService>(out var dataService))
		{
			UIStatus.Get().AddInfo("Product service not initialized", 10f);
			return false;
		}
		if (Shop.Get() == null || !Shop.Get().IsOpen())
		{
			UIStatus.Get().AddInfo("Shop is not ready or open", 10f);
			return false;
		}
		string error;
		List<ProductDataModel> products = dataService.Debug_GetProductsWithTag(args[0], out error).ToList();
		if (products == null && products.Count() == 0)
		{
			UIStatus.Get().AddInfo("No products with tag " + args[0] + " - " + error, 10f);
			return false;
		}
		if (!TierAttributes.TryGetLayoutStyleData(TierAttributes.LayoutStyle.Default, out var defaultMap))
		{
			UIStatus.Get().AddInfo("Cannot find default layout", 10f);
			return false;
		}
		List<ProductTierDataModel> tiers = new List<ProductTierDataModel>();
		for (int i = 0; i < Mathf.CeilToInt((float)products.Count() / (float)defaultMap.LayoutMap.Count); i++)
		{
			ProductTierDataModel tier = new ProductTierDataModel
			{
				LayoutWidth = defaultMap.LayoutWidth,
				LayoutHeight = defaultMap.LayoutHeight,
				LayoutMap = defaultMap.LayoutMap.ToDataModelList(),
				MaxLayoutCount = defaultMap.MaxLayoutCount
			};
			for (int k = 0; k < defaultMap.LayoutMap.Count; k++)
			{
				tier.BrowserButtons.Add(products[i + k].ToButton());
			}
			tiers.Add(tier);
		}
		Shop.Get().Debug_FillBrowser(tiers);
		UIStatus.Get().AddInfo($"Shop filled with {products.Count()} products that have tag {args[0]}", 10f);
		return true;
	}

	private bool OnProcessCheat_testproduct(string func, string[] args, string rawArgs)
	{
		string usage = "USAGE: testproduct <pmt_product_id>";
		if (args.Length < 1 || !long.TryParse(args[0], out var pmtProductId))
		{
			UIStatus.Get().AddInfo(usage, 10f);
			return true;
		}
		if (!ProductId.IsValid(pmtProductId))
		{
			UIStatus.Get().AddInfo($"Product ID: {pmtProductId} is out of range.", 10f);
			return true;
		}
		if (!ServiceManager.TryGet<IProductDataService>(out var dataService))
		{
			UIStatus.Get().AddInfo("Product service not initialized", 10f);
			return false;
		}
		if (Shop.Get() == null || !Shop.Get().IsOpen())
		{
			UIStatus.Get().AddInfo("Shop is not ready or open", 10f);
			return false;
		}
		List<ProductTierDataModel> tiers = new List<ProductTierDataModel>();
		ProductId productId = ProductId.CreateFrom(pmtProductId);
		string error;
		ProductDataModel product = dataService.Debug_GetProduct(productId, out error);
		if (product != null)
		{
			foreach (TierAttributes.LayoutStyle value in Enum.GetValues(typeof(TierAttributes.LayoutStyle)))
			{
				if (TierAttributes.TryGetLayoutStyleData(value, out var map))
				{
					ProductTierDataModel tier = new ProductTierDataModel
					{
						LayoutWidth = map.LayoutWidth,
						LayoutHeight = map.LayoutHeight,
						LayoutMap = map.LayoutMap.ToDataModelList(),
						MaxLayoutCount = map.MaxLayoutCount
					};
					for (int i = 0; i < tier.LayoutMap.Count; i++)
					{
						tier.BrowserButtons.Add(product.ToButton());
					}
					tiers.Add(tier);
				}
			}
			Shop.Get().Debug_FillBrowser(tiers);
			UIStatus.Get().AddInfo($"Shop filled with product {pmtProductId}", 10f);
		}
		else
		{
			UIStatus.Get().AddInfo("Error: " + error, 10f);
		}
		return true;
	}

	private bool OnProcessCheat_testadventurestore(string func, string[] args, string rawArgs)
	{
		string usage = "USAGE: testadventurestore <wing_id> <is_full_adventure>";
		if (args.Length < 1 || !int.TryParse(args[0], out var wingId))
		{
			UIStatus.Get().AddInfo(usage, 10f);
			return true;
		}
		bool isFull = false;
		if (args.Length >= 2 && !GeneralUtils.TryParseBool(args[1], out isFull))
		{
			UIStatus.Get().AddInfo(usage, 10f);
			return true;
		}
		WingDbfRecord wingRecord = GameDbf.Wing.GetRecord(wingId);
		if (wingRecord == null)
		{
			UIStatus.Get().AddInfo($"wing {wingId} not found", 10f);
			return true;
		}
		if (AdventureProgressMgr.Get() == null)
		{
			UIStatus.Get().AddInfo("AdventureProgressMgr not initialized", 10f);
			return true;
		}
		int adventureId = wingRecord.AdventureId;
		ProductType productType = ProductType.PRODUCT_TYPE_WING;
		ShopType shopType = ShopType.ADVENTURE_STORE;
		int numItemsRequired = 0;
		int pmtProductId = 0;
		switch ((AdventureDbId)adventureId)
		{
		case AdventureDbId.INVALID:
			UIStatus.Get().AddInfo($"wing {wingId} is not part of an adventure.", 10f);
			return true;
		case AdventureDbId.TUTORIAL:
		case AdventureDbId.PRACTICE:
		case AdventureDbId.ICC:
		case AdventureDbId.GIL:
		case AdventureDbId.TRL:
			UIStatus.Get().AddInfo($"wing {wingId} is part of a free adventure.", 10f);
			return true;
		case AdventureDbId.NAXXRAMAS:
			productType = ProductType.PRODUCT_TYPE_NAXX;
			shopType = ShopType.ADVENTURE_STORE;
			numItemsRequired = 1;
			break;
		case AdventureDbId.BRM:
			productType = ProductType.PRODUCT_TYPE_BRM;
			shopType = ShopType.ADVENTURE_STORE;
			numItemsRequired = 1;
			break;
		case AdventureDbId.LOE:
			productType = ProductType.PRODUCT_TYPE_LOE;
			shopType = ShopType.ADVENTURE_STORE;
			numItemsRequired = 1;
			break;
		case AdventureDbId.KARA:
			productType = ProductType.PRODUCT_TYPE_WING;
			shopType = ShopType.ADVENTURE_STORE;
			numItemsRequired = 1;
			break;
		default:
			productType = ProductType.PRODUCT_TYPE_WING;
			if (isFull)
			{
				shopType = ShopType.ADVENTURE_STORE_FULL_PURCHASE_WIDGET;
				pmtProductId = wingRecord.PmtProductIdForThisAndRestOfAdventure;
				if (pmtProductId == 0)
				{
					UIStatus.Get().AddInfo($"wing {wingId} has no product id defined to complete the adventure", 10f);
					return true;
				}
			}
			else
			{
				shopType = ShopType.ADVENTURE_STORE_WING_PURCHASE_WIDGET;
				pmtProductId = wingRecord.PmtProductIdForSingleWingPurchase;
				if (pmtProductId == 0)
				{
					UIStatus.Get().AddInfo($"wing {wingId} has no product id defined by the single wing", 10f);
					return true;
				}
			}
			break;
		}
		string failReason;
		ItemOwnershipStatus status = StoreManager.GetStaticProductItemOwnershipStatus(productType, wingRecord.ID, out failReason);
		if (status == ItemOwnershipStatus.OWNED)
		{
			UIStatus.Get().AddInfo($"Cannot show store where wing ownership status is {status.ToString()}", 10f);
		}
		StoreManager.Get().StartAdventureTransaction(productType, wingRecord.ID, null, null, shopType, numItemsRequired, useOverlayUI: false, null, pmtProductId);
		return true;
	}

	private bool OnProcessCheat_shopbadging(string func, string[] args, string rawArgs)
	{
		string usage = "USAGE: shopbadging <true/false>";
		bool show = false;
		if (!ServiceManager.TryGet<IProductDataService>(out var dataService))
		{
			UIStatus.Get().AddInfo("Product service not initialized", 10f);
			return false;
		}
		if (args.Length != 0 && !string.IsNullOrEmpty(args[0]) && args[0] == "log")
		{
			dataService.CheckForNewItems();
			return true;
		}
		if (args.Length != 0 && !GeneralUtils.TryParseBool(args[0], out show))
		{
			UIStatus.Get().AddInfo(usage, 10f);
			return true;
		}
		if (ShopInitialization.Status != ShopStatus.Ready)
		{
			UIStatus.Get().AddInfo("Shop is not ready", 10f);
			return false;
		}
		if (show)
		{
			dataService.ClearLatestDisplayedProducts();
		}
		else
		{
			dataService.MarkShopAsVisited();
		}
		Shop shop = Shop.Get();
		if (shop != null)
		{
			shop.RaiseShopButtonStatusEvent();
		}
		return true;
	}

	private bool OnProcessCheat_shopnewlabel(string func, string[] args, string rawArgs)
	{
		string usage = "USAGE: shopnewlabel <true/false>";
		bool show = false;
		if (args.Length != 0 && !GeneralUtils.TryParseBool(args[0], out show))
		{
			UIStatus.Get().AddInfo(usage, 10f);
			return true;
		}
		if (!ServiceManager.TryGet<IProductDataService>(out var dataService))
		{
			UIStatus.Get().AddInfo("Product service not initialized", 10f);
			return false;
		}
		if (ShopInitialization.Status != ShopStatus.Ready)
		{
			UIStatus.Get().AddInfo("Shop is not ready", 10f);
			return false;
		}
		if (show)
		{
			dataService.ClearSeenProducts();
		}
		else
		{
			dataService.MarkAllProductsAsSeen();
		}
		return true;
	}
}
