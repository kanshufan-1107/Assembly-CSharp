using System.Collections;
using System.Collections.Generic;

public class BOM_06_Cariel_Fight_002 : BOM_06_Cariel_Dungeon
{
	private static readonly AssetReference VO_Story_Hero_Cariel_Female_Human_BOM_Cariel_Mission2ExchangeA_01 = new AssetReference("VO_Story_Hero_Cariel_Female_Human_BOM_Cariel_Mission2ExchangeA_01.prefab:d68dec253b6c35f46b39dce5e8fffbeb");

	private static readonly AssetReference VO_Story_Hero_Cariel_Female_Human_BOM_Cariel_Mission2ExchangeC_01 = new AssetReference("VO_Story_Hero_Cariel_Female_Human_BOM_Cariel_Mission2ExchangeC_01.prefab:390d3cf5bec720640b9fd6c0c7b55798");

	private static readonly AssetReference VO_Story_Hero_Cariel_Female_Human_BOM_Cariel_Mission2ExchangeD_01 = new AssetReference("VO_Story_Hero_Cariel_Female_Human_BOM_Cariel_Mission2ExchangeD_01.prefab:e867830dedb5f0746ac9e906baf14de4");

	private static readonly AssetReference VO_Story_Hero_Cariel_Female_Human_BOM_Cariel_Mission2Intro_01 = new AssetReference("VO_Story_Hero_Cariel_Female_Human_BOM_Cariel_Mission2Intro_01.prefab:cca7154c19e17304b97798f7d77c7b07");

	private static readonly AssetReference VO_Story_Hero_Cariel_Female_Human_BOM_Cariel_Mission2Victory_01 = new AssetReference("VO_Story_Hero_Cariel_Female_Human_BOM_Cariel_Mission2Victory_01.prefab:b1e011be3761e64459e13d0737a9faad");

	private static readonly AssetReference VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Cariel_Mission2Death_01 = new AssetReference("VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Cariel_Mission2Death_01.prefab:a8fbba6c26e78dc4eba4f4b307d39049");

	private static readonly AssetReference VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Cariel_Mission2EmoteResponse_01 = new AssetReference("VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Cariel_Mission2EmoteResponse_01.prefab:447ef5cc84981fa49918ed7856b03024");

	private static readonly AssetReference VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Cariel_Mission2ExchangeA_01 = new AssetReference("VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Cariel_Mission2ExchangeA_01.prefab:82ec42a577803144bb0dcf829d9b45bd");

	private static readonly AssetReference VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Cariel_Mission2ExchangeB_01 = new AssetReference("VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Cariel_Mission2ExchangeB_01.prefab:aa965ec8f5eab3648b6c474f5230ab99");

	private static readonly AssetReference VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Cariel_Mission2ExchangeC_02 = new AssetReference("VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Cariel_Mission2ExchangeC_02.prefab:32c35ab51138c194f9e4e2b069727cb8");

	private static readonly AssetReference VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Cariel_Mission2ExchangeD_02 = new AssetReference("VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Cariel_Mission2ExchangeD_02.prefab:3aa0f20b1fca5e640b8ffeb6f8670aca");

	private static readonly AssetReference VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Cariel_Mission2HeroPower_01 = new AssetReference("VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Cariel_Mission2HeroPower_01.prefab:1e44cfca9838a184fa871d91145f0bff");

	private static readonly AssetReference VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Cariel_Mission2HeroPower_02 = new AssetReference("VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Cariel_Mission2HeroPower_02.prefab:42cce615c58313d40b7ac796b5b465fb");

	private static readonly AssetReference VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Cariel_Mission2HeroPower_03 = new AssetReference("VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Cariel_Mission2HeroPower_03.prefab:a1d6ee21d50366f4e9337d314f69c12d");

	private static readonly AssetReference VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Cariel_Mission2Idle_01 = new AssetReference("VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Cariel_Mission2Idle_01.prefab:da4e728c540eed541980e9021070a34a");

	private static readonly AssetReference VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Cariel_Mission2Idle_02 = new AssetReference("VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Cariel_Mission2Idle_02.prefab:ffaaa3d0a1846b542bac651fb0ba971a");

	private static readonly AssetReference VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Cariel_Mission2Idle_03 = new AssetReference("VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Cariel_Mission2Idle_03.prefab:38eb6ffea817dc34e904f1e615d46e2e");

