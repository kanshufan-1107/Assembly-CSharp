using System.Collections;
using System.Collections.Generic;
using Blizzard.T5.Core;
using UnityEngine;

public class BoH_Faelin_09B : BoH_Faelin_Dungeon
{
	private static Map<GameEntityOption, bool> s_booleanOptions = InitBooleanOptions();

	private static readonly AssetReference VO_Story_11_Errgl_009hb_Male_Murloc_Story_Faelin_Mission9bEmoteResponse_01 = new AssetReference("VO_Story_11_Errgl_009hb_Male_Murloc_Story_Faelin_Mission9bEmoteResponse_01.prefab:d0cc9002f456edd4984621f8fd62c3aa");

	private static readonly AssetReference VO_Story_11_Errgl_009hb_Male_Murloc_Story_Faelin_Mission9bExchangeB_01 = new AssetReference("VO_Story_11_Errgl_009hb_Male_Murloc_Story_Faelin_Mission9bExchangeB_01.prefab:27c283db388c0924bb70af23721c5b40");

	private static readonly AssetReference VO_Story_11_Errgl_009hb_Male_Murloc_Story_Faelin_Mission9bExchangeE_01 = new AssetReference("VO_Story_11_Errgl_009hb_Male_Murloc_Story_Faelin_Mission9bExchangeE_01.prefab:af02495f1bdc86548b4f6f66e8186bdc");

	private static readonly AssetReference VO_Story_11_Errgl_009hb_Male_Murloc_Story_Faelin_Mission9bHeroPower_01 = new AssetReference("VO_Story_11_Errgl_009hb_Male_Murloc_Story_Faelin_Mission9bHeroPower_01.prefab:6b37794f99c95d94f8fa852d4f349499");

	private static readonly AssetReference VO_Story_11_Errgl_009hb_Male_Murloc_Story_Faelin_Mission9bHeroPower_02 = new AssetReference("VO_Story_11_Errgl_009hb_Male_Murloc_Story_Faelin_Mission9bHeroPower_02.prefab:2b7da8286ffce5c439e48b0a2c156192");

	private static readonly AssetReference VO_Story_11_Errgl_009hb_Male_Murloc_Story_Faelin_Mission9bHeroPower_03 = new AssetReference("VO_Story_11_Errgl_009hb_Male_Murloc_Story_Faelin_Mission9bHeroPower_03.prefab:2dddf31470a9a48479ff90caa0777956");

	private static readonly AssetReference VO_Story_11_Errgl_009hb_Male_Murloc_Story_Faelin_Mission9bIdle_01 = new AssetReference("VO_Story_11_Errgl_009hb_Male_Murloc_Story_Faelin_Mission9bIdle_01.prefab:7e8c78086af97d048875048301f8b86d");

	private static readonly AssetReference VO_Story_11_Errgl_009hb_Male_Murloc_Story_Faelin_Mission9bIdle_02 = new AssetReference("VO_Story_11_Errgl_009hb_Male_Murloc_Story_Faelin_Mission9bIdle_02.prefab:6e059065f1784cb4a9132ddf3ebac318");

	private static readonly AssetReference VO_Story_11_Errgl_009hb_Male_Murloc_Story_Faelin_Mission9bIdle_03 = new AssetReference("VO_Story_11_Errgl_009hb_Male_Murloc_Story_Faelin_Mission9bIdle_03.prefab:63fb587bf3f60b54ba555dca2b3c4ae9");

	private static readonly AssetReference VO_Story_11_Errgl_009hb_Male_Murloc_Story_Faelin_Mission9bLoss_01 = new AssetReference("VO_Story_11_Errgl_009hb_Male_Murloc_Story_Faelin_Mission9bLoss_01.prefab:662bc710d322f3345b03e1191e754615");

	private static readonly AssetReference VO_Story_11_Errgl_009hb_Male_Murloc_Story_Faelin_Mission9bStart_01 = new AssetReference("VO_Story_11_Errgl_009hb_Male_Murloc_Story_Faelin_Mission9bStart_01.prefab:663cf5d6132485041b5e1f40b1074b74");

	private static readonly AssetReference VO_Story_11_Errgl_009hb_Male_Murloc_Story_Faelin_Mission9bStart_02 = new AssetReference("VO_Story_11_Errgl_009hb_Male_Murloc_Story_Faelin_Mission9bStart_02.prefab:bc637edb0e610d347817d9c20edf5033");

