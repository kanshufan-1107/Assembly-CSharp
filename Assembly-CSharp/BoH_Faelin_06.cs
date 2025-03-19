using System.Collections;
using System.Collections.Generic;
using Blizzard.T5.Core;
using UnityEngine;

public class BoH_Faelin_06 : BoH_Faelin_Dungeon
{
	private static Map<GameEntityOption, bool> s_booleanOptions = InitBooleanOptions();

	private static readonly AssetReference VO_Story_11_Captain_006hb_Female_Human_Story_Faelin_Mission6_Lorebook_1_01 = new AssetReference("VO_Story_11_Captain_006hb_Female_Human_Story_Faelin_Mission6_Lorebook_1_01.prefab:92996f49bf4cdb94d9a373a722ed2a6d");

	private static readonly AssetReference VO_Story_11_Captain_006hb_Female_Human_Story_Faelin_Mission6EmoteResponse_01 = new AssetReference("VO_Story_11_Captain_006hb_Female_Human_Story_Faelin_Mission6EmoteResponse_01.prefab:db52af761ecc44447aea061bec01a89b");

	private static readonly AssetReference VO_Story_11_Captain_006hb_Female_Human_Story_Faelin_Mission6ExchangeA_01 = new AssetReference("VO_Story_11_Captain_006hb_Female_Human_Story_Faelin_Mission6ExchangeA_01.prefab:9d890cb2fd540d54cb44faf8e768bf0e");

	private static readonly AssetReference VO_Story_11_Captain_006hb_Female_Human_Story_Faelin_Mission6ExchangeA_02 = new AssetReference("VO_Story_11_Captain_006hb_Female_Human_Story_Faelin_Mission6ExchangeA_02.prefab:32f6eee71dda9cf4ea64ed74c475fe1e");

	private static readonly AssetReference VO_Story_11_Captain_006hb_Female_Human_Story_Faelin_Mission6ExchangeB_01 = new AssetReference("VO_Story_11_Captain_006hb_Female_Human_Story_Faelin_Mission6ExchangeB_01.prefab:ca465950bf3d82646ab27e0870eb2054");

	private static readonly AssetReference VO_Story_11_Captain_006hb_Female_Human_Story_Faelin_Mission6ExchangeC_01 = new AssetReference("VO_Story_11_Captain_006hb_Female_Human_Story_Faelin_Mission6ExchangeC_01.prefab:25a9dd140f213544ab2ef43b470aa5eb");

	private static readonly AssetReference VO_Story_11_Captain_006hb_Female_Human_Story_Faelin_Mission6ExchangeD_01 = new AssetReference("VO_Story_11_Captain_006hb_Female_Human_Story_Faelin_Mission6ExchangeD_01.prefab:591303bea1f447b4993f3de70c877e52");

	private static readonly AssetReference VO_Story_11_Captain_006hb_Female_Human_Story_Faelin_Mission6ExchangeD_02 = new AssetReference("VO_Story_11_Captain_006hb_Female_Human_Story_Faelin_Mission6ExchangeD_02.prefab:9b4121263bd9a82489253e168e1ee7ac");

	private static readonly AssetReference VO_Story_11_Captain_006hb_Female_Human_Story_Faelin_Mission6Idle_01 = new AssetReference("VO_Story_11_Captain_006hb_Female_Human_Story_Faelin_Mission6Idle_01.prefab:022e6e25e24de694891dce3955c9e3e8");

	private static readonly AssetReference VO_Story_11_Captain_006hb_Female_Human_Story_Faelin_Mission6Idle_02 = new AssetReference("VO_Story_11_Captain_006hb_Female_Human_Story_Faelin_Mission6Idle_02.prefab:db417e5228d6cf444ac024b3477e8137");

	private static readonly AssetReference VO_Story_11_Captain_006hb_Female_Human_Story_Faelin_Mission6Idle_03 = new AssetReference("VO_Story_11_Captain_006hb_Female_Human_Story_Faelin_Mission6Idle_03.prefab:46e1fab81bf61a1498a6a1d52dd7a21b");

	private static readonly AssetReference VO_Story_11_Captain_006hb_Female_Human_Story_Faelin_Mission6Loss_01 = new AssetReference("VO_Story_11_Captain_006hb_Female_Human_Story_Faelin_Mission6Loss_01.prefab:0df3724e7781d1f43bcfe999a0480f70");

