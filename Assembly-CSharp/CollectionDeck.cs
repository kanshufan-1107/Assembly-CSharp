using System;
using System.Collections.Generic;
using System.Linq;
using Assets;
using PegasusShared;
using PegasusUtil;
using UnityEngine;

public class CollectionDeck
{
	public enum SlotStatus
	{
		UNKNOWN,
		VALID,
		NOT_VALID,
		MISSING
	}

	public enum ChangeSource
	{
		Unknown,
		ClickToFixMissingAndInvalidCards,
		MarkDeckAsSeen,
		PocoSetDeckName,
		OnScenePreUnload,
		SaveCurrentDeck,
		NavigateToSceneForPartyChallenge,
		StartChallengeProcess,
		StopDragToReorder,
		ReconcileCardOwnership,
		ClickToFixExtraCards,
		Cheat
	}

	public class CardCountByStatus
	{
		public int Min;

		public int Max;

		public int Total;

		public int Valid;

		public int Invalid;

		public int Missing;

		public int MissingPlusInvalid;

		public int Extra;
	}

	public static int DefaultMaxDeckNameCharacters = 24;

	public static List<DeckRule.RuleType> DefaultIgnoreRules = new List<DeckRule.RuleType>
	{
		DeckRule.RuleType.PLAYER_OWNS_EACH_COPY,
		DeckRule.RuleType.IS_CARD_PLAYABLE,
		DeckRule.RuleType.HAS_TAG_VALUE
	};

	private int m_changeNumber;

	private string m_name;

	private List<CollectionDeckSlot> m_slots = new List<CollectionDeckSlot>();

	private bool m_netContentsLoaded;

	private bool m_isSavingContentChanges;

	private bool m_isSavingNameChanges;

	private bool m_isBeingDeleted;

	private string m_randomHeroCardId = "None";

	private string m_currentDisplayHeroCardId = "None";

	private ShareableDeck m_createdFromShareableDeck;

	private readonly SideboardManager m_sideboardManager;

	public long ID;

	public DeckTemplate.SourceType TemplateSource;

	public bool HeroOverridden;

	public bool RandomHeroUseFavorite = true;

	public int? CardBackID;

	public int? CosmeticCoinID;

	public bool RandomCoinUseFavorite = true;

	public int SeasonId;

	public int BrawlLibraryItemId;

	private ChangeSource m_pendingChangeSource;

	public bool NeedsName;

	public long SortOrder;

	public ulong CreateDate;

	public bool Locked;

	public DeckSourceType SourceType;

	public string HeroPowerCardID = string.Empty;

	public string UIHeroOverrideCardID = string.Empty;

	public TAG_PREMIUM UIHeroOverridePremium;

	public int DeckTemplateId;

	private readonly RuneType[] m_runeOrder = new RuneType[DeckRule_DeathKnightRuneLimit.MaxRuneSlots];

	public virtual DeckType Type { get; set; } = DeckType.NORMAL_DECK;

	public string Name
	{
		get
		{
			return m_name;
		}
		set
		{
			if (value == null)
			{
				Debug.LogError($"CollectionDeck.SetName() - null name given for deck {this}");
			}
			else if (!value.Equals(m_name, StringComparison.InvariantCultureIgnoreCase))
			{
				m_name = value;
			}
		}
	}

	public virtual string HeroCardID { get; set; } = string.Empty;

	public RunePattern Runes { get; private set; }

	public virtual FormatType FormatType { get; set; }

	public bool IsShared { get; set; }

	public bool IsCreatedWithDeckComplete { get; set; }

	public bool IsBrawlDeck => TavernBrawlManager.IsBrawlDeckType(Type);

	public bool IsDuelsDeck
	{
		get
		{
			if (Type != DeckType.PVPDR_DECK)
			{
				return Type == DeckType.PVPDR_DISPLAY_DECK;
			}
			return true;
		}
	}

	public bool IsConstructedDeck => Type == DeckType.NORMAL_DECK;

	public bool IsTwistDeck => FormatType == FormatType.FT_TWIST;

	public bool IsStandardDeck => FormatType == FormatType.FT_STANDARD;

	public bool IsWildDeck => FormatType == FormatType.FT_WILD;

	public bool IsDeckTemplate => TemplateSource != DeckTemplate.SourceType.NONE;

	public bool IsLoanerDeck => TemplateSource == DeckTemplate.SourceType.LOANER;

	public bool IsTwistHeroicDeck
	{
		get
		{
			if (IsTwistDeck)
			{
				return TemplateSource == DeckTemplate.SourceType.SCENARIO;
			}
			return false;
		}
	}

	public bool IsValidForRuleset
	{
		get
		{
			if (IsShared)
			{
				return true;
			}
			if (!m_netContentsLoaded && Type != DeckType.CLIENT_ONLY_DECK && Type != DeckType.PVPDR_DISPLAY_DECK && !IsDeckTemplate)
			{
				return false;
			}
			DeckRuleset deckRuleset = GetRuleset(null);
			if (deckRuleset != null)
			{
				if (IsDeckTemplate)
				{
					return deckRuleset.IsDeckValid(this, DeckRule.RuleType.PLAYER_OWNS_EACH_COPY);
				}
				return deckRuleset.IsDeckValid(this);
			}
			return false;
		}
	}

	public ShareableDeck CreatedFromShareableDeck => m_createdFromShareableDeck;

	public bool HasSideboardCards => m_sideboardManager.HasSideboardCards();

	public CollectionDeck()
	{
		m_sideboardManager = new SideboardManager(this);
	}

	public override string ToString()
	{
		return $"Deck [id={ID} name=\"{Name}\" heroCardId={HeroCardID} cardBackId={CardBackID} " + $"heroOverridden={HeroOverridden} slotCount={GetSlotCount()} needsName={NeedsName} sortOrder={SortOrder}]";
	}

	public void SetRuneAtIndex(int index, RuneType runeType)
	{
		if (index < 0 || index >= m_runeOrder.Length)
		{
			Debug.LogWarning($"CollectionDeck: SetRuneAtIndex: index {index} is out of range of {m_runeOrder.Length}");
			return;
		}
		m_runeOrder[index] = runeType;
		Runes = new RunePattern(m_runeOrder);
		foreach (KeyValuePair<string, SideboardDeck> allSideboard in m_sideboardManager.GetAllSideboards())
		{
			allSideboard.Value.SetRuneAtIndex(index, runeType);
		}
	}

	public RuneType GetRuneAtIndex(int index)
	{
		if (index < 0 || index >= m_runeOrder.Length)
		{
			Debug.LogWarning($"CollectionDeck: GetRuneAtIndex: index {index} is out of range of {m_runeOrder.Length}");
			return RuneType.RT_NONE;
		}
		return m_runeOrder[index];
	}

	public RuneType[] GetRuneOrder()
	{
		RuneType[] result = new RuneType[m_runeOrder.Length];
		for (int i = 0; i < m_runeOrder.Length; i++)
		{
			result[i] = m_runeOrder[i];
		}
		return result;
	}

	public bool IsRuneOrderEqual(RuneType[] otherRuneOrder)
	{
		if (otherRuneOrder == null)
		{
			Debug.LogError("IsRuneOrderEqual() - other rune order is null.");
			return false;
		}
		int maxRuneSlots = DeckRule_DeathKnightRuneLimit.MaxRuneSlots;
		if (m_runeOrder.Length != otherRuneOrder.Length)
		{
			Debug.LogError("IsRuneOrderEqual() - rune orders are not the same length.");
			return false;
		}
		if (otherRuneOrder.Length < maxRuneSlots || m_runeOrder.Length < maxRuneSlots)
		{
			Debug.LogError("IsRuneOrderEqual() - rune order is less than MaxRuneSlots size");
			return false;
		}
		for (int i = 0; i < maxRuneSlots; i++)
		{
			if (m_runeOrder[i] != otherRuneOrder[i])
			{
				return false;
			}
		}
		return true;
	}

	public void SetRuneOrder(params RuneType[] runeTypes)
	{
		if (runeTypes == null)
		{
			Debug.LogError("SetRuneOrder() - rune types is null.");
			return;
		}
		int minRuneSlotsSize = Math.Min(m_runeOrder.Length, runeTypes.Length);
		for (int i = 0; i < minRuneSlotsSize; i++)
		{
			m_runeOrder[i] = runeTypes[i];
		}
		Runes = new RunePattern(m_runeOrder);
	}

	public void ClearRuneOrder()
	{
		SetRuneOrder(default(RuneType), default(RuneType), default(RuneType));
	}

	public bool HasUIHeroOverride()
	{
		return !string.IsNullOrEmpty(UIHeroOverrideCardID);
	}

	public string GetDisplayHeroCardID(bool rerollFavoriteHero)
	{
		if (HasUIHeroOverride())
		{
			m_currentDisplayHeroCardId = UIHeroOverrideCardID;
		}
		else if (HeroOverridden || IsDuelsDeck || IsTwistDeck)
		{
			m_currentDisplayHeroCardId = HeroCardID;
		}
		else if (rerollFavoriteHero || m_randomHeroCardId == "None")
		{
			int currentDbId = GameUtils.TranslateCardIdToDbId(m_currentDisplayHeroCardId);
			int randomHeroId = CollectionManager.Get().GetRandomHeroIdOwnedByPlayer(GetClass(), RandomHeroUseFavorite, currentDbId);
			if (randomHeroId > 0)
			{
				m_randomHeroCardId = GameUtils.TranslateDbIdToCardId(randomHeroId);
			}
			m_currentDisplayHeroCardId = m_randomHeroCardId;
		}
		return m_currentDisplayHeroCardId;
	}

	public TAG_PREMIUM? GetDisplayHeroPremiumOverride()
	{
		if (HasUIHeroOverride())
		{
			return UIHeroOverridePremium;
		}
		return null;
	}

	public List<int> GetCards()
	{
		int numSlots = m_slots.Count;
		List<int> cards = new List<int>(numSlots);
		for (int i = 0; i < numSlots; i++)
		{
			int cardId = GameUtils.TranslateCardIdToDbId(m_slots[i].CardID);
			for (int j = 0; j < m_slots[i].Count; j++)
			{
				cards.Add(cardId);
			}
		}
		return cards;
	}

	public List<string> GetCardsWithCardID()
	{
		List<string> cards = new List<string>();
		for (int i = 0; i < m_slots.Count; i++)
		{
			for (int j = 0; j < m_slots[i].Count; j++)
			{
				cards.Add(m_slots[i].CardID);
			}
		}
		return cards;
	}

