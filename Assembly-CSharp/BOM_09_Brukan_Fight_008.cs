using System.Collections;
using System.Collections.Generic;

public class BOM_09_Brukan_Fight_008 : BOM_09_Brukan_Dungeon
{
	private static readonly AssetReference VO_BOM_09_008_Female_Draenei_Xyrella_Emote_Attack_01 = new AssetReference("VO_BOM_09_008_Female_Draenei_Xyrella_Emote_Attack_01.prefab:32368b2b8ee2dbc4baed462f9e646741");

	private static readonly AssetReference VO_BOM_09_008_Female_Draenei_Xyrella_Emote_Play_01 = new AssetReference("VO_BOM_09_008_Female_Draenei_Xyrella_Emote_Play_01.prefab:f77bb14593e949f429a77dc5e0c32b66");

	private static readonly AssetReference VO_BOM_09_008_Female_Draenei_Xyrella_InGame_HE_Custom_IfXyrellaPlayed_01_B = new AssetReference("VO_BOM_09_008_Female_Draenei_Xyrella_InGame_HE_Custom_IfXyrellaPlayed_01_B.prefab:9ebf4239361b04d43ac1d71dbf2e55d2");

	private static readonly AssetReference VO_BOM_09_008_Female_Human_Cariel_InGame_HE_Custom_IfCarielPlayed_01_B = new AssetReference("VO_BOM_09_008_Female_Human_Cariel_InGame_HE_Custom_IfCarielPlayed_01_B.prefab:fe2e215a4f75bd543a1fec3501699d35");

	private static readonly AssetReference VO_BOM_09_008_Female_Human_Cariel_InGame_HE_Custom_IfTamsinHalfHealthPlusOne_01_B = new AssetReference("VO_BOM_09_008_Female_Human_Cariel_InGame_HE_Custom_IfTamsinHalfHealthPlusOne_01_B.prefab:724f4924d7e904f4995b5b895bb0d941");

	private static readonly AssetReference VO_BOM_09_008_Female_Human_Cariel_InGame_VictoryPostExplosion_01 = new AssetReference("VO_BOM_09_008_Female_Human_Cariel_InGame_VictoryPostExplosion_01.prefab:7b48e2ea87bb0414fa2ffa80ef1d5384");

	private static readonly AssetReference VO_BOM_09_008_Female_Orc_Rokara_InGame_HE_Custom_IfRokaraPlayed_01_B = new AssetReference("VO_BOM_09_008_Female_Orc_Rokara_InGame_HE_Custom_IfRokaraPlayed_01_B.prefab:e4c94acef61fd434a8e331a783ada2e0");

	private static readonly AssetReference VO_BOM_09_008_Female_Orc_Rokara_InGame_HE_Custom_IfTamsinHalfHealth_01_B = new AssetReference("VO_BOM_09_008_Female_Orc_Rokara_InGame_HE_Custom_IfTamsinHalfHealth_01_B.prefab:01c97608a32dda34b9d4c25af3400c82");

	private static readonly AssetReference VO_BOM_09_008_Female_Undead_LichTamsin_InGame_BossIdle_01 = new AssetReference("VO_BOM_09_008_Female_Undead_LichTamsin_InGame_BossIdle_01.prefab:8daee33ac8b4e784d91d06c2b623630e");

	private static readonly AssetReference VO_BOM_09_008_Female_Undead_LichTamsin_InGame_BossIdle_02 = new AssetReference("VO_BOM_09_008_Female_Undead_LichTamsin_InGame_BossIdle_02.prefab:f04748b03bae4274ea84f3ce21e7c782");

	private static readonly AssetReference VO_BOM_09_008_Female_Undead_LichTamsin_InGame_BossIdle_03 = new AssetReference("VO_BOM_09_008_Female_Undead_LichTamsin_InGame_BossIdle_03.prefab:ee3f862335c2a0c42b741dac3361fc35");

