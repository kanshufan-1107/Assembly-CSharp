using System.Collections;
using System.Collections.Generic;

public class LettuceBoss_LETL_834H_VoHandler : VoPlaybackHandler
{
	private static readonly AssetReference LETL_833H_Attack_01 = new AssetReference("LETL_833H_Attack_01.prefab:e76fcb9c88febcc4bb798121706f9e30");

	private static readonly AssetReference LETL_833H_Death_01 = new AssetReference("LETL_833H_Death_01.prefab:e54c160a014371444a8d26ab25f637a9");

	private static readonly AssetReference LETL_833H_Intro_01 = new AssetReference("LETL_833H_Intro_01.prefab:37071039ab31c7f4b8845641e0ec8a84");

	private List<string> m_IdleLines = new List<string>();

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> soundFiles = new List<string> { LETL_833H_Attack_01, LETL_833H_Death_01, LETL_833H_Intro_01 };
		SetBossVOLines(soundFiles);
		foreach (string soundFile in soundFiles)
		{
			GameState.Get().GetGameEntity().PreloadSound(soundFile);
		}
	}

	public override void OnCreateGame()
	{
		base.OnCreateGame();
		m_introLine = LETL_833H_Intro_01;
		m_deathLine = LETL_833H_Death_01;
	}

	public override IEnumerator RespondToWillPlayCardWithTiming(string cardID, Entity playedEntity)
	{
		while (m_enemySpeaking)
		{
			yield return null;
		}
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_834H");
		if (playedEntity.GetLettuceAbilityOwner().GetCardId() == "LETL_834H")
		{
			switch (cardID)
			{
			case "LETL_8342P1":
			case "LETL_8342P2":
			case "LETL_8342P3":
				GameState.Get().SetBusy(busy: true);
				yield return MissionPlayVOOnce(bossActor, LETL_833H_Attack_01);
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_834H");
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_834H");
		if (entity.GetCardId() == "LETL_834H")
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_834H");
		if (turn == 1)
		{
			yield return MissionPlayVOOnce(bossActor, m_introLine);
		}
	}
}
