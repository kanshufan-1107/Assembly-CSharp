using System.Collections;
using System.Collections.Generic;
using Blizzard.T5.Core;
using UnityEngine;

public class TB_BattleoftheBands_Zok : TB_BattleoftheBands_Dungeon
{
	private static Map<GameEntityOption, bool> s_booleanOptions = InitBooleanOptions();

	private static readonly AssetReference VO_TB_BotB_Mission2_Lorebook1_Male_Quilboar_TB_BotB_Mission2Lorebook_01 = new AssetReference("VO_TB_BotB_Mission2_Lorebook1_Male_Quilboar_TB_BotB_Mission2Lorebook_01.prefab:d5942eac7c5b2814bba07a451860f4ad");

	private static readonly AssetReference VO_TB_BotB_Mission5_Lorebook1_Male_Goblin_TB_BotB_Mission5Lorebook_01 = new AssetReference("VO_TB_BotB_Mission5_Lorebook1_Male_Goblin_TB_BotB_Mission5Lorebook_01.prefab:93c97ac54db68a7488e2206fdc3fbfd3");

	private static readonly AssetReference VO_TB_BotB_Mission5_Lorebook1_Male_Goblin_TB_BotB_Mission5Lorebook_02 = new AssetReference("VO_TB_BotB_Mission5_Lorebook1_Male_Goblin_TB_BotB_Mission5Lorebook_02.prefab:f7b58d0329b413d488423bc9d013d780");

	private static readonly AssetReference VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission2ExchangeA_02 = new AssetReference("VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission2ExchangeA_02.prefab:7e175bb7482cbbb45aa52e328c1fe353");

	private static readonly AssetReference VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission2ExchangeB_02 = new AssetReference("VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission2ExchangeB_02.prefab:e228d99a634432b498a17538ef42a24d");

	private static readonly AssetReference VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission2Start_01 = new AssetReference("VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission2Start_01.prefab:d380a0db3567e1f4a9b289d7356188d6");

	private static readonly AssetReference VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission2Victory_02 = new AssetReference("VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission2Victory_02.prefab:b211dcd1325d50f4ab5ac0f4bb92d1a9");

	private static readonly AssetReference VO_TB_BotB_Zok_Male_Quilboar_TB_BotB_Mission2EmoteResponse_01 = new AssetReference("VO_TB_BotB_Zok_Male_Quilboar_TB_BotB_Mission2EmoteResponse_01.prefab:f27f6a5f5ba7821408625588c05121be");

	private static readonly AssetReference VO_TB_BotB_Zok_Male_Quilboar_TB_BotB_Mission2ExchangeA_01 = new AssetReference("VO_TB_BotB_Zok_Male_Quilboar_TB_BotB_Mission2ExchangeA_01.prefab:0f7b523a1e15f2e49918a3b41e4784da");

	private static readonly AssetReference VO_TB_BotB_Zok_Male_Quilboar_TB_BotB_Mission2ExchangeB_01 = new AssetReference("VO_TB_BotB_Zok_Male_Quilboar_TB_BotB_Mission2ExchangeB_01.prefab:86e1a47ebfc9e29468aaa5b8ac37e6c9");

	private static readonly AssetReference VO_TB_BotB_Zok_Male_Quilboar_TB_BotB_Mission2HeroPower_01 = new AssetReference("VO_TB_BotB_Zok_Male_Quilboar_TB_BotB_Mission2HeroPower_01.prefab:31cb26684f3de1d43843189de8bf67d2");

	private static readonly AssetReference VO_TB_BotB_Zok_Male_Quilboar_TB_BotB_Mission2HeroPower_02 = new AssetReference("VO_TB_BotB_Zok_Male_Quilboar_TB_BotB_Mission2HeroPower_02.prefab:02703e9d15639904482529c849d8977d");

	private static readonly AssetReference VO_TB_BotB_Zok_Male_Quilboar_TB_BotB_Mission2HeroPower_03 = new AssetReference("VO_TB_BotB_Zok_Male_Quilboar_TB_BotB_Mission2HeroPower_03.prefab:b42fcdfa5f1456748bb6ecad24bf1448");

