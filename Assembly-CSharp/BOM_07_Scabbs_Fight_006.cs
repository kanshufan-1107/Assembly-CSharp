using System.Collections;
using System.Collections.Generic;

public class BOM_07_Scabbs_Fight_006 : BOM_07_Scabbs_Dungeon
{
	private static readonly AssetReference VO_Story_Hero_Brukan_Male_Troll_BOM_Scabbs_Mission6ExchangeF_Kurtrus_01 = new AssetReference("VO_Story_Hero_Brukan_Male_Troll_BOM_Scabbs_Mission6ExchangeF_Kurtrus_01.prefab:868018134e224639822e16d224cac590");

	private static readonly AssetReference VO_Story_Hero_Brukan_Male_Troll_BOM_Scabbs_Mission6ExchangeF_Kurtrus_03 = new AssetReference("VO_Story_Hero_Brukan_Male_Troll_BOM_Scabbs_Mission6ExchangeF_Kurtrus_03.prefab:b11c62a1eec34dd6a784645a90bf6ceb");

	private static readonly AssetReference VO_Story_Hero_Cariel_Female_Human_BOM_Scabbs_Mission6ExchangeC_Cariel_01 = new AssetReference("VO_Story_Hero_Cariel_Female_Human_BOM_Scabbs_Mission6ExchangeC_Cariel_01.prefab:289e8bac3053479eb048c985e72c3e13");

	private static readonly AssetReference VO_Story_Hero_Cariel_Female_Human_BOM_Scabbs_Mission6ExchangeD_Cariel_01 = new AssetReference("VO_Story_Hero_Cariel_Female_Human_BOM_Scabbs_Mission6ExchangeD_Cariel_01.prefab:eb9f4cadc18b4fe2b3dce23e048371f3");

	private static readonly AssetReference VO_Story_Hero_Cariel_Female_Human_BOM_Scabbs_Mission6ExchangeF_Cariel_02 = new AssetReference("VO_Story_Hero_Cariel_Female_Human_BOM_Scabbs_Mission6ExchangeF_Cariel_02.prefab:7360dc139c3940fab9f171288bfeaedd");

	private static readonly AssetReference VO_Story_Hero_Cariel_Female_Human_BOM_Scabbs_Mission6Select_01 = new AssetReference("VO_Story_Hero_Cariel_Female_Human_BOM_Scabbs_Mission6Select_01.prefab:15cfa98276cd4834bf4d900afea3fa32");

	private static readonly AssetReference VO_Story_Hero_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission6ExchangeD_03 = new AssetReference("VO_Story_Hero_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission6ExchangeD_03.prefab:4189cf71902c4db49e05be0c6d5c95fb");

	private static readonly AssetReference VO_Story_Hero_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission6ExchangeE_02 = new AssetReference("VO_Story_Hero_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission6ExchangeE_02.prefab:e3d5e0e384bb43eaa04878bc200d1096");

	private static readonly AssetReference VO_Story_Hero_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission6ExchangeF_Xyrella_01 = new AssetReference("VO_Story_Hero_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission6ExchangeF_Xyrella_01.prefab:8a102d22166143e48f90e3d02a1d321c");

	private static readonly AssetReference VO_Story_Hero_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission6ExchangeF_Xyrella_03 = new AssetReference("VO_Story_Hero_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission6ExchangeF_Xyrella_03.prefab:b3c0180b1b654af4aba1ec26922f9a13");

	private static readonly AssetReference VO_Story_Hero_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission6Victory_02 = new AssetReference("VO_Story_Hero_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission6Victory_02.prefab:2e91a4943b294b8db4adde432c4b1b3f");

	private static readonly AssetReference VO_Story_Hero_Guff_Male_Tauren_BOM_Scabbs_Mission6ExchangeD_02 = new AssetReference("VO_Story_Hero_Guff_Male_Tauren_BOM_Scabbs_Mission6ExchangeD_02.prefab:da4f28b0ba114e4e9ca7b43d9da804d1");

	private static readonly AssetReference VO_Story_Hero_Guff_Male_Tauren_BOM_Scabbs_Mission6ExchangeF_Tavish_01 = new AssetReference("VO_Story_Hero_Guff_Male_Tauren_BOM_Scabbs_Mission6ExchangeF_Tavish_01.prefab:fa06e65368cf47699621d71b6c52ab27");

