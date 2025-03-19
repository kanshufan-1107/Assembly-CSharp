using System.Collections;
using System.Collections.Generic;

public class BOM_08_Tavish_Fight_003 : BOM_08_Tavish_Dungeon
{
	private static readonly AssetReference VO_BOM_08_003_Female_Draenei_Xyrella_InGame_HE_Custom_BOM_08_Xyrella_03t_Play_01 = new AssetReference("VO_BOM_08_003_Female_Draenei_Xyrella_InGame_HE_Custom_BOM_08_Xyrella_03t_Play_01.prefab:ecc39377104af254493bdb352bb0fad2");

	private static readonly AssetReference VO_BOM_08_003_Female_Human_Cariel_InGame_HE_Custom_BOM_08_Cariel_03t_Play_01 = new AssetReference("VO_BOM_08_003_Female_Human_Cariel_InGame_HE_Custom_BOM_08_Cariel_03t_Play_01.prefab:8cb9422ecb08a4d4eacb7ffdceaea514");

	private static readonly AssetReference VO_BOM_08_003_Female_Human_Cariel_InGame_HE_Custom_If_CarielBrukan_InPlay_01_A = new AssetReference("VO_BOM_08_003_Female_Human_Cariel_InGame_HE_Custom_If_CarielBrukan_InPlay_01_A.prefab:c1f0a5866c34b8a49b70b7d8ff53bb1e");

	private static readonly AssetReference VO_BOM_08_003_Female_Orc_Rokara_InGame_BossIdle_01 = new AssetReference("VO_BOM_08_003_Female_Orc_Rokara_InGame_BossIdle_01.prefab:ea2e747cfcc606743a8ecef543be7a75");

	private static readonly AssetReference VO_BOM_08_003_Female_Orc_Rokara_InGame_BossIdle_02 = new AssetReference("VO_BOM_08_003_Female_Orc_Rokara_InGame_BossIdle_02.prefab:2b6878f8cdecc794288d2f179d22b359");

	private static readonly AssetReference VO_BOM_08_003_Female_Orc_Rokara_InGame_BossIdle_03 = new AssetReference("VO_BOM_08_003_Female_Orc_Rokara_InGame_BossIdle_03.prefab:ea0b3d1f5d557624890a4844604fadbb");

	private static readonly AssetReference VO_BOM_08_003_Female_Orc_Rokara_InGame_BossUsesHeroPower_01 = new AssetReference("VO_BOM_08_003_Female_Orc_Rokara_InGame_BossUsesHeroPower_01.prefab:9eaf31f59188e634f921f44eae63b458");

	private static readonly AssetReference VO_BOM_08_003_Female_Orc_Rokara_InGame_BossUsesHeroPower_02 = new AssetReference("VO_BOM_08_003_Female_Orc_Rokara_InGame_BossUsesHeroPower_02.prefab:7ebf51854a7cc914e9a00560b035c043");

	private static readonly AssetReference VO_BOM_08_003_Female_Orc_Rokara_InGame_BossUsesHeroPower_03 = new AssetReference("VO_BOM_08_003_Female_Orc_Rokara_InGame_BossUsesHeroPower_03.prefab:e26200d4a0eb8b14cb7cc71c1f84c090");

	private static readonly AssetReference VO_BOM_08_003_Female_Orc_Rokara_InGame_EmoteResponse_01 = new AssetReference("VO_BOM_08_003_Female_Orc_Rokara_InGame_EmoteResponse_01.prefab:2375b454e97c8f54bbeac14591ff31d3");

	private static readonly AssetReference VO_BOM_08_003_Female_Orc_Rokara_InGame_HE_Custom_IfRokaraPlaysPup_01 = new AssetReference("VO_BOM_08_003_Female_Orc_Rokara_InGame_HE_Custom_IfRokaraPlaysPup_01.prefab:1a0cccfd93f12954789c0c12c68b08b5");

	private static readonly AssetReference VO_BOM_08_003_Female_Orc_Rokara_InGame_Introduction_01_B = new AssetReference("VO_BOM_08_003_Female_Orc_Rokara_InGame_Introduction_01_B.prefab:d4df2f9e914b49848b325515d543742f");

	private static readonly AssetReference VO_BOM_08_003_Female_Orc_Rokara_InGame_PlayerLoss_01 = new AssetReference("VO_BOM_08_003_Female_Orc_Rokara_InGame_PlayerLoss_01.prefab:f03d2a7d04e03c841849ea5152731f05");

