using System.Collections;
using System.Collections.Generic;

public class LettuceBoss_LT23_800H_VoHandler : VoPlaybackHandler
{
	private static readonly AssetReference VO_TSC_962_Male_Murloc_Attack_01 = new AssetReference("VO_TSC_962_Male_Murloc_Attack_01.prefab:aa994c5abc5e2cd4ab4180dc6b455a18");

	private static readonly AssetReference VO_TSC_962_Male_Murloc_Death_01 = new AssetReference("VO_TSC_962_Male_Murloc_Death_01.prefab:7bef4971902ef174dabeb1fe59a91302");

	private static readonly AssetReference VO_TSC_962_Male_Murloc_Play_01 = new AssetReference("VO_TSC_962_Male_Murloc_Play_01.prefab:8d5ea0dcfceedc249ae3a45c3264f790");

	private static readonly AssetReference VO_TSC_962t_Male_Murloc_Play_01 = new AssetReference("VO_TSC_962t_Male_Murloc_Play_01.prefab:230cb701b1ca1cf4b84929e8c3b4f4f3");

	private List<string> m_IdleLines = new List<string> { VO_TSC_962_Male_Murloc_Play_01 };

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> soundFiles = new List<string> { VO_TSC_962_Male_Murloc_Attack_01, VO_TSC_962_Male_Murloc_Death_01, VO_TSC_962_Male_Murloc_Play_01, VO_TSC_962t_Male_Murloc_Play_01 };
		SetBossVOLines(soundFiles);
		foreach (string soundFile in soundFiles)
		{
			GameState.Get().GetGameEntity().PreloadSound(soundFile);
		}
	}

	public override void OnCreateGame()
	{
		base.OnCreateGame();
		m_introLine = VO_TSC_962_Male_Murloc_Play_01;
		m_deathLine = VO_TSC_962_Male_Murloc_Death_01;
	}

	public override IEnumerator RespondToWillPlayCardWithTiming(string cardID, Entity playedEntity)
	{
		while (m_enemySpeaking)
		{
			yield return null;
		}
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LT23_800H");
		if (playedEntity.GetLettuceAbilityOwner().GetCardId() == "LT23_800H" && cardID == "LT23_800P1")
		{
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVOOnce(bossActor, VO_TSC_962t_Male_Murloc_Play_01);
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LT23_800H");
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LT23_800H");
		if (entity.GetCardId() == "LT23_800H")
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LT23_800H");
		if (turn == 1)
		{
			yield return MissionPlayVOOnce(bossActor, m_introLine);
		}
	}
}
