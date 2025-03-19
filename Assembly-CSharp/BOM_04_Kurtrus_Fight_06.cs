using System.Collections;
using System.Collections.Generic;

public class BOM_04_Kurtrus_Fight_06 : BOM_04_Kurtrus_Dungeon
{
	private static readonly AssetReference VO_Story_Hero_Cariel_Female_Human_Story_Kurtrus_Mission6ExchangeA_Cariel_01 = new AssetReference("VO_Story_Hero_Cariel_Female_Human_Story_Kurtrus_Mission6ExchangeA_Cariel_01.prefab:f12f354ad2c108f4aab31199b59407b5");

	private static readonly AssetReference VO_Story_Hero_Cariel_Female_Human_Story_Kurtrus_Mission6ExchangeG_Cariel_01 = new AssetReference("VO_Story_Hero_Cariel_Female_Human_Story_Kurtrus_Mission6ExchangeG_Cariel_01.prefab:a18f74dbb43d348419efe589cc34cd34");

	private static readonly AssetReference VO_Story_Hero_Cariel_Female_Human_Story_Kurtrus_Mission6ExchangeM_Cariel_01 = new AssetReference("VO_Story_Hero_Cariel_Female_Human_Story_Kurtrus_Mission6ExchangeM_Cariel_01.prefab:4ab8b55970c3a0c4e870b27047295ffd");

	private static readonly AssetReference VO_Story_Hero_Cariel_Female_Human_Story_Kurtrus_Mission6Victory_03 = new AssetReference("VO_Story_Hero_Cariel_Female_Human_Story_Kurtrus_Mission6Victory_03.prefab:26bc12b54337d8247a082845e02c3195");

	private static readonly AssetReference VO_Story_Hero_Dawngrasp_X_BloodElf_Story_Kurtrus_Mission6Death_01 = new AssetReference("VO_Story_Hero_Dawngrasp_X_BloodElf_Story_Kurtrus_Mission6Death_01.prefab:be33bbe45fd517b4ca48c62a5ff0d607");

	private static readonly AssetReference VO_Story_Hero_Dawngrasp_X_BloodElf_Story_Kurtrus_Mission6EmoteResponse_01 = new AssetReference("VO_Story_Hero_Dawngrasp_X_BloodElf_Story_Kurtrus_Mission6EmoteResponse_01.prefab:8487df824dd6c154a887ad8e0d0e6ddb");

	private static readonly AssetReference VO_Story_Hero_Dawngrasp_X_BloodElf_Story_Kurtrus_Mission6ExchangeE_02 = new AssetReference("VO_Story_Hero_Dawngrasp_X_BloodElf_Story_Kurtrus_Mission6ExchangeE_02.prefab:df4e30cdb07517d40aa4639e273cf1a3");

	private static readonly AssetReference VO_Story_Hero_Dawngrasp_X_BloodElf_Story_Kurtrus_Mission6ExchangeK_01 = new AssetReference("VO_Story_Hero_Dawngrasp_X_BloodElf_Story_Kurtrus_Mission6ExchangeK_01.prefab:1844aefc3ac103e40852345b050ae2da");

	private static readonly AssetReference VO_Story_Hero_Dawngrasp_X_BloodElf_Story_Kurtrus_Mission6ExchangeL_01 = new AssetReference("VO_Story_Hero_Dawngrasp_X_BloodElf_Story_Kurtrus_Mission6ExchangeL_01.prefab:2f1b596ff8d08ad4fbee8d6c43987f3b");

	private static readonly AssetReference VO_Story_Hero_Dawngrasp_X_BloodElf_Story_Kurtrus_Mission6ExchangeM_01 = new AssetReference("VO_Story_Hero_Dawngrasp_X_BloodElf_Story_Kurtrus_Mission6ExchangeM_01.prefab:6318945eac055fa41b9d8c5c4acc678d");

	private static readonly AssetReference VO_Story_Hero_Dawngrasp_X_BloodElf_Story_Kurtrus_Mission6ExchangeN_01 = new AssetReference("VO_Story_Hero_Dawngrasp_X_BloodElf_Story_Kurtrus_Mission6ExchangeN_01.prefab:b746b4e0ddf4ca949b816b43711579f4");

