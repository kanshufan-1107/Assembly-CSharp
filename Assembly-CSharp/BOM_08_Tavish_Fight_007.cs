using System.Collections;
using System.Collections.Generic;

public class BOM_08_Tavish_Fight_007 : BOM_08_Tavish_Dungeon
{
	private static readonly AssetReference VO_BOM_08_007_Female_Draenei_Xyrella_InGame_HE_Custom_IfXyrellaPlayed_01_B = new AssetReference("VO_BOM_08_007_Female_Draenei_Xyrella_InGame_HE_Custom_IfXyrellaPlayed_01_B.prefab:9393d4e99a9bbec4f8ddfbfb90bd7ab8");

	private static readonly AssetReference VO_BOM_08_007_Female_Orc_Rokara_InGame_HE_Custom_IfRokaraFreed_01_A = new AssetReference("VO_BOM_08_007_Female_Orc_Rokara_InGame_HE_Custom_IfRokaraFreed_01_A.prefab:ae9459726c5d7d348af36297266e3058");

	private static readonly AssetReference VO_BOM_08_007_Male_Dragon_Kazakusan_InGame_Victory_PreExplosion_01_C = new AssetReference("VO_BOM_08_007_Male_Dragon_Kazakusan_InGame_Victory_PreExplosion_01_C.prefab:3ec5cbcfbc6cbb2499b38dd4e2794463");

	private static readonly AssetReference VO_BOM_08_007_Male_Dwarf_Tavish_InGame_HE_Custom_IfRokaraFreed_01_B = new AssetReference("VO_BOM_08_007_Male_Dwarf_Tavish_InGame_HE_Custom_IfRokaraFreed_01_B.prefab:0233e6c172f26a44cac187bd3b5e8cb7");

	private static readonly AssetReference VO_BOM_08_007_Male_Dwarf_Tavish_InGame_Introduction_01_A = new AssetReference("VO_BOM_08_007_Male_Dwarf_Tavish_InGame_Introduction_01_A.prefab:5e98ab3c77d39214aaf450d62625c25a");

	private static readonly AssetReference VO_BOM_08_007_Male_Dwarf_Tavish_InGame_Turn_09_01_A = new AssetReference("VO_BOM_08_007_Male_Dwarf_Tavish_InGame_Turn_09_01_A.prefab:624de724b52c3f047a3871cfef306598");

	private static readonly AssetReference VO_BOM_08_007_Male_Dwarf_Tavish_InGame_Victory_PreExplosion_01_A = new AssetReference("VO_BOM_08_007_Male_Dwarf_Tavish_InGame_Victory_PreExplosion_01_A.prefab:4ed9dfbddb235ac43aad7254efe1edca");

	private static readonly AssetReference VO_BOM_08_007_Male_Dwarf_Tavish_UI_AWD_Boss_Reveal_General_07_01 = new AssetReference("VO_BOM_08_007_Male_Dwarf_Tavish_UI_AWD_Boss_Reveal_General_07_01.prefab:b7f4db9f2e83db34fa2967b97c5d9821");

	private static readonly AssetReference VO_BOM_08_007_Male_Dwarf_Tavish_UI_AWD_Boss_Reveal_General_07_02 = new AssetReference("VO_BOM_08_007_Male_Dwarf_Tavish_UI_AWD_Boss_Reveal_General_07_02.prefab:6112550293a4c2f4f8b53ff66ac87fd2");

	private static readonly AssetReference VO_BOM_08_007_Male_Gnome_Scabbs_InGame_HE_Custom_IfScabbsPlayed_01 = new AssetReference("VO_BOM_08_007_Male_Gnome_Scabbs_InGame_HE_Custom_IfScabbsPlayed_01.prefab:ce54d74613770ca4a9e09f844d02098b");

	private static readonly AssetReference VO_BOM_08_007_Male_NightElf_Kurtrus_InGame_HE_Custom_IfKurtrusPlayed_01 = new AssetReference("VO_BOM_08_007_Male_NightElf_Kurtrus_InGame_HE_Custom_IfKurtrusPlayed_01.prefab:5ac39690471f2294389f2bd9c32f2cd3");

	private static readonly AssetReference VO_BOM_08_007_Male_Tauren_Guff_InGame_Turn_01_01 = new AssetReference("VO_BOM_08_007_Male_Tauren_Guff_InGame_Turn_01_01.prefab:460124dfb2415704394c18ba910bd32a");

