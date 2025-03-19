using System.Collections;
using System.Collections.Generic;
using Blizzard.T5.Core;
using UnityEngine;

public class TB_BattleoftheBands_Kangor : TB_BattleoftheBands_Dungeon
{
	private static Map<GameEntityOption, bool> s_booleanOptions = InitBooleanOptions();

	private static readonly AssetReference VO_TB_BotB_ETC_Male_Tauren_TB_BotB_Mission7Lorebook_01 = new AssetReference("VO_TB_BotB_ETC_Male_Tauren_TB_BotB_Mission7Lorebook_01.prefab:96afef03ca7668c479faf2ccf7916624");

	private static readonly AssetReference VO_TB_BotB_Kangor_Male_CrystalGolem_TB_BotB_Mission7EmoteResponse_01 = new AssetReference("VO_TB_BotB_Kangor_Male_CrystalGolem_TB_BotB_Mission7EmoteResponse_01.prefab:9bd6fc93f9600004885ef7e57331d039");

	private static readonly AssetReference VO_TB_BotB_Kangor_Male_CrystalGolem_TB_BotB_Mission7ExchangeA_02 = new AssetReference("VO_TB_BotB_Kangor_Male_CrystalGolem_TB_BotB_Mission7ExchangeA_02.prefab:a99d8d92ff743e548a70e387ea73c2fe");

	private static readonly AssetReference VO_TB_BotB_Kangor_Male_CrystalGolem_TB_BotB_Mission7ExchangeB_01 = new AssetReference("VO_TB_BotB_Kangor_Male_CrystalGolem_TB_BotB_Mission7ExchangeB_01.prefab:b16e5f01ee9d4cb4b9b6c5db8ac490c4");

	private static readonly AssetReference VO_TB_BotB_Kangor_Male_CrystalGolem_TB_BotB_Mission7HeroPower_01 = new AssetReference("VO_TB_BotB_Kangor_Male_CrystalGolem_TB_BotB_Mission7HeroPower_01.prefab:10b4df199609d0f4bb5b99cdec335be4");

	private static readonly AssetReference VO_TB_BotB_Kangor_Male_CrystalGolem_TB_BotB_Mission7HeroPower_02 = new AssetReference("VO_TB_BotB_Kangor_Male_CrystalGolem_TB_BotB_Mission7HeroPower_02.prefab:8be157bd97878cf49850364bbed95de3");

	private static readonly AssetReference VO_TB_BotB_Kangor_Male_CrystalGolem_TB_BotB_Mission7HeroPower_03 = new AssetReference("VO_TB_BotB_Kangor_Male_CrystalGolem_TB_BotB_Mission7HeroPower_03.prefab:40ff2cfcc42676847864da66880557fc");

	private static readonly AssetReference VO_TB_BotB_Kangor_Male_CrystalGolem_TB_BotB_Mission7Idle_01 = new AssetReference("VO_TB_BotB_Kangor_Male_CrystalGolem_TB_BotB_Mission7Idle_01.prefab:ad4206214351c894e9a40ce40ac077fe");

	private static readonly AssetReference VO_TB_BotB_Kangor_Male_CrystalGolem_TB_BotB_Mission7Idle_02 = new AssetReference("VO_TB_BotB_Kangor_Male_CrystalGolem_TB_BotB_Mission7Idle_02.prefab:a6f4cb45f16aaf44983560baa6aef226");

	private static readonly AssetReference VO_TB_BotB_Kangor_Male_CrystalGolem_TB_BotB_Mission7Idle_03 = new AssetReference("VO_TB_BotB_Kangor_Male_CrystalGolem_TB_BotB_Mission7Idle_03.prefab:6d0c5a556f88bab4989f18e3e1d58f5c");

	private static readonly AssetReference VO_TB_BotB_Kangor_Male_CrystalGolem_TB_BotB_Mission7Loss_01 = new AssetReference("VO_TB_BotB_Kangor_Male_CrystalGolem_TB_BotB_Mission7Loss_01.prefab:e6ed6dc9e0b25f64f971b751fa15cc80");

