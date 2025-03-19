using System;
using System.Collections;
using System.Collections.Generic;
using Blizzard.T5.Jobs;
using Blizzard.T5.Services;
using Hearthstone;
using UnityEngine;

public class GameDbf : IService
{
	public static Dbf<AccountLicenseDbfRecord> AccountLicense;

	public static Dbf<AchieveDbfRecord> Achieve;

	public static Dbf<AchieveConditionDbfRecord> AchieveCondition;

	public static Dbf<AchieveRegionDataDbfRecord> AchieveRegionData;

	public static Dbf<AchievementDbfRecord> Achievement;

	public static Dbf<AchievementCategoryDbfRecord> AchievementCategory;

	public static Dbf<AchievementSectionDbfRecord> AchievementSection;

	public static Dbf<AchievementSectionItemDbfRecord> AchievementSectionItem;

	public static Dbf<AchievementSubcategoryDbfRecord> AchievementSubcategory;

	public static Dbf<AdventureDbfRecord> Adventure;

	public static Dbf<AdventureDataDbfRecord> AdventureData;

	public static Dbf<AdventureDeckDbfRecord> AdventureDeck;

	public static Dbf<AdventureGuestHeroesDbfRecord> AdventureGuestHeroes;

	public static Dbf<AdventureHeroPowerDbfRecord> AdventureHeroPower;

	public static Dbf<AdventureLoadoutTreasuresDbfRecord> AdventureLoadoutTreasures;

	public static Dbf<AdventureMissionDbfRecord> AdventureMission;

	public static Dbf<AdventureModeDbfRecord> AdventureMode;

	public static Dbf<BannerDbfRecord> Banner;

	public static Dbf<BattlegroundsBoardSkinDbfRecord> BattlegroundsBoardSkin;

	public static Dbf<BattlegroundsEmoteDbfRecord> BattlegroundsEmote;

	public static Dbf<BattlegroundsFinisherDbfRecord> BattlegroundsFinisher;

	public static Dbf<BattlegroundsGuideSkinDbfRecord> BattlegroundsGuideSkin;

	public static Dbf<BattlegroundsHeroSkinDbfRecord> BattlegroundsHeroSkin;

	public static Dbf<BattlegroundsSeasonDbfRecord> BattlegroundsSeason;

	public static Dbf<BoardDbfRecord> Board;

	public static Dbf<BonusBountyDropChanceDbfRecord> BonusBountyDropChance;

	public static Dbf<BoosterDbfRecord> Booster;

	public static Dbf<BoosterCardSetDbfRecord> BoosterCardSet;

	public static Dbf<BoxProductBannerDbfRecord> BoxProductBanner;

	public static Dbf<BuildingTierDbfRecord> BuildingTier;

	public static Dbf<CardDbfRecord> Card;

	public static Dbf<CardAdditonalSearchTermsDbfRecord> CardAdditonalSearchTerms;

	public static Dbf<CardBackDbfRecord> CardBack;

	public static Dbf<CardChangeDbfRecord> CardChange;

	public static Dbf<CardDiscoverStringDbfRecord> CardDiscoverString;

	public static Dbf<CardEquipmentAltTextDbfRecord> CardEquipmentAltText;

	public static Dbf<CardHeroDbfRecord> CardHero;

	public static Dbf<CardPlayerDeckOverrideDbfRecord> CardPlayerDeckOverride;

	public static Dbf<CardRaceDbfRecord> CardRace;

	public static Dbf<CardSetDbfRecord> CardSet;

	public static Dbf<CardSetSpellOverrideDbfRecord> CardSetSpellOverride;

	public static Dbf<CardSetTimingDbfRecord> CardSetTiming;

	public static Dbf<CardTagDbfRecord> CardTag;

	public static Dbf<CardValueDbfRecord> CardValue;

	public static Dbf<CatchupPackEventDbfRecord> CatchupPackEvent;

	public static Dbf<CharacterDbfRecord> Character;

	public static Dbf<CharacterDialogDbfRecord> CharacterDialog;

	public static Dbf<CharacterDialogItemsDbfRecord> CharacterDialogItems;

	public static Dbf<ClassDbfRecord> Class;

	public static Dbf<ClassExclusionsDbfRecord> ClassExclusions;

	public static Dbf<ClientStringDbfRecord> ClientString;

	public static Dbf<CosmeticCoinDbfRecord> CosmeticCoin;

	public static Dbf<CounterpartCardsDbfRecord> CounterpartCards;

	public static Dbf<CreditsYearDbfRecord> CreditsYear;

	public static Dbf<DeckDbfRecord> Deck;

	public static Dbf<DeckCardDbfRecord> DeckCard;

	public static Dbf<DeckRulesetDbfRecord> DeckRuleset;

	public static Dbf<DeckRulesetRuleDbfRecord> DeckRulesetRule;

	public static Dbf<DeckRulesetRuleSubsetDbfRecord> DeckRulesetRuleSubset;

	public static Dbf<DeckTemplateDbfRecord> DeckTemplate;

	public static Dbf<DeckTemplateChoicesDbfRecord> DeckTemplateChoices;

	public static Dbf<DetailsVideoCueDbfRecord> DetailsVideoCue;

	public static Dbf<DkRuneListDbfRecord> DkRuneList;

	public static Dbf<DraftContentDbfRecord> DraftContent;

	public static Dbf<EventRewardTrackDbfRecord> EventRewardTrack;

	public static Dbf<ExternalUrlDbfRecord> ExternalUrl;

	public static Dbf<FixedRewardDbfRecord> FixedReward;

	public static Dbf<FixedRewardActionDbfRecord> FixedRewardAction;

	public static Dbf<FixedRewardMapDbfRecord> FixedRewardMap;

	public static Dbf<FormulaDbfRecord> Formula;

	public static Dbf<FormulaChangePointDbfRecord> FormulaChangePoint;

	public static Dbf<GameModeDbfRecord> GameMode;

	public static Dbf<GameSaveSubkeyDbfRecord> GameSaveSubkey;

	public static Dbf<GlobalDbfRecord> Global;

	public static Dbf<GuestHeroDbfRecord> GuestHero;

	public static Dbf<GuestHeroSelectionRatioDbfRecord> GuestHeroSelectionRatio;

	public static Dbf<HiddenCardSetsDbfRecord> HiddenCardSets;

	public static Dbf<HiddenLicenseDbfRecord> HiddenLicense;

	public static Dbf<InitCardValueDbfRecord> InitCardValue;

	public static Dbf<KeywordTextDbfRecord> KeywordText;

	public static Dbf<LeagueDbfRecord> League;

	public static Dbf<LeagueBgPublicRatingEquivDbfRecord> LeagueBgPublicRatingEquiv;

	public static Dbf<LeagueGameTypeDbfRecord> LeagueGameType;

	public static Dbf<LeagueRankDbfRecord> LeagueRank;

	public static Dbf<LettuceAbilityDbfRecord> LettuceAbility;

	public static Dbf<LettuceAbilityTierDbfRecord> LettuceAbilityTier;

	public static Dbf<LettuceBountyDbfRecord> LettuceBounty;

	public static Dbf<LettuceBountyFinalRespresentiveRewardsDbfRecord> LettuceBountyFinalRespresentiveRewards;

	public static Dbf<LettuceBountyFinalRewardsDbfRecord> LettuceBountyFinalRewards;

	public static Dbf<LettuceBountySetDbfRecord> LettuceBountySet;

	public static Dbf<LettuceEquipmentDbfRecord> LettuceEquipment;

	public static Dbf<LettuceEquipmentModifierDataDbfRecord> LettuceEquipmentModifierData;

	public static Dbf<LettuceEquipmentTierDbfRecord> LettuceEquipmentTier;

	public static Dbf<LettuceMapNodeTypeDbfRecord> LettuceMapNodeType;

	public static Dbf<LettuceMapNodeTypeAnomalyDbfRecord> LettuceMapNodeTypeAnomaly;

	public static Dbf<LettuceMercenaryDbfRecord> LettuceMercenary;

	public static Dbf<LettuceMercenaryAbilityDbfRecord> LettuceMercenaryAbility;

	public static Dbf<LettuceMercenaryEquipmentDbfRecord> LettuceMercenaryEquipment;

	public static Dbf<LettuceMercenaryLevelDbfRecord> LettuceMercenaryLevel;

	public static Dbf<LettuceMercenaryLevelStatsDbfRecord> LettuceMercenaryLevelStats;

	public static Dbf<LettuceMercenarySpecializationDbfRecord> LettuceMercenarySpecialization;

	public static Dbf<LettuceTreasureDbfRecord> LettuceTreasure;

	public static Dbf<LettuceTutorialVoDbfRecord> LettuceTutorialVo;

	public static Dbf<LoginPopupSequenceDbfRecord> LoginPopupSequence;

	public static Dbf<LoginPopupSequencePopupDbfRecord> LoginPopupSequencePopup;

	public static Dbf<LoginRewardDbfRecord> LoginReward;

	public static Dbf<LuckyDrawBoxDbfRecord> LuckyDrawBox;

	public static Dbf<LuckyDrawRewardsDbfRecord> LuckyDrawRewards;

	public static Dbf<MercTriggeredEventDbfRecord> MercTriggeredEvent;

	public static Dbf<MercTriggeringEventDbfRecord> MercTriggeringEvent;

	public static Dbf<MercenariesRandomRewardDbfRecord> MercenariesRandomReward;

	public static Dbf<MercenariesRankedSeasonRewardRankDbfRecord> MercenariesRankedSeasonRewardRank;

	public static Dbf<MercenaryAllowedTreasureDbfRecord> MercenaryAllowedTreasure;

	public static Dbf<MercenaryArtVariationDbfRecord> MercenaryArtVariation;

	public static Dbf<MercenaryArtVariationPremiumDbfRecord> MercenaryArtVariationPremium;

	public static Dbf<MercenaryBuildingDbfRecord> MercenaryBuilding;

	public static Dbf<MercenaryVillageTriggerDbfRecord> MercenaryVillageTrigger;

	public static Dbf<MercenaryVisitorDbfRecord> MercenaryVisitor;

	public static Dbf<MiniSetDbfRecord> MiniSet;

	public static Dbf<ModifiedLettuceAbilityCardTagDbfRecord> ModifiedLettuceAbilityCardTag;

	public static Dbf<ModifiedLettuceAbilityValueDbfRecord> ModifiedLettuceAbilityValue;

	public static Dbf<MythicAbilityScalingCardTagDbfRecord> MythicAbilityScalingCardTag;

	public static Dbf<MythicEquipmentScalingCardTagDbfRecord> MythicEquipmentScalingCardTag;

	public static Dbf<MythicEquipmentScalingDestinationCardTagDbfRecord> MythicEquipmentScalingDestinationCardTag;

