using System;
using System.Collections.Generic;
using System.Linq;
using Assets;
using Blizzard.T5.Core;
using Blizzard.T5.Core.Utils;
using UnityEngine;

public class GameDbfIndex
{
	private Map<string, CardDbfRecord> m_cardsByCardId;

	private Map<int, List<CardTagDbfRecord>> m_cardTagsByCardDbId;

	private Map<string, CardDiscoverStringDbfRecord> m_cardDiscoverStringsByCardId;

	private List<string> m_allCardIds;

	private List<int> m_allCardDbIds;

	private List<string> m_collectibleCardIds;

	private List<int> m_collectibleCardDbIds;

	private int m_collectibleCardCount;

	private HashSet<CardDbfRecord> m_featuredCardEventCards;

	private Map<int, List<CardSetTimingDbfRecord>> m_cardSetTimingsByCardId;

	private Map<int, CardHeroDbfRecord> m_cardHeroByCardId;

	private Map<(int, int), FixedRewardDbfRecord> m_fixedRewardsByCardId;

	private Map<int, List<FixedRewardMapDbfRecord>> m_fixedRewardsByAction;

	private Map<FixedRewardAction.Type, List<FixedRewardActionDbfRecord>> m_fixedActionRecordsByType;

	private Map<int, List<int>> m_subsetsReferencedByRuleId;

	private Map<int, HashSet<string>> m_subsetCards;

	private Map<int, HashSet<int>> m_rulesByDeckRulesetId;

	private Map<int, Map<SpellType, string>> m_spellOverridesByCardSetId;

	private Map<int, int> m_cardsWithPlayerDeckOverrides;

	private Map<int, List<RewardTrackLevelDbfRecord>> m_rewardTrackLevelsByRewardTrackId;

	private Map<int, List<RewardItemDbfRecord>> m_rewardItemsByRewardListId;

	private Map<int, List<LettuceTreasureDbfRecord>> m_mercenaryValidTreasureIndex;

	private Map<int, LettuceEquipmentTierDbfRecord> m_equipmentTierByCardId;

	private Map<int, MercenaryUnlock> m_equipmentUnlockByEquipmentId;

	private Map<int, Map<TAG_PREMIUM, MercenaryUnlock>> m_UnlockByArtVariationIdAndPremium;

	private Map<int, int> m_MercenariesTaskIndex;

	private int m_maxMercenaryLevel;

	public GameDbfIndex()
	{
		Initialize();
	}

	public void Initialize()
	{
		m_cardsByCardId = new Map<string, CardDbfRecord>();
		m_cardTagsByCardDbId = new Map<int, List<CardTagDbfRecord>>();
		m_cardDiscoverStringsByCardId = new Map<string, CardDiscoverStringDbfRecord>();
		m_allCardIds = new List<string>();
		m_allCardDbIds = new List<int>();
		m_collectibleCardIds = new List<string>();
		m_collectibleCardDbIds = new List<int>();
		m_collectibleCardCount = 0;
		m_featuredCardEventCards = new HashSet<CardDbfRecord>();
		m_cardSetTimingsByCardId = new Map<int, List<CardSetTimingDbfRecord>>();
		m_cardHeroByCardId = new Map<int, CardHeroDbfRecord>();
		m_fixedRewardsByCardId = new Map<(int, int), FixedRewardDbfRecord>();
		m_fixedRewardsByAction = new Map<int, List<FixedRewardMapDbfRecord>>();
		m_fixedActionRecordsByType = new Map<FixedRewardAction.Type, List<FixedRewardActionDbfRecord>>();
		m_subsetsReferencedByRuleId = new Map<int, List<int>>();
		m_subsetCards = new Map<int, HashSet<string>>();
		m_rulesByDeckRulesetId = new Map<int, HashSet<int>>();
		m_cardsWithPlayerDeckOverrides = new Map<int, int>();
		m_spellOverridesByCardSetId = new Map<int, Map<SpellType, string>>();
		m_rewardTrackLevelsByRewardTrackId = new Map<int, List<RewardTrackLevelDbfRecord>>();
		m_rewardItemsByRewardListId = new Map<int, List<RewardItemDbfRecord>>();
		m_equipmentTierByCardId = new Map<int, LettuceEquipmentTierDbfRecord>();
		m_equipmentUnlockByEquipmentId = new Map<int, MercenaryUnlock>();
		m_UnlockByArtVariationIdAndPremium = new Map<int, Map<TAG_PREMIUM, MercenaryUnlock>>();
		m_MercenariesTaskIndex = new Map<int, int>();
		m_mercenaryValidTreasureIndex = new Map<int, List<LettuceTreasureDbfRecord>>();
		m_maxMercenaryLevel = 0;
	}

	public void PostProcessDbfLoad_CardTag(Dbf<CardTagDbfRecord> dbf)
	{
		m_cardTagsByCardDbId.Clear();
		foreach (CardTagDbfRecord record in dbf.GetRecords())
		{
			OnCardTagAdded(record);
		}
	}

	public void OnCardTagAdded(CardTagDbfRecord cardTagRecord)
	{
		int cardDbId = cardTagRecord.CardId;
		List<CardTagDbfRecord> tags = null;
		if (!m_cardTagsByCardDbId.TryGetValue(cardDbId, out tags))
		{
			tags = new List<CardTagDbfRecord>();
			m_cardTagsByCardDbId[cardDbId] = tags;
		}
		tags.Add(cardTagRecord);
	}

	public void OnCardTagRemoved(List<CardTagDbfRecord> removedRecords)
	{
		foreach (CardTagDbfRecord rec in removedRecords)
		{
			using Map<int, List<CardTagDbfRecord>>.ValueCollection.Enumerator enumerator2 = m_cardTagsByCardDbId.Values.GetEnumerator();
			while (enumerator2.MoveNext() && !enumerator2.Current.Remove(rec))
			{
			}
		}
	}

	public void PostProcessDbfLoad_CardDiscoverString(Dbf<CardDiscoverStringDbfRecord> dbf)
	{
		m_cardDiscoverStringsByCardId.Clear();
		foreach (CardDiscoverStringDbfRecord record in dbf.GetRecords())
		{
			OnCardDiscoverStringAdded(record);
		}
	}

