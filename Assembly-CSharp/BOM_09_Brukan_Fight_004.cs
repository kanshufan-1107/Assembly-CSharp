using System.Collections;
using System.Collections.Generic;

public class BOM_09_Brukan_Fight_004 : BOM_09_Brukan_Dungeon
{
	private static readonly AssetReference VO_BOM_09_004_Female_Human_Cariel_Emote_Attack_01 = new AssetReference("VO_BOM_09_004_Female_Human_Cariel_Emote_Attack_01.prefab:543e6bb5315f3f6408667be5a9c62ff8");

	private static readonly AssetReference VO_BOM_09_004_Female_Human_Cariel_Emote_Play_01 = new AssetReference("VO_BOM_09_004_Female_Human_Cariel_Emote_Play_01.prefab:b2d6ffa771cf3c84195470709c295bb7");

	private static readonly AssetReference VO_BOM_09_004_Female_Human_Cariel_InGame_Turn_11_01_A = new AssetReference("VO_BOM_09_004_Female_Human_Cariel_InGame_Turn_11_01_A.prefab:45ad570c78380f4449069e10c071284f");

	private static readonly AssetReference VO_BOM_09_004_Female_Human_Cariel_InGame_Turn_11_01_C = new AssetReference("VO_BOM_09_004_Female_Human_Cariel_InGame_Turn_11_01_C.prefab:ac027cbcb7b6e0b4280fff6ca4728244");

	private static readonly AssetReference VO_BOM_09_004_Female_Human_Cariel_InGame_VictoryPostExplosion_01_B = new AssetReference("VO_BOM_09_004_Female_Human_Cariel_InGame_VictoryPostExplosion_01_B.prefab:e4deef74665cce140b2a30cd615f1fe0");

	private static readonly AssetReference VO_BOM_09_004_Female_Human_Cariel_InGame_VictoryPostExplosion_01_C = new AssetReference("VO_BOM_09_004_Female_Human_Cariel_InGame_VictoryPostExplosion_01_C.prefab:a6ca2266c6c771f418a06a2a1079d73f");

	private static readonly AssetReference VO_BOM_09_004_Female_Orc_Rokara_InGame_Turn_03_01_A = new AssetReference("VO_BOM_09_004_Female_Orc_Rokara_InGame_Turn_03_01_A.prefab:9207f67da650fef47bb531d0b977a13c");

	private static readonly AssetReference VO_BOM_09_004_Male_Kobold_Snivvle_InGame_BossDeath_01 = new AssetReference("VO_BOM_09_004_Male_Kobold_Snivvle_InGame_BossDeath_01.prefab:e62bdb6af4642fc4aab2f38a90f0575e");

	private static readonly AssetReference VO_BOM_09_004_Male_Kobold_Snivvle_InGame_BossDeath_02 = new AssetReference("VO_BOM_09_004_Male_Kobold_Snivvle_InGame_BossDeath_02.prefab:8d63e7c6cb4d553498214160abf3d28a");

	private static readonly AssetReference VO_BOM_09_004_Male_Kobold_Snivvle_InGame_BossIdle_01 = new AssetReference("VO_BOM_09_004_Male_Kobold_Snivvle_InGame_BossIdle_01.prefab:6ccc862d8d7b1e84c9251eb695a3ab50");

	private static readonly AssetReference VO_BOM_09_004_Male_Kobold_Snivvle_InGame_BossIdle_02 = new AssetReference("VO_BOM_09_004_Male_Kobold_Snivvle_InGame_BossIdle_02.prefab:b1cefa4e631091b418a45562858ec733");

	private static readonly AssetReference VO_BOM_09_004_Male_Kobold_Snivvle_InGame_BossIdle_03 = new AssetReference("VO_BOM_09_004_Male_Kobold_Snivvle_InGame_BossIdle_03.prefab:cbb81ffa776072945950d58d93a4b59b");

