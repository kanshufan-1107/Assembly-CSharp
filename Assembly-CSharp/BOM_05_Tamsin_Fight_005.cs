using System.Collections;
using System.Collections.Generic;

public class BOM_05_Tamsin_Fight_005 : BOM_05_Tamsin_Dungeon
{
	private static readonly AssetReference VO_Story_Hero_Elling_Male_Human_BOM_Tamsin_Mission5Death_01 = new AssetReference("VO_Story_Hero_Elling_Male_Human_BOM_Tamsin_Mission5Death_01.prefab:20f1f54e406d38945820939adf5a981a");

	private static readonly AssetReference VO_Story_Hero_Elling_Male_Human_BOM_Tamsin_Mission5EmoteResponse_01 = new AssetReference("VO_Story_Hero_Elling_Male_Human_BOM_Tamsin_Mission5EmoteResponse_01.prefab:e43a091bac01ef14d81e5e5958745a70");

	private static readonly AssetReference VO_Story_Hero_Elling_Male_Human_BOM_Tamsin_Mission5ExchangeA_01 = new AssetReference("VO_Story_Hero_Elling_Male_Human_BOM_Tamsin_Mission5ExchangeA_01.prefab:8c8a9b302925cad4b87b15c2d2c711d1");

	private static readonly AssetReference VO_Story_Hero_Elling_Male_Human_BOM_Tamsin_Mission5ExchangeB_01 = new AssetReference("VO_Story_Hero_Elling_Male_Human_BOM_Tamsin_Mission5ExchangeB_01.prefab:f2147eedb8822d346ba584284c0bbb49");

	private static readonly AssetReference VO_Story_Hero_Elling_Male_Human_BOM_Tamsin_Mission5ExchangeC_01 = new AssetReference("VO_Story_Hero_Elling_Male_Human_BOM_Tamsin_Mission5ExchangeC_01.prefab:c502099198a80ad439cb211cb9124920");

	private static readonly AssetReference VO_Story_Hero_Elling_Male_Human_BOM_Tamsin_Mission5ExchangeD_01 = new AssetReference("VO_Story_Hero_Elling_Male_Human_BOM_Tamsin_Mission5ExchangeD_01.prefab:86ec0f3e84f6edc42a43decc0e75a40e");

	private static readonly AssetReference VO_Story_Hero_Elling_Male_Human_BOM_Tamsin_Mission5ExchangeE_01 = new AssetReference("VO_Story_Hero_Elling_Male_Human_BOM_Tamsin_Mission5ExchangeE_01.prefab:1741c1494e942144da448521526b1549");

	private static readonly AssetReference VO_Story_Hero_Elling_Male_Human_BOM_Tamsin_Mission5ExchangeF_01 = new AssetReference("VO_Story_Hero_Elling_Male_Human_BOM_Tamsin_Mission5ExchangeF_01.prefab:4a8edd9ee8741384586dbf6f85f70d3c");

	private static readonly AssetReference VO_Story_Hero_Elling_Male_Human_BOM_Tamsin_Mission5ExchangeG_01 = new AssetReference("VO_Story_Hero_Elling_Male_Human_BOM_Tamsin_Mission5ExchangeG_01.prefab:21f1b66b1f31b6b45a5c5d9603dd063f");

	private static readonly AssetReference VO_Story_Hero_Elling_Male_Human_BOM_Tamsin_Mission5ExchangeH_01 = new AssetReference("VO_Story_Hero_Elling_Male_Human_BOM_Tamsin_Mission5ExchangeH_01.prefab:1c872cdb917337a4bb09461101909ecb");

	private static readonly AssetReference VO_Story_Hero_Elling_Male_Human_BOM_Tamsin_Mission5ExchangeI_01 = new AssetReference("VO_Story_Hero_Elling_Male_Human_BOM_Tamsin_Mission5ExchangeI_01.prefab:395f441b17cec1c49b2bc2f0b58a9099");

	private static readonly AssetReference VO_Story_Hero_Elling_Male_Human_BOM_Tamsin_Mission5ExchangeJ_01 = new AssetReference("VO_Story_Hero_Elling_Male_Human_BOM_Tamsin_Mission5ExchangeJ_01.prefab:521f03932612e55489c1587eb117d4d0");

