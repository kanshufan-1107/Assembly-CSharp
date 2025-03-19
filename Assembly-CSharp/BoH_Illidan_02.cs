using System.Collections;
using System.Collections.Generic;
using Blizzard.T5.Core;
using UnityEngine;

public class BoH_Illidan_02 : BoH_Illidan_Dungeon
{
	private static Map<GameEntityOption, bool> s_booleanOptions = InitBooleanOptions();

	private static readonly AssetReference VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission2ExchangeA_02 = new AssetReference("VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission2ExchangeA_02.prefab:82466bbba2b65ce4a8e0732fe9779738");

	private static readonly AssetReference VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission2ExchangeB_02 = new AssetReference("VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission2ExchangeB_02.prefab:2ed99b5fa7b7094408bd1c957099feff");

	private static readonly AssetReference VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission2ExchangeC_01 = new AssetReference("VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission2ExchangeC_01.prefab:8e0263b27ed23794bb3677aa7708d0b0");

	private static readonly AssetReference VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission2ExchangeD_01 = new AssetReference("VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission2ExchangeD_01.prefab:7e97a522941ad1946bf77eedc2a4f509");

	private static readonly AssetReference VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission2ExchangeE_01 = new AssetReference("VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission2ExchangeE_01.prefab:31d696683decd0d42b94edd3d776f289");

	private static readonly AssetReference VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission2ExchangeE_02 = new AssetReference("VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission2ExchangeE_02.prefab:befe0f4b837e5e64fa1be4c1b87a1a8f");

	private static readonly AssetReference VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission2ExchangeE_04 = new AssetReference("VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission2ExchangeE_04.prefab:e5277149d8a74574383ad686868e5163");

	private static readonly AssetReference VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission2Intro_01 = new AssetReference("VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission2Intro_01.prefab:be7df774bd298b241bd56343516066fa");

	private static readonly AssetReference VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission2Victory_02 = new AssetReference("VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission2Victory_02.prefab:55608d077b62b554496fc56cf532da83");

	private static readonly AssetReference VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission2Victory_04 = new AssetReference("VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission2Victory_04.prefab:f2af70a36c98b694a91bb1bd6fb9ed7e");

	private static readonly AssetReference VO_Story_Hero_Malfurion_Male_NightElf_Story_Illidan_Mission2Victory_01 = new AssetReference("VO_Story_Hero_Malfurion_Male_NightElf_Story_Illidan_Mission2Victory_01.prefab:f76929bab4bd20944b2ee87906a015a7");

	private static readonly AssetReference VO_Story_Hero_Malfurion_Male_NightElf_Story_Illidan_Mission2Victory_03 = new AssetReference("VO_Story_Hero_Malfurion_Male_NightElf_Story_Illidan_Mission2Victory_03.prefab:6d774f509f7cece42a32fa9c3b0d8337");

	private static readonly AssetReference VO_Story_Hero_Tichondrius_Male_Demon_Story_Illidan_Mission2Death_01 = new AssetReference("VO_Story_Hero_Tichondrius_Male_Demon_Story_Illidan_Mission2Death_01.prefab:68d4badb7ff945b459fca8f17fa8fabd");

	private static readonly AssetReference VO_Story_Hero_Tichondrius_Male_Demon_Story_Illidan_Mission2EmoteResponse_01 = new AssetReference("VO_Story_Hero_Tichondrius_Male_Demon_Story_Illidan_Mission2EmoteResponse_01.prefab:7dca711ae551fc247b362944b3d44f42");

	private static readonly AssetReference VO_Story_Hero_Tichondrius_Male_Demon_Story_Illidan_Mission2ExchangeB_01 = new AssetReference("VO_Story_Hero_Tichondrius_Male_Demon_Story_Illidan_Mission2ExchangeB_01.prefab:b7a6a79757eed4e41b50bb4de908663a");

	private static readonly AssetReference VO_Story_Hero_Tichondrius_Male_Demon_Story_Illidan_Mission2ExchangeC_01 = new AssetReference("VO_Story_Hero_Tichondrius_Male_Demon_Story_Illidan_Mission2ExchangeC_01.prefab:785ce0b9d3b8e644987606f6be9ed120");

	private static readonly AssetReference VO_Story_Hero_Tichondrius_Male_Demon_Story_Illidan_Mission2ExchangeD_02 = new AssetReference("VO_Story_Hero_Tichondrius_Male_Demon_Story_Illidan_Mission2ExchangeD_02.prefab:f9ed709cf586fa041b36b64e18c55757");