	private static readonly AssetReference VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Cariel_Mission2Intro_02 = new AssetReference("VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Cariel_Mission2Intro_02.prefab:858c9821dcd865b49859f4183e4dec15");

	private static readonly AssetReference VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Cariel_Mission2Loss_01 = new AssetReference("VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Cariel_Mission2Loss_01.prefab:782f90cd21a72204aa5e1203ca87056c");

	private static readonly AssetReference VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Cariel_Mission2Victory_02 = new AssetReference("VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Cariel_Mission2Victory_02.prefab:ed320e2598ab7c24e945ea28b651fe30");

	private static readonly AssetReference VO_PVPDR_Hero_Cariel_Female_Human_Death_01 = new AssetReference("VO_PVPDR_Hero_Cariel_Female_Human_Death_01.prefab:523b6a53885a78244ab354f929dbd2b0");

	private List<string> m_InGame_BossUsesHeroPowerLines = new List<string> { VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Cariel_Mission2HeroPower_01, VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Cariel_Mission2HeroPower_02, VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Cariel_Mission2HeroPower_03 };

	private List<string> m_InGame_BossIdleLines = new List<string> { VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Cariel_Mission2Idle_01, VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Cariel_Mission2Idle_02, VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Cariel_Mission2Idle_03 };

	private HashSet<string> m_playedLines = new HashSet<string>();

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> m_SoundFiles = new List<string>
		{
			VO_Story_Hero_Cariel_Female_Human_BOM_Cariel_Mission2ExchangeA_01, VO_Story_Hero_Cariel_Female_Human_BOM_Cariel_Mission2ExchangeC_01, VO_Story_Hero_Cariel_Female_Human_BOM_Cariel_Mission2ExchangeD_01, VO_Story_Hero_Cariel_Female_Human_BOM_Cariel_Mission2Intro_01, VO_Story_Hero_Cariel_Female_Human_BOM_Cariel_Mission2Victory_01, VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Cariel_Mission2Death_01, VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Cariel_Mission2EmoteResponse_01, VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Cariel_Mission2ExchangeA_01, VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Cariel_Mission2ExchangeB_01, VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Cariel_Mission2ExchangeC_02,
			VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Cariel_Mission2ExchangeD_02, VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Cariel_Mission2HeroPower_01, VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Cariel_Mission2HeroPower_02, VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Cariel_Mission2HeroPower_03, VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Cariel_Mission2Idle_01, VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Cariel_Mission2Idle_02, VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Cariel_Mission2Idle_03, VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Cariel_Mission2Intro_02, VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Cariel_Mission2Loss_01, VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Cariel_Mission2Victory_02,
			VO_PVPDR_Hero_Cariel_Female_Human_Death_01
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
		m_OverrideMusicTrack = MusicPlaylistType.InGame_BAR;
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
		case 519:
			yield return MissionPlaySound(enemyActor, VO_PVPDR_Hero_Cariel_Female_Human_Death_01);
			break;
		case 517:
			yield return MissionPlayVO(enemyActor, m_InGame_BossIdleLines);
			break;
		case 510:
			yield return MissionPlayVO(enemyActor, m_InGame_BossUsesHeroPowerLines);
			break;
		case 515:
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Cariel_Mission2EmoteResponse_01);
			break;
		case 514:
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Cariel_Female_Human_BOM_Cariel_Mission2Intro_01);
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Cariel_Mission2Intro_02);
			break;
		case 507:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Cariel_Mission2Loss_01);
			GameState.Get().SetBusy(busy: false);
			break;
		case 504:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Cariel_Female_Human_BOM_Cariel_Mission2Victory_01);
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Cariel_Mission2Victory_02);
			GameState.Get().SetBusy(busy: false);
			break;
		default:
			yield return base.HandleMissionEventWithTiming(missionEvent);
			break;
		case 516:
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
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Cariel_Female_Human_BOM_Cariel_Mission2ExchangeA_01);
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Cariel_Mission2ExchangeA_01);
			break;
		case 7:
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Cariel_Mission2ExchangeB_01);
			break;
		case 11:
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Cariel_Female_Human_BOM_Cariel_Mission2ExchangeC_01);
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Cariel_Mission2ExchangeC_02);
			break;
		case 15:
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Cariel_Female_Human_BOM_Cariel_Mission2ExchangeD_01);
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Cariel_Mission2ExchangeD_02);
			break;
		}
	}
}
