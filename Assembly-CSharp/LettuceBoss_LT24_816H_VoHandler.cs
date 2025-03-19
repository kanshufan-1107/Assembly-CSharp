using System.Collections;
using System.Collections.Generic;

public class LettuceBoss_LT24_816H_VoHandler : VoPlaybackHandler
{
	private static readonly AssetReference NightbaneBoss_Attack_1 = new AssetReference("NightbaneBoss_Attack_1.prefab:e2bf2eaf82c5ecf46922c19e35deb0ae");

	private static readonly AssetReference NightbaneBoss_Death_1 = new AssetReference("NightbaneBoss_Death_1.prefab:626bcca9d3329b04396c679564669a01");

	private static readonly AssetReference NightbaneBoss_Start_1 = new AssetReference("NightbaneBoss_Start_1.prefab:7968327ff2a98c64fa4abd973e1c4c56");

	private List<string> m_IdleLines = new List<string> { NightbaneBoss_Attack_1 };

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> soundFiles = new List<string> { NightbaneBoss_Start_1, NightbaneBoss_Attack_1, NightbaneBoss_Death_1 };
		SetBossVOLines(soundFiles);
		foreach (string soundFile in soundFiles)
		{
			GameState.Get().GetGameEntity().PreloadSound(soundFile);
		}
	}

	public override void OnCreateGame()
	{
		base.OnCreateGame();
		m_introLine = NightbaneBoss_Start_1;
		m_deathLine = NightbaneBoss_Death_1;
	}

	public override IEnumerator RespondToWillPlayCardWithTiming(string cardID, Entity playedEntity)
	{
		while (m_enemySpeaking)
		{
			yield return null;
		}
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LT24_816H");
		if (playedEntity.GetLettuceAbilityOwner().GetCardId() == "LT24_816H" && cardID == "LT24_816P1")
		{
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVOOnce(bossActor, NightbaneBoss_Attack_1);
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LT24_816H");
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LT24_816H");
		if (entity.GetCardId() == "LT24_816H")
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LT24_816H");
		if (turn == 1)
		{
			yield return MissionPlayVOOnce(bossActor, m_introLine);
		}
	}
}
