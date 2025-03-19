using System.Collections;
using System.Collections.Generic;

public class LettuceBoss_LT24_815H_VoHandler : VoPlaybackHandler
{
	private static readonly AssetReference VO_Barnes_Male_Human_LETL_C03_T05_Dialogue_01 = new AssetReference("VO_Barnes_Male_Human_LETL_C03_T05_Dialogue_01.prefab:e2311d48f799a044d92d18ccd7976be0");

	private static readonly AssetReference KAR_114_Male_Human_Attack_01 = new AssetReference("KAR_114_Male_Human_Attack_01.prefab:fa49b48d923085f499520d64f32e2807");

	private static readonly AssetReference KAR_114_Male_Human_Death_01 = new AssetReference("KAR_114_Male_Human_Death_01.prefab:fd181cc333977384db5c38e503a700fc");

	private static readonly AssetReference KAR_114_Male_Human_Idle_01 = new AssetReference("KAR_114_Male_Human_Idle_01.prefab:1427869adc205c045bb07c8e5eb0235b");

	private static readonly AssetReference KAR_114_Male_Human_Idle_02 = new AssetReference("KAR_114_Male_Human_Idle_02.prefab:741b0ec8194d05049a13d93672155523");

	private static readonly AssetReference KAR_114_Male_Human_Idle_03 = new AssetReference("KAR_114_Male_Human_Idle_03.prefab:586f74f59db5d1249bd6e8d52c6cafe3");

	private static readonly AssetReference KAR_114_Male_Human_Idle_04 = new AssetReference("KAR_114_Male_Human_Idle_04.prefab:de5de7465397c8c4caabd9f6398b43ee");

	private List<string> m_IdleLines = new List<string> { KAR_114_Male_Human_Idle_01, KAR_114_Male_Human_Idle_02, KAR_114_Male_Human_Idle_04 };

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> soundFiles = new List<string> { VO_Barnes_Male_Human_LETL_C03_T05_Dialogue_01, KAR_114_Male_Human_Idle_01, KAR_114_Male_Human_Idle_02, KAR_114_Male_Human_Idle_04, KAR_114_Male_Human_Attack_01, KAR_114_Male_Human_Idle_03, KAR_114_Male_Human_Death_01 };
		SetBossVOLines(soundFiles);
		foreach (string soundFile in soundFiles)
		{
			GameState.Get().GetGameEntity().PreloadSound(soundFile);
		}
	}

	public override void OnCreateGame()
	{
		base.OnCreateGame();
		m_introLine = VO_Barnes_Male_Human_LETL_C03_T05_Dialogue_01;
		m_deathLine = KAR_114_Male_Human_Death_01;
	}

	public override IEnumerator RespondToWillPlayCardWithTiming(string cardID, Entity playedEntity)
	{
		while (m_enemySpeaking)
		{
			yield return null;
		}
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LT24_815H");
		if (!(playedEntity.GetLettuceAbilityOwner().GetCardId() == "LT24_815H"))
		{
			yield break;
		}
		if (!(cardID == "LT24_815P1"))
		{
			if (cardID == "LT23_820P1")
			{
				GameState.Get().SetBusy(busy: true);
				yield return MissionPlayVOOnce(bossActor, KAR_114_Male_Human_Attack_01);
				GameState.Get().SetBusy(busy: false);
			}
		}
		else
		{
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVOOnce(bossActor, KAR_114_Male_Human_Idle_03);
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LT24_815H");
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LT24_815H");
		if (entity.GetCardId() == "LT24_815H")
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LT24_815H");
		if (turn == 1)
		{
			yield return MissionPlayVOOnce(bossActor, m_introLine);
		}
	}
}
