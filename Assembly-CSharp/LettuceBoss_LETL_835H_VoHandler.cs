using System.Collections;
using System.Collections.Generic;

public class LettuceBoss_LETL_835H_VoHandler : VoPlaybackHandler
{
	private static readonly AssetReference VO_LETL_835H_Male_Elemental_Attack_01 = new AssetReference("VO_LETL_835H_Male_Elemental_Attack_01.prefab:4d133d250746fc2469efd49962d52383");

	private static readonly AssetReference VO_LETL_835H_Male_Elemental_Attack_02 = new AssetReference("VO_LETL_835H_Male_Elemental_Attack_02.prefab:f6478eda37755454b87f3168bde0ae3b");

	private static readonly AssetReference VO_LETL_835H_Male_Elemental_Death_01 = new AssetReference("VO_LETL_835H_Male_Elemental_Death_01.prefab:978a33228da84e542a7426da7b229643");

	private static readonly AssetReference VO_LETL_835H_Male_Elemental_Idle_01 = new AssetReference("VO_LETL_835H_Male_Elemental_Idle_01.prefab:7cdef781eadc7f24f8db8b9a085ee72c");

	private static readonly AssetReference VO_LETL_835H_Male_Elemental_Intro_01 = new AssetReference("VO_LETL_835H_Male_Elemental_Intro_01.prefab:f1392c836ff71254d80ab87bb4c3d256");

	private static readonly AssetReference VO_LETL_835H_Male_Elemental_Intro_02 = new AssetReference("VO_LETL_835H_Male_Elemental_Intro_02.prefab:d311daa691097384a9e3f369807d0fa2");

	private List<string> m_IdleLines = new List<string> { VO_LETL_835H_Male_Elemental_Idle_01 };

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> soundFiles = new List<string> { VO_LETL_835H_Male_Elemental_Attack_01, VO_LETL_835H_Male_Elemental_Attack_02, VO_LETL_835H_Male_Elemental_Death_01, VO_LETL_835H_Male_Elemental_Idle_01, VO_LETL_835H_Male_Elemental_Intro_01, VO_LETL_835H_Male_Elemental_Intro_02 };
		SetBossVOLines(soundFiles);
		foreach (string soundFile in soundFiles)
		{
			GameState.Get().GetGameEntity().PreloadSound(soundFile);
		}
	}

	public override void OnCreateGame()
	{
		base.OnCreateGame();
		m_introLine = VO_LETL_835H_Male_Elemental_Intro_01;
		m_deathLine = VO_LETL_835H_Male_Elemental_Death_01;
	}

	public override IEnumerator RespondToWillPlayCardWithTiming(string cardID, Entity playedEntity)
	{
		while (m_enemySpeaking)
		{
			yield return null;
		}
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_835H");
		if (playedEntity.GetLettuceAbilityOwner().GetCardId() == "LETL_835H")
		{
			switch (cardID)
			{
			case "LETL_835P1_01":
			case "LETL_835P1_02":
				GameState.Get().SetBusy(busy: true);
				yield return MissionPlayVOOnce(bossActor, VO_LETL_835H_Male_Elemental_Attack_01);
				GameState.Get().SetBusy(busy: false);
				break;
			case "LETL_835P2_01":
			case "LETL_835P2_03":
				GameState.Get().SetBusy(busy: true);
				yield return MissionPlayVOOnce(bossActor, VO_LETL_835H_Male_Elemental_Attack_02);
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_835H");
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_835H");
		if (entity.GetCardId() == "LETL_835H")
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_835H");
		if (turn == 1)
		{
			yield return MissionPlayVOOnce(bossActor, m_introLine);
		}
	}
}
