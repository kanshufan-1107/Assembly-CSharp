using System.Collections;
using System.Collections.Generic;

public class BOM_08_Tavish_Fight_004 : BOM_08_Tavish_Dungeon
{
	private static readonly AssetReference VO_BOM_08_004_Female_Draenei_Xyrella_InGame_HE_Custom_IfXyrellaFail_01 = new AssetReference("VO_BOM_08_004_Female_Draenei_Xyrella_InGame_HE_Custom_IfXyrellaFail_01.prefab:08a4056e8baaa474e9be07f1b8754372");

	private static readonly AssetReference VO_BOM_08_004_Female_Draenei_Xyrella_InGame_HE_Custom_IfXyrellaSucceed_01_A = new AssetReference("VO_BOM_08_004_Female_Draenei_Xyrella_InGame_HE_Custom_IfXyrellaSucceed_01_A.prefab:0245b7bbc9f66874d893797ecd8b41b5");

	private static readonly AssetReference VO_BOM_08_004_Female_Draenei_Xyrella_InGame_HE_Custom_IfXyrellaSucceed_01_B = new AssetReference("VO_BOM_08_004_Female_Draenei_Xyrella_InGame_HE_Custom_IfXyrellaSucceed_01_B.prefab:938931ede96b829449f8d3717762229e");

	private static readonly AssetReference VO_BOM_08_004_Female_Draenei_Xyrella_InGame_HE_Custom_IfXyrellaSucceed_01_D = new AssetReference("VO_BOM_08_004_Female_Draenei_Xyrella_InGame_HE_Custom_IfXyrellaSucceed_01_D.prefab:03b5a87b43c1c354cade6b8faa52cf46");

	private static readonly AssetReference VO_BOM_08_004_Female_Draenei_Xyrella_InGame_Turn_07_01_B = new AssetReference("VO_BOM_08_004_Female_Draenei_Xyrella_InGame_Turn_07_01_B.prefab:1658d1a9b19c58747a0ca54090fcaa0c");

	private static readonly AssetReference VO_BOM_08_004_Male_Dwarf_Tavish_InGame_HE_Custom_IfCarielSucceed_01_B = new AssetReference("VO_BOM_08_004_Male_Dwarf_Tavish_InGame_HE_Custom_IfCarielSucceed_01_B.prefab:71484b067c8037546ac59e2bff9c6250");

	private static readonly AssetReference VO_BOM_08_004_Male_Dwarf_Tavish_InGame_HE_Custom_IfKurtrusSucceed_01_B = new AssetReference("VO_BOM_08_004_Male_Dwarf_Tavish_InGame_HE_Custom_IfKurtrusSucceed_01_B.prefab:966dd57fe7804de4fafc7edd33194343");

	private static readonly AssetReference VO_BOM_08_004_Male_Dwarf_Tavish_InGame_HE_Custom_IfScabbsSuccess_01_C = new AssetReference("VO_BOM_08_004_Male_Dwarf_Tavish_InGame_HE_Custom_IfScabbsSuccess_01_C.prefab:50c5014e5e6b9734cb03b4d20f56dbb9");

	private static readonly AssetReference VO_BOM_08_004_Male_Dwarf_Tavish_InGame_HE_Custom_IfXyrellaSucceed_01_C = new AssetReference("VO_BOM_08_004_Male_Dwarf_Tavish_InGame_HE_Custom_IfXyrellaSucceed_01_C.prefab:823fd1a539d1d1f4ab034cc87736b76a");

	private static readonly AssetReference VO_BOM_08_004_Male_Dwarf_Tavish_InGame_Introduction_01 = new AssetReference("VO_BOM_08_004_Male_Dwarf_Tavish_InGame_Introduction_01.prefab:d263543a75fe65b4fa1acebc563cb7c8");

	private static readonly AssetReference VO_BOM_08_004_Male_Dwarf_Tavish_InGame_Turn_01_01_B = new AssetReference("VO_BOM_08_004_Male_Dwarf_Tavish_InGame_Turn_01_01_B.prefab:094e8f56ccc206b41913a4819180f6a1");