	private static readonly AssetReference VO_Story_Hero_Dawngrasp_X_BloodElf_Story_Kurtrus_Mission6HeroPower_01 = new AssetReference("VO_Story_Hero_Dawngrasp_X_BloodElf_Story_Kurtrus_Mission6HeroPower_01.prefab:5a536901470a5744384f280414f63a3c");

	private static readonly AssetReference VO_Story_Hero_Dawngrasp_X_BloodElf_Story_Kurtrus_Mission6HeroPower_02 = new AssetReference("VO_Story_Hero_Dawngrasp_X_BloodElf_Story_Kurtrus_Mission6HeroPower_02.prefab:7ad3ba798b23bad45b6d007f3f74444e");

	private static readonly AssetReference VO_Story_Hero_Dawngrasp_X_BloodElf_Story_Kurtrus_Mission6HeroPower_03 = new AssetReference("VO_Story_Hero_Dawngrasp_X_BloodElf_Story_Kurtrus_Mission6HeroPower_03.prefab:4a9ee11d13bd0df4cb4ce89720842daa");

	private static readonly AssetReference VO_Story_Hero_Dawngrasp_X_BloodElf_Story_Kurtrus_Mission6Idle_01 = new AssetReference("VO_Story_Hero_Dawngrasp_X_BloodElf_Story_Kurtrus_Mission6Idle_01.prefab:6e6fb0a71726f3547ab16bece219c761");

	private static readonly AssetReference VO_Story_Hero_Dawngrasp_X_BloodElf_Story_Kurtrus_Mission6Idle_02 = new AssetReference("VO_Story_Hero_Dawngrasp_X_BloodElf_Story_Kurtrus_Mission6Idle_02.prefab:598216353aca0394d80db4c523b3edb7");

	private static readonly AssetReference VO_Story_Hero_Dawngrasp_X_BloodElf_Story_Kurtrus_Mission6Idle_03 = new AssetReference("VO_Story_Hero_Dawngrasp_X_BloodElf_Story_Kurtrus_Mission6Idle_03.prefab:3de0f7c31c2e8f9488ea1a720908ffb9");

	private static readonly AssetReference VO_Story_Hero_Dawngrasp_X_BloodElf_Story_Kurtrus_Mission6Loss_01 = new AssetReference("VO_Story_Hero_Dawngrasp_X_BloodElf_Story_Kurtrus_Mission6Loss_01.prefab:33a5d620e4a2cf84983071368a52cf01");

	private static readonly AssetReference VO_Story_Hero_Kazakus_Male_Troll_Story_Kurtrus_Mission6Death_01 = new AssetReference("VO_Story_Hero_Kazakus_Male_Troll_Story_Kurtrus_Mission6Death_01.prefab:20b737cb7ad69f44ea339c81c15e6ee8");

	private static readonly AssetReference VO_Story_Hero_Kazakus_Male_Troll_Story_Kurtrus_Mission6EmoteResponse_01 = new AssetReference("VO_Story_Hero_Kazakus_Male_Troll_Story_Kurtrus_Mission6EmoteResponse_01.prefab:c774866b4b28e2a4290cad3aa52d7dae");

	private static readonly AssetReference VO_Story_Hero_Kazakus_Male_Troll_Story_Kurtrus_Mission6ExchangeB_01 = new AssetReference("VO_Story_Hero_Kazakus_Male_Troll_Story_Kurtrus_Mission6ExchangeB_01.prefab:9c476d2b400c6eb41a789af942185602");

	private static readonly AssetReference VO_Story_Hero_Kazakus_Male_Troll_Story_Kurtrus_Mission6ExchangeC_01 = new AssetReference("VO_Story_Hero_Kazakus_Male_Troll_Story_Kurtrus_Mission6ExchangeC_01.prefab:38a3dd25d0dbfdd45bb4f297689a0169");

	private static readonly AssetReference VO_Story_Hero_Kazakus_Male_Troll_Story_Kurtrus_Mission6ExchangeD_01 = new AssetReference("VO_Story_Hero_Kazakus_Male_Troll_Story_Kurtrus_Mission6ExchangeD_01.prefab:95f7a42b988adb844b3324e0a05c671f");

	private static readonly AssetReference VO_Story_Hero_Kazakus_Male_Troll_Story_Kurtrus_Mission6ExchangeE_03 = new AssetReference("VO_Story_Hero_Kazakus_Male_Troll_Story_Kurtrus_Mission6ExchangeE_03.prefab:0496de544ae531a4599d2e72f6c89e61");

