using System.Collections;
using System.Collections.Generic;

public class BOM_07_Scabbs_Fight_008 : BOM_07_Scabbs_Dungeon
{
	private static readonly AssetReference VO_Story_Hero_Edwin_Male_Human_BOM_Scabbs_Mission8BrukanIdle_02 = new AssetReference("VO_Story_Hero_Edwin_Male_Human_BOM_Scabbs_Mission8BrukanIdle_02.prefab:d6a778aacc594aeaa6f7409ef2e050cb");

	private static readonly AssetReference VO_Story_Hero_Edwin_Male_Human_BOM_Scabbs_Mission8CarielIdle_02 = new AssetReference("VO_Story_Hero_Edwin_Male_Human_BOM_Scabbs_Mission8CarielIdle_02.prefab:d510b6cafca148d2971207cdba8207e0");

	private static readonly AssetReference VO_Story_Hero_Edwin_Male_Human_BOM_Scabbs_Mission8DawngraspIdle_02 = new AssetReference("VO_Story_Hero_Edwin_Male_Human_BOM_Scabbs_Mission8DawngraspIdle_02.prefab:352a936458044d7e8efa4be5ece25bbd");

	private static readonly AssetReference VO_Story_Hero_Edwin_Male_Human_BOM_Scabbs_Mission8Death_01 = new AssetReference("VO_Story_Hero_Edwin_Male_Human_BOM_Scabbs_Mission8Death_01.prefab:24b136e85ced4795a6d8f3af38b8f37b");

	private static readonly AssetReference VO_Story_Hero_Edwin_Male_Human_BOM_Scabbs_Mission8EmoteResponse_01 = new AssetReference("VO_Story_Hero_Edwin_Male_Human_BOM_Scabbs_Mission8EmoteResponse_01.prefab:e2742f572f2e4c6b9b945366ad4660ca");

	private static readonly AssetReference VO_Story_Hero_Edwin_Male_Human_BOM_Scabbs_Mission8ExchangeA_02 = new AssetReference("VO_Story_Hero_Edwin_Male_Human_BOM_Scabbs_Mission8ExchangeA_02.prefab:1079909dd12044debfdcff567b6111a9");

	private static readonly AssetReference VO_Story_Hero_Edwin_Male_Human_BOM_Scabbs_Mission8ExchangeB_02 = new AssetReference("VO_Story_Hero_Edwin_Male_Human_BOM_Scabbs_Mission8ExchangeB_02.prefab:024ae91ccd8a4047bf96361f7a444727");

	private static readonly AssetReference VO_Story_Hero_Edwin_Male_Human_BOM_Scabbs_Mission8ExchangeC_01 = new AssetReference("VO_Story_Hero_Edwin_Male_Human_BOM_Scabbs_Mission8ExchangeC_01.prefab:99ad4101895c452eb5f40f6fcee3f8cf");

	private static readonly AssetReference VO_Story_Hero_Edwin_Male_Human_BOM_Scabbs_Mission8ExchangeD_01 = new AssetReference("VO_Story_Hero_Edwin_Male_Human_BOM_Scabbs_Mission8ExchangeD_01.prefab:a327bf52f1bf4a7a93e5cc1e8c3e71f3");

	private static readonly AssetReference VO_Story_Hero_Edwin_Male_Human_BOM_Scabbs_Mission8ExchangeE_02 = new AssetReference("VO_Story_Hero_Edwin_Male_Human_BOM_Scabbs_Mission8ExchangeE_02.prefab:4d88f3771fbe4504a86f5dab65d487b5");

	private static readonly AssetReference VO_Story_Hero_Edwin_Male_Human_BOM_Scabbs_Mission8GuffIdle_02 = new AssetReference("VO_Story_Hero_Edwin_Male_Human_BOM_Scabbs_Mission8GuffIdle_02.prefab:7d8427b552b14200b907877652c83775");

