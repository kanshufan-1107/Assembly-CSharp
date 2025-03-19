using System.Collections;
using System.Collections.Generic;

public class BOM_09_Brukan_Fight_006 : BOM_09_Brukan_Dungeon
{
	private static readonly AssetReference VO_BOM_09_006_Female_Human_Cariel_InGame_Turn_07_01_A = new AssetReference("VO_BOM_09_006_Female_Human_Cariel_InGame_Turn_07_01_A.prefab:c218c0fc957e5124d895e7884a1df6d4");

	private static readonly AssetReference VO_BOM_09_006_Female_Orc_Rokara_InGame_Turn_07_01_B = new AssetReference("VO_BOM_09_006_Female_Orc_Rokara_InGame_Turn_07_01_B.prefab:40d8abb07cb8d2a418c44ea33febc53a");

	private static readonly AssetReference VO_BOM_09_006_Male_Elemental_Lokholar_InGame_BossIdle_01 = new AssetReference("VO_BOM_09_006_Male_Elemental_Lokholar_InGame_BossIdle_01.prefab:65934244265b9474299a3cd16080f15e");

	private static readonly AssetReference VO_BOM_09_006_Male_Elemental_Lokholar_InGame_BossIdle_02 = new AssetReference("VO_BOM_09_006_Male_Elemental_Lokholar_InGame_BossIdle_02.prefab:83cba3fe482afbd4387ef18ae65d8a7e");

	private static readonly AssetReference VO_BOM_09_006_Male_Elemental_Lokholar_InGame_BossIdle_03 = new AssetReference("VO_BOM_09_006_Male_Elemental_Lokholar_InGame_BossIdle_03.prefab:edf2d3b81eb8092468e29b75f3fb1ce1");

	private static readonly AssetReference VO_BOM_09_006_Male_Elemental_Lokholar_InGame_BossUsesHeroPower_01 = new AssetReference("VO_BOM_09_006_Male_Elemental_Lokholar_InGame_BossUsesHeroPower_01.prefab:680d8a663a817614d934e97283506bc0");

	private static readonly AssetReference VO_BOM_09_006_Male_Elemental_Lokholar_InGame_BossUsesHeroPower_02 = new AssetReference("VO_BOM_09_006_Male_Elemental_Lokholar_InGame_BossUsesHeroPower_02.prefab:36f33b29a0e9bef41a0e7ba20b6f5b47");

	private static readonly AssetReference VO_BOM_09_006_Male_Elemental_Lokholar_InGame_BossUsesHeroPower_03 = new AssetReference("VO_BOM_09_006_Male_Elemental_Lokholar_InGame_BossUsesHeroPower_03.prefab:a5df506c5cb316e448a583b4754b4169");

	private static readonly AssetReference VO_BOM_09_006_Male_Elemental_Lokholar_InGame_EmoteResponse_01 = new AssetReference("VO_BOM_09_006_Male_Elemental_Lokholar_InGame_EmoteResponse_01.prefab:8610e4af1611f2d4b9be034d6a243962");

	private static readonly AssetReference VO_BOM_09_006_Male_Elemental_Lokholar_InGame_Introduction_01_A = new AssetReference("VO_BOM_09_006_Male_Elemental_Lokholar_InGame_Introduction_01_A.prefab:a5efd5e5393fffd47bd36bbee15bc9fe");

	private static readonly AssetReference VO_BOM_09_006_Male_Elemental_Lokholar_InGame_PlayerLoss_01 = new AssetReference("VO_BOM_09_006_Male_Elemental_Lokholar_InGame_PlayerLoss_01.prefab:a6d2dd706cbc37741829a148a137e9da");

	private static readonly AssetReference VO_BOM_09_006_Male_Elemental_Lokholar_InGame_Turn_03_01_A = new AssetReference("VO_BOM_09_006_Male_Elemental_Lokholar_InGame_Turn_03_01_A.prefab:abc90138fd76ae44d861e94920e4d23f");

