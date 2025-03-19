using System.Collections;
using System.Collections.Generic;

public class LettuceBoss_LETL_819H_VoHandler : VoPlaybackHandler
{
	private static readonly AssetReference VO_LETL_819H_Male_Tauren_Attack_01 = new AssetReference("VO_LETL_819H_Male_Tauren_Attack_01.prefab:43f710e1033ddb74aba009e8814b93db");

	private static readonly AssetReference VO_LETL_819H_Male_Tauren_Attack_02 = new AssetReference("VO_LETL_819H_Male_Tauren_Attack_02.prefab:ec8a6a2ed71cfac4dad0541856a4597a");

	private static readonly AssetReference VO_LETL_819H_Male_Tauren_Death_01 = new AssetReference("VO_LETL_819H_Male_Tauren_Death_01.prefab:efdd409c719fe4b4399b819690a79975");

	private static readonly AssetReference VO_LETL_819H_Male_Tauren_Idle_01 = new AssetReference("VO_LETL_819H_Male_Tauren_Idle_01.prefab:81bf0779f04538b42bd6443e9f46e6ce");

	private static readonly AssetReference VO_LETL_819H_Male_Tauren_Intro_01 = new AssetReference("VO_LETL_819H_Male_Tauren_Intro_01.prefab:d78fb69a7d5c5ee40aadbde46774efcf");

	private List<string> m_IdleLines = new List<string> { VO_LETL_819H_Male_Tauren_Idle_01 };

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> soundFiles = new List<string> { VO_LETL_819H_Male_Tauren_Attack_01, VO_LETL_819H_Male_Tauren_Attack_02, VO_LETL_819H_Male_Tauren_Death_01, VO_LETL_819H_Male_Tauren_Idle_01, VO_LETL_819H_Male_Tauren_Intro_01 };
		SetBossVOLines(soundFiles);
		foreach (string soundFile in soundFiles)
		{
			GameState.Get().GetGameEntity().PreloadSound(soundFile);
		}
	}

	public override void OnCreateGame()
	{
		base.OnCreateGame();
		m_introLine = VO_LETL_819H_Male_Tauren_Intro_01;
		m_deathLine = VO_LETL_819H_Male_Tauren_Death_01;
	}

	public override IEnumerator RespondToWillPlayCardWithTiming(string cardID, Entity playedEntity)
	{
		while (m_enemySpeaking)
		{
			yield return null;
		}
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_819H");
		if (playedEntity.GetLettuceAbilityOwner().GetCardId() == "LETL_819H")
		{
			switch (cardID)
			{
			case "LETL_406_02":
			case "LETL_020P6_01":
				GameState.Get().SetBusy(busy: true);
				yield return MissionPlayVOOnce(bossActor, VO_LETL_819H_Male_Tauren_Attack_01);
				GameState.Get().SetBusy(busy: false);
				break;
			case "LETL_441_02":
				GameState.Get().SetBusy(busy: true);
				yield return MissionPlayVOOnce(bossActor, VO_LETL_819H_Male_Tauren_Attack_02);
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_819H");
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_819H");
		if (entity.GetCardId() == "LETL_819H")
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_819H");
		if (turn == 1)
		{
			yield return MissionPlayVOOnce(bossActor, m_introLine);
		}
	}
}
