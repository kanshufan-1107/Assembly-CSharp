using System.Collections;
using System.Collections.Generic;

public class LettuceBoss_LETL_853H_VoHandler : VoPlaybackHandler
{
	private static readonly AssetReference VO_BOM_09_006_Male_Elemental_Lokholar_InGame_BossIdle_02 = new AssetReference("VO_BOM_09_006_Male_Elemental_Lokholar_InGame_BossIdle_02.prefab:fd6694af06b96c74ea92572c22fbc984");

	private static readonly AssetReference VO_BOM_09_006_Male_Elemental_Lokholar_InGame_BossIdle_03 = new AssetReference("VO_BOM_09_006_Male_Elemental_Lokholar_InGame_BossIdle_03.prefab:22ee7f95139c667448ef2c0420c509c7");

	private static readonly AssetReference VO_BOM_09_005_Male_Elemental_Lokholar_InGame_VictoryPreExplosion_01_B = new AssetReference("VO_BOM_09_005_Male_Elemental_Lokholar_InGame_VictoryPreExplosion_01_B.prefab:fe01006941e39b346a5a2e060875eda2");

	private static readonly AssetReference Death = new AssetReference("Death.prefab:76a4ff0c9ea3bea4daff6d9c21dd1e9a");

	private List<string> m_IdleLines = new List<string> { VO_BOM_09_006_Male_Elemental_Lokholar_InGame_BossIdle_03 };

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> soundFiles = new List<string> { VO_BOM_09_006_Male_Elemental_Lokholar_InGame_BossIdle_02, VO_BOM_09_006_Male_Elemental_Lokholar_InGame_BossIdle_03, VO_BOM_09_005_Male_Elemental_Lokholar_InGame_VictoryPreExplosion_01_B, Death };
		SetBossVOLines(soundFiles);
		foreach (string soundFile in soundFiles)
		{
			GameState.Get().GetGameEntity().PreloadSound(soundFile);
		}
	}

	public override void OnCreateGame()
	{
		base.OnCreateGame();
		m_introLine = VO_BOM_09_005_Male_Elemental_Lokholar_InGame_VictoryPreExplosion_01_B;
		m_deathLine = Death;
	}

	public override IEnumerator RespondToWillPlayCardWithTiming(string cardID, Entity playedEntity)
	{
		while (m_enemySpeaking)
		{
			yield return null;
		}
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_853H");
		if (playedEntity.GetLettuceAbilityOwner().GetCardId() == "LETL_853H" && cardID == "LETL_853P3_01")
		{
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(bossActor, VO_BOM_09_006_Male_Elemental_Lokholar_InGame_BossIdle_02);
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_853H");
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_853H");
		if (entity.GetCardId() == "LETL_853H")
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_853H");
		if (turn == 1)
		{
			yield return MissionPlayVOOnce(bossActor, m_introLine);
		}
	}
}