	private static readonly AssetReference VO_BOM_08_003_Female_Orc_Rokara_InGame_Turn_03_01_A = new AssetReference("VO_BOM_08_003_Female_Orc_Rokara_InGame_Turn_03_01_A.prefab:e7e587ccb78520947bff3012516fb543");

	private static readonly AssetReference VO_BOM_08_003_Female_Orc_Rokara_InGame_Turn_07_01_A = new AssetReference("VO_BOM_08_003_Female_Orc_Rokara_InGame_Turn_07_01_A.prefab:ca41288e7c80ad34885b3caf1a362c72");

	private static readonly AssetReference VO_BOM_08_003_Female_Orc_Rokara_InGame_Turn_11_01_A = new AssetReference("VO_BOM_08_003_Female_Orc_Rokara_InGame_Turn_11_01_A.prefab:3afddfdafff69264a91d3d1df1c41e5f");

	private static readonly AssetReference VO_BOM_08_003_Female_Orc_Rokara_InGame_Turn_11_01_A_1 = new AssetReference("VO_BOM_08_003_Female_Orc_Rokara_InGame_Turn_11_01_A_1.prefab:5d784e8ac6fc5d44fb1ec1c94e584fa5");

	private static readonly AssetReference VO_BOM_08_003_Female_Orc_Rokara_InGame_Turn_11_01_A_2 = new AssetReference("VO_BOM_08_003_Female_Orc_Rokara_InGame_Turn_11_01_A_2.prefab:5a3dffe1f8856e14aa2f8d4ab1c1d44a");

	private static readonly AssetReference VO_BOM_08_003_Female_Orc_Rokara_InGame_Victory_PreExplosion_01_B = new AssetReference("VO_BOM_08_003_Female_Orc_Rokara_InGame_Victory_PreExplosion_01_B.prefab:ba81798c861667e4a863b104b32a072f");

	private static readonly AssetReference VO_BOM_08_003_Female_Orc_Rokara_UI_AWD_Boss_Reveal_General_03_01_C = new AssetReference("VO_BOM_08_003_Female_Orc_Rokara_UI_AWD_Boss_Reveal_General_03_01_C.prefab:9539ab658536a5049855508aeab447bc");

	private static readonly AssetReference VO_BOM_08_003_Male_Dwarf_Tavish_InGame_HE_Custom_If_GuffCrabby_InPlay_01_B = new AssetReference("VO_BOM_08_003_Male_Dwarf_Tavish_InGame_HE_Custom_If_GuffCrabby_InPlay_01_B.prefab:1cc77395cc8faba45a8173969f5ae11b");

	private static readonly AssetReference VO_BOM_08_003_Male_Dwarf_Tavish_InGame_Introduction_01_A = new AssetReference("VO_BOM_08_003_Male_Dwarf_Tavish_InGame_Introduction_01_A.prefab:eec6f7c912463ef49bfdb2cef7010d55");

	private static readonly AssetReference VO_BOM_08_003_Male_Dwarf_Tavish_InGame_Turn_03_01_B = new AssetReference("VO_BOM_08_003_Male_Dwarf_Tavish_InGame_Turn_03_01_B.prefab:f21346102fe768647971ad7ec8a3926b");

	private static readonly AssetReference VO_BOM_08_003_Male_Dwarf_Tavish_InGame_Turn_07_01_B = new AssetReference("VO_BOM_08_003_Male_Dwarf_Tavish_InGame_Turn_07_01_B.prefab:028642ec1ca860b489f75ae02e83673d");

	private static readonly AssetReference VO_BOM_08_003_Male_Dwarf_Tavish_InGame_Turn_07_01_C = new AssetReference("VO_BOM_08_003_Male_Dwarf_Tavish_InGame_Turn_07_01_C.prefab:c0b1ea7ebd63b094dbeea7bacb5c7dee");

	private static readonly AssetReference VO_BOM_08_003_Male_Dwarf_Tavish_InGame_Turn_11_01_B = new AssetReference("VO_BOM_08_003_Male_Dwarf_Tavish_InGame_Turn_11_01_B.prefab:e0ba984bd9289b8439dcc6103ab7f2c7");

	private static readonly AssetReference VO_BOM_08_003_Male_Dwarf_Tavish_UI_AWD_Boss_Reveal_General_03_01_A = new AssetReference("VO_BOM_08_003_Male_Dwarf_Tavish_UI_AWD_Boss_Reveal_General_03_01_A.prefab:ef7bda990885b9244a6659f75f173401");

