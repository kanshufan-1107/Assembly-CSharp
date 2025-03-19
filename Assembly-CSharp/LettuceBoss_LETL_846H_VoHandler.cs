using System.Collections;
using System.Collections.Generic;

public class LettuceBoss_LETL_846H_VoHandler : VoPlaybackHandler
{
	private static readonly AssetReference VO_LETL_846H_Male_Dragon_Attack_01 = new AssetReference("VO_LETL_846H_Male_Dragon_Attack_01.prefab:3753e37b081972f4794f890a095b54ed");

	private static readonly AssetReference VO_LETL_846H_Male_Dragon_Attack_02 = new AssetReference("VO_LETL_846H_Male_Dragon_Attack_02.prefab:9912dbea914f6014d888d084a5de343c");

	private static readonly AssetReference VO_LETL_846H_Male_Dragon_Death_01 = new AssetReference("VO_LETL_846H_Male_Dragon_Death_01.prefab:bf61206cd0f7c0343b5f8b8641f4afb9");

	private static readonly AssetReference VO_LETL_846H_Male_Dragon_Idle_01 = new AssetReference("VO_LETL_846H_Male_Dragon_Idle_01.prefab:712f13da3a24e284385e2f1b5e77f4cf");

	private static readonly AssetReference VO_LETL_846H_Male_Dragon_Intro_01 = new AssetReference("VO_LETL_846H_Male_Dragon_Intro_01.prefab:60a635df010d16d4cb359f1320cd3938");

	private List<string> m_IdleLines = new List<string> { VO_LETL_846H_Male_Dragon_Idle_01 };

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> soundFiles = new List<string> { VO_LETL_846H_Male_Dragon_Attack_01, VO_LETL_846H_Male_Dragon_Attack_02, VO_LETL_846H_Male_Dragon_Death_01, VO_LETL_846H_Male_Dragon_Idle_01, VO_LETL_846H_Male_Dragon_Intro_01 };
		SetBossVOLines(soundFiles);
		foreach (string soundFile in soundFiles)
		{
			GameState.Get().GetGameEntity().PreloadSound(soundFile);
		}
	}

	public override void OnCreateGame()
	{
		base.OnCreateGame();
		m_introLine = VO_LETL_846H_Male_Dragon_Intro_01;
		m_deathLine = VO_LETL_846H_Male_Dragon_Death_01;
	}

	public override IEnumerator RespondToWillPlayCardWithTiming(string cardID, Entity playedEntity)
	{
		while (m_enemySpeaking)
		{
			yield return null;
		}
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_846H");
		if (!(playedEntity.GetLettuceAbilityOwner().GetCardId() == "LETL_846H"))
		{
			yield break;
		}
		if (!(cardID == "LETL_846P3_01"))
		{
			if (cardID == "LETL_846P4_01")
			{
				GameState.Get().SetBusy(busy: true);
				yield return MissionPlayVOOnce(bossActor, VO_LETL_846H_Male_Dragon_Attack_01);
				GameState.Get().SetBusy(busy: false);
			}
		}
		else
		{
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVOOnce(bossActor, VO_LETL_846H_Male_Dragon_Attack_01);
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_846H");
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_846H");
		if (entity.GetCardId() == "LETL_846H")
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_846H");
		if (turn == 1)
		{
			yield return MissionPlayVOOnce(bossActor, m_introLine);
		}
	}
}