	public static Dbf<NextTiersDbfRecord> NextTiers;

	public static Dbf<OverrideQuestPoolIdListDbfRecord> OverrideQuestPoolIdList;

	public static Dbf<OwnershipReqListDbfRecord> OwnershipReqList;

	public static Dbf<PaidPremiumTierDbfRecord> PaidPremiumTier;

	public static Dbf<PlayerExperimentDbfRecord> PlayerExperiment;

	public static Dbf<PowerDefinitionDbfRecord> PowerDefinition;

	public static Dbf<ProductDbfRecord> Product;

	public static Dbf<ProductClientDataDbfRecord> ProductClientData;

	public static Dbf<PvpdrSeasonDbfRecord> PvpdrSeason;

	public static Dbf<QuestDbfRecord> Quest;

	public static Dbf<QuestChangeDbfRecord> QuestChange;

	public static Dbf<QuestDialogDbfRecord> QuestDialog;

	public static Dbf<QuestDialogOnCompleteDbfRecord> QuestDialogOnComplete;

	public static Dbf<QuestDialogOnProgress1DbfRecord> QuestDialogOnProgress1;

	public static Dbf<QuestDialogOnProgress2DbfRecord> QuestDialogOnProgress2;

	public static Dbf<QuestDialogOnReceivedDbfRecord> QuestDialogOnReceived;

	public static Dbf<QuestModifierDbfRecord> QuestModifier;

	public static Dbf<QuestPoolDbfRecord> QuestPool;

	public static Dbf<RankedPlaySeasonDbfRecord> RankedPlaySeason;

	public static Dbf<RegionOverridesDbfRecord> RegionOverrides;

	public static Dbf<RelatedCardsDbfRecord> RelatedCards;

	public static Dbf<RepeatableTaskListDbfRecord> RepeatableTaskList;

	public static Dbf<ReplacementsWhenPlayedDbfRecord> ReplacementsWhenPlayed;

	public static Dbf<RewardBagDbfRecord> RewardBag;

	public static Dbf<RewardChestDbfRecord> RewardChest;

	public static Dbf<RewardChestContentsDbfRecord> RewardChestContents;

	public static Dbf<RewardItemDbfRecord> RewardItem;

	public static Dbf<RewardListDbfRecord> RewardList;

	public static Dbf<RewardTrackDbfRecord> RewardTrack;

	public static Dbf<RewardTrackLevelDbfRecord> RewardTrackLevel;

	public static Dbf<ScalingTreasureCardTagDbfRecord> ScalingTreasureCardTag;

	public static Dbf<ScenarioDbfRecord> Scenario;

	public static Dbf<ScenarioGuestHeroesDbfRecord> ScenarioGuestHeroes;

	public static Dbf<ScenarioOverrideDbfRecord> ScenarioOverride;

	public static Dbf<ScheduledCharacterDialogDbfRecord> ScheduledCharacterDialog;

	public static Dbf<ScoreLabelDbfRecord> ScoreLabel;

	public static Dbf<SellableDeckDbfRecord> SellableDeck;

	public static Dbf<SetRotationEventDbfRecord> SetRotationEvent;

	public static Dbf<ShopTierDbfRecord> ShopTier;

	public static Dbf<ShopTierProductSaleDbfRecord> ShopTierProductSale;

	public static Dbf<SideboardCardDbfRecord> SideboardCard;

	public static Dbf<SignatureCardDbfRecord> SignatureCard;

	public static Dbf<SignatureFrameDbfRecord> SignatureFrame;

	public static Dbf<SpecialEventDbfRecord> SpecialEvent;

	public static Dbf<SubsetDbfRecord> Subset;

	public static Dbf<SubsetCardDbfRecord> SubsetCard;

	public static Dbf<SubsetRuleDbfRecord> SubsetRule;

	public static Dbf<TaskListDbfRecord> TaskList;

	public static Dbf<TavernBrawlTicketDbfRecord> TavernBrawlTicket;

	public static Dbf<TavernGuideQuestDbfRecord> TavernGuideQuest;

	public static Dbf<TavernGuideQuestRecommendedClassesDbfRecord> TavernGuideQuestRecommendedClasses;

	public static Dbf<TavernGuideQuestSetDbfRecord> TavernGuideQuestSet;

	public static Dbf<TestVariationDbfRecord> TestVariation;

	public static Dbf<TierPropertiesDbfRecord> TierProperties;

	public static Dbf<TriggerDbfRecord> Trigger;

	public static Dbf<UnlockedTavernGuideSetDbfRecord> UnlockedTavernGuideSet;

	public static Dbf<VisitorTaskDbfRecord> VisitorTask;

	public static Dbf<VisitorTaskChainDbfRecord> VisitorTaskChain;

	public static Dbf<WingDbfRecord> Wing;

	public static Dbf<XpOnPlacementDbfRecord> XpOnPlacement;

	public static Dbf<XpOnPlacementGameTypeMultiplierDbfRecord> XpOnPlacementGameTypeMultiplier;

	public static Dbf<XpPerTimeGameTypeMultiplierDbfRecord> XpPerTimeGameTypeMultiplier;

	private static GameDbfIndex s_index;

	private static DOPAsset s_DOPAsset = null;

	private static Dictionary<string, IDbf> s_allDbfs = new Dictionary<string, IDbf>();

	public const string kDOPAssetPath = "Assets/Game/DBF-Asset/DOPAsset.asset";

	public static bool IsLoaded { get; set; }

	public static IEnumerable<IDbf> AllDbfs => s_allDbfs.Values;

