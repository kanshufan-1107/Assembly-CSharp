using System.Collections;
using System.Collections.Generic;

public class LettuceBoss_LETL_836H_VoHandler : VoPlaybackHandler
{
	private static readonly AssetReference VO_LETL_836H_Male_Dwarf_Attack_01 = new AssetReference("VO_LETL_836H_Male_Dwarf_Attack_01.prefab:95133ab617b9c7341a90d2be35773593");

	private static readonly AssetReference VO_LETL_836H_Male_Dwarf_Attack_02 = new AssetReference("VO_LETL_836H_Male_Dwarf_Attack_02.prefab:ebed45970a8526e46a68dc30fbd79a5b");

	private static readonly AssetReference VO_LETL_836H_Male_Dwarf_Death_01 = new AssetReference("VO_LETL_836H_Male_Dwarf_Death_01.prefab:5fc5642a24c9ba3449115e79ecbd4a08");

	private static readonly AssetReference VO_LETL_836H_Male_Dwarf_Idle_01 = new AssetReference("VO_LETL_836H_Male_Dwarf_Idle_01.prefab:0d1a1a041dfa1ec4fac4faa165d25341");

	private static readonly AssetReference VO_LETL_836H_Male_Dwarf_Intro_01 = new AssetReference("VO_LETL_836H_Male_Dwarf_Intro_01.prefab:bf4c653a66c57274cae70371ef618c61");

	private List<string> m_IdleLines = new List<string> { VO_LETL_836H_Male_Dwarf_Idle_01 };

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> soundFiles = new List<string> { VO_LETL_836H_Male_Dwarf_Attack_01, VO_LETL_836H_Male_Dwarf_Attack_02, VO_LETL_836H_Male_Dwarf_Death_01, VO_LETL_836H_Male_Dwarf_Idle_01, VO_LETL_836H_Male_Dwarf_Intro_01 };
		SetBossVOLines(soundFiles);
		foreach (string soundFile in soundFiles)
		{
			GameState.Get().GetGameEntity().PreloadSound(soundFile);
		}
	}

	public override void OnCreateGame()
	{
		base.OnCreateGame();
		m_introLine = VO_LETL_836H_Male_Dwarf_Intro_01;
		m_deathLine = VO_LETL_836H_Male_Dwarf_Death_01;
	}

	public override IEnumerator RespondToWillPlayCardWithTiming(string cardID, Entity playedEntity)
	{
		while (m_enemySpeaking)
		{
			yield return null;
		}
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_836H");
		if (!(playedEntity.GetLettuceAbilityOwner().GetCardId() == "LETL_836H"))
		{
			yield break;
		}
		if (!(cardID == "LETL_836P1_01"))
		{
			if (cardID == "LETL_836P2_01")
			{
				GameState.Get().SetBusy(busy: true);
				yield return MissionPlayVOOnce(bossActor, VO_LETL_836H_Male_Dwarf_Attack_02);
				GameState.Get().SetBusy(busy: false);
			}
		}
		else
		{
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVOOnce(bossActor, VO_LETL_836H_Male_Dwarf_Attack_01);
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_836H");
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_836H");
		if (entity.GetCardId() == "LETL_836H")
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_836H");
		if (turn == 1)
		{
			yield return MissionPlayVOOnce(bossActor, m_introLine);
		}
	}
}
