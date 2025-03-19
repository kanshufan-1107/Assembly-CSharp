using System.Collections;
using System.Collections.Generic;

public class BOM_10_Dawngrasp_Fight_008 : BOM_10_Dawngrasp_Dungeon
{
	private static readonly AssetReference VO_BOM_10_008_Female_Draenei_Runi_Death_01 = new AssetReference("VO_BOM_10_008_Female_Draenei_Runi_Death_01.prefab:ce1ffc1860e053949bc011d8a69db07c");

	private static readonly AssetReference VO_BOM_10_008_Female_Draenei_Runi_InGame_VictoryPostExplosion_01_A = new AssetReference("VO_BOM_10_008_Female_Draenei_Runi_InGame_VictoryPostExplosion_01_A.prefab:e6644cfee70445d4f84e85b9f6616c38");

	private static readonly AssetReference VO_BOM_10_008_Female_Draenei_Xyrella_InGame_Turn_07_01 = new AssetReference("VO_BOM_10_008_Female_Draenei_Xyrella_InGame_Turn_07_01.prefab:1cda5be5260d3da41919f10319e57b6f");

	private static readonly AssetReference VO_BOM_10_008_Female_Draenei_Xyrella_InGame_Turn_11_01_B = new AssetReference("VO_BOM_10_008_Female_Draenei_Xyrella_InGame_Turn_11_01_B.prefab:fd870a2452238ff4b8c5c6cf72130ea2");

	private static readonly AssetReference VO_BOM_10_008_Female_Draenei_Xyrella_InGame_VictoryPostExplosion_01_B = new AssetReference("VO_BOM_10_008_Female_Draenei_Xyrella_InGame_VictoryPostExplosion_01_B.prefab:b0164b5a8fadf0e4abcd56ba7ca4f552");

	private static readonly AssetReference VO_BOM_10_008_X_BloodElf_Dawngrasp_InGame_HE_Custom_IfBossAt15Health_01_A = new AssetReference("VO_BOM_10_008_X_BloodElf_Dawngrasp_InGame_HE_Custom_IfBossAt15Health_01_A.prefab:61d24690286094542b5cd772e7dfcf34");

	private static readonly AssetReference VO_BOM_10_008_X_BloodElf_Dawngrasp_InGame_Introduction_01_B = new AssetReference("VO_BOM_10_008_X_BloodElf_Dawngrasp_InGame_Introduction_01_B.prefab:767db95f8ab720d4db7ac34f018bec95");

	private static readonly AssetReference VO_BOM_10_008_X_BloodElf_Dawngrasp_InGame_VictoryPreExplosion_01_A = new AssetReference("VO_BOM_10_008_X_BloodElf_Dawngrasp_InGame_VictoryPreExplosion_01_A.prefab:3f75e17a4782b5540848ba20031fa068");

	private static readonly AssetReference VO_BOM_10_008_X_BloodElf_Dawngrasp_UI_AWD_Boss_Reveal_General_08_01_A = new AssetReference("VO_BOM_10_008_X_BloodElf_Dawngrasp_UI_AWD_Boss_Reveal_General_08_01_A.prefab:6501fe35a6dfe5144921b15718d922d3");

	private static readonly AssetReference VO_BOM_10_008_X_BloodElf_Dawngrasp_UI_AWD_Boss_Reveal_General_08_01_B = new AssetReference("VO_BOM_10_008_X_BloodElf_Dawngrasp_UI_AWD_Boss_Reveal_General_08_01_B.prefab:21c14e07f3344b84ab743d449c1cabfe");

	private static readonly AssetReference VO_BOM_10_008_X_BloodElf_Dawngrasp_UI_AWD_Boss_Reveal_General_09_01_A = new AssetReference("VO_BOM_10_008_X_BloodElf_Dawngrasp_UI_AWD_Boss_Reveal_General_09_01_A.prefab:ed2bebe43208d2d49837d4366783f9c4");

