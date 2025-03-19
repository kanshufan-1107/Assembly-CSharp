using System.Collections;
using System.Collections.Generic;
using Blizzard.T5.Core;
using UnityEngine;

public class BoH_Faelin_16 : BoH_Faelin_Dungeon
{
	private static Map<GameEntityOption, bool> s_booleanOptions = InitBooleanOptions();

	private static readonly AssetReference VO_Story_11_Azshara_016hb_Female_Naga_Story_Faelin_Mission16EmoteResponse_01 = new AssetReference("VO_Story_11_Azshara_016hb_Female_Naga_Story_Faelin_Mission16EmoteResponse_01.prefab:630bd76ff24f0ca429f713160ad78953");

	private static readonly AssetReference VO_Story_11_Azshara_016hb_Female_Naga_Story_Faelin_Mission16ExchangeA_01 = new AssetReference("VO_Story_11_Azshara_016hb_Female_Naga_Story_Faelin_Mission16ExchangeA_01.prefab:5aa27205fb6242f45bd86fa3388071b3");

	private static readonly AssetReference VO_Story_11_Azshara_016hb_Female_Naga_Story_Faelin_Mission16ExchangeB_01 = new AssetReference("VO_Story_11_Azshara_016hb_Female_Naga_Story_Faelin_Mission16ExchangeB_01.prefab:b2bfe8472c09946449b8bfbc200ae3f3");

	private static readonly AssetReference VO_Story_11_Azshara_016hb_Female_Naga_Story_Faelin_Mission16ExchangeC_01 = new AssetReference("VO_Story_11_Azshara_016hb_Female_Naga_Story_Faelin_Mission16ExchangeC_01.prefab:001b1c8cc2761b746a8b72041fbdcf5e");

	private static readonly AssetReference VO_Story_11_Azshara_016hb_Female_Naga_Story_Faelin_Mission16ExchangeD_01 = new AssetReference("VO_Story_11_Azshara_016hb_Female_Naga_Story_Faelin_Mission16ExchangeD_01.prefab:bf54c884e4c51d446b1d546cd1278e2c");

	private static readonly AssetReference VO_Story_11_Azshara_016hb_Female_Naga_Story_Faelin_Mission16ExchangeE_01 = new AssetReference("VO_Story_11_Azshara_016hb_Female_Naga_Story_Faelin_Mission16ExchangeE_01.prefab:fdb997951bf398946a84bc70e07e63a5");

	private static readonly AssetReference VO_Story_11_Azshara_016hb_Female_Naga_Story_Faelin_Mission16ExchangeF_01 = new AssetReference("VO_Story_11_Azshara_016hb_Female_Naga_Story_Faelin_Mission16ExchangeF_01.prefab:3d9fc74fd30d0b14db8eef41ec593851");

	private static readonly AssetReference VO_Story_11_Azshara_016hb_Female_Naga_Story_Faelin_Mission16ExchangeG_01 = new AssetReference("VO_Story_11_Azshara_016hb_Female_Naga_Story_Faelin_Mission16ExchangeG_01.prefab:1c0f58333df065347a9f68012c83bf8d");

	private static readonly AssetReference VO_Story_11_Azshara_016hb_Female_Naga_Story_Faelin_Mission16ExchangeH_01 = new AssetReference("VO_Story_11_Azshara_016hb_Female_Naga_Story_Faelin_Mission16ExchangeH_01.prefab:e69d073ab4e864d43917680ee30cef27");

	private static readonly AssetReference VO_Story_11_Azshara_016hb_Female_Naga_Story_Faelin_Mission16ExchangeI_01 = new AssetReference("VO_Story_11_Azshara_016hb_Female_Naga_Story_Faelin_Mission16ExchangeI_01.prefab:bd74cd24bef382c43852a16767cd85e5");

	private static readonly AssetReference VO_Story_11_Azshara_016hb_Female_Naga_Story_Faelin_Mission16HeroPower_01 = new AssetReference("VO_Story_11_Azshara_016hb_Female_Naga_Story_Faelin_Mission16HeroPower_01.prefab:c78dad1e96f479f4f8babdde15d45341");

	private static readonly AssetReference VO_Story_11_Azshara_016hb_Female_Naga_Story_Faelin_Mission16HeroPower_02 = new AssetReference("VO_Story_11_Azshara_016hb_Female_Naga_Story_Faelin_Mission16HeroPower_02.prefab:65b566932bebaad46addf2b7a08362c1");

	private static readonly AssetReference VO_Story_11_Azshara_016hb_Female_Naga_Story_Faelin_Mission16HeroPower_03 = new AssetReference("VO_Story_11_Azshara_016hb_Female_Naga_Story_Faelin_Mission16HeroPower_03.prefab:ffdfdf28f98262f488ae00967b2b883b");

