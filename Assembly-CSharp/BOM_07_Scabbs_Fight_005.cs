using System.Collections;
using System.Collections.Generic;

public class BOM_07_Scabbs_Fight_005 : BOM_07_Scabbs_Dungeon
{
	private static readonly AssetReference VO_Story_Hero_Cariel_Female_Human_BOM_Scabbs_Mission5ExchangeB_Cariel_02 = new AssetReference("VO_Story_Hero_Cariel_Female_Human_BOM_Scabbs_Mission5ExchangeB_Cariel_02.prefab:563c824451ba40caaaaa13afec28dc3c");

	private static readonly AssetReference VO_Story_Hero_Cariel_Female_Human_BOM_Scabbs_Mission5ExchangeC_Cariel_02 = new AssetReference("VO_Story_Hero_Cariel_Female_Human_BOM_Scabbs_Mission5ExchangeC_Cariel_02.prefab:e911da490e034aa691261780e0c2722b");

	private static readonly AssetReference VO_Story_Hero_Cariel_Female_Human_BOM_Scabbs_Mission5Select_01 = new AssetReference("VO_Story_Hero_Cariel_Female_Human_BOM_Scabbs_Mission5Select_01.prefab:35e502c6e2234f9c93a5d73bc5deaa2e");

	private static readonly AssetReference VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Scabbs_Mission5ExchangeB_Kurtrus_02 = new AssetReference("VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Scabbs_Mission5ExchangeB_Kurtrus_02.prefab:c29f8fa4b48141ddb872e4255f2b01d1");

	private static readonly AssetReference VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Scabbs_Mission5ExchangeC_Kurtrus_02 = new AssetReference("VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Scabbs_Mission5ExchangeC_Kurtrus_02.prefab:9e7ea8afb742457ba2424d9ddbc80597");

	private static readonly AssetReference VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Scabbs_Mission5Select_01 = new AssetReference("VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Scabbs_Mission5Select_01.prefab:26d6a75b04f04973a0f255eda6344a7a");

	private static readonly AssetReference VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission5ExchangeA_02 = new AssetReference("VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission5ExchangeA_02.prefab:812a146b5b0a4aa7b87c9859937a14bf");

	private static readonly AssetReference VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission5ExchangeB_Cariel_01 = new AssetReference("VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission5ExchangeB_Cariel_01.prefab:17d1afebec5f4d028380ca99b0e21613");

	private static readonly AssetReference VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission5ExchangeB_Kurtrus_01 = new AssetReference("VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission5ExchangeB_Kurtrus_01.prefab:2d31e6e28c234bb4868c359c18461c59");

	private static readonly AssetReference VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission5ExchangeB_Tavish_01 = new AssetReference("VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission5ExchangeB_Tavish_01.prefab:a05320e1e3d642e1ae820a0cdd7293d4");

	private static readonly AssetReference VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission5ExchangeB_Xyrella_01 = new AssetReference("VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission5ExchangeB_Xyrella_01.prefab:89ab017db2b54d7ab9a38f9e4a6e54b1");

	private static readonly AssetReference VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission5ExchangeC_Cariel_01 = new AssetReference("VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission5ExchangeC_Cariel_01.prefab:9f625351a66b4fddb4188476c56cabdf");

	private static readonly AssetReference VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission5ExchangeC_Kurtrus_01 = new AssetReference("VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission5ExchangeC_Kurtrus_01.prefab:94d1ea64b798435eae22df16cd588200");

	private static readonly AssetReference VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission5ExchangeC_Tavish_02 = new AssetReference("VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission5ExchangeC_Tavish_02.prefab:ac11b19f269a4b3d8f1058cbfc696c0a");

	private static readonly AssetReference VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission5ExchangeC_Xyrella_02 = new AssetReference("VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission5ExchangeC_Xyrella_02.prefab:803471307ff346e2899574cd04767876");

	private static readonly AssetReference VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission5ExchangeD_01 = new AssetReference("VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission5ExchangeD_01.prefab:0c8b7912f1ef4798b430484b8688928a");

	private static readonly AssetReference VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission5ExchangeD_03 = new AssetReference("VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission5ExchangeD_03.prefab:e117c0a2405d418ea64b7538b8de03c7");

