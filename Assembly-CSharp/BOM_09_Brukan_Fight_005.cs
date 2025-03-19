using System.Collections;
using System.Collections.Generic;

public class BOM_09_Brukan_Fight_005 : BOM_09_Brukan_Dungeon
{
	private static readonly AssetReference VO_BOM_09_005_Female_Draenei_Xyrella_InGame_HE_Custom_IfTurnStartsWithXyrellaAndGuffInPlay_01_A = new AssetReference("VO_BOM_09_005_Female_Draenei_Xyrella_InGame_HE_Custom_IfTurnStartsWithXyrellaAndGuffInPlay_01_A.prefab:fa4a839dd5b938d4d8d0c0ee335896c2");

	private static readonly AssetReference VO_BOM_09_005_Male_Elemental_Lokholar_InGame_VictoryPreExplosion_01_B = new AssetReference("VO_BOM_09_005_Male_Elemental_Lokholar_InGame_VictoryPreExplosion_01_B.prefab:35c4059ac49c1ba40b28d70ead03b663");

	private static readonly AssetReference VO_BOM_09_005_Male_Orc_DrekThar_InGame_Turn_03_01_B = new AssetReference("VO_BOM_09_005_Male_Orc_DrekThar_InGame_Turn_03_01_B.prefab:bb3728ecc73d8eb4aa6a1433e2ae61fe");

	private static readonly AssetReference VO_BOM_09_005_Male_Orc_Thrall_InGame_HE_Custom_IfDrektharVisionDestroyed_01_B = new AssetReference("VO_BOM_09_005_Male_Orc_Thrall_InGame_HE_Custom_IfDrektharVisionDestroyed_01_B.prefab:a6fade12c5de3f44fa1798b97a158d3a");

	private static readonly AssetReference VO_BOM_09_005_Male_Tauren_Primalist_InGame_BossIdle_01 = new AssetReference("VO_BOM_09_005_Male_Tauren_Primalist_InGame_BossIdle_01.prefab:63b0754f5dca92544837ee6394c85bbc");

	private static readonly AssetReference VO_BOM_09_005_Male_Tauren_Primalist_InGame_BossIdle_02 = new AssetReference("VO_BOM_09_005_Male_Tauren_Primalist_InGame_BossIdle_02.prefab:dcb8a99e5db48d046b3709e0607cffcf");

	private static readonly AssetReference VO_BOM_09_005_Male_Tauren_Primalist_InGame_BossIdle_03 = new AssetReference("VO_BOM_09_005_Male_Tauren_Primalist_InGame_BossIdle_03.prefab:c644d8857f7b5b743aa7ec502e9913be");

	private static readonly AssetReference VO_BOM_09_005_Male_Tauren_Primalist_InGame_BossUsesHeroPower_01 = new AssetReference("VO_BOM_09_005_Male_Tauren_Primalist_InGame_BossUsesHeroPower_01.prefab:85cc5d4347f51b14cb206a7126cf997c");

	private static readonly AssetReference VO_BOM_09_005_Male_Tauren_Primalist_InGame_BossUsesHeroPower_02 = new AssetReference("VO_BOM_09_005_Male_Tauren_Primalist_InGame_BossUsesHeroPower_02.prefab:bbbf9232941cfcc47a8014a632de1cea");

	private static readonly AssetReference VO_BOM_09_005_Male_Tauren_Primalist_InGame_BossUsesHeroPower_03 = new AssetReference("VO_BOM_09_005_Male_Tauren_Primalist_InGame_BossUsesHeroPower_03.prefab:e23f906ec929054439c928e1bdafb18b");

	private static readonly AssetReference VO_BOM_09_005_Male_Tauren_Primalist_InGame_EmoteResponse_01 = new AssetReference("VO_BOM_09_005_Male_Tauren_Primalist_InGame_EmoteResponse_01.prefab:7dd27cd815f8bb14cb09e186ff9c0c93");

	private static readonly AssetReference VO_BOM_09_005_Male_Tauren_Primalist_InGame_HE_Custom_IfDrektharVisionDestroyed_01_A = new AssetReference("VO_BOM_09_005_Male_Tauren_Primalist_InGame_HE_Custom_IfDrektharVisionDestroyed_01_A.prefab:eb93dce2bd7951749b84321f59714622");

	private static readonly AssetReference VO_BOM_09_005_Male_Tauren_Primalist_InGame_HE_Custom_IfThrallVisionDestroyed_01_A = new AssetReference("VO_BOM_09_005_Male_Tauren_Primalist_InGame_HE_Custom_IfThrallVisionDestroyed_01_A.prefab:741c42f285be67b43ba1b086c4eb59c0");

	private static readonly AssetReference VO_BOM_09_005_Male_Tauren_Primalist_InGame_HE_Custom_OneTurnAfterDaelinPlayed_01_B = new AssetReference("VO_BOM_09_005_Male_Tauren_Primalist_InGame_HE_Custom_OneTurnAfterDaelinPlayed_01_B.prefab:7f5d7916991525a43baf1bdfa0447bd4");

