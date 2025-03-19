using System.Collections;
using System.Collections.Generic;

public class BOM_04_Kurtrus_Fight_02 : BOM_04_Kurtrus_Dungeon
{
	private static readonly AssetReference VO_Story_Hero_Aranna_Female_NightElf_Story_Kurtrus_Mission2Death_01 = new AssetReference("VO_Story_Hero_Aranna_Female_NightElf_Story_Kurtrus_Mission2Death_01.prefab:b43d110eb36c20a4daefcc65034df4f8");

	private static readonly AssetReference VO_Story_Hero_Aranna_Female_NightElf_Story_Kurtrus_Mission2EmoteResponse_01 = new AssetReference("VO_Story_Hero_Aranna_Female_NightElf_Story_Kurtrus_Mission2EmoteResponse_01.prefab:67186388f968812459c57bc97f7d120d");

	private static readonly AssetReference VO_Story_Hero_Aranna_Female_NightElf_Story_Kurtrus_Mission2ExchangeA_01 = new AssetReference("VO_Story_Hero_Aranna_Female_NightElf_Story_Kurtrus_Mission2ExchangeA_01.prefab:fb9e23370c94b4f428e2eec6124fac96");

	private static readonly AssetReference VO_Story_Hero_Aranna_Female_NightElf_Story_Kurtrus_Mission2ExchangeC_02 = new AssetReference("VO_Story_Hero_Aranna_Female_NightElf_Story_Kurtrus_Mission2ExchangeC_02.prefab:66c7eabb2477eb24b8a7e648c4780070");

	private static readonly AssetReference VO_Story_Hero_Aranna_Female_NightElf_Story_Kurtrus_Mission2ExchangeD_02 = new AssetReference("VO_Story_Hero_Aranna_Female_NightElf_Story_Kurtrus_Mission2ExchangeD_02.prefab:045083143876f7a4c9aaa40d6fd116a0");

	private static readonly AssetReference VO_Story_Hero_Aranna_Female_NightElf_Story_Kurtrus_Mission2ExchangeE_01 = new AssetReference("VO_Story_Hero_Aranna_Female_NightElf_Story_Kurtrus_Mission2ExchangeE_01.prefab:161efa5a0c1b88c49b1d2ddf36a4010c");

	private static readonly AssetReference VO_Story_Hero_Aranna_Female_NightElf_Story_Kurtrus_Mission2ExchangeF_01 = new AssetReference("VO_Story_Hero_Aranna_Female_NightElf_Story_Kurtrus_Mission2ExchangeF_01.prefab:2d828c31f7dff3140a9ab1b769ff6c17");

	private static readonly AssetReference VO_Story_Hero_Aranna_Female_NightElf_Story_Kurtrus_Mission2HeroPower_01 = new AssetReference("VO_Story_Hero_Aranna_Female_NightElf_Story_Kurtrus_Mission2HeroPower_01.prefab:80640082af89daf4bb2af3016c9ce0b9");

	private static readonly AssetReference VO_Story_Hero_Aranna_Female_NightElf_Story_Kurtrus_Mission2HeroPower_02 = new AssetReference("VO_Story_Hero_Aranna_Female_NightElf_Story_Kurtrus_Mission2HeroPower_02.prefab:ee19731073b4f2f42a524440c2c607d3");

	private static readonly AssetReference VO_Story_Hero_Aranna_Female_NightElf_Story_Kurtrus_Mission2HeroPower_03 = new AssetReference("VO_Story_Hero_Aranna_Female_NightElf_Story_Kurtrus_Mission2HeroPower_03.prefab:b8a679c6586a4514390432fb77cb7942");

	private static readonly AssetReference VO_Story_Hero_Aranna_Female_NightElf_Story_Kurtrus_Mission2Idle_01 = new AssetReference("VO_Story_Hero_Aranna_Female_NightElf_Story_Kurtrus_Mission2Idle_01.prefab:59ec80ddd2235414fba7799ee2681f63");

	private static readonly AssetReference VO_Story_Hero_Aranna_Female_NightElf_Story_Kurtrus_Mission2Idle_02 = new AssetReference("VO_Story_Hero_Aranna_Female_NightElf_Story_Kurtrus_Mission2Idle_02.prefab:c05f53048d8bfb743a03575875b68f1c");

