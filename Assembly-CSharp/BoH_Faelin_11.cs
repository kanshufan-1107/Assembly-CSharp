using System.Collections;
using System.Collections.Generic;
using Blizzard.T5.Core;
using UnityEngine;

public class BoH_Faelin_11 : BoH_Faelin_Dungeon
{
	private static Map<GameEntityOption, bool> s_booleanOptions = InitBooleanOptions();

	private static readonly AssetReference VO_Story_11_Dathril_011hp_Male_NightElf_Story_Faelin_Mission11_Lorebook_2_01 = new AssetReference("VO_Story_11_Dathril_011hp_Male_NightElf_Story_Faelin_Mission11_Lorebook_2_01.prefab:9b39fa282e5646c43b3fc52f49fd91c2");

	private static readonly AssetReference VO_Story_11_Dathril_011hp_Male_NightElf_Story_Faelin_Mission11ExchangeA_01 = new AssetReference("VO_Story_11_Dathril_011hp_Male_NightElf_Story_Faelin_Mission11ExchangeA_01.prefab:f2d080e534a887b438a2eeba2f5ff2ac");

	private static readonly AssetReference VO_Story_11_Dathril_011hp_Male_NightElf_Story_Faelin_Mission11ExchangeB_01 = new AssetReference("VO_Story_11_Dathril_011hp_Male_NightElf_Story_Faelin_Mission11ExchangeB_01.prefab:e01fa43a18b4d174297847318e609f07");

	private static readonly AssetReference VO_Story_11_Dathril_011hp_Male_NightElf_Story_Faelin_Mission11ExchangeC_01 = new AssetReference("VO_Story_11_Dathril_011hp_Male_NightElf_Story_Faelin_Mission11ExchangeC_01.prefab:becc7297d3997d8499f9bc685652a496");

	private static readonly AssetReference VO_Story_11_Dathril_011hp_Male_NightElf_Story_Faelin_Mission11ExchangeD_01 = new AssetReference("VO_Story_11_Dathril_011hp_Male_NightElf_Story_Faelin_Mission11ExchangeD_01.prefab:1599e71a866b188479732c7bc7653933");

	private static readonly AssetReference VO_Story_11_Dathril_011hp_Male_NightElf_Story_Faelin_Mission11ExchangeE_01 = new AssetReference("VO_Story_11_Dathril_011hp_Male_NightElf_Story_Faelin_Mission11ExchangeE_01.prefab:82e3248fa3f9f7f44a631b4a71e6d472");

	private static readonly AssetReference VO_Story_11_Dathril_011hp_Male_NightElf_Story_Faelin_Mission11Scene_01 = new AssetReference("VO_Story_11_Dathril_011hp_Male_NightElf_Story_Faelin_Mission11Scene_01.prefab:193f1f2dcf0f3de47b2f8cd5dbbe10bc");

	private static readonly AssetReference VO_Story_11_Dathril_011hp_Male_NightElf_Story_Faelin_Mission11Scene_02 = new AssetReference("VO_Story_11_Dathril_011hp_Male_NightElf_Story_Faelin_Mission11Scene_02.prefab:4848de5f190b9cb439ed2dcb82984660");

	private static readonly AssetReference VO_Story_11_Dathril_011hp_Male_NightElf_Story_Faelin_Mission11Scene_03 = new AssetReference("VO_Story_11_Dathril_011hp_Male_NightElf_Story_Faelin_Mission11Scene_03.prefab:5ebab724858c9494cb213781ba5f6391");

	private static readonly AssetReference VO_Story_11_Dathril_011hp_Male_NightElf_Story_Faelin_Mission11Scene_04 = new AssetReference("VO_Story_11_Dathril_011hp_Male_NightElf_Story_Faelin_Mission11Scene_04.prefab:2164455706676e746b42323e31cdfc48");

	private static readonly AssetReference VO_Story_11_Dathril_011hp_Male_NightElf_Story_Faelin_Mission11Scene_05 = new AssetReference("VO_Story_11_Dathril_011hp_Male_NightElf_Story_Faelin_Mission11Scene_05.prefab:ee3c9098b538d2044aac6af3d00b97d4");