	private static readonly AssetReference VO_BOM_08_007_Male_Tauren_Guff_InGame_Turn_09_01_B = new AssetReference("VO_BOM_08_007_Male_Tauren_Guff_InGame_Turn_09_01_B.prefab:86b196f38505642438d7cd09b2cc55db");

	private static readonly AssetReference VO_BOM_08_007_Male_Troll_Brukan_InGame_HE_Custom_IfBrukanFreed_01_A = new AssetReference("VO_BOM_08_007_Male_Troll_Brukan_InGame_HE_Custom_IfBrukanFreed_01_A.prefab:662ee1de3b339a2488359f4543fd0849");

	private static readonly AssetReference VO_BOM_08_007_Male_Troll_Kazakus_InGame_BossIdle_01 = new AssetReference("VO_BOM_08_007_Male_Troll_Kazakus_InGame_BossIdle_01.prefab:fe7f749a926087340b0a00248823dc35");

	private static readonly AssetReference VO_BOM_08_007_Male_Troll_Kazakus_InGame_BossIdle_02 = new AssetReference("VO_BOM_08_007_Male_Troll_Kazakus_InGame_BossIdle_02.prefab:66dea27d42762f748a99404c051e3dce");

	private static readonly AssetReference VO_BOM_08_007_Male_Troll_Kazakus_InGame_BossIdle_03 = new AssetReference("VO_BOM_08_007_Male_Troll_Kazakus_InGame_BossIdle_03.prefab:ab70ea6849d14e94a8109fdfbe2aa676");

	private static readonly AssetReference VO_BOM_08_007_Male_Troll_Kazakus_InGame_BossUsesHeroPower_01 = new AssetReference("VO_BOM_08_007_Male_Troll_Kazakus_InGame_BossUsesHeroPower_01.prefab:6e496575d2fe4054ba8b0d0ac607f8d6");

	private static readonly AssetReference VO_BOM_08_007_Male_Troll_Kazakus_InGame_BossUsesHeroPower_02 = new AssetReference("VO_BOM_08_007_Male_Troll_Kazakus_InGame_BossUsesHeroPower_02.prefab:495ca1fece6269644b51a426f680c982");

	private static readonly AssetReference VO_BOM_08_007_Male_Troll_Kazakus_InGame_BossUsesHeroPower_03 = new AssetReference("VO_BOM_08_007_Male_Troll_Kazakus_InGame_BossUsesHeroPower_03.prefab:642c6be607eaff14ba3982d04f32d31b");

	private static readonly AssetReference VO_BOM_08_007_Male_Troll_Kazakus_InGame_EmoteResponse_01 = new AssetReference("VO_BOM_08_007_Male_Troll_Kazakus_InGame_EmoteResponse_01.prefab:8480e61a8b4ed984dadf1ded17e6b2a6");

	private static readonly AssetReference VO_BOM_08_007_Male_Troll_Kazakus_InGame_HE_Custom_IfXyrellaPlayed_01_A = new AssetReference("VO_BOM_08_007_Male_Troll_Kazakus_InGame_HE_Custom_IfXyrellaPlayed_01_A.prefab:71ea7228018a7294aaab491254d995d3");

	private static readonly AssetReference VO_BOM_08_007_Male_Troll_Kazakus_InGame_Introduction_01_B = new AssetReference("VO_BOM_08_007_Male_Troll_Kazakus_InGame_Introduction_01_B.prefab:151dd32e1c9ba4d48922a963336322eb");

	private static readonly AssetReference VO_BOM_08_007_Male_Troll_Kazakus_InGame_PlayerLoss_01 = new AssetReference("VO_BOM_08_007_Male_Troll_Kazakus_InGame_PlayerLoss_01.prefab:6f4fb4f5cf8b2bb41912c75986c1a829");

	private static readonly AssetReference VO_BOM_08_007_Male_Troll_Kazakus_InGame_Turn_05_01 = new AssetReference("VO_BOM_08_007_Male_Troll_Kazakus_InGame_Turn_05_01.prefab:5b3c6d8a9603c3c48af970df399927ac");

	private static readonly AssetReference VO_BOM_08_007_Male_Troll_Kazakus_InGame_Turn_13_01 = new AssetReference("VO_BOM_08_007_Male_Troll_Kazakus_InGame_Turn_13_01.prefab:f70a20c501d28e4488ee59b144ebdaf6");