	private static readonly AssetReference VO_Story_Hero_Aranna_Female_NightElf_Story_Kurtrus_Mission2Idle_03 = new AssetReference("VO_Story_Hero_Aranna_Female_NightElf_Story_Kurtrus_Mission2Idle_03.prefab:9f34349ef7317484ea1aff4d21b5190c");

	private static readonly AssetReference VO_Story_Hero_Aranna_Female_NightElf_Story_Kurtrus_Mission2Intro_01 = new AssetReference("VO_Story_Hero_Aranna_Female_NightElf_Story_Kurtrus_Mission2Intro_01.prefab:21224e887d08d574689807cdcfb57021");

	private static readonly AssetReference VO_Story_Hero_Aranna_Female_NightElf_Story_Kurtrus_Mission2Loss_01 = new AssetReference("VO_Story_Hero_Aranna_Female_NightElf_Story_Kurtrus_Mission2Loss_01.prefab:3a396ac7686df824d9d3943f62f41519");

	private static readonly AssetReference VO_Story_Hero_Aranna_Female_NightElf_Story_Kurtrus_Mission2Victory_01 = new AssetReference("VO_Story_Hero_Aranna_Female_NightElf_Story_Kurtrus_Mission2Victory_01.prefab:1437a91d2d704e843b1b396b761e29f4");

	private static readonly AssetReference VO_Story_Hero_Aranna_Female_NightElf_Story_Kurtrus_Mission2Victory_03 = new AssetReference("VO_Story_Hero_Aranna_Female_NightElf_Story_Kurtrus_Mission2Victory_03.prefab:2f4047f89601cf2419c519bbd5e9bf3b");

	private static readonly AssetReference VO_Story_Hero_Cariel_Female_Human_Story_Kurtrus_Mission2ExchangeB_01 = new AssetReference("VO_Story_Hero_Cariel_Female_Human_Story_Kurtrus_Mission2ExchangeB_01.prefab:f877388b02becf0429a09a6e14bd5925");

	private static readonly AssetReference VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission2ExchangeA_02 = new AssetReference("VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission2ExchangeA_02.prefab:20d116e6516957f499702734ada828ff");

	private static readonly AssetReference VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission2ExchangeB_02 = new AssetReference("VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission2ExchangeB_02.prefab:8bc8922f28285c448abef1c9388937b9");

	private static readonly AssetReference VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission2ExchangeC_01 = new AssetReference("VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission2ExchangeC_01.prefab:dc78579305592384aa418183a7e56377");

	private static readonly AssetReference VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission2ExchangeD_01 = new AssetReference("VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission2ExchangeD_01.prefab:cd7c395d8202c5d438f4dd747c00ac36");

	private static readonly AssetReference VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission2Intro_02 = new AssetReference("VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission2Intro_02.prefab:ed318da900ba58a4692584b04667706d");

	private static readonly AssetReference VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission2Victory_02 = new AssetReference("VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission2Victory_02.prefab:a9df8da01046f0c419930f671b11f9a0");

	private List<string> m_missionEventTriggerInGame_VictoryPreExplosionLines = new List<string> { VO_Story_Hero_Aranna_Female_NightElf_Story_Kurtrus_Mission2Victory_01, VO_Story_Hero_Aranna_Female_NightElf_Story_Kurtrus_Mission2Victory_03, VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission2Victory_02 };

	private List<string> m_InGame_BossIdleLines = new List<string> { VO_Story_Hero_Aranna_Female_NightElf_Story_Kurtrus_Mission2Idle_01, VO_Story_Hero_Aranna_Female_NightElf_Story_Kurtrus_Mission2Idle_02, VO_Story_Hero_Aranna_Female_NightElf_Story_Kurtrus_Mission2Idle_03 };

	private List<string> m_InGame_BossHeroPowerLines = new List<string> { VO_Story_Hero_Aranna_Female_NightElf_Story_Kurtrus_Mission2HeroPower_01, VO_Story_Hero_Aranna_Female_NightElf_Story_Kurtrus_Mission2HeroPower_02, VO_Story_Hero_Aranna_Female_NightElf_Story_Kurtrus_Mission2HeroPower_03 };

