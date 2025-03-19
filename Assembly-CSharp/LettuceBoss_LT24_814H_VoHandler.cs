using System.Collections;
using System.Collections.Generic;

public class LettuceBoss_LT24_814H_VoHandler : VoPlaybackHandler
{
	private static readonly AssetReference VO_TheCrone_Female_Troll_LETL_Attack_01 = new AssetReference("VO_TheCrone_Female_Troll_LETL_Attack_01.prefab:cf12f060b6b570a44aca02ca5fc42695");

	private static readonly AssetReference VO_TheCrone_Female_Troll_LETL_Attack_02 = new AssetReference("VO_TheCrone_Female_Troll_LETL_Attack_02.prefab:88c2d1b42348b164a8d3ff2ec6624d9c");

	private static readonly AssetReference VO_TheCrone_Female_Troll_LETL_Death_01 = new AssetReference("VO_TheCrone_Female_Troll_LETL_Death_01.prefab:5e016d46563961f43a21fa397fa9b168");

	private static readonly AssetReference VO_TheCrone_Female_Troll_LETL_Idle_01 = new AssetReference("VO_TheCrone_Female_Troll_LETL_Idle_01.prefab:9119cca1c2d92e04ab89b9f4356aecce");

	private static readonly AssetReference VO_TheCrone_Female_Troll_LETL_Idle_02 = new AssetReference("VO_TheCrone_Female_Troll_LETL_Idle_02.prefab:1d38b49b0c1fb3645af8455b9f213606");

	private static readonly AssetReference VO_TheCrone_Female_Troll_LETL_Intro_01 = new AssetReference("VO_TheCrone_Female_Troll_LETL_Intro_01.prefab:a4359fe0647297a43a7857e7524e90d9");

	private List<string> m_IdleLines = new List<string> { VO_TheCrone_Female_Troll_LETL_Idle_01, VO_TheCrone_Female_Troll_LETL_Idle_02 };

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> soundFiles = new List<string> { VO_TheCrone_Female_Troll_LETL_Intro_01, VO_TheCrone_Female_Troll_LETL_Idle_01, VO_TheCrone_Female_Troll_LETL_Idle_02, VO_TheCrone_Female_Troll_LETL_Attack_01, VO_TheCrone_Female_Troll_LETL_Attack_02, VO_TheCrone_Female_Troll_LETL_Death_01 };
		SetBossVOLines(soundFiles);
		foreach (string soundFile in soundFiles)
		{
			GameState.Get().GetGameEntity().PreloadSound(soundFile);
		}
	}

	public override void OnCreateGame()
	{
		base.OnCreateGame();
		m_introLine = VO_TheCrone_Female_Troll_LETL_Intro_01;
		m_deathLine = VO_TheCrone_Female_Troll_LETL_Death_01;
	}

	public override IEnumerator RespondToWillPlayCardWithTiming(string cardID, Entity playedEntity)
	{
		while (m_enemySpeaking)
		{
			yield return null;
		}
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LT24_814H");
		if (!(playedEntity.GetLettuceAbilityOwner().GetCardId() == "LT24_814H"))
		{
			yield break;
		}
		if (!(cardID == "LT24_814P1"))
		{
			if (cardID == "LT24_814P2")
			{
				GameState.Get().SetBusy(busy: true);
				yield return MissionPlayVOOnce(bossActor, VO_TheCrone_Female_Troll_LETL_Attack_01);
				GameState.Get().SetBusy(busy: false);
			}
		}
		else
		{
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVOOnce(bossActor, VO_TheCrone_Female_Troll_LETL_Attack_02);
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LT24_814H");
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LT24_814H");
		if (entity.GetCardId() == "LT24_814H")
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LT24_814H");
		if (turn == 1)
		{
			yield return MissionPlayVOOnce(bossActor, m_introLine);
		}
	}
}
