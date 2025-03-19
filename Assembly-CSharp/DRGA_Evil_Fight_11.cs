using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DRGA_Evil_Fight_11 : DRGA_Dungeon
{
	private static readonly AssetReference VO_DRGA_BOSS_01h_Male_Human_Evil_Fight_11_Boss_Death_Reno_01 = new AssetReference("VO_DRGA_BOSS_01h_Male_Human_Evil_Fight_11_Boss_Death_Reno_01.prefab:77f38ef3df3289f408454040b4ecae62");

	private static readonly AssetReference VO_DRGA_BOSS_01h_Male_Human_Evil_Fight_11_Boss_Death_Reno_Alt_01 = new AssetReference("VO_DRGA_BOSS_01h_Male_Human_Evil_Fight_11_Boss_Death_Reno_Alt_01.prefab:19f80b1782bc2824bbe12d1689f405d5");

	private static readonly AssetReference VO_DRGA_BOSS_01h_Male_Human_Evil_Fight_11_Boss_HeroPower_01_Reno_01 = new AssetReference("VO_DRGA_BOSS_01h_Male_Human_Evil_Fight_11_Boss_HeroPower_01_Reno_01.prefab:e5c37c5b5fff8b548ac5b9f14914efa6");

	private static readonly AssetReference VO_DRGA_BOSS_01h_Male_Human_Evil_Fight_11_Boss_HeroPower_02_Reno_01 = new AssetReference("VO_DRGA_BOSS_01h_Male_Human_Evil_Fight_11_Boss_HeroPower_02_Reno_01.prefab:4a98f209662f2e94398e72af80852789");

	private static readonly AssetReference VO_DRGA_BOSS_01h_Male_Human_Evil_Fight_11_Boss_HeroPower_03_Reno_01 = new AssetReference("VO_DRGA_BOSS_01h_Male_Human_Evil_Fight_11_Boss_HeroPower_03_Reno_01.prefab:895da455c05a5be4888b93d183468ebd");

	private static readonly AssetReference VO_DRGA_BOSS_01h_Male_Human_Evil_Fight_11_Boss_HeroPower_04_Reno_01 = new AssetReference("VO_DRGA_BOSS_01h_Male_Human_Evil_Fight_11_Boss_HeroPower_04_Reno_01.prefab:d89aa8ac4f866ef40b06a949830cfff3");

	private static readonly AssetReference VO_DRGA_BOSS_01h_Male_Human_Evil_Fight_11_Boss_HeroPower_05_Reno_01 = new AssetReference("VO_DRGA_BOSS_01h_Male_Human_Evil_Fight_11_Boss_HeroPower_05_Reno_01.prefab:3c617a2310ed4af4dab5d45730d9d8d2");

	private static readonly AssetReference VO_DRGA_BOSS_01h_Male_Human_Evil_Fight_11_BossAttack_Reno_01 = new AssetReference("VO_DRGA_BOSS_01h_Male_Human_Evil_Fight_11_BossAttack_Reno_01.prefab:321e819cd8b3b9541910c49db7f31d5a");

	private static readonly AssetReference VO_DRGA_BOSS_01h_Male_Human_Evil_Fight_11_BossStart_Reno_01 = new AssetReference("VO_DRGA_BOSS_01h_Male_Human_Evil_Fight_11_BossStart_Reno_01.prefab:7f767113c34f56f439cd4006881a9480");

	private static readonly AssetReference VO_DRGA_BOSS_01h_Male_Human_Evil_Fight_11_EmoteResponse_Reno_01 = new AssetReference("VO_DRGA_BOSS_01h_Male_Human_Evil_Fight_11_EmoteResponse_Reno_01.prefab:af0b69c14b688fb45b62cbf43cd2e3f8");

	private static readonly AssetReference VO_DRGA_BOSS_01h_Male_Human_Evil_Fight_11_Idle_01_Reno_01 = new AssetReference("VO_DRGA_BOSS_01h_Male_Human_Evil_Fight_11_Idle_01_Reno_01.prefab:eeac0c3fb63d72747ab45ff2415badbe");

	private static readonly AssetReference VO_DRGA_BOSS_01h_Male_Human_Evil_Fight_11_Idle_02_Reno_01 = new AssetReference("VO_DRGA_BOSS_01h_Male_Human_Evil_Fight_11_Idle_02_Reno_01.prefab:61d8549a5c5144e4dbe2c21de591bb74");

	private static readonly AssetReference VO_DRGA_BOSS_01h_Male_Human_Evil_Fight_11_Idle_03_Reno_01 = new AssetReference("VO_DRGA_BOSS_01h_Male_Human_Evil_Fight_11_Idle_03_Reno_01.prefab:983f39cbca91e5e49a53f520663b68e1");

	private static readonly AssetReference VO_DRGA_BOSS_07h_Male_Ethereal_Evil_Fight_11_Galakrond_Awesome_01_01 = new AssetReference("VO_DRGA_BOSS_07h_Male_Ethereal_Evil_Fight_11_Galakrond_Awesome_01_01.prefab:5bf1baf4a4278384b9b548cf8451f9ab");

	private static readonly AssetReference VO_DRGA_BOSS_07h_Male_Ethereal_Evil_Fight_11_Galakrond_Awesome_02_01 = new AssetReference("VO_DRGA_BOSS_07h_Male_Ethereal_Evil_Fight_11_Galakrond_Awesome_02_01.prefab:f5582d9e476adc543a16ef1d59fcfb1a");

	private static readonly AssetReference VO_DRGA_BOSS_07h_Male_Ethereal_Evil_Fight_11_Galakrond_Awesome_03_01 = new AssetReference("VO_DRGA_BOSS_07h_Male_Ethereal_Evil_Fight_11_Galakrond_Awesome_03_01.prefab:b63c1dfc1cdd7d941b698f3f728bed91");

	private static readonly AssetReference VO_DRGA_BOSS_07h_Male_Ethereal_Evil_Fight_11_Galakrond_Awesome_04_01 = new AssetReference("VO_DRGA_BOSS_07h_Male_Ethereal_Evil_Fight_11_Galakrond_Awesome_04_01.prefab:61ede9459db3a4741955c11c4b4f1377");

	private static readonly AssetReference VO_DRGA_BOSS_07h_Male_Ethereal_Evil_Fight_11_Galakrond_Awesome_05_01 = new AssetReference("VO_DRGA_BOSS_07h_Male_Ethereal_Evil_Fight_11_Galakrond_Awesome_05_01.prefab:0c7cc25ad2bfbb84da564b8939916340");

	private static readonly AssetReference VO_DRGA_BOSS_07h_Male_Ethereal_Evil_Fight_11_Galakrond_Awesome_06a_01 = new AssetReference("VO_DRGA_BOSS_07h_Male_Ethereal_Evil_Fight_11_Galakrond_Awesome_06a_01.prefab:f3fceb65864d6344bab7a9092c457812");

	private static readonly AssetReference VO_DRGA_BOSS_07h_Male_Ethereal_Evil_Fight_11_Galakrond_Awesome_06b_01 = new AssetReference("VO_DRGA_BOSS_07h_Male_Ethereal_Evil_Fight_11_Galakrond_Awesome_06b_01.prefab:d4d657e9db014314caab2f40636ff0b4");

	private static readonly AssetReference VO_DRGA_BOSS_07h_Male_Ethereal_Evil_Fight_11_Galakrond_Born_01 = new AssetReference("VO_DRGA_BOSS_07h_Male_Ethereal_Evil_Fight_11_Galakrond_Born_01.prefab:97d4176bf290fb64a9f7aba668c6d013");

	private static readonly AssetReference VO_DRGA_BOSS_07h_Male_Ethereal_Evil_Fight_11_Galakrond_LateGame_01a_01 = new AssetReference("VO_DRGA_BOSS_07h_Male_Ethereal_Evil_Fight_11_Galakrond_LateGame_01a_01.prefab:86b37070de8df5342b1ad83c47667576");

	private static readonly AssetReference VO_DRGA_BOSS_07h_Male_Ethereal_Evil_Fight_11_Galakrond_LateGame_01b_01 = new AssetReference("VO_DRGA_BOSS_07h_Male_Ethereal_Evil_Fight_11_Galakrond_LateGame_01b_01.prefab:23922063ad1675c41bfe9e6f3b6f6859");

	private static readonly AssetReference VO_DRGA_BOSS_07h_Male_Ethereal_Evil_Fight_11_Galakrond_LateGame_02_01 = new AssetReference("VO_DRGA_BOSS_07h_Male_Ethereal_Evil_Fight_11_Galakrond_LateGame_02_01.prefab:72eae8a8b874bd14d81b3627af18c847");

	private static readonly AssetReference VO_DRGA_BOSS_07h_Male_Ethereal_Evil_Fight_11_Galakrond_LateGame_03_01 = new AssetReference("VO_DRGA_BOSS_07h_Male_Ethereal_Evil_Fight_11_Galakrond_LateGame_03_01.prefab:22aa8be74f27779419b2335966330876");

	private static readonly AssetReference VO_DRGA_BOSS_07h_Male_Ethereal_Evil_Fight_11_Galakrond_LateGame_04_01 = new AssetReference("VO_DRGA_BOSS_07h_Male_Ethereal_Evil_Fight_11_Galakrond_LateGame_04_01.prefab:6c217ab12ba5dd942a3e92b7794660e2");

	private static readonly AssetReference VO_DRGA_BOSS_07h_Male_Ethereal_Evil_Fight_11_Galakrond_Victory_01 = new AssetReference("VO_DRGA_BOSS_07h_Male_Ethereal_Evil_Fight_11_Galakrond_Victory_01.prefab:11c0fff12681abf4cbb2163e95ea19f1");

	private static readonly AssetReference VO_DRGA_BOSS_07h_Male_Ethereal_Evil_Fight_11_PlayerStart_01 = new AssetReference("VO_DRGA_BOSS_07h_Male_Ethereal_Evil_Fight_11_PlayerStart_01.prefab:c46f24e0b34808142807111a15344873");

	private static readonly AssetReference VO_DRGA_BOSS_07h_Male_Ethereal_Evil_Fight_11_PlayerStart_Anduin_01 = new AssetReference("VO_DRGA_BOSS_07h_Male_Ethereal_Evil_Fight_11_PlayerStart_Anduin_01.prefab:9d2bb802ee0746f408d562ae7e13f46a");

	private static readonly AssetReference VO_DRGA_BOSS_07h_Male_Ethereal_Evil_Fight_11_PlayerStart_Sylvanas_a_01 = new AssetReference("VO_DRGA_BOSS_07h_Male_Ethereal_Evil_Fight_11_PlayerStart_Sylvanas_a_01.prefab:be943a6d259b41d4884b2dc351e051ad");

	private static readonly AssetReference VO_DRGA_BOSS_07h_Male_Ethereal_Evil_Fight_11_PlayerStart_Sylvanas_b_01 = new AssetReference("VO_DRGA_BOSS_07h_Male_Ethereal_Evil_Fight_11_PlayerStart_Sylvanas_b_01.prefab:d85c0941095876b409f50a479ee61c91");

	private static readonly AssetReference VO_DRGA_BOSS_37h_Male_Human_Evil_Fight_11_Boss_Death_Anduin_01 = new AssetReference("VO_DRGA_BOSS_37h_Male_Human_Evil_Fight_11_Boss_Death_Anduin_01.prefab:d12f0c859628d464ea2d58a8a4576dac");

	private static readonly AssetReference VO_DRGA_BOSS_37h_Male_Human_Evil_Fight_11_Boss_HeroPower_01_Anduin_01 = new AssetReference("VO_DRGA_BOSS_37h_Male_Human_Evil_Fight_11_Boss_HeroPower_01_Anduin_01.prefab:1f3e63cf82062164fa69b4dc1e5f29bd");

	private static readonly AssetReference VO_DRGA_BOSS_37h_Male_Human_Evil_Fight_11_Boss_HeroPower_02_Anduin_01 = new AssetReference("VO_DRGA_BOSS_37h_Male_Human_Evil_Fight_11_Boss_HeroPower_02_Anduin_01.prefab:e98a1c3e21706ab46ab16ea5a2952164");

	private static readonly AssetReference VO_DRGA_BOSS_37h_Male_Human_Evil_Fight_11_Boss_HeroPower_03_Anduin_01 = new AssetReference("VO_DRGA_BOSS_37h_Male_Human_Evil_Fight_11_Boss_HeroPower_03_Anduin_01.prefab:63ada6ddec7cb3f48b2d28fa19cd97e9");

	private static readonly AssetReference VO_DRGA_BOSS_37h_Male_Human_Evil_Fight_11_BossAttack_Anduin_01 = new AssetReference("VO_DRGA_BOSS_37h_Male_Human_Evil_Fight_11_BossAttack_Anduin_01.prefab:de8612b7bf980be4490b519a6e359eae");

	private static readonly AssetReference VO_DRGA_BOSS_37h_Male_Human_Evil_Fight_11_BossStart_Anduin_01 = new AssetReference("VO_DRGA_BOSS_37h_Male_Human_Evil_Fight_11_BossStart_Anduin_01.prefab:cf2f6ca5ebc64044daae9078d6ad5f25");

	private static readonly AssetReference VO_DRGA_BOSS_37h_Male_Human_Evil_Fight_11_EmoteResponse_Anduin_01 = new AssetReference("VO_DRGA_BOSS_37h_Male_Human_Evil_Fight_11_EmoteResponse_Anduin_01.prefab:8cfbed66c07551a429ad4fd6e5a37e5a");

	private static readonly AssetReference VO_DRGA_BOSS_37h_Male_Human_Evil_Fight_11_Idle_01_Anduin_01 = new AssetReference("VO_DRGA_BOSS_37h_Male_Human_Evil_Fight_11_Idle_01_Anduin_01.prefab:102e893c5422414409995141dc5176b9");

	private static readonly AssetReference VO_DRGA_BOSS_37h_Male_Human_Evil_Fight_11_Idle_02_Anduin_01 = new AssetReference("VO_DRGA_BOSS_37h_Male_Human_Evil_Fight_11_Idle_02_Anduin_01.prefab:e67929bb380e8c34ba3be8657580ffbd");

	private static readonly AssetReference VO_DRGA_BOSS_38h_Female_Forsaken_Evil_Fight_11_Boss_Death_Sylvanas_01 = new AssetReference("VO_DRGA_BOSS_38h_Female_Forsaken_Evil_Fight_11_Boss_Death_Sylvanas_01.prefab:920f5f25f7526b64ebc1874ce03a069c");

	private static readonly AssetReference VO_DRGA_BOSS_38h_Female_Forsaken_Evil_Fight_11_Boss_HeroPower_01_Sylvanas_01 = new AssetReference("VO_DRGA_BOSS_38h_Female_Forsaken_Evil_Fight_11_Boss_HeroPower_01_Sylvanas_01.prefab:66770ca958cfb614e9a0928ccbf67f7e");

	private static readonly AssetReference VO_DRGA_BOSS_38h_Female_Forsaken_Evil_Fight_11_Boss_HeroPower_02_Sylvanas_01 = new AssetReference("VO_DRGA_BOSS_38h_Female_Forsaken_Evil_Fight_11_Boss_HeroPower_02_Sylvanas_01.prefab:49bdc2d40ab6e2848b8c2cd454d9af84");

	private static readonly AssetReference VO_DRGA_BOSS_38h_Female_Forsaken_Evil_Fight_11_Boss_HeroPower_03_Sylvanas_01 = new AssetReference("VO_DRGA_BOSS_38h_Female_Forsaken_Evil_Fight_11_Boss_HeroPower_03_Sylvanas_01.prefab:eae26bd4bab21fe4084c86deb1dab642");

	private static readonly AssetReference VO_DRGA_BOSS_38h_Female_Forsaken_Evil_Fight_11_BossAttack_Sylvanas_01 = new AssetReference("VO_DRGA_BOSS_38h_Female_Forsaken_Evil_Fight_11_BossAttack_Sylvanas_01.prefab:64fcde735b952f8468eb1ba72cc7d8ba");

	private static readonly AssetReference VO_DRGA_BOSS_38h_Female_Forsaken_Evil_Fight_11_BossStart_Sylvanas_01 = new AssetReference("VO_DRGA_BOSS_38h_Female_Forsaken_Evil_Fight_11_BossStart_Sylvanas_01.prefab:a3f5e8bdcf9f74f4f88117fd307e93a6");

	private static readonly AssetReference VO_DRGA_BOSS_38h_Female_Forsaken_Evil_Fight_11_EmoteResponse_Sylvanas_01 = new AssetReference("VO_DRGA_BOSS_38h_Female_Forsaken_Evil_Fight_11_EmoteResponse_Sylvanas_01.prefab:778129426d2e1ef43b9767467af3e3f4");

	private static readonly AssetReference VO_DRGA_BOSS_38h_Female_Forsaken_Evil_Fight_11_Idle_01_Sylvanas_01 = new AssetReference("VO_DRGA_BOSS_38h_Female_Forsaken_Evil_Fight_11_Idle_01_Sylvanas_01.prefab:2302a2610659ed94f92e2d046280fc72");

	private static readonly AssetReference VO_DRGA_BOSS_38h_Female_Forsaken_Evil_Fight_11_Idle_02_Sylvanas_01 = new AssetReference("VO_DRGA_BOSS_38h_Female_Forsaken_Evil_Fight_11_Idle_02_Sylvanas_01.prefab:6cd88de1fd763ae4981a59e5c0dba454");

	private static readonly AssetReference VO_DRGA_BOSS_38h_Female_Forsaken_Evil_Fight_11_Idle_03_Sylvanas_01 = new AssetReference("VO_DRGA_BOSS_38h_Female_Forsaken_Evil_Fight_11_Idle_03_Sylvanas_01.prefab:19c19254823851c45a78e9c91aa79c71");

	private static readonly AssetReference VO_DRGA_BOSS_07h_Male_Ethereal_Good_Fight_12_Boss_Invoke_01_01 = new AssetReference("VO_DRGA_BOSS_07h_Male_Ethereal_Good_Fight_12_Boss_Invoke_01_01.prefab:eb9c22f23e619604b9de2e8cbf35486f");

	private static readonly AssetReference VO_DRGA_BOSS_07h_Male_Ethereal_Good_Fight_12_Boss_Invoke_02_01 = new AssetReference("VO_DRGA_BOSS_07h_Male_Ethereal_Good_Fight_12_Boss_Invoke_02_01.prefab:f13d2d8751b90254bab7a94dd70d67db");

	private static readonly AssetReference VO_DRGA_BOSS_07h_Male_Ethereal_Good_Fight_12_Boss_Invoke_03_01 = new AssetReference("VO_DRGA_BOSS_07h_Male_Ethereal_Good_Fight_12_Boss_Invoke_03_01.prefab:f9b97fd400ffca44db629c80efcb3895");

	private static readonly AssetReference VO_DRGA_BOSS_07h_Male_Ethereal_Good_Fight_12_Boss_Invoke_04_01 = new AssetReference("VO_DRGA_BOSS_07h_Male_Ethereal_Good_Fight_12_Boss_Invoke_04_01.prefab:32a4b7c99f3aae145b2a414c984f355b");

	private static readonly AssetReference VO_DRGA_BOSS_07h_Male_Ethereal_Good_Fight_12_Boss_Invoke_05_01 = new AssetReference("VO_DRGA_BOSS_07h_Male_Ethereal_Good_Fight_12_Boss_Invoke_05_01.prefab:3b0a5793ab112f54599e953587948acf");

	private static readonly AssetReference VO_DRGA_BOSS_07h_Male_Ethereal_Good_Fight_12_Boss_Invoke_06_01 = new AssetReference("VO_DRGA_BOSS_07h_Male_Ethereal_Good_Fight_12_Boss_Invoke_06_01.prefab:ca6c35bf0c1d5d140aafc25c11c78cc7");

	private static readonly AssetReference VO_DRGA_BOSS_07h_Male_Ethereal_Good_Fight_12_BossAttack_Gala_01 = new AssetReference("VO_DRGA_BOSS_07h_Male_Ethereal_Good_Fight_12_BossAttack_Gala_01.prefab:861be9550341a2b47b1383d91e916507");

	private static readonly AssetReference VO_DRGA_BOSS_07h_Male_Ethereal_Good_Fight_12_BossAttack_NoGala_01 = new AssetReference("VO_DRGA_BOSS_07h_Male_Ethereal_Good_Fight_12_BossAttack_NoGala_01.prefab:4500792ba86ca0143b8d0040571c2d35");

	private static readonly AssetReference VO_DRGA_BOSS_07h_Male_Ethereal_Good_Fight_12_Misc_01_Galakrond_01 = new AssetReference("VO_DRGA_BOSS_07h_Male_Ethereal_Good_Fight_12_Misc_01_Galakrond_01.prefab:fef0bd9eb9481db43bc33e8639f61bc6");

	private static readonly AssetReference VO_DRGA_BOSS_07h_Male_Ethereal_Good_Fight_12_Misc_02_Galakrond_01 = new AssetReference("VO_DRGA_BOSS_07h_Male_Ethereal_Good_Fight_12_Misc_02_Galakrond_01.prefab:ce67c7d371bd22d4b9e346aa1583df7f");

	private static readonly AssetReference VO_DRGA_BOSS_07h_Male_Ethereal_Good_Fight_12_Misc_03_Galakrond_01 = new AssetReference("VO_DRGA_BOSS_07h_Male_Ethereal_Good_Fight_12_Misc_03_Galakrond_01.prefab:7ac14feda2477984aa143219f855979f");

	private static readonly AssetReference VO_DRGA_BOSS_07h_Male_Ethereal_Good_Fight_12_Misc_04_Galakrond_01 = new AssetReference("VO_DRGA_BOSS_07h_Male_Ethereal_Good_Fight_12_Misc_04_Galakrond_01.prefab:194b03d8ed0c58f4b9c685c397dc6965");

	private static readonly AssetReference VO_DRGA_BOSS_07h_Male_Ethereal_Good_Fight_12_Misc_05_Galakrond_01 = new AssetReference("VO_DRGA_BOSS_07h_Male_Ethereal_Good_Fight_12_Misc_05_Galakrond_01.prefab:b3b2591977dbf4f47a76584b055f4869");

	private static readonly AssetReference VO_DRGA_BOSS_07h_Male_Ethereal_Good_Fight_12_RenoDragon_Misc_05_01 = new AssetReference("VO_DRGA_BOSS_07h_Male_Ethereal_Good_Fight_12_RenoDragon_Misc_05_01.prefab:9d551762e7ecc1845842217663425566");

	private static readonly AssetReference VO_DRGA_BOSS_24h_Male_Dragon_Evil_Fight_12_Idle_02_01 = new AssetReference("VO_DRGA_BOSS_24h_Male_Dragon_Evil_Fight_12_Idle_02_01.prefab:c1442324e17668d4abfbd46d6eed287a");

	private static readonly AssetReference VO_DRGA_BOSS_07h_Male_Ethereal_Good_Fight_12_Idle_01_01 = new AssetReference("VO_DRGA_BOSS_07h_Male_Ethereal_Good_Fight_12_Idle_01_01.prefab:42615239b9fb58b4c8b518c764b63137");

	private static readonly AssetReference VO_DRGA_BOSS_07h_Male_Ethereal_Good_Fight_12_Idle_02_01 = new AssetReference("VO_DRGA_BOSS_07h_Male_Ethereal_Good_Fight_12_Idle_02_01.prefab:d13f509fccbb2474bbf7bc5a276d7a4d");

	private static readonly AssetReference VO_DRGA_BOSS_07h_Male_Ethereal_Good_Fight_12_Idle_03_01 = new AssetReference("VO_DRGA_BOSS_07h_Male_Ethereal_Good_Fight_12_Idle_03_01.prefab:a998f80b9b268df4fb86df39a37f992e");

	private static readonly AssetReference VO_DRGA_BOSS_07h_Male_Ethereal_Good_Fight_12_Idle_04_01 = new AssetReference("VO_DRGA_BOSS_07h_Male_Ethereal_Good_Fight_12_Idle_04_01.prefab:a998749fb50c6264a9f178edb21d5b7f");

	private static readonly AssetReference VO_DRGA_BOSS_24h_Male_Dragon_Evil_Fight_12_Laugh_01 = new AssetReference("VO_DRGA_BOSS_24h_Male_Dragon_Evil_Fight_12_Laugh_01.prefab:09c06754b47077445a8b9dd2c0d9b83a");

	private static readonly AssetReference VO_DRGA_BOSS_24h_Male_Dragon_Evil_Fight_12_Boss_HeroPower_01_01 = new AssetReference("VO_DRGA_BOSS_24h_Male_Dragon_Evil_Fight_12_Boss_HeroPower_01_01.prefab:a60ff4ac653d29d4aaa283131c7424e3");

	private static readonly AssetReference VO_DRGA_BOSS_24h_Male_Dragon_Evil_Fight_12_Boss_HeroPower_02_01 = new AssetReference("VO_DRGA_BOSS_24h_Male_Dragon_Evil_Fight_12_Boss_HeroPower_02_01.prefab:76009e7b08ac2414a885b15b03901408");

	private static readonly AssetReference VO_DRGA_BOSS_24h_Male_Dragon_Evil_Fight_12_Boss_HeroPower_03_01 = new AssetReference("VO_DRGA_BOSS_24h_Male_Dragon_Evil_Fight_12_Boss_HeroPower_03_01.prefab:c107dd155cf1fb2409f9bc203d836dd7");

	private static readonly AssetReference VO_DRGA_BOSS_01h_Male_Human_Good_Fight_11_Misc_04_01 = new AssetReference("VO_DRGA_BOSS_01h_Male_Human_Good_Fight_11_Misc_04_01.prefab:fbfe70bab9271e746adc673ebe4e8ab4");

	private static readonly AssetReference VO_DRGA_BOSS_01h_Male_Human_Evil_Fight_03_BossStartHeroic_01 = new AssetReference("VO_DRGA_BOSS_01h_Male_Human_Evil_Fight_03_BossStartHeroic_01.prefab:659d57c7389ab52448ac1a90cb4b47a6");

	private static readonly AssetReference VO_DRG_610_Male_Dragon_Play_01 = new AssetReference("VO_DRG_610_Male_Dragon_Play_01.prefab:c81c587900090674aa26f25a03fb3479");

	private static readonly AssetReference VO_DRG_600_Male_Dragon_Start_01 = new AssetReference("VO_DRG_600_Male_Dragon_Start_01.prefab:d3bc566b53d56454097f27ca04ce28f4");

	private List<string> m_AnduinBossHeroPower = new List<string> { VO_DRGA_BOSS_37h_Male_Human_Evil_Fight_11_Boss_HeroPower_01_Anduin_01, VO_DRGA_BOSS_37h_Male_Human_Evil_Fight_11_Boss_HeroPower_02_Anduin_01, VO_DRGA_BOSS_37h_Male_Human_Evil_Fight_11_Boss_HeroPower_03_Anduin_01 };

	private List<string> m_Galakrond_Awesome = new List<string> { VO_DRGA_BOSS_07h_Male_Ethereal_Evil_Fight_11_Galakrond_Awesome_01_01, VO_DRGA_BOSS_07h_Male_Ethereal_Evil_Fight_11_Galakrond_Awesome_02_01, VO_DRGA_BOSS_07h_Male_Ethereal_Evil_Fight_11_Galakrond_Awesome_03_01, VO_DRGA_BOSS_07h_Male_Ethereal_Evil_Fight_11_Galakrond_Awesome_04_01, VO_DRGA_BOSS_07h_Male_Ethereal_Evil_Fight_11_Galakrond_Awesome_05_01 };

	private List<string> m_Galakrond_Devastation = new List<string> { VO_DRGA_BOSS_24h_Male_Dragon_Evil_Fight_12_Boss_HeroPower_01_01, VO_DRGA_BOSS_24h_Male_Dragon_Evil_Fight_12_Boss_HeroPower_02_01, VO_DRGA_BOSS_24h_Male_Dragon_Evil_Fight_12_Boss_HeroPower_03_01 };

	private List<string> m_Galakrond_Lategame = new List<string> { VO_DRGA_BOSS_07h_Male_Ethereal_Evil_Fight_11_Galakrond_LateGame_02_01, VO_DRGA_BOSS_07h_Male_Ethereal_Evil_Fight_11_Galakrond_LateGame_03_01, VO_DRGA_BOSS_07h_Male_Ethereal_Evil_Fight_11_Galakrond_LateGame_04_01 };

	private List<string> m_missionEventTriggerRenoHeroPower = new List<string> { VO_DRGA_BOSS_01h_Male_Human_Evil_Fight_11_Boss_HeroPower_01_Reno_01, VO_DRGA_BOSS_01h_Male_Human_Evil_Fight_11_Boss_HeroPower_02_Reno_01, VO_DRGA_BOSS_01h_Male_Human_Evil_Fight_11_Boss_HeroPower_03_Reno_01, VO_DRGA_BOSS_01h_Male_Human_Evil_Fight_11_Boss_HeroPower_04_Reno_01, VO_DRGA_BOSS_01h_Male_Human_Evil_Fight_11_Boss_HeroPower_05_Reno_01 };

	private List<string> m_missionEventTriggerAnduinHeroPower = new List<string> { VO_DRGA_BOSS_37h_Male_Human_Evil_Fight_11_Boss_HeroPower_01_Anduin_01, VO_DRGA_BOSS_37h_Male_Human_Evil_Fight_11_Boss_HeroPower_02_Anduin_01, VO_DRGA_BOSS_37h_Male_Human_Evil_Fight_11_Boss_HeroPower_03_Anduin_01 };

	private List<string> m_missionEventTriggerSylvanasHeroPower = new List<string> { VO_DRGA_BOSS_38h_Female_Forsaken_Evil_Fight_11_Boss_HeroPower_01_Sylvanas_01, VO_DRGA_BOSS_38h_Female_Forsaken_Evil_Fight_11_Boss_HeroPower_02_Sylvanas_01, VO_DRGA_BOSS_38h_Female_Forsaken_Evil_Fight_11_Boss_HeroPower_03_Sylvanas_01 };

	private List<string> m_InvokeGalakrond = new List<string> { VO_DRGA_BOSS_07h_Male_Ethereal_Good_Fight_12_Boss_Invoke_01_01, VO_DRGA_BOSS_07h_Male_Ethereal_Good_Fight_12_Boss_Invoke_02_01, VO_DRGA_BOSS_07h_Male_Ethereal_Good_Fight_12_Boss_Invoke_03_01, VO_DRGA_BOSS_07h_Male_Ethereal_Good_Fight_12_Boss_Invoke_04_01, VO_DRGA_BOSS_07h_Male_Ethereal_Good_Fight_12_Boss_Invoke_05_01, VO_DRGA_BOSS_07h_Male_Ethereal_Good_Fight_12_Boss_Invoke_06_01 };

	private static List<string> m_AnduinIdleLines = new List<string> { VO_DRGA_BOSS_37h_Male_Human_Evil_Fight_11_Idle_01_Anduin_01, VO_DRGA_BOSS_37h_Male_Human_Evil_Fight_11_Idle_02_Anduin_01 };

	private List<string> m_AnduinIdleLinesCopy = new List<string>(m_AnduinIdleLines);

	private static List<string> m_SylvanasIdleLines = new List<string> { VO_DRGA_BOSS_38h_Female_Forsaken_Evil_Fight_11_Idle_01_Sylvanas_01, VO_DRGA_BOSS_38h_Female_Forsaken_Evil_Fight_11_Idle_02_Sylvanas_01, VO_DRGA_BOSS_38h_Female_Forsaken_Evil_Fight_11_Idle_03_Sylvanas_01 };

	private List<string> m_SylvanasIdleLinesCopy = new List<string>(m_SylvanasIdleLines);

	private static List<string> m_RenoIdleLines = new List<string> { VO_DRGA_BOSS_01h_Male_Human_Evil_Fight_11_Idle_01_Reno_01, VO_DRGA_BOSS_01h_Male_Human_Evil_Fight_11_Idle_02_Reno_01, VO_DRGA_BOSS_01h_Male_Human_Evil_Fight_11_Idle_03_Reno_01 };

	private List<string> m_RenoIdleLinesCopy = new List<string>(m_RenoIdleLines);

	private List<string> m_VO_BossHasGalakrond = new List<string> { VO_DRGA_BOSS_07h_Male_Ethereal_Good_Fight_12_Misc_01_Galakrond_01, VO_DRGA_BOSS_07h_Male_Ethereal_Good_Fight_12_Misc_02_Galakrond_01, VO_DRGA_BOSS_07h_Male_Ethereal_Good_Fight_12_Misc_03_Galakrond_01, VO_DRGA_BOSS_07h_Male_Ethereal_Good_Fight_12_Misc_04_Galakrond_01, VO_DRGA_BOSS_07h_Male_Ethereal_Good_Fight_12_Misc_05_Galakrond_01 };

	private List<string> m_VO_DRGA_BOSS_07h_Male_Ethereal_Good_Fight_12_ExpositionLines = new List<string> { VO_DRGA_BOSS_07h_Male_Ethereal_Good_Fight_12_Idle_01_01, VO_DRGA_BOSS_07h_Male_Ethereal_Good_Fight_12_Idle_02_01, VO_DRGA_BOSS_07h_Male_Ethereal_Good_Fight_12_Idle_03_01, VO_DRGA_BOSS_07h_Male_Ethereal_Good_Fight_12_Idle_04_01 };

	private List<string> m_RenoHeroPower = new List<string> { VO_DRGA_BOSS_01h_Male_Human_Evil_Fight_11_Boss_HeroPower_01_Reno_01, VO_DRGA_BOSS_01h_Male_Human_Evil_Fight_11_Boss_HeroPower_04_Reno_01, VO_DRGA_BOSS_01h_Male_Human_Evil_Fight_11_Boss_HeroPower_05_Reno_01 };

	public int m_TurnOfGalakrondLine;

	public bool m_GalakrondFirstAttack = true;

	public bool m_GalakrondAnduinFirstAttack = true;

	public bool m_GalakrondLategameFirstAttack = true;

	private HashSet<string> m_playedLines = new HashSet<string>();

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> soundFiles = new List<string>
		{
			VO_DRGA_BOSS_01h_Male_Human_Evil_Fight_11_Boss_Death_Reno_01, VO_DRGA_BOSS_01h_Male_Human_Evil_Fight_11_Boss_Death_Reno_Alt_01, VO_DRGA_BOSS_01h_Male_Human_Evil_Fight_11_Boss_HeroPower_01_Reno_01, VO_DRGA_BOSS_01h_Male_Human_Evil_Fight_11_Boss_HeroPower_02_Reno_01, VO_DRGA_BOSS_01h_Male_Human_Evil_Fight_11_Boss_HeroPower_03_Reno_01, VO_DRGA_BOSS_01h_Male_Human_Evil_Fight_11_Boss_HeroPower_04_Reno_01, VO_DRGA_BOSS_01h_Male_Human_Evil_Fight_11_Boss_HeroPower_05_Reno_01, VO_DRGA_BOSS_01h_Male_Human_Evil_Fight_11_BossAttack_Reno_01, VO_DRGA_BOSS_01h_Male_Human_Evil_Fight_11_BossStart_Reno_01, VO_DRGA_BOSS_01h_Male_Human_Evil_Fight_11_EmoteResponse_Reno_01,
			VO_DRGA_BOSS_01h_Male_Human_Evil_Fight_11_Idle_01_Reno_01, VO_DRGA_BOSS_01h_Male_Human_Evil_Fight_11_Idle_02_Reno_01, VO_DRGA_BOSS_01h_Male_Human_Evil_Fight_11_Idle_03_Reno_01, VO_DRGA_BOSS_07h_Male_Ethereal_Evil_Fight_11_Galakrond_Awesome_01_01, VO_DRGA_BOSS_07h_Male_Ethereal_Evil_Fight_11_Galakrond_Awesome_02_01, VO_DRGA_BOSS_07h_Male_Ethereal_Evil_Fight_11_Galakrond_Awesome_03_01, VO_DRGA_BOSS_07h_Male_Ethereal_Evil_Fight_11_Galakrond_Awesome_04_01, VO_DRGA_BOSS_07h_Male_Ethereal_Evil_Fight_11_Galakrond_Awesome_05_01, VO_DRGA_BOSS_07h_Male_Ethereal_Evil_Fight_11_Galakrond_Awesome_06a_01, VO_DRGA_BOSS_07h_Male_Ethereal_Evil_Fight_11_Galakrond_Awesome_06b_01,
			VO_DRGA_BOSS_07h_Male_Ethereal_Evil_Fight_11_Galakrond_Born_01, VO_DRGA_BOSS_07h_Male_Ethereal_Evil_Fight_11_Galakrond_LateGame_01a_01, VO_DRGA_BOSS_07h_Male_Ethereal_Evil_Fight_11_Galakrond_LateGame_01b_01, VO_DRGA_BOSS_07h_Male_Ethereal_Evil_Fight_11_Galakrond_LateGame_02_01, VO_DRGA_BOSS_07h_Male_Ethereal_Evil_Fight_11_Galakrond_LateGame_03_01, VO_DRGA_BOSS_07h_Male_Ethereal_Evil_Fight_11_Galakrond_LateGame_04_01, VO_DRGA_BOSS_07h_Male_Ethereal_Evil_Fight_11_Galakrond_Victory_01, VO_DRGA_BOSS_07h_Male_Ethereal_Evil_Fight_11_PlayerStart_01, VO_DRGA_BOSS_07h_Male_Ethereal_Evil_Fight_11_PlayerStart_Anduin_01, VO_DRGA_BOSS_07h_Male_Ethereal_Evil_Fight_11_PlayerStart_Sylvanas_a_01,
			VO_DRGA_BOSS_07h_Male_Ethereal_Evil_Fight_11_PlayerStart_Sylvanas_b_01, VO_DRGA_BOSS_37h_Male_Human_Evil_Fight_11_Boss_Death_Anduin_01, VO_DRGA_BOSS_37h_Male_Human_Evil_Fight_11_Boss_HeroPower_01_Anduin_01, VO_DRGA_BOSS_37h_Male_Human_Evil_Fight_11_Boss_HeroPower_02_Anduin_01, VO_DRGA_BOSS_37h_Male_Human_Evil_Fight_11_Boss_HeroPower_03_Anduin_01, VO_DRGA_BOSS_37h_Male_Human_Evil_Fight_11_BossAttack_Anduin_01, VO_DRGA_BOSS_37h_Male_Human_Evil_Fight_11_BossStart_Anduin_01, VO_DRGA_BOSS_37h_Male_Human_Evil_Fight_11_EmoteResponse_Anduin_01, VO_DRGA_BOSS_37h_Male_Human_Evil_Fight_11_Idle_01_Anduin_01, VO_DRGA_BOSS_37h_Male_Human_Evil_Fight_11_Idle_02_Anduin_01,
			VO_DRGA_BOSS_38h_Female_Forsaken_Evil_Fight_11_Boss_Death_Sylvanas_01, VO_DRGA_BOSS_38h_Female_Forsaken_Evil_Fight_11_Boss_HeroPower_01_Sylvanas_01, VO_DRGA_BOSS_38h_Female_Forsaken_Evil_Fight_11_Boss_HeroPower_02_Sylvanas_01, VO_DRGA_BOSS_38h_Female_Forsaken_Evil_Fight_11_Boss_HeroPower_03_Sylvanas_01, VO_DRGA_BOSS_38h_Female_Forsaken_Evil_Fight_11_BossAttack_Sylvanas_01, VO_DRGA_BOSS_38h_Female_Forsaken_Evil_Fight_11_BossStart_Sylvanas_01, VO_DRGA_BOSS_38h_Female_Forsaken_Evil_Fight_11_EmoteResponse_Sylvanas_01, VO_DRGA_BOSS_38h_Female_Forsaken_Evil_Fight_11_Idle_01_Sylvanas_01, VO_DRGA_BOSS_38h_Female_Forsaken_Evil_Fight_11_Idle_02_Sylvanas_01, VO_DRGA_BOSS_38h_Female_Forsaken_Evil_Fight_11_Idle_03_Sylvanas_01,
			VO_DRGA_BOSS_07h_Male_Ethereal_Good_Fight_12_Boss_Invoke_01_01, VO_DRGA_BOSS_07h_Male_Ethereal_Good_Fight_12_Boss_Invoke_02_01, VO_DRGA_BOSS_07h_Male_Ethereal_Good_Fight_12_Boss_Invoke_03_01, VO_DRGA_BOSS_07h_Male_Ethereal_Good_Fight_12_Boss_Invoke_04_01, VO_DRGA_BOSS_07h_Male_Ethereal_Good_Fight_12_Boss_Invoke_05_01, VO_DRGA_BOSS_07h_Male_Ethereal_Good_Fight_12_Boss_Invoke_06_01, VO_DRGA_BOSS_07h_Male_Ethereal_Good_Fight_12_BossAttack_Gala_01, VO_DRGA_BOSS_07h_Male_Ethereal_Good_Fight_12_BossAttack_NoGala_01, VO_DRGA_BOSS_07h_Male_Ethereal_Good_Fight_12_Misc_01_Galakrond_01, VO_DRGA_BOSS_07h_Male_Ethereal_Good_Fight_12_Misc_02_Galakrond_01,
			VO_DRGA_BOSS_07h_Male_Ethereal_Good_Fight_12_Misc_03_Galakrond_01, VO_DRGA_BOSS_07h_Male_Ethereal_Good_Fight_12_Misc_04_Galakrond_01, VO_DRGA_BOSS_07h_Male_Ethereal_Good_Fight_12_Misc_05_Galakrond_01, VO_DRGA_BOSS_07h_Male_Ethereal_Good_Fight_12_RenoDragon_Misc_05_01, VO_DRGA_BOSS_24h_Male_Dragon_Evil_Fight_12_Idle_02_01, VO_DRGA_BOSS_07h_Male_Ethereal_Good_Fight_12_Idle_01_01, VO_DRGA_BOSS_07h_Male_Ethereal_Good_Fight_12_Idle_02_01, VO_DRGA_BOSS_07h_Male_Ethereal_Good_Fight_12_Idle_03_01, VO_DRGA_BOSS_07h_Male_Ethereal_Good_Fight_12_Idle_04_01, VO_DRGA_BOSS_24h_Male_Dragon_Evil_Fight_12_Laugh_01,
			VO_DRGA_BOSS_24h_Male_Dragon_Evil_Fight_12_Boss_HeroPower_01_01, VO_DRGA_BOSS_24h_Male_Dragon_Evil_Fight_12_Boss_HeroPower_02_01, VO_DRGA_BOSS_24h_Male_Dragon_Evil_Fight_12_Boss_HeroPower_03_01, VO_DRGA_BOSS_01h_Male_Human_Good_Fight_11_Misc_04_01, VO_DRGA_BOSS_01h_Male_Human_Evil_Fight_03_BossStartHeroic_01, VO_DRG_610_Male_Dragon_Play_01, VO_DRG_600_Male_Dragon_Start_01
		};
		SetBossVOLines(soundFiles);
		foreach (string soundFile in soundFiles)
		{
			PreloadSound(soundFile);
		}
	}

	protected override bool GetShouldSuppressDeathTextBubble()
	{
		return true;
	}

	public override void OnCreateGame()
	{
		base.OnCreateGame();
		m_deathLine = VO_DRGA_BOSS_38h_Female_Forsaken_Evil_Fight_11_Boss_Death_Sylvanas_01;
	}

	protected override void PlayEmoteResponse(EmoteType emoteType, CardSoundSpell emoteSpell)
	{
		Actor enemyActor = GameState.Get().GetOpposingSidePlayer().GetHeroCard()
			.GetActor();
		if (emoteType == EmoteType.START)
		{
			if (!m_Heroic)
			{
				Gameplay.Get().StartCoroutine(PlaySoundAndBlockSpeech(VO_DRGA_BOSS_01h_Male_Human_Evil_Fight_11_BossStart_Reno_01, Notification.SpeechBubbleDirection.TopRight, enemyActor));
				return;
			}
			Gameplay.Get().StartCoroutine(PlaySoundAndBlockSpeech(VO_DRGA_BOSS_01h_Male_Human_Evil_Fight_03_BossStartHeroic_01, Notification.SpeechBubbleDirection.TopRight, enemyActor));
		}
		if (MissionEntity.STANDARD_EMOTE_RESPONSE_TRIGGERS.Contains(emoteType))
		{
			switch (GameState.Get().GetOpposingSidePlayer().GetHero()
				.GetCardId())
			{
			case "DRGA_BOSS_01h4":
				Gameplay.Get().StartCoroutine(PlaySoundAndBlockSpeech(VO_DRGA_BOSS_01h_Male_Human_Evil_Fight_11_EmoteResponse_Reno_01, Notification.SpeechBubbleDirection.TopRight, enemyActor));
				break;
			case "DRGA_BOSS_37h":
				Gameplay.Get().StartCoroutine(PlaySoundAndBlockSpeech(VO_DRGA_BOSS_37h_Male_Human_Evil_Fight_11_EmoteResponse_Anduin_01, Notification.SpeechBubbleDirection.TopRight, enemyActor));
				break;
			case "DRGA_BOSS_38h":
				Gameplay.Get().StartCoroutine(PlaySoundAndBlockSpeech(VO_DRGA_BOSS_38h_Female_Forsaken_Evil_Fight_11_EmoteResponse_Sylvanas_01, Notification.SpeechBubbleDirection.TopRight, enemyActor));
				break;
			default:
				Gameplay.Get().StartCoroutine(PlaySoundAndBlockSpeech(VO_DRGA_BOSS_01h_Male_Human_Evil_Fight_11_EmoteResponse_Reno_01, Notification.SpeechBubbleDirection.TopRight, enemyActor));
				break;
			}
		}
	}

	protected override IEnumerator HandleMissionEventWithTiming(int missionEvent)
	{
		if (missionEvent == 911)
		{
			GameState.Get().SetBusy(busy: true);
			while (m_enemySpeaking)
			{
				yield return null;
			}
			GameState.Get().SetBusy(busy: false);
			yield break;
		}
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
		string enemyHeroCardID = GameState.Get().GetOpposingSidePlayer().GetHero()
			.GetCardId();
		switch (missionEvent)
		{
		case 100:
			yield return PlayAndRemoveRandomLineOnlyOnce(enemyActor, m_AnduinBossHeroPower);
			break;
		case 101:
			yield return PlayLineOnlyOnce(enemyActor, VO_DRGA_BOSS_37h_Male_Human_Evil_Fight_11_BossAttack_Anduin_01);
			break;
		case 102:
			yield return PlayLineOnlyOnce(enemyActor, VO_DRGA_BOSS_37h_Male_Human_Evil_Fight_11_BossStart_Anduin_01);
			break;
		case 103:
			yield return PlayLineInOrderOnce(friendlyActor, m_Galakrond_Awesome);
			break;
		case 104:
			yield return PlayLineAlways(friendlyActor, VO_DRGA_BOSS_07h_Male_Ethereal_Evil_Fight_11_Galakrond_Born_01);
			break;
		case 106:
			GameState.Get().SetBusy(busy: true);
			yield return PlayLineOnlyOnce(friendlyActor, VO_DRGA_BOSS_07h_Male_Ethereal_Evil_Fight_11_Galakrond_Victory_01);
			GameState.Get().SetBusy(busy: false);
			break;
		case 107:
			GameState.Get().SetBusy(busy: true);
			yield return PlayLineAlways(enemyActor, VO_DRGA_BOSS_37h_Male_Human_Evil_Fight_11_Boss_Death_Anduin_01);
			GameState.Get().SetBusy(busy: false);
			break;
		case 109:
			GameState.Get().SetBusy(busy: true);
			yield return PlayLineAlways(enemyActor, VO_DRGA_BOSS_01h_Male_Human_Evil_Fight_11_Boss_Death_Reno_01);
			GameState.Get().SetBusy(busy: false);
			break;
		case 110:
			GameState.Get().SetBusy(busy: true);
			yield return PlayLineOnlyOnce(enemyActor, VO_DRGA_BOSS_01h_Male_Human_Evil_Fight_11_BossAttack_Reno_01);
			GameState.Get().SetBusy(busy: false);
			break;
		case 112:
			yield return PlayLineAlways(enemyActor, VO_DRGA_BOSS_38h_Female_Forsaken_Evil_Fight_11_Boss_Death_Sylvanas_01);
			break;
		case 113:
			yield return PlayLineOnlyOnce(enemyActor, VO_DRGA_BOSS_38h_Female_Forsaken_Evil_Fight_11_BossAttack_Sylvanas_01);
			break;
		case 114:
			yield return PlayAndRemoveRandomLineOnlyOnce(enemyActor, m_missionEventTriggerRenoHeroPower);
			break;
		case 115:
			yield return PlayAndRemoveRandomLineOnlyOnce(enemyActor, m_missionEventTriggerAnduinHeroPower);
			break;
		case 116:
			yield return PlayAndRemoveRandomLineOnlyOnce(enemyActor, m_missionEventTriggerSylvanasHeroPower);
			break;
		case 126:
			yield return PlayLineAlways(enemyActor, VO_DRGA_BOSS_07h_Male_Ethereal_Good_Fight_12_RenoDragon_Misc_05_01);
			break;
		case 132:
			if (m_Galakrond)
			{
				yield return PlayLineOnlyOnce(friendlyActor, VO_DRGA_BOSS_07h_Male_Ethereal_Good_Fight_12_BossAttack_Gala_01);
			}
			if (!m_Galakrond)
			{
				yield return PlayLineOnlyOnce(friendlyActor, VO_DRGA_BOSS_07h_Male_Ethereal_Good_Fight_12_BossAttack_NoGala_01);
			}
			break;
		case 133:
			if (m_Galakrond)
			{
				yield return PlayAndRemoveRandomLineOnlyOnce(enemyActor, m_VO_BossHasGalakrond);
			}
			break;
		case 134:
			yield return PlayLineInOrderOnce(friendlyActor, m_InvokeGalakrond);
			break;
		case 136:
			yield return PlayLineInOrderOnce(friendlyActor, m_VO_DRGA_BOSS_07h_Male_Ethereal_Good_Fight_12_ExpositionLines);
			break;
		case 137:
			yield return PlayAndRemoveRandomLineOnlyOnceWithBrassRing(GetFriendlyActorByCardId("DRGA_005"), null, m_Galakrond_Devastation);
			break;
		case 138:
			GameState.Get().SetBusy(busy: true);
			yield return PlayLineAlways(enemyActor, VO_DRGA_BOSS_37h_Male_Human_Evil_Fight_11_BossStart_Anduin_01);
			GameState.Get().SetBusy(busy: false);
			break;
		case 143:
			GameState.Get().SetBusy(busy: true);
			yield return PlayLineAlways(friendlyActor, VO_DRGA_BOSS_07h_Male_Ethereal_Evil_Fight_11_PlayerStart_Anduin_01);
			GameState.Get().SetBusy(busy: false);
			break;
		case 139:
			GameState.Get().SetBusy(busy: true);
			yield return PlayLineAlways(enemyActor, VO_DRGA_BOSS_38h_Female_Forsaken_Evil_Fight_11_BossStart_Sylvanas_01);
			GameState.Get().SetBusy(busy: false);
			yield return PlayLineAlways(friendlyActor, VO_DRGA_BOSS_07h_Male_Ethereal_Evil_Fight_11_PlayerStart_Sylvanas_a_01);
			yield return PlayLineAlways(friendlyActor, VO_DRGA_BOSS_07h_Male_Ethereal_Evil_Fight_11_PlayerStart_Sylvanas_b_01);
			break;
		case 140:
			GameState.Get().SetBusy(busy: true);
			yield return PlayLineAlways(friendlyActor, VO_DRGA_BOSS_07h_Male_Ethereal_Evil_Fight_11_Galakrond_Born_01);
			GameState.Get().SetBusy(busy: false);
			break;
		case 141:
			m_Galakrond = true;
			yield return PlayLineAlwaysWithBrassRing(GetFriendlyActorByCardId("DRGA_005"), null, VO_DRGA_BOSS_24h_Male_Dragon_Evil_Fight_12_Idle_02_01);
			if (enemyHeroCardID == "DRGA_BOSS_01h4")
			{
				yield return PlayLineAlways(enemyActor, VO_DRGA_BOSS_01h_Male_Human_Good_Fight_11_Misc_04_01);
			}
			break;
		case 142:
			GameState.Get().SetBusy(busy: true);
			m_Galakrond = true;
			yield return PlayLineAlwaysWithBrassRing(GetFriendlyActorByCardId("DRGA_005"), null, VO_DRGA_BOSS_24h_Male_Dragon_Evil_Fight_12_Idle_02_01);
			GameState.Get().SetBusy(busy: false);
			break;
		case 144:
			GameState.Get().SetBusy(busy: true);
			yield return PlayLineAlways(GetFriendlyActorByCardId("DRGA_005"), null, VO_DRG_610_Male_Dragon_Play_01);
			yield return PlayLineAlways(enemyActor, VO_DRGA_BOSS_01h_Male_Human_Evil_Fight_11_Boss_Death_Reno_01);
			GameState.Get().SetBusy(busy: false);
			break;
		case 145:
			GameState.Get().SetBusy(busy: true);
			yield return PlayLineAlways(GetFriendlyActorByCardId("DRGA_005"), null, VO_DRG_600_Male_Dragon_Start_01);
			yield return PlayLineAlways(enemyActor, VO_DRGA_BOSS_37h_Male_Human_Evil_Fight_11_Boss_Death_Anduin_01);
			GameState.Get().SetBusy(busy: false);
			break;
		case 199:
			yield return PlayAndRemoveRandomLineOnlyOnce(enemyActor, m_RenoHeroPower);
			break;
		case 250:
		{
			int currentTurn = GetTag(GAME_TAG.TURN) - GameState.Get().GetGameEntity().GetTag(GAME_TAG.EXTRA_TURNS_TAKEN_THIS_GAME);
			if (m_TurnOfGalakrondLine >= currentTurn)
			{
				break;
			}
			m_TurnOfGalakrondLine = currentTurn;
			if (m_GalakrondFirstAttack)
			{
				GameState.Get().SetBusy(busy: true);
				yield return PlayLineAlways(friendlyActor, VO_DRGA_BOSS_07h_Male_Ethereal_Good_Fight_12_Misc_02_Galakrond_01);
				m_GalakrondFirstAttack = false;
				GameState.Get().SetBusy(busy: false);
				break;
			}
			if (m_GalakrondAnduinFirstAttack && enemyHeroCardID == "DRGA_BOSS_37h")
			{
				yield return PlayLineAlways(friendlyActor, VO_DRGA_BOSS_07h_Male_Ethereal_Evil_Fight_11_Galakrond_Awesome_06a_01);
				yield return PlayLineAlwaysWithBrassRing(GetFriendlyActorByCardId("DRGA_005"), null, VO_DRGA_BOSS_24h_Male_Dragon_Evil_Fight_12_Laugh_01);
				yield return PlayLineAlways(friendlyActor, VO_DRGA_BOSS_07h_Male_Ethereal_Evil_Fight_11_Galakrond_Awesome_06b_01);
				m_GalakrondAnduinFirstAttack = false;
				break;
			}
			switch (enemyHeroCardID)
			{
			case "DRGA_BOSS_37h":
			case "DRGA_BOSS_01h4":
				yield return PlayAndRemoveRandomLineOnlyOnce(friendlyActor, m_Galakrond_Awesome);
				break;
			case "DRGA_BOSS_38h":
				if (m_GalakrondLategameFirstAttack)
				{
					yield return PlayLineAlways(friendlyActor, VO_DRGA_BOSS_07h_Male_Ethereal_Evil_Fight_11_Galakrond_LateGame_01a_01);
					yield return PlayLineAlways(friendlyActor, VO_DRGA_BOSS_07h_Male_Ethereal_Evil_Fight_11_Galakrond_LateGame_01b_01);
					m_GalakrondLategameFirstAttack = false;
				}
				else
				{
					yield return PlayLineInOrderOnce(friendlyActor, m_Galakrond_Lategame);
				}
				break;
			}
			break;
		}
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
		}
	}

	public override void OnPlayThinkEmote()
	{
		if (m_enemySpeaking)
		{
			return;
		}
		Player currentPlayer = GameState.Get().GetCurrentPlayer();
		if (!currentPlayer.IsFriendlySide() || currentPlayer.GetHeroCard().HasActiveEmoteSound())
		{
			return;
		}
		string opposingHeroCard = GameState.Get().GetOpposingSidePlayer().GetHero()
			.GetCardId();
		float thinkEmoteBossThinkChancePercentage = GetThinkEmoteBossThinkChancePercentage();
		float randomThink = Random.Range(0f, 1f);
		if (thinkEmoteBossThinkChancePercentage > randomThink)
		{
			Actor enemyActor = GameState.Get().GetOpposingSidePlayer().GetHeroCard()
				.GetActor();
			switch (opposingHeroCard)
			{
			case "DRGA_BOSS_01h4":
			{
				string voLine2 = PopRandomLine(m_RenoIdleLinesCopy);
				if (m_RenoIdleLinesCopy.Count == 0)
				{
					m_RenoIdleLinesCopy = new List<string>(m_RenoIdleLines);
				}
				Gameplay.Get().StartCoroutine(PlayBossLine(enemyActor, voLine2));
				break;
			}
			case "DRGA_BOSS_37h":
			{
				string voLine3 = PopRandomLine(m_AnduinIdleLinesCopy);
				if (m_AnduinIdleLinesCopy.Count == 0)
				{
					m_AnduinIdleLinesCopy = new List<string>(m_AnduinIdleLines);
				}
				Gameplay.Get().StartCoroutine(PlayBossLine(enemyActor, voLine3));
				break;
			}
			case "DRGA_BOSS_38h":
			{
				string voLine = PopRandomLine(m_SylvanasIdleLinesCopy);
				if (m_SylvanasIdleLinesCopy.Count == 0)
				{
					m_SylvanasIdleLinesCopy = new List<string>(m_SylvanasIdleLines);
				}
				Gameplay.Get().StartCoroutine(PlayBossLine(enemyActor, voLine));
				break;
			}
			}
		}
		else
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
				.PlayEmote(thinkEmote);
		}
	}
}
