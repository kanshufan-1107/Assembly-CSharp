using System.Collections;
using System.Collections.Generic;
using Blizzard.T5.Core;

public class BoH_Illidan_03 : BoH_Illidan_Dungeon
{
	private static Map<GameEntityOption, bool> s_booleanOptions = InitBooleanOptions();

	private static readonly AssetReference VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission3ExchangeA_01 = new AssetReference("VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission3ExchangeA_01.prefab:21f6c3dfb6638324598a39fd250abddb");

	private static readonly AssetReference VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission3ExchangeA_03 = new AssetReference("VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission3ExchangeA_03.prefab:75609445a7e55044ab0dbb51a74933d3");

	private static readonly AssetReference VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission3ExchangeB_02 = new AssetReference("VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission3ExchangeB_02.prefab:37d104472d18ba44aa1bfee49b22d131");

	private static readonly AssetReference VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission3ExchangeC_02 = new AssetReference("VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission3ExchangeC_02.prefab:37c7a402eb1590b4eb520a4d29f4822e");

	private static readonly AssetReference VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission3ExchangeD_02 = new AssetReference("VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission3ExchangeD_02.prefab:5fc3b33b968918e41937d6fdd685e444");

	private static readonly AssetReference VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission3Intro_01 = new AssetReference("VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission3Intro_01.prefab:8ad0e02fd98dc4e4e96e36495d9306f6");

	private static readonly AssetReference VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission3Victory_02 = new AssetReference("VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission3Victory_02.prefab:f743c41fede7cf044b7bee5481e3d3ea");

	private static readonly AssetReference VO_Story_Hero_Maiev_Female_NightElf_Story_Illidan_Mission2EmoteResponse_01 = new AssetReference("VO_Story_Hero_Maiev_Female_NightElf_Story_Illidan_Mission2EmoteResponse_01.prefab:32255347523d57d4abbead3556518b1f");

	private static readonly AssetReference VO_Story_Hero_Maiev_Female_NightElf_Story_Illidan_Mission2HeroPower_01 = new AssetReference("VO_Story_Hero_Maiev_Female_NightElf_Story_Illidan_Mission2HeroPower_01.prefab:0ab6b68e72dd3f846a975e64440c39bb");

	private static readonly AssetReference VO_Story_Hero_Maiev_Female_NightElf_Story_Illidan_Mission2HeroPower_02 = new AssetReference("VO_Story_Hero_Maiev_Female_NightElf_Story_Illidan_Mission2HeroPower_02.prefab:b805237209df86941a162fea6fb7f42f");

	private static readonly AssetReference VO_Story_Hero_Maiev_Female_NightElf_Story_Illidan_Mission2HeroPower_03 = new AssetReference("VO_Story_Hero_Maiev_Female_NightElf_Story_Illidan_Mission2HeroPower_03.prefab:bc3d87a82ad4c234c9b2ed33acaeedd6");

	private static readonly AssetReference VO_Story_Hero_Maiev_Female_NightElf_Story_Illidan_Mission2Idle_01 = new AssetReference("VO_Story_Hero_Maiev_Female_NightElf_Story_Illidan_Mission2Idle_01.prefab:e9d782c1740bc5345b1d9aa2d0523aae");

	private static readonly AssetReference VO_Story_Hero_Maiev_Female_NightElf_Story_Illidan_Mission2Idle_02 = new AssetReference("VO_Story_Hero_Maiev_Female_NightElf_Story_Illidan_Mission2Idle_02.prefab:ac8209c5f0c05f54d9c2cc6f65952376");

	private static readonly AssetReference VO_Story_Hero_Maiev_Female_NightElf_Story_Illidan_Mission2Idle_03 = new AssetReference("VO_Story_Hero_Maiev_Female_NightElf_Story_Illidan_Mission2Idle_03.prefab:11e6e819de57dfd4f9f287878c2ac8eb");

	private static readonly AssetReference VO_Story_Hero_Maiev_Female_NightElf_Story_Illidan_Mission2Loss_01 = new AssetReference("VO_Story_Hero_Maiev_Female_NightElf_Story_Illidan_Mission2Loss_01.prefab:f0216d1bcbe29df42b0b3140f63f42df");

	private static readonly AssetReference VO_Story_Hero_Maiev_Female_NightElf_Story_Illidan_Mission3ExchangeA_02 = new AssetReference("VO_Story_Hero_Maiev_Female_NightElf_Story_Illidan_Mission3ExchangeA_02.prefab:619ac423a38363b49904d91b5e7812ce");

	private static readonly AssetReference VO_Story_Hero_Maiev_Female_NightElf_Story_Illidan_Mission3ExchangeB_01 = new AssetReference("VO_Story_Hero_Maiev_Female_NightElf_Story_Illidan_Mission3ExchangeB_01.prefab:a61713e2e8b1cbf488359ecbf696b718");

	private static readonly AssetReference VO_Story_Hero_Maiev_Female_NightElf_Story_Illidan_Mission3ExchangeC_01 = new AssetReference("VO_Story_Hero_Maiev_Female_NightElf_Story_Illidan_Mission3ExchangeC_01.prefab:2656236758534bf46bb19d9ecb8254d6");

	private static readonly AssetReference VO_Story_Hero_Maiev_Female_NightElf_Story_Illidan_Mission3ExchangeD_01 = new AssetReference("VO_Story_Hero_Maiev_Female_NightElf_Story_Illidan_Mission3ExchangeD_01.prefab:dc333db26633fad44ad70646d262e700");