	private static readonly AssetReference VO_BOM_09_008_Female_Undead_LichTamsin_InGame_BossUsesHeroPower_01 = new AssetReference("VO_BOM_09_008_Female_Undead_LichTamsin_InGame_BossUsesHeroPower_01.prefab:6248050bfe35fe549a834ea8de821701");

	private static readonly AssetReference VO_BOM_09_008_Female_Undead_LichTamsin_InGame_BossUsesHeroPower_02 = new AssetReference("VO_BOM_09_008_Female_Undead_LichTamsin_InGame_BossUsesHeroPower_02.prefab:49a5009d05b6a87438a01b3b4f50ee74");

	private static readonly AssetReference VO_BOM_09_008_Female_Undead_LichTamsin_InGame_BossUsesHeroPower_03 = new AssetReference("VO_BOM_09_008_Female_Undead_LichTamsin_InGame_BossUsesHeroPower_03.prefab:d0d609729077d774c993f9a91b0f4035");

	private static readonly AssetReference VO_BOM_09_008_Female_Undead_LichTamsin_InGame_EmoteResponse_01 = new AssetReference("VO_BOM_09_008_Female_Undead_LichTamsin_InGame_EmoteResponse_01.prefab:390f6116d30a6db45bc786662e1ee3f6");

	private static readonly AssetReference VO_BOM_09_008_Female_Undead_LichTamsin_InGame_HE_Custom_IfCarielPlayed_01_A = new AssetReference("VO_BOM_09_008_Female_Undead_LichTamsin_InGame_HE_Custom_IfCarielPlayed_01_A.prefab:6829094d643a3f24b8bf2bf6f33df243");

	private static readonly AssetReference VO_BOM_09_008_Female_Undead_LichTamsin_InGame_HE_Custom_IfCarielPlayed_01_C = new AssetReference("VO_BOM_09_008_Female_Undead_LichTamsin_InGame_HE_Custom_IfCarielPlayed_01_C.prefab:dd7b7c8d589606a46be7eef5f5b9cd64");

	private static readonly AssetReference VO_BOM_09_008_Female_Undead_LichTamsin_InGame_HE_Custom_IfIvusDestroyed_01 = new AssetReference("VO_BOM_09_008_Female_Undead_LichTamsin_InGame_HE_Custom_IfIvusDestroyed_01.prefab:8f283593ab2ce1b4e99ad0ecb7166ac5");

	private static readonly AssetReference VO_BOM_09_008_Female_Undead_LichTamsin_InGame_HE_Custom_IfKurtrusPlayed_01_B = new AssetReference("VO_BOM_09_008_Female_Undead_LichTamsin_InGame_HE_Custom_IfKurtrusPlayed_01_B.prefab:e39a3fbe9ed9e2d42b6e03d5286e871c");

	private static readonly AssetReference VO_BOM_09_008_Female_Undead_LichTamsin_InGame_HE_Custom_IfLokholarDestroyed_01 = new AssetReference("VO_BOM_09_008_Female_Undead_LichTamsin_InGame_HE_Custom_IfLokholarDestroyed_01.prefab:ff7fd7f251a278445ac0793c40e2e5dd");

	private static readonly AssetReference VO_BOM_09_008_Female_Undead_LichTamsin_InGame_HE_Custom_IfTamsinTenHealth_01_A = new AssetReference("VO_BOM_09_008_Female_Undead_LichTamsin_InGame_HE_Custom_IfTamsinTenHealth_01_A.prefab:6ecf1e5fc99647849a57086485a98be3");

	private static readonly AssetReference VO_BOM_09_008_Female_Undead_LichTamsin_InGame_HE_Custom_IfXyrellaPlayed_01_A = new AssetReference("VO_BOM_09_008_Female_Undead_LichTamsin_InGame_HE_Custom_IfXyrellaPlayed_01_A.prefab:2944e4b2e864acd4e87d408121d3ab65");

