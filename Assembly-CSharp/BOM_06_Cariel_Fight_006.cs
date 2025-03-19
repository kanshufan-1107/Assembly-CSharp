using System.Collections;
using System.Collections.Generic;

public class BOM_06_Cariel_Fight_006 : BOM_06_Cariel_Dungeon
{
	private static readonly AssetReference VO_Story_Hero_Anetheron_Male_Demon_BOM_Cariel_Mission5Death_01 = new AssetReference("VO_Story_Hero_Anetheron_Male_Demon_BOM_Cariel_Mission5Death_01.prefab:6854dcc84ad110f4a8bf41f56756b7ad");

	private static readonly AssetReference VO_Story_Hero_BigAnetheron_Male_Demon_BOM_Cariel_Mission6Intro_02 = new AssetReference("VO_Story_Hero_BigAnetheron_Male_Demon_BOM_Cariel_Mission6Intro_02.prefab:23453c4913e8ecc43803d83d54e8eb55");

	private static readonly AssetReference VO_Story_Hero_BigAnetheron_Male_Demon_BOM_Cariel_Mission6Victory_01 = new AssetReference("VO_Story_Hero_BigAnetheron_Male_Demon_BOM_Cariel_Mission6Victory_01.prefab:0453ed292697bd449aa6b7c8f42f8ae7");

	private static readonly AssetReference VO_Story_Hero_Cariel_Female_Human_BOM_Cariel_Mission6ExchangeA_02 = new AssetReference("VO_Story_Hero_Cariel_Female_Human_BOM_Cariel_Mission6ExchangeA_02.prefab:3e7d59bbc4b1c984ab8e92d61e7235ad");

	private static readonly AssetReference VO_Story_Hero_Cariel_Female_Human_BOM_Cariel_Mission6Heal_01 = new AssetReference("VO_Story_Hero_Cariel_Female_Human_BOM_Cariel_Mission6Heal_01.prefab:bce42f79096a9344abdf9b76916a0c3c");

	private static readonly AssetReference VO_Story_Hero_Cariel_Female_Human_BOM_Cariel_Mission6Heal_02 = new AssetReference("VO_Story_Hero_Cariel_Female_Human_BOM_Cariel_Mission6Heal_02.prefab:9fd51e353d998934a99891541b814f68");

	private static readonly AssetReference VO_Story_Hero_Cariel_Female_Human_BOM_Cariel_Mission6Intro_01 = new AssetReference("VO_Story_Hero_Cariel_Female_Human_BOM_Cariel_Mission6Intro_01.prefab:eede34f07d9745c42879962c8a8cb41e");

	private static readonly AssetReference VO_Story_Hero_Cariel_Female_Human_BOM_Cariel_Mission6Rez_01 = new AssetReference("VO_Story_Hero_Cariel_Female_Human_BOM_Cariel_Mission6Rez_01.prefab:dbfc9bec79d7ff64fae0bd2a9bc923f5");

	private static readonly AssetReference VO_Story_Hero_Cariel_Female_Human_BOM_Cariel_Mission6Rez_02 = new AssetReference("VO_Story_Hero_Cariel_Female_Human_BOM_Cariel_Mission6Rez_02.prefab:6c47347d053daaf4ba7787eb6b08b6f9");

	private static readonly AssetReference VO_Story_Hero_Cariel_Female_Human_BOM_Cariel_Mission6Victory_02 = new AssetReference("VO_Story_Hero_Cariel_Female_Human_BOM_Cariel_Mission6Victory_02.prefab:0a4ae1cf22379a94798ddaf194fb1851");

	private static readonly AssetReference VO_Story_Hero_Cariel_Female_Human_BOM_Cariel_Mission6Victory_04 = new AssetReference("VO_Story_Hero_Cariel_Female_Human_BOM_Cariel_Mission6Victory_04.prefab:155b960625abf314bb4cb8ea3385f967");

	private static readonly AssetReference VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Cariel_Mission6Attack_01 = new AssetReference("VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Cariel_Mission6Attack_01.prefab:21bddcae79b817f4ebc8ee095b2e41e9");

	private static readonly AssetReference VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Cariel_Mission6Attack_02 = new AssetReference("VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Cariel_Mission6Attack_02.prefab:6f55c6afd59180842b457b6e28bd1281");

	private static readonly AssetReference VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Cariel_Mission6Attack_03 = new AssetReference("VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Cariel_Mission6Attack_03.prefab:91f6e354f2ef4964aaff5f549563ff4d");

