using System.Collections;
using System.Collections.Generic;
using Blizzard.T5.Core;
using UnityEngine;

public class BoH_Guldan_02 : BoH_Guldan_Dungeon
{
	private static Map<GameEntityOption, bool> s_booleanOptions = InitBooleanOptions();

	private static readonly AssetReference VO_Story_Hero_ForgottenWarrior_Male_Orc_Story_Guldan_Mission1Death_01 = new AssetReference("VO_Story_Hero_ForgottenWarrior_Male_Orc_Story_Guldan_Mission1Death_01.prefab:be1a76fd913f0254a88d82fc1da47502");

	private static readonly AssetReference VO_Story_Hero_ForgottenWarrior_Male_Orc_Story_Guldan_Mission2ExchangeE_01 = new AssetReference("VO_Story_Hero_ForgottenWarrior_Male_Orc_Story_Guldan_Mission2ExchangeE_01.prefab:04df8cdaaac556f4e82c687e1925a274");

	private static readonly AssetReference VO_Story_Hero_ForgottenWarrior_Male_Orc_Story_Guldan_Mission1EmoteResponse_01 = new AssetReference("VO_Story_Hero_ForgottenWarrior_Male_Orc_Story_Guldan_Mission1EmoteResponse_01.prefab:0098404ee4a570f46a7b3d71f555bcbd");

	private static readonly AssetReference VO_Story_Hero_ForgottenWarrior_Male_Orc_Story_Guldan_Mission1HeroPower_01 = new AssetReference("VO_Story_Hero_ForgottenWarrior_Male_Orc_Story_Guldan_Mission1HeroPower_01.prefab:61877bc64a2a57d49b2b88f22bab865e");

	private static readonly AssetReference VO_Story_Hero_ForgottenWarrior_Male_Orc_Story_Guldan_Mission1HeroPower_02 = new AssetReference("VO_Story_Hero_ForgottenWarrior_Male_Orc_Story_Guldan_Mission1HeroPower_02.prefab:21fb7c0331137c34d803e755ea0c7784");

	private static readonly AssetReference VO_Story_Hero_ForgottenWarrior_Male_Orc_Story_Guldan_Mission1HeroPower_03 = new AssetReference("VO_Story_Hero_ForgottenWarrior_Male_Orc_Story_Guldan_Mission1HeroPower_03.prefab:6960caa2c8f2a214cb278fdb4dbce75e");

	private static readonly AssetReference VO_Story_Hero_ForgottenWarrior_Male_Orc_Story_Guldan_Mission1Idle_01 = new AssetReference("VO_Story_Hero_ForgottenWarrior_Male_Orc_Story_Guldan_Mission1Idle_01.prefab:8954269023fceef4fad1e84811e5902b");

	private static readonly AssetReference VO_Story_Hero_ForgottenWarrior_Male_Orc_Story_Guldan_Mission1Idle_02 = new AssetReference("VO_Story_Hero_ForgottenWarrior_Male_Orc_Story_Guldan_Mission1Idle_02.prefab:e0a2c83167c32d242b794f126d5d7c95");

	private static readonly AssetReference VO_Story_Hero_ForgottenWarrior_Male_Orc_Story_Guldan_Mission1Idle_03 = new AssetReference("VO_Story_Hero_ForgottenWarrior_Male_Orc_Story_Guldan_Mission1Idle_03.prefab:3b6145c4d0f70974681b2747a29f30da");

	private static readonly AssetReference VO_Story_Hero_ForgottenWarrior_Male_Orc_Story_Guldan_Mission1Loss_01 = new AssetReference("VO_Story_Hero_ForgottenWarrior_Male_Orc_Story_Guldan_Mission1Loss_01.prefab:4f4db36a4b8988943bbb62574d937809");

	private static readonly AssetReference VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission2ExchangeA_01 = new AssetReference("VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission2ExchangeA_01.prefab:3fadc4e0e82009d4ba6decbd17b97612");

	private static readonly AssetReference VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission2ExchangeB_01 = new AssetReference("VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission2ExchangeB_01.prefab:f65fa572184cdab4abf156d43faede36");

	private static readonly AssetReference VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission2ExchangeC_01 = new AssetReference("VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission2ExchangeC_01.prefab:25a710d6c0ff74e44ae26733145e9d89");

