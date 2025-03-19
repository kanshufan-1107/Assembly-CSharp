using System.Collections;
using System.Collections.Generic;
using Blizzard.T5.Core;
using UnityEngine;

public class BoH_Faelin_09A : BoH_Faelin_Dungeon
{
	private static Map<GameEntityOption, bool> s_booleanOptions = InitBooleanOptions();

	private static readonly AssetReference VO_Story_11_Caye_012hp_Female_Nightborne_Story_Faelin_Mission9aExchangeD_01 = new AssetReference("VO_Story_11_Caye_012hp_Female_Nightborne_Story_Faelin_Mission9aExchangeD_01.prefab:039434708a3def24f8d114c041b20395");

	private static readonly AssetReference VO_Story_11_Caye_012hp_Female_Nightborne_Story_Faelin_Mission9aExchangeE_01 = new AssetReference("VO_Story_11_Caye_012hp_Female_Nightborne_Story_Faelin_Mission9aExchangeE_01.prefab:8ae9503f19415fb4d87e8b301d36fcbb");

	private static readonly AssetReference VO_Story_11_Caye_012hp_Female_Nightborne_Story_Faelin_PreMission9a_01 = new AssetReference("VO_Story_11_Caye_012hp_Female_Nightborne_Story_Faelin_PreMission9a_01.prefab:bf47ee3ae9f708c4b9ad086c9dec5f52");

	private static readonly AssetReference VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission9aExchangeAa_01 = new AssetReference("VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission9aExchangeAa_01.prefab:c93a2913e870b644aa41fb42fb265d16");

	private static readonly AssetReference VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission9aExchangeBa_01 = new AssetReference("VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission9aExchangeBa_01.prefab:c200c5234146a834c9012dde0b6b1fbd");

	private static readonly AssetReference VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission9aExchangeCa_01 = new AssetReference("VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission9aExchangeCa_01.prefab:3e2aa5f69217ba24aa9fff6eef3d69ad");

	private static readonly AssetReference VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission9aExchangeG_01 = new AssetReference("VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission9aExchangeG_01.prefab:94f5bb5903d64ac43a678504b385746f");

	private static readonly AssetReference VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission9aStart_01 = new AssetReference("VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission9aStart_01.prefab:12be7f7663d75c44d967bdee70a72209");

	private static readonly AssetReference VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission9aVictory_01 = new AssetReference("VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission9aVictory_01.prefab:973349211c92d0542918cf1f8fc2e33a");

	private static readonly AssetReference VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_PreMission9a_01 = new AssetReference("VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_PreMission9a_01.prefab:df93a009333cfd249a86744fb9fe3ed9");

	private static readonly AssetReference VO_Story_11_Finley_009hp_Male_Murloc_Story_Faelin_Mission9a_Lorebook_1_01 = new AssetReference("VO_Story_11_Finley_009hp_Male_Murloc_Story_Faelin_Mission9a_Lorebook_1_01.prefab:5c8548036e93ec344842bfce8d5a3696");

	private static readonly AssetReference VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission9aExchangeA_01 = new AssetReference("VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission9aExchangeA_01.prefab:9068660092e93834e8184f1026052520");

	private static readonly AssetReference VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission9aExchangeB_01 = new AssetReference("VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission9aExchangeB_01.prefab:62b6aa92147969b41902c212586fe9b7");

	private static readonly AssetReference VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission9aExchangeC_01 = new AssetReference("VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission9aExchangeC_01.prefab:fa33ce88d6e0bef4fa51e90194575096");

	private static readonly AssetReference VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission9aExchangeD_01 = new AssetReference("VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission9aExchangeD_01.prefab:daf72a7f6a8af4e49958cbd9e95578a0");

	private static readonly AssetReference VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission9aVictory_01 = new AssetReference("VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission9aVictory_01.prefab:4e1393f57bc639146af867b531e87bcb");

	private static readonly AssetReference VO_Story_11_Halus_010hp_Male_BloodElf_Story_Faelin_Mission9aExchangeD_01 = new AssetReference("VO_Story_11_Halus_010hp_Male_BloodElf_Story_Faelin_Mission9aExchangeD_01.prefab:a7c470ad0a966b5429aa5c7975ec651b");

	private static readonly AssetReference VO_Story_11_Halus_010hp_Male_BloodElf_Story_Faelin_Mission9aExchangeE_01 = new AssetReference("VO_Story_11_Halus_010hp_Male_BloodElf_Story_Faelin_Mission9aExchangeE_01.prefab:35744532d5a9d134a8b246e0027e1584");

