using System.Collections;
using System.Collections.Generic;

public class BOM_08_Tavish_Fight_005 : BOM_08_Tavish_Dungeon
{
	private static readonly AssetReference VO_BOM_08_005_Female_Human_Cariel_InGame_Victory_PostExplosion_01_A = new AssetReference("VO_BOM_08_005_Female_Human_Cariel_InGame_Victory_PostExplosion_01_A.prefab:5281380bf9a821f4fa6854ea01410648");

	private static readonly AssetReference VO_BOM_08_005_Male_Dwarf_Tavish_InGame_HE_Custom_FirstTimeBossAt10Health_01_B = new AssetReference("VO_BOM_08_005_Male_Dwarf_Tavish_InGame_HE_Custom_FirstTimeBossAt10Health_01_B.prefab:7666fe9d7a5eef44996e6c0dbb171f0c");

	private static readonly AssetReference VO_BOM_08_005_Male_Dwarf_Tavish_InGame_HE_Custom_FirstTimeScabbsDies_01 = new AssetReference("VO_BOM_08_005_Male_Dwarf_Tavish_InGame_HE_Custom_FirstTimeScabbsDies_01.prefab:bc501d2a1a644784da74c70ebdb278ae");

	private static readonly AssetReference VO_BOM_08_005_Male_Dwarf_Tavish_InGame_HE_Custom_FirstTimeScabbsRez_01 = new AssetReference("VO_BOM_08_005_Male_Dwarf_Tavish_InGame_HE_Custom_FirstTimeScabbsRez_01.prefab:d2ec91f0fe523054ab4d8d84428148ad");

	private static readonly AssetReference VO_BOM_08_005_Male_Dwarf_Tavish_InGame_Introduction_05_01_A = new AssetReference("VO_BOM_08_005_Male_Dwarf_Tavish_InGame_Introduction_05_01_A.prefab:ccc1d688a40b2d84cbb29666bf54e4dc");

	private static readonly AssetReference VO_BOM_08_005_Male_Dwarf_Tavish_InGame_Turn_01_01_A = new AssetReference("VO_BOM_08_005_Male_Dwarf_Tavish_InGame_Turn_01_01_A.prefab:b903e9e467c1d8043ac18db0addfdbbc");

	private static readonly AssetReference VO_BOM_08_005_Male_Dwarf_Tavish_InGame_Turn_07_01_A = new AssetReference("VO_BOM_08_005_Male_Dwarf_Tavish_InGame_Turn_07_01_A.prefab:30721fa7d7d7a3e4aac05a868b590513");

	private static readonly AssetReference VO_BOM_08_005_Male_Dwarf_Tavish_InGame_Turn_07_01_C = new AssetReference("VO_BOM_08_005_Male_Dwarf_Tavish_InGame_Turn_07_01_C.prefab:897d4e461720f824586ee5f5295836bd");

	private static readonly AssetReference VO_BOM_08_005_Male_Dwarf_Tavish_InGame_Turn_09_01_A = new AssetReference("VO_BOM_08_005_Male_Dwarf_Tavish_InGame_Turn_09_01_A.prefab:ea5452af662d89e40acd3d559dba7700");

	private static readonly AssetReference VO_BOM_08_005_Male_Dwarf_Tavish_InGame_Turn_13_01_A = new AssetReference("VO_BOM_08_005_Male_Dwarf_Tavish_InGame_Turn_13_01_A.prefab:c9c1f845197bee44a94f61a45de53699");

	private static readonly AssetReference VO_BOM_08_005_Male_Dwarf_Tavish_InGame_Turn_13_01_C = new AssetReference("VO_BOM_08_005_Male_Dwarf_Tavish_InGame_Turn_13_01_C.prefab:ca8eb562b138b28469ef9209660adaf7");

	private static readonly AssetReference VO_BOM_08_005_Male_Dwarf_Tavish_InGame_Victory_PostExplosion_01_B = new AssetReference("VO_BOM_08_005_Male_Dwarf_Tavish_InGame_Victory_PostExplosion_01_B.prefab:1b54a74261c07444694ef667a4a48fd9");

	private static readonly AssetReference VO_BOM_08_005_Male_Dwarf_Tavish_UI_AWD_Boss_Reveal_General_01_A = new AssetReference("VO_BOM_08_005_Male_Dwarf_Tavish_UI_AWD_Boss_Reveal_General_01_A.prefab:eda4acd0fcbeaa04a8b196383fc13693");

	private static readonly AssetReference VO_BOM_08_005_Male_Dwarf_Tavish_UI_AWD_Boss_Reveal_General_01_B = new AssetReference("VO_BOM_08_005_Male_Dwarf_Tavish_UI_AWD_Boss_Reveal_General_01_B.prefab:eca41fd59bad9654a9621a3ee1116c3e");

