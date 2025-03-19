using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Assets;
using Blizzard.GameService.SDK.Client.Integration;
using Blizzard.T5.Core;
using Blizzard.T5.Core.Utils;
using Blizzard.T5.Jobs;
using Blizzard.T5.Services;
using Blizzard.Telemetry.WTCG.Client;
using BobNetProto;
using Hearthstone;
using Hearthstone.BreakingNews;
using Hearthstone.Core;
using Hearthstone.Streaming;
using PegasusLettuce;
using PegasusShared;
using PegasusUtil;
using UnityEngine;

public class NetCache : IService, IHasUpdate
{
	public delegate void DelNewNoticesListener(List<ProfileNotice> newNotices, bool isInitialNoticeList);

	public delegate void DelGoldBalanceListener(NetCacheGoldBalance balance);

	public delegate void DelRenownBalanceListener(NetCacheRenownBalance balance);

	public delegate void DelBattlegroundsTokenBalanceListener(NetCacheBattlegroundsTokenBalance balance);

	public delegate void DelFavoriteCardBackChangedListener(int newFavoriteCardBackID, bool isFavorite);

	public delegate void DelFavoriteBattlegroundsHeroSkinChangedListener(int baseHeroCardId, int battlegroundsHeroSkinId);

	public delegate void DelFavoriteBattlegroundsGuideSkinChangedListener(BattlegroundsGuideSkinId? newFavoriteBattlegroundsGuideSkinID);

	public delegate void DelFavoriteBattlegroundsBoardSkinChangedListener(BattlegroundsBoardSkinId? newFavoriteBattlegroundsBoardSkinID);

	public delegate void DelFavoriteBattlegroundsFinisherChangedListener(BattlegroundsFinisherId? newFavoriteBattlegroundsFinisherID);

	public delegate void DelBattlegroundsEmoteLoadoutChangedListener(Hearthstone.BattlegroundsEmoteLoadout newLoadout);

	public delegate void DelFavoriteCoinChangedListener(int newFavoriteCoinID, bool isFavorite);

	public delegate void DelOwnedBattlegroundsSkinsChanged();

	public class NetCacheGamesPlayed
	{
		public int GamesStarted { get; set; }

		public int GamesWon { get; set; }

		public int GamesLost { get; set; }
	}

	public class NetCacheFeatures
	{
		public class CacheMisc
		{
			public int ClientOptionsUpdateIntervalSeconds { get; set; }

			public bool AllowLiveFPSGathering { get; set; }
		}

		public class CacheGames
		{
			public enum FeatureFlags
			{
				Invalid,
				Tournament,
				Practice,
				Casual,
				Forge,
				Friendly,
				TavernBrawl,
				Battlegrounds,
				BattlegroundsFriendlyChallenge,
				BattlegroundsTutorial,
				Duels,
				PaidDuels,
				Mercenaries,
				MercenariesAI,
				MercenariesCoOp,
				MercenariesFriendly,
				MercenariesMythic,
				BattlegroundsDuos
			}

			public bool Tournament { get; set; }

			public bool Practice { get; set; }

			public bool Casual { get; set; }

			public bool Forge { get; set; }

			public bool Friendly { get; set; }

			public bool TavernBrawl { get; set; }

			public bool Battlegrounds { get; set; }

			public bool BattlegroundsFriendlyChallenge { get; set; }

			public bool BattlegroundsTutorial { get; set; }

			public bool BattlegroundsDuos { get; set; }

			public int ShowUserUI { get; set; }

			public bool Duels { get; set; }

			public bool PaidDuels { get; set; }

			public bool Mercenaries { get; set; }

			public bool MercenariesAI { get; set; }

			public bool MercenariesCoOp { get; set; }

			public bool MercenariesFriendly { get; set; }

			public bool MercenariesMythic { get; set; }

			public bool GetFeatureFlag(FeatureFlags flag)
			{
				return flag switch
				{
					FeatureFlags.Tournament => Tournament, 
					FeatureFlags.Practice => Practice, 
					FeatureFlags.Casual => Casual, 
					FeatureFlags.Forge => Forge, 
					FeatureFlags.Friendly => Friendly, 
					FeatureFlags.TavernBrawl => TavernBrawl, 
					FeatureFlags.Battlegrounds => Battlegrounds, 
					FeatureFlags.BattlegroundsFriendlyChallenge => BattlegroundsFriendlyChallenge, 
					FeatureFlags.BattlegroundsTutorial => BattlegroundsTutorial, 
					FeatureFlags.BattlegroundsDuos => BattlegroundsDuos, 
					FeatureFlags.Duels => Duels, 
					FeatureFlags.PaidDuels => PaidDuels, 
					FeatureFlags.Mercenaries => Mercenaries, 
					FeatureFlags.MercenariesAI => MercenariesAI, 
					FeatureFlags.MercenariesCoOp => MercenariesCoOp, 
					FeatureFlags.MercenariesFriendly => MercenariesFriendly, 
					FeatureFlags.MercenariesMythic => MercenariesMythic, 
					_ => false, 
				};
			}
		}

		public class CacheCollection
		{
			[CompilerGenerated]
			private bool _003CCrafting_003Ek__BackingField;

			public bool Manager { get; set; }

			public bool Crafting
			{
				[CompilerGenerated]
				set
				{
					_003CCrafting_003Ek__BackingField = value;
				}
			}

			public bool DeckReordering { get; set; }

			public bool MultipleFavoriteCardBacks { get; set; }

			public bool CosmeticsRenderingEnabled { get; set; }
		}

		public class CacheStore
		{
			[CompilerGenerated]
			private int _003CNumClassicPacksUntilDeprioritize_003Ek__BackingField;

			public bool Store { get; set; }

			public bool BattlePay { get; set; }

			public bool BuyWithGold { get; set; }

			public bool SimpleCheckout { get; set; }

			public bool SoftAccountPurchasing { get; set; }

			public bool VirtualCurrencyEnabled { get; set; }

			public int NumClassicPacksUntilDeprioritize
			{
				[CompilerGenerated]
				set
				{
					_003CNumClassicPacksUntilDeprioritize_003Ek__BackingField = value;
				}
			}

			public bool SimpleCheckoutIOS { get; set; }

			public bool SimpleCheckoutAndroidAmazon { get; set; }

			public bool SimpleCheckoutAndroidGoogle { get; set; }

			public bool SimpleCheckoutAndroidGlobal { get; set; }

			public bool SimpleCheckoutWin { get; set; }

			public bool SimpleCheckoutMac { get; set; }

			public int BoosterRotatingSoonWarnDaysWithoutSale { get; set; }

			public int BoosterRotatingSoonWarnDaysWithSale { get; set; }

			public bool BuyCardBacksFromCollectionManager { get; set; }

			public bool BuyHeroSkinsFromCollectionManager { get; set; }

			public bool LargeItemBundleDetailsEnabled { get; set; }
		}

		public class CacheHeroes
		{
			[CompilerGenerated]
			private bool _003CHunter_003Ek__BackingField;

			[CompilerGenerated]
			private bool _003CMage_003Ek__BackingField;

			[CompilerGenerated]
			private bool _003CPaladin_003Ek__BackingField;

			[CompilerGenerated]
			private bool _003CPriest_003Ek__BackingField;

			[CompilerGenerated]
			private bool _003CRogue_003Ek__BackingField;

			[CompilerGenerated]
			private bool _003CShaman_003Ek__BackingField;

			[CompilerGenerated]
			private bool _003CWarlock_003Ek__BackingField;

			[CompilerGenerated]
			private bool _003CWarrior_003Ek__BackingField;

			public bool Hunter
			{
				[CompilerGenerated]
				set
				{
					_003CHunter_003Ek__BackingField = value;
				}
			}

			public bool Mage
			{
				[CompilerGenerated]
				set
				{
					_003CMage_003Ek__BackingField = value;
				}
			}

			public bool Paladin
			{
				[CompilerGenerated]
				set
				{
					_003CPaladin_003Ek__BackingField = value;
				}
			}

			public bool Priest
			{
				[CompilerGenerated]
				set
				{
					_003CPriest_003Ek__BackingField = value;
				}
			}

			public bool Rogue
			{
				[CompilerGenerated]
				set
				{
					_003CRogue_003Ek__BackingField = value;
				}
			}

			public bool Shaman
			{
				[CompilerGenerated]
				set
				{
					_003CShaman_003Ek__BackingField = value;
				}
			}

			public bool Warlock
			{
				[CompilerGenerated]
				set
				{
					_003CWarlock_003Ek__BackingField = value;
				}
			}

			public bool Warrior
			{
				[CompilerGenerated]
				set
				{
					_003CWarrior_003Ek__BackingField = value;
				}
			}
		}

		public class CacheMercenaries
		{
			public int FullyUpgradedStatBoostAttack { get; set; }

			public int FullyUpgradedStatBoostHealth { get; set; }

			public float AttackBoostPerMythicLevel { get; set; }

			public float HealthBoostPerMythicLevel { get; set; }

			public int MythicAbilityRenownScaleAssetId { get; set; }

			public int MythicEquipmentRenownScaleAssetId { get; set; }

			public int MythicTreasureRenownScaleAssetId { get; set; }
		}

		public class CacheTraceroute
		{
			public int MaxHops { get; set; }

			public int MessageSize { get; set; }

			public int MaxRetries { get; set; }

			public int TimeoutMs { get; set; }

			public bool ResolveHost { get; set; }
		}

		public class CacheReturningPlayer
		{
			public bool LoginCountNoticeSupressionEnabled { get; set; }

			public int NoticeSuppressionLoginThreshold { get; set; }
		}

		public class Defaults
		{
			public static readonly float TutorialPreviewVideosTimeout = 7f;
		}

		public bool CaisEnabledNonMobile;

		public bool CaisEnabledMobileChina;

		public bool CaisEnabledMobileSouthKorea;

		public bool SendTelemetryPresence;

		[CompilerGenerated]
		private float _003CSpecialEventTimingMod_003Ek__BackingField;

		[CompilerGenerated]
		private uint _003CPVPDRClosedToNewSessionsSeconds_003Ek__BackingField;

		[CompilerGenerated]
		private int _003CBattlegroundsEarlyAccessLicense_003Ek__BackingField;

		[CompilerGenerated]
		private uint _003CDuelsEarlyAccessLicense_003Ek__BackingField;

		public CacheMisc Misc { get; set; }

		public CacheGames Games { get; set; }

		public CacheCollection Collection { get; set; }

		public CacheStore Store { get; set; }

		public CacheHeroes Heroes { get; set; }

		public CacheMercenaries Mercenaries { get; set; }

		public CacheTraceroute Traceroute { get; set; }

		public CacheReturningPlayer ReturningPlayer { get; set; }

		public int XPSoloLimit { get; set; }

		public int MaxHeroLevel { get; set; }

		public float SpecialEventTimingMod
		{
			[CompilerGenerated]
			set
			{
				_003CSpecialEventTimingMod_003Ek__BackingField = value;
			}
		}

		public int FriendWeekConcederMaxDefense { get; set; }

		public int FriendWeekConcededGameMinTotalTurns { get; set; }

		public bool FriendWeekAllowsTavernBrawlRecordUpdate { get; set; }

		public uint ArenaClosedToNewSessionsSeconds { get; set; }

		public uint PVPDRClosedToNewSessionsSeconds
		{
			[CompilerGenerated]
			set
			{
				_003CPVPDRClosedToNewSessionsSeconds_003Ek__BackingField = value;
			}
		}

		public bool QuickOpenEnabled { get; set; }

		public bool ForceIosLowRes { get; set; }

		public bool EnableSmartDeckCompletion { get; set; }

		public bool AllowOfflineClientActivity { get; set; }

		public bool AllowOfflineClientDeckDeletion { get; set; }

		public int BattlegroundsEarlyAccessLicense
		{
			[CompilerGenerated]
			set
			{
				_003CBattlegroundsEarlyAccessLicense_003Ek__BackingField = value;
			}
		}

		public int BattlegroundsMaxRankedPartySize { get; set; }

		public bool JournalButtonDisabled { get; set; }

		public bool AchievementToastDisabled { get; set; }

		public uint DuelsEarlyAccessLicense
		{
			[CompilerGenerated]
			set
			{
				_003CDuelsEarlyAccessLicense_003Ek__BackingField = value;
			}
		}

		public bool ContentstackEnabled { get; set; }

		public bool PersonalizedMessagesEnabled { get; set; }

		public bool AppRatingEnabled { get; set; }

		public float AppRatingSamplingPercentage { get; set; }

		public List<int> DuelsCardDenylist { get; set; }

		public List<int> ConstructedCardDenylist { get; set; }

		public List<int> SideboardCardDenylist { get; set; }

		public List<int> TwistCardDenylist { get; set; }

		public List<int> StandardCardDenylist { get; set; }

		public List<int> WildCardDenylist { get; set; }

		public List<int> TavernBrawlCardDenylist { get; set; }

		public List<int> TwistDeckTemplateDenylist { get; set; }

		public List<string> VFXDenylist { get; set; }

		public int TwistSeasonOverride { get; set; }

		public int TwistScenarioOverride { get; set; }

		public Dictionary<CardDefinition, int> TwistHeroicDeckHeroHealthOverrides { get; set; }

		public bool RankedPlayEnableScenarioOverrides { get; set; }

		public bool BattlegroundsSkinsEnabled { get; set; }

		public bool BattlegroundsBoardSkinsEnabled { get; set; }

		public bool BattlegroundsFinishersEnabled { get; set; }

		public bool BattlegroundsEmotesEnabled { get; set; }

		public bool BattlegroundsRewardTrackEnabled { get; set; }

		public bool TutorialPreviewVideosEnabled { get; set; }

		public float TutorialPreviewVideosTimeout { get; set; }

		public bool MercenariesEnableVillages { get; set; }

		public bool MercenariesPackOpeningEnabled { get; set; }

		public int MercenariesTeamMaxSize { get; set; }

		public int MinHPForProgressAfterConcede { get; set; }

		public int MinTurnsForProgressAfterConcede { get; set; }

		public float BGDuosLeaverRatingPenalty { get; set; }

		public int BGMinTurnsForProgressAfterConcede { get; set; }

		public bool EnablePlayingFromMiniHand { get; set; }

		public bool BattlegroundsMedalFriendListDisplayEnabled { get; set; }

		public bool EnableUpgradeToGolden { get; set; }

		public bool ShouldPrevalidatePastedDeckCodes { get; set; }

		public bool EnableClickToFixDeck { get; set; }

		public bool RecentFriendListDisplayEnabled { get; set; }

		public bool OvercappedDecksEnabled { get; set; }

		public bool ReportPlayerEnabled { get; set; }

		public bool LuckyDrawEnabled { get; set; }

		public bool ContinuousQuickOpenEnabled { get; set; }

		public bool LegacyCardValueCacheEnabled { get; set; }

		public bool BattlenetBillingFlowDisableOverride { get; set; }

		public string BattlegroundsLuckyDrawDisabledCountryCode { get; set; }

		public bool SkippableTutorialEnabled { get; set; }

		public bool EnableNDERerollSpecialCases { get; set; }

		public bool ShopButtonOnPackOpeningScreenEnabled { get; set; }

		public bool MassPackOpeningEnabled { get; set; }

		public int MassPackOpeningPackLimit { get; set; }

		public int MassPackOpeningGoldenPackLimit { get; set; }

		public int MassPackOpeningHooverChunkSize { get; set; }

		public bool MassCatchupPackOpeningEnabled { get; set; }

		public int MassCatchupPackOpeningPackLimit { get; set; }

		public bool CancelMatchmakingDuringLoanerDeckGrant { get; set; }

		public bool AllowBGInviteWhileInNPPG { get; set; }

		public int CardValueOverrideServerRegion { get; set; }

		public bool ArenaRedraftOnLossEnabled { get; set; }

		public int CallSDKInterval { get; set; }

		public bool EnableAllOptionsIDCheck { get; set; }

		public bool BoxProductBannerEnabled { get; set; }

		public bool TracerouteEnabled { get; set; }

		public NetCacheFeatures()
		{
			Misc = new CacheMisc();
			Games = new CacheGames();
			Collection = new CacheCollection();
			Store = new CacheStore();
			Heroes = new CacheHeroes();
			Mercenaries = new CacheMercenaries();
			Traceroute = new CacheTraceroute();
			ReturningPlayer = new CacheReturningPlayer();
		}
	}

	public class NetCacheArcaneDustBalance
	{
		public long Balance { get; set; }
	}

	public class NetCacheGoldBalance
	{
		public long CappedBalance { get; set; }

		public long BonusBalance { get; set; }

		public long GetTotal()
		{
			return CappedBalance + BonusBalance;
		}
	}

	public class NetCacheRenownBalance
	{
		public long Balance { get; set; }
	}

	public class NetCacheBattlegroundsTokenBalance
	{
		public long Balance { get; set; }
	}

	public class NetPlayerArenaTickets
	{
		public int Balance { get; set; }
	}

	public class HeroLevel
	{
		public class LevelInfo
		{
			public int Level { get; set; }

			public int MaxLevel { get; set; }

			public long XP { get; set; }

			public long MaxXP { get; set; }

			public LevelInfo()
			{
				Level = 0;
				MaxLevel = 60;
				XP = 0L;
				MaxXP = 0L;
			}

			public bool IsMaxLevel()
			{
				return Level == MaxLevel;
			}

			public override string ToString()
			{
				return $"[LevelInfo: Level={Level}, XP={XP}, MaxXP={MaxXP}]";
			}
		}

		public TAG_CLASS Class { get; set; }

		public LevelInfo PrevLevel { get; set; }

		public LevelInfo CurrentLevel { get; set; }

		public HeroLevel()
		{
			Class = TAG_CLASS.INVALID;
			PrevLevel = null;
			CurrentLevel = new LevelInfo();
		}

		public override string ToString()
		{
			return $"[HeroLevel: Class={Class}, PrevLevel={PrevLevel}, CurrentLevel={CurrentLevel}]";
		}
	}

	public class NetCacheHeroLevels
	{
		public List<HeroLevel> Levels { get; set; }

		public NetCacheHeroLevels()
		{
			Levels = new List<HeroLevel>();
		}

		public override string ToString()
		{
			string str = "[START NetCacheHeroLevels]\n";
			foreach (HeroLevel level in Levels)
			{
				str += $"{level}\n";
			}
			return str + "[END NetCacheHeroLevels]";
		}
	}

	public class NetCacheProfileProgress
	{
		public TutorialProgress CampaignProgress { get; set; }

		public int BestForgeWins { get; set; }

		public long LastForgeDate { get; set; }

		public bool TutorialComplete { get; set; }
	}

	public class NetCacheDisplayBanner
	{
		public int Id { get; set; }
	}

	public class NetCacheCardBacks
	{
		public HashSet<int> FavoriteCardBacks { get; set; }

		public HashSet<int> CardBacks { get; set; }

		public NetCacheCardBacks()
		{
			FavoriteCardBacks = new HashSet<int>();
			CardBacks = new HashSet<int>();
		}
	}

	public class NetCacheCoins
	{
		public HashSet<int> FavoriteCoins { get; set; }

		public HashSet<int> Coins { get; set; }

		public NetCacheCoins()
		{
			Coins = new HashSet<int>();
			FavoriteCoins = new HashSet<int>();
		}
	}

	public class BoosterStack
	{
		public int Id { get; set; }

		public int Count { get; set; }

		public int EverGrantedCount { get; set; }
	}

	public class NetCacheBoosters
	{
		public List<BoosterStack> BoosterStacks { get; set; }

		public NetCacheBoosters()
		{
			BoosterStacks = new List<BoosterStack>();
		}

		public BoosterStack GetBoosterStack(int id)
		{
			return BoosterStacks.Find((BoosterStack obj) => obj.Id == id);
		}

		public int GetTotalNumBoosters()
		{
			int count = 0;
			foreach (BoosterStack boosterStack in BoosterStacks)
			{
				count += boosterStack.Count;
			}
			return count;
		}
	}

	public class DeckHeader
	{
		public long ID { get; set; }

		public string Name { get; set; }

		public int? CardBack { get; set; }

		public int? CosmeticCoin { get; set; }

		public bool RandomCoinUseFavorite { get; set; }

		public string Hero { get; set; }

		public string UIHeroOverride { get; set; }

		public TAG_PREMIUM UIHeroOverridePremium { get; set; }

		public string HeroPower { get; set; }

		public DeckType Type { get; set; }

		public bool HeroOverridden { get; set; }

		public bool RandomHeroUseFavorite { get; set; }

		public int SeasonId { get; set; }

		public int BrawlLibraryItemId { get; set; }

		public bool NeedsName { get; set; }

		public long SortOrder { get; set; }

		public RuneType Rune1 { get; set; }

		public RuneType Rune2 { get; set; }

		public RuneType Rune3 { get; set; }

		public PegasusShared.FormatType FormatType { get; set; }

		public bool Locked { get; set; }

		public DeckSourceType SourceType { get; set; }

		public DateTime? CreateDate { get; set; }

		public DateTime? LastModified { get; set; }

		public override string ToString()
		{
			return $"[DeckHeader: ID={ID} Name={Name} Hero={Hero} HeroPower={HeroPower} DeckType={Type} " + $"CardBack={CardBack} CosmeticCoin={CosmeticCoin} RandomCoinUseFavorite={RandomCoinUseFavorite} " + $"HeroOverridden={HeroOverridden} RandomHeroUseFavorite={RandomHeroUseFavorite} " + $"NeedsName={NeedsName} SortOrder={SortOrder} SourceType={SourceType} Rune1={Rune1} Rune2={Rune2} Rune3={Rune3}";
		}
	}

	public class NetCacheDecks
	{
		public List<DeckHeader> Decks { get; set; }

		public NetCacheDecks()
		{
			Decks = new List<DeckHeader>();
		}
	}

	public class CardDefinition
	{
		public string Name { get; set; }

		public TAG_PREMIUM Premium { get; set; }

		public override bool Equals(object obj)
		{
			if (obj is CardDefinition other && Premium == other.Premium)
			{
				return Name.Equals(other.Name);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return (int)(Name.GetHashCode() + Premium);
		}

		public override string ToString()
		{
			return $"[CardDefinition: Name={Name}, Premium={Premium}]";
		}
	}

	public class CardValue
	{
		public int BaseBuyValue { get; set; }

		public int BaseSellValue { get; set; }

		public int BaseUpgradeValue { get; set; }

		public int BuyValueOverride { get; set; }

		public int SellValueOverride { get; set; }

		public EventTimingType OverrideEvent { get; set; }

		public int GetBuyValue()
		{
			if (!IsOverrideActive() || BuyValueOverride <= 0)
			{
				return BaseBuyValue;
			}
			return BuyValueOverride;
		}

		public int GetSellValue()
		{
			if (!IsOverrideActive() || SellValueOverride <= 0)
			{
				return BaseSellValue;
			}
			return SellValueOverride;
		}

		public int GetUpgradeValue()
		{
			return BaseUpgradeValue;
		}

		public bool IsOverrideActive()
		{
			return EventTimingManager.Get().IsEventActive(OverrideEvent);
		}
	}

	public class NetCacheCardValues
	{
		public Dictionary<CardDefinition, CardValue> Values { get; set; }

		public NetCacheCardValues()
		{
			Values = new Dictionary<CardDefinition, CardValue>();
		}

		public NetCacheCardValues(int initialSize)
		{
			Values = new Dictionary<CardDefinition, CardValue>(initialSize);
		}
	}

	public class NetCacheDisconnectedGame
	{
		public GameServerInfo ServerInfo { get; set; }

		public PegasusShared.GameType GameType { get; set; }

		public PegasusShared.FormatType FormatType { get; set; }

		public bool LoadGameState { get; set; }
	}

	public class BoosterCard
	{
		[CompilerGenerated]
		private long _003CDate_003Ek__BackingField;

		public CardDefinition Def { get; set; }

		public long Date
		{
			[CompilerGenerated]
			set
			{
				_003CDate_003Ek__BackingField = value;
			}
		}

		public BoosterCard()
		{
			Def = new CardDefinition();
		}
	}

	public class CardStack
	{
		public CardDefinition Def { get; set; }

		public long Date { get; set; }

		public int Count { get; set; }

		public int NumSeen { get; set; }

		public CardStack()
		{
			Def = new CardDefinition();
		}
	}

	public class NetCacheCollection
	{
		public int TotalCardsOwned;

		public Map<TAG_CLASS, HashSet<string>> CoreCardsUnlockedPerClass = new Map<TAG_CLASS, HashSet<string>>();

		public List<CardStack> Stacks { get; set; }

		public NetCacheCollection()
		{
			Stacks = new List<CardStack>();
			foreach (TAG_CLASS classType in Enum.GetValues(typeof(TAG_CLASS)))
			{
				CoreCardsUnlockedPerClass[classType] = new HashSet<string>();
			}
		}
	}

	public class PlayerRecord
	{
		[CompilerGenerated]
		private int _003CTies_003Ek__BackingField;

		public PegasusShared.GameType RecordType { get; set; }

		public int Data { get; set; }

		public int Wins { get; set; }

		public int Losses { get; set; }

		public int Ties
		{
			[CompilerGenerated]
			set
			{
				_003CTies_003Ek__BackingField = value;
			}
		}
	}

	public class NetCachePlayerRecords
	{
		public List<PlayerRecord> Records { get; set; }

		public NetCachePlayerRecords()
		{
			Records = new List<PlayerRecord>();
		}
	}

	public class NetCacheRewardProgress
	{
		public int Season { get; set; }

		public long SeasonEndDate { get; set; }

		public long NextQuestCancelDate { get; set; }
	}

	public class NetCacheMedalInfo
	{
		public Map<PegasusShared.FormatType, MedalInfoData> MedalData = new Map<PegasusShared.FormatType, MedalInfoData>();

		private static Map<PegasusShared.FormatType, int> m_cheatLocalOverrideStarLevelData = new Map<PegasusShared.FormatType, int>();

		private static Map<PegasusShared.FormatType, int> m_cheatLocalOverrideLegendRankData = new Map<PegasusShared.FormatType, int>();

		public NetCacheMedalInfo PreviousMedalInfo { get; set; }

		public NetCacheMedalInfo()
		{
		}

		public NetCacheMedalInfo(MedalInfo packet)
		{
			foreach (MedalInfoData medalInfoData in packet.MedalData)
			{
				MedalData.Add(medalInfoData.FormatType, medalInfoData);
			}
			foreach (KeyValuePair<PegasusShared.FormatType, int> starLevelOverride in m_cheatLocalOverrideStarLevelData)
			{
				MedalData[starLevelOverride.Key].StarLevel = starLevelOverride.Value;
			}
			foreach (KeyValuePair<PegasusShared.FormatType, int> legendRankOverride in m_cheatLocalOverrideLegendRankData)
			{
				MedalData[legendRankOverride.Key].LegendRank = legendRankOverride.Value;
			}
		}

		public NetCacheMedalInfo Clone()
		{
			NetCacheMedalInfo clone = new NetCacheMedalInfo();
			foreach (KeyValuePair<PegasusShared.FormatType, MedalInfoData> kvp in MedalData)
			{
				clone.MedalData.Add(kvp.Key, CloneMedalInfoData(kvp.Value));
			}
			return clone;
		}

		public MedalInfoData GetMedalInfoData(PegasusShared.FormatType formatType)
		{
			if (!MedalData.TryGetValue(formatType, out var result))
			{
				Debug.LogError("NetCacheMedalInfo.GetMedalInfoData failed to find data for the format type " + formatType.ToString() + ". Returning null");
			}
			return result;
		}

		public void CheatLocalOverrideStarLevel(PegasusShared.FormatType formatType, int starLevel)
		{
			m_cheatLocalOverrideStarLevelData[formatType] = starLevel;
			MedalData[formatType].StarLevel = starLevel;
		}

		public void CheatLocalOverrideLegendRank(PegasusShared.FormatType formatType, int legendRank)
		{
			m_cheatLocalOverrideLegendRankData[formatType] = legendRank;
			MedalData[formatType].LegendRank = legendRank;
		}

		public static void CheatLocalOverrideClear()
		{
			m_cheatLocalOverrideStarLevelData.Clear();
			m_cheatLocalOverrideLegendRankData.Clear();
		}

		public static MedalInfoData CloneMedalInfoData(MedalInfoData original)
		{
			MedalInfoData clone = new MedalInfoData();
			clone.LeagueId = original.LeagueId;
			clone.SeasonWins = original.SeasonWins;
			clone.Stars = original.Stars;
			clone.Streak = original.Streak;
			clone.StarLevel = original.StarLevel;
			clone.HasLegendRank = original.HasLegendRank;
			clone.LegendRank = original.LegendRank;
			clone.HasBestStarLevel = original.HasBestStarLevel;
			clone.BestStarLevel = original.BestStarLevel;
			clone.HasSeasonGames = original.HasSeasonGames;
			clone.SeasonGames = original.SeasonGames;
			clone.StarsPerWin = original.StarsPerWin;
			if (original.HasRatingId)
			{
				clone.RatingId = original.RatingId;
			}
			if (original.HasSeasonId)
			{
				clone.SeasonId = original.SeasonId;
			}
			if (original.HasRating)
			{
				clone.Rating = original.Rating;
			}
			if (original.HasVariance)
			{
				clone.Variance = original.Variance;
			}
			if (original.HasBestStars)
			{
				clone.BestStars = original.BestStars;
			}
			if (original.HasBestEverLeagueId)
			{
				clone.BestEverLeagueId = original.BestEverLeagueId;
			}
			if (original.HasBestEverStarLevel)
			{
				clone.BestEverStarLevel = original.BestEverStarLevel;
			}
			if (original.HasBestRating)
			{
				clone.BestRating = original.BestRating;
			}
			if (original.HasPublicRating)
			{
				clone.PublicRating = original.PublicRating;
			}
			if (original.HasFormatType)
			{
				clone.FormatType = original.FormatType;
			}
			return clone;
		}

		public override string ToString()
		{
			return $"[NetCacheMedalInfo] \n MedalData={MedalData.ToString()}";
		}
	}

	public class NetCacheBaconRatingInfo
	{
		public int Rating { get; set; }

		public int DuosRating { get; set; }

		public override string ToString()
		{
			return string.Format("[NetCacheBaconRatingInfo] \n Rating={0} \n DuosRating={0}", Rating, DuosRating);
		}
	}

	public class NetCachePVPDRStatsInfo
	{
		public int Rating { get; set; }

		public int PaidRating { get; set; }

		public int HighWatermark { get; set; }

		public override string ToString()
		{
			return $"[NetCachePVPDRStatsInfo] \n Rating={Rating} PaidRating={PaidRating} HighWatermark={HighWatermark}";
		}
	}

	public class NetCacheMercenariesPlayerInfo
	{
		public class BountyInfo
		{
			public int FewestTurns { get; set; }

			public int Completions { get; set; }

			public bool IsComplete { get; set; }

			public bool IsAcknowledged { get; set; }

			public List<uint> BossCardIds { get; set; }

			public int MaxMythicLevel { get; set; }

			public DateTime? UnlockTime { get; set; }

			public BountyInfo Clone()
			{
				BountyInfo clone = new BountyInfo();
				clone.FewestTurns = FewestTurns;
				clone.Completions = Completions;
				clone.IsComplete = IsComplete;
				clone.IsAcknowledged = IsAcknowledged;
				if (BossCardIds != null)
				{
					clone.BossCardIds = new List<uint>();
					clone.BossCardIds.AddRange(BossCardIds);
				}
				clone.MaxMythicLevel = MaxMythicLevel;
				clone.UnlockTime = UnlockTime;
				return clone;
			}
		}

		public Dictionary<MercenaryBuilding.Mercenarybuildingtype, bool> BuildingEnabledMap;

		public List<int> DisabledMercenaryList;

		public HashSet<int> DisabledVisitorList;

		public List<int> DisabledBuildingTierUpgradeList;

		public int PvpRating { get; set; }

		public uint PvpRewardChestWinsProgress { get; set; }

		public uint PvpRewardChestWinsRequired { get; set; }

		public Dictionary<int, BountyInfo> BountyInfoMap { get; set; }

		public int PvpSeasonHighestRating { get; set; }

		public int PvpSeasonId { get; set; }

		public int CurrentMythicBountyLevel { get; set; }

		public int MinMythicBountyLevel { get; set; }

		public int MaxMythicBountyLevel { get; set; }

		public DateTime GeneratedBountyResetTime { get; set; }

		public override string ToString()
		{
			return $"[NetCacheMercenariesPlayerInfo] \n PvpRating={PvpRating}, PvpRewardChestWinsProgress={PvpRewardChestWinsProgress}, PvpRewardChestWinsRequired={PvpRewardChestWinsRequired}";
		}
	}