	public void OnCardDiscoverStringAdded(CardDiscoverStringDbfRecord cardDiscoverStringRecord)
	{
		m_cardDiscoverStringsByCardId[cardDiscoverStringRecord.NoteMiniGuid] = cardDiscoverStringRecord;
	}

	public void OnCardDiscoverStringRemoved(List<CardDiscoverStringDbfRecord> removedRecords)
	{
		foreach (CardDiscoverStringDbfRecord rec in removedRecords)
		{
			m_cardDiscoverStringsByCardId.Remove(rec.NoteMiniGuid);
		}
	}

	public int GetCardTagValue(int cardDbId, GAME_TAG tagId)
	{
		List<CardTagDbfRecord> tags = null;
		if (!m_cardTagsByCardDbId.TryGetValue(cardDbId, out tags))
		{
			return 0;
		}
		int i = 0;
		for (int iMax = tags.Count; i < iMax; i++)
		{
			CardTagDbfRecord tag = tags[i];
			if (tag.TagId == (int)tagId)
			{
				return tag.TagValue;
			}
		}
		return 0;
	}

	public bool GetCardHasTag(int cardDbId, GAME_TAG tagId)
	{
		List<CardTagDbfRecord> tags = null;
		if (!m_cardTagsByCardDbId.TryGetValue(cardDbId, out tags))
		{
			return false;
		}
		int i = 0;
		for (int iMax = tags.Count; i < iMax; i++)
		{
			if (tags[i].TagId == (int)tagId)
			{
				return true;
			}
		}
		return false;
	}

	public bool TryGetCardTagRecords(int cardDbId, out List<CardTagDbfRecord> tagRecords)
	{
		return m_cardTagsByCardDbId.TryGetValue(cardDbId, out tagRecords);
	}

	public void PostProcessDbfLoad_Card(Dbf<CardDbfRecord> dbf)
	{
		m_cardsByCardId.Clear();
		m_allCardDbIds.Clear();
		m_allCardIds.Clear();
		m_collectibleCardCount = 0;
		m_collectibleCardIds.Clear();
		m_collectibleCardDbIds.Clear();
		m_featuredCardEventCards.Clear();
		foreach (CardDbfRecord record in dbf.GetRecords())
		{
			OnCardAdded(record);
		}
	}

	public void OnCardAdded(CardDbfRecord cardRecord)
	{
		int dbId = cardRecord.ID;
		bool num = GetCardTagValue(dbId, GAME_TAG.COLLECTIBLE) == 1;
		string cardId = cardRecord.NoteMiniGuid;
		m_cardsByCardId[cardId] = cardRecord;
		m_allCardDbIds.Add(dbId);
		m_allCardIds.Add(cardId);
		if (num)
		{
			m_collectibleCardCount++;
			m_collectibleCardIds.Add(cardId);
			m_collectibleCardDbIds.Add(dbId);
		}
		if (cardRecord.FeaturedCardsEvent != EventTimingType.UNKNOWN)
		{
			m_featuredCardEventCards.Add(cardRecord);
		}
	}

	public void OnCardRemoved(List<CardDbfRecord> removedRecords)
	{
		HashSet<int> removedCardDbIds = new HashSet<int>();
		HashSet<string> removedCardIds = new HashSet<string>();
		foreach (CardDbfRecord rec in removedRecords)
		{
			removedCardDbIds.Add(rec.ID);
			if (rec.NoteMiniGuid != null)
			{
				removedCardIds.Add(rec.NoteMiniGuid);
				m_cardsByCardId.Remove(rec.NoteMiniGuid);
			}
			m_featuredCardEventCards.Remove(rec);
		}
		if (removedCardDbIds.Count > 0)
		{
			m_allCardDbIds.RemoveAll((int cardDbId) => removedCardDbIds.Contains(cardDbId));
			m_collectibleCardDbIds.RemoveAll((int cardDbId) => m_collectibleCardDbIds.Contains(cardDbId));
		}
		if (removedCardIds.Count > 0)
		{
			m_allCardIds.RemoveAll((string cardId) => removedCardIds.Contains(cardId));
			m_collectibleCardIds.RemoveAll((string cardId) => removedCardIds.Contains(cardId));
		}
	}

	public void PostProcessDbfLoad_CardSetTiming(Dbf<CardSetTimingDbfRecord> dbf)
	{
		m_cardSetTimingsByCardId.Clear();
		foreach (CardSetTimingDbfRecord record in dbf.GetRecords())
		{
			OnCardSetTimingAdded(record);
		}
	}

	public void OnCardSetTimingAdded(CardSetTimingDbfRecord record)
	{
		int cardId = record.CardId;
		if (!m_cardSetTimingsByCardId.TryGetValue(cardId, out var records))
		{
			records = new List<CardSetTimingDbfRecord>();
			m_cardSetTimingsByCardId.Add(cardId, records);
		}
		records.Add(record);
	}

	public void OnCardSetTimingRemoved(List<CardSetTimingDbfRecord> removedRecords)
	{
		HashSet<int> removedCardSetTimings = new HashSet<int>(removedRecords.Select((CardSetTimingDbfRecord r) => r.ID));
		foreach (int cardId in new HashSet<int>(removedRecords.Select((CardSetTimingDbfRecord r) => r.CardId)))
		{
			if (m_cardSetTimingsByCardId.TryGetValue(cardId, out var cardSetTimingRecords))
			{
				cardSetTimingRecords.RemoveAll((CardSetTimingDbfRecord r) => removedCardSetTimings.Contains(r.ID));
			}
		}
	}

	public List<CardSetTimingDbfRecord> GetCardSetTimingByCardId(int cardId)
	{
		List<CardSetTimingDbfRecord> result = null;
		if (!m_cardSetTimingsByCardId.TryGetValue(cardId, out result))
		{
			result = new List<CardSetTimingDbfRecord>();
			m_cardSetTimingsByCardId[cardId] = result;
		}
		return result;
	}

	public void PostProcessDbfLoad_CardHero(Dbf<CardHeroDbfRecord> dbf)
	{
		m_cardHeroByCardId.Clear();
		foreach (CardHeroDbfRecord record in dbf.GetRecords())
		{
			OnCardHeroAdded(record);
		}
	}

