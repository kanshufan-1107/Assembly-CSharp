using System.Collections;
using System.Collections.Generic;

public class RLK_Prologue_Fight_004 : RLK_Prologue_Dungeon
{
	private static readonly AssetReference VO_RLK_Prologue_Male_Human_LichKing_InGame_VictoryPostExplosion_04_C = new AssetReference("VO_RLK_Prologue_Male_Human_LichKing_InGame_VictoryPostExplosion_04_C.prefab:6a1dbdc2ea389e24e984498d23a5a481");

	private static readonly AssetReference VO_RLK_Prologue_IllidanD_004hb2_Male_Demon_InGame_VictoryPreExplosion_01 = new AssetReference("VO_RLK_Prologue_IllidanD_004hb2_Male_Demon_InGame_VictoryPreExplosion_01.prefab:4c4531e31c677ee46b5bdd7ae3361f04");

	private static readonly AssetReference VO_RLK_Prologue_0_0_FroznThrn_InGame_VictoryPostExplosion_04_A = new AssetReference("VO_RLK_Prologue_0_0_FroznThrn_InGame_VictoryPostExplosion_04_A.prefab:228a8e7f0b802994b834dbe1c7c02eac");

	private static readonly AssetReference VO_RLK_Prologue_0_0_FroznThrn_InGame_VictoryPostExplosion_04_B = new AssetReference("VO_RLK_Prologue_0_0_FroznThrn_InGame_VictoryPostExplosion_04_B.prefab:07d78f204f0ed664299548f5b39c3b75");

	private static readonly AssetReference VO_RLK_Prologue_Female_Naga_LadyVashj_InGame_Turn_05_04_A = new AssetReference("VO_RLK_Prologue_Female_Naga_LadyVashj_InGame_Turn_05_04_A.prefab:fa1e0d7374af36d488817b7100e5179e");

	private static readonly AssetReference VO_RLK_Prologue_Male_Demon_Illidan_InGame_Turn_19_04_A = new AssetReference("VO_RLK_Prologue_Male_Demon_Illidan_InGame_Turn_19_04_A.prefab:fc8e2c313242e0b47b5c055c64049105");

	private static readonly AssetReference VO_RLK_Prologue_Male_Demon_IllidanD_InGame_BossIdle_04_D = new AssetReference("VO_RLK_Prologue_Male_Demon_IllidanD_InGame_BossIdle_04_D.prefab:dd67159abd1b93948a7382fcc7e02d69");

	private static readonly AssetReference VO_RLK_Prologue_Male_Demon_IllidanD_InGame_BossIdle_04_E = new AssetReference("VO_RLK_Prologue_Male_Demon_IllidanD_InGame_BossIdle_04_E.prefab:1bc38b49976aa904b8df26d7b345b719");

	private static readonly AssetReference VO_RLK_Prologue_Male_Demon_IllidanD_InGame_BossIdle_04_F = new AssetReference("VO_RLK_Prologue_Male_Demon_IllidanD_InGame_BossIdle_04_F.prefab:fe5273a87f8bcd8459d801c07433d5e6");

	private static readonly AssetReference VO_RLK_Prologue_Male_Demon_IllidanD_InGame_EmoteResponse_04_B = new AssetReference("VO_RLK_Prologue_Male_Demon_IllidanD_InGame_EmoteResponse_04_B.prefab:af17e4bc2fc2c25409fdbdf1fdbc7459");

	private static readonly AssetReference VO_RLK_Prologue_Male_Demon_IllidanD_InGame_LossPreExplosion_04_B = new AssetReference("VO_RLK_Prologue_Male_Demon_IllidanD_InGame_LossPreExplosion_04_B.prefab:e54e03aa159cd124388900e8c96a477e");

	private static readonly AssetReference VO_RLK_Prologue_Male_Human_Arthas_InGame_Turn_03_04_A = new AssetReference("VO_RLK_Prologue_Male_Human_Arthas_InGame_Turn_03_04_A.prefab:e938a2c659b7c6148b38c5372eff4803");

	private static readonly AssetReference VO_RLK_Prologue_Male_Human_Arthas_InGame_Turn_11_04_B = new AssetReference("VO_RLK_Prologue_Male_Human_Arthas_InGame_Turn_11_04_B.prefab:2b21fc7067211a44bb35c3678b067735");

	private static readonly AssetReference VO_RLK_Prologue_Male_NightElf_Illidan_InGame_BossAttacksSpecial_04_A = new AssetReference("VO_RLK_Prologue_Male_NightElf_Illidan_InGame_BossAttacksSpecial_04_A.prefab:3abf6184c7dd05d419232b1b48085b5d");