	private static readonly AssetReference VO_Story_Hero_Guff_Male_Tauren_BOM_Scabbs_Mission6ExchangeF_Tavish_03 = new AssetReference("VO_Story_Hero_Guff_Male_Tauren_BOM_Scabbs_Mission6ExchangeF_Tavish_03.prefab:21712d7330194cd09d5feea438ec7590");

	private static readonly AssetReference VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Scabbs_Mission6ExchangeC_Kurtrus_01 = new AssetReference("VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Scabbs_Mission6ExchangeC_Kurtrus_01.prefab:1505ba33cc0f4b9190ccfcccb0f7666f");

	private static readonly AssetReference VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Scabbs_Mission6ExchangeD_Kurtrus_01 = new AssetReference("VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Scabbs_Mission6ExchangeD_Kurtrus_01.prefab:70fc538af17e4678ab5a6c6b24085f6d");

	private static readonly AssetReference VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Scabbs_Mission6ExchangeF_Kurtrus_02 = new AssetReference("VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Scabbs_Mission6ExchangeF_Kurtrus_02.prefab:f3e84a7cb8b94b79aa047dce8115a59e");

	private static readonly AssetReference VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Scabbs_Mission6Select_01 = new AssetReference("VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Scabbs_Mission6Select_01.prefab:0529cde6eee047ada445ea2e39353337");

	private static readonly AssetReference VO_Story_Hero_Rokara_Female_Orc_BOM_Scabbs_Mission6ExchangeF_Cariel_01 = new AssetReference("VO_Story_Hero_Rokara_Female_Orc_BOM_Scabbs_Mission6ExchangeF_Cariel_01.prefab:54ce9a613ee44a9cb5944dea22442485");

	private static readonly AssetReference VO_Story_Hero_Rokara_Female_Orc_BOM_Scabbs_Mission6ExchangeF_Cariel_03 = new AssetReference("VO_Story_Hero_Rokara_Female_Orc_BOM_Scabbs_Mission6ExchangeF_Cariel_03.prefab:87d12386e8c342768d0eb5cfc45abd34");

	private static readonly AssetReference VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission6ExchangeA_01 = new AssetReference("VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission6ExchangeA_01.prefab:65d3fc8065d843a69e0ac3aafad066df");

	private static readonly AssetReference VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission6ExchangeE_01 = new AssetReference("VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission6ExchangeE_01.prefab:6ed252b38fe34132a6469bd461558da3");

	private static readonly AssetReference VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission6Intro_01 = new AssetReference("VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission6Intro_01.prefab:bc92f74d6e874a07afe1499a5f5d5acf");

	private static readonly AssetReference VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission6Victory_01 = new AssetReference("VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission6Victory_01.prefab:e088b7abd66744ad8614c58157d42510");

	private static readonly AssetReference VO_Story_Hero_Smite_Male_Tauren_BOM_Scabbs_Mission6Death_01 = new AssetReference("VO_Story_Hero_Smite_Male_Tauren_BOM_Scabbs_Mission6Death_01.prefab:9abc8b89fae6463aa8ab7e0882bec487");

	private static readonly AssetReference VO_Story_Hero_Smite_Male_Tauren_BOM_Scabbs_Mission6EmoteResponse_01 = new AssetReference("VO_Story_Hero_Smite_Male_Tauren_BOM_Scabbs_Mission6EmoteResponse_01.prefab:10ec565b71a541a2955490caf07af6ff");

	private static readonly AssetReference VO_Story_Hero_Smite_Male_Tauren_BOM_Scabbs_Mission6ExchangeA_02 = new AssetReference("VO_Story_Hero_Smite_Male_Tauren_BOM_Scabbs_Mission6ExchangeA_02.prefab:43bc9191de2c49bc896c0f0944b364d6");

	private static readonly AssetReference VO_Story_Hero_Smite_Male_Tauren_BOM_Scabbs_Mission6ExchangeB_01 = new AssetReference("VO_Story_Hero_Smite_Male_Tauren_BOM_Scabbs_Mission6ExchangeB_01.prefab:4998893970f44a688c92a7b119034c86");

