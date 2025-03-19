using System.Collections;
using System.Collections.Generic;

public class LettuceBoss_LT23_825H_VoHandler : VoPlaybackHandler
{
	private static readonly AssetReference CowKing_TB_SPT_DPromo_Hero2_Death = new AssetReference("CowKing_TB_SPT_DPromo_Hero2_Death.prefab:62dd0de3b827da94c9550809489a97c6");

	private static readonly AssetReference CowKing_TB_SPT_DPromo_Hero2_Play = new AssetReference("CowKing_TB_SPT_DPromo_Hero2_Play.prefab:2e748a031af8a6d46a9aa1a35da82756");

	private List<string> m_IdleLines = new List<string> { CowKing_TB_SPT_DPromo_Hero2_Play };

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> soundFiles = new List<string> { CowKing_TB_SPT_DPromo_Hero2_Play, CowKing_TB_SPT_DPromo_Hero2_Death };
		SetBossVOLines(soundFiles);
		foreach (string soundFile in soundFiles)
		{
			GameState.Get().GetGameEntity().PreloadSound(soundFile);
		}
	}

	public override void OnCreateGame()
	{
		base.OnCreateGame();
		m_introLine = CowKing_TB_SPT_DPromo_Hero2_Play;
		m_deathLine = CowKing_TB_SPT_DPromo_Hero2_Death;
	}

	public override IEnumerator RespondToWillPlayCardWithTiming(string cardID, Entity playedEntity)
	{
		while (m_enemySpeaking)
		{
			yield return null;
		}
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LT23_825H");
		if (playedEntity.GetLettuceAbilityOwner().GetCardId() == "LT23_825H" && cardID == "LT23_825P1")
		{
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVOOnce(bossActor, CowKing_TB_SPT_DPromo_Hero2_Play);
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LT23_825H");
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LT23_825H");
		if (entity.GetCardId() == "LT23_825H")
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LT23_825H");
		if (turn == 1)
		{
			yield return MissionPlayVOOnce(bossActor, m_introLine);
		}
	}
}
