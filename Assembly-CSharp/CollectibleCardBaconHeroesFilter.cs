using System.Collections.Generic;
using System.Linq;
using Hearthstone.Progression;
using UnityEngine;

public class CollectibleCardBaconHeroesFilter : CollectibleCardFilter
{
	private class HeroSkinComparerAllMode : IComparer<CollectibleCard>
	{
		public int Compare(CollectibleCard card1, CollectibleCard card2)
		{
			string heroName1 = card1.Name;
			string baseHeroName1 = heroName1;
			bool card1IsSkin = card1.GetEntityDef().HasTag(GAME_TAG.BACON_SKIN);
			if (card1IsSkin)
			{
				int baseCardDBIId = card1.GetEntityDef().GetTag(GAME_TAG.BACON_SKIN_PARENT_ID);
				string baseCardId = GameUtils.TranslateDbIdToCardId(baseCardDBIId);
				if (baseCardId == null)
				{
					Debug.LogError($"BattlegroundsCollectibleCardHeroesFilter.HeroSkinComparer: Could not find card with asset ID {baseCardDBIId} in our card manifest");
					return 0;
				}
				baseHeroName1 = GameUtils.GetCardRecord(baseCardId).Name;
			}
			string heroName2 = card2.Name;
			string baseHeroName2 = heroName2;
			bool card2IsSkin = card2.GetEntityDef().HasTag(GAME_TAG.BACON_SKIN);
			if (card2IsSkin)
			{
				int baseCardDBIId2 = card2.GetEntityDef().GetTag(GAME_TAG.BACON_SKIN_PARENT_ID);
				string baseCardId2 = GameUtils.TranslateDbIdToCardId(baseCardDBIId2);
				if (baseCardId2 == null)
				{
					Debug.LogError($"BattlegroundsCollectibleCardHeroesFilter.HeroSkinComparer: Could not find card with asset ID {baseCardDBIId2} in our card manifest");
					return 0;
				}
				baseHeroName2 = GameUtils.GetCardRecord(baseCardId2).Name;
			}
			if (baseHeroName1 != baseHeroName2)
			{
				return baseHeroName1.CompareTo(baseHeroName2);
			}
			if (card1IsSkin && !card2IsSkin)
			{
				return 1;
			}
			if (card2IsSkin && !card1IsSkin)
			{
				return -1;
			}
			return heroName1.CompareTo(heroName2);
		}
	}

	private class HeroSkinComparerDefaultMode : IComparer<CollectibleCard>
	{
		public int Compare(CollectibleCard card1, CollectibleCard card2)
		{
			return s_HeroSkinComparerAllMode.Compare(card1, card2);
		}
	}

	private int m_heroesPerPage = 6;

	private int m_heroCount;

	private int m_totalPages;

	private List<CollectibleCard> m_allBGHeroes = new List<CollectibleCard>();

	private static IComparer<CollectibleCard> s_HeroSkinComparerAllMode = new HeroSkinComparerAllMode();

	private static IComparer<CollectibleCard> s_HeroSkinComparerDefaultMode = new HeroSkinComparerDefaultMode();

	public void Init(int heroesPerPage)
	{
		m_heroesPerPage = heroesPerPage;
	}

	public override void UpdateResults()
	{
		CollectionUtils.BattlegroundsHeroSkinFilterMode currentFilterMode = (CollectionManager.Get().GetCollectibleDisplay() as BaconCollectionDisplay).GetHeroSkinFilterMode();
		m_allBGHeroes.Clear();
		List<CollectionManager.CollectibleCardFilterFunc> filterFuncs = new List<CollectionManager.CollectibleCardFilterFunc>();
		if (!string.IsNullOrEmpty(m_filterText))
		{
			filterFuncs.AddRange(FiltersFromSearchString(m_filterText));
		}
		List<string> battlegroundsCardIds = CollectionManager.Get().GetAllBattlegroundsHeroCardIds();
		for (int cardIndex = 0; cardIndex < battlegroundsCardIds.Count; cardIndex++)
		{
			string cardId = battlegroundsCardIds[cardIndex];
			EntityDef entityDef = DefLoader.Get().GetEntityDef(cardId);
			CollectibleCard card = new CollectibleCard(GameUtils.GetCardRecord(cardId), entityDef, TAG_PREMIUM.NORMAL);
			if (entityDef.HasTag(GAME_TAG.BACON_SKIN))
			{
				card.OwnedCount = (CollectionManager.Get().OwnsBattlegroundsHeroSkin(card.CardId) ? 1 : 0);
			}
			else
			{
				card.OwnedCount = 1;
			}
			if (card.OwnedCount == 0 && currentFilterMode == CollectionUtils.BattlegroundsHeroSkinFilterMode.DEFAULT)
			{
				continue;
			}
			card.SeenCount = card.OwnedCount;
			if (card.SeenCount > 0 && CollectionManager.Get().ShouldShowNewBattlegroundsHeroSkinGlow(cardId))
			{
				card.SeenCount = 0;
			}
			string baseHeroCardId = CollectionManager.Get().GetBattlegroundsBaseHeroCardId(cardId);
			CardDbfRecord baseCardRecord = GameUtils.GetCardRecord(baseHeroCardId);
			EntityDef baseEntityDef = DefLoader.Get().GetEntityDef(baseHeroCardId);
			CollectibleCard baseHeroCard = new CollectibleCard(baseCardRecord, baseEntityDef, TAG_PREMIUM.NORMAL);
			if ((BaconHeroSkinUtils.GetBattleGroundsHeroRotationType(baseCardRecord, baseEntityDef) == BaconHeroSkinUtils.RotationType.Resting && (currentFilterMode == CollectionUtils.BattlegroundsHeroSkinFilterMode.DEFAULT || entityDef.HasTag(GAME_TAG.BACON_OMIT_WHEN_OUT_OF_ROTATION))) || (currentFilterMode == CollectionUtils.BattlegroundsHeroSkinFilterMode.DEFAULT && !RewardTrackManager.Get().HasBattlegroundsPreviewHeroes() && BaconHeroSkinUtils.GetBattleGroundsHeroRotationType(baseCardRecord, baseEntityDef) == BaconHeroSkinUtils.RotationType.Preview))
			{
				continue;
			}
			bool failedFilters = false;
			for (int filterIdx = 0; filterIdx < filterFuncs.Count; filterIdx++)
			{
				if (!filterFuncs[filterIdx](card) && !filterFuncs[filterIdx](baseHeroCard))
				{
					failedFilters = true;
					break;
				}
			}
			if (!failedFilters)
			{
				m_allBGHeroes.Add(card);
			}
		}
		switch (currentFilterMode)
		{
		case CollectionUtils.BattlegroundsHeroSkinFilterMode.DEFAULT:
			m_allBGHeroes.Sort(s_HeroSkinComparerDefaultMode);
			break;
		case CollectionUtils.BattlegroundsHeroSkinFilterMode.ALL:
			m_allBGHeroes.Sort(s_HeroSkinComparerAllMode);
			break;
		default:
			Log.CollectionManager.PrintError("Battlegrounds heroes filtered by an unknown mode type.");
			break;
		}
		m_heroCount = m_allBGHeroes.Count;
		m_totalPages = m_heroCount / m_heroesPerPage + ((m_heroCount % m_heroesPerPage > 0) ? 1 : 0);
	}

	public override List<CollectibleCard> GetPageContents(int page)
	{
		return m_allBGHeroes.Skip(m_heroesPerPage * (page - 1)).Take(m_heroesPerPage).ToList();
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
