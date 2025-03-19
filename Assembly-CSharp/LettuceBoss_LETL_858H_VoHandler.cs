using System.Collections;
using System.Collections.Generic;

public class LettuceBoss_LETL_858H_VoHandler : VoPlaybackHandler
{
	private static readonly AssetReference VO_IvusForestLord_Male_Elemental_Attack_01 = new AssetReference("VO_IvusForestLord_Male_Elemental_Attack_01.prefab:fcec32ed74edaa1489183abcfc082777");

	private static readonly AssetReference VO_IvusForestLord_Male_Elemental_Attack_02 = new AssetReference("VO_IvusForestLord_Male_Elemental_Attack_02.prefab:e32c9da1ba658e443a1aefba638d4b4a");

	private static readonly AssetReference VO_IvusForestLord_Male_Elemental_Death_01 = new AssetReference("VO_IvusForestLord_Male_Elemental_Death_01.prefab:f191132e0fe93964ea43dbd8ba26c9ed");

	private static readonly AssetReference VO_IvusForestLord_Male_Elemental_Idle_01 = new AssetReference("VO_IvusForestLord_Male_Elemental_Idle_01.prefab:bb90fe7bfc9560b4387ad9adf100cf32");

	private static readonly AssetReference VO_IvusForestLord_Male_Elemental_Intro_01 = new AssetReference("VO_IvusForestLord_Male_Elemental_Intro_01.prefab:bd66bfac62c752749bc2574cab3c09d0");

	private List<string> m_IdleLines = new List<string> { VO_IvusForestLord_Male_Elemental_Idle_01 };

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> soundFiles = new List<string> { VO_IvusForestLord_Male_Elemental_Attack_01, VO_IvusForestLord_Male_Elemental_Death_01, VO_IvusForestLord_Male_Elemental_Idle_01, VO_IvusForestLord_Male_Elemental_Intro_01 };
		SetBossVOLines(soundFiles);
		foreach (string soundFile in soundFiles)
		{
			GameState.Get().GetGameEntity().PreloadSound(soundFile);
		}
	}

	public override void OnCreateGame()
	{
		base.OnCreateGame();
		m_introLine = VO_IvusForestLord_Male_Elemental_Intro_01;
		m_deathLine = VO_IvusForestLord_Male_Elemental_Death_01;
	}

	public override IEnumerator RespondToWillPlayCardWithTiming(string cardID, Entity playedEntity)
	{
		while (m_enemySpeaking)
		{
			yield return null;
		}
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_858H");
		if (playedEntity.GetLettuceAbilityOwner().GetCardId() == "LETL_858H" && cardID == "LETL_858P1_01")
		{
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(bossActor, VO_IvusForestLord_Male_Elemental_Attack_01);
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_858H");
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_858H");
		if (entity.GetCardId() == "LETL_858H")
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_858H");
		if (turn == 1)
		{
			yield return MissionPlayVOOnce(bossActor, m_introLine);
		}
	}
}
