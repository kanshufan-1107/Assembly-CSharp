using System.Collections;
using System.Collections.Generic;

public class BOM_09_Brukan_Fight_003 : BOM_09_Brukan_Dungeon
{
	private static readonly AssetReference VO_BOM_09_003_Female_Orc_Rokara_InGame_Turn_01_01_A = new AssetReference("VO_BOM_09_003_Female_Orc_Rokara_InGame_Turn_01_01_A.prefab:9b3d4870f5c9f354fbd2edab7eaf2835");

	private static readonly AssetReference VO_BOM_09_003_Female_Orc_Rokara_InGame_Turn_05_01_A = new AssetReference("VO_BOM_09_003_Female_Orc_Rokara_InGame_Turn_05_01_A.prefab:80c2f4d39cf128b40b705c46a1bd1296");

	private static readonly AssetReference VO_BOM_09_003_Female_Orc_Rokara_InGame_Turn_09_01_A = new AssetReference("VO_BOM_09_003_Female_Orc_Rokara_InGame_Turn_09_01_A.prefab:b280c9f5c9e208f47bc928df7aac9611");

	private static readonly AssetReference VO_BOM_09_003_Female_Orc_Rokara_InGame_VictoryPostExplosion_01_B = new AssetReference("VO_BOM_09_003_Female_Orc_Rokara_InGame_VictoryPostExplosion_01_B.prefab:55914118a3593f649a72bdc10fddcd8f");

	private static readonly AssetReference VO_BOM_09_003_Male_Human_Ichman_InGame_BossDeath_01 = new AssetReference("VO_BOM_09_003_Male_Human_Ichman_InGame_BossDeath_01.prefab:c3e1a1b6d6c6ab2489a96bf463891975");

	private static readonly AssetReference VO_BOM_09_003_Male_Human_Ichman_InGame_BossIdle_01 = new AssetReference("VO_BOM_09_003_Male_Human_Ichman_InGame_BossIdle_01.prefab:103d4a27a6818f546870bbb1d69f60df");

	private static readonly AssetReference VO_BOM_09_003_Male_Human_Ichman_InGame_BossIdle_02 = new AssetReference("VO_BOM_09_003_Male_Human_Ichman_InGame_BossIdle_02.prefab:60e84f9349e75ab4985602b9e13aa020");

	private static readonly AssetReference VO_BOM_09_003_Male_Human_Ichman_InGame_BossIdle_03 = new AssetReference("VO_BOM_09_003_Male_Human_Ichman_InGame_BossIdle_03.prefab:f2c312d8a2dc9d7409141b9d1c375378");

	private static readonly AssetReference VO_BOM_09_003_Male_Human_Ichman_InGame_BossUsesHeroPower_01 = new AssetReference("VO_BOM_09_003_Male_Human_Ichman_InGame_BossUsesHeroPower_01.prefab:9c406740475e35744aed740ab8cbf8ee");

	private static readonly AssetReference VO_BOM_09_003_Male_Human_Ichman_InGame_BossUsesHeroPower_02 = new AssetReference("VO_BOM_09_003_Male_Human_Ichman_InGame_BossUsesHeroPower_02.prefab:659a07dfcb6007349a423b23969622c9");

	private static readonly AssetReference VO_BOM_09_003_Male_Human_Ichman_InGame_BossUsesHeroPower_03 = new AssetReference("VO_BOM_09_003_Male_Human_Ichman_InGame_BossUsesHeroPower_03.prefab:a5c3921e63485814598ca2ba6eebedb5");

	private static readonly AssetReference VO_BOM_09_003_Male_Human_Ichman_InGame_EmoteResponse_01 = new AssetReference("VO_BOM_09_003_Male_Human_Ichman_InGame_EmoteResponse_01.prefab:a4c6c28cddd63a64aa7ff39d8aaeaf1a");

