using System.Collections;
using System.Collections.Generic;

public class LettuceBoss_LETL_864H_VoHandler : VoPlaybackHandler
{
	private static readonly AssetReference VO_Onyxia_Female_Dragon_Fluff_06 = new AssetReference("VO_Onyxia_Female_Dragon_Fluff_06.prefab:48a8ea05c4ba7bb4b846f4c766f96b6f");

	private static readonly AssetReference VO_Onyxia_Female_Dragon_Idle_01 = new AssetReference("VO_Onyxia_Female_Dragon_Idle_01.prefab:e8995350199836a45b27dfe78cd42496");

	private static readonly AssetReference VO_Onyxia_Female_Dragon_Attack_02 = new AssetReference("VO_Onyxia_Female_Dragon_Attack_02.prefab:3a4f16f62d51c1a4d90cdd1d79c6eb3d");

	private static readonly AssetReference VO_Onyxia_Female_Dragon_PhaseTransition_03 = new AssetReference("VO_Onyxia_Female_Dragon_PhaseTransition_03.prefab:11aad5ceae7899a4b8cff52e8d8ae8cc");

	private static readonly AssetReference Death = new AssetReference("Death.prefab:b6bd9ea0e27442b4b8ee69edb1b18f41");

	private List<string> m_IdleLines = new List<string> { VO_Onyxia_Female_Dragon_Idle_01 };

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> soundFiles = new List<string> { VO_Onyxia_Female_Dragon_Fluff_06, VO_Onyxia_Female_Dragon_Idle_01, VO_Onyxia_Female_Dragon_Attack_02, VO_Onyxia_Female_Dragon_PhaseTransition_03 };
		SetBossVOLines(soundFiles);
		foreach (string soundFile in soundFiles)
		{
			GameState.Get().GetGameEntity().PreloadSound(soundFile);
		}
	}

	public override void OnCreateGame()
	{
		base.OnCreateGame();
		m_introLine = VO_Onyxia_Female_Dragon_Fluff_06;
		m_deathLine = Death;
	}

	public override IEnumerator RespondToWillPlayCardWithTiming(string cardID, Entity playedEntity)
	{
		while (m_enemySpeaking)
		{
			yield return null;
		}
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_864H");
		if (playedEntity.GetLettuceAbilityOwner().GetCardId() == "LETL_864H")
		{
			switch (cardID)
			{
			case "LT22_024P2_03":
				GameState.Get().SetBusy(busy: true);
				yield return MissionPlayVO(bossActor, VO_Onyxia_Female_Dragon_Attack_02);
				GameState.Get().SetBusy(busy: false);
				break;
			case "LT22_024P2_05":
				GameState.Get().SetBusy(busy: true);
				yield return MissionPlayVO(bossActor, VO_Onyxia_Female_Dragon_Attack_02);
				GameState.Get().SetBusy(busy: false);
				break;
			case "LETL_864P1":
				GameState.Get().SetBusy(busy: true);
				yield return MissionPlayVO(bossActor, VO_Onyxia_Female_Dragon_PhaseTransition_03);
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_864H");
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_864H");
		if (entity.GetCardId() == "LETL_864H")
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_864H");
		if (turn == 1)
		{
			yield return MissionPlayVOOnce(bossActor, m_introLine);
		}
	}
}
