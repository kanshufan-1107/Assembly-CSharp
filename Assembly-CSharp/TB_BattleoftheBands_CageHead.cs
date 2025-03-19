using System.Collections;
using System.Collections.Generic;
using Blizzard.T5.Core;
using UnityEngine;

public class TB_BattleoftheBands_CageHead : TB_BattleoftheBands_Dungeon
{
	private static Map<GameEntityOption, bool> s_booleanOptions = InitBooleanOptions();

	private static readonly AssetReference VO_TB_BotB_CageHead_Male_Undead_TB_BotB_Mission9EmoteResponse_01 = new AssetReference("VO_TB_BotB_CageHead_Male_Undead_TB_BotB_Mission9EmoteResponse_01.prefab:b26a40270b0de344a9191f84a22dd38d");

	private static readonly AssetReference VO_TB_BotB_CageHead_Male_Undead_TB_BotB_Mission9ExchangeA_02 = new AssetReference("VO_TB_BotB_CageHead_Male_Undead_TB_BotB_Mission9ExchangeA_02.prefab:66f013535019cb84c886133054558546");

	private static readonly AssetReference VO_TB_BotB_CageHead_Male_Undead_TB_BotB_Mission9ExchangeB_01 = new AssetReference("VO_TB_BotB_CageHead_Male_Undead_TB_BotB_Mission9ExchangeB_01.prefab:65e6c279802e3964eb3fd89e2b5a5ded");

	private static readonly AssetReference VO_TB_BotB_CageHead_Male_Undead_TB_BotB_Mission9ExchangeB_03 = new AssetReference("VO_TB_BotB_CageHead_Male_Undead_TB_BotB_Mission9ExchangeB_03.prefab:bc311cf5beb2dff4f9e8dfe469dbc81c");

	private static readonly AssetReference VO_TB_BotB_CageHead_Male_Undead_TB_BotB_Mission9ExchangeC_02 = new AssetReference("VO_TB_BotB_CageHead_Male_Undead_TB_BotB_Mission9ExchangeC_02.prefab:559394cfed896304b8c281a5c628b269");

	private static readonly AssetReference VO_TB_BotB_CageHead_Male_Undead_TB_BotB_Mission9HeroPower_01 = new AssetReference("VO_TB_BotB_CageHead_Male_Undead_TB_BotB_Mission9HeroPower_01.prefab:8d368a30e120d2149b6ba72028a8df3f");

	private static readonly AssetReference VO_TB_BotB_CageHead_Male_Undead_TB_BotB_Mission9HeroPower_02 = new AssetReference("VO_TB_BotB_CageHead_Male_Undead_TB_BotB_Mission9HeroPower_02.prefab:4b542ecd000327e44b50ec4d2e8e9ee0");

	private static readonly AssetReference VO_TB_BotB_CageHead_Male_Undead_TB_BotB_Mission9HeroPower_03 = new AssetReference("VO_TB_BotB_CageHead_Male_Undead_TB_BotB_Mission9HeroPower_03.prefab:ce4435e8b8f13f142a80e916ebdb3767");

	private static readonly AssetReference VO_TB_BotB_CageHead_Male_Undead_TB_BotB_Mission9Idle_01 = new AssetReference("VO_TB_BotB_CageHead_Male_Undead_TB_BotB_Mission9Idle_01.prefab:de23dacf8e8ef514c9491f8a7e862b09");

	private static readonly AssetReference VO_TB_BotB_CageHead_Male_Undead_TB_BotB_Mission9Idle_02 = new AssetReference("VO_TB_BotB_CageHead_Male_Undead_TB_BotB_Mission9Idle_02.prefab:5172457a6355adb46be586060b9468d2");

	private static readonly AssetReference VO_TB_BotB_CageHead_Male_Undead_TB_BotB_Mission9Idle_03 = new AssetReference("VO_TB_BotB_CageHead_Male_Undead_TB_BotB_Mission9Idle_03.prefab:bcd293fb182ba57499d7e4c53df3397b");

	private static readonly AssetReference VO_TB_BotB_CageHead_Male_Undead_TB_BotB_Mission9Loss_01 = new AssetReference("VO_TB_BotB_CageHead_Male_Undead_TB_BotB_Mission9Loss_01.prefab:26a032cfc2f778941b9104ada90acc0d");