	private static readonly AssetReference VO_Story_11_Azshara_016hb_Female_Naga_Story_Faelin_Mission16Idle_01 = new AssetReference("VO_Story_11_Azshara_016hb_Female_Naga_Story_Faelin_Mission16Idle_01.prefab:f29df5501ba39644ea8df9e38b6f79d1");

	private static readonly AssetReference VO_Story_11_Azshara_016hb_Female_Naga_Story_Faelin_Mission16Idle_02 = new AssetReference("VO_Story_11_Azshara_016hb_Female_Naga_Story_Faelin_Mission16Idle_02.prefab:0eeeef8525c22864ead2b070c8c6e893");

	private static readonly AssetReference VO_Story_11_Azshara_016hb_Female_Naga_Story_Faelin_Mission16Idle_03 = new AssetReference("VO_Story_11_Azshara_016hb_Female_Naga_Story_Faelin_Mission16Idle_03.prefab:1ff2c1ada2aef6044bd84d294438511b");

	private static readonly AssetReference VO_Story_11_Azshara_016hb_Female_Naga_Story_Faelin_Mission16Loss_01 = new AssetReference("VO_Story_11_Azshara_016hb_Female_Naga_Story_Faelin_Mission16Loss_01.prefab:00de73090267ea745afaa101d1d1db75");

	private static readonly AssetReference VO_Story_11_Azshara_016hb_Female_Naga_Story_Faelin_Mission16Start_01 = new AssetReference("VO_Story_11_Azshara_016hb_Female_Naga_Story_Faelin_Mission16Start_01.prefab:64f56da1e856fb14d9585c8c9ada3826");

	private static readonly AssetReference VO_Story_11_Azshara_016hb_Female_Naga_Story_Faelin_Mission16Start_02 = new AssetReference("VO_Story_11_Azshara_016hb_Female_Naga_Story_Faelin_Mission16Start_02.prefab:398873bc9e8e19a47a6bb4f4a6b6f071");

	private static readonly AssetReference VO_Story_11_Azshara_016hb_Female_Naga_Story_Faelin_Mission16Victory_01 = new AssetReference("VO_Story_11_Azshara_016hb_Female_Naga_Story_Faelin_Mission16Victory_01.prefab:8e147dc6bc1ec6b4b8bfb098ae3cd4dc");

	private static readonly AssetReference VO_Story_11_Caye_012hp_Female_Nightborne_Story_Faelin_Mission16ExchangeA_01 = new AssetReference("VO_Story_11_Caye_012hp_Female_Nightborne_Story_Faelin_Mission16ExchangeA_01.prefab:023134b221305e74484d985e4b01b423");

	private static readonly AssetReference VO_Story_11_Caye_012hp_Female_Nightborne_Story_Faelin_Mission16ExchangeJ_01 = new AssetReference("VO_Story_11_Caye_012hp_Female_Nightborne_Story_Faelin_Mission16ExchangeJ_01.prefab:adf8e3ec09487654192c8bd95ce353a4");

	private static readonly AssetReference VO_Story_11_Caye_012hp_Female_Nightborne_Story_Faelin_Mission16Victory_01 = new AssetReference("VO_Story_11_Caye_012hp_Female_Nightborne_Story_Faelin_Mission16Victory_01.prefab:ddf46a83dda6bdb4eb11496fa3dea08f");

	private static readonly AssetReference VO_Story_11_Caye_012hp_Female_Nightborne_Story_Faelin_Mission16Victory_02 = new AssetReference("VO_Story_11_Caye_012hp_Female_Nightborne_Story_Faelin_Mission16Victory_02.prefab:124cad6621fc2dc4c9c81ebc214f2c86");

	private static readonly AssetReference VO_Story_11_Caye_012hp_Female_Nightborne_Story_Faelin_PostMission16_01 = new AssetReference("VO_Story_11_Caye_012hp_Female_Nightborne_Story_Faelin_PostMission16_01.prefab:ef283a79eedcf7a498c91fb299dc990a");

	private static readonly AssetReference VO_Story_11_Caye_012hp_Female_Nightborne_Story_Faelin_PostMission16_02 = new AssetReference("VO_Story_11_Caye_012hp_Female_Nightborne_Story_Faelin_PostMission16_02.prefab:57471ecdbcc231c4fa9c4c552d3a17ec");

	private static readonly AssetReference VO_Story_11_Caye_012hp_Female_Nightborne_Story_Faelin_PreMission16_01 = new AssetReference("VO_Story_11_Caye_012hp_Female_Nightborne_Story_Faelin_PreMission16_01.prefab:ba64f62cad82e334ca46a698edd040b1");

