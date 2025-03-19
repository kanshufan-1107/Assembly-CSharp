using System.Collections;
using System.Collections.Generic;
using Blizzard.T5.Core;
using UnityEngine;

public class BoH_Faelin_05B : BoH_Faelin_Dungeon
{
	private static Map<GameEntityOption, bool> s_booleanOptions = InitBooleanOptions();

	private static readonly AssetReference VO_Story_11_Caye_012hp_Female_Nightborne_Story_Faelin_Mission5bExchangeE_01 = new AssetReference("VO_Story_11_Caye_012hp_Female_Nightborne_Story_Faelin_Mission5bExchangeE_01.prefab:a192ad6cbab2504469d66b42ff4a31ab");

	private static readonly AssetReference VO_Story_11_Caye_012hp_Female_Nightborne_Story_Faelin_PreMission5b_01 = new AssetReference("VO_Story_11_Caye_012hp_Female_Nightborne_Story_Faelin_PreMission5b_01.prefab:693d0b6ec102af8468fb3bacfa16fd76");

	private static readonly AssetReference VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission5bExchangeC_01 = new AssetReference("VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission5bExchangeC_01.prefab:f0de328a1f74a344387ea132088e6443");

	private static readonly AssetReference VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission5bExchangeC_02 = new AssetReference("VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission5bExchangeC_02.prefab:a87a995c7f7d410439c9eee82bddeea2");

	private static readonly AssetReference VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission5bVictory_01 = new AssetReference("VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission5bVictory_01.prefab:14f3f62092885a947b2083d249b38df0");

	private static readonly AssetReference VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_PreMission5b_01 = new AssetReference("VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_PreMission5b_01.prefab:4831405789177c34596602c6117c8cfa");

	private static readonly AssetReference VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission5b_Lorebook_1_01 = new AssetReference("VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission5b_Lorebook_1_01.prefab:090bed5da25e790419c35f26c4eb98fd");

	private static readonly AssetReference VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission5b_Lorebook_2_01 = new AssetReference("VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission5b_Lorebook_2_01.prefab:b3cd610c80e9fe942bed2f151285bf5a");

	private static readonly AssetReference VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission5bExchangeA_01 = new AssetReference("VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission5bExchangeA_01.prefab:9ee6b77036273e94f9b74d67db74d3e1");

	private static readonly AssetReference VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission5bExchangeB_01 = new AssetReference("VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission5bExchangeB_01.prefab:b8d7008175b76de47965e80f9009d68a");

	private static readonly AssetReference VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission5bExchangeB_02 = new AssetReference("VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission5bExchangeB_02.prefab:37cef26df8374d7428ae1bef7e15df11");

	private static readonly AssetReference VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission5bExchangeC_01 = new AssetReference("VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission5bExchangeC_01.prefab:346b07c50876dee4da163195e4c21480");

	private static readonly AssetReference VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission5bExchangeD_01 = new AssetReference("VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission5bExchangeD_01.prefab:6d7ba97ac49bce1498d63e0af3b22e13");

	private static readonly AssetReference VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission5bStart_01 = new AssetReference("VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission5bStart_01.prefab:d0868ab303602d14aa69d7585b7a7726");

	private static readonly AssetReference VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission5bVictory_01 = new AssetReference("VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission5bVictory_01.prefab:d37974c1a454822449e878954097df14");

	private static readonly AssetReference VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission5bVictory_02 = new AssetReference("VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission5bVictory_02.prefab:42df94ba267f5c34a80db1d9b6304171");

	private static readonly AssetReference VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_PreMission5b_01 = new AssetReference("VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_PreMission5b_01.prefab:43a1ab1100116e64082ef7f158cb28b1");

	private static readonly AssetReference VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission5bExchangeA_01 = new AssetReference("VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission5bExchangeA_01.prefab:94f08840091ae1048885c671ab0db417");

	private static readonly AssetReference VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission5bExchangeA_02 = new AssetReference("VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission5bExchangeA_02.prefab:273c562cfc88b7e439fb15379417847b");

	private static readonly AssetReference VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission5bExchangeE_01 = new AssetReference("VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission5bExchangeE_01.prefab:d2018f5d256106e4eac610566300292f");

