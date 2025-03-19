using System.Collections;
using System.Collections.Generic;
using Blizzard.T5.Core;
using UnityEngine;

public class BoH_Guldan_08 : BoH_Guldan_Dungeon
{
	private static Map<GameEntityOption, bool> s_booleanOptions = InitBooleanOptions();

	private static readonly AssetReference VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission8ExchangeA_01 = new AssetReference("VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission8ExchangeA_01.prefab:bfffa8d0c74245f48b32f7b35be36529");

	private static readonly AssetReference VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission8ExchangeB_01 = new AssetReference("VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission8ExchangeB_01.prefab:19035ed837cbc4044a3e1dfb5b66630f");

	private static readonly AssetReference VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission8ExchangeC_02 = new AssetReference("VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission8ExchangeC_02.prefab:9f0d16adc25ee744d915bfa3850199d7");

	private static readonly AssetReference VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission8ExchangeC_03 = new AssetReference("VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission8ExchangeC_03.prefab:4b82e8d371a339f4685518a779d4190b");

	private static readonly AssetReference VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission8ExchangeD_01 = new AssetReference("VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission8ExchangeD_01.prefab:c13deb01879587e4f85a35b6d7cbacab");

	private static readonly AssetReference VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission8Intro_01 = new AssetReference("VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission8Intro_01.prefab:a6d727be1a8401947a6979898be0066a");

	private static readonly AssetReference VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission8Victory_02 = new AssetReference("VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission8Victory_02.prefab:2df4a6511b075f04bb45304d0a55da8d");

	private static readonly AssetReference VO_Story_Hero_Sargeras_Male_Demon_Story_Guldan_Mission8Victory_01 = new AssetReference("VO_Story_Hero_Sargeras_Male_Demon_Story_Guldan_Mission8Victory_01.prefab:9e2bcef0aba425c4992be995db6eabfc");

	private static readonly AssetReference VO_Story_Hero_Sargeras_Male_Demon_Story_Guldan_Mission8Victory_03 = new AssetReference("VO_Story_Hero_Sargeras_Male_Demon_Story_Guldan_Mission8Victory_03.prefab:8556744386f9257438951fa1066e9126");

	private static readonly AssetReference VO_Story_Hero_Sargeras_Male_Demon_Story_Guldan_Mission8Victory_04 = new AssetReference("VO_Story_Hero_Sargeras_Male_Demon_Story_Guldan_Mission8Victory_04.prefab:217f42a9ace670b4496de56dd2ac6861");

	private static readonly AssetReference VO_Story_Hero_TombGuardian_Male_Demon_Story_Guldan_Mission8Death_01 = new AssetReference("VO_Story_Hero_TombGuardian_Male_Demon_Story_Guldan_Mission8Death_01.prefab:a6f51ea0cb304f529fc355190e23529f");

	private static readonly AssetReference VO_Story_Hero_TombGuardian_Male_Demon_Story_Guldan_Mission8EmoteResponse_01 = new AssetReference("VO_Story_Hero_TombGuardian_Male_Demon_Story_Guldan_Mission8EmoteResponse_01.prefab:0b12d88bcc3ac6847a940ac0d45274e8");

	private static readonly AssetReference VO_Story_Hero_TombGuardian_Male_Demon_Story_Guldan_Mission8Idle_01 = new AssetReference("VO_Story_Hero_TombGuardian_Male_Demon_Story_Guldan_Mission8Idle_01.prefab:009f00816bde3764793960d93c388add");

	private static readonly AssetReference VO_Story_Hero_TombGuardian_Male_Demon_Story_Guldan_Mission8Idle_02 = new AssetReference("VO_Story_Hero_TombGuardian_Male_Demon_Story_Guldan_Mission8Idle_02.prefab:377605b5c0841e24db78aeaf9e7950a1");

	private static readonly AssetReference VO_Story_Hero_TombGuardian_Male_Demon_Story_Guldan_Mission8Idle_03 = new AssetReference("VO_Story_Hero_TombGuardian_Male_Demon_Story_Guldan_Mission8Idle_03.prefab:b1ba6fdef9ac0a7479115c1f3bd8b35a");

	private static readonly AssetReference VO_Story_Hero_TombGuardian_Male_Demon_Story_Guldan_Mission8Intro_03 = new AssetReference("VO_Story_Hero_TombGuardian_Male_Demon_Story_Guldan_Mission8Intro_03.prefab:24735976c3b54f14793bff40a8f61e46");

	private static readonly AssetReference VO_Story_Hero_TombGuardian_Male_Demon_Story_Guldan_Mission8Loss_01 = new AssetReference("VO_Story_Hero_TombGuardian_Male_Demon_Story_Guldan_Mission8Loss_01.prefab:db2a2f57d8587cb48bc3f0983df56084");

	private static readonly AssetReference VO_Story_Minion_ChoGall_Male_Ogre_Story_Guldan_Mission8ExchangeC_01 = new AssetReference("VO_Story_Minion_ChoGall_Male_Ogre_Story_Guldan_Mission8ExchangeC_01.prefab:3cb93ac4fb14ac84394ad4e8569eccd0");

