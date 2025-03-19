using System.Collections;
using System.Collections.Generic;

public class BOM_04_Kurtrus_Fight_08 : BOM_04_Kurtrus_Dungeon
{
	private static readonly AssetReference VO_Story_Hero_Cariel_Female_Human_Story_Kurtrus_Mission8ExchangeA_01 = new AssetReference("VO_Story_Hero_Cariel_Female_Human_Story_Kurtrus_Mission8ExchangeA_01.prefab:772f6dfc635d0684a935d4b837fce7a4");

	private static readonly AssetReference VO_Story_Hero_Cariel_Female_Human_Story_Kurtrus_Mission8ExchangeB_02 = new AssetReference("VO_Story_Hero_Cariel_Female_Human_Story_Kurtrus_Mission8ExchangeB_02.prefab:bcd6fbb46309efb49be27caf7d9eb9fe");

	private static readonly AssetReference VO_Story_Hero_Cariel_Female_Human_Story_Kurtrus_Mission8ExchangeC_01 = new AssetReference("VO_Story_Hero_Cariel_Female_Human_Story_Kurtrus_Mission8ExchangeC_01.prefab:d483ccf7589af404084da45765091f08");

	private static readonly AssetReference VO_Story_Hero_Cariel_Female_Human_Story_Kurtrus_Mission8ExchangeD_01 = new AssetReference("VO_Story_Hero_Cariel_Female_Human_Story_Kurtrus_Mission8ExchangeD_01.prefab:a9fb4490210dc2447916b4bd4bdc6fd5");

	private static readonly AssetReference VO_Story_Hero_Cariel_Female_Human_Story_Kurtrus_Mission8ExchangeE_02 = new AssetReference("VO_Story_Hero_Cariel_Female_Human_Story_Kurtrus_Mission8ExchangeE_02.prefab:37e41ef1ffe1abd4d8aa7d6d9de51304");

	private static readonly AssetReference VO_Story_Hero_Cariel_Female_Human_Story_Kurtrus_Mission8ExchangeF_02 = new AssetReference("VO_Story_Hero_Cariel_Female_Human_Story_Kurtrus_Mission8ExchangeF_02.prefab:bd3906da5edd6ef4baea3f34b6155803");

	private static readonly AssetReference VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission8ExchangeB_01 = new AssetReference("VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission8ExchangeB_01.prefab:231b141d315987a41960d9daf3f74683");

	private static readonly AssetReference VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission8ExchangeB_03 = new AssetReference("VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission8ExchangeB_03.prefab:d8a295dcfa4abfb4fa9d1ac220f7adf8");

	private static readonly AssetReference VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission8ExchangeE_01 = new AssetReference("VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission8ExchangeE_01.prefab:db01eead688c93f468c9a5f3f2ac832c");

	private static readonly AssetReference VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission8ExchangeF_01 = new AssetReference("VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission8ExchangeF_01.prefab:93a306cb289f66942ade201b806c03c1");

	private static readonly AssetReference VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission8ExchangeG_01 = new AssetReference("VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission8ExchangeG_01.prefab:1a705edea336e344dbb7bdcb5221db2e");

	private static readonly AssetReference VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission8ExchangeH_01 = new AssetReference("VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission8ExchangeH_01.prefab:7bf442a3d85d0d5439cffe9e9ebc48b7");

	private static readonly AssetReference VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission8Intro_02 = new AssetReference("VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission8Intro_02.prefab:aeb7d7d5f87c91a44ac2b7fe7925be9f");

	private static readonly AssetReference VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission8Victory_01 = new AssetReference("VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission8Victory_01.prefab:18c7ca5fd5919ab4091db0c734d8c17d");

	private static readonly AssetReference VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission8Victory_03 = new AssetReference("VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission8Victory_03.prefab:3dd91008c9f04d248b3926e1579f4edc");

	private static readonly AssetReference VO_Story_Hero_Tamsin_Female_Forsaken_Story_Kurtrus_Mission8Death_01 = new AssetReference("VO_Story_Hero_Tamsin_Female_Forsaken_Story_Kurtrus_Mission8Death_01.prefab:be16c8fb815d67b4f841cc9cd2d0667b");

