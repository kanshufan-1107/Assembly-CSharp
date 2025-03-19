using System.Collections.Generic;

public class SideboardManager
{
	private readonly Dictionary<string, SideboardDeck> m_sideboards;

	private readonly Dictionary<string, SideboardDeck> m_removedSideboards;

	private string m_editedSideboardId;

	public CollectionDeck MainDeck { get; }

	public bool HasSideboardCards()
	{
		int count = 0;
		foreach (KeyValuePair<string, SideboardDeck> sideboard in m_sideboards)
		{
			count += sideboard.Value.GetSlotCount();
		}
		return count > 0;
	}

	public SideboardManager(CollectionDeck mainDeck)
	{
		MainDeck = mainDeck;
		m_sideboards = new Dictionary<string, SideboardDeck>();
		m_removedSideboards = new Dictionary<string, SideboardDeck>();
	}

	public SideboardDeck SetEditedSideboard(string ownerId, TAG_PREMIUM premium)
	{
		if (ownerId == null)
		{
			return null;
		}
		m_editedSideboardId = ownerId;
		return GetOrCreateSideboard(m_editedSideboardId, premium, setEdited: true);
	}

	public SideboardDeck GetSideboard(string ownerCardId)
	{
		if (ownerCardId == null)
		{
			return null;
		}
		if (!m_sideboards.TryGetValue(ownerCardId, out var sideboard))
		{
			return null;
		}
		return sideboard;
	}

	public SideboardDeck GetCurrentSideboardDeck()
	{
		return GetSideboard(m_editedSideboardId);
	}

	public SideboardDeck GetOrCreateSideboard(string ownerCardId, TAG_PREMIUM premium, bool setEdited)
	{
		EntityDef entityDef = DefLoader.Get().GetEntityDef(ownerCardId);
		if (entityDef != null && !entityDef.HasSideboard)
		{
			return null;
		}
		SideboardDeck sideboard = GetSideboard(ownerCardId);
		if (sideboard == null)
		{
			sideboard = ((GameUtils.GetCardTagValue(ownerCardId, GAME_TAG.SIDEBOARD_TYPE) != 2) ? new SideboardDeck(ownerCardId, premium, MainDeck) : new ZilliaxSideboardDeck(ownerCardId, premium, MainDeck));
			m_sideboards.Add(ownerCardId, sideboard);
			if (m_removedSideboards.TryGetValue(ownerCardId, out var oldSideboard))
			{
				sideboard.AddCardsFrom(oldSideboard);
				m_removedSideboards.Remove(ownerCardId);
			}
		}
		else
		{
			sideboard.DataModel.Premium = premium;
		}
		if (setEdited)
		{
			m_editedSideboardId = ownerCardId;
		}
		return sideboard;
	}

	public void ClearSideboards()
	{
		m_sideboards.Clear();
	}

	public int GetCardIdCount(string cardID, bool includeUnowned)
	{
		int count = 0;
		foreach (KeyValuePair<string, SideboardDeck> sideboard in m_sideboards)
		{
			foreach (CollectionDeckSlot slot in sideboard.Value.GetSlots())
			{
				if (slot.CardID.Equals(cardID) && (includeUnowned || slot.Owned))
				{
					count += slot.Count;
				}
			}
		}
		return count;
	}

	public int GetCardCountHasTag(GAME_TAG tagName)
	{
		int count = 0;
		foreach (KeyValuePair<string, SideboardDeck> sideboard in m_sideboards)
		{
			foreach (CollectionDeckSlot slot in sideboard.Value.GetSlots())
			{
				if (GameUtils.GetCardHasTag(slot.CardID, tagName))
				{
					count += slot.Count;
				}
			}
		}
		return count;
	}

	public Dictionary<string, SideboardDeck> GetAllSideboards()
	{
		return m_sideboards;
	}

	public void AddCard(string cardId, int ownerCardDbId, TAG_PREMIUM cardPremium, bool allowInvalid)
	{
		string ownerCardId = GameUtils.TranslateDbIdToCardId(ownerCardDbId);
		GetOrCreateSideboard(ownerCardId, cardPremium, setEdited: false).AddCard(cardId, cardPremium, allowInvalid, null);
	}

	public void AddCardWithPrefferedPremium(string cardId, int ownerCardDbId, TAG_PREMIUM cardPremium, bool allowInvalid)
	{
		string ownerCardId = GameUtils.TranslateDbIdToCardId(ownerCardDbId);
		SideboardDeck sideboard = GetOrCreateSideboard(ownerCardId, cardPremium, setEdited: false);
		TAG_PREMIUM? prefferedPremium = sideboard.GetPreferredPremiumThatCanBeAdded(cardId).GetValueOrDefault();
		sideboard.AddCard(cardId, prefferedPremium.Value, allowInvalid, null);
	}

	public void CopySideboard(SideboardDeck otherSideboard)
	{
		SideboardDeck sideboard = GetOrCreateSideboard(otherSideboard.DataModel.OwnerCardId, otherSideboard.DataModel.Premium, setEdited: false);
		sideboard.ClearSlotContents();
		List<CollectionDeckSlot> slots = new List<CollectionDeckSlot>();
		for (int i = 0; i < otherSideboard.GetSlotCount(); i++)
		{
			CollectionDeckSlot otherSlot = otherSideboard.GetSlotByIndex(i);
			CollectionDeckSlot slot = new CollectionDeckSlot();
			slot.CopyFrom(otherSlot);
			slots.Add(slot);
		}
		sideboard.AddSlots(slots);
	}

	public List<SideboardDeck> GetIncompleteSideboards()
	{
		List<SideboardDeck> results = new List<SideboardDeck>();
		foreach (KeyValuePair<string, SideboardDeck> kvp in m_sideboards)
		{
			if (kvp.Value.GetTotalCardCount() < kvp.Value.DataModel.MaxCards || kvp.Value.GetTotalInvalidCardCount(null) > 0 || kvp.Value.GetTotalInvalidRuneCardCount() > 0)
			{
				results.Add(kvp.Value);
			}
		}
		return results;
	}

	public void ClearEditedSideboard()
	{
		m_editedSideboardId = null;
	}

	public void RemoveSideboard(string ownerCardId)
	{
		SideboardDeck sideboard = GetSideboard(ownerCardId);
		if (sideboard != null)
		{
			m_removedSideboards[ownerCardId] = sideboard;
			m_sideboards.Remove(ownerCardId);
			if (m_editedSideboardId == ownerCardId)
			{
				m_editedSideboardId = null;
			}
		}
	}

	public void ClearRemovedSideboards()
	{
		m_removedSideboards.Clear();
	}
}
