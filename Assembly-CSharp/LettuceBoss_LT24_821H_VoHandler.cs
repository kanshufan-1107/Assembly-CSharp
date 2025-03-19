using System.Collections;
using System.Collections.Generic;

public class LettuceBoss_LT24_821H_VoHandler : VoPlaybackHandler
{
	private static readonly AssetReference VO_PrinceMalchezaar_Male_Demon_LETL_Attack_01 = new AssetReference("VO_PrinceMalchezaar_Male_Demon_LETL_Attack_01.prefab:52f5238e917074545b4f7a08023d278b");

	private static readonly AssetReference VO_PrinceMalchezaar_Male_Demon_LETL_Attack_02 = new AssetReference("VO_PrinceMalchezaar_Male_Demon_LETL_Attack_02.prefab:c0b35b268187bec49abcb1f4c44f1cc6");

	private static readonly AssetReference VO_PrinceMalchezaar_Male_Demon_LETL_Attack_03 = new AssetReference("VO_PrinceMalchezaar_Male_Demon_LETL_Attack_03.prefab:2d5770ac675f51549992ace3a08af4e7");

	private static readonly AssetReference VO_PrinceMalchezaar_Male_Demon_LETL_Death_02 = new AssetReference("VO_PrinceMalchezaar_Male_Demon_LETL_Death_02.prefab:1f23ef572082db84083cbcd07b15f846");

	private static readonly AssetReference VO_PrinceMalchezaar_Male_Demon_LETL_Idle_01 = new AssetReference("VO_PrinceMalchezaar_Male_Demon_LETL_Idle_01.prefab:68e00c20497ff5848a4a42e356ac0459");

	private static readonly AssetReference VO_PrinceMalchezaar_Male_Demon_LETL_Idle_02 = new AssetReference("VO_PrinceMalchezaar_Male_Demon_LETL_Idle_02.prefab:691de03bd17b1b04aad7e803b5f8e428");

	private static readonly AssetReference VO_PrinceMalchezaar_Male_Demon_LETL_Idle_03 = new AssetReference("VO_PrinceMalchezaar_Male_Demon_LETL_Idle_03.prefab:f916720a82e770341b834b9d0038273d");

	private static readonly AssetReference VO_PrinceMalchezaar_Male_Demon_LETL_Intro_05 = new AssetReference("VO_PrinceMalchezaar_Male_Demon_LETL_Intro_05.prefab:902d90a7004e6784dada2800213497f3");

	private List<string> m_IdleLines = new List<string> { VO_PrinceMalchezaar_Male_Demon_LETL_Idle_01, VO_PrinceMalchezaar_Male_Demon_LETL_Idle_02, VO_PrinceMalchezaar_Male_Demon_LETL_Idle_03 };

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> soundFiles = new List<string> { VO_PrinceMalchezaar_Male_Demon_LETL_Intro_05, VO_PrinceMalchezaar_Male_Demon_LETL_Attack_01, VO_PrinceMalchezaar_Male_Demon_LETL_Attack_02, VO_PrinceMalchezaar_Male_Demon_LETL_Attack_03, VO_PrinceMalchezaar_Male_Demon_LETL_Idle_01, VO_PrinceMalchezaar_Male_Demon_LETL_Idle_02, VO_PrinceMalchezaar_Male_Demon_LETL_Idle_03, VO_PrinceMalchezaar_Male_Demon_LETL_Death_02 };
		SetBossVOLines(soundFiles);
		foreach (string soundFile in soundFiles)
		{
			GameState.Get().GetGameEntity().PreloadSound(soundFile);
		}
	}

	public override void OnCreateGame()
	{
		base.OnCreateGame();
		m_introLine = VO_PrinceMalchezaar_Male_Demon_LETL_Intro_05;
		m_deathLine = VO_PrinceMalchezaar_Male_Demon_LETL_Death_02;
	}

	public override IEnumerator RespondToWillPlayCardWithTiming(string cardID, Entity playedEntity)
	{
		while (m_enemySpeaking)
		{
			yield return null;
		}
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LT24_821H");
		if (playedEntity.GetLettuceAbilityOwner().GetCardId() == "LT24_821H")
		{
			switch (cardID)
			{
			case "LETL_006P1_03":
				GameState.Get().SetBusy(busy: true);
				yield return MissionPlayVOOnce(bossActor, VO_PrinceMalchezaar_Male_Demon_LETL_Attack_02);
				GameState.Get().SetBusy(busy: false);
				break;
			case "LETL_006P1_05":
				GameState.Get().SetBusy(busy: true);
				yield return MissionPlayVOOnce(bossActor, VO_PrinceMalchezaar_Male_Demon_LETL_Attack_02);
				GameState.Get().SetBusy(busy: false);
				break;
			case "LETL_006P9_03":
				GameState.Get().SetBusy(busy: true);
				yield return MissionPlayVOOnce(bossActor, VO_PrinceMalchezaar_Male_Demon_LETL_Attack_03);
				GameState.Get().SetBusy(busy: false);
				break;
			case "LETL_006P9_05":
				GameState.Get().SetBusy(busy: true);
				yield return MissionPlayVOOnce(bossActor, VO_PrinceMalchezaar_Male_Demon_LETL_Attack_03);
				GameState.Get().SetBusy(busy: false);
				break;
			case "LETL_006P8_03":
				GameState.Get().SetBusy(busy: true);
				yield return MissionPlayVOOnce(bossActor, VO_PrinceMalchezaar_Male_Demon_LETL_Attack_01);
				GameState.Get().SetBusy(busy: false);
				break;
			case "LETL_006P8_05":
				GameState.Get().SetBusy(busy: true);
				yield return MissionPlayVOOnce(bossActor, VO_PrinceMalchezaar_Male_Demon_LETL_Attack_01);
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LT24_821H");
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LT24_821H");
		if (entity.GetCardId() == "LT24_821H")
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LT24_821H");
		if (turn == 1)
		{
			yield return MissionPlayVOOnce(bossActor, m_introLine);
		}
	}
}