	private static readonly AssetReference VO_Story_Hero_Kazakus_Male_Troll_Story_Kurtrus_Mission6ExchangeF_01 = new AssetReference("VO_Story_Hero_Kazakus_Male_Troll_Story_Kurtrus_Mission6ExchangeF_01.prefab:002f94670bb17f04e800cbb26492dcf0");

	private static readonly AssetReference VO_Story_Hero_Kazakus_Male_Troll_Story_Kurtrus_Mission6ExchangeJ_01 = new AssetReference("VO_Story_Hero_Kazakus_Male_Troll_Story_Kurtrus_Mission6ExchangeJ_01.prefab:64501147b1435e24eb37c5bcb3339ea4");

	private static readonly AssetReference VO_Story_Hero_Kazakus_Male_Troll_Story_Kurtrus_Mission6HeroPower_01 = new AssetReference("VO_Story_Hero_Kazakus_Male_Troll_Story_Kurtrus_Mission6HeroPower_01.prefab:32e5a7bf927c558428e3421fb400f31b");

	private static readonly AssetReference VO_Story_Hero_Kazakus_Male_Troll_Story_Kurtrus_Mission6HeroPower_02 = new AssetReference("VO_Story_Hero_Kazakus_Male_Troll_Story_Kurtrus_Mission6HeroPower_02.prefab:b6cedc6c64c32dc4cb44936ca6775009");

	private static readonly AssetReference VO_Story_Hero_Kazakus_Male_Troll_Story_Kurtrus_Mission6HeroPower_03 = new AssetReference("VO_Story_Hero_Kazakus_Male_Troll_Story_Kurtrus_Mission6HeroPower_03.prefab:d6626c40f2191e44fac80fe4fe3029cf");

	private static readonly AssetReference VO_Story_Hero_Kazakus_Male_Troll_Story_Kurtrus_Mission6Idle_01 = new AssetReference("VO_Story_Hero_Kazakus_Male_Troll_Story_Kurtrus_Mission6Idle_01.prefab:b969ff3970632ec4488c4c0cf3f21f0f");

	private static readonly AssetReference VO_Story_Hero_Kazakus_Male_Troll_Story_Kurtrus_Mission6Idle_02 = new AssetReference("VO_Story_Hero_Kazakus_Male_Troll_Story_Kurtrus_Mission6Idle_02.prefab:3d625d213f4d3a74ab8d533a93475f69");

	private static readonly AssetReference VO_Story_Hero_Kazakus_Male_Troll_Story_Kurtrus_Mission6Idle_03 = new AssetReference("VO_Story_Hero_Kazakus_Male_Troll_Story_Kurtrus_Mission6Idle_03.prefab:5602c0e6bf7d7ff438e17a8dde33cd51");

	private static readonly AssetReference VO_Story_Hero_Kazakus_Male_Troll_Story_Kurtrus_Mission6Intro_01 = new AssetReference("VO_Story_Hero_Kazakus_Male_Troll_Story_Kurtrus_Mission6Intro_01.prefab:867d475124ccc3243b5c30befed99416");

	private static readonly AssetReference VO_Story_Hero_Kazakus_Male_Troll_Story_Kurtrus_Mission6Loss_01 = new AssetReference("VO_Story_Hero_Kazakus_Male_Troll_Story_Kurtrus_Mission6Loss_01.prefab:f61e11592b20e654a89be6df0ec670a1");

	private static readonly AssetReference VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission6ExchangeC_02 = new AssetReference("VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission6ExchangeC_02.prefab:44ed2c562623e05499bf9b9422170298");

	private static readonly AssetReference VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission6ExchangeE_01 = new AssetReference("VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission6ExchangeE_01.prefab:213b8f9f87aa22a42887488d38fdb4a8");

	private static readonly AssetReference VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission6ExchangeF_02 = new AssetReference("VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission6ExchangeF_02.prefab:86f78ef1ac4c14440a06637b379ffb67");

	private static readonly AssetReference VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission6ExchangeL_02 = new AssetReference("VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission6ExchangeL_02.prefab:e08227a24e293b2409e0510a42c4b199");

	private static readonly AssetReference VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission6Intro_02 = new AssetReference("VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission6Intro_02.prefab:ed37ddcd59c1dc349afab410cbc0d3d2");