	private static readonly AssetReference VO_TB_BotB_CageHead_Male_Undead_TB_BotB_Mission9Start_01 = new AssetReference("VO_TB_BotB_CageHead_Male_Undead_TB_BotB_Mission9Start_01.prefab:8af14151bd2d6e04fbb20cf650a4b0c0");

	private static readonly AssetReference VO_TB_BotB_CageHead_Male_Undead_TB_BotB_Mission9Victory_01 = new AssetReference("VO_TB_BotB_CageHead_Male_Undead_TB_BotB_Mission9Victory_01.prefab:82b09cbe1eab38140a33dacb9b230ac8");

	private static readonly AssetReference VO_TB_BotB_CageHead_Male_Undead_TB_BotB_Mission9Victory_03 = new AssetReference("VO_TB_BotB_CageHead_Male_Undead_TB_BotB_Mission9Victory_03.prefab:2e550ed5708582042b03b1735d4e1cb3");

	private static readonly AssetReference VO_TB_BotB_Mission9_Lorebook1_Male_Undead_TB_BotB_Mission9Lorebook_01 = new AssetReference("VO_TB_BotB_Mission9_Lorebook1_Male_Undead_TB_BotB_Mission9Lorebook_01.prefab:80ff60dbf5bbcdc488e8a4e5762f7a5c");

	private static readonly AssetReference VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission9ExchangeA_01 = new AssetReference("VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission9ExchangeA_01.prefab:593e46638c95bfa45958bb7f36dc84f1");

	private static readonly AssetReference VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission9ExchangeB_02 = new AssetReference("VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission9ExchangeB_02.prefab:3a9dfc853ec48344b8ba7f09fc3664fe");

	private static readonly AssetReference VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission9ExchangeC_01 = new AssetReference("VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission9ExchangeC_01.prefab:1e39b0f8603bfcc4fa7325190c1f5eb8");

	private static readonly AssetReference VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission9ExchangeC_03 = new AssetReference("VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission9ExchangeC_03.prefab:bc8675813df28c14a8fcf0d246cf5c02");

	private static readonly AssetReference VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission9Start_02 = new AssetReference("VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission9Start_02.prefab:54ed387164906de4d9f68dc2aba3fce2");

	private static readonly AssetReference VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission9Victory_02 = new AssetReference("VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission9Victory_02.prefab:6f84a3e08cd59ea44b001074f9befa65");

	private Notification m_popup;

	private Dictionary<int, string[]> m_popUpInfo = new Dictionary<int, string[]> { 
	{
		228,
		new string[1] { "TB_BOTB_CAGEHEAD_LOREBOOK1" }
	} };

	private float popUpScale = 1.65f;

	private Vector3 popUpPos;

	private new List<string> m_BossIdleLines = new List<string> { VO_TB_BotB_CageHead_Male_Undead_TB_BotB_Mission9Idle_01, VO_TB_BotB_CageHead_Male_Undead_TB_BotB_Mission9Idle_02, VO_TB_BotB_CageHead_Male_Undead_TB_BotB_Mission9Idle_03 };

	private List<string> m_BossUsesHeroPowerLines = new List<string> { VO_TB_BotB_CageHead_Male_Undead_TB_BotB_Mission9HeroPower_01, VO_TB_BotB_CageHead_Male_Undead_TB_BotB_Mission9HeroPower_02, VO_TB_BotB_CageHead_Male_Undead_TB_BotB_Mission9HeroPower_03 };

	private HashSet<string> m_playedLines = new HashSet<string>();

	private static Map<GameEntityOption, bool> InitBooleanOptions()
	{
		return new Map<GameEntityOption, bool> { 
		{
			GameEntityOption.DO_OPENING_TAUNTS,
			false
		} };
	}

