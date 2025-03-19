using System.Collections;
using System.Collections.Generic;

public class LettuceBoss_LETL_862H_VoHandler : VoPlaybackHandler
{
	private static readonly AssetReference VO_DragonboneGolem_Male_Construct_Attack_01 = new AssetReference("VO_DragonboneGolem_Male_Construct_Attack_01.prefab:cc7478f1188143f43afa3303a1b009bd");

	private static readonly AssetReference VO_DragonboneGolem_Male_Construct_Attack_02 = new AssetReference("VO_DragonboneGolem_Male_Construct_Attack_02.prefab:77f9b82372e11624da090239e1817b66");

	private static readonly AssetReference VO_DragonboneGolem_Male_Construct_Death_01 = new AssetReference("VO_DragonboneGolem_Male_Construct_Death_01.prefab:25524cca49884194883d9f2c38b3c53e");

	private static readonly AssetReference VO_DragonboneGolem_Male_Construct_Idle_01 = new AssetReference("VO_DragonboneGolem_Male_Construct_Idle_01.prefab:beec59dea175a5d4289bb7d33555bed7");

	private static readonly AssetReference VO_DragonboneGolem_Male_Construct_Intro_01 = new AssetReference("VO_DragonboneGolem_Male_Construct_Intro_01.prefab:ff5546e5387f6c7408bfee80d33f6f88");

	private List<string> m_IdleLines = new List<string> { VO_DragonboneGolem_Male_Construct_Idle_01 };

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> soundFiles = new List<string> { VO_DragonboneGolem_Male_Construct_Attack_01, VO_DragonboneGolem_Male_Construct_Attack_02, VO_DragonboneGolem_Male_Construct_Death_01, VO_DragonboneGolem_Male_Construct_Idle_01, VO_DragonboneGolem_Male_Construct_Intro_01 };
		SetBossVOLines(soundFiles);
		foreach (string soundFile in soundFiles)
		{
			GameState.Get().GetGameEntity().PreloadSound(soundFile);
		}
	}

	public override void OnCreateGame()
	{
		base.OnCreateGame();
		m_introLine = VO_DragonboneGolem_Male_Construct_Intro_01;
		m_deathLine = VO_DragonboneGolem_Male_Construct_Death_01;
	}

	public override IEnumerator RespondToWillPlayCardWithTiming(string cardID, Entity playedEntity)
	{
		while (m_enemySpeaking)
		{
			yield return null;
		}
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_862H");
		if (playedEntity.GetLettuceAbilityOwner().GetCardId() == "LETL_862H" && cardID == "LETL_862P1")
		{
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(bossActor, VO_DragonboneGolem_Male_Construct_Attack_02);
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_862H");
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_862H");
		if (entity.GetCardId() == "LETL_862H")
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_862H");
		if (turn == 1)
		{
			yield return MissionPlayVOOnce(bossActor, m_introLine);
		}
	}
}