	private static readonly AssetReference VO_BOM_10_008_X_BloodElf_Dawngrasp_UI_AWD_Boss_Reveal_General_09_01_B = new AssetReference("VO_BOM_10_008_X_BloodElf_Dawngrasp_UI_AWD_Boss_Reveal_General_09_01_B.prefab:966a74d18c6b6b447a157c658e5fce9e");

	private static readonly AssetReference VO_BOM_10_008_X_BloodElf_Dawngrasp_UI_AWD_Boss_Reveal_General_09_01_C = new AssetReference("VO_BOM_10_008_X_BloodElf_Dawngrasp_UI_AWD_Boss_Reveal_General_09_01_C.prefab:e6f4e39402fe39643a013aac53df6d81");

	private static readonly AssetReference VO_BOM_10_008_X_Naaru_Mida_InGame_BossIdle_01 = new AssetReference("VO_BOM_10_008_X_Naaru_Mida_InGame_BossIdle_01.prefab:0f8dc09321963174382c624ca376ffcd");

	private static readonly AssetReference VO_BOM_10_008_X_Naaru_Mida_InGame_BossIdle_02 = new AssetReference("VO_BOM_10_008_X_Naaru_Mida_InGame_BossIdle_02.prefab:77719c5028cf07444a38e9140d663ec1");

	private static readonly AssetReference VO_BOM_10_008_X_Naaru_Mida_InGame_BossIdle_03 = new AssetReference("VO_BOM_10_008_X_Naaru_Mida_InGame_BossIdle_03.prefab:e33774d6310920c4f8c043ad1cb92532");

	private static readonly AssetReference VO_BOM_10_008_X_Naaru_Mida_InGame_BossUsesHeroPower_01 = new AssetReference("VO_BOM_10_008_X_Naaru_Mida_InGame_BossUsesHeroPower_01.prefab:7fcedf6ab6d341648b6b99b5ed32ee07");

	private static readonly AssetReference VO_BOM_10_008_X_Naaru_Mida_InGame_BossUsesHeroPower_02 = new AssetReference("VO_BOM_10_008_X_Naaru_Mida_InGame_BossUsesHeroPower_02.prefab:b406aa14f5c581148bcc33a4db465b5a");

	private static readonly AssetReference VO_BOM_10_008_X_Naaru_Mida_InGame_BossUsesHeroPower_03 = new AssetReference("VO_BOM_10_008_X_Naaru_Mida_InGame_BossUsesHeroPower_03.prefab:76c4757fada2a0f41a96477995d9d595");

	private static readonly AssetReference VO_BOM_10_008_X_Naaru_Mida_InGame_BossUsesHeroPower_04 = new AssetReference("VO_BOM_10_008_X_Naaru_Mida_InGame_BossUsesHeroPower_04.prefab:f2442c8499a9b6f43be72e0a5526a9e3");

	private static readonly AssetReference VO_BOM_10_008_X_Naaru_Mida_InGame_BossUsesHeroPower_05 = new AssetReference("VO_BOM_10_008_X_Naaru_Mida_InGame_BossUsesHeroPower_05.prefab:c0843b1d25c93b749bd1801575ad135f");

	private static readonly AssetReference VO_BOM_10_008_X_Naaru_Mida_InGame_BossUsesHeroPower_06 = new AssetReference("VO_BOM_10_008_X_Naaru_Mida_InGame_BossUsesHeroPower_06.prefab:cec9578e4e3be2743b3fcb24b98ef57c");

	private static readonly AssetReference VO_BOM_10_008_X_Naaru_Mida_InGame_BossUsesHeroPower_07 = new AssetReference("VO_BOM_10_008_X_Naaru_Mida_InGame_BossUsesHeroPower_07.prefab:0d8275133e3f934499ad9b009508f9bb");

	private static readonly AssetReference VO_BOM_10_008_X_Naaru_Mida_InGame_BossUsesHeroPower_08 = new AssetReference("VO_BOM_10_008_X_Naaru_Mida_InGame_BossUsesHeroPower_08.prefab:ccfe459d388ac76489dae787f1e1c0fd");