	private static Action[] GetLoadDbfActions(DbfFormat format)
	{
		return new Action[186]
		{
			delegate
			{
				AccountLicense = Dbf<AccountLicenseDbfRecord>.Load("ACCOUNT_LICENSE", format);
			},
			delegate
			{
				Achieve = Dbf<AchieveDbfRecord>.Load("ACHIEVE", format);
			},
			delegate
			{
				AchieveCondition = Dbf<AchieveConditionDbfRecord>.Load("ACHIEVE_CONDITION", format);
			},
			delegate
			{
				AchieveRegionData = Dbf<AchieveRegionDataDbfRecord>.Load("ACHIEVE_REGION_DATA", format);
			},
			delegate
			{
				Achievement = Dbf<AchievementDbfRecord>.Load("ACHIEVEMENT", format);
			},
			delegate
			{
				AchievementCategory = Dbf<AchievementCategoryDbfRecord>.Load("ACHIEVEMENT_CATEGORY", format);
			},
			delegate
			{
				AchievementSection = Dbf<AchievementSectionDbfRecord>.Load("ACHIEVEMENT_SECTION", format);
			},
			delegate
			{
				AchievementSectionItem = Dbf<AchievementSectionItemDbfRecord>.Load("ACHIEVEMENT_SECTION_ITEM", format);
			},
			delegate
			{
				AchievementSubcategory = Dbf<AchievementSubcategoryDbfRecord>.Load("ACHIEVEMENT_SUBCATEGORY", format);
			},
			delegate
			{
				Adventure = Dbf<AdventureDbfRecord>.Load("ADVENTURE", format);
			},
			delegate
			{
				AdventureData = Dbf<AdventureDataDbfRecord>.Load("ADVENTURE_DATA", format);
			},
			delegate
			{
				AdventureDeck = Dbf<AdventureDeckDbfRecord>.Load("ADVENTURE_DECK", format);
			},
			delegate
			{
				AdventureGuestHeroes = Dbf<AdventureGuestHeroesDbfRecord>.Load("ADVENTURE_GUEST_HEROES", format);
			},
			delegate
			{
				AdventureHeroPower = Dbf<AdventureHeroPowerDbfRecord>.Load("ADVENTURE_HERO_POWER", format);
			},
			delegate
			{
				AdventureLoadoutTreasures = Dbf<AdventureLoadoutTreasuresDbfRecord>.Load("ADVENTURE_LOADOUT_TREASURES", format);
			},
			delegate
			{
				AdventureMission = Dbf<AdventureMissionDbfRecord>.Load("ADVENTURE_MISSION", format);
			},
			delegate
			{
				AdventureMode = Dbf<AdventureModeDbfRecord>.Load("ADVENTURE_MODE", format);
			},
			delegate
			{
				Banner = Dbf<BannerDbfRecord>.Load("BANNER", format);
			},
			delegate
			{
				BattlegroundsBoardSkin = Dbf<BattlegroundsBoardSkinDbfRecord>.Load("BATTLEGROUNDS_BOARD_SKIN", format);
			},
			delegate
			{
				BattlegroundsEmote = Dbf<BattlegroundsEmoteDbfRecord>.Load("BATTLEGROUNDS_EMOTE", format);
			},
			delegate
			{
				BattlegroundsFinisher = Dbf<BattlegroundsFinisherDbfRecord>.Load("BATTLEGROUNDS_FINISHER", format);
			},
			delegate
			{
				BattlegroundsGuideSkin = Dbf<BattlegroundsGuideSkinDbfRecord>.Load("BATTLEGROUNDS_GUIDE_SKIN", format);
			},
			delegate
			{
				BattlegroundsHeroSkin = Dbf<BattlegroundsHeroSkinDbfRecord>.Load("BATTLEGROUNDS_HERO_SKIN", format);
			},
			delegate
			{
				BattlegroundsSeason = Dbf<BattlegroundsSeasonDbfRecord>.Load("BATTLEGROUNDS_SEASON", format);
			},
			delegate
			{
				Board = Dbf<BoardDbfRecord>.Load("BOARD", format);
			},
			delegate
			{
				BonusBountyDropChance = Dbf<BonusBountyDropChanceDbfRecord>.Load("BONUS_BOUNTY_DROP_CHANCE", format);
			},
			delegate
			{
				Booster = Dbf<BoosterDbfRecord>.Load("BOOSTER", format);
			},
			delegate
			{
				BoosterCardSet = Dbf<BoosterCardSetDbfRecord>.Load("BOOSTER_CARD_SET", format);
			},
			delegate
			{
				BoxProductBanner = Dbf<BoxProductBannerDbfRecord>.Load("BOX_PRODUCT_BANNER", format);
			},
			delegate
			{
				BuildingTier = Dbf<BuildingTierDbfRecord>.Load("BUILDING_TIER", format);
			},
			delegate
			{
				Card = Dbf<CardDbfRecord>.Load("CARD", format);
			},
			delegate
			{
				CardAdditonalSearchTerms = Dbf<CardAdditonalSearchTermsDbfRecord>.Load("CARD_ADDITONAL_SEARCH_TERMS", format);
			},
			delegate
			{
				CardBack = Dbf<CardBackDbfRecord>.Load("CARD_BACK", format);
			},
			delegate
			{
				CardChange = Dbf<CardChangeDbfRecord>.Load("CARD_CHANGE", format);
			},
			delegate
			{
				CardDiscoverString = Dbf<CardDiscoverStringDbfRecord>.Load("CARD_DISCOVER_STRING", format);
			},
			delegate
			{
				CardEquipmentAltText = Dbf<CardEquipmentAltTextDbfRecord>.Load("CARD_EQUIPMENT_ALT_TEXT", format);
			},
			delegate
			{
				CardHero = Dbf<CardHeroDbfRecord>.Load("CARD_HERO", format);
			},
			delegate
			{
				CardPlayerDeckOverride = Dbf<CardPlayerDeckOverrideDbfRecord>.Load("CARD_PLAYER_DECK_OVERRIDE", format);
			},
			delegate
			{
				CardRace = Dbf<CardRaceDbfRecord>.Load("CARD_RACE", format);
			},
			delegate
			{
				CardSet = Dbf<CardSetDbfRecord>.Load("CARD_SET", format);
			},
			delegate
			{
				CardSetSpellOverride = Dbf<CardSetSpellOverrideDbfRecord>.Load("CARD_SET_SPELL_OVERRIDE", format);
			},
			delegate
			{
				CardSetTiming = Dbf<CardSetTimingDbfRecord>.Load("CARD_SET_TIMING", format);
			},
			delegate
			{
				CardTag = Dbf<CardTagDbfRecord>.Load("CARD_TAG", format);
			},
			delegate
			{
				CardValue = Dbf<CardValueDbfRecord>.Load("CARD_VALUE", format);
			},
			delegate
			{
				CatchupPackEvent = Dbf<CatchupPackEventDbfRecord>.Load("CATCHUP_PACK_EVENT", format);
			},
			delegate
			{
				Character = Dbf<CharacterDbfRecord>.Load("CHARACTER", format);
			},
			delegate
			{
				CharacterDialog = Dbf<CharacterDialogDbfRecord>.Load("CHARACTER_DIALOG", format);
			},
			delegate
			{
				CharacterDialogItems = Dbf<CharacterDialogItemsDbfRecord>.Load("CHARACTER_DIALOG_ITEMS", format);
			},
			delegate
			{
				Class = Dbf<ClassDbfRecord>.Load("CLASS", format);
			},
			delegate
			{
				ClassExclusions = Dbf<ClassExclusionsDbfRecord>.Load("CLASS_EXCLUSIONS", format);
			},
			delegate
			{
				ClientString = Dbf<ClientStringDbfRecord>.Load("CLIENT_STRING", format);
			},
			delegate
			{
				CosmeticCoin = Dbf<CosmeticCoinDbfRecord>.Load("COSMETIC_COIN", format);
			},
			delegate
			{
				CounterpartCards = Dbf<CounterpartCardsDbfRecord>.Load("COUNTERPART_CARDS", format);
			},
			delegate
			{
				CreditsYear = Dbf<CreditsYearDbfRecord>.Load("CREDITS_YEAR", format);
			},
			delegate
			{
				Deck = Dbf<DeckDbfRecord>.Load("DECK", format);
			},
			delegate
			{
				DeckCard = Dbf<DeckCardDbfRecord>.Load("DECK_CARD", format);
			},
			delegate
			{
				DeckRuleset = Dbf<DeckRulesetDbfRecord>.Load("DECK_RULESET", format);
			},
			delegate
			{
				DeckRulesetRule = Dbf<DeckRulesetRuleDbfRecord>.Load("DECK_RULESET_RULE", format);
			},
			delegate
			{
				DeckRulesetRuleSubset = Dbf<DeckRulesetRuleSubsetDbfRecord>.Load("DECK_RULESET_RULE_SUBSET", format);
			},
			delegate
			{
				DeckTemplate = Dbf<DeckTemplateDbfRecord>.Load("DECK_TEMPLATE", format);
			},
			delegate
			{
				DeckTemplateChoices = Dbf<DeckTemplateChoicesDbfRecord>.Load("DECK_TEMPLATE_CHOICES", format);
			},
			delegate
			{
				DetailsVideoCue = Dbf<DetailsVideoCueDbfRecord>.Load("DETAILS_VIDEO_CUE", format);
			},
			delegate
			{
				DkRuneList = Dbf<DkRuneListDbfRecord>.Load("DK_RUNE_LIST", format);
			},
			delegate
			{
				DraftContent = Dbf<DraftContentDbfRecord>.Load("DRAFT_CONTENT", format);
			},
			delegate
			{
				EventRewardTrack = Dbf<EventRewardTrackDbfRecord>.Load("EVENT_REWARD_TRACK", format);
			},
			delegate
			{
				ExternalUrl = Dbf<ExternalUrlDbfRecord>.Load("EXTERNAL_URL", format);
			},
			delegate
			{
				FixedReward = Dbf<FixedRewardDbfRecord>.Load("FIXED_REWARD", format);
			},
			delegate
			{
				FixedRewardAction = Dbf<FixedRewardActionDbfRecord>.Load("FIXED_REWARD_ACTION", format);
			},
			delegate
			{
				FixedRewardMap = Dbf<FixedRewardMapDbfRecord>.Load("FIXED_REWARD_MAP", format);
			},
			delegate
			{
				Formula = Dbf<FormulaDbfRecord>.Load("FORMULA", format);
			},
			delegate
			{
				FormulaChangePoint = Dbf<FormulaChangePointDbfRecord>.Load("FORMULA_CHANGE_POINT", format);
			},
			delegate
			{
				GameMode = Dbf<GameModeDbfRecord>.Load("GAME_MODE", format);
			},
			delegate
			{
				GameSaveSubkey = Dbf<GameSaveSubkeyDbfRecord>.Load("GAME_SAVE_SUBKEY", format);
			},
			delegate
			{
				Global = Dbf<GlobalDbfRecord>.Load("GLOBAL", format);
			},
			delegate
			{
				GuestHero = Dbf<GuestHeroDbfRecord>.Load("GUEST_HERO", format);
			},
			delegate
			{
				GuestHeroSelectionRatio = Dbf<GuestHeroSelectionRatioDbfRecord>.Load("GUEST_HERO_SELECTION_RATIO", format);
			},
			delegate
			{
				HiddenCardSets = Dbf<HiddenCardSetsDbfRecord>.Load("HIDDEN_CARD_SETS", format);
			},
			delegate
			{
				HiddenLicense = Dbf<HiddenLicenseDbfRecord>.Load("HIDDEN_LICENSE", format);
			},
			delegate
			{
				InitCardValue = Dbf<InitCardValueDbfRecord>.Load("INIT_CARD_VALUE", format);
			},
			delegate
			{
				KeywordText = Dbf<KeywordTextDbfRecord>.Load("KEYWORD_TEXT", format);
			},
			delegate
			{
				League = Dbf<LeagueDbfRecord>.Load("LEAGUE", format);
			},
			delegate
			{
				LeagueBgPublicRatingEquiv = Dbf<LeagueBgPublicRatingEquivDbfRecord>.Load("LEAGUE_BG_PUBLIC_RATING_EQUIV", format);
			},
			delegate
			{
				LeagueGameType = Dbf<LeagueGameTypeDbfRecord>.Load("LEAGUE_GAME_TYPE", format);
			},
			delegate
			{
				LeagueRank = Dbf<LeagueRankDbfRecord>.Load("LEAGUE_RANK", format);
			},
			delegate
			{
				LettuceAbility = Dbf<LettuceAbilityDbfRecord>.Load("LETTUCE_ABILITY", format);
			},
			delegate
			{
				LettuceAbilityTier = Dbf<LettuceAbilityTierDbfRecord>.Load("LETTUCE_ABILITY_TIER", format);
			},
			delegate
			{
				LettuceBounty = Dbf<LettuceBountyDbfRecord>.Load("LETTUCE_BOUNTY", format);
			},
			delegate
			{
				LettuceBountyFinalRespresentiveRewards = Dbf<LettuceBountyFinalRespresentiveRewardsDbfRecord>.Load("LETTUCE_BOUNTY_FINAL_RESPRESENTIVE_REWARDS", format);
			},
			delegate
			{
				LettuceBountyFinalRewards = Dbf<LettuceBountyFinalRewardsDbfRecord>.Load("LETTUCE_BOUNTY_FINAL_REWARDS", format);
			},
			delegate
			{
				LettuceBountySet = Dbf<LettuceBountySetDbfRecord>.Load("LETTUCE_BOUNTY_SET", format);
			},
			delegate
			{
				LettuceEquipment = Dbf<LettuceEquipmentDbfRecord>.Load("LETTUCE_EQUIPMENT", format);
			},
			delegate
			{
				LettuceEquipmentModifierData = Dbf<LettuceEquipmentModifierDataDbfRecord>.Load("LETTUCE_EQUIPMENT_MODIFIER_DATA", format);
			},
			delegate
			{
				LettuceEquipmentTier = Dbf<LettuceEquipmentTierDbfRecord>.Load("LETTUCE_EQUIPMENT_TIER", format);
			},
			delegate
			{
				LettuceMapNodeType = Dbf<LettuceMapNodeTypeDbfRecord>.Load("LETTUCE_MAP_NODE_TYPE", format);
			},
			delegate
			{
				LettuceMapNodeTypeAnomaly = Dbf<LettuceMapNodeTypeAnomalyDbfRecord>.Load("LETTUCE_MAP_NODE_TYPE_ANOMALY", format);
			},
			delegate
			{
				LettuceMercenary = Dbf<LettuceMercenaryDbfRecord>.Load("LETTUCE_MERCENARY", format);
			},
			delegate
			{
				LettuceMercenaryAbility = Dbf<LettuceMercenaryAbilityDbfRecord>.Load("LETTUCE_MERCENARY_ABILITY", format);
			},
			delegate
			{
				LettuceMercenaryEquipment = Dbf<LettuceMercenaryEquipmentDbfRecord>.Load("LETTUCE_MERCENARY_EQUIPMENT", format);
			},
			delegate
			{
				LettuceMercenaryLevel = Dbf<LettuceMercenaryLevelDbfRecord>.Load("LETTUCE_MERCENARY_LEVEL", format);
			},
			delegate
			{
				LettuceMercenaryLevelStats = Dbf<LettuceMercenaryLevelStatsDbfRecord>.Load("LETTUCE_MERCENARY_LEVEL_STATS", format);
			},
			delegate
			{
				LettuceMercenarySpecialization = Dbf<LettuceMercenarySpecializationDbfRecord>.Load("LETTUCE_MERCENARY_SPECIALIZATION", format);
			},
			delegate
			{
				LettuceTreasure = Dbf<LettuceTreasureDbfRecord>.Load("LETTUCE_TREASURE", format);
			},
			delegate
			{
				LettuceTutorialVo = Dbf<LettuceTutorialVoDbfRecord>.Load("LETTUCE_TUTORIAL_VO", format);
			},
			delegate
			{
				LoginPopupSequence = Dbf<LoginPopupSequenceDbfRecord>.Load("LOGIN_POPUP_SEQUENCE", format);
			},
			delegate
			{
				LoginPopupSequencePopup = Dbf<LoginPopupSequencePopupDbfRecord>.Load("LOGIN_POPUP_SEQUENCE_POPUP", format);
			},
			delegate
			{
				LoginReward = Dbf<LoginRewardDbfRecord>.Load("LOGIN_REWARD", format);
			},
			delegate
			{
				LuckyDrawBox = Dbf<LuckyDrawBoxDbfRecord>.Load("LUCKY_DRAW_BOX", format);
			},
			delegate
			{
				LuckyDrawRewards = Dbf<LuckyDrawRewardsDbfRecord>.Load("LUCKY_DRAW_REWARDS", format);
			},
			delegate
			{
				MercTriggeredEvent = Dbf<MercTriggeredEventDbfRecord>.Load("MERC_TRIGGERED_EVENT", format);
			},
			delegate
			{
				MercTriggeringEvent = Dbf<MercTriggeringEventDbfRecord>.Load("MERC_TRIGGERING_EVENT", format);
			},
			delegate
			{
				MercenariesRandomReward = Dbf<MercenariesRandomRewardDbfRecord>.Load("MERCENARIES_RANDOM_REWARD", format);
			},
			delegate
			{
				MercenariesRankedSeasonRewardRank = Dbf<MercenariesRankedSeasonRewardRankDbfRecord>.Load("MERCENARIES_RANKED_SEASON_REWARD_RANK", format);
			},
			delegate
			{
				MercenaryAllowedTreasure = Dbf<MercenaryAllowedTreasureDbfRecord>.Load("MERCENARY_ALLOWED_TREASURE", format);
			},
			delegate
			{
				MercenaryArtVariation = Dbf<MercenaryArtVariationDbfRecord>.Load("MERCENARY_ART_VARIATION", format);
			},
			delegate
			{
				MercenaryArtVariationPremium = Dbf<MercenaryArtVariationPremiumDbfRecord>.Load("MERCENARY_ART_VARIATION_PREMIUM", format);
			},
			delegate
			{
				MercenaryBuilding = Dbf<MercenaryBuildingDbfRecord>.Load("MERCENARY_BUILDING", format);
			},
			delegate
			{
				MercenaryVillageTrigger = Dbf<MercenaryVillageTriggerDbfRecord>.Load("MERCENARY_VILLAGE_TRIGGER", format);
			},
			delegate
			{
				MercenaryVisitor = Dbf<MercenaryVisitorDbfRecord>.Load("MERCENARY_VISITOR", format);
			},
			delegate
			{
				MiniSet = Dbf<MiniSetDbfRecord>.Load("MINI_SET", format);
			},
			delegate
			{
				ModifiedLettuceAbilityCardTag = Dbf<ModifiedLettuceAbilityCardTagDbfRecord>.Load("MODIFIED_LETTUCE_ABILITY_CARD_TAG", format);
			},
			delegate
			{
				ModifiedLettuceAbilityValue = Dbf<ModifiedLettuceAbilityValueDbfRecord>.Load("MODIFIED_LETTUCE_ABILITY_VALUE", format);
			},
			delegate
			{
				MythicAbilityScalingCardTag = Dbf<MythicAbilityScalingCardTagDbfRecord>.Load("MYTHIC_ABILITY_SCALING_CARD_TAG", format);
			},
			delegate
			{
				MythicEquipmentScalingCardTag = Dbf<MythicEquipmentScalingCardTagDbfRecord>.Load("MYTHIC_EQUIPMENT_SCALING_CARD_TAG", format);
			},
			delegate
			{
				MythicEquipmentScalingDestinationCardTag = Dbf<MythicEquipmentScalingDestinationCardTagDbfRecord>.Load("MYTHIC_EQUIPMENT_SCALING_DESTINATION_CARD_TAG", format);
			},
			delegate
			{
				NextTiers = Dbf<NextTiersDbfRecord>.Load("NEXT_TIERS", format);
			},
			delegate
			{
				OverrideQuestPoolIdList = Dbf<OverrideQuestPoolIdListDbfRecord>.Load("OVERRIDE_QUEST_POOL_ID_LIST", format);
			},
			delegate
			{
				OwnershipReqList = Dbf<OwnershipReqListDbfRecord>.Load("OWNERSHIP_REQ_LIST", format);
			},
			delegate
			{
				PaidPremiumTier = Dbf<PaidPremiumTierDbfRecord>.Load("PAID_PREMIUM_TIER", format);
			},
			delegate
			{
				PlayerExperiment = Dbf<PlayerExperimentDbfRecord>.Load("PLAYER_EXPERIMENT", format);
			},
			delegate
			{
				PowerDefinition = Dbf<PowerDefinitionDbfRecord>.Load("POWER_DEFINITION", format);
			},
			delegate
			{
				Product = Dbf<ProductDbfRecord>.Load("PRODUCT", format);
			},
			delegate
			{
				ProductClientData = Dbf<ProductClientDataDbfRecord>.Load("PRODUCT_CLIENT_DATA", format);
			},
			delegate
			{
				PvpdrSeason = Dbf<PvpdrSeasonDbfRecord>.Load("PVPDR_SEASON", format);
			},
			delegate
			{
				Quest = Dbf<QuestDbfRecord>.Load("QUEST", format);
			},
			delegate
			{
				QuestChange = Dbf<QuestChangeDbfRecord>.Load("QUEST_CHANGE", format);
			},
			delegate
			{
				QuestDialog = Dbf<QuestDialogDbfRecord>.Load("QUEST_DIALOG", format);
			},
			delegate
			{
				QuestDialogOnComplete = Dbf<QuestDialogOnCompleteDbfRecord>.Load("QUEST_DIALOG_ON_COMPLETE", format);
			},
			delegate
			{
				QuestDialogOnProgress1 = Dbf<QuestDialogOnProgress1DbfRecord>.Load("QUEST_DIALOG_ON_PROGRESS1", format);
			},
			delegate
			{
				QuestDialogOnProgress2 = Dbf<QuestDialogOnProgress2DbfRecord>.Load("QUEST_DIALOG_ON_PROGRESS2", format);
			},
			delegate
			{
				QuestDialogOnReceived = Dbf<QuestDialogOnReceivedDbfRecord>.Load("QUEST_DIALOG_ON_RECEIVED", format);
			},
			delegate
			{
				QuestModifier = Dbf<QuestModifierDbfRecord>.Load("QUEST_MODIFIER", format);
			},
			delegate
			{
				QuestPool = Dbf<QuestPoolDbfRecord>.Load("QUEST_POOL", format);
			},
			delegate
			{
				RankedPlaySeason = Dbf<RankedPlaySeasonDbfRecord>.Load("RANKED_PLAY_SEASON", format);
			},
			delegate
			{
				RegionOverrides = Dbf<RegionOverridesDbfRecord>.Load("REGION_OVERRIDES", format);
			},
			delegate
			{
				RelatedCards = Dbf<RelatedCardsDbfRecord>.Load("RELATED_CARDS", format);
			},
			delegate
			{
				RepeatableTaskList = Dbf<RepeatableTaskListDbfRecord>.Load("REPEATABLE_TASK_LIST", format);
			},
			delegate
			{
				ReplacementsWhenPlayed = Dbf<ReplacementsWhenPlayedDbfRecord>.Load("REPLACEMENTS_WHEN_PLAYED", format);
			},
			delegate
			{
				RewardBag = Dbf<RewardBagDbfRecord>.Load("REWARD_BAG", format);
			},
			delegate
			{
				RewardChest = Dbf<RewardChestDbfRecord>.Load("REWARD_CHEST", format);
			},
			delegate
			{
				RewardChestContents = Dbf<RewardChestContentsDbfRecord>.Load("REWARD_CHEST_CONTENTS", format);
			},
			delegate
			{
				RewardItem = Dbf<RewardItemDbfRecord>.Load("REWARD_ITEM", format);
			},
			delegate
			{
				RewardList = Dbf<RewardListDbfRecord>.Load("REWARD_LIST", format);
			},
			delegate
			{
				RewardTrack = Dbf<RewardTrackDbfRecord>.Load("REWARD_TRACK", format);
			},
			delegate
			{
				RewardTrackLevel = Dbf<RewardTrackLevelDbfRecord>.Load("REWARD_TRACK_LEVEL", format);
			},
			delegate
			{
				ScalingTreasureCardTag = Dbf<ScalingTreasureCardTagDbfRecord>.Load("SCALING_TREASURE_CARD_TAG", format);
			},
			delegate
			{
				Scenario = Dbf<ScenarioDbfRecord>.Load("SCENARIO", format);
			},
			delegate
			{
				ScenarioGuestHeroes = Dbf<ScenarioGuestHeroesDbfRecord>.Load("SCENARIO_GUEST_HEROES", format);
			},
			delegate
			{
				ScenarioOverride = Dbf<ScenarioOverrideDbfRecord>.Load("SCENARIO_OVERRIDE", format);
			},
			delegate
			{
				ScheduledCharacterDialog = Dbf<ScheduledCharacterDialogDbfRecord>.Load("SCHEDULED_CHARACTER_DIALOG", format);
			},
			delegate
			{
				ScoreLabel = Dbf<ScoreLabelDbfRecord>.Load("SCORE_LABEL", format);
			},
			delegate
			{
				SellableDeck = Dbf<SellableDeckDbfRecord>.Load("SELLABLE_DECK", format);
			},
			delegate
			{
				SetRotationEvent = Dbf<SetRotationEventDbfRecord>.Load("SET_ROTATION_EVENT", format);
			},
			delegate
			{
				ShopTier = Dbf<ShopTierDbfRecord>.Load("SHOP_TIER", format);
			},
			delegate
			{
				ShopTierProductSale = Dbf<ShopTierProductSaleDbfRecord>.Load("SHOP_TIER_PRODUCT_SALE", format);
			},
			delegate
			{
				SideboardCard = Dbf<SideboardCardDbfRecord>.Load("SIDEBOARD_CARD", format);
			},
			delegate
			{
				SignatureCard = Dbf<SignatureCardDbfRecord>.Load("SIGNATURE_CARD", format);
			},
			delegate
			{
				SignatureFrame = Dbf<SignatureFrameDbfRecord>.Load("SIGNATURE_FRAME", format);
			},
			delegate
			{
				SpecialEvent = Dbf<SpecialEventDbfRecord>.Load("SPECIAL_EVENT", format);
			},
			delegate
			{
				Subset = Dbf<SubsetDbfRecord>.Load("SUBSET", format);
			},
			delegate
			{
				SubsetCard = Dbf<SubsetCardDbfRecord>.Load("SUBSET_CARD", format);
			},
			delegate
			{
				SubsetRule = Dbf<SubsetRuleDbfRecord>.Load("SUBSET_RULE", format);
			},
			delegate
			{
				TaskList = Dbf<TaskListDbfRecord>.Load("TASK_LIST", format);
			},
			delegate
			{
				TavernBrawlTicket = Dbf<TavernBrawlTicketDbfRecord>.Load("TAVERN_BRAWL_TICKET", format);
			},
			delegate
			{
				TavernGuideQuest = Dbf<TavernGuideQuestDbfRecord>.Load("TAVERN_GUIDE_QUEST", format);
			},
			delegate
			{
				TavernGuideQuestRecommendedClasses = Dbf<TavernGuideQuestRecommendedClassesDbfRecord>.Load("TAVERN_GUIDE_QUEST_RECOMMENDED_CLASSES", format);
			},
			delegate
			{
				TavernGuideQuestSet = Dbf<TavernGuideQuestSetDbfRecord>.Load("TAVERN_GUIDE_QUEST_SET", format);
			},
			delegate
			{
				TestVariation = Dbf<TestVariationDbfRecord>.Load("TEST_VARIATION", format);
			},
			delegate
			{
				TierProperties = Dbf<TierPropertiesDbfRecord>.Load("TIER_PROPERTIES", format);
			},
			delegate
			{
				Trigger = Dbf<TriggerDbfRecord>.Load("TRIGGER", format);
			},
			delegate
			{
				UnlockedTavernGuideSet = Dbf<UnlockedTavernGuideSetDbfRecord>.Load("UNLOCKED_TAVERN_GUIDE_SET", format);
			},
			delegate
			{
				VisitorTask = Dbf<VisitorTaskDbfRecord>.Load("VISITOR_TASK", format);
			},
			delegate
			{
				VisitorTaskChain = Dbf<VisitorTaskChainDbfRecord>.Load("VISITOR_TASK_CHAIN", format);
			},
			delegate
			{
				Wing = Dbf<WingDbfRecord>.Load("WING", format);
			},
			delegate
			{
				XpOnPlacement = Dbf<XpOnPlacementDbfRecord>.Load("XP_ON_PLACEMENT", format);
			},
			delegate
			{
				XpOnPlacementGameTypeMultiplier = Dbf<XpOnPlacementGameTypeMultiplierDbfRecord>.Load("XP_ON_PLACEMENT_GAME_TYPE_MULTIPLIER", format);
			},
			delegate
			{
				XpPerTimeGameTypeMultiplier = Dbf<XpPerTimeGameTypeMultiplierDbfRecord>.Load("XP_PER_TIME_GAME_TYPE_MULTIPLIER", format);
			}
		};
	}

