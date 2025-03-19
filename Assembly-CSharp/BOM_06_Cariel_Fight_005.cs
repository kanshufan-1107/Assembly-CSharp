using System.Collections;
using System.Collections.Generic;

public class BOM_06_Cariel_Fight_005 : BOM_06_Cariel_Dungeon
{
	private static readonly AssetReference VO_Story_Hero_Anetheron_Male_Demon_BOM_Cariel_Mission5Death_01 = new AssetReference("VO_Story_Hero_Anetheron_Male_Demon_BOM_Cariel_Mission5Death_01.prefab:6854dcc84ad110f4a8bf41f56756b7ad");

	private static readonly AssetReference VO_Story_Hero_Anetheron_Male_Demon_BOM_Cariel_Mission5EmoteResponse_01 = new AssetReference("VO_Story_Hero_Anetheron_Male_Demon_BOM_Cariel_Mission5EmoteResponse_01.prefab:9e4ff9851bb79424a89b9e1000e47548");

	private static readonly AssetReference VO_Story_Hero_Anetheron_Male_Demon_BOM_Cariel_Mission5ExchangeC_02 = new AssetReference("VO_Story_Hero_Anetheron_Male_Demon_BOM_Cariel_Mission5ExchangeC_02.prefab:16201a1a3d4e82246b7cbeb726c919c9");

	private static readonly AssetReference VO_Story_Hero_Anetheron_Male_Demon_BOM_Cariel_Mission5ExchangeE_02 = new AssetReference("VO_Story_Hero_Anetheron_Male_Demon_BOM_Cariel_Mission5ExchangeE_02.prefab:7c68ddbddcda0d045968f27120383ff1");

	private static readonly AssetReference VO_Story_Hero_Anetheron_Male_Demon_BOM_Cariel_Mission5HeroPower_01 = new AssetReference("VO_Story_Hero_Anetheron_Male_Demon_BOM_Cariel_Mission5HeroPower_01.prefab:d8a63eafe858afa47820853e77aa0719");

	private static readonly AssetReference VO_Story_Hero_Anetheron_Male_Demon_BOM_Cariel_Mission5HeroPower_02 = new AssetReference("VO_Story_Hero_Anetheron_Male_Demon_BOM_Cariel_Mission5HeroPower_02.prefab:438dcdeb7dcf3d64ab1c66e557776252");

	private static readonly AssetReference VO_Story_Hero_Anetheron_Male_Demon_BOM_Cariel_Mission5HeroPower_03 = new AssetReference("VO_Story_Hero_Anetheron_Male_Demon_BOM_Cariel_Mission5HeroPower_03.prefab:abf2d5f070a9cf74e9dc194f906f4a24");

	private static readonly AssetReference VO_Story_Hero_Anetheron_Male_Demon_BOM_Cariel_Mission5HeroPower_04 = new AssetReference("VO_Story_Hero_Anetheron_Male_Demon_BOM_Cariel_Mission5HeroPower_04.prefab:c843c5c78a09ddb44859f1db76d6104d");

	private static readonly AssetReference VO_Story_Hero_Anetheron_Male_Demon_BOM_Cariel_Mission5Idle_01 = new AssetReference("VO_Story_Hero_Anetheron_Male_Demon_BOM_Cariel_Mission5Idle_01.prefab:695afe7e4c549fe408624e289c92db8b");

	private static readonly AssetReference VO_Story_Hero_Anetheron_Male_Demon_BOM_Cariel_Mission5Idle_02 = new AssetReference("VO_Story_Hero_Anetheron_Male_Demon_BOM_Cariel_Mission5Idle_02.prefab:d6bbbc481b08c9a49bfa92b7152ce71e");

	private static readonly AssetReference VO_Story_Hero_Anetheron_Male_Demon_BOM_Cariel_Mission5Idle_03 = new AssetReference("VO_Story_Hero_Anetheron_Male_Demon_BOM_Cariel_Mission5Idle_03.prefab:88ea2646a9ca873419bde510f9d139ec");

	private static readonly AssetReference VO_Story_Hero_Anetheron_Male_Demon_BOM_Cariel_Mission5Intro_02 = new AssetReference("VO_Story_Hero_Anetheron_Male_Demon_BOM_Cariel_Mission5Intro_02.prefab:a1dbf53a029cea94f9c920d58cf602bd");

