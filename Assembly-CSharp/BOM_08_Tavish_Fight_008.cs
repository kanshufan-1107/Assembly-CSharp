using System.Collections;
using System.Collections.Generic;

public class BOM_08_Tavish_Fight_008 : BOM_08_Tavish_Dungeon
{
	private static readonly AssetReference VO_BOM_08_008_Female_Undead_LichTamsin_InGame_Victory_PreExplosion_01_C = new AssetReference("VO_BOM_08_008_Female_Undead_LichTamsin_InGame_Victory_PreExplosion_01_C.prefab:52e9a1f2950eeb74986c31b407a6fd42");

	private static readonly AssetReference VO_BOM_08_008_Female_Undead_LichTamsin_UI_AWD_Boss_Reveal_General_09_01_C = new AssetReference("VO_BOM_08_008_Female_Undead_LichTamsin_UI_AWD_Boss_Reveal_General_09_01_C.prefab:31ffcac3ffa1c3d429c4e5d36c70a177");

	private static readonly AssetReference VO_BOM_08_008_Male_Dragon_Kazakusan_InGame_HE_Custom_IfSevenMercsInPlay_01_B = new AssetReference("VO_BOM_08_008_Male_Dragon_Kazakusan_InGame_HE_Custom_IfSevenMercsInPlay_01_B.prefab:379b42400ef22f643a5a5f573209f38a");

	private static readonly AssetReference VO_BOM_08_008_Male_Dragon_Kazakusan_InGame_Introduction_01_A = new AssetReference("VO_BOM_08_008_Male_Dragon_Kazakusan_InGame_Introduction_01_A.prefab:e9020efb606a43045ad74b41f8c6da58");

	private static readonly AssetReference VO_BOM_08_008_Male_Dragon_Kazakusan_InGame_Turn_03_01_A = new AssetReference("VO_BOM_08_008_Male_Dragon_Kazakusan_InGame_Turn_03_01_A.prefab:7a2f978ca073470488502031aace889c");

	private static readonly AssetReference VO_BOM_08_008_Male_Dragon_Kazakusan_InGame_Turn_07_01_A = new AssetReference("VO_BOM_08_008_Male_Dragon_Kazakusan_InGame_Turn_07_01_A.prefab:bdc62bdd39e32954dad4ecd20846fcd6");

	private static readonly AssetReference VO_BOM_08_008_Male_Dragon_Kazakusan_InGame_Turn_11_01 = new AssetReference("VO_BOM_08_008_Male_Dragon_Kazakusan_InGame_Turn_11_01.prefab:b13c84e938275834f945119209ac5246");

	private static readonly AssetReference VO_BOM_08_008_Male_Dragon_Kazakusan_InGame_Victory_PreExplosion_01_A = new AssetReference("VO_BOM_08_008_Male_Dragon_Kazakusan_InGame_Victory_PreExplosion_01_A.prefab:e5b6ee9262d0e5a449627b6fdf37dfc6");

	private static readonly AssetReference VO_BOM_08_008_Male_Dragon_Kazakusan_InGame_Victory_PreExplosion_01_B = new AssetReference("VO_BOM_08_008_Male_Dragon_Kazakusan_InGame_Victory_PreExplosion_01_B.prefab:e9685c4753e26be44b4541aca80ac960");

	private static readonly AssetReference VO_BOM_08_008_Male_Dwarf_Tavish_InGame_HE_Custom_IfSevenMercsInPlay_01_A = new AssetReference("VO_BOM_08_008_Male_Dwarf_Tavish_InGame_HE_Custom_IfSevenMercsInPlay_01_A.prefab:ffc15732d33eddc4fb4e7670570866ca");

	private static readonly AssetReference VO_BOM_08_008_Male_Dwarf_Tavish_InGame_Introduction_01_B = new AssetReference("VO_BOM_08_008_Male_Dwarf_Tavish_InGame_Introduction_01_B.prefab:54754a1a90cf9a04fa45a44aa80a05f9");

	private static readonly AssetReference VO_BOM_08_008_Male_Dwarf_Tavish_InGame_Turn_07_01_B = new AssetReference("VO_BOM_08_008_Male_Dwarf_Tavish_InGame_Turn_07_01_B.prefab:6f039842f35e2894b90637d6ab4d2ef9");

	private static readonly AssetReference VO_BOM_08_008_Male_Dwarf_Tavish_UI_AWD_Boss_Reveal_General_08_01_A = new AssetReference("VO_BOM_08_008_Male_Dwarf_Tavish_UI_AWD_Boss_Reveal_General_08_01_A.prefab:7016c4bb7c7259b478ecffcfdb3b1ccb");

