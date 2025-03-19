using System.Collections;
using System.Collections.Generic;

public class BOM_08_Tavish_Fight_002 : BOM_08_Tavish_Dungeon
{
	private static readonly AssetReference VO_BOM_08_002_Female_Draenei_Xyrella_InGame_HE_Custom_BOM_08_Boss_02HPSelect_01_01 = new AssetReference("VO_BOM_08_002_Female_Draenei_Xyrella_InGame_HE_Custom_BOM_08_Boss_02HPSelect_01_01.prefab:85d2eb810befc324aa02a88215f19f09");

	private static readonly AssetReference VO_BOM_08_002_Female_Draenei_Xyrella_InGame_Turn_03_01_A = new AssetReference("VO_BOM_08_002_Female_Draenei_Xyrella_InGame_Turn_03_01_A.prefab:a8913722961056449b3ea3c25cdd051a");

	private static readonly AssetReference VO_BOM_08_002_Female_Draenei_Xyrella_InGame_Turn_11_01_A = new AssetReference("VO_BOM_08_002_Female_Draenei_Xyrella_InGame_Turn_11_01_A.prefab:35f68956ece23f642a11fbd73050cb3a");

	private static readonly AssetReference VO_BOM_08_002_Female_Draenei_Xyrella_UI_AWD_Boss_Reveal_General_08_01_B = new AssetReference("VO_BOM_08_002_Female_Draenei_Xyrella_UI_AWD_Boss_Reveal_General_08_01_B.prefab:cca59406c5cfe9f4198c4b27f242861d");

	private static readonly AssetReference VO_BOM_08_002_Female_Human_Cariel_InGame_Turn_03_01_A = new AssetReference("VO_BOM_08_002_Female_Human_Cariel_InGame_Turn_03_01_A.prefab:2fed22c6fe8d6cb44a4914bcf2c837a6");

	private static readonly AssetReference VO_BOM_08_002_Female_Human_Cariel_InGame_Turn_03_01_B = new AssetReference("VO_BOM_08_002_Female_Human_Cariel_InGame_Turn_03_01_B.prefab:07d453ca1710e644e8ea54b837bba8d9");

	private static readonly AssetReference VO_BOM_08_002_Female_Human_Cariel_InGame_Turn_07_01_B = new AssetReference("VO_BOM_08_002_Female_Human_Cariel_InGame_Turn_07_01_B.prefab:3ba865429b778884d91dd22cdf877ed7");

	private static readonly AssetReference VO_BOM_08_002_Female_Human_Cariel_InGame_Turn_11_01_C = new AssetReference("VO_BOM_08_002_Female_Human_Cariel_InGame_Turn_11_01_C.prefab:e6751fd0640730043865dea75f3dbdc6");

	private static readonly AssetReference VO_BOM_08_002_Female_Human_Cariel_UI_AWD_Boss_Reveal_General_06_01_C = new AssetReference("VO_BOM_08_002_Female_Human_Cariel_UI_AWD_Boss_Reveal_General_06_01_C.prefab:4a5e6015d5bb50c4582dc62664689c5a");

	private static readonly AssetReference VO_BOM_08_002_Male_Dwarf_Tavish_InGame_Introduction_01_A = new AssetReference("VO_BOM_08_002_Male_Dwarf_Tavish_InGame_Introduction_01_A.prefab:8783b5f98b770b045b6489950daa7f16");

	private static readonly AssetReference VO_BOM_08_002_Male_Dwarf_Tavish_InGame_Introduction_01_C = new AssetReference("VO_BOM_08_002_Male_Dwarf_Tavish_InGame_Introduction_01_C.prefab:84c1b8e1543aee641b311b0297250a3f");

	private static readonly AssetReference VO_BOM_08_002_Male_Dwarf_Tavish_InGame_Turn_03_01_B = new AssetReference("VO_BOM_08_002_Male_Dwarf_Tavish_InGame_Turn_03_01_B.prefab:a9cbada2873dcaa428c0fa49b33d2548");

