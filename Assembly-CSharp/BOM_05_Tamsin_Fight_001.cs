using System.Collections;
using System.Collections.Generic;

public class BOM_05_Tamsin_Fight_001 : BOM_05_Tamsin_Dungeon
{
	private static readonly AssetReference VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Tamsin_Mission1Death_01 = new AssetReference("VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Tamsin_Mission1Death_01.prefab:cf75f493a8c84b23ad40fff172f7a677");

	private static readonly AssetReference VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Tamsin_Mission1EmoteResponse_01 = new AssetReference("VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Tamsin_Mission1EmoteResponse_01.prefab:1ce727aadb0241779941d055cf862aba");

	private static readonly AssetReference VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Tamsin_Mission1ExchangeA_01 = new AssetReference("VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Tamsin_Mission1ExchangeA_01.prefab:5583aae8f04a40ebaea5e69327cc291d");

	private static readonly AssetReference VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Tamsin_Mission1ExchangeB_01 = new AssetReference("VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Tamsin_Mission1ExchangeB_01.prefab:6c7d1e10a36241b695f4258766d07712");

	private static readonly AssetReference VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Tamsin_Mission1ExchangeD_02 = new AssetReference("VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Tamsin_Mission1ExchangeD_02.prefab:df298a453a684cb39376eda87525006b");

	private static readonly AssetReference VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Tamsin_Mission1HeroPower_01 = new AssetReference("VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Tamsin_Mission1HeroPower_01.prefab:781c4205c1c049feb3275c3babea4c7b");

	private static readonly AssetReference VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Tamsin_Mission1HeroPower_02 = new AssetReference("VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Tamsin_Mission1HeroPower_02.prefab:cb649e9d2cd344b3a544ab808fa1d2c3");

	private static readonly AssetReference VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Tamsin_Mission1HeroPower_03 = new AssetReference("VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Tamsin_Mission1HeroPower_03.prefab:7b185b378a5a401c8c5298cf5935406c");

	private static readonly AssetReference VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Tamsin_Mission1Idle_01 = new AssetReference("VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Tamsin_Mission1Idle_01.prefab:31d56046b64e4c1c84268ab1b9ddb1cb");

	private static readonly AssetReference VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Tamsin_Mission1Idle_03 = new AssetReference("VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Tamsin_Mission1Idle_03.prefab:a13dc05bd97c49559a26a860232b9030");

	private static readonly AssetReference VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Tamsin_Mission1Intro_01 = new AssetReference("VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Tamsin_Mission1Intro_01.prefab:ec1a3f0af8bb4d4389cf99f24597b89c");

	private static readonly AssetReference VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Tamsin_Mission1Loss_01 = new AssetReference("VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Tamsin_Mission1Loss_01.prefab:2a068840219948dd99b1398a08a4aba9");

	private static readonly AssetReference VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission1ExchangeA_02 = new AssetReference("VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission1ExchangeA_02.prefab:a521cbd2fc304c8c92a765aaa7ea89c5");

	private static readonly AssetReference VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission1ExchangeB_02 = new AssetReference("VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission1ExchangeB_02.prefab:5266125027d5420680c2d7561d45b84e");

	private static readonly AssetReference VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission1ExchangeD_01 = new AssetReference("VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission1ExchangeD_01.prefab:2b34003aa4b14464966d17011469675d");

	private static readonly AssetReference VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission1Intro_02 = new AssetReference("VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission1Intro_02.prefab:ae52dc2ceb7c4f82912e70c603b5110b");

	private static readonly AssetReference VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission8ExchangeH_01 = new AssetReference("VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission8ExchangeH_01.prefab:7bf442a3d85d0d5439cffe9e9ebc48b7");

	private static readonly AssetReference VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Tamsin_Mission1Idle_02 = new AssetReference("VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Tamsin_Mission1Idle_02.prefab:80c14575236681e43814d82594e2f889");

	private static readonly AssetReference VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Tamsin_Mission1Victory_02 = new AssetReference("VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Tamsin_Mission1Victory_02.prefab:895c68aa9b7e0e84790857a4b84c112a");

	private static readonly AssetReference VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission1ExchangeC_02 = new AssetReference("VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission1ExchangeC_02.prefab:538e1afd878011e4a989dc002d4139a5");

	private static readonly AssetReference VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission1Victory_01 = new AssetReference("VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission1Victory_01.prefab:c8f9d3eac0ad4da4fa93cf64e257a299");

	private static readonly AssetReference VO_Story_Hero_Tamsin_Female_Forsaken_Story_Kurtrus_Mission8ExchangeH_02 = new AssetReference("VO_Story_Hero_Tamsin_Female_Forsaken_Story_Kurtrus_Mission8ExchangeH_02.prefab:2bfe81262b54212408ee97ed5d03107f");

	private static readonly AssetReference VO_Story_Hero_Tamsin_Female_Forsaken_Story_Kurtrus_Mission8ExchangeN_01 = new AssetReference("VO_Story_Hero_Tamsin_Female_Forsaken_Story_Kurtrus_Mission8ExchangeN_01.prefab:5ad1f252e7dd58143bfc9c7360cb7e15");

	private static readonly AssetReference VO_Story_Hero_Tamsin_Female_Forsaken_Story_Kurtrus_Mission8ExchangeO_01 = new AssetReference("VO_Story_Hero_Tamsin_Female_Forsaken_Story_Kurtrus_Mission8ExchangeO_01.prefab:22afab267083eb54d986ab1d24c70793");

