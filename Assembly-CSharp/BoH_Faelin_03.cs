using System.Collections;
using System.Collections.Generic;
using Blizzard.T5.Core;
using UnityEngine;

public class BoH_Faelin_03 : BoH_Faelin_Dungeon
{
	private static Map<GameEntityOption, bool> s_booleanOptions = InitBooleanOptions();

	private static readonly AssetReference VO_Story_11_Caye_012hp_Female_Nightborne_Story_Faelin_Mission3ExchangeA_01 = new AssetReference("VO_Story_11_Caye_012hp_Female_Nightborne_Story_Faelin_Mission3ExchangeA_01.prefab:3a38123ee80bc454aa20562d5fa28431");

	private static readonly AssetReference VO_Story_11_Caye_012hp_Female_Nightborne_Story_Faelin_PreMission3_01 = new AssetReference("VO_Story_11_Caye_012hp_Female_Nightborne_Story_Faelin_PreMission3_01.prefab:ebad3ecfde882e4419101d6c61eff279");

	private static readonly AssetReference VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission3ExchangeD_01 = new AssetReference("VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission3ExchangeD_01.prefab:7b2c434b96007994f83de41993e07cf0");

	private static readonly AssetReference VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission3ExchangeD_02 = new AssetReference("VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission3ExchangeD_02.prefab:1385940a0f6555b4a9b26677b2fdfd08");

	private static readonly AssetReference VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission3ExchangeF_01 = new AssetReference("VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission3ExchangeF_01.prefab:24f35d5e30beb2c4da37feb2af7d6c79");

	private static readonly AssetReference VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission3Start_01 = new AssetReference("VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission3Start_01.prefab:e3cf7948eee52324a819c67bb20a9b9d");

	private static readonly AssetReference VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission3Victory_01 = new AssetReference("VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission3Victory_01.prefab:7ff9734334e458e4ca9dae4d17100b4c");

	private static readonly AssetReference VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_PreMission3_01 = new AssetReference("VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_PreMission3_01.prefab:8668ef6a2fdb86d41b7d761e0f73b015");

	private static readonly AssetReference VO_Story_11_Finley_009hp_Male_Murloc_Story_Faelin_Mission3ExchangeE_01 = new AssetReference("VO_Story_11_Finley_009hp_Male_Murloc_Story_Faelin_Mission3ExchangeE_01.prefab:4af6b65a06a22224b9c8c2c698b1d349");

	private static readonly AssetReference VO_Story_11_Finley_009hp_Male_Murloc_Story_Faelin_Mission3Victory_01 = new AssetReference("VO_Story_11_Finley_009hp_Male_Murloc_Story_Faelin_Mission3Victory_01.prefab:89153b184de50054fb1393d1184b9285");

	private static readonly AssetReference VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission3_Lorebook_3_01 = new AssetReference("VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission3_Lorebook_3_01.prefab:7658d3118ce4d5341813574923030033");

	private static readonly AssetReference VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission3ExchangeA_01 = new AssetReference("VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission3ExchangeA_01.prefab:af02b67048e0f48499da01b9d30b2def");

	private static readonly AssetReference VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission3ExchangeB_01 = new AssetReference("VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission3ExchangeB_01.prefab:672668948097d464f97a26118bea9ccb");

	private static readonly AssetReference VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission3Victory_01 = new AssetReference("VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission3Victory_01.prefab:20509b742b85d7b4bb9d31bb72fdcb21");

	private static readonly AssetReference VO_Story_11_Halus_010hp_Male_BloodElf_Story_Faelin_Mission3ExchangeC_01 = new AssetReference("VO_Story_11_Halus_010hp_Male_BloodElf_Story_Faelin_Mission3ExchangeC_01.prefab:cba4fd39ec2446d40b94575e32948b09");

	private static readonly AssetReference VO_Story_11_Halus_010hp_Male_BloodElf_Story_Faelin_Mission3Victory_01 = new AssetReference("VO_Story_11_Halus_010hp_Male_BloodElf_Story_Faelin_Mission3Victory_01.prefab:104517e1f5ef31e4b8c98b5e54a08e9a");

