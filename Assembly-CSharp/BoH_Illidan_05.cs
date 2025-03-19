using System.Collections;
using System.Collections.Generic;
using Blizzard.T5.Core;

public class BoH_Illidan_05 : BoH_Illidan_Dungeon
{
	private static Map<GameEntityOption, bool> s_booleanOptions = InitBooleanOptions();

	private static readonly AssetReference VO_Story_Hero_Akama_Male_Draenei_Story_Illidan_Mission5ExchangeB_01 = new AssetReference("VO_Story_Hero_Akama_Male_Draenei_Story_Illidan_Mission5ExchangeB_01.prefab:1bfb14fb2bb8e4143ab9b86de643bfb6");

	private static readonly AssetReference VO_Story_Hero_Akama_Male_Draenei_Story_Illidan_Mission5ExchangeB_03 = new AssetReference("VO_Story_Hero_Akama_Male_Draenei_Story_Illidan_Mission5ExchangeB_03.prefab:bd52d557bc3080d4e920758eca32aac4");

	private static readonly AssetReference VO_Story_Hero_Akama_Male_Draenei_Story_Illidan_Mission5ExchangeE_02 = new AssetReference("VO_Story_Hero_Akama_Male_Draenei_Story_Illidan_Mission5ExchangeE_02.prefab:fda5a5ba0fae18c4296ef94e2cb15c23");

	private static readonly AssetReference VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission5ExchangeA_02 = new AssetReference("VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission5ExchangeA_02.prefab:97c7cc7b3017e0f40a861de6b64a8634");

	private static readonly AssetReference VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission5ExchangeB_02 = new AssetReference("VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission5ExchangeB_02.prefab:7812ea15e9131964b8169b64d3088fc9");

	private static readonly AssetReference VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission5ExchangeC_01 = new AssetReference("VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission5ExchangeC_01.prefab:a201f9101d873134e8ea8492c2e6b8f1");

	private static readonly AssetReference VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission5ExchangeD_02 = new AssetReference("VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission5ExchangeD_02.prefab:c9dd4833332327f47926c508a7d60142");

	private static readonly AssetReference VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission5ExchangeE_01 = new AssetReference("VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission5ExchangeE_01.prefab:28845e0bf6ca5a0468fceb41ccb915ee");

	private static readonly AssetReference VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission5Intro_02 = new AssetReference("VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission5Intro_02.prefab:5cf6ca9c2b905fa4386f1b29883353fa");

	private static readonly AssetReference VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission5Victory_02 = new AssetReference("VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission5Victory_02.prefab:6422b3a8d075a744eb35b7a2debe92fa");

	private static readonly AssetReference VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission5Victory_03 = new AssetReference("VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission5Victory_03.prefab:3756cb40520792c4186b093d40c8fdbd");

	private static readonly AssetReference VO_Story_Hero_Magtheridon_Male_Demon_Story_Illidan_Mission2EmoteResponse_01 = new AssetReference("VO_Story_Hero_Magtheridon_Male_Demon_Story_Illidan_Mission2EmoteResponse_01.prefab:0423e1997a164f9ebff1a15e5a9fb8f1");

	private static readonly AssetReference VO_Story_Hero_Magtheridon_Male_Demon_Story_Illidan_Mission2HeroPower_01 = new AssetReference("VO_Story_Hero_Magtheridon_Male_Demon_Story_Illidan_Mission2HeroPower_01.prefab:7f216246965f44bd90600bd96d0ea809");

	private static readonly AssetReference VO_Story_Hero_Magtheridon_Male_Demon_Story_Illidan_Mission2HeroPower_02 = new AssetReference("VO_Story_Hero_Magtheridon_Male_Demon_Story_Illidan_Mission2HeroPower_02.prefab:ac5ae43defe74fa19526aa3ef91e7d8f");

	private static readonly AssetReference VO_Story_Hero_Magtheridon_Male_Demon_Story_Illidan_Mission2HeroPower_03 = new AssetReference("VO_Story_Hero_Magtheridon_Male_Demon_Story_Illidan_Mission2HeroPower_03.prefab:4297e7cbf7974fb7aff75c591fc70653");

	private static readonly AssetReference VO_Story_Hero_Magtheridon_Male_Demon_Story_Illidan_Mission2Idle_01 = new AssetReference("VO_Story_Hero_Magtheridon_Male_Demon_Story_Illidan_Mission2Idle_01.prefab:910480ec95cc401e86838ab21d3704c1");