	private static readonly AssetReference VO_BOM_09_003_Male_Human_Ichman_InGame_Introduction_01_A = new AssetReference("VO_BOM_09_003_Male_Human_Ichman_InGame_Introduction_01_A.prefab:85e8bea3dd7cc0a4da7c63fcea1c513b");

	private static readonly AssetReference VO_BOM_09_003_Male_Human_Ichman_InGame_PlayerLoss_01 = new AssetReference("VO_BOM_09_003_Male_Human_Ichman_InGame_PlayerLoss_01.prefab:b5e1af113814b6f44a634d4727089bcf");

	private static readonly AssetReference VO_BOM_09_003_Male_Human_Ichman_InGame_Turn_05_01_A = new AssetReference("VO_BOM_09_003_Male_Human_Ichman_InGame_Turn_05_01_A.prefab:6c5ed6da811a6344cbae5a8be9128f27");

	private static readonly AssetReference VO_BOM_09_003_Male_Human_Ichman_InGame_Turn_09_01_A = new AssetReference("VO_BOM_09_003_Male_Human_Ichman_InGame_Turn_09_01_A.prefab:cfcf52d7266705444bef1b79db3e3903");

	private static readonly AssetReference VO_BOM_09_003_Male_Human_Ichman_InGame_Turn_13_01_A = new AssetReference("VO_BOM_09_003_Male_Human_Ichman_InGame_Turn_13_01_A.prefab:b16cedbe18fb091479a3ada3c907d69a");

	private static readonly AssetReference VO_BOM_09_003_Male_Tauren_Guff_InGame_Turn_01_01_A = new AssetReference("VO_BOM_09_003_Male_Tauren_Guff_InGame_Turn_01_01_A.prefab:a35d7df9c5adef6458743a4f41d03631");

	private static readonly AssetReference VO_BOM_09_003_Male_Tauren_Guff_InGame_Turn_05_01_A = new AssetReference("VO_BOM_09_003_Male_Tauren_Guff_InGame_Turn_05_01_A.prefab:310e993790996f546b70981a3240d467");

	private static readonly AssetReference VO_BOM_09_003_Male_Tauren_Guff_InGame_Turn_09_01_A = new AssetReference("VO_BOM_09_003_Male_Tauren_Guff_InGame_Turn_09_01_A.prefab:2f3ec88680429a04c8265fff0f49b266");

	private static readonly AssetReference VO_BOM_09_003_Male_Troll_Brukan_InGame_Introduction_01_B = new AssetReference("VO_BOM_09_003_Male_Troll_Brukan_InGame_Introduction_01_B.prefab:d3bf6b50ea157c842bd22eaa64093c45");

	private static readonly AssetReference VO_BOM_09_003_Male_Troll_Brukan_InGame_Turn_13_01_B = new AssetReference("VO_BOM_09_003_Male_Troll_Brukan_InGame_Turn_13_01_B.prefab:c508f32a8a8f0f5478ad010056a5fe47");

	private static readonly AssetReference VO_BOM_09_003_Male_Troll_Brukan_InGame_VictoryPostExplosion_01_A = new AssetReference("VO_BOM_09_003_Male_Troll_Brukan_InGame_VictoryPostExplosion_01_A.prefab:ed922bd9870e19e4f8f02aa24f4ffa0d");

	private static readonly AssetReference VO_BOM_09_003_Male_Troll_Brukan_InGame_VictoryPostExplosion_01_C = new AssetReference("VO_BOM_09_003_Male_Troll_Brukan_InGame_VictoryPostExplosion_01_C.prefab:1920431057b8bcf489bac12fe9832fe2");

	private static readonly AssetReference VO_BOM_09_003_X_BloodElf_Dawngrasp_InGame_Turn_01_01_A = new AssetReference("VO_BOM_09_003_X_BloodElf_Dawngrasp_InGame_Turn_01_01_A.prefab:363cc7bf7ddc60c40b1a709dfeeca72c");

