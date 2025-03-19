using System.Collections;
using System.Collections.Generic;

public class LettuceBoss_LETL_824H_VoHandler : VoPlaybackHandler
{
	private static readonly AssetReference VO_LETL_824H_Male_Furbolg_Attack_01 = new AssetReference("VO_LETL_824H_Male_Furbolg_Attack_01.prefab:e2a6c3b3556edfc44b40e34ee4f1ae7a");

	private static readonly AssetReference VO_LETL_824H_Male_Furbolg_Attack_02 = new AssetReference("VO_LETL_824H_Male_Furbolg_Attack_02.prefab:21e87934c573de64cbcfac43c6616096");

	private static readonly AssetReference VO_LETL_824H_Male_Furbolg_Death_01 = new AssetReference("VO_LETL_824H_Male_Furbolg_Death_01.prefab:88cf91b7cd5685d4e8dcc1d74ff621af");

	private static readonly AssetReference VO_LETL_824H_Male_Furbolg_Idle_01 = new AssetReference("VO_LETL_824H_Male_Furbolg_Idle_01.prefab:7dfd4fef40026e34b820595c14e8fbcc");

	private static readonly AssetReference VO_LETL_824H_Male_Furbolg_Intro_01 = new AssetReference("VO_LETL_824H_Male_Furbolg_Intro_01.prefab:cc2be53dff13ddf499b42ee90b8a86e6");

	private List<string> m_IdleLines = new List<string> { VO_LETL_824H_Male_Furbolg_Idle_01 };

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> soundFiles = new List<string> { VO_LETL_824H_Male_Furbolg_Attack_01, VO_LETL_824H_Male_Furbolg_Attack_02, VO_LETL_824H_Male_Furbolg_Death_01, VO_LETL_824H_Male_Furbolg_Idle_01, VO_LETL_824H_Male_Furbolg_Intro_01 };
		SetBossVOLines(soundFiles);
		foreach (string soundFile in soundFiles)
		{
			GameState.Get().GetGameEntity().PreloadSound(soundFile);
		}
	}

	public override void OnCreateGame()
	{
		base.OnCreateGame();
		m_introLine = VO_LETL_824H_Male_Furbolg_Intro_01;
		m_deathLine = VO_LETL_824H_Male_Furbolg_Death_01;
	}

	public override IEnumerator RespondToWillPlayCardWithTiming(string cardID, Entity playedEntity)
	{
		while (m_enemySpeaking)
		{
			yield return null;
		}
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_824H");
		if (!(playedEntity.GetLettuceAbilityOwner().GetCardId() == "LETL_824H"))
		{
			yield break;
		}
		if (!(cardID == "LETL_824P1_01"))
		{
			if (cardID == "LETL_82P3_02")
			{
				GameState.Get().SetBusy(busy: true);
				yield return MissionPlayVOOnce(bossActor, VO_LETL_824H_Male_Furbolg_Attack_02);
				GameState.Get().SetBusy(busy: false);
			}
		}
		else
		{
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVOOnce(bossActor, VO_LETL_824H_Male_Furbolg_Attack_01);
			GameState.Get().SetBusy(busy: false);
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_824H");
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_824H");
		if (entity.GetCardId() == "LETL_824H")
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_824H");
		if (turn == 1)
		{
			yield return MissionPlayVOOnce(bossActor, m_introLine);
		}
	}
}