	private static readonly AssetReference VO_BOM_08_003_Male_Dwarf_Tavish_UI_AWD_Boss_Reveal_General_03_01_B = new AssetReference("VO_BOM_08_003_Male_Dwarf_Tavish_UI_AWD_Boss_Reveal_General_03_01_B.prefab:505d6acde1a3d3947a8fc9adb0506191");

	private static readonly AssetReference VO_BOM_08_003_Male_Gnome_Scabbs_InGame_HE_Custom_BOM_08_Scabbs_03t_Play_01 = new AssetReference("VO_BOM_08_003_Male_Gnome_Scabbs_InGame_HE_Custom_BOM_08_Scabbs_03t_Play_01.prefab:7e8482384d254c5489f980796894f774");

	private static readonly AssetReference VO_BOM_08_003_Male_Gnome_Scabbs_InGame_HE_Custom_If_ScabbsDawngrasp_InPlay_01_B = new AssetReference("VO_BOM_08_003_Male_Gnome_Scabbs_InGame_HE_Custom_If_ScabbsDawngrasp_InPlay_01_B.prefab:014571b4fdb1e9148a7a2cdb8b0fee93");

	private static readonly AssetReference VO_BOM_08_003_Male_NightElf_Kurtrus_InGame_HE_Custom_BOM_08_Kurtrus_03t_Play_01 = new AssetReference("VO_BOM_08_003_Male_NightElf_Kurtrus_InGame_HE_Custom_BOM_08_Kurtrus_03t_Play_01.prefab:df02319cd1b31734899950ed8051abd6");

	private static readonly AssetReference VO_BOM_08_003_Male_Tauren_Guff_InGame_HE_Custom_BOM_08_Guff_03t_Play_01 = new AssetReference("VO_BOM_08_003_Male_Tauren_Guff_InGame_HE_Custom_BOM_08_Guff_03t_Play_01.prefab:a98624eec8fd5fd4eb68bfb51974aa67");

	private static readonly AssetReference VO_BOM_08_003_Male_Tauren_Guff_InGame_HE_Custom_If_GuffCrabby_InPlay_01_A = new AssetReference("VO_BOM_08_003_Male_Tauren_Guff_InGame_HE_Custom_If_GuffCrabby_InPlay_01_A.prefab:a6a0da91e6c0d5e4e9e51cfd59a50b16");

	private static readonly AssetReference VO_BOM_08_003_Male_Tauren_Guff_InGame_HE_Custom_If_GuffCrabby_InPlay_01_C = new AssetReference("VO_BOM_08_003_Male_Tauren_Guff_InGame_HE_Custom_If_GuffCrabby_InPlay_01_C.prefab:ac13afa637c12e84baa2afb491d164fa");

	private static readonly AssetReference VO_BOM_08_003_Male_Tauren_Guff_InGame_Victory_PreExplosion_01_C = new AssetReference("VO_BOM_08_003_Male_Tauren_Guff_InGame_Victory_PreExplosion_01_C.prefab:40c03c7fbb9f33044b44f0e3ad2a891f");

	private static readonly AssetReference VO_BOM_08_003_Male_Tauren_Guff_UI_AWD_Boss_Reveal_General_07_01 = new AssetReference("VO_BOM_08_003_Male_Tauren_Guff_UI_AWD_Boss_Reveal_General_07_01.prefab:5d8430474c1ddfa408ab9c1a678bb8f2");

	private static readonly AssetReference VO_BOM_08_003_Male_Troll_Brukan_Emote_Attack_01 = new AssetReference("VO_BOM_08_003_Male_Troll_Brukan_Emote_Attack_01.prefab:5dee9bc17cc5b39408c95131e023fb56");

	private static readonly AssetReference VO_BOM_08_003_Male_Troll_Brukan_Emote_Play_01 = new AssetReference("VO_BOM_08_003_Male_Troll_Brukan_Emote_Play_01.prefab:aa1db122c131f514a8d100f2c91c64a2");

	private static readonly AssetReference VO_BOM_08_003_Male_Troll_Brukan_InGame_HE_Custom_BOM_08_Brukan_03t_Play_01 = new AssetReference("VO_BOM_08_003_Male_Troll_Brukan_InGame_HE_Custom_BOM_08_Brukan_03t_Play_01.prefab:11ad97d1ea5a922449d724eb550381a0");