	private static readonly AssetReference VO_BOM_09_003_X_BloodElf_Dawngrasp_InGame_Turn_05_01_A = new AssetReference("VO_BOM_09_003_X_BloodElf_Dawngrasp_InGame_Turn_05_01_A.prefab:c3560ea542b2c6642b69c3a106e9e3f1");

	private static readonly AssetReference VO_BOM_09_003_X_BloodElf_Dawngrasp_InGame_Turn_09_01_A = new AssetReference("VO_BOM_09_003_X_BloodElf_Dawngrasp_InGame_Turn_09_01_A.prefab:b0d3e480215cdaf469bae9a0a58e5b11");

	private List<string> m_InGame_BossIdleLines = new List<string> { VO_BOM_09_003_Male_Human_Ichman_InGame_BossIdle_01, VO_BOM_09_003_Male_Human_Ichman_InGame_BossIdle_02, VO_BOM_09_003_Male_Human_Ichman_InGame_BossIdle_03 };

	private List<string> m_InGame_BossUsesHeroPowerLines = new List<string> { VO_BOM_09_003_Male_Human_Ichman_InGame_BossUsesHeroPower_01, VO_BOM_09_003_Male_Human_Ichman_InGame_BossUsesHeroPower_02, VO_BOM_09_003_Male_Human_Ichman_InGame_BossUsesHeroPower_03 };