	private static readonly AssetReference VO_Story_Hero_Smite_Male_Tauren_BOM_Scabbs_Mission6ExchangeC_01 = new AssetReference("VO_Story_Hero_Smite_Male_Tauren_BOM_Scabbs_Mission6ExchangeC_01.prefab:b94b9705cc1242a89cc9d1d667bb4faf");

	private static readonly AssetReference VO_Story_Hero_Smite_Male_Tauren_BOM_Scabbs_Mission6ExchangeG_01 = new AssetReference("VO_Story_Hero_Smite_Male_Tauren_BOM_Scabbs_Mission6ExchangeG_01.prefab:48f4b41433ee49489e94500c58262ba4");

	private static readonly AssetReference VO_Story_Hero_Smite_Male_Tauren_BOM_Scabbs_Mission6HeroPower_01 = new AssetReference("VO_Story_Hero_Smite_Male_Tauren_BOM_Scabbs_Mission6HeroPower_01.prefab:7732c17aca6749c792a12d356e3d8167");

	private static readonly AssetReference VO_Story_Hero_Smite_Male_Tauren_BOM_Scabbs_Mission6HeroPower_02 = new AssetReference("VO_Story_Hero_Smite_Male_Tauren_BOM_Scabbs_Mission6HeroPower_02.prefab:59d2b1c5192944ae89ff0efd757754e9");

	private static readonly AssetReference VO_Story_Hero_Smite_Male_Tauren_BOM_Scabbs_Mission6HeroPower_03 = new AssetReference("VO_Story_Hero_Smite_Male_Tauren_BOM_Scabbs_Mission6HeroPower_03.prefab:5d00a3f3c3a649ae9694295af222e378");

	private static readonly AssetReference VO_Story_Hero_Smite_Male_Tauren_BOM_Scabbs_Mission6Idle_01 = new AssetReference("VO_Story_Hero_Smite_Male_Tauren_BOM_Scabbs_Mission6Idle_01.prefab:76682d76c2b84e00b8ca778b93c29c48");

	private static readonly AssetReference VO_Story_Hero_Smite_Male_Tauren_BOM_Scabbs_Mission6Idle_02 = new AssetReference("VO_Story_Hero_Smite_Male_Tauren_BOM_Scabbs_Mission6Idle_02.prefab:40980572b7e843ffae75b94353220bb4");

	private static readonly AssetReference VO_Story_Hero_Smite_Male_Tauren_BOM_Scabbs_Mission6Idle_03 = new AssetReference("VO_Story_Hero_Smite_Male_Tauren_BOM_Scabbs_Mission6Idle_03.prefab:97d49b588a1145f2b710846a2fc5d030");

	private static readonly AssetReference VO_Story_Hero_Smite_Male_Tauren_BOM_Scabbs_Mission6Intro_02 = new AssetReference("VO_Story_Hero_Smite_Male_Tauren_BOM_Scabbs_Mission6Intro_02.prefab:1ada9a1a4117453ea1a864c659b65433");

	private static readonly AssetReference VO_Story_Hero_Smite_Male_Tauren_BOM_Scabbs_Mission6Loss_01 = new AssetReference("VO_Story_Hero_Smite_Male_Tauren_BOM_Scabbs_Mission6Loss_01.prefab:d265886014c34b4db149493f639af151");

	private static readonly AssetReference VO_Story_Hero_Tavish_Male_Dwarf_BOM_Scabbs_Mission6ExchangeC_Tavish_01 = new AssetReference("VO_Story_Hero_Tavish_Male_Dwarf_BOM_Scabbs_Mission6ExchangeC_Tavish_01.prefab:c771367d1a86452797a677e606b05900");

	private static readonly AssetReference VO_Story_Hero_Tavish_Male_Dwarf_BOM_Scabbs_Mission6ExchangeD_Tavish_01 = new AssetReference("VO_Story_Hero_Tavish_Male_Dwarf_BOM_Scabbs_Mission6ExchangeD_Tavish_01.prefab:c6f689340f7342c09af2bcb5a7eaf30d");

	private static readonly AssetReference VO_Story_Hero_Tavish_Male_Dwarf_BOM_Scabbs_Mission6ExchangeF_Tavish_02 = new AssetReference("VO_Story_Hero_Tavish_Male_Dwarf_BOM_Scabbs_Mission6ExchangeF_Tavish_02.prefab:545cb616f8f140ad9c62bd7630bb70f6");