	private static readonly AssetReference VO_BOM_09_004_Male_Kobold_Snivvle_InGame_BossUsesHeroPower_01 = new AssetReference("VO_BOM_09_004_Male_Kobold_Snivvle_InGame_BossUsesHeroPower_01.prefab:1ce24c931cbbf544580232c5cef78407");

	private static readonly AssetReference VO_BOM_09_004_Male_Kobold_Snivvle_InGame_BossUsesHeroPower_02 = new AssetReference("VO_BOM_09_004_Male_Kobold_Snivvle_InGame_BossUsesHeroPower_02.prefab:d72e74b52dcb6814daa6c18ed7987ae4");

	private static readonly AssetReference VO_BOM_09_004_Male_Kobold_Snivvle_InGame_BossUsesHeroPower_03 = new AssetReference("VO_BOM_09_004_Male_Kobold_Snivvle_InGame_BossUsesHeroPower_03.prefab:93b889f6b8dea134088b78b9a1f79bbe");

	private static readonly AssetReference VO_BOM_09_004_Male_Kobold_Snivvle_InGame_EmoteResponse_01 = new AssetReference("VO_BOM_09_004_Male_Kobold_Snivvle_InGame_EmoteResponse_01.prefab:412a77bb53c6d91439a9f4310f1aeadf");

	private static readonly AssetReference VO_BOM_09_004_Male_Kobold_Snivvle_InGame_HE_Custom_IfFirstCrateBroken_01_A = new AssetReference("VO_BOM_09_004_Male_Kobold_Snivvle_InGame_HE_Custom_IfFirstCrateBroken_01_A.prefab:895ab432293c0224baa9cbe2bfa90ed6");

	private static readonly AssetReference VO_BOM_09_004_Male_Kobold_Snivvle_InGame_Introduction_01_B = new AssetReference("VO_BOM_09_004_Male_Kobold_Snivvle_InGame_Introduction_01_B.prefab:c69099629b3b14b49b27fc8b4f150514");

	private static readonly AssetReference VO_BOM_09_004_Male_Kobold_Snivvle_InGame_PlayerLoss_01 = new AssetReference("VO_BOM_09_004_Male_Kobold_Snivvle_InGame_PlayerLoss_01.prefab:502c1fc0e08b3384081fee78d3495ec1");

	private static readonly AssetReference VO_BOM_09_004_Male_Kobold_Snivvle_InGame_Turn_03_01_B = new AssetReference("VO_BOM_09_004_Male_Kobold_Snivvle_InGame_Turn_03_01_B.prefab:1605447ee3969124c807e9d7ef358250");

	private static readonly AssetReference VO_BOM_09_004_Male_Kobold_Snivvle_InGame_Turn_05_01_A = new AssetReference("VO_BOM_09_004_Male_Kobold_Snivvle_InGame_Turn_05_01_A.prefab:54816f311556c1345a371d476517725b");

	private static readonly AssetReference VO_BOM_09_004_Male_Kobold_Snivvle_InGame_Turn_05_01_C = new AssetReference("VO_BOM_09_004_Male_Kobold_Snivvle_InGame_Turn_05_01_C.prefab:ea56e45035363b6478978d81d44a765f");

	private static readonly AssetReference VO_BOM_09_004_Male_Kobold_Snivvle_InGame_Turn_09_01_A = new AssetReference("VO_BOM_09_004_Male_Kobold_Snivvle_InGame_Turn_09_01_A.prefab:a2f35e15574629241b321b9221a7e4e0");

	private static readonly AssetReference VO_BOM_09_004_Male_Kobold_Togwaggle_Emote_Attack_01 = new AssetReference("VO_BOM_09_004_Male_Kobold_Togwaggle_Emote_Attack_01.prefab:16552d07305fa4441b45320ebd553d86");

	private static readonly AssetReference VO_BOM_09_004_Male_Kobold_Togwaggle_Emote_Death_01 = new AssetReference("VO_BOM_09_004_Male_Kobold_Togwaggle_Emote_Death_01.prefab:fcb1746dd92c9964eaee99f2030c9c32");

