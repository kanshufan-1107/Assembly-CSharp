using System.Collections;
using System.Collections.Generic;

public class BOM_10_Dawngrasp_Fight_004 : BOM_10_Dawngrasp_Dungeon
{
	private static readonly AssetReference VO_BOM_10_004_Female_Draenei_Xyrella_UI_AWD_Boss_Reveal_General_08_01_C = new AssetReference("VO_BOM_10_004_Female_Draenei_Xyrella_UI_AWD_Boss_Reveal_General_08_01_C.prefab:e922abbe793de834db594137264a3f93");

	private static readonly AssetReference VO_BOM_10_004_Female_Dragon_Onyxia_InGame_BossIdle_01 = new AssetReference("VO_BOM_10_004_Female_Dragon_Onyxia_InGame_BossIdle_01.prefab:32ea99dd0d9d88a419df7aef13dd387e");

	private static readonly AssetReference VO_BOM_10_004_Female_Dragon_Onyxia_InGame_BossIdle_02 = new AssetReference("VO_BOM_10_004_Female_Dragon_Onyxia_InGame_BossIdle_02.prefab:3c0889d825ba2f84db7dc3f0463546c9");

	private static readonly AssetReference VO_BOM_10_004_Female_Dragon_Onyxia_InGame_BossIdle_03 = new AssetReference("VO_BOM_10_004_Female_Dragon_Onyxia_InGame_BossIdle_03.prefab:b3d1d5d01980cee4e80129f2a040745f");

	private static readonly AssetReference VO_BOM_10_004_Female_Dragon_Onyxia_InGame_BossUsesHeroPower_01 = new AssetReference("VO_BOM_10_004_Female_Dragon_Onyxia_InGame_BossUsesHeroPower_01.prefab:a3b1f9592738cbc479b261f36f5424a1");

	private static readonly AssetReference VO_BOM_10_004_Female_Dragon_Onyxia_InGame_BossUsesHeroPower_02 = new AssetReference("VO_BOM_10_004_Female_Dragon_Onyxia_InGame_BossUsesHeroPower_02.prefab:98e00918da9c8ab4bbb92ffd7e66bcfc");

	private static readonly AssetReference VO_BOM_10_004_Female_Dragon_Onyxia_InGame_BossUsesHeroPower_03 = new AssetReference("VO_BOM_10_004_Female_Dragon_Onyxia_InGame_BossUsesHeroPower_03.prefab:4c032646d05f4d949aa591e3810ec10c");

	private static readonly AssetReference VO_BOM_10_004_Female_Dragon_Onyxia_InGame_EmoteResponse_01 = new AssetReference("VO_BOM_10_004_Female_Dragon_Onyxia_InGame_EmoteResponse_01.prefab:e6f04b0f83f213c40abbf194ecb0b4a6");

	private static readonly AssetReference VO_BOM_10_004_Female_Dragon_Onyxia_InGame_Introduction_01_A = new AssetReference("VO_BOM_10_004_Female_Dragon_Onyxia_InGame_Introduction_01_A.prefab:abe01c98d5f107349a9360f02dcd74d9");

	private static readonly AssetReference VO_BOM_10_004_Female_Dragon_Onyxia_InGame_PlayerLoss_01 = new AssetReference("VO_BOM_10_004_Female_Dragon_Onyxia_InGame_PlayerLoss_01.prefab:332de72c7c722ac4ead162813856a18d");

	private static readonly AssetReference VO_BOM_10_004_Female_Dragon_Onyxia_InGame_Turn_01_01_C = new AssetReference("VO_BOM_10_004_Female_Dragon_Onyxia_InGame_Turn_01_01_C.prefab:20c77b99b8391544f8c07d1e8a2929c9");

	private static readonly AssetReference VO_BOM_10_004_Female_Dragon_Onyxia_InGame_Turn_05_01_A = new AssetReference("VO_BOM_10_004_Female_Dragon_Onyxia_InGame_Turn_05_01_A.prefab:93077558f3825f64ba908df2e3d05cbe");

	private static readonly AssetReference VO_BOM_10_004_Female_Dragon_Onyxia_InGame_Turn_07_01_A = new AssetReference("VO_BOM_10_004_Female_Dragon_Onyxia_InGame_Turn_07_01_A.prefab:c54f1a0084ccbd34bab9fd96b3bbcf60");

