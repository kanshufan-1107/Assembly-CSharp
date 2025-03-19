using System.Collections;
using System.Collections.Generic;

public class LettuceBoss_LT23_821H_VoHandler : VoPlaybackHandler
{
	private static readonly AssetReference VO_Maestra_Female_Orc_LETL_Attack_03 = new AssetReference("VO_Maestra_Female_Orc_LETL_Attack_03.prefab:ec8904d190c4e7c48969df0b42862cc1");

	private static readonly AssetReference VO_Maestra_Female_Orc_LETL_Death_01 = new AssetReference("VO_Maestra_Female_Orc_LETL_Death_01.prefab:36524b468e0f9cd4f9c493838706be93");

	private static readonly AssetReference VO_Maestra_Female_Orc_LETL_Idle_01 = new AssetReference("VO_Maestra_Female_Orc_LETL_Idle_01.prefab:c0d88723ec6aa104eaaa1ae55abafedf");

	private static readonly AssetReference VO_Maestra_Female_Orc_LETL_Intro_01 = new AssetReference("VO_Maestra_Female_Orc_LETL_Intro_01.prefab:0432e7581966f82439bce9d76cb1aa43");

	private List<string> m_IdleLines = new List<string> { VO_Maestra_Female_Orc_LETL_Idle_01 };

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> soundFiles = new List<string> { VO_Maestra_Female_Orc_LETL_Intro_01, VO_Maestra_Female_Orc_LETL_Attack_03, VO_Maestra_Female_Orc_LETL_Idle_01, VO_Maestra_Female_Orc_LETL_Death_01 };
		SetBossVOLines(soundFiles);
		foreach (string soundFile in soundFiles)
		{
			GameState.Get().GetGameEntity().PreloadSound(soundFile);
		}
	}

	public override void OnCreateGame()
	{
		base.OnCreateGame();
		m_introLine = VO_Maestra_Female_Orc_LETL_Intro_01;
		m_deathLine = VO_Maestra_Female_Orc_LETL_Death_01;
	}

	public override IEnumerator RespondToWillPlayCardWithTiming(string cardID, Entity playedEntity)
	{
		while (m_enemySpeaking)
		{
			yield return null;
		}
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LT23_821H");
		if (playedEntity.GetLettuceAbilityOwner().GetCardId() == "LT23_821H" && cardID == "LT23_821P1")
		{
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVOOnce(bossActor, VO_Maestra_Female_Orc_LETL_Attack_03);
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LT23_821H");
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LT23_821H");
		if (entity.GetCardId() == "LT23_821H")
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LT23_821H");
		if (turn == 1)
		{
			yield return MissionPlayVOOnce(bossActor, m_introLine);
		}
	}
}