	private static readonly AssetReference VO_BOM_08_004_Male_Dwarf_Tavish_InGame_Turn_03_01_A = new AssetReference("VO_BOM_08_004_Male_Dwarf_Tavish_InGame_Turn_03_01_A.prefab:ccb165cde247f2a4cab2786e07d7ae22");

	private static readonly AssetReference VO_BOM_08_004_Male_Dwarf_Tavish_InGame_Turn_03_01_C = new AssetReference("VO_BOM_08_004_Male_Dwarf_Tavish_InGame_Turn_03_01_C.prefab:8146d1a57af77c84b9d515ba2699264d");

	private static readonly AssetReference VO_BOM_08_004_Male_Dwarf_Tavish_InGame_Turn_05_01_A = new AssetReference("VO_BOM_08_004_Male_Dwarf_Tavish_InGame_Turn_05_01_A.prefab:286cdd5577838d147a96b513e0084349");

	private static readonly AssetReference VO_BOM_08_004_Male_Dwarf_Tavish_InGame_Turn_07_01_A = new AssetReference("VO_BOM_08_004_Male_Dwarf_Tavish_InGame_Turn_07_01_A.prefab:0c0ac3aa3215ac742a8a80166ce6f372");

	private static readonly AssetReference VO_BOM_08_004_Male_Dwarf_Tavish_InGame_Victory_PreExplosion_01_A = new AssetReference("VO_BOM_08_004_Male_Dwarf_Tavish_InGame_Victory_PreExplosion_01_A.prefab:ef7c8fb8c9d8c1d43a478889c7e44d6e");

	private static readonly AssetReference VO_BOM_08_004_Male_Dwarf_Tavish_UI_AWD_Boss_Reveal_General_04_01_C = new AssetReference("VO_BOM_08_004_Male_Dwarf_Tavish_UI_AWD_Boss_Reveal_General_04_01_C.prefab:1522732604bec1646b1a0d827a64e022");

	private static readonly AssetReference VO_BOM_08_004_Male_Gnome_Scabbs_InGame_HE_Custom_IfScabbsFail_01 = new AssetReference("VO_BOM_08_004_Male_Gnome_Scabbs_InGame_HE_Custom_IfScabbsFail_01.prefab:2604e5a74ecd87a40bc31ab5ceb2f501");

	private static readonly AssetReference VO_BOM_08_004_Male_Gnome_Scabbs_InGame_HE_Custom_IfScabbsSuccess_01_A = new AssetReference("VO_BOM_08_004_Male_Gnome_Scabbs_InGame_HE_Custom_IfScabbsSuccess_01_A.prefab:a49a9809735e8a943a0f196e34aeb415");

	private static readonly AssetReference VO_BOM_08_004_Male_Gnome_Scabbs_InGame_HE_Custom_IfScabbsSuccess_01_B = new AssetReference("VO_BOM_08_004_Male_Gnome_Scabbs_InGame_HE_Custom_IfScabbsSuccess_01_B.prefab:ec61ee143b63b0048860de25e95ab087");

	private static readonly AssetReference VO_BOM_08_004_Male_Gnome_Scabbs_InGame_Turn_01_01_A = new AssetReference("VO_BOM_08_004_Male_Gnome_Scabbs_InGame_Turn_01_01_A.prefab:208fa34815040de48a6863976aa68cff");

	private static readonly AssetReference VO_BOM_08_004_Male_Gnome_Scabbs_InGame_Turn_01_01_C = new AssetReference("VO_BOM_08_004_Male_Gnome_Scabbs_InGame_Turn_01_01_C.prefab:eb1ba3f6f57fdef46bf79278d9d28240");

	private static readonly AssetReference VO_BOM_08_004_Female_Human_Cariel_InGame_HE_Custom_IfCarielFail_01 = new AssetReference("VO_BOM_08_004_Female_Human_Cariel_InGame_HE_Custom_IfCarielFail_01.prefab:37ce21c7d2be5a64d979cfe88c91c6cc");

	private static readonly AssetReference VO_BOM_08_004_Female_Human_Cariel_InGame_HE_Custom_IfCarielSucceed_01_A = new AssetReference("VO_BOM_08_004_Female_Human_Cariel_InGame_HE_Custom_IfCarielSucceed_01_A.prefab:9b076a4640cbcb94daff0868b89f73b7");

