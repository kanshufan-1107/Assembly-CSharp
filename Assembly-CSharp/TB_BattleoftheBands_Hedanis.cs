using System.Collections;
using System.Collections.Generic;
using Blizzard.T5.Core;
using UnityEngine;

public class TB_BattleoftheBands_Hedanis : TB_BattleoftheBands_Dungeon
{
	private static Map<GameEntityOption, bool> s_booleanOptions = InitBooleanOptions();

	private static readonly AssetReference VO_TB_BotB_Hedanis_Male_VoidElf_TB_BotB_Mission5EmoteResponse_01 = new AssetReference("VO_TB_BotB_Hedanis_Male_VoidElf_TB_BotB_Mission5EmoteResponse_01.prefab:215c9c3bef3678846be5af911d22b5de");

	private static readonly AssetReference VO_TB_BotB_Hedanis_Male_VoidElf_TB_BotB_Mission5ExchangeA_01 = new AssetReference("VO_TB_BotB_Hedanis_Male_VoidElf_TB_BotB_Mission5ExchangeA_01.prefab:edb7492d31111864fb388f8279e9440e");

	private static readonly AssetReference VO_TB_BotB_Hedanis_Male_VoidElf_TB_BotB_Mission5ExchangeB_01 = new AssetReference("VO_TB_BotB_Hedanis_Male_VoidElf_TB_BotB_Mission5ExchangeB_01.prefab:e8c7dff16b8cdc043b052bb79ae526b8");

	private static readonly AssetReference VO_TB_BotB_Hedanis_Male_VoidElf_TB_BotB_Mission5ExchangeC_01 = new AssetReference("VO_TB_BotB_Hedanis_Male_VoidElf_TB_BotB_Mission5ExchangeC_01.prefab:5609b598510328940be1545725f3ec3e");

	private static readonly AssetReference VO_TB_BotB_Hedanis_Male_VoidElf_TB_BotB_Mission5HeroPower_01 = new AssetReference("VO_TB_BotB_Hedanis_Male_VoidElf_TB_BotB_Mission5HeroPower_01.prefab:e30894531989f4d44b2130b4045d1d27");

	private static readonly AssetReference VO_TB_BotB_Hedanis_Male_VoidElf_TB_BotB_Mission5HeroPower_02 = new AssetReference("VO_TB_BotB_Hedanis_Male_VoidElf_TB_BotB_Mission5HeroPower_02.prefab:d3a7a3a0e3d2fb8439c6db7a7989ea2a");

	private static readonly AssetReference VO_TB_BotB_Hedanis_Male_VoidElf_TB_BotB_Mission5HeroPower_03 = new AssetReference("VO_TB_BotB_Hedanis_Male_VoidElf_TB_BotB_Mission5HeroPower_03.prefab:504362ae61ab40f4b960fdcda2f8d2a9");

	private static readonly AssetReference VO_TB_BotB_Hedanis_Male_VoidElf_TB_BotB_Mission5Idle_01 = new AssetReference("VO_TB_BotB_Hedanis_Male_VoidElf_TB_BotB_Mission5Idle_01.prefab:5eb56a9aa20fb9b49874862a3ddfd30c");

	private static readonly AssetReference VO_TB_BotB_Hedanis_Male_VoidElf_TB_BotB_Mission5Idle_02 = new AssetReference("VO_TB_BotB_Hedanis_Male_VoidElf_TB_BotB_Mission5Idle_02.prefab:c73969d2e476e6e4d9dfa8d934eb1bae");

	private static readonly AssetReference VO_TB_BotB_Hedanis_Male_VoidElf_TB_BotB_Mission5Idle_03 = new AssetReference("VO_TB_BotB_Hedanis_Male_VoidElf_TB_BotB_Mission5Idle_03.prefab:cafcca2d12ec1374b8ce81be70494a92");

	private static readonly AssetReference VO_TB_BotB_Hedanis_Male_VoidElf_TB_BotB_Mission5Loss_01 = new AssetReference("VO_TB_BotB_Hedanis_Male_VoidElf_TB_BotB_Mission5Loss_01.prefab:f6e153a0353c1974687ff69cade6600a");

	private static readonly AssetReference VO_TB_BotB_Hedanis_Male_VoidElf_TB_BotB_Mission5Start_01 = new AssetReference("VO_TB_BotB_Hedanis_Male_VoidElf_TB_BotB_Mission5Start_01.prefab:775a535df235ba74694c7623b0ff8dcb");

