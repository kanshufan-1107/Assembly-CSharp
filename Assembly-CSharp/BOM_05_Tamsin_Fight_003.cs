using System.Collections;
using System.Collections.Generic;

public class BOM_05_Tamsin_Fight_003 : BOM_05_Tamsin_Dungeon
{
	private static readonly AssetReference VO_Story_Hero_Brukan_Male_Troll_BOM_Tamsin_Mission3ExchangeB_01 = new AssetReference("VO_Story_Hero_Brukan_Male_Troll_BOM_Tamsin_Mission3ExchangeB_01.prefab:383a3ac74acf1bf4c87a2f08cd1e84ef");

	private static readonly AssetReference VO_Story_Hero_Brukan_Male_Troll_BOM_Tamsin_Mission3ExchangeB_03 = new AssetReference("VO_Story_Hero_Brukan_Male_Troll_BOM_Tamsin_Mission3ExchangeB_03.prefab:04bba726a602cc041a1dd042cacb827f");

	private static readonly AssetReference VO_Story_Hero_Brukan_Male_Troll_BOM_Tamsin_Mission3ExchangeF_01 = new AssetReference("VO_Story_Hero_Brukan_Male_Troll_BOM_Tamsin_Mission3ExchangeF_01.prefab:1f6df8d09ca5e2442b7c037b29fb7d76");

	private static readonly AssetReference VO_Story_Hero_Brukan_Male_Troll_BOM_Tamsin_Mission3Victory_01 = new AssetReference("VO_Story_Hero_Brukan_Male_Troll_BOM_Tamsin_Mission3Victory_01.prefab:fc2020bf9d6dc09429b4fa915ff39d27");

	private static readonly AssetReference VO_Story_Hero_Elfarran_Female_NightElf_BOM_Tamsin_Mission3Death_01 = new AssetReference("VO_Story_Hero_Elfarran_Female_NightElf_BOM_Tamsin_Mission3Death_01.prefab:ac89fea7fac937941b00f7c936158c65");

	private static readonly AssetReference VO_Story_Hero_Elfarran_Female_NightElf_BOM_Tamsin_Mission3EmoteResponse_01 = new AssetReference("VO_Story_Hero_Elfarran_Female_NightElf_BOM_Tamsin_Mission3EmoteResponse_01.prefab:936e64cd9da3bc044ace7f943254ee2b");

	private static readonly AssetReference VO_Story_Hero_Elfarran_Female_NightElf_BOM_Tamsin_Mission3ExchangeA_01 = new AssetReference("VO_Story_Hero_Elfarran_Female_NightElf_BOM_Tamsin_Mission3ExchangeA_01.prefab:379c65d628d82cd4bb55821d8c6a3688");

	private static readonly AssetReference VO_Story_Hero_Elfarran_Female_NightElf_BOM_Tamsin_Mission3HeroPower_01 = new AssetReference("VO_Story_Hero_Elfarran_Female_NightElf_BOM_Tamsin_Mission3HeroPower_01.prefab:e293df90977b2534ba1e5ac9138d5945");

	private static readonly AssetReference VO_Story_Hero_Elfarran_Female_NightElf_BOM_Tamsin_Mission3HeroPower_02 = new AssetReference("VO_Story_Hero_Elfarran_Female_NightElf_BOM_Tamsin_Mission3HeroPower_02.prefab:bbaa1ee246c33874da68b342d1c4f0ce");

	private static readonly AssetReference VO_Story_Hero_Elfarran_Female_NightElf_BOM_Tamsin_Mission3HeroPower_03 = new AssetReference("VO_Story_Hero_Elfarran_Female_NightElf_BOM_Tamsin_Mission3HeroPower_03.prefab:a7e050ddaae773d42ad20e79e3380700");

