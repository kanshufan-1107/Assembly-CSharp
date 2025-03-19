using System.Collections;
using System.Collections.Generic;

public class LettuceBoss_LT24_817H_VoHandler : VoPlaybackHandler
{
	private static readonly AssetReference VO_TheCurator_Male_Mech_LETL_Attack_01 = new AssetReference("VO_TheCurator_Male_Mech_LETL_Attack_01.prefab:344f3c4b70e39904195fbfdc01cdebc3");

	private static readonly AssetReference VO_TheCurator_Male_Mech_LETL_Attack_02 = new AssetReference("VO_TheCurator_Male_Mech_LETL_Attack_02.prefab:04d0e6d4f626c5b4dbf09247c96abf6d");

	private static readonly AssetReference VO_TheCurator_Male_Mech_LETL_Death_01 = new AssetReference("VO_TheCurator_Male_Mech_LETL_Death_01.prefab:9ac49afd140927349befb4ef3ddb4ad9");

	private static readonly AssetReference VO_TheCurator_Male_Mech_LETL_Idle_01 = new AssetReference("VO_TheCurator_Male_Mech_LETL_Idle_01.prefab:67f7eba207c43cf489ac86a3f3832ca8");

	private static readonly AssetReference VO_TheCurator_Male_Mech_LETL_Idle_02 = new AssetReference("VO_TheCurator_Male_Mech_LETL_Idle_02.prefab:99dc89f8ecbb4794892e9eea3f1b341b");

	private static readonly AssetReference VO_TheCurator_Male_Mech_LETL_Intro_01 = new AssetReference("VO_TheCurator_Male_Mech_LETL_Intro_01.prefab:8f0a0733f41d65f40bf8d29c8bb72ed9");

	private List<string> m_IdleLines = new List<string> { VO_TheCurator_Male_Mech_LETL_Idle_01, VO_TheCurator_Male_Mech_LETL_Idle_02 };

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> soundFiles = new List<string> { VO_TheCurator_Male_Mech_LETL_Intro_01, VO_TheCurator_Male_Mech_LETL_Attack_01, VO_TheCurator_Male_Mech_LETL_Attack_02, VO_TheCurator_Male_Mech_LETL_Idle_01, VO_TheCurator_Male_Mech_LETL_Idle_02, VO_TheCurator_Male_Mech_LETL_Death_01 };
		SetBossVOLines(soundFiles);
		foreach (string soundFile in soundFiles)
		{
			GameState.Get().GetGameEntity().PreloadSound(soundFile);
		}
	}

	public override void OnCreateGame()
	{
		base.OnCreateGame();
		m_introLine = VO_TheCurator_Male_Mech_LETL_Intro_01;
		m_deathLine = VO_TheCurator_Male_Mech_LETL_Death_01;
	}

	public override IEnumerator RespondToWillPlayCardWithTiming(string cardID, Entity playedEntity)
	{
		while (m_enemySpeaking)
		{
			yield return null;
		}
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LT24_817H");
		if (!(playedEntity.GetLettuceAbilityOwner().GetCardId() == "LT24_817H"))
		{
			yield break;
		}
		if (!(cardID == "LT24_817P1"))
		{
			if (cardID == "LT24_817P2")
			{
				GameState.Get().SetBusy(busy: true);
				yield return MissionPlayVOOnce(bossActor, VO_TheCurator_Male_Mech_LETL_Attack_02);
				GameState.Get().SetBusy(busy: false);
			}
		}
		else
		{
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVOOnce(bossActor, VO_TheCurator_Male_Mech_LETL_Attack_01);
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LT24_817H");
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LT24_817H");
		if (entity.GetCardId() == "LT24_817H")
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LT24_817H");
		if (turn == 1)
		{
			yield return MissionPlayVOOnce(bossActor, m_introLine);
		}
	}
}