	private static readonly AssetReference VO_Story_Hero_Anetheron_Male_Demon_BOM_Cariel_Mission5Loss_01 = new AssetReference("VO_Story_Hero_Anetheron_Male_Demon_BOM_Cariel_Mission5Loss_01.prefab:9fd045005a4d050458e6405c22ab62be");

	private static readonly AssetReference VO_Story_Hero_Anetheron_Male_Demon_BOM_Cariel_Mission5Victory_03 = new AssetReference("VO_Story_Hero_Anetheron_Male_Demon_BOM_Cariel_Mission5Victory_03.prefab:b3534b3a79f81574391c91ba1919e066");

	private static readonly AssetReference VO_Story_Hero_Cariel_Female_Human_BOM_Cariel_Mission5ExchangeA_01 = new AssetReference("VO_Story_Hero_Cariel_Female_Human_BOM_Cariel_Mission5ExchangeA_01.prefab:ccb5bdeed351f164ab65685b294dfb54");

	private static readonly AssetReference VO_Story_Hero_Cariel_Female_Human_BOM_Cariel_Mission5ExchangeB_02 = new AssetReference("VO_Story_Hero_Cariel_Female_Human_BOM_Cariel_Mission5ExchangeB_02.prefab:d6ca04a2b51dd3747b06f5ba4f15fceb");

	private static readonly AssetReference VO_Story_Hero_Cariel_Female_Human_BOM_Cariel_Mission5ExchangeC_01 = new AssetReference("VO_Story_Hero_Cariel_Female_Human_BOM_Cariel_Mission5ExchangeC_01.prefab:bee63fad5a093f047bac2313a53d8051");

	private static readonly AssetReference VO_Story_Hero_Cariel_Female_Human_BOM_Cariel_Mission5ExchangeD_01 = new AssetReference("VO_Story_Hero_Cariel_Female_Human_BOM_Cariel_Mission5ExchangeD_01.prefab:ddcca0e74cb55b64dbf17981afe5d5c2");

	private static readonly AssetReference VO_Story_Hero_Cariel_Female_Human_BOM_Cariel_Mission5ExchangeF_01 = new AssetReference("VO_Story_Hero_Cariel_Female_Human_BOM_Cariel_Mission5ExchangeF_01.prefab:c54666675b8c11b4fb3685ac7f048551");

	private static readonly AssetReference VO_Story_Hero_Cariel_Female_Human_BOM_Cariel_Mission5ExchangeF_03 = new AssetReference("VO_Story_Hero_Cariel_Female_Human_BOM_Cariel_Mission5ExchangeF_03.prefab:b5056d1770b87dd42a9b2d570cfcd2d1");

	private static readonly AssetReference VO_Story_Hero_Cariel_Female_Human_BOM_Cariel_Mission5Intro_01 = new AssetReference("VO_Story_Hero_Cariel_Female_Human_BOM_Cariel_Mission5Intro_01.prefab:78a26d8c776d283498a9a7208835f880");

	private static readonly AssetReference VO_Story_Hero_Cariel_Female_Human_BOM_Cariel_Mission5Victory_01 = new AssetReference("VO_Story_Hero_Cariel_Female_Human_BOM_Cariel_Mission5Victory_01.prefab:bf11fb6b4ba05b24186bb679105ab784");

	private static readonly AssetReference VO_Story_Minion_Kurtrus_Male_NightElf_BOM_Cariel_Mission5ExchangeA_02 = new AssetReference("VO_Story_Minion_Kurtrus_Male_NightElf_BOM_Cariel_Mission5ExchangeA_02.prefab:26f3c19bdb0848d4ebb7c70a5c72c331");

	private static readonly AssetReference VO_Story_Minion_Kurtrus_Male_NightElf_BOM_Cariel_Mission5ExchangeB_01 = new AssetReference("VO_Story_Minion_Kurtrus_Male_NightElf_BOM_Cariel_Mission5ExchangeB_01.prefab:fa17389eddef8564c9ab84e54004498d");

	private static readonly AssetReference VO_Story_Minion_Kurtrus_Male_NightElf_BOM_Cariel_Mission5ExchangeD_02 = new AssetReference("VO_Story_Minion_Kurtrus_Male_NightElf_BOM_Cariel_Mission5ExchangeD_02.prefab:2d517779f49b60b459ab37929530cc16");