	private static readonly AssetReference VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission5Intro_01 = new AssetReference("VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission5Intro_01.prefab:bdcb804b9d044689aa69c88c32bc2294");

	private static readonly AssetReference VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission5Victory_02 = new AssetReference("VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission5Victory_02.prefab:c9edd1e88343407b9249b688e9411bad");

	private static readonly AssetReference VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission5Victory_04 = new AssetReference("VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission5Victory_04.prefab:c4577aa426954fdea304de64d1acc2e1");

	private static readonly AssetReference VO_Story_Hero_Sneed_Male_Goblin_BOM_Scabbs_Mission5Death_01 = new AssetReference("VO_Story_Hero_Sneed_Male_Goblin_BOM_Scabbs_Mission5Death_01.prefab:94ba744a364e4b3b8aab85b175ac0004");

	private static readonly AssetReference VO_Story_Hero_Sneed_Male_Goblin_BOM_Scabbs_Mission5EmoteResponse_01 = new AssetReference("VO_Story_Hero_Sneed_Male_Goblin_BOM_Scabbs_Mission5EmoteResponse_01.prefab:38fb7f34659a434d8e20d86069ea7621");

	private static readonly AssetReference VO_Story_Hero_Sneed_Male_Goblin_BOM_Scabbs_Mission5ExchangeA_01 = new AssetReference("VO_Story_Hero_Sneed_Male_Goblin_BOM_Scabbs_Mission5ExchangeA_01.prefab:ef0b1701c94a41c58308abef6e3821d8");

	private static readonly AssetReference VO_Story_Hero_Sneed_Male_Goblin_BOM_Scabbs_Mission5ExchangeD_02 = new AssetReference("VO_Story_Hero_Sneed_Male_Goblin_BOM_Scabbs_Mission5ExchangeD_02.prefab:b7a347807cdb4ab897605a0ef695ffda");

	private static readonly AssetReference VO_Story_Hero_Sneed_Male_Goblin_BOM_Scabbs_Mission5HeroPower_01 = new AssetReference("VO_Story_Hero_Sneed_Male_Goblin_BOM_Scabbs_Mission5HeroPower_01.prefab:2145087bc3df4e6abbeab8f5daaf9c90");

	private static readonly AssetReference VO_Story_Hero_Sneed_Male_Goblin_BOM_Scabbs_Mission5HeroPower_02 = new AssetReference("VO_Story_Hero_Sneed_Male_Goblin_BOM_Scabbs_Mission5HeroPower_02.prefab:c573059587e04dafa08bbd4428307cfc");

	private static readonly AssetReference VO_Story_Hero_Sneed_Male_Goblin_BOM_Scabbs_Mission5HeroPower_03 = new AssetReference("VO_Story_Hero_Sneed_Male_Goblin_BOM_Scabbs_Mission5HeroPower_03.prefab:ca6729fe4a76433bb4ad3f0d624676b7");

	private static readonly AssetReference VO_Story_Hero_Sneed_Male_Goblin_BOM_Scabbs_Mission5HeroPower_04 = new AssetReference("VO_Story_Hero_Sneed_Male_Goblin_BOM_Scabbs_Mission5HeroPower_04.prefab:a98fe387aa1e4db9ba1b95b851287de9");

	private static readonly AssetReference VO_Story_Hero_Sneed_Male_Goblin_BOM_Scabbs_Mission5Idle_01 = new AssetReference("VO_Story_Hero_Sneed_Male_Goblin_BOM_Scabbs_Mission5Idle_01.prefab:952fe6b1d6c247cea5bfad5b88ca21d6");

	private static readonly AssetReference VO_Story_Hero_Sneed_Male_Goblin_BOM_Scabbs_Mission5Idle_02 = new AssetReference("VO_Story_Hero_Sneed_Male_Goblin_BOM_Scabbs_Mission5Idle_02.prefab:fa8668ff93b846078448e46f23adf7ca");

	private static readonly AssetReference VO_Story_Hero_Sneed_Male_Goblin_BOM_Scabbs_Mission5Idle_03 = new AssetReference("VO_Story_Hero_Sneed_Male_Goblin_BOM_Scabbs_Mission5Idle_03.prefab:e2bb814ec488489c8f38da50d8abef32");