	private static readonly AssetReference VO_Story_11_Hooktusk_003hb_Female_Troll_Story_Faelin_Mission3_Lorebook_1_01 = new AssetReference("VO_Story_11_Hooktusk_003hb_Female_Troll_Story_Faelin_Mission3_Lorebook_1_01.prefab:77a9d52082d134848bbcfd097ff74b68");

	private static readonly AssetReference VO_Story_11_Hooktusk_003hb_Female_Troll_Story_Faelin_Mission3EmoteResponse_01 = new AssetReference("VO_Story_11_Hooktusk_003hb_Female_Troll_Story_Faelin_Mission3EmoteResponse_01.prefab:4f00924e383b07b4491645c11a0509eb");

	private static readonly AssetReference VO_Story_11_Hooktusk_003hb_Female_Troll_Story_Faelin_Mission3ExchangeC_01 = new AssetReference("VO_Story_11_Hooktusk_003hb_Female_Troll_Story_Faelin_Mission3ExchangeC_01.prefab:5fdd95af2bb0f184eb5cfc13639a7af1");

	private static readonly AssetReference VO_Story_11_Hooktusk_003hb_Female_Troll_Story_Faelin_Mission3ExchangeD_01 = new AssetReference("VO_Story_11_Hooktusk_003hb_Female_Troll_Story_Faelin_Mission3ExchangeD_01.prefab:ee3480b8aa69ecb44b5a986e16989300");

	private static readonly AssetReference VO_Story_11_Hooktusk_003hb_Female_Troll_Story_Faelin_Mission3ExchangeD_02 = new AssetReference("VO_Story_11_Hooktusk_003hb_Female_Troll_Story_Faelin_Mission3ExchangeD_02.prefab:9d0b671518804914b99ea3a1cb5c50c3");

	private static readonly AssetReference VO_Story_11_Hooktusk_003hb_Female_Troll_Story_Faelin_Mission3ExchangeE_01 = new AssetReference("VO_Story_11_Hooktusk_003hb_Female_Troll_Story_Faelin_Mission3ExchangeE_01.prefab:21dfa3a3a87a7324084bea13d212b2be");

	private static readonly AssetReference VO_Story_11_Hooktusk_003hb_Female_Troll_Story_Faelin_Mission3HeroPower_01 = new AssetReference("VO_Story_11_Hooktusk_003hb_Female_Troll_Story_Faelin_Mission3HeroPower_01.prefab:a7236135730219e45afb7b49f1549002");

	private static readonly AssetReference VO_Story_11_Hooktusk_003hb_Female_Troll_Story_Faelin_Mission3HeroPower_02 = new AssetReference("VO_Story_11_Hooktusk_003hb_Female_Troll_Story_Faelin_Mission3HeroPower_02.prefab:6f448c1e62d74ca43b42db48cc421c5d");

	private static readonly AssetReference VO_Story_11_Hooktusk_003hb_Female_Troll_Story_Faelin_Mission3HeroPower_03 = new AssetReference("VO_Story_11_Hooktusk_003hb_Female_Troll_Story_Faelin_Mission3HeroPower_03.prefab:0e79c3eafedb233489febc5d3a8d6e6b");

	private static readonly AssetReference VO_Story_11_Hooktusk_003hb_Female_Troll_Story_Faelin_Mission3Idle_01 = new AssetReference("VO_Story_11_Hooktusk_003hb_Female_Troll_Story_Faelin_Mission3Idle_01.prefab:f04660ca4e57d0d4d997d08ffc1a20f5");

	private static readonly AssetReference VO_Story_11_Hooktusk_003hb_Female_Troll_Story_Faelin_Mission3Idle_02 = new AssetReference("VO_Story_11_Hooktusk_003hb_Female_Troll_Story_Faelin_Mission3Idle_02.prefab:348885a77d85756428d7b261f21a04ba");