	public List<CardWithPremiumStatus> GetCardsWithPremiumStatus()
	{
		List<CardWithPremiumStatus> cards = new List<CardWithPremiumStatus>();
		for (int i = 0; i < m_slots.Count; i++)
		{
			long cardId = GameUtils.TranslateCardIdToDbId(m_slots[i].CardID);
			int diamondCount = m_slots[i].GetCount(TAG_PREMIUM.DIAMOND);
			int signatureCount = m_slots[i].GetCount(TAG_PREMIUM.SIGNATURE);
			int goldenCount = m_slots[i].GetCount(TAG_PREMIUM.GOLDEN);
			int normalCount = m_slots[i].GetCount(TAG_PREMIUM.NORMAL);
			for (int j = 0; j < diamondCount; j++)
			{
				CardWithPremiumStatus diamondCard = new CardWithPremiumStatus(cardId, TAG_PREMIUM.DIAMOND);
				cards.Add(diamondCard);
			}
			for (int k = 0; k < signatureCount; k++)
			{
				CardWithPremiumStatus signatureCard = new CardWithPremiumStatus(cardId, TAG_PREMIUM.SIGNATURE);
				cards.Add(signatureCard);
			}
			for (int l = 0; l < goldenCount; l++)
			{
				CardWithPremiumStatus goldenCard = new CardWithPremiumStatus(cardId, TAG_PREMIUM.GOLDEN);
				cards.Add(goldenCard);
			}
			for (int m = 0; m < normalCount; m++)
			{
				CardWithPremiumStatus normalCard = new CardWithPremiumStatus(cardId, TAG_PREMIUM.NORMAL);
				cards.Add(normalCard);
			}
		}
		return cards;
	}

	public bool DeckTemplate_HasUnownedRequirements(out string requiredCardId)
	{
		requiredCardId = null;
		if (!IsDeckTemplate)
		{
			return false;
		}
		DeckTemplateDbfRecord deckTemplateRecord = GameDbf.DeckTemplate.GetRecord(DeckTemplateId);
		if (deckTemplateRecord == null)
		{
			return false;
		}
		CollectionManager cm = CollectionManager.Get();
		if (cm == null)
		{
			return false;
		}
		int requiredCount = 0;
		bool foundCardReq = false;
		foreach (OwnershipReqListDbfRecord req in deckTemplateRecord.OwnershipReqList)
		{
			if (req.ReqType != OwnershipReqList.ReqTypes.CARD)
			{
				continue;
			}
			foundCardReq = true;
			requiredCardId = GameUtils.TranslateDbIdToCardId(req.ReqCardId);
			requiredCount = cm.GetTotalNumCopiesInCollection(requiredCardId);
			if (requiredCount != 0)
			{
				continue;
			}
			List<CounterpartCardsDbfRecord> counterpartCards = GameUtils.GetCounterpartCards(req.ReqCardId);
			if (counterpartCards == null)
			{
				continue;
			}
			foreach (CounterpartCardsDbfRecord item in counterpartCards)
			{
				string counterpartCardId = GameUtils.TranslateDbIdToCardId(item.DeckEquivalentCardId);
				requiredCount += cm.GetTotalNumCopiesInCollection(counterpartCardId);
			}
		}
		if (foundCardReq)
		{
			return requiredCount == 0;
		}
		return false;
	}

	public void MarkNetworkContentsLoaded()
	{
		m_netContentsLoaded = true;
	}

	public bool NetworkContentsLoaded()
	{
		return m_netContentsLoaded;
	}

	public void MarkBeingDeleted()
	{
		m_isBeingDeleted = true;
	}

	public bool IsBeingDeleted()
	{
		return m_isBeingDeleted;
	}

	public bool IsSavingChanges()
	{
		if (!m_isSavingNameChanges)
		{
			return m_isSavingContentChanges;
		}
		return true;
	}

	public bool IsBeingEdited()
	{
		return this == CollectionManager.Get().GetEditedDeck();
	}

	public int GetMaxCardCount()
	{
		DeckRuleset ruleset = GetRuleset(null);
		if (ruleset != null)
		{
			return ruleset.GetDeckSize(this);
		}
		Debug.LogError("GetMaxCardCount() - unable to get correct count, ruleset was unavailable");
		return 0;
	}

	public virtual bool GetCardCountRange(out int min, out int max)
	{
		DeckRuleset ruleset = GetRuleset(null);
		if (ruleset != null)
		{
			min = ruleset.GetMinimumAllowedDeckSize(this);
			max = ruleset.GetDeckSize(this);
			return true;
		}
		Debug.LogError("GetMaxCardCount() - unable to get correct count, ruleset was unavailable");
		min = 0;
		max = 0;
		return false;
	}

	public int GetTotalCardCount()
	{
		int count = 0;
		foreach (CollectionDeckSlot slot in m_slots)
		{
			count += slot.Count;
		}
		return count;
	}

	public CardCountByStatus CountCardsByStatus(FormatType? formatTypeToValidateAgainst = null, bool isgnoreOwnership = false)
	{
		CardCountByStatus counts = new CardCountByStatus();
		foreach (CollectionDeckSlot slot in m_slots)
		{
			counts.Total += slot.Count;
			if (IsValidSlot(slot, isgnoreOwnership, ignoreGameplayEvent: false, enforceRemainingDeckRuleset: true, formatTypeToValidateAgainst))
			{
				counts.Valid += slot.Count;
			}
			else
			{
				counts.Invalid += slot.Count;
			}
		}
		GetCardCountRange(out counts.Min, out counts.Max);
		counts.Max = GetMaxCardCount();
		counts.Missing = Mathf.Max(0, counts.Min - counts.Total);
		counts.Extra = Mathf.Max(0, counts.Total - counts.Max);
		counts.MissingPlusInvalid = counts.Missing + counts.Invalid;
		return counts;
	}

	public int GetTotalValidCardCount(FormatType? formatTypeToValidateAgainst = null)
	{
		int count = 0;
		foreach (CollectionDeckSlot slot in m_slots)
		{
			if (IsValidSlot(slot, ignoreOwnership: false, ignoreGameplayEvent: false, enforceRemainingDeckRuleset: true, formatTypeToValidateAgainst))
			{
				count += slot.Count;
			}
		}
		return count;
	}

	public virtual int GetTotalInvalidCardCount(FormatType? formatTypeToValidateAgainst = null, bool includeInvalidRuneCards = false)
	{
		int count = 0;
		foreach (CollectionDeckSlot slot in m_slots)
		{
			if (!IsValidSlot(slot, IsDeckTemplate, ignoreGameplayEvent: false, enforceRemainingDeckRuleset: true, formatTypeToValidateAgainst))
			{
				count += slot.Count;
			}
		}
		if (includeInvalidRuneCards)
		{
			count += GetTotalInvalidRuneCardCount();
		}
		return count;
	}

	public int GetTotalInvalidRuneCardCount()
	{
		int count = 0;
		foreach (CollectionDeckSlot slot in m_slots)
		{
			EntityDef entityDef = slot.GetEntityDef();
			if (entityDef != null && !CanAddRunes(entityDef.GetRuneCost(), Runes.CombinedValue))
			{
				count += slot.Count;
			}
		}
		return count;
	}

	public int GetInvalidSideboardCardCount(FormatType? format)
	{
		int result = 0;
		foreach (KeyValuePair<string, SideboardDeck> allSideboard in GetAllSideboards())
		{
			CardCountByStatus sideboardCardCount = allSideboard.Value.CountCardsByStatus(format, IsLoanerDeck);
			result += sideboardCardCount.Invalid;
		}
		return result;
	}

	public int GetMissingSideboardCardCount()
	{
		int result = 0;
		foreach (KeyValuePair<string, SideboardDeck> kvp in GetAllSideboards())
		{
			EntityDef ownerEntityDef = DefLoader.Get().GetEntityDef(kvp.Key);
			if (ownerEntityDef.HasTag(GAME_TAG.MIN_SIDEBOARD_CARDS))
			{
				int minCards = ownerEntityDef.GetTag(GAME_TAG.MIN_SIDEBOARD_CARDS);
				int sideboardCardCount = kvp.Value.GetTotalCardCount();
				if (sideboardCardCount < minCards)
				{
					result += minCards - sideboardCardCount;
				}
			}
		}
		return result;
	}

	public List<CollectionDeckSlot> GetSlots()
	{
		return m_slots;
	}

	public int GetTotalDeckSizeIncludingSideboards()
	{
		int mainDeckTotal = 0;
		foreach (CollectionDeckSlot slot2 in m_slots)
		{
			mainDeckTotal += slot2?.Count ?? 0;
		}
		int sumOfAllSideboards = 0;
		foreach (KeyValuePair<string, SideboardDeck> allSideboard in GetAllSideboards())
		{
			SideboardDeck currentSideboard = allSideboard.Value;
			if (currentSideboard == null)
			{
				continue;
			}
			foreach (CollectionDeckSlot slot3 in currentSideboard.GetSlots())
			{
				sumOfAllSideboards += slot3?.Count ?? 0;
			}
		}
		return mainDeckTotal + sumOfAllSideboards;
	}

	public int GetSlotCount()
	{
		return m_slots.Count;
	}

	public int GetNumberOfCardsWithSideboards()
	{
		return m_sideboardManager.GetAllSideboards().Count;
	}

	public virtual bool IsValidSlot(CollectionDeckSlot slot, bool ignoreOwnership = false, bool ignoreGameplayEvent = false, bool enforceRemainingDeckRuleset = false, FormatType? formatTypeToValidateAgainst = null)
	{
		EntityDef entityDef = slot.GetEntityDef();
		return IsEntityDefValid(entityDef, slot, ignoreOwnership, ignoreGameplayEvent, enforceRemainingDeckRuleset, formatTypeToValidateAgainst);
	}

	public bool IsEntityDefValid(EntityDef entityDef, CollectionDeckSlot slot = null, bool ignoreOwnership = false, bool ignoreGameplayEvent = false, bool enforceRemainingDeckRuleset = false, FormatType? formatTypeToValidateAgainst = null)
	{
		if (Locked && !IsBrawlDeck)
		{
			return true;
		}
		FormatType deckFormatTypeThatCouldBeWild = formatTypeToValidateAgainst ?? FormatType;
		if (deckFormatTypeThatCouldBeWild != FormatType.FT_WILD && deckFormatTypeThatCouldBeWild != FormatType.FT_TWIST && GameUtils.IsWildCard(entityDef.GetCardId()))
		{
			return false;
		}
		if (FormatType == FormatType.FT_TWIST && (!RankMgr.IsCurrentTwistSeasonActive() || RankMgr.IsCurrentTwistSeasonUsingHeroicDecks()))
		{
			return false;
		}
		if (GetRuleset(null) == null)
		{
			Debug.LogError("IsValidSlot() - Unable to find ruleset");
			return false;
		}
		if (GetRuleset(null).HasIsPlayableRule() && !ignoreGameplayEvent && !GameUtils.IsCardGameplayEventActive(entityDef.GetCardId()))
		{
			return false;
		}
		if (slot != null && !ignoreOwnership && !slot.Owned)
		{
			return false;
		}
		if (deckFormatTypeThatCouldBeWild != 0 && GameUtils.IsBanned(this, entityDef))
		{
			return false;
		}
		if (GetRuleset(null).HasMaxTouristsRule(out var maxTourists) && entityDef.HasTag(GAME_TAG.TOURIST) && GetCardCountHasTag(GAME_TAG.TOURIST) > maxTourists)
		{
			return false;
		}
		if (enforceRemainingDeckRuleset && entityDef != null)
		{
			List<DeckRule.RuleType> ignoreRules = new List<DeckRule.RuleType>();
			if (ignoreOwnership)
			{
				ignoreRules.Add(DeckRule.RuleType.PLAYER_OWNS_EACH_COPY);
			}
			if (ignoreGameplayEvent)
			{
				ignoreRules.Add(DeckRule.RuleType.IS_CARD_PLAYABLE);
			}
			if (!GetRuleset(formatTypeToValidateAgainst).Filter(entityDef, this, (ignoreRules.Count == 0) ? null : ignoreRules.ToArray()))
			{
				return false;
			}
		}
		return true;
	}