	public void OnCardHeroAdded(CardHeroDbfRecord record)
	{
		int cardId = record.CardId;
		m_cardHeroByCardId[cardId] = record;
	}

	public void OnCardHeroRemoved(List<CardHeroDbfRecord> removedRecords)
	{
		new HashSet<int>(removedRecords.Select((CardHeroDbfRecord r) => r.ID));
		foreach (int cardId in new HashSet<int>(removedRecords.Select((CardHeroDbfRecord r) => r.CardId)))
		{
			m_cardHeroByCardId.Remove(cardId);
		}
	}

	public CardHeroDbfRecord GetCardHeroByCardId(int cardId)
	{
		m_cardHeroByCardId.TryGetValue(cardId, out var result);
		return result;
	}

	public void PostProcessDbfLoad_FixedReward(Dbf<FixedRewardDbfRecord> dbf)
	{
		m_fixedRewardsByCardId.Clear();
		foreach (FixedRewardDbfRecord record in dbf.GetRecords())
		{
			if (record.CardRecord != null)
			{
				m_fixedRewardsByCardId.Add((record.CardId, record.CardPremium), record);
			}
		}
	}

	public void PostProcessDbfLoad_FixedRewardMap(Dbf<FixedRewardMapDbfRecord> dbf)
	{
		m_fixedRewardsByAction.Clear();
		foreach (FixedRewardMapDbfRecord record in dbf.GetRecords())
		{
			OnFixedRewardMapAdded(record);
		}
	}

	public void OnFixedRewardMapAdded(FixedRewardMapDbfRecord record)
	{
		int actionId = record.ActionId;
		if (!m_fixedRewardsByAction.TryGetValue(actionId, out var records))
		{
			records = new List<FixedRewardMapDbfRecord>();
			m_fixedRewardsByAction.Add(actionId, records);
		}
		records.Add(record);
	}

	public void OnFixedRewardMapRemoved(List<FixedRewardMapDbfRecord> removedRecords)
	{
		HashSet<int> removedIds = new HashSet<int>(removedRecords.Select((FixedRewardMapDbfRecord r) => r.ID));
		foreach (int actionId in new HashSet<int>(removedRecords.Select((FixedRewardMapDbfRecord r) => r.ActionId)))
		{
			if (m_fixedRewardsByAction.TryGetValue(actionId, out var records))
			{
				records.RemoveAll((FixedRewardMapDbfRecord r) => removedIds.Contains(r.ID));
			}
		}
	}

	public void PostProcessDbfLoad_FixedRewardAction(Dbf<FixedRewardActionDbfRecord> dbf)
	{
		m_fixedActionRecordsByType.Clear();
		foreach (FixedRewardActionDbfRecord record in dbf.GetRecords())
		{
			OnFixedRewardActionAdded(record);
		}
	}

	public void OnFixedRewardActionAdded(FixedRewardActionDbfRecord record)
	{
		FixedRewardAction.Type fixedActionType = record.Type;
		if (!m_fixedActionRecordsByType.TryGetValue(fixedActionType, out var records))
		{
			records = new List<FixedRewardActionDbfRecord>();
			m_fixedActionRecordsByType.Add(fixedActionType, records);
		}
		records.Add(record);
	}

	public void OnFixedRewardActionRemoved(List<FixedRewardActionDbfRecord> removedRecords)
	{
		HashSet<int> removedIds = new HashSet<int>(removedRecords.Select((FixedRewardActionDbfRecord r) => r.ID));
		HashSet<FixedRewardAction.Type> removedTypes = null;
		try
		{
			removedTypes = new HashSet<FixedRewardAction.Type>(removedRecords.Select((FixedRewardActionDbfRecord r) => EnumUtils.GetEnum<FixedRewardAction.Type>(r.Type.ToString())));
		}
		catch
		{
			Debug.LogErrorFormat("Error parsing FixedRewardAction.Type, type did not match a FixedRewardType: {0}", string.Join(", ", removedRecords.Select((FixedRewardActionDbfRecord r) => r.Type.ToString()).ToArray()));
			removedTypes = new HashSet<FixedRewardAction.Type>();
		}
		foreach (FixedRewardAction.Type type in removedTypes)
		{
			if (m_fixedActionRecordsByType.TryGetValue(type, out var records))
			{
				records.RemoveAll((FixedRewardActionDbfRecord r) => removedIds.Contains(r.ID));
			}
		}
	}

	public void PostProcessDbfLoad_DeckRulesetRuleSubset(Dbf<DeckRulesetRuleSubsetDbfRecord> dbf)
	{
		m_subsetsReferencedByRuleId.Clear();
		foreach (DeckRulesetRuleSubsetDbfRecord record in dbf.GetRecords())
		{
			OnDeckRulesetRuleSubsetAdded(record);
		}
	}

	public void OnDeckRulesetRuleSubsetAdded(DeckRulesetRuleSubsetDbfRecord record)
	{
		int ruleId = record.DeckRulesetRuleId;
		int subsetId = record.SubsetId;
		if (!m_subsetsReferencedByRuleId.TryGetValue(ruleId, out var list))
		{
			list = new List<int>();
			m_subsetsReferencedByRuleId[ruleId] = list;
		}
		list.Add(subsetId);
	}

	public void OnDeckRulesetRuleSubsetRemoved(List<DeckRulesetRuleSubsetDbfRecord> removedRecords)
	{
		foreach (DeckRulesetRuleSubsetDbfRecord rec in removedRecords)
		{
			if (m_subsetsReferencedByRuleId.TryGetValue(rec.DeckRulesetRuleId, out var list))
			{
				list.RemoveAll((int subsetId) => subsetId == rec.SubsetId);
			}
		}
	}

	public void PostProcessDbfLoad_SubsetCard(Dbf<SubsetCardDbfRecord> dbf)
	{
		m_subsetCards.Clear();
		foreach (SubsetCardDbfRecord record in dbf.GetRecords())
		{
			OnSubsetCardAdded(record);
		}
	}

