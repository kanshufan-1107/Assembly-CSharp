using System.Collections;
using System.Collections.Generic;

public class BOM_08_Tavish_Fight_006 : BOM_08_Tavish_Dungeon
{
	private static readonly AssetReference VO_BOM_08_006_Female_Human_Cariel_InGame_HE_Custom_AfterChampionFalls_02_01_B = new AssetReference("VO_BOM_08_006_Female_Human_Cariel_InGame_HE_Custom_AfterChampionFalls_02_01_B.prefab:ab758255182547c4abbd65480669b4cb");

	private static readonly AssetReference VO_BOM_08_006_Female_Human_Cariel_InGame_HE_Custom_AfterChampionFalls_03_01_B = new AssetReference("VO_BOM_08_006_Female_Human_Cariel_InGame_HE_Custom_AfterChampionFalls_03_01_B.prefab:cb73357298f564b4c81cc7c9dddbc597");

	private static readonly AssetReference VO_BOM_08_006_Female_Human_Cariel_InGame_HE_Custom_AfterChampionFalls_04_01_C = new AssetReference("VO_BOM_08_006_Female_Human_Cariel_InGame_HE_Custom_AfterChampionFalls_04_01_C.prefab:be5d3d99881baa84ba5bb7381e8afb89");

	private static readonly AssetReference VO_BOM_08_006_Female_Human_Cariel_InGame_Turn_01_01_A = new AssetReference("VO_BOM_08_006_Female_Human_Cariel_InGame_Turn_01_01_A.prefab:90ebf26629224cd459f43b1fc43c025e");

	private static readonly AssetReference VO_BOM_08_006_Male_Dwarf_Tavish_InGame_HE_Custom_AfterChampionFalls_01_01_B = new AssetReference("VO_BOM_08_006_Male_Dwarf_Tavish_InGame_HE_Custom_AfterChampionFalls_01_01_B.prefab:ec0992820a07d5043b2144bc5dea4be1");

	private static readonly AssetReference VO_BOM_08_006_Male_Dwarf_Tavish_InGame_HE_Custom_AfterChampionFalls_02_01_A = new AssetReference("VO_BOM_08_006_Male_Dwarf_Tavish_InGame_HE_Custom_AfterChampionFalls_02_01_A.prefab:44715a5496447b144abe7796e3ab4f61");

	private static readonly AssetReference VO_BOM_08_006_Male_Dwarf_Tavish_InGame_HE_Custom_AfterChampionFalls_04_01_B = new AssetReference("VO_BOM_08_006_Male_Dwarf_Tavish_InGame_HE_Custom_AfterChampionFalls_04_01_B.prefab:f5de7b93586ac1545a4f5ea80e129073");

	private static readonly AssetReference VO_BOM_08_006_Male_Dwarf_Tavish_InGame_Introduction_01_A = new AssetReference("VO_BOM_08_006_Male_Dwarf_Tavish_InGame_Introduction_01_A.prefab:dd6330389f705e341a314a1f904823cb");

	private static readonly AssetReference VO_BOM_08_006_Male_Dwarf_Tavish_UI_AWD_Boss_Reveal_General_06_01_A = new AssetReference("VO_BOM_08_006_Male_Dwarf_Tavish_UI_AWD_Boss_Reveal_General_06_01_A.prefab:c21fd18026a92d644875f5bca6cab5fd");

	private static readonly AssetReference VO_BOM_08_006_Male_Dwarf_Tavish_UI_AWD_Boss_Reveal_General_06_01_B = new AssetReference("VO_BOM_08_006_Male_Dwarf_Tavish_UI_AWD_Boss_Reveal_General_06_01_B.prefab:78707dedd42914644bedbef2f7127606");

	private static readonly AssetReference VO_BOM_08_006_Male_Orc_DrekThar_InGame_BossDeath_01 = new AssetReference("VO_BOM_08_006_Male_Orc_DrekThar_InGame_BossDeath_01.prefab:971210d32a5b1f1468fa980befb95a0d");

	private static readonly AssetReference VO_BOM_08_006_Male_Orc_DrekThar_InGame_BossIdle_01 = new AssetReference("VO_BOM_08_006_Male_Orc_DrekThar_InGame_BossIdle_01.prefab:7a35c9e81b641ef41b3da1d66591c5a4");

	private static readonly AssetReference VO_BOM_08_006_Male_Orc_DrekThar_InGame_BossIdle_02 = new AssetReference("VO_BOM_08_006_Male_Orc_DrekThar_InGame_BossIdle_02.prefab:5db1e882a4b902b44b28f862c8e5e239");

