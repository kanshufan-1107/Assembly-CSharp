using System.Collections;
using System.Collections.Generic;

public class BOM_09_Brukan_Fight_007 : BOM_09_Brukan_Dungeon
{
	private static readonly AssetReference VO_BOM_09_007_Female_Draenei_Xyrella_InGame_VictoryPostExplosion_01_B = new AssetReference("VO_BOM_09_007_Female_Draenei_Xyrella_InGame_VictoryPostExplosion_01_B.prefab:55d9a52465554c34ea02219f1fba09be");

	private static readonly AssetReference VO_BOM_09_007_Female_Draenei_Xyrella_InGame_VictoryPostExplosion_01_C = new AssetReference("VO_BOM_09_007_Female_Draenei_Xyrella_InGame_VictoryPostExplosion_01_C.prefab:961f0f33486c71f4294c807abf42308e");

	private static readonly AssetReference VO_BOM_09_007_Male_Dwarf_Tavish_InGame_HE_Custom_IfVanndarAtTenHealth_01_B = new AssetReference("VO_BOM_09_007_Male_Dwarf_Tavish_InGame_HE_Custom_IfVanndarAtTenHealth_01_B.prefab:2d5bf03bb6e736a43ba3827a593831bb");

	private static readonly AssetReference VO_BOM_09_007_Male_Dwarf_Vanndar_InGame_BossIdle_01 = new AssetReference("VO_BOM_09_007_Male_Dwarf_Vanndar_InGame_BossIdle_01.prefab:117cb530c8d10af428dbd164458c70e7");

	private static readonly AssetReference VO_BOM_09_007_Male_Dwarf_Vanndar_InGame_BossIdle_02 = new AssetReference("VO_BOM_09_007_Male_Dwarf_Vanndar_InGame_BossIdle_02.prefab:0ef682e6d94243a41b1e0d4f3ad513be");

	private static readonly AssetReference VO_BOM_09_007_Male_Dwarf_Vanndar_InGame_BossIdle_03 = new AssetReference("VO_BOM_09_007_Male_Dwarf_Vanndar_InGame_BossIdle_03.prefab:7991415743c27f441a102cc55a96bede");

	private static readonly AssetReference VO_BOM_09_007_Male_Dwarf_Vanndar_InGame_BossUsesHeroPower_01 = new AssetReference("VO_BOM_09_007_Male_Dwarf_Vanndar_InGame_BossUsesHeroPower_01.prefab:b86c6a79037ac9a498781606c233e33d");

	private static readonly AssetReference VO_BOM_09_007_Male_Dwarf_Vanndar_InGame_BossUsesHeroPower_02 = new AssetReference("VO_BOM_09_007_Male_Dwarf_Vanndar_InGame_BossUsesHeroPower_02.prefab:519104e91926bce49bfd3b8bb177a196");

	private static readonly AssetReference VO_BOM_09_007_Male_Dwarf_Vanndar_InGame_BossUsesHeroPower_03 = new AssetReference("VO_BOM_09_007_Male_Dwarf_Vanndar_InGame_BossUsesHeroPower_03.prefab:af951dc49b15eac448ab3f08aa6572a2");

	private static readonly AssetReference VO_BOM_09_007_Male_Dwarf_Vanndar_InGame_EmoteResponse_01 = new AssetReference("VO_BOM_09_007_Male_Dwarf_Vanndar_InGame_EmoteResponse_01.prefab:92f17fe46fe52df45b447609a4a6c2fd");

	private static readonly AssetReference VO_BOM_09_007_Male_Dwarf_Vanndar_InGame_HE_Custom_IfVanndarAtTenHealth_01_A = new AssetReference("VO_BOM_09_007_Male_Dwarf_Vanndar_InGame_HE_Custom_IfVanndarAtTenHealth_01_A.prefab:4f595c04461fffc40b05f56e30554512");

	private static readonly AssetReference VO_BOM_09_007_Male_Dwarf_Vanndar_InGame_Introduction_01_A = new AssetReference("VO_BOM_09_007_Male_Dwarf_Vanndar_InGame_Introduction_01_A.prefab:a816af134e3dd554db74dd47122a9e4e");