	private static readonly AssetReference VO_BOM_10_008_X_Naaru_Mida_InGame_BossUsesHeroPower_09 = new AssetReference("VO_BOM_10_008_X_Naaru_Mida_InGame_BossUsesHeroPower_09.prefab:852d1e7163bc59244b4474f51d048c74");

	private static readonly AssetReference VO_BOM_10_008_X_Naaru_Mida_InGame_BossUsesHeroPower_10 = new AssetReference("VO_BOM_10_008_X_Naaru_Mida_InGame_BossUsesHeroPower_10.prefab:6f2a5ffe3a62dc04390657ef310a1524");

	private static readonly AssetReference VO_BOM_10_008_X_Naaru_Mida_InGame_BossUsesHeroPower_11 = new AssetReference("VO_BOM_10_008_X_Naaru_Mida_InGame_BossUsesHeroPower_11.prefab:82ced1c546c33724bb832db2a02cfe3f");

	private static readonly AssetReference VO_BOM_10_008_X_Naaru_Mida_InGame_Death_01 = new AssetReference("VO_BOM_10_008_X_Naaru_Mida_InGame_Death_01.prefab:6ff27f802677a5544a676a606865bc02");

	private static readonly AssetReference VO_BOM_10_008_X_Naaru_Mida_InGame_EmoteResponse_01 = new AssetReference("VO_BOM_10_008_X_Naaru_Mida_InGame_EmoteResponse_01.prefab:379ef2bbb04c88549a3cbc5c798cbfb8");

	private static readonly AssetReference VO_BOM_10_008_X_Naaru_Mida_InGame_Introduction_01_A = new AssetReference("VO_BOM_10_008_X_Naaru_Mida_InGame_Introduction_01_A.prefab:c3e421a6acaa64042b7ba3dc2206a931");

	private static readonly AssetReference VO_BOM_10_008_X_Naaru_Mida_InGame_PlayerLoss_01 = new AssetReference("VO_BOM_10_008_X_Naaru_Mida_InGame_PlayerLoss_01.prefab:4c4c19369742df448965105202ca051b");

	private static readonly AssetReference VO_BOM_10_008_X_Naaru_Mida_InGame_Turn_03_01 = new AssetReference("VO_BOM_10_008_X_Naaru_Mida_InGame_Turn_03_01.prefab:c34db4c917a09074a97e194e230ba945");

	private static readonly AssetReference VO_BOM_10_008_X_Naaru_Mida_InGame_Turn_03_02 = new AssetReference("VO_BOM_10_008_X_Naaru_Mida_InGame_Turn_03_02.prefab:5ecf8430fa8779b40b14e3a1998710c1");

	private static readonly AssetReference VO_BOM_10_008_X_Naaru_Mida_InGame_Turn_11_01_A = new AssetReference("VO_BOM_10_008_X_Naaru_Mida_InGame_Turn_11_01_A.prefab:b602865cee37f9d49aca4797b1109d68");

	private static readonly AssetReference VO_BOM_10_008_X_Naaru_Mida_InGame_Turn_11_01_C = new AssetReference("VO_BOM_10_008_X_Naaru_Mida_InGame_Turn_11_01_C.prefab:4b098533114d06f4e9056cb5dd7832c0");

	private static readonly AssetReference VO_BOM_10_008_X_Naaru_Mida_InGame_VictoryPreExplosion_01_B = new AssetReference("VO_BOM_10_008_X_Naaru_Mida_InGame_VictoryPreExplosion_01_B.prefab:ffbd4d003d3d1704599f78d4b23508c0");

	private static readonly AssetReference VO_BOM_10_008_X_Naaru_Mida_InGame_VictoryPreExplosion_01_C = new AssetReference("VO_BOM_10_008_X_Naaru_Mida_InGame_VictoryPreExplosion_01_C.prefab:2b78e3a3fc031ca418ca1c65e5f4a9e0");

