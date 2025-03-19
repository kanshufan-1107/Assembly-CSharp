using System.Collections;
using System.Collections.Generic;

public class BOM_04_Kurtrus_Fight_01 : BOM_04_Kurtrus_Dungeon
{
	private static readonly AssetReference VO_Story_Hero_Cariel_Female_Human_Story_Kurtrus_Mission1ExchangeB_01 = new AssetReference("VO_Story_Hero_Cariel_Female_Human_Story_Kurtrus_Mission1ExchangeB_01.prefab:27d1ad482127cdb42b2fa35bcf469d85");

	private static readonly AssetReference VO_Story_Hero_Cariel_Female_Human_Story_Kurtrus_Mission1ExchangeC_02 = new AssetReference("VO_Story_Hero_Cariel_Female_Human_Story_Kurtrus_Mission1ExchangeC_02.prefab:1fbb87044173f5044bf994f3b2c75345");

	private static readonly AssetReference VO_Story_Hero_Cariel_Female_Human_Story_Kurtrus_Mission1ExchangeD_03 = new AssetReference("VO_Story_Hero_Cariel_Female_Human_Story_Kurtrus_Mission1ExchangeD_03.prefab:47176342bec603045b8c6db47fc11d57");

	private static readonly AssetReference VO_Story_Hero_Cariel_Female_Human_Story_Kurtrus_Mission1ExchangeF_02 = new AssetReference("VO_Story_Hero_Cariel_Female_Human_Story_Kurtrus_Mission1ExchangeF_02.prefab:092dc18d0b6d43948a3720c1b0be9ddd");

	private static readonly AssetReference VO_Story_Hero_Cariel_Female_Human_Story_Kurtrus_Mission1ExchangeG_01 = new AssetReference("VO_Story_Hero_Cariel_Female_Human_Story_Kurtrus_Mission1ExchangeG_01.prefab:f7c7b17fae46a9747877181117ee95f6");

	private static readonly AssetReference VO_Story_Hero_Cariel_Female_Human_Story_Kurtrus_Mission1ExchangeH_01 = new AssetReference("VO_Story_Hero_Cariel_Female_Human_Story_Kurtrus_Mission1ExchangeH_01.prefab:2d36bba4c7079eb419b4dfbf5eda79e2");

	private static readonly AssetReference VO_Story_Hero_Cariel_Female_Human_Story_Kurtrus_Mission1Victory_02 = new AssetReference("VO_Story_Hero_Cariel_Female_Human_Story_Kurtrus_Mission1Victory_02.prefab:32a75c4755b79224bb1e58bdb3f53a89");

	private static readonly AssetReference VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission1ExchangeA_01 = new AssetReference("VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission1ExchangeA_01.prefab:e8339c6194105ca499806580016a8384");

	private static readonly AssetReference VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission1ExchangeC_01 = new AssetReference("VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission1ExchangeC_01.prefab:259b22fa795799a49afc49c89eb2cc0f");

	private static readonly AssetReference VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission1ExchangeD_01 = new AssetReference("VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission1ExchangeD_01.prefab:b9b927dc176d4aa4881fd1db58a812f3");

	private static readonly AssetReference VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission1ExchangeD_02 = new AssetReference("VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission1ExchangeD_02.prefab:e4b63af59a503954c8b1ed92a72fccdb");

	private static readonly AssetReference VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission1ExchangeE_01 = new AssetReference("VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission1ExchangeE_01.prefab:cc631ea8c9e89074bb1a7d76cf844f88");

	private static readonly AssetReference VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission1ExchangeE_02 = new AssetReference("VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission1ExchangeE_02.prefab:670a10ca4a5f5ce4aa2f2275e4dbdaf1");

	private static readonly AssetReference VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission1ExchangeF_01 = new AssetReference("VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission1ExchangeF_01.prefab:4f73f19120a75e2468c5d4ce09406e36");

