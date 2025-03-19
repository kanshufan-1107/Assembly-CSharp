using System.Collections;
using System.Collections.Generic;

public class LettuceBoss_LETL_820H_VoHandler : VoPlaybackHandler
{
	private static readonly AssetReference VO_Story_Hero_Barak_Male_Centaur_Story_Guff_Mission3Death_01 = new AssetReference("VO_Story_Hero_Barak_Male_Centaur_Story_Guff_Mission3Death_01.prefab:cee9af5f93604f543ae52d62c22b7a25");

	private static readonly AssetReference VO_Story_Hero_Barak_Male_Centaur_Story_Guff_Mission3ExchangeA_01 = new AssetReference("VO_Story_Hero_Barak_Male_Centaur_Story_Guff_Mission3ExchangeA_01.prefab:6f38b4201afb1564997c88ea4f83ce4a");

	private static readonly AssetReference VO_Story_Hero_Barak_Male_Centaur_Story_Guff_Mission3HeroPower_01 = new AssetReference("VO_Story_Hero_Barak_Male_Centaur_Story_Guff_Mission3HeroPower_01.prefab:d6db2227c9dcb7345a05de10aea7b262");

	private static readonly AssetReference VO_Story_Hero_Barak_Male_Centaur_Story_Guff_Mission3HeroPower_02 = new AssetReference("VO_Story_Hero_Barak_Male_Centaur_Story_Guff_Mission3HeroPower_02.prefab:568e12955b775ba48b69f4dee0bd1c53");

	private static readonly AssetReference VO_Story_Hero_Barak_Male_Centaur_Story_Guff_Mission3HeroPower_03 = new AssetReference("VO_Story_Hero_Barak_Male_Centaur_Story_Guff_Mission3HeroPower_03.prefab:708e426e1a5f7bc48b90d0dd6ba20a79");

	private static readonly AssetReference VO_Story_Hero_Barak_Male_Centaur_Story_Guff_Mission3Idle_01 = new AssetReference("VO_Story_Hero_Barak_Male_Centaur_Story_Guff_Mission3Idle_01.prefab:2966bd6c1b4df3e4da8af264c7fa9b3c");

	private static readonly AssetReference VO_Story_Hero_Barak_Male_Centaur_Story_Guff_Mission3Idle_02 = new AssetReference("VO_Story_Hero_Barak_Male_Centaur_Story_Guff_Mission3Idle_02.prefab:417d4a0429e188d43b084fcf6337def6");

	private static readonly AssetReference VO_Story_Hero_Barak_Male_Centaur_Story_Guff_Mission3Idle_03 = new AssetReference("VO_Story_Hero_Barak_Male_Centaur_Story_Guff_Mission3Idle_03.prefab:107f51e08e2990a4aacbcda196c57ed3");

	private List<string> m_IdleLines = new List<string> { VO_Story_Hero_Barak_Male_Centaur_Story_Guff_Mission3Idle_01, VO_Story_Hero_Barak_Male_Centaur_Story_Guff_Mission3Idle_02, VO_Story_Hero_Barak_Male_Centaur_Story_Guff_Mission3Idle_03 };

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> soundFiles = new List<string> { VO_Story_Hero_Barak_Male_Centaur_Story_Guff_Mission3Death_01, VO_Story_Hero_Barak_Male_Centaur_Story_Guff_Mission3ExchangeA_01, VO_Story_Hero_Barak_Male_Centaur_Story_Guff_Mission3HeroPower_01, VO_Story_Hero_Barak_Male_Centaur_Story_Guff_Mission3HeroPower_02, VO_Story_Hero_Barak_Male_Centaur_Story_Guff_Mission3HeroPower_03, VO_Story_Hero_Barak_Male_Centaur_Story_Guff_Mission3Idle_01, VO_Story_Hero_Barak_Male_Centaur_Story_Guff_Mission3Idle_02, VO_Story_Hero_Barak_Male_Centaur_Story_Guff_Mission3Idle_03 };
		SetBossVOLines(soundFiles);
		foreach (string soundFile in soundFiles)
		{
			GameState.Get().GetGameEntity().PreloadSound(soundFile);
		}
	}

	public override void OnCreateGame()
	{
		base.OnCreateGame();
		m_introLine = VO_Story_Hero_Barak_Male_Centaur_Story_Guff_Mission3ExchangeA_01;
		m_deathLine = VO_Story_Hero_Barak_Male_Centaur_Story_Guff_Mission3Death_01;
	}

	public override IEnumerator RespondToWillPlayCardWithTiming(string cardID, Entity playedEntity)
	{
		while (m_enemySpeaking)
		{
			yield return null;
		}
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_820H");
		if (!(playedEntity.GetLettuceAbilityOwner().GetCardId() == "LETL_820H"))
		{
			yield break;
		}
		if (!(cardID == "LETL_820P1_01"))
		{
			if (cardID == "LETL_820P2_01")
			{
				GameState.Get().SetBusy(busy: true);
				yield return MissionPlayVOOnce(bossActor, VO_Story_Hero_Barak_Male_Centaur_Story_Guff_Mission3HeroPower_02);
				GameState.Get().SetBusy(busy: false);
			}
		}
		else
		{
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVOOnce(bossActor, VO_Story_Hero_Barak_Male_Centaur_Story_Guff_Mission3HeroPower_01);
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_820H");
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_820H");
		if (entity.GetCardId() == "LETL_820H")
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_820H");
		if (turn == 1)
		{
			yield return MissionPlayVOOnce(bossActor, m_introLine);
		}
	}
}
