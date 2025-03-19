using System.Collections;
using System.Collections.Generic;

public class LettuceBoss_LT23_803H_VoHandler : VoPlaybackHandler
{
	private static readonly AssetReference VO_CaptainShivers_Male_Human_Attack_01 = new AssetReference("VO_CaptainShivers_Male_Human_Attack_01.prefab:3c25aeb1338cf7444a3abbf13dcd0c46");

	private static readonly AssetReference VO_CaptainShivers_Male_Human_Death_01 = new AssetReference("VO_CaptainShivers_Male_Human_Death_01.prefab:bd6541155bd0a9c4284825ad2db91b5a");

	private static readonly AssetReference VO_CaptainShivers_Male_Human_Idle_01 = new AssetReference("VO_CaptainShivers_Male_Human_Idle_01.prefab:8fc3b272745000043a3ebb2896b58c7f");

	private static readonly AssetReference VO_CaptainShivers_Male_Human_Intro_01 = new AssetReference("VO_CaptainShivers_Male_Human_Intro_01.prefab:b979d11e222915844ba95b4690328e58");

	private List<string> m_IdleLines = new List<string> { VO_CaptainShivers_Male_Human_Idle_01 };

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> soundFiles = new List<string> { VO_CaptainShivers_Male_Human_Attack_01, VO_CaptainShivers_Male_Human_Death_01, VO_CaptainShivers_Male_Human_Idle_01, VO_CaptainShivers_Male_Human_Intro_01 };
		SetBossVOLines(soundFiles);
		foreach (string soundFile in soundFiles)
		{
			GameState.Get().GetGameEntity().PreloadSound(soundFile);
		}
	}

	public override void OnCreateGame()
	{
		base.OnCreateGame();
		m_introLine = VO_CaptainShivers_Male_Human_Intro_01;
		m_deathLine = VO_CaptainShivers_Male_Human_Death_01;
	}

	public override IEnumerator RespondToWillPlayCardWithTiming(string cardID, Entity playedEntity)
	{
		while (m_enemySpeaking)
		{
			yield return null;
		}
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LT23_803H");
		if (playedEntity.GetLettuceAbilityOwner().GetCardId() == "LT23_803H" && cardID == "LT23_803P1")
		{
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVOOnce(bossActor, VO_CaptainShivers_Male_Human_Attack_01);
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LT23_803H");
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LT23_803H");
		if (entity.GetCardId() == "LT23_803H")
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LT23_803H");
		if (turn == 1)
		{
			yield return MissionPlayVOOnce(bossActor, m_introLine);
		}
	}
}
