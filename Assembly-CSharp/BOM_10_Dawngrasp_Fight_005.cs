using System.Collections;
using System.Collections.Generic;

public class BOM_10_Dawngrasp_Fight_005 : BOM_10_Dawngrasp_Dungeon
{
	private static readonly AssetReference VO_BOM_10_005_Female_Dragon_Onyxia_InGame_BossIdle_01 = new AssetReference("VO_BOM_10_005_Female_Dragon_Onyxia_InGame_BossIdle_01.prefab:10244f7a739d5144e9afdebf32e94ce7");

	private static readonly AssetReference VO_BOM_10_005_Female_Dragon_Onyxia_InGame_BossIdle_02 = new AssetReference("VO_BOM_10_005_Female_Dragon_Onyxia_InGame_BossIdle_02.prefab:1b3cafd938ad55641bc2e51de92c19c8");

	private static readonly AssetReference VO_BOM_10_005_Female_Dragon_Onyxia_InGame_BossIdle_03 = new AssetReference("VO_BOM_10_005_Female_Dragon_Onyxia_InGame_BossIdle_03.prefab:37b78b038d1b9a04d9387a79c74ef606");

	private static readonly AssetReference VO_BOM_10_005_Female_Dragon_Onyxia_InGame_BossUsesHeroPower_01 = new AssetReference("VO_BOM_10_005_Female_Dragon_Onyxia_InGame_BossUsesHeroPower_01.prefab:2f0f7a4268ed0fa4aa9238aa75510d80");

	private static readonly AssetReference VO_BOM_10_005_Female_Dragon_Onyxia_InGame_BossUsesHeroPower_02 = new AssetReference("VO_BOM_10_005_Female_Dragon_Onyxia_InGame_BossUsesHeroPower_02.prefab:d2d97d8f19cb3814482aadea0c5b3997");

	private static readonly AssetReference VO_BOM_10_005_Female_Dragon_Onyxia_InGame_BossUsesHeroPower_03 = new AssetReference("VO_BOM_10_005_Female_Dragon_Onyxia_InGame_BossUsesHeroPower_03.prefab:dae62a38f30c36c48b00e9ee419ea1e9");

	private static readonly AssetReference VO_BOM_10_005_Female_Dragon_Onyxia_InGame_EmoteResponse_01 = new AssetReference("VO_BOM_10_005_Female_Dragon_Onyxia_InGame_EmoteResponse_01.prefab:932e18db73a03134d87cc4526e38273a");

	private static readonly AssetReference VO_BOM_10_005_Female_Dragon_Onyxia_InGame_Introduction_01_A = new AssetReference("VO_BOM_10_005_Female_Dragon_Onyxia_InGame_Introduction_01_A.prefab:78054ed216f5f484eb02c930b0cb1a16");

	private static readonly AssetReference VO_BOM_10_005_Female_Dragon_Onyxia_InGame_PlayerLoss_01 = new AssetReference("VO_BOM_10_005_Female_Dragon_Onyxia_InGame_PlayerLoss_01.prefab:2aaab1d8c81055643ad39f7b2834c86a");

	private static readonly AssetReference VO_BOM_10_005_Female_Dragon_Onyxia_InGame_VictoryPreExplosion_01_A = new AssetReference("VO_BOM_10_005_Female_Dragon_Onyxia_InGame_VictoryPreExplosion_01_A.prefab:f42f8a28ea1aea949be814cb7af33f15");

	private static readonly AssetReference VO_BOM_10_005_Male_Gnome_Scabbs_InGame_HE_Custom_IfTurnStartsWithScabbsInPlay_01_B = new AssetReference("VO_BOM_10_005_Male_Gnome_Scabbs_InGame_HE_Custom_IfTurnStartsWithScabbsInPlay_01_B.prefab:5962fa4fca929264482f599b14ee8e4d");

	private static readonly AssetReference VO_BOM_10_005_Male_Tauren_Guff_InGame_HE_Custom_IfGuffDestroysWhelp_01 = new AssetReference("VO_BOM_10_005_Male_Tauren_Guff_InGame_HE_Custom_IfGuffDestroysWhelp_01.prefab:1c425ae3cf04adf4086147f26664cdf6");