	private static readonly AssetReference VO_Story_11_Captain_006hb_Female_Human_Story_Faelin_Mission6Start_01 = new AssetReference("VO_Story_11_Captain_006hb_Female_Human_Story_Faelin_Mission6Start_01.prefab:5a4f4bbcf0e8fcb439564c2574fe716e");

	private static readonly AssetReference VO_Story_11_Captain_006hb_Female_Human_Story_Faelin_Mission6Start_02 = new AssetReference("VO_Story_11_Captain_006hb_Female_Human_Story_Faelin_Mission6Start_02.prefab:71a9416f888bd1c4cad726143938fddf");

	private static readonly AssetReference VO_Story_11_Captain_006hb_Female_Human_Story_Faelin_Mission6Victory_01 = new AssetReference("VO_Story_11_Captain_006hb_Female_Human_Story_Faelin_Mission6Victory_01.prefab:dfbf570666a57a1458a403dabc947925");

	private static readonly AssetReference VO_Story_11_Captain_006hb_Female_Human_Story_Faelin_Mission6Victory_02 = new AssetReference("VO_Story_11_Captain_006hb_Female_Human_Story_Faelin_Mission6Victory_02.prefab:4f79dec3168fa964ba4fd208bd9e93e6");

	private static readonly AssetReference VO_Story_11_Captain_006hb_Female_Human_Story_Faelin_Mission6Victory_03 = new AssetReference("VO_Story_11_Captain_006hb_Female_Human_Story_Faelin_Mission6Victory_03.prefab:7f66ffa297398b34da4a28c80f0d9a5d");

	private static readonly AssetReference VO_Story_11_Caye_012hp_Female_Nightborne_Story_Faelin_PreMission6_01 = new AssetReference("VO_Story_11_Caye_012hp_Female_Nightborne_Story_Faelin_PreMission6_01.prefab:4c144ab2325a64f4b87cf4c38986c49f");

	private static readonly AssetReference VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission6_Lorebook_2_01 = new AssetReference("VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission6_Lorebook_2_01.prefab:8382f476f86607e46ab9c5dc67cfdfb1");

	private static readonly AssetReference VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission6_Lorebook_2_02 = new AssetReference("VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission6_Lorebook_2_02.prefab:e64b398cb7a25494ca0f9a5b4873fd7d");

	private static readonly AssetReference VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission6ExchangeA_01 = new AssetReference("VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission6ExchangeA_01.prefab:fae1bc60366d5674dbdbed42914c6a07");

	private static readonly AssetReference VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission6ExchangeD_01 = new AssetReference("VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission6ExchangeD_01.prefab:d466af0c1b88fd8499a269ba961cf0c6");

	private static readonly AssetReference VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission6Start_01 = new AssetReference("VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission6Start_01.prefab:7e3fb3429c83d6b4f9316289b8c43304");

	private static readonly AssetReference VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission6Victory_01 = new AssetReference("VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission6Victory_01.prefab:0cf589a9b45cba0438becfd0f7e90dec");

	private static readonly AssetReference VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission6Victory_02 = new AssetReference("VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission6Victory_02.prefab:5b9235736df33054083d0b70be30ffe1");

	private static readonly AssetReference VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_PreMission6_01 = new AssetReference("VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_PreMission6_01.prefab:cf9e0b1e04fd5184f858ca4b5689050c");

	private static readonly AssetReference VO_Story_11_Finley_009hp_Male_Murloc_Story_Faelin_Mission6ExchangeB_01 = new AssetReference("VO_Story_11_Finley_009hp_Male_Murloc_Story_Faelin_Mission6ExchangeB_01.prefab:3b90711d8a8d0604fabf4bf27e3b58e3");

	private static readonly AssetReference VO_Story_11_Finley_009hp_Male_Murloc_Story_Faelin_Mission6ExchangeC_01 = new AssetReference("VO_Story_11_Finley_009hp_Male_Murloc_Story_Faelin_Mission6ExchangeC_01.prefab:c0f5ecc1d0fa4424daaa02d58745b735");

	private static readonly AssetReference VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission6_Lorebook_3_01 = new AssetReference("VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission6_Lorebook_3_01.prefab:b89fd4aaf529d6643b4ec035d99ed9d9");

