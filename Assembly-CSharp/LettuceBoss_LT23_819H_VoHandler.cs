using System.Collections;
using System.Collections.Generic;

public class LettuceBoss_LT23_819H_VoHandler : VoPlaybackHandler
{
	private static readonly AssetReference VO_Sayge_Male_Worgen_LETL_Attack_01 = new AssetReference("VO_Sayge_Male_Worgen_LETL_Attack_01.prefab:6d2e139b5b565e141ad42c091c38bbe0");

	private static readonly AssetReference VO_Sayge_Male_Worgen_LETL_Attack_02 = new AssetReference("VO_Sayge_Male_Worgen_LETL_Attack_02.prefab:dcadfa8053fbc8a46b1667f71e7ca649");

	private static readonly AssetReference VO_Sayge_Male_Worgen_LETL_Death_01 = new AssetReference("VO_Sayge_Male_Worgen_LETL_Death_01.prefab:d91b98f921720c44280c65300443675d");

	private static readonly AssetReference VO_Sayge_Male_Worgen_LETL_Idle_01 = new AssetReference("VO_Sayge_Male_Worgen_LETL_Idle_01.prefab:4aae4ce13b6081a498c0b0e3ff02a06e");

	private static readonly AssetReference VO_Sayge_Male_Worgen_LETL_Intro_01 = new AssetReference("VO_Sayge_Male_Worgen_LETL_Intro_01.prefab:b3ceffbd3f497a84e8e517f126e24f2e");

	private List<string> m_IdleLines = new List<string> { VO_Sayge_Male_Worgen_LETL_Idle_01 };

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> soundFiles = new List<string> { VO_Sayge_Male_Worgen_LETL_Intro_01, VO_Sayge_Male_Worgen_LETL_Attack_01, VO_Sayge_Male_Worgen_LETL_Attack_02, VO_Sayge_Male_Worgen_LETL_Idle_01, VO_Sayge_Male_Worgen_LETL_Death_01 };
		SetBossVOLines(soundFiles);
		foreach (string soundFile in soundFiles)
		{
			GameState.Get().GetGameEntity().PreloadSound(soundFile);
		}
	}

	public override void OnCreateGame()
	{
		base.OnCreateGame();
		m_introLine = VO_Sayge_Male_Worgen_LETL_Intro_01;
		m_deathLine = VO_Sayge_Male_Worgen_LETL_Death_01;
	}

	public override IEnumerator RespondToWillPlayCardWithTiming(string cardID, Entity playedEntity)
	{
		while (m_enemySpeaking)
		{
			yield return null;
		}
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LT23_819H");
		if (!(playedEntity.GetLettuceAbilityOwner().GetCardId() == "LT23_819H"))
		{
			yield break;
		}
		if (!(cardID == "LT23_819P1"))
		{
			if (cardID == "LT23_819P2")
			{
				GameState.Get().SetBusy(busy: true);
				yield return MissionPlayVOOnce(bossActor, VO_Sayge_Male_Worgen_LETL_Attack_02);
				GameState.Get().SetBusy(busy: false);
			}
		}
		else
		{
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVOOnce(bossActor, VO_Sayge_Male_Worgen_LETL_Attack_01);
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LT23_819H");
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LT23_819H");
		if (entity.GetCardId() == "LT23_819H")
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LT23_819H");
		if (turn == 1)
		{
			yield return MissionPlayVOOnce(bossActor, m_introLine);
		}
	}
}