	private static readonly AssetReference VO_Story_Hero_Edwin_Male_Human_BOM_Scabbs_Mission8HeroPower_01 = new AssetReference("VO_Story_Hero_Edwin_Male_Human_BOM_Scabbs_Mission8HeroPower_01.prefab:a0c254ad6bf64f03afee0e5e143b520c");

	private static readonly AssetReference VO_Story_Hero_Edwin_Male_Human_BOM_Scabbs_Mission8HeroPower_02 = new AssetReference("VO_Story_Hero_Edwin_Male_Human_BOM_Scabbs_Mission8HeroPower_02.prefab:a26442353f254a019985b85317c61783");

	private static readonly AssetReference VO_Story_Hero_Edwin_Male_Human_BOM_Scabbs_Mission8HeroPower_03 = new AssetReference("VO_Story_Hero_Edwin_Male_Human_BOM_Scabbs_Mission8HeroPower_03.prefab:cbc4b47fda7a42c8a036b3150bb7ae3c");

	private static readonly AssetReference VO_Story_Hero_Edwin_Male_Human_BOM_Scabbs_Mission8Idle_01 = new AssetReference("VO_Story_Hero_Edwin_Male_Human_BOM_Scabbs_Mission8Idle_01.prefab:23d3114ee0d54f30a18929737746b4da");

	private static readonly AssetReference VO_Story_Hero_Edwin_Male_Human_BOM_Scabbs_Mission8Idle_02 = new AssetReference("VO_Story_Hero_Edwin_Male_Human_BOM_Scabbs_Mission8Idle_02.prefab:e1d527cf7f3a4ea7ac491416ba7c72b4");

	private static readonly AssetReference VO_Story_Hero_Edwin_Male_Human_BOM_Scabbs_Mission8Idle_03 = new AssetReference("VO_Story_Hero_Edwin_Male_Human_BOM_Scabbs_Mission8Idle_03.prefab:2212600982b74844befefcf8c276647b");

	private static readonly AssetReference VO_Story_Hero_Edwin_Male_Human_BOM_Scabbs_Mission8Idle_04 = new AssetReference("VO_Story_Hero_Edwin_Male_Human_BOM_Scabbs_Mission8Idle_04.prefab:cc7362368c744cd58ccccb8bda9cc11d");

	private static readonly AssetReference VO_Story_Hero_Edwin_Male_Human_BOM_Scabbs_Mission8Intro_02 = new AssetReference("VO_Story_Hero_Edwin_Male_Human_BOM_Scabbs_Mission8Intro_02.prefab:5a85612842634a3ca3d78c329934630c");

	private static readonly AssetReference VO_Story_Hero_Edwin_Male_Human_BOM_Scabbs_Mission8KurtrusIdle_02 = new AssetReference("VO_Story_Hero_Edwin_Male_Human_BOM_Scabbs_Mission8KurtrusIdle_02.prefab:0c37816ebe7b4b1e94fc4543d7c27946");

	private static readonly AssetReference VO_Story_Hero_Edwin_Male_Human_BOM_Scabbs_Mission8Loss_01 = new AssetReference("VO_Story_Hero_Edwin_Male_Human_BOM_Scabbs_Mission8Loss_01.prefab:7ca49aa915684e559482ea257ed93a25");

	private static readonly AssetReference VO_Story_Hero_Edwin_Male_Human_BOM_Scabbs_Mission8RokaraIdle_02 = new AssetReference("VO_Story_Hero_Edwin_Male_Human_BOM_Scabbs_Mission8RokaraIdle_02.prefab:290bcdce544c466db0de80c9bf072358");

	private static readonly AssetReference VO_Story_Hero_Edwin_Male_Human_BOM_Scabbs_Mission8TavishIdle_02 = new AssetReference("VO_Story_Hero_Edwin_Male_Human_BOM_Scabbs_Mission8TavishIdle_02.prefab:bec41e87ae654e97a423cf89cd257acf");

