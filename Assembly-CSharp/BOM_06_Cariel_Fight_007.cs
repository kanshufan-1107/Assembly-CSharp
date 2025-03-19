using System.Collections;
using System.Collections.Generic;

public class BOM_06_Cariel_Fight_007 : BOM_06_Cariel_Dungeon
{
	private static readonly AssetReference VO_Story_Hero_Cariel_Female_Human_BOM_Cariel_Mission7ExchangeA_01 = new AssetReference("VO_Story_Hero_Cariel_Female_Human_BOM_Cariel_Mission7ExchangeA_01.prefab:69e651e1c4fafd548bacc8615029f4b0");

	private static readonly AssetReference VO_Story_Hero_Cariel_Female_Human_BOM_Cariel_Mission7ExchangeB_01 = new AssetReference("VO_Story_Hero_Cariel_Female_Human_BOM_Cariel_Mission7ExchangeB_01.prefab:d911d379a26a2574ebe8b8871c0199de");

	private static readonly AssetReference VO_Story_Hero_Cariel_Female_Human_BOM_Cariel_Mission7ExchangeC_02 = new AssetReference("VO_Story_Hero_Cariel_Female_Human_BOM_Cariel_Mission7ExchangeC_02.prefab:a06970381eb51f14aaf8d7dee18a5da6");

	private static readonly AssetReference VO_Story_Hero_Cariel_Female_Human_BOM_Cariel_Mission7ExchangeD_02 = new AssetReference("VO_Story_Hero_Cariel_Female_Human_BOM_Cariel_Mission7ExchangeD_02.prefab:a1f4eb027db9a2d46bbc33febe539845");

	private static readonly AssetReference VO_Story_Hero_Cariel_Female_Human_BOM_Cariel_Mission7Intro_01 = new AssetReference("VO_Story_Hero_Cariel_Female_Human_BOM_Cariel_Mission7Intro_01.prefab:02a8acca5adc8f54387a6d834feee5ea");

	private static readonly AssetReference VO_Story_Hero_Cariel_Female_Human_BOM_Cariel_Mission7Victory_01 = new AssetReference("VO_Story_Hero_Cariel_Female_Human_BOM_Cariel_Mission7Victory_01.prefab:810698467c684ec458b012c0162d3115");

	private static readonly AssetReference VO_Story_Hero_Monstrosity_Male_Undead_BOM_Cariel_Mission7Death_01 = new AssetReference("VO_Story_Hero_Monstrosity_Male_Undead_BOM_Cariel_Mission7Death_01.prefab:16c13c59e7bb6dd4a96fd60b971ee0df");

	private static readonly AssetReference VO_Story_Hero_Monstrosity_Male_Undead_BOM_Cariel_Mission7EmoteResponse_01 = new AssetReference("VO_Story_Hero_Monstrosity_Male_Undead_BOM_Cariel_Mission7EmoteResponse_01.prefab:602fb200ecb5abb48b18643993555666");

	private static readonly AssetReference VO_Story_Hero_Monstrosity_Male_Undead_BOM_Cariel_Mission7ExchangeA_02 = new AssetReference("VO_Story_Hero_Monstrosity_Male_Undead_BOM_Cariel_Mission7ExchangeA_02.prefab:5c52f58198c586445992c0fdaaaa6333");

	private static readonly AssetReference VO_Story_Hero_Monstrosity_Male_Undead_BOM_Cariel_Mission7ExchangeB_02 = new AssetReference("VO_Story_Hero_Monstrosity_Male_Undead_BOM_Cariel_Mission7ExchangeB_02.prefab:b6fbecb8daf13284eb44231093c195b1");

	private static readonly AssetReference VO_Story_Hero_Monstrosity_Male_Undead_BOM_Cariel_Mission7HeroPower_01 = new AssetReference("VO_Story_Hero_Monstrosity_Male_Undead_BOM_Cariel_Mission7HeroPower_01.prefab:a1c1c255261b96247b4de5791b8b2d6d");

	private static readonly AssetReference VO_Story_Hero_Monstrosity_Male_Undead_BOM_Cariel_Mission7HeroPower_02 = new AssetReference("VO_Story_Hero_Monstrosity_Male_Undead_BOM_Cariel_Mission7HeroPower_02.prefab:3129b48ce6abbf84f84f929cb5304e29");