	public class NetCacheMercenariesVillageInfo
	{
		[CompilerGenerated]
		private List<MercenariesBuildingState> _003CLastBuildingUpdate_003Ek__BackingField;

		private readonly List<int> m_emptyTierList = new List<int>();

		private Dictionary<int, List<int>> m_tierTreeCache = new Dictionary<int, List<int>>();

		private Dictionary<int, int> m_unbuiltTierLookup = new Dictionary<int, int>();

		private Dictionary<TAG_RARITY, int> m_renownConversionLookup = new Dictionary<TAG_RARITY, int>();

		public bool Initialized { get; set; }

		public List<MercenariesBuildingState> BuildingStates { get; set; }

		public List<MercenariesBuildingState> LastBuildingUpdate
		{
			[CompilerGenerated]
			set
			{
				_003CLastBuildingUpdate_003Ek__BackingField = value;
			}
		}

		public List<MercenariesRenownConvertRate> ConversionRates { get; private set; }

		public int UnlockedBountyDifficultyLevel { get; private set; }

		public void TrySetDifficultyUnlock(MercenariesBuildingState bldgState)
		{
			if (GameDbf.MercenaryBuilding.GetRecord(bldgState.BuildingId).MercenaryBuildingType != MercenaryBuilding.Mercenarybuildingtype.PVEZONES)
			{
				return;
			}
			foreach (TierPropertiesDbfRecord prop in GameDbf.BuildingTier.GetRecord(bldgState.CurrentTierId).MercenaryBuildingTierProperties)
			{
				if (prop.TierPropertyType == TierProperties.Buildingtierproperty.PVEMODE)
				{
					UnlockedBountyDifficultyLevel = prop.TierPropertyValue;
					break;
				}
			}
		}

		public List<int> GetNextTierListByTierId(int tierId)
		{
			if (m_tierTreeCache.TryGetValue(tierId, out var tierList))
			{
				return tierList;
			}
			return m_emptyTierList;
		}

		public bool BuildingIsBuilt(MercenariesBuildingState bldgState)
		{
			if (m_unbuiltTierLookup.TryGetValue(bldgState.BuildingId, out var unbuiltTierId))
			{
				return bldgState.CurrentTierId != unbuiltTierId;
			}
			return false;
		}

		public void CacheTierTree()
		{
			if (m_tierTreeCache.Count > 0)
			{
				m_tierTreeCache.Clear();
			}
			foreach (MercenaryBuildingDbfRecord bldg in GameDbf.MercenaryBuilding.GetRecords())
			{
				BuildingTierDbfRecord tier = GameDbf.BuildingTier.GetRecord((BuildingTierDbfRecord r) => r.MercenaryBuildingId == bldg.ID);
				m_unbuiltTierLookup.Add(bldg.ID, tier.ID);
				AddTierToTierTreeCache(bldg.DefaultTier);
			}
		}

		private void AddTierToTierTreeCache(int tierId)
		{
			if (m_tierTreeCache.ContainsKey(tierId))
			{
				return;
			}
			List<int> tierIdList = new List<int>();
			m_tierTreeCache.Add(tierId, tierIdList);
			List<NextTiersDbfRecord> nextTierList = GameDbf.NextTiers.GetRecords((NextTiersDbfRecord r) => r.BuildingTierId == tierId);
			if (nextTierList == null || nextTierList.Count == 0)
			{
				return;
			}
			foreach (NextTiersDbfRecord nextTier in nextTierList)
			{
				tierIdList.Add(nextTier.NextTierId);
				AddTierToTierTreeCache(nextTier.NextTierId);
			}
		}

		public void CacheRenownConversionRates(List<MercenariesRenownConvertRate> conversionRates)
		{
			ConversionRates = conversionRates;
			m_renownConversionLookup.Clear();
			if (ConversionRates == null || ConversionRates.Count == 0)
			{
				return;
			}
			foreach (MercenariesRenownConvertRate conversionRate in conversionRates)
			{
				TAG_RARITY rarity = (TAG_RARITY)conversionRate.CoinRarityId;
				if (m_renownConversionLookup.ContainsKey(rarity))
				{
					Log.Lettuce.PrintError($"Duplicate rarity {rarity} in renown conversion rates - Skipping value");
				}
				else if (conversionRate.CoinConversionRate != 0)
				{
					m_renownConversionLookup[rarity] = (int)conversionRate.CoinConversionRate;
				}
			}
		}

		public bool TryGetRenownRate(TAG_RARITY rarity, out int conversionRate)
		{
			return m_renownConversionLookup.TryGetValue(rarity, out conversionRate);
		}
	}

	public class NetCacheMercenariesVillageVisitorInfo
	{
		public List<MercenariesVisitorState> VisitorStates { get; set; }

		public int[] VisitingMercenaries { get; set; }

		public List<MercenariesTaskState> CompletedTasks { get; set; }

		public List<MercenariesCompletedVisitorState> CompletedVisitorStates { get; set; }

		public List<MercenariesRenownOfferData> ActiveRenownOffers { get; set; }
	}

	public class NetCacheMercenariesMythicTreasureInfo
	{
		public class MythicTreasureScalar
		{
			public int TreasureId { get; set; }

			public int Scalar { get; set; }
		}

		public Dictionary<int, MythicTreasureScalar> MythicTreasureScalarMap;
	}

	public abstract class ProfileNotice
	{
		public enum NoticeType
		{
			GAINED_MEDAL = 1,
			REWARD_BOOSTER = 2,
			REWARD_CARD = 3,
			DISCONNECTED_GAME = 4,
			PRECON_DECK = 5,
			REWARD_DUST = 6,
			REWARD_MOUNT = 7,
			REWARD_FORGE = 8,
			REWARD_CURRENCY = 9,
			PURCHASE = 10,
			REWARD_CARD_BACK = 11,
			BONUS_STARS = 12,
			ADVENTURE_PROGRESS = 14,
			HERO_LEVEL_UP = 15,
			ACCOUNT_LICENSE = 16,
			TAVERN_BRAWL_REWARDS = 17,
			TAVERN_BRAWL_TICKET = 18,
			EVENT = 19,
			GENERIC_REWARD_CHEST = 20,
			LEAGUE_PROMOTION_REWARDS = 21,
			CARD_REPLACEMENT = 22,
			DISCONNECTED_GAME_NEW = 23,
			DECK_REMOVED = 25,
			DECK_GRANTED = 26,
			MINI_SET_GRANTED = 27,
			SELLABLE_DECK_GRANTED = 28,
			REWARD_BATTLEGROUNDS_GUIDE = 29,
			REWARD_BATTLEGROUNDS_HERO = 30,
			MERCENARIES_REWARDS_CURRENCY = 31,
			MERCENARIES_REWARDS_EXPERIENCE = 32,
			MERCENARIES_REWARDS_EQUIPMENT = 33,
			MERCENARIES_REWARDS = 34,
			MERCENARIES_ABILITY_UNLOCK = 35,
			MERCENARIES_MERC_FULL_UPGRADE = 36,
			MERCENARIES_MERC_LICENSE = 37,
			MERCENARIES_CURRENCY_LICENSE = 38,
			MERCENARIES_BOOSTER_LICENSE = 39,
			MERCENARIES_RANDOM_REWARD_LICENSE = 40,
			MERCENARIES_SEASON_ROLL = 41,
			MERCENARIES_SEASON_REWARDS = 42,
			MERCENARIES_ZONE_UNLOCK = 43,
			REWARD_BATTLEGROUNDS_BOARD_SKIN = 44,
			REWARD_BATTLEGROUNDS_FINISHER = 45,
			REWARD_BATTLEGROUNDS_EMOTE = 46,
			REWARD_LUCKY_DRAW = 47,
			REDUNDANT_NDE_REROLL = 48,
			REDUNDANT_NDE_REROLL_RESULT = 49
		}

		public enum NoticeOrigin
		{
			UNKNOWN = -1,
			SEASON = 1,
			BETA_REIMBURSE = 2,
			FORGE = 3,
			TOURNEY = 4,
			PRECON_DECK = 5,
			ACK = 6,
			ACHIEVEMENT = 7,
			LEVEL_UP = 8,
			PURCHASE_COMPLETE = 10,
			PURCHASE_FAILED = 11,
			PURCHASE_CANCELED = 12,
			BLIZZCON = 13,
			EVENT = 14,
			DISCONNECTED_GAME = 15,
			OUT_OF_BAND_LICENSE = 16,
			IGR = 17,
			ADVENTURE_PROGRESS = 18,
			ADVENTURE_FLAGS = 19,
			TAVERN_BRAWL_REWARD = 20,
			ACCOUNT_LICENSE_FLAGS = 21,
			FROM_PURCHASE = 22,
			HOF_COMPENSATION = 23,
			GENERIC_REWARD_CHEST_ACHIEVE = 24,
			GENERIC_REWARD_CHEST = 25,
			LEAGUE_PROMOTION = 26,
			CARD_REPLACEMENT = 27,
			NOTICE_ORIGIN_LEVEL_UP_MULTIPLE = 28,
			NOTICE_ORIGIN_DUELS = 29,
			NOTICE_ORIGIN_MERCENARIES = 30,
			NOTICE_ORIGIN_LUCKY_DRAW = 31,
			NOTICE_ORIGIN_NDE_REDUNDANT_REROLL = 32
		}

		private NoticeType m_type;

		public long NoticeID { get; set; }

		public NoticeType Type => m_type;

		public NoticeOrigin Origin { get; set; }

		public long OriginData { get; set; }

		public long Date { get; set; }

		protected ProfileNotice(NoticeType init)
		{
			m_type = init;
			NoticeID = 0L;
			Origin = NoticeOrigin.UNKNOWN;
			OriginData = 0L;
			Date = 0L;
		}

		public override string ToString()
		{
			return $"[{GetType()}: NoticeID={NoticeID}, Type={Type}, Origin={Origin}, OriginData={OriginData}, Date={Date}]";
		}
	}

	public class ProfileNoticeMedal : ProfileNotice
	{
		public int LeagueId { get; set; }

		public int StarLevel { get; set; }

		public int LegendRank { get; set; }

		public int BestStarLevel { get; set; }

		public PegasusShared.FormatType FormatType { get; set; }

		public Network.RewardChest Chest { get; set; }

		public bool WasLimitedByBestEverStarLevel { get; set; }

		public ProfileNoticeMedal()
			: base(NoticeType.GAINED_MEDAL)
		{
		}

		public override string ToString()
		{
			return $"{base.ToString()} [LeagueId={LeagueId} StarLevel={StarLevel}, LegendRank={LegendRank}, BestStarLevel={BestStarLevel}, FormatType={FormatType}, Chest={Chest}, WasLimitedByBestEverStarLevel={WasLimitedByBestEverStarLevel}]";
		}
	}

	public class ProfileNoticeRewardBooster : ProfileNotice
	{
		public int Id { get; set; }

		public int Count { get; set; }

		public ProfileNoticeRewardBooster()
			: base(NoticeType.REWARD_BOOSTER)
		{
			Id = 0;
			Count = 0;
		}

		public override string ToString()
		{
			return $"{base.ToString()} [Id={Id}, Count={Count}]";
		}
	}

	public class ProfileNoticeRewardCard : ProfileNotice
	{
		public string CardID { get; set; }

		public TAG_PREMIUM Premium { get; set; }

		public int Quantity { get; set; }

		public ProfileNoticeRewardCard()
			: base(NoticeType.REWARD_CARD)
		{
		}

		public override string ToString()
		{
			return $"{base.ToString()} [CardID={CardID}, Premium={Premium}, Quantity={Quantity}]";
		}
	}

	public class ProfileNoticeRewardBattlegroundsGuideSkin : ProfileNotice
	{
		public string CardID { get; set; }

		public int FixedRewardMapID { get; set; }

		public ProfileNoticeRewardBattlegroundsGuideSkin()
			: base(NoticeType.REWARD_BATTLEGROUNDS_GUIDE)
		{
		}

		public override string ToString()
		{
			return string.Format("{0}", base.ToString(), CardID);
		}
	}

	public class ProfileNoticeRewardBattlegroundsHeroSkin : ProfileNotice
	{
		public string CardID { get; set; }

		public int FixedRewardMapID { get; set; }

		public ProfileNoticeRewardBattlegroundsHeroSkin()
			: base(NoticeType.REWARD_BATTLEGROUNDS_HERO)
		{
		}

		public override string ToString()
		{
			return string.Format("{0}", base.ToString(), CardID);
		}
	}

	public class ProfileNoticePreconDeck : ProfileNotice
	{
		public long DeckID { get; set; }

		public int HeroAsset { get; set; }

		public ProfileNoticePreconDeck()
			: base(NoticeType.PRECON_DECK)
		{
		}

		public override string ToString()
		{
			return $"{base.ToString()} [DeckID={DeckID}, HeroAsset={HeroAsset}]";
		}
	}

	public class ProfileNoticeDeckRemoved : ProfileNotice
	{
		public long DeckID { get; set; }

		public ProfileNoticeDeckRemoved()
			: base(NoticeType.DECK_REMOVED)
		{
		}

		public override string ToString()
		{
			return $"{base.ToString()} [DeckID={DeckID}]";
		}
	}

	public class ProfileNoticeDeckGranted : ProfileNotice
	{
		public int DeckDbiID { get; set; }

		public int ClassId { get; set; }

		public long PlayerDeckID { get; set; }

		public ProfileNoticeDeckGranted()
			: base(NoticeType.DECK_GRANTED)
		{
		}

		public override string ToString()
		{
			return $"{base.ToString()} [DeckDbiID={DeckDbiID}, ClassId={ClassId}]";
		}
	}

	public class ProfileNoticeMiniSetGranted : ProfileNotice
	{
		public int MiniSetID { get; set; }

		public int Premium { get; set; }

		public ProfileNoticeMiniSetGranted()
			: base(NoticeType.MINI_SET_GRANTED)
		{
		}

		public override string ToString()
		{
			return $"{base.ToString()} [CardsRewardID={MiniSetID}]";
		}
	}

	public class ProfileNoticeSellableDeckGranted : ProfileNotice
	{
		public int SellableDeckID { get; set; }

		public long PlayerDeckID { get; set; }

		public TAG_PREMIUM Premium { get; set; }

		public ProfileNoticeSellableDeckGranted()
			: base(NoticeType.SELLABLE_DECK_GRANTED)
		{
		}

		public override string ToString()
		{
			return $"{base.ToString()} [SellableDeckID={SellableDeckID}]";
		}
	}

	public class ProfileNoticeRewardDust : ProfileNotice
	{
		public int Amount { get; set; }

		public ProfileNoticeRewardDust()
			: base(NoticeType.REWARD_DUST)
		{
		}

		public override string ToString()
		{
			return $"{base.ToString()} [Amount={Amount}]";
		}
	}

	public class ProfileNoticeRewardMount : ProfileNotice
	{
		public int MountID { get; set; }

		public ProfileNoticeRewardMount()
			: base(NoticeType.REWARD_MOUNT)
		{
		}

		public override string ToString()
		{
			return $"{base.ToString()} [MountID={MountID}]";
		}
	}

	public class ProfileNoticeRewardForge : ProfileNotice
	{
		public int Quantity { get; set; }

		public ProfileNoticeRewardForge()
			: base(NoticeType.REWARD_FORGE)
		{
		}

		public override string ToString()
		{
			return $"{base.ToString()} [Quantity={Quantity}]";
		}
	}

	public class ProfileNoticeRewardCurrency : ProfileNotice
	{
		public int Amount { get; set; }

		public PegasusShared.CurrencyType CurrencyType { get; set; }

		public ProfileNoticeRewardCurrency()
			: base(NoticeType.REWARD_CURRENCY)
		{
		}

		public override string ToString()
		{
			return $"{base.ToString()} [CurrencyType={CurrencyType.ToString()}, Amount={Amount}]";
		}
	}

	public class ProfileNoticePurchase : ProfileNotice
	{
		public long? PMTProductID { get; set; }

		public string CurrencyCode { get; set; }

		public long Data { get; set; }

		public ProfileNoticePurchase()
			: base(NoticeType.PURCHASE)
		{
		}

		public override string ToString()
		{
			return $"[ProfileNoticePurchase: NoticeID={base.NoticeID}, Type={base.Type}, Origin={base.Origin}, OriginData={base.OriginData}, Date={base.Date} PMTProductID='{PMTProductID}', Data={Data} Currency={CurrencyCode}]";
		}
	}

	public class ProfileNoticeRewardCardBack : ProfileNotice
	{
		public int CardBackID { get; set; }

		public ProfileNoticeRewardCardBack()
			: base(NoticeType.REWARD_CARD_BACK)
		{
		}

		public override string ToString()
		{
			return $"[ProfileNoticePurchase: NoticeID={base.NoticeID}, Type={base.Type}, Origin={base.Origin}, OriginData={base.OriginData}, Date={base.Date} CardBackID={CardBackID}]";
		}
	}

	public class ProfileNoticeBonusStars : ProfileNotice
	{
		public int StarLevel { get; set; }

		public int Stars { get; set; }

		public ProfileNoticeBonusStars()
			: base(NoticeType.BONUS_STARS)
		{
		}

		public override string ToString()
		{
			return $"{base.ToString()} [StarLevel={StarLevel}, Stars={Stars}]";
		}
	}

	public class ProfileNoticeEvent : ProfileNotice
	{
		public int EventType { get; }
	}

	public class ProfileNoticeDisconnectedGame : ProfileNotice
	{
		public PegasusShared.GameType GameType { get; set; }

		public PegasusShared.FormatType FormatType { get; set; }

		public int MissionId { get; set; }

		public ProfileNoticeDisconnectedGameResult.GameResult GameResult { get; set; }

		public ProfileNoticeDisconnectedGameResult.PlayerResult YourResult { get; set; }

		public ProfileNoticeDisconnectedGameResult.PlayerResult OpponentResult { get; set; }

		public int PlayerIndex { get; set; }

		public ProfileNoticeDisconnectedGame()
			: base(NoticeType.DISCONNECTED_GAME)
		{
		}

		public override string ToString()
		{
			return $"{base.ToString()} [GameType={GameType}, FormatType={FormatType}, MissionId={MissionId} GameResult={GameResult}, YourResult={YourResult}, OpponentResult={OpponentResult}, PlayerIndex={PlayerIndex}]";
		}
	}

	public class ProfileNoticeAdventureProgress : ProfileNotice
	{
		public int Wing { get; set; }

		public int? Progress { get; set; }

		public ulong? Flags { get; set; }

		public ProfileNoticeAdventureProgress()
			: base(NoticeType.ADVENTURE_PROGRESS)
		{
		}

		public override string ToString()
		{
			return $"{base.ToString()} [Wing={Wing}, Progress={Progress}, Flags={Flags}]";
		}
	}

	public class ProfileNoticeLevelUp : ProfileNotice
	{
		public int HeroClass { get; set; }

		public int NewLevel { get; set; }

		public int TotalLevel { get; set; }

		public ProfileNoticeLevelUp()
			: base(NoticeType.HERO_LEVEL_UP)
		{
		}

		public override string ToString()
		{
			return $"{base.ToString()} [HeroClass={HeroClass}, NewLevel={NewLevel}], TotalLevel={TotalLevel}";
		}
	}

	public class ProfileNoticeAcccountLicense : ProfileNotice
	{
		public long License { get; set; }

		public long CasID { get; set; }

		public ProfileNoticeAcccountLicense()
			: base(NoticeType.ACCOUNT_LICENSE)
		{
		}

		public override string ToString()
		{
			return $"{base.ToString()} [License={License}, CasID={CasID}]";
		}
	}

	public class ProfileNoticeTavernBrawlRewards : ProfileNotice
	{
		public RewardChest Chest { get; set; }

		public int Wins { get; set; }

		public TavernBrawlMode Mode { get; set; }

		public ProfileNoticeTavernBrawlRewards()
			: base(NoticeType.TAVERN_BRAWL_REWARDS)
		{
		}

		public override string ToString()
		{
			return $"{base.ToString()} [Chest={Chest}, Wins={Wins}, Mode={Mode}]";
		}
	}

	public class ProfileNoticeTavernBrawlTicket : ProfileNotice
	{
		[CompilerGenerated]
		private int _003CTicketType_003Ek__BackingField;

		[CompilerGenerated]
		private int _003CQuantity_003Ek__BackingField;

		public int TicketType
		{
			[CompilerGenerated]
			set
			{
				_003CTicketType_003Ek__BackingField = value;
			}
		}

		public int Quantity
		{
			[CompilerGenerated]
			set
			{
				_003CQuantity_003Ek__BackingField = value;
			}
		}

		public ProfileNoticeTavernBrawlTicket()
			: base(NoticeType.TAVERN_BRAWL_TICKET)
		{
		}
	}

	public class ProfileNoticeGenericRewardChest : ProfileNotice
	{
		public int RewardChestAssetId { get; set; }

		public RewardChest RewardChest { get; set; }

		public uint RewardChestByteSize { get; set; }

		public byte[] RewardChestHash { get; set; }

		public ProfileNoticeGenericRewardChest()
			: base(NoticeType.GENERIC_REWARD_CHEST)
		{
		}
	}

	public class NetCacheProfileNotices
	{
		public List<ProfileNotice> Notices { get; set; }

		public NetCacheProfileNotices()
		{
			Notices = new List<ProfileNotice>();
		}
	}

	public class ProfileNoticeLeaguePromotionRewards : ProfileNotice
	{
		public RewardChest Chest { get; set; }

		public int LeagueId { get; set; }

		public ProfileNoticeLeaguePromotionRewards()
			: base(NoticeType.LEAGUE_PROMOTION_REWARDS)
		{
		}

		public override string ToString()
		{
			return $"{base.ToString()} [Chest={Chest}, LeagueId={LeagueId}]";
		}
	}

	public class ProfileNoticeMercenariesRewards : ProfileNotice
	{
		public PegasusShared.ProfileNoticeMercenariesRewards.RewardType RewardType { get; set; }

		public RewardChest Chest { get; set; }

		public ProfileNoticeMercenariesRewards()
			: base(NoticeType.MERCENARIES_REWARDS)
		{
		}

		public override string ToString()
		{
			return $"{base.ToString()} [Chest={Chest}]";
		}
	}

	public class ProfileNoticeMercenariesAbilityUnlock : ProfileNotice
	{
		public int MercenaryId { get; set; }

		public int AbilityId { get; set; }

		public ProfileNoticeMercenariesAbilityUnlock()
			: base(NoticeType.MERCENARIES_ABILITY_UNLOCK)
		{
		}

		public override string ToString()
		{
			return $"[ProfileNoticeMercenariesAbilityUnlock: NoticeID={base.NoticeID}, Type={base.Type}, Origin={base.Origin}, OriginData={base.OriginData}, Date={base.Date} MercenaryId={MercenaryId} AbilityId={AbilityId}]";
		}
	}

	public class ProfileNoticeMercenariesZoneUnlock : ProfileNotice
	{
		public int ZoneId { get; set; }

		public ProfileNoticeMercenariesZoneUnlock()
			: base(NoticeType.MERCENARIES_ZONE_UNLOCK)
		{
		}

		public override string ToString()
		{
			return $"[ProfileNoticeMercenariesZoneUnlock: NoticeID={base.NoticeID}, Type={base.Type}, Origin={base.Origin}, OriginData={base.OriginData}, Date={base.Date} ZoneId={ZoneId}]";
		}
	}

	public class ProfileNoticeRewardBattlegroundsBoard : ProfileNotice
	{
		public long BoardSkinID { get; set; }

		public int FixedRewardMapID { get; set; }

		public ProfileNoticeRewardBattlegroundsBoard()
			: base(NoticeType.REWARD_BATTLEGROUNDS_BOARD_SKIN)
		{
		}

		public override string ToString()
		{
			return $"{base.ToString()} [BoardSkinID={BoardSkinID}]";
		}
	}

	public class ProfileNoticeRewardBattlegroundsFinisher : ProfileNotice
	{
		public long FinisherID { get; set; }

		public int FixedRewardMapID { get; set; }

		public ProfileNoticeRewardBattlegroundsFinisher()
			: base(NoticeType.REWARD_BATTLEGROUNDS_FINISHER)
		{
		}

		public override string ToString()
		{
			return $"{base.ToString()} [FinisherID={FinisherID}]";
		}
	}

	public class ProfileNoticeRewardBattlegroundsEmote : ProfileNotice
	{
		public long EmoteID { get; set; }

		public int FixedRewardMapID { get; set; }

		public ProfileNoticeRewardBattlegroundsEmote()
			: base(NoticeType.REWARD_BATTLEGROUNDS_EMOTE)
		{
		}

		public override string ToString()
		{
			return $"{base.ToString()} [EmoteID={EmoteID}]";
		}
	}

	public class ProfileNoticeMercenariesSeasonRoll : ProfileNotice
	{
		public int EndedSeasonId { get; set; }

		public int HighestSeasonRating { get; set; }

		public ProfileNoticeMercenariesSeasonRoll()
			: base(NoticeType.MERCENARIES_SEASON_ROLL)
		{
		}
	}

	public class ProfileNoticeMercenariesBoosterLicense : ProfileNotice
	{
		public int Count { get; set; }

		public ProfileNoticeMercenariesBoosterLicense()
			: base(NoticeType.MERCENARIES_BOOSTER_LICENSE)
		{
		}

		public override string ToString()
		{
			return $"[ProfileNoticeMercenariesBoosterLicense: NoticeID={base.NoticeID}, Type={base.Type}, Origin={base.Origin}, OriginData={base.OriginData}, Date={base.Date} Count={Count}]";
		}
	}

	public class ProfileNoticeMercenariesCurrencyLicense : ProfileNotice
	{
		public int MercenaryId { get; set; }

		public long CurrencyAmount { get; set; }

		public ProfileNoticeMercenariesCurrencyLicense()
			: base(NoticeType.MERCENARIES_CURRENCY_LICENSE)
		{
		}

		public override string ToString()
		{
			return $"[ProfileNoticeMercenariesBoosterLicense: NoticeID={base.NoticeID}, Type={base.Type}, Origin={base.Origin}, OriginData={base.OriginData}, Date={base.Date} MercenaryId={MercenaryId} CurrencyAmount={CurrencyAmount}]";
		}
	}

	public class ProfileNoticeMercenariesMercenaryLicense : ProfileNotice
	{
		public int MercenaryId { get; set; }

		public int ArtVariationId { get; set; }

		public uint ArtVariationPremium { get; set; }

		public long CurrencyAmount { get; set; }

		public ProfileNoticeMercenariesMercenaryLicense()
			: base(NoticeType.MERCENARIES_MERC_LICENSE)
		{
		}

		public override string ToString()
		{
			return $"[ProfileNoticeMercenariesMercenaryLicense: NoticeID={base.NoticeID}, Type={base.Type}, Origin={base.Origin}, OriginData={base.OriginData}, Date={base.Date} MercenaryId={MercenaryId}, ArtVariationId={ArtVariationId}, ArtVariationPremium={ArtVariationPremium} CurrencyAmount={CurrencyAmount}]";
		}
	}

	public class ProfileNoticeMercenariesRandomRewardLicense : ProfileNotice
	{
		public int MercenaryId { get; set; }

		public int ArtVariationId { get; set; }

		public uint ArtVariationPremium { get; set; }

		public long CurrencyAmount { get; set; }

		public bool IsConvertedMercenary { get; set; }

		public ProfileNoticeMercenariesRandomRewardLicense()
			: base(NoticeType.MERCENARIES_RANDOM_REWARD_LICENSE)
		{
		}

		public override string ToString()
		{
			return $"[ProfileNoticeMercenariesRandomRewardLicense: NoticeID={base.NoticeID}, Type={base.Type}, Origin={base.Origin}, OriginData={base.OriginData}, Date={base.Date} MercenaryId={MercenaryId}, ArtVariationId={ArtVariationId}, ArtVariationPremium={ArtVariationPremium} CurrencyAmount={CurrencyAmount}]";
		}
	}

	public class ProfileNoticeMercenariesMercenaryFullyUpgraded : ProfileNotice
	{
		public int MercenaryId { get; set; }

		public ProfileNoticeMercenariesMercenaryFullyUpgraded()
			: base(NoticeType.MERCENARIES_MERC_FULL_UPGRADE)
		{
		}

		public override string ToString()
		{
			return $"[ProfileNoticeMercenariesAbilityUnlock: NoticeID={base.NoticeID}, Type={base.Type}, Origin={base.Origin}, OriginData={base.OriginData}, Date={base.Date} MercenaryId={MercenaryId}";
		}
	}

	public class ProfileNoticeMercenariesSeasonRewards : ProfileNotice
	{
		public RewardChest Chest { get; set; }

		public int RewardAssetId { get; set; }

		public ProfileNoticeMercenariesSeasonRewards()
			: base(NoticeType.MERCENARIES_SEASON_REWARDS)
		{
		}

		public override string ToString()
		{
			return $"[Chest={Chest}, RewardAssetId={RewardAssetId}]";
		}
	}

	public class ProfileNoticeLuckyDrawReward : ProfileNotice
	{
		public int LuckyDrawRewardId { get; set; }

		public PegasusShared.ProfileNoticeLuckyDrawReward.OriginType LuckyDrawOrigin { get; set; }

		public ProfileNoticeLuckyDrawReward()
			: base(NoticeType.REWARD_LUCKY_DRAW)
		{
		}

		public override string ToString()
		{
			return $"[ProfileNoticeLuckyDrawReward: LuckyDrawRewardAssetId={LuckyDrawRewardId}, LuckyDrawOrigin={LuckyDrawOrigin}]";
		}
	}

	public class ProfileNoticeRedundantNDEReroll : ProfileNotice
	{
		public string CardID { get; set; }

		public TAG_PREMIUM Premium { get; set; }

		public TAG_PREMIUM RerollPremiumOverride { get; set; }

		public ProfileNoticeRedundantNDEReroll()
			: base(NoticeType.REDUNDANT_NDE_REROLL)
		{
		}

		public override string ToString()
		{
			return $"{base.ToString()} [CardID={CardID}, Premium={Premium}]";
		}
	}

	public class ProfileNoticeRedundantNDERerollResult : ProfileNotice
	{
		public int RerolledCardID { get; set; }

		public int GrantedCardID { get; set; }

		public TAG_PREMIUM Premium { get; set; }

		public ProfileNoticeRedundantNDERerollResult()
			: base(NoticeType.REDUNDANT_NDE_REROLL_RESULT)
		{
		}

		public override string ToString()
		{
			return $"{base.ToString()}, [RerolledCardID={RerolledCardID}, GrantedCardID={GrantedCardID}, Premium={Premium}]";
		}
	}

	public abstract class ClientOptionBase : ICloneable
	{
		public abstract void PopulateIntoPacket(ServerOption type, SetOptions packet);

		public override bool Equals(object other)
		{
			if (other == null)
			{
				return false;
			}
			if (other.GetType() != GetType())
			{
				return false;
			}
			return true;
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		public object Clone()
		{
			return MemberwiseClone();
		}
	}

	public class ClientOptionInt : ClientOptionBase
	{
		public int OptionValue { get; set; }

		public ClientOptionInt(int val)
		{
			OptionValue = val;
		}

		public override void PopulateIntoPacket(ServerOption type, SetOptions packet)
		{
			PegasusUtil.ClientOption option = new PegasusUtil.ClientOption();
			option.Index = (int)type;
			option.AsInt32 = OptionValue;
			packet.Options.Add(option);
		}

		public override bool Equals(object other)
		{
			if (base.Equals(other) && ((ClientOptionInt)other).OptionValue == OptionValue)
			{
				return true;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return OptionValue.GetHashCode();
		}
	}

	public class ClientOptionLong : ClientOptionBase
	{
		public long OptionValue { get; set; }

		public ClientOptionLong(long val)
		{
			OptionValue = val;
		}

		public override void PopulateIntoPacket(ServerOption type, SetOptions packet)
		{
			PegasusUtil.ClientOption option = new PegasusUtil.ClientOption();
			option.Index = (int)type;
			option.AsInt64 = OptionValue;
			packet.Options.Add(option);
		}

		public override bool Equals(object other)
		{
			if (base.Equals(other) && ((ClientOptionLong)other).OptionValue == OptionValue)
			{
				return true;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return OptionValue.GetHashCode();
		}
	}

	public class ClientOptionFloat : ClientOptionBase
	{
		public float OptionValue { get; set; }

		public ClientOptionFloat(float val)
		{
			OptionValue = val;
		}