	public virtual SlotStatus GetSlotStatus(CollectionDeckSlot slot)
	{
		if (slot == null)
		{
			return SlotStatus.UNKNOWN;
		}
		if (ShouldSplitSlotsByOwnershipOrFormatValidity())
		{
			if (!GameUtils.IsCardCollectible(slot.CardID))
			{
				return SlotStatus.NOT_VALID;
			}
			if (!GameUtils.IsCardGameplayEventEverActive(slot.CardID))
			{
				return SlotStatus.NOT_VALID;
			}
			if (!slot.Owned)
			{
				return SlotStatus.MISSING;
			}
			if (!IsValidSlot(slot, ignoreOwnership: true, ignoreGameplayEvent: false, enforceRemainingDeckRuleset: true, null))
			{
				return SlotStatus.NOT_VALID;
			}
		}
		return SlotStatus.VALID;
	}

	public bool HasReplaceableSlot()
	{
		for (int i = 0; i < m_slots.Count; i++)
		{
			if (!IsValidSlot(m_slots[i], ignoreOwnership: false, ignoreGameplayEvent: false, enforceRemainingDeckRuleset: false, null))
			{
				return true;
			}
		}
		return false;
	}

	public CollectionDeckSlot GetSlotByIndex(int slotIndex)
	{
		if (slotIndex < 0 || slotIndex >= GetSlotCount())
		{
			return null;
		}
		return m_slots[slotIndex];
	}

	public CollectionDeckSlot GetExistingSlot(CollectionDeckSlot searchSlot)
	{
		if (ShouldSplitSlotsByOwnershipOrFormatValidity())
		{
			foreach (CollectionDeckSlot slot in m_slots)
			{
				if (slot.CardID == searchSlot.CardID && slot.Owned == searchSlot.Owned)
				{
					return slot;
				}
			}
		}
		else
		{
			foreach (CollectionDeckSlot slot2 in m_slots)
			{
				if (slot2.CardID == searchSlot.CardID)
				{
					return slot2;
				}
			}
		}
		return null;
	}

	public DeckRuleset GetRuleset(FormatType? formatTypeToValidateAgainst = null)
	{
		DeckRuleset deckRuleset = null;
		switch (Type)
		{
		case DeckType.NORMAL_DECK:
		case DeckType.PRECON_DECK:
			deckRuleset = DeckRuleset.GetRuleset(formatTypeToValidateAgainst.HasValue ? formatTypeToValidateAgainst.Value : FormatType);
			break;
		case DeckType.TAVERN_BRAWL_DECK:
		case DeckType.FSG_BRAWL_DECK:
			deckRuleset = TavernBrawlManager.Get().GetCurrentDeckRuleset();
			break;
		}
		if (deckRuleset == null)
		{
			deckRuleset = DeckRuleset.GetRuleset(FormatType.FT_WILD);
		}
		return deckRuleset;
	}

	public bool IsValidForFormat(FormatType formatType)
	{
		if (formatType == FormatType.FT_WILD && FormatType == FormatType.FT_STANDARD)
		{
			return true;
		}
		return FormatType == formatType;
	}

	public static bool DoesModeRequireSpecificFormat(SceneMgr.Mode mode, bool isRanked)
	{
		if (mode == SceneMgr.Mode.TOURNAMENT && isRanked)
		{
			return true;
		}
		if (mode == SceneMgr.Mode.FRIENDLY)
		{
			return true;
		}
		return false;
	}

	public bool IsValidForModeAndFormat(SceneMgr.Mode mode, bool isRanked, FormatType formatType)
	{
		if (!GameUtils.HasUnlockedClasses(GetHeroClasses()))
		{
			return false;
		}
		bool isValidAdventureFormat = FormatType != FormatType.FT_CLASSIC && FormatType != FormatType.FT_TWIST;
		if (mode == SceneMgr.Mode.ADVENTURE && !isValidAdventureFormat)
		{
			return false;
		}
		if (FormatType == FormatType.FT_CLASSIC && !GameUtils.CLASSIC_ORDERED_HERO_CLASSES.Contains(GetClass()))
		{
			return false;
		}
		if (FormatType == FormatType.FT_TWIST && (mode == SceneMgr.Mode.TOURNAMENT || mode == SceneMgr.Mode.FRIENDLY))
		{
			if (RankMgr.IsTwistDeckWithNoSeason(this))
			{
				return false;
			}
			if (RankMgr.IsClassLockedForTwist(GetClass()))
			{
				return false;
			}
			if (!RankMgr.IsTwistDeckValidForSeason(this))
			{
				return false;
			}
			if (!isRanked && IsTwistHeroicDeck)
			{
				return false;
			}
		}
		if (DoesModeRequireSpecificFormat(mode, isRanked) && !IsValidForFormat(formatType))
		{
			return false;
		}
		return true;
	}

	public void CopyFrom(CollectionDeck otherDeck)
	{
		ID = otherDeck.ID;
		Type = otherDeck.Type;
		m_name = otherDeck.m_name;
		HeroCardID = otherDeck.HeroCardID;
		HeroOverridden = otherDeck.HeroOverridden;
		CosmeticCoinID = otherDeck.CosmeticCoinID;
		RandomCoinUseFavorite = otherDeck.RandomCoinUseFavorite;
		CardBackID = otherDeck.CardBackID;
		NeedsName = otherDeck.NeedsName;
		SeasonId = otherDeck.SeasonId;
		BrawlLibraryItemId = otherDeck.BrawlLibraryItemId;
		FormatType = otherDeck.FormatType;
		SortOrder = otherDeck.SortOrder;
		SourceType = otherDeck.SourceType;
		UIHeroOverrideCardID = otherDeck.UIHeroOverrideCardID;
		UIHeroOverridePremium = otherDeck.UIHeroOverridePremium;
		RuneType[] otherRuneOrder = otherDeck.GetRuneOrder();
		SetRuneOrder(otherRuneOrder);
		m_slots.Clear();
		for (int i = 0; i < otherDeck.GetSlotCount(); i++)
		{
			CollectionDeckSlot otherSlot = otherDeck.GetSlotByIndex(i);
			CollectionDeckSlot slot = new CollectionDeckSlot();
			slot.CopyFrom(otherSlot);
			m_slots.Add(slot);
		}
		m_sideboardManager.ClearSideboards();
		foreach (KeyValuePair<string, SideboardDeck> allSideboard in otherDeck.GetAllSideboards())
		{
			SideboardDeck otherSideboard = allSideboard.Value;
			m_sideboardManager.CopySideboard(otherSideboard);
		}
	}

	public void CopyContents(CollectionDeck otherDeck)
	{
		HeroCardID = otherDeck.HeroCardID;
		UIHeroOverrideCardID = otherDeck.UIHeroOverrideCardID;
		UIHeroOverridePremium = otherDeck.UIHeroOverridePremium;
		RuneType[] otherRuneOrder = otherDeck.GetRuneOrder();
		SetRuneOrder(otherRuneOrder);
		m_slots.Clear();
		m_sideboardManager.ClearSideboards();
		for (int i = 0; i < otherDeck.GetSlotCount(); i++)
		{
			CollectionDeckSlot otherSlot = otherDeck.GetSlotByIndex(i);
			foreach (TAG_PREMIUM premiumType in Enum.GetValues(typeof(TAG_PREMIUM)))
			{
				for (int c = 0; c < otherSlot.GetCount(premiumType); c++)
				{
					AddCard(otherSlot.CardID, premiumType, true, null);
				}
			}
		}
		foreach (KeyValuePair<string, SideboardDeck> kvp in otherDeck.GetAllSideboards())
		{
			SideboardDeck otherSideboard = kvp.Value;
			m_sideboardManager.GetOrCreateSideboard(kvp.Key, otherSideboard.DataModel.Premium, setEdited: false).AddCardsFrom(otherSideboard);
		}
	}

	public bool FillFromShareableDeck(ShareableDeck shareableDeck)
	{
		HeroCardID = GameUtils.TranslateDbIdToCardId(shareableDeck.HeroCardDbId);
		FormatType = shareableDeck.FormatType;
		bool wereAllCardsAdded = true;
		m_slots.Clear();
		for (int i = 0; i < shareableDeck.DeckContents.Cards.Count; i++)
		{
			string cardId = GameUtils.TranslateDbIdToCardId(shareableDeck.DeckContents.Cards[i].Def.Asset);
			TAG_PREMIUM premium = (TAG_PREMIUM)shareableDeck.DeckContents.Cards[i].Def.Premium;
			int cardQuantity = shareableDeck.DeckContents.Cards[i].Qty;
			for (int q = 0; q < cardQuantity; q++)
			{
				if (!AddCard(cardId, premium, false, null))
				{
					wereAllCardsAdded = false;
				}
			}
		}
		foreach (SideBoardCardData sideBoardCardData in shareableDeck.DeckContents.SideboardCards)
		{
			string ownerCardId = GameUtils.TranslateDbIdToCardId(sideBoardCardData.LinkedCardDbId);
			SideboardDeck sideboard = m_sideboardManager.GetOrCreateSideboard(ownerCardId, (TAG_PREMIUM)sideBoardCardData.Def.Premium, setEdited: false);
			if (sideboard == null)
			{
				wereAllCardsAdded = false;
				continue;
			}
			string cardId2 = GameUtils.TranslateDbIdToCardId(sideBoardCardData.Def.Asset);
			sideboard.AddCard(cardId2, (TAG_PREMIUM)sideBoardCardData.Def.Premium, false, null);
		}
		return wereAllCardsAdded;
	}