	public void OnSubsetCardAdded(SubsetCardDbfRecord record)
	{
		int subsetId = record.SubsetId;
		int cardDbId = record.CardId;
		CardDbfRecord cardRecord = GameDbf.Card.GetRecord(cardDbId);
		if (cardRecord != null)
		{
			if (!m_subsetCards.TryGetValue(subsetId, out var subset))
			{
				subset = new HashSet<string>();
				m_subsetCards[subsetId] = subset;
			}
			subset.Add(cardRecord.NoteMiniGuid);
		}
	}

	public void OnSubsetCardRemoved(List<SubsetCardDbfRecord> removedRecords)
	{
		foreach (SubsetCardDbfRecord rec in removedRecords)
		{
			if (m_subsetCards.TryGetValue(rec.SubsetId, out var subset) && subset != null)
			{
				CardDbfRecord cardRecord = GameDbf.Card.GetRecord(rec.CardId);
				if (cardRecord != null && cardRecord.NoteMiniGuid != null)
				{
					subset.Remove(cardRecord.NoteMiniGuid);
				}
			}
		}
	}

	public void PostProcessDbfLoad_DeckRulesetRule(Dbf<DeckRulesetRuleDbfRecord> dbf)
	{
		m_rulesByDeckRulesetId.Clear();
		foreach (DeckRulesetRuleDbfRecord record in dbf.GetRecords())
		{
			OnDeckRulesetRuleAdded(record);
		}
	}

	public void OnDeckRulesetRuleAdded(DeckRulesetRuleDbfRecord record)
	{
		if (!m_rulesByDeckRulesetId.TryGetValue(record.DeckRulesetId, out var ruleIds))
		{
			ruleIds = new HashSet<int>();
			m_rulesByDeckRulesetId[record.DeckRulesetId] = ruleIds;
		}
		ruleIds.Add(record.ID);
	}

	public void OnDeckRulesetRuleRemoved(List<DeckRulesetRuleDbfRecord> removedRecords)
	{
		foreach (DeckRulesetRuleDbfRecord rec in removedRecords)
		{
			if (m_rulesByDeckRulesetId.TryGetValue(rec.DeckRulesetId, out var ruleIds))
			{
				ruleIds.Remove(rec.ID);
			}
		}
	}

	public void PostProcessDbfLoad_CardPlayerDeckOverride(Dbf<CardPlayerDeckOverrideDbfRecord> dbf)
	{
		m_cardsWithPlayerDeckOverrides.Clear();
		foreach (CardPlayerDeckOverrideDbfRecord record in dbf.GetRecords())
		{
			OnCardPlayerDeckOverrideAdded(record);
		}
	}

	public void OnCardPlayerDeckOverrideAdded(CardPlayerDeckOverrideDbfRecord record)
	{
		m_cardsWithPlayerDeckOverrides[record.CardId] = record.ID;
	}

	public void OnCardPlayerDeckOverrideRemoved(List<CardPlayerDeckOverrideDbfRecord> removedRecords)
	{
		foreach (CardPlayerDeckOverrideDbfRecord rec in removedRecords)
		{
			m_cardsWithPlayerDeckOverrides.Remove(rec.CardId);
		}
	}

	public CardDbfRecord GetCardRecord(string cardId)
	{
		if (string.IsNullOrEmpty(cardId))
		{
			return null;
		}
		if (cardId == "PlaceholderCard")
		{
			CachePlaceholderRecord();
		}
		CardDbfRecord record = null;
		m_cardsByCardId.TryGetValue(cardId, out record);
		return record;
	}

	public CardSetDbfRecord GetCardSet(TAG_CARD_SET cardSetId)
	{
		return GameDbf.CardSet.GetRecord((int)cardSetId);
	}

	public string GetCardSetSpellOverride(TAG_CARD_SET cardSetId, SpellType spellType)
	{
		Map<SpellType, string> spellOverridesForSet = null;
		if (m_spellOverridesByCardSetId.TryGetValue((int)cardSetId, out spellOverridesForSet))
		{
			string spellOverridePrefab = null;
			if (spellOverridesForSet.TryGetValue(spellType, out spellOverridePrefab))
			{
				return spellOverridePrefab;
			}
		}
		return null;
	}

	public void PostProcessDbfLoad_CardSetSpellOverride(Dbf<CardSetSpellOverrideDbfRecord> dbf)
	{
		m_spellOverridesByCardSetId.Clear();
		foreach (CardSetSpellOverrideDbfRecord record in dbf.GetRecords())
		{
			OnCardSetSpellOverrideAdded(record);
		}
	}

	public void OnCardSetSpellOverrideAdded(CardSetSpellOverrideDbfRecord record)
	{
		if (!m_spellOverridesByCardSetId.TryGetValue(record.CardSetId, out var spellOverrides))
		{
			spellOverrides = new Map<SpellType, string>();
			m_spellOverridesByCardSetId[record.CardSetId] = spellOverrides;
		}
		SpellType spellTypeValue = (SpellType)Enum.Parse(typeof(SpellType), record.SpellType);
		if (Enum.IsDefined(typeof(SpellType), spellTypeValue))
		{
			spellOverrides.Add(spellTypeValue, record.OverridePrefab);
		}
	}

	public void OnCardSetSpellOverrideRemoved(List<CardSetSpellOverrideDbfRecord> removedRecords)
	{
		foreach (CardSetSpellOverrideDbfRecord record in removedRecords)
		{
			if (!m_spellOverridesByCardSetId.TryGetValue(record.CardSetId, out var spellOverrides))
			{
				continue;
			}
			SpellType spellTypeValue = (SpellType)Enum.Parse(typeof(SpellType), record.SpellType);
			if (Enum.IsDefined(typeof(SpellType), spellTypeValue))
			{
				spellOverrides.Remove(spellTypeValue);
				if (spellOverrides.Count == 0)
				{
					m_spellOverridesByCardSetId.Remove(record.CardSetId);
				}
			}
		}
	}

	public void PostProcessDbfLoad_RewardTrackLevel(Dbf<RewardTrackLevelDbfRecord> dbf)
	{
		m_rewardTrackLevelsByRewardTrackId.Clear();
		foreach (RewardTrackLevelDbfRecord record in dbf.GetRecords())
		{
			OnRewardTrackLevelAdded(record);
		}
	}