	private static readonly AssetReference VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission2ExchangeC_02 = new AssetReference("VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission2ExchangeC_02.prefab:c4829f967cb091944855aaaadfb70c75");

	private static readonly AssetReference VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission2ExchangeC_05 = new AssetReference("VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission2ExchangeC_05.prefab:f075c630f4400584fa2ac61cfd86dbbb");

	private static readonly AssetReference VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission2ExchangeD_01 = new AssetReference("VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission2ExchangeD_01.prefab:6eca325f66206fa418c56d6614f5838d");

	private static readonly AssetReference VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission2ExchangeE_02 = new AssetReference("VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission2ExchangeE_02.prefab:b22733bf465e0e04d8c9e4099af96392");

	private static readonly AssetReference VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission2Intro_01 = new AssetReference("VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission2Intro_01.prefab:73bea8c574e94ab4d91d4c11be55bde0");

	private static readonly AssetReference VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission2Victory_01 = new AssetReference("VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission2Victory_01.prefab:380da9432ed9e3946951ba2f1bb9be79");

	private static readonly AssetReference VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission2Victory_02 = new AssetReference("VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission2Victory_02.prefab:810bc7d1f7de1074db27eaa2ca806a9a");

	private static readonly AssetReference VO_Story_Hero_ThroneofElements_Elemental_Story_Guldan_Mission2Death = new AssetReference("VO_Story_Hero_ThroneofElements_Elemental_Story_Guldan_Mission2Death.prefab:acc43d4da2fd4b06b843afb12aa1affe");

	private static readonly AssetReference VO_Story_Hero_ThroneofElements_Elemental_Story_Guldan_Mission2EmoteResponse = new AssetReference("VO_Story_Hero_ThroneofElements_Elemental_Story_Guldan_Mission2EmoteResponse.prefab:e816cdeecbd348c5bed82a6bcb525ff0");

	private static readonly AssetReference VO_Story_Hero_ThroneofElements_Elemental_Story_Guldan_Mission2Intro_02 = new AssetReference("VO_Story_Hero_ThroneofElements_Elemental_Story_Guldan_Mission2Intro_02.prefab:be90053469f0486e9335f9d5d00b81d7");

	private static readonly AssetReference VO_Story_Hero_ThroneofElements_Elemental_Story_Guldan_Mission2Loss = new AssetReference("VO_Story_Hero_ThroneofElements_Elemental_Story_Guldan_Mission2Loss.prefab:b160e05444ca4e1880fea01b2dae8d8e");

	private static readonly AssetReference VO_Story_Minion_Kiljaeden_Male_Demon_Story_Guldan_Mission2ExchangeC_03 = new AssetReference("VO_Story_Minion_Kiljaeden_Male_Demon_Story_Guldan_Mission2ExchangeC_03.prefab:d7a3300c908dcd54bb92e17fba5d294c");

	private static readonly AssetReference VO_Story_Minion_Kiljaeden_Male_Demon_Story_Guldan_Mission2ExchangeC_04 = new AssetReference("VO_Story_Minion_Kiljaeden_Male_Demon_Story_Guldan_Mission2ExchangeC_04.prefab:16d1b53f5ca30ee49884466c4ae87af1");

	private static readonly AssetReference VO_Story_Minion_ForgottenShaman_Male_Orc_Death_02 = new AssetReference("VO_Story_Minion_ForgottenShaman_Male_Orc_Death_02.prefab:c8aaa63ac28bbb040865aae405a2c7d6");

	private static readonly AssetReference VO_Story_Minion_ForgottenShaman_Male_Orc_Death_01 = new AssetReference("VO_Story_Minion_ForgottenShaman_Male_Orc_Death_01.prefab:48307db3097f21046a8814a9b491a87f");

	private Dictionary<int, string[]> m_popUpInfo = new Dictionary<int, string[]> { 
	{
		228,
		new string[1] { "BOH_GULDAN_02b" }
	} };

	private Player friendlySidePlayer;

	private Entity playerEntity;

	private float popUpScale = 1.25f;

	private Vector3 popUpPos;

	private Notification StartPopup;

	public static readonly AssetReference KiljaedenBrassRing = new AssetReference("Kiljaeden_03_BrassRing_Quote.prefab:577ba278ff9f7a04eb0761c123fbe0ec");

	public static readonly AssetReference GuldanBrassRing = new AssetReference("Guldan_BrassRing_Quote.prefab:8c5b1c852b1322843b48cb70758958a9");

