using System.Collections;
using System.Collections.Generic;
using Blizzard.T5.Core;
using UnityEngine;

public class TB_BattleoftheBands_Blingtron : TB_BattleoftheBands_Dungeon
{
	private static Map<GameEntityOption, bool> s_booleanOptions = InitBooleanOptions();

	private static readonly AssetReference VO_TB_BotB_Blingtron_Male_Mech_TB_BotB_Mission6EmoteResponse_01 = new AssetReference("VO_TB_BotB_Blingtron_Male_Mech_TB_BotB_Mission6EmoteResponse_01.prefab:7f21880b3d1a3a9449fe29b13a0062b2");

	private static readonly AssetReference VO_TB_BotB_Blingtron_Male_Mech_TB_BotB_Mission6ExchangeA_02 = new AssetReference("VO_TB_BotB_Blingtron_Male_Mech_TB_BotB_Mission6ExchangeA_02.prefab:8a1cf6ad92b717e4b8c6e24eaea8b7b8");

	private static readonly AssetReference VO_TB_BotB_Blingtron_Male_Mech_TB_BotB_Mission6ExchangeB_02 = new AssetReference("VO_TB_BotB_Blingtron_Male_Mech_TB_BotB_Mission6ExchangeB_02.prefab:a7b7b69d488ba1745bde560afbfb0576");

	private static readonly AssetReference VO_TB_BotB_Blingtron_Male_Mech_TB_BotB_Mission6HeroPower_01 = new AssetReference("VO_TB_BotB_Blingtron_Male_Mech_TB_BotB_Mission6HeroPower_01.prefab:0d820900cf8c5804dbbd3aeeea4493d8");

	private static readonly AssetReference VO_TB_BotB_Blingtron_Male_Mech_TB_BotB_Mission6HeroPower_02 = new AssetReference("VO_TB_BotB_Blingtron_Male_Mech_TB_BotB_Mission6HeroPower_02.prefab:c20859934b9e4e246a5551df3ffd5dc9");

	private static readonly AssetReference VO_TB_BotB_Blingtron_Male_Mech_TB_BotB_Mission6HeroPower_03 = new AssetReference("VO_TB_BotB_Blingtron_Male_Mech_TB_BotB_Mission6HeroPower_03.prefab:02242dcaf36dc554bbaeeefabfa69092");

	private static readonly AssetReference VO_TB_BotB_Blingtron_Male_Mech_TB_BotB_Mission6Idle_01 = new AssetReference("VO_TB_BotB_Blingtron_Male_Mech_TB_BotB_Mission6Idle_01.prefab:db1d63ebf8b8fe5498a99db52cf5a93d");

	private static readonly AssetReference VO_TB_BotB_Blingtron_Male_Mech_TB_BotB_Mission6Idle_02 = new AssetReference("VO_TB_BotB_Blingtron_Male_Mech_TB_BotB_Mission6Idle_02.prefab:c334fde7e71ac2249ba6c3afdaf0ea8e");

	private static readonly AssetReference VO_TB_BotB_Blingtron_Male_Mech_TB_BotB_Mission6Idle_03 = new AssetReference("VO_TB_BotB_Blingtron_Male_Mech_TB_BotB_Mission6Idle_03.prefab:f2a990dc0443625448d6fbdc872105a0");

	private static readonly AssetReference VO_TB_BotB_Blingtron_Male_Mech_TB_BotB_Mission6Loss_01 = new AssetReference("VO_TB_BotB_Blingtron_Male_Mech_TB_BotB_Mission6Loss_01.prefab:9740761fd51de08448382062d9e76cad");

	private static readonly AssetReference VO_TB_BotB_Blingtron_Male_Mech_TB_BotB_Mission6Start_01 = new AssetReference("VO_TB_BotB_Blingtron_Male_Mech_TB_BotB_Mission6Start_01.prefab:68766ef1d6ff15f4197d81568b692ba4");

	private static readonly AssetReference VO_TB_BotB_Blingtron_Male_Mech_TB_BotB_Mission6Victory_01 = new AssetReference("VO_TB_BotB_Blingtron_Male_Mech_TB_BotB_Mission6Victory_01.prefab:1f07912d33ae45347ae60001e05fa8d0");

	private static readonly AssetReference VO_TB_BotB_Mission6_Lorebook1_Female_Gnome_TB_BotB_Mission6Lorebook_01 = new AssetReference("VO_TB_BotB_Mission6_Lorebook1_Female_Gnome_TB_BotB_Mission6Lorebook_01.prefab:5d2abdc5e20b9ec46b337e3d680fb1ba");

	private static readonly AssetReference VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission6ExchangeA_01 = new AssetReference("VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission6ExchangeA_01.prefab:bd16e481c7513d74b969d003599fa248");

	private static readonly AssetReference VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission6ExchangeB_01 = new AssetReference("VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission6ExchangeB_01.prefab:b955a3c10d9423549b1910b0208dd1e7");

	private static readonly AssetReference VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission6Start_02 = new AssetReference("VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission6Start_02.prefab:43510049fc8d4334c9a9215974556449");

	private static readonly AssetReference VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission6Victory_02 = new AssetReference("VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission6Victory_02.prefab:76e108f647d0d9245b1fb8817d42038e");

	private Notification m_popup;

	private Dictionary<int, string[]> m_popUpInfo = new Dictionary<int, string[]> { 
	{
		228,
		new string[1] { "TB_BOTB_BLINGTRON_LOREBOOK1" }
	} };

	private float popUpScale = 1.65f;

	private Vector3 popUpPos;

