using System.Collections;
using System.Collections.Generic;
using Blizzard.T5.Core;
using UnityEngine;

public class BoH_Faelin_10A : BoH_Faelin_Dungeon
{
	private static Map<GameEntityOption, bool> s_booleanOptions = InitBooleanOptions();

	private static readonly AssetReference VO_Story_11_Caye_012hp_Female_Nightborne_Story_Faelin_Mission10aExchangeD_01 = new AssetReference("VO_Story_11_Caye_012hp_Female_Nightborne_Story_Faelin_Mission10aExchangeD_01.prefab:646ab83ea3927364fa3d4d6db7999f9c");

	private static readonly AssetReference VO_Story_11_Caye_012hp_Female_Nightborne_Story_Faelin_Mission10aExchangeF_01 = new AssetReference("VO_Story_11_Caye_012hp_Female_Nightborne_Story_Faelin_Mission10aExchangeF_01.prefab:99b23070ebdd7c04ba83318aacb6f9de");

	private static readonly AssetReference VO_Story_11_Caye_012hp_Female_Nightborne_Story_Faelin_PreMission10a_01 = new AssetReference("VO_Story_11_Caye_012hp_Female_Nightborne_Story_Faelin_PreMission10a_01.prefab:05304b39a3df7b940b95662027d87bed");

	private static readonly AssetReference VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission10aExchangeA_01 = new AssetReference("VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission10aExchangeA_01.prefab:24d08175ba1cb9741b4391211c501d31");

	private static readonly AssetReference VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission10aExchangeC_01 = new AssetReference("VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission10aExchangeC_01.prefab:765e320520f43ef49bccae1a027bd7b4");

	private static readonly AssetReference VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission10aExchangeD_01 = new AssetReference("VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission10aExchangeD_01.prefab:7addf5a6c45e63d45939f4b6005fd165");

	private static readonly AssetReference VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission10aExchangeE_01 = new AssetReference("VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission10aExchangeE_01.prefab:4dd8014a9b290394e89506bd5304fe0e");

	private static readonly AssetReference VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission10aStart_01 = new AssetReference("VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission10aStart_01.prefab:414bc7b22927fc74ebce1da0924057d9");

	private static readonly AssetReference VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission10aVictory_01 = new AssetReference("VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission10aVictory_01.prefab:fe28ea1cb9a9f35419eb79de833cb444");

	private static readonly AssetReference VO_Story_11_Finley_009hp_Male_Murloc_Story_Faelin_Mission10a_Lorebook_2_01 = new AssetReference("VO_Story_11_Finley_009hp_Male_Murloc_Story_Faelin_Mission10a_Lorebook_2_01.prefab:e85b02e533247cf4788af2a1f75cdd95");

	private static readonly AssetReference VO_Story_11_Finley_009hp_Male_Murloc_Story_Faelin_Mission10a_Lorebook_3_01 = new AssetReference("VO_Story_11_Finley_009hp_Male_Murloc_Story_Faelin_Mission10a_Lorebook_3_01.prefab:7e73dac11d7b3084c81d30f999816783");

	private static readonly AssetReference VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission10a_Lorebook_4_01 = new AssetReference("VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission10a_Lorebook_4_01.prefab:d8a3abe62806d6b408478b67f2058677");

	private static readonly AssetReference VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission10aExchangeF_01 = new AssetReference("VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission10aExchangeF_01.prefab:260fdbd4a6ce0964da936343f6bf1b96");

	private static readonly AssetReference VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission10aExchangeF_02 = new AssetReference("VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission10aExchangeF_02.prefab:c7a6f8b76707a8e4ab291ad6a2f90a1e");

	private static readonly AssetReference VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_PreMission10a_01 = new AssetReference("VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_PreMission10a_01.prefab:063b638a5eb0172409355adace56cc48");