	private static readonly AssetReference VO_Story_Minion_Kurtrus_Male_NightElf_BOM_Cariel_Mission5ExchangeE_01 = new AssetReference("VO_Story_Minion_Kurtrus_Male_NightElf_BOM_Cariel_Mission5ExchangeE_01.prefab:ede33109b718a95418412dc766d41c50");

	private static readonly AssetReference VO_Story_Minion_Kurtrus_Male_NightElf_BOM_Cariel_Mission5ExchangeF_02 = new AssetReference("VO_Story_Minion_Kurtrus_Male_NightElf_BOM_Cariel_Mission5ExchangeF_02.prefab:7bf0b501ca442584db6340fce684baea");

	private static readonly AssetReference VO_Story_Minion_Kurtrus_Male_NightElf_BOM_Cariel_Mission5Victory_02 = new AssetReference("VO_Story_Minion_Kurtrus_Male_NightElf_BOM_Cariel_Mission5Victory_02.prefab:4c99a0ba7eb14be4e9c6fbc9e7a1bb7f");

	private static readonly AssetReference VO_Story_Hero_BigAnetheron_Male_Demon_BOM_Cariel_Carrion_01 = new AssetReference("VO_Story_Hero_BigAnetheron_Male_Demon_BOM_Cariel_Carrion_01.prefab:b3b39d9ff0b13ca4683864fb9b0e728d");

	private static readonly AssetReference VO_Story_Hero_BigAnetheron_Male_Demon_BOM_Cariel_Carrion_02 = new AssetReference("VO_Story_Hero_BigAnetheron_Male_Demon_BOM_Cariel_Carrion_02.prefab:4dd266d52ea862345a29e9d383623eab");

	private static readonly AssetReference VO_Story_Hero_BigAnetheron_Male_Demon_BOM_Cariel_Carrion_03 = new AssetReference("VO_Story_Hero_BigAnetheron_Male_Demon_BOM_Cariel_Carrion_03.prefab:c10c8e32471fb5841b88a940dfb15820");

	private static readonly AssetReference VO_Story_Hero_BigAnetheron_Male_Demon_BOM_Cariel_Inferno_01 = new AssetReference("VO_Story_Hero_BigAnetheron_Male_Demon_BOM_Cariel_Inferno_01.prefab:c4c562fe5af56494386db9c71632cff5");

	private static readonly AssetReference VO_Story_Hero_BigAnetheron_Male_Demon_BOM_Cariel_Inferno_02 = new AssetReference("VO_Story_Hero_BigAnetheron_Male_Demon_BOM_Cariel_Inferno_02.prefab:92c45ef64084f82468c7b957d8e8c689");

	private static readonly AssetReference VO_Story_Hero_BigAnetheron_Male_Demon_BOM_Cariel_Inferno_03 = new AssetReference("VO_Story_Hero_BigAnetheron_Male_Demon_BOM_Cariel_Inferno_03.prefab:4288c07931c87234dbdf6bfa76a22a8d");

	private static readonly AssetReference VO_Story_Hero_BigAnetheron_Male_Demon_BOM_Cariel_Sleep_01 = new AssetReference("VO_Story_Hero_BigAnetheron_Male_Demon_BOM_Cariel_Sleep_01.prefab:bf985207a0dc2cf408710359e5d91abc");

	private static readonly AssetReference VO_Story_Hero_BigAnetheron_Male_Demon_BOM_Cariel_Sleep_02 = new AssetReference("VO_Story_Hero_BigAnetheron_Male_Demon_BOM_Cariel_Sleep_02.prefab:acde37bdb71c9664ab9947647812af17");

	private static readonly AssetReference VO_Story_Hero_BigAnetheron_Male_Demon_BOM_Cariel_Sleep_03 = new AssetReference("VO_Story_Hero_BigAnetheron_Male_Demon_BOM_Cariel_Sleep_03.prefab:f065fa035b5f05748804fadff6a46692");

	private static readonly AssetReference VO_PVPDR_Hero_Cariel_Female_Human_Death_01 = new AssetReference("VO_PVPDR_Hero_Cariel_Female_Human_Death_01.prefab:523b6a53885a78244ab354f929dbd2b0");

	private List<string> m_InGame_BossUsesHeroPowerLines = new List<string> { VO_Story_Hero_Anetheron_Male_Demon_BOM_Cariel_Mission5HeroPower_01, VO_Story_Hero_Anetheron_Male_Demon_BOM_Cariel_Mission5HeroPower_02, VO_Story_Hero_Anetheron_Male_Demon_BOM_Cariel_Mission5HeroPower_03, VO_Story_Hero_Anetheron_Male_Demon_BOM_Cariel_Mission5HeroPower_04 };