	private static readonly AssetReference VO_BOM_08_008_Male_Dwarf_Tavish_UI_AWD_Boss_Reveal_General_08_01_C = new AssetReference("VO_BOM_08_008_Male_Dwarf_Tavish_UI_AWD_Boss_Reveal_General_08_01_C.prefab:2fdbeed48f9386640baceca6a4fadbbd");

	private static readonly AssetReference VO_BOM_08_008_Male_Dwarf_Tavish_UI_AWD_Boss_Reveal_General_09_01_A = new AssetReference("VO_BOM_08_008_Male_Dwarf_Tavish_UI_AWD_Boss_Reveal_General_09_01_A.prefab:4dbc3e2e520e18b41b515d3bb7045047");

	private static readonly AssetReference VO_BOM_08_008_Male_Dwarf_Tavish_UI_AWD_Boss_Reveal_General_09_01_B = new AssetReference("VO_BOM_08_008_Male_Dwarf_Tavish_UI_AWD_Boss_Reveal_General_09_01_B.prefab:9640645a35d12f04bbcf45dff1dcd6aa");

	private static readonly AssetReference VO_BOM_08_008_X_Naaru_Mida_InGame_Turn_03_01_B = new AssetReference("VO_BOM_08_008_X_Naaru_Mida_InGame_Turn_03_01_B.prefab:0e1cfcf67aa6cb646b7ed4253397f4f5");

	private HashSet<string> m_playedLines = new HashSet<string>();

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> m_SoundFiles = new List<string>
		{
			VO_BOM_08_008_Female_Undead_LichTamsin_InGame_Victory_PreExplosion_01_C, VO_BOM_08_008_Male_Dragon_Kazakusan_InGame_HE_Custom_IfSevenMercsInPlay_01_B, VO_BOM_08_008_Male_Dragon_Kazakusan_InGame_Introduction_01_A, VO_BOM_08_008_Male_Dragon_Kazakusan_InGame_Turn_03_01_A, VO_BOM_08_008_Male_Dragon_Kazakusan_InGame_Turn_07_01_A, VO_BOM_08_008_Male_Dragon_Kazakusan_InGame_Turn_11_01, VO_BOM_08_008_Male_Dragon_Kazakusan_InGame_Victory_PreExplosion_01_A, VO_BOM_08_008_Male_Dragon_Kazakusan_InGame_Victory_PreExplosion_01_B, VO_BOM_08_008_Male_Dwarf_Tavish_InGame_HE_Custom_IfSevenMercsInPlay_01_A, VO_BOM_08_008_Male_Dwarf_Tavish_InGame_Introduction_01_B,
			VO_BOM_08_008_Male_Dwarf_Tavish_InGame_Turn_07_01_B, VO_BOM_08_008_X_Naaru_Mida_InGame_Turn_03_01_B
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
		m_OverrideMusicTrack = MusicPlaylistType.InGame_DRGEVILBoss;
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
		case 515:
			yield return MissionPlayVO(enemyActor, VO_BOM_08_008_Male_Dragon_Kazakusan_InGame_HE_Custom_IfSevenMercsInPlay_01_B);
			break;
		case 514:
			yield return MissionPlayVO(enemyActor, VO_BOM_08_008_Male_Dragon_Kazakusan_InGame_Introduction_01_A);
			yield return MissionPlayVO(friendlyActor, VO_BOM_08_008_Male_Dwarf_Tavish_InGame_Introduction_01_B);
			break;
		case 504:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(enemyActor, VO_BOM_08_008_Male_Dragon_Kazakusan_InGame_Victory_PreExplosion_01_A);
			yield return MissionPlayVO(enemyActor, VO_BOM_08_008_Male_Dragon_Kazakusan_InGame_Victory_PreExplosion_01_B);
			yield return MissionPlayVO(enemyActor, VO_BOM_08_008_Female_Undead_LichTamsin_InGame_Victory_PreExplosion_01_C);
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
		case 3:
			yield return MissionPlayVO(enemyActor, VO_BOM_08_008_Male_Dragon_Kazakusan_InGame_Turn_03_01_A);
			yield return MissionPlayVO("BOM_08_Mida_008t", VO_BOM_08_008_X_Naaru_Mida_InGame_Turn_03_01_B);
			break;
		case 7:
			yield return MissionPlayVO(enemyActor, VO_BOM_08_008_Male_Dragon_Kazakusan_InGame_Turn_07_01_A);
			yield return MissionPlayVO(friendlyActor, VO_BOM_08_008_Male_Dwarf_Tavish_InGame_Turn_07_01_B);
			break;
		case 11:
			yield return MissionPlayVO(enemyActor, VO_BOM_08_008_Male_Dragon_Kazakusan_InGame_Turn_11_01);
			break;
		}
	}
}