	private static readonly AssetReference VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission11_Scene_01 = new AssetReference("VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission11_Scene_01.prefab:4cf6022c731573b4bb12d14ca07ff01c");

	private static readonly AssetReference VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission11_Scene_02 = new AssetReference("VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission11_Scene_02.prefab:4b0796a7133c59a4d997f5c5abaf22b7");

	private static readonly AssetReference VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission11_Scene_03 = new AssetReference("VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission11_Scene_03.prefab:dc76657159a6fb94383935fd5ab5e01f");

	private static readonly AssetReference VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission11_Scene_04 = new AssetReference("VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission11_Scene_04.prefab:3574003951b706d4ab731fcbfef0c2bd");

	private static readonly AssetReference VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission11_Scene_05 = new AssetReference("VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission11_Scene_05.prefab:14db8f4d70c409e4197eebac2cda0aad");

	private static readonly AssetReference VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission11ExchangeA_01 = new AssetReference("VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission11ExchangeA_01.prefab:a5fdbcd6e395047428f188b9dbb0463e");

	private static readonly AssetReference VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission11ExchangeB_01 = new AssetReference("VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission11ExchangeB_01.prefab:02c1544fe6850e54ea9f0e68ef6e9338");

	private static readonly AssetReference VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission11Start_01 = new AssetReference("VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission11Start_01.prefab:44a4095b8310b74458d60bee836bfe77");

	private static readonly AssetReference VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission11Victory_01 = new AssetReference("VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission11Victory_01.prefab:5c086df45f73ddb4e9147af71284fc12");

	private static readonly AssetReference VO_Story_11_Handmaiden_015hb_Female_NightElf_Story_Faelin_Mission11ExchangeE_01 = new AssetReference("VO_Story_11_Handmaiden_015hb_Female_NightElf_Story_Faelin_Mission11ExchangeE_01.prefab:71065f0935cd6d744999a36cfd8d6d7d");

	private static readonly AssetReference VO_Story_11_Handmaiden_015hb_Female_NightElf_Story_Faelin_Mission11ExchangeE_02 = new AssetReference("VO_Story_11_Handmaiden_015hb_Female_NightElf_Story_Faelin_Mission11ExchangeE_02.prefab:541b618004f558f4297a7f2ca653cd61");

	private static readonly AssetReference VO_Story_11_Mission11_Lorebook1_Male_NightElf_Story_Faelin_Mission11_Lorebook_1_01 = new AssetReference("VO_Story_11_Mission11_Lorebook1_Male_NightElf_Story_Faelin_Mission11_Lorebook_1_01.prefab:9c54c981ba5285349b02ab6362c33c2a");

	private static readonly AssetReference RuinsOfHouseEvenlar_EmoteResponse = new AssetReference("RuinsOfHouseEvenlar_EmoteResponse.prefab:d4e270e8f250a134f81f7be8dd5628ee");

	private static readonly AssetReference RuinsOfHouseEvenlar_MissionFailed = new AssetReference("RuinsOfHouseEvenlar_MissionFailed.prefab:ed2f9778ab0cedb45b1706d3151c517f");

	private static readonly AssetReference TheSundering_Destruction = new AssetReference("TheSundering_Destruction.prefab:da8ff40853c8fc049977cdd67146d939");

	private static readonly AssetReference TheSundering_EmoteResponse = new AssetReference("TheSundering_EmoteResponse.prefab:b886b3099c004f140933f6c1aa2d5c65");

	private Notification m_popup;

	private Dictionary<int, string[]> m_popUpInfo = new Dictionary<int, string[]>
	{
		{
			228,
			new string[1] { "BOH_FAELIN_MISSION11_LOREBOOK_01" }
		},
		{
			229,
			new string[1] { "BOH_FAELIN_MISSION11_LOREBOOK_02" }
		}
	};

	private float popUpScale = 1.65f;

	private Vector3 popUpPos;

	public static readonly AssetReference IniBrassRing = new AssetReference("HS25-189_H_Ini_BrassRing_Quote.prefab:9f20435bdae99d24bae357f002abcf27");

	public static readonly AssetReference BookBrassRing = new AssetReference("Wisdomball_Pop-up_BrassRing_Quote.prefab:896ee20514caff74db639aa7055838f6");

