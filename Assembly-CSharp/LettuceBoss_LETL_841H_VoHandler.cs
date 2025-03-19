using System.Collections;
using System.Collections.Generic;

public class LettuceBoss_LETL_841H_VoHandler : VoPlaybackHandler
{
	private static readonly AssetReference VO_BRMA06_3_INTRO_01 = new AssetReference("VO_BRMA06_3_INTRO_01.prefab:ccee32264258cd14f9875a94ff81d0ea");

	private static readonly AssetReference VO_BRMA06_3_RESPONSE_03 = new AssetReference("VO_BRMA06_3_RESPONSE_03.prefab:3abe0ccef6f202a45b4727361bc704df");

	private static readonly AssetReference VO_BRMA06_3_TURN1_02 = new AssetReference("VO_BRMA06_3_TURN1_02.prefab:7d7272a7a2a62bf4f91488020ed8ab94");

	private static readonly AssetReference VO_EX1_298_Attack_02 = new AssetReference("VO_EX1_298_Attack_02.prefab:7dd0c364ae8f57049bf82c4c94b72292");

	private static readonly AssetReference VO_EX1_298_Death_04 = new AssetReference("VO_EX1_298_Death_04.prefab:5dd820a21c877bc4693cf0ec8837a555");

	private static readonly AssetReference VO_EX1_298_Play_01 = new AssetReference("VO_EX1_298_Play_01.prefab:2c96b78abf795554e9cfe5643cab2141");

	private static readonly AssetReference VO_EX1_298_Trigger_03 = new AssetReference("VO_EX1_298_Trigger_03.prefab:b95e45a2eafca924786a3e53344bc9f5");

	private static readonly AssetReference VO_BRMA06_1_DEATH_04 = new AssetReference("VO_BRMA06_1_DEATH_04.prefab:78c2973f7c025a641bb953654e358879");

	private static readonly AssetReference VO_BRMA06_1_RESPONSE_03 = new AssetReference("VO_BRMA06_1_RESPONSE_03.prefab:a908e5d8056a26b4dbdc0ea833f19a6e");

	private static readonly AssetReference VO_BRMA06_1_SUMMON_RAG_05 = new AssetReference("VO_BRMA06_1_SUMMON_RAG_05.prefab:e79eafab2edcfe2428e817359ec11c65");

	private static readonly AssetReference VO_BRMA06_1_TURN1_02 = new AssetReference("VO_BRMA06_1_TURN1_02.prefab:76b698614db27b14c8ebac0e4d01b6f9");

	private static readonly AssetReference VO_BRMA06_1_TURN1_02_ALT = new AssetReference("VO_BRMA06_1_TURN1_02_ALT.prefab:e0ae95e6abc774f4b9bc68f07f7bbc29");

	private static readonly AssetReference VO_BRMA05_1_CARD_05 = new AssetReference("VO_BRMA05_1_CARD_05.prefab:c0bc2f9cc3d3ae047ba80ffa0f70dcb8");

	private static readonly AssetReference VO_BRMA05_1_DEATH_04 = new AssetReference("VO_BRMA05_1_DEATH_04.prefab:48366fa92e2fb6648b45700ce40715b7");

	private static readonly AssetReference VO_BRMA05_1_HERO_POWER_06 = new AssetReference("VO_BRMA05_1_HERO_POWER_06.prefab:2792e43708ba1df48baa3a41d636097a");

	private static readonly AssetReference VO_BRMA05_1_RESPONSE_03 = new AssetReference("VO_BRMA05_1_RESPONSE_03.prefab:beac5b0620de49f42a2f2a66a906d4d6");

	private static readonly AssetReference VO_BRMA05_1_START_01 = new AssetReference("VO_BRMA05_1_START_01.prefab:590531d432b26ed46a1b36981630723d");

	private static readonly AssetReference VO_BRMA05_1_TURN1_02 = new AssetReference("VO_BRMA05_1_TURN1_02.prefab:b68353491d7f88a4a8479e7a031aec12");

	private static readonly AssetReference VO_BRMA04_1_CARD_04 = new AssetReference("VO_BRMA04_1_CARD_04.prefab:53f20ec5598fc8a459615f6a57c661be");

	private static readonly AssetReference VO_BRMA04_1_HERO_POWER_05 = new AssetReference("VO_BRMA04_1_HERO_POWER_05.prefab:1c2e947768a86424abf65a8b5ad573ec");

	private static readonly AssetReference VO_BRMA04_1_RESPONSE_03 = new AssetReference("VO_BRMA04_1_RESPONSE_03.prefab:75a029ecfd071914aaf0def7bc041b85");

	private static readonly AssetReference VO_BRMA04_1_DEATH_06 = new AssetReference("VO_BRMA04_1_DEATH_06.prefab:34e63d08fa3428e4091c5cdbe63dd894");

	private static readonly AssetReference VO_BRMA04_1_START_01 = new AssetReference("VO_BRMA04_1_START_01.prefab:5d9de41d8c48c924a88ff1a539711761");