	public void OnRewardTrackLevelAdded(RewardTrackLevelDbfRecord record)
	{
		int rewardTrackId = record.RewardTrackId;
		if (!m_rewardTrackLevelsByRewardTrackId.TryGetValue(rewardTrackId, out var records))
		{
			records = new List<RewardTrackLevelDbfRecord>();
			m_rewardTrackLevelsByRewardTrackId.Add(rewardTrackId, records);
		}
		records.Add(record);
	}

	public void OnRewardTrackLevelRemoved(List<RewardTrackLevelDbfRecord> removedRecords)
	{
		HashSet<int> removedLevelIds = new HashSet<int>(removedRecords.Select((RewardTrackLevelDbfRecord r) => r.ID));
		foreach (int rewardTrackId in new HashSet<int>(removedRecords.Select((RewardTrackLevelDbfRecord r) => r.RewardTrackId)))
		{
			if (m_rewardTrackLevelsByRewardTrackId.TryGetValue(rewardTrackId, out var rewardTrackLevelDbfRecords))
			{
				rewardTrackLevelDbfRecords.RemoveAll((RewardTrackLevelDbfRecord r) => removedLevelIds.Contains(r.ID));
			}
		}
	}

	public List<RewardTrackLevelDbfRecord> GetRewardTrackLevelsByRewardTrackId(int rewardTrackId)
	{
		List<RewardTrackLevelDbfRecord> result = null;
		if (!m_rewardTrackLevelsByRewardTrackId.TryGetValue(rewardTrackId, out result))
		{
			result = new List<RewardTrackLevelDbfRecord>();
			m_rewardTrackLevelsByRewardTrackId[rewardTrackId] = result;
		}
		return result;
	}

	public void PostProcessDbfLoad_RewardItems(Dbf<RewardItemDbfRecord> dbf)
	{
		m_rewardItemsByRewardListId.Clear();
		foreach (RewardItemDbfRecord record in dbf.GetRecords())
		{
			OnRewardItemAdded(record);
		}
	}

	public void OnRewardItemAdded(RewardItemDbfRecord record)
	{
		int rewardListId = record.RewardListId;
		if (!m_rewardItemsByRewardListId.TryGetValue(rewardListId, out var records))
		{
			records = new List<RewardItemDbfRecord>();
			m_rewardItemsByRewardListId.Add(rewardListId, records);
		}
		records.Add(record);
	}

	public void OnRewardItemRemoved(List<RewardItemDbfRecord> removedRecords)
	{
		HashSet<int> removedRewardItemIds = new HashSet<int>(removedRecords.Select((RewardItemDbfRecord r) => r.ID));
		foreach (int rewardListId in new HashSet<int>(removedRecords.Select((RewardItemDbfRecord r) => r.RewardListId)))
		{
			if (m_rewardItemsByRewardListId.TryGetValue(rewardListId, out var rewardItemlDbfRecords))
			{
				rewardItemlDbfRecords.RemoveAll((RewardItemDbfRecord r) => removedRewardItemIds.Contains(r.ID));
			}
		}
	}

	public List<RewardItemDbfRecord> GetRewardItemsByRewardTrackId(int rewardTrackId)
	{
		List<RewardItemDbfRecord> result = null;
		if (!m_rewardItemsByRewardListId.TryGetValue(rewardTrackId, out result))
		{
			result = new List<RewardItemDbfRecord>();
			m_rewardItemsByRewardListId[rewardTrackId] = result;
		}
		return result;
	}

	public string GetCardDiscoverString(string cardId)
	{
		if (string.IsNullOrEmpty(cardId))
		{
			return null;
		}
		CardDiscoverStringDbfRecord record = null;
		if (m_cardDiscoverStringsByCardId.TryGetValue(cardId, out record))
		{
			return record.StringId;
		}
		return null;
	}

	public string GetClientString(int recordId)
	{
		return GameDbf.ClientString.GetRecord(recordId)?.Text;
	}

	private void CachePlaceholderRecord()
	{
		if (m_cardsByCardId.ContainsKey("PlaceholderCard"))
		{
			return;
		}
		CardDbfRecord placeholderCard = new CardDbfRecord();
		placeholderCard.SetID(-1);
		placeholderCard.SetNoteMiniGuid("PlaceholderCard");
		DbfLocValue name = new DbfLocValue();
		name.SetString(Locale.enUS, "Placeholder Card");
		placeholderCard.SetName(name);
		DbfLocValue text = new DbfLocValue();
		text.SetString(Locale.enUS, "Battlecry: Someone remembers to publish this card.");
		placeholderCard.SetTextInHand(text);
		Dictionary<GAME_TAG, int> obj = new Dictionary<GAME_TAG, int>
		{
			{
				GAME_TAG.CARD_SET,
				7
			},
			{
				GAME_TAG.CARDTYPE,
				4
			},
			{
				GAME_TAG.CLASS,
				4
			},
			{
				GAME_TAG.RARITY,
				4
			},
			{
				GAME_TAG.FACTION,
				3
			},
			{
				GAME_TAG.COST,
				9
			},
			{
				GAME_TAG.HEALTH,
				8
			},
			{
				GAME_TAG.ATK,
				6
			}
		};
		List<CardTagDbfRecord> placeholderTags = new List<CardTagDbfRecord>();
		foreach (KeyValuePair<GAME_TAG, int> kv in obj)
		{
			CardTagDbfRecord tag = new CardTagDbfRecord();
			tag.SetCardId(placeholderCard.ID);
			tag.SetTagId((int)kv.Key);
			tag.SetTagValue(kv.Value);
			placeholderTags.Add(tag);
		}
		m_cardsByCardId.Add("PlaceholderCard", placeholderCard);
		m_cardTagsByCardDbId.Add(placeholderCard.ID, placeholderTags);
	}

	public int GetCollectibleCardCount()
	{
		return m_collectibleCardCount;
	}

	public List<string> GetAllCardIds()
	{
		return m_allCardIds;
	}

	public List<int> GetAllCardDbIds()
	{
		return m_allCardDbIds;
	}

	public List<string> GetCollectibleCardIds()
	{
		return m_collectibleCardIds;
	}

	public List<int> GetCollectibleCardDbIds()
	{
		return m_collectibleCardDbIds;
	}

