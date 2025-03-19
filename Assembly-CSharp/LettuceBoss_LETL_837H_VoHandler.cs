using System.Collections;
using System.Collections.Generic;

public class LettuceBoss_LETL_837H_VoHandler : VoPlaybackHandler
{
	private static readonly AssetReference VO_LETL_837H_Male_Dwarf_Attack_01 = new AssetReference("VO_LETL_837H_Male_Dwarf_Attack_01.prefab:ec51c087926ec69458871c4fec4d9ef5");

	private static readonly AssetReference VO_LETL_837H_Male_Dwarf_Attack_02 = new AssetReference("VO_LETL_837H_Male_Dwarf_Attack_02.prefab:0dbcdf52419317e46a33ef2dc3b8e1b4");

	private static readonly AssetReference VO_LETL_837H_Male_Dwarf_Death_01 = new AssetReference("VO_LETL_837H_Male_Dwarf_Death_01.prefab:0ee0b213d9e941545add54425c48bc4d");

	private static readonly AssetReference VO_LETL_837H_Male_Dwarf_Idle_01 = new AssetReference("VO_LETL_837H_Male_Dwarf_Idle_01.prefab:bb9c8e9cafba4454da306f78abe3184f");

	private static readonly AssetReference VO_LETL_837H_Male_Dwarf_Intro_01 = new AssetReference("VO_LETL_837H_Male_Dwarf_Intro_01.prefab:005552efbb15d9844aac3686d5f03c23");

	private List<string> m_IdleLines = new List<string> { VO_LETL_837H_Male_Dwarf_Idle_01 };

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> soundFiles = new List<string> { VO_LETL_837H_Male_Dwarf_Attack_01, VO_LETL_837H_Male_Dwarf_Attack_02, VO_LETL_837H_Male_Dwarf_Death_01, VO_LETL_837H_Male_Dwarf_Idle_01, VO_LETL_837H_Male_Dwarf_Intro_01 };
		SetBossVOLines(soundFiles);
		foreach (string soundFile in soundFiles)
		{
			GameState.Get().GetGameEntity().PreloadSound(soundFile);
		}
	}

	public override void OnCreateGame()
	{
		base.OnCreateGame();
		m_introLine = VO_LETL_837H_Male_Dwarf_Intro_01;
		m_deathLine = VO_LETL_837H_Male_Dwarf_Death_01;
	}

	public override IEnumerator RespondToWillPlayCardWithTiming(string cardID, Entity playedEntity)
	{
		while (m_enemySpeaking)
		{
			yield return null;
		}
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_837H");
		if (playedEntity.GetLettuceAbilityOwner().GetCardId() == "LETL_837H")
		{
			switch (cardID)
			{
			case "LETL_837_1":
			case "LETL_837_3":
				GameState.Get().SetBusy(busy: true);
				yield return MissionPlayVOOnce(bossActor, VO_LETL_837H_Male_Dwarf_Attack_01);
				GameState.Get().SetBusy(busy: false);
				break;
			case "LETL_837_2":
				GameState.Get().SetBusy(busy: true);
				yield return MissionPlayVOOnce(bossActor, VO_LETL_837H_Male_Dwarf_Attack_02);
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_837H");
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_837H");
		if (entity.GetCardId() == "LETL_837H")
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_837H");
		if (turn == 1)
		{
			yield return MissionPlayVOOnce(bossActor, m_introLine);
		}
	}
}