	public TB_BattleoftheBands_CageHead()
	{
		m_gameOptions.AddBooleanOptions(s_booleanOptions);
	}

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> m_SoundFiles = new List<string>
		{
			VO_TB_BotB_CageHead_Male_Undead_TB_BotB_Mission9Start_01, VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission9Start_02, VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission9ExchangeA_01, VO_TB_BotB_CageHead_Male_Undead_TB_BotB_Mission9ExchangeA_02, VO_TB_BotB_CageHead_Male_Undead_TB_BotB_Mission9ExchangeB_01, VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission9ExchangeB_02, VO_TB_BotB_CageHead_Male_Undead_TB_BotB_Mission9ExchangeB_03, VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission9ExchangeC_01, VO_TB_BotB_CageHead_Male_Undead_TB_BotB_Mission9ExchangeC_02, VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission9ExchangeC_03,
			VO_TB_BotB_CageHead_Male_Undead_TB_BotB_Mission9Victory_01, VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission9Victory_02, VO_TB_BotB_CageHead_Male_Undead_TB_BotB_Mission9Victory_03, VO_TB_BotB_CageHead_Male_Undead_TB_BotB_Mission9HeroPower_01, VO_TB_BotB_CageHead_Male_Undead_TB_BotB_Mission9HeroPower_02, VO_TB_BotB_CageHead_Male_Undead_TB_BotB_Mission9HeroPower_03, VO_TB_BotB_CageHead_Male_Undead_TB_BotB_Mission9Idle_01, VO_TB_BotB_CageHead_Male_Undead_TB_BotB_Mission9Idle_02, VO_TB_BotB_CageHead_Male_Undead_TB_BotB_Mission9Idle_03, VO_TB_BotB_CageHead_Male_Undead_TB_BotB_Mission9Loss_01,
			VO_TB_BotB_CageHead_Male_Undead_TB_BotB_Mission9EmoteResponse_01, VO_TB_BotB_Mission9_Lorebook1_Male_Undead_TB_BotB_Mission9Lorebook_01
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
			yield return MissionPlayVO(enemyActor, VO_TB_BotB_CageHead_Male_Undead_TB_BotB_Mission9EmoteResponse_01);
			break;
		case 514:
			yield return MissionPlayVO(enemyActor, VO_TB_BotB_CageHead_Male_Undead_TB_BotB_Mission9Start_01);
			yield return MissionPlayVO(friendlyActor, VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission9Start_02);
			break;
		case 507:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(enemyActor, VO_TB_BotB_CageHead_Male_Undead_TB_BotB_Mission9Loss_01);
			GameState.Get().SetBusy(busy: false);
			break;
		case 504:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(enemyActor, VO_TB_BotB_CageHead_Male_Undead_TB_BotB_Mission9Victory_01);
			yield return MissionPlayVO(friendlyActor, VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission9Victory_02);
			yield return MissionPlayVO(enemyActor, VO_TB_BotB_CageHead_Male_Undead_TB_BotB_Mission9Victory_03);
			GameState.Get().SetBusy(busy: false);
			break;
		case 228:
			if (!(m_popup != null))
			{
				GameState.Get().SetBusy(busy: true);
				m_popup = NotificationManager.Get().CreatePopupText(UserAttentionBlocker.NONE, popUpPos, TutorialEntity.GetTextScale() * popUpScale, GameStrings.Get(m_popUpInfo[missionEvent][0]), convertLegacyPosition: false, NotificationManager.PopupTextType.FANCY);
				yield return GameEntity.Coroutines.StartCoroutine(PlaySoundAndBlockSpeech(VO_TB_BotB_Mission9_Lorebook1_Male_Undead_TB_BotB_Mission9Lorebook_01, Notification.SpeechBubbleDirection.None, GetEnemyActorByCardId("Story_11_Mission1_Lorebook1"), 2.5f));
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
			yield return MissionPlayVO(friendlyActor, VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission9ExchangeA_01);
			yield return MissionPlayVO(enemyActor, VO_TB_BotB_CageHead_Male_Undead_TB_BotB_Mission9ExchangeA_02);
			break;
		case 7:
			yield return MissionPlayVO(enemyActor, VO_TB_BotB_CageHead_Male_Undead_TB_BotB_Mission9ExchangeB_01);
			yield return MissionPlayVO(friendlyActor, VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission9ExchangeB_02);
			yield return MissionPlayVO(enemyActor, VO_TB_BotB_CageHead_Male_Undead_TB_BotB_Mission9ExchangeB_03);
			break;
		case 11:
			yield return MissionPlayVO(friendlyActor, VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission9ExchangeC_01);
			yield return MissionPlayVO(enemyActor, VO_TB_BotB_CageHead_Male_Undead_TB_BotB_Mission9ExchangeC_02);
			yield return MissionPlayVO(friendlyActor, VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission9ExchangeC_03);
			break;
		}
	}

	public override bool NotifyOfBattlefieldCardClicked(Entity clickedEntity, bool wasInTargetMode)
	{
		if (!base.NotifyOfBattlefieldCardClicked(clickedEntity, wasInTargetMode))
		{
			return false;
		}
		if (!wasInTargetMode && clickedEntity.GetCardId() == "TB_BotB_CageHead_Lorebook1")
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