	private static readonly AssetReference VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission1ExchangeG_02 = new AssetReference("VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission1ExchangeG_02.prefab:22931e39c48d1714aa5dc0b6d16b8b6b");

	private static readonly AssetReference VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission1ExchangeH_02 = new AssetReference("VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission1ExchangeH_02.prefab:95520ce4453f8494ab9d1d36eab495f1");

	private static readonly AssetReference VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission1Intro_01 = new AssetReference("VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission1Intro_01.prefab:d08ec76a729a73c44970d677f6d667d7");

	private static readonly AssetReference VO_Story_Hero_Samuro_Male_Orc_Story_Kurtrus_Mission1Death_01 = new AssetReference("VO_Story_Hero_Samuro_Male_Orc_Story_Kurtrus_Mission1Death_01.prefab:583b8d8b0dab61248bc28ae4256f0fcf");

	private static readonly AssetReference VO_Story_Hero_Samuro_Male_Orc_Story_Kurtrus_Mission1EmoteResponse_01 = new AssetReference("VO_Story_Hero_Samuro_Male_Orc_Story_Kurtrus_Mission1EmoteResponse_01.prefab:c7905ad7c75142946a9f8ad8d85b1aa4");

	private static readonly AssetReference VO_Story_Hero_Samuro_Male_Orc_Story_Kurtrus_Mission1ExchangeA_02 = new AssetReference("VO_Story_Hero_Samuro_Male_Orc_Story_Kurtrus_Mission1ExchangeA_02.prefab:3a3785e7c4d8f434bab1cc101e819871");

	private static readonly AssetReference VO_Story_Hero_Samuro_Male_Orc_Story_Kurtrus_Mission1ExchangeB_02 = new AssetReference("VO_Story_Hero_Samuro_Male_Orc_Story_Kurtrus_Mission1ExchangeB_02.prefab:59fcb77e61e41e540bbf7739a25b9e58");

	private static readonly AssetReference VO_Story_Hero_Samuro_Male_Orc_Story_Kurtrus_Mission1HeroPower_01 = new AssetReference("VO_Story_Hero_Samuro_Male_Orc_Story_Kurtrus_Mission1HeroPower_01.prefab:afb84a07bc1da1042917d53a448a885a");

	private static readonly AssetReference VO_Story_Hero_Samuro_Male_Orc_Story_Kurtrus_Mission1HeroPower_02 = new AssetReference("VO_Story_Hero_Samuro_Male_Orc_Story_Kurtrus_Mission1HeroPower_02.prefab:b72e9a2077fffb24b9bf083801462fc2");

	private static readonly AssetReference VO_Story_Hero_Samuro_Male_Orc_Story_Kurtrus_Mission1HeroPower_03 = new AssetReference("VO_Story_Hero_Samuro_Male_Orc_Story_Kurtrus_Mission1HeroPower_03.prefab:ebff50ff7a024924c95df2504d733bc8");

	private static readonly AssetReference VO_Story_Hero_Samuro_Male_Orc_Story_Kurtrus_Mission1Idle_01 = new AssetReference("VO_Story_Hero_Samuro_Male_Orc_Story_Kurtrus_Mission1Idle_01.prefab:16193ddb3091aea4598cdfadebc8b671");

	private static readonly AssetReference VO_Story_Hero_Samuro_Male_Orc_Story_Kurtrus_Mission1Idle_02 = new AssetReference("VO_Story_Hero_Samuro_Male_Orc_Story_Kurtrus_Mission1Idle_02.prefab:9f78728064f20e1448e1d2725c998a24");

	private static readonly AssetReference VO_Story_Hero_Samuro_Male_Orc_Story_Kurtrus_Mission1Idle_03 = new AssetReference("VO_Story_Hero_Samuro_Male_Orc_Story_Kurtrus_Mission1Idle_03.prefab:e0d48e9f39510954b811bf8f96b6bf43");

	private static readonly AssetReference VO_Story_Hero_Samuro_Male_Orc_Story_Kurtrus_Mission1Intro_02 = new AssetReference("VO_Story_Hero_Samuro_Male_Orc_Story_Kurtrus_Mission1Intro_02.prefab:a7283746c74ac794080bb10f77d6360f");