	private static readonly AssetReference VO_Story_11_Errgl_009hb_Male_Murloc_Story_Faelin_Mission9bVictory_01 = new AssetReference("VO_Story_11_Errgl_009hb_Male_Murloc_Story_Faelin_Mission9bVictory_01.prefab:a2eaf06c49b8453469baf1f42931ff43");

	private static readonly AssetReference VO_Story_11_Errgl_009hb_Male_Murloc_Story_Faelin_Mission9bVictory_02 = new AssetReference("VO_Story_11_Errgl_009hb_Male_Murloc_Story_Faelin_Mission9bVictory_02.prefab:3f18e42df50625042822da4c47134679");

	private static readonly AssetReference VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission9bExchangeA_01 = new AssetReference("VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission9bExchangeA_01.prefab:f2bf6c748f3dae443b74b98c240b3197");

	private static readonly AssetReference VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission9bExchangeD_01 = new AssetReference("VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission9bExchangeD_01.prefab:f6eaa8214079ef8459034f2f874f647c");

	private static readonly AssetReference VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission9bStart_01 = new AssetReference("VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission9bStart_01.prefab:5313c02a2c958d94b81931a7569a890d");

	private static readonly AssetReference VO_Story_11_Finley_009hp_Male_Murloc_Story_Faelin_Mission9b_Lorebook_2_01 = new AssetReference("VO_Story_11_Finley_009hp_Male_Murloc_Story_Faelin_Mission9b_Lorebook_2_01.prefab:ab4e9502b056f864a94a7cc22a066264");

	private static readonly AssetReference VO_Story_11_Finley_009hp_Male_Murloc_Story_Faelin_Mission9bExchangeA_01 = new AssetReference("VO_Story_11_Finley_009hp_Male_Murloc_Story_Faelin_Mission9bExchangeA_01.prefab:4913a3da66a07c94a874db088ba5c90d");

	private static readonly AssetReference VO_Story_11_Finley_009hp_Male_Murloc_Story_Faelin_Mission9bExchangeA_02 = new AssetReference("VO_Story_11_Finley_009hp_Male_Murloc_Story_Faelin_Mission9bExchangeA_02.prefab:de610d554e000b742b907cf07bf5e96e");

	private static readonly AssetReference VO_Story_11_Finley_009hp_Male_Murloc_Story_Faelin_Mission9bExchangeB_01 = new AssetReference("VO_Story_11_Finley_009hp_Male_Murloc_Story_Faelin_Mission9bExchangeB_01.prefab:68b8e7b5e38cda643ad0cfc911e667c8");

	private static readonly AssetReference VO_Story_11_Finley_009hp_Male_Murloc_Story_Faelin_Mission9bExchangeC_01 = new AssetReference("VO_Story_11_Finley_009hp_Male_Murloc_Story_Faelin_Mission9bExchangeC_01.prefab:df2b570732a10464a967c617b550a468");

	private static readonly AssetReference VO_Story_11_Finley_009hp_Male_Murloc_Story_Faelin_Mission9bExchangeD_01 = new AssetReference("VO_Story_11_Finley_009hp_Male_Murloc_Story_Faelin_Mission9bExchangeD_01.prefab:ed519b98543b63440a27e47ba6431e6a");

	private static readonly AssetReference VO_Story_11_Finley_009hp_Male_Murloc_Story_Faelin_Mission9bExchangeE_01 = new AssetReference("VO_Story_11_Finley_009hp_Male_Murloc_Story_Faelin_Mission9bExchangeE_01.prefab:a3a6ce0d639960d40ad8a88dd8363a2e");

	private static readonly AssetReference VO_Story_11_Finley_009hp_Male_Murloc_Story_Faelin_Mission9bStart_01 = new AssetReference("VO_Story_11_Finley_009hp_Male_Murloc_Story_Faelin_Mission9bStart_01.prefab:e7607d95c65466745af10e46aac29f21");

	private static readonly AssetReference VO_Story_11_Finley_009hp_Male_Murloc_Story_Faelin_Mission9bStart_02 = new AssetReference("VO_Story_11_Finley_009hp_Male_Murloc_Story_Faelin_Mission9bStart_02.prefab:d54213e086e10f7438f685986f9d8c37");

	private static readonly AssetReference VO_Story_11_Finley_009hp_Male_Murloc_Story_Faelin_Mission9bVictory_01 = new AssetReference("VO_Story_11_Finley_009hp_Male_Murloc_Story_Faelin_Mission9bVictory_01.prefab:aef2401011891e643b207ba71acd443a");

