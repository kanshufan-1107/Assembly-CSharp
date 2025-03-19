using System.Collections;
using System.Collections.Generic;

public class BOM_10_Dawngrasp_Fight_006 : BOM_10_Dawngrasp_Dungeon
{
	private static readonly AssetReference VO_BOM_10_006_Female_Draenei_Xyrella_InGame_VictoryPostExplosion_01_A = new AssetReference("VO_BOM_10_006_Female_Draenei_Xyrella_InGame_VictoryPostExplosion_01_A.prefab:555715599fabde545ab701bd081ba095");

	private static readonly AssetReference VO_BOM_10_006_Female_Dragon_Onyxia_InGame_BossIdle_01 = new AssetReference("VO_BOM_10_006_Female_Dragon_Onyxia_InGame_BossIdle_01.prefab:cdbf930009efa474bb33c7934fec9192");

	private static readonly AssetReference VO_BOM_10_006_Female_Dragon_Onyxia_InGame_BossIdle_02 = new AssetReference("VO_BOM_10_006_Female_Dragon_Onyxia_InGame_BossIdle_02.prefab:62b8dea61de482f469417d6382e21680");

	private static readonly AssetReference VO_BOM_10_006_Female_Dragon_Onyxia_InGame_BossIdle_03 = new AssetReference("VO_BOM_10_006_Female_Dragon_Onyxia_InGame_BossIdle_03.prefab:297c638b7290c7445bfe0a1b8c67caf7");

	private static readonly AssetReference VO_BOM_10_006_Female_Dragon_Onyxia_InGame_BossUsesHeroPower_01 = new AssetReference("VO_BOM_10_006_Female_Dragon_Onyxia_InGame_BossUsesHeroPower_01.prefab:be7636f1a1962a746b049d859687e4a1");

	private static readonly AssetReference VO_BOM_10_006_Female_Dragon_Onyxia_InGame_BossUsesHeroPower_02 = new AssetReference("VO_BOM_10_006_Female_Dragon_Onyxia_InGame_BossUsesHeroPower_02.prefab:8ff92bb121aa605458b5e53e58b866fc");

	private static readonly AssetReference VO_BOM_10_006_Female_Dragon_Onyxia_InGame_BossUsesHeroPower_03 = new AssetReference("VO_BOM_10_006_Female_Dragon_Onyxia_InGame_BossUsesHeroPower_03.prefab:58711c381071662408784361d86688ac");

	private static readonly AssetReference VO_BOM_10_006_Female_Dragon_Onyxia_InGame_Death_01 = new AssetReference("VO_BOM_10_006_Female_Dragon_Onyxia_InGame_Death_01.prefab:140e57c94dd444a4682dcf69b0c2c312");

	private static readonly AssetReference VO_BOM_10_006_Female_Dragon_Onyxia_InGame_EmoteResponse_01 = new AssetReference("VO_BOM_10_006_Female_Dragon_Onyxia_InGame_EmoteResponse_01.prefab:48639d1cec0527e429811b7bc572dfb4");

	private static readonly AssetReference VO_BOM_10_006_Female_Dragon_Onyxia_InGame_HE_Custom_IfDawngraspTargetsOnyxia_01 = new AssetReference("VO_BOM_10_006_Female_Dragon_Onyxia_InGame_HE_Custom_IfDawngraspTargetsOnyxia_01.prefab:2c5188a2223e1344095494cb18700669");

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

	private static readonly AssetReference VO_BOM_10_006_Female_Dragon_Onyxia_InGame_Introduction_01_B = new AssetReference("VO_BOM_10_006_Female_Dragon_Onyxia_InGame_Introduction_01_B.prefab:c31a77b1d5506724fbff338d7eaf42db");

	private static readonly AssetReference VO_BOM_10_006_Female_Dragon_Onyxia_InGame_PlayerLoss_01 = new AssetReference("VO_BOM_10_006_Female_Dragon_Onyxia_InGame_PlayerLoss_01.prefab:e3d1c2ce9db07e74c9c9f9875811d6df");

	private static readonly AssetReference VO_BOM_10_006_Female_Dragon_Onyxia_InGame_VictoryPreExplosion_01 = new AssetReference("VO_BOM_10_006_Female_Dragon_Onyxia_InGame_VictoryPreExplosion_01.prefab:09171c3378020bc4294c1841e46c8057");

