using System.Collections;
using System.Collections.Generic;

public class LettuceBoss_LT23_816H_VoHandler : VoPlaybackHandler
{
	private static readonly AssetReference VO_GarroshHellscream_Male_Orc_LETL_Attack_02 = new AssetReference("VO_GarroshHellscream_Male_Orc_LETL_Attack_02.prefab:5a7f3a0c0bd23ee40a7e80b530c88db4");

	private static readonly AssetReference VO_GarroshHellscream_Male_Orc_LETL_Attack_03 = new AssetReference("VO_GarroshHellscream_Male_Orc_LETL_Attack_03.prefab:c2bb2213bdfc1ce419ceb7fb9af29f5a");

	private static readonly AssetReference VO_GarroshHellscream_Male_Orc_LETL_Death_01 = new AssetReference("VO_GarroshHellscream_Male_Orc_LETL_Death_01.prefab:bac71eedd575d4c479f2d73eb42f806b");

	private static readonly AssetReference VO_GarroshHellscream_Male_Orc_LETL_Idle_01 = new AssetReference("VO_GarroshHellscream_Male_Orc_LETL_Idle_01.prefab:92a4e8187d63e34478fbe42114176acd");

	private static readonly AssetReference VO_GarroshHellscream_Male_Orc_LETL_Intro_01 = new AssetReference("VO_GarroshHellscream_Male_Orc_LETL_Intro_01.prefab:f3b824187d1e9b84488df31f0294c188");

	private List<string> m_IdleLines = new List<string> { VO_GarroshHellscream_Male_Orc_LETL_Idle_01 };

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> soundFiles = new List<string> { VO_GarroshHellscream_Male_Orc_LETL_Intro_01, VO_GarroshHellscream_Male_Orc_LETL_Attack_02, VO_GarroshHellscream_Male_Orc_LETL_Attack_03, VO_GarroshHellscream_Male_Orc_LETL_Idle_01, VO_GarroshHellscream_Male_Orc_LETL_Death_01 };
		SetBossVOLines(soundFiles);
		foreach (string soundFile in soundFiles)
		{
			GameState.Get().GetGameEntity().PreloadSound(soundFile);
		}
	}

	public override void OnCreateGame()
	{
		base.OnCreateGame();
		m_introLine = VO_GarroshHellscream_Male_Orc_LETL_Intro_01;
		m_deathLine = VO_GarroshHellscream_Male_Orc_LETL_Death_01;
	}

	public override IEnumerator RespondToWillPlayCardWithTiming(string cardID, Entity playedEntity)
	{
		while (m_enemySpeaking)
		{
			yield return null;
		}
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LT23_816H");
		if (!(playedEntity.GetLettuceAbilityOwner().GetCardId() == "LT23_816H"))
		{
			yield break;
		}
		if (!(cardID == "LT23_816P1"))
		{
			if (cardID == "LT23_816P2")
			{
				GameState.Get().SetBusy(busy: true);
				yield return MissionPlayVOOnce(bossActor, VO_GarroshHellscream_Male_Orc_LETL_Attack_02);
				GameState.Get().SetBusy(busy: false);
			}
		}
		else
		{
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVOOnce(bossActor, VO_GarroshHellscream_Male_Orc_LETL_Attack_03);
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LT23_816H");
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LT23_816H");
		if (entity.GetCardId() == "LT23_816H")
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LT23_816H");
		if (turn == 1)
		{
			yield return MissionPlayVOOnce(bossActor, m_introLine);
		}
	}
}
