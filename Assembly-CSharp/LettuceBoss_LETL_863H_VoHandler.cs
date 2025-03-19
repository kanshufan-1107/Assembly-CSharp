using System.Collections;
using System.Collections.Generic;

public class LettuceBoss_LETL_863H_VoHandler : VoPlaybackHandler
{
	private static readonly AssetReference VO_ONY_030_Female_Human_Attack_01 = new AssetReference("VO_ONY_030_Female_Human_Attack_01.prefab:7c4a9b15ca2909446b31f9e19c5bb033");

	private static readonly AssetReference VO_ONY_030_Female_Human_Death_01 = new AssetReference("VO_ONY_030_Female_Human_Death_01.prefab:0e3da644e0e19de48aa5087847722734");

	private static readonly AssetReference VO_ONY_030_Female_Human_Play_01 = new AssetReference("VO_ONY_030_Female_Human_Play_01.prefab:1410616ecc4a1db4d9a7381d4c341d7f");

	private List<string> m_IdleLines = new List<string> { VO_ONY_030_Female_Human_Play_01 };

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> soundFiles = new List<string> { VO_ONY_030_Female_Human_Attack_01, VO_ONY_030_Female_Human_Death_01, VO_ONY_030_Female_Human_Play_01 };
		SetBossVOLines(soundFiles);
		foreach (string soundFile in soundFiles)
		{
			GameState.Get().GetGameEntity().PreloadSound(soundFile);
		}
	}

	public override void OnCreateGame()
	{
		base.OnCreateGame();
		m_introLine = VO_ONY_030_Female_Human_Play_01;
		m_deathLine = VO_ONY_030_Female_Human_Death_01;
	}

	public override IEnumerator RespondToWillPlayCardWithTiming(string cardID, Entity playedEntity)
	{
		while (m_enemySpeaking)
		{
			yield return null;
		}
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_863H");
		if (playedEntity.GetLettuceAbilityOwner().GetCardId() == "LETL_863H" && cardID == "LETL_863P1")
		{
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(bossActor, VO_ONY_030_Female_Human_Attack_01);
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_863H");
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_863H");
		if (entity.GetCardId() == "LETL_863H")
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_863H");
		if (turn == 1)
		{
			yield return MissionPlayVOOnce(bossActor, m_introLine);
		}
	}
}
