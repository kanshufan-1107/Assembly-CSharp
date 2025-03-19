using System.Collections;
using System.Collections.Generic;

public class LettuceBoss_LT23_805H_VoHandler : VoPlaybackHandler
{
	private static readonly AssetReference VO_AcolyteOfNZoth_Male_Human_Attack_02 = new AssetReference("VO_AcolyteOfNZoth_Male_Human_Attack_02.prefab:04ca7c9a9e6808c42aeb6ed4a17f4636");

	private static readonly AssetReference VO_AcolyteOfNZoth_Male_Human_Death_01 = new AssetReference("VO_AcolyteOfNZoth_Male_Human_Death_01.prefab:8939de60b02c0cb4a87b3472d8832cae");

	private static readonly AssetReference VO_AcolyteOfNZoth_Male_Human_Idle_01 = new AssetReference("VO_AcolyteOfNZoth_Male_Human_Idle_01.prefab:fe5107aefac42dd46b2744734b48c104");

	private static readonly AssetReference VO_AcolyteOfNZoth_Male_Human_Intro_01 = new AssetReference("VO_AcolyteOfNZoth_Male_Human_Intro_01.prefab:4c61662228801f943a9924bca1474c80");

	private List<string> m_IdleLines = new List<string> { VO_AcolyteOfNZoth_Male_Human_Idle_01 };

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> soundFiles = new List<string> { VO_AcolyteOfNZoth_Male_Human_Death_01, VO_AcolyteOfNZoth_Male_Human_Idle_01, VO_AcolyteOfNZoth_Male_Human_Intro_01, VO_AcolyteOfNZoth_Male_Human_Attack_02 };
		SetBossVOLines(soundFiles);
		foreach (string soundFile in soundFiles)
		{
			GameState.Get().GetGameEntity().PreloadSound(soundFile);
		}
	}

	public override void OnCreateGame()
	{
		base.OnCreateGame();
		m_introLine = VO_AcolyteOfNZoth_Male_Human_Intro_01;
		m_deathLine = VO_AcolyteOfNZoth_Male_Human_Death_01;
	}

	public override IEnumerator RespondToWillPlayCardWithTiming(string cardID, Entity playedEntity)
	{
		while (m_enemySpeaking)
		{
			yield return null;
		}
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LT23_805H");
		if (playedEntity.GetLettuceAbilityOwner().GetCardId() == "LT23_805H" && cardID == "LT23_805P1")
		{
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVOOnce(bossActor, VO_AcolyteOfNZoth_Male_Human_Attack_02);
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LT23_805H");
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LT23_805H");
		if (entity.GetCardId() == "LT23_805H")
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LT23_805H");
		if (turn == 1)
		{
			yield return MissionPlayVOOnce(bossActor, m_introLine);
		}
	}
}