	private static readonly AssetReference VO_RLK_Prologue_Male_NightElf_Illidan_InGame_BossAttacksSpecial_04_B = new AssetReference("VO_RLK_Prologue_Male_NightElf_Illidan_InGame_BossAttacksSpecial_04_B.prefab:acbaa176e23044041b119e6e97f0c4ba");

	private static readonly AssetReference VO_RLK_Prologue_Male_NightElf_Illidan_InGame_BossIdle_04_A = new AssetReference("VO_RLK_Prologue_Male_NightElf_Illidan_InGame_BossIdle_04_A.prefab:3d5b73ecd2c54944a91918645bc09b58");

	private static readonly AssetReference VO_RLK_Prologue_Male_NightElf_Illidan_InGame_BossIdle_04_B = new AssetReference("VO_RLK_Prologue_Male_NightElf_Illidan_InGame_BossIdle_04_B.prefab:55cb5fdb96796bb49bc922fff90cce4b");

	private static readonly AssetReference VO_RLK_Prologue_Male_NightElf_Illidan_InGame_BossIdle_04_C = new AssetReference("VO_RLK_Prologue_Male_NightElf_Illidan_InGame_BossIdle_04_C.prefab:6c1a12148eb12ae4f9575eb176042ff9");

	private static readonly AssetReference VO_RLK_Prologue_Male_NightElf_Illidan_InGame_EmoteResponse_04_A = new AssetReference("VO_RLK_Prologue_Male_NightElf_Illidan_InGame_EmoteResponse_04_A.prefab:50eff948baa8ed845b82446517b63823");

	private static readonly AssetReference VO_RLK_Prologue_Male_NightElf_Illidan_InGame_Introduction_04_A = new AssetReference("VO_RLK_Prologue_Male_NightElf_Illidan_InGame_Introduction_04_A.prefab:da6500f15d2f9b94eaf62fb190d42c4a");

	private static readonly AssetReference VO_RLK_Prologue_Male_NightElf_Illidan_InGame_LossPreExplosion_04_A = new AssetReference("VO_RLK_Prologue_Male_NightElf_Illidan_InGame_LossPreExplosion_04_A.prefab:d38b553a7b746274693bbf35ad57efb3");

	private static readonly AssetReference VO_RLK_Prologue_Male_NightElf_Illidan_InGame_Turn_03_04_B = new AssetReference("VO_RLK_Prologue_Male_NightElf_Illidan_InGame_Turn_03_04_B.prefab:61f91a16f90c37443abde0c77e8e3870");

	private static readonly AssetReference VO_RLK_Prologue_Male_NightElf_Illidan_InGame_Turn_03_04_C = new AssetReference("VO_RLK_Prologue_Male_NightElf_Illidan_InGame_Turn_03_04_C.prefab:31290d22b64e02c49b775ca585cd4f42");

	private static readonly AssetReference VO_RLK_Prologue_Male_NightElf_Illidan_InGame_Turn_11_04_A = new AssetReference("VO_RLK_Prologue_Male_NightElf_Illidan_InGame_Turn_11_04_A.prefab:13924d5c0486f2d49b8eccac314b8123");

	private static readonly AssetReference VO_RLK_Prologue_Male_NightElf_Illidan_InGame_Turn_11_04_C = new AssetReference("VO_RLK_Prologue_Male_NightElf_Illidan_InGame_Turn_11_04_C.prefab:e6ab37efdd1bc7e4bbd1316f2a40f34b");

	private bool bHasTransformationTriggered;

	private List<string> m_InGame_BossIdleLines = new List<string> { VO_RLK_Prologue_Male_NightElf_Illidan_InGame_BossIdle_04_A, VO_RLK_Prologue_Male_NightElf_Illidan_InGame_BossIdle_04_B, VO_RLK_Prologue_Male_NightElf_Illidan_InGame_BossIdle_04_C };

	private List<string> m_InGame_BossSpecialIdleLines = new List<string> { VO_RLK_Prologue_Male_Demon_IllidanD_InGame_BossIdle_04_D, VO_RLK_Prologue_Male_Demon_IllidanD_InGame_BossIdle_04_E, VO_RLK_Prologue_Male_Demon_IllidanD_InGame_BossIdle_04_F };