	private static readonly AssetReference VO_Story_11_Finley_009hp_Male_Murloc_Story_Faelin_Mission9bVictory_02 = new AssetReference("VO_Story_11_Finley_009hp_Male_Murloc_Story_Faelin_Mission9bVictory_02.prefab:ca2e1332a799f5d49a3315e2f431732c");

	private static readonly AssetReference VO_Story_11_Finley_009hp_Male_Murloc_Story_Faelin_PreMission9b_01 = new AssetReference("VO_Story_11_Finley_009hp_Male_Murloc_Story_Faelin_PreMission9b_01.prefab:d8c70b51c9664d249840784a633e3f5f");

	private static readonly AssetReference VO_Story_11_Finley_009hp_Male_Murloc_Story_Faelin_PreMission9b_02 = new AssetReference("VO_Story_11_Finley_009hp_Male_Murloc_Story_Faelin_PreMission9b_02.prefab:58aa15e984304a04aa21bbffadf25534");

	private static readonly AssetReference VO_Story_11_FinleysDad_Male_Murloc_Story_Faelin_Mission9b_Lorebook_1_01 = new AssetReference("VO_Story_11_FinleysDad_Male_Murloc_Story_Faelin_Mission9b_Lorebook_1_01.prefab:45e1275c9c4d4104095d96966e8d99d9");

	private static readonly AssetReference VO_Story_11_FinleysDad_Male_Murloc_Story_Faelin_Mission9bExchangeD_01 = new AssetReference("VO_Story_11_FinleysDad_Male_Murloc_Story_Faelin_Mission9bExchangeD_01.prefab:5006d15e6e614de409169206133fc696");

	private static readonly AssetReference VO_Story_11_FinleysDad_Male_Murloc_Story_Faelin_Mission9bVictory_01 = new AssetReference("VO_Story_11_FinleysDad_Male_Murloc_Story_Faelin_Mission9bVictory_01.prefab:fc8f9f73af026114696a3e85fbab718c");

	private static readonly AssetReference VO_Story_11_FinleysMum_Female_Murloc_Story_Faelin_Mission9bExchangeC_01 = new AssetReference("VO_Story_11_FinleysMum_Female_Murloc_Story_Faelin_Mission9bExchangeC_01.prefab:3fda5b45d2bd02c40ab5d0123d227704");

	private static readonly AssetReference VO_Story_11_FinleysMum_Female_Murloc_Story_Faelin_Mission9bExchangeC_02 = new AssetReference("VO_Story_11_FinleysMum_Female_Murloc_Story_Faelin_Mission9bExchangeC_02.prefab:dee029124916f5a4b86d2cc5b04e058e");

	private static readonly AssetReference VO_Story_11_FinleysMum_Female_Murloc_Story_Faelin_Mission9bVictory_01 = new AssetReference("VO_Story_11_FinleysMum_Female_Murloc_Story_Faelin_Mission9bVictory_01.prefab:250f4696a5a303245801acc2f7194046");

	private static readonly AssetReference VO_Story_11_FinleysMum_Female_Murloc_Story_Faelin_Mission9bVictory_02 = new AssetReference("VO_Story_11_FinleysMum_Female_Murloc_Story_Faelin_Mission9bVictory_02.prefab:24c1686299f555f4496ade09eb32a10f");

	private Notification m_popup;

	private Dictionary<int, string[]> m_popUpInfo = new Dictionary<int, string[]>
	{
		{
			228,
			new string[1] { "BOH_FAELIN_MISSION9b_LOREBOOK_01" }
		},
		{
			229,
			new string[1] { "BOH_FAELIN_MISSION9b_LOREBOOK_02" }
		}
	};

	private float popUpScale = 1.65f;

	private Vector3 popUpPos;

	public static readonly AssetReference IniBrassRing = new AssetReference("HS25-189_H_Ini_BrassRing_Quote.prefab:9f20435bdae99d24bae357f002abcf27");

	public static readonly AssetReference CayeBrassRing = new AssetReference("LC23-001_H_Caye_BrassRing_Quote.prefab:2364e8ce64f9c404fb569a86b17f3d01");

	public static readonly AssetReference BookBrassRing = new AssetReference("Wisdomball_Pop-up_BrassRing_Quote.prefab:896ee20514caff74db639aa7055838f6");

	public static readonly AssetReference FinleyBrassRing = new AssetReference("HS25-190_M_Finley_BrassRing_Quote.prefab:7123c129afe769e4aa9fb262a8f7c919");