	private static readonly AssetReference VO_Story_11_Dathril_012hb_Male_Naga_Story_Faelin_Mission16_Lorebook_1_01 = new AssetReference("VO_Story_11_Dathril_012hb_Male_Naga_Story_Faelin_Mission16_Lorebook_1_01.prefab:0f73fbb7865bfaa4a9930174deecb805");

	private static readonly AssetReference VO_Story_11_Dathril_012hb_Male_Naga_Story_Faelin_Mission16ExchangeI_01 = new AssetReference("VO_Story_11_Dathril_012hb_Male_Naga_Story_Faelin_Mission16ExchangeI_01.prefab:d999d1df11e55ab489eabd7066b777df");

	private static readonly AssetReference VO_Story_11_Dathril_012hb_Male_Naga_Story_Faelin_Mission16ExchangeJ_01 = new AssetReference("VO_Story_11_Dathril_012hb_Male_Naga_Story_Faelin_Mission16ExchangeJ_01.prefab:cad342bb11297be4fb72b28a55a3613f");

	private static readonly AssetReference VO_Story_11_Dathril_012hb_Male_Naga_Story_Faelin_Mission16ExchangeJ_02 = new AssetReference("VO_Story_11_Dathril_012hb_Male_Naga_Story_Faelin_Mission16ExchangeJ_02.prefab:5dae5fdc29daa4e41915fbdf9ac5357c");

	private static readonly AssetReference VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission16ExchangeA_01 = new AssetReference("VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission16ExchangeA_01.prefab:96ad398e1dc3a6f4e8db3bcba1bf4047");

	private static readonly AssetReference VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission16ExchangeE_01 = new AssetReference("VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission16ExchangeE_01.prefab:e77bd32235329d742aaa882312ff3aeb");

	private static readonly AssetReference VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission16ExchangeH_01 = new AssetReference("VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission16ExchangeH_01.prefab:f896e8b9d1f5d0a49ba304360e834a7d");

	private static readonly AssetReference VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission16ExchangeH_02 = new AssetReference("VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission16ExchangeH_02.prefab:99f520072e3f8f947bc5d62f5e9ffb94");

	private static readonly AssetReference VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission16ExchangeJ_01 = new AssetReference("VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission16ExchangeJ_01.prefab:2d0f375503c80424abb8883fc86a9619");

	private static readonly AssetReference VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission16Victory_01 = new AssetReference("VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission16Victory_01.prefab:0ae1df0bf33ecee4a9f330ef1eabdba5");

	private static readonly AssetReference VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission16Victory_02 = new AssetReference("VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission16Victory_02.prefab:61d14d2085b538e4c98220fb1a938d82");

	private static readonly AssetReference VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission16Victory_03 = new AssetReference("VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission16Victory_03.prefab:ffa91b70719e3ce49a5b298a97a361b7");

	private static readonly AssetReference VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission16Victory_04 = new AssetReference("VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission16Victory_04.prefab:1a52e0a77d88b804b9ee94ce1eab43d1");

	private static readonly AssetReference VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_PostMission16_01 = new AssetReference("VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_PostMission16_01.prefab:57ec72257ae8f8446995033eb838c96a");

	private static readonly AssetReference VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_PostMission16_02 = new AssetReference("VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_PostMission16_02.prefab:b4319f2eb9f044d42bd13adb84d3fedd");

	private static readonly AssetReference VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_PreMission16_01 = new AssetReference("VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_PreMission16_01.prefab:22c4ceb78d587674abcfff35b78954b8");

	private static readonly AssetReference VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_PreMission16_02 = new AssetReference("VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_PreMission16_02.prefab:3ba2c403538da1b44a0ccd950d4d8471");

	private static readonly AssetReference VO_Story_11_Finley_009hp_Male_Murloc_Story_Faelin_Mission16ExchangeA_01 = new AssetReference("VO_Story_11_Finley_009hp_Male_Murloc_Story_Faelin_Mission16ExchangeA_01.prefab:946e2986d42c0dc4cb0d0d0e2de8c9ae");

	private static readonly AssetReference VO_Story_11_Finley_009hp_Male_Murloc_Story_Faelin_Mission16ExchangeA_02 = new AssetReference("VO_Story_11_Finley_009hp_Male_Murloc_Story_Faelin_Mission16ExchangeA_02.prefab:9b37de51c34aa554ebda4a820b4dc88d");