	private static readonly AssetReference VO_TB_BotB_Hedanis_Male_VoidElf_TB_BotB_Mission5Victory_01 = new AssetReference("VO_TB_BotB_Hedanis_Male_VoidElf_TB_BotB_Mission5Victory_01.prefab:e99c3a9824b9a234f885d56c8bbd5ab8");

	private static readonly AssetReference VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission5ExchangeA_02 = new AssetReference("VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission5ExchangeA_02.prefab:43296cb4d56236f4180de755aa6ba7bd");

	private static readonly AssetReference VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission5ExchangeB_02 = new AssetReference("VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission5ExchangeB_02.prefab:6ef9dae964e30434ca0749ecf064296f");

	private static readonly AssetReference VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission5Start_02 = new AssetReference("VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission5Start_02.prefab:7450b5d37986ab5458287d89384eaad0");

	private static readonly AssetReference VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission5Victory_02 = new AssetReference("VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission5Victory_02.prefab:2d34664ddf0a4e549bad48ba51dea69b");

	private static readonly AssetReference VO_TB_BotB_Mission5_Lorebook1_Male_Goblin_TB_BotB_Mission5Lorebook_01 = new AssetReference("VO_TB_BotB_Mission5_Lorebook1_Male_Goblin_TB_BotB_Mission5Lorebook_01.prefab:3be9b8dd6f55289448d20cf73e0f1a4b");

	private static readonly AssetReference VO_TB_BotB_Mission5_Lorebook1_Male_Goblin_TB_BotB_Mission5Lorebook_02 = new AssetReference("VO_TB_BotB_Mission5_Lorebook1_Male_Goblin_TB_BotB_Mission5Lorebook_02.prefab:7c87253fffe5e3e4f9625458c53503d8");

	private Notification m_popup;

	private Dictionary<int, string[]> m_popUpInfo = new Dictionary<int, string[]>
	{
		{
			228,
			new string[1] { "TB_BOTB_HEDANIS_LOREBOOK1" }
		},
		{
			229,
			new string[1] { "TB_BOTB_HEDANIS_LOREBOOK1" }
		}
	};

	private float popUpScale = 1.65f;

	private Vector3 popUpPos;

	private new List<string> m_BossIdleLines = new List<string> { VO_TB_BotB_Hedanis_Male_VoidElf_TB_BotB_Mission5Idle_01, VO_TB_BotB_Hedanis_Male_VoidElf_TB_BotB_Mission5Idle_02, VO_TB_BotB_Hedanis_Male_VoidElf_TB_BotB_Mission5Idle_03 };

	private List<string> m_BossUsesHeroPowerLines = new List<string> { VO_TB_BotB_Hedanis_Male_VoidElf_TB_BotB_Mission5HeroPower_01, VO_TB_BotB_Hedanis_Male_VoidElf_TB_BotB_Mission5HeroPower_02, VO_TB_BotB_Hedanis_Male_VoidElf_TB_BotB_Mission5HeroPower_03 };

	private HashSet<string> m_playedLines = new HashSet<string>();

	private static Map<GameEntityOption, bool> InitBooleanOptions()
	{
		return new Map<GameEntityOption, bool> { 
		{
			GameEntityOption.DO_OPENING_TAUNTS,
			false
		} };
	}

