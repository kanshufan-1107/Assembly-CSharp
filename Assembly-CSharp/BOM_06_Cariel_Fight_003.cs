using System.Collections;
using System.Collections.Generic;

public class BOM_06_Cariel_Fight_003 : BOM_06_Cariel_Dungeon
{
	private static readonly AssetReference VO_Story_Hero_Cariel_Female_Human_BOM_Cariel_Mission3ExchangeB_01 = new AssetReference("VO_Story_Hero_Cariel_Female_Human_BOM_Cariel_Mission3ExchangeB_01.prefab:50c261cad5c1bb740abe3f308caf79d1");

	private static readonly AssetReference VO_Story_Hero_Cariel_Female_Human_BOM_Cariel_Mission3ExchangeB_03 = new AssetReference("VO_Story_Hero_Cariel_Female_Human_BOM_Cariel_Mission3ExchangeB_03.prefab:8b39a0cfc30470e4892b387568400628");

	private static readonly AssetReference VO_Story_Hero_Cariel_Female_Human_BOM_Cariel_Mission3ExchangeC_02 = new AssetReference("VO_Story_Hero_Cariel_Female_Human_BOM_Cariel_Mission3ExchangeC_02.prefab:9694ef5bdc408e042b5e429c1cc18431");

	private static readonly AssetReference VO_Story_Hero_Cariel_Female_Human_BOM_Cariel_Mission3ExchangeD_01 = new AssetReference("VO_Story_Hero_Cariel_Female_Human_BOM_Cariel_Mission3ExchangeD_01.prefab:7d43534275829594ab5bd30939f0a06d");

	private static readonly AssetReference VO_Story_Hero_Cariel_Female_Human_BOM_Cariel_Mission3ExchangeD_02 = new AssetReference("VO_Story_Hero_Cariel_Female_Human_BOM_Cariel_Mission3ExchangeD_02.prefab:90acd4bee8f5c7a48baae30003ff6ff6");

	private static readonly AssetReference VO_Story_Hero_Cariel_Female_Human_BOM_Cariel_Mission3Intro_01 = new AssetReference("VO_Story_Hero_Cariel_Female_Human_BOM_Cariel_Mission3Intro_01.prefab:c82c8db8aa2f79f4c9cabf43dc393fff");

	private static readonly AssetReference VO_Story_Hero_Cariel_Female_Human_BOM_Cariel_Mission3Victory_02 = new AssetReference("VO_Story_Hero_Cariel_Female_Human_BOM_Cariel_Mission3Victory_02.prefab:721f220baae8c284baebd035968f6534");

	private static readonly AssetReference VO_Story_Hero_Cariel_Female_Human_BOM_Cariel_Mission3Victory_04 = new AssetReference("VO_Story_Hero_Cariel_Female_Human_BOM_Cariel_Mission3Victory_04.prefab:d779991e8eb1b2f4a9b1e5191a1e4209");

	private static readonly AssetReference VO_Story_Hero_Cornelius_Male_Human_BOM_Cariel_Mission3Death_01 = new AssetReference("VO_Story_Hero_Cornelius_Male_Human_BOM_Cariel_Mission3Death_01.prefab:18c24933ff3736a47bae7c3af0cf0ab5");

	private static readonly AssetReference VO_Story_Hero_Cornelius_Male_Human_BOM_Cariel_Mission3EmoteResponse_01 = new AssetReference("VO_Story_Hero_Cornelius_Male_Human_BOM_Cariel_Mission3EmoteResponse_01.prefab:7eec7fad79831eb4bb66cbf690378289");

	private static readonly AssetReference VO_Story_Hero_Cornelius_Male_Human_BOM_Cariel_Mission3ExchangeA_01 = new AssetReference("VO_Story_Hero_Cornelius_Male_Human_BOM_Cariel_Mission3ExchangeA_01.prefab:ec1ce988ee257024f966d6ade69d6ee7");

	private static readonly AssetReference VO_Story_Hero_Cornelius_Male_Human_BOM_Cariel_Mission3ExchangeB_02 = new AssetReference("VO_Story_Hero_Cornelius_Male_Human_BOM_Cariel_Mission3ExchangeB_02.prefab:725abe9c31fe82a4a901f2604994322e");

	private static readonly AssetReference VO_Story_Hero_Cornelius_Male_Human_BOM_Cariel_Mission3ExchangeC_01 = new AssetReference("VO_Story_Hero_Cornelius_Male_Human_BOM_Cariel_Mission3ExchangeC_01.prefab:e0d8b31b7ad27fe4ca313b188946f17b");