	private static readonly AssetReference VO_BOM_08_006_Male_Orc_DrekThar_InGame_BossIdle_03 = new AssetReference("VO_BOM_08_006_Male_Orc_DrekThar_InGame_BossIdle_03.prefab:19f94038b891a9f4ca2f27d8d07ec097");

	private static readonly AssetReference VO_BOM_08_006_Male_Orc_DrekThar_InGame_BossUsesHeroPower_01 = new AssetReference("VO_BOM_08_006_Male_Orc_DrekThar_InGame_BossUsesHeroPower_01.prefab:52c414b10c7a9b74c95a81ae739ccd5e");

	private static readonly AssetReference VO_BOM_08_006_Male_Orc_DrekThar_InGame_BossUsesHeroPower_02 = new AssetReference("VO_BOM_08_006_Male_Orc_DrekThar_InGame_BossUsesHeroPower_02.prefab:9c24255aa960a5042b7147a339d04d7d");

	private static readonly AssetReference VO_BOM_08_006_Male_Orc_DrekThar_InGame_EmoteResponse_01 = new AssetReference("VO_BOM_08_006_Male_Orc_DrekThar_InGame_EmoteResponse_01.prefab:88b4179bdd910244c929ac7940c5c65b");

	private static readonly AssetReference VO_BOM_08_006_Male_Orc_DrekThar_InGame_HE_Custom_AfterChampionFalls_01_01_A = new AssetReference("VO_BOM_08_006_Male_Orc_DrekThar_InGame_HE_Custom_AfterChampionFalls_01_01_A.prefab:fa63c7482bb24114db290ae6249df788");

	private static readonly AssetReference VO_BOM_08_006_Male_Orc_DrekThar_InGame_HE_Custom_AfterChampionFalls_01_01_C = new AssetReference("VO_BOM_08_006_Male_Orc_DrekThar_InGame_HE_Custom_AfterChampionFalls_01_01_C.prefab:b497233fc15661e4f8cf74bb6767c472");

	private static readonly AssetReference VO_BOM_08_006_Male_Orc_DrekThar_InGame_HE_Custom_AfterChampionFalls_03_01_A = new AssetReference("VO_BOM_08_006_Male_Orc_DrekThar_InGame_HE_Custom_AfterChampionFalls_03_01_A.prefab:67bd261a90626a543a153f99afe11fe5");

	private static readonly AssetReference VO_BOM_08_006_Male_Orc_DrekThar_InGame_Introduction_01_B = new AssetReference("VO_BOM_08_006_Male_Orc_DrekThar_InGame_Introduction_01_B.prefab:7a10b7a882ccfe54d992b7493720aa45");

	private static readonly AssetReference VO_BOM_08_006_Male_Orc_DrekThar_InGame_PlayerLoss_01 = new AssetReference("VO_BOM_08_006_Male_Orc_DrekThar_InGame_PlayerLoss_01.prefab:56e4c5759046f614f8b0c49fc0c2234b");

	private static readonly AssetReference VO_BOM_08_006_Male_Orc_DrekThar_InGame_Turn_01_01_B = new AssetReference("VO_BOM_08_006_Male_Orc_DrekThar_InGame_Turn_01_01_B.prefab:ab4f24357eb70724189be1c36f60192a");

	private static readonly AssetReference VO_BOM_08_006_Male_Undead_Grummus_InGame_HE_Custom_AfterChampionFalls_04_01_A = new AssetReference("VO_BOM_08_006_Male_Undead_Grummus_InGame_HE_Custom_AfterChampionFalls_04_01_A.prefab:9726bf91c91b65f45ac87bf7b4540cfa");

	private static readonly AssetReference VO_BOM_08_006_Male_Orc_DrekThar_InGame_LossPreExplosion_01 = new AssetReference("VO_BOM_08_006_Male_Orc_DrekThar_InGame_LossPreExplosion_01.prefab:a868649d16234530b6d7db6bfc467fc4");

	private List<string> m_InGame_BossIdleLines = new List<string> { VO_BOM_08_006_Male_Orc_DrekThar_InGame_BossIdle_01, VO_BOM_08_006_Male_Orc_DrekThar_InGame_BossIdle_02, VO_BOM_08_006_Male_Orc_DrekThar_InGame_BossIdle_03 };

	private List<string> m_InGame_BossUsesHeroPowerLines = new List<string> { VO_BOM_08_006_Male_Orc_DrekThar_InGame_BossUsesHeroPower_01, VO_BOM_08_006_Male_Orc_DrekThar_InGame_BossUsesHeroPower_02 };