	private static JobResultCollection GetLoadDbfJobs(DbfFormat format)
	{
		return new JobResultCollection(Dbf<AccountLicenseDbfRecord>.CreateLoadAsyncJob("ACCOUNT_LICENSE", format, ref AccountLicense), Dbf<AchieveDbfRecord>.CreateLoadAsyncJob("ACHIEVE", format, ref Achieve), Dbf<AchieveConditionDbfRecord>.CreateLoadAsyncJob("ACHIEVE_CONDITION", format, ref AchieveCondition), Dbf<AchieveRegionDataDbfRecord>.CreateLoadAsyncJob("ACHIEVE_REGION_DATA", format, ref AchieveRegionData), Dbf<AchievementDbfRecord>.CreateLoadAsyncJob("ACHIEVEMENT", format, ref Achievement), Dbf<AchievementCategoryDbfRecord>.CreateLoadAsyncJob("ACHIEVEMENT_CATEGORY", format, ref AchievementCategory), Dbf<AchievementSectionDbfRecord>.CreateLoadAsyncJob("ACHIEVEMENT_SECTION", format, ref AchievementSection), Dbf<AchievementSectionItemDbfRecord>.CreateLoadAsyncJob("ACHIEVEMENT_SECTION_ITEM", format, ref AchievementSectionItem), Dbf<AchievementSubcategoryDbfRecord>.CreateLoadAsyncJob("ACHIEVEMENT_SUBCATEGORY", format, ref AchievementSubcategory), Dbf<AdventureDbfRecord>.CreateLoadAsyncJob("ADVENTURE", format, ref Adventure), Dbf<AdventureDataDbfRecord>.CreateLoadAsyncJob("ADVENTURE_DATA", format, ref AdventureData), Dbf<AdventureDeckDbfRecord>.CreateLoadAsyncJob("ADVENTURE_DECK", format, ref AdventureDeck), Dbf<AdventureGuestHeroesDbfRecord>.CreateLoadAsyncJob("ADVENTURE_GUEST_HEROES", format, ref AdventureGuestHeroes), Dbf<AdventureHeroPowerDbfRecord>.CreateLoadAsyncJob("ADVENTURE_HERO_POWER", format, ref AdventureHeroPower), Dbf<AdventureLoadoutTreasuresDbfRecord>.CreateLoadAsyncJob("ADVENTURE_LOADOUT_TREASURES", format, ref AdventureLoadoutTreasures), Dbf<AdventureMissionDbfRecord>.CreateLoadAsyncJob("ADVENTURE_MISSION", format, ref AdventureMission), Dbf<AdventureModeDbfRecord>.CreateLoadAsyncJob("ADVENTURE_MODE", format, ref AdventureMode), Dbf<BannerDbfRecord>.CreateLoadAsyncJob("BANNER", format, ref Banner), Dbf<BattlegroundsBoardSkinDbfRecord>.CreateLoadAsyncJob("BATTLEGROUNDS_BOARD_SKIN", format, ref BattlegroundsBoardSkin), Dbf<BattlegroundsEmoteDbfRecord>.CreateLoadAsyncJob("BATTLEGROUNDS_EMOTE", format, ref BattlegroundsEmote), Dbf<BattlegroundsFinisherDbfRecord>.CreateLoadAsyncJob("BATTLEGROUNDS_FINISHER", format, ref BattlegroundsFinisher), Dbf<BattlegroundsGuideSkinDbfRecord>.CreateLoadAsyncJob("BATTLEGROUNDS_GUIDE_SKIN", format, ref BattlegroundsGuideSkin), Dbf<BattlegroundsHeroSkinDbfRecord>.CreateLoadAsyncJob("BATTLEGROUNDS_HERO_SKIN", format, ref BattlegroundsHeroSkin), Dbf<BattlegroundsSeasonDbfRecord>.CreateLoadAsyncJob("BATTLEGROUNDS_SEASON", format, ref BattlegroundsSeason), Dbf<BoardDbfRecord>.CreateLoadAsyncJob("BOARD", format, ref Board), Dbf<BonusBountyDropChanceDbfRecord>.CreateLoadAsyncJob("BONUS_BOUNTY_DROP_CHANCE", format, ref BonusBountyDropChance), Dbf<BoosterDbfRecord>.CreateLoadAsyncJob("BOOSTER", format, ref Booster), Dbf<BoosterCardSetDbfRecord>.CreateLoadAsyncJob("BOOSTER_CARD_SET", format, ref BoosterCardSet), Dbf<BoxProductBannerDbfRecord>.CreateLoadAsyncJob("BOX_PRODUCT_BANNER", format, ref BoxProductBanner), Dbf<BuildingTierDbfRecord>.CreateLoadAsyncJob("BUILDING_TIER", format, ref BuildingTier), Dbf<CardDbfRecord>.CreateLoadAsyncJob("CARD", format, ref Card), Dbf<CardAdditonalSearchTermsDbfRecord>.CreateLoadAsyncJob("CARD_ADDITONAL_SEARCH_TERMS", format, ref CardAdditonalSearchTerms), Dbf<CardBackDbfRecord>.CreateLoadAsyncJob("CARD_BACK", format, ref CardBack), Dbf<CardChangeDbfRecord>.CreateLoadAsyncJob("CARD_CHANGE", format, ref CardChange), Dbf<CardDiscoverStringDbfRecord>.CreateLoadAsyncJob("CARD_DISCOVER_STRING", format, ref CardDiscoverString), Dbf<CardEquipmentAltTextDbfRecord>.CreateLoadAsyncJob("CARD_EQUIPMENT_ALT_TEXT", format, ref CardEquipmentAltText), Dbf<CardHeroDbfRecord>.CreateLoadAsyncJob("CARD_HERO", format, ref CardHero), Dbf<CardPlayerDeckOverrideDbfRecord>.CreateLoadAsyncJob("CARD_PLAYER_DECK_OVERRIDE", format, ref CardPlayerDeckOverride), Dbf<CardRaceDbfRecord>.CreateLoadAsyncJob("CARD_RACE", format, ref CardRace), Dbf<CardSetDbfRecord>.CreateLoadAsyncJob("CARD_SET", format, ref CardSet), Dbf<CardSetSpellOverrideDbfRecord>.CreateLoadAsyncJob("CARD_SET_SPELL_OVERRIDE", format, ref CardSetSpellOverride), Dbf<CardSetTimingDbfRecord>.CreateLoadAsyncJob("CARD_SET_TIMING", format, ref CardSetTiming), Dbf<CardTagDbfRecord>.CreateLoadAsyncJob("CARD_TAG", format, ref CardTag), Dbf<CardValueDbfRecord>.CreateLoadAsyncJob("CARD_VALUE", format, ref CardValue), Dbf<CatchupPackEventDbfRecord>.CreateLoadAsyncJob("CATCHUP_PACK_EVENT", format, ref CatchupPackEvent), Dbf<CharacterDbfRecord>.CreateLoadAsyncJob("CHARACTER", format, ref Character), Dbf<CharacterDialogDbfRecord>.CreateLoadAsyncJob("CHARACTER_DIALOG", format, ref CharacterDialog), Dbf<CharacterDialogItemsDbfRecord>.CreateLoadAsyncJob("CHARACTER_DIALOG_ITEMS", format, ref CharacterDialogItems), Dbf<ClassDbfRecord>.CreateLoadAsyncJob("CLASS", format, ref Class), Dbf<ClassExclusionsDbfRecord>.CreateLoadAsyncJob("CLASS_EXCLUSIONS", format, ref ClassExclusions), Dbf<ClientStringDbfRecord>.CreateLoadAsyncJob("CLIENT_STRING", format, ref ClientString), Dbf<CosmeticCoinDbfRecord>.CreateLoadAsyncJob("COSMETIC_COIN", format, ref CosmeticCoin), Dbf<CounterpartCardsDbfRecord>.CreateLoadAsyncJob("COUNTERPART_CARDS", format, ref CounterpartCards), Dbf<CreditsYearDbfRecord>.CreateLoadAsyncJob("CREDITS_YEAR", format, ref CreditsYear), Dbf<DeckDbfRecord>.CreateLoadAsyncJob("DECK", format, ref Deck), Dbf<DeckCardDbfRecord>.CreateLoadAsyncJob("DECK_CARD", format, ref DeckCard), Dbf<DeckRulesetDbfRecord>.CreateLoadAsyncJob("DECK_RULESET", format, ref DeckRuleset), Dbf<DeckRulesetRuleDbfRecord>.CreateLoadAsyncJob("DECK_RULESET_RULE", format, ref DeckRulesetRule), Dbf<DeckRulesetRuleSubsetDbfRecord>.CreateLoadAsyncJob("DECK_RULESET_RULE_SUBSET", format, ref DeckRulesetRuleSubset), Dbf<DeckTemplateDbfRecord>.CreateLoadAsyncJob("DECK_TEMPLATE", format, ref DeckTemplate), Dbf<DeckTemplateChoicesDbfRecord>.CreateLoadAsyncJob("DECK_TEMPLATE_CHOICES", format, ref DeckTemplateChoices), Dbf<DetailsVideoCueDbfRecord>.CreateLoadAsyncJob("DETAILS_VIDEO_CUE", format, ref DetailsVideoCue), Dbf<DkRuneListDbfRecord>.CreateLoadAsyncJob("DK_RUNE_LIST", format, ref DkRuneList), Dbf<DraftContentDbfRecord>.CreateLoadAsyncJob("DRAFT_CONTENT", format, ref DraftContent), Dbf<EventRewardTrackDbfRecord>.CreateLoadAsyncJob("EVENT_REWARD_TRACK", format, ref EventRewardTrack), Dbf<ExternalUrlDbfRecord>.CreateLoadAsyncJob("EXTERNAL_URL", format, ref ExternalUrl), Dbf<FixedRewardDbfRecord>.CreateLoadAsyncJob("FIXED_REWARD", format, ref FixedReward), Dbf<FixedRewardActionDbfRecord>.CreateLoadAsyncJob("FIXED_REWARD_ACTION", format, ref FixedRewardAction), Dbf<FixedRewardMapDbfRecord>.CreateLoadAsyncJob("FIXED_REWARD_MAP", format, ref FixedRewardMap), Dbf<FormulaDbfRecord>.CreateLoadAsyncJob("FORMULA", format, ref Formula), Dbf<FormulaChangePointDbfRecord>.CreateLoadAsyncJob("FORMULA_CHANGE_POINT", format, ref FormulaChangePoint), Dbf<GameModeDbfRecord>.CreateLoadAsyncJob("GAME_MODE", format, ref GameMode), Dbf<GameSaveSubkeyDbfRecord>.CreateLoadAsyncJob("GAME_SAVE_SUBKEY", format, ref GameSaveSubkey), Dbf<GlobalDbfRecord>.CreateLoadAsyncJob("GLOBAL", format, ref Global), Dbf<GuestHeroDbfRecord>.CreateLoadAsyncJob("GUEST_HERO", format, ref GuestHero), Dbf<GuestHeroSelectionRatioDbfRecord>.CreateLoadAsyncJob("GUEST_HERO_SELECTION_RATIO", format, ref GuestHeroSelectionRatio), Dbf<HiddenCardSetsDbfRecord>.CreateLoadAsyncJob("HIDDEN_CARD_SETS", format, ref HiddenCardSets), Dbf<HiddenLicenseDbfRecord>.CreateLoadAsyncJob("HIDDEN_LICENSE", format, ref HiddenLicense), Dbf<InitCardValueDbfRecord>.CreateLoadAsyncJob("INIT_CARD_VALUE", format, ref InitCardValue), Dbf<KeywordTextDbfRecord>.CreateLoadAsyncJob("KEYWORD_TEXT", format, ref KeywordText), Dbf<LeagueDbfRecord>.CreateLoadAsyncJob("LEAGUE", format, ref League), Dbf<LeagueBgPublicRatingEquivDbfRecord>.CreateLoadAsyncJob("LEAGUE_BG_PUBLIC_RATING_EQUIV", format, ref LeagueBgPublicRatingEquiv), Dbf<LeagueGameTypeDbfRecord>.CreateLoadAsyncJob("LEAGUE_GAME_TYPE", format, ref LeagueGameType), Dbf<LeagueRankDbfRecord>.CreateLoadAsyncJob("LEAGUE_RANK", format, ref LeagueRank), Dbf<LettuceAbilityDbfRecord>.CreateLoadAsyncJob("LETTUCE_ABILITY", format, ref LettuceAbility), Dbf<LettuceAbilityTierDbfRecord>.CreateLoadAsyncJob("LETTUCE_ABILITY_TIER", format, ref LettuceAbilityTier), Dbf<LettuceBountyDbfRecord>.CreateLoadAsyncJob("LETTUCE_BOUNTY", format, ref LettuceBounty), Dbf<LettuceBountyFinalRespresentiveRewardsDbfRecord>.CreateLoadAsyncJob("LETTUCE_BOUNTY_FINAL_RESPRESENTIVE_REWARDS", format, ref LettuceBountyFinalRespresentiveRewards), Dbf<LettuceBountyFinalRewardsDbfRecord>.CreateLoadAsyncJob("LETTUCE_BOUNTY_FINAL_REWARDS", format, ref LettuceBountyFinalRewards), Dbf<LettuceBountySetDbfRecord>.CreateLoadAsyncJob("LETTUCE_BOUNTY_SET", format, ref LettuceBountySet), Dbf<LettuceEquipmentDbfRecord>.CreateLoadAsyncJob("LETTUCE_EQUIPMENT", format, ref LettuceEquipment), Dbf<LettuceEquipmentModifierDataDbfRecord>.CreateLoadAsyncJob("LETTUCE_EQUIPMENT_MODIFIER_DATA", format, ref LettuceEquipmentModifierData), Dbf<LettuceEquipmentTierDbfRecord>.CreateLoadAsyncJob("LETTUCE_EQUIPMENT_TIER", format, ref LettuceEquipmentTier), Dbf<LettuceMapNodeTypeDbfRecord>.CreateLoadAsyncJob("LETTUCE_MAP_NODE_TYPE", format, ref LettuceMapNodeType), Dbf<LettuceMapNodeTypeAnomalyDbfRecord>.CreateLoadAsyncJob("LETTUCE_MAP_NODE_TYPE_ANOMALY", format, ref LettuceMapNodeTypeAnomaly), Dbf<LettuceMercenaryDbfRecord>.CreateLoadAsyncJob("LETTUCE_MERCENARY", format, ref LettuceMercenary), Dbf<LettuceMercenaryAbilityDbfRecord>.CreateLoadAsyncJob("LETTUCE_MERCENARY_ABILITY", format, ref LettuceMercenaryAbility), Dbf<LettuceMercenaryEquipmentDbfRecord>.CreateLoadAsyncJob("LETTUCE_MERCENARY_EQUIPMENT", format, ref LettuceMercenaryEquipment), Dbf<LettuceMercenaryLevelDbfRecord>.CreateLoadAsyncJob("LETTUCE_MERCENARY_LEVEL", format, ref LettuceMercenaryLevel), Dbf<LettuceMercenaryLevelStatsDbfRecord>.CreateLoadAsyncJob("LETTUCE_MERCENARY_LEVEL_STATS", format, ref LettuceMercenaryLevelStats), Dbf<LettuceMercenarySpecializationDbfRecord>.CreateLoadAsyncJob("LETTUCE_MERCENARY_SPECIALIZATION", format, ref LettuceMercenarySpecialization), Dbf<LettuceTreasureDbfRecord>.CreateLoadAsyncJob("LETTUCE_TREASURE", format, ref LettuceTreasure), Dbf<LettuceTutorialVoDbfRecord>.CreateLoadAsyncJob("LETTUCE_TUTORIAL_VO", format, ref LettuceTutorialVo), Dbf<LoginPopupSequenceDbfRecord>.CreateLoadAsyncJob("LOGIN_POPUP_SEQUENCE", format, ref LoginPopupSequence), Dbf<LoginPopupSequencePopupDbfRecord>.CreateLoadAsyncJob("LOGIN_POPUP_SEQUENCE_POPUP", format, ref LoginPopupSequencePopup), Dbf<LoginRewardDbfRecord>.CreateLoadAsyncJob("LOGIN_REWARD", format, ref LoginReward), Dbf<LuckyDrawBoxDbfRecord>.CreateLoadAsyncJob("LUCKY_DRAW_BOX", format, ref LuckyDrawBox), Dbf<LuckyDrawRewardsDbfRecord>.CreateLoadAsyncJob("LUCKY_DRAW_REWARDS", format, ref LuckyDrawRewards), Dbf<MercTriggeredEventDbfRecord>.CreateLoadAsyncJob("MERC_TRIGGERED_EVENT", format, ref MercTriggeredEvent), Dbf<MercTriggeringEventDbfRecord>.CreateLoadAsyncJob("MERC_TRIGGERING_EVENT", format, ref MercTriggeringEvent), Dbf<MercenariesRandomRewardDbfRecord>.CreateLoadAsyncJob("MERCENARIES_RANDOM_REWARD", format, ref MercenariesRandomReward), Dbf<MercenariesRankedSeasonRewardRankDbfRecord>.CreateLoadAsyncJob("MERCENARIES_RANKED_SEASON_REWARD_RANK", format, ref MercenariesRankedSeasonRewardRank), Dbf<MercenaryAllowedTreasureDbfRecord>.CreateLoadAsyncJob("MERCENARY_ALLOWED_TREASURE", format, ref MercenaryAllowedTreasure), Dbf<MercenaryArtVariationDbfRecord>.CreateLoadAsyncJob("MERCENARY_ART_VARIATION", format, ref MercenaryArtVariation), Dbf<MercenaryArtVariationPremiumDbfRecord>.CreateLoadAsyncJob("MERCENARY_ART_VARIATION_PREMIUM", format, ref MercenaryArtVariationPremium), Dbf<MercenaryBuildingDbfRecord>.CreateLoadAsyncJob("MERCENARY_BUILDING", format, ref MercenaryBuilding), Dbf<MercenaryVillageTriggerDbfRecord>.CreateLoadAsyncJob("MERCENARY_VILLAGE_TRIGGER", format, ref MercenaryVillageTrigger), Dbf<MercenaryVisitorDbfRecord>.CreateLoadAsyncJob("MERCENARY_VISITOR", format, ref MercenaryVisitor), Dbf<MiniSetDbfRecord>.CreateLoadAsyncJob("MINI_SET", format, ref MiniSet), Dbf<ModifiedLettuceAbilityCardTagDbfRecord>.CreateLoadAsyncJob("MODIFIED_LETTUCE_ABILITY_CARD_TAG", format, ref ModifiedLettuceAbilityCardTag), Dbf<ModifiedLettuceAbilityValueDbfRecord>.CreateLoadAsyncJob("MODIFIED_LETTUCE_ABILITY_VALUE", format, ref ModifiedLettuceAbilityValue), Dbf<MythicAbilityScalingCardTagDbfRecord>.CreateLoadAsyncJob("MYTHIC_ABILITY_SCALING_CARD_TAG", format, ref MythicAbilityScalingCardTag), Dbf<MythicEquipmentScalingCardTagDbfRecord>.CreateLoadAsyncJob("MYTHIC_EQUIPMENT_SCALING_CARD_TAG", format, ref MythicEquipmentScalingCardTag), Dbf<MythicEquipmentScalingDestinationCardTagDbfRecord>.CreateLoadAsyncJob("MYTHIC_EQUIPMENT_SCALING_DESTINATION_CARD_TAG", format, ref MythicEquipmentScalingDestinationCardTag), Dbf<NextTiersDbfRecord>.CreateLoadAsyncJob("NEXT_TIERS", format, ref NextTiers), Dbf<OverrideQuestPoolIdListDbfRecord>.CreateLoadAsyncJob("OVERRIDE_QUEST_POOL_ID_LIST", format, ref OverrideQuestPoolIdList), Dbf<OwnershipReqListDbfRecord>.CreateLoadAsyncJob("OWNERSHIP_REQ_LIST", format, ref OwnershipReqList), Dbf<PaidPremiumTierDbfRecord>.CreateLoadAsyncJob("PAID_PREMIUM_TIER", format, ref PaidPremiumTier), Dbf<PlayerExperimentDbfRecord>.CreateLoadAsyncJob("PLAYER_EXPERIMENT", format, ref PlayerExperiment), Dbf<PowerDefinitionDbfRecord>.CreateLoadAsyncJob("POWER_DEFINITION", format, ref PowerDefinition), Dbf<ProductDbfRecord>.CreateLoadAsyncJob("PRODUCT", format, ref Product), Dbf<ProductClientDataDbfRecord>.CreateLoadAsyncJob("PRODUCT_CLIENT_DATA", format, ref ProductClientData), Dbf<PvpdrSeasonDbfRecord>.CreateLoadAsyncJob("PVPDR_SEASON", format, ref PvpdrSeason), Dbf<QuestDbfRecord>.CreateLoadAsyncJob("QUEST", format, ref Quest), Dbf<QuestChangeDbfRecord>.CreateLoadAsyncJob("QUEST_CHANGE", format, ref QuestChange), Dbf<QuestDialogDbfRecord>.CreateLoadAsyncJob("QUEST_DIALOG", format, ref QuestDialog), Dbf<QuestDialogOnCompleteDbfRecord>.CreateLoadAsyncJob("QUEST_DIALOG_ON_COMPLETE", format, ref QuestDialogOnComplete), Dbf<QuestDialogOnProgress1DbfRecord>.CreateLoadAsyncJob("QUEST_DIALOG_ON_PROGRESS1", format, ref QuestDialogOnProgress1), Dbf<QuestDialogOnProgress2DbfRecord>.CreateLoadAsyncJob("QUEST_DIALOG_ON_PROGRESS2", format, ref QuestDialogOnProgress2), Dbf<QuestDialogOnReceivedDbfRecord>.CreateLoadAsyncJob("QUEST_DIALOG_ON_RECEIVED", format, ref QuestDialogOnReceived), Dbf<QuestModifierDbfRecord>.CreateLoadAsyncJob("QUEST_MODIFIER", format, ref QuestModifier), Dbf<QuestPoolDbfRecord>.CreateLoadAsyncJob("QUEST_POOL", format, ref QuestPool), Dbf<RankedPlaySeasonDbfRecord>.CreateLoadAsyncJob("RANKED_PLAY_SEASON", format, ref RankedPlaySeason), Dbf<RegionOverridesDbfRecord>.CreateLoadAsyncJob("REGION_OVERRIDES", format, ref RegionOverrides), Dbf<RelatedCardsDbfRecord>.CreateLoadAsyncJob("RELATED_CARDS", format, ref RelatedCards), Dbf<RepeatableTaskListDbfRecord>.CreateLoadAsyncJob("REPEATABLE_TASK_LIST", format, ref RepeatableTaskList), Dbf<ReplacementsWhenPlayedDbfRecord>.CreateLoadAsyncJob("REPLACEMENTS_WHEN_PLAYED", format, ref ReplacementsWhenPlayed), Dbf<RewardBagDbfRecord>.CreateLoadAsyncJob("REWARD_BAG", format, ref RewardBag), Dbf<RewardChestDbfRecord>.CreateLoadAsyncJob("REWARD_CHEST", format, ref RewardChest), Dbf<RewardChestContentsDbfRecord>.CreateLoadAsyncJob("REWARD_CHEST_CONTENTS", format, ref RewardChestContents), Dbf<RewardItemDbfRecord>.CreateLoadAsyncJob("REWARD_ITEM", format, ref RewardItem), Dbf<RewardListDbfRecord>.CreateLoadAsyncJob("REWARD_LIST", format, ref RewardList), Dbf<RewardTrackDbfRecord>.CreateLoadAsyncJob("REWARD_TRACK", format, ref RewardTrack), Dbf<RewardTrackLevelDbfRecord>.CreateLoadAsyncJob("REWARD_TRACK_LEVEL", format, ref RewardTrackLevel), Dbf<ScalingTreasureCardTagDbfRecord>.CreateLoadAsyncJob("SCALING_TREASURE_CARD_TAG", format, ref ScalingTreasureCardTag), Dbf<ScenarioDbfRecord>.CreateLoadAsyncJob("SCENARIO", format, ref Scenario), Dbf<ScenarioGuestHeroesDbfRecord>.CreateLoadAsyncJob("SCENARIO_GUEST_HEROES", format, ref ScenarioGuestHeroes), Dbf<ScenarioOverrideDbfRecord>.CreateLoadAsyncJob("SCENARIO_OVERRIDE", format, ref ScenarioOverride), Dbf<ScheduledCharacterDialogDbfRecord>.CreateLoadAsyncJob("SCHEDULED_CHARACTER_DIALOG", format, ref ScheduledCharacterDialog), Dbf<ScoreLabelDbfRecord>.CreateLoadAsyncJob("SCORE_LABEL", format, ref ScoreLabel), Dbf<SellableDeckDbfRecord>.CreateLoadAsyncJob("SELLABLE_DECK", format, ref SellableDeck), Dbf<SetRotationEventDbfRecord>.CreateLoadAsyncJob("SET_ROTATION_EVENT", format, ref SetRotationEvent), Dbf<ShopTierDbfRecord>.CreateLoadAsyncJob("SHOP_TIER", format, ref ShopTier), Dbf<ShopTierProductSaleDbfRecord>.CreateLoadAsyncJob("SHOP_TIER_PRODUCT_SALE", format, ref ShopTierProductSale), Dbf<SideboardCardDbfRecord>.CreateLoadAsyncJob("SIDEBOARD_CARD", format, ref SideboardCard), Dbf<SignatureCardDbfRecord>.CreateLoadAsyncJob("SIGNATURE_CARD", format, ref SignatureCard), Dbf<SignatureFrameDbfRecord>.CreateLoadAsyncJob("SIGNATURE_FRAME", format, ref SignatureFrame), Dbf<SpecialEventDbfRecord>.CreateLoadAsyncJob("SPECIAL_EVENT", format, ref SpecialEvent), Dbf<SubsetDbfRecord>.CreateLoadAsyncJob("SUBSET", format, ref Subset), Dbf<SubsetCardDbfRecord>.CreateLoadAsyncJob("SUBSET_CARD", format, ref SubsetCard), Dbf<SubsetRuleDbfRecord>.CreateLoadAsyncJob("SUBSET_RULE", format, ref SubsetRule), Dbf<TaskListDbfRecord>.CreateLoadAsyncJob("TASK_LIST", format, ref TaskList), Dbf<TavernBrawlTicketDbfRecord>.CreateLoadAsyncJob("TAVERN_BRAWL_TICKET", format, ref TavernBrawlTicket), Dbf<TavernGuideQuestDbfRecord>.CreateLoadAsyncJob("TAVERN_GUIDE_QUEST", format, ref TavernGuideQuest), Dbf<TavernGuideQuestRecommendedClassesDbfRecord>.CreateLoadAsyncJob("TAVERN_GUIDE_QUEST_RECOMMENDED_CLASSES", format, ref TavernGuideQuestRecommendedClasses), Dbf<TavernGuideQuestSetDbfRecord>.CreateLoadAsyncJob("TAVERN_GUIDE_QUEST_SET", format, ref TavernGuideQuestSet), Dbf<TestVariationDbfRecord>.CreateLoadAsyncJob("TEST_VARIATION", format, ref TestVariation), Dbf<TierPropertiesDbfRecord>.CreateLoadAsyncJob("TIER_PROPERTIES", format, ref TierProperties), Dbf<TriggerDbfRecord>.CreateLoadAsyncJob("TRIGGER", format, ref Trigger), Dbf<UnlockedTavernGuideSetDbfRecord>.CreateLoadAsyncJob("UNLOCKED_TAVERN_GUIDE_SET", format, ref UnlockedTavernGuideSet), Dbf<VisitorTaskDbfRecord>.CreateLoadAsyncJob("VISITOR_TASK", format, ref VisitorTask), Dbf<VisitorTaskChainDbfRecord>.CreateLoadAsyncJob("VISITOR_TASK_CHAIN", format, ref VisitorTaskChain), Dbf<WingDbfRecord>.CreateLoadAsyncJob("WING", format, ref Wing), Dbf<XpOnPlacementDbfRecord>.CreateLoadAsyncJob("XP_ON_PLACEMENT", format, ref XpOnPlacement), Dbf<XpOnPlacementGameTypeMultiplierDbfRecord>.CreateLoadAsyncJob("XP_ON_PLACEMENT_GAME_TYPE_MULTIPLIER", format, ref XpOnPlacementGameTypeMultiplier), Dbf<XpPerTimeGameTypeMultiplierDbfRecord>.CreateLoadAsyncJob("XP_PER_TIME_GAME_TYPE_MULTIPLIER", format, ref XpPerTimeGameTypeMultiplier));
	}