	private static readonly AssetReference VO_Story_Hero_Sneed_Male_Goblin_BOM_Scabbs_Mission5Intro_02 = new AssetReference("VO_Story_Hero_Sneed_Male_Goblin_BOM_Scabbs_Mission5Intro_02.prefab:cc6ef543f8194ba8ba5cf47934b29482");

	private static readonly AssetReference VO_Story_Hero_Sneed_Male_Goblin_BOM_Scabbs_Mission5Loss_01 = new AssetReference("VO_Story_Hero_Sneed_Male_Goblin_BOM_Scabbs_Mission5Loss_01.prefab:5eccf0766b4440cca08c1bdd2e281b86");

	private static readonly AssetReference VO_Story_Hero_Sneed_Male_Goblin_BOM_Scabbs_Mission5Victory_01 = new AssetReference("VO_Story_Hero_Sneed_Male_Goblin_BOM_Scabbs_Mission5Victory_01.prefab:053a351ac78141ad8865d12eaed2f1fe");

	private static readonly AssetReference VO_Story_Hero_Tavish_Male_Dwarf_BOM_Scabbs_Mission5ExchangeB_Tavish_02 = new AssetReference("VO_Story_Hero_Tavish_Male_Dwarf_BOM_Scabbs_Mission5ExchangeB_Tavish_02.prefab:0abd298ba3c6446fa7a987cafa74e932");

	private static readonly AssetReference VO_Story_Hero_Tavish_Male_Dwarf_BOM_Scabbs_Mission5ExchangeC_Tavish_01 = new AssetReference("VO_Story_Hero_Tavish_Male_Dwarf_BOM_Scabbs_Mission5ExchangeC_Tavish_01.prefab:75eadd85da2241e6b0992df3bb299ad6");

	private static readonly AssetReference VO_Story_Hero_Tavish_Male_Dwarf_BOM_Scabbs_Mission5ExchangeC_Tavish_03 = new AssetReference("VO_Story_Hero_Tavish_Male_Dwarf_BOM_Scabbs_Mission5ExchangeC_Tavish_03.prefab:0308cec1dfea44db9efe8fa8ed165bcc");

	private static readonly AssetReference VO_Story_Hero_Tavish_Male_Dwarf_BOM_Scabbs_Mission5Select_01 = new AssetReference("VO_Story_Hero_Tavish_Male_Dwarf_BOM_Scabbs_Mission5Select_01.prefab:5ed2e77e59b34088b142949992528775");

	private static readonly AssetReference VO_Story_Hero_Tavish_Male_Dwarf_BOM_Scabbs_Mission5Victory_03 = new AssetReference("VO_Story_Hero_Tavish_Male_Dwarf_BOM_Scabbs_Mission5Victory_03.prefab:f9be7dc2790b43e688ca726a38c54548");

	private static readonly AssetReference VO_Story_Hero_Xyrella_Female_Draenei_BOM_Scabbs_Mission5ExchangeB_Xyrella_02 = new AssetReference("VO_Story_Hero_Xyrella_Female_Draenei_BOM_Scabbs_Mission5ExchangeB_Xyrella_02.prefab:ac0cf5a42652484597bc27bb8ebddd94");

	private static readonly AssetReference VO_Story_Hero_Xyrella_Female_Draenei_BOM_Scabbs_Mission5ExchangeC_Xyrella_01 = new AssetReference("VO_Story_Hero_Xyrella_Female_Draenei_BOM_Scabbs_Mission5ExchangeC_Xyrella_01.prefab:9b4fdeda6254433bb50f76425e526e4f");

	private static readonly AssetReference VO_Story_Hero_Xyrella_Female_Draenei_BOM_Scabbs_Mission5Select_01 = new AssetReference("VO_Story_Hero_Xyrella_Female_Draenei_BOM_Scabbs_Mission5Select_01.prefab:5d4938d2144c4f51a7faaa5376b7e681");

	private static readonly AssetReference VO_Story_Hero_Cariel_Female_Human_BOM_Scabbs_HeroPower_01 = new AssetReference("VO_Story_Hero_Cariel_Female_Human_BOM_Scabbs_HeroPower_01.prefab:c40bb67529e2f75488a687b29e366dce");