	private static readonly AssetReference VO_BOM_09_007_Male_Dwarf_Vanndar_InGame_PlayerLoss_01 = new AssetReference("VO_BOM_09_007_Male_Dwarf_Vanndar_InGame_PlayerLoss_01.prefab:10420b2680f1af74c8f4b5219a7094b9");

	private static readonly AssetReference VO_BOM_09_007_Male_Dwarf_Vanndar_InGame_VictoryPreExplosion_01_A = new AssetReference("VO_BOM_09_007_Male_Dwarf_Vanndar_InGame_VictoryPreExplosion_01_A.prefab:cbd34aeda963d9448b088ead9851564c");

	private static readonly AssetReference VO_BOM_09_007_Male_Tauren_Guff_InGame_Turn_17_01_A = new AssetReference("VO_BOM_09_007_Male_Tauren_Guff_InGame_Turn_17_01_A.prefab:4dcf4860ee8a31842b92d578517d36d6");

	private static readonly AssetReference VO_BOM_09_007_Male_Troll_Brukan_InGame_Introduction_01_B = new AssetReference("VO_BOM_09_007_Male_Troll_Brukan_InGame_Introduction_01_B.prefab:155875659e478c24383d7176c88b8f8a");

	private static readonly AssetReference VO_BOM_09_007_Male_Troll_Brukan_InGame_Turn_03_01_A = new AssetReference("VO_BOM_09_007_Male_Troll_Brukan_InGame_Turn_03_01_A.prefab:87d2e975c11df6d44bdf2d3068689dc2");

	private static readonly AssetReference VO_BOM_09_007_Male_Troll_Brukan_InGame_Turn_03_01_B = new AssetReference("VO_BOM_09_007_Male_Troll_Brukan_InGame_Turn_03_01_B.prefab:c14ac86a3f7d7c849a34de0e70725faf");

	private static readonly AssetReference VO_BOM_09_007_Male_Troll_Brukan_InGame_Turn_17_01_B = new AssetReference("VO_BOM_09_007_Male_Troll_Brukan_InGame_Turn_17_01_B.prefab:7c001e3db25be654ca3081460796dd23");

	private static readonly AssetReference VO_BOM_09_007_Male_Troll_Brukan_InGame_VictoryPreExplosion_01_B = new AssetReference("VO_BOM_09_007_Male_Troll_Brukan_InGame_VictoryPreExplosion_01_B.prefab:4b2651fd76504db47ab49f26afbf645c");

	private static readonly AssetReference VO_BOM_09_007_Male_Troll_Brukan_InGame_VictoryPreExplosion_01_C = new AssetReference("VO_BOM_09_007_Male_Troll_Brukan_InGame_VictoryPreExplosion_01_C.prefab:5c903fa8a7d458348a669131fb5741e0");

	private List<string> m_InGame_BossIdleLines = new List<string> { VO_BOM_09_007_Male_Dwarf_Vanndar_InGame_BossIdle_01, VO_BOM_09_007_Male_Dwarf_Vanndar_InGame_BossIdle_02, VO_BOM_09_007_Male_Dwarf_Vanndar_InGame_BossIdle_03 };

	private List<string> m_InGame_BossUsesHeroPowerLines = new List<string> { VO_BOM_09_007_Male_Dwarf_Vanndar_InGame_BossUsesHeroPower_01, VO_BOM_09_007_Male_Dwarf_Vanndar_InGame_BossUsesHeroPower_02, VO_BOM_09_007_Male_Dwarf_Vanndar_InGame_BossUsesHeroPower_03 };

