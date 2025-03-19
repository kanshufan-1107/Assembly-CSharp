using System.Collections;
using System.Collections.Generic;
using Blizzard.T5.Core;
using UnityEngine;

public class TB_BattleoftheBands_Voone : TB_BattleoftheBands_Dungeon
{
	private static Map<GameEntityOption, bool> s_booleanOptions = InitBooleanOptions();

	private static readonly AssetReference VO_TB_BotB_Mission10_Lorebook1_Male_Goblin_TB_BotB_Mission10Lorebook_01 = new AssetReference("VO_TB_BotB_Mission10_Lorebook1_Male_Goblin_TB_BotB_Mission10Lorebook_01.prefab:9975316c14550ef41b5ac65094a7b780");

	private static readonly AssetReference VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission10ExchangeA_02 = new AssetReference("VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission10ExchangeA_02.prefab:a5d31a766b5c0d74ab2feda9b57a4e25");

	private static readonly AssetReference VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission10ExchangeA_03 = new AssetReference("VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission10ExchangeA_03.prefab:3765dc3ce97ad5149aeade7d6d6f2aaa");

	private static readonly AssetReference VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission10ExchangeB_01 = new AssetReference("VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission10ExchangeB_01.prefab:bebd4c414f0279340a70b3f4f3fbb386");

	private static readonly AssetReference VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission10ExchangeC_02 = new AssetReference("VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission10ExchangeC_02.prefab:a356febb96c578f4da2ae505df22abc3");

	private static readonly AssetReference VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission10ExchangeC_04 = new AssetReference("VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission10ExchangeC_04.prefab:83485d23b77d69b428d880b180abc0f4");

	private static readonly AssetReference VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission10ExchangeC_05 = new AssetReference("VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission10ExchangeC_05.prefab:fb3337c8b90b9c241ae078f3b3549e0a");

	private static readonly AssetReference VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission10ExchangeC_06 = new AssetReference("VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission10ExchangeC_06.prefab:da9152de34c96a34db05eda9c1094e02");

	private static readonly AssetReference VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission10Start_02 = new AssetReference("VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission10Start_02.prefab:811fe5f13ba74e346a1ee70b4d45486c");

	private static readonly AssetReference VO_TB_BotB_Voone_Male_Troll_TB_BotB_Mission10EmoteResponse_01 = new AssetReference("VO_TB_BotB_Voone_Male_Troll_TB_BotB_Mission10EmoteResponse_01.prefab:f6b35961cff4f224c8494584bc5ef898");

	private static readonly AssetReference VO_TB_BotB_Voone_Male_Troll_TB_BotB_Mission10ExchangeA_01 = new AssetReference("VO_TB_BotB_Voone_Male_Troll_TB_BotB_Mission10ExchangeA_01.prefab:d516af28442ac0b48ae03512439803f3");

	private static readonly AssetReference VO_TB_BotB_Voone_Male_Troll_TB_BotB_Mission10ExchangeB_02 = new AssetReference("VO_TB_BotB_Voone_Male_Troll_TB_BotB_Mission10ExchangeB_02.prefab:7e5893a9f9ae36d47aea47de54ea39e8");

	private static readonly AssetReference VO_TB_BotB_Voone_Male_Troll_TB_BotB_Mission10ExchangeC_01 = new AssetReference("VO_TB_BotB_Voone_Male_Troll_TB_BotB_Mission10ExchangeC_01.prefab:3cec3fd63e3dd7e409143b4d745b8b3c");

	private static readonly AssetReference VO_TB_BotB_Voone_Male_Troll_TB_BotB_Mission10ExchangeC_03 = new AssetReference("VO_TB_BotB_Voone_Male_Troll_TB_BotB_Mission10ExchangeC_03.prefab:c8022d3cd99fb6c4d8c6004d22f0878b");

	private static readonly AssetReference VO_TB_BotB_Voone_Male_Troll_TB_BotB_Mission10HeroPower_01 = new AssetReference("VO_TB_BotB_Voone_Male_Troll_TB_BotB_Mission10HeroPower_01.prefab:d6b638b2e23573a489d0b3658773a689");

	private static readonly AssetReference VO_TB_BotB_Voone_Male_Troll_TB_BotB_Mission10HeroPower_02 = new AssetReference("VO_TB_BotB_Voone_Male_Troll_TB_BotB_Mission10HeroPower_02.prefab:b06da345c7afbc24381e073ceceb07bd");

	private static readonly AssetReference VO_TB_BotB_Voone_Male_Troll_TB_BotB_Mission10HeroPower_03 = new AssetReference("VO_TB_BotB_Voone_Male_Troll_TB_BotB_Mission10HeroPower_03.prefab:e2c31b539e24a5e42810b117f1dba936");

	private static readonly AssetReference VO_TB_BotB_Voone_Male_Troll_TB_BotB_Mission10Idle_01 = new AssetReference("VO_TB_BotB_Voone_Male_Troll_TB_BotB_Mission10Idle_01.prefab:7522830ccb3e48542b08b3137c6a8e71");