	private static readonly AssetReference VO_Story_Hero_Tavish_Male_Dwarf_BOM_Scabbs_Mission6ExchangeF_Tavish_04 = new AssetReference("VO_Story_Hero_Tavish_Male_Dwarf_BOM_Scabbs_Mission6ExchangeF_Tavish_04.prefab:9bb667d8ca2e48e3a98757ce28c1256a");

	private static readonly AssetReference VO_Story_Hero_Tavish_Male_Dwarf_BOM_Scabbs_Mission6Select_01 = new AssetReference("VO_Story_Hero_Tavish_Male_Dwarf_BOM_Scabbs_Mission6Select_01.prefab:e6a1676d63ea4e19a531df53c9087c11");

	private static readonly AssetReference VO_Story_Hero_Xyrella_Female_Draenei_BOM_Scabbs_Mission6ExchangeC_Xyrella_01 = new AssetReference("VO_Story_Hero_Xyrella_Female_Draenei_BOM_Scabbs_Mission6ExchangeC_Xyrella_01.prefab:ea296542f70b4586be1bc917e1e78871");

	private static readonly AssetReference VO_Story_Hero_Xyrella_Female_Draenei_BOM_Scabbs_Mission6ExchangeD_Xyrella_01 = new AssetReference("VO_Story_Hero_Xyrella_Female_Draenei_BOM_Scabbs_Mission6ExchangeD_Xyrella_01.prefab:81fc5f4c043542afae96bb68729c1775");

	private static readonly AssetReference VO_Story_Hero_Xyrella_Female_Draenei_BOM_Scabbs_Mission6ExchangeF_Xyrella_02 = new AssetReference("VO_Story_Hero_Xyrella_Female_Draenei_BOM_Scabbs_Mission6ExchangeF_Xyrella_02.prefab:09085f2175b54a1c83cc8523287aa376");

	private static readonly AssetReference VO_Story_Hero_Xyrella_Female_Draenei_BOM_Scabbs_Mission6Select_01 = new AssetReference("VO_Story_Hero_Xyrella_Female_Draenei_BOM_Scabbs_Mission6Select_01.prefab:aada5370fc6341a9b147f5316d488d98");

	private static readonly AssetReference VO_PVPDR_Hero_Scabbs_Male_Gnome_Death_01 = new AssetReference("VO_PVPDR_Hero_Scabbs_Male_Gnome_Death_01.prefab:85e10a0b219b1974aaa959bd523c554b");

	private List<string> m_InGame_BossUsesHeroPowerLines = new List<string> { VO_Story_Hero_Smite_Male_Tauren_BOM_Scabbs_Mission6HeroPower_01, VO_Story_Hero_Smite_Male_Tauren_BOM_Scabbs_Mission6HeroPower_02, VO_Story_Hero_Smite_Male_Tauren_BOM_Scabbs_Mission6HeroPower_03 };

	private List<string> m_InGame_BossIdleLines = new List<string> { VO_Story_Hero_Smite_Male_Tauren_BOM_Scabbs_Mission6Idle_01, VO_Story_Hero_Smite_Male_Tauren_BOM_Scabbs_Mission6Idle_02, VO_Story_Hero_Smite_Male_Tauren_BOM_Scabbs_Mission6Idle_03 };

