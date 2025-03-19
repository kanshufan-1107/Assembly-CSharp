using System.Collections;
using System.Collections.Generic;
using Blizzard.T5.Core;
using UnityEngine;

public class TB_BattleoftheBands_Manastorm : TB_BattleoftheBands_Dungeon
{
	private static Map<GameEntityOption, bool> s_booleanOptions = InitBooleanOptions();

	private static readonly AssetReference VO_TB_BotB_Manastorm_Male_Gnome_TB_BotB_Mission4EmoteResponse_01 = new AssetReference("VO_TB_BotB_Manastorm_Male_Gnome_TB_BotB_Mission4EmoteResponse_01.prefab:6f1907920574f7141932afc3d4e4c59b");

	private static readonly AssetReference VO_TB_BotB_Manastorm_Male_Gnome_TB_BotB_Mission4ExchangeA_01 = new AssetReference("VO_TB_BotB_Manastorm_Male_Gnome_TB_BotB_Mission4ExchangeA_01.prefab:f7116f13ef6c38046af4affb2e178579");

	private static readonly AssetReference VO_TB_BotB_Manastorm_Male_Gnome_TB_BotB_Mission4ExchangeA_03 = new AssetReference("VO_TB_BotB_Manastorm_Male_Gnome_TB_BotB_Mission4ExchangeA_03.prefab:628712a32436886498aa407d95356d4e");

	private static readonly AssetReference VO_TB_BotB_Manastorm_Male_Gnome_TB_BotB_Mission4ExchangeB_01 = new AssetReference("VO_TB_BotB_Manastorm_Male_Gnome_TB_BotB_Mission4ExchangeB_01.prefab:3e5faae6f82947447b28063f3088fc87");

	private static readonly AssetReference VO_TB_BotB_Manastorm_Male_Gnome_TB_BotB_Mission4HeroPower_01 = new AssetReference("VO_TB_BotB_Manastorm_Male_Gnome_TB_BotB_Mission4HeroPower_01.prefab:254dad3fe6ee62a42b56094e285248c9");

	private static readonly AssetReference VO_TB_BotB_Manastorm_Male_Gnome_TB_BotB_Mission4HeroPower_02 = new AssetReference("VO_TB_BotB_Manastorm_Male_Gnome_TB_BotB_Mission4HeroPower_02.prefab:01323583f2d6b054bb8df3d0340a2f9b");

	private static readonly AssetReference VO_TB_BotB_Manastorm_Male_Gnome_TB_BotB_Mission4HeroPower_03 = new AssetReference("VO_TB_BotB_Manastorm_Male_Gnome_TB_BotB_Mission4HeroPower_03.prefab:c9aaa18bdd32a0b42bedaeba79c50674");

	private static readonly AssetReference VO_TB_BotB_Manastorm_Male_Gnome_TB_BotB_Mission4Idle_01 = new AssetReference("VO_TB_BotB_Manastorm_Male_Gnome_TB_BotB_Mission4Idle_01.prefab:3c4c75f84cc97d844946a6162ae74d11");

	private static readonly AssetReference VO_TB_BotB_Manastorm_Male_Gnome_TB_BotB_Mission4Idle_02 = new AssetReference("VO_TB_BotB_Manastorm_Male_Gnome_TB_BotB_Mission4Idle_02.prefab:e775b7724005b894da1f3107b3578b6c");

	private static readonly AssetReference VO_TB_BotB_Manastorm_Male_Gnome_TB_BotB_Mission4Idle_03 = new AssetReference("VO_TB_BotB_Manastorm_Male_Gnome_TB_BotB_Mission4Idle_03.prefab:a2e765828f78e3b42b9126fe69414db4");

	private static readonly AssetReference VO_TB_BotB_Manastorm_Male_Gnome_TB_BotB_Mission4Loss_01 = new AssetReference("VO_TB_BotB_Manastorm_Male_Gnome_TB_BotB_Mission4Loss_01.prefab:37577ef5dd89e634fa6130c99501427b");

