using System.Collections;
using System.Collections.Generic;
using Blizzard.T5.Core;

public class BoH_Illidan_06 : BoH_Illidan_Dungeon
{
	private static Map<GameEntityOption, bool> s_booleanOptions = InitBooleanOptions();

	private static readonly AssetReference VO_TB_KT_Anubarak_Male_CryptLord_PathofKT_Boss03_Death_01 = new AssetReference("VO_TB_KT_Anubarak_Male_CryptLord_PathofKT_Boss03_Death_01.prefab:5a62d79d4d05ead47b98672f31a6bfe1");

	private static readonly AssetReference VO_Story_Hero_Anubarak_Male_Nerubian_Story_Illidan_Mission2EmoteResponse_01 = new AssetReference("VO_Story_Hero_Anubarak_Male_Nerubian_Story_Illidan_Mission2EmoteResponse_01.prefab:44ce5081288c7b34482c85ac381ca6dd");

	private static readonly AssetReference VO_Story_Hero_Anubarak_Male_Nerubian_Story_Illidan_Mission2HeroPower_01 = new AssetReference("VO_Story_Hero_Anubarak_Male_Nerubian_Story_Illidan_Mission2HeroPower_01.prefab:346fde71fdc3df845a58df7ca35c09c6");

	private static readonly AssetReference VO_Story_Hero_Anubarak_Male_Nerubian_Story_Illidan_Mission2HeroPower_02 = new AssetReference("VO_Story_Hero_Anubarak_Male_Nerubian_Story_Illidan_Mission2HeroPower_02.prefab:7d9b954c44259864e9bb524f1cc94a46");

	private static readonly AssetReference VO_Story_Hero_Anubarak_Male_Nerubian_Story_Illidan_Mission2HeroPower_03 = new AssetReference("VO_Story_Hero_Anubarak_Male_Nerubian_Story_Illidan_Mission2HeroPower_03.prefab:5dd7a63238627744da4b08b46051e28b");

	private static readonly AssetReference VO_Story_Hero_Anubarak_Male_Nerubian_Story_Illidan_Mission2Idle_01 = new AssetReference("VO_Story_Hero_Anubarak_Male_Nerubian_Story_Illidan_Mission2Idle_01.prefab:ae46eaa37c572fb488e29e496b8c5bc9");

	private static readonly AssetReference VO_Story_Hero_Anubarak_Male_Nerubian_Story_Illidan_Mission2Idle_02 = new AssetReference("VO_Story_Hero_Anubarak_Male_Nerubian_Story_Illidan_Mission2Idle_02.prefab:3cd95a7e90d4f3943845d22cf6eb3620");

	private static readonly AssetReference VO_Story_Hero_Anubarak_Male_Nerubian_Story_Illidan_Mission2Idle_03 = new AssetReference("VO_Story_Hero_Anubarak_Male_Nerubian_Story_Illidan_Mission2Idle_03.prefab:59829bf0da3cc354aa339c8b4082f1b3");

	private static readonly AssetReference VO_Story_Hero_Anubarak_Male_Nerubian_Story_Illidan_Mission2Loss_01 = new AssetReference("VO_Story_Hero_Anubarak_Male_Nerubian_Story_Illidan_Mission2Loss_01.prefab:2a53a0ee9d18e5641a13b99592c6793c");

	private static readonly AssetReference VO_Story_Hero_Anubarak_Male_Nerubian_Story_Illidan_Mission6ExchangeA_01 = new AssetReference("VO_Story_Hero_Anubarak_Male_Nerubian_Story_Illidan_Mission6ExchangeA_01.prefab:aa3049b50571c0844864fc4edb9641d8");

	private static readonly AssetReference VO_Story_Hero_Anubarak_Male_Nerubian_Story_Illidan_Mission6ExchangeB_01 = new AssetReference("VO_Story_Hero_Anubarak_Male_Nerubian_Story_Illidan_Mission6ExchangeB_01.prefab:360ffa5f7cdd3f54cb747cfd53e4c22c");

	private static readonly AssetReference VO_Story_Hero_Anubarak_Male_Nerubian_Story_Illidan_Mission6ExchangeD_02 = new AssetReference("VO_Story_Hero_Anubarak_Male_Nerubian_Story_Illidan_Mission6ExchangeD_02.prefab:26f7f61efed06774eb70e1a4298557de");

