using System.Collections;
using System.Collections.Generic;

public class BOM_04_Kurtrus_Fight_03 : BOM_04_Kurtrus_Dungeon
{
	private static readonly AssetReference VO_Story_Hero_Cariel_Female_Human_Story_Kurtrus_Mission3ExchangeB_01 = new AssetReference("VO_Story_Hero_Cariel_Female_Human_Story_Kurtrus_Mission3ExchangeB_01.prefab:d592da87d9d57634a83d6125b417e816");

	private static readonly AssetReference VO_Story_Hero_Cariel_Female_Human_Story_Kurtrus_Mission3ExchangeD_02 = new AssetReference("VO_Story_Hero_Cariel_Female_Human_Story_Kurtrus_Mission3ExchangeD_02.prefab:d15712269994fa0448bce6192b718118");

	private static readonly AssetReference VO_Story_Hero_Cariel_Female_Human_Story_Kurtrus_Mission3ExchangeE_01 = new AssetReference("VO_Story_Hero_Cariel_Female_Human_Story_Kurtrus_Mission3ExchangeE_01.prefab:c50ee703013a10d4c83d6bc9835e4600");

	private static readonly AssetReference VO_Story_Hero_Cariel_Female_Human_Story_Kurtrus_Mission3Victory_01 = new AssetReference("VO_Story_Hero_Cariel_Female_Human_Story_Kurtrus_Mission3Victory_01.prefab:9ef65d6084f8b284cb0016b12cea7f2b");

	private static readonly AssetReference VO_Story_Hero_Cariel_Female_Human_Story_Kurtrus_Mission3Victory_02 = new AssetReference("VO_Story_Hero_Cariel_Female_Human_Story_Kurtrus_Mission3Victory_02.prefab:91df5cb95a1c7af44abf5de3171acbe9");

	private static readonly AssetReference VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission3ExchangeB_02 = new AssetReference("VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission3ExchangeB_02.prefab:0a6fa7f24c3fbb143a06f25e0debb484");

	private static readonly AssetReference VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission3ExchangeC_01 = new AssetReference("VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission3ExchangeC_01.prefab:45c92d2aa73670640b407f99844cee9a");

	private static readonly AssetReference VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission3ExchangeD_01 = new AssetReference("VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission3ExchangeD_01.prefab:da6f6437eaaf0154ab2464f8fb1bc876");

	private static readonly AssetReference VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission3ExchangeE_02 = new AssetReference("VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission3ExchangeE_02.prefab:2615056861d259e4f908c74767a5c6bd");

	private static readonly AssetReference VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission3Intro_01 = new AssetReference("VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission3Intro_01.prefab:61106068e7fdc1b4abf3bb2727a656fd");

	private static readonly AssetReference VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission3Victory_03 = new AssetReference("VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission3Victory_03.prefab:9507233b0ea35f24aa55a6d1bcc5ca5f");

	private static readonly AssetReference VO_Story_Hero_Sangtusk_Female_Quilboar_Story_Kurtrus_Mission3Death_01 = new AssetReference("VO_Story_Hero_Sangtusk_Female_Quilboar_Story_Kurtrus_Mission3Death_01.prefab:9b0f17d87e05c294cbbdbc22f3985696");

	private static readonly AssetReference VO_Story_Hero_Sangtusk_Female_Quilboar_Story_Kurtrus_Mission3EmoteResponse_01 = new AssetReference("VO_Story_Hero_Sangtusk_Female_Quilboar_Story_Kurtrus_Mission3EmoteResponse_01.prefab:885b5ff2e7f4e5846afb05341460d362");

	private static readonly AssetReference VO_Story_Hero_Sangtusk_Female_Quilboar_Story_Kurtrus_Mission3ExchangeA_01 = new AssetReference("VO_Story_Hero_Sangtusk_Female_Quilboar_Story_Kurtrus_Mission3ExchangeA_01.prefab:7c37f34a2aeb4f74d85bc662c7ef6a91");

	private static readonly AssetReference VO_Story_Hero_Sangtusk_Female_Quilboar_Story_Kurtrus_Mission3ExchangeF_01 = new AssetReference("VO_Story_Hero_Sangtusk_Female_Quilboar_Story_Kurtrus_Mission3ExchangeF_01.prefab:5fd2ebf772de6b543afecf383f5aca04");

	private static readonly AssetReference VO_Story_Hero_Sangtusk_Female_Quilboar_Story_Kurtrus_Mission3ExchangeG_01 = new AssetReference("VO_Story_Hero_Sangtusk_Female_Quilboar_Story_Kurtrus_Mission3ExchangeG_01.prefab:ec08c2cfd774b6045a6842dfcecd9db7");

