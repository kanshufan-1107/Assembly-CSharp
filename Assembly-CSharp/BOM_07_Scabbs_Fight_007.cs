using System.Collections;
using System.Collections.Generic;

public class BOM_07_Scabbs_Fight_007 : BOM_07_Scabbs_Dungeon
{
	private static readonly AssetReference VO_Story_Hero_Cariel_Female_Human_BOM_Scabbs_Mission7ExchangeD_Cariel_02 = new AssetReference("VO_Story_Hero_Cariel_Female_Human_BOM_Scabbs_Mission7ExchangeD_Cariel_02.prefab:94e232731f194e3dbd8b279978b3f9f2");

	private static readonly AssetReference VO_Story_Hero_Cariel_Female_Human_BOM_Scabbs_Mission7Select_01 = new AssetReference("VO_Story_Hero_Cariel_Female_Human_BOM_Scabbs_Mission7Select_01.prefab:ced2deb6c80d4be9b3abdd5be5760c8a");

	private static readonly AssetReference VO_Story_Hero_Cookie_Male_Murloc_BOM_Scabbs_Mission7Death_01 = new AssetReference("VO_Story_Hero_Cookie_Male_Murloc_BOM_Scabbs_Mission7Death_01.prefab:79d4128c19434c90b64b978b2d553e3d");

	private static readonly AssetReference VO_Story_Hero_Cookie_Male_Murloc_BOM_Scabbs_Mission7EmoteResponse_01 = new AssetReference("VO_Story_Hero_Cookie_Male_Murloc_BOM_Scabbs_Mission7EmoteResponse_01.prefab:ba380ba488134bc5b4f8effb31f4bb20");

	private static readonly AssetReference VO_Story_Hero_Cookie_Male_Murloc_BOM_Scabbs_Mission7ExchangeB_01 = new AssetReference("VO_Story_Hero_Cookie_Male_Murloc_BOM_Scabbs_Mission7ExchangeB_01.prefab:b15f4b810a024abaa13b1e60017374ae");

	private static readonly AssetReference VO_Story_Hero_Cookie_Male_Murloc_BOM_Scabbs_Mission7ExchangeC_01 = new AssetReference("VO_Story_Hero_Cookie_Male_Murloc_BOM_Scabbs_Mission7ExchangeC_01.prefab:16552ab1bdb84b19994b40e07f7c08d7");

	private static readonly AssetReference VO_Story_Hero_Cookie_Male_Murloc_BOM_Scabbs_Mission7HeroPower_01 = new AssetReference("VO_Story_Hero_Cookie_Male_Murloc_BOM_Scabbs_Mission7HeroPower_01.prefab:8ba6871a0a7547b5a241a47a1c7a1155");

	private static readonly AssetReference VO_Story_Hero_Cookie_Male_Murloc_BOM_Scabbs_Mission7HeroPower_02 = new AssetReference("VO_Story_Hero_Cookie_Male_Murloc_BOM_Scabbs_Mission7HeroPower_02.prefab:ed0e73b031334cbcb9df881c163b7346");

	private static readonly AssetReference VO_Story_Hero_Cookie_Male_Murloc_BOM_Scabbs_Mission7HeroPower_03 = new AssetReference("VO_Story_Hero_Cookie_Male_Murloc_BOM_Scabbs_Mission7HeroPower_03.prefab:bd94192247ba4d67b7197e6999d98de0");

	private static readonly AssetReference VO_Story_Hero_Cookie_Male_Murloc_BOM_Scabbs_Mission7Idle_01 = new AssetReference("VO_Story_Hero_Cookie_Male_Murloc_BOM_Scabbs_Mission7Idle_01.prefab:55ccbbd0786940bfa5b9306abb663849");

	private static readonly AssetReference VO_Story_Hero_Cookie_Male_Murloc_BOM_Scabbs_Mission7Idle_02 = new AssetReference("VO_Story_Hero_Cookie_Male_Murloc_BOM_Scabbs_Mission7Idle_02.prefab:6bc684de19924ff3b3603be8c2b5e1f8");