	private static readonly AssetReference VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Cariel_Mission6Death_01 = new AssetReference("VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Cariel_Mission6Death_01.prefab:73491994cf87fd847915ff82ba13ef97");

	private static readonly AssetReference VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Cariel_Mission6Death_02 = new AssetReference("VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Cariel_Mission6Death_02.prefab:8678be09a2abc984fa9717c88080bfd1");

	private static readonly AssetReference VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Cariel_Mission6Death_03 = new AssetReference("VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Cariel_Mission6Death_03.prefab:3782c0b6dc1582b4584081aea8634d31");

	private static readonly AssetReference VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Cariel_Mission6ExchangeA_01 = new AssetReference("VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Cariel_Mission6ExchangeA_01.prefab:6bdc7853b93d36f42bf5b8e1e477c50e");

	private static readonly AssetReference VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Cariel_Mission6Heal_01 = new AssetReference("VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Cariel_Mission6Heal_01.prefab:b0226de1669694842b88a439c4d36053");

	private static readonly AssetReference VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Cariel_Mission6IsDead_01 = new AssetReference("VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Cariel_Mission6IsDead_01.prefab:bbea4f500b2cef44dac09c45e61eade9");

	private static readonly AssetReference VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Cariel_Mission6Play_01 = new AssetReference("VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Cariel_Mission6Play_01.prefab:b42cb01471fc00e49b57630797e837e7");

	private static readonly AssetReference VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Cariel_Mission6Rez_01 = new AssetReference("VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Cariel_Mission6Rez_01.prefab:f2b3ba75b740884428143ca3ce2062c1");

	private static readonly AssetReference VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Cariel_Mission6Trigger_01 = new AssetReference("VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Cariel_Mission6Trigger_01.prefab:82311b13d69dcc844851886de8413023");

	private static readonly AssetReference VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Cariel_Mission6Trigger_02 = new AssetReference("VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Cariel_Mission6Trigger_02.prefab:170c881671978a746b2d5333a6a7c3a1");

	private static readonly AssetReference VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Cariel_Mission6Trigger_03 = new AssetReference("VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Cariel_Mission6Trigger_03.prefab:758045c1fd6d61940a670ae9650d8e28");

	private static readonly AssetReference VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Cariel_Mission6Victory_03 = new AssetReference("VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Cariel_Mission6Victory_03.prefab:36f95ef7353b3cf409bf4b21ae75da90");

	private static readonly AssetReference VO_Story_Hero_BigAnetheron_Male_Demon_BOM_Cariel_Carrion_01 = new AssetReference("VO_Story_Hero_BigAnetheron_Male_Demon_BOM_Cariel_Carrion_01.prefab:b3b39d9ff0b13ca4683864fb9b0e728d");

	private static readonly AssetReference VO_Story_Hero_BigAnetheron_Male_Demon_BOM_Cariel_Carrion_02 = new AssetReference("VO_Story_Hero_BigAnetheron_Male_Demon_BOM_Cariel_Carrion_02.prefab:4dd266d52ea862345a29e9d383623eab");

	private static readonly AssetReference VO_Story_Hero_BigAnetheron_Male_Demon_BOM_Cariel_Carrion_03 = new AssetReference("VO_Story_Hero_BigAnetheron_Male_Demon_BOM_Cariel_Carrion_03.prefab:c10c8e32471fb5841b88a940dfb15820");

	private static readonly AssetReference VO_Story_Hero_BigAnetheron_Male_Demon_BOM_Cariel_Inferno_01 = new AssetReference("VO_Story_Hero_BigAnetheron_Male_Demon_BOM_Cariel_Inferno_01.prefab:c4c562fe5af56494386db9c71632cff5");

	private static readonly AssetReference VO_Story_Hero_BigAnetheron_Male_Demon_BOM_Cariel_Inferno_02 = new AssetReference("VO_Story_Hero_BigAnetheron_Male_Demon_BOM_Cariel_Inferno_02.prefab:92c45ef64084f82468c7b957d8e8c689");

	private static readonly AssetReference VO_Story_Hero_BigAnetheron_Male_Demon_BOM_Cariel_Inferno_03 = new AssetReference("VO_Story_Hero_BigAnetheron_Male_Demon_BOM_Cariel_Inferno_03.prefab:4288c07931c87234dbdf6bfa76a22a8d");