	private static readonly AssetReference VO_BOM_10_005_Male_Tauren_Guff_InGame_HE_Custom_IfGuffDestroysWhelp_02 = new AssetReference("VO_BOM_10_005_Male_Tauren_Guff_InGame_HE_Custom_IfGuffDestroysWhelp_02.prefab:139449173c26400408c87eea8ce6f58e");

	private static readonly AssetReference VO_BOM_10_005_Male_Tauren_Guff_InGame_HE_Custom_IfGuffDestroysWhelp_03 = new AssetReference("VO_BOM_10_005_Male_Tauren_Guff_InGame_HE_Custom_IfGuffDestroysWhelp_03.prefab:aec8d2a07e6e71f49bde72e175547472");

	private static readonly AssetReference VO_BOM_10_005_Male_Tauren_Guff_InGame_HE_Custom_IfGuffDestroysWhelp_04 = new AssetReference("VO_BOM_10_005_Male_Tauren_Guff_InGame_HE_Custom_IfGuffDestroysWhelp_04.prefab:669539fe55c139743ad4710e75cadc44");

	private static readonly AssetReference VO_BOM_10_005_Male_Tauren_Guff_InGame_HE_Custom_IfTurnStartsWithXyrellaAndGuffInPlay_01_B = new AssetReference("VO_BOM_10_005_Male_Tauren_Guff_InGame_HE_Custom_IfTurnStartsWithXyrellaAndGuffInPlay_01_B.prefab:3f94166da2e5c624c854636c641a0576");

	private static readonly AssetReference VO_BOM_10_005_Male_Tauren_Guff_InGame_HE_Custom_IfTurnStartsWithXyrellaAndGuffInPlay_01_C = new AssetReference("VO_BOM_10_005_Male_Tauren_Guff_InGame_HE_Custom_IfTurnStartsWithXyrellaAndGuffInPlay_01_C.prefab:a9ff33ea0002d84459fc7703b7b333fb");

	private static readonly AssetReference VO_BOM_10_005_X_BloodElf_Dawngrasp_InGame_HE_Custom_IfTurnStartsWithScabbsInPlay_01_A = new AssetReference("VO_BOM_10_005_X_BloodElf_Dawngrasp_InGame_HE_Custom_IfTurnStartsWithScabbsInPlay_01_A.prefab:9d44dd7536e61834683a2a3c61b3213c");

	private static readonly AssetReference VO_BOM_10_005_X_BloodElf_Dawngrasp_InGame_Introduction_01_B = new AssetReference("VO_BOM_10_005_X_BloodElf_Dawngrasp_InGame_Introduction_01_B.prefab:4ebc12236897d6b409f87cf3a5ab068c");

	private static readonly AssetReference VO_BOM_10_005_X_BloodElf_Dawngrasp_InGame_VictoryPreExplosion_01_B = new AssetReference("VO_BOM_10_005_X_BloodElf_Dawngrasp_InGame_VictoryPreExplosion_01_B.prefab:6ff1e659fcd2927408dcd87d0a674467");

	private static readonly AssetReference VO_BOM_10_005_X_BloodElf_Dawngrasp_UI_AWD_Boss_Reveal_General_05_01_A = new AssetReference("VO_BOM_10_005_X_BloodElf_Dawngrasp_UI_AWD_Boss_Reveal_General_05_01_A.prefab:fad89b90abd90664baf5d0736cd5e23c");

	private static readonly AssetReference VO_BOM_10_006_Female_Dragon_Onyxia_InGame_HE_Custom_IfOnyxiaPlaysBellowingRoar_01 = new AssetReference("VO_BOM_10_006_Female_Dragon_Onyxia_InGame_HE_Custom_IfOnyxiaPlaysBellowingRoar_01.prefab:146c14cf06222254ea327ea21be368dc");

	private static readonly AssetReference VO_BOM_10_006_Female_Dragon_Onyxia_InGame_HE_Custom_IfOnyxiaPlaysBreathOfFire_1_01 = new AssetReference("VO_BOM_10_006_Female_Dragon_Onyxia_InGame_HE_Custom_IfOnyxiaPlaysBreathOfFire_1_01.prefab:7eba5a5e10327e046b67a5948deb5588");

