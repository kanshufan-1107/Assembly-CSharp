using System.Collections;
using System.Collections.Generic;
using Blizzard.T5.Core;
using UnityEngine;

public class BoH_Faelin_02 : BoH_Faelin_Dungeon
{
	private static Map<GameEntityOption, bool> s_booleanOptions = InitBooleanOptions();

	private static readonly AssetReference VO_Story_11_Caye_012hp_Female_Nightborne_Story_Faelin_Mission2_Lorebook_1_01 = new AssetReference("VO_Story_11_Caye_012hp_Female_Nightborne_Story_Faelin_Mission2_Lorebook_1_01.prefab:0e65a65005c74174b92f407da976b12e");

	private static readonly AssetReference VO_Story_11_Caye_012hp_Female_Nightborne_Story_Faelin_Mission2ExchangeC_01 = new AssetReference("VO_Story_11_Caye_012hp_Female_Nightborne_Story_Faelin_Mission2ExchangeC_01.prefab:e56a16cc54232ed409aaa3961a3efe9a");

	private static readonly AssetReference VO_Story_11_Caye_012hp_Female_Nightborne_Story_Faelin_PreMission2_01 = new AssetReference("VO_Story_11_Caye_012hp_Female_Nightborne_Story_Faelin_PreMission2_01.prefab:f3022e32ecbb4de419a1912bec81a611");

	private static readonly AssetReference VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission2_Lorebook_3_01 = new AssetReference("VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission2_Lorebook_3_01.prefab:46f195b64dace414c93f75b9e6616c3e");

	private static readonly AssetReference VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission2ExchangeA_01 = new AssetReference("VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission2ExchangeA_01.prefab:2396e6bf85f657b498145aeec6235154");

	private static readonly AssetReference VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission2ExchangeC_01 = new AssetReference("VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission2ExchangeC_01.prefab:36829772e8a9c1b4499e72a9c3c536f1");

	private static readonly AssetReference VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission2ExchangeG_01 = new AssetReference("VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission2ExchangeG_01.prefab:da121d6a617befa48a20c8b455db5466");

	private static readonly AssetReference VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission2ExchangeIa_01 = new AssetReference("VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission2ExchangeIa_01.prefab:921e3c82c8f5b8e4d8a7e3e5181f75c1");

	private static readonly AssetReference VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission2ExchangeIb_01 = new AssetReference("VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission2ExchangeIb_01.prefab:1162d6e9d4827af488b904f6b56a4e55");

	private static readonly AssetReference VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission2ExchangeIc_01 = new AssetReference("VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission2ExchangeIc_01.prefab:b505824cd043fe0439122efb213fcf4b");

	private static readonly AssetReference VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission2Start_01 = new AssetReference("VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission2Start_01.prefab:b5d71f61abbd73d418ebc1e679e04d9d");

	private static readonly AssetReference VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission2Victory_01 = new AssetReference("VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission2Victory_01.prefab:92ecdf63988c8bf47ac460c550166109");

	private static readonly AssetReference VO_Story_11_Finley_009hp_Male_Murloc_Story_Faelin_Mission2ExchangeA_01 = new AssetReference("VO_Story_11_Finley_009hp_Male_Murloc_Story_Faelin_Mission2ExchangeA_01.prefab:95d878558a935134d91168b0807a732f");

	private static readonly AssetReference VO_Story_11_Finley_009hp_Male_Murloc_Story_Faelin_Mission2ExchangeI_01 = new AssetReference("VO_Story_11_Finley_009hp_Male_Murloc_Story_Faelin_Mission2ExchangeI_01.prefab:5dd35490bec94ce46bb2d2b97c98fd86");

	private static readonly AssetReference VO_Story_11_Finley_009hp_Male_Murloc_Story_Faelin_Mission2ExchangeI_02 = new AssetReference("VO_Story_11_Finley_009hp_Male_Murloc_Story_Faelin_Mission2ExchangeI_02.prefab:c6b3fbfbadcdb2e44a87b996aa7c2c68");