		public override void PopulateIntoPacket(ServerOption type, SetOptions packet)
		{
			PegasusUtil.ClientOption option = new PegasusUtil.ClientOption();
			option.Index = (int)type;
			option.AsFloat = OptionValue;
			packet.Options.Add(option);
		}

		public override bool Equals(object other)
		{
			if (base.Equals(other) && ((ClientOptionFloat)other).OptionValue == OptionValue)
			{
				return true;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return OptionValue.GetHashCode();
		}
	}

	public class ClientOptionULong : ClientOptionBase
	{
		public ulong OptionValue { get; set; }

		public ClientOptionULong(ulong val)
		{
			OptionValue = val;
		}

		public override void PopulateIntoPacket(ServerOption type, SetOptions packet)
		{
			PegasusUtil.ClientOption option = new PegasusUtil.ClientOption();
			option.Index = (int)type;
			option.AsUint64 = OptionValue;
			packet.Options.Add(option);
		}

		public override bool Equals(object other)
		{
			if (base.Equals(other) && ((ClientOptionULong)other).OptionValue == OptionValue)
			{
				return true;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return OptionValue.GetHashCode();
		}
	}

	public class NetCacheClientOptions
	{
		private DateTime? m_mostRecentDispatchToServer;

		private DateTime? m_currentScheduledDispatchTime;

		private int ClientOptionsUpdateIntervalSeconds
		{
			get
			{
				NetCacheFeatures features = Get().GetNetObject<NetCacheFeatures>();
				if (features != null && features.Misc != null)
				{
					return features.Misc.ClientOptionsUpdateIntervalSeconds;
				}
				return 180;
			}
		}

		public Map<ServerOption, ClientOptionBase> ClientState { get; private set; }

		private Map<ServerOption, ClientOptionBase> ServerState { get; set; }

		public NetCacheClientOptions()
		{
			ClientState = new Map<ServerOption, ClientOptionBase>();
			ServerState = new Map<ServerOption, ClientOptionBase>();
		}

		public void UpdateServerState()
		{
			foreach (KeyValuePair<ServerOption, ClientOptionBase> kv in ClientState)
			{
				if (kv.Value != null)
				{
					ServerState[kv.Key] = (ClientOptionBase)kv.Value.Clone();
				}
				else
				{
					ServerState[kv.Key] = null;
				}
			}
		}

		public void OnUpdateIntervalElasped(object userData)
		{
			m_currentScheduledDispatchTime = null;
			DispatchClientOptionsToServer();
		}

		public void CancelScheduledDispatchToServer()
		{
			Processor.CancelScheduledCallback(OnUpdateIntervalElasped);
			m_currentScheduledDispatchTime = null;
		}

		public void DispatchClientOptionsToServer()
		{
			CancelScheduledDispatchToServer();
			bool requiresDispatch = false;
			SetOptions packet = new SetOptions();
			foreach (KeyValuePair<ServerOption, ClientOptionBase> kv in ClientState)
			{
				if (ServerState.TryGetValue(kv.Key, out var optionBase))
				{
					if (kv.Value != null || optionBase != null)
					{
						if ((kv.Value == null && optionBase != null) || (kv.Value != null && optionBase == null))
						{
							requiresDispatch = true;
							break;
						}
						if (!optionBase.Equals(kv.Value))
						{
							requiresDispatch = true;
							break;
						}
					}
					continue;
				}
				requiresDispatch = true;
				break;
			}
			if (!requiresDispatch)
			{
				return;
			}
			foreach (KeyValuePair<ServerOption, ClientOptionBase> kv2 in ClientState)
			{
				if (kv2.Value != null)
				{
					kv2.Value.PopulateIntoPacket(kv2.Key, packet);
				}
			}
			Network.Get().SetClientOptions(packet);
			m_mostRecentDispatchToServer = DateTime.UtcNow;
			UpdateServerState();
		}

		public void RemoveInvalidOptions()
		{
			List<ServerOption> removeOptions = new List<ServerOption>();
			foreach (KeyValuePair<ServerOption, ClientOptionBase> kv in ClientState)
			{
				ServerOption key = kv.Key;
				ClientOptionBase value = kv.Value;
				Type keyType = Options.Get().GetServerOptionType(key);
				if (value != null)
				{
					Type valueType = value.GetType();
					if (keyType == typeof(int))
					{
						if (valueType == typeof(ClientOptionInt))
						{
							continue;
						}
					}
					else if (keyType == typeof(long))
					{
						if (valueType == typeof(ClientOptionLong))
						{
							continue;
						}
					}
					else if (keyType == typeof(float))
					{
						if (valueType == typeof(ClientOptionFloat))
						{
							continue;
						}
					}
					else if (keyType == typeof(ulong) && valueType == typeof(ClientOptionULong))
					{
						continue;
					}
					if (keyType == null)
					{
						Log.Net.Print("NetCacheClientOptions.RemoveInvalidOptions() - Option {0} has type {1}, but value is type {2}. Removing it.", key, keyType, valueType);
					}
					else
					{
						Log.Net.Print("NetCacheClientOptions.RemoveInvalidOptions() - Option {0} has type {1}, but value is type {2}. Removing it.", EnumUtils.GetString(key), keyType, valueType);
					}
				}
				removeOptions.Add(key);
			}
			foreach (ServerOption option in removeOptions)
			{
				ClientState.Remove(option);
				ServerState.Remove(option);
			}
		}

		public void CheckForDispatchToServer()
		{
			float updateIntervalSeconds = ClientOptionsUpdateIntervalSeconds;
			if (updateIntervalSeconds <= 0f)
			{
				return;
			}
			DateTime utcNow = DateTime.UtcNow;
			bool doDispatchNow = false;
			bool doDispatchWithDelay = false;
			if (!m_mostRecentDispatchToServer.HasValue)
			{
				doDispatchNow = true;
			}
			else if (!m_currentScheduledDispatchTime.HasValue)
			{
				TimeSpan timeSinceMostRecentDispatch = utcNow - m_mostRecentDispatchToServer.Value;
				if (timeSinceMostRecentDispatch.TotalSeconds >= (double)updateIntervalSeconds)
				{
					doDispatchNow = true;
				}
				else
				{
					doDispatchWithDelay = true;
					updateIntervalSeconds -= (float)timeSinceMostRecentDispatch.TotalSeconds;
				}
			}
			if (!doDispatchNow && !doDispatchWithDelay && m_currentScheduledDispatchTime.HasValue && (m_currentScheduledDispatchTime.Value - utcNow).TotalSeconds > (double)updateIntervalSeconds)
			{
				doDispatchWithDelay = true;
			}
			if (doDispatchNow || doDispatchWithDelay)
			{
				float secondsToWait = (doDispatchNow ? 0f : updateIntervalSeconds);
				m_currentScheduledDispatchTime = utcNow;
				Processor.CancelScheduledCallback(OnUpdateIntervalElasped);
				Processor.ScheduleCallback(secondsToWait, realTime: true, OnUpdateIntervalElasped);
			}
		}
	}

	public class NetCacheFavoriteHeroes
	{
		public List<(TAG_CLASS, CardDefinition)> FavoriteHeroes { get; set; }

		public NetCacheFavoriteHeroes()
		{
			FavoriteHeroes = new List<(TAG_CLASS, CardDefinition)>();
		}
	}

	public class NetCacheAccountLicenses
	{
		public Map<long, AccountLicenseInfo> AccountLicenses { get; set; }

		public NetCacheAccountLicenses()
		{
			AccountLicenses = new Map<long, AccountLicenseInfo>();
		}
	}

	public class NetCacheBattlegroundsHeroSkins
	{
		public Map<int, HashSet<int>> BattlegroundsFavoriteHeroSkins { get; set; }

		public HashSet<BattlegroundsHeroSkinId> OwnedBattlegroundsSkins { get; }

		public HashSet<BattlegroundsHeroSkinId> UnseenSkinIds { get; }

		public NetCacheBattlegroundsHeroSkins()
		{
			OwnedBattlegroundsSkins = new HashSet<BattlegroundsHeroSkinId>();
			BattlegroundsFavoriteHeroSkins = new Map<int, HashSet<int>>();
			UnseenSkinIds = new HashSet<BattlegroundsHeroSkinId>();
		}
	}

	public class NetCacheBattlegroundsGuideSkins
	{
		public BattlegroundsGuideSkinId? BattlegroundsFavoriteGuideSkin { get; set; }

		public HashSet<BattlegroundsGuideSkinId> OwnedBattlegroundsGuideSkins { get; }

		public HashSet<BattlegroundsGuideSkinId> UnseenSkinIds { get; }

		public NetCacheBattlegroundsGuideSkins()
		{
			OwnedBattlegroundsGuideSkins = new HashSet<BattlegroundsGuideSkinId>();
			BattlegroundsFavoriteGuideSkin = null;
			UnseenSkinIds = new HashSet<BattlegroundsGuideSkinId>();
		}
	}

	public class NetCacheBattlegroundsBoardSkins
	{
		public HashSet<BattlegroundsBoardSkinId> BattlegroundsFavoriteBoardSkins { get; set; }

		public HashSet<BattlegroundsBoardSkinId> OwnedBattlegroundsBoardSkins { get; set; }

		public HashSet<BattlegroundsBoardSkinId> UnseenSkinIds { get; }

		public NetCacheBattlegroundsBoardSkins()
		{
			OwnedBattlegroundsBoardSkins = new HashSet<BattlegroundsBoardSkinId>();
			BattlegroundsFavoriteBoardSkins = new HashSet<BattlegroundsBoardSkinId>();
			UnseenSkinIds = new HashSet<BattlegroundsBoardSkinId>();
		}
	}

	public class NetCacheBattlegroundsFinishers
	{
		public HashSet<BattlegroundsFinisherId> BattlegroundsFavoriteFinishers { get; set; }

		public HashSet<BattlegroundsFinisherId> OwnedBattlegroundsFinishers { get; set; }

		public HashSet<BattlegroundsFinisherId> UnseenSkinIds { get; }

		public NetCacheBattlegroundsFinishers()
		{
			OwnedBattlegroundsFinishers = new HashSet<BattlegroundsFinisherId>();
			BattlegroundsFavoriteFinishers = new HashSet<BattlegroundsFinisherId>();
			UnseenSkinIds = new HashSet<BattlegroundsFinisherId>();
		}
	}

	public class NetCacheBattlegroundsEmotes
	{
		private Hearthstone.BattlegroundsEmoteLoadout _currentLoadout = new Hearthstone.BattlegroundsEmoteLoadout();

		public HashSet<BattlegroundsEmoteId> OwnedBattlegroundsEmotes { get; set; }

		public HashSet<BattlegroundsEmoteId> UnseenEmoteIds { get; }

		public Hearthstone.BattlegroundsEmoteLoadout CurrentLoadout
		{
			get
			{
				return new Hearthstone.BattlegroundsEmoteLoadout(_currentLoadout);
			}
			set
			{
				_currentLoadout = new Hearthstone.BattlegroundsEmoteLoadout(value);
			}
		}

		public NetCacheBattlegroundsEmotes()
		{
			OwnedBattlegroundsEmotes = new HashSet<BattlegroundsEmoteId>();
			UnseenEmoteIds = new HashSet<BattlegroundsEmoteId>();
			CurrentLoadout = new Hearthstone.BattlegroundsEmoteLoadout();
		}
	}

	public class NetCacheLettuceMap
	{
		public PegasusLettuce.LettuceMap Map { get; set; }

		public NetCacheLettuceMap()
		{
			Map = null;
		}
	}

	public delegate void ErrorCallback(ErrorInfo info);

	public enum ErrorCode
	{
		NONE,
		TIMEOUT,
		SERVER
	}

	public class ErrorInfo
	{
		[CompilerGenerated]
		private uint _003CServerError_003Ek__BackingField;

		public ErrorCode Error { get; set; }

		public uint ServerError
		{
			[CompilerGenerated]
			set
			{
				_003CServerError_003Ek__BackingField = value;
			}
		}

		public RequestFunc RequestingFunction { get; set; }

		public Map<Type, Request> RequestedTypes { get; set; }

		public string RequestStackTrace { get; set; }
	}

	public delegate void NetCacheCallback();

	public delegate void RequestFunc(NetCacheCallback callback, ErrorCallback errorCallback);

	public enum RequestResult
	{
		UNKNOWN,
		PENDING,
		IN_PROCESS,
		GENERIC_COMPLETE,
		DATA_COMPLETE,
		ERROR,
		MIGRATION_REQUIRED
	}

	public class Request
	{
		public Type m_type;

		public bool m_reload;

		public RequestResult m_result;

		public Request(Type rt, bool rl = false)
		{
			m_type = rt;
			m_reload = rl;
			m_result = RequestResult.UNKNOWN;
		}
	}

	private class NetCacheBatchRequest
	{
		public Map<Type, Request> m_requests = new Map<Type, Request>();

		public NetCacheCallback m_callback;

		public ErrorCallback m_errorCallback;

		public bool m_canTimeout = true;

		public float m_timeAdded = Time.realtimeSinceStartup;

		public RequestFunc m_requestFunc;

		public string m_requestStackTrace;

		public NetCacheBatchRequest(NetCacheCallback reply, ErrorCallback errorCallback, RequestFunc requestFunc)
		{
			m_callback = reply;
			m_errorCallback = errorCallback;
			m_requestFunc = requestFunc;
			m_requestStackTrace = Environment.StackTrace;
		}

		public void AddRequests(List<Request> requests)
		{
			foreach (Request r in requests)
			{
				AddRequest(r);
			}
		}

		public void AddRequest(Request r)
		{
			if (!m_requests.ContainsKey(r.m_type))
			{
				m_requests.Add(r.m_type, r);
			}
		}
	}

	private static readonly Map<Type, GetAccountInfo.Request> m_getAccountInfoTypeMap = new Map<Type, GetAccountInfo.Request>
	{
		{
			typeof(NetCacheDecks),
			GetAccountInfo.Request.DECK_LIST
		},
		{
			typeof(NetCacheMedalInfo),
			GetAccountInfo.Request.MEDAL_INFO
		},
		{
			typeof(NetCacheCardBacks),
			GetAccountInfo.Request.CARD_BACKS
		},
		{
			typeof(NetCachePlayerRecords),
			GetAccountInfo.Request.PLAYER_RECORD
		},
		{
			typeof(NetCacheGamesPlayed),
			GetAccountInfo.Request.GAMES_PLAYED
		},
		{
			typeof(NetCacheProfileProgress),
			GetAccountInfo.Request.CAMPAIGN_INFO
		},
		{
			typeof(NetCacheCardValues),
			GetAccountInfo.Request.CARD_VALUES
		},
		{
			typeof(NetCacheFeatures),
			GetAccountInfo.Request.FEATURES
		},
		{
			typeof(NetCacheRewardProgress),
			GetAccountInfo.Request.REWARD_PROGRESS
		},
		{
			typeof(NetCacheHeroLevels),
			GetAccountInfo.Request.HERO_XP
		},
		{
			typeof(NetCacheFavoriteHeroes),
			GetAccountInfo.Request.FAVORITE_HEROES
		},
		{
			typeof(NetCacheAccountLicenses),
			GetAccountInfo.Request.ACCOUNT_LICENSES
		},
		{
			typeof(NetCacheCoins),
			GetAccountInfo.Request.COINS
		},
		{
			typeof(NetCacheBattlegroundsHeroSkins),
			GetAccountInfo.Request.BATTLEGROUNDS_SKINS
		},
		{
			typeof(NetCacheBattlegroundsGuideSkins),
			GetAccountInfo.Request.BATTLEGROUNDS_GUIDE_SKINS
		},
		{
			typeof(NetCacheBattlegroundsBoardSkins),
			GetAccountInfo.Request.BATTLEGROUNDS_BOARD_SKINS
		},
		{
			typeof(NetCacheBattlegroundsFinishers),
			GetAccountInfo.Request.BATTLEGROUNDS_FINISHERS
		},
		{
			typeof(NetCacheBattlegroundsEmotes),
			GetAccountInfo.Request.BATTLEGROUNDS_EMOTES
		}
	};

	private static readonly Map<Type, int> m_genericRequestTypeMap = new Map<Type, int> { 
	{
		typeof(ClientStaticAssetsResponse),
		340
	} };

	private static readonly List<Type> m_ServerInitiatedAccountInfoTypes = new List<Type>
	{
		typeof(NetCacheCollection),
		typeof(NetCacheClientOptions),
		typeof(NetCacheArcaneDustBalance),
		typeof(NetCacheGoldBalance),
		typeof(NetCacheProfileNotices),
		typeof(NetCacheBoosters),
		typeof(NetCacheDecks),
		typeof(NetCacheRenownBalance),
		typeof(NetCacheBattlegroundsTokenBalance)
	};

	private static readonly Map<GetAccountInfo.Request, Type> m_requestTypeMap = GetInvertTypeMap();

	private Map<Type, object> m_netCache = new Map<Type, object>();

	private NetCacheHeroLevels m_prevHeroLevels;

	private NetCacheMedalInfo m_previousMedalInfo;

	private List<DelNewNoticesListener> m_newNoticesListeners = new List<DelNewNoticesListener>();

	private List<DelGoldBalanceListener> m_goldBalanceListeners = new List<DelGoldBalanceListener>();

	private List<DelRenownBalanceListener> m_renownBalanceListeners = new List<DelRenownBalanceListener>();

	private List<DelBattlegroundsTokenBalanceListener> m_battlegroundsTokenBalanceListeners = new List<DelBattlegroundsTokenBalanceListener>();

	private Map<Type, HashSet<Action>> m_updatedListeners = new Map<Type, HashSet<Action>>();

	private Map<Type, int> m_changeRequests = new Map<Type, int>();

	private bool m_receivedInitialClientState;

	private HashSet<long> m_ackedNotices = new HashSet<long>();

	private List<ProfileNotice> m_queuedProfileNotices = new List<ProfileNotice>();

	private bool m_receivedInitialProfileNotices;

	private long m_currencyVersion;

	private long m_initialCollectionVersion;

	private HashSet<long> m_expectedCardModifications = new HashSet<long>();

	private HashSet<long> m_handledCardModifications = new HashSet<long>();

	[CompilerGenerated]
	private DelBattlegroundsEmoteLoadoutChangedListener BattlegroundsEmoteLoadoutChangedListener;

	private long m_lastForceCheckedSeason;

	private List<NetCacheBatchRequest> m_cacheRequests = new List<NetCacheBatchRequest>();

	private List<NetCacheBatchRequest> m_cacheRequestScratchList = new List<NetCacheBatchRequest>();

	private List<Type> m_inTransitRequests = new List<Type>();

	private static bool m_fatalErrorCodeSet = false;

	public bool HasReceivedInitialClientState => m_receivedInitialClientState;

	public InitialClientState InitialClientState { get; set; }

	public event DelFavoriteCardBackChangedListener FavoriteCardBackChanged;

	public event DelFavoriteBattlegroundsHeroSkinChangedListener FavoriteBattlegroundsHeroSkinChanged;

	public event DelFavoriteBattlegroundsGuideSkinChangedListener FavoriteBattlegroundsGuideSkinChanged;

	public event DelFavoriteBattlegroundsBoardSkinChangedListener FavoriteBattlegroundsBoardSkinChanged;

	public event DelFavoriteBattlegroundsFinisherChangedListener FavoriteBattlegroundsFinisherChanged;

	public event DelFavoriteCoinChangedListener FavoriteCoinChanged;

	public event DelOwnedBattlegroundsSkinsChanged OwnedBattlegroundsSkinsChanged;

	private static Map<GetAccountInfo.Request, Type> GetInvertTypeMap()
	{
		Map<GetAccountInfo.Request, Type> inverted = new Map<GetAccountInfo.Request, Type>();
		foreach (KeyValuePair<Type, GetAccountInfo.Request> entry in m_getAccountInfoTypeMap)
		{
			inverted[entry.Value] = entry.Key;
		}
		return inverted;
	}

	public IEnumerator<IAsyncJobResult> Initialize(ServiceLocator serviceLocator)
	{
		serviceLocator.Get<Network>().RegisterThrottledPacketListener(OnPacketThrottled);
		RegisterNetCacheHandlers();
		yield break;
	}

	public Type[] GetDependencies()
	{
		return new Type[1] { typeof(Network) };
	}

	public void Shutdown()
	{
	}

	public static NetCache Get()
	{
		return ServiceManager.Get<NetCache>();
	}

	public T GetNetObject<T>()
	{
		Type type = typeof(T);
		object data = GetTestData(type);
		if (data != null)
		{
			return (T)data;
		}
		if (m_netCache.TryGetValue(typeof(T), out data) && data is T)
		{
			return (T)data;
		}
		return default(T);
	}

	public bool IsNetObjectAvailable<T>()
	{
		return GetNetObject<T>() != null;
	}

	private object GetTestData(Type type)
	{
		if (type == typeof(NetCacheBoosters) && GameUtils.IsFakePackOpeningEnabled())
		{
			NetCacheBoosters netCacheBoosters = new NetCacheBoosters();
			int packCount = GameUtils.GetFakePackCount();
			BoosterStack boosterStack = new BoosterStack
			{
				Id = 1,
				Count = packCount
			};
			netCacheBoosters.BoosterStacks.Add(boosterStack);
			return netCacheBoosters;
		}
		return null;
	}

	public void UnloadNetObject<T>()
	{
		Type type = typeof(T);
		m_netCache[type] = null;
	}

	public void ReloadNetObject<T>()
	{
		NetCacheReload_Internal(null, typeof(T));
	}

	public void RefreshNetObject<T>()
	{
		RequestNetCacheObject(typeof(T));
	}

	public long GetArcaneDustBalance()
	{
		NetCacheArcaneDustBalance dustBalance = GetNetObject<NetCacheArcaneDustBalance>();
		if (dustBalance == null)
		{
			return 0L;
		}
		if (CraftingManager.IsInitialized)
		{
			return dustBalance.Balance + CraftingManager.Get().GetUnCommitedArcaneDustChanges();
		}
		return dustBalance.Balance;
	}

	public long GetGoldBalance()
	{
		return GetNetObject<NetCacheGoldBalance>()?.GetTotal() ?? 0;
	}

	public long GetRenownBalance()
	{
		return GetNetObject<NetCacheRenownBalance>()?.Balance ?? 0;
	}

	public long GetBattlegroundsTokenBalance()
	{
		return GetNetObject<NetCacheBattlegroundsTokenBalance>()?.Balance ?? 0;
	}

	public int GetArenaTicketBalance()
	{
		return GetNetObject<NetPlayerArenaTickets>()?.Balance ?? 0;
	}

	private bool GetOption<T>(ServerOption type, out T ret) where T : ClientOptionBase
	{
		ret = null;
		NetCacheClientOptions allOptions = Get().GetNetObject<NetCacheClientOptions>();
		if (!ClientOptionExists(type))
		{
			return false;
		}
		if (!(allOptions.ClientState[type] is T thisOption))
		{
			return false;
		}
		ret = thisOption;
		return true;
	}

	public int GetIntOption(ServerOption type)
	{
		ClientOptionInt thisOption = null;
		if (!GetOption<ClientOptionInt>(type, out thisOption))
		{
			return 0;
		}
		return thisOption.OptionValue;
	}

	public bool GetIntOption(ServerOption type, out int ret)
	{
		ret = 0;
		ClientOptionInt thisOption = null;
		if (!GetOption<ClientOptionInt>(type, out thisOption))
		{
			return false;
		}
		ret = thisOption.OptionValue;
		return true;
	}

	public long GetLongOption(ServerOption type)
	{
		ClientOptionLong thisOption = null;
		if (!GetOption<ClientOptionLong>(type, out thisOption))
		{
			return 0L;
		}
		return thisOption.OptionValue;
	}

	public bool GetLongOption(ServerOption type, out long ret)
	{
		ret = 0L;
		ClientOptionLong thisOption = null;
		if (!GetOption<ClientOptionLong>(type, out thisOption))
		{
			return false;
		}
		ret = thisOption.OptionValue;
		return true;
	}

	public float GetFloatOption(ServerOption type)
	{
		ClientOptionFloat thisOption = null;
		if (!GetOption<ClientOptionFloat>(type, out thisOption))
		{
			return 0f;
		}
		return thisOption.OptionValue;
	}

	public bool GetFloatOption(ServerOption type, out float ret)
	{
		ret = 0f;
		ClientOptionFloat thisOption = null;
		if (!GetOption<ClientOptionFloat>(type, out thisOption))
		{
			return false;
		}
		ret = thisOption.OptionValue;
		return true;
	}

	public ulong GetULongOption(ServerOption type)
	{
		ClientOptionULong thisOption = null;
		if (!GetOption<ClientOptionULong>(type, out thisOption))
		{
			return 0uL;
		}
		return thisOption.OptionValue;
	}

	public bool GetULongOption(ServerOption type, out ulong ret)
	{
		ret = 0uL;
		ClientOptionULong thisOption = null;
		if (!GetOption<ClientOptionULong>(type, out thisOption))
		{
			return false;
		}
		ret = thisOption.OptionValue;
		return true;
	}

	public void RegisterUpdatedListener(Type type, Action listener)
	{
		if (listener != null)
		{
			if (!m_updatedListeners.TryGetValue(type, out var listeners))
			{
				listeners = new HashSet<Action>();
				m_updatedListeners[type] = listeners;
			}
			m_updatedListeners[type].Add(listener);
		}
	}

	public void RemoveUpdatedListener(Type type, Action listener)
	{
		if (listener != null && m_updatedListeners.TryGetValue(type, out var listeners))
		{
			listeners.Remove(listener);
		}
	}

	public void RegisterNewNoticesListener(DelNewNoticesListener listener)
	{
		if (!m_newNoticesListeners.Contains(listener))
		{
			m_newNoticesListeners.Add(listener);
		}
	}

	public void RemoveNewNoticesListener(DelNewNoticesListener listener)
	{
		m_newNoticesListeners.Remove(listener);
	}

	public bool RemoveNotice(long ID)
	{
		if (!(m_netCache[typeof(NetCacheProfileNotices)] is NetCacheProfileNotices profileNotices))
		{
			Debug.LogWarning($"NetCache.RemoveNotice({ID}) - profileNotices is null");
			return false;
		}
		if (profileNotices.Notices == null)
		{
			Debug.LogWarning($"NetCache.RemoveNotice({ID}) - profileNotices.Notices is null");
			return false;
		}
		ProfileNotice notice = profileNotices.Notices.Find((ProfileNotice obj) => obj.NoticeID == ID);
		if (notice == null)
		{
			return false;
		}
		if (!profileNotices.Notices.Contains(notice))
		{
			Debug.LogWarning($"NetCache.RemoveNotice({ID}) - profileNotices.Notices does not contain notice to be removed");
			return false;
		}
		profileNotices.Notices.Remove(notice);
		m_ackedNotices.Add(notice.NoticeID);
		return true;
	}

	public void NetCacheChanged<T>()
	{
		Type type = typeof(T);
		int changeRequests = 0;
		m_changeRequests.TryGetValue(type, out changeRequests);
		changeRequests++;
		m_changeRequests[type] = changeRequests;
		if (changeRequests <= 1)
		{
			while (m_changeRequests[type] > 0)
			{
				NetCacheChangedImpl<T>();
				m_changeRequests[type] -= 1;
			}
		}
	}

	private void NetCacheChangedImpl<T>()
	{
		NetCacheBatchRequest[] array = m_cacheRequests.ToArray();
		foreach (NetCacheBatchRequest request in array)
		{
			foreach (KeyValuePair<Type, Request> request2 in request.m_requests)
			{
				if (!(request2.Key != typeof(T)))
				{
					NetCacheCheckRequest(request);
					break;
				}
			}
		}
	}

	public void CheckSeasonForRoll()
	{
		if (GetNetObject<NetCacheProfileNotices>() == null)
		{
			return;
		}
		NetCacheRewardProgress rewardProgress = GetNetObject<NetCacheRewardProgress>();
		if (rewardProgress != null)
		{
			DateTime now = DateTime.UtcNow;
			DateTime seasonEnd = DateTime.FromFileTimeUtc(rewardProgress.SeasonEndDate);
			if (!(seasonEnd >= now) && m_lastForceCheckedSeason != rewardProgress.Season)
			{
				m_lastForceCheckedSeason = rewardProgress.Season;
				Log.Net.Print("NetCache.CheckSeasonForRoll oldSeason = {0} season end = {1} utc now = {2}", m_lastForceCheckedSeason, seasonEnd, now);
			}
		}
	}

	public void RegisterGoldBalanceListener(DelGoldBalanceListener listener)
	{
		if (!m_goldBalanceListeners.Contains(listener))
		{
			m_goldBalanceListeners.Add(listener);
		}
	}

	public void RemoveGoldBalanceListener(DelGoldBalanceListener listener)
	{
		m_goldBalanceListeners.Remove(listener);
	}

	public void RegisterRenownBalanceListener(DelRenownBalanceListener listener)
	{
		if (!m_renownBalanceListeners.Contains(listener))
		{
			m_renownBalanceListeners.Add(listener);
		}
	}

	public void RemoveRenownBalanceListener(DelRenownBalanceListener listener)
	{
		m_renownBalanceListeners.Remove(listener);
	}

	public void RegisterBattlegroundsTokenBalanceListener(DelBattlegroundsTokenBalanceListener listener)
	{
		if (!m_battlegroundsTokenBalanceListeners.Contains(listener))
		{
			m_battlegroundsTokenBalanceListeners.Add(listener);
		}
	}

	public void RemoveBattlegroundsTokenBalanceListener(DelBattlegroundsTokenBalanceListener listener)
	{
		m_battlegroundsTokenBalanceListeners.Remove(listener);
	}

	public static void DefaultErrorHandler(ErrorInfo info)
	{
		if (info.Error == ErrorCode.TIMEOUT)
		{
			BreakingNews breakingNews = ServiceManager.Get<BreakingNews>();
			if (breakingNews != null && breakingNews.ShouldShowForCurrentPlatform)
			{
				string errorMsg = "GLOBAL_ERROR_NETWORK_UTIL_TIMEOUT";
				Network.Get().ShowBreakingNewsOrError(errorMsg);
			}
			else
			{
				ShowError(info, "GLOBAL_ERROR_NETWORK_UTIL_TIMEOUT");
			}
		}
		else
		{
			ShowError(info, "GLOBAL_ERROR_NETWORK_GENERIC");
		}
	}

	public static void ShowError(ErrorInfo info, string localizationKey, params object[] localizationArgs)
	{
		Error.AddFatal(FatalErrorReason.NET_CACHE, localizationKey, localizationArgs);
		Debug.LogError(GetInternalErrorMessage(info));
	}

	public static string GetInternalErrorMessage(ErrorInfo info, bool includeStackTrace = true)
	{
		Map<Type, object> netCacheMap = Get().m_netCache;
		StringBuilder builder = new StringBuilder();
		builder.AppendFormat("NetCache Error: {0}", info.Error);
		builder.AppendFormat("\nFrom: {0}", info.RequestingFunction.Method.Name);
		builder.AppendFormat("\nRequested Data ({0}):", info.RequestedTypes.Count);
		foreach (KeyValuePair<Type, Request> entry in info.RequestedTypes)
		{
			object obj = null;
			netCacheMap.TryGetValue(entry.Key, out obj);
			if (obj == null)
			{
				builder.AppendFormat("\n[{0}] MISSING", entry.Key);
			}
			else
			{
				builder.AppendFormat("\n[{0}]", entry.Key);
			}
		}
		if (includeStackTrace)
		{
			builder.AppendFormat("\nStack Trace:\n{0}", info.RequestStackTrace);
		}
		return builder.ToString();
	}