	private static readonly AssetReference VO_Story_Hero_Maiev_Female_NightElf_Story_Illidan_Mission3ExchangeE_01 = new AssetReference("VO_Story_Hero_Maiev_Female_NightElf_Story_Illidan_Mission3ExchangeE_01.prefab:356fab569c0118940a2b272f8a4ae24d");

	private static readonly AssetReference VO_Story_Hero_Maiev_Female_NightElf_Story_Illidan_Mission3Intro_02 = new AssetReference("VO_Story_Hero_Maiev_Female_NightElf_Story_Illidan_Mission3Intro_02.prefab:788eb0a6f3f515b4c83009372e4376bb");

	private static readonly AssetReference VO_Story_Hero_Maiev_Female_NightElf_Story_Illidan_Mission4Victory_01 = new AssetReference("VO_Story_Hero_Maiev_Female_NightElf_Story_Illidan_Mission4Victory_01.prefab:8bd965b4e0c8dab4ba08e236ad9e2a5d");

	private List<string> m_BossUsesHeroPowerLines = new List<string> { VO_Story_Hero_Maiev_Female_NightElf_Story_Illidan_Mission2HeroPower_01, VO_Story_Hero_Maiev_Female_NightElf_Story_Illidan_Mission2HeroPower_02, VO_Story_Hero_Maiev_Female_NightElf_Story_Illidan_Mission2HeroPower_03 };

	private new List<string> m_BossIdleLines = new List<string> { VO_Story_Hero_Maiev_Female_NightElf_Story_Illidan_Mission2Idle_01, VO_Story_Hero_Maiev_Female_NightElf_Story_Illidan_Mission2Idle_02, VO_Story_Hero_Maiev_Female_NightElf_Story_Illidan_Mission2Idle_03 };

	private HashSet<string> m_playedLines = new HashSet<string>();

	private static Map<GameEntityOption, bool> InitBooleanOptions()
	{
		return new Map<GameEntityOption, bool> { 
		{
			GameEntityOption.DO_OPENING_TAUNTS,
			false
		} };
	}

	public BoH_Illidan_03()
	{
		m_gameOptions.AddBooleanOptions(s_booleanOptions);
	}

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> m_SoundFiles = new List<string>
		{
			VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission3ExchangeA_01, VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission3ExchangeA_03, VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission3ExchangeB_02, VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission3ExchangeC_02, VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission3ExchangeD_02, VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission3Intro_01, VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission3Victory_02, VO_Story_Hero_Maiev_Female_NightElf_Story_Illidan_Mission2EmoteResponse_01, VO_Story_Hero_Maiev_Female_NightElf_Story_Illidan_Mission2HeroPower_01, VO_Story_Hero_Maiev_Female_NightElf_Story_Illidan_Mission2HeroPower_02,
			VO_Story_Hero_Maiev_Female_NightElf_Story_Illidan_Mission2HeroPower_03, VO_Story_Hero_Maiev_Female_NightElf_Story_Illidan_Mission2Idle_01, VO_Story_Hero_Maiev_Female_NightElf_Story_Illidan_Mission2Idle_02, VO_Story_Hero_Maiev_Female_NightElf_Story_Illidan_Mission2Idle_03, VO_Story_Hero_Maiev_Female_NightElf_Story_Illidan_Mission2Loss_01, VO_Story_Hero_Maiev_Female_NightElf_Story_Illidan_Mission3ExchangeA_02, VO_Story_Hero_Maiev_Female_NightElf_Story_Illidan_Mission3ExchangeB_01, VO_Story_Hero_Maiev_Female_NightElf_Story_Illidan_Mission3ExchangeC_01, VO_Story_Hero_Maiev_Female_NightElf_Story_Illidan_Mission3ExchangeD_01, VO_Story_Hero_Maiev_Female_NightElf_Story_Illidan_Mission3ExchangeE_01,
			VO_Story_Hero_Maiev_Female_NightElf_Story_Illidan_Mission3Intro_02, VO_Story_Hero_Maiev_Female_NightElf_Story_Illidan_Mission4Victory_01
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
		m_OverrideMusicTrack = MusicPlaylistType.InGame_DHPrologueBoss;
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
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Maiev_Female_NightElf_Story_Illidan_Mission2EmoteResponse_01);
			break;
		case 514:
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission3Intro_01);
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Maiev_Female_NightElf_Story_Illidan_Mission3Intro_02);
			break;
		case 507:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Maiev_Female_NightElf_Story_Illidan_Mission2Loss_01);
			GameState.Get().SetBusy(busy: false);
			break;
		case 504:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Maiev_Female_NightElf_Story_Illidan_Mission4Victory_01);
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission3Victory_02);
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
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission3ExchangeA_01);
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Maiev_Female_NightElf_Story_Illidan_Mission3ExchangeA_02);
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission3ExchangeA_03);
			break;
		case 5:
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Maiev_Female_NightElf_Story_Illidan_Mission3ExchangeB_01);
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission3ExchangeB_02);
			break;
		case 9:
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Maiev_Female_NightElf_Story_Illidan_Mission3ExchangeC_01);
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission3ExchangeC_02);
			break;
		case 13:
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Maiev_Female_NightElf_Story_Illidan_Mission3ExchangeD_01);
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission3ExchangeD_02);
			break;
		case 15:
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Maiev_Female_NightElf_Story_Illidan_Mission3ExchangeE_01);
			break;
		}
	}
}
