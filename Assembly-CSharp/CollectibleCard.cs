using System;
using System.Collections.Generic;
using System.Text;
using Assets;
using UnityEngine;

public class CollectibleCard : ICollectible, IComparable
{
	private int m_CardDbId = -1;

	private DateTime m_LatestInsertDate = new DateTime(0L);

	private HashSet<string> m_SearchableTokens;

	private SearchableString m_LongSearchableName;

	private EntityDef m_EntityDef;

	private TAG_PREMIUM m_PremiumType;

	private CardDbfRecord m_CardRecord;

	private string m_CardName;

	public int CardDbId => m_CardDbId;

	public string CardId => m_EntityDef.GetCardId();

	public string Name => m_CardName;

	public string CardInHandText
	{
		get
		{
			CardTextBuilder overrideBuilder = m_EntityDef.GetCardTextBuilder();
			if (overrideBuilder != null)
			{
				return overrideBuilder.BuildCardTextInHand(m_EntityDef);
			}
			return CardTextBuilder.GetDefaultCardTextInHand(m_EntityDef);
		}
	}

	public string ArtistName => m_EntityDef.GetArtistName(TAG_PREMIUM.NORMAL);

	public string SignatureArtistName => m_EntityDef.GetArtistName(TAG_PREMIUM.SIGNATURE);

	public int ManaCost => m_EntityDef.GetCost();

	public int Attack => m_EntityDef.GetATK();

	public int Health => m_EntityDef.GetHealth();

	public TAG_CARD_SET Set => m_EntityDef.GetCardSet();

	public TAG_CLASS Class => m_EntityDef.GetClass();

	public TAG_RARITY Rarity => m_EntityDef.GetRarity();

	public List<TAG_RACE> Races => m_EntityDef.GetRaces();

	public TAG_CARDTYPE CardType => m_EntityDef.GetCardType();

	public bool IsHeroSkin => m_EntityDef.IsHeroSkin();

	public bool IsSpell => m_EntityDef.IsSpell();

	public TAG_PREMIUM PremiumType => m_PremiumType;

	public TAG_ROLE Role => m_EntityDef.GetMercenaryRole();

	public TAG_LETTUCE_FACTION MercenaryFaction => m_EntityDef.GetTag<TAG_LETTUCE_FACTION>(GAME_TAG.LETTUCE_FACTION);

	public bool IsMercenaryAbility => m_EntityDef.IsLettuceAbility();

	public RunePattern Runes => m_EntityDef.GetRuneCost();

	public int SeenCount { get; set; }

	public int OwnedCount { get; set; }

	public int DisenchantCount
	{
		get
		{
			if (!IsCraftable || GameUtils.IsClassicCard(m_EntityDef))
			{
				return 0;
			}
			return Mathf.Max(OwnedCount - DefaultMaxCopiesPerDeck, 0);
		}
	}

	public int IsCraftableDisenchantCount => Mathf.Max(OwnedCount - DefaultMaxCopiesPerDeck, 0);

	public TAG_SPELL_SCHOOL SpellSchool => m_EntityDef.GetSpellSchool();

	public int DefaultMaxCopiesPerDeck
	{
		get
		{
			if (!m_EntityDef.IsElite())
			{
				return 2;
			}
			return 1;
		}
	}

	public bool IsRefundable
	{
		get
		{
			if (!IsCraftable)
			{
				return false;
			}
			CardValueDbfRecord cardValueDbfRecord;
			NetCache.CardValue cardValue = CraftingManager.GetCardValue(CardId, PremiumType, out cardValueDbfRecord);
			if (cardValue != null && cardValue.SellValueOverride != 0 && cardValue.IsOverrideActive())
			{
				if (cardValueDbfRecord != null)
				{
					return cardValueDbfRecord.SellState != CardValue.SellState.PERMANENT_OVERRIDE_USE_CUSTOM_VALUE;
				}
				return true;
			}
			return false;
		}
	}

