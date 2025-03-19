using System.Collections;
using System.Collections.Generic;
using Blizzard.T5.Core;
using UnityEngine;

public class TB_BattleoftheBands_Halveria : TB_BattleoftheBands_Dungeon
{
	private static Map<GameEntityOption, bool> s_booleanOptions = InitBooleanOptions();

	private static readonly AssetReference VO_TB_BotB_Halveria_Female_Worgen_TB_BotB_Mission11EmoteResponse_01 = new AssetReference("VO_TB_BotB_Halveria_Female_Worgen_TB_BotB_Mission11EmoteResponse_01.prefab:be153593bb1a76d4a8584c3842116094");

	private static readonly AssetReference VO_TB_BotB_Halveria_Female_Worgen_TB_BotB_Mission11ExchangeA_01 = new AssetReference("VO_TB_BotB_Halveria_Female_Worgen_TB_BotB_Mission11ExchangeA_01.prefab:36365ebd3ee69dc48bf7a78172da239e");

	private static readonly AssetReference VO_TB_BotB_Halveria_Female_Worgen_TB_BotB_Mission11ExchangeB_01 = new AssetReference("VO_TB_BotB_Halveria_Female_Worgen_TB_BotB_Mission11ExchangeB_01.prefab:d0f71139c495b0045856b9da7783a7aa");

	private static readonly AssetReference VO_TB_BotB_Halveria_Female_Worgen_TB_BotB_Mission11HeroPower_01 = new AssetReference("VO_TB_BotB_Halveria_Female_Worgen_TB_BotB_Mission11HeroPower_01.prefab:17515c530979c3f4d9c3c4b167902985");

	private static readonly AssetReference VO_TB_BotB_Halveria_Female_Worgen_TB_BotB_Mission11HeroPower_02 = new AssetReference("VO_TB_BotB_Halveria_Female_Worgen_TB_BotB_Mission11HeroPower_02.prefab:8244eb8a493b4604b82ff8b7a19393e0");

	private static readonly AssetReference VO_TB_BotB_Halveria_Female_Worgen_TB_BotB_Mission11HeroPower_03 = new AssetReference("VO_TB_BotB_Halveria_Female_Worgen_TB_BotB_Mission11HeroPower_03.prefab:db7096ac92301f74f96bce962ae9234e");

	private static readonly AssetReference VO_TB_BotB_Halveria_Female_Worgen_TB_BotB_Mission11Idle_01 = new AssetReference("VO_TB_BotB_Halveria_Female_Worgen_TB_BotB_Mission11Idle_01.prefab:6d6ca13f449729843a50861f70e50aed");

	private static readonly AssetReference VO_TB_BotB_Halveria_Female_Worgen_TB_BotB_Mission11Idle_02 = new AssetReference("VO_TB_BotB_Halveria_Female_Worgen_TB_BotB_Mission11Idle_02.prefab:fb54259c140b2c842a94e9b6f5bcc683");

	private static readonly AssetReference VO_TB_BotB_Halveria_Female_Worgen_TB_BotB_Mission11Idle_03 = new AssetReference("VO_TB_BotB_Halveria_Female_Worgen_TB_BotB_Mission11Idle_03.prefab:5e0d06c01264e3c409149b79e60d7f3c");

	private static readonly AssetReference VO_TB_BotB_Halveria_Female_Worgen_TB_BotB_Mission11Lorebook_01 = new AssetReference("VO_TB_BotB_Halveria_Female_Worgen_TB_BotB_Mission11Lorebook_01.prefab:d7b3ade0bdcb985449422398f1f00b5b");

	private static readonly AssetReference VO_TB_BotB_Halveria_Female_Worgen_TB_BotB_Mission11Loss_01 = new AssetReference("VO_TB_BotB_Halveria_Female_Worgen_TB_BotB_Mission11Loss_01.prefab:242cf79ebf6459342b07ec36f58e8ad0");

	private static readonly AssetReference VO_TB_BotB_Halveria_Female_Worgen_TB_BotB_Mission11Start_01 = new AssetReference("VO_TB_BotB_Halveria_Female_Worgen_TB_BotB_Mission11Start_01.prefab:2146a781ad341b84f9a28009c9473e95");

	private static readonly AssetReference VO_TB_BotB_Halveria_Female_Worgen_TB_BotB_Mission11Victory_01 = new AssetReference("VO_TB_BotB_Halveria_Female_Worgen_TB_BotB_Mission11Victory_01.prefab:49c8093431ca5dc4296f4c58ce4ed976");

