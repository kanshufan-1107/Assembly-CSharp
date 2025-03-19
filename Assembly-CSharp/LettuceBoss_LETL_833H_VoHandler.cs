using System.Collections;
using System.Collections.Generic;

public class LettuceBoss_LETL_833H_VoHandler : VoPlaybackHandler
{
	private static readonly AssetReference VO_LETL_833H_Female_Orc_Attack_01 = new AssetReference("VO_LETL_833H_Female_Orc_Attack_01.prefab:c161e569962766548b7532e8e541f78f");

	private static readonly AssetReference VO_LETL_833H_Female_Orc_Attack_02 = new AssetReference("VO_LETL_833H_Female_Orc_Attack_02.prefab:3765845fa8be180428a3ba22a3c8be4f");

	private static readonly AssetReference VO_LETL_833H_Female_Orc_Death_01 = new AssetReference("VO_LETL_833H_Female_Orc_Death_01.prefab:0a71fca7a26d26e4cb5d17e286aef353");

	private static readonly AssetReference VO_LETL_833H_Female_Orc_Idle_01 = new AssetReference("VO_LETL_833H_Female_Orc_Idle_01.prefab:d1eccba0d354c634d967599c3e748a1a");

	private static readonly AssetReference VO_LETL_833H_Female_Orc_Intro_01 = new AssetReference("VO_LETL_833H_Female_Orc_Intro_01.prefab:553a7ddb39249fb4d8126d305f1df53c");

	private List<string> m_IdleLines = new List<string> { VO_LETL_833H_Female_Orc_Idle_01 };

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> soundFiles = new List<string> { VO_LETL_833H_Female_Orc_Attack_01, VO_LETL_833H_Female_Orc_Attack_02, VO_LETL_833H_Female_Orc_Death_01, VO_LETL_833H_Female_Orc_Idle_01, VO_LETL_833H_Female_Orc_Intro_01 };
		SetBossVOLines(soundFiles);
		foreach (string soundFile in soundFiles)
		{
			GameState.Get().GetGameEntity().PreloadSound(soundFile);
		}
	}

	public override void OnCreateGame()
	{
		base.OnCreateGame();
		m_introLine = VO_LETL_833H_Female_Orc_Intro_01;
		m_deathLine = VO_LETL_833H_Female_Orc_Death_01;
	}

	public override IEnumerator RespondToWillPlayCardWithTiming(string cardID, Entity playedEntity)
	{
		while (m_enemySpeaking)
		{
			yield return null;
		}
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_833H");
		if (!(playedEntity.GetLettuceAbilityOwner().GetCardId() == "LETL_833H"))
		{
			yield break;
		}
		if (!(cardID == "LETL_003P1_05"))
		{
			if (cardID == "LETL_034P3_03")
			{
				GameState.Get().SetBusy(busy: true);
				yield return MissionPlayVOOnce(bossActor, VO_LETL_833H_Female_Orc_Attack_02);
				GameState.Get().SetBusy(busy: false);
			}
		}
		else
		{
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVOOnce(bossActor, VO_LETL_833H_Female_Orc_Attack_01);
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_833H");
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_833H");
		if (entity.GetCardId() == "LETL_833H")
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_833H");
		if (turn == 1)
		{
			yield return MissionPlayVOOnce(bossActor, m_introLine);
		}
	}
}
