using System.Collections;
using System.Collections.Generic;

public class BOM_10_Dawngrasp_Fight_003 : BOM_10_Dawngrasp_Dungeon
{
	private static readonly AssetReference VO_BOM_10_003_Female_Human_Jaina_InGame_HE_Custom_IfBrollDestroyedPlus2_01_C = new AssetReference("VO_BOM_10_003_Female_Human_Jaina_InGame_HE_Custom_IfBrollDestroyedPlus2_01_C.prefab:1db0e4432ed36384f89431549dc32933");

	private static readonly AssetReference VO_BOM_10_003_X_BloodElf_Dawngrasp_InGame_HE_Custom_IfBrollDestroyedPlus2_01_B = new AssetReference("VO_BOM_10_003_X_BloodElf_Dawngrasp_InGame_HE_Custom_IfBrollDestroyedPlus2_01_B.prefab:d65e54418c9e8b64192cdf34facdcd6f");

	private static readonly AssetReference VO_BOM_10_003_Female_Human_Jaina_InGame_HE_Custom_IfBrollDestroyedPlus1_01_A = new AssetReference("VO_BOM_10_003_Female_Human_Jaina_InGame_HE_Custom_IfBrollDestroyedPlus1_01_A.prefab:d48dbf3e6d08c5c41a570732da4da358");

	private static readonly AssetReference VO_BOM_10_003_X_BloodElf_Dawngrasp_InGame_HE_Custom_IfBrollDestroyedPlus1_01_B = new AssetReference("VO_BOM_10_003_X_BloodElf_Dawngrasp_InGame_HE_Custom_IfBrollDestroyedPlus1_01_B.prefab:f5929916a93fc9541bb0dcae28f27414");

	private static readonly AssetReference VO_BOM_10_003_Female_BloodElf_Valeera_InGame_HE_Custom_IfValeeraDestroyed_01_A = new AssetReference("VO_BOM_10_003_Female_BloodElf_Valeera_InGame_HE_Custom_IfValeeraDestroyed_01_A.prefab:ede9fdabdb759ea4fb6b62276e01079f");

	private static readonly AssetReference VO_BOM_10_003_Female_BloodElf_Valeera_InGame_Turn_03_01_A = new AssetReference("VO_BOM_10_003_Female_BloodElf_Valeera_InGame_Turn_03_01_A.prefab:c6872b28c66d23d43a8760e7208fb5cd");

	private static readonly AssetReference VO_BOM_10_003_Female_Human_Jaina_InGame_HE_Custom_IfJainaDestroyed_01_A = new AssetReference("VO_BOM_10_003_Female_Human_Jaina_InGame_HE_Custom_IfJainaDestroyed_01_A.prefab:16d25362b20f08840bbc42ad7ca5f5e3");

	private static readonly AssetReference VO_BOM_10_003_Male_Human_Varian_InGame_BossIdle_01 = new AssetReference("VO_BOM_10_003_Male_Human_Varian_InGame_BossIdle_01.prefab:b84dc19b84d570544a946b1c67beecb1");

	private static readonly AssetReference VO_BOM_10_003_Male_Human_Varian_InGame_BossIdle_02 = new AssetReference("VO_BOM_10_003_Male_Human_Varian_InGame_BossIdle_02.prefab:ce014a7112675594aa02d1c1f4ad7e7d");

	private static readonly AssetReference VO_BOM_10_003_Male_Human_Varian_InGame_BossIdle_03 = new AssetReference("VO_BOM_10_003_Male_Human_Varian_InGame_BossIdle_03.prefab:2c92f59dd5d401345b03e60805253262");

	private static readonly AssetReference VO_BOM_10_003_Male_Human_Varian_InGame_BossUsesHeroPower_01 = new AssetReference("VO_BOM_10_003_Male_Human_Varian_InGame_BossUsesHeroPower_01.prefab:f42e85b5be6cc6248b99da3fbb6e842d");

	private static readonly AssetReference VO_BOM_10_003_Male_Human_Varian_InGame_BossUsesHeroPower_02 = new AssetReference("VO_BOM_10_003_Male_Human_Varian_InGame_BossUsesHeroPower_02.prefab:605048ea5f75aab478e29ac4f0a05f5f");

