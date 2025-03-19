using System.Collections;
using System.Collections.Generic;
using Blizzard.T5.Core;

public class BoH_Faelin_14 : BoH_Faelin_Dungeon
{
	private static Map<GameEntityOption, bool> s_booleanOptions = InitBooleanOptions();

	private static readonly AssetReference VO_Story_11_Caye_012hp_Female_Nightborne_Story_Faelin_Mission14ExchangeA_01 = new AssetReference("VO_Story_11_Caye_012hp_Female_Nightborne_Story_Faelin_Mission14ExchangeA_01.prefab:b08ee020bc21e6f49aa31c3f9c17e7ea");

	private static readonly AssetReference VO_Story_11_Caye_012hp_Female_Nightborne_Story_Faelin_PreMission14_01 = new AssetReference("VO_Story_11_Caye_012hp_Female_Nightborne_Story_Faelin_PreMission14_01.prefab:64ac847bc26647f47a9a8a4684155f45");

	private static readonly AssetReference VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission14ExchangeA_01 = new AssetReference("VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission14ExchangeA_01.prefab:93be20207c1fc624c82a6c7689ea0550");

	private static readonly AssetReference VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission14Victory_01 = new AssetReference("VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission14Victory_01.prefab:4471f9ba949108e4b944c72c9e33ce05");

	private static readonly AssetReference VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission14Victory_02 = new AssetReference("VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission14Victory_02.prefab:5f9950f763ef8ce41a6820490db7bef8");

	private static readonly AssetReference VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission14Victory_03 = new AssetReference("VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission14Victory_03.prefab:436480a19dbc0e94394c6ee31098188f");

	private static readonly AssetReference VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission14Victory_04 = new AssetReference("VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission14Victory_04.prefab:0a9a7631e03b95246a37c6ff2e63c341");

	private static readonly AssetReference VO_Story_11_Finley_009hp_Male_Murloc_Story_Faelin_Mission14ExchangeB_01 = new AssetReference("VO_Story_11_Finley_009hp_Male_Murloc_Story_Faelin_Mission14ExchangeB_01.prefab:90bed5d3610d3b4468977a929156fa6f");

	private static readonly AssetReference VO_Story_11_Finley_009hp_Male_Murloc_Story_Faelin_Mission14Victory_01 = new AssetReference("VO_Story_11_Finley_009hp_Male_Murloc_Story_Faelin_Mission14Victory_01.prefab:ebb2714e356bc2146b6de14ecc8a88eb");

	private static readonly AssetReference VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission14ExchangeB_01 = new AssetReference("VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission14ExchangeB_01.prefab:0b44671e3f4157e41b65d85a1631006d");

	private static readonly AssetReference VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission14Victory_01 = new AssetReference("VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission14Victory_01.prefab:e0bb47f0bcbeda8459288e425b3d4d6c");

	private static readonly AssetReference VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission14_Loss_01 = new AssetReference("VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission14_Loss_01.prefab:14b044f06d87dc34697c200e519c39d3");

	private static readonly AssetReference VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission14_Loss_02 = new AssetReference("VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission14_Loss_02.prefab:c77385a2f2c986c4492e9caa1fb462ac");

	private static readonly AssetReference VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission14_Reaction_01 = new AssetReference("VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission14_Reaction_01.prefab:3a47a521409f59142b37ff6c04ae34b4");

	private static readonly AssetReference VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission14_Reaction_02 = new AssetReference("VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission14_Reaction_02.prefab:dc2570bb7d0956045b02c33e771150d7");

	private static readonly AssetReference VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission14_Reaction_03 = new AssetReference("VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission14_Reaction_03.prefab:1819128e09c58cd4c99498f5b545a10a");

	private static readonly AssetReference VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission14_Reaction_04 = new AssetReference("VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission14_Reaction_04.prefab:11938993fc90c574f9294754b0e0f8c2");

	private static readonly AssetReference VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission14ExchangeA_01 = new AssetReference("VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission14ExchangeA_01.prefab:b383a36e4f051fc4ca5d67f82c0847b5");

	private static readonly AssetReference VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission14ExchangeB_01 = new AssetReference("VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission14ExchangeB_01.prefab:e42ea0bfca4592e408be27d5d8b2ac06");

	private static readonly AssetReference VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission14ExchangeC_01 = new AssetReference("VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission14ExchangeC_01.prefab:c4dd276d4d601d14294441013a78645a");

	private static readonly AssetReference VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission14ExchangeC_02 = new AssetReference("VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission14ExchangeC_02.prefab:1acd56182be2a8349bce451430e8485d");

	private static readonly AssetReference VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission14ExchangeD_01 = new AssetReference("VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission14ExchangeD_01.prefab:26e2791ab3509904c956a5f7d7063e4e");

	private static readonly AssetReference VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission14ExchangeE_01 = new AssetReference("VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission14ExchangeE_01.prefab:7dfa265801fdaaf47b2fdb5090aea842");

	private static readonly AssetReference VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission14Start_01 = new AssetReference("VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission14Start_01.prefab:cb01ce65bca3c2b448d4f59ade690767");