	public IEnumerator<IAsyncJobResult> Initialize(ServiceLocator serviceLocator)
	{
		yield return CreateLoadDbfJob();
	}

	public Type[] GetDependencies()
	{
		return null;
	}

	public void Shutdown()
	{
	}

	public static GameDbfIndex GetIndex()
	{
		if (s_index == null)
		{
			s_index = new GameDbfIndex();
		}
		return s_index;
	}

	public static bool ShouldForceXmlLoading()
	{
		if (HearthstoneApplication.IsPublic())
		{
			return false;
		}
		if (HearthstoneApplication.UsingStandaloneLocalData())
		{
			return true;
		}
		object defaultXmlLoadingObj = Options.Get().GetOption(Option.DBF_XML_LOADING);
		if (defaultXmlLoadingObj != null)
		{
			return (bool)defaultXmlLoadingObj;
		}
		return Application.isEditor;
	}

	public static JobDefinition CreateLoadDbfJob()
	{
		return new JobDefinition("GameDbf.Load", Load());
	}

	public static void LoadXml()
	{
		IEnumerator loadingCoroutine = Load(useXmlLoading: true, useAssetJobs: false);
		while (loadingCoroutine.MoveNext())
		{
		}
	}

	public static IEnumerator<IAsyncJobResult> Load()
	{
		return Load(useXmlLoading: false, useAssetJobs: true);
	}