	private static readonly AssetReference VO_BOM_10_006_Female_Dragon_Onyxia_InGame_HE_Custom_IfOnyxiaPlaysBreathOfFire_2_01 = new AssetReference("VO_BOM_10_006_Female_Dragon_Onyxia_InGame_HE_Custom_IfOnyxiaPlaysBreathOfFire_2_01.prefab:b5db05c03e925144da175528ce6e6091");

	private static readonly AssetReference VO_BOM_10_006_Female_Dragon_Onyxia_InGame_HE_Custom_IfOnyxiaPlaysBreathOfFire_3_01 = new AssetReference("VO_BOM_10_006_Female_Dragon_Onyxia_InGame_HE_Custom_IfOnyxiaPlaysBreathOfFire_3_01.prefab:8e59e069c1fa0384cb124a5a0a0fa775");

	private static readonly AssetReference VO_BOM_10_006_Female_Dragon_Onyxia_InGame_HE_Custom_IfOnyxiaPlaysDeepBreath_01 = new AssetReference("VO_BOM_10_006_Female_Dragon_Onyxia_InGame_HE_Custom_IfOnyxiaPlaysDeepBreath_01.prefab:c12251d7a4ee5384496337950b063ec4");

	private static readonly AssetReference VO_BOM_10_006_Female_Dragon_Onyxia_InGame_HE_Custom_IfOnyxiaPlaysDontStandInTheFire_01 = new AssetReference("VO_BOM_10_006_Female_Dragon_Onyxia_InGame_HE_Custom_IfOnyxiaPlaysDontStandInTheFire_01.prefab:828fa93394fa35c4e9d84bb8afbbae41");

	private static readonly AssetReference VO_BOM_10_006_Female_Dragon_Onyxia_InGame_HE_Custom_IfOnyxiaPlaysScaleOfOnyxia_1_01 = new AssetReference("VO_BOM_10_006_Female_Dragon_Onyxia_InGame_HE_Custom_IfOnyxiaPlaysScaleOfOnyxia_1_01.prefab:42626a743f400ac4ab94d8743abf9ffe");

	private static readonly AssetReference VO_BOM_10_006_Female_Dragon_Onyxia_InGame_HE_Custom_IfOnyxiaPlaysScaleOfOnyxia_2_01 = new AssetReference("VO_BOM_10_006_Female_Dragon_Onyxia_InGame_HE_Custom_IfOnyxiaPlaysScaleOfOnyxia_2_01.prefab:725b4a3dfe8198a45b7c65fde7741489");

	private static readonly AssetReference VO_BOM_10_006_Female_Dragon_Onyxia_InGame_HE_Custom_IfOnyxiaPlaysTailSweep_01 = new AssetReference("VO_BOM_10_006_Female_Dragon_Onyxia_InGame_HE_Custom_IfOnyxiaPlaysTailSweep_01.prefab:492d1f256bf2f524aa415f276f628647");

	private static readonly AssetReference VO_BOM_10_006_Female_Dragon_Onyxia_InGame_HE_Custom_IfOnyxiaPlaysUltimateImpfestation_01 = new AssetReference("VO_BOM_10_006_Female_Dragon_Onyxia_InGame_HE_Custom_IfOnyxiaPlaysUltimateImpfestation_01.prefab:fdd5b28d7946ec8408f6c85b7a9b5cbd");

	private static readonly AssetReference VO_BOM_10_006_Female_Dragon_Onyxia_InGame_HE_Custom_IfOnyxiaPlaysWingBuffet_01 = new AssetReference("VO_BOM_10_006_Female_Dragon_Onyxia_InGame_HE_Custom_IfOnyxiaPlaysWingBuffet_01.prefab:d8e765b6a4232a940bc9666df645c05d");

	private List<string> m_InGame_BossCastsBreathOfFire = new List<string> { VO_BOM_10_006_Female_Dragon_Onyxia_InGame_HE_Custom_IfOnyxiaPlaysBreathOfFire_1_01, VO_BOM_10_006_Female_Dragon_Onyxia_InGame_HE_Custom_IfOnyxiaPlaysBreathOfFire_2_01, VO_BOM_10_006_Female_Dragon_Onyxia_InGame_HE_Custom_IfOnyxiaPlaysBreathOfFire_3_01 };

