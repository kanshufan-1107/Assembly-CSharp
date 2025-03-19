using System.Collections;
using System.Collections.Generic;

public class LettuceBoss_LETL_818H_VoHandler : VoPlaybackHandler
{
	private static readonly AssetReference VO_LETL_818H_Male_Undead_Attack_01 = new AssetReference("VO_LETL_818H_Male_Undead_Attack_01.prefab:de0c7500417b65e409b7d57d05997302");

	private static readonly AssetReference VO_LETL_818H_Male_Undead_Attack_02 = new AssetReference("VO_LETL_818H_Male_Undead_Attack_02.prefab:8f5dc726acb7ecc409af1a63621bf6a0");

	private static readonly AssetReference VO_LETL_818H_Male_Undead_Death_01 = new AssetReference("VO_LETL_818H_Male_Undead_Death_01.prefab:96eae087ad449fe44b02ec9a5ce52c2a");

	private static readonly AssetReference VO_LETL_818H_Male_Undead_Idle_01 = new AssetReference("VO_LETL_818H_Male_Undead_Idle_01.prefab:1dc2a7b22e03894468bd547cd436d337");

	private static readonly AssetReference VO_LETL_818H_Male_Undead_Idle_02 = new AssetReference("VO_LETL_818H_Male_Undead_Idle_02.prefab:ed0d5f982f037d84b8c604e5db6831e0");

	private static readonly AssetReference VO_LETL_818H_Male_Undead_Intro_01 = new AssetReference("VO_LETL_818H_Male_Undead_Intro_01.prefab:c253249feecde494fa877b6224a52dd2");

	private static readonly AssetReference VO_LETL_818H_Male_Undead_Intro_02 = new AssetReference("VO_LETL_818H_Male_Undead_Intro_02.prefab:4304ddb2dfff225418212626151f9a3e");

	private List<string> m_IdleLines = new List<string> { VO_LETL_818H_Male_Undead_Idle_01, VO_LETL_818H_Male_Undead_Idle_02 };

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> soundFiles = new List<string> { VO_LETL_818H_Male_Undead_Attack_01, VO_LETL_818H_Male_Undead_Attack_02, VO_LETL_818H_Male_Undead_Death_01, VO_LETL_818H_Male_Undead_Idle_01, VO_LETL_818H_Male_Undead_Idle_02, VO_LETL_818H_Male_Undead_Intro_01, VO_LETL_818H_Male_Undead_Intro_02 };
		SetBossVOLines(soundFiles);
		foreach (string soundFile in soundFiles)
		{
			GameState.Get().GetGameEntity().PreloadSound(soundFile);
		}
	}

	public override void OnCreateGame()
	{
		base.OnCreateGame();
		m_introLine = VO_LETL_818H_Male_Undead_Intro_01;
		m_deathLine = VO_LETL_818H_Male_Undead_Death_01;
	}

	public override IEnumerator RespondToWillPlayCardWithTiming(string cardID, Entity playedEntity)
	{
		while (m_enemySpeaking)
		{
			yield return null;
		}
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_818H");
		if (playedEntity.GetLettuceAbilityOwner().GetCardId() == "LETL_818H")
		{
			switch (cardID)
			{
			case "LETL_818P1_01":
				GameState.Get().SetBusy(busy: true);
				yield return MissionPlayVOOnce(bossActor, VO_LETL_818H_Male_Undead_Attack_01);
				GameState.Get().SetBusy(busy: false);
				break;
			case "LETL_818P2_01":
			case "LETL_818P3_01":
				GameState.Get().SetBusy(busy: true);
				yield return MissionPlayVOOnce(bossActor, VO_LETL_818H_Male_Undead_Attack_02);
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_818H");
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_818H");
		if (entity.GetCardId() == "LETL_818H")
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_818H");
		if (turn == 1)
		{
			yield return MissionPlayVOOnce(bossActor, m_introLine);
		}
	}
}