	private static readonly AssetReference VO_BOM_08_007_Male_Troll_Kazakus_InGame_Victory_PreExplosion_01_B = new AssetReference("VO_BOM_08_007_Male_Troll_Kazakus_InGame_Victory_PreExplosion_01_B.prefab:fab1403eedc252d46b99ed816615bf7c");

	private static readonly AssetReference VO_BOM_08_007_X_BloodElf_Dawngrasp_InGame_HE_Custom_IfDawngraspFreed_01_A = new AssetReference("VO_BOM_08_007_X_BloodElf_Dawngrasp_InGame_HE_Custom_IfDawngraspFreed_01_A.prefab:840b450ed7bc62f49b975f737e6a19b2");

	private static readonly AssetReference VO_BOM_08_007_Male_Dragon_Kazakusan_InGame_VictoryPreExplosion_01_C = new AssetReference("VO_BOM_08_007_Male_Dragon_Kazakusan_InGame_VictoryPreExplosion_01_C.prefab:fe9e3fcf588b471fabf1cc279474923a");

	private static readonly AssetReference VO_BOM_08_007_Male_Troll_Kazakus_InGame_LossPreExplosion_01 = new AssetReference("VO_BOM_08_007_Male_Troll_Kazakus_InGame_LossPreExplosion_01.prefab:7820ffced6d9492ca1f0794c42d9fc52");

	private static readonly AssetReference VO_BOM_08_007_Male_Troll_Kazakus_InGame_VictoryPreExplosion_01_B = new AssetReference("VO_BOM_08_007_Male_Troll_Kazakus_InGame_VictoryPreExplosion_01_B.prefab:1ed67be94b184bf281f1ebd2d95783f6");

	private static readonly AssetReference VO_BOM_08_X_BloodElf_Dawngrasp_InGame_HE_Custom_IfDawngraspFreed_01_A = new AssetReference("VO_BOM_08_X_BloodElf_Dawngrasp_InGame_HE_Custom_IfDawngraspFreed_01_A.prefab:a3c0d9c425474e1eaa1c5a7c57545a80");

	private List<string> m_InGame_BossIdleLines = new List<string> { VO_BOM_08_007_Male_Troll_Kazakus_InGame_BossIdle_01, VO_BOM_08_007_Male_Troll_Kazakus_InGame_BossIdle_02, VO_BOM_08_007_Male_Troll_Kazakus_InGame_BossIdle_03 };

	private List<string> m_InGame_BossUsesHeroPowerLines = new List<string> { VO_BOM_08_007_Male_Troll_Kazakus_InGame_BossUsesHeroPower_01, VO_BOM_08_007_Male_Troll_Kazakus_InGame_BossUsesHeroPower_02, VO_BOM_08_007_Male_Troll_Kazakus_InGame_BossUsesHeroPower_03 };