	private void NetCacheMakeBatchRequest(NetCacheBatchRequest batchRequest)
	{
		List<GetAccountInfo.Request> requestIdList = new List<GetAccountInfo.Request>();
		List<GenericRequest> genericRequests = null;
		foreach (KeyValuePair<Type, Request> request2 in batchRequest.m_requests)
		{
			Request request = request2.Value;
			if (request == null)
			{
				Debug.LogError($"NetUseBatchRequest Null request for {request.m_type.Name}...SKIP");
				continue;
			}
			if (m_ServerInitiatedAccountInfoTypes.Contains(request.m_type))
			{
				if (request.m_reload)
				{
					Log.All.PrintWarning("Attempting to reload server-initiated NetCache request {0}. This is not valid - the server sends this data when it changes!", request.m_type.FullName);
				}
				continue;
			}
			if (request.m_reload)
			{
				m_netCache[request.m_type] = null;
			}
			if ((m_netCache.ContainsKey(request.m_type) && m_netCache[request.m_type] != null) || m_inTransitRequests.Contains(request.m_type))
			{
				continue;
			}
			request.m_result = RequestResult.PENDING;
			m_inTransitRequests.Add(request.m_type);
			int genericRequestId;
			if (m_getAccountInfoTypeMap.TryGetValue(request.m_type, out var getAccountInfoType))
			{
				requestIdList.Add(getAccountInfoType);
			}
			else if (m_genericRequestTypeMap.TryGetValue(request.m_type, out genericRequestId))
			{
				if (genericRequests == null)
				{
					genericRequests = new List<GenericRequest>();
				}
				GenericRequest genericRequest = new GenericRequest();
				genericRequest.RequestId = genericRequestId;
				genericRequests.Add(genericRequest);
			}
			else
			{
				Log.Net.Print("NetCache: Unable to make request for type={0}", request.m_type.FullName);
			}
		}
		if (requestIdList.Count > 0 || genericRequests != null)
		{
			Network.Get().RequestNetCacheObjectList(requestIdList, genericRequests);
		}
		if (m_cacheRequests.FindIndex((NetCacheBatchRequest o) => o.m_callback != null && o.m_callback == batchRequest.m_callback) >= 0)
		{
			Log.Net.PrintError("NetCache: detected multiple registrations for same callback! {0}.{1}", batchRequest.m_callback.Target.GetType().Name, batchRequest.m_callback.Method.Name);
		}
		m_cacheRequests.Add(batchRequest);
		NetCacheCheckRequest(batchRequest);
	}

	private void NetCacheUse_Internal(NetCacheBatchRequest request, Type type)
	{
		if (request != null && request.m_requests.ContainsKey(type))
		{
			Log.Net.Print($"NetCache ...SKIP {type.Name}");
			return;
		}
		if (m_netCache.ContainsKey(type) && m_netCache[type] != null)
		{
			Log.Net.Print($"NetCache ...USE {type.Name}");
			return;
		}
		Log.Net.Print($"NetCache <<<GET {type.Name}");
		RequestNetCacheObject(type);
	}

	private void RequestNetCacheObject(Type type)
	{
		if (!m_inTransitRequests.Contains(type))
		{
			m_inTransitRequests.Add(type);
			Network.Get().RequestNetCacheObject(m_getAccountInfoTypeMap[type]);
		}
	}

	private void NetCacheReload_Internal(NetCacheBatchRequest request, Type type)
	{
		m_netCache[type] = null;
		if (type == typeof(NetCacheProfileNotices))
		{
			Debug.LogError("NetCacheReload_Internal - tried to issue request with type NetCacheProfileNotices - this is no longer allowed!");
		}
		else
		{
			NetCacheUse_Internal(request, type);
		}
	}

	private void NetCacheCheckRequest(NetCacheBatchRequest request)
	{
		foreach (KeyValuePair<Type, Request> entry in request.m_requests)
		{
			if (!m_netCache.ContainsKey(entry.Key) || m_netCache[entry.Key] == null)
			{
				return;
			}
		}
		request.m_canTimeout = false;
		if (request.m_callback != null)
		{
			request.m_callback();
		}
	}

	private void UpdateRequestNeedState(Type type, RequestResult result)
	{
		foreach (NetCacheBatchRequest request in m_cacheRequests)
		{
			if (request.m_requests.ContainsKey(type))
			{
				request.m_requests[type].m_result = result;
			}
		}
	}

	private void OnNetCacheObjReceived<T>(T netCacheObject)
	{
		Type type = typeof(T);
		Log.Net.Print($"OnNetCacheObjReceived SAVE --> {type.Name}");
		UpdateRequestNeedState(type, RequestResult.DATA_COMPLETE);
		m_netCache[type] = netCacheObject;
		m_inTransitRequests.Remove(type);
		NetCacheChanged<T>();
		if (m_updatedListeners.TryGetValue(type, out var listeners))
		{
			Action[] array = listeners.ToArray();
			for (int i = 0; i < array.Length; i++)
			{
				array[i]();
			}
		}
	}

	public void Clear()
	{
		Log.Net.PrintDebug("Clearing NetCache");
		m_netCache.Clear();
		m_prevHeroLevels = null;
		m_previousMedalInfo = null;
		m_changeRequests.Clear();
		m_cacheRequests.Clear();
		m_inTransitRequests.Clear();
		m_receivedInitialClientState = false;
		m_ackedNotices.Clear();
		m_queuedProfileNotices.Clear();
		m_receivedInitialProfileNotices = false;
		m_currencyVersion = 0L;
		m_initialCollectionVersion = 0L;
		m_expectedCardModifications.Clear();
		m_handledCardModifications.Clear();
		if (HearthstoneApplication.IsInternal() && ServiceManager.TryGet<SceneDebugger>(out var sceneDebugger))
		{
			sceneDebugger.SetPlayerId(null);
		}
	}

	public void ClearForNewAuroraConnection()
	{
		m_cacheRequests.Clear();
		m_inTransitRequests.Clear();
		m_receivedInitialClientState = false;
	}

	public void UnregisterNetCacheHandler(NetCacheCallback handler)
	{
		m_cacheRequests.RemoveAll((NetCacheBatchRequest o) => o.m_callback == handler);
	}

	public void Update()
	{
		if (!Network.IsRunning())
		{
			return;
		}
		m_cacheRequestScratchList.Clear();
		m_cacheRequestScratchList.AddRange(m_cacheRequests);
		float now = Time.realtimeSinceStartup;
		foreach (NetCacheBatchRequest request in m_cacheRequestScratchList)
		{
			if (!request.m_canTimeout || now - request.m_timeAdded < Network.GetMaxDeferredWait() || Network.Get().HaveUnhandledPackets())
			{
				continue;
			}
			request.m_canTimeout = false;
			if (m_fatalErrorCodeSet)
			{
				continue;
			}
			ErrorInfo info = new ErrorInfo();
			info.Error = ErrorCode.TIMEOUT;
			info.RequestingFunction = request.m_requestFunc;
			info.RequestedTypes = new Map<Type, Request>(request.m_requests);
			info.RequestStackTrace = request.m_requestStackTrace;
			string code = "CT";
			int count = 0;
			foreach (KeyValuePair<Type, Request> req in request.m_requests)
			{
				RequestResult result = req.Value.m_result;
				if ((uint)(result - 3) > 1u)
				{
					string[] tokens = req.Value.m_type.ToString().Split('+');
					if (tokens.GetLength(0) != 0)
					{
						string typeName = tokens[tokens.GetLength(0) - 1];
						string[] obj = new string[5] { code, ";", typeName, "=", null };
						int result2 = (int)req.Value.m_result;
						obj[4] = result2.ToString();
						code = string.Concat(obj);
						count++;
					}
				}
				if (count >= 3)
				{
					break;
				}
			}
			FatalErrorMgr.Get().SetErrorCode("HS", code);
			m_fatalErrorCodeSet = true;
			request.m_errorCallback(info);
		}
		CheckSeasonForRoll();
	}

	private void OnGenericResponse()
	{
		Network.GenericResponse genericResponse = Network.Get().GetGenericResponse();
		if (genericResponse == null)
		{
			Debug.LogError($"NetCache - GenericResponse parse error");
		}
		else
		{
			if ((long)genericResponse.RequestId != 201)
			{
				return;
			}
			if (!m_requestTypeMap.TryGetValue((GetAccountInfo.Request)genericResponse.RequestSubId, out var requestType))
			{
				Debug.LogError($"NetCache - Ignoring unexpected requestId={genericResponse.RequestId}:{genericResponse.RequestSubId}");
				return;
			}
			NetCacheBatchRequest[] array = m_cacheRequests.ToArray();
			foreach (NetCacheBatchRequest request in array)
			{
				if (!request.m_requests.ContainsKey(requestType))
				{
					continue;
				}
				switch (genericResponse.ResultCode)
				{
				case Network.GenericResponse.Result.RESULT_REQUEST_IN_PROCESS:
					if (RequestResult.PENDING == request.m_requests[requestType].m_result)
					{
						request.m_requests[requestType].m_result = RequestResult.IN_PROCESS;
					}
					continue;
				case Network.GenericResponse.Result.RESULT_REQUEST_COMPLETE:
					request.m_requests[requestType].m_result = RequestResult.GENERIC_COMPLETE;
					Debug.LogWarning($"GenericResponse Success for requestId={genericResponse.RequestId}:{genericResponse.RequestSubId}");
					continue;
				case Network.GenericResponse.Result.RESULT_DATA_MIGRATION_REQUIRED:
					request.m_requests[requestType].m_result = RequestResult.MIGRATION_REQUIRED;
					Debug.LogWarning($"GenericResponse player migration required code={(int)genericResponse.ResultCode} {genericResponse.ResultCode.ToString()} for requestId={genericResponse.RequestId}:{genericResponse.RequestSubId}");
					continue;
				}
				Debug.LogError($"Unhandled failure code={(int)genericResponse.ResultCode} {genericResponse.ResultCode.ToString()} for requestId={genericResponse.RequestId}:{genericResponse.RequestSubId}");
				request.m_requests[requestType].m_result = RequestResult.ERROR;
				ErrorInfo info = new ErrorInfo();
				info.Error = ErrorCode.SERVER;
				info.ServerError = (uint)genericResponse.ResultCode;
				info.RequestingFunction = request.m_requestFunc;
				info.RequestedTypes = new Map<Type, Request>(request.m_requests);
				info.RequestStackTrace = request.m_requestStackTrace;
				FatalErrorMgr.Get().SetErrorCode("HS", "CG" + genericResponse.ResultCode, genericResponse.RequestId.ToString(), genericResponse.RequestSubId.ToString());
				request.m_errorCallback(info);
			}
		}
	}

	private void OnDBAction()
	{
		Network.DBAction dbAction = Network.Get().GetDbAction();
		if (Network.DBAction.ResultType.SUCCESS != dbAction.Result)
		{
			Debug.LogError($"Unhandled dbAction {dbAction.Action} with error {dbAction.Result}");
		}
	}

	private void OnInitialClientState()
	{
		InitialClientState packet = Network.Get().GetInitialClientState();
		if (packet == null)
		{
			return;
		}
		m_receivedInitialClientState = true;
		if (packet.HasGuardianVars)
		{
			OnGuardianVars(packet.GuardianVars);
		}
		if (packet.HasPlayerProfileProgress)
		{
			NetCacheProfileProgress profileProgress = new NetCacheProfileProgress
			{
				CampaignProgress = (TutorialProgress)packet.PlayerProfileProgress.Progress,
				BestForgeWins = packet.PlayerProfileProgress.BestForge,
				LastForgeDate = (packet.PlayerProfileProgress.HasLastForge ? TimeUtils.PegDateToFileTimeUtc(packet.PlayerProfileProgress.LastForge) : 0),
				TutorialComplete = packet.PlayerProfileProgress.TutorialComplete
			};
			OnNetCacheObjReceived(profileProgress);
		}
		if (packet.GameSaveData != null)
		{
			GameSaveDataManager.Get().ApplyGameSaveDataFromInitialClientState();
		}
		if (packet.SpecialEventTiming.Count > 0)
		{
			long devTimeOffsetSeconds = (packet.HasDevTimeOffsetSeconds ? packet.DevTimeOffsetSeconds : 0);
			EventTimingManager.Get().InitEventTimingsFromServer(devTimeOffsetSeconds, packet.SpecialEventTiming);
		}
		if (packet.HasClientOptions)
		{
			OnClientOptions(packet.ClientOptions);
		}
		OfflineDataCache.OfflineData data = OfflineDataCache.ReadOfflineDataFromFile();
		if (packet.HasCollection)
		{
			OnCollection(ref data, packet.Collection);
		}
		else
		{
			OnCollection(ref data, data.Collection);
		}
		if (packet.HasAchievements)
		{
			AchieveManager.Get().OnInitialAchievements(packet.Achievements);
		}
		if (packet.HasNotices)
		{
			OnInitialClientState_ProfileNotices(packet.Notices);
		}
		if (packet.HasGameCurrencyStates)
		{
			OnCurrencyState(packet.GameCurrencyStates);
		}
		if (packet.HasBoosters)
		{
			OnBoosters(packet.Boosters);
		}
		if (packet.HasPlayerDraftTickets)
		{
			OnPlayerDraftTickets(packet.PlayerDraftTickets);
		}
		foreach (TavernBrawlInfo info in packet.TavernBrawlsList)
		{
			PegasusPacket simulatedPacket = new PegasusPacket(316, 0, info);
			Network.Get().SimulateReceivedPacketFromServer(simulatedPacket);
		}
		if (packet.HasDisconnectedGame)
		{
			OnDisconnectedGame(packet.DisconnectedGame);
		}
		if (packet.HasArenaSession)
		{
			PegasusPacket simulatedPacket2 = new PegasusPacket(351, 0, packet.ArenaSession);
			Network.Get().SimulateReceivedPacketFromServer(simulatedPacket2);
		}
		if (packet.HasDisplayBanner)
		{
			OnDisplayBanner(packet.DisplayBanner);
		}
		if (packet.Decks != null)
		{
			OnReceivedDeckHeaders_InitialClientState(ref data, packet.Decks, packet.DeckContents, packet.ValidCachedDeckIds);
		}
		OfflineDataCache.WriteOfflineDataToFile(data);
		if (packet.MedalInfo != null)
		{
			OnMedalInfo(packet.MedalInfo);
		}
		if (HearthstoneApplication.IsInternal() && packet.HasPlayerId)
		{
			if (!ServiceManager.TryGet<SceneDebugger>(out var sceneDebugger))
			{
				return;
			}
			sceneDebugger.SetPlayerId(packet.PlayerId);
		}
		if (Network.Get() != null)
		{
			Network.Get().OnInitialClientStateProcessed();
		}
	}

	public void OnCollection(ref OfflineDataCache.OfflineData data, Collection collection)
	{
		m_initialCollectionVersion = collection.CollectionVersion;
		if (CollectionManager.Get() != null)
		{
			OnNetCacheObjReceived(CollectionManager.Get().OnInitialCollectionReceived(collection));
		}
		OfflineDataCache.CacheCollection(ref data, collection);
	}

	private void OnBoosters(Boosters boosters)
	{
		NetCacheBoosters result = new NetCacheBoosters();
		for (int i = 0; i < boosters.List.Count; i++)
		{
			BoosterInfo booster = boosters.List[i];
			BoosterStack boosterStackToInsert = new BoosterStack
			{
				Id = booster.Type,
				Count = booster.Count,
				EverGrantedCount = booster.EverGrantedCount
			};
			result.BoosterStacks.Add(boosterStackToInsert);
		}
		OnNetCacheObjReceived(result);
	}

	public void OnPlayerDraftTickets(PlayerDraftTickets playerDraftTickets)
	{
		NetPlayerArenaTickets result = new NetPlayerArenaTickets();
		result.Balance = playerDraftTickets.UnusedTicketBalance;
		OnNetCacheObjReceived(result);
	}

	private void OnDisconnectedGame(GameConnectionInfo packet)
	{
		if (packet.HasAddress)
		{
			NetCacheDisconnectedGame dcGame = new NetCacheDisconnectedGame();
			dcGame.ServerInfo = new GameServerInfo();
			dcGame.ServerInfo.Address = packet.Address;
			dcGame.ServerInfo.GameHandle = (uint)packet.GameHandle;
			dcGame.ServerInfo.ClientHandle = packet.ClientHandle;
			dcGame.ServerInfo.Port = (uint)packet.Port;
			dcGame.ServerInfo.AuroraPassword = packet.AuroraPassword;
			dcGame.ServerInfo.Mission = packet.Scenario;
			dcGame.ServerInfo.BrawlLibraryItemId = packet.BrawlLibraryItemId;
			dcGame.ServerInfo.Version = BattleNet.GetVersion();
			dcGame.GameType = packet.GameType;
			dcGame.FormatType = packet.FormatType;
			if (packet.HasLoadGameState)
			{
				dcGame.LoadGameState = packet.LoadGameState;
			}
			else
			{
				dcGame.LoadGameState = false;
			}
			OnNetCacheObjReceived(dcGame);
		}
	}

	private void OnDisplayBanner(int displayBanner)
	{
		NetCacheDisplayBanner result = new NetCacheDisplayBanner();
		result.Id = displayBanner;
		OnNetCacheObjReceived(result);
	}

	private void OnReceivedDeckHeaders()
	{
		NetCacheDecks netCacheDecks = Network.Get().GetDeckHeaders();
		OnNetCacheObjReceived(netCacheDecks);
	}

	private void OnReceivedDeckHeaders_InitialClientState(ref OfflineDataCache.OfflineData data, List<DeckInfo> deckHeaders, List<DeckContents> deckContents, List<long> validCachedDeckIds)
	{
		foreach (DeckInfo fakeDeckHeader in OfflineDataCache.GetFakeDeckInfos(data))
		{
			deckHeaders.Add(fakeDeckHeader);
		}
		NetCacheDecks netCacheDecks = Network.GetDeckHeaders(deckHeaders);
		OnNetCacheObjReceived(netCacheDecks);
		Network.Get().ReconcileDeckContentsForChangedOfflineDecks(ref data, deckHeaders, deckContents, validCachedDeckIds);
		CollectionManager.Get().OnInitialClientStateDeckContents(netCacheDecks, data.LocalDeckContents);
	}

	public List<DeckInfo> GetDeckListFromNetCache()
	{
		List<DeckInfo> deckInfoList = new List<DeckInfo>();
		foreach (DeckHeader deckHeader in GetNetObject<NetCacheDecks>().Decks)
		{
			deckInfoList.Add(Network.GetDeckInfoFromDeckHeader(deckHeader));
		}
		return deckInfoList;
	}

	private void OnCardValues()
	{
		NetCacheCardValues cardValues = Get().GetNetObject<NetCacheCardValues>();
		CardValues packet = Network.Get().GetCardValues();
		if (packet != null)
		{
			if (cardValues == null)
			{
				cardValues = new NetCacheCardValues(packet.Cards.Count);
			}
			EventTimingManager EventTimingManager = EventTimingManager.Get();
			foreach (PegasusUtil.CardValue card in packet.Cards)
			{
				string cardId = GameUtils.TranslateDbIdToCardId(card.Card.Asset);
				if (cardId == null)
				{
					Log.All.PrintError("NetCache.OnCardValues(): Cannot find card '{0}' in card manifest.  Confirm your card manifest matches your game server's database.", card.Card.Asset);
					continue;
				}
				CardDefinition key = new CardDefinition
				{
					Name = cardId,
					Premium = (TAG_PREMIUM)card.Card.Premium
				};
				CardValue value = new CardValue
				{
					BaseBuyValue = card.Buy,
					BaseSellValue = card.Sell,
					BaseUpgradeValue = card.Upgrade,
					BuyValueOverride = (card.HasBuyValueOverride ? card.BuyValueOverride : 0),
					SellValueOverride = (card.HasSellValueOverride ? card.SellValueOverride : 0),
					OverrideEvent = (card.HasOverrideEventName ? EventTimingManager.GetEventType(card.OverrideEventName) : EventTimingType.SPECIAL_EVENT_NEVER)
				};
				if (cardValues.Values.ContainsKey(key))
				{
					Log.All.PrintError("NetCache.OnCardValues(): An item with the same key has already been added with cardId '{0}', premium '{1}'.", cardId, card.Card.Premium);
				}
				else
				{
					cardValues.Values.Add(key, value);
				}
			}
		}
		else if (cardValues == null)
		{
			cardValues = new NetCacheCardValues();
		}
		OnNetCacheObjReceived(cardValues);
	}

	private void OnMedalInfo()
	{
		NetCacheMedalInfo medalInfo = Network.Get().GetMedalInfo();
		if (m_previousMedalInfo != null)
		{
			medalInfo.PreviousMedalInfo = m_previousMedalInfo.Clone();
		}
		m_previousMedalInfo = medalInfo;
		OnNetCacheObjReceived(medalInfo);
	}

	private void OnMedalInfo(MedalInfo packet)
	{
		NetCacheMedalInfo medalInfo = new NetCacheMedalInfo(packet);
		if (m_previousMedalInfo != null)
		{
			medalInfo.PreviousMedalInfo = m_previousMedalInfo.Clone();
		}
		m_previousMedalInfo = medalInfo;
		OnNetCacheObjReceived(medalInfo);
	}

	private void OnBaconRatingInfo()
	{
		NetCacheBaconRatingInfo baconRatingInfo = Network.Get().GetBaconRatingInfo();
		OnNetCacheObjReceived(baconRatingInfo);
	}

	private void OnPVPDRStatsInfo()
	{
		NetCachePVPDRStatsInfo PVPDRStatsInfo = Network.Get().GetPVPDRStatsInfo();
		OnNetCacheObjReceived(PVPDRStatsInfo);
	}

	private void OnLettuceMapResponse()
	{
		LettuceMapResponse response = Network.Get().GetLettuceMapResponse();
		NetCacheLettuceMap lettuceMap = new NetCacheLettuceMap
		{
			Map = response.Map
		};
		OnNetCacheObjReceived(lettuceMap);
	}

	private Dictionary<MercenaryBuilding.Mercenarybuildingtype, bool> MakeBuildingEnabledMap(MercenariesOperabilityData opData)
	{
		if (opData == null)
		{
			return new Dictionary<MercenaryBuilding.Mercenarybuildingtype, bool>
			{
				{
					MercenaryBuilding.Mercenarybuildingtype.BUILDINGMANAGER,
					false
				},
				{
					MercenaryBuilding.Mercenarybuildingtype.COLLECTION,
					false
				},
				{
					MercenaryBuilding.Mercenarybuildingtype.MAILBOX,
					false
				},
				{
					MercenaryBuilding.Mercenarybuildingtype.PVEZONES,
					false
				},
				{
					MercenaryBuilding.Mercenarybuildingtype.PVP,
					false
				},
				{
					MercenaryBuilding.Mercenarybuildingtype.SHOP,
					false
				},
				{
					MercenaryBuilding.Mercenarybuildingtype.TASKBOARD,
					false
				},
				{
					MercenaryBuilding.Mercenarybuildingtype.TRAININGHALL,
					false
				}
			};
		}
		return new Dictionary<MercenaryBuilding.Mercenarybuildingtype, bool>
		{
			{
				MercenaryBuilding.Mercenarybuildingtype.BUILDINGMANAGER,
				!opData.HasBuildingManagementEnabled || opData.BuildingManagementEnabled
			},
			{
				MercenaryBuilding.Mercenarybuildingtype.COLLECTION,
				!opData.HasCollectionPortalEnabled || opData.CollectionPortalEnabled
			},
			{
				MercenaryBuilding.Mercenarybuildingtype.MAILBOX,
				!opData.HasInGameMessagingEnabled || opData.InGameMessagingEnabled
			},
			{
				MercenaryBuilding.Mercenarybuildingtype.PVEZONES,
				!opData.HasPvePortalEnabled || opData.PvePortalEnabled
			},
			{
				MercenaryBuilding.Mercenarybuildingtype.PVP,
				!opData.HasPvpPortalEnabled || opData.PvpPortalEnabled
			},
			{
				MercenaryBuilding.Mercenarybuildingtype.SHOP,
				!opData.HasShopPortalEnabled || opData.ShopPortalEnabled
			},
			{
				MercenaryBuilding.Mercenarybuildingtype.TASKBOARD,
				!opData.HasTasksEnabled || opData.TasksEnabled
			},
			{
				MercenaryBuilding.Mercenarybuildingtype.TRAININGHALL,
				!opData.HasTrainingHallEnabled || opData.TrainingHallEnabled
			}
		};
	}

	private void OnMercenariesPlayerInfoResponse()
	{
		MercenariesPlayerInfoResponse response = Network.Get().MercenariesPlayerInfoResponse();
		if (response == null)
		{
			Log.CollectionManager.PrintError("OnMercenariesPlayerInfoResponse(): No response received.");
			return;
		}
		if (!response.HasPvpRewardChestWinsProgress)
		{
			Log.CollectionManager.PrintError("OnMercenariesPlayerInfoResponse(): No pvp reward chest wins progress received.");
			return;
		}
		if (!response.HasPvpRewardChestWinsRequired)
		{
			Log.CollectionManager.PrintError("OnMercenariesPlayerInfoResponse(): No pvp reward chest wins required received.");
			return;
		}
		Dictionary<int, NetCacheMercenariesPlayerInfo.BountyInfo> bountyInfoMap = new Dictionary<int, NetCacheMercenariesPlayerInfo.BountyInfo>();
		foreach (MercenariesPlayerBountyInfo responseBountyInfo in response.BountyInfoList.BountyInfo)
		{
			NetCacheMercenariesPlayerInfo.BountyInfo bountyInfo = new NetCacheMercenariesPlayerInfo.BountyInfo
			{
				FewestTurns = (int)responseBountyInfo.FewestTurns,
				Completions = (int)responseBountyInfo.Completions,
				IsComplete = responseBountyInfo.IsComplete,
				IsAcknowledged = responseBountyInfo.Acknowledged,
				BossCardIds = (responseBountyInfo.HasBossCards ? responseBountyInfo.BossCards.BossCardId : null),
				MaxMythicLevel = (int)responseBountyInfo.MaxMythicLevel,
				UnlockTime = (responseBountyInfo.HasUnlockUnixTime ? new DateTime?(TimeUtils.UnixTimeStampToDateTimeUtc(responseBountyInfo.UnlockUnixTime)) : ((DateTime?)null))
			};
			bountyInfoMap.Add((int)responseBountyInfo.BountyId, bountyInfo);
		}
		MercenariesOperabilityData opData = (response.HasOperabilityData ? response.OperabilityData : null);
		NetCacheMercenariesPlayerInfo playerInfo = new NetCacheMercenariesPlayerInfo
		{
			PvpRating = response.PvpRating,
			PvpRewardChestWinsProgress = response.PvpRewardChestWinsProgress,
			PvpRewardChestWinsRequired = response.PvpRewardChestWinsRequired,
			BountyInfoMap = bountyInfoMap,
			BuildingEnabledMap = MakeBuildingEnabledMap(opData),
			DisabledMercenaryList = (opData?.DisabledMercenaryId ?? new List<int>()),
			DisabledVisitorList = new HashSet<int>(opData?.DisabledVisitorId ?? new List<int>()),
			DisabledBuildingTierUpgradeList = (opData?.DisabledBuildingTierUpgradeId ?? new List<int>()),
			PvpSeasonHighestRating = response.PvpSeasonHighestRating,
			PvpSeasonId = response.PvpSeasonId,
			CurrentMythicBountyLevel = (int)(response.MythicBountyLevelInfo?.CurrentMythicBountyLevel ?? 0),
			MinMythicBountyLevel = (int)(response.MythicBountyLevelInfo?.MinMythicBountyLevel ?? 0),
			MaxMythicBountyLevel = (int)(response.MythicBountyLevelInfo?.MaxMythicBountyLevel ?? 0),
			GeneratedBountyResetTime = TimeUtils.UnixTimeStampToDateTimeUtc(response.GeneratedBountyResetTime)
		};
		OnNetCacheObjReceived(playerInfo);
	}

	public void UpdateNetCachePlayerInfoAcknowledgedBounties(List<int> bountiesToAcknowledge)
	{
		NetCacheMercenariesPlayerInfo playerInfo = Get().GetNetObject<NetCacheMercenariesPlayerInfo>();
		foreach (int bountyID in bountiesToAcknowledge)
		{
			if (playerInfo.BountyInfoMap.ContainsKey(bountyID))
			{
				playerInfo.BountyInfoMap[bountyID].IsAcknowledged = true;
				continue;
			}
			NetCacheMercenariesPlayerInfo.BountyInfo newBountyInfo = new NetCacheMercenariesPlayerInfo.BountyInfo();
			newBountyInfo.IsAcknowledged = true;
			newBountyInfo.IsComplete = false;
			newBountyInfo.Completions = 0;
			newBountyInfo.FewestTurns = 0;
			newBountyInfo.BossCardIds = null;
			playerInfo.BountyInfoMap[bountyID] = newBountyInfo;
		}
		OnNetCacheObjReceived(playerInfo);
	}

	private void OnMercenariesPvPWinUpdate()
	{
		MercenariesPvPWinUpdate response = Network.Get().MercenariesPvPWinUpdate();
		if (response == null)
		{
			Log.CollectionManager.PrintError("OnMercenariesPvPWinUpdate(): No response received.");
			return;
		}
		if (!response.HasPvpRewardChestWinsProgress)
		{
			Log.CollectionManager.PrintError("OnMercenariesPvPWinUpdate(): No pvp reward chest wins progress received.");
			return;
		}
		if (!response.HasPvpRewardChestWinsRequired)
		{
			Log.CollectionManager.PrintError("OnMercenariesPvPWinUpdate(): No pvp reward chest wins required received.");
			return;
		}
		NetCacheMercenariesPlayerInfo playerInfo = Get().GetNetObject<NetCacheMercenariesPlayerInfo>();
		if (playerInfo != null)
		{
			playerInfo.PvpRewardChestWinsProgress = response.PvpRewardChestWinsProgress;
			playerInfo.PvpRewardChestWinsRequired = response.PvpRewardChestWinsRequired;
			OnNetCacheObjReceived(playerInfo);
		}
	}

	private void OnMercenariesPlayerBountyInfoUpdate()
	{
		MercenariesPlayerBountyInfoUpdate response = Network.Get().MercenariesPlayerBountyInfoUpdate();
		if (response == null)
		{
			Log.CollectionManager.PrintError("OnMercenariesPlayerBountyInfoUpdate(): No response received.");
			return;
		}
		NetCacheMercenariesPlayerInfo playerInfo = Get().GetNetObject<NetCacheMercenariesPlayerInfo>();
		if (playerInfo == null)
		{
			Log.CollectionManager.PrintError("OnMercenariesPlayerBountyInfoUpdate(): No player info.");
			return;
		}
		if (playerInfo.BountyInfoMap == null)
		{
			playerInfo.BountyInfoMap = new Dictionary<int, NetCacheMercenariesPlayerInfo.BountyInfo>();
		}
		if (response.BountyInfo == null)
		{
			Log.CollectionManager.PrintError("OnMercenariesPlayerBountyInfoUpdate(): No bounty info.");
			return;
		}
		playerInfo.BountyInfoMap.TryGetValue((int)response.BountyInfo.BountyId, out var bountyInfo);
		if (bountyInfo != null)
		{
			if (bountyInfo.FewestTurns == 0)
			{
				bountyInfo.FewestTurns = (int)response.BountyInfo.FewestTurns;
			}
			else if (response.BountyInfo.FewestTurns != 0)
			{
				bountyInfo.FewestTurns = Math.Min(bountyInfo.FewestTurns, (int)response.BountyInfo.FewestTurns);
			}
			bountyInfo.Completions = Math.Max(bountyInfo.Completions, (int)response.BountyInfo.Completions);
			bountyInfo.IsComplete = bountyInfo.IsComplete || response.BountyInfo.IsComplete;
			bountyInfo.IsAcknowledged = bountyInfo.IsAcknowledged || response.BountyInfo.IsAcknowledged;
			if (response.BountyInfo.HasBossCards)
			{
				bountyInfo.BossCardIds = response.BountyInfo.BossCards.BossCardId;
			}
			bountyInfo.MaxMythicLevel = (int)response.BountyInfo.MaxMythicLevel;
			bountyInfo.UnlockTime = (response.BountyInfo.HasUnlockUnixTime ? new DateTime?(TimeUtils.UnixTimeStampToDateTimeUtc(response.BountyInfo.UnlockUnixTime)) : ((DateTime?)null));
		}
		else
		{
			playerInfo.BountyInfoMap[(int)response.BountyInfo.BountyId] = new NetCacheMercenariesPlayerInfo.BountyInfo
			{
				FewestTurns = (int)response.BountyInfo.FewestTurns,
				Completions = (int)response.BountyInfo.Completions,
				IsComplete = response.BountyInfo.IsComplete,
				IsAcknowledged = response.BountyInfo.IsAcknowledged,
				BossCardIds = (response.BountyInfo.HasBossCards ? response.BountyInfo.BossCards.BossCardId : null),
				MaxMythicLevel = (int)response.BountyInfo.MaxMythicLevel,
				UnlockTime = (response.BountyInfo.HasUnlockUnixTime ? new DateTime?(TimeUtils.UnixTimeStampToDateTimeUtc(response.BountyInfo.UnlockUnixTime)) : ((DateTime?)null))
			};
		}
		OnNetCacheObjReceived(playerInfo);
	}