	private List<string> m_IdleLines = new List<string> { VO_BRMA06_1_RESPONSE_03 };

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> soundFiles = new List<string>
		{
			VO_BRMA06_3_INTRO_01, VO_BRMA06_3_RESPONSE_03, VO_BRMA06_3_TURN1_02, VO_EX1_298_Attack_02, VO_EX1_298_Death_04, VO_EX1_298_Play_01, VO_EX1_298_Trigger_03, VO_BRMA06_1_DEATH_04, VO_BRMA06_1_RESPONSE_03, VO_BRMA06_1_SUMMON_RAG_05,
			VO_BRMA06_1_TURN1_02, VO_BRMA06_1_TURN1_02_ALT, VO_BRMA05_1_CARD_05, VO_BRMA05_1_DEATH_04, VO_BRMA05_1_HERO_POWER_06, VO_BRMA05_1_RESPONSE_03, VO_BRMA05_1_START_01, VO_BRMA05_1_TURN1_02, VO_BRMA04_1_CARD_04, VO_BRMA04_1_HERO_POWER_05,
			VO_BRMA04_1_RESPONSE_03, VO_BRMA04_1_DEATH_06, VO_BRMA04_1_START_01
		};
		SetBossVOLines(soundFiles);
		foreach (string soundFile in soundFiles)
		{
			GameState.Get().GetGameEntity().PreloadSound(soundFile);
		}
	}

	public override void OnCreateGame()
	{
		base.OnCreateGame();
		m_introLine = VO_BRMA06_1_TURN1_02_ALT;
		m_deathLine = VO_BRMA06_1_SUMMON_RAG_05;
	}

	public override IEnumerator RespondToWillPlayCardWithTiming(string cardID, Entity playedEntity)
	{
		while (m_enemySpeaking)
		{
			yield return null;
		}
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_841H");
		Actor guest1bossActor = FindEnemyActorInPlayByDesignCode("LETL_840H");
		Actor guest2bossActor = FindEnemyActorInPlayByDesignCode("LETL_839H");
		Entity entityThatownsThatAbility = playedEntity.GetLettuceAbilityOwner();
		string designCode = entityThatownsThatAbility.GetCardId();
		if (designCode == "LETL_841H")
		{
			if (cardID == "LETL_452_03" || cardID == "LETL_452_05")
			{
				GameState.Get().SetBusy(busy: true);
				yield return MissionPlayVOOnce(bossActor, VO_BRMA06_1_TURN1_02);
				GameState.Get().SetBusy(busy: false);
			}
		}
		if (designCode == "LETL_840H")
		{
			switch (cardID)
			{
			case "LETL_030P2_04":
			case "LETL_030P2_05":
				GameState.Get().SetBusy(busy: true);
				yield return MissionPlayVOOnce(guest1bossActor, VO_BRMA05_1_HERO_POWER_06);
				GameState.Get().SetBusy(busy: false);
				break;
			case "LETL_030P4_04":
			case "LETL_030P4_05":
				GameState.Get().SetBusy(busy: true);
				yield return MissionPlayVOOnce(guest1bossActor, VO_BRMA05_1_RESPONSE_03);
				GameState.Get().SetBusy(busy: false);
				break;
			}
		}
		if (designCode == "LETL_839H")
		{
			switch (cardID)
			{
			case "LETL_839P1_01":
			case "LETL_839P1_03":
				GameState.Get().SetBusy(busy: true);
				yield return MissionPlayVOOnce(guest2bossActor, VO_BRMA04_1_CARD_04);
				GameState.Get().SetBusy(busy: false);
				break;
			case "LETL_839P2_01":
			case "LETL_839P2_03":
				GameState.Get().SetBusy(busy: true);
				yield return MissionPlayVOOnce(guest2bossActor, VO_BRMA04_1_HERO_POWER_05);
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_841H");
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
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_841H");
		Actor guest1bossActor = FindEnemyActorInPlayByDesignCode("LETL_840H");
		Actor guest2bossActor = FindEnemyActorInPlayByDesignCode("LETL_839H");
		Actor guest3bossActor = FindEnemyActorInPlayByDesignCode("LETL_841H2");
		Actor guest4bossActor = FindEnemyActorInPlayByDesignCode("LETL_841H2_Heroic");
		switch (entity.GetCardId())
		{
		case "LETL_841H":
			yield return MissionPlaySound(bossActor, m_deathLine);
			break;
		case "LETL_840H":
			yield return MissionPlaySound(guest1bossActor, VO_BRMA05_1_DEATH_04);
			break;
		case "LETL_839H":
			yield return MissionPlaySound(guest2bossActor, VO_BRMA04_1_DEATH_06);
			break;
		case "LETL_841H2":
			yield return MissionPlaySound(guest3bossActor, VO_EX1_298_Death_04);
			break;
		case "LETL_841H2_Heroic":
			yield return MissionPlaySound(guest4bossActor, VO_EX1_298_Death_04);
			break;
		}
	}

	public override IEnumerator HandleStartOfTurnWithTiming(int turn)
	{
		while (m_enemySpeaking)
		{
			yield return null;
		}
		Actor bossActor = FindEnemyActorInPlayByDesignCode("LETL_841H");
		if (turn == 1)
		{
			yield return MissionPlayVOOnce(bossActor, m_introLine);
		}
	}
}
