using System.Collections;
using System.Collections.Generic;

public class LettuceBoss_LT23_817H_VoHandler : VoPlaybackHandler
{
	private static readonly AssetReference VO_YShaarj_Male_OldGod_LETL_Attack_02 = new AssetReference("VO_YShaarj_Male_OldGod_LETL_Attack_02.prefab:c40fe49ecaf573149a66a2adbddd6d82");

	private static readonly AssetReference VO_YShaarj_Male_OldGod_LETL_Attack_03 = new AssetReference("VO_YShaarj_Male_OldGod_LETL_Attack_03.prefab:542abb4375d882d44bbe0f0fee02c421");

	private static readonly AssetReference VO_YShaarj_Male_OldGod_LETL_Death_02 = new AssetReference("VO_YShaarj_Male_OldGod_LETL_Death_02.prefab:251828353146b014aa7d3d834c0f5f26");

	private static readonly AssetReference VO_YShaarj_Male_OldGod_LETL_Idle_01 = new AssetReference("VO_YShaarj_Male_OldGod_LETL_Idle_01.prefab:4f7f8e0aba2cf43499d88093fd35d5b8");

	private static readonly AssetReference VO_YShaarj_Male_OldGod_LETL_Intro_02 = new AssetReference("VO_YShaarj_Male_OldGod_LETL_Intro_02.prefab:e1c687dba0681c1408e75128d862b05c");

	private List<string> m_IdleLines = new List<string> { VO_YShaarj_Male_OldGod_LETL_Idle_01 };

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> soundFiles = new List<string> { VO_YShaarj_Male_OldGod_LETL_Intro_02, VO_YShaarj_Male_OldGod_LETL_Attack_02, VO_YShaarj_Male_OldGod_LETL_Attack_03, VO_YShaarj_Male_OldGod_LETL_Idle_01, VO_YShaarj_Male_OldGod_LETL_Death_02 };
		SetBossVOLines(soundFiles);
		foreach (string soundFile in soundFiles)
		{
			GameState.Get().GetGameEntity().PreloadSound(soundFile);
		}
	}

	public override void OnCreateGame()
	{
		base.OnCreateGame();
		m_introLine = VO_YShaarj_Male_OldGod_LETL_Intro_02;
		m_deathLine = VO_YShaarj_Male_OldGod_LETL_Death_02;
	}

	public override IEnumerator RespondToWillPlayCardWithTiming(string cardID, Entity playedEntity)
	{
		while (m_enemySpeaking)
		{
			yield return null;
		}
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LT23_817H");
		if (playedEntity.GetLettuceAbilityOwner().GetCardId() == "LT23_817H")
		{
			switch (cardID)
			{
			case "LT23_817P1":
				GameState.Get().SetBusy(busy: true);
				yield return MissionPlayVOOnce(bossActor, VO_YShaarj_Male_OldGod_LETL_Attack_02);
				GameState.Get().SetBusy(busy: false);
				break;
			case "LT23_028P2_03":
				GameState.Get().SetBusy(busy: true);
				yield return MissionPlayVOOnce(bossActor, VO_YShaarj_Male_OldGod_LETL_Attack_03);
				GameState.Get().SetBusy(busy: false);
				break;
			case "LT23_028P2_05":
				GameState.Get().SetBusy(busy: true);
				yield return MissionPlayVOOnce(bossActor, VO_YShaarj_Male_OldGod_LETL_Attack_03);
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LT23_817H");
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LT23_817H");
		if (entity.GetCardId() == "LT23_817H")
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LT23_817H");
		if (turn == 1)
		{
			yield return MissionPlayVOOnce(bossActor, m_introLine);
		}
	}
}