	private static readonly AssetReference VO_BOM_09_008_Female_Undead_LichTamsin_InGame_Introduction_01_A = new AssetReference("VO_BOM_09_008_Female_Undead_LichTamsin_InGame_Introduction_01_A.prefab:24868064d1eec9b49b48b1bb44c5894a");

	private static readonly AssetReference VO_BOM_09_008_Female_Undead_LichTamsin_InGame_PlayerLoss_01 = new AssetReference("VO_BOM_09_008_Female_Undead_LichTamsin_InGame_PlayerLoss_01.prefab:38ace5068497bf3408e3131650f0176e");

	private static readonly AssetReference VO_BOM_09_008_Male_NightElf_Kurtrus_Emote_Attack_01 = new AssetReference("VO_BOM_09_008_Male_NightElf_Kurtrus_Emote_Attack_01.prefab:c8e0d9fd23321064bb159fe036b78b6f");

	private static readonly AssetReference VO_BOM_09_008_Male_NightElf_Kurtrus_Emote_Death_01 = new AssetReference("VO_BOM_09_008_Male_NightElf_Kurtrus_Emote_Death_01.prefab:531a5704caae264439cc812be97ffd37");

	private static readonly AssetReference VO_BOM_09_008_Male_NightElf_Kurtrus_Emote_Play_01 = new AssetReference("VO_BOM_09_008_Male_NightElf_Kurtrus_Emote_Play_01.prefab:ee79880b6b493724cad19eb94d934ee5");

	private static readonly AssetReference VO_BOM_09_008_Male_NightElf_Kurtrus_InGame_HE_Custom_IfKurtrusPlayed_01_A = new AssetReference("VO_BOM_09_008_Male_NightElf_Kurtrus_InGame_HE_Custom_IfKurtrusPlayed_01_A.prefab:20be73e0782e22045826a399143ccf8e");

	private static readonly AssetReference VO_BOM_09_008_Male_Troll_Brukan_InGame_HE_Custom_IfRokaraPlayed_01_A = new AssetReference("VO_BOM_09_008_Male_Troll_Brukan_InGame_HE_Custom_IfRokaraPlayed_01_A.prefab:11cffb1e91707b946b67b126468e0281");

	private static readonly AssetReference VO_BOM_09_008_Male_Troll_Brukan_InGame_HE_Custom_IfRokaraPlayed_01_C = new AssetReference("VO_BOM_09_008_Male_Troll_Brukan_InGame_HE_Custom_IfRokaraPlayed_01_C.prefab:53b200f6228c596419856bb4b7ffad88");

	private static readonly AssetReference VO_BOM_09_008_Male_Troll_Brukan_InGame_HE_Custom_IfTamsinHalfHealth_01_A = new AssetReference("VO_BOM_09_008_Male_Troll_Brukan_InGame_HE_Custom_IfTamsinHalfHealth_01_A.prefab:61c3f137418f5f748bfbec0649126483");

	private static readonly AssetReference VO_BOM_09_008_Male_Troll_Brukan_InGame_HE_Custom_IfTamsinHalfHealth_01_C = new AssetReference("VO_BOM_09_008_Male_Troll_Brukan_InGame_HE_Custom_IfTamsinHalfHealth_01_C.prefab:04a52dbf640a78041b97fcf22a81b1b1");

	private static readonly AssetReference VO_BOM_09_008_Male_Troll_Brukan_InGame_HE_Custom_IfTamsinHalfHealthPlusOne_01_A = new AssetReference("VO_BOM_09_008_Male_Troll_Brukan_InGame_HE_Custom_IfTamsinHalfHealthPlusOne_01_A.prefab:3fd11582517c11240a635cf070b3cac4");

	private static readonly AssetReference VO_BOM_09_008_Male_Troll_Brukan_InGame_HE_Custom_IfTamsinHalfHealthPlusOne_01_C = new AssetReference("VO_BOM_09_008_Male_Troll_Brukan_InGame_HE_Custom_IfTamsinHalfHealthPlusOne_01_C.prefab:3f14edbf312aeee468bf80eefce4cc5d");