	private static readonly AssetReference VO_BOM_10_006_Female_Dragon_Onyxia_UI_AWD_Boss_Reveal_General_06_01_B = new AssetReference("VO_BOM_10_006_Female_Dragon_Onyxia_UI_AWD_Boss_Reveal_General_06_01_B.prefab:a4eea4144589f5544a5cb91816e724ba");

	private static readonly AssetReference VO_BOM_10_006_Male_Human_Varian_InGame_VictoryPostExplosion_01_C = new AssetReference("VO_BOM_10_006_Male_Human_Varian_InGame_VictoryPostExplosion_01_C.prefab:3524b787d2d196c408b04b00a7c63d50");

	private static readonly AssetReference VO_BOM_10_006_X_BloodElf_Dawngrasp_InGame_HE_Custom_IfDawngraspPlaysDrakefireAmulet_01 = new AssetReference("VO_BOM_10_006_X_BloodElf_Dawngrasp_InGame_HE_Custom_IfDawngraspPlaysDrakefireAmulet_01.prefab:2a34170e82544e947b3307281c4a3492");

	private static readonly AssetReference VO_BOM_10_006_X_BloodElf_Dawngrasp_InGame_HE_Custom_IfNaaruDestroyed_01 = new AssetReference("VO_BOM_10_006_X_BloodElf_Dawngrasp_InGame_HE_Custom_IfNaaruDestroyed_01.prefab:0103bb2bde4f19645b09403e678349fa");

	private static readonly AssetReference VO_BOM_10_006_X_BloodElf_Dawngrasp_InGame_Introduction_01_A = new AssetReference("VO_BOM_10_006_X_BloodElf_Dawngrasp_InGame_Introduction_01_A.prefab:c4df0be80e0831f4999d4b267093a730");

	private static readonly AssetReference VO_BOM_10_006_X_BloodElf_Dawngrasp_InGame_VictoryPostExplosion_01_B = new AssetReference("VO_BOM_10_006_X_BloodElf_Dawngrasp_InGame_VictoryPostExplosion_01_B.prefab:3b771133ef562064fb0b71890048a0c4");

	private static readonly AssetReference VO_BOM_10_006_X_BloodElf_Dawngrasp_UI_AWD_Boss_Reveal_General_06_01_A = new AssetReference("VO_BOM_10_006_X_BloodElf_Dawngrasp_UI_AWD_Boss_Reveal_General_06_01_A.prefab:ffac87b250843e341a9ed4dba2bcee75");

	private static readonly AssetReference VO_BOM_10_004_Female_Human_Cariel_InGame_HE_Custom_IfCarielDestroysMinion_01_B = new AssetReference("VO_BOM_10_004_Female_Human_Cariel_InGame_HE_Custom_IfCarielDestroysMinion_01_B.prefab:7d4dc705d489dc849b0c654b4a82388a");

	private static readonly AssetReference VO_BOM_10_004_Female_Human_Cariel_InGame_HE_Custom_IfTurnStartsWithCarielAndKurtrusInPlay_01_A = new AssetReference("VO_BOM_10_004_Female_Human_Cariel_InGame_HE_Custom_IfTurnStartsWithCarielAndKurtrusInPlay_01_A.prefab:199757a26cc461746a68bd9985673ffb");

	private static readonly AssetReference VO_BOM_10_004_Female_Human_Cariel_InGame_HE_Custom_IfTurnStartsWithCarielAndKurtrusInPlay_01_B = new AssetReference("VO_BOM_10_004_Female_Human_Cariel_InGame_HE_Custom_IfTurnStartsWithCarielAndKurtrusInPlay_01_B.prefab:0cfe2f76957a3d44c8642d08cfed873a");

	private static readonly AssetReference VO_BOM_10_004_Female_Orc_Rokara_InGame_HE_Custom_IfCarielDestroysMinion_01_A = new AssetReference("VO_BOM_10_004_Female_Orc_Rokara_InGame_HE_Custom_IfCarielDestroysMinion_01_A.prefab:d3747dc86eafe0a4b9445b832631aaad");

	private static readonly AssetReference VO_BOM_10_004_Female_Orc_Rokara_InGame_HE_Custom_IfTavishDestroysMinion_01_A = new AssetReference("VO_BOM_10_004_Female_Orc_Rokara_InGame_HE_Custom_IfTavishDestroysMinion_01_A.prefab:9da02eb17db8ab744939319ac42dfcc1");