	private static readonly AssetReference VO_Story_Hero_Elling_Male_Human_BOM_Tamsin_Mission5ExchangeK_01 = new AssetReference("VO_Story_Hero_Elling_Male_Human_BOM_Tamsin_Mission5ExchangeK_01.prefab:d10707e37fbe07a4cb9bf5060795eb14");

	private static readonly AssetReference VO_Story_Hero_Elling_Male_Human_BOM_Tamsin_Mission5ExchangeL_01 = new AssetReference("VO_Story_Hero_Elling_Male_Human_BOM_Tamsin_Mission5ExchangeL_01.prefab:bd2051d6d8f3d4241bcf4b3fc0044ea9");

	private static readonly AssetReference VO_Story_Hero_Elling_Male_Human_BOM_Tamsin_Mission5ExchangeM_01 = new AssetReference("VO_Story_Hero_Elling_Male_Human_BOM_Tamsin_Mission5ExchangeM_01.prefab:08a542d1d533b1c479776312e519ab11");

	private static readonly AssetReference VO_Story_Hero_Elling_Male_Human_BOM_Tamsin_Mission5ExchangeN_01 = new AssetReference("VO_Story_Hero_Elling_Male_Human_BOM_Tamsin_Mission5ExchangeN_01.prefab:869684dac058cff4096009b977468f1f");

	private static readonly AssetReference VO_Story_Hero_Elling_Male_Human_BOM_Tamsin_Mission5ExchangeO_01 = new AssetReference("VO_Story_Hero_Elling_Male_Human_BOM_Tamsin_Mission5ExchangeO_01.prefab:0cd635f6264e5af479171fe609560c9d");

	private static readonly AssetReference VO_Story_Hero_Elling_Male_Human_BOM_Tamsin_Mission5HeroPower_01 = new AssetReference("VO_Story_Hero_Elling_Male_Human_BOM_Tamsin_Mission5HeroPower_01.prefab:d887048020a8b7841b45e8b1bc37a9b2");

	private static readonly AssetReference VO_Story_Hero_Elling_Male_Human_BOM_Tamsin_Mission5HeroPower_02 = new AssetReference("VO_Story_Hero_Elling_Male_Human_BOM_Tamsin_Mission5HeroPower_02.prefab:30df109cc6d91d14ab17a95c28236a7e");

	private static readonly AssetReference VO_Story_Hero_Elling_Male_Human_BOM_Tamsin_Mission5HeroPower_03 = new AssetReference("VO_Story_Hero_Elling_Male_Human_BOM_Tamsin_Mission5HeroPower_03.prefab:fabcb675cceb4f24c83f9a6dee366860");

	private static readonly AssetReference VO_Story_Hero_Elling_Male_Human_BOM_Tamsin_Mission5Idle_01 = new AssetReference("VO_Story_Hero_Elling_Male_Human_BOM_Tamsin_Mission5Idle_01.prefab:53fecf47ae8d1c94ea7c320af60991a6");

	private static readonly AssetReference VO_Story_Hero_Elling_Male_Human_BOM_Tamsin_Mission5Idle_02 = new AssetReference("VO_Story_Hero_Elling_Male_Human_BOM_Tamsin_Mission5Idle_02.prefab:3fa0bdf74b301364e8b4496bae4ede6f");

	private static readonly AssetReference VO_Story_Hero_Elling_Male_Human_BOM_Tamsin_Mission5Idle_03 = new AssetReference("VO_Story_Hero_Elling_Male_Human_BOM_Tamsin_Mission5Idle_03.prefab:ee22c5762c6db544a89e958bf62d9665");

	private static readonly AssetReference VO_Story_Hero_Elling_Male_Human_BOM_Tamsin_Mission5Idle_04 = new AssetReference("VO_Story_Hero_Elling_Male_Human_BOM_Tamsin_Mission5Idle_04.prefab:54f981d00e5ecef40a2c669baab5bc8c");

	private static readonly AssetReference VO_Story_Hero_Elling_Male_Human_BOM_Tamsin_Mission5Idle_05 = new AssetReference("VO_Story_Hero_Elling_Male_Human_BOM_Tamsin_Mission5Idle_05.prefab:d8fee24760a6da94bbe228fdfbd9128c");

