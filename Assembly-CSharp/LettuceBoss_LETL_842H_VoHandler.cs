using System.Collections;
using System.Collections.Generic;

public class LettuceBoss_LETL_842H_VoHandler : VoPlaybackHandler
{
	private static readonly AssetReference VO_BRMA07_1_CARD_04 = new AssetReference("VO_BRMA07_1_CARD_04.prefab:f498bd13724f67d48a0f0bc55034c44b");

	private static readonly AssetReference VO_BRMA07_1_HERO_POWER_05 = new AssetReference("VO_BRMA07_1_HERO_POWER_05.prefab:10f8a1b1fc7c9374b8c2b741f27694be");

	private static readonly AssetReference VO_BRMA07_1_RESPONSE_03 = new AssetReference("VO_BRMA07_1_RESPONSE_03.prefab:b43d82ce7a9bb59438b594dd3c185050");

	private static readonly AssetReference VO_BRMA07_1_TURN1_02 = new AssetReference("VO_BRMA07_1_TURN1_02.prefab:ac11bf2418c6e0f418f2216348b224c3");

	private List<string> m_IdleLines = new List<string> { VO_BRMA07_1_TURN1_02 };

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> soundFiles = new List<string> { VO_BRMA07_1_CARD_04, VO_BRMA07_1_HERO_POWER_05, VO_BRMA07_1_RESPONSE_03, VO_BRMA07_1_TURN1_02 };
		SetBossVOLines(soundFiles);
		foreach (string soundFile in soundFiles)
		{
			GameState.Get().GetGameEntity().PreloadSound(soundFile);
		}
	}

	public override void OnCreateGame()
	{
		base.OnCreateGame();
		m_introLine = VO_BRMA07_1_CARD_04;
		m_deathLine = VO_BRMA07_1_RESPONSE_03;
	}

	public override IEnumerator RespondToWillPlayCardWithTiming(string cardID, Entity playedEntity)
	{
		while (m_enemySpeaking)
		{
			yield return null;
		}
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_842H");
		if (playedEntity.GetLettuceAbilityOwner().GetCardId() == "LETL_842H")
		{
			if (cardID == "LETL_842P1_01" || cardID == "LETL_842P1_02")
			{
				GameState.Get().SetBusy(busy: true);
				yield return MissionPlayVOOnce(bossActor, VO_BRMA07_1_HERO_POWER_05);
				GameState.Get().SetBusy(busy: false);
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_842H");
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_842H");
		if (entity.GetCardId() == "LETL_842H")
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_842H");
		if (turn == 1)
		{
			yield return MissionPlayVOOnce(bossActor, m_introLine);
		}
	}
}
