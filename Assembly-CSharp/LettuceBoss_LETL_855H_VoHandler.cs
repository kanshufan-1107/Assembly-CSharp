using System.Collections;
using System.Collections.Generic;

public class LettuceBoss_LETL_855H_VoHandler : VoPlaybackHandler
{
	private static readonly AssetReference VO_LieutenantRotimer_Male_Dwarf_Attack_01 = new AssetReference("VO_LieutenantRotimer_Male_Dwarf_Attack_01.prefab:db2ca013e746e4a4996e5f240b7b7cc0");

	private static readonly AssetReference VO_LieutenantRotimer_Male_Dwarf_Death_01 = new AssetReference("VO_LieutenantRotimer_Male_Dwarf_Death_01.prefab:72029f03a0b937c43b6c19e04fbeb8ad");

	private static readonly AssetReference VO_LieutenantRotimer_Male_Dwarf_Idle_01 = new AssetReference("VO_LieutenantRotimer_Male_Dwarf_Idle_01.prefab:99afda9f89cfb934b857c6243a028f4a");

	private static readonly AssetReference VO_LieutenantRotimer_Male_Dwarf_Intro_01 = new AssetReference("VO_LieutenantRotimer_Male_Dwarf_Intro_01.prefab:5fa23c6ba4082d140a9337a22dfd9294");

	private List<string> m_IdleLines = new List<string> { VO_LieutenantRotimer_Male_Dwarf_Idle_01 };

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> soundFiles = new List<string> { VO_LieutenantRotimer_Male_Dwarf_Attack_01, VO_LieutenantRotimer_Male_Dwarf_Death_01, VO_LieutenantRotimer_Male_Dwarf_Idle_01, VO_LieutenantRotimer_Male_Dwarf_Intro_01 };
		SetBossVOLines(soundFiles);
		foreach (string soundFile in soundFiles)
		{
			GameState.Get().GetGameEntity().PreloadSound(soundFile);
		}
	}

	public override void OnCreateGame()
	{
		base.OnCreateGame();
		m_introLine = VO_LieutenantRotimer_Male_Dwarf_Intro_01;
		m_deathLine = VO_LieutenantRotimer_Male_Dwarf_Death_01;
	}

	public override IEnumerator RespondToWillPlayCardWithTiming(string cardID, Entity playedEntity)
	{
		while (m_enemySpeaking)
		{
			yield return null;
		}
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_855H");
		if (playedEntity.GetLettuceAbilityOwner().GetCardId() == "LETL_855H")
		{
			if (cardID == "LETL_855P1_04" || cardID == "LETL_855P1_05")
			{
				GameState.Get().SetBusy(busy: true);
				yield return MissionPlayVOOnce(bossActor, VO_LieutenantRotimer_Male_Dwarf_Attack_01);
				GameState.Get().SetBusy(busy: false);
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_855H");
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_855H");
		if (entity.GetCardId() == "LETL_855H")
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_855H");
		if (turn == 1)
		{
			yield return MissionPlayVOOnce(bossActor, m_introLine);
		}
	}
}