	private static readonly AssetReference VO_BOM_09_005_Male_Tauren_Primalist_InGame_Introduction_01_B = new AssetReference("VO_BOM_09_005_Male_Tauren_Primalist_InGame_Introduction_01_B.prefab:97c97029a04544742851b67edf0fa266");

	private static readonly AssetReference VO_BOM_09_005_Male_Tauren_Primalist_InGame_PlayerLoss_01 = new AssetReference("VO_BOM_09_005_Male_Tauren_Primalist_InGame_PlayerLoss_01.prefab:6e67b30e1f2a9b44ab33e4922c883bc5");

	private static readonly AssetReference VO_BOM_09_005_Male_Tauren_Primalist_InGame_Turn_03_01_A = new AssetReference("VO_BOM_09_005_Male_Tauren_Primalist_InGame_Turn_03_01_A.prefab:30d5281b4ccc94945a44a176674f1090");

	private static readonly AssetReference VO_BOM_09_005_Male_Tauren_Primalist_InGame_Turn_05_01_A = new AssetReference("VO_BOM_09_005_Male_Tauren_Primalist_InGame_Turn_05_01_A.prefab:f09e9980ca5023b4e9c00561b7b81644");

	private static readonly AssetReference VO_BOM_09_005_Male_Tauren_Primalist_InGame_VictoryPreExplosion_01_A = new AssetReference("VO_BOM_09_005_Male_Tauren_Primalist_InGame_VictoryPreExplosion_01_A.prefab:38961e0c7c2f90644af257b1918e1fad");

	private static readonly AssetReference VO_BOM_09_005_Male_Troll_Brukan_InGame_HE_Custom_IfDrektharVisionDestroyed_01_C = new AssetReference("VO_BOM_09_005_Male_Troll_Brukan_InGame_HE_Custom_IfDrektharVisionDestroyed_01_C.prefab:35d010678c4b2f549b49e5e2f888d24d");

	private static readonly AssetReference VO_BOM_09_005_Male_Troll_Brukan_InGame_HE_Custom_OneTurnAfterDaelinPlayed_01_A = new AssetReference("VO_BOM_09_005_Male_Troll_Brukan_InGame_HE_Custom_OneTurnAfterDaelinPlayed_01_A.prefab:80fa341ffaa9c3a4c9001300bef3b975");

	private static readonly AssetReference VO_BOM_09_005_Male_Troll_Brukan_InGame_HE_Custom_TwoTurnsAfterDaelinPlayed_01_A = new AssetReference("VO_BOM_09_005_Male_Troll_Brukan_InGame_HE_Custom_TwoTurnsAfterDaelinPlayed_01_A.prefab:8f73a97b400e1a24bad0902a9c1eb1e7");

	private static readonly AssetReference VO_BOM_09_005_Male_Troll_Brukan_InGame_Introduction_01_A = new AssetReference("VO_BOM_09_005_Male_Troll_Brukan_InGame_Introduction_01_A.prefab:65b1886614e69f143a12f2e6a9795874");

	private static readonly AssetReference VO_Story_Hero_Daelin_Male_Human_Story_Rexxar_Mission7Intro_01 = new AssetReference("VO_Story_Hero_Daelin_Male_Human_Story_Rexxar_Mission7Intro_01.prefab:e957ebe67fd8fc248ae8c47ef9fcd2ec");

	private List<string> m_InGame_BossIdleLines = new List<string> { VO_BOM_09_005_Male_Tauren_Primalist_InGame_BossIdle_01, VO_BOM_09_005_Male_Tauren_Primalist_InGame_BossIdle_02, VO_BOM_09_005_Male_Tauren_Primalist_InGame_BossIdle_03 };

	private List<string> m_InGame_BossUsesHeroPowerLines = new List<string> { VO_BOM_09_005_Male_Tauren_Primalist_InGame_BossUsesHeroPower_01, VO_BOM_09_005_Male_Tauren_Primalist_InGame_BossUsesHeroPower_02, VO_BOM_09_005_Male_Tauren_Primalist_InGame_BossUsesHeroPower_03 };