	private static readonly AssetReference VO_BOM_08_002_Male_Dwarf_Tavish_InGame_Turn_07_01_A = new AssetReference("VO_BOM_08_002_Male_Dwarf_Tavish_InGame_Turn_07_01_A.prefab:ddb64e69a97e16548b1fa761d4b47380");

	private static readonly AssetReference VO_BOM_08_002_Male_Dwarf_Tavish_InGame_Turn_07_01_B = new AssetReference("VO_BOM_08_002_Male_Dwarf_Tavish_InGame_Turn_07_01_B.prefab:a3b1a33e5ab5c26428f9def2546d94c6");

	private static readonly AssetReference VO_BOM_08_002_Male_Dwarf_Tavish_InGame_VictoryPostExplosion_01_A = new AssetReference("VO_BOM_08_002_Male_Dwarf_Tavish_InGame_VictoryPostExplosion_01_A.prefab:b6246ef31936c784282363f5381aeeb0");

	private static readonly AssetReference VO_BOM_08_002_Male_Dwarf_Tavish_UI_AWD_Boss_Reveal_General_02_01_B = new AssetReference("VO_BOM_08_002_Male_Dwarf_Tavish_UI_AWD_Boss_Reveal_General_02_01_B.prefab:0fff1aefa8fe5c342956da674faabdfc");

	private static readonly AssetReference VO_BOM_08_002_Male_Dwarf_Tavish_UI_AWD_Boss_Reveal_General_02_01_C = new AssetReference("VO_BOM_08_002_Male_Dwarf_Tavish_UI_AWD_Boss_Reveal_General_02_01_C.prefab:44c4d0798365d6a4fa6f2ff2de1f2ac5");

	private static readonly AssetReference VO_BOM_08_002_Male_Gnome_Scabbs_InGame_Turn_07_01_A = new AssetReference("VO_BOM_08_002_Male_Gnome_Scabbs_InGame_Turn_07_01_A.prefab:cc203a32087395b4db4bc2b20075176b");

	private static readonly AssetReference VO_BOM_08_002_Male_Gnome_Scabbs_InGame_Turn_11_01_B = new AssetReference("VO_BOM_08_002_Male_Gnome_Scabbs_InGame_Turn_11_01_B.prefab:002f53dbc22ee0c4bbd014d1c6a8dbb8");

	private static readonly AssetReference VO_BOM_08_002_Male_Gnome_Scabbs_UI_AWD_Boss_Reveal_General_01_01_B = new AssetReference("VO_BOM_08_002_Male_Gnome_Scabbs_UI_AWD_Boss_Reveal_General_01_01_B.prefab:32b4ce8a12f3cc14c9f830d0fa24c247");

	private static readonly AssetReference VO_BOM_08_002_Male_NightElf_Kurtrus_InGame_HE_Custom_BOM_08_Boss_02HPSelect_02_01 = new AssetReference("VO_BOM_08_002_Male_NightElf_Kurtrus_InGame_HE_Custom_BOM_08_Boss_02 HPSelect_02_01.prefab:9db415d46093cf243808aab1ecc61a17");

	private static readonly AssetReference VO_BOM_08_002_Male_NightElf_Kurtrus_InGame_Turn_11_01_A = new AssetReference("VO_BOM_08_002_Male_NightElf_Kurtrus_InGame_Turn_11_01_A.prefab:fe8820b453b31c549ac75610b3c63ff9");

	private static readonly AssetReference VO_BOM_08_002_Male_NightElf_Kurtrus_InGame_Turn_11_01_B = new AssetReference("VO_BOM_08_002_Male_NightElf_Kurtrus_InGame_Turn_11_01_B.prefab:2a24ac9efb2a1a745915ec3776b68d33");

