using System.Collections;
using System.Collections.Generic;

public class BOM_09_Brukan_Fight_001 : BOM_09_Brukan_Dungeon
{
	private static readonly AssetReference VO_BOM_09_001_Female_Undead_LichTamsin_InGame_BossDeath_01 = new AssetReference("VO_BOM_09_001_Female_Undead_LichTamsin_InGame_BossDeath_01.prefab:8a37a3b1baeadbf4caaf25e9b6c73f9c");

	private static readonly AssetReference VO_BOM_09_001_Female_Undead_LichTamsin_InGame_BossIdle_01 = new AssetReference("VO_BOM_09_001_Female_Undead_LichTamsin_InGame_BossIdle_01.prefab:a9d5e2786fca70548a8353465f9b51eb");

	private static readonly AssetReference VO_BOM_09_001_Female_Undead_LichTamsin_InGame_BossIdle_02 = new AssetReference("VO_BOM_09_001_Female_Undead_LichTamsin_InGame_BossIdle_02.prefab:e481f380c3e25f64280f9a49ecea7d1e");

	private static readonly AssetReference VO_BOM_09_001_Female_Undead_LichTamsin_InGame_BossIdle_03 = new AssetReference("VO_BOM_09_001_Female_Undead_LichTamsin_InGame_BossIdle_03.prefab:8855d46259043c14f98f81325060e241");

	private static readonly AssetReference VO_BOM_09_001_Female_Undead_LichTamsin_InGame_BossUsesHeroPower_01 = new AssetReference("VO_BOM_09_001_Female_Undead_LichTamsin_InGame_BossUsesHeroPower_01.prefab:0a8fe0ba6ecadd44ebe877b1a502c1ee");

	private static readonly AssetReference VO_BOM_09_001_Female_Undead_LichTamsin_InGame_BossUsesHeroPower_02 = new AssetReference("VO_BOM_09_001_Female_Undead_LichTamsin_InGame_BossUsesHeroPower_02.prefab:7dd041a86ea75334aba81733f9925440");

	private static readonly AssetReference VO_BOM_09_001_Female_Undead_LichTamsin_InGame_BossUsesHeroPower_03 = new AssetReference("VO_BOM_09_001_Female_Undead_LichTamsin_InGame_BossUsesHeroPower_03.prefab:4875a2e390dba1449a957782d224761e");

	private static readonly AssetReference VO_BOM_09_001_Female_Undead_LichTamsin_InGame_EmoteResponse_01 = new AssetReference("VO_BOM_09_001_Female_Undead_LichTamsin_InGame_EmoteResponse_01.prefab:d1290e0077acbc34abfb58b0c0a31a40");

	private static readonly AssetReference VO_BOM_09_001_Female_Undead_LichTamsin_InGame_Introduction_01_A = new AssetReference("VO_BOM_09_001_Female_Undead_LichTamsin_InGame_Introduction_01_A.prefab:759da6cfbc4326c4f9cb5f80ee8dafcf");

	private static readonly AssetReference VO_BOM_09_001_Female_Undead_LichTamsin_InGame_PlayerLoss_01 = new AssetReference("VO_BOM_09_001_Female_Undead_LichTamsin_InGame_PlayerLoss_01.prefab:fb3567a9d809f2a4980f6e5c6839baf0");

	private static readonly AssetReference VO_BOM_09_001_Female_Undead_LichTamsin_InGame_Turn_03_01_A = new AssetReference("VO_BOM_09_001_Female_Undead_LichTamsin_InGame_Turn_03_01_A.prefab:a0c87725121824a49a912868600ada52");

	private static readonly AssetReference VO_BOM_09_001_Female_Undead_LichTamsin_InGame_Turn_03_01_B = new AssetReference("VO_BOM_09_001_Female_Undead_LichTamsin_InGame_Turn_03_01_B.prefab:d87101f11b24ba84f99dd2ec45017ccb");