	private static readonly AssetReference VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission11ExchangeA_02 = new AssetReference("VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission11ExchangeA_02.prefab:1f757319a1686c14f866fb9abe93625a");

	private static readonly AssetReference VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission11ExchangeA_03 = new AssetReference("VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission11ExchangeA_03.prefab:2c5d20e9375a609429b21984f2ea3662");

	private static readonly AssetReference VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission11ExchangeA_04 = new AssetReference("VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission11ExchangeA_04.prefab:f12e35e81a1e34440b13efc5bf28f424");

	private static readonly AssetReference VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission11ExchangeA_05 = new AssetReference("VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission11ExchangeA_05.prefab:ac423b2c7629b584d85dcba5678e11ea");

	private static readonly AssetReference VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission11ExchangeA_06 = new AssetReference("VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission11ExchangeA_06.prefab:3765f036a65c38f42bae68974b3964c9");

	private static readonly AssetReference VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission11ExchangeB_02 = new AssetReference("VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission11ExchangeB_02.prefab:d532faa5517b68246a144cb98baa38c9");

	private static readonly AssetReference VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission11Start_02 = new AssetReference("VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission11Start_02.prefab:be716906359e984488d344d5797f6e73");

	private static readonly AssetReference VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission11Start_03 = new AssetReference("VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission11Start_03.prefab:c608a053505c4c44786ed6bc7e0106cd");

	private static readonly AssetReference VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission11Victory_02 = new AssetReference("VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission11Victory_02.prefab:4004e5c1c6c9e844ea9b10bee77ac01c");

	private Notification m_popup;

	private Dictionary<int, string[]> m_popUpInfo = new Dictionary<int, string[]> { 
	{
		228,
		new string[1] { "TB_BOTB_HALVERIA_LOREBOOK1" }
	} };

	private float popUpScale = 1.65f;

	private Vector3 popUpPos;

	private new List<string> m_BossIdleLines = new List<string> { VO_TB_BotB_Halveria_Female_Worgen_TB_BotB_Mission11Idle_01, VO_TB_BotB_Halveria_Female_Worgen_TB_BotB_Mission11Idle_02, VO_TB_BotB_Halveria_Female_Worgen_TB_BotB_Mission11Idle_03 };

	private List<string> m_BossUsesHeroPowerLines = new List<string> { VO_TB_BotB_Halveria_Female_Worgen_TB_BotB_Mission11HeroPower_01, VO_TB_BotB_Halveria_Female_Worgen_TB_BotB_Mission11HeroPower_02, VO_TB_BotB_Halveria_Female_Worgen_TB_BotB_Mission11HeroPower_03 };

	private HashSet<string> m_playedLines = new HashSet<string>();

	private static Map<GameEntityOption, bool> InitBooleanOptions()
	{
		return new Map<GameEntityOption, bool> { 
		{
			GameEntityOption.DO_OPENING_TAUNTS,
			false
		} };
	}

