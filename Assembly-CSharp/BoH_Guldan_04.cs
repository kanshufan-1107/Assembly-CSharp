using System.Collections;
using System.Collections.Generic;
using Blizzard.T5.Core;
using UnityEngine;

public class BoH_Guldan_04 : BoH_Guldan_Dungeon
{
	private static Map<GameEntityOption, bool> s_booleanOptions = InitBooleanOptions();

	private static readonly AssetReference VO_Story_Hero_Blackhand_Male_Orc_Story_Guldan_Mission4EmoteResponse_01 = new AssetReference("VO_Story_Hero_Blackhand_Male_Orc_Story_Guldan_Mission4EmoteResponse_01.prefab:454f4954cf1af3d46bd3e72bba850d35");

	private static readonly AssetReference VO_Story_Hero_Blackhand_Male_Orc_Story_Guldan_Mission4ExchangeB_01 = new AssetReference("VO_Story_Hero_Blackhand_Male_Orc_Story_Guldan_Mission4ExchangeB_01.prefab:dd909894b3917924a846202aa476c434");

	private static readonly AssetReference VO_Story_Hero_Blackhand_Male_Orc_Story_Guldan_Mission4ExchangeB_02 = new AssetReference("VO_Story_Hero_Blackhand_Male_Orc_Story_Guldan_Mission4ExchangeB_02.prefab:ea1f5b2cad791814ab2c8d7dbff25113");

	private static readonly AssetReference VO_Story_Hero_Blackhand_Male_Orc_Story_Guldan_Mission4ExchangeC_01 = new AssetReference("VO_Story_Hero_Blackhand_Male_Orc_Story_Guldan_Mission4ExchangeC_01.prefab:a63653e1d5bfd0f4fb309c7ed56d8794");

	private static readonly AssetReference VO_Story_Hero_Blackhand_Male_Orc_Story_Guldan_Mission4Idle_01 = new AssetReference("VO_Story_Hero_Blackhand_Male_Orc_Story_Guldan_Mission4Idle_01.prefab:70c12d1a8710d3c4a801857e052ab5e8");

	private static readonly AssetReference VO_Story_Hero_Blackhand_Male_Orc_Story_Guldan_Mission4Idle_02 = new AssetReference("VO_Story_Hero_Blackhand_Male_Orc_Story_Guldan_Mission4Idle_02.prefab:764681b095126584c94e691380b3bbe0");

	private static readonly AssetReference VO_Story_Hero_Blackhand_Male_Orc_Story_Guldan_Mission4Idle_03 = new AssetReference("VO_Story_Hero_Blackhand_Male_Orc_Story_Guldan_Mission4Idle_03.prefab:8bb8febe6b222314281b69a79bfc20fb");

	private static readonly AssetReference VO_Story_Hero_Blackhand_Male_Orc_Story_Guldan_Mission4Loss_01 = new AssetReference("VO_Story_Hero_Blackhand_Male_Orc_Story_Guldan_Mission4Loss_01.prefab:06e520c5728e8c9458478f1488f992e7");

	private static readonly AssetReference VO_Story_Hero_Blackhand_Male_Orc_Story_Guldan_Mission4Victory_02 = new AssetReference("VO_Story_Hero_Blackhand_Male_Orc_Story_Guldan_Mission4Victory_02.prefab:c3dc7ba6ab0dfe94fb62c8bcba118b92");

	private static readonly AssetReference VO_Story_Hero_Durotan_Male_Orc_Story_Guldan_Mission4EmoteResponse_01 = new AssetReference("VO_Story_Hero_Durotan_Male_Orc_Story_Guldan_Mission4EmoteResponse_01.prefab:b9a4eb225ffc1134aaf24598ed50007e");

	private static readonly AssetReference VO_Story_Hero_Durotan_Male_Orc_Story_Guldan_Mission4ExchangeD_01 = new AssetReference("VO_Story_Hero_Durotan_Male_Orc_Story_Guldan_Mission4ExchangeD_01.prefab:be572ece28469344cbfa2f8adb7966b9");

	private static readonly AssetReference VO_Story_Hero_Durotan_Male_Orc_Story_Guldan_Mission4ExchangeD_03 = new AssetReference("VO_Story_Hero_Durotan_Male_Orc_Story_Guldan_Mission4ExchangeD_03.prefab:45c196e1e9c7f7244bc009b57a701494");

	private static readonly AssetReference VO_Story_Hero_Durotan_Male_Orc_Story_Guldan_Mission4ExchangeE_02 = new AssetReference("VO_Story_Hero_Durotan_Male_Orc_Story_Guldan_Mission4ExchangeE_02.prefab:f8ec226b89b07c04ea44e5e52762f887");

	private static readonly AssetReference VO_Story_Hero_Durotan_Male_Orc_Story_Guldan_Mission4Idle_01 = new AssetReference("VO_Story_Hero_Durotan_Male_Orc_Story_Guldan_Mission4Idle_01.prefab:bc1977a3c310bdb4faf481a64c1e573d");

	private static readonly AssetReference VO_Story_Hero_Durotan_Male_Orc_Story_Guldan_Mission4Idle_02 = new AssetReference("VO_Story_Hero_Durotan_Male_Orc_Story_Guldan_Mission4Idle_02.prefab:5282d1d8319f5e64a8fefccb608b1436");

	private static readonly AssetReference VO_Story_Hero_Durotan_Male_Orc_Story_Guldan_Mission4Idle_03 = new AssetReference("VO_Story_Hero_Durotan_Male_Orc_Story_Guldan_Mission4Idle_03.prefab:56b284ad1409dcc4e9c8a6468205e10f");

	private static readonly AssetReference VO_Story_Hero_Durotan_Male_Orc_Story_Guldan_Mission4Loss_01 = new AssetReference("VO_Story_Hero_Durotan_Male_Orc_Story_Guldan_Mission4Loss_01.prefab:81f91a91c61c3a04c93ba8081ef74e04");

