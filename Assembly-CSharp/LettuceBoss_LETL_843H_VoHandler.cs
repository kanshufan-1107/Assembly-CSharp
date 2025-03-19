using System.Collections;
using System.Collections.Generic;

public class LettuceBoss_LETL_843H_VoHandler : VoPlaybackHandler
{
	private static readonly AssetReference VO_LETL_843H_Male_Dragonkin_Attack_01 = new AssetReference("VO_LETL_843H_Male_Dragonkin_Attack_01.prefab:a2b0065e82e378745a05bfe528a54422");

	private static readonly AssetReference VO_LETL_843H_Male_Dragonkin_Attack_02 = new AssetReference("VO_LETL_843H_Male_Dragonkin_Attack_02.prefab:47756de9ab0498d408a007a4986ec465");

	private static readonly AssetReference VO_LETL_843H_Male_Dragonkin_Death_01 = new AssetReference("VO_LETL_843H_Male_Dragonkin_Death_01.prefab:450b37c081fa37843a7cf1aeec27cf2f");

	private static readonly AssetReference VO_LETL_843H_Male_Dragonkin_Idle_01 = new AssetReference("VO_LETL_843H_Male_Dragonkin_Idle_01.prefab:9cb173a8359417e4b99d2273551a4c62");

	private static readonly AssetReference VO_LETL_843H_Male_Dragonkin_Intro_01 = new AssetReference("VO_LETL_843H_Male_Dragonkin_Intro_01.prefab:aab8cacd0c5d0cc429914009291ee574");

	private List<string> m_IdleLines = new List<string> { VO_LETL_843H_Male_Dragonkin_Idle_01 };

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> soundFiles = new List<string> { VO_LETL_843H_Male_Dragonkin_Attack_01, VO_LETL_843H_Male_Dragonkin_Attack_02, VO_LETL_843H_Male_Dragonkin_Death_01, VO_LETL_843H_Male_Dragonkin_Idle_01, VO_LETL_843H_Male_Dragonkin_Intro_01 };
		SetBossVOLines(soundFiles);
		foreach (string soundFile in soundFiles)
		{
			GameState.Get().GetGameEntity().PreloadSound(soundFile);
		}
	}

	public override void OnCreateGame()
	{
		base.OnCreateGame();
		m_introLine = VO_LETL_843H_Male_Dragonkin_Intro_01;
		m_deathLine = VO_LETL_843H_Male_Dragonkin_Death_01;
	}

	public override IEnumerator RespondToWillPlayCardWithTiming(string cardID, Entity playedEntity)
	{
		while (m_enemySpeaking)
		{
			yield return null;
		}
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_843H");
		if (playedEntity.GetLettuceAbilityOwner().GetCardId() == "LETL_843H")
		{
			switch (cardID)
			{
			case "LETL_843P2_01":
			case "LETL_843P2_02":
				GameState.Get().SetBusy(busy: true);
				yield return MissionPlayVOOnce(bossActor, VO_LETL_843H_Male_Dragonkin_Attack_01);
				GameState.Get().SetBusy(busy: false);
				break;
			case "LETL_843P1_01":
			case "LETL_842P1_02":
				GameState.Get().SetBusy(busy: true);
				yield return MissionPlayVOOnce(bossActor, VO_LETL_843H_Male_Dragonkin_Attack_02);
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_843H");
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_843H");
		if (entity.GetCardId() == "LETL_843H")
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_843H");
		if (turn == 1)
		{
			yield return MissionPlayVOOnce(bossActor, m_introLine);
		}
	}
}