	private static readonly AssetReference VO_Story_11_Finley_009hp_Male_Murloc_Story_Faelin_Mission16ExchangeB_01 = new AssetReference("VO_Story_11_Finley_009hp_Male_Murloc_Story_Faelin_Mission16ExchangeB_01.prefab:f199c720c0baa744e9a60effdd6c8eba");

	private static readonly AssetReference VO_Story_11_Finley_009hp_Male_Murloc_Story_Faelin_Mission16ExchangeB_02 = new AssetReference("VO_Story_11_Finley_009hp_Male_Murloc_Story_Faelin_Mission16ExchangeB_02.prefab:9d2ea34f21f0b664b857d190feb16937");

	private static readonly AssetReference VO_Story_11_Finley_009hp_Male_Murloc_Story_Faelin_Mission16Victory_01 = new AssetReference("VO_Story_11_Finley_009hp_Male_Murloc_Story_Faelin_Mission16Victory_01.prefab:dc270dc6167d4fd43afdd137f90c8ac3");

	private static readonly AssetReference VO_Story_11_Finley_009hp_Male_Murloc_Story_Faelin_Mission16Victory_02 = new AssetReference("VO_Story_11_Finley_009hp_Male_Murloc_Story_Faelin_Mission16Victory_02.prefab:df60074f1cde782418da40b571bb7740");

	private static readonly AssetReference VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission16ExchangeA_01 = new AssetReference("VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission16ExchangeA_01.prefab:32fb6a89261324c4094039e3c57c113d");

	private static readonly AssetReference VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission16ExchangeB_01 = new AssetReference("VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission16ExchangeB_01.prefab:25865fff5723c124aa25e2e741ad9ac9");

	private static readonly AssetReference VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission16Victory_01 = new AssetReference("VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission16Victory_01.prefab:c33834e6c3d79664fa4417e1a81aae87");

	private static readonly AssetReference VO_Story_11_Halus_010hp_Male_BloodElf_Story_Faelin_Mission16ExchangeA_01 = new AssetReference("VO_Story_11_Halus_010hp_Male_BloodElf_Story_Faelin_Mission16ExchangeA_01.prefab:de2a9895e635c1b49afc40991a618121");

	private static readonly AssetReference VO_Story_11_Halus_010hp_Male_BloodElf_Story_Faelin_Mission16ExchangeB_01 = new AssetReference("VO_Story_11_Halus_010hp_Male_BloodElf_Story_Faelin_Mission16ExchangeB_01.prefab:2042e4a11642be9449014275379bda88");

	private static readonly AssetReference VO_Story_11_Halus_010hp_Male_BloodElf_Story_Faelin_Mission16ExchangeI_01 = new AssetReference("VO_Story_11_Halus_010hp_Male_BloodElf_Story_Faelin_Mission16ExchangeI_01.prefab:976f363960062d740b5b878f752c5845");

	private static readonly AssetReference VO_Story_11_Halus_010hp_Male_BloodElf_Story_Faelin_Mission16Victory_04 = new AssetReference("VO_Story_11_Halus_010hp_Male_BloodElf_Story_Faelin_Mission16Victory_04.prefab:e3e6e26849d15ca41a5b6f7ac8bf0765");

	private static readonly AssetReference VO_Story_11_Handmaiden_015hb_Female_Naga_Story_Faelin_Mission16Start_01 = new AssetReference("VO_Story_11_Handmaiden_015hb_Female_Naga_Story_Faelin_Mission16Start_01.prefab:f373bddb46c13f541a9a64973077d292");

	private static readonly AssetReference VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission16Victory_01 = new AssetReference("VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission16Victory_01.prefab:b8e33e71ee9715b4a8dd526da24c9e1f");

	private static readonly AssetReference VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_PostMission16_01 = new AssetReference("VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_PostMission16_01.prefab:bc3d30a9bc921c848b88f8424e6a582f");

	private static readonly AssetReference VO_Story_11_Azshara_016hb_Female_Naga_Mission16_Lorebook_2_01 = new AssetReference("VO_Story_11_Azshara_016hb_Female_Naga_Mission16_Lorebook_2_01.prefab:d6d36a6e2ed684249b90177e7d64b5a1");

	private Notification m_popup;

	private Dictionary<int, string[]> m_popUpInfo = new Dictionary<int, string[]>
	{
		{
			229,
			new string[1] { "BOH_FAELIN_MISSION16_LOREBOOK_01" }
		},
		{
			228,
			new string[1] { "BOH_FAELIN_MISSION16_LOREBOOK_02" }
		}
	};

	private float popUpScale = 1.65f;

	private Vector3 popUpPos;