	private static readonly AssetReference VO_Story_Hero_Grommash_Male_Orc_Story_Guldan_Mission4EmoteResponse_01 = new AssetReference("VO_Story_Hero_Grommash_Male_Orc_Story_Guldan_Mission4EmoteResponse_01.prefab:464505364ddeb4740aa5b14e8e893789");

	private static readonly AssetReference VO_Story_Hero_Grommash_Male_Orc_Story_Guldan_Mission4Idle_01 = new AssetReference("VO_Story_Hero_Grommash_Male_Orc_Story_Guldan_Mission4Idle_01.prefab:ef4bb83b5dbe6fa478355aea686a342e");

	private static readonly AssetReference VO_Story_Hero_Grommash_Male_Orc_Story_Guldan_Mission4Idle_02 = new AssetReference("VO_Story_Hero_Grommash_Male_Orc_Story_Guldan_Mission4Idle_02.prefab:e3c409dda06daeb41950a63114ed3abf");

	private static readonly AssetReference VO_Story_Hero_Grommash_Male_Orc_Story_Guldan_Mission4Idle_03 = new AssetReference("VO_Story_Hero_Grommash_Male_Orc_Story_Guldan_Mission4Idle_03.prefab:f5aeb3d44fe9c0048950ae1d19b99627");

	private static readonly AssetReference VO_Story_Hero_Grommash_Male_Orc_Story_Guldan_Mission4Intro_03 = new AssetReference("VO_Story_Hero_Grommash_Male_Orc_Story_Guldan_Mission4Intro_03.prefab:64b389ce819f1434ea3ca1844b63e029");

	private static readonly AssetReference VO_Story_Hero_Grommash_Male_Orc_Story_Guldan_Mission4Loss_01 = new AssetReference("VO_Story_Hero_Grommash_Male_Orc_Story_Guldan_Mission4Loss_01.prefab:3bd8f17fc8de20d458e5a039fb9465cd");

	private static readonly AssetReference VO_Story_Hero_Grommash_Male_Orc_Story_Guldan_Mission4Victory_03 = new AssetReference("VO_Story_Hero_Grommash_Male_Orc_Story_Guldan_Mission4Victory_03.prefab:aab4c5194c716dc4ca33d9b25e96ea9e");

	private static readonly AssetReference VO_Story_Hero_Grommash_Male_Orc_Story_Guldan_Mission4Enrage = new AssetReference("VO_Story_Hero_Grommash_Male_Orc_Story_Guldan_Mission4Enrage.prefab:da6e15255c0e30d4fb755e2cbf8fce69");

	private static readonly AssetReference VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission4Entice_01 = new AssetReference("VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission4Entice_01.prefab:e83967de96b21b44789d8304d65d01ef");

	private static readonly AssetReference VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission4ExchangeA_01 = new AssetReference("VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission4ExchangeA_01.prefab:9f3b04d8be1024e4d92f6c872297b911");

	private static readonly AssetReference VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission4ExchangeD_01 = new AssetReference("VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission4ExchangeD_01.prefab:e0f000680cbab424d82ff289f9ec5195");

	private static readonly AssetReference VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission4ExchangeD_03 = new AssetReference("VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission4ExchangeD_03.prefab:18a949084ea7b2645abcd507366e46a3");

	private static readonly AssetReference VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission4ExchangeE_01 = new AssetReference("VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission4ExchangeE_01.prefab:3333ca8b99568ed4899d85680e85542f");

	private static readonly AssetReference VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission4ExchangeE_03 = new AssetReference("VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission4ExchangeE_03.prefab:8bdf5b3cfac64c74591984437cd0eb7d");

	private static readonly AssetReference VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission4Intro_01 = new AssetReference("VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission4Intro_01.prefab:f3d740cfbffea9f489485623e5672242");

	private static readonly AssetReference VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission4Intro_02 = new AssetReference("VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission4Intro_02.prefab:4245a95039db62f4ca4f1b02ae5404fa");

	private static readonly AssetReference VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission4Plead_01 = new AssetReference("VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission4Plead_01.prefab:72ff809c7d07fcd428d853eb5d13d71f");

	private static readonly AssetReference VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission4Threaten_01 = new AssetReference("VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission4Threaten_01.prefab:7578bc771fece5746a547f40ed0c5f24");

	private static readonly AssetReference VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission4Victory_01 = new AssetReference("VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission4Victory_01.prefab:b55d46f37d6d0284ba56b4ac1713d100");

	private static readonly AssetReference VO_HERO_07e_Male_Orc_Attack_01 = new AssetReference("VO_HERO_07e_Male_Orc_Attack_01.prefab:f117e76df5791974997058617aea12c9");

	private static readonly AssetReference VO_HERO_07g_Male_Orc_Greetings_01 = new AssetReference("VO_HERO_07g_Male_Orc_Greetings_01.prefab:cb5086e8fefff9945b59fddd2b34a2d5");

	private static readonly AssetReference VO_HERO_07f_Male_Orc_Threaten_01 = new AssetReference("VO_HERO_07f_Male_Orc_Threaten_01.prefab:d7340620efdabe94abedbee2965c0a89");

	private static readonly AssetReference VO_Story_Minion_Fenris_Male_Orc_Story_Guldan_Mission4Drink_01 = new AssetReference("VO_Story_Minion_Fenris_Male_Orc_Story_Guldan_Mission4Drink_01.prefab:03b5b31e15add924d9fc0b10a19d60c5");

	private static readonly AssetReference VO_Story_Minion_Fenris_Male_Orc_Story_Guldan_Mission4HeroPower_03 = new AssetReference("VO_Story_Minion_Fenris_Male_Orc_Story_Guldan_Mission4HeroPower_03.prefab:ffa6d72dd56bbe249908a1d7ec9d7445");

