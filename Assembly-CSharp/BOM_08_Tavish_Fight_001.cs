using System.Collections;
using System.Collections.Generic;

public class BOM_08_Tavish_Fight_001 : BOM_08_Tavish_Dungeon
{
	private static readonly AssetReference VO_BOM_08_001_Female_Draenei_Xyrella_InGame_Turn_01_01_A = new AssetReference("VO_BOM_08_001_Female_Draenei_Xyrella_InGame_Turn_01_01_A.prefab:d6375f49a1865dd4fb6d4bfcbf930abc");

	private static readonly AssetReference VO_BOM_08_001_Male_Dwarf_Tavish_InGame_Introduction_01_B = new AssetReference("VO_BOM_08_001_Male_Dwarf_Tavish_InGame_Introduction_01_B.prefab:e07145d55d9550b4e8ae611775bc91e0");

	private static readonly AssetReference VO_BOM_08_001_Male_Dwarf_Tavish_InGame_Turn_03_01_A = new AssetReference("VO_BOM_08_001_Male_Dwarf_Tavish_InGame_Turn_03_01_A.prefab:396c2e9168b1fc746ac15568dd75b578");

	private static readonly AssetReference VO_BOM_08_001_Male_Dwarf_Tavish_InGame_Turn_07_01_B = new AssetReference("VO_BOM_08_001_Male_Dwarf_Tavish_InGame_Turn_07_01_B.prefab:8a04fca067af74143af4d9eba6853791");

	private static readonly AssetReference VO_BOM_08_001_Male_Dwarf_Tavish_InGame_Turn_11_01_A = new AssetReference("VO_BOM_08_001_Male_Dwarf_Tavish_InGame_Turn_11_01_A.prefab:830f184c76c4f314ca3b239ecb714d15");

	private static readonly AssetReference VO_BOM_08_001_Male_Dwarf_Tavish_InGame_Turn_11_01_B = new AssetReference("VO_BOM_08_001_Male_Dwarf_Tavish_InGame_Turn_11_01_B.prefab:029f5dfdf1c38244f9acf6cb41b627a2");

	private static readonly AssetReference VO_BOM_08_001_Male_Dwarf_Vanndar_InGame_BossIdle_01 = new AssetReference("VO_BOM_08_001_Male_Dwarf_Vanndar_InGame_BossIdle_01.prefab:ac8b0646fb8fa57419d4a0065e181493");

	private static readonly AssetReference VO_BOM_08_001_Male_Dwarf_Vanndar_InGame_BossIdle_02 = new AssetReference("VO_BOM_08_001_Male_Dwarf_Vanndar_InGame_BossIdle_02.prefab:4af46f75d971cf143a4139f19130b5e4");

	private static readonly AssetReference VO_BOM_08_001_Male_Dwarf_Vanndar_InGame_BossIdle_03 = new AssetReference("VO_BOM_08_001_Male_Dwarf_Vanndar_InGame_BossIdle_03.prefab:3cd360d5df83d5a43b48b3146c8d1e84");

	private static readonly AssetReference VO_BOM_08_001_Male_Dwarf_Vanndar_InGame_BossUsesHeroPower_01 = new AssetReference("VO_BOM_08_001_Male_Dwarf_Vanndar_InGame_BossUsesHeroPower_01.prefab:a7a1422e97dad48488dce968bf4af20e");

	private static readonly AssetReference VO_BOM_08_001_Male_Dwarf_Vanndar_InGame_BossUsesHeroPower_02 = new AssetReference("VO_BOM_08_001_Male_Dwarf_Vanndar_InGame_BossUsesHeroPower_02.prefab:cecf927d286268f488fdabc24873cea4");

	private static readonly AssetReference VO_BOM_08_001_Male_Dwarf_Vanndar_InGame_BossUsesHeroPower_03 = new AssetReference("VO_BOM_08_001_Male_Dwarf_Vanndar_InGame_BossUsesHeroPower_03.prefab:681acbb3dc24f7c4f813e8b73dc6d010");

	private static readonly AssetReference VO_BOM_08_001_Male_Dwarf_Vanndar_InGame_EmoteResponse_01 = new AssetReference("VO_BOM_08_001_Male_Dwarf_Vanndar_InGame_EmoteResponse_01.prefab:5ab748ca97e6c794e85a843aecd04547");

