using System.Collections;
using System.Collections.Generic;

public class LettuceBoss_LT24_811H_VoHandler : VoPlaybackHandler
{
	private static readonly AssetReference VO_AttumenTheHuntsman_Male_Undead_LETL_Attack_01 = new AssetReference("VO_AttumenTheHuntsman_Male_Undead_LETL_Attack_01.prefab:3b1738abff9713f4ea338230f7e6fda3");

	private static readonly AssetReference VO_AttumenTheHuntsman_Male_Undead_LETL_Attack_02 = new AssetReference("VO_AttumenTheHuntsman_Male_Undead_LETL_Attack_02.prefab:024b8e27863310f4eaf7a55684ee25be");

	private static readonly AssetReference VO_AttumenTheHuntsman_Male_Undead_LETL_Death_01 = new AssetReference("VO_AttumenTheHuntsman_Male_Undead_LETL_Death_01.prefab:bc489218ac6b36d47812e6ec69039add");

	private static readonly AssetReference VO_AttumenTheHuntsman_Male_Undead_LETL_Idle_01 = new AssetReference("VO_AttumenTheHuntsman_Male_Undead_LETL_Idle_01.prefab:5d46b76c0d29ba343a942f366b99b838");

	private static readonly AssetReference VO_AttumenTheHuntsman_Male_Undead_LETL_Intro_01 = new AssetReference("VO_AttumenTheHuntsman_Male_Undead_LETL_Intro_01.prefab:85e92c0e8eb73d147b053c95c0f59c5c");

	private List<string> m_IdleLines = new List<string> { VO_AttumenTheHuntsman_Male_Undead_LETL_Idle_01 };

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> soundFiles = new List<string> { VO_AttumenTheHuntsman_Male_Undead_LETL_Intro_01, VO_AttumenTheHuntsman_Male_Undead_LETL_Idle_01, VO_AttumenTheHuntsman_Male_Undead_LETL_Attack_01, VO_AttumenTheHuntsman_Male_Undead_LETL_Attack_02, VO_AttumenTheHuntsman_Male_Undead_LETL_Death_01 };
		SetBossVOLines(soundFiles);
		foreach (string soundFile in soundFiles)
		{
			GameState.Get().GetGameEntity().PreloadSound(soundFile);
		}
	}

	public override void OnCreateGame()
	{
		base.OnCreateGame();
		m_introLine = VO_AttumenTheHuntsman_Male_Undead_LETL_Intro_01;
		m_deathLine = VO_AttumenTheHuntsman_Male_Undead_LETL_Death_01;
	}

	public override IEnumerator RespondToWillPlayCardWithTiming(string cardID, Entity playedEntity)
	{
		while (m_enemySpeaking)
		{
			yield return null;
		}
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LT24_811H");
		if (playedEntity.GetLettuceAbilityOwner().GetCardId() == "LT24_811H")
		{
			switch (cardID)
			{
			case "LETL_262_03":
				GameState.Get().SetBusy(busy: true);
				yield return MissionPlayVOOnce(bossActor, VO_AttumenTheHuntsman_Male_Undead_LETL_Attack_01);
				GameState.Get().SetBusy(busy: false);
				break;
			case "LETL_262_05":
				GameState.Get().SetBusy(busy: true);
				yield return MissionPlayVOOnce(bossActor, VO_AttumenTheHuntsman_Male_Undead_LETL_Attack_01);
				GameState.Get().SetBusy(busy: false);
				break;
			case "LETL_015P9_03":
				GameState.Get().SetBusy(busy: true);
				yield return MissionPlayVOOnce(bossActor, VO_AttumenTheHuntsman_Male_Undead_LETL_Attack_02);
				GameState.Get().SetBusy(busy: false);
				break;
			case "LETL_015P9_05":
				GameState.Get().SetBusy(busy: true);
				yield return MissionPlayVOOnce(bossActor, VO_AttumenTheHuntsman_Male_Undead_LETL_Attack_02);
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LT24_811H");
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LT24_811H");
		if (entity.GetCardId() == "LT24_811H")
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LT24_811H");
		if (turn == 1)
		{
			yield return MissionPlayVOOnce(bossActor, m_introLine);
		}
	}
}