	public static IEnumerator<IAsyncJobResult> Load(bool useXmlLoading)
	{
		return Load(useXmlLoading, useAssetJobs: true);
	}

	public static IEnumerator<IAsyncJobResult> Load(bool useXmlLoading, bool useAssetJobs)
	{
		if (HearthstoneApplication.IsHearthstoneRunning)
		{
			yield return new WaitForDbfBundleReady();
		}
		if (s_index == null)
		{
			s_index = new GameDbfIndex();
		}
		else
		{
			s_index.Initialize();
		}
		if (ShouldForceXmlLoading())
		{
			useXmlLoading = true;
		}
		DbfFormat format = (useXmlLoading ? DbfFormat.XML : DbfFormat.ASSET);
		DbfShared.Initialize();
		DbfShared.Reset();
		if (!useXmlLoading)
		{
			if (useAssetJobs)
			{
				yield return new JobDefinition("GameDbf.LoadDBFSharedAssetBundle", DbfShared.Job_LoadSharedDBFAssetBundle());
			}
			else
			{
				DbfShared.LoadSharedAssetBundle();
			}
		}
		Log.Dbf?.Print("Loading DBFS with format={0}", format);
		CPUTimeSoftYield softYielder = new CPUTimeSoftYield(1f / (float)Application.targetFrameRate * 0.8f);
		Action[] loadActions;
		int i;
		if (!useAssetJobs)
		{
			loadActions = GetLoadDbfActions(format);
			Action[] array = loadActions;
			for (i = 0; i < array.Length; i++)
			{
				array[i]();
				if (softYielder.ShouldSoftYield())
				{
					yield return null;
					softYielder.NewFrame();
				}
			}
		}
		else
		{
			yield return GetLoadDbfJobs(format);
		}
		loadActions = GetPostProcessDbfActions();
		i = 0;
		while (i < loadActions.Length)
		{
			loadActions[i]();
			if (softYielder.ShouldSoftYield())
			{
				yield return null;
				softYielder.NewFrame();
			}
			int num = i + 1;
			i = num;
		}
		IsLoaded = true;
		SetDbfCallbacksForIndexing();
	}

