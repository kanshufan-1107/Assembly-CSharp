using System.Collections;
using System.Collections.Generic;

public class BOM_06_Cariel_Fight_008 : BOM_06_Cariel_Dungeon
{
	private static readonly AssetReference VO_Story_Hero_Cariel_Female_Human_BOM_Cariel_Mission8ExchangeA_02 = new AssetReference("VO_Story_Hero_Cariel_Female_Human_BOM_Cariel_Mission8ExchangeA_02.prefab:229865b8355ed4c41b14b6f067d92367");

	private static readonly AssetReference VO_Story_Hero_Cariel_Female_Human_BOM_Cariel_Mission8ExchangeC_02 = new AssetReference("VO_Story_Hero_Cariel_Female_Human_BOM_Cariel_Mission8ExchangeC_02.prefab:41605f2279f34854cb5640b6c69288bd");

	private static readonly AssetReference VO_Story_Hero_Cariel_Female_Human_BOM_Cariel_Mission8ExchangeD_01 = new AssetReference("VO_Story_Hero_Cariel_Female_Human_BOM_Cariel_Mission8ExchangeD_01.prefab:796e7865a17dd9c4fa9a0139006b91ac");

	private static readonly AssetReference VO_Story_Hero_Cariel_Female_Human_BOM_Cariel_Mission8Intro_01 = new AssetReference("VO_Story_Hero_Cariel_Female_Human_BOM_Cariel_Mission8Intro_01.prefab:15432414ca01d19448b8e12313940d7c");

	private static readonly AssetReference VO_Story_Hero_Cariel_Female_Human_BOM_Cariel_Mission8Victory_02 = new AssetReference("VO_Story_Hero_Cariel_Female_Human_BOM_Cariel_Mission8Victory_02.prefab:19271e42a0c040f4d9f0bb9cafabc4c5");

	private static readonly AssetReference VO_Story_Hero_Tamsin_Female_Undead_BOM_Cariel_Mission8Death_01 = new AssetReference("VO_Story_Hero_Tamsin_Female_Undead_BOM_Cariel_Mission8Death_01.prefab:eae7cbeddd6518e45848242f109d8966");

	private static readonly AssetReference VO_Story_Hero_Tamsin_Female_Undead_BOM_Cariel_Mission8EmoteResponse_01 = new AssetReference("VO_Story_Hero_Tamsin_Female_Undead_BOM_Cariel_Mission8EmoteResponse_01.prefab:226873f727f533e4189e9eb1f074d5ef");

	private static readonly AssetReference VO_Story_Hero_Tamsin_Female_Undead_BOM_Cariel_Mission8ExchangeA_01 = new AssetReference("VO_Story_Hero_Tamsin_Female_Undead_BOM_Cariel_Mission8ExchangeA_01.prefab:99e947bebfad3d74ca668aedc6b9ba0d");

	private static readonly AssetReference VO_Story_Hero_Tamsin_Female_Undead_BOM_Cariel_Mission8ExchangeB_01 = new AssetReference("VO_Story_Hero_Tamsin_Female_Undead_BOM_Cariel_Mission8ExchangeB_01.prefab:8b264fd0a73443e4b85aa29efe0d94c4");

	private static readonly AssetReference VO_Story_Hero_Tamsin_Female_Undead_BOM_Cariel_Mission8ExchangeC_01 = new AssetReference("VO_Story_Hero_Tamsin_Female_Undead_BOM_Cariel_Mission8ExchangeC_01.prefab:fe0c5ab31b03e0e4f8fb1570d5d39a03");

	private static readonly AssetReference VO_Story_Hero_Tamsin_Female_Undead_BOM_Cariel_Mission8ExchangeC_03 = new AssetReference("VO_Story_Hero_Tamsin_Female_Undead_BOM_Cariel_Mission8ExchangeC_03.prefab:5c3d3f7279ebbdc4e855de6379ff021f");

	private static readonly AssetReference VO_Story_Hero_Tamsin_Female_Undead_BOM_Cariel_Mission8ExchangeD_02 = new AssetReference("VO_Story_Hero_Tamsin_Female_Undead_BOM_Cariel_Mission8ExchangeD_02.prefab:1ccc0b4707e5755428f4670ebc30c2d4");

	private static readonly AssetReference VO_Story_Hero_Tamsin_Female_Undead_BOM_Cariel_Mission8ExchangeE_01 = new AssetReference("VO_Story_Hero_Tamsin_Female_Undead_BOM_Cariel_Mission8ExchangeE_01.prefab:45f82ca562bbe474d9f38f2740182f4a");

	private static readonly AssetReference VO_Story_Hero_Tamsin_Female_Undead_BOM_Cariel_Mission8HeroPower_01 = new AssetReference("VO_Story_Hero_Tamsin_Female_Undead_BOM_Cariel_Mission8HeroPower_01.prefab:d6eef54561de9964d8e660e5bf57da21");

