using System.Collections;
using System.Collections.Generic;
using Blizzard.T5.Core;
using UnityEngine;

public class BoH_Faelin_10B : BoH_Faelin_Dungeon
{
	private static Map<GameEntityOption, bool> s_booleanOptions = InitBooleanOptions();

	private static readonly AssetReference VO_Story_11_Caye_012hp_Female_Nightborne_Story_Faelin_Mission10bExchangeD_01 = new AssetReference("VO_Story_11_Caye_012hp_Female_Nightborne_Story_Faelin_Mission10bExchangeD_01.prefab:f841ff6c80bf4724db76bc44c43fb46e");

	private static readonly AssetReference VO_Story_11_EnchantedStatue_010hb_Female_NightElf_Story_Faelin_Mission10b_Lorebook_1_01 = new AssetReference("VO_Story_11_EnchantedStatue_010hb_Female_NightElf_Story_Faelin_Mission10b_Lorebook_1_01.prefab:5014bffffe7ffc74fa8af9ccef35ff2b");

	private static readonly AssetReference VO_Story_11_EnchantedStatue_010hb_Female_NightElf_Story_Faelin_Mission10b_Lorebook_2_01 = new AssetReference("VO_Story_11_EnchantedStatue_010hb_Female_NightElf_Story_Faelin_Mission10b_Lorebook_2_01.prefab:bc8f2d5d44f0a594abd4e0d546620774");

	private static readonly AssetReference VO_Story_11_EnchantedStatue_010hb_Female_NightElf_Story_Faelin_Mission10bEmoteResponse_01 = new AssetReference("VO_Story_11_EnchantedStatue_010hb_Female_NightElf_Story_Faelin_Mission10bEmoteResponse_01.prefab:ac13ce3a162b3e842bc9c5e9ce6d4171");

	private static readonly AssetReference VO_Story_11_EnchantedStatue_010hb_Female_NightElf_Story_Faelin_Mission10bExchangeA_01 = new AssetReference("VO_Story_11_EnchantedStatue_010hb_Female_NightElf_Story_Faelin_Mission10bExchangeA_01.prefab:a24a84263da09d845839c13303e3c9a6");

	private static readonly AssetReference VO_Story_11_EnchantedStatue_010hb_Female_NightElf_Story_Faelin_Mission10bExchangeB_01 = new AssetReference("VO_Story_11_EnchantedStatue_010hb_Female_NightElf_Story_Faelin_Mission10bExchangeB_01.prefab:1de66db59e5d357418e05ba665a8e2ff");

	private static readonly AssetReference VO_Story_11_EnchantedStatue_010hb_Female_NightElf_Story_Faelin_Mission10bHeroPower_01 = new AssetReference("VO_Story_11_EnchantedStatue_010hb_Female_NightElf_Story_Faelin_Mission10bHeroPower_01.prefab:ce54550061785ba4394511429ba0a0c0");

	private static readonly AssetReference VO_Story_11_EnchantedStatue_010hb_Female_NightElf_Story_Faelin_Mission10bHeroPower_02 = new AssetReference("VO_Story_11_EnchantedStatue_010hb_Female_NightElf_Story_Faelin_Mission10bHeroPower_02.prefab:cdd0ea6a245eec64c82f07e5fba052d1");

	private static readonly AssetReference VO_Story_11_EnchantedStatue_010hb_Female_NightElf_Story_Faelin_Mission10bHeroPower_03 = new AssetReference("VO_Story_11_EnchantedStatue_010hb_Female_NightElf_Story_Faelin_Mission10bHeroPower_03.prefab:7dc5e3dd0180bd44386219de8a3e090d");

	private static readonly AssetReference VO_Story_11_EnchantedStatue_010hb_Female_NightElf_Story_Faelin_Mission10bIdle_01 = new AssetReference("VO_Story_11_EnchantedStatue_010hb_Female_NightElf_Story_Faelin_Mission10bIdle_01.prefab:e7ce9891e1f7f6748b9ec7170294a3dc");