	private static readonly AssetReference VO_TB_BotB_Manastorm_Male_Gnome_TB_BotB_Mission4Start_02 = new AssetReference("VO_TB_BotB_Manastorm_Male_Gnome_TB_BotB_Mission4Start_02.prefab:7752736007d9005428b8b8c152490b0f");

	private static readonly AssetReference VO_TB_BotB_Manastorm_Male_Gnome_TB_BotB_Mission4Victory_01 = new AssetReference("VO_TB_BotB_Manastorm_Male_Gnome_TB_BotB_Mission4Victory_01.prefab:87da348c1eeed7f4d96bbdbfb05532cf");

	private static readonly AssetReference VO_TB_BotB_Mission4_Lorebook1_Female_Gnome_TB_BotB_Mission4Lorebook_01 = new AssetReference("VO_TB_BotB_Mission4_Lorebook1_Female_Gnome_TB_BotB_Mission4Lorebook_01.prefab:fbb522de0e399ad4d907189f5b3d0aa1");

	private static readonly AssetReference VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission4ExchangeA_02 = new AssetReference("VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission4ExchangeA_02.prefab:b248d8069cbcab2409fd2b5107a59540");

	private static readonly AssetReference VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission4ExchangeB_02 = new AssetReference("VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission4ExchangeB_02.prefab:a385c694531da4643a1b2f0213ef9218");

	private static readonly AssetReference VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission4Start_01 = new AssetReference("VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission4Start_01.prefab:8cec9c9f9c9d0c14695e277da7c1ccc0");

	private static readonly AssetReference VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission4Victory_02 = new AssetReference("VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission4Victory_02.prefab:e1a1c07820f2c634fa634c768ad29111");

	private static readonly AssetReference VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_PreMission4_01 = new AssetReference("VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_PreMission4_01.prefab:a076dbf6084e87d459cc8b6d810c97a8");

	private Notification m_popup;

	private Dictionary<int, string[]> m_popUpInfo = new Dictionary<int, string[]> { 
	{
		228,
		new string[1] { "TB_BOTB_MANASTORM_LOREBOOK1" }
	} };

	private float popUpScale = 1.65f;

	private Vector3 popUpPos;

	private new List<string> m_BossIdleLines = new List<string> { VO_TB_BotB_Manastorm_Male_Gnome_TB_BotB_Mission4Idle_01, VO_TB_BotB_Manastorm_Male_Gnome_TB_BotB_Mission4Idle_02, VO_TB_BotB_Manastorm_Male_Gnome_TB_BotB_Mission4Idle_03 };

	private List<string> m_BossUsesHeroPowerLines = new List<string> { VO_TB_BotB_Manastorm_Male_Gnome_TB_BotB_Mission4HeroPower_01, VO_TB_BotB_Manastorm_Male_Gnome_TB_BotB_Mission4HeroPower_02, VO_TB_BotB_Manastorm_Male_Gnome_TB_BotB_Mission4HeroPower_03 };

	private HashSet<string> m_playedLines = new HashSet<string>();

	private static Map<GameEntityOption, bool> InitBooleanOptions()
	{
		return new Map<GameEntityOption, bool> { 
		{
			GameEntityOption.DO_OPENING_TAUNTS,
			false
		} };
	}