	private static readonly AssetReference VO_Story_11_Mission10a_Lorebook1_Female_NightElf_Story_Faelin_Mission10a_Lorebook_1_01 = new AssetReference("VO_Story_11_Mission10a_Lorebook1_Female_NightElf_Story_Faelin_Mission10a_Lorebook_1_01.prefab:fcc73ab92786d114697207025c54c349");

	private static readonly AssetReference VO_Story_11_Mission10a_Lorebook2_Female_NightElf_Story_Faelin_Mission10a_Lorebook_2_01 = new AssetReference("VO_Story_11_Mission10a_Lorebook2_Female_NightElf_Story_Faelin_Mission10a_Lorebook_2_01.prefab:f9580e4c4584ea54d8ac57a2d313c5b3");

	private static readonly AssetReference VO_Story_11_NagaGuard_010hb_Male_Naga_Story_Faelin_Mission10aDeath_01 = new AssetReference("VO_Story_11_NagaGuard_010hb_Male_Naga_Story_Faelin_Mission10aDeath_01.prefab:563446cec5ccfc34ebbedaf13e3a041c");

	private static readonly AssetReference VO_Story_11_NagaGuard_010hb_Male_Naga_Story_Faelin_Mission10aEmoteResponse_01 = new AssetReference("VO_Story_11_NagaGuard_010hb_Male_Naga_Story_Faelin_Mission10aEmoteResponse_01.prefab:fc2a4aab5113d544f981f1779df3e9de");

	private static readonly AssetReference VO_Story_11_NagaGuard_010hb_Male_Naga_Story_Faelin_Mission10aExchangeA_01 = new AssetReference("VO_Story_11_NagaGuard_010hb_Male_Naga_Story_Faelin_Mission10aExchangeA_01.prefab:9edbab882d295e94cac57b44652d6539");

	private static readonly AssetReference VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission10aExchangeA_01 = new AssetReference("VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission10aExchangeA_01.prefab:241897171a1df67428084f9822452355");

	private static readonly AssetReference VO_Story_11_NagaGuard_010hb_Male_Naga_Story_Faelin_Mission10aExchangeB_01 = new AssetReference("VO_Story_11_NagaGuard_010hb_Male_Naga_Story_Faelin_Mission10aExchangeB_01.prefab:dd85cb8c37198a54691285bcc5719ec6");

	private static readonly AssetReference VO_Story_11_NagaGuard_010hb_Male_Naga_Story_Faelin_Mission10aExchangeC_01 = new AssetReference("VO_Story_11_NagaGuard_010hb_Male_Naga_Story_Faelin_Mission10aExchangeC_01.prefab:48b5773fd792a804a9c73bf74d8e8e2a");

	private static readonly AssetReference VO_Story_11_Finley_009hp_Male_Murloc_Story_Faelin_Mission10aExchangeC_01 = new AssetReference("VO_Story_11_Finley_009hp_Male_Murloc_Story_Faelin_Mission10aExchangeC_01.prefab:670509deac16877478a9216d204153b9");

	private static readonly AssetReference VO_Story_11_Finley_009hp_Male_Murloc_Story_Faelin_Mission10aExchangeE_01 = new AssetReference("VO_Story_11_Finley_009hp_Male_Murloc_Story_Faelin_Mission10aExchangeE_01.prefab:393b3fa1e8c0bfa4982804902fd24226");

	private static readonly AssetReference VO_Story_11_NagaGuard_010hb_Male_Naga_Story_Faelin_Mission10aHeroPower_01 = new AssetReference("VO_Story_11_NagaGuard_010hb_Male_Naga_Story_Faelin_Mission10aHeroPower_01.prefab:db3a20388a087244a9a0c338230e342e");

	private static readonly AssetReference VO_Story_11_NagaGuard_010hb_Male_Naga_Story_Faelin_Mission10aHeroPower_02 = new AssetReference("VO_Story_11_NagaGuard_010hb_Male_Naga_Story_Faelin_Mission10aHeroPower_02.prefab:af35db5ab0abcad44afd525161ae13f9");

