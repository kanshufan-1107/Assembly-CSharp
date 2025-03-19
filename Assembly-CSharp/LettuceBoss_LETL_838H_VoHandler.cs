using System.Collections;
using System.Collections.Generic;

public class LettuceBoss_LETL_838H_VoHandler : VoPlaybackHandler
{
	private static readonly AssetReference VO_BRMA03_1_CARD_04 = new AssetReference("VO_BRMA03_1_CARD_04.prefab:2ebdf13895d3b4e4e8979764b99e89e0");

	private static readonly AssetReference VO_BRMA03_1_HERO_POWER_06 = new AssetReference("VO_BRMA03_1_HERO_POWER_06.prefab:2ad44580bf0939c4292a8a454a6fb859");

	private static readonly AssetReference VO_BRM_028_Death_08 = new AssetReference("VO_BRM_028_Death_08.prefab:b61171d1faa5daa4aadcc4743027bb50");

	private List<string> m_IdleLines = new List<string>();

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> soundFiles = new List<string> { VO_BRMA03_1_CARD_04, VO_BRMA03_1_HERO_POWER_06, VO_BRM_028_Death_08 };
		SetBossVOLines(soundFiles);
		foreach (string soundFile in soundFiles)
		{
			GameState.Get().GetGameEntity().PreloadSound(soundFile);
		}
	}

	public override void OnCreateGame()
	{
		base.OnCreateGame();
		m_introLine = VO_BRMA03_1_CARD_04;
		m_deathLine = VO_BRM_028_Death_08;
	}

	public override IEnumerator RespondToWillPlayCardWithTiming(string cardID, Entity playedEntity)
	{
		while (m_enemySpeaking)
		{
			yield return null;
		}
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_838H");
		if (playedEntity.GetLettuceAbilityOwner().GetCardId() == "LETL_838H")
		{
			if (cardID == "LETL_838P1_03" || cardID == "LETL_838P1_04")
			{
				GameState.Get().SetBusy(busy: true);
				yield return MissionPlayVOOnce(bossActor, VO_BRMA03_1_HERO_POWER_06);
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_838H");
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_838H");
		if (entity.GetCardId() == "LETL_838H")
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_838H");
		if (turn == 1)
		{
			yield return MissionPlayVOOnce(bossActor, m_introLine);
		}
	}
}
