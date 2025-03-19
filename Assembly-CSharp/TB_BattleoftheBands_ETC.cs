using System.Collections;
using System.Collections.Generic;
using Blizzard.T5.Core;
using UnityEngine;

public class TB_BattleoftheBands_ETC : TB_BattleoftheBands_Dungeon
{
	private static Map<GameEntityOption, bool> s_booleanOptions = InitBooleanOptions();

	private static readonly AssetReference VO_TB_BotB_ETC_Male_Tauren_TB_BotB_Mission12EmoteResponse_01 = new AssetReference("VO_TB_BotB_ETC_Male_Tauren_TB_BotB_Mission12EmoteResponse_01.prefab:1bc5db04941c47c449b52a541a16fb17");

	private static readonly AssetReference VO_TB_BotB_ETC_Male_Tauren_TB_BotB_Mission12ExchangeA_01 = new AssetReference("VO_TB_BotB_ETC_Male_Tauren_TB_BotB_Mission12ExchangeA_01.prefab:d4e4c50f8aca78a469d3d4edae4a92f0");

	private static readonly AssetReference VO_TB_BotB_ETC_Male_Tauren_TB_BotB_Mission12ExchangeA_03 = new AssetReference("VO_TB_BotB_ETC_Male_Tauren_TB_BotB_Mission12ExchangeA_03.prefab:c2be9ce2dd266704382cfb6738032a33");

	private static readonly AssetReference VO_TB_BotB_ETC_Male_Tauren_TB_BotB_Mission12ExchangeA_04 = new AssetReference("VO_TB_BotB_ETC_Male_Tauren_TB_BotB_Mission12ExchangeA_04.prefab:72fb6914624a56f4698521db7111c27d");

	private static readonly AssetReference VO_TB_BotB_ETC_Male_Tauren_TB_BotB_Mission12ExchangeB_02 = new AssetReference("VO_TB_BotB_ETC_Male_Tauren_TB_BotB_Mission12ExchangeB_02.prefab:8aa52017ca2bad842996cf0e33184999");

	private static readonly AssetReference VO_TB_BotB_ETC_Male_Tauren_TB_BotB_Mission12ExchangeB_04 = new AssetReference("VO_TB_BotB_ETC_Male_Tauren_TB_BotB_Mission12ExchangeB_04.prefab:9d41ca071a746d14c91f0c61707f7234");

	private static readonly AssetReference VO_TB_BotB_ETC_Male_Tauren_TB_BotB_Mission12ExchangeC_02 = new AssetReference("VO_TB_BotB_ETC_Male_Tauren_TB_BotB_Mission12ExchangeC_02.prefab:0facdf55bcd7865419c7145a5c1aa2bf");

	private static readonly AssetReference VO_TB_BotB_ETC_Male_Tauren_TB_BotB_Mission12ExchangeC_04 = new AssetReference("VO_TB_BotB_ETC_Male_Tauren_TB_BotB_Mission12ExchangeC_04.prefab:14acc934c90672142a8b6a8092453b90");

	private static readonly AssetReference VO_TB_BotB_ETC_Male_Tauren_TB_BotB_Mission12HeroPower_01 = new AssetReference("VO_TB_BotB_ETC_Male_Tauren_TB_BotB_Mission12HeroPower_01.prefab:fe3f0fdfe2ef4464fab98e18a3cd19cc");

	private static readonly AssetReference VO_TB_BotB_ETC_Male_Tauren_TB_BotB_Mission12HeroPower_02 = new AssetReference("VO_TB_BotB_ETC_Male_Tauren_TB_BotB_Mission12HeroPower_02.prefab:ea75ecc4f08fea44bb442df6892fbbe5");

	private static readonly AssetReference VO_TB_BotB_ETC_Male_Tauren_TB_BotB_Mission12HeroPower_03 = new AssetReference("VO_TB_BotB_ETC_Male_Tauren_TB_BotB_Mission12HeroPower_03.prefab:fec9d1e425d99ac40b3f3cbb01b1835c");

	private static readonly AssetReference VO_TB_BotB_ETC_Male_Tauren_TB_BotB_Mission12Idle_01 = new AssetReference("VO_TB_BotB_ETC_Male_Tauren_TB_BotB_Mission12Idle_01.prefab:9088055a9da771e4cacbf2160fe0791c");

	private static readonly AssetReference VO_TB_BotB_ETC_Male_Tauren_TB_BotB_Mission12Idle_02 = new AssetReference("VO_TB_BotB_ETC_Male_Tauren_TB_BotB_Mission12Idle_02.prefab:98f84e52314914c41a9a743be620adb7");