	private static readonly AssetReference VO_Story_11_Finley_009hp_Male_Murloc_Story_Faelin_Mission2Victory_01 = new AssetReference("VO_Story_11_Finley_009hp_Male_Murloc_Story_Faelin_Mission2Victory_01.prefab:418795c65c85e5a469e98a7dac910d12");

	private static readonly AssetReference VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission2ExchangeA_01 = new AssetReference("VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission2ExchangeA_01.prefab:2c50f13929d4f77438911a02d2679520");

	private static readonly AssetReference VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission2ExchangeI_01 = new AssetReference("VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission2ExchangeI_01.prefab:a96f643b9936b894a9c879eaff891a59");

	private static readonly AssetReference VO_Story_11_Halus_010hp_Male_BloodElf_Story_Faelin_Mission2ExchangeA_01 = new AssetReference("VO_Story_11_Halus_010hp_Male_BloodElf_Story_Faelin_Mission2ExchangeA_01.prefab:d0462f39ce9fa1344afc275f537f78c0");

	private static readonly AssetReference VO_Story_11_Halus_010hp_Male_BloodElf_Story_Faelin_Mission2ExchangeI_01 = new AssetReference("VO_Story_11_Halus_010hp_Male_BloodElf_Story_Faelin_Mission2ExchangeI_01.prefab:a60a4614fe87e6d459cfafb242e9a88b");

	private static readonly AssetReference VO_Story_11_Halus_010hp_Male_BloodElf_Story_Faelin_Mission2ExchangeI_02 = new AssetReference("VO_Story_11_Halus_010hp_Male_BloodElf_Story_Faelin_Mission2ExchangeI_02.prefab:8582df12a0f0dde46aa97c0549d86754");

	private static readonly AssetReference VO_Story_11_Handmaiden_015hb_Female_NightElf_Story_Faelin_Mission2_Lorebook_1_01 = new AssetReference("VO_Story_11_Handmaiden_015hb_Female_NightElf_Story_Faelin_Mission2_Lorebook_1_01.prefab:2b222a38a5814a48a4778c2b2639e108");

	private static readonly AssetReference VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission2ExchangeA_01 = new AssetReference("VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission2ExchangeA_01.prefab:b475153661331fa45bc5d5e896a67a2c");

	private static readonly AssetReference VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission2ExchangeG_01 = new AssetReference("VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission2ExchangeG_01.prefab:b6a4de3f67981de469a2fd8413b104dd");

	private static readonly AssetReference VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission2Victory_01 = new AssetReference("VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission2Victory_01.prefab:0c730c2ba3e6a5b42b81d9b0904b07c3");

	private static readonly AssetReference VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_PreMission2_01 = new AssetReference("VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_PreMission2_01.prefab:84ff826dc3a7e4645b05357195a52167");

	private static readonly AssetReference VO_Story_11_Looter_002hb_Male_Undead_Story_Faelin_Mission2EmoteResponse_01 = new AssetReference("VO_Story_11_Looter_002hb_Male_Undead_Story_Faelin_Mission2EmoteResponse_01.prefab:98ace26998cb0c648b515b9c16dc9221");

	private static readonly AssetReference VO_Story_11_Looter_002hb_Male_Undead_Story_Faelin_Mission2ExchangeC_01 = new AssetReference("VO_Story_11_Looter_002hb_Male_Undead_Story_Faelin_Mission2ExchangeC_01.prefab:9b3f188bd0f2578489e4ccc28ae3d6d5");

	private static readonly AssetReference VO_Story_11_Looter_002hb_Male_Undead_Story_Faelin_Mission2HeroPower_01 = new AssetReference("VO_Story_11_Looter_002hb_Male_Undead_Story_Faelin_Mission2HeroPower_01.prefab:e291f133928e3d142b6ff8790997df8c");