	private static readonly AssetReference VO_Story_Hero_Sangtusk_Female_Quilboar_Story_Kurtrus_Mission3HeroPower_01 = new AssetReference("VO_Story_Hero_Sangtusk_Female_Quilboar_Story_Kurtrus_Mission3HeroPower_01.prefab:1bfc0285e3df18b4aa76d3595ccdf517");

	private static readonly AssetReference VO_Story_Hero_Sangtusk_Female_Quilboar_Story_Kurtrus_Mission3HeroPower_02 = new AssetReference("VO_Story_Hero_Sangtusk_Female_Quilboar_Story_Kurtrus_Mission3HeroPower_02.prefab:cc7301a9036137b4486c1a78cb9db79c");

	private static readonly AssetReference VO_Story_Hero_Sangtusk_Female_Quilboar_Story_Kurtrus_Mission3HeroPower_03 = new AssetReference("VO_Story_Hero_Sangtusk_Female_Quilboar_Story_Kurtrus_Mission3HeroPower_03.prefab:3c61a1356a95c584fbfb511fcd6cc146");

	private static readonly AssetReference VO_Story_Hero_Sangtusk_Female_Quilboar_Story_Kurtrus_Mission3Idle_01 = new AssetReference("VO_Story_Hero_Sangtusk_Female_Quilboar_Story_Kurtrus_Mission3Idle_01.prefab:a6ff53494ffea8d41a2a34e47ef4f3bc");

	private static readonly AssetReference VO_Story_Hero_Sangtusk_Female_Quilboar_Story_Kurtrus_Mission3Idle_02 = new AssetReference("VO_Story_Hero_Sangtusk_Female_Quilboar_Story_Kurtrus_Mission3Idle_02.prefab:33e03a02f60a90740ab9d31e7fc82113");

	private static readonly AssetReference VO_Story_Hero_Sangtusk_Female_Quilboar_Story_Kurtrus_Mission3Idle_03 = new AssetReference("VO_Story_Hero_Sangtusk_Female_Quilboar_Story_Kurtrus_Mission3Idle_03.prefab:1615b914c3a73d045b6d38d587685d29");

	private static readonly AssetReference VO_Story_Hero_Sangtusk_Female_Quilboar_Story_Kurtrus_Mission3Intro_02 = new AssetReference("VO_Story_Hero_Sangtusk_Female_Quilboar_Story_Kurtrus_Mission3Intro_02.prefab:fd730dda7d0326646a08e48241c4eebd");

	private static readonly AssetReference VO_Story_Hero_Sangtusk_Female_Quilboar_Story_Kurtrus_Mission3Loss_01 = new AssetReference("VO_Story_Hero_Sangtusk_Female_Quilboar_Story_Kurtrus_Mission3Loss_01.prefab:125788a4c85723c4886f0b340948bef4");

	private List<string> m_InGame_BossIdleLines = new List<string> { VO_Story_Hero_Sangtusk_Female_Quilboar_Story_Kurtrus_Mission3Idle_01, VO_Story_Hero_Sangtusk_Female_Quilboar_Story_Kurtrus_Mission3Idle_02, VO_Story_Hero_Sangtusk_Female_Quilboar_Story_Kurtrus_Mission3Idle_03 };

	private List<string> m_InGame_BossHeroPowerLines = new List<string> { VO_Story_Hero_Sangtusk_Female_Quilboar_Story_Kurtrus_Mission3HeroPower_01, VO_Story_Hero_Sangtusk_Female_Quilboar_Story_Kurtrus_Mission3HeroPower_02, VO_Story_Hero_Sangtusk_Female_Quilboar_Story_Kurtrus_Mission3HeroPower_03, VO_Story_Hero_Sangtusk_Female_Quilboar_Story_Kurtrus_Mission3ExchangeF_01, VO_Story_Hero_Sangtusk_Female_Quilboar_Story_Kurtrus_Mission3ExchangeG_01 };

	private List<string> m_missionEventTriggerInGame_VictoryPreExplosionLines = new List<string> { VO_Story_Hero_Cariel_Female_Human_Story_Kurtrus_Mission3Victory_01, VO_Story_Hero_Cariel_Female_Human_Story_Kurtrus_Mission3Victory_02, VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission3Victory_03 };