	private static readonly AssetReference VO_Story_Hero_Elfarran_Female_NightElf_BOM_Tamsin_Mission3Idle_01 = new AssetReference("VO_Story_Hero_Elfarran_Female_NightElf_BOM_Tamsin_Mission3Idle_01.prefab:45ed74f41d4e0ae4bb46d5df274eccc5");

	private static readonly AssetReference VO_Story_Hero_Elfarran_Female_NightElf_BOM_Tamsin_Mission3Idle_02 = new AssetReference("VO_Story_Hero_Elfarran_Female_NightElf_BOM_Tamsin_Mission3Idle_02.prefab:535e388ac16473d438bffdfd12a854ca");

	private static readonly AssetReference VO_Story_Hero_Elfarran_Female_NightElf_BOM_Tamsin_Mission3Idle_03 = new AssetReference("VO_Story_Hero_Elfarran_Female_NightElf_BOM_Tamsin_Mission3Idle_03.prefab:fced72fa4ab2cc1459071e21148e647d");

	private static readonly AssetReference VO_Story_Hero_Elfarran_Female_NightElf_BOM_Tamsin_Mission3Intro_01 = new AssetReference("VO_Story_Hero_Elfarran_Female_NightElf_BOM_Tamsin_Mission3Intro_01.prefab:eb2fea45da8119849bd7a745f1c5ea29");

	private static readonly AssetReference VO_Story_Hero_Elfarran_Female_NightElf_BOM_Tamsin_Mission3Loss_01 = new AssetReference("VO_Story_Hero_Elfarran_Female_NightElf_BOM_Tamsin_Mission3Loss_01.prefab:a347be878a295ee4cbb1710b274f7def");

	private static readonly AssetReference VO_Story_Hero_Guff_Male_Tauren_BOM_Tamsin_Mission3ExchangeE_01 = new AssetReference("VO_Story_Hero_Guff_Male_Tauren_BOM_Tamsin_Mission3ExchangeE_01.prefab:e4ed28c5279d3914984c06bf3cf737d7");

	private static readonly AssetReference VO_Story_Hero_Rokara_Female_Orc_BOM_Tamsin_Mission3ExchangeD_02 = new AssetReference("VO_Story_Hero_Rokara_Female_Orc_BOM_Tamsin_Mission3ExchangeD_02.prefab:46aba2ddbf37205489bde6282758e3d4");

	private static readonly AssetReference VO_Story_Hero_Rokara_Female_Orc_BOM_Tamsin_Mission3ExchangeH_02 = new AssetReference("VO_Story_Hero_Rokara_Female_Orc_BOM_Tamsin_Mission3ExchangeH_02.prefab:76363ca7143aff149b0f12e7a5be9a90");

	private static readonly AssetReference VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission3ExchangeA_02 = new AssetReference("VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission3ExchangeA_02.prefab:d1d5d12cb182c4841b41ae1bcef49298");

	private static readonly AssetReference VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission3ExchangeB_02 = new AssetReference("VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission3ExchangeB_02.prefab:c06d6933852393846b4859dce192c06d");

	private static readonly AssetReference VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission3ExchangeC_01 = new AssetReference("VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission3ExchangeC_01.prefab:6df86f2ed57d90a47975264f762cc115");

	private static readonly AssetReference VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission3ExchangeC_02 = new AssetReference("VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission3ExchangeC_02.prefab:72029164304e72946b3f9bc6df916bce");

	private static readonly AssetReference VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission3ExchangeD_01 = new AssetReference("VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission3ExchangeD_01.prefab:ab4ad91c72a247946be3659f4e4b3ab4");

	private static readonly AssetReference VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission3ExchangeE_02 = new AssetReference("VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission3ExchangeE_02.prefab:12952df36d9754e46937084e68c2bd6a");

	private static readonly AssetReference VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission3ExchangeG_01 = new AssetReference("VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission3ExchangeG_01.prefab:29598e518fdf5894893d5d229e98c3f8");