	private static readonly AssetReference VO_Story_Hero_Cariel_Female_Human_BOM_Scabbs_HeroPower_02 = new AssetReference("VO_Story_Hero_Cariel_Female_Human_BOM_Scabbs_HeroPower_02.prefab:1bd2a973a10a50f449fa853434c5c19c");

	private static readonly AssetReference VO_Story_Hero_Cariel_Female_Human_BOM_Scabbs_HeroPower_03 = new AssetReference("VO_Story_Hero_Cariel_Female_Human_BOM_Scabbs_HeroPower_03.prefab:de12dc2c97279184aae3cc11e1756c0c");

	private static readonly AssetReference VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Scabbs_HeroPower_01 = new AssetReference("VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Scabbs_HeroPower_01.prefab:782455e872e6bc147938f97842d9817e");

	private static readonly AssetReference VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Scabbs_HeroPower_02 = new AssetReference("VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Scabbs_HeroPower_02.prefab:6f7943e790fab104d86b4d19c6ce975b");

	private static readonly AssetReference VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Scabbs_HeroPower_03 = new AssetReference("VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Scabbs_HeroPower_03.prefab:ad35989c054538b4da47fd868b74a657");

	private static readonly AssetReference VO_Story_Hero_Tavish_Male_Dwarf_BOM_Scabbs_HeroPower_01 = new AssetReference("VO_Story_Hero_Tavish_Male_Dwarf_BOM_Scabbs_HeroPower_01.prefab:41c1f8fa116752f46af51caeb6caeed9");

	private static readonly AssetReference VO_Story_Hero_Tavish_Male_Dwarf_BOM_Scabbs_HeroPower_02 = new AssetReference("VO_Story_Hero_Tavish_Male_Dwarf_BOM_Scabbs_HeroPower_02.prefab:c5d62462456c86d4991db76dc7a2bde1");

	private static readonly AssetReference VO_Story_Hero_Tavish_Male_Dwarf_BOM_Scabbs_HeroPower_03 = new AssetReference("VO_Story_Hero_Tavish_Male_Dwarf_BOM_Scabbs_HeroPower_03.prefab:2b8ab2e036415d14e934f46f7f1c2d6d");

	private static readonly AssetReference VO_Story_Hero_Xyrella_Female_Draenei_BOM_Scabbs_HeroPower_01 = new AssetReference("VO_Story_Hero_Xyrella_Female_Draenei_BOM_Scabbs_HeroPower_01.prefab:6cc5a32c122f74f47afad43a2625ccab");

	private static readonly AssetReference VO_Story_Hero_Xyrella_Female_Draenei_BOM_Scabbs_HeroPower_02 = new AssetReference("VO_Story_Hero_Xyrella_Female_Draenei_BOM_Scabbs_HeroPower_02.prefab:56dc1b62b35fd6141bdaefa9e544381f");

	private static readonly AssetReference VO_Story_Hero_Xyrella_Female_Draenei_BOM_Scabbs_HeroPower_03 = new AssetReference("VO_Story_Hero_Xyrella_Female_Draenei_BOM_Scabbs_HeroPower_03.prefab:1bf8a68524d3dad41bc7755a7987d208");

	private static readonly AssetReference VO_PVPDR_Hero_Scabbs_Male_Gnome_Death_01 = new AssetReference("VO_PVPDR_Hero_Scabbs_Male_Gnome_Death_01.prefab:85e10a0b219b1974aaa959bd523c554b");

	private List<string> m_VO_Story_Hero_Cariel_Female_Human_BOM_Scabbs_Mission5SelectLines = new List<string>();

	private List<string> m_InGame_PlayerUsesHeroPowerLines_Cariel = new List<string> { VO_Story_Hero_Cariel_Female_Human_BOM_Scabbs_HeroPower_01, VO_Story_Hero_Cariel_Female_Human_BOM_Scabbs_HeroPower_02, VO_Story_Hero_Cariel_Female_Human_BOM_Scabbs_HeroPower_03 };

	private List<string> m_InGame_PlayerUsesHeroPowerLines_Kurtrus = new List<string> { VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Scabbs_HeroPower_01, VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Scabbs_HeroPower_02, VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Scabbs_HeroPower_03 };

