using System.Collections;
using System.Collections.Generic;

public class LettuceBoss_LT24_813H_VoHandler : VoPlaybackHandler
{
	private static readonly AssetReference VO_KAR_044_Attack = new AssetReference("VO_KAR_044_Attack.prefab:2a09cd263390eda4186d3c094b1e8837");

	private static readonly AssetReference VO_KAR_044_Death = new AssetReference("VO_KAR_044_Death.prefab:aac1fb840a70d4f499bb1693f1ce3419");

	private static readonly AssetReference VO_KAR_044_Idle = new AssetReference("VO_KAR_044_Idle.prefab:0a4e68d54e47a5e4baaccf390faf7e8a");

	private static readonly AssetReference VO_KAR_044_Intro = new AssetReference("VO_KAR_044_Intro.prefab:3b90bfd5a76bdf341bce3de3d0604a7f");

	private List<string> m_IdleLines = new List<string> { VO_KAR_044_Idle };

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> soundFiles = new List<string> { VO_KAR_044_Intro, VO_KAR_044_Idle, VO_KAR_044_Attack, VO_KAR_044_Death };
		SetBossVOLines(soundFiles);
		foreach (string soundFile in soundFiles)
		{
			GameState.Get().GetGameEntity().PreloadSound(soundFile);
		}
	}

	public override void OnCreateGame()
	{
		base.OnCreateGame();
		m_introLine = VO_KAR_044_Intro;
		m_deathLine = VO_KAR_044_Death;
	}

	public override IEnumerator RespondToWillPlayCardWithTiming(string cardID, Entity playedEntity)
	{
		while (m_enemySpeaking)
		{
			yield return null;
		}
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LT24_813H");
		if (playedEntity.GetLettuceAbilityOwner().GetCardId() == "LT24_813H")
		{
			switch (cardID)
			{
			case "LT22_001P2_03":
				GameState.Get().SetBusy(busy: true);
				yield return MissionPlayVOOnce(bossActor, VO_KAR_044_Attack);
				GameState.Get().SetBusy(busy: false);
				break;
			case "LT22_001P2_05":
				GameState.Get().SetBusy(busy: true);
				yield return MissionPlayVOOnce(bossActor, VO_KAR_044_Attack);
				GameState.Get().SetBusy(busy: false);
				break;
			case "LETL_441_03":
				GameState.Get().SetBusy(busy: true);
				yield return MissionPlayVOOnce(bossActor, VO_KAR_044_Attack);
				GameState.Get().SetBusy(busy: false);
				break;
			case "LETL_441_05":
				GameState.Get().SetBusy(busy: true);
				yield return MissionPlayVOOnce(bossActor, VO_KAR_044_Attack);
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LT24_813H");
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LT24_813H");
		if (entity.GetCardId() == "LT24_813H")
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LT24_813H");
		if (turn == 1)
		{
			yield return MissionPlayVOOnce(bossActor, m_introLine);
		}
	}
}