	private static readonly AssetReference VO_TB_BotB_ETC_Male_Tauren_TB_BotB_Mission12Idle_03 = new AssetReference("VO_TB_BotB_ETC_Male_Tauren_TB_BotB_Mission12Idle_03.prefab:28992a8d55d035845a782e254cb90d9f");

	private static readonly AssetReference VO_TB_BotB_ETC_Male_Tauren_TB_BotB_Mission12Lorebook_01 = new AssetReference("VO_TB_BotB_ETC_Male_Tauren_TB_BotB_Mission12Lorebook_01.prefab:de2feb8f5d1b1594fbd1714fae57aef9");

	private static readonly AssetReference VO_TB_BotB_ETC_Male_Tauren_TB_BotB_Mission12Loss_01 = new AssetReference("VO_TB_BotB_ETC_Male_Tauren_TB_BotB_Mission12Loss_01.prefab:7827b933fb28a7b4b8ec3f1d75d49eb9");

	private static readonly AssetReference VO_TB_BotB_ETC_Male_Tauren_TB_BotB_Mission12Start_01 = new AssetReference("VO_TB_BotB_ETC_Male_Tauren_TB_BotB_Mission12Start_01.prefab:5ddb3604c757e094aa81a67072f54019");

	private static readonly AssetReference VO_TB_BotB_ETC_Male_Tauren_TB_BotB_Mission12Victory_01 = new AssetReference("VO_TB_BotB_ETC_Male_Tauren_TB_BotB_Mission12Victory_01.prefab:c505ca9b1905aed49a9aa320d70b87e3");

	private static readonly AssetReference VO_TB_BotB_ETC_Male_Tauren_TB_BotB_Mission12Victory_03 = new AssetReference("VO_TB_BotB_ETC_Male_Tauren_TB_BotB_Mission12Victory_03.prefab:6e0a30360aacf3645ac26c6871a868ab");

	private static readonly AssetReference VO_TB_BotB_ETC_Male_Tauren_TB_BotB_Mission12Victory_05 = new AssetReference("VO_TB_BotB_ETC_Male_Tauren_TB_BotB_Mission12Victory_05.prefab:c09873990b89e49439f1c9592596c24b");

	private static readonly AssetReference VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission12ExchangeA_02 = new AssetReference("VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission12ExchangeA_02.prefab:d3b9b1af501fdb84790bf90ba5783125");

	private static readonly AssetReference VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission12ExchangeB_01 = new AssetReference("VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission12ExchangeB_01.prefab:920a344a5a1371d4c91b193c10772fe5");

	private static readonly AssetReference VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission12ExchangeB_03 = new AssetReference("VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission12ExchangeB_03.prefab:62ef37df9e6b71a4ab2510e96609014a");

	private static readonly AssetReference VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission12ExchangeC_01 = new AssetReference("VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission12ExchangeC_01.prefab:c73a473d223719848bbc72a1a25bb4af");

	private static readonly AssetReference VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission12ExchangeC_03 = new AssetReference("VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission12ExchangeC_03.prefab:e5bc4504cff72934396a98a7a3605310");

	private static readonly AssetReference VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission12Start_02 = new AssetReference("VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission12Start_02.prefab:63e8dc8ccc6e4c84a825efc21a6d6b33");

	private static readonly AssetReference VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission12Victory_02 = new AssetReference("VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission12Victory_02.prefab:8bc6c8db62496d841a256657f03c3def");

	private static readonly AssetReference VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission12Victory_04 = new AssetReference("VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission12Victory_04.prefab:0e63db10b3f27c24eabca3441bd54f52");

	private static readonly AssetReference VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission12Victory_06 = new AssetReference("VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission12Victory_06.prefab:947b57dba257ba643ae175244e5f0024");

	private static readonly AssetReference VO_TB_BotB_Mission5_Lorebook1_Male_Goblin_TB_BotB_Mission5Lorebook_02 = new AssetReference("VO_TB_BotB_Mission5_Lorebook1_Male_Goblin_TB_BotB_Mission5Lorebook_02.prefab:1c92a87ab146ee44980412b996cf6355");

	private Notification m_popup;

	private Dictionary<int, string[]> m_popUpInfo = new Dictionary<int, string[]>
	{
		{
			228,
			new string[1] { "TB_BOTB_ETC_LOREBOOK1" }
		},
		{
			229,
			new string[1] { "TB_BOTB_HEDANIS_LOREBOOK2" }
		}
	};

	private float popUpScale = 1.65f;

	private Vector3 popUpPos;

	private new List<string> m_BossIdleLines = new List<string> { VO_TB_BotB_ETC_Male_Tauren_TB_BotB_Mission12Idle_01, VO_TB_BotB_ETC_Male_Tauren_TB_BotB_Mission12Idle_02, VO_TB_BotB_ETC_Male_Tauren_TB_BotB_Mission12Idle_03 };

