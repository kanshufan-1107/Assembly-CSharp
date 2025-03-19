using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BOM_07_Scabbs_Fight_004 : BOM_07_Scabbs_Dungeon
{
	private static readonly AssetReference VO_Story_Hero_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission4BossActive_01 = new AssetReference("VO_Story_Hero_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission4BossActive_01.prefab:012d57ef1d7e4ab2b6b3f3e90bfbcede");

	private static readonly AssetReference VO_Story_Hero_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission4BossActive_02 = new AssetReference("VO_Story_Hero_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission4BossActive_02.prefab:934c1ca0f1fb48eeb8a532c1a23fe5f7");

	private static readonly AssetReference VO_Story_Hero_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission4BossActive_03 = new AssetReference("VO_Story_Hero_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission4BossActive_03.prefab:5032c805c002413793543160efda6e81");

	private static readonly AssetReference VO_Story_Hero_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission4BossInactive_04 = new AssetReference("VO_Story_Hero_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission4BossInactive_04.prefab:33b2ad359c3c4c79adf18c65d5417a71");

	private static readonly AssetReference VO_Story_Hero_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission4BossInactive_05 = new AssetReference("VO_Story_Hero_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission4BossInactive_05.prefab:0c4d399e23704398a0a7bb41fd22f082");

	private static readonly AssetReference VO_Story_Hero_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission4CorrectCombo_01 = new AssetReference("VO_Story_Hero_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission4CorrectCombo_01.prefab:57b20d6d1d8b44179a566084e32ca6eb");

	private static readonly AssetReference VO_Story_Hero_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission4CorrectCombo_02 = new AssetReference("VO_Story_Hero_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission4CorrectCombo_02.prefab:7944d555f7aa41ba94147f29ef0bc781");

	private static readonly AssetReference VO_Story_Hero_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission4CorrectCombo_03 = new AssetReference("VO_Story_Hero_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission4CorrectCombo_03.prefab:742e339fb75c45a297de5ad844d26f2e");

	private static readonly AssetReference VO_Story_Hero_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission4CorrectCombo_04 = new AssetReference("VO_Story_Hero_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission4CorrectCombo_04.prefab:2ab1afd0aa0946e9bc938ca35b517320");

	private static readonly AssetReference VO_Story_Hero_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission4CorrectCombo_05 = new AssetReference("VO_Story_Hero_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission4CorrectCombo_05.prefab:9d1e5705461b44fbab445ad202ce71ca");

	private static readonly AssetReference VO_Story_Hero_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission4ExchangeA_01 = new AssetReference("VO_Story_Hero_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission4ExchangeA_01.prefab:e676a61201714d7ea94433326b712791");

	private static readonly AssetReference VO_Story_Hero_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission4ExchangeA_02 = new AssetReference("VO_Story_Hero_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission4ExchangeA_02.prefab:949804eacfaa41a1be01694630829f2e");

	private static readonly AssetReference VO_Story_Hero_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission4ExchangeAAlt_02 = new AssetReference("VO_Story_Hero_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission4ExchangeAAlt_02.prefab:12c2a8aca62f44acaefc5553c7ce6033");

	private static readonly AssetReference VO_Story_Hero_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission4ExchangeB_01 = new AssetReference("VO_Story_Hero_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission4ExchangeB_01.prefab:49c5c9fde32847439cf5886925c48b09");

	private static readonly AssetReference VO_Story_Hero_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission4ExchangeC_01 = new AssetReference("VO_Story_Hero_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission4ExchangeC_01.prefab:d7b616311826404991a5c48d3c4344d1");

	private static readonly AssetReference VO_Story_Hero_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission4ExchangeD_01 = new AssetReference("VO_Story_Hero_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission4ExchangeD_01.prefab:f4690a09131b432a87064a825f8381d1");

	private static readonly AssetReference VO_Story_Hero_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission4ExchangeD_03 = new AssetReference("VO_Story_Hero_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission4ExchangeD_03.prefab:530f60eb55074a62aa751309639bdb63");

	private static readonly AssetReference VO_Story_Hero_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission4ExchangeE_01 = new AssetReference("VO_Story_Hero_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission4ExchangeE_01.prefab:4314cfa7ca7a49fd9eab08fa5ce4b5ee");

	private static readonly AssetReference VO_Story_Hero_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission4ExchangeE_03 = new AssetReference("VO_Story_Hero_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission4ExchangeE_03.prefab:5fd4c05412d548cebc20f6c039c576f0");

	private static readonly AssetReference VO_Story_Hero_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission4ExchangeF_02 = new AssetReference("VO_Story_Hero_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission4ExchangeF_02.prefab:60e6b398b5804a33a400ca38bfe514cb");

	private static readonly AssetReference VO_Story_Hero_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission4Guards_01 = new AssetReference("VO_Story_Hero_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission4Guards_01.prefab:b1ed354c77054c088fdcf3ed63df045e");

	private static readonly AssetReference VO_Story_Hero_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission4Guards_02 = new AssetReference("VO_Story_Hero_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission4Guards_02.prefab:2bbded7f75f848139b569f18a4ecd8b0");

	private static readonly AssetReference VO_Story_Hero_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission4Idle_01 = new AssetReference("VO_Story_Hero_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission4Idle_01.prefab:6919e94a28fb4a1d8154079d4df704a8");

	private static readonly AssetReference VO_Story_Hero_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission4Idle_02 = new AssetReference("VO_Story_Hero_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission4Idle_02.prefab:5e60cf412d9248caa3cf0b33c6ba927f");

	private static readonly AssetReference VO_Story_Hero_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission4Idle_03 = new AssetReference("VO_Story_Hero_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission4Idle_03.prefab:0fbb9d55b64645599ab69266bcceeca6");

	private static readonly AssetReference VO_Story_Hero_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission4Idle_04 = new AssetReference("VO_Story_Hero_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission4Idle_04.prefab:c13a40f6386b4c748787a4d98068c9a4");

	private static readonly AssetReference VO_Story_Hero_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission4Victory_01 = new AssetReference("VO_Story_Hero_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission4Victory_01.prefab:23f7c42a6fea4cfd8c9caaa5ef6f09a7");

	private static readonly AssetReference VO_Story_Hero_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission4WrongCombo_01 = new AssetReference("VO_Story_Hero_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission4WrongCombo_01.prefab:86e9b16d33694df2b429cbd63f540a00");

	private static readonly AssetReference VO_Story_Hero_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission4WrongCombo_02 = new AssetReference("VO_Story_Hero_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission4WrongCombo_02.prefab:c381192a568145ca8801da70065b309a");

	private static readonly AssetReference VO_Story_Hero_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission4WrongCombo_03 = new AssetReference("VO_Story_Hero_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission4WrongCombo_03.prefab:943b7940ece748e7afe0b0180a3d065d");

	private static readonly AssetReference VO_Story_Hero_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission4WrongCombo_04 = new AssetReference("VO_Story_Hero_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission4WrongCombo_04.prefab:2f923040ebf5401eafc5d6d75b2d1125");

	private static readonly AssetReference VO_Story_Hero_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission4WrongCombo_05 = new AssetReference("VO_Story_Hero_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission4WrongCombo_05.prefab:12c0de4e6d7e464aa52e2f4bc8b6f769");

	private static readonly AssetReference VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission4ExchangeD_02 = new AssetReference("VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission4ExchangeD_02.prefab:aca8e25b410c4ae69fba6a846fa900b7");

	private static readonly AssetReference VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission4ExchangeE_02 = new AssetReference("VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission4ExchangeE_02.prefab:55443b9c7962483a89f0d4da191e1e50");

	private static readonly AssetReference VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission4ExchangeF_01 = new AssetReference("VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission4ExchangeF_01.prefab:922db907cbac4bfcacb1641aa14841bf");

	private static readonly AssetReference VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission4ExchangeF_03 = new AssetReference("VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission4ExchangeF_03.prefab:18fbc0c785654f40a190c09bbbd92bab");

	private static readonly AssetReference VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission4Idle_01 = new AssetReference("VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission4Idle_01.prefab:425692a884b8496cba36393b56ccd4b1");

	private static readonly AssetReference VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission4Idle_02 = new AssetReference("VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission4Idle_02.prefab:f9b161e571884e1ebbc06f685fc423f3");

	private static readonly AssetReference VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission4Idle_03 = new AssetReference("VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission4Idle_03.prefab:4c50dcafd9bd4b68ba0f33c6f1186470");

	private static readonly AssetReference VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission4Idle_04 = new AssetReference("VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission4Idle_04.prefab:0ab7dc5bf65d4fa58cb60477d361abaf");

	private static readonly AssetReference VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission4Intro_01 = new AssetReference("VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission4Intro_01.prefab:d4cc06e275ca4281aec3c65eaec0f97e");

	private static readonly AssetReference VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission4IntroAlt_01 = new AssetReference("VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission4IntroAlt_01.prefab:681d437a52de4d5eae464c9557eb71ca");

	private static readonly AssetReference VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission4Victory_02 = new AssetReference("VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission4Victory_02.prefab:001b78147716484392791e0115f8b594");

	private static readonly AssetReference VO_Story_Hero_Thelwater_Male_Human_BOM_Scabbs_Mission4BossInactive_01 = new AssetReference("VO_Story_Hero_Thelwater_Male_Human_BOM_Scabbs_Mission4BossInactive_01.prefab:7491bf00c64d4299b5ec516c83beb14c");

	private static readonly AssetReference VO_Story_Hero_Thelwater_Male_Human_BOM_Scabbs_Mission4BossInactive_02 = new AssetReference("VO_Story_Hero_Thelwater_Male_Human_BOM_Scabbs_Mission4BossInactive_02.prefab:7897920055634d72aa63389902e10d93");

	private static readonly AssetReference VO_Story_Hero_Thelwater_Male_Human_BOM_Scabbs_Mission4BossInactive_03 = new AssetReference("VO_Story_Hero_Thelwater_Male_Human_BOM_Scabbs_Mission4BossInactive_03.prefab:1c9e666dbdea417794baa82ecb4d8c33");

	private static readonly AssetReference VO_Story_Hero_Thelwater_Male_Human_BOM_Scabbs_Mission4Death_01 = new AssetReference("VO_Story_Hero_Thelwater_Male_Human_BOM_Scabbs_Mission4Death_01.prefab:5513ddf2b7884d6594c3657e75e6f5d0");

	private static readonly AssetReference VO_Story_Hero_Thelwater_Male_Human_BOM_Scabbs_Mission4EmoteResponse_01 = new AssetReference("VO_Story_Hero_Thelwater_Male_Human_BOM_Scabbs_Mission4EmoteResponse_01.prefab:b942c78b82c946f09e831d4d3a600de4");

	private static readonly AssetReference VO_Story_Hero_Thelwater_Male_Human_BOM_Scabbs_Mission4ExchangeAAlt_01 = new AssetReference("VO_Story_Hero_Thelwater_Male_Human_BOM_Scabbs_Mission4ExchangeAAlt_01.prefab:80359930986649a5b9051e6f32b49def");

	private static readonly AssetReference VO_Story_Hero_Thelwater_Male_Human_BOM_Scabbs_Mission4HeroPower_01 = new AssetReference("VO_Story_Hero_Thelwater_Male_Human_BOM_Scabbs_Mission4HeroPower_01.prefab:bba679d261e1453396f90b644e5b8f2f");

	private static readonly AssetReference VO_Story_Hero_Thelwater_Male_Human_BOM_Scabbs_Mission4HeroPower_02 = new AssetReference("VO_Story_Hero_Thelwater_Male_Human_BOM_Scabbs_Mission4HeroPower_02.prefab:93dd4e05bc6b478d906eff9c8a044a0a");

	private static readonly AssetReference VO_Story_Hero_Thelwater_Male_Human_BOM_Scabbs_Mission4HeroPower_03 = new AssetReference("VO_Story_Hero_Thelwater_Male_Human_BOM_Scabbs_Mission4HeroPower_03.prefab:60b85988d23e45a3b1bcdb45c9308fe3");

	private static readonly AssetReference VO_Story_Hero_Thelwater_Male_Human_BOM_Scabbs_Mission4HeroPower_04 = new AssetReference("VO_Story_Hero_Thelwater_Male_Human_BOM_Scabbs_Mission4HeroPower_04.prefab:260194273d20421a8ac19776ab19cc1d");

	private static readonly AssetReference VO_Story_Hero_Thelwater_Male_Human_BOM_Scabbs_Mission4Idle_01 = new AssetReference("VO_Story_Hero_Thelwater_Male_Human_BOM_Scabbs_Mission4Idle_01.prefab:a003cd9105304ba893ce40e9abd99b1d");

	private static readonly AssetReference VO_Story_Hero_Thelwater_Male_Human_BOM_Scabbs_Mission4Idle_02 = new AssetReference("VO_Story_Hero_Thelwater_Male_Human_BOM_Scabbs_Mission4Idle_02.prefab:7586a173aaea426e8d8fa5d7b3d99636");

	private static readonly AssetReference VO_Story_Hero_Thelwater_Male_Human_BOM_Scabbs_Mission4Idle_03 = new AssetReference("VO_Story_Hero_Thelwater_Male_Human_BOM_Scabbs_Mission4Idle_03.prefab:ba3bffad55ed4ca8acdabb43b4c7322f");

	private static readonly AssetReference VO_Story_Hero_Thelwater_Male_Human_BOM_Scabbs_Mission4Intro_02 = new AssetReference("VO_Story_Hero_Thelwater_Male_Human_BOM_Scabbs_Mission4Intro_02.prefab:f52f2ea8a09d4ccf8a859684caee152a");

	private static readonly AssetReference VO_Story_Hero_Thelwater_Male_Human_BOM_Scabbs_Mission4IntroAlt_02 = new AssetReference("VO_Story_Hero_Thelwater_Male_Human_BOM_Scabbs_Mission4IntroAlt_02.prefab:973e72d8ecb4456b88e6f9b80a1b632b");

	private static readonly AssetReference VO_Story_Hero_Thelwater_Male_Human_BOM_Scabbs_Mission4Loss_01 = new AssetReference("VO_Story_Hero_Thelwater_Male_Human_BOM_Scabbs_Mission4Loss_01.prefab:2c2653d1a3f446c8ab5ea38f32a090a0");

	private static readonly AssetReference VO_PVPDR_Hero_Scabbs_Male_Gnome_Death_01 = new AssetReference("VO_PVPDR_Hero_Scabbs_Male_Gnome_Death_01.prefab:85e10a0b219b1974aaa959bd523c554b");

	private List<string> m_VO_Story_Hero_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission4BossActiveLines = new List<string>
	{
		VO_Story_Hero_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission4BossActive_01, VO_Story_Hero_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission4BossActive_02, VO_Story_Hero_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission4BossActive_03, VO_Story_Hero_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission4BossInactive_04, VO_Story_Hero_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission4BossInactive_05, VO_Story_Hero_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission4CorrectCombo_01, VO_Story_Hero_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission4CorrectCombo_02, VO_Story_Hero_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission4CorrectCombo_03, VO_Story_Hero_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission4CorrectCombo_04, VO_Story_Hero_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission4CorrectCombo_05,
		VO_Story_Hero_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission4ExchangeAAlt_02, VO_Story_Hero_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission4Guards_01, VO_Story_Hero_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission4Guards_02, VO_Story_Hero_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission4WrongCombo_01, VO_Story_Hero_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission4WrongCombo_02, VO_Story_Hero_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission4WrongCombo_03, VO_Story_Hero_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission4WrongCombo_04, VO_Story_Hero_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission4WrongCombo_05, VO_Story_Hero_Thelwater_Male_Human_BOM_Scabbs_Mission4BossInactive_01, VO_Story_Hero_Thelwater_Male_Human_BOM_Scabbs_Mission4BossInactive_02,
		VO_Story_Hero_Thelwater_Male_Human_BOM_Scabbs_Mission4BossInactive_03, VO_Story_Hero_Thelwater_Male_Human_BOM_Scabbs_Mission4ExchangeAAlt_01
	};

	private List<string> m_InGame_BossIdleLines = new List<string> { VO_Story_Hero_Thelwater_Male_Human_BOM_Scabbs_Mission4Idle_01, VO_Story_Hero_Thelwater_Male_Human_BOM_Scabbs_Mission4Idle_02, VO_Story_Hero_Thelwater_Male_Human_BOM_Scabbs_Mission4Idle_03 };

	private List<string> m_InGame_PlayerAltIdleLines = new List<string> { VO_Story_Hero_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission4Idle_01, VO_Story_Hero_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission4Idle_02, VO_Story_Hero_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission4Idle_03, VO_Story_Hero_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission4Idle_04 };

	private List<string> m_InGame_PlayerIdleLines = new List<string> { VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission4Idle_01, VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission4Idle_02, VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission4Idle_03, VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission4Idle_04 };

	private List<string> m_InGame_BossUsesHeroPowerLines = new List<string> { VO_Story_Hero_Thelwater_Male_Human_BOM_Scabbs_Mission4HeroPower_01, VO_Story_Hero_Thelwater_Male_Human_BOM_Scabbs_Mission4HeroPower_02, VO_Story_Hero_Thelwater_Male_Human_BOM_Scabbs_Mission4HeroPower_03, VO_Story_Hero_Thelwater_Male_Human_BOM_Scabbs_Mission4HeroPower_04 };

	private HashSet<string> m_playedLines = new HashSet<string>();

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> m_SoundFiles = new List<string>
		{
			VO_Story_Hero_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission4BossActive_01, VO_Story_Hero_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission4BossActive_02, VO_Story_Hero_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission4BossActive_03, VO_Story_Hero_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission4BossInactive_04, VO_Story_Hero_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission4BossInactive_05, VO_Story_Hero_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission4CorrectCombo_01, VO_Story_Hero_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission4CorrectCombo_02, VO_Story_Hero_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission4CorrectCombo_03, VO_Story_Hero_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission4CorrectCombo_04, VO_Story_Hero_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission4CorrectCombo_05,
			VO_Story_Hero_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission4ExchangeA_01, VO_Story_Hero_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission4ExchangeA_02, VO_Story_Hero_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission4ExchangeAAlt_02, VO_Story_Hero_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission4ExchangeB_01, VO_Story_Hero_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission4ExchangeC_01, VO_Story_Hero_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission4ExchangeD_01, VO_Story_Hero_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission4ExchangeD_03, VO_Story_Hero_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission4ExchangeE_01, VO_Story_Hero_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission4ExchangeE_03, VO_Story_Hero_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission4ExchangeF_02,
			VO_Story_Hero_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission4Guards_01, VO_Story_Hero_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission4Guards_02, VO_Story_Hero_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission4Idle_01, VO_Story_Hero_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission4Idle_02, VO_Story_Hero_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission4Idle_03, VO_Story_Hero_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission4Idle_04, VO_Story_Hero_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission4Victory_01, VO_Story_Hero_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission4WrongCombo_01, VO_Story_Hero_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission4WrongCombo_02, VO_Story_Hero_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission4WrongCombo_03,
			VO_Story_Hero_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission4WrongCombo_04, VO_Story_Hero_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission4WrongCombo_05, VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission4ExchangeD_02, VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission4ExchangeE_02, VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission4ExchangeF_01, VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission4ExchangeF_03, VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission4Idle_01, VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission4Idle_02, VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission4Idle_03, VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission4Idle_04,
			VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission4Intro_01, VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission4IntroAlt_01, VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission4Victory_02, VO_Story_Hero_Thelwater_Male_Human_BOM_Scabbs_Mission4BossInactive_01, VO_Story_Hero_Thelwater_Male_Human_BOM_Scabbs_Mission4BossInactive_02, VO_Story_Hero_Thelwater_Male_Human_BOM_Scabbs_Mission4BossInactive_03, VO_Story_Hero_Thelwater_Male_Human_BOM_Scabbs_Mission4Death_01, VO_Story_Hero_Thelwater_Male_Human_BOM_Scabbs_Mission4EmoteResponse_01, VO_Story_Hero_Thelwater_Male_Human_BOM_Scabbs_Mission4ExchangeAAlt_01, VO_Story_Hero_Thelwater_Male_Human_BOM_Scabbs_Mission4HeroPower_01,
			VO_Story_Hero_Thelwater_Male_Human_BOM_Scabbs_Mission4HeroPower_02, VO_Story_Hero_Thelwater_Male_Human_BOM_Scabbs_Mission4HeroPower_03, VO_Story_Hero_Thelwater_Male_Human_BOM_Scabbs_Mission4HeroPower_04, VO_Story_Hero_Thelwater_Male_Human_BOM_Scabbs_Mission4Idle_01, VO_Story_Hero_Thelwater_Male_Human_BOM_Scabbs_Mission4Idle_02, VO_Story_Hero_Thelwater_Male_Human_BOM_Scabbs_Mission4Idle_03, VO_Story_Hero_Thelwater_Male_Human_BOM_Scabbs_Mission4Intro_02, VO_Story_Hero_Thelwater_Male_Human_BOM_Scabbs_Mission4IntroAlt_02, VO_Story_Hero_Thelwater_Male_Human_BOM_Scabbs_Mission4Loss_01, VO_PVPDR_Hero_Scabbs_Male_Gnome_Death_01
		};
		SetBossVOLines(m_SoundFiles);
		foreach (string soundFile in m_SoundFiles)
		{
			PreloadSound(soundFile);
		}
	}

	public override void OnCreateGame()
	{
		base.OnCreateGame();
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
		switch (missionEvent)
		{
		case 516:
			yield return MissionPlaySound(enemyActor, VO_Story_Hero_Thelwater_Male_Human_BOM_Scabbs_Mission4Death_01);
			break;
		case 517:
			yield return MissionPlayVO(enemyActor, m_InGame_BossIdleLines);
			break;
		case 510:
			yield return MissionPlayVO(enemyActor, m_InGame_BossUsesHeroPowerLines);
			break;
		case 515:
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Thelwater_Male_Human_BOM_Scabbs_Mission4EmoteResponse_01);
			break;
		case 514:
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission4Intro_01);
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Thelwater_Male_Human_BOM_Scabbs_Mission4Intro_02);
			break;
		case 507:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Thelwater_Male_Human_BOM_Scabbs_Mission4Loss_01);
			GameState.Get().SetBusy(busy: false);
			break;
		case 518:
		{
			float num = Random.Range(0f, 1f);
			float dawnGraspSpeakingChance = 0.5f;
			if (num > dawnGraspSpeakingChance)
			{
				yield return MissionPlayVO(friendlyActor, m_InGame_PlayerIdleLines);
			}
			else
			{
				yield return MissionPlayVO(BOM_07_Scabbs_Dungeon.SWDawngraspMinion_BrassRing_Quote, m_InGame_PlayerAltIdleLines);
			}
			break;
		}
		case 519:
			yield return MissionPlaySound(enemyActor, VO_PVPDR_Hero_Scabbs_Male_Gnome_Death_01);
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
		switch (turn)
		{
		case 1:
			yield return MissionPlayVO(BOM_07_Scabbs_Dungeon.SWDawngraspMinion_BrassRing_Quote, VO_Story_Hero_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission4ExchangeA_01);
			break;
		case 3:
			yield return MissionPlayVO(BOM_07_Scabbs_Dungeon.SWDawngraspMinion_BrassRing_Quote, VO_Story_Hero_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission4ExchangeB_01);
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission4ExchangeD_02);
			yield return MissionPlayVO(BOM_07_Scabbs_Dungeon.SWDawngraspMinion_BrassRing_Quote, VO_Story_Hero_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission4ExchangeD_03);
			break;
		case 7:
			yield return MissionPlayVO(BOM_07_Scabbs_Dungeon.SWDawngraspMinion_BrassRing_Quote, VO_Story_Hero_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission4ExchangeE_01);
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission4ExchangeE_02);
			yield return MissionPlayVO(BOM_07_Scabbs_Dungeon.SWDawngraspMinion_BrassRing_Quote, VO_Story_Hero_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission4ExchangeE_03);
			break;
		case 11:
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission4Idle_03);
			yield return MissionPlayVO(BOM_07_Scabbs_Dungeon.SWDawngraspMinion_BrassRing_Quote, VO_Story_Hero_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission4Idle_01);
			break;
		case 15:
			yield return MissionPlayVO(BOM_07_Scabbs_Dungeon.SWDawngraspMinion_BrassRing_Quote, VO_Story_Hero_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission4Idle_03);
			break;
		case 19:
			yield return MissionPlayVO(BOM_07_Scabbs_Dungeon.SWDawngraspMinion_BrassRing_Quote, VO_Story_Hero_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission4Idle_04);
			break;
		}
	}
}
