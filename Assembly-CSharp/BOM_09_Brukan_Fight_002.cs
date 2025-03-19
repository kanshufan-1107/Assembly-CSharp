using System.Collections;
using System.Collections.Generic;

public class BOM_09_Brukan_Fight_002 : BOM_09_Brukan_Dungeon
{
	private static readonly AssetReference VO_BOM_09_002_Female_Orc_Rokara_InGame_HE_Custom_AfterFourthWaveCleared_01_A = new AssetReference("VO_BOM_09_002_Female_Orc_Rokara_InGame_HE_Custom_AfterFourthWaveCleared_01_A.prefab:28ac4a67cc31db34bbc79a39d24ae588");

	private static readonly AssetReference VO_BOM_09_002_Female_Orc_Rokara_InGame_HE_Custom_AfterThirdWaveCleared_01_A = new AssetReference("VO_BOM_09_002_Female_Orc_Rokara_InGame_HE_Custom_AfterThirdWaveCleared_01_A.prefab:18654602d29aa6341bff8394eb975429");

	private static readonly AssetReference VO_BOM_09_002_Female_Orc_Rokara_InGame_Turn_03_01_A = new AssetReference("VO_BOM_09_002_Female_Orc_Rokara_InGame_Turn_03_01_A.prefab:ec793787c5e734b4b9b900b8e215a94e");

	private static readonly AssetReference VO_BOM_09_002_Female_Orc_Rokara_InGame_Turn_03_01_B = new AssetReference("VO_BOM_09_002_Female_Orc_Rokara_InGame_Turn_03_01_B.prefab:2f96046b41ddb9143bcc52c802022f05");

	private static readonly AssetReference VO_BOM_09_002_Female_Orc_Rokara_InGame_VictoryPreExplosion_01_C = new AssetReference("VO_BOM_09_002_Female_Orc_Rokara_InGame_VictoryPreExplosion_01_C.prefab:256c683a71526d94ca61b2bbb878c1b9");

	private static readonly AssetReference VO_BOM_09_002_Male_Tauren_Guff_Emote_Attack_01 = new AssetReference("VO_BOM_09_002_Male_Tauren_Guff_Emote_Attack_01.prefab:e8b721f8ca5722f43a1d431fbcca131c");

	private static readonly AssetReference VO_BOM_09_002_Male_Tauren_Guff_Emote_Play_01 = new AssetReference("VO_BOM_09_002_Male_Tauren_Guff_Emote_Play_01.prefab:76c044981d76fbb4baf6774f6bc0b46f");

	private static readonly AssetReference VO_BOM_09_002_Male_Tauren_Guff_InGame_HE_Custom_AfterFirstWaveCleared_01_B = new AssetReference("VO_BOM_09_002_Male_Tauren_Guff_InGame_HE_Custom_AfterFirstWaveCleared_01_B.prefab:74eac570430a36f43a8f9411e1c49a76");

	private static readonly AssetReference VO_BOM_09_002_Male_Tauren_Guff_InGame_Introduction_01_B = new AssetReference("VO_BOM_09_002_Male_Tauren_Guff_InGame_Introduction_01_B.prefab:9f8f995aee2fb6f4ea46e1f63ef5a3fe");

	private static readonly AssetReference VO_BOM_09_002_Male_Troll_Brukan_InGame_HE_Custom_AfterFirstWaveCleared_01_A = new AssetReference("VO_BOM_09_002_Male_Troll_Brukan_InGame_HE_Custom_AfterFirstWaveCleared_01_A.prefab:787dd0606a835aa4ea86e68d81785142");

	private static readonly AssetReference VO_BOM_09_002_Male_Troll_Brukan_InGame_HE_Custom_AfterFirstWaveCleared_01_C = new AssetReference("VO_BOM_09_002_Male_Troll_Brukan_InGame_HE_Custom_AfterFirstWaveCleared_01_C.prefab:c5677e2ded341d346a83df28d1f5d15b");

	private static readonly AssetReference VO_BOM_09_002_Male_Troll_Brukan_InGame_HE_Custom_AfterSecondWaveCleared_01_C = new AssetReference("VO_BOM_09_002_Male_Troll_Brukan_InGame_HE_Custom_AfterSecondWaveCleared_01_C.prefab:6cdcb30097e172c46a30e88f14932ff6");

	private static readonly AssetReference VO_BOM_09_002_Male_Troll_Brukan_InGame_HE_Custom_AfterThirdWaveCleared_01_B = new AssetReference("VO_BOM_09_002_Male_Troll_Brukan_InGame_HE_Custom_AfterThirdWaveCleared_01_B.prefab:afdff33b6c165bf41903b21bc144eb16");

	private static readonly AssetReference VO_BOM_09_002_Male_Troll_Brukan_InGame_HE_Custom_AfterThirdWaveCleared_01_C = new AssetReference("VO_BOM_09_002_Male_Troll_Brukan_InGame_HE_Custom_AfterThirdWaveCleared_01_C.prefab:3963e05c27313dd478b31d3bbc805a19");

