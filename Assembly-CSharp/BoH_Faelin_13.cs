using System.Collections;
using System.Collections.Generic;
using Blizzard.T5.Core;
using UnityEngine;

public class BoH_Faelin_13 : BoH_Faelin_Dungeon
{
	private static Map<GameEntityOption, bool> s_booleanOptions = InitBooleanOptions();

	private static readonly AssetReference VO_Story_11_AbyssalJailor_013hb_Male_Faceless_Story_Faelin_Mission13EmoteResponse_01 = new AssetReference("VO_Story_11_AbyssalJailor_013hb_Male_Faceless_Story_Faelin_Mission13EmoteResponse_01.prefab:c77ebaa2592ddfd429524016140508e1");

	private static readonly AssetReference VO_Story_11_AbyssalJailor_013hb_Male_Faceless_Story_Faelin_Mission13ExchangeD_01 = new AssetReference("VO_Story_11_AbyssalJailor_013hb_Male_Faceless_Story_Faelin_Mission13ExchangeD_01.prefab:81baf765947057d42a11378bf7b514bb");

	private static readonly AssetReference VO_Story_11_AbyssalJailor_013hb_Male_Faceless_Story_Faelin_Mission13HeroPower_01 = new AssetReference("VO_Story_11_AbyssalJailor_013hb_Male_Faceless_Story_Faelin_Mission13HeroPower_01.prefab:be57bdc29752a614e94842cc25a13ce7");

	private static readonly AssetReference VO_Story_11_AbyssalJailor_013hb_Male_Faceless_Story_Faelin_Mission13HeroPower_02 = new AssetReference("VO_Story_11_AbyssalJailor_013hb_Male_Faceless_Story_Faelin_Mission13HeroPower_02.prefab:53166803f0b90724db70b9f4a6adafab");

	private static readonly AssetReference VO_Story_11_AbyssalJailor_013hb_Male_Faceless_Story_Faelin_Mission13HeroPower_03 = new AssetReference("VO_Story_11_AbyssalJailor_013hb_Male_Faceless_Story_Faelin_Mission13HeroPower_03.prefab:4013e1759f77d98438e0a64d74a16f06");

	private static readonly AssetReference VO_Story_11_AbyssalJailor_013hb_Male_Faceless_Story_Faelin_Mission13Idle_01 = new AssetReference("VO_Story_11_AbyssalJailor_013hb_Male_Faceless_Story_Faelin_Mission13Idle_01.prefab:b13fcb52802de5c43a29ba0d10ee37b8");

	private static readonly AssetReference VO_Story_11_AbyssalJailor_013hb_Male_Faceless_Story_Faelin_Mission13Idle_02 = new AssetReference("VO_Story_11_AbyssalJailor_013hb_Male_Faceless_Story_Faelin_Mission13Idle_02.prefab:65d9c5102b50ece45beff6281ad77d65");

	private static readonly AssetReference VO_Story_11_AbyssalJailor_013hb_Male_Faceless_Story_Faelin_Mission13Idle_03 = new AssetReference("VO_Story_11_AbyssalJailor_013hb_Male_Faceless_Story_Faelin_Mission13Idle_03.prefab:23db9ed44e0e3714f9eb7170e679ff21");

	private static readonly AssetReference VO_Story_11_AbyssalJailor_013hb_Male_Faceless_Story_Faelin_Mission13Loss_01 = new AssetReference("VO_Story_11_AbyssalJailor_013hb_Male_Faceless_Story_Faelin_Mission13Loss_01.prefab:90467970d3ab8d3479257ccb07c2afb4");

	private static readonly AssetReference VO_Story_11_AbyssalJailor_013hb_Male_Faceless_Story_Faelin_Mission13Start_01 = new AssetReference("VO_Story_11_AbyssalJailor_013hb_Male_Faceless_Story_Faelin_Mission13Start_01.prefab:8b9465eda7659be419756a9885bbb944");

	private static readonly AssetReference VO_Story_11_Caye_012hp_Female_Nightborne_Story_Faelin_Mission13ExchangeA_01 = new AssetReference("VO_Story_11_Caye_012hp_Female_Nightborne_Story_Faelin_Mission13ExchangeA_01.prefab:d2b2efd395030d54cad50395f69fbcb3");

	private static readonly AssetReference VO_Story_11_Caye_012hp_Female_Nightborne_Story_Faelin_Mission13ExchangeE_01 = new AssetReference("VO_Story_11_Caye_012hp_Female_Nightborne_Story_Faelin_Mission13ExchangeE_01.prefab:8e765d27f95af104a9accde57c1d1dec");