	private static readonly AssetReference VO_Story_Hero_Elling_Male_Human_BOM_Tamsin_Mission5Intro_01 = new AssetReference("VO_Story_Hero_Elling_Male_Human_BOM_Tamsin_Mission5Intro_01.prefab:0828f5ec0e6d46d43ba07d709d52b34c");

	private static readonly AssetReference VO_Story_Hero_Elling_Male_Human_BOM_Tamsin_Mission5Loss_01 = new AssetReference("VO_Story_Hero_Elling_Male_Human_BOM_Tamsin_Mission5Loss_01.prefab:5eba03b3e521a114a8dd2d90e9d7a6bf");

	private static readonly AssetReference VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission5ExchangeA_02 = new AssetReference("VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission5ExchangeA_02.prefab:87e189c33194e4f4aa153a303c645766");

	private static readonly AssetReference VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission5ExchangeB_02 = new AssetReference("VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission5ExchangeB_02.prefab:d771c1c20b172eb4f9b7b08875a11df9");

	private static readonly AssetReference VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission5ExchangeC_02 = new AssetReference("VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission5ExchangeC_02.prefab:bb3bed1f033cadc408af96892aa06a82");

	private static readonly AssetReference VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission5ExchangeD_02 = new AssetReference("VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission5ExchangeD_02.prefab:fa1ce544327321147ac1bd0abd25fefd");

	private static readonly AssetReference VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission5ExchangeE_02 = new AssetReference("VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission5ExchangeE_02.prefab:db89dba6ccea2a54abb782d1445248df");

	private static readonly AssetReference VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission5ExchangeF_02 = new AssetReference("VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission5ExchangeF_02.prefab:85a601a2ed02b0c468e4a0f252ea3a7b");

	private static readonly AssetReference VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission5ExchangeG_02 = new AssetReference("VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission5ExchangeG_02.prefab:16638ac3921bffb468374b05e33ab087");

	private static readonly AssetReference VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission5ExchangeH_02 = new AssetReference("VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission5ExchangeH_02.prefab:4d1db1fd65ae6c64e8cf7f0098a711aa");

	private static readonly AssetReference VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission5ExchangeI_02 = new AssetReference("VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission5ExchangeI_02.prefab:73dc77e852f2f6146a76268a4e22a473");

	private static readonly AssetReference VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission5ExchangeJ_02 = new AssetReference("VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission5ExchangeJ_02.prefab:3eec5c1e729c4274a81abe5f636978b6");

	private static readonly AssetReference VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission5ExchangeK_02 = new AssetReference("VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission5ExchangeK_02.prefab:56d854a07b6f1a443af2ecf20ff0c0cb");

	private static readonly AssetReference VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission5ExchangeL_02 = new AssetReference("VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission5ExchangeL_02.prefab:d882586ddf520984a84a637e321e9d84");

	private static readonly AssetReference VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission5ExchangeM_02 = new AssetReference("VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission5ExchangeM_02.prefab:5928b5b683188554bb0185322824eb28");

	private static readonly AssetReference VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission5ExchangeN_02 = new AssetReference("VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission5ExchangeN_02.prefab:d971ea497c3d41142a13bc82808c7a54");

	private static readonly AssetReference VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission5ExchangeO_02 = new AssetReference("VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission5ExchangeO_02.prefab:983a700ceaacf8846b1dd9bfa5b69969");

	private static readonly AssetReference VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission5Intro_02 = new AssetReference("VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission5Intro_02.prefab:d0f1be8183b487c4db68c4d55d92cbbb");

	private static readonly AssetReference VO_PVPDR_Hero_Tamsin_Female_Forsaken_Death_01 = new AssetReference("VO_PVPDR_Hero_Tamsin_Female_Forsaken_Death_01.prefab:0261caf1dfbfae146bf927a03851b086");

	private List<string> m_InGame_BossUsesHeroPowerLines = new List<string> { VO_Story_Hero_Elling_Male_Human_BOM_Tamsin_Mission5HeroPower_01, VO_Story_Hero_Elling_Male_Human_BOM_Tamsin_Mission5HeroPower_02, VO_Story_Hero_Elling_Male_Human_BOM_Tamsin_Mission5HeroPower_03 };