	private static readonly AssetReference VO_Story_Hero_Cookie_Male_Murloc_BOM_Scabbs_Mission7Idle_03 = new AssetReference("VO_Story_Hero_Cookie_Male_Murloc_BOM_Scabbs_Mission7Idle_03.prefab:d5426a6e7e3449c28f70689d35d58a89");

	private static readonly AssetReference VO_Story_Hero_Cookie_Male_Murloc_BOM_Scabbs_Mission7Intro_02 = new AssetReference("VO_Story_Hero_Cookie_Male_Murloc_BOM_Scabbs_Mission7Intro_02.prefab:3c1bdd70d35b4385bb3e6755adf9cc14");

	private static readonly AssetReference VO_Story_Hero_Cookie_Male_Murloc_BOM_Scabbs_Mission7Loss_01 = new AssetReference("VO_Story_Hero_Cookie_Male_Murloc_BOM_Scabbs_Mission7Loss_01.prefab:bbb6b7731c904724a7243468181b76ba");

	private static readonly AssetReference VO_Story_Hero_Edwin_Male_Human_BOM_Scabbs_Mission7Victory_02 = new AssetReference("VO_Story_Hero_Edwin_Male_Human_BOM_Scabbs_Mission7Victory_02.prefab:b66d60ad6e26c7b4cb7b2a379b82fb71");

	private static readonly AssetReference VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Scabbs_Mission7ExchangeD_Kurtrus_02 = new AssetReference("VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Scabbs_Mission7ExchangeD_Kurtrus_02.prefab:f401e92db15e4920aeb581503947ee7c");

	private static readonly AssetReference VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Scabbs_Mission7Select_01 = new AssetReference("VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Scabbs_Mission7Select_01.prefab:59d78f15cd9b425a9254779ec1fa3334");

	private static readonly AssetReference VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission7ExchangeA_01 = new AssetReference("VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission7ExchangeA_01.prefab:de9a7ceb2f344bf29a20cecbfbcc19d5");

	private static readonly AssetReference VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission7ExchangeB_02 = new AssetReference("VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission7ExchangeB_02.prefab:0993a92d67e2469a85cce6f58a153dd9");

	private static readonly AssetReference VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission7ExchangeC_02 = new AssetReference("VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission7ExchangeC_02.prefab:14165831379a44ebb6b30576dfb3651b");

	private static readonly AssetReference VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission7ExchangeD_01 = new AssetReference("VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission7ExchangeD_01.prefab:f3c8b3eca08e405cac2d1b7495d025af");

	private static readonly AssetReference VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission7Intro_01 = new AssetReference("VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission7Intro_01.prefab:dda9f210ca8a48589be8661bc52bf07e");

	private static readonly AssetReference VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission7Victory_01 = new AssetReference("VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission7Victory_01.prefab:b9cca3dc510e4282964e92112ff9e009");

	private static readonly AssetReference VO_Story_Hero_Tavish_Male_Dwarf_BOM_Scabbs_Mission7ExchangeD_Tavish_02 = new AssetReference("VO_Story_Hero_Tavish_Male_Dwarf_BOM_Scabbs_Mission7ExchangeD_Tavish_02.prefab:e878fb0e05724118894de76cdcda82c6");

	private static readonly AssetReference VO_Story_Hero_Tavish_Male_Dwarf_BOM_Scabbs_Mission7Select_01 = new AssetReference("VO_Story_Hero_Tavish_Male_Dwarf_BOM_Scabbs_Mission7Select_01.prefab:dc4cfefc0de7430e9859491bcbfa381b");

	private static readonly AssetReference VO_Story_Hero_Xyrella_Female_Draenei_BOM_Scabbs_Mission7ExchangeD_Xyrella_02 = new AssetReference("VO_Story_Hero_Xyrella_Female_Draenei_BOM_Scabbs_Mission7ExchangeD_Xyrella_02.prefab:47573f6b24174620895895c5069afc33");