	private static readonly AssetReference VO_BOM_10_004_Female_Dragon_Onyxia_InGame_VictoryPreExplosion_01_B = new AssetReference("VO_BOM_10_004_Female_Dragon_Onyxia_InGame_VictoryPreExplosion_01_B.prefab:5b9c20e3fd2af51499b32b8ca13141fe");

	private static readonly AssetReference VO_BOM_10_004_Female_Dragon_Onyxia_UI_AWD_Boss_Reveal_General_04_01_C = new AssetReference("VO_BOM_10_004_Female_Dragon_Onyxia_UI_AWD_Boss_Reveal_General_04_01_C.prefab:2b6fc7fa245cef2499a947ae0ebbc5cb");

	private static readonly AssetReference VO_BOM_10_004_Female_Human_Cariel_InGame_HE_Custom_IfCarielDestroysMinion_01_B = new AssetReference("VO_BOM_10_004_Female_Human_Cariel_InGame_HE_Custom_IfCarielDestroysMinion_01_B.prefab:7d4dc705d489dc849b0c654b4a82388a");

	private static readonly AssetReference VO_BOM_10_004_Female_Human_Cariel_InGame_HE_Custom_IfTurnStartsWithCarielAndKurtrusInPlay_01_A = new AssetReference("VO_BOM_10_004_Female_Human_Cariel_InGame_HE_Custom_IfTurnStartsWithCarielAndKurtrusInPlay_01_A.prefab:199757a26cc461746a68bd9985673ffb");

	private static readonly AssetReference VO_BOM_10_004_Female_Human_Cariel_InGame_HE_Custom_IfTurnStartsWithCarielAndKurtrusInPlay_01_B = new AssetReference("VO_BOM_10_004_Female_Human_Cariel_InGame_HE_Custom_IfTurnStartsWithCarielAndKurtrusInPlay_01_B.prefab:0cfe2f76957a3d44c8642d08cfed873a");

	private static readonly AssetReference VO_BOM_10_004_Female_Human_Cariel_UI_AWD_Boss_Reveal_General_01_01_C = new AssetReference("VO_BOM_10_004_Female_Human_Cariel_UI_AWD_Boss_Reveal_General_01_01_C.prefab:5e083dcd0e5c0cd49b33f371f20c6daf");

	private static readonly AssetReference VO_BOM_10_004_Female_Human_Cariel_UI_AWD_Boss_Reveal_General_03_01_C = new AssetReference("VO_BOM_10_004_Female_Human_Cariel_UI_AWD_Boss_Reveal_General_03_01_C.prefab:19a2a06986be71d47bec4ba74a6d93ea");

	private static readonly AssetReference VO_BOM_10_004_Female_Orc_Rokara_InGame_HE_Custom_IfCarielDestroysMinion_01_A = new AssetReference("VO_BOM_10_004_Female_Orc_Rokara_InGame_HE_Custom_IfCarielDestroysMinion_01_A.prefab:d3747dc86eafe0a4b9445b832631aaad");

	private static readonly AssetReference VO_BOM_10_004_Female_Orc_Rokara_InGame_HE_Custom_IfTavishDestroysMinion_01_A = new AssetReference("VO_BOM_10_004_Female_Orc_Rokara_InGame_HE_Custom_IfTavishDestroysMinion_01_A.prefab:9da02eb17db8ab744939319ac42dfcc1");

	private static readonly AssetReference VO_BOM_10_004_Female_Orc_Rokara_InGame_HE_Custom_IfTavishDestroysMinion_01_C = new AssetReference("VO_BOM_10_004_Female_Orc_Rokara_InGame_HE_Custom_IfTavishDestroysMinion_01_C.prefab:49d992ddeda7c0744b53d49ece864e9a");

	private static readonly AssetReference VO_BOM_10_004_Male_Dwarf_Tavish_InGame_HE_Custom_IfTavishDestroysMinion_01_B = new AssetReference("VO_BOM_10_004_Male_Dwarf_Tavish_InGame_HE_Custom_IfTavishDestroysMinion_01_B.prefab:d13a215240e57fa478352068b49df32a");