	private static readonly AssetReference VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission3ExchangeH_01 = new AssetReference("VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission3ExchangeH_01.prefab:0a988a1ff99518a498ff34811fbbe423");

	private static readonly AssetReference VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission3Intro_02 = new AssetReference("VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission3Intro_02.prefab:895f9fd788a582240b7be1a764bd849c");

	private static readonly AssetReference VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission3Victory_02 = new AssetReference("VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission3Victory_02.prefab:560b465abd277a046b782ea039da658e");

	private static readonly AssetReference VO_PVPDR_Hero_Tamsin_Female_Forsaken_Death_01 = new AssetReference("VO_PVPDR_Hero_Tamsin_Female_Forsaken_Death_01.prefab:0261caf1dfbfae146bf927a03851b086");

	private List<string> m_InGame_BossUsesHeroPowerLines = new List<string> { VO_Story_Hero_Elfarran_Female_NightElf_BOM_Tamsin_Mission3HeroPower_01, VO_Story_Hero_Elfarran_Female_NightElf_BOM_Tamsin_Mission3HeroPower_02, VO_Story_Hero_Elfarran_Female_NightElf_BOM_Tamsin_Mission3HeroPower_03 };

	private List<string> m_InGame_BossIdleLines = new List<string> { VO_Story_Hero_Elfarran_Female_NightElf_BOM_Tamsin_Mission3Idle_01, VO_Story_Hero_Elfarran_Female_NightElf_BOM_Tamsin_Mission3Idle_02, VO_Story_Hero_Elfarran_Female_NightElf_BOM_Tamsin_Mission3Idle_03 };

	private List<string> m_InGame_IntroductionLines = new List<string> { VO_Story_Hero_Elfarran_Female_NightElf_BOM_Tamsin_Mission3Intro_01, VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission3Intro_02 };