	public static readonly AssetReference IniBrassRing = new AssetReference("HS25-189_H_Ini_BrassRing_Quote.prefab:9f20435bdae99d24bae357f002abcf27");

	public static readonly AssetReference CayeBrassRing = new AssetReference("LC23-001_H_Caye_BrassRing_Quote.prefab:2364e8ce64f9c404fb569a86b17f3d01");

	public static readonly AssetReference BookBrassRing = new AssetReference("Wisdomball_Pop-up_BrassRing_Quote.prefab:896ee20514caff74db639aa7055838f6");

	public static readonly AssetReference FinleyBrassRing = new AssetReference("HS25-190_M_Finley_BrassRing_Quote.prefab:7123c129afe769e4aa9fb262a8f7c919");

	public static readonly AssetReference HalusBrassRing = new AssetReference("LC23-003_H_Halus_BrassRing_Quote.prefab:36e0aabb3ed4ce84599d9dd4e9ebdcf5");

	public static readonly AssetReference GraceBrassRing = new AssetReference("LC23-002_H_Grace_BrassRing_Quote.prefab:1f849ba2e303ff3449d34b96f960c96a");

	public static readonly AssetReference FaelinBrassRing = new AssetReference("SKN23-002_H_FAELIN_BrassRing_Quote.prefab:9984152d900140547aa7d721f3e49428");

	private new List<string> m_BossIdleLines = new List<string> { VO_Story_11_Azshara_016hb_Female_Naga_Story_Faelin_Mission16Idle_01, VO_Story_11_Azshara_016hb_Female_Naga_Story_Faelin_Mission16Idle_02, VO_Story_11_Azshara_016hb_Female_Naga_Story_Faelin_Mission16Idle_03 };

	private List<string> m_BossUsesHeroPowerLines = new List<string> { VO_Story_11_Azshara_016hb_Female_Naga_Story_Faelin_Mission16HeroPower_01, VO_Story_11_Azshara_016hb_Female_Naga_Story_Faelin_Mission16HeroPower_02, VO_Story_11_Azshara_016hb_Female_Naga_Story_Faelin_Mission16HeroPower_03 };

	private HashSet<string> m_playedLines = new HashSet<string>();

	private static Map<GameEntityOption, bool> InitBooleanOptions()
	{
		return new Map<GameEntityOption, bool> { 
		{
			GameEntityOption.DO_OPENING_TAUNTS,
			false
		} };
	}