	public HashSet<CardDbfRecord> GetCardsWithFeaturedCardsEvent()
	{
		return m_featuredCardEventCards;
	}

	public FixedRewardDbfRecord GetFixedRewardRecordsForCardId(int cardId, int premiumType)
	{
		FixedRewardDbfRecord result = null;
		m_fixedRewardsByCardId.TryGetValue((cardId, premiumType), out result);
		return result;
	}

	public List<FixedRewardMapDbfRecord> GetFixedRewardMapRecordsForAction(int actionId)
	{
		List<FixedRewardMapDbfRecord> result = null;
		if (!m_fixedRewardsByAction.TryGetValue(actionId, out result))
		{
			result = new List<FixedRewardMapDbfRecord>();
			m_fixedRewardsByAction[actionId] = result;
		}
		return result;
	}

	public List<FixedRewardActionDbfRecord> GetFixedActionRecordsForType(FixedRewardAction.Type type)
	{
		List<FixedRewardActionDbfRecord> result = null;
		if (!m_fixedActionRecordsByType.TryGetValue(type, out result))
		{
			result = new List<FixedRewardActionDbfRecord>();
			m_fixedActionRecordsByType[type] = result;
		}
		return result;
	}

	public List<HashSet<string>> GetSubsetsForRule(int ruleId)
	{
		List<HashSet<string>> result = new List<HashSet<string>>();
		if (m_subsetsReferencedByRuleId.TryGetValue(ruleId, out var subsetIds))
		{
			for (int i = 0; i < subsetIds.Count; i++)
			{
				result.Add(GetSubsetById(subsetIds[i]));
			}
		}
		return result;
	}

	public List<int> GetSubsetDbfRecordsForRule(int ruleId)
	{
		List<int> result = new List<int>();
		m_subsetsReferencedByRuleId.TryGetValue(ruleId, out result);
		return result;
	}

	public List<int> GetCardSetIdsForSubsetRule(int ruleId)
	{
		List<int> subsetIds = new List<int>();
		List<int> referencedSubsetIds = new List<int>();
		if (m_subsetsReferencedByRuleId.TryGetValue(ruleId, out subsetIds))
		{
			foreach (int subsetId in subsetIds)
			{
				SubsetDbfRecord subsetDbf = GameDbf.Subset.GetRecord(subsetId);
				if (subsetDbf == null)
				{
					continue;
				}
				foreach (SubsetRuleDbfRecord rule in subsetDbf.Rules)
				{
					if (rule.Tag == 183 && !rule.RuleIsNot && rule.MaxValue == rule.MinValue)
					{
						referencedSubsetIds.Add(rule.MaxValue);
					}
				}
			}
		}
		return referencedSubsetIds;
	}

	public DeckRulesetRuleDbfRecord[] GetRulesForDeckRuleset(int deckRulesetId)
	{
		if (!m_rulesByDeckRulesetId.TryGetValue(deckRulesetId, out var ruleIds))
		{
			ruleIds = new HashSet<int>();
		}
		return (from ruleId in ruleIds
			let ruleDbf = GameDbf.DeckRulesetRule.GetRecord(ruleId)
			where ruleDbf != null
			select ruleDbf).ToArray();
	}

	public HashSet<string> GetSubsetById(int id)
	{
		HashSet<string> result = null;
		if (!m_subsetCards.TryGetValue(id, out result))
		{
			result = new HashSet<string>();
			m_subsetCards[id] = result;
		}
		return result;
	}

	public IEnumerable<CardPlayerDeckOverrideDbfRecord> GetAllCardPlayerDeckOverrides()
	{
		return m_cardsWithPlayerDeckOverrides.Select(delegate(KeyValuePair<int, int> kv)
		{
			Dbf<CardPlayerDeckOverrideDbfRecord> cardPlayerDeckOverride = GameDbf.CardPlayerDeckOverride;
			KeyValuePair<int, int> keyValuePair = kv;
			return cardPlayerDeckOverride.GetRecord(keyValuePair.Value);
		});
	}

	public bool HasCardPlayerDeckOverride(string cardId)
	{
		int cardDbId = GameUtils.TranslateCardIdToDbId(cardId);
		int cardPlayerDeckOverrideId;
		return m_cardsWithPlayerDeckOverrides.TryGetValue(cardDbId, out cardPlayerDeckOverrideId);
	}

	public CardPlayerDeckOverrideDbfRecord GetCardPlayerDeckOverride(string cardId)
	{
		int cardDbId = GameUtils.TranslateCardIdToDbId(cardId);
		if (!m_cardsWithPlayerDeckOverrides.TryGetValue(cardDbId, out var cardPlayerDeckOverrideId))
		{
			return null;
		}
		return GameDbf.CardPlayerDeckOverride.GetRecord(cardPlayerDeckOverrideId);
	}

	public void PostProcessDbfLoad_LettuceEquipmentTier()
	{
		m_equipmentTierByCardId.Clear();
		foreach (LettuceEquipmentTierDbfRecord record in GameDbf.LettuceEquipmentTier.GetRecords())
		{
			OnLettuceEquipmentTierAdded(record);
		}
	}

	public void OnLettuceEquipmentTierAdded(LettuceEquipmentTierDbfRecord tier)
	{
		m_equipmentTierByCardId[tier.CardId] = tier;
		List<BonusBountyDropChanceDbfRecord> bonusBounty = tier.BonusBountyDropChances;
		if (bonusBounty == null)
		{
			return;
		}
		int i = 0;
		for (int iMax = bonusBounty.Count; i < iMax; i++)
		{
			BonusBountyDropChanceDbfRecord bountyDrop = bonusBounty[i];
			if (bountyDrop.LettuceBountyRecord != null)
			{
				int equipmentId = tier.LettuceEquipmentId;
				if (m_equipmentUnlockByEquipmentId.TryGetValue(equipmentId, out var unlock))
				{
					Log.Lettuce.PrintError($"GameDbFIndex.OnLettuceEquipmentTierAdded(): EquipmentID [{equipmentId}] is already unlocked by {unlock}");
				}
				else
				{
					m_equipmentUnlockByEquipmentId.Add(equipmentId, MercenaryUnlock.Create(bountyDrop.LettuceBountyRecord));
				}
			}
		}
	}