	private static readonly AssetReference VO_Story_11_NagaGuard_010hb_Male_Naga_Story_Faelin_Mission10aHeroPower_03 = new AssetReference("VO_Story_11_NagaGuard_010hb_Male_Naga_Story_Faelin_Mission10aHeroPower_03.prefab:b2b58035b9039524387f06d94d8f0f56");

	private static readonly AssetReference VO_Story_11_NagaGuard_010hb_Male_Naga_Story_Faelin_Mission10aIdle_01 = new AssetReference("VO_Story_11_NagaGuard_010hb_Male_Naga_Story_Faelin_Mission10aIdle_01.prefab:45a086949eda9c04c80f86fe424ce196");

	private static readonly AssetReference VO_Story_11_NagaGuard_010hb_Male_Naga_Story_Faelin_Mission10aIdle_02 = new AssetReference("VO_Story_11_NagaGuard_010hb_Male_Naga_Story_Faelin_Mission10aIdle_02.prefab:656f2379fad7fef46a9424970e7d3a2a");

	private static readonly AssetReference VO_Story_11_NagaGuard_010hb_Male_Naga_Story_Faelin_Mission10aIdle_03 = new AssetReference("VO_Story_11_NagaGuard_010hb_Male_Naga_Story_Faelin_Mission10aIdle_03.prefab:75df86c4168fffc49b18f183fc4d797d");

	private static readonly AssetReference VO_Story_11_NagaGuard_010hb_Male_Naga_Story_Faelin_Mission10aLoss_01 = new AssetReference("VO_Story_11_NagaGuard_010hb_Male_Naga_Story_Faelin_Mission10aLoss_01.prefab:be424f627a9588545a5f1f0bb05ec396");

	private static readonly AssetReference VO_Story_11_NagaGuard_010hb_Male_Naga_Story_Faelin_Mission10aStart_01 = new AssetReference("VO_Story_11_NagaGuard_010hb_Male_Naga_Story_Faelin_Mission10aStart_01.prefab:edd081ab49aa5524686811b4fce0d588");

	private static readonly AssetReference VO_Story_11_NagaGuard_010hb_Male_Naga_Story_Faelin_Mission10aStart_02 = new AssetReference("VO_Story_11_NagaGuard_010hb_Male_Naga_Story_Faelin_Mission10aStart_02.prefab:a7077ebeff8ed7b459e4333a0a605d22");

	private static readonly AssetReference VO_Story_11_NagaGuard_010hb_Male_Naga_Story_Faelin_Mission10aVictory_01 = new AssetReference("VO_Story_11_NagaGuard_010hb_Male_Naga_Story_Faelin_Mission10aVictory_01.prefab:12117b4049e036645b75acab3c161980");

	private static readonly AssetReference VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission10aVictory_01 = new AssetReference("VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission10aVictory_01.prefab:ae0515efa43444643bd185ac3d75730a");

	private static readonly AssetReference VO_Story_11_Nemesis_005hb_Male_Human_Story_Faelin_Mission10a_Lorebook_3_01 = new AssetReference("VO_Story_11_Nemesis_005hb_Male_Human_Story_Faelin_Mission10a_Lorebook_3_01.prefab:d9cc9cf408a9f2747a32c00fed185be7");

	private Notification m_popup;

	private Dictionary<int, string[]> m_popUpInfo = new Dictionary<int, string[]>
	{
		{
			228,
			new string[1] { "BOH_FAELIN_MISSION10a_LOREBOOK_01" }
		},
		{
			229,
			new string[1] { "BOH_FAELIN_MISSION10a_LOREBOOK_02" }
		},
		{
			230,
			new string[1] { "BOH_FAELIN_MISSION10a_LOREBOOK_02b" }
		},
		{
			231,
			new string[1] { "BOH_FAELIN_MISSION10a_LOREBOOK_03" }
		},
		{
			232,
			new string[1] { "BOH_FAELIN_MISSION10a_LOREBOOK_04" }
		}
	};

	private float popUpScale = 1.65f;

	private Vector3 popUpPos;

	public static readonly AssetReference IniBrassRing = new AssetReference("HS25-189_H_Ini_BrassRing_Quote.prefab:9f20435bdae99d24bae357f002abcf27");