	private static readonly AssetReference VO_Story_Hero_Cornelius_Male_Human_BOM_Cariel_Mission3ExchangeC_03 = new AssetReference("VO_Story_Hero_Cornelius_Male_Human_BOM_Cariel_Mission3ExchangeC_03.prefab:5f7617aeb2da1ef4e84d6f88944f4139");

	private static readonly AssetReference VO_Story_Hero_Cornelius_Male_Human_BOM_Cariel_Mission3HeroPower_01 = new AssetReference("VO_Story_Hero_Cornelius_Male_Human_BOM_Cariel_Mission3HeroPower_01.prefab:1f3d828de9e9fee40acb1b969cec3fb4");

	private static readonly AssetReference VO_Story_Hero_Cornelius_Male_Human_BOM_Cariel_Mission3HeroPower_02 = new AssetReference("VO_Story_Hero_Cornelius_Male_Human_BOM_Cariel_Mission3HeroPower_02.prefab:7cbcdf35543074040a8593a5c37480af");

	private static readonly AssetReference VO_Story_Hero_Cornelius_Male_Human_BOM_Cariel_Mission3HeroPower_03 = new AssetReference("VO_Story_Hero_Cornelius_Male_Human_BOM_Cariel_Mission3HeroPower_03.prefab:e94d1f478cf201b4d98f38d32aed9812");

	private static readonly AssetReference VO_Story_Hero_Cornelius_Male_Human_BOM_Cariel_Mission3Idle_01 = new AssetReference("VO_Story_Hero_Cornelius_Male_Human_BOM_Cariel_Mission3Idle_01.prefab:8e7a201b345538940bfa98db1d1cc811");

	private static readonly AssetReference VO_Story_Hero_Cornelius_Male_Human_BOM_Cariel_Mission3Idle_02 = new AssetReference("VO_Story_Hero_Cornelius_Male_Human_BOM_Cariel_Mission3Idle_02.prefab:72e079e1eb0eb264d8a29111aef7d2e9");

	private static readonly AssetReference VO_Story_Hero_Cornelius_Male_Human_BOM_Cariel_Mission3Idle_03 = new AssetReference("VO_Story_Hero_Cornelius_Male_Human_BOM_Cariel_Mission3Idle_03.prefab:1328c4711e847884682d908c1e68cbcf");

	private static readonly AssetReference VO_Story_Hero_Cornelius_Male_Human_BOM_Cariel_Mission3Intro_02 = new AssetReference("VO_Story_Hero_Cornelius_Male_Human_BOM_Cariel_Mission3Intro_02.prefab:9684aa14e001a91459f8e4afc8fbf383");

	private static readonly AssetReference VO_Story_Hero_Cornelius_Male_Human_BOM_Cariel_Mission3Loss_01 = new AssetReference("VO_Story_Hero_Cornelius_Male_Human_BOM_Cariel_Mission3Loss_01.prefab:76cb5f88a86702547a512b916be72b8c");

	private static readonly AssetReference VO_Story_Hero_Cornelius_Male_Human_BOM_Cariel_Mission3Victory_01 = new AssetReference("VO_Story_Hero_Cornelius_Male_Human_BOM_Cariel_Mission3Victory_01.prefab:18f2bc79e32ccf64090f001a5fd6bddd");

	private static readonly AssetReference VO_Story_Hero_Cornelius_Male_Human_BOM_Cariel_Mission3Victory_03 = new AssetReference("VO_Story_Hero_Cornelius_Male_Human_BOM_Cariel_Mission3Victory_03.prefab:d809d842f049ddd4d840f2a2bd33f534");

	private static readonly AssetReference VO_PVPDR_Hero_Cariel_Female_Human_Death_01 = new AssetReference("VO_PVPDR_Hero_Cariel_Female_Human_Death_01.prefab:523b6a53885a78244ab354f929dbd2b0");

	private List<string> m_InGame_BossUsesHeroPowerLines = new List<string> { VO_Story_Hero_Cornelius_Male_Human_BOM_Cariel_Mission3HeroPower_01, VO_Story_Hero_Cornelius_Male_Human_BOM_Cariel_Mission3HeroPower_02, VO_Story_Hero_Cornelius_Male_Human_BOM_Cariel_Mission3HeroPower_03 };

	private List<string> m_InGame_BossIdleLines = new List<string> { VO_Story_Hero_Cornelius_Male_Human_BOM_Cariel_Mission3Idle_01, VO_Story_Hero_Cornelius_Male_Human_BOM_Cariel_Mission3Idle_02, VO_Story_Hero_Cornelius_Male_Human_BOM_Cariel_Mission3Idle_03 };

