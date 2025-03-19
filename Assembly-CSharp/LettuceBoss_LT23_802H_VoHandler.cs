using System.Collections;
using System.Collections.Generic;

public class LettuceBoss_LT23_802H_VoHandler : VoPlaybackHandler
{
	private static readonly AssetReference VO_LadyAlisstra_Female_Naga_Attack_01 = new AssetReference("VO_LadyAlisstra_Female_Naga_Attack_01.prefab:3a715ab13b403df4697aaf2ae1527aa2");

	private static readonly AssetReference VO_LadyAlisstra_Female_Naga_Death_01 = new AssetReference("VO_LadyAlisstra_Female_Naga_Death_01.prefab:bc08263bb78b56847a46aee46c3915a1");

	private static readonly AssetReference VO_LadyAlisstra_Female_Naga_Idle_01 = new AssetReference("VO_LadyAlisstra_Female_Naga_Idle_01.prefab:832c6cbf53d71074d9d38274641ef024");

	private static readonly AssetReference VO_LadyAlisstra_Female_Naga_Intro_01 = new AssetReference("VO_LadyAlisstra_Female_Naga_Intro_01.prefab:39e0cf37548eed342af52f99bb6a5cc0");

	private List<string> m_IdleLines = new List<string> { VO_LadyAlisstra_Female_Naga_Idle_01 };

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> soundFiles = new List<string> { VO_LadyAlisstra_Female_Naga_Attack_01, VO_LadyAlisstra_Female_Naga_Death_01, VO_LadyAlisstra_Female_Naga_Idle_01, VO_LadyAlisstra_Female_Naga_Intro_01 };
		SetBossVOLines(soundFiles);
		foreach (string soundFile in soundFiles)
		{
			GameState.Get().GetGameEntity().PreloadSound(soundFile);
		}
	}

	public override void OnCreateGame()
	{
		base.OnCreateGame();
		m_introLine = VO_LadyAlisstra_Female_Naga_Intro_01;
		m_deathLine = VO_LadyAlisstra_Female_Naga_Death_01;
	}

	public override IEnumerator RespondToWillPlayCardWithTiming(string cardID, Entity playedEntity)
	{
		while (m_enemySpeaking)
		{
			yield return null;
		}
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LT23_802H");
		if (playedEntity.GetLettuceAbilityOwner().GetCardId() == "LT23_802H" && cardID == "LT23_802P2")
		{
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVOOnce(bossActor, VO_LadyAlisstra_Female_Naga_Attack_01);
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LT23_802H");
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LT23_802H");
		if (entity.GetCardId() == "LT23_802H")
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LT23_802H");
		if (turn == 1)
		{
			yield return MissionPlayVOOnce(bossActor, m_introLine);
		}
	}
}