	private static readonly AssetReference VO_BOM_09_004_Male_Kobold_Togwaggle_Emote_Play_01 = new AssetReference("VO_BOM_09_004_Male_Kobold_Togwaggle_Emote_Play_01.prefab:c2dc95d27981baa4c9a65197cb23b1fb");

	private static readonly AssetReference VO_BOM_09_004_Male_Kobold_Togwaggle_InGame_HE_Custom_IfSecondCrateBroken_01_A = new AssetReference("VO_BOM_09_004_Male_Kobold_Togwaggle_InGame_HE_Custom_IfSecondCrateBroken_01_A.prefab:6c29e536eef4b0541a62690549a344ab");

	private static readonly AssetReference VO_BOM_09_004_Male_Kobold_Togwaggle_InGame_Turn_05_01_B = new AssetReference("VO_BOM_09_004_Male_Kobold_Togwaggle_InGame_Turn_05_01_B.prefab:721c991cb4d81924d823e57153957532");

	private static readonly AssetReference VO_BOM_09_004_Male_Kobold_Togwaggle_InGame_VictoryPostExplosion_01_D = new AssetReference("VO_BOM_09_004_Male_Kobold_Togwaggle_InGame_VictoryPostExplosion_01_D.prefab:fc7eac4a96086fc43bced582dbd86c0c");

	private static readonly AssetReference VO_BOM_09_004_Male_Tauren_Guff_InGame_Turn_09_01_B = new AssetReference("VO_BOM_09_004_Male_Tauren_Guff_InGame_Turn_09_01_B.prefab:5e73dd59dc9fd77458185744777f75a6");

	private static readonly AssetReference VO_BOM_09_004_Male_Tauren_Guff_InGame_Turn_09_01_C = new AssetReference("VO_BOM_09_004_Male_Tauren_Guff_InGame_Turn_09_01_C.prefab:d92869c6855ca5444adc0b063e53b460");

	private static readonly AssetReference VO_BOM_09_004_Male_Troll_Brukan_InGame_Introduction_01_A = new AssetReference("VO_BOM_09_004_Male_Troll_Brukan_InGame_Introduction_01_A.prefab:9a9d19bd9ae0b7045b04e13e2f147388");

	private static readonly AssetReference VO_BOM_09_004_Male_Troll_Brukan_InGame_VictoryPostExplosion_01_A = new AssetReference("VO_BOM_09_004_Male_Troll_Brukan_InGame_VictoryPostExplosion_01_A.prefab:3659d92d93429474ba094f1654f84e08");

	private static readonly AssetReference VO_BOM_09_004_X_BloodElf_Dawngrasp_InGame_Turn_11_01_B = new AssetReference("VO_BOM_09_004_X_BloodElf_Dawngrasp_InGame_Turn_11_01_B.prefab:da4c53ad8c6635048972c7bd913fb6fa");

	private List<string> m_InGame_BossIdleLines = new List<string> { VO_BOM_09_004_Male_Kobold_Snivvle_InGame_BossIdle_01, VO_BOM_09_004_Male_Kobold_Snivvle_InGame_BossIdle_02, VO_BOM_09_004_Male_Kobold_Snivvle_InGame_BossIdle_03 };

	private List<string> m_InGame_BossUsesHeroPowerLines = new List<string> { VO_BOM_09_004_Male_Kobold_Snivvle_InGame_BossUsesHeroPower_01, VO_BOM_09_004_Male_Kobold_Snivvle_InGame_BossUsesHeroPower_02 };

	private static readonly AssetReference TogwaggleBrassRing = new AssetReference("Togwaggle_pop-up_BrassRing_Quote.prefab:99e68bee5c488cb45a212327619b0922");