	private static readonly AssetReference VO_BOM_08_002_Male_Trogg_Morloch_InGame_BossDeath_01 = new AssetReference("VO_BOM_08_002_Male_Trogg_Morloch_InGame_BossDeath_01.prefab:f0ea84472f1cd6c4ebddb31020ce6d79");

	private static readonly AssetReference VO_BOM_08_002_Male_Trogg_Morloch_InGame_BossIdle_01 = new AssetReference("VO_BOM_08_002_Male_Trogg_Morloch_InGame_BossIdle_01.prefab:8524adb7333186941924ede8b1351864");

	private static readonly AssetReference VO_BOM_08_002_Male_Trogg_Morloch_InGame_BossIdle_02 = new AssetReference("VO_BOM_08_002_Male_Trogg_Morloch_InGame_BossIdle_02.prefab:b8b7871154f09234784fcb9ff0bdd8fb");

	private static readonly AssetReference VO_BOM_08_002_Male_Trogg_Morloch_InGame_BossIdle_03 = new AssetReference("VO_BOM_08_002_Male_Trogg_Morloch_InGame_BossIdle_03.prefab:ebe17c21eb66c1d49a1f3341a05abf1e");

	private static readonly AssetReference VO_BOM_08_002_Male_Trogg_Morloch_InGame_BossUsesHeroPower_01 = new AssetReference("VO_BOM_08_002_Male_Trogg_Morloch_InGame_BossUsesHeroPower_01.prefab:1020e5d9886a9c649a3fe40d74436edb");

	private static readonly AssetReference VO_BOM_08_002_Male_Trogg_Morloch_InGame_BossUsesHeroPower_02 = new AssetReference("VO_BOM_08_002_Male_Trogg_Morloch_InGame_BossUsesHeroPower_02.prefab:2d84febac48c314409fb455c0c90ce48");

	private static readonly AssetReference VO_BOM_08_002_Male_Trogg_Morloch_InGame_BossUsesHeroPower_03 = new AssetReference("VO_BOM_08_002_Male_Trogg_Morloch_InGame_BossUsesHeroPower_03.prefab:7aaa3d554f1fb9748b8f3962befa702f");

	private static readonly AssetReference VO_BOM_08_002_Male_Trogg_Morloch_InGame_EmoteResponse_01 = new AssetReference("VO_BOM_08_002_Male_Trogg_Morloch_InGame_EmoteResponse_01.prefab:3bc79c707e49109448e1b4b7324dcc98");

	private static readonly AssetReference VO_BOM_08_002_Male_Trogg_Morloch_InGame_Introduction_01_B = new AssetReference("VO_BOM_08_002_Male_Trogg_Morloch_InGame_Introduction_01_B.prefab:fb6308a2795abbc45aa241fdf18bcc40");

	private static readonly AssetReference VO_BOM_08_002_Male_Trogg_Morloch_InGame_PlayerLoss_01 = new AssetReference("VO_BOM_08_002_Male_Trogg_Morloch_InGame_PlayerLoss_01.prefab:36a23abb249f8f6489b91f1c41d5742a");

	private List<string> m_InGame_BossIdleLines = new List<string> { VO_BOM_08_002_Male_Trogg_Morloch_InGame_BossIdle_01, VO_BOM_08_002_Male_Trogg_Morloch_InGame_BossIdle_02, VO_BOM_08_002_Male_Trogg_Morloch_InGame_BossIdle_03 };

	private List<string> m_InGame_BossUsesHeroPowerLines = new List<string> { VO_BOM_08_002_Male_Trogg_Morloch_InGame_BossUsesHeroPower_01, VO_BOM_08_002_Male_Trogg_Morloch_InGame_BossUsesHeroPower_02, VO_BOM_08_002_Male_Trogg_Morloch_InGame_BossUsesHeroPower_03 };