	private static readonly AssetReference VO_BOM_09_008_Male_Troll_Brukan_InGame_HE_Custom_IfTamsinTenHealth_01_B = new AssetReference("VO_BOM_09_008_Male_Troll_Brukan_InGame_HE_Custom_IfTamsinTenHealth_01_B.prefab:99d26a251acdce44ebd88c5849b201d3");

	private static readonly AssetReference VO_BOM_09_008_Male_Troll_Brukan_InGame_HE_Custom_IfTamsinTenHealth_01_B_alt = new AssetReference("VO_BOM_09_008_Male_Troll_Brukan_InGame_HE_Custom_IfTamsinTenHealth_01_B_alt.prefab:44aac68bb1808a746971ad01a3e619cd");

	private static readonly AssetReference VO_BOM_09_008_Male_Troll_Brukan_InGame_HE_Custom_IfTamsinTenHealth_01_C = new AssetReference("VO_BOM_09_008_Male_Troll_Brukan_InGame_HE_Custom_IfTamsinTenHealth_01_C.prefab:1bb98daeb71018b448f2e6eecb7bec68");

	private static readonly AssetReference VO_BOM_09_008_Male_Troll_Brukan_InGame_Introduction_01_B = new AssetReference("VO_BOM_09_008_Male_Troll_Brukan_InGame_Introduction_01_B.prefab:49063aed8278e744ba20414d9fac0541");

	private List<string> m_InGame_BossIdleLines = new List<string> { VO_BOM_09_008_Female_Undead_LichTamsin_InGame_BossIdle_01, VO_BOM_09_008_Female_Undead_LichTamsin_InGame_BossIdle_02, VO_BOM_09_008_Female_Undead_LichTamsin_InGame_BossIdle_03 };

	private List<string> m_InGame_BossUsesHeroPowerLines = new List<string> { VO_BOM_09_008_Female_Undead_LichTamsin_InGame_BossUsesHeroPower_01, VO_BOM_09_008_Female_Undead_LichTamsin_InGame_BossUsesHeroPower_02, VO_BOM_09_008_Female_Undead_LichTamsin_InGame_BossUsesHeroPower_03 };

