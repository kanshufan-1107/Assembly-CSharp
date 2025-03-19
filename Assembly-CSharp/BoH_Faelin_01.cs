using System.Collections;
using System.Collections.Generic;
using Blizzard.T5.Core;
using UnityEngine;

public class BoH_Faelin_01 : BoH_Faelin_Dungeon
{
	private static Map<GameEntityOption, bool> s_booleanOptions = InitBooleanOptions();

	private static readonly AssetReference VO_Story_11_Caye_012hp_Female_Nightborne_Story_Faelin_Mission1_Part2_01 = new AssetReference("VO_Story_11_Caye_012hp_Female_Nightborne_Story_Faelin_Mission1_Part2_01.prefab:3bc6602572d007841980bb9ecac1f243");

	private static readonly AssetReference VO_Story_11_Caye_012hp_Female_Nightborne_Story_Faelin_Mission1_Progress2_01 = new AssetReference("VO_Story_11_Caye_012hp_Female_Nightborne_Story_Faelin_Mission1_Progress2_01.prefab:d6aa3a0ccc7cac8418b478ce541dc2b1");

	private static readonly AssetReference VO_Story_11_Caye_012hp_Female_Nightborne_Story_Faelin_Mission1ExchangeB_01 = new AssetReference("VO_Story_11_Caye_012hp_Female_Nightborne_Story_Faelin_Mission1ExchangeB_01.prefab:70b10a0ef2a54e04d9fb04adfc406e4a");

	private static readonly AssetReference VO_Story_11_Caye_012hp_Female_Nightborne_Story_Faelin_Mission1ExchangeE_01 = new AssetReference("VO_Story_11_Caye_012hp_Female_Nightborne_Story_Faelin_Mission1ExchangeE_01.prefab:c3e8619798cd9714081cab4b91c89551");

	private static readonly AssetReference VO_Story_11_Caye_012hp_Female_Nightborne_Story_Faelin_Mission1Victory_01 = new AssetReference("VO_Story_11_Caye_012hp_Female_Nightborne_Story_Faelin_Mission1Victory_01.prefab:0a4fd8aa7ad22f14691352e5a6dc8573");

	private static readonly AssetReference VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission1ExchangeA_01 = new AssetReference("VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission1ExchangeA_01.prefab:7d82e06d97df3a84cad038070dc20b40");

	private static readonly AssetReference VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission1ExchangeEc_01 = new AssetReference("VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission1ExchangeEc_01.prefab:841dd05226fcbd04b8dae5991d26ac4a");

	private static readonly AssetReference VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission1Part2_01 = new AssetReference("VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission1Part2_01.prefab:bc301b6cf95d6d14997c57b6e4ef7209");

	private static readonly AssetReference VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission1Progress2_01 = new AssetReference("VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission1Progress2_01.prefab:ea4d71843d31af441afe51bb987e5538");

	private static readonly AssetReference VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission1Progress4_01 = new AssetReference("VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission1Progress4_01.prefab:6d4fecf6e7416034bb4b97443167f82b");

	private static readonly AssetReference VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission1Start_01 = new AssetReference("VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission1Start_01.prefab:1050285eb892d4147b6e91c3028631a1");

	private static readonly AssetReference VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission1Start_02 = new AssetReference("VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission1Start_02.prefab:a76ce4f36e130ae4883d6edd06beb288");

	private static readonly AssetReference VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission1Victory_01 = new AssetReference("VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission1Victory_01.prefab:5d88e7fd7d942a549bbc6d11828f89f4");

	private static readonly AssetReference VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_PreMission1_01 = new AssetReference("VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_PreMission1_01.prefab:611da30466a921e49aa5f90d4a969cc6");

	private static readonly AssetReference VO_Story_11_Finley_009hp_Male_Murloc_Story_Faelin_Mission1_Progress1_01 = new AssetReference("VO_Story_11_Finley_009hp_Male_Murloc_Story_Faelin_Mission1_Progress1_01.prefab:91a2db988c26a684aa401e51a867dd67");

	private static readonly AssetReference VO_Story_11_Finley_009hp_Male_Murloc_Story_Faelin_Mission1ExchangeA_01 = new AssetReference("VO_Story_11_Finley_009hp_Male_Murloc_Story_Faelin_Mission1ExchangeA_01.prefab:cd41b75acb9c68d44bfa7bb6f6b99beb");

	private static readonly AssetReference VO_Story_11_Finley_009hp_Male_Murloc_Story_Faelin_Mission1ExchangeB_01 = new AssetReference("VO_Story_11_Finley_009hp_Male_Murloc_Story_Faelin_Mission1ExchangeB_01.prefab:01089a0c737e2a14daed28f3c18f239d");

	private static readonly AssetReference VO_Story_11_Finley_009hp_Male_Murloc_Story_Faelin_Mission1ExchangeE_01 = new AssetReference("VO_Story_11_Finley_009hp_Male_Murloc_Story_Faelin_Mission1ExchangeE_01.prefab:e6fd5715fdecb764e85811064efd8198");

