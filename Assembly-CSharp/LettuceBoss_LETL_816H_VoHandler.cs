using System.Collections;
using System.Collections.Generic;

public class LettuceBoss_LETL_816H_VoHandler : VoPlaybackHandler
{
	private static readonly AssetReference VO_LETL_816H_Male_Elemental_Attack_01 = new AssetReference("VO_LETL_816H_Male_Elemental_Attack_01.prefab:7a37cda844e99754cb63defec34fa2f7");

	private static readonly AssetReference VO_LETL_816H_Male_Elemental_Attack_02 = new AssetReference("VO_LETL_816H_Male_Elemental_Attack_02.prefab:1a7ebd8913bc70e499d2a2d0d117a014");

	private static readonly AssetReference VO_LETL_816H_Male_Elemental_Death_01 = new AssetReference("VO_LETL_816H_Male_Elemental_Death_01.prefab:f1528eb6e8224ef4abfce3fdf19fa532");

	private static readonly AssetReference VO_LETL_816H_Male_Elemental_Idle_01 = new AssetReference("VO_LETL_816H_Male_Elemental_Idle_01.prefab:da4dd45a4ee171e46a47e5336d76ca3b");

	private static readonly AssetReference VO_LETL_816H_Male_Elemental_Intro_01 = new AssetReference("VO_LETL_816H_Male_Elemental_Intro_01.prefab:fc1eb6f6a155248458b305c17efedb73");

	private static readonly AssetReference VO_LETL_816H_Male_Elemental_Intro_02 = new AssetReference("VO_LETL_816H_Male_Elemental_Intro_02.prefab:233a9d840e918a04b89c5e8b9ceb04d5");

	private List<string> m_IdleLines = new List<string> { VO_LETL_816H_Male_Elemental_Idle_01 };

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> soundFiles = new List<string> { VO_LETL_816H_Male_Elemental_Attack_01, VO_LETL_816H_Male_Elemental_Attack_02, VO_LETL_816H_Male_Elemental_Death_01, VO_LETL_816H_Male_Elemental_Idle_01, VO_LETL_816H_Male_Elemental_Intro_01, VO_LETL_816H_Male_Elemental_Intro_02 };
		SetBossVOLines(soundFiles);
		foreach (string soundFile in soundFiles)
		{
			GameState.Get().GetGameEntity().PreloadSound(soundFile);
		}
	}

	public override void OnCreateGame()
	{
		base.OnCreateGame();
		m_introLine = VO_LETL_816H_Male_Elemental_Intro_01;
		m_deathLine = VO_LETL_816H_Male_Elemental_Death_01;
	}

	public override IEnumerator RespondToWillPlayCardWithTiming(string cardID, Entity playedEntity)
	{
		while (m_enemySpeaking)
		{
			yield return null;
		}
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_816H");
		if (playedEntity.GetLettuceAbilityOwner().GetCardId() == "LETL_816H")
		{
			switch (cardID)
			{
			case "LETL_816P1_01":
			case "LETL_816P1_03":
				GameState.Get().SetBusy(busy: true);
				yield return MissionPlayVOOnce(bossActor, VO_LETL_816H_Male_Elemental_Attack_01);
				GameState.Get().SetBusy(busy: false);
				break;
			case "LETL_816P2_01":
				GameState.Get().SetBusy(busy: true);
				yield return MissionPlayVOOnce(bossActor, VO_LETL_816H_Male_Elemental_Attack_02);
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_816H");
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_816H");
		if (entity.GetCardId() == "LETL_816H")
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_816H");
		if (turn == 1)
		{
			yield return MissionPlayVOOnce(bossActor, m_introLine);
		}
	}
}
