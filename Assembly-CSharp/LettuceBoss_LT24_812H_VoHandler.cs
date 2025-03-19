using System.Collections;
using System.Collections.Generic;

public class LettuceBoss_LT24_812H_VoHandler : VoPlaybackHandler
{
	private static readonly AssetReference VO_BabblingBook_Male_Book_LETL_Attack_01 = new AssetReference("VO_BabblingBook_Male_Book_LETL_Attack_01.prefab:4aa04bc94175ccf4f944b17223dbf371");

	private static readonly AssetReference VO_BabblingBook_Male_Book_LETL_Attack_02 = new AssetReference("VO_BabblingBook_Male_Book_LETL_Attack_02.prefab:475300d85a200414d8dda4b424fa3094");

	private static readonly AssetReference VO_BabblingBook_Male_Book_LETL_Death_01 = new AssetReference("VO_BabblingBook_Male_Book_LETL_Death_01.prefab:e9e557f30bc74384c818a379934e199a");

	private static readonly AssetReference VO_BabblingBook_Male_Book_LETL_Idle_01 = new AssetReference("VO_BabblingBook_Male_Book_LETL_Idle_01.prefab:ad4f2cbb8d12ac04d8ae6f0a905367da");

	private static readonly AssetReference VO_BabblingBook_Male_Book_LETL_Idle_02 = new AssetReference("VO_BabblingBook_Male_Book_LETL_Idle_02.prefab:d94b919d5199a0742afbb930f7c99432");

	private static readonly AssetReference VO_BabblingBook_Male_Book_LETL_Idle_03 = new AssetReference("VO_BabblingBook_Male_Book_LETL_Idle_03.prefab:4ac360177f629584eb633cbb323e5cb8");

	private static readonly AssetReference VO_BabblingBook_Male_Book_LETL_Intro_01 = new AssetReference("VO_BabblingBook_Male_Book_LETL_Intro_01.prefab:8c426406fefbb5a4ca332a09af199536");

	private List<string> m_IdleLines = new List<string> { VO_BabblingBook_Male_Book_LETL_Idle_01, VO_BabblingBook_Male_Book_LETL_Idle_02, VO_BabblingBook_Male_Book_LETL_Idle_03 };

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> soundFiles = new List<string> { VO_BabblingBook_Male_Book_LETL_Intro_01, VO_BabblingBook_Male_Book_LETL_Idle_01, VO_BabblingBook_Male_Book_LETL_Idle_02, VO_BabblingBook_Male_Book_LETL_Idle_03, VO_BabblingBook_Male_Book_LETL_Attack_01, VO_BabblingBook_Male_Book_LETL_Attack_02, VO_BabblingBook_Male_Book_LETL_Death_01 };
		SetBossVOLines(soundFiles);
		foreach (string soundFile in soundFiles)
		{
			GameState.Get().GetGameEntity().PreloadSound(soundFile);
		}
	}

	public override void OnCreateGame()
	{
		base.OnCreateGame();
		m_introLine = VO_BabblingBook_Male_Book_LETL_Intro_01;
		m_deathLine = VO_BabblingBook_Male_Book_LETL_Death_01;
	}

	public override IEnumerator RespondToWillPlayCardWithTiming(string cardID, Entity playedEntity)
	{
		while (m_enemySpeaking)
		{
			yield return null;
		}
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LT24_812H");
		if (!(playedEntity.GetLettuceAbilityOwner().GetCardId() == "LT24_812H"))
		{
			yield break;
		}
		if (!(cardID == "LT24_812P1"))
		{
			if (cardID == "LETL_1139")
			{
				GameState.Get().SetBusy(busy: true);
				yield return MissionPlayVOOnce(bossActor, VO_BabblingBook_Male_Book_LETL_Attack_02);
				GameState.Get().SetBusy(busy: false);
			}
		}
		else
		{
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVOOnce(bossActor, VO_BabblingBook_Male_Book_LETL_Attack_01);
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LT24_812H");
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LT24_812H");
		if (entity.GetCardId() == "LT24_812H")
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LT24_812H");
		if (turn == 1)
		{
			yield return MissionPlayVOOnce(bossActor, m_introLine);
		}
	}
}