	private HashSet<string> m_playedLines = new HashSet<string>();

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> m_SoundFiles = new List<string>
		{
			VO_Story_Hero_Cariel_Female_Human_Story_Kurtrus_Mission3ExchangeB_01, VO_Story_Hero_Cariel_Female_Human_Story_Kurtrus_Mission3ExchangeD_02, VO_Story_Hero_Cariel_Female_Human_Story_Kurtrus_Mission3ExchangeE_01, VO_Story_Hero_Cariel_Female_Human_Story_Kurtrus_Mission3Victory_01, VO_Story_Hero_Cariel_Female_Human_Story_Kurtrus_Mission3Victory_02, VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission3ExchangeB_02, VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission3ExchangeC_01, VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission3ExchangeD_01, VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission3ExchangeE_02, VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission3Intro_01,
			VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission3Victory_03, VO_Story_Hero_Sangtusk_Female_Quilboar_Story_Kurtrus_Mission3Death_01, VO_Story_Hero_Sangtusk_Female_Quilboar_Story_Kurtrus_Mission3EmoteResponse_01, VO_Story_Hero_Sangtusk_Female_Quilboar_Story_Kurtrus_Mission3ExchangeA_01, VO_Story_Hero_Sangtusk_Female_Quilboar_Story_Kurtrus_Mission3ExchangeF_01, VO_Story_Hero_Sangtusk_Female_Quilboar_Story_Kurtrus_Mission3ExchangeG_01, VO_Story_Hero_Sangtusk_Female_Quilboar_Story_Kurtrus_Mission3HeroPower_01, VO_Story_Hero_Sangtusk_Female_Quilboar_Story_Kurtrus_Mission3HeroPower_02, VO_Story_Hero_Sangtusk_Female_Quilboar_Story_Kurtrus_Mission3HeroPower_03, VO_Story_Hero_Sangtusk_Female_Quilboar_Story_Kurtrus_Mission3Idle_01,
			VO_Story_Hero_Sangtusk_Female_Quilboar_Story_Kurtrus_Mission3Idle_02, VO_Story_Hero_Sangtusk_Female_Quilboar_Story_Kurtrus_Mission3Idle_03, VO_Story_Hero_Sangtusk_Female_Quilboar_Story_Kurtrus_Mission3Intro_02, VO_Story_Hero_Sangtusk_Female_Quilboar_Story_Kurtrus_Mission3Loss_01
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
		m_OverrideMusicTrack = MusicPlaylistType.InGame_BAR;
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
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission3Intro_01);
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Sangtusk_Female_Quilboar_Story_Kurtrus_Mission3Intro_02);
			break;
		case 506:
			MissionPause(pause: true);
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Sangtusk_Female_Quilboar_Story_Kurtrus_Mission3Loss_01);
			MissionPause(pause: false);
			break;
		case 505:
			MissionPause(pause: true);
			yield return MissionPlayVO("BOM_04_Cariel_001t", VO_Story_Hero_Cariel_Female_Human_Story_Kurtrus_Mission3Victory_01);
			yield return MissionPlayVO("BOM_04_Cariel_001t", VO_Story_Hero_Cariel_Female_Human_Story_Kurtrus_Mission3Victory_02);
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission3Victory_03);
			MissionPause(pause: false);
			break;
		case 517:
			yield return MissionPlayVO(enemyActor, m_InGame_BossIdleLines);
			break;
		case 515:
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Sangtusk_Female_Quilboar_Story_Kurtrus_Mission3EmoteResponse_01);
			break;
		case 510:
			yield return MissionPlayVO(enemyActor, m_InGame_BossHeroPowerLines);
			break;
		case 516:
			yield return MissionPlaySound(enemyActor, VO_Story_Hero_Sangtusk_Female_Quilboar_Story_Kurtrus_Mission3Death_01);
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
		case 3:
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Sangtusk_Female_Quilboar_Story_Kurtrus_Mission3ExchangeA_01);
			break;
		case 5:
			yield return MissionPlayVO("BOM_04_Cariel_001t", VO_Story_Hero_Cariel_Female_Human_Story_Kurtrus_Mission3ExchangeB_01);
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission3ExchangeB_02);
			break;
		case 7:
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission3ExchangeC_01);
			break;
		case 9:
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission3ExchangeD_01);
			yield return MissionPlayVO("BOM_04_Cariel_001t", VO_Story_Hero_Cariel_Female_Human_Story_Kurtrus_Mission3ExchangeD_02);
			break;
		case 11:
			yield return MissionPlayVO("BOM_04_Cariel_001t", VO_Story_Hero_Cariel_Female_Human_Story_Kurtrus_Mission3ExchangeE_01);
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission3ExchangeE_02);
			break;
		}
	}
}
