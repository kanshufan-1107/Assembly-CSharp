using System.Collections;
using System.Collections.Generic;

public class LettuceBoss_LETL_830H_VoHandler : VoPlaybackHandler
{
	private static readonly AssetReference VO_LETL_830H_Male_Furbolg_Attack_01 = new AssetReference("VO_LETL_830H_Male_Furbolg_Attack_01.prefab:aba0a4e9dde66c14b8b52730d39b5a11");

	private static readonly AssetReference VO_LETL_830H_Male_Furbolg_Attack_02 = new AssetReference("VO_LETL_830H_Male_Furbolg_Attack_02.prefab:65296488cfeafd24db0ab1d1429bbcce");

	private static readonly AssetReference VO_LETL_830H_Male_Furbolg_Death_01 = new AssetReference("VO_LETL_830H_Male_Furbolg_Death_01.prefab:9b9afc28f664394409635d64bedfe4a7");

	private static readonly AssetReference VO_LETL_830H_Male_Furbolg_Idle_01 = new AssetReference("VO_LETL_830H_Male_Furbolg_Idle_01.prefab:afb99a8946e2a544993a8899187658e6");

	private static readonly AssetReference VO_LETL_830H_Male_Furbolg_Intro_01 = new AssetReference("VO_LETL_830H_Male_Furbolg_Intro_01.prefab:74558d35e57a3594593f8f8efc30dfad");

	private List<string> m_IdleLines = new List<string> { VO_LETL_830H_Male_Furbolg_Idle_01 };

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> soundFiles = new List<string> { VO_LETL_830H_Male_Furbolg_Attack_01, VO_LETL_830H_Male_Furbolg_Attack_02, VO_LETL_830H_Male_Furbolg_Death_01, VO_LETL_830H_Male_Furbolg_Idle_01, VO_LETL_830H_Male_Furbolg_Intro_01 };
		SetBossVOLines(soundFiles);
		foreach (string soundFile in soundFiles)
		{
			GameState.Get().GetGameEntity().PreloadSound(soundFile);
		}
	}

	public override void OnCreateGame()
	{
		base.OnCreateGame();
		m_introLine = VO_LETL_830H_Male_Furbolg_Intro_01;
		m_deathLine = VO_LETL_830H_Male_Furbolg_Death_01;
	}

	public override IEnumerator RespondToWillPlayCardWithTiming(string cardID, Entity playedEntity)
	{
		while (m_enemySpeaking)
		{
			yield return null;
		}
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_830H");
		if (playedEntity.GetLettuceAbilityOwner().GetCardId() == "LETL_830H")
		{
			switch (cardID)
			{
			case "LETL_830P1_01":
			case "LETL_830P1_03":
				GameState.Get().SetBusy(busy: true);
				yield return MissionPlayVOOnce(bossActor, VO_LETL_830H_Male_Furbolg_Attack_01);
				GameState.Get().SetBusy(busy: false);
				break;
			case "LETL_830P2_01":
			case "LETL_830P2_03":
				GameState.Get().SetBusy(busy: true);
				yield return MissionPlayVOOnce(bossActor, VO_LETL_830H_Male_Furbolg_Attack_02);
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_830H");
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_830H");
		if (entity.GetCardId() == "LETL_830H")
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_830H");
		if (turn == 1)
		{
			yield return MissionPlayVOOnce(bossActor, m_introLine);
		}
	}
}