	private static readonly AssetReference VO_Story_11_Looter_002hb_Male_Undead_Story_Faelin_Mission2HeroPower_02 = new AssetReference("VO_Story_11_Looter_002hb_Male_Undead_Story_Faelin_Mission2HeroPower_02.prefab:c822555474d79af439e56afdc1861d77");

	private static readonly AssetReference VO_Story_11_Looter_002hb_Male_Undead_Story_Faelin_Mission2HeroPower_03 = new AssetReference("VO_Story_11_Looter_002hb_Male_Undead_Story_Faelin_Mission2HeroPower_03.prefab:8c3419e5a04534347bdd16b7f9e98687");

	private static readonly AssetReference VO_Story_11_Looter_002hb_Male_Undead_Story_Faelin_Mission2Idle_01 = new AssetReference("VO_Story_11_Looter_002hb_Male_Undead_Story_Faelin_Mission2Idle_01.prefab:542edbf1a2fa4424895f1ed942eed303");

	private static readonly AssetReference VO_Story_11_Looter_002hb_Male_Undead_Story_Faelin_Mission2Idle_02 = new AssetReference("VO_Story_11_Looter_002hb_Male_Undead_Story_Faelin_Mission2Idle_02.prefab:f10c43715e943254fa632a39b771d0cb");

	private static readonly AssetReference VO_Story_11_Looter_002hb_Male_Undead_Story_Faelin_Mission2Idle_03 = new AssetReference("VO_Story_11_Looter_002hb_Male_Undead_Story_Faelin_Mission2Idle_03.prefab:de352a28270abe5499179966fdd35796");

	private static readonly AssetReference VO_Story_11_Looter_002hb_Male_Undead_Story_Faelin_Mission2Loss_01 = new AssetReference("VO_Story_11_Looter_002hb_Male_Undead_Story_Faelin_Mission2Loss_01.prefab:daeaf94eb55bed544a66326dc1ddfa97");

	private static readonly AssetReference VO_Story_11_Looter_002hb_Male_Undead_Story_Faelin_Mission2Start_01 = new AssetReference("VO_Story_11_Looter_002hb_Male_Undead_Story_Faelin_Mission2Start_01.prefab:54a98375f783c5a4da8f668d78570f6e");

	private static readonly AssetReference VO_Story_11_Looter_002hb_Male_Undead_Story_Faelin_Mission2Start_02 = new AssetReference("VO_Story_11_Looter_002hb_Male_Undead_Story_Faelin_Mission2Start_02.prefab:6459e4db9c160f848b07d08db3ba5aaf");

	private static readonly AssetReference VO_Story_11_Looter_002hb_Male_Undead_Story_Faelin_Mission2Victory_01 = new AssetReference("VO_Story_11_Looter_002hb_Male_Undead_Story_Faelin_Mission2Victory_01.prefab:02056a878e656a148b909b50b07ef9f0");

	private static readonly AssetReference VO_Story_11_Looter_002hb_Male_Undead_Story_Faelin_Mission2Victory_02 = new AssetReference("VO_Story_11_Looter_002hb_Male_Undead_Story_Faelin_Mission2Victory_02.prefab:dc612faa2a0d7094b96526185e998644");

	private static readonly AssetReference VO_Story_11_FirstMatePip_Male_Vulpera_Story_Faelin_Mission2_Lorebook_2_01 = new AssetReference("VO_Story_11_FirstMatePip_Male_Vulpera_Story_Faelin_Mission2_Lorebook_2_01.prefab:28609bd169dfedb438ff680dc3160662");

	private Notification m_popup;