	private bool IsCraftableIgnoringEvent
	{
		get
		{
			string cardId = CardId;
			if (CraftingManager.GetCardValue(cardId, PremiumType) == null)
			{
				return false;
			}
			if (IsHeroSkin)
			{
				return false;
			}
			if (!FixedRewardsMgr.Get().CanCraftCard(cardId, PremiumType))
			{
				return false;
			}
			return true;
		}
	}

	public bool IsCraftable
	{
		get
		{
			string cardId = CardId;
			if (!IsCraftableIgnoringEvent)
			{
				return false;
			}
			if (CraftingUI.IsCraftingEventForCardActive(cardId, PremiumType, out var _))
			{
				return true;
			}
			return false;
		}
	}

	public bool IsEverCraftable
	{
		get
		{
			string cardId = CardId;
			if (!IsCraftableIgnoringEvent)
			{
				return false;
			}
			bool willBecomeActiveInFuture;
			return CraftingUI.IsCraftingEventForCardActive(cardId, PremiumType, out willBecomeActiveInFuture) || willBecomeActiveInFuture;
		}
	}

	public bool IsNewCard
	{
		get
		{
			if (OwnedCount > 0 && SeenCount < OwnedCount)
			{
				return SeenCount < DefaultMaxCopiesPerDeck;
			}
			return false;
		}
	}

	public bool IsNewCollectible => IsNewCard;

	public int SuggestWeight => m_CardRecord.SuggestionWeight;

	public DateTime LatestInsertDate
	{
		set
		{
			if (value > m_LatestInsertDate)
			{
				m_LatestInsertDate = value;
			}
		}
	}

	public CollectibleCard(CardDbfRecord cardRecord, EntityDef refEntityDef, TAG_PREMIUM premiumType)
	{
		m_CardDbId = cardRecord.ID;
		m_EntityDef = refEntityDef;
		m_PremiumType = premiumType;
		m_CardRecord = cardRecord;
		m_CardName = CardTextBuilder.GetDefaultCardName(m_EntityDef);
	}

	public HashSet<string> GetSearchableTokens()
	{
		if (m_SearchableTokens == null)
		{
			m_SearchableTokens = new HashSet<string>();
			if (GameUtils.IsLegacySet(Set))
			{
				CollectibleCardFilter.AddSearchableTokensToSet(TAG_CARD_SET.LEGACY, GameStrings.HasCardSetName, GameStrings.GetCardSetName, m_SearchableTokens);
				CollectibleCardFilter.AddSearchableTokensToSet(TAG_CARD_SET.LEGACY, GameStrings.HasCardSetNameShortened, GameStrings.GetCardSetNameShortened, m_SearchableTokens);
				CollectibleCardFilter.AddSearchableTokensToSet(TAG_CARD_SET.LEGACY, GameStrings.HasCardSetNameInitials, GameStrings.GetCardSetNameInitials, m_SearchableTokens);
			}
			else
			{
				CollectibleCardFilter.AddSearchableTokensToSet(Set, GameStrings.HasCardSetName, GameStrings.GetCardSetName, m_SearchableTokens);
				CollectibleCardFilter.AddSearchableTokensToSet(Set, GameStrings.HasCardSetNameShortened, GameStrings.GetCardSetNameShortened, m_SearchableTokens);
				CollectibleCardFilter.AddSearchableTokensToSet(Set, GameStrings.HasCardSetNameInitials, GameStrings.GetCardSetNameInitials, m_SearchableTokens);
			}
			if (!IsMercenaryAbility)
			{
				CollectibleCardFilter.AddSearchableTokensToSet(Rarity, GameStrings.HasRarityText, GameStrings.GetRarityText, m_SearchableTokens);
			}
			foreach (TAG_RACE race in Races)
			{
				CollectibleCardFilter.AddSearchableTokensToSet(race, GameStrings.HasRaceName, GameStrings.GetRaceName, m_SearchableTokens);
			}
			CollectibleCardFilter.AddSearchableTokensToSet(CardType, GameStrings.HasCardTypeName, GameStrings.GetCardTypeName, m_SearchableTokens);
			CollectibleCardFilter.AddSearchableTokensToSet(SpellSchool, GameStrings.HasSpellSchoolName, GameStrings.GetSpellSchoolName, m_SearchableTokens);
			CollectibleCardFilter.AddSearchableTokensToSet(MercenaryFaction, GameStrings.HasMercenaryFactionName, GameStrings.GetMercenaryFactionName, m_SearchableTokens);
			if (m_EntityDef.HasTag(GAME_TAG.MINI_SET))
			{
				CollectibleCardFilter.AddSearchableTokensToSet(Set, GameStrings.HasMiniSetName, GameStrings.GetMiniSetName, m_SearchableTokens);
			}
			if (m_EntityDef.IsMultiClass())
			{
				List<TAG_CLASS> classes = new List<TAG_CLASS>();
				m_EntityDef.GetClasses(classes);
				foreach (TAG_CLASS tag in classes)
				{
					if (GameStrings.HasClassName(tag))
					{
						CollectibleCardFilter.AddSingleSearchableTokenToSet(GameStrings.GetClassName(tag), m_SearchableTokens);
					}
				}
				CollectibleCardFilter.AddSingleSearchableTokenToSet(GameStrings.Get("GLOBAL_KEYWORD_MULTICLASS_SEARCH_TOKEN"), m_SearchableTokens);
			}
			if (Races.Contains(TAG_RACE.ALL))
			{
				foreach (TAG_RACE value in Enum.GetValues(typeof(TAG_RACE)))
				{
					CollectibleCardFilter.AddSearchableTokensToSet(value, GameStrings.HasRaceName, GameStrings.GetRaceName, m_SearchableTokens);
				}
			}
			foreach (KeywordTextDbfRecord keywordText in GameDbf.KeywordText.GetRecords())
			{
				if (keywordText.AutoAddSearchableToken && m_EntityDef.HasTag((GAME_TAG)keywordText.Tag))
				{
					CollectibleCardFilter.AddSingleSearchableTokenToSet(GameStrings.GetKeywordName((GAME_TAG)keywordText.Tag), m_SearchableTokens);
				}
			}
		}
		return m_SearchableTokens;
	}