	private static int CompareVisitorStates(MercenariesVisitorState x, MercenariesVisitorState y)
	{
		MercenaryVisitorDbfRecord recordX = LettuceVillageDataUtil.GetVisitorRecordByID(x.VisitorId);
		MercenaryVisitorDbfRecord recordY = LettuceVillageDataUtil.GetVisitorRecordByID(y.VisitorId);
		if (recordX == null || recordY == null)
		{
			return 0;
		}
		if (recordX.VisitorType > recordY.VisitorType)
		{
			return -1;
		}
		if (recordY.VisitorType > recordX.VisitorType)
		{
			return 1;
		}
		long utcX = TimeUtils.PegDateToFileTimeUtc(x.LastArrivalDate);
		return TimeUtils.PegDateToFileTimeUtc(y.LastArrivalDate).CompareTo(utcX);
	}

	private void OnMercenariesBountyAcknowledgeResponse()
	{
		Network.Get().AcknowledgeBountiesResponse();
	}

	private void OnVillageDataResponse()
	{
		MercenariesGetVillageResponse response = Network.Get().MercenariesVillageStatusResponse();
		NetCacheMercenariesVillageInfo villageInfo = new NetCacheMercenariesVillageInfo();
		NetCacheMercenariesVillageVisitorInfo visitorInfo = new NetCacheMercenariesVillageVisitorInfo();
		villageInfo.Initialized = true;
		if (response == null)
		{
			Log.CollectionManager.PrintError("OnVillageDataResponse(): No response received.");
			OnNetCacheObjReceived(villageInfo);
			OnNetCacheObjReceived(visitorInfo);
			return;
		}
		if (!response.Success)
		{
			Debug.LogError("Failed to load village data");
		}
		visitorInfo.VisitorStates = response.Visitor ?? new List<MercenariesVisitorState>();
		visitorInfo.VisitorStates.Sort(CompareVisitorStates);
		visitorInfo.CompletedTasks = new List<MercenariesTaskState>();
		visitorInfo.CompletedVisitorStates = response.CompletedVisitor ?? new List<MercenariesCompletedVisitorState>();
		visitorInfo.ActiveRenownOffers = response.RenownOffer ?? new List<MercenariesRenownOfferData>();
		CollectVisitingMercenariesFromVisitorStates(visitorInfo);
		villageInfo.BuildingStates = new List<MercenariesBuildingState>();
		foreach (MercenariesBuildingState state in response.Building)
		{
			if (IsBuildingStateValid(state))
			{
				villageInfo.TrySetDifficultyUnlock(state);
				villageInfo.BuildingStates.Add(state);
			}
		}
		villageInfo.CacheTierTree();
		villageInfo.CacheRenownConversionRates(response.RenownConversionRate);
		OnNetCacheObjReceived(villageInfo);
		OnNetCacheObjReceived(visitorInfo);
		NarrativeManager.Get().PreloadDialogForActiveVillageBuildings();
		GameSaveDataManager.Get().ApplyGameSaveDataUpdate(response.GameSaveData);
	}

	private void OnVillageVisitorStateUpdated()
	{
		MercenariesVisitorStateUpdate response = Network.Get().MercenariesVisitorStateUpdate();
		if (response == null)
		{
			Log.CollectionManager.PrintError("OnVillageVisitorStateUpdated(): No response received.");
			return;
		}
		NetCacheMercenariesVillageVisitorInfo visitorInfo = Get().GetNetObject<NetCacheMercenariesVillageVisitorInfo>();
		if (visitorInfo == null)
		{
			visitorInfo = new NetCacheMercenariesVillageVisitorInfo
			{
				VisitorStates = new List<MercenariesVisitorState>(),
				CompletedVisitorStates = new List<MercenariesCompletedVisitorState>(),
				ActiveRenownOffers = new List<MercenariesRenownOfferData>()
			};
		}
		if (response.Visitor != null)
		{
			foreach (MercenariesVisitorState stateUpdate in response.Visitor)
			{
				if (!visitorInfo.VisitorStates.Exists((MercenariesVisitorState state) => state.VisitorId == stateUpdate.VisitorId))
				{
					visitorInfo.VisitorStates.Add(stateUpdate);
				}
				else
				{
					for (int i = visitorInfo.VisitorStates.Count - 1; i >= 0; i--)
					{
						MercenariesVisitorState visitorState = visitorInfo.VisitorStates[i];
						if (visitorState.VisitorId == stateUpdate.VisitorId)
						{
							if (stateUpdate.ActiveTaskState == null || stateUpdate.ActiveTaskState.TaskId == 0)
							{
								if (visitorState.ActiveTaskState != null)
								{
									VisitorTaskChainDbfRecord taskChain = LettuceVillageDataUtil.GetCurrentTaskChainByVisitorState(visitorState);
									if (taskChain != null && stateUpdate.TaskChainProgress >= taskChain.TaskList.Count)
									{
										visitorInfo.CompletedVisitorStates.Add(new MercenariesCompletedVisitorState
										{
											VisitorId = stateUpdate.VisitorId,
											CompletedTaskChainId = taskChain.ID
										});
									}
								}
								visitorInfo.VisitorStates.RemoveAt(i);
							}
							else
							{
								visitorInfo.VisitorStates[i] = stateUpdate;
							}
						}
					}
				}
				if (stateUpdate.HasActiveTaskState && stateUpdate.ActiveTaskState.Status_ == MercenariesTaskState.Status.COMPLETE)
				{
					if (visitorInfo.CompletedTasks == null)
					{
						visitorInfo.CompletedTasks = new List<MercenariesTaskState>();
					}
					visitorInfo.CompletedTasks.Add(stateUpdate.ActiveTaskState);
				}
			}
			visitorInfo.VisitorStates.Sort(CompareVisitorStates);
		}
		if (response.UpdatedRenownOffer != null && response.UpdatedRenownOffer.Count > 0)
		{
			foreach (MercenariesRenownOfferData updatedOffer in response.UpdatedRenownOffer)
			{
				bool found = false;
				for (int i2 = visitorInfo.ActiveRenownOffers.Count - 1; i2 >= 0; i2--)
				{
					if (visitorInfo.ActiveRenownOffers[i2].RenownOfferId == updatedOffer.RenownOfferId)
					{
						visitorInfo.ActiveRenownOffers[i2] = updatedOffer;
						found = true;
						break;
					}
				}
				if (!found)
				{
					if (visitorInfo.ActiveRenownOffers == null)
					{
						visitorInfo.ActiveRenownOffers = new List<MercenariesRenownOfferData>();
					}
					visitorInfo.ActiveRenownOffers.Add(updatedOffer);
				}
			}
		}
		if (response.RemovedRenownOfferId != null && response.RemovedRenownOfferId.Count > 0 && visitorInfo.ActiveRenownOffers != null)
		{
			for (int i3 = visitorInfo.ActiveRenownOffers.Count - 1; i3 >= 0; i3--)
			{
				MercenariesRenownOfferData activeOffer = visitorInfo.ActiveRenownOffers[i3];
				if (response.RemovedRenownOfferId.Contains(activeOffer.RenownOfferId))
				{
					visitorInfo.ActiveRenownOffers.RemoveAt(i3);
				}
			}
		}
		GameSaveDataManager.Get().ApplyGameSaveDataUpdate(response.GameSaveData);
		CollectVisitingMercenariesFromVisitorStates(visitorInfo);
		OnNetCacheObjReceived(visitorInfo);
	}

	private void CollectVisitingMercenariesFromVisitorStates(NetCacheMercenariesVillageVisitorInfo visitorInfo)
	{
		HashSet<int> mercAccumulator = new HashSet<int>();
		NetCacheMercenariesPlayerInfo playerInfo = GetNetObject<NetCacheMercenariesPlayerInfo>();
		if (playerInfo == null)
		{
			Debug.LogError("Null playerInfo in CollectVisitingMercenariesFromVisitorStates");
			return;
		}
		HashSet<int> disabledVisitors = playerInfo.DisabledVisitorList;
		if (disabledVisitors == null)
		{
			Debug.LogError("Null disabledVisitors in CollectVisitingMercenariesFromVisitorStates");
			disabledVisitors = new HashSet<int>();
		}
		if (visitorInfo.VisitorStates != null)
		{
			foreach (MercenariesVisitorState state in visitorInfo.VisitorStates)
			{
				if (disabledVisitors.Contains(state.VisitorId) || state.ActiveTaskState == null)
				{
					continue;
				}
				MercenaryVisitorDbfRecord mercRecord = LettuceVillageDataUtil.GetVisitorRecordByID(state.VisitorId);
				if (mercRecord == null)
				{
					Debug.LogError($"mercRecord {state.VisitorId} not found in CollectVisitingMercenariesFromVisitorStates");
				}
				else
				{
					if (mercRecord.VisitorType == MercenaryVisitor.VillageVisitorType.STANDARD)
					{
						continue;
					}
					VisitorTaskDbfRecord activeTaskRecord = LettuceVillageDataUtil.GetTaskRecordByID(state.ActiveTaskState.TaskId);
					if (mercRecord != null && activeTaskRecord != null)
					{
						if (mercRecord.VisitorType == MercenaryVisitor.VillageVisitorType.PROCEDURAL)
						{
							mercAccumulator.Add(state.ProceduralMercenaryId);
						}
						else
						{
							mercAccumulator.Add(LettuceVillageDataUtil.GetMercenaryIdForVisitor(mercRecord, activeTaskRecord));
						}
					}
				}
			}
		}
		if (visitorInfo.ActiveRenownOffers != null)
		{
			foreach (MercenariesRenownOfferData renownOffer in visitorInfo.ActiveRenownOffers)
			{
				if (renownOffer.MercenaryId != 0)
				{
					mercAccumulator.Add(renownOffer.MercenaryId);
				}
			}
		}
		visitorInfo.VisitingMercenaries = mercAccumulator.ToArray();
	}

	private void OnRefreshVillageDataResponse()
	{
		MercenariesRefreshVillageResponse response = Network.Get().MercenariesVillageRefreshResponse();
		if (response == null || !response.Success)
		{
			Debug.LogError($"Failed to refresh village data");
		}
		NetCacheMercenariesPlayerInfo playerInfo = GetNetObject<NetCacheMercenariesPlayerInfo>();
		bool updatedPlayerInfo = false;
		if (response.HasGeneratedBountyResetTime)
		{
			playerInfo.GeneratedBountyResetTime = TimeUtils.UnixTimeStampToDateTimeUtc(response.GeneratedBountyResetTime);
			updatedPlayerInfo = true;
		}
		if (response.HasMythicBountyLevelInfo)
		{
			playerInfo.CurrentMythicBountyLevel = (int)response.MythicBountyLevelInfo.CurrentMythicBountyLevel;
			playerInfo.MaxMythicBountyLevel = (int)response.MythicBountyLevelInfo.MaxMythicBountyLevel;
			playerInfo.MinMythicBountyLevel = (int)response.MythicBountyLevelInfo.MinMythicBountyLevel;
			updatedPlayerInfo = true;
		}
		if (response.HasBountyInfoList)
		{
			foreach (MercenariesPlayerBountyInfo bountyInfo in response.BountyInfoList.BountyInfo)
			{
				playerInfo.BountyInfoMap.TryGetValue((int)bountyInfo.BountyId, out var existingBountyInfo);
				if (existingBountyInfo != null)
				{
					existingBountyInfo.FewestTurns = (int)bountyInfo.FewestTurns;
					existingBountyInfo.Completions = (int)bountyInfo.Completions;
					existingBountyInfo.IsComplete = bountyInfo.IsComplete;
					existingBountyInfo.IsAcknowledged = bountyInfo.IsAcknowledged;
					if (bountyInfo.HasBossCards)
					{
						existingBountyInfo.BossCardIds = bountyInfo.BossCards.BossCardId;
					}
					existingBountyInfo.MaxMythicLevel = (int)bountyInfo.MaxMythicLevel;
					existingBountyInfo.UnlockTime = (bountyInfo.HasUnlockUnixTime ? new DateTime?(TimeUtils.UnixTimeStampToDateTimeUtc(bountyInfo.UnlockUnixTime)) : ((DateTime?)null));
				}
				else
				{
					playerInfo.BountyInfoMap[(int)bountyInfo.BountyId] = new NetCacheMercenariesPlayerInfo.BountyInfo
					{
						FewestTurns = (int)bountyInfo.FewestTurns,
						Completions = (int)bountyInfo.Completions,
						IsComplete = bountyInfo.IsComplete,
						IsAcknowledged = bountyInfo.IsAcknowledged,
						BossCardIds = (bountyInfo.HasBossCards ? bountyInfo.BossCards.BossCardId : null),
						MaxMythicLevel = (int)bountyInfo.MaxMythicLevel,
						UnlockTime = (bountyInfo.HasUnlockUnixTime ? new DateTime?(TimeUtils.UnixTimeStampToDateTimeUtc(bountyInfo.UnlockUnixTime)) : ((DateTime?)null))
					};
				}
			}
			updatedPlayerInfo = true;
		}
		if (updatedPlayerInfo)
		{
			OnNetCacheObjReceived(playerInfo);
		}
	}

	private bool IsBuildingStateValid(MercenariesBuildingState bldgState)
	{
		if (bldgState == null)
		{
			return false;
		}
		MercenaryBuildingDbfRecord bldgRecord = GameDbf.MercenaryBuilding.GetRecord((MercenaryBuildingDbfRecord r) => r.ID == bldgState.BuildingId);
		if (bldgRecord == null)
		{
			return false;
		}
		if (GameDbf.BuildingTier.GetRecords((BuildingTierDbfRecord r) => r.MercenaryBuildingId == bldgRecord.ID && r.ID == bldgState.CurrentTierId) == null)
		{
			return false;
		}
		return true;
	}

	private void OnVillageBuildingStateUpdated()
	{
		MercenariesBuildingStateUpdate response = Network.Get().MercenariesBuildingStateUpdate();
		NetCacheMercenariesVillageInfo villageInfo = Get().GetNetObject<NetCacheMercenariesVillageInfo>();
		foreach (MercenariesBuildingState building in response.Building)
		{
			if (!IsBuildingStateValid(building))
			{
				continue;
			}
			bool found = false;
			for (int i = 0; i < villageInfo.BuildingStates.Count; i++)
			{
				if (villageInfo.BuildingStates[i].BuildingId == building.BuildingId)
				{
					villageInfo.BuildingStates[i] = building;
					found = true;
					break;
				}
			}
			if (!found)
			{
				villageInfo.BuildingStates.Add(building);
			}
			villageInfo.TrySetDifficultyUnlock(building);
		}
		GameSaveDataManager.Get().ApplyGameSaveDataUpdate(response.GameSaveData);
		villageInfo.LastBuildingUpdate = response.Building;
		OnNetCacheObjReceived(villageInfo);
	}

	private void OnGuardianVars()
	{
		GuardianVars packet = Network.Get().GetGuardianVars();
		if (packet != null)
		{
			OnGuardianVars(packet);
		}
	}

	private void OnGuardianVars(GuardianVars packet)
	{
		NetCacheFeatures result = new NetCacheFeatures();
		result.Games.Tournament = !packet.HasTourney || packet.Tourney;
		result.Games.Practice = !packet.HasPractice || packet.Practice;
		result.Games.Casual = !packet.HasCasual || packet.Casual;
		result.Games.Forge = !packet.HasForge || packet.Forge;
		result.Games.Friendly = !packet.HasFriendly || packet.Friendly;
		result.Games.TavernBrawl = !packet.HasTavernBrawl || packet.TavernBrawl;
		result.Games.Battlegrounds = !packet.HasBattlegrounds || packet.Battlegrounds;
		result.Games.BattlegroundsFriendlyChallenge = !packet.HasBattlegroundsFriendlyChallenge || packet.BattlegroundsFriendlyChallenge;
		result.Games.BattlegroundsTutorial = !packet.HasBattlegroundsTutorial || packet.BattlegroundsTutorial;
		result.Games.BattlegroundsDuos = !packet.HasBattlegroundsDuos || packet.BattlegroundsDuos;
		result.Games.ShowUserUI = (packet.HasShowUserUI ? packet.ShowUserUI : 0);
		result.Games.Duels = !packet.HasDuels || packet.Duels;
		result.Games.PaidDuels = !packet.HasPaidDuels || packet.PaidDuels;
		result.Games.Mercenaries = !packet.HasMercenaries || packet.Mercenaries;
		result.Games.MercenariesAI = !packet.HasMercenariesAi || packet.MercenariesAi;
		result.Games.MercenariesCoOp = !packet.HasMercenariesCoop || packet.MercenariesCoop;
		result.Games.MercenariesFriendly = !packet.HasMercenariesFriendlyChallenge || packet.MercenariesFriendlyChallenge;
		result.Games.MercenariesMythic = !packet.HasMercenariesMythic || packet.MercenariesMythic;
		result.Collection.Manager = !packet.HasManager || packet.Manager;
		result.Collection.Crafting = !packet.HasCrafting || packet.Crafting;
		result.Collection.DeckReordering = !packet.HasDeckReordering || packet.DeckReordering;
		result.Collection.MultipleFavoriteCardBacks = !packet.HasMultipleFavoriteCardBacks || packet.MultipleFavoriteCardBacks;
		result.Collection.CosmeticsRenderingEnabled = packet.CosmeticsRenderingEnabled;
		result.Store.Store = !packet.HasStore || packet.Store;
		result.Store.BattlePay = !packet.HasBattlePay || packet.BattlePay;
		result.Store.BuyWithGold = !packet.HasBuyWithGold || packet.BuyWithGold;
		result.Store.SimpleCheckout = !packet.HasSimpleCheckout || packet.SimpleCheckout;
		result.Store.SoftAccountPurchasing = !packet.HasSoftAccountPurchasing || packet.SoftAccountPurchasing;
		result.Store.VirtualCurrencyEnabled = packet.HasVirtualCurrencyEnabled && packet.VirtualCurrencyEnabled;
		result.Store.NumClassicPacksUntilDeprioritize = (packet.HasNumClassicPacksUntilDeprioritize ? packet.NumClassicPacksUntilDeprioritize : (-1));
		result.Store.SimpleCheckoutIOS = !packet.HasSimpleCheckoutIos || packet.SimpleCheckoutIos;
		result.Store.SimpleCheckoutAndroidAmazon = !packet.HasSimpleCheckoutAndroidAmazon || packet.SimpleCheckoutAndroidAmazon;
		result.Store.SimpleCheckoutAndroidGoogle = !packet.HasSimpleCheckoutAndroidGoogle || packet.SimpleCheckoutAndroidGoogle;
		result.Store.SimpleCheckoutAndroidGlobal = !packet.HasSimpleCheckoutAndroidGlobal || packet.SimpleCheckoutAndroidGlobal;
		result.Store.SimpleCheckoutWin = !packet.HasSimpleCheckoutWin || packet.SimpleCheckoutWin;
		result.Store.SimpleCheckoutMac = !packet.HasSimpleCheckoutMac || packet.SimpleCheckoutMac;
		result.Store.BoosterRotatingSoonWarnDaysWithoutSale = (packet.HasBoosterRotatingSoonWarnDaysWithoutSale ? packet.BoosterRotatingSoonWarnDaysWithoutSale : 0);
		result.Store.BoosterRotatingSoonWarnDaysWithSale = (packet.HasBoosterRotatingSoonWarnDaysWithSale ? packet.BoosterRotatingSoonWarnDaysWithSale : 0);
		result.Store.BuyCardBacksFromCollectionManager = !packet.HasBuyCardBacksFromCollectionManagerEnabled || packet.BuyCardBacksFromCollectionManagerEnabled;
		result.Store.BuyHeroSkinsFromCollectionManager = !packet.HasBuyHeroSkinsFromCollectionManagerEnabled || packet.BuyHeroSkinsFromCollectionManagerEnabled;
		result.Store.LargeItemBundleDetailsEnabled = !packet.HasLargeItemBundleDetailsEnabled || packet.LargeItemBundleDetailsEnabled;
		result.Heroes.Hunter = !packet.HasHunter || packet.Hunter;
		result.Heroes.Mage = !packet.HasMage || packet.Mage;
		result.Heroes.Paladin = !packet.HasPaladin || packet.Paladin;
		result.Heroes.Priest = !packet.HasPriest || packet.Priest;
		result.Heroes.Rogue = !packet.HasRogue || packet.Rogue;
		result.Heroes.Shaman = !packet.HasShaman || packet.Shaman;
		result.Heroes.Warlock = !packet.HasWarlock || packet.Warlock;
		result.Heroes.Warrior = !packet.HasWarrior || packet.Warrior;
		result.Misc.ClientOptionsUpdateIntervalSeconds = (packet.HasClientOptionsUpdateIntervalSeconds ? packet.ClientOptionsUpdateIntervalSeconds : 0);
		result.Misc.AllowLiveFPSGathering = packet.HasAllowLiveFpsGathering && packet.AllowLiveFpsGathering;
		result.CancelMatchmakingDuringLoanerDeckGrant = packet.HasCancelMatchmakingDuringLoanerDeckGrant && packet.CancelMatchmakingDuringLoanerDeckGrant;
		result.AllowBGInviteWhileInNPPG = packet.HasAllowBgInviteInNppg && packet.AllowBgInviteInNppg;
		result.ArenaRedraftOnLossEnabled = packet.HasArenaRedraftOnLossEnabled && packet.ArenaRedraftOnLossEnabled;
		result.BoxProductBannerEnabled = packet.HasBoxProductBannerEnabled && packet.BoxProductBannerEnabled;
		result.CaisEnabledNonMobile = !packet.HasCaisEnabledNonMobile || packet.CaisEnabledNonMobile;
		result.CaisEnabledMobileChina = packet.HasCaisEnabledMobileChina && packet.CaisEnabledMobileChina;
		result.CaisEnabledMobileSouthKorea = packet.HasCaisEnabledMobileSouthKorea && packet.CaisEnabledMobileSouthKorea;
		result.SendTelemetryPresence = packet.HasSendTelemetryPresence && packet.SendTelemetryPresence;
		result.XPSoloLimit = packet.XpSoloLimit;
		result.MaxHeroLevel = packet.MaxHeroLevel;
		result.SpecialEventTimingMod = packet.EventTimingMod;
		result.FriendWeekConcederMaxDefense = packet.FriendWeekConcederMaxDefense;
		result.FriendWeekConcededGameMinTotalTurns = packet.FriendWeekConcededGameMinTotalTurns;
		result.FriendWeekAllowsTavernBrawlRecordUpdate = packet.FriendWeekAllowsTavernBrawlRecordUpdate;
		result.ArenaClosedToNewSessionsSeconds = (packet.HasArenaClosedToNewSessionsSeconds ? packet.ArenaClosedToNewSessionsSeconds : 0u);
		result.PVPDRClosedToNewSessionsSeconds = (packet.HasPvpdrClosedToNewSessionsSeconds ? packet.PvpdrClosedToNewSessionsSeconds : 0u);
		result.QuickOpenEnabled = packet.HasQuickOpenEnabled && packet.QuickOpenEnabled;
		result.ForceIosLowRes = packet.HasAllowIosHighres && !packet.AllowIosHighres;
		result.AllowOfflineClientActivity = packet.HasAllowOfflineClientActivityDesktop && packet.AllowOfflineClientActivityDesktop;
		result.EnableSmartDeckCompletion = packet.HasEnableSmartDeckCompletion && packet.EnableSmartDeckCompletion;
		result.AllowOfflineClientDeckDeletion = packet.HasAllowOfflineClientDeckDeletion && packet.AllowOfflineClientDeckDeletion;
		result.BattlegroundsEarlyAccessLicense = (packet.HasBattlegroundsEarlyAccessLicense ? packet.BattlegroundsEarlyAccessLicense : 0);
		result.BattlegroundsMaxRankedPartySize = (packet.HasBattlegroundsMaxRankedPartySize ? packet.BattlegroundsMaxRankedPartySize : PartyManager.BATTLEGROUNDS_MAX_RANKED_PARTY_SIZE_FALLBACK);
		result.JournalButtonDisabled = packet.JournalButtonDisabled;
		result.AchievementToastDisabled = packet.AchievementToastDisabled;
		result.DuelsEarlyAccessLicense = (packet.HasDuelsEarlyAccessLicense ? packet.DuelsEarlyAccessLicense : 0u);
		result.ContentstackEnabled = !packet.HasContentstackEnabled || packet.ContentstackEnabled;
		result.PersonalizedMessagesEnabled = !packet.HasPersonalizeMessagesEnabled || packet.PersonalizeMessagesEnabled;
		result.AppRatingEnabled = !packet.HasAppRatingEnabled || packet.AppRatingEnabled;
		result.AppRatingSamplingPercentage = packet.AppRatingSamplingPercentage;
		result.DuelsCardDenylist = packet.DuelsCardDenylist;
		result.ConstructedCardDenylist = packet.ConstructedCardDenylist;
		result.SideboardCardDenylist = packet.SideboardCardDenylist;
		result.TwistCardDenylist = packet.TwistCardDenylist;
		result.TwistDeckTemplateDenylist = packet.TwistDeckTemplateDenylist;
		result.StandardCardDenylist = packet.StandardCardDenylist;
		result.WildCardDenylist = packet.WildCardDenylist;
		result.TavernBrawlCardDenylist = packet.TavernBrawlCardDenylist;
		result.VFXDenylist = packet.VfxDenylist;
		result.CallSDKInterval = (packet.HasCallsdkInterval ? packet.CallsdkInterval : (-1));
		result.EnableAllOptionsIDCheck = packet.HasEnableAlloptionsIdCheck && packet.EnableAlloptionsIdCheck;
		result.TwistSeasonOverride = packet.TwistSeasonOverride;
		result.TwistScenarioOverride = packet.TwistScenarioOverride;
		if (packet.TwistHeroOverrideList != null)
		{
			result.TwistHeroicDeckHeroHealthOverrides = new Dictionary<CardDefinition, int>();
			foreach (TwistHeroOverride healthOverride in packet.TwistHeroOverrideList)
			{
				CardDefinition def = new CardDefinition
				{
					Name = GameUtils.TranslateDbIdToCardId(healthOverride.HeroCardId),
					Premium = TAG_PREMIUM.NORMAL
				};
				result.TwistHeroicDeckHeroHealthOverrides.Add(def, healthOverride.Health);
			}
		}
		result.RankedPlayEnableScenarioOverrides = packet.RankedPlayEnableScenarioOverrides;
		result.BattlegroundsSkinsEnabled = packet.BattlegroundsSkinsEnabled;
		result.BattlegroundsBoardSkinsEnabled = packet.BattlegroundsBoardSkinsEnabled;
		result.BattlegroundsFinishersEnabled = packet.BattlegroundsFinishersEnabled;
		result.BattlegroundsEmotesEnabled = packet.BattlegroundsEmotesEnabled;
		result.BattlegroundsRewardTrackEnabled = packet.BattlegroundsRewardTrackEnabled;
		switch (PlatformSettings.OS)
		{
		case OSCategory.iOS:
			result.TutorialPreviewVideosEnabled = packet.HasTutorialPreviewVideosEnabledIos && packet.TutorialPreviewVideosEnabledIos;
			break;
		case OSCategory.Android:
			result.TutorialPreviewVideosEnabled = packet.HasTutorialPreviewVideosEnabledAndroid && packet.TutorialPreviewVideosEnabledAndroid;
			break;
		case OSCategory.PC:
		case OSCategory.Mac:
			result.TutorialPreviewVideosEnabled = packet.HasTutorialPreviewVideosEnabledDesktop && packet.TutorialPreviewVideosEnabledDesktop;
			break;
		}
		result.TutorialPreviewVideosTimeout = (packet.HasTutorialPreviewVideosTimeout ? packet.TutorialPreviewVideosTimeout : NetCacheFeatures.Defaults.TutorialPreviewVideosTimeout);
		result.SkippableTutorialEnabled = packet.HasSkippableTutorialEnabled && packet.SkippableTutorialEnabled;
		result.MinHPForProgressAfterConcede = (packet.HasMinHpForProgressAfterConcede ? packet.MinHpForProgressAfterConcede : 0);
		result.MinTurnsForProgressAfterConcede = (packet.HasMinTurnsForProgressAfterConcede ? packet.MinTurnsForProgressAfterConcede : 0);
		result.BGDuosLeaverRatingPenalty = (packet.HasBgDuosLeaverRatingPenalty ? packet.BgDuosLeaverRatingPenalty : 0f);
		result.BGMinTurnsForProgressAfterConcede = (packet.HasBgMinTurnsForProgressAfterConcede ? packet.BgMinTurnsForProgressAfterConcede : 0);
		result.EnablePlayingFromMiniHand = packet.HasEnablePlayFromMiniHand && packet.EnablePlayFromMiniHand;
		result.EnableUpgradeToGolden = packet.HasUpgradeToGoldenEnabled && packet.UpgradeToGoldenEnabled;
		result.ShouldPrevalidatePastedDeckCodes = packet.HasPrevalidatePastedDeckCodesOnClient && packet.PrevalidatePastedDeckCodesOnClient;
		result.EnableClickToFixDeck = packet.HasClickToFixDeckEnabled && packet.ClickToFixDeckEnabled;
		result.LegacyCardValueCacheEnabled = packet.HasLegacyCachedCardValuesEnabled && packet.LegacyCachedCardValuesEnabled;
		result.OvercappedDecksEnabled = packet.HasOvercappedDecksEnabled && packet.OvercappedDecksEnabled;
		result.ReportPlayerEnabled = packet.HasReportPlayerEnabled && packet.ReportPlayerEnabled;
		result.LuckyDrawEnabled = packet.HasLuckyDrawEnabled && packet.LuckyDrawEnabled;
		result.BattlenetBillingFlowDisableOverride = packet.HasBattlenetBillingFlowDisableOverride && packet.BattlenetBillingFlowDisableOverride;
		result.BattlegroundsLuckyDrawDisabledCountryCode = (packet.HasBattlegroundsLuckyDrawDisabledCountryCode ? packet.BattlegroundsLuckyDrawDisabledCountryCode : "");
		result.ContinuousQuickOpenEnabled = packet.HasContinuousQuickOpenEnabled && packet.ContinuousQuickOpenEnabled;
		result.ShopButtonOnPackOpeningScreenEnabled = packet.HasShopButtonOnPackOpeningScreenEnabled && packet.ShopButtonOnPackOpeningScreenEnabled;
		result.MassPackOpeningEnabled = packet.HasMassPackOpeningEnabled && packet.MassPackOpeningEnabled;
		result.MassPackOpeningPackLimit = (packet.HasMassPackOpeningPackLimit ? packet.MassPackOpeningPackLimit : 0);
		result.MassPackOpeningGoldenPackLimit = (packet.HasMassPackOpeningGoldenPackLimit ? packet.MassPackOpeningGoldenPackLimit : 0);
		result.MassPackOpeningHooverChunkSize = (packet.HasMassPackOpeningHooverChunkSize ? packet.MassPackOpeningHooverChunkSize : 0);
		result.MassCatchupPackOpeningEnabled = packet.HasMassCatchupPackOpeningEnabled && packet.MassCatchupPackOpeningEnabled;
		result.MassCatchupPackOpeningPackLimit = (packet.HasMassCatchupPackOpeningPackLimit ? packet.MassCatchupPackOpeningPackLimit : 0);
		result.MercenariesEnableVillages = packet.HasMercenariesEnableVillage && packet.MercenariesEnableVillage;
		result.MercenariesPackOpeningEnabled = packet.HasMercenariesPackOpeningEnabled && packet.MercenariesPackOpeningEnabled;
		result.Mercenaries.FullyUpgradedStatBoostAttack = (packet.HasMercenariesFullyUpgradedStatBoostAttack ? packet.MercenariesFullyUpgradedStatBoostAttack : 0);
		result.Mercenaries.FullyUpgradedStatBoostHealth = (packet.HasMercenariesFullyUpgradedStatBoostHealth ? packet.MercenariesFullyUpgradedStatBoostHealth : 0);
		result.Mercenaries.AttackBoostPerMythicLevel = (packet.HasMercenariesAttackBoostPerMythicLevel ? packet.MercenariesAttackBoostPerMythicLevel : 0f);
		result.Mercenaries.HealthBoostPerMythicLevel = (packet.HasMercenariesHealthBoostPerMythicLevel ? packet.MercenariesHealthBoostPerMythicLevel : 0f);
		result.MercenariesTeamMaxSize = (packet.HasMercenariesMaxTeamSize ? packet.MercenariesMaxTeamSize : 6);
		result.Mercenaries.MythicAbilityRenownScaleAssetId = ((!packet.HasMercenariesAbilityRenownCostScaleAssetId) ? 1 : packet.MercenariesAbilityRenownCostScaleAssetId);
		result.Mercenaries.MythicEquipmentRenownScaleAssetId = ((!packet.HasMercenariesEquipmentRenownCostScaleAssetId) ? 1 : packet.MercenariesEquipmentRenownCostScaleAssetId);
		result.Mercenaries.MythicTreasureRenownScaleAssetId = ((!packet.HasMercenariesTreasureRenownCostScaleAssetId) ? 1 : packet.MercenariesTreasureRenownCostScaleAssetId);
		result.TracerouteEnabled = !packet.HasTracerouteEnabled || packet.TracerouteEnabled;
		result.Traceroute.MaxHops = (packet.HasTracerouteMaxHops ? packet.TracerouteMaxHops : 30);
		result.Traceroute.MessageSize = (packet.HasTracerouteMessageSize ? packet.TracerouteMessageSize : 32);
		result.Traceroute.MaxRetries = (packet.HasTracerouteMaxRetries ? packet.TracerouteMaxRetries : 3);
		result.Traceroute.TimeoutMs = (packet.HasTracerouteTimeoutMs ? packet.TracerouteTimeoutMs : 3000);
		result.Traceroute.ResolveHost = packet.HasTracerouteResolveHost && packet.TracerouteResolveHost;
		result.BattlegroundsMedalFriendListDisplayEnabled = packet.HasBattlegroundsMedalFriendListDisplayEnabled && packet.BattlegroundsMedalFriendListDisplayEnabled;
		result.RecentFriendListDisplayEnabled = packet.HasRecentFriendListDisplayEnabled && packet.RecentFriendListDisplayEnabled;
		result.EnableNDERerollSpecialCases = packet.HasNdeRerollSpecialCasesEnabled && packet.HasNdeRerollSpecialCasesEnabled;
		result.CardValueOverrideServerRegion = (packet.HasCardValueOverrideServerRegion ? packet.CardValueOverrideServerRegion : 0);
		result.ReturningPlayer.LoginCountNoticeSupressionEnabled = packet.ReturningPlayerLoginCountNotificationSuppressEnabled;
		result.ReturningPlayer.NoticeSuppressionLoginThreshold = packet.ReturningPlayerLoginCountNotificationSuppressThreshold;
		OnNetCacheObjReceived(result);
	}