	private static readonly AssetReference VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission1_Progress1_01 = new AssetReference("VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission1_Progress1_01.prefab:b201d41f982024f40a5b57863e6aa98d");

	private static readonly AssetReference VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission1ExchangeA_01 = new AssetReference("VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission1ExchangeA_01.prefab:c46e5e94d76d2c74ebd0b8926a5c24ec");

	private static readonly AssetReference VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission1ExchangeB_01 = new AssetReference("VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission1ExchangeB_01.prefab:23015dd68ed631748bf5458b306b6169");

	private static readonly AssetReference VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission1ExchangeE_01 = new AssetReference("VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission1ExchangeE_01.prefab:8bedb9e1da4f2824c88829ddc5c829b6");

	private static readonly AssetReference VO_Story_11_Halus_010hp_Male_BloodElf_Story_Faelin_Mission1_Progress1_01 = new AssetReference("VO_Story_11_Halus_010hp_Male_BloodElf_Story_Faelin_Mission1_Progress1_01.prefab:09700c9bfbedf0b49b311287a2206e24");

	private static readonly AssetReference VO_Story_11_Halus_010hp_Male_BloodElf_Story_Faelin_Mission1ExchangeA_01 = new AssetReference("VO_Story_11_Halus_010hp_Male_BloodElf_Story_Faelin_Mission1ExchangeA_01.prefab:47f8266c9c38eaa40a8e9f94bd64318e");

	private static readonly AssetReference VO_Story_11_Halus_010hp_Male_BloodElf_Story_Faelin_Mission1ExchangeB_01 = new AssetReference("VO_Story_11_Halus_010hp_Male_BloodElf_Story_Faelin_Mission1ExchangeB_01.prefab:ef1868fe9046e174f910c413d9861b24");

	private static readonly AssetReference VO_Story_11_Halus_010hp_Male_BloodElf_Story_Faelin_Mission1ExchangeE_01 = new AssetReference("VO_Story_11_Halus_010hp_Male_BloodElf_Story_Faelin_Mission1ExchangeE_01.prefab:29f9c06dd052b7a4799603e1867087a4");

	private static readonly AssetReference VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission1_Lorebook_1_01 = new AssetReference("VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission1_Lorebook_1_01.prefab:c1aca8dcacaa6204dbc6f4ed37fdd8d2");

	private static readonly AssetReference VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission1_Lorebook_2_01 = new AssetReference("VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission1_Lorebook_2_01.prefab:55f5528948fd818438b16558f22f3e1c");

	private static readonly AssetReference VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission1_Part2_01 = new AssetReference("VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission1_Part2_01.prefab:77f9a06c1c95c6f409a36a5ee91e51b6");

	private static readonly AssetReference VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission1_Progress1_01 = new AssetReference("VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission1_Progress1_01.prefab:98e8589d54a30f045ad4512cea5683e0");

	private static readonly AssetReference VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission1_Progress1_02 = new AssetReference("VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission1_Progress1_02.prefab:62b828151b148dc43b902adf835dff73");

	private static readonly AssetReference VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission1_Progress1a_01 = new AssetReference("VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission1_Progress1a_01.prefab:a4030911e403f484698329ed0c02cdb0");

	private static readonly AssetReference VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission1_Progress1b_01 = new AssetReference("VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission1_Progress1b_01.prefab:3568838dff2dfdd42b152a09c15b2bec");

	private static readonly AssetReference VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission1_Progress1c_01 = new AssetReference("VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission1_Progress1c_01.prefab:8d092803de30da447a3dd9733f0a8718");

	private static readonly AssetReference VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission1_Progress2_01 = new AssetReference("VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission1_Progress2_01.prefab:504946e18c5bc44469184c17c13733f3");

	private static readonly AssetReference VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission1_Progress3_01 = new AssetReference("VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission1_Progress3_01.prefab:17caea4079c81854895e45eb8ae548d7");

	private static readonly AssetReference VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission1_Progress4_01 = new AssetReference("VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission1_Progress4_01.prefab:12cfefb4f7821d8499dbada6de779878");

	private static readonly AssetReference VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission1Start_01 = new AssetReference("VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission1Start_01.prefab:3d26428e1209015479287862f0ed8623");

	private static readonly AssetReference VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission1Victory_01 = new AssetReference("VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission1Victory_01.prefab:ef4a1f42a7f46524b86d1ea329b6c0a6");

	private static readonly AssetReference VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_PreMission1_01 = new AssetReference("VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_PreMission1_01.prefab:90ad383b2194ac8428ef2a9656d58c40");

	private static readonly AssetReference VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_PreMission1_02 = new AssetReference("VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_PreMission1_02.prefab:8be516a4ee67e2640b6bfc47f6df96e5");

	private static readonly AssetReference VO_Story_11_Patrol_001hb_Female_Nightborne_Story_Faelin_Mission1EmoteResponse_01 = new AssetReference("VO_Story_11_Patrol_001hb_Female_Nightborne_Story_Faelin_Mission1EmoteResponse_01.prefab:976f2dc0a4296a549b9529107671ad66");

