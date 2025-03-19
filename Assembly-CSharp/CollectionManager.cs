using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using Assets;
using Blizzard.GameService.SDK.Client.Integration;
using Blizzard.T5.Core;
using Blizzard.T5.Core.Utils;
using Blizzard.T5.Services;
using Hearthstone;
using Hearthstone.Core;
using Hearthstone.DataModels;
using Hearthstone.Store;
using Hearthstone.UI;
using PegasusLettuce;
using PegasusShared;
using PegasusUtil;
using UnityEngine;

public class CollectionManager
{
	public delegate bool CollectibleCardFilterFunc(CollectibleCard card);

	public class PreconDeck
	{
		private long m_id;

		public long ID => m_id;

		public PreconDeck(long id)
		{
			m_id = id;
		}
	}

	public class TemplateDeck
	{
		public int m_id;

		public int m_deckTemplateId;

		public TAG_CLASS m_class;

		public int m_sortOrder;

		public Map<string, int> m_cardIds = new Map<string, int>();

		public Map<string, List<string>> m_sideboardCardIds = new Map<string, List<string>>();

		public string m_title;

		public string m_description;

		public string m_displayTexture;

		public EventTimingType m_event;

		public bool m_isStarterDeck;

		public FormatType m_formatType;

		public RuneType m_rune1;

		public RuneType m_rune2;

		public RuneType m_rune3;
	}

	public class FindCardsResult
	{
		public List<CollectibleCard> m_cards = new List<CollectibleCard>();

		public bool m_resultsWithoutManaFilterExist;

		public bool m_resultsWithoutSetFilterExist;

		public bool m_resultsUnownedExist;

		public bool m_resultsInWildExist;
	}

	public struct CardModification
	{
		public enum ModificationType
		{
			Add,
			Remove
		}

		public ModificationType modificationType;

		public string cardID;

		public TAG_PREMIUM premium;

		public int count;

		public bool seenBefore;
	}

	public delegate void DelCollectionManagerReady();

	public delegate void DelOnCollectionLoaded();

	public delegate void DelOnCollectionChanged();

	public delegate void DelOnDeckCreated(long id, string name);

	public delegate void DelOnDeckDeleted(CollectionDeck removedDeck);

	public delegate void DelOnDeckContents(long id);

	public delegate void DelOnAllDeckContents();

	public delegate void DelOnNewCardSeen(string cardID, TAG_PREMIUM premium);

	public delegate void DelOnCardRewardsInserted(List<string> cardIDs, List<TAG_PREMIUM> premium);

	public delegate void OnMassDisenchant(int amount);

	public delegate void OnEditedDeckChanged(CollectionDeck newDeck, CollectionDeck oldDeck, object callbackData);

	public delegate void FavoriteHeroChangedCallback(TAG_CLASS heroClass, NetCache.CardDefinition favoriteHero, bool isFavorite, object userData);

	public delegate void OnUIHeroOverrideCardRemovedCallback();

	public delegate void DeckAutoFillCallback(CollectionDeck deck, IEnumerable<DeckMaker.DeckFill> deckFill);

	private class TagCardSetEnumComparer : IEqualityComparer<TAG_CARD_SET>
	{
		public bool Equals(TAG_CARD_SET x, TAG_CARD_SET y)
		{
			return x == y;
		}

		public int GetHashCode(TAG_CARD_SET obj)
		{
			return (int)obj;
		}
	}

	private class TagClassEnumComparer : IEqualityComparer<TAG_CLASS>
	{
		public bool Equals(TAG_CLASS x, TAG_CLASS y)
		{
			return x == y;
		}

		public int GetHashCode(TAG_CLASS obj)
		{
			return (int)obj;
		}
	}

	private class TagCardTypeEnumComparer : IEqualityComparer<TAG_CARDTYPE>
	{
		public bool Equals(TAG_CARDTYPE x, TAG_CARDTYPE y)
		{
			return x == y;
		}

		public int GetHashCode(TAG_CARDTYPE obj)
		{
			return (int)obj;
		}
	}

	private struct CollectibleCardIndex
	{
		public string CardId;

		public TAG_PREMIUM Premium;

		public CollectibleCardIndex(string cardId, TAG_PREMIUM premium)
		{
			CardId = cardId;
			Premium = premium;
		}
	}

	private class CollectibleCardIndexComparer : IEqualityComparer<CollectibleCardIndex>
	{
		public bool Equals(CollectibleCardIndex x, CollectibleCardIndex y)
		{
			if (x.CardId == y.CardId)
			{
				return x.Premium == y.Premium;
			}
			return false;
		}

		public int GetHashCode(CollectibleCardIndex obj)
		{
			return (obj.CardId, obj.Premium).GetHashCode();
		}
	}

	private class FavoriteHeroChangedListener : EventListener<FavoriteHeroChangedCallback>
	{
		public void Fire(TAG_CLASS heroClass, NetCache.CardDefinition favoriteHero, bool isFavorite)
		{
			m_callback(heroClass, favoriteHero, isFavorite, m_userData);
		}
	}

	private class OnUIHeroOverrideCardRemovedListener : EventListener<OnUIHeroOverrideCardRemovedCallback>
	{
		public void Fire()
		{
			m_callback();
		}
	}

	private class PendingDeckCreateData
	{
		public DeckType m_deckType;

		public string m_name;

		public int m_heroDbId;

		public FormatType m_formatType;

		public DeckSourceType m_sourceType;

		public string m_pastedDeckHash;
	}

	private class PendingDeckDeleteData
	{
		public long m_deckId;
	}

	private class PendingDeckEditData
	{
		public long m_deckId;
	}

	private class PendingDeckRenameData
	{
		public long m_deckId;

		public string m_name;
	}

	public class DeckSort : IComparer<CollectionDeck>
	{
		public int Compare(CollectionDeck a, CollectionDeck b)
		{
			if (a.SortOrder == b.SortOrder)
			{
				return b.CreateDate.CompareTo(a.CreateDate);
			}
			return a.SortOrder.CompareTo(b.SortOrder);
		}
	}

	public class FindMercenariesResult
	{
		public List<LettuceMercenary> m_mercenaries = new List<LettuceMercenary>();
	}

	public delegate bool MercenaryFilterFunc(LettuceMercenary merc);

	public delegate void DelOnTeamCreated(long id);

	public delegate void DelOnTeamDeleted(LettuceTeam removedTeam);

	public delegate void DelOnTeamContents(long id);

	public delegate void DelOnAllTeamContents();

	public delegate void OnEditingTeamChanged(LettuceTeam newTeam, LettuceTeam oldTeam, object callbackData);

	private class TagRoleEnumComparer : IEqualityComparer<TAG_ROLE>
	{
		public bool Equals(TAG_ROLE x, TAG_ROLE y)
		{
			return x == y;
		}

		public int GetHashCode(TAG_ROLE obj)
		{
			return (int)obj;
		}
	}

	private class PendingTeamCreateData
	{
		public string m_name;

		public string m_pastedTeamHash;

		public long m_teamId;

		public PegasusLettuce.LettuceTeam.Type m_type;

		public uint m_sortOrder;
	}

	private class PendingTeamDeleteData
	{
		public long m_teamId;
	}

	private class PendingTeamEditData
	{
		public long m_teamId;
	}

	private class PendingMercenaryEditData
	{
		public long m_mercenaryId;
	}

	private Map<BattlegroundsHeroSkinId, int> m_BattlegroundsHeroSkinIdToHeroBaseCardId;

	private Map<BattlegroundsHeroSkinId, int> m_BattlegroundsHeroSkinIdToHeroSkinCardId;

	private Map<int, BattlegroundsHeroSkinId> m_BattlegroundsHeroSkinCardIdToHeroSkinId;

	private Map<int, int> m_BattlegroundsHeroSkinCardIdToHeroBaseCardId;

	private List<string> m_BattlegroundsHeroCardIds;

	private Map<BattlegroundsGuideSkinId, int> m_BattlegroundsGuideSkinIdToSkinCardId;

	private HashSet<BattlegroundsGuideSkinId> m_BattlegroundsGuideSkinIds;

	private List<string> m_BattlegroundsGuideCardIds;

	private static readonly string m_DefaultGuideCardId = "TB_BaconShopBob";

	private Map<int, BattlegroundsGuideSkinId> m_BattlegroundsGuideSkinCardIdToGuideSkinId;

	private HashSet<BattlegroundsBoardSkinId> m_BattlegroundsBoardSkinIds;

	private HashSet<BattlegroundsFinisherId> m_BattlegroundsFinisherIds;

	private HashSet<BattlegroundsEmoteId> m_BattlegroundsEmoteIds;

	public static FormatType s_PreHeroPickerFormat = FormatType.FT_STANDARD;

	public static FormatType s_HeroPickerFormat = FormatType.FT_STANDARD;

	private static Comparison<CollectibleCard> OrderedCardsSort = delegate(CollectibleCard a, CollectibleCard b)
	{
		int num = a.ManaCost.CompareTo(b.ManaCost);
		if (num == 0)
		{
			num = string.Compare(a.Name, b.Name, ignoreCase: false, Localization.GetCultureInfo());
			if (num == 0)
			{
				int premiumSortOrder = GetPremiumSortOrder(a.PremiumType);
				int premiumSortOrder2 = GetPremiumSortOrder(b.PremiumType);
				num = premiumSortOrder.CompareTo(premiumSortOrder2);
			}
		}
		return num;
	};

	private static CollectionManager s_instance;

	private bool m_collectionLoaded;

	private bool m_achievesLoaded;

	private bool m_netCacheLoaded;

	private bool m_duelsSessionInfoLoaded;

	private Map<long, CollectionDeck> m_decks = new Map<long, CollectionDeck>();

	private Map<long, CollectionDeck> m_baseDecks = new Map<long, CollectionDeck>();

	private Map<TAG_CLASS, PreconDeck> m_preconDecks = new Map<TAG_CLASS, PreconDeck>();

	private Map<TAG_CLASS, List<TemplateDeck>> m_templateDecks = new Map<TAG_CLASS, List<TemplateDeck>>();

	private Map<int, TemplateDeck> m_templateDeckMap = new Map<int, TemplateDeck>();

	private CollectionDeck m_EditedDeck;

	private List<TAG_CARD_SET> m_displayableCardSets = new List<TAG_CARD_SET>();

	[CompilerGenerated]
	private static DelCollectionManagerReady OnCollectionManagerReady;

	private List<DelOnCollectionLoaded> m_collectionLoadedListeners = new List<DelOnCollectionLoaded>();

	private List<DelOnCollectionChanged> m_collectionChangedListeners = new List<DelOnCollectionChanged>();

	private List<DelOnDeckCreated> m_deckCreatedListeners = new List<DelOnDeckCreated>();

	private List<DelOnDeckDeleted> m_deckDeletedListeners = new List<DelOnDeckDeleted>();

	private List<DelOnDeckContents> m_deckContentsListeners = new List<DelOnDeckContents>();

	private List<DelOnAllDeckContents> m_allDeckContentsListeners = new List<DelOnAllDeckContents>();

	private List<DelOnNewCardSeen> m_newCardSeenListeners = new List<DelOnNewCardSeen>();

	private List<DelOnCardRewardsInserted> m_cardRewardListeners = new List<DelOnCardRewardsInserted>();

	private List<OnMassDisenchant> m_massDisenchantListeners = new List<OnMassDisenchant>();

	private List<OnEditedDeckChanged> m_editedDeckChangedListeners = new List<OnEditedDeckChanged>();

	private List<Action> m_initialCollectionReceivedListeners = new List<Action>();

	private List<Action> m_renameFinishedListeners = new List<Action>();

	private List<Action<bool>> m_renameValidatedListeners = new List<Action<bool>>();

	private Map<long, float> m_pendingRequestDeckContents;

	private List<CollectibleCard> m_collectibleCards = new List<CollectibleCard>();

	private Map<int, int> m_coreCounterpartCardMap = new Map<int, int>();

	private Map<CollectibleCardIndex, CollectibleCard> m_collectibleCardIndex;

	private float m_collectionLastModifiedTime;

	private DateTime? m_timeOfLastPlayerDeckSave;

	private bool m_accountHasWildCards;

	private float m_lastSearchForWildCardsTime;

	private List<Action> m_onNetCacheDecksProcessed = new List<Action>();

	private Dictionary<long, DeckAutoFillCallback> m_smartDeckCallbackByDeckId = new Dictionary<long, DeckAutoFillCallback>();

	private HashSet<long> m_decksToRequestContentsAfterDeckSetDataResonse = new HashSet<long>();

	private HashSet<int> m_inTransitDeckCreateRequests = new HashSet<int>();

	private HashSet<TAG_CARD_SET> m_filterCardSet = new HashSet<TAG_CARD_SET>(new TagCardSetEnumComparer());

	private HashSet<TAG_CLASS> m_filterCardClass = new HashSet<TAG_CLASS>(new TagClassEnumComparer());

	private HashSet<TAG_CARDTYPE> m_filterCardType = new HashSet<TAG_CARDTYPE>(new TagCardTypeEnumComparer());

	private Map<TAG_CARD_SET, bool> m_filterIsSetRotatedCache;

	private Map<int, int> m_startsWithMatchNames = new Map<int, int>();

	private Map<string, TAG_CARD_SET> m_cachedCardSetValues = new Map<string, TAG_CARD_SET>();

	private List<TAG_CLASS> m_cardClasses = new List<TAG_CLASS>();

	private HashSet<int> m_UniqueHero = new HashSet<int>();

	private List<HashSet<string>> m_filterSubsets = new List<HashSet<string>>();

	private List<FavoriteHeroChangedListener> m_favoriteHeroChangedListeners = new List<FavoriteHeroChangedListener>();

	private List<OnUIHeroOverrideCardRemovedListener> m_onUIHeroOverrideCardRemovedListeners = new List<OnUIHeroOverrideCardRemovedListener>();

	private bool m_waitingForBoxTransition;

	private bool m_hasVisitedCollection;

	private bool m_editMode;

	private TAG_PREMIUM m_premiumPreference = TAG_PREMIUM.DIAMOND;

	private CollectibleDisplay m_collectibleDisplay;

	private PendingDeckCreateData m_pendingDeckCreate;

	private List<PendingDeckDeleteData> m_pendingDeckDeleteList;

	private List<PendingDeckRenameData> m_pendingDeckRenameList;

	private List<PendingDeckEditData> m_pendingDeckEditList;

	private long m_currentPVPDRDeckId;

	private DeckRuleset m_deckRuleset;

	private Dictionary<string, ShareableDeck> m_decksToCheatIn = new Dictionary<string, ShareableDeck>();

	private static Comparison<LettuceMercenary> OrderMercernaries = (LettuceMercenary a, LettuceMercenary b) => string.Compare(a.m_mercName, b.m_mercName, ignoreCase: false, Localization.GetCultureInfo());

	private HashSet<TAG_ROLE> m_filterCardRole = new HashSet<TAG_ROLE>(new TagRoleEnumComparer());

	private Map<long, LettuceTeam> m_teams = new Map<long, LettuceTeam>();

	private long m_editingTeamID;

	private List<LettuceMercenary> m_collectibleMercenaries = new List<LettuceMercenary>();

	private Map<long, LettuceMercenary> m_collectibleMercenaryDBIds = new Map<long, LettuceMercenary>();

	private List<LettuceMercenary> m_extraMercenaries = new List<LettuceMercenary>();

	private Map<long, LettuceMercenary> m_extraMercenaryDBIds = new Map<long, LettuceMercenary>();

	private List<DelOnTeamCreated> m_teamCreatedListeners = new List<DelOnTeamCreated>();

	private List<DelOnTeamDeleted> m_teamDeletedListeners = new List<DelOnTeamDeleted>();

	private List<DelOnTeamContents> m_teamContentsListeners = new List<DelOnTeamContents>();

	private List<DelOnAllTeamContents> m_allTeamContentsListeners = new List<DelOnAllTeamContents>();

	private List<OnEditingTeamChanged> m_editingTeamChangedListeners = new List<OnEditingTeamChanged>();

	private HashSet<long> m_teamsToRequestContentsAfterTeamSetDataResonse = new HashSet<long>();

	private HashSet<int> m_inTransitTeamCreateRequests = new HashSet<int>();

	private bool m_editTeamMode;

	private bool m_initialDataRequested;

	private bool m_mercsAndTeamsReceived;

	private bool m_playerInfoReceived;

	private bool m_hasVisitedDetailsDisplay;

	private MercenariesCollectionResponse m_mercenariesCollectionResponse;

	private LettuceTeamList m_mercTeamListResponse;

	private PendingTeamCreateData m_pendingTeamCreate;

	private List<PendingTeamDeleteData> m_pendingTeamDeleteList;

	private List<PendingTeamEditData> m_pendingTeamEditList;

	private List<PendingMercenaryEditData> m_pendingMercenaryEditList;

	public bool HasSeenOvercappedDeckInfoPopup { get; set; }

	public bool HasSeenExtraRunesDeckInfoPopup { get; set; }

	public event Action OnLettuceLoaded;

	public event Action OnMercenariesTrainingAddResponseReceived;

	public event Action OnMercenariesTrainingRemoveResponseReceived;

	public event Action OnMercenariesTrainingCollectResponseReceived;

	public event Action<int, int, TAG_PREMIUM> MercenaryArtVariationChangedEvent;

	private void BattlegroundsDataInit()
	{
		m_BattlegroundsHeroSkinIdToHeroBaseCardId = new Map<BattlegroundsHeroSkinId, int>();
		m_BattlegroundsHeroSkinIdToHeroSkinCardId = new Map<BattlegroundsHeroSkinId, int>();
		m_BattlegroundsHeroSkinCardIdToHeroSkinId = new Map<int, BattlegroundsHeroSkinId>();
		m_BattlegroundsHeroSkinCardIdToHeroBaseCardId = new Map<int, int>();
		m_BattlegroundsHeroCardIds = new List<string>();
		m_BattlegroundsGuideSkinIds = new HashSet<BattlegroundsGuideSkinId>();
		m_BattlegroundsGuideCardIds = new List<string>();
		m_BattlegroundsGuideSkinCardIdToGuideSkinId = new Map<int, BattlegroundsGuideSkinId>();
		m_BattlegroundsGuideSkinIdToSkinCardId = new Map<BattlegroundsGuideSkinId, int>();
		m_BattlegroundsBoardSkinIds = new HashSet<BattlegroundsBoardSkinId>();
		m_BattlegroundsFinisherIds = new HashSet<BattlegroundsFinisherId>();
		m_BattlegroundsEmoteIds = new HashSet<BattlegroundsEmoteId>();
		foreach (BattlegroundsHeroSkinDbfRecord record in GameDbf.BattlegroundsHeroSkin.GetRecords())
		{
			BattlegroundsHeroSkinId skinId = BattlegroundsHeroSkinId.FromTrustedValue(record.ID);
			m_BattlegroundsHeroSkinIdToHeroBaseCardId[skinId] = record.BaseCardId;
			m_BattlegroundsHeroSkinIdToHeroSkinCardId[skinId] = record.SkinCardId;
			m_BattlegroundsHeroSkinCardIdToHeroSkinId[record.SkinCardId] = skinId;
			m_BattlegroundsHeroSkinCardIdToHeroBaseCardId[record.SkinCardId] = record.BaseCardId;
		}
		foreach (CardHeroDbfRecord heroDbfRecord in GameDbf.CardHero.GetRecords((CardHeroDbfRecord card_hero) => card_hero.HeroType == CardHero.HeroType.BATTLEGROUNDS_HERO))
		{
			m_BattlegroundsHeroCardIds.Add(GameUtils.TranslateDbIdToCardId(heroDbfRecord.CardId));
		}
		foreach (BattlegroundsGuideSkinDbfRecord record2 in GameDbf.BattlegroundsGuideSkin.GetRecords())
		{
			BattlegroundsGuideSkinId skinId2 = BattlegroundsGuideSkinId.FromTrustedValue(record2.ID);
			m_BattlegroundsGuideSkinIds.Add(skinId2);
			m_BattlegroundsGuideSkinIdToSkinCardId[skinId2] = record2.SkinCardId;
			m_BattlegroundsGuideSkinCardIdToGuideSkinId[record2.SkinCardId] = skinId2;
		}
		foreach (CardHeroDbfRecord guideDbfRecord in GameDbf.CardHero.GetRecords((CardHeroDbfRecord card_hero) => card_hero.HeroType == CardHero.HeroType.BATTLEGROUNDS_GUIDE))
		{
			m_BattlegroundsGuideCardIds.Add(GameUtils.TranslateDbIdToCardId(guideDbfRecord.CardId));
		}
		foreach (BattlegroundsBoardSkinDbfRecord record4 in GameDbf.BattlegroundsBoardSkin.GetRecords())
		{
			BattlegroundsBoardSkinId skinId3 = BattlegroundsBoardSkinId.FromTrustedValue(record4.ID);
			m_BattlegroundsBoardSkinIds.Add(skinId3);
		}
		foreach (BattlegroundsFinisherDbfRecord record3 in GameDbf.BattlegroundsFinisher.GetRecords())
		{
			if (record3.Enabled)
			{
				BattlegroundsFinisherId finisherId = BattlegroundsFinisherId.FromTrustedValue(record3.ID);
				m_BattlegroundsFinisherIds.Add(finisherId);
			}
		}
		foreach (BattlegroundsEmoteDbfRecord record5 in GameDbf.BattlegroundsEmote.GetRecords())
		{
			BattlegroundsEmoteId emoteId = BattlegroundsEmoteId.FromTrustedValue(record5.ID);
			m_BattlegroundsEmoteIds.Add(emoteId);
		}
	}

	public bool IsValidBattlegroundsGuideSkinId(BattlegroundsGuideSkinId skinId)
	{
		return m_BattlegroundsGuideSkinIds.Contains(skinId);
	}

	public bool IsValidBattlegroundsBoardSkinId(BattlegroundsBoardSkinId skinId)
	{
		return m_BattlegroundsBoardSkinIds.Contains(skinId);
	}

	public bool IsValidBattlegroundsFinisherId(BattlegroundsFinisherId finisherId)
	{
		return m_BattlegroundsFinisherIds.Contains(finisherId);
	}

	public bool IsValidBattlegroundsEmoteId(BattlegroundsEmoteId emoteId)
	{
		return m_BattlegroundsEmoteIds.Contains(emoteId);
	}

	public bool GetBattlegroundsBaseCardIdForHeroSkinId(BattlegroundsHeroSkinId skinId, out int baseHeroCardId)
	{
		return m_BattlegroundsHeroSkinIdToHeroBaseCardId.TryGetValue(skinId, out baseHeroCardId);
	}

	public bool GetBattlegroundsHeroSkinCardIdForSkinId(BattlegroundsHeroSkinId skinId, out int skinHeroCardId)
	{
		return m_BattlegroundsHeroSkinIdToHeroSkinCardId.TryGetValue(skinId, out skinHeroCardId);
	}

	public bool GetBattlegroundsHeroSkinIdForSkinCardId(int skinCardId, out BattlegroundsHeroSkinId skinId)
	{
		return m_BattlegroundsHeroSkinCardIdToHeroSkinId.TryGetValue(skinCardId, out skinId);
	}

	public string GetBattlegroundsBaseHeroCardId(string skinOrBaseCardId)
	{
		int cardId = GameUtils.TranslateCardIdToDbId(skinOrBaseCardId);
		if (cardId == 0)
		{
			Log.CollectionManager.PrintError("GetBattlegroundsBaseCardId: could not find card with ID: {0}", skinOrBaseCardId);
			return skinOrBaseCardId;
		}
		if (!m_BattlegroundsHeroSkinCardIdToHeroBaseCardId.ContainsKey(cardId))
		{
			return skinOrBaseCardId;
		}
		int baseId = m_BattlegroundsHeroSkinCardIdToHeroBaseCardId[cardId];
		string baseCardId = GameUtils.TranslateDbIdToCardId(baseId);
		if (baseCardId == null || baseCardId.Length == 0)
		{
			Log.CollectionManager.PrintError("GetBattlegroundsBaseCardId: could not find base card ID string for ID: {0}", baseId);
			return skinOrBaseCardId;
		}
		return baseCardId;
	}

	public List<string> GetAllBattlegroundsHeroCardIds()
	{
		return m_BattlegroundsHeroCardIds;
	}

	public List<string> GetAllBattlegroundsGuideCardIds()
	{
		return m_BattlegroundsGuideCardIds;
	}

	public bool GetBattlegroundsGuideSkinCardIdForSkinId(BattlegroundsGuideSkinId skinId, out int cardId)
	{
		return m_BattlegroundsGuideSkinIdToSkinCardId.TryGetValue(skinId, out cardId);
	}

	public string GetFavoriteBattlegroundsGuideSkinCardId()
	{
		NetCache.NetCacheBattlegroundsGuideSkins netCacheBattlegroundsGuideSkins = NetCache.Get().GetNetObject<NetCache.NetCacheBattlegroundsGuideSkins>();
		if (netCacheBattlegroundsGuideSkins == null)
		{
			Log.CollectionManager.PrintError("Trying to invoke GetFavoriteBattlegroundsGuideSkinCardId before protobuf response from server.");
			return m_DefaultGuideCardId;
		}
		BattlegroundsGuideSkinId? favoriteGuideSkinId = netCacheBattlegroundsGuideSkins.BattlegroundsFavoriteGuideSkin;
		if (!favoriteGuideSkinId.HasValue)
		{
			return m_DefaultGuideCardId;
		}
		if (!m_BattlegroundsGuideSkinIdToSkinCardId.TryGetValue(favoriteGuideSkinId.Value, out var favoriteGuidSkinCardId))
		{
			Log.CollectionManager.PrintError("GetFavoriteBattlegroundsGuideSkinCardId: Could not find card for skin id: {1}", favoriteGuideSkinId);
			return m_DefaultGuideCardId;
		}
		return GameUtils.TranslateDbIdToCardId(favoriteGuidSkinCardId);
	}

	public bool IsBattlegroundsGuideCardId(string cardId)
	{
		return m_BattlegroundsGuideCardIds.Contains(cardId);
	}

	public bool OwnsBattlegroundsHeroSkin(string skinCardId)
	{
		CardDbfRecord skinCardRecord = GameUtils.GetCardRecord(skinCardId);
		if (skinCardRecord == null)
		{
			return false;
		}
		return OwnsBattlegroundsHeroSkin(skinCardRecord.ID);
	}

	public bool OwnsBattlegroundsHeroSkin(int skinCardId)
	{
		if (!m_BattlegroundsHeroSkinCardIdToHeroSkinId.TryGetValue(skinCardId, out var skinId))
		{
			return false;
		}
		return NetCache.Get().GetNetObject<NetCache.NetCacheBattlegroundsHeroSkins>().OwnedBattlegroundsSkins.Contains(skinId);
	}

	public bool IsBattlegroundsHeroCard(string cardId)
	{
		return m_BattlegroundsHeroCardIds.Contains(cardId);
	}

	public bool IsBattlegroundsHeroSkinCard(int cardId)
	{
		return m_BattlegroundsHeroSkinCardIdToHeroSkinId.ContainsKey(cardId);
	}

	public bool IsBattlegroundsGuideSkinCard(int cardId)
	{
		return m_BattlegroundsGuideSkinCardIdToGuideSkinId.ContainsKey(cardId);
	}

	public bool HasFavoriteBattlegroundsGuideSkin()
	{
		return NetCache.Get().GetNetObject<NetCache.NetCacheBattlegroundsGuideSkins>()?.BattlegroundsFavoriteGuideSkin.HasValue ?? false;
	}

	public bool GetFavoriteBattlegroundsGuideSkin(out BattlegroundsGuideSkinId favoriteSkinId)
	{
		NetCache.NetCacheBattlegroundsGuideSkins netCacheFavoriteGuides = NetCache.Get().GetNetObject<NetCache.NetCacheBattlegroundsGuideSkins>();
		if (netCacheFavoriteGuides != null && netCacheFavoriteGuides.BattlegroundsFavoriteGuideSkin.HasValue)
		{
			favoriteSkinId = netCacheFavoriteGuides.BattlegroundsFavoriteGuideSkin.Value;
			return true;
		}
		favoriteSkinId = default(BattlegroundsGuideSkinId);
		return false;
	}

	public bool GetBattlegroundsGuideSkinIdForCardId(int skinCardId, out BattlegroundsGuideSkinId skinId)
	{
		return m_BattlegroundsGuideSkinCardIdToGuideSkinId.TryGetValue(skinCardId, out skinId);
	}

	public bool OwnsBattlegroundsGuideSkin(string skinCardId)
	{
		CardDbfRecord skinCardRecord = GameUtils.GetCardRecord(skinCardId);
		if (skinCardRecord == null)
		{
			return false;
		}
		return OwnsBattlegroundsGuideSkin(skinCardRecord.ID);
	}

	public bool OwnsBattlegroundsGuideSkin(int skinCardId)
	{
		if (m_BattlegroundsGuideSkinCardIdToGuideSkinId.TryGetValue(skinCardId, out var skinId))
		{
			return NetCache.Get().GetNetObject<NetCache.NetCacheBattlegroundsGuideSkins>()?.OwnedBattlegroundsGuideSkins.Contains(skinId) ?? false;
		}
		return false;
	}

	public bool OwnsAnyBattlegroundsGuideSkin()
	{
		NetCache.NetCacheBattlegroundsGuideSkins battlegroundsGuideData = NetCache.Get().GetNetObject<NetCache.NetCacheBattlegroundsGuideSkins>();
		if (battlegroundsGuideData == null)
		{
			return false;
		}
		return battlegroundsGuideData.OwnedBattlegroundsGuideSkins.Count > 0;
	}