	private static readonly AssetReference VO_Story_Hero_Samuro_Male_Orc_Story_Kurtrus_Mission1Loss_01 = new AssetReference("VO_Story_Hero_Samuro_Male_Orc_Story_Kurtrus_Mission1Loss_01.prefab:512af5e1462731645bcc629d151efd66");

	private static readonly AssetReference VO_Story_Hero_Samuro_Male_Orc_Story_Kurtrus_Mission1Victory_01 = new AssetReference("VO_Story_Hero_Samuro_Male_Orc_Story_Kurtrus_Mission1Victory_01.prefab:4b96546c0b5ab8d4a8709e0771577633");

	private List<string> m_VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission1ExchangeDLines = new List<string> { VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission1ExchangeD_01, VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission1ExchangeD_02 };

	private List<string> m_VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission1ExchangeELines = new List<string> { VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission1ExchangeE_01, VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission1ExchangeE_02 };

	private List<string> m_InGame_BossIdleLines = new List<string> { VO_Story_Hero_Samuro_Male_Orc_Story_Kurtrus_Mission1Idle_01, VO_Story_Hero_Samuro_Male_Orc_Story_Kurtrus_Mission1Idle_02, VO_Story_Hero_Samuro_Male_Orc_Story_Kurtrus_Mission1Idle_03 };

	private List<string> m_InGame_BossHeroPowerLines = new List<string> { VO_Story_Hero_Samuro_Male_Orc_Story_Kurtrus_Mission1HeroPower_01, VO_Story_Hero_Samuro_Male_Orc_Story_Kurtrus_Mission1HeroPower_02, VO_Story_Hero_Samuro_Male_Orc_Story_Kurtrus_Mission1HeroPower_03 };