	private static readonly AssetReference VO_Story_Hero_Anubarak_Male_Nerubian_Story_Illidan_Mission6ExchangeD_04 = new AssetReference("VO_Story_Hero_Anubarak_Male_Nerubian_Story_Illidan_Mission6ExchangeD_04.prefab:a6c1a458f534d0c44975fe5677267025");

	private static readonly AssetReference VO_Story_Hero_Arthas_Male_Human_Story_Illidan_Mission6Intro_02 = new AssetReference("VO_Story_Hero_Arthas_Male_Human_Story_Illidan_Mission6Intro_02.prefab:d1dab658c7a1df34291388b4737fe67b");

	private static readonly AssetReference VO_Story_Hero_Arthas_Male_Human_Story_Illidan_Mission6Intro_03 = new AssetReference("VO_Story_Hero_Arthas_Male_Human_Story_Illidan_Mission6Intro_03.prefab:2b62e17da3c5888418dd863be33ff355");

	private static readonly AssetReference VO_Story_Hero_Arthas_Male_Human_Story_Illidan_Mission6Victory_01 = new AssetReference("VO_Story_Hero_Arthas_Male_Human_Story_Illidan_Mission6Victory_01.prefab:f52b971e4a724f94cbb9fd89bb74659b");

	private static readonly AssetReference VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission6ExchangeA_02 = new AssetReference("VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission6ExchangeA_02.prefab:ed5abd9ba6b517948a166fb418430067");

	private static readonly AssetReference VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission6ExchangeB_02 = new AssetReference("VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission6ExchangeB_02.prefab:bbb669a89b1702e4aa960097b11f3ca7");

	private static readonly AssetReference VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission6ExchangeC_01 = new AssetReference("VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission6ExchangeC_01.prefab:033fcad376853fc4790f8d240539c335");

	private static readonly AssetReference VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission6ExchangeD_01 = new AssetReference("VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission6ExchangeD_01.prefab:866574f6f93985e4090c0645bd57acb4");

	private static readonly AssetReference VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission6ExchangeD_03 = new AssetReference("VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission6ExchangeD_03.prefab:c8a163e7b0cd5444392dd679354644a8");

	private static readonly AssetReference VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission6Intro_01 = new AssetReference("VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission6Intro_01.prefab:a48aaf09334cc4e4f8bf150cd1ca3b97");

	private static readonly AssetReference VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission6Victory_02 = new AssetReference("VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission6Victory_02.prefab:b3839de2660b56d428105d7a6f3746fb");

	private static readonly AssetReference VO_Story_Minion_Kaelthas_Male_BloodElf_Story_Illidan_Mission6ExchangeA_03 = new AssetReference("VO_Story_Minion_Kaelthas_Male_BloodElf_Story_Illidan_Mission6ExchangeA_03.prefab:73d404874478d074193ec04c4a7ac0d7");

	public static readonly AssetReference ArthasBrassRing = new AssetReference("Arthas_Sword_BrassRing_Quote.prefab:170b624397fa95d40aaee3faede08055");

	public static readonly AssetReference KaelthasBrassRing = new AssetReference("Kaelthas_BrassRing_Quote.prefab:e2c98e804ab04dd49bfbd665c1647eca");

	private List<string> m_BossUsesHeroPowerLines = new List<string> { VO_Story_Hero_Anubarak_Male_Nerubian_Story_Illidan_Mission2HeroPower_01, VO_Story_Hero_Anubarak_Male_Nerubian_Story_Illidan_Mission2HeroPower_02, VO_Story_Hero_Anubarak_Male_Nerubian_Story_Illidan_Mission2HeroPower_03 };

	private new List<string> m_BossIdleLines = new List<string> { VO_Story_Hero_Anubarak_Male_Nerubian_Story_Illidan_Mission2Idle_01, VO_Story_Hero_Anubarak_Male_Nerubian_Story_Illidan_Mission2Idle_02, VO_Story_Hero_Anubarak_Male_Nerubian_Story_Illidan_Mission2Idle_03 };

	private HashSet<string> m_playedLines = new HashSet<string>();