	private static readonly AssetReference VO_BOM_08_004_Female_Human_Cariel_InGame_HE_Custom_IfCarielSucceed_01_C = new AssetReference("VO_BOM_08_004_Female_Human_Cariel_InGame_HE_Custom_IfCarielSucceed_01_C.prefab:fcb7c8372aed8154094247ee39d6e8c3");

	private static readonly AssetReference VO_BOM_08_004_Female_Human_Cariel_InGame_HE_Custom_IfCarielSucceed_01_D = new AssetReference("VO_BOM_08_004_Female_Human_Cariel_InGame_HE_Custom_IfCarielSucceed_01_D.prefab:59d294e8c07f516419f5718fd4f41411");

	private static readonly AssetReference VO_BOM_08_004_Female_Human_Cariel_InGame_Turn_03_01_B = new AssetReference("VO_BOM_08_004_Female_Human_Cariel_InGame_Turn_03_01_B.prefab:b351f54087740ed4fa4de4c708195e90");

	private static readonly AssetReference VO_BOM_08_004_Male_NightElf_Kurtrus_InGame_HE_Custom_IfKurtrusFail_01 = new AssetReference("VO_BOM_08_004_Male_NightElf_Kurtrus_InGame_HE_Custom_IfKurtrusFail_01.prefab:9a8bfcb1d8365f34e8aa8b99dcb4477c");

	private static readonly AssetReference VO_BOM_08_004_Male_NightElf_Kurtrus_InGame_HE_Custom_IfKurtrusSucceed_01_A = new AssetReference("VO_BOM_08_004_Male_NightElf_Kurtrus_InGame_HE_Custom_IfKurtrusSucceed_01_A.prefab:a125f742904bfd140a879bf36a535026");

	private static readonly AssetReference VO_BOM_08_004_Male_NightElf_Kurtrus_InGame_HE_Custom_IfKurtrusSucceed_01_C = new AssetReference("VO_BOM_08_004_Male_NightElf_Kurtrus_InGame_HE_Custom_IfKurtrusSucceed_01_C.prefab:2332b5978c7cf2142a2f0ed29f0c296b");

	private static readonly AssetReference VO_BOM_08_004_Male_NightElf_Kurtrus_InGame_HE_Custom_IfKurtrusSucceed_01_D = new AssetReference("VO_BOM_08_004_Male_NightElf_Kurtrus_InGame_HE_Custom_IfKurtrusSucceed_01_D.prefab:b870d4d1e4276784d96701239c1d7d3d");

	private static readonly AssetReference VO_BOM_08_004_Male_NightElf_Kurtrus_InGame_HE_Custom_IfKurtrusSucceed_01_E = new AssetReference("VO_BOM_08_004_Male_NightElf_Kurtrus_InGame_HE_Custom_IfKurtrusSucceed_01_E.prefab:d70216eebbef7584a8b9756debee62f2");

	private static readonly AssetReference VO_BOM_08_004_Male_NightElf_Kurtrus_InGame_HE_Custom_IfKurtrusSucceed_01_F = new AssetReference("VO_BOM_08_004_Male_NightElf_Kurtrus_InGame_HE_Custom_IfKurtrusSucceed_01_F.prefab:ea9cc3736c53739428d981b1ac2b9047");

	private static readonly AssetReference VO_BOM_08_004_Male_NightElf_Kurtrus_InGame_Turn_05_01_B = new AssetReference("VO_BOM_08_004_Male_NightElf_Kurtrus_InGame_Turn_05_01_B.prefab:21c309b4c8006b04b8d36a2a299e3864");

	private int m_PuzzleNumber = 1;