	private List<string> m_InGame_BossIdleLines = new List<string> { VO_Story_Hero_Elling_Male_Human_BOM_Tamsin_Mission5Idle_01, VO_Story_Hero_Elling_Male_Human_BOM_Tamsin_Mission5Idle_02, VO_Story_Hero_Elling_Male_Human_BOM_Tamsin_Mission5Idle_03, VO_Story_Hero_Elling_Male_Human_BOM_Tamsin_Mission5Idle_04, VO_Story_Hero_Elling_Male_Human_BOM_Tamsin_Mission5Idle_05 };

	private HashSet<string> m_playedLines = new HashSet<string>();

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> m_SoundFiles = new List<string>
		{
			VO_Story_Hero_Elling_Male_Human_BOM_Tamsin_Mission5Death_01, VO_Story_Hero_Elling_Male_Human_BOM_Tamsin_Mission5EmoteResponse_01, VO_Story_Hero_Elling_Male_Human_BOM_Tamsin_Mission5ExchangeA_01, VO_Story_Hero_Elling_Male_Human_BOM_Tamsin_Mission5ExchangeB_01, VO_Story_Hero_Elling_Male_Human_BOM_Tamsin_Mission5ExchangeC_01, VO_Story_Hero_Elling_Male_Human_BOM_Tamsin_Mission5ExchangeD_01, VO_Story_Hero_Elling_Male_Human_BOM_Tamsin_Mission5ExchangeE_01, VO_Story_Hero_Elling_Male_Human_BOM_Tamsin_Mission5ExchangeF_01, VO_Story_Hero_Elling_Male_Human_BOM_Tamsin_Mission5ExchangeG_01, VO_Story_Hero_Elling_Male_Human_BOM_Tamsin_Mission5ExchangeH_01,
			VO_Story_Hero_Elling_Male_Human_BOM_Tamsin_Mission5ExchangeI_01, VO_Story_Hero_Elling_Male_Human_BOM_Tamsin_Mission5ExchangeJ_01, VO_Story_Hero_Elling_Male_Human_BOM_Tamsin_Mission5ExchangeK_01, VO_Story_Hero_Elling_Male_Human_BOM_Tamsin_Mission5ExchangeL_01, VO_Story_Hero_Elling_Male_Human_BOM_Tamsin_Mission5ExchangeM_01, VO_Story_Hero_Elling_Male_Human_BOM_Tamsin_Mission5ExchangeN_01, VO_Story_Hero_Elling_Male_Human_BOM_Tamsin_Mission5ExchangeO_01, VO_Story_Hero_Elling_Male_Human_BOM_Tamsin_Mission5HeroPower_01, VO_Story_Hero_Elling_Male_Human_BOM_Tamsin_Mission5HeroPower_02, VO_Story_Hero_Elling_Male_Human_BOM_Tamsin_Mission5HeroPower_03,
			VO_Story_Hero_Elling_Male_Human_BOM_Tamsin_Mission5Idle_01, VO_Story_Hero_Elling_Male_Human_BOM_Tamsin_Mission5Idle_02, VO_Story_Hero_Elling_Male_Human_BOM_Tamsin_Mission5Idle_03, VO_Story_Hero_Elling_Male_Human_BOM_Tamsin_Mission5Idle_04, VO_Story_Hero_Elling_Male_Human_BOM_Tamsin_Mission5Idle_05, VO_Story_Hero_Elling_Male_Human_BOM_Tamsin_Mission5Intro_01, VO_Story_Hero_Elling_Male_Human_BOM_Tamsin_Mission5Loss_01, VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission5ExchangeA_02, VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission5ExchangeB_02, VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission5ExchangeC_02,
			VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission5ExchangeD_02, VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission5ExchangeE_02, VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission5ExchangeF_02, VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission5ExchangeG_02, VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission5ExchangeH_02, VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission5ExchangeI_02, VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission5ExchangeJ_02, VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission5ExchangeK_02, VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission5ExchangeL_02, VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission5ExchangeM_02,
			VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission5ExchangeN_02, VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission5ExchangeO_02, VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission5Intro_02, VO_PVPDR_Hero_Tamsin_Female_Forsaken_Death_01
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
			MissionPause(pause: true);
			yield return MissionPlaySound(enemyActor, VO_Story_Hero_Elling_Male_Human_BOM_Tamsin_Mission5Death_01);
			MissionPause(pause: false);
			break;
		case 519:
			MissionPause(pause: true);
			yield return MissionPlaySound(friendlyActor, VO_PVPDR_Hero_Tamsin_Female_Forsaken_Death_01);
			MissionPause(pause: false);
			break;
		case 517:
			yield return MissionPlayVO(enemyActor, m_InGame_BossIdleLines);
			break;
		case 510:
			yield return MissionPlayVO(enemyActor, m_InGame_BossUsesHeroPowerLines);
			break;
		case 515:
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Elling_Male_Human_BOM_Tamsin_Mission5EmoteResponse_01);
			break;
		case 514:
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Elling_Male_Human_BOM_Tamsin_Mission5Intro_01);
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission5Intro_02);
			break;
		case 507:
			MissionPause(pause: true);
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Elling_Male_Human_BOM_Tamsin_Mission5Loss_01);
			MissionPause(pause: false);
			break;
		case 100:
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Elling_Male_Human_BOM_Tamsin_Mission5ExchangeD_01);
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission5ExchangeD_02);
			break;
		case 101:
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Elling_Male_Human_BOM_Tamsin_Mission5ExchangeG_01);
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission5ExchangeG_02);
			break;
		case 102:
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Elling_Male_Human_BOM_Tamsin_Mission5ExchangeH_01);
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission5ExchangeH_02);
			break;
		case 103:
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Elling_Male_Human_BOM_Tamsin_Mission5ExchangeI_01);
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission5ExchangeI_02);
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
			Actor friendlyActor = GameState.Get().GetFriendlySidePlayer().GetHero()
				.GetCard()
				.GetActor();
			switch (cardID)
			{
			case "SW_323":
				yield return MissionPlayVO(enemyActor, VO_Story_Hero_Elling_Male_Human_BOM_Tamsin_Mission5ExchangeC_01);
				yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission5ExchangeC_02);
				break;
			case "SW_139":
				yield return MissionPlayVO(enemyActor, VO_Story_Hero_Elling_Male_Human_BOM_Tamsin_Mission5ExchangeF_01);
				yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission5ExchangeF_02);
				break;
			case "SW_045":
				yield return MissionPlayVO(enemyActor, VO_Story_Hero_Elling_Male_Human_BOM_Tamsin_Mission5ExchangeJ_01);
				yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission5ExchangeJ_02);
				break;
			case "SW_056":
				yield return MissionPlayVO(enemyActor, VO_Story_Hero_Elling_Male_Human_BOM_Tamsin_Mission5ExchangeK_01);
				yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission5ExchangeK_02);
				break;
			case "SW_069":
				yield return MissionPlayVO(enemyActor, VO_Story_Hero_Elling_Male_Human_BOM_Tamsin_Mission5ExchangeL_01);
				yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission5ExchangeL_02);
				break;
			case "SW_070":
				yield return MissionPlayVO(enemyActor, VO_Story_Hero_Elling_Male_Human_BOM_Tamsin_Mission5ExchangeM_01);
				yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission5ExchangeM_02);
				break;
			case "SW_076":
				yield return MissionPlayVO(enemyActor, VO_Story_Hero_Elling_Male_Human_BOM_Tamsin_Mission5ExchangeN_01);
				yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission5ExchangeN_02);
				break;
			case "SW_074":
				yield return MissionPlayVO(enemyActor, VO_Story_Hero_Elling_Male_Human_BOM_Tamsin_Mission5ExchangeO_01);
				yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission5ExchangeO_02);
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
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Elling_Male_Human_BOM_Tamsin_Mission5ExchangeA_01);
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission5ExchangeA_02);
			break;
		case 5:
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Elling_Male_Human_BOM_Tamsin_Mission5ExchangeB_01);
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission5ExchangeB_02);
			break;
		}
	}
}
