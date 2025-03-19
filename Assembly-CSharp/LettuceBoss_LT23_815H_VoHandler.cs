using System.Collections;
using System.Collections.Generic;

public class LettuceBoss_LT23_815H_VoHandler : VoPlaybackHandler
{
	private static readonly AssetReference VO_EmpressShekzara_Female_Mantid_LETL_Attack_02 = new AssetReference("VO_EmpressShekzara_Female_Mantid_LETL_Attack_02.prefab:8eb5a7d74717f4a4f959c59a19bfdf85");

	private static readonly AssetReference VO_EmpressShekzara_Female_Mantid_LETL_Attack_06 = new AssetReference("VO_EmpressShekzara_Female_Mantid_LETL_Attack_06.prefab:454b79078b8534447951ad5ebf4791a0");

	private static readonly AssetReference VO_EmpressShekzara_Female_Mantid_LETL_Death_01 = new AssetReference("VO_EmpressShekzara_Female_Mantid_LETL_Death_01.prefab:5f020428a1b886644abe43e4ca4c72e5");

	private static readonly AssetReference VO_EmpressShekzara_Female_Mantid_LETL_Idle_01 = new AssetReference("VO_EmpressShekzara_Female_Mantid_LETL_Idle_01.prefab:a2bb4606aa51152418320e6d621c03cf");

	private static readonly AssetReference VO_EmpressShekzara_Female_Mantid_LETL_Intro_01 = new AssetReference("VO_EmpressShekzara_Female_Mantid_LETL_Intro_01.prefab:8525dd76fb8f24f4986661863be34583");

	private List<string> m_IdleLines = new List<string> { VO_EmpressShekzara_Female_Mantid_LETL_Idle_01 };

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> soundFiles = new List<string> { VO_EmpressShekzara_Female_Mantid_LETL_Intro_01, VO_EmpressShekzara_Female_Mantid_LETL_Attack_02, VO_EmpressShekzara_Female_Mantid_LETL_Attack_06, VO_EmpressShekzara_Female_Mantid_LETL_Idle_01, VO_EmpressShekzara_Female_Mantid_LETL_Death_01 };
		SetBossVOLines(soundFiles);
		foreach (string soundFile in soundFiles)
		{
			GameState.Get().GetGameEntity().PreloadSound(soundFile);
		}
	}

	public override void OnCreateGame()
	{
		base.OnCreateGame();
		m_introLine = VO_EmpressShekzara_Female_Mantid_LETL_Intro_01;
		m_deathLine = VO_EmpressShekzara_Female_Mantid_LETL_Death_01;
	}

	public override IEnumerator RespondToWillPlayCardWithTiming(string cardID, Entity playedEntity)
	{
		while (m_enemySpeaking)
		{
			yield return null;
		}
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LT23_815H");
		FindEnemyActorInPlayByDesignCode("LT23_815H3");
		string designCode = playedEntity.GetLettuceAbilityOwner().GetCardId();
		if (designCode == "LT23_815H")
		{
			if (cardID == "LT23_815P1")
			{
				GameState.Get().SetBusy(busy: true);
				yield return MissionPlayVOOnce(bossActor, VO_EmpressShekzara_Female_Mantid_LETL_Attack_02);
				GameState.Get().SetBusy(busy: false);
			}
		}
		else if (designCode == "LT23_815H3" && cardID == "LT23_815P2")
		{
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVOOnce(bossActor, VO_EmpressShekzara_Female_Mantid_LETL_Attack_06);
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LT23_815H");
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LT23_815H");
		if (entity.GetCardId() == "LT23_815H")
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LT23_815H");
		if (turn == 1)
		{
			yield return MissionPlayVOOnce(bossActor, m_introLine);
		}
	}
}
