using System.Collections;
using System.Collections.Generic;

public class LettuceBoss_LETL_856H_VoHandler : VoPlaybackHandler
{
	private static readonly AssetReference VO_CommanderIchman_Male_Human_Intro_01 = new AssetReference("VO_CommanderIchman_Male_Human_Intro_01.prefab:50290f586dc98244c9c66c676114da38");

	private static readonly AssetReference VO_CommanderIchman_Male_Human_Idle_01 = new AssetReference("VO_CommanderIchman_Male_Human_Idle_01.prefab:362ec012070015f44a2fd2e008cb287a");

	private static readonly AssetReference VO_CommanderIchman_Male_Human_Attack_01 = new AssetReference("VO_CommanderIchman_Male_Human_Attack_01.prefab:3a75a2dc2cb40544395c79ff72d4b81b");

	private static readonly AssetReference VO_CommanderIchman_Male_Human_Death_01 = new AssetReference("VO_CommanderIchman_Male_Human_Death_01.prefab:4443357feb9eff24cbd96481439f4809");

	private List<string> m_IdleLines = new List<string> { VO_CommanderIchman_Male_Human_Idle_01 };

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> soundFiles = new List<string> { VO_CommanderIchman_Male_Human_Intro_01, VO_CommanderIchman_Male_Human_Idle_01, VO_CommanderIchman_Male_Human_Attack_01, VO_CommanderIchman_Male_Human_Death_01 };
		SetBossVOLines(soundFiles);
		foreach (string soundFile in soundFiles)
		{
			GameState.Get().GetGameEntity().PreloadSound(soundFile);
		}
	}

	public override void OnCreateGame()
	{
		base.OnCreateGame();
		m_introLine = VO_CommanderIchman_Male_Human_Intro_01;
		m_deathLine = VO_CommanderIchman_Male_Human_Death_01;
	}

	public override IEnumerator RespondToWillPlayCardWithTiming(string cardID, Entity playedEntity)
	{
		while (m_enemySpeaking)
		{
			yield return null;
		}
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_856H");
		if (playedEntity.GetLettuceAbilityOwner().GetCardId() == "LETL_856H" && cardID == "LETL_856P1_01")
		{
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(bossActor, VO_CommanderIchman_Male_Human_Attack_01);
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_856H");
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_856H");
		if (entity.GetCardId() == "LETL_856H")
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_856H");
		if (turn == 1)
		{
			yield return MissionPlayVOOnce(bossActor, m_introLine);
		}
	}
}
