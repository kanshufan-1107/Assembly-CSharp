using System.Collections;
using System.Collections.Generic;

public class LettuceBoss_LT23_807H_VoHandler : VoPlaybackHandler
{
	private static readonly AssetReference VO_CoralElemental_Male_Elemental_LETL_Attack_01 = new AssetReference("VO_CoralElemental_Male_Elemental_LETL_Attack_01.prefab:eb3929c967dc477488c9e4e44266fc39");

	private static readonly AssetReference VO_CoralElemental_Male_Elemental_LETL_Attack_02 = new AssetReference("VO_CoralElemental_Male_Elemental_LETL_Attack_02.prefab:be0f2aa367134b549997b832a505218f");

	private static readonly AssetReference VO_CoralElemental_Male_Elemental_LETL_Death_01 = new AssetReference("VO_CoralElemental_Male_Elemental_LETL_Death_01.prefab:f5369b9ea249dfb4fa62181275c56534");

	private static readonly AssetReference VO_CoralElemental_Male_Elemental_LETL_Idle_01 = new AssetReference("VO_CoralElemental_Male_Elemental_LETL_Idle_01.prefab:447b4ee72b611c84f9237adce18527a1");

	private static readonly AssetReference VO_CoralElemental_Male_Elemental_LETL_Intro_01 = new AssetReference("VO_CoralElemental_Male_Elemental_LETL_Intro_01.prefab:7d21a39c67daf6d4191f86b92a456fa3");

	private List<string> m_IdleLines = new List<string> { VO_CoralElemental_Male_Elemental_LETL_Idle_01 };

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> soundFiles = new List<string> { VO_CoralElemental_Male_Elemental_LETL_Death_01, VO_CoralElemental_Male_Elemental_LETL_Idle_01, VO_CoralElemental_Male_Elemental_LETL_Intro_01, VO_CoralElemental_Male_Elemental_LETL_Attack_02, VO_CoralElemental_Male_Elemental_LETL_Attack_01 };
		SetBossVOLines(soundFiles);
		foreach (string soundFile in soundFiles)
		{
			GameState.Get().GetGameEntity().PreloadSound(soundFile);
		}
	}

	public override void OnCreateGame()
	{
		base.OnCreateGame();
		m_introLine = VO_CoralElemental_Male_Elemental_LETL_Intro_01;
		m_deathLine = VO_CoralElemental_Male_Elemental_LETL_Death_01;
	}

	public override IEnumerator RespondToWillPlayCardWithTiming(string cardID, Entity playedEntity)
	{
		while (m_enemySpeaking)
		{
			yield return null;
		}
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LT23_807H");
		if (playedEntity.GetLettuceAbilityOwner().GetCardId() == "LT23_807H" && cardID == "LT23_807P1")
		{
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVOOnce(bossActor, VO_CoralElemental_Male_Elemental_LETL_Attack_01);
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LT23_807H");
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LT23_807H");
		if (entity.GetCardId() == "LT23_807H")
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LT23_807H");
		if (turn == 1)
		{
			yield return MissionPlayVOOnce(bossActor, m_introLine);
		}
	}
}
