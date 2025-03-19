using System.Collections;
using System.Collections.Generic;

public class BOM_07_Scabbs_Fight_002 : BOM_07_Scabbs_Dungeon
{
	private static readonly AssetReference VO_Story_Hero_Prestor_Female_Human_BOM_Scabbs_Mission2EmoteResponse_01 = new AssetReference("VO_Story_Hero_Prestor_Female_Human_BOM_Scabbs_Mission2EmoteResponse_01.prefab:b7931e3741004615ad1684988cd75e1a");

	private static readonly AssetReference VO_Story_Hero_Prestor_Female_Human_BOM_Scabbs_Mission2ExchangeA_01 = new AssetReference("VO_Story_Hero_Prestor_Female_Human_BOM_Scabbs_Mission2ExchangeA_01.prefab:ca535a06fde14a7aba0710a0b8eef7ef");

	private static readonly AssetReference VO_Story_Hero_Prestor_Female_Human_BOM_Scabbs_Mission2ExchangeB_01 = new AssetReference("VO_Story_Hero_Prestor_Female_Human_BOM_Scabbs_Mission2ExchangeB_01.prefab:2d9d145cc54348f1ac6bae60260216dc");

	private static readonly AssetReference VO_Story_Hero_Prestor_Female_Human_BOM_Scabbs_Mission2ExchangeC_01 = new AssetReference("VO_Story_Hero_Prestor_Female_Human_BOM_Scabbs_Mission2ExchangeC_01.prefab:8bd8a41055a2484fbda4aa6aeef69939");

	private static readonly AssetReference VO_Story_Hero_Prestor_Female_Human_BOM_Scabbs_Mission2ExchangeD_01 = new AssetReference("VO_Story_Hero_Prestor_Female_Human_BOM_Scabbs_Mission2ExchangeD_01.prefab:2b94c8ebb069463198e3058ce2e151a1");

	private static readonly AssetReference VO_Story_Hero_Prestor_Female_Human_BOM_Scabbs_Mission2ExchangeE_01 = new AssetReference("VO_Story_Hero_Prestor_Female_Human_BOM_Scabbs_Mission2ExchangeE_01.prefab:080b5e20314f41d8b8e86912a2a74955");

	private static readonly AssetReference VO_Story_Hero_Prestor_Female_Human_BOM_Scabbs_Mission2ExchangeF_01 = new AssetReference("VO_Story_Hero_Prestor_Female_Human_BOM_Scabbs_Mission2ExchangeF_01.prefab:752e624c5243411cb4c5cccbfe44422d");

	private static readonly AssetReference VO_Story_Hero_Prestor_Female_Human_BOM_Scabbs_Mission2ExchangeG_01 = new AssetReference("VO_Story_Hero_Prestor_Female_Human_BOM_Scabbs_Mission2ExchangeG_01.prefab:53ebe5808fae4553b59e236fe6bc0c95");

	private static readonly AssetReference VO_Story_Hero_Prestor_Female_Human_BOM_Scabbs_Mission2HeroPower_01 = new AssetReference("VO_Story_Hero_Prestor_Female_Human_BOM_Scabbs_Mission2HeroPower_01.prefab:a5a27a7038d04e3c80acbe12af92a3de");

	private static readonly AssetReference VO_Story_Hero_Prestor_Female_Human_BOM_Scabbs_Mission2HeroPower_02 = new AssetReference("VO_Story_Hero_Prestor_Female_Human_BOM_Scabbs_Mission2HeroPower_02.prefab:9b22221773e746e58e1a8220946b9816");

	private static readonly AssetReference VO_Story_Hero_Prestor_Female_Human_BOM_Scabbs_Mission2HeroPower_03 = new AssetReference("VO_Story_Hero_Prestor_Female_Human_BOM_Scabbs_Mission2HeroPower_03.prefab:073a420775a84273a8d01d5be83b1856");