	private static readonly AssetReference VO_BOM_10_004_Male_Gnome_GuildLeader_InGame_Turn_01_01_A = new AssetReference("VO_BOM_10_004_Male_Gnome_GuildLeader_InGame_Turn_01_01_A.prefab:92108926c01a1bb4491ec43086417b28");

	private static readonly AssetReference VO_BOM_10_004_Male_Gnome_GuildLeader_InGame_Turn_01_01_B = new AssetReference("VO_BOM_10_004_Male_Gnome_GuildLeader_InGame_Turn_01_01_B.prefab:479a6c0227f04624a967a45afc73b7e0");

	private static readonly AssetReference VO_BOM_10_004_Male_NightElf_Kurtrus_InGame_HE_Custom_IfTurnStartsWithCarielAndKurtrusInPlay_01_B = new AssetReference("VO_BOM_10_004_Male_NightElf_Kurtrus_InGame_HE_Custom_IfTurnStartsWithCarielAndKurtrusInPlay_01_B.prefab:5ffc8b7d0c88d74458f707bcbfb61357");

	private static readonly AssetReference VO_BOM_10_004_Male_NightElf_Kurtrus_InGame_HE_Custom_IfTurnStartsWithCarielAndKurtrusInPlay_01_C = new AssetReference("VO_BOM_10_004_Male_NightElf_Kurtrus_InGame_HE_Custom_IfTurnStartsWithCarielAndKurtrusInPlay_01_C.prefab:15df69805501d7e4aabb7fae817525cc");

	private static readonly AssetReference VO_BOM_10_004_X_BloodElf_Dawngrasp_InGame_Turn_05_01_B = new AssetReference("VO_BOM_10_004_X_BloodElf_Dawngrasp_InGame_Turn_05_01_B.prefab:aacc64cc594d360478d25e64a459edaa");

	private static readonly AssetReference VO_BOM_10_004_X_BloodElf_Dawngrasp_UI_AWD_Boss_Reveal_General_04_01_A = new AssetReference("VO_BOM_10_004_X_BloodElf_Dawngrasp_UI_AWD_Boss_Reveal_General_04_01_A.prefab:f984635f43889954aa0ee59a58e80706");

	private static readonly AssetReference VO_BOM_10_004_X_BloodElf_Dawngrasp_UI_AWD_Boss_Reveal_General_04_01_B = new AssetReference("VO_BOM_10_004_X_BloodElf_Dawngrasp_UI_AWD_Boss_Reveal_General_04_01_B.prefab:7ce0b64caf0e42b4fab1a26298bada11");

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

	private static readonly AssetReference VO_BOM_08_004_Male_Dragon_Kazakusan_InGame_VictoryPreExplosion_01_A = new AssetReference("VO_BOM_08_004_Male_Dragon_Kazakusan_InGame_VictoryPreExplosion_01_A.prefab:db6654fb1c2fa6a40b43f8b69a7df09c");

	private List<string> m_InGame_BossIdleLines = new List<string> { VO_BOM_10_004_Female_Dragon_Onyxia_InGame_BossIdle_01, VO_BOM_10_004_Female_Dragon_Onyxia_InGame_BossIdle_02, VO_BOM_10_004_Female_Dragon_Onyxia_InGame_BossIdle_03 };

	private List<string> m_InGame_BossUsesHeroPowerLines = new List<string> { VO_BOM_10_004_Female_Dragon_Onyxia_InGame_BossUsesHeroPower_01, VO_BOM_10_004_Female_Dragon_Onyxia_InGame_BossUsesHeroPower_02, VO_BOM_10_004_Female_Dragon_Onyxia_InGame_BossUsesHeroPower_03 };

