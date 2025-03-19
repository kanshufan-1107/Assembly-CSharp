using System.Collections;
using System.Collections.Generic;

public class LettuceBoss_LETL_839H_VoHandler : VoPlaybackHandler
{
	private static readonly AssetReference VO_BRMA04_1_CARD_04 = new AssetReference("VO_BRMA04_1_CARD_04.prefab:53f20ec5598fc8a459615f6a57c661be");

	private static readonly AssetReference VO_BRMA04_1_HERO_POWER_05 = new AssetReference("VO_BRMA04_1_HERO_POWER_05.prefab:1c2e947768a86424abf65a8b5ad573ec");

	private static readonly AssetReference VO_BRMA04_1_RESPONSE_03 = new AssetReference("VO_BRMA04_1_RESPONSE_03.prefab:75a029ecfd071914aaf0def7bc041b85");

	private static readonly AssetReference VO_BRMA04_1_DEATH_06 = new AssetReference("VO_BRMA04_1_DEATH_06.prefab:34e63d08fa3428e4091c5cdbe63dd894");

	private static readonly AssetReference VO_BRMA04_1_START_01 = new AssetReference("VO_BRMA04_1_START_01.prefab:5d9de41d8c48c924a88ff1a539711761");

	private List<string> m_IdleLines = new List<string> { VO_BRMA04_1_RESPONSE_03 };

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> soundFiles = new List<string> { VO_BRMA04_1_CARD_04, VO_BRMA04_1_HERO_POWER_05, VO_BRMA04_1_RESPONSE_03, VO_BRMA04_1_DEATH_06, VO_BRMA04_1_START_01 };
		SetBossVOLines(soundFiles);
		foreach (string soundFile in soundFiles)
		{
			GameState.Get().GetGameEntity().PreloadSound(soundFile);
		}
	}

	public override void OnCreateGame()
	{
		base.OnCreateGame();
		m_introLine = VO_BRMA04_1_START_01;
		m_deathLine = VO_BRMA04_1_DEATH_06;
	}

	public override IEnumerator RespondToWillPlayCardWithTiming(string cardID, Entity playedEntity)
	{
		while (m_enemySpeaking)
		{
			yield return null;
		}
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_839H");
		if (playedEntity.GetLettuceAbilityOwner().GetCardId() == "LETL_839H")
		{
			switch (cardID)
			{
			case "LETL_839P1_01":
			case "LETL_839P1_03":
				GameState.Get().SetBusy(busy: true);
				yield return MissionPlayVOOnce(bossActor, VO_BRMA04_1_CARD_04);
				GameState.Get().SetBusy(busy: false);
				break;
			case "LETL_839P2_01":
			case "LETL_839P2_03":
				GameState.Get().SetBusy(busy: true);
				yield return MissionPlayVOOnce(bossActor, VO_BRMA04_1_HERO_POWER_05);
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_839H");
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_839H");
		if (entity.GetCardId() == "LETL_839H")
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_839H");
		if (turn == 1)
		{
			yield return MissionPlayVOOnce(bossActor, m_introLine);
		}
	}
}
