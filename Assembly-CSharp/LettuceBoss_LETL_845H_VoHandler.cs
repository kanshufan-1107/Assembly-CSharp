using System.Collections;
using System.Collections.Generic;

public class LettuceBoss_LETL_845H_VoHandler : VoPlaybackHandler
{
	private static readonly AssetReference VO_LETL_845H_Male_Dragonkin_Attack_01 = new AssetReference("VO_LETL_845H_Male_Dragonkin_Attack_01.prefab:29cfb93ab47fa53429eae273eb82f4b8");

	private static readonly AssetReference VO_LETL_845H_Male_Dragonkin_Attack_02 = new AssetReference("VO_LETL_845H_Male_Dragonkin_Attack_02.prefab:17a6278444341804fa4f139a485307c4");

	private static readonly AssetReference VO_LETL_845H_Male_Dragonkin_Death_01 = new AssetReference("VO_LETL_845H_Male_Dragonkin_Death_01.prefab:4bc4aa1243216744eb2c0ec6076b31f1");

	private static readonly AssetReference VO_LETL_845H_Male_Dragonkin_Idle_01 = new AssetReference("VO_LETL_845H_Male_Dragonkin_Idle_01.prefab:8273ad8b09ecfbf4283955e65726fc5b");

	private static readonly AssetReference VO_LETL_845H_Male_Dragonkin_Intro_01 = new AssetReference("VO_LETL_845H_Male_Dragonkin_Intro_01.prefab:4438e0df50187164688d35ccf015c358");

	private List<string> m_IdleLines = new List<string> { VO_LETL_845H_Male_Dragonkin_Idle_01 };

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> soundFiles = new List<string> { VO_LETL_845H_Male_Dragonkin_Attack_01, VO_LETL_845H_Male_Dragonkin_Attack_02, VO_LETL_845H_Male_Dragonkin_Death_01, VO_LETL_845H_Male_Dragonkin_Idle_01, VO_LETL_845H_Male_Dragonkin_Intro_01 };
		SetBossVOLines(soundFiles);
		foreach (string soundFile in soundFiles)
		{
			GameState.Get().GetGameEntity().PreloadSound(soundFile);
		}
	}

	public override void OnCreateGame()
	{
		base.OnCreateGame();
		m_introLine = VO_LETL_845H_Male_Dragonkin_Intro_01;
		m_deathLine = VO_LETL_845H_Male_Dragonkin_Death_01;
	}

	public override IEnumerator RespondToWillPlayCardWithTiming(string cardID, Entity playedEntity)
	{
		while (m_enemySpeaking)
		{
			yield return null;
		}
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_845H");
		if (!(playedEntity.GetLettuceAbilityOwner().GetCardId() == "LETL_845H"))
		{
			yield break;
		}
		if (!(cardID == "LETL_844P1_01"))
		{
			if (cardID == "LETL_844P2_01")
			{
				GameState.Get().SetBusy(busy: true);
				yield return MissionPlayVOOnce(bossActor, VO_LETL_845H_Male_Dragonkin_Attack_02);
				GameState.Get().SetBusy(busy: false);
			}
		}
		else
		{
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVOOnce(bossActor, VO_LETL_845H_Male_Dragonkin_Attack_01);
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_845H");
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_845H");
		if (entity.GetCardId() == "LETL_845H")
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_845H");
		if (turn == 1)
		{
			yield return MissionPlayVOOnce(bossActor, m_introLine);
		}
	}
}