	private static readonly AssetReference VO_Story_11_Mission5b_Lorebook1_Male_Human_Story_Faelin_Mission5b_Lorebook_1_01 = new AssetReference("VO_Story_11_Mission5b_Lorebook1_Male_Human_Story_Faelin_Mission5b_Lorebook_1_01.prefab:7cf4a730ce0711c4b881e75a4c983fb2");

	private static readonly AssetReference VO_Story_11_Nemesis_005hb_Male_Human_Story_Faelin_Mission5bEmoteResponse_01 = new AssetReference("VO_Story_11_Nemesis_005hb_Male_Human_Story_Faelin_Mission5bEmoteResponse_01.prefab:22db121cf1400c14d901d74b113b3f19");

	private static readonly AssetReference VO_Story_11_Nemesis_005hb_Male_Human_Story_Faelin_Mission5bExchangeA_01 = new AssetReference("VO_Story_11_Nemesis_005hb_Male_Human_Story_Faelin_Mission5bExchangeA_01.prefab:98a2dd58da513d04aa5db22830a2fc30");

	private static readonly AssetReference VO_Story_11_Nemesis_005hb_Male_Human_Story_Faelin_Mission5bExchangeB_01 = new AssetReference("VO_Story_11_Nemesis_005hb_Male_Human_Story_Faelin_Mission5bExchangeB_01.prefab:249a023bf35935449a9494b8c4fafe9f");

	private static readonly AssetReference VO_Story_11_Nemesis_005hb_Male_Human_Story_Faelin_Mission5bExchangeC_01 = new AssetReference("VO_Story_11_Nemesis_005hb_Male_Human_Story_Faelin_Mission5bExchangeC_01.prefab:674f49bfa970f384c85495077f55cd0b");

	private static readonly AssetReference VO_Story_11_Nemesis_005hb_Male_Human_Story_Faelin_Mission5bExchangeC_02 = new AssetReference("VO_Story_11_Nemesis_005hb_Male_Human_Story_Faelin_Mission5bExchangeC_02.prefab:9d68f2698608d184690809ff15405223");

	private static readonly AssetReference VO_Story_11_Nemesis_005hb_Male_Human_Story_Faelin_Mission5bHeroPower_01 = new AssetReference("VO_Story_11_Nemesis_005hb_Male_Human_Story_Faelin_Mission5bHeroPower_01.prefab:3fe960524c657b947999faacfe59a58c");

	private static readonly AssetReference VO_Story_11_Nemesis_005hb_Male_Human_Story_Faelin_Mission5bHeroPower_02 = new AssetReference("VO_Story_11_Nemesis_005hb_Male_Human_Story_Faelin_Mission5bHeroPower_02.prefab:6417a5f5a3c93b44183310e45926b67a");

	private static readonly AssetReference VO_Story_11_Nemesis_005hb_Male_Human_Story_Faelin_Mission5bHeroPower_03 = new AssetReference("VO_Story_11_Nemesis_005hb_Male_Human_Story_Faelin_Mission5bHeroPower_03.prefab:c3b6dbc298a56b94cad974b61344dc69");

	private static readonly AssetReference VO_Story_11_Nemesis_005hb_Male_Human_Story_Faelin_Mission5bIdle_01 = new AssetReference("VO_Story_11_Nemesis_005hb_Male_Human_Story_Faelin_Mission5bIdle_01.prefab:dcf9abad2f06617468c8e59be5b684ee");

	private static readonly AssetReference VO_Story_11_Nemesis_005hb_Male_Human_Story_Faelin_Mission5bIdle_02 = new AssetReference("VO_Story_11_Nemesis_005hb_Male_Human_Story_Faelin_Mission5bIdle_02.prefab:2c3191219d8d09043858ac24e2d97b85");

	private static readonly AssetReference VO_Story_11_Nemesis_005hb_Male_Human_Story_Faelin_Mission5bIdle_03 = new AssetReference("VO_Story_11_Nemesis_005hb_Male_Human_Story_Faelin_Mission5bIdle_03.prefab:3255ed18aaf4d01439b8193a822be4ce");

	private static readonly AssetReference VO_Story_11_Nemesis_005hb_Male_Human_Story_Faelin_Mission5bLoss_01 = new AssetReference("VO_Story_11_Nemesis_005hb_Male_Human_Story_Faelin_Mission5bLoss_01.prefab:ae018a5197d21934a81e1408d0acc6ed");