	private static readonly AssetReference VO_BOM_10_003_Male_Human_Varian_InGame_BossUsesHeroPower_03 = new AssetReference("VO_BOM_10_003_Male_Human_Varian_InGame_BossUsesHeroPower_03.prefab:8c19fecb14982c24da1796115233f4e8");

	private static readonly AssetReference VO_BOM_10_003_Male_Human_Varian_InGame_EmoteResponse_01 = new AssetReference("VO_BOM_10_003_Male_Human_Varian_InGame_EmoteResponse_01.prefab:739ed4724ee50e649be79ebddb2181c3");

	private static readonly AssetReference VO_BOM_10_003_Male_Human_Varian_InGame_HE_Custom_IfBrollDestroyed_01_B = new AssetReference("VO_BOM_10_003_Male_Human_Varian_InGame_HE_Custom_IfBrollDestroyed_01_B.prefab:77ef481b253f3b2488b3de4f757aefc0");

	private static readonly AssetReference VO_BOM_10_003_Male_Human_Varian_InGame_HE_Custom_IfJainaDestroyed_01_B = new AssetReference("VO_BOM_10_003_Male_Human_Varian_InGame_HE_Custom_IfJainaDestroyed_01_B.prefab:f6933fb47abd8224598601047d0cdeb3");

	private static readonly AssetReference VO_BOM_10_003_Male_Human_Varian_InGame_HE_Custom_IfJainaDestroyed_01_D = new AssetReference("VO_BOM_10_003_Male_Human_Varian_InGame_HE_Custom_IfJainaDestroyed_01_D.prefab:e2fd56cff0f8af44b83a1514c81b5a6f");

	private static readonly AssetReference VO_BOM_10_003_Male_Human_Varian_InGame_HE_Custom_IfValeeraDestroyed_01_B = new AssetReference("VO_BOM_10_003_Male_Human_Varian_InGame_HE_Custom_IfValeeraDestroyed_01_B.prefab:659a9f4be27787c47b652562b9e39481");

	private static readonly AssetReference VO_BOM_10_003_Male_Human_Varian_InGame_Introduction_01_B = new AssetReference("VO_BOM_10_003_Male_Human_Varian_InGame_Introduction_01_B.prefab:8638e04e935b4c74fba02ee3afa31661");

	private static readonly AssetReference VO_BOM_10_003_Male_Human_Varian_InGame_PlayerLoss_01 = new AssetReference("VO_BOM_10_003_Male_Human_Varian_InGame_PlayerLoss_01.prefab:cc5031edcbcec164794390c2023628c4");

	private static readonly AssetReference VO_BOM_10_003_Male_Human_Varian_InGame_Turn_01_01_B = new AssetReference("VO_BOM_10_003_Male_Human_Varian_InGame_Turn_01_01_B.prefab:decc3270fb87bfb4584597d484051f9f");

	private static readonly AssetReference VO_BOM_10_003_Male_Human_Varian_InGame_Victory_PreExplosion_01_A = new AssetReference("VO_BOM_10_003_Male_Human_Varian_InGame_Victory_PreExplosion_01_A.prefab:b6fa49d985924034da92499660778409");

	private static readonly AssetReference VO_BOM_10_003_Male_Human_Varian_InGame_Victory_PreExplosion_01_B = new AssetReference("VO_BOM_10_003_Male_Human_Varian_InGame_Victory_PreExplosion_01_B.prefab:78e0af3f25ea069468279af0e2e79359");

	private static readonly AssetReference VO_BOM_10_003_Male_NightElf_Broll_InGame_HE_Custom_IfBrollDestroyed_01_A = new AssetReference("VO_BOM_10_003_Male_NightElf_Broll_InGame_HE_Custom_IfBrollDestroyed_01_A.prefab:958ffb039f24bb9468f01df374f147b8");

	private static readonly AssetReference VO_BOM_10_003_Male_NightElf_Broll_InGame_HE_Custom_IfValeeraDestroyedPlus1_01_A = new AssetReference("VO_BOM_10_003_Male_NightElf_Broll_InGame_HE_Custom_IfValeeraDestroyedPlus1_01_A.prefab:68c334c0040aa3c4f86195029cb3c083");