	private static readonly AssetReference VO_BOM_08_005_Male_Gnome_Scabbs_InGame_HE_Custom_FirstTimeBossAt10Health_01_A = new AssetReference("VO_BOM_08_005_Male_Gnome_Scabbs_InGame_HE_Custom_FirstTimeBossAt10Health_01_A.prefab:ec66c54f32160a242a98c94849fe9bfd");

	private static readonly AssetReference VO_BOM_08_005_Male_Gnome_Scabbs_InGame_HE_Custom_FirstTimeTavishKills_01 = new AssetReference("VO_BOM_08_005_Male_Gnome_Scabbs_InGame_HE_Custom_FirstTimeTavishKills_01.prefab:509bcd27b0f5ccb459822794ca0f867e");

	private static readonly AssetReference VO_BOM_08_005_Male_Gnome_Scabbs_InGame_HE_Custom_FirstTimeTavishPlaysTrap_01 = new AssetReference("VO_BOM_08_005_Male_Gnome_Scabbs_InGame_HE_Custom_FirstTimeTavishPlaysTrap_01.prefab:d184fa7e195e8b2499fcebf15515ec2f");

	private static readonly AssetReference VO_BOM_08_005_Male_Gnome_Scabbs_InGame_Turn_01_01_B = new AssetReference("VO_BOM_08_005_Male_Gnome_Scabbs_InGame_Turn_01_01_B.prefab:2aaf27772af586049970279b19cfb232");

	private static readonly AssetReference VO_BOM_08_005_Male_Gnome_Scabbs_InGame_Turn_07_01_B = new AssetReference("VO_BOM_08_005_Male_Gnome_Scabbs_InGame_Turn_07_01_B.prefab:b332e7559d7329c4a84c6bb572b34676");

	private static readonly AssetReference VO_BOM_08_005_Male_Gnome_Scabbs_InGame_Turn_09_01_B = new AssetReference("VO_BOM_08_005_Male_Gnome_Scabbs_InGame_Turn_09_01_B.prefab:b474efed8a34d9241b3201f4e8a5cf77");

	private static readonly AssetReference VO_BOM_08_005_Male_Gnome_Scabbs_InGame_Turn_09_01_C = new AssetReference("VO_BOM_08_005_Male_Gnome_Scabbs_InGame_Turn_09_01_C.prefab:245c4b2ebb2c1024b920e505f17c24f4");

	private static readonly AssetReference VO_BOM_08_005_Male_Gnome_Scabbs_InGame_Turn_13_01_B = new AssetReference("VO_BOM_08_005_Male_Gnome_Scabbs_InGame_Turn_13_01_B.prefab:cc4938aba5050cd4e917347df6010279");

	private static readonly AssetReference VO_BOM_08_005_Male_Orc_Galvangar_InGame_BossDeath_01 = new AssetReference("VO_BOM_08_005_Male_Orc_Galvangar_InGame_BossDeath_01.prefab:78a5b8027f353824b92abe21ad1b3394");

	private static readonly AssetReference VO_BOM_08_005_Male_Orc_Galvangar_InGame_BossIdle_01 = new AssetReference("VO_BOM_08_005_Male_Orc_Galvangar_InGame_BossIdle_01.prefab:b92063736bbfd314fae75bfac8ba5bc3");

	private static readonly AssetReference VO_BOM_08_005_Male_Orc_Galvangar_InGame_BossIdle_02 = new AssetReference("VO_BOM_08_005_Male_Orc_Galvangar_InGame_BossIdle_02.prefab:d505b53db39386e439575bc6176646c2");

	private static readonly AssetReference VO_BOM_08_005_Male_Orc_Galvangar_InGame_BossIdle_03 = new AssetReference("VO_BOM_08_005_Male_Orc_Galvangar_InGame_BossIdle_03.prefab:7732f05989fca7840a83ddc57975fefe");

	private static readonly AssetReference VO_BOM_08_005_Male_Orc_Galvangar_InGame_BossUsesHeroPower_01 = new AssetReference("VO_BOM_08_005_Male_Orc_Galvangar_InGame_BossUsesHeroPower_01.prefab:23ddfe9c2d687324680898daf8a58e66");

	private static readonly AssetReference VO_BOM_08_005_Male_Orc_Galvangar_InGame_BossUsesHeroPower_02 = new AssetReference("VO_BOM_08_005_Male_Orc_Galvangar_InGame_BossUsesHeroPower_02.prefab:cc1a7cbafbd0d87448cfa99cd1c699b4");