	private Dictionary<int, string[]> m_popUpInfo = new Dictionary<int, string[]>
	{
		{
			228,
			new string[1] { "BOH_FAELIN_MISSION2_LOREBOOK_02" }
		},
		{
			229,
			new string[1] { "BOH_FAELIN_MISSION2_LOREBOOK_01" }
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

	private new List<string> m_BossIdleLines = new List<string> { VO_Story_11_Looter_002hb_Male_Undead_Story_Faelin_Mission2Idle_01, VO_Story_11_Looter_002hb_Male_Undead_Story_Faelin_Mission2Idle_02, VO_Story_11_Looter_002hb_Male_Undead_Story_Faelin_Mission2Idle_03 };

	private List<string> m_BossUsesHeroPowerLines = new List<string> { VO_Story_11_Looter_002hb_Male_Undead_Story_Faelin_Mission2HeroPower_01, VO_Story_11_Looter_002hb_Male_Undead_Story_Faelin_Mission2HeroPower_02, VO_Story_11_Looter_002hb_Male_Undead_Story_Faelin_Mission2HeroPower_03 };

	private HashSet<string> m_playedLines = new HashSet<string>();

	private static Map<GameEntityOption, bool> InitBooleanOptions()
	{
		return new Map<GameEntityOption, bool> { 
		{
			GameEntityOption.DO_OPENING_TAUNTS,
			false
		} };
	}

	public BoH_Faelin_02()
	{
		m_gameOptions.AddBooleanOptions(s_booleanOptions);
	}

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> m_SoundFiles = new List<string>
		{
			VO_Story_11_Looter_002hb_Male_Undead_Story_Faelin_Mission2Start_01, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission2Start_01, VO_Story_11_Looter_002hb_Male_Undead_Story_Faelin_Mission2Start_02, VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission2ExchangeA_01, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission2ExchangeA_01, VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission2ExchangeA_01, VO_Story_11_Finley_009hp_Male_Murloc_Story_Faelin_Mission2ExchangeA_01, VO_Story_11_Halus_010hp_Male_BloodElf_Story_Faelin_Mission2ExchangeA_01, VO_Story_11_Looter_002hb_Male_Undead_Story_Faelin_Mission2ExchangeC_01, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission2ExchangeC_01,
			VO_Story_11_Caye_012hp_Female_Nightborne_Story_Faelin_Mission2ExchangeC_01, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission2ExchangeG_01, VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission2ExchangeG_01, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission2ExchangeIa_01, VO_Story_11_Finley_009hp_Male_Murloc_Story_Faelin_Mission2ExchangeI_01, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission2ExchangeIb_01, VO_Story_11_Finley_009hp_Male_Murloc_Story_Faelin_Mission2ExchangeI_02, VO_Story_11_Halus_010hp_Male_BloodElf_Story_Faelin_Mission2ExchangeI_01, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission2ExchangeIc_01, VO_Story_11_Halus_010hp_Male_BloodElf_Story_Faelin_Mission2ExchangeI_02,
			VO_Story_11_Looter_002hb_Male_Undead_Story_Faelin_Mission2Victory_01, VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission2Victory_01, VO_Story_11_Looter_002hb_Male_Undead_Story_Faelin_Mission2Victory_02, VO_Story_11_Finley_009hp_Male_Murloc_Story_Faelin_Mission2Victory_01, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission2Victory_01, VO_Story_11_Looter_002hb_Male_Undead_Story_Faelin_Mission2EmoteResponse_01, VO_Story_11_Looter_002hb_Male_Undead_Story_Faelin_Mission2HeroPower_01, VO_Story_11_Looter_002hb_Male_Undead_Story_Faelin_Mission2HeroPower_02, VO_Story_11_Looter_002hb_Male_Undead_Story_Faelin_Mission2HeroPower_03, VO_Story_11_Looter_002hb_Male_Undead_Story_Faelin_Mission2Loss_01,
			VO_Story_11_Looter_002hb_Male_Undead_Story_Faelin_Mission2Idle_01, VO_Story_11_Looter_002hb_Male_Undead_Story_Faelin_Mission2Idle_02, VO_Story_11_Looter_002hb_Male_Undead_Story_Faelin_Mission2Idle_03, VO_Story_11_Handmaiden_015hb_Female_NightElf_Story_Faelin_Mission2_Lorebook_1_01, VO_Story_11_Caye_012hp_Female_Nightborne_Story_Faelin_Mission2_Lorebook_1_01, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission2_Lorebook_3_01, VO_Story_11_FirstMatePip_Male_Vulpera_Story_Faelin_Mission2_Lorebook_2_01
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
		m_OverrideMusicTrack = MusicPlaylistType.InGame_LOOTFinalBoss;
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
			yield return MissionPlayVO(enemyActor, VO_Story_11_Looter_002hb_Male_Undead_Story_Faelin_Mission2EmoteResponse_01);
			break;
		case 514:
			yield return MissionPlayVO(enemyActor, VO_Story_11_Looter_002hb_Male_Undead_Story_Faelin_Mission2Start_01);
			yield return MissionPlayVO(friendlyActor, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission2Start_01);
			yield return MissionPlayVO(enemyActor, VO_Story_11_Looter_002hb_Male_Undead_Story_Faelin_Mission2Start_02);
			break;
		case 507:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(enemyActor, VO_Story_11_Looter_002hb_Male_Undead_Story_Faelin_Mission2Loss_01);
			GameState.Get().SetBusy(busy: false);
			break;
		case 504:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(enemyActor, VO_Story_11_Looter_002hb_Male_Undead_Story_Faelin_Mission2Victory_01);
			yield return MissionPlayVO(IniBrassRing, VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission2Victory_01);
			yield return MissionPlayVO(enemyActor, VO_Story_11_Looter_002hb_Male_Undead_Story_Faelin_Mission2Victory_02);
			yield return MissionPlayVO(friendlyActor, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission2Victory_01);
			GameState.Get().SetBusy(busy: false);
			break;
		case 101:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(enemyActor, VO_Story_11_Looter_002hb_Male_Undead_Story_Faelin_Mission2Victory_01);
			yield return MissionPlayVO(IniBrassRing, VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission2Victory_01);
			yield return MissionPlayVO(enemyActor, VO_Story_11_Looter_002hb_Male_Undead_Story_Faelin_Mission2Victory_02);
			yield return MissionPlayVO(FinleyBrassRing, VO_Story_11_Finley_009hp_Male_Murloc_Story_Faelin_Mission2Victory_01);
			GameState.Get().SetBusy(busy: false);
			break;
		case 102:
			yield return MissionPlayVO(IniBrassRing, VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission2ExchangeA_01);
			yield return MissionPlayVO(friendlyActor, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission2ExchangeA_01);
			yield return MissionPlayVO(HalusBrassRing, VO_Story_11_Halus_010hp_Male_BloodElf_Story_Faelin_Mission2ExchangeA_01);
			break;
		case 103:
			yield return MissionPlayVO(IniBrassRing, VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission2ExchangeA_01);
			yield return MissionPlayVO(friendlyActor, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission2ExchangeA_01);
			yield return MissionPlayVO(GraceBrassRing, VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission2ExchangeA_01);
			break;
		case 104:
			yield return MissionPlayVO(IniBrassRing, VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission2ExchangeA_01);
			yield return MissionPlayVO(friendlyActor, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission2ExchangeA_01);
			yield return MissionPlayVO(FinleyBrassRing, VO_Story_11_Finley_009hp_Male_Murloc_Story_Faelin_Mission2ExchangeA_01);
			break;
		case 106:
			yield return MissionPlayVO(FinleyBrassRing, VO_Story_11_Finley_009hp_Male_Murloc_Story_Faelin_Mission2ExchangeI_01);
			yield return MissionPlayVO(friendlyActor, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission2ExchangeIb_01);
			yield return MissionPlayVO(FinleyBrassRing, VO_Story_11_Finley_009hp_Male_Murloc_Story_Faelin_Mission2ExchangeI_02);
			break;
		case 107:
			yield return MissionPlayVO(HalusBrassRing, VO_Story_11_Halus_010hp_Male_BloodElf_Story_Faelin_Mission2ExchangeI_01);
			yield return MissionPlayVO(friendlyActor, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission2ExchangeIc_01);
			yield return MissionPlayVO(HalusBrassRing, VO_Story_11_Halus_010hp_Male_BloodElf_Story_Faelin_Mission2ExchangeI_02);
			break;
		case 229:
			if (!(m_popup != null))
			{
				GameState.Get().SetBusy(busy: true);
				m_popup = NotificationManager.Get().CreatePopupText(UserAttentionBlocker.NONE, popUpPos, TutorialEntity.GetTextScale() * popUpScale, GameStrings.Get(m_popUpInfo[missionEvent][0]), convertLegacyPosition: false, NotificationManager.PopupTextType.FANCY);
				yield return GameEntity.Coroutines.StartCoroutine(PlaySoundAndBlockSpeech(VO_Story_11_Handmaiden_015hb_Female_NightElf_Story_Faelin_Mission2_Lorebook_1_01, Notification.SpeechBubbleDirection.None, GetEnemyActorByCardId("Story_11_Mission1_Lorebook1"), 2.5f));
				NotificationManager.Get().DestroyNotification(m_popup, 0f);
				m_popup = null;
				GameState.Get().SetBusy(busy: false);
				yield return MissionPlayVO(CayeBrassRing, VO_Story_11_Caye_012hp_Female_Nightborne_Story_Faelin_Mission2_Lorebook_1_01);
				yield return MissionPlayVO(friendlyActor, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission2_Lorebook_3_01);
			}
			break;
		case 228:
			if (!(m_popup != null))
			{
				GameState.Get().SetBusy(busy: true);
				m_popup = NotificationManager.Get().CreatePopupText(UserAttentionBlocker.NONE, popUpPos, TutorialEntity.GetTextScale() * popUpScale, GameStrings.Get(m_popUpInfo[missionEvent][0]), convertLegacyPosition: false, NotificationManager.PopupTextType.FANCY);
				yield return GameEntity.Coroutines.StartCoroutine(PlaySoundAndBlockSpeech(VO_Story_11_FirstMatePip_Male_Vulpera_Story_Faelin_Mission2_Lorebook_2_01, Notification.SpeechBubbleDirection.None, GetEnemyActorByCardId("Story_11_Mission1_Lorebook1"), 2.5f));
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
		case 3:
			yield return MissionPlayVO(enemyActor, VO_Story_11_Looter_002hb_Male_Undead_Story_Faelin_Mission2ExchangeC_01);
			yield return MissionPlayVO(friendlyActor, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission2ExchangeC_01);
			yield return MissionPlayVO(CayeBrassRing, VO_Story_11_Caye_012hp_Female_Nightborne_Story_Faelin_Mission2ExchangeC_01);
			break;
		case 7:
			yield return MissionPlayVO(friendlyActor, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission2ExchangeG_01);
			yield return MissionPlayVO(IniBrassRing, VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission2ExchangeG_01);
			yield return MissionPlayVO(friendlyActor, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission2ExchangeIa_01);
			break;
		case 15:
			yield return MissionPlayVO(enemyActor, VO_Story_11_Looter_002hb_Male_Undead_Story_Faelin_Mission2HeroPower_01);
			break;
		case 17:
			yield return MissionPlayVO(enemyActor, VO_Story_11_Looter_002hb_Male_Undead_Story_Faelin_Mission2HeroPower_02);
			break;
		case 19:
			yield return MissionPlayVO(enemyActor, VO_Story_11_Looter_002hb_Male_Undead_Story_Faelin_Mission2HeroPower_03);
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
			if (clickedEntity.GetCardId() == "Story_11_Mission2_Lorebook1")
			{
				if (m_popup == null)
				{
					Network.Get().SendClientScriptGameEvent(ClientScriptGameEventType.MISSION_EVENT, 229);
					HandleMissionEvent(229);
				}
				return false;
			}
			if (clickedEntity.GetCardId() == "Story_11_Mission2_Lorebook2")
			{
				if (m_popup == null)
				{
					Network.Get().SendClientScriptGameEvent(ClientScriptGameEventType.MISSION_EVENT, 228);
					HandleMissionEvent(228);
				}
				return false;
			}
		}
		return true;
	}
}
