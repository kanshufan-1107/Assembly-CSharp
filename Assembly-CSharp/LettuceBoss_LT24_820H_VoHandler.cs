using System.Collections;
using System.Collections.Generic;

public class LettuceBoss_LT24_820H_VoHandler : VoPlaybackHandler
{
	private static readonly AssetReference VO_WhiteKing_LETL_Attack = new AssetReference("VO_WhiteKing_LETL_Attack.prefab:4ead53188c2fb934b830fd8b37b78118");

	private static readonly AssetReference VO_WhiteKing_LETL_Death = new AssetReference("VO_WhiteKing_LETL_Death.prefab:88f9fe615cfa00a47bcce2f593ccf1a2");

	private static readonly AssetReference VO_WhiteKing_LETL_Idle = new AssetReference("VO_WhiteKing_LETL_Idle.prefab:739a12af67d94f44fa7872344d46b888");

	private static readonly AssetReference VO_WhiteKing_LETL_Intro = new AssetReference("VO_WhiteKing_LETL_Intro.prefab:ab96a6b632c39594faa8d7a61e5117dd");

	private List<string> m_IdleLines = new List<string> { VO_WhiteKing_LETL_Idle };

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> soundFiles = new List<string> { VO_WhiteKing_LETL_Intro, VO_WhiteKing_LETL_Attack, VO_WhiteKing_LETL_Idle, VO_WhiteKing_LETL_Death };
		SetBossVOLines(soundFiles);
		foreach (string soundFile in soundFiles)
		{
			GameState.Get().GetGameEntity().PreloadSound(soundFile);
		}
	}

	public override void OnCreateGame()
	{
		base.OnCreateGame();
		m_introLine = VO_WhiteKing_LETL_Intro;
		m_deathLine = VO_WhiteKing_LETL_Death;
	}

	public override IEnumerator RespondToWillPlayCardWithTiming(string cardID, Entity playedEntity)
	{
		while (m_enemySpeaking)
		{
			yield return null;
		}
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LT24_820H");
		if (playedEntity.GetLettuceAbilityOwner().GetCardId() == "LT24_820H" && cardID == "LT24_820P3")
		{
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVOOnce(bossActor, VO_WhiteKing_LETL_Attack);
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LT24_820H");
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LT24_820H");
		if (entity.GetCardId() == "LT24_820H")
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LT24_820H");
		if (turn == 1)
		{
			yield return MissionPlayVOOnce(bossActor, m_introLine);
		}
	}
}
