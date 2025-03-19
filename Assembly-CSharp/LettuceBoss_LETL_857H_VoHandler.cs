using System.Collections;
using System.Collections.Generic;

public class LettuceBoss_LETL_857H_VoHandler : VoPlaybackHandler
{
	private static readonly AssetReference VO_AV_284_Female_Human_Attack_01 = new AssetReference("VO_AV_284_Female_Human_Attack_01.prefab:3f9f1fdb27458be4894f634b5df7f41b");

	private static readonly AssetReference VO_BalindaStonehearth_Female_Human_Bark_02 = new AssetReference("VO_BalindaStonehearth_Female_Human_Bark_02.prefab:ba3f8d60daa9d734185f2e94a210a49f");

	private static readonly AssetReference VO_AV_284_Female_Human_Death_01 = new AssetReference("VO_BalindaStonehearth_Female_Human_Death_01.prefab:98889acd8a4de0142bc8e5d3b46cf6f9");

	private static readonly AssetReference VO_BalindaStonehearth_Female_Human_Bark_08 = new AssetReference("VO_BalindaStonehearth_Female_Human_Bark_08.prefab:6fd08c045520fc0458367aaa6dd1281d");

	private List<string> m_IdleLines = new List<string> { VO_BalindaStonehearth_Female_Human_Bark_02 };

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> soundFiles = new List<string> { VO_AV_284_Female_Human_Attack_01, VO_BalindaStonehearth_Female_Human_Bark_02, VO_AV_284_Female_Human_Death_01, VO_BalindaStonehearth_Female_Human_Bark_08 };
		SetBossVOLines(soundFiles);
		foreach (string soundFile in soundFiles)
		{
			GameState.Get().GetGameEntity().PreloadSound(soundFile);
		}
	}

	public override void OnCreateGame()
	{
		base.OnCreateGame();
		m_introLine = VO_BalindaStonehearth_Female_Human_Bark_08;
		m_deathLine = VO_AV_284_Female_Human_Death_01;
	}

	public override IEnumerator RespondToWillPlayCardWithTiming(string cardID, Entity playedEntity)
	{
		while (m_enemySpeaking)
		{
			yield return null;
		}
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_857H");
		if (playedEntity.GetLettuceAbilityOwner().GetCardId() == "LETL_857H" && cardID == "LETL_857P1_01")
		{
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVOOnce(bossActor, VO_AV_284_Female_Human_Attack_01);
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_857H");
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_857H");
		if (entity.GetCardId() == "LETL_857H")
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_857H");
		if (turn == 1)
		{
			yield return MissionPlayVOOnce(bossActor, m_introLine);
		}
	}
}