	private static readonly AssetReference VO_TB_BotB_Voone_Male_Troll_TB_BotB_Mission10Idle_02 = new AssetReference("VO_TB_BotB_Voone_Male_Troll_TB_BotB_Mission10Idle_02.prefab:c217b850369f10844886fb6f41a2370f");

	private static readonly AssetReference VO_TB_BotB_Voone_Male_Troll_TB_BotB_Mission10Idle_03 = new AssetReference("VO_TB_BotB_Voone_Male_Troll_TB_BotB_Mission10Idle_03.prefab:93a6bc7a18f1228479b60f416140db4f");

	private static readonly AssetReference VO_TB_BotB_Voone_Male_Troll_TB_BotB_Mission10Loss_01 = new AssetReference("VO_TB_BotB_Voone_Male_Troll_TB_BotB_Mission10Loss_01.prefab:8da2f4c4aa8e2554ca13a5bd2900b8ca");

	private static readonly AssetReference VO_TB_BotB_Voone_Male_Troll_TB_BotB_Mission10Start_01 = new AssetReference("VO_TB_BotB_Voone_Male_Troll_TB_BotB_Mission10Start_01.prefab:d7fa0d2e776064341bde936583133e94");

	private static readonly AssetReference VO_TB_BotB_Voone_Male_Troll_TB_BotB_Mission10Victory_01 = new AssetReference("VO_TB_BotB_Voone_Male_Troll_TB_BotB_Mission10Victory_01.prefab:6d9c0cdd880a64f4eb7c19a47cbde2fd");

	private static readonly AssetReference VO_TB_BotB_Voone_Male_Troll_TB_BotB_Mission10Victory_02 = new AssetReference("VO_TB_BotB_Voone_Male_Troll_TB_BotB_Mission10Victory_02.prefab:4ba3108b707be3a4695e247cd04261fe");

	private Notification m_popup;

	private Dictionary<int, string[]> m_popUpInfo = new Dictionary<int, string[]> { 
	{
		228,
		new string[1] { "TB_BOTB_VOONE_LOREBOOK1" }
	} };

	private float popUpScale = 1.65f;

	private Vector3 popUpPos;

	private new List<string> m_BossIdleLines = new List<string> { VO_TB_BotB_Voone_Male_Troll_TB_BotB_Mission10Idle_01, VO_TB_BotB_Voone_Male_Troll_TB_BotB_Mission10Idle_02, VO_TB_BotB_Voone_Male_Troll_TB_BotB_Mission10Idle_03 };

	private List<string> m_BossUsesHeroPowerLines = new List<string> { VO_TB_BotB_Voone_Male_Troll_TB_BotB_Mission10HeroPower_01, VO_TB_BotB_Voone_Male_Troll_TB_BotB_Mission10HeroPower_02, VO_TB_BotB_Voone_Male_Troll_TB_BotB_Mission10HeroPower_03 };

	private HashSet<string> m_playedLines = new HashSet<string>();

	private static Map<GameEntityOption, bool> InitBooleanOptions()
	{
		return new Map<GameEntityOption, bool> { 
		{
			GameEntityOption.DO_OPENING_TAUNTS,
			false
		} };
	}