	private static readonly AssetReference VO_Story_Hero_Tamsin_Female_Forsaken_Story_Kurtrus_Mission8EmoteResponse_01 = new AssetReference("VO_Story_Hero_Tamsin_Female_Forsaken_Story_Kurtrus_Mission8EmoteResponse_01.prefab:4fe4edb505515f3439bc904375535c51");

	private static readonly AssetReference VO_Story_Hero_Tamsin_Female_Forsaken_Story_Kurtrus_Mission8ExchangeA_02 = new AssetReference("VO_Story_Hero_Tamsin_Female_Forsaken_Story_Kurtrus_Mission8ExchangeA_02.prefab:29353c3e4920d4845ae9e16079f7cf06");

	private static readonly AssetReference VO_Story_Hero_Tamsin_Female_Forsaken_Story_Kurtrus_Mission8ExchangeC_02 = new AssetReference("VO_Story_Hero_Tamsin_Female_Forsaken_Story_Kurtrus_Mission8ExchangeC_02.prefab:cec263ff2242ae042b6b6cc7f4c77526");

	private static readonly AssetReference VO_Story_Hero_Tamsin_Female_Forsaken_Story_Kurtrus_Mission8ExchangeD_02 = new AssetReference("VO_Story_Hero_Tamsin_Female_Forsaken_Story_Kurtrus_Mission8ExchangeD_02.prefab:f5d949c17feae2b409ca956cf5f85bb3");

	private static readonly AssetReference VO_Story_Hero_Tamsin_Female_Forsaken_Story_Kurtrus_Mission8ExchangeE_03 = new AssetReference("VO_Story_Hero_Tamsin_Female_Forsaken_Story_Kurtrus_Mission8ExchangeE_03.prefab:7bb9f2404acba5a49b84246cc907b14a");

	private static readonly AssetReference VO_Story_Hero_Tamsin_Female_Forsaken_Story_Kurtrus_Mission8ExchangeF_03 = new AssetReference("VO_Story_Hero_Tamsin_Female_Forsaken_Story_Kurtrus_Mission8ExchangeF_03.prefab:5430a1d1ac6cb85468038cdf1c4e3e12");

	private static readonly AssetReference VO_Story_Hero_Tamsin_Female_Forsaken_Story_Kurtrus_Mission8ExchangeG_02 = new AssetReference("VO_Story_Hero_Tamsin_Female_Forsaken_Story_Kurtrus_Mission8ExchangeG_02.prefab:8a18eb7b6edae29499f44a66d9516f14");

	private static readonly AssetReference VO_Story_Hero_Tamsin_Female_Forsaken_Story_Kurtrus_Mission8ExchangeH_02 = new AssetReference("VO_Story_Hero_Tamsin_Female_Forsaken_Story_Kurtrus_Mission8ExchangeH_02.prefab:2bfe81262b54212408ee97ed5d03107f");

	private static readonly AssetReference VO_Story_Hero_Tamsin_Female_Forsaken_Story_Kurtrus_Mission8ExchangeJ_01 = new AssetReference("VO_Story_Hero_Tamsin_Female_Forsaken_Story_Kurtrus_Mission8ExchangeJ_01.prefab:9554b5216db3d724284b2805a5224631");

	private static readonly AssetReference VO_Story_Hero_Tamsin_Female_Forsaken_Story_Kurtrus_Mission8ExchangeK_01 = new AssetReference("VO_Story_Hero_Tamsin_Female_Forsaken_Story_Kurtrus_Mission8ExchangeK_01.prefab:6f09953ee47765742a9b946a8bbe5521");

	private static readonly AssetReference VO_Story_Hero_Tamsin_Female_Forsaken_Story_Kurtrus_Mission8ExchangeL_01 = new AssetReference("VO_Story_Hero_Tamsin_Female_Forsaken_Story_Kurtrus_Mission8ExchangeL_01.prefab:9ef887f7b29ebb2498ab832ddb274a61");

	private static readonly AssetReference VO_Story_Hero_Tamsin_Female_Forsaken_Story_Kurtrus_Mission8ExchangeM_01 = new AssetReference("VO_Story_Hero_Tamsin_Female_Forsaken_Story_Kurtrus_Mission8ExchangeM_01.prefab:bd8b7190e93c82d4f9b9890b453895d8");