	private static readonly AssetReference VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission6Victory_01 = new AssetReference("VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission6Victory_01.prefab:649930fe8890ce049b26109c896dbd43");

	private static readonly AssetReference VO_Story_Hero_Scabbs_Male_Gnome_Story_Kurtrus_Mission6ExchangeA_Scabbs_01 = new AssetReference("VO_Story_Hero_Scabbs_Male_Gnome_Story_Kurtrus_Mission6ExchangeA_Scabbs_01.prefab:4869404e36df6524b8dc65155ade1ac9");

	private static readonly AssetReference VO_Story_Hero_Scabbs_Male_Gnome_Story_Kurtrus_Mission6ExchangeG_Scabbs_01 = new AssetReference("VO_Story_Hero_Scabbs_Male_Gnome_Story_Kurtrus_Mission6ExchangeG_Scabbs_01.prefab:108df96dd0b656e41be0c644a3776b48");

	private static readonly AssetReference VO_Story_Hero_Scabbs_Male_Gnome_Story_Kurtrus_Mission6ExchangeM_Scabbs_01 = new AssetReference("VO_Story_Hero_Scabbs_Male_Gnome_Story_Kurtrus_Mission6ExchangeM_Scabbs_01.prefab:9ed1d56d8e0325b42a725b242bfdb676");

	private static readonly AssetReference VO_Story_Hero_Tavish_Male_Dwarf_Story_Kurtrus_Mission6ExchangeA_Tavish_01 = new AssetReference("VO_Story_Hero_Tavish_Male_Dwarf_Story_Kurtrus_Mission6ExchangeA_Tavish_01.prefab:52ad76a2882ae094eb14325c265ecc6f");

	private static readonly AssetReference VO_Story_Hero_Tavish_Male_Dwarf_Story_Kurtrus_Mission6ExchangeG_Tavish_01 = new AssetReference("VO_Story_Hero_Tavish_Male_Dwarf_Story_Kurtrus_Mission6ExchangeG_Tavish_01.prefab:db5548fec3d3c594e83f38656f90a314");

	private static readonly AssetReference VO_Story_Hero_Tavish_Male_Dwarf_Story_Kurtrus_Mission6ExchangeM_Tavish_01 = new AssetReference("VO_Story_Hero_Tavish_Male_Dwarf_Story_Kurtrus_Mission6ExchangeM_Tavish_01.prefab:92dbb7fc2ed2c7743b0b03c9ad0f46de");

	private static readonly AssetReference VO_Story_Hero_Xyrella_Female_Draenei_Story_Kurtrus_Mission6ExchangeA_Xyrella_01 = new AssetReference("VO_Story_Hero_Xyrella_Female_Draenei_Story_Kurtrus_Mission6ExchangeA_Xyrella_01.prefab:e7bfe27c4f1883c48a93ea87cf560011");

	private static readonly AssetReference VO_Story_Hero_Xyrella_Female_Draenei_Story_Kurtrus_Mission6ExchangeB_02 = new AssetReference("VO_Story_Hero_Xyrella_Female_Draenei_Story_Kurtrus_Mission6ExchangeB_02.prefab:b483995d86ee6e545a4af51c33a72221");

	private static readonly AssetReference VO_Story_Hero_Xyrella_Female_Draenei_Story_Kurtrus_Mission6ExchangeD_02 = new AssetReference("VO_Story_Hero_Xyrella_Female_Draenei_Story_Kurtrus_Mission6ExchangeD_02.prefab:39648743aa4afb74baa4ce4e28878da7");

	private static readonly AssetReference VO_Story_Hero_Xyrella_Female_Draenei_Story_Kurtrus_Mission6ExchangeG_Xyrella_01 = new AssetReference("VO_Story_Hero_Xyrella_Female_Draenei_Story_Kurtrus_Mission6ExchangeG_Xyrella_01.prefab:7419208769e04df42a54fe0b9c45ef9c");

	private static readonly AssetReference VO_Story_Hero_Xyrella_Female_Draenei_Story_Kurtrus_Mission6ExchangeM_Xyrella_01 = new AssetReference("VO_Story_Hero_Xyrella_Female_Draenei_Story_Kurtrus_Mission6ExchangeM_Xyrella_01.prefab:173edbce7d2281841b3f9f3446da1ac8");