	private static readonly AssetReference VO_Story_Minion_Fenris_Male_Orc_Story_Guldan_Mission4Idle_01 = new AssetReference("VO_Story_Minion_Fenris_Male_Orc_Story_Guldan_Mission4Idle_01.prefab:e9d956e9eaa9b6c40a21e7d11b7b2b56");

	private static readonly AssetReference VO_Story_Minion_Fenris_Male_Orc_Story_Guldan_Mission4Idle_02 = new AssetReference("VO_Story_Minion_Fenris_Male_Orc_Story_Guldan_Mission4Idle_02.prefab:49279813c48c29042ad04df3337187c2");

	private static readonly AssetReference VO_Story_Minion_Fenris_Male_Orc_Story_Guldan_Mission4Idle_03 = new AssetReference("VO_Story_Minion_Fenris_Male_Orc_Story_Guldan_Mission4Idle_03.prefab:c2f1eeb810d11d14ca7b1e2949068575");

	private static readonly AssetReference VO_Story_Minion_Fenris_Male_Orc_Story_Guldan_Mission4Loss_01 = new AssetReference("VO_Story_Minion_Fenris_Male_Orc_Story_Guldan_Mission4Loss_01.prefab:407a9c102a352f24bb594c40e47f3780");

	private static readonly AssetReference VO_Story_Minion_Hurkan_Male_Orc_Drink_01 = new AssetReference("VO_Story_Minion_Hurkan_Male_Orc_Drink_01.prefab:d6e4cdecce72ad04ca2e88b98c2b744d");

	private static readonly AssetReference VO_Story_Minion_Hurkan_Male_Orc_HeroPower_01 = new AssetReference("VO_Story_Minion_Hurkan_Male_Orc_HeroPower_01.prefab:291c04a44b17c604e858fe13fcfd0978");

	private static readonly AssetReference VO_Story_Minion_Hurkan_Male_Orc_Idle_01 = new AssetReference("VO_Story_Minion_Hurkan_Male_Orc_Idle_01.prefab:acb343cf87a806c46b7bdfad027b9db6");

	private static readonly AssetReference VO_Story_Minion_Hurkan_Male_Orc_Idle_02 = new AssetReference("VO_Story_Minion_Hurkan_Male_Orc_Idle_02.prefab:065608e62fb5afb4cbc77dfdcad1e3ba");

	private static readonly AssetReference VO_Story_Minion_Hurkan_Male_Orc_Idle_03 = new AssetReference("VO_Story_Minion_Hurkan_Male_Orc_Idle_03.prefab:1ae3ef454f15b9a4b8b229842d63034d");

	private static readonly AssetReference VO_Story_Minion_Hurkan_Male_Orc_Loss_01 = new AssetReference("VO_Story_Minion_Hurkan_Male_Orc_Loss_01.prefab:2f86d71679195af46923e9aa9d67bf36");

	private static readonly AssetReference VO_Story_Minion_Kargath_Male_Orc_Attack_01 = new AssetReference("VO_Story_Minion_Kargath_Male_Orc_Attack_01.prefab:6e4bd6380de23734084cd1aa7c0de63a");

	private static readonly AssetReference VO_Story_Minion_Kargath_Male_Orc_EmoteResponse_01 = new AssetReference("VO_Story_Minion_Kargath_Male_Orc_EmoteResponse_01.prefab:25e558f007a94de439ff938634c56b53");

	private static readonly AssetReference VO_Story_Minion_Kargath_Male_Orc_Idle_01 = new AssetReference("VO_Story_Minion_Kargath_Male_Orc_Idle_01.prefab:1072d68777a008d459b6134cbe76ecf0");

	private static readonly AssetReference VO_Story_Minion_Kargath_Male_Orc_Idle_02 = new AssetReference("VO_Story_Minion_Kargath_Male_Orc_Idle_02.prefab:e48b1ead7bd50f6498f3bfcb1d7e5169");

	private static readonly AssetReference VO_Story_Minion_Kargath_Male_Orc_Idle_03 = new AssetReference("VO_Story_Minion_Kargath_Male_Orc_Idle_03.prefab:814e3d5bf22ce4546a356ab51fcd6b7c");

	private static readonly AssetReference VO_Story_Minion_Kargath_Male_Orc_Loss_01 = new AssetReference("VO_Story_Minion_Kargath_Male_Orc_Loss_01.prefab:7a170f0fb7032fd41bee1fdab3f830a0");

	private static readonly AssetReference VO_Story_Minion_Kargath_Male_Orc_Play_01 = new AssetReference("VO_Story_Minion_Kargath_Male_Orc_Play_01.prefab:d0bb875ead1297442b8e599b8a96dc1d");

	private static readonly AssetReference VO_Story_Minion_Kilrogg_Male_Orc_Drink_01 = new AssetReference("VO_Story_Minion_Kilrogg_Male_Orc_Drink_01.prefab:1d2403937b549cf45a1cc5de6a1a6559");

	private static readonly AssetReference VO_Story_Minion_Kilrogg_Male_Orc_HeroPower_03 = new AssetReference("VO_Story_Minion_Kilrogg_Male_Orc_HeroPower_03.prefab:1693f82da1e218645b028edc153611dd");

	private static readonly AssetReference VO_Story_Minion_Kilrogg_Male_Orc_Idle_01 = new AssetReference("VO_Story_Minion_Kilrogg_Male_Orc_Idle_01.prefab:ff06f7c8ab3cd08449ceb24a1a089bbd");

	private static readonly AssetReference VO_Story_Minion_Kilrogg_Male_Orc_Idle_02 = new AssetReference("VO_Story_Minion_Kilrogg_Male_Orc_Idle_02.prefab:1212ca541a9214748b448cb4be7291e3");

