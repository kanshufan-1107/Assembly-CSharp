using System.Collections;
using System.Collections.Generic;

public class BOM_07_Scabbs_Fight_001 : BOM_07_Scabbs_Dungeon
{
	private static readonly AssetReference VO_Story_Hero_Bob_Male_Human_BOM_Scabbs_Mission1Beast_01 = new AssetReference("VO_Story_Hero_Bob_Male_Human_BOM_Scabbs_Mission1Beast_01.prefab:c86b05f0d23e4d63bcaea46b29751c67");

	private static readonly AssetReference VO_Story_Hero_Bob_Male_Human_BOM_Scabbs_Mission1Beast_02 = new AssetReference("VO_Story_Hero_Bob_Male_Human_BOM_Scabbs_Mission1Beast_02.prefab:a570d9c6593a4614a39c22a46f3decb2");

	private static readonly AssetReference VO_Story_Hero_Bob_Male_Human_BOM_Scabbs_Mission1Death_01 = new AssetReference("VO_Story_Hero_Bob_Male_Human_BOM_Scabbs_Mission1Death_01.prefab:6db253e3b9134a579dcd663713e8edf7");

	private static readonly AssetReference VO_Story_Hero_Bob_Male_Human_BOM_Scabbs_Mission1Demon_01 = new AssetReference("VO_Story_Hero_Bob_Male_Human_BOM_Scabbs_Mission1Demon_01.prefab:403de3b82bb84822bbd98815371e96a3");

	private static readonly AssetReference VO_Story_Hero_Bob_Male_Human_BOM_Scabbs_Mission1Demon_02 = new AssetReference("VO_Story_Hero_Bob_Male_Human_BOM_Scabbs_Mission1Demon_02.prefab:329c247965e0481786b5e1b189e611f5");

	private static readonly AssetReference VO_Story_Hero_Bob_Male_Human_BOM_Scabbs_Mission1Dragon_01 = new AssetReference("VO_Story_Hero_Bob_Male_Human_BOM_Scabbs_Mission1Dragon_01.prefab:673387c8360c43fe978f4156dc03a49c");

	private static readonly AssetReference VO_Story_Hero_Bob_Male_Human_BOM_Scabbs_Mission1Dragon_02 = new AssetReference("VO_Story_Hero_Bob_Male_Human_BOM_Scabbs_Mission1Dragon_02.prefab:82888a72705c45758cdff119a511ee4f");

	private static readonly AssetReference VO_Story_Hero_Bob_Male_Human_BOM_Scabbs_Mission1Elemental_01 = new AssetReference("VO_Story_Hero_Bob_Male_Human_BOM_Scabbs_Mission1Elemental_01.prefab:fd947c735fc34a86a5b0cd66408ebd2f");

	private static readonly AssetReference VO_Story_Hero_Bob_Male_Human_BOM_Scabbs_Mission1Elemental_02 = new AssetReference("VO_Story_Hero_Bob_Male_Human_BOM_Scabbs_Mission1Elemental_02.prefab:ffe9b9a9beee45bfa9935add2bad6a5d");

	private static readonly AssetReference VO_Story_Hero_Bob_Male_Human_BOM_Scabbs_Mission1EmoteResponse_01 = new AssetReference("VO_Story_Hero_Bob_Male_Human_BOM_Scabbs_Mission1EmoteResponse_01.prefab:f22bd2d0c54c4f8595b62764a8adaa27");

	private static readonly AssetReference VO_Story_Hero_Bob_Male_Human_BOM_Scabbs_Mission1ExchangeA_01 = new AssetReference("VO_Story_Hero_Bob_Male_Human_BOM_Scabbs_Mission1ExchangeA_01.prefab:5dac0557c59e461bb374441e25d30104");

	private static readonly AssetReference VO_Story_Hero_Bob_Male_Human_BOM_Scabbs_Mission1ExchangeA_03 = new AssetReference("VO_Story_Hero_Bob_Male_Human_BOM_Scabbs_Mission1ExchangeA_03.prefab:0fd9e569342c4da29e551bf348b60955");