	public static readonly AssetReference FaelinBrassRing = new AssetReference("SKN23-002_H_FAELIN_BrassRing_Quote.prefab:9984152d900140547aa7d721f3e49428");

	public static readonly AssetReference YoungFaelinBrassRing = new AssetReference("YoungFaelin_BrassRing_Quote.prefab:28fd0d4ad4d73e04faf19b1ca2aee115");

	public static readonly AssetReference YoungFaelinShockedBrassRing = new AssetReference("YoungFaelin_shocked_BrassRing_Quote.prefab:dfa4754498b7cf54ebdbd0ea87c00301");

	public static readonly AssetReference DathrilBrassRing = new AssetReference("Dathril_BrassRing_Quote.prefab:b09166d68ceefd540aefda0388f6ea8a");

	public static readonly AssetReference DathrilAngryBrassRing = new AssetReference("Dathril_angry_BrassRing_Quote.prefab:b58df46e823b36b49994e2683c15651c");

	public static readonly AssetReference DathrilSadBrassRing = new AssetReference("Dathril_sad_BrassRing_Quote.prefab:aa3db7c264c211e49a4a2541ecae8e05");

	private HashSet<string> m_playedLines = new HashSet<string>();

	private Notification m_turnCounter;

	private static Map<GameEntityOption, bool> InitBooleanOptions()
	{
		return new Map<GameEntityOption, bool> { 
		{
			GameEntityOption.DO_OPENING_TAUNTS,
			false
		} };
	}