	private static readonly AssetReference VO_Story_Hero_Tamsin_Female_Forsaken_Story_Kurtrus_Mission8ExchangeN_01 = new AssetReference("VO_Story_Hero_Tamsin_Female_Forsaken_Story_Kurtrus_Mission8ExchangeN_01.prefab:5ad1f252e7dd58143bfc9c7360cb7e15");

	private static readonly AssetReference VO_Story_Hero_Tamsin_Female_Forsaken_Story_Kurtrus_Mission8ExchangeO_01 = new AssetReference("VO_Story_Hero_Tamsin_Female_Forsaken_Story_Kurtrus_Mission8ExchangeO_01.prefab:22afab267083eb54d986ab1d24c70793");

	private static readonly AssetReference VO_Story_Hero_Tamsin_Female_Forsaken_Story_Kurtrus_Mission8ExchangeP_01 = new AssetReference("VO_Story_Hero_Tamsin_Female_Forsaken_Story_Kurtrus_Mission8ExchangeP_01.prefab:ffc66a494a7fe8748a7ac5e9bb2fa912");

	private static readonly AssetReference VO_Story_Hero_Tamsin_Female_Forsaken_Story_Kurtrus_Mission8HeroPower_01 = new AssetReference("VO_Story_Hero_Tamsin_Female_Forsaken_Story_Kurtrus_Mission8HeroPower_01.prefab:49dfd1f7f63aeee4297044fed1413f61");

	private static readonly AssetReference VO_Story_Hero_Tamsin_Female_Forsaken_Story_Kurtrus_Mission8HeroPower_02 = new AssetReference("VO_Story_Hero_Tamsin_Female_Forsaken_Story_Kurtrus_Mission8HeroPower_02.prefab:e148f4e183d2da4449db4e6ad47e4510");

	private static readonly AssetReference VO_Story_Hero_Tamsin_Female_Forsaken_Story_Kurtrus_Mission8HeroPower_03 = new AssetReference("VO_Story_Hero_Tamsin_Female_Forsaken_Story_Kurtrus_Mission8HeroPower_03.prefab:89104f5f2d03c0e489446252fec05048");

	private static readonly AssetReference VO_Story_Hero_Tamsin_Female_Forsaken_Story_Kurtrus_Mission8HeroPower_04 = new AssetReference("VO_Story_Hero_Tamsin_Female_Forsaken_Story_Kurtrus_Mission8HeroPower_04.prefab:6caceff817124df43ab646c83632c594");

	private static readonly AssetReference VO_Story_Hero_Tamsin_Female_Forsaken_Story_Kurtrus_Mission8HeroPower_05 = new AssetReference("VO_Story_Hero_Tamsin_Female_Forsaken_Story_Kurtrus_Mission8HeroPower_05.prefab:400d258b95f05bb408472d6b98b7b559");

	private static readonly AssetReference VO_Story_Hero_Tamsin_Female_Forsaken_Story_Kurtrus_Mission8HeroPower_06 = new AssetReference("VO_Story_Hero_Tamsin_Female_Forsaken_Story_Kurtrus_Mission8HeroPower_06.prefab:51241a0fb31a62a49990bf7a8e3206bd");

	private static readonly AssetReference VO_Story_Hero_Tamsin_Female_Forsaken_Story_Kurtrus_Mission8HeroPower_07 = new AssetReference("VO_Story_Hero_Tamsin_Female_Forsaken_Story_Kurtrus_Mission8HeroPower_07.prefab:22d448ddfdba9e545b77297956e2c235");

	private static readonly AssetReference VO_Story_Hero_Tamsin_Female_Forsaken_Story_Kurtrus_Mission8HeroPower_08 = new AssetReference("VO_Story_Hero_Tamsin_Female_Forsaken_Story_Kurtrus_Mission8HeroPower_08.prefab:2c4619795bff1be45b081dfa3968a78b");

	private static readonly AssetReference VO_Story_Hero_Tamsin_Female_Forsaken_Story_Kurtrus_Mission8HeroPower_09 = new AssetReference("VO_Story_Hero_Tamsin_Female_Forsaken_Story_Kurtrus_Mission8HeroPower_09.prefab:fae02a384a3f9d54ebbf81b3b1afe118");