	public static readonly AssetReference HalusBrassRing = new AssetReference("LC23-003_H_Halus_BrassRing_Quote.prefab:36e0aabb3ed4ce84599d9dd4e9ebdcf5");

	public static readonly AssetReference GraceBrassRing = new AssetReference("LC23-002_H_Grace_BrassRing_Quote.prefab:1f849ba2e303ff3449d34b96f960c96a");

	public static readonly AssetReference FaelinBrassRing = new AssetReference("SKN23-002_H_FAELIN_BrassRing_Quote.prefab:9984152d900140547aa7d721f3e49428");

	private new List<string> m_BossIdleLines = new List<string> { VO_Story_11_Errgl_009hb_Male_Murloc_Story_Faelin_Mission9bIdle_01, VO_Story_11_Errgl_009hb_Male_Murloc_Story_Faelin_Mission9bIdle_02, VO_Story_11_Errgl_009hb_Male_Murloc_Story_Faelin_Mission9bIdle_03 };

	private List<string> m_BossUsesHeroPowerLines = new List<string> { VO_Story_11_Errgl_009hb_Male_Murloc_Story_Faelin_Mission9bHeroPower_01, VO_Story_11_Errgl_009hb_Male_Murloc_Story_Faelin_Mission9bHeroPower_02, VO_Story_11_Errgl_009hb_Male_Murloc_Story_Faelin_Mission9bHeroPower_03 };

	private HashSet<string> m_playedLines = new HashSet<string>();

	private static Map<GameEntityOption, bool> InitBooleanOptions()
	{
		return new Map<GameEntityOption, bool> { 
		{
			GameEntityOption.DO_OPENING_TAUNTS,
			false
		} };
	}

