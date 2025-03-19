using System.Collections;
using System.Collections.Generic;

public class LettuceBoss_LETL_850H_VoHandler : VoPlaybackHandler
{
	private static readonly AssetReference VO_LouisPhilips_Male_Undead_Attack_01 = new AssetReference("VO_LouisPhilips_Male_Undead_Attack_01.prefab:5da447897a591114c87b203093e3bd9a");

	private static readonly AssetReference VO_LouisPhilips_Male_Undead_Attack_02 = new AssetReference("VO_LouisPhilips_Male_Undead_Attack_02.prefab:89345aa697638e543baa652d53b65f44");

	private static readonly AssetReference VO_LouisPhilips_Male_Undead_Death_01 = new AssetReference("VO_LouisPhilips_Male_Undead_Death_01.prefab:481e90b300c4a9f4bb3c4ea4096f4750");

	private static readonly AssetReference VO_LouisPhilips_Male_Undead_Idle_01 = new AssetReference("VO_LouisPhilips_Male_Undead_Idle_01.prefab:a306338235bf99d4db3b6c9b4a520de7");

	private static readonly AssetReference VO_LouisPhilips_Male_Undead_Intro_01 = new AssetReference("VO_LouisPhilips_Male_Undead_Intro_01.prefab:5cf772bd7263c914e994904af5d6bbf6");

	private List<string> m_IdleLines = new List<string> { VO_LouisPhilips_Male_Undead_Idle_01 };

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> soundFiles = new List<string> { VO_LouisPhilips_Male_Undead_Attack_01, VO_LouisPhilips_Male_Undead_Attack_02, VO_LouisPhilips_Male_Undead_Death_01, VO_LouisPhilips_Male_Undead_Idle_01, VO_LouisPhilips_Male_Undead_Intro_01 };
		SetBossVOLines(soundFiles);
		foreach (string soundFile in soundFiles)
		{
			GameState.Get().GetGameEntity().PreloadSound(soundFile);
		}
	}

	public override void OnCreateGame()
	{
		base.OnCreateGame();
		m_introLine = VO_LouisPhilips_Male_Undead_Intro_01;
		m_deathLine = VO_LouisPhilips_Male_Undead_Death_01;
	}

	public override IEnumerator RespondToWillPlayCardWithTiming(string cardID, Entity playedEntity)
	{
		while (m_enemySpeaking)
		{
			yield return null;
		}
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_850H");
		if (playedEntity.GetLettuceAbilityOwner().GetCardId() == "LETL_850H")
		{
			switch (cardID)
			{
			case "LETL_847P2_01":
				GameState.Get().SetBusy(busy: true);
				yield return MissionPlayVO(bossActor, VO_LouisPhilips_Male_Undead_Attack_01);
				GameState.Get().SetBusy(busy: false);
				break;
			case "LETL_850P1_01":
			case "LETL_850P1_02":
				GameState.Get().SetBusy(busy: true);
				yield return MissionPlayVO(bossActor, VO_LouisPhilips_Male_Undead_Attack_02);
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_850H");
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_850H");
		if (entity.GetCardId() == "LETL_850H")
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_850H");
		if (turn == 1)
		{
			yield return MissionPlayVOOnce(bossActor, m_introLine);
		}
	}
}