	private static readonly AssetReference VO_Story_Hero_BigAnetheron_Male_Demon_BOM_Cariel_Sleep_01 = new AssetReference("VO_Story_Hero_BigAnetheron_Male_Demon_BOM_Cariel_Sleep_01.prefab:bf985207a0dc2cf408710359e5d91abc");

	private static readonly AssetReference VO_Story_Hero_BigAnetheron_Male_Demon_BOM_Cariel_Sleep_02 = new AssetReference("VO_Story_Hero_BigAnetheron_Male_Demon_BOM_Cariel_Sleep_02.prefab:acde37bdb71c9664ab9947647812af17");

	private static readonly AssetReference VO_Story_Hero_BigAnetheron_Male_Demon_BOM_Cariel_Sleep_03 = new AssetReference("VO_Story_Hero_BigAnetheron_Male_Demon_BOM_Cariel_Sleep_03.prefab:f065fa035b5f05748804fadff6a46692");

	private static readonly AssetReference VO_Story_Hero_Anetheron_Male_Demon_BOM_Cariel_Mission5Loss_01 = new AssetReference("VO_Story_Hero_Anetheron_Male_Demon_BOM_Cariel_Mission5Loss_01.prefab:9fd045005a4d050458e6405c22ab62be");

	private static readonly AssetReference VO_Story_Hero_Anetheron_Male_Demon_BOM_Cariel_Mission5EmoteResponse_01 = new AssetReference("VO_Story_Hero_Anetheron_Male_Demon_BOM_Cariel_Mission5EmoteResponse_01.prefab:9e4ff9851bb79424a89b9e1000e47548");

	private static readonly AssetReference VO_PVPDR_Hero_Cariel_Female_Human_Death_01 = new AssetReference("VO_PVPDR_Hero_Cariel_Female_Human_Death_01.prefab:523b6a53885a78244ab354f929dbd2b0");

	private static readonly AssetReference VO_Story_Hero_Anetheron_Male_Demon_BOM_Cariel_Mission5Idle_01 = new AssetReference("VO_Story_Hero_Anetheron_Male_Demon_BOM_Cariel_Mission5Idle_01.prefab:695afe7e4c549fe408624e289c92db8b");

	private static readonly AssetReference VO_Story_Hero_Anetheron_Male_Demon_BOM_Cariel_Mission5Idle_02 = new AssetReference("VO_Story_Hero_Anetheron_Male_Demon_BOM_Cariel_Mission5Idle_02.prefab:d6bbbc481b08c9a49bfa92b7152ce71e");

	private static readonly AssetReference VO_Story_Hero_Anetheron_Male_Demon_BOM_Cariel_Mission5Idle_03 = new AssetReference("VO_Story_Hero_Anetheron_Male_Demon_BOM_Cariel_Mission5Idle_03.prefab:88ea2646a9ca873419bde510f9d139ec");

	private List<string> m_VO_Story_Hero_BigAnetheron_Male_Demon_BOM_Cariel_SleepLines = new List<string> { VO_Story_Hero_BigAnetheron_Male_Demon_BOM_Cariel_Sleep_01, VO_Story_Hero_BigAnetheron_Male_Demon_BOM_Cariel_Sleep_02, VO_Story_Hero_BigAnetheron_Male_Demon_BOM_Cariel_Sleep_03 };

	private List<string> m_VO_Story_Hero_BigAnetheron_Male_Demon_BOM_Cariel_InfernalLines = new List<string> { VO_Story_Hero_BigAnetheron_Male_Demon_BOM_Cariel_Inferno_01, VO_Story_Hero_BigAnetheron_Male_Demon_BOM_Cariel_Inferno_02, VO_Story_Hero_BigAnetheron_Male_Demon_BOM_Cariel_Inferno_03 };

	private List<string> m_VO_Story_Hero_BigAnetheron_Male_Demon_BOM_Cariel_CarrionLines = new List<string> { VO_Story_Hero_BigAnetheron_Male_Demon_BOM_Cariel_Carrion_01, VO_Story_Hero_BigAnetheron_Male_Demon_BOM_Cariel_Carrion_02, VO_Story_Hero_BigAnetheron_Male_Demon_BOM_Cariel_Carrion_03 };

	private List<string> m_InGame_IntroductionLines = new List<string> { VO_Story_Hero_BigAnetheron_Male_Demon_BOM_Cariel_Mission6Intro_02, VO_Story_Hero_Cariel_Female_Human_BOM_Cariel_Mission6Intro_01 };