	public static readonly AssetReference CayeBrassRing = new AssetReference("LC23-001_H_Caye_BrassRing_Quote.prefab:2364e8ce64f9c404fb569a86b17f3d01");

	public static readonly AssetReference BookBrassRing = new AssetReference("Wisdomball_Pop-up_BrassRing_Quote.prefab:896ee20514caff74db639aa7055838f6");

	public static readonly AssetReference FinleyBrassRing = new AssetReference("HS25-190_M_Finley_BrassRing_Quote.prefab:7123c129afe769e4aa9fb262a8f7c919");

	public static readonly AssetReference HalusBrassRing = new AssetReference("LC23-003_H_Halus_BrassRing_Quote.prefab:36e0aabb3ed4ce84599d9dd4e9ebdcf5");

	public static readonly AssetReference GraceBrassRing = new AssetReference("LC23-002_H_Grace_BrassRing_Quote.prefab:1f849ba2e303ff3449d34b96f960c96a");

	private new List<string> m_BossIdleLines = new List<string> { VO_Story_11_NagaGuard_010hb_Male_Naga_Story_Faelin_Mission10aIdle_01, VO_Story_11_NagaGuard_010hb_Male_Naga_Story_Faelin_Mission10aIdle_02, VO_Story_11_NagaGuard_010hb_Male_Naga_Story_Faelin_Mission10aIdle_03 };

	private List<string> m_BossUsesHeroPowerLines = new List<string> { VO_Story_11_NagaGuard_010hb_Male_Naga_Story_Faelin_Mission10aHeroPower_01, VO_Story_11_NagaGuard_010hb_Male_Naga_Story_Faelin_Mission10aHeroPower_02, VO_Story_11_NagaGuard_010hb_Male_Naga_Story_Faelin_Mission10aHeroPower_03 };

	private HashSet<string> m_playedLines = new HashSet<string>();

	private static Map<GameEntityOption, bool> InitBooleanOptions()
	{
		return new Map<GameEntityOption, bool> { 
		{
			GameEntityOption.DO_OPENING_TAUNTS,
			false
		} };
	}