	public SearchableString GetSearchableString()
	{
		if (m_LongSearchableName == null)
		{
			StringBuilder searchName = new StringBuilder();
			searchName.Append(Name);
			searchName.Append(" ");
			searchName.Append(CardInHandText);
			foreach (CardAdditonalSearchTermsDbfRecord searchTerm in m_CardRecord.SearchTerms)
			{
				searchName.Append(" ");
				searchName.Append(searchTerm.SearchTerm.GetString());
			}
			m_LongSearchableName = new SearchableString(searchName.ToString());
		}
		return m_LongSearchableName;
	}

	public bool FindTextInCard(string searchStr)
	{
		return CollectionUtils.FindTextInCollectible(this, searchStr);
	}

	public void AddCounts(int addOwnedCount, int addSeenCount, DateTime latestInsertDate)
	{
		OwnedCount += addOwnedCount;
		SeenCount += addSeenCount;
		LatestInsertDate = latestInsertDate;
	}

	public void RemoveCounts(int removeOwnedCount)
	{
		OwnedCount = Mathf.Max(OwnedCount - removeOwnedCount, 0);
	}

	public void ClearCounts()
	{
		OwnedCount = 0;
		SeenCount = 0;
	}

	public EntityDef GetEntityDef()
	{
		return m_EntityDef;
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (this == obj)
		{
			return true;
		}
		if (CardDbId == ((CollectibleCard)obj).CardDbId)
		{
			return PremiumType == ((CollectibleCard)obj).PremiumType;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return (int)(CardId.GetHashCode() + PremiumType);
	}

	public int CompareTo(object other)
	{
		if (!(other is CollectibleCard otherCard))
		{
			return -1;
		}
		return CollectionManager.EntityDefSortComparison(m_EntityDef, otherCard.m_EntityDef);
	}

	public bool HasCardTag(GAME_TAG tag)
	{
		return m_EntityDef.GetTag(tag) > 0;
	}
}