	public void OnLettuceEquipmentTierRemoved(List<LettuceEquipmentTierDbfRecord> removedRecords)
	{
		foreach (LettuceEquipmentTierDbfRecord tier in removedRecords)
		{
			m_equipmentTierByCardId.Remove(tier.CardId);
		}
	}

	public LettuceEquipmentTierDbfRecord GetEquipmentTierFromCardID(int cardId)
	{
		if (!m_equipmentTierByCardId.ContainsKey(cardId))
		{
			Log.Lettuce.PrintError($"Missing LETTUCE_EQUIPMENT_TIER record for CARD database ID: {cardId}. Did you forget to create it in HearthEdit 2?");
			return null;
		}
		return m_equipmentTierByCardId[cardId];
	}

	public void PostProcessDbfLoad_VisitorTask()
	{
		foreach (MercenaryVisitorDbfRecord record in GameDbf.MercenaryVisitor.GetRecords())
		{
			List<VisitorTaskChainDbfRecord> chainList = record.VisitorTaskChains;
			if (chainList == null || chainList.Count <= 0)
			{
				continue;
			}
			List<TaskListDbfRecord> taskList = chainList.Last()?.TaskList;
			if (taskList != null)
			{
				for (int taskIndex = 0; taskIndex < taskList.Count; taskIndex++)
				{
					OnVisitorTaskAdded(taskList[taskIndex].TaskRecord, taskIndex);
					m_MercenariesTaskIndex[taskList[taskIndex].TaskRecord.ID] = taskIndex;
				}
			}
		}
	}

	public void OnVisitorTaskAdded(VisitorTaskDbfRecord record, int taskIndex)
	{
		RewardListDbfRecord rewardList = record.RewardListRecord;
		if (rewardList == null)
		{
			return;
		}
		List<RewardItemDbfRecord> rewardItems = rewardList.RewardItems;
		if (rewardItems == null)
		{
			return;
		}
		foreach (RewardItemDbfRecord item in rewardItems)
		{
			switch (item.RewardType)
			{
			case RewardItem.RewardType.MERCENARY:
				if (item.MercenaryArtVariationRecord != null)
				{
					MercenaryUnlock newUnlock = MercenaryUnlock.Create(record, taskIndex);
					foreach (MercenaryArtVariationPremiumDbfRecord premiumVariation in item.MercenaryArtVariationRecord.MercenaryArtVariationPremiums)
					{
						if (premiumVariation.Premium == (MercenaryArtVariationPremium.MercenariesPremium)item.MercenaryArtPremium && !string.IsNullOrEmpty(premiumVariation.CustomAcquireText))
						{
							newUnlock.m_unlockType = MercenaryUnlock.UnlockType.Custom;
							newUnlock.m_customAcquireText = premiumVariation.CustomAcquireText;
						}
					}
					MercenaryUnlock existingUnlock = AddArtVariationUnlock(item.MercenaryArtVariation, item.MercenaryArtPremium, newUnlock);
					if (existingUnlock != null)
					{
						Log.Lettuce.PrintError($"GameDbFIndex.OnVisitorTaskAdded(): ArtVariationID [{item.MercenaryArtVariation}][{item.MercenaryArtPremium}] is already unlocked by {existingUnlock}");
					}
				}
				else
				{
					if (item.MercenaryRecord == null)
					{
						break;
					}
					foreach (MercenaryArtVariationDbfRecord artVariation in item.MercenaryRecord.MercenaryArtVariations)
					{
						if (!artVariation.DefaultVariation)
						{
							continue;
						}
						MercenaryUnlock newUnlock2 = MercenaryUnlock.Create(record, taskIndex);
						foreach (MercenaryArtVariationPremiumDbfRecord premiumVariation2 in artVariation.MercenaryArtVariationPremiums)
						{
							if (premiumVariation2.Premium == MercenaryArtVariationPremium.MercenariesPremium.PREMIUM_NORMAL && !string.IsNullOrEmpty(premiumVariation2.CustomAcquireText))
							{
								newUnlock2.m_unlockType = MercenaryUnlock.UnlockType.Custom;
								newUnlock2.m_customAcquireText = premiumVariation2.CustomAcquireText;
							}
						}
						AddArtVariationUnlock(artVariation.ID, RewardItem.MercenariesPremium.PREMIUM_NORMAL, newUnlock2);
						break;
					}
				}
				break;
			case RewardItem.RewardType.MERCENARY_EQUIPMENT:
			{
				int mercEquipment = item.MercenaryEquipment;
				if (mercEquipment != 0)
				{
					if (m_equipmentUnlockByEquipmentId.TryGetValue(item.MercenaryEquipment, out var unlock))
					{
						Log.Lettuce.PrintError($"GameDbFIndex.OnVisitorTaskAdded(): EquipmentID [{mercEquipment}] is already unlocked by {unlock}");
					}
					else
					{
						m_equipmentUnlockByEquipmentId.Add(mercEquipment, MercenaryUnlock.Create(record, taskIndex));
					}
				}
				break;
			}
			}
		}
	}

	public int GetTaskChainIndexForTask(int taskID)
	{
		if (m_MercenariesTaskIndex.ContainsKey(taskID))
		{
			return m_MercenariesTaskIndex[taskID];
		}
		return -1;
	}

	public void PostProcessDbfLoad_Achievement()
	{
		foreach (AchievementDbfRecord record in GameDbf.Achievement.GetRecords())
		{
			OnAchievementAdded(record);
		}
	}

	public void OnAchievementAdded(AchievementDbfRecord record)
	{
		RewardListDbfRecord rewardList = record.RewardListRecord;
		if (rewardList == null)
		{
			return;
		}
		List<RewardItemDbfRecord> rewardItems = rewardList.RewardItems;
		if (rewardItems == null)
		{
			return;
		}
		foreach (RewardItemDbfRecord item in rewardItems)
		{
			if (item.MercenaryEquipment != 0)
			{
				int mercEquipment = item.MercenaryEquipment;
				if (m_equipmentUnlockByEquipmentId.TryGetValue(mercEquipment, out var unlock))
				{
					Log.Lettuce.PrintError($"GameDbFIndex.OnAchievementAdded(): EquipmentID [{mercEquipment}] is already unlocked by {unlock}");
				}
				else
				{
					m_equipmentUnlockByEquipmentId.Add(mercEquipment, MercenaryUnlock.Create(record));
				}
			}
		}
	}