	private static readonly AssetReference VO_BOM_10_003_X_BloodElf_Dawngrasp_InGame_HE_Custom_IfJainaDestroyed_01_C = new AssetReference("VO_BOM_10_003_X_BloodElf_Dawngrasp_InGame_HE_Custom_IfJainaDestroyed_01_C.prefab:5c0a06b05a61dc449ad66566a65633af");

	private static readonly AssetReference VO_BOM_10_003_X_BloodElf_Dawngrasp_InGame_Introduction_01_A = new AssetReference("VO_BOM_10_003_X_BloodElf_Dawngrasp_InGame_Introduction_01_A.prefab:62f4f847cbd819a4cbc429739631a5e8");

	private static readonly AssetReference VO_BOM_10_003_X_BloodElf_Dawngrasp_InGame_Turn_01_01_A = new AssetReference("VO_BOM_10_003_X_BloodElf_Dawngrasp_InGame_Turn_01_01_A.prefab:0eb14ffc9f4ab2245a0c87a8f6c53d5b");

	private static readonly AssetReference VO_BOM_10_003_X_BloodElf_Dawngrasp_UI_AWD_Boss_Reveal_General_03_01_A = new AssetReference("VO_BOM_10_003_X_BloodElf_Dawngrasp_UI_AWD_Boss_Reveal_General_03_01_A.prefab:b4adf7bcfcd5b3340b0df0f6d4588b86");

	private static readonly AssetReference VO_BOM_10_003_X_BloodElf_Dawngrasp_UI_AWD_Boss_Reveal_General_03_01_B = new AssetReference("VO_BOM_10_003_X_BloodElf_Dawngrasp_UI_AWD_Boss_Reveal_General_03_01_B.prefab:7b48b63d31c692841a224d6fba8b12fe");

	private List<string> m_InGame_BossIdleLines = new List<string> { VO_BOM_10_003_Male_Human_Varian_InGame_BossIdle_01, VO_BOM_10_003_Male_Human_Varian_InGame_BossIdle_02, VO_BOM_10_003_Male_Human_Varian_InGame_BossIdle_03 };

	private List<string> m_InGame_BossUsesHeroPowerLines = new List<string> { VO_BOM_10_003_Male_Human_Varian_InGame_BossUsesHeroPower_01, VO_BOM_10_003_Male_Human_Varian_InGame_BossUsesHeroPower_02, VO_BOM_10_003_Male_Human_Varian_InGame_BossUsesHeroPower_03 };