	private static readonly AssetReference VO_Story_11_Caye_012hp_Female_Nightborne_Story_Faelin_Mission13Start_01 = new AssetReference("VO_Story_11_Caye_012hp_Female_Nightborne_Story_Faelin_Mission13Start_01.prefab:4bcc79cf8af3e004c8af6e3758e42346");

	private static readonly AssetReference VO_Story_11_Caye_012hp_Female_Nightborne_Story_Faelin_Mission13Victory_01 = new AssetReference("VO_Story_11_Caye_012hp_Female_Nightborne_Story_Faelin_Mission13Victory_01.prefab:01d933c44f98d3242905b5a4ca250c8a");

	private static readonly AssetReference VO_Story_11_Caye_012hp_Female_Nightborne_Story_Faelin_PreMission13_01 = new AssetReference("VO_Story_11_Caye_012hp_Female_Nightborne_Story_Faelin_PreMission13_01.prefab:4ed3964bd5f18ac43bf82627d2da92fa");

	private static readonly AssetReference VO_Story_11_Dathril_012hb_Male_Naga_Story_Faelin_Mission13_Lorebook_2_01 = new AssetReference("VO_Story_11_Dathril_012hb_Male_Naga_Story_Faelin_Mission13_Lorebook_2_01.prefab:1c3e6d33c9b33444fbdf65bf420a6914");

	private static readonly AssetReference VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission13_Lorebook_2_01 = new AssetReference("VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission13_Lorebook_2_01.prefab:fed361d86849883449288a167321cdb3");

	private static readonly AssetReference VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission13ExchangeB_01 = new AssetReference("VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission13ExchangeB_01.prefab:065ccd7474ef95e438701913851b5f63");

	private static readonly AssetReference VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission13Victory_01 = new AssetReference("VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission13Victory_01.prefab:178dd9037df98ca40ba67156b2d51a67");

	private static readonly AssetReference VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_PreMission13_01 = new AssetReference("VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_PreMission13_01.prefab:2208212ea98e31749bdb95cd2b0370c6");

	private static readonly AssetReference VO_Story_11_Finley_009hp_Male_Murloc_Story_Faelin_Mission13ExchangeD_01 = new AssetReference("VO_Story_11_Finley_009hp_Male_Murloc_Story_Faelin_Mission13ExchangeD_01.prefab:217dee39779556045a5db7ed53572bc5");

	private static readonly AssetReference VO_Story_11_Finley_009hp_Male_Murloc_Story_Faelin_Mission13ExchangeF_01 = new AssetReference("VO_Story_11_Finley_009hp_Male_Murloc_Story_Faelin_Mission13ExchangeF_01.prefab:b329922d5940c0f42b895b1ccd154693");

	private static readonly AssetReference VO_Story_11_Finley_009hp_Male_Murloc_Story_Faelin_Mission13Victory_01 = new AssetReference("VO_Story_11_Finley_009hp_Male_Murloc_Story_Faelin_Mission13Victory_01.prefab:1fcc589165b23ef44b1e9c2bc52d0c94");

	private static readonly AssetReference VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission13ExchangeA_01 = new AssetReference("VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission13ExchangeA_01.prefab:3d9489cd32cc73546bb96a98916fa339");

	private static readonly AssetReference VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission13ExchangeB_01 = new AssetReference("VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission13ExchangeB_01.prefab:776ca1b2b0ebb0444ac94b119d88fde7");

	private static readonly AssetReference VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission13ExchangeB_02 = new AssetReference("VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission13ExchangeB_02.prefab:905930b468f433a42af72a6f2adec972");

	private static readonly AssetReference VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission13ExchangeE_01 = new AssetReference("VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission13ExchangeE_01.prefab:4021cc1ee7e20f744a8d1ee86881cbd4");

	private static readonly AssetReference VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission13Victory_01 = new AssetReference("VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission13Victory_01.prefab:aebed0707eac49d49a4ebaeb4eb1c97e");

	private static readonly AssetReference VO_Story_11_Halus_010hp_Male_BloodElf_Story_Faelin_Mission13ExchangeB_01 = new AssetReference("VO_Story_11_Halus_010hp_Male_BloodElf_Story_Faelin_Mission13ExchangeB_01.prefab:fb18790b3064c314bafb819a1a28e46f");