	private static readonly AssetReference VO_Story_11_EnchantedStatue_010hb_Female_NightElf_Story_Faelin_Mission10bIdle_02 = new AssetReference("VO_Story_11_EnchantedStatue_010hb_Female_NightElf_Story_Faelin_Mission10bIdle_02.prefab:aac9404238ce4c44394fb74c844dcb43");

	private static readonly AssetReference VO_Story_11_EnchantedStatue_010hb_Female_NightElf_Story_Faelin_Mission10bIdle_03 = new AssetReference("VO_Story_11_EnchantedStatue_010hb_Female_NightElf_Story_Faelin_Mission10bIdle_03.prefab:383dcc1ff5cf55a488d42d675c0e03b4");

	private static readonly AssetReference VO_Story_11_EnchantedStatue_010hb_Female_NightElf_Story_Faelin_Mission10bLoss_01 = new AssetReference("VO_Story_11_EnchantedStatue_010hb_Female_NightElf_Story_Faelin_Mission10bLoss_01.prefab:1462703cd31dc04499467fa0bb57d275");

	private static readonly AssetReference VO_Story_11_EnchantedStatue_010hb_Female_NightElf_Story_Faelin_Mission10bStart_01 = new AssetReference("VO_Story_11_EnchantedStatue_010hb_Female_NightElf_Story_Faelin_Mission10bStart_01.prefab:21279d0ef6ae60948b4de6015b86d593");

	private static readonly AssetReference VO_Story_11_EnchantedStatue_010hb_Female_NightElf_Story_Faelin_Mission10bVictory_01 = new AssetReference("VO_Story_11_EnchantedStatue_010hb_Female_NightElf_Story_Faelin_Mission10bVictory_01.prefab:83379aa02c7439e4486c23c722eb8db8");

	private static readonly AssetReference VO_Story_11_EnchantedStatue_010hb_Female_NightElf_Story_Faelin_Mission10bVictory_02 = new AssetReference("VO_Story_11_EnchantedStatue_010hb_Female_NightElf_Story_Faelin_Mission10bVictory_02.prefab:815caa420ee727b40aa3b1311721c9ad");

	private static readonly AssetReference VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission10bExchangeD_01 = new AssetReference("VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission10bExchangeD_01.prefab:97a20c4e1d3cb3e47b27959c1db05e9a");

	private static readonly AssetReference VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission10bVictory_01 = new AssetReference("VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission10bVictory_01.prefab:82c3ed1cbe37baa4a9fdcd066f8d0915");

	private static readonly AssetReference VO_Story_11_Halus_010hp_Male_BloodElf_Story_Faelin_Mission10bExchangeA_01 = new AssetReference("VO_Story_11_Halus_010hp_Male_BloodElf_Story_Faelin_Mission10bExchangeA_01.prefab:93b54f40b7a4c1441bceb526217eaf46");

	private static readonly AssetReference VO_Story_11_Halus_010hp_Male_BloodElf_Story_Faelin_Mission10bExchangeB_01 = new AssetReference("VO_Story_11_Halus_010hp_Male_BloodElf_Story_Faelin_Mission10bExchangeB_01.prefab:8ef3e2c3e8f7da84bbc4dc333065b523");

	private static readonly AssetReference VO_Story_11_Halus_010hp_Male_BloodElf_Story_Faelin_Mission10bExchangeC_01 = new AssetReference("VO_Story_11_Halus_010hp_Male_BloodElf_Story_Faelin_Mission10bExchangeC_01.prefab:11830fad1dfba594c884d692e6b48f73");

	private static readonly AssetReference VO_Story_11_Halus_010hp_Male_BloodElf_Story_Faelin_Mission10bExchangeD_01 = new AssetReference("VO_Story_11_Halus_010hp_Male_BloodElf_Story_Faelin_Mission10bExchangeD_01.prefab:07bf4a8aedd731d4a94fc096214972bb");