	private static readonly AssetReference VO_BOM_08_001_Male_Dwarf_Vanndar_InGame_Introduction_01_A = new AssetReference("VO_BOM_08_001_Male_Dwarf_Vanndar_InGame_Introduction_01_A.prefab:85431fa76da24a14c8471c80a9e99ad7");

	private static readonly AssetReference VO_BOM_08_001_Male_Dwarf_Vanndar_InGame_PlayerLoss_01 = new AssetReference("VO_BOM_08_001_Male_Dwarf_Vanndar_InGame_PlayerLoss_01.prefab:dedeee266b47b4d46953390cc00f5ea4");

	private static readonly AssetReference VO_BOM_08_001_Male_Dwarf_Vanndar_InGame_Turn_01_01_B = new AssetReference("VO_BOM_08_001_Male_Dwarf_Vanndar_InGame_Turn_01_01_B.prefab:c90627204bde22549b53098157eaf260");

	private static readonly AssetReference VO_BOM_08_001_Male_Dwarf_Vanndar_InGame_Turn_03_01_B = new AssetReference("VO_BOM_08_001_Male_Dwarf_Vanndar_InGame_Turn_03_01_B.prefab:bc66dd54453b8494e9b3bef2810871c3");

	private static readonly AssetReference VO_BOM_08_001_Male_Dwarf_Vanndar_InGame_Turn_07_01_A = new AssetReference("VO_BOM_08_001_Male_Dwarf_Vanndar_InGame_Turn_07_01_A.prefab:73398895759de534ba09a46b054be3de");

	private static readonly AssetReference VO_BOM_08_001_Male_Dwarf_Vanndar_InGame_Turn_13_01_A = new AssetReference("VO_BOM_08_001_Male_Dwarf_Vanndar_InGame_Turn_13_01_A.prefab:36b2b771b63eb034e8b1008fa864df4d");

	private static readonly AssetReference VO_BOM_08_001_Male_Dwarf_Vanndar_InGame_VictoryPreExplosion_01_A = new AssetReference("VO_BOM_08_001_Male_Dwarf_Vanndar_InGame_VictoryPreExplosion_01_A.prefab:fec4df40e3a3b514db6c1546ae93f56b");

	private static readonly AssetReference VO_BOM_08_001_Male_Dwarf_Vanndar_InGame_VictoryPreExplosion_01_B = new AssetReference("VO_BOM_08_001_Male_Dwarf_Vanndar_InGame_VictoryPreExplosion_01_B.prefab:6e4cafc69fae7464ca3f8626f86a9ff9");

	private List<string> m_InGame_BossIdleLines = new List<string> { VO_BOM_08_001_Male_Dwarf_Vanndar_InGame_BossIdle_01, VO_BOM_08_001_Male_Dwarf_Vanndar_InGame_BossIdle_02, VO_BOM_08_001_Male_Dwarf_Vanndar_InGame_BossIdle_03 };

	private List<string> m_InGame_BossUsesHeroPowerLines = new List<string> { VO_BOM_08_001_Male_Dwarf_Vanndar_InGame_BossUsesHeroPower_01, VO_BOM_08_001_Male_Dwarf_Vanndar_InGame_BossUsesHeroPower_02, VO_BOM_08_001_Male_Dwarf_Vanndar_InGame_BossUsesHeroPower_03 };

