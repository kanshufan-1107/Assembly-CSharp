using System.Collections;
using System.Collections.Generic;

public class LettuceBoss_LETL_827H_VoHandler : VoPlaybackHandler
{
	private static readonly AssetReference VO_LETL_827H_Male_Satyr_Attack_01 = new AssetReference("VO_LETL_827H_Male_Satyr_Attack_01.prefab:f1adae6989959ea45967a4ffbd3e49ea");

	private static readonly AssetReference VO_LETL_827H_Male_Satyr_Attack_02 = new AssetReference("VO_LETL_827H_Male_Satyr_Attack_02.prefab:53d135560ef09cb4dba5f87145d86b78");

	private static readonly AssetReference VO_LETL_827H_Male_Satyr_Death_01 = new AssetReference("VO_LETL_827H_Male_Satyr_Death_01.prefab:d2470aeb83d83da458e72089b031b5ce");

	private static readonly AssetReference VO_LETL_827H_Male_Satyr_Idle_01 = new AssetReference("VO_LETL_827H_Male_Satyr_Idle_01.prefab:774dde4d0fcad194cbdac154ccb52599");

	private static readonly AssetReference VO_LETL_827H_Male_Satyr_Intro_01 = new AssetReference("VO_LETL_827H_Male_Satyr_Intro_01.prefab:c2ad2e1bc00e3624fb966195753ff3dc");

	private List<string> m_IdleLines = new List<string> { VO_LETL_827H_Male_Satyr_Idle_01 };

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> soundFiles = new List<string> { VO_LETL_827H_Male_Satyr_Attack_01, VO_LETL_827H_Male_Satyr_Attack_02, VO_LETL_827H_Male_Satyr_Death_01, VO_LETL_827H_Male_Satyr_Idle_01, VO_LETL_827H_Male_Satyr_Intro_01 };
		SetBossVOLines(soundFiles);
		foreach (string soundFile in soundFiles)
		{
			GameState.Get().GetGameEntity().PreloadSound(soundFile);
		}
	}

	public override void OnCreateGame()
	{
		base.OnCreateGame();
		m_introLine = VO_LETL_827H_Male_Satyr_Intro_01;
		m_deathLine = VO_LETL_827H_Male_Satyr_Death_01;
	}

	public override IEnumerator RespondToWillPlayCardWithTiming(string cardID, Entity playedEntity)
	{
		while (m_enemySpeaking)
		{
			yield return null;
		}
		FindEnemyActorInPlayByDesignCode("LETL_827H");
		Entity lettuceAbilityOwner = playedEntity.GetLettuceAbilityOwner();
		Actor actorThatCastedAbility = lettuceAbilityOwner.GetCard().GetActor();
		if (lettuceAbilityOwner.GetCardId() == "LETL_827H")
		{
			switch (cardID)
			{
			case "LETL_827P1_01":
			case "LETL_827P1_05":
			case "LETL_257_03":
			case "LETL_257_05":
				GameState.Get().SetBusy(busy: true);
				yield return MissionPlayVOOnce(actorThatCastedAbility, VO_LETL_827H_Male_Satyr_Attack_01);
				GameState.Get().SetBusy(busy: false);
				break;
			case "LETL_009P9_01":
			case "LETL_009P9_03":
				GameState.Get().SetBusy(busy: true);
				yield return MissionPlayVOOnce(actorThatCastedAbility, VO_LETL_827H_Male_Satyr_Attack_02);
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_827H");
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_827H");
		if (entity.GetCardId() == "LETL_827H")
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_827H");
		if (turn == 1)
		{
			yield return MissionPlayVOOnce(bossActor, m_introLine);
		}
	}
}
