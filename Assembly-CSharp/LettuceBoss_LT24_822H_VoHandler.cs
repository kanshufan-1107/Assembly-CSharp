using System.Collections;
using System.Collections.Generic;

public class LettuceBoss_LT24_822H_VoHandler : VoPlaybackHandler
{
	private static readonly AssetReference VO_ImageOfMedivh_Male_Human_LETL_Attack_01 = new AssetReference("VO_ImageOfMedivh_Male_Human_LETL_Attack_01.prefab:a821189f68a733b4e89b4b91ba83b31b");

	private static readonly AssetReference VO_ImageOfMedivh_Male_Human_LETL_Attack_02 = new AssetReference("VO_ImageOfMedivh_Male_Human_LETL_Attack_02.prefab:6088ff795e1e6104a9123c53aca402c0");

	private static readonly AssetReference VO_ImageOfMedivh_Male_Human_LETL_Death_01 = new AssetReference("VO_ImageOfMedivh_Male_Human_LETL_Death_01.prefab:db536a843f699094d8740ffbfcb4770d");

	private static readonly AssetReference VO_ImageOfMedivh_Male_Human_LETL_Idle_01 = new AssetReference("VO_ImageOfMedivh_Male_Human_LETL_Idle_01.prefab:5ad12130d18a1594785cc22bd0a283b4");

	private static readonly AssetReference VO_ImageOfMedivh_Male_Human_LETL_Intro_01 = new AssetReference("VO_ImageOfMedivh_Male_Human_LETL_Intro_01.prefab:4c012cfe8eeae1b4aaa391a1f16030e9");

	private List<string> m_IdleLines = new List<string> { VO_ImageOfMedivh_Male_Human_LETL_Idle_01 };

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> soundFiles = new List<string> { VO_ImageOfMedivh_Male_Human_LETL_Intro_01, VO_ImageOfMedivh_Male_Human_LETL_Attack_01, VO_ImageOfMedivh_Male_Human_LETL_Attack_02, VO_ImageOfMedivh_Male_Human_LETL_Idle_01, VO_ImageOfMedivh_Male_Human_LETL_Death_01 };
		SetBossVOLines(soundFiles);
		foreach (string soundFile in soundFiles)
		{
			GameState.Get().GetGameEntity().PreloadSound(soundFile);
		}
	}

	public override void OnCreateGame()
	{
		base.OnCreateGame();
		m_introLine = VO_ImageOfMedivh_Male_Human_LETL_Intro_01;
		m_deathLine = VO_ImageOfMedivh_Male_Human_LETL_Death_01;
	}

	public override IEnumerator RespondToWillPlayCardWithTiming(string cardID, Entity playedEntity)
	{
		while (m_enemySpeaking)
		{
			yield return null;
		}
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LT24_822H");
		if (!(playedEntity.GetLettuceAbilityOwner().GetCardId() == "LT24_822H"))
		{
			yield break;
		}
		if (!(cardID == "LT24_822P1"))
		{
			if (cardID == "LT24_822P2")
			{
				GameState.Get().SetBusy(busy: true);
				yield return MissionPlayVOOnce(bossActor, VO_ImageOfMedivh_Male_Human_LETL_Attack_01);
				GameState.Get().SetBusy(busy: false);
			}
		}
		else
		{
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVOOnce(bossActor, VO_ImageOfMedivh_Male_Human_LETL_Attack_02);
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LT24_822H");
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LT24_822H");
		if (entity.GetCardId() == "LT24_822H")
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LT24_822H");
		if (turn == 1)
		{
			yield return MissionPlayVOOnce(bossActor, m_introLine);
		}
	}
}