	private static readonly AssetReference VO_Story_Minion_ChoGall_Male_Ogre_Story_Guldan_Mission8ExchangeD_02 = new AssetReference("VO_Story_Minion_ChoGall_Male_Ogre_Story_Guldan_Mission8ExchangeD_02.prefab:89906b094648a0b44af8e4dd4dfc236b");

	private static readonly AssetReference VO_Story_Minion_ChoGall_Male_Ogre_Story_Guldan_Mission8Intro_02 = new AssetReference("VO_Story_Minion_ChoGall_Male_Ogre_Story_Guldan_Mission8Intro_02.prefab:2bcee366f59150e459ff47258a0ed973");

	private Dictionary<int, string[]> m_popUpInfo = new Dictionary<int, string[]> { 
	{
		228,
		new string[1] { "BOH_GULDAN_08" }
	} };

	private Player friendlySidePlayer;

	private Entity playerEntity;

	private float popUpScale = 1.25f;

	private Vector3 popUpPos;

	private Notification StartPopup;

	public static readonly AssetReference SargerasBrassRing = new AssetReference("Sargeras_Popup_BrassRing.prefab:df705ac0326836746af538133a79b587");

	private new List<string> m_BossIdleLines = new List<string> { VO_Story_Hero_TombGuardian_Male_Demon_Story_Guldan_Mission8Idle_01, VO_Story_Hero_TombGuardian_Male_Demon_Story_Guldan_Mission8Idle_01, VO_Story_Hero_TombGuardian_Male_Demon_Story_Guldan_Mission8Idle_01 };

	private List<string> m_EmoteResponseLines = new List<string> { VO_Story_Hero_TombGuardian_Male_Demon_Story_Guldan_Mission8EmoteResponse_01 };

	private HashSet<string> m_playedLines = new HashSet<string>();

	private static Map<GameEntityOption, bool> InitBooleanOptions()
	{
		return new Map<GameEntityOption, bool> { 
		{
			GameEntityOption.DO_OPENING_TAUNTS,
			false
		} };
	}