	private static readonly AssetReference VO_Story_Hero_Tamsin_Female_Forsaken_Story_Kurtrus_Mission8ExchangeP_01 = new AssetReference("VO_Story_Hero_Tamsin_Female_Forsaken_Story_Kurtrus_Mission8ExchangeP_01.prefab:ffc66a494a7fe8748a7ac5e9bb2fa912");

	private static readonly AssetReference VO_PVPDR_Hero_Tamsin_Female_Forsaken_Death_01 = new AssetReference("VO_PVPDR_Hero_Tamsin_Female_Forsaken_Death_01.prefab:0261caf1dfbfae146bf927a03851b086");

	private List<string> m_InGame_BossUsesHeroPowerLines = new List<string> { VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Tamsin_Mission1HeroPower_01, VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Tamsin_Mission1HeroPower_02, VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Tamsin_Mission1HeroPower_03 };

	private List<string> m_InGame_BossIdleLines = new List<string> { VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Tamsin_Mission1Idle_01, VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Tamsin_Mission1Idle_03 };

	private HashSet<string> m_playedLines = new HashSet<string>();

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> m_SoundFiles = new List<string>
		{
			VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Tamsin_Mission1Death_01, VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Tamsin_Mission1EmoteResponse_01, VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Tamsin_Mission1ExchangeA_01, VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Tamsin_Mission1ExchangeB_01, VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission8ExchangeH_01, VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Tamsin_Mission1ExchangeD_02, VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Tamsin_Mission1HeroPower_01, VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Tamsin_Mission1HeroPower_02, VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Tamsin_Mission1HeroPower_03, VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Tamsin_Mission1Idle_01,
			VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Tamsin_Mission1Idle_02, VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Tamsin_Mission1Idle_03, VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Tamsin_Mission1Intro_01, VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Tamsin_Mission1Loss_01, VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Tamsin_Mission1Victory_02, VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission1ExchangeA_02, VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission1ExchangeB_02, VO_Story_Hero_Tamsin_Female_Forsaken_Story_Kurtrus_Mission8ExchangeH_02, VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission1ExchangeD_01, VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission1Intro_02,
			VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission1Victory_01, VO_Story_Hero_Tamsin_Female_Forsaken_Story_Kurtrus_Mission8ExchangeN_01, VO_Story_Hero_Tamsin_Female_Forsaken_Story_Kurtrus_Mission8ExchangeO_01, VO_Story_Hero_Tamsin_Female_Forsaken_Story_Kurtrus_Mission8ExchangeP_01, VO_PVPDR_Hero_Tamsin_Female_Forsaken_Death_01
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
		case 516:
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Tamsin_Mission1Death_01);
			break;
		case 519:
			yield return MissionPlaySound(friendlyActor, VO_PVPDR_Hero_Tamsin_Female_Forsaken_Death_01);
			break;
		case 517:
			yield return MissionPlayVO(enemyActor, m_InGame_BossIdleLines);
			break;
		case 510:
			yield return MissionPlayVO(enemyActor, m_InGame_BossUsesHeroPowerLines);
			break;
		case 515:
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Tamsin_Mission1EmoteResponse_01);
			break;
		case 514:
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Tamsin_Mission1Intro_01);
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission1Intro_02);
			break;
		case 507:
			MissionPause(pause: true);
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Tamsin_Mission1Loss_01);
			MissionPause(pause: false);
			break;
		case 504:
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission1Victory_01);
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Tamsin_Mission1Victory_02);
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
			GameState.Get().GetOpposingSidePlayer().GetHero()
				.GetCard()
				.GetActor();
			Actor friendlyActor = GameState.Get().GetFriendlySidePlayer().GetHero()
				.GetCard()
				.GetActor();
			switch (cardID)
			{
			case "BAR_914":
			case "BAR_914t":
			case "BAR_914t2":
				yield return MissionPlayVOOnce(friendlyActor, VO_Story_Hero_Tamsin_Female_Forsaken_Story_Kurtrus_Mission8ExchangeN_01);
				break;
			case "BAR_910":
				yield return MissionPlayVOOnce(friendlyActor, VO_Story_Hero_Tamsin_Female_Forsaken_Story_Kurtrus_Mission8ExchangeO_01);
				break;
			case "BAR_911":
			case "EX1_312":
			case "CORE_EX1_312":
			case "VAN_EX1_312":
				yield return MissionPlayVOOnce(friendlyActor, VO_Story_Hero_Tamsin_Female_Forsaken_Story_Kurtrus_Mission8ExchangeP_01);
				break;
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
		Actor friendlyActor = GameState.Get().GetFriendlySidePlayer().GetHero()
			.GetCard()
			.GetActor();
		switch (turn)
		{
		case 3:
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Tamsin_Mission1ExchangeA_01);
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission1ExchangeA_02);
			break;
		case 7:
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Tamsin_Mission1ExchangeB_01);
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission1ExchangeB_02);
			break;
		case 11:
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission8ExchangeH_01);
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Tamsin_Female_Forsaken_Story_Kurtrus_Mission8ExchangeH_02);
			break;
		case 15:
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission1ExchangeD_01);
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Tamsin_Mission1ExchangeD_02);
			break;
		}
	}
}