	public void FillFromTemplateDeck(CollectionManager.TemplateDeck tplDeck)
	{
		ClearSlotContents();
		Name = tplDeck.m_title;
		SetRuneOrder(tplDeck.m_rune1, tplDeck.m_rune2, tplDeck.m_rune3);
		foreach (KeyValuePair<string, int> cardId in tplDeck.m_cardIds)
		{
			CollectionManager.Get().GetOwnedCardCount(cardId.Key, out var _, out var ownedGoldens, out var ownedSignature, out var ownedDiamonds);
			int addTplCount = cardId.Value;
			while (addTplCount > 0 && ownedDiamonds > 0)
			{
				AddCard(cardId.Key, TAG_PREMIUM.DIAMOND, true, null);
				ownedDiamonds--;
				addTplCount--;
			}
			while (addTplCount > 0 && ownedSignature > 0)
			{
				AddCard(cardId.Key, TAG_PREMIUM.SIGNATURE, true, null);
				ownedSignature--;
				addTplCount--;
			}
			while (addTplCount > 0 && ownedGoldens > 0)
			{
				AddCard(cardId.Key, TAG_PREMIUM.GOLDEN, true, null);
				ownedGoldens--;
				addTplCount--;
			}
			while (addTplCount > 0)
			{
				AddCard(cardId.Key, TAG_PREMIUM.NORMAL, true, null);
				addTplCount--;
			}
		}
		foreach (KeyValuePair<string, List<string>> sideboardCardId in tplDeck.m_sideboardCardIds)
		{
			sideboardCardId.Deconstruct(out var key, out var value);
			string cardId2 = key;
			List<string> sideboardCards = value;
			int sideboardOwnerDbId = GameUtils.TranslateCardIdToDbId(cardId2);
			foreach (string sideboardCard in sideboardCards)
			{
				CollectionManager.Get().GetOwnedCardCount(sideboardCard, out var ownedStandards2, out var ownedGoldens2, out var ownedSignature2, out var ownedDiamonds2);
				ownedDiamonds2 -= GetOwnedCardCountInDeck(sideboardCard, TAG_PREMIUM.DIAMOND);
				ownedSignature2 -= GetOwnedCardCountInDeck(sideboardCard, TAG_PREMIUM.SIGNATURE);
				ownedGoldens2 -= GetOwnedCardCountInDeck(sideboardCard, TAG_PREMIUM.GOLDEN);
				ownedStandards2 -= GetOwnedCardCountInDeck(sideboardCard, TAG_PREMIUM.NORMAL);
				if (ownedDiamonds2 > 0)
				{
					AddCardToSideboard(sideboardCard, sideboardOwnerDbId, TAG_PREMIUM.DIAMOND, allowInvalid: true);
				}
				else if (ownedSignature2 > 0)
				{
					AddCardToSideboard(sideboardCard, sideboardOwnerDbId, TAG_PREMIUM.SIGNATURE, allowInvalid: true);
				}
				else if (ownedGoldens2 > 0)
				{
					AddCardToSideboard(sideboardCard, sideboardOwnerDbId, TAG_PREMIUM.GOLDEN, allowInvalid: true);
				}
				else
				{
					AddCardToSideboard(sideboardCard, sideboardOwnerDbId, TAG_PREMIUM.NORMAL, allowInvalid: true);
				}
			}
		}
		SetRuneOrder(tplDeck.m_rune1, tplDeck.m_rune2, tplDeck.m_rune3);
	}

	public void FillFromCardList(IEnumerable<DeckMaker.DeckFill> fillCards, ChangeSource changeSource)
	{
		if (fillCards == null)
		{
			return;
		}
		foreach (DeckMaker.DeckFill fillCard in fillCards)
		{
			if (GetTotalCardCount() >= GetMaxCardCount())
			{
				break;
			}
			if (fillCard.m_addCard != null)
			{
				TAG_PREMIUM? premiumTag = GetPreferredPremiumThatCanBeAdded(fillCard.m_addCard.GetCardId());
				if (premiumTag.HasValue)
				{
					AddCard(fillCard.m_addCard.GetCardId(), premiumTag.Value, false, null);
				}
			}
		}
		SendChanges(changeSource);
	}

	public void ReconcileOwnershipOnCollectionCardRemoved(string cardID, TAG_PREMIUM premium)
	{
		int numAlreadySlotted = ReconcileOwnershipOnRemoval(cardID, premium);
		foreach (KeyValuePair<string, SideboardDeck> allSideboard in m_sideboardManager.GetAllSideboards())
		{
			numAlreadySlotted += allSideboard.Value.ReconcileOwnershipOnRemoval(cardID, premium, numAlreadySlotted);
		}
		if (!IsBeingEdited())
		{
			m_pendingChangeSource = ChangeSource.ReconcileCardOwnership;
		}
	}

	private int ReconcileOwnershipOnRemoval(string cardID, TAG_PREMIUM premium, int numAlreadySlotted = 0)
	{
		CollectionDeckSlot ownedSlot = FindFirstOwnedSlotByCardId(cardID, owned: true);
		if (ownedSlot == null)
		{
			return 0;
		}
		int numAvailable = Mathf.Max(CollectionManager.Get().GetOwnedCount(cardID, premium) - numAlreadySlotted, 0);
		int slotCount = ownedSlot.GetCount(premium);
		if (numAvailable >= slotCount)
		{
			return slotCount;
		}
		int numCardsToRemove = slotCount - numAvailable;
		ownedSlot.RemoveCard(numCardsToRemove, premium);
		if (numCardsToRemove > 0)
		{
			RemoveSideboard(cardID);
		}
		while (numCardsToRemove > 0)
		{
			TAG_PREMIUM? premiumToAdd = GetPreferredPremiumThatCanBeAdded(cardID).GetValueOrDefault();
			AddCard(cardID, premiumToAdd.Value, false, null);
			numCardsToRemove--;
		}
		return ownedSlot.GetCount(premium);
	}

	public void ReconcileOwnershipOnCollectionCardAdded(string cardID)
	{
		bool wasDeckModified = ReconcileOwnershipOnAdd(cardID);
		foreach (KeyValuePair<string, SideboardDeck> allSideboard in m_sideboardManager.GetAllSideboards())
		{
			if (allSideboard.Value.ReconcileOwnershipOnAdd(cardID))
			{
				wasDeckModified = true;
			}
		}
		if (wasDeckModified)
		{
			m_pendingChangeSource = ChangeSource.ReconcileCardOwnership;
		}
	}

	private bool ReconcileOwnershipOnAdd(string cardID)
	{
		CollectionDeckSlot unownedSlot = FindFirstOwnedSlotByCardId(cardID, owned: false);
		if (unownedSlot == null)
		{
			return false;
		}
		bool wasDeckModified = false;
		for (int unownedCount = unownedSlot.Count; unownedCount > 0; unownedCount--)
		{
			TAG_PREMIUM? premiumToAdd = GetPreferredPremiumThatCanBeAdded(cardID);
			if (!premiumToAdd.HasValue)
			{
				break;
			}
			if (AddCard(cardID, premiumToAdd.Value, false, null))
			{
				wasDeckModified = true;
			}
		}
		return wasDeckModified;
	}

	public void SendChangesIfPending()
	{
		if (m_pendingChangeSource != 0)
		{
			SendChanges(m_pendingChangeSource);
			m_pendingChangeSource = ChangeSource.Unknown;
		}
	}

	public CollectionDeckSlot FindInvalidSlot(FormatType? formatTypeToValidateAgainst = null, bool ignoreOwnership = false)
	{
		return GetSlots().Find((CollectionDeckSlot slot) => !IsValidSlot(slot, ignoreOwnership, ignoreGameplayEvent: false, enforceRemainingDeckRuleset: true, formatTypeToValidateAgainst));
	}

	public List<CollectionDeckSlot> FindInvalidSlots(FormatType? formatTypeToValidateAgainst = null)
	{
		return GetSlots().FindAll((CollectionDeckSlot slot) => !IsValidSlot(slot, ignoreOwnership: false, ignoreGameplayEvent: false, enforceRemainingDeckRuleset: true, formatTypeToValidateAgainst));
	}

	public void RemoveInvalidCards(FormatType? formatTypeToValidateAgainst = null)
	{
		foreach (CollectionDeckSlot slot in FindInvalidSlots(formatTypeToValidateAgainst))
		{
			RemoveSlot(slot);
		}
	}

	public void RemoveExtraCards(FormatType? formatTypeToValidateAgainst = null)
	{
		CardCountByStatus cardCount = CountCardsByStatus(formatTypeToValidateAgainst);
		if (cardCount.Extra <= 0)
		{
			return;
		}
		foreach (CollectionDeckSlot slot2 in FindInvalidSlots(formatTypeToValidateAgainst))
		{
			while (cardCount.Extra > 0 && slot2.Count > 0)
			{
				slot2.RemoveCard(1, slot2.UnPreferredPremium);
				cardCount.Extra--;
			}
		}
		cardCount = CountCardsByStatus(formatTypeToValidateAgainst);
		if (cardCount.Extra <= 0)
		{
			return;
		}
		List<CollectionDeckSlot> validSlots = m_slots.Where((CollectionDeckSlot slot) => !slot.GetEntityDef().HasTag(GAME_TAG.DECK_RULE_MOD_DECK_SIZE)).ToList();
		while (cardCount.Extra > 0 && validSlots.Count > 0)
		{
			int index = UnityEngine.Random.Range(0, validSlots.Count);
			CollectionDeckSlot slot3 = validSlots[index];
			slot3.RemoveCard(1, slot3.UnPreferredPremium);
			if (slot3.Count == 0)
			{
				validSlots.RemoveAt(index);
			}
			cardCount.Extra--;
		}
	}

	public int GetCardIdCount(string cardID, bool includeUnowned = true, bool includeSideboards = true)
	{
		int count = 0;
		foreach (CollectionDeckSlot slot in m_slots)
		{
			if (slot.CardID.Equals(cardID) && (includeUnowned || slot.Owned))
			{
				count += slot.Count;
			}
		}
		if (includeSideboards && HasSideboardCards)
		{
			count += m_sideboardManager.GetCardIdCount(cardID, includeUnowned);
		}
		return count;
	}

	public int GetCardCountHasTag(GAME_TAG tagName, bool includeSideboards = true)
	{
		int count = 0;
		foreach (CollectionDeckSlot slot in m_slots)
		{
			if (GameUtils.GetCardHasTag(slot.CardID, tagName))
			{
				count += slot.Count;
			}
		}
		if (includeSideboards && HasSideboardCards)
		{
			count += m_sideboardManager.GetCardCountHasTag(tagName);
		}
		return count;
	}

	public int GetCardCountAllMatchingSlots(string cardID)
	{
		int count = 0;
		foreach (CollectionDeckSlot slot in m_slots)
		{
			if (slot.CardID.Equals(cardID))
			{
				count += slot.Count;
			}
		}
		return count;
	}

	public int GetCardCountAllMatchingSlots(string cardID, TAG_PREMIUM premium)
	{
		int count = 0;
		foreach (CollectionDeckSlot slot in m_slots)
		{
			if (slot.CardID.Equals(cardID))
			{
				count += slot.GetCount(premium);
			}
		}
		foreach (KeyValuePair<string, SideboardDeck> allSideboard in m_sideboardManager.GetAllSideboards())
		{
			count += allSideboard.Value.GetCardCountAllMatchingSlots(cardID, premium);
		}
		return count;
	}

	public int GetOwnedCardCountInDeck(string cardID, TAG_PREMIUM premium, bool owned = true)
	{
		if (!ShouldSplitSlotsByOwnershipOrFormatValidity())
		{
			return GetCardCountAllMatchingSlots(cardID);
		}
		int count = 0;
		foreach (CollectionDeckSlot slot in m_slots)
		{
			if (slot.CardID.Equals(cardID) && slot.Owned == owned)
			{
				count += slot.GetCount(premium);
			}
		}
		foreach (KeyValuePair<string, SideboardDeck> kvp in m_sideboardManager.GetAllSideboards())
		{
			bool requireOwnership = owned;
			if (kvp.Value is ZilliaxSideboardDeck)
			{
				requireOwnership = false;
			}
			count += kvp.Value.GetOwnedCardCountInDeck(cardID, premium, requireOwnership);
		}
		return count;
	}

