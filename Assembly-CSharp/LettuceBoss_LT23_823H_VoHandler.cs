using System.Collections;
using System.Collections.Generic;

public class LettuceBoss_LT23_823H_VoHandler : VoPlaybackHandler
{
	private static readonly AssetReference VO_YoggSaron_Male_OldGod_LETL_Attack_03 = new AssetReference("VO_YoggSaron_Male_OldGod_LETL_Attack_03.prefab:a27f80f1fff20f84faab365e4a0e9737");

	private static readonly AssetReference VO_YoggSaron_Male_OldGod_LETL_Attack_04 = new AssetReference("VO_YoggSaron_Male_OldGod_LETL_Attack_04.prefab:954a5567ec701dc4d818272e3975d3fb");

	private static readonly AssetReference VO_YoggSaron_Male_OldGod_LETL_C02_T24_Dialogue_01 = new AssetReference("VO_YoggSaron_Male_OldGod_LETL_C02_T24_Dialogue_01.prefab:61989fc2efa100a498fa9408c985d33e");

	private static readonly AssetReference VO_YoggSaron_Male_OldGod_LETL_Death_02 = new AssetReference("VO_YoggSaron_Male_OldGod_LETL_Death_02.prefab:5546a12e2a7ebcb41af3efec2d501e45");

	private static readonly AssetReference VO_YoggSaron_Male_OldGod_LETL_Idle_01 = new AssetReference("VO_YoggSaron_Male_OldGod_LETL_Idle_01.prefab:ed8a43a97a40a804faaac06c5fd029ae");

	private static readonly AssetReference VO_YoggSaron_Male_OldGod_LETL_Intro_02 = new AssetReference("VO_YoggSaron_Male_OldGod_LETL_Intro_02.prefab:32aa9fc44fa0a8e42b2e866dfecc133e");

	private List<string> m_IdleLines = new List<string> { VO_YoggSaron_Male_OldGod_LETL_Idle_01 };

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> soundFiles = new List<string> { VO_YoggSaron_Male_OldGod_LETL_Intro_02, VO_YoggSaron_Male_OldGod_LETL_Attack_03, VO_YoggSaron_Male_OldGod_LETL_Attack_04, VO_YoggSaron_Male_OldGod_LETL_C02_T24_Dialogue_01, VO_YoggSaron_Male_OldGod_LETL_Idle_01, VO_YoggSaron_Male_OldGod_LETL_Death_02 };
		SetBossVOLines(soundFiles);
		foreach (string soundFile in soundFiles)
		{
			GameState.Get().GetGameEntity().PreloadSound(soundFile);
		}
	}

	public override void OnCreateGame()
	{
		base.OnCreateGame();
		m_introLine = VO_YoggSaron_Male_OldGod_LETL_Intro_02;
		m_deathLine = VO_YoggSaron_Male_OldGod_LETL_Death_02;
	}

	public override IEnumerator RespondToWillPlayCardWithTiming(string cardID, Entity playedEntity)
	{
		while (m_enemySpeaking)
		{
			yield return null;
		}
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LT23_823H");
		if (playedEntity.GetLettuceAbilityOwner().GetCardId() == "LT23_823H")
		{
			switch (cardID)
			{
			case "LT23_823P1":
				GameState.Get().SetBusy(busy: true);
				yield return MissionPlayVOOnce(bossActor, VO_YoggSaron_Male_OldGod_LETL_Attack_03);
				GameState.Get().SetBusy(busy: false);
				break;
			case "LT23_823P2":
				GameState.Get().SetBusy(busy: true);
				yield return MissionPlayVOOnce(bossActor, VO_YoggSaron_Male_OldGod_LETL_Attack_04);
				GameState.Get().SetBusy(busy: false);
				break;
			case "LT23_823P3":
				GameState.Get().SetBusy(busy: true);
				yield return MissionPlayVOOnce(bossActor, VO_YoggSaron_Male_OldGod_LETL_C02_T24_Dialogue_01);
				GameState.Get().SetBusy(busy: false);
				break;
			}
		}
	}

	public override List<string> GetIdleLines()
	{
		return m_IdleLines;
	}

	public override IEnumerator HandleMissionEventWithTiming(int missionEvent)
	{
		while (m_enemySpeaking)
		{
			yield return null;
		}
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LT23_823H");
		switch (missionEvent)
		{
		case 517:
			yield return MissionPlayVO(bossActor, m_IdleLines);
			break;
		case 514:
			yield return MissionPlayVO(bossActor, m_introLine);
			break;
		default:
			yield return base.HandleMissionEventWithTiming(missionEvent);
			break;
		}
	}

	public override void NotifyOfMinionDied(Entity entity)
	{
		Gameplay.Get().StartCoroutine(NotifyOfMinionDiedWithTiming(entity));
	}

	public IEnumerator NotifyOfMinionDiedWithTiming(Entity entity)
	{
		while (m_enemySpeaking)
		{
			yield return null;
		}
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LT23_823H");
		if (entity.GetCardId() == "LT23_823H")
		{
			yield return MissionPlaySound(bossActor, m_deathLine);
		}
	}

	public override IEnumerator HandleStartOfTurnWithTiming(int turn)
	{
		while (m_enemySpeaking)
		{
			yield return null;
		}
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LT23_823H");
		if (turn == 1)
		{
			yield return MissionPlayVOOnce(bossActor, m_introLine);
		}
	}
}
