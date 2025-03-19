using System.Collections;
using System.Collections.Generic;

public class LettuceBoss_LETL_852H_VoHandler : VoPlaybackHandler
{
	private static readonly AssetReference VO_Popsicooler_Female_Mech_Attack_01 = new AssetReference("VO_Popsicooler_Female_Mech_Attack_01.prefab:c3f1bb2b85f45634fa6a2ef4e382f303");

	private static readonly AssetReference VO_Popsicooler_Female_Mech_Death_01 = new AssetReference("VO_Popsicooler_Female_Mech_Death_01.prefab:bf912d623c8eae242b4ddba68235e333");

	private static readonly AssetReference VO_Popsicooler_Female_Mech_Intro_01 = new AssetReference("VO_Popsicooler_Female_Mech_Intro_01.prefab:bfc1babc2aa43d14a87f4f61cf98ec4e");

	private List<string> m_IdleLines = new List<string> { VO_Popsicooler_Female_Mech_Intro_01 };

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> soundFiles = new List<string> { VO_Popsicooler_Female_Mech_Attack_01, VO_Popsicooler_Female_Mech_Death_01, VO_Popsicooler_Female_Mech_Intro_01 };
		SetBossVOLines(soundFiles);
		foreach (string soundFile in soundFiles)
		{
			GameState.Get().GetGameEntity().PreloadSound(soundFile);
		}
	}

	public override void OnCreateGame()
	{
		base.OnCreateGame();
		m_introLine = VO_Popsicooler_Female_Mech_Intro_01;
		m_deathLine = VO_Popsicooler_Female_Mech_Death_01;
	}

	public override IEnumerator RespondToWillPlayCardWithTiming(string cardID, Entity playedEntity)
	{
		while (m_enemySpeaking)
		{
			yield return null;
		}
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_852H");
		if (playedEntity.GetLettuceAbilityOwner().GetCardId() == "LETL_852H" && cardID == "LETL_852P3")
		{
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(bossActor, VO_Popsicooler_Female_Mech_Attack_01);
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_852H");
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_852H");
		if (entity.GetCardId() == "LETL_852H")
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_852H");
		if (turn == 1)
		{
			yield return MissionPlayVOOnce(bossActor, m_introLine);
		}
	}
}