	public int GetCardCountInSet(HashSet<string> set, bool isNot)
	{
		int count = 0;
		for (int i = 0; i < m_slots.Count; i++)
		{
			CollectionDeckSlot slot = m_slots[i];
			if (set.Contains(slot.CardID) == !isNot)
			{
				count += slot.Count;
			}
		}
		return count;
	}

	public void ClearSlotContents()
	{
		m_slots.Clear();
	}

	public TAG_PREMIUM? GetPreferredPremiumThatCanBeAdded(string cardId)
	{
		if (CanCardBeAddedAsOwned(cardId, TAG_PREMIUM.DIAMOND))
		{
			return TAG_PREMIUM.DIAMOND;
		}
		if (CanCardBeAddedAsOwned(cardId, TAG_PREMIUM.SIGNATURE))
		{
			return TAG_PREMIUM.SIGNATURE;
		}
		if (CanCardBeAddedAsOwned(cardId, TAG_PREMIUM.GOLDEN))
		{
			return TAG_PREMIUM.GOLDEN;
		}
		if (CanCardBeAddedAsOwned(cardId, TAG_PREMIUM.NORMAL))
		{
			return TAG_PREMIUM.NORMAL;
		}
		return null;
	}

	public bool CanCardBeAddedAsOwned(string cardID, TAG_PREMIUM premium)
	{
		int currentCount = ((!(this is SideboardDeck sideboardDeck)) ? GetOwnedCardCountInDeck(cardID, premium) : sideboardDeck.MainDeck.GetOwnedCardCountInDeck(cardID, premium));
		return CollectionManager.Get().GetOwnedCount(cardID, premium) > currentCount;
	}

	public virtual bool AddCard(string cardID, TAG_PREMIUM premium, bool allowInvalid = false, EntityDef entityDef = null, params DeckRule.RuleType[] ignoreRules)
	{
		bool owned = false;
		if (ShouldSplitSlotsByOwnershipOrFormatValidity())
		{
			owned = CanCardBeAddedAsOwned(cardID, premium);
			if (GameUtils.IsCardCollectible(cardID) && !owned)
			{
				premium = TAG_PREMIUM.NORMAL;
			}
		}
		CollectionDeckSlot unownedSlot = FindFirstOwnedSlotByCardId(cardID, owned: false);
		CollectionDeckSlot targetSlot;
		if (owned)
		{
			targetSlot = FindFirstOwnedSlotByCardId(cardID, owned: true);
			if (unownedSlot != null)
			{
				unownedSlot.RemoveCard(1, unownedSlot.UnPreferredPremium);
				if (unownedSlot.Count == 0)
				{
					RemoveSideboard(cardID);
				}
			}
		}
		else
		{
			targetSlot = unownedSlot;
		}
		if (!allowInvalid)
		{
			EntityDef entityDefToAdd = entityDef ?? DefLoader.Get().GetEntityDef(cardID);
			if (!CanAddCard(entityDefToAdd, premium, ignoreRules))
			{
				return false;
			}
		}
		bool wasAdded;
		if (targetSlot == null)
		{
			targetSlot = InsertSlotWithCard(cardID, premium, owned, 1);
			wasAdded = targetSlot != null;
		}
		else
		{
			targetSlot.AddCard(1, premium);
			wasAdded = true;
		}
		if (wasAdded)
		{
			EntityDef entityDefToAdd2 = targetSlot.GetEntityDef();
			if (!(this is SideboardDeck))
			{
				UpdateDeckRunes(entityDefToAdd2);
				if (entityDefToAdd2.HasSideboard)
				{
					m_sideboardManager.GetOrCreateSideboard(entityDefToAdd2.GetCardId(), premium, setEdited: false);
				}
			}
		}
		return wasAdded;
	}

	private void UpdateDeckRunes(EntityBase entity)
	{
		if (entity == null)
		{
			return;
		}
		RuneType[] runeOrder = Runes.CombineRunes(entity.GetRuneCost(), DeckRule_DeathKnightRuneLimit.MaxRuneSlots).ToArray();
		int addedRuneIndex = 0;
		for (int i = 0; i < m_runeOrder.Length; i++)
		{
			if (m_runeOrder[i] == RuneType.RT_NONE && addedRuneIndex < runeOrder.Length)
			{
				SetRuneAtIndex(i, runeOrder[addedRuneIndex]);
				addedRuneIndex++;
			}
		}
	}

	public CollectionDeckSlot InsertSlotWithCard(string cardID, TAG_PREMIUM premium, bool owned, int count)
	{
		CollectionDeckSlot slot = new CollectionDeckSlot
		{
			CardID = cardID,
			Owned = owned
		};
		slot.SetCount(count, premium);
		int slotIndex = GetInsertionIdxByDefaultSort(slot);
		if (InsertSlot(slotIndex, slot))
		{
			return slot;
		}
		return null;
	}

	public bool AddCard_DungeonCrawlBuff(string cardId, TAG_PREMIUM premium, List<int> enchantments)
	{
		CollectionDeckSlot slot = InsertSlotWithCard(cardId, premium, owned: true, 1);
		if (slot == null)
		{
			return false;
		}
		slot.CreateEntityDefOverride();
		foreach (int enchantmentCardId in enchantments)
		{
			EntityDef entityDef = DefLoader.Get().GetEntityDef(enchantmentCardId);
			int attackUp = entityDef.GetTag(GAME_TAG.UI_BUFF_ATK_UP);
			if (attackUp != 0)
			{
				int attack = slot.m_entityDefOverride.GetTag(GAME_TAG.ATK);
				slot.m_entityDefOverride.SetTag(GAME_TAG.ATK, attack + attackUp);
			}
			int healthUp = entityDef.GetTag(GAME_TAG.UI_BUFF_HEALTH_UP);
			if (healthUp != 0)
			{
				int health = slot.m_entityDefOverride.GetTag(GAME_TAG.HEALTH);
				slot.m_entityDefOverride.SetTag(GAME_TAG.HEALTH, health + healthUp);
			}
			int durabilityUp = entityDef.GetTag(GAME_TAG.UI_BUFF_DURABILITY_UP);
			if (durabilityUp != 0)
			{
				int durability = slot.m_entityDefOverride.GetTag(GAME_TAG.DURABILITY);
				slot.m_entityDefOverride.SetTag(GAME_TAG.DURABILITY, durability + durabilityUp);
			}
			int costUp = entityDef.GetTag(GAME_TAG.UI_BUFF_COST_UP);
			if (costUp != 0)
			{
				int cost = slot.m_entityDefOverride.GetTag(GAME_TAG.COST);
				slot.m_entityDefOverride.SetTag(GAME_TAG.COST, cost + costUp);
			}
			int costDown = entityDef.GetTag(GAME_TAG.UI_BUFF_COST_DOWN);
			if (costDown != 0)
			{
				int cost2 = slot.m_entityDefOverride.GetTag(GAME_TAG.COST);
				slot.m_entityDefOverride.SetTag(GAME_TAG.COST, Math.Max(cost2 - costDown, 0));
			}
			if (entityDef.GetTag(GAME_TAG.UI_BUFF_SET_COST_ZERO) != 0)
			{
				slot.m_entityDefOverride.SetTag(GAME_TAG.COST, 0);
			}
		}
		return true;
	}

	public virtual bool RemoveCard(string cardID, TAG_PREMIUM premium, bool valid, bool enforceRemainingDeckRuleset)
	{
		CollectionDeckSlot slot = FindFirstSlotByCardIdAndValidity(cardID, valid, ignoreGameplayEvent: false, enforceRemainingDeckRuleset);
		if (slot == null)
		{
			return false;
		}
		slot.RemoveCard(1, premium);
		ReconcileOwnershipOnAdd(cardID);
		UpdateUIHeroOverrideCardRemoval(cardID);
		return true;
	}

	public void RemoveAllCards()
	{
		m_slots = new List<CollectionDeckSlot>();
	}

	private void UpdateUIHeroOverrideCardRemoval(string cardID)
	{
		if (GameDbf.GetIndex().HasCardPlayerDeckOverride(cardID) && (CollectionDeckTray.Get() == null || !CollectionDeckTray.Get().IsShowingDeckContents()))
		{
			ClearUIHeroOverride();
			CollectionManager.Get().OnUIHeroOverrideCardRemoved();
		}
	}

	public void ClearUIHeroOverride()
	{
		UIHeroOverrideCardID = string.Empty;
		UIHeroOverridePremium = TAG_PREMIUM.NORMAL;
		Name = GameStrings.Format("GLOBAL_BASIC_DECK_NAME", GameStrings.GetClassName(GetClass()));
		m_currentDisplayHeroCardId = HeroCardID;
	}

	public void OnContentChangesComplete()
	{
		m_isSavingContentChanges = false;
	}

	public void OnNameChangeComplete()
	{
		m_isSavingNameChanges = false;
	}

	public void SendChanges(ChangeSource changeSource)
	{
		CollectionDeck baseDeck = CollectionManager.Get().GetBaseDeck(ID);
		if (this == baseDeck)
		{
			Debug.LogError($"CollectionDeck.Send() - {baseDeck} is a base deck. You cannot send a base deck to the network.");
			return;
		}
		if (baseDeck == null)
		{
			Log.CollectionManager.PrintError("CollectionDeck.SendChanges() - No base deck with id=" + ID);
			return;
		}
		List<Network.CardUserData> contentChanges = GenerateContentChanges(baseDeck, this);
		List<Network.SideboardCardUserData> sideboardChanges = GenerateSideboardContentChanges(baseDeck, this);
		int heroAssetIDChange;
		bool? heroOverrideStatusChange;
		int uiHeroOverrideAssetIDChange;
		TAG_PREMIUM uiHeroOverridePremiumChange;
		bool hasHeroChange = GenerateHeroDiff(baseDeck, out heroAssetIDChange, out heroOverrideStatusChange, out uiHeroOverrideAssetIDChange, out uiHeroOverridePremiumChange);
		int? cosmeticCoinChange;
		bool hasCosmeticCoinChange = GenerateCosmeticCoinDiff(baseDeck, out cosmeticCoinChange);
		bool? randomCoinChange = null;
		bool hasRandomCoinChange = baseDeck.RandomCoinUseFavorite != RandomCoinUseFavorite;
		if (hasRandomCoinChange)
		{
			randomCoinChange = RandomCoinUseFavorite;
		}
		int? cardBackChange;
		bool hasCardBackChange = GenerateCardBackDiff(baseDeck, out cardBackChange);
		bool hasFormatChange = baseDeck.FormatType != FormatType;
		bool hasSortOrderChange = baseDeck.SortOrder != SortOrder;
		RuneType[] baseDeckRuneOrder = baseDeck.GetRuneOrder();
		bool hasRuneOrderChange = !IsRuneOrderEqual(baseDeckRuneOrder);
		bool? randomHeroChange = null;
		bool hasRandomHeroChange = baseDeck.RandomHeroUseFavorite != RandomHeroUseFavorite;
		if (hasRandomHeroChange)
		{
			randomHeroChange = RandomHeroUseFavorite;
		}
		Network.Get();
		SendDeckRenameChange(baseDeck, shouldValidateDeckName: false, Type, SourceType);
		string pastedDeckString = null;
		if (m_createdFromShareableDeck != null)
		{
			pastedDeckString = m_createdFromShareableDeck.Serialize(includeComments: false);
		}
		if (contentChanges.Count > 0 || sideboardChanges.Count > 0 || hasHeroChange || hasRandomHeroChange || hasCosmeticCoinChange || hasRandomCoinChange || hasCardBackChange || hasFormatChange || hasSortOrderChange || hasRuneOrderChange)
		{
			m_isSavingContentChanges = true;
			m_changeNumber++;
			Network.Get().SendDeckData(changeSource, m_changeNumber, ID, contentChanges, sideboardChanges, heroAssetIDChange, heroOverrideStatusChange, uiHeroOverrideAssetIDChange, uiHeroOverridePremiumChange, cosmeticCoinChange, randomCoinChange, cardBackChange, FormatType, SortOrder, randomHeroChange, m_runeOrder, pastedDeckString);
		}
		if (!Network.IsLoggedIn())
		{
			OnContentChangesComplete();
			OnNameChangeComplete();
		}
	}