	private static readonly AssetReference VO_Story_11_Patrol_001hb_Female_Nightborne_Story_Faelin_Mission1ExchangeBa_01 = new AssetReference("VO_Story_11_Patrol_001hb_Female_Nightborne_Story_Faelin_Mission1ExchangeBa_01.prefab:b5e6150ef95a47446a5ef6a18ab11510");

	private static readonly AssetReference VO_Story_11_Patrol_001hb_Female_Nightborne_Story_Faelin_Mission1ExchangeBc_01 = new AssetReference("VO_Story_11_Patrol_001hb_Female_Nightborne_Story_Faelin_Mission1ExchangeBc_01.prefab:e9940925329ab0449af6a7199c0a01c4");

	private static readonly AssetReference VO_Story_11_Patrol_001hb_Female_Nightborne_Story_Faelin_Mission1ExchangeEa_01 = new AssetReference("VO_Story_11_Patrol_001hb_Female_Nightborne_Story_Faelin_Mission1ExchangeEa_01.prefab:cabf70c1c39b0204ebd4c1a4d3c99bb8");

	private static readonly AssetReference VO_Story_11_Patrol_001hb_Female_Nightborne_Story_Faelin_Mission1ExchangeEb_01 = new AssetReference("VO_Story_11_Patrol_001hb_Female_Nightborne_Story_Faelin_Mission1ExchangeEb_01.prefab:ded2d99cc211b1c45ae2f38fdef99a6e");

	private static readonly AssetReference VO_Story_11_Patrol_001hb_Female_Nightborne_Story_Faelin_Mission1HeroPower_01 = new AssetReference("VO_Story_11_Patrol_001hb_Female_Nightborne_Story_Faelin_Mission1HeroPower_01.prefab:8c20506f88a884f4f973bf6d805bb387");

	private static readonly AssetReference VO_Story_11_Patrol_001hb_Female_Nightborne_Story_Faelin_Mission1HeroPower_02 = new AssetReference("VO_Story_11_Patrol_001hb_Female_Nightborne_Story_Faelin_Mission1HeroPower_02.prefab:323f448253b1dbd4b9ba7f2d5225c578");

	private static readonly AssetReference VO_Story_11_Patrol_001hb_Female_Nightborne_Story_Faelin_Mission1HeroPower_03 = new AssetReference("VO_Story_11_Patrol_001hb_Female_Nightborne_Story_Faelin_Mission1HeroPower_03.prefab:f3d95f1a3d81206409e49a464e82d7c6");

	private static readonly AssetReference VO_Story_11_Patrol_001hb_Female_Nightborne_Story_Faelin_Mission1Idle_01 = new AssetReference("VO_Story_11_Patrol_001hb_Female_Nightborne_Story_Faelin_Mission1Idle_01.prefab:7d35f7dec8a873f47ae20d57b1dc772e");

	private static readonly AssetReference VO_Story_11_Patrol_001hb_Female_Nightborne_Story_Faelin_Mission1Idle_02 = new AssetReference("VO_Story_11_Patrol_001hb_Female_Nightborne_Story_Faelin_Mission1Idle_02.prefab:251b031ae63b1314e802647bee4d615f");

	private static readonly AssetReference VO_Story_11_Patrol_001hb_Female_Nightborne_Story_Faelin_Mission1Idle_03 = new AssetReference("VO_Story_11_Patrol_001hb_Female_Nightborne_Story_Faelin_Mission1Idle_03.prefab:f4b798c62d6f17c4a9406130c4c27942");

	private static readonly AssetReference VO_Story_11_Patrol_001hb_Female_Nightborne_Story_Faelin_Mission1Loss_01 = new AssetReference("VO_Story_11_Patrol_001hb_Female_Nightborne_Story_Faelin_Mission1Loss_01.prefab:1e98928c818e61d48bddc28e60b774a2");

	private static readonly AssetReference VO_Story_11_Patrol_001hb_Female_Nightborne_Story_Faelin_Mission1Part2_01 = new AssetReference("VO_Story_11_Patrol_001hb_Female_Nightborne_Story_Faelin_Mission1Part2_01.prefab:a1db5063a86b7554d93c5b1946e5c454");

	private static readonly AssetReference VO_Story_11_Patrol_001hb_Female_Nightborne_Story_Faelin_Mission1Victory_01 = new AssetReference("VO_Story_11_Patrol_001hb_Female_Nightborne_Story_Faelin_Mission1Victory_01.prefab:109b2fa097a0e0447b578d01979c852a");

	private static readonly AssetReference Story_11_Leviathan_001hb_Rev_Sound = new AssetReference("Story_11_Leviathan_001hb_Rev_Sound.prefab:63e2a91d4276f1a429b25b7ddcddd53d");

	private static readonly AssetReference Story_11_Leviathan_001hb_EmoteResponse_Sound = new AssetReference("Story_11_Leviathan_001hb_EmoteResponse_Sound.prefab:90b2aae30ca06f648a8033e394c1fa22");

	private static readonly AssetReference Story_11_Leviathan_001hb_Loss_Sound = new AssetReference("Story_11_Leviathan_001hb_Loss_Sound.prefab:c8702d2dff96aa1468e091e19a0f99cb");