	private static readonly AssetReference VO_Story_11_Nemesis_005hb_Male_Human_Story_Faelin_Mission5bStart_01 = new AssetReference("VO_Story_11_Nemesis_005hb_Male_Human_Story_Faelin_Mission5bStart_01.prefab:35789e9566b7c03428b840a556a250c5");

	private static readonly AssetReference VO_Story_11_Nemesis_005hb_Male_Human_Story_Faelin_Mission5bVictory_01 = new AssetReference("VO_Story_11_Nemesis_005hb_Male_Human_Story_Faelin_Mission5bVictory_01.prefab:cd01ccda0b2655b4283a7c6213477b30");

	private static readonly AssetReference VO_Story_11_Nemesis_005hb_Male_Human_Story_Faelin_Mission5bVictory_02 = new AssetReference("VO_Story_11_Nemesis_005hb_Male_Human_Story_Faelin_Mission5bVictory_02.prefab:458ba7e54de90c64facedab3d85de477");

	private static readonly AssetReference VO_Story_11_Nemesis_005hb_Male_Human_Story_Faelin_Mission5bVictory_03 = new AssetReference("VO_Story_11_Nemesis_005hb_Male_Human_Story_Faelin_Mission5bVictory_03.prefab:9a33cfcc810af5d4095468f11819b63a");

	private Notification m_popup;

	private Dictionary<int, string[]> m_popUpInfo = new Dictionary<int, string[]>
	{
		{
			228,
			new string[1] { "BOH_FAELIN_MISSION5B_LOREBOOK_01" }
		},
		{
			229,
			new string[1] { "BOH_FAELIN_MISSION5B_LOREBOOK_02" }
		},
		{
			230,
			new string[1] { "BOH_FAELIN_MISSION5B_LOREBOOK_03" }
		}
	};

	private float popUpScale = 1.65f;

	private Vector3 popUpPos;

	public static readonly AssetReference IniBrassRing = new AssetReference("HS25-189_H_Ini_BrassRing_Quote.prefab:9f20435bdae99d24bae357f002abcf27");

	public static readonly AssetReference CayeBrassRing = new AssetReference("LC23-001_H_Caye_BrassRing_Quote.prefab:2364e8ce64f9c404fb569a86b17f3d01");

	public static readonly AssetReference BookBrassRing = new AssetReference("Wisdomball_Pop-up_BrassRing_Quote.prefab:896ee20514caff74db639aa7055838f6");

	public static readonly AssetReference GraceBrassRing = new AssetReference("LC23-002_H_Grace_BrassRing_Quote.prefab:1f849ba2e303ff3449d34b96f960c96a");

	public static readonly AssetReference FaelinBrassRing = new AssetReference("SKN23-002_H_FAELIN_BrassRing_Quote.prefab:9984152d900140547aa7d721f3e49428");

	private new List<string> m_BossIdleLines = new List<string> { VO_Story_11_Nemesis_005hb_Male_Human_Story_Faelin_Mission5bIdle_01, VO_Story_11_Nemesis_005hb_Male_Human_Story_Faelin_Mission5bIdle_02, VO_Story_11_Nemesis_005hb_Male_Human_Story_Faelin_Mission5bIdle_03 };

	private List<string> m_BossUsesHeroPowerLines = new List<string> { VO_Story_11_Nemesis_005hb_Male_Human_Story_Faelin_Mission5bHeroPower_01, VO_Story_11_Nemesis_005hb_Male_Human_Story_Faelin_Mission5bHeroPower_02, VO_Story_11_Nemesis_005hb_Male_Human_Story_Faelin_Mission5bHeroPower_03 };

	private HashSet<string> m_playedLines = new HashSet<string>();

	private static Map<GameEntityOption, bool> InitBooleanOptions()
	{
		return new Map<GameEntityOption, bool> { 
		{
			GameEntityOption.DO_OPENING_TAUNTS,
			false
		} };
	}