	private HashSet<string> m_playedLines = new HashSet<string>();

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> m_SoundFiles = new List<string>
		{
			VO_Story_Hero_Cariel_Female_Human_Story_Kurtrus_Mission1ExchangeB_01, VO_Story_Hero_Cariel_Female_Human_Story_Kurtrus_Mission1ExchangeC_02, VO_Story_Hero_Cariel_Female_Human_Story_Kurtrus_Mission1ExchangeD_03, VO_Story_Hero_Cariel_Female_Human_Story_Kurtrus_Mission1ExchangeF_02, VO_Story_Hero_Cariel_Female_Human_Story_Kurtrus_Mission1ExchangeG_01, VO_Story_Hero_Cariel_Female_Human_Story_Kurtrus_Mission1ExchangeH_01, VO_Story_Hero_Cariel_Female_Human_Story_Kurtrus_Mission1Victory_02, VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission1ExchangeA_01, VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission1ExchangeC_01, VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission1ExchangeD_01,
			VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission1ExchangeD_02, VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission1ExchangeE_01, VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission1ExchangeE_02, VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission1ExchangeF_01, VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission1ExchangeG_02, VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission1ExchangeH_02, VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission1Intro_01, VO_Story_Hero_Samuro_Male_Orc_Story_Kurtrus_Mission1Death_01, VO_Story_Hero_Samuro_Male_Orc_Story_Kurtrus_Mission1EmoteResponse_01, VO_Story_Hero_Samuro_Male_Orc_Story_Kurtrus_Mission1ExchangeA_02,
			VO_Story_Hero_Samuro_Male_Orc_Story_Kurtrus_Mission1ExchangeB_02, VO_Story_Hero_Samuro_Male_Orc_Story_Kurtrus_Mission1HeroPower_01, VO_Story_Hero_Samuro_Male_Orc_Story_Kurtrus_Mission1HeroPower_02, VO_Story_Hero_Samuro_Male_Orc_Story_Kurtrus_Mission1HeroPower_03, VO_Story_Hero_Samuro_Male_Orc_Story_Kurtrus_Mission1Idle_01, VO_Story_Hero_Samuro_Male_Orc_Story_Kurtrus_Mission1Idle_02, VO_Story_Hero_Samuro_Male_Orc_Story_Kurtrus_Mission1Idle_03, VO_Story_Hero_Samuro_Male_Orc_Story_Kurtrus_Mission1Intro_02, VO_Story_Hero_Samuro_Male_Orc_Story_Kurtrus_Mission1Loss_01, VO_Story_Hero_Samuro_Male_Orc_Story_Kurtrus_Mission1Victory_01
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
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission1Intro_01);
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Samuro_Male_Orc_Story_Kurtrus_Mission1Intro_02);
			break;
		case 506:
			MissionPause(pause: true);
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Samuro_Male_Orc_Story_Kurtrus_Mission1Loss_01);
			MissionPause(pause: false);
			break;
		case 504:
			MissionPause(pause: true);
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Samuro_Male_Orc_Story_Kurtrus_Mission1Victory_01);
			MissionPause(pause: false);
			break;
		case 505:
			MissionPause(pause: true);
			yield return MissionPlayVO("BOM_04_Cariel_001t", VO_Story_Hero_Cariel_Female_Human_Story_Kurtrus_Mission1Victory_02);
			MissionPause(pause: false);
			break;
		case 517:
			yield return MissionPlayVO(enemyActor, m_InGame_BossIdleLines);
			break;
		case 515:
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Samuro_Male_Orc_Story_Kurtrus_Mission1EmoteResponse_01);
			break;
		case 510:
			yield return MissionPlayVO(enemyActor, m_InGame_BossHeroPowerLines);
			break;
		case 516:
			yield return MissionPlaySound(enemyActor, VO_Story_Hero_Samuro_Male_Orc_Story_Kurtrus_Mission1Death_01);
			break;
		case 100:
			if (shouldPlayBanterVO())
			{
				yield return MissionPlayVOOnce("BOM_04_Cariel_001t", VO_Story_Hero_Cariel_Female_Human_Story_Kurtrus_Mission1ExchangeH_01);
				yield return MissionPlayVOOnce(friendlyActor, VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission1ExchangeH_02);
			}
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
			Actor friendlyActor = GameState.Get().GetFriendlySidePlayer().GetHero()
				.GetCard()
				.GetActor();
			if (cardID == "BT_354")
			{
				yield return MissionPlayVO("BOM_04_Cariel_001t", VO_Story_Hero_Cariel_Female_Human_Story_Kurtrus_Mission1ExchangeG_01);
				yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission1ExchangeG_02);
			}
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
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission1ExchangeA_01);
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Samuro_Male_Orc_Story_Kurtrus_Mission1ExchangeA_02);
			break;
		case 3:
			yield return MissionPlayVO("BOM_04_Cariel_001t", VO_Story_Hero_Cariel_Female_Human_Story_Kurtrus_Mission1ExchangeB_01);
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Samuro_Male_Orc_Story_Kurtrus_Mission1ExchangeB_02);
			break;
		case 7:
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission1ExchangeC_01);
			yield return MissionPlayVO("BOM_04_Cariel_001t", VO_Story_Hero_Cariel_Female_Human_Story_Kurtrus_Mission1ExchangeC_02);
			break;
		case 11:
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission1ExchangeD_01);
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission1ExchangeD_02);
			yield return MissionPlayVO("BOM_04_Cariel_001t", VO_Story_Hero_Cariel_Female_Human_Story_Kurtrus_Mission1ExchangeD_03);
			break;
		case 13:
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission1ExchangeE_01);
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission1ExchangeE_02);
			break;
		case 15:
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Kurtrus_Male_NightElf_Story_Kurtrus_Mission1ExchangeF_01);
			yield return MissionPlayVO("BOM_04_Cariel_001t", VO_Story_Hero_Cariel_Female_Human_Story_Kurtrus_Mission1ExchangeF_02);
			break;
		}
	}
}