	private static readonly AssetReference VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission6ExchangeB_01 = new AssetReference("VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission6ExchangeB_01.prefab:2ebf19f0596111a49b26ef6632d8b74a");

	private static readonly AssetReference VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission6ExchangeC_01 = new AssetReference("VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission6ExchangeC_01.prefab:b7f929b62b6001d4a8581781947e1224");

	private static readonly AssetReference VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission6Victory_01 = new AssetReference("VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission6Victory_01.prefab:5e0277179b62e0249ae0b34c04fdd5ab");

	private static readonly AssetReference VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission6Victory_02 = new AssetReference("VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission6Victory_02.prefab:20cfb1150666c4040a26a78717b4388d");

	private static readonly AssetReference VO_Story_11_Halus_010hp_Male_BloodElf_Story_Faelin_Mission6ExchangeB_01 = new AssetReference("VO_Story_11_Halus_010hp_Male_BloodElf_Story_Faelin_Mission6ExchangeB_01.prefab:18173e9b80e80e5468a4b432cc2420a1");

	private static readonly AssetReference VO_Story_11_Halus_010hp_Male_BloodElf_Story_Faelin_Mission6ExchangeC_01 = new AssetReference("VO_Story_11_Halus_010hp_Male_BloodElf_Story_Faelin_Mission6ExchangeC_01.prefab:94264cb36be62a44abdc1c4c55c63e9d");

	private static readonly AssetReference VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission6_Lorebook_2_01 = new AssetReference("VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission6_Lorebook_2_01.prefab:79c584b2c2d524247a42c7834ce75595");

	private static readonly AssetReference VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission6_Lorebook_2_02 = new AssetReference("VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission6_Lorebook_2_02.prefab:65ac1717457531a438cf7373e5e5b7e3");

	private static readonly AssetReference VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_PreMission6_01 = new AssetReference("VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_PreMission6_01.prefab:cc45b9c8016f26041a256eebb38166be");

	private static readonly AssetReference VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_PreMission6_02 = new AssetReference("VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_PreMission6_02.prefab:07d6e01afecb22e479eed8c6834bcd4e");

	private Notification m_popup;

	private Dictionary<int, string[]> m_popUpInfo = new Dictionary<int, string[]>
	{
		{
			228,
			new string[1] { "BOH_FAELIN_06" }
		},
		{
			229,
			new string[1] { "BOH_FAELIN_MISSION6_LOREBOOK_01" }
		},
		{
			230,
			new string[1] { "BOH_FAELIN_MISSION6_LOREBOOK_02" }
		},
		{
			231,
			new string[1] { "BOH_FAELIN_MISSION6_LOREBOOK_03" }
		},
		{
			232,
			new string[1] { "BOH_FAELIN_06a" }
		}
	};

	public static readonly AssetReference IniBrassRing = new AssetReference("HS25-189_H_Ini_BrassRing_Quote.prefab:9f20435bdae99d24bae357f002abcf27");

	public static readonly AssetReference CayeBrassRing = new AssetReference("LC23-001_H_Caye_BrassRing_Quote.prefab:2364e8ce64f9c404fb569a86b17f3d01");

	public static readonly AssetReference BookBrassRing = new AssetReference("Wisdomball_Pop-up_BrassRing_Quote.prefab:896ee20514caff74db639aa7055838f6");

	public static readonly AssetReference FinleyBrassRing = new AssetReference("HS25-190_M_Finley_BrassRing_Quote.prefab:7123c129afe769e4aa9fb262a8f7c919");

	public static readonly AssetReference HalusBrassRing = new AssetReference("LC23-003_H_Halus_BrassRing_Quote.prefab:36e0aabb3ed4ce84599d9dd4e9ebdcf5");

	public static readonly AssetReference GraceBrassRing = new AssetReference("LC23-002_H_Grace_BrassRing_Quote.prefab:1f849ba2e303ff3449d34b96f960c96a");

	private float popUpScale = 1.65f;

	private Vector3 popUpPos;

	private new List<string> m_BossIdleLines = new List<string> { VO_Story_11_Captain_006hb_Female_Human_Story_Faelin_Mission6Idle_01, VO_Story_11_Captain_006hb_Female_Human_Story_Faelin_Mission6Idle_02, VO_Story_11_Captain_006hb_Female_Human_Story_Faelin_Mission6Idle_03 };

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