	private new List<string> m_BossIdleLines = new List<string> { VO_TB_BotB_Blingtron_Male_Mech_TB_BotB_Mission6Idle_01, VO_TB_BotB_Blingtron_Male_Mech_TB_BotB_Mission6Idle_02, VO_TB_BotB_Blingtron_Male_Mech_TB_BotB_Mission6Idle_03 };

	private List<string> m_BossHeroPowerLines = new List<string> { VO_TB_BotB_Blingtron_Male_Mech_TB_BotB_Mission6HeroPower_01, VO_TB_BotB_Blingtron_Male_Mech_TB_BotB_Mission6HeroPower_02, VO_TB_BotB_Blingtron_Male_Mech_TB_BotB_Mission6HeroPower_03 };

	private HashSet<string> m_playedLines = new HashSet<string>();

	private static Map<GameEntityOption, bool> InitBooleanOptions()
	{
		return new Map<GameEntityOption, bool> { 
		{
			GameEntityOption.DO_OPENING_TAUNTS,
			false
		} };
	}

	public TB_BattleoftheBands_Blingtron()
	{
		m_gameOptions.AddBooleanOptions(s_booleanOptions);
	}

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> m_SoundFiles = new List<string>
		{
			VO_TB_BotB_Blingtron_Male_Mech_TB_BotB_Mission6Start_01, VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission6Start_02, VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission6ExchangeA_01, VO_TB_BotB_Blingtron_Male_Mech_TB_BotB_Mission6ExchangeA_02, VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission6ExchangeB_01, VO_TB_BotB_Blingtron_Male_Mech_TB_BotB_Mission6ExchangeB_02, VO_TB_BotB_Blingtron_Male_Mech_TB_BotB_Mission6Victory_01, VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission6Victory_02, VO_TB_BotB_Blingtron_Male_Mech_TB_BotB_Mission6HeroPower_01, VO_TB_BotB_Blingtron_Male_Mech_TB_BotB_Mission6HeroPower_02,
			VO_TB_BotB_Blingtron_Male_Mech_TB_BotB_Mission6HeroPower_03, VO_TB_BotB_Blingtron_Male_Mech_TB_BotB_Mission6Idle_01, VO_TB_BotB_Blingtron_Male_Mech_TB_BotB_Mission6Idle_02, VO_TB_BotB_Blingtron_Male_Mech_TB_BotB_Mission6Idle_03, VO_TB_BotB_Blingtron_Male_Mech_TB_BotB_Mission6Loss_01, VO_TB_BotB_Blingtron_Male_Mech_TB_BotB_Mission6EmoteResponse_01, VO_TB_BotB_Mission6_Lorebook1_Female_Gnome_TB_BotB_Mission6Lorebook_01
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
			yield return MissionPlayVO(enemyActor, VO_TB_BotB_Blingtron_Male_Mech_TB_BotB_Mission6EmoteResponse_01);
			break;
		case 514:
			yield return MissionPlayVO(enemyActor, VO_TB_BotB_Blingtron_Male_Mech_TB_BotB_Mission6Start_01);
			yield return MissionPlayVO(friendlyActor, VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission6Start_02);
			break;
		case 507:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(enemyActor, VO_TB_BotB_Blingtron_Male_Mech_TB_BotB_Mission6Loss_01);
			GameState.Get().SetBusy(busy: false);
			break;
		case 504:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(enemyActor, VO_TB_BotB_Blingtron_Male_Mech_TB_BotB_Mission6Victory_01);
			yield return MissionPlayVO(friendlyActor, VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission6Victory_02);
			GameState.Get().SetBusy(busy: false);
			break;
		case 101:
			GameState.Get().SetBusy(busy: true);
			yield return PlayLineInOrderOnce(enemyActor, m_BossHeroPowerLines);
			GameState.Get().SetBusy(busy: false);
			break;
		case 228:
			if (!(m_popup != null))
			{
				GameState.Get().SetBusy(busy: true);
				m_popup = NotificationManager.Get().CreatePopupText(UserAttentionBlocker.NONE, popUpPos, TutorialEntity.GetTextScale() * popUpScale, GameStrings.Get(m_popUpInfo[missionEvent][0]), convertLegacyPosition: false, NotificationManager.PopupTextType.FANCY);
				yield return GameEntity.Coroutines.StartCoroutine(PlaySoundAndBlockSpeech(VO_TB_BotB_Mission6_Lorebook1_Female_Gnome_TB_BotB_Mission6Lorebook_01, Notification.SpeechBubbleDirection.None, GetEnemyActorByCardId("TB_BotB_Blingtron_Lorebook1"), 2.5f));
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
			yield return MissionPlayVO(friendlyActor, VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission6ExchangeA_01);
			yield return MissionPlayVO(enemyActor, VO_TB_BotB_Blingtron_Male_Mech_TB_BotB_Mission6ExchangeA_02);
			break;
		case 7:
			yield return MissionPlayVO(friendlyActor, VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission6ExchangeB_01);
			yield return MissionPlayVO(enemyActor, VO_TB_BotB_Blingtron_Male_Mech_TB_BotB_Mission6ExchangeB_02);
			break;
		}
	}

	public override bool NotifyOfBattlefieldCardClicked(Entity clickedEntity, bool wasInTargetMode)
	{
		if (!base.NotifyOfBattlefieldCardClicked(clickedEntity, wasInTargetMode))
		{
			return false;
		}
		if (!wasInTargetMode && clickedEntity.GetCardId() == "TB_BotB_Blingtron_Lorebook1")
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