	private List<string> m_InGame_PlayerUsesHeroPowerLines_Tavish = new List<string> { VO_Story_Hero_Tavish_Male_Dwarf_BOM_Scabbs_HeroPower_01, VO_Story_Hero_Tavish_Male_Dwarf_BOM_Scabbs_HeroPower_02, VO_Story_Hero_Tavish_Male_Dwarf_BOM_Scabbs_HeroPower_03 };

	private List<string> m_InGame_PlayerUsesHeroPowerLines_Xyrella = new List<string> { VO_Story_Hero_Xyrella_Female_Draenei_BOM_Scabbs_HeroPower_01, VO_Story_Hero_Xyrella_Female_Draenei_BOM_Scabbs_HeroPower_02, VO_Story_Hero_Xyrella_Female_Draenei_BOM_Scabbs_HeroPower_03 };

	private List<string> m_InGame_BossUsesHeroPowerLines = new List<string> { VO_Story_Hero_Sneed_Male_Goblin_BOM_Scabbs_Mission5HeroPower_01, VO_Story_Hero_Sneed_Male_Goblin_BOM_Scabbs_Mission5HeroPower_02, VO_Story_Hero_Sneed_Male_Goblin_BOM_Scabbs_Mission5HeroPower_03, VO_Story_Hero_Sneed_Male_Goblin_BOM_Scabbs_Mission5HeroPower_04 };

	private List<string> m_InGame_BossIdleLines = new List<string> { VO_Story_Hero_Sneed_Male_Goblin_BOM_Scabbs_Mission5Idle_01, VO_Story_Hero_Sneed_Male_Goblin_BOM_Scabbs_Mission5Idle_02, VO_Story_Hero_Sneed_Male_Goblin_BOM_Scabbs_Mission5Idle_03 };