	private static readonly AssetReference VO_Story_Hero_Magtheridon_Male_Demon_Story_Illidan_Mission2Idle_02 = new AssetReference("VO_Story_Hero_Magtheridon_Male_Demon_Story_Illidan_Mission2Idle_02.prefab:d8dd3ac85bec406fb6b7b6b8bab071b4");

	private static readonly AssetReference VO_Story_Hero_Magtheridon_Male_Demon_Story_Illidan_Mission2Idle_03 = new AssetReference("VO_Story_Hero_Magtheridon_Male_Demon_Story_Illidan_Mission2Idle_03.prefab:ae19e76b64094e5d8c3d7d2badd6fe21");

	private static readonly AssetReference VO_Story_Hero_Magtheridon_Male_Demon_Story_Illidan_Mission2Loss_01 = new AssetReference("VO_Story_Hero_Magtheridon_Male_Demon_Story_Illidan_Mission2Loss_01.prefab:90b1fed613f34348a039b2a062feb858");

	private static readonly AssetReference VO_Story_Hero_Magtheridon_Male_Demon_Story_Illidan_Mission5ExchangeA_01 = new AssetReference("VO_Story_Hero_Magtheridon_Male_Demon_Story_Illidan_Mission5ExchangeA_01.prefab:7d24dd7f63bd42d48264a3a1f144ec10");

	private static readonly AssetReference VO_Story_Hero_Magtheridon_Male_Demon_Story_Illidan_Mission5Intro_01 = new AssetReference("VO_Story_Hero_Magtheridon_Male_Demon_Story_Illidan_Mission5Intro_01.prefab:5b2a28d9d623418babe030ef9e927e54");

	private static readonly AssetReference VO_Story_Hero_Magtheridon_Male_Demon_Story_Illidan_Mission5Victory_01 = new AssetReference("VO_Story_Hero_Magtheridon_Male_Demon_Story_Illidan_Mission5Victory_01.prefab:381955d43c874e3180f7dd6a4d567bdc");

	private static readonly AssetReference VO_Story_Minion_Kaelthas_Male_BloodElf_Story_Illidan_Mission5ExchangeD_01 = new AssetReference("VO_Story_Minion_Kaelthas_Male_BloodElf_Story_Illidan_Mission5ExchangeD_01.prefab:47698713a9df46b499c9857a883aba6e");

	private static readonly AssetReference VO_BTA_BOSS_12h_Male_Demon_Mission_Fight_12_BossDeathA_01 = new AssetReference("VO_BTA_BOSS_12h_Male_Demon_Mission_Fight_12_BossDeathA_01.prefab:14e830bbea2e94842a2136916c74f14d");

	public static readonly AssetReference AkamaBrassRing = new AssetReference("Akama_BrassRing_Quote.prefab:5ebe6238357ccd540be5937d2c57e10c");

	public static readonly AssetReference KaelthasBrassRing = new AssetReference("Kaelthas_BrassRing_Quote.prefab:e2c98e804ab04dd49bfbd665c1647eca");

	private List<string> m_BossUsesHeroPowerLines = new List<string> { VO_Story_Hero_Magtheridon_Male_Demon_Story_Illidan_Mission2HeroPower_01, VO_Story_Hero_Magtheridon_Male_Demon_Story_Illidan_Mission2HeroPower_02, VO_Story_Hero_Magtheridon_Male_Demon_Story_Illidan_Mission2HeroPower_03 };

	private new List<string> m_BossIdleLines = new List<string> { VO_Story_Hero_Magtheridon_Male_Demon_Story_Illidan_Mission2Idle_01, VO_Story_Hero_Magtheridon_Male_Demon_Story_Illidan_Mission2Idle_02, VO_Story_Hero_Magtheridon_Male_Demon_Story_Illidan_Mission2Idle_03 };

	private HashSet<string> m_playedLines = new HashSet<string>();

	private static Map<GameEntityOption, bool> InitBooleanOptions()
	{
		return new Map<GameEntityOption, bool> { 
		{
			GameEntityOption.DO_OPENING_TAUNTS,
			false
		} };
	}

