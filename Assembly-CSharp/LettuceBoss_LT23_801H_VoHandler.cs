using System.Collections;
using System.Collections.Generic;

public class LettuceBoss_LT23_801H_VoHandler : VoPlaybackHandler
{
	private static readonly AssetReference VO_WarlordRekes_Male_Naga_Attack_02 = new AssetReference("VO_WarlordRekes_Male_Naga_Attack_02.prefab:3166cbc5855d63943b471dfc6e9b3936");

	private static readonly AssetReference VO_WarlordRekes_Male_Naga_Death_01 = new AssetReference("VO_WarlordRekes_Male_Naga_Death_01.prefab:899aa139d9174434eb83c4962f2445f9");

	private static readonly AssetReference VO_WarlordRekes_Male_Naga_Idle_01 = new AssetReference("VO_WarlordRekes_Male_Naga_Idle_01.prefab:3ba9fcd6b7f5da1479129ba585f88eae");

	private static readonly AssetReference VO_WarlordRekes_Male_Naga_Intro_01 = new AssetReference("VO_WarlordRekes_Male_Naga_Intro_01.prefab:486107c2ed26c6746bb94c8afde57c95");

	private List<string> m_IdleLines = new List<string> { VO_WarlordRekes_Male_Naga_Idle_01 };

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> soundFiles = new List<string> { VO_WarlordRekes_Male_Naga_Death_01, VO_WarlordRekes_Male_Naga_Intro_01, VO_WarlordRekes_Male_Naga_Attack_02, VO_WarlordRekes_Male_Naga_Idle_01 };
		SetBossVOLines(soundFiles);
		foreach (string soundFile in soundFiles)
		{
			GameState.Get().GetGameEntity().PreloadSound(soundFile);
		}
	}

	public override void OnCreateGame()
	{
		base.OnCreateGame();
		m_introLine = VO_WarlordRekes_Male_Naga_Intro_01;
		m_deathLine = VO_WarlordRekes_Male_Naga_Death_01;
	}

	public override IEnumerator RespondToWillPlayCardWithTiming(string cardID, Entity playedEntity)
	{
		while (m_enemySpeaking)
		{
			yield return null;
		}
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LT23_801H");
		if (playedEntity.GetLettuceAbilityOwner().GetCardId() == "LT23_801H" && cardID == "LT23_801P1")
		{
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVOOnce(bossActor, VO_WarlordRekes_Male_Naga_Attack_02);
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LT23_801H");
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LT23_801H");
		if (entity.GetCardId() == "LT23_801H")
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LT23_801H");
		if (turn == 1)
		{
			yield return MissionPlayVOOnce(bossActor, m_introLine);
		}
	}
}
