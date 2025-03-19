using System.Collections;
using System.Collections.Generic;
using Blizzard.T5.Core;
using UnityEngine;

public class TB_BattleoftheBands_Rin : TB_BattleoftheBands_Dungeon
{
	private static Map<GameEntityOption, bool> s_booleanOptions = InitBooleanOptions();

	private static readonly AssetReference VO_TB_BotB_ETC_Male_Tauren_TB_BotB_Mission8Lorebook_01 = new AssetReference("VO_TB_BotB_ETC_Male_Tauren_TB_BotB_Mission8Lorebook_01.prefab:52a1cdc98a655b04fb325a2e5ef92d6b");

	private static readonly AssetReference VO_TB_BotB_Rin_Female_Gnome_TB_BotB_Mission8EmoteResponse_01 = new AssetReference("VO_TB_BotB_Rin_Female_Gnome_TB_BotB_Mission8EmoteResponse_01.prefab:cc6f178ec8f78594bb88b1539e1b56f5");

	private static readonly AssetReference VO_TB_BotB_Rin_Female_Gnome_TB_BotB_Mission8ExchangeA_01 = new AssetReference("VO_TB_BotB_Rin_Female_Gnome_TB_BotB_Mission8ExchangeA_01.prefab:294f0cf37089da047a2e0ced4fa25528");

	private static readonly AssetReference VO_TB_BotB_Rin_Female_Gnome_TB_BotB_Mission8ExchangeA_03 = new AssetReference("VO_TB_BotB_Rin_Female_Gnome_TB_BotB_Mission8ExchangeA_03.prefab:b8d6d96131c2d994185b818d4202862b");

	private static readonly AssetReference VO_TB_BotB_Rin_Female_Gnome_TB_BotB_Mission8ExchangeB_01 = new AssetReference("VO_TB_BotB_Rin_Female_Gnome_TB_BotB_Mission8ExchangeB_01.prefab:d6969613196e0eb4a8cf65a3990d8521");

	private static readonly AssetReference VO_TB_BotB_Rin_Female_Gnome_TB_BotB_Mission8ExchangeB_03 = new AssetReference("VO_TB_BotB_Rin_Female_Gnome_TB_BotB_Mission8ExchangeB_03.prefab:29b9e93592eb10e48a90b64d02d6a539");

	private static readonly AssetReference VO_TB_BotB_Rin_Female_Gnome_TB_BotB_Mission8ExchangeC_01 = new AssetReference("VO_TB_BotB_Rin_Female_Gnome_TB_BotB_Mission8ExchangeC_01.prefab:4bb3ab5d27636a642aca419eb4b79d00");

	private static readonly AssetReference VO_TB_BotB_Rin_Female_Gnome_TB_BotB_Mission8HeroPower_01 = new AssetReference("VO_TB_BotB_Rin_Female_Gnome_TB_BotB_Mission8HeroPower_01.prefab:13b59589d59d07740aaa69cffd6ba45c");

	private static readonly AssetReference VO_TB_BotB_Rin_Female_Gnome_TB_BotB_Mission8HeroPower_02 = new AssetReference("VO_TB_BotB_Rin_Female_Gnome_TB_BotB_Mission8HeroPower_02.prefab:5f8ff6c21967a0b43a51a66f33898c48");

	private static readonly AssetReference VO_TB_BotB_Rin_Female_Gnome_TB_BotB_Mission8HeroPower_03 = new AssetReference("VO_TB_BotB_Rin_Female_Gnome_TB_BotB_Mission8HeroPower_03.prefab:c5a50f2c6034d7d419f9590bb39bce1b");

	private static readonly AssetReference VO_TB_BotB_Rin_Female_Gnome_TB_BotB_Mission8Idle_01 = new AssetReference("VO_TB_BotB_Rin_Female_Gnome_TB_BotB_Mission8Idle_01.prefab:362bf6b6ad7d08b41bacf02b188f298e");

	private static readonly AssetReference VO_TB_BotB_Rin_Female_Gnome_TB_BotB_Mission8Idle_02 = new AssetReference("VO_TB_BotB_Rin_Female_Gnome_TB_BotB_Mission8Idle_02.prefab:20c42c1082de911459d7e62a2bcc1223");

	private static readonly AssetReference VO_TB_BotB_Rin_Female_Gnome_TB_BotB_Mission8Idle_03 = new AssetReference("VO_TB_BotB_Rin_Female_Gnome_TB_BotB_Mission8Idle_03.prefab:1c394d4a8a355f644a6aaaf312f5e698");

	private static readonly AssetReference VO_TB_BotB_Rin_Female_Gnome_TB_BotB_Mission8Loss_01 = new AssetReference("VO_TB_BotB_Rin_Female_Gnome_TB_BotB_Mission8Loss_01.prefab:00e6bc2dc9e711b4aa992d15611587fc");