	private static readonly AssetReference VO_BOM_08_003_Male_Troll_Brukan_InGame_HE_Custom_If_CarielBrukan_InPlay_01_B = new AssetReference("VO_BOM_08_003_Male_Troll_Brukan_InGame_HE_Custom_If_CarielBrukan_InPlay_01_B.prefab:6db49b7dfd35d3d48b4bcf8fa55d21d0");

	private static readonly AssetReference VO_BOM_08_003_Male_Troll_Brukan_InGame_HE_Custom_If_CarielBrukan_InPlay_01_C = new AssetReference("VO_BOM_08_003_Male_Troll_Brukan_InGame_HE_Custom_If_CarielBrukan_InPlay_01_C.prefab:44e83e32c26e2fa4a84d364267d93c04");

	private static readonly AssetReference VO_BOM_08_003_Male_Troll_Brukan_inGame_Victory_PreExplosion_01_A = new AssetReference("VO_BOM_08_003_Male_Troll_Brukan_inGame_Victory_PreExplosion_01_A.prefab:89ac8c426e0430a44b2644aff895bc4b");

	private static readonly AssetReference VO_BOM_08_003_X_BloodElf_Dawngrasp_InGame_HE_Custom_BOM_08_Dawngrasp_03t_Play_01 = new AssetReference("VO_BOM_08_003_X_BloodElf_Dawngrasp_InGame_HE_Custom_BOM_08_Dawngrasp_03t_Play_01.prefab:d344b4bbf6be35943975acc5a47daa3f");

	private static readonly AssetReference VO_BOM_08_003_X_BloodElf_Dawngrasp_InGame_HE_Custom_If_ScabbsDawngrasp_InPlay_01_A = new AssetReference("VO_BOM_08_003_X_BloodElf_Dawngrasp_InGame_HE_Custom_If_ScabbsDawngrasp_InPlay_01_A.prefab:55076c845bd3b57468db6c23f2bbe9bf");

	private List<string> m_VO_BOM_08_003_Female_Draenei_Xyrella_InGame_HE_Custom_BOM_08_Xyrella_03t_PlayLines = new List<string>
	{
		VO_BOM_08_003_Female_Draenei_Xyrella_InGame_HE_Custom_BOM_08_Xyrella_03t_Play_01, VO_BOM_08_003_Female_Human_Cariel_InGame_HE_Custom_BOM_08_Cariel_03t_Play_01, VO_BOM_08_003_Female_Human_Cariel_InGame_HE_Custom_If_CarielBrukan_InPlay_01_A, VO_BOM_08_003_Female_Orc_Rokara_InGame_HE_Custom_IfRokaraPlaysPup_01, VO_BOM_08_003_Male_Dwarf_Tavish_InGame_HE_Custom_If_GuffCrabby_InPlay_01_B, VO_BOM_08_003_Male_Gnome_Scabbs_InGame_HE_Custom_BOM_08_Scabbs_03t_Play_01, VO_BOM_08_003_Male_Gnome_Scabbs_InGame_HE_Custom_If_ScabbsDawngrasp_InPlay_01_B, VO_BOM_08_003_Male_NightElf_Kurtrus_InGame_HE_Custom_BOM_08_Kurtrus_03t_Play_01, VO_BOM_08_003_Male_Tauren_Guff_InGame_HE_Custom_BOM_08_Guff_03t_Play_01, VO_BOM_08_003_Male_Tauren_Guff_InGame_HE_Custom_If_GuffCrabby_InPlay_01_A,
		VO_BOM_08_003_Male_Tauren_Guff_InGame_HE_Custom_If_GuffCrabby_InPlay_01_C, VO_BOM_08_003_Male_Troll_Brukan_InGame_HE_Custom_BOM_08_Brukan_03t_Play_01, VO_BOM_08_003_Male_Troll_Brukan_InGame_HE_Custom_If_CarielBrukan_InPlay_01_B, VO_BOM_08_003_Male_Troll_Brukan_InGame_HE_Custom_If_CarielBrukan_InPlay_01_C
	};

	private List<string> m_InGame_BossIdleLines = new List<string> { VO_BOM_08_003_Female_Orc_Rokara_InGame_BossIdle_01, VO_BOM_08_003_Female_Orc_Rokara_InGame_BossIdle_02, VO_BOM_08_003_Female_Orc_Rokara_InGame_BossIdle_03 };

	private List<string> m_InGame_BossUsesHeroPowerLines = new List<string> { VO_BOM_08_003_Female_Orc_Rokara_InGame_BossUsesHeroPower_01, VO_BOM_08_003_Female_Orc_Rokara_InGame_BossUsesHeroPower_02, VO_BOM_08_003_Female_Orc_Rokara_InGame_BossUsesHeroPower_03 };