	private HashSet<string> m_playedLines = new HashSet<string>();

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> m_SoundFiles = new List<string>
		{
			VO_BOM_09_004_Female_Human_Cariel_InGame_Turn_11_01_A, VO_BOM_09_004_Female_Human_Cariel_InGame_Turn_11_01_C, VO_BOM_09_004_Female_Human_Cariel_InGame_VictoryPostExplosion_01_B, VO_BOM_09_004_Female_Human_Cariel_InGame_VictoryPostExplosion_01_C, VO_BOM_09_004_Female_Orc_Rokara_InGame_Turn_03_01_A, VO_BOM_09_004_Male_Kobold_Snivvle_InGame_BossDeath_01, VO_BOM_09_004_Male_Kobold_Snivvle_InGame_BossIdle_01, VO_BOM_09_004_Male_Kobold_Snivvle_InGame_BossIdle_02, VO_BOM_09_004_Male_Kobold_Snivvle_InGame_BossIdle_03, VO_BOM_09_004_Male_Kobold_Snivvle_InGame_BossUsesHeroPower_01,
			VO_BOM_09_004_Male_Kobold_Snivvle_InGame_BossUsesHeroPower_02, VO_BOM_09_004_Male_Kobold_Snivvle_InGame_BossUsesHeroPower_03, VO_BOM_09_004_Male_Kobold_Snivvle_InGame_EmoteResponse_01, VO_BOM_09_004_Male_Kobold_Snivvle_InGame_HE_Custom_IfFirstCrateBroken_01_A, VO_BOM_09_004_Male_Kobold_Snivvle_InGame_Introduction_01_B, VO_BOM_09_004_Male_Kobold_Snivvle_InGame_PlayerLoss_01, VO_BOM_09_004_Male_Kobold_Snivvle_InGame_Turn_03_01_B, VO_BOM_09_004_Male_Kobold_Snivvle_InGame_Turn_05_01_A, VO_BOM_09_004_Male_Kobold_Snivvle_InGame_Turn_05_01_C, VO_BOM_09_004_Male_Kobold_Snivvle_InGame_Turn_09_01_A,
			VO_BOM_09_004_Male_Kobold_Togwaggle_InGame_HE_Custom_IfSecondCrateBroken_01_A, VO_BOM_09_004_Male_Kobold_Togwaggle_InGame_Turn_05_01_B, VO_BOM_09_004_Male_Kobold_Togwaggle_InGame_VictoryPostExplosion_01_D, VO_BOM_09_004_Male_Tauren_Guff_InGame_Turn_09_01_B, VO_BOM_09_004_Male_Tauren_Guff_InGame_Turn_09_01_C, VO_BOM_09_004_Male_Troll_Brukan_InGame_Introduction_01_A, VO_BOM_09_004_Male_Troll_Brukan_InGame_VictoryPostExplosion_01_A, VO_BOM_09_004_X_BloodElf_Dawngrasp_InGame_Turn_11_01_B
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
			yield return MissionPlaySound(enemyActor, VO_BOM_09_004_Male_Kobold_Snivvle_InGame_BossDeath_01);
			break;
		case 517:
			yield return MissionPlayVO(enemyActor, m_InGame_BossIdleLines);
			break;
		case 510:
			yield return MissionPlayVO(enemyActor, m_InGame_BossUsesHeroPowerLines);
			break;
		case 515:
			yield return MissionPlayVO(enemyActor, VO_BOM_09_004_Male_Kobold_Snivvle_InGame_EmoteResponse_01);
			break;
		case 514:
			yield return MissionPlayVO(friendlyActor, VO_BOM_09_004_Male_Troll_Brukan_InGame_Introduction_01_A);
			yield return MissionPlayVO(enemyActor, VO_BOM_09_004_Male_Kobold_Snivvle_InGame_Introduction_01_B);
			break;
		case 506:
			MissionPause(pause: true);
			yield return MissionPlayVO(enemyActor, VO_BOM_09_004_Male_Kobold_Snivvle_InGame_PlayerLoss_01);
			MissionPause(pause: false);
			break;
		case 505:
			MissionPause(pause: true);
			yield return MissionPlayVO(friendlyActor, VO_BOM_09_004_Male_Troll_Brukan_InGame_VictoryPostExplosion_01_A);
			yield return MissionPlayVO("BOM_09_Cariel_004t", BOM_09_Brukan_Dungeon.Alterac_CarielArt_BrassRing_Quote, VO_BOM_09_004_Female_Human_Cariel_InGame_VictoryPostExplosion_01_B);
			yield return MissionPlayVO("BOM_09_Cariel_004t", BOM_09_Brukan_Dungeon.Alterac_CarielArt_BrassRing_Quote, VO_BOM_09_004_Female_Human_Cariel_InGame_VictoryPostExplosion_01_C);
			yield return MissionPlayVO(BOM_09_Brukan_Dungeon.MinerTogwaggle_BrassRing_Quote, VO_BOM_09_004_Male_Kobold_Togwaggle_InGame_VictoryPostExplosion_01_D);
			MissionPause(pause: false);
			break;
		case 100:
			yield return MissionPlayVOOnce(enemyActor, VO_BOM_09_004_Male_Kobold_Snivvle_InGame_HE_Custom_IfFirstCrateBroken_01_A);
			break;
		case 101:
			yield return MissionPlayVOOnce(BOM_09_Brukan_Dungeon.MinerTogwaggle_BrassRing_Quote, VO_BOM_09_004_Male_Kobold_Togwaggle_InGame_HE_Custom_IfSecondCrateBroken_01_A);
			break;
		case 102:
			MissionPause(pause: true);
			yield return MissionPlayVO(enemyActor, VO_BOM_09_004_Male_Kobold_Snivvle_InGame_Turn_09_01_A);
			yield return MissionPlayVO(BOM_09_Brukan_Dungeon.Guff_BrassRing_Quote, VO_BOM_09_004_Male_Tauren_Guff_InGame_Turn_09_01_B);
			MissionPause(pause: false);
			break;
		case 103:
			yield return MissionPlayVO(BOM_09_Brukan_Dungeon.Guff_BrassRing_Quote, VO_BOM_09_004_Male_Tauren_Guff_InGame_Turn_09_01_C);
			break;
		case 104:
			MissionPause(pause: true);
			yield return MissionPlayVOOnce(BOM_09_Brukan_Dungeon.Alterac_CarielArt_BrassRing_Quote, VO_BOM_09_004_Female_Human_Cariel_InGame_Turn_11_01_A);
			yield return MissionPlayVOOnce(BOM_09_Brukan_Dungeon.DawngraspTier5_BrassRing_Quote, VO_BOM_09_004_X_BloodElf_Dawngrasp_InGame_Turn_11_01_B);
			yield return MissionPlayVOOnce(BOM_09_Brukan_Dungeon.Alterac_CarielArt_BrassRing_Quote, VO_BOM_09_004_Female_Human_Cariel_InGame_Turn_11_01_C);
			MissionPause(pause: false);
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
			Actor enemyActor = GameState.Get().GetOpposingSidePlayer().GetHero()
				.GetCard()
				.GetActor();
			GameState.Get().GetFriendlySidePlayer().GetHero()
				.GetCard()
				.GetActor();
			if (cardID == "BOM_09_Togwaggle_004t")
			{
				yield return MissionPlayVO(enemyActor, VO_BOM_09_004_Male_Kobold_Snivvle_InGame_Turn_05_01_A);
				yield return MissionPlayVO("BOM_09_Togwaggle_004t", BOM_09_Brukan_Dungeon.MinerTogwaggle_BrassRing_Quote, VO_BOM_09_004_Male_Kobold_Togwaggle_InGame_Turn_05_01_B);
				yield return MissionPlayVO(enemyActor, VO_BOM_09_004_Male_Kobold_Snivvle_InGame_Turn_05_01_C);
			}
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
		if (turn == 3)
		{
			yield return MissionPlayVO("BOM_09_004_Rokara_002t", BOM_09_Brukan_Dungeon.RokaraTier5_BrassRing_Quote, VO_BOM_09_004_Female_Orc_Rokara_InGame_Turn_03_01_A);
			yield return MissionPlayVO(enemyActor, VO_BOM_09_004_Male_Kobold_Snivvle_InGame_Turn_03_01_B);
		}
	}
}
