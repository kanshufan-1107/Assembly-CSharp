using System.Collections;
using System.Collections.Generic;

public class LettuceBoss_LT23_818H_VoHandler : VoPlaybackHandler
{
	private static readonly AssetReference VO_RingmasterWhatley_Male_Worgen_LETL_Attack_01 = new AssetReference("VO_RingmasterWhatley_Male_Worgen_LETL_Attack_01.prefab:2b96ef5f6b726b44483a874e706ffc43");

	private static readonly AssetReference VO_RingmasterWhatley_Male_Worgen_LETL_Attack_02 = new AssetReference("VO_RingmasterWhatley_Male_Worgen_LETL_Attack_02.prefab:a1787841f812b33448f6ecce17cc8af0");

	private static readonly AssetReference VO_RingmasterWhatley_Male_Worgen_LETL_Death_01 = new AssetReference("VO_RingmasterWhatley_Male_Worgen_LETL_Death_01.prefab:5456390430fffa34aa3435a62922a29c");

	private static readonly AssetReference VO_RingmasterWhatley_Male_Worgen_LETL_Idle_01 = new AssetReference("VO_RingmasterWhatley_Male_Worgen_LETL_Idle_01.prefab:8d56b944d6b561f438c0a51d8047ab45");

	private static readonly AssetReference VO_RingmasterWhatley_Male_Worgen_LETL_Idle_02 = new AssetReference("VO_RingmasterWhatley_Male_Worgen_LETL_Idle_02.prefab:f971734d8745a0944a7e88503199460a");

	private static readonly AssetReference VO_RingmasterWhatley_Male_Worgen_LETL_Intro_01 = new AssetReference("VO_RingmasterWhatley_Male_Worgen_LETL_Intro_01.prefab:b662b6bc3bc2acf43bbce3d4ccb6219c");

	private List<string> m_IdleLines = new List<string> { VO_RingmasterWhatley_Male_Worgen_LETL_Idle_01, VO_RingmasterWhatley_Male_Worgen_LETL_Idle_02 };

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> soundFiles = new List<string> { VO_RingmasterWhatley_Male_Worgen_LETL_Intro_01, VO_RingmasterWhatley_Male_Worgen_LETL_Attack_01, VO_RingmasterWhatley_Male_Worgen_LETL_Attack_02, VO_RingmasterWhatley_Male_Worgen_LETL_Idle_01, VO_RingmasterWhatley_Male_Worgen_LETL_Idle_02, VO_RingmasterWhatley_Male_Worgen_LETL_Death_01 };
		SetBossVOLines(soundFiles);
		foreach (string soundFile in soundFiles)
		{
			GameState.Get().GetGameEntity().PreloadSound(soundFile);
		}
	}

	public override void OnCreateGame()
	{
		base.OnCreateGame();
		m_introLine = VO_RingmasterWhatley_Male_Worgen_LETL_Intro_01;
		m_deathLine = VO_RingmasterWhatley_Male_Worgen_LETL_Death_01;
	}

	public override IEnumerator RespondToWillPlayCardWithTiming(string cardID, Entity playedEntity)
	{
		while (m_enemySpeaking)
		{
			yield return null;
		}
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LT23_818H");
		if (!(playedEntity.GetLettuceAbilityOwner().GetCardId() == "LT23_818H"))
		{
			yield break;
		}
		if (!(cardID == "LT23_818P1"))
		{
			if (cardID == "LT23_818P2")
			{
				GameState.Get().SetBusy(busy: true);
				yield return MissionPlayVOOnce(bossActor, VO_RingmasterWhatley_Male_Worgen_LETL_Attack_02);
				GameState.Get().SetBusy(busy: false);
			}
		}
		else
		{
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVOOnce(bossActor, VO_RingmasterWhatley_Male_Worgen_LETL_Attack_01);
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LT23_818H");
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LT23_818H");
		if (entity.GetCardId() == "LT23_818H")
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LT23_818H");
		if (turn == 1)
		{
			yield return MissionPlayVOOnce(bossActor, m_introLine);
		}
	}
}