	private static Action[] GetPostProcessDbfActions()
	{
		return new Action[28]
		{
			delegate
			{
				s_index.PostProcessDbfLoad_MercenaryEquipmentUnlock();
			},
			delegate
			{
				s_index.PostProcessDbfLoad_MercenaryArtVariationUnlock();
			},
			delegate
			{
				s_index.PostProcessDbfLoad_CardTag(CardTag);
			},
			delegate
			{
				s_index.PostProcessDbfLoad_Card(Card);
			},
			delegate
			{
				s_index.PostProcessDbfLoad_CardDiscoverString(CardDiscoverString);
			},
			delegate
			{
				s_index.PostProcessDbfLoad_CardSetSpellOverride(CardSetSpellOverride);
			},
			delegate
			{
				s_index.PostProcessDbfLoad_DeckRulesetRule(DeckRulesetRule);
			},
			delegate
			{
				s_index.PostProcessDbfLoad_DeckRulesetRuleSubset(DeckRulesetRuleSubset);
			},
			delegate
			{
				s_index.PostProcessDbfLoad_FixedRewardAction(FixedRewardAction);
			},
			delegate
			{
				s_index.PostProcessDbfLoad_FixedRewardMap(FixedRewardMap);
			},
			delegate
			{
				s_index.PostProcessDbfLoad_FixedReward(FixedReward);
			},
			delegate
			{
				s_index.PostProcessDbfLoad_SubsetCard(SubsetCard);
			},
			delegate
			{
				s_index.PostProcessDbfLoad_CardPlayerDeckOverride(CardPlayerDeckOverride);
			},
			delegate
			{
				RankMgr.Get().PostProcessDbfLoad_League();
			},
			delegate
			{
				s_index.PostProcessDbfLoad_LettuceEquipmentTier();
			},
			delegate
			{
				s_index.PostProcessDbfLoad_VisitorTask();
			},
			delegate
			{
				s_index.PostProcessDbfLoad_Achievement();
			},
			delegate
			{
				s_index.PostProcessDbfLoad_MercenaryLevel();
			},
			delegate
			{
				s_index.PostProcessDbfLoad_MercenaryArtVariation();
			},
			delegate
			{
				s_index.PostProcessDbfLoad_RewardTrackLevel(RewardTrackLevel);
			},
			delegate
			{
				s_index.PostProcessDbfLoad_RewardItems(RewardItem);
			},
			delegate
			{
				s_index.PostProcessDbfLoad_CardSetTiming(CardSetTiming);
			},
			delegate
			{
				s_index.PostProcessDbfLoad_CardHero(CardHero);
			},
			delegate
			{
				s_index.PostProcessDbfLoad_LettuceAbilityTier();
			},
			delegate
			{
				s_index.PostProcessDbfLoad_MercenarySpecializations();
			},
			delegate
			{
				s_index.PostProcessDbfLoad_MercenaryAbilities();
			},
			delegate
			{
				s_index.PostProcessDbfLoad_MercenaryEquipment();
			},
			delegate
			{
				s_index.PostProcessDbfLoad_MercenaryTreasure();
			}
		};
	}