	private HashSet<string> m_playedLines = new HashSet<string>();

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> m_SoundFiles = new List<string>
		{
			VO_BOM_09_003_Female_Orc_Rokara_InGame_Turn_01_01_A, VO_BOM_09_003_Female_Orc_Rokara_InGame_Turn_05_01_A, VO_BOM_09_003_Female_Orc_Rokara_InGame_Turn_09_01_A, VO_BOM_09_003_Female_Orc_Rokara_InGame_VictoryPostExplosion_01_B, VO_BOM_09_003_Male_Human_Ichman_InGame_BossDeath_01, VO_BOM_09_003_Male_Human_Ichman_InGame_BossIdle_01, VO_BOM_09_003_Male_Human_Ichman_InGame_BossIdle_02, VO_BOM_09_003_Male_Human_Ichman_InGame_BossIdle_03, VO_BOM_09_003_Male_Human_Ichman_InGame_BossUsesHeroPower_01, VO_BOM_09_003_Male_Human_Ichman_InGame_BossUsesHeroPower_02,
			VO_BOM_09_003_Male_Human_Ichman_InGame_BossUsesHeroPower_03, VO_BOM_09_003_Male_Human_Ichman_InGame_EmoteResponse_01, VO_BOM_09_003_Male_Human_Ichman_InGame_Introduction_01_A, VO_BOM_09_003_Male_Human_Ichman_InGame_PlayerLoss_01, VO_BOM_09_003_Male_Human_Ichman_InGame_Turn_05_01_A, VO_BOM_09_003_Male_Human_Ichman_InGame_Turn_09_01_A, VO_BOM_09_003_Male_Human_Ichman_InGame_Turn_13_01_A, VO_BOM_09_003_Male_Tauren_Guff_InGame_Turn_01_01_A, VO_BOM_09_003_Male_Tauren_Guff_InGame_Turn_05_01_A, VO_BOM_09_003_Male_Tauren_Guff_InGame_Turn_09_01_A,
			VO_BOM_09_003_Male_Troll_Brukan_InGame_Introduction_01_B, VO_BOM_09_003_Male_Troll_Brukan_InGame_Turn_13_01_B, VO_BOM_09_003_Male_Troll_Brukan_InGame_VictoryPostExplosion_01_A, VO_BOM_09_003_Male_Troll_Brukan_InGame_VictoryPostExplosion_01_C, VO_BOM_09_003_X_BloodElf_Dawngrasp_InGame_Turn_01_01_A, VO_BOM_09_003_X_BloodElf_Dawngrasp_InGame_Turn_05_01_A, VO_BOM_09_003_X_BloodElf_Dawngrasp_InGame_Turn_09_01_A
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
			yield return MissionPlayVO(enemyActor, VO_BOM_09_003_Male_Human_Ichman_InGame_BossDeath_01);
			break;
		case 517:
			yield return MissionPlayVO(enemyActor, m_InGame_BossIdleLines);
			break;
		case 510:
			yield return MissionPlayVO(enemyActor, m_InGame_BossUsesHeroPowerLines);
			break;
		case 515:
			yield return MissionPlayVO(enemyActor, VO_BOM_09_003_Male_Human_Ichman_InGame_EmoteResponse_01);
			break;
		case 514:
			yield return MissionPlayVO(enemyActor, VO_BOM_09_003_Male_Human_Ichman_InGame_Introduction_01_A);
			yield return MissionPlayVO(friendlyActor, VO_BOM_09_003_Male_Troll_Brukan_InGame_Introduction_01_B);
			break;
		case 506:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(enemyActor, VO_BOM_09_003_Male_Human_Ichman_InGame_PlayerLoss_01);
			GameState.Get().SetBusy(busy: false);
			break;
		case 505:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(friendlyActor, VO_BOM_09_003_Male_Troll_Brukan_InGame_VictoryPostExplosion_01_A);
			yield return MissionPlayVO("BOM_09_Rokara_003p", BOM_09_Brukan_Dungeon.RokaraTier5_BrassRing_Quote, VO_BOM_09_003_Female_Orc_Rokara_InGame_VictoryPostExplosion_01_B);
			yield return MissionPlayVO(friendlyActor, VO_BOM_09_003_Male_Troll_Brukan_InGame_VictoryPostExplosion_01_C);
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
		case 1:
			yield return MissionPlayVO("BOM_09_Rokara_003p", VO_BOM_09_003_Female_Orc_Rokara_InGame_Turn_01_01_A);
			yield return MissionPlayVO("BOM_09_Guff_003p", VO_BOM_09_003_Male_Tauren_Guff_InGame_Turn_01_01_A);
			yield return MissionPlayVO("BOM_09_Dawngrasp_003p", VO_BOM_09_003_X_BloodElf_Dawngrasp_InGame_Turn_01_01_A);
			break;
		case 5:
			yield return MissionPlayVO(enemyActor, VO_BOM_09_003_Male_Human_Ichman_InGame_Turn_05_01_A);
			yield return MissionPlayVO("BOM_09_Rokara_003p", VO_BOM_09_003_Female_Orc_Rokara_InGame_Turn_05_01_A);
			yield return MissionPlayVO("BOM_09_Guff_003p", VO_BOM_09_003_Male_Tauren_Guff_InGame_Turn_05_01_A);
			yield return MissionPlayVO("BOM_09_Dawngrasp_003p", VO_BOM_09_003_X_BloodElf_Dawngrasp_InGame_Turn_05_01_A);
			break;
		case 9:
			yield return MissionPlayVO(enemyActor, VO_BOM_09_003_Male_Human_Ichman_InGame_Turn_09_01_A);
			yield return MissionPlayVO("BOM_09_Rokara_003p", VO_BOM_09_003_Female_Orc_Rokara_InGame_Turn_09_01_A);
			yield return MissionPlayVO("BOM_09_Guff_003p", VO_BOM_09_003_Male_Tauren_Guff_InGame_Turn_09_01_A);
			yield return MissionPlayVO("BOM_09_Dawngrasp_003p", VO_BOM_09_003_X_BloodElf_Dawngrasp_InGame_Turn_09_01_A);
			break;
		case 13:
			yield return MissionPlayVO(enemyActor, VO_BOM_09_003_Male_Human_Ichman_InGame_Turn_13_01_A);
			yield return MissionPlayVO(friendlyActor, VO_BOM_09_003_Male_Troll_Brukan_InGame_Turn_13_01_B);
			break;
		}
	}
}