	private static readonly AssetReference VO_BOM_08_005_Male_Orc_Galvangar_InGame_BossUsesHeroPower_03 = new AssetReference("VO_BOM_08_005_Male_Orc_Galvangar_InGame_BossUsesHeroPower_03.prefab:06050c29e87b9a645b8a0a882892ff90");

	private static readonly AssetReference VO_BOM_08_005_Male_Orc_Galvangar_InGame_EmoteResponse_01 = new AssetReference("VO_BOM_08_005_Male_Orc_Galvangar_InGame_EmoteResponse_01.prefab:ef25f6e32f36c95448994eaebe4b38a3");

	private static readonly AssetReference VO_BOM_08_005_Male_Orc_Galvangar_InGame_Introduction_05_01_B = new AssetReference("VO_BOM_08_005_Male_Orc_Galvangar_InGame_Introduction_05_01_B.prefab:aa7a60e0b9bc84f4c96682925dd5d15e");

	private static readonly AssetReference VO_BOM_08_005_Male_Orc_Galvangar_InGame_PlayerLoss_01 = new AssetReference("VO_BOM_08_005_Male_Orc_Galvangar_InGame_PlayerLoss_01.prefab:1e4e7e8132b10da4fa1d4c5d9fc8bfcd");

	private static readonly AssetReference VO_BOM_08_005_Female_Human_Cariel_InGame_VictoryPostExplosion_01_A = new AssetReference("VO_BOM_08_005_Female_Human_Cariel_InGame_VictoryPostExplosion_01_A.prefab:846897505283404ebec1b2062153bca5");

	private static readonly AssetReference VO_BOM_08_005_Male_Dwarf_Tavish_InGame_VictoryPostExplosion_01_B = new AssetReference("VO_BOM_08_005_Male_Dwarf_Tavish_InGame_VictoryPostExplosion_01_B.prefab:b05a4e49bd6841258d957b57edb6689e");

	private static readonly AssetReference VO_BOM_08_005_Male_Orc_Galvangar_InGame_LossPreExplosion_01 = new AssetReference("VO_BOM_08_005_Male_Orc_Galvangar_InGame_LossPreExplosion_01.prefab:bb81e5e42a274d6185545d34e0411c7b");

	private List<string> m_InGame_BossIdleLines = new List<string> { VO_BOM_08_005_Male_Orc_Galvangar_InGame_BossIdle_01, VO_BOM_08_005_Male_Orc_Galvangar_InGame_BossIdle_02, VO_BOM_08_005_Male_Orc_Galvangar_InGame_BossIdle_03 };

	private List<string> m_InGame_BossUsesHeroPowerLines = new List<string> { VO_BOM_08_005_Male_Orc_Galvangar_InGame_BossUsesHeroPower_01, VO_BOM_08_005_Male_Orc_Galvangar_InGame_BossUsesHeroPower_02, VO_BOM_08_005_Male_Orc_Galvangar_InGame_BossUsesHeroPower_03 };