	private static Map<GameEntityOption, bool> InitBooleanOptions()
	{
		return new Map<GameEntityOption, bool> { 
		{
			GameEntityOption.DO_OPENING_TAUNTS,
			false
		} };
	}

	public BoH_Illidan_06()
	{
		m_gameOptions.AddBooleanOptions(s_booleanOptions);
	}

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> m_SoundFiles = new List<string>
		{
			VO_TB_KT_Anubarak_Male_CryptLord_PathofKT_Boss03_Death_01, VO_Story_Hero_Anubarak_Male_Nerubian_Story_Illidan_Mission2EmoteResponse_01, VO_Story_Hero_Anubarak_Male_Nerubian_Story_Illidan_Mission2HeroPower_01, VO_Story_Hero_Anubarak_Male_Nerubian_Story_Illidan_Mission2HeroPower_02, VO_Story_Hero_Anubarak_Male_Nerubian_Story_Illidan_Mission2HeroPower_03, VO_Story_Hero_Anubarak_Male_Nerubian_Story_Illidan_Mission2Idle_01, VO_Story_Hero_Anubarak_Male_Nerubian_Story_Illidan_Mission2Idle_02, VO_Story_Hero_Anubarak_Male_Nerubian_Story_Illidan_Mission2Idle_03, VO_Story_Hero_Anubarak_Male_Nerubian_Story_Illidan_Mission2Loss_01, VO_Story_Hero_Anubarak_Male_Nerubian_Story_Illidan_Mission6ExchangeA_01,
			VO_Story_Hero_Anubarak_Male_Nerubian_Story_Illidan_Mission6ExchangeB_01, VO_Story_Hero_Anubarak_Male_Nerubian_Story_Illidan_Mission6ExchangeD_02, VO_Story_Hero_Anubarak_Male_Nerubian_Story_Illidan_Mission6ExchangeD_04, VO_Story_Hero_Arthas_Male_Human_Story_Illidan_Mission6Intro_02, VO_Story_Hero_Arthas_Male_Human_Story_Illidan_Mission6Intro_03, VO_Story_Hero_Arthas_Male_Human_Story_Illidan_Mission6Victory_01, VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission6ExchangeA_02, VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission6ExchangeB_02, VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission6ExchangeC_01, VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission6ExchangeD_01,
			VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission6ExchangeD_03, VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission6Intro_01, VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission6Victory_02, VO_Story_Minion_Kaelthas_Male_BloodElf_Story_Illidan_Mission6ExchangeA_03
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
		m_OverrideMusicTrack = MusicPlaylistType.InGame_ICC;
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
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Anubarak_Male_Nerubian_Story_Illidan_Mission2EmoteResponse_01);
			break;
		case 514:
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission6Intro_01);
			yield return MissionPlayVO(ArthasBrassRing, VO_Story_Hero_Arthas_Male_Human_Story_Illidan_Mission6Intro_02);
			yield return MissionPlayVO(ArthasBrassRing, VO_Story_Hero_Arthas_Male_Human_Story_Illidan_Mission6Intro_03);
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Anubarak_Male_Nerubian_Story_Illidan_Mission6ExchangeA_01);
			break;
		case 507:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Anubarak_Male_Nerubian_Story_Illidan_Mission2Loss_01);
			GameState.Get().SetBusy(busy: false);
			break;
		case 504:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(ArthasBrassRing, VO_Story_Hero_Arthas_Male_Human_Story_Illidan_Mission6Victory_01);
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission6Victory_02);
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
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission6ExchangeA_02);
			yield return MissionPlayVO(KaelthasBrassRing, VO_Story_Minion_Kaelthas_Male_BloodElf_Story_Illidan_Mission6ExchangeA_03);
			break;
		case 3:
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Anubarak_Male_Nerubian_Story_Illidan_Mission6ExchangeB_01);
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission6ExchangeB_02);
			break;
		case 5:
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission6ExchangeC_01);
			break;
		case 9:
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission6ExchangeD_01);
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Anubarak_Male_Nerubian_Story_Illidan_Mission6ExchangeD_02);
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission6ExchangeD_03);
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Anubarak_Male_Nerubian_Story_Illidan_Mission6ExchangeD_04);
			break;
		}
	}
}