	private HashSet<string> m_playedLines = new HashSet<string>();

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> m_SoundFiles = new List<string>
		{
			VO_BOM_09_007_Male_Dwarf_Tavish_InGame_HE_Custom_IfVanndarAtTenHealth_01_B, VO_BOM_09_007_Male_Dwarf_Vanndar_InGame_BossIdle_01, VO_BOM_09_007_Male_Dwarf_Vanndar_InGame_BossIdle_02, VO_BOM_09_007_Male_Dwarf_Vanndar_InGame_BossIdle_03, VO_BOM_09_007_Male_Dwarf_Vanndar_InGame_BossUsesHeroPower_01, VO_BOM_09_007_Male_Dwarf_Vanndar_InGame_BossUsesHeroPower_02, VO_BOM_09_007_Male_Dwarf_Vanndar_InGame_BossUsesHeroPower_03, VO_BOM_09_007_Male_Dwarf_Vanndar_InGame_EmoteResponse_01, VO_BOM_09_007_Male_Dwarf_Vanndar_InGame_HE_Custom_IfVanndarAtTenHealth_01_A, VO_BOM_09_007_Male_Dwarf_Vanndar_InGame_Introduction_01_A,
			VO_BOM_09_007_Male_Dwarf_Vanndar_InGame_PlayerLoss_01, VO_BOM_09_007_Male_Dwarf_Vanndar_InGame_VictoryPreExplosion_01_A, VO_BOM_09_007_Male_Troll_Brukan_InGame_Introduction_01_B, VO_BOM_09_007_Male_Troll_Brukan_InGame_Turn_03_01_A, VO_BOM_09_007_Male_Troll_Brukan_InGame_Turn_03_01_B, VO_BOM_09_007_Male_Troll_Brukan_InGame_Turn_17_01_B, VO_BOM_09_007_Male_Troll_Brukan_InGame_VictoryPreExplosion_01_B, VO_BOM_09_007_Male_Troll_Brukan_InGame_VictoryPreExplosion_01_C, VO_BOM_09_007_Male_Tauren_Guff_InGame_Turn_17_01_A
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
			yield return MissionPlayVO(enemyActor, VO_BOM_09_007_Male_Dwarf_Vanndar_InGame_EmoteResponse_01);
			break;
		case 514:
			yield return MissionPlayVO(enemyActor, VO_BOM_09_007_Male_Dwarf_Vanndar_InGame_Introduction_01_A);
			yield return MissionPlayVO(friendlyActor, VO_BOM_09_007_Male_Troll_Brukan_InGame_Introduction_01_B);
			break;
		case 506:
			yield return MissionPlayVO(enemyActor, VO_BOM_09_007_Male_Dwarf_Vanndar_InGame_PlayerLoss_01);
			break;
		case 504:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(enemyActor, VO_BOM_09_007_Male_Dwarf_Vanndar_InGame_VictoryPreExplosion_01_A);
			yield return MissionPlayVO(friendlyActor, VO_BOM_09_007_Male_Troll_Brukan_InGame_VictoryPreExplosion_01_B);
			yield return MissionPlayVO(friendlyActor, VO_BOM_09_007_Male_Troll_Brukan_InGame_VictoryPreExplosion_01_C);
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
		Actor enemyActor = GameState.Get().GetOpposingSidePlayer().GetHero()
			.GetCard()
			.GetActor();
		Actor friendlyActor = GameState.Get().GetFriendlySidePlayer().GetHero()
			.GetCard()
			.GetActor();
		switch (turn)
		{
		case 3:
			yield return MissionPlayVO(friendlyActor, VO_BOM_09_007_Male_Troll_Brukan_InGame_Turn_03_01_A);
			yield return MissionPlayVO(friendlyActor, VO_BOM_09_007_Male_Troll_Brukan_InGame_Turn_03_01_B);
			break;
		case 9:
			yield return MissionPlayVO(enemyActor, VO_BOM_09_007_Male_Dwarf_Vanndar_InGame_HE_Custom_IfVanndarAtTenHealth_01_A);
			yield return MissionPlayVO(BOM_09_Brukan_Dungeon.Tavish4_BrassRing_Quote, VO_BOM_09_007_Male_Dwarf_Tavish_InGame_HE_Custom_IfVanndarAtTenHealth_01_B);
			break;
		}
	}
}
