using System.Collections;
using System.Collections.Generic;

public class BOM_04_Kurtrus_Fight_07 : BOM_04_Kurtrus_Dungeon
{
	private static readonly AssetReference VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission7ExchangeA_02 = new AssetReference("VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission7ExchangeA_02.prefab:bd086cbff853f3344aa8278ed95e7e06");

	private static readonly AssetReference VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission7ExchangeC_01 = new AssetReference("VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission7ExchangeC_01.prefab:dfa337e5236069a4f997aac3d461e8bc");

	private static readonly AssetReference VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission7Intro_01 = new AssetReference("VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission7Intro_01.prefab:4b388879601ac1846b4f007b51f6e106");

	private static readonly AssetReference VO_Story_Hero_Neeru_Male_Orc_Story_Kurtrus_Mission7Death_01 = new AssetReference("VO_Story_Hero_Neeru_Male_Orc_Story_Kurtrus_Mission7Death_01.prefab:82ef51bddcc36c6428db7199870653fe");

	private static readonly AssetReference VO_Story_Hero_Neeru_Male_Orc_Story_Kurtrus_Mission7EmoteResponse_01 = new AssetReference("VO_Story_Hero_Neeru_Male_Orc_Story_Kurtrus_Mission7EmoteResponse_01.prefab:4c074a5ff59d9c241bdb186802a1dcec");

	private static readonly AssetReference VO_Story_Hero_Neeru_Male_Orc_Story_Kurtrus_Mission7ExchangeA_01 = new AssetReference("VO_Story_Hero_Neeru_Male_Orc_Story_Kurtrus_Mission7ExchangeA_01.prefab:6cc7b17da2a3ae44d8951afa81a11d1e");

	private static readonly AssetReference VO_Story_Hero_Neeru_Male_Orc_Story_Kurtrus_Mission7ExchangeB_02 = new AssetReference("VO_Story_Hero_Neeru_Male_Orc_Story_Kurtrus_Mission7ExchangeB_02.prefab:3182a0c9603484f45a275bda93a2bc5c");

	private static readonly AssetReference VO_Story_Hero_Neeru_Male_Orc_Story_Kurtrus_Mission7ExchangeC_02 = new AssetReference("VO_Story_Hero_Neeru_Male_Orc_Story_Kurtrus_Mission7ExchangeC_02.prefab:c2b8afe65ef23c143bd1a34161cc187d");

	private static readonly AssetReference VO_Story_Hero_Neeru_Male_Orc_Story_Kurtrus_Mission7ExchangeD_01 = new AssetReference("VO_Story_Hero_Neeru_Male_Orc_Story_Kurtrus_Mission7ExchangeD_01.prefab:4ce71ffab55fb1843bc5c3cc9fb31425");

	private static readonly AssetReference VO_Story_Hero_Neeru_Male_Orc_Story_Kurtrus_Mission7ExchangeE_01 = new AssetReference("VO_Story_Hero_Neeru_Male_Orc_Story_Kurtrus_Mission7ExchangeE_01.prefab:3400e6bf814086f42bce1e50b16a30b3");

	private static readonly AssetReference VO_Story_Hero_Neeru_Male_Orc_Story_Kurtrus_Mission7HeroPower_01 = new AssetReference("VO_Story_Hero_Neeru_Male_Orc_Story_Kurtrus_Mission7HeroPower_01.prefab:007b130ddcc08ca4e9924be455bc2734");

	private static readonly AssetReference VO_Story_Hero_Neeru_Male_Orc_Story_Kurtrus_Mission7HeroPower_02 = new AssetReference("VO_Story_Hero_Neeru_Male_Orc_Story_Kurtrus_Mission7HeroPower_02.prefab:8bc50b80bf51d6844b8cc4c89573eaaf");

	private static readonly AssetReference VO_Story_Hero_Neeru_Male_Orc_Story_Kurtrus_Mission7HeroPower_03 = new AssetReference("VO_Story_Hero_Neeru_Male_Orc_Story_Kurtrus_Mission7HeroPower_03.prefab:f05a7e6f1db294347a677c9c1cdb14fd");

	private static readonly AssetReference VO_Story_Hero_Neeru_Male_Orc_Story_Kurtrus_Mission7Idle_01 = new AssetReference("VO_Story_Hero_Neeru_Male_Orc_Story_Kurtrus_Mission7Idle_01.prefab:88b0f8abdf5585f47a6e744d8391fbf0");

	private static readonly AssetReference VO_Story_Hero_Neeru_Male_Orc_Story_Kurtrus_Mission7Idle_02 = new AssetReference("VO_Story_Hero_Neeru_Male_Orc_Story_Kurtrus_Mission7Idle_02.prefab:0bb351ffc71261d49a7c311b5c08f9dd");

