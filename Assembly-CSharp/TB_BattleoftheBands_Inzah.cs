using System.Collections;
using System.Collections.Generic;
using Blizzard.T5.Core;
using UnityEngine;

public class TB_BattleoftheBands_Inzah : TB_BattleoftheBands_Dungeon
{
	private static Map<GameEntityOption, bool> s_booleanOptions = InitBooleanOptions();

	private static readonly AssetReference VO_TB_BotB_Inzah_Male_Vulpera_TB_BotB_Mission3EmoteResponse_01 = new AssetReference("VO_TB_BotB_Inzah_Male_Vulpera_TB_BotB_Mission3EmoteResponse_01.prefab:faf213f6877871046858b24f26eb0733");

	private static readonly AssetReference VO_TB_BotB_Inzah_Male_Vulpera_TB_BotB_Mission3ExchangeA_02 = new AssetReference("VO_TB_BotB_Inzah_Male_Vulpera_TB_BotB_Mission3ExchangeA_02.prefab:727f3b69ce47a594c8512af10552327f");

	private static readonly AssetReference VO_TB_BotB_Inzah_Male_Vulpera_TB_BotB_Mission3ExchangeB_02 = new AssetReference("VO_TB_BotB_Inzah_Male_Vulpera_TB_BotB_Mission3ExchangeB_02.prefab:9dd701ff8f726a547ad7b35d600984d5");

	private static readonly AssetReference VO_TB_BotB_Inzah_Male_Vulpera_TB_BotB_Mission3ExchangeC_01 = new AssetReference("VO_TB_BotB_Inzah_Male_Vulpera_TB_BotB_Mission3ExchangeC_01.prefab:4c2208232feabf242a4ed63d6e0532a6");

	private static readonly AssetReference VO_TB_BotB_Inzah_Male_Vulpera_TB_BotB_Mission3ExchangeC_03 = new AssetReference("VO_TB_BotB_Inzah_Male_Vulpera_TB_BotB_Mission3ExchangeC_03.prefab:01a26452f15c3454b968efda5fd66ce1");

	private static readonly AssetReference VO_TB_BotB_Inzah_Male_Vulpera_TB_BotB_Mission3Idle_01 = new AssetReference("VO_TB_BotB_Inzah_Male_Vulpera_TB_BotB_Mission3Idle_01.prefab:528cf4c28eec87a42a4c70a94c2682ed");

	private static readonly AssetReference VO_TB_BotB_Inzah_Male_Vulpera_TB_BotB_Mission3Idle_02 = new AssetReference("VO_TB_BotB_Inzah_Male_Vulpera_TB_BotB_Mission3Idle_02.prefab:4d66f3e450076e3419fa78bd2893eff2");

	private static readonly AssetReference VO_TB_BotB_Inzah_Male_Vulpera_TB_BotB_Mission3Idle_03 = new AssetReference("VO_TB_BotB_Inzah_Male_Vulpera_TB_BotB_Mission3Idle_03.prefab:9f522e7180cd5ee4aaef3cd7ed9038fe");

	private static readonly AssetReference VO_TB_BotB_Inzah_Male_Vulpera_TB_BotB_Mission3Lorebook_01 = new AssetReference("VO_TB_BotB_Inzah_Male_Vulpera_TB_BotB_Mission3Lorebook_01.prefab:9c3904ee51f75ed45b7e6787fe40cdd0");

	private static readonly AssetReference VO_TB_BotB_Inzah_Male_Vulpera_TB_BotB_Mission3Loss_01 = new AssetReference("VO_TB_BotB_Inzah_Male_Vulpera_TB_BotB_Mission3Loss_01.prefab:a78809ad727d40d47ba396696c79df46");

	private static readonly AssetReference VO_TB_BotB_Inzah_Male_Vulpera_TB_BotB_Mission3Start_01 = new AssetReference("VO_TB_BotB_Inzah_Male_Vulpera_TB_BotB_Mission3Start_01.prefab:8bd46323df6689e4fb85bed2d2201e83");

	private static readonly AssetReference VO_TB_BotB_Inzah_Male_Vulpera_TB_BotB_Mission3Victory_01 = new AssetReference("VO_TB_BotB_Inzah_Male_Vulpera_TB_BotB_Mission3Victory_01.prefab:7ee5832af760a6141bd37918d85d58c9");

	private static readonly AssetReference VO_TB_BotB_Inzah_Male_Vulpera_TB_BotB_Mission3Victory_03 = new AssetReference("VO_TB_BotB_Inzah_Male_Vulpera_TB_BotB_Mission3Victory_03.prefab:e44154811049208468a3abfa6c2c0523");

	private static readonly AssetReference VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission3ExchangeA_01 = new AssetReference("VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission3ExchangeA_01.prefab:efce824e8a233fe40b6e5b12f5f8c551");

	private static readonly AssetReference VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission3ExchangeB_01 = new AssetReference("VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission3ExchangeB_01.prefab:3a48bed391eaddc42a9b753db35bc3ad");

	private static readonly AssetReference VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission3ExchangeC_02 = new AssetReference("VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission3ExchangeC_02.prefab:bdf3aa164c3842b4dad4b2bf02b2b298");

	private static readonly AssetReference VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission3Start_02 = new AssetReference("VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission3Start_02.prefab:f1479689f29a2a34f985551c891294ae");

	private static readonly AssetReference VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission3Victory_02 = new AssetReference("VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission3Victory_02.prefab:c34bbb8ba1c611b45a06095b05301095");

	private Notification m_popup;

	private Dictionary<int, string[]> m_popUpInfo = new Dictionary<int, string[]> { 
	{
		228,
		new string[1] { "TB_BOTB_INZAH_LOREBOOK1" }
	} };