	public BoH_Illidan_05()
	{
		m_gameOptions.AddBooleanOptions(s_booleanOptions);
	}

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> m_SoundFiles = new List<string>
		{
			VO_Story_Hero_Akama_Male_Draenei_Story_Illidan_Mission5ExchangeB_01, VO_Story_Hero_Akama_Male_Draenei_Story_Illidan_Mission5ExchangeB_03, VO_Story_Hero_Akama_Male_Draenei_Story_Illidan_Mission5ExchangeE_02, VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission5ExchangeA_02, VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission5ExchangeB_02, VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission5ExchangeC_01, VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission5ExchangeD_02, VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission5ExchangeE_01, VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission5Intro_02, VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission5Victory_02,
			VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission5Victory_03, VO_Story_Hero_Magtheridon_Male_Demon_Story_Illidan_Mission2EmoteResponse_01, VO_Story_Hero_Magtheridon_Male_Demon_Story_Illidan_Mission2HeroPower_01, VO_Story_Hero_Magtheridon_Male_Demon_Story_Illidan_Mission2HeroPower_02, VO_Story_Hero_Magtheridon_Male_Demon_Story_Illidan_Mission2HeroPower_03, VO_Story_Hero_Magtheridon_Male_Demon_Story_Illidan_Mission2Idle_01, VO_Story_Hero_Magtheridon_Male_Demon_Story_Illidan_Mission2Idle_02, VO_Story_Hero_Magtheridon_Male_Demon_Story_Illidan_Mission2Idle_03, VO_Story_Hero_Magtheridon_Male_Demon_Story_Illidan_Mission2Loss_01, VO_Story_Hero_Magtheridon_Male_Demon_Story_Illidan_Mission5ExchangeA_01,
			VO_Story_Hero_Magtheridon_Male_Demon_Story_Illidan_Mission5Intro_01, VO_Story_Hero_Magtheridon_Male_Demon_Story_Illidan_Mission5Victory_01, VO_Story_Minion_Kaelthas_Male_BloodElf_Story_Illidan_Mission5ExchangeD_01, VO_BTA_BOSS_12h_Male_Demon_Mission_Fight_12_BossDeathA_01
		};
		SetBossVOLines(m_SoundFiles);
		foreach (string soundFile in m_SoundFiles)
		{
			PreloadSound(soundFile);
		}
	}

	public override bool ShouldPlayHeroBlowUpSpells(TAG_PLAYSTATE playState)
	{
		return playState != TAG_PLAYSTATE.WON;
	}

	public override List<string> GetBossHeroPowerRandomLines()
	{
		return m_BossUsesHeroPowerLines;
	}

	public override List<string> GetBossIdleLines()
	{
		return m_BossIdleLines;
	}

	public override void OnCreateGame()
	{
		base.OnCreateGame();
		m_OverrideMusicTrack = MusicPlaylistType.InGame_DRGEVILBoss;
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
		case 515:
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Magtheridon_Male_Demon_Story_Illidan_Mission2EmoteResponse_01);
			break;
		case 514:
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Magtheridon_Male_Demon_Story_Illidan_Mission5Intro_01);
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission5Intro_02);
			break;
		case 507:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Magtheridon_Male_Demon_Story_Illidan_Mission2Loss_01);
			GameState.Get().SetBusy(busy: false);
			break;
		case 504:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Magtheridon_Male_Demon_Story_Illidan_Mission5Victory_01);
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission5Victory_02);
			GameState.Get().SetBusy(busy: false);
			break;
		case 101:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission5Victory_03);
			GameState.Get().SetBusy(busy: false);
			break;
		case 102:
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission5ExchangeC_01);
			break;
		case 103:
			yield return MissionPlayVO(KaelthasBrassRing, VO_Story_Minion_Kaelthas_Male_BloodElf_Story_Illidan_Mission5ExchangeD_01);
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission5ExchangeD_02);
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
		case 1:
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Magtheridon_Male_Demon_Story_Illidan_Mission5ExchangeA_01);
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission5ExchangeA_02);
			break;
		case 3:
			yield return MissionPlayVO(AkamaBrassRing, VO_Story_Hero_Akama_Male_Draenei_Story_Illidan_Mission5ExchangeB_01);
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission5ExchangeB_02);
			yield return MissionPlayVO(AkamaBrassRing, VO_Story_Hero_Akama_Male_Draenei_Story_Illidan_Mission5ExchangeB_03);
			break;
		case 5:
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission5ExchangeE_01);
			yield return MissionPlayVO(AkamaBrassRing, VO_Story_Hero_Akama_Male_Draenei_Story_Illidan_Mission5ExchangeE_02);
			break;
		}
	}
}