	public TB_BattleoftheBands_Voone()
	{
		m_gameOptions.AddBooleanOptions(s_booleanOptions);
	}

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> m_SoundFiles = new List<string>
		{
			VO_TB_BotB_Voone_Male_Troll_TB_BotB_Mission10Start_01, VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission10Start_02, VO_TB_BotB_Voone_Male_Troll_TB_BotB_Mission10ExchangeA_01, VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission10ExchangeA_02, VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission10ExchangeA_03, VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission10ExchangeB_01, VO_TB_BotB_Voone_Male_Troll_TB_BotB_Mission10ExchangeB_02, VO_TB_BotB_Voone_Male_Troll_TB_BotB_Mission10ExchangeC_01, VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission10ExchangeC_02, VO_TB_BotB_Voone_Male_Troll_TB_BotB_Mission10ExchangeC_03,
			VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission10ExchangeC_04, VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission10ExchangeC_05, VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission10ExchangeC_06, VO_TB_BotB_Voone_Male_Troll_TB_BotB_Mission10Victory_01, VO_TB_BotB_Voone_Male_Troll_TB_BotB_Mission10Victory_02, VO_TB_BotB_Voone_Male_Troll_TB_BotB_Mission10HeroPower_01, VO_TB_BotB_Voone_Male_Troll_TB_BotB_Mission10HeroPower_02, VO_TB_BotB_Voone_Male_Troll_TB_BotB_Mission10HeroPower_03, VO_TB_BotB_Voone_Male_Troll_TB_BotB_Mission10Idle_01, VO_TB_BotB_Voone_Male_Troll_TB_BotB_Mission10Idle_02,
			VO_TB_BotB_Voone_Male_Troll_TB_BotB_Mission10Idle_03, VO_TB_BotB_Voone_Male_Troll_TB_BotB_Mission10Loss_01, VO_TB_BotB_Voone_Male_Troll_TB_BotB_Mission10EmoteResponse_01, VO_TB_BotB_Mission10_Lorebook1_Male_Goblin_TB_BotB_Mission10Lorebook_01
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
			yield return MissionPlayVO(enemyActor, VO_TB_BotB_Voone_Male_Troll_TB_BotB_Mission10EmoteResponse_01);
			break;
		case 514:
			yield return MissionPlayVO(enemyActor, VO_TB_BotB_Voone_Male_Troll_TB_BotB_Mission10Start_01);
			yield return MissionPlayVO(friendlyActor, VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission10Start_02);
			break;
		case 507:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(enemyActor, VO_TB_BotB_Voone_Male_Troll_TB_BotB_Mission10Loss_01);
			GameState.Get().SetBusy(busy: false);
			break;
		case 504:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(enemyActor, VO_TB_BotB_Voone_Male_Troll_TB_BotB_Mission10Victory_01);
			GameState.Get().SetBusy(busy: false);
			break;
		case 101:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(enemyActor, VO_TB_BotB_Voone_Male_Troll_TB_BotB_Mission10Victory_02);
			GameState.Get().SetBusy(busy: false);
			break;
		case 102:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(enemyActor, VO_TB_BotB_Voone_Male_Troll_TB_BotB_Mission10Victory_02);
			GameState.Get().SetBusy(busy: false);
			break;
		case 103:
			yield return MissionPlayVO(enemyActor, VO_TB_BotB_Voone_Male_Troll_TB_BotB_Mission10ExchangeA_01);
			yield return MissionPlayVO(friendlyActor, VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission10ExchangeA_02);
			break;
		case 104:
			yield return MissionPlayVO(enemyActor, VO_TB_BotB_Voone_Male_Troll_TB_BotB_Mission10ExchangeA_01);
			yield return MissionPlayVO(friendlyActor, VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission10ExchangeA_03);
			break;
		case 105:
			yield return MissionPlayVO(enemyActor, VO_TB_BotB_Voone_Male_Troll_TB_BotB_Mission10ExchangeC_01);
			yield return MissionPlayVO(friendlyActor, VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission10ExchangeC_02);
			yield return MissionPlayVO(enemyActor, VO_TB_BotB_Voone_Male_Troll_TB_BotB_Mission10ExchangeC_03);
			yield return MissionPlayVO(friendlyActor, VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission10ExchangeC_04);
			break;
		case 106:
			yield return MissionPlayVO(enemyActor, VO_TB_BotB_Voone_Male_Troll_TB_BotB_Mission10ExchangeC_01);
			yield return MissionPlayVO(friendlyActor, VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission10ExchangeC_02);
			yield return MissionPlayVO(enemyActor, VO_TB_BotB_Voone_Male_Troll_TB_BotB_Mission10ExchangeC_03);
			yield return MissionPlayVO(friendlyActor, VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission10ExchangeC_05);
			break;
		case 107:
			yield return MissionPlayVO(enemyActor, VO_TB_BotB_Voone_Male_Troll_TB_BotB_Mission10ExchangeC_01);
			yield return MissionPlayVO(friendlyActor, VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission10ExchangeC_02);
			yield return MissionPlayVO(enemyActor, VO_TB_BotB_Voone_Male_Troll_TB_BotB_Mission10ExchangeC_03);
			yield return MissionPlayVO(friendlyActor, VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission10ExchangeC_06);
			break;
		case 228:
			if (!(m_popup != null))
			{
				GameState.Get().SetBusy(busy: true);
				m_popup = NotificationManager.Get().CreatePopupText(UserAttentionBlocker.NONE, popUpPos, TutorialEntity.GetTextScale() * popUpScale, GameStrings.Get(m_popUpInfo[missionEvent][0]), convertLegacyPosition: false, NotificationManager.PopupTextType.FANCY);
				yield return GameEntity.Coroutines.StartCoroutine(PlaySoundAndBlockSpeech(VO_TB_BotB_Mission10_Lorebook1_Male_Goblin_TB_BotB_Mission10Lorebook_01, Notification.SpeechBubbleDirection.None, GetEnemyActorByCardId("TB_BotB_Voone_Lorebook1"), 2.5f));
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
		if (turn == 3)
		{
			yield return MissionPlayVO(friendlyActor, VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission10ExchangeB_01);
			yield return MissionPlayVO(enemyActor, VO_TB_BotB_Voone_Male_Troll_TB_BotB_Mission10ExchangeB_02);
		}
	}

	public override bool NotifyOfBattlefieldCardClicked(Entity clickedEntity, bool wasInTargetMode)
	{
		if (!base.NotifyOfBattlefieldCardClicked(clickedEntity, wasInTargetMode))
		{
			return false;
		}
		if (!wasInTargetMode && clickedEntity.GetCardId() == "TB_BotB_Voone_Lorebook1")
		{
			if (m_popup == null)
			{
				Network.Get().SendClientScriptGameEvent(ClientScriptGameEventType.MISSION_EVENT, 228);
				HandleMissionEvent(228);
			}
			return false;
		}
		return true;
	}
}