	public BoH_Faelin_05B()
	{
		m_gameOptions.AddBooleanOptions(s_booleanOptions);
	}

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> m_SoundFiles = new List<string>
		{
			VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission5bExchangeA_01, VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission5bStart_01, VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission5bExchangeA_02, VO_Story_11_Nemesis_005hb_Male_Human_Story_Faelin_Mission5bStart_01, VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission5bExchangeA_01, VO_Story_11_Nemesis_005hb_Male_Human_Story_Faelin_Mission5bExchangeA_01, VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission5bExchangeB_01, VO_Story_11_Nemesis_005hb_Male_Human_Story_Faelin_Mission5bExchangeB_01, VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission5bExchangeB_02, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission5bExchangeC_01,
			VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission5bExchangeC_01, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission5bExchangeC_02, VO_Story_11_Nemesis_005hb_Male_Human_Story_Faelin_Mission5bExchangeC_01, VO_Story_11_Nemesis_005hb_Male_Human_Story_Faelin_Mission5bExchangeC_02, VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission5bExchangeD_01, VO_Story_11_Caye_012hp_Female_Nightborne_Story_Faelin_Mission5bExchangeE_01, VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission5bExchangeE_01, VO_Story_11_Nemesis_005hb_Male_Human_Story_Faelin_Mission5bVictory_01, VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission5bVictory_01, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission5bVictory_01,
			VO_Story_11_Nemesis_005hb_Male_Human_Story_Faelin_Mission5bVictory_02, VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission5bVictory_02, VO_Story_11_Nemesis_005hb_Male_Human_Story_Faelin_Mission5bVictory_03, VO_Story_11_Nemesis_005hb_Male_Human_Story_Faelin_Mission5bEmoteResponse_01, VO_Story_11_Nemesis_005hb_Male_Human_Story_Faelin_Mission5bHeroPower_01, VO_Story_11_Nemesis_005hb_Male_Human_Story_Faelin_Mission5bHeroPower_02, VO_Story_11_Nemesis_005hb_Male_Human_Story_Faelin_Mission5bHeroPower_03, VO_Story_11_Nemesis_005hb_Male_Human_Story_Faelin_Mission5bLoss_01, VO_Story_11_Nemesis_005hb_Male_Human_Story_Faelin_Mission5bIdle_01, VO_Story_11_Nemesis_005hb_Male_Human_Story_Faelin_Mission5bIdle_02,
			VO_Story_11_Nemesis_005hb_Male_Human_Story_Faelin_Mission5bIdle_03, VO_Story_11_Mission5b_Lorebook1_Male_Human_Story_Faelin_Mission5b_Lorebook_1_01, VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission5b_Lorebook_1_01, VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission5b_Lorebook_2_01
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

	public override void OnCreateGame()
	{
		base.OnCreateGame();
		m_OverrideMusicTrack = MusicPlaylistType.InGame_GILFinalBoss;
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
			yield return MissionPlayVO(enemyActor, VO_Story_11_Nemesis_005hb_Male_Human_Story_Faelin_Mission5bEmoteResponse_01);
			break;
		case 514:
			yield return MissionPlayVO(IniBrassRing, VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission5bExchangeA_01);
			yield return MissionPlayVO(friendlyActor, VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission5bStart_01);
			yield return MissionPlayVO(IniBrassRing, VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission5bExchangeA_02);
			break;
		case 507:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(enemyActor, VO_Story_11_Nemesis_005hb_Male_Human_Story_Faelin_Mission5bLoss_01);
			GameState.Get().SetBusy(busy: false);
			break;
		case 504:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(enemyActor, VO_Story_11_Nemesis_005hb_Male_Human_Story_Faelin_Mission5bVictory_01);
			yield return MissionPlayVO(friendlyActor, VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission5bVictory_01);
			yield return MissionPlayVO(FaelinBrassRing, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission5bVictory_01);
			yield return MissionPlayVO(enemyActor, VO_Story_11_Nemesis_005hb_Male_Human_Story_Faelin_Mission5bVictory_02);
			GameState.Get().SetBusy(busy: false);
			break;
		case 101:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(friendlyActor, VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission5bVictory_02);
			yield return MissionPlayVO(enemyActor, VO_Story_11_Nemesis_005hb_Male_Human_Story_Faelin_Mission5bVictory_03);
			GameState.Get().SetBusy(busy: false);
			break;
		case 228:
			if (!(m_popup != null))
			{
				GameState.Get().SetBusy(busy: true);
				m_popup = NotificationManager.Get().CreatePopupText(UserAttentionBlocker.NONE, popUpPos, TutorialEntity.GetTextScale() * popUpScale, GameStrings.Get(m_popUpInfo[missionEvent][0]), convertLegacyPosition: false, NotificationManager.PopupTextType.FANCY);
				yield return GameEntity.Coroutines.StartCoroutine(PlaySoundAndBlockSpeech(VO_Story_11_Mission5b_Lorebook1_Male_Human_Story_Faelin_Mission5b_Lorebook_1_01, Notification.SpeechBubbleDirection.None, GetEnemyActorByCardId("Story_11_Mission1_Lorebook1"), 2.5f));
				NotificationManager.Get().DestroyNotification(m_popup, 0f);
				m_popup = null;
				GameState.Get().SetBusy(busy: false);
				yield return MissionPlayVO(friendlyActor, VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission5b_Lorebook_1_01);
			}
			break;
		case 229:
			if (!(m_popup != null))
			{
				GameState.Get().SetBusy(busy: true);
				m_popup = NotificationManager.Get().CreatePopupText(UserAttentionBlocker.NONE, popUpPos, TutorialEntity.GetTextScale() * popUpScale, GameStrings.Get(m_popUpInfo[missionEvent][0]), convertLegacyPosition: false, NotificationManager.PopupTextType.FANCY);
				yield return GameEntity.Coroutines.StartCoroutine(PlaySoundAndBlockSpeech(VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission5b_Lorebook_2_01, Notification.SpeechBubbleDirection.None, GetEnemyActorByCardId("Story_11_Mission1_Lorebook1"), 2.5f));
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
		case 1:
			yield return MissionPlayVO(enemyActor, VO_Story_11_Nemesis_005hb_Male_Human_Story_Faelin_Mission5bStart_01);
			break;
		case 3:
			yield return MissionPlayVO(friendlyActor, VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission5bExchangeA_01);
			yield return MissionPlayVO(enemyActor, VO_Story_11_Nemesis_005hb_Male_Human_Story_Faelin_Mission5bExchangeA_01);
			break;
		case 5:
			yield return MissionPlayVO(friendlyActor, VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission5bExchangeB_01);
			yield return MissionPlayVO(enemyActor, VO_Story_11_Nemesis_005hb_Male_Human_Story_Faelin_Mission5bExchangeB_01);
			yield return MissionPlayVO(friendlyActor, VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission5bExchangeB_02);
			break;
		case 7:
			yield return MissionPlayVO(FaelinBrassRing, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission5bExchangeC_01);
			yield return MissionPlayVO(friendlyActor, VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission5bExchangeC_01);
			yield return MissionPlayVO(FaelinBrassRing, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission5bExchangeC_02);
			break;
		case 13:
			yield return MissionPlayVO(enemyActor, VO_Story_11_Nemesis_005hb_Male_Human_Story_Faelin_Mission5bExchangeC_01);
			yield return MissionPlayVO(enemyActor, VO_Story_11_Nemesis_005hb_Male_Human_Story_Faelin_Mission5bExchangeC_02);
			yield return MissionPlayVO(friendlyActor, VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission5bExchangeD_01);
			break;
		case 9:
			yield return MissionPlayVO(CayeBrassRing, VO_Story_11_Caye_012hp_Female_Nightborne_Story_Faelin_Mission5bExchangeE_01);
			yield return MissionPlayVO(IniBrassRing, VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission5bExchangeE_01);
			break;
		}
	}

	public override bool NotifyOfBattlefieldCardClicked(Entity clickedEntity, bool wasInTargetMode)
	{
		if (!base.NotifyOfBattlefieldCardClicked(clickedEntity, wasInTargetMode))
		{
			return false;
		}
		if (!wasInTargetMode)
		{
			if (clickedEntity.GetCardId() == "Story_11_Mission5b_Lorebook1")
			{
				if (m_popup == null)
				{
					Network.Get().SendClientScriptGameEvent(ClientScriptGameEventType.MISSION_EVENT, 228);
					HandleMissionEvent(228);
				}
				return false;
			}
			if (clickedEntity.GetCardId() == "Story_11_Mission5b_Lorebook2")
			{
				if (m_popup == null)
				{
					Network.Get().SendClientScriptGameEvent(ClientScriptGameEventType.MISSION_EVENT, 229);
					HandleMissionEvent(229);
				}
				return false;
			}
		}
		return true;
	}
}