	private static readonly AssetReference VO_Story_Hero_Tamsin_Female_Undead_BOM_Cariel_Mission8HeroPower_02 = new AssetReference("VO_Story_Hero_Tamsin_Female_Undead_BOM_Cariel_Mission8HeroPower_02.prefab:e7d923342ba123d46b8b9e6aaa33a233");

	private static readonly AssetReference VO_Story_Hero_Tamsin_Female_Undead_BOM_Cariel_Mission8HeroPower_03 = new AssetReference("VO_Story_Hero_Tamsin_Female_Undead_BOM_Cariel_Mission8HeroPower_03.prefab:e7645230a442d50429c324abc0307b1e");

	private static readonly AssetReference VO_Story_Hero_Tamsin_Female_Undead_BOM_Cariel_Mission8Idle_01 = new AssetReference("VO_Story_Hero_Tamsin_Female_Undead_BOM_Cariel_Mission8Idle_01.prefab:e2f6aa72a9036e840b0b3b233cf2dbfb");

	private static readonly AssetReference VO_Story_Hero_Tamsin_Female_Undead_BOM_Cariel_Mission8Idle_02 = new AssetReference("VO_Story_Hero_Tamsin_Female_Undead_BOM_Cariel_Mission8Idle_02.prefab:d843d35851a600b468f1db40d64000c1");

	private static readonly AssetReference VO_Story_Hero_Tamsin_Female_Undead_BOM_Cariel_Mission8Idle_03 = new AssetReference("VO_Story_Hero_Tamsin_Female_Undead_BOM_Cariel_Mission8Idle_03.prefab:3c334e801fcbd024c8652780f3024de4");

	private static readonly AssetReference VO_Story_Hero_Tamsin_Female_Undead_BOM_Cariel_Mission8Intro_02 = new AssetReference("VO_Story_Hero_Tamsin_Female_Undead_BOM_Cariel_Mission8Intro_02.prefab:0e8a3bf441adaf943afff7612e6885fd");

	private static readonly AssetReference VO_Story_Hero_Tamsin_Female_Undead_BOM_Cariel_Mission8Loss_01 = new AssetReference("VO_Story_Hero_Tamsin_Female_Undead_BOM_Cariel_Mission8Loss_01.prefab:1ca2fb7835fab3c45b14fc8d5db88372");

	private static readonly AssetReference VO_Story_Hero_Tamsin_Female_Undead_BOM_Cariel_Mission8Low_01 = new AssetReference("VO_Story_Hero_Tamsin_Female_Undead_BOM_Cariel_Mission8Low_01.prefab:c29bfb8bfa03472439221f7bf57f07f2");

	private static readonly AssetReference VO_Story_Hero_Tamsin_Female_Undead_BOM_Cariel_Mission8Mill_01 = new AssetReference("VO_Story_Hero_Tamsin_Female_Undead_BOM_Cariel_Mission8Mill_01.prefab:cae38a0bbc66a5548ac82db921f41c53");

	private static readonly AssetReference VO_Story_Hero_Tamsin_Female_Undead_BOM_Cariel_Mission8Mill_02 = new AssetReference("VO_Story_Hero_Tamsin_Female_Undead_BOM_Cariel_Mission8Mill_02.prefab:074bda8d51996da46b643265a1b175f8");

	private static readonly AssetReference VO_Story_Hero_Tamsin_Female_Undead_BOM_Cariel_Mission8Mill_03 = new AssetReference("VO_Story_Hero_Tamsin_Female_Undead_BOM_Cariel_Mission8Mill_03.prefab:1ab9b2c0c3ab13842b5442ae8b42e300");

	private static readonly AssetReference VO_Story_Hero_Tamsin_Female_Undead_BOM_Cariel_Mission8Mill_04 = new AssetReference("VO_Story_Hero_Tamsin_Female_Undead_BOM_Cariel_Mission8Mill_04.prefab:a6af2eed66156894d9226d6a4b0b978d");

	private static readonly AssetReference VO_Story_Hero_Tamsin_Female_Undead_BOM_Cariel_Mission8Victory_01 = new AssetReference("VO_Story_Hero_Tamsin_Female_Undead_BOM_Cariel_Mission8Victory_01.prefab:386ad91fe0ce2b44e8284243e9104683");

	private static readonly AssetReference VO_PVPDR_Hero_Cariel_Female_Human_Death_01 = new AssetReference("VO_PVPDR_Hero_Cariel_Female_Human_Death_01.prefab:523b6a53885a78244ab354f929dbd2b0");

	private List<string> m_InGame_BossUsesHeroPowerLines = new List<string> { VO_Story_Hero_Tamsin_Female_Undead_BOM_Cariel_Mission8HeroPower_01, VO_Story_Hero_Tamsin_Female_Undead_BOM_Cariel_Mission8HeroPower_02, VO_Story_Hero_Tamsin_Female_Undead_BOM_Cariel_Mission8HeroPower_03 };