	public bool OwnsBattlegroundsBoardSkin(BattlegroundsBoardSkinId skinId)
	{
		if (skinId.IsDefaultBoard())
		{
			return true;
		}
		return NetCache.Get().GetNetObject<NetCache.NetCacheBattlegroundsBoardSkins>()?.OwnedBattlegroundsBoardSkins.Contains(skinId) ?? false;
	}

	public bool IsFavoriteBattlegroundsBoardSkin(BattlegroundsBoardSkinId skinId)
	{
		return NetCache.Get().GetNetObject<NetCache.NetCacheBattlegroundsBoardSkins>()?.BattlegroundsFavoriteBoardSkins.Contains(skinId) ?? false;
	}

	public bool OwnsBattlegroundsFinisher(BattlegroundsFinisherId finisherId)
	{
		if (finisherId.IsDefaultFinisher())
		{
			return true;
		}
		return NetCache.Get().GetNetObject<NetCache.NetCacheBattlegroundsFinishers>()?.OwnedBattlegroundsFinishers.Contains(finisherId) ?? false;
	}

	public bool IsFavoriteBattlegroundsFinisher(BattlegroundsFinisherId finisherId)
	{
		NetCache.NetCacheBattlegroundsFinishers battlegroundsFinishers = NetCache.Get().GetNetObject<NetCache.NetCacheBattlegroundsFinishers>();
		if (battlegroundsFinishers == null)
		{
			return false;
		}
		if (battlegroundsFinishers.BattlegroundsFavoriteFinishers.Count == 0)
		{
			return finisherId.IsDefaultFinisher();
		}
		return battlegroundsFinishers.BattlegroundsFavoriteFinishers.Contains(finisherId);
	}

	public bool OwnsBattlegroundsEmote(BattlegroundsEmoteId emoteId)
	{
		if (emoteId.IsDefaultEmote())
		{
			return true;
		}
		return NetCache.Get().GetNetObject<NetCache.NetCacheBattlegroundsEmotes>()?.OwnedBattlegroundsEmotes.Contains(emoteId) ?? false;
	}

	public bool HasAnyNewBattlegroundsSkins()
	{
		if (CountNewBattlegroundsHeroSkins() <= 0 && CountNewBattlegroundsGuideSkins() <= 0 && CountNewBattlegroundsBoardSkins() <= 0 && CountNewBattlegroundsFinishers() <= 0)
		{
			return CountNewBattlegroundsEmotes() > 0;
		}
		return true;
	}

	public int CountNewBattlegroundsHeroSkins()
	{
		return NetCache.Get().GetNetObject<NetCache.NetCacheBattlegroundsHeroSkins>().UnseenSkinIds.Count;
	}

	public int CountNewBattlegroundsGuideSkins()
	{
		return NetCache.Get().GetNetObject<NetCache.NetCacheBattlegroundsGuideSkins>().UnseenSkinIds.Count;
	}

	public int CountNewBattlegroundsBoardSkins()
	{
		return NetCache.Get().GetNetObject<NetCache.NetCacheBattlegroundsBoardSkins>().UnseenSkinIds.Count;
	}

	public int CountNewBattlegroundsFinishers()
	{
		return NetCache.Get().GetNetObject<NetCache.NetCacheBattlegroundsFinishers>()?.UnseenSkinIds.Count ?? 0;
	}

	public int CountNewBattlegroundsEmotes()
	{
		return NetCache.Get().GetNetObject<NetCache.NetCacheBattlegroundsEmotes>()?.UnseenEmoteIds.Count ?? 0;
	}

	public void MarkBattlegroundsHeroSkinSeen(string skinCardId, TAG_PREMIUM premium)
	{
		CardDbfRecord skinCardRecord = GameUtils.GetCardRecord(skinCardId);
		if (skinCardRecord == null || !m_BattlegroundsHeroSkinCardIdToHeroSkinId.TryGetValue(skinCardRecord.ID, out var skinId) || !Network.Get().TryAddSeenBattlegroundsHeroSkin(skinId))
		{
			return;
		}
		foreach (DelOnNewCardSeen newCardSeenListener in m_newCardSeenListeners)
		{
			newCardSeenListener(skinCardId, premium);
		}
	}

	public void MarkBattlegroundsGuideSkinSeen(string skinCardId, TAG_PREMIUM premium)
	{
		CardDbfRecord skinCardRecord = GameUtils.GetCardRecord(skinCardId);
		if (skinCardRecord == null || !m_BattlegroundsGuideSkinCardIdToGuideSkinId.TryGetValue(skinCardRecord.ID, out var skinId) || !Network.Get().TryAddSeenBattlegroundsGuideSkin(skinId))
		{
			return;
		}
		foreach (DelOnNewCardSeen newCardSeenListener in m_newCardSeenListeners)
		{
			newCardSeenListener(skinCardId, premium);
		}
	}

	public void MarkBattlegroundsBoardSkinSeen(BattlegroundsBoardSkinId skinId)
	{
		if (!skinId.IsDefaultBoard() && Network.Get().TryAddSeenBattlegroundsBoardSkin(skinId))
		{
			OnCollectionChanged();
		}
	}

	public void MarkBattlegroundsFinisherSeen(BattlegroundsFinisherId finisherId)
	{
		if (!finisherId.IsDefaultFinisher() && Network.Get().TryAddSeenBattlegroundsFinisher(finisherId))
		{
			OnCollectionChanged();
		}
	}

	public void MarkBattlegroundsEmoteSeen(BattlegroundsEmoteId emoteId)
	{
		if (!emoteId.IsDefaultEmote() && Network.Get().TryAddSeenBattlegroundsEmote(emoteId))
		{
			OnCollectionChanged();
		}
	}

	public bool ShouldShowNewBattlegroundsHeroSkinGlow(string skinCardId)
	{
		CardDbfRecord skinCardRecord = GameUtils.GetCardRecord(skinCardId);
		if (skinCardRecord == null)
		{
			return false;
		}
		if (!m_BattlegroundsHeroSkinCardIdToHeroSkinId.TryGetValue(skinCardRecord.ID, out var skinId))
		{
			return false;
		}
		NetCache.NetCacheBattlegroundsHeroSkins netCacheBattlegroundsHeroSkins = NetCache.Get().GetNetObject<NetCache.NetCacheBattlegroundsHeroSkins>();
		if (netCacheBattlegroundsHeroSkins == null)
		{
			Log.CollectionManager.PrintError("Trying to invoke ShouldShowNewBattlegroundsHeroSkinGlow before protobuf response from server.");
			return false;
		}
		return netCacheBattlegroundsHeroSkins.UnseenSkinIds.Contains(skinId);
	}

	public bool ShouldShowNewBattlegroundsGuideSkinGlow(string skinCardId)
	{
		CardDbfRecord skinCardRecord = GameUtils.GetCardRecord(skinCardId);
		if (skinCardRecord == null)
		{
			return false;
		}
		if (!m_BattlegroundsGuideSkinCardIdToGuideSkinId.TryGetValue(skinCardRecord.ID, out var skinId))
		{
			return false;
		}
		NetCache.NetCacheBattlegroundsGuideSkins netCacheBattlegroundsGuideSkins = NetCache.Get().GetNetObject<NetCache.NetCacheBattlegroundsGuideSkins>();
		if (netCacheBattlegroundsGuideSkins == null)
		{
			Log.CollectionManager.PrintError("Trying to invoke ShouldShowNewBattlegroundsGuideSkinGlow before protobuf response from server.");
			return false;
		}
		return netCacheBattlegroundsGuideSkins.UnseenSkinIds.Contains(skinId);
	}

	public bool ShouldShowNewBattlegroundsBoardSkinGlow(BattlegroundsBoardSkinId skinId)
	{
		if (skinId.IsDefaultBoard())
		{
			return false;
		}
		NetCache.NetCacheBattlegroundsBoardSkins netCacheBattlegroundsBoardSkins = NetCache.Get().GetNetObject<NetCache.NetCacheBattlegroundsBoardSkins>();
		if (netCacheBattlegroundsBoardSkins == null)
		{
			Log.CollectionManager.PrintError("Trying to invoke ShouldShowNewBattlegroundsBoardSkinGlow before protobuf response from server.");
			return false;
		}
		return netCacheBattlegroundsBoardSkins.UnseenSkinIds.Contains(skinId);
	}

	public bool ShouldShowNewBattlegroundsFinisherGlow(BattlegroundsFinisherId finisherId)
	{
		if (finisherId.IsDefaultFinisher())
		{
			return false;
		}
		NetCache.NetCacheBattlegroundsFinishers netCacheBattlegroundsFinishers = NetCache.Get().GetNetObject<NetCache.NetCacheBattlegroundsFinishers>();
		if (netCacheBattlegroundsFinishers == null)
		{
			Log.CollectionManager.PrintError("Trying to invoke ShouldShowNewBattlegroundsFinisherGlow before protobuf response from server.");
			return false;
		}
		return netCacheBattlegroundsFinishers.UnseenSkinIds.Contains(finisherId);
	}

	public bool ShouldShowNewBattlegroundsEmoteGlow(BattlegroundsEmoteId emoteId)
	{
		if (emoteId.IsDefaultEmote())
		{
			return false;
		}
		NetCache.NetCacheBattlegroundsEmotes netCacheBattlegroundsEmotes = NetCache.Get().GetNetObject<NetCache.NetCacheBattlegroundsEmotes>();
		if (netCacheBattlegroundsEmotes == null)
		{
			Log.CollectionManager.PrintError("Trying to invoke ShouldShowNewBattlegroundsEmoteGlow before protobuf response from server.");
			return false;
		}
		return netCacheBattlegroundsEmotes.UnseenEmoteIds.Contains(emoteId);
	}

	public BattlegroundsEmoteLoadoutDataModel CreateEmoteLoadoutDataModel()
	{
		NetCache.NetCacheBattlegroundsEmotes emoteData = NetCache.Get().GetNetObject<NetCache.NetCacheBattlegroundsEmotes>();
		BattlegroundsEmoteLoadoutDataModel dataModel = new BattlegroundsEmoteLoadoutDataModel();
		dataModel.EmoteList = new DataModelList<BattlegroundsEmoteDataModel>();
		if (emoteData == null)
		{
			Log.CollectionManager.PrintError("Trying to invoke CreateEmoteLoadoutDataModel before protobuf response from server.");
			return dataModel;
		}
		if (emoteData.CurrentLoadout != null)
		{
			BattlegroundsEmoteId[] emotes = emoteData.CurrentLoadout.Emotes;
			foreach (BattlegroundsEmoteId id in emotes)
			{
				BattlegroundsEmoteDbfRecord emoteDbf = GameDbf.BattlegroundsEmote.GetRecord(id.ToValue());
				if (emoteDbf != null)
				{
					CollectibleBattlegroundsEmote emote = new CollectibleBattlegroundsEmote(emoteDbf);
					dataModel.EmoteList.Add(emote.CreateEmoteDataModel());
				}
				else
				{
					dataModel.EmoteList.Add(new BattlegroundsEmoteDataModel());
				}
			}
		}
		return dataModel;
	}

	public bool IsEquippedBattlegroundsEmote(BattlegroundsEmoteId emoteId)
	{
		BaconCollectionDisplay bcd = Get().GetCollectibleDisplay() as BaconCollectionDisplay;
		bool inLoadout = false;
		if (bcd != null && bcd.TryCheckEmoteInLoadout(emoteId.ToValue(), out inLoadout))
		{
			return inLoadout;
		}
		NetCache.NetCacheBattlegroundsEmotes netCacheBattlegroundsEmotes = NetCache.Get().GetNetObject<NetCache.NetCacheBattlegroundsEmotes>();
		if (netCacheBattlegroundsEmotes == null)
		{
			Log.CollectionManager.PrintError("Trying to invoke ShouldShowNewBattlegroundsEmoteGlow before protobuf response from server.");
			return false;
		}
		for (int i = 0; i < netCacheBattlegroundsEmotes.CurrentLoadout.Emotes.Length; i++)
		{
			if (emoteId.Equals(netCacheBattlegroundsEmotes.CurrentLoadout.Emotes[i]))
			{
				return true;
			}
		}
		return false;
	}

	private static int GetPremiumSortOrder(TAG_PREMIUM premiumType)
	{
		switch (premiumType)
		{
		case TAG_PREMIUM.DIAMOND:
			return 3;
		case TAG_PREMIUM.SIGNATURE:
			return 2;
		case TAG_PREMIUM.GOLDEN:
			return 1;
		case TAG_PREMIUM.NORMAL:
			return 0;
		default:
			Debug.LogWarning("CollectionManager.GetPremiumSortOrder - Unknown premium type");
			return (int)premiumType;
		}
	}