	private HashSet<string> m_playedLines = new HashSet<string>();

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> m_SoundFiles = new List<string>
		{
			VO_BOM_08_005_Female_Human_Cariel_InGame_Victory_PostExplosion_01_A, VO_BOM_08_005_Male_Dwarf_Tavish_InGame_HE_Custom_FirstTimeBossAt10Health_01_B, VO_BOM_08_005_Male_Dwarf_Tavish_InGame_HE_Custom_FirstTimeScabbsDies_01, VO_BOM_08_005_Male_Dwarf_Tavish_InGame_HE_Custom_FirstTimeScabbsRez_01, VO_BOM_08_005_Male_Dwarf_Tavish_InGame_Introduction_05_01_A, VO_BOM_08_005_Male_Dwarf_Tavish_InGame_Turn_01_01_A, VO_BOM_08_005_Male_Dwarf_Tavish_InGame_Turn_07_01_A, VO_BOM_08_005_Male_Dwarf_Tavish_InGame_Turn_07_01_C, VO_BOM_08_005_Male_Dwarf_Tavish_InGame_Turn_09_01_A, VO_BOM_08_005_Male_Dwarf_Tavish_InGame_Turn_13_01_A,
			VO_BOM_08_005_Male_Dwarf_Tavish_InGame_Turn_13_01_C, VO_BOM_08_005_Male_Dwarf_Tavish_InGame_Victory_PostExplosion_01_B, VO_BOM_08_005_Male_Gnome_Scabbs_InGame_HE_Custom_FirstTimeBossAt10Health_01_A, VO_BOM_08_005_Male_Gnome_Scabbs_InGame_HE_Custom_FirstTimeTavishKills_01, VO_BOM_08_005_Male_Gnome_Scabbs_InGame_HE_Custom_FirstTimeTavishPlaysTrap_01, VO_BOM_08_005_Male_Gnome_Scabbs_InGame_Turn_01_01_B, VO_BOM_08_005_Male_Gnome_Scabbs_InGame_Turn_07_01_B, VO_BOM_08_005_Male_Gnome_Scabbs_InGame_Turn_09_01_B, VO_BOM_08_005_Male_Gnome_Scabbs_InGame_Turn_09_01_C, VO_BOM_08_005_Male_Gnome_Scabbs_InGame_Turn_13_01_B,
			VO_BOM_08_005_Male_Orc_Galvangar_InGame_BossDeath_01, VO_BOM_08_005_Male_Orc_Galvangar_InGame_BossIdle_01, VO_BOM_08_005_Male_Orc_Galvangar_InGame_BossIdle_02, VO_BOM_08_005_Male_Orc_Galvangar_InGame_BossIdle_03, VO_BOM_08_005_Male_Orc_Galvangar_InGame_BossUsesHeroPower_01, VO_BOM_08_005_Male_Orc_Galvangar_InGame_BossUsesHeroPower_02, VO_BOM_08_005_Male_Orc_Galvangar_InGame_BossUsesHeroPower_03, VO_BOM_08_005_Male_Orc_Galvangar_InGame_EmoteResponse_01, VO_BOM_08_005_Male_Orc_Galvangar_InGame_Introduction_05_01_B, VO_BOM_08_005_Male_Orc_Galvangar_InGame_PlayerLoss_01
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
		case 516:
			yield return MissionPlaySound(enemyActor, VO_BOM_08_005_Male_Orc_Galvangar_InGame_BossDeath_01);
			break;
		case 517:
			yield return MissionPlayVO(enemyActor, m_InGame_BossIdleLines);
			break;
		case 510:
			yield return MissionPlayVO(enemyActor, m_InGame_BossUsesHeroPowerLines);
			break;
		case 515:
			yield return MissionPlayVO(enemyActor, VO_BOM_08_005_Male_Orc_Galvangar_InGame_EmoteResponse_01);
			break;
		case 514:
			yield return MissionPlayVO(friendlyActor, VO_BOM_08_005_Male_Dwarf_Tavish_InGame_Introduction_05_01_A);
			yield return MissionPlayVO(enemyActor, VO_BOM_08_005_Male_Orc_Galvangar_InGame_Introduction_05_01_B);
			break;
		case 506:
			yield return MissionPlayVO(enemyActor, VO_BOM_08_005_Male_Orc_Galvangar_InGame_PlayerLoss_01);
			break;
		case 505:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(BOM_08_Tavish_Dungeon.Alterac_CarielArt_BrassRing_Quote, VO_BOM_08_005_Female_Human_Cariel_InGame_Victory_PostExplosion_01_A);
			yield return MissionPlayVO(friendlyActor, VO_BOM_08_005_Male_Dwarf_Tavish_InGame_Victory_PostExplosion_01_B);
			GameState.Get().SetBusy(busy: false);
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
			yield return MissionPlayVO(friendlyActor, VO_BOM_08_005_Male_Dwarf_Tavish_InGame_Turn_01_01_A);
			yield return MissionPlayVO("BOM_08_Scabbs_005t", VO_BOM_08_005_Male_Gnome_Scabbs_InGame_Turn_01_01_B);
			break;
		case 7:
			yield return MissionPlayVO(friendlyActor, VO_BOM_08_005_Male_Dwarf_Tavish_InGame_Turn_07_01_A);
			yield return MissionPlayVO("BOM_08_Scabbs_005t", VO_BOM_08_005_Male_Gnome_Scabbs_InGame_Turn_07_01_B);
			yield return MissionPlayVO(friendlyActor, VO_BOM_08_005_Male_Dwarf_Tavish_InGame_Turn_07_01_C);
			break;
		case 11:
			yield return MissionPlayVO(friendlyActor, VO_BOM_08_005_Male_Dwarf_Tavish_InGame_Turn_09_01_A);
			yield return MissionPlayVO("BOM_08_Scabbs_005t", VO_BOM_08_005_Male_Gnome_Scabbs_InGame_Turn_09_01_B);
			yield return MissionPlayVO("BOM_08_Scabbs_005t", VO_BOM_08_005_Male_Gnome_Scabbs_InGame_Turn_09_01_C);
			break;
		case 17:
			yield return MissionPlayVO(friendlyActor, VO_BOM_08_005_Male_Dwarf_Tavish_InGame_Turn_13_01_A);
			yield return MissionPlayVO("BOM_08_Scabbs_005t", VO_BOM_08_005_Male_Gnome_Scabbs_InGame_Turn_13_01_B);
			yield return MissionPlayVO(friendlyActor, VO_BOM_08_005_Male_Dwarf_Tavish_InGame_Turn_13_01_C);
			break;
		}
	}
}