	private static readonly AssetReference Story_11_LeviathanEngine_Boiler_Fixed = new AssetReference("Story_11_LeviathanEngine_Boiler_Fixed.prefab:36d2920917700e7438ce97cd7131c417");

	private static readonly AssetReference Story_11_LeviathanEngine_FuelTank_Fixed = new AssetReference("Story_11_LeviathanEngine_FuelTank_Fixed.prefab:ac48428147c093e46b5ed85958d67c5a");

	private static readonly AssetReference Story_11_LeviathanEngine_PressureValve_Fixed = new AssetReference("Story_11_LeviathanEngine_PressureValve_Fixed.prefab:334181be58fb57d4e9844619fe5a3c51");

	private static readonly AssetReference EX1_006_AlarmOBot_turn_start_effect = new AssetReference("EX1_006_AlarmOBot_turn_start_effect.prefab:fef95b018442e46f6b67e2f99dbc3932");

	private static readonly AssetReference VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission8_Reaction_01 = new AssetReference("VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission8_Reaction_01.prefab:3a5e940ad8c7aed4a9788c7336e3c57e");

	private static readonly AssetReference VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission8_Reaction_02 = new AssetReference("VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission8_Reaction_02.prefab:07a4ee4bfb204fd44a4c03656d53ed57");

	private static readonly AssetReference VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission8_Reaction_03 = new AssetReference("VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission8_Reaction_03.prefab:4f95caa9522cbe14ea4e9f5c0c7a9f9c");

	private Notification m_popup;