	private static readonly AssetReference VO_Story_11_Halus_010hp_Male_BloodElf_Story_Faelin_Mission13ExchangeC_01 = new AssetReference("VO_Story_11_Halus_010hp_Male_BloodElf_Story_Faelin_Mission13ExchangeC_01.prefab:271ced2d0a7320b4d82780ca034cc9ab");

	private static readonly AssetReference VO_Story_11_Halus_010hp_Male_BloodElf_Story_Faelin_Mission13Victory_01 = new AssetReference("VO_Story_11_Halus_010hp_Male_BloodElf_Story_Faelin_Mission13Victory_01.prefab:a4a5acf792452094bb496e603281abd0");

	private static readonly AssetReference VO_Story_11_Mission13_Lorebook1_Female_Naga_Story_Faelin_Mission13_Lorebook_1_01 = new AssetReference("VO_Story_11_Mission13_Lorebook1_Female_Naga_Story_Faelin_Mission13_Lorebook_1_01.prefab:46fb9c9d0bffe9c42a4dcb60137fb551");

	private static readonly AssetReference VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission13ExchangeA_01 = new AssetReference("VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission13ExchangeA_01.prefab:9e3986421e295cf40b9742f2473a97c2");

	private static readonly AssetReference VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission13ExchangeD_01 = new AssetReference("VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission13ExchangeD_01.prefab:8ab417f7eba321942b65f1c7c8026849");

	private static readonly AssetReference VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission13Victory_01 = new AssetReference("VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission13Victory_01.prefab:e2fb7734e22e7f6439db9a3b312fde67");

	private static readonly AssetReference VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_PreMission13_01 = new AssetReference("VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_PreMission13_01.prefab:62953a30be9da5b4ba45331b22e8eec0");

	private static readonly AssetReference VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_PreMission13_02 = new AssetReference("VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_PreMission13_02.prefab:bd6779440731a7b44a9bc66bae65b326");

	private Notification m_popup;