	private static readonly AssetReference VO_Story_Hero_Monstrosity_Male_Undead_BOM_Cariel_Mission7HeroPower_03 = new AssetReference("VO_Story_Hero_Monstrosity_Male_Undead_BOM_Cariel_Mission7HeroPower_03.prefab:560049faf02e8ad48a5152816eff2058");

	private static readonly AssetReference VO_Story_Hero_Monstrosity_Male_Undead_BOM_Cariel_Mission7Idle_01 = new AssetReference("VO_Story_Hero_Monstrosity_Male_Undead_BOM_Cariel_Mission7Idle_01.prefab:94915c1c7a405594288026d3e8a4191d");

	private static readonly AssetReference VO_Story_Hero_Monstrosity_Male_Undead_BOM_Cariel_Mission7Idle_02 = new AssetReference("VO_Story_Hero_Monstrosity_Male_Undead_BOM_Cariel_Mission7Idle_02.prefab:dd7f182bb72765a43a83049178adf43a");

	private static readonly AssetReference VO_Story_Hero_Monstrosity_Male_Undead_BOM_Cariel_Mission7Idle_03 = new AssetReference("VO_Story_Hero_Monstrosity_Male_Undead_BOM_Cariel_Mission7Idle_03.prefab:efab505f55ce1774795a33af88ad552c");

	private static readonly AssetReference VO_Story_Hero_Monstrosity_Male_Undead_BOM_Cariel_Mission7Intro_02 = new AssetReference("VO_Story_Hero_Monstrosity_Male_Undead_BOM_Cariel_Mission7Intro_02.prefab:5532c255e2e447042aaab3f96a0cdf67");

	private static readonly AssetReference VO_Story_Hero_Monstrosity_Male_Undead_BOM_Cariel_Mission7Loss_01 = new AssetReference("VO_Story_Hero_Monstrosity_Male_Undead_BOM_Cariel_Mission7Loss_01.prefab:093495bbdef9d0a44b963de2fd18e2a2");

	private static readonly AssetReference VO_Story_Hero_Monstrosity_Male_Undead_BOM_Cariel_Mission7Victory_02 = new AssetReference("VO_Story_Hero_Monstrosity_Male_Undead_BOM_Cariel_Mission7Victory_02.prefab:dc97526a58b0be24c8ed5a90b5f47259");

	private static readonly AssetReference VO_Story_Hero_Tamsin_Female_Undead_BOM_Cariel_Mission7ExchangeC_01 = new AssetReference("VO_Story_Hero_Tamsin_Female_Undead_BOM_Cariel_Mission7ExchangeC_01.prefab:f77ebcb453d32004dae6e1d602c78539");

	private static readonly AssetReference VO_Story_Hero_Tamsin_Female_Undead_BOM_Cariel_Mission7ExchangeD_01 = new AssetReference("VO_Story_Hero_Tamsin_Female_Undead_BOM_Cariel_Mission7ExchangeD_01.prefab:b84c1985130f44243bf1918896a034fa");

	private static readonly AssetReference VO_PVPDR_Hero_Cariel_Female_Human_Death_01 = new AssetReference("VO_PVPDR_Hero_Cariel_Female_Human_Death_01.prefab:523b6a53885a78244ab354f929dbd2b0");

	private List<string> m_InGame_BossUsesHeroPowerLines = new List<string> { VO_Story_Hero_Monstrosity_Male_Undead_BOM_Cariel_Mission7HeroPower_01, VO_Story_Hero_Monstrosity_Male_Undead_BOM_Cariel_Mission7HeroPower_02, VO_Story_Hero_Monstrosity_Male_Undead_BOM_Cariel_Mission7HeroPower_03 };

	private List<string> m_InGame_BossIdleLines = new List<string> { VO_Story_Hero_Monstrosity_Male_Undead_BOM_Cariel_Mission7Idle_01, VO_Story_Hero_Monstrosity_Male_Undead_BOM_Cariel_Mission7Idle_02, VO_Story_Hero_Monstrosity_Male_Undead_BOM_Cariel_Mission7Idle_03 };