	public void SendDeckRenameChange(CollectionDeck baseDeck, bool shouldValidateDeckName, DeckType deckType, DeckSourceType sourceType)
	{
		if (baseDeck == null)
		{
			baseDeck = CollectionManager.Get().GetBaseDeck(ID);
			if (this == baseDeck)
			{
				Debug.LogError($"CollectionDeck.CollectionDeck baseDeck() - {baseDeck} is a base deck. You cannot send a base deck to the network.");
				return;
			}
			if (baseDeck == null)
			{
				return;
			}
		}
		GenerateNameDiff(baseDeck, out var deckName);
		if (deckName != null)
		{
			m_isSavingNameChanges = true;
			Network.Get().RenameDeck(ID, deckName, shouldValidateDeckName, baseDeck.Type, baseDeck.SourceType);
		}
	}

	public Dictionary<string, SideboardDeck> GetAllSideboards()
	{
		return m_sideboardManager.GetAllSideboards();
	}

	public static string GetUserFriendlyCopyErrorMessageFromDeckRuleViolation(DeckRuleViolation violation)
	{
		if (violation == null || violation.Rule == null)
		{
			return string.Empty;
		}
		switch (violation.Rule.Type)
		{
		case DeckRule.RuleType.PLAYER_OWNS_EACH_COPY:
		case DeckRule.RuleType.DECK_SIZE:
			return GameStrings.Get("GLUE_COLLECTION_DECK_COPY_TOOLTIP_INCOMPLETE");
		case DeckRule.RuleType.IS_IN_ANY_SUBSET:
		case DeckRule.RuleType.IS_NOT_ROTATED:
			return GameStrings.Get("GLUE_COLLECTION_DECK_COPY_TOOLTIP_FORMAT");
		case DeckRule.RuleType.IS_CARD_PLAYABLE:
			return GameStrings.Get("GLUE_COLLECTION_DECK_COPY_TOOLTIP_UNPLAYABLE");
		default:
			return violation.DisplayError;
		}
	}

	public void SetShareableDeckCreatedFrom(ShareableDeck shareableDeck)
	{
		m_createdFromShareableDeck = shareableDeck;
	}

	public bool CanAddCard(EntityDef entityDef, TAG_PREMIUM premium, params DeckRule.RuleType[] ignoreRules)
	{
		return CanAddCard(entityDef, premium, this, ignoreRules);
	}

	public bool CanAddCard(EntityDef entityDef, TAG_PREMIUM premium, CollectionDeck validationDeck, params DeckRule.RuleType[] ignoreRules)
	{
		if (entityDef == null)
		{
			return false;
		}
		if (DeckType.DRAFT_DECK == Type || DeckType.CLIENT_ONLY_DECK == Type)
		{
			return true;
		}
		DeckRuleset deckRuleset = CollectionManager.Get().GetDeckRuleset();
		if (deckRuleset == null)
		{
			return true;
		}
		List<DeckRule.RuleType> ignoreList = new List<DeckRule.RuleType>(ignoreRules);
		ignoreList.AddRange(DefaultIgnoreRules);
		RuleInvalidReason reason;
		DeckRule brokenRule;
		return deckRuleset.CanAddToDeck(entityDef, premium, validationDeck, out reason, out brokenRule, ignoreList.ToArray());
	}

	private bool InsertSlot(int slotIndex, CollectionDeckSlot slot)
	{
		if (slotIndex < 0 || slotIndex > GetSlotCount())
		{
			return false;
		}
		slot.OnSlotEmptied = (CollectionDeckSlot.DelOnSlotEmptied)Delegate.Combine(slot.OnSlotEmptied, new CollectionDeckSlot.DelOnSlotEmptied(OnSlotEmptied));
		slot.Index = slotIndex;
		m_slots.Insert(slotIndex, slot);
		UpdateSlotIndices(slotIndex, GetSlotCount() - 1);
		return true;
	}

	private void RemoveSlot(CollectionDeckSlot slot)
	{
		slot.OnSlotEmptied = (CollectionDeckSlot.DelOnSlotEmptied)Delegate.Remove(slot.OnSlotEmptied, new CollectionDeckSlot.DelOnSlotEmptied(OnSlotEmptied));
		int sourceSlotIndex = slot.Index;
		m_slots.RemoveAt(sourceSlotIndex);
		slot.m_entityDefOverride = null;
		UpdateSlotIndices(sourceSlotIndex, GetSlotCount() - 1);
		UpdateUIHeroOverrideCardRemoval(slot.CardID);
	}

	private void OnSlotEmptied(CollectionDeckSlot slot)
	{
		if (GetExistingSlot(slot) == null)
		{
			Log.Decks.Print($"CollectionDeck.OnSlotCountUpdated(): Trying to remove slot {slot}, but it does not exist in deck {this}");
		}
		else
		{
			RemoveSlot(slot);
		}
	}

	public void ForceUpdateSlotPosition(CollectionDeckSlot slotToUpdate)
	{
		if (slotToUpdate != null)
		{
			int originalSlotIndex = slotToUpdate.Index;
			slotToUpdate.OnSlotEmptied = (CollectionDeckSlot.DelOnSlotEmptied)Delegate.Remove(slotToUpdate.OnSlotEmptied, new CollectionDeckSlot.DelOnSlotEmptied(OnSlotEmptied));
			m_slots.RemoveAt(originalSlotIndex);
			UpdateSlotIndices(originalSlotIndex, GetSlotCount() - 1);
			int newSlotIndex = GetInsertionIdxByDefaultSort(slotToUpdate);
			InsertSlot(newSlotIndex, slotToUpdate);
		}
	}

	private void UpdateSlotIndices(int indexA, int indexB)
	{
		if (GetSlotCount() != 0)
		{
			int startIndex;
			int endIndex;
			if (indexA < indexB)
			{
				startIndex = indexA;
				endIndex = indexB;
			}
			else
			{
				startIndex = indexB;
				endIndex = indexA;
			}
			startIndex = Math.Max(0, startIndex);
			endIndex = Math.Min(endIndex, GetSlotCount() - 1);
			for (int i = startIndex; i <= endIndex; i++)
			{
				GetSlotByIndex(i).Index = i;
			}
		}
	}

	public CollectionDeckSlot FindFirstSlotByCardId(string cardID)
	{
		return m_slots.Find((CollectionDeckSlot slot) => slot.CardID.Equals(cardID));
	}

	public CollectionDeckSlot FindFirstOwnedSlotByCardId(string cardID, bool owned)
	{
		if (!ShouldSplitSlotsByOwnershipOrFormatValidity())
		{
			return FindFirstSlotByCardId(cardID);
		}
		return m_slots.Find((CollectionDeckSlot slot) => slot.CardID.Equals(cardID) && slot.Owned == owned);
	}

	public CollectionDeckSlot FindFirstSlotByCardIdAndValidity(string cardID, bool valid, bool ignoreGameplayEvent, bool enforceRemainingDeckRuleset)
	{
		if (!ShouldSplitSlotsByOwnershipOrFormatValidity())
		{
			Log.Decks.PrintWarning("Your deck doesn't care about Validity.  Why are you using 'FindFirstValidSlot' as opposed to 'FindFirstOwnedSlot'? This may be a bug!");
			return FindFirstSlotByCardId(cardID);
		}
		return m_slots.Find((CollectionDeckSlot slot) => slot.CardID == cardID && valid == IsValidSlot(slot, ignoreOwnership: false, ignoreGameplayEvent, enforceRemainingDeckRuleset, null));
	}

	private void GenerateNameDiff(CollectionDeck baseDeck, out string deckName)
	{
		deckName = null;
		if (!Name.Equals(baseDeck.Name))
		{
			deckName = Name;
		}
	}

	private bool GenerateHeroDiff(CollectionDeck baseDeck, out int heroAssetID, out bool? heroOverrideStatus, out int overrideHeroAssetID, out TAG_PREMIUM overrideHeroPremium)
	{
		heroAssetID = -1;
		overrideHeroAssetID = -1;
		overrideHeroPremium = TAG_PREMIUM.NORMAL;
		heroOverrideStatus = HeroOverridden;
		bool changed = false;
		if (HeroOverridden != baseDeck.HeroOverridden)
		{
			changed = true;
		}
		bool heroMatchesBaseDeck = HeroCardID == baseDeck.HeroCardID;
		if (HeroOverridden && !heroMatchesBaseDeck)
		{
			heroAssetID = GameUtils.TranslateCardIdToDbId(HeroCardID);
			changed = true;
		}
		if (!(UIHeroOverrideCardID == baseDeck.UIHeroOverrideCardID) || UIHeroOverridePremium != baseDeck.UIHeroOverridePremium)
		{
			overrideHeroAssetID = ((!string.IsNullOrEmpty(UIHeroOverrideCardID)) ? GameUtils.TranslateCardIdToDbId(UIHeroOverrideCardID) : 0);
			overrideHeroPremium = UIHeroOverridePremium;
			changed = true;
		}
		return changed;
	}

	private bool GenerateCosmeticCoinDiff(CollectionDeck baseDeck, out int? cosmeticCoinId)
	{
		cosmeticCoinId = -1;
		if (CosmeticCoinID == baseDeck.CosmeticCoinID)
		{
			return false;
		}
		cosmeticCoinId = CosmeticCoinID;
		return true;
	}