	private static readonly AssetReference VO_Story_Hero_Bob_Male_Human_BOM_Scabbs_Mission1ExchangeB_02 = new AssetReference("VO_Story_Hero_Bob_Male_Human_BOM_Scabbs_Mission1ExchangeB_02.prefab:2dfce34832ae4477b356c9546d471d3f");

	private static readonly AssetReference VO_Story_Hero_Bob_Male_Human_BOM_Scabbs_Mission1ExchangeC_02 = new AssetReference("VO_Story_Hero_Bob_Male_Human_BOM_Scabbs_Mission1ExchangeC_02.prefab:07afc589267747678bfc117237530e4f");

	private static readonly AssetReference VO_Story_Hero_Bob_Male_Human_BOM_Scabbs_Mission1Idle_01 = new AssetReference("VO_Story_Hero_Bob_Male_Human_BOM_Scabbs_Mission1Idle_01.prefab:475ab5d73aff4b5ba32cab5922d82172");

	private static readonly AssetReference VO_Story_Hero_Bob_Male_Human_BOM_Scabbs_Mission1Idle_02 = new AssetReference("VO_Story_Hero_Bob_Male_Human_BOM_Scabbs_Mission1Idle_02.prefab:62ce597d155c4b13b93f8b03fb2a5461");

	private static readonly AssetReference VO_Story_Hero_Bob_Male_Human_BOM_Scabbs_Mission1Idle_03 = new AssetReference("VO_Story_Hero_Bob_Male_Human_BOM_Scabbs_Mission1Idle_03.prefab:02fc51b7f29c4d64b6029f3689278d45");

	private static readonly AssetReference VO_Story_Hero_Bob_Male_Human_BOM_Scabbs_Mission1Intro_01 = new AssetReference("VO_Story_Hero_Bob_Male_Human_BOM_Scabbs_Mission1Intro_01.prefab:2fe68ff190bf46dcba2a91762ffe2146");

	private static readonly AssetReference VO_Story_Hero_Bob_Male_Human_BOM_Scabbs_Mission1Loss_01 = new AssetReference("VO_Story_Hero_Bob_Male_Human_BOM_Scabbs_Mission1Loss_01.prefab:5dc86854007b42ce82812050d0e0fb1f");

	private static readonly AssetReference VO_Story_Hero_Bob_Male_Human_BOM_Scabbs_Mission1LowHealth_01 = new AssetReference("VO_Story_Hero_Bob_Male_Human_BOM_Scabbs_Mission1LowHealth_01.prefab:3f925590fdee48239c17a951183ab353");

	private static readonly AssetReference VO_Story_Hero_Bob_Male_Human_BOM_Scabbs_Mission1Mech_01 = new AssetReference("VO_Story_Hero_Bob_Male_Human_BOM_Scabbs_Mission1Mech_01.prefab:c8fe6cd3e0d0473d9d6c66eb0b0ff7bf");

	private static readonly AssetReference VO_Story_Hero_Bob_Male_Human_BOM_Scabbs_Mission1Mech_02 = new AssetReference("VO_Story_Hero_Bob_Male_Human_BOM_Scabbs_Mission1Mech_02.prefab:85f482789eeb4c5aa9b22a999d493191");

	private static readonly AssetReference VO_Story_Hero_Bob_Male_Human_BOM_Scabbs_Mission1Murloc_01 = new AssetReference("VO_Story_Hero_Bob_Male_Human_BOM_Scabbs_Mission1Murloc_01.prefab:34d8b2c43cbb4f5aa28e604fab53b300");

	private static readonly AssetReference VO_Story_Hero_Bob_Male_Human_BOM_Scabbs_Mission1Murloc_02 = new AssetReference("VO_Story_Hero_Bob_Male_Human_BOM_Scabbs_Mission1Murloc_02.prefab:bc17ad439043498bad43d9cc847a2560");

	private static readonly AssetReference VO_Story_Hero_Bob_Male_Human_BOM_Scabbs_Mission1Pirate_01 = new AssetReference("VO_Story_Hero_Bob_Male_Human_BOM_Scabbs_Mission1Pirate_01.prefab:09a78b243cae4f4d9283828dedb3fb50");