	private static readonly AssetReference VO_Story_Hero_Tamsin_Female_Forsaken_Story_Kurtrus_Mission8HeroPower_10 = new AssetReference("VO_Story_Hero_Tamsin_Female_Forsaken_Story_Kurtrus_Mission8HeroPower_10.prefab:76a60f942c23bb749a1439e8e5165d09");

	private static readonly AssetReference VO_Story_Hero_Tamsin_Female_Forsaken_Story_Kurtrus_Mission8Idle_01 = new AssetReference("VO_Story_Hero_Tamsin_Female_Forsaken_Story_Kurtrus_Mission8Idle_01.prefab:974a96151033c44449a5e691f6584cc5");

	private static readonly AssetReference VO_Story_Hero_Tamsin_Female_Forsaken_Story_Kurtrus_Mission8Idle_02 = new AssetReference("VO_Story_Hero_Tamsin_Female_Forsaken_Story_Kurtrus_Mission8Idle_02.prefab:38ae3624a4e357944bc6f2db6a6d827a");

	private static readonly AssetReference VO_Story_Hero_Tamsin_Female_Forsaken_Story_Kurtrus_Mission8Idle_03 = new AssetReference("VO_Story_Hero_Tamsin_Female_Forsaken_Story_Kurtrus_Mission8Idle_03.prefab:c757fdb84a8b29f41993eab3565d3242");

	private static readonly AssetReference VO_Story_Hero_Tamsin_Female_Forsaken_Story_Kurtrus_Mission8Idle_04 = new AssetReference("VO_Story_Hero_Tamsin_Female_Forsaken_Story_Kurtrus_Mission8Idle_04.prefab:688adbaa200d8f94fbb226a2d5de9c52");

	private static readonly AssetReference VO_Story_Hero_Tamsin_Female_Forsaken_Story_Kurtrus_Mission8Idle_05 = new AssetReference("VO_Story_Hero_Tamsin_Female_Forsaken_Story_Kurtrus_Mission8Idle_05.prefab:851f6f1edbb190a498f836c9b775fe2f");

	private static readonly AssetReference VO_Story_Hero_Tamsin_Female_Forsaken_Story_Kurtrus_Mission8Idle_06 = new AssetReference("VO_Story_Hero_Tamsin_Female_Forsaken_Story_Kurtrus_Mission8Idle_06.prefab:67f986152f91bd94294f9b3598f54f4b");

	private static readonly AssetReference VO_Story_Hero_Tamsin_Female_Forsaken_Story_Kurtrus_Mission8Intro_01 = new AssetReference("VO_Story_Hero_Tamsin_Female_Forsaken_Story_Kurtrus_Mission8Intro_01.prefab:908ee97a4cc08cf41b1d42132e7f1872");

	private static readonly AssetReference VO_Story_Hero_Tamsin_Female_Forsaken_Story_Kurtrus_Mission8Loss_01 = new AssetReference("VO_Story_Hero_Tamsin_Female_Forsaken_Story_Kurtrus_Mission8Loss_01.prefab:740c313900f7e1342a5e3707b59fa418");

	private static readonly AssetReference VO_Story_Hero_Tamsin_Female_Forsaken_Story_Kurtrus_Mission8Victory_02 = new AssetReference("VO_Story_Hero_Tamsin_Female_Forsaken_Story_Kurtrus_Mission8Victory_02.prefab:c103b014b9f4f0b4bb71e6c58fd5b2c2");

	private List<string> m_VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission8ExchangeBLines = new List<string> { VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission8ExchangeB_01, VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission8ExchangeB_03 };

	private List<string> m_missionEventTriggerInGame_VictoryPreExplosionLines = new List<string> { VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission8Victory_01, VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission8Victory_03, VO_Story_Hero_Tamsin_Female_Forsaken_Story_Kurtrus_Mission8Victory_02 };