	public BoH_Faelin_06()
	{
		m_gameOptions.AddBooleanOptions(s_booleanOptions);
	}

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> m_SoundFiles = new List<string>
		{
			VO_Story_11_Captain_006hb_Female_Human_Story_Faelin_Mission6Start_01, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission6Start_01, VO_Story_11_Captain_006hb_Female_Human_Story_Faelin_Mission6Start_02, VO_Story_11_Captain_006hb_Female_Human_Story_Faelin_Mission6ExchangeA_01, VO_Story_11_Captain_006hb_Female_Human_Story_Faelin_Mission6ExchangeA_02, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission6ExchangeA_01, VO_Story_11_Captain_006hb_Female_Human_Story_Faelin_Mission6ExchangeB_01, VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission6ExchangeB_01, VO_Story_11_Finley_009hp_Male_Murloc_Story_Faelin_Mission6ExchangeB_01, VO_Story_11_Halus_010hp_Male_BloodElf_Story_Faelin_Mission6ExchangeB_01,
			VO_Story_11_Captain_006hb_Female_Human_Story_Faelin_Mission6ExchangeC_01, VO_Story_11_Finley_009hp_Male_Murloc_Story_Faelin_Mission6ExchangeC_01, VO_Story_11_Halus_010hp_Male_BloodElf_Story_Faelin_Mission6ExchangeC_01, VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission6ExchangeC_01, VO_Story_11_Captain_006hb_Female_Human_Story_Faelin_Mission6ExchangeD_01, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission6ExchangeD_01, VO_Story_11_Captain_006hb_Female_Human_Story_Faelin_Mission6ExchangeD_02, VO_Story_11_Captain_006hb_Female_Human_Story_Faelin_Mission6Victory_01, VO_Story_11_Captain_006hb_Female_Human_Story_Faelin_Mission6Victory_02, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission6Victory_01,
			VO_Story_11_Captain_006hb_Female_Human_Story_Faelin_Mission6Victory_03, VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission6Victory_01, VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission6Victory_02, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission6Victory_02, VO_Story_11_Captain_006hb_Female_Human_Story_Faelin_Mission6EmoteResponse_01, VO_Story_11_Captain_006hb_Female_Human_Story_Faelin_Mission6Loss_01, VO_Story_11_Captain_006hb_Female_Human_Story_Faelin_Mission6Idle_01, VO_Story_11_Captain_006hb_Female_Human_Story_Faelin_Mission6Idle_02, VO_Story_11_Captain_006hb_Female_Human_Story_Faelin_Mission6Idle_03, VO_Story_11_Captain_006hb_Female_Human_Story_Faelin_Mission6_Lorebook_1_01,
			VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission6_Lorebook_2_01, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission6_Lorebook_2_01, VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission6_Lorebook_2_02, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission6_Lorebook_2_02, VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission6_Lorebook_3_01
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

	public override void OnCreateGame()
	{
		base.OnCreateGame();
		m_OverrideMusicTrack = MusicPlaylistType.InGame_TSC_Leviathan;
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
			yield return MissionPlayVO(enemyActor, VO_Story_11_Captain_006hb_Female_Human_Story_Faelin_Mission6EmoteResponse_01);
			break;
		case 514:
			yield return MissionPlayVO(enemyActor, VO_Story_11_Captain_006hb_Female_Human_Story_Faelin_Mission6Start_01);
			yield return MissionPlayVO(friendlyActor, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission6Start_01);
			yield return MissionPlayVO(enemyActor, VO_Story_11_Captain_006hb_Female_Human_Story_Faelin_Mission6Start_02);
			break;
		case 507:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(enemyActor, VO_Story_11_Captain_006hb_Female_Human_Story_Faelin_Mission6Loss_01);
			GameState.Get().SetBusy(busy: false);
			break;
		case 504:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(enemyActor, VO_Story_11_Captain_006hb_Female_Human_Story_Faelin_Mission6Victory_01);
			yield return MissionPlayVO(enemyActor, VO_Story_11_Captain_006hb_Female_Human_Story_Faelin_Mission6Victory_02);
			yield return MissionPlayVO(friendlyActor, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission6Victory_01);
			yield return MissionPlayVO(enemyActor, VO_Story_11_Captain_006hb_Female_Human_Story_Faelin_Mission6Victory_03);
			GameState.Get().SetBusy(busy: false);
			break;
		case 101:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(enemyActor, VO_Story_11_Captain_006hb_Female_Human_Story_Faelin_Mission6Victory_01);
			yield return MissionPlayVO(enemyActor, VO_Story_11_Captain_006hb_Female_Human_Story_Faelin_Mission6Victory_02);
			yield return MissionPlayVO(friendlyActor, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission6Victory_01);
			yield return MissionPlayVO(GraceBrassRing, VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission6Victory_01);
			yield return MissionPlayVO(GraceBrassRing, VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission6Victory_02);
			yield return MissionPlayVO(friendlyActor, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission6Victory_02);
			GameState.Get().SetBusy(busy: false);
			break;
		case 107:
			yield return MissionPlayVO(enemyActor, VO_Story_11_Captain_006hb_Female_Human_Story_Faelin_Mission6ExchangeB_01);
			yield return MissionPlayVO(HalusBrassRing, VO_Story_11_Halus_010hp_Male_BloodElf_Story_Faelin_Mission6ExchangeB_01);
			break;
		case 108:
			yield return MissionPlayVO(enemyActor, VO_Story_11_Captain_006hb_Female_Human_Story_Faelin_Mission6ExchangeB_01);
			yield return MissionPlayVO(GraceBrassRing, VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission6ExchangeB_01);
			break;
		case 109:
			yield return MissionPlayVO(enemyActor, VO_Story_11_Captain_006hb_Female_Human_Story_Faelin_Mission6ExchangeB_01);
			yield return MissionPlayVO(FinleyBrassRing, VO_Story_11_Finley_009hp_Male_Murloc_Story_Faelin_Mission6ExchangeB_01);
			break;
		case 110:
			yield return MissionPlayVO(enemyActor, VO_Story_11_Captain_006hb_Female_Human_Story_Faelin_Mission6ExchangeC_01);
			yield return MissionPlayVO(HalusBrassRing, VO_Story_11_Halus_010hp_Male_BloodElf_Story_Faelin_Mission6ExchangeC_01);
			break;
		case 111:
			yield return MissionPlayVO(enemyActor, VO_Story_11_Captain_006hb_Female_Human_Story_Faelin_Mission6ExchangeC_01);
			yield return MissionPlayVO(GraceBrassRing, VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission6ExchangeC_01);
			break;
		case 112:
			yield return MissionPlayVO(enemyActor, VO_Story_11_Captain_006hb_Female_Human_Story_Faelin_Mission6ExchangeC_01);
			yield return MissionPlayVO(FinleyBrassRing, VO_Story_11_Finley_009hp_Male_Murloc_Story_Faelin_Mission6ExchangeC_01);
			break;
		case 113:
			yield return MissionPlayVO(BookBrassRing, VO_Story_11_Captain_006hb_Female_Human_Story_Faelin_Mission6_Lorebook_1_01);
			break;
		case 114:
			yield return MissionPlayVO(BookBrassRing, VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission6_Lorebook_2_01);
			yield return MissionPlayVO(friendlyActor, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission6_Lorebook_2_01);
			yield return MissionPlayVO(IniBrassRing, VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission6_Lorebook_2_02);
			yield return MissionPlayVO(friendlyActor, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission6_Lorebook_2_02);
			break;
		case 115:
			yield return MissionPlayVO(BookBrassRing, VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission6_Lorebook_3_01);
			break;
		case 228:
		{
			Notification popup = NotificationManager.Get().CreatePopupText(UserAttentionBlocker.NONE, popUpPos, TutorialEntity.GetTextScale() * popUpScale, GameStrings.Get(m_popUpInfo[missionEvent][0]), convertLegacyPosition: false, NotificationManager.PopupTextType.FANCY);
			yield return new WaitForSeconds(3.5f);
			NotificationManager.Get().DestroyNotification(popup, 0f);
			break;
		}
		case 229:
			if (!(m_popup != null))
			{
				GameState.Get().SetBusy(busy: true);
				m_popup = NotificationManager.Get().CreatePopupText(UserAttentionBlocker.NONE, popUpPos, TutorialEntity.GetTextScale() * popUpScale, GameStrings.Get(m_popUpInfo[missionEvent][0]), convertLegacyPosition: false, NotificationManager.PopupTextType.FANCY);
				yield return GameEntity.Coroutines.StartCoroutine(PlaySoundAndBlockSpeech(VO_Story_11_Captain_006hb_Female_Human_Story_Faelin_Mission6_Lorebook_1_01, Notification.SpeechBubbleDirection.None, GetEnemyActorByCardId("Story_11_Mission1_Lorebook1"), 2.5f));
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
				yield return GameEntity.Coroutines.StartCoroutine(PlaySoundAndBlockSpeech(VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission6_Lorebook_2_01, Notification.SpeechBubbleDirection.None, GetEnemyActorByCardId("Story_11_Mission1_Lorebook1"), 2.5f));
				NotificationManager.Get().DestroyNotification(m_popup, 0f);
				m_popup = null;
				GameState.Get().SetBusy(busy: false);
				yield return MissionPlayVO(friendlyActor, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission6_Lorebook_2_01);
				yield return MissionPlayVO(IniBrassRing, VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission6_Lorebook_2_02);
				yield return MissionPlayVO(friendlyActor, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission6_Lorebook_2_02);
			}
			break;
		case 231:
			if (!(m_popup != null))
			{
				GameState.Get().SetBusy(busy: true);
				m_popup = NotificationManager.Get().CreatePopupText(UserAttentionBlocker.NONE, popUpPos, TutorialEntity.GetTextScale() * popUpScale, GameStrings.Get(m_popUpInfo[missionEvent][0]), convertLegacyPosition: false, NotificationManager.PopupTextType.FANCY);
				yield return GameEntity.Coroutines.StartCoroutine(PlaySoundAndBlockSpeech(VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission6_Lorebook_3_01, Notification.SpeechBubbleDirection.None, GetEnemyActorByCardId("Story_11_Mission1_Lorebook1"), 2.5f));
				NotificationManager.Get().DestroyNotification(m_popup, 0f);
				m_popup = null;
				GameState.Get().SetBusy(busy: false);
			}
			break;
		case 232:
		{
			Notification popup = NotificationManager.Get().CreatePopupText(UserAttentionBlocker.NONE, popUpPos, TutorialEntity.GetTextScale() * popUpScale, GameStrings.Get(m_popUpInfo[missionEvent][0]), convertLegacyPosition: false, NotificationManager.PopupTextType.FANCY);
			yield return new WaitForSeconds(3.5f);
			NotificationManager.Get().DestroyNotification(popup, 0f);
			break;
		}
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
			yield return MissionPlayVO(enemyActor, VO_Story_11_Captain_006hb_Female_Human_Story_Faelin_Mission6ExchangeA_01);
			yield return MissionPlayVO(enemyActor, VO_Story_11_Captain_006hb_Female_Human_Story_Faelin_Mission6ExchangeA_02);
			yield return MissionPlayVO(friendlyActor, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission6ExchangeA_01);
			break;
		case 11:
			yield return MissionPlayVO(enemyActor, VO_Story_11_Captain_006hb_Female_Human_Story_Faelin_Mission6ExchangeD_01);
			yield return MissionPlayVO(friendlyActor, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission6ExchangeD_01);
			yield return MissionPlayVO(enemyActor, VO_Story_11_Captain_006hb_Female_Human_Story_Faelin_Mission6ExchangeD_02);
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
			if (clickedEntity.GetCardId() == "Story_11_Mission6_Lorebook1")
			{
				if (m_popup == null)
				{
					Network.Get().SendClientScriptGameEvent(ClientScriptGameEventType.MISSION_EVENT, 229);
					HandleMissionEvent(229);
				}
				return false;
			}
			if (clickedEntity.GetCardId() == "Story_11_Mission6_Lorebook2")
			{
				if (m_popup == null)
				{
					Network.Get().SendClientScriptGameEvent(ClientScriptGameEventType.MISSION_EVENT, 230);
					HandleMissionEvent(230);
				}
				return false;
			}
			if (clickedEntity.GetCardId() == "Story_11_Mission6_Lorebook3")
			{
				if (m_popup == null)
				{
					Network.Get().SendClientScriptGameEvent(ClientScriptGameEventType.MISSION_EVENT, 231);
					HandleMissionEvent(231);
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
		string counterName = GameStrings.FormatPlurals("BOH_FAELIN_06b", pluralNumbers);
		m_turnCounter.ChangeDialogText(counterName, cost.ToString(), "", "");
	}
}