	private static readonly AssetReference VO_TB_BotB_Kangor_Male_CrystalGolem_TB_BotB_Mission7Start_01 = new AssetReference("VO_TB_BotB_Kangor_Male_CrystalGolem_TB_BotB_Mission7Start_01.prefab:ac98a0b51a1038e44a4ebfaf3d89d0f3");

	private static readonly AssetReference VO_TB_BotB_Kangor_Male_CrystalGolem_TB_BotB_Mission7Start_03 = new AssetReference("VO_TB_BotB_Kangor_Male_CrystalGolem_TB_BotB_Mission7Start_03.prefab:1ff5d9e8c9aa0d24da9fb841dc15bc95");

	private static readonly AssetReference VO_TB_BotB_Kangor_Male_CrystalGolem_TB_BotB_Mission7Victory_01 = new AssetReference("VO_TB_BotB_Kangor_Male_CrystalGolem_TB_BotB_Mission7Victory_01.prefab:8d7c014dbe7d03a43bfdcd89feaba892");

	private static readonly AssetReference VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission7ExchangeA_01 = new AssetReference("VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission7ExchangeA_01.prefab:e4fe26b8c6a76af439cd46a6dfe45d5f");

	private static readonly AssetReference VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission7ExchangeB_02 = new AssetReference("VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission7ExchangeB_02.prefab:1e442215a836b6845a47914d2d32030e");

	private static readonly AssetReference VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission7Start_02 = new AssetReference("VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission7Start_02.prefab:9efe5b85aac8a46488f2f927fe2cb4d8");

	private static readonly AssetReference VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission7Victory_02 = new AssetReference("VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission7Victory_02.prefab:9c7ff3ac0ebf93944993ae9e8344b6b0");

	private Notification m_popup;

	private Dictionary<int, string[]> m_popUpInfo = new Dictionary<int, string[]> { 
	{
		228,
		new string[1] { "TB_BOTB_KANGOR_LOREBOOK1" }
	} };

	private float popUpScale = 1.65f;

	private Vector3 popUpPos;

	private new List<string> m_BossIdleLines = new List<string> { VO_TB_BotB_Kangor_Male_CrystalGolem_TB_BotB_Mission7Idle_01, VO_TB_BotB_Kangor_Male_CrystalGolem_TB_BotB_Mission7Idle_02, VO_TB_BotB_Kangor_Male_CrystalGolem_TB_BotB_Mission7Idle_03 };

	private List<string> m_BossUsesHeroPowerLines = new List<string> { VO_TB_BotB_Kangor_Male_CrystalGolem_TB_BotB_Mission7HeroPower_01, VO_TB_BotB_Kangor_Male_CrystalGolem_TB_BotB_Mission7HeroPower_02, VO_TB_BotB_Kangor_Male_CrystalGolem_TB_BotB_Mission7HeroPower_03 };

	private HashSet<string> m_playedLines = new HashSet<string>();

	private static Map<GameEntityOption, bool> InitBooleanOptions()
	{
		return new Map<GameEntityOption, bool> { 
		{
			GameEntityOption.DO_OPENING_TAUNTS,
			false
		} };
	}