	private Dictionary<int, string[]> m_popUpInfo = new Dictionary<int, string[]>
	{
		{
			230,
			new string[1] { "BOH_FAELIN_MISSION1_LOREBOOK_01" }
		},
		{
			229,
			new string[1] { "BOH_FAELIN_MISSION1_LOREBOOK_02" }
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

	private new List<string> m_BossIdleLines = new List<string> { VO_Story_11_Patrol_001hb_Female_Nightborne_Story_Faelin_Mission1Idle_01, VO_Story_11_Patrol_001hb_Female_Nightborne_Story_Faelin_Mission1Idle_02, VO_Story_11_Patrol_001hb_Female_Nightborne_Story_Faelin_Mission1Idle_03 };

	private List<string> m_InGame_BossUsesHeroPowerLines = new List<string> { VO_Story_11_Patrol_001hb_Female_Nightborne_Story_Faelin_Mission1HeroPower_01, VO_Story_11_Patrol_001hb_Female_Nightborne_Story_Faelin_Mission1HeroPower_02, VO_Story_11_Patrol_001hb_Female_Nightborne_Story_Faelin_Mission1HeroPower_03 };

	private List<string> m_NegativeReactionLines = new List<string> { VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission8_Reaction_01, VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission8_Reaction_02, VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission8_Reaction_03 };

	private HashSet<string> m_playedLines = new HashSet<string>();

	private static Map<GameEntityOption, bool> InitBooleanOptions()
	{
		return new Map<GameEntityOption, bool> { 
		{
			GameEntityOption.DO_OPENING_TAUNTS,
			false
		} };
	}

	public override float GetThinkIdleChancePercentage()
	{
		return 0.1f;
	}

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> m_SoundFiles = new List<string>
		{
			VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission1Start_01, VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission1Start_01, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission1Start_02, VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission1_Progress1_01, VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission1_Progress1_02, VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission1_Progress1_01, VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission1_Progress1a_01, VO_Story_11_Halus_010hp_Male_BloodElf_Story_Faelin_Mission1_Progress1_01, VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission1_Progress1b_01, VO_Story_11_Finley_009hp_Male_Murloc_Story_Faelin_Mission1_Progress1_01,
			VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission1_Progress1c_01, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission1Progress2_01, VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission1_Progress2_01, VO_Story_11_Caye_012hp_Female_Nightborne_Story_Faelin_Mission1_Progress2_01, VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission1_Progress3_01, VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission1_Progress4_01, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission1Progress4_01, VO_Story_11_Caye_012hp_Female_Nightborne_Story_Faelin_Mission1_Part2_01, VO_Story_11_Patrol_001hb_Female_Nightborne_Story_Faelin_Mission1Part2_01, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission1Part2_01,
			VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission1_Part2_01, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission1ExchangeA_01, VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission1ExchangeA_01, VO_Story_11_Finley_009hp_Male_Murloc_Story_Faelin_Mission1ExchangeA_01, VO_Story_11_Halus_010hp_Male_BloodElf_Story_Faelin_Mission1ExchangeA_01, VO_Story_11_Patrol_001hb_Female_Nightborne_Story_Faelin_Mission1ExchangeBa_01, VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission1ExchangeB_01, VO_Story_11_Finley_009hp_Male_Murloc_Story_Faelin_Mission1ExchangeB_01, VO_Story_11_Caye_012hp_Female_Nightborne_Story_Faelin_Mission1ExchangeB_01, VO_Story_11_Patrol_001hb_Female_Nightborne_Story_Faelin_Mission1ExchangeBc_01,
			VO_Story_11_Halus_010hp_Male_BloodElf_Story_Faelin_Mission1ExchangeB_01, VO_Story_11_Patrol_001hb_Female_Nightborne_Story_Faelin_Mission1ExchangeEa_01, VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission1ExchangeE_01, VO_Story_11_Caye_012hp_Female_Nightborne_Story_Faelin_Mission1ExchangeE_01, VO_Story_11_Finley_009hp_Male_Murloc_Story_Faelin_Mission1ExchangeE_01, VO_Story_11_Patrol_001hb_Female_Nightborne_Story_Faelin_Mission1ExchangeEb_01, VO_Story_11_Halus_010hp_Male_BloodElf_Story_Faelin_Mission1ExchangeE_01, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission1ExchangeEc_01, VO_Story_11_Patrol_001hb_Female_Nightborne_Story_Faelin_Mission1Victory_01, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission1Victory_01,
			VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission1Victory_01, VO_Story_11_Caye_012hp_Female_Nightborne_Story_Faelin_Mission1Victory_01, VO_Story_11_Patrol_001hb_Female_Nightborne_Story_Faelin_Mission1EmoteResponse_01, VO_Story_11_Patrol_001hb_Female_Nightborne_Story_Faelin_Mission1HeroPower_01, VO_Story_11_Patrol_001hb_Female_Nightborne_Story_Faelin_Mission1HeroPower_02, VO_Story_11_Patrol_001hb_Female_Nightborne_Story_Faelin_Mission1HeroPower_03, VO_Story_11_Patrol_001hb_Female_Nightborne_Story_Faelin_Mission1Loss_01, VO_Story_11_Patrol_001hb_Female_Nightborne_Story_Faelin_Mission1Idle_01, VO_Story_11_Patrol_001hb_Female_Nightborne_Story_Faelin_Mission1Idle_02, VO_Story_11_Patrol_001hb_Female_Nightborne_Story_Faelin_Mission1Idle_03,
			VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission8_Reaction_01, VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission8_Reaction_02, VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission8_Reaction_03, VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission1_Lorebook_1_01, VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission1_Lorebook_2_01, Story_11_Leviathan_001hb_Rev_Sound, Story_11_Leviathan_001hb_EmoteResponse_Sound, Story_11_Leviathan_001hb_Loss_Sound, Story_11_LeviathanEngine_Boiler_Fixed, Story_11_LeviathanEngine_FuelTank_Fixed,
			Story_11_LeviathanEngine_PressureValve_Fixed, EX1_006_AlarmOBot_turn_start_effect
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

	public override bool ShouldDoAlternateMulliganIntro()
	{
		return true;
	}

	public override List<string> GetBossIdleLines()
	{
		return m_BossIdleLines;
	}

	public override void OnCreateGame()
	{
		base.OnCreateGame();
		m_OverrideMusicTrack = MusicPlaylistType.InGame_TSC_Leviathan;
	}

	protected override IEnumerator HandleMissionEventWithTiming(int missionEvent)
	{
		while (m_enemySpeaking)
		{
			yield return null;
		}
		if (missionEvent < 0)
		{
			Debug.LogError($"BoH_Faelin.HandleMissionEventWithTiming(): missionEvent < 0");
			yield break;
		}
		GameState gameState = GameState.Get();
		if (gameState == null)
		{
			Debug.LogError($"BoH_Faelin.HandleMissionEventWithTiming(): GameState is null");
			yield break;
		}
		Player opposingSidePlayer = gameState.GetOpposingSidePlayer();
		if (opposingSidePlayer == null)
		{
			Debug.LogError($"BoH_Faelin.HandleMissionEventWithTiming(): opposingSidePlayer is null");
			yield break;
		}
		Entity opposingSidePlayersHero = opposingSidePlayer.GetHero();
		if (opposingSidePlayersHero == null)
		{
			Debug.LogError($"BoH_Faelin.HandleMissionEventWithTiming(): opposingSidePlayersHero is null");
			yield break;
		}
		Card opposingHeroCard = opposingSidePlayersHero.GetCard();
		if (opposingHeroCard == null)
		{
			Debug.LogError($"BoH_Faelin.HandleMissionEventWithTiming(): opposingHeroCard is null");
			yield break;
		}
		string opposingHeroCardId = opposingSidePlayersHero.GetCardId();
		if (string.IsNullOrEmpty(opposingHeroCardId))
		{
			Debug.LogError($"BoH_Faelin.HandleMissionEventWithTiming(): opposingHeroCardId is null");
			yield break;
		}
		Actor enemyActor = opposingHeroCard.GetActor();
		if (enemyActor == null)
		{
			Debug.LogError($"BoH_Faelin.HandleMissionEventWithTiming(): enemyActor is null");
			yield break;
		}
		Player friendlySidePlayer = gameState.GetFriendlySidePlayer();
		if (friendlySidePlayer == null)
		{
			Debug.LogError($"BoH_Faelin.HandleMissionEventWithTiming(): friendlySidePlayer is null");
			yield break;
		}
		Entity friendlySidePlayersHero = friendlySidePlayer.GetHero();
		if (friendlySidePlayersHero == null)
		{
			Debug.LogError($"BoH_Faelin.HandleMissionEventWithTiming(): friendlySidePlayersHero is null");
			yield break;
		}
		Card friendlySidePlayersHeroCard = friendlySidePlayersHero.GetCard();
		if (friendlySidePlayersHeroCard == null)
		{
			Debug.LogError($"BoH_Faelin.HandleMissionEventWithTiming(): friendlySidePlayersHeroCard is null");
			yield break;
		}
		Actor friendlyActor = friendlySidePlayersHeroCard.GetActor();
		if (friendlyActor == null)
		{
			Debug.LogError($"BoH_Faelin.HandleMissionEventWithTiming(): friendlyActor is null");
			yield break;
		}
		Gameplay gameplay = Gameplay.Get();
		if (gameplay == null)
		{
			Debug.LogError($"BoH_Faelin.HandleMissionEventWithTiming(): Gameplay is null");
			yield break;
		}
		NameBanner nameBannerForSide = gameplay.GetNameBannerForSide(Player.Side.OPPOSING);
		if (nameBannerForSide != null)
		{
			nameBannerForSide.SetName("");
			nameBannerForSide.UpdateHeroNameBanner();
		}
		popUpPos = new Vector3(0f, 0f, -40f);
		switch (missionEvent)
		{
		case 515:
			if (string.Equals(opposingHeroCardId, "Story_11_Patrol_001hb"))
			{
				yield return MissionPlayVO(enemyActor, VO_Story_11_Patrol_001hb_Female_Nightborne_Story_Faelin_Mission1EmoteResponse_01);
			}
			else
			{
				yield return MissionPlaySound(enemyActor, Story_11_Leviathan_001hb_EmoteResponse_Sound);
			}
			break;
		case 119:
			yield return MissionPlayVO(FaelinBrassRing, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission1Start_01);
			yield return MissionPlayVO(friendlyActor, VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission1Start_01);
			yield return MissionPlayVO(FaelinBrassRing, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission1Start_02);
			break;
		case 507:
			gameState.SetBusy(busy: true);
			yield return MissionPlayVO(enemyActor, VO_Story_11_Patrol_001hb_Female_Nightborne_Story_Faelin_Mission1Loss_01);
			gameState.SetBusy(busy: false);
			break;
		case 120:
			gameState.SetBusy(busy: true);
			yield return MissionPlaySound(enemyActor, Story_11_Leviathan_001hb_Loss_Sound);
			gameState.SetBusy(busy: false);
			break;
		case 504:
			gameState.SetBusy(busy: true);
			yield return MissionPlayVO(enemyActor, VO_Story_11_Patrol_001hb_Female_Nightborne_Story_Faelin_Mission1Victory_01);
			yield return MissionPlayVO(friendlyActor, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission1Victory_01);
			yield return MissionPlayVO(IniBrassRing, VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission1Victory_01);
			yield return MissionPlayVO(CayeBrassRing, VO_Story_11_Caye_012hp_Female_Nightborne_Story_Faelin_Mission1Victory_01);
			gameState.SetBusy(busy: false);
			break;
		case 121:
			yield return MissionPlaySound(friendlyActor, Story_11_LeviathanEngine_Boiler_Fixed);
			break;
		case 122:
			yield return MissionPlaySound(friendlyActor, Story_11_LeviathanEngine_FuelTank_Fixed);
			break;
		case 123:
			yield return MissionPlaySound(friendlyActor, Story_11_LeviathanEngine_PressureValve_Fixed);
			break;
		case 101:
			yield return MissionPlayVO(GraceBrassRing, VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission1_Progress1_01);
			yield return MissionPlayVO(friendlyActor, VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission1_Progress1a_01);
			break;
		case 102:
			yield return MissionPlayVO(HalusBrassRing, VO_Story_11_Halus_010hp_Male_BloodElf_Story_Faelin_Mission1_Progress1_01);
			yield return MissionPlayVO(friendlyActor, VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission1_Progress1b_01);
			break;
		case 103:
			yield return MissionPlayVO(FinleyBrassRing, VO_Story_11_Finley_009hp_Male_Murloc_Story_Faelin_Mission1_Progress1_01);
			yield return MissionPlayVO(friendlyActor, VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission1_Progress1c_01);
			break;
		case 104:
			yield return MissionPlayVO(FaelinBrassRing, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission1Progress2_01);
			yield return MissionPlayVO(friendlyActor, VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission1_Progress2_01);
			yield return MissionPlayVO(CayeBrassRing, VO_Story_11_Caye_012hp_Female_Nightborne_Story_Faelin_Mission1_Progress2_01);
			break;
		case 105:
			yield return MissionPlayVO(friendlyActor, VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission1_Progress3_01);
			break;
		case 106:
			yield return MissionPlaySound(enemyActor, Story_11_Leviathan_001hb_Rev_Sound);
			yield return MissionPlayVO(friendlyActor, VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission1_Progress4_01);
			yield return MissionPlayVO(FaelinBrassRing, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission1Progress4_01);
			break;
		case 107:
			yield return MissionPlaySound(enemyActor, EX1_006_AlarmOBot_turn_start_effect);
			yield return MissionPlayVO(CayeBrassRing, VO_Story_11_Caye_012hp_Female_Nightborne_Story_Faelin_Mission1_Part2_01);
			yield return MissionPlayVO(enemyActor, VO_Story_11_Patrol_001hb_Female_Nightborne_Story_Faelin_Mission1Part2_01);
			yield return MissionPlayVO(friendlyActor, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission1Part2_01);
			yield return MissionPlayVO(IniBrassRing, VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission1_Part2_01);
			yield return MissionPlayVO(friendlyActor, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission1ExchangeA_01);
			yield return MissionPlayVO(HalusBrassRing, VO_Story_11_Halus_010hp_Male_BloodElf_Story_Faelin_Mission1ExchangeA_01);
			break;
		case 108:
			yield return MissionPlaySound(enemyActor, EX1_006_AlarmOBot_turn_start_effect);
			yield return MissionPlayVO(CayeBrassRing, VO_Story_11_Caye_012hp_Female_Nightborne_Story_Faelin_Mission1_Part2_01);
			yield return MissionPlayVO(enemyActor, VO_Story_11_Patrol_001hb_Female_Nightborne_Story_Faelin_Mission1Part2_01);
			yield return MissionPlayVO(friendlyActor, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission1Part2_01);
			yield return MissionPlayVO(IniBrassRing, VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission1_Part2_01);
			yield return MissionPlayVO(friendlyActor, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission1ExchangeA_01);
			yield return MissionPlayVO(GraceBrassRing, VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission1ExchangeA_01);
			break;
		case 109:
			yield return MissionPlaySound(enemyActor, EX1_006_AlarmOBot_turn_start_effect);
			yield return MissionPlayVO(CayeBrassRing, VO_Story_11_Caye_012hp_Female_Nightborne_Story_Faelin_Mission1_Part2_01);
			yield return MissionPlayVO(enemyActor, VO_Story_11_Patrol_001hb_Female_Nightborne_Story_Faelin_Mission1Part2_01);
			yield return MissionPlayVO(friendlyActor, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission1Part2_01);
			yield return MissionPlayVO(IniBrassRing, VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission1_Part2_01);
			yield return MissionPlayVO(friendlyActor, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission1ExchangeA_01);
			yield return MissionPlayVO(FinleyBrassRing, VO_Story_11_Finley_009hp_Male_Murloc_Story_Faelin_Mission1ExchangeA_01);
			break;
		case 110:
			yield return MissionPlayVO(enemyActor, VO_Story_11_Patrol_001hb_Female_Nightborne_Story_Faelin_Mission1ExchangeBc_01);
			yield return MissionPlayVO(HalusBrassRing, VO_Story_11_Halus_010hp_Male_BloodElf_Story_Faelin_Mission1ExchangeB_01);
			break;
		case 111:
			yield return MissionPlayVO(enemyActor, VO_Story_11_Patrol_001hb_Female_Nightborne_Story_Faelin_Mission1ExchangeBa_01);
			yield return MissionPlayVO(GraceBrassRing, VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission1ExchangeB_01);
			yield return MissionPlayVO(CayeBrassRing, VO_Story_11_Caye_012hp_Female_Nightborne_Story_Faelin_Mission1ExchangeB_01);
			break;
		case 112:
			yield return MissionPlayVO(enemyActor, VO_Story_11_Patrol_001hb_Female_Nightborne_Story_Faelin_Mission1ExchangeBa_01);
			yield return MissionPlayVO(FinleyBrassRing, VO_Story_11_Finley_009hp_Male_Murloc_Story_Faelin_Mission1ExchangeB_01);
			yield return MissionPlayVO(CayeBrassRing, VO_Story_11_Caye_012hp_Female_Nightborne_Story_Faelin_Mission1ExchangeB_01);
			break;
		case 113:
			yield return MissionPlayVO(enemyActor, VO_Story_11_Patrol_001hb_Female_Nightborne_Story_Faelin_Mission1ExchangeEb_01);
			yield return MissionPlayVO(HalusBrassRing, VO_Story_11_Halus_010hp_Male_BloodElf_Story_Faelin_Mission1ExchangeE_01);
			yield return MissionPlayVO(friendlyActor, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission1ExchangeEc_01);
			break;
		case 114:
			yield return MissionPlayVO(enemyActor, VO_Story_11_Patrol_001hb_Female_Nightborne_Story_Faelin_Mission1ExchangeEa_01);
			yield return MissionPlayVO(GraceBrassRing, VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission1ExchangeE_01);
			break;
		case 115:
			yield return MissionPlayVO(CayeBrassRing, VO_Story_11_Caye_012hp_Female_Nightborne_Story_Faelin_Mission1ExchangeE_01);
			yield return MissionPlayVO(FinleyBrassRing, VO_Story_11_Finley_009hp_Male_Murloc_Story_Faelin_Mission1ExchangeE_01);
			break;
		case 116:
			yield return MissionPlayVOOnce(enemyActor, m_InGame_BossUsesHeroPowerLines);
			break;
		case 124:
			yield return PlayLineInOrderOnce(friendlyActor, m_NegativeReactionLines);
			break;
		case 229:
		{
			if (m_popup != null)
			{
				break;
			}
			if (m_popUpInfo == null)
			{
				Debug.LogError($"BoH_Faelin.HandleMissionEventWithTiming(): m_popUpInfo is null");
				break;
			}
			string[] gameStringKeys2 = null;
			if (!m_popUpInfo.TryGetValue(missionEvent, out gameStringKeys2))
			{
				Debug.LogError($"BoH_Faelin.HandleMissionEventWithTiming(): gameStringKeys is null");
				break;
			}
			if (gameStringKeys2.Length == 0)
			{
				Debug.LogError($"BoH_Faelin.HandleMissionEventWithTiming(): gameStringKeys is empty");
				break;
			}
			NotificationManager notificationManager = NotificationManager.Get();
			if (notificationManager == null)
			{
				Debug.LogError($"BoH_Faelin.HandleMissionEventWithTiming(): notificationManager is null");
				break;
			}
			gameState.SetBusy(busy: true);
			string gameStringKey2 = gameStringKeys2[0];
			m_popup = notificationManager.CreatePopupText(UserAttentionBlocker.NONE, popUpPos, TutorialEntity.GetTextScale() * popUpScale, GameStrings.Get(gameStringKey2), convertLegacyPosition: false, NotificationManager.PopupTextType.FANCY);
			yield return GameEntity.Coroutines.StartCoroutine(PlaySoundAndBlockSpeech(VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission1_Lorebook_1_01, Notification.SpeechBubbleDirection.None, GetEnemyActorByCardId("Story_11_Mission1_Lorebook1"), 2.5f));
			notificationManager.DestroyNotification(m_popup, 0f);
			m_popup = null;
			gameState.SetBusy(busy: false);
			break;
		}
		case 230:
		{
			if (m_popup != null)
			{
				break;
			}
			if (m_popUpInfo == null)
			{
				Debug.LogError($"BoH_Faelin.HandleMissionEventWithTiming(): m_popUpInfo is null");
				break;
			}
			string[] gameStringKeys = null;
			if (!m_popUpInfo.TryGetValue(missionEvent, out gameStringKeys))
			{
				Debug.LogError($"BoH_Faelin.HandleMissionEventWithTiming(): gameStringKeys is null");
				break;
			}
			if (gameStringKeys.Length == 0)
			{
				Debug.LogError($"BoH_Faelin.HandleMissionEventWithTiming(): gameStringKeys is empty");
				break;
			}
			NotificationManager notificationManager = NotificationManager.Get();
			if (notificationManager == null)
			{
				Debug.LogError($"BoH_Faelin.HandleMissionEventWithTiming(): notificationManager is null");
				break;
			}
			gameState.SetBusy(busy: true);
			string gameStringKey = gameStringKeys[0];
			m_popup = notificationManager.CreatePopupText(UserAttentionBlocker.NONE, popUpPos, TutorialEntity.GetTextScale() * popUpScale, GameStrings.Get(gameStringKey), convertLegacyPosition: false, NotificationManager.PopupTextType.FANCY);
			yield return GameEntity.Coroutines.StartCoroutine(PlaySoundAndBlockSpeech(VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission1_Lorebook_2_01, Notification.SpeechBubbleDirection.None, GetEnemyActorByCardId("Story_11_Mission1_Lorebook2"), 2.5f));
			notificationManager.DestroyNotification(m_popup, 0f);
			m_popup = null;
			gameState.SetBusy(busy: false);
			break;
		}
		case 231:
		{
			opposingSidePlayer.UpdateDisplayInfo();
			NameBanner nameBanner = gameplay.GetNameBannerForSide(Player.Side.OPPOSING);
			if (nameBanner != null)
			{
				nameBanner.UpdateHeroNameBanner();
			}
			break;
		}
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
			if (opposingHeroCard == "Story_11_Patrol_001hb")
			{
				string voLine = PopRandomLine(m_BossIdleLinesCopy);
				if (m_BossIdleLinesCopy.Count == 0)
				{
					m_BossIdleLinesCopy = new List<string>(m_BossIdleLines);
				}
				yield return MissionPlayVO(enemyActor, voLine);
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
		if (turn == 1)
		{
			yield return MissionPlayVO(IniBrassRing, VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission1_Progress1_01);
			yield return MissionPlayVO(IniBrassRing, VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission1_Progress1_02);
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
			if (clickedEntity.GetCardId() == "Story_11_Mission1_Lorebook1")
			{
				if (m_popup == null)
				{
					Network.Get().SendClientScriptGameEvent(ClientScriptGameEventType.MISSION_EVENT, 229);
					HandleMissionEvent(229);
				}
				return false;
			}
			if (clickedEntity.GetCardId() == "Story_11_Mission1_Lorebook2")
			{
				if (m_popup == null)
				{
					Network.Get().SendClientScriptGameEvent(ClientScriptGameEventType.MISSION_EVENT, 230);
					HandleMissionEvent(230);
				}
				return false;
			}
		}
		return true;
	}
}