	private List<string> m_InGame_BossIdleLines = new List<string> { VO_BOM_10_008_X_Naaru_Mida_InGame_BossIdle_01, VO_BOM_10_008_X_Naaru_Mida_InGame_BossIdle_02, VO_BOM_10_008_X_Naaru_Mida_InGame_BossIdle_03 };

	private HashSet<string> m_playedLines = new HashSet<string>();

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> m_SoundFiles = new List<string>
		{
			VO_BOM_10_008_Female_Draenei_Runi_InGame_VictoryPostExplosion_01_A, VO_BOM_10_008_X_BloodElf_Dawngrasp_InGame_HE_Custom_IfBossAt15Health_01_A, VO_BOM_10_008_X_BloodElf_Dawngrasp_InGame_Introduction_01_B, VO_BOM_10_008_X_BloodElf_Dawngrasp_InGame_VictoryPreExplosion_01_A, VO_BOM_10_008_X_Naaru_Mida_InGame_BossIdle_01, VO_BOM_10_008_X_Naaru_Mida_InGame_BossIdle_02, VO_BOM_10_008_X_Naaru_Mida_InGame_BossIdle_03, VO_BOM_10_008_X_Naaru_Mida_InGame_BossUsesHeroPower_01, VO_BOM_10_008_X_Naaru_Mida_InGame_BossUsesHeroPower_02, VO_BOM_10_008_X_Naaru_Mida_InGame_BossUsesHeroPower_03,
			VO_BOM_10_008_X_Naaru_Mida_InGame_BossUsesHeroPower_04, VO_BOM_10_008_X_Naaru_Mida_InGame_BossUsesHeroPower_05, VO_BOM_10_008_X_Naaru_Mida_InGame_BossUsesHeroPower_06, VO_BOM_10_008_X_Naaru_Mida_InGame_BossUsesHeroPower_07, VO_BOM_10_008_X_Naaru_Mida_InGame_BossUsesHeroPower_08, VO_BOM_10_008_X_Naaru_Mida_InGame_BossUsesHeroPower_09, VO_BOM_10_008_X_Naaru_Mida_InGame_BossUsesHeroPower_10, VO_BOM_10_008_X_Naaru_Mida_InGame_BossUsesHeroPower_11, VO_BOM_10_008_X_Naaru_Mida_InGame_Death_01, VO_BOM_10_008_X_Naaru_Mida_InGame_EmoteResponse_01,
			VO_BOM_10_008_X_Naaru_Mida_InGame_Introduction_01_A, VO_BOM_10_008_X_Naaru_Mida_InGame_PlayerLoss_01, VO_BOM_10_008_X_Naaru_Mida_InGame_Turn_03_01, VO_BOM_10_008_X_Naaru_Mida_InGame_Turn_03_02, VO_BOM_10_008_X_Naaru_Mida_InGame_Turn_11_01_A, VO_BOM_10_008_X_Naaru_Mida_InGame_Turn_11_01_C, VO_BOM_10_008_X_Naaru_Mida_InGame_VictoryPreExplosion_01_B, VO_BOM_10_008_X_Naaru_Mida_InGame_VictoryPreExplosion_01_C, VO_BOM_10_008_Female_Draenei_Xyrella_InGame_Turn_07_01, VO_BOM_10_008_Female_Draenei_Xyrella_InGame_Turn_11_01_B
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
		case 517:
			yield return MissionPlayVO(enemyActor, m_InGame_BossIdleLines);
			break;
		case 516:
			yield return MissionPlayVO(enemyActor, VO_BOM_10_008_X_Naaru_Mida_InGame_Death_01);
			break;
		case 515:
			yield return MissionPlayVO(enemyActor, VO_BOM_10_008_X_Naaru_Mida_InGame_EmoteResponse_01);
			break;
		case 514:
			yield return MissionPlayVO(enemyActor, VO_BOM_10_008_X_Naaru_Mida_InGame_Introduction_01_A);
			yield return MissionPlayVO(friendlyActor, VO_BOM_10_008_X_BloodElf_Dawngrasp_InGame_Introduction_01_B);
			break;
		case 506:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(enemyActor, VO_BOM_10_008_X_Naaru_Mida_InGame_PlayerLoss_01);
			GameState.Get().SetBusy(busy: false);
			break;
		case 505:
			yield return MissionPlayVO("BOM_10_Runi_008t", VO_BOM_10_008_Female_Draenei_Runi_InGame_VictoryPostExplosion_01_A);
			break;
		case 100:
			yield return MissionPlayVO(friendlyActor, VO_BOM_10_008_X_BloodElf_Dawngrasp_InGame_HE_Custom_IfBossAt15Health_01_A);
			break;
		case 101:
			yield return MissionPlayVO(enemyActor, VO_BOM_10_008_X_Naaru_Mida_InGame_BossUsesHeroPower_01);
			break;
		case 102:
			yield return MissionPlayVO(enemyActor, VO_BOM_10_008_X_Naaru_Mida_InGame_BossUsesHeroPower_02);
			break;
		case 103:
			yield return MissionPlayVO(enemyActor, VO_BOM_10_008_X_Naaru_Mida_InGame_BossUsesHeroPower_03);
			break;
		case 104:
			yield return MissionPlayVO(enemyActor, VO_BOM_10_008_X_Naaru_Mida_InGame_BossUsesHeroPower_04);
			break;
		case 105:
			yield return MissionPlayVO(enemyActor, VO_BOM_10_008_X_Naaru_Mida_InGame_BossUsesHeroPower_05);
			break;
		case 106:
			yield return MissionPlayVO(enemyActor, VO_BOM_10_008_X_Naaru_Mida_InGame_BossUsesHeroPower_06);
			break;
		case 107:
			yield return MissionPlayVO(enemyActor, VO_BOM_10_008_X_Naaru_Mida_InGame_BossUsesHeroPower_07);
			break;
		case 108:
			yield return MissionPlayVO(enemyActor, VO_BOM_10_008_X_Naaru_Mida_InGame_BossUsesHeroPower_08);
			break;
		case 109:
			yield return MissionPlayVO(enemyActor, VO_BOM_10_008_X_Naaru_Mida_InGame_BossUsesHeroPower_09);
			break;
		case 110:
			yield return MissionPlayVO(enemyActor, VO_BOM_10_008_X_Naaru_Mida_InGame_BossUsesHeroPower_10);
			break;
		case 111:
			yield return MissionPlayVO(enemyActor, VO_BOM_10_008_X_Naaru_Mida_InGame_BossUsesHeroPower_11);
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
		GameState.Get().GetFriendlySidePlayer().GetHero()
			.GetCard()
			.GetActor();
		switch (turn)
		{
		case 3:
			yield return MissionPlayVO(enemyActor, VO_BOM_10_008_X_Naaru_Mida_InGame_Turn_03_01);
			yield return MissionPlayVO(enemyActor, VO_BOM_10_008_X_Naaru_Mida_InGame_Turn_03_02);
			break;
		case 5:
			yield return MissionPlayVO(BOM_10_Dawngrasp_Dungeon.Alterac_XyrellaArt_BrassRing_Quote, VO_BOM_10_008_Female_Draenei_Xyrella_InGame_Turn_07_01);
			break;
		case 7:
			yield return MissionPlayVO(enemyActor, VO_BOM_10_008_X_Naaru_Mida_InGame_Turn_11_01_A);
			yield return MissionPlayVO(BOM_10_Dawngrasp_Dungeon.Alterac_XyrellaArt_BrassRing_Quote, VO_BOM_10_008_Female_Draenei_Xyrella_InGame_Turn_11_01_B);
			yield return MissionPlayVO(enemyActor, VO_BOM_10_008_X_Naaru_Mida_InGame_Turn_11_01_C);
			break;
		}
	}
}