	private HashSet<string> m_playedLines = new HashSet<string>();

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> m_SoundFiles = new List<string>
		{
			VO_BOM_08_006_Female_Human_Cariel_InGame_HE_Custom_AfterChampionFalls_02_01_B, VO_BOM_08_006_Female_Human_Cariel_InGame_HE_Custom_AfterChampionFalls_03_01_B, VO_BOM_08_006_Female_Human_Cariel_InGame_HE_Custom_AfterChampionFalls_04_01_C, VO_BOM_08_006_Female_Human_Cariel_InGame_Turn_01_01_A, VO_BOM_08_006_Male_Dwarf_Tavish_InGame_HE_Custom_AfterChampionFalls_01_01_B, VO_BOM_08_006_Male_Dwarf_Tavish_InGame_HE_Custom_AfterChampionFalls_02_01_A, VO_BOM_08_006_Male_Dwarf_Tavish_InGame_HE_Custom_AfterChampionFalls_04_01_B, VO_BOM_08_006_Male_Dwarf_Tavish_InGame_Introduction_01_A, VO_BOM_08_006_Male_Orc_DrekThar_InGame_BossDeath_01, VO_BOM_08_006_Male_Orc_DrekThar_InGame_BossIdle_01,
			VO_BOM_08_006_Male_Orc_DrekThar_InGame_BossIdle_02, VO_BOM_08_006_Male_Orc_DrekThar_InGame_BossIdle_03, VO_BOM_08_006_Male_Orc_DrekThar_InGame_BossUsesHeroPower_01, VO_BOM_08_006_Male_Orc_DrekThar_InGame_BossUsesHeroPower_02, VO_BOM_08_006_Male_Orc_DrekThar_InGame_EmoteResponse_01, VO_BOM_08_006_Male_Orc_DrekThar_InGame_HE_Custom_AfterChampionFalls_01_01_A, VO_BOM_08_006_Male_Orc_DrekThar_InGame_HE_Custom_AfterChampionFalls_01_01_C, VO_BOM_08_006_Male_Orc_DrekThar_InGame_HE_Custom_AfterChampionFalls_03_01_A, VO_BOM_08_006_Male_Orc_DrekThar_InGame_Introduction_01_B, VO_BOM_08_006_Male_Orc_DrekThar_InGame_PlayerLoss_01,
			VO_BOM_08_006_Male_Orc_DrekThar_InGame_Turn_01_01_B, VO_BOM_08_006_Male_Undead_Grummus_InGame_HE_Custom_AfterChampionFalls_04_01_A
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
		m_OverrideMusicTrack = MusicPlaylistType.InGame_AV;
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
			yield return MissionPlaySound(enemyActor, VO_BOM_08_006_Male_Orc_DrekThar_InGame_BossDeath_01);
			break;
		case 517:
			yield return MissionPlayVO(enemyActor, m_InGame_BossIdleLines);
			break;
		case 510:
			yield return MissionPlayVO(enemyActor, m_InGame_BossUsesHeroPowerLines);
			break;
		case 515:
			yield return MissionPlayVO(enemyActor, VO_BOM_08_006_Male_Orc_DrekThar_InGame_EmoteResponse_01);
			break;
		case 514:
			yield return MissionPlayVO(friendlyActor, VO_BOM_08_006_Male_Dwarf_Tavish_InGame_Introduction_01_A);
			yield return MissionPlayVO(enemyActor, VO_BOM_08_006_Male_Orc_DrekThar_InGame_Introduction_01_B);
			break;
		case 506:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(enemyActor, VO_BOM_08_006_Male_Orc_DrekThar_InGame_PlayerLoss_01);
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
			yield return MissionPlayVO(BOM_08_Tavish_Dungeon.Alterac_CarielArt_BrassRing_Quote, VO_BOM_08_006_Female_Human_Cariel_InGame_Turn_01_01_A);
			yield return MissionPlayVO(enemyActor, VO_BOM_08_006_Male_Orc_DrekThar_InGame_Turn_01_01_B);
			break;
		case 7:
			yield return MissionPlayVO(friendlyActor, VO_BOM_08_006_Male_Dwarf_Tavish_InGame_HE_Custom_AfterChampionFalls_02_01_A);
			yield return MissionPlayVO(BOM_08_Tavish_Dungeon.Alterac_CarielArt_BrassRing_Quote, VO_BOM_08_006_Female_Human_Cariel_InGame_HE_Custom_AfterChampionFalls_02_01_B);
			break;
		}
	}
}