	private List<string> m_BossUsesHeroPowerLines = new List<string> { VO_Story_Hero_ForgottenWarrior_Male_Orc_Story_Guldan_Mission1HeroPower_01, VO_Story_Hero_ForgottenWarrior_Male_Orc_Story_Guldan_Mission1HeroPower_02, VO_Story_Hero_ForgottenWarrior_Male_Orc_Story_Guldan_Mission1HeroPower_03 };

	private new List<string> m_BossIdleLines = new List<string> { VO_Story_Hero_ForgottenWarrior_Male_Orc_Story_Guldan_Mission1Idle_01, VO_Story_Hero_ForgottenWarrior_Male_Orc_Story_Guldan_Mission1Idle_02, VO_Story_Hero_ForgottenWarrior_Male_Orc_Story_Guldan_Mission1Idle_03 };

	private new List<string> m_BossIdleLinesCopy = new List<string> { VO_Story_Hero_ForgottenWarrior_Male_Orc_Story_Guldan_Mission1Idle_01, VO_Story_Hero_ForgottenWarrior_Male_Orc_Story_Guldan_Mission1Idle_02, VO_Story_Hero_ForgottenWarrior_Male_Orc_Story_Guldan_Mission1Idle_03 };

	private List<string> m_EmoteResponseLines = new List<string> { VO_Story_Hero_ThroneofElements_Elemental_Story_Guldan_Mission2EmoteResponse };

	private HashSet<string> m_playedLines = new HashSet<string>();

	private Notification m_turnCounter;

	private MineCartRushArt m_mineCartArt;

	private static Map<GameEntityOption, bool> InitBooleanOptions()
	{
		return new Map<GameEntityOption, bool> { 
		{
			GameEntityOption.DO_OPENING_TAUNTS,
			false
		} };
	}