	private static readonly AssetReference VO_Story_11_Hooktusk_003hb_Female_Troll_Story_Faelin_Mission3Idle_03 = new AssetReference("VO_Story_11_Hooktusk_003hb_Female_Troll_Story_Faelin_Mission3Idle_03.prefab:38a5a2faa1b43da41abf2a747ff5b73e");

	private static readonly AssetReference VO_Story_11_Hooktusk_003hb_Female_Troll_Story_Faelin_Mission3Loss_01 = new AssetReference("VO_Story_11_Hooktusk_003hb_Female_Troll_Story_Faelin_Mission3Loss_01.prefab:85eec7d5f9d1ed646bd22b2981ec9d8f");

	private static readonly AssetReference VO_Story_11_Hooktusk_003hb_Female_Troll_Story_Faelin_Mission3Start_01 = new AssetReference("VO_Story_11_Hooktusk_003hb_Female_Troll_Story_Faelin_Mission3Start_01.prefab:13882cb043048cf41833b1aad48e4fc1");

	private static readonly AssetReference VO_Story_11_Hooktusk_003hb_Female_Troll_Story_Faelin_Mission3Victory_01 = new AssetReference("VO_Story_11_Hooktusk_003hb_Female_Troll_Story_Faelin_Mission3Victory_01.prefab:df01d93b8386abd4aa5c546c63152378");

	private static readonly AssetReference VO_Story_11_Hooktusk_003hb_Female_Troll_Story_Faelin_Mission3Victory_02 = new AssetReference("VO_Story_11_Hooktusk_003hb_Female_Troll_Story_Faelin_Mission3Victory_02.prefab:01bdd3440b1f2034488b910be5b4ad8f");

	private static readonly AssetReference VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission3ExchangeA_01 = new AssetReference("VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission3ExchangeA_01.prefab:e4083315edfca6949bcbd48f0284bc6c");

	private static readonly AssetReference VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission3ExchangeA_02 = new AssetReference("VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission3ExchangeA_02.prefab:803b87e8d51560446b6295969141f4d8");

	private static readonly AssetReference VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission3ExchangeB_01 = new AssetReference("VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission3ExchangeB_01.prefab:08b275c7685cc4249a4ac3e7c9ca7a5b");

	private static readonly AssetReference VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission3ExchangeC_01 = new AssetReference("VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission3ExchangeC_01.prefab:fdd770b41260247458c87718c21096d7");

	private static readonly AssetReference VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission3ExchangeC_02 = new AssetReference("VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission3ExchangeC_02.prefab:0feb125b4cabc6f4f991b3212782b890");

	private static readonly AssetReference VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission3ExchangeE_01 = new AssetReference("VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission3ExchangeE_01.prefab:c5e97a99c057d1540abc0d19d63462aa");

	private static readonly AssetReference VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission3ExchangeF_01 = new AssetReference("VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission3ExchangeF_01.prefab:90375f75683ac98409eaf034e5b0f541");

	private static readonly AssetReference VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission3ExchangeG_01 = new AssetReference("VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission3ExchangeG_01.prefab:3f2c93d0e9379ae44a27dad9ce584b2f");

	private static readonly AssetReference VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_PreMission3_01 = new AssetReference("VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_PreMission3_01.prefab:6a05f9fa6d645a64c93a1f11f9039a61");

	private static readonly AssetReference VO_Story_11_Mission3_Lorebook2_Female_Goblin_Story_Faelin_Mission3_Lorebook_2_01 = new AssetReference("VO_Story_11_Mission3_Lorebook2_Female_Goblin_Story_Faelin_Mission3_Lorebook_2_01.prefab:e2cbe3d77ef58284dacaf200b62b4624");

	private Notification m_popup;