	private HashSet<string> m_playedLines = new HashSet<string>();

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> m_SoundFiles = new List<string>
		{
			VO_Story_Hero_Brukan_Male_Troll_BOM_Tamsin_Mission3ExchangeB_01, VO_Story_Hero_Brukan_Male_Troll_BOM_Tamsin_Mission3ExchangeB_03, VO_Story_Hero_Brukan_Male_Troll_BOM_Tamsin_Mission3ExchangeF_01, VO_Story_Hero_Brukan_Male_Troll_BOM_Tamsin_Mission3Victory_01, VO_Story_Hero_Elfarran_Female_NightElf_BOM_Tamsin_Mission3Death_01, VO_Story_Hero_Elfarran_Female_NightElf_BOM_Tamsin_Mission3EmoteResponse_01, VO_Story_Hero_Elfarran_Female_NightElf_BOM_Tamsin_Mission3ExchangeA_01, VO_Story_Hero_Elfarran_Female_NightElf_BOM_Tamsin_Mission3HeroPower_01, VO_Story_Hero_Elfarran_Female_NightElf_BOM_Tamsin_Mission3HeroPower_02, VO_Story_Hero_Elfarran_Female_NightElf_BOM_Tamsin_Mission3HeroPower_03,
			VO_Story_Hero_Elfarran_Female_NightElf_BOM_Tamsin_Mission3Idle_01, VO_Story_Hero_Elfarran_Female_NightElf_BOM_Tamsin_Mission3Idle_02, VO_Story_Hero_Elfarran_Female_NightElf_BOM_Tamsin_Mission3Idle_03, VO_Story_Hero_Elfarran_Female_NightElf_BOM_Tamsin_Mission3Intro_01, VO_Story_Hero_Elfarran_Female_NightElf_BOM_Tamsin_Mission3Loss_01, VO_Story_Hero_Guff_Male_Tauren_BOM_Tamsin_Mission3ExchangeE_01, VO_Story_Hero_Rokara_Female_Orc_BOM_Tamsin_Mission3ExchangeD_02, VO_Story_Hero_Rokara_Female_Orc_BOM_Tamsin_Mission3ExchangeH_02, VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission3ExchangeA_02, VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission3ExchangeB_02,
			VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission3ExchangeC_01, VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission3ExchangeC_02, VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission3ExchangeD_01, VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission3ExchangeE_02, VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission3ExchangeG_01, VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission3ExchangeH_01, VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission3Intro_02, VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission3Victory_02, VO_PVPDR_Hero_Tamsin_Female_Forsaken_Death_01
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
		switch (missionEvent)
		{
		case 516:
			yield return MissionPlaySound(enemyActor, VO_Story_Hero_Elfarran_Female_NightElf_BOM_Tamsin_Mission3Death_01);
			break;
		case 519:
			MissionPause(pause: true);
			yield return MissionPlaySound(friendlyActor, VO_PVPDR_Hero_Tamsin_Female_Forsaken_Death_01);
			MissionPause(pause: false);
			break;
		case 517:
			yield return MissionPlayVO(enemyActor, m_InGame_BossIdleLines);
			break;
		case 510:
			yield return MissionPlayVO(enemyActor, m_InGame_BossUsesHeroPowerLines);
			break;
		case 515:
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Elfarran_Female_NightElf_BOM_Tamsin_Mission3EmoteResponse_01);
			break;
		case 514:
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Elfarran_Female_NightElf_BOM_Tamsin_Mission3Intro_01);
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission3Intro_02);
			break;
		case 507:
			MissionPause(pause: true);
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Elfarran_Female_NightElf_BOM_Tamsin_Mission3Loss_01);
			MissionPause(pause: false);
			break;
		case 505:
			MissionPause(pause: true);
			yield return MissionPlayVO(Brukan_BrassRing, VO_Story_Hero_Brukan_Male_Troll_BOM_Tamsin_Mission3Victory_01);
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission3Victory_02);
			MissionPause(pause: false);
			break;
		case 100:
			yield return MissionPlayVO("SW_028t5", VO_Story_Hero_Rokara_Female_Orc_BOM_Tamsin_Mission3ExchangeH_02);
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission3ExchangeH_01);
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
			switch (cardID)
			{
			case "BOM_05_Brukan_03t":
				yield return MissionPlayVOOnce("BOM_05_Brukan_03t", VO_Story_Hero_Brukan_Male_Troll_BOM_Tamsin_Mission3ExchangeF_01);
				break;
			case "SW_028t5":
				yield return MissionPlayVOOnce(friendlyActor, VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission3ExchangeD_01);
				yield return MissionPlayVOOnce("SW_028t5", VO_Story_Hero_Rokara_Female_Orc_BOM_Tamsin_Mission3ExchangeD_02);
				break;
			case "BOM_05_Guff_03t":
				yield return MissionPlayVOOnce("BOM_05_Guff_03t", VO_Story_Hero_Guff_Male_Tauren_BOM_Tamsin_Mission3ExchangeE_01);
				yield return MissionPlayVOOnce(friendlyActor, VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission3ExchangeE_02);
				break;
			case "EX1_312":
			case "CORE_EX1_312":
			case "VAN_EX1_312":
				yield return MissionPlayVOOnce(friendlyActor, VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission3ExchangeG_01);
				break;
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
		case 3:
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Elfarran_Female_NightElf_BOM_Tamsin_Mission3ExchangeA_01);
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission3ExchangeA_02);
			break;
		case 7:
			yield return MissionPlayVO(Brukan_BrassRing, VO_Story_Hero_Brukan_Male_Troll_BOM_Tamsin_Mission3ExchangeB_01);
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission3ExchangeB_02);
			yield return MissionPlayVO(Brukan_BrassRing, VO_Story_Hero_Brukan_Male_Troll_BOM_Tamsin_Mission3ExchangeB_03);
			break;
		case 11:
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission3ExchangeC_01);
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission3ExchangeC_02);
			break;
		}
	}
}
