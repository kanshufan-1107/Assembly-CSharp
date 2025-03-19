using System.Collections;
using System.Collections.Generic;

public class LettuceBoss_LETL_847H_VoHandler : VoPlaybackHandler
{
	private static readonly AssetReference ChromaggusBoss_Death_1 = new AssetReference("ChromaggusBoss_Death_1.prefab:0af71a3749e50c842a1b7faac6b11b7f");

	private static readonly AssetReference ChromaggusBoss_Start_1 = new AssetReference("ChromaggusBoss_Start_1.prefab:9658c158e9c81094180c1e07bf337dd7");

	private List<string> m_IdleLines = new List<string> { ChromaggusBoss_Start_1 };

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> soundFiles = new List<string> { ChromaggusBoss_Death_1, ChromaggusBoss_Start_1 };
		SetBossVOLines(soundFiles);
		foreach (string soundFile in soundFiles)
		{
			GameState.Get().GetGameEntity().PreloadSound(soundFile);
		}
	}

	public override void OnCreateGame()
	{
		base.OnCreateGame();
		m_introLine = ChromaggusBoss_Start_1;
		m_deathLine = ChromaggusBoss_Death_1;
	}

	public override IEnumerator RespondToWillPlayCardWithTiming(string cardID, Entity playedEntity)
	{
		while (m_enemySpeaking)
		{
			yield return null;
		}
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_847H");
		if (playedEntity.GetLettuceAbilityOwner().GetCardId() == "LETL_847H" && cardID == "LETL_847P2_01")
		{
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVOOnce(bossActor, ChromaggusBoss_Start_1);
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_847H");
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_847H");
		if (entity.GetCardId() == "LETL_847H")
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_847H");
		if (turn == 1)
		{
			yield return MissionPlayVOOnce(bossActor, m_introLine);
		}
	}
}