	public BoH_Faelin_16()
	{
		m_gameOptions.AddBooleanOptions(s_booleanOptions);
	}

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> m_SoundFiles = new List<string>
		{
			VO_Story_11_Azshara_016hb_Female_Naga_Story_Faelin_Mission16Start_01, VO_Story_11_Handmaiden_015hb_Female_Naga_Story_Faelin_Mission16Start_01, VO_Story_11_Azshara_016hb_Female_Naga_Story_Faelin_Mission16Start_02, VO_Story_11_Azshara_016hb_Female_Naga_Story_Faelin_Mission16ExchangeA_01, VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission16ExchangeA_01, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission16ExchangeA_01, VO_Story_11_Finley_009hp_Male_Murloc_Story_Faelin_Mission16ExchangeA_01, VO_Story_11_Caye_012hp_Female_Nightborne_Story_Faelin_Mission16ExchangeA_01, VO_Story_11_Halus_010hp_Male_BloodElf_Story_Faelin_Mission16ExchangeA_01, VO_Story_11_Azshara_016hb_Female_Naga_Story_Faelin_Mission16ExchangeB_01,
			VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission16ExchangeB_01, VO_Story_11_Halus_010hp_Male_BloodElf_Story_Faelin_Mission16ExchangeB_01, VO_Story_11_Azshara_016hb_Female_Naga_Story_Faelin_Mission16ExchangeC_01, VO_Story_11_Finley_009hp_Male_Murloc_Story_Faelin_Mission16ExchangeB_01, VO_Story_11_Azshara_016hb_Female_Naga_Story_Faelin_Mission16ExchangeD_01, VO_Story_11_Azshara_016hb_Female_Naga_Story_Faelin_Mission16ExchangeE_01, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission16ExchangeE_01, VO_Story_11_Azshara_016hb_Female_Naga_Story_Faelin_Mission16ExchangeF_01, VO_Story_11_Azshara_016hb_Female_Naga_Story_Faelin_Mission16ExchangeG_01, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission16ExchangeH_01,
			VO_Story_11_Azshara_016hb_Female_Naga_Story_Faelin_Mission16ExchangeH_01, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission16ExchangeH_02, VO_Story_11_Azshara_016hb_Female_Naga_Story_Faelin_Mission16ExchangeI_01, VO_Story_11_Dathril_012hb_Male_Naga_Story_Faelin_Mission16ExchangeI_01, VO_Story_11_Halus_010hp_Male_BloodElf_Story_Faelin_Mission16ExchangeI_01, VO_Story_11_Dathril_012hb_Male_Naga_Story_Faelin_Mission16ExchangeJ_01, VO_Story_11_Dathril_012hb_Male_Naga_Story_Faelin_Mission16ExchangeJ_02, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission16ExchangeJ_01, VO_Story_11_Caye_012hp_Female_Nightborne_Story_Faelin_Mission16ExchangeJ_01, VO_Story_11_Azshara_016hb_Female_Naga_Story_Faelin_Mission16Victory_01,
			VO_Story_11_Caye_012hp_Female_Nightborne_Story_Faelin_Mission16Victory_01, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission16Victory_01, VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission16Victory_01, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission16Victory_02, VO_Story_11_Finley_009hp_Male_Murloc_Story_Faelin_Mission16Victory_01, VO_Story_11_Caye_012hp_Female_Nightborne_Story_Faelin_Mission16Victory_02, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission16Victory_03, VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission16Victory_01, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission16Victory_04, VO_Story_11_Halus_010hp_Male_BloodElf_Story_Faelin_Mission16Victory_04,
			VO_Story_11_Azshara_016hb_Female_Naga_Story_Faelin_Mission16EmoteResponse_01, VO_Story_11_Azshara_016hb_Female_Naga_Story_Faelin_Mission16HeroPower_01, VO_Story_11_Azshara_016hb_Female_Naga_Story_Faelin_Mission16HeroPower_02, VO_Story_11_Azshara_016hb_Female_Naga_Story_Faelin_Mission16HeroPower_03, VO_Story_11_Azshara_016hb_Female_Naga_Story_Faelin_Mission16Loss_01, VO_Story_11_Azshara_016hb_Female_Naga_Story_Faelin_Mission16Idle_01, VO_Story_11_Azshara_016hb_Female_Naga_Story_Faelin_Mission16Idle_02, VO_Story_11_Azshara_016hb_Female_Naga_Story_Faelin_Mission16Idle_03, VO_Story_11_Azshara_016hb_Female_Naga_Mission16_Lorebook_2_01, VO_Story_11_Dathril_012hb_Male_Naga_Story_Faelin_Mission16_Lorebook_1_01
		};
		SetBossVOLines(m_SoundFiles);
		foreach (string soundFile in m_SoundFiles)
		{
			PreloadSound(soundFile);
		}
	}

	public override bool ShouldPlayHeroBlowUpSpells(TAG_PLAYSTATE playState)
	{
		return playState != TAG_PLAYSTATE.WON;
	}

	public override List<string> GetBossIdleLines()
	{
		return m_BossIdleLines;
	}

	public override List<string> GetBossHeroPowerRandomLines()
	{
		return m_BossUsesHeroPowerLines;
	}

	public override void OnCreateGame()
	{
		base.OnCreateGame();
		m_OverrideMusicTrack = MusicPlaylistType.InGame_TSC_Boss;
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
		popUpPos = new Vector3(0f, 0f, -40f);
		switch (missionEvent)
		{
		case 515:
			yield return MissionPlayVO(enemyActor, VO_Story_11_Azshara_016hb_Female_Naga_Story_Faelin_Mission16EmoteResponse_01);
			break;
		case 514:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(enemyActor, VO_Story_11_Azshara_016hb_Female_Naga_Story_Faelin_Mission16Start_01);
			yield return MissionPlayVO(GetEnemyActorByCardId("Story_11_Zainra2"), VO_Story_11_Handmaiden_015hb_Female_Naga_Story_Faelin_Mission16Start_01);
			GameState.Get().SetBusy(busy: false);
			break;
		case 507:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(enemyActor, VO_Story_11_Azshara_016hb_Female_Naga_Story_Faelin_Mission16Loss_01);
			GameState.Get().SetBusy(busy: false);
			break;
		case 504:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(friendlyActor, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission16Victory_01);
			yield return MissionPlayVO(IniBrassRing, VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission16Victory_01);
			yield return MissionPlayVO(friendlyActor, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission16Victory_04);
			yield return MissionPlayVO(HalusBrassRing, VO_Story_11_Halus_010hp_Male_BloodElf_Story_Faelin_Mission16Victory_04);
			GameState.Get().SetBusy(busy: false);
			break;
		case 101:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(CayeBrassRing, VO_Story_11_Caye_012hp_Female_Nightborne_Story_Faelin_Mission16Victory_02);
			yield return MissionPlayVO(friendlyActor, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission16Victory_03);
			yield return MissionPlayVO(GraceBrassRing, VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission16Victory_01);
			GameState.Get().SetBusy(busy: false);
			break;
		case 102:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(friendlyActor, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission16Victory_01);
			yield return MissionPlayVO(IniBrassRing, VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission16Victory_01);
			yield return MissionPlayVO(friendlyActor, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission16Victory_02);
			yield return MissionPlayVO(FinleyBrassRing, VO_Story_11_Finley_009hp_Male_Murloc_Story_Faelin_Mission16Victory_01);
			GameState.Get().SetBusy(busy: false);
			break;
		case 103:
			yield return MissionPlayVO(enemyActor, VO_Story_11_Azshara_016hb_Female_Naga_Story_Faelin_Mission16ExchangeA_01);
			yield return MissionPlayVO(HalusBrassRing, VO_Story_11_Halus_010hp_Male_BloodElf_Story_Faelin_Mission16ExchangeA_01);
			break;
		case 104:
			yield return MissionPlayVO(enemyActor, VO_Story_11_Azshara_016hb_Female_Naga_Story_Faelin_Mission16ExchangeA_01);
			yield return MissionPlayVO(GraceBrassRing, VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission16ExchangeA_01);
			yield return MissionPlayVO(friendlyActor, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission16ExchangeA_01);
			break;
		case 105:
			yield return MissionPlayVO(enemyActor, VO_Story_11_Azshara_016hb_Female_Naga_Story_Faelin_Mission16ExchangeA_01);
			yield return MissionPlayVO(FinleyBrassRing, VO_Story_11_Finley_009hp_Male_Murloc_Story_Faelin_Mission16ExchangeA_01);
			yield return MissionPlayVO(CayeBrassRing, VO_Story_11_Caye_012hp_Female_Nightborne_Story_Faelin_Mission16ExchangeA_01);
			break;
		case 106:
			yield return MissionPlayVO(enemyActor, VO_Story_11_Azshara_016hb_Female_Naga_Story_Faelin_Mission16ExchangeB_01);
			yield return MissionPlayVO(HalusBrassRing, VO_Story_11_Halus_010hp_Male_BloodElf_Story_Faelin_Mission16ExchangeB_01);
			break;
		case 107:
			yield return MissionPlayVO(enemyActor, VO_Story_11_Azshara_016hb_Female_Naga_Story_Faelin_Mission16ExchangeB_01);
			yield return MissionPlayVO(GraceBrassRing, VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission16ExchangeB_01);
			break;
		case 108:
			yield return MissionPlayVO(enemyActor, VO_Story_11_Azshara_016hb_Female_Naga_Story_Faelin_Mission16ExchangeB_01);
			break;
		case 109:
			yield return MissionPlayVO(enemyActor, VO_Story_11_Azshara_016hb_Female_Naga_Story_Faelin_Mission16ExchangeC_01);
			break;
		case 110:
			yield return MissionPlayVO(enemyActor, VO_Story_11_Azshara_016hb_Female_Naga_Story_Faelin_Mission16ExchangeC_01);
			yield return MissionPlayVO(FinleyBrassRing, VO_Story_11_Finley_009hp_Male_Murloc_Story_Faelin_Mission16ExchangeB_01);
			break;
		case 111:
			yield return MissionPlayVO(enemyActor, VO_Story_11_Azshara_016hb_Female_Naga_Story_Faelin_Mission16ExchangeD_01);
			break;
		case 112:
			yield return MissionPlayVO(enemyActor, VO_Story_11_Azshara_016hb_Female_Naga_Story_Faelin_Mission16ExchangeE_01);
			yield return MissionPlayVO(friendlyActor, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission16ExchangeE_01);
			break;
		case 113:
			yield return MissionPlayVO(enemyActor, VO_Story_11_Azshara_016hb_Female_Naga_Story_Faelin_Mission16ExchangeF_01);
			break;
		case 114:
			yield return MissionPlayVO(enemyActor, VO_Story_11_Azshara_016hb_Female_Naga_Story_Faelin_Mission16ExchangeG_01);
			break;
		case 115:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(friendlyActor, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission16ExchangeH_01);
			yield return MissionPlayVO(enemyActor, VO_Story_11_Azshara_016hb_Female_Naga_Story_Faelin_Mission16ExchangeH_01);
			yield return MissionPlayVO(friendlyActor, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission16ExchangeH_02);
			GameState.Get().SetBusy(busy: false);
			break;
		case 116:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(enemyActor, VO_Story_11_Azshara_016hb_Female_Naga_Story_Faelin_Mission16ExchangeI_01);
			yield return MissionPlayVO(HalusBrassRing, VO_Story_11_Halus_010hp_Male_BloodElf_Story_Faelin_Mission16ExchangeI_01);
			GameState.Get().SetBusy(busy: false);
			break;
		case 117:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(enemyActor, VO_Story_11_Azshara_016hb_Female_Naga_Story_Faelin_Mission16ExchangeI_01);
			yield return MissionPlayVO(GetEnemyActorByCardId("Story_11_Dathril2"), VO_Story_11_Dathril_012hb_Male_Naga_Story_Faelin_Mission16ExchangeI_01);
			GameState.Get().SetBusy(busy: false);
			break;
		case 118:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(GetFriendlyActorByCardId("Story_11_Dathril2"), VO_Story_11_Dathril_012hb_Male_Naga_Story_Faelin_Mission16ExchangeJ_01);
			yield return MissionPlayVO(GetFriendlyActorByCardId("Story_11_Dathril2"), VO_Story_11_Dathril_012hb_Male_Naga_Story_Faelin_Mission16ExchangeJ_02);
			GameState.Get().SetBusy(busy: false);
			break;
		case 119:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(friendlyActor, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission16ExchangeJ_01);
			yield return MissionPlayVO(CayeBrassRing, VO_Story_11_Caye_012hp_Female_Nightborne_Story_Faelin_Mission16ExchangeJ_01);
			GameState.Get().SetBusy(busy: false);
			break;
		case 228:
			if (!(m_popup != null))
			{
				GameState.Get().SetBusy(busy: true);
				m_popup = NotificationManager.Get().CreatePopupText(UserAttentionBlocker.NONE, popUpPos, TutorialEntity.GetTextScale() * popUpScale, GameStrings.Get(m_popUpInfo[missionEvent][0]), convertLegacyPosition: false, NotificationManager.PopupTextType.FANCY);
				yield return GameEntity.Coroutines.StartCoroutine(PlaySoundAndBlockSpeech(VO_Story_11_Dathril_012hb_Male_Naga_Story_Faelin_Mission16_Lorebook_1_01, Notification.SpeechBubbleDirection.None, GetEnemyActorByCardId("Story_11_Mission1_Lorebook1"), 2.5f));
				NotificationManager.Get().DestroyNotification(m_popup, 0f);
				m_popup = null;
				GameState.Get().SetBusy(busy: false);
			}
			break;
		case 229:
			if (!(m_popup != null))
			{
				GameState.Get().SetBusy(busy: true);
				m_popup = NotificationManager.Get().CreatePopupText(UserAttentionBlocker.NONE, popUpPos, TutorialEntity.GetTextScale() * popUpScale, GameStrings.Get(m_popUpInfo[missionEvent][0]), convertLegacyPosition: false, NotificationManager.PopupTextType.FANCY);
				yield return GameEntity.Coroutines.StartCoroutine(PlaySoundAndBlockSpeech(VO_Story_11_Azshara_016hb_Female_Naga_Mission16_Lorebook_2_01, Notification.SpeechBubbleDirection.None, GetEnemyActorByCardId("Story_11_Mission1_Lorebook1"), 2.5f));
				NotificationManager.Get().DestroyNotification(m_popup, 0f);
				m_popup = null;
				GameState.Get().SetBusy(busy: false);
			}
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
		GameState.Get().GetFriendlySidePlayer().GetHero()
			.GetCard()
			.GetActor();
		if (turn == 1)
		{
			yield return MissionPlayVO(enemyActor, VO_Story_11_Azshara_016hb_Female_Naga_Story_Faelin_Mission16Start_02);
		}
	}

	public override bool NotifyOfBattlefieldCardClicked(Entity clickedEntity, bool wasInTargetMode)
	{
		if (!base.NotifyOfBattlefieldCardClicked(clickedEntity, wasInTargetMode))
		{
			return false;
		}
		if (!wasInTargetMode)
		{
			if (clickedEntity.GetCardId() == "Story_11_Mission16_Lorebook1")
			{
				if (m_popup == null)
				{
					Network.Get().SendClientScriptGameEvent(ClientScriptGameEventType.MISSION_EVENT, 228);
					HandleMissionEvent(228);
				}
				return false;
			}
			if (clickedEntity.GetCardId() == "Story_11_Mission16_Lorebook2")
			{
				if (m_popup == null)
				{
					Network.Get().SendClientScriptGameEvent(ClientScriptGameEventType.MISSION_EVENT, 229);
					HandleMissionEvent(229);
				}
				return false;
			}
		}
		return true;
	}
}