	private static readonly AssetReference VO_BOM_09_002_Male_Troll_Brukan_InGame_Introduction_01_A = new AssetReference("VO_BOM_09_002_Male_Troll_Brukan_InGame_Introduction_01_A.prefab:713d37ac0bfa9984fa6787e91d5ecb09");

	private static readonly AssetReference VO_BOM_09_002_Male_Troll_Brukan_InGame_VictoryPreExplosion_01_A = new AssetReference("VO_BOM_09_002_Male_Troll_Brukan_InGame_VictoryPreExplosion_01_A.prefab:006da0ba6cbfaef46b1353957eee3477");

	private static readonly AssetReference VO_BOM_09_002_Male_Troll_Brukan_InGame_VictoryPreExplosion_01_B = new AssetReference("VO_BOM_09_002_Male_Troll_Brukan_InGame_VictoryPreExplosion_01_B.prefab:b61bb4f7816520d489398b068a3f0e33");

	private static readonly AssetReference VO_BOM_09_002_X_BloodElf_Dawngrasp_InGame_HE_Custom_AfterFourthWaveCleared_01_B = new AssetReference("VO_BOM_09_002_X_BloodElf_Dawngrasp_InGame_HE_Custom_AfterFourthWaveCleared_01_B.prefab:5f2389d35dcc1864497fe8d1de73a5e7");

	private static readonly AssetReference VO_BOM_09_002_X_BloodElf_Dawngrasp_InGame_HE_Custom_AfterFourthWaveCleared_01_C = new AssetReference("VO_BOM_09_002_X_BloodElf_Dawngrasp_InGame_HE_Custom_AfterFourthWaveCleared_01_C.prefab:96c3bdf5b4ae2114c939fca63d603eca");

	private static readonly AssetReference VO_BOM_09_002_X_BloodElf_Dawngrasp_InGame_HE_Custom_AfterSecondWaveCleared_01_A = new AssetReference("VO_BOM_09_002_X_BloodElf_Dawngrasp_InGame_HE_Custom_AfterSecondWaveCleared_01_A.prefab:3bb66f70d8907d64d90d6a4106a54411");

	private static readonly AssetReference VO_BOM_09_002_X_BloodElf_Dawngrasp_InGame_HE_Custom_AfterSecondWaveCleared_01_B = new AssetReference("VO_BOM_09_002_X_BloodElf_Dawngrasp_InGame_HE_Custom_AfterSecondWaveCleared_01_B.prefab:d8e54d10cb2f52845a24410c6b25e70c");

