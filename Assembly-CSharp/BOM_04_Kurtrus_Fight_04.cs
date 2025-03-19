using System.Collections;
using System.Collections.Generic;

public class BOM_04_Kurtrus_Fight_04 : BOM_04_Kurtrus_Dungeon
{
	private static readonly AssetReference VO_Story_Hero_Cariel_Female_Human_Story_Kurtrus_Mission4ExchangeB_01 = new AssetReference("VO_Story_Hero_Cariel_Female_Human_Story_Kurtrus_Mission4ExchangeB_01.prefab:06ff2221f0645f741811de0b8e391efc");

	private static readonly AssetReference VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission4ExchangeA_02 = new AssetReference("VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission4ExchangeA_02.prefab:cabefac0904e14f488ae2ed099536129");

	private static readonly AssetReference VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission4ExchangeD_01 = new AssetReference("VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission4ExchangeD_01.prefab:e0ab1d43ff524f2449799a7676c84e25");

	private static readonly AssetReference VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission4Intro_01 = new AssetReference("VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission4Intro_01.prefab:4828b4ab2e42cd3449ba6bc4befe32b6");

	private static readonly AssetReference VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission4Victory_01 = new AssetReference("VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission4Victory_01.prefab:ad8b52102a48f9e4383645e29c2876ec");

	private static readonly AssetReference VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission4Victory_02 = new AssetReference("VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission4Victory_02.prefab:3fdee1f1a9512d345ae342a882b66279");

	private static readonly AssetReference VO_Story_Hero_Sarilus_Male_Forsaken_Story_Kurtrus_Mission4Death_01 = new AssetReference("VO_Story_Hero_Sarilus_Male_Forsaken_Story_Kurtrus_Mission4Death_01.prefab:8cbab207c72267d4b8efe12af82a6373");

	private static readonly AssetReference VO_Story_Hero_Sarilus_Male_Forsaken_Story_Kurtrus_Mission4EmoteResponse_01 = new AssetReference("VO_Story_Hero_Sarilus_Male_Forsaken_Story_Kurtrus_Mission4EmoteResponse_01.prefab:f717a4f5f10e9444b89babc522ccb3c9");

	private static readonly AssetReference VO_Story_Hero_Sarilus_Male_Forsaken_Story_Kurtrus_Mission4HeroPower_01 = new AssetReference("VO_Story_Hero_Sarilus_Male_Forsaken_Story_Kurtrus_Mission4HeroPower_01.prefab:3ebe57a3b0ae9404ea7a0b862188b63e");

	private static readonly AssetReference VO_Story_Hero_Sarilus_Male_Forsaken_Story_Kurtrus_Mission4HeroPower_02 = new AssetReference("VO_Story_Hero_Sarilus_Male_Forsaken_Story_Kurtrus_Mission4HeroPower_02.prefab:b40b0d83e5567064894d585c9a537e42");

	private static readonly AssetReference VO_Story_Hero_Sarilus_Male_Forsaken_Story_Kurtrus_Mission4HeroPower_03 = new AssetReference("VO_Story_Hero_Sarilus_Male_Forsaken_Story_Kurtrus_Mission4HeroPower_03.prefab:188b0959802769b4fa8ffcef7b2b1924");

	private static readonly AssetReference VO_Story_Hero_Sarilus_Male_Forsaken_Story_Kurtrus_Mission4Idle_01 = new AssetReference("VO_Story_Hero_Sarilus_Male_Forsaken_Story_Kurtrus_Mission4Idle_01.prefab:1a47a6534f4b5414e97743d4fdd9e39e");

	private static readonly AssetReference VO_Story_Hero_Sarilus_Male_Forsaken_Story_Kurtrus_Mission4Idle_02 = new AssetReference("VO_Story_Hero_Sarilus_Male_Forsaken_Story_Kurtrus_Mission4Idle_02.prefab:f56048990a02b3942a491773c7880223");

	private static readonly AssetReference VO_Story_Hero_Sarilus_Male_Forsaken_Story_Kurtrus_Mission4Idle_03 = new AssetReference("VO_Story_Hero_Sarilus_Male_Forsaken_Story_Kurtrus_Mission4Idle_03.prefab:f1cf7a7f2c2e2d44ea0b8726dc79396e");

	private static readonly AssetReference VO_Story_Hero_Sarilus_Male_Forsaken_Story_Kurtrus_Mission4Intro_02 = new AssetReference("VO_Story_Hero_Sarilus_Male_Forsaken_Story_Kurtrus_Mission4Intro_02.prefab:64ef8e7afbe0df74d940aee13688aad5");