	private List<string> m_InGame_BossIdleLines = new List<string> { VO_Story_Hero_Tamsin_Female_Forsaken_Story_Kurtrus_Mission8Idle_01, VO_Story_Hero_Tamsin_Female_Forsaken_Story_Kurtrus_Mission8Idle_02, VO_Story_Hero_Tamsin_Female_Forsaken_Story_Kurtrus_Mission8Idle_03, VO_Story_Hero_Tamsin_Female_Forsaken_Story_Kurtrus_Mission8Idle_04, VO_Story_Hero_Tamsin_Female_Forsaken_Story_Kurtrus_Mission8Idle_05, VO_Story_Hero_Tamsin_Female_Forsaken_Story_Kurtrus_Mission8Idle_06 };

	private List<string> m_InGame_BossHeroPowerLines = new List<string> { VO_Story_Hero_Tamsin_Female_Forsaken_Story_Kurtrus_Mission8HeroPower_01, VO_Story_Hero_Tamsin_Female_Forsaken_Story_Kurtrus_Mission8HeroPower_02, VO_Story_Hero_Tamsin_Female_Forsaken_Story_Kurtrus_Mission8HeroPower_03, VO_Story_Hero_Tamsin_Female_Forsaken_Story_Kurtrus_Mission8HeroPower_04, VO_Story_Hero_Tamsin_Female_Forsaken_Story_Kurtrus_Mission8HeroPower_05, VO_Story_Hero_Tamsin_Female_Forsaken_Story_Kurtrus_Mission8HeroPower_06, VO_Story_Hero_Tamsin_Female_Forsaken_Story_Kurtrus_Mission8HeroPower_07, VO_Story_Hero_Tamsin_Female_Forsaken_Story_Kurtrus_Mission8HeroPower_08, VO_Story_Hero_Tamsin_Female_Forsaken_Story_Kurtrus_Mission8HeroPower_09, VO_Story_Hero_Tamsin_Female_Forsaken_Story_Kurtrus_Mission8HeroPower_10 };