	public BoH_Guldan_02()
	{
		m_gameOptions.AddBooleanOptions(s_booleanOptions);
	}

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> m_SoundFiles = new List<string>
		{
			VO_Story_Hero_ForgottenWarrior_Male_Orc_Story_Guldan_Mission1Death_01, VO_Story_Hero_ForgottenWarrior_Male_Orc_Story_Guldan_Mission2ExchangeE_01, VO_Story_Hero_ForgottenWarrior_Male_Orc_Story_Guldan_Mission1EmoteResponse_01, VO_Story_Hero_ForgottenWarrior_Male_Orc_Story_Guldan_Mission1HeroPower_01, VO_Story_Hero_ForgottenWarrior_Male_Orc_Story_Guldan_Mission1HeroPower_02, VO_Story_Hero_ForgottenWarrior_Male_Orc_Story_Guldan_Mission1HeroPower_03, VO_Story_Hero_ForgottenWarrior_Male_Orc_Story_Guldan_Mission1Idle_01, VO_Story_Hero_ForgottenWarrior_Male_Orc_Story_Guldan_Mission1Idle_02, VO_Story_Hero_ForgottenWarrior_Male_Orc_Story_Guldan_Mission1Idle_03, VO_Story_Hero_ForgottenWarrior_Male_Orc_Story_Guldan_Mission1Loss_01,
			VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission2ExchangeA_01, VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission2ExchangeB_01, VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission2ExchangeC_01, VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission2ExchangeC_02, VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission2ExchangeC_05, VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission2ExchangeD_01, VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission2ExchangeE_02, VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission2Intro_01, VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission2Victory_01, VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission2Victory_02,
			VO_Story_Hero_ThroneofElements_Elemental_Story_Guldan_Mission2Death, VO_Story_Hero_ThroneofElements_Elemental_Story_Guldan_Mission2EmoteResponse, VO_Story_Hero_ThroneofElements_Elemental_Story_Guldan_Mission2Intro_02, VO_Story_Hero_ThroneofElements_Elemental_Story_Guldan_Mission2Loss, VO_Story_Minion_Kiljaeden_Male_Demon_Story_Guldan_Mission2ExchangeC_03, VO_Story_Minion_Kiljaeden_Male_Demon_Story_Guldan_Mission2ExchangeC_04, VO_Story_Minion_ForgottenShaman_Male_Orc_Death_02, VO_Story_Minion_ForgottenShaman_Male_Orc_Death_01
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
		Actor enemyActor = GameState.Get().GetOpposingSidePlayer().GetHero()
			.GetCard()
			.GetActor();
		Actor friendlyActor = GameState.Get().GetFriendlySidePlayer().GetHero()
			.GetCard()
			.GetActor();
		GameState.Get().SetBusy(busy: true);
		yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission2Intro_01);
		yield return MissionPlayVO(enemyActor, VO_Story_Hero_ThroneofElements_Elemental_Story_Guldan_Mission2Intro_02);
		GameState.Get().SetBusy(busy: false);
	}

	public override void OnCreateGame()
	{
		m_OverrideMusicTrack = MusicPlaylistType.InGame_BT;
		base.OnCreateGame();
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
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_ForgottenWarrior_Male_Orc_Story_Guldan_Mission1Loss_01);
			GameState.Get().SetBusy(busy: false);
			break;
		case 105:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_ThroneofElements_Elemental_Story_Guldan_Mission2Loss);
			GameState.Get().SetBusy(busy: false);
			break;
		case 106:
			yield return MissionPlayVOOnce(enemyActor, m_BossUsesHeroPowerLines);
			break;
		case 504:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission2Victory_02);
			GameState.Get().SetBusy(busy: false);
			break;
		case 101:
			GameState.Get().SetBusy(busy: true);
			yield return PlayLineAlways(friendlyActor, VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission2ExchangeC_01);
			yield return PlayLineAlways(friendlyActor, VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission2ExchangeC_02);
			GameState.Get().SetBusy(busy: false);
			break;
		case 108:
			GameState.Get().SetBusy(busy: true);
			yield return PlayLineAlways(KiljaedenBrassRing, VO_Story_Minion_Kiljaeden_Male_Demon_Story_Guldan_Mission2ExchangeC_03);
			yield return PlayLineAlways(KiljaedenBrassRing, VO_Story_Minion_Kiljaeden_Male_Demon_Story_Guldan_Mission2ExchangeC_04);
			yield return PlayLineAlways(friendlyActor, VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission2ExchangeC_05);
			GameState.Get().SetBusy(busy: false);
			break;
		case 102:
			GameState.Get().SetBusy(busy: true);
			yield return PlayLineAlways(GuldanBrassRing, VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission2ExchangeD_01);
			GameState.Get().SetBusy(busy: false);
			break;
		case 103:
			GameState.Get().SetBusy(busy: true);
			yield return PlayLineAlways(enemyActor, VO_Story_Hero_ForgottenWarrior_Male_Orc_Story_Guldan_Mission2ExchangeE_01);
			yield return PlayLineAlways(friendlyActor, VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission2ExchangeE_02);
			GameState.Get().SetBusy(busy: false);
			break;
		case 104:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission2Victory_01);
			GameState.Get().SetBusy(busy: false);
			break;
		case 107:
			yield return new WaitForSeconds(3f);
			break;
		case 228:
		{
			Notification popup = NotificationManager.Get().CreatePopupText(UserAttentionBlocker.NONE, popUpPos, TutorialEntity.GetTextScale() * popUpScale, GameStrings.Get(m_popUpInfo[missionEvent][0]), convertLegacyPosition: false, NotificationManager.PopupTextType.FANCY);
			yield return new WaitForSeconds(3.5f);
			NotificationManager.Get().DestroyNotification(popup, 0f);
			break;
		}
		case 515:
			if (GameState.Get().GetOpposingSidePlayer().GetHero()
				.GetCardId() == "Story_09_Throne_002hb")
			{
				yield return MissionPlayVO(enemyActor, VO_Story_Hero_ThroneofElements_Elemental_Story_Guldan_Mission2EmoteResponse);
			}
			else
			{
				yield return MissionPlayVO(enemyActor, VO_Story_Hero_ForgottenWarrior_Male_Orc_Story_Guldan_Mission1EmoteResponse_01);
			}
			break;
		default:
			yield return base.HandleMissionEventWithTiming(missionEvent);
			break;
		}
	}

	public override IEnumerator OnPlayThinkEmoteWithTiming()
	{
		if (m_enemySpeaking)
		{
			yield break;
		}
		Player currentPlayer = GameState.Get().GetCurrentPlayer();
		if (!currentPlayer.IsFriendlySide() || currentPlayer.GetHeroCard().HasActiveEmoteSound())
		{
			yield break;
		}
		string opposingHeroCard = GameState.Get().GetOpposingSidePlayer().GetHero()
			.GetCardId();
		float thinkEmoteBossIdleChancePercentage = GetThinkEmoteBossIdleChancePercentage();
		float randomThink = Random.Range(0f, 1f);
		if (thinkEmoteBossIdleChancePercentage > randomThink || (!m_Mission_FriendlyPlayIdleLines && m_Mission_EnemyPlayIdleLines))
		{
			Actor enemyActor = GameState.Get().GetOpposingSidePlayer().GetHeroCard()
				.GetActor();
			if (opposingHeroCard == "Story_09_Warrior_001hb")
			{
				string voLine = PopRandomLine(m_BossIdleLinesCopy);
				if (m_BossIdleLinesCopy.Count == 0)
				{
					m_BossIdleLinesCopy = new List<string>(m_BossIdleLines);
				}
				yield return MissionPlayVO(enemyActor, voLine);
			}
		}
		else if (m_Mission_FriendlyPlayIdleLines)
		{
			EmoteType thinkEmote = EmoteType.THINK1;
			switch (Random.Range(1, 4))
			{
			case 1:
				thinkEmote = EmoteType.THINK1;
				break;
			case 2:
				thinkEmote = EmoteType.THINK2;
				break;
			case 3:
				thinkEmote = EmoteType.THINK3;
				break;
			}
			GameState.Get().GetCurrentPlayer().GetHeroCard()
				.PlayEmote(thinkEmote)
				.GetActiveAudioSource();
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
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission2ExchangeA_01);
			break;
		case 7:
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission2ExchangeB_01);
			break;
		}
	}

	public override void NotifyOfMulliganEnded()
	{
		base.NotifyOfMulliganEnded();
		InitVisuals();
	}

	private void InitVisuals()
	{
		int cost = GetCost();
		InitTurnCounter(cost);
	}

	public override void OnTagChanged(TagDelta change)
	{
		base.OnTagChanged(change);
		if (change.tag == 48 && change.newValue != change.oldValue)
		{
			UpdateVisuals(change.newValue);
		}
	}

	private void InitTurnCounter(int cost)
	{
		if (cost >= 1)
		{
			GameObject turnCounterGo = AssetLoader.Get().InstantiatePrefab("LOE_Turn_Timer.prefab:b05530aa55868554fb8f0c66632b3c22");
			m_turnCounter = turnCounterGo.GetComponent<Notification>();
			PlayMakerFSM component = m_turnCounter.GetComponent<PlayMakerFSM>();
			component.FsmVariables.GetFsmBool("RunningMan").Value = true;
			component.FsmVariables.GetFsmBool("MineCart").Value = false;
			component.FsmVariables.GetFsmBool("Airship").Value = false;
			component.FsmVariables.GetFsmBool("Destroyer").Value = false;
			component.SendEvent("Birth");
			Actor enemyActor = GameState.Get().GetOpposingSidePlayer().GetHeroCard()
				.GetActor();
			m_turnCounter.transform.parent = enemyActor.gameObject.transform;
			m_turnCounter.transform.localPosition = new Vector3(-1.4f, 0.187f, -0.11f);
			m_turnCounter.transform.localScale = Vector3.one * 0.52f;
			UpdateTurnCounterText(cost);
		}
	}

	private void UpdateVisuals(int cost)
	{
		UpdateTurnCounter(cost);
	}

	private void UpdateMineCartArt()
	{
		Actor enemyActor = GameState.Get().GetOpposingSidePlayer().GetHeroCard()
			.GetActor();
		m_mineCartArt.DoPortraitSwap(enemyActor);
	}

	private void UpdateTurnCounter(int cost)
	{
		m_turnCounter.GetComponent<PlayMakerFSM>().SendEvent("Action");
		if (cost <= 0)
		{
			Object.Destroy(m_turnCounter.gameObject);
		}
		else
		{
			UpdateTurnCounterText(cost);
		}
	}

	private void UpdateTurnCounterText(int cost)
	{
		GameStrings.PluralNumber[] pluralNumbers = new GameStrings.PluralNumber[1]
		{
			new GameStrings.PluralNumber
			{
				m_index = 0,
				m_number = cost
			}
		};
		string counterName = GameStrings.FormatPlurals("BOH_GULDAN_02", pluralNumbers);
		m_turnCounter.ChangeDialogText(counterName, cost.ToString(), "", "");
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