	private static readonly AssetReference VO_Story_Hero_Sarilus_Male_Forsaken_Story_Kurtrus_Mission4Loss_01 = new AssetReference("VO_Story_Hero_Sarilus_Male_Forsaken_Story_Kurtrus_Mission4Loss_01.prefab:1a64b282ad778bb428b65ce5ce6287c9");

	private static readonly AssetReference VO_Story_Hero_Scabbs_Male_Gnome_Story_Kurtrus_Mission4ExchangeB_02 = new AssetReference("VO_Story_Hero_Scabbs_Male_Gnome_Story_Kurtrus_Mission4ExchangeB_02.prefab:1855867ee38048640ad24af470025735");

	private static readonly AssetReference VO_Story_Hero_Tavish_Male_Dwarf_Story_Kurtrus_Mission4ExchangeB_03 = new AssetReference("VO_Story_Hero_Tavish_Male_Dwarf_Story_Kurtrus_Mission4ExchangeB_03.prefab:7c4366d78fc37f54b8370ecd7ff5f78a");

	private static readonly AssetReference VO_Story_Hero_Tavish_Male_Dwarf_Story_Kurtrus_Mission4ExchangeD_03 = new AssetReference("VO_Story_Hero_Tavish_Male_Dwarf_Story_Kurtrus_Mission4ExchangeD_03.prefab:8dd3ecff652258449b62c6c8caad5277");

	private static readonly AssetReference VO_Story_Hero_Xyrella_Female_Draenei_Story_Kurtrus_Mission4ExchangeA_01 = new AssetReference("VO_Story_Hero_Xyrella_Female_Draenei_Story_Kurtrus_Mission4ExchangeA_01.prefab:78e44ec9c6b8cf245b1935ead141de5f");

	private static readonly AssetReference VO_Story_Hero_Xyrella_Female_Draenei_Story_Kurtrus_Mission4ExchangeC_01 = new AssetReference("VO_Story_Hero_Xyrella_Female_Draenei_Story_Kurtrus_Mission4ExchangeC_01.prefab:d4f7434a807781241b98e6b56f26f961");

	private static readonly AssetReference VO_Story_Hero_Xyrella_Female_Draenei_Story_Kurtrus_Mission4ExchangeC_02 = new AssetReference("VO_Story_Hero_Xyrella_Female_Draenei_Story_Kurtrus_Mission4ExchangeC_02.prefab:e4343a6256278f446a4c10df24b76621");

	private static readonly AssetReference VO_Story_Hero_Xyrella_Female_Draenei_Story_Kurtrus_Mission4ExchangeD_02 = new AssetReference("VO_Story_Hero_Xyrella_Female_Draenei_Story_Kurtrus_Mission4ExchangeD_02.prefab:981d93a600992154aaa775cfe5db698c");

	private List<string> m_missionEventTriggerInGame_VictoryPreExplosionLines = new List<string> { VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission4Victory_01, VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission4Victory_02 };

	private List<string> m_VO_Story_Hero_Xyrella_Female_Draenei_Story_Kurtrus_Mission4ExchangeCLines = new List<string> { VO_Story_Hero_Xyrella_Female_Draenei_Story_Kurtrus_Mission4ExchangeC_01, VO_Story_Hero_Xyrella_Female_Draenei_Story_Kurtrus_Mission4ExchangeC_02 };

	private List<string> m_InGame_BossIdleLines = new List<string> { VO_Story_Hero_Sarilus_Male_Forsaken_Story_Kurtrus_Mission4Idle_01, VO_Story_Hero_Sarilus_Male_Forsaken_Story_Kurtrus_Mission4Idle_02, VO_Story_Hero_Sarilus_Male_Forsaken_Story_Kurtrus_Mission4Idle_03 };

	private List<string> m_InGame_BossHeroPowerLines = new List<string> { VO_Story_Hero_Sarilus_Male_Forsaken_Story_Kurtrus_Mission4HeroPower_01, VO_Story_Hero_Sarilus_Male_Forsaken_Story_Kurtrus_Mission4HeroPower_02, VO_Story_Hero_Sarilus_Male_Forsaken_Story_Kurtrus_Mission4HeroPower_03 };

