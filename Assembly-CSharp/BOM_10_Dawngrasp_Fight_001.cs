using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BOM_10_Dawngrasp_Fight_001 : BOM_10_Dawngrasp_Dungeon
{
	private static readonly AssetReference VO_BOM_10_001_Female_Human_Cariel_InGame_Turn_03_01_A = new AssetReference("VO_BOM_10_001_Female_Human_Cariel_InGame_Turn_03_01_A.prefab:79694483b399db54f94d6da52ba68d24");

	private static readonly AssetReference VO_BOM_10_001_Female_Human_Cariel_InGame_Turn_11_01_B = new AssetReference("VO_BOM_10_001_Female_Human_Cariel_InGame_Turn_11_01_B.prefab:9b060edf3b2dc8b489e48187b661235a");

	private static readonly AssetReference VO_BOM_10_001_Female_Human_Cariel_InGame_Turn_11_01_D = new AssetReference("VO_BOM_10_001_Female_Human_Cariel_InGame_Turn_11_01_D.prefab:b64ddd0653fe84c4cb4318f22299b02d");

	private static readonly AssetReference VO_BOM_10_001_Female_Human_Cariel_InGame_Turn_15_01_B = new AssetReference("VO_BOM_10_001_Female_Human_Cariel_InGame_Turn_15_01_B.prefab:60bef7ba651980e40bd7d5bea3d71c6f");

	private static readonly AssetReference VO_BOM_10_001_Female_Orc_Rokara_InGame_Introduction_01_B = new AssetReference("VO_BOM_10_001_Female_Orc_Rokara_InGame_Introduction_01_B.prefab:47f8823bf3028af4882f1e09a1a1e236");

	private static readonly AssetReference VO_BOM_10_001_Female_Orc_Rokara_InGame_Turn_03_01_B = new AssetReference("VO_BOM_10_001_Female_Orc_Rokara_InGame_Turn_03_01_B.prefab:2aee131f421754a4a8b20d91e01f1554");

	private static readonly AssetReference VO_BOM_10_001_Female_Orc_Rokara_InGame_Turn_09_01_A = new AssetReference("VO_BOM_10_001_Female_Orc_Rokara_InGame_Turn_09_01_A.prefab:5875efccec7928e4b8ff2bf63f3be0ad");

	private static readonly AssetReference VO_BOM_10_001_Female_Orc_Rokara_InGame_Turn_13_01_A = new AssetReference("VO_BOM_10_001_Female_Orc_Rokara_InGame_Turn_13_01_A.prefab:82e5150b595af4b4a81542ef48fa475a");

	private static readonly AssetReference VO_BOM_10_001_Male_Dwarf_Tavish_InGame_Turn_07_01 = new AssetReference("VO_BOM_10_001_Male_Dwarf_Tavish_InGame_Turn_07_01.prefab:1c6cf981366278b469803cb2d21410e5");

	private static readonly AssetReference VO_BOM_10_001_Male_Gnome_Scabbs_InGame_Turn_01_C = new AssetReference("VO_BOM_10_001_Male_Gnome_Scabbs_InGame_Turn_01_C.prefab:2db7cb23a2e5e5e4486ff022ae3cee5c");

	private static readonly AssetReference VO_BOM_10_001_Male_Gnome_Scabbs_InGame_Turn_13_01_B = new AssetReference("VO_BOM_10_001_Male_Gnome_Scabbs_InGame_Turn_13_01_B.prefab:b119c49c84bfc2a469eae1a1654c21e4");

	private static readonly AssetReference VO_BOM_10_001_Male_NightElf_Kurtrus_InGame_Turn_11_01_A = new AssetReference("VO_BOM_10_001_Male_NightElf_Kurtrus_InGame_Turn_11_01_A.prefab:a69b736b0f18a1041a4b3cc0e033f7c9");

	private static readonly AssetReference VO_BOM_10_001_Male_NightElf_Kurtrus_InGame_Turn_11_01_C = new AssetReference("VO_BOM_10_001_Male_NightElf_Kurtrus_InGame_Turn_11_01_C.prefab:bc2c4c8a91525e94bbd73c8f4c550ccd");

	private static readonly AssetReference VO_BOM_10_001_Male_NightElf_Kurtrus_InGame_Turn_11_01_E = new AssetReference("VO_BOM_10_001_Male_NightElf_Kurtrus_InGame_Turn_11_01_E.prefab:a741dd4d91bf9a14c8ecbfeb58d457f8");

	private static readonly AssetReference VO_BOM_10_001_Male_Tauren_Guff_InGame_Turn_05_01_A = new AssetReference("VO_BOM_10_001_Male_Tauren_Guff_InGame_Turn_05_01_A.prefab:abae1346aee74874a855b9e648ba8f36");

	private static readonly AssetReference VO_BOM_10_001_Male_Tauren_Guff_InGame_Turn_05_01_B = new AssetReference("VO_BOM_10_001_Male_Tauren_Guff_InGame_Turn_05_01_B.prefab:d6182998d5ae34c4fb2f352549856263");

	private static readonly AssetReference VO_BOM_10_001_X_BloodElf_Dawngrasp_InGame_BossIdle_01 = new AssetReference("VO_BOM_10_001_X_BloodElf_Dawngrasp_InGame_BossIdle_01.prefab:e4e4a355071d58641ad2c059be09615e");

	private static readonly AssetReference VO_BOM_10_001_X_BloodElf_Dawngrasp_InGame_BossIdle_02 = new AssetReference("VO_BOM_10_001_X_BloodElf_Dawngrasp_InGame_BossIdle_02.prefab:ee395c4af604a8248ae190c3696b5965");

	private static readonly AssetReference VO_BOM_10_001_X_BloodElf_Dawngrasp_InGame_BossIdle_03 = new AssetReference("VO_BOM_10_001_X_BloodElf_Dawngrasp_InGame_BossIdle_03.prefab:3447085fc8d04224b8925833c656c8ef");

	private static readonly AssetReference VO_BOM_10_001_X_BloodElf_Dawngrasp_InGame_Introduction_01_A = new AssetReference("VO_BOM_10_001_X_BloodElf_Dawngrasp_InGame_Introduction_01_A.prefab:0b1dfeca15d4fec4ea1c926332405a8e");

	private static readonly AssetReference VO_BOM_10_001_X_BloodElf_Dawngrasp_InGame_PlayerLoss_01 = new AssetReference("VO_BOM_10_001_X_BloodElf_Dawngrasp_InGame_PlayerLoss_01.prefab:7eea540d31fd1144e8f809a76d38b344");

	private static readonly AssetReference VO_BOM_10_001_X_BloodElf_Dawngrasp_InGame_Turn_01_01 = new AssetReference("VO_BOM_10_001_X_BloodElf_Dawngrasp_InGame_Turn_01_01.prefab:19302b365e9e5354b9e597a7da0a8c09");

	private static readonly AssetReference VO_BOM_10_001_X_BloodElf_Dawngrasp_InGame_Turn_03_01_C = new AssetReference("VO_BOM_10_001_X_BloodElf_Dawngrasp_InGame_Turn_03_01_C.prefab:b574b4b2ef00f844c9a8273c0a4224e9");

	private static readonly AssetReference VO_BOM_10_001_X_BloodElf_Dawngrasp_InGame_Turn_09_01_B = new AssetReference("VO_BOM_10_001_X_BloodElf_Dawngrasp_InGame_Turn_09_01_B.prefab:ffdf4abe531de1d458954d8860bd5782");

	private static readonly AssetReference VO_BOM_10_001_X_BloodElf_Dawngrasp_InGame_Turn_15_01_C = new AssetReference("VO_BOM_10_001_X_BloodElf_Dawngrasp_InGame_Turn_15_01_C.prefab:a8eabea98c1fe00409548a241dce9780");

	private static readonly AssetReference VO_BOM_10_001_X_BloodElf_Dawngrasp_InGame_Turn_17_01_A = new AssetReference("VO_BOM_10_001_X_BloodElf_Dawngrasp_InGame_Turn_17_01_A.prefab:9d2a364a299317f4797da7876c5aeb71");

	private static readonly AssetReference VO_BOM_10_001_X_BloodElf_Dawngrasp_InGame_Turn_17_01_B = new AssetReference("VO_BOM_10_001_X_BloodElf_Dawngrasp_InGame_Turn_17_01_B.prefab:f22db9b9bde04fd4099e29a4247eabb4");

	private static readonly AssetReference VO_BOM_10_001_X_BloodElf_Dawngrasp_InGame_Turn_17_01_C = new AssetReference("VO_BOM_10_001_X_BloodElf_Dawngrasp_InGame_Turn_17_01_C.prefab:cd12d6746e9371b4c9ce28a2f4ff00df");

	private static readonly AssetReference VO_BOM_10_001_X_BloodElf_Dawngrasp_InGame_VictoryPreExplosion_01_A = new AssetReference("VO_BOM_10_001_X_BloodElf_Dawngrasp_InGame_VictoryPreExplosion_01_A.prefab:a2dd3d2bd60f1e749a04cf5d5020d355");

	private static readonly AssetReference VO_BOM_10_001_X_BloodElf_Dawngrasp_InGame_VictoryPreExplosion_01_B = new AssetReference("VO_BOM_10_001_X_BloodElf_Dawngrasp_InGame_VictoryPreExplosion_01_B.prefab:8e71eee564328a446bc265967420e60a");

	private static readonly AssetReference VO_BOM_10_001_X_BloodElf_Dawngrasp_InGame_VictoryPreExplosion_01_C = new AssetReference("VO_BOM_10_001_X_BloodElf_Dawngrasp_InGame_VictoryPreExplosion_01_C.prefab:7b253764edafdee4f8fd4b829b89da0a");

	private static readonly AssetReference VO_BOM_10_001_X_BloodElf_Dawngrasp_UI_AWD_Boss_Reveal_General_01_01_A = new AssetReference("VO_BOM_10_001_X_BloodElf_Dawngrasp_UI_AWD_Boss_Reveal_General_01_01_A.prefab:e7eb4107e8693e54a8cce5bf0add0749");

	private static readonly AssetReference VO_BOM_10_001_X_BloodElf_Dawngrasp_UI_AWD_Boss_Reveal_General_01_01_B = new AssetReference("VO_BOM_10_001_X_BloodElf_Dawngrasp_UI_AWD_Boss_Reveal_General_01_01_B.prefab:169204e8f4a3048429ab244db09a9d6b");

	private static readonly AssetReference VO_BOM_10_001_Female_Draenei_Xyrella_InGame_Turn_15_01_A = new AssetReference("VO_BOM_10_001_Female_Draenei_Xyrella_InGame_Turn_15_01_A.prefab:55f4df5b0c618ab42b1068c7247b0a49");

	private List<string> m_InGame_BossIdleLines = new List<string> { VO_BOM_10_001_X_BloodElf_Dawngrasp_InGame_BossIdle_01, VO_BOM_10_001_X_BloodElf_Dawngrasp_InGame_BossIdle_02, VO_BOM_10_001_X_BloodElf_Dawngrasp_InGame_BossIdle_03 };

	private HashSet<string> m_playedLines = new HashSet<string>();

	private Notification m_turnCounter;

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> m_SoundFiles = new List<string>
		{
			VO_BOM_10_001_Female_Human_Cariel_InGame_Turn_11_01_B, VO_BOM_10_001_Female_Human_Cariel_InGame_Turn_11_01_D, VO_BOM_10_001_Female_Human_Cariel_InGame_Turn_15_01_B, VO_BOM_10_001_Female_Orc_Rokara_InGame_Introduction_01_B, VO_BOM_10_001_Female_Orc_Rokara_InGame_Turn_09_01_A, VO_BOM_10_001_Female_Orc_Rokara_InGame_Turn_13_01_A, VO_BOM_10_001_Male_Dwarf_Tavish_InGame_Turn_07_01, VO_BOM_10_001_Male_Gnome_Scabbs_InGame_Turn_01_C, VO_BOM_10_001_Male_Gnome_Scabbs_InGame_Turn_13_01_B, VO_BOM_10_001_Male_NightElf_Kurtrus_InGame_Turn_11_01_A,
			VO_BOM_10_001_Male_NightElf_Kurtrus_InGame_Turn_11_01_C, VO_BOM_10_001_Male_NightElf_Kurtrus_InGame_Turn_11_01_E, VO_BOM_10_001_Male_Tauren_Guff_InGame_Turn_05_01_A, VO_BOM_10_001_Male_Tauren_Guff_InGame_Turn_05_01_B, VO_BOM_10_001_X_BloodElf_Dawngrasp_InGame_BossIdle_01, VO_BOM_10_001_X_BloodElf_Dawngrasp_InGame_BossIdle_02, VO_BOM_10_001_X_BloodElf_Dawngrasp_InGame_BossIdle_03, VO_BOM_10_001_X_BloodElf_Dawngrasp_InGame_Introduction_01_A, VO_BOM_10_001_X_BloodElf_Dawngrasp_InGame_PlayerLoss_01, VO_BOM_10_001_X_BloodElf_Dawngrasp_InGame_Turn_01_01,
			VO_BOM_10_001_Female_Human_Cariel_InGame_Turn_03_01_A, VO_BOM_10_001_Female_Orc_Rokara_InGame_Turn_03_01_B, VO_BOM_10_001_X_BloodElf_Dawngrasp_InGame_Turn_03_01_C, VO_BOM_10_001_X_BloodElf_Dawngrasp_InGame_Turn_09_01_B, VO_BOM_10_001_X_BloodElf_Dawngrasp_InGame_Turn_15_01_C, VO_BOM_10_001_X_BloodElf_Dawngrasp_InGame_Turn_17_01_A, VO_BOM_10_001_X_BloodElf_Dawngrasp_InGame_Turn_17_01_B, VO_BOM_10_001_X_BloodElf_Dawngrasp_InGame_Turn_17_01_C, VO_BOM_10_001_X_BloodElf_Dawngrasp_InGame_VictoryPreExplosion_01_A, VO_BOM_10_001_X_BloodElf_Dawngrasp_InGame_VictoryPreExplosion_01_B,
			VO_BOM_10_001_X_BloodElf_Dawngrasp_InGame_VictoryPreExplosion_01_C, VO_BOM_10_001_X_BloodElf_Dawngrasp_UI_AWD_Boss_Reveal_General_01_01_A, VO_BOM_10_001_X_BloodElf_Dawngrasp_UI_AWD_Boss_Reveal_General_01_01_B, VO_BOM_10_001_Female_Draenei_Xyrella_InGame_Turn_15_01_A
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
		GameState.Get().GetOpposingSidePlayer().GetHero()
			.GetCard()
			.GetActor();
		Actor friendlyActor = GameState.Get().GetFriendlySidePlayer().GetHero()
			.GetCard()
			.GetActor();
		switch (missionEvent)
		{
		case 517:
			yield return MissionPlayVO(friendlyActor, m_InGame_BossIdleLines);
			break;
		case 514:
			yield return MissionPlayVO(friendlyActor, VO_BOM_10_001_X_BloodElf_Dawngrasp_InGame_Introduction_01_A);
			yield return MissionPlayVO("BOM_10_Rokara_001t", BOM_10_Dawngrasp_Dungeon.RokaraTier5_BrassRing_Quote, VO_BOM_10_001_Female_Orc_Rokara_InGame_Introduction_01_B);
			break;
		case 506:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(friendlyActor, VO_BOM_10_001_X_BloodElf_Dawngrasp_InGame_PlayerLoss_01);
			GameState.Get().SetBusy(busy: false);
			break;
		case 504:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(friendlyActor, VO_BOM_10_001_X_BloodElf_Dawngrasp_InGame_VictoryPreExplosion_01_A);
			yield return MissionPlayVO(friendlyActor, VO_BOM_10_001_X_BloodElf_Dawngrasp_InGame_VictoryPreExplosion_01_B);
			yield return MissionPlayVO(friendlyActor, VO_BOM_10_001_X_BloodElf_Dawngrasp_InGame_VictoryPreExplosion_01_C);
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
		GameState.Get().GetOpposingSidePlayer().GetHero()
			.GetCard()
			.GetActor();
		Actor friendlyActor = GameState.Get().GetFriendlySidePlayer().GetHero()
			.GetCard()
			.GetActor();
		switch (turn)
		{
		case 1:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(friendlyActor, VO_BOM_10_001_X_BloodElf_Dawngrasp_InGame_Turn_01_01);
			GameState.Get().SetBusy(busy: false);
			break;
		case 3:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO("BOM_10_Cariel_004t", BOM_10_Dawngrasp_Dungeon.Alterac_CarielArt_BrassRing_Quote, VO_BOM_10_001_Female_Human_Cariel_InGame_Turn_03_01_A);
			yield return MissionPlayVO("BOM_10_Rokara_001t", BOM_10_Dawngrasp_Dungeon.RokaraTier5_BrassRing_Quote, VO_BOM_10_001_Female_Orc_Rokara_InGame_Turn_03_01_B);
			yield return MissionPlayVO(friendlyActor, VO_BOM_10_001_X_BloodElf_Dawngrasp_InGame_Turn_03_01_C);
			GameState.Get().SetBusy(busy: false);
			break;
		case 5:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO("BOM_10_Guff_005t", BOM_10_Dawngrasp_Dungeon.Guff_BrassRing_Quote, VO_BOM_10_001_Male_Tauren_Guff_InGame_Turn_05_01_A);
			yield return MissionPlayVO("BOM_10_Guff_005t", BOM_10_Dawngrasp_Dungeon.Guff_BrassRing_Quote, VO_BOM_10_001_Male_Tauren_Guff_InGame_Turn_05_01_B);
			GameState.Get().SetBusy(busy: false);
			break;
		case 7:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO("BOM_10_Tavish_001t", BOM_10_Dawngrasp_Dungeon.Alterac_TavishArt_BrassRing_Quote, VO_BOM_10_001_Male_Dwarf_Tavish_InGame_Turn_07_01);
			GameState.Get().SetBusy(busy: false);
			break;
		case 9:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO("BOM_10_Rokara_001t", BOM_10_Dawngrasp_Dungeon.RokaraTier5_BrassRing_Quote, VO_BOM_10_001_Female_Orc_Rokara_InGame_Turn_09_01_A);
			yield return MissionPlayVO(friendlyActor, VO_BOM_10_001_X_BloodElf_Dawngrasp_InGame_Turn_09_01_B);
			GameState.Get().SetBusy(busy: false);
			break;
		case 11:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO("BOM_10_Kurtrus_001t", BOM_10_Dawngrasp_Dungeon.KurtrusTier5_BrassRing_Quote, VO_BOM_10_001_Male_NightElf_Kurtrus_InGame_Turn_11_01_A);
			yield return MissionPlayVO("BOM_10_Cariel_004t", BOM_10_Dawngrasp_Dungeon.Alterac_CarielArt_BrassRing_Quote, VO_BOM_10_001_Female_Human_Cariel_InGame_Turn_11_01_B);
			yield return MissionPlayVO("BOM_10_Kurtrus_001t", BOM_10_Dawngrasp_Dungeon.KurtrusTier5_BrassRing_Quote, VO_BOM_10_001_Male_NightElf_Kurtrus_InGame_Turn_11_01_C);
			yield return MissionPlayVO("BOM_10_Cariel_004t", BOM_10_Dawngrasp_Dungeon.Alterac_CarielArt_BrassRing_Quote, VO_BOM_10_001_Female_Human_Cariel_InGame_Turn_11_01_D);
			yield return MissionPlayVO("BOM_10_Kurtrus_001t", BOM_10_Dawngrasp_Dungeon.KurtrusTier5_BrassRing_Quote, VO_BOM_10_001_Male_NightElf_Kurtrus_InGame_Turn_11_01_E);
			GameState.Get().SetBusy(busy: false);
			break;
		case 13:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO("BOM_10_Rokara_001t", BOM_10_Dawngrasp_Dungeon.RokaraTier5_BrassRing_Quote, VO_BOM_10_001_Female_Orc_Rokara_InGame_Turn_13_01_A);
			yield return MissionPlayVO("BOM_10_Scabbs_001t", BOM_10_Dawngrasp_Dungeon.Alterac_ScabbsArt_BrassRing_Quote, VO_BOM_10_001_Male_Gnome_Scabbs_InGame_Turn_13_01_B);
			yield return MissionPlayVO("BOM_10_Scabbs_001t", BOM_10_Dawngrasp_Dungeon.Alterac_ScabbsArt_BrassRing_Quote, VO_BOM_10_001_Male_Gnome_Scabbs_InGame_Turn_01_C);
			GameState.Get().SetBusy(busy: false);
			break;
		case 15:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO("BOM_10_Xyrella_004t", BOM_10_Dawngrasp_Dungeon.Alterac_XyrellaArt_BrassRing_Quote, VO_BOM_10_001_Female_Draenei_Xyrella_InGame_Turn_15_01_A);
			yield return MissionPlayVO("BOM_10_Cariel_004t", BOM_10_Dawngrasp_Dungeon.Alterac_CarielArt_BrassRing_Quote, VO_BOM_10_001_Female_Human_Cariel_InGame_Turn_15_01_B);
			yield return MissionPlayVO(friendlyActor, VO_BOM_10_001_X_BloodElf_Dawngrasp_InGame_Turn_15_01_C);
			GameState.Get().SetBusy(busy: false);
			break;
		case 17:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(friendlyActor, VO_BOM_10_001_X_BloodElf_Dawngrasp_InGame_Turn_17_01_A);
			yield return MissionPlayVO(friendlyActor, VO_BOM_10_001_X_BloodElf_Dawngrasp_InGame_Turn_17_01_B);
			yield return MissionPlayVO(friendlyActor, VO_BOM_10_001_X_BloodElf_Dawngrasp_InGame_Turn_17_01_C);
			GameState.Get().SetBusy(busy: false);
			break;
		}
	}

	public override void NotifyOfMulliganEnded()
	{
		base.NotifyOfMulliganEnded();
		InitVisuals();
	}

	private void InitVisuals()
	{
		int cost = GetCost();
		InitTurnCounter(cost);
	}

	public override void OnTagChanged(TagDelta change)
	{
		base.OnTagChanged(change);
		if (change.tag == 48 && change.newValue != change.oldValue)
		{
			UpdateVisuals(change.newValue);
		}
	}

	private void InitTurnCounter(int cost)
	{
		GameObject turnCounterGo = AssetLoader.Get().InstantiatePrefab("LOE_Turn_Timer.prefab:b05530aa55868554fb8f0c66632b3c22");
		m_turnCounter = turnCounterGo.GetComponent<Notification>();
		PlayMakerFSM component = m_turnCounter.GetComponent<PlayMakerFSM>();
		component.FsmVariables.GetFsmBool("RunningMan").Value = true;
		component.FsmVariables.GetFsmBool("MineCart").Value = false;
		component.FsmVariables.GetFsmBool("Airship").Value = false;
		component.FsmVariables.GetFsmBool("Destroyer").Value = false;
		component.SendEvent("Birth");
		Actor enemyActor = GameState.Get().GetOpposingSidePlayer().GetHeroCard()
			.GetActor();
		m_turnCounter.transform.parent = enemyActor.gameObject.transform;
		m_turnCounter.transform.localPosition = new Vector3(-1.4f, 0.187f, -0.11f);
		m_turnCounter.transform.localScale = Vector3.one * 0.52f;
		UpdateTurnCounterText(cost);
	}

	private void UpdateVisuals(int cost)
	{
		UpdateTurnCounter(cost);
	}

	private void UpdateTurnCounter(int cost)
	{
		m_turnCounter.GetComponent<PlayMakerFSM>().SendEvent("Action");
		if (cost <= 0)
		{
			Object.Destroy(m_turnCounter.gameObject);
		}
		else
		{
			UpdateTurnCounterText(cost);
		}
	}

	private void UpdateTurnCounterText(int cost)
	{
		GameStrings.PluralNumber[] pluralNumbers = new GameStrings.PluralNumber[1]
		{
			new GameStrings.PluralNumber
			{
				m_index = 0,
				m_number = cost
			}
		};
		string counterName = GameStrings.FormatPlurals("BOM_DAWNGRASP_01", pluralNumbers);
		m_turnCounter.ChangeDialogText(counterName, cost.ToString(), "", "");
	}
}
