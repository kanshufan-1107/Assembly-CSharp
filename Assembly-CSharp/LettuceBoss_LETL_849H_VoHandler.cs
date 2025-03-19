using System.Collections;
using System.Collections.Generic;

public class LettuceBoss_LETL_849H_VoHandler : VoPlaybackHandler
{
	private static readonly AssetReference VO_RavakGrimtotem_Male_Tauren_Attack_01 = new AssetReference("VO_RavakGrimtotem_Male_Tauren_Attack_01.prefab:04b90286281623b40a39a02043b3824f");

	private static readonly AssetReference VO_RavakGrimtotem_Male_Tauren_Attack_02 = new AssetReference("VO_RavakGrimtotem_Male_Tauren_Attack_02.prefab:60d3946b33c590a49ac3def1016f9cf3");

	private static readonly AssetReference VO_RavakGrimtotem_Male_Tauren_Death_01 = new AssetReference("VO_RavakGrimtotem_Male_Tauren_Death_01.prefab:03622b1b601cdaf4f89edb67f9e697e2");

	private static readonly AssetReference VO_RavakGrimtotem_Male_Tauren_Idle_01 = new AssetReference("VO_RavakGrimtotem_Male_Tauren_Idle_01.prefab:36d69fd3299ee25488ca68dc8ad53202");

	private static readonly AssetReference VO_RavakGrimtotem_Male_Tauren_Intro_01 = new AssetReference("VO_RavakGrimtotem_Male_Tauren_Intro_01.prefab:1ef7373f761e7cd45b4d9e0e5f4974e6");

	private List<string> m_IdleLines = new List<string> { VO_RavakGrimtotem_Male_Tauren_Idle_01 };

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> soundFiles = new List<string> { VO_RavakGrimtotem_Male_Tauren_Attack_01, VO_RavakGrimtotem_Male_Tauren_Attack_02, VO_RavakGrimtotem_Male_Tauren_Death_01, VO_RavakGrimtotem_Male_Tauren_Idle_01, VO_RavakGrimtotem_Male_Tauren_Intro_01 };
		SetBossVOLines(soundFiles);
		foreach (string soundFile in soundFiles)
		{
			GameState.Get().GetGameEntity().PreloadSound(soundFile);
		}
	}

	public override void OnCreateGame()
	{
		base.OnCreateGame();
		m_introLine = VO_RavakGrimtotem_Male_Tauren_Intro_01;
		m_deathLine = VO_RavakGrimtotem_Male_Tauren_Death_01;
	}

	public override IEnumerator RespondToWillPlayCardWithTiming(string cardID, Entity playedEntity)
	{
		while (m_enemySpeaking)
		{
			yield return null;
		}
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_849H");
		if (playedEntity.GetLettuceAbilityOwner().GetCardId() == "LETL_849H")
		{
			switch (cardID)
			{
			case "LETL_849P2_05":
			case "LETL_849P2_04":
				GameState.Get().SetBusy(busy: true);
				yield return MissionPlayVO(bossActor, VO_RavakGrimtotem_Male_Tauren_Attack_01);
				GameState.Get().SetBusy(busy: false);
				break;
			case "LETL_849P5":
				GameState.Get().SetBusy(busy: true);
				yield return MissionPlayVO(bossActor, VO_RavakGrimtotem_Male_Tauren_Attack_02);
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_849H");
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_849H");
		if (entity.GetCardId() == "LETL_849H")
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_849H");
		if (turn == 1)
		{
			yield return MissionPlayVOOnce(bossActor, m_introLine);
		}
	}
}
