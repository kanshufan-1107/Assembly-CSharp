using System.Collections;
using System.Collections.Generic;

public class LettuceBoss_LETL_821H_VoHandler : VoPlaybackHandler
{
	private static readonly AssetReference VO_LETL_821H_Male_Goblin_Attack_01 = new AssetReference("VO_LETL_821H_Male_Goblin_Attack_01.prefab:36e96a6853dabe04ca0705dc010b0731");

	private static readonly AssetReference VO_LETL_821H_Male_Goblin_Attack_02 = new AssetReference("VO_LETL_821H_Male_Goblin_Attack_02.prefab:e3b443fc732ece448a830702fe4400c0");

	private static readonly AssetReference VO_LETL_821H_Male_Goblin_Death_01 = new AssetReference("VO_LETL_821H_Male_Goblin_Death_01.prefab:3fb6a14829378dd40b802f2b9dd3cb17");

	private static readonly AssetReference VO_LETL_821H_Male_Goblin_Idle_01 = new AssetReference("VO_LETL_821H_Male_Goblin_Idle_01.prefab:1ed0b24571a0e1e439b840195f224499");

	private static readonly AssetReference VO_LETL_821H_Male_Goblin_Intro_01 = new AssetReference("VO_LETL_821H_Male_Goblin_Intro_01.prefab:e14a156414339c840a640a87a4ffab08");

	private static readonly AssetReference Death = new AssetReference("Death.prefab:f08e4880fab48f3468d9f3b778d2aea7");

	private List<string> m_IdleLines = new List<string> { VO_LETL_821H_Male_Goblin_Idle_01 };

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> soundFiles = new List<string> { VO_LETL_821H_Male_Goblin_Attack_01, VO_LETL_821H_Male_Goblin_Attack_02, VO_LETL_821H_Male_Goblin_Idle_01, VO_LETL_821H_Male_Goblin_Intro_01, Death };
		SetBossVOLines(soundFiles);
		foreach (string soundFile in soundFiles)
		{
			GameState.Get().GetGameEntity().PreloadSound(soundFile);
		}
	}

	public override void OnCreateGame()
	{
		base.OnCreateGame();
		m_introLine = VO_LETL_821H_Male_Goblin_Intro_01;
		m_deathLine = Death;
	}

	public override IEnumerator RespondToWillPlayCardWithTiming(string cardID, Entity playedEntity)
	{
		while (m_enemySpeaking)
		{
			yield return null;
		}
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_821H");
		if (!(playedEntity.GetLettuceAbilityOwner().GetCardId() == "LETL_821H"))
		{
			yield break;
		}
		if (!(cardID == "LETL_821P1_02"))
		{
			if (cardID == "LETL_516_02")
			{
				GameState.Get().SetBusy(busy: true);
				yield return MissionPlayVOOnce(bossActor, VO_LETL_821H_Male_Goblin_Attack_02);
				GameState.Get().SetBusy(busy: false);
			}
		}
		else
		{
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVOOnce(bossActor, VO_LETL_821H_Male_Goblin_Attack_01);
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_821H");
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_821H");
		if (entity.GetCardId() == "LETL_821H")
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_821H");
		if (turn == 1)
		{
			yield return MissionPlayVOOnce(bossActor, m_introLine);
		}
	}
}
