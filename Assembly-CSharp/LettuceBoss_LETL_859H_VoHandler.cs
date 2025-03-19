using System.Collections;
using System.Collections.Generic;

public class LettuceBoss_LETL_859H_VoHandler : VoPlaybackHandler
{
	private static readonly AssetReference VO_VanndarStormpike_Male_Dwarf_Attack_01 = new AssetReference("VO_VanndarStormpike_Male_Dwarf_Attack_01.prefab:e9bd71b4a3611d641a3449a8cf8131aa");

	private static readonly AssetReference VO_VanndarStormpike_Male_Dwarf_Bark_10 = new AssetReference("VO_VanndarStormpike_Male_Dwarf_Bark_10.prefab:0bc51d6c0b9aa3d44880a5e3e4296e2b");

	private static readonly AssetReference VO_VanndarStormpike_Male_Dwarf_Death_01 = new AssetReference("VO_VanndarStormpike_Male_Dwarf_Death_01.prefab:f68fc7b7e88c0864db1d6e470c97489c");

	private static readonly AssetReference VO_VanndarStormpike_Male_Dwarf_Play_01 = new AssetReference("VO_VanndarStormpike_Male_Dwarf_Play_01.prefab:191a9ca0e4a2e4244a98eaf5478bff1d");

	private List<string> m_IdleLines = new List<string> { VO_VanndarStormpike_Male_Dwarf_Bark_10 };

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> soundFiles = new List<string> { VO_VanndarStormpike_Male_Dwarf_Attack_01, VO_VanndarStormpike_Male_Dwarf_Bark_10, VO_VanndarStormpike_Male_Dwarf_Death_01, VO_VanndarStormpike_Male_Dwarf_Play_01 };
		SetBossVOLines(soundFiles);
		foreach (string soundFile in soundFiles)
		{
			GameState.Get().GetGameEntity().PreloadSound(soundFile);
		}
	}

	public override void OnCreateGame()
	{
		base.OnCreateGame();
		m_introLine = VO_VanndarStormpike_Male_Dwarf_Play_01;
		m_deathLine = VO_VanndarStormpike_Male_Dwarf_Death_01;
	}

	public override IEnumerator RespondToWillPlayCardWithTiming(string cardID, Entity playedEntity)
	{
		while (m_enemySpeaking)
		{
			yield return null;
		}
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_859H");
		if (playedEntity.GetLettuceAbilityOwner().GetCardId() == "LETL_859H" && cardID == "LETL_859P1")
		{
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(bossActor, VO_VanndarStormpike_Male_Dwarf_Attack_01);
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_859H");
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_859H");
		if (entity.GetCardId() == "LETL_859H")
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_859H");
		if (turn == 1)
		{
			yield return MissionPlayVOOnce(bossActor, m_introLine);
		}
	}
}