	private static readonly AssetReference VO_Story_11_Halus_010hp_Male_BloodElf_Story_Faelin_Mission9aExchangeF_01 = new AssetReference("VO_Story_11_Halus_010hp_Male_BloodElf_Story_Faelin_Mission9aExchangeF_01.prefab:d32df8c58ab979c4091679da2c97e11a");

	private static readonly AssetReference VO_Story_11_Halus_010hp_Male_BloodElf_Story_Faelin_Mission9aExchangeG_01 = new AssetReference("VO_Story_11_Halus_010hp_Male_BloodElf_Story_Faelin_Mission9aExchangeG_01.prefab:db0757173d0be654d961fda67ed84538");

	private static readonly AssetReference VO_Story_11_Halus_010hp_Male_BloodElf_Story_Faelin_Mission9aVictory_01 = new AssetReference("VO_Story_11_Halus_010hp_Male_BloodElf_Story_Faelin_Mission9aVictory_01.prefab:bb67500ebf1619f409aa104299af08cb");

	private static readonly AssetReference VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission9aExchangeC_01 = new AssetReference("VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission9aExchangeC_01.prefab:a24717203c90a09468a8ce00ada31c1e");

	private static readonly AssetReference VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission9aExchangeD_01 = new AssetReference("VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission9aExchangeD_01.prefab:f20b3b7f3d296f64cadbaac02092ff67");

	private static readonly AssetReference VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission9aExchangeD_02 = new AssetReference("VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission9aExchangeD_02.prefab:6f0bbb858279ee745a2d94ae7b2a1c41");

	private static readonly AssetReference VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission9aExchangeF_01 = new AssetReference("VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission9aExchangeF_01.prefab:e9c49aaeccaa62a49b2f65e18ce5bfac");

	private static readonly AssetReference VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission9aStart_01 = new AssetReference("VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission9aStart_01.prefab:432a0d42b76793e41bb486907b9351a9");

	private static readonly AssetReference VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission9aVictory_01 = new AssetReference("VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission9aVictory_01.prefab:919953265997c0f469250b3de637eb0f");

	private static readonly AssetReference VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission9aVictory_02 = new AssetReference("VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission9aVictory_02.prefab:c09a7b71f30c3234ebfe30b149df0ce1");

	private static readonly AssetReference VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_PreMission9a_01 = new AssetReference("VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_PreMission9a_01.prefab:c47d12f9da98b6148ad2d5175f8f3939");

	private static readonly AssetReference VO_Story_11_Mission9a_Lorebook1_Female_Human_Story_Faelin_Mission9a_Lorebook_1_01 = new AssetReference("VO_Story_11_Mission9a_Lorebook1_Female_Human_Story_Faelin_Mission9a_Lorebook_1_01.prefab:e28a04c944205af418e9512ecef3d545");

	private static readonly AssetReference VO_Story_11_Murloc_009hb_Male_Murloc_Story_Faelin_Mission9aDeath_01 = new AssetReference("VO_Story_11_Murloc_009hb_Male_Murloc_Story_Faelin_Mission9aDeath_01.prefab:3ce09253a3111fb458c64f5dff99f03b");

	private static readonly AssetReference VO_Story_11_Murloc_009hb_Male_Murloc_Story_Faelin_Mission9aEmoteResponse_01 = new AssetReference("VO_Story_11_Murloc_009hb_Male_Murloc_Story_Faelin_Mission9aEmoteResponse_01.prefab:bd6a104e84b25d447a4408de43bee656");

	private static readonly AssetReference VO_Story_11_Murloc_009hb_Male_Murloc_Story_Faelin_Mission9aLoss_01 = new AssetReference("VO_Story_11_Murloc_009hb_Male_Murloc_Story_Faelin_Mission9aLoss_01.prefab:6f1f585019847614e948f6ef396a8988");

	private static readonly AssetReference VO_Story_11_Murloc_009hb_Male_Murloc_Story_Faelin_Mission9aStart_01 = new AssetReference("VO_Story_11_Murloc_009hb_Male_Murloc_Story_Faelin_Mission9aStart_01.prefab:a4c2fee450b8fa24ab14fefab27d02cb");

	private Notification m_popup;