	private List<string> m_InGame_BossCastsScaleOfOnyxiae = new List<string> { VO_BOM_10_006_Female_Dragon_Onyxia_InGame_HE_Custom_IfOnyxiaPlaysScaleOfOnyxia_1_01, VO_BOM_10_006_Female_Dragon_Onyxia_InGame_HE_Custom_IfOnyxiaPlaysScaleOfOnyxia_2_01 };

	private List<string> m_InGame_BossIdleLines = new List<string> { VO_BOM_10_005_Female_Dragon_Onyxia_InGame_BossIdle_01, VO_BOM_10_005_Female_Dragon_Onyxia_InGame_BossIdle_02, VO_BOM_10_005_Female_Dragon_Onyxia_InGame_BossIdle_03 };

	private List<string> m_InGame_BossUsesHeroPowerLines = new List<string> { VO_BOM_10_005_Female_Dragon_Onyxia_InGame_BossUsesHeroPower_01, VO_BOM_10_005_Female_Dragon_Onyxia_InGame_BossUsesHeroPower_02, VO_BOM_10_005_Female_Dragon_Onyxia_InGame_BossUsesHeroPower_03 };

	private HashSet<string> m_playedLines = new HashSet<string>();

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> m_SoundFiles = new List<string>
		{
			VO_BOM_10_005_Female_Dragon_Onyxia_InGame_BossIdle_01, VO_BOM_10_005_Female_Dragon_Onyxia_InGame_BossIdle_02, VO_BOM_10_005_Female_Dragon_Onyxia_InGame_BossIdle_03, VO_BOM_10_005_Female_Dragon_Onyxia_InGame_BossUsesHeroPower_01, VO_BOM_10_005_Female_Dragon_Onyxia_InGame_BossUsesHeroPower_02, VO_BOM_10_005_Female_Dragon_Onyxia_InGame_BossUsesHeroPower_03, VO_BOM_10_005_Female_Dragon_Onyxia_InGame_EmoteResponse_01, VO_BOM_10_005_Female_Dragon_Onyxia_InGame_Introduction_01_A, VO_BOM_10_005_Female_Dragon_Onyxia_InGame_PlayerLoss_01, VO_BOM_10_005_Female_Dragon_Onyxia_InGame_VictoryPreExplosion_01_A,
			VO_BOM_10_005_Male_Gnome_Scabbs_InGame_HE_Custom_IfTurnStartsWithScabbsInPlay_01_B, VO_BOM_10_005_Male_Tauren_Guff_InGame_HE_Custom_IfGuffDestroysWhelp_01, VO_BOM_10_005_Male_Tauren_Guff_InGame_HE_Custom_IfTurnStartsWithXyrellaAndGuffInPlay_01_B, VO_BOM_10_005_Male_Tauren_Guff_InGame_HE_Custom_IfTurnStartsWithXyrellaAndGuffInPlay_01_C, VO_BOM_10_005_X_BloodElf_Dawngrasp_InGame_HE_Custom_IfTurnStartsWithScabbsInPlay_01_A, VO_BOM_10_005_X_BloodElf_Dawngrasp_InGame_Introduction_01_B, VO_BOM_10_005_X_BloodElf_Dawngrasp_InGame_VictoryPreExplosion_01_B, VO_BOM_10_005_X_BloodElf_Dawngrasp_UI_AWD_Boss_Reveal_General_05_01_A, VO_BOM_10_006_Female_Dragon_Onyxia_InGame_HE_Custom_IfOnyxiaPlaysBellowingRoar_01, VO_BOM_10_006_Female_Dragon_Onyxia_InGame_HE_Custom_IfOnyxiaPlaysBreathOfFire_1_01,
			VO_BOM_10_006_Female_Dragon_Onyxia_InGame_HE_Custom_IfOnyxiaPlaysBreathOfFire_2_01, VO_BOM_10_006_Female_Dragon_Onyxia_InGame_HE_Custom_IfOnyxiaPlaysBreathOfFire_3_01, VO_BOM_10_006_Female_Dragon_Onyxia_InGame_HE_Custom_IfOnyxiaPlaysDeepBreath_01, VO_BOM_10_006_Female_Dragon_Onyxia_InGame_HE_Custom_IfOnyxiaPlaysDontStandInTheFire_01, VO_BOM_10_006_Female_Dragon_Onyxia_InGame_HE_Custom_IfOnyxiaPlaysScaleOfOnyxia_1_01, VO_BOM_10_006_Female_Dragon_Onyxia_InGame_HE_Custom_IfOnyxiaPlaysScaleOfOnyxia_2_01, VO_BOM_10_006_Female_Dragon_Onyxia_InGame_HE_Custom_IfOnyxiaPlaysTailSweep_01, VO_BOM_10_006_Female_Dragon_Onyxia_InGame_HE_Custom_IfOnyxiaPlaysUltimateImpfestation_01, VO_BOM_10_006_Female_Dragon_Onyxia_InGame_HE_Custom_IfOnyxiaPlaysWingBuffet_01
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
			yield return MissionPlayVO(enemyActor, VO_BOM_10_005_Female_Dragon_Onyxia_InGame_EmoteResponse_01);
			break;
		case 514:
			yield return MissionPlayVO(enemyActor, VO_BOM_10_005_Female_Dragon_Onyxia_InGame_Introduction_01_A);
			yield return MissionPlayVO(friendlyActor, VO_BOM_10_005_X_BloodElf_Dawngrasp_InGame_Introduction_01_B);
			break;
		case 506:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(enemyActor, VO_BOM_10_005_Female_Dragon_Onyxia_InGame_PlayerLoss_01);
			GameState.Get().SetBusy(busy: false);
			break;
		case 504:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(enemyActor, VO_BOM_10_005_Female_Dragon_Onyxia_InGame_VictoryPreExplosion_01_A);
			yield return MissionPlayVO(friendlyActor, VO_BOM_10_005_X_BloodElf_Dawngrasp_InGame_VictoryPreExplosion_01_B);
			GameState.Get().SetBusy(busy: false);
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
			Actor enemyActor = GameState.Get().GetOpposingSidePlayer().GetHero()
				.GetCard()
				.GetActor();
			GameState.Get().GetFriendlySidePlayer().GetHero()
				.GetCard()
				.GetActor();
			switch (cardID)
			{
			case "BOM_10_BellowingRoar_006s":
				yield return MissionPlayVO(enemyActor, VO_BOM_10_006_Female_Dragon_Onyxia_InGame_HE_Custom_IfOnyxiaPlaysBellowingRoar_01);
				break;
			case "BOM_10_BreathOfFire_006s":
				yield return MissionPlayVO(enemyActor, m_InGame_BossCastsBreathOfFire);
				break;
			case "ONY_006":
				yield return MissionPlayVO(enemyActor, VO_BOM_10_006_Female_Dragon_Onyxia_InGame_HE_Custom_IfOnyxiaPlaysDeepBreath_01);
				break;
			case "ONY_011":
				yield return MissionPlayVO(enemyActor, VO_BOM_10_006_Female_Dragon_Onyxia_InGame_HE_Custom_IfOnyxiaPlaysDontStandInTheFire_01);
				break;
			case "ONY_021":
				yield return MissionPlayVO(enemyActor, m_InGame_BossCastsScaleOfOnyxiae);
				break;
			case "BOM_10_TailSweep_006s":
				yield return MissionPlayVO(enemyActor, VO_BOM_10_006_Female_Dragon_Onyxia_InGame_HE_Custom_IfOnyxiaPlaysTailSweep_01);
				break;
			case "ONY_033":
				yield return MissionPlayVO(enemyActor, VO_BOM_10_006_Female_Dragon_Onyxia_InGame_HE_Custom_IfOnyxiaPlaysUltimateImpfestation_01);
				break;
			case "BOM_10_WingBuffet_006s":
				yield return MissionPlayVO(enemyActor, VO_BOM_10_006_Female_Dragon_Onyxia_InGame_HE_Custom_IfOnyxiaPlaysWingBuffet_01);
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
		GameState.Get().GetFriendlySidePlayer().GetHero()
			.GetCard()
			.GetActor();
	}

	public override void NotifyOfMinionDied(Entity entity)
	{
		Gameplay.Get().StartCoroutine(NotifyOfMinionDiedWithTiming(entity));
	}

	public IEnumerator NotifyOfMinionDiedWithTiming(Entity entity)
	{
		while (m_enemySpeaking)
		{
			yield return null;
		}
		entity.GetCardId();
	}
}