	private Dictionary<int, string[]> m_popUpInfo = new Dictionary<int, string[]>
	{
		{
			228,
			new string[1] { "BOH_FAELIN_MISSION13_LOREBOOK_01" }
		},
		{
			229,
			new string[1] { "BOH_FAELIN_MISSION13_LOREBOOK_02" }
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

	public static readonly AssetReference FaelinBrassRing = new AssetReference("SKN23-002_H_BOH_Faelin_Sad_BrassRing_Quote.prefab:a2375dc715c6d284a90383ff376e1b03");

	private new List<string> m_BossIdleLines = new List<string> { VO_Story_11_AbyssalJailor_013hb_Male_Faceless_Story_Faelin_Mission13Idle_01, VO_Story_11_AbyssalJailor_013hb_Male_Faceless_Story_Faelin_Mission13Idle_02, VO_Story_11_AbyssalJailor_013hb_Male_Faceless_Story_Faelin_Mission13Idle_03 };

	private List<string> m_BossUsesHeroPowerLines = new List<string> { VO_Story_11_AbyssalJailor_013hb_Male_Faceless_Story_Faelin_Mission13HeroPower_01, VO_Story_11_AbyssalJailor_013hb_Male_Faceless_Story_Faelin_Mission13HeroPower_02, VO_Story_11_AbyssalJailor_013hb_Male_Faceless_Story_Faelin_Mission13HeroPower_03 };

	private HashSet<string> m_playedLines = new HashSet<string>();

	private static Map<GameEntityOption, bool> InitBooleanOptions()
	{
		return new Map<GameEntityOption, bool> { 
		{
			GameEntityOption.DO_OPENING_TAUNTS,
			false
		} };
	}

	public BoH_Faelin_13()
	{
		m_gameOptions.AddBooleanOptions(s_booleanOptions);
	}

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> m_SoundFiles = new List<string>
		{
			VO_Story_11_Caye_012hp_Female_Nightborne_Story_Faelin_Mission13Start_01, VO_Story_11_AbyssalJailor_013hb_Male_Faceless_Story_Faelin_Mission13Start_01, VO_Story_11_Caye_012hp_Female_Nightborne_Story_Faelin_Mission13ExchangeA_01, VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission13ExchangeA_01, VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission13ExchangeA_01, VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission13ExchangeB_01, VO_Story_11_Halus_010hp_Male_BloodElf_Story_Faelin_Mission13ExchangeB_01, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission13ExchangeB_01, VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission13ExchangeB_02, VO_Story_11_Halus_010hp_Male_BloodElf_Story_Faelin_Mission13ExchangeC_01,
			VO_Story_11_Finley_009hp_Male_Murloc_Story_Faelin_Mission13ExchangeD_01, VO_Story_11_AbyssalJailor_013hb_Male_Faceless_Story_Faelin_Mission13ExchangeD_01, VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission13ExchangeD_01, VO_Story_11_Caye_012hp_Female_Nightborne_Story_Faelin_Mission13ExchangeE_01, VO_Story_11_Finley_009hp_Male_Murloc_Story_Faelin_Mission13ExchangeF_01, VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission13ExchangeE_01, VO_Story_11_Caye_012hp_Female_Nightborne_Story_Faelin_Mission13Victory_01, VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission13Victory_01, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission13Victory_01, VO_Story_11_Finley_009hp_Male_Murloc_Story_Faelin_Mission13Victory_01,
			VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission13Victory_01, VO_Story_11_Halus_010hp_Male_BloodElf_Story_Faelin_Mission13Victory_01, VO_Story_11_Mission13_Lorebook1_Female_Naga_Story_Faelin_Mission13_Lorebook_1_01, VO_Story_11_Dathril_012hb_Male_Naga_Story_Faelin_Mission13_Lorebook_2_01, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission13_Lorebook_2_01, VO_Story_11_AbyssalJailor_013hb_Male_Faceless_Story_Faelin_Mission13EmoteResponse_01, VO_Story_11_AbyssalJailor_013hb_Male_Faceless_Story_Faelin_Mission13HeroPower_01, VO_Story_11_AbyssalJailor_013hb_Male_Faceless_Story_Faelin_Mission13HeroPower_02, VO_Story_11_AbyssalJailor_013hb_Male_Faceless_Story_Faelin_Mission13HeroPower_03, VO_Story_11_AbyssalJailor_013hb_Male_Faceless_Story_Faelin_Mission13Loss_01,
			VO_Story_11_AbyssalJailor_013hb_Male_Faceless_Story_Faelin_Mission13Idle_01, VO_Story_11_AbyssalJailor_013hb_Male_Faceless_Story_Faelin_Mission13Idle_02, VO_Story_11_AbyssalJailor_013hb_Male_Faceless_Story_Faelin_Mission13Idle_03
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
		m_OverrideMusicTrack = MusicPlaylistType.InGame_TSC_Nazjatar;
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
			yield return MissionPlayVO(enemyActor, VO_Story_11_AbyssalJailor_013hb_Male_Faceless_Story_Faelin_Mission13EmoteResponse_01);
			break;
		case 514:
			yield return MissionPlayVO(friendlyActor, VO_Story_11_Caye_012hp_Female_Nightborne_Story_Faelin_Mission13Start_01);
			yield return MissionPlayVO(enemyActor, VO_Story_11_AbyssalJailor_013hb_Male_Faceless_Story_Faelin_Mission13Start_01);
			break;
		case 507:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(enemyActor, VO_Story_11_AbyssalJailor_013hb_Male_Faceless_Story_Faelin_Mission13Loss_01);
			GameState.Get().SetBusy(busy: false);
			break;
		case 504:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(friendlyActor, VO_Story_11_Caye_012hp_Female_Nightborne_Story_Faelin_Mission13Victory_01);
			yield return MissionPlayVO(IniBrassRing, VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission13Victory_01);
			yield return MissionPlayVO(FaelinBrassRing, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission13Victory_01);
			yield return MissionPlayVO(HalusBrassRing, VO_Story_11_Halus_010hp_Male_BloodElf_Story_Faelin_Mission13Victory_01);
			GameState.Get().SetBusy(busy: false);
			break;
		case 101:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(friendlyActor, VO_Story_11_Caye_012hp_Female_Nightborne_Story_Faelin_Mission13Victory_01);
			yield return MissionPlayVO(IniBrassRing, VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission13Victory_01);
			yield return MissionPlayVO(FaelinBrassRing, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission13Victory_01);
			yield return MissionPlayVO(GraceBrassRing, VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission13Victory_01);
			GameState.Get().SetBusy(busy: false);
			break;
		case 102:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(friendlyActor, VO_Story_11_Caye_012hp_Female_Nightborne_Story_Faelin_Mission13Victory_01);
			yield return MissionPlayVO(IniBrassRing, VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission13Victory_01);
			yield return MissionPlayVO(FaelinBrassRing, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission13Victory_01);
			yield return MissionPlayVO(FinleyBrassRing, VO_Story_11_Finley_009hp_Male_Murloc_Story_Faelin_Mission13Victory_01);
			GameState.Get().SetBusy(busy: false);
			break;
		case 103:
			yield return MissionPlayVO(friendlyActor, VO_Story_11_Caye_012hp_Female_Nightborne_Story_Faelin_Mission13ExchangeA_01);
			yield return MissionPlayVO(IniBrassRing, VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission13ExchangeA_01);
			break;
		case 104:
			yield return MissionPlayVO(friendlyActor, VO_Story_11_Caye_012hp_Female_Nightborne_Story_Faelin_Mission13ExchangeA_01);
			yield return MissionPlayVO(IniBrassRing, VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission13ExchangeA_01);
			yield return MissionPlayVO(GraceBrassRing, VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission13ExchangeA_01);
			break;
		case 105:
			yield return MissionPlayVO(HalusBrassRing, VO_Story_11_Halus_010hp_Male_BloodElf_Story_Faelin_Mission13ExchangeB_01);
			yield return MissionPlayVO(FaelinBrassRing, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission13ExchangeB_01);
			yield return MissionPlayVO(HalusBrassRing, VO_Story_11_Halus_010hp_Male_BloodElf_Story_Faelin_Mission13ExchangeC_01);
			break;
		case 106:
			yield return MissionPlayVO(GraceBrassRing, VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission13ExchangeB_01);
			yield return MissionPlayVO(FaelinBrassRing, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission13ExchangeB_01);
			yield return MissionPlayVO(GraceBrassRing, VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission13ExchangeB_02);
			break;
		case 107:
			yield return MissionPlayVO(FinleyBrassRing, VO_Story_11_Finley_009hp_Male_Murloc_Story_Faelin_Mission13ExchangeD_01);
			break;
		case 108:
			yield return MissionPlayVO(GraceBrassRing, VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission13ExchangeE_01);
			break;
		case 109:
			yield return MissionPlayVO(CayeBrassRing, VO_Story_11_Caye_012hp_Female_Nightborne_Story_Faelin_Mission13ExchangeE_01);
			yield return MissionPlayVO(FinleyBrassRing, VO_Story_11_Finley_009hp_Male_Murloc_Story_Faelin_Mission13ExchangeF_01);
			break;
		case 228:
			if (!(m_popup != null))
			{
				GameState.Get().SetBusy(busy: true);
				m_popup = NotificationManager.Get().CreatePopupText(UserAttentionBlocker.NONE, popUpPos, TutorialEntity.GetTextScale() * popUpScale, GameStrings.Get(m_popUpInfo[missionEvent][0]), convertLegacyPosition: false, NotificationManager.PopupTextType.FANCY);
				yield return GameEntity.Coroutines.StartCoroutine(PlaySoundAndBlockSpeech(VO_Story_11_Mission13_Lorebook1_Female_Naga_Story_Faelin_Mission13_Lorebook_1_01, Notification.SpeechBubbleDirection.None, GetEnemyActorByCardId("Story_11_Mission1_Lorebook1"), 2.5f));
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
				yield return GameEntity.Coroutines.StartCoroutine(PlaySoundAndBlockSpeech(VO_Story_11_Dathril_012hb_Male_Naga_Story_Faelin_Mission13_Lorebook_2_01, Notification.SpeechBubbleDirection.None, GetEnemyActorByCardId("Story_11_Mission1_Lorebook1"), 2.5f));
				NotificationManager.Get().DestroyNotification(m_popup, 0f);
				m_popup = null;
				GameState.Get().SetBusy(busy: false);
				yield return MissionPlayVO(FaelinBrassRing, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission13_Lorebook_2_01);
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
		GameState.Get().GetFriendlySidePlayer().GetHero()
			.GetCard()
			.GetActor();
		if (turn == 7)
		{
			yield return MissionPlayVO(enemyActor, VO_Story_11_AbyssalJailor_013hb_Male_Faceless_Story_Faelin_Mission13ExchangeD_01);
			yield return MissionPlayVO(IniBrassRing, VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission13ExchangeD_01);
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
			if (clickedEntity.GetCardId() == "Story_11_Mission13_Lorebook1")
			{
				if (m_popup == null)
				{
					Network.Get().SendClientScriptGameEvent(ClientScriptGameEventType.MISSION_EVENT, 228);
					HandleMissionEvent(228);
				}
				return false;
			}
			if (clickedEntity.GetCardId() == "Story_11_Mission13_Lorebook2")
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