	public TB_BattleoftheBands_Kangor()
	{
		m_gameOptions.AddBooleanOptions(s_booleanOptions);
	}

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> m_SoundFiles = new List<string>
		{
			VO_TB_BotB_Kangor_Male_CrystalGolem_TB_BotB_Mission7Start_01, VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission7Start_02, VO_TB_BotB_Kangor_Male_CrystalGolem_TB_BotB_Mission7Start_03, VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission7ExchangeA_01, VO_TB_BotB_Kangor_Male_CrystalGolem_TB_BotB_Mission7ExchangeA_02, VO_TB_BotB_Kangor_Male_CrystalGolem_TB_BotB_Mission7ExchangeB_01, VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission7ExchangeB_02, VO_TB_BotB_Kangor_Male_CrystalGolem_TB_BotB_Mission7Victory_01, VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission7Victory_02, VO_TB_BotB_Kangor_Male_CrystalGolem_TB_BotB_Mission7HeroPower_01,
			VO_TB_BotB_Kangor_Male_CrystalGolem_TB_BotB_Mission7HeroPower_02, VO_TB_BotB_Kangor_Male_CrystalGolem_TB_BotB_Mission7HeroPower_03, VO_TB_BotB_Kangor_Male_CrystalGolem_TB_BotB_Mission7Idle_01, VO_TB_BotB_Kangor_Male_CrystalGolem_TB_BotB_Mission7Idle_02, VO_TB_BotB_Kangor_Male_CrystalGolem_TB_BotB_Mission7Idle_03, VO_TB_BotB_Kangor_Male_CrystalGolem_TB_BotB_Mission7Loss_01, VO_TB_BotB_Kangor_Male_CrystalGolem_TB_BotB_Mission7EmoteResponse_01, VO_TB_BotB_ETC_Male_Tauren_TB_BotB_Mission7Lorebook_01
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
			yield return MissionPlayVO(enemyActor, VO_TB_BotB_Kangor_Male_CrystalGolem_TB_BotB_Mission7EmoteResponse_01);
			break;
		case 514:
			yield return MissionPlayVO(enemyActor, VO_TB_BotB_Kangor_Male_CrystalGolem_TB_BotB_Mission7Start_01);
			yield return MissionPlayVO(friendlyActor, VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission7Start_02);
			yield return MissionPlayVO(enemyActor, VO_TB_BotB_Kangor_Male_CrystalGolem_TB_BotB_Mission7Start_03);
			break;
		case 507:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(enemyActor, VO_TB_BotB_Kangor_Male_CrystalGolem_TB_BotB_Mission7Loss_01);
			GameState.Get().SetBusy(busy: false);
			break;
		case 504:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(enemyActor, VO_TB_BotB_Kangor_Male_CrystalGolem_TB_BotB_Mission7Victory_01);
			yield return MissionPlayVO(friendlyActor, VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission7Victory_02);
			GameState.Get().SetBusy(busy: false);
			break;
		case 228:
			if (!(m_popup != null))
			{
				GameState.Get().SetBusy(busy: true);
				m_popup = NotificationManager.Get().CreatePopupText(UserAttentionBlocker.NONE, popUpPos, TutorialEntity.GetTextScale() * popUpScale, GameStrings.Get(m_popUpInfo[missionEvent][0]), convertLegacyPosition: false, NotificationManager.PopupTextType.FANCY);
				yield return GameEntity.Coroutines.StartCoroutine(PlaySoundAndBlockSpeech(VO_TB_BotB_ETC_Male_Tauren_TB_BotB_Mission7Lorebook_01, Notification.SpeechBubbleDirection.None, GetEnemyActorByCardId("TB_BotB_Kangor_Lorebook1"), 2.5f));
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
		case 2:
			yield return MissionPlayVO(friendlyActor, VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission7ExchangeA_01);
			yield return MissionPlayVO(enemyActor, VO_TB_BotB_Kangor_Male_CrystalGolem_TB_BotB_Mission7ExchangeA_02);
			break;
		case 9:
			yield return MissionPlayVO(enemyActor, VO_TB_BotB_Kangor_Male_CrystalGolem_TB_BotB_Mission7ExchangeB_01);
			yield return MissionPlayVO(friendlyActor, VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission7ExchangeB_02);
			break;
		}
	}

	public override bool NotifyOfBattlefieldCardClicked(Entity clickedEntity, bool wasInTargetMode)
	{
		if (!base.NotifyOfBattlefieldCardClicked(clickedEntity, wasInTargetMode))
		{
			return false;
		}
		if (!wasInTargetMode && clickedEntity.GetCardId() == "TB_BotB_Kangor_Lorebook1")
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