	public void PostProcessDbfLoad_MercenaryArtVariation()
	{
		foreach (MercenaryArtVariationDbfRecord record in GameDbf.MercenaryArtVariation.GetRecords())
		{
			foreach (MercenaryArtVariationPremiumDbfRecord premiumRecord in record.MercenaryArtVariationPremiums)
			{
				if (!string.IsNullOrEmpty(premiumRecord.CustomAcquireText))
				{
					MercenaryUnlock customUnlock = new MercenaryUnlock(MercenaryUnlock.UnlockType.Custom, premiumRecord.CustomAcquireText);
					MercenaryUnlock existingUnlock = AddArtVariationUnlock(record.ID, premiumRecord.Premium, customUnlock);
					if (existingUnlock != null)
					{
						Log.Lettuce.PrintError($"GameDbFIndex.PostProcessDbfLoad_MercenaryArtVariation(): ArtVariationID [{record.ID}][{premiumRecord.Premium}] is already unlocked by {existingUnlock}");
					}
				}
				else if (premiumRecord.RewardTrack)
				{
					MercenaryUnlock rewardTrackUnlock = new MercenaryUnlock(MercenaryUnlock.UnlockType.RewardTrack);
					MercenaryUnlock existingUnlock2 = AddArtVariationUnlock(record.ID, premiumRecord.Premium, rewardTrackUnlock);
					if (existingUnlock2 != null)
					{
						Log.Lettuce.PrintError($"GameDbFIndex.PostProcessDbfLoad_MercenaryArtVariation(): ArtVariationID [{record.ID}][{premiumRecord.Premium}] is already unlocked by {existingUnlock2}");
					}
				}
			}
		}
	}

	public void PostProcessDbfLoad_MercenaryEquipmentUnlock()
	{
		m_equipmentUnlockByEquipmentId.Clear();
	}

	public MercenaryUnlock GetEquipmentUnlockFromEquipmentID(int equipmentId)
	{
		if (!m_equipmentUnlockByEquipmentId.TryGetValue(equipmentId, out var unlock))
		{
			Log.Lettuce.PrintError($"Missing: MercenaryEquipmentUnlock not found for EQUIPMENT database ID: {equipmentId}");
			return null;
		}
		return unlock;
	}

	public void PostProcessDbfLoad_MercenaryArtVariationUnlock()
	{
		m_UnlockByArtVariationIdAndPremium.Clear();
	}

	public MercenaryUnlock GetArtVariationUnlock(int artVariationId, TAG_PREMIUM tagPremium)
	{
		if (m_UnlockByArtVariationIdAndPremium.TryGetValue(artVariationId, out var byPremuim) && byPremuim.TryGetValue(tagPremium, out var unlock))
		{
			return unlock;
		}
		return MercenaryUnlock.FromPacks;
	}

	private MercenaryUnlock AddArtVariationUnlock(int artVariationId, TAG_PREMIUM tagPremium, MercenaryUnlock newUnlock)
	{
		if (!m_UnlockByArtVariationIdAndPremium.TryGetValue(artVariationId, out var byPremuim))
		{
			byPremuim = new Map<TAG_PREMIUM, MercenaryUnlock>();
			m_UnlockByArtVariationIdAndPremium.Add(artVariationId, byPremuim);
		}
		if (byPremuim.TryGetValue(tagPremium, out var existingUnlock))
		{
			return existingUnlock;
		}
		byPremuim.Add(tagPremium, newUnlock);
		return null;
	}

	private MercenaryUnlock AddArtVariationUnlock(int artVariationId, RewardItem.MercenariesPremium premium, MercenaryUnlock newUnlock)
	{
		TAG_PREMIUM tagPremium = TAG_PREMIUM.NORMAL;
		switch (premium)
		{
		case RewardItem.MercenariesPremium.PREMIUM_GOLDEN:
			tagPremium = TAG_PREMIUM.GOLDEN;
			break;
		case RewardItem.MercenariesPremium.PREMIUM_DIAMOND:
			tagPremium = TAG_PREMIUM.DIAMOND;
			break;
		}
		return AddArtVariationUnlock(artVariationId, tagPremium, newUnlock);
	}

	private MercenaryUnlock AddArtVariationUnlock(int artVariationId, MercenaryArtVariationPremium.MercenariesPremium premium, MercenaryUnlock newUnlock)
	{
		TAG_PREMIUM tagPremium = TAG_PREMIUM.NORMAL;
		switch (premium)
		{
		case MercenaryArtVariationPremium.MercenariesPremium.PREMIUM_GOLDEN:
			tagPremium = TAG_PREMIUM.GOLDEN;
			break;
		case MercenaryArtVariationPremium.MercenariesPremium.PREMIUM_DIAMOND:
			tagPremium = TAG_PREMIUM.DIAMOND;
			break;
		}
		return AddArtVariationUnlock(artVariationId, tagPremium, newUnlock);
	}

	public void PostProcessDbfLoad_MercenaryLevel()
	{
		m_maxMercenaryLevel = 0;
		foreach (LettuceMercenaryLevelDbfRecord record in GameDbf.LettuceMercenaryLevel.GetRecords())
		{
			m_maxMercenaryLevel = Math.Max(m_maxMercenaryLevel, record.Level);
		}
	}

	public void OnMercenaryLevelAdded(LettuceMercenaryLevelDbfRecord record)
	{
		m_maxMercenaryLevel = Math.Max(m_maxMercenaryLevel, record.Level);
	}

	public void OnMercenaryLevelRemoved(List<LettuceMercenaryLevelDbfRecord> records)
	{
		foreach (LettuceMercenaryLevelDbfRecord record in records)
		{
			if (record.Level == m_maxMercenaryLevel)
			{
				PostProcessDbfLoad_MercenaryLevel();
				break;
			}
		}
	}

	public int GetMercenaryMaxLevel()
	{
		return m_maxMercenaryLevel;
	}
}