	private static readonly AssetReference VO_Story_Hero_Prestor_Female_Human_BOM_Scabbs_Mission2Idle_01 = new AssetReference("VO_Story_Hero_Prestor_Female_Human_BOM_Scabbs_Mission2Idle_01.prefab:3a68aa6d627943f492b6cec32171af4c");

	private static readonly AssetReference VO_Story_Hero_Prestor_Female_Human_BOM_Scabbs_Mission2Idle_02 = new AssetReference("VO_Story_Hero_Prestor_Female_Human_BOM_Scabbs_Mission2Idle_02.prefab:506031be3ead4a0da83af9c51b0417d4");

	private static readonly AssetReference VO_Story_Hero_Prestor_Female_Human_BOM_Scabbs_Mission2Idle_03 = new AssetReference("VO_Story_Hero_Prestor_Female_Human_BOM_Scabbs_Mission2Idle_03.prefab:0f1ca504c66141688bd95de2efcde70d");

	private static readonly AssetReference VO_Story_Hero_Prestor_Female_Human_BOM_Scabbs_Mission2Intro_01 = new AssetReference("VO_Story_Hero_Prestor_Female_Human_BOM_Scabbs_Mission2Intro_01.prefab:c3617e0edd3d4a2f9733f0a564541020");

	private static readonly AssetReference VO_Story_Hero_Prestor_Female_Human_BOM_Scabbs_Mission2Loss_01 = new AssetReference("VO_Story_Hero_Prestor_Female_Human_BOM_Scabbs_Mission2Loss_01.prefab:e3032d02837f4d21b5a7450bd273f7e7");

	private static readonly AssetReference VO_Story_Hero_Prestor_Female_Human_BOM_Scabbs_Mission2Victory_02 = new AssetReference("VO_Story_Hero_Prestor_Female_Human_BOM_Scabbs_Mission2Victory_02.prefab:3b3989c42bd14852b32cb8a6ceeee6ea");

	private static readonly AssetReference VO_Story_Hero_Prestor_Female_Human_BOM_Scabbs_Mission2Victory_03 = new AssetReference("VO_Story_Hero_Prestor_Female_Human_BOM_Scabbs_Mission2Victory_03.prefab:ea4d5605b30c487eb411349c0344f43e");

	private static readonly AssetReference VO_Story_Hero_Prestor_Female_Human_BOM_Scabbs_Mission2Victory_04 = new AssetReference("VO_Story_Hero_Prestor_Female_Human_BOM_Scabbs_Mission2Victory_04.prefab:9ce34a32d7b44dd78e6b79412ec621f4");

	private static readonly AssetReference VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission2ExchangeA_01 = new AssetReference("VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission2ExchangeA_01.prefab:94286ca6f80648ca8a976ea498904efa");

	private static readonly AssetReference VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission2ExchangeB_01 = new AssetReference("VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission2ExchangeB_01.prefab:4539a56158b540fa9c2c6f53af1a6f57");

	private static readonly AssetReference VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission2ExchangeC_01 = new AssetReference("VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission2ExchangeC_01.prefab:e072e5416a764dcfba4871d7da98bb89");

	private static readonly AssetReference VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission2ExchangeD_01 = new AssetReference("VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission2ExchangeD_01.prefab:0145d0e24c5b4e85bf3a8694aaf3a778");

	private static readonly AssetReference VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission2ExchangeE_01 = new AssetReference("VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission2ExchangeE_01.prefab:9349014d91c944edb785fe0acd3f8c11");

	private static readonly AssetReference VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission2ExchangeF_01 = new AssetReference("VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission2ExchangeF_01.prefab:cf33545700ab4af29598c4aa28459638");

	private static readonly AssetReference VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission2Intro_01 = new AssetReference("VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission2Intro_01.prefab:0bd02301a6d941a39ccc6dd81dd54c7f");

	private static readonly AssetReference VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission2Victory_01 = new AssetReference("VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission2Victory_01.prefab:b71c7e4320734b39936d791f358c9941");

