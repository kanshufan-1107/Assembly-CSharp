using System;

[Serializable]
public enum MusicPlaylistType
{
	Invalid = 0,
	UI_MainTitle = 100,
	UI_Tournament = 101,
	UI_Arena = 102,
	UI_Friendly = 103,
	UI_CollectionManager = 104,
	UI_PackOpening = 105,
	UI_Credits = 106,
	UI_EndGameScreen = 107,
	UI_TavernBrawl = 108,
	UI_CMHeroSkinPreview = 109,
	UI_HeroicBrawl = 110,
	UI_NullSilence = 111,
	UI_Battlegrounds = 112,
	UI_Store = 113,
	UI_Duels = 114,
	UI_Journal = 115,
	UI_MercenariesPVPLobby = 116,
	UI_MercenariesSubMenus = 117,
	UI_MercenariesCMCaster = 118,
	UI_MercenariesCMFighter = 119,
	UI_MercenariesCMTank = 120,
	UI_MercenariesVillage = 121,
	UI_MercenariesZoneSelection = 122,
	UI_MercenariesShop = 123,
	UI_MercenariesTaskboard = 125,
	UI_MercenariesMailbox = 126,
	UI_MercenariesPackOpening = 127,
	UI_MercenariesWorkshop = 128,
	UI_MercenariesRenownConversion = 129,
	UI_BattleBash = 130,
	InGame_Mulligan = 200,
	InGame_MulliganSoft = 201,
	InGame_Default = 202,
	InGame_GvGBoard = 203,
	InGame_NaxxramasAdventure = 204,
	InGame_BRMAdventure = 205,
	InGame_LOE1Adventure = 206,
	InGame_LOE2Adventure = 207,
	InGame_LOE_Minecart = 208,
	InGame_LOE_Wing3 = 209,
	InGame_LOE_Wing4Mission4 = 210,
	InGame_Karazhan = 211,
	InGame_KarazhanPrologue = 212,
	InGame_KarazhanFreeMedivh = 213,
	InGame_ICC = 214,
	InGame_ICCLichKing = 215,
	InGame_ICCMulligan = 216,
	InGame_LOOT = 217,
	InGame_LOOTFinalBoss = 218,
	InGame_LOOTMulligan = 219,
	InGame_GIL = 220,
	InGame_GILFinalBoss = 221,
	InGame_GILMulligan = 222,
	InGame_BOT = 223,
	InGame_BOTFinalBoss = 224,
	InGame_BOTMulligan = 225,
	InGame_TRL = 226,
	InGame_TRLFinalBoss = 227,
	InGame_TRLMulligan = 228,
	InGame_TRLAdventure = 229,
	InGame_DAL = 230,
	InGame_DALFinalBoss = 231,
	InGame_DALMulligan = 232,
	InGame_ULD = 233,
	InGame_ULDFinalBoss = 234,
	InGame_ULDMulligan = 235,
	InGame_BGSShop = 236,
	InGame_BGSCombat = 237,
	InGame_DRG = 238,
	InGame_DRGMulligan = 239,
	InGame_DRGLOEBoss = 240,
	InGame_DRGEVILBoss = 241,
	InGame_BT = 242,
	InGame_DHPrologue = 243,
	InGame_DHMulligan = 244,
	InGame_DHPrologueBoss = 245,
	InGame_BT_FinalBoss = 246,
	InGame_SCH = 247,
	InGame_SCH_FinalLevels = 248,
	InGame_SCH_Mulligan = 249,
	InGame_DMF = 250,
	InGame_DMF_FinalLevels = 251,
	InGame_DMF_Mulligan = 252,
	InGame_BAR = 253,
	InGame_SW = 254,
	InGame_MERC1 = 255,
	InGame_MERCBOSS1 = 256,
	InGame_SW_Stockades = 257,
	InGame_AV = 258,
	InGame_AV_TavishBOM = 259,
	InGame_AV_BrukanBOM = 260,
	InGame_TSC = 261,
	InGame_TSC_Boss = 262,
	InGame_TSC_Leviathan = 263,
	InGame_TSC_Nazjatar = 264,
	InGame_REV = 265,
	InGame_MOTLK = 266,
	InGame_ETC = 267,
	InGame_TTN = 268,
	InGame_WW = 269,
	InGame_TOY = 270,
	UISolo_Practice = 300,
	UISolo_Naxxramas = 301,
	UISolo_BRM = 302,
	UISolo_LOE_Select = 303,
	UISolo_LOE_Mission = 304,
	UISolo_Karazhan = 305,
	UISolo_ICC = 306,
	UISolo_LOOT = 307,
	UISolo_LOOT_Select = 308,
	UISolo_GIL = 309,
	UISolo_GIL_Select = 310,
	UISolo_BOT = 311,
	UISolo_BOT_Select = 312,
	UISolo_TRL = 313,
	UISolo_TRL_Select = 314,
	UISolo_DAL_Select = 315,
	UISolo_DAL = 316,
	UISolo_ULD_Select = 317,
	UISolo_ULD = 318,
	UISolo_DRG_Select = 319,
	UISolo_DRG = 320,
	UISolo_DHPrologue = 321,
	UISolo_DHPrologue_Select = 322,
	UISolo_RPE = 323,
	UISolo_BT = 324,
	UISolo_BOH = 325,
	UISolo_BOHJaina = 326,
	UISolo_BOHRexxar = 327,
	UISolo_BOHGarrosh = 328,
	UISolo_BOHUther = 329,
	UISolo_BOHAnduin = 330,
	UISolo_BOHValeera = 331,
	UISolo_BOHThrall = 332,
	UISolo_BOMRokara = 333,
	UISolo_BOM = 334,
	UISolo_BOMXyrella = 335,
	UISolo_BOMGuff = 336,
	UISolo_BOHMalfurion = 337,
	UISolo_BOHGuldan = 338,
	UISolo_BOMKurtrus = 339,
	UISolo_BOHIllidan = 340,
	UISolo_BOMTamsin = 341,
	UISolo_BOMCariel = 342,
	UISolo_BOMTavish = 343,
	UISolo_BOMScabbs = 344,
	UISolo_BOMBrukan = 345,
	UISolo_BOMVarden = 346,
	UISolo_BOHFaelin = 347,
	UISolo_RLKAdvMenu = 348,
	Store_PacksClassic = 400,
	Store_PacksGvG = 401,
	Store_PacksTGT = 402,
	Store_PacksOG = 403,
	Store_PacksMSG = 404,
	Store_PacksUNG = 405,
	Store_PacksICC = 406,
	Store_PacksLOOT = 407,
	Store_PacksGIL = 408,
	Store_PacksBOT = 409,
	Store_PacksTRL = 410,
	Store_PacksDAL = 411,
	Store_PacksULD = 412,
	Store_PacksDRG = 413,
	Store_PacksBT = 414,
	Store_PacksSCH = 415,
	Store_packsDMF = 416,
	Store_PacksDMFMiniSet = 417,
	Store_PacksBAR = 418,
	Store_PacksSW = 419,
	Store_PacksMERC = 420,
	Store_PacksAV = 421,
	Store_PacksTSC = 422,
	Store_PacksREV = 423,
	Store_PacksRLK = 424,
	Store_PacksETC = 425,
	Store_PacksTTN = 426,
	Store_PacksCOT = 427,
	Store_PacksWW = 428,
	Store_PacksTOY = 429,
	Store_PacksVAC = 430,
	Store_PacksTTA = 431,
	Store_PacksGDB = 432,
	Store_PacksSC = 433,
	Store_AdvNaxxramas = 450,
	Store_AdvBRM = 451,
	Store_AdvLOE = 452,
	Store_AdvKarazhan = 453,
	Misc_Tutorial01 = 501,
	Misc_Tutorial01PackOpen = 502,
	CollectionManager_Battlegrounds = 600,
	Hero_Magni = 900,
	Hero_Alleria = 901,
	Hero_Medvih = 902,
	Hero_Liadrin = 903,
	Hero_Khadgar = 904,
	Hero_Morgl = 905,
	Hero_Tyrande = 906,
	Hero_Maiev = 907,
	Hero_Arthas = 908,
	Hero_Nemsy = 909,
	Hero_Lunara = 910,
	Hero_MechaJ = 912,
	Hero_SirAnnoyo = 913,
	Hero_Rastakhan = 914,
	Hero_Lazul = 915,
	Hero_ThunderKing = 916,
	Hero_Elise = 917,
	Hero_Deathwing = 918,
	Hero_Hazelbark = 919,
	Hero_Sylvanas = 920,
	Hero_LadyVashj = 921,
	Hero_Aranna = 922,
	Hero_KelThuzad = 923,
	Hero_NZoth = 924,
	Hero_Annhylde = 925,
	Hero_Hamuul = 926,
	Hero_Prestor = 927,
	Hero_FF_Alliance = 928,
	Hero_FF_Horde = 929,
	Hero_Ragnaros = 930,
	Hero_Yrel = 931,
	Hero_EdwinVanCleef = 932,
	Hero_VanndarStormpike = 933,
	Hero_Mechaskins = 934,
	Hero_Celeste = 935,
	Hero_DiaoChanValeera = 936,
	Hero_Xuen = 937,
	Hero_QueenAzshara = 938,
	Hero_Faelin = 939,
	Hero_Leeroy = 940,
	Hero_SirFinleyMrrgleton = 941,
	Hero_Garona = 942,
	Hero_KaelthasSunstrider = 943,
	Hero_SireDenathrius = 944,
	Hero_PrinceRenethal = 945,
	Hero_MagicalGuardians = 946,
	Hero_FoodTheme = 947,
	Hero_Jaraxxustein = 948,
	Hero_LorthemarTheron = 949,
	Hero_DeathKnights = 950,
	Hero_SallyWhitemane = 951,
	Hero_RangerGeneralSylvanas = 952,
	Hero_DarionMograine = 953,
	Hero_MalGanis = 954,
	Hero_Huln = 955,
	Hero_Omen = 956,
	Hero_Varian = 957,
	Hero_Saraad = 958,
	Hero_Hedanis = 959,
	Hero_Inzah = 960,
	Hero_EliteTaurenChieftain = 961,
	Hero_Halveria = 962,
	Hero_MCBlingtron = 963,
	Hero_KitWaxwhisker = 964,
	Hero_Rafaam = 965,
	Hero_Inge = 966,
	Hero_V07TR0N = 967,
	Hero_Sargeras = 968,
	Hero_NightWarriorTyrande = 969,
	Hero_Thorim = 970,
	Hero_Nozdormu = 971,
	Hero_CthunLegendary = 973,
	Hero_QueenLanathel = 974,
	Hero_Ulfar = 975,
	Hero_ReskaPitBoss = 976,
	Hero_EliseTheLeader = 977,
	Hero_LordGodfrey = 978,
	Hero_JusticeJaina = 979,
	Hero_DisidraStormglory = 980,
	Hero_KaileneEvergaze = 981,
	Hero_PatchesthePirate = 982,
	Hero_KingKrush = 983,
	Hero_ZailStarfallen = 984,
	Hero_Nerzhul = 985,
	Hero_BoulderfistOgre = 986,
	Hero_Arfus = 987,
	Hero_HakkartheHoundmaster = 988,
	Hero_IllidanStormrage = 989,
	Hero_MarinTheFox = 990,
	Hero_AFK = 991,
	Hero_Poxi = 992,
	Hero_Talanji = 993,
	Hero_AponiBrightmane = 994,
	Hero_RagnarosTheFireLord = 995,
	Hero_Leyara = 996,
	Hero_BolvarFordragon = 997,
	Hero_ProphetVelen = 998,
	Hero_WarlordDraka = 999,
	Hero_Malygos = 1000,
	Hero_Akama = 1001,
	Hero_NathanosBlightcaller = 1002,
	Hero_TirionFordring = 1003,
	Hero_Murgulis = 1004,
	Hero_FelfireRagnaros = 1005,
	Hero_Artanis = 1006,
	Hero_Raynor = 1007,
	Hero_Kerrigan = 1008
}