	private static readonly AssetReference VO_Story_Hero_Tichondrius_Male_Demon_Story_Illidan_Mission2ExchangeE_02 = new AssetReference("VO_Story_Hero_Tichondrius_Male_Demon_Story_Illidan_Mission2ExchangeE_02.prefab:25a8e0f5b72dbe54fac1a90fa09c2e6e");

	private static readonly AssetReference VO_Story_Hero_Tichondrius_Male_Demon_Story_Illidan_Mission2HeroPower_01 = new AssetReference("VO_Story_Hero_Tichondrius_Male_Demon_Story_Illidan_Mission2HeroPower_01.prefab:a30c669af0ce4b54f8a82f93863c75f1");

	private static readonly AssetReference VO_Story_Hero_Tichondrius_Male_Demon_Story_Illidan_Mission2HeroPower_02 = new AssetReference("VO_Story_Hero_Tichondrius_Male_Demon_Story_Illidan_Mission2HeroPower_02.prefab:fba78ceeb7e40c24fa1a1bf139a6dfc2");

	private static readonly AssetReference VO_Story_Hero_Tichondrius_Male_Demon_Story_Illidan_Mission2HeroPower_03 = new AssetReference("VO_Story_Hero_Tichondrius_Male_Demon_Story_Illidan_Mission2HeroPower_03.prefab:b4e687162b77f514a8606d6ccf075a34");

	private static readonly AssetReference VO_Story_Hero_Tichondrius_Male_Demon_Story_Illidan_Mission2Idle_01 = new AssetReference("VO_Story_Hero_Tichondrius_Male_Demon_Story_Illidan_Mission2Idle_01.prefab:a03a110b667e00c4fbeda4ce0b46d41d");

	private static readonly AssetReference VO_Story_Hero_Tichondrius_Male_Demon_Story_Illidan_Mission2Idle_02 = new AssetReference("VO_Story_Hero_Tichondrius_Male_Demon_Story_Illidan_Mission2Idle_02.prefab:b796299650257db409645403a6520777");

	private static readonly AssetReference VO_Story_Hero_Tichondrius_Male_Demon_Story_Illidan_Mission2Idle_03 = new AssetReference("VO_Story_Hero_Tichondrius_Male_Demon_Story_Illidan_Mission2Idle_03.prefab:94f3f04e865105f46bd8cc54010a4c24");

	private static readonly AssetReference VO_Story_Hero_Tichondrius_Male_Demon_Story_Illidan_Mission2Intro_01 = new AssetReference("VO_Story_Hero_Tichondrius_Male_Demon_Story_Illidan_Mission2Intro_01.prefab:2c33fae495f6d32459227e260d4063fa");

	private static readonly AssetReference VO_Story_Hero_Tichondrius_Male_Demon_Story_Illidan_Mission2Loss_01 = new AssetReference("VO_Story_Hero_Tichondrius_Male_Demon_Story_Illidan_Mission2Loss_01.prefab:165001fc4ef97374d8b7ae77f3b2282d");

	private Dictionary<int, string[]> m_popUpInfo = new Dictionary<int, string[]> { 
	{
		228,
		new string[1] { "BOH_ILLIDAN_02" }
	} };

	private float popUpScale = 1.25f;

	private Vector3 popUpPos;

	public static readonly AssetReference MalfurionBrassRing = new AssetReference("Malfurion_BrassRing_Quote.prefab:854afc33ad3808447935b8cb9753d3a8");

	private List<string> m_BossUsesHeroPowerLines = new List<string> { VO_Story_Hero_Tichondrius_Male_Demon_Story_Illidan_Mission2HeroPower_01, VO_Story_Hero_Tichondrius_Male_Demon_Story_Illidan_Mission2HeroPower_02, VO_Story_Hero_Tichondrius_Male_Demon_Story_Illidan_Mission2HeroPower_03 };

	private new List<string> m_BossIdleLines = new List<string> { VO_Story_Hero_Tichondrius_Male_Demon_Story_Illidan_Mission2Idle_01, VO_Story_Hero_Tichondrius_Male_Demon_Story_Illidan_Mission2Idle_02, VO_Story_Hero_Tichondrius_Male_Demon_Story_Illidan_Mission2Idle_03 };

	private HashSet<string> m_playedLines = new HashSet<string>();

	private static Map<GameEntityOption, bool> InitBooleanOptions()
	{
		return new Map<GameEntityOption, bool> { 
		{
			GameEntityOption.DO_OPENING_TAUNTS,
			false
		} };
	}

