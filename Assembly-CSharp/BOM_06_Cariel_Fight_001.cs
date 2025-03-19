using System.Collections;
using System.Collections.Generic;

public class BOM_06_Cariel_Fight_001 : BOM_06_Cariel_Dungeon
{
	private static readonly AssetReference VO_Story_Hero_Cornelius_Male_Human_BOM_Cariel_Mission1ExchangeB_01 = new AssetReference("VO_Story_Hero_Cornelius_Male_Human_BOM_Cariel_Mission1ExchangeB_01.prefab:cce033f9e1828664d9a8c02b96a7949e");

	private static readonly AssetReference VO_Story_Hero_Cornelius_Male_Human_BOM_Cariel_Mission1ExchangeC_02 = new AssetReference("VO_Story_Hero_Cornelius_Male_Human_BOM_Cariel_Mission1ExchangeC_02.prefab:1ea49dab7054d5e488e5eec133409cda");

	private static readonly AssetReference VO_Story_Hero_Cornelius_Male_Human_BOM_Cariel_Mission1Victory_03 = new AssetReference("VO_Story_Hero_Cornelius_Male_Human_BOM_Cariel_Mission1Victory_03.prefab:cc5fe224fb34dec40bd2e8e9d2aee45a");

	private static readonly AssetReference VO_Story_Hero_YoungCariel_Female_Human_BOM_Cariel_Mission1Death_01 = new AssetReference("VO_Story_Hero_YoungCariel_Female_Human_BOM_Cariel_Mission1Death_01.prefab:2238891356db80949993ef5b1dc09f03");

	private static readonly AssetReference VO_Story_Hero_YoungCariel_Female_Human_BOM_Cariel_Mission1ExchangeA_02 = new AssetReference("VO_Story_Hero_YoungCariel_Female_Human_BOM_Cariel_Mission1ExchangeA_02.prefab:f993c4ff9c1c2c34281d4578b23d151b");

	private static readonly AssetReference VO_Story_Hero_YoungCariel_Female_Human_BOM_Cariel_Mission1ExchangeB_02 = new AssetReference("VO_Story_Hero_YoungCariel_Female_Human_BOM_Cariel_Mission1ExchangeB_02.prefab:8249e6eb710f0a84ea2d6710c098b9b5");

	private static readonly AssetReference VO_Story_Hero_YoungCariel_Female_Human_BOM_Cariel_Mission1ExchangeD_01 = new AssetReference("VO_Story_Hero_YoungCariel_Female_Human_BOM_Cariel_Mission1ExchangeD_01.prefab:93b683d2b696f1148b97659cb206fe0b");

	private static readonly AssetReference VO_Story_Hero_YoungCariel_Female_Human_BOM_Cariel_Mission1ExchangeE_02 = new AssetReference("VO_Story_Hero_YoungCariel_Female_Human_BOM_Cariel_Mission1ExchangeE_02.prefab:910bd99551777bf499e6821210cb5c7d");

	private static readonly AssetReference VO_Story_Hero_YoungCariel_Female_Human_BOM_Cariel_Mission1Greetings_01 = new AssetReference("VO_Story_Hero_YoungCariel_Female_Human_BOM_Cariel_Mission1Greetings_01.prefab:ae9bc02d6a6ba7441b691a088fef0f52");

	private static readonly AssetReference VO_Story_Hero_YoungCariel_Female_Human_BOM_Cariel_Mission1Intro_01 = new AssetReference("VO_Story_Hero_YoungCariel_Female_Human_BOM_Cariel_Mission1Intro_01.prefab:8134a8eb871d61d47849e7f5fbbd69ee");

	private static readonly AssetReference VO_Story_Hero_YoungCariel_Female_Human_BOM_Cariel_Mission1Victory_01 = new AssetReference("VO_Story_Hero_YoungCariel_Female_Human_BOM_Cariel_Mission1Victory_01.prefab:a84741e162e435f418ea3ca2db0c90ae");

	private static readonly AssetReference VO_Story_Hero_YoungTamsin_Female_Human_BOM_Cariel_Mission1Death_01 = new AssetReference("VO_Story_Hero_YoungTamsin_Female_Human_BOM_Cariel_Mission1Death_01.prefab:aad75d4e6e0792944b1b9bf7ab2e7e25");