	public void OnCurrencyState(GameCurrencyStates currencyState)
	{
		if (!currencyState.HasCurrencyVersion || m_currencyVersion > currencyState.CurrencyVersion)
		{
			Log.Net.PrintDebug("Ignoring currency state: {0}, (cached currency version: {1})", currencyState.ToHumanReadableString(), m_currencyVersion);
			return;
		}
		Log.Net.PrintDebug("Caching currency state: {0}", currencyState.ToHumanReadableString());
		m_currencyVersion = currencyState.CurrencyVersion;
		if (currencyState.HasArcaneDustBalance)
		{
			NetCacheArcaneDustBalance arcaneDust = GetNetObject<NetCacheArcaneDustBalance>();
			if (arcaneDust == null)
			{
				arcaneDust = new NetCacheArcaneDustBalance();
			}
			arcaneDust.Balance = currencyState.ArcaneDustBalance;
			OnNetCacheObjReceived(arcaneDust);
		}
		if (currencyState.HasCappedGoldBalance && currencyState.HasBonusGoldBalance)
		{
			NetCacheGoldBalance goldBalance = GetNetObject<NetCacheGoldBalance>();
			if (goldBalance == null)
			{
				goldBalance = new NetCacheGoldBalance();
			}
			goldBalance.CappedBalance = currencyState.CappedGoldBalance;
			goldBalance.BonusBalance = currencyState.BonusGoldBalance;
			OnNetCacheObjReceived(goldBalance);
			foreach (DelGoldBalanceListener goldBalanceListener in m_goldBalanceListeners)
			{
				goldBalanceListener(goldBalance);
			}
		}
		if (currencyState.HasRenownBalance)
		{
			NetCacheRenownBalance renown = GetNetObject<NetCacheRenownBalance>();
			if (renown == null)
			{
				renown = new NetCacheRenownBalance();
			}
			renown.Balance = currencyState.RenownBalance;
			OnNetCacheObjReceived(renown);
			foreach (DelRenownBalanceListener renownBalanceListener in m_renownBalanceListeners)
			{
				renownBalanceListener(renown);
			}
		}
		if (currencyState.HasBgTokenBalance)
		{
			NetCacheBattlegroundsTokenBalance bgToken = GetNetObject<NetCacheBattlegroundsTokenBalance>();
			if (bgToken == null)
			{
				bgToken = new NetCacheBattlegroundsTokenBalance();
			}
			bgToken.Balance = currencyState.BgTokenBalance;
			OnNetCacheObjReceived(bgToken);
			foreach (DelBattlegroundsTokenBalanceListener battlegroundsTokenBalanceListener in m_battlegroundsTokenBalanceListeners)
			{
				battlegroundsTokenBalanceListener(bgToken);
			}
		}
		if (ServiceManager.TryGet<CurrencyManager>(out var currencyManager))
		{
			currencyManager.RefreshWallet();
		}
	}

	public void OnBoosterModifications(BoosterModifications packet)
	{
		NetCacheBoosters netBoosters = Get().GetNetObject<NetCacheBoosters>();
		if (netBoosters == null)
		{
			return;
		}
		foreach (BoosterInfo boosterChange in packet.Modifications)
		{
			BoosterStack stack = netBoosters.GetBoosterStack(boosterChange.Type);
			if (stack == null)
			{
				stack = new BoosterStack
				{
					Id = boosterChange.Type
				};
				netBoosters.BoosterStacks.Add(stack);
			}
			if (boosterChange.Count > 0)
			{
				stack.EverGrantedCount += boosterChange.EverGrantedCount;
			}
			stack.Count += boosterChange.Count;
		}
		OnNetCacheObjReceived(netBoosters);
	}

	public bool AddExpectedCollectionModification(long version)
	{
		if (!m_handledCardModifications.Contains(version))
		{
			m_expectedCardModifications.Add(version);
			return true;
		}
		return false;
	}

	public void OnCollectionModification(ClientStateNotification packet)
	{
		CollectionModifications collectionModifications = packet.CollectionModifications;
		if (m_handledCardModifications.Contains(collectionModifications.CollectionVersion) || m_initialCollectionVersion >= collectionModifications.CollectionVersion)
		{
			Log.Net.PrintDebug("Ignoring redundant coolection modification (modification was v.{0}; we are v.{1})", collectionModifications.CollectionVersion, Math.Max(m_handledCardModifications.DefaultIfEmpty(0L).Max(), m_initialCollectionVersion));
			return;
		}
		OnCollectionModificationInternal(collectionModifications);
		if (packet.HasAchievementNotifications)
		{
			AchieveManager.Get().OnAchievementNotifications(packet.AchievementNotifications.AchievementNotifications_);
		}
		if (packet.HasNoticeNotifications)
		{
			Network.Get().OnNoticeNotifications(packet.NoticeNotifications);
		}
		if (packet.HasBoosterModifications)
		{
			OnBoosterModifications(packet.BoosterModifications);
		}
		if (collectionModifications.CardModifications.Count <= 0)
		{
			return;
		}
		if (CollectionManager.Get().GetCollectibleDisplay() != null && CollectionManager.Get().GetCollectibleDisplay().GetPageManager() != null)
		{
			CollectionManager.Get().GetCollectibleDisplay().GetPageManager()
				.RefreshCurrentPageContents();
			CollectionManager.Get().GetCollectibleDisplay().UpdateCurrentPageCardLocks();
		}
		if (CraftingManager.IsInitialized)
		{
			CraftingManager craftingManager = CraftingManager.Get();
			if (craftingManager != null && craftingManager.m_craftingUI != null && craftingManager.m_craftingUI.IsEnabled())
			{
				craftingManager.m_craftingUI.UpdateCraftingButtonsAndSoulboundText();
			}
		}
	}

	private void OnCollectionModificationInternal(CollectionModifications packet)
	{
		m_handledCardModifications.Add(packet.CollectionVersion);
		m_expectedCardModifications.Remove(packet.CollectionVersion);
		List<CardModification> cardModifications = packet.CardModifications;
		List<CollectionManager.CardModification> cardModifications2 = new List<CollectionManager.CardModification>();
		foreach (CardModification card in cardModifications)
		{
			Log.Net.PrintDebug("Handling card collection modification (collection version {0}): {1}", packet.CollectionVersion, card.ToHumanReadableString());
			string cardID = GameUtils.TranslateDbIdToCardId(card.AssetCardId);
			if (card.Quantity > 0)
			{
				int cardsHandled = 0;
				int cardsSeen = Math.Min(card.AmountSeen, card.Quantity);
				if (card.AmountSeen > 0)
				{
					cardModifications2.Add(new CollectionManager.CardModification
					{
						modificationType = CollectionManager.CardModification.ModificationType.Add,
						cardID = cardID,
						premium = (TAG_PREMIUM)card.Premium,
						count = cardsSeen,
						seenBefore = true
					});
					cardsHandled = cardsSeen;
				}
				int cardsLeft = card.Quantity - cardsHandled;
				if (cardsLeft > 0)
				{
					cardModifications2.Add(new CollectionManager.CardModification
					{
						modificationType = CollectionManager.CardModification.ModificationType.Add,
						cardID = cardID,
						premium = (TAG_PREMIUM)card.Premium,
						count = cardsLeft,
						seenBefore = false
					});
				}
			}
			else if (card.Quantity < 0)
			{
				cardModifications2.Add(new CollectionManager.CardModification
				{
					modificationType = CollectionManager.CardModification.ModificationType.Remove,
					cardID = cardID,
					premium = (TAG_PREMIUM)card.Premium,
					count = -1 * card.Quantity
				});
			}
		}
		CollectionManager.Get().OnCardsModified(cardModifications2);
	}

	public void OnCardBackModifications(CardBackModifications packet)
	{
		NetCacheCardBacks netCacheCardBacks = GetNetObject<NetCacheCardBacks>();
		if (netCacheCardBacks == null)
		{
			Debug.LogWarning($"NetCache.OnCardBackModifications(): trying to access NetCacheCardBacks before it's been loaded");
			return;
		}
		foreach (CardBackModification modification in packet.CardBackModifications_)
		{
			netCacheCardBacks.CardBacks.Add(modification.AssetCardBackId);
			if (modification.HasAutoSetAsFavorite && modification.AutoSetAsFavorite)
			{
				ProcessNewFavoriteCardBack(modification.AssetCardBackId);
			}
		}
	}

	public void OnBattlegroundsGuideSkinModifications(BattlegroundsGuideSkinModifications packet)
	{
		NetCacheBattlegroundsGuideSkins netCacheBattlegroundsGuideSkins = GetNetObject<NetCacheBattlegroundsGuideSkins>();
		if (netCacheBattlegroundsGuideSkins == null)
		{
			Debug.LogWarning($"NetCache.OnBattlegroundsGuideSkinModifications(): trying to access NetCacheBattlegroundsGuideSkins before it has been loaded.");
			return;
		}
		bool changed = false;
		foreach (BattlegroundsGuideSkinModification battlegroundsGuideSkinModification in packet.BattlegroundsGuideSkinModifications_)
		{
			if (!battlegroundsGuideSkinModification.HasBattlegroundsGuideSkinId)
			{
				Debug.LogWarning("NetCache.OnBattlegroundsGuideSkinModifications(): received BattlegroundsGuideSkinModification message has no BattlegroundsGuideSkinId.");
				continue;
			}
			BattlegroundsGuideSkinId? skinId = BattlegroundsGuideSkinId.FromUntrustedValue(battlegroundsGuideSkinModification.BattlegroundsGuideSkinId);
			if (!skinId.HasValue)
			{
				Debug.LogWarning("NetCache.OnBattlegroundsGuideSkinModifications(): received BattlegroundsGuideSkinModification message has invalid BattlegroundsGuideSkinId.");
			}
			else if (battlegroundsGuideSkinModification.HasAddBattlegroundsGuideSkin && battlegroundsGuideSkinModification.AddBattlegroundsGuideSkin)
			{
				netCacheBattlegroundsGuideSkins.OwnedBattlegroundsGuideSkins.Add(skinId.Value);
				if (battlegroundsGuideSkinModification.HasAutoSetAsFavorite && battlegroundsGuideSkinModification.AutoSetAsFavorite)
				{
					ProcessNewFavoriteBattlegroundsGuideSkin(skinId.Value);
				}
				netCacheBattlegroundsGuideSkins.UnseenSkinIds.Add(skinId.Value);
				changed = true;
			}
			else if (battlegroundsGuideSkinModification.HasRemoveBattlegroundsGuideSkin && battlegroundsGuideSkinModification.RemoveBattlegroundsGuideSkin)
			{
				netCacheBattlegroundsGuideSkins.OwnedBattlegroundsGuideSkins.Remove(skinId.Value);
				netCacheBattlegroundsGuideSkins.UnseenSkinIds.Remove(skinId.Value);
				changed = true;
			}
		}
		if (changed && this.OwnedBattlegroundsSkinsChanged != null)
		{
			this.OwnedBattlegroundsSkinsChanged();
		}
	}

	public void OnBattlegroundsHeroSkinModifications(BattlegroundsHeroSkinModifications packet)
	{
		NetCacheBattlegroundsHeroSkins netCacheBattlegroundsHeroSkins = GetNetObject<NetCacheBattlegroundsHeroSkins>();
		if (netCacheBattlegroundsHeroSkins == null)
		{
			Debug.LogWarning($"NetCache.OnBattlegroundsHeroSkinModifications(): trying to access NetCacheBattlegroundsHeroSkins before it has been loaded.");
			return;
		}
		bool changed = false;
		foreach (BattlegroundsHeroSkinModification battlegroundsHeroSkinModification in packet.BattlegroundsHeroSkinModifications_)
		{
			if (!battlegroundsHeroSkinModification.HasBattlegroundsHeroSkinId)
			{
				Debug.LogWarning("NetCache.OnBattlegroundsHeroSkinModifications(): received BattlegroundsHeroSkinModification message has no HasBattlegroundsHeroSkinId.");
				continue;
			}
			if (battlegroundsHeroSkinModification.BattlegroundsHeroSkinId == BaconHeroSkinUtils.SKIN_ID_FOR_FAVORITED_BASE_HERO)
			{
				Debug.LogWarning($"NetCache.OnBattlegroundsHeroSkinModifications(): Id for BaseHero (ID: {BaconHeroSkinUtils.SKIN_ID_FOR_FAVORITED_BASE_HERO}) is an invalid value ");
				continue;
			}
			BattlegroundsHeroSkinId? skinId = BattlegroundsHeroSkinId.FromUntrustedValue(battlegroundsHeroSkinModification.BattlegroundsHeroSkinId);
			if (!skinId.HasValue)
			{
				Debug.LogWarning("NetCache.OnBattlegroundsHeroSkinModifications(): received BattlegroundsHeroSkinModification message has invalid HasBattlegroundsHeroSkinId.");
			}
			else if (battlegroundsHeroSkinModification.HasAddBattlegroundsHeroSkin && battlegroundsHeroSkinModification.AddBattlegroundsHeroSkin)
			{
				netCacheBattlegroundsHeroSkins.OwnedBattlegroundsSkins.Add(skinId.Value);
				if (battlegroundsHeroSkinModification.HasAutoSetAsFavorite && battlegroundsHeroSkinModification.AutoSetAsFavorite)
				{
					ProcessNewFavoriteBattlegroundsHeroSkin(skinId.Value);
				}
				netCacheBattlegroundsHeroSkins.UnseenSkinIds.Add(skinId.Value);
				changed = true;
			}
			else if (battlegroundsHeroSkinModification.HasRemoveBattlegroundsHeroSkin && battlegroundsHeroSkinModification.RemoveBattlegroundsHeroSkin)
			{
				netCacheBattlegroundsHeroSkins.OwnedBattlegroundsSkins.Remove(skinId.Value);
				netCacheBattlegroundsHeroSkins.UnseenSkinIds.Remove(skinId.Value);
				changed = true;
			}
		}
		if (changed && this.OwnedBattlegroundsSkinsChanged != null)
		{
			this.OwnedBattlegroundsSkinsChanged();
		}
	}

	public void OnBattlegroundsBoardSkinModifications(BattlegroundsBoardSkinModifications packet)
	{
		NetCacheBattlegroundsBoardSkins netCacheBattlegroundsBoardSkins = GetNetObject<NetCacheBattlegroundsBoardSkins>();
		if (netCacheBattlegroundsBoardSkins == null)
		{
			Debug.LogWarning($"NetCache.OnBattlegroundsBoardSkinModifications(): trying to access NetCacheBattlegroundsBoardSkins before it has been loaded.");
			return;
		}
		bool changed = false;
		foreach (BattlegroundsBoardSkinModification battlegroundsBoardSkinModification in packet.BattlegroundsBoardSkinModifications_)
		{
			if (!battlegroundsBoardSkinModification.HasBattlegroundsBoardSkinId)
			{
				Debug.LogWarning("NetCache.OnBattlegroundsBoardSkinModifications(): received BattlegroundsBoardSkinModification message has no HasBattlegroundsBoardSkinId.");
				continue;
			}
			BattlegroundsBoardSkinId? skinId = BattlegroundsBoardSkinId.FromUntrustedValue(battlegroundsBoardSkinModification.BattlegroundsBoardSkinId);
			if (!skinId.HasValue || skinId.Value.IsDefaultBoard())
			{
				Debug.LogWarning("NetCache.OnBattlegroundsBoardSkinModifications(): received BattlegroundsBoardSkinModification message has invalid HasBattlegroundsBoardSkinId.");
			}
			else if (battlegroundsBoardSkinModification.HasAddBattlegroundsBoardSkin && battlegroundsBoardSkinModification.AddBattlegroundsBoardSkin)
			{
				netCacheBattlegroundsBoardSkins.OwnedBattlegroundsBoardSkins.Add(skinId.Value);
				if (battlegroundsBoardSkinModification.HasAutoSetAsFavorite && battlegroundsBoardSkinModification.AutoSetAsFavorite)
				{
					ProcessNewFavoriteBattlegroundsBoardSkin(skinId.Value);
				}
				netCacheBattlegroundsBoardSkins.UnseenSkinIds.Add(skinId.Value);
				changed = true;
			}
			else if (battlegroundsBoardSkinModification.HasRemoveBattlegroundsBoardSkin && battlegroundsBoardSkinModification.RemoveBattlegroundsBoardSkin)
			{
				netCacheBattlegroundsBoardSkins.OwnedBattlegroundsBoardSkins.Remove(skinId.Value);
				netCacheBattlegroundsBoardSkins.UnseenSkinIds.Remove(skinId.Value);
				changed = true;
			}
		}
		if (changed && this.OwnedBattlegroundsSkinsChanged != null)
		{
			this.OwnedBattlegroundsSkinsChanged();
		}
	}

	private void OnSetBattlegroundsEmoteLoadoutResponse()
	{
		SetBattlegroundsEmoteLoadoutResponse setBGEmoteloadoutResponse = Network.Get().GetSetBattlegroundsEmoteLoadoutResponse();
		if (!setBGEmoteloadoutResponse.Success)
		{
			return;
		}
		NetCacheBattlegroundsEmotes netCacheBGEmotes = Get().GetNetObject<NetCacheBattlegroundsEmotes>();
		if (netCacheBGEmotes == null)
		{
			return;
		}
		Hearthstone.BattlegroundsEmoteLoadout netLoadout = Hearthstone.BattlegroundsEmoteLoadout.MakeFromNetwork(setBGEmoteloadoutResponse.Loadout);
		if (netLoadout != null && netLoadout != netCacheBGEmotes.CurrentLoadout)
		{
			netCacheBGEmotes.CurrentLoadout = netLoadout;
			if (BattlegroundsEmoteLoadoutChangedListener != null)
			{
				BattlegroundsEmoteLoadoutChangedListener(netLoadout);
			}
		}
	}

	public void OnBattlegroundsFinisherModifications(BattlegroundsFinisherModifications packet)
	{
		NetCacheBattlegroundsFinishers netCacheBattlegroundsFinishers = GetNetObject<NetCacheBattlegroundsFinishers>();
		if (netCacheBattlegroundsFinishers == null)
		{
			Debug.LogWarning($"NetCache.OnBattlegroundsFinisherModifications(): trying to access NetCacheBattlegroundsFinishers before it has been loaded.");
			return;
		}
		bool changed = false;
		foreach (BattlegroundsFinisherModification battlegroundsFinisherModification in packet.BattlegroundsFinisherModifications_)
		{
			if (!battlegroundsFinisherModification.HasBattlegroundsFinisherId)
			{
				Debug.LogWarning("NetCache.OnBattlegroundsFinisherModifications(): received BattlegroundsFinisherModification message has no HasBattlegroundsFinisherId.");
				continue;
			}
			BattlegroundsFinisherId? finisherId = BattlegroundsFinisherId.FromUntrustedValue(battlegroundsFinisherModification.BattlegroundsFinisherId);
			if (!finisherId.HasValue || finisherId.Value.IsDefaultFinisher())
			{
				Debug.LogWarning("NetCache.OnBattlegroundsFinisherModifications(): received BattlegroundsFinisherModification message has invalid HasBattlegroundsFinisherId.");
			}
			else if (battlegroundsFinisherModification.HasAddBattlegroundsFinisher && battlegroundsFinisherModification.AddBattlegroundsFinisher)
			{
				netCacheBattlegroundsFinishers.OwnedBattlegroundsFinishers.Add(finisherId.Value);
				if (battlegroundsFinisherModification.HasAutoSetAsFavorite && battlegroundsFinisherModification.AutoSetAsFavorite)
				{
					ProcessNewFavoriteBattlegroundsFinisher(finisherId.Value);
				}
				netCacheBattlegroundsFinishers.UnseenSkinIds.Add(finisherId.Value);
				changed = true;
			}
			else if (battlegroundsFinisherModification.HasRemoveBattlegroundsFinisher && battlegroundsFinisherModification.RemoveBattlegroundsFinisher)
			{
				netCacheBattlegroundsFinishers.OwnedBattlegroundsFinishers.Remove(finisherId.Value);
				netCacheBattlegroundsFinishers.UnseenSkinIds.Remove(finisherId.Value);
				changed = true;
			}
		}
		if (changed && this.OwnedBattlegroundsSkinsChanged != null)
		{
			this.OwnedBattlegroundsSkinsChanged();
		}
	}

	private void OnBattlegroundsEmotesResponse()
	{
		BattlegroundsEmotesResponse packet = Network.Get().GetBattlegroundsEmotesResponse();
		NetCacheBattlegroundsEmotes netCacheBGEmotes = new NetCacheBattlegroundsEmotes();
		foreach (BattlegroundsEmoteInfo packetOwnedEmote in packet.OwnedEmotes)
		{
			BattlegroundsEmoteId? ownedEmoteId = BattlegroundsEmoteId.FromUntrustedValue(packetOwnedEmote.EmoteId);
			if (!ownedEmoteId.HasValue)
			{
				Log.Net.PrintError("OnBattlegroundsEmotesResponse FAILED (packetOwnedEmote = {0} due to negative ID)", packetOwnedEmote);
				continue;
			}
			if (ownedEmoteId.Value.IsDefaultEmote())
			{
				Log.Net.PrintError("OnBattlegroundsEmotesResponse FAILED (packetOwnedEmote = {0} due to default)", packetOwnedEmote);
				continue;
			}
			if (!CollectionManager.Get().IsValidBattlegroundsEmoteId(ownedEmoteId.Value))
			{
				Log.Net.PrintError("OnBattlegroundsEmotesResponse FAILED (packetOwnedEmote = {0} due to not present in Hearthedit)", packetOwnedEmote);
				continue;
			}
			netCacheBGEmotes.OwnedBattlegroundsEmotes.Add(ownedEmoteId.Value);
			if (!packetOwnedEmote.HasSeen)
			{
				netCacheBGEmotes.UnseenEmoteIds.Add(ownedEmoteId.Value);
			}
		}
		Hearthstone.BattlegroundsEmoteLoadout fromNetwork = Hearthstone.BattlegroundsEmoteLoadout.MakeFromNetwork(packet.Loadout);
		if (fromNetwork == null)
		{
			Log.Net.PrintError("OnBattlegroundsEmotesResponse FAILED due to invalid loadout.");
		}
		else
		{
			netCacheBGEmotes.CurrentLoadout = fromNetwork;
		}
		OnNetCacheObjReceived(netCacheBGEmotes);
	}

	public void OnBattlegroundsEmoteModifications(BattlegroundsEmoteModifications packet)
	{
		NetCacheBattlegroundsEmotes netCacheBattlegroundsEmotes = GetNetObject<NetCacheBattlegroundsEmotes>();
		if (netCacheBattlegroundsEmotes == null)
		{
			Debug.LogWarning($"NetCache.OnBattlegroundsEmoteModifications(): trying to access NetCacheBattlegroundsEmotes before it has been loaded.");
			return;
		}
		bool changed = false;
		foreach (BattlegroundsEmoteModification battlegroundsEmoteModification in packet.BattlegroundsEmoteModifications_)
		{
			if (!battlegroundsEmoteModification.HasBattlegroundsEmoteId)
			{
				Debug.LogWarning("NetCache.OnBattlegroundsEmoteModifications(): received BattlegroundsEmoteModification message has no HasBattlegroundsEmoteId.");
				continue;
			}
			BattlegroundsEmoteId? battlegroundsEmoteId = BattlegroundsEmoteId.FromUntrustedValue(battlegroundsEmoteModification.BattlegroundsEmoteId);
			if (!battlegroundsEmoteId.HasValue)
			{
				Debug.LogWarning("NetCache.OnBattlegroundsEmoteModifications(): received BattlegroundsEmoteModification message has invalid HasBattlegroundsEmoteId.");
			}
			else if (battlegroundsEmoteModification.HasRemoveBattlegroundsEmote && battlegroundsEmoteModification.AddBattlegroundsEmote)
			{
				netCacheBattlegroundsEmotes.OwnedBattlegroundsEmotes.Add(battlegroundsEmoteId.Value);
				netCacheBattlegroundsEmotes.UnseenEmoteIds.Add(battlegroundsEmoteId.Value);
				changed = true;
			}
			else if (battlegroundsEmoteModification.HasRemoveBattlegroundsEmote && battlegroundsEmoteModification.RemoveBattlegroundsEmote)
			{
				netCacheBattlegroundsEmotes.OwnedBattlegroundsEmotes.Remove(battlegroundsEmoteId.Value);
				netCacheBattlegroundsEmotes.UnseenEmoteIds.Remove(battlegroundsEmoteId.Value);
				changed = true;
			}
		}
		if (changed && this.OwnedBattlegroundsSkinsChanged != null)
		{
			this.OwnedBattlegroundsSkinsChanged();
		}
	}

	private void OnSetFavoriteCardBackResponse()
	{
		Network.CardBackResponse response = Network.Get().GetCardBackResponse();
		if (!response.Success)
		{
			Log.CardbackMgr.PrintError("SetFavoriteCardBack FAILED (cardBack = {0})", response.CardBack);
		}
		else
		{
			ProcessNewFavoriteCardBack(response.CardBack, response.IsFavorite);
		}
	}

	public void ProcessNewFavoriteCardBack(int cardBackId, bool isFavorite = true)
	{
		NetCacheCardBacks netCacheCardBacks = GetNetObject<NetCacheCardBacks>();
		if (netCacheCardBacks == null)
		{
			Debug.LogWarning($"NetCache.ProcessNewFavoriteCardBack(): trying to access NetCacheCardBacks before it's been loaded");
			return;
		}
		if (isFavorite)
		{
			netCacheCardBacks.FavoriteCardBacks.Add(cardBackId);
		}
		else
		{
			netCacheCardBacks.FavoriteCardBacks.Remove(cardBackId);
		}
		if (this.FavoriteCardBackChanged != null)
		{
			this.FavoriteCardBackChanged(cardBackId, isFavorite);
		}
	}

	public void ProcessNewFavoriteBattlegroundsGuideSkin(BattlegroundsGuideSkinId newFavoriteBattlegroundsGuideSkinID)
	{
		NetCacheBattlegroundsGuideSkins netCacheBattlegroundsGuideSkins = GetNetObject<NetCacheBattlegroundsGuideSkins>();
		if (netCacheBattlegroundsGuideSkins == null)
		{
			Debug.LogWarning($"NetCache.ProcessNewFavoriteBattlegroundsGuideSkin(): trying to access NetCacheBattlegroundsGuideSkins before it has been loaded.");
		}
		else if (!(netCacheBattlegroundsGuideSkins.BattlegroundsFavoriteGuideSkin == newFavoriteBattlegroundsGuideSkinID))
		{
			netCacheBattlegroundsGuideSkins.BattlegroundsFavoriteGuideSkin = newFavoriteBattlegroundsGuideSkinID;
			if (this.FavoriteBattlegroundsGuideSkinChanged != null)
			{
				this.FavoriteBattlegroundsGuideSkinChanged(newFavoriteBattlegroundsGuideSkinID);
			}
		}
	}

	public void ProcessNewFavoriteBattlegroundsHeroSkin(BattlegroundsHeroSkinId newBattlegroundsHeroSkinId)
	{
		if (!newBattlegroundsHeroSkinId.IsValid())
		{
			Debug.LogWarning("NetCache.ProcessNewFavoriteBattlegroundsHeroSkin(): Invalid BattlegroundsHeroSkinId");
			return;
		}
		string miniGuid = GameUtils.TranslateDbIdToCardId(newBattlegroundsHeroSkinId.ToValue());
		int baseHeroCardId = GameUtils.TranslateCardIdToDbId(CollectionManager.Get().GetBattlegroundsBaseHeroCardId(miniGuid));
		NetCacheBattlegroundsHeroSkins netCacheBattlegroundsHeroSkins = GetNetObject<NetCacheBattlegroundsHeroSkins>();
		netCacheBattlegroundsHeroSkins.BattlegroundsFavoriteHeroSkins.TryGetValue(baseHeroCardId, out var favoriteSkins);
		if (favoriteSkins == null)
		{
			favoriteSkins = new HashSet<int>();
			netCacheBattlegroundsHeroSkins.BattlegroundsFavoriteHeroSkins.Add(baseHeroCardId, favoriteSkins);
		}
		favoriteSkins.Add(newBattlegroundsHeroSkinId.ToValue());
		if (this.FavoriteBattlegroundsHeroSkinChanged != null)
		{
			this.FavoriteBattlegroundsHeroSkinChanged(baseHeroCardId, newBattlegroundsHeroSkinId.ToValue());
		}
	}

	public void ProcessNewFavoriteBattlegroundsBoardSkin(BattlegroundsBoardSkinId newFavoriteBattlegroundsBoardSkinID)
	{
		NetCacheBattlegroundsBoardSkins netCacheBattlegroundsBoardSkins = GetNetObject<NetCacheBattlegroundsBoardSkins>();
		if (netCacheBattlegroundsBoardSkins == null)
		{
			Debug.LogWarning($"NetCache.ProcessNewFavoriteBattlegroundsBoardSkin(): trying to access NetCacheBattlegroundsBoardSkins before it has been loaded.");
		}
		else if (netCacheBattlegroundsBoardSkins.BattlegroundsFavoriteBoardSkins.Add(newFavoriteBattlegroundsBoardSkinID) && this.FavoriteBattlegroundsBoardSkinChanged != null)
		{
			this.FavoriteBattlegroundsBoardSkinChanged(newFavoriteBattlegroundsBoardSkinID);
		}
	}

	public void ProcessNewFavoriteBattlegroundsFinisher(BattlegroundsFinisherId newFavoriteBattlegroundsFinisherID)
	{
		NetCacheBattlegroundsFinishers netCacheBattlegroundsFinishers = GetNetObject<NetCacheBattlegroundsFinishers>();
		if (netCacheBattlegroundsFinishers == null)
		{
			Debug.LogWarning($"NetCache.ProcessNewFavoriteBattlegroundsFinisher(): trying to access NetCacheBattlegroundsFinishers before it has been loaded.");
		}
		else if (!netCacheBattlegroundsFinishers.BattlegroundsFavoriteFinishers.Contains(newFavoriteBattlegroundsFinisherID))
		{
			netCacheBattlegroundsFinishers.BattlegroundsFavoriteFinishers.Add(newFavoriteBattlegroundsFinisherID);
			if (this.FavoriteBattlegroundsFinisherChanged != null)
			{
				this.FavoriteBattlegroundsFinisherChanged(newFavoriteBattlegroundsFinisherID);
			}
		}
	}