	private static readonly AssetReference VO_Story_Hero_Bob_Male_Human_BOM_Scabbs_Mission1Pirate_02 = new AssetReference("VO_Story_Hero_Bob_Male_Human_BOM_Scabbs_Mission1Pirate_02.prefab:57cda197f97541d6b7eef8ad4cec4516");

	private static readonly AssetReference VO_Story_Hero_Bob_Male_Human_BOM_Scabbs_Mission1Quilboar_01 = new AssetReference("VO_Story_Hero_Bob_Male_Human_BOM_Scabbs_Mission1Quilboar_01.prefab:b5c2aa5b0ade493084f1ed4741180a19");

	private static readonly AssetReference VO_Story_Hero_Bob_Male_Human_BOM_Scabbs_Mission1Quilboar_02 = new AssetReference("VO_Story_Hero_Bob_Male_Human_BOM_Scabbs_Mission1Quilboar_02.prefab:1eb5f3604bff46eb8a669ce0779c8971");

	private static readonly AssetReference VO_Story_Hero_Bob_Male_Human_BOM_Scabbs_Mission1Victory_01 = new AssetReference("VO_Story_Hero_Bob_Male_Human_BOM_Scabbs_Mission1Victory_01.prefab:d7788521e5cb46d2b1136d8445639369");

	private static readonly AssetReference VO_Story_Hero_Bob_Male_Human_BOM_Scabbs_Mission1Victory_03 = new AssetReference("VO_Story_Hero_Bob_Male_Human_BOM_Scabbs_Mission1Victory_03.prefab:649a97f513a9473c93d10a2a9b65add1");

	private static readonly AssetReference VO_Story_Hero_Bob_Male_Human_BOM_Scabbs_Mission1Victory_05 = new AssetReference("VO_Story_Hero_Bob_Male_Human_BOM_Scabbs_Mission1Victory_05.prefab:3a5639b6ca00428aa9d18afaa7a4269e");

	private static readonly AssetReference VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission1ExchangeA_02 = new AssetReference("VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission1ExchangeA_02.prefab:df1b24266ecc4aaca4a726069165aab2");

	private static readonly AssetReference VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission1ExchangeB_01 = new AssetReference("VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission1ExchangeB_01.prefab:ab0d863704a441bebd10d7d4835f3d85");

	private static readonly AssetReference VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission1ExchangeC_01 = new AssetReference("VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission1ExchangeC_01.prefab:bf00d7ce77b949958f1915037b186e2d");

	private static readonly AssetReference VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission1Intro_02 = new AssetReference("VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission1Intro_02.prefab:52d1f6ee678f45f5aa7bb9a52a9e10da");

	private static readonly AssetReference VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission1Victory_02 = new AssetReference("VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission1Victory_02.prefab:cc8da4fc83b1444683f253613a92dd73");

	private static readonly AssetReference VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission1Victory_04 = new AssetReference("VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission1Victory_04.prefab:a4275c502d294257b9a2d1b03b31e44e");

	private static readonly AssetReference VO_DALA_BOSS_99h_Male_Human_RecruitSmallMinion_01 = new AssetReference("VO_DALA_BOSS_99h_Male_Human_RecruitSmallMinion_01.prefab:347c19b9fd13bcf4db40d99a28ed0e9b");

	private static readonly AssetReference VO_DALA_BOSS_99h_Male_Human_RecruitSmallMinion_02 = new AssetReference("VO_DALA_BOSS_99h_Male_Human_RecruitSmallMinion_02.prefab:eb000d8de28cd6d478b9a718ebe1fd9e");

	private static readonly AssetReference VO_DALA_BOSS_99h_Male_Human_RecruitLargeMinion_03 = new AssetReference("VO_DALA_BOSS_99h_Male_Human_RecruitLargeMinion_03.prefab:907297373eaf42d44b8fa703fd3cce1d");