	private HashSet<string> m_playedLines = new HashSet<string>();

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> m_SoundFiles = new List<string>
		{
			VO_BOM_08_003_Female_Draenei_Xyrella_InGame_HE_Custom_BOM_08_Xyrella_03t_Play_01, VO_BOM_08_003_Female_Human_Cariel_InGame_HE_Custom_BOM_08_Cariel_03t_Play_01, VO_BOM_08_003_Female_Human_Cariel_InGame_HE_Custom_If_CarielBrukan_InPlay_01_A, VO_BOM_08_003_Female_Orc_Rokara_InGame_BossIdle_01, VO_BOM_08_003_Female_Orc_Rokara_InGame_BossIdle_02, VO_BOM_08_003_Female_Orc_Rokara_InGame_BossIdle_03, VO_BOM_08_003_Female_Orc_Rokara_InGame_BossUsesHeroPower_01, VO_BOM_08_003_Female_Orc_Rokara_InGame_BossUsesHeroPower_02, VO_BOM_08_003_Female_Orc_Rokara_InGame_BossUsesHeroPower_03, VO_BOM_08_003_Female_Orc_Rokara_InGame_EmoteResponse_01,
			VO_BOM_08_003_Female_Orc_Rokara_InGame_HE_Custom_IfRokaraPlaysPup_01, VO_BOM_08_003_Female_Orc_Rokara_InGame_Introduction_01_B, VO_BOM_08_003_Female_Orc_Rokara_InGame_PlayerLoss_01, VO_BOM_08_003_Female_Orc_Rokara_InGame_Turn_03_01_A, VO_BOM_08_003_Female_Orc_Rokara_InGame_Turn_07_01_A, VO_BOM_08_003_Female_Orc_Rokara_InGame_Turn_11_01_A, VO_BOM_08_003_Female_Orc_Rokara_InGame_Victory_PreExplosion_01_B, VO_BOM_08_003_Male_Dwarf_Tavish_InGame_HE_Custom_If_GuffCrabby_InPlay_01_B, VO_BOM_08_003_Male_Dwarf_Tavish_InGame_Introduction_01_A, VO_BOM_08_003_Male_Dwarf_Tavish_InGame_Turn_03_01_B,
			VO_BOM_08_003_Male_Dwarf_Tavish_InGame_Turn_07_01_B, VO_BOM_08_003_Male_Dwarf_Tavish_InGame_Turn_07_01_C, VO_BOM_08_003_Male_Dwarf_Tavish_InGame_Turn_11_01_B, VO_BOM_08_003_Male_Gnome_Scabbs_InGame_HE_Custom_BOM_08_Scabbs_03t_Play_01, VO_BOM_08_003_Male_Gnome_Scabbs_InGame_HE_Custom_If_ScabbsDawngrasp_InPlay_01_B, VO_BOM_08_003_Male_NightElf_Kurtrus_InGame_HE_Custom_BOM_08_Kurtrus_03t_Play_01, VO_BOM_08_003_Male_Tauren_Guff_InGame_HE_Custom_BOM_08_Guff_03t_Play_01, VO_BOM_08_003_Male_Tauren_Guff_InGame_HE_Custom_If_GuffCrabby_InPlay_01_A, VO_BOM_08_003_Male_Tauren_Guff_InGame_HE_Custom_If_GuffCrabby_InPlay_01_C, VO_BOM_08_003_Male_Tauren_Guff_InGame_Victory_PreExplosion_01_C,
			VO_BOM_08_003_Male_Troll_Brukan_InGame_HE_Custom_BOM_08_Brukan_03t_Play_01, VO_BOM_08_003_Male_Troll_Brukan_InGame_HE_Custom_If_CarielBrukan_InPlay_01_B, VO_BOM_08_003_Male_Troll_Brukan_InGame_HE_Custom_If_CarielBrukan_InPlay_01_C, VO_BOM_08_003_Male_Troll_Brukan_inGame_Victory_PreExplosion_01_A, VO_BOM_08_003_X_BloodElf_Dawngrasp_InGame_HE_Custom_BOM_08_Dawngrasp_03t_Play_01, VO_BOM_08_003_X_BloodElf_Dawngrasp_InGame_HE_Custom_If_ScabbsDawngrasp_InPlay_01_A
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
		m_OverrideMusicTrack = MusicPlaylistType.InGame_AV_TavishBOM;
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
		case 517:
			yield return MissionPlayVO(enemyActor, m_InGame_BossIdleLines);
			break;
		case 510:
			yield return MissionPlayVO(enemyActor, m_InGame_BossUsesHeroPowerLines);
			break;
		case 515:
			yield return MissionPlayVO(enemyActor, VO_BOM_08_003_Female_Orc_Rokara_InGame_EmoteResponse_01);
			break;
		case 514:
			yield return MissionPlayVO(friendlyActor, VO_BOM_08_003_Male_Dwarf_Tavish_InGame_Introduction_01_A);
			yield return MissionPlayVO(enemyActor, VO_BOM_08_003_Female_Orc_Rokara_InGame_Introduction_01_B);
			break;
		case 506:
			yield return MissionPlayVO(enemyActor, VO_BOM_08_003_Female_Orc_Rokara_InGame_PlayerLoss_01);
			break;
		case 504:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO("BOM_08_Brukan_003t", VO_BOM_08_003_Male_Troll_Brukan_inGame_Victory_PreExplosion_01_A);
			yield return MissionPlayVO(enemyActor, VO_BOM_08_003_Female_Orc_Rokara_InGame_Victory_PreExplosion_01_B);
			yield return MissionPlayVO("BOM_08_Guff_003t", VO_BOM_08_003_Male_Tauren_Guff_InGame_Victory_PreExplosion_01_C);
			GameState.Get().SetBusy(busy: false);
			break;
		default:
			yield return base.HandleMissionEventWithTiming(missionEvent);
			break;
		case 516:
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
			yield return MissionPlayVO(enemyActor, VO_BOM_08_003_Female_Orc_Rokara_InGame_Turn_03_01_A);
			yield return MissionPlayVO(friendlyActor, VO_BOM_08_003_Male_Dwarf_Tavish_InGame_Turn_03_01_B);
			break;
		case 7:
			yield return MissionPlayVO(enemyActor, VO_BOM_08_003_Female_Orc_Rokara_InGame_Turn_07_01_A);
			yield return MissionPlayVO(friendlyActor, VO_BOM_08_003_Male_Dwarf_Tavish_InGame_Turn_07_01_B);
			yield return MissionPlayVO(friendlyActor, VO_BOM_08_003_Male_Dwarf_Tavish_InGame_Turn_07_01_C);
			break;
		case 11:
			yield return MissionPlayVO(enemyActor, VO_BOM_08_003_Female_Orc_Rokara_InGame_Turn_11_01_A);
			yield return MissionPlayVO(friendlyActor, VO_BOM_08_003_Male_Dwarf_Tavish_InGame_Turn_11_01_B);
			break;
		case 15:
			yield return MissionPlayVO("BOM_08_Dawngrasp_003t", VO_BOM_08_003_X_BloodElf_Dawngrasp_InGame_HE_Custom_If_ScabbsDawngrasp_InPlay_01_A);
			yield return MissionPlayVO("BOM_08_Scabbs_003t", VO_BOM_08_003_Male_Gnome_Scabbs_InGame_HE_Custom_If_ScabbsDawngrasp_InPlay_01_B);
			break;
		case 19:
			yield return MissionPlayVO("BOM_08_Cariel_003t", VO_BOM_08_003_Female_Human_Cariel_InGame_HE_Custom_If_CarielBrukan_InPlay_01_A);
			yield return MissionPlayVO(BOM_08_Tavish_Dungeon.Brukan_20_4_BrassRing_Quote, VO_BOM_08_003_Male_Troll_Brukan_InGame_HE_Custom_If_CarielBrukan_InPlay_01_B);
			yield return MissionPlayVO(BOM_08_Tavish_Dungeon.Brukan_20_4_BrassRing_Quote, VO_BOM_08_003_Male_Troll_Brukan_InGame_HE_Custom_If_CarielBrukan_InPlay_01_C);
			break;
		case 21:
			yield return MissionPlayVO("BOM_08_Guff_003t", VO_BOM_08_003_Male_Tauren_Guff_InGame_HE_Custom_If_GuffCrabby_InPlay_01_A);
			yield return MissionPlayVO(friendlyActor, VO_BOM_08_003_Male_Dwarf_Tavish_InGame_HE_Custom_If_GuffCrabby_InPlay_01_B);
			yield return MissionPlayVO("BOM_08_Guff_003t", VO_BOM_08_003_Male_Tauren_Guff_InGame_HE_Custom_If_GuffCrabby_InPlay_01_C);
			break;
		}
	}
}