	private void OnSetFavoriteCosmeticCoinResponse()
	{
		Network.CosmeticCoinResponse response = Network.Get().GetCoinResponse();
		if (!response.Success)
		{
			Log.Net.PrintError("SetFavoriteCardBack FAILED (coin = {0})", response.Coin);
		}
		else
		{
			ProcessNewFavoriteCoin(response.Coin, response.IsFavorite);
		}
	}

	public void ProcessNewFavoriteCoin(int coinId, bool isFavorite)
	{
		NetCacheCoins netCacheCoins = GetNetObject<NetCacheCoins>();
		if (netCacheCoins == null)
		{
			Debug.LogWarning($"NetCache.ProcessNewFavoriteCoin(): trying to accessNetCacheCoins before it's been loaded");
			return;
		}
		if (isFavorite)
		{
			netCacheCoins.FavoriteCoins.Add(coinId);
		}
		else
		{
			netCacheCoins.FavoriteCoins.Remove(coinId);
		}
		if (this.FavoriteCoinChanged != null)
		{
			this.FavoriteCoinChanged(coinId, isFavorite);
		}
	}

	private void OnGamesInfo()
	{
		NetCacheGamesPlayed gamesPlayed = Network.Get().GetGamesInfo();
		if (gamesPlayed == null)
		{
			Debug.LogWarning("error getting games info");
		}
		else
		{
			OnNetCacheObjReceived(gamesPlayed);
		}
	}

	private void OnProfileProgress()
	{
		OnNetCacheObjReceived(Network.Get().GetProfileProgress());
	}

	private void OnHearthstoneUnavailableGame()
	{
		OnHearthstoneUnavailable(gamePacket: true);
	}

	private void OnHearthstoneUnavailableUtil()
	{
		OnHearthstoneUnavailable(gamePacket: false);
	}

	private void OnHearthstoneUnavailable(bool gamePacket)
	{
		Network.UnavailableReason reason = Network.Get().GetHearthstoneUnavailable(gamePacket);
		Debug.Log("Hearthstone Unavailable!  Reason: " + reason.mainReason);
		string mainReason = reason.mainReason;
		if (!(mainReason == "VERSION"))
		{
			if (mainReason == "OFFLINE")
			{
				Network.Get().ShowConnectionFailureError("GLOBAL_ERROR_NETWORK_UNAVAILABLE_OFFLINE");
				return;
			}
			TelemetryManager.Client().SendNetworkError(NetworkError.ErrorType.SERVICE_UNAVAILABLE, $"{reason.mainReason} - {reason.subReason} - {reason.extraData}", 0);
			Network.Get().ShowConnectionFailureError("GLOBAL_ERROR_NETWORK_UNAVAILABLE_UNKNOWN");
			return;
		}
		ErrorParams parms = new ErrorParams();
		if (PlatformSettings.IsMobile() && GameDownloadManagerProvider.Get() != null && !GameDownloadManagerProvider.Get().IsNewMobileVersionReleased)
		{
			parms.m_message = GameStrings.Format("GLOBAL_ERROR_NETWORK_UNAVAILABLE_NEW_VERSION");
			parms.m_reason = FatalErrorReason.UNAVAILABLE_NEW_VERSION;
		}
		else
		{
			parms.m_message = GameStrings.Format("GLOBAL_ERROR_NETWORK_UNAVAILABLE_UPGRADE");
			if ((bool)Error.HAS_APP_STORE)
			{
				parms.m_redirectToStore = true;
			}
			parms.m_reason = FatalErrorReason.UNAVAILABLE_UPGRADE;
		}
		Error.AddFatal(parms);
		ReconnectMgr.Get().FullResetRequired = true;
		ReconnectMgr.Get().UpdateRequired = true;
	}

	private void OnCardBacks()
	{
		Network network = Network.Get();
		OnNetCacheObjReceived(network.GetCardBacks());
		CardBacks packet = network.GetCardBacksPacket();
		if (packet == null)
		{
			return;
		}
		List<int> favoriteCardBackIds = packet.FavoriteCardBacks;
		OfflineDataCache.OfflineData data = OfflineDataCache.ReadOfflineDataFromFile();
		List<SetFavoriteCardBack> requests = OfflineDataCache.GenerateSetFavoriteCardBackFromDiff(data, favoriteCardBackIds);
		if (requests != null && requests.Count > 0)
		{
			foreach (SetFavoriteCardBack request in requests)
			{
				network.SetFavoriteCardBack(request.CardBack, request.IsFavorite);
			}
		}
		OfflineDataCache.ClearCardBackDirtyFlag(ref data);
		OfflineDataCache.CacheCardBacks(ref data, packet);
		OfflineDataCache.WriteOfflineDataToFile(data);
	}

	private void OnCoins()
	{
		Network network = Network.Get();
		OnNetCacheObjReceived(network.GetCoins());
		CosmeticCoins packet = network.GetCoinsPacket();
		if (packet == null)
		{
			return;
		}
		List<int> favoriteCoinIds = packet.FavoriteCoins;
		OfflineDataCache.OfflineData data = OfflineDataCache.ReadOfflineDataFromFile();
		foreach (SetFavoriteCosmeticCoin request in OfflineDataCache.GenerateSetFavoriteCosmeticCoinFromDiff(data, favoriteCoinIds))
		{
			network.SetFavoriteCosmeticCoin(ref data, request.Coin, request.IsFavorite);
		}
		OfflineDataCache.ClearCoinDirtyFlag(ref data);
		OfflineDataCache.CacheCoins(ref data, packet);
		OfflineDataCache.WriteOfflineDataToFile(data);
	}

	private void OnBattlegroundsHeroSkinsResponse()
	{
		CollectionManager collectionManager = CollectionManager.Get();
		BattlegroundsHeroSkinsResponse battlegroundsHeroSkinsResponse = Network.Get().GetBattlegroundsHeroSkinsResponse();
		NetCacheBattlegroundsHeroSkins netCacheBGHeroSkins = new NetCacheBattlegroundsHeroSkins();
		foreach (BattlegroundsHeroSkinInfo packetOwnedSkin in battlegroundsHeroSkinsResponse.OwnedSkins)
		{
			int skinId = BaconHeroSkinUtils.SKIN_ID_FOR_FAVORITED_BASE_HERO;
			if (packetOwnedSkin.HeroSkinId != BaconHeroSkinUtils.SKIN_ID_FOR_FAVORITED_BASE_HERO)
			{
				BattlegroundsHeroSkinDbfRecord record = GameDbf.BattlegroundsHeroSkin.GetRecord(packetOwnedSkin.HeroSkinId);
				BattlegroundsHeroSkinId battlegroundsHeroSkinId = BattlegroundsHeroSkinId.FromTrustedValue(record.ID);
				if (packetOwnedSkin.HeroSkinId != BaconHeroSkinUtils.SKIN_ID_FOR_FAVORITED_BASE_HERO)
				{
					netCacheBGHeroSkins.OwnedBattlegroundsSkins.Add(battlegroundsHeroSkinId);
				}
				skinId = record.ID;
				string cardMiniGUID = GameUtils.TranslateDbIdToCardId(record.SkinCardId);
				if (!collectionManager.IsBattlegroundsHeroCard(cardMiniGUID))
				{
					Log.Net.PrintError("OnBattlegroundsHeroSkinsResponse FAILED (packetOwnedSkin = {0})", packetOwnedSkin);
					continue;
				}
				if (!packetOwnedSkin.HasSeen)
				{
					netCacheBGHeroSkins.UnseenSkinIds.Add(battlegroundsHeroSkinId);
				}
			}
			if (packetOwnedSkin.IsFavorite)
			{
				netCacheBGHeroSkins.BattlegroundsFavoriteHeroSkins.TryGetValue(packetOwnedSkin.BaseHeroCardId, out var favoriteSkins);
				if (favoriteSkins == null)
				{
					favoriteSkins = new HashSet<int>();
					netCacheBGHeroSkins.BattlegroundsFavoriteHeroSkins.Add(packetOwnedSkin.BaseHeroCardId, favoriteSkins);
				}
				favoriteSkins.Add(skinId);
			}
		}
		OnNetCacheObjReceived(netCacheBGHeroSkins);
	}

	private void OnSetBattlegroundsFavoriteHeroSkinResponse()
	{
		SetBattlegroundsFavoriteHeroSkinResponse setBGFavoriteSkinResponse = Network.Get().GetSetBattlegroundsFavoriteHeroSkinResponse();
		if (!setBGFavoriteSkinResponse.Success)
		{
			return;
		}
		int heroSkinId = setBGFavoriteSkinResponse.HeroSkinId;
		int baseHeroCardId = setBGFavoriteSkinResponse.BaseHeroCardId;
		NetCacheBattlegroundsHeroSkins netCacheBGHeroSkins = GetNetObject<NetCacheBattlegroundsHeroSkins>();
		if (netCacheBGHeroSkins != null)
		{
			netCacheBGHeroSkins.BattlegroundsFavoriteHeroSkins.TryGetValue(baseHeroCardId, out var favoriteSkins);
			if (favoriteSkins == null)
			{
				favoriteSkins = new HashSet<int>();
				netCacheBGHeroSkins.BattlegroundsFavoriteHeroSkins.Add(baseHeroCardId, favoriteSkins);
			}
			int skinId = heroSkinId;
			favoriteSkins.Add(skinId);
			if (this.FavoriteBattlegroundsHeroSkinChanged != null)
			{
				this.FavoriteBattlegroundsHeroSkinChanged(baseHeroCardId, heroSkinId);
			}
		}
	}

	private void OnClearBattlegroundsFavoriteHeroSkinResponse()
	{
		ClearBattlegroundsFavoriteHeroSkinResponse clearBGFavoriteSkinResponse = Network.Get().GetClearBattlegroundsFavoriteHeroSkinResponse();
		if (!clearBGFavoriteSkinResponse.Success)
		{
			return;
		}
		int heroSkinId = clearBGFavoriteSkinResponse.HeroSkinId;
		int baseHeroId = clearBGFavoriteSkinResponse.BaseHeroCardId;
		NetCacheBattlegroundsHeroSkins netCacheBGHeroSkins = GetNetObject<NetCacheBattlegroundsHeroSkins>();
		if (netCacheBGHeroSkins == null)
		{
			return;
		}
		netCacheBGHeroSkins.BattlegroundsFavoriteHeroSkins.TryGetValue(baseHeroId, out var favoriteSkins);
		if (favoriteSkins != null)
		{
			int skinId = heroSkinId;
			favoriteSkins.Remove(skinId);
			if (this.FavoriteBattlegroundsHeroSkinChanged != null)
			{
				this.FavoriteBattlegroundsHeroSkinChanged(baseHeroId, heroSkinId);
			}
		}
	}

	private void OnBattlegroundsGuideSkinsResponse()
	{
		BattlegroundsGuideSkinsResponse battlegroundsGuideSkinsResponse = Network.Get().GetBattlegroundsGuideSkinsResponse();
		NetCacheBattlegroundsGuideSkins netCacheBGGuideSkins = new NetCacheBattlegroundsGuideSkins
		{
			BattlegroundsFavoriteGuideSkin = null
		};
		foreach (BattlegroundsGuideSkinInfo packetOwnedSkin in battlegroundsGuideSkinsResponse.OwnedSkins)
		{
			BattlegroundsGuideSkinId? ownedSkinId = BattlegroundsGuideSkinId.FromUntrustedValue(packetOwnedSkin.GuideSkinId);
			if (!ownedSkinId.HasValue)
			{
				Log.Net.PrintError("OnBattlegroundsGuideSkinsResponse FAILED (packetOwnedSkin = {0})", packetOwnedSkin);
				continue;
			}
			if (!CollectionManager.Get().IsValidBattlegroundsGuideSkinId(ownedSkinId.Value))
			{
				Log.Net.PrintError("OnBattlegroundsGuideSkinsResponse FAILED (packetOwnedSkin = {0})", packetOwnedSkin);
				continue;
			}
			netCacheBGGuideSkins.OwnedBattlegroundsGuideSkins.Add(ownedSkinId.Value);
			if (packetOwnedSkin.IsFavorite)
			{
				if (netCacheBGGuideSkins.BattlegroundsFavoriteGuideSkin.HasValue)
				{
					Log.Net.PrintError("OnBattlegroundsGuideSkinsResponse FAILED (multiple favorite skins)");
				}
				else
				{
					netCacheBGGuideSkins.BattlegroundsFavoriteGuideSkin = ownedSkinId;
				}
			}
			if (!packetOwnedSkin.HasSeen)
			{
				netCacheBGGuideSkins.UnseenSkinIds.Add(ownedSkinId.Value);
			}
		}
		OnNetCacheObjReceived(netCacheBGGuideSkins);
	}

	private void OnSetBattlegroundsFavoriteGuideSkinResponse()
	{
		SetBattlegroundsFavoriteGuideSkinResponse setBGFavoriteGuideSkinResponse = Network.Get().GetSetBattlegroundsFavoriteGuideSkinResponse();
		if (!setBGFavoriteGuideSkinResponse.Success)
		{
			return;
		}
		BattlegroundsGuideSkinId? skinID = BattlegroundsGuideSkinId.FromUntrustedValue(setBGFavoriteGuideSkinResponse.GuideSkinId);
		if (!skinID.HasValue || !CollectionManager.Get().IsValidBattlegroundsGuideSkinId(skinID.Value))
		{
			Log.Net.PrintError("OnSetBattlegroundsFavoriteGuideSkinResponse FAILED - invalid skin ID (GuideSkinId = {0})", skinID);
		}
		NetCacheBattlegroundsGuideSkins netCacheBGGuideSkins = Get().GetNetObject<NetCacheBattlegroundsGuideSkins>();
		if (netCacheBGGuideSkins != null)
		{
			netCacheBGGuideSkins.BattlegroundsFavoriteGuideSkin = skinID;
			if (this.FavoriteBattlegroundsGuideSkinChanged != null)
			{
				this.FavoriteBattlegroundsGuideSkinChanged(skinID);
			}
		}
	}

	private void OnClearBattlegroundsFavoriteGuideSkinResponse()
	{
		if (!Network.Get().GetClearBattlegroundsFavoriteGuideSkinResponse().Success)
		{
			return;
		}
		NetCacheBattlegroundsGuideSkins netCacheBGGuideSkins = Get().GetNetObject<NetCacheBattlegroundsGuideSkins>();
		if (netCacheBGGuideSkins != null)
		{
			netCacheBGGuideSkins.BattlegroundsFavoriteGuideSkin = null;
			if (this.FavoriteBattlegroundsGuideSkinChanged != null)
			{
				this.FavoriteBattlegroundsGuideSkinChanged(null);
			}
		}
	}

	private void OnBattlegroundsBoardSkinsResponse()
	{
		BattlegroundsBoardSkinsResponse battlegroundsBoardSkinsResponse = Network.Get().GetBattlegroundsBoardSkinsResponse();
		NetCacheBattlegroundsBoardSkins netCacheBGBoardSkins = new NetCacheBattlegroundsBoardSkins
		{
			BattlegroundsFavoriteBoardSkins = new HashSet<BattlegroundsBoardSkinId>()
		};
		foreach (BattlegroundsBoardSkinInfo ownedBattlegroundsBoardSkinInfo in battlegroundsBoardSkinsResponse.OwnedSkins)
		{
			BattlegroundsBoardSkinId? ownedBattlegroundsBoardSkinId = BattlegroundsBoardSkinId.FromUntrustedValue(ownedBattlegroundsBoardSkinInfo.BoardSkinId);
			if (!ownedBattlegroundsBoardSkinId.HasValue)
			{
				Log.Net.PrintWarning("NetCache::OnBattlegroundsBoardSkinsResponse BattlegroundsBoardSkinId missing value");
				continue;
			}
			if (!ownedBattlegroundsBoardSkinId.Value.IsValid() || !CollectionManager.Get().IsValidBattlegroundsBoardSkinId(ownedBattlegroundsBoardSkinId.Value))
			{
				Log.Net.PrintWarning(string.Format("{0}::{1} BattlegroundsBoardSkinId is invalid: {2}", "NetCache", "OnBattlegroundsBoardSkinsResponse", ownedBattlegroundsBoardSkinId));
				continue;
			}
			netCacheBGBoardSkins.OwnedBattlegroundsBoardSkins.Add(ownedBattlegroundsBoardSkinId.Value);
			if (ownedBattlegroundsBoardSkinInfo.IsFavorite)
			{
				netCacheBGBoardSkins.BattlegroundsFavoriteBoardSkins.Add(ownedBattlegroundsBoardSkinId.Value);
			}
			if (!ownedBattlegroundsBoardSkinInfo.HasSeen)
			{
				netCacheBGBoardSkins.UnseenSkinIds.Add(ownedBattlegroundsBoardSkinId.Value);
			}
		}
		OnNetCacheObjReceived(netCacheBGBoardSkins);
	}

	private void OnSetBattlegroundsFavoriteBoardSkinResponse()
	{
		SetBattlegroundsFavoriteBoardSkinResponse setBGFavoriteBoardSkinResponse = Network.Get().GetSetBattlegroundsFavoriteBoardSkinResponse();
		if (!setBGFavoriteBoardSkinResponse.Success)
		{
			return;
		}
		BattlegroundsBoardSkinId? battlegroundsBoardSkinId = BattlegroundsBoardSkinId.FromUntrustedValue(setBGFavoriteBoardSkinResponse.BoardSkinId);
		if (!battlegroundsBoardSkinId.HasValue)
		{
			Log.Net.PrintWarning("NetCache::OnSetBattlegroundsFavoriteBoardSkinResponse BattlegroundsBoardSkinId missing value");
			return;
		}
		if (!battlegroundsBoardSkinId.Value.IsValid() || !CollectionManager.Get().IsValidBattlegroundsBoardSkinId(battlegroundsBoardSkinId.Value))
		{
			Log.Net.PrintWarning(string.Format("{0}::{1} BattlegroundsBoardSkinId is invalid: {2}", "NetCache", "OnSetBattlegroundsFavoriteBoardSkinResponse", battlegroundsBoardSkinId));
			return;
		}
		NetCacheBattlegroundsBoardSkins netCacheBGBoardSkins = Get().GetNetObject<NetCacheBattlegroundsBoardSkins>();
		if (netCacheBGBoardSkins != null && netCacheBGBoardSkins.BattlegroundsFavoriteBoardSkins.Add(battlegroundsBoardSkinId.Value) && this.FavoriteBattlegroundsBoardSkinChanged != null)
		{
			this.FavoriteBattlegroundsBoardSkinChanged(battlegroundsBoardSkinId);
		}
	}

	private void OnClearBattlegroundsFavoriteBoardSkinResponse()
	{
		ClearBattlegroundsFavoriteBoardSkinResponse clearBGFavoriteSkinResponse = Network.Get().GetClearBattlegroundsFavoriteBoardSkinResponse();
		if (!clearBGFavoriteSkinResponse.Success)
		{
			return;
		}
		BattlegroundsBoardSkinId? battlegroundsBoardSkinId = BattlegroundsBoardSkinId.FromUntrustedValue(clearBGFavoriteSkinResponse.BoardSkinId);
		if (!battlegroundsBoardSkinId.HasValue)
		{
			Log.Net.PrintWarning("NetCache::OnClearBattlegroundsFavoriteBoardSkinResponse BattlegroundsBoardSkinId missing value");
			return;
		}
		if (!battlegroundsBoardSkinId.Value.IsValid() || !CollectionManager.Get().IsValidBattlegroundsBoardSkinId(battlegroundsBoardSkinId.Value))
		{
			Log.Net.PrintWarning(string.Format("{0}::{1} BattlegroundsBoardSkinId is invalid: {2}", "NetCache", "OnClearBattlegroundsFavoriteBoardSkinResponse", battlegroundsBoardSkinId));
			return;
		}
		NetCacheBattlegroundsBoardSkins netCacheBGBoardSkins = Get().GetNetObject<NetCacheBattlegroundsBoardSkins>();
		if (netCacheBGBoardSkins != null && netCacheBGBoardSkins.BattlegroundsFavoriteBoardSkins.Remove(battlegroundsBoardSkinId.Value) && this.FavoriteBattlegroundsBoardSkinChanged != null)
		{
			this.FavoriteBattlegroundsBoardSkinChanged(null);
		}
	}

	private void OnBattlegroundsFinishersResponse()
	{
		BattlegroundsFinishersResponse battlegroundsFinishersResponse = Network.Get().GetBattlegroundsFinishersResponse();
		NetCacheBattlegroundsFinishers netCacheBGFinishers = new NetCacheBattlegroundsFinishers
		{
			BattlegroundsFavoriteFinishers = new HashSet<BattlegroundsFinisherId>()
		};
		foreach (BattlegroundsFinisherInfo packetOwnedSkin in battlegroundsFinishersResponse.OwnedSkins)
		{
			BattlegroundsFinisherId? ownedSkinId = BattlegroundsFinisherId.FromUntrustedValue(packetOwnedSkin.FinisherId);
			if (!ownedSkinId.HasValue)
			{
				Log.Net.PrintError("OnBattlegroundsFinishersResponse FAILED (packetOwnedSkin = {0})", packetOwnedSkin);
				continue;
			}
			if (!CollectionManager.Get().IsValidBattlegroundsFinisherId(ownedSkinId.Value))
			{
				Log.Net.PrintError("OnBattlegroundsFinishersResponse FAILED (packetOwnedSkin = {0})", packetOwnedSkin);
				continue;
			}
			netCacheBGFinishers.OwnedBattlegroundsFinishers.Add(ownedSkinId.Value);
			if (packetOwnedSkin.IsFavorite)
			{
				netCacheBGFinishers.BattlegroundsFavoriteFinishers.Add(ownedSkinId.Value);
			}
			if (!packetOwnedSkin.HasSeen)
			{
				netCacheBGFinishers.UnseenSkinIds.Add(ownedSkinId.Value);
			}
		}
		OnNetCacheObjReceived(netCacheBGFinishers);
	}

	private void OnSetBattlegroundsFavoriteFinisherResponse()
	{
		SetBattlegroundsFavoriteFinisherResponse setBGFavoriteFinisherResponse = Network.Get().GetSetBattlegroundsFavoriteFinisherResponse();
		if (!setBGFavoriteFinisherResponse.Success)
		{
			return;
		}
		BattlegroundsFinisherId? battlegroundsFinisherId = BattlegroundsFinisherId.FromUntrustedValue(setBGFavoriteFinisherResponse.FinisherId);
		if (!battlegroundsFinisherId.HasValue)
		{
			Log.Net.PrintError("OnSetBattlegroundsFavoriteFinisherResponse FAILED - missing value (FinisherId = {0})", battlegroundsFinisherId);
			return;
		}
		if (!CollectionManager.Get().IsValidBattlegroundsFinisherId(battlegroundsFinisherId.Value))
		{
			Log.Net.PrintError("OnSetBattlegroundsFavoriteFinisherResponse FAILED - invalid BattlegroundsFinisherId (FinisherId = {0})", battlegroundsFinisherId);
			return;
		}
		NetCacheBattlegroundsFinishers netCacheBGFinishers = GetNetObject<NetCacheBattlegroundsFinishers>();
		if (netCacheBGFinishers != null)
		{
			netCacheBGFinishers.BattlegroundsFavoriteFinishers.Add(battlegroundsFinisherId.Value);
			if (this.FavoriteBattlegroundsFinisherChanged != null)
			{
				this.FavoriteBattlegroundsFinisherChanged(battlegroundsFinisherId);
			}
		}
	}

	private void OnClearBattlegroundsFavoriteFinisherResponse()
	{
		ClearBattlegroundsFavoriteFinisherResponse clearBGFavoriteFinisherResponse = Network.Get().GetClearBattlegroundsFavoriteFinisherResponse();
		if (!clearBGFavoriteFinisherResponse.Success)
		{
			return;
		}
		BattlegroundsFinisherId? battlegroundsFinisherId = BattlegroundsFinisherId.FromUntrustedValue(clearBGFavoriteFinisherResponse.FinisherId);
		if (!battlegroundsFinisherId.HasValue)
		{
			Log.Net.PrintError("OnClearBattlegroundsFavoriteFinisherResponse FAILED - missing value (FinisherId = {0})", battlegroundsFinisherId);
			return;
		}
		if (!CollectionManager.Get().IsValidBattlegroundsFinisherId(battlegroundsFinisherId.Value))
		{
			Log.Net.PrintError("OnClearBattlegroundsFavoriteFinisherResponse FAILED - invalid BattlegroundsFinisherId (FinisherId = {0})", battlegroundsFinisherId);
			return;
		}
		NetCacheBattlegroundsFinishers netCacheBGFinishers = GetNetObject<NetCacheBattlegroundsFinishers>();
		if (netCacheBGFinishers != null)
		{
			netCacheBGFinishers.BattlegroundsFavoriteFinishers.Remove(battlegroundsFinisherId.Value);
			if (this.FavoriteBattlegroundsFinisherChanged != null)
			{
				this.FavoriteBattlegroundsFinisherChanged(null);
			}
		}
	}

	private void OnPlayerRecords()
	{
		PlayerRecords packet = Network.Get().GetPlayerRecordsPacket();
		OnPlayerRecordsPacket(packet);
	}

	public void OnPlayerRecordsPacket(PlayerRecords packet)
	{
		OnNetCacheObjReceived(Network.GetPlayerRecords(packet));
	}

	private void OnRewardProgress()
	{
		OnNetCacheObjReceived(Network.Get().GetRewardProgress());
	}

	private NetCacheHeroLevels GetAllHeroXP(HeroXP packet)
	{
		if (packet == null)
		{
			return new NetCacheHeroLevels();
		}
		NetCacheHeroLevels result = new NetCacheHeroLevels();
		for (int i = 0; i < packet.XpInfos.Count; i++)
		{
			HeroXPInfo xpInfo = packet.XpInfos[i];
			HeroLevel heroLevel = new HeroLevel();
			heroLevel.Class = (TAG_CLASS)xpInfo.ClassId;
			heroLevel.CurrentLevel.Level = xpInfo.Level;
			heroLevel.CurrentLevel.XP = xpInfo.CurrXp;
			heroLevel.CurrentLevel.MaxXP = xpInfo.MaxXp;
			result.Levels.Add(heroLevel);
		}
		return result;
	}

	public void OnHeroXP(HeroXP packet)
	{
		NetCacheHeroLevels newHeroLevels = GetAllHeroXP(packet);
		if (m_prevHeroLevels != null)
		{
			foreach (HeroLevel newHeroLevel in newHeroLevels.Levels)
			{
				HeroLevel prevHeroLevel = m_prevHeroLevels.Levels.Find((HeroLevel obj) => obj.Class == newHeroLevel.Class);
				if (prevHeroLevel == null)
				{
					continue;
				}
				if (newHeroLevel != null && newHeroLevel.CurrentLevel != null && newHeroLevel.CurrentLevel.Level != prevHeroLevel.CurrentLevel.Level && (newHeroLevel.CurrentLevel.Level == 20 || newHeroLevel.CurrentLevel.Level == 30 || newHeroLevel.CurrentLevel.Level == 40 || newHeroLevel.CurrentLevel.Level == 50 || newHeroLevel.CurrentLevel.Level == 60))
				{
					if (newHeroLevel.Class == TAG_CLASS.DRUID)
					{
						BnetPresenceMgr.Get().SetGameField(5u, newHeroLevel.CurrentLevel.Level);
					}
					else if (newHeroLevel.Class == TAG_CLASS.HUNTER)
					{
						BnetPresenceMgr.Get().SetGameField(6u, newHeroLevel.CurrentLevel.Level);
					}
					else if (newHeroLevel.Class == TAG_CLASS.MAGE)
					{
						BnetPresenceMgr.Get().SetGameField(7u, newHeroLevel.CurrentLevel.Level);
					}
					else if (newHeroLevel.Class == TAG_CLASS.PALADIN)
					{
						BnetPresenceMgr.Get().SetGameField(8u, newHeroLevel.CurrentLevel.Level);
					}
					else if (newHeroLevel.Class == TAG_CLASS.PRIEST)
					{
						BnetPresenceMgr.Get().SetGameField(9u, newHeroLevel.CurrentLevel.Level);
					}
					else if (newHeroLevel.Class == TAG_CLASS.ROGUE)
					{
						BnetPresenceMgr.Get().SetGameField(10u, newHeroLevel.CurrentLevel.Level);
					}
					else if (newHeroLevel.Class == TAG_CLASS.SHAMAN)
					{
						BnetPresenceMgr.Get().SetGameField(11u, newHeroLevel.CurrentLevel.Level);
					}
					else if (newHeroLevel.Class == TAG_CLASS.WARLOCK)
					{
						BnetPresenceMgr.Get().SetGameField(12u, newHeroLevel.CurrentLevel.Level);
					}
					else if (newHeroLevel.Class == TAG_CLASS.WARRIOR)
					{
						BnetPresenceMgr.Get().SetGameField(13u, newHeroLevel.CurrentLevel.Level);
					}
					else if (newHeroLevel.Class == TAG_CLASS.DEMONHUNTER)
					{
						BnetPresenceMgr.Get().SetGameField(30u, newHeroLevel.CurrentLevel.Level);
					}
					else if (newHeroLevel.Class == TAG_CLASS.DEATHKNIGHT)
					{
						BnetPresenceMgr.Get().SetGameField(31u, newHeroLevel.CurrentLevel.Level);
					}
					else
					{
						Error.AddDevWarningNonRepeating("Missing Hero", "This hero isn't sending out toasts in OnHeroXP().");
					}
				}
				newHeroLevel.PrevLevel = prevHeroLevel.CurrentLevel;
			}
		}
		m_prevHeroLevels = newHeroLevels;
		OnNetCacheObjReceived(newHeroLevels);
	}

	private void OnAllHeroXP()
	{
		HeroXP packet = Network.Get().GetHeroXP();
		OnHeroXP(packet);
	}

	private void OnInitialClientState_ProfileNotices(ProfileNotices profileNotices)
	{
		List<ProfileNotice> receivedNotices = new List<ProfileNotice>();
		Network.Get().HandleProfileNotices(profileNotices.List, ref receivedNotices);
		m_receivedInitialProfileNotices = true;
		HandleIncomingProfileNotices(receivedNotices, isInitialNoticeList: true);
		HandleIncomingProfileNotices(m_queuedProfileNotices, isInitialNoticeList: true);
		m_queuedProfileNotices.Clear();
	}

	public void HandleIncomingProfileNotices(List<ProfileNotice> receivedNotices, bool isInitialNoticeList)
	{
		if (!m_receivedInitialProfileNotices)
		{
			m_queuedProfileNotices.AddRange(receivedNotices);
			return;
		}
		if (receivedNotices.Find((ProfileNotice obj) => obj.Type == ProfileNotice.NoticeType.GAINED_MEDAL) != null)
		{
			m_previousMedalInfo = null;
			NetCacheMedalInfo medalInfo = GetNetObject<NetCacheMedalInfo>();
			if (medalInfo != null)
			{
				medalInfo.PreviousMedalInfo = null;
			}
		}
		List<ProfileNotice> newNotices = FindNewNotices(receivedNotices);
		NetCacheProfileNotices netCacheNotices = GetNetObject<NetCacheProfileNotices>();
		if (netCacheNotices == null)
		{
			netCacheNotices = new NetCacheProfileNotices();
		}
		for (int i = 0; i < newNotices.Count; i++)
		{
			if (!m_ackedNotices.Contains(newNotices[i].NoticeID))
			{
				netCacheNotices.Notices.Add(newNotices[i]);
			}
		}
		OnNetCacheObjReceived(netCacheNotices);
		DelNewNoticesListener[] listeners = m_newNoticesListeners.ToArray();
		foreach (ProfileNotice notice in newNotices)
		{
			Log.Achievements.Print("NetCache.OnProfileNotices() sending {0} to {1} listeners", notice, listeners.Length);
		}
		DelNewNoticesListener[] array = listeners;
		foreach (DelNewNoticesListener listener in array)
		{
			Log.Achievements.Print("NetCache.OnProfileNotices(): sending notices to {0}::{1}", listener.Method.ReflectedType.Name, listener.Method.Name);
			listener(newNotices, isInitialNoticeList);
		}
	}