	private HashSet<string> m_playedLines = new HashSet<string>();

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> m_SoundFiles = new List<string>
		{
			VO_Story_Hero_Cariel_Female_Human_BOM_Cariel_Mission7ExchangeA_01, VO_Story_Hero_Cariel_Female_Human_BOM_Cariel_Mission7ExchangeB_01, VO_Story_Hero_Cariel_Female_Human_BOM_Cariel_Mission7ExchangeC_02, VO_Story_Hero_Cariel_Female_Human_BOM_Cariel_Mission7ExchangeD_02, VO_Story_Hero_Cariel_Female_Human_BOM_Cariel_Mission7Intro_01, VO_Story_Hero_Cariel_Female_Human_BOM_Cariel_Mission7Victory_01, VO_Story_Hero_Monstrosity_Male_Undead_BOM_Cariel_Mission7Death_01, VO_Story_Hero_Monstrosity_Male_Undead_BOM_Cariel_Mission7EmoteResponse_01, VO_Story_Hero_Monstrosity_Male_Undead_BOM_Cariel_Mission7ExchangeA_02, VO_Story_Hero_Monstrosity_Male_Undead_BOM_Cariel_Mission7ExchangeB_02,
			VO_Story_Hero_Monstrosity_Male_Undead_BOM_Cariel_Mission7HeroPower_01, VO_Story_Hero_Monstrosity_Male_Undead_BOM_Cariel_Mission7HeroPower_02, VO_Story_Hero_Monstrosity_Male_Undead_BOM_Cariel_Mission7HeroPower_03, VO_Story_Hero_Monstrosity_Male_Undead_BOM_Cariel_Mission7Idle_01, VO_Story_Hero_Monstrosity_Male_Undead_BOM_Cariel_Mission7Idle_02, VO_Story_Hero_Monstrosity_Male_Undead_BOM_Cariel_Mission7Idle_03, VO_Story_Hero_Monstrosity_Male_Undead_BOM_Cariel_Mission7Intro_02, VO_Story_Hero_Monstrosity_Male_Undead_BOM_Cariel_Mission7Loss_01, VO_Story_Hero_Monstrosity_Male_Undead_BOM_Cariel_Mission7Victory_02, VO_Story_Hero_Tamsin_Female_Undead_BOM_Cariel_Mission7ExchangeC_01,
			VO_Story_Hero_Tamsin_Female_Undead_BOM_Cariel_Mission7ExchangeD_01, VO_PVPDR_Hero_Cariel_Female_Human_Death_01
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
		case 516:
			yield return MissionPlaySound(enemyActor, VO_Story_Hero_Monstrosity_Male_Undead_BOM_Cariel_Mission7Death_01);
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
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Monstrosity_Male_Undead_BOM_Cariel_Mission7EmoteResponse_01);
			break;
		case 514:
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Cariel_Female_Human_BOM_Cariel_Mission7Intro_01);
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Monstrosity_Male_Undead_BOM_Cariel_Mission7Intro_02);
			break;
		case 507:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Monstrosity_Male_Undead_BOM_Cariel_Mission7Loss_01);
			GameState.Get().SetBusy(busy: false);
			break;
		case 504:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Cariel_Female_Human_BOM_Cariel_Mission7Victory_01);
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Monstrosity_Male_Undead_BOM_Cariel_Mission7Victory_02);
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
		Actor enemyActor = GameState.Get().GetOpposingSidePlayer().GetHero()
			.GetCard()
			.GetActor();
		Actor friendlyActor = GameState.Get().GetFriendlySidePlayer().GetHero()
			.GetCard()
			.GetActor();
		switch (turn)
		{
		case 1:
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Cariel_Female_Human_BOM_Cariel_Mission7ExchangeA_01);
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Monstrosity_Male_Undead_BOM_Cariel_Mission7ExchangeA_02);
			break;
		case 5:
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Cariel_Female_Human_BOM_Cariel_Mission7ExchangeB_01);
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Monstrosity_Male_Undead_BOM_Cariel_Mission7ExchangeB_02);
			break;
		case 9:
			yield return MissionPlayVO(Tamsin_BrassRing, VO_Story_Hero_Tamsin_Female_Undead_BOM_Cariel_Mission7ExchangeC_01);
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Cariel_Female_Human_BOM_Cariel_Mission7ExchangeC_02);
			break;
		case 11:
			yield return MissionPlayVO(Tamsin_BrassRing, VO_Story_Hero_Tamsin_Female_Undead_BOM_Cariel_Mission7ExchangeD_01);
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Cariel_Female_Human_BOM_Cariel_Mission7ExchangeD_02);
			break;
		}
	}
}
