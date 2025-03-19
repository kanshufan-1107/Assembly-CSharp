using System.Collections;
using System.Collections.Generic;

public class LettuceBoss_LETL_825H_VoHandler : VoPlaybackHandler
{
	private static readonly AssetReference VO_LETL_825H_Male_NightElf_Attack_01 = new AssetReference("VO_LETL_825H_Male_NightElf_Attack_01.prefab:1ff5afec68209014b9d85342a90211c5");

	private static readonly AssetReference VO_LETL_825H_Male_NightElf_Attack_02 = new AssetReference("VO_LETL_825H_Male_NightElf_Attack_02.prefab:03361106491539b4bb6981c48a715458");

	private static readonly AssetReference VO_LETL_825H_Male_NightElf_Death_01 = new AssetReference("VO_LETL_825H_Male_NightElf_Death_01.prefab:de5f6dfee266ea443b35048de5195b85");

	private static readonly AssetReference VO_LETL_825H_Male_NightElf_Idle_01 = new AssetReference("VO_LETL_825H_Male_NightElf_Idle_01.prefab:4918879783c682149b0ab19545120d94");

	private static readonly AssetReference VO_LETL_825H_Male_NightElf_Intro_01 = new AssetReference("VO_LETL_825H_Male_NightElf_Intro_01.prefab:defcdae7295849c418dc490ec0977659");

	private List<string> m_IdleLines = new List<string> { VO_LETL_825H_Male_NightElf_Idle_01 };

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> soundFiles = new List<string> { VO_LETL_825H_Male_NightElf_Attack_01, VO_LETL_825H_Male_NightElf_Attack_02, VO_LETL_825H_Male_NightElf_Death_01, VO_LETL_825H_Male_NightElf_Idle_01, VO_LETL_825H_Male_NightElf_Intro_01 };
		SetBossVOLines(soundFiles);
		foreach (string soundFile in soundFiles)
		{
			GameState.Get().GetGameEntity().PreloadSound(soundFile);
		}
	}

	public override void OnCreateGame()
	{
		base.OnCreateGame();
		m_introLine = VO_LETL_825H_Male_NightElf_Intro_01;
		m_deathLine = VO_LETL_825H_Male_NightElf_Death_01;
	}

	public override IEnumerator RespondToWillPlayCardWithTiming(string cardID, Entity playedEntity)
	{
		while (m_enemySpeaking)
		{
			yield return null;
		}
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_825H");
		if (playedEntity.GetLettuceAbilityOwner().GetCardId() == "LETL_825H")
		{
			switch (cardID)
			{
			case "LETL_003P1_02":
			case "LETL_003P1_04":
			case "LETL_412_02":
			case "LETL_412_05":
				GameState.Get().SetBusy(busy: true);
				yield return MissionPlayVOOnce(bossActor, VO_LETL_825H_Male_NightElf_Attack_01);
				GameState.Get().SetBusy(busy: false);
				break;
			case "LETL_019P1_03":
			case "LETL_019P1_05":
				GameState.Get().SetBusy(busy: true);
				yield return MissionPlayVOOnce(bossActor, VO_LETL_825H_Male_NightElf_Attack_02);
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_825H");
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_825H");
		if (entity.GetCardId() == "LETL_825H")
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_825H");
		if (turn == 1)
		{
			yield return MissionPlayVOOnce(bossActor, m_introLine);
		}
	}
}
