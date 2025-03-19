using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BOM_10_Dawngrasp_Fight_007 : BOM_10_Dawngrasp_Dungeon
{
	private static readonly AssetReference VO_BOM_10_007_Female_Draenei_Xyrella_InGame_Turn_03_01_A = new AssetReference("VO_BOM_10_007_Female_Draenei_Xyrella_InGame_Turn_03_01_A.prefab:76aacb60b05c536409f2915f5efd83f9");

	private static readonly AssetReference VO_BOM_10_007_Female_Dragon_UndeadOnyxia_InGame_BossIdle_01 = new AssetReference("VO_BOM_10_007_Female_Dragon_UndeadOnyxia_InGame_BossIdle_01.prefab:e83ad7f6fe8b9884380aa88adc8a219f");

	private static readonly AssetReference VO_BOM_10_007_Female_Dragon_UndeadOnyxia_InGame_BossIdle_02 = new AssetReference("VO_BOM_10_007_Female_Dragon_UndeadOnyxia_InGame_BossIdle_02.prefab:22860d494735fbe4fa6e64edcccfb940");

	private static readonly AssetReference VO_BOM_10_007_Female_Dragon_UndeadOnyxia_InGame_BossIdle_03 = new AssetReference("VO_BOM_10_007_Female_Dragon_UndeadOnyxia_InGame_BossIdle_03.prefab:57d2d32734a93fc49a28a118bf1cdc28");

	private static readonly AssetReference VO_BOM_10_007_Female_Dragon_UndeadOnyxia_InGame_BossUsesHeroPower_01 = new AssetReference("VO_BOM_10_007_Female_Dragon_UndeadOnyxia_InGame_BossUsesHeroPower_01.prefab:cffdc1f7616ab374da2a26f80c8682ec");

	private static readonly AssetReference VO_BOM_10_007_Female_Dragon_UndeadOnyxia_InGame_BossUsesHeroPower_02 = new AssetReference("VO_BOM_10_007_Female_Dragon_UndeadOnyxia_InGame_BossUsesHeroPower_02.prefab:40e09482413d8274c8f2ae02751eea2c");

	private static readonly AssetReference VO_BOM_10_007_Female_Dragon_UndeadOnyxia_InGame_BossUsesHeroPower_03 = new AssetReference("VO_BOM_10_007_Female_Dragon_UndeadOnyxia_InGame_BossUsesHeroPower_03.prefab:36742a66b54887042952143d11beee5c");

	private static readonly AssetReference VO_BOM_10_007_Female_Dragon_UndeadOnyxia_InGame_Death_01 = new AssetReference("VO_BOM_10_007_Female_Dragon_UndeadOnyxia_InGame_Death_01.prefab:7a1741baed0c8654c82e6f4b8dc9e6dd");

	private static readonly AssetReference VO_BOM_10_007_Female_Dragon_UndeadOnyxia_InGame_EmoteResponse_01 = new AssetReference("VO_BOM_10_007_Female_Dragon_UndeadOnyxia_InGame_EmoteResponse_01.prefab:2310394969d7d00428120e98b024af0f");

	private static readonly AssetReference VO_BOM_10_007_Female_Dragon_UndeadOnyxia_InGame_Introduction_01_B = new AssetReference("VO_BOM_10_007_Female_Dragon_UndeadOnyxia_InGame_Introduction_01_B.prefab:60b829133023ea34fad2013eed524bfe");

	private static readonly AssetReference VO_BOM_10_007_Female_Dragon_UndeadOnyxia_InGame_PlayerLoss_01 = new AssetReference("VO_BOM_10_007_Female_Dragon_UndeadOnyxia_InGame_PlayerLoss_01.prefab:5822a66e0dd03bc4cb16352e1529ff41");

	private static readonly AssetReference VO_BOM_10_007_Female_Dragon_UndeadOnyxia_InGame_Turn_03_01_B = new AssetReference("VO_BOM_10_007_Female_Dragon_UndeadOnyxia_InGame_Turn_03_01_B.prefab:a2bfe58fb2dc6fc479d9377fcdaeba8e");

	private static readonly AssetReference VO_BOM_10_007_Female_Dragon_UndeadOnyxia_InGame_Turn_03_01_C = new AssetReference("VO_BOM_10_007_Female_Dragon_UndeadOnyxia_InGame_Turn_03_01_C.prefab:235e1a76eb0d05e41a4ec31bb5359534");

	private static readonly AssetReference VO_BOM_10_007_X_BloodElf_Dawngrasp_InGame_Introduction_01_A = new AssetReference("VO_BOM_10_007_X_BloodElf_Dawngrasp_InGame_Introduction_01_A.prefab:c04c5773e7676f64c92682ae6fbcfa58");

	private static readonly AssetReference VO_BOM_10_007_X_BloodElf_Dawngrasp_InGame_VictoryPostExplosion_01_A = new AssetReference("VO_BOM_10_007_X_BloodElf_Dawngrasp_InGame_VictoryPostExplosion_01_A.prefab:65fc4f6e9899e224e9df82f4fb0f10db");

	private static readonly AssetReference VO_BOM_09_007_Female_Draenei_Xyrella_InGame_VictoryPostExplosion_01_B = new AssetReference("VO_BOM_09_007_Female_Draenei_Xyrella_InGame_VictoryPostExplosion_01_B.prefab:55d9a52465554c34ea02219f1fba09be");

	private static readonly AssetReference VO_BOM_09_007_Female_Draenei_Xyrella_InGame_VictoryPostExplosion_01_C = new AssetReference("VO_BOM_09_007_Female_Draenei_Xyrella_InGame_VictoryPostExplosion_01_C.prefab:961f0f33486c71f4294c807abf42308e");

	private List<string> m_InGame_BossIdleLines = new List<string> { VO_BOM_10_007_Female_Dragon_UndeadOnyxia_InGame_BossIdle_01, VO_BOM_10_007_Female_Dragon_UndeadOnyxia_InGame_BossIdle_02, VO_BOM_10_007_Female_Dragon_UndeadOnyxia_InGame_BossIdle_03 };

	private List<string> m_InGame_BossUsesHeroPowerLines = new List<string> { VO_BOM_10_007_Female_Dragon_UndeadOnyxia_InGame_BossUsesHeroPower_01, VO_BOM_10_007_Female_Dragon_UndeadOnyxia_InGame_BossUsesHeroPower_02, VO_BOM_10_007_Female_Dragon_UndeadOnyxia_InGame_BossUsesHeroPower_03 };

	private HashSet<string> m_playedLines = new HashSet<string>();

	private Notification m_turnCounter;

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> m_SoundFiles = new List<string>
		{
			VO_BOM_10_007_Female_Draenei_Xyrella_InGame_Turn_03_01_A, VO_BOM_10_007_Female_Dragon_UndeadOnyxia_InGame_BossIdle_01, VO_BOM_10_007_Female_Dragon_UndeadOnyxia_InGame_BossIdle_02, VO_BOM_10_007_Female_Dragon_UndeadOnyxia_InGame_BossIdle_03, VO_BOM_10_007_Female_Dragon_UndeadOnyxia_InGame_BossUsesHeroPower_01, VO_BOM_10_007_Female_Dragon_UndeadOnyxia_InGame_BossUsesHeroPower_02, VO_BOM_10_007_Female_Dragon_UndeadOnyxia_InGame_BossUsesHeroPower_03, VO_BOM_10_007_Female_Dragon_UndeadOnyxia_InGame_Death_01, VO_BOM_10_007_Female_Dragon_UndeadOnyxia_InGame_EmoteResponse_01, VO_BOM_10_007_Female_Dragon_UndeadOnyxia_InGame_Introduction_01_B,
			VO_BOM_10_007_Female_Dragon_UndeadOnyxia_InGame_PlayerLoss_01, VO_BOM_10_007_Female_Dragon_UndeadOnyxia_InGame_Turn_03_01_B, VO_BOM_10_007_Female_Dragon_UndeadOnyxia_InGame_Turn_03_01_C, VO_BOM_10_007_X_BloodElf_Dawngrasp_InGame_Introduction_01_A, VO_BOM_10_007_X_BloodElf_Dawngrasp_InGame_VictoryPostExplosion_01_A, VO_BOM_09_007_Female_Draenei_Xyrella_InGame_VictoryPostExplosion_01_B, VO_BOM_09_007_Female_Draenei_Xyrella_InGame_VictoryPostExplosion_01_C
		};
		SetBossVOLines(m_SoundFiles);
		foreach (string soundFile in m_SoundFiles)
		{
			PreloadSound(soundFile);
		}
	}

	public override void OnCreateGame()
	{
		base.OnCreateGame();
	}

	protected override IEnumerator HandleMissionEventWithTiming(int missionEvent)
	{
		while (m_enemySpeaking)
		{
			yield return null;
		}
		Actor enemyActor = GameState.Get().GetOpposingSidePlayer().GetHero()
			.GetCard()
			.GetActor();
		Actor friendlyActor = GameState.Get().GetFriendlySidePlayer().GetHero()
			.GetCard()
			.GetActor();
		switch (missionEvent)
		{
		case 517:
			yield return MissionPlayVO(enemyActor, m_InGame_BossIdleLines);
			break;
		case 510:
			yield return MissionPlayVO(enemyActor, m_InGame_BossUsesHeroPowerLines);
			break;
		case 516:
			yield return MissionPlaySound(enemyActor, VO_BOM_10_007_Female_Dragon_UndeadOnyxia_InGame_Death_01);
			break;
		case 515:
			yield return MissionPlayVO(enemyActor, VO_BOM_10_007_Female_Dragon_UndeadOnyxia_InGame_EmoteResponse_01);
			break;
		case 514:
			yield return MissionPlayVO(friendlyActor, VO_BOM_10_007_X_BloodElf_Dawngrasp_InGame_Introduction_01_A);
			yield return MissionPlayVO(enemyActor, VO_BOM_10_007_Female_Dragon_UndeadOnyxia_InGame_Introduction_01_B);
			break;
		case 506:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(enemyActor, VO_BOM_10_007_Female_Dragon_UndeadOnyxia_InGame_PlayerLoss_01);
			GameState.Get().SetBusy(busy: false);
			break;
		case 505:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(friendlyActor, VO_BOM_10_007_X_BloodElf_Dawngrasp_InGame_VictoryPostExplosion_01_A);
			yield return MissionPlayVO("BOM_10_Xyrella_004t", BOM_10_Dawngrasp_Dungeon.Alterac_XyrellaArt_BrassRing_Quote, VO_BOM_09_007_Female_Draenei_Xyrella_InGame_VictoryPostExplosion_01_B);
			yield return MissionPlayVO("BOM_10_Xyrella_004t", BOM_10_Dawngrasp_Dungeon.Alterac_XyrellaArt_BrassRing_Quote, VO_BOM_09_007_Female_Draenei_Xyrella_InGame_VictoryPostExplosion_01_C);
			GameState.Get().SetBusy(busy: false);
			break;
		default:
			yield return base.HandleMissionEventWithTiming(missionEvent);
			break;
		}
	}

	protected override IEnumerator RespondToFriendlyPlayedCardWithTiming(Entity entity)
	{
		yield return base.RespondToFriendlyPlayedCardWithTiming(entity);
		while (m_enemySpeaking)
		{
			yield return null;
		}
		while (entity.GetCardType() == TAG_CARDTYPE.INVALID)
		{
			yield return null;
		}
		if (!m_playedLines.Contains(entity.GetCardId()) || entity.GetCardType() == TAG_CARDTYPE.HERO_POWER)
		{
			yield return WaitForEntitySoundToFinish(entity);
			string cardID = entity.GetCardId();
			m_playedLines.Add(cardID);
			GameState.Get().GetOpposingSidePlayer().GetHero()
				.GetCard()
				.GetActor();
			GameState.Get().GetFriendlySidePlayer().GetHero()
				.GetCard()
				.GetActor();
		}
	}

	protected override IEnumerator RespondToPlayedCardWithTiming(Entity entity)
	{
		while (m_enemySpeaking)
		{
			yield return null;
		}
		while (entity.GetCardType() == TAG_CARDTYPE.INVALID)
		{
			yield return null;
		}
		if (!m_playedLines.Contains(entity.GetCardId()) || entity.GetCardType() == TAG_CARDTYPE.HERO_POWER)
		{
			yield return base.RespondToPlayedCardWithTiming(entity);
			yield return WaitForEntitySoundToFinish(entity);
			string cardID = entity.GetCardId();
			m_playedLines.Add(cardID);
			Actor enemyActor = GameState.Get().GetOpposingSidePlayer().GetHero()
				.GetCard()
				.GetActor();
			GameState.Get().GetFriendlySidePlayer().GetHero()
				.GetCard()
				.GetActor();
			if (cardID == "")
			{
				yield return MissionPlayVO(enemyActor, VO_BOM_10_007_Female_Dragon_UndeadOnyxia_InGame_Turn_03_01_C);
			}
		}
	}

	protected override IEnumerator HandleStartOfTurnWithTiming(int turn)
	{
		while (m_enemySpeaking)
		{
			yield return null;
		}
		Actor enemyActor = GameState.Get().GetOpposingSidePlayer().GetHero()
			.GetCard()
			.GetActor();
		GameState.Get().GetFriendlySidePlayer().GetHero()
			.GetCard()
			.GetActor();
		if (turn == 3)
		{
			yield return MissionPlayVO("BOM_10_Xyrella_004t", BOM_10_Dawngrasp_Dungeon.Alterac_XyrellaArt_BrassRing_Quote, VO_BOM_10_007_Female_Draenei_Xyrella_InGame_Turn_03_01_A);
			yield return MissionPlayVO(enemyActor, VO_BOM_10_007_Female_Dragon_UndeadOnyxia_InGame_Turn_03_01_B);
			yield return MissionPlayVO(enemyActor, VO_BOM_10_007_Female_Dragon_UndeadOnyxia_InGame_Turn_03_01_C);
		}
	}

	public override void NotifyOfMulliganEnded()
	{
		base.NotifyOfMulliganEnded();
		InitVisuals();
	}

	private void InitVisuals()
	{
		int cost = GetCost();
		InitTurnCounter(cost);
	}

	public override void OnTagChanged(TagDelta change)
	{
		base.OnTagChanged(change);
		if (change.tag == 48 && change.newValue != change.oldValue)
		{
			UpdateVisuals(change.newValue);
		}
	}

	private void InitTurnCounter(int cost)
	{
		GameObject turnCounterGo = AssetLoader.Get().InstantiatePrefab("LOE_Turn_Timer.prefab:b05530aa55868554fb8f0c66632b3c22");
		m_turnCounter = turnCounterGo.GetComponent<Notification>();
		PlayMakerFSM component = m_turnCounter.GetComponent<PlayMakerFSM>();
		component.FsmVariables.GetFsmBool("RunningMan").Value = true;
		component.FsmVariables.GetFsmBool("MineCart").Value = false;
		component.FsmVariables.GetFsmBool("Airship").Value = false;
		component.FsmVariables.GetFsmBool("Destroyer").Value = false;
		component.SendEvent("Birth");
		Actor enemyActor = GameState.Get().GetOpposingSidePlayer().GetHeroCard()
			.GetActor();
		m_turnCounter.transform.parent = enemyActor.gameObject.transform;
		m_turnCounter.transform.localPosition = new Vector3(-1.4f, 0.187f, -0.11f);
		m_turnCounter.transform.localScale = Vector3.one * 0.52f;
		UpdateTurnCounterText(cost);
	}

	private void UpdateVisuals(int cost)
	{
		UpdateTurnCounter(cost);
	}

	private void UpdateTurnCounter(int cost)
	{
		m_turnCounter.GetComponent<PlayMakerFSM>().SendEvent("Action");
		if (cost <= 0)
		{
			Object.Destroy(m_turnCounter.gameObject);
		}
		else
		{
			UpdateTurnCounterText(cost);
		}
	}

	private void UpdateTurnCounterText(int cost)
	{
		GameStrings.PluralNumber[] pluralNumbers = new GameStrings.PluralNumber[1]
		{
			new GameStrings.PluralNumber
			{
				m_index = 0,
				m_number = cost
			}
		};
		string counterName = GameStrings.FormatPlurals("BOM_DAWNGRASP_07", pluralNumbers);
		m_turnCounter.ChangeDialogText(counterName, cost.ToString(), "", "");
	}
}