	private static readonly AssetReference VO_Story_Hero_Neeru_Male_Orc_Story_Kurtrus_Mission7Idle_03 = new AssetReference("VO_Story_Hero_Neeru_Male_Orc_Story_Kurtrus_Mission7Idle_03.prefab:c7e17e42871307f4bae3b2d6be0f3fc3");

	private static readonly AssetReference VO_Story_Hero_Neeru_Male_Orc_Story_Kurtrus_Mission7Intro_02 = new AssetReference("VO_Story_Hero_Neeru_Male_Orc_Story_Kurtrus_Mission7Intro_02.prefab:a61ed411a770f704d8c62b7834d1b5d2");

	private static readonly AssetReference VO_Story_Hero_Neeru_Male_Orc_Story_Kurtrus_Mission7Loss_01 = new AssetReference("VO_Story_Hero_Neeru_Male_Orc_Story_Kurtrus_Mission7Loss_01.prefab:e1544fc02d4059748aac550938999bac");

	private static readonly AssetReference VO_Story_Hero_Tamsin_Female_Forsaken_Story_Kurtrus_Mission7Victory_01 = new AssetReference("VO_Story_Hero_Tamsin_Female_Forsaken_Story_Kurtrus_Mission7Victory_01.prefab:89898c1f4c5cc9941a0f6310e62fc48f");

	private static readonly AssetReference VO_Story_Hero_Tamsin_Female_Forsaken_Story_Kurtrus_Mission7Victory_03 = new AssetReference("VO_Story_Hero_Tamsin_Female_Forsaken_Story_Kurtrus_Mission7Victory_03.prefab:212012bf245530a448cd29937321fa09");

	private static readonly AssetReference VO_Story_Hero_Cariel_Female_Human_Story_Kurtrus_Mission7ExchangeB_01 = new AssetReference("VO_Story_Hero_Cariel_Female_Human_Story_Kurtrus_Mission7ExchangeB_01.prefab:ebfb9c80940a6b343a4714aa6b401b23");

	private static readonly AssetReference VO_Story_Hero_Cariel_Female_Human_Story_Kurtrus_Mission7ExchangeB_03 = new AssetReference("VO_Story_Hero_Cariel_Female_Human_Story_Kurtrus_Mission7ExchangeB_03.prefab:e2bd7aa00d706be40ac5aa1ff0b21379");

	private static readonly AssetReference VO_Story_Hero_Cariel_Female_Human_Story_Kurtrus_Mission7Victory_02 = new AssetReference("VO_Story_Hero_Cariel_Female_Human_Story_Kurtrus_Mission7Victory_02.prefab:37d73c2fca247e249b3984b60e9d4d73");

	private List<string> m_missionEventTriggerInGame_VictoryPreExplosionLines = new List<string> { VO_Story_Hero_Tamsin_Female_Forsaken_Story_Kurtrus_Mission7Victory_01, VO_Story_Hero_Tamsin_Female_Forsaken_Story_Kurtrus_Mission7Victory_03 };

	private List<string> m_InGame_BossIdleLines = new List<string> { VO_Story_Hero_Neeru_Male_Orc_Story_Kurtrus_Mission7Idle_01, VO_Story_Hero_Neeru_Male_Orc_Story_Kurtrus_Mission7Idle_02, VO_Story_Hero_Neeru_Male_Orc_Story_Kurtrus_Mission7Idle_03 };

	private List<string> m_InGame_BossHeroPowerLines = new List<string> { VO_Story_Hero_Neeru_Male_Orc_Story_Kurtrus_Mission7HeroPower_01, VO_Story_Hero_Neeru_Male_Orc_Story_Kurtrus_Mission7HeroPower_02, VO_Story_Hero_Neeru_Male_Orc_Story_Kurtrus_Mission7HeroPower_03 };

	private List<string> m_InGame_BossPlaysAltarOfFire = new List<string> { VO_Story_Hero_Neeru_Male_Orc_Story_Kurtrus_Mission7ExchangeD_01, VO_Story_Hero_Neeru_Male_Orc_Story_Kurtrus_Mission7ExchangeE_01 };

