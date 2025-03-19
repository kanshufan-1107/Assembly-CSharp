using System.Collections;
using System.Collections.Generic;

public class LettuceBoss_LETL_860H_VoHandler : VoPlaybackHandler
{
	private static readonly AssetReference VO_Kazakus_Male_Troll_Bark_12 = new AssetReference("VO_Kazakus_Male_Troll_Bark_12.prefab:0ada67e6e7d73344899cea2a354b7631");

	private static readonly AssetReference VO_BOM_08_007_Male_Troll_Kazakus_InGame_Introduction_01_B = new AssetReference("VO_BOM_08_007_Male_Troll_Kazakus_InGame_Introduction_01_B.prefab:151dd32e1c9ba4d48922a963336322eb");

	private static readonly AssetReference VO_BOM_08_008_Male_Dragon_Kazakusan_InGame_Turn_07_01_A = new AssetReference("VO_BOM_08_008_Male_Dragon_Kazakusan_InGame_Turn_07_01_A.prefab:bdc62bdd39e32954dad4ecd20846fcd6");

	private static readonly AssetReference VO_Kazakus_Male_Troll_Bark_19 = new AssetReference("VO_Kazakus_Male_Troll_Bark_19.prefab:15fa5594bea41544f84f0d5a5b6727ca");

	private static readonly AssetReference VO_Kazakus_Male_Troll_Bark_20 = new AssetReference("VO_Kazakus_Male_Troll_Bark_20.prefab:2a126bc2e07b4c141887a955daccfbad");

	private List<string> m_IdleLines = new List<string> { VO_Kazakus_Male_Troll_Bark_12 };

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> soundFiles = new List<string> { VO_Kazakus_Male_Troll_Bark_12, VO_BOM_08_007_Male_Troll_Kazakus_InGame_Introduction_01_B, VO_BOM_08_008_Male_Dragon_Kazakusan_InGame_Turn_07_01_A, VO_Kazakus_Male_Troll_Bark_19, VO_Kazakus_Male_Troll_Bark_20 };
		SetBossVOLines(soundFiles);
		foreach (string soundFile in soundFiles)
		{
			GameState.Get().GetGameEntity().PreloadSound(soundFile);
		}
	}

	public override void OnCreateGame()
	{
		base.OnCreateGame();
		m_introLine = VO_BOM_08_007_Male_Troll_Kazakus_InGame_Introduction_01_B;
		m_deathLine = VO_Kazakus_Male_Troll_Bark_19;
	}

	public override IEnumerator RespondToWillPlayCardWithTiming(string cardID, Entity playedEntity)
	{
		while (m_enemySpeaking)
		{
			yield return null;
		}
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_860H");
		Actor bossActor2 = FindEnemyActorInPlayByDesignCode("LETL_860H4");
		string designCode = playedEntity.GetLettuceAbilityOwner().GetCardId();
		if (designCode == "LETL_860H")
		{
			if (cardID == "LETL_860P2" || cardID == "LETL_860P2_05")
			{
				GameState.Get().SetBusy(busy: true);
				yield return MissionPlayVO(bossActor, VO_Kazakus_Male_Troll_Bark_20);
				GameState.Get().SetBusy(busy: false);
			}
		}
		else if (designCode == "LETL_860H4" && cardID == "LETL_860P1")
		{
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(bossActor2, VO_BOM_08_008_Male_Dragon_Kazakusan_InGame_Turn_07_01_A);
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_860H");
		Actor bossActor2 = FindEnemyActorInPlayByDesignCode("LETL_860H4");
		switch (missionEvent)
		{
		case 517:
			yield return MissionPlayVO(bossActor, m_IdleLines);
			yield return MissionPlayVO(bossActor2, m_IdleLines);
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_860H");
		if (entity.GetCardId() == "LETL_860H")
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_860H");
		if (turn == 1)
		{
			yield return MissionPlayVOOnce(bossActor, m_introLine);
		}
	}
}