	public BoH_Faelin_11()
	{
		m_gameOptions.AddBooleanOptions(s_booleanOptions);
	}

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> m_SoundFiles = new List<string>
		{
			VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission11Start_01, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission11ExchangeA_01, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission11ExchangeB_01, VO_Story_11_Dathril_011hp_Male_NightElf_Story_Faelin_Mission11Scene_01, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission11_Scene_01, VO_Story_11_Dathril_011hp_Male_NightElf_Story_Faelin_Mission11Scene_02, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission11_Scene_02, VO_Story_11_Dathril_011hp_Male_NightElf_Story_Faelin_Mission11Scene_03, VO_Story_11_Dathril_011hp_Male_NightElf_Story_Faelin_Mission11Scene_04, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission11_Scene_03,
			VO_Story_11_Dathril_011hp_Male_NightElf_Story_Faelin_Mission11Scene_05, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission11_Scene_04, VO_Story_11_Dathril_011hp_Male_NightElf_Story_Faelin_Mission11ExchangeA_01, VO_Story_11_Dathril_011hp_Male_NightElf_Story_Faelin_Mission11ExchangeB_01, VO_Story_11_Dathril_011hp_Male_NightElf_Story_Faelin_Mission11ExchangeC_01, VO_Story_11_Dathril_011hp_Male_NightElf_Story_Faelin_Mission11ExchangeD_01, VO_Story_11_Handmaiden_015hb_Female_NightElf_Story_Faelin_Mission11ExchangeE_01, VO_Story_11_Dathril_011hp_Male_NightElf_Story_Faelin_Mission11ExchangeE_01, VO_Story_11_Handmaiden_015hb_Female_NightElf_Story_Faelin_Mission11ExchangeE_02, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission11_Scene_05,
			VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission11Victory_01, VO_Story_11_Mission11_Lorebook1_Male_NightElf_Story_Faelin_Mission11_Lorebook_1_01, VO_Story_11_Dathril_011hp_Male_NightElf_Story_Faelin_Mission11_Lorebook_2_01, RuinsOfHouseEvenlar_EmoteResponse, RuinsOfHouseEvenlar_MissionFailed, TheSundering_Destruction, TheSundering_EmoteResponse
		};
		SetBossVOLines(m_SoundFiles);
		foreach (string soundFile in m_SoundFiles)
		{
			PreloadSound(soundFile);
		}
	}

	public override bool ShouldDoAlternateMulliganIntro()
	{
		return true;
	}

	public override bool ShouldPlayHeroBlowUpSpells(TAG_PLAYSTATE playState)
	{
		return playState != TAG_PLAYSTATE.WON;
	}

	public override void OnCreateGame()
	{
		base.OnCreateGame();
		m_Mission_EnemyPlayIdleLines = false;
		m_OverrideMusicTrack = MusicPlaylistType.InGame_TSC_Nazjatar;
	}

	protected override IEnumerator HandleMissionEventWithTiming(int missionEvent)
	{
		while (m_enemySpeaking)
		{
			yield return null;
		}
		Actor enemyActor = GameState.Get().GetOpposingSidePlayer().GetHero()
			.GetCard()
			.GetActor();
		Actor friendlyActor = GameState.Get().GetFriendlySidePlayer().GetHero()
			.GetCard()
			.GetActor();
		popUpPos = new Vector3(0f, 0f, -40f);
		switch (missionEvent)
		{
		case 515:
			if (GameState.Get().GetOpposingSidePlayer().GetHero()
				.GetCardId() == "Story_11_Ruins_011hb")
			{
				yield return MissionPlaySound(enemyActor, RuinsOfHouseEvenlar_EmoteResponse);
			}
			else
			{
				yield return MissionPlaySound(enemyActor, TheSundering_EmoteResponse);
			}
			break;
		case 507:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlaySound(enemyActor, TheSundering_Destruction);
			GameState.Get().SetBusy(busy: false);
			break;
		case 114:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlaySound(enemyActor, RuinsOfHouseEvenlar_MissionFailed);
			GameState.Get().SetBusy(busy: false);
			break;
		case 504:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(friendlyActor, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission11Victory_01);
			GameState.Get().SetBusy(busy: false);
			break;
		case 100:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(friendlyActor, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission11Start_01);
			yield return MissionPlayVO(friendlyActor, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission11ExchangeA_01);
			GameState.Get().SetBusy(busy: false);
			break;
		case 101:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(friendlyActor, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission11ExchangeB_01);
			yield return MissionPlayVO(DathrilBrassRing, VO_Story_11_Dathril_011hp_Male_NightElf_Story_Faelin_Mission11Scene_01);
			GameState.Get().SetBusy(busy: false);
			break;
		case 102:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(YoungFaelinBrassRing, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission11_Scene_01);
			yield return MissionPlayVO(DathrilBrassRing, VO_Story_11_Dathril_011hp_Male_NightElf_Story_Faelin_Mission11Scene_02);
			yield return MissionPlayVO(YoungFaelinShockedBrassRing, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission11_Scene_02);
			yield return MissionPlayVO(DathrilSadBrassRing, VO_Story_11_Dathril_011hp_Male_NightElf_Story_Faelin_Mission11Scene_03);
			yield return MissionPlayVO(DathrilAngryBrassRing, VO_Story_11_Dathril_011hp_Male_NightElf_Story_Faelin_Mission11Scene_04);
			yield return MissionPlayVO(YoungFaelinBrassRing, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission11_Scene_03);
			GameState.Get().SetBusy(busy: false);
			break;
		case 103:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(DathrilSadBrassRing, VO_Story_11_Dathril_011hp_Male_NightElf_Story_Faelin_Mission11Scene_05);
			GameState.Get().SetBusy(busy: false);
			break;
		case 104:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(friendlyActor, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission11_Scene_04);
			GameState.Get().SetBusy(busy: false);
			break;
		case 105:
			InitVisuals();
			GameState.Get().GetOpposingSidePlayer().UpdateDisplayInfo();
			Gameplay.Get().GetNameBannerForSide(Player.Side.OPPOSING).UpdateHeroNameBanner();
			break;
		case 106:
			yield return MissionPlayVO(friendlyActor, VO_Story_11_Dathril_011hp_Male_NightElf_Story_Faelin_Mission11ExchangeB_01);
			break;
		case 107:
			yield return MissionPlayVO(friendlyActor, VO_Story_11_Dathril_011hp_Male_NightElf_Story_Faelin_Mission11ExchangeC_01);
			break;
		case 108:
			yield return MissionPlayVO(friendlyActor, VO_Story_11_Dathril_011hp_Male_NightElf_Story_Faelin_Mission11ExchangeD_01);
			break;
		case 109:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(GetEnemyActorByCardId("Story_11_ZainraMinion"), VO_Story_11_Handmaiden_015hb_Female_NightElf_Story_Faelin_Mission11ExchangeE_01);
			yield return MissionPlayVO(friendlyActor, VO_Story_11_Dathril_011hp_Male_NightElf_Story_Faelin_Mission11ExchangeE_01);
			yield return MissionPlayVO(GetEnemyActorByCardId("Story_11_ZainraMinion"), VO_Story_11_Handmaiden_015hb_Female_NightElf_Story_Faelin_Mission11ExchangeE_02);
			GameState.Get().SetBusy(busy: false);
			break;
		case 110:
			yield return MissionPlayVO(friendlyActor, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission11_Scene_05);
			break;
		case 113:
			yield return MissionPlayVO(friendlyActor, VO_Story_11_Dathril_011hp_Male_NightElf_Story_Faelin_Mission11ExchangeA_01);
			break;
		case 228:
			if (!(m_popup != null))
			{
				GameState.Get().SetBusy(busy: true);
				m_popup = NotificationManager.Get().CreatePopupText(UserAttentionBlocker.NONE, popUpPos, TutorialEntity.GetTextScale() * popUpScale, GameStrings.Get(m_popUpInfo[missionEvent][0]), convertLegacyPosition: false, NotificationManager.PopupTextType.FANCY);
				yield return GameEntity.Coroutines.StartCoroutine(PlaySoundAndBlockSpeech(VO_Story_11_Mission11_Lorebook1_Male_NightElf_Story_Faelin_Mission11_Lorebook_1_01, Notification.SpeechBubbleDirection.None, GetEnemyActorByCardId("Story_11_Mission1_Lorebook1"), 2.5f));
				NotificationManager.Get().DestroyNotification(m_popup, 0f);
				m_popup = null;
				GameState.Get().SetBusy(busy: false);
			}
			break;
		case 229:
			if (!(m_popup != null))
			{
				GameState.Get().SetBusy(busy: true);
				m_popup = NotificationManager.Get().CreatePopupText(UserAttentionBlocker.NONE, popUpPos, TutorialEntity.GetTextScale() * popUpScale, GameStrings.Get(m_popUpInfo[missionEvent][0]), convertLegacyPosition: false, NotificationManager.PopupTextType.FANCY);
				yield return GameEntity.Coroutines.StartCoroutine(PlaySoundAndBlockSpeech(VO_Story_11_Dathril_011hp_Male_NightElf_Story_Faelin_Mission11_Lorebook_2_01, Notification.SpeechBubbleDirection.None, GetEnemyActorByCardId("Story_11_Mission1_Lorebook1"), 2.5f));
				NotificationManager.Get().DestroyNotification(m_popup, 0f);
				m_popup = null;
				GameState.Get().SetBusy(busy: false);
			}
			break;
		default:
			yield return base.HandleMissionEventWithTiming(missionEvent);
			break;
		}
	}

	protected override IEnumerator RespondToFriendlyPlayedCardWithTiming(Entity entity)
	{
		yield return base.RespondToFriendlyPlayedCardWithTiming(entity);
		while (m_enemySpeaking)
		{
			yield return null;
		}
		while (entity.GetCardType() == TAG_CARDTYPE.INVALID)
		{
			yield return null;
		}
		if (!m_playedLines.Contains(entity.GetCardId()) || entity.GetCardType() == TAG_CARDTYPE.HERO_POWER)
		{
			yield return WaitForEntitySoundToFinish(entity);
			string cardID = entity.GetCardId();
			m_playedLines.Add(cardID);
			GameState.Get().GetOpposingSidePlayer().GetHero()
				.GetCard()
				.GetActor();
			GameState.Get().GetFriendlySidePlayer().GetHero()
				.GetCard()
				.GetActor();
		}
	}

	protected override IEnumerator RespondToPlayedCardWithTiming(Entity entity)
	{
		while (m_enemySpeaking)
		{
			yield return null;
		}
		while (entity.GetCardType() == TAG_CARDTYPE.INVALID)
		{
			yield return null;
		}
		if (!m_playedLines.Contains(entity.GetCardId()) || entity.GetCardType() == TAG_CARDTYPE.HERO_POWER)
		{
			yield return base.RespondToPlayedCardWithTiming(entity);
			yield return WaitForEntitySoundToFinish(entity);
			string cardID = entity.GetCardId();
			m_playedLines.Add(cardID);
			GameState.Get().GetOpposingSidePlayer().GetHero()
				.GetCard()
				.GetActor();
			GameState.Get().GetFriendlySidePlayer().GetHero()
				.GetCard()
				.GetActor();
		}
	}

	protected override IEnumerator HandleStartOfTurnWithTiming(int turn)
	{
		while (m_enemySpeaking)
		{
			yield return null;
		}
		GameState.Get().GetOpposingSidePlayer().GetHero()
			.GetCard()
			.GetActor();
		Actor friendlyActor = GameState.Get().GetFriendlySidePlayer().GetHero()
			.GetCard()
			.GetActor();
		if (turn == 1)
		{
			yield return MissionPlayVO(friendlyActor, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission11ExchangeA_01);
		}
	}

	public override bool NotifyOfBattlefieldCardClicked(Entity clickedEntity, bool wasInTargetMode)
	{
		if (!base.NotifyOfBattlefieldCardClicked(clickedEntity, wasInTargetMode))
		{
			return false;
		}
		if (!wasInTargetMode)
		{
			if (clickedEntity.GetCardId() == "Story_11_Mission11_Lorebook1")
			{
				if (m_popup == null)
				{
					Network.Get().SendClientScriptGameEvent(ClientScriptGameEventType.MISSION_EVENT, 228);
					HandleMissionEvent(228);
				}
				return false;
			}
			if (clickedEntity.GetCardId() == "Story_11_Mission11_Lorebook2")
			{
				if (m_popup == null)
				{
					Network.Get().SendClientScriptGameEvent(ClientScriptGameEventType.MISSION_EVENT, 229);
					HandleMissionEvent(229);
				}
				return false;
			}
		}
		return true;
	}

	public override void NotifyOfMulliganEnded()
	{
		base.NotifyOfMulliganEnded();
		InitVisuals();
	}

	private void InitVisuals()
	{
		int cost = GetCost();
		InitTurnCounter(cost);
	}

	public override void OnTagChanged(TagDelta change)
	{
		base.OnTagChanged(change);
		if (change.tag == 48 && change.newValue != change.oldValue)
		{
			UpdateVisuals(change.newValue);
		}
	}

	private void InitTurnCounter(int cost)
	{
		GameObject turnCounterGo = AssetLoader.Get().InstantiatePrefab("LOE_Turn_Timer.prefab:b05530aa55868554fb8f0c66632b3c22");
		m_turnCounter = turnCounterGo.GetComponent<Notification>();
		PlayMakerFSM component = m_turnCounter.GetComponent<PlayMakerFSM>();
		component.FsmVariables.GetFsmBool("RunningMan").Value = true;
		component.FsmVariables.GetFsmBool("MineCart").Value = false;
		component.FsmVariables.GetFsmBool("Airship").Value = false;
		component.FsmVariables.GetFsmBool("Destroyer").Value = false;
		component.SendEvent("Birth");
		Actor enemyActor = GameState.Get().GetOpposingSidePlayer().GetHeroCard()
			.GetActor();
		m_turnCounter.transform.parent = enemyActor.gameObject.transform;
		m_turnCounter.transform.localPosition = new Vector3(-1.4f, 0.187f, -0.11f);
		m_turnCounter.transform.localScale = Vector3.one * 0.52f;
		UpdateTurnCounterText(cost);
	}

	private void UpdateVisuals(int cost)
	{
		UpdateTurnCounter(cost);
	}

	private void UpdateTurnCounter(int cost)
	{
		m_turnCounter.GetComponent<PlayMakerFSM>().SendEvent("Action");
		if (cost <= 0)
		{
			Object.Destroy(m_turnCounter.gameObject);
		}
		else
		{
			UpdateTurnCounterText(cost);
		}
	}

	private void UpdateTurnCounterText(int cost)
	{
		GameStrings.PluralNumber[] pluralNumbers = new GameStrings.PluralNumber[1]
		{
			new GameStrings.PluralNumber
			{
				m_index = 0,
				m_number = cost
			}
		};
		string counterName = GameStrings.FormatPlurals("BOH_UTHER_08", pluralNumbers);
		m_turnCounter.ChangeDialogText(counterName, cost.ToString(), "", "");
	}
}
