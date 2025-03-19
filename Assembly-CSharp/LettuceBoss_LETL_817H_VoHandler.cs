using System.Collections;
using System.Collections.Generic;

public class LettuceBoss_LETL_817H_VoHandler : VoPlaybackHandler
{
	private static readonly AssetReference VO_Story_Hero_Serena_Female_Harpy_Story_Xyrella_Mission2Death_01 = new AssetReference("VO_Story_Hero_Serena_Female_Harpy_Story_Xyrella_Mission2Death_01.prefab:29971ca128573ea488eb4479810e57d9");

	private static readonly AssetReference VO_Story_Hero_Serena_Female_Harpy_Story_Xyrella_Mission2EmoteResponse_01 = new AssetReference("VO_Story_Hero_Serena_Female_Harpy_Story_Xyrella_Mission2EmoteResponse_01.prefab:a3b2b5e278c44794f9833088ecc84cda");

	private static readonly AssetReference VO_Story_Hero_Serena_Female_Harpy_Story_Xyrella_Mission2HeroPower_01 = new AssetReference("VO_Story_Hero_Serena_Female_Harpy_Story_Xyrella_Mission2HeroPower_01.prefab:d441233485d1c704a93bfb861a2323ec");

	private static readonly AssetReference VO_Story_Hero_Serena_Female_Harpy_Story_Xyrella_Mission2HeroPower_02 = new AssetReference("VO_Story_Hero_Serena_Female_Harpy_Story_Xyrella_Mission2HeroPower_02.prefab:88bcffef7c36ed04196478b1dda606f1");

	private static readonly AssetReference VO_Story_Hero_Serena_Female_Harpy_Story_Xyrella_Mission2HeroPower_03 = new AssetReference("VO_Story_Hero_Serena_Female_Harpy_Story_Xyrella_Mission2HeroPower_03.prefab:3b6f517ee3e5f00469259ab1f87a2c8e");

	private static readonly AssetReference VO_Story_Hero_Serena_Female_Harpy_Story_Xyrella_Mission2HeroPower_04 = new AssetReference("VO_Story_Hero_Serena_Female_Harpy_Story_Xyrella_Mission2HeroPower_04.prefab:9c1afe58d526d8940ad5d59660fbb504");

	private static readonly AssetReference VO_Story_Hero_Serena_Female_Harpy_Story_Xyrella_Mission2HeroPower_05 = new AssetReference("VO_Story_Hero_Serena_Female_Harpy_Story_Xyrella_Mission2HeroPower_05.prefab:abe095c594ea39e4a82e48844729f695");

	private List<string> m_IdleLines = new List<string> { VO_Story_Hero_Serena_Female_Harpy_Story_Xyrella_Mission2HeroPower_05 };

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> soundFiles = new List<string> { VO_Story_Hero_Serena_Female_Harpy_Story_Xyrella_Mission2Death_01, VO_Story_Hero_Serena_Female_Harpy_Story_Xyrella_Mission2EmoteResponse_01, VO_Story_Hero_Serena_Female_Harpy_Story_Xyrella_Mission2HeroPower_02, VO_Story_Hero_Serena_Female_Harpy_Story_Xyrella_Mission2HeroPower_03, VO_Story_Hero_Serena_Female_Harpy_Story_Xyrella_Mission2HeroPower_05 };
		SetBossVOLines(soundFiles);
		foreach (string soundFile in soundFiles)
		{
			GameState.Get().GetGameEntity().PreloadSound(soundFile);
		}
	}

	public override void OnCreateGame()
	{
		base.OnCreateGame();
		m_introLine = VO_Story_Hero_Serena_Female_Harpy_Story_Xyrella_Mission2EmoteResponse_01;
		m_deathLine = VO_Story_Hero_Serena_Female_Harpy_Story_Xyrella_Mission2Death_01;
	}

	public override IEnumerator RespondToWillPlayCardWithTiming(string cardID, Entity playedEntity)
	{
		while (m_enemySpeaking)
		{
			yield return null;
		}
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_817H");
		if (!(playedEntity.GetLettuceAbilityOwner().GetCardId() == "LETL_817H"))
		{
			yield break;
		}
		if (!(cardID == "LETL_817P3_01"))
		{
			if (cardID == "LETL_817P2_01")
			{
				GameState.Get().SetBusy(busy: true);
				yield return MissionPlayVOOnce(bossActor, VO_Story_Hero_Serena_Female_Harpy_Story_Xyrella_Mission2HeroPower_03);
				GameState.Get().SetBusy(busy: false);
			}
		}
		else
		{
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVOOnce(bossActor, VO_Story_Hero_Serena_Female_Harpy_Story_Xyrella_Mission2HeroPower_02);
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_817H");
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_817H");
		if (entity.GetCardId() == "LETL_817H")
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_817H");
		if (turn == 1)
		{
			yield return MissionPlayVOOnce(bossActor, m_introLine);
		}
	}
}