	private float popUpScale = 1.65f;

	private Vector3 popUpPos;

	private new List<string> m_BossIdleLines = new List<string> { VO_TB_BotB_Inzah_Male_Vulpera_TB_BotB_Mission3Idle_01, VO_TB_BotB_Inzah_Male_Vulpera_TB_BotB_Mission3Idle_02, VO_TB_BotB_Inzah_Male_Vulpera_TB_BotB_Mission3Idle_03 };

	private HashSet<string> m_playedLines = new HashSet<string>();

	private static Map<GameEntityOption, bool> InitBooleanOptions()
	{
		return new Map<GameEntityOption, bool> { 
		{
			GameEntityOption.DO_OPENING_TAUNTS,
			false
		} };
	}

	public TB_BattleoftheBands_Inzah()
	{
		m_gameOptions.AddBooleanOptions(s_booleanOptions);
	}

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> m_SoundFiles = new List<string>
		{
			VO_TB_BotB_Inzah_Male_Vulpera_TB_BotB_Mission3Start_01, VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission3Start_02, VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission3ExchangeA_01, VO_TB_BotB_Inzah_Male_Vulpera_TB_BotB_Mission3ExchangeA_02, VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission3ExchangeB_01, VO_TB_BotB_Inzah_Male_Vulpera_TB_BotB_Mission3ExchangeB_02, VO_TB_BotB_Inzah_Male_Vulpera_TB_BotB_Mission3ExchangeC_01, VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission3ExchangeC_02, VO_TB_BotB_Inzah_Male_Vulpera_TB_BotB_Mission3ExchangeC_03, VO_TB_BotB_Inzah_Male_Vulpera_TB_BotB_Mission3Victory_01,
			VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission3Victory_02, VO_TB_BotB_Inzah_Male_Vulpera_TB_BotB_Mission3Victory_03, VO_TB_BotB_Inzah_Male_Vulpera_TB_BotB_Mission3Idle_01, VO_TB_BotB_Inzah_Male_Vulpera_TB_BotB_Mission3Idle_02, VO_TB_BotB_Inzah_Male_Vulpera_TB_BotB_Mission3Idle_03, VO_TB_BotB_Inzah_Male_Vulpera_TB_BotB_Mission3Loss_01, VO_TB_BotB_Inzah_Male_Vulpera_TB_BotB_Mission3EmoteResponse_01, VO_TB_BotB_Inzah_Male_Vulpera_TB_BotB_Mission3Lorebook_01
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
			yield return MissionPlayVO(enemyActor, VO_TB_BotB_Inzah_Male_Vulpera_TB_BotB_Mission3EmoteResponse_01);
			break;
		case 514:
			yield return MissionPlayVO(enemyActor, VO_TB_BotB_Inzah_Male_Vulpera_TB_BotB_Mission3Start_01);
			yield return MissionPlayVO(friendlyActor, VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission3Start_02);
			break;
		case 507:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(enemyActor, VO_TB_BotB_Inzah_Male_Vulpera_TB_BotB_Mission3Loss_01);
			GameState.Get().SetBusy(busy: false);
			break;
		case 504:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(enemyActor, VO_TB_BotB_Inzah_Male_Vulpera_TB_BotB_Mission3Victory_01);
			yield return MissionPlayVO(friendlyActor, VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission3Victory_02);
			yield return MissionPlayVO(enemyActor, VO_TB_BotB_Inzah_Male_Vulpera_TB_BotB_Mission3Victory_03);
			GameState.Get().SetBusy(busy: false);
			break;
		case 228:
			if (!(m_popup != null))
			{
				GameState.Get().SetBusy(busy: true);
				m_popup = NotificationManager.Get().CreatePopupText(UserAttentionBlocker.NONE, popUpPos, TutorialEntity.GetTextScale() * popUpScale, GameStrings.Get(m_popUpInfo[missionEvent][0]), convertLegacyPosition: false, NotificationManager.PopupTextType.FANCY);
				yield return GameEntity.Coroutines.StartCoroutine(PlaySoundAndBlockSpeech(VO_TB_BotB_Inzah_Male_Vulpera_TB_BotB_Mission3Lorebook_01, Notification.SpeechBubbleDirection.None, GetEnemyActorByCardId("TB_BotB_Inzah_Lorebook1"), 2.5f));
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
			yield return MissionPlayVO(friendlyActor, VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission3ExchangeA_01);
			yield return MissionPlayVO(enemyActor, VO_TB_BotB_Inzah_Male_Vulpera_TB_BotB_Mission3ExchangeA_02);
			break;
		case 7:
			yield return MissionPlayVO(friendlyActor, VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission3ExchangeB_01);
			yield return MissionPlayVO(enemyActor, VO_TB_BotB_Inzah_Male_Vulpera_TB_BotB_Mission3ExchangeB_02);
			break;
		case 11:
			yield return MissionPlayVO(enemyActor, VO_TB_BotB_Inzah_Male_Vulpera_TB_BotB_Mission3ExchangeC_01);
			yield return MissionPlayVO(friendlyActor, VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission3ExchangeC_02);
			yield return MissionPlayVO(enemyActor, VO_TB_BotB_Inzah_Male_Vulpera_TB_BotB_Mission3ExchangeC_03);
			break;
		}
	}

	public override bool NotifyOfBattlefieldCardClicked(Entity clickedEntity, bool wasInTargetMode)
	{
		if (!base.NotifyOfBattlefieldCardClicked(clickedEntity, wasInTargetMode))
		{
			return false;
		}
		if (!wasInTargetMode && clickedEntity.GetCardId() == "TB_BotB_Inzah_Lorebook1")
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
