using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BOM_05_Tamsin_Fight_007 : BOM_05_Tamsin_Dungeon
{
	private static readonly AssetReference VO_Story_Hero_Anetheron_Male_Demon_BOM_Tamsin_Mission7ExchangeF_01 = new AssetReference("VO_Story_Hero_Anetheron_Male_Demon_BOM_Tamsin_Mission7ExchangeF_01.prefab:931651d7895599740a269cf60bf08f6b");

	private static readonly AssetReference VO_Story_Hero_Bolvar_Male_Human_BOM_Tamsin_Mission7Death_01 = new AssetReference("VO_Story_Hero_Bolvar_Male_Human_BOM_Tamsin_Mission7Death_01.prefab:ea8847f4a1a9487488d87209f8cfd5a3");

	private static readonly AssetReference VO_Story_Hero_Bolvar_Male_Human_BOM_Tamsin_Mission7EmoteResponse_01 = new AssetReference("VO_Story_Hero_Bolvar_Male_Human_BOM_Tamsin_Mission7EmoteResponse_01.prefab:fdfa7962760a91b40a98f4d03f72b2c7");

	private static readonly AssetReference VO_Story_Hero_Bolvar_Male_Human_BOM_Tamsin_Mission7ExchangeA_01 = new AssetReference("VO_Story_Hero_Bolvar_Male_Human_BOM_Tamsin_Mission7ExchangeA_01.prefab:771eadcef1414394388b5b12c3ebe3ba");

	private static readonly AssetReference VO_Story_Hero_Bolvar_Male_Human_BOM_Tamsin_Mission7ExchangeB_01 = new AssetReference("VO_Story_Hero_Bolvar_Male_Human_BOM_Tamsin_Mission7ExchangeB_01.prefab:e6cfb4835b9864a438551db68e300f82");

	private static readonly AssetReference VO_Story_Hero_Bolvar_Male_Human_BOM_Tamsin_Mission7ExchangeC_01 = new AssetReference("VO_Story_Hero_Bolvar_Male_Human_BOM_Tamsin_Mission7ExchangeC_01.prefab:867f4c52f84254d46914b343f89056a2");

	private static readonly AssetReference VO_Story_Hero_Bolvar_Male_Human_BOM_Tamsin_Mission7ExchangeH_02 = new AssetReference("VO_Story_Hero_Bolvar_Male_Human_BOM_Tamsin_Mission7ExchangeH_02.prefab:873ebce06091230479574d30bf327e41");

	private static readonly AssetReference VO_Story_Hero_Bolvar_Male_Human_BOM_Tamsin_Mission7HeroPower_01 = new AssetReference("VO_Story_Hero_Bolvar_Male_Human_BOM_Tamsin_Mission7HeroPower_01.prefab:93286ffb319b5c34cac43d41c611b452");

	private static readonly AssetReference VO_Story_Hero_Bolvar_Male_Human_BOM_Tamsin_Mission7HeroPower_02 = new AssetReference("VO_Story_Hero_Bolvar_Male_Human_BOM_Tamsin_Mission7HeroPower_02.prefab:847c9b8bc5e1dec4a96dccb3bb801a4a");

	private static readonly AssetReference VO_Story_Hero_Bolvar_Male_Human_BOM_Tamsin_Mission7HeroPower_03 = new AssetReference("VO_Story_Hero_Bolvar_Male_Human_BOM_Tamsin_Mission7HeroPower_03.prefab:f086ecb21885da74b8af3a4ca3918c7f");

	private static readonly AssetReference VO_Story_Hero_Bolvar_Male_Human_BOM_Tamsin_Mission7HeroPower_04 = new AssetReference("VO_Story_Hero_Bolvar_Male_Human_BOM_Tamsin_Mission7HeroPower_04.prefab:d3348436c9efc9e44943379e0fafdf26");

	private static readonly AssetReference VO_Story_Hero_Bolvar_Male_Human_BOM_Tamsin_Mission7HeroPower_05 = new AssetReference("VO_Story_Hero_Bolvar_Male_Human_BOM_Tamsin_Mission7HeroPower_05.prefab:3386f4dec099266428ff90c211b40b16");

	private static readonly AssetReference VO_Story_Hero_Bolvar_Male_Human_BOM_Tamsin_Mission7Idle_01 = new AssetReference("VO_Story_Hero_Bolvar_Male_Human_BOM_Tamsin_Mission7Idle_01.prefab:a0cbf7a097e882949a5e8a5e92020f0c");

	private static readonly AssetReference VO_Story_Hero_Bolvar_Male_Human_BOM_Tamsin_Mission7Idle_02 = new AssetReference("VO_Story_Hero_Bolvar_Male_Human_BOM_Tamsin_Mission7Idle_02.prefab:5234484865f41aa48ba39b03c8ba6333");

	private static readonly AssetReference VO_Story_Hero_Bolvar_Male_Human_BOM_Tamsin_Mission7Idle_03 = new AssetReference("VO_Story_Hero_Bolvar_Male_Human_BOM_Tamsin_Mission7Idle_03.prefab:40b2ce2f73909794dacf8c7db6dd40b2");

	private static readonly AssetReference VO_Story_Hero_Bolvar_Male_Human_BOM_Tamsin_Mission7Intro_01 = new AssetReference("VO_Story_Hero_Bolvar_Male_Human_BOM_Tamsin_Mission7Intro_01.prefab:044eb012009b16e40af75951b6183fd1");

	private static readonly AssetReference VO_Story_Hero_Bolvar_Male_Human_BOM_Tamsin_Mission7Loss_01 = new AssetReference("VO_Story_Hero_Bolvar_Male_Human_BOM_Tamsin_Mission7Loss_01.prefab:22938c135fc6eaf409548543146230f2");

	private static readonly AssetReference VO_Story_Hero_Bolvar_Male_Human_BOM_Tamsin_Mission7Victory_01 = new AssetReference("VO_Story_Hero_Bolvar_Male_Human_BOM_Tamsin_Mission7Victory_01.prefab:2bd26e40f45fc0049bb387fd8a4b5e85");

	private static readonly AssetReference VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission7ExchangeA_02 = new AssetReference("VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission7ExchangeA_02.prefab:6e7ccceeaaab1114ab581ce1c5e4790f");

	private static readonly AssetReference VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission7ExchangeB_02 = new AssetReference("VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission7ExchangeB_02.prefab:2d974788300efb846a767b83690de3d7");

	private static readonly AssetReference VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission7ExchangeC_02 = new AssetReference("VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission7ExchangeC_02.prefab:627ff9431c70ab244b1ecd1be19b410c");

	private static readonly AssetReference VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission7ExchangeD_01 = new AssetReference("VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission7ExchangeD_01.prefab:8c8b432f50a64184982c0cdbaa555d0b");

	private static readonly AssetReference VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission7ExchangeE_01 = new AssetReference("VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission7ExchangeE_01.prefab:4957dbcc435c59843aa6278fff500ad7");

	private static readonly AssetReference VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission7ExchangeF_02 = new AssetReference("VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission7ExchangeF_02.prefab:7cd80246af6489845ae3d156bac874aa");

	private static readonly AssetReference VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission7ExchangeG_01 = new AssetReference("VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission7ExchangeG_01.prefab:bcae906df71d68c4f9f7a31581d646e9");

	private static readonly AssetReference VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission7Intro_02 = new AssetReference("VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission7Intro_02.prefab:79fc66027819c0449b7726e71ab26a7a");

	private static readonly AssetReference VO_Story_Minion_Anetheron_Male_Demon_BOM_Tamsin_Mission7Attack_01 = new AssetReference("VO_Story_Minion_Anetheron_Male_Demon_BOM_Tamsin_Mission7Attack_01.prefab:98c3612baf476be49a5e8c7709630ded");

	private static readonly AssetReference VO_Story_Minion_Anetheron_Male_Demon_BOM_Tamsin_Mission7Attack_02 = new AssetReference("VO_Story_Minion_Anetheron_Male_Demon_BOM_Tamsin_Mission7Attack_02.prefab:a64ac84771307fb47abcb36320879ea9");

	private static readonly AssetReference VO_Story_Minion_Anetheron_Male_Demon_BOM_Tamsin_Mission7Attack_03 = new AssetReference("VO_Story_Minion_Anetheron_Male_Demon_BOM_Tamsin_Mission7Attack_03.prefab:13cfce4a2b1033844920b35fbb3a353f");

	private static readonly AssetReference VO_Story_Minion_Anetheron_Male_Demon_BOM_Tamsin_Mission7Death_01 = new AssetReference("VO_Story_Minion_Anetheron_Male_Demon_BOM_Tamsin_Mission7Death_01.prefab:ee73493b8ff694b459cd2ce1f38dbc7b");

	private static readonly AssetReference VO_Story_Minion_Anetheron_Male_Demon_BOM_Tamsin_Mission7Play_01 = new AssetReference("VO_Story_Minion_Anetheron_Male_Demon_BOM_Tamsin_Mission7Play_01.prefab:3fba50cb5fbb26042875418d8433c3b3");

	private static readonly AssetReference VO_PVPDR_Hero_Tamsin_Female_Forsaken_Death_01 = new AssetReference("VO_PVPDR_Hero_Tamsin_Female_Forsaken_Death_01.prefab:0261caf1dfbfae146bf927a03851b086");

	private Dictionary<int, string[]> m_popUpInfo = new Dictionary<int, string[]> { 
	{
		228,
		new string[1] { "BOM_TAMSIN_07" }
	} };

	private float popUpScale = 1.25f;

	private Vector3 popUpPos;

	private List<string> m_InGame_BossUsesHeroPowerLines = new List<string> { VO_Story_Hero_Bolvar_Male_Human_BOM_Tamsin_Mission7HeroPower_01, VO_Story_Hero_Bolvar_Male_Human_BOM_Tamsin_Mission7HeroPower_02, VO_Story_Hero_Bolvar_Male_Human_BOM_Tamsin_Mission7HeroPower_03, VO_Story_Hero_Bolvar_Male_Human_BOM_Tamsin_Mission7HeroPower_04, VO_Story_Hero_Bolvar_Male_Human_BOM_Tamsin_Mission7HeroPower_05 };

	private List<string> m_InGame_BossIdleLines = new List<string> { VO_Story_Hero_Bolvar_Male_Human_BOM_Tamsin_Mission7Idle_01, VO_Story_Hero_Bolvar_Male_Human_BOM_Tamsin_Mission7Idle_02, VO_Story_Hero_Bolvar_Male_Human_BOM_Tamsin_Mission7Idle_03 };

	private List<string> m_InGame_IntroductionLines = new List<string> { VO_Story_Hero_Bolvar_Male_Human_BOM_Tamsin_Mission7Intro_01, VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission7Intro_02 };

	private HashSet<string> m_playedLines = new HashSet<string>();

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> m_SoundFiles = new List<string>
		{
			VO_Story_Hero_Anetheron_Male_Demon_BOM_Tamsin_Mission7ExchangeF_01, VO_Story_Hero_Bolvar_Male_Human_BOM_Tamsin_Mission7Death_01, VO_Story_Hero_Bolvar_Male_Human_BOM_Tamsin_Mission7EmoteResponse_01, VO_Story_Hero_Bolvar_Male_Human_BOM_Tamsin_Mission7ExchangeA_01, VO_Story_Hero_Bolvar_Male_Human_BOM_Tamsin_Mission7ExchangeB_01, VO_Story_Hero_Bolvar_Male_Human_BOM_Tamsin_Mission7ExchangeC_01, VO_Story_Hero_Bolvar_Male_Human_BOM_Tamsin_Mission7ExchangeH_02, VO_Story_Hero_Bolvar_Male_Human_BOM_Tamsin_Mission7HeroPower_01, VO_Story_Hero_Bolvar_Male_Human_BOM_Tamsin_Mission7HeroPower_02, VO_Story_Hero_Bolvar_Male_Human_BOM_Tamsin_Mission7HeroPower_03,
			VO_Story_Hero_Bolvar_Male_Human_BOM_Tamsin_Mission7HeroPower_04, VO_Story_Hero_Bolvar_Male_Human_BOM_Tamsin_Mission7HeroPower_05, VO_Story_Hero_Bolvar_Male_Human_BOM_Tamsin_Mission7Idle_01, VO_Story_Hero_Bolvar_Male_Human_BOM_Tamsin_Mission7Idle_02, VO_Story_Hero_Bolvar_Male_Human_BOM_Tamsin_Mission7Idle_03, VO_Story_Hero_Bolvar_Male_Human_BOM_Tamsin_Mission7Intro_01, VO_Story_Hero_Bolvar_Male_Human_BOM_Tamsin_Mission7Loss_01, VO_Story_Hero_Bolvar_Male_Human_BOM_Tamsin_Mission7Victory_01, VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission7ExchangeA_02, VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission7ExchangeB_02,
			VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission7ExchangeC_02, VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission7ExchangeD_01, VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission7ExchangeE_01, VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission7ExchangeF_02, VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission7ExchangeG_01, VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission7Intro_02, VO_PVPDR_Hero_Tamsin_Female_Forsaken_Death_01
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
		popUpPos = new Vector3(0f, 0f, -40f);
		switch (missionEvent)
		{
		case 516:
			yield return MissionPlaySound(enemyActor, VO_Story_Hero_Bolvar_Male_Human_BOM_Tamsin_Mission7Death_01);
			break;
		case 519:
			yield return MissionPlaySound(friendlyActor, VO_PVPDR_Hero_Tamsin_Female_Forsaken_Death_01);
			break;
		case 517:
			yield return MissionPlayVO(enemyActor, m_InGame_BossIdleLines);
			break;
		case 510:
			yield return MissionPlayVO(enemyActor, m_InGame_BossUsesHeroPowerLines);
			break;
		case 515:
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Bolvar_Male_Human_BOM_Tamsin_Mission7EmoteResponse_01);
			break;
		case 514:
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Bolvar_Male_Human_BOM_Tamsin_Mission7Intro_01);
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission7Intro_02);
			break;
		case 507:
			MissionPause(pause: true);
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Bolvar_Male_Human_BOM_Tamsin_Mission7Loss_01);
			MissionPause(pause: false);
			break;
		case 504:
			MissionPause(pause: true);
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Bolvar_Male_Human_BOM_Tamsin_Mission7Victory_01);
			MissionPause(pause: false);
			break;
		case 100:
			yield return MissionPlayVOOnce(friendlyActor, VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission7ExchangeD_01);
			break;
		case 101:
			yield return MissionPlayVOOnce(friendlyActor, VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission7ExchangeG_01);
			break;
		case 102:
			yield return MissionPlayVOOnce("BOM_05_Anetheron_07t", VO_Story_Hero_Anetheron_Male_Demon_BOM_Tamsin_Mission7ExchangeF_01);
			yield return MissionPlayVOOnce(friendlyActor, VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission7ExchangeF_02);
			break;
		case 228:
		{
			yield return new WaitForSeconds(2f);
			Notification popup = NotificationManager.Get().CreatePopupText(UserAttentionBlocker.NONE, popUpPos, TutorialEntity.GetTextScale() * popUpScale, GameStrings.Get(m_popUpInfo[missionEvent][0]), convertLegacyPosition: false, NotificationManager.PopupTextType.FANCY);
			NotificationManager.Get().DestroyNotification(popup, 7.5f);
			break;
		}
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
			if (cardID == "BAR_911")
			{
				yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission7ExchangeE_01);
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
			Actor enemyActor = GameState.Get().GetOpposingSidePlayer().GetHero()
				.GetCard()
				.GetActor();
			Actor friendlyActor = GameState.Get().GetFriendlySidePlayer().GetHero()
				.GetCard()
				.GetActor();
			switch (cardID)
			{
			case "AT_075":
			case "CORE_AT_075":
				yield return MissionPlayVO(enemyActor, VO_Story_Hero_Bolvar_Male_Human_BOM_Tamsin_Mission7ExchangeB_01);
				yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission7ExchangeB_02);
				break;
			case "CS2_131":
			case "VAN_CS2_131":
				yield return MissionPlayVO(enemyActor, VO_Story_Hero_Bolvar_Male_Human_BOM_Tamsin_Mission7ExchangeC_01);
				yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission7ExchangeC_02);
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
		if (turn == 3)
		{
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Bolvar_Male_Human_BOM_Tamsin_Mission7ExchangeA_01);
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission7ExchangeA_02);
		}
	}
}
