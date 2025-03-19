using System.Collections;
using System.Collections.Generic;

public class LettuceBoss_LT23_812H_VoHandler : VoPlaybackHandler
{
	private static readonly AssetReference VO_TaranZhu_Male_Pandaren_LETL_Attack_01 = new AssetReference("VO_TaranZhu_Male_Pandaren_LETL_Attack_01.prefab:51d880e5e2691a848a3ff40bfa1ac3d1");

	private static readonly AssetReference VO_TaranZhu_Male_Pandaren_LETL_Attack_02 = new AssetReference("VO_TaranZhu_Male_Pandaren_LETL_Attack_02.prefab:9435a50bafff3894c9aeda14d0a357af");

	private static readonly AssetReference VO_TaranZhu_Male_Pandaren_LETL_Death_01 = new AssetReference("VO_TaranZhu_Male_Pandaren_LETL_Death_01.prefab:df78807536bbd5f4c90e9e5d2a9d822c");

	private static readonly AssetReference VO_TaranZhu_Male_Pandaren_LETL_Idle_01 = new AssetReference("VO_TaranZhu_Male_Pandaren_LETL_Idle_01.prefab:da4ab8b755cc257499e1cd6571ebef3a");

	private static readonly AssetReference VO_TaranZhu_Male_Pandaren_LETL_Intro_01 = new AssetReference("VO_TaranZhu_Male_Pandaren_LETL_Intro_01.prefab:ebc4380e2cb8cbc469b30f3120c89106");

	private List<string> m_IdleLines = new List<string> { VO_TaranZhu_Male_Pandaren_LETL_Idle_01 };

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> soundFiles = new List<string> { VO_TaranZhu_Male_Pandaren_LETL_Intro_01, VO_TaranZhu_Male_Pandaren_LETL_Attack_01, VO_TaranZhu_Male_Pandaren_LETL_Attack_02, VO_TaranZhu_Male_Pandaren_LETL_Idle_01, VO_TaranZhu_Male_Pandaren_LETL_Death_01 };
		SetBossVOLines(soundFiles);
		foreach (string soundFile in soundFiles)
		{
			GameState.Get().GetGameEntity().PreloadSound(soundFile);
		}
	}

	public override void OnCreateGame()
	{
		base.OnCreateGame();
		m_introLine = VO_TaranZhu_Male_Pandaren_LETL_Intro_01;
		m_deathLine = VO_TaranZhu_Male_Pandaren_LETL_Death_01;
	}

	public override IEnumerator RespondToWillPlayCardWithTiming(string cardID, Entity playedEntity)
	{
		while (m_enemySpeaking)
		{
			yield return null;
		}
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LT23_812H");
		if (playedEntity.GetLettuceAbilityOwner().GetCardId() == "LT23_812H")
		{
			switch (cardID)
			{
			case "LETL_024P3_03":
				GameState.Get().SetBusy(busy: true);
				yield return MissionPlayVOOnce(bossActor, VO_TaranZhu_Male_Pandaren_LETL_Attack_02);
				GameState.Get().SetBusy(busy: false);
				break;
			case "LETL_024P3_05":
				GameState.Get().SetBusy(busy: true);
				yield return MissionPlayVOOnce(bossActor, VO_TaranZhu_Male_Pandaren_LETL_Attack_02);
				GameState.Get().SetBusy(busy: false);
				break;
			case "LT22_007P2_03":
				GameState.Get().SetBusy(busy: true);
				yield return MissionPlayVOOnce(bossActor, VO_TaranZhu_Male_Pandaren_LETL_Attack_01);
				GameState.Get().SetBusy(busy: false);
				break;
			case "LT22_007P2_05":
				GameState.Get().SetBusy(busy: true);
				yield return MissionPlayVOOnce(bossActor, VO_TaranZhu_Male_Pandaren_LETL_Attack_01);
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LT23_812H");
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LT23_812H");
		if (entity.GetCardId() == "LT23_812H")
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LT23_812H");
		if (turn == 1)
		{
			yield return MissionPlayVOOnce(bossActor, m_introLine);
		}
	}
}