	private static readonly AssetReference VO_Story_Minion_Kilrogg_Male_Orc_Idle_03 = new AssetReference("VO_Story_Minion_Kilrogg_Male_Orc_Idle_03.prefab:c07482b561a90ea448871b891737057a");

	private static readonly AssetReference VO_Story_Minion_Kilrogg_Male_Orc_Loss_01 = new AssetReference("VO_Story_Minion_Kilrogg_Male_Orc_Loss_01.prefab:d0e70369cc194aa45a5986f01bc4050a");

	private static readonly AssetReference VO_Story_Minion_Orgrim_Male_Orc_Story_Guldan_Mission4ExchangeC_02 = new AssetReference("VO_Story_Minion_Orgrim_Male_Orc_Story_Guldan_Mission4ExchangeC_02.prefab:a83009a4705e9be43a14ffc749f68a93");

	private static readonly AssetReference VO_Story_Minion_Zuluhed_Male_Orc_Drink_01 = new AssetReference("VO_Story_Minion_Zuluhed_Male_Orc_Drink_01.prefab:0bb65fbe74312e94f8b0dc547bedf89f");

	private static readonly AssetReference VO_Story_Minion_Zuluhed_Male_Orc_HeroPower_01 = new AssetReference("VO_Story_Minion_Zuluhed_Male_Orc_HeroPower_01.prefab:c7943d7b5177b2b4e9943c86844e486e");

	private static readonly AssetReference VO_Story_Minion_Zuluhed_Male_Orc_Idle_01 = new AssetReference("VO_Story_Minion_Zuluhed_Male_Orc_Idle_01.prefab:a829ea6cf67babc4781b83b595cc7eea");

	private static readonly AssetReference VO_Story_Minion_Zuluhed_Male_Orc_Idle_02 = new AssetReference("VO_Story_Minion_Zuluhed_Male_Orc_Idle_02.prefab:f18322e0e4d68574c82f6932cf74aaf3");

	private static readonly AssetReference VO_Story_Minion_Zuluhed_Male_Orc_Idle_03 = new AssetReference("VO_Story_Minion_Zuluhed_Male_Orc_Idle_03.prefab:8e94c624cb91297409d25d3f9e20ab81");

	private static readonly AssetReference VO_Story_Minion_Zuluhed_Male_Orc_Loss_01 = new AssetReference("VO_Story_Minion_Zuluhed_Male_Orc_Loss_01.prefab:9afdeedd658a05e4091a0ffeb72de5d9");

	private static readonly AssetReference VO_Story_Minion_Zuluhed_Male_Orc_Attack_01 = new AssetReference("VO_Story_Minion_Zuluhed_Male_Orc_Attack_01.prefab:6e4942d7f83a25c4cbcb09d3192c1d31");

	private static readonly AssetReference VO_Story_Minion_Zuluhed_Male_Orc_Story_Group_FTH = new AssetReference("VO_Story_Minion_Zuluhed_Male_Orc_Story_Group_FTH.prefab:69149b4d2387bcf459aa910e213c6d63");

	public static readonly AssetReference OrgrimBrassRing = new AssetReference("OrgrimMaghar_BrassRing_Quote.prefab:3904ac9e5ec0e114eaa19f8771f20134");

	private new List<string> m_BossIdleLines = new List<string> { VO_Story_Hero_Grommash_Male_Orc_Story_Guldan_Mission4Idle_01, VO_Story_Hero_Grommash_Male_Orc_Story_Guldan_Mission4Idle_02, VO_Story_Hero_Grommash_Male_Orc_Story_Guldan_Mission4Idle_03 };

	private new List<string> m_BossIdleLinesCopy = new List<string> { VO_Story_Hero_Grommash_Male_Orc_Story_Guldan_Mission4Idle_01, VO_Story_Hero_Grommash_Male_Orc_Story_Guldan_Mission4Idle_02, VO_Story_Hero_Grommash_Male_Orc_Story_Guldan_Mission4Idle_03 };

	private List<string> m_BossBlackhandIdleLines = new List<string> { VO_Story_Hero_Blackhand_Male_Orc_Story_Guldan_Mission4Idle_01, VO_Story_Hero_Blackhand_Male_Orc_Story_Guldan_Mission4Idle_02, VO_Story_Hero_Blackhand_Male_Orc_Story_Guldan_Mission4Idle_03 };

	private List<string> m_BossBlackhandIdleLinesCopy = new List<string> { VO_Story_Hero_Blackhand_Male_Orc_Story_Guldan_Mission4Idle_01, VO_Story_Hero_Blackhand_Male_Orc_Story_Guldan_Mission4Idle_02, VO_Story_Hero_Blackhand_Male_Orc_Story_Guldan_Mission4Idle_03 };

	private List<string> m_BossDurotanIdleLines = new List<string> { VO_Story_Hero_Durotan_Male_Orc_Story_Guldan_Mission4Idle_01, VO_Story_Hero_Durotan_Male_Orc_Story_Guldan_Mission4Idle_02, VO_Story_Hero_Durotan_Male_Orc_Story_Guldan_Mission4Idle_03 };

	private List<string> m_BossDurotanIdleLinesCopy = new List<string> { VO_Story_Hero_Durotan_Male_Orc_Story_Guldan_Mission4Idle_01, VO_Story_Hero_Durotan_Male_Orc_Story_Guldan_Mission4Idle_02, VO_Story_Hero_Durotan_Male_Orc_Story_Guldan_Mission4Idle_03 };

	private List<string> m_BossHurkanIdleLines = new List<string> { VO_Story_Minion_Hurkan_Male_Orc_Idle_01, VO_Story_Minion_Hurkan_Male_Orc_Idle_02, VO_Story_Minion_Hurkan_Male_Orc_HeroPower_01 };