	private static readonly AssetReference VO_Story_11_Halus_010hp_Male_BloodElf_Story_Faelin_Mission10bExchangeE_01 = new AssetReference("VO_Story_11_Halus_010hp_Male_BloodElf_Story_Faelin_Mission10bExchangeE_01.prefab:01800f1a96b28034fad380d0e77fd491");

	private static readonly AssetReference VO_Story_11_Halus_010hp_Male_BloodElf_Story_Faelin_Mission10bStart_01 = new AssetReference("VO_Story_11_Halus_010hp_Male_BloodElf_Story_Faelin_Mission10bStart_01.prefab:a3f5fc114f1307d468e5dde23195a31f");

	private static readonly AssetReference VO_Story_11_Halus_010hp_Male_BloodElf_Story_Faelin_Mission10bStart_02 = new AssetReference("VO_Story_11_Halus_010hp_Male_BloodElf_Story_Faelin_Mission10bStart_02.prefab:4b2ba3287e649f24ba479e226a529f88");

	private static readonly AssetReference VO_Story_11_Halus_010hp_Male_BloodElf_Story_Faelin_Mission10bStart_03 = new AssetReference("VO_Story_11_Halus_010hp_Male_BloodElf_Story_Faelin_Mission10bStart_03.prefab:d7174459cc3e9e24797f0db5e5c9da13");

	private static readonly AssetReference VO_Story_11_Halus_010hp_Male_BloodElf_Story_Faelin_Mission10bVictory_01 = new AssetReference("VO_Story_11_Halus_010hp_Male_BloodElf_Story_Faelin_Mission10bVictory_01.prefab:d252cac0dc064614590353517e8d6d0f");

	private static readonly AssetReference VO_Story_11_Halus_010hp_Male_BloodElf_Story_Faelin_Mission10bVictory_02 = new AssetReference("VO_Story_11_Halus_010hp_Male_BloodElf_Story_Faelin_Mission10bVictory_02.prefab:7ff773004e7161b4c9765568982a907c");

	private static readonly AssetReference VO_Story_11_Halus_010hp_Male_BloodElf_Story_Faelin_PreMission10b_01 = new AssetReference("VO_Story_11_Halus_010hp_Male_BloodElf_Story_Faelin_PreMission10b_01.prefab:8469830a2097f2b4581d347706c18cc2");

	private static readonly AssetReference VO_Story_11_Halus_010hp_Male_BloodElf_Story_Faelin_PreMission10b_02 = new AssetReference("VO_Story_11_Halus_010hp_Male_BloodElf_Story_Faelin_PreMission10b_02.prefab:1dea37c919f2777438a41b489c3875ec");

	private static readonly AssetReference VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission10bExchangeD_01 = new AssetReference("VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission10bExchangeD_01.prefab:462d534ec4c31b54082543ba2d97ab97");

	private Notification m_popup;

	private Dictionary<int, string[]> m_popUpInfo = new Dictionary<int, string[]>
	{
		{
			229,
			new string[1] { "BOH_FAELIN_MISSION10b_LOREBOOK_01" }
		},
		{
			230,
			new string[1] { "BOH_FAELIN_MISSION10b_LOREBOOK_02" }
		}
	};

	private float popUpScale = 1.65f;

	private Vector3 popUpPos;

	public static readonly AssetReference IniBrassRing = new AssetReference("HS25-189_H_Ini_BrassRing_Quote.prefab:9f20435bdae99d24bae357f002abcf27");

	public static readonly AssetReference CayeBrassRing = new AssetReference("LC23-001_H_Caye_BrassRing_Quote.prefab:2364e8ce64f9c404fb569a86b17f3d01");

	public static readonly AssetReference BookBrassRing = new AssetReference("Wisdomball_Pop-up_BrassRing_Quote.prefab:896ee20514caff74db639aa7055838f6");