	private HashSet<string> m_playedLines = new HashSet<string>();

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> m_SoundFiles = new List<string>
		{
			VO_BOM_10_004_Female_Dragon_Onyxia_InGame_BossIdle_01, VO_BOM_10_004_Female_Dragon_Onyxia_InGame_BossIdle_02, VO_BOM_10_004_Female_Dragon_Onyxia_InGame_BossIdle_03, VO_BOM_10_004_Female_Dragon_Onyxia_InGame_BossUsesHeroPower_01, VO_BOM_10_004_Female_Dragon_Onyxia_InGame_BossUsesHeroPower_02, VO_BOM_10_004_Female_Dragon_Onyxia_InGame_BossUsesHeroPower_03, VO_BOM_10_004_Female_Dragon_Onyxia_InGame_EmoteResponse_01, VO_BOM_10_004_Female_Dragon_Onyxia_InGame_Introduction_01_A, VO_BOM_10_004_Female_Dragon_Onyxia_InGame_PlayerLoss_01, VO_BOM_10_004_Female_Dragon_Onyxia_InGame_Turn_01_01_C,
			VO_BOM_10_004_Female_Dragon_Onyxia_InGame_Turn_05_01_A, VO_BOM_10_004_Female_Dragon_Onyxia_InGame_Turn_07_01_A, VO_BOM_10_004_Female_Dragon_Onyxia_InGame_VictoryPreExplosion_01_B, VO_BOM_10_004_Female_Human_Cariel_InGame_HE_Custom_IfCarielDestroysMinion_01_B, VO_BOM_10_004_Female_Human_Cariel_InGame_HE_Custom_IfTurnStartsWithCarielAndKurtrusInPlay_01_A, VO_BOM_10_004_Male_Gnome_GuildLeader_InGame_Turn_01_01_A, VO_BOM_10_004_Male_Gnome_GuildLeader_InGame_Turn_01_01_B, VO_BOM_10_004_X_BloodElf_Dawngrasp_InGame_Turn_05_01_B, VO_BOM_10_006_Female_Dragon_Onyxia_InGame_HE_Custom_IfDawngraspTargetsOnyxia_01, VO_BOM_10_006_Female_Dragon_Onyxia_InGame_HE_Custom_IfOnyxiaPlaysBellowingRoar_01,
			VO_BOM_10_006_Female_Dragon_Onyxia_InGame_HE_Custom_IfOnyxiaPlaysBreathOfFire_1_01, VO_BOM_10_006_Female_Dragon_Onyxia_InGame_HE_Custom_IfOnyxiaPlaysBreathOfFire_2_01, VO_BOM_10_006_Female_Dragon_Onyxia_InGame_HE_Custom_IfOnyxiaPlaysBreathOfFire_3_01, VO_BOM_10_006_Female_Dragon_Onyxia_InGame_HE_Custom_IfOnyxiaPlaysDeepBreath_01, VO_BOM_10_006_Female_Dragon_Onyxia_InGame_HE_Custom_IfOnyxiaPlaysDontStandInTheFire_01, VO_BOM_10_006_Female_Dragon_Onyxia_InGame_HE_Custom_IfOnyxiaPlaysScaleOfOnyxia_1_01, VO_BOM_10_006_Female_Dragon_Onyxia_InGame_HE_Custom_IfOnyxiaPlaysScaleOfOnyxia_2_01, VO_BOM_10_006_Female_Dragon_Onyxia_InGame_HE_Custom_IfOnyxiaPlaysTailSweep_01, VO_BOM_10_006_Female_Dragon_Onyxia_InGame_HE_Custom_IfOnyxiaPlaysUltimateImpfestation_01, VO_BOM_10_006_Female_Dragon_Onyxia_InGame_HE_Custom_IfOnyxiaPlaysWingBuffet_01,
			VO_BOM_08_004_Male_Dragon_Kazakusan_InGame_VictoryPreExplosion_01_A
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
		GameState.Get().GetFriendlySidePlayer().GetHero()
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
			yield return MissionPlayVO(enemyActor, VO_BOM_10_004_Female_Dragon_Onyxia_InGame_EmoteResponse_01);
			break;
		case 514:
			yield return MissionPlayVO(enemyActor, VO_BOM_10_004_Female_Dragon_Onyxia_InGame_Introduction_01_A);
			break;
		case 506:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(enemyActor, VO_BOM_10_004_Female_Dragon_Onyxia_InGame_PlayerLoss_01);
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
		case 5:
			yield return MissionPlayVO(enemyActor, VO_BOM_10_004_Female_Dragon_Onyxia_InGame_Turn_05_01_A);
			yield return MissionPlayVO(friendlyActor, VO_BOM_10_004_X_BloodElf_Dawngrasp_InGame_Turn_05_01_B);
			break;
		case 8:
			yield return MissionPlayVO(enemyActor, VO_BOM_10_004_Female_Dragon_Onyxia_InGame_Turn_07_01_A);
			break;
		}
	}
}
