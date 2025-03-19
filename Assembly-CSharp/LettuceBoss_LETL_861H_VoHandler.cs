using System.Collections;
using System.Collections.Generic;

public class LettuceBoss_LETL_861H_VoHandler : VoPlaybackHandler
{
	private static readonly AssetReference VO_Smolderwing_Male_Dragon_Attack_01 = new AssetReference("VO_Smolderwing_Male_Dragon_Attack_01.prefab:b037c3832239e18419af3f48c9a9ce41");

	private static readonly AssetReference VO_Smolderwing_Male_Dragon_Attack_02 = new AssetReference("VO_Smolderwing_Male_Dragon_Attack_02.prefab:aac48028d4af14344885197f3c41d392");

	private static readonly AssetReference VO_Smolderwing_Male_Dragon_Death_01 = new AssetReference("VO_Smolderwing_Male_Dragon_Death_01.prefab:e42799c9240f2df41ba9c078e3536416");

	private static readonly AssetReference VO_Smolderwing_Male_Dragon_Idle_01 = new AssetReference("VO_Smolderwing_Male_Dragon_Idle_01.prefab:1bd60de254a457b4994cac9286faf2ce");

	private static readonly AssetReference VO_Smolderwing_Male_Dragon_Intro_01 = new AssetReference("VO_Smolderwing_Male_Dragon_Intro_01.prefab:0468a727e7cce144f9f6dfdf1385b315");

	private List<string> m_IdleLines = new List<string> { VO_Smolderwing_Male_Dragon_Idle_01 };

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> soundFiles = new List<string> { VO_Smolderwing_Male_Dragon_Attack_01, VO_Smolderwing_Male_Dragon_Attack_02, VO_Smolderwing_Male_Dragon_Death_01, VO_Smolderwing_Male_Dragon_Idle_01, VO_Smolderwing_Male_Dragon_Intro_01 };
		SetBossVOLines(soundFiles);
		foreach (string soundFile in soundFiles)
		{
			GameState.Get().GetGameEntity().PreloadSound(soundFile);
		}
	}

	public override void OnCreateGame()
	{
		base.OnCreateGame();
		m_introLine = VO_Smolderwing_Male_Dragon_Intro_01;
		m_deathLine = VO_Smolderwing_Male_Dragon_Death_01;
	}

	public override IEnumerator RespondToWillPlayCardWithTiming(string cardID, Entity playedEntity)
	{
		while (m_enemySpeaking)
		{
			yield return null;
		}
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_861H");
		if (!(playedEntity.GetLettuceAbilityOwner().GetCardId() == "LETL_861H"))
		{
			yield break;
		}
		if (!(cardID == "LETL_861P2_01"))
		{
			if (cardID == "LETL_861P1_01")
			{
				GameState.Get().SetBusy(busy: true);
				yield return MissionPlayVO(bossActor, VO_Smolderwing_Male_Dragon_Attack_01);
				GameState.Get().SetBusy(busy: false);
			}
		}
		else
		{
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(bossActor, VO_Smolderwing_Male_Dragon_Attack_02);
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_861H");
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_861H");
		if (entity.GetCardId() == "LETL_861H")
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_861H");
		if (turn == 1)
		{
			yield return MissionPlayVOOnce(bossActor, m_introLine);
		}
	}
}