	public static readonly AssetReference FaelinBrassRing = new AssetReference("SKN23-002_H_FAELIN_BrassRing_Quote.prefab:9984152d900140547aa7d721f3e49428");

	private new List<string> m_BossIdleLines = new List<string> { VO_Story_11_EnchantedStatue_010hb_Female_NightElf_Story_Faelin_Mission10bIdle_01, VO_Story_11_EnchantedStatue_010hb_Female_NightElf_Story_Faelin_Mission10bIdle_02, VO_Story_11_EnchantedStatue_010hb_Female_NightElf_Story_Faelin_Mission10bIdle_03 };

	private List<string> m_BossUsesHeroPowerLines = new List<string> { VO_Story_11_EnchantedStatue_010hb_Female_NightElf_Story_Faelin_Mission10bHeroPower_01, VO_Story_11_EnchantedStatue_010hb_Female_NightElf_Story_Faelin_Mission10bHeroPower_02, VO_Story_11_EnchantedStatue_010hb_Female_NightElf_Story_Faelin_Mission10bHeroPower_03 };

	private HashSet<string> m_playedLines = new HashSet<string>();

	private static Map<GameEntityOption, bool> InitBooleanOptions()
	{
		return new Map<GameEntityOption, bool> { 
		{
			GameEntityOption.DO_OPENING_TAUNTS,
			false
		} };
	}