	private Dictionary<int, string[]> m_popUpInfo = new Dictionary<int, string[]>
	{
		{
			228,
			new string[1] { "BOH_FAELIN_MISSION3_LOREBOOK_01" }
		},
		{
			229,
			new string[1] { "BOH_FAELIN_MISSION3_LOREBOOK_02" }
		},
		{
			230,
			new string[1] { "BOH_FAELIN_MISSION3_LOREBOOK_03" }
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

	private new List<string> m_BossIdleLines = new List<string> { VO_Story_11_Hooktusk_003hb_Female_Troll_Story_Faelin_Mission3Idle_01, VO_Story_11_Hooktusk_003hb_Female_Troll_Story_Faelin_Mission3Idle_02, VO_Story_11_Hooktusk_003hb_Female_Troll_Story_Faelin_Mission3Idle_03 };

	private List<string> m_BossUsesHeroPowerLines = new List<string> { VO_Story_11_Hooktusk_003hb_Female_Troll_Story_Faelin_Mission3HeroPower_01, VO_Story_11_Hooktusk_003hb_Female_Troll_Story_Faelin_Mission3HeroPower_02, VO_Story_11_Hooktusk_003hb_Female_Troll_Story_Faelin_Mission3HeroPower_03 };

	private HashSet<string> m_playedLines = new HashSet<string>();

	private static Map<GameEntityOption, bool> InitBooleanOptions()
	{
		return new Map<GameEntityOption, bool> { 
		{
			GameEntityOption.DO_OPENING_TAUNTS,
			false
		} };
	}

	public BoH_Faelin_03()
	{
		m_gameOptions.AddBooleanOptions(s_booleanOptions);
	}

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> m_SoundFiles = new List<string>
		{
			VO_Story_11_Hooktusk_003hb_Female_Troll_Story_Faelin_Mission3Start_01, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission3Start_01, VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission3ExchangeA_01, VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission3ExchangeA_01, VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission3ExchangeA_02, VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission3ExchangeB_01, VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission3ExchangeB_01, VO_Story_11_Hooktusk_003hb_Female_Troll_Story_Faelin_Mission3ExchangeC_01, VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission3ExchangeC_01, VO_Story_11_Halus_010hp_Male_BloodElf_Story_Faelin_Mission3ExchangeC_01,
			VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission3ExchangeC_02, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission3ExchangeD_01, VO_Story_11_Hooktusk_003hb_Female_Troll_Story_Faelin_Mission3ExchangeD_01, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission3ExchangeD_02, VO_Story_11_Hooktusk_003hb_Female_Troll_Story_Faelin_Mission3ExchangeD_02, VO_Story_11_Hooktusk_003hb_Female_Troll_Story_Faelin_Mission3ExchangeE_01, VO_Story_11_Finley_009hp_Male_Murloc_Story_Faelin_Mission3ExchangeE_01, VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission3ExchangeE_01, VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission3ExchangeF_01, VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission3ExchangeG_01,
			VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission3ExchangeF_01, VO_Story_11_Hooktusk_003hb_Female_Troll_Story_Faelin_Mission3Victory_01, VO_Story_11_Hooktusk_003hb_Female_Troll_Story_Faelin_Mission3Victory_02, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission3Victory_01, VO_Story_11_Finley_009hp_Male_Murloc_Story_Faelin_Mission3Victory_01, VO_Story_11_Halus_010hp_Male_BloodElf_Story_Faelin_Mission3Victory_01, VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission3Victory_01, VO_Story_11_Hooktusk_003hb_Female_Troll_Story_Faelin_Mission3EmoteResponse_01, VO_Story_11_Hooktusk_003hb_Female_Troll_Story_Faelin_Mission3HeroPower_01, VO_Story_11_Hooktusk_003hb_Female_Troll_Story_Faelin_Mission3HeroPower_02,
			VO_Story_11_Hooktusk_003hb_Female_Troll_Story_Faelin_Mission3HeroPower_03, VO_Story_11_Hooktusk_003hb_Female_Troll_Story_Faelin_Mission3Loss_01, VO_Story_11_Hooktusk_003hb_Female_Troll_Story_Faelin_Mission3Idle_01, VO_Story_11_Hooktusk_003hb_Female_Troll_Story_Faelin_Mission3Idle_02, VO_Story_11_Hooktusk_003hb_Female_Troll_Story_Faelin_Mission3Idle_03, VO_Story_11_Hooktusk_003hb_Female_Troll_Story_Faelin_Mission3_Lorebook_1_01, VO_Story_11_Mission3_Lorebook2_Female_Goblin_Story_Faelin_Mission3_Lorebook_2_01, VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission3_Lorebook_3_01
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
		m_OverrideMusicTrack = MusicPlaylistType.InGame_TRLFinalBoss;
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
			yield return MissionPlayVO(enemyActor, VO_Story_11_Hooktusk_003hb_Female_Troll_Story_Faelin_Mission3EmoteResponse_01);
			break;
		case 514:
			yield return MissionPlayVO(enemyActor, VO_Story_11_Hooktusk_003hb_Female_Troll_Story_Faelin_Mission3Start_01);
			yield return MissionPlayVO(friendlyActor, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission3Start_01);
			break;
		case 507:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(enemyActor, VO_Story_11_Hooktusk_003hb_Female_Troll_Story_Faelin_Mission3Loss_01);
			GameState.Get().SetBusy(busy: false);
			break;
		case 504:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(enemyActor, VO_Story_11_Hooktusk_003hb_Female_Troll_Story_Faelin_Mission3Victory_01);
			yield return MissionPlayVO(enemyActor, VO_Story_11_Hooktusk_003hb_Female_Troll_Story_Faelin_Mission3Victory_02);
			yield return MissionPlayVO(friendlyActor, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission3Victory_01);
			yield return MissionPlayVO(HalusBrassRing, VO_Story_11_Halus_010hp_Male_BloodElf_Story_Faelin_Mission3Victory_01);
			GameState.Get().SetBusy(busy: false);
			break;
		case 101:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(enemyActor, VO_Story_11_Hooktusk_003hb_Female_Troll_Story_Faelin_Mission3Victory_01);
			yield return MissionPlayVO(enemyActor, VO_Story_11_Hooktusk_003hb_Female_Troll_Story_Faelin_Mission3Victory_02);
			yield return MissionPlayVO(friendlyActor, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission3Victory_01);
			yield return MissionPlayVO(GraceBrassRing, VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission3Victory_01);
			GameState.Get().SetBusy(busy: false);
			break;
		case 102:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(enemyActor, VO_Story_11_Hooktusk_003hb_Female_Troll_Story_Faelin_Mission3Victory_01);
			yield return MissionPlayVO(enemyActor, VO_Story_11_Hooktusk_003hb_Female_Troll_Story_Faelin_Mission3Victory_02);
			yield return MissionPlayVO(friendlyActor, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission3Victory_01);
			yield return MissionPlayVO(FinleyBrassRing, VO_Story_11_Finley_009hp_Male_Murloc_Story_Faelin_Mission3Victory_01);
			GameState.Get().SetBusy(busy: false);
			break;
		case 103:
			yield return MissionPlayVO(IniBrassRing, VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission3ExchangeA_01);
			break;
		case 104:
			yield return MissionPlayVO(IniBrassRing, VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission3ExchangeA_01);
			yield return MissionPlayVO(GraceBrassRing, VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission3ExchangeA_01);
			yield return MissionPlayVO(IniBrassRing, VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission3ExchangeA_02);
			break;
		case 105:
			yield return MissionPlayVO(GraceBrassRing, VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission3ExchangeB_01);
			yield return MissionPlayVO(IniBrassRing, VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission3ExchangeB_01);
			break;
		case 106:
			yield return MissionPlayVO(enemyActor, VO_Story_11_Hooktusk_003hb_Female_Troll_Story_Faelin_Mission3ExchangeC_01);
			yield return MissionPlayVO(IniBrassRing, VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission3ExchangeC_01);
			yield return MissionPlayVO(HalusBrassRing, VO_Story_11_Halus_010hp_Male_BloodElf_Story_Faelin_Mission3ExchangeC_01);
			yield return MissionPlayVO(IniBrassRing, VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission3ExchangeC_02);
			break;
		case 107:
			yield return MissionPlayVO(enemyActor, VO_Story_11_Hooktusk_003hb_Female_Troll_Story_Faelin_Mission3ExchangeC_01);
			yield return MissionPlayVO(IniBrassRing, VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission3ExchangeC_01);
			break;
		case 108:
			yield return MissionPlayVO(enemyActor, VO_Story_11_Hooktusk_003hb_Female_Troll_Story_Faelin_Mission3ExchangeE_01);
			break;
		case 109:
			yield return MissionPlayVO(enemyActor, VO_Story_11_Hooktusk_003hb_Female_Troll_Story_Faelin_Mission3ExchangeE_01);
			yield return MissionPlayVO(FinleyBrassRing, VO_Story_11_Finley_009hp_Male_Murloc_Story_Faelin_Mission3ExchangeE_01);
			break;
		case 110:
			yield return MissionPlayVO(IniBrassRing, VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission3ExchangeE_01);
			break;
		case 111:
			yield return MissionPlayVO(IniBrassRing, VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission3ExchangeF_01);
			break;
		case 112:
			yield return MissionPlayVO(IniBrassRing, VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission3ExchangeG_01);
			yield return MissionPlayVO(friendlyActor, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission3ExchangeF_01);
			break;
		case 228:
			if (!(m_popup != null))
			{
				GameState.Get().SetBusy(busy: true);
				m_popup = NotificationManager.Get().CreatePopupText(UserAttentionBlocker.NONE, popUpPos, TutorialEntity.GetTextScale() * popUpScale, GameStrings.Get(m_popUpInfo[missionEvent][0]), convertLegacyPosition: false, NotificationManager.PopupTextType.FANCY);
				yield return GameEntity.Coroutines.StartCoroutine(PlaySoundAndBlockSpeech(VO_Story_11_Hooktusk_003hb_Female_Troll_Story_Faelin_Mission3_Lorebook_1_01, Notification.SpeechBubbleDirection.None, GetEnemyActorByCardId("Story_11_Mission1_Lorebook1"), 2.5f));
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
				yield return GameEntity.Coroutines.StartCoroutine(PlaySoundAndBlockSpeech(VO_Story_11_Mission3_Lorebook2_Female_Goblin_Story_Faelin_Mission3_Lorebook_2_01, Notification.SpeechBubbleDirection.None, GetEnemyActorByCardId("Story_11_Mission1_Lorebook1"), 2.5f));
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
				yield return GameEntity.Coroutines.StartCoroutine(PlaySoundAndBlockSpeech(VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission3_Lorebook_3_01, Notification.SpeechBubbleDirection.None, GetEnemyActorByCardId("Story_11_Mission1_Lorebook1"), 2.5f));
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
		if (turn == 9)
		{
			yield return MissionPlayVO(friendlyActor, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission3ExchangeD_01);
			yield return MissionPlayVO(enemyActor, VO_Story_11_Hooktusk_003hb_Female_Troll_Story_Faelin_Mission3ExchangeD_01);
			yield return MissionPlayVO(friendlyActor, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission3ExchangeD_02);
			yield return MissionPlayVO(enemyActor, VO_Story_11_Hooktusk_003hb_Female_Troll_Story_Faelin_Mission3ExchangeD_02);
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
			if (clickedEntity.GetCardId() == "Story_11_Mission3_Lorebook1")
			{
				if (m_popup == null)
				{
					Network.Get().SendClientScriptGameEvent(ClientScriptGameEventType.MISSION_EVENT, 228);
					HandleMissionEvent(228);
				}
				return false;
			}
			if (clickedEntity.GetCardId() == "Story_11_Mission3_Lorebook2")
			{
				if (m_popup == null)
				{
					Network.Get().SendClientScriptGameEvent(ClientScriptGameEventType.MISSION_EVENT, 229);
					HandleMissionEvent(229);
				}
				return false;
			}
			if (clickedEntity.GetCardId() == "Story_11_Mission3_Lorebook3")
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