	private static readonly AssetReference VO_Story_Hero_Xyrella_Female_Draenei_Story_Kurtrus_Mission6Victory_02 = new AssetReference("VO_Story_Hero_Xyrella_Female_Draenei_Story_Kurtrus_Mission6Victory_02.prefab:15625151b0eca7944a5f0342c9d805f8");

	private List<string> m_InGame_Kazakus_BossIdleLines = new List<string> { VO_Story_Hero_Kazakus_Male_Troll_Story_Kurtrus_Mission6Idle_01, VO_Story_Hero_Kazakus_Male_Troll_Story_Kurtrus_Mission6Idle_02, VO_Story_Hero_Kazakus_Male_Troll_Story_Kurtrus_Mission6Idle_03 };

	private List<string> m_InGame_Kazakus_BossHeroPowerLines = new List<string> { VO_Story_Hero_Kazakus_Male_Troll_Story_Kurtrus_Mission6HeroPower_01, VO_Story_Hero_Kazakus_Male_Troll_Story_Kurtrus_Mission6HeroPower_02, VO_Story_Hero_Kazakus_Male_Troll_Story_Kurtrus_Mission6HeroPower_03 };

	private List<string> m_InGame_Dawngrasp_BossHeroPowerLines = new List<string> { VO_Story_Hero_Dawngrasp_X_BloodElf_Story_Kurtrus_Mission6HeroPower_01, VO_Story_Hero_Dawngrasp_X_BloodElf_Story_Kurtrus_Mission6HeroPower_02, VO_Story_Hero_Dawngrasp_X_BloodElf_Story_Kurtrus_Mission6HeroPower_03 };

	private List<string> m_InGame_Dawngrasp_BossIdleLines = new List<string> { VO_Story_Hero_Dawngrasp_X_BloodElf_Story_Kurtrus_Mission6Idle_01, VO_Story_Hero_Dawngrasp_X_BloodElf_Story_Kurtrus_Mission6Idle_02, VO_Story_Hero_Dawngrasp_X_BloodElf_Story_Kurtrus_Mission6Idle_03 };