	public BoH_Faelin_10B()
	{
		m_gameOptions.AddBooleanOptions(s_booleanOptions);
	}

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> m_SoundFiles = new List<string>
		{
			VO_Story_11_Halus_010hp_Male_BloodElf_Story_Faelin_Mission10bStart_01, VO_Story_11_Halus_010hp_Male_BloodElf_Story_Faelin_Mission10bStart_02, VO_Story_11_EnchantedStatue_010hb_Female_NightElf_Story_Faelin_Mission10bStart_01, VO_Story_11_Halus_010hp_Male_BloodElf_Story_Faelin_Mission10bStart_03, VO_Story_11_EnchantedStatue_010hb_Female_NightElf_Story_Faelin_Mission10bExchangeA_01, VO_Story_11_Halus_010hp_Male_BloodElf_Story_Faelin_Mission10bExchangeA_01, VO_Story_11_EnchantedStatue_010hb_Female_NightElf_Story_Faelin_Mission10bExchangeB_01, VO_Story_11_Halus_010hp_Male_BloodElf_Story_Faelin_Mission10bExchangeB_01, VO_Story_11_Halus_010hp_Male_BloodElf_Story_Faelin_Mission10bExchangeC_01, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission10bExchangeD_01,
			VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission10bExchangeD_01, VO_Story_11_Halus_010hp_Male_BloodElf_Story_Faelin_Mission10bExchangeD_01, VO_Story_11_Caye_012hp_Female_Nightborne_Story_Faelin_Mission10bExchangeD_01, VO_Story_11_Halus_010hp_Male_BloodElf_Story_Faelin_Mission10bExchangeE_01, VO_Story_11_EnchantedStatue_010hb_Female_NightElf_Story_Faelin_Mission10bVictory_01, VO_Story_11_EnchantedStatue_010hb_Female_NightElf_Story_Faelin_Mission10bVictory_02, VO_Story_11_Halus_010hp_Male_BloodElf_Story_Faelin_Mission10bVictory_01, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission10bVictory_01, VO_Story_11_Halus_010hp_Male_BloodElf_Story_Faelin_Mission10bVictory_02, VO_Story_11_EnchantedStatue_010hb_Female_NightElf_Story_Faelin_Mission10bEmoteResponse_01,
			VO_Story_11_EnchantedStatue_010hb_Female_NightElf_Story_Faelin_Mission10bHeroPower_01, VO_Story_11_EnchantedStatue_010hb_Female_NightElf_Story_Faelin_Mission10bHeroPower_02, VO_Story_11_EnchantedStatue_010hb_Female_NightElf_Story_Faelin_Mission10bHeroPower_03, VO_Story_11_EnchantedStatue_010hb_Female_NightElf_Story_Faelin_Mission10bLoss_01, VO_Story_11_EnchantedStatue_010hb_Female_NightElf_Story_Faelin_Mission10bIdle_01, VO_Story_11_EnchantedStatue_010hb_Female_NightElf_Story_Faelin_Mission10bIdle_02, VO_Story_11_EnchantedStatue_010hb_Female_NightElf_Story_Faelin_Mission10bIdle_03, VO_Story_11_EnchantedStatue_010hb_Female_NightElf_Story_Faelin_Mission10b_Lorebook_1_01, VO_Story_11_EnchantedStatue_010hb_Female_NightElf_Story_Faelin_Mission10b_Lorebook_2_01
		};
		SetBossVOLines(m_SoundFiles);
		foreach (string soundFile in m_SoundFiles)
		{
			PreloadSound(soundFile);
		}
	}

	public override bool ShouldPlayHeroBlowUpSpells(TAG_PLAYSTATE playState)
	{
		return playState != TAG_PLAYSTATE.WON;
	}

	public override List<string> GetBossIdleLines()
	{
		return m_BossIdleLines;
	}

	public override List<string> GetBossHeroPowerRandomLines()
	{
		return m_BossUsesHeroPowerLines;
	}

	public override void OnCreateGame()
	{
		base.OnCreateGame();
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
			yield return MissionPlayVO(enemyActor, VO_Story_11_EnchantedStatue_010hb_Female_NightElf_Story_Faelin_Mission10bEmoteResponse_01);
			break;
		case 514:
			yield return MissionPlayVO(friendlyActor, VO_Story_11_Halus_010hp_Male_BloodElf_Story_Faelin_Mission10bStart_01);
			yield return MissionPlayVO(friendlyActor, VO_Story_11_Halus_010hp_Male_BloodElf_Story_Faelin_Mission10bStart_02);
			yield return MissionPlayVO(enemyActor, VO_Story_11_EnchantedStatue_010hb_Female_NightElf_Story_Faelin_Mission10bStart_01);
			yield return MissionPlayVO(friendlyActor, VO_Story_11_Halus_010hp_Male_BloodElf_Story_Faelin_Mission10bStart_03);
			break;
		case 507:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(enemyActor, VO_Story_11_EnchantedStatue_010hb_Female_NightElf_Story_Faelin_Mission10bLoss_01);
			GameState.Get().SetBusy(busy: false);
			break;
		case 504:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(enemyActor, VO_Story_11_EnchantedStatue_010hb_Female_NightElf_Story_Faelin_Mission10bVictory_01);
			yield return MissionPlayVO(enemyActor, VO_Story_11_EnchantedStatue_010hb_Female_NightElf_Story_Faelin_Mission10bVictory_02);
			GameState.Get().SetBusy(busy: false);
			break;
		case 101:
			yield return MissionPlayVO(FaelinBrassRing, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission10bExchangeD_01);
			yield return MissionPlayVO(IniBrassRing, VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission10bExchangeD_01);
			yield return MissionPlayVO(friendlyActor, VO_Story_11_Halus_010hp_Male_BloodElf_Story_Faelin_Mission10bExchangeD_01);
			yield return MissionPlayVO(CayeBrassRing, VO_Story_11_Caye_012hp_Female_Nightborne_Story_Faelin_Mission10bExchangeD_01);
			break;
		case 102:
			yield return MissionPlayVO(friendlyActor, VO_Story_11_Halus_010hp_Male_BloodElf_Story_Faelin_Mission10bExchangeE_01);
			break;
		case 105:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(friendlyActor, VO_Story_11_Halus_010hp_Male_BloodElf_Story_Faelin_Mission10bVictory_01);
			yield return MissionPlayVO(FaelinBrassRing, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission10bVictory_01);
			yield return MissionPlayVO(friendlyActor, VO_Story_11_Halus_010hp_Male_BloodElf_Story_Faelin_Mission10bVictory_02);
			GameState.Get().SetBusy(busy: false);
			break;
		case 229:
			if (!(m_popup != null))
			{
				GameState.Get().SetBusy(busy: true);
				m_popup = NotificationManager.Get().CreatePopupText(UserAttentionBlocker.NONE, popUpPos, TutorialEntity.GetTextScale() * popUpScale, GameStrings.Get(m_popUpInfo[missionEvent][0]), convertLegacyPosition: false, NotificationManager.PopupTextType.FANCY);
				yield return GameEntity.Coroutines.StartCoroutine(PlaySoundAndBlockSpeech(VO_Story_11_EnchantedStatue_010hb_Female_NightElf_Story_Faelin_Mission10b_Lorebook_1_01, Notification.SpeechBubbleDirection.None, GetEnemyActorByCardId("Story_11_Mission1_Lorebook1"), 2.5f));
				NotificationManager.Get().DestroyNotification(m_popup, 0f);
				m_popup = null;
				GameState.Get().SetBusy(busy: false);
			}
			break;
		case 230:
			if (!(m_popup != null))
			{
				GameState.Get().SetBusy(busy: true);
				m_popup = NotificationManager.Get().CreatePopupText(UserAttentionBlocker.NONE, popUpPos, TutorialEntity.GetTextScale() * popUpScale, GameStrings.Get(m_popUpInfo[missionEvent][0]), convertLegacyPosition: false, NotificationManager.PopupTextType.FANCY);
				yield return GameEntity.Coroutines.StartCoroutine(PlaySoundAndBlockSpeech(VO_Story_11_EnchantedStatue_010hb_Female_NightElf_Story_Faelin_Mission10b_Lorebook_2_01, Notification.SpeechBubbleDirection.None, GetEnemyActorByCardId("Story_11_Mission1_Lorebook1"), 2.5f));
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
		Actor enemyActor = GameState.Get().GetOpposingSidePlayer().GetHero()
			.GetCard()
			.GetActor();
		Actor friendlyActor = GameState.Get().GetFriendlySidePlayer().GetHero()
			.GetCard()
			.GetActor();
		switch (turn)
		{
		case 1:
			yield return MissionPlayVO(enemyActor, VO_Story_11_EnchantedStatue_010hb_Female_NightElf_Story_Faelin_Mission10bExchangeA_01);
			yield return MissionPlayVO(friendlyActor, VO_Story_11_Halus_010hp_Male_BloodElf_Story_Faelin_Mission10bExchangeA_01);
			break;
		case 3:
			yield return MissionPlayVO(enemyActor, VO_Story_11_EnchantedStatue_010hb_Female_NightElf_Story_Faelin_Mission10bExchangeB_01);
			yield return MissionPlayVO(friendlyActor, VO_Story_11_Halus_010hp_Male_BloodElf_Story_Faelin_Mission10bExchangeB_01);
			break;
		case 11:
			yield return MissionPlayVO(friendlyActor, VO_Story_11_Halus_010hp_Male_BloodElf_Story_Faelin_Mission10bExchangeC_01);
			break;
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
			if (clickedEntity.GetCardId() == "Story_11_Mission10b_Lorebook1")
			{
				if (m_popup == null)
				{
					Network.Get().SendClientScriptGameEvent(ClientScriptGameEventType.MISSION_EVENT, 229);
					HandleMissionEvent(229);
				}
				return false;
			}
			if (clickedEntity.GetCardId() == "Story_11_Mission10b_Lorebook2")
			{
				if (m_popup == null)
				{
					Network.Get().SendClientScriptGameEvent(ClientScriptGameEventType.MISSION_EVENT, 230);
					HandleMissionEvent(230);
				}
				return false;
			}
		}
		return true;
	}
}