	private static readonly AssetReference VO_BOM_09_001_Female_Undead_LichTamsin_InGame_Turn_07_01_A = new AssetReference("VO_BOM_09_001_Female_Undead_LichTamsin_InGame_Turn_07_01_A.prefab:fd6065e619dfce14982e51e043b59ec6");

	private static readonly AssetReference VO_BOM_09_001_Female_Undead_LichTamsin_InGame_Turn_07_01_B = new AssetReference("VO_BOM_09_001_Female_Undead_LichTamsin_InGame_Turn_07_01_B.prefab:131336d81b1ff6d48996fa0357589b10");

	private static readonly AssetReference VO_BOM_09_001_Female_Undead_LichTamsin_InGame_Turn_11_01_A = new AssetReference("VO_BOM_09_001_Female_Undead_LichTamsin_InGame_Turn_11_01_A.prefab:dd7ba5506155fe34f8ce38d97f57fae0");

	private static readonly AssetReference VO_BOM_09_001_Female_Undead_LichTamsin_InGame_VictoryPreExplosion_01_A = new AssetReference("VO_BOM_09_001_Female_Undead_LichTamsin_InGame_VictoryPreExplosion_01_A.prefab:8f9fa02475f5b5146893593334d1891c");

	private static readonly AssetReference VO_BOM_09_001_Male_Troll_Brukan_Emote_Picked_01 = new AssetReference("VO_BOM_09_001_Male_Troll_Brukan_Emote_Picked_01.prefab:d461082a64c01034bbd0984e82cfb65b");

	private static readonly AssetReference VO_BOM_09_001_Male_Troll_Brukan_InGame_Introduction_01_B = new AssetReference("VO_BOM_09_001_Male_Troll_Brukan_InGame_Introduction_01_B.prefab:6aeb54bd4fb814c4b82aa67e0aa5cece");

	private static readonly AssetReference VO_BOM_09_001_Male_Troll_Brukan_InGame_Turn_11_01_B = new AssetReference("VO_BOM_09_001_Male_Troll_Brukan_InGame_Turn_11_01_B.prefab:60db748614930c442bc33919e156e079");

	private static readonly AssetReference VO_BOM_09_001_Male_Troll_Brukan_InGame_VictoryPreExplosion_01_B = new AssetReference("VO_BOM_09_001_Male_Troll_Brukan_InGame_VictoryPreExplosion_01_B.prefab:ec327eef6ff006f4faa85c42ed3e9fc4");

	private static readonly AssetReference VO_BOM_09_001_Male_Troll_Brukan_UI_AWD_Boss_Reveal_General_01_01_A = new AssetReference("VO_BOM_09_001_Male_Troll_Brukan_UI_AWD_Boss_Reveal_General_01_01_A.prefab:4cb78985532860a4d86fd7fe3068241d");

	private static readonly AssetReference VO_BOM_09_001_Male_Troll_Brukan_UI_AWD_Boss_Reveal_General_01_01_B = new AssetReference("VO_BOM_09_001_Male_Troll_Brukan_UI_AWD_Boss_Reveal_General_01_01_B.prefab:bc8b00f613389ee48be6de756a43f7ec");

	private static readonly AssetReference VO_BOM_09_001_Male_Troll_Brukan_UI_AWD_Boss_Reveal_General_01_01_C = new AssetReference("VO_BOM_09_001_Male_Troll_Brukan_UI_AWD_Boss_Reveal_General_01_01_C.prefab:edfc6518a3038a84799e9fc3946deb9e");

	private List<string> m_InGame_BossIdleLines = new List<string> { VO_BOM_09_001_Female_Undead_LichTamsin_InGame_BossIdle_01, VO_BOM_09_001_Female_Undead_LichTamsin_InGame_BossIdle_02, VO_BOM_09_001_Female_Undead_LichTamsin_InGame_BossIdle_03 };

	private List<string> m_InGame_BossUsesHeroPowerLines = new List<string> { VO_BOM_09_001_Female_Undead_LichTamsin_InGame_BossUsesHeroPower_01, VO_BOM_09_001_Female_Undead_LichTamsin_InGame_BossUsesHeroPower_02, VO_BOM_09_001_Female_Undead_LichTamsin_InGame_BossUsesHeroPower_03 };