	private HashSet<string> m_playedLines = new HashSet<string>();

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> m_SoundFiles = new List<string>
		{
			VO_BOM_09_008_Female_Draenei_Xyrella_InGame_HE_Custom_IfXyrellaPlayed_01_B, VO_BOM_09_008_Female_Human_Cariel_InGame_HE_Custom_IfCarielPlayed_01_B, VO_BOM_09_008_Female_Human_Cariel_InGame_HE_Custom_IfTamsinHalfHealthPlusOne_01_B, VO_BOM_09_008_Female_Human_Cariel_InGame_VictoryPostExplosion_01, VO_BOM_09_008_Female_Orc_Rokara_InGame_HE_Custom_IfRokaraPlayed_01_B, VO_BOM_09_008_Female_Orc_Rokara_InGame_HE_Custom_IfTamsinHalfHealth_01_B, VO_BOM_09_008_Female_Undead_LichTamsin_InGame_BossIdle_01, VO_BOM_09_008_Female_Undead_LichTamsin_InGame_BossIdle_02, VO_BOM_09_008_Female_Undead_LichTamsin_InGame_BossIdle_03, VO_BOM_09_008_Female_Undead_LichTamsin_InGame_BossUsesHeroPower_01,
			VO_BOM_09_008_Female_Undead_LichTamsin_InGame_BossUsesHeroPower_02, VO_BOM_09_008_Female_Undead_LichTamsin_InGame_BossUsesHeroPower_03, VO_BOM_09_008_Female_Undead_LichTamsin_InGame_EmoteResponse_01, VO_BOM_09_008_Female_Undead_LichTamsin_InGame_HE_Custom_IfCarielPlayed_01_A, VO_BOM_09_008_Female_Undead_LichTamsin_InGame_HE_Custom_IfCarielPlayed_01_C, VO_BOM_09_008_Female_Undead_LichTamsin_InGame_HE_Custom_IfIvusDestroyed_01, VO_BOM_09_008_Female_Undead_LichTamsin_InGame_HE_Custom_IfKurtrusPlayed_01_B, VO_BOM_09_008_Female_Undead_LichTamsin_InGame_HE_Custom_IfLokholarDestroyed_01, VO_BOM_09_008_Female_Undead_LichTamsin_InGame_HE_Custom_IfTamsinTenHealth_01_A, VO_BOM_09_008_Female_Undead_LichTamsin_InGame_HE_Custom_IfXyrellaPlayed_01_A,
			VO_BOM_09_008_Female_Undead_LichTamsin_InGame_Introduction_01_A, VO_BOM_09_008_Female_Undead_LichTamsin_InGame_PlayerLoss_01, VO_BOM_09_008_Male_NightElf_Kurtrus_InGame_HE_Custom_IfKurtrusPlayed_01_A, VO_BOM_09_008_Male_Troll_Brukan_InGame_HE_Custom_IfRokaraPlayed_01_A, VO_BOM_09_008_Male_Troll_Brukan_InGame_HE_Custom_IfRokaraPlayed_01_C, VO_BOM_09_008_Male_Troll_Brukan_InGame_HE_Custom_IfTamsinHalfHealth_01_A, VO_BOM_09_008_Male_Troll_Brukan_InGame_HE_Custom_IfTamsinHalfHealth_01_C, VO_BOM_09_008_Male_Troll_Brukan_InGame_HE_Custom_IfTamsinHalfHealthPlusOne_01_A, VO_BOM_09_008_Male_Troll_Brukan_InGame_HE_Custom_IfTamsinHalfHealthPlusOne_01_C, VO_BOM_09_008_Male_Troll_Brukan_InGame_HE_Custom_IfTamsinTenHealth_01_B,
			VO_BOM_09_008_Male_Troll_Brukan_InGame_HE_Custom_IfTamsinTenHealth_01_C, VO_BOM_09_008_Male_Troll_Brukan_InGame_Introduction_01_B
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
			yield return MissionPlayVO(enemyActor, VO_BOM_09_008_Female_Undead_LichTamsin_InGame_EmoteResponse_01);
			break;
		case 514:
			yield return MissionPlayVO(enemyActor, VO_BOM_09_008_Female_Undead_LichTamsin_InGame_Introduction_01_A);
			yield return MissionPlayVO(friendlyActor, VO_BOM_09_008_Male_Troll_Brukan_InGame_Introduction_01_B);
			break;
		case 507:
			yield return MissionPlayVO(enemyActor, VO_BOM_09_008_Female_Undead_LichTamsin_InGame_PlayerLoss_01);
			break;
		case 505:
			MissionPause(pause: true);
			yield return MissionPlayVO("BOM_09_Cariel_008t", BOM_09_Brukan_Dungeon.Alterac_CarielArt_BrassRing_Quote, VO_BOM_09_008_Female_Human_Cariel_InGame_VictoryPostExplosion_01);
			MissionPause(pause: false);
			break;
		case 100:
			MissionPause(pause: true);
			yield return MissionPlayVO(friendlyActor, VO_BOM_09_008_Male_Troll_Brukan_InGame_HE_Custom_IfTamsinHalfHealth_01_A);
			yield return MissionPlayVO("BOM_09_Rokara_008t", BOM_09_Brukan_Dungeon.RokaraTier5_BrassRing_Quote, VO_BOM_09_008_Female_Orc_Rokara_InGame_HE_Custom_IfTamsinHalfHealth_01_B);
			yield return MissionPlayVO(friendlyActor, VO_BOM_09_008_Male_Troll_Brukan_InGame_HE_Custom_IfTamsinHalfHealth_01_C);
			MissionPause(pause: false);
			break;
		case 101:
			yield return MissionPlayVO(friendlyActor, VO_BOM_09_008_Male_Troll_Brukan_InGame_HE_Custom_IfTamsinHalfHealthPlusOne_01_A);
			yield return MissionPlayVO("BOM_09_Cariel_008t", BOM_09_Brukan_Dungeon.Alterac_CarielArt_BrassRing_Quote, VO_BOM_09_008_Female_Human_Cariel_InGame_HE_Custom_IfTamsinHalfHealthPlusOne_01_B);
			yield return MissionPlayVO(friendlyActor, VO_BOM_09_008_Male_Troll_Brukan_InGame_HE_Custom_IfTamsinHalfHealthPlusOne_01_C);
			yield return MissionPlayVO(enemyActor, VO_BOM_09_008_Female_Undead_LichTamsin_InGame_HE_Custom_IfTamsinTenHealth_01_A);
			yield return MissionPlayVO(friendlyActor, VO_BOM_09_008_Male_Troll_Brukan_InGame_HE_Custom_IfTamsinTenHealth_01_B);
			yield return MissionPlayVO(friendlyActor, VO_BOM_09_008_Male_Troll_Brukan_InGame_HE_Custom_IfTamsinTenHealth_01_C);
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
			Actor enemyActor = GameState.Get().GetOpposingSidePlayer().GetHero()
				.GetCard()
				.GetActor();
			Actor friendlyActor = GameState.Get().GetFriendlySidePlayer().GetHero()
				.GetCard()
				.GetActor();
			switch (cardID)
			{
			case "BOM_09_Kurtrus_008t":
				yield return MissionPlayVOOnce("BOM_09_Kurtrus_008t", VO_BOM_09_008_Male_NightElf_Kurtrus_InGame_HE_Custom_IfKurtrusPlayed_01_A);
				yield return MissionPlayVOOnce(enemyActor, VO_BOM_09_008_Female_Undead_LichTamsin_InGame_HE_Custom_IfKurtrusPlayed_01_B);
				break;
			case "BOM_09_Xyrella_008t":
				yield return MissionPlayVOOnce(enemyActor, VO_BOM_09_008_Female_Undead_LichTamsin_InGame_HE_Custom_IfXyrellaPlayed_01_A);
				yield return MissionPlayVOOnce("BOM_09_Xyrella_008t", VO_BOM_09_008_Female_Draenei_Xyrella_InGame_HE_Custom_IfXyrellaPlayed_01_B);
				break;
			case "BOM_09_Cariel_008t":
				yield return MissionPlayVOOnce(enemyActor, VO_BOM_09_008_Female_Undead_LichTamsin_InGame_HE_Custom_IfCarielPlayed_01_A);
				yield return MissionPlayVOOnce("BOM_09_Cariel_008t", VO_BOM_09_008_Female_Human_Cariel_InGame_HE_Custom_IfCarielPlayed_01_B);
				yield return MissionPlayVOOnce(enemyActor, VO_BOM_09_008_Female_Undead_LichTamsin_InGame_HE_Custom_IfCarielPlayed_01_C);
				break;
			case "BOM_09_Rokara_008t":
				yield return MissionPlayVOOnce(friendlyActor, VO_BOM_09_008_Male_Troll_Brukan_InGame_HE_Custom_IfRokaraPlayed_01_A);
				yield return MissionPlayVOOnce("BOM_09_Rokara_008t", VO_BOM_09_008_Female_Orc_Rokara_InGame_HE_Custom_IfRokaraPlayed_01_B);
				yield return MissionPlayVOOnce(friendlyActor, VO_BOM_09_008_Male_Troll_Brukan_InGame_HE_Custom_IfRokaraPlayed_01_C);
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
		GameState.Get().GetOpposingSidePlayer().GetHero()
			.GetCard()
			.GetActor();
		GameState.Get().GetFriendlySidePlayer().GetHero()
			.GetCard()
			.GetActor();
	}
}