	private static readonly AssetReference VO_Story_Minion_DragonspawnDefender_Male_Dragonspawn_BOM_Scabbs_Mission2Attack_01 = new AssetReference("VO_Story_Minion_DragonspawnDefender_Male_Dragonspawn_BOM_Scabbs_Mission2Attack_01.prefab:d21c378d593a4759a6649cf0a57ef364");

	private static readonly AssetReference VO_Story_Minion_DragonspawnDefender_Male_Dragonspawn_BOM_Scabbs_Mission2Death_01 = new AssetReference("VO_Story_Minion_DragonspawnDefender_Male_Dragonspawn_BOM_Scabbs_Mission2Death_01.prefab:272a10692b1941c2a71f99dde0dd7e1b");

	private static readonly AssetReference VO_Story_Minion_DragonspawnDefender_Male_Dragonspawn_BOM_Scabbs_Mission2Play_01 = new AssetReference("VO_Story_Minion_DragonspawnDefender_Male_Dragonspawn_BOM_Scabbs_Mission2Play_01.prefab:75a93cb1e3f348f8924c50bb7835a519");

	private static readonly AssetReference VO_Story_Hero_Prestor_Female_Human_BOM_Scabbs_Mission2Death_01 = new AssetReference("VO_Story_Hero_Prestor_Female_Human_BOM_Scabbs_Mission2Death_01.prefab:1a6ebb20178241c7b60aaad060ef96c9");

	private static readonly AssetReference VO_PVPDR_Hero_Scabbs_Male_Gnome_Death_01 = new AssetReference("VO_PVPDR_Hero_Scabbs_Male_Gnome_Death_01.prefab:85e10a0b219b1974aaa959bd523c554b");

	private List<string> m_InGame_BossUsesHeroPowerLines = new List<string> { VO_Story_Hero_Prestor_Female_Human_BOM_Scabbs_Mission2HeroPower_01, VO_Story_Hero_Prestor_Female_Human_BOM_Scabbs_Mission2HeroPower_02, VO_Story_Hero_Prestor_Female_Human_BOM_Scabbs_Mission2HeroPower_03 };

	private List<string> m_InGame_BossIdleLines = new List<string> { VO_Story_Hero_Prestor_Female_Human_BOM_Scabbs_Mission2Idle_01, VO_Story_Hero_Prestor_Female_Human_BOM_Scabbs_Mission2Idle_02, VO_Story_Hero_Prestor_Female_Human_BOM_Scabbs_Mission2Idle_03 };