	private List<string> m_InGame_PlayerUsesHeroPowerHeal = new List<string> { VO_Story_Hero_Cariel_Female_Human_BOM_Cariel_Mission6Heal_01, VO_Story_Hero_Cariel_Female_Human_BOM_Cariel_Mission6Heal_02 };

	private List<string> m_InGame_PlayerUsesHeroPowerRez = new List<string> { VO_Story_Hero_Cariel_Female_Human_BOM_Cariel_Mission6Rez_01, VO_Story_Hero_Cariel_Female_Human_BOM_Cariel_Mission6Rez_02 };

	private List<string> m_Kurtrus_Attack = new List<string> { VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Cariel_Mission6Attack_01, VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Cariel_Mission6Attack_02, VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Cariel_Mission6Attack_03 };

	private List<string> m_Kurtrus_TriggerLine = new List<string> { VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Cariel_Mission6Trigger_01, VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Cariel_Mission6Trigger_02, VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Cariel_Mission6Trigger_03 };

	private List<string> m_Kurtrus_IsDeadLines = new List<string> { VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Cariel_Mission6Death_01, VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Cariel_Mission6Death_02, VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Cariel_Mission6Death_03, VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Cariel_Mission6IsDead_01 };

	private List<string> m_InGame_BossIdleLines = new List<string> { VO_Story_Hero_Anetheron_Male_Demon_BOM_Cariel_Mission5Idle_01, VO_Story_Hero_Anetheron_Male_Demon_BOM_Cariel_Mission5Idle_02, VO_Story_Hero_Anetheron_Male_Demon_BOM_Cariel_Mission5Idle_03 };