	private HashSet<string> m_playedLines = new HashSet<string>();

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> m_SoundFiles = new List<string>
		{
			VO_Story_Hero_Brukan_Male_Troll_BOM_Scabbs_Mission6ExchangeF_Kurtrus_01, VO_Story_Hero_Brukan_Male_Troll_BOM_Scabbs_Mission6ExchangeF_Kurtrus_03, VO_Story_Hero_Cariel_Female_Human_BOM_Scabbs_Mission6ExchangeC_Cariel_01, VO_Story_Hero_Cariel_Female_Human_BOM_Scabbs_Mission6ExchangeD_Cariel_01, VO_Story_Hero_Cariel_Female_Human_BOM_Scabbs_Mission6ExchangeF_Cariel_02, VO_Story_Hero_Cariel_Female_Human_BOM_Scabbs_Mission6Select_01, VO_Story_Hero_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission6ExchangeD_03, VO_Story_Hero_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission6ExchangeE_02, VO_Story_Hero_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission6ExchangeF_Xyrella_01, VO_Story_Hero_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission6ExchangeF_Xyrella_03,
			VO_Story_Hero_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission6Victory_02, VO_Story_Hero_Guff_Male_Tauren_BOM_Scabbs_Mission6ExchangeD_02, VO_Story_Hero_Guff_Male_Tauren_BOM_Scabbs_Mission6ExchangeF_Tavish_01, VO_Story_Hero_Guff_Male_Tauren_BOM_Scabbs_Mission6ExchangeF_Tavish_03, VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Scabbs_Mission6ExchangeC_Kurtrus_01, VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Scabbs_Mission6ExchangeD_Kurtrus_01, VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Scabbs_Mission6ExchangeF_Kurtrus_02, VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Scabbs_Mission6Select_01, VO_Story_Hero_Rokara_Female_Orc_BOM_Scabbs_Mission6ExchangeF_Cariel_01, VO_Story_Hero_Rokara_Female_Orc_BOM_Scabbs_Mission6ExchangeF_Cariel_03,
			VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission6ExchangeA_01, VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission6ExchangeE_01, VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission6Intro_01, VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission6Victory_01, VO_Story_Hero_Smite_Male_Tauren_BOM_Scabbs_Mission6Death_01, VO_Story_Hero_Smite_Male_Tauren_BOM_Scabbs_Mission6EmoteResponse_01, VO_Story_Hero_Smite_Male_Tauren_BOM_Scabbs_Mission6ExchangeA_02, VO_Story_Hero_Smite_Male_Tauren_BOM_Scabbs_Mission6ExchangeB_01, VO_Story_Hero_Smite_Male_Tauren_BOM_Scabbs_Mission6ExchangeC_01, VO_Story_Hero_Smite_Male_Tauren_BOM_Scabbs_Mission6ExchangeG_01,
			VO_Story_Hero_Smite_Male_Tauren_BOM_Scabbs_Mission6HeroPower_01, VO_Story_Hero_Smite_Male_Tauren_BOM_Scabbs_Mission6HeroPower_02, VO_Story_Hero_Smite_Male_Tauren_BOM_Scabbs_Mission6HeroPower_03, VO_Story_Hero_Smite_Male_Tauren_BOM_Scabbs_Mission6Idle_01, VO_Story_Hero_Smite_Male_Tauren_BOM_Scabbs_Mission6Idle_02, VO_Story_Hero_Smite_Male_Tauren_BOM_Scabbs_Mission6Idle_03, VO_Story_Hero_Smite_Male_Tauren_BOM_Scabbs_Mission6Intro_02, VO_Story_Hero_Smite_Male_Tauren_BOM_Scabbs_Mission6Loss_01, VO_Story_Hero_Tavish_Male_Dwarf_BOM_Scabbs_Mission6ExchangeC_Tavish_01, VO_Story_Hero_Tavish_Male_Dwarf_BOM_Scabbs_Mission6ExchangeD_Tavish_01,
			VO_Story_Hero_Tavish_Male_Dwarf_BOM_Scabbs_Mission6ExchangeF_Tavish_02, VO_Story_Hero_Tavish_Male_Dwarf_BOM_Scabbs_Mission6ExchangeF_Tavish_04, VO_Story_Hero_Tavish_Male_Dwarf_BOM_Scabbs_Mission6Select_01, VO_Story_Hero_Xyrella_Female_Draenei_BOM_Scabbs_Mission6ExchangeC_Xyrella_01, VO_Story_Hero_Xyrella_Female_Draenei_BOM_Scabbs_Mission6ExchangeD_Xyrella_01, VO_Story_Hero_Xyrella_Female_Draenei_BOM_Scabbs_Mission6ExchangeF_Xyrella_02, VO_Story_Hero_Xyrella_Female_Draenei_BOM_Scabbs_Mission6Select_01, VO_PVPDR_Hero_Scabbs_Male_Gnome_Death_01
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
		Actor friendlyHeroPowerActor = GameState.Get().GetFriendlySidePlayer().GetHeroPower()
			.GetCard()
			.GetActor();
		switch (missionEvent)
		{
		case 516:
			yield return MissionPlaySound(enemyActor, VO_Story_Hero_Smite_Male_Tauren_BOM_Scabbs_Mission6Death_01);
			break;
		case 517:
			yield return MissionPlayVO(enemyActor, m_InGame_BossIdleLines);
			break;
		case 510:
			yield return MissionPlayVO(enemyActor, m_InGame_BossUsesHeroPowerLines);
			break;
		case 515:
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Smite_Male_Tauren_BOM_Scabbs_Mission6EmoteResponse_01);
			break;
		case 514:
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission6Intro_01);
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Smite_Male_Tauren_BOM_Scabbs_Mission6Intro_02);
			break;
		case 507:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Smite_Male_Tauren_BOM_Scabbs_Mission6Loss_01);
			GameState.Get().SetBusy(busy: false);
			break;
		case 505:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission6Victory_01);
			yield return MissionPlayVO(BOM_07_Scabbs_Dungeon.SWDawngraspMinion_BrassRing_Quote, VO_Story_Hero_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission6Victory_02);
			GameState.Get().SetBusy(busy: false);
			break;
		case 100:
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Smite_Male_Tauren_BOM_Scabbs_Mission6ExchangeG_01);
			break;
		case 101:
			GameState.Get().SetBusy(busy: true);
			if (HeroPowerIsCariel)
			{
				yield return MissionPlayVO(friendlyHeroPowerActor, VO_Story_Hero_Cariel_Female_Human_BOM_Scabbs_Mission6ExchangeD_Cariel_01);
			}
			if (HeroPowerIsKurtrus)
			{
				yield return MissionPlayVO(friendlyHeroPowerActor, VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Scabbs_Mission6ExchangeD_Kurtrus_01);
			}
			if (HeroPowerIsTavish)
			{
				yield return MissionPlayVO(friendlyHeroPowerActor, VO_Story_Hero_Tavish_Male_Dwarf_BOM_Scabbs_Mission6ExchangeD_Tavish_01);
			}
			if (HeroPowerIsXyrella)
			{
				yield return MissionPlayVO(friendlyHeroPowerActor, VO_Story_Hero_Xyrella_Female_Draenei_BOM_Scabbs_Mission6ExchangeD_Xyrella_01);
			}
			GameState.Get().SetBusy(busy: false);
			break;
		case 102:
			yield return MissionPlayVO("BOM_07_Scabbs_Dawngrasp_006t", VO_Story_Hero_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission6ExchangeD_03);
			break;
		case 1001:
			if (HeroPowerIsCariel)
			{
				yield return MissionPlayVO(friendlyHeroPowerActor, VO_Story_Hero_Cariel_Female_Human_BOM_Scabbs_Mission6Select_01);
			}
			if (HeroPowerIsKurtrus)
			{
				yield return MissionPlayVO(friendlyHeroPowerActor, VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Scabbs_Mission6Select_01);
			}
			if (HeroPowerIsTavish)
			{
				yield return MissionPlayVO(friendlyHeroPowerActor, VO_Story_Hero_Tavish_Male_Dwarf_BOM_Scabbs_Mission6Select_01);
			}
			if (HeroPowerIsXyrella)
			{
				yield return MissionPlayVO(friendlyHeroPowerActor, VO_Story_Hero_Xyrella_Female_Draenei_BOM_Scabbs_Mission6Select_01);
			}
			break;
		case 519:
			yield return MissionPlaySound(enemyActor, VO_PVPDR_Hero_Scabbs_Male_Gnome_Death_01);
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
		Actor friendlyHeroPowerActor = GameState.Get().GetFriendlySidePlayer().GetHeroPower()
			.GetCard()
			.GetActor();
		switch (turn)
		{
		case 3:
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission6ExchangeA_01);
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Smite_Male_Tauren_BOM_Scabbs_Mission6ExchangeA_02);
			break;
		case 5:
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Smite_Male_Tauren_BOM_Scabbs_Mission6ExchangeB_01);
			break;
		case 7:
			if (HeroPowerIsCariel)
			{
				yield return MissionPlayVO(friendlyHeroPowerActor, VO_Story_Hero_Cariel_Female_Human_BOM_Scabbs_Mission6ExchangeC_Cariel_01);
			}
			if (HeroPowerIsKurtrus)
			{
				yield return MissionPlayVO(friendlyHeroPowerActor, VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Scabbs_Mission6ExchangeC_Kurtrus_01);
			}
			if (HeroPowerIsTavish)
			{
				yield return MissionPlayVO(friendlyHeroPowerActor, VO_Story_Hero_Tavish_Male_Dwarf_BOM_Scabbs_Mission6ExchangeC_Tavish_01);
			}
			if (HeroPowerIsXyrella)
			{
				yield return MissionPlayVO(friendlyHeroPowerActor, VO_Story_Hero_Xyrella_Female_Draenei_BOM_Scabbs_Mission6ExchangeC_Xyrella_01);
			}
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Smite_Male_Tauren_BOM_Scabbs_Mission6ExchangeC_01);
			break;
		case 11:
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission6ExchangeE_01);
			yield return MissionPlayVO("BOM_07_Scabbs_Dawngrasp_006t", VO_Story_Hero_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission6ExchangeE_02);
			break;
		case 13:
			if (HeroPowerIsCariel)
			{
				yield return MissionPlayVO("BOM_07_Scabbs_Rokara_006t", BOM_07_Scabbs_Dungeon.Rokara_B_BrassRing_Quote, VO_Story_Hero_Rokara_Female_Orc_BOM_Scabbs_Mission6ExchangeF_Cariel_01);
				yield return MissionPlayVO(friendlyHeroPowerActor, VO_Story_Hero_Cariel_Female_Human_BOM_Scabbs_Mission6ExchangeF_Cariel_02);
				yield return MissionPlayVO("BOM_07_Scabbs_Rokara_006t", BOM_07_Scabbs_Dungeon.Rokara_B_BrassRing_Quote, VO_Story_Hero_Rokara_Female_Orc_BOM_Scabbs_Mission6ExchangeF_Cariel_03);
			}
			if (HeroPowerIsKurtrus)
			{
				yield return MissionPlayVO("BOM_07_Scabbs_Brukan_006t", BOM_07_Scabbs_Dungeon.Brukan_20_4_BrassRing_Quote, VO_Story_Hero_Brukan_Male_Troll_BOM_Scabbs_Mission6ExchangeF_Kurtrus_01);
				yield return MissionPlayVO(friendlyHeroPowerActor, VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Scabbs_Mission6ExchangeF_Kurtrus_02);
				yield return MissionPlayVO("BOM_07_Scabbs_Brukan_006t", BOM_07_Scabbs_Dungeon.Brukan_20_4_BrassRing_Quote, VO_Story_Hero_Brukan_Male_Troll_BOM_Scabbs_Mission6ExchangeF_Kurtrus_03);
			}
			if (HeroPowerIsTavish)
			{
				yield return MissionPlayVO("BOM_07_Scabbs_Guff_006t", BOM_07_Scabbs_Dungeon.Guff_BrassRing_Quote, VO_Story_Hero_Guff_Male_Tauren_BOM_Scabbs_Mission6ExchangeF_Tavish_01);
				yield return MissionPlayVO(friendlyHeroPowerActor, VO_Story_Hero_Tavish_Male_Dwarf_BOM_Scabbs_Mission6ExchangeF_Tavish_02);
				yield return MissionPlayVO("BOM_07_Scabbs_Guff_006t", BOM_07_Scabbs_Dungeon.Guff_BrassRing_Quote, VO_Story_Hero_Guff_Male_Tauren_BOM_Scabbs_Mission6ExchangeF_Tavish_03);
				yield return MissionPlayVO(friendlyHeroPowerActor, VO_Story_Hero_Tavish_Male_Dwarf_BOM_Scabbs_Mission6ExchangeF_Tavish_04);
			}
			if (HeroPowerIsXyrella)
			{
				yield return MissionPlayVO("BOM_07_Scabbs_Dawngrasp_006t", BOM_07_Scabbs_Dungeon.SWDawngraspMinion_BrassRing_Quote, VO_Story_Hero_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission6ExchangeF_Xyrella_01);
				yield return MissionPlayVO(friendlyHeroPowerActor, VO_Story_Hero_Xyrella_Female_Draenei_BOM_Scabbs_Mission6ExchangeF_Xyrella_02);
				yield return MissionPlayVO("BOM_07_Scabbs_Dawngrasp_006t", BOM_07_Scabbs_Dungeon.SWDawngraspMinion_BrassRing_Quote, VO_Story_Hero_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission6ExchangeF_Xyrella_03);
			}
			break;
		}
	}
}
