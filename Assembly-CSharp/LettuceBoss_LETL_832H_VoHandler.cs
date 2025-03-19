using System.Collections;
using System.Collections.Generic;

public class LettuceBoss_LETL_832H_VoHandler : VoPlaybackHandler
{
	private static readonly AssetReference VO_DRGA_BOSS_10h_Male_Elemental_Good_Fight_02_Boss_Death_01 = new AssetReference("VO_DRGA_BOSS_10h_Male_Elemental_Good_Fight_02_Boss_Death_01.prefab:da1b64ec5dcd7694eac0d67c240e4880");

	private static readonly AssetReference VO_DRGA_BOSS_10h_Male_Elemental_Good_Fight_02_BossAttack_01 = new AssetReference("VO_DRGA_BOSS_10h_Male_Elemental_Good_Fight_02_BossAttack_01.prefab:484551f72cc900e478af602332b3a57e");

	private static readonly AssetReference VO_DRGA_BOSS_10h_Male_Elemental_Good_Fight_02_BossStart_01 = new AssetReference("VO_DRGA_BOSS_10h_Male_Elemental_Good_Fight_02_BossStart_01.prefab:92219bb8f6e482f4ebe0ff4577c3ddfc");

	private static readonly AssetReference VO_DRGA_BOSS_10h_Male_Elemental_Good_Fight_02_EmoteResponse_01 = new AssetReference("VO_DRGA_BOSS_10h_Male_Elemental_Good_Fight_02_EmoteResponse_01.prefab:145f4cbc25254034f9a5ad8fd60539ec");

	private static readonly AssetReference VO_DRGA_BOSS_10h_Male_Elemental_Good_Fight_02_Idle_01_01 = new AssetReference("VO_DRGA_BOSS_10h_Male_Elemental_Good_Fight_02_Idle_01_01.prefab:849043988be8d9846a718f7fd239bc0e");

	private static readonly AssetReference VO_DRGA_BOSS_10h_Male_Elemental_Good_Fight_02_Idle_02_01 = new AssetReference("VO_DRGA_BOSS_10h_Male_Elemental_Good_Fight_02_Idle_02_01.prefab:d85dc236aeb873d4fa10374d9300d21c");

	private static readonly AssetReference VO_DRGA_BOSS_10h_Male_Elemental_Good_Fight_02_Idle_03_01 = new AssetReference("VO_DRGA_BOSS_10h_Male_Elemental_Good_Fight_02_Idle_03_01.prefab:6374771862d99fd4994ff3c74b2b9c36");

	private List<string> m_IdleLines = new List<string> { VO_DRGA_BOSS_10h_Male_Elemental_Good_Fight_02_Idle_01_01, VO_DRGA_BOSS_10h_Male_Elemental_Good_Fight_02_Idle_02_01, VO_DRGA_BOSS_10h_Male_Elemental_Good_Fight_02_Idle_03_01 };

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> soundFiles = new List<string> { VO_DRGA_BOSS_10h_Male_Elemental_Good_Fight_02_Boss_Death_01, VO_DRGA_BOSS_10h_Male_Elemental_Good_Fight_02_BossAttack_01, VO_DRGA_BOSS_10h_Male_Elemental_Good_Fight_02_BossStart_01, VO_DRGA_BOSS_10h_Male_Elemental_Good_Fight_02_EmoteResponse_01, VO_DRGA_BOSS_10h_Male_Elemental_Good_Fight_02_Idle_01_01, VO_DRGA_BOSS_10h_Male_Elemental_Good_Fight_02_Idle_02_01, VO_DRGA_BOSS_10h_Male_Elemental_Good_Fight_02_Idle_03_01 };
		SetBossVOLines(soundFiles);
		foreach (string soundFile in soundFiles)
		{
			GameState.Get().GetGameEntity().PreloadSound(soundFile);
		}
	}

	public override void OnCreateGame()
	{
		base.OnCreateGame();
		m_introLine = VO_DRGA_BOSS_10h_Male_Elemental_Good_Fight_02_BossStart_01;
		m_deathLine = VO_DRGA_BOSS_10h_Male_Elemental_Good_Fight_02_Boss_Death_01;
	}

	public override IEnumerator RespondToWillPlayCardWithTiming(string cardID, Entity playedEntity)
	{
		while (m_enemySpeaking)
		{
			yield return null;
		}
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_832H");
		if (playedEntity.GetLettuceAbilityOwner().GetCardId() == "LETL_832H" && cardID == "LETL_832P1_01")
		{
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVOOnce(bossActor, VO_DRGA_BOSS_10h_Male_Elemental_Good_Fight_02_BossAttack_01);
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_832H");
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_832H");
		if (entity.GetCardId() == "LETL_832H")
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_832H");
		if (turn == 1)
		{
			yield return MissionPlayVOOnce(bossActor, m_introLine);
		}
	}
}