	public BoH_Faelin_09B()
	{
		m_gameOptions.AddBooleanOptions(s_booleanOptions);
	}

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> m_SoundFiles = new List<string>
		{
			VO_Story_11_Finley_009hp_Male_Murloc_Story_Faelin_Mission9bStart_01, VO_Story_11_Errgl_009hb_Male_Murloc_Story_Faelin_Mission9bStart_01, VO_Story_11_Finley_009hp_Male_Murloc_Story_Faelin_Mission9bStart_02, VO_Story_11_Errgl_009hb_Male_Murloc_Story_Faelin_Mission9bStart_02, VO_Story_11_Finley_009hp_Male_Murloc_Story_Faelin_Mission9bExchangeA_01, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission9bExchangeA_01, VO_Story_11_Finley_009hp_Male_Murloc_Story_Faelin_Mission9bExchangeA_02, VO_Story_11_Finley_009hp_Male_Murloc_Story_Faelin_Mission9bExchangeB_01, VO_Story_11_Errgl_009hb_Male_Murloc_Story_Faelin_Mission9bExchangeB_01, VO_Story_11_FinleysMum_Female_Murloc_Story_Faelin_Mission9bExchangeC_01,
			VO_Story_11_Finley_009hp_Male_Murloc_Story_Faelin_Mission9bExchangeC_01, VO_Story_11_FinleysMum_Female_Murloc_Story_Faelin_Mission9bExchangeC_02, VO_Story_11_FinleysDad_Male_Murloc_Story_Faelin_Mission9bExchangeD_01, VO_Story_11_Finley_009hp_Male_Murloc_Story_Faelin_Mission9bExchangeD_01, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission9bExchangeD_01, VO_Story_11_Finley_009hp_Male_Murloc_Story_Faelin_Mission9bExchangeE_01, VO_Story_11_Errgl_009hb_Male_Murloc_Story_Faelin_Mission9bExchangeE_01, VO_Story_11_FinleysMum_Female_Murloc_Story_Faelin_Mission9bVictory_01, VO_Story_11_Errgl_009hb_Male_Murloc_Story_Faelin_Mission9bVictory_01, VO_Story_11_FinleysDad_Male_Murloc_Story_Faelin_Mission9bVictory_01,
			VO_Story_11_Errgl_009hb_Male_Murloc_Story_Faelin_Mission9bVictory_02, VO_Story_11_Finley_009hp_Male_Murloc_Story_Faelin_Mission9bVictory_01, VO_Story_11_FinleysMum_Female_Murloc_Story_Faelin_Mission9bVictory_02, VO_Story_11_Finley_009hp_Male_Murloc_Story_Faelin_Mission9bVictory_02, VO_Story_11_Errgl_009hb_Male_Murloc_Story_Faelin_Mission9bEmoteResponse_01, VO_Story_11_Errgl_009hb_Male_Murloc_Story_Faelin_Mission9bHeroPower_01, VO_Story_11_Errgl_009hb_Male_Murloc_Story_Faelin_Mission9bHeroPower_02, VO_Story_11_Errgl_009hb_Male_Murloc_Story_Faelin_Mission9bHeroPower_03, VO_Story_11_Errgl_009hb_Male_Murloc_Story_Faelin_Mission9bLoss_01, VO_Story_11_Errgl_009hb_Male_Murloc_Story_Faelin_Mission9bIdle_01,
			VO_Story_11_Errgl_009hb_Male_Murloc_Story_Faelin_Mission9bIdle_02, VO_Story_11_Errgl_009hb_Male_Murloc_Story_Faelin_Mission9bIdle_03, VO_Story_11_FinleysDad_Male_Murloc_Story_Faelin_Mission9b_Lorebook_1_01, VO_Story_11_Finley_009hp_Male_Murloc_Story_Faelin_Mission9b_Lorebook_2_01
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
		m_OverrideMusicTrack = MusicPlaylistType.InGame_LOOTFinalBoss;
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
			yield return MissionPlayVO(enemyActor, VO_Story_11_Errgl_009hb_Male_Murloc_Story_Faelin_Mission9bEmoteResponse_01);
			break;
		case 514:
			yield return MissionPlayVO(friendlyActor, VO_Story_11_Finley_009hp_Male_Murloc_Story_Faelin_Mission9bStart_01);
			yield return MissionPlayVO(enemyActor, VO_Story_11_Errgl_009hb_Male_Murloc_Story_Faelin_Mission9bStart_01);
			yield return MissionPlayVO(friendlyActor, VO_Story_11_Finley_009hp_Male_Murloc_Story_Faelin_Mission9bStart_02);
			yield return MissionPlayVO(enemyActor, VO_Story_11_Errgl_009hb_Male_Murloc_Story_Faelin_Mission9bStart_02);
			yield return MissionPlayVO(friendlyActor, VO_Story_11_Finley_009hp_Male_Murloc_Story_Faelin_Mission9bExchangeA_01);
			break;
		case 507:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(enemyActor, VO_Story_11_Errgl_009hb_Male_Murloc_Story_Faelin_Mission9bLoss_01);
			GameState.Get().SetBusy(busy: false);
			break;
		case 504:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(GetEnemyActorByCardId("Story_11_FinleysMum"), VO_Story_11_FinleysMum_Female_Murloc_Story_Faelin_Mission9bVictory_01);
			yield return MissionPlayVO(enemyActor, VO_Story_11_Errgl_009hb_Male_Murloc_Story_Faelin_Mission9bVictory_01);
			yield return MissionPlayVO(GetEnemyActorByCardId("Story_11_FinleysDad"), VO_Story_11_FinleysDad_Male_Murloc_Story_Faelin_Mission9bVictory_01);
			yield return MissionPlayVO(enemyActor, VO_Story_11_Errgl_009hb_Male_Murloc_Story_Faelin_Mission9bVictory_02);
			GameState.Get().SetBusy(busy: false);
			break;
		case 103:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(friendlyActor, VO_Story_11_Finley_009hp_Male_Murloc_Story_Faelin_Mission9bVictory_01);
			yield return MissionPlayVO(GetEnemyActorByCardId("Story_11_FinleysMum"), VO_Story_11_FinleysMum_Female_Murloc_Story_Faelin_Mission9bVictory_02);
			yield return MissionPlayVO(friendlyActor, VO_Story_11_Finley_009hp_Male_Murloc_Story_Faelin_Mission9bVictory_02);
			GameState.Get().SetBusy(busy: false);
			break;
		case 228:
			if (!(m_popup != null))
			{
				GameState.Get().SetBusy(busy: true);
				m_popup = NotificationManager.Get().CreatePopupText(UserAttentionBlocker.NONE, popUpPos, TutorialEntity.GetTextScale() * popUpScale, GameStrings.Get(m_popUpInfo[missionEvent][0]), convertLegacyPosition: false, NotificationManager.PopupTextType.FANCY);
				yield return GameEntity.Coroutines.StartCoroutine(PlaySoundAndBlockSpeech(VO_Story_11_FinleysDad_Male_Murloc_Story_Faelin_Mission9b_Lorebook_1_01, Notification.SpeechBubbleDirection.None, GetEnemyActorByCardId("Story_11_Mission1_Lorebook1"), 2.5f));
				NotificationManager.Get().DestroyNotification(m_popup, 0f);
				m_popup = null;
				GameState.Get().SetBusy(busy: false);
			}
			break;
		case 229:
			if (!(m_popup != null))
			{
				GameState.Get().SetBusy(busy: true);
				m_popup = NotificationManager.Get().CreatePopupText(UserAttentionBlocker.NONE, popUpPos, TutorialEntity.GetTextScale() * popUpScale, GameStrings.Get(m_popUpInfo[missionEvent][0]), convertLegacyPosition: false, NotificationManager.PopupTextType.FANCY);
				yield return GameEntity.Coroutines.StartCoroutine(PlaySoundAndBlockSpeech(VO_Story_11_Finley_009hp_Male_Murloc_Story_Faelin_Mission9b_Lorebook_2_01, Notification.SpeechBubbleDirection.None, GetEnemyActorByCardId("Story_11_Mission1_Lorebook1"), 2.5f));
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
		if (m_playedLines.Contains(entity.GetCardId()) && entity.GetCardType() != TAG_CARDTYPE.HERO_POWER)
		{
			yield break;
		}
		yield return base.RespondToPlayedCardWithTiming(entity);
		yield return WaitForEntitySoundToFinish(entity);
		string cardID = entity.GetCardId();
		m_playedLines.Add(cardID);
		GameState.Get().GetOpposingSidePlayer().GetHero()
			.GetCard()
			.GetActor();
		Actor friendlyActor = GameState.Get().GetFriendlySidePlayer().GetHero()
			.GetCard()
			.GetActor();
		if (!(cardID == "Story_11_FinleysMum"))
		{
			if (cardID == "Story_11_FinleysDad")
			{
				Actor m_FinleysDad = GetEnemyActorByCardId("Story_11_FinleysDad");
				if (m_FinleysDad != null)
				{
					yield return PlayLineAlways(m_FinleysDad, VO_Story_11_FinleysDad_Male_Murloc_Story_Faelin_Mission9bExchangeD_01);
				}
				yield return PlayLineAlways(friendlyActor, VO_Story_11_Finley_009hp_Male_Murloc_Story_Faelin_Mission9bExchangeD_01);
				yield return PlayLineAlways(FaelinBrassRing, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission9bExchangeD_01);
			}
		}
		else
		{
			Actor m_FinleysMum = GetEnemyActorByCardId("Story_11_FinleysMum");
			if (m_FinleysMum != null)
			{
				yield return PlayLineAlways(m_FinleysMum, VO_Story_11_FinleysMum_Female_Murloc_Story_Faelin_Mission9bExchangeC_01);
			}
			yield return PlayLineAlways(friendlyActor, VO_Story_11_Finley_009hp_Male_Murloc_Story_Faelin_Mission9bExchangeC_01);
			yield return PlayLineAlways(m_FinleysMum, VO_Story_11_FinleysMum_Female_Murloc_Story_Faelin_Mission9bExchangeC_02);
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
			yield return MissionPlayVO(FaelinBrassRing, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission9bExchangeA_01);
			yield return MissionPlayVO(friendlyActor, VO_Story_11_Finley_009hp_Male_Murloc_Story_Faelin_Mission9bExchangeA_02);
			break;
		case 3:
			yield return MissionPlayVO(friendlyActor, VO_Story_11_Finley_009hp_Male_Murloc_Story_Faelin_Mission9bExchangeB_01);
			yield return MissionPlayVO(enemyActor, VO_Story_11_Errgl_009hb_Male_Murloc_Story_Faelin_Mission9bExchangeB_01);
			break;
		case 19:
			yield return MissionPlayVO(friendlyActor, VO_Story_11_Finley_009hp_Male_Murloc_Story_Faelin_Mission9bExchangeE_01);
			yield return MissionPlayVO(enemyActor, VO_Story_11_Errgl_009hb_Male_Murloc_Story_Faelin_Mission9bExchangeE_01);
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
			if (clickedEntity.GetCardId() == "Story_11_Mission9b_Lorebook1")
			{
				if (m_popup == null)
				{
					Network.Get().SendClientScriptGameEvent(ClientScriptGameEventType.MISSION_EVENT, 228);
					HandleMissionEvent(228);
				}
				return false;
			}
			if (clickedEntity.GetCardId() == "Story_11_Mission9b_Lorebook2")
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