	private static void SetDbfCallbacksForIndexing()
	{
		CardTag.AddListeners(s_index.OnCardTagAdded, s_index.OnCardTagRemoved);
		Card.AddListeners(s_index.OnCardAdded, s_index.OnCardRemoved);
		CardDiscoverString.AddListeners(s_index.OnCardDiscoverStringAdded, s_index.OnCardDiscoverStringRemoved);
		CardSetSpellOverride.AddListeners(s_index.OnCardSetSpellOverrideAdded, s_index.OnCardSetSpellOverrideRemoved);
		CardPlayerDeckOverride.AddListeners(s_index.OnCardPlayerDeckOverrideAdded, s_index.OnCardPlayerDeckOverrideRemoved);
		DeckRulesetRule.AddListeners(s_index.OnDeckRulesetRuleAdded, s_index.OnDeckRulesetRuleRemoved);
		DeckRulesetRuleSubset.AddListeners(s_index.OnDeckRulesetRuleSubsetAdded, s_index.OnDeckRulesetRuleSubsetRemoved);
		FixedRewardAction.AddListeners(s_index.OnFixedRewardActionAdded, s_index.OnFixedRewardActionRemoved);
		FixedRewardMap.AddListeners(s_index.OnFixedRewardMapAdded, s_index.OnFixedRewardMapRemoved);
		SubsetCard.AddListeners(s_index.OnSubsetCardAdded, s_index.OnSubsetCardRemoved);
		LettuceEquipmentTier.AddListeners(s_index.OnLettuceEquipmentTierAdded, s_index.OnLettuceEquipmentTierRemoved);
		LettuceMercenaryLevel.AddListeners(s_index.OnMercenaryLevelAdded, s_index.OnMercenaryLevelRemoved);
		RewardTrackLevel.AddListeners(s_index.OnRewardTrackLevelAdded, s_index.OnRewardTrackLevelRemoved);
		RewardItem.AddListeners(s_index.OnRewardItemAdded, s_index.OnRewardItemRemoved);
		CardSetTiming.AddListeners(s_index.OnCardSetTimingAdded, s_index.OnCardSetTimingRemoved);
		CardHero.AddListeners(s_index.OnCardHeroAdded, s_index.OnCardHeroRemoved);
		LettuceAbilityTier.AddListeners(s_index.OnLettuceAbilityTierAdded, s_index.OnLettuceAbilityTierRemoved);
		MercenaryArtVariation.AddListeners(s_index.OnMercenaryArtVariationAdded, s_index.OnMercenaryArtVariationRemoved);
		LettuceMercenarySpecialization.AddListeners(s_index.OnMercenarySpecializationAdded, s_index.OnMercenarySpecializationRemoved);
		LettuceMercenaryAbility.AddListeners(s_index.OnMercenaryAbilityAdded, s_index.OnMercenaryAbilityRemoved);
		LettuceMercenaryEquipment.AddListeners(s_index.OnMercenaryEquipmentAdded, s_index.OnMercenaryEquipmentRemoved);
		MercenaryAllowedTreasure.AddListeners(s_index.OnMercenaryTreasureAdded, s_index.OnMercenaryTreasureRemoved);
	}

	public static void Reload(string name, string xml)
	{
		if (!(name == "ACHIEVE"))
		{
			if (name == "CARD_BACK")
			{
				CardBack = Dbf<CardBackDbfRecord>.Load(name, DbfFormat.XML);
				if (ServiceManager.TryGet<CardBackManager>(out var cardBackManager))
				{
					cardBackManager.InitCardBackData();
				}
			}
			else
			{
				Error.AddDevFatal("Reloading {0} is unsupported", name);
			}
		}
		else
		{
			Achieve = Dbf<AchieveDbfRecord>.Load(name, DbfFormat.XML);
			if (ServiceManager.TryGet<AchieveManager>(out var achieveManager))
			{
				achieveManager.InitAchieveManager();
			}
		}
	}

	public static int GetDataVersion()
	{
		return GetDOPAsset().DataVersion;
	}

	private static DOPAsset GetDOPAsset()
	{
		if (s_DOPAsset != null)
		{
			return s_DOPAsset;
		}
		if (Application.isEditor)
		{
			s_DOPAsset = DOPAsset.GenerateDOPAsset();
		}
		else
		{
			AssetBundle assetBundle = DbfShared.GetAssetBundle();
			if (assetBundle != null)
			{
				s_DOPAsset = assetBundle.LoadAsset<DOPAsset>("Assets/Game/DBF-Asset/DOPAsset.asset");
			}
			if (s_DOPAsset == null)
			{
				Log.Dbf.PrintWarning("Failed to load DOP asset, generating default...");
				s_DOPAsset = DOPAsset.GenerateDOPAsset();
			}
		}
		return s_DOPAsset;
	}

	public static void RegisterDbf(IDbf dbf)
	{
		s_allDbfs[dbf.GetName()] = dbf;
	}

	public static IDbf GetIDbf(string name)
	{
		s_allDbfs.TryGetValue(name, out var value);
		return value;
	}
}