	private Dictionary<int, string[]> m_popUpInfo = new Dictionary<int, string[]>
	{
		{
			229,
			new string[1] { "BOH_FAELIN_MISSION9a_LOREBOOK_01" }
		},
		{
			228,
			new string[1] { "BOH_FAELIN_MISSION9a_LOREBOOK_02" }
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

	private HashSet<string> m_playedLines = new HashSet<string>();

	private static Map<GameEntityOption, bool> InitBooleanOptions()
	{
		return new Map<GameEntityOption, bool> { 
		{
			GameEntityOption.DO_OPENING_TAUNTS,
			false
		} };
	}

	public BoH_Faelin_09A()
	{
		m_gameOptions.AddBooleanOptions(s_booleanOptions);
	}

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> m_SoundFiles = new List<string>
		{
			VO_Story_11_Murloc_009hb_Male_Murloc_Story_Faelin_Mission9aStart_01, VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission9aStart_01, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission9aStart_01, VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission9aExchangeA_01, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission9aExchangeAa_01, VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission9aExchangeB_01, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission9aExchangeBa_01, VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission9aExchangeC_01, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission9aExchangeCa_01, VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission9aExchangeC_01,
			VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission9aExchangeD_01, VO_Story_11_Caye_012hp_Female_Nightborne_Story_Faelin_Mission9aExchangeD_01, VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission9aExchangeD_02, VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission9aExchangeD_01, VO_Story_11_Halus_010hp_Male_BloodElf_Story_Faelin_Mission9aExchangeD_01, VO_Story_11_Halus_010hp_Male_BloodElf_Story_Faelin_Mission9aExchangeE_01, VO_Story_11_Caye_012hp_Female_Nightborne_Story_Faelin_Mission9aExchangeE_01, VO_Story_11_Halus_010hp_Male_BloodElf_Story_Faelin_Mission9aExchangeF_01, VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission9aExchangeF_01, VO_Story_11_Halus_010hp_Male_BloodElf_Story_Faelin_Mission9aExchangeG_01,
			VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission9aExchangeG_01, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission9aVictory_01, VO_Story_11_Halus_010hp_Male_BloodElf_Story_Faelin_Mission9aVictory_01, VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission9aVictory_02, VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission9aVictory_01, VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission9aVictory_01, VO_Story_11_Murloc_009hb_Male_Murloc_Story_Faelin_Mission9aLoss_01, VO_Story_11_Murloc_009hb_Male_Murloc_Story_Faelin_Mission9aDeath_01, VO_Story_11_Murloc_009hb_Male_Murloc_Story_Faelin_Mission9aEmoteResponse_01, VO_Story_11_Mission9a_Lorebook1_Female_Human_Story_Faelin_Mission9a_Lorebook_1_01,
			VO_Story_11_Finley_009hp_Male_Murloc_Story_Faelin_Mission9a_Lorebook_1_01
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

	public override void OnCreateGame()
	{
		base.OnCreateGame();
		m_Mission_EnemyPlayIdleLines = false;
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
			yield return MissionPlayVO(enemyActor, VO_Story_11_Murloc_009hb_Male_Murloc_Story_Faelin_Mission9aEmoteResponse_01);
			break;
		case 514:
			yield return MissionPlayVO(enemyActor, VO_Story_11_Murloc_009hb_Male_Murloc_Story_Faelin_Mission9aStart_01);
			yield return MissionPlayVO(IniBrassRing, VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission9aStart_01);
			yield return MissionPlayVO(friendlyActor, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission9aStart_01);
			break;
		case 507:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(enemyActor, VO_Story_11_Murloc_009hb_Male_Murloc_Story_Faelin_Mission9aLoss_01);
			GameState.Get().SetBusy(busy: false);
			break;
		case 504:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(friendlyActor, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission9aVictory_01);
			yield return MissionPlayVO(HalusBrassRing, VO_Story_11_Halus_010hp_Male_BloodElf_Story_Faelin_Mission9aVictory_01);
			yield return MissionPlayVO(IniBrassRing, VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission9aVictory_02);
			GameState.Get().SetBusy(busy: false);
			break;
		case 101:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(friendlyActor, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission9aVictory_01);
			yield return MissionPlayVO(GraceBrassRing, VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission9aVictory_01);
			GameState.Get().SetBusy(busy: false);
			break;
		case 102:
			yield return MissionPlayVO(HalusBrassRing, VO_Story_11_Halus_010hp_Male_BloodElf_Story_Faelin_Mission9aExchangeE_01);
			yield return MissionPlayVO(CayeBrassRing, VO_Story_11_Caye_012hp_Female_Nightborne_Story_Faelin_Mission9aExchangeE_01);
			break;
		case 103:
			yield return MissionPlayVO(GraceBrassRing, VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission9aExchangeA_01);
			yield return MissionPlayVO(friendlyActor, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission9aExchangeAa_01);
			break;
		case 104:
			yield return MissionPlayVO(HalusBrassRing, VO_Story_11_Halus_010hp_Male_BloodElf_Story_Faelin_Mission9aExchangeF_01);
			yield return MissionPlayVO(IniBrassRing, VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission9aExchangeF_01);
			break;
		case 105:
			yield return MissionPlayVO(GraceBrassRing, VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission9aExchangeB_01);
			yield return MissionPlayVO(friendlyActor, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission9aExchangeBa_01);
			break;
		case 106:
			yield return MissionPlayVO(HalusBrassRing, VO_Story_11_Halus_010hp_Male_BloodElf_Story_Faelin_Mission9aExchangeG_01);
			yield return MissionPlayVO(friendlyActor, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission9aExchangeG_01);
			break;
		case 107:
			yield return MissionPlayVO(GraceBrassRing, VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission9aExchangeC_01);
			yield return MissionPlayVO(friendlyActor, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission9aExchangeCa_01);
			break;
		case 108:
			yield return MissionPlayVO(IniBrassRing, VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission9aExchangeD_01);
			yield return MissionPlayVO(CayeBrassRing, VO_Story_11_Caye_012hp_Female_Nightborne_Story_Faelin_Mission9aExchangeD_01);
			yield return MissionPlayVO(IniBrassRing, VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission9aExchangeD_02);
			yield return MissionPlayVO(HalusBrassRing, VO_Story_11_Halus_010hp_Male_BloodElf_Story_Faelin_Mission9aExchangeD_01);
			break;
		case 109:
			yield return MissionPlayVO(IniBrassRing, VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission9aExchangeD_01);
			yield return MissionPlayVO(CayeBrassRing, VO_Story_11_Caye_012hp_Female_Nightborne_Story_Faelin_Mission9aExchangeD_01);
			yield return MissionPlayVO(IniBrassRing, VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission9aExchangeD_02);
			yield return MissionPlayVO(GraceBrassRing, VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission9aExchangeD_01);
			break;
		case 228:
			if (!(m_popup != null))
			{
				GameState.Get().SetBusy(busy: true);
				m_popup = NotificationManager.Get().CreatePopupText(UserAttentionBlocker.NONE, popUpPos, TutorialEntity.GetTextScale() * popUpScale, GameStrings.Get(m_popUpInfo[missionEvent][0]), convertLegacyPosition: false, NotificationManager.PopupTextType.FANCY);
				yield return GameEntity.Coroutines.StartCoroutine(PlaySoundAndBlockSpeech(VO_Story_11_Finley_009hp_Male_Murloc_Story_Faelin_Mission9a_Lorebook_1_01, Notification.SpeechBubbleDirection.None, GetEnemyActorByCardId("Story_11_Mission1_Lorebook1"), 2.5f));
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
				yield return GameEntity.Coroutines.StartCoroutine(PlaySoundAndBlockSpeech(VO_Story_11_Mission9a_Lorebook1_Female_Human_Story_Faelin_Mission9a_Lorebook_1_01, Notification.SpeechBubbleDirection.None, GetEnemyActorByCardId("Story_11_Mission1_Lorebook1"), 2.5f));
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
		GameState.Get().GetOpposingSidePlayer().GetHero()
			.GetCard()
			.GetActor();
		GameState.Get().GetFriendlySidePlayer().GetHero()
			.GetCard()
			.GetActor();
	}

	public override bool NotifyOfBattlefieldCardClicked(Entity clickedEntity, bool wasInTargetMode)
	{
		if (!base.NotifyOfBattlefieldCardClicked(clickedEntity, wasInTargetMode))
		{
			return false;
		}
		if (!wasInTargetMode)
		{
			if (clickedEntity.GetCardId() == "Story_11_Mission9a_Lorebook1")
			{
				if (m_popup == null)
				{
					Network.Get().SendClientScriptGameEvent(ClientScriptGameEventType.MISSION_EVENT, 228);
					HandleMissionEvent(228);
				}
				return false;
			}
			if (clickedEntity.GetCardId() == "Story_11_Mission9a_Lorebook2")
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