	private List<string> m_BossHurkanIdleLinesCopy = new List<string> { VO_Story_Minion_Hurkan_Male_Orc_Idle_01, VO_Story_Minion_Hurkan_Male_Orc_Idle_02, VO_Story_Minion_Hurkan_Male_Orc_HeroPower_01 };

	private List<string> m_BossFenrisIdleLines = new List<string> { VO_Story_Minion_Fenris_Male_Orc_Story_Guldan_Mission4Idle_01, VO_Story_Minion_Fenris_Male_Orc_Story_Guldan_Mission4Idle_02, VO_Story_Minion_Fenris_Male_Orc_Story_Guldan_Mission4HeroPower_03 };

	private List<string> m_BossFenrisIdleLinesCopy = new List<string> { VO_Story_Minion_Fenris_Male_Orc_Story_Guldan_Mission4Idle_01, VO_Story_Minion_Fenris_Male_Orc_Story_Guldan_Mission4Idle_02, VO_Story_Minion_Fenris_Male_Orc_Story_Guldan_Mission4HeroPower_03 };

	private List<string> m_BossKilroggIdleLines = new List<string> { VO_Story_Minion_Kilrogg_Male_Orc_HeroPower_03, VO_Story_Minion_Kilrogg_Male_Orc_Idle_02, VO_Story_Minion_Kilrogg_Male_Orc_Idle_03 };

	private List<string> m_BossKilroggIdleLinesCopy = new List<string> { VO_Story_Minion_Kilrogg_Male_Orc_HeroPower_03, VO_Story_Minion_Kilrogg_Male_Orc_Idle_02, VO_Story_Minion_Kilrogg_Male_Orc_Idle_03 };

	private List<string> m_BossKargathIdleLines = new List<string> { VO_Story_Minion_Kargath_Male_Orc_Idle_01, VO_Story_Minion_Kargath_Male_Orc_Play_01, VO_Story_Minion_Kargath_Male_Orc_Idle_03 };

	private List<string> m_BossKargathIdleLinesCopy = new List<string> { VO_Story_Minion_Kargath_Male_Orc_Idle_01, VO_Story_Minion_Kargath_Male_Orc_Play_01, VO_Story_Minion_Kargath_Male_Orc_Idle_03 };

	private List<string> m_BossZuluhedIdleLines = new List<string> { VO_Story_Minion_Zuluhed_Male_Orc_Idle_01, VO_Story_Minion_Zuluhed_Male_Orc_Idle_02, VO_Story_Minion_Zuluhed_Male_Orc_Attack_01 };

	private List<string> m_BossZuluhedIdleLinesCopy = new List<string> { VO_Story_Minion_Zuluhed_Male_Orc_Idle_01, VO_Story_Minion_Zuluhed_Male_Orc_Idle_02, VO_Story_Minion_Zuluhed_Male_Orc_Attack_01 };

	private HashSet<string> m_playedLines = new HashSet<string>();

	private static Map<GameEntityOption, bool> InitBooleanOptions()
	{
		return new Map<GameEntityOption, bool> { 
		{
			GameEntityOption.DO_OPENING_TAUNTS,
			false
		} };
	}