	private static readonly AssetReference VO_PVPDR_Hero_Scabbs_Male_Gnome_Death_01 = new AssetReference("VO_PVPDR_Hero_Scabbs_Male_Gnome_Death_01.prefab:85e10a0b219b1974aaa959bd523c554b");

	private List<string> m_VO_Story_Hero_Bob_Male_Human_BOM_Scabbs_Mission1BeastLines = new List<string> { VO_Story_Hero_Bob_Male_Human_BOM_Scabbs_Mission1Beast_01, VO_Story_Hero_Bob_Male_Human_BOM_Scabbs_Mission1Beast_02 };

	private List<string> m_VO_Story_Hero_Bob_Male_Human_BOM_Scabbs_Mission1DemonLines = new List<string> { VO_Story_Hero_Bob_Male_Human_BOM_Scabbs_Mission1Demon_01, VO_Story_Hero_Bob_Male_Human_BOM_Scabbs_Mission1Demon_02 };

	private List<string> m_VO_Story_Hero_Bob_Male_Human_BOM_Scabbs_Mission1DragonLines = new List<string> { VO_Story_Hero_Bob_Male_Human_BOM_Scabbs_Mission1Dragon_01, VO_Story_Hero_Bob_Male_Human_BOM_Scabbs_Mission1Dragon_02 };

	private List<string> m_VO_Story_Hero_Bob_Male_Human_BOM_Scabbs_Mission1ElementalLines = new List<string> { VO_Story_Hero_Bob_Male_Human_BOM_Scabbs_Mission1Elemental_01, VO_Story_Hero_Bob_Male_Human_BOM_Scabbs_Mission1Elemental_02 };

	private List<string> m_VO_Story_Hero_Bob_Male_Human_BOM_Scabbs_Mission1MechLines = new List<string> { VO_Story_Hero_Bob_Male_Human_BOM_Scabbs_Mission1Mech_01, VO_Story_Hero_Bob_Male_Human_BOM_Scabbs_Mission1Mech_02 };

	private List<string> m_VO_Story_Hero_Bob_Male_Human_BOM_Scabbs_Mission1MurlocLines = new List<string> { VO_Story_Hero_Bob_Male_Human_BOM_Scabbs_Mission1Murloc_01, VO_Story_Hero_Bob_Male_Human_BOM_Scabbs_Mission1Murloc_02 };

	private List<string> m_VO_Story_Hero_Bob_Male_Human_BOM_Scabbs_Mission1PirateLines = new List<string> { VO_Story_Hero_Bob_Male_Human_BOM_Scabbs_Mission1Pirate_01, VO_Story_Hero_Bob_Male_Human_BOM_Scabbs_Mission1Pirate_02 };

	private List<string> m_VO_Story_Hero_Bob_Male_Human_BOM_Scabbs_Mission1QuilboarLines = new List<string> { VO_Story_Hero_Bob_Male_Human_BOM_Scabbs_Mission1Quilboar_01, VO_Story_Hero_Bob_Male_Human_BOM_Scabbs_Mission1Quilboar_02 };

	private List<string> m_InGame_BossUsesHeroPowerLines = new List<string> { VO_DALA_BOSS_99h_Male_Human_RecruitSmallMinion_01, VO_DALA_BOSS_99h_Male_Human_RecruitSmallMinion_02, VO_DALA_BOSS_99h_Male_Human_RecruitLargeMinion_03 };

	private List<string> m_InGame_BossIdleLines = new List<string> { VO_Story_Hero_Bob_Male_Human_BOM_Scabbs_Mission1Idle_01, VO_Story_Hero_Bob_Male_Human_BOM_Scabbs_Mission1Idle_02, VO_Story_Hero_Bob_Male_Human_BOM_Scabbs_Mission1Idle_03 };

	private List<string> m_InGame_VictoryPreExplosionLines = new List<string> { VO_Story_Hero_Bob_Male_Human_BOM_Scabbs_Mission1Victory_01, VO_Story_Hero_Bob_Male_Human_BOM_Scabbs_Mission1Victory_03, VO_Story_Hero_Bob_Male_Human_BOM_Scabbs_Mission1Victory_05, VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission1Victory_02, VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission1Victory_04 };