	private HashSet<string> m_playedLines = new HashSet<string>();

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> m_SoundFiles = new List<string>
		{
			VO_Story_Hero_Prestor_Female_Human_BOM_Scabbs_Mission2Death_01, VO_Story_Hero_Prestor_Female_Human_BOM_Scabbs_Mission2EmoteResponse_01, VO_Story_Hero_Prestor_Female_Human_BOM_Scabbs_Mission2ExchangeA_01, VO_Story_Hero_Prestor_Female_Human_BOM_Scabbs_Mission2ExchangeB_01, VO_Story_Hero_Prestor_Female_Human_BOM_Scabbs_Mission2ExchangeC_01, VO_Story_Hero_Prestor_Female_Human_BOM_Scabbs_Mission2ExchangeD_01, VO_Story_Hero_Prestor_Female_Human_BOM_Scabbs_Mission2ExchangeE_01, VO_Story_Hero_Prestor_Female_Human_BOM_Scabbs_Mission2ExchangeF_01, VO_Story_Hero_Prestor_Female_Human_BOM_Scabbs_Mission2ExchangeG_01, VO_Story_Hero_Prestor_Female_Human_BOM_Scabbs_Mission2HeroPower_01,
			VO_Story_Hero_Prestor_Female_Human_BOM_Scabbs_Mission2HeroPower_02, VO_Story_Hero_Prestor_Female_Human_BOM_Scabbs_Mission2HeroPower_03, VO_Story_Hero_Prestor_Female_Human_BOM_Scabbs_Mission2Idle_01, VO_Story_Hero_Prestor_Female_Human_BOM_Scabbs_Mission2Idle_02, VO_Story_Hero_Prestor_Female_Human_BOM_Scabbs_Mission2Idle_03, VO_Story_Hero_Prestor_Female_Human_BOM_Scabbs_Mission2Intro_01, VO_Story_Hero_Prestor_Female_Human_BOM_Scabbs_Mission2Loss_01, VO_Story_Hero_Prestor_Female_Human_BOM_Scabbs_Mission2Victory_02, VO_Story_Hero_Prestor_Female_Human_BOM_Scabbs_Mission2Victory_03, VO_Story_Hero_Prestor_Female_Human_BOM_Scabbs_Mission2Victory_04,
			VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission2ExchangeA_01, VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission2ExchangeB_01, VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission2ExchangeC_01, VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission2ExchangeD_01, VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission2ExchangeE_01, VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission2ExchangeF_01, VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission2Intro_01, VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission2Victory_01, VO_Story_Minion_DragonspawnDefender_Male_Dragonspawn_BOM_Scabbs_Mission2Attack_01, VO_Story_Minion_DragonspawnDefender_Male_Dragonspawn_BOM_Scabbs_Mission2Death_01,
			VO_Story_Minion_DragonspawnDefender_Male_Dragonspawn_BOM_Scabbs_Mission2Play_01, VO_PVPDR_Hero_Scabbs_Male_Gnome_Death_01
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
			yield return MissionPlaySound(enemyActor, VO_Story_Hero_Prestor_Female_Human_BOM_Scabbs_Mission2Death_01);
			break;
		case 517:
			yield return MissionPlayVO(enemyActor, m_InGame_BossIdleLines);
			break;
		case 510:
			yield return MissionPlayVO(enemyActor, m_InGame_BossUsesHeroPowerLines);
			break;
		case 515:
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Prestor_Female_Human_BOM_Scabbs_Mission2EmoteResponse_01);
			break;
		case 514:
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission2Intro_01);
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Prestor_Female_Human_BOM_Scabbs_Mission2Intro_01);
			break;
		case 507:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Prestor_Female_Human_BOM_Scabbs_Mission2Loss_01);
			GameState.Get().SetBusy(busy: false);
			break;
		case 900:
			yield return MissionPlaySound("BOM_07_Scabbs_DragonspawnDefender_002t", VO_Story_Minion_DragonspawnDefender_Male_Dragonspawn_BOM_Scabbs_Mission2Death_01);
			break;
		case 901:
			yield return MissionPlayVO("BOM_07_Scabbs_DragonspawnDefender_002t", VO_Story_Minion_DragonspawnDefender_Male_Dragonspawn_BOM_Scabbs_Mission2Play_01);
			break;
		case 519:
			yield return MissionPlaySound(enemyActor, VO_PVPDR_Hero_Scabbs_Male_Gnome_Death_01);
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
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Prestor_Female_Human_BOM_Scabbs_Mission2ExchangeA_01);
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission2ExchangeA_01);
			break;
		case 5:
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission2ExchangeB_01);
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Prestor_Female_Human_BOM_Scabbs_Mission2ExchangeB_01);
			break;
		case 7:
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission2ExchangeC_01);
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Prestor_Female_Human_BOM_Scabbs_Mission2ExchangeC_01);
			break;
		case 9:
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission2ExchangeD_01);
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Prestor_Female_Human_BOM_Scabbs_Mission2ExchangeD_01);
			break;
		case 11:
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission2ExchangeE_01);
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Prestor_Female_Human_BOM_Scabbs_Mission2ExchangeE_01);
			break;
		case 13:
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission2ExchangeF_01);
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Prestor_Female_Human_BOM_Scabbs_Mission2ExchangeF_01);
			break;
		case 15:
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Prestor_Female_Human_BOM_Scabbs_Mission2ExchangeG_01);
			break;
		}
	}
}