	private HashSet<string> m_playedLines = new HashSet<string>();

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> m_SoundFiles = new List<string>
		{
			VO_Story_Hero_Cariel_Female_Human_Story_Kurtrus_Mission6ExchangeA_Cariel_01, VO_Story_Hero_Cariel_Female_Human_Story_Kurtrus_Mission6ExchangeG_Cariel_01, VO_Story_Hero_Cariel_Female_Human_Story_Kurtrus_Mission6ExchangeM_Cariel_01, VO_Story_Hero_Cariel_Female_Human_Story_Kurtrus_Mission6Victory_03, VO_Story_Hero_Dawngrasp_X_BloodElf_Story_Kurtrus_Mission6Death_01, VO_Story_Hero_Dawngrasp_X_BloodElf_Story_Kurtrus_Mission6EmoteResponse_01, VO_Story_Hero_Dawngrasp_X_BloodElf_Story_Kurtrus_Mission6ExchangeE_02, VO_Story_Hero_Dawngrasp_X_BloodElf_Story_Kurtrus_Mission6ExchangeK_01, VO_Story_Hero_Dawngrasp_X_BloodElf_Story_Kurtrus_Mission6ExchangeL_01, VO_Story_Hero_Dawngrasp_X_BloodElf_Story_Kurtrus_Mission6ExchangeM_01,
			VO_Story_Hero_Dawngrasp_X_BloodElf_Story_Kurtrus_Mission6ExchangeN_01, VO_Story_Hero_Dawngrasp_X_BloodElf_Story_Kurtrus_Mission6HeroPower_01, VO_Story_Hero_Dawngrasp_X_BloodElf_Story_Kurtrus_Mission6HeroPower_02, VO_Story_Hero_Dawngrasp_X_BloodElf_Story_Kurtrus_Mission6HeroPower_03, VO_Story_Hero_Dawngrasp_X_BloodElf_Story_Kurtrus_Mission6Idle_01, VO_Story_Hero_Dawngrasp_X_BloodElf_Story_Kurtrus_Mission6Idle_02, VO_Story_Hero_Dawngrasp_X_BloodElf_Story_Kurtrus_Mission6Idle_03, VO_Story_Hero_Dawngrasp_X_BloodElf_Story_Kurtrus_Mission6Loss_01, VO_Story_Hero_Kazakus_Male_Troll_Story_Kurtrus_Mission6Death_01, VO_Story_Hero_Kazakus_Male_Troll_Story_Kurtrus_Mission6EmoteResponse_01,
			VO_Story_Hero_Kazakus_Male_Troll_Story_Kurtrus_Mission6ExchangeB_01, VO_Story_Hero_Kazakus_Male_Troll_Story_Kurtrus_Mission6ExchangeC_01, VO_Story_Hero_Kazakus_Male_Troll_Story_Kurtrus_Mission6ExchangeD_01, VO_Story_Hero_Kazakus_Male_Troll_Story_Kurtrus_Mission6ExchangeE_03, VO_Story_Hero_Kazakus_Male_Troll_Story_Kurtrus_Mission6ExchangeF_01, VO_Story_Hero_Kazakus_Male_Troll_Story_Kurtrus_Mission6ExchangeJ_01, VO_Story_Hero_Kazakus_Male_Troll_Story_Kurtrus_Mission6HeroPower_01, VO_Story_Hero_Kazakus_Male_Troll_Story_Kurtrus_Mission6HeroPower_02, VO_Story_Hero_Kazakus_Male_Troll_Story_Kurtrus_Mission6HeroPower_03, VO_Story_Hero_Kazakus_Male_Troll_Story_Kurtrus_Mission6Idle_01,
			VO_Story_Hero_Kazakus_Male_Troll_Story_Kurtrus_Mission6Idle_02, VO_Story_Hero_Kazakus_Male_Troll_Story_Kurtrus_Mission6Idle_03, VO_Story_Hero_Kazakus_Male_Troll_Story_Kurtrus_Mission6Intro_01, VO_Story_Hero_Kazakus_Male_Troll_Story_Kurtrus_Mission6Loss_01, VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission6ExchangeC_02, VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission6ExchangeE_01, VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission6ExchangeF_02, VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission6ExchangeL_02, VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission6Intro_02, VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission6Victory_01,
			VO_Story_Hero_Scabbs_Male_Gnome_Story_Kurtrus_Mission6ExchangeA_Scabbs_01, VO_Story_Hero_Scabbs_Male_Gnome_Story_Kurtrus_Mission6ExchangeG_Scabbs_01, VO_Story_Hero_Scabbs_Male_Gnome_Story_Kurtrus_Mission6ExchangeM_Scabbs_01, VO_Story_Hero_Tavish_Male_Dwarf_Story_Kurtrus_Mission6ExchangeA_Tavish_01, VO_Story_Hero_Tavish_Male_Dwarf_Story_Kurtrus_Mission6ExchangeG_Tavish_01, VO_Story_Hero_Tavish_Male_Dwarf_Story_Kurtrus_Mission6ExchangeM_Tavish_01, VO_Story_Hero_Xyrella_Female_Draenei_Story_Kurtrus_Mission6ExchangeA_Xyrella_01, VO_Story_Hero_Xyrella_Female_Draenei_Story_Kurtrus_Mission6ExchangeB_02, VO_Story_Hero_Xyrella_Female_Draenei_Story_Kurtrus_Mission6ExchangeD_02, VO_Story_Hero_Xyrella_Female_Draenei_Story_Kurtrus_Mission6ExchangeG_Xyrella_01,
			VO_Story_Hero_Xyrella_Female_Draenei_Story_Kurtrus_Mission6ExchangeM_Xyrella_01, VO_Story_Hero_Xyrella_Female_Draenei_Story_Kurtrus_Mission6Victory_02
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
		Actor friendlyHeroPowerActor = GameState.Get().GetFriendlySidePlayer().GetHeroPower()
			.GetCard()
			.GetActor();
		string opposingHeroCard = GameState.Get().GetOpposingSidePlayer().GetHero()
			.GetCardId();
		switch (missionEvent)
		{
		case 514:
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Kazakus_Male_Troll_Story_Kurtrus_Mission6Intro_01);
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission6Intro_02);
			break;
		case 506:
			if (opposingHeroCard == "BOM_04_Kazakus_006hb")
			{
				yield return MissionPlayVO(enemyActor, VO_Story_Hero_Kazakus_Male_Troll_Story_Kurtrus_Mission6Loss_01);
			}
			else
			{
				yield return MissionPlayVO(enemyActor, VO_Story_Hero_Dawngrasp_X_BloodElf_Story_Kurtrus_Mission6Loss_01);
			}
			break;
		case 504:
			MissionPause(pause: true);
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission6Victory_01);
			if (XyrellaIsHeroPower)
			{
				yield return MissionPlayVO(friendlyHeroPowerActor, VO_Story_Hero_Xyrella_Female_Draenei_Story_Kurtrus_Mission6Victory_02);
			}
			else
			{
				yield return MissionPlayVO(Xyrella2_BrassRing_Quote, VO_Story_Hero_Xyrella_Female_Draenei_Story_Kurtrus_Mission6Victory_02);
			}
			if (CarielIsHeroPower)
			{
				yield return MissionPlayVO(friendlyHeroPowerActor, VO_Story_Hero_Cariel_Female_Human_Story_Kurtrus_Mission6Victory_03);
			}
			else
			{
				yield return MissionPlayVO(Cariel_BrassRing_Quote, VO_Story_Hero_Cariel_Female_Human_Story_Kurtrus_Mission6Victory_03);
			}
			MissionPause(pause: false);
			break;
		case 517:
			if (opposingHeroCard == "BOM_04_Kazakus_006hb")
			{
				yield return MissionPlayVO(enemyActor, m_InGame_Kazakus_BossIdleLines);
			}
			else
			{
				yield return MissionPlayVO(enemyActor, m_InGame_Dawngrasp_BossIdleLines);
			}
			break;
		case 515:
			if (opposingHeroCard == "BOM_04_Kazakus_006hb")
			{
				yield return MissionPlayVO(enemyActor, VO_Story_Hero_Kazakus_Male_Troll_Story_Kurtrus_Mission6EmoteResponse_01);
			}
			else
			{
				yield return MissionPlayVO(enemyActor, VO_Story_Hero_Dawngrasp_X_BloodElf_Story_Kurtrus_Mission6EmoteResponse_01);
			}
			break;
		case 510:
			if (opposingHeroCard == "BOM_04_Kazakus_006hb")
			{
				yield return MissionPlayVO(enemyActor, m_InGame_Kazakus_BossHeroPowerLines);
			}
			else
			{
				yield return MissionPlayVO(enemyActor, m_InGame_Dawngrasp_BossHeroPowerLines);
			}
			break;
		case 100:
			MissionPause(pause: true);
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Kazakus_Male_Troll_Story_Kurtrus_Mission6ExchangeJ_01);
			MissionPause(pause: false);
			break;
		case 101:
			MissionPause(pause: true);
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Dawngrasp_X_BloodElf_Story_Kurtrus_Mission6ExchangeK_01);
			MissionPause(pause: false);
			break;
		case 102:
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission6ExchangeE_01);
			yield return MissionPlayVO(Dawngrasp_BrassRing, VO_Story_Hero_Dawngrasp_X_BloodElf_Story_Kurtrus_Mission6ExchangeE_02);
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Kazakus_Male_Troll_Story_Kurtrus_Mission6ExchangeE_03);
			break;
		case 107:
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Kazakus_Male_Troll_Story_Kurtrus_Mission6ExchangeD_01);
			if (XyrellaIsHeroPower)
			{
				yield return MissionPlayVO(friendlyHeroPowerActor, VO_Story_Hero_Xyrella_Female_Draenei_Story_Kurtrus_Mission6ExchangeD_02);
			}
			else
			{
				yield return MissionPlayVO(Xyrella2_BrassRing_Quote, VO_Story_Hero_Xyrella_Female_Draenei_Story_Kurtrus_Mission6ExchangeD_02);
			}
			break;
		case 108:
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Kazakus_Male_Troll_Story_Kurtrus_Mission6ExchangeC_01);
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission6ExchangeC_02);
			break;
		case 103:
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Kazakus_Male_Troll_Story_Kurtrus_Mission6ExchangeF_01);
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission6ExchangeF_02);
			break;
		case 104:
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Dawngrasp_X_BloodElf_Story_Kurtrus_Mission6ExchangeL_01);
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission6ExchangeL_02);
			break;
		case 105:
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Dawngrasp_X_BloodElf_Story_Kurtrus_Mission6ExchangeM_01);
			if (ScabbsIsHeroPower)
			{
				yield return MissionPlayVO(friendlyHeroPowerActor, VO_Story_Hero_Scabbs_Male_Gnome_Story_Kurtrus_Mission6ExchangeM_Scabbs_01);
			}
			if (TavishIsHeroPower)
			{
				yield return MissionPlayVO(friendlyHeroPowerActor, VO_Story_Hero_Tavish_Male_Dwarf_Story_Kurtrus_Mission6ExchangeM_Tavish_01);
			}
			if (XyrellaIsHeroPower)
			{
				yield return MissionPlayVO(friendlyHeroPowerActor, VO_Story_Hero_Xyrella_Female_Draenei_Story_Kurtrus_Mission6ExchangeM_Xyrella_01);
			}
			break;
		case 106:
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Dawngrasp_X_BloodElf_Story_Kurtrus_Mission6ExchangeN_01);
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
		if (m_playedLines.Contains(entity.GetCardId()) && entity.GetCardType() != TAG_CARDTYPE.HERO_POWER)
		{
			yield break;
		}
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
		Actor friendlyHeroPowerActor = GameState.Get().GetFriendlySidePlayer().GetHeroPower()
			.GetCard()
			.GetActor();
		switch (cardID)
		{
		case "CFM_621t":
		case "CFM_621t4":
		case "CFM_621t5":
			if (CarielIsHeroPower)
			{
				yield return MissionPlayVO(friendlyHeroPowerActor, VO_Story_Hero_Cariel_Female_Human_Story_Kurtrus_Mission6ExchangeG_Cariel_01);
			}
			if (ScabbsIsHeroPower)
			{
				yield return MissionPlayVO(friendlyHeroPowerActor, VO_Story_Hero_Scabbs_Male_Gnome_Story_Kurtrus_Mission6ExchangeG_Scabbs_01);
			}
			if (TavishIsHeroPower)
			{
				yield return MissionPlayVO(friendlyHeroPowerActor, VO_Story_Hero_Tavish_Male_Dwarf_Story_Kurtrus_Mission6ExchangeG_Tavish_01);
			}
			if (XyrellaIsHeroPower)
			{
				yield return MissionPlayVO(friendlyHeroPowerActor, VO_Story_Hero_Xyrella_Female_Draenei_Story_Kurtrus_Mission6ExchangeG_Xyrella_01);
			}
			break;
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
		GameState.Get().GetFriendlySidePlayer().GetHero()
			.GetCard()
			.GetActor();
		Actor friendlyHeroPowerActor = GameState.Get().GetFriendlySidePlayer().GetHeroPower()
			.GetCard()
			.GetActor();
		switch (turn)
		{
		case 3:
			if (CarielIsHeroPower)
			{
				yield return MissionPlayVO(friendlyHeroPowerActor, VO_Story_Hero_Cariel_Female_Human_Story_Kurtrus_Mission6ExchangeA_Cariel_01);
			}
			if (ScabbsIsHeroPower)
			{
				yield return MissionPlayVO(friendlyHeroPowerActor, VO_Story_Hero_Scabbs_Male_Gnome_Story_Kurtrus_Mission6ExchangeA_Scabbs_01);
			}
			if (TavishIsHeroPower)
			{
				yield return MissionPlayVO(friendlyHeroPowerActor, VO_Story_Hero_Tavish_Male_Dwarf_Story_Kurtrus_Mission6ExchangeA_Tavish_01);
			}
			if (XyrellaIsHeroPower)
			{
				yield return MissionPlayVO(friendlyHeroPowerActor, VO_Story_Hero_Xyrella_Female_Draenei_Story_Kurtrus_Mission6ExchangeA_Xyrella_01);
			}
			break;
		case 5:
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Kazakus_Male_Troll_Story_Kurtrus_Mission6ExchangeB_01);
			if (XyrellaIsHeroPower)
			{
				yield return MissionPlayVO(friendlyHeroPowerActor, VO_Story_Hero_Xyrella_Female_Draenei_Story_Kurtrus_Mission6ExchangeB_02);
			}
			else
			{
				yield return MissionPlayVO(Xyrella2_BrassRing_Quote, VO_Story_Hero_Xyrella_Female_Draenei_Story_Kurtrus_Mission6ExchangeB_02);
			}
			break;
		}
	}
}