	private HashSet<string> m_playedLines = new HashSet<string>();

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> m_SoundFiles = new List<string>
		{
			VO_BOM_09_002_Female_Orc_Rokara_InGame_HE_Custom_AfterFourthWaveCleared_01_A, VO_BOM_09_002_Female_Orc_Rokara_InGame_HE_Custom_AfterThirdWaveCleared_01_A, VO_BOM_09_002_Female_Orc_Rokara_InGame_Turn_03_01_A, VO_BOM_09_002_Female_Orc_Rokara_InGame_Turn_03_01_B, VO_BOM_09_002_Female_Orc_Rokara_InGame_VictoryPreExplosion_01_C, VO_BOM_09_002_Male_Tauren_Guff_InGame_HE_Custom_AfterFirstWaveCleared_01_B, VO_BOM_09_002_Male_Tauren_Guff_InGame_Introduction_01_B, VO_BOM_09_002_Male_Troll_Brukan_InGame_HE_Custom_AfterFirstWaveCleared_01_A, VO_BOM_09_002_Male_Troll_Brukan_InGame_HE_Custom_AfterFirstWaveCleared_01_C, VO_BOM_09_002_Male_Troll_Brukan_InGame_HE_Custom_AfterSecondWaveCleared_01_C,
			VO_BOM_09_002_Male_Troll_Brukan_InGame_HE_Custom_AfterThirdWaveCleared_01_B, VO_BOM_09_002_Male_Troll_Brukan_InGame_HE_Custom_AfterThirdWaveCleared_01_C, VO_BOM_09_002_Male_Troll_Brukan_InGame_Introduction_01_A, VO_BOM_09_002_Male_Troll_Brukan_InGame_VictoryPreExplosion_01_A, VO_BOM_09_002_Male_Troll_Brukan_InGame_VictoryPreExplosion_01_B, VO_BOM_09_002_X_BloodElf_Dawngrasp_InGame_HE_Custom_AfterFourthWaveCleared_01_B, VO_BOM_09_002_X_BloodElf_Dawngrasp_InGame_HE_Custom_AfterFourthWaveCleared_01_C, VO_BOM_09_002_X_BloodElf_Dawngrasp_InGame_HE_Custom_AfterSecondWaveCleared_01_A, VO_BOM_09_002_X_BloodElf_Dawngrasp_InGame_HE_Custom_AfterSecondWaveCleared_01_B
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
		GameState.Get().GetOpposingSidePlayer().GetHero()
			.GetCard()
			.GetActor();
		Actor friendlyActor = GameState.Get().GetFriendlySidePlayer().GetHero()
			.GetCard()
			.GetActor();
		switch (missionEvent)
		{
		case 504:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(friendlyActor, VO_BOM_09_002_Male_Troll_Brukan_InGame_VictoryPreExplosion_01_A);
			yield return MissionPlayVO(friendlyActor, VO_BOM_09_002_Male_Troll_Brukan_InGame_VictoryPreExplosion_01_B);
			yield return MissionPlayVO(BOM_09_Brukan_Dungeon.RokaraTier5_BrassRing_Quote, VO_BOM_09_002_Female_Orc_Rokara_InGame_VictoryPreExplosion_01_C);
			GameState.Get().SetBusy(busy: false);
			break;
		case 101:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVOOnce(friendlyActor, VO_BOM_09_002_Male_Troll_Brukan_InGame_HE_Custom_AfterFirstWaveCleared_01_A);
			yield return MissionPlayVOOnce(BOM_09_Brukan_Dungeon.Guff_BrassRing_Quote, VO_BOM_09_002_Male_Tauren_Guff_InGame_HE_Custom_AfterFirstWaveCleared_01_B);
			yield return MissionPlayVOOnce(friendlyActor, VO_BOM_09_002_Male_Troll_Brukan_InGame_HE_Custom_AfterFirstWaveCleared_01_C);
			GameState.Get().SetBusy(busy: false);
			break;
		case 102:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVOOnce(BOM_09_Brukan_Dungeon.DawngraspTier5_BrassRing_Quote, VO_BOM_09_002_X_BloodElf_Dawngrasp_InGame_HE_Custom_AfterSecondWaveCleared_01_A);
			yield return MissionPlayVOOnce(BOM_09_Brukan_Dungeon.DawngraspTier5_BrassRing_Quote, VO_BOM_09_002_X_BloodElf_Dawngrasp_InGame_HE_Custom_AfterSecondWaveCleared_01_B);
			yield return MissionPlayVOOnce(friendlyActor, VO_BOM_09_002_Male_Troll_Brukan_InGame_HE_Custom_AfterSecondWaveCleared_01_C);
			GameState.Get().SetBusy(busy: false);
			break;
		case 103:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVOOnce(BOM_09_Brukan_Dungeon.RokaraTier5_BrassRing_Quote, VO_BOM_09_002_Female_Orc_Rokara_InGame_HE_Custom_AfterThirdWaveCleared_01_A);
			yield return MissionPlayVOOnce(friendlyActor, VO_BOM_09_002_Male_Troll_Brukan_InGame_HE_Custom_AfterThirdWaveCleared_01_B);
			yield return MissionPlayVOOnce(friendlyActor, VO_BOM_09_002_Male_Troll_Brukan_InGame_HE_Custom_AfterThirdWaveCleared_01_C);
			GameState.Get().SetBusy(busy: false);
			break;
		case 104:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVOOnce(BOM_09_Brukan_Dungeon.RokaraTier5_BrassRing_Quote, VO_BOM_09_002_Female_Orc_Rokara_InGame_HE_Custom_AfterFourthWaveCleared_01_A);
			yield return MissionPlayVOOnce(BOM_09_Brukan_Dungeon.DawngraspTier5_BrassRing_Quote, VO_BOM_09_002_X_BloodElf_Dawngrasp_InGame_HE_Custom_AfterFourthWaveCleared_01_B);
			yield return MissionPlayVOOnce(BOM_09_Brukan_Dungeon.DawngraspTier5_BrassRing_Quote, VO_BOM_09_002_X_BloodElf_Dawngrasp_InGame_HE_Custom_AfterFourthWaveCleared_01_C);
			GameState.Get().SetBusy(busy: false);
			break;
		default:
			yield return base.HandleMissionEventWithTiming(missionEvent);
			break;
		case 514:
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
		if (turn == 1)
		{
			yield return MissionPlayVO(friendlyActor, VO_BOM_09_002_Male_Troll_Brukan_InGame_Introduction_01_A);
			yield return MissionPlayVO(BOM_09_Brukan_Dungeon.Guff_BrassRing_Quote, VO_BOM_09_002_Male_Tauren_Guff_InGame_Introduction_01_B);
			yield return MissionPlayVO(BOM_09_Brukan_Dungeon.RokaraTier5_BrassRing_Quote, VO_BOM_09_002_Female_Orc_Rokara_InGame_Turn_03_01_A);
			yield return MissionPlayVO(BOM_09_Brukan_Dungeon.RokaraTier5_BrassRing_Quote, VO_BOM_09_002_Female_Orc_Rokara_InGame_Turn_03_01_B);
		}
	}
}
