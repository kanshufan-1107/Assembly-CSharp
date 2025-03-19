using System.Collections;
using System.Collections.Generic;

public class LettuceBoss_LETL_844H_VoHandler : VoPlaybackHandler
{
	private static readonly AssetReference VO_LETL_844H_Male_Orc_Attack_01 = new AssetReference("VO_LETL_844H_Male_Orc_Attack_01.prefab:9b627bbeeeba8434995cc21a170d8614");

	private static readonly AssetReference VO_LETL_844H_Male_Orc_Attack_02 = new AssetReference("VO_LETL_844H_Male_Orc_Attack_02.prefab:fa840a9a5fc89d94ab5404ed9d3a83cf");

	private static readonly AssetReference VO_LETL_844H_Male_Orc_Death_01 = new AssetReference("VO_LETL_844H_Male_Orc_Death_01.prefab:5344bd97a54057e45a27fb97fd52a978");

	private static readonly AssetReference VO_LETL_844H_Male_Orc_Idle_01 = new AssetReference("VO_LETL_844H_Male_Orc_Idle_01.prefab:2ab3528369cab9f408cfeee79827e23a");

	private static readonly AssetReference VO_LETL_844H_Male_Orc_Intro_01 = new AssetReference("VO_LETL_844H_Male_Orc_Intro_01.prefab:ec04d635155da764f8759e507f26a481");

	private List<string> m_IdleLines = new List<string> { VO_LETL_844H_Male_Orc_Idle_01 };

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> soundFiles = new List<string> { VO_LETL_844H_Male_Orc_Attack_01, VO_LETL_844H_Male_Orc_Attack_02, VO_LETL_844H_Male_Orc_Death_01, VO_LETL_844H_Male_Orc_Idle_01, VO_LETL_844H_Male_Orc_Intro_01 };
		SetBossVOLines(soundFiles);
		foreach (string soundFile in soundFiles)
		{
			GameState.Get().GetGameEntity().PreloadSound(soundFile);
		}
	}

	public override void OnCreateGame()
	{
		base.OnCreateGame();
		m_introLine = VO_LETL_844H_Male_Orc_Intro_01;
		m_deathLine = VO_LETL_844H_Male_Orc_Death_01;
	}

	public override IEnumerator RespondToWillPlayCardWithTiming(string cardID, Entity playedEntity)
	{
		while (m_enemySpeaking)
		{
			yield return null;
		}
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_844H");
		if (!(playedEntity.GetLettuceAbilityOwner().GetCardId() == "LETL_844H"))
		{
			yield break;
		}
		if (!(cardID == "LETL_033P5_03"))
		{
			if (cardID == "LETLT_080_02")
			{
				GameState.Get().SetBusy(busy: true);
				yield return MissionPlayVOOnce(bossActor, VO_LETL_844H_Male_Orc_Attack_02);
				GameState.Get().SetBusy(busy: false);
			}
		}
		else
		{
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVOOnce(bossActor, VO_LETL_844H_Male_Orc_Attack_01);
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_844H");
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_844H");
		if (entity.GetCardId() == "LETL_844H")
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_844H");
		if (turn == 1)
		{
			yield return MissionPlayVOOnce(bossActor, m_introLine);
		}
	}
}