	private HashSet<string> m_playedLines = new HashSet<string>();

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> m_SoundFiles = new List<string>
		{
			VO_Story_Hero_Anetheron_Male_Demon_BOM_Cariel_Mission5Death_01, VO_Story_Hero_BigAnetheron_Male_Demon_BOM_Cariel_Carrion_01, VO_Story_Hero_BigAnetheron_Male_Demon_BOM_Cariel_Carrion_02, VO_Story_Hero_BigAnetheron_Male_Demon_BOM_Cariel_Carrion_03, VO_Story_Hero_BigAnetheron_Male_Demon_BOM_Cariel_Inferno_01, VO_Story_Hero_BigAnetheron_Male_Demon_BOM_Cariel_Inferno_02, VO_Story_Hero_BigAnetheron_Male_Demon_BOM_Cariel_Inferno_03, VO_Story_Hero_BigAnetheron_Male_Demon_BOM_Cariel_Mission6Intro_02, VO_Story_Hero_BigAnetheron_Male_Demon_BOM_Cariel_Mission6Victory_01, VO_Story_Hero_BigAnetheron_Male_Demon_BOM_Cariel_Sleep_01,
			VO_Story_Hero_BigAnetheron_Male_Demon_BOM_Cariel_Sleep_02, VO_Story_Hero_BigAnetheron_Male_Demon_BOM_Cariel_Sleep_03, VO_Story_Hero_Cariel_Female_Human_BOM_Cariel_Mission6ExchangeA_02, VO_Story_Hero_Cariel_Female_Human_BOM_Cariel_Mission6Heal_01, VO_Story_Hero_Cariel_Female_Human_BOM_Cariel_Mission6Heal_02, VO_Story_Hero_Cariel_Female_Human_BOM_Cariel_Mission6Intro_01, VO_Story_Hero_Cariel_Female_Human_BOM_Cariel_Mission6Rez_01, VO_Story_Hero_Cariel_Female_Human_BOM_Cariel_Mission6Rez_02, VO_Story_Hero_Cariel_Female_Human_BOM_Cariel_Mission6Victory_02, VO_Story_Hero_Cariel_Female_Human_BOM_Cariel_Mission6Victory_04,
			VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Cariel_Mission6Victory_03, VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Cariel_Mission6Death_01, VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Cariel_Mission6Death_02, VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Cariel_Mission6Death_03, VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Cariel_Mission6Attack_01, VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Cariel_Mission6Attack_02, VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Cariel_Mission6Attack_03, VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Cariel_Mission6ExchangeA_01, VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Cariel_Mission6Heal_01, VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Cariel_Mission6IsDead_01,
			VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Cariel_Mission6Play_01, VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Cariel_Mission6Rez_01, VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Cariel_Mission6Trigger_01, VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Cariel_Mission6Trigger_02, VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Cariel_Mission6Trigger_03, VO_Story_Hero_Anetheron_Male_Demon_BOM_Cariel_Mission5Loss_01, VO_Story_Hero_Anetheron_Male_Demon_BOM_Cariel_Mission5EmoteResponse_01, VO_PVPDR_Hero_Cariel_Female_Human_Death_01, VO_Story_Hero_Anetheron_Male_Demon_BOM_Cariel_Mission5Idle_01, VO_Story_Hero_Anetheron_Male_Demon_BOM_Cariel_Mission5Idle_02,
			VO_Story_Hero_Anetheron_Male_Demon_BOM_Cariel_Mission5Idle_03
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
		switch (missionEvent)
		{
		case 514:
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Cariel_Female_Human_BOM_Cariel_Mission6Intro_01);
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_BigAnetheron_Male_Demon_BOM_Cariel_Mission6Intro_02);
			break;
		case 516:
			yield return MissionPlaySound(enemyActor, VO_Story_Hero_Anetheron_Male_Demon_BOM_Cariel_Mission5Death_01);
			break;
		case 519:
			yield return MissionPlaySound(enemyActor, VO_PVPDR_Hero_Cariel_Female_Human_Death_01);
			break;
		case 515:
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Anetheron_Male_Demon_BOM_Cariel_Mission5EmoteResponse_01);
			break;
		case 507:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Anetheron_Male_Demon_BOM_Cariel_Mission5Loss_01);
			GameState.Get().SetBusy(busy: false);
			break;
		case 505:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Cariel_Female_Human_BOM_Cariel_Mission6Victory_02);
			yield return MissionPlayVO(Kurtrus_Stormwind_BrassRing_Quote, VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Cariel_Mission6Victory_03);
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Cariel_Female_Human_BOM_Cariel_Mission6Victory_04);
			GameState.Get().SetBusy(busy: false);
			break;
		case 61110:
			if (shouldPlayBanterVO())
			{
				yield return MissionPlayVO("BOM_06_Kurtrus_006t", m_Kurtrus_Attack);
			}
			break;
		case 61111:
			if (shouldPlayBanterVO())
			{
				yield return MissionPlayVO("BOM_06_Kurtrus_006t", m_Kurtrus_TriggerLine);
			}
			break;
		case 61112:
			if (shouldPlayBanterVO())
			{
				yield return MissionPlayVO("BOM_06_Kurtrus_006t", m_Kurtrus_IsDeadLines);
			}
			break;
		case 61113:
			if (shouldPlayBanterVO())
			{
				yield return MissionPlayVO(friendlyActor, m_InGame_PlayerUsesHeroPowerHeal);
				yield return MissionPlayVOOnce("BOM_06_Kurtrus_006t", VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Cariel_Mission6Heal_01);
			}
			break;
		case 61114:
			if (shouldPlayBanterVO())
			{
				yield return MissionPlayVO(friendlyActor, m_InGame_PlayerUsesHeroPowerRez);
				yield return MissionPlayVOOnce("BOM_06_Kurtrus_006t", VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Cariel_Mission6Rez_01);
			}
			break;
		case 517:
			yield return MissionPlayVO(enemyActor, m_InGame_BossIdleLines);
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
			Actor enemyActor = GameState.Get().GetOpposingSidePlayer().GetHero()
				.GetCard()
				.GetActor();
			GameState.Get().GetFriendlySidePlayer().GetHero()
				.GetCard()
				.GetActor();
			switch (cardID)
			{
			case "BOM_06_Sleep_005s":
				yield return MissionPlayVOOnce(enemyActor, m_VO_Story_Hero_BigAnetheron_Male_Demon_BOM_Cariel_SleepLines);
				break;
			case "BOM_06_CarrionSwarm_005s":
				yield return MissionPlayVOOnce(enemyActor, m_VO_Story_Hero_BigAnetheron_Male_Demon_BOM_Cariel_CarrionLines);
				break;
			case "BOM_06_SummonInfernal_005s":
				yield return MissionPlayVOOnce(enemyActor, m_VO_Story_Hero_BigAnetheron_Male_Demon_BOM_Cariel_InfernalLines);
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
		GameState.Get().GetOpposingSidePlayer().GetHero()
			.GetCard()
			.GetActor();
		Actor friendlyActor = GameState.Get().GetFriendlySidePlayer().GetHero()
			.GetCard()
			.GetActor();
		if (turn == 3)
		{
			yield return MissionPlayVO(Kurtrus_Stormwind_BrassRing_Quote, VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Cariel_Mission6ExchangeA_01);
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Cariel_Female_Human_BOM_Cariel_Mission6ExchangeA_02);
		}
	}
}