	private List<string> m_InGame_BossIdleLines = new List<string> { VO_Story_Hero_Anetheron_Male_Demon_BOM_Cariel_Mission5Idle_01, VO_Story_Hero_Anetheron_Male_Demon_BOM_Cariel_Mission5Idle_02, VO_Story_Hero_Anetheron_Male_Demon_BOM_Cariel_Mission5Idle_03 };

	private HashSet<string> m_playedLines = new HashSet<string>();

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> m_SoundFiles = new List<string>
		{
			VO_Story_Hero_Anetheron_Male_Demon_BOM_Cariel_Mission5Death_01, VO_Story_Hero_Anetheron_Male_Demon_BOM_Cariel_Mission5EmoteResponse_01, VO_Story_Hero_Anetheron_Male_Demon_BOM_Cariel_Mission5ExchangeC_02, VO_Story_Hero_Anetheron_Male_Demon_BOM_Cariel_Mission5ExchangeE_02, VO_Story_Hero_Anetheron_Male_Demon_BOM_Cariel_Mission5HeroPower_01, VO_Story_Hero_Anetheron_Male_Demon_BOM_Cariel_Mission5HeroPower_02, VO_Story_Hero_Anetheron_Male_Demon_BOM_Cariel_Mission5HeroPower_03, VO_Story_Hero_Anetheron_Male_Demon_BOM_Cariel_Mission5HeroPower_04, VO_Story_Hero_Anetheron_Male_Demon_BOM_Cariel_Mission5Idle_01, VO_Story_Hero_Anetheron_Male_Demon_BOM_Cariel_Mission5Idle_02,
			VO_Story_Hero_Anetheron_Male_Demon_BOM_Cariel_Mission5Idle_03, VO_Story_Hero_Anetheron_Male_Demon_BOM_Cariel_Mission5Intro_02, VO_Story_Hero_Anetheron_Male_Demon_BOM_Cariel_Mission5Loss_01, VO_Story_Hero_Anetheron_Male_Demon_BOM_Cariel_Mission5Victory_03, VO_Story_Hero_Cariel_Female_Human_BOM_Cariel_Mission5ExchangeA_01, VO_Story_Hero_Cariel_Female_Human_BOM_Cariel_Mission5ExchangeB_02, VO_Story_Hero_Cariel_Female_Human_BOM_Cariel_Mission5ExchangeC_01, VO_Story_Hero_Cariel_Female_Human_BOM_Cariel_Mission5ExchangeD_01, VO_Story_Hero_Cariel_Female_Human_BOM_Cariel_Mission5ExchangeF_01, VO_Story_Hero_Cariel_Female_Human_BOM_Cariel_Mission5ExchangeF_03,
			VO_Story_Hero_Cariel_Female_Human_BOM_Cariel_Mission5Intro_01, VO_Story_Hero_Cariel_Female_Human_BOM_Cariel_Mission5Victory_01, VO_Story_Minion_Kurtrus_Male_NightElf_BOM_Cariel_Mission5ExchangeA_02, VO_Story_Minion_Kurtrus_Male_NightElf_BOM_Cariel_Mission5ExchangeB_01, VO_Story_Minion_Kurtrus_Male_NightElf_BOM_Cariel_Mission5ExchangeD_02, VO_Story_Minion_Kurtrus_Male_NightElf_BOM_Cariel_Mission5ExchangeE_01, VO_Story_Minion_Kurtrus_Male_NightElf_BOM_Cariel_Mission5ExchangeF_02, VO_Story_Minion_Kurtrus_Male_NightElf_BOM_Cariel_Mission5Victory_02, VO_Story_Hero_BigAnetheron_Male_Demon_BOM_Cariel_Carrion_01, VO_Story_Hero_BigAnetheron_Male_Demon_BOM_Cariel_Carrion_02,
			VO_Story_Hero_BigAnetheron_Male_Demon_BOM_Cariel_Carrion_03, VO_Story_Hero_BigAnetheron_Male_Demon_BOM_Cariel_Inferno_01, VO_Story_Hero_BigAnetheron_Male_Demon_BOM_Cariel_Inferno_02, VO_Story_Hero_BigAnetheron_Male_Demon_BOM_Cariel_Inferno_03, VO_Story_Hero_BigAnetheron_Male_Demon_BOM_Cariel_Sleep_01, VO_Story_Hero_BigAnetheron_Male_Demon_BOM_Cariel_Sleep_02, VO_Story_Hero_BigAnetheron_Male_Demon_BOM_Cariel_Sleep_03, VO_PVPDR_Hero_Cariel_Female_Human_Death_01
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
		m_OverrideMusicTrack = MusicPlaylistType.InGame_DRG;
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
		Actor friendlyHeroPowerActor = GameState.Get().GetFriendlySidePlayer().GetHeroPower()
			.GetCard()
			.GetActor();
		switch (missionEvent)
		{
		case 516:
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Anetheron_Male_Demon_BOM_Cariel_Mission5Death_01);
			break;
		case 519:
			yield return MissionPlaySound(enemyActor, VO_PVPDR_Hero_Cariel_Female_Human_Death_01);
			break;
		case 517:
			yield return MissionPlayVO(enemyActor, m_InGame_BossIdleLines);
			break;
		case 510:
			yield return MissionPlayVO(enemyActor, m_InGame_BossUsesHeroPowerLines);
			break;
		case 515:
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Anetheron_Male_Demon_BOM_Cariel_Mission5EmoteResponse_01);
			break;
		case 514:
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Cariel_Female_Human_BOM_Cariel_Mission5Intro_01);
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Anetheron_Male_Demon_BOM_Cariel_Mission5Intro_02);
			break;
		case 507:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Anetheron_Male_Demon_BOM_Cariel_Mission5Loss_01);
			GameState.Get().SetBusy(busy: false);
			break;
		case 504:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Cariel_Female_Human_BOM_Cariel_Mission5Victory_01);
			yield return MissionPlayVO(friendlyHeroPowerActor, VO_Story_Minion_Kurtrus_Male_NightElf_BOM_Cariel_Mission5Victory_02);
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Anetheron_Male_Demon_BOM_Cariel_Mission5Victory_03);
			GameState.Get().SetBusy(busy: false);
			break;
		case 100:
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Cariel_Female_Human_BOM_Cariel_Mission5ExchangeC_01);
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Anetheron_Male_Demon_BOM_Cariel_Mission5ExchangeC_02);
			break;
		case 101:
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Cariel_Female_Human_BOM_Cariel_Mission5ExchangeD_01);
			yield return MissionPlayVO("BOM_06_LeftHandWithKurtrus_005", VO_Story_Minion_Kurtrus_Male_NightElf_BOM_Cariel_Mission5ExchangeD_02);
			break;
		case 102:
			yield return MissionPlayVO("BOM_06_LeftHandWithKurtrus_005", VO_Story_Minion_Kurtrus_Male_NightElf_BOM_Cariel_Mission5ExchangeE_01);
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Anetheron_Male_Demon_BOM_Cariel_Mission5ExchangeE_02);
			break;
		case 103:
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Cariel_Female_Human_BOM_Cariel_Mission5ExchangeF_01);
			yield return MissionPlayVO(friendlyHeroPowerActor, VO_Story_Minion_Kurtrus_Male_NightElf_BOM_Cariel_Mission5ExchangeF_02);
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Cariel_Female_Human_BOM_Cariel_Mission5ExchangeF_03);
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
			if (cardID == "BOM_06_BigAnetheron_006hb")
			{
				yield return MissionPlayVO(enemyActor, VO_Story_Hero_BigAnetheron_Male_Demon_BOM_Cariel_Carrion_01);
			}
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
		GameState.Get().GetFriendlySidePlayer().GetHeroPower()
			.GetCard()
			.GetActor();
		switch (turn)
		{
		case 3:
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Cariel_Female_Human_BOM_Cariel_Mission5ExchangeA_01);
			yield return MissionPlayVO("BOM_06_LeftHandWithKurtrus_005", VO_Story_Minion_Kurtrus_Male_NightElf_BOM_Cariel_Mission5ExchangeA_02);
			break;
		case 7:
			yield return MissionPlayVO("BOM_06_LeftHandWithKurtrus_005", VO_Story_Minion_Kurtrus_Male_NightElf_BOM_Cariel_Mission5ExchangeB_01);
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Cariel_Female_Human_BOM_Cariel_Mission5ExchangeB_02);
			break;
		}
	}
}
