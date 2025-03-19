using System.Collections;
using System.Collections.Generic;

public class LettuceBoss_LT24_810H_VoHandler : VoPlaybackHandler
{
	private static readonly AssetReference VO_OG_321_Male_Dwarf_Play_01 = new AssetReference("VO_OG_321_Male_Dwarf_Play_01.prefab:89430659a930d5740b8ac7fc26fdcd79");

	private static readonly AssetReference VO_OG_321_Male_Dwarf_Attack_01 = new AssetReference("VO_OG_321_Male_Dwarf_Attack_01.prefab:45109a26787b1ff418f175e1b9b2fb09");

	private static readonly AssetReference VO_OG_321_Male_Dwarf_Death_01 = new AssetReference("VO_OG_321_Male_Dwarf_Death_01.prefab:19f9d2e895b74034e94ba4ab534bd318");

	private List<string> m_IdleLines = new List<string> { VO_OG_321_Male_Dwarf_Play_01 };

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> soundFiles = new List<string> { VO_OG_321_Male_Dwarf_Play_01, VO_OG_321_Male_Dwarf_Attack_01, VO_OG_321_Male_Dwarf_Death_01 };
		SetBossVOLines(soundFiles);
		foreach (string soundFile in soundFiles)
		{
			GameState.Get().GetGameEntity().PreloadSound(soundFile);
		}
	}

	public override void OnCreateGame()
	{
		base.OnCreateGame();
		m_introLine = VO_OG_321_Male_Dwarf_Play_01;
		m_deathLine = VO_OG_321_Male_Dwarf_Death_01;
	}

	public override IEnumerator RespondToWillPlayCardWithTiming(string cardID, Entity playedEntity)
	{
		while (m_enemySpeaking)
		{
			yield return null;
		}
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LT24_810H2");
		if (playedEntity.GetLettuceAbilityOwner().GetCardId() == "LT24_810H2")
		{
			switch (cardID)
			{
			case "LETL_033P5_03":
				GameState.Get().SetBusy(busy: true);
				yield return MissionPlayVOOnce(bossActor, VO_OG_321_Male_Dwarf_Attack_01);
				GameState.Get().SetBusy(busy: false);
				break;
			case "LETL_033P5_05":
				GameState.Get().SetBusy(busy: true);
				yield return MissionPlayVOOnce(bossActor, VO_OG_321_Male_Dwarf_Attack_01);
				GameState.Get().SetBusy(busy: false);
				break;
			case "LT23T_126_02":
				GameState.Get().SetBusy(busy: true);
				yield return MissionPlayVOOnce(bossActor, VO_OG_321_Male_Dwarf_Attack_01);
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LT24_810H2");
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LT24_810H2");
		if (entity.GetCardId() == "LT24_810H2")
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LT24_810H2");
		if (turn == 1)
		{
			yield return MissionPlayVOOnce(bossActor, m_introLine);
		}
	}
}