	private bool GenerateCardBackDiff(CollectionDeck baseDeck, out int? cardBackID)
	{
		cardBackID = -1;
		if (CardBackID == baseDeck.CardBackID)
		{
			return false;
		}
		cardBackID = CardBackID;
		return true;
	}

	private static List<Network.CardUserData> GetCardUserDataFromSlot(CollectionDeckSlot deckSlot, bool deleted)
	{
		List<Network.CardUserData> list = new List<Network.CardUserData>();
		Network.CardUserData normalUserData = new Network.CardUserData
		{
			DbId = GameUtils.TranslateCardIdToDbId(deckSlot.CardID),
			Count = ((!deleted) ? deckSlot.GetCount(TAG_PREMIUM.NORMAL) : 0),
			Premium = TAG_PREMIUM.NORMAL
		};
		Network.CardUserData goldenUserData = new Network.CardUserData
		{
			DbId = normalUserData.DbId,
			Count = ((!deleted) ? deckSlot.GetCount(TAG_PREMIUM.GOLDEN) : 0),
			Premium = TAG_PREMIUM.GOLDEN
		};
		Network.CardUserData signatureUserData = new Network.CardUserData
		{
			DbId = normalUserData.DbId,
			Count = ((!deleted) ? deckSlot.GetCount(TAG_PREMIUM.SIGNATURE) : 0),
			Premium = TAG_PREMIUM.SIGNATURE
		};
		Network.CardUserData diamondUserData = new Network.CardUserData
		{
			DbId = normalUserData.DbId,
			Count = ((!deleted) ? deckSlot.GetCount(TAG_PREMIUM.DIAMOND) : 0),
			Premium = TAG_PREMIUM.DIAMOND
		};
		list.Add(normalUserData);
		list.Add(goldenUserData);
		list.Add(signatureUserData);
		list.Add(diamondUserData);
		return list;
	}

	private static List<Network.SideboardCardUserData> GetSideboardCardUserDataFromSlot(int linkedCardDbId, CollectionDeckSlot deckSlot, bool deleted)
	{
		List<Network.SideboardCardUserData> result = new List<Network.SideboardCardUserData>();
		foreach (Network.CardUserData cardData in GetCardUserDataFromSlot(deckSlot, deleted))
		{
			result.Add(new Network.SideboardCardUserData
			{
				LinkedCardDbId = linkedCardDbId,
				Card = cardData
			});
		}
		return result;
	}

	private static List<Network.CardUserData> GenerateContentChanges(CollectionDeck originalDeck, CollectionDeck editedDeck)
	{
		SortedDictionary<string, CollectionDeckSlot> bDeck = new SortedDictionary<string, CollectionDeckSlot>();
		foreach (CollectionDeckSlot b in originalDeck.GetSlots())
		{
			CollectionDeckSlot newSlot = null;
			if (bDeck.TryGetValue(b.CardID, out newSlot))
			{
				foreach (TAG_PREMIUM premium in Enum.GetValues(typeof(TAG_PREMIUM)))
				{
					newSlot.AddCard(b.GetCount(premium), premium);
				}
			}
			else
			{
				newSlot = new CollectionDeckSlot();
				newSlot.CopyFrom(b);
				bDeck.Add(newSlot.CardID, newSlot);
			}
		}
		SortedDictionary<string, CollectionDeckSlot> aDeck = new SortedDictionary<string, CollectionDeckSlot>();
		foreach (CollectionDeckSlot a in editedDeck.GetSlots())
		{
			CollectionDeckSlot newSlot2 = null;
			if (aDeck.TryGetValue(a.CardID, out newSlot2))
			{
				foreach (TAG_PREMIUM premium2 in Enum.GetValues(typeof(TAG_PREMIUM)))
				{
					newSlot2.AddCard(a.GetCount(premium2), premium2);
				}
			}
			else
			{
				newSlot2 = new CollectionDeckSlot();
				newSlot2.CopyFrom(a);
				aDeck.Add(newSlot2.CardID, newSlot2);
			}
		}
		SortedDictionary<string, CollectionDeckSlot>.Enumerator bDeckSlotIt = bDeck.GetEnumerator();
		SortedDictionary<string, CollectionDeckSlot>.Enumerator aDeckSlotIt = aDeck.GetEnumerator();
		List<Network.CardUserData> contentChanges = new List<Network.CardUserData>();
		bool bSlotReady = bDeckSlotIt.MoveNext();
		bool aSlotReady = aDeckSlotIt.MoveNext();
		while (bSlotReady && aSlotReady)
		{
			CollectionDeckSlot beforeCard = bDeckSlotIt.Current.Value;
			CollectionDeckSlot afterCard = aDeckSlotIt.Current.Value;
			if (beforeCard.CardID == afterCard.CardID)
			{
				if (beforeCard.GetCount(TAG_PREMIUM.NORMAL) != afterCard.GetCount(TAG_PREMIUM.NORMAL) || beforeCard.GetCount(TAG_PREMIUM.GOLDEN) != afterCard.GetCount(TAG_PREMIUM.GOLDEN) || beforeCard.GetCount(TAG_PREMIUM.SIGNATURE) != afterCard.GetCount(TAG_PREMIUM.SIGNATURE) || beforeCard.GetCount(TAG_PREMIUM.DIAMOND) != afterCard.GetCount(TAG_PREMIUM.DIAMOND) || beforeCard.Owned != afterCard.Owned)
				{
					contentChanges.AddRange(GetCardUserDataFromSlot(afterCard, afterCard.Count == 0));
				}
				bSlotReady = bDeckSlotIt.MoveNext();
				aSlotReady = aDeckSlotIt.MoveNext();
			}
			else if (beforeCard.CardID.CompareTo(afterCard.CardID) < 0)
			{
				contentChanges.AddRange(GetCardUserDataFromSlot(beforeCard, deleted: true));
				bSlotReady = bDeckSlotIt.MoveNext();
			}
			else
			{
				contentChanges.AddRange(GetCardUserDataFromSlot(afterCard, deleted: false));
				aSlotReady = aDeckSlotIt.MoveNext();
			}
		}
		while (bSlotReady)
		{
			CollectionDeckSlot beforeCard2 = bDeckSlotIt.Current.Value;
			contentChanges.AddRange(GetCardUserDataFromSlot(beforeCard2, deleted: true));
			bSlotReady = bDeckSlotIt.MoveNext();
		}
		while (aSlotReady)
		{
			CollectionDeckSlot afterCard2 = aDeckSlotIt.Current.Value;
			contentChanges.AddRange(GetCardUserDataFromSlot(afterCard2, deleted: false));
			aSlotReady = aDeckSlotIt.MoveNext();
		}
		return contentChanges;
	}

	private static List<Network.SideboardCardUserData> GenerateSideboardContentChanges(CollectionDeck originalDeck, CollectionDeck editedDeck)
	{
		List<Network.SideboardCardUserData> result = new List<Network.SideboardCardUserData>();
		if (originalDeck == null || editedDeck == null)
		{
			return result;
		}
		Dictionary<string, SideboardDeck> originalDeckSideboards = originalDeck.GetAllSideboards();
		Dictionary<string, SideboardDeck> editedDeckSideboards = editedDeck.GetAllSideboards();
		if (editedDeckSideboards.Count > 0)
		{
			foreach (KeyValuePair<string, SideboardDeck> kvp in editedDeckSideboards)
			{
				SideboardDeck editedSideboard = kvp.Value;
				if (originalDeckSideboards.TryGetValue(kvp.Key, out var baseDeckSideboard))
				{
					foreach (Network.CardUserData cardUserData in GenerateContentChanges(baseDeckSideboard, editedSideboard))
					{
						result.Add(new Network.SideboardCardUserData
						{
							Card = cardUserData,
							LinkedCardDbId = editedSideboard.OwnerCardDbId
						});
					}
					continue;
				}
				foreach (CollectionDeckSlot slot2 in editedSideboard.GetSlots())
				{
					foreach (Network.CardUserData cardUserData2 in GetCardUserDataFromSlot(slot2, deleted: false))
					{
						result.Add(new Network.SideboardCardUserData
						{
							Card = cardUserData2,
							LinkedCardDbId = editedSideboard.OwnerCardDbId
						});
					}
				}
			}
		}
		if (originalDeckSideboards.Count > 0)
		{
			foreach (KeyValuePair<string, SideboardDeck> kvp2 in originalDeckSideboards)
			{
				if (editedDeck.GetCardIdCount(kvp2.Key, includeUnowned: true, includeSideboards: false) != 0)
				{
					continue;
				}
				foreach (CollectionDeckSlot slot in kvp2.Value.GetSlots())
				{
					result.AddRange(GetSideboardCardUserDataFromSlot(kvp2.Value.OwnerCardDbId, slot, deleted: true));
				}
			}
		}
		return result;
	}

	private int GetInsertionIdxByDefaultSort(CollectionDeckSlot slot)
	{
		EntityDef entityDef = slot.GetEntityDef();
		if (entityDef == null)
		{
			Log.Decks.Print($"CollectionDeck.GetInsertionIdxByDefaultSort(): could not get entity def for {slot.CardID}");
			return -1;
		}
		int slotIndex;
		for (slotIndex = 0; slotIndex < GetSlotCount(); slotIndex++)
		{
			CollectionDeckSlot otherSlot = GetSlotByIndex(slotIndex);
			EntityDef otherEntityDef = otherSlot.GetEntityDef();
			if (otherEntityDef == null)
			{
				Log.Decks.Print($"CollectionDeck.GetInsertionIdxByDefaultSort(): entityDef is null at slot index {slotIndex}");
				break;
			}
			int entityDefComparison = CollectionManager.EntityDefSortComparison(entityDef, otherEntityDef);
			if (entityDefComparison < 0 || (entityDefComparison <= 0 && (!ShouldSplitSlotsByOwnershipOrFormatValidity() || slot.Owned == otherSlot.Owned)))
			{
				break;
			}
		}
		return slotIndex;
	}

	public TAG_CLASS GetClass()
	{
		return DefLoader.Get().GetEntityDef(HeroCardID)?.GetClass() ?? TAG_CLASS.INVALID;
	}

	public List<TAG_CLASS> GetHeroClasses()
	{
		EntityDef heroDef = DefLoader.Get().GetEntityDef(HeroCardID);
		List<TAG_CLASS> classes = new List<TAG_CLASS>();
		if (heroDef == null)
		{
			classes.Clear();
			classes.Add(TAG_CLASS.INVALID);
			return classes;
		}
		heroDef.GetClasses(classes);
		return classes;
	}

	public virtual List<TAG_CLASS> GetClasses()
	{
		List<TAG_CLASS> classes = new List<TAG_CLASS>();
		List<TAG_CLASS> heroClasses = GetHeroClasses();
		classes.AddRange(heroClasses);
		foreach (CollectionDeckSlot slot in m_slots)
		{
			EntityDef entityDef = slot.GetEntityDef();
			if (entityDef != null && entityDef.HasTag(GAME_TAG.TOURIST))
			{
				TAG_CLASS touristClass = entityDef.GetClass();
				if (heroClasses.Contains(touristClass))
				{
					TAG_CLASS classId = (TAG_CLASS)entityDef.GetTag(GAME_TAG.TOURIST);
					classes.Add(classId);
				}
			}
		}
		return classes;
	}