	private static readonly AssetReference VO_Story_Hero_Edwin_Male_Human_BOM_Scabbs_Mission8Victory_01 = new AssetReference("VO_Story_Hero_Edwin_Male_Human_BOM_Scabbs_Mission8Victory_01.prefab:35a88d923b2d48f6bf0b64b2e2f76a1a");

	private static readonly AssetReference VO_Story_Hero_Edwin_Male_Human_BOM_Scabbs_Mission8XyrellaIdle_02 = new AssetReference("VO_Story_Hero_Edwin_Male_Human_BOM_Scabbs_Mission8XyrellaIdle_02.prefab:415e7aefbb9b495fbe5dc965229c7e8d");

	private static readonly AssetReference VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission8ExchangeB_01 = new AssetReference("VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission8ExchangeB_01.prefab:3dd255e31fa3459c8767fe56b2db298c");

	private static readonly AssetReference VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission8Intro_01 = new AssetReference("VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission8Intro_01.prefab:922ed444d3244c69b10704cf59f008eb");

	private static readonly AssetReference VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission8Victory_02 = new AssetReference("VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission8Victory_02.prefab:bf7fd9e050dbac041bc1d3540f9fcc52");

	private static readonly AssetReference VO_Story_Hero_Tavish_Male_Dwarf_BOM_Scabbs_Mission8Victory_03 = new AssetReference("VO_Story_Hero_Tavish_Male_Dwarf_BOM_Scabbs_Mission8Victory_03.prefab:10d6caae5b264116930ed35fb4f52061");

	private static readonly AssetReference VO_Story_Hero_Vanessa_Female_Human_BOM_Scabbs_Mission8ExchangeA_01 = new AssetReference("VO_Story_Hero_Vanessa_Female_Human_BOM_Scabbs_Mission8ExchangeA_01.prefab:0f2ae626b2bf431e846826e8b3402a3a");

	private static readonly AssetReference VO_Story_Hero_Vanessa_Female_Human_BOM_Scabbs_Mission8ExchangeE_01 = new AssetReference("VO_Story_Hero_Vanessa_Female_Human_BOM_Scabbs_Mission8ExchangeE_01.prefab:16880311bfce4530999f136a1d775854");

	private static readonly AssetReference VO_Story_Hero_Xyrella_Female_Draenei_BOM_Scabbs_Mission8Victory_04 = new AssetReference("VO_Story_Hero_Xyrella_Female_Draenei_BOM_Scabbs_Mission8Victory_04.prefab:de917d938dccc9841b60c34abbb13aa4");

	private static readonly AssetReference VO_Story_Minion_Brukan_Male_Troll_BOM_Scabbs_Mission8BrukanIdle_01 = new AssetReference("VO_Story_Minion_Brukan_Male_Troll_BOM_Scabbs_Mission8BrukanIdle_01.prefab:585a772da5fc49a7b46792d49e3c29cd");

	private static readonly AssetReference VO_Story_Minion_Cariel_Female_Human_BOM_Scabbs_Mission8CarielIdle_01 = new AssetReference("VO_Story_Minion_Cariel_Female_Human_BOM_Scabbs_Mission8CarielIdle_01.prefab:2dbb51517ea94ae5a3d0d102cbb69f8f");

	private static readonly AssetReference VO_Story_Minion_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission8DawngraspIdle_01 = new AssetReference("VO_Story_Minion_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission8DawngraspIdle_01.prefab:9904bdf398214acc85606e7b58498931");

	private static readonly AssetReference VO_Story_Minion_Guff_Male_Tauren_BOM_Scabbs_Mission8GuffIdle_01 = new AssetReference("VO_Story_Minion_Guff_Male_Tauren_BOM_Scabbs_Mission8GuffIdle_01.prefab:77adf20f4761d554791aa448f67567aa");

