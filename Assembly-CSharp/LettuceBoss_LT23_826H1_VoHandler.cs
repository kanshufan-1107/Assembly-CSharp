using System.Collections;
using System.Collections.Generic;

public class LettuceBoss_LT23_826H1_VoHandler : VoPlaybackHandler
{
	private static readonly AssetReference VO_QueenAzshara_Female_Naga_LETL_Death_02 = new AssetReference("VO_QueenAzshara_Female_Naga_LETL_Death_02.prefab:84763ca6799ad3d4fbc0c51918e35dc6");

	private static readonly AssetReference VO_QueenAzshara_Female_Naga_LETL_Special_01 = new AssetReference("VO_QueenAzshara_Female_Naga_LETL_Special_01.prefab:145a720717c30eb49aea146f6a2b4bd5");

	private static readonly AssetReference VO_QueenAzshara_Female_Naga_LETL_Idle_01 = new AssetReference("VO_QueenAzshara_Female_Naga_LETL_Idle_01.prefab:35fc3059e56ea7f43853c636b00cc888");

	private static readonly AssetReference VO_QueenAzshara_Female_Naga_LETL_Attack_01 = new AssetReference("VO_QueenAzshara_Female_Naga_LETL_Attack_01.prefab:f978e9b5b96321441b9575b492aef691");

	private static readonly AssetReference VO_QueenAzshara_Female_Naga_LETL_Attack_02 = new AssetReference("VO_QueenAzshara_Female_Naga_LETL_Attack_02.prefab:406561acf76d05741a1e3eb9366114e2");

	private List<string> m_IdleLines = new List<string> { VO_QueenAzshara_Female_Naga_LETL_Idle_01 };

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> soundFiles = new List<string> { VO_QueenAzshara_Female_Naga_LETL_Special_01, VO_QueenAzshara_Female_Naga_LETL_Death_02, VO_QueenAzshara_Female_Naga_LETL_Idle_01, VO_QueenAzshara_Female_Naga_LETL_Attack_01, VO_QueenAzshara_Female_Naga_LETL_Attack_02 };
		SetBossVOLines(soundFiles);
		foreach (string soundFile in soundFiles)
		{
			GameState.Get().GetGameEntity().PreloadSound(soundFile);
		}
	}

	public override void OnCreateGame()
	{
		base.OnCreateGame();
		m_introLine = VO_QueenAzshara_Female_Naga_LETL_Special_01;
		m_deathLine = VO_QueenAzshara_Female_Naga_LETL_Death_02;
	}

	public override IEnumerator RespondToWillPlayCardWithTiming(string cardID, Entity playedEntity)
	{
		while (m_enemySpeaking)
		{
			yield return null;
		}
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LT23_826H1");
		if (!(playedEntity.GetLettuceAbilityOwner().GetCardId() == "LT23_826H1"))
		{
			yield break;
		}
		if (!(cardID == "LT23_826P3"))
		{
			if (cardID == "LT23_826P1")
			{
				GameState.Get().SetBusy(busy: true);
				yield return MissionPlayVOOnce(bossActor, VO_QueenAzshara_Female_Naga_LETL_Attack_02);
				GameState.Get().SetBusy(busy: false);
			}
		}
		else
		{
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVOOnce(bossActor, VO_QueenAzshara_Female_Naga_LETL_Attack_01);
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LT23_826H1");
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LT23_826H1");
		if (entity.GetCardId() == "LT23_826H1")
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LT23_826H1");
		if (turn == 1)
		{
			yield return MissionPlayVOOnce(bossActor, m_introLine);
		}
	}
}