	public virtual List<TAG_CLASS> GetTouristClasses()
	{
		List<TAG_CLASS> classes = new List<TAG_CLASS>();
		List<TAG_CLASS> heroClasses = GetHeroClasses();
		foreach (CollectionDeckSlot slot in m_slots)
		{
			EntityDef entityDef = slot.GetEntityDef();
			if (entityDef != null && entityDef.HasTag(GAME_TAG.TOURIST))
			{
				TAG_CLASS touristClass = entityDef.GetClass();
				if (heroClasses.Contains(touristClass))
				{
					TAG_CLASS classId = (TAG_CLASS)entityDef.GetTag(GAME_TAG.TOURIST);
					classes.Add(classId);
				}
			}
		}
		return classes;
	}

	public bool HasClass(TAG_CLASS tagClass)
	{
		foreach (TAG_CLASS @class in GetClasses())
		{
			if (@class == tagClass)
			{
				return true;
			}
		}
		return false;
	}

	public ShareableDeck GetShareableDeck()
	{
		DeckContents deckContents = GetDeckContents();
		int heroCardDbId = GameUtils.TranslateCardIdToDbId(HeroCardID);
		return new ShareableDeck(Name, heroCardDbId, deckContents, FormatType, Type == DeckType.DRAFT_DECK);
	}

	public bool CanCopyAsShareableDeck(out DeckRuleViolation topViolation)
	{
		topViolation = null;
		if (GetRuleset(null) != null)
		{
			if (!GetRuleset(null).IsDeckValid(this, out var violations) && violations != null && violations.Count > 0)
			{
				topViolation = violations[0];
				return false;
			}
			return true;
		}
		return false;
	}

	public void LogDeckStringInformation()
	{
		Log.Decks.PrintInfo(string.Format("{0} {1}", "###", Name));
		Log.Decks.PrintInfo(string.Format("{0}Deck ID: {1}", "# ", ID));
		Log.Decks.PrintInfo(GetShareableDeck().Serialize(includeComments: false));
	}

	public DeckContents GetDeckContents()
	{
		DeckContents deckContents = new DeckContents
		{
			DeckId = ID
		};
		foreach (CollectionDeckSlot slot in m_slots)
		{
			DeckCardData deckCardData = new DeckCardData
			{
				Def = new PegasusShared.CardDef
				{
					Asset = GameUtils.TranslateCardIdToDbId(slot.CardID),
					Premium = (int)slot.PreferredPremium
				},
				Qty = slot.Count
			};
			deckContents.Cards.Add(deckCardData);
			SideboardDeck sideBoard = m_sideboardManager.GetSideboard(slot.CardID);
			if (sideBoard == null)
			{
				continue;
			}
			foreach (CollectionDeckSlot sideBoardSlot in sideBoard.m_slots)
			{
				SideBoardCardData sideBoardCardData = new SideBoardCardData
				{
					Def = new PegasusShared.CardDef
					{
						Asset = GameUtils.TranslateCardIdToDbId(sideBoardSlot.CardID),
						Premium = (int)sideBoardSlot.PreferredPremium
					},
					Qty = sideBoardSlot.Count
				};
				sideBoardCardData.LinkedCardDbId = GameUtils.TranslateCardIdToDbId(slot.CardID);
				deckContents.SideboardCards.Add(sideBoardCardData);
			}
		}
		return deckContents;
	}

	public bool ShouldSplitSlotsByOwnershipOrFormatValidity()
	{
		if (Locked && !IsBrawlDeck)
		{
			return false;
		}
		switch (Type)
		{
		case DeckType.CLIENT_ONLY_DECK:
		case DeckType.DRAFT_DECK:
			return false;
		case DeckType.TAVERN_BRAWL_DECK:
		case DeckType.FSG_BRAWL_DECK:
			if (TavernBrawlManager.Get().IsCurrentBrawlTypeActive && TavernBrawlManager.Get().GetCurrentDeckRuleset() != null && TavernBrawlManager.Get().GetCurrentDeckRuleset().HasOwnershipOrRotatedRule())
			{
				return true;
			}
			return false;
		default:
			return true;
		}
	}

	public bool CanAddRunes(RunePattern runesToAdd, int maxRuneSlots)
	{
		return Runes.CanAddRunes(runesToAdd, maxRuneSlots);
	}

	public bool ContainsDeathKnightRuneCards()
	{
		RunePattern cardRuneSlots = default(RunePattern);
		foreach (CollectionDeckSlot slot in m_slots)
		{
			EntityDef entityDef = DefLoader.Get().GetEntityDef(slot.CardID);
			cardRuneSlots.SetCostsFromEntity(entityDef);
			if (cardRuneSlots.HasRunes)
			{
				return true;
			}
		}
		return false;
	}

	public void AddSlots(List<CollectionDeckSlot> slots)
	{
		if (slots != null)
		{
			m_slots.AddRange(slots);
		}
	}

	public SideboardDeck GetCurrentSideboardDeck()
	{
		return m_sideboardManager.GetCurrentSideboardDeck();
	}

	public bool IsEditingSideboardFor(string sideboardOwnerId)
	{
		SideboardDeck currentEditedSideboard = GetCurrentSideboardDeck();
		if (currentEditedSideboard != null)
		{
			return currentEditedSideboard.DataModel.OwnerCardId == sideboardOwnerId;
		}
		return false;
	}

	public SideboardDeck SetEditedSideboard(string ownerId, TAG_PREMIUM premium)
	{
		return m_sideboardManager.SetEditedSideboard(ownerId, premium);
	}

	public void ClearEditedSideboard()
	{
		m_sideboardManager.ClearEditedSideboard();
	}

	public void ClearSideboards()
	{
		m_sideboardManager.ClearSideboards();
	}

	public bool HasSideboard(string ownerCardId)
	{
		return m_sideboardManager.GetSideboard(ownerCardId) != null;
	}

	public void RemoveSideboard(string ownerCardId)
	{
		m_sideboardManager.RemoveSideboard(ownerCardId);
	}

	public void AddCardToSideboard(string cardId, int ownerCardDbId, TAG_PREMIUM cardPremium, bool allowInvalid)
	{
		m_sideboardManager.AddCard(cardId, ownerCardDbId, cardPremium, allowInvalid);
	}

	public void AddCardToSideboardPreferredPremium(string cardId, int ownerCardDbId, TAG_PREMIUM cardPremium, bool allowInvalid)
	{
		m_sideboardManager.AddCardWithPrefferedPremium(cardId, ownerCardDbId, cardPremium, allowInvalid);
	}

	public virtual void AddCardsFrom(CollectionDeck other)
	{
		if (other == null)
		{
			return;
		}
		foreach (CollectionDeckSlot otherSlot in other.GetSlots())
		{
			foreach (TAG_PREMIUM p in Enum.GetValues(typeof(TAG_PREMIUM)))
			{
				for (int i = 0; i < otherSlot.GetCount(p); i++)
				{
					AddCard(otherSlot.CardID, p, false, null);
				}
			}
		}
	}

	public List<SideboardDeck> GetIncompleteSideboards()
	{
		return m_sideboardManager.GetIncompleteSideboards();
	}

	public SideboardDeck GetSideboard(string ownerId)
	{
		return m_sideboardManager.GetSideboard(ownerId);
	}

	public SideboardDeck GetOrCreateSideboard(string ownerId, TAG_PREMIUM premium)
	{
		return m_sideboardManager.GetOrCreateSideboard(ownerId, premium, setEdited: false);
	}

	public void RemoveOrphanedSideboards()
	{
		foreach (string sideboardOwnerCard in m_sideboardManager.GetAllSideboards().Keys.ToList())
		{
			if (GetCardIdCount(sideboardOwnerCard, includeUnowned: true, includeSideboards: false) == 0)
			{
				m_sideboardManager.RemoveSideboard(sideboardOwnerCard);
			}
		}
	}

	public void ClearRemovedSideboards()
	{
		m_sideboardManager.ClearRemovedSideboards();
	}

	public bool ProcessForDynamicallySortingSlot(CollectionDeckSlot slot)
	{
		if (GetSideboard(slot.CardID) is ZilliaxSideboardDeck zilliaxSideboardDeck)
		{
			slot.m_entityDefOverride = zilliaxSideboardDeck.DynamicZilliaxDef;
			return true;
		}
		return false;
	}

	public static CollectionDeck Create(DeckTemplateDbfRecord templateRecord, DeckTemplate.SourceType sourceType)
	{
		float start = Time.realtimeSinceStartup;
		int deckId = templateRecord.DeckId;
		DeckDbfRecord deckRecord = GameDbf.Deck.GetRecord(deckId);
		if (deckRecord == null)
		{
			Debug.LogError($"Unable to find deck with ID {deckId}");
			return null;
		}
		TAG_CLASS classType = (TAG_CLASS)templateRecord.ClassId;
		string heroCardID = GameUtils.TranslateDbIdToCardId(templateRecord.HeroCardId);
		if (string.IsNullOrEmpty(heroCardID))
		{
			heroCardID = CollectionManager.GetVanillaHero(classType);
		}
		CollectionDeck deck = new CollectionDeck
		{
			ID = 0L,
			Type = DeckType.NORMAL_DECK,
			Name = deckRecord.Name,
			HeroCardID = heroCardID,
			SortOrder = templateRecord.SortOrder,
			FormatType = (FormatType)templateRecord.FormatType,
			CreateDate = TimeUtils.DateTimeToUnixTimeStamp(DateTime.Now),
			DeckTemplateId = templateRecord.ID,
			TemplateSource = sourceType
		};
		deck.ClearSideboards();
		foreach (DeckCardDbfRecord card in deckRecord.Cards)
		{
			if (card == null)
			{
				continue;
			}
			string cardID = GameUtils.TranslateDbIdToCardId(card.CardRecord.ID);
			deck.AddCard(cardID, TAG_PREMIUM.NORMAL, false, null);
			foreach (SideboardCardDbfRecord sideboardCard in card.SideboardCards)
			{
				deck.AddCardToSideboard(GameUtils.TranslateDbIdToCardId(sideboardCard.SideboardCardId), card.CardRecord.ID, TAG_PREMIUM.NORMAL, allowInvalid: true);
			}
		}
		float end = Time.realtimeSinceStartup;
		Log.CollectionManager.Print("_decktemplate: Time spent loading loaner decks: " + (end - start));
		return deck;
	}

	public bool ShouldShowDeathKnightRunes()
	{
		if (GetMaxCardCount() == 1)
		{
			return false;
		}
		return HasClass(TAG_CLASS.DEATHKNIGHT);
	}

	protected int GetTotalCardCountExcludingUnownedOfCardId(string cardID)
	{
		int count = 0;
		foreach (CollectionDeckSlot slot in m_slots)
		{
			if (slot.Owned || slot.CardID != cardID)
			{
				count += slot.Count;
			}
		}
		return count;
	}
}