	private static readonly AssetReference VO_Story_Minion_Kurtrus_Male_NightElf_BOM_Scabbs_Mission8KurtrusIdle_01 = new AssetReference("VO_Story_Minion_Kurtrus_Male_NightElf_BOM_Scabbs_Mission8KurtrusIdle_01.prefab:2e0f30d413924f3196bca294798eb370");

	private static readonly AssetReference VO_Story_Minion_Rokara_Female_Orc_BOM_Scabbs_Mission8RokaraIdle_01 = new AssetReference("VO_Story_Minion_Rokara_Female_Orc_BOM_Scabbs_Mission8RokaraIdle_01.prefab:14aa2f227c204799b3ebe51fa72c3176");

	private static readonly AssetReference VO_Story_Minion_Tavish_Male_Dwarf_BOM_Scabbs_Mission8TavishIdle_01 = new AssetReference("VO_Story_Minion_Tavish_Male_Dwarf_BOM_Scabbs_Mission8TavishIdle_01.prefab:9d64be28ad80452988cfd17d908c8e00");

	private static readonly AssetReference VO_Story_Minion_Xyrella_Female_Draenei_BOM_Scabbs_Mission8XyrellaIdle_01 = new AssetReference("VO_Story_Minion_Xyrella_Female_Draenei_BOM_Scabbs_Mission8XyrellaIdle_01.prefab:a7dd3828db1749cf9a69e5e283d4a6fd");

	private static readonly AssetReference VO_PVPDR_Hero_Scabbs_Male_Gnome_Death_01 = new AssetReference("VO_PVPDR_Hero_Scabbs_Male_Gnome_Death_01.prefab:85e10a0b219b1974aaa959bd523c554b");

	private static readonly AssetReference VO_Story_Minion_Vanessa_Female_Human_BOM_Scabbs_Mission8Attack_01 = new AssetReference("VO_Story_Minion_Vanessa_Female_Human_BOM_Scabbs_Mission8Attack_01.prefab:dbbc59f1e06b4f75b256e788a111655b");

	private static readonly AssetReference VO_Story_Minion_Vanessa_Female_Human_BOM_Scabbs_Mission8Death_01 = new AssetReference("VO_Story_Minion_Vanessa_Female_Human_BOM_Scabbs_Mission8Death_01.prefab:970a9ff3c5fd472383e10a9a3c6f5deb");

	private static readonly AssetReference VO_Story_Minion_Vanessa_Female_Human_BOM_Scabbs_Mission8Play_01 = new AssetReference("VO_Story_Minion_Vanessa_Female_Human_BOM_Scabbs_Mission8Play_01.prefab:98097ff7a0174abea7d959af9064a79c");

	private List<string> m_VO_Story_Hero_Edwin_Male_Human_BOM_Scabbs_Mission8BrukanIdleLines = new List<string>
	{
		VO_Story_Hero_Edwin_Male_Human_BOM_Scabbs_Mission8BrukanIdle_02, VO_Story_Hero_Edwin_Male_Human_BOM_Scabbs_Mission8CarielIdle_02, VO_Story_Hero_Edwin_Male_Human_BOM_Scabbs_Mission8DawngraspIdle_02, VO_Story_Hero_Edwin_Male_Human_BOM_Scabbs_Mission8GuffIdle_02, VO_Story_Hero_Edwin_Male_Human_BOM_Scabbs_Mission8KurtrusIdle_02, VO_Story_Hero_Edwin_Male_Human_BOM_Scabbs_Mission8RokaraIdle_02, VO_Story_Hero_Edwin_Male_Human_BOM_Scabbs_Mission8TavishIdle_02, VO_Story_Hero_Edwin_Male_Human_BOM_Scabbs_Mission8XyrellaIdle_02, VO_Story_Minion_Brukan_Male_Troll_BOM_Scabbs_Mission8BrukanIdle_01, VO_Story_Minion_Cariel_Female_Human_BOM_Scabbs_Mission8CarielIdle_01,
		VO_Story_Minion_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission8DawngraspIdle_01, VO_Story_Minion_Kurtrus_Male_NightElf_BOM_Scabbs_Mission8KurtrusIdle_01, VO_Story_Minion_Rokara_Female_Orc_BOM_Scabbs_Mission8RokaraIdle_01, VO_Story_Minion_Tavish_Male_Dwarf_BOM_Scabbs_Mission8TavishIdle_01, VO_Story_Minion_Vanessa_Female_Human_BOM_Scabbs_Mission8Attack_01, VO_Story_Minion_Vanessa_Female_Human_BOM_Scabbs_Mission8Death_01, VO_Story_Minion_Xyrella_Female_Draenei_BOM_Scabbs_Mission8XyrellaIdle_01
	};