	private static readonly AssetReference VO_TB_BotB_Rin_Female_Gnome_TB_BotB_Mission8Start_01 = new AssetReference("VO_TB_BotB_Rin_Female_Gnome_TB_BotB_Mission8Start_01.prefab:d1075378a21bda349affa1ebdfbd72fc");

	private static readonly AssetReference VO_TB_BotB_Rin_Female_Gnome_TB_BotB_Mission8Victory_01 = new AssetReference("VO_TB_BotB_Rin_Female_Gnome_TB_BotB_Mission8Victory_01.prefab:a20c96a4f354cdb4d82fe46090e5144d");

	private static readonly AssetReference VO_TB_BotB_Rin_Female_Gnome_TB_BotB_Mission8Victory_03 = new AssetReference("VO_TB_BotB_Rin_Female_Gnome_TB_BotB_Mission8Victory_03.prefab:5f6b3aa43ac3e834b8e011845e59d802");

	private static readonly AssetReference VO_TB_BotB_Rin_Female_Gnome_TB_BotB_Mission8Victory_05 = new AssetReference("VO_TB_BotB_Rin_Female_Gnome_TB_BotB_Mission8Victory_05.prefab:cbe4ee36b2882e64c9ae957f9599b07f");

	private static readonly AssetReference VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission8ExchangeA_02 = new AssetReference("VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission8ExchangeA_02.prefab:6c00be73e3535ac429b307b22de59ddc");

	private static readonly AssetReference VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission8ExchangeA_04 = new AssetReference("VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission8ExchangeA_04.prefab:fa70cd2f5cd0fcc4a938f9ff1835ba8d");

	private static readonly AssetReference VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission8ExchangeB_02 = new AssetReference("VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission8ExchangeB_02.prefab:cb5da25803c19e6428639281926dc005");

	private static readonly AssetReference VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission8ExchangeC_02 = new AssetReference("VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission8ExchangeC_02.prefab:ef18a4b1331359c4dba010147d31061d");

	private static readonly AssetReference VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission8Start_02 = new AssetReference("VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission8Start_02.prefab:dacd57fc9a05594489d6bb5834444785");

	private static readonly AssetReference VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission8Victory_02 = new AssetReference("VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission8Victory_02.prefab:06772732f9be36c4f913dc754c1170dd");

	private static readonly AssetReference VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission8Victory_04 = new AssetReference("VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission8Victory_04.prefab:e5e61464da77fe5469b47593572b17be");

	private Notification m_popup;

	private Dictionary<int, string[]> m_popUpInfo = new Dictionary<int, string[]> { 
	{
		228,
		new string[1] { "TB_BOTB_RIN_LOREBOOK1" }
	} };

	private float popUpScale = 1.65f;

	private Vector3 popUpPos;

	private new List<string> m_BossIdleLines = new List<string> { VO_TB_BotB_Rin_Female_Gnome_TB_BotB_Mission8Idle_01, VO_TB_BotB_Rin_Female_Gnome_TB_BotB_Mission8Idle_02, VO_TB_BotB_Rin_Female_Gnome_TB_BotB_Mission8Idle_03 };

	private List<string> m_BossUsesHeroPowerLines = new List<string> { VO_TB_BotB_Rin_Female_Gnome_TB_BotB_Mission8HeroPower_01, VO_TB_BotB_Rin_Female_Gnome_TB_BotB_Mission8HeroPower_02, VO_TB_BotB_Rin_Female_Gnome_TB_BotB_Mission8HeroPower_03 };

	private HashSet<string> m_playedLines = new HashSet<string>();

	private static Map<GameEntityOption, bool> InitBooleanOptions()
	{
		return new Map<GameEntityOption, bool> { 
		{
			GameEntityOption.DO_OPENING_TAUNTS,
			false
		} };
	}