	public TB_BattleoftheBands_Manastorm()
	{
		m_gameOptions.AddBooleanOptions(s_booleanOptions);
	}

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> m_SoundFiles = new List<string>
		{
			VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission4Start_01, VO_TB_BotB_Manastorm_Male_Gnome_TB_BotB_Mission4Start_02, VO_TB_BotB_Manastorm_Male_Gnome_TB_BotB_Mission4ExchangeA_01, VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission4ExchangeA_02, VO_TB_BotB_Manastorm_Male_Gnome_TB_BotB_Mission4ExchangeA_03, VO_TB_BotB_Manastorm_Male_Gnome_TB_BotB_Mission4ExchangeB_01, VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission4ExchangeB_02, VO_TB_BotB_Manastorm_Male_Gnome_TB_BotB_Mission4Victory_01, VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission4Victory_02, VO_TB_BotB_Manastorm_Male_Gnome_TB_BotB_Mission4HeroPower_01,
			VO_TB_BotB_Manastorm_Male_Gnome_TB_BotB_Mission4HeroPower_02, VO_TB_BotB_Manastorm_Male_Gnome_TB_BotB_Mission4HeroPower_03, VO_TB_BotB_Manastorm_Male_Gnome_TB_BotB_Mission4Idle_01, VO_TB_BotB_Manastorm_Male_Gnome_TB_BotB_Mission4Idle_02, VO_TB_BotB_Manastorm_Male_Gnome_TB_BotB_Mission4Idle_03, VO_TB_BotB_Manastorm_Male_Gnome_TB_BotB_Mission4Loss_01, VO_TB_BotB_Manastorm_Male_Gnome_TB_BotB_Mission4EmoteResponse_01, VO_TB_BotB_Mission4_Lorebook1_Female_Gnome_TB_BotB_Mission4Lorebook_01
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
			yield return MissionPlayVO(enemyActor, VO_TB_BotB_Manastorm_Male_Gnome_TB_BotB_Mission4EmoteResponse_01);
			break;
		case 514:
			yield return MissionPlayVO(friendlyActor, VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission4Start_01);
			yield return MissionPlayVO(enemyActor, VO_TB_BotB_Manastorm_Male_Gnome_TB_BotB_Mission4Start_02);
			break;
		case 507:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(enemyActor, VO_TB_BotB_Manastorm_Male_Gnome_TB_BotB_Mission4Loss_01);
			GameState.Get().SetBusy(busy: false);
			break;
		case 504:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(enemyActor, VO_TB_BotB_Manastorm_Male_Gnome_TB_BotB_Mission4Victory_01);
			yield return MissionPlayVO(friendlyActor, VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission4Victory_02);
			GameState.Get().SetBusy(busy: false);
			break;
		case 228:
			if (!(m_popup != null))
			{
				GameState.Get().SetBusy(busy: true);
				m_popup = NotificationManager.Get().CreatePopupText(UserAttentionBlocker.NONE, popUpPos, TutorialEntity.GetTextScale() * popUpScale, GameStrings.Get(m_popUpInfo[missionEvent][0]), convertLegacyPosition: false, NotificationManager.PopupTextType.FANCY);
				yield return GameEntity.Coroutines.StartCoroutine(PlaySoundAndBlockSpeech(VO_TB_BotB_Mission4_Lorebook1_Female_Gnome_TB_BotB_Mission4Lorebook_01, Notification.SpeechBubbleDirection.None, GetEnemyActorByCardId("TB_BotB_Manastorm_Lorebook1"), 2.5f));
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
			yield return MissionPlayVO(enemyActor, VO_TB_BotB_Manastorm_Male_Gnome_TB_BotB_Mission4ExchangeA_01);
			yield return MissionPlayVO(friendlyActor, VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission4ExchangeA_02);
			yield return MissionPlayVO(enemyActor, VO_TB_BotB_Manastorm_Male_Gnome_TB_BotB_Mission4ExchangeA_03);
			break;
		case 7:
			yield return MissionPlayVO(enemyActor, VO_TB_BotB_Manastorm_Male_Gnome_TB_BotB_Mission4ExchangeB_01);
			yield return MissionPlayVO(friendlyActor, VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission4ExchangeB_02);
			break;
		}
	}

	public override bool NotifyOfBattlefieldCardClicked(Entity clickedEntity, bool wasInTargetMode)
	{
		if (!base.NotifyOfBattlefieldCardClicked(clickedEntity, wasInTargetMode))
		{
			return false;
		}
		if (!wasInTargetMode && clickedEntity.GetCardId() == "TB_BotB_Manastorm_Lorebook1")
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