	private static readonly AssetReference VO_BOM_09_006_Male_Elemental_Lokholar_InGame_VictoryPreExplosion_01_A = new AssetReference("VO_BOM_09_006_Male_Elemental_Lokholar_InGame_VictoryPreExplosion_01_A.prefab:d51e26b1145cb184eaef1fcb80215347");

	private static readonly AssetReference VO_BOM_09_006_Male_Tauren_Guff_InGame_HE_Custom_IfGuffDealtLethal_01_B = new AssetReference("VO_BOM_09_006_Male_Tauren_Guff_InGame_HE_Custom_IfGuffDealtLethal_01_B.prefab:75d578fe38391b64895944b3e3f59efc");

	private static readonly AssetReference VO_BOM_09_006_Male_Troll_Brukan_InGame_HE_Custom_IfGuffDealtLethal_01_A = new AssetReference("VO_BOM_09_006_Male_Troll_Brukan_InGame_HE_Custom_IfGuffDealtLethal_01_A.prefab:2b0a68287119f294e840fbb03fffee83");

	private static readonly AssetReference VO_BOM_09_006_Male_Troll_Brukan_InGame_HE_Custom_IfGuffDealtLethal_01_C = new AssetReference("VO_BOM_09_006_Male_Troll_Brukan_InGame_HE_Custom_IfGuffDealtLethal_01_C.prefab:93317e59c5ad3b246a366260c86511fc");

	private static readonly AssetReference VO_BOM_09_006_Male_Troll_Brukan_InGame_Introduction_01_B = new AssetReference("VO_BOM_09_006_Male_Troll_Brukan_InGame_Introduction_01_B.prefab:ab6bb459b6e8b234fa0d2f54e3074f69");

	private static readonly AssetReference VO_BOM_09_006_Male_Troll_Brukan_InGame_Turn_03_01_B = new AssetReference("VO_BOM_09_006_Male_Troll_Brukan_InGame_Turn_03_01_B.prefab:7e1e354078a08b4458aac6ee3aa5a42b");

	private static readonly AssetReference VO_BOM_09_006_Male_Troll_Brukan_InGame_Turn_11_01_A = new AssetReference("VO_BOM_09_006_Male_Troll_Brukan_InGame_Turn_11_01_A.prefab:94a54a771dbad7c4192bdf13787d72b7");

	private static readonly AssetReference VO_BOM_09_006_Male_Troll_Brukan_InGame_VictoryPreExplosion_01_B = new AssetReference("VO_BOM_09_006_Male_Troll_Brukan_InGame_VictoryPreExplosion_01_B.prefab:8652ec7f71b77814cbe3659ce2626b4a");

	private static readonly AssetReference VO_BOM_09_006_X_BloodElf_Dawngrasp_InGame_Turn_11_01_B = new AssetReference("VO_BOM_09_006_X_BloodElf_Dawngrasp_InGame_Turn_11_01_B.prefab:7fbff9a3c74f4e248a4ba90f547d89b6");

	private List<string> m_InGame_BossIdleLines = new List<string> { VO_BOM_09_006_Male_Elemental_Lokholar_InGame_BossIdle_01, VO_BOM_09_006_Male_Elemental_Lokholar_InGame_BossIdle_02, VO_BOM_09_006_Male_Elemental_Lokholar_InGame_BossIdle_03 };

	private List<string> m_InGame_BossUsesHeroPowerLines = new List<string> { VO_BOM_09_006_Male_Elemental_Lokholar_InGame_BossUsesHeroPower_01, VO_BOM_09_006_Male_Elemental_Lokholar_InGame_BossUsesHeroPower_02, VO_BOM_09_006_Male_Elemental_Lokholar_InGame_BossUsesHeroPower_03 };