	private HashSet<string> m_playedLines = new HashSet<string>();

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> m_SoundFiles = new List<string>
		{
			VO_Story_Hero_Bob_Male_Human_BOM_Scabbs_Mission1Beast_01, VO_Story_Hero_Bob_Male_Human_BOM_Scabbs_Mission1Beast_02, VO_Story_Hero_Bob_Male_Human_BOM_Scabbs_Mission1Death_01, VO_Story_Hero_Bob_Male_Human_BOM_Scabbs_Mission1Demon_01, VO_Story_Hero_Bob_Male_Human_BOM_Scabbs_Mission1Demon_02, VO_Story_Hero_Bob_Male_Human_BOM_Scabbs_Mission1Dragon_01, VO_Story_Hero_Bob_Male_Human_BOM_Scabbs_Mission1Dragon_02, VO_Story_Hero_Bob_Male_Human_BOM_Scabbs_Mission1Elemental_01, VO_Story_Hero_Bob_Male_Human_BOM_Scabbs_Mission1Elemental_02, VO_Story_Hero_Bob_Male_Human_BOM_Scabbs_Mission1EmoteResponse_01,
			VO_Story_Hero_Bob_Male_Human_BOM_Scabbs_Mission1ExchangeA_01, VO_Story_Hero_Bob_Male_Human_BOM_Scabbs_Mission1ExchangeA_03, VO_Story_Hero_Bob_Male_Human_BOM_Scabbs_Mission1ExchangeB_02, VO_Story_Hero_Bob_Male_Human_BOM_Scabbs_Mission1ExchangeC_02, VO_DALA_BOSS_99h_Male_Human_RecruitSmallMinion_01, VO_DALA_BOSS_99h_Male_Human_RecruitSmallMinion_02, VO_DALA_BOSS_99h_Male_Human_RecruitLargeMinion_03, VO_Story_Hero_Bob_Male_Human_BOM_Scabbs_Mission1Idle_01, VO_Story_Hero_Bob_Male_Human_BOM_Scabbs_Mission1Idle_02, VO_Story_Hero_Bob_Male_Human_BOM_Scabbs_Mission1Idle_03,
			VO_Story_Hero_Bob_Male_Human_BOM_Scabbs_Mission1Intro_01, VO_Story_Hero_Bob_Male_Human_BOM_Scabbs_Mission1Loss_01, VO_Story_Hero_Bob_Male_Human_BOM_Scabbs_Mission1LowHealth_01, VO_Story_Hero_Bob_Male_Human_BOM_Scabbs_Mission1Mech_01, VO_Story_Hero_Bob_Male_Human_BOM_Scabbs_Mission1Mech_02, VO_Story_Hero_Bob_Male_Human_BOM_Scabbs_Mission1Murloc_01, VO_Story_Hero_Bob_Male_Human_BOM_Scabbs_Mission1Murloc_02, VO_Story_Hero_Bob_Male_Human_BOM_Scabbs_Mission1Pirate_01, VO_Story_Hero_Bob_Male_Human_BOM_Scabbs_Mission1Pirate_02, VO_Story_Hero_Bob_Male_Human_BOM_Scabbs_Mission1Quilboar_01,
			VO_Story_Hero_Bob_Male_Human_BOM_Scabbs_Mission1Quilboar_02, VO_Story_Hero_Bob_Male_Human_BOM_Scabbs_Mission1Victory_01, VO_Story_Hero_Bob_Male_Human_BOM_Scabbs_Mission1Victory_03, VO_Story_Hero_Bob_Male_Human_BOM_Scabbs_Mission1Victory_05, VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission1ExchangeA_02, VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission1ExchangeB_01, VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission1ExchangeC_01, VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission1Intro_02, VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission1Victory_02, VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission1Victory_04,
			VO_PVPDR_Hero_Scabbs_Male_Gnome_Death_01
		};
		SetBossVOLines(m_SoundFiles);
		foreach (string soundFile in m_SoundFiles)
		{
			PreloadSound(soundFile);
		}
	}

	public override void OnCreateGame()
	{
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
		switch (missionEvent)
		{
		case 516:
			yield return MissionPlaySound(enemyActor, VO_Story_Hero_Bob_Male_Human_BOM_Scabbs_Mission1Death_01);
			break;
		case 519:
			yield return MissionPlaySound(enemyActor, VO_PVPDR_Hero_Scabbs_Male_Gnome_Death_01);
			break;
		case 517:
			yield return MissionPlayVO(enemyActor, m_InGame_BossIdleLines);
			break;
		case 510:
			yield return MissionPlayVO(enemyActor, m_InGame_BossUsesHeroPowerLines);
			break;
		case 515:
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Bob_Male_Human_BOM_Scabbs_Mission1EmoteResponse_01);
			break;
		case 514:
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Bob_Male_Human_BOM_Scabbs_Mission1Intro_01);
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission1Intro_02);
			break;
		case 507:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Bob_Male_Human_BOM_Scabbs_Mission1Loss_01);
			GameState.Get().SetBusy(busy: false);
			break;
		case 504:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission1Victory_04);
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Bob_Male_Human_BOM_Scabbs_Mission1Victory_05);
			GameState.Get().SetBusy(busy: false);
			break;
		case 501:
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Bob_Male_Human_BOM_Scabbs_Mission1LowHealth_01);
			break;
		case 102:
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Bob_Male_Human_BOM_Scabbs_Mission1Victory_01);
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission1Victory_02);
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Bob_Male_Human_BOM_Scabbs_Mission1Victory_03);
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
			Actor enemyActor = GameState.Get().GetOpposingSidePlayer().GetHero()
				.GetCard()
				.GetActor();
			GameState.Get().GetFriendlySidePlayer().GetHero()
				.GetCard()
				.GetActor();
			switch (cardID)
			{
			case "FP1_010":
			case "CORE_EX1_534":
			case "CFM_316":
				yield return MissionPlayVOOnce(enemyActor, m_VO_Story_Hero_Bob_Male_Human_BOM_Scabbs_Mission1BeastLines);
				break;
			case "GVG_062":
			case "ICC_029":
				yield return MissionPlayVOOnce(enemyActor, m_VO_Story_Hero_Bob_Male_Human_BOM_Scabbs_Mission1DragonLines);
				break;
			case "YOD_026":
			case "DMF_533":
			case "LOOT_368":
				yield return MissionPlayVOOnce(enemyActor, m_VO_Story_Hero_Bob_Male_Human_BOM_Scabbs_Mission1DemonLines);
				break;
			case "BOT_537":
			case "BOT_312":
				yield return MissionPlayVOOnce(enemyActor, m_VO_Story_Hero_Bob_Male_Human_BOM_Scabbs_Mission1MechLines);
				break;
			case "EX1_062":
			case "DAL_077":
			case "CORE_EX1_506":
			case "CORE_EX1_103":
				yield return MissionPlayVOOnce(enemyActor, m_VO_Story_Hero_Bob_Male_Human_BOM_Scabbs_Mission1MurlocLines);
				break;
			case "CORE_NEW1_027":
			case "BOM_07_Scallywag_001t":
				yield return MissionPlayVOOnce(enemyActor, m_VO_Story_Hero_Bob_Male_Human_BOM_Scabbs_Mission1PirateLines);
				break;
			case "BOM_07_Bonker_001t":
			case "BOM_07_Roadboar_001t":
				yield return MissionPlayVOOnce(enemyActor, m_VO_Story_Hero_Bob_Male_Human_BOM_Scabbs_Mission1QuilboarLines);
				break;
			}
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
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Bob_Male_Human_BOM_Scabbs_Mission1ExchangeA_01);
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission1ExchangeA_02);
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Bob_Male_Human_BOM_Scabbs_Mission1ExchangeA_03);
			break;
		case 5:
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission1ExchangeB_01);
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Bob_Male_Human_BOM_Scabbs_Mission1ExchangeB_02);
			break;
		case 7:
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission1ExchangeC_01);
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Bob_Male_Human_BOM_Scabbs_Mission1ExchangeC_02);
			break;
		}
	}
}