	private HashSet<string> m_playedLines = new HashSet<string>();

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> m_SoundFiles = new List<string>
		{
			VO_Story_Hero_Aranna_Female_NightElf_Story_Kurtrus_Mission2Death_01, VO_Story_Hero_Aranna_Female_NightElf_Story_Kurtrus_Mission2EmoteResponse_01, VO_Story_Hero_Aranna_Female_NightElf_Story_Kurtrus_Mission2ExchangeA_01, VO_Story_Hero_Aranna_Female_NightElf_Story_Kurtrus_Mission2ExchangeC_02, VO_Story_Hero_Aranna_Female_NightElf_Story_Kurtrus_Mission2ExchangeD_02, VO_Story_Hero_Aranna_Female_NightElf_Story_Kurtrus_Mission2ExchangeE_01, VO_Story_Hero_Aranna_Female_NightElf_Story_Kurtrus_Mission2ExchangeF_01, VO_Story_Hero_Aranna_Female_NightElf_Story_Kurtrus_Mission2HeroPower_01, VO_Story_Hero_Aranna_Female_NightElf_Story_Kurtrus_Mission2HeroPower_02, VO_Story_Hero_Aranna_Female_NightElf_Story_Kurtrus_Mission2HeroPower_03,
			VO_Story_Hero_Aranna_Female_NightElf_Story_Kurtrus_Mission2Idle_01, VO_Story_Hero_Aranna_Female_NightElf_Story_Kurtrus_Mission2Idle_02, VO_Story_Hero_Aranna_Female_NightElf_Story_Kurtrus_Mission2Idle_03, VO_Story_Hero_Aranna_Female_NightElf_Story_Kurtrus_Mission2Intro_01, VO_Story_Hero_Aranna_Female_NightElf_Story_Kurtrus_Mission2Loss_01, VO_Story_Hero_Aranna_Female_NightElf_Story_Kurtrus_Mission2Victory_01, VO_Story_Hero_Aranna_Female_NightElf_Story_Kurtrus_Mission2Victory_03, VO_Story_Hero_Cariel_Female_Human_Story_Kurtrus_Mission2ExchangeB_01, VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission2ExchangeA_02, VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission2ExchangeB_02,
			VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission2ExchangeC_01, VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission2ExchangeD_01, VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission2Intro_02, VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission2Victory_02
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
		m_OverrideMusicTrack = MusicPlaylistType.InGame_DHPrologueBoss;
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
		case 514:
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Aranna_Female_NightElf_Story_Kurtrus_Mission2Intro_01);
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission2Intro_02);
			break;
		case 506:
			MissionPause(pause: true);
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Aranna_Female_NightElf_Story_Kurtrus_Mission2Loss_01);
			MissionPause(pause: false);
			break;
		case 504:
			MissionPause(pause: true);
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Aranna_Female_NightElf_Story_Kurtrus_Mission2Victory_01);
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission2Victory_02);
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Aranna_Female_NightElf_Story_Kurtrus_Mission2Victory_03);
			MissionPause(pause: false);
			break;
		case 517:
			yield return MissionPlayVO(enemyActor, m_InGame_BossIdleLines);
			break;
		case 515:
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Aranna_Female_NightElf_Story_Kurtrus_Mission2EmoteResponse_01);
			break;
		case 510:
			yield return MissionPlayVO(enemyActor, m_InGame_BossHeroPowerLines);
			break;
		case 100:
			if (shouldPlayBanterVO())
			{
				yield return MissionPlayVOOnce(enemyActor, VO_Story_Hero_Aranna_Female_NightElf_Story_Kurtrus_Mission2ExchangeE_01);
			}
			break;
		case 101:
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Aranna_Female_NightElf_Story_Kurtrus_Mission2ExchangeF_01);
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
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission2ExchangeA_02);
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Aranna_Female_NightElf_Story_Kurtrus_Mission2ExchangeA_01);
			break;
		case 5:
			yield return MissionPlayVO("BOM_04_Cariel_001t", VO_Story_Hero_Cariel_Female_Human_Story_Kurtrus_Mission2ExchangeB_01);
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission2ExchangeB_02);
			break;
		case 9:
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission2ExchangeC_01);
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Aranna_Female_NightElf_Story_Kurtrus_Mission2ExchangeC_02);
			break;
		case 13:
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission2ExchangeD_01);
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Aranna_Female_NightElf_Story_Kurtrus_Mission2ExchangeD_02);
			break;
		}
	}
}