	private List<string> m_InGame_BossIdleLines = new List<string> { VO_Story_Hero_Tamsin_Female_Undead_BOM_Cariel_Mission8Idle_01, VO_Story_Hero_Tamsin_Female_Undead_BOM_Cariel_Mission8Idle_02, VO_Story_Hero_Tamsin_Female_Undead_BOM_Cariel_Mission8Idle_03 };

	private HashSet<string> m_playedLines = new HashSet<string>();

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> m_SoundFiles = new List<string>
		{
			VO_Story_Hero_Cariel_Female_Human_BOM_Cariel_Mission8ExchangeA_02, VO_Story_Hero_Cariel_Female_Human_BOM_Cariel_Mission8ExchangeC_02, VO_Story_Hero_Cariel_Female_Human_BOM_Cariel_Mission8ExchangeD_01, VO_Story_Hero_Cariel_Female_Human_BOM_Cariel_Mission8Intro_01, VO_Story_Hero_Cariel_Female_Human_BOM_Cariel_Mission8Victory_02, VO_Story_Hero_Tamsin_Female_Undead_BOM_Cariel_Mission8Death_01, VO_Story_Hero_Tamsin_Female_Undead_BOM_Cariel_Mission8EmoteResponse_01, VO_Story_Hero_Tamsin_Female_Undead_BOM_Cariel_Mission8ExchangeA_01, VO_Story_Hero_Tamsin_Female_Undead_BOM_Cariel_Mission8ExchangeB_01, VO_Story_Hero_Tamsin_Female_Undead_BOM_Cariel_Mission8ExchangeC_01,
			VO_Story_Hero_Tamsin_Female_Undead_BOM_Cariel_Mission8ExchangeC_03, VO_Story_Hero_Tamsin_Female_Undead_BOM_Cariel_Mission8ExchangeD_02, VO_Story_Hero_Tamsin_Female_Undead_BOM_Cariel_Mission8ExchangeE_01, VO_Story_Hero_Tamsin_Female_Undead_BOM_Cariel_Mission8HeroPower_01, VO_Story_Hero_Tamsin_Female_Undead_BOM_Cariel_Mission8HeroPower_02, VO_Story_Hero_Tamsin_Female_Undead_BOM_Cariel_Mission8HeroPower_03, VO_Story_Hero_Tamsin_Female_Undead_BOM_Cariel_Mission8Idle_01, VO_Story_Hero_Tamsin_Female_Undead_BOM_Cariel_Mission8Idle_02, VO_Story_Hero_Tamsin_Female_Undead_BOM_Cariel_Mission8Idle_03, VO_Story_Hero_Tamsin_Female_Undead_BOM_Cariel_Mission8Intro_02,
			VO_Story_Hero_Tamsin_Female_Undead_BOM_Cariel_Mission8Loss_01, VO_Story_Hero_Tamsin_Female_Undead_BOM_Cariel_Mission8Low_01, VO_Story_Hero_Tamsin_Female_Undead_BOM_Cariel_Mission8Mill_01, VO_Story_Hero_Tamsin_Female_Undead_BOM_Cariel_Mission8Mill_02, VO_Story_Hero_Tamsin_Female_Undead_BOM_Cariel_Mission8Mill_03, VO_Story_Hero_Tamsin_Female_Undead_BOM_Cariel_Mission8Mill_04, VO_Story_Hero_Tamsin_Female_Undead_BOM_Cariel_Mission8Victory_01, VO_PVPDR_Hero_Cariel_Female_Human_Death_01
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
			yield return MissionPlaySound(enemyActor, VO_Story_Hero_Tamsin_Female_Undead_BOM_Cariel_Mission8Death_01);
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
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Tamsin_Female_Undead_BOM_Cariel_Mission8EmoteResponse_01);
			break;
		case 514:
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Cariel_Female_Human_BOM_Cariel_Mission8Intro_01);
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Tamsin_Female_Undead_BOM_Cariel_Mission8Intro_02);
			break;
		case 507:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Tamsin_Female_Undead_BOM_Cariel_Mission8Loss_01);
			GameState.Get().SetBusy(busy: false);
			break;
		default:
			yield return base.HandleMissionEventWithTiming(missionEvent);
			break;
		case 504:
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
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Tamsin_Female_Undead_BOM_Cariel_Mission8ExchangeA_01);
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Cariel_Female_Human_BOM_Cariel_Mission8ExchangeA_02);
			break;
		case 7:
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Tamsin_Female_Undead_BOM_Cariel_Mission8ExchangeB_01);
			break;
		case 11:
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Tamsin_Female_Undead_BOM_Cariel_Mission8ExchangeC_01);
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Cariel_Female_Human_BOM_Cariel_Mission8ExchangeC_02);
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Tamsin_Female_Undead_BOM_Cariel_Mission8ExchangeC_03);
			break;
		case 15:
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Cariel_Female_Human_BOM_Cariel_Mission8ExchangeD_01);
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Tamsin_Female_Undead_BOM_Cariel_Mission8ExchangeD_02);
			break;
		case 19:
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Tamsin_Female_Undead_BOM_Cariel_Mission8ExchangeE_01);
			break;
		}
	}
}