	public BoH_Guldan_08()
	{
		m_gameOptions.AddBooleanOptions(s_booleanOptions);
	}

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> m_SoundFiles = new List<string>
		{
			VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission8ExchangeA_01, VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission8ExchangeB_01, VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission8ExchangeC_02, VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission8ExchangeC_03, VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission8ExchangeD_01, VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission8Intro_01, VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission8Victory_02, VO_Story_Hero_Sargeras_Male_Demon_Story_Guldan_Mission8Victory_01, VO_Story_Hero_Sargeras_Male_Demon_Story_Guldan_Mission8Victory_03, VO_Story_Hero_Sargeras_Male_Demon_Story_Guldan_Mission8Victory_04,
			VO_Story_Hero_TombGuardian_Male_Demon_Story_Guldan_Mission8Death_01, VO_Story_Hero_TombGuardian_Male_Demon_Story_Guldan_Mission8EmoteResponse_01, VO_Story_Hero_TombGuardian_Male_Demon_Story_Guldan_Mission8Idle_01, VO_Story_Hero_TombGuardian_Male_Demon_Story_Guldan_Mission8Idle_02, VO_Story_Hero_TombGuardian_Male_Demon_Story_Guldan_Mission8Idle_03, VO_Story_Hero_TombGuardian_Male_Demon_Story_Guldan_Mission8Intro_03, VO_Story_Hero_TombGuardian_Male_Demon_Story_Guldan_Mission8Loss_01, VO_Story_Minion_ChoGall_Male_Ogre_Story_Guldan_Mission8ExchangeC_01, VO_Story_Minion_ChoGall_Male_Ogre_Story_Guldan_Mission8ExchangeD_02, VO_Story_Minion_ChoGall_Male_Ogre_Story_Guldan_Mission8Intro_02
		};
		SetBossVOLines(m_SoundFiles);
		foreach (string soundFile in m_SoundFiles)
		{
			PreloadSound(soundFile);
		}
	}

	private void Start()
	{
		friendlySidePlayer = GameState.Get().GetFriendlySidePlayer();
	}

	private void SetPopupPosition()
	{
		if (friendlySidePlayer.IsCurrentPlayer())
		{
			if ((bool)UniversalInputManager.UsePhoneUI)
			{
				popUpPos.z = -66f;
			}
			else
			{
				popUpPos.z = -44f;
			}
		}
		else if ((bool)UniversalInputManager.UsePhoneUI)
		{
			popUpPos.z = 66f;
		}
		else
		{
			popUpPos.z = 44f;
		}
	}

	protected override bool GetShouldSuppressDeathTextBubble()
	{
		return true;
	}

	public override bool ShouldPlayHeroBlowUpSpells(TAG_PLAYSTATE playState)
	{
		return playState != TAG_PLAYSTATE.WON;
	}

	public override IEnumerator DoActionsBeforeDealingBaseMulliganCards()
	{
		GameState.Get().GetOpposingSidePlayer().GetHero()
			.GetCard()
			.GetActor();
		Actor friendlyActor = GameState.Get().GetFriendlySidePlayer().GetHero()
			.GetCard()
			.GetActor();
		GameState.Get().SetBusy(busy: true);
		yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission8Intro_01);
		yield return MissionPlayVO(GetFriendlyActorByCardId("Story_09_ChoGall"), VO_Story_Minion_ChoGall_Male_Ogre_Story_Guldan_Mission8Intro_02);
		GameState.Get().SetBusy(busy: false);
	}

	public override List<string> GetBossIdleLines()
	{
		return m_BossIdleLines;
	}

	protected override void PlayEmoteResponse(EmoteType emoteType, CardSoundSpell emoteSpell)
	{
		Actor enemyActor = GameState.Get().GetOpposingSidePlayer().GetHeroCard()
			.GetActor();
		GameState.Get().GetFriendlySidePlayer().GetHeroCard()
			.GetActor();
		GameState.Get().GetFriendlySidePlayer().GetHero()
			.GetCardId();
		if (MissionEntity.STANDARD_EMOTE_RESPONSE_TRIGGERS.Contains(emoteType))
		{
			Gameplay.Get().StartCoroutine(PlaySoundAndBlockSpeech(m_standardEmoteResponseLine, Notification.SpeechBubbleDirection.TopRight, enemyActor));
		}
	}

	public override void OnCreateGame()
	{
		m_OverrideMusicTrack = MusicPlaylistType.InGame_DRGEVILBoss;
		base.OnCreateGame();
		m_standardEmoteResponseLine = VO_Story_Hero_TombGuardian_Male_Demon_Story_Guldan_Mission8EmoteResponse_01;
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
		case 507:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_TombGuardian_Male_Demon_Story_Guldan_Mission8Loss_01);
			GameState.Get().SetBusy(busy: false);
			break;
		case 101:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(SargerasBrassRing, VO_Story_Hero_Sargeras_Male_Demon_Story_Guldan_Mission8Victory_01);
			GameState.Get().SetBusy(busy: false);
			break;
		case 102:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(SargerasBrassRing, VO_Story_Hero_Sargeras_Male_Demon_Story_Guldan_Mission8Victory_04);
			GameState.Get().SetBusy(busy: false);
			break;
		case 108:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(GetFriendlyActorByCardId("Story_09_ChoGall"), VO_Story_Minion_ChoGall_Male_Ogre_Story_Guldan_Mission8ExchangeC_01);
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission8ExchangeC_02);
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission8ExchangeC_03);
			GameState.Get().SetBusy(busy: false);
			break;
		case 109:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(GetFriendlyActorByCardId("Story_09_ChoGallDormant"), VO_Story_Minion_ChoGall_Male_Ogre_Story_Guldan_Mission8ExchangeC_01);
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission8ExchangeC_02);
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission8ExchangeC_03);
			GameState.Get().SetBusy(busy: false);
			break;
		case 110:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission8ExchangeD_01);
			yield return MissionPlayVO(GetFriendlyActorByCardId("Story_09_ChoGall"), VO_Story_Minion_ChoGall_Male_Ogre_Story_Guldan_Mission8ExchangeD_02);
			GameState.Get().SetBusy(busy: false);
			break;
		case 111:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission8ExchangeD_01);
			yield return MissionPlayVO(GetFriendlyActorByCardId("Story_09_ChoGallDormant"), VO_Story_Minion_ChoGall_Male_Ogre_Story_Guldan_Mission8ExchangeD_02);
			GameState.Get().SetBusy(busy: false);
			break;
		case 504:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission8Victory_02);
			yield return MissionPlayVO(SargerasBrassRing, VO_Story_Hero_Sargeras_Male_Demon_Story_Guldan_Mission8Victory_03);
			GameState.Get().SetBusy(busy: false);
			break;
		case 228:
		{
			Notification popup = NotificationManager.Get().CreatePopupText(UserAttentionBlocker.NONE, popUpPos, TutorialEntity.GetTextScale() * popUpScale, GameStrings.Get(m_popUpInfo[missionEvent][0]), convertLegacyPosition: false, NotificationManager.PopupTextType.FANCY);
			yield return new WaitForSeconds(3.5f);
			NotificationManager.Get().DestroyNotification(popup, 0f);
			break;
		}
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
		GameState.Get().GetOpposingSidePlayer().GetHero()
			.GetCard()
			.GetActor();
		Actor friendlyActor = GameState.Get().GetFriendlySidePlayer().GetHero()
			.GetCard()
			.GetActor();
		switch (turn)
		{
		case 1:
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission8ExchangeA_01);
			break;
		case 5:
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission8ExchangeB_01);
			break;
		}
	}

	private IEnumerator ShowPopup(string displayString)
	{
		StartPopup = NotificationManager.Get().CreatePopupText(UserAttentionBlocker.NONE, popUpPos, TutorialEntity.GetTextScale(), GameStrings.Get(displayString), convertLegacyPosition: false);
		NotificationManager.Get().DestroyNotification(StartPopup, 7f);
		GameState.Get().SetBusy(busy: true);
		yield return new WaitForSeconds(2f);
		GameState.Get().SetBusy(busy: false);
	}
}