	private static readonly AssetReference VO_Story_Hero_Xyrella_Female_Draenei_BOM_Scabbs_Mission7Select_01 = new AssetReference("VO_Story_Hero_Xyrella_Female_Draenei_BOM_Scabbs_Mission7Select_01.prefab:ceb369688c7447f3bc29f5384a1a70aa");

	private static readonly AssetReference VO_Story_Minion_Brukan_Male_Troll_BOM_Scabbs_Mission7Idle_01 = new AssetReference("VO_Story_Minion_Brukan_Male_Troll_BOM_Scabbs_Mission7Idle_01.prefab:a3d37fee630943be8938586aa13b5a38");

	private static readonly AssetReference VO_Story_Minion_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission7Idle_01 = new AssetReference("VO_Story_Minion_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission7Idle_01.prefab:a59f4b2cf2a0473889fa3cf25b160fa9");

	private static readonly AssetReference VO_Story_Minion_Guff_Male_Tauren_BOM_Scabbs_Mission7Idle_01 = new AssetReference("VO_Story_Minion_Guff_Male_Tauren_BOM_Scabbs_Mission7Idle_01.prefab:2ec2c9a585f640f1afe7911bbe900bb1");

	private static readonly AssetReference VO_Story_Minion_Rokara_Female_Orc_BOM_Scabbs_Mission7Idle_01 = new AssetReference("VO_Story_Minion_Rokara_Female_Orc_BOM_Scabbs_Mission7Idle_01.prefab:bd9a74d518e04982aae84c1c944ef912");

	private static readonly AssetReference VO_PVPDR_Hero_Scabbs_Male_Gnome_Death_01 = new AssetReference("VO_PVPDR_Hero_Scabbs_Male_Gnome_Death_01.prefab:85e10a0b219b1974aaa959bd523c554b");

	private static readonly AssetReference VO_Story_Minion_Guff_Male_Tauren_BOM_Scabbs_Mission8GuffIdle_01 = new AssetReference("VO_Story_Minion_Guff_Male_Tauren_BOM_Scabbs_Mission8GuffIdle_01.prefab:f561a92f30c743db8105b996edb9e9d0");

	private List<string> m_VO_Story_Hero_Cariel_Female_Human_BOM_Scabbs_Mission7SelectLines = new List<string> { VO_Story_Hero_Cariel_Female_Human_BOM_Scabbs_Mission7Select_01, VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Scabbs_Mission7Select_01, VO_Story_Hero_Xyrella_Female_Draenei_BOM_Scabbs_Mission7Select_01, VO_Story_Minion_Brukan_Male_Troll_BOM_Scabbs_Mission7Idle_01, VO_Story_Minion_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission7Idle_01, VO_Story_Minion_Guff_Male_Tauren_BOM_Scabbs_Mission7Idle_01, VO_Story_Minion_Guff_Male_Tauren_BOM_Scabbs_Mission8GuffIdle_01, VO_Story_Minion_Rokara_Female_Orc_BOM_Scabbs_Mission7Idle_01 };

	private List<string> m_InGame_BossUsesHeroPowerLines = new List<string> { VO_Story_Hero_Cookie_Male_Murloc_BOM_Scabbs_Mission7HeroPower_01, VO_Story_Hero_Cookie_Male_Murloc_BOM_Scabbs_Mission7HeroPower_02, VO_Story_Hero_Cookie_Male_Murloc_BOM_Scabbs_Mission7HeroPower_03 };

	private List<string> m_InGame_BossIdleLines = new List<string> { VO_Story_Hero_Cookie_Male_Murloc_BOM_Scabbs_Mission7Idle_01, VO_Story_Hero_Cookie_Male_Murloc_BOM_Scabbs_Mission7Idle_02, VO_Story_Hero_Cookie_Male_Murloc_BOM_Scabbs_Mission7Idle_03 };

	private static readonly AssetReference Van_Cleef_BrassRing_Quote = new AssetReference("Van_Cleef_BrassRing_Quote.prefab:931d9487ef9f5694eb8db9dab7836829");

