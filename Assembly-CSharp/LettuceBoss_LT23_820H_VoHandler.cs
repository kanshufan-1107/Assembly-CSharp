using System.Collections;
using System.Collections.Generic;

public class LettuceBoss_LT23_820H_VoHandler : VoPlaybackHandler
{
	private static readonly AssetReference DMF_070_DarkmoonRabbit_Attack = new AssetReference("DMF_070_DarkmoonRabbit_Attack.prefab:a488f54e9eb7c994eb7183081309a736");

	private static readonly AssetReference DMF_070_DarkmoonRabbit_Death = new AssetReference("DMF_070_DarkmoonRabbit_Death.prefab:29fe5ce453638aa46bd60d6b0a8f1d67");

	private static readonly AssetReference DMF_070_DarkmoonRabbit_Play = new AssetReference("DMF_070_DarkmoonRabbit_Play.prefab:80021d88e5bec2549b4753ea9d8c7a65");

	private List<string> m_IdleLines = new List<string> { DMF_070_DarkmoonRabbit_Play };

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> soundFiles = new List<string> { DMF_070_DarkmoonRabbit_Play, DMF_070_DarkmoonRabbit_Attack, DMF_070_DarkmoonRabbit_Death };
		SetBossVOLines(soundFiles);
		foreach (string soundFile in soundFiles)
		{
			GameState.Get().GetGameEntity().PreloadSound(soundFile);
		}
	}

	public override void OnCreateGame()
	{
		base.OnCreateGame();
		m_introLine = DMF_070_DarkmoonRabbit_Play;
		m_deathLine = DMF_070_DarkmoonRabbit_Death;
	}

	public override IEnumerator RespondToWillPlayCardWithTiming(string cardID, Entity playedEntity)
	{
		while (m_enemySpeaking)
		{
			yield return null;
		}
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LT23_820H");
		if (playedEntity.GetLettuceAbilityOwner().GetCardId() == "LT23_820H" && cardID == "LETLT_118_02")
		{
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVOOnce(bossActor, DMF_070_DarkmoonRabbit_Attack);
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LT23_820H");
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LT23_820H");
		if (entity.GetCardId() == "LT23_820H")
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LT23_820H");
		if (turn == 1)
		{
			yield return MissionPlayVOOnce(bossActor, m_introLine);
		}
	}
}