	private HashSet<string> m_playedLines = new HashSet<string>();

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> m_SoundFiles = new List<string>
		{
			VO_Story_Hero_Cariel_Female_Human_Story_Kurtrus_Mission4ExchangeB_01, VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission4ExchangeA_02, VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission4ExchangeD_01, VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission4Intro_01, VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission4Victory_01, VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission4Victory_02, VO_Story_Hero_Sarilus_Male_Forsaken_Story_Kurtrus_Mission4Death_01, VO_Story_Hero_Sarilus_Male_Forsaken_Story_Kurtrus_Mission4EmoteResponse_01, VO_Story_Hero_Sarilus_Male_Forsaken_Story_Kurtrus_Mission4HeroPower_01, VO_Story_Hero_Sarilus_Male_Forsaken_Story_Kurtrus_Mission4HeroPower_02,
			VO_Story_Hero_Sarilus_Male_Forsaken_Story_Kurtrus_Mission4HeroPower_03, VO_Story_Hero_Sarilus_Male_Forsaken_Story_Kurtrus_Mission4Idle_01, VO_Story_Hero_Sarilus_Male_Forsaken_Story_Kurtrus_Mission4Idle_02, VO_Story_Hero_Sarilus_Male_Forsaken_Story_Kurtrus_Mission4Idle_03, VO_Story_Hero_Sarilus_Male_Forsaken_Story_Kurtrus_Mission4Intro_02, VO_Story_Hero_Sarilus_Male_Forsaken_Story_Kurtrus_Mission4Loss_01, VO_Story_Hero_Scabbs_Male_Gnome_Story_Kurtrus_Mission4ExchangeB_02, VO_Story_Hero_Tavish_Male_Dwarf_Story_Kurtrus_Mission4ExchangeB_03, VO_Story_Hero_Tavish_Male_Dwarf_Story_Kurtrus_Mission4ExchangeD_03, VO_Story_Hero_Xyrella_Female_Draenei_Story_Kurtrus_Mission4ExchangeA_01,
			VO_Story_Hero_Xyrella_Female_Draenei_Story_Kurtrus_Mission4ExchangeC_01, VO_Story_Hero_Xyrella_Female_Draenei_Story_Kurtrus_Mission4ExchangeC_02, VO_Story_Hero_Xyrella_Female_Draenei_Story_Kurtrus_Mission4ExchangeD_02
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
		case 514:
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission4Intro_01);
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Sarilus_Male_Forsaken_Story_Kurtrus_Mission4Intro_02);
			break;
		case 506:
			MissionPause(pause: true);
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Sarilus_Male_Forsaken_Story_Kurtrus_Mission4Loss_01);
			MissionPause(pause: false);
			break;
		case 505:
			MissionPause(pause: true);
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission4Victory_01);
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission4Victory_02);
			MissionPause(pause: false);
			break;
		case 517:
			yield return MissionPlayVO(enemyActor, m_InGame_BossIdleLines);
			break;
		case 515:
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Sarilus_Male_Forsaken_Story_Kurtrus_Mission4EmoteResponse_01);
			break;
		case 510:
			yield return MissionPlayVO(enemyActor, m_InGame_BossHeroPowerLines);
			break;
		case 516:
			yield return MissionPlaySound(enemyActor, VO_Story_Hero_Sarilus_Male_Forsaken_Story_Kurtrus_Mission4Death_01);
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
		GameState.Get().GetOpposingSidePlayer().GetHero()
			.GetCard()
			.GetActor();
		Actor friendlyActor = GameState.Get().GetFriendlySidePlayer().GetHero()
			.GetCard()
			.GetActor();
		switch (turn)
		{
		case 1:
			yield return MissionPlayVO("BOM_04_Xyrella_004t", VO_Story_Hero_Xyrella_Female_Draenei_Story_Kurtrus_Mission4ExchangeA_01);
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission4ExchangeA_02);
			break;
		case 5:
			yield return MissionPlayVO("BOM_04_Cariel_001t", VO_Story_Hero_Cariel_Female_Human_Story_Kurtrus_Mission4ExchangeB_01);
			yield return MissionPlayVO("BOM_04_Scabbs_004t", VO_Story_Hero_Scabbs_Male_Gnome_Story_Kurtrus_Mission4ExchangeB_02);
			yield return MissionPlayVO("BOM_04_Tavish_004t", VO_Story_Hero_Tavish_Male_Dwarf_Story_Kurtrus_Mission4ExchangeB_03);
			break;
		case 9:
			yield return MissionPlayVO("BOM_04_Xyrella_004t", VO_Story_Hero_Xyrella_Female_Draenei_Story_Kurtrus_Mission4ExchangeC_01);
			yield return MissionPlayVO("BOM_04_Xyrella_004t", VO_Story_Hero_Xyrella_Female_Draenei_Story_Kurtrus_Mission4ExchangeC_02);
			break;
		case 13:
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission4ExchangeD_01);
			yield return MissionPlayVO("BOM_04_Xyrella_004t", VO_Story_Hero_Xyrella_Female_Draenei_Story_Kurtrus_Mission4ExchangeD_02);
			yield return MissionPlayVO("BOM_04_Tavish_004t", VO_Story_Hero_Tavish_Male_Dwarf_Story_Kurtrus_Mission4ExchangeD_03);
			break;
		}
	}
}