	private HashSet<string> m_playedLines = new HashSet<string>();

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> m_SoundFiles = new List<string>
		{
			VO_BOM_08_002_Female_Draenei_Xyrella_InGame_HE_Custom_BOM_08_Boss_02HPSelect_01_01, VO_BOM_08_002_Female_Draenei_Xyrella_InGame_Turn_03_01_A, VO_BOM_08_002_Female_Draenei_Xyrella_InGame_Turn_11_01_A, VO_BOM_08_002_Female_Human_Cariel_InGame_Turn_03_01_A, VO_BOM_08_002_Female_Human_Cariel_InGame_Turn_03_01_B, VO_BOM_08_002_Female_Human_Cariel_InGame_Turn_07_01_B, VO_BOM_08_002_Female_Human_Cariel_InGame_Turn_11_01_C, VO_BOM_08_002_Male_Dwarf_Tavish_InGame_Introduction_01_A, VO_BOM_08_002_Male_Dwarf_Tavish_InGame_Introduction_01_C, VO_BOM_08_002_Male_Dwarf_Tavish_InGame_Turn_03_01_B,
			VO_BOM_08_002_Male_Dwarf_Tavish_InGame_Turn_07_01_A, VO_BOM_08_002_Male_Dwarf_Tavish_InGame_Turn_07_01_B, VO_BOM_08_002_Male_Dwarf_Tavish_InGame_VictoryPostExplosion_01_A, VO_BOM_08_002_Male_Gnome_Scabbs_InGame_Turn_07_01_A, VO_BOM_08_002_Male_Gnome_Scabbs_InGame_Turn_11_01_B, VO_BOM_08_002_Male_NightElf_Kurtrus_InGame_HE_Custom_BOM_08_Boss_02HPSelect_02_01, VO_BOM_08_002_Male_NightElf_Kurtrus_InGame_Turn_11_01_A, VO_BOM_08_002_Male_NightElf_Kurtrus_InGame_Turn_11_01_B, VO_BOM_08_002_Male_Trogg_Morloch_InGame_BossDeath_01, VO_BOM_08_002_Male_Trogg_Morloch_InGame_BossIdle_01,
			VO_BOM_08_002_Male_Trogg_Morloch_InGame_BossIdle_02, VO_BOM_08_002_Male_Trogg_Morloch_InGame_BossIdle_03, VO_BOM_08_002_Male_Trogg_Morloch_InGame_BossUsesHeroPower_01, VO_BOM_08_002_Male_Trogg_Morloch_InGame_BossUsesHeroPower_02, VO_BOM_08_002_Male_Trogg_Morloch_InGame_BossUsesHeroPower_03, VO_BOM_08_002_Male_Trogg_Morloch_InGame_EmoteResponse_01, VO_BOM_08_002_Male_Trogg_Morloch_InGame_Introduction_01_B, VO_BOM_08_002_Male_Trogg_Morloch_InGame_PlayerLoss_01
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
		case 516:
			yield return MissionPlaySound(enemyActor, VO_BOM_08_002_Male_Trogg_Morloch_InGame_BossDeath_01);
			break;
		case 517:
			yield return MissionPlayVO(enemyActor, m_InGame_BossIdleLines);
			break;
		case 510:
			yield return MissionPlayVO(enemyActor, m_InGame_BossUsesHeroPowerLines);
			break;
		case 515:
			yield return MissionPlayVO(enemyActor, VO_BOM_08_002_Male_Trogg_Morloch_InGame_EmoteResponse_01);
			break;
		case 514:
			yield return MissionPlayVO(friendlyActor, VO_BOM_08_002_Male_Dwarf_Tavish_InGame_Introduction_01_A);
			yield return MissionPlayVO(enemyActor, VO_BOM_08_002_Male_Trogg_Morloch_InGame_Introduction_01_B);
			yield return MissionPlayVO(friendlyActor, VO_BOM_08_002_Male_Dwarf_Tavish_InGame_Introduction_01_C);
			break;
		case 506:
			yield return MissionPlayVO(enemyActor, VO_BOM_08_002_Male_Trogg_Morloch_InGame_PlayerLoss_01);
			break;
		case 505:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(friendlyActor, VO_BOM_08_002_Male_Dwarf_Tavish_InGame_VictoryPostExplosion_01_A);
			GameState.Get().SetBusy(busy: false);
			break;
		case 1001:
			if (FindActorInPlayByDesignCode("BOM_08_Xyrella_002t") != null)
			{
				yield return MissionPlayVO("BOM_08_Xyrella_002t", VO_BOM_08_002_Female_Draenei_Xyrella_InGame_HE_Custom_BOM_08_Boss_02HPSelect_01_01);
			}
			if (FindActorInPlayByDesignCode("BOM_08_Cariel_002t") != null)
			{
				yield return MissionPlayVO("BOM_08_Kurtrus_002t", VO_BOM_08_002_Male_NightElf_Kurtrus_InGame_HE_Custom_BOM_08_Boss_02HPSelect_02_01);
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
		switch (turn)
		{
		case 3:
			if (FindActorInPlayByDesignCode("BOM_08_Xyrella_002t") != null)
			{
				yield return MissionPlayVO("BOM_08_Xyrella_002t", VO_BOM_08_002_Female_Draenei_Xyrella_InGame_Turn_03_01_A);
				yield return MissionPlayVO(friendlyActor, VO_BOM_08_002_Male_Dwarf_Tavish_InGame_Turn_03_01_B);
			}
			if (FindActorInPlayByDesignCode("BOM_08_Cariel_002t") != null)
			{
				yield return MissionPlayVO("BOM_08_Cariel_002t", VO_BOM_08_002_Female_Human_Cariel_InGame_Turn_03_01_A);
				yield return MissionPlayVO("BOM_08_Cariel_002t", VO_BOM_08_002_Female_Human_Cariel_InGame_Turn_03_01_B);
			}
			break;
		case 7:
			if (FindActorInPlayByDesignCode("BOM_08_Xyrella_002t") != null)
			{
				yield return MissionPlayVO("BOM_08_Scabbs_002t", VO_BOM_08_002_Male_Gnome_Scabbs_InGame_Turn_07_01_A);
				yield return MissionPlayVO(friendlyActor, VO_BOM_08_002_Male_Dwarf_Tavish_InGame_Turn_07_01_B);
			}
			if (FindActorInPlayByDesignCode("BOM_08_Cariel_002t") != null)
			{
				yield return MissionPlayVO(friendlyActor, VO_BOM_08_002_Male_Dwarf_Tavish_InGame_Turn_07_01_A);
				yield return MissionPlayVO("BOM_08_Cariel_002t", VO_BOM_08_002_Female_Human_Cariel_InGame_Turn_07_01_B);
			}
			break;
		case 11:
			if (FindActorInPlayByDesignCode("BOM_08_Xyrella_002t") != null)
			{
				yield return MissionPlayVO("BOM_08_Xyrella_002t", VO_BOM_08_002_Female_Draenei_Xyrella_InGame_Turn_11_01_A);
				yield return MissionPlayVO("BOM_08_Scabbs_002t", VO_BOM_08_002_Male_Gnome_Scabbs_InGame_Turn_11_01_B);
			}
			if (FindActorInPlayByDesignCode("BOM_08_Cariel_002t") != null)
			{
				yield return MissionPlayVO("BOM_08_Kurtrus_002t", VO_BOM_08_002_Male_NightElf_Kurtrus_InGame_Turn_11_01_A);
				yield return MissionPlayVO("BOM_08_Kurtrus_002t", VO_BOM_08_002_Male_NightElf_Kurtrus_InGame_Turn_11_01_B);
				yield return MissionPlayVO("BOM_08_Cariel_002t", VO_BOM_08_002_Female_Human_Cariel_InGame_Turn_11_01_C);
			}
			break;
		}
	}
}