	private HashSet<string> m_playedLines = new HashSet<string>();

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> m_SoundFiles = new List<string>
		{
			VO_Story_Hero_Cariel_Female_Human_Story_Kurtrus_Mission8ExchangeA_01, VO_Story_Hero_Cariel_Female_Human_Story_Kurtrus_Mission8ExchangeB_02, VO_Story_Hero_Cariel_Female_Human_Story_Kurtrus_Mission8ExchangeC_01, VO_Story_Hero_Cariel_Female_Human_Story_Kurtrus_Mission8ExchangeD_01, VO_Story_Hero_Cariel_Female_Human_Story_Kurtrus_Mission8ExchangeE_02, VO_Story_Hero_Cariel_Female_Human_Story_Kurtrus_Mission8ExchangeF_02, VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission8ExchangeB_01, VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission8ExchangeB_03, VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission8ExchangeE_01, VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission8ExchangeF_01,
			VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission8ExchangeG_01, VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission8ExchangeH_01, VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission8Intro_02, VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission8Victory_01, VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission8Victory_03, VO_Story_Hero_Tamsin_Female_Forsaken_Story_Kurtrus_Mission8Death_01, VO_Story_Hero_Tamsin_Female_Forsaken_Story_Kurtrus_Mission8EmoteResponse_01, VO_Story_Hero_Tamsin_Female_Forsaken_Story_Kurtrus_Mission8ExchangeA_02, VO_Story_Hero_Tamsin_Female_Forsaken_Story_Kurtrus_Mission8ExchangeC_02, VO_Story_Hero_Tamsin_Female_Forsaken_Story_Kurtrus_Mission8ExchangeD_02,
			VO_Story_Hero_Tamsin_Female_Forsaken_Story_Kurtrus_Mission8ExchangeE_03, VO_Story_Hero_Tamsin_Female_Forsaken_Story_Kurtrus_Mission8ExchangeF_03, VO_Story_Hero_Tamsin_Female_Forsaken_Story_Kurtrus_Mission8ExchangeG_02, VO_Story_Hero_Tamsin_Female_Forsaken_Story_Kurtrus_Mission8ExchangeH_02, VO_Story_Hero_Tamsin_Female_Forsaken_Story_Kurtrus_Mission8ExchangeJ_01, VO_Story_Hero_Tamsin_Female_Forsaken_Story_Kurtrus_Mission8ExchangeK_01, VO_Story_Hero_Tamsin_Female_Forsaken_Story_Kurtrus_Mission8ExchangeL_01, VO_Story_Hero_Tamsin_Female_Forsaken_Story_Kurtrus_Mission8ExchangeM_01, VO_Story_Hero_Tamsin_Female_Forsaken_Story_Kurtrus_Mission8ExchangeN_01, VO_Story_Hero_Tamsin_Female_Forsaken_Story_Kurtrus_Mission8ExchangeO_01,
			VO_Story_Hero_Tamsin_Female_Forsaken_Story_Kurtrus_Mission8ExchangeP_01, VO_Story_Hero_Tamsin_Female_Forsaken_Story_Kurtrus_Mission8HeroPower_01, VO_Story_Hero_Tamsin_Female_Forsaken_Story_Kurtrus_Mission8HeroPower_02, VO_Story_Hero_Tamsin_Female_Forsaken_Story_Kurtrus_Mission8HeroPower_03, VO_Story_Hero_Tamsin_Female_Forsaken_Story_Kurtrus_Mission8HeroPower_04, VO_Story_Hero_Tamsin_Female_Forsaken_Story_Kurtrus_Mission8HeroPower_05, VO_Story_Hero_Tamsin_Female_Forsaken_Story_Kurtrus_Mission8HeroPower_06, VO_Story_Hero_Tamsin_Female_Forsaken_Story_Kurtrus_Mission8HeroPower_07, VO_Story_Hero_Tamsin_Female_Forsaken_Story_Kurtrus_Mission8HeroPower_08, VO_Story_Hero_Tamsin_Female_Forsaken_Story_Kurtrus_Mission8HeroPower_09,
			VO_Story_Hero_Tamsin_Female_Forsaken_Story_Kurtrus_Mission8HeroPower_10, VO_Story_Hero_Tamsin_Female_Forsaken_Story_Kurtrus_Mission8Idle_01, VO_Story_Hero_Tamsin_Female_Forsaken_Story_Kurtrus_Mission8Idle_02, VO_Story_Hero_Tamsin_Female_Forsaken_Story_Kurtrus_Mission8Idle_03, VO_Story_Hero_Tamsin_Female_Forsaken_Story_Kurtrus_Mission8Idle_04, VO_Story_Hero_Tamsin_Female_Forsaken_Story_Kurtrus_Mission8Idle_05, VO_Story_Hero_Tamsin_Female_Forsaken_Story_Kurtrus_Mission8Idle_06, VO_Story_Hero_Tamsin_Female_Forsaken_Story_Kurtrus_Mission8Intro_01, VO_Story_Hero_Tamsin_Female_Forsaken_Story_Kurtrus_Mission8Loss_01, VO_Story_Hero_Tamsin_Female_Forsaken_Story_Kurtrus_Mission8Victory_02
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
		m_OverrideMusicTrack = MusicPlaylistType.InGame_SCH_FinalLevels;
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
		case 514:
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Tamsin_Female_Forsaken_Story_Kurtrus_Mission8Intro_01);
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission8Intro_02);
			break;
		case 506:
			MissionPause(pause: true);
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Tamsin_Female_Forsaken_Story_Kurtrus_Mission8Loss_01);
			MissionPause(pause: false);
			break;
		case 504:
			MissionPause(pause: true);
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission8Victory_01);
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Tamsin_Female_Forsaken_Story_Kurtrus_Mission8Victory_02);
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission8Victory_03);
			MissionPause(pause: false);
			break;
		case 517:
			yield return MissionPlayVO(enemyActor, m_InGame_BossIdleLines);
			break;
		case 515:
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Tamsin_Female_Forsaken_Story_Kurtrus_Mission8EmoteResponse_01);
			break;
		case 510:
			yield return MissionPlayVO(enemyActor, m_InGame_BossHeroPowerLines);
			break;
		case 516:
			yield return MissionPlaySound(enemyActor, VO_Story_Hero_Tamsin_Female_Forsaken_Story_Kurtrus_Mission8Death_01);
			break;
		case 100:
			yield return MissionPlayVOOnce(enemyActor, VO_Story_Hero_Tamsin_Female_Forsaken_Story_Kurtrus_Mission8ExchangeJ_01);
			break;
		case 101:
			yield return MissionPlayVOOnce(enemyActor, VO_Story_Hero_Tamsin_Female_Forsaken_Story_Kurtrus_Mission8ExchangeM_01);
			break;
		case 102:
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission8ExchangeG_01);
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Tamsin_Female_Forsaken_Story_Kurtrus_Mission8ExchangeG_02);
			break;
		case 103:
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission8ExchangeH_01);
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Tamsin_Female_Forsaken_Story_Kurtrus_Mission8ExchangeH_02);
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
		if (m_playedLines.Contains(entity.GetCardId()) && entity.GetCardType() != TAG_CARDTYPE.HERO_POWER)
		{
			yield break;
		}
		yield return WaitForEntitySoundToFinish(entity);
		string cardID = entity.GetCardId();
		m_playedLines.Add(cardID);
		Actor enemyActor = GameState.Get().GetOpposingSidePlayer().GetHero()
			.GetCard()
			.GetActor();
		GameState.Get().GetFriendlySidePlayer().GetHero()
			.GetCard()
			.GetActor();
		if (!(cardID == "BAR_325"))
		{
			if (cardID == "BAR_327")
			{
				yield return MissionPlayVO(enemyActor, VO_Story_Hero_Tamsin_Female_Forsaken_Story_Kurtrus_Mission8ExchangeL_01);
			}
		}
		else
		{
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Tamsin_Female_Forsaken_Story_Kurtrus_Mission8ExchangeK_01);
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
			switch (cardID)
			{
			case "BAR_914":
			case "BAR_914t":
			case "BAR_914t2":
				yield return MissionPlayVOOnce(enemyActor, VO_Story_Hero_Tamsin_Female_Forsaken_Story_Kurtrus_Mission8ExchangeN_01);
				break;
			case "BAR_910":
				yield return MissionPlayVOOnce(enemyActor, VO_Story_Hero_Tamsin_Female_Forsaken_Story_Kurtrus_Mission8ExchangeO_01);
				break;
			case "BAR_911":
			case "EX1_312":
			case "CORE_EX1_312":
			case "VAN_EX1_312":
				yield return MissionPlayVOOnce(enemyActor, VO_Story_Hero_Tamsin_Female_Forsaken_Story_Kurtrus_Mission8ExchangeP_01);
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
			yield return MissionPlayVO("BOM_04_Cariel_008t", VO_Story_Hero_Cariel_Female_Human_Story_Kurtrus_Mission8ExchangeA_01);
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Tamsin_Female_Forsaken_Story_Kurtrus_Mission8ExchangeA_02);
			break;
		case 5:
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission8ExchangeB_01);
			yield return MissionPlayVO("BOM_04_Cariel_008t", VO_Story_Hero_Cariel_Female_Human_Story_Kurtrus_Mission8ExchangeB_02);
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission8ExchangeB_03);
			break;
		case 7:
			yield return MissionPlayVO("BOM_04_Cariel_008t", VO_Story_Hero_Cariel_Female_Human_Story_Kurtrus_Mission8ExchangeC_01);
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Tamsin_Female_Forsaken_Story_Kurtrus_Mission8ExchangeC_02);
			break;
		case 9:
			yield return MissionPlayVO("BOM_04_Cariel_008t", VO_Story_Hero_Cariel_Female_Human_Story_Kurtrus_Mission8ExchangeD_01);
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Tamsin_Female_Forsaken_Story_Kurtrus_Mission8ExchangeD_02);
			break;
		case 11:
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission8ExchangeE_01);
			yield return MissionPlayVO("BOM_04_Cariel_008t", VO_Story_Hero_Cariel_Female_Human_Story_Kurtrus_Mission8ExchangeE_02);
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Tamsin_Female_Forsaken_Story_Kurtrus_Mission8ExchangeE_03);
			break;
		case 13:
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission8ExchangeF_01);
			yield return MissionPlayVO("BOM_04_Cariel_008t", VO_Story_Hero_Cariel_Female_Human_Story_Kurtrus_Mission8ExchangeF_02);
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Tamsin_Female_Forsaken_Story_Kurtrus_Mission8ExchangeF_03);
			break;
		}
	}
}
