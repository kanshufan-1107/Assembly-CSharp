using System.Collections;
using System.Collections.Generic;

public class LettuceBoss_LETL_831H_VoHandler : VoPlaybackHandler
{
	private static readonly AssetReference VO_LETL_831H_Male_NightElf_Attack_01 = new AssetReference("VO_LETL_831H_Male_NightElf_Attack_01.prefab:ae8635d9bf07bd4409fc1774d89646f7");

	private static readonly AssetReference VO_LETL_831H_Male_NightElf_Attack_02 = new AssetReference("VO_LETL_831H_Male_NightElf_Attack_02.prefab:8ffc464b1ec07944c81ef6d35e65e450");

	private static readonly AssetReference VO_LETL_831H_Male_NightElf_Death_01 = new AssetReference("VO_LETL_831H_Male_NightElf_Death_01.prefab:b97ee5fb1e543974d92f4310fbd899ca");

	private static readonly AssetReference VO_LETL_831H_Male_NightElf_Idle_01 = new AssetReference("VO_LETL_831H_Male_NightElf_Idle_01.prefab:234f92d5bc87dbe429a9241995d7e905");

	private static readonly AssetReference VO_LETL_831H_Male_NightElf_Intro_01 = new AssetReference("VO_LETL_831H_Male_NightElf_Intro_01.prefab:55a6e14f344f6e545a55c3081566227e");

	private List<string> m_IdleLines = new List<string> { VO_LETL_831H_Male_NightElf_Idle_01 };

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> soundFiles = new List<string> { VO_LETL_831H_Male_NightElf_Attack_01, VO_LETL_831H_Male_NightElf_Attack_02, VO_LETL_831H_Male_NightElf_Death_01, VO_LETL_831H_Male_NightElf_Idle_01, VO_LETL_831H_Male_NightElf_Intro_01 };
		SetBossVOLines(soundFiles);
		foreach (string soundFile in soundFiles)
		{
			GameState.Get().GetGameEntity().PreloadSound(soundFile);
		}
	}

	public override void OnCreateGame()
	{
		base.OnCreateGame();
		m_introLine = VO_LETL_831H_Male_NightElf_Intro_01;
		m_deathLine = VO_LETL_831H_Male_NightElf_Death_01;
	}

	public override IEnumerator RespondToWillPlayCardWithTiming(string cardID, Entity playedEntity)
	{
		while (m_enemySpeaking)
		{
			yield return null;
		}
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_831H");
		if (playedEntity.GetLettuceAbilityOwner().GetCardId() == "LETL_831H")
		{
			switch (cardID)
			{
			case "LETL_015P9_05":
				GameState.Get().SetBusy(busy: true);
				yield return MissionPlayVOOnce(bossActor, VO_LETL_831H_Male_NightElf_Attack_02);
				GameState.Get().SetBusy(busy: false);
				break;
			case "LETL_013P3_05":
			case "LETL_017P7_05":
				GameState.Get().SetBusy(busy: true);
				yield return MissionPlayVOOnce(bossActor, VO_LETL_831H_Male_NightElf_Attack_01);
				GameState.Get().SetBusy(busy: false);
				break;
			}
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_831H");
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_831H");
		if (entity.GetCardId() == "LETL_831H")
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_831H");
		if (turn == 1)
		{
			yield return MissionPlayVOOnce(bossActor, m_introLine);
		}
	}
}