	public TB_BattleoftheBands_Hedanis()
	{
		m_gameOptions.AddBooleanOptions(s_booleanOptions);
	}

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> m_SoundFiles = new List<string>
		{
			VO_TB_BotB_Hedanis_Male_VoidElf_TB_BotB_Mission5Start_01, VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission5Start_02, VO_TB_BotB_Hedanis_Male_VoidElf_TB_BotB_Mission5ExchangeA_01, VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission5ExchangeA_02, VO_TB_BotB_Hedanis_Male_VoidElf_TB_BotB_Mission5ExchangeB_01, VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission5ExchangeB_02, VO_TB_BotB_Hedanis_Male_VoidElf_TB_BotB_Mission5ExchangeC_01, VO_TB_BotB_Hedanis_Male_VoidElf_TB_BotB_Mission5Victory_01, VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission5Victory_02, VO_TB_BotB_Hedanis_Male_VoidElf_TB_BotB_Mission5HeroPower_01,
			VO_TB_BotB_Hedanis_Male_VoidElf_TB_BotB_Mission5HeroPower_02, VO_TB_BotB_Hedanis_Male_VoidElf_TB_BotB_Mission5HeroPower_03, VO_TB_BotB_Hedanis_Male_VoidElf_TB_BotB_Mission5Idle_01, VO_TB_BotB_Hedanis_Male_VoidElf_TB_BotB_Mission5Idle_02, VO_TB_BotB_Hedanis_Male_VoidElf_TB_BotB_Mission5Idle_03, VO_TB_BotB_Hedanis_Male_VoidElf_TB_BotB_Mission5Loss_01, VO_TB_BotB_Hedanis_Male_VoidElf_TB_BotB_Mission5EmoteResponse_01, VO_TB_BotB_Mission5_Lorebook1_Male_Goblin_TB_BotB_Mission5Lorebook_01, VO_TB_BotB_Mission5_Lorebook1_Male_Goblin_TB_BotB_Mission5Lorebook_02
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
			yield return MissionPlayVO(enemyActor, VO_TB_BotB_Hedanis_Male_VoidElf_TB_BotB_Mission5EmoteResponse_01);
			break;
		case 514:
			yield return MissionPlayVO(enemyActor, VO_TB_BotB_Hedanis_Male_VoidElf_TB_BotB_Mission5Start_01);
			yield return MissionPlayVO(friendlyActor, VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission5Start_02);
			break;
		case 507:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(enemyActor, VO_TB_BotB_Hedanis_Male_VoidElf_TB_BotB_Mission5Loss_01);
			GameState.Get().SetBusy(busy: false);
			break;
		case 504:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(enemyActor, VO_TB_BotB_Hedanis_Male_VoidElf_TB_BotB_Mission5Victory_01);
			yield return MissionPlayVO(friendlyActor, VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission5Victory_02);
			GameState.Get().SetBusy(busy: false);
			break;
		case 228:
			if (!(m_popup != null))
			{
				GameState.Get().SetBusy(busy: true);
				m_popup = NotificationManager.Get().CreatePopupText(UserAttentionBlocker.NONE, popUpPos, TutorialEntity.GetTextScale() * popUpScale, GameStrings.Get(m_popUpInfo[missionEvent][0]), convertLegacyPosition: false, NotificationManager.PopupTextType.FANCY);
				yield return GameEntity.Coroutines.StartCoroutine(PlaySoundAndBlockSpeech(VO_TB_BotB_Mission5_Lorebook1_Male_Goblin_TB_BotB_Mission5Lorebook_01, Notification.SpeechBubbleDirection.None, GetEnemyActorByCardId("TB_BotB_Hedanis_Lorebook1"), 2.5f));
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
				yield return GameEntity.Coroutines.StartCoroutine(PlaySoundAndBlockSpeech(VO_TB_BotB_Mission5_Lorebook1_Male_Goblin_TB_BotB_Mission5Lorebook_01, Notification.SpeechBubbleDirection.None, GetEnemyActorByCardId("Story_11_Mission1_Lorebook1"), 2.5f));
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
			yield return MissionPlayVO(enemyActor, VO_TB_BotB_Hedanis_Male_VoidElf_TB_BotB_Mission5ExchangeA_01);
			yield return MissionPlayVO(friendlyActor, VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission5ExchangeA_02);
			break;
		case 7:
			yield return MissionPlayVO(enemyActor, VO_TB_BotB_Hedanis_Male_VoidElf_TB_BotB_Mission5ExchangeB_01);
			yield return MissionPlayVO(friendlyActor, VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission5ExchangeB_02);
			break;
		case 12:
			yield return MissionPlayVO(enemyActor, VO_TB_BotB_Hedanis_Male_VoidElf_TB_BotB_Mission5ExchangeC_01);
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
			if (clickedEntity.GetCardId() == "Story_11_Mission3_Lorebook1")
			{
				if (m_popup == null)
				{
					Network.Get().SendClientScriptGameEvent(ClientScriptGameEventType.MISSION_EVENT, 228);
					HandleMissionEvent(228);
				}
				return false;
			}
			if (clickedEntity.GetCardId() == "TB_BotB_Hedanis_Lorebook1")
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