	private List<string> m_InGame_BossUsesHeroPowerLines = new List<string> { VO_Story_Hero_Edwin_Male_Human_BOM_Scabbs_Mission8HeroPower_01, VO_Story_Hero_Edwin_Male_Human_BOM_Scabbs_Mission8HeroPower_02, VO_Story_Hero_Edwin_Male_Human_BOM_Scabbs_Mission8HeroPower_03 };

	private List<string> m_InGame_BossIdleLines = new List<string> { VO_Story_Hero_Edwin_Male_Human_BOM_Scabbs_Mission8Idle_01, VO_Story_Hero_Edwin_Male_Human_BOM_Scabbs_Mission8Idle_02, VO_Story_Hero_Edwin_Male_Human_BOM_Scabbs_Mission8Idle_03, VO_Story_Hero_Edwin_Male_Human_BOM_Scabbs_Mission8Idle_04 };

	private HashSet<string> m_playedLines = new HashSet<string>();

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> m_SoundFiles = new List<string>
		{
			VO_Story_Hero_Edwin_Male_Human_BOM_Scabbs_Mission8BrukanIdle_02, VO_Story_Hero_Edwin_Male_Human_BOM_Scabbs_Mission8CarielIdle_02, VO_Story_Hero_Edwin_Male_Human_BOM_Scabbs_Mission8DawngraspIdle_02, VO_Story_Hero_Edwin_Male_Human_BOM_Scabbs_Mission8Death_01, VO_Story_Hero_Edwin_Male_Human_BOM_Scabbs_Mission8EmoteResponse_01, VO_Story_Hero_Edwin_Male_Human_BOM_Scabbs_Mission8ExchangeA_02, VO_Story_Hero_Edwin_Male_Human_BOM_Scabbs_Mission8ExchangeB_02, VO_Story_Hero_Edwin_Male_Human_BOM_Scabbs_Mission8ExchangeC_01, VO_Story_Hero_Edwin_Male_Human_BOM_Scabbs_Mission8ExchangeD_01, VO_Story_Hero_Edwin_Male_Human_BOM_Scabbs_Mission8ExchangeE_02,
			VO_Story_Hero_Edwin_Male_Human_BOM_Scabbs_Mission8GuffIdle_02, VO_Story_Hero_Edwin_Male_Human_BOM_Scabbs_Mission8HeroPower_01, VO_Story_Hero_Edwin_Male_Human_BOM_Scabbs_Mission8HeroPower_02, VO_Story_Hero_Edwin_Male_Human_BOM_Scabbs_Mission8HeroPower_03, VO_Story_Hero_Edwin_Male_Human_BOM_Scabbs_Mission8Idle_01, VO_Story_Hero_Edwin_Male_Human_BOM_Scabbs_Mission8Idle_02, VO_Story_Hero_Edwin_Male_Human_BOM_Scabbs_Mission8Idle_03, VO_Story_Hero_Edwin_Male_Human_BOM_Scabbs_Mission8Idle_04, VO_Story_Hero_Edwin_Male_Human_BOM_Scabbs_Mission8Intro_02, VO_Story_Hero_Edwin_Male_Human_BOM_Scabbs_Mission8KurtrusIdle_02,
			VO_Story_Hero_Edwin_Male_Human_BOM_Scabbs_Mission8Loss_01, VO_Story_Hero_Edwin_Male_Human_BOM_Scabbs_Mission8RokaraIdle_02, VO_Story_Hero_Edwin_Male_Human_BOM_Scabbs_Mission8TavishIdle_02, VO_Story_Hero_Edwin_Male_Human_BOM_Scabbs_Mission8Victory_01, VO_Story_Hero_Edwin_Male_Human_BOM_Scabbs_Mission8XyrellaIdle_02, VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission8ExchangeB_01, VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission8Intro_01, VO_Story_Hero_Vanessa_Female_Human_BOM_Scabbs_Mission8ExchangeA_01, VO_Story_Hero_Vanessa_Female_Human_BOM_Scabbs_Mission8ExchangeE_01, VO_Story_Minion_Brukan_Male_Troll_BOM_Scabbs_Mission8BrukanIdle_01,
			VO_Story_Minion_Cariel_Female_Human_BOM_Scabbs_Mission8CarielIdle_01, VO_Story_Minion_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission8DawngraspIdle_01, VO_Story_Minion_Kurtrus_Male_NightElf_BOM_Scabbs_Mission8KurtrusIdle_01, VO_Story_Minion_Rokara_Female_Orc_BOM_Scabbs_Mission8RokaraIdle_01, VO_Story_Minion_Tavish_Male_Dwarf_BOM_Scabbs_Mission8TavishIdle_01, VO_Story_Minion_Vanessa_Female_Human_BOM_Scabbs_Mission8Attack_01, VO_Story_Minion_Vanessa_Female_Human_BOM_Scabbs_Mission8Death_01, VO_Story_Minion_Vanessa_Female_Human_BOM_Scabbs_Mission8Play_01, VO_Story_Minion_Xyrella_Female_Draenei_BOM_Scabbs_Mission8XyrellaIdle_01, VO_Story_Minion_Guff_Male_Tauren_BOM_Scabbs_Mission8GuffIdle_01,
			VO_PVPDR_Hero_Scabbs_Male_Gnome_Death_01
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
			yield return MissionPlaySound(enemyActor, VO_Story_Hero_Edwin_Male_Human_BOM_Scabbs_Mission8Death_01);
			break;
		case 517:
			yield return MissionPlayVO(enemyActor, m_InGame_BossIdleLines);
			break;
		case 510:
			yield return MissionPlayVO(enemyActor, m_InGame_BossUsesHeroPowerLines);
			break;
		case 515:
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Edwin_Male_Human_BOM_Scabbs_Mission8EmoteResponse_01);
			break;
		case 514:
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission8Intro_01);
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Edwin_Male_Human_BOM_Scabbs_Mission8Intro_02);
			break;
		case 507:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Edwin_Male_Human_BOM_Scabbs_Mission8Loss_01);
			GameState.Get().SetBusy(busy: false);
			break;
		case 901:
			yield return MissionPlayVO("BOM_07_Scabbs_VanessaVanCleef_008t", VO_Story_Minion_Vanessa_Female_Human_BOM_Scabbs_Mission8Play_01);
			break;
		case 504:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Edwin_Male_Human_BOM_Scabbs_Mission8Victory_01);
			GameState.Get().SetBusy(busy: false);
			break;
		case 100:
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Vanessa_Female_Human_BOM_Scabbs_Mission8ExchangeE_01);
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Edwin_Male_Human_BOM_Scabbs_Mission8ExchangeE_02);
			break;
		case 519:
			yield return MissionPlaySound(enemyActor, VO_PVPDR_Hero_Scabbs_Male_Gnome_Death_01);
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
			GameState.Get().GetFriendlySidePlayer().GetHero()
				.GetCard()
				.GetActor();
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
			yield return MissionPlayVO("BOM_07_Scabbs_VanessaVanCleef_008t", VO_Story_Hero_Vanessa_Female_Human_BOM_Scabbs_Mission8ExchangeA_01);
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Edwin_Male_Human_BOM_Scabbs_Mission8ExchangeA_02);
			break;
		case 5:
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission8ExchangeB_01);
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Edwin_Male_Human_BOM_Scabbs_Mission8ExchangeB_02);
			break;
		case 7:
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Edwin_Male_Human_BOM_Scabbs_Mission8ExchangeC_01);
			break;
		case 9:
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Edwin_Male_Human_BOM_Scabbs_Mission8ExchangeD_01);
			break;
		case 15:
			yield return MissionPlayVO("BOM_07_Scabbs_Xyrella_008t", BOM_07_Scabbs_Dungeon.Xyrella2_BrassRing_Quote, VO_Story_Minion_Xyrella_Female_Draenei_BOM_Scabbs_Mission8XyrellaIdle_01);
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Edwin_Male_Human_BOM_Scabbs_Mission8XyrellaIdle_02);
			break;
		case 17:
			yield return MissionPlayVO("BOM_07_Scabbs_Rokara_008t", BOM_07_Scabbs_Dungeon.Rokara_B_BrassRing_Quote, VO_Story_Minion_Rokara_Female_Orc_BOM_Scabbs_Mission8RokaraIdle_01);
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Edwin_Male_Human_BOM_Scabbs_Mission8RokaraIdle_02);
			break;
		case 19:
			yield return MissionPlayVO("BOM_07_Scabbs_Cariel_008t", BOM_07_Scabbs_Dungeon.Cariel_BrassRing_Quote, VO_Story_Minion_Cariel_Female_Human_BOM_Scabbs_Mission8CarielIdle_01);
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Edwin_Male_Human_BOM_Scabbs_Mission8CarielIdle_02);
			break;
		case 21:
			yield return MissionPlayVO("BOM_07_Scabbs_Brukan_008t", BOM_07_Scabbs_Dungeon.Brukan_20_4_BrassRing_Quote, VO_Story_Minion_Brukan_Male_Troll_BOM_Scabbs_Mission8BrukanIdle_01);
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Edwin_Male_Human_BOM_Scabbs_Mission8BrukanIdle_02);
			break;
		case 23:
			yield return MissionPlayVO("BOM_07_Scabbs_Dawngrasp_008t", BOM_07_Scabbs_Dungeon.SWDawngraspMinion_BrassRing_Quote, VO_Story_Minion_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission8DawngraspIdle_01);
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Edwin_Male_Human_BOM_Scabbs_Mission8DawngraspIdle_02);
			break;
		case 25:
			yield return MissionPlayVO("BOM_07_Scabbs_Guff_008t", BOM_07_Scabbs_Dungeon.Guff_BrassRing_Quote, VO_Story_Minion_Guff_Male_Tauren_BOM_Scabbs_Mission8GuffIdle_01);
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Edwin_Male_Human_BOM_Scabbs_Mission8GuffIdle_02);
			break;
		case 27:
			yield return MissionPlayVO("BOM_07_Scabbs_Tavish_008t", BOM_07_Scabbs_Dungeon.Tavish4_BrassRing_Quote, VO_Story_Minion_Tavish_Male_Dwarf_BOM_Scabbs_Mission8TavishIdle_01);
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Edwin_Male_Human_BOM_Scabbs_Mission8TavishIdle_02);
			break;
		case 29:
			yield return MissionPlayVO("BOM_07_Scabbs_Kurtrus_008t", BOM_07_Scabbs_Dungeon.Kurtrus_Stormwind_BrassRing_Quote, VO_Story_Minion_Kurtrus_Male_NightElf_BOM_Scabbs_Mission8KurtrusIdle_01);
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Edwin_Male_Human_BOM_Scabbs_Mission8KurtrusIdle_02);
			break;
		}
	}
}