	public TB_BattleoftheBands_Halveria()
	{
		m_gameOptions.AddBooleanOptions(s_booleanOptions);
	}

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> m_SoundFiles = new List<string>
		{
			VO_TB_BotB_Halveria_Female_Worgen_TB_BotB_Mission11Start_01, VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission11Start_03, VO_TB_BotB_Halveria_Female_Worgen_TB_BotB_Mission11ExchangeA_01, VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission11ExchangeA_02, VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission11ExchangeA_03, VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission11ExchangeA_04, VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission11ExchangeA_05, VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission11ExchangeA_06, VO_TB_BotB_Halveria_Female_Worgen_TB_BotB_Mission11ExchangeB_01, VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission11ExchangeB_02,
			VO_TB_BotB_Halveria_Female_Worgen_TB_BotB_Mission11Victory_01, VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission11Victory_02, VO_TB_BotB_Halveria_Female_Worgen_TB_BotB_Mission11HeroPower_01, VO_TB_BotB_Halveria_Female_Worgen_TB_BotB_Mission11HeroPower_02, VO_TB_BotB_Halveria_Female_Worgen_TB_BotB_Mission11HeroPower_03, VO_TB_BotB_Halveria_Female_Worgen_TB_BotB_Mission11Idle_01, VO_TB_BotB_Halveria_Female_Worgen_TB_BotB_Mission11Idle_02, VO_TB_BotB_Halveria_Female_Worgen_TB_BotB_Mission11Idle_03, VO_TB_BotB_Halveria_Female_Worgen_TB_BotB_Mission11Loss_01, VO_TB_BotB_Halveria_Female_Worgen_TB_BotB_Mission11EmoteResponse_01,
			VO_TB_BotB_Halveria_Female_Worgen_TB_BotB_Mission11Lorebook_01
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
			yield return MissionPlayVO(enemyActor, VO_TB_BotB_Halveria_Female_Worgen_TB_BotB_Mission11EmoteResponse_01);
			break;
		case 514:
			yield return MissionPlayVO(enemyActor, VO_TB_BotB_Halveria_Female_Worgen_TB_BotB_Mission11Start_01);
			yield return MissionPlayVO(friendlyActor, VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission11Start_03);
			break;
		case 507:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(enemyActor, VO_TB_BotB_Halveria_Female_Worgen_TB_BotB_Mission11Loss_01);
			GameState.Get().SetBusy(busy: false);
			break;
		case 504:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(enemyActor, VO_TB_BotB_Halveria_Female_Worgen_TB_BotB_Mission11Victory_01);
			yield return MissionPlayVO(friendlyActor, VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission11Victory_02);
			GameState.Get().SetBusy(busy: false);
			break;
		case 101:
			yield return MissionPlayVO(enemyActor, VO_TB_BotB_Halveria_Female_Worgen_TB_BotB_Mission11ExchangeA_01);
			yield return MissionPlayVO(friendlyActor, VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission11ExchangeA_02);
			break;
		case 102:
			yield return MissionPlayVO(enemyActor, VO_TB_BotB_Halveria_Female_Worgen_TB_BotB_Mission11ExchangeA_01);
			yield return MissionPlayVO(friendlyActor, VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission11ExchangeA_03);
			break;
		case 103:
			yield return MissionPlayVO(enemyActor, VO_TB_BotB_Halveria_Female_Worgen_TB_BotB_Mission11ExchangeA_01);
			yield return MissionPlayVO(friendlyActor, VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission11ExchangeA_04);
			break;
		case 104:
			yield return MissionPlayVO(enemyActor, VO_TB_BotB_Halveria_Female_Worgen_TB_BotB_Mission11ExchangeA_01);
			yield return MissionPlayVO(friendlyActor, VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission11ExchangeA_05);
			break;
		case 105:
			yield return MissionPlayVO(enemyActor, VO_TB_BotB_Halveria_Female_Worgen_TB_BotB_Mission11ExchangeA_01);
			yield return MissionPlayVO(friendlyActor, VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission11ExchangeA_06);
			break;
		case 228:
			if (!(m_popup != null))
			{
				GameState.Get().SetBusy(busy: true);
				m_popup = NotificationManager.Get().CreatePopupText(UserAttentionBlocker.NONE, popUpPos, TutorialEntity.GetTextScale() * popUpScale, GameStrings.Get(m_popUpInfo[missionEvent][0]), convertLegacyPosition: false, NotificationManager.PopupTextType.FANCY);
				yield return GameEntity.Coroutines.StartCoroutine(PlaySoundAndBlockSpeech(VO_TB_BotB_Halveria_Female_Worgen_TB_BotB_Mission11Lorebook_01, Notification.SpeechBubbleDirection.None, GetEnemyActorByCardId("TB_BotB_Halveria_Lorebook1"), 2.5f));
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
		if (turn == 9)
		{
			yield return MissionPlayVO(enemyActor, VO_TB_BotB_Halveria_Female_Worgen_TB_BotB_Mission11ExchangeB_01);
			yield return MissionPlayVO(friendlyActor, VO_TB_BotB_RisingStar_Warrior_Nonbinary_NightElf_TB_BotB_Mission11ExchangeB_02);
		}
	}

	public override bool NotifyOfBattlefieldCardClicked(Entity clickedEntity, bool wasInTargetMode)
	{
		if (!base.NotifyOfBattlefieldCardClicked(clickedEntity, wasInTargetMode))
		{
			return false;
		}
		if (!wasInTargetMode && clickedEntity.GetCardId() == "TB_BotB_Halveria_Lorebook1")
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