	private static readonly AssetReference VO_Story_Hero_YoungTamsin_Female_Human_BOM_Cariel_Mission1EmoteResponse_01 = new AssetReference("VO_Story_Hero_YoungTamsin_Female_Human_BOM_Cariel_Mission1EmoteResponse_01.prefab:536faea5b25aacc44b0d91d050bc4af2");

	private static readonly AssetReference VO_Story_Hero_YoungTamsin_Female_Human_BOM_Cariel_Mission1ExchangeA_01 = new AssetReference("VO_Story_Hero_YoungTamsin_Female_Human_BOM_Cariel_Mission1ExchangeA_01.prefab:204883a6729c67e48a39cb9c8bcf9871");

	private static readonly AssetReference VO_Story_Hero_YoungTamsin_Female_Human_BOM_Cariel_Mission1ExchangeC_01 = new AssetReference("VO_Story_Hero_YoungTamsin_Female_Human_BOM_Cariel_Mission1ExchangeC_01.prefab:6859fedfbf383ce4fb571a7f917ba07a");

	private static readonly AssetReference VO_Story_Hero_YoungTamsin_Female_Human_BOM_Cariel_Mission1ExchangeE_01 = new AssetReference("VO_Story_Hero_YoungTamsin_Female_Human_BOM_Cariel_Mission1ExchangeE_01.prefab:0d584320fdc83f34b8ab4c3a65a8a4be");

	private static readonly AssetReference VO_Story_Hero_YoungTamsin_Female_Human_BOM_Cariel_Mission1ExchangeF_01 = new AssetReference("VO_Story_Hero_YoungTamsin_Female_Human_BOM_Cariel_Mission1ExchangeF_01.prefab:5da7fd13840b5324ba9bf2adb6ed3cad");

	private static readonly AssetReference VO_Story_Hero_YoungTamsin_Female_Human_BOM_Cariel_Mission1HeroPower_01 = new AssetReference("VO_Story_Hero_YoungTamsin_Female_Human_BOM_Cariel_Mission1HeroPower_01.prefab:2930be644fcc7b045947531794b9ec17");

	private static readonly AssetReference VO_Story_Hero_YoungTamsin_Female_Human_BOM_Cariel_Mission1HeroPower_02 = new AssetReference("VO_Story_Hero_YoungTamsin_Female_Human_BOM_Cariel_Mission1HeroPower_02.prefab:d48b41dd840ba6246a867390b93a3cc6");

	private static readonly AssetReference VO_Story_Hero_YoungTamsin_Female_Human_BOM_Cariel_Mission1HeroPower_03 = new AssetReference("VO_Story_Hero_YoungTamsin_Female_Human_BOM_Cariel_Mission1HeroPower_03.prefab:919ee7023f9591f46a7f5a73927189a1");

	private static readonly AssetReference VO_Story_Hero_YoungTamsin_Female_Human_BOM_Cariel_Mission1HeroPower_04 = new AssetReference("VO_Story_Hero_YoungTamsin_Female_Human_BOM_Cariel_Mission1HeroPower_04.prefab:1d36f11db1ded024c9c890e8750b7501");

	private static readonly AssetReference VO_Story_Hero_YoungTamsin_Female_Human_BOM_Cariel_Mission1HeroPower_05 = new AssetReference("VO_Story_Hero_YoungTamsin_Female_Human_BOM_Cariel_Mission1HeroPower_05.prefab:3e71aef4286141d4ba9a78695258be1c");

	private static readonly AssetReference VO_Story_Hero_YoungTamsin_Female_Human_BOM_Cariel_Mission1HeroPower_06 = new AssetReference("VO_Story_Hero_YoungTamsin_Female_Human_BOM_Cariel_Mission1HeroPower_06.prefab:ccbdf9546ed42d84d8b9c1d8ef613418");

	private static readonly AssetReference VO_Story_Hero_YoungTamsin_Female_Human_BOM_Cariel_Mission1Idle_01 = new AssetReference("VO_Story_Hero_YoungTamsin_Female_Human_BOM_Cariel_Mission1Idle_01.prefab:4ce12dd9901e36e4093754b9da429de5");

	private static readonly AssetReference VO_Story_Hero_YoungTamsin_Female_Human_BOM_Cariel_Mission1Idle_02 = new AssetReference("VO_Story_Hero_YoungTamsin_Female_Human_BOM_Cariel_Mission1Idle_02.prefab:6127542aeff7c544b9880edc6ac56160");

