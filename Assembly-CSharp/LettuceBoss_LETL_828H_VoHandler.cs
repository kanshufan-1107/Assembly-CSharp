using System.Collections;
using System.Collections.Generic;

public class LettuceBoss_LETL_828H_VoHandler : VoPlaybackHandler
{
	private static readonly AssetReference VO_LETL_828H_Male_Ancient_Attack_01 = new AssetReference("VO_LETL_828H_Male_Ancient_Attack_01.prefab:894586fb0af2e8e448b8ce05d29aa0f4");

	private static readonly AssetReference VO_LETL_828H_Male_Ancient_Attack_02 = new AssetReference("VO_LETL_828H_Male_Ancient_Attack_02.prefab:3763ab6f1bf94774db4082d2e2c18735");

	private static readonly AssetReference VO_LETL_828H_Male_Ancient_Death_01 = new AssetReference("VO_LETL_828H_Male_Ancient_Death_01.prefab:ccfba26268e7d5240b23f754437b3020");

	private static readonly AssetReference VO_LETL_828H_Male_Ancient_Idle_01 = new AssetReference("VO_LETL_828H_Male_Ancient_Idle_01.prefab:c25937487d86d944a8436a09f687d833");

	private static readonly AssetReference VO_LETL_828H_Male_Ancient_Intro_01 = new AssetReference("VO_LETL_828H_Male_Ancient_Intro_01.prefab:6067716e6019bec419a1bf0d60849f93");

	private List<string> m_IdleLines = new List<string> { VO_LETL_828H_Male_Ancient_Idle_01 };

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> soundFiles = new List<string> { VO_LETL_828H_Male_Ancient_Attack_01, VO_LETL_828H_Male_Ancient_Attack_02, VO_LETL_828H_Male_Ancient_Death_01, VO_LETL_828H_Male_Ancient_Idle_01, VO_LETL_828H_Male_Ancient_Intro_01 };
		SetBossVOLines(soundFiles);
		foreach (string soundFile in soundFiles)
		{
			GameState.Get().GetGameEntity().PreloadSound(soundFile);
		}
	}

	public override void OnCreateGame()
	{
		base.OnCreateGame();
		m_introLine = VO_LETL_828H_Male_Ancient_Intro_01;
		m_deathLine = VO_LETL_828H_Male_Ancient_Death_01;
	}

	public override IEnumerator RespondToWillPlayCardWithTiming(string cardID, Entity playedEntity)
	{
		while (m_enemySpeaking)
		{
			yield return null;
		}
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_828H");
		if (playedEntity.GetLettuceAbilityOwner().GetCardId() == "LETL_828H")
		{
			switch (cardID)
			{
			case "LETL_828P1_02":
			case "LETL_828P1_05":
				GameState.Get().SetBusy(busy: true);
				yield return MissionPlayVOOnce(bossActor, VO_LETL_828H_Male_Ancient_Attack_01);
				GameState.Get().SetBusy(busy: false);
				break;
			case "LETL_828P2_01":
			case "LETL_828P2_03":
				GameState.Get().SetBusy(busy: true);
				yield return MissionPlayVOOnce(bossActor, VO_LETL_828H_Male_Ancient_Attack_02);
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_828H");
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_828H");
		if (entity.GetCardId() == "LETL_828H")
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_828H");
		if (turn == 1)
		{
			yield return MissionPlayVOOnce(bossActor, m_introLine);
		}
	}
}