	private HashSet<string> m_playedLines = new HashSet<string>();

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> m_SoundFiles = new List<string>
		{
			VO_Story_Hero_Cariel_Female_Human_BOM_Cariel_Mission3ExchangeB_01, VO_Story_Hero_Cariel_Female_Human_BOM_Cariel_Mission3ExchangeB_03, VO_Story_Hero_Cariel_Female_Human_BOM_Cariel_Mission3ExchangeC_02, VO_Story_Hero_Cariel_Female_Human_BOM_Cariel_Mission3ExchangeD_01, VO_Story_Hero_Cariel_Female_Human_BOM_Cariel_Mission3ExchangeD_02, VO_Story_Hero_Cariel_Female_Human_BOM_Cariel_Mission3Intro_01, VO_Story_Hero_Cariel_Female_Human_BOM_Cariel_Mission3Victory_02, VO_Story_Hero_Cariel_Female_Human_BOM_Cariel_Mission3Victory_04, VO_Story_Hero_Cornelius_Male_Human_BOM_Cariel_Mission3Death_01, VO_Story_Hero_Cornelius_Male_Human_BOM_Cariel_Mission3EmoteResponse_01,
			VO_Story_Hero_Cornelius_Male_Human_BOM_Cariel_Mission3ExchangeA_01, VO_Story_Hero_Cornelius_Male_Human_BOM_Cariel_Mission3ExchangeB_02, VO_Story_Hero_Cornelius_Male_Human_BOM_Cariel_Mission3ExchangeC_01, VO_Story_Hero_Cornelius_Male_Human_BOM_Cariel_Mission3ExchangeC_03, VO_Story_Hero_Cornelius_Male_Human_BOM_Cariel_Mission3HeroPower_01, VO_Story_Hero_Cornelius_Male_Human_BOM_Cariel_Mission3HeroPower_02, VO_Story_Hero_Cornelius_Male_Human_BOM_Cariel_Mission3HeroPower_03, VO_Story_Hero_Cornelius_Male_Human_BOM_Cariel_Mission3Idle_01, VO_Story_Hero_Cornelius_Male_Human_BOM_Cariel_Mission3Idle_02, VO_Story_Hero_Cornelius_Male_Human_BOM_Cariel_Mission3Idle_03,
			VO_Story_Hero_Cornelius_Male_Human_BOM_Cariel_Mission3Intro_02, VO_Story_Hero_Cornelius_Male_Human_BOM_Cariel_Mission3Loss_01, VO_Story_Hero_Cornelius_Male_Human_BOM_Cariel_Mission3Victory_01, VO_Story_Hero_Cornelius_Male_Human_BOM_Cariel_Mission3Victory_03, VO_PVPDR_Hero_Cariel_Female_Human_Death_01
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
		m_OverrideMusicTrack = MusicPlaylistType.InGame_SW_Stockades;
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
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Cornelius_Male_Human_BOM_Cariel_Mission3EmoteResponse_01);
			break;
		case 514:
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Cariel_Female_Human_BOM_Cariel_Mission3Intro_01);
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Cornelius_Male_Human_BOM_Cariel_Mission3Intro_02);
			break;
		case 507:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Cornelius_Male_Human_BOM_Cariel_Mission3Loss_01);
			GameState.Get().SetBusy(busy: false);
			break;
		case 504:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Cornelius_Male_Human_BOM_Cariel_Mission3Victory_01);
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Cariel_Female_Human_BOM_Cariel_Mission3Victory_02);
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Cornelius_Male_Human_BOM_Cariel_Mission3Victory_03);
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Cariel_Female_Human_BOM_Cariel_Mission3Victory_04);
			GameState.Get().SetBusy(busy: false);
			break;
		default:
			yield return base.HandleMissionEventWithTiming(missionEvent);
			break;
		case 516:
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
		case 3:
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Cornelius_Male_Human_BOM_Cariel_Mission3ExchangeA_01);
			break;
		case 7:
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Cariel_Female_Human_BOM_Cariel_Mission3ExchangeB_01);
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Cornelius_Male_Human_BOM_Cariel_Mission3ExchangeB_02);
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Cariel_Female_Human_BOM_Cariel_Mission3ExchangeB_03);
			break;
		case 11:
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Cornelius_Male_Human_BOM_Cariel_Mission3ExchangeC_01);
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Cariel_Female_Human_BOM_Cariel_Mission3ExchangeC_02);
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Cornelius_Male_Human_BOM_Cariel_Mission3ExchangeC_03);
			break;
		case 15:
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Cariel_Female_Human_BOM_Cariel_Mission3ExchangeD_01);
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Cariel_Female_Human_BOM_Cariel_Mission3ExchangeD_02);
			break;
		}
	}
}