	private HashSet<string> m_playedLines = new HashSet<string>();

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> m_SoundFiles = new List<string>
		{
			VO_Story_Hero_Cariel_Female_Human_BOM_Scabbs_HeroPower_01, VO_Story_Hero_Cariel_Female_Human_BOM_Scabbs_HeroPower_02, VO_Story_Hero_Cariel_Female_Human_BOM_Scabbs_HeroPower_03, VO_Story_Hero_Cariel_Female_Human_BOM_Scabbs_Mission5ExchangeB_Cariel_02, VO_Story_Hero_Cariel_Female_Human_BOM_Scabbs_Mission5ExchangeC_Cariel_02, VO_Story_Hero_Cariel_Female_Human_BOM_Scabbs_Mission5Select_01, VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Scabbs_HeroPower_01, VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Scabbs_HeroPower_02, VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Scabbs_HeroPower_03, VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Scabbs_Mission5ExchangeB_Kurtrus_02,
			VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Scabbs_Mission5ExchangeC_Kurtrus_02, VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Scabbs_Mission5Select_01, VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission5ExchangeA_02, VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission5ExchangeB_Cariel_01, VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission5ExchangeB_Kurtrus_01, VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission5ExchangeB_Tavish_01, VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission5ExchangeB_Xyrella_01, VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission5ExchangeC_Cariel_01, VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission5ExchangeC_Kurtrus_01, VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission5ExchangeC_Tavish_02,
			VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission5ExchangeC_Xyrella_02, VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission5ExchangeD_01, VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission5ExchangeD_03, VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission5Intro_01, VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission5Victory_02, VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission5Victory_04, VO_Story_Hero_Sneed_Male_Goblin_BOM_Scabbs_Mission5Death_01, VO_Story_Hero_Sneed_Male_Goblin_BOM_Scabbs_Mission5EmoteResponse_01, VO_Story_Hero_Sneed_Male_Goblin_BOM_Scabbs_Mission5ExchangeA_01, VO_Story_Hero_Sneed_Male_Goblin_BOM_Scabbs_Mission5ExchangeD_02,
			VO_Story_Hero_Sneed_Male_Goblin_BOM_Scabbs_Mission5HeroPower_01, VO_Story_Hero_Sneed_Male_Goblin_BOM_Scabbs_Mission5HeroPower_02, VO_Story_Hero_Sneed_Male_Goblin_BOM_Scabbs_Mission5HeroPower_03, VO_Story_Hero_Sneed_Male_Goblin_BOM_Scabbs_Mission5HeroPower_04, VO_Story_Hero_Sneed_Male_Goblin_BOM_Scabbs_Mission5Idle_01, VO_Story_Hero_Sneed_Male_Goblin_BOM_Scabbs_Mission5Idle_02, VO_Story_Hero_Sneed_Male_Goblin_BOM_Scabbs_Mission5Idle_03, VO_Story_Hero_Sneed_Male_Goblin_BOM_Scabbs_Mission5Intro_02, VO_Story_Hero_Sneed_Male_Goblin_BOM_Scabbs_Mission5Loss_01, VO_Story_Hero_Sneed_Male_Goblin_BOM_Scabbs_Mission5Victory_01,
			VO_Story_Hero_Tavish_Male_Dwarf_BOM_Scabbs_HeroPower_01, VO_Story_Hero_Tavish_Male_Dwarf_BOM_Scabbs_HeroPower_02, VO_Story_Hero_Tavish_Male_Dwarf_BOM_Scabbs_HeroPower_03, VO_Story_Hero_Tavish_Male_Dwarf_BOM_Scabbs_Mission5ExchangeB_Tavish_02, VO_Story_Hero_Tavish_Male_Dwarf_BOM_Scabbs_Mission5ExchangeC_Tavish_01, VO_Story_Hero_Tavish_Male_Dwarf_BOM_Scabbs_Mission5ExchangeC_Tavish_03, VO_Story_Hero_Tavish_Male_Dwarf_BOM_Scabbs_Mission5Select_01, VO_Story_Hero_Tavish_Male_Dwarf_BOM_Scabbs_Mission5Victory_03, VO_Story_Hero_Xyrella_Female_Draenei_BOM_Scabbs_HeroPower_01, VO_Story_Hero_Xyrella_Female_Draenei_BOM_Scabbs_HeroPower_02,
			VO_Story_Hero_Xyrella_Female_Draenei_BOM_Scabbs_HeroPower_03, VO_Story_Hero_Xyrella_Female_Draenei_BOM_Scabbs_Mission5ExchangeB_Xyrella_02, VO_Story_Hero_Xyrella_Female_Draenei_BOM_Scabbs_Mission5ExchangeC_Xyrella_01, VO_Story_Hero_Xyrella_Female_Draenei_BOM_Scabbs_Mission5Select_01, VO_PVPDR_Hero_Scabbs_Male_Gnome_Death_01
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
			yield return MissionPlaySound(enemyActor, VO_Story_Hero_Sneed_Male_Goblin_BOM_Scabbs_Mission5Death_01);
			break;
		case 517:
			yield return MissionPlayVO(enemyActor, m_InGame_BossIdleLines);
			break;
		case 510:
			yield return MissionPlayVO(enemyActor, m_InGame_BossUsesHeroPowerLines);
			break;
		case 515:
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Sneed_Male_Goblin_BOM_Scabbs_Mission5EmoteResponse_01);
			break;
		case 514:
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission5Intro_01);
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Sneed_Male_Goblin_BOM_Scabbs_Mission5Intro_02);
			break;
		case 507:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Sneed_Male_Goblin_BOM_Scabbs_Mission5Loss_01);
			GameState.Get().SetBusy(busy: false);
			break;
		case 508:
			if (HeroPowerIsCariel)
			{
				yield return MissionPlayVO(friendlyHeroPowerActor, m_InGame_PlayerUsesHeroPowerLines_Cariel);
			}
			if (HeroPowerIsKurtrus)
			{
				yield return MissionPlayVO(friendlyHeroPowerActor, m_InGame_PlayerUsesHeroPowerLines_Kurtrus);
			}
			if (HeroPowerIsTavish)
			{
				yield return MissionPlayVO(friendlyHeroPowerActor, m_InGame_PlayerUsesHeroPowerLines_Tavish);
			}
			if (HeroPowerIsXyrella)
			{
				yield return MissionPlayVO(friendlyHeroPowerActor, m_InGame_PlayerUsesHeroPowerLines_Xyrella);
			}
			break;
		case 505:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission5Victory_02);
			yield return MissionPlayVO(BOM_07_Scabbs_Dungeon.Tavish4_BrassRing_Quote, VO_Story_Hero_Tavish_Male_Dwarf_BOM_Scabbs_Mission5Victory_03);
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission5Victory_04);
			GameState.Get().SetBusy(busy: false);
			break;
		case 504:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Sneed_Male_Goblin_BOM_Scabbs_Mission5Victory_01);
			GameState.Get().SetBusy(busy: false);
			break;
		case 1001:
			if (HeroPowerIsCariel)
			{
				yield return MissionPlayVO(friendlyHeroPowerActor, VO_Story_Hero_Cariel_Female_Human_BOM_Scabbs_Mission5Select_01);
			}
			if (HeroPowerIsKurtrus)
			{
				yield return MissionPlayVO(friendlyHeroPowerActor, VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Scabbs_Mission5Select_01);
			}
			if (HeroPowerIsTavish)
			{
				yield return MissionPlayVO(friendlyHeroPowerActor, VO_Story_Hero_Tavish_Male_Dwarf_BOM_Scabbs_Mission5Select_01);
			}
			if (HeroPowerIsXyrella)
			{
				yield return MissionPlayVO(friendlyHeroPowerActor, VO_Story_Hero_Xyrella_Female_Draenei_BOM_Scabbs_Mission5Select_01);
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
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Sneed_Male_Goblin_BOM_Scabbs_Mission5ExchangeA_01);
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission5ExchangeA_02);
			break;
		case 7:
			if (HeroPowerIsCariel)
			{
				yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission5ExchangeB_Cariel_01);
				yield return MissionPlayVO(friendlyHeroPowerActor, VO_Story_Hero_Cariel_Female_Human_BOM_Scabbs_Mission5ExchangeB_Cariel_02);
			}
			if (HeroPowerIsKurtrus)
			{
				yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission5ExchangeB_Kurtrus_01);
				yield return MissionPlayVO(friendlyHeroPowerActor, VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Scabbs_Mission5ExchangeB_Kurtrus_02);
			}
			if (HeroPowerIsTavish)
			{
				yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission5ExchangeB_Tavish_01);
				yield return MissionPlayVO(friendlyHeroPowerActor, VO_Story_Hero_Tavish_Male_Dwarf_BOM_Scabbs_Mission5ExchangeB_Tavish_02);
			}
			if (HeroPowerIsXyrella)
			{
				yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission5ExchangeB_Xyrella_01);
				yield return MissionPlayVO(friendlyHeroPowerActor, VO_Story_Hero_Xyrella_Female_Draenei_BOM_Scabbs_Mission5ExchangeB_Xyrella_02);
			}
			break;
		case 11:
			if (HeroPowerIsCariel)
			{
				yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission5ExchangeC_Cariel_01);
				yield return MissionPlayVO(friendlyHeroPowerActor, VO_Story_Hero_Cariel_Female_Human_BOM_Scabbs_Mission5ExchangeC_Cariel_02);
			}
			if (HeroPowerIsKurtrus)
			{
				yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission5ExchangeC_Kurtrus_01);
				yield return MissionPlayVO(friendlyHeroPowerActor, VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Scabbs_Mission5ExchangeC_Kurtrus_02);
			}
			if (HeroPowerIsTavish)
			{
				yield return MissionPlayVO(friendlyHeroPowerActor, VO_Story_Hero_Tavish_Male_Dwarf_BOM_Scabbs_Mission5ExchangeC_Tavish_01);
				yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission5ExchangeC_Tavish_02);
				yield return MissionPlayVO(friendlyHeroPowerActor, VO_Story_Hero_Tavish_Male_Dwarf_BOM_Scabbs_Mission5ExchangeC_Tavish_03);
			}
			if (HeroPowerIsXyrella)
			{
				yield return MissionPlayVO(friendlyHeroPowerActor, VO_Story_Hero_Xyrella_Female_Draenei_BOM_Scabbs_Mission5ExchangeC_Xyrella_01);
				yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission5ExchangeC_Xyrella_02);
			}
			break;
		case 15:
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission5ExchangeD_01);
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Sneed_Male_Goblin_BOM_Scabbs_Mission5ExchangeD_02);
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission5ExchangeD_03);
			break;
		}
	}
}
