using System.Collections;
using System.Collections.Generic;

public class LettuceBoss_LETL_866H_VoHandler : VoPlaybackHandler
{
	private static readonly AssetReference VO_Mida_Female_Naaru_LETL_Intro_01 = new AssetReference("VO_Mida_Female_Naaru_LETL_Intro_01.prefab:00f3be348907ca94db4c49c4225d85a0");

	private static readonly AssetReference VO_Mida_Female_Naaru_LETL_Idle_03 = new AssetReference("VO_Mida_Female_Naaru_LETL_Idle_03.prefab:6fa7b23a5cd77af4f91ae2f18b6c880d");

	private static readonly AssetReference VO_Mida_Female_Naaru_LETL_Idle_02 = new AssetReference("VO_Mida_Female_Naaru_LETL_Idle_02.prefab:e1d8614bcb6cc694ebeb5c93608ea050");

	private static readonly AssetReference VO_Mida_Female_Naaru_LETL_Idle_01 = new AssetReference("VO_Mida_Female_Naaru_LETL_Idle_01.prefab:66679ed14d285f841a7f8830cc1f2b55");

	private static readonly AssetReference VO_Mida_Female_Naaru_LETL_Death_01 = new AssetReference("VO_Mida_Female_Naaru_LETL_Death_01.prefab:1c039ebea8a6f734ba5b9cf2618a6cef");

	private static readonly AssetReference VO_Mida_Female_Naaru_LETL_Ability_02 = new AssetReference("VO_Mida_Female_Naaru_LETL_Ability_02.prefab:1704cffae7bb9ff4c93b7b4e60c89ff1");

	private static readonly AssetReference VO_Mida_Female_Naaru_LETL_Ability_01 = new AssetReference("VO_Mida_Female_Naaru_LETL_Ability_01.prefab:be18da211e0914642aebed0c921e2019");

	private List<string> m_IdleLines = new List<string> { VO_Mida_Female_Naaru_LETL_Idle_03, VO_Mida_Female_Naaru_LETL_Idle_02, VO_Mida_Female_Naaru_LETL_Idle_01 };

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> soundFiles = new List<string> { VO_Mida_Female_Naaru_LETL_Intro_01, VO_Mida_Female_Naaru_LETL_Idle_03, VO_Mida_Female_Naaru_LETL_Idle_02, VO_Mida_Female_Naaru_LETL_Idle_01, VO_Mida_Female_Naaru_LETL_Death_01, VO_Mida_Female_Naaru_LETL_Ability_02, VO_Mida_Female_Naaru_LETL_Ability_01 };
		SetBossVOLines(soundFiles);
		foreach (string soundFile in soundFiles)
		{
			GameState.Get().GetGameEntity().PreloadSound(soundFile);
		}
	}

	public override void OnCreateGame()
	{
		base.OnCreateGame();
		m_introLine = VO_Mida_Female_Naaru_LETL_Intro_01;
		m_deathLine = VO_Mida_Female_Naaru_LETL_Death_01;
	}

	public override IEnumerator RespondToWillPlayCardWithTiming(string cardID, Entity playedEntity)
	{
		while (m_enemySpeaking)
		{
			yield return null;
		}
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_866H");
		string designCode = playedEntity.GetLettuceAbilityOwner().GetCardId();
		if (designCode == "LETL_866H")
		{
			if (cardID == "LETL_866P1_01")
			{
				GameState.Get().SetBusy(busy: true);
				yield return MissionPlayVO(bossActor, VO_Mida_Female_Naaru_LETL_Ability_02);
				GameState.Get().SetBusy(busy: false);
			}
		}
		else if (designCode == "LETL_866H" && cardID == "LETL_866P2_01")
		{
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(bossActor, VO_Mida_Female_Naaru_LETL_Ability_01);
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_866H");
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_866H");
		if (entity.GetCardId() == "LETL_866H")
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_866H");
		if (turn == 1)
		{
			yield return MissionPlayVOOnce(bossActor, m_introLine);
		}
	}
}