	private static readonly AssetReference VO_Story_Hero_YoungTamsin_Female_Human_BOM_Cariel_Mission1Idle_03 = new AssetReference("VO_Story_Hero_YoungTamsin_Female_Human_BOM_Cariel_Mission1Idle_03.prefab:07bb3d1761693874480a919b3e55c69c");

	private static readonly AssetReference VO_Story_Hero_YoungTamsin_Female_Human_BOM_Cariel_Mission1Intro_02 = new AssetReference("VO_Story_Hero_YoungTamsin_Female_Human_BOM_Cariel_Mission1Intro_02.prefab:3ab2a18a61855fb4192dbacc31b979c2");

	private static readonly AssetReference VO_Story_Hero_YoungTamsin_Female_Human_BOM_Cariel_Mission1Loss_01 = new AssetReference("VO_Story_Hero_YoungTamsin_Female_Human_BOM_Cariel_Mission1Loss_01.prefab:329ade4a35385fa47898d396d4391aea");

	private static readonly AssetReference VO_Story_Hero_YoungTamsin_Female_Human_BOM_Cariel_Mission1Victory_02 = new AssetReference("VO_Story_Hero_YoungTamsin_Female_Human_BOM_Cariel_Mission1Victory_02.prefab:ac5d2f620cf16674c8f75ce29972d4e5");

	private List<string> m_InGame_BossUsesHeroPowerLines = new List<string> { VO_Story_Hero_YoungTamsin_Female_Human_BOM_Cariel_Mission1HeroPower_01, VO_Story_Hero_YoungTamsin_Female_Human_BOM_Cariel_Mission1HeroPower_02, VO_Story_Hero_YoungTamsin_Female_Human_BOM_Cariel_Mission1HeroPower_03, VO_Story_Hero_YoungTamsin_Female_Human_BOM_Cariel_Mission1HeroPower_04, VO_Story_Hero_YoungTamsin_Female_Human_BOM_Cariel_Mission1HeroPower_05, VO_Story_Hero_YoungTamsin_Female_Human_BOM_Cariel_Mission1HeroPower_06 };

	private List<string> m_InGame_BossIdleLines = new List<string> { VO_Story_Hero_YoungTamsin_Female_Human_BOM_Cariel_Mission1Idle_01, VO_Story_Hero_YoungTamsin_Female_Human_BOM_Cariel_Mission1Idle_02, VO_Story_Hero_YoungTamsin_Female_Human_BOM_Cariel_Mission1Idle_03 };