	private List<ProfileNotice> FindNewNotices(List<ProfileNotice> receivedNotices)
	{
		List<ProfileNotice> newNotices = new List<ProfileNotice>();
		NetCacheProfileNotices oldNotices = GetNetObject<NetCacheProfileNotices>();
		if (oldNotices == null)
		{
			newNotices.AddRange(receivedNotices);
		}
		else
		{
			foreach (ProfileNotice receivedNotice in receivedNotices)
			{
				if (oldNotices.Notices.Find((ProfileNotice obj) => obj.NoticeID == receivedNotice.NoticeID) == null)
				{
					newNotices.Add(receivedNotice);
				}
			}
		}
		return newNotices;
	}

	public void OnClientOptions(ClientOptions packet)
	{
		NetCacheClientOptions options = GetNetObject<NetCacheClientOptions>();
		bool firstRead = options == null;
		if (firstRead)
		{
			options = new NetCacheClientOptions();
		}
		if (packet.HasFailed && packet.Failed)
		{
			Debug.LogError("ReadClientOptions: packet.Failed=true. Unable to retrieve client options from UtilServer.");
			Network.Get().ShowConnectionFailureError("GLOBAL_ERROR_NETWORK_GENERIC");
			return;
		}
		foreach (PegasusUtil.ClientOption option in packet.Options)
		{
			ServerOption key = (ServerOption)option.Index;
			if (option.HasAsInt32)
			{
				options.ClientState[key] = new ClientOptionInt(option.AsInt32);
			}
			else if (option.HasAsInt64)
			{
				options.ClientState[key] = new ClientOptionLong(option.AsInt64);
			}
			else if (option.HasAsFloat)
			{
				options.ClientState[key] = new ClientOptionFloat(option.AsFloat);
			}
			else if (option.HasAsUint64)
			{
				options.ClientState[key] = new ClientOptionULong(option.AsUint64);
			}
		}
		options.UpdateServerState();
		OnNetCacheObjReceived(options);
		if (firstRead)
		{
			OptionsMigration.UpgradeServerOptions();
		}
		options.RemoveInvalidOptions();
	}

	private void SetClientOption(ServerOption type, ClientOptionBase newVal)
	{
		Type typeKey = typeof(NetCacheClientOptions);
		if (!m_netCache.TryGetValue(typeKey, out var cachedOptions) || !(cachedOptions is NetCacheClientOptions))
		{
			Debug.LogWarning("NetCache.OnClientOptions: Attempting to set an option before initializing the options cache.");
			return;
		}
		NetCacheClientOptions obj = (NetCacheClientOptions)cachedOptions;
		obj.ClientState[type] = newVal;
		obj.CheckForDispatchToServer();
		NetCacheChanged<NetCacheClientOptions>();
	}

	public void SetIntOption(ServerOption type, int val)
	{
		SetClientOption(type, new ClientOptionInt(val));
	}

	public void SetLongOption(ServerOption type, long val)
	{
		SetClientOption(type, new ClientOptionLong(val));
	}

	public void SetFloatOption(ServerOption type, float val)
	{
		SetClientOption(type, new ClientOptionFloat(val));
	}

	public void SetULongOption(ServerOption type, ulong val)
	{
		SetClientOption(type, new ClientOptionULong(val));
	}

	public void DeleteClientOption(ServerOption type)
	{
		SetClientOption(type, null);
	}

	public bool ClientOptionExists(ServerOption type)
	{
		NetCacheClientOptions allOptions = GetNetObject<NetCacheClientOptions>();
		if (allOptions == null)
		{
			return false;
		}
		if (!allOptions.ClientState.ContainsKey(type))
		{
			return false;
		}
		return allOptions.ClientState[type] != null;
	}

	public void DispatchClientOptionsToServer()
	{
		Get().GetNetObject<NetCacheClientOptions>()?.DispatchClientOptionsToServer();
	}

	private void OnFavoriteHeroesResponse()
	{
		FavoriteHeroesResponse packet = Network.Get().GetFavoriteHeroesResponse();
		NetCacheFavoriteHeroes netCacheFavoriteHeroes = new NetCacheFavoriteHeroes();
		foreach (FavoriteHero packetFavoriteHero in packet.FavoriteHeroes)
		{
			if (!EnumUtils.TryCast<TAG_CLASS>(packetFavoriteHero.ClassId, out var heroClass))
			{
				Debug.LogWarning($"NetCache.OnFavoriteHeroesResponse() unrecognized hero class {packetFavoriteHero.ClassId}");
				continue;
			}
			if (!EnumUtils.TryCast<TAG_PREMIUM>(packetFavoriteHero.Hero.Premium, out var heroPremium))
			{
				Debug.LogWarning($"NetCache.OnFavoriteHeroesResponse() unrecognized hero premium {packetFavoriteHero.Hero.Premium} for hero class {heroClass}");
				continue;
			}
			CardDefinition favoriteHero = new CardDefinition
			{
				Name = GameUtils.TranslateDbIdToCardId(packetFavoriteHero.Hero.Asset),
				Premium = heroPremium
			};
			netCacheFavoriteHeroes.FavoriteHeroes.Add((heroClass, favoriteHero));
		}
		OfflineDataCache.OfflineData data = OfflineDataCache.ReadOfflineDataFromFile();
		List<SetFavoriteHero> favoriteHeroesRequestList = OfflineDataCache.GenerateSetFavoriteHeroFromDiff(data, netCacheFavoriteHeroes);
		if (favoriteHeroesRequestList.Any())
		{
			foreach (SetFavoriteHero heroRequestToSend in favoriteHeroesRequestList)
			{
				CardDefinition cardDef = new CardDefinition
				{
					Name = GameUtils.TranslateDbIdToCardId(heroRequestToSend.FavoriteHero.Hero.Asset),
					Premium = (TAG_PREMIUM)heroRequestToSend.FavoriteHero.Hero.Premium
				};
				Network.Get().SetFavoriteHero((TAG_CLASS)heroRequestToSend.FavoriteHero.ClassId, cardDef, heroRequestToSend.IsFavorite);
			}
			OfflineDataCache.ClearFavoriteHeroesDirtyFlag();
		}
		OnNetCacheObjReceived(netCacheFavoriteHeroes);
		OfflineDataCache.CacheFavoriteHeroes(ref data, packet);
		OfflineDataCache.WriteOfflineDataToFile(data);
	}

	private void OnSetFavoriteHeroResponse()
	{
		Network.SetFavoriteHeroResponse setFavoriteHeroResponse = Network.Get().GetSetFavoriteHeroResponse();
		if (!setFavoriteHeroResponse.Success)
		{
			return;
		}
		if (TAG_CLASS.NEUTRAL == setFavoriteHeroResponse.HeroClass || setFavoriteHeroResponse.Hero == null)
		{
			Debug.LogWarning($"NetCache.OnSetFavoriteHeroResponse: setting hero was a success, but message contains invalid class ({setFavoriteHeroResponse.HeroClass}) and/or hero ({setFavoriteHeroResponse.Hero})");
			return;
		}
		NetCacheFavoriteHeroes netCacheFavoriteHeroes = Get().GetNetObject<NetCacheFavoriteHeroes>();
		if (netCacheFavoriteHeroes != null)
		{
			if (setFavoriteHeroResponse.IsFavorite)
			{
				netCacheFavoriteHeroes.FavoriteHeroes.Add((setFavoriteHeroResponse.HeroClass, setFavoriteHeroResponse.Hero));
			}
			else
			{
				netCacheFavoriteHeroes.FavoriteHeroes.Remove((setFavoriteHeroResponse.HeroClass, setFavoriteHeroResponse.Hero));
			}
			Log.CollectionManager.Print("CollectionManager.OnSetFavoriteHeroResponse: favorite hero status for {0} updated to {1}", setFavoriteHeroResponse.Hero, setFavoriteHeroResponse.IsFavorite);
		}
		CollectionManager.Get()?.UpdateFavoriteHero(setFavoriteHeroResponse.HeroClass, setFavoriteHeroResponse.Hero.Name, setFavoriteHeroResponse.Hero.Premium, setFavoriteHeroResponse.IsFavorite);
		PegasusShared.CardDef cardDef = new PegasusShared.CardDef
		{
			Asset = GameUtils.TranslateCardIdToDbId(setFavoriteHeroResponse.Hero.Name),
			Premium = (int)setFavoriteHeroResponse.Hero.Premium
		};
		OfflineDataCache.SetFavoriteHero((int)setFavoriteHeroResponse.HeroClass, cardDef, wasCalledOffline: false, setFavoriteHeroResponse.IsFavorite);
	}

	private void OnAccountLicensesInfoResponse()
	{
		AccountLicensesInfoResponse accountLicensesInfoResponse = Network.Get().GetAccountLicensesInfoResponse();
		NetCacheAccountLicenses netCacheAccountLicenses = new NetCacheAccountLicenses();
		foreach (AccountLicenseInfo packetAccountLicenseInfo in accountLicensesInfoResponse.List)
		{
			netCacheAccountLicenses.AccountLicenses[packetAccountLicenseInfo.License] = packetAccountLicenseInfo;
		}
		OnNetCacheObjReceived(netCacheAccountLicenses);
	}

	private void OnClientStaticAssetsResponse()
	{
		ClientStaticAssetsResponse packet = Network.Get().GetClientStaticAssetsResponse();
		if (packet != null)
		{
			OnNetCacheObjReceived(packet);
		}
	}

	private void OnMercenariesTeamListResponse()
	{
		MercenariesTeamListResponse packet = Network.Get().MercenariesTeamListResponse();
		if (packet != null && packet.HasTeamList)
		{
			OnNetCacheObjReceived(packet.TeamList);
		}
	}

	private void OnMercenariesMythicTreasureScalarsResponse()
	{
		NetCacheMercenariesMythicTreasureInfo treasureInfo = GetNetObject<NetCacheMercenariesMythicTreasureInfo>();
		if (treasureInfo == null)
		{
			treasureInfo = new NetCacheMercenariesMythicTreasureInfo();
			treasureInfo.MythicTreasureScalarMap = new Dictionary<int, NetCacheMercenariesMythicTreasureInfo.MythicTreasureScalar>();
		}
		MercenariesMythicTreasureScalarsResponse packet = Network.Get().MercenariesMythicTreasureScalarsResponse();
		if (packet == null)
		{
			OnNetCacheObjReceived(treasureInfo);
			return;
		}
		if (packet.HasErrorCode && packet.ErrorCode != 0)
		{
			OnNetCacheObjReceived(treasureInfo);
			return;
		}
		if (!packet.HasTreasureScalarList)
		{
			OnNetCacheObjReceived(treasureInfo);
			return;
		}
		treasureInfo.MythicTreasureScalarMap.Clear();
		foreach (MercenaryMythicTreasureScalar treasureScalar in packet.TreasureScalarList.TreasureScalars)
		{
			if (treasureScalar.HasTreasureId && treasureScalar.HasScalar)
			{
				treasureInfo.MythicTreasureScalarMap.Add(treasureScalar.TreasureId, new NetCacheMercenariesMythicTreasureInfo.MythicTreasureScalar
				{
					TreasureId = treasureScalar.TreasureId,
					Scalar = (int)treasureScalar.Scalar
				});
			}
		}
		OnNetCacheObjReceived(treasureInfo);
	}

	private void OnMercenariesMythicTreasureScalarPurchaseResponse()
	{
		NetCacheMercenariesMythicTreasureInfo treasureInfo = GetNetObject<NetCacheMercenariesMythicTreasureInfo>();
		if (treasureInfo == null)
		{
			treasureInfo = new NetCacheMercenariesMythicTreasureInfo();
			treasureInfo.MythicTreasureScalarMap = new Dictionary<int, NetCacheMercenariesMythicTreasureInfo.MythicTreasureScalar>();
		}
		MercenariesMythicTreasureScalarPurchaseResponse packet = Network.Get().MercenariesMythicTreasureScalarPurchaseResponse();
		if (packet == null)
		{
			OnNetCacheObjReceived(treasureInfo);
			return;
		}
		if (!packet.Success)
		{
			OnNetCacheObjReceived(treasureInfo);
			return;
		}
		if (!packet.HasTreasureScalar || !packet.TreasureScalar.HasTreasureId || !packet.TreasureScalar.HasScalar || !packet.HasPurchaseCount)
		{
			OnNetCacheObjReceived(treasureInfo);
			return;
		}
		if (!treasureInfo.MythicTreasureScalarMap.TryGetValue(packet.TreasureScalar.TreasureId, out var treasureScalar))
		{
			treasureScalar = new NetCacheMercenariesMythicTreasureInfo.MythicTreasureScalar
			{
				TreasureId = packet.TreasureScalar.TreasureId,
				Scalar = 0
			};
			treasureInfo.MythicTreasureScalarMap.Add(treasureScalar.TreasureId, treasureScalar);
		}
		if (treasureScalar.Scalar > packet.TreasureScalar.Scalar)
		{
			OnNetCacheObjReceived(treasureInfo);
			return;
		}
		treasureScalar.Scalar = (int)packet.TreasureScalar.Scalar;
		OnNetCacheObjReceived(treasureInfo);
	}

	private void OnMercenariesUpdateMythicBountyLevelResponse()
	{
		MercenariesMythicBountyLevelUpdate packet = Network.Get().MercenariesMythicBountyLevelUpdate();
		if (packet != null && packet.HasMythicBountyLevelInfo)
		{
			NetCacheMercenariesPlayerInfo playerInfo = GetNetObject<NetCacheMercenariesPlayerInfo>();
			int newMythicBountyLevel = (int)packet.MythicBountyLevelInfo.CurrentMythicBountyLevel;
			int newMinMythicBountyLevel = (int)packet.MythicBountyLevelInfo.MinMythicBountyLevel;
			int newMaxMythicBountyLevel = (int)packet.MythicBountyLevelInfo.MaxMythicBountyLevel;
			if (playerInfo.CurrentMythicBountyLevel != newMythicBountyLevel || playerInfo.MinMythicBountyLevel != newMinMythicBountyLevel || playerInfo.MaxMythicBountyLevel != newMaxMythicBountyLevel)
			{
				LettuceVillageDataUtil.ResetSavedMythicBountyLevel();
			}
			playerInfo.CurrentMythicBountyLevel = newMythicBountyLevel;
			playerInfo.MinMythicBountyLevel = newMinMythicBountyLevel;
			playerInfo.MaxMythicBountyLevel = newMaxMythicBountyLevel;
			OnNetCacheObjReceived(playerInfo);
		}
	}

	private void RegisterNetCacheHandlers()
	{
		Network network = Network.Get();
		network.RegisterNetHandler(DBAction.PacketID.ID, OnDBAction);
		network.RegisterNetHandler(GenericResponse.PacketID.ID, OnGenericResponse);
		network.RegisterNetHandler(InitialClientState.PacketID.ID, OnInitialClientState);
		network.RegisterNetHandler(MedalInfo.PacketID.ID, OnMedalInfo);
		network.RegisterNetHandler(BattlegroundsRatingInfoResponse.PacketID.ID, OnBaconRatingInfo);
		network.RegisterNetHandler(ProfileProgress.PacketID.ID, OnProfileProgress);
		network.RegisterNetHandler(GamesInfo.PacketID.ID, OnGamesInfo);
		network.RegisterNetHandler(CardValues.PacketID.ID, OnCardValues);
		network.RegisterNetHandler(GuardianVars.PacketID.ID, OnGuardianVars);
		network.RegisterNetHandler(PlayerRecords.PacketID.ID, OnPlayerRecords);
		network.RegisterNetHandler(RewardProgress.PacketID.ID, OnRewardProgress);
		network.RegisterNetHandler(HeroXP.PacketID.ID, OnAllHeroXP);
		network.RegisterNetHandler(CardBacks.PacketID.ID, OnCardBacks);
		network.RegisterNetHandler(SetFavoriteCardBackResponse.PacketID.ID, OnSetFavoriteCardBackResponse);
		network.RegisterNetHandler(FavoriteHeroesResponse.PacketID.ID, OnFavoriteHeroesResponse);
		network.RegisterNetHandler(SetFavoriteHeroResponse.PacketID.ID, OnSetFavoriteHeroResponse);
		network.RegisterNetHandler(AccountLicensesInfoResponse.PacketID.ID, OnAccountLicensesInfoResponse);
		network.RegisterNetHandler(DeckList.PacketID.ID, OnReceivedDeckHeaders);
		network.RegisterNetHandler(PVPDRStatsInfoResponse.PacketID.ID, OnPVPDRStatsInfo);
		network.RegisterNetHandler(CosmeticCoins.PacketID.ID, OnCoins);
		network.RegisterNetHandler(SetFavoriteCosmeticCoinResponse.PacketID.ID, OnSetFavoriteCosmeticCoinResponse);
		network.RegisterNetHandler(BattlegroundsHeroSkinsResponse.PacketID.ID, OnBattlegroundsHeroSkinsResponse);
		network.RegisterNetHandler(SetBattlegroundsFavoriteHeroSkinResponse.PacketID.ID, OnSetBattlegroundsFavoriteHeroSkinResponse);
		network.RegisterNetHandler(ClearBattlegroundsFavoriteHeroSkinResponse.PacketID.ID, OnClearBattlegroundsFavoriteHeroSkinResponse);
		network.RegisterNetHandler(BattlegroundsGuideSkinsResponse.PacketID.ID, OnBattlegroundsGuideSkinsResponse);
		network.RegisterNetHandler(SetBattlegroundsFavoriteGuideSkinResponse.PacketID.ID, OnSetBattlegroundsFavoriteGuideSkinResponse);
		network.RegisterNetHandler(ClearBattlegroundsFavoriteGuideSkinResponse.PacketID.ID, OnClearBattlegroundsFavoriteGuideSkinResponse);
		network.RegisterNetHandler(BattlegroundsBoardSkinsResponse.PacketID.ID, OnBattlegroundsBoardSkinsResponse);
		network.RegisterNetHandler(SetBattlegroundsFavoriteBoardSkinResponse.PacketID.ID, OnSetBattlegroundsFavoriteBoardSkinResponse);
		network.RegisterNetHandler(ClearBattlegroundsFavoriteBoardSkinResponse.PacketID.ID, OnClearBattlegroundsFavoriteBoardSkinResponse);
		network.RegisterNetHandler(BattlegroundsFinishersResponse.PacketID.ID, OnBattlegroundsFinishersResponse);
		network.RegisterNetHandler(SetBattlegroundsFavoriteFinisherResponse.PacketID.ID, OnSetBattlegroundsFavoriteFinisherResponse);
		network.RegisterNetHandler(ClearBattlegroundsFavoriteFinisherResponse.PacketID.ID, OnClearBattlegroundsFavoriteFinisherResponse);
		network.RegisterNetHandler(BattlegroundsEmotesResponse.PacketID.ID, OnBattlegroundsEmotesResponse);
		network.RegisterNetHandler(SetBattlegroundsEmoteLoadoutResponse.PacketID.ID, OnSetBattlegroundsEmoteLoadoutResponse);
		network.RegisterNetHandler(Deadend.PacketID.ID, OnHearthstoneUnavailableGame);
		network.RegisterNetHandler(DeadendUtil.PacketID.ID, OnHearthstoneUnavailableUtil);
		network.RegisterNetHandler(ClientStaticAssetsResponse.PacketID.ID, OnClientStaticAssetsResponse);
		network.RegisterNetHandler(MercenariesTeamListResponse.PacketID.ID, OnMercenariesTeamListResponse);
		network.RegisterNetHandler(LettuceMapResponse.PacketID.ID, OnLettuceMapResponse);
		network.RegisterNetHandler(MercenariesPlayerInfoResponse.PacketID.ID, OnMercenariesPlayerInfoResponse);
		network.RegisterNetHandler(MercenariesPvPWinUpdate.PacketID.ID, OnMercenariesPvPWinUpdate);
		network.RegisterNetHandler(MercenariesPlayerBountyInfoUpdate.PacketID.ID, OnMercenariesPlayerBountyInfoUpdate);
		network.RegisterNetHandler(MercenariesBountyAcknowledgeResponse.PacketID.ID, OnMercenariesBountyAcknowledgeResponse);
		network.RegisterNetHandler(MercenariesMythicTreasureScalarsResponse.PacketID.ID, OnMercenariesMythicTreasureScalarsResponse);
		network.RegisterNetHandler(MercenariesMythicTreasureScalarPurchaseResponse.PacketID.ID, OnMercenariesMythicTreasureScalarPurchaseResponse);
		network.RegisterNetHandler(MercenariesMythicBountyLevelUpdate.PacketID.ID, OnMercenariesUpdateMythicBountyLevelResponse);
		network.RegisterNetHandler(MercenariesGetVillageResponse.PacketID.ID, OnVillageDataResponse);
		network.RegisterNetHandler(MercenariesBuildingStateUpdate.PacketID.ID, OnVillageBuildingStateUpdated);
		network.RegisterNetHandler(MercenariesVisitorStateUpdate.PacketID.ID, OnVillageVisitorStateUpdated);
		network.RegisterNetHandler(MercenariesRefreshVillageResponse.PacketID.ID, OnRefreshVillageDataResponse);
	}

	public void RegisterCollectionManager(NetCacheCallback callback)
	{
		RegisterCollectionManager(callback, DefaultErrorHandler);
	}

	public void RegisterCollectionManager(NetCacheCallback callback, ErrorCallback errorCallback)
	{
		NetCacheBatchRequest request = new NetCacheBatchRequest(callback, errorCallback, RegisterCollectionManager);
		AddCollectionManagerToRequest(ref request);
		NetCacheMakeBatchRequest(request);
	}

	public void RegisterScreenCollectionManager(NetCacheCallback callback)
	{
		RegisterScreenCollectionManager(callback, DefaultErrorHandler);
	}

	public void RegisterScreenCollectionManager(NetCacheCallback callback, ErrorCallback errorCallback)
	{
		NetCacheBatchRequest request = new NetCacheBatchRequest(callback, errorCallback, RegisterScreenCollectionManager);
		AddCollectionManagerToRequest(ref request);
		request.AddRequests(new List<Request>
		{
			new Request(typeof(NetCacheCollection)),
			new Request(typeof(NetCacheFeatures)),
			new Request(typeof(NetCacheHeroLevels))
		});
		NetCacheMakeBatchRequest(request);
	}

	public void RegisterScreenForge(NetCacheCallback callback)
	{
		RegisterScreenForge(callback, DefaultErrorHandler);
	}

	public void RegisterScreenForge(NetCacheCallback callback, ErrorCallback errorCallback)
	{
		NetCacheBatchRequest request = new NetCacheBatchRequest(callback, errorCallback, RegisterScreenForge);
		AddCollectionManagerToRequest(ref request);
		request.AddRequests(new List<Request>
		{
			new Request(typeof(NetCacheFeatures)),
			new Request(typeof(NetCacheHeroLevels))
		});
		NetCacheMakeBatchRequest(request);
	}

	public void RegisterScreenTourneys(NetCacheCallback callback, ErrorCallback errorCallback)
	{
		NetCacheBatchRequest request = new NetCacheBatchRequest(callback, errorCallback, RegisterScreenTourneys);
		request.AddRequests(new List<Request>
		{
			new Request(typeof(NetCachePlayerRecords)),
			new Request(typeof(NetCacheDecks)),
			new Request(typeof(NetCacheFeatures)),
			new Request(typeof(NetCacheHeroLevels))
		});
		NetCacheMakeBatchRequest(request);
	}

	public void RegisterScreenFriendly(NetCacheCallback callback)
	{
		RegisterScreenFriendly(callback, DefaultErrorHandler);
	}

	public void RegisterScreenFriendly(NetCacheCallback callback, ErrorCallback errorCallback)
	{
		NetCacheBatchRequest request = new NetCacheBatchRequest(callback, errorCallback, RegisterScreenFriendly);
		request.AddRequests(new List<Request>
		{
			new Request(typeof(NetCacheDecks)),
			new Request(typeof(NetCacheHeroLevels))
		});
		NetCacheMakeBatchRequest(request);
	}

	public void RegisterScreenPractice(NetCacheCallback callback)
	{
		RegisterScreenPractice(callback, DefaultErrorHandler);
	}

	public void RegisterScreenPractice(NetCacheCallback callback, ErrorCallback errorCallback)
	{
		NetCacheBatchRequest request = new NetCacheBatchRequest(callback, errorCallback, RegisterScreenPractice);
		request.AddRequests(new List<Request>
		{
			new Request(typeof(NetCachePlayerRecords), rl: true),
			new Request(typeof(NetCacheDecks)),
			new Request(typeof(NetCacheFeatures)),
			new Request(typeof(NetCacheHeroLevels)),
			new Request(typeof(NetCacheRewardProgress))
		});
		NetCacheMakeBatchRequest(request);
	}

	public void RegisterScreenEndOfGame(NetCacheCallback callback)
	{
		RegisterScreenEndOfGame(callback, DefaultErrorHandler);
	}

	public void RegisterScreenEndOfGame(NetCacheCallback callback, ErrorCallback errorCallback)
	{
		if (ServiceManager.TryGet<GameMgr>(out var gameMgr) && gameMgr.IsSpectator())
		{
			Processor.ScheduleCallback(0f, realTime: false, delegate
			{
				callback();
			});
			return;
		}
		NetCacheBatchRequest request = new NetCacheBatchRequest(callback, errorCallback, RegisterScreenEndOfGame);
		request.AddRequests(new List<Request>
		{
			new Request(typeof(NetCacheMedalInfo), rl: true),
			new Request(typeof(NetCacheHeroLevels), rl: true)
		});
		NetCacheMakeBatchRequest(request);
		PegasusShared.GameType num = gameMgr?.GetGameType() ?? PegasusShared.GameType.GT_UNKNOWN;
		bool refreshTavernBrawlPlayerRecord = GameUtils.IsTavernBrawlGameType(num);
		if (num == PegasusShared.GameType.GT_VS_FRIEND && FriendChallengeMgr.Get().IsChallengeTavernBrawl())
		{
			NetCacheFeatures guardianVars = Get().GetNetObject<NetCacheFeatures>();
			if (guardianVars != null && guardianVars.FriendWeekAllowsTavernBrawlRecordUpdate && EventTimingManager.Get().IsEventActive(EventTimingType.FRIEND_WEEK))
			{
				refreshTavernBrawlPlayerRecord = true;
			}
		}
		if (refreshTavernBrawlPlayerRecord)
		{
			TavernBrawlManager.Get().RefreshPlayerRecord();
		}
	}

	public void RegisterScreenPackOpening(NetCacheCallback callback, ErrorCallback errorCallback)
	{
		NetCacheBatchRequest request = new NetCacheBatchRequest(callback, errorCallback, RegisterScreenPackOpening);
		request.AddRequest(new Request(typeof(NetCacheBoosters)));
		NetCacheMakeBatchRequest(request);
	}

	public void RegisterScreenBox(NetCacheCallback callback)
	{
		RegisterScreenBox(callback, DefaultErrorHandler);
	}

	public void RegisterScreenBox(NetCacheCallback callback, ErrorCallback errorCallback)
	{
		NetCacheBatchRequest request = new NetCacheBatchRequest(callback, errorCallback, RegisterScreenBox);
		Debug.Log("RegisterScreenBox tempGuardianVars=" + GetNetObject<NetCacheFeatures>());
		request.AddRequests(new List<Request>
		{
			new Request(typeof(NetCacheBoosters)),
			new Request(typeof(NetCacheClientOptions)),
			new Request(typeof(NetCacheProfileProgress)),
			new Request(typeof(NetCacheFeatures)),
			new Request(typeof(NetCacheMedalInfo)),
			new Request(typeof(NetCacheHeroLevels)),
			new Request(typeof(NetCachePlayerRecords), rl: true)
		});
		NetCacheMakeBatchRequest(request);
	}

	public void RegisterScreenStartup(NetCacheCallback callback)
	{
		RegisterScreenStartup(callback, DefaultErrorHandler);
	}

	public void RegisterScreenStartup(NetCacheCallback callback, ErrorCallback errorCallback)
	{
		NetCacheBatchRequest request = new NetCacheBatchRequest(callback, errorCallback, RegisterScreenStartup);
		request.AddRequests(new List<Request>
		{
			new Request(typeof(NetCacheProfileProgress))
		});
		NetCacheMakeBatchRequest(request);
	}

	public void RegisterScreenLogin(NetCacheCallback callback)
	{
		RegisterScreenLogin(callback, DefaultErrorHandler);
	}

	public void RegisterScreenLogin(NetCacheCallback callback, ErrorCallback errorCallback)
	{
		NetCacheBatchRequest request = new NetCacheBatchRequest(callback, errorCallback, RegisterScreenLogin);
		request.AddRequests(new List<Request>
		{
			new Request(typeof(NetCacheRewardProgress)),
			new Request(typeof(NetCachePlayerRecords)),
			new Request(typeof(NetCacheGoldBalance)),
			new Request(typeof(NetCacheHeroLevels)),
			new Request(typeof(NetCacheCardBacks), rl: true),
			new Request(typeof(NetCacheFavoriteHeroes), rl: true),
			new Request(typeof(NetCacheAccountLicenses)),
			new Request(typeof(ClientStaticAssetsResponse)),
			new Request(typeof(NetCacheClientOptions)),
			new Request(typeof(NetCacheCoins)),
			new Request(typeof(NetCacheBattlegroundsHeroSkins)),
			new Request(typeof(NetCacheBattlegroundsGuideSkins)),
			new Request(typeof(NetCacheBattlegroundsBoardSkins)),
			new Request(typeof(NetCacheBattlegroundsFinishers)),
			new Request(typeof(NetCacheBattlegroundsEmotes))
		});
		NetCacheMakeBatchRequest(request);
	}

	public void RegisterTutorialEndGameScreen(NetCacheCallback callback)
	{
		RegisterTutorialEndGameScreen(callback, DefaultErrorHandler);
	}

	public void RegisterTutorialEndGameScreen(NetCacheCallback callback, ErrorCallback errorCallback)
	{
		if (ServiceManager.TryGet<GameMgr>(out var gameMgr) && gameMgr.IsSpectator())
		{
			Processor.ScheduleCallback(0f, realTime: false, delegate
			{
				callback();
			});
			return;
		}
		NetCacheBatchRequest request = new NetCacheBatchRequest(callback, errorCallback, RegisterTutorialEndGameScreen);
		request.AddRequests(new List<Request>
		{
			new Request(typeof(NetCacheProfileProgress))
		});
		NetCacheMakeBatchRequest(request);
	}

	public void RegisterFriendChallenge(NetCacheCallback callback)
	{
		RegisterFriendChallenge(callback, DefaultErrorHandler);
	}

	public void RegisterFriendChallenge(NetCacheCallback callback, ErrorCallback errorCallback)
	{
		NetCacheBatchRequest request = new NetCacheBatchRequest(callback, errorCallback, RegisterFriendChallenge);
		request.AddRequest(new Request(typeof(NetCacheProfileProgress)));
		NetCacheMakeBatchRequest(request);
	}

	public void RegisterScreenBattlegrounds(NetCacheCallback callback)
	{
		RegisterScreenBattlegrounds(callback, DefaultErrorHandler);
	}

	public void RegisterScreenBattlegrounds(NetCacheCallback callback, ErrorCallback errorCallback)
	{
		NetCacheBatchRequest request = new NetCacheBatchRequest(callback, errorCallback, RegisterScreenBattlegrounds);
		request.AddRequests(new List<Request>
		{
			new Request(typeof(NetCacheFeatures))
		});
		NetCacheMakeBatchRequest(request);
	}

	private void AddCollectionManagerToRequest(ref NetCacheBatchRequest request)
	{
		request.AddRequests(new List<Request>
		{
			new Request(typeof(NetCacheProfileNotices)),
			new Request(typeof(NetCacheDecks)),
			new Request(typeof(NetCacheCollection)),
			new Request(typeof(NetCacheCardValues)),
			new Request(typeof(NetCacheArcaneDustBalance)),
			new Request(typeof(NetCacheClientOptions))
		});
	}

	private void OnPacketThrottled(int packetID, long retryMillis)
	{
		if (packetID != 201)
		{
			return;
		}
		float retryTime = Time.realtimeSinceStartup + (float)retryMillis / 1000f;
		foreach (NetCacheBatchRequest cacheRequest in m_cacheRequests)
		{
			cacheRequest.m_timeAdded = retryTime;
		}
	}

	public void Cheat_AddNotice(ProfileNotice notice)
	{
		if (HearthstoneApplication.IsInternal())
		{
			UnloadNetObject<NetCacheProfileNotices>();
			PopupDisplayManager.Get().RewardPopups.ClearSeenNotices();
			notice.NoticeID = 9999L;
			m_ackedNotices.Remove(notice.NoticeID);
			List<ProfileNotice> notices = new List<ProfileNotice>();
			notices.Add(notice);
			HandleIncomingProfileNotices(notices, isInitialNoticeList: false);
		}
	}
}