	private HashSet<string> m_playedLines = new HashSet<string>();

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> m_SoundFiles = new List<string>
		{
			VO_RLK_Prologue_0_0_FroznThrn_InGame_VictoryPostExplosion_04_A, VO_RLK_Prologue_0_0_FroznThrn_InGame_VictoryPostExplosion_04_B, VO_RLK_Prologue_Female_Naga_LadyVashj_InGame_Turn_05_04_A, VO_RLK_Prologue_Male_Human_Arthas_InGame_Turn_03_04_A, VO_RLK_Prologue_Male_Human_Arthas_InGame_Turn_11_04_B, VO_RLK_Prologue_Male_Human_LichKing_InGame_VictoryPostExplosion_04_C, VO_RLK_Prologue_Male_NightElf_Illidan_InGame_BossAttacksSpecial_04_A, VO_RLK_Prologue_Male_NightElf_Illidan_InGame_BossAttacksSpecial_04_B, VO_RLK_Prologue_Male_NightElf_Illidan_InGame_BossIdle_04_A, VO_RLK_Prologue_Male_NightElf_Illidan_InGame_BossIdle_04_B,
			VO_RLK_Prologue_Male_NightElf_Illidan_InGame_BossIdle_04_C, VO_RLK_Prologue_Male_NightElf_Illidan_InGame_EmoteResponse_04_A, VO_RLK_Prologue_Male_NightElf_Illidan_InGame_Introduction_04_A, VO_RLK_Prologue_Male_NightElf_Illidan_InGame_LossPreExplosion_04_A, VO_RLK_Prologue_Male_NightElf_Illidan_InGame_Turn_03_04_B, VO_RLK_Prologue_Male_NightElf_Illidan_InGame_Turn_03_04_C, VO_RLK_Prologue_Male_NightElf_Illidan_InGame_Turn_11_04_A, VO_RLK_Prologue_Male_NightElf_Illidan_InGame_Turn_11_04_C, VO_RLK_Prologue_Male_Demon_Illidan_InGame_Turn_19_04_A, VO_RLK_Prologue_Male_Demon_IllidanD_InGame_BossIdle_04_D,
			VO_RLK_Prologue_Male_Demon_IllidanD_InGame_BossIdle_04_E, VO_RLK_Prologue_Male_Demon_IllidanD_InGame_BossIdle_04_F, VO_RLK_Prologue_Male_Demon_IllidanD_InGame_EmoteResponse_04_B, VO_RLK_Prologue_Male_Demon_IllidanD_InGame_LossPreExplosion_04_B, VO_RLK_Prologue_IllidanD_004hb2_Male_Demon_InGame_VictoryPreExplosion_01
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
		GameState.Get().GetFriendlySidePlayer().GetHero()
			.GetCard()
			.GetActor();
		switch (missionEvent)
		{
		case 517:
			if (bHasTransformationTriggered)
			{
				yield return MissionPlayVO(enemyActor, m_InGame_BossSpecialIdleLines);
			}
			else
			{
				yield return MissionPlayVO(enemyActor, m_InGame_BossIdleLines);
			}
			break;
		case 514:
			yield return MissionPlayVO(enemyActor, VO_RLK_Prologue_Male_NightElf_Illidan_InGame_Introduction_04_A);
			break;
		case 515:
			if (bHasTransformationTriggered)
			{
				yield return MissionPlayVO(enemyActor, VO_RLK_Prologue_Male_Demon_IllidanD_InGame_EmoteResponse_04_B);
			}
			else
			{
				yield return MissionPlayVO(enemyActor, VO_RLK_Prologue_Male_NightElf_Illidan_InGame_EmoteResponse_04_A);
			}
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
			yield return MissionPlayVO(friendlyActor, VO_RLK_Prologue_Male_Human_Arthas_InGame_Turn_03_04_A);
			yield return MissionPlayVO(enemyActor, VO_RLK_Prologue_Male_NightElf_Illidan_InGame_Turn_03_04_B);
			yield return MissionPlayVO(enemyActor, VO_RLK_Prologue_Male_NightElf_Illidan_InGame_Turn_03_04_C);
			break;
		case 9:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(enemyActor, VO_RLK_Prologue_Male_NightElf_Illidan_InGame_Turn_11_04_A);
			yield return MissionPlayVO(friendlyActor, VO_RLK_Prologue_Male_Human_Arthas_InGame_Turn_11_04_B);
			yield return MissionPlayVO(enemyActor, VO_RLK_Prologue_Male_NightElf_Illidan_InGame_Turn_11_04_C);
			GameState.Get().SetBusy(busy: false);
			break;
		case 10:
			bHasTransformationTriggered = true;
			break;
		case 15:
			yield return MissionPlayVO(enemyActor, VO_RLK_Prologue_Male_Demon_Illidan_InGame_Turn_19_04_A);
			break;
		}
	}
}