	private HashSet<string> m_playedLines = new HashSet<string>();

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> m_SoundFiles = new List<string>
		{
			VO_BOM_09_001_Female_Undead_LichTamsin_InGame_BossDeath_01, VO_BOM_09_001_Female_Undead_LichTamsin_InGame_BossIdle_01, VO_BOM_09_001_Female_Undead_LichTamsin_InGame_BossIdle_02, VO_BOM_09_001_Female_Undead_LichTamsin_InGame_BossIdle_03, VO_BOM_09_001_Female_Undead_LichTamsin_InGame_BossUsesHeroPower_01, VO_BOM_09_001_Female_Undead_LichTamsin_InGame_BossUsesHeroPower_02, VO_BOM_09_001_Female_Undead_LichTamsin_InGame_BossUsesHeroPower_03, VO_BOM_09_001_Female_Undead_LichTamsin_InGame_EmoteResponse_01, VO_BOM_09_001_Female_Undead_LichTamsin_InGame_Introduction_01_A, VO_BOM_09_001_Female_Undead_LichTamsin_InGame_PlayerLoss_01,
			VO_BOM_09_001_Female_Undead_LichTamsin_InGame_Turn_03_01_A, VO_BOM_09_001_Female_Undead_LichTamsin_InGame_Turn_03_01_B, VO_BOM_09_001_Female_Undead_LichTamsin_InGame_Turn_07_01_A, VO_BOM_09_001_Female_Undead_LichTamsin_InGame_Turn_07_01_B, VO_BOM_09_001_Female_Undead_LichTamsin_InGame_Turn_11_01_A, VO_BOM_09_001_Female_Undead_LichTamsin_InGame_VictoryPreExplosion_01_A, VO_BOM_09_001_Male_Troll_Brukan_InGame_Introduction_01_B, VO_BOM_09_001_Male_Troll_Brukan_InGame_Turn_11_01_B, VO_BOM_09_001_Male_Troll_Brukan_InGame_VictoryPreExplosion_01_B
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
			yield return MissionPlayVO(enemyActor, VO_BOM_09_001_Female_Undead_LichTamsin_InGame_BossDeath_01);
			break;
		case 517:
			yield return MissionPlayVO(enemyActor, m_InGame_BossIdleLines);
			break;
		case 510:
			yield return MissionPlayVO(enemyActor, m_InGame_BossUsesHeroPowerLines);
			break;
		case 515:
			yield return MissionPlayVO(enemyActor, VO_BOM_09_001_Female_Undead_LichTamsin_InGame_EmoteResponse_01);
			break;
		case 514:
			yield return MissionPlayVO(enemyActor, VO_BOM_09_001_Female_Undead_LichTamsin_InGame_Introduction_01_A);
			yield return MissionPlayVO(friendlyActor, VO_BOM_09_001_Male_Troll_Brukan_InGame_Introduction_01_B);
			break;
		case 506:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(enemyActor, VO_BOM_09_001_Female_Undead_LichTamsin_InGame_PlayerLoss_01);
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
			yield return MissionPlayVO(enemyActor, VO_BOM_09_001_Female_Undead_LichTamsin_InGame_Turn_03_01_A);
			yield return MissionPlayVO(enemyActor, VO_BOM_09_001_Female_Undead_LichTamsin_InGame_Turn_03_01_B);
			break;
		case 7:
			yield return MissionPlayVO(enemyActor, VO_BOM_09_001_Female_Undead_LichTamsin_InGame_Turn_07_01_A);
			yield return MissionPlayVO(enemyActor, VO_BOM_09_001_Female_Undead_LichTamsin_InGame_Turn_07_01_B);
			break;
		case 11:
			yield return MissionPlayVO(enemyActor, VO_BOM_09_001_Female_Undead_LichTamsin_InGame_Turn_11_01_A);
			yield return MissionPlayVO(friendlyActor, VO_BOM_09_001_Male_Troll_Brukan_InGame_Turn_11_01_B);
			break;
		}
	}
}
