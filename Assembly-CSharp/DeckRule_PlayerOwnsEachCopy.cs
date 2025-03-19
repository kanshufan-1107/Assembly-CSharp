using System;
using System.Collections.Generic;
using Blizzard.T5.Core;

public class DeckRule_PlayerOwnsEachCopy : DeckRule
{
	public DeckRule_PlayerOwnsEachCopy(DeckRulesetRuleDbfRecord record)
		: base(RuleType.PLAYER_OWNS_EACH_COPY, record)
	{
	}

	public override bool CanAddToDeck(EntityDef def, TAG_PREMIUM premium, CollectionDeck deck, out RuleInvalidReason reason)
	{
		reason = null;
		string cardId = def.GetCardId();
		if (!AppliesTo(cardId))
		{
			return true;
		}
		CollectibleCard card = CollectionManager.Get().GetCard(cardId, premium);
		bool requireOwnership = IsCardOwnershipRequired(card.GetEntityDef());
		int ownedCopies = deck.GetOwnedCardCountInDeck(cardId, premium, requireOwnership);
		if (card == null || ownedCopies >= card.OwnedCount)
		{
			reason = new RuleInvalidReason(GameStrings.Get("GLUE_COLLECTION_LOCK_NO_MORE_INSTANCES"));
			return GetResult(val: false);
		}
		return GetResult(val: true);
	}

	private bool IsCardOwnershipRequired(EntityDef entityDef)
	{
		if (entityDef == null)
		{
			return false;
		}
		if (entityDef.GetTag(GAME_TAG.SIDEBOARD_TYPE) == 2)
		{
			return false;
		}
		return true;
	}

	private bool IsZilliaxDeluxe3000Piece(CardDbfRecord cardRecord)
	{
		if (cardRecord == null)
		{
			return false;
		}
		if (GameUtils.TryGetCardTagRecords(cardRecord.NoteMiniGuid, out var cardTags))
		{
			foreach (CardTagDbfRecord tagRecord in cardTags)
			{
				if (tagRecord.TagId == 3376 && tagRecord.TagValue > 0)
				{
					return true;
				}
				if (tagRecord.TagId == 3377 && tagRecord.TagValue > 0)
				{
					return true;
				}
			}
		}
		return false;
	}

	private bool PieceIsOwnedByZilliax(CardDbfRecord cardRecord, Dictionary<string, SideboardDeck> sideboards)
	{
		if (cardRecord == null || sideboards == null)
		{
			return false;
		}
		foreach (KeyValuePair<string, SideboardDeck> sideboard in sideboards)
		{
			if (sideboard.Value.OwnerCardDbId != 102983)
			{
				continue;
			}
			foreach (int card in sideboard.Value.GetCards())
			{
				if (card == cardRecord.ID)
				{
					return true;
				}
			}
		}
		return false;
	}

	public override bool IsDeckValid(CollectionDeck deck, out RuleInvalidReason reason)
	{
		reason = null;
		if (deck == null)
		{
			return false;
		}
		if (deck.Locked)
		{
			return true;
		}
		CollectionManager cm = CollectionManager.Get();
		Map<KeyValuePair<string, TAG_PREMIUM>, int> totalCountsInDeck = new Map<KeyValuePair<string, TAG_PREMIUM>, int>();
		UpdateTotalCardCountsInDeck(totalCountsInDeck, deck);
		foreach (KeyValuePair<string, SideboardDeck> allSideboard in deck.GetAllSideboards())
		{
			UpdateTotalCardCountsInDeck(totalCountsInDeck, allSideboard.Value);
		}
		int unownedCount = 0;
		foreach (KeyValuePair<KeyValuePair<string, TAG_PREMIUM>, int> kv in totalCountsInDeck)
		{
			string cardID = kv.Key.Key;
			TAG_PREMIUM premium = kv.Key.Value;
			int countInDeck = kv.Value;
			if (countInDeck == 0)
			{
				continue;
			}
			CollectibleCard card = cm.GetCard(cardID, premium);
			int ownCount = 0;
			bool isZilliaxPiece = false;
			if (card == null)
			{
				CardDbfRecord cardRecord = GameUtils.GetCardRecord(cardID);
				if (cardRecord != null)
				{
					bool num = IsZilliaxDeluxe3000Piece(cardRecord);
					bool pieceIsOwnedByZilliax = PieceIsOwnedByZilliax(cardRecord, deck.GetAllSideboards());
					isZilliaxPiece = num && pieceIsOwnedByZilliax;
				}
			}
			else
			{
				ownCount = card.OwnedCount;
			}
			if (ownCount < countInDeck && !isZilliaxPiece)
			{
				unownedCount += countInDeck - ownCount;
			}
		}
		bool result = GetResult(unownedCount == 0);
		if (!result)
		{
			reason = new RuleInvalidReason(GameStrings.Format("GLUE_COLLECTION_DECK_RULE_MISSING_CARDS", unownedCount), unownedCount);
		}
		return result;
	}

	private void UpdateTotalCardCountsInDeck(Map<KeyValuePair<string, TAG_PREMIUM>, int> totalCountsInDeck, CollectionDeck deck)
	{
		List<CollectionDeckSlot> slots = deck.GetSlots();
		for (int i = 0; i < slots.Count; i++)
		{
			CollectionDeckSlot slot = slots[i];
			if (slot.Count <= 0 || !AppliesTo(slot.CardID))
			{
				continue;
			}
			foreach (TAG_PREMIUM premium in Enum.GetValues(typeof(TAG_PREMIUM)))
			{
				KeyValuePair<string, TAG_PREMIUM> key = new KeyValuePair<string, TAG_PREMIUM>(slot.CardID, premium);
				int count = 0;
				totalCountsInDeck.TryGetValue(key, out count);
				totalCountsInDeck[key] = count + slot.GetCount(premium);
			}
		}
	}
}