	private static readonly AssetReference VO_TB_BotB_Zok_Male_Quilboar_TB_BotB_Mission2Idle_01 = new AssetReference("VO_TB_BotB_Zok_Male_Quilboar_TB_BotB_Mission2Idle_01.prefab:e75e71dd18e748f4296718c37ccfdba4");

	private static readonly AssetReference VO_TB_BotB_Zok_Male_Quilboar_TB_BotB_Mission2Idle_02 = new AssetReference("VO_TB_BotB_Zok_Male_Quilboar_TB_BotB_Mission2Idle_02.prefab:581484cb1f478584eacb6bc4c48a0612");

	private static readonly AssetReference VO_TB_BotB_Zok_Male_Quilboar_TB_BotB_Mission2Idle_03 = new AssetReference("VO_TB_BotB_Zok_Male_Quilboar_TB_BotB_Mission2Idle_03.prefab:ac9c9c0d7f9f3054fb7902102eca5a3b");

	private static readonly AssetReference VO_TB_BotB_Zok_Male_Quilboar_TB_BotB_Mission2Loss_01 = new AssetReference("VO_TB_BotB_Zok_Male_Quilboar_TB_BotB_Mission2Loss_01.prefab:1f117d90fb0e14f40896be7a0beedb89");

	private static readonly AssetReference VO_TB_BotB_Zok_Male_Quilboar_TB_BotB_Mission2Loss_02 = new AssetReference("VO_TB_BotB_Zok_Male_Quilboar_TB_BotB_Mission2Loss_02.prefab:235f0f0bf8ff13c44abfe11f6ac168e3");

	private static readonly AssetReference VO_TB_BotB_Zok_Male_Quilboar_TB_BotB_Mission2Start_02 = new AssetReference("VO_TB_BotB_Zok_Male_Quilboar_TB_BotB_Mission2Start_02.prefab:717690d3a6b036d419ac17ea14e8000d");

	private static readonly AssetReference VO_TB_BotB_Zok_Male_Quilboar_TB_BotB_Mission2Victory_01 = new AssetReference("VO_TB_BotB_Zok_Male_Quilboar_TB_BotB_Mission2Victory_01.prefab:f7d171094b6bf0a449856673c2df4504");

	private static readonly AssetReference VO_TB_BotB_Zok_Male_Quilboar_TB_BotB_PreMission2_02 = new AssetReference("VO_TB_BotB_Zok_Male_Quilboar_TB_BotB_PreMission2_02.prefab:f8559f5fe80c232498e922733103a2a7");

	private Notification m_popup;

	private Dictionary<int, string[]> m_popUpInfo = new Dictionary<int, string[]> { 
	{
		228,
		new string[1] { "TB_BOTB_ZOK_LOREBOOK1" }
	} };

	private float popUpScale = 1.65f;

	private Vector3 popUpPos;

	private new List<string> m_BossIdleLines = new List<string> { VO_TB_BotB_Zok_Male_Quilboar_TB_BotB_Mission2Idle_01, VO_TB_BotB_Zok_Male_Quilboar_TB_BotB_Mission2Idle_02, VO_TB_BotB_Zok_Male_Quilboar_TB_BotB_Mission2Idle_03 };

	private List<string> m_BossUsesHeroPowerLines = new List<string> { VO_TB_BotB_Zok_Male_Quilboar_TB_BotB_Mission2HeroPower_01, VO_TB_BotB_Zok_Male_Quilboar_TB_BotB_Mission2HeroPower_02, VO_TB_BotB_Zok_Male_Quilboar_TB_BotB_Mission2HeroPower_03 };

	private HashSet<string> m_playedLines = new HashSet<string>();

	private static Map<GameEntityOption, bool> InitBooleanOptions()
	{
		return new Map<GameEntityOption, bool> { 
		{
			GameEntityOption.DO_OPENING_TAUNTS,
			false
		} };
	}