	private static readonly AssetReference VO_BOM_10_004_Female_Orc_Rokara_InGame_HE_Custom_IfTavishDestroysMinion_01_C = new AssetReference("VO_BOM_10_004_Female_Orc_Rokara_InGame_HE_Custom_IfTavishDestroysMinion_01_C.prefab:49d992ddeda7c0744b53d49ece864e9a");

	private static readonly AssetReference VO_BOM_10_004_Male_Dwarf_Tavish_InGame_HE_Custom_IfTavishDestroysMinion_01_B = new AssetReference("VO_BOM_10_004_Male_Dwarf_Tavish_InGame_HE_Custom_IfTavishDestroysMinion_01_B.prefab:d13a215240e57fa478352068b49df32a");

	private static readonly AssetReference VO_BOM_10_004_Male_NightElf_Kurtrus_InGame_HE_Custom_IfTurnStartsWithCarielAndKurtrusInPlay_01_B = new AssetReference("VO_BOM_10_004_Male_NightElf_Kurtrus_InGame_HE_Custom_IfTurnStartsWithCarielAndKurtrusInPlay_01_B.prefab:5ffc8b7d0c88d74458f707bcbfb61357");

	private static readonly AssetReference VO_BOM_10_004_Male_NightElf_Kurtrus_InGame_HE_Custom_IfTurnStartsWithCarielAndKurtrusInPlay_01_C = new AssetReference("VO_BOM_10_004_Male_NightElf_Kurtrus_InGame_HE_Custom_IfTurnStartsWithCarielAndKurtrusInPlay_01_C.prefab:15df69805501d7e4aabb7fae817525cc");

	private static readonly AssetReference VO_BOM_09_005_Female_Draenei_Xyrella_InGame_HE_Custom_IfTurnStartsWithXyrellaAndGuffInPlay_01_A = new AssetReference("VO_BOM_09_005_Female_Draenei_Xyrella_InGame_HE_Custom_IfTurnStartsWithXyrellaAndGuffInPlay_01_A.prefab:fa4a839dd5b938d4d8d0c0ee335896c2");

	private static readonly AssetReference VO_BOM_10_005_Male_Tauren_Guff_InGame_HE_Custom_IfTurnStartsWithXyrellaAndGuffInPlay_01_B = new AssetReference("VO_BOM_10_005_Male_Tauren_Guff_InGame_HE_Custom_IfTurnStartsWithXyrellaAndGuffInPlay_01_B.prefab:3f94166da2e5c624c854636c641a0576");

	private static readonly AssetReference VO_BOM_10_005_Male_Tauren_Guff_InGame_HE_Custom_IfTurnStartsWithXyrellaAndGuffInPlay_01_C = new AssetReference("VO_BOM_10_005_Male_Tauren_Guff_InGame_HE_Custom_IfTurnStartsWithXyrellaAndGuffInPlay_01_C.prefab:a9ff33ea0002d84459fc7703b7b333fb");

	private static readonly AssetReference VO_BOM_10_005_Male_Gnome_Scabbs_InGame_HE_Custom_IfTurnStartsWithScabbsInPlay_01_B = new AssetReference("VO_BOM_10_005_Male_Gnome_Scabbs_InGame_HE_Custom_IfTurnStartsWithScabbsInPlay_01_B.prefab:5962fa4fca929264482f599b14ee8e4d");

	private static readonly AssetReference VO_BOM_10_005_X_BloodElf_Dawngrasp_InGame_HE_Custom_IfTurnStartsWithScabbsInPlay_01_A = new AssetReference("VO_BOM_10_005_X_BloodElf_Dawngrasp_InGame_HE_Custom_IfTurnStartsWithScabbsInPlay_01_A.prefab:9d44dd7536e61834683a2a3c61b3213c");

	private static readonly AssetReference Varian_BrassRing_Quote = new AssetReference("Varian_BrassRing_Quote.prefab:b192b80fcc22d1145bfa81b476cecc09");

	private List<string> m_InGame_BossCastsBreathOfFire = new List<string> { VO_BOM_10_006_Female_Dragon_Onyxia_InGame_HE_Custom_IfOnyxiaPlaysBreathOfFire_1_01, VO_BOM_10_006_Female_Dragon_Onyxia_InGame_HE_Custom_IfOnyxiaPlaysBreathOfFire_2_01, VO_BOM_10_006_Female_Dragon_Onyxia_InGame_HE_Custom_IfOnyxiaPlaysBreathOfFire_3_01 };

