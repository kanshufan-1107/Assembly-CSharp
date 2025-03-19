using System.Collections;
using System.Collections.Generic;

public class LettuceBoss_LETL_840H_VoHandler : VoPlaybackHandler
{
	private static readonly AssetReference VO_BRMA05_1_CARD_05 = new AssetReference("VO_BRMA05_1_CARD_05.prefab:c0bc2f9cc3d3ae047ba80ffa0f70dcb8");

	private static readonly AssetReference VO_BRMA05_1_DEATH_04 = new AssetReference("VO_BRMA05_1_DEATH_04.prefab:48366fa92e2fb6648b45700ce40715b7");

	private static readonly AssetReference VO_BRMA05_1_HERO_POWER_06 = new AssetReference("VO_BRMA05_1_HERO_POWER_06.prefab:2792e43708ba1df48baa3a41d636097a");

	private static readonly AssetReference VO_BRMA05_1_RESPONSE_03 = new AssetReference("VO_BRMA05_1_RESPONSE_03.prefab:beac5b0620de49f42a2f2a66a906d4d6");

	private static readonly AssetReference VO_BRMA05_1_START_01 = new AssetReference("VO_BRMA05_1_START_01.prefab:590531d432b26ed46a1b36981630723d");

	private static readonly AssetReference VO_BRMA05_1_TURN1_02 = new AssetReference("VO_BRMA05_1_TURN1_02.prefab:b68353491d7f88a4a8479e7a031aec12");

	private List<string> m_IdleLines = new List<string> { VO_BRMA05_1_TURN1_02, VO_BRMA05_1_CARD_05 };

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> soundFiles = new List<string> { VO_BRMA05_1_CARD_05, VO_BRMA05_1_DEATH_04, VO_BRMA05_1_HERO_POWER_06, VO_BRMA05_1_RESPONSE_03, VO_BRMA05_1_START_01, VO_BRMA05_1_TURN1_02 };
		SetBossVOLines(soundFiles);
		foreach (string soundFile in soundFiles)
		{
			GameState.Get().GetGameEntity().PreloadSound(soundFile);
		}
	}

	public override void OnCreateGame()
	{
		base.OnCreateGame();
		m_introLine = VO_BRMA05_1_START_01;
		m_deathLine = VO_BRMA05_1_DEATH_04;
	}

	public override IEnumerator RespondToWillPlayCardWithTiming(string cardID, Entity playedEntity)
	{
		while (m_enemySpeaking)
		{
			yield return null;
		}
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_840H");
		if (playedEntity.GetLettuceAbilityOwner().GetCardId() == "LETL_840H")
		{
			switch (cardID)
			{
			case "LETL_030P2_04":
			case "LETL_030P2_05":
				GameState.Get().SetBusy(busy: true);
				yield return MissionPlayVOOnce(bossActor, VO_BRMA05_1_HERO_POWER_06);
				GameState.Get().SetBusy(busy: false);
				break;
			case "LETL_030P4_04":
			case "LETL_030P4_05":
				GameState.Get().SetBusy(busy: true);
				yield return MissionPlayVOOnce(bossActor, VO_BRMA05_1_RESPONSE_03);
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_840H");
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_840H");
		if (entity.GetCardId() == "LETL_840H")
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_840H");
		if (turn == 1)
		{
			yield return MissionPlayVOOnce(bossActor, m_introLine);
		}
	}
}