	private HashSet<string> m_playedLines = new HashSet<string>();

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> m_SoundFiles = new List<string>
		{
			VO_BOM_10_003_Female_BloodElf_Valeera_InGame_HE_Custom_IfValeeraDestroyed_01_A, VO_BOM_10_003_Female_BloodElf_Valeera_InGame_Turn_03_01_A, VO_BOM_10_003_Female_Human_Jaina_InGame_HE_Custom_IfBrollDestroyedPlus1_01_A, VO_BOM_10_003_Female_Human_Jaina_InGame_HE_Custom_IfBrollDestroyedPlus2_01_C, VO_BOM_10_003_Female_Human_Jaina_InGame_HE_Custom_IfJainaDestroyed_01_A, VO_BOM_10_003_Male_Human_Varian_InGame_BossIdle_01, VO_BOM_10_003_Male_Human_Varian_InGame_BossIdle_02, VO_BOM_10_003_Male_Human_Varian_InGame_BossIdle_03, VO_BOM_10_003_Male_Human_Varian_InGame_BossUsesHeroPower_01, VO_BOM_10_003_Male_Human_Varian_InGame_BossUsesHeroPower_02,
			VO_BOM_10_003_Male_Human_Varian_InGame_BossUsesHeroPower_03, VO_BOM_10_003_Male_Human_Varian_InGame_EmoteResponse_01, VO_BOM_10_003_Male_Human_Varian_InGame_HE_Custom_IfBrollDestroyed_01_B, VO_BOM_10_003_Male_Human_Varian_InGame_HE_Custom_IfJainaDestroyed_01_B, VO_BOM_10_003_Male_Human_Varian_InGame_HE_Custom_IfJainaDestroyed_01_D, VO_BOM_10_003_Male_Human_Varian_InGame_HE_Custom_IfValeeraDestroyed_01_B, VO_BOM_10_003_Male_Human_Varian_InGame_Introduction_01_B, VO_BOM_10_003_Male_Human_Varian_InGame_PlayerLoss_01, VO_BOM_10_003_Male_Human_Varian_InGame_Turn_01_01_B, VO_BOM_10_003_Male_Human_Varian_InGame_Victory_PreExplosion_01_A,
			VO_BOM_10_003_Male_Human_Varian_InGame_Victory_PreExplosion_01_B, VO_BOM_10_003_Male_NightElf_Broll_InGame_HE_Custom_IfBrollDestroyed_01_A, VO_BOM_10_003_Male_NightElf_Broll_InGame_HE_Custom_IfValeeraDestroyedPlus1_01_A, VO_BOM_10_003_X_BloodElf_Dawngrasp_InGame_HE_Custom_IfBrollDestroyedPlus1_01_B, VO_BOM_10_003_X_BloodElf_Dawngrasp_InGame_HE_Custom_IfBrollDestroyedPlus2_01_B, VO_BOM_10_003_X_BloodElf_Dawngrasp_InGame_HE_Custom_IfJainaDestroyed_01_C, VO_BOM_10_003_X_BloodElf_Dawngrasp_InGame_Introduction_01_A, VO_BOM_10_003_X_BloodElf_Dawngrasp_InGame_Turn_01_01_A, VO_BOM_10_003_X_BloodElf_Dawngrasp_UI_AWD_Boss_Reveal_General_03_01_A, VO_BOM_10_003_X_BloodElf_Dawngrasp_UI_AWD_Boss_Reveal_General_03_01_B
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
		case 510:
			yield return MissionPlayVO(enemyActor, m_InGame_BossUsesHeroPowerLines);
			break;
		case 515:
			yield return MissionPlayVO(enemyActor, VO_BOM_10_003_Male_Human_Varian_InGame_EmoteResponse_01);
			break;
		case 514:
			yield return MissionPlayVO(friendlyActor, VO_BOM_10_003_X_BloodElf_Dawngrasp_InGame_Introduction_01_A);
			yield return MissionPlayVO(enemyActor, VO_BOM_10_003_Male_Human_Varian_InGame_Introduction_01_B);
			break;
		case 506:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(enemyActor, VO_BOM_10_003_Male_Human_Varian_InGame_PlayerLoss_01);
			GameState.Get().SetBusy(busy: false);
			break;
		case 504:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(enemyActor, VO_BOM_10_003_Male_Human_Varian_InGame_Victory_PreExplosion_01_A);
			yield return MissionPlayVO(enemyActor, VO_BOM_10_003_Male_Human_Varian_InGame_Victory_PreExplosion_01_B);
			GameState.Get().SetBusy(busy: false);
			break;
		case 100:
			yield return MissionPlayVO("BOM_10_Jaina_003t", BOM_10_Dawngrasp_Dungeon.Jaina_BrassRing_Quote, VO_BOM_10_003_Female_Human_Jaina_InGame_HE_Custom_IfBrollDestroyedPlus1_01_A);
			yield return MissionPlayVO(friendlyActor, VO_BOM_10_003_X_BloodElf_Dawngrasp_InGame_HE_Custom_IfBrollDestroyedPlus1_01_B);
			break;
		case 101:
			yield return MissionPlayVO(friendlyActor, VO_BOM_10_003_X_BloodElf_Dawngrasp_InGame_HE_Custom_IfBrollDestroyedPlus2_01_B);
			yield return MissionPlayVO("BOM_10_Jaina_003t", BOM_10_Dawngrasp_Dungeon.Jaina_BrassRing_Quote, VO_BOM_10_003_Female_Human_Jaina_InGame_HE_Custom_IfBrollDestroyedPlus2_01_C);
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
			yield return MissionPlayVO(enemyActor, VO_BOM_10_003_Male_Human_Varian_InGame_Turn_01_01_B);
			yield return MissionPlayVO(friendlyActor, VO_BOM_10_003_X_BloodElf_Dawngrasp_InGame_Turn_01_01_A);
			break;
		case 3:
			yield return MissionPlayVO("BOM_10_Valeera_003t", VO_BOM_10_003_Female_BloodElf_Valeera_InGame_Turn_03_01_A);
			break;
		}
	}
}