	public BoH_Guldan_04()
	{
		m_gameOptions.AddBooleanOptions(s_booleanOptions);
	}

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> m_SoundFiles = new List<string>
		{
			VO_Story_Hero_Blackhand_Male_Orc_Story_Guldan_Mission4EmoteResponse_01, VO_Story_Hero_Blackhand_Male_Orc_Story_Guldan_Mission4ExchangeB_01, VO_Story_Hero_Blackhand_Male_Orc_Story_Guldan_Mission4ExchangeB_02, VO_Story_Hero_Blackhand_Male_Orc_Story_Guldan_Mission4ExchangeC_01, VO_Story_Hero_Blackhand_Male_Orc_Story_Guldan_Mission4Idle_01, VO_Story_Hero_Blackhand_Male_Orc_Story_Guldan_Mission4Idle_02, VO_Story_Hero_Blackhand_Male_Orc_Story_Guldan_Mission4Idle_03, VO_Story_Hero_Blackhand_Male_Orc_Story_Guldan_Mission4Loss_01, VO_Story_Hero_Blackhand_Male_Orc_Story_Guldan_Mission4Victory_02, VO_Story_Hero_Durotan_Male_Orc_Story_Guldan_Mission4EmoteResponse_01,
			VO_Story_Hero_Durotan_Male_Orc_Story_Guldan_Mission4ExchangeD_01, VO_Story_Hero_Durotan_Male_Orc_Story_Guldan_Mission4ExchangeD_03, VO_Story_Hero_Durotan_Male_Orc_Story_Guldan_Mission4ExchangeE_02, VO_Story_Hero_Durotan_Male_Orc_Story_Guldan_Mission4Idle_01, VO_Story_Hero_Durotan_Male_Orc_Story_Guldan_Mission4Idle_02, VO_Story_Hero_Durotan_Male_Orc_Story_Guldan_Mission4Idle_03, VO_Story_Hero_Durotan_Male_Orc_Story_Guldan_Mission4Loss_01, VO_Story_Hero_Grommash_Male_Orc_Story_Guldan_Mission4EmoteResponse_01, VO_Story_Hero_Grommash_Male_Orc_Story_Guldan_Mission4Idle_01, VO_Story_Hero_Grommash_Male_Orc_Story_Guldan_Mission4Idle_02,
			VO_Story_Hero_Grommash_Male_Orc_Story_Guldan_Mission4Idle_03, VO_Story_Hero_Grommash_Male_Orc_Story_Guldan_Mission4Intro_03, VO_Story_Hero_Grommash_Male_Orc_Story_Guldan_Mission4Loss_01, VO_Story_Hero_Grommash_Male_Orc_Story_Guldan_Mission4Enrage, VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission4Entice_01, VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission4ExchangeA_01, VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission4ExchangeD_01, VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission4ExchangeD_03, VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission4ExchangeE_01, VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission4ExchangeE_03,
			VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission4Intro_01, VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission4Intro_02, VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission4Plead_01, VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission4Threaten_01, VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission4Victory_01, VO_HERO_07e_Male_Orc_Attack_01, VO_HERO_07f_Male_Orc_Threaten_01, VO_HERO_07g_Male_Orc_Greetings_01, VO_Story_Minion_Fenris_Male_Orc_Story_Guldan_Mission4Drink_01, VO_Story_Minion_Fenris_Male_Orc_Story_Guldan_Mission4HeroPower_03,
			VO_Story_Minion_Fenris_Male_Orc_Story_Guldan_Mission4Idle_01, VO_Story_Minion_Fenris_Male_Orc_Story_Guldan_Mission4Idle_02, VO_Story_Minion_Fenris_Male_Orc_Story_Guldan_Mission4Idle_03, VO_Story_Minion_Fenris_Male_Orc_Story_Guldan_Mission4Loss_01, VO_Story_Minion_Hurkan_Male_Orc_Drink_01, VO_Story_Minion_Hurkan_Male_Orc_HeroPower_01, VO_Story_Minion_Hurkan_Male_Orc_Idle_01, VO_Story_Minion_Hurkan_Male_Orc_Idle_02, VO_Story_Minion_Hurkan_Male_Orc_Idle_03, VO_Story_Minion_Hurkan_Male_Orc_Loss_01,
			VO_Story_Minion_Kargath_Male_Orc_Attack_01, VO_Story_Minion_Kargath_Male_Orc_EmoteResponse_01, VO_Story_Minion_Kargath_Male_Orc_Idle_01, VO_Story_Minion_Kargath_Male_Orc_Idle_02, VO_Story_Minion_Kargath_Male_Orc_Idle_03, VO_Story_Minion_Kargath_Male_Orc_Loss_01, VO_Story_Minion_Kargath_Male_Orc_Play_01, VO_Story_Minion_Kilrogg_Male_Orc_Drink_01, VO_Story_Minion_Kilrogg_Male_Orc_HeroPower_03, VO_Story_Minion_Kilrogg_Male_Orc_Idle_01,
			VO_Story_Minion_Kilrogg_Male_Orc_Idle_02, VO_Story_Minion_Kilrogg_Male_Orc_Idle_03, VO_Story_Minion_Kilrogg_Male_Orc_Loss_01, VO_Story_Minion_Orgrim_Male_Orc_Story_Guldan_Mission4ExchangeC_02, VO_Story_Minion_Zuluhed_Male_Orc_Drink_01, VO_Story_Minion_Zuluhed_Male_Orc_HeroPower_01, VO_Story_Minion_Zuluhed_Male_Orc_Idle_01, VO_Story_Minion_Zuluhed_Male_Orc_Idle_02, VO_Story_Minion_Zuluhed_Male_Orc_Idle_03, VO_Story_Minion_Zuluhed_Male_Orc_Loss_01,
			VO_Story_Minion_Zuluhed_Male_Orc_Attack_01, VO_Story_Minion_Zuluhed_Male_Orc_Story_Group_FTH
		};
		SetBossVOLines(m_SoundFiles);
		foreach (string soundFile in m_SoundFiles)
		{
			PreloadSound(soundFile);
		}
	}

	protected override bool GetShouldSuppressDeathTextBubble()
	{
		return true;
	}

	public override bool ShouldPlayHeroBlowUpSpells(TAG_PLAYSTATE playState)
	{
		return playState != TAG_PLAYSTATE.WON;
	}

	public override List<string> GetBossIdleLines()
	{
		return m_BossIdleLines;
	}

	public override void OnCreateGame()
	{
		m_OverrideMusicTrack = MusicPlaylistType.InGame_BT;
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
		case 520:
			switch (GameState.Get().GetOpposingSidePlayer().GetHero()
				.GetCardId())
			{
			case "Story_09_Grommash_004hb":
				yield return MissionPlayVO(enemyActor, VO_Story_Hero_Grommash_Male_Orc_Story_Guldan_Mission4Loss_01);
				break;
			case "Story_09_Blackhand_004hb":
				yield return MissionPlayVO(enemyActor, VO_Story_Hero_Blackhand_Male_Orc_Story_Guldan_Mission4Loss_01);
				break;
			case "Story_09_Durotan":
				yield return MissionPlayVO(enemyActor, VO_Story_Hero_Durotan_Male_Orc_Story_Guldan_Mission4Loss_01);
				break;
			case "Story_09_Hurkan":
				yield return MissionPlayVO(enemyActor, VO_Story_Minion_Hurkan_Male_Orc_Loss_01);
				break;
			case "Story_09_Fenris":
				yield return MissionPlayVO(enemyActor, VO_Story_Minion_Fenris_Male_Orc_Story_Guldan_Mission4Loss_01);
				break;
			case "Story_09_Kargath":
				yield return MissionPlayVO(enemyActor, VO_Story_Minion_Kargath_Male_Orc_Loss_01);
				break;
			case "Story_09_Kilrogg":
				yield return MissionPlayVO(enemyActor, VO_Story_Minion_Kilrogg_Male_Orc_Loss_01);
				break;
			case "Story_09_Zuluhed":
				yield return MissionPlayVO(enemyActor, VO_Story_Minion_Zuluhed_Male_Orc_Loss_01);
				break;
			}
			break;
		case 504:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission4Victory_01);
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Blackhand_Male_Orc_Story_Guldan_Mission4Victory_02);
			yield return MissionPlayVO(GetFriendlyActorByCardId("Story_09_ZuluhedMinion2"), VO_Story_Minion_Zuluhed_Male_Orc_Story_Group_FTH);
			GameState.Get().SetBusy(busy: false);
			break;
		case 101:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Grommash_Male_Orc_Story_Guldan_Mission4Intro_03);
			GameState.Get().SetBusy(busy: false);
			break;
		case 102:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission4ExchangeA_01);
			GameState.Get().SetBusy(busy: false);
			break;
		case 103:
			GameState.Get().SetBusy(busy: true);
			yield return PlayLineAlways(enemyActor, VO_Story_Hero_Blackhand_Male_Orc_Story_Guldan_Mission4ExchangeB_02);
			GameState.Get().SetBusy(busy: false);
			break;
		case 104:
			GameState.Get().SetBusy(busy: true);
			yield return PlayLineAlways(enemyActor, VO_Story_Hero_Blackhand_Male_Orc_Story_Guldan_Mission4ExchangeC_01);
			yield return PlayLineAlways(OrgrimBrassRing, VO_Story_Minion_Orgrim_Male_Orc_Story_Guldan_Mission4ExchangeC_02);
			GameState.Get().SetBusy(busy: false);
			break;
		case 105:
			GameState.Get().SetBusy(busy: true);
			yield return PlayLineAlways(enemyActor, VO_Story_Hero_Durotan_Male_Orc_Story_Guldan_Mission4ExchangeD_01);
			yield return PlayLineAlways(friendlyActor, VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission4ExchangeD_03);
			yield return PlayLineAlways(enemyActor, VO_Story_Hero_Durotan_Male_Orc_Story_Guldan_Mission4ExchangeD_03);
			GameState.Get().SetBusy(busy: false);
			break;
		case 106:
			GameState.Get().SetBusy(busy: true);
			yield return PlayLineAlways(friendlyActor, VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission4ExchangeE_01);
			yield return PlayLineAlways(enemyActor, VO_Story_Hero_Durotan_Male_Orc_Story_Guldan_Mission4ExchangeE_02);
			yield return PlayLineAlways(friendlyActor, VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission4ExchangeE_03);
			GameState.Get().SetBusy(busy: false);
			break;
		case 120:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(enemyActor, VO_Story_Minion_Hurkan_Male_Orc_Drink_01);
			GameState.Get().SetBusy(busy: false);
			break;
		case 121:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(enemyActor, VO_Story_Minion_Fenris_Male_Orc_Story_Guldan_Mission4Drink_01);
			GameState.Get().SetBusy(busy: false);
			break;
		case 122:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(enemyActor, VO_Story_Minion_Kargath_Male_Orc_Attack_01);
			GameState.Get().SetBusy(busy: false);
			break;
		case 123:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(enemyActor, VO_Story_Minion_Kilrogg_Male_Orc_Drink_01);
			GameState.Get().SetBusy(busy: false);
			break;
		case 124:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(enemyActor, VO_Story_Minion_Zuluhed_Male_Orc_Idle_03);
			yield return MissionPlayVO(enemyActor, VO_Story_Minion_Zuluhed_Male_Orc_Drink_01);
			GameState.Get().SetBusy(busy: false);
			break;
		case 125:
			GameState.Get().SetBusy(busy: true);
			yield return PlayLineAlways(friendlyActor, VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission4ExchangeA_01);
			GameState.Get().SetBusy(busy: false);
			break;
		case 126:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Blackhand_Male_Orc_Story_Guldan_Mission4ExchangeB_01);
			GameState.Get().SetBusy(busy: false);
			break;
		case 127:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission4Intro_01);
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission4Intro_02);
			GameState.Get().SetBusy(busy: false);
			break;
		case 128:
			GameState.Get().SetBusy(busy: true);
			yield return PlayLineAlways(friendlyActor, VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission4ExchangeD_01);
			GameState.Get().SetBusy(busy: false);
			break;
		case 129:
			yield return PlayLineAlways(enemyActor, VO_Story_Minion_Hurkan_Male_Orc_Idle_03);
			break;
		case 130:
			yield return PlayLineAlways(enemyActor, VO_Story_Minion_Fenris_Male_Orc_Story_Guldan_Mission4Idle_03);
			break;
		case 131:
			yield return PlayLineAlways(enemyActor, VO_Story_Minion_Kilrogg_Male_Orc_Idle_01);
			yield return PlayLineAlways(friendlyActor, VO_HERO_07e_Male_Orc_Attack_01);
			break;
		case 132:
			yield return PlayLineAlways(enemyActor, VO_Story_Minion_Zuluhed_Male_Orc_HeroPower_01);
			yield return PlayLineAlways(friendlyActor, VO_HERO_07f_Male_Orc_Threaten_01);
			break;
		case 133:
			yield return PlayLineAlways(enemyActor, VO_Story_Minion_Kargath_Male_Orc_Idle_02);
			yield return PlayLineAlways(friendlyActor, VO_HERO_07g_Male_Orc_Greetings_01);
			break;
		case 134:
			yield return PlayLineAlways(enemyActor, VO_Story_Hero_Grommash_Male_Orc_Story_Guldan_Mission4Enrage);
			break;
		case 515:
			switch (GameState.Get().GetOpposingSidePlayer().GetHero()
				.GetCardId())
			{
			case "Story_09_Grommash_004hb":
				yield return MissionPlayVO(enemyActor, VO_Story_Hero_Grommash_Male_Orc_Story_Guldan_Mission4EmoteResponse_01);
				break;
			case "Story_09_Blackhand_004hb":
				yield return MissionPlayVO(enemyActor, VO_Story_Hero_Blackhand_Male_Orc_Story_Guldan_Mission4EmoteResponse_01);
				break;
			case "Story_09_Durotan":
				yield return MissionPlayVO(enemyActor, VO_Story_Hero_Durotan_Male_Orc_Story_Guldan_Mission4EmoteResponse_01);
				break;
			case "Story_09_Hurkan":
				yield return MissionPlayVO(enemyActor, VO_Story_Minion_Hurkan_Male_Orc_HeroPower_01);
				break;
			case "Story_09_Fenris":
				yield return MissionPlayVO(enemyActor, VO_Story_Minion_Fenris_Male_Orc_Story_Guldan_Mission4HeroPower_03);
				break;
			case "Story_09_Kargath":
				yield return MissionPlayVO(enemyActor, VO_Story_Minion_Kargath_Male_Orc_EmoteResponse_01);
				break;
			case "Story_09_Kilrogg":
				yield return MissionPlayVO(enemyActor, VO_Story_Minion_Kilrogg_Male_Orc_HeroPower_03);
				break;
			case "Story_09_Zuluhed":
				yield return MissionPlayVO(enemyActor, VO_Story_Minion_Zuluhed_Male_Orc_HeroPower_01);
				break;
			}
			break;
		default:
			yield return base.HandleMissionEventWithTiming(missionEvent);
			break;
		}
	}

	public override IEnumerator OnPlayThinkEmoteWithTiming()
	{
		if (m_enemySpeaking)
		{
			yield break;
		}
		Player currentPlayer = GameState.Get().GetCurrentPlayer();
		if (!currentPlayer.IsFriendlySide() || currentPlayer.GetHeroCard().HasActiveEmoteSound())
		{
			yield break;
		}
		string opposingHeroCard = GameState.Get().GetOpposingSidePlayer().GetHero()
			.GetCardId();
		float thinkEmoteBossIdleChancePercentage = GetThinkEmoteBossIdleChancePercentage();
		float randomThink = Random.Range(0f, 1f);
		if (thinkEmoteBossIdleChancePercentage > randomThink || (!m_Mission_FriendlyPlayIdleLines && m_Mission_EnemyPlayIdleLines))
		{
			Actor enemyActor = GameState.Get().GetOpposingSidePlayer().GetHeroCard()
				.GetActor();
			switch (opposingHeroCard)
			{
			case "Story_09_Grommash_004hb":
			{
				string voLine5 = PopRandomLine(m_BossIdleLinesCopy);
				if (m_BossIdleLinesCopy.Count == 0)
				{
					m_BossIdleLinesCopy = new List<string>(m_BossIdleLines);
				}
				yield return MissionPlayVO(enemyActor, voLine5);
				break;
			}
			case "Story_09_Blackhand_004hb":
			{
				string voLine8 = PopRandomLine(m_BossBlackhandIdleLinesCopy);
				if (m_BossBlackhandIdleLinesCopy.Count == 0)
				{
					m_BossBlackhandIdleLinesCopy = new List<string>(m_BossBlackhandIdleLines);
				}
				yield return MissionPlayVO(enemyActor, voLine8);
				break;
			}
			case "Story_09_Durotan":
			{
				string voLine6 = PopRandomLine(m_BossDurotanIdleLinesCopy);
				if (m_BossDurotanIdleLinesCopy.Count == 0)
				{
					m_BossDurotanIdleLinesCopy = new List<string>(m_BossDurotanIdleLines);
				}
				yield return MissionPlayVO(enemyActor, voLine6);
				break;
			}
			case "Story_09_Hurkan":
			{
				string voLine3 = PopRandomLine(m_BossHurkanIdleLinesCopy);
				if (m_BossHurkanIdleLinesCopy.Count == 0)
				{
					m_BossHurkanIdleLinesCopy = new List<string>(m_BossHurkanIdleLines);
				}
				yield return MissionPlayVO(enemyActor, voLine3);
				break;
			}
			case "Story_09_Fenris":
			{
				string voLine2 = PopRandomLine(m_BossFenrisIdleLinesCopy);
				if (m_BossFenrisIdleLinesCopy.Count == 0)
				{
					m_BossFenrisIdleLinesCopy = new List<string>(m_BossFenrisIdleLines);
				}
				yield return MissionPlayVO(enemyActor, voLine2);
				break;
			}
			case "Story_09_Kilrogg":
			{
				string voLine7 = PopRandomLine(m_BossKilroggIdleLinesCopy);
				if (m_BossKilroggIdleLinesCopy.Count == 0)
				{
					m_BossKilroggIdleLinesCopy = new List<string>(m_BossKilroggIdleLines);
				}
				yield return MissionPlayVO(enemyActor, voLine7);
				break;
			}
			case "Story_09_Kargath":
			{
				string voLine4 = PopRandomLine(m_BossKargathIdleLinesCopy);
				if (m_BossKargathIdleLinesCopy.Count == 0)
				{
					m_BossKargathIdleLinesCopy = new List<string>(m_BossKargathIdleLines);
				}
				yield return MissionPlayVO(enemyActor, voLine4);
				break;
			}
			case "Story_09_Zuluhed":
			{
				string voLine = PopRandomLine(m_BossZuluhedIdleLinesCopy);
				if (m_BossZuluhedIdleLinesCopy.Count == 0)
				{
					m_BossZuluhedIdleLinesCopy = new List<string>(m_BossZuluhedIdleLines);
				}
				yield return MissionPlayVO(enemyActor, voLine);
				break;
			}
			}
		}
		else if (m_Mission_FriendlyPlayIdleLines)
		{
			EmoteType thinkEmote = EmoteType.THINK1;
			switch (Random.Range(1, 4))
			{
			case 1:
				thinkEmote = EmoteType.THINK1;
				break;
			case 2:
				thinkEmote = EmoteType.THINK2;
				break;
			case 3:
				thinkEmote = EmoteType.THINK3;
				break;
			}
			GameState.Get().GetCurrentPlayer().GetHeroCard()
				.PlayEmote(thinkEmote)
				.GetActiveAudioSource();
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
		GameState.Get().GetFriendlySidePlayer().GetHero()
			.GetCard()
			.GetActor();
	}
}