	private HashSet<string> m_playedLines = new HashSet<string>();

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> m_SoundFiles = new List<string>
		{
			VO_BOM_08_007_Female_Draenei_Xyrella_InGame_HE_Custom_IfXyrellaPlayed_01_B, VO_BOM_08_007_Female_Orc_Rokara_InGame_HE_Custom_IfRokaraFreed_01_A, VO_BOM_08_007_Male_Dragon_Kazakusan_InGame_Victory_PreExplosion_01_C, VO_BOM_08_007_Male_Dwarf_Tavish_InGame_HE_Custom_IfRokaraFreed_01_B, VO_BOM_08_007_Male_Dwarf_Tavish_InGame_Introduction_01_A, VO_BOM_08_007_Male_Dwarf_Tavish_InGame_Turn_09_01_A, VO_BOM_08_007_Male_Dwarf_Tavish_InGame_Victory_PreExplosion_01_A, VO_BOM_08_007_Male_Gnome_Scabbs_InGame_HE_Custom_IfScabbsPlayed_01, VO_BOM_08_007_Male_NightElf_Kurtrus_InGame_HE_Custom_IfKurtrusPlayed_01, VO_BOM_08_007_Male_Tauren_Guff_InGame_Turn_01_01,
			VO_BOM_08_007_Male_Tauren_Guff_InGame_Turn_09_01_B, VO_BOM_08_007_Male_Troll_Brukan_InGame_HE_Custom_IfBrukanFreed_01_A, VO_BOM_08_007_Male_Troll_Kazakus_InGame_BossIdle_01, VO_BOM_08_007_Male_Troll_Kazakus_InGame_BossIdle_02, VO_BOM_08_007_Male_Troll_Kazakus_InGame_BossIdle_03, VO_BOM_08_007_Male_Troll_Kazakus_InGame_BossUsesHeroPower_01, VO_BOM_08_007_Male_Troll_Kazakus_InGame_BossUsesHeroPower_02, VO_BOM_08_007_Male_Troll_Kazakus_InGame_BossUsesHeroPower_03, VO_BOM_08_007_Male_Troll_Kazakus_InGame_EmoteResponse_01, VO_BOM_08_007_Male_Troll_Kazakus_InGame_HE_Custom_IfXyrellaPlayed_01_A,
			VO_BOM_08_007_Male_Troll_Kazakus_InGame_Introduction_01_B, VO_BOM_08_007_Male_Troll_Kazakus_InGame_PlayerLoss_01, VO_BOM_08_007_Male_Troll_Kazakus_InGame_Turn_05_01, VO_BOM_08_007_Male_Troll_Kazakus_InGame_Turn_13_01, VO_BOM_08_007_Male_Troll_Kazakus_InGame_Victory_PreExplosion_01_B, VO_BOM_08_007_X_BloodElf_Dawngrasp_InGame_HE_Custom_IfDawngraspFreed_01_A
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
		m_OverrideMusicTrack = MusicPlaylistType.InGame_AV;
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
			yield return MissionPlayVO(enemyActor, VO_BOM_08_007_Male_Troll_Kazakus_InGame_EmoteResponse_01);
			break;
		case 514:
			yield return MissionPlayVO(enemyActor, VO_BOM_08_007_Male_Troll_Kazakus_InGame_Introduction_01_B);
			break;
		case 506:
			yield return MissionPlayVO(enemyActor, VO_BOM_08_007_Male_Troll_Kazakus_InGame_PlayerLoss_01);
			break;
		case 504:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(friendlyActor, VO_BOM_08_007_Male_Dwarf_Tavish_InGame_Victory_PreExplosion_01_A);
			yield return MissionPlayVO(enemyActor, VO_BOM_08_007_Male_Troll_Kazakus_InGame_Victory_PreExplosion_01_B);
			yield return MissionPlayVO(enemyActor, VO_BOM_08_007_Male_Dragon_Kazakusan_InGame_Victory_PreExplosion_01_C);
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
			Actor enemyActor = GameState.Get().GetOpposingSidePlayer().GetHero()
				.GetCard()
				.GetActor();
			GameState.Get().GetFriendlySidePlayer().GetHero()
				.GetCard()
				.GetActor();
			switch (cardID)
			{
			case "BOM_08_Xyrella_003t":
				yield return MissionPlayVO(enemyActor, VO_BOM_08_007_Male_Troll_Kazakus_InGame_HE_Custom_IfXyrellaPlayed_01_A);
				yield return MissionPlayVO("BOM_08_Xyrella_003t", VO_BOM_08_007_Female_Draenei_Xyrella_InGame_HE_Custom_IfXyrellaPlayed_01_B);
				break;
			case "BOM_08_Scabbs_003t":
				yield return MissionPlayVO("BOM_08_Scabbs_003t", VO_BOM_08_007_Male_Gnome_Scabbs_InGame_HE_Custom_IfScabbsPlayed_01);
				break;
			case "BOM_08_Kurtrus_003t":
				yield return MissionPlayVO("BOM_08_Kurtrus_003t", VO_BOM_08_007_Male_NightElf_Kurtrus_InGame_HE_Custom_IfKurtrusPlayed_01);
				break;
			}
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
			yield return MissionPlayVO("BOM_08_Guff_003t", VO_BOM_08_007_Male_Tauren_Guff_InGame_Turn_01_01);
			break;
		case 5:
			yield return MissionPlayVO(enemyActor, VO_BOM_08_007_Male_Troll_Kazakus_InGame_Turn_05_01);
			break;
		case 9:
			yield return MissionPlayVO(friendlyActor, VO_BOM_08_007_Male_Dwarf_Tavish_InGame_Turn_09_01_A);
			yield return MissionPlayVO("BOM_08_Guff_003t", VO_BOM_08_007_Male_Tauren_Guff_InGame_Turn_09_01_B);
			break;
		case 13:
			yield return MissionPlayVO(enemyActor, VO_BOM_08_007_Male_Troll_Kazakus_InGame_Turn_13_01);
			break;
		}
	}
}
