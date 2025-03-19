using System.Collections;
using System.Collections.Generic;

public class LettuceBoss_LETL_854H_VoHandler : VoPlaybackHandler
{
	private static readonly AssetReference VO_BOM_08_006_Male_Orc_DrekThar_InGame_EmoteResponse_01 = new AssetReference("VO_BOM_08_006_Male_Orc_DrekThar_InGame_EmoteResponse_01.prefab:88b4179bdd910244c929ac7940c5c65b");

	private static readonly AssetReference VO_BOM_08_006_Male_Orc_DrekThar_InGame_BossIdle_01 = new AssetReference("VO_BOM_08_006_Male_Orc_DrekThar_InGame_BossIdle_01.prefab:7a35c9e81b641ef41b3da1d66591c5a4");

	private static readonly AssetReference VO_BOM_08_006_Male_Orc_DrekThar_InGame_BossIdle_02 = new AssetReference("VO_BOM_08_006_Male_Orc_DrekThar_InGame_BossIdle_02.prefab:5db1e882a4b902b44b28f862c8e5e239");

	private static readonly AssetReference Death = new AssetReference("Death.prefab:76a4ff0c9ea3bea4daff6d9c21dd1e9a");

	private List<string> m_IdleLines = new List<string> { VO_BOM_08_006_Male_Orc_DrekThar_InGame_BossIdle_01 };

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> soundFiles = new List<string> { VO_BOM_08_006_Male_Orc_DrekThar_InGame_EmoteResponse_01, VO_BOM_08_006_Male_Orc_DrekThar_InGame_BossIdle_01, VO_BOM_08_006_Male_Orc_DrekThar_InGame_BossIdle_02, Death };
		SetBossVOLines(soundFiles);
		foreach (string soundFile in soundFiles)
		{
			GameState.Get().GetGameEntity().PreloadSound(soundFile);
		}
	}

	public override void OnCreateGame()
	{
		base.OnCreateGame();
		m_introLine = VO_BOM_08_006_Male_Orc_DrekThar_InGame_EmoteResponse_01;
		m_deathLine = Death;
	}

	public override IEnumerator RespondToWillPlayCardWithTiming(string cardID, Entity playedEntity)
	{
		while (m_enemySpeaking)
		{
			yield return null;
		}
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_854H");
		if (playedEntity.GetLettuceAbilityOwner().GetCardId() == "LETL_854H" && cardID == "LETL_854P1_01")
		{
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(bossActor, VO_BOM_08_006_Male_Orc_DrekThar_InGame_BossIdle_02);
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_854H");
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_854H");
		if (entity.GetCardId() == "LETL_854H")
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_854H");
		if (turn == 1)
		{
			yield return MissionPlayVOOnce(bossActor, m_introLine);
		}
	}
}