	private HashSet<string> m_playedLines = new HashSet<string>();

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> m_SoundFiles = new List<string>
		{
			VO_BOM_08_004_Female_Draenei_Xyrella_InGame_HE_Custom_IfXyrellaFail_01, VO_BOM_08_004_Female_Draenei_Xyrella_InGame_HE_Custom_IfXyrellaSucceed_01_A, VO_BOM_08_004_Female_Draenei_Xyrella_InGame_HE_Custom_IfXyrellaSucceed_01_B, VO_BOM_08_004_Female_Draenei_Xyrella_InGame_HE_Custom_IfXyrellaSucceed_01_D, VO_BOM_08_004_Female_Draenei_Xyrella_InGame_Turn_07_01_B, VO_BOM_08_004_Female_Human_Cariel_InGame_HE_Custom_IfCarielFail_01, VO_BOM_08_004_Female_Human_Cariel_InGame_HE_Custom_IfCarielSucceed_01_A, VO_BOM_08_004_Female_Human_Cariel_InGame_HE_Custom_IfCarielSucceed_01_C, VO_BOM_08_004_Female_Human_Cariel_InGame_HE_Custom_IfCarielSucceed_01_D, VO_BOM_08_004_Female_Human_Cariel_InGame_Turn_03_01_B,
			VO_BOM_08_004_Male_Dwarf_Tavish_InGame_HE_Custom_IfCarielSucceed_01_B, VO_BOM_08_004_Male_Dwarf_Tavish_InGame_HE_Custom_IfKurtrusSucceed_01_B, VO_BOM_08_004_Male_Dwarf_Tavish_InGame_HE_Custom_IfScabbsSuccess_01_C, VO_BOM_08_004_Male_Dwarf_Tavish_InGame_HE_Custom_IfXyrellaSucceed_01_C, VO_BOM_08_004_Male_Dwarf_Tavish_InGame_Introduction_01, VO_BOM_08_004_Male_Dwarf_Tavish_InGame_Turn_01_01_B, VO_BOM_08_004_Male_Dwarf_Tavish_InGame_Turn_03_01_A, VO_BOM_08_004_Male_Dwarf_Tavish_InGame_Turn_03_01_C, VO_BOM_08_004_Male_Dwarf_Tavish_InGame_Turn_05_01_A, VO_BOM_08_004_Male_Dwarf_Tavish_InGame_Turn_07_01_A,
			VO_BOM_08_004_Male_Dwarf_Tavish_InGame_Victory_PreExplosion_01_A, VO_BOM_08_004_Male_Gnome_Scabbs_InGame_HE_Custom_IfScabbsFail_01, VO_BOM_08_004_Male_Gnome_Scabbs_InGame_HE_Custom_IfScabbsSuccess_01_A, VO_BOM_08_004_Male_Gnome_Scabbs_InGame_HE_Custom_IfScabbsSuccess_01_B, VO_BOM_08_004_Male_Gnome_Scabbs_InGame_Turn_01_01_A, VO_BOM_08_004_Male_Gnome_Scabbs_InGame_Turn_01_01_C, VO_BOM_08_004_Male_NightElf_Kurtrus_InGame_HE_Custom_IfKurtrusFail_01, VO_BOM_08_004_Male_NightElf_Kurtrus_InGame_HE_Custom_IfKurtrusSucceed_01_A, VO_BOM_08_004_Male_NightElf_Kurtrus_InGame_HE_Custom_IfKurtrusSucceed_01_C, VO_BOM_08_004_Male_NightElf_Kurtrus_InGame_HE_Custom_IfKurtrusSucceed_01_D,
			VO_BOM_08_004_Male_NightElf_Kurtrus_InGame_HE_Custom_IfKurtrusSucceed_01_E, VO_BOM_08_004_Male_NightElf_Kurtrus_InGame_HE_Custom_IfKurtrusSucceed_01_F, VO_BOM_08_004_Male_NightElf_Kurtrus_InGame_Turn_05_01_B
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
		m_OverrideMusicTrack = MusicPlaylistType.InGame_Default;
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
		case 514:
			yield return MissionPlayVO(friendlyActor, VO_BOM_08_004_Male_Dwarf_Tavish_InGame_Introduction_01);
			break;
		case 504:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(friendlyActor, VO_BOM_08_004_Male_Dwarf_Tavish_InGame_Victory_PreExplosion_01_A);
			GameState.Get().SetBusy(busy: false);
			break;
		case 101:
			MissionPause(pause: true);
			yield return MissionPlayVOOnce(BOM_08_Tavish_Dungeon.Scabbs5_BrassRing_Quote, VO_BOM_08_004_Male_Gnome_Scabbs_InGame_HE_Custom_IfScabbsSuccess_01_A);
			yield return MissionPlayVOOnce(BOM_08_Tavish_Dungeon.Scabbs5_BrassRing_Quote, VO_BOM_08_004_Male_Gnome_Scabbs_InGame_HE_Custom_IfScabbsSuccess_01_B);
			yield return MissionPlayVOOnce(friendlyActor, VO_BOM_08_004_Male_Dwarf_Tavish_InGame_HE_Custom_IfScabbsSuccess_01_C);
			MissionPause(pause: false);
			break;
		case 102:
			MissionPause(pause: true);
			yield return MissionPlayVOOnce(BOM_08_Tavish_Dungeon.Cariel5_BrassRing_Quote, VO_BOM_08_004_Female_Human_Cariel_InGame_HE_Custom_IfCarielSucceed_01_A);
			yield return MissionPlayVOOnce(friendlyActor, VO_BOM_08_004_Male_Dwarf_Tavish_InGame_HE_Custom_IfCarielSucceed_01_B);
			yield return MissionPlayVOOnce(BOM_08_Tavish_Dungeon.Cariel5_BrassRing_Quote, VO_BOM_08_004_Female_Human_Cariel_InGame_HE_Custom_IfCarielSucceed_01_C);
			yield return MissionPlayVOOnce(BOM_08_Tavish_Dungeon.Cariel5_BrassRing_Quote, VO_BOM_08_004_Female_Human_Cariel_InGame_HE_Custom_IfCarielSucceed_01_D);
			MissionPause(pause: false);
			break;
		case 103:
			MissionPause(pause: true);
			yield return MissionPlayVOOnce(BOM_08_Tavish_Dungeon.KurtrusTier5_BrassRing_Quote, VO_BOM_08_004_Male_NightElf_Kurtrus_InGame_HE_Custom_IfKurtrusSucceed_01_A);
			yield return MissionPlayVOOnce(friendlyActor, VO_BOM_08_004_Male_Dwarf_Tavish_InGame_HE_Custom_IfKurtrusSucceed_01_B);
			yield return MissionPlayVOOnce(BOM_08_Tavish_Dungeon.KurtrusTier5_BrassRing_Quote, VO_BOM_08_004_Male_NightElf_Kurtrus_InGame_HE_Custom_IfKurtrusSucceed_01_C);
			yield return MissionPlayVOOnce(BOM_08_Tavish_Dungeon.KurtrusTier5_BrassRing_Quote, VO_BOM_08_004_Male_NightElf_Kurtrus_InGame_HE_Custom_IfKurtrusSucceed_01_D);
			yield return MissionPlayVOOnce(BOM_08_Tavish_Dungeon.KurtrusTier5_BrassRing_Quote, VO_BOM_08_004_Male_NightElf_Kurtrus_InGame_HE_Custom_IfKurtrusSucceed_01_E);
			yield return MissionPlayVOOnce(BOM_08_Tavish_Dungeon.KurtrusTier5_BrassRing_Quote, VO_BOM_08_004_Male_NightElf_Kurtrus_InGame_HE_Custom_IfKurtrusSucceed_01_F);
			MissionPause(pause: false);
			break;
		case 104:
			MissionPause(pause: true);
			yield return MissionPlayVOOnce(BOM_08_Tavish_Dungeon.Alterac_XyrellaArt_BrassRing_Quote, VO_BOM_08_004_Female_Draenei_Xyrella_InGame_HE_Custom_IfXyrellaSucceed_01_A);
			yield return MissionPlayVOOnce(BOM_08_Tavish_Dungeon.Alterac_XyrellaArt_BrassRing_Quote, VO_BOM_08_004_Female_Draenei_Xyrella_InGame_HE_Custom_IfXyrellaSucceed_01_B);
			yield return MissionPlayVOOnce(friendlyActor, VO_BOM_08_004_Male_Dwarf_Tavish_InGame_HE_Custom_IfXyrellaSucceed_01_C);
			yield return MissionPlayVOOnce(BOM_08_Tavish_Dungeon.Alterac_XyrellaArt_BrassRing_Quote, VO_BOM_08_004_Female_Draenei_Xyrella_InGame_HE_Custom_IfXyrellaSucceed_01_D);
			MissionPause(pause: false);
			break;
		case 520:
			switch (m_PuzzleNumber)
			{
			case 1:
				yield return MissionPlayVOOnce(BOM_08_Tavish_Dungeon.Scabbs5_BrassRing_Quote, VO_BOM_08_004_Male_Gnome_Scabbs_InGame_HE_Custom_IfScabbsFail_01);
				break;
			case 2:
				yield return MissionPlayVOOnce(BOM_08_Tavish_Dungeon.Alterac_CarielArt_BrassRing_Quote, VO_BOM_08_004_Female_Human_Cariel_InGame_HE_Custom_IfCarielFail_01);
				break;
			case 3:
				yield return MissionPlayVOOnce(BOM_08_Tavish_Dungeon.KurtrusTier5_BrassRing_Quote, VO_BOM_08_004_Male_NightElf_Kurtrus_InGame_HE_Custom_IfKurtrusFail_01);
				break;
			case 4:
				yield return MissionPlayVOOnce(BOM_08_Tavish_Dungeon.Alterac_XyrellaArt_BrassRing_Quote, VO_BOM_08_004_Female_Draenei_Xyrella_InGame_HE_Custom_IfXyrellaFail_01);
				break;
			}
			break;
		case 201:
			m_PuzzleNumber = 1;
			break;
		case 202:
			m_PuzzleNumber = 2;
			break;
		case 203:
			m_PuzzleNumber = 3;
			break;
		case 204:
			m_PuzzleNumber = 4;
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
			MissionPause(pause: true);
			yield return MissionPlayVOOnce(BOM_08_Tavish_Dungeon.Scabbs5_BrassRing_Quote, VO_BOM_08_004_Male_Gnome_Scabbs_InGame_Turn_01_01_A);
			yield return MissionPlayVOOnce(friendlyActor, VO_BOM_08_004_Male_Dwarf_Tavish_InGame_Turn_01_01_B);
			yield return MissionPlayVOOnce(BOM_08_Tavish_Dungeon.Scabbs5_BrassRing_Quote, VO_BOM_08_004_Male_Gnome_Scabbs_InGame_Turn_01_01_C);
			MissionPause(pause: false);
			break;
		case 2:
			switch (m_PuzzleNumber)
			{
			case 2:
				MissionPause(pause: true);
				yield return MissionPlayVOOnce(friendlyActor, VO_BOM_08_004_Male_Dwarf_Tavish_InGame_Turn_03_01_A);
				yield return MissionPlayVOOnce(BOM_08_Tavish_Dungeon.Cariel5_BrassRing_Quote, VO_BOM_08_004_Female_Human_Cariel_InGame_Turn_03_01_B);
				yield return MissionPlayVOOnce(friendlyActor, VO_BOM_08_004_Male_Dwarf_Tavish_InGame_Turn_03_01_C);
				MissionPause(pause: false);
				break;
			case 3:
				MissionPause(pause: true);
				yield return MissionPlayVOOnce(friendlyActor, VO_BOM_08_004_Male_Dwarf_Tavish_InGame_Turn_05_01_A);
				yield return MissionPlayVOOnce(BOM_08_Tavish_Dungeon.KurtrusTier5_BrassRing_Quote, VO_BOM_08_004_Male_NightElf_Kurtrus_InGame_Turn_05_01_B);
				MissionPause(pause: false);
				break;
			case 4:
				MissionPause(pause: true);
				yield return MissionPlayVO(friendlyActor, VO_BOM_08_004_Male_Dwarf_Tavish_InGame_Turn_07_01_A);
				yield return MissionPlayVOOnce(BOM_08_Tavish_Dungeon.Alterac_XyrellaArt_BrassRing_Quote, VO_BOM_08_004_Female_Draenei_Xyrella_InGame_Turn_07_01_B);
				MissionPause(pause: false);
				break;
			}
			break;
		}
	}
}