	private HashSet<string> m_playedLines = new HashSet<string>();

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> m_SoundFiles = new List<string>
		{
			VO_Story_Hero_YoungCariel_Female_Human_BOM_Cariel_Mission1Death_01, VO_Story_Hero_YoungCariel_Female_Human_BOM_Cariel_Mission1ExchangeA_02, VO_Story_Hero_YoungCariel_Female_Human_BOM_Cariel_Mission1ExchangeB_02, VO_Story_Hero_YoungCariel_Female_Human_BOM_Cariel_Mission1ExchangeD_01, VO_Story_Hero_YoungCariel_Female_Human_BOM_Cariel_Mission1ExchangeE_02, VO_Story_Hero_YoungCariel_Female_Human_BOM_Cariel_Mission1Intro_01, VO_Story_Hero_YoungCariel_Female_Human_BOM_Cariel_Mission1Victory_01, VO_Story_Hero_YoungTamsin_Female_Human_BOM_Cariel_Mission1Death_01, VO_Story_Hero_YoungTamsin_Female_Human_BOM_Cariel_Mission1EmoteResponse_01, VO_Story_Hero_YoungTamsin_Female_Human_BOM_Cariel_Mission1ExchangeA_01,
			VO_Story_Hero_YoungTamsin_Female_Human_BOM_Cariel_Mission1ExchangeC_01, VO_Story_Hero_YoungTamsin_Female_Human_BOM_Cariel_Mission1ExchangeE_01, VO_Story_Hero_YoungTamsin_Female_Human_BOM_Cariel_Mission1ExchangeF_01, VO_Story_Hero_YoungTamsin_Female_Human_BOM_Cariel_Mission1HeroPower_01, VO_Story_Hero_YoungTamsin_Female_Human_BOM_Cariel_Mission1HeroPower_02, VO_Story_Hero_YoungTamsin_Female_Human_BOM_Cariel_Mission1HeroPower_03, VO_Story_Hero_YoungTamsin_Female_Human_BOM_Cariel_Mission1HeroPower_04, VO_Story_Hero_YoungTamsin_Female_Human_BOM_Cariel_Mission1HeroPower_05, VO_Story_Hero_YoungTamsin_Female_Human_BOM_Cariel_Mission1HeroPower_06, VO_Story_Hero_YoungTamsin_Female_Human_BOM_Cariel_Mission1Idle_01,
			VO_Story_Hero_YoungTamsin_Female_Human_BOM_Cariel_Mission1Idle_02, VO_Story_Hero_YoungTamsin_Female_Human_BOM_Cariel_Mission1Idle_03, VO_Story_Hero_YoungTamsin_Female_Human_BOM_Cariel_Mission1Intro_02, VO_Story_Hero_YoungTamsin_Female_Human_BOM_Cariel_Mission1Loss_01, VO_Story_Hero_YoungTamsin_Female_Human_BOM_Cariel_Mission1Victory_02, VO_Story_Hero_Cornelius_Male_Human_BOM_Cariel_Mission1ExchangeB_01, VO_Story_Hero_Cornelius_Male_Human_BOM_Cariel_Mission1ExchangeC_02, VO_Story_Hero_Cornelius_Male_Human_BOM_Cariel_Mission1Victory_03
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
		m_OverrideMusicTrack = MusicPlaylistType.InGame_SW;
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
		case 517:
			yield return MissionPlayVO(enemyActor, m_InGame_BossIdleLines);
			break;
		case 510:
			yield return MissionPlayVO(enemyActor, m_InGame_BossUsesHeroPowerLines);
			break;
		case 515:
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_YoungTamsin_Female_Human_BOM_Cariel_Mission1EmoteResponse_01);
			break;
		case 514:
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_YoungCariel_Female_Human_BOM_Cariel_Mission1Intro_01);
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_YoungTamsin_Female_Human_BOM_Cariel_Mission1Intro_02);
			break;
		case 507:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_YoungTamsin_Female_Human_BOM_Cariel_Mission1Loss_01);
			GameState.Get().SetBusy(busy: false);
			break;
		case 519:
			yield return MissionPlaySound(friendlyActor, VO_Story_Hero_YoungCariel_Female_Human_BOM_Cariel_Mission1Death_01);
			break;
		case 504:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_YoungCariel_Female_Human_BOM_Cariel_Mission1Victory_01);
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_YoungTamsin_Female_Human_BOM_Cariel_Mission1Victory_02);
			yield return MissionPlayVO(YoungCornelius_BrassRing_Quote, VO_Story_Hero_Cornelius_Male_Human_BOM_Cariel_Mission1Victory_03);
			GameState.Get().SetBusy(busy: false);
			break;
		case 101:
			yield return MissionPlayVOOnce(enemyActor, VO_Story_Hero_YoungTamsin_Female_Human_BOM_Cariel_Mission1ExchangeE_01);
			yield return MissionPlayVOOnce(friendlyActor, VO_Story_Hero_YoungCariel_Female_Human_BOM_Cariel_Mission1ExchangeE_02);
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
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_YoungTamsin_Female_Human_BOM_Cariel_Mission1ExchangeA_01);
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_YoungCariel_Female_Human_BOM_Cariel_Mission1ExchangeA_02);
			break;
		case 7:
			yield return MissionPlayVO(YoungCornelius_BrassRing_Quote, VO_Story_Hero_Cornelius_Male_Human_BOM_Cariel_Mission1ExchangeB_01);
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_YoungCariel_Female_Human_BOM_Cariel_Mission1ExchangeB_02);
			break;
		case 11:
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_YoungTamsin_Female_Human_BOM_Cariel_Mission1ExchangeC_01);
			yield return MissionPlayVO(YoungCornelius_BrassRing_Quote, VO_Story_Hero_Cornelius_Male_Human_BOM_Cariel_Mission1ExchangeC_02);
			break;
		case 13:
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_YoungCariel_Female_Human_BOM_Cariel_Mission1ExchangeD_01);
			break;
		}
	}
}