	private HashSet<string> m_playedLines = new HashSet<string>();

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> m_SoundFiles = new List<string>
		{
			VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission7ExchangeA_02, VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission7ExchangeC_01, VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission7Intro_01, VO_Story_Hero_Neeru_Male_Orc_Story_Kurtrus_Mission7Death_01, VO_Story_Hero_Neeru_Male_Orc_Story_Kurtrus_Mission7EmoteResponse_01, VO_Story_Hero_Neeru_Male_Orc_Story_Kurtrus_Mission7ExchangeA_01, VO_Story_Hero_Neeru_Male_Orc_Story_Kurtrus_Mission7ExchangeB_02, VO_Story_Hero_Neeru_Male_Orc_Story_Kurtrus_Mission7ExchangeC_02, VO_Story_Hero_Neeru_Male_Orc_Story_Kurtrus_Mission7ExchangeD_01, VO_Story_Hero_Neeru_Male_Orc_Story_Kurtrus_Mission7ExchangeE_01,
			VO_Story_Hero_Neeru_Male_Orc_Story_Kurtrus_Mission7HeroPower_01, VO_Story_Hero_Neeru_Male_Orc_Story_Kurtrus_Mission7HeroPower_02, VO_Story_Hero_Neeru_Male_Orc_Story_Kurtrus_Mission7HeroPower_03, VO_Story_Hero_Neeru_Male_Orc_Story_Kurtrus_Mission7Idle_01, VO_Story_Hero_Neeru_Male_Orc_Story_Kurtrus_Mission7Idle_02, VO_Story_Hero_Neeru_Male_Orc_Story_Kurtrus_Mission7Idle_03, VO_Story_Hero_Neeru_Male_Orc_Story_Kurtrus_Mission7Intro_02, VO_Story_Hero_Neeru_Male_Orc_Story_Kurtrus_Mission7Loss_01, VO_Story_Hero_Tamsin_Female_Forsaken_Story_Kurtrus_Mission7Victory_01, VO_Story_Hero_Tamsin_Female_Forsaken_Story_Kurtrus_Mission7Victory_03,
			VO_Story_Hero_Cariel_Female_Human_Story_Kurtrus_Mission7ExchangeB_01, VO_Story_Hero_Cariel_Female_Human_Story_Kurtrus_Mission7ExchangeB_03, VO_Story_Hero_Cariel_Female_Human_Story_Kurtrus_Mission7Victory_02
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
		m_OverrideMusicTrack = MusicPlaylistType.InGame_GILFinalBoss;
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
		case 514:
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission7Intro_01);
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Neeru_Male_Orc_Story_Kurtrus_Mission7Intro_02);
			break;
		case 506:
			MissionPause(pause: true);
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Neeru_Male_Orc_Story_Kurtrus_Mission7Loss_01);
			MissionPause(pause: false);
			break;
		case 505:
			MissionPause(pause: true);
			yield return MissionPlayVO(Tamsin_BrassRing, VO_Story_Hero_Tamsin_Female_Forsaken_Story_Kurtrus_Mission7Victory_01);
			if (CarielIsHeroPower)
			{
				yield return MissionPlayVO(friendlyHeroPowerActor, VO_Story_Hero_Cariel_Female_Human_Story_Kurtrus_Mission7Victory_02);
			}
			else
			{
				yield return MissionPlayVO(Cariel_BrassRing_Quote, VO_Story_Hero_Cariel_Female_Human_Story_Kurtrus_Mission7Victory_02);
			}
			yield return MissionPlayVO(Tamsin_BrassRing, VO_Story_Hero_Tamsin_Female_Forsaken_Story_Kurtrus_Mission7Victory_03);
			MissionPause(pause: false);
			break;
		case 517:
			yield return MissionPlayVO(enemyActor, m_InGame_BossIdleLines);
			break;
		case 515:
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Neeru_Male_Orc_Story_Kurtrus_Mission7EmoteResponse_01);
			break;
		case 510:
			yield return MissionPlayVO(enemyActor, m_InGame_BossHeroPowerLines);
			break;
		case 516:
			yield return MissionPlaySound(enemyActor, VO_Story_Hero_Neeru_Male_Orc_Story_Kurtrus_Mission7Death_01);
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
			Actor enemyActor = GameState.Get().GetOpposingSidePlayer().GetHero()
				.GetCard()
				.GetActor();
			GameState.Get().GetFriendlySidePlayer().GetHero()
				.GetCard()
				.GetActor();
			if (cardID == "BAR_913")
			{
				yield return MissionPlayVOOnceInOrder(enemyActor, m_InGame_BossPlaysAltarOfFire);
			}
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
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Neeru_Male_Orc_Story_Kurtrus_Mission7ExchangeA_01);
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission7ExchangeA_02);
			break;
		case 7:
			yield return MissionPlayVO("BOM_04_Cariel_001t", VO_Story_Hero_Cariel_Female_Human_Story_Kurtrus_Mission7ExchangeB_01);
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Neeru_Male_Orc_Story_Kurtrus_Mission7ExchangeB_02);
			yield return MissionPlayVO("BOM_04_Cariel_001t", VO_Story_Hero_Cariel_Female_Human_Story_Kurtrus_Mission7ExchangeB_03);
			break;
		case 11:
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission7ExchangeC_01);
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Neeru_Male_Orc_Story_Kurtrus_Mission7ExchangeC_02);
			break;
		}
	}
}
