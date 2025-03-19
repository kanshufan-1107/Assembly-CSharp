using System.Collections;
using System.Collections.Generic;

public class LettuceBoss_LT23_804H_VoHandler : VoPlaybackHandler
{
	private static readonly AssetReference VO_Chogall_Male_Ogre_LETL_Attack_01 = new AssetReference("VO_Chogall_Male_Ogre_LETL_Attack_01.prefab:e5892e80ee1ce2b4780b3e683861b3ae");

	private static readonly AssetReference VO_Chogall_Male_Ogre_LETL_Bark_10 = new AssetReference("VO_Chogall_Male_Ogre_LETL_Bark_10.prefab:634c29628d74b184ea0f002825b125b5");

	private static readonly AssetReference VO_Chogall_Male_Ogre_LETL_Death_01 = new AssetReference("VO_Chogall_Male_Ogre_LETL_Death_01.prefab:952b9f8ac3a59264788dd9c2fb2ea3ad");

	private static readonly AssetReference VO_Chogall_Male_Ogre_LETL_Intro_01 = new AssetReference("VO_Chogall_Male_Ogre_LETL_Intro_01.prefab:e91e8f71c07e801479c142da012a3de7");

	private static readonly AssetReference VO_Chogall_Male_Ogre_LETL_Bark_03 = new AssetReference("VO_Chogall_Male_Ogre_LETL_Bark_03.prefab:3e8041fbd1a87e04782637267cfa7c5f");

	private List<string> m_IdleLines = new List<string> { VO_Chogall_Male_Ogre_LETL_Bark_10 };

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> soundFiles = new List<string> { VO_Chogall_Male_Ogre_LETL_Attack_01, VO_Chogall_Male_Ogre_LETL_Bark_10, VO_Chogall_Male_Ogre_LETL_Death_01, VO_Chogall_Male_Ogre_LETL_Intro_01, VO_Chogall_Male_Ogre_LETL_Bark_03 };
		SetBossVOLines(soundFiles);
		foreach (string soundFile in soundFiles)
		{
			GameState.Get().GetGameEntity().PreloadSound(soundFile);
		}
	}

	public override void OnCreateGame()
	{
		base.OnCreateGame();
		m_introLine = VO_Chogall_Male_Ogre_LETL_Intro_01;
		m_deathLine = VO_Chogall_Male_Ogre_LETL_Death_01;
	}

	public override IEnumerator RespondToWillPlayCardWithTiming(string cardID, Entity playedEntity)
	{
		while (m_enemySpeaking)
		{
			yield return null;
		}
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LT23_804H");
		Actor bossActor2 = FindEnemyActorInPlayByDesignCode("LT23_804H2");
		string designCode = playedEntity.GetLettuceAbilityOwner().GetCardId();
		if (designCode == "LT23_804H")
		{
			if (cardID == "LT23_804P1")
			{
				GameState.Get().SetBusy(busy: true);
				yield return MissionPlayVO(bossActor, VO_Chogall_Male_Ogre_LETL_Attack_01);
				GameState.Get().SetBusy(busy: false);
			}
		}
		else if (designCode == "LT23_804H2" && cardID == "LT23_804P2")
		{
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(bossActor2, VO_Chogall_Male_Ogre_LETL_Bark_03);
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LT23_804H");
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LT23_804H");
		if (entity.GetCardId() == "LT23_804H")
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LT23_804H");
		if (turn == 1)
		{
			yield return MissionPlayVOOnce(bossActor, m_introLine);
		}
	}
}