	private HashSet<string> m_playedLines = new HashSet<string>();

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> m_SoundFiles = new List<string>
		{
			VO_BOM_08_001_Female_Draenei_Xyrella_InGame_Turn_01_01_A, VO_BOM_08_001_Male_Dwarf_Tavish_InGame_Introduction_01_B, VO_BOM_08_001_Male_Dwarf_Tavish_InGame_Turn_03_01_A, VO_BOM_08_001_Male_Dwarf_Tavish_InGame_Turn_07_01_B, VO_BOM_08_001_Male_Dwarf_Tavish_InGame_Turn_11_01_A, VO_BOM_08_001_Male_Dwarf_Tavish_InGame_Turn_11_01_B, VO_BOM_08_001_Male_Dwarf_Vanndar_InGame_BossIdle_01, VO_BOM_08_001_Male_Dwarf_Vanndar_InGame_BossIdle_02, VO_BOM_08_001_Male_Dwarf_Vanndar_InGame_BossIdle_03, VO_BOM_08_001_Male_Dwarf_Vanndar_InGame_BossUsesHeroPower_01,
			VO_BOM_08_001_Male_Dwarf_Vanndar_InGame_BossUsesHeroPower_02, VO_BOM_08_001_Male_Dwarf_Vanndar_InGame_BossUsesHeroPower_03, VO_BOM_08_001_Male_Dwarf_Vanndar_InGame_EmoteResponse_01, VO_BOM_08_001_Male_Dwarf_Vanndar_InGame_Introduction_01_A, VO_BOM_08_001_Male_Dwarf_Vanndar_InGame_PlayerLoss_01, VO_BOM_08_001_Male_Dwarf_Vanndar_InGame_Turn_01_01_B, VO_BOM_08_001_Male_Dwarf_Vanndar_InGame_Turn_03_01_B, VO_BOM_08_001_Male_Dwarf_Vanndar_InGame_Turn_07_01_A, VO_BOM_08_001_Male_Dwarf_Vanndar_InGame_Turn_13_01_A, VO_BOM_08_001_Male_Dwarf_Vanndar_InGame_VictoryPreExplosion_01_A,
			VO_BOM_08_001_Male_Dwarf_Vanndar_InGame_VictoryPreExplosion_01_B
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
			yield return MissionPlayVO(enemyActor, VO_BOM_08_001_Male_Dwarf_Vanndar_InGame_EmoteResponse_01);
			break;
		case 514:
			yield return MissionPlayVO(enemyActor, VO_BOM_08_001_Male_Dwarf_Vanndar_InGame_Introduction_01_A);
			yield return MissionPlayVO(friendlyActor, VO_BOM_08_001_Male_Dwarf_Tavish_InGame_Introduction_01_B);
			break;
		case 506:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(enemyActor, VO_BOM_08_001_Male_Dwarf_Vanndar_InGame_PlayerLoss_01);
			GameState.Get().SetBusy(busy: false);
			break;
		case 504:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(enemyActor, VO_BOM_08_001_Male_Dwarf_Vanndar_InGame_VictoryPreExplosion_01_A);
			yield return MissionPlayVO(enemyActor, VO_BOM_08_001_Male_Dwarf_Vanndar_InGame_VictoryPreExplosion_01_B);
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
		case 1:
			yield return MissionPlayVO(BOM_08_Tavish_Dungeon.Alterac_XyrellaArt_BrassRing_Quote, VO_BOM_08_001_Female_Draenei_Xyrella_InGame_Turn_01_01_A);
			yield return MissionPlayVO(enemyActor, VO_BOM_08_001_Male_Dwarf_Vanndar_InGame_Turn_01_01_B);
			break;
		case 3:
			yield return MissionPlayVO(friendlyActor, VO_BOM_08_001_Male_Dwarf_Tavish_InGame_Turn_03_01_A);
			yield return MissionPlayVO(enemyActor, VO_BOM_08_001_Male_Dwarf_Vanndar_InGame_Turn_03_01_B);
			break;
		case 7:
			yield return MissionPlayVO(enemyActor, VO_BOM_08_001_Male_Dwarf_Vanndar_InGame_Turn_07_01_A);
			yield return MissionPlayVO(friendlyActor, VO_BOM_08_001_Male_Dwarf_Tavish_InGame_Turn_07_01_B);
			break;
		case 11:
			yield return MissionPlayVO(friendlyActor, VO_BOM_08_001_Male_Dwarf_Tavish_InGame_Turn_11_01_A);
			yield return MissionPlayVO(friendlyActor, VO_BOM_08_001_Male_Dwarf_Tavish_InGame_Turn_11_01_B);
			break;
		case 13:
			yield return MissionPlayVO(enemyActor, VO_BOM_08_001_Male_Dwarf_Vanndar_InGame_Turn_13_01_A);
			break;
		}
	}
}