	public TB_BattleoftheBands_Rin()
	{
		m_gameOptions.AddBooleanOptions(s_booleanOptions);
	}

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> m_SoundFiles = new List<string>
		{
			VO_TB_BotB_Rin_Female_Gnome_TB_BotB_Mission8Start_01, VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission8Start_02, VO_TB_BotB_Rin_Female_Gnome_TB_BotB_Mission8ExchangeA_01, VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission8ExchangeA_02, VO_TB_BotB_Rin_Female_Gnome_TB_BotB_Mission8ExchangeA_03, VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission8ExchangeA_04, VO_TB_BotB_Rin_Female_Gnome_TB_BotB_Mission8ExchangeB_01, VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission8ExchangeB_02, VO_TB_BotB_Rin_Female_Gnome_TB_BotB_Mission8ExchangeB_03, VO_TB_BotB_Rin_Female_Gnome_TB_BotB_Mission8ExchangeC_01,
			VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission8ExchangeC_02, VO_TB_BotB_Rin_Female_Gnome_TB_BotB_Mission8Victory_01, VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission8Victory_02, VO_TB_BotB_Rin_Female_Gnome_TB_BotB_Mission8Victory_03, VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission8Victory_04, VO_TB_BotB_Rin_Female_Gnome_TB_BotB_Mission8Victory_05, VO_TB_BotB_Rin_Female_Gnome_TB_BotB_Mission8HeroPower_01, VO_TB_BotB_Rin_Female_Gnome_TB_BotB_Mission8HeroPower_02, VO_TB_BotB_Rin_Female_Gnome_TB_BotB_Mission8HeroPower_03, VO_TB_BotB_Rin_Female_Gnome_TB_BotB_Mission8Idle_01,
			VO_TB_BotB_Rin_Female_Gnome_TB_BotB_Mission8Idle_02, VO_TB_BotB_Rin_Female_Gnome_TB_BotB_Mission8Idle_03, VO_TB_BotB_Rin_Female_Gnome_TB_BotB_Mission8Loss_01, VO_TB_BotB_Rin_Female_Gnome_TB_BotB_Mission8EmoteResponse_01, VO_TB_BotB_ETC_Male_Tauren_TB_BotB_Mission8Lorebook_01
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
			yield return MissionPlayVO(enemyActor, VO_TB_BotB_Rin_Female_Gnome_TB_BotB_Mission8EmoteResponse_01);
			break;
		case 514:
			yield return MissionPlayVO(enemyActor, VO_TB_BotB_Rin_Female_Gnome_TB_BotB_Mission8Start_01);
			yield return MissionPlayVO(friendlyActor, VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission8Start_02);
			break;
		case 507:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(enemyActor, VO_TB_BotB_Rin_Female_Gnome_TB_BotB_Mission8Loss_01);
			GameState.Get().SetBusy(busy: false);
			break;
		case 504:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(enemyActor, VO_TB_BotB_Rin_Female_Gnome_TB_BotB_Mission8Victory_01);
			yield return MissionPlayVO(friendlyActor, VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission8Victory_02);
			yield return MissionPlayVO(enemyActor, VO_TB_BotB_Rin_Female_Gnome_TB_BotB_Mission8Victory_03);
			yield return MissionPlayVO(friendlyActor, VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission8Victory_04);
			yield return MissionPlayVO(enemyActor, VO_TB_BotB_Rin_Female_Gnome_TB_BotB_Mission8Victory_05);
			GameState.Get().SetBusy(busy: false);
			break;
		case 228:
			if (!(m_popup != null))
			{
				GameState.Get().SetBusy(busy: true);
				m_popup = NotificationManager.Get().CreatePopupText(UserAttentionBlocker.NONE, popUpPos, TutorialEntity.GetTextScale() * popUpScale, GameStrings.Get(m_popUpInfo[missionEvent][0]), convertLegacyPosition: false, NotificationManager.PopupTextType.FANCY);
				yield return GameEntity.Coroutines.StartCoroutine(PlaySoundAndBlockSpeech(VO_TB_BotB_ETC_Male_Tauren_TB_BotB_Mission8Lorebook_01, Notification.SpeechBubbleDirection.None, GetEnemyActorByCardId("TB_BotB_Rin_Lorebook1"), 2.5f));
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
			yield return MissionPlayVO(enemyActor, VO_TB_BotB_Rin_Female_Gnome_TB_BotB_Mission8ExchangeA_01);
			yield return MissionPlayVO(friendlyActor, VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission8ExchangeA_02);
			yield return MissionPlayVO(enemyActor, VO_TB_BotB_Rin_Female_Gnome_TB_BotB_Mission8ExchangeA_03);
			yield return MissionPlayVO(friendlyActor, VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission8ExchangeA_04);
			break;
		case 7:
			yield return MissionPlayVO(enemyActor, VO_TB_BotB_Rin_Female_Gnome_TB_BotB_Mission8ExchangeB_01);
			yield return MissionPlayVO(friendlyActor, VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission8ExchangeB_02);
			yield return MissionPlayVO(enemyActor, VO_TB_BotB_Rin_Female_Gnome_TB_BotB_Mission8ExchangeB_03);
			break;
		case 13:
			yield return MissionPlayVO(enemyActor, VO_TB_BotB_Rin_Female_Gnome_TB_BotB_Mission8ExchangeC_01);
			yield return MissionPlayVO(friendlyActor, VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission8ExchangeC_02);
			break;
		}
	}

	public override bool NotifyOfBattlefieldCardClicked(Entity clickedEntity, bool wasInTargetMode)
	{
		if (!base.NotifyOfBattlefieldCardClicked(clickedEntity, wasInTargetMode))
		{
			return false;
		}
		if (!wasInTargetMode && clickedEntity.GetCardId() == "TB_BotB_Rin_Lorebook1")
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
