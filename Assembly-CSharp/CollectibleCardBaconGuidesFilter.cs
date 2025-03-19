using System.Collections.Generic;
using System.Linq;

public class CollectibleCardBaconGuidesFilter : CollectibleCardFilter
{
	private class GuideSkinComparer : IComparer<CollectibleCard>
	{
		public int Compare(CollectibleCard card1, CollectibleCard card2)
		{
			bool card1IsBob = !card1.GetEntityDef().HasTag(GAME_TAG.BACON_BOB_SKIN);
			bool card2IsBob = !card2.GetEntityDef().HasTag(GAME_TAG.BACON_BOB_SKIN);
			if (card1IsBob && !card2IsBob)
			{
				return -1;
			}
			if (card2IsBob && !card1IsBob)
			{
				return 1;
			}
			bool card1IsOwned = CollectionManager.Get().OwnsBattlegroundsGuideSkin(card1.CardDbId);
			bool card2IsOwned = CollectionManager.Get().OwnsBattlegroundsGuideSkin(card2.CardDbId);
			if (card1IsOwned && !card2IsOwned)
			{
				return -1;
			}
			if (card2IsOwned && !card1IsOwned)
			{
				return 1;
			}
			string name = card1.Name;
			string heroName2 = card2.Name;
			return name.CompareTo(heroName2);
		}
	}

	private int m_guidesPerPage = 6;

	private int m_guideCount;

	private int m_totalPages;

	private List<CollectibleCard> m_allBGGuides = new List<CollectibleCard>();

	private static GuideSkinComparer s_GuideSkinComparer = new GuideSkinComparer();

	public void Init(int guidesPerPage)
	{
		m_guidesPerPage = guidesPerPage;
	}

	public override void UpdateResults()
	{
		m_allBGGuides.Clear();
		List<CollectionManager.CollectibleCardFilterFunc> filterFuncs = new List<CollectionManager.CollectibleCardFilterFunc>();
		if (!string.IsNullOrEmpty(m_filterText))
		{
			filterFuncs.AddRange(FiltersFromSearchString(m_filterText));
		}
		List<string> battlegroundsCardIds = CollectionManager.Get().GetAllBattlegroundsGuideCardIds();
		for (int cardIndex = 0; cardIndex < battlegroundsCardIds.Count; cardIndex++)
		{
			string cardId = battlegroundsCardIds[cardIndex];
			EntityDef entityDef = DefLoader.Get().GetEntityDef(cardId);
			CollectibleCard card = new CollectibleCard(GameUtils.GetCardRecord(cardId), entityDef, TAG_PREMIUM.NORMAL);
			if (entityDef.HasTag(GAME_TAG.BACON_BOB_SKIN))
			{
				card.OwnedCount = (CollectionManager.Get().OwnsBattlegroundsGuideSkin(card.CardId) ? 1 : 0);
			}
			else
			{
				card.OwnedCount = 1;
			}
			card.SeenCount = card.OwnedCount;
			if (card.SeenCount > 0 && CollectionManager.Get().ShouldShowNewBattlegroundsGuideSkinGlow(cardId))
			{
				card.SeenCount = 0;
			}
			bool failedFilters = false;
			for (int filterIdx = 0; filterIdx < filterFuncs.Count; filterIdx++)
			{
				if (!filterFuncs[filterIdx](card))
				{
					failedFilters = true;
					break;
				}
			}
			if (!failedFilters)
			{
				m_allBGGuides.Add(card);
			}
		}
		m_allBGGuides.Sort(s_GuideSkinComparer);
		m_guideCount = m_allBGGuides.Count;
		m_totalPages = m_guideCount / m_guidesPerPage + ((m_guideCount % m_guidesPerPage > 0) ? 1 : 0);
	}

	public override List<CollectibleCard> GetPageContents(int page)
	{
		return m_allBGGuides.Skip(m_guidesPerPage * (page - 1)).Take(m_guidesPerPage).ToList();
	}

	public override List<CollectibleCard> GetFirstNonEmptyPage(out int collectionPage)
	{
		collectionPage = 0;
		for (int i = 0; i < GetTotalNumPages(); i++)
		{
			List<CollectibleCard> cardList = GetPageContents(i);
			if (cardList.Count > 0)
			{
				collectionPage = i;
				return cardList;
			}
		}
		return new List<CollectibleCard>();
	}

	public override int GetTotalNumPages()
	{
		return m_totalPages;
	}
}