	private List<string> m_InGame_BossCastsScaleOfOnyxiae = new List<string> { VO_BOM_10_006_Female_Dragon_Onyxia_InGame_HE_Custom_IfOnyxiaPlaysScaleOfOnyxia_1_01, VO_BOM_10_006_Female_Dragon_Onyxia_InGame_HE_Custom_IfOnyxiaPlaysScaleOfOnyxia_2_01 };

	private List<string> m_InGame_BossIdleLines = new List<string> { VO_BOM_10_006_Female_Dragon_Onyxia_InGame_BossIdle_01, VO_BOM_10_006_Female_Dragon_Onyxia_InGame_BossIdle_02, VO_BOM_10_006_Female_Dragon_Onyxia_InGame_BossIdle_03 };

	private List<string> m_InGame_BossUsesHeroPowerLines = new List<string> { VO_BOM_10_006_Female_Dragon_Onyxia_InGame_BossUsesHeroPower_01, VO_BOM_10_006_Female_Dragon_Onyxia_InGame_BossUsesHeroPower_02, VO_BOM_10_006_Female_Dragon_Onyxia_InGame_BossUsesHeroPower_03 };

	private HashSet<string> m_playedLines = new HashSet<string>();

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> m_SoundFiles = new List<string>
		{
			VO_BOM_10_006_Female_Dragon_Onyxia_InGame_BossIdle_01, VO_BOM_10_006_Female_Dragon_Onyxia_InGame_BossIdle_02, VO_BOM_10_006_Female_Dragon_Onyxia_InGame_BossIdle_03, VO_BOM_10_006_Female_Dragon_Onyxia_InGame_BossUsesHeroPower_01, VO_BOM_10_006_Female_Dragon_Onyxia_InGame_BossUsesHeroPower_02, VO_BOM_10_006_Female_Dragon_Onyxia_InGame_BossUsesHeroPower_03, VO_BOM_10_006_Female_Dragon_Onyxia_InGame_Death_01, VO_BOM_10_006_Female_Dragon_Onyxia_InGame_EmoteResponse_01, VO_BOM_10_006_Female_Dragon_Onyxia_InGame_HE_Custom_IfOnyxiaPlaysBellowingRoar_01, VO_BOM_10_006_Female_Dragon_Onyxia_InGame_HE_Custom_IfOnyxiaPlaysBreathOfFire_1_01,
			VO_BOM_10_006_Female_Dragon_Onyxia_InGame_HE_Custom_IfOnyxiaPlaysBreathOfFire_2_01, VO_BOM_10_006_Female_Dragon_Onyxia_InGame_HE_Custom_IfOnyxiaPlaysBreathOfFire_3_01, VO_BOM_10_006_Female_Dragon_Onyxia_InGame_HE_Custom_IfOnyxiaPlaysDeepBreath_01, VO_BOM_10_006_Female_Dragon_Onyxia_InGame_HE_Custom_IfOnyxiaPlaysDontStandInTheFire_01, VO_BOM_10_006_Female_Dragon_Onyxia_InGame_HE_Custom_IfOnyxiaPlaysScaleOfOnyxia_1_01, VO_BOM_10_006_Female_Dragon_Onyxia_InGame_HE_Custom_IfOnyxiaPlaysScaleOfOnyxia_2_01, VO_BOM_10_006_Female_Dragon_Onyxia_InGame_HE_Custom_IfOnyxiaPlaysTailSweep_01, VO_BOM_10_006_Female_Dragon_Onyxia_InGame_HE_Custom_IfOnyxiaPlaysUltimateImpfestation_01, VO_BOM_10_006_Female_Dragon_Onyxia_InGame_HE_Custom_IfOnyxiaPlaysWingBuffet_01, VO_BOM_10_006_Female_Dragon_Onyxia_InGame_Introduction_01_B,
			VO_BOM_10_006_Female_Dragon_Onyxia_InGame_PlayerLoss_01, VO_BOM_10_006_Female_Dragon_Onyxia_InGame_VictoryPreExplosion_01, VO_BOM_10_006_Male_Human_Varian_InGame_VictoryPostExplosion_01_C, VO_BOM_10_006_X_BloodElf_Dawngrasp_InGame_HE_Custom_IfDawngraspPlaysDrakefireAmulet_01, VO_BOM_10_006_X_BloodElf_Dawngrasp_InGame_Introduction_01_A, VO_BOM_10_006_X_BloodElf_Dawngrasp_InGame_VictoryPostExplosion_01_B, VO_BOM_10_004_Female_Human_Cariel_InGame_HE_Custom_IfCarielDestroysMinion_01_B, VO_BOM_10_004_Female_Human_Cariel_InGame_HE_Custom_IfTurnStartsWithCarielAndKurtrusInPlay_01_A, VO_BOM_10_004_Female_Human_Cariel_InGame_HE_Custom_IfTurnStartsWithCarielAndKurtrusInPlay_01_B, VO_BOM_10_004_Female_Orc_Rokara_InGame_HE_Custom_IfCarielDestroysMinion_01_A,
			VO_BOM_10_004_Female_Orc_Rokara_InGame_HE_Custom_IfTavishDestroysMinion_01_A, VO_BOM_10_004_Female_Orc_Rokara_InGame_HE_Custom_IfTavishDestroysMinion_01_C, VO_BOM_10_004_Male_Dwarf_Tavish_InGame_HE_Custom_IfTavishDestroysMinion_01_B, VO_BOM_10_004_Male_NightElf_Kurtrus_InGame_HE_Custom_IfTurnStartsWithCarielAndKurtrusInPlay_01_B, VO_BOM_10_004_Male_NightElf_Kurtrus_InGame_HE_Custom_IfTurnStartsWithCarielAndKurtrusInPlay_01_C, VO_BOM_09_005_Female_Draenei_Xyrella_InGame_HE_Custom_IfTurnStartsWithXyrellaAndGuffInPlay_01_A, VO_BOM_10_005_Male_Tauren_Guff_InGame_HE_Custom_IfTurnStartsWithXyrellaAndGuffInPlay_01_B, VO_BOM_10_005_Male_Tauren_Guff_InGame_HE_Custom_IfTurnStartsWithXyrellaAndGuffInPlay_01_C, VO_BOM_10_005_X_BloodElf_Dawngrasp_InGame_HE_Custom_IfTurnStartsWithScabbsInPlay_01_A, VO_BOM_10_005_Male_Gnome_Scabbs_InGame_HE_Custom_IfTurnStartsWithScabbsInPlay_01_B,
			VO_BOM_10_006_Female_Dragon_Onyxia_InGame_HE_Custom_IfDawngraspTargetsOnyxia_01, VO_BOM_10_006_X_BloodElf_Dawngrasp_InGame_HE_Custom_IfNaaruDestroyed_01
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
		case 516:
			yield return MissionPlayVO(enemyActor, VO_BOM_10_006_Female_Dragon_Onyxia_InGame_Death_01);
			break;
		case 515:
			yield return MissionPlayVO(enemyActor, VO_BOM_10_006_Female_Dragon_Onyxia_InGame_EmoteResponse_01);
			break;
		case 506:
			yield return MissionPlayVO(enemyActor, VO_BOM_10_006_Female_Dragon_Onyxia_InGame_PlayerLoss_01);
			break;
		case 505:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(friendlyActor, VO_BOM_10_006_X_BloodElf_Dawngrasp_InGame_VictoryPostExplosion_01_B);
			yield return MissionPlayVO(Varian_BrassRing_Quote, VO_BOM_10_006_Male_Human_Varian_InGame_VictoryPostExplosion_01_C);
			GameState.Get().SetBusy(busy: false);
			break;
		case 101:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVOOnce(BOM_10_Dawngrasp_Dungeon.Alterac_CarielArt_BrassRing_Quote, VO_BOM_10_004_Female_Human_Cariel_InGame_HE_Custom_IfTurnStartsWithCarielAndKurtrusInPlay_01_A);
			yield return MissionPlayVOOnce(BOM_10_Dawngrasp_Dungeon.Alterac_CarielArt_BrassRing_Quote, VO_BOM_10_004_Female_Human_Cariel_InGame_HE_Custom_IfTurnStartsWithCarielAndKurtrusInPlay_01_B);
			yield return MissionPlayVOOnce(BOM_10_Dawngrasp_Dungeon.KurtrusTier5_BrassRing_Quote, VO_BOM_10_004_Male_NightElf_Kurtrus_InGame_HE_Custom_IfTurnStartsWithCarielAndKurtrusInPlay_01_B);
			yield return MissionPlayVOOnce(BOM_10_Dawngrasp_Dungeon.KurtrusTier5_BrassRing_Quote, VO_BOM_10_004_Male_NightElf_Kurtrus_InGame_HE_Custom_IfTurnStartsWithCarielAndKurtrusInPlay_01_C);
			GameState.Get().SetBusy(busy: false);
			break;
		case 103:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVOOnce(BOM_10_Dawngrasp_Dungeon.RokaraTier5_BrassRing_Quote, VO_BOM_10_004_Female_Orc_Rokara_InGame_HE_Custom_IfTavishDestroysMinion_01_A);
			yield return MissionPlayVOOnce(BOM_10_Dawngrasp_Dungeon.Alterac_TavishArt_BrassRing_Quote, VO_BOM_10_004_Male_Dwarf_Tavish_InGame_HE_Custom_IfTavishDestroysMinion_01_B);
			yield return MissionPlayVOOnce(BOM_10_Dawngrasp_Dungeon.RokaraTier5_BrassRing_Quote, VO_BOM_10_004_Female_Orc_Rokara_InGame_HE_Custom_IfTavishDestroysMinion_01_C);
			GameState.Get().SetBusy(busy: false);
			break;
		case 104:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVOOnce(BOM_10_Dawngrasp_Dungeon.RokaraTier5_BrassRing_Quote, VO_BOM_10_004_Female_Orc_Rokara_InGame_HE_Custom_IfCarielDestroysMinion_01_A);
			yield return MissionPlayVOOnce(BOM_10_Dawngrasp_Dungeon.Alterac_CarielArt_BrassRing_Quote, VO_BOM_10_004_Female_Human_Cariel_InGame_HE_Custom_IfCarielDestroysMinion_01_B);
			GameState.Get().SetBusy(busy: false);
			break;
		case 105:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVOOnce(BOM_10_Dawngrasp_Dungeon.Alterac_XyrellaArt_BrassRing_Quote, VO_BOM_09_005_Female_Draenei_Xyrella_InGame_HE_Custom_IfTurnStartsWithXyrellaAndGuffInPlay_01_A);
			yield return MissionPlayVOOnce(BOM_10_Dawngrasp_Dungeon.Guff_BrassRing_Quote, VO_BOM_10_005_Male_Tauren_Guff_InGame_HE_Custom_IfTurnStartsWithXyrellaAndGuffInPlay_01_B);
			yield return MissionPlayVOOnce(BOM_10_Dawngrasp_Dungeon.Guff_BrassRing_Quote, VO_BOM_10_005_Male_Tauren_Guff_InGame_HE_Custom_IfTurnStartsWithXyrellaAndGuffInPlay_01_C);
			GameState.Get().SetBusy(busy: false);
			break;
		case 106:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVOOnce(friendlyActor, VO_BOM_10_005_X_BloodElf_Dawngrasp_InGame_HE_Custom_IfTurnStartsWithScabbsInPlay_01_A);
			yield return MissionPlayVOOnce(BOM_10_Dawngrasp_Dungeon.Alterac_ScabbsArt_BrassRing_Quote, VO_BOM_10_005_Male_Gnome_Scabbs_InGame_HE_Custom_IfTurnStartsWithScabbsInPlay_01_B);
			GameState.Get().SetBusy(busy: false);
			break;
		case 107:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVOOnce(enemyActor, VO_BOM_10_006_Female_Dragon_Onyxia_InGame_HE_Custom_IfDawngraspTargetsOnyxia_01);
			GameState.Get().SetBusy(busy: false);
			break;
		case 108:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVOOnce(enemyActor, VO_BOM_10_006_Female_Dragon_Onyxia_InGame_VictoryPreExplosion_01);
			GameState.Get().SetBusy(busy: false);
			break;
		default:
			yield return base.HandleMissionEventWithTiming(missionEvent);
			break;
		case 102:
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
			if (cardID == "ONY_029")
			{
				yield return MissionPlayVO(friendlyActor, VO_BOM_10_006_X_BloodElf_Dawngrasp_InGame_HE_Custom_IfDawngraspPlaysDrakefireAmulet_01);
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
		Actor enemyActor = GameState.Get().GetOpposingSidePlayer().GetHero()
			.GetCard()
			.GetActor();
		Actor friendlyActor = GameState.Get().GetFriendlySidePlayer().GetHero()
			.GetCard()
			.GetActor();
		if (turn == 1)
		{
			yield return MissionPlayVOOnce(friendlyActor, VO_BOM_10_006_X_BloodElf_Dawngrasp_InGame_Introduction_01_A);
			yield return MissionPlayVOOnce(enemyActor, VO_BOM_10_006_Female_Dragon_Onyxia_InGame_Introduction_01_B);
		}
	}
}