	public TB_BattleoftheBands_Zok()
	{
		m_gameOptions.AddBooleanOptions(s_booleanOptions);
	}

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> m_SoundFiles = new List<string>
		{
			VO_TB_BotB_Zok_Male_Quilboar_TB_BotB_PreMission2_02, VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission2Start_01, VO_TB_BotB_Zok_Male_Quilboar_TB_BotB_Mission2Start_02, VO_TB_BotB_Zok_Male_Quilboar_TB_BotB_Mission2ExchangeA_01, VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission2ExchangeA_02, VO_TB_BotB_Zok_Male_Quilboar_TB_BotB_Mission2ExchangeB_01, VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission2ExchangeB_02, VO_TB_BotB_Zok_Male_Quilboar_TB_BotB_Mission2Victory_01, VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission2Victory_02, VO_TB_BotB_Zok_Male_Quilboar_TB_BotB_Mission2HeroPower_01,
			VO_TB_BotB_Zok_Male_Quilboar_TB_BotB_Mission2HeroPower_02, VO_TB_BotB_Zok_Male_Quilboar_TB_BotB_Mission2HeroPower_03, VO_TB_BotB_Zok_Male_Quilboar_TB_BotB_Mission2Idle_01, VO_TB_BotB_Zok_Male_Quilboar_TB_BotB_Mission2Idle_02, VO_TB_BotB_Zok_Male_Quilboar_TB_BotB_Mission2Idle_03, VO_TB_BotB_Zok_Male_Quilboar_TB_BotB_Mission2Loss_01, VO_TB_BotB_Zok_Male_Quilboar_TB_BotB_Mission2EmoteResponse_01, VO_TB_BotB_Mission2_Lorebook1_Male_Quilboar_TB_BotB_Mission2Lorebook_01
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
			yield return MissionPlayVO(enemyActor, VO_TB_BotB_Zok_Male_Quilboar_TB_BotB_Mission2EmoteResponse_01);
			break;
		case 514:
			yield return MissionPlayVO(enemyActor, VO_TB_BotB_Zok_Male_Quilboar_TB_BotB_PreMission2_02);
			yield return MissionPlayVO(friendlyActor, VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission2Start_01);
			yield return MissionPlayVO(enemyActor, VO_TB_BotB_Zok_Male_Quilboar_TB_BotB_Mission2Start_02);
			break;
		case 507:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(enemyActor, VO_TB_BotB_Zok_Male_Quilboar_TB_BotB_Mission2Loss_01);
			GameState.Get().SetBusy(busy: false);
			break;
		case 504:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(enemyActor, VO_TB_BotB_Zok_Male_Quilboar_TB_BotB_Mission2Victory_01);
			yield return MissionPlayVO(friendlyActor, VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission2Victory_02);
			GameState.Get().SetBusy(busy: false);
			break;
		case 228:
			if (!(m_popup != null))
			{
				GameState.Get().SetBusy(busy: true);
				m_popup = NotificationManager.Get().CreatePopupText(UserAttentionBlocker.NONE, popUpPos, TutorialEntity.GetTextScale() * popUpScale, GameStrings.Get(m_popUpInfo[missionEvent][0]), convertLegacyPosition: false, NotificationManager.PopupTextType.FANCY);
				yield return GameEntity.Coroutines.StartCoroutine(PlaySoundAndBlockSpeech(VO_TB_BotB_Mission2_Lorebook1_Male_Quilboar_TB_BotB_Mission2Lorebook_01, Notification.SpeechBubbleDirection.None, GetEnemyActorByCardId("TB_BotB_Zok_Lorebook1"), 2.5f));
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
			yield return MissionPlayVO(enemyActor, VO_TB_BotB_Zok_Male_Quilboar_TB_BotB_Mission2ExchangeA_01);
			yield return MissionPlayVO(friendlyActor, VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission2ExchangeA_02);
			break;
		case 7:
			yield return MissionPlayVO(enemyActor, VO_TB_BotB_Zok_Male_Quilboar_TB_BotB_Mission2ExchangeB_01);
			yield return MissionPlayVO(friendlyActor, VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission2ExchangeB_02);
			break;
		}
	}

	public override bool NotifyOfBattlefieldCardClicked(Entity clickedEntity, bool wasInTargetMode)
	{
		if (!base.NotifyOfBattlefieldCardClicked(clickedEntity, wasInTargetMode))
		{
			return false;
		}
		if (!wasInTargetMode && clickedEntity.GetCardId() == "TB_BotB_Zok_Lorebook1")
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