	private static readonly AssetReference VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_PreMission14_01 = new AssetReference("VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_PreMission14_01.prefab:094d122b00961544ca0eb6e988b7722c");

	private static readonly AssetReference VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_PreMission14_02 = new AssetReference("VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_PreMission14_02.prefab:0e81737efed7ab5448ff3510afced41a");

	private static readonly AssetReference VO_Story_11_Halus_010hp_Male_BloodElf_Story_Faelin_Mission14ExchangeB_01 = new AssetReference("VO_Story_11_Halus_010hp_Male_BloodElf_Story_Faelin_Mission14ExchangeB_01.prefab:62977f16e3685f64fa4d5e898ce26989");

	private static readonly AssetReference VO_Story_11_Halus_010hp_Male_BloodElf_Story_Faelin_Mission14Victory_01 = new AssetReference("VO_Story_11_Halus_010hp_Male_BloodElf_Story_Faelin_Mission14Victory_01.prefab:073851dc38e9c4346970fafb5d64b89f");

	private static readonly AssetReference Story_11_Leviathan_001hb_Death_Sound = new AssetReference("Story_11_Leviathan_001hb_Death_Sound.prefab:d5051de62f48f4b49aa855220792f2f4");

	private static readonly AssetReference Story_11_Leviathan_001hb_EmoteResponse_Sound = new AssetReference("Story_11_Leviathan_001hb_EmoteResponse_Sound.prefab:90b2aae30ca06f648a8033e394c1fa22");

	private static readonly AssetReference Story_11_Leviathan_001hb_Loss_Sound = new AssetReference("Story_11_Leviathan_001hb_Loss_Sound.prefab:c8702d2dff96aa1468e091e19a0f99cb");

	private static readonly AssetReference Story_11_Leviathan_001hb_Rev_Sound = new AssetReference("Story_11_Leviathan_001hb_Rev_Sound.prefab:63e2a91d4276f1a429b25b7ddcddd53d");

	public static readonly AssetReference IniBrassRing = new AssetReference("HS25-189_H_Ini_BrassRing_Quote.prefab:9f20435bdae99d24bae357f002abcf27");

	public static readonly AssetReference CayeBrassRing = new AssetReference("LC23-001_H_Caye_BrassRing_Quote.prefab:2364e8ce64f9c404fb569a86b17f3d01");

	public static readonly AssetReference BookBrassRing = new AssetReference("Wisdomball_Pop-up_BrassRing_Quote.prefab:896ee20514caff74db639aa7055838f6");

	public static readonly AssetReference FinleyBrassRing = new AssetReference("HS25-190_M_Finley_BrassRing_Quote.prefab:7123c129afe769e4aa9fb262a8f7c919");

	public static readonly AssetReference HalusBrassRing = new AssetReference("LC23-003_H_Halus_BrassRing_Quote.prefab:36e0aabb3ed4ce84599d9dd4e9ebdcf5");

	public static readonly AssetReference GraceBrassRing = new AssetReference("LC23-002_H_Grace_BrassRing_Quote.prefab:1f849ba2e303ff3449d34b96f960c96a");

	public static readonly AssetReference FaelinBrassRing = new AssetReference("SKN23-002_H_FAELIN_BrassRing_Quote.prefab:9984152d900140547aa7d721f3e49428");

	public static readonly AssetReference FaelinSadBrassRing = new AssetReference("SKN23-002_H_BOH_Faelin_Sad_BrassRing_Quote.prefab:a2375dc715c6d284a90383ff376e1b03");

	private List<string> m_LossLines = new List<string> { VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission14_Loss_01, VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission14_Loss_02, VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission14_Reaction_01, VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission14_Reaction_02, VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission14_Reaction_03 };

	private HashSet<string> m_playedLines = new HashSet<string>();

	private static Map<GameEntityOption, bool> InitBooleanOptions()
	{
		return new Map<GameEntityOption, bool> { 
		{
			GameEntityOption.DO_OPENING_TAUNTS,
			false
		} };
	}