	public NetCache.NetCacheCollection OnInitialCollectionReceived(Collection collection)
	{
		NetCache.NetCacheCollection result = new NetCache.NetCacheCollection();
		if (collection == null)
		{
			return result;
		}
		List<string> failedCardDbIds = new List<string>();
		for (int ii = 0; ii < collection.Stacks.Count; ii++)
		{
			CardStack cardStack = collection.Stacks[ii];
			NetCache.CardStack cardToInsert = new NetCache.CardStack();
			cardToInsert.Def.Name = GameUtils.TranslateDbIdToCardId(cardStack.CardDef.Asset);
			if (string.IsNullOrEmpty(cardToInsert.Def.Name))
			{
				failedCardDbIds.Add(cardStack.CardDef.Asset.ToString());
				continue;
			}
			cardToInsert.Def.Premium = (TAG_PREMIUM)cardStack.CardDef.Premium;
			cardToInsert.Date = TimeUtils.PegDateToFileTimeUtc(cardStack.LatestInsertDate);
			cardToInsert.Count = cardStack.Count;
			cardToInsert.NumSeen = cardStack.NumSeen;
			result.Stacks.Add(cardToInsert);
			result.TotalCardsOwned += cardToInsert.Count;
			if (GameUtils.IsCardCollectible(cardToInsert.Def.Name))
			{
				EntityDef entity = DefLoader.Get().GetEntityDef(cardToInsert.Def.Name);
				SetCounts(cardToInsert, entity);
				if (entity.IsCoreCard() && cardToInsert.Def.Premium == TAG_PREMIUM.NORMAL)
				{
					result.CoreCardsUnlockedPerClass[entity.GetClass()].Add(entity.GetCardId());
				}
			}
		}
		Action[] array = m_initialCollectionReceivedListeners.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			array[i]();
		}
		if (failedCardDbIds.Count > 0)
		{
			string cardList = string.Join(", ", failedCardDbIds.ToArray());
			Error.AddDevWarning("Card Errors", "CollectionManager.OnInitialCollectionRecieved: Cards with the following dbIds could not be found:\n{0}", cardList);
		}
		return result;
	}

	private void OnCardSale()
	{
		Network.CardSaleResult sale = Network.Get().GetCardSaleResult();
		bool success;
		switch (sale.Action)
		{
		case Network.CardSaleResult.SaleResult.CARD_WAS_SOLD:
			CraftingManager.Get().OnCardDisenchanted(sale);
			success = true;
			break;
		case Network.CardSaleResult.SaleResult.CARD_WAS_BOUGHT:
			CraftingManager.Get().OnCardCreated(sale);
			success = true;
			break;
		case Network.CardSaleResult.SaleResult.CARD_WAS_UPGRADED:
			CraftingManager.Get().OnCardUpgraded(sale);
			success = true;
			break;
		case Network.CardSaleResult.SaleResult.SOULBOUND:
			CraftingManager.Get().OnCardDisenchantSoulboundError(sale);
			success = false;
			break;
		case Network.CardSaleResult.SaleResult.FAILED_WRONG_SELL_PRICE:
			CraftingManager.Get().OnCardValueChangedError(sale);
			success = false;
			break;
		case Network.CardSaleResult.SaleResult.FAILED_WRONG_BUY_PRICE:
			CraftingManager.Get().OnCardValueChangedError(sale);
			success = false;
			break;
		case Network.CardSaleResult.SaleResult.FAILED_NO_PERMISSION:
			CraftingManager.Get().OnCardPermissionError(sale);
			success = false;
			break;
		case Network.CardSaleResult.SaleResult.FAILED_EVENT_NOT_ACTIVE:
			CraftingManager.Get().OnCardCraftingEventNotActiveError(sale);
			success = false;
			break;
		case Network.CardSaleResult.SaleResult.GENERIC_FAILURE:
			CraftingManager.Get().OnCardGenericError(sale);
			success = false;
			break;
		case Network.CardSaleResult.SaleResult.COUNT_MISMATCH:
			CraftingManager.Get().OnCardCountError(sale);
			success = false;
			break;
		default:
			CraftingManager.Get().OnCardUnknownError(sale);
			success = false;
			break;
		}
		string cardSaleInfo = $"CollectionManager.OnCardSale {sale.Action} for card {sale.AssetName} (asset {sale.AssetID}) premium {sale.Premium}";
		if (!success)
		{
			Debug.LogWarning(cardSaleInfo);
			return;
		}
		Log.Crafting.Print(cardSaleInfo);
		OnCollectionChanged();
	}

	private void OnMassDisenchantResponse()
	{
		Network.MassDisenchantResponse massDisenchantResponse = Network.Get().GetMassDisenchantResponse();
		if (massDisenchantResponse.Amount == 0)
		{
			Debug.LogError("CollectionManager.OnMassDisenchantResponse(): Amount is 0. This means the backend failed to mass disenchant correctly.");
			return;
		}
		OnMassDisenchant[] array = m_massDisenchantListeners.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			array[i](massDisenchantResponse.Amount);
		}
	}

	public void UpdateFavoriteHero(TAG_CLASS heroClass, string heroCardId, TAG_PREMIUM premium, bool isFavorite)
	{
		if (m_favoriteHeroChangedListeners.Count > 0)
		{
			NetCache.CardDefinition networkCard = new NetCache.CardDefinition();
			networkCard.Name = heroCardId;
			networkCard.Premium = premium;
			FavoriteHeroChangedListener[] array = m_favoriteHeroChangedListeners.ToArray();
			for (int i = 0; i < array.Length; i++)
			{
				array[i].Fire(heroClass, networkCard, isFavorite);
			}
		}
	}

	private void OnPVPDRSessionInfoResponse()
	{
		m_currentPVPDRDeckId = 0L;
		PVPDRSessionInfoResponse response = Network.Get().GetPVPDRSessionInfoResponse();
		if (response.HasSession)
		{
			m_currentPVPDRDeckId = response.Session.DeckId;
		}
		m_duelsSessionInfoLoaded = true;
	}

	public void NetCache_OnDecksReceived()
	{
		foreach (NetCache.DeckHeader netCacheDeck in NetCache.Get().GetNetObject<NetCache.NetCacheDecks>().Decks)
		{
			if (netCacheDeck.Type == DeckType.NORMAL_DECK && GetDeck(netCacheDeck.ID) == null && DefLoader.Get().GetEntityDef(netCacheDeck.Hero) != null)
			{
				AddDeck(netCacheDeck, updateNetCache: false);
			}
		}
		for (int i = m_onNetCacheDecksProcessed.Count - 1; i >= 0; i--)
		{
			m_onNetCacheDecksProcessed[i]();
		}
	}

	public void AddOnNetCacheDecksProcessedListener(Action a)
	{
		m_onNetCacheDecksProcessed.Add(a);
	}

	public void RemoveOnNetCacheDecksProcessedListener(Action a)
	{
		m_onNetCacheDecksProcessed.Remove(a);
	}

	public void OnFavoriteBattlegroundsGuideSkinChanged(BattlegroundsGuideSkinId? newFavoriteBattlegroundsGuideSkinID)
	{
	}

	public void OnInitialClientStateDeckContents(NetCache.NetCacheDecks netCacheDecks, List<DeckContents> deckContents)
	{
		if (deckContents == null)
		{
			return;
		}
		foreach (NetCache.DeckHeader deckHeader in netCacheDecks.Decks)
		{
			if (deckHeader.Type != DeckType.PRECON_DECK)
			{
				AddDeck(deckHeader, updateNetCache: false);
			}
		}
		UpdateFromDeckContents(deckContents);
	}

	private void OnGetDeckContentsResponse()
	{
		GetDeckContentsResponse response = Network.Get().GetDeckContentsResponse();
		UpdateFromDeckContents(response.Decks);
	}

	public void UpdateFromDeckContents(List<DeckContents> deckContents)
	{
		if (deckContents == null)
		{
			Log.CollectionManager.PrintError("Could not update CollectionManager from Deck Contents. Deck Contents was null");
			return;
		}
		foreach (DeckContents deckContent in deckContents)
		{
			if (deckContent == null)
			{
				Log.CollectionManager.PrintError("UpdateFromDeckContents: deckContents contained a null deckContent.");
				continue;
			}
			Network.DeckContents netDeck = Network.DeckContents.FromPacket(deckContent);
			if (m_pendingRequestDeckContents != null)
			{
				m_pendingRequestDeckContents.Remove(netDeck.Deck);
			}
			CollectionDeck deck = null;
			if (m_decks != null)
			{
				m_decks.TryGetValue(netDeck.Deck, out deck);
			}
			else
			{
				Log.CollectionManager.PrintError("UpdateFromDeckContents: m_decks is null!");
			}
			CollectionDeck baseDeck = null;
			if (m_baseDecks != null)
			{
				m_baseDecks.TryGetValue(netDeck.Deck, out baseDeck);
			}
			else
			{
				Log.CollectionManager.PrintError("UpdateFromDeckContents: m_baseDecks is null!");
			}
			if (deck != null && baseDeck != null)
			{
				bool isDeckBeingEdited = deck != null && IsInEditMode() && GetEditedDeck().ID == deck.ID;
				if (!isDeckBeingEdited)
				{
					deck.ClearSlotContents();
				}
				baseDeck.ClearSlotContents();
				foreach (Network.CardUserData entry in netDeck.Cards)
				{
					string cardId = GameUtils.TranslateDbIdToCardId(entry.DbId);
					if (cardId == null)
					{
						continue;
					}
					for (int numToAdd = entry.Count; numToAdd > 0; numToAdd--)
					{
						if (!isDeckBeingEdited)
						{
							deck.AddCard(cardId, entry.Premium, true, null);
						}
						baseDeck.AddCard(cardId, entry.Premium, true, null);
					}
				}
				foreach (Network.SideboardCardUserData sideboardEntry in netDeck.SideboardCards)
				{
					string cardId2 = GameUtils.TranslateDbIdToCardId(sideboardEntry.Card.DbId);
					if (cardId2 == null)
					{
						continue;
					}
					for (int numToAdd2 = sideboardEntry.Card.Count; numToAdd2 > 0; numToAdd2--)
					{
						if (!isDeckBeingEdited)
						{
							deck.AddCardToSideboard(cardId2, sideboardEntry.LinkedCardDbId, sideboardEntry.Card.Premium, allowInvalid: true);
						}
						baseDeck.AddCardToSideboard(cardId2, sideboardEntry.LinkedCardDbId, sideboardEntry.Card.Premium, allowInvalid: true);
					}
				}
				deck.MarkNetworkContentsLoaded();
			}
			FireDeckContentsEvent(netDeck.Deck);
		}
		foreach (CollectionDeck value in GetDecks().Values)
		{
			if (!value.NetworkContentsLoaded())
			{
				return;
			}
		}
		LogAllDeckStringsInCollection();
		if (m_pendingRequestDeckContents != null)
		{
			float now = Time.realtimeSinceStartup;
			long[] deckIds = (from kv in m_pendingRequestDeckContents
				where now - kv.Value > 10f
				select kv.Key).ToArray();
			for (int i = 0; i < deckIds.Length; i++)
			{
				m_pendingRequestDeckContents.Remove(deckIds[i]);
			}
		}
		if (m_pendingRequestDeckContents == null || m_pendingRequestDeckContents.Count == 0)
		{
			FireAllDeckContentsEvent();
		}
	}

	private void OnDBAction()
	{
		Network.DBAction response = Network.Get().GetDeckResponse();
		Log.CollectionManager.Print($"MetaData:{response.MetaData} DBAction:{response.Action} Result:{response.Result}");
		bool isNameAction = false;
		bool isContentsAction = false;
		switch (response.Action)
		{
		case Network.DBAction.ActionType.CREATE_DECK:
			if (response.Result != Network.DBAction.ResultType.SUCCESS && CollectionDeckTray.Get() != null)
			{
				CollectionDeckTray.Get().GetDecksContent().CreateNewDeckCancelled();
			}
			break;
		case Network.DBAction.ActionType.SET_DECK:
			isContentsAction = true;
			if (m_decksToRequestContentsAfterDeckSetDataResonse.Contains(response.MetaData))
			{
				Network.Get().RequestDeckContents(response.MetaData);
				m_decksToRequestContentsAfterDeckSetDataResonse.Remove(response.MetaData);
			}
			if (m_timeOfLastPlayerDeckSave.HasValue)
			{
				DateTime now = DateTime.Now;
				DateTime? timeOfLastPlayerDeckSave = m_timeOfLastPlayerDeckSave;
				double timeSinceDeckSetDataSent = (now - timeOfLastPlayerDeckSave).Value.TotalSeconds;
				TelemetryManager.Client().SendDeckUpdateResponseInfo((float)timeSinceDeckSetDataSent);
				SetTimeOfLastPlayerDeckSave(null);
			}
			if (m_pendingDeckEditList != null && m_pendingDeckEditList.Any())
			{
				m_pendingDeckEditList.RemoveAll((PendingDeckEditData d) => d.m_deckId == response.MetaData);
			}
			break;
		case Network.DBAction.ActionType.RENAME_DECK:
			isNameAction = true;
			if (m_pendingDeckRenameList != null && m_pendingDeckRenameList.Any())
			{
				m_pendingDeckRenameList.RemoveAll((PendingDeckRenameData d) => d.m_deckId == response.MetaData);
			}
			break;
		}
		if (!(isNameAction || isContentsAction))
		{
			return;
		}
		long deckID = response.MetaData;
		CollectionDeck deck = GetDeck(deckID);
		CollectionDeck baseDeck = GetBaseDeck(deckID);
		if (deck == null)
		{
			return;
		}
		if (response.Result == Network.DBAction.ResultType.SUCCESS)
		{
			Log.CollectionManager.Print(string.Format("CollectionManager.OnDBAction(): overwriting baseDeck with {0} updated deck ({1}:{2})", deck.IsValidForRuleset ? "valid" : "INVALID", deck.ID, deck.Name));
			baseDeck.CopyFrom(deck);
			NetCache.NetCacheDecks netCacheDecks = NetCache.Get().GetNetObject<NetCache.NetCacheDecks>();
			if (netCacheDecks != null && netCacheDecks.Decks != null)
			{
				NetCache.DeckHeader netCacheDeck = netCacheDecks.Decks.Find((NetCache.DeckHeader deckHeader) => deckHeader.ID == deckID);
				if (netCacheDeck != null)
				{
					RuneType[] runeOrder = deck.GetRuneOrder();
					netCacheDeck.HeroOverridden = deck.HeroOverridden;
					netCacheDeck.SeasonId = deck.SeasonId;
					netCacheDeck.BrawlLibraryItemId = deck.BrawlLibraryItemId;
					netCacheDeck.NeedsName = deck.NeedsName;
					netCacheDeck.FormatType = deck.FormatType;
					netCacheDeck.LastModified = DateTime.Now;
					netCacheDeck.Rune1 = runeOrder[0];
					netCacheDeck.Rune2 = runeOrder[1];
					netCacheDeck.Rune3 = runeOrder[2];
				}
			}
		}
		else
		{
			Log.CollectionManager.Print($"CollectionManager.OnDBAction(): overwriting deck that failed to update with base deck ({baseDeck.ID}:{baseDeck.Name})");
			deck.CopyFrom(baseDeck);
		}
		if (isNameAction)
		{
			deck.OnNameChangeComplete();
		}
		if (isContentsAction)
		{
			deck.OnContentChangesComplete();
		}
	}

	private void OnDeckCreatedNetworkResponse()
	{
		int? requestId;
		NetCache.DeckHeader deck = Network.Get().GetCreatedDeck(out requestId);
		OnDeckCreated(deck, requestId);
		List<DeckInfo> deckListFromNetCache = NetCache.Get().GetDeckListFromNetCache();
		OfflineDataCache.CacheLocalAndOriginalDeckList(deckListFromNetCache, deckListFromNetCache);
	}

	private void OnDeckCreated(NetCache.DeckHeader deck, int? requestId)
	{
		Log.CollectionManager.Print($"DeckCreated:{deck.Name} ID:{deck.ID} Hero:{deck.Hero}");
		m_pendingDeckCreate = null;
		AddDeck(deck).MarkNetworkContentsLoaded();
		if (requestId.HasValue)
		{
			if (!m_inTransitDeckCreateRequests.Contains(requestId.Value))
			{
				return;
			}
			m_inTransitDeckCreateRequests.Remove(requestId.Value);
		}
		DelOnDeckCreated[] array = m_deckCreatedListeners.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			array[i](deck.ID, deck.Name);
		}
	}

	private void OnDeckDeleted()
	{
		OnDeckDeleted(Network.Get().GetDeletedDeckID());
	}

	private void OnDeckDeleted(long deckId)
	{
		Log.CollectionManager.Print("CollectionManager.OnDeckDeleted");
		Log.CollectionManager.Print($"DeckDeleted:{deckId}");
		CollectionDeck removedDeck = RemoveDeck(deckId);
		if (m_pendingDeckDeleteList != null && m_pendingDeckDeleteList.Any())
		{
			m_pendingDeckDeleteList.RemoveAll((PendingDeckDeleteData d) => d.m_deckId == deckId);
		}
		if (CollectionDeckTray.Get() == null)
		{
			return;
		}
		CollectionDeck editedDeckInEditMode = GetEditedDeck();
		if (IsInEditMode() && editedDeckInEditMode != null && editedDeckInEditMode.ID == deckId)
		{
			Navigation.Pop();
			if (SceneMgr.Get().GetMode() != SceneMgr.Mode.HUB)
			{
				SceneMgr.Get().SetNextMode(SceneMgr.Mode.HUB);
				AlertPopup.PopupInfo info = new AlertPopup.PopupInfo
				{
					m_headerText = GameStrings.Get("GLUE_OFFLINE_FEATURE_DISABLED_HEADER"),
					m_text = GameStrings.Get("GLUE_OFFLINE_DECK_DELETED_REMOTELY_ERROR_BODY"),
					m_responseDisplay = AlertPopup.ResponseDisplay.OK,
					m_showAlertIcon = true
				};
				DialogManager.Get().ShowPopup(info);
			}
		}
		if (removedDeck != null)
		{
			DelOnDeckDeleted[] array = m_deckDeletedListeners.ToArray();
			for (int i = 0; i < array.Length; i++)
			{
				array[i](removedDeck);
			}
		}
	}

	public void OnDeckDeletedWhileOffline(long deckId)
	{
		OnDeckDeleted(deckId);
	}

	public void AddPendingDeckDelete(long deckId)
	{
		if (m_pendingDeckDeleteList == null)
		{
			m_pendingDeckDeleteList = new List<PendingDeckDeleteData>();
		}
		m_pendingDeckDeleteList.Add(new PendingDeckDeleteData
		{
			m_deckId = deckId
		});
	}

	public void AddPendingDeckEdit(long deckId)
	{
		if (m_pendingDeckEditList == null)
		{
			m_pendingDeckEditList = new List<PendingDeckEditData>();
		}
		m_pendingDeckEditList.Add(new PendingDeckEditData
		{
			m_deckId = deckId
		});
	}

	public void AddPendingDeckRename(long deckId, string name, bool playerInitiated)
	{
		if (m_pendingDeckRenameList == null)
		{
			m_pendingDeckRenameList = new List<PendingDeckRenameData>();
		}
		m_pendingDeckRenameList.Add(new PendingDeckRenameData
		{
			m_deckId = deckId,
			m_name = name
		});
		if (playerInitiated)
		{
			Action[] array = m_renameFinishedListeners.ToArray();
			for (int i = 0; i < array.Length; i++)
			{
				array[i]();
			}
		}
	}

	private void OnDeckRenamed()
	{
		Network.DeckName renamedDeck = Network.Get().GetRenamedDeck();
		long id = renamedDeck.Deck;
		string newName = renamedDeck.Name;
		bool isCensored = renamedDeck.IsCensored;
		OnDeckRenamed(id, newName, isCensored);
	}

	private void OnDeckRenamed(long deckId, string newName, bool isCensored)
	{
		Log.CollectionManager.Print($"OnDeckRenamed {deckId}");
		CollectionDeck baseDeck = GetBaseDeck(deckId);
		CollectionDeck deck = GetDeck(deckId);
		if (baseDeck == null || deck == null)
		{
			Debug.LogWarning($"For deck with ID {deckId}, unable to handle OnDeckRenamed event to new name {newName} due to null deck or null baseDeck");
			return;
		}
		baseDeck.Name = newName;
		deck.Name = newName;
		if (CollectionDeckTray.Get() != null)
		{
			CollectionDeckTray.Get().m_decksContent.UpdateDeckName(null, shouldValidateDeckName: false);
		}
		NetCache.NetCacheDecks netCacheDecks = NetCache.Get().GetNetObject<NetCache.NetCacheDecks>();
		if (netCacheDecks != null && netCacheDecks.Decks != null)
		{
			NetCache.DeckHeader renamedDeck = netCacheDecks.Decks.Find((NetCache.DeckHeader deckHeader) => deckHeader.ID == deckId);
			if (renamedDeck != null)
			{
				renamedDeck.Name = newName;
				renamedDeck.LastModified = DateTime.Now;
			}
			OfflineDataCache.RenameDeck(deckId, newName);
			deck.OnNameChangeComplete();
			Action<bool>[] array = m_renameValidatedListeners.ToArray();
			for (int i = 0; i < array.Length; i++)
			{
				array[i](!isCensored);
			}
		}
	}

	public static void Init()
	{
		if (s_instance == null)
		{
			s_instance = new CollectionManager();
			HearthstoneApplication.Get().WillReset += s_instance.WillReset;
			NetCache.Get().FavoriteBattlegroundsGuideSkinChanged += s_instance.OnFavoriteBattlegroundsGuideSkinChanged;
			s_instance.InitImpl();
		}
	}

	public static CollectionManager Get()
	{
		return s_instance;
	}

	public CollectibleDisplay GetCollectibleDisplay()
	{
		return m_collectibleDisplay;
	}

	public bool IsFullyLoaded()
	{
		return m_collectionLoaded;
	}

	public void RegisterCollectionNetHandlers()
	{
		Network network = Network.Get();
		network.RegisterNetHandler(BoughtSoldCard.PacketID.ID, OnCardSale);
		network.RegisterNetHandler(MassDisenchantResponse.PacketID.ID, OnMassDisenchantResponse);
		network.RegisterNetHandler(PVPDRSessionInfoResponse.PacketID.ID, OnPVPDRSessionInfoResponse);
	}

	public void RemoveCollectionNetHandlers()
	{
		Network network = Network.Get();
		network.RemoveNetHandler(BoughtSoldCard.PacketID.ID, OnCardSale);
		network.RemoveNetHandler(MassDisenchantResponse.PacketID.ID, OnMassDisenchantResponse);
		network.RemoveNetHandler(PVPDRSessionInfoResponse.PacketID.ID, OnPVPDRSessionInfoResponse);
	}

	public bool HasVisitedCollection()
	{
		return m_hasVisitedCollection;
	}

	public void SetHasVisitedCollection(bool enable)
	{
		m_hasVisitedCollection = enable;
	}

	public bool IsWaitingForBoxTransition()
	{
		return m_waitingForBoxTransition;
	}

	public void NotifyOfBoxTransitionStart()
	{
		Box.Get().AddTransitionFinishedListener(OnBoxTransitionFinished);
		m_waitingForBoxTransition = true;
	}

	public void OnBoxTransitionFinished(object userData)
	{
		Box.Get().RemoveTransitionFinishedListener(OnBoxTransitionFinished);
		m_waitingForBoxTransition = false;
	}

	public void SetCollectibleDisplay(CollectibleDisplay display)
	{
		m_collectibleDisplay = display;
	}

	public void AddCardReward(CardRewardData cardReward, bool markAsNew)
	{
		List<CardRewardData> cardRewards = new List<CardRewardData>();
		cardRewards.Add(cardReward);
		AddCardRewards(cardRewards, markAsNew);
	}

	public void AddCardRewards(List<CardRewardData> cardRewards, bool markAsNew)
	{
		List<string> cardIDs = new List<string>();
		List<TAG_PREMIUM> cardPremiums = new List<TAG_PREMIUM>();
		List<DateTime> insertDates = new List<DateTime>();
		List<int> counts = new List<int>();
		DateTime insertDate = DateTime.Now;
		foreach (CardRewardData cardReward in cardRewards)
		{
			cardIDs.Add(cardReward.CardID);
			cardPremiums.Add(cardReward.Premium);
			insertDates.Add(insertDate);
			counts.Add(cardReward.Count);
		}
		InsertNewCollectionCards(cardIDs, cardPremiums, insertDates, counts, !markAsNew);
		SendPendingDeckUpdates();
		AchieveManager.Get().ValidateAchievesNow();
		DelOnCardRewardsInserted[] array = m_cardRewardListeners.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			array[i](cardIDs, cardPremiums);
		}
	}

	public float CollectionLastModifiedTime()
	{
		return m_collectionLastModifiedTime;
	}

	public static int EntityDefSortComparison(EntityDef entityDef1, EntityDef entityDef2)
	{
		int num = (entityDef1.HasTag(GAME_TAG.DECK_LIST_SORT_ORDER) ? entityDef1.GetTag(GAME_TAG.DECK_LIST_SORT_ORDER) : int.MaxValue);
		int sortOrder2 = (entityDef2.HasTag(GAME_TAG.DECK_LIST_SORT_ORDER) ? entityDef2.GetTag(GAME_TAG.DECK_LIST_SORT_ORDER) : int.MaxValue);
		int sortOrderDiff = num - sortOrder2;
		if (sortOrderDiff != 0)
		{
			return sortOrderDiff;
		}
		int cost = entityDef1.GetCost();
		int cost2 = entityDef2.GetCost();
		int costDiff = cost - cost2;
		if (costDiff != 0)
		{
			return costDiff;
		}
		string name = entityDef1.GetName();
		string name2 = entityDef2.GetName();
		int stringDiff = string.Compare(name, name2, ignoreCase: true);
		if (stringDiff != 0)
		{
			return stringDiff;
		}
		int cardTypeSortOrder = GetCardTypeSortOrder(entityDef1);
		int order2 = GetCardTypeSortOrder(entityDef2);
		return cardTypeSortOrder - order2;
	}

	public static int GetCardTypeSortOrder(EntityDef entityDef)
	{
		return entityDef.GetCardType() switch
		{
			TAG_CARDTYPE.WEAPON => 1, 
			TAG_CARDTYPE.SPELL => 2, 
			TAG_CARDTYPE.MINION => 3, 
			_ => 0, 
		};
	}

	private bool IsSetRotatedWithCache(TAG_CARD_SET set, Map<TAG_CARD_SET, bool> cache)
	{
		if (!cache.TryGetValue(set, out var result))
		{
			result = (cache[set] = GameUtils.IsSetRotated(set));
		}
		return result;
	}

	public FindCardsResult FindCards(string searchString = null, List<CollectibleCardFilter.FilterMask> filterMasks = null, int? manaCost = null, TAG_CARD_SET[] theseCardSets = null, TAG_CLASS[] theseClassTypes = null, TAG_CARDTYPE[] theseCardTypes = null, TAG_ROLE[] theseRoleTypes = null, TAG_RARITY? rarity = null, TAG_RACE? race = null, bool? isHero = null, int? minOwned = null, bool? notSeen = null, bool? isCraftable = null, CollectibleCardFilterFunc[] priorityFilters = null, DeckRuleset deckRuleset = null, bool returnAfterFirstResult = false, HashSet<string> leagueBannedCardsSubset = null, List<int> specificCards = null, bool? filterCoreCounterpartCards = null, List<HashSet<string>> theseSubsets = null)
	{
		FindCardsResult results = new FindCardsResult();
		CollectibleCardFilter.FilterMask searchFilterMask = CollectibleCardFilter.FilterMask.PREMIUM_ALL;
		m_filterCardSet.Clear();
		m_filterCardClass.Clear();
		m_filterCardType.Clear();
		m_filterCardRole.Clear();
		m_filterIsSetRotatedCache.Clear();
		m_cachedCardSetValues.Clear();
		m_filterSubsets.Clear();
		List<CollectibleCardFilterFunc> filterFuncs = new List<CollectibleCardFilterFunc>();
		if (priorityFilters != null)
		{
			filterFuncs.AddRange(priorityFilters);
		}
		CollectibleCardFilterFunc missingFilter = delegate(CollectibleCard card)
		{
			if (card.IsHeroSkin)
			{
				return card.OwnedCount < 1;
			}
			if (!card.IsEverCraftable)
			{
				return false;
			}
			int ownedCardCountByFilterMask = GetOwnedCardCountByFilterMask(card.CardId, searchFilterMask);
			if (ownedCardCountByFilterMask >= card.DefaultMaxCopiesPerDeck)
			{
				return false;
			}
			List<CounterpartCardsDbfRecord> counterpartCards = GameUtils.GetCounterpartCards(card.CardDbId);
			if (counterpartCards != null)
			{
				foreach (CounterpartCardsDbfRecord current in counterpartCards)
				{
					CollectibleCard card2 = GetCard(GameUtils.TranslateDbIdToCardId(current.DeckEquivalentCardId), card.PremiumType);
					if (card2 != null && m_filterCardSet.Contains(card2.Set) && GetOwnedCardCountByFilterMask(card2.CardId, searchFilterMask) + ownedCardCountByFilterMask >= card.DefaultMaxCopiesPerDeck)
					{
						return false;
					}
				}
			}
			return true;
		};
		CollectibleCardFilterFunc extraFilter = delegate(CollectibleCard card)
		{
			if (card.IsHeroSkin)
			{
				return false;
			}
			if (!card.IsCraftable)
			{
				return false;
			}
			return (GetOwnedCardCountByFilterMask(card.CardId, searchFilterMask) > card.DefaultMaxCopiesPerDeck) ? true : false;
		};
		CollectibleCardFilterFunc favoriteFilter = (CollectibleCard card) => card.IsHeroSkin && IsFavoriteHero(card.CardId);
		if (filterMasks != null)
		{
			filterFuncs.Add(maskFilter);
		}
		bool hasSearchString = !string.IsNullOrEmpty(searchString);
		if (theseClassTypes != null && theseClassTypes.Length != 0)
		{
			filterFuncs.Add(classTypeFilter);
		}
		if (theseCardTypes != null && theseCardTypes.Length != 0)
		{
			foreach (TAG_CARDTYPE desiredCards in theseCardTypes)
			{
				m_filterCardType.Add(desiredCards);
			}
			filterFuncs.Add((CollectibleCard card) => m_filterCardType.Contains(card.CardType));
		}
		if (theseRoleTypes != null && theseRoleTypes.Length != 0)
		{
			filterFuncs.Add((CollectibleCard card) => theseRoleTypes.Contains(card.Role));
		}
		if (rarity.HasValue)
		{
			filterFuncs.Add((CollectibleCard card) => card.Rarity == rarity.Value);
		}
		if (race.HasValue)
		{
			filterFuncs.Add((CollectibleCard card) => card.Races.Contains(race.Value));
		}
		bool isHeroValue;
		if (isHero.HasValue)
		{
			isHeroValue = isHero.Value;
			filterFuncs.Add(heroFilter);
		}
		if (notSeen.HasValue)
		{
			if (notSeen.Value)
			{
				filterFuncs.Add((CollectibleCard card) => card.SeenCount < card.OwnedCount);
			}
			else
			{
				filterFuncs.Add((CollectibleCard card) => card.SeenCount == card.OwnedCount);
			}
		}
		if (isCraftable.HasValue)
		{
			filterFuncs.Add((CollectibleCard card) => card.IsCraftable == isCraftable.Value);
		}
		if (hasSearchString)
		{
			m_startsWithMatchNames.Clear();
			string lowerSearchString = searchString.ToLower();
			filterFuncs.Add(delegate(CollectibleCard card)
			{
				if (card.Set == TAG_CARD_SET.LETTUCE)
				{
					return false;
				}
				ReadOnlySpan<char> source = card.Name.AsSpan();
				Span<char> span = stackalloc char[source.Length];
				source.ToLower(span, CultureInfo.CurrentCulture);
				bool flag = false;
				ReadOnlySpan<char> stringSpan = span;
				Span<char> span2 = stackalloc char[1] { ' ' };
				ReadOnlySpanExtensions.SplitEnumerator enumerator2 = new ReadOnlySpanExtensions.SplitEnumerator(stringSpan, span2).GetEnumerator();
				while (enumerator2.MoveNext())
				{
					if (enumerator2.Current.StartsWith(lowerSearchString.AsSpan()))
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					SearchableString.SearchInternationalText(card.Name.ToLower(), lowerSearchString);
				}
				if (flag)
				{
					if (!m_startsWithMatchNames.ContainsKey(card.CardDbId))
					{
						m_startsWithMatchNames[card.CardDbId] = 0;
					}
					m_startsWithMatchNames[card.CardDbId] += card.OwnedCount;
				}
				return true;
			});
		}
		if (manaCost.HasValue)
		{
			int minManaCost = manaCost.Value;
			int maxManaCost = manaCost.Value;
			if (maxManaCost >= 7)
			{
				maxManaCost = int.MaxValue;
			}
			filterFuncs.Add(delegate(CollectibleCard card)
			{
				int num;
				if (card.ManaCost >= minManaCost)
				{
					num = ((card.ManaCost <= maxManaCost) ? 1 : 0);
					if (num != 0)
					{
						goto IL_0053;
					}
				}
				else
				{
					num = 0;
				}
				if (m_startsWithMatchNames.ContainsKey(card.CardDbId))
				{
					results.m_resultsWithoutManaFilterExist = true;
				}
				goto IL_0053;
				IL_0053:
				return (byte)num != 0;
			});
		}
		if (theseCardSets != null && theseCardSets.Length != 0)
		{
			foreach (TAG_CARD_SET desiredCards2 in theseCardSets)
			{
				m_filterCardSet.Add(desiredCards2);
			}
			filterFuncs.Add(standardSetFilter);
		}
		int minOwnedValue;
		if (minOwned.HasValue)
		{
			minOwnedValue = minOwned.Value;
			filterFuncs.Add(minOwnedFilter);
		}
		if (theseSubsets != null && theseSubsets.Count > 0)
		{
			foreach (HashSet<string> desiredSubset in theseSubsets)
			{
				m_filterSubsets.Add(desiredSubset);
			}
			filterFuncs.Add(twistFilter);
		}
		if (theseCardSets != null && theseCardSets.Length != 0)
		{
			filterFuncs.Add(wildSetFilter);
		}
		CollectionDeck deck;
		if (deckRuleset != null)
		{
			deck = Get().GetEditedDeck();
			filterFuncs.Add(deckRulesetFilter);
		}
		if (leagueBannedCardsSubset != null)
		{
			filterFuncs.Add((CollectibleCard card) => !leagueBannedCardsSubset.Contains(card.GetEntityDef().GetCardId()));
		}
		if (specificCards != null)
		{
			filterFuncs.Add((CollectibleCard card) => specificCards.Contains(card.CardDbId));
		}
		if (hasSearchString)
		{
			string[] lowercaseSearchTermsArray = searchString.ToLower().Split(' ');
			string missingString = GameStrings.Get("GLUE_COLLECTION_MANAGER_SEARCH_MISSING");
			string extraString = GameStrings.Get("GLUE_COLLECTION_MANAGER_SEARCH_EXTRA");
			string favoriteString = GameStrings.Get("GLUE_COLLECTION_MANAGER_SEARCH_FAVORITE");
			if (lowercaseSearchTermsArray.Contains(favoriteString) && CardBackManager.Get().MultipleFavoriteCardBacksEnabled())
			{
				filterFuncs.Add(favoriteFilter);
			}
			else if (lowercaseSearchTermsArray.Contains(missingString))
			{
				searchFilterMask = CollectibleCardFilter.FilterMask.PREMIUM_ALL | CollectibleCardFilter.FilterMask.UNOWNED;
				filterFuncs.Add(missingFilter);
			}
			else if (lowercaseSearchTermsArray.Contains(extraString))
			{
				searchFilterMask = CollectibleCardFilter.FilterMask.PREMIUM_ALL | CollectibleCardFilter.FilterMask.OWNED;
				filterFuncs.Add(extraFilter);
			}
			CollectibleCardFilter collectibleCardFilter = new CollectibleCardClassFilter();
			filterFuncs.AddRange(collectibleCardFilter.FiltersFromSearchString(searchString));
		}
		Predicate<CollectibleCard> testCard = delegate(CollectibleCard card)
		{
			int j = 0;
			for (int count = filterFuncs.Count; j < count; j++)
			{
				if (!filterFuncs[j](card))
				{
					return false;
				}
			}
			return true;
		};
		if (returnAfterFirstResult)
		{
			CollectibleCard firstResult = m_collectibleCards.Find(testCard);
			if (firstResult != null)
			{
				results.m_cards.Add(firstResult);
			}
		}
		else
		{
			results.m_cards = m_collectibleCards.FindAll(testCard);
		}
		if (filterCoreCounterpartCards.HasValue && filterCoreCounterpartCards == true)
		{
			FilterOutCardWithCoreOrWondersCounterparts(results.m_cards);
		}
		return results;
		bool classTypeFilter(CollectibleCard card)
		{
			card.GetEntityDef().GetClasses(m_cardClasses);
			int l = 0;
			for (int iMax2 = theseClassTypes.Length; l < iMax2; l++)
			{
				TAG_CLASS theseTagClass = theseClassTypes[l];
				for (int m = 0; m < m_cardClasses.Count; m++)
				{
					if (theseTagClass == m_cardClasses[m])
					{
						return true;
					}
				}
			}
			return false;
		}
		bool deckRulesetFilter(CollectibleCard card)
		{
			bool include2 = deckRuleset.Filter(card.GetEntityDef(), deck);
			if (!include2 && card.OwnedCount > 0 && deckRuleset.FilterFailsOnShowInvalidRule(card.GetEntityDef(), deck))
			{
				include2 = true;
			}
			return include2;
		}
		bool heroFilter(CollectibleCard card)
		{
			if (card.IsHeroSkin == isHeroValue)
			{
				return true;
			}
			return false;
		}
		bool maskFilter(CollectibleCard card)
		{
			CollectibleCardFilter.FilterMask cardMask = CollectibleCardFilter.FilterMaskFromPremiumType(card.PremiumType);
			cardMask = ((card.OwnedCount <= 0) ? (cardMask | CollectibleCardFilter.FilterMask.UNOWNED) : (cardMask | CollectibleCardFilter.FilterMask.OWNED));
			int k = 0;
			for (int iMax = filterMasks.Count; k < iMax; k++)
			{
				if ((filterMasks[k] & cardMask) == cardMask)
				{
					return true;
				}
			}
			return false;
		}
		bool minOwnedFilter(CollectibleCard card)
		{
			int ownedCount = card.OwnedCount;
			bool num3 = ownedCount >= minOwnedValue;
			if (!num3)
			{
				int cardDbId = card.CardDbId;
				if (m_startsWithMatchNames.ContainsKey(cardDbId))
				{
					m_startsWithMatchNames[cardDbId] -= ownedCount;
					if (m_startsWithMatchNames[cardDbId] < 1)
					{
						results.m_resultsUnownedExist = true;
					}
				}
			}
			return num3;
		}
		bool standardSetFilter(CollectibleCard card)
		{
			string cardId = card.CardId;
			TAG_CARD_SET cardSet = TAG_CARD_SET.INVALID;
			if (!m_cachedCardSetValues.TryGetValue(cardId, out cardSet))
			{
				cardSet = card.Set;
				m_cachedCardSetValues.Add(cardId, cardSet);
			}
			if (!IsSetRotatedWithCache(cardSet, m_filterIsSetRotatedCache))
			{
				bool num2 = m_filterCardSet.Contains(cardSet);
				if (!num2 && m_startsWithMatchNames.ContainsKey(card.CardDbId))
				{
					results.m_resultsWithoutSetFilterExist = true;
				}
				return num2;
			}
			return true;
		}
		bool twistFilter(CollectibleCard card)
		{
			bool include = false;
			foreach (HashSet<string> filterSubset in m_filterSubsets)
			{
				if (filterSubset.Contains(card.CardId))
				{
					include = true;
					break;
				}
			}
			if (!include && m_startsWithMatchNames.ContainsKey(card.CardDbId))
			{
				results.m_resultsInWildExist = true;
			}
			return include;
		}
		bool wildSetFilter(CollectibleCard card)
		{
			TAG_CARD_SET cardSet2 = m_cachedCardSetValues[card.CardId];
			if (IsSetRotatedWithCache(cardSet2, m_filterIsSetRotatedCache))
			{
				bool num4 = m_filterCardSet.Contains(cardSet2);
				if (!num4 && m_startsWithMatchNames.ContainsKey(card.CardDbId))
				{
					results.m_resultsInWildExist = true;
				}
				return num4;
			}
			return true;
		}
	}

	public FindCardsResult FindOrderedCards(string searchString = null, List<CollectibleCardFilter.FilterMask> filterMasks = null, int? manaCost = null, TAG_CARD_SET[] theseCardSets = null, TAG_CLASS[] theseClassTypes = null, TAG_CARDTYPE[] theseCardTypes = null, TAG_ROLE[] theseRoleTypes = null, TAG_RARITY? rarity = null, TAG_RACE? race = null, bool? isHero = null, int? minOwned = null, bool? notSeen = null, bool? isCraftable = null, CollectibleCardFilterFunc[] priorityFilters = null, DeckRuleset deckRuleset = null, bool returnAfterFirstResult = false, HashSet<string> leagueBannedCardsSubset = null, List<int> specificCards = null, bool? filterCounterpartCards = null, List<HashSet<string>> theseSubsets = null)
	{
		FindCardsResult findCardsResult = FindCards(searchString, filterMasks, manaCost, theseCardSets, theseClassTypes, theseCardTypes, theseRoleTypes, rarity, race, isHero, minOwned, notSeen, isCraftable, priorityFilters, deckRuleset, returnAfterFirstResult, leagueBannedCardsSubset, specificCards, filterCounterpartCards, theseSubsets);
		findCardsResult.m_cards.Sort(OrderedCardsSort);
		return findCardsResult;
	}

	public bool HasCoreOrWondersCounterpart(CollectibleCard card)
	{
		List<CounterpartCardsDbfRecord> counterpartCards = GameUtils.GetCounterpartCards(card.CardDbId);
		if (counterpartCards != null)
		{
			foreach (CounterpartCardsDbfRecord counterpartCard in counterpartCards)
			{
				CollectibleCard counterpartCardData = GetCard(GameUtils.TranslateDbIdToCardId(counterpartCard.DeckEquivalentCardId), card.PremiumType);
				if (counterpartCardData != null && (counterpartCardData.Set == TAG_CARD_SET.CORE || counterpartCardData.Set == TAG_CARD_SET.WONDERS))
				{
					return true;
				}
			}
		}
		return false;
	}

	public void FilterOutCardWithCoreOrWondersCounterparts(List<CollectibleCard> collectibleCards)
	{
		HashSet<CollectibleCardIndex> counterpartCardsToRemove = new HashSet<CollectibleCardIndex>(new CollectibleCardIndexComparer());
		foreach (CollectibleCard card in collectibleCards)
		{
			if (card.Set != TAG_CARD_SET.CORE && card.Set != TAG_CARD_SET.WONDERS)
			{
				continue;
			}
			List<CounterpartCardsDbfRecord> counterpartCards = GameUtils.GetCounterpartCards(card.CardDbId);
			if (counterpartCards == null)
			{
				continue;
			}
			foreach (CounterpartCardsDbfRecord counterpartCard in counterpartCards)
			{
				CollectibleCard counterpartCardData = GetCard(GameUtils.TranslateDbIdToCardId(counterpartCard.DeckEquivalentCardId), card.PremiumType);
				if (counterpartCardData == null)
				{
					continue;
				}
				string cardToRemove = null;
				if (card.Set == TAG_CARD_SET.CORE && card.OwnedCount == card.DefaultMaxCopiesPerDeck)
				{
					cardToRemove = counterpartCardData.CardId;
				}
				else
				{
					if ((card.OwnedCount == 1 && counterpartCardData.OwnedCount == 1) || ((card.Set == TAG_CARD_SET.CORE || card.Set == TAG_CARD_SET.WONDERS) && card.OwnedCount < counterpartCardData.OwnedCount) || counterpartCardData.Set == TAG_CARD_SET.CORE)
					{
						continue;
					}
					cardToRemove = ((card.OwnedCount >= counterpartCardData.OwnedCount) ? counterpartCardData.CardId : card.CardId);
				}
				if (cardToRemove != null)
				{
					counterpartCardsToRemove.Add(new CollectibleCardIndex(cardToRemove, card.PremiumType));
				}
			}
		}
		for (int i = collectibleCards.Count - 1; i > -1; i--)
		{
			CollectibleCard collectibleCard = collectibleCards[i];
			if (counterpartCardsToRemove.Contains(new CollectibleCardIndex(collectibleCard.CardId, collectibleCard.PremiumType)))
			{
				collectibleCards.RemoveAt(i);
			}
		}
	}

	public List<CollectibleCard> GetAllCards()
	{
		return m_collectibleCards;
	}

	public bool IsCardOwned(string cardId)
	{
		return GetTotalOwnedCount(cardId) > 0;
	}

	public void RegisterCollectionLoadedListener(DelOnCollectionLoaded listener)
	{
		if (!m_collectionLoadedListeners.Contains(listener))
		{
			m_collectionLoadedListeners.Add(listener);
		}
	}

	public bool RemoveCollectionLoadedListener(DelOnCollectionLoaded listener)
	{
		return m_collectionLoadedListeners.Remove(listener);
	}

	public void RegisterCollectionChangedListener(DelOnCollectionChanged listener)
	{
		if (!m_collectionChangedListeners.Contains(listener))
		{
			m_collectionChangedListeners.Add(listener);
		}
	}

	public bool RemoveCollectionChangedListener(DelOnCollectionChanged listener)
	{
		return m_collectionChangedListeners.Remove(listener);
	}

	public void RegisterDeckCreatedListener(DelOnDeckCreated listener)
	{
		if (!m_deckCreatedListeners.Contains(listener))
		{
			m_deckCreatedListeners.Add(listener);
		}
	}

	public bool RemoveDeckCreatedListener(DelOnDeckCreated listener)
	{
		return m_deckCreatedListeners.Remove(listener);
	}

	public void RegisterDeckDeletedListener(DelOnDeckDeleted listener)
	{
		if (!m_deckDeletedListeners.Contains(listener))
		{
			m_deckDeletedListeners.Add(listener);
		}
	}

	public bool RemoveDeckDeletedListener(DelOnDeckDeleted listener)
	{
		return m_deckDeletedListeners.Remove(listener);
	}

	public void RegisterDeckContentsListener(DelOnDeckContents listener)
	{
		if (!m_deckContentsListeners.Contains(listener))
		{
			m_deckContentsListeners.Add(listener);
		}
	}

	public bool RemoveDeckContentsListener(DelOnDeckContents listener)
	{
		return m_deckContentsListeners.Remove(listener);
	}

	public void RegisterNewCardSeenListener(DelOnNewCardSeen listener)
	{
		if (!m_newCardSeenListeners.Contains(listener))
		{
			m_newCardSeenListeners.Add(listener);
		}
	}

	public bool RemoveNewCardSeenListener(DelOnNewCardSeen listener)
	{
		return m_newCardSeenListeners.Remove(listener);
	}

	public void RegisterCardRewardsInsertedListener(DelOnCardRewardsInserted listener)
	{
		if (!m_cardRewardListeners.Contains(listener))
		{
			m_cardRewardListeners.Add(listener);
		}
	}

	public bool RemoveCardRewardsInsertedListener(DelOnCardRewardsInserted listener)
	{
		return m_cardRewardListeners.Remove(listener);
	}

	public void RegisterMassDisenchantListener(OnMassDisenchant listener)
	{
		if (!m_massDisenchantListeners.Contains(listener))
		{
			m_massDisenchantListeners.Add(listener);
		}
	}

	public void RemoveMassDisenchantListener(OnMassDisenchant listener)
	{
		m_massDisenchantListeners.Remove(listener);
	}

	public void RegisterEditedDeckChanged(OnEditedDeckChanged listener)
	{
		m_editedDeckChangedListeners.Add(listener);
	}

	public void RemoveEditedDeckChanged(OnEditedDeckChanged listener)
	{
		m_editedDeckChangedListeners.Remove(listener);
	}

	public bool RegisterFavoriteHeroChangedListener(FavoriteHeroChangedCallback callback)
	{
		return RegisterFavoriteHeroChangedListener(callback, null);
	}

	public bool RegisterFavoriteHeroChangedListener(FavoriteHeroChangedCallback callback, object userData)
	{
		FavoriteHeroChangedListener listener = new FavoriteHeroChangedListener();
		listener.SetCallback(callback);
		listener.SetUserData(userData);
		if (m_favoriteHeroChangedListeners.Contains(listener))
		{
			return false;
		}
		m_favoriteHeroChangedListeners.Add(listener);
		return true;
	}

	public bool RemoveFavoriteHeroChangedListener(FavoriteHeroChangedCallback callback)
	{
		return RemoveFavoriteHeroChangedListener(callback, null);
	}

	public bool RemoveFavoriteHeroChangedListener(FavoriteHeroChangedCallback callback, object userData)
	{
		FavoriteHeroChangedListener listener = new FavoriteHeroChangedListener();
		listener.SetCallback(callback);
		listener.SetUserData(userData);
		return m_favoriteHeroChangedListeners.Remove(listener);
	}

	public bool RegisterOnUIHeroOverrideCardRemovedListener(OnUIHeroOverrideCardRemovedCallback callback)
	{
		return RegisterOnUIHeroOverrideCardRemovedListener(callback, null);
	}

	public bool RegisterOnUIHeroOverrideCardRemovedListener(OnUIHeroOverrideCardRemovedCallback callback, object userData)
	{
		OnUIHeroOverrideCardRemovedListener listener = new OnUIHeroOverrideCardRemovedListener();
		listener.SetCallback(callback);
		listener.SetUserData(userData);
		if (m_onUIHeroOverrideCardRemovedListeners.Contains(listener))
		{
			return false;
		}
		m_onUIHeroOverrideCardRemovedListeners.Add(listener);
		return true;
	}

	public bool RemoveOnUIHeroOverrideCardRemovedListener(OnUIHeroOverrideCardRemovedCallback callback)
	{
		return RemoveOnUIHeroOverrideCardRemovedListener(callback, null);
	}

	public bool RemoveOnUIHeroOverrideCardRemovedListener(OnUIHeroOverrideCardRemovedCallback callback, object userData)
	{
		OnUIHeroOverrideCardRemovedListener listener = new OnUIHeroOverrideCardRemovedListener();
		listener.SetCallback(callback);
		listener.SetUserData(userData);
		return m_onUIHeroOverrideCardRemovedListeners.Remove(listener);
	}

	public void RegisterOnInitialCollectionReceivedListener(Action callback)
	{
		if (!m_initialCollectionReceivedListeners.Contains(callback))
		{
			m_initialCollectionReceivedListeners.Add(callback);
		}
	}

	public void RemoveOnInitialCollectionReceivedListener(Action callback)
	{
		if (m_initialCollectionReceivedListeners.Contains(callback))
		{
			m_initialCollectionReceivedListeners.Remove(callback);
		}
	}

	public void RegisterRenameFinishedListener(Action callback)
	{
		if (!m_renameFinishedListeners.Contains(callback))
		{
			m_renameFinishedListeners.Add(callback);
		}
	}

	public void RemoveRenameFinishedListener(Action callback)
	{
		if (m_renameFinishedListeners.Contains(callback))
		{
			m_renameFinishedListeners.Remove(callback);
		}
	}

	public void RegisterRenameValidatedListener(Action<bool> callback)
	{
		if (!m_renameValidatedListeners.Contains(callback))
		{
			m_renameValidatedListeners.Add(callback);
		}
	}

	public void RemoveRenameValidatedListener(Action<bool> callback)
	{
		if (m_renameValidatedListeners.Contains(callback))
		{
			m_renameValidatedListeners.Remove(callback);
		}
	}

	public bool OwnsAnyCollectible()
	{
		if (CardBackManager.Get().GetNumCardBacksOwned() > 0)
		{
			return true;
		}
		if (CosmeticCoinManager.Get().GetCoinsOwned().Count > 0)
		{
			return true;
		}
		if (GetOwnedCards().Count > 0)
		{
			return true;
		}
		return false;
	}

	public TAG_PREMIUM GetBestCardPremium(string cardID)
	{
		CollectibleCard card = null;
		if (m_collectibleCardIndex.TryGetValue(new CollectibleCardIndex(cardID, TAG_PREMIUM.DIAMOND), out card) && card.OwnedCount > 0)
		{
			return TAG_PREMIUM.DIAMOND;
		}
		if (m_collectibleCardIndex.TryGetValue(new CollectibleCardIndex(cardID, TAG_PREMIUM.SIGNATURE), out card) && card.OwnedCount > 0)
		{
			return TAG_PREMIUM.SIGNATURE;
		}
		if (m_collectibleCardIndex.TryGetValue(new CollectibleCardIndex(cardID, TAG_PREMIUM.GOLDEN), out card) && card.OwnedCount > 0)
		{
			return TAG_PREMIUM.GOLDEN;
		}
		return TAG_PREMIUM.NORMAL;
	}

	public CollectibleCard GetCard(string cardID, TAG_PREMIUM premium)
	{
		CollectibleCard card = null;
		m_collectibleCardIndex.TryGetValue(new CollectibleCardIndex(cardID, premium), out card);
		return card;
	}

	public List<CollectibleCard> GetOwnedHeroesForClass(TAG_CLASS heroClass)
	{
		int? minOwned = 1;
		bool? isHero = true;
		TAG_CLASS[] theseClassTypes = new TAG_CLASS[1] { heroClass };
		return FindCards(null, null, null, null, theseClassTypes, null, null, null, null, isHero, minOwned, null, null, null, null, returnAfterFirstResult: false, null, null, null).m_cards;
	}

	public int GetCountOfOwnedHeroesForClass(TAG_CLASS heroClass)
	{
		return GetOwnedHeroesForClass(heroClass).Count;
	}

	public int GetRandomHeroIdOwnedByPlayer(TAG_CLASS heroClass, bool shouldLimitToFavorites, int? heroIdToExclude = null)
	{
		if (shouldLimitToFavorites)
		{
			string favoriteHeroCardId = GetVanillaHero(heroClass);
			NetCache.CardDefinition favoriteHero = GetRandomFavoriteHero(heroClass, heroIdToExclude);
			if (favoriteHero != null)
			{
				favoriteHeroCardId = favoriteHero.Name;
			}
			return GameUtils.TranslateCardIdToDbId(favoriteHeroCardId);
		}
		List<CollectibleCard> ownedHeroes = GetOwnedHeroesForClass(heroClass);
		if (GetHeroPremium(heroClass) == TAG_PREMIUM.GOLDEN && ownedHeroes.Count > 1)
		{
			string vanillaHeroCardId = GetVanillaHero(heroClass);
			int vanillaHeroIndex = ownedHeroes.FindIndex((CollectibleCard hero) => hero.PremiumType == TAG_PREMIUM.NORMAL && hero.CardId == vanillaHeroCardId);
			if (vanillaHeroIndex > -1 && ownedHeroes.Exists((CollectibleCard hero) => TAG_PREMIUM.GOLDEN == hero.PremiumType && hero.CardId == vanillaHeroCardId))
			{
				ownedHeroes.RemoveAt(vanillaHeroIndex);
			}
		}
		if (heroIdToExclude.HasValue && ownedHeroes.Count > 1)
		{
			ownedHeroes.RemoveAll((CollectibleCard hero) => hero.CardDbId == heroIdToExclude);
		}
		if (ownedHeroes.Count == 0)
		{
			return 0;
		}
		int randomIndex = UnityEngine.Random.Range(0, ownedHeroes.Count);
		return ownedHeroes[randomIndex].CardDbId;
	}

	public List<(TAG_CLASS, NetCache.CardDefinition)> GetFavoriteHeroes()
	{
		NetCache.NetCacheFavoriteHeroes netCacheFavoriteHeroes = NetCache.Get().GetNetObject<NetCache.NetCacheFavoriteHeroes>();
		if (netCacheFavoriteHeroes == null)
		{
			return GetFavoriteHeroesFromOfflineData();
		}
		return netCacheFavoriteHeroes.FavoriteHeroes;
	}

	public List<NetCache.CardDefinition> GetFavoriteHeroesForClass(TAG_CLASS heroClass)
	{
		List<(TAG_CLASS, NetCache.CardDefinition)> favoriteHeroes = GetFavoriteHeroes();
		List<NetCache.CardDefinition> favoriteHeroesForClass = new List<NetCache.CardDefinition>();
		foreach (var favorite in favoriteHeroes)
		{
			if (favorite.Item1 == heroClass)
			{
				favoriteHeroesForClass.Add(favorite.Item2);
			}
		}
		return favoriteHeroesForClass;
	}

	public NetCache.CardDefinition GetRandomFavoriteHero(TAG_CLASS heroClass, int? heroIdToExclude = null)
	{
		List<NetCache.CardDefinition> favoriteHeroesForClass = GetFavoriteHeroesForClass(heroClass);
		if (favoriteHeroesForClass.Count() == 0)
		{
			return null;
		}
		if (heroIdToExclude.HasValue && favoriteHeroesForClass.Count > 1)
		{
			string heroCardIdToExclude = GameUtils.TranslateDbIdToCardId(heroIdToExclude.Value);
			favoriteHeroesForClass.RemoveAll((NetCache.CardDefinition hero) => hero.Name == heroCardIdToExclude);
		}
		int randomIndex = UnityEngine.Random.Range(0, favoriteHeroesForClass.Count);
		return favoriteHeroesForClass[randomIndex];
	}

	public bool IsFavoriteHero(string heroId)
	{
		NetCache.NetCacheFavoriteHeroes netCacheFavoriteHeroes = NetCache.Get().GetNetObject<NetCache.NetCacheFavoriteHeroes>();
		if (netCacheFavoriteHeroes == null)
		{
			Log.CollectionManager.PrintError("IsFavoriteHero: Unable to get net cache for favorite heroes");
			return false;
		}
		return netCacheFavoriteHeroes.FavoriteHeroes.Any(((TAG_CLASS, NetCache.CardDefinition) obj) => obj.Item2.Name == heroId);
	}

	public NetCache.CardDefinition GetFavoriteHero(string heroId)
	{
		NetCache.NetCacheFavoriteHeroes netCacheFavoriteHeroes = NetCache.Get().GetNetObject<NetCache.NetCacheFavoriteHeroes>();
		if (netCacheFavoriteHeroes == null)
		{
			Log.CollectionManager.PrintError("GetFavoriteHero: Unable to get net cache for favorite heroes");
			return null;
		}
		return netCacheFavoriteHeroes.FavoriteHeroes.Find(((TAG_CLASS, NetCache.CardDefinition) obj) => obj.Item2.Name == heroId).Item2;
	}

	private List<(TAG_CLASS, NetCache.CardDefinition)> GetFavoriteHeroesFromOfflineData()
	{
		List<FavoriteHero> favoriteHeroesFromCache = OfflineDataCache.GetFavoriteHeroesFromCache();
		List<(TAG_CLASS, NetCache.CardDefinition)> heroesToReturn = new List<(TAG_CLASS, NetCache.CardDefinition)>();
		foreach (FavoriteHero favoriteHero in favoriteHeroesFromCache)
		{
			heroesToReturn.Add(((TAG_CLASS)favoriteHero.ClassId, new NetCache.CardDefinition
			{
				Name = GameUtils.TranslateDbIdToCardId(favoriteHero.Hero.Asset),
				Premium = (TAG_PREMIUM)favoriteHero.Hero.Premium
			}));
		}
		return heroesToReturn;
	}

	public int GetCoreCardsIOwn(TAG_CLASS cardClass)
	{
		return NetCache.Get().GetNetObject<NetCache.NetCacheCollection>()?.CoreCardsUnlockedPerClass[cardClass].Count ?? 0;
	}

	public List<CollectibleCard> GetOwnedCards()
	{
		int? minOwned = 1;
		return FindCards(null, null, null, null, null, null, null, null, null, null, minOwned, null, null, null, null, returnAfterFirstResult: false, null, null, null).m_cards;
	}

	public void GetOwnedCardCount(string cardId, out int normal, out int golden, out int signature, out int diamond)
	{
		normal = 0;
		golden = 0;
		signature = 0;
		diamond = 0;
		if (m_collectibleCardIndex.TryGetValue(new CollectibleCardIndex(cardId, TAG_PREMIUM.NORMAL), out var card))
		{
			normal += card.OwnedCount;
		}
		if (m_collectibleCardIndex.TryGetValue(new CollectibleCardIndex(cardId, TAG_PREMIUM.GOLDEN), out card))
		{
			golden += card.OwnedCount;
		}
		if (m_collectibleCardIndex.TryGetValue(new CollectibleCardIndex(cardId, TAG_PREMIUM.SIGNATURE), out card))
		{
			signature += card.OwnedCount;
		}
		if (m_collectibleCardIndex.TryGetValue(new CollectibleCardIndex(cardId, TAG_PREMIUM.DIAMOND), out card))
		{
			diamond += card.OwnedCount;
		}
	}

	public int GetOwnedCardCountByFilterMask(string cardId, CollectibleCardFilter.FilterMask filterMask)
	{
		int ownedCount = 0;
		CollectibleCard card = null;
		if ((filterMask & CollectibleCardFilter.FilterMask.PREMIUM_NORMAL) != 0 && m_collectibleCardIndex.TryGetValue(new CollectibleCardIndex(cardId, TAG_PREMIUM.NORMAL), out card))
		{
			ownedCount += card.OwnedCount;
		}
		if ((filterMask & CollectibleCardFilter.FilterMask.PREMIUM_GOLDEN) != 0 && m_collectibleCardIndex.TryGetValue(new CollectibleCardIndex(cardId, TAG_PREMIUM.GOLDEN), out card))
		{
			ownedCount += card.OwnedCount;
		}
		if ((filterMask & CollectibleCardFilter.FilterMask.PREMIUM_SIGNATURE) != 0 && m_collectibleCardIndex.TryGetValue(new CollectibleCardIndex(cardId, TAG_PREMIUM.SIGNATURE), out card))
		{
			ownedCount += card.OwnedCount;
		}
		if ((filterMask & CollectibleCardFilter.FilterMask.PREMIUM_DIAMOND) != 0 && m_collectibleCardIndex.TryGetValue(new CollectibleCardIndex(cardId, TAG_PREMIUM.DIAMOND), out card))
		{
			ownedCount += card.OwnedCount;
		}
		return ownedCount;
	}

	public List<TAG_CARD_SET> GetDisplayableCardSets()
	{
		return m_displayableCardSets;
	}

	public bool IsCardInCollection(string cardID, TAG_PREMIUM premium)
	{
		if (m_collectibleCardIndex.TryGetValue(new CollectibleCardIndex(cardID, premium), out var card))
		{
			return card.OwnedCount > 0;
		}
		return false;
	}

	public int GetNumCopiesInCollection(string cardID, TAG_PREMIUM premium)
	{
		if (m_collectibleCardIndex.TryGetValue(new CollectibleCardIndex(cardID, premium), out var card))
		{
			return card.OwnedCount;
		}
		return 0;
	}

	public int GetTotalNumCopiesInCollection(string cardID)
	{
		int total = 0;
		CollectibleCardIndex cardIndex = default(CollectibleCardIndex);
		cardIndex.CardId = cardID;
		cardIndex.Premium = TAG_PREMIUM.NORMAL;
		if (m_collectibleCardIndex.TryGetValue(cardIndex, out var normalCard))
		{
			total += normalCard.OwnedCount;
		}
		cardIndex.Premium = TAG_PREMIUM.GOLDEN;
		if (m_collectibleCardIndex.TryGetValue(cardIndex, out var goldenCard))
		{
			total += goldenCard.OwnedCount;
		}
		cardIndex.Premium = TAG_PREMIUM.DIAMOND;
		if (m_collectibleCardIndex.TryGetValue(cardIndex, out var diamondCard))
		{
			total += diamondCard.OwnedCount;
		}
		cardIndex.Premium = TAG_PREMIUM.SIGNATURE;
		if (m_collectibleCardIndex.TryGetValue(cardIndex, out var signatureCard))
		{
			total += signatureCard.OwnedCount;
		}
		return total;
	}

	public void GetMassDisenchantCards(List<CollectibleCard> collectibleCards)
	{
		collectibleCards.Clear();
		foreach (CollectibleCard ownedCard in GetOwnedCards())
		{
			if (ownedCard.DisenchantCount > 0 && ownedCard.PremiumType != TAG_PREMIUM.SIGNATURE)
			{
				collectibleCards.Add(ownedCard);
			}
		}
	}

	public void GetMassDisenchantCardsAndCount(List<CollectibleCard> collectibleCards, out int disenchantCount)
	{
		collectibleCards.Clear();
		disenchantCount = 0;
		foreach (CollectibleCard ownedCard in GetOwnedCards())
		{
			int count = ownedCard.DisenchantCount;
			if (count > 0 && ownedCard.PremiumType != TAG_PREMIUM.SIGNATURE)
			{
				collectibleCards.Add(ownedCard);
				disenchantCount += count;
			}
		}
	}

	public int GetCardsToMassDisenchantCount()
	{
		int deCount = 0;
		foreach (CollectibleCard ownedCard in GetOwnedCards())
		{
			if (ownedCard.PremiumType != TAG_PREMIUM.SIGNATURE)
			{
				deCount += ownedCard.DisenchantCount;
			}
		}
		return deCount;
	}

	public void MarkAllInstancesAsSeen(string cardID, TAG_PREMIUM premium)
	{
		NetCache.NetCacheCollection netCacheCards = NetCache.Get().GetNetObject<NetCache.NetCacheCollection>();
		int dbId = GameUtils.TranslateCardIdToDbId(cardID);
		if (dbId == 0)
		{
			return;
		}
		CollectibleCard card = GetCard(cardID, premium);
		if (card == null || card.SeenCount == card.OwnedCount)
		{
			return;
		}
		Network.Get().AckCardSeenBefore(dbId, premium);
		card.SeenCount = card.OwnedCount;
		NetCache.CardStack netCacheStack = netCacheCards.Stacks.Find((NetCache.CardStack obj) => obj.Def.Name == card.CardId && obj.Def.Premium == card.PremiumType);
		if (netCacheStack != null)
		{
			netCacheStack.NumSeen = netCacheStack.Count;
		}
		foreach (DelOnNewCardSeen newCardSeenListener in m_newCardSeenListeners)
		{
			newCardSeenListener(cardID, premium);
		}
	}

	public void OnCardsModified(List<CardModification> cardChanges)
	{
		foreach (CardModification cardChange in cardChanges)
		{
			switch (cardChange.modificationType)
			{
			case CardModification.ModificationType.Add:
				InsertNewCollectionCard(cardChange.cardID, cardChange.premium, DateTime.Now, cardChange.count, cardChange.seenBefore);
				break;
			case CardModification.ModificationType.Remove:
				RemoveCollectionCard(cardChange.cardID, cardChange.premium, cardChange.count);
				break;
			}
		}
		SendPendingDeckUpdates();
		OnCollectionChanged();
		AchieveManager.Get().ValidateAchievesNow();
	}

	public void SendPendingDeckUpdates()
	{
		List<CollectionDeck> decks = GetDecks(DeckType.NORMAL_DECK);
		if (decks == null)
		{
			return;
		}
		foreach (CollectionDeck item in decks)
		{
			item.SendChangesIfPending();
		}
	}

	public void OnUIHeroOverrideCardRemoved()
	{
		if (m_onUIHeroOverrideCardRemovedListeners.Count > 0)
		{
			OnUIHeroOverrideCardRemovedListener[] array = m_onUIHeroOverrideCardRemovedListeners.ToArray();
			for (int i = 0; i < array.Length; i++)
			{
				array[i].Fire();
			}
		}
	}

	public PreconDeck GetPreconDeck(TAG_CLASS heroClass)
	{
		if (!m_preconDecks.ContainsKey(heroClass))
		{
			Log.All.PrintWarning($"CollectionManager.GetPreconDeck(): Could not retrieve precon deck for class {heroClass}");
			return null;
		}
		return m_preconDecks[heroClass];
	}

	public SortedDictionary<long, CollectionDeck> GetDecks()
	{
		SortedDictionary<long, CollectionDeck> decks = new SortedDictionary<long, CollectionDeck>();
		foreach (KeyValuePair<long, CollectionDeck> kv in m_decks)
		{
			CollectionDeck deck = kv.Value;
			if (deck != null && (!deck.IsBrawlDeck || TavernBrawlManager.Get().IsSeasonActive(deck.Type, deck.SeasonId, deck.BrawlLibraryItemId)))
			{
				decks.Add(kv.Key, kv.Value);
			}
		}
		return decks;
	}

	public List<CollectionDeck> GetDecks(DeckType deckType)
	{
		if (!NetCache.Get().IsNetObjectAvailable<NetCache.NetCacheDecks>())
		{
			Debug.LogWarning("Attempting to get decks from CollectionManager, even though NetCacheDecks is not ready (meaning it's waiting for the decks to be updated)!");
		}
		List<CollectionDeck> deckList = new List<CollectionDeck>();
		foreach (CollectionDeck deck in m_decks.Values)
		{
			if (deck.Type == deckType && (!deck.IsBrawlDeck || TavernBrawlManager.Get().IsSeasonActive(deck.Type, deck.SeasonId, deck.BrawlLibraryItemId)))
			{
				deckList.Add(deck);
			}
		}
		deckList.Sort(new DeckSort());
		return deckList;
	}

	public List<long> LoadDeckFromDBF(int deckID, out string deckName, out string deckDescription)
	{
		deckName = string.Empty;
		deckDescription = string.Empty;
		DeckDbfRecord deckRecord = GameDbf.Deck.GetRecord(deckID);
		if (deckRecord == null)
		{
			Debug.LogError($"Unable to find deck with ID {deckID}");
			return null;
		}
		if (deckRecord.Name == null)
		{
			Debug.LogErrorFormat("Deck with ID {0} has no name defined.", deckID);
		}
		else
		{
			deckName = deckRecord.Name.GetString();
		}
		if (deckRecord.Description != null)
		{
			deckDescription = deckRecord.Description.GetString();
		}
		List<long> cards = new List<long>();
		DeckCardDbfRecord currentDeckCard = GameDbf.DeckCard.GetRecord(deckRecord.TopCardId);
		while (currentDeckCard != null)
		{
			int cardDbID = currentDeckCard.CardId;
			cards.Add(cardDbID);
			int nextCardId = currentDeckCard.NextCard;
			currentDeckCard = ((nextCardId != 0) ? GameDbf.DeckCard.GetRecord(nextCardId) : null);
		}
		return cards;
	}

	public CollectionDeck GetDeck(long id)
	{
		if (m_decks.TryGetValue(id, out var deck))
		{
			if (deck != null && deck.IsBrawlDeck && !TavernBrawlManager.Get().IsSeasonActive(deck.Type, deck.SeasonId, deck.BrawlLibraryItemId))
			{
				return null;
			}
			return deck;
		}
		return null;
	}

	public bool AreAllDeckContentsReady()
	{
		if (!FixedRewardsMgr.Get().IsStartupFinished())
		{
			return false;
		}
		if (m_decks.FirstOrDefault((KeyValuePair<long, CollectionDeck> kv) => !kv.Value.NetworkContentsLoaded() && !kv.Value.IsBrawlDeck && !kv.Value.IsDuelsDeck).Value != null)
		{
			return false;
		}
		return true;
	}

	public bool ShouldAccountSeeStandardWild()
	{
		if (!RankMgr.Get().WildCardsAllowedInCurrentLeague())
		{
			return false;
		}
		if (AccountHasUnlockedWild())
		{
			return true;
		}
		return false;
	}

	public bool AccountHasUnlockedWild()
	{
		long unlockWildFlag = 0L;
		if (!GameSaveDataManager.Get().GetSubkeyValue(GameSaveKeyId.PLAYER_FLAGS, GameSaveKeySubkeyId.PLAYER_FLAGS_UNLOCKED_WILD, out unlockWildFlag))
		{
			return false;
		}
		return unlockWildFlag != 0;
	}

	public bool ShouldSeeTwistModeNotification()
	{
		if (!TwistDetailsDisplayManager.TwistSeasonInfoModel.IsTwistSeasonEnabled)
		{
			return false;
		}
		GameSaveDataManager.Get().GetSubkeyValue(GameSaveKeyId.FTUE, GameSaveKeySubkeyId.HAS_SEEN_TWIST_MODE_NOTIFICATION, out long hasSeenTwistModeNotification);
		if (hasSeenTwistModeNotification < 1)
		{
			return ShouldAccountSeeStandardWild();
		}
		return false;
	}

	public bool AccountHasRotatedBoosters(DateTime utcTimestamp)
	{
		NetCache.NetCacheBoosters boosters = NetCache.Get().GetNetObject<NetCache.NetCacheBoosters>();
		if (boosters != null)
		{
			foreach (NetCache.BoosterStack boosterStack in boosters.BoosterStacks)
			{
				if (GameUtils.IsBoosterRotated((BoosterDbId)boosterStack.Id, utcTimestamp))
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool AccountHasWildCards()
	{
		if (GetNumberOfWildDecks() > 0)
		{
			return true;
		}
		if (m_lastSearchForWildCardsTime > m_collectionLastModifiedTime)
		{
			return m_accountHasWildCards;
		}
		m_accountHasWildCards = m_collectibleCards.Any((CollectibleCard c) => c.OwnedCount > 0 && GameUtils.IsCardRotated(c.GetEntityDef()));
		m_lastSearchForWildCardsTime = Time.realtimeSinceStartup;
		return m_accountHasWildCards;
	}

	public int GetNumberOfWildDecks()
	{
		return m_decks.Values.Count((CollectionDeck deck) => deck.FormatType == FormatType.FT_WILD);
	}

	public int GetNumberOfStandardDecks()
	{
		return m_decks.Values.Count((CollectionDeck deck) => deck.FormatType == FormatType.FT_STANDARD);
	}

	public int GetNumberOfTwistDecks()
	{
		return m_decks.Values.Count((CollectionDeck deck) => deck.FormatType == FormatType.FT_TWIST);
	}

	public bool AccountHasValidDeck(FormatType formatType)
	{
		foreach (CollectionDeck deck in Get().GetDecks(DeckType.NORMAL_DECK))
		{
			if (deck.IsValidForRuleset && deck.IsValidForFormat(formatType))
			{
				return true;
			}
		}
		return false;
	}

	public CollectionDeck GetEditedDeck()
	{
		CollectionDeck deck = m_EditedDeck;
		if (deck != null && deck.IsBrawlDeck)
		{
			TavernBrawlManager tbm = TavernBrawlManager.Get();
			if (tbm != null)
			{
				TavernBrawlMission tbMission = (tbm.IsCurrentBrawlTypeActive ? tbm.CurrentMission() : null);
				if (tbMission == null || deck.SeasonId != tbMission.seasonId)
				{
					return null;
				}
			}
		}
		return deck;
	}

	public int GetDeckSize()
	{
		if (m_deckRuleset == null)
		{
			return 30;
		}
		return m_deckRuleset.GetDeckSize(GetEditedDeck());
	}

	public int GetDeckSizeWhileEditing(EntityDef cardBeingAdded = null)
	{
		if (m_deckRuleset == null)
		{
			return 30;
		}
		return m_deckRuleset.GetDeckSizeWhileEditing(GetEditedDeck(), cardBeingAdded);
	}

	public List<TemplateDeck> GetTemplateDecks(FormatType formatType, TAG_CLASS classType)
	{
		if (m_templateDeckMap.Values.Count == 0)
		{
			LoadTemplateDecks();
		}
		List<TemplateDeck> tplDecks = null;
		m_templateDecks.TryGetValue(classType, out tplDecks);
		if (formatType == FormatType.FT_WILD)
		{
			return tplDecks.Where((TemplateDeck x) => x.m_formatType == FormatType.FT_STANDARD || x.m_formatType == FormatType.FT_WILD).ToList();
		}
		return tplDecks.Where((TemplateDeck x) => x.m_formatType == formatType).ToList();
	}

	public List<TemplateDeck> GetNonStarterTemplateDecks(FormatType formatType, TAG_CLASS classType)
	{
		return GetTemplateDecks(formatType, classType)?.Where((TemplateDeck x) => !x.m_isStarterDeck).ToList();
	}

	public TemplateDeck GetTemplateDeck(int id)
	{
		if (m_templateDeckMap.Values.Count == 0)
		{
			LoadTemplateDecks();
		}
		m_templateDeckMap.TryGetValue(id, out var templateDeck);
		return templateDeck;
	}

	public bool IsInEditMode()
	{
		return m_editMode;
	}

	public bool IsEditingDeathKnightDeck()
	{
		return GetEditedDeck()?.ShouldShowDeathKnightRunes() ?? false;
	}

	public void StartEditingDeck(CollectionDeck deck, object callbackData = null)
	{
		if (deck == null)
		{
			return;
		}
		m_editMode = true;
		FriendChallengeMgr.Get().UpdateMyAvailability();
		CollectionManagerDisplay cmd = Get().GetCollectibleDisplay() as CollectionManagerDisplay;
		if (cmd != null)
		{
			cmd.SetHeroSkinClass(null);
			ActiveFilterButton filterButton = cmd.GetFilterButton();
			if (filterButton != null)
			{
				filterButton.UpdateFilterView();
			}
			cmd.HideAllCosmeticTips();
		}
		DeckRuleset deckRuleset;
		if (SceneMgr.Get().IsInTavernBrawlMode())
		{
			deckRuleset = TavernBrawlManager.Get().GetCurrentDeckRuleset();
		}
		else
		{
			deckRuleset = DeckRuleset.GetRuleset(deck.FormatType);
			PresenceMgr.Get().SetStatus(Global.PresenceStatus.DECKEDITOR);
		}
		SetDeckRuleset(deckRuleset);
		SetEditedDeck(deck, callbackData);
	}

	public void DoneEditing()
	{
		bool editMode = m_editMode;
		m_editMode = false;
		FriendChallengeMgr.Get().UpdateMyAvailability();
		if (editMode && SceneMgr.Get() != null && !SceneMgr.Get().IsInTavernBrawlMode())
		{
			PresenceMgr.Get().SetPrevStatus();
		}
		SetDeckRuleset(null);
		CollectionManagerDisplay cmd = Get().GetCollectibleDisplay() as CollectionManagerDisplay;
		if (editMode && cmd != null)
		{
			cmd.EnableTutorialsByViewMode(cmd.GetViewMode());
		}
		if (SceneMgr.Get().IsInLettuceMode())
		{
			ClearEditingTeam();
		}
		else
		{
			ClearEditedDeck();
		}
	}

	public DeckRuleset GetDeckRuleset()
	{
		return m_deckRuleset;
	}

	public FormatType GetThemeShowing(CollectionDeck deck = null)
	{
		if (CollectionUtils.IsHeroSkinDisplayMode(GetCollectibleDisplay().GetViewMode()))
		{
			return deck?.FormatType ?? FormatType.FT_STANDARD;
		}
		if (CollectionManagerDisplay.IsSpecialOneDeckMode())
		{
			return FormatType.FT_STANDARD;
		}
		if (deck == null)
		{
			deck = GetEditedDeck();
		}
		if (deck != null && deck.Type != DeckType.CLIENT_ONLY_DECK)
		{
			return deck.FormatType;
		}
		CollectionPageManager pageManager = ((m_collectibleDisplay != null) ? (m_collectibleDisplay.GetPageManager() as CollectionPageManager) : null);
		if (m_collectibleDisplay != null && pageManager != null && m_collectibleDisplay.SetFilterTrayInitialized())
		{
			if (pageManager.CardSetFilterIsTwist())
			{
				return FormatType.FT_TWIST;
			}
			if (pageManager.CardSetFilterIncludesWild())
			{
				return FormatType.FT_WILD;
			}
			if (pageManager.CardSetFilterIsClassic())
			{
				return FormatType.FT_CLASSIC;
			}
		}
		return FormatType.FT_STANDARD;
	}

	public void SetDeckRuleset(DeckRuleset deckRuleset)
	{
		m_deckRuleset = deckRuleset;
		CollectionPageManager pageManager = ((m_collectibleDisplay != null) ? (m_collectibleDisplay.GetPageManager() as CollectionPageManager) : null);
		if (pageManager != null)
		{
			pageManager.SetDeckRuleset(deckRuleset);
		}
	}

	public void SetEditedDeck(CollectionDeck deck, object callbackData = null)
	{
		CollectionDeck prevTaggedDeck = GetEditedDeck();
		if (deck != prevTaggedDeck)
		{
			m_EditedDeck = deck;
			OnEditedDeckChanged[] array = m_editedDeckChangedListeners.ToArray();
			for (int i = 0; i < array.Length; i++)
			{
				array[i](deck, prevTaggedDeck, callbackData);
			}
		}
	}

	public void ClearEditedDeck()
	{
		SetEditedDeck(null);
	}

	public void SendCreateDeck(DeckType deckType, string name, string heroCardID, DeckSourceType deckSourceType = DeckSourceType.DECK_SOURCE_TYPE_NORMAL, string pastedDeckHashString = null)
	{
		int dbId = GameUtils.TranslateCardIdToDbId(heroCardID);
		if (dbId == 0)
		{
			Debug.LogWarning($"CollectionManager.SendCreateDeck(): Unknown hero cardID {heroCardID}");
			return;
		}
		FormatType formatType = Options.GetFormatType();
		int brawlLibraryItemId = 0;
		if (SceneMgr.Get().IsInTavernBrawlMode())
		{
			formatType = TavernBrawlManager.Get().CurrentMission().formatType;
		}
		if (deckType == DeckType.PVPDR_DECK)
		{
			formatType = FormatType.FT_WILD;
		}
		if (formatType == FormatType.FT_UNKNOWN)
		{
			Debug.LogWarning($"CollectionManager.SendCreateDeck(): Bad format type {formatType.ToString()}");
			return;
		}
		if ((uint)(deckType - 6) <= 1u)
		{
			brawlLibraryItemId = TavernBrawlManager.Get().CurrentMission().SelectedBrawlLibraryItemId;
		}
		if (m_pendingDeckCreate != null)
		{
			Log.Offline.PrintWarning("SendCreateDeck - Attempting to create a deck while another is still pending.");
		}
		m_pendingDeckCreate = new PendingDeckCreateData
		{
			m_deckType = deckType,
			m_name = name,
			m_heroDbId = dbId,
			m_formatType = formatType,
			m_sourceType = deckSourceType,
			m_pastedDeckHash = pastedDeckHashString
		};
		if (Network.IsLoggedIn())
		{
			Network.Get().CreateDeck(deckType, name, dbId, formatType, -100L, deckSourceType, out var requestId, pastedDeckHashString, brawlLibraryItemId);
			if (requestId.HasValue)
			{
				m_inTransitDeckCreateRequests.Add(requestId.Value);
			}
		}
		else
		{
			CreateDeckOffline(m_pendingDeckCreate);
		}
	}

	private void CreateDeckOffline(PendingDeckCreateData data)
	{
		DeckInfo deckInfo = OfflineDataCache.CreateDeck(data.m_deckType, data.m_name, data.m_heroDbId, data.m_formatType, -100L, data.m_sourceType, data.m_pastedDeckHash);
		if (deckInfo == null)
		{
			AlertPopup.PopupInfo info = new AlertPopup.PopupInfo
			{
				m_headerText = GameStrings.Get("GLUE_OFFLINE_FEATURE_DISABLED_HEADER"),
				m_text = GameStrings.Get("GLUE_OFFLINE_DECK_ERROR_BODY"),
				m_responseDisplay = AlertPopup.ResponseDisplay.OK,
				m_showAlertIcon = true
			};
			DialogManager.Get().ShowPopup(info);
			CollectionManagerDisplay cmd = m_collectibleDisplay as CollectionManagerDisplay;
			if (cmd != null)
			{
				cmd.CancelSelectNewDeckHeroMode();
			}
			if (CollectionDeckTray.Get() != null)
			{
				CollectionDeckTray.Get().m_doneButton.SetEnabled(enabled: true);
			}
		}
		else
		{
			NetCache.DeckHeader deckHeader = Network.GetDeckHeaderFromDeckInfo(deckInfo);
			Processor.ScheduleCallback(0.5f, realTime: false, delegate
			{
				OnDeckCreated(deckHeader, null);
			});
		}
	}

	public void HandleDisconnect()
	{
		if (m_pendingDeckCreate != null)
		{
			CreateDeckOffline(m_pendingDeckCreate);
			m_pendingDeckCreate = null;
		}
		if (m_pendingDeckDeleteList != null)
		{
			PendingDeckDeleteData[] array = m_pendingDeckDeleteList.ToArray();
			foreach (PendingDeckDeleteData deckDeleteData in array)
			{
				OnDeckDeletedWhileOffline(deckDeleteData.m_deckId);
			}
			m_pendingDeckDeleteList = null;
		}
		if (m_pendingDeckEditList != null)
		{
			foreach (PendingDeckEditData deckEditData in m_pendingDeckEditList)
			{
				GetDeck(deckEditData.m_deckId)?.OnContentChangesComplete();
			}
			m_pendingDeckEditList = null;
		}
		if (m_pendingDeckRenameList == null)
		{
			return;
		}
		foreach (PendingDeckRenameData deckRenameData in m_pendingDeckRenameList)
		{
			CollectionDeck deck = GetDeck(deckRenameData.m_deckId);
			if (deck != null)
			{
				OfflineDataCache.RenameDeck(deckRenameData.m_deckId, deckRenameData.m_name);
				deck.OnNameChangeComplete();
			}
		}
		m_pendingDeckRenameList = null;
	}

	public bool RequestDeckContentsForDecksWithoutContentsLoaded(DelOnAllDeckContents callback = null)
	{
		float now = Time.realtimeSinceStartup;
		IEnumerable<KeyValuePair<long, CollectionDeck>> decks = m_decks.Where((KeyValuePair<long, CollectionDeck> kv) => !kv.Value.NetworkContentsLoaded());
		decks = decks.Where((KeyValuePair<long, CollectionDeck> kv) => !kv.Value.IsBrawlDeck || TavernBrawlManager.Get().IsTavernBrawlActiveByDeckType(kv.Value.Type));
		if (!decks.Any())
		{
			callback?.Invoke();
			return false;
		}
		if (callback != null && !m_allDeckContentsListeners.Contains(callback))
		{
			m_allDeckContentsListeners.Add(callback);
		}
		if (m_pendingRequestDeckContents != null)
		{
			decks = decks.Where((KeyValuePair<long, CollectionDeck> kv) => !m_pendingRequestDeckContents.ContainsKey(kv.Value.ID) || now - m_pendingRequestDeckContents[kv.Value.ID] >= 10f);
		}
		IEnumerable<long> deckIds = decks.Select((KeyValuePair<long, CollectionDeck> kv) => kv.Value.ID);
		if (deckIds.Any())
		{
			long[] deckIdArray = deckIds.ToArray();
			if (m_pendingRequestDeckContents == null)
			{
				m_pendingRequestDeckContents = new Map<long, float>();
			}
			for (int i = 0; i < deckIdArray.Length; i++)
			{
				m_pendingRequestDeckContents[deckIdArray[i]] = now;
			}
			Network.Get().RequestDeckContents(deckIdArray);
			return true;
		}
		return true;
	}

	public void RequestDeckContents(long id)
	{
		CollectionDeck deck = GetDeck(id);
		if (deck != null && deck.NetworkContentsLoaded())
		{
			FireDeckContentsEvent(id);
		}
		else if (Network.IsLoggedIn())
		{
			float now = Time.realtimeSinceStartup;
			if (m_pendingRequestDeckContents != null && m_pendingRequestDeckContents.TryGetValue(id, out var requestedTimestamp))
			{
				if (now - requestedTimestamp < 10f)
				{
					return;
				}
				m_pendingRequestDeckContents.Remove(id);
			}
			if (m_pendingRequestDeckContents == null)
			{
				m_pendingRequestDeckContents = new Map<long, float>();
			}
			m_pendingRequestDeckContents[id] = now;
			Network.Get().RequestDeckContents(id);
		}
		else
		{
			OnGetDeckContentsResponse();
		}
	}

	public CollectionDeck GetBaseDeck(long id)
	{
		if (m_baseDecks.TryGetValue(id, out var deck))
		{
			return deck;
		}
		return null;
	}

	public string AutoGenerateDeckName(TAG_CLASS classTag)
	{
		string className = GameStrings.GetClassName(classTag);
		int num = 1;
		string deckName;
		do
		{
			deckName = GameStrings.Format("GLUE_COLLECTION_CUSTOM_DECKNAME_TEMPLATE", className, (num == 1) ? "" : num.ToString());
			if (deckName.Length > CollectionDeck.DefaultMaxDeckNameCharacters)
			{
				deckName = GameStrings.Format("GLUE_COLLECTION_CUSTOM_DECKNAME_SHORT", className, (num == 1) ? "" : num.ToString());
			}
			num++;
		}
		while (IsDeckNameTaken(deckName));
		return deckName;
	}

	public bool HasPendingSmartDeckRequest(long deckId)
	{
		return m_smartDeckCallbackByDeckId.ContainsKey(deckId);
	}

	public void AutoFillDeck(CollectionDeck deck, bool allowSmartDeckCompletion, DeckAutoFillCallback resultCallback)
	{
		if (!HasPendingSmartDeckRequest(deck.ID))
		{
			deck.IsCreatedWithDeckComplete = true;
			if (!NetCache.Get().GetNetObject<NetCache.NetCacheFeatures>().EnableSmartDeckCompletion)
			{
				allowSmartDeckCompletion = false;
			}
			if (!Network.IsLoggedIn())
			{
				allowSmartDeckCompletion = false;
			}
			if (deck.FormatType == FormatType.FT_CLASSIC || deck.FormatType == FormatType.FT_TWIST)
			{
				allowSmartDeckCompletion = false;
			}
			if (allowSmartDeckCompletion)
			{
				m_smartDeckCallbackByDeckId.Add(deck.ID, resultCallback);
				Network.Get().RequestSmartDeckCompletion(deck);
				Processor.ScheduleCallback(5f, realTime: true, OnSmartDeckTimeout, deck.ID);
			}
			else
			{
				resultCallback(deck, DeckMaker.GetFillCards(deck, deck.GetRuleset(null)));
			}
		}
	}

	private void OnSmartDeckTimeout(object userdata)
	{
		if (userdata == null || !(userdata is long deckId))
		{
			Log.CollectionManager.PrintError("OnSmartDeckTimeout: userData is null. DeckID is not valid");
		}
		else if (HasPendingSmartDeckRequest(deckId))
		{
			CollectionDeck deck = GetDeck(deckId);
			IEnumerable<DeckMaker.DeckFill> deckFill = DeckMaker.GetFillCards(deck, deck.GetRuleset(null));
			m_smartDeckCallbackByDeckId[deckId](deck, deckFill);
			m_smartDeckCallbackByDeckId.Remove(deckId);
		}
	}

	private void OnSmartDeckResponse()
	{
		SmartDeckResponse response = Network.Get().GetSmartDeckResponse();
		if (response.HasErrorCode && response.ErrorCode != 0)
		{
			Log.CollectionManager.PrintError("OnSmartDeckResponse: Response contained errors. ErrorCode=" + response.ErrorCode);
			if (response.ResponseMessage != null)
			{
				OnSmartDeckTimeout(response.ResponseMessage.DeckId);
			}
		}
		if (response.ResponseMessage != null)
		{
			long deckId = response.ResponseMessage.DeckId;
			Processor.CancelScheduledCallback(OnSmartDeckTimeout, deckId);
			if (HasPendingSmartDeckRequest(deckId))
			{
				CollectionDeck deck = GetDeck(deckId);
				List<DeckMaker.DeckFill> fillCards = GetCardFillFromSmartDeckResponse(deck, response);
				m_smartDeckCallbackByDeckId[deckId](deck, fillCards);
				m_smartDeckCallbackByDeckId.Remove(deckId);
			}
		}
	}

	private List<DeckMaker.DeckFill> GetCardFillFromSmartDeckResponse(CollectionDeck deck, SmartDeckResponse response)
	{
		Log.CollectionManager.PrintDebug("Smart Deck Response Received: " + response.ToHumanReadableString());
		List<DeckMaker.DeckFill> fillCards = new List<DeckMaker.DeckFill>();
		int maxDeckSize = deck.GetMaxCardCount();
		foreach (DeckCardData card in response.ResponseMessage.PlayerDeckCard)
		{
			string cardId = GameUtils.TranslateDbIdToCardId(card.Def.Asset);
			int cardsToAdd = card.Qty - deck.GetCardIdCount(cardId);
			EntityDef addCardEntityDef = DefLoader.Get().GetEntityDef(card.Def.Asset);
			for (int i = 0; i < cardsToAdd; i++)
			{
				fillCards.Add(new DeckMaker.DeckFill
				{
					m_addCard = addCardEntityDef
				});
			}
			if (addCardEntityDef.HasTag(GAME_TAG.DECK_RULE_MOD_DECK_SIZE))
			{
				maxDeckSize = addCardEntityDef.GetTag(GAME_TAG.DECK_RULE_MOD_DECK_SIZE);
			}
		}
		int updatedCardCount = deck.GetTotalValidCardCount(null) + fillCards.Count;
		int additionalCardsNeeded = maxDeckSize - updatedCardCount;
		if (additionalCardsNeeded > 0)
		{
			fillCards.AddRange(DeckMaker.GetFillCards(deck, deck.GetRuleset(null)));
			Log.CollectionManager.PrintWarning("Smart Deck: Insufficient number of cards. Adding {0} more cards to deck {1}.", additionalCardsNeeded, deck.ID);
		}
		return fillCards;
	}

	private bool OnBnetError(BnetErrorInfo info, object _)
	{
		if (info.GetError() == BattleNetErrors.ERROR_ATTRIBUTE_MAX_SIZE_EXCEEDED && m_smartDeckCallbackByDeckId.Count > 0)
		{
			Log.CollectionManager.PrintError($"BnetError {info}: timing out all pending Smart Deck requests.");
			long[] array = m_smartDeckCallbackByDeckId.Keys.ToArray();
			foreach (long deckId in array)
			{
				SmartDeckRequest request = Network.GenerateSmartDeckRequestMessage(GetDeck(deckId));
				TelemetryManager.Client().SendSmartDeckCompleteFailed((int)request.GetSerializedSize());
				OnSmartDeckTimeout(deckId);
			}
			return true;
		}
		return false;
	}

	public static string GetHeroCardId(TAG_CLASS heroClass, CardHero.HeroType heroType)
	{
		if (heroClass == TAG_CLASS.WHIZBANG)
		{
			return "BOT_914h";
		}
		foreach (CardHeroDbfRecord cardHeroRecord in GameDbf.CardHero.GetRecords())
		{
			if (cardHeroRecord.HeroType == heroType && GameUtils.GetTagClassFromCardDbId(cardHeroRecord.CardId) == heroClass)
			{
				return GameUtils.TranslateDbIdToCardId(cardHeroRecord.CardId);
			}
		}
		return string.Empty;
	}

	public static string GetVanillaHero(TAG_CLASS classTag)
	{
		return GetHeroCardId(classTag, CardHero.HeroType.VANILLA);
	}

	public TAG_PREMIUM GetHeroPremium(TAG_CLASS classTag)
	{
		string heroCardId = GetVanillaHero(classTag);
		return GetBestCardPremium(heroCardId);
	}

	public bool ShouldShowDeckTemplatePageForClass(TAG_CLASS classType)
	{
		int @int = Options.Get().GetInt(Option.SKIP_DECK_TEMPLATE_PAGE_FOR_CLASS_FLAGS, 0);
		int classBit = 1 << (int)classType;
		return (@int & classBit) == 0;
	}

	public void SetShowDeckTemplatePageForClass(TAG_CLASS classType, bool show)
	{
		int skipPageFlags = Options.Get().GetInt(Option.SKIP_DECK_TEMPLATE_PAGE_FOR_CLASS_FLAGS, 0);
		int classBit = 1 << (int)classType;
		skipPageFlags |= classBit;
		if (show)
		{
			skipPageFlags ^= classBit;
		}
		Options.Get().SetInt(Option.SKIP_DECK_TEMPLATE_PAGE_FOR_CLASS_FLAGS, skipPageFlags);
	}

	public bool ShouldShowWildToStandardTutorial(bool checkPrevSceneIsPlayMode = true)
	{
		if (!ShouldAccountSeeStandardWild())
		{
			return false;
		}
		if (SceneMgr.Get().GetMode() != SceneMgr.Mode.COLLECTIONMANAGER)
		{
			return false;
		}
		if (checkPrevSceneIsPlayMode && SceneMgr.Get().GetPrevMode() != SceneMgr.Mode.TOURNAMENT)
		{
			return false;
		}
		return Options.Get().GetBool(Option.NEEDS_TO_MAKE_STANDARD_DECK);
	}

	public bool UpdateDeckWithNewId(long oldId, long newId)
	{
		if (CollectionDeckTray.Get() != null && !CollectionDeckTray.Get().GetDecksContent().UpdateDeckBoxWithNewId(oldId, newId))
		{
			return false;
		}
		CollectionDeck editedDeck = GetEditedDeck();
		if (IsInEditMode() && editedDeck.ID == oldId && m_decks.ContainsKey(newId))
		{
			m_decks[newId].CopyContents(editedDeck);
			SetEditedDeck(m_decks[newId]);
		}
		RemoveDeck(oldId);
		return true;
	}

	public int GetOwnedCount(string cardId, TAG_PREMIUM premium)
	{
		GetOwnedCardCount(cardId, out var ownedStandards, out var ownedGoldens, out var ownedSignatures, out var ownedDiamonds);
		int ownedCount = 0;
		switch (premium)
		{
		case TAG_PREMIUM.NORMAL:
			ownedCount = ownedStandards;
			break;
		case TAG_PREMIUM.GOLDEN:
			ownedCount = ownedGoldens;
			break;
		case TAG_PREMIUM.SIGNATURE:
			ownedCount = ownedSignatures;
			break;
		case TAG_PREMIUM.DIAMOND:
			ownedCount = ownedDiamonds;
			break;
		}
		return ownedCount;
	}

	public int GetTotalOwnedCount(string cardId)
	{
		GetOwnedCardCount(cardId, out var ownedStandards, out var ownedGoldens, out var ownedSignatures, out var ownedDiamonds);
		return ownedStandards + ownedGoldens + ownedSignatures + ownedDiamonds;
	}

	private void InitImpl()
	{
		m_filterIsSetRotatedCache = new Map<TAG_CARD_SET, bool>(EnumUtils.Length<TAG_CARD_SET>(), new TagCardSetEnumComparer());
		List<CardTagDbfRecord> specialQualityCardTags = GameDbf.CardTag.GetRecords().FindAll(delegate(CardTagDbfRecord record)
		{
			GAME_TAG tagId = (GAME_TAG)record.TagId;
			return tagId == GAME_TAG.HAS_DIAMOND_QUALITY || tagId == GAME_TAG.HAS_SIGNATURE_QUALITY;
		});
		List<string> collectibleCardsId = GameUtils.GetAllCollectibleCardIds();
		m_collectibleCardIndex = new Map<CollectibleCardIndex, CollectibleCard>(collectibleCardsId.Count * 2 + specialQualityCardTags.Count, new CollectibleCardIndexComparer());
		m_collectibleCards = new List<CollectibleCard>(collectibleCardsId.Count * 2 + specialQualityCardTags.Count);
		DefLoader defLoader = DefLoader.Get();
		foreach (string cardId in collectibleCardsId)
		{
			EntityDef entityDef = defLoader.GetEntityDef(cardId);
			if (entityDef == null)
			{
				Error.AddDevFatal("Failed to find an EntityDef for collectible card {0}", cardId);
				return;
			}
			RegisterCard(entityDef, cardId, TAG_PREMIUM.NORMAL);
			if (entityDef.GetCardSet() != TAG_CARD_SET.HERO_SKINS || GameUtils.IsVanillaHero(cardId))
			{
				RegisterCard(entityDef, cardId, TAG_PREMIUM.GOLDEN);
			}
		}
		foreach (CardTagDbfRecord record2 in specialQualityCardTags)
		{
			string cardId2 = GameUtils.TranslateDbIdToCardId(record2.CardId);
			if (!GameUtils.IsCardCollectible(cardId2))
			{
				continue;
			}
			EntityDef entityDef2 = defLoader.GetEntityDef(cardId2);
			if (entityDef2 != null)
			{
				TAG_PREMIUM qualityLevel = TAG_PREMIUM.NORMAL;
				switch ((GAME_TAG)record2.TagId)
				{
				case GAME_TAG.HAS_DIAMOND_QUALITY:
					qualityLevel = TAG_PREMIUM.DIAMOND;
					break;
				case GAME_TAG.HAS_SIGNATURE_QUALITY:
					qualityLevel = TAG_PREMIUM.SIGNATURE;
					break;
				default:
					Debug.LogError("CollectionManager::InitImpl - Unknown card quality level");
					break;
				}
				RegisterCard(entityDef2, cardId2, qualityLevel);
			}
		}
		Network network = Network.Get();
		network.RegisterNetHandler(GetDeckContentsResponse.PacketID.ID, OnGetDeckContentsResponse);
		network.RegisterNetHandler(DBAction.PacketID.ID, OnDBAction);
		network.RegisterNetHandler(DeckCreated.PacketID.ID, OnDeckCreatedNetworkResponse);
		network.RegisterNetHandler(DeckDeleted.PacketID.ID, OnDeckDeleted);
		network.RegisterNetHandler(DeckRenamed.PacketID.ID, OnDeckRenamed);
		network.RegisterNetHandler(SmartDeckResponse.PacketID.ID, OnSmartDeckResponse);
		network.AddBnetErrorListener(BnetFeature.Games, OnBnetError);
		if (HearthstoneApplication.IsInternal())
		{
			CheatMgr.Get().RegisterCategory("collection");
			CheatMgr.Get().RegisterCheatHandler("deckadd", OnProcessCheat_AddDecks);
			CheatMgr.Get().RegisterCheatHandler("deckreplace", OnProcessCheat_ReplaceDecks);
			CheatMgr.Get().RegisterCheatHandler("deckremoveall", OnProcessCheat_RemoveDecks);
			Get().RegisterDeckCreatedListener(OnDeckCreatedFromDeckcodeCheat);
		}
		BattlegroundsDataInit();
		LettuceInitImpl();
		NetCache.Get().RegisterCollectionManager(OnNetCacheReady);
		LoginManager.Get().OnAchievesLoaded += OnAchievesLoaded;
	}

	private void WillReset()
	{
		m_achievesLoaded = false;
		m_netCacheLoaded = false;
		m_collectionLoaded = false;
		m_duelsSessionInfoLoaded = false;
		HearthstoneApplication.Get().WillReset -= s_instance.WillReset;
		NetCache.Get().FavoriteBattlegroundsGuideSkinChanged -= s_instance.OnFavoriteBattlegroundsGuideSkinChanged;
		NetCache.Get().RemoveUpdatedListener(typeof(NetCache.NetCacheDecks), s_instance.NetCache_OnDecksReceived);
		if (HearthstoneApplication.IsInternal())
		{
			Get().RemoveDeckCreatedListener(OnDeckCreatedFromDeckcodeCheat);
		}
		m_decks.Clear();
		m_baseDecks.Clear();
		m_preconDecks.Clear();
		m_favoriteHeroChangedListeners.Clear();
		m_templateDecks.Clear();
		m_templateDeckMap.Clear();
		m_displayableCardSets.Clear();
		m_onUIHeroOverrideCardRemovedListeners.Clear();
		m_renameFinishedListeners.Clear();
		m_renameValidatedListeners.Clear();
		m_collectibleCards = new List<CollectibleCard>();
		m_collectibleCardIndex = new Map<CollectibleCardIndex, CollectibleCard>();
		m_collectionLastModifiedTime = 0f;
		m_lastSearchForWildCardsTime = 0f;
		m_EditedDeck = null;
		LettuceReset();
		s_instance = null;
	}

	private void OnCollectionChanged()
	{
		DelOnCollectionChanged[] array = m_collectionChangedListeners.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			array[i]();
		}
	}

	private CollectibleCard RegisterCard(EntityDef entityDef, string cardID, TAG_PREMIUM premium)
	{
		CollectibleCardIndex idxKey = new CollectibleCardIndex(cardID, premium);
		CollectibleCard card = null;
		if (!m_collectibleCardIndex.TryGetValue(idxKey, out card))
		{
			card = new CollectibleCard(GameUtils.GetCardRecord(cardID), entityDef, premium);
			m_collectibleCards.Add(card);
			m_collectibleCardIndex.Add(idxKey, card);
		}
		return card;
	}

	private void ClearCardCounts(EntityDef entityDef, string cardID, TAG_PREMIUM premium)
	{
		RegisterCard(entityDef, cardID, premium).ClearCounts();
	}

	private CollectibleCard SetCounts(NetCache.CardStack netStack, EntityDef entityDef)
	{
		ClearCardCounts(entityDef, netStack.Def.Name, netStack.Def.Premium);
		return AddCounts(entityDef, netStack.Def.Name, netStack.Def.Premium, new DateTime(netStack.Date), netStack.Count, netStack.NumSeen);
	}

	private CollectibleCard AddCounts(EntityDef entityDef, string cardID, TAG_PREMIUM premium, DateTime insertDate, int count, int numSeen)
	{
		if (entityDef == null)
		{
			Debug.LogError($"CollectionManager.RegisterCardStack(): DefLoader failed to get entity def for {cardID}");
			return null;
		}
		m_collectionLastModifiedTime = Time.realtimeSinceStartup;
		CollectibleCard card = RegisterCard(entityDef, cardID, premium);
		if (GameUtils.IsCoreCard(cardID))
		{
			count = Math.Min(card.DefaultMaxCopiesPerDeck - card.OwnedCount, count);
			numSeen = Math.Min(numSeen, count);
		}
		card.AddCounts(count, numSeen, insertDate);
		return card;
	}

	private void AddPreconDeckFromNotice(NetCache.ProfileNoticePreconDeck preconDeckNotice)
	{
		EntityDef hero = DefLoader.Get().GetEntityDef(preconDeckNotice.HeroAsset);
		if (hero != null)
		{
			AddPreconDeck(hero.GetClass(), preconDeckNotice.DeckID);
			NetCache.NetCacheDecks netCacheDecks = NetCache.Get().GetNetObject<NetCache.NetCacheDecks>();
			if (netCacheDecks != null)
			{
				NetCache.DeckHeader deckHeader = new NetCache.DeckHeader
				{
					ID = preconDeckNotice.DeckID,
					Name = "precon",
					Hero = hero.GetCardId(),
					HeroPower = GameUtils.GetHeroPowerCardIdFromHero(preconDeckNotice.HeroAsset),
					Type = DeckType.PRECON_DECK,
					SortOrder = preconDeckNotice.DeckID,
					SourceType = DeckSourceType.DECK_SOURCE_TYPE_BASIC_DECK
				};
				netCacheDecks.Decks.Add(deckHeader);
				Network.Get().AckNotice(preconDeckNotice.NoticeID);
			}
		}
	}

	private void AddPreconDeck(TAG_CLASS heroClass, long deckID)
	{
		if (m_preconDecks.ContainsKey(heroClass))
		{
			Log.CollectionManager.PrintDebug($"CollectionManager.AddPreconDeck(): Already have a precon deck for class {heroClass}, cannot add deckID {deckID}");
			return;
		}
		Log.CollectionManager.Print($"CollectionManager.AddPreconDeck() heroClass={heroClass} deckID={deckID}");
		m_preconDecks[heroClass] = new PreconDeck(deckID);
	}

	private CollectionDeck AddDeck(NetCache.DeckHeader deckHeader)
	{
		return AddDeck(deckHeader, updateNetCache: true);
	}

	private CollectionDeck AddDeck(NetCache.DeckHeader deckHeader, bool updateNetCache)
	{
		if (deckHeader.Type != DeckType.NORMAL_DECK && !TavernBrawlManager.IsBrawlDeckType(deckHeader.Type) && deckHeader.Type != DeckType.PVPDR_DECK)
		{
			Debug.LogWarning($"CollectionManager.AddDeck(): deckHeader {deckHeader} is not of type NORMAL_DECK, Brawl, or PVPDR deck");
			return null;
		}
		ulong createDateTimestamp = (ulong)deckHeader.ID;
		if (deckHeader.CreateDate.HasValue)
		{
			createDateTimestamp = TimeUtils.DateTimeToUnixTimeStamp(deckHeader.CreateDate.Value);
		}
		CollectionDeck deck = new CollectionDeck
		{
			ID = deckHeader.ID,
			Type = deckHeader.Type,
			Name = deckHeader.Name,
			HeroCardID = deckHeader.Hero,
			HeroOverridden = deckHeader.HeroOverridden,
			CosmeticCoinID = deckHeader.CosmeticCoin,
			RandomCoinUseFavorite = deckHeader.RandomCoinUseFavorite,
			CardBackID = deckHeader.CardBack,
			SeasonId = deckHeader.SeasonId,
			BrawlLibraryItemId = deckHeader.BrawlLibraryItemId,
			NeedsName = deckHeader.NeedsName,
			SortOrder = deckHeader.SortOrder,
			FormatType = deckHeader.FormatType,
			SourceType = deckHeader.SourceType,
			CreateDate = createDateTimestamp,
			Locked = deckHeader.Locked,
			UIHeroOverrideCardID = deckHeader.UIHeroOverride,
			UIHeroOverridePremium = deckHeader.UIHeroOverridePremium,
			RandomHeroUseFavorite = deckHeader.RandomHeroUseFavorite
		};
		deck.SetRuneOrder(deckHeader.Rune1, deckHeader.Rune2, deckHeader.Rune3);
		if (deck.NeedsName && string.IsNullOrEmpty(deck.Name))
		{
			deck.Name = GameStrings.Format("GLOBAL_BASIC_DECK_NAME", GameStrings.GetClassName(deck.GetClass()));
			Log.CollectionManager.Print($"Set deck name to {deck.Name}");
		}
		if (!IsInEditMode() || GetEditedDeck() == null || GetEditedDeck().ID != deck.ID)
		{
			if (m_decks.ContainsKey(deckHeader.ID))
			{
				m_decks.Remove(deckHeader.ID);
			}
			m_decks.Add(deckHeader.ID, deck);
		}
		CollectionDeck baseDeck = new CollectionDeck
		{
			ID = deckHeader.ID,
			Type = deckHeader.Type,
			Name = deckHeader.Name,
			HeroCardID = deckHeader.Hero,
			HeroOverridden = deckHeader.HeroOverridden,
			CosmeticCoinID = deckHeader.CosmeticCoin,
			RandomCoinUseFavorite = deckHeader.RandomCoinUseFavorite,
			CardBackID = deckHeader.CardBack,
			SeasonId = deckHeader.SeasonId,
			BrawlLibraryItemId = deckHeader.BrawlLibraryItemId,
			NeedsName = deckHeader.NeedsName,
			SortOrder = deckHeader.SortOrder,
			FormatType = deckHeader.FormatType,
			SourceType = deckHeader.SourceType,
			UIHeroOverrideCardID = deckHeader.UIHeroOverride,
			UIHeroOverridePremium = deckHeader.UIHeroOverridePremium,
			RandomHeroUseFavorite = deckHeader.RandomHeroUseFavorite
		};
		baseDeck.SetRuneOrder(deckHeader.Rune1, deckHeader.Rune2, deckHeader.Rune3);
		if (m_baseDecks.ContainsKey(deckHeader.ID))
		{
			m_baseDecks.Remove(deckHeader.ID);
		}
		m_baseDecks.Add(deckHeader.ID, baseDeck);
		if (updateNetCache)
		{
			NetCache.Get().GetNetObject<NetCache.NetCacheDecks>().Decks.Add(deckHeader);
		}
		return deck;
	}

	private CollectionDeck RemoveDeck(long id)
	{
		CollectionDeck removedDeck = null;
		if (m_baseDecks.TryGetValue(id, out removedDeck))
		{
			m_baseDecks.Remove(id);
		}
		if (m_decks.TryGetValue(id, out removedDeck))
		{
			m_decks.Remove(id);
		}
		NetCache.NetCacheDecks netCacheDecks = NetCache.Get().GetNetObject<NetCache.NetCacheDecks>();
		if (netCacheDecks == null)
		{
			return removedDeck;
		}
		for (int i = 0; i < netCacheDecks.Decks.Count; i++)
		{
			if (netCacheDecks.Decks[i].ID == id)
			{
				netCacheDecks.Decks.RemoveAt(i);
				break;
			}
		}
		return removedDeck;
	}

	private void LogAllDeckStringsInCollection()
	{
		Log.Decks.PrintInfo("Deck Contents Received:");
		foreach (CollectionDeck value in GetDecks().Values)
		{
			value.LogDeckStringInformation();
		}
	}

	private bool IsDeckNameTaken(string name)
	{
		foreach (CollectionDeck value in GetDecks().Values)
		{
			if (value.Name.Trim().Equals(name, StringComparison.InvariantCultureIgnoreCase))
			{
				return true;
			}
		}
		return false;
	}

	private void FireDeckContentsEvent(long id)
	{
		DelOnDeckContents[] array = m_deckContentsListeners.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			array[i](id);
		}
	}

	private void FireAllDeckContentsEvent()
	{
		DelOnAllDeckContents[] array = m_allDeckContentsListeners.ToArray();
		m_allDeckContentsListeners.Clear();
		DelOnAllDeckContents[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			array2[i]();
		}
	}

	private void OnNetCacheReady()
	{
		NetCache.Get().UnregisterNetCacheHandler(OnNetCacheReady);
		m_netCacheLoaded = true;
		Log.CollectionManager.Print("CollectionManager.OnNetCacheReady");
		m_displayableCardSets.AddRange(from cardSetRecord in GameDbf.CardSet.GetRecords()
			where cardSetRecord != null && cardSetRecord.IsCollectible && cardSetRecord.ID != 17 && cardSetRecord.ID != 1586 && cardSetRecord.ID != 1705
			where EventTimingManager.Get().IsEventActive(cardSetRecord.SetFilterEvent)
			select (TAG_CARD_SET)cardSetRecord.ID);
		UpdateShowAdvancedCMOption();
		if (Options.GetFormatType() == FormatType.FT_WILD && !ShouldAccountSeeStandardWild())
		{
			Log.CollectionManager.Print("Options are set to Wild mode, but account shouldn't see Standard/Wild, so setting format type to Standard!");
			Options.SetFormatType(FormatType.FT_STANDARD);
		}
		NetCache.NetCacheProfileNotices profileNotices = NetCache.Get().GetNetObject<NetCache.NetCacheProfileNotices>();
		if (profileNotices != null)
		{
			OnNewNotices(profileNotices.Notices, isInitialNoticeList: true);
		}
		NetCache.Get().RegisterNewNoticesListener(OnNewNotices);
		CheckAchievesAndNetCacheLoaded();
	}

	private void OnAchievesLoaded()
	{
		LoginManager.Get().OnAchievesLoaded -= OnAchievesLoaded;
		m_achievesLoaded = true;
		CheckAchievesAndNetCacheLoaded();
	}

	private void CheckAchievesAndNetCacheLoaded()
	{
		if (m_achievesLoaded && m_netCacheLoaded)
		{
			CreateCollectionDecksFromNetCache();
			DelOnCollectionLoaded[] array = m_collectionLoadedListeners.ToArray();
			for (int i = 0; i < array.Length; i++)
			{
				array[i]();
			}
			m_collectionLoaded = true;
			if (OnCollectionManagerReady != null)
			{
				OnCollectionManagerReady();
			}
		}
	}

	private void CreateCollectionDecksFromNetCache()
	{
		List<NetCache.DeckHeader> deckHeaders = new List<NetCache.DeckHeader>();
		NetCache.NetCacheDecks decks = NetCache.Get().GetNetObject<NetCache.NetCacheDecks>();
		if (decks != null)
		{
			deckHeaders = decks.Decks;
		}
		foreach (NetCache.DeckHeader entry in deckHeaders)
		{
			switch (entry.Type)
			{
			case DeckType.NORMAL_DECK:
			case DeckType.TAVERN_BRAWL_DECK:
			case DeckType.FSG_BRAWL_DECK:
			case DeckType.PVPDR_DECK:
				AddDeck(entry, updateNetCache: false);
				break;
			case DeckType.PRECON_DECK:
			{
				EntityDef entityDef = DefLoader.Get().GetEntityDef(entry.Hero);
				if (entityDef == null)
				{
					Debug.LogErrorFormat("CollectionManager.OnAchievesLoaded: cannot add precon deck because cannot determine class for hero with string cardId={0} (deckId={1})", entry.Hero, entry.ID);
				}
				else
				{
					AddPreconDeck(entityDef.GetClass(), entry.ID);
				}
				break;
			}
			default:
				Debug.LogWarning($"CollectionManager.OnAchievesLoaded(): don't know how to handle deck type {entry.Type}");
				break;
			}
		}
		List<DeckContents> deckContents = OfflineDataCache.GetLocalDeckContentsFromCache();
		if (deckContents != null)
		{
			UpdateFromDeckContents(deckContents);
		}
		NetCache.Get().RegisterUpdatedListener(typeof(NetCache.NetCacheDecks), s_instance.NetCache_OnDecksReceived);
	}

	private void OnNewNotices(List<NetCache.ProfileNotice> newNotices, bool isInitialNoticeList)
	{
		List<NetCache.ProfileNotice> list = newNotices.FindAll((NetCache.ProfileNotice obj) => obj.Type == NetCache.ProfileNotice.NoticeType.PRECON_DECK);
		bool newPreconDeck = false;
		foreach (NetCache.ProfileNotice item in list)
		{
			NetCache.ProfileNoticePreconDeck preconDeckNotice = item as NetCache.ProfileNoticePreconDeck;
			AddPreconDeckFromNotice(preconDeckNotice);
			newPreconDeck = true;
		}
		bool deckRemoved = false;
		foreach (NetCache.ProfileNotice item2 in newNotices.FindAll((NetCache.ProfileNotice obj) => obj.Type == NetCache.ProfileNotice.NoticeType.DECK_REMOVED))
		{
			NetCache.ProfileNoticeDeckRemoved removedDeckNotice = item2 as NetCache.ProfileNoticeDeckRemoved;
			RemoveDeck(removedDeckNotice.DeckID);
			Network.Get().AckNotice(removedDeckNotice.NoticeID);
			deckRemoved = true;
		}
		if (newPreconDeck || deckRemoved)
		{
			NetCache.Get().ReloadNetObject<NetCache.NetCacheDecks>();
		}
	}

	private void UpdateShowAdvancedCMOption()
	{
		if (Options.Get().GetBool(Option.SHOW_ADVANCED_COLLECTIONMANAGER, defaultVal: false))
		{
			return;
		}
		NetCache.NetCacheCollection collection = NetCache.Get().GetNetObject<NetCache.NetCacheCollection>();
		if (collection == null)
		{
			return;
		}
		bool cardCountRequirement = collection.TotalCardsOwned >= 116;
		if (RankMgr.Get().IsNewPlayer())
		{
			if (!AccountHasUnlockedWild() && !cardCountRequirement)
			{
				return;
			}
		}
		else if (!cardCountRequirement)
		{
			return;
		}
		Options.Get().SetBool(Option.SHOW_ADVANCED_COLLECTIONMANAGER, val: true);
	}

	private void InsertNewCollectionCard(string cardID, TAG_PREMIUM premium, DateTime insertDate, int count, bool seenBefore)
	{
		EntityDef entityDef = DefLoader.Get().GetEntityDef(cardID);
		if (entityDef == null)
		{
			Log.CollectionManager.PrintWarning("Couldn't find entity def for card with card ID {0}", cardID);
			return;
		}
		int numSeen = (seenBefore ? count : 0);
		AddCounts(entityDef, cardID, premium, insertDate, count, numSeen);
		if (entityDef.IsHeroSkin())
		{
			if (!ServiceManager.TryGet<IProductDataService>(out var dataService))
			{
				Log.CollectionManager.PrintError("Cannot update product status due to Data Service not being available");
			}
			else
			{
				dataService.UpdateProductStatus();
			}
			return;
		}
		foreach (CollectionDeck deck in GetDecks(DeckType.NORMAL_DECK))
		{
			if (!deck.IsBeingEdited())
			{
				deck.ReconcileOwnershipOnCollectionCardAdded(cardID);
			}
		}
		CollectionDeckTray deckTray = CollectionDeckTray.Get();
		if (deckTray != null)
		{
			deckTray.HandleAddedCardDeckUpdate(entityDef, premium, count);
			if (deckTray.m_decksContent != null)
			{
				deckTray.m_decksContent.RefreshMissingCardIndicators();
			}
		}
		NotifyNetCacheOfNewCards(new NetCache.CardDefinition
		{
			Name = cardID,
			Premium = premium
		}, insertDate.Ticks, count, seenBefore);
		UpdateShowAdvancedCMOption();
	}

	private void InsertNewCollectionCards(List<string> cardIDs, List<TAG_PREMIUM> cardPremiums, List<DateTime> insertDates, List<int> counts, bool seenBefore)
	{
		for (int i = 0; i < cardIDs.Count; i++)
		{
			string cardID = cardIDs[i];
			TAG_PREMIUM premium = cardPremiums[i];
			DateTime insertDate = insertDates[i];
			int count = counts[i];
			InsertNewCollectionCard(cardID, premium, insertDate, count, seenBefore);
		}
	}

	private void RemoveCollectionCard(string cardID, TAG_PREMIUM premium, int count)
	{
		GetCard(cardID, premium).RemoveCounts(count);
		m_collectionLastModifiedTime = Time.realtimeSinceStartup;
		foreach (CollectionDeck deck in GetDecks(DeckType.NORMAL_DECK))
		{
			deck.ReconcileOwnershipOnCollectionCardRemoved(cardID, premium);
		}
		foreach (CollectionDeck deck2 in GetDecks(DeckType.TAVERN_BRAWL_DECK))
		{
			deck2.ReconcileOwnershipOnCollectionCardRemoved(cardID, premium);
		}
		CollectionDeckTray deckTray = CollectionDeckTray.Get();
		if (deckTray != null)
		{
			deckTray.HandleDeletedCardDeckUpdate(cardID);
			if (deckTray.m_decksContent != null)
			{
				deckTray.m_decksContent.RefreshMissingCardIndicators();
			}
		}
		NotifyNetCacheOfRemovedCards(new NetCache.CardDefinition
		{
			Name = cardID,
			Premium = premium
		}, count);
	}

	private void UpdateCardCounts(NetCache.NetCacheCollection netCacheCards, NetCache.CardDefinition cardDef, int count, int newCount)
	{
		netCacheCards.TotalCardsOwned += count;
		if (cardDef.Premium != 0)
		{
			return;
		}
		EntityDef entityDef = DefLoader.Get().GetEntityDef(cardDef.Name);
		if (entityDef.IsCoreCard())
		{
			int maxCopies = (entityDef.IsElite() ? 1 : 2);
			if (newCount < 0 || newCount > maxCopies)
			{
				Debug.LogError("CollectionManager.UpdateCardCounts: created an illegal stack size of " + newCount + " for card " + entityDef);
				count = 0;
			}
			netCacheCards.CoreCardsUnlockedPerClass[entityDef.GetClass()].Add(entityDef.GetCardId());
		}
	}

	private void NotifyNetCacheOfRemovedCards(NetCache.CardDefinition cardDef, int count)
	{
		NetCache.NetCacheCollection netCacheCards = NetCache.Get().GetNetObject<NetCache.NetCacheCollection>();
		NetCache.CardStack netCacheStack = netCacheCards.Stacks.Find((NetCache.CardStack obj) => obj.Def.Name.Equals(cardDef.Name) && obj.Def.Premium == cardDef.Premium);
		if (netCacheStack == null)
		{
			Debug.LogError("CollectionManager.NotifyNetCacheOfRemovedCards() - trying to remove a card from an empty stack!");
			return;
		}
		netCacheStack.Count -= count;
		if (netCacheStack.Count <= 0)
		{
			netCacheCards.Stacks.Remove(netCacheStack);
		}
		UpdateCardCounts(netCacheCards, cardDef, -count, netCacheStack.Count);
	}

	private void NotifyNetCacheOfNewCards(NetCache.CardDefinition cardDef, long insertDate, int count, bool seenBefore)
	{
		NetCache.NetCacheCollection netCacheCards = NetCache.Get().GetNetObject<NetCache.NetCacheCollection>();
		if (netCacheCards == null)
		{
			return;
		}
		NetCache.CardStack netCacheStack = netCacheCards.Stacks.Find((NetCache.CardStack obj) => obj.Def.Name.Equals(cardDef.Name) && obj.Def.Premium == cardDef.Premium);
		if (netCacheStack == null)
		{
			netCacheStack = new NetCache.CardStack
			{
				Def = cardDef,
				Date = insertDate,
				Count = count,
				NumSeen = (seenBefore ? count : 0)
			};
			netCacheCards.Stacks.Add(netCacheStack);
		}
		else
		{
			if (insertDate > netCacheStack.Date)
			{
				netCacheStack.Date = insertDate;
			}
			netCacheStack.Count += count;
			if (seenBefore)
			{
				netCacheStack.NumSeen += count;
			}
		}
		UpdateCardCounts(netCacheCards, cardDef, count, netCacheStack.Count);
	}

	private void LoadTemplateDecks()
	{
		float start = Time.realtimeSinceStartup;
		foreach (DeckTemplateDbfRecord record in GameDbf.DeckTemplate.GetRecords())
		{
			EventTimingType eventType = record.Event;
			if (eventType != EventTimingType.UNKNOWN && !EventTimingManager.Get().IsEventActive(eventType))
			{
				continue;
			}
			int deckId = record.DeckId;
			if (m_templateDeckMap.ContainsKey(deckId))
			{
				continue;
			}
			DeckDbfRecord deckRecord = GameDbf.Deck.GetRecord(deckId);
			if (deckRecord == null)
			{
				Debug.LogError($"Unable to find deck with ID {deckId}");
				continue;
			}
			Map<string, int> deckCards = new Map<string, int>();
			Map<string, List<string>> sideboardCards = new Map<string, List<string>>();
			DeckCardDbfRecord currentDeckCard = GameDbf.DeckCard.GetRecord(deckRecord.TopCardId);
			while (currentDeckCard != null)
			{
				int cardDbId = currentDeckCard.CardId;
				CardDbfRecord cardRecord = GameDbf.Card.GetRecord(cardDbId);
				if (cardRecord != null)
				{
					string cardId = cardRecord.NoteMiniGuid;
					if (deckCards.ContainsKey(cardId))
					{
						deckCards[cardId]++;
					}
					else
					{
						deckCards[cardId] = 1;
					}
					if (currentDeckCard.SideboardCards.Count > 0)
					{
						sideboardCards.Add(cardId, new List<string>(currentDeckCard.SideboardCards.Count));
						foreach (SideboardCardDbfRecord sideboardCard in currentDeckCard.SideboardCards)
						{
							sideboardCards[cardId].Add(GameUtils.TranslateDbIdToCardId(sideboardCard.SideboardCardId));
						}
					}
				}
				else
				{
					Debug.LogError($"Card ID in deck not found in CARD.XML: {cardDbId}");
				}
				int nextCardId = currentDeckCard.NextCard;
				currentDeckCard = ((nextCardId != 0) ? GameDbf.DeckCard.GetRecord(nextCardId) : null);
			}
			TAG_CLASS classType = (TAG_CLASS)record.ClassId;
			List<TemplateDeck> tplDecks = null;
			if (!m_templateDecks.TryGetValue(classType, out tplDecks))
			{
				tplDecks = new List<TemplateDeck>();
				m_templateDecks.Add(classType, tplDecks);
			}
			TemplateDeck newTplDeck = new TemplateDeck
			{
				m_id = deckId,
				m_deckTemplateId = record.ID,
				m_class = classType,
				m_sortOrder = record.SortOrder,
				m_cardIds = deckCards,
				m_sideboardCardIds = sideboardCards,
				m_title = deckRecord.Name,
				m_description = deckRecord.Description,
				m_displayTexture = record.DisplayTexture,
				m_event = record.Event,
				m_isStarterDeck = record.IsStarterDeck,
				m_formatType = (FormatType)record.FormatType
			};
			if (record.DKRunes != null)
			{
				if (record.DKRunes.Count >= 1)
				{
					newTplDeck.m_rune1 = (RuneType)record.DKRunes[0].Rune;
				}
				if (record.DKRunes.Count >= 2)
				{
					newTplDeck.m_rune2 = (RuneType)record.DKRunes[1].Rune;
				}
				if (record.DKRunes.Count >= 3)
				{
					newTplDeck.m_rune3 = (RuneType)record.DKRunes[2].Rune;
				}
			}
			tplDecks.Add(newTplDeck);
			m_templateDeckMap.Add(newTplDeck.m_id, newTplDeck);
		}
		foreach (KeyValuePair<TAG_CLASS, List<TemplateDeck>> templateDeck in m_templateDecks)
		{
			templateDeck.Value.Sort(delegate(TemplateDeck a, TemplateDeck b)
			{
				int num = a.m_sortOrder.CompareTo(b.m_sortOrder);
				if (num == 0)
				{
					num = a.m_id.CompareTo(b.m_id);
				}
				return num;
			});
		}
		float end = Time.realtimeSinceStartup;
		Log.CollectionManager.Print("_decktemplate: Time spent loading template decks: " + (end - start));
	}

	public TAG_PREMIUM GetPreferredPremium()
	{
		return m_premiumPreference;
	}

	public void SetPremiumPreference(TAG_PREMIUM premium)
	{
		m_premiumPreference = premium;
		RefreshCurrentPageContents();
	}

	public void RefreshCurrentPageContents()
	{
		if (m_collectibleDisplay != null)
		{
			m_collectibleDisplay.GetPageManager().RefreshCurrentPageContents();
		}
	}

	public void RegisterDecksToRequestContentsAfterDeckSetDataResponse(List<long> decksToRequest)
	{
		foreach (long deckId in decksToRequest)
		{
			if (!m_decksToRequestContentsAfterDeckSetDataResonse.Contains(deckId))
			{
				m_decksToRequestContentsAfterDeckSetDataResonse.Add(deckId);
			}
		}
	}

	public static void ShowFeatureDisabledWhileOfflinePopup()
	{
		AlertPopup.PopupInfo info = new AlertPopup.PopupInfo
		{
			m_headerText = GameStrings.Get("GLUE_OFFLINE_FEATURE_DISABLED_HEADER"),
			m_text = GameStrings.Get("GLUE_OFFLINE_FEATURE_DISABLED_BODY"),
			m_responseDisplay = AlertPopup.ResponseDisplay.OK,
			m_showAlertIcon = false
		};
		DialogManager.Get().ShowPopup(info);
	}

	public void SetTimeOfLastPlayerDeckSave(DateTime? time)
	{
		m_timeOfLastPlayerDeckSave = time;
	}

	public static List<int> GetFeaturedCards()
	{
		return (from c in GameDbf.GetIndex().GetCardsWithFeaturedCardsEvent()
			where EventTimingManager.Get().IsEventActive(c.FeaturedCardsEvent)
			select c.ID).ToList();
	}

	private bool OnProcessCheat_AddDecks(string func, string[] args, string rawArgs)
	{
		if (args.Length != 0)
		{
			for (int i = 0; i < args.Length; i++)
			{
				ShareableDeck deck = ShareableDeck.Deserialize(args[i]);
				if (deck != null)
				{
					AddDeckFromShareableDeck(deck);
				}
			}
		}
		else
		{
			string usage = "USAGE: deckadd <whitespace separated list of deckcodes>";
			UIStatus.Get().AddInfo(usage, 5f);
		}
		return true;
	}

	private bool OnProcessCheat_ReplaceDecks(string func, string[] args, string rawArgs)
	{
		OnProcessCheat_RemoveDecks("deckremoveall", args, rawArgs);
		OnProcessCheat_AddDecks("deckadd", args, rawArgs);
		return true;
	}

	private bool OnProcessCheat_RemoveDecks(string func, string[] args, string rawArgs)
	{
		foreach (KeyValuePair<long, CollectionDeck> deck in m_decks)
		{
			Network.Get().DeleteDeck(deck.Key, deck.Value.Type);
		}
		return true;
	}

	private void AddDeckFromShareableDeck(ShareableDeck deck)
	{
		string deckcode = deck.Serialize(includeComments: false);
		if (!m_decksToCheatIn.ContainsKey(deckcode))
		{
			m_decksToCheatIn.Add(deckcode, deck);
		}
		Network.Get().CreateDeck(DeckType.NORMAL_DECK, deckcode, deck.HeroCardDbId, deck.FormatType, 0L, DeckSourceType.DECK_SOURCE_TYPE_PASTED_DECK, out var requestId, deckcode);
		if (requestId.HasValue)
		{
			m_inTransitDeckCreateRequests.Add(requestId.Value);
		}
	}

	private void OnDeckCreatedFromDeckcodeCheat(long deckId, string name)
	{
		if (!m_decksToCheatIn.TryGetValue(name, out var deck))
		{
			return;
		}
		List<Network.CardUserData> contentChanges = new List<Network.CardUserData>();
		foreach (DeckCardData card in deck.DeckContents.Cards)
		{
			Network.CardUserData cardUserData = new Network.CardUserData();
			cardUserData.DbId = card.Def.Asset;
			cardUserData.Count = card.Qty;
			cardUserData.Premium = (TAG_PREMIUM)card.Def.Premium;
			contentChanges.Add(cardUserData);
		}
		Network.Get().SendDeckData(CollectionDeck.ChangeSource.Cheat, 0, deckId, contentChanges, new List<Network.SideboardCardUserData>(), -1, false, -1, TAG_PREMIUM.NORMAL, -1, null, -1, deck.FormatType, 0L, null, null, name);
		string heroDesignerId = GameUtils.TranslateDbIdToCardId(deck.HeroCardDbId);
		string className = GameStrings.GetClassName(DefLoader.Get().GetEntityDef(heroDesignerId).GetClass());
		Network.Get().RenameDeck(deckId, "Custom " + className, playerInitiated: false);
		if (m_decks.TryGetValue(deckId, out var cachedDeck))
		{
			cachedDeck.FillFromShareableDeck(deck);
		}
		m_decksToCheatIn.Remove(name);
	}

	public FindMercenariesResult FindMercenaries(string searchString = null, bool? isOwned = null, bool? isUpgradeable = null, bool? isCraftable = null, bool? excludeCraftableFromOwned = null, bool ordered = true, bool? isFullyUpgraded = null)
	{
		FindMercenariesResult result = new FindMercenariesResult();
		List<MercenaryFilterFunc> filterFuncs = new List<MercenaryFilterFunc>();
		CollectibleCardRoleFilter.SearchTerms setSearchTerms = default(CollectibleCardRoleFilter.SearchTerms);
		if (!string.IsNullOrEmpty(searchString))
		{
			filterFuncs.AddRange(CollectibleCardRoleFilter.FilterMercsFromSearchString(searchString, ref setSearchTerms));
		}
		bool filterByOwned = setSearchTerms.Owned || (isOwned.HasValue && isOwned.Value);
		bool num = setSearchTerms.Missing || (isOwned.HasValue && !isOwned.Value);
		bool dontIncludeCraftableWithOwned = filterByOwned && excludeCraftableFromOwned.HasValue && excludeCraftableFromOwned.Value;
		if (filterByOwned)
		{
			filterFuncs.Add((LettuceMercenary merc) => merc.m_owned || (merc.IsReadyForCrafting() && !dontIncludeCraftableWithOwned));
		}
		if (num)
		{
			filterFuncs.Add((LettuceMercenary merc) => !merc.m_owned);
		}
		if (isUpgradeable.HasValue)
		{
			filterFuncs.Add((LettuceMercenary merc) => merc.CanAnyCardBeUpgraded() == isUpgradeable.Value);
		}
		if (isCraftable.HasValue)
		{
			filterFuncs.Add((LettuceMercenary merc) => !merc.m_owned && merc.IsReadyForCrafting() == isCraftable.Value);
		}
		if (isFullyUpgraded.HasValue)
		{
			filterFuncs.Add((LettuceMercenary merc) => merc.m_isFullyUpgraded == isFullyUpgraded.Value);
		}
		Predicate<LettuceMercenary> mercFilter = delegate(LettuceMercenary merc)
		{
			if (merc == null)
			{
				return false;
			}
			for (int i = 0; i < filterFuncs.Count; i++)
			{
				if (!filterFuncs[i](merc))
				{
					return false;
				}
			}
			return true;
		};
		result.m_mercenaries = m_collectibleMercenaries.FindAll(mercFilter);
		if (ordered)
		{
			result.m_mercenaries.Sort(OrderMercernaries);
		}
		return result;
	}

	public void StartInitialMercenaryLoadIfRequired()
	{
		if (m_initialDataRequested)
		{
			return;
		}
		foreach (LettuceMercenaryDbfRecord mercenaryRecord in GameDbf.LettuceMercenary.GetRecords())
		{
			if (mercenaryRecord.Collectible)
			{
				RegisterMercenary(mercenaryRecord.ID);
			}
		}
		m_initialDataRequested = true;
		Network.Get().MercenariesPlayerInfoRequest();
		Network.Get().MercenariesCollectionRequest();
		Network.Get().MercenariesTeamListRequest();
		Network.Get().MercenariesMythicTreasureScalarsRequest();
		HearthstoneApplication hearthstoneApplication = HearthstoneApplication.Get();
		if (hearthstoneApplication != null)
		{
			hearthstoneApplication.OnShutdown += OnShutdown;
			hearthstoneApplication.Paused += OnPause;
		}
	}

	private void OnPause()
	{
		if (Application.isMobilePlatform)
		{
			Network.Get().MercenariesCollectionRequest();
		}
	}

	private void OnShutdown()
	{
		Network.Get().MercenariesCollectionRequest();
	}

	public bool IsLettuceLoaded()
	{
		if (m_mercsAndTeamsReceived)
		{
			return m_playerInfoReceived;
		}
		return false;
	}

	public bool GetHasOpenedDetailsDisplay()
	{
		return m_hasVisitedDetailsDisplay;
	}

	public void SetHasVisitedDetailsDisplayTrue()
	{
		m_hasVisitedDetailsDisplay = true;
	}

	public void RegisterTeamCreatedListener(DelOnTeamCreated listener)
	{
		if (!m_teamCreatedListeners.Contains(listener))
		{
			m_teamCreatedListeners.Add(listener);
		}
	}

	public bool RemoveTeamCreatedListener(DelOnTeamCreated listener)
	{
		return m_teamCreatedListeners.Remove(listener);
	}

	public void RegisterTeamDeletedListener(DelOnTeamDeleted listener)
	{
		if (!m_teamDeletedListeners.Contains(listener))
		{
			m_teamDeletedListeners.Add(listener);
		}
	}

	public bool RemoveTeamDeletedListener(DelOnTeamDeleted listener)
	{
		return m_teamDeletedListeners.Remove(listener);
	}

	public void RegisterTeamContentsListener(DelOnTeamContents listener)
	{
		if (!m_teamContentsListeners.Contains(listener))
		{
			m_teamContentsListeners.Add(listener);
		}
	}

	public bool RemoveTeamContentsListener(DelOnTeamContents listener)
	{
		return m_teamContentsListeners.Remove(listener);
	}

	public void RegisterEditingTeamChanged(OnEditingTeamChanged listener)
	{
		if (!m_editingTeamChangedListeners.Contains(listener))
		{
			m_editingTeamChangedListeners.Add(listener);
		}
	}

	public void RemoveEditingTeamChanged(OnEditingTeamChanged listener)
	{
		m_editingTeamChangedListeners.Remove(listener);
	}

	public void TriggerNewCardSeenListeners(string id = "", TAG_PREMIUM premium = TAG_PREMIUM.NORMAL)
	{
		foreach (DelOnNewCardSeen newCardSeenListener in m_newCardSeenListeners)
		{
			newCardSeenListener(id, premium);
		}
	}

	private void ProcessMercInitDataAfterEverythingReceived()
	{
		if (m_mercTeamListResponse != null && m_mercenariesCollectionResponse != null && m_playerInfoReceived)
		{
			ProcessCollectibleMercenariesResponse(m_mercenariesCollectionResponse);
			ProcessMercenariesTeamListResponse(m_mercTeamListResponse);
			m_mercenariesCollectionResponse = null;
			m_mercTeamListResponse = null;
			m_mercsAndTeamsReceived = true;
			this.OnLettuceLoaded?.Invoke();
			this.OnLettuceLoaded = null;
		}
	}

	private void OnMercenariesCollectionResponse()
	{
		if (IsLettuceLoaded())
		{
			ProcessCollectibleMercenariesResponse(Network.Get().MercenariesCollectionResponse());
			return;
		}
		m_mercenariesCollectionResponse = Network.Get().MercenariesCollectionResponse();
		ProcessMercInitDataAfterEverythingReceived();
	}

	private void ProcessCollectibleMercenariesResponse(MercenariesCollectionResponse response)
	{
		if (response == null)
		{
			Log.CollectionManager.PrintError("OnMercenariesCollectionResponse(): No response received.");
			return;
		}
		if (!response.HasMercenaryList || response.MercenaryList == null)
		{
			Log.CollectionManager.PrintError("OnMercenariesCollectionResponse(): No mercenary list received.");
			return;
		}
		foreach (MercenaryDetailed mercenary in response.MercenaryList.Mercenaries)
		{
			if (mercenary.Mercenary.HasAssetId)
			{
				LettuceMercenary collectibleMercenary = RegisterMercenary(mercenary.Mercenary.AssetId);
				if (collectibleMercenary == null)
				{
					Log.CollectionManager.PrintError("OnMercenariesCollectionResponse(): Invalid mercenary with DB ID [{0}].", mercenary.Mercenary.AssetId);
				}
				else
				{
					UpdateCollectibleMercenary(ref collectibleMercenary, mercenary);
				}
			}
		}
	}

	private void OnMercenariesCollectionUpdate()
	{
		MercenariesCollectionUpdate response = Network.Get().MercenariesCollectionUpdate();
		if (response == null)
		{
			Log.CollectionManager.PrintError("OnMercenariesCollectionUpdate(): No response received.");
		}
		else
		{
			if (!IsLettuceLoaded())
			{
				return;
			}
			if (!response.HasMercenaryList)
			{
				Log.CollectionManager.PrintError("OnMercenariesCollectionUpdate(): No mercenary list received.");
				return;
			}
			foreach (MercenaryDetailed mercenary in response.MercenaryList.Mercenaries)
			{
				if (mercenary.Mercenary.HasAssetId)
				{
					LettuceMercenary collectibleMercenary = GetMercenary(mercenary.Mercenary.AssetId);
					if (collectibleMercenary == null)
					{
						Log.CollectionManager.PrintError("OnMercenariesCollectionResponse(): Invalid mercenary with DB ID [{0}].", mercenary.Mercenary.AssetId);
					}
					else
					{
						UpdateCollectibleMercenary(ref collectibleMercenary, mercenary);
					}
				}
			}
			GameSaveDataManager.Get().ApplyGameSaveDataUpdate(response.GameSaveData);
			LettuceVillageDataUtil.RefreshData();
		}
	}

	private void UpdateCollectibleMercenary(ref LettuceMercenary collectibleMercenary, MercenaryDetailed mercenary)
	{
		if (mercenary.Mercenary.HasAcquired)
		{
			collectibleMercenary.m_owned = mercenary.Mercenary.Acquired;
		}
		if (mercenary.HasIsFullyUpgraded)
		{
			collectibleMercenary.m_isFullyUpgraded = mercenary.IsFullyUpgraded;
		}
		if (mercenary.HasPortraitList && mercenary.PortraitList.Portraits.Count > 0)
		{
			collectibleMercenary.m_artVariations.Clear();
			foreach (MercenaryPortrait portrait in mercenary.PortraitList.Portraits)
			{
				MercenaryArtVariationPremiumDbfRecord portraitRecord = GameDbf.MercenaryArtVariationPremium.GetRecord(portrait.PortraitId);
				if (portraitRecord == null)
				{
					Log.CollectionManager.PrintError("UpdateCollectibleMercenary: no record for portrait!");
					continue;
				}
				MercenaryArtVariationDbfRecord variationRecord = GameDbf.MercenaryArtVariation.GetRecord(portraitRecord.MercenaryArtVariationId);
				if (variationRecord == null)
				{
					Log.CollectionManager.PrintError("UpdateCollectibleMercenary: no record for art variation!");
					continue;
				}
				collectibleMercenary.m_artVariations.Add(new LettuceMercenary.ArtVariation(variationRecord, (TAG_PREMIUM)portraitRecord.Premium, variationRecord.DefaultVariation, portrait.AcquireAcknowledged));
				if (portrait.Equipped)
				{
					collectibleMercenary.GetBaseLoadout().SetArtVariation(variationRecord, (TAG_PREMIUM)portraitRecord.Premium);
				}
			}
		}
		if (mercenary.Mercenary.HasExp)
		{
			collectibleMercenary.SetExperience(mercenary.Mercenary.Exp);
		}
		if (mercenary.Mercenary.HasCurrencyAmount)
		{
			collectibleMercenary.m_currencyAmount = mercenary.Mercenary.CurrencyAmount;
		}
		if (mercenary.HasAbilityList)
		{
			foreach (MercenaryAbility ability in mercenary.AbilityList.Abilities)
			{
				if (!ability.HasAssetId)
				{
					Log.CollectionManager.PrintError("OnMercenariesCollectionResponse(): No ability ID.");
					continue;
				}
				int abilityIndex = collectibleMercenary.GetAbilityIndex(ability.AssetId);
				if (abilityIndex == -1)
				{
					Log.CollectionManager.PrintError($"OnMercenariesCollectionResponse(): Ability ID [{ability.AssetId}] not found in collectible mercenary [{collectibleMercenary.m_mercName}:{collectibleMercenary.ID}].");
					continue;
				}
				LettuceAbility collectibleMercenaryAbility = collectibleMercenary.m_abilityList[abilityIndex];
				if (ability.HasTier)
				{
					collectibleMercenaryAbility.m_tier = (int)ability.Tier;
				}
				if (ability.HasMythicModifier)
				{
					collectibleMercenaryAbility.m_mythicModifier = (int)ability.MythicModifier;
				}
				collectibleMercenaryAbility.m_acquireAcknowledged = ability.AcquireAcknowledged;
				collectibleMercenaryAbility.m_upgradeAcknowledged = ability.UpgradeAcknowledged;
			}
		}
		if (mercenary.HasEquipmentList)
		{
			foreach (MercenaryEquipment equipment in mercenary.EquipmentList.Equipment)
			{
				if (!equipment.HasAssetId)
				{
					Log.CollectionManager.PrintError("UpdateCollectibleMercenary: No asset Id on Equipment!");
					continue;
				}
				int equipmentIndex = collectibleMercenary.GetEquipmentIndex(equipment.AssetId);
				if (equipmentIndex == -1)
				{
					Log.CollectionManager.PrintError("OnMercenariesCollectionResponse(): Equipment ID [{0}] not found in collectible mercenary [{1}].", equipment.AssetId, collectibleMercenary.ID);
					continue;
				}
				LettuceAbility collectibleMercenaryEquipment = collectibleMercenary.m_equipmentList[equipmentIndex];
				collectibleMercenaryEquipment.Owned = true;
				collectibleMercenaryEquipment.m_acquireAcknowledged = equipment.AcquireAcknowledged;
				collectibleMercenaryEquipment.m_upgradeAcknowledged = equipment.UpgradeAcknowledged;
				LettuceEquipmentDbfRecord record = GameDbf.LettuceEquipment.GetRecord(equipment.AssetId);
				if (equipment.HasTier)
				{
					collectibleMercenaryEquipment.m_tier = (int)equipment.Tier;
				}
				if (equipment.HasMythicModifier)
				{
					collectibleMercenaryEquipment.m_mythicModifier = (int)equipment.MythicModifier;
				}
				if (equipment.Equipped)
				{
					collectibleMercenary.GetBaseLoadout().SetSlottedEquipment(record);
				}
			}
		}
		if (mercenary.Mercenary.HasTrainingStartDate)
		{
			collectibleMercenary.m_trainingStartDate = mercenary.Mercenary.TrainingStartDate;
		}
		else
		{
			collectibleMercenary.m_trainingStartDate = null;
		}
	}

	private void OnMercenariesTeamUpdate()
	{
		MercenariesTeamUpdate response = Network.Get().MercenariesTeamUpdate();
		if (response == null)
		{
			Log.CollectionManager.PrintError("OnMercenariesTeamUpdate(): No response received.");
		}
		else if (!response.HasTeam || response.Team == null)
		{
			Log.CollectionManager.PrintError("OnMercenariesTeamUpdate(): No mercenary team received.");
		}
		else
		{
			UpdateTeam(response.Team);
		}
	}

	private void OnMercenariesTrainingAddResponse()
	{
		MercenariesTrainingAddResponse response = Network.Get().MercenariesTrainingAddResponse();
		if (response == null)
		{
			Log.CollectionManager.PrintError("OnMercenariesTrainingAddResponse(): No response received.");
			return;
		}
		LettuceMercenary collectibleMercenary = GetMercenary(response.MercenaryId);
		if (collectibleMercenary == null)
		{
			Log.CollectionManager.PrintError(string.Format("{0}(): Could not find mercenary instance {1}", "OnMercenariesTrainingAddResponse", response.MercenaryId));
			return;
		}
		collectibleMercenary.m_trainingStartDate = response.TrainingStartDate;
		this.OnMercenariesTrainingAddResponseReceived?.Invoke();
	}

	private void OnMercenariesTrainingRemoveResponse()
	{
		MercenariesTrainingRemoveResponse response = Network.Get().MercenariesTrainingRemoveResponse();
		if (response == null)
		{
			Log.CollectionManager.PrintError("OnMercenariesTrainingRemoveResponse(): No response received.");
			return;
		}
		LettuceMercenary collectibleMercenary = GetMercenary(response.MercenaryId);
		if (collectibleMercenary == null)
		{
			Log.CollectionManager.PrintError(string.Format("{0}(): Could not find mercenary instance {1}", "OnMercenariesTrainingRemoveResponse", response.MercenaryId));
			return;
		}
		collectibleMercenary.m_trainingStartDate = null;
		this.OnMercenariesTrainingRemoveResponseReceived?.Invoke();
	}

	private void OnMercenariesTrainingCollectResponse()
	{
		MercenariesTrainingCollectResponse response = Network.Get().MercenariesTrainingCollectResponse();
		if (response == null)
		{
			Log.CollectionManager.PrintError("OnMercenariesTrainingCollectResponse(): No response received.");
			return;
		}
		LettuceMercenary collectibleMercenary = GetMercenary(response.MercenaryId);
		if (collectibleMercenary == null)
		{
			Log.CollectionManager.PrintError(string.Format("{0}(): Could not find mercenary instance {1}", "OnMercenariesTrainingCollectResponse", response.MercenaryId));
			return;
		}
		collectibleMercenary.m_trainingStartDate = response.NewTrainingStartDate;
		this.OnMercenariesTrainingCollectResponseReceived?.Invoke();
	}

	private void OnMercenariesCurrencyUpdate()
	{
		Network network = Network.Get();
		if (network == null)
		{
			Log.CollectionManager.PrintError("OnMercenariesCurrencyUpdate(): Network connection does not exist. Likely this handler has not been cleaned up on desruction.");
			return;
		}
		MercenariesCurrencyUpdate response = network.MercenariesCurrencyUpdate();
		if (response == null)
		{
			Log.CollectionManager.PrintError("OnMercenariesCurrencyUpdate(): No response received.");
		}
		else
		{
			if (!IsLettuceLoaded())
			{
				return;
			}
			if (!response.HasMercenaryId)
			{
				Log.CollectionManager.PrintError("OnMercenariesCurrencyUpdate(): No mercenary ID received.");
				return;
			}
			if (!response.HasPostCurrency)
			{
				Log.CollectionManager.PrintError("OnMercenariesCurrencyUpdate(): No post currency received.");
				return;
			}
			LettuceMercenary collectibleMercenary = GetMercenary(response.MercenaryId);
			if (collectibleMercenary == null)
			{
				Log.CollectionManager.PrintError("OnMercenariesCurrencyUpdate(): Invalid mercenary with DB ID [{0}].", response.MercenaryId);
			}
			else
			{
				if (collectibleMercenary.m_currencyAmount == response.PostCurrency)
				{
					return;
				}
				collectibleMercenary.m_currencyAmount = response.PostCurrency;
				bool canAnyCardBeUpgraded = collectibleMercenary.CanAnyCardBeUpgraded();
				LettuceCollectionDisplay lcd = Get()?.GetCollectibleDisplay() as LettuceCollectionDisplay;
				if (lcd != null)
				{
					LettuceCollectionPageManager lcpm = lcd.GetPageManager() as LettuceCollectionPageManager;
					if (lcpm == null)
					{
						Log.Lettuce.PrintWarning("MercenaryDetailDisplay.UpdateDataModelsAfterTransaction - Unable to retrieve LettuceCollectionPageManager!");
					}
					else
					{
						LettuceMercenaryDataModel pageMerc = lcpm.GetMercenaryOnPage(response.MercenaryId);
						if (pageMerc != null && pageMerc.MercenaryCoin != null)
						{
							pageMerc.ChildUpgradeAvailable = canAnyCardBeUpgraded;
							pageMerc.MercenaryCoin.Quantity = (int)collectibleMercenary.m_currencyAmount;
						}
					}
				}
				MercenaryDetailDisplay mercDisplay = MercenaryDetailDisplay.Get();
				if (mercDisplay != null)
				{
					LettuceMercenaryDataModel detailsMerc = mercDisplay.GetMercenaryDisplayDataModel();
					if (detailsMerc != null)
					{
						if (detailsMerc.MercenaryCoin != null)
						{
							detailsMerc.MercenaryCoin.Quantity = (int)collectibleMercenary.m_currencyAmount;
						}
						CollectionUtils.UpdateReadyForUpgradeStatus(detailsMerc, collectibleMercenary);
					}
				}
				LettuceMercenaryDataModel trayMerc = CollectionDeckTray.Get()?.GetMercsContent()?.GetMercenaryDataModel(collectibleMercenary.ID);
				if (trayMerc != null)
				{
					trayMerc.ChildUpgradeAvailable = canAnyCardBeUpgraded;
				}
			}
		}
	}

	private void OnMercenariesExperienceUpdate()
	{
		MercenariesExperienceUpdate response = Network.Get().MercenariesExperienceUpdate();
		if (response == null)
		{
			Log.CollectionManager.PrintError("OnMercenariesExperienceUpdate(): No response received.");
			return;
		}
		if (!response.HasMercenaryId)
		{
			Log.CollectionManager.PrintError("OnMercenariesExperienceUpdate(): No mercenary ID received.");
			return;
		}
		if (!response.HasExpDelta)
		{
			Log.CollectionManager.PrintError("OnMercenariesExperienceUpdate(): No experience delta received.");
			return;
		}
		LettuceMercenary collectibleMercenary = GetMercenary(response.MercenaryId);
		if (collectibleMercenary == null)
		{
			Log.CollectionManager.PrintError("OnMercenariesExperienceUpdate(): Invalid mercenary with DB ID [{0}].", response.MercenaryId);
		}
		else
		{
			collectibleMercenary.SetExperience(collectibleMercenary.m_experience + response.ExpDelta);
		}
	}

	private void OnMercenariesRewardUpdate()
	{
		MercenariesRewardUpdate response = Network.Get().MercenariesRewardUpdate();
		if (response == null)
		{
			Log.CollectionManager.PrintError("OnMercenariesRewardUpdate(): No response received.");
			return;
		}
		foreach (MercenariesExperienceUpdate experienceUpdate in response.ExperienceUpdates)
		{
			if (!experienceUpdate.HasMercenaryId)
			{
				Log.CollectionManager.PrintError("OnMercenariesRewardUpdate(): No mercenary ID received.");
				continue;
			}
			if (!experienceUpdate.HasExpDelta)
			{
				Log.CollectionManager.PrintError("OnMercenariesRewardUpdate(): No experience delta received.");
				continue;
			}
			LettuceMercenary collectibleMercenary = GetMercenary(experienceUpdate.MercenaryId);
			if (collectibleMercenary == null)
			{
				Log.CollectionManager.PrintError("OnMercenariesRewardUpdate(): Invalid mercenary with DB ID [{0}].", experienceUpdate.MercenaryId);
				continue;
			}
			collectibleMercenary.SetExperience(collectibleMercenary.m_experience + experienceUpdate.ExpDelta);
			Log.All.Print("EXP UPDATE - mercenary[" + collectibleMercenary.ID + "] amount=" + experienceUpdate.ExpDelta + "]");
		}
		foreach (MercenariesEquipmentUpdate equipmentUpdate in response.EquipmentUpdates)
		{
			if (!equipmentUpdate.HasMercenaryId)
			{
				Log.CollectionManager.PrintError("OnMercenariesRewardUpdate(): No mercenary ID received.");
				continue;
			}
			if (!equipmentUpdate.HasEquipmentId)
			{
				Log.CollectionManager.PrintError("OnMercenariesRewardUpdate(): No equipment ID received.");
				continue;
			}
			if (!equipmentUpdate.HasTier)
			{
				Log.CollectionManager.PrintError("OnMercenariesRewardUpdate(): No tier value received.");
				continue;
			}
			LettuceMercenary collectibleMercenary2 = GetMercenary(equipmentUpdate.MercenaryId);
			if (collectibleMercenary2 == null)
			{
				Log.CollectionManager.PrintError("OnMercenariesRewardUpdate(): Invalid mercenary with DB ID [{0}].", equipmentUpdate.MercenaryId);
				continue;
			}
			LettuceAbility equipment = collectibleMercenary2.GetLettuceEquipment(equipmentUpdate.EquipmentId);
			if (equipment == null)
			{
				Log.CollectionManager.PrintError("OnMercenariesRewardUpdate(): Invalid equipment with DB ID [{0}] on mercenary with DB ID [{1}].", equipmentUpdate.EquipmentId, equipmentUpdate.MercenaryId);
			}
			else if (equipmentUpdate.HasCurrencyDelta)
			{
				collectibleMercenary2.m_currencyAmount += equipmentUpdate.CurrencyDelta;
				Log.All.Print("EQUIPMENT UPDATE - mercenary[" + collectibleMercenary2.ID + "] equipment[" + equipment.ID + "] amount=" + equipmentUpdate.CurrencyDelta + "]");
			}
			else
			{
				equipment.Owned = true;
				equipment.m_tier = (int)equipmentUpdate.Tier;
				Log.All.Print("EQUIPMENT UPDATE - mercenary[" + collectibleMercenary2.ID + "] equipment[" + equipment.ID + "] tier=" + equipmentUpdate.Tier + "]");
			}
		}
	}

	private void OnUpdateMercenariesTeamResponse()
	{
		UpdateMercenariesTeamResponse response = Network.Get().UpdateMercenariesTeamResponse();
		if (response == null)
		{
			Log.CollectionManager.PrintError("CollectionManager_Lettuce.OnUpdateMercenariesTeamResponse(): No response received.");
		}
		else if (!response.HasTeamId)
		{
			Log.CollectionManager.PrintError("CollectionManager_Lettuce.OnUpdateMercenariesTeamResponse(): No team ID received.");
		}
		else
		{
			if (m_pendingTeamEditList == null)
			{
				return;
			}
			for (int i = m_pendingTeamEditList.Count - 1; i > -1; i--)
			{
				if (m_pendingTeamEditList[i].m_teamId == response.TeamId)
				{
					m_pendingTeamEditList.RemoveAt(i);
				}
			}
		}
	}

	private void OnUpdateMercenariesTeamNameResponse()
	{
		UpdateMercenariesTeamNameResponse response = Network.Get().UpdateMercenariesTeamNameResponse();
		if (response == null)
		{
			Log.CollectionManager.PrintError("CollectionManager_Lettuce.OnUpdateMercenariesTeamNameResponce(): No response received.");
			return;
		}
		if (response.IsNameCensored && response.HasTeamName)
		{
			CollectionDeckTray deckTray = CollectionDeckTray.Get();
			DeckTrayTeamListContent teamListContent = ((deckTray != null) ? deckTray.GetTeamsContent() : null);
			if (teamListContent != null)
			{
				teamListContent.UpdateTeamName(response.TeamName);
			}
		}
		Action<bool>[] array = m_renameValidatedListeners.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			array[i](!response.IsNameCensored);
		}
	}

	private void NetCache_OnMercenariesTeamListResponse()
	{
		if (IsLettuceLoaded())
		{
			ProcessMercenariesTeamListResponse(NetCache.Get().GetNetObject<LettuceTeamList>());
			return;
		}
		m_mercTeamListResponse = NetCache.Get().GetNetObject<LettuceTeamList>();
		ProcessMercInitDataAfterEverythingReceived();
	}

	private void ProcessMercenariesTeamListResponse(LettuceTeamList netCacheTeamList)
	{
		foreach (PegasusLettuce.LettuceTeam team in netCacheTeamList.Teams)
		{
			if (!team.HasTeamId)
			{
				Log.CollectionManager.PrintError("CollectionManager_Lettuce.NetCache_OnMercenariesTeamListResponse(): Team has no team ID!");
			}
			else if (GetTeam(team.TeamId) == null)
			{
				AddReplaceTeam(team);
			}
		}
	}

	private void NetCache_OnMercenariesPlayerInfoResponse()
	{
		m_playerInfoReceived = true;
		if (!IsLettuceLoaded())
		{
			ProcessMercInitDataAfterEverythingReceived();
		}
	}

	private void OnTeamCreatedNetworkResponse()
	{
		CreateMercenariesTeamResponse response = Network.Get().CreateMercenariesTeamResponse();
		if (response == null)
		{
			Log.CollectionManager.PrintError("CollectionManager_Lettuce.OnTeamCreatedNetworkResponse(): No response received.");
			return;
		}
		if (!response.HasTeam)
		{
			Log.CollectionManager.PrintError("CollectionManager_Lettuce.OnTeamCreatedNetworkResponse(): No team received.");
			return;
		}
		if (!response.Team.HasName)
		{
			Log.CollectionManager.PrintError("CollectionManager_Lettuce.OnTeamCreatedNetworkResponse(): Received team has no name.");
			return;
		}
		if (!response.Team.HasTeamId)
		{
			Log.CollectionManager.PrintError("CollectionManager_Lettuce.OnTeamCreatedNetworkResponse(): Received teams has no team ID.");
			return;
		}
		if (!response.Team.HasType_)
		{
			Log.CollectionManager.PrintError("CollectionManager_Lettuce.OnTeamCreatedNetworkResponse(): Received teams has no team type.");
			return;
		}
		PendingTeamCreateData createData = new PendingTeamCreateData
		{
			m_name = response.Team.Name,
			m_teamId = response.Team.TeamId,
			m_type = response.Team.Type_,
			m_sortOrder = response.Team.SortOrder
		};
		int? requestId = response.RequestId;
		OnTeamCreated(createData, requestId);
	}

	private void OnTeamCreated(PendingTeamCreateData pendingTeamCreate, int? requestId)
	{
		m_pendingTeamCreate = null;
		LettuceTeam newTeam = AddTeam(pendingTeamCreate);
		newTeam?.MarkNetworkContentsLoaded();
		if (requestId.HasValue)
		{
			if (!m_inTransitTeamCreateRequests.Contains(requestId.Value))
			{
				return;
			}
			m_inTransitTeamCreateRequests.Remove(requestId.Value);
		}
		DelOnTeamCreated[] array = m_teamCreatedListeners.ToArray();
		foreach (DelOnTeamCreated listener in array)
		{
			if (newTeam != null)
			{
				listener(newTeam.ID);
			}
		}
	}

	private void OnTeamDeleted()
	{
		DeleteMercenariesTeamResponse response = Network.Get().DeleteMercenariesTeamResponse();
		if (response == null)
		{
			Log.CollectionManager.PrintError("CollectionManager_Lettuce.OnTeamDeleted(): No response received.");
			return;
		}
		if (!response.HasTeamId)
		{
			Log.CollectionManager.PrintError("CollectionManager_Lettuce.OnTeamDeleted(): No team ID received.");
			return;
		}
		Log.CollectionManager.Print("CollectionManager_Lettuce.OnTeamDeleted");
		Log.CollectionManager.Print($"TeamDeleted:{response.TeamId}");
		LettuceTeam removedTeam = RemoveTeam(response.TeamId);
		if (m_pendingTeamDeleteList != null)
		{
			for (int i = m_pendingTeamDeleteList.Count - 1; i > -1; i--)
			{
				if (m_pendingTeamDeleteList[i].m_teamId == response.TeamId)
				{
					m_pendingTeamDeleteList.RemoveAt(i);
				}
			}
		}
		if (CollectionDeckTray.Get() == null)
		{
			return;
		}
		LettuceTeam editedTeamInEditMode = GetEditingTeam();
		if (IsInEditTeamMode() && editedTeamInEditMode != null && editedTeamInEditMode.ID == response.TeamId)
		{
			Navigation.Pop();
			SceneMgr.Get().SetNextMode(SceneMgr.Mode.HUB);
			AlertPopup.PopupInfo info = new AlertPopup.PopupInfo
			{
				m_headerText = GameStrings.Get("GLUE_OFFLINE_FEATURE_DISABLED_HEADER"),
				m_text = GameStrings.Get("GLUE_OFFLINE_DECK_DELETED_REMOTELY_ERROR_BODY"),
				m_responseDisplay = AlertPopup.ResponseDisplay.OK,
				m_showAlertIcon = true
			};
			DialogManager.Get().ShowPopup(info);
		}
		if (removedTeam != null)
		{
			DelOnTeamDeleted[] array = m_teamDeletedListeners.ToArray();
			for (int j = 0; j < array.Length; j++)
			{
				array[j](removedTeam);
			}
		}
	}

	public void OnTeamDeletedWhileOffline(long teamId)
	{
	}

	public void AddPendingTeamDelete(long teamId)
	{
		if (m_pendingTeamDeleteList == null)
		{
			m_pendingTeamDeleteList = new List<PendingTeamDeleteData>();
		}
		m_pendingTeamDeleteList.Add(new PendingTeamDeleteData
		{
			m_teamId = teamId
		});
	}

	public void SendTeamNameChangedEvent()
	{
		Action[] array = m_renameFinishedListeners.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			array[i]();
		}
	}

	public void SendCreateTeam(string name, PegasusLettuce.LettuceTeam.Type type, string pastedTeamHashString = null)
	{
		if (m_pendingTeamCreate != null)
		{
			Log.Offline.PrintWarning("SendCreateTeam - Attempting to create a team while another is still pending.");
		}
		m_pendingTeamCreate = new PendingTeamCreateData
		{
			m_name = name,
			m_pastedTeamHash = pastedTeamHashString,
			m_type = type
		};
		if (Network.IsLoggedIn())
		{
			Network.Get().CreateMercenariesTeamRequest(name, type, out var requestId);
			if (requestId.HasValue)
			{
				m_inTransitTeamCreateRequests.Add(requestId.Value);
			}
		}
		else
		{
			CreateTeamOffline(m_pendingTeamCreate);
		}
	}

	private void CreateTeamOffline(PendingTeamCreateData data)
	{
		Processor.ScheduleCallback(0.5f, realTime: false, delegate
		{
			OnTeamCreated(data, null);
		});
	}

	private void FireTeamContentsEvent(long id)
	{
		DelOnTeamContents[] array = m_teamContentsListeners.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			array[i](id);
		}
	}

	public string AutoGenerateTeamName()
	{
		int num = 1;
		string teamName;
		do
		{
			teamName = GameStrings.Format("GLUE_COLLECTION_CUSTOM_TEAMNAME_TEMPLATE", num.ToString());
			num++;
		}
		while (IsTeamNameTaken(teamName));
		return teamName;
	}

	private bool IsTeamNameTaken(string name)
	{
		foreach (LettuceTeam team in GetTeams())
		{
			if (team.Name.Trim().Equals(name, StringComparison.InvariantCultureIgnoreCase))
			{
				return true;
			}
		}
		return false;
	}

	public LettuceTeam GetEditingTeam()
	{
		LettuceTeam findTeam = null;
		m_teams.TryGetValue(m_editingTeamID, out findTeam);
		return findTeam;
	}

	public LettuceTeam SetEditingTeam(long teamId, object callbackData = null)
	{
		LettuceTeam findTeam = null;
		m_teams.TryGetValue(teamId, out findTeam);
		SetEditingTeam(findTeam, callbackData);
		return findTeam;
	}

	public void SetEditingTeam(LettuceTeam team, object callbackData = null)
	{
		LettuceTeam prevEditingTeam = GetEditingTeam();
		if (team != prevEditingTeam)
		{
			m_editingTeamID = team?.ID ?? 0;
			OnEditingTeamChanged[] array = m_editingTeamChangedListeners.ToArray();
			for (int i = 0; i < array.Length; i++)
			{
				array[i](team, prevEditingTeam, callbackData);
			}
		}
	}

	public void ClearEditingTeam()
	{
		SetEditingTeam(null);
	}

	private LettuceTeam AddReplaceTeam(PegasusLettuce.LettuceTeam networkTeam)
	{
		LettuceTeam team = LettuceTeam.Convert(networkTeam);
		if (team != null)
		{
			AddTeam(team, updateNetCache: false);
			team.MarkNetworkContentsLoaded();
			FireTeamContentsEvent(team.ID);
		}
		return team;
	}

	private LettuceTeam AddTeam(PendingTeamCreateData pendingTeamCreate)
	{
		if (pendingTeamCreate == null)
		{
			return null;
		}
		LettuceTeam team = new LettuceTeam(pendingTeamCreate.m_sortOrder)
		{
			ID = pendingTeamCreate.m_teamId,
			Name = pendingTeamCreate.m_name,
			NeedsName = false,
			TeamType = pendingTeamCreate.m_type
		};
		AddTeam(team, updateNetCache: true);
		return team;
	}

	private void AddTeam(LettuceTeam team, bool updateNetCache)
	{
		if (m_teams.ContainsKey(team.ID))
		{
			m_teams.Remove(team.ID);
		}
		m_teams.Add(team.ID, team);
		if (updateNetCache)
		{
			LettuceTeamList netCacheTeamList = NetCache.Get().GetNetObject<LettuceTeamList>();
			PegasusLettuce.LettuceTeam networkTeam = netCacheTeamList.Teams.Find((PegasusLettuce.LettuceTeam t) => t.TeamId == team.ID);
			if (networkTeam != null)
			{
				netCacheTeamList.Teams.Remove(networkTeam);
			}
			networkTeam = LettuceTeam.Convert(team);
			if (networkTeam != null)
			{
				netCacheTeamList.Teams.Add(networkTeam);
			}
		}
	}

	private void UpdateTeam(PegasusLettuce.LettuceTeam team)
	{
		if (team == null)
		{
			Log.CollectionManager.PrintError("UpdateFromTeamList: teamList contained a null team!");
		}
		else if (!team.HasTeamId)
		{
			Log.CollectionManager.PrintError("UpdateFromTeamList: Team has no team ID!");
		}
		else if (!team.HasType_)
		{
			Log.CollectionManager.PrintError("UpdateFromTeamList: Team has no team type!");
		}
		else if (m_teams == null)
		{
			Log.CollectionManager.PrintError("UpdateFromTeamList: m_teams is null!");
		}
		else if (AddReplaceTeam(team) == null)
		{
			Log.CollectionManager.PrintError("UpdateFromTeamList: failed to update team!");
		}
	}

	public LettuceTeam RemoveTeam(long id)
	{
		LettuceTeam removedTeam = null;
		if (m_teams.TryGetValue(id, out removedTeam))
		{
			m_teams.Remove(id);
		}
		LettuceTeamList netCacheTeamList = NetCache.Get().GetNetObject<LettuceTeamList>();
		if (netCacheTeamList == null)
		{
			return removedTeam;
		}
		for (int i = 0; i < netCacheTeamList.Teams.Count; i++)
		{
			PegasusLettuce.LettuceTeam team = netCacheTeamList.Teams[i];
			if (team.HasTeamId && team.TeamId == id)
			{
				netCacheTeamList.Teams.RemoveAt(i);
				break;
			}
		}
		return removedTeam;
	}

	public bool IsInEditTeamMode()
	{
		return m_editTeamMode;
	}

	public LettuceTeam StartEditingTeam(long teamId, object callbackData = null)
	{
		m_editTeamMode = true;
		PresenceMgr.Get().SetStatus(Global.PresenceStatus.MERCENARIES_TEAM_EDITOR);
		return SetEditingTeam(teamId, callbackData);
	}

	public void DoneEditingTeam()
	{
		_ = m_editTeamMode;
		m_editTeamMode = false;
		PresenceMgr.Get().SetStatus(Global.PresenceStatus.MERCENARIES_COLLECTION);
	}

	public void RequestTeamContents(long id)
	{
		LettuceTeam team = GetTeam(id);
		if (team != null && team.NetworkContentsLoaded())
		{
			FireTeamContentsEvent(id);
		}
	}

	public LettuceTeam GetTeam(long id)
	{
		if (m_teams.TryGetValue(id, out var team))
		{
			return team;
		}
		return null;
	}

	public List<LettuceTeam> GetTeams()
	{
		List<LettuceTeam> teams = new List<LettuceTeam>();
		foreach (LettuceTeam team in m_teams.Values)
		{
			teams.Add(team);
		}
		return teams;
	}

	public static void SortTeams(List<LettuceTeam> teams)
	{
		teams?.Sort((LettuceTeam a, LettuceTeam b) => (a.SortOrder + -100).CompareTo(b.SortOrder + -100));
	}

	public int GetTeamSize()
	{
		return NetCache.Get().GetNetObject<NetCache.NetCacheFeatures>().MercenariesTeamMaxSize;
	}

	public LettuceMercenary GetMercenary(long mercenaryDbId, bool AttemptToGenerate = false, bool ReportError = true)
	{
		LettuceMercenary mercenary = null;
		m_collectibleMercenaryDBIds.TryGetValue(mercenaryDbId, out mercenary);
		if (mercenary == null)
		{
			m_extraMercenaryDBIds.TryGetValue(mercenaryDbId, out mercenary);
		}
		if (mercenary == null && AttemptToGenerate)
		{
			mercenary = GenerateMercenary((int)mercenaryDbId);
			if (mercenary != null)
			{
				m_extraMercenaries.Add(mercenary);
				m_extraMercenaryDBIds.Add(mercenaryDbId, mercenary);
			}
		}
		if (ReportError && mercenary == null)
		{
			Log.Lettuce.PrintError("Invalid mercenary for card ID [{0}]", mercenaryDbId);
		}
		return mercenary;
	}

	public LettuceMercenary GetMercenary(string cardId)
	{
		foreach (LettuceMercenaryDbfRecord record in GameDbf.LettuceMercenary.GetRecords())
		{
			foreach (MercenaryArtVariationDbfRecord variation in record.MercenaryArtVariations)
			{
				if (variation.CardRecord.NoteMiniGuid == cardId)
				{
					return m_collectibleMercenaryDBIds[variation.LettuceMercenaryId];
				}
			}
		}
		return null;
	}

	public int GetTotalMercenaryCount()
	{
		return m_collectibleMercenaries.Count;
	}

	public int GetOwnedMercenaryCount()
	{
		int ownedCount = 0;
		foreach (LettuceMercenary collectibleMercenary in m_collectibleMercenaries)
		{
			if (collectibleMercenary.m_owned)
			{
				ownedCount++;
			}
		}
		return ownedCount;
	}

	public bool HasFullyUpgradedAnyCollectibleMercenary()
	{
		foreach (LettuceMercenary collectibleMercenary in m_collectibleMercenaries)
		{
			if (collectibleMercenary.m_isFullyUpgraded)
			{
				return true;
			}
		}
		return false;
	}

	public void SendEquippedMercenaryEquipment(int mercenaryDbId)
	{
		LettuceMercenary mercenary = GetMercenary(mercenaryDbId);
		if (mercenary == null)
		{
			Log.CollectionManager.PrintError("SendEquippedMercenaryEquipment(): Invalid mercenary [{0}]!", mercenaryDbId);
		}
		else
		{
			Network.Get().UpdateEquippedMercenaryEquipment(mercenaryDbId, mercenary.GetCurrentLoadout().m_equipmentRecord?.ID);
			AddPendingMercenaryEdit(mercenaryDbId);
			mercenary.m_equipmentSelectionChanged = false;
		}
	}

	public void AddPendingMercenaryEdit(long mercenaryDbId)
	{
		if (m_pendingMercenaryEditList == null)
		{
			m_pendingMercenaryEditList = new List<PendingMercenaryEditData>();
		}
		m_pendingMercenaryEditList.Add(new PendingMercenaryEditData
		{
			m_mercenaryId = mercenaryDbId
		});
	}

	private void OnUpdateEquippedMercenaryEquipmentResponse()
	{
		UpdateEquippedMercenaryEquipmentResponse response = Network.Get().UpdateEquippedMercenaryEquipmentResponse();
		if (response == null)
		{
			Log.CollectionManager.PrintError("UpdateEquippedMercenaryEquipmentResponse(): No response received.");
		}
		else if (!response.HasMercenaryId)
		{
			Log.CollectionManager.PrintError("UpdateEquippedMercenaryEquipmentResponse(): No mercenary ID received.");
		}
		else
		{
			if (m_pendingMercenaryEditList == null)
			{
				return;
			}
			for (int i = m_pendingMercenaryEditList.Count - 1; i > -1; i--)
			{
				if (m_pendingMercenaryEditList[i].m_mercenaryId == response.MercenaryId)
				{
					m_pendingMercenaryEditList.RemoveAt(i);
				}
			}
		}
	}

	public void SendSelectedMercenaryArtVariation(int mercenaryDbId, int artVariationId, TAG_PREMIUM premium)
	{
		LettuceMercenary mercenary = GetMercenary(mercenaryDbId);
		if (mercenary == null)
		{
			Log.CollectionManager.PrintError("SendSelectedMercenaryArtVariation(): Invalid mercenary [{0}]!", mercenaryDbId);
		}
		else
		{
			Network.Get().UpdateEquippedMercenaryArtVariation(mercenaryDbId, artVariationId, premium);
			mercenary.SetEquippedArtVariation(artVariationId, premium);
			this.MercenaryArtVariationChangedEvent?.Invoke(mercenaryDbId, artVariationId, premium);
		}
	}

	private void OnUpdateEquippedMercenaryPortraitResponse()
	{
		UpdateEquippedMercenaryPortraitResponse response = Network.Get().UpdateEquippedMercenaryPortraitResponse();
		if (response == null)
		{
			Log.CollectionManager.PrintError("OnUpdateEquippedMercenaryPortraitResponse(): No response received.");
		}
		else if (!response.HasMercenaryId)
		{
			Log.CollectionManager.PrintError("OnUpdateEquippedMercenaryPortraitResponse(): No mercenary ID received.");
		}
	}

	private void LettuceReset()
	{
		m_teams.Clear();
		m_editingTeamID = 0L;
		m_mercsAndTeamsReceived = false;
	}

	private void LettuceInitImpl()
	{
		Network.Get().RegisterNetHandler(MercenariesCollectionResponse.PacketID.ID, OnMercenariesCollectionResponse);
		Network.Get().RegisterNetHandler(MercenariesCollectionUpdate.PacketID.ID, OnMercenariesCollectionUpdate);
		Network.Get().RegisterNetHandler(MercenariesCurrencyUpdate.PacketID.ID, OnMercenariesCurrencyUpdate);
		Network.Get().RegisterNetHandler(MercenariesExperienceUpdate.PacketID.ID, OnMercenariesExperienceUpdate);
		Network.Get().RegisterNetHandler(MercenariesRewardUpdate.PacketID.ID, OnMercenariesRewardUpdate);
		Network.Get().RegisterNetHandler(MercenariesTeamUpdate.PacketID.ID, OnMercenariesTeamUpdate);
		Network.Get().RegisterNetHandler(MercenariesTrainingAddResponse.PacketID.ID, OnMercenariesTrainingAddResponse);
		Network.Get().RegisterNetHandler(MercenariesTrainingRemoveResponse.PacketID.ID, OnMercenariesTrainingRemoveResponse);
		Network.Get().RegisterNetHandler(MercenariesTrainingCollectResponse.PacketID.ID, OnMercenariesTrainingCollectResponse);
		Network.Get().RegisterNetHandler(CreateMercenariesTeamResponse.PacketID.ID, OnTeamCreatedNetworkResponse);
		Network.Get().RegisterNetHandler(UpdateMercenariesTeamResponse.PacketID.ID, OnUpdateMercenariesTeamResponse);
		Network.Get().RegisterNetHandler(UpdateMercenariesTeamNameResponse.PacketID.ID, OnUpdateMercenariesTeamNameResponse);
		Network.Get().RegisterNetHandler(DeleteMercenariesTeamResponse.PacketID.ID, OnTeamDeleted);
		Network.Get().RegisterNetHandler(UpdateEquippedMercenaryEquipmentResponse.PacketID.ID, OnUpdateEquippedMercenaryEquipmentResponse);
		Network.Get().RegisterNetHandler(UpdateEquippedMercenaryPortraitResponse.PacketID.ID, OnUpdateEquippedMercenaryPortraitResponse);
		NetCache.Get().RegisterUpdatedListener(typeof(LettuceTeamList), NetCache_OnMercenariesTeamListResponse);
		NetCache.Get().RegisterUpdatedListener(typeof(NetCache.NetCacheMercenariesPlayerInfo), NetCache_OnMercenariesPlayerInfoResponse);
	}

	private LettuceMercenary RegisterMercenary(int mercenaryDbId)
	{
		LettuceMercenary mercenary = GetMercenary(mercenaryDbId, AttemptToGenerate: false, ReportError: false);
		if (mercenary == null)
		{
			mercenary = GenerateMercenary(mercenaryDbId);
			if (mercenary != null)
			{
				m_collectibleMercenaries.Add(mercenary);
				m_collectibleMercenaryDBIds.Add(mercenaryDbId, mercenary);
			}
		}
		return mercenary;
	}

	private void RegisterMercenaryCard(int cardDBId)
	{
		EntityDef entityDef = DefLoader.Get().GetEntityDef(cardDBId);
		if (entityDef == null)
		{
			Error.AddDevFatal($"Failed to find an EntityDef for mercenary card {cardDBId}");
		}
		else
		{
			RegisterCard(entityDef, entityDef.GetCardId(), TAG_PREMIUM.NORMAL);
		}
	}

	public LettuceMercenary GenerateMercenary(int mercenaryDbId)
	{
		LettuceMercenary mercenary = null;
		LettuceMercenaryDbfRecord mercenaryRecord = GameDbf.LettuceMercenary.GetRecord(mercenaryDbId);
		if (mercenaryRecord == null)
		{
			Log.CollectionManager.PrintError("CollectionManager_Lettuce.RegisterMercenary(): Invalid mercenary ID [{0}]!", mercenaryDbId);
			return mercenary;
		}
		mercenary = new LettuceMercenary
		{
			ID = mercenaryDbId,
			m_mercName = mercenaryRecord.NoteDesc,
			m_mercShortName = mercenaryRecord.NoteDesc,
			m_rarity = (TAG_RARITY)mercenaryRecord.Rarity,
			m_acquireType = (TAG_ACQUIRE_TYPE)mercenaryRecord.AcquireType,
			m_customAcquireText = mercenaryRecord.HowToAcquireText
		};
		LettuceMercenary.ArtVariation defaultVariation = LettuceMercenary.CreateDefaultArtVariation(mercenaryDbId);
		mercenary.m_artVariations.Add(defaultVariation);
		mercenary.GetBaseLoadout().SetArtVariation(defaultVariation.m_record, defaultVariation.m_premium);
		foreach (MercenaryArtVariationDbfRecord artVariation in mercenaryRecord.MercenaryArtVariations)
		{
			if (mercenary.m_role == TAG_ROLE.INVALID)
			{
				EntityDef mercenaryDef = DefLoader.Get().GetEntityDef(artVariation.CardId);
				if (mercenaryDef != null)
				{
					string shortName = mercenaryDef.GetShortName();
					mercenary.m_mercName = mercenaryDef.GetName();
					mercenary.m_mercShortName = (string.IsNullOrEmpty(shortName) ? mercenary.m_mercName : shortName);
					mercenary.m_role = mercenaryDef.GetTag<TAG_ROLE>(GAME_TAG.LETTUCE_ROLE);
				}
			}
			RegisterMercenaryCard(artVariation.CardId);
		}
		foreach (LettuceMercenarySpecializationDbfRecord mercenarySpecializationRecord in mercenaryRecord.LettuceMercenarySpecializations)
		{
			mercenary.m_abilitySpecializations.Add(mercenarySpecializationRecord.Name);
			foreach (LettuceMercenaryAbilityDbfRecord mercenaryAbilityRecord in mercenarySpecializationRecord.LettuceMercenaryAbilities)
			{
				LettuceAbilityDbfRecord abilityRecord = mercenaryAbilityRecord.LettuceAbilityRecord;
				LettuceAbility ability = new LettuceAbility(CollectionUtils.MercenariesModeCardType.Ability)
				{
					ID = abilityRecord.ID,
					m_abilityName = abilityRecord.NoteDesc,
					m_unlockLevel = mercenaryAbilityRecord.LettuceMercenaryLevelIdRequired
				};
				foreach (LettuceAbilityTierDbfRecord abilityTierRecord in abilityRecord.LettuceAbilityTiers)
				{
					if (abilityTierRecord.Tier < 1 || abilityTierRecord.Tier > ability.m_tierList.Length)
					{
						Log.CollectionManager.PrintError("CollectionManager_Lettuce.RegisterMercenary(): Invalid ability tier [{0}] from ability record [{1}]!", abilityTierRecord.Tier, abilityTierRecord);
						continue;
					}
					LettuceAbility.AbilityTier obj = ability.m_tierList[abilityTierRecord.Tier - 1];
					obj.ID = abilityTierRecord.ID;
					obj.m_tier = abilityTierRecord.Tier;
					obj.m_coinCost = abilityTierRecord.CoinCraftCost;
					string cardId = GameUtils.TranslateDbIdToCardId(abilityTierRecord.CardId, showWarning: true);
					obj.m_cardId = cardId;
					obj.m_cardName = abilityTierRecord.CardRecord.Name.GetString();
					obj.m_validTier = true;
					RegisterMercenaryCard(abilityTierRecord.CardId);
				}
				mercenary.m_abilityList.Add(ability);
			}
		}
		foreach (LettuceMercenaryEquipmentDbfRecord item in mercenaryRecord.LettuceMercenaryEquipment)
		{
			LettuceEquipmentDbfRecord equipmentRecord = item.LettuceEquipmentRecord;
			if (equipmentRecord == null)
			{
				Log.CollectionManager.PrintError("CollectionManager_Lettuce.RegisterMercenary(): Mercenary " + mercenaryRecord.NoteDesc + " equipment record is null!");
				continue;
			}
			LettuceAbility equipment = new LettuceAbility(CollectionUtils.MercenariesModeCardType.Equipment)
			{
				ID = equipmentRecord.ID,
				m_abilityName = equipmentRecord.NoteDesc
			};
			foreach (LettuceEquipmentTierDbfRecord equipmentTierRecord in equipmentRecord.LettuceEquipmentTiers)
			{
				if (equipmentTierRecord.Tier < 1 || equipmentTierRecord.Tier > equipment.m_tierList.Length)
				{
					Log.CollectionManager.PrintError("CollectionManager_Lettuce.RegisterMercenary(): Invalid equipment tier [{0}] from equipment record [{1}]!", equipmentTierRecord.Tier, equipmentTierRecord);
					continue;
				}
				LettuceAbility.AbilityTier obj2 = equipment.m_tierList[equipmentTierRecord.Tier - 1];
				obj2.ID = equipmentTierRecord.ID;
				obj2.m_tier = equipmentTierRecord.Tier;
				obj2.m_coinCost = equipmentTierRecord.CoinCraftCost;
				string cardId2 = GameUtils.TranslateDbIdToCardId(equipmentTierRecord.CardId, showWarning: true);
				obj2.m_cardId = cardId2;
				obj2.m_cardName = equipmentTierRecord.CardRecord.Name.GetString();
				obj2.m_validTier = true;
				RegisterMercenaryCard(equipmentTierRecord.CardId);
			}
			equipment.m_tier = equipment.GetBaseTier();
			mercenary.m_equipmentList.Add(equipment);
		}
		foreach (MercenaryAllowedTreasureDbfRecord item2 in mercenaryRecord.MercenaryTreasure)
		{
			LettuceTreasureDbfRecord treasureRecord = item2.TreasureRecord;
			if (treasureRecord == null)
			{
				Log.CollectionManager.PrintError("CollectionManager_Lettuce.RegisterMercenary(): Mercenary " + mercenaryRecord.NoteDesc + " treasure record is null!");
			}
			else
			{
				RegisterMercenaryCard(treasureRecord.CardId);
			}
		}
		return mercenary;
	}

	public (LettuceMercenary, LettuceMercenary) GetMercenariesInTraining()
	{
		(LettuceMercenary, LettuceMercenary) inTraining = (null, null);
		foreach (KeyValuePair<long, LettuceMercenary> kvp in m_collectibleMercenaryDBIds)
		{
			if (kvp.Value.m_trainingStartDate != null)
			{
				if (inTraining.Item1 != null)
				{
					inTraining.Item2 = kvp.Value;
					break;
				}
				inTraining.Item1 = kvp.Value;
			}
		}
		return inTraining;
	}

	public bool DoesMercenaryNeedToBeAcknowledged(LettuceMercenary merc)
	{
		if (merc != null)
		{
			foreach (LettuceAbility ability in merc.m_abilityList)
			{
				if (!ability.IsAcknowledged(merc))
				{
					return true;
				}
			}
			foreach (LettuceAbility equipment in merc.m_equipmentList)
			{
				if (!equipment.IsAcknowledged(merc))
				{
					return true;
				}
			}
			if (GetNumNewPortraitsToAcknowledgeForMercenary(merc) > 0)
			{
				return true;
			}
		}
		return false;
	}

	public int GetNumNewPortraitsToAcknowledgeForMercenary(LettuceMercenary merc)
	{
		int numNew = 0;
		foreach (LettuceMercenary.ArtVariation artVariation in merc.m_artVariations)
		{
			if (!artVariation.m_acknowledged)
			{
				numNew++;
			}
		}
		return numNew;
	}

	public bool DoesAnyMercenaryNeedToBeAcknowledged()
	{
		foreach (KeyValuePair<long, LettuceMercenary> kvp in m_collectibleMercenaryDBIds)
		{
			if (kvp.Value.m_owned && DoesMercenaryNeedToBeAcknowledged(kvp.Value))
			{
				return true;
			}
		}
		return false;
	}

	public int GetNumMercenariesToAcknowledgeForRole(TAG_ROLE roleTag)
	{
		int numNew = 0;
		foreach (KeyValuePair<long, LettuceMercenary> kvp in m_collectibleMercenaryDBIds)
		{
			if (kvp.Value.m_owned && kvp.Value.m_role == roleTag && DoesMercenaryNeedToBeAcknowledged(kvp.Value))
			{
				numNew++;
			}
		}
		return numNew;
	}

	public void MarkMercenaryAsAcknowledgedinCollection(MercenaryAcknowledgeData ackData)
	{
		LettuceMercenary merc = GetMercenary(ackData.MercenaryId);
		if (merc == null)
		{
			return;
		}
		switch (ackData.Type)
		{
		case MercenaryAcknowledgeData.AcknowledgeType.ACKNOWLEDGE_MERC_ABILITY_ACQUIRED:
		{
			foreach (LettuceAbility ability in merc.m_abilityList)
			{
				if (ackData.AssetId == ability.ID)
				{
					ability.m_acquireAcknowledged = true;
					break;
				}
			}
			break;
		}
		case MercenaryAcknowledgeData.AcknowledgeType.ACKNOWLEDGE_MERC_ABILITY_UPGRADE:
		{
			foreach (LettuceAbility ability3 in merc.m_abilityList)
			{
				if (ackData.AssetId == ability3.ID)
				{
					ability3.m_upgradeAcknowledged = true;
					break;
				}
			}
			break;
		}
		case MercenaryAcknowledgeData.AcknowledgeType.ACKNOWLEDGE_MERC_ABILITY_ALL:
		{
			foreach (LettuceAbility ability2 in merc.m_abilityList)
			{
				if (ackData.AssetId == ability2.ID)
				{
					ability2.m_acquireAcknowledged = true;
					ability2.m_upgradeAcknowledged = true;
					break;
				}
			}
			break;
		}
		case MercenaryAcknowledgeData.AcknowledgeType.ACKNOWLEDGE_MERC_EQUIPMENT_ACQUIRED:
		{
			foreach (LettuceAbility equipment3 in merc.m_equipmentList)
			{
				if (ackData.AssetId == equipment3.ID)
				{
					equipment3.m_acquireAcknowledged = true;
					break;
				}
			}
			break;
		}
		case MercenaryAcknowledgeData.AcknowledgeType.ACKNOWLEDGE_MERC_EQUIPMENT_UPGRADE:
		{
			foreach (LettuceAbility equipment2 in merc.m_equipmentList)
			{
				if (ackData.AssetId == equipment2.ID)
				{
					equipment2.m_upgradeAcknowledged = true;
					break;
				}
			}
			break;
		}
		case MercenaryAcknowledgeData.AcknowledgeType.ACKNOWLEDGE_MERC_EQUIPMENT_ALL:
		{
			foreach (LettuceAbility equipment in merc.m_equipmentList)
			{
				if (ackData.AssetId == equipment.ID)
				{
					equipment.m_acquireAcknowledged = true;
					equipment.m_upgradeAcknowledged = true;
					break;
				}
			}
			break;
		}
		case MercenaryAcknowledgeData.AcknowledgeType.ACKNOWLEDGE_MERC_PORTRAIT_ACQUIRED:
		{
			MercenaryArtVariationPremiumDbfRecord portraitRecord = GameDbf.MercenaryArtVariationPremium.GetRecord(ackData.AssetId);
			if (portraitRecord == null)
			{
				break;
			}
			{
				foreach (LettuceMercenary.ArtVariation artVariation in merc.m_artVariations)
				{
					if (artVariation.m_premium == (TAG_PREMIUM)portraitRecord.Premium && artVariation.m_record.ID == portraitRecord.MercenaryArtVariationId)
					{
						artVariation.m_acknowledged = true;
						break;
					}
				}
				break;
			}
		}
		}
	}
}