	public BoH_Faelin_10A()
	{
		m_gameOptions.AddBooleanOptions(s_booleanOptions);
	}

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> m_SoundFiles = new List<string>
		{
			VO_Story_11_NagaGuard_010hb_Male_Naga_Story_Faelin_Mission10aStart_01, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission10aStart_01, VO_Story_11_NagaGuard_010hb_Male_Naga_Story_Faelin_Mission10aStart_02, VO_Story_11_NagaGuard_010hb_Male_Naga_Story_Faelin_Mission10aExchangeA_01, VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission10aExchangeA_01, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission10aExchangeA_01, VO_Story_11_NagaGuard_010hb_Male_Naga_Story_Faelin_Mission10aExchangeB_01, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission10aExchangeC_01, VO_Story_11_NagaGuard_010hb_Male_Naga_Story_Faelin_Mission10aExchangeC_01, VO_Story_11_Finley_009hp_Male_Murloc_Story_Faelin_Mission10aExchangeC_01,
			VO_Story_11_Caye_012hp_Female_Nightborne_Story_Faelin_Mission10aExchangeD_01, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission10aExchangeD_01, VO_Story_11_Finley_009hp_Male_Murloc_Story_Faelin_Mission10aExchangeE_01, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission10aExchangeE_01, VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission10aExchangeF_01, VO_Story_11_Caye_012hp_Female_Nightborne_Story_Faelin_Mission10aExchangeF_01, VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission10aExchangeF_02, VO_Story_11_NagaGuard_010hb_Male_Naga_Story_Faelin_Mission10aVictory_01, VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission10aVictory_01, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission10aVictory_01,
			VO_Story_11_NagaGuard_010hb_Male_Naga_Story_Faelin_Mission10aEmoteResponse_01, VO_Story_11_NagaGuard_010hb_Male_Naga_Story_Faelin_Mission10aHeroPower_01, VO_Story_11_NagaGuard_010hb_Male_Naga_Story_Faelin_Mission10aHeroPower_02, VO_Story_11_NagaGuard_010hb_Male_Naga_Story_Faelin_Mission10aHeroPower_03, VO_Story_11_NagaGuard_010hb_Male_Naga_Story_Faelin_Mission10aLoss_01, VO_Story_11_NagaGuard_010hb_Male_Naga_Story_Faelin_Mission10aIdle_01, VO_Story_11_NagaGuard_010hb_Male_Naga_Story_Faelin_Mission10aIdle_02, VO_Story_11_NagaGuard_010hb_Male_Naga_Story_Faelin_Mission10aIdle_03, VO_Story_11_NagaGuard_010hb_Male_Naga_Story_Faelin_Mission10aDeath_01, VO_Story_11_Mission10a_Lorebook1_Female_NightElf_Story_Faelin_Mission10a_Lorebook_1_01,
			VO_Story_11_Mission10a_Lorebook2_Female_NightElf_Story_Faelin_Mission10a_Lorebook_2_01, VO_Story_11_Finley_009hp_Male_Murloc_Story_Faelin_Mission10a_Lorebook_2_01, VO_Story_11_Finley_009hp_Male_Murloc_Story_Faelin_Mission10a_Lorebook_3_01, VO_Story_11_Nemesis_005hb_Male_Human_Story_Faelin_Mission10a_Lorebook_3_01, VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission10a_Lorebook_4_01
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
			yield return MissionPlayVO(enemyActor, VO_Story_11_NagaGuard_010hb_Male_Naga_Story_Faelin_Mission10aEmoteResponse_01);
			break;
		case 514:
			yield return MissionPlayVO(enemyActor, VO_Story_11_NagaGuard_010hb_Male_Naga_Story_Faelin_Mission10aStart_01);
			yield return MissionPlayVO(friendlyActor, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission10aStart_01);
			yield return MissionPlayVO(enemyActor, VO_Story_11_NagaGuard_010hb_Male_Naga_Story_Faelin_Mission10aStart_02);
			break;
		case 507:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(enemyActor, VO_Story_11_NagaGuard_010hb_Male_Naga_Story_Faelin_Mission10aLoss_01);
			GameState.Get().SetBusy(busy: false);
			break;
		case 504:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(IniBrassRing, VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission10aVictory_01);
			yield return MissionPlayVO(friendlyActor, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission10aVictory_01);
			GameState.Get().SetBusy(busy: false);
			break;
		case 101:
			yield return MissionPlayVO(friendlyActor, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission10aExchangeA_01);
			yield return MissionPlayVO(enemyActor, VO_Story_11_NagaGuard_010hb_Male_Naga_Story_Faelin_Mission10aExchangeB_01);
			yield return MissionPlayVO(friendlyActor, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission10aExchangeC_01);
			yield return MissionPlayVO(enemyActor, VO_Story_11_NagaGuard_010hb_Male_Naga_Story_Faelin_Mission10aExchangeC_01);
			break;
		case 102:
			yield return MissionPlayVO(friendlyActor, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission10aExchangeA_01);
			yield return MissionPlayVO(enemyActor, VO_Story_11_NagaGuard_010hb_Male_Naga_Story_Faelin_Mission10aExchangeB_01);
			yield return MissionPlayVO(friendlyActor, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission10aExchangeC_01);
			yield return MissionPlayVO(enemyActor, VO_Story_11_NagaGuard_010hb_Male_Naga_Story_Faelin_Mission10aExchangeC_01);
			yield return MissionPlayVO(FinleyBrassRing, VO_Story_11_Finley_009hp_Male_Murloc_Story_Faelin_Mission10aExchangeC_01);
			break;
		case 103:
			yield return MissionPlayVO(GraceBrassRing, VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission10aExchangeF_01);
			yield return MissionPlayVO(CayeBrassRing, VO_Story_11_Caye_012hp_Female_Nightborne_Story_Faelin_Mission10aExchangeF_01);
			yield return MissionPlayVO(GraceBrassRing, VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission10aExchangeF_02);
			break;
		case 104:
			yield return MissionPlayVO(FinleyBrassRing, VO_Story_11_Finley_009hp_Male_Murloc_Story_Faelin_Mission10aExchangeE_01);
			yield return MissionPlayVO(friendlyActor, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission10aExchangeE_01);
			break;
		case 105:
			yield return MissionPlayVO(BookBrassRing, VO_Story_11_Mission10a_Lorebook1_Female_NightElf_Story_Faelin_Mission10a_Lorebook_1_01);
			break;
		case 106:
			yield return MissionPlayVO(BookBrassRing, VO_Story_11_Mission10a_Lorebook2_Female_NightElf_Story_Faelin_Mission10a_Lorebook_2_01);
			break;
		case 107:
			yield return MissionPlayVO(BookBrassRing, VO_Story_11_Mission10a_Lorebook2_Female_NightElf_Story_Faelin_Mission10a_Lorebook_2_01);
			yield return MissionPlayVO(FinleyBrassRing, VO_Story_11_Finley_009hp_Male_Murloc_Story_Faelin_Mission10a_Lorebook_2_01);
			break;
		case 108:
			yield return MissionPlayVO(BookBrassRing, VO_Story_11_Finley_009hp_Male_Murloc_Story_Faelin_Mission10a_Lorebook_3_01);
			break;
		case 109:
			yield return MissionPlayVO(BookBrassRing, VO_Story_11_Nemesis_005hb_Male_Human_Story_Faelin_Mission10a_Lorebook_3_01);
			yield return MissionPlayVO(GraceBrassRing, VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission10a_Lorebook_4_01);
			break;
		case 110:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(enemyActor, VO_Story_11_NagaGuard_010hb_Male_Naga_Story_Faelin_Mission10aVictory_01);
			GameState.Get().SetBusy(busy: false);
			break;
		case 228:
			if (!(m_popup != null))
			{
				GameState.Get().SetBusy(busy: true);
				m_popup = NotificationManager.Get().CreatePopupText(UserAttentionBlocker.NONE, popUpPos, TutorialEntity.GetTextScale() * popUpScale, GameStrings.Get(m_popUpInfo[missionEvent][0]), convertLegacyPosition: false, NotificationManager.PopupTextType.FANCY);
				yield return GameEntity.Coroutines.StartCoroutine(PlaySoundAndBlockSpeech(VO_Story_11_Mission10a_Lorebook1_Female_NightElf_Story_Faelin_Mission10a_Lorebook_1_01, Notification.SpeechBubbleDirection.None, GetEnemyActorByCardId("Story_11_Mission1_Lorebook1"), 2.5f));
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
				yield return GameEntity.Coroutines.StartCoroutine(PlaySoundAndBlockSpeech(VO_Story_11_Mission10a_Lorebook2_Female_NightElf_Story_Faelin_Mission10a_Lorebook_2_01, Notification.SpeechBubbleDirection.None, GetEnemyActorByCardId("Story_11_Mission1_Lorebook1"), 2.5f));
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
				yield return GameEntity.Coroutines.StartCoroutine(PlaySoundAndBlockSpeech(VO_Story_11_Mission10a_Lorebook2_Female_NightElf_Story_Faelin_Mission10a_Lorebook_2_01, Notification.SpeechBubbleDirection.None, GetEnemyActorByCardId("Story_11_Mission1_Lorebook1"), 2.5f));
				NotificationManager.Get().DestroyNotification(m_popup, 0f);
				m_popup = null;
				GameState.Get().SetBusy(busy: false);
				yield return MissionPlayVO(FinleyBrassRing, VO_Story_11_Finley_009hp_Male_Murloc_Story_Faelin_Mission10a_Lorebook_2_01);
			}
			break;
		case 231:
			if (!(m_popup != null))
			{
				GameState.Get().SetBusy(busy: true);
				m_popup = NotificationManager.Get().CreatePopupText(UserAttentionBlocker.NONE, popUpPos, TutorialEntity.GetTextScale() * popUpScale, GameStrings.Get(m_popUpInfo[missionEvent][0]), convertLegacyPosition: false, NotificationManager.PopupTextType.FANCY);
				yield return GameEntity.Coroutines.StartCoroutine(PlaySoundAndBlockSpeech(VO_Story_11_Finley_009hp_Male_Murloc_Story_Faelin_Mission10a_Lorebook_3_01, Notification.SpeechBubbleDirection.None, GetEnemyActorByCardId("Story_11_Mission1_Lorebook1"), 2.5f));
				NotificationManager.Get().DestroyNotification(m_popup, 0f);
				m_popup = null;
				GameState.Get().SetBusy(busy: false);
			}
			break;
		case 232:
			if (!(m_popup != null))
			{
				GameState.Get().SetBusy(busy: true);
				m_popup = NotificationManager.Get().CreatePopupText(UserAttentionBlocker.NONE, popUpPos, TutorialEntity.GetTextScale() * popUpScale, GameStrings.Get(m_popUpInfo[missionEvent][0]), convertLegacyPosition: false, NotificationManager.PopupTextType.FANCY);
				yield return GameEntity.Coroutines.StartCoroutine(PlaySoundAndBlockSpeech(VO_Story_11_Nemesis_005hb_Male_Human_Story_Faelin_Mission10a_Lorebook_3_01, Notification.SpeechBubbleDirection.None, GetEnemyActorByCardId("Story_11_Mission1_Lorebook1"), 2.5f));
				NotificationManager.Get().DestroyNotification(m_popup, 0f);
				m_popup = null;
				GameState.Get().SetBusy(busy: false);
				yield return MissionPlayVO(GraceBrassRing, VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission10a_Lorebook_4_01);
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
			yield return MissionPlayVO(enemyActor, VO_Story_11_NagaGuard_010hb_Male_Naga_Story_Faelin_Mission10aExchangeA_01);
			yield return MissionPlayVO(IniBrassRing, VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission10aExchangeA_01);
			break;
		case 5:
			yield return MissionPlayVO(CayeBrassRing, VO_Story_11_Caye_012hp_Female_Nightborne_Story_Faelin_Mission10aExchangeD_01);
			yield return MissionPlayVO(friendlyActor, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission10aExchangeD_01);
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
			if (clickedEntity.GetCardId() == "Story_11_Mission10a_Lorebook1")
			{
				if (m_popup == null)
				{
					Network.Get().SendClientScriptGameEvent(ClientScriptGameEventType.MISSION_EVENT, 228);
					HandleMissionEvent(228);
				}
				return false;
			}
			if (clickedEntity.GetCardId() == "Story_11_Mission10a_Lorebook2")
			{
				if (m_popup == null)
				{
					Network.Get().SendClientScriptGameEvent(ClientScriptGameEventType.MISSION_EVENT, 229);
					HandleMissionEvent(229);
				}
				return false;
			}
			if (clickedEntity.GetCardId() == "Story_11_Mission10a_Lorebook2b")
			{
				if (m_popup == null)
				{
					Network.Get().SendClientScriptGameEvent(ClientScriptGameEventType.MISSION_EVENT, 230);
					HandleMissionEvent(230);
				}
				return false;
			}
			if (clickedEntity.GetCardId() == "Story_11_Mission10a_Lorebook3")
			{
				if (m_popup == null)
				{
					Network.Get().SendClientScriptGameEvent(ClientScriptGameEventType.MISSION_EVENT, 231);
					HandleMissionEvent(231);
				}
				return false;
			}
			if (clickedEntity.GetCardId() == "Story_11_Mission10a_Lorebook4")
			{
				if (m_popup == null)
				{
					Network.Get().SendClientScriptGameEvent(ClientScriptGameEventType.MISSION_EVENT, 232);
					HandleMissionEvent(232);
				}
				return false;
			}
		}
		return true;
	}
}