	public BoH_Faelin_14()
	{
		m_gameOptions.AddBooleanOptions(s_booleanOptions);
	}

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> m_SoundFiles = new List<string>
		{
			VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission14Start_01, VO_Story_11_Caye_012hp_Female_Nightborne_Story_Faelin_Mission14ExchangeA_01, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission14ExchangeA_01, VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission14ExchangeA_01, VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission14ExchangeB_01, VO_Story_11_Finley_009hp_Male_Murloc_Story_Faelin_Mission14ExchangeB_01, VO_Story_11_Halus_010hp_Male_BloodElf_Story_Faelin_Mission14ExchangeB_01, VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission14ExchangeB_01, VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission14ExchangeC_01, VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission14ExchangeC_02,
			VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission14ExchangeD_01, VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission14_Reaction_01, VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission14_Reaction_02, VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission14_Reaction_03, VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission14_Reaction_04, VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission14ExchangeE_01, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission14Victory_01, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission14Victory_02, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission14Victory_03, VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission14Victory_01,
			VO_Story_11_Finley_009hp_Male_Murloc_Story_Faelin_Mission14Victory_01, VO_Story_11_Halus_010hp_Male_BloodElf_Story_Faelin_Mission14Victory_01, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission14Victory_04, VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission14_Loss_01, VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission14_Loss_02, Story_11_Leviathan_001hb_Death_Sound, Story_11_Leviathan_001hb_EmoteResponse_Sound, Story_11_Leviathan_001hb_Loss_Sound, Story_11_Leviathan_001hb_Rev_Sound
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
		m_OverrideMusicTrack = MusicPlaylistType.InGame_TSC_Leviathan;
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
		switch (missionEvent)
		{
		case 515:
			yield return MissionPlaySound(enemyActor, Story_11_Leviathan_001hb_EmoteResponse_Sound);
			break;
		case 507:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(friendlyActor, VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission14_Loss_01);
			GameState.Get().SetBusy(busy: false);
			break;
		case 504:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(FaelinSadBrassRing, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission14Victory_01);
			yield return MissionPlayVO(FaelinSadBrassRing, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission14Victory_02);
			yield return MissionPlayVO(FaelinSadBrassRing, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission14Victory_03);
			yield return MissionPlayVO(HalusBrassRing, VO_Story_11_Halus_010hp_Male_BloodElf_Story_Faelin_Mission14Victory_01);
			yield return MissionPlayVO(FaelinBrassRing, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission14Victory_04);
			GameState.Get().SetBusy(busy: false);
			break;
		case 101:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(FaelinSadBrassRing, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission14Victory_01);
			yield return MissionPlayVO(FaelinSadBrassRing, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission14Victory_02);
			yield return MissionPlayVO(FaelinSadBrassRing, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission14Victory_03);
			yield return MissionPlayVO(GraceBrassRing, VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission14Victory_01);
			yield return MissionPlayVO(FaelinBrassRing, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission14Victory_04);
			GameState.Get().SetBusy(busy: false);
			break;
		case 102:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(FaelinSadBrassRing, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission14Victory_01);
			yield return MissionPlayVO(FaelinSadBrassRing, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission14Victory_02);
			yield return MissionPlayVO(FaelinSadBrassRing, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission14Victory_03);
			yield return MissionPlayVO(FinleyBrassRing, VO_Story_11_Finley_009hp_Male_Murloc_Story_Faelin_Mission14Victory_01);
			yield return MissionPlayVO(FaelinBrassRing, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission14Victory_04);
			GameState.Get().SetBusy(busy: false);
			break;
		case 520:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlaySound(enemyActor, Story_11_Leviathan_001hb_Loss_Sound);
			yield return MissionPlayVO(friendlyActor, m_LossLines);
			GameState.Get().SetBusy(busy: false);
			break;
		case 103:
			yield return MissionPlayVO(friendlyActor, VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission14Start_01);
			break;
		case 104:
			yield return MissionPlayVOOnce(CayeBrassRing, VO_Story_11_Caye_012hp_Female_Nightborne_Story_Faelin_Mission14ExchangeA_01);
			yield return MissionPlayVOOnce(FaelinSadBrassRing, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission14ExchangeA_01);
			yield return MissionPlayVOOnce(friendlyActor, VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission14ExchangeA_01);
			break;
		case 105:
			yield return MissionPlayVOOnce(HalusBrassRing, VO_Story_11_Halus_010hp_Male_BloodElf_Story_Faelin_Mission14ExchangeB_01);
			yield return MissionPlayVOOnce(friendlyActor, VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission14ExchangeB_01);
			break;
		case 106:
			yield return MissionPlayVOOnce(GraceBrassRing, VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission14ExchangeB_01);
			yield return MissionPlayVOOnce(friendlyActor, VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission14ExchangeB_01);
			break;
		case 107:
			yield return MissionPlayVOOnce(FinleyBrassRing, VO_Story_11_Finley_009hp_Male_Murloc_Story_Faelin_Mission14ExchangeB_01);
			yield return MissionPlayVOOnce(friendlyActor, VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission14ExchangeB_01);
			break;
		case 108:
			yield return MissionPlayVO(friendlyActor, VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission14ExchangeC_01);
			yield return MissionPlayVO(friendlyActor, VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission14ExchangeC_02);
			break;
		case 109:
			yield return MissionPlayVO(friendlyActor, VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission14ExchangeD_01);
			break;
		case 110:
			yield return MissionPlayVO(friendlyActor, VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission14_Reaction_01);
			break;
		case 111:
			yield return MissionPlayVO(friendlyActor, VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission14_Reaction_02);
			break;
		case 112:
			yield return MissionPlayVO(friendlyActor, VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission14_Reaction_02);
			break;
		case 113:
			yield return MissionPlayVO(friendlyActor, VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission14_Reaction_04);
			break;
		case 114:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlaySound(enemyActor, Story_11_Leviathan_001hb_Rev_Sound);
			yield return MissionPlayVO(friendlyActor, VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission14ExchangeE_01);
			GameState.Get().SetBusy(busy: false);
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
}
