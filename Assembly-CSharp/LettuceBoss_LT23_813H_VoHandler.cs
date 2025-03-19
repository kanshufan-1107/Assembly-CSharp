using System.Collections;
using System.Collections.Generic;

public class LettuceBoss_LT23_813H_VoHandler : VoPlaybackHandler
{
	private static readonly AssetReference VO_Xaril_Male_Mantid_LETL_Attack_01 = new AssetReference("VO_Xaril_Male_Mantid_LETL_Attack_01.prefab:7d0d29d594294e748aa2767018aaa703");

	private static readonly AssetReference VO_Xaril_Male_Mantid_LETL_Attack_02 = new AssetReference("VO_Xaril_Male_Mantid_LETL_Attack_02.prefab:76962cebd9f527f479dd7527c0eed4d3");

	private static readonly AssetReference VO_Xaril_Male_Mantid_LETL_Death_01 = new AssetReference("VO_Xaril_Male_Mantid_LETL_Death_01.prefab:7de9c877d1413cb45a60cf47e648ce0a");

	private static readonly AssetReference VO_Xaril_Male_Mantid_LETL_Idle_01 = new AssetReference("VO_Xaril_Male_Mantid_LETL_Idle_01.prefab:aeac2603aec1f594abe820b430690265");

	private static readonly AssetReference VO_Xaril_Male_Mantid_LETL_Intro_01 = new AssetReference("VO_Xaril_Male_Mantid_LETL_Intro_01.prefab:323182c2b89f6f044af32da43ae81564");

	private List<string> m_IdleLines = new List<string> { VO_Xaril_Male_Mantid_LETL_Idle_01 };

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> soundFiles = new List<string> { VO_Xaril_Male_Mantid_LETL_Intro_01, VO_Xaril_Male_Mantid_LETL_Attack_01, VO_Xaril_Male_Mantid_LETL_Attack_02, VO_Xaril_Male_Mantid_LETL_Idle_01, VO_Xaril_Male_Mantid_LETL_Death_01 };
		SetBossVOLines(soundFiles);
		foreach (string soundFile in soundFiles)
		{
			GameState.Get().GetGameEntity().PreloadSound(soundFile);
		}
	}

	public override void OnCreateGame()
	{
		base.OnCreateGame();
		m_introLine = VO_Xaril_Male_Mantid_LETL_Intro_01;
		m_deathLine = VO_Xaril_Male_Mantid_LETL_Death_01;
	}

	public override IEnumerator RespondToWillPlayCardWithTiming(string cardID, Entity playedEntity)
	{
		while (m_enemySpeaking)
		{
			yield return null;
		}
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LT23_813H");
		if (playedEntity.GetLettuceAbilityOwner().GetCardId() == "LT23_813H")
		{
			switch (cardID)
			{
			case "LT23_813P1":
				GameState.Get().SetBusy(busy: true);
				yield return MissionPlayVOOnce(bossActor, VO_Xaril_Male_Mantid_LETL_Attack_01);
				GameState.Get().SetBusy(busy: false);
				break;
			case "LETL_019P1_03":
				GameState.Get().SetBusy(busy: true);
				yield return MissionPlayVOOnce(bossActor, VO_Xaril_Male_Mantid_LETL_Attack_02);
				GameState.Get().SetBusy(busy: false);
				break;
			case "LETL_019P1_05":
				GameState.Get().SetBusy(busy: true);
				yield return MissionPlayVOOnce(bossActor, VO_Xaril_Male_Mantid_LETL_Attack_02);
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LT23_813H");
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LT23_813H");
		if (entity.GetCardId() == "LT23_813H")
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LT23_813H");
		if (turn == 1)
		{
			yield return MissionPlayVOOnce(bossActor, m_introLine);
		}
	}
}