	private HashSet<string> m_playedLines = new HashSet<string>();

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> m_SoundFiles = new List<string>
		{
			VO_BOM_09_006_Female_Human_Cariel_InGame_Turn_07_01_A, VO_BOM_09_006_Female_Orc_Rokara_InGame_Turn_07_01_B, VO_BOM_09_006_Male_Elemental_Lokholar_InGame_BossIdle_01, VO_BOM_09_006_Male_Elemental_Lokholar_InGame_BossIdle_02, VO_BOM_09_006_Male_Elemental_Lokholar_InGame_BossIdle_03, VO_BOM_09_006_Male_Elemental_Lokholar_InGame_BossUsesHeroPower_01, VO_BOM_09_006_Male_Elemental_Lokholar_InGame_BossUsesHeroPower_02, VO_BOM_09_006_Male_Elemental_Lokholar_InGame_BossUsesHeroPower_03, VO_BOM_09_006_Male_Elemental_Lokholar_InGame_EmoteResponse_01, VO_BOM_09_006_Male_Elemental_Lokholar_InGame_Introduction_01_A,
			VO_BOM_09_006_Male_Elemental_Lokholar_InGame_PlayerLoss_01, VO_BOM_09_006_Male_Elemental_Lokholar_InGame_Turn_03_01_A, VO_BOM_09_006_Male_Elemental_Lokholar_InGame_VictoryPreExplosion_01_A, VO_BOM_09_006_Male_Tauren_Guff_InGame_HE_Custom_IfGuffDealtLethal_01_B, VO_BOM_09_006_Male_Troll_Brukan_InGame_HE_Custom_IfGuffDealtLethal_01_A, VO_BOM_09_006_Male_Troll_Brukan_InGame_HE_Custom_IfGuffDealtLethal_01_C, VO_BOM_09_006_Male_Troll_Brukan_InGame_Introduction_01_B, VO_BOM_09_006_Male_Troll_Brukan_InGame_Turn_03_01_B, VO_BOM_09_006_Male_Troll_Brukan_InGame_Turn_11_01_A, VO_BOM_09_006_Male_Troll_Brukan_InGame_VictoryPreExplosion_01_B,
			VO_BOM_09_006_X_BloodElf_Dawngrasp_InGame_Turn_11_01_B
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
			yield return MissionPlayVO(enemyActor, VO_BOM_09_006_Male_Elemental_Lokholar_InGame_EmoteResponse_01);
			break;
		case 514:
			yield return MissionPlayVO(enemyActor, VO_BOM_09_006_Male_Elemental_Lokholar_InGame_Introduction_01_A);
			yield return MissionPlayVO(friendlyActor, VO_BOM_09_006_Male_Troll_Brukan_InGame_Introduction_01_B);
			break;
		case 506:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(enemyActor, VO_BOM_09_006_Male_Elemental_Lokholar_InGame_PlayerLoss_01);
			GameState.Get().SetBusy(busy: false);
			break;
		case 504:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(enemyActor, VO_BOM_09_006_Male_Elemental_Lokholar_InGame_VictoryPreExplosion_01_A);
			yield return MissionPlayVO(friendlyActor, VO_BOM_09_006_Male_Troll_Brukan_InGame_VictoryPreExplosion_01_B);
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
			yield return MissionPlayVO(enemyActor, VO_BOM_09_006_Male_Elemental_Lokholar_InGame_Turn_03_01_A);
			yield return MissionPlayVO(friendlyActor, VO_BOM_09_006_Male_Troll_Brukan_InGame_Turn_03_01_B);
			break;
		case 7:
			yield return MissionPlayVO(BOM_09_Brukan_Dungeon.Alterac_CarielArt_BrassRing_Quote, VO_BOM_09_006_Female_Human_Cariel_InGame_Turn_07_01_A);
			yield return MissionPlayVO(BOM_09_Brukan_Dungeon.RokaraTier5_BrassRing_Quote, VO_BOM_09_006_Female_Orc_Rokara_InGame_Turn_07_01_B);
			break;
		case 11:
			yield return MissionPlayVO(friendlyActor, VO_BOM_09_006_Male_Troll_Brukan_InGame_Turn_11_01_A);
			yield return MissionPlayVO(BOM_09_Brukan_Dungeon.DawngraspTier5_BrassRing_Quote, VO_BOM_09_006_X_BloodElf_Dawngrasp_InGame_Turn_11_01_B);
			break;
		}
	}
}