	private HashSet<string> m_playedLines = new HashSet<string>();

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> m_SoundFiles = new List<string>
		{
			VO_BOM_09_005_Male_Elemental_Lokholar_InGame_VictoryPreExplosion_01_B, VO_BOM_09_005_Male_Orc_DrekThar_InGame_Turn_03_01_B, VO_BOM_09_005_Male_Orc_Thrall_InGame_HE_Custom_IfDrektharVisionDestroyed_01_B, VO_BOM_09_005_Male_Tauren_Primalist_InGame_BossIdle_01, VO_BOM_09_005_Male_Tauren_Primalist_InGame_BossIdle_02, VO_BOM_09_005_Male_Tauren_Primalist_InGame_BossIdle_03, VO_BOM_09_005_Male_Tauren_Primalist_InGame_BossUsesHeroPower_01, VO_BOM_09_005_Male_Tauren_Primalist_InGame_BossUsesHeroPower_02, VO_BOM_09_005_Male_Tauren_Primalist_InGame_BossUsesHeroPower_03, VO_BOM_09_005_Male_Tauren_Primalist_InGame_EmoteResponse_01,
			VO_BOM_09_005_Male_Tauren_Primalist_InGame_HE_Custom_IfDrektharVisionDestroyed_01_A, VO_BOM_09_005_Male_Tauren_Primalist_InGame_HE_Custom_IfThrallVisionDestroyed_01_A, VO_BOM_09_005_Male_Tauren_Primalist_InGame_HE_Custom_OneTurnAfterDaelinPlayed_01_B, VO_BOM_09_005_Male_Tauren_Primalist_InGame_Introduction_01_B, VO_BOM_09_005_Male_Tauren_Primalist_InGame_PlayerLoss_01, VO_BOM_09_005_Male_Tauren_Primalist_InGame_Turn_03_01_A, VO_BOM_09_005_Male_Tauren_Primalist_InGame_Turn_05_01_A, VO_BOM_09_005_Male_Tauren_Primalist_InGame_VictoryPreExplosion_01_A, VO_BOM_09_005_Male_Troll_Brukan_InGame_HE_Custom_IfDrektharVisionDestroyed_01_C, VO_BOM_09_005_Male_Troll_Brukan_InGame_HE_Custom_OneTurnAfterDaelinPlayed_01_A,
			VO_BOM_09_005_Male_Troll_Brukan_InGame_HE_Custom_TwoTurnsAfterDaelinPlayed_01_A, VO_BOM_09_005_Male_Troll_Brukan_InGame_Introduction_01_A, VO_Story_Hero_Daelin_Male_Human_Story_Rexxar_Mission7Intro_01
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
		case 517:
			yield return MissionPlayVO(enemyActor, m_InGame_BossIdleLines);
			break;
		case 510:
			yield return MissionPlayVO(enemyActor, m_InGame_BossUsesHeroPowerLines);
			break;
		case 515:
			yield return MissionPlayVO(enemyActor, VO_BOM_09_005_Male_Tauren_Primalist_InGame_EmoteResponse_01);
			break;
		case 514:
			yield return MissionPlayVO(friendlyActor, VO_BOM_09_005_Male_Troll_Brukan_InGame_Introduction_01_A);
			yield return MissionPlayVO(enemyActor, VO_BOM_09_005_Male_Tauren_Primalist_InGame_Introduction_01_B);
			break;
		case 506:
			MissionPause(pause: true);
			yield return MissionPlayVO(enemyActor, VO_BOM_09_005_Male_Tauren_Primalist_InGame_PlayerLoss_01);
			MissionPause(pause: false);
			break;
		case 100:
			yield return MissionPlayVO(enemyActor, VO_BOM_09_005_Male_Tauren_Primalist_InGame_HE_Custom_IfDrektharVisionDestroyed_01_A);
			yield return MissionPlayVO("BOM_09_Thrall_005t", VO_BOM_09_005_Male_Orc_Thrall_InGame_HE_Custom_IfDrektharVisionDestroyed_01_B);
			yield return MissionPlayVO(friendlyActor, VO_BOM_09_005_Male_Troll_Brukan_InGame_HE_Custom_IfDrektharVisionDestroyed_01_C);
			break;
		case 101:
			yield return MissionPlayVO(enemyActor, VO_BOM_09_005_Male_Tauren_Primalist_InGame_HE_Custom_IfThrallVisionDestroyed_01_A);
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Daelin_Male_Human_Story_Rexxar_Mission7Intro_01);
			break;
		case 102:
			yield return MissionPlayVO(friendlyActor, VO_BOM_09_005_Male_Troll_Brukan_InGame_HE_Custom_OneTurnAfterDaelinPlayed_01_A);
			yield return MissionPlayVO(enemyActor, VO_BOM_09_005_Male_Tauren_Primalist_InGame_HE_Custom_OneTurnAfterDaelinPlayed_01_B);
			break;
		case 104:
			yield return MissionPlayVO(enemyActor, VO_BOM_09_005_Male_Troll_Brukan_InGame_HE_Custom_TwoTurnsAfterDaelinPlayed_01_A);
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
		GameState.Get().GetFriendlySidePlayer().GetHero()
			.GetCard()
			.GetActor();
		switch (turn)
		{
		case 3:
			yield return MissionPlayVO(enemyActor, VO_BOM_09_005_Male_Tauren_Primalist_InGame_Turn_03_01_A);
			yield return MissionPlayVO("BOM_09_DrekThar_005t", VO_BOM_09_005_Male_Orc_DrekThar_InGame_Turn_03_01_B);
			break;
		case 5:
			yield return MissionPlayVO(enemyActor, VO_BOM_09_005_Male_Tauren_Primalist_InGame_Turn_05_01_A);
			break;
		}
	}
}