	public BoH_Illidan_02()
	{
		m_gameOptions.AddBooleanOptions(s_booleanOptions);
	}

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> m_SoundFiles = new List<string>
		{
			VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission2ExchangeA_02, VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission2ExchangeB_02, VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission2ExchangeC_01, VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission2ExchangeD_01, VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission2ExchangeE_01, VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission2ExchangeE_02, VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission2ExchangeE_04, VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission2Intro_01, VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission2Victory_02, VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission2Victory_04,
			VO_Story_Hero_Malfurion_Male_NightElf_Story_Illidan_Mission2Victory_01, VO_Story_Hero_Malfurion_Male_NightElf_Story_Illidan_Mission2Victory_03, VO_Story_Hero_Tichondrius_Male_Demon_Story_Illidan_Mission2Death_01, VO_Story_Hero_Tichondrius_Male_Demon_Story_Illidan_Mission2EmoteResponse_01, VO_Story_Hero_Tichondrius_Male_Demon_Story_Illidan_Mission2ExchangeB_01, VO_Story_Hero_Tichondrius_Male_Demon_Story_Illidan_Mission2ExchangeC_01, VO_Story_Hero_Tichondrius_Male_Demon_Story_Illidan_Mission2ExchangeD_02, VO_Story_Hero_Tichondrius_Male_Demon_Story_Illidan_Mission2ExchangeE_02, VO_Story_Hero_Tichondrius_Male_Demon_Story_Illidan_Mission2HeroPower_01, VO_Story_Hero_Tichondrius_Male_Demon_Story_Illidan_Mission2HeroPower_02,
			VO_Story_Hero_Tichondrius_Male_Demon_Story_Illidan_Mission2HeroPower_03, VO_Story_Hero_Tichondrius_Male_Demon_Story_Illidan_Mission2Idle_01, VO_Story_Hero_Tichondrius_Male_Demon_Story_Illidan_Mission2Idle_02, VO_Story_Hero_Tichondrius_Male_Demon_Story_Illidan_Mission2Idle_03, VO_Story_Hero_Tichondrius_Male_Demon_Story_Illidan_Mission2Intro_01, VO_Story_Hero_Tichondrius_Male_Demon_Story_Illidan_Mission2Loss_01
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
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Tichondrius_Male_Demon_Story_Illidan_Mission2EmoteResponse_01);
			break;
		case 514:
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Tichondrius_Male_Demon_Story_Illidan_Mission2Intro_01);
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission2Intro_01);
			break;
		case 507:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Tichondrius_Male_Demon_Story_Illidan_Mission2Loss_01);
			GameState.Get().SetBusy(busy: false);
			break;
		case 504:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(GetFriendlyActorByCardId("Story_01_Malfurion"), VO_Story_Hero_Malfurion_Male_NightElf_Story_Illidan_Mission2Victory_01);
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission2Victory_02);
			yield return MissionPlayVO(GetFriendlyActorByCardId("Story_01_Malfurion"), VO_Story_Hero_Malfurion_Male_NightElf_Story_Illidan_Mission2Victory_03);
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission2Victory_04);
			GameState.Get().SetBusy(busy: false);
			break;
		case 101:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission2ExchangeE_01);
			GameState.Get().SetBusy(busy: false);
			break;
		case 102:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission2ExchangeE_02);
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Tichondrius_Male_Demon_Story_Illidan_Mission2ExchangeE_02);
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission2ExchangeE_04);
			GameState.Get().SetBusy(busy: false);
			break;
		case 228:
		{
			yield return new WaitForSeconds(2f);
			Notification popup = NotificationManager.Get().CreatePopupText(UserAttentionBlocker.NONE, popUpPos, TutorialEntity.GetTextScale() * popUpScale, GameStrings.Get(m_popUpInfo[missionEvent][0]), convertLegacyPosition: false, NotificationManager.PopupTextType.FANCY);
			NotificationManager.Get().DestroyNotification(popup, 7.5f);
			break;
		}
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
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission2ExchangeA_02);
			break;
		case 7:
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Tichondrius_Male_Demon_Story_Illidan_Mission2ExchangeB_01);
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission2ExchangeB_02);
			break;
		case 11:
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Tichondrius_Male_Demon_Story_Illidan_Mission2ExchangeC_01);
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission2ExchangeC_01);
			break;
		case 13:
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission2ExchangeD_01);
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Tichondrius_Male_Demon_Story_Illidan_Mission2ExchangeD_02);
			break;
		}
	}
}