	private List<string> m_BossUsesHeroPowerLines = new List<string> { VO_TB_BotB_ETC_Male_Tauren_TB_BotB_Mission12HeroPower_01, VO_TB_BotB_ETC_Male_Tauren_TB_BotB_Mission12HeroPower_02, VO_TB_BotB_ETC_Male_Tauren_TB_BotB_Mission12HeroPower_03 };

	private HashSet<string> m_playedLines = new HashSet<string>();

	private static Map<GameEntityOption, bool> InitBooleanOptions()
	{
		return new Map<GameEntityOption, bool> { 
		{
			GameEntityOption.DO_OPENING_TAUNTS,
			false
		} };
	}

	public TB_BattleoftheBands_ETC()
	{
		m_gameOptions.AddBooleanOptions(s_booleanOptions);
	}

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> m_SoundFiles = new List<string>
		{
			VO_TB_BotB_ETC_Male_Tauren_TB_BotB_Mission12Start_01, VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission12Start_02, VO_TB_BotB_ETC_Male_Tauren_TB_BotB_Mission12ExchangeA_01, VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission12ExchangeA_02, VO_TB_BotB_ETC_Male_Tauren_TB_BotB_Mission12ExchangeA_03, VO_TB_BotB_ETC_Male_Tauren_TB_BotB_Mission12ExchangeA_04, VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission12ExchangeB_01, VO_TB_BotB_ETC_Male_Tauren_TB_BotB_Mission12ExchangeB_02, VO_TB_BotB_ETC_Male_Tauren_TB_BotB_Mission12ExchangeB_04, VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission12ExchangeC_01,
			VO_TB_BotB_ETC_Male_Tauren_TB_BotB_Mission12ExchangeC_02, VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission12ExchangeC_03, VO_TB_BotB_ETC_Male_Tauren_TB_BotB_Mission12ExchangeC_04, VO_TB_BotB_ETC_Male_Tauren_TB_BotB_Mission12Victory_01, VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission12Victory_02, VO_TB_BotB_ETC_Male_Tauren_TB_BotB_Mission12Victory_03, VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission12Victory_04, VO_TB_BotB_ETC_Male_Tauren_TB_BotB_Mission12Victory_05, VO_TB_BotB_ETC_Male_Tauren_TB_BotB_Mission12HeroPower_01, VO_TB_BotB_ETC_Male_Tauren_TB_BotB_Mission12HeroPower_02,
			VO_TB_BotB_ETC_Male_Tauren_TB_BotB_Mission12HeroPower_03, VO_TB_BotB_ETC_Male_Tauren_TB_BotB_Mission12Idle_01, VO_TB_BotB_ETC_Male_Tauren_TB_BotB_Mission12Idle_02, VO_TB_BotB_ETC_Male_Tauren_TB_BotB_Mission12Idle_03, VO_TB_BotB_ETC_Male_Tauren_TB_BotB_Mission12Loss_01, VO_TB_BotB_ETC_Male_Tauren_TB_BotB_Mission12EmoteResponse_01, VO_TB_BotB_ETC_Male_Tauren_TB_BotB_Mission12Lorebook_01, VO_TB_BotB_Mission5_Lorebook1_Male_Goblin_TB_BotB_Mission5Lorebook_02
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

	public override List<string> GetBossIdleLines()
	{
		return m_BossIdleLines;
	}

	public override List<string> GetBossHeroPowerRandomLines()
	{
		return m_BossUsesHeroPowerLines;
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
		popUpPos = new Vector3(0f, 0f, -40f);
		switch (missionEvent)
		{
		case 515:
			yield return MissionPlayVO(enemyActor, VO_TB_BotB_ETC_Male_Tauren_TB_BotB_Mission12EmoteResponse_01);
			break;
		case 514:
			yield return MissionPlayVO(enemyActor, VO_TB_BotB_ETC_Male_Tauren_TB_BotB_Mission12Start_01);
			yield return MissionPlayVO(friendlyActor, VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission12Start_02);
			break;
		case 507:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(enemyActor, VO_TB_BotB_ETC_Male_Tauren_TB_BotB_Mission12Loss_01);
			GameState.Get().SetBusy(busy: false);
			break;
		case 504:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(enemyActor, VO_TB_BotB_ETC_Male_Tauren_TB_BotB_Mission12Victory_01);
			yield return MissionPlayVO(friendlyActor, VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission12Victory_02);
			yield return MissionPlayVO(enemyActor, VO_TB_BotB_ETC_Male_Tauren_TB_BotB_Mission12Victory_03);
			yield return MissionPlayVO(friendlyActor, VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission12Victory_04);
			yield return MissionPlayVO(enemyActor, VO_TB_BotB_ETC_Male_Tauren_TB_BotB_Mission12Victory_05);
			GameState.Get().SetBusy(busy: false);
			break;
		case 228:
			if (!(m_popup != null))
			{
				GameState.Get().SetBusy(busy: true);
				m_popup = NotificationManager.Get().CreatePopupText(UserAttentionBlocker.NONE, popUpPos, TutorialEntity.GetTextScale() * popUpScale, GameStrings.Get(m_popUpInfo[missionEvent][0]), convertLegacyPosition: false, NotificationManager.PopupTextType.FANCY);
				yield return GameEntity.Coroutines.StartCoroutine(PlaySoundAndBlockSpeech(VO_TB_BotB_ETC_Male_Tauren_TB_BotB_Mission12Lorebook_01, Notification.SpeechBubbleDirection.None, GetEnemyActorByCardId("TB_BotB_ETC_Lorebook1"), 2.5f));
				NotificationManager.Get().DestroyNotification(m_popup, 0f);
				m_popup = null;
				GameState.Get().SetBusy(busy: false);
			}
			break;
		case 229:
			if (!(m_popup != null))
			{
				GameState.Get().SetBusy(busy: true);
				m_popup = NotificationManager.Get().CreatePopupText(UserAttentionBlocker.NONE, popUpPos, TutorialEntity.GetTextScale() * popUpScale, GameStrings.Get(m_popUpInfo[missionEvent][0]), convertLegacyPosition: false, NotificationManager.PopupTextType.FANCY);
				yield return GameEntity.Coroutines.StartCoroutine(PlaySoundAndBlockSpeech(VO_TB_BotB_Mission5_Lorebook1_Male_Goblin_TB_BotB_Mission5Lorebook_02, Notification.SpeechBubbleDirection.None, GetEnemyActorByCardId("TB_BotB_Hedanis_Lorebook2"), 2.5f));
				NotificationManager.Get().DestroyNotification(m_popup, 0f);
				m_popup = null;
				GameState.Get().SetBusy(busy: false);
			}
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
			yield return MissionPlayVO(enemyActor, VO_TB_BotB_ETC_Male_Tauren_TB_BotB_Mission12ExchangeA_01);
			yield return MissionPlayVO(friendlyActor, VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission12ExchangeA_02);
			yield return MissionPlayVO(enemyActor, VO_TB_BotB_ETC_Male_Tauren_TB_BotB_Mission12ExchangeA_03);
			yield return MissionPlayVO(enemyActor, VO_TB_BotB_ETC_Male_Tauren_TB_BotB_Mission12ExchangeA_04);
			break;
		case 9:
			yield return MissionPlayVO(friendlyActor, VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission12ExchangeB_01);
			yield return MissionPlayVO(enemyActor, VO_TB_BotB_ETC_Male_Tauren_TB_BotB_Mission12ExchangeB_02);
			yield return MissionPlayVO(enemyActor, VO_TB_BotB_ETC_Male_Tauren_TB_BotB_Mission12ExchangeB_04);
			break;
		case 13:
			yield return MissionPlayVO(friendlyActor, VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission12ExchangeC_01);
			yield return MissionPlayVO(enemyActor, VO_TB_BotB_ETC_Male_Tauren_TB_BotB_Mission12ExchangeC_02);
			yield return MissionPlayVO(friendlyActor, VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission12ExchangeC_03);
			yield return MissionPlayVO(enemyActor, VO_TB_BotB_ETC_Male_Tauren_TB_BotB_Mission12ExchangeC_04);
			break;
		}
	}

	public override bool NotifyOfBattlefieldCardClicked(Entity clickedEntity, bool wasInTargetMode)
	{
		if (!base.NotifyOfBattlefieldCardClicked(clickedEntity, wasInTargetMode))
		{
			return false;
		}
		if (!wasInTargetMode)
		{
			if (clickedEntity.GetCardId() == "TB_BotB_ETC_Lorebook1")
			{
				if (m_popup == null)
				{
					Network.Get().SendClientScriptGameEvent(ClientScriptGameEventType.MISSION_EVENT, 228);
					HandleMissionEvent(228);
				}
				return false;
			}
			if (clickedEntity.GetCardId() == "TB_BotB_Hedanis_Lorebook2")
			{
				if (m_popup == null)
				{
					Network.Get().SendClientScriptGameEvent(ClientScriptGameEventType.MISSION_EVENT, 229);
					HandleMissionEvent(229);
				}
				return false;
			}
		}
		return true;
	}
}