	private HashSet<string> m_playedLines = new HashSet<string>();

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> m_SoundFiles = new List<string>
		{
			VO_Story_Hero_Cariel_Female_Human_BOM_Scabbs_Mission7ExchangeD_Cariel_02, VO_Story_Hero_Cariel_Female_Human_BOM_Scabbs_Mission7Select_01, VO_Story_Hero_Cookie_Male_Murloc_BOM_Scabbs_Mission7Death_01, VO_Story_Hero_Cookie_Male_Murloc_BOM_Scabbs_Mission7EmoteResponse_01, VO_Story_Hero_Cookie_Male_Murloc_BOM_Scabbs_Mission7ExchangeB_01, VO_Story_Hero_Cookie_Male_Murloc_BOM_Scabbs_Mission7ExchangeC_01, VO_Story_Hero_Cookie_Male_Murloc_BOM_Scabbs_Mission7HeroPower_01, VO_Story_Hero_Cookie_Male_Murloc_BOM_Scabbs_Mission7HeroPower_02, VO_Story_Hero_Cookie_Male_Murloc_BOM_Scabbs_Mission7HeroPower_03, VO_Story_Hero_Cookie_Male_Murloc_BOM_Scabbs_Mission7Idle_01,
			VO_Story_Hero_Cookie_Male_Murloc_BOM_Scabbs_Mission7Idle_02, VO_Story_Hero_Cookie_Male_Murloc_BOM_Scabbs_Mission7Idle_03, VO_Story_Hero_Cookie_Male_Murloc_BOM_Scabbs_Mission7Intro_02, VO_Story_Hero_Cookie_Male_Murloc_BOM_Scabbs_Mission7Loss_01, VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Scabbs_Mission7ExchangeD_Kurtrus_02, VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Scabbs_Mission7Select_01, VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission7ExchangeA_01, VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission7ExchangeB_02, VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission7ExchangeC_02, VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission7ExchangeD_01,
			VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission7Intro_01, VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission7Victory_01, VO_Story_Hero_Tavish_Male_Dwarf_BOM_Scabbs_Mission7ExchangeD_Tavish_02, VO_Story_Hero_Tavish_Male_Dwarf_BOM_Scabbs_Mission7Select_01, VO_Story_Hero_Xyrella_Female_Draenei_BOM_Scabbs_Mission7ExchangeD_Xyrella_02, VO_Story_Hero_Xyrella_Female_Draenei_BOM_Scabbs_Mission7Select_01, VO_Story_Minion_Brukan_Male_Troll_BOM_Scabbs_Mission7Idle_01, VO_Story_Minion_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission7Idle_01, VO_Story_Minion_Guff_Male_Tauren_BOM_Scabbs_Mission7Idle_01, VO_Story_Minion_Guff_Male_Tauren_BOM_Scabbs_Mission8GuffIdle_01,
			VO_Story_Minion_Rokara_Female_Orc_BOM_Scabbs_Mission7Idle_01, VO_Story_Hero_Edwin_Male_Human_BOM_Scabbs_Mission7Victory_02, VO_PVPDR_Hero_Scabbs_Male_Gnome_Death_01
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
		Actor friendlyHeroPowerActor = GameState.Get().GetFriendlySidePlayer().GetHeroPower()
			.GetCard()
			.GetActor();
		switch (missionEvent)
		{
		case 516:
			yield return MissionPlaySound(enemyActor, VO_Story_Hero_Cookie_Male_Murloc_BOM_Scabbs_Mission7Death_01);
			break;
		case 517:
			yield return MissionPlayVO(enemyActor, m_InGame_BossIdleLines);
			break;
		case 510:
			yield return MissionPlayVO(enemyActor, m_InGame_BossUsesHeroPowerLines);
			break;
		case 515:
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Cookie_Male_Murloc_BOM_Scabbs_Mission7EmoteResponse_01);
			break;
		case 514:
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission7Intro_01);
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Cookie_Male_Murloc_BOM_Scabbs_Mission7Intro_02);
			break;
		case 507:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Cookie_Male_Murloc_BOM_Scabbs_Mission7Loss_01);
			GameState.Get().SetBusy(busy: false);
			break;
		case 505:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission7Victory_01);
			yield return MissionPlayVO(Van_Cleef_BrassRing_Quote, VO_Story_Hero_Edwin_Male_Human_BOM_Scabbs_Mission7Victory_02);
			GameState.Get().SetBusy(busy: false);
			break;
		case 1001:
			if (HeroPowerIsCariel)
			{
				yield return MissionPlayVO(friendlyHeroPowerActor, VO_Story_Hero_Cariel_Female_Human_BOM_Scabbs_Mission7Select_01);
			}
			if (HeroPowerIsKurtrus)
			{
				yield return MissionPlayVO(friendlyHeroPowerActor, VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Scabbs_Mission7Select_01);
			}
			if (HeroPowerIsTavish)
			{
				yield return MissionPlayVO(friendlyHeroPowerActor, VO_Story_Hero_Tavish_Male_Dwarf_BOM_Scabbs_Mission7Select_01);
			}
			if (HeroPowerIsXyrella)
			{
				yield return MissionPlayVO(friendlyHeroPowerActor, VO_Story_Hero_Xyrella_Female_Draenei_BOM_Scabbs_Mission7Select_01);
			}
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
		Actor friendlyHeroPowerActor = GameState.Get().GetFriendlySidePlayer().GetHeroPower()
			.GetCard()
			.GetActor();
		switch (turn)
		{
		case 3:
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission7ExchangeA_01);
			break;
		case 5:
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Cookie_Male_Murloc_BOM_Scabbs_Mission7ExchangeB_01);
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission7ExchangeB_02);
			break;
		case 7:
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Cookie_Male_Murloc_BOM_Scabbs_Mission7ExchangeC_01);
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission7ExchangeC_02);
			break;
		case 9:
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission7ExchangeD_01);
			if (HeroPowerIsCariel)
			{
				yield return MissionPlayVO(friendlyHeroPowerActor, VO_Story_Hero_Cariel_Female_Human_BOM_Scabbs_Mission7ExchangeD_Cariel_02);
			}
			if (HeroPowerIsKurtrus)
			{
				yield return MissionPlayVO(friendlyHeroPowerActor, VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Scabbs_Mission7ExchangeD_Kurtrus_02);
			}
			if (HeroPowerIsXyrella)
			{
				yield return MissionPlayVO(friendlyHeroPowerActor, VO_Story_Hero_Xyrella_Female_Draenei_BOM_Scabbs_Mission7ExchangeD_Xyrella_02);
			}
			if (HeroPowerIsTavish)
			{
				yield return MissionPlayVO(friendlyHeroPowerActor, VO_Story_Hero_Tavish_Male_Dwarf_BOM_Scabbs_Mission7ExchangeD_Tavish_02);
			}
			break;
		case 13:
			if (HeroPowerIsCariel)
			{
				yield return MissionPlayVO("BOM_07_Scabbs_Brukan_007t", BOM_07_Scabbs_Dungeon.Brukan_20_4_BrassRing_Quote, VO_Story_Minion_Brukan_Male_Troll_BOM_Scabbs_Mission7Idle_01);
			}
			if (HeroPowerIsKurtrus)
			{
				yield return MissionPlayVO("BOM_07_Scabbs_Dawngrasp_007t", BOM_07_Scabbs_Dungeon.SWDawngraspMinion_BrassRing_Quote, VO_Story_Minion_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission7Idle_01);
			}
			if (HeroPowerIsXyrella)
			{
				yield return MissionPlayVO("BOM_07_Scabbs_Guff_007t", BOM_07_Scabbs_Dungeon.Guff_BrassRing_Quote, VO_Story_Minion_Guff_Male_Tauren_BOM_Scabbs_Mission7Idle_01);
			}
			if (HeroPowerIsTavish)
			{
				yield return MissionPlayVO("BOM_07_Scabbs_Rokara_007t", BOM_07_Scabbs_Dungeon.Rokara_B_BrassRing_Quote, VO_Story_Minion_Rokara_Female_Orc_BOM_Scabbs_Mission7Idle_01);
			}
			break;
		}
	}
}
