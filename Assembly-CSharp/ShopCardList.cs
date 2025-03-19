using System.Collections.Generic;
using System.Linq;
using Hearthstone.DataModels;
using Hearthstone.UI;

public class ShopCardList
{
	private UIBScrollable m_scrollbar;

	private MiniSetDetailsDataModel m_dataModel;

	private CardTileDataModel m_clickedTile;

	private CardDataModel m_selectedCard = new CardDataModel
	{
		CardId = ""
	};

	private Widget m_parentWidget;

	public ShopCardList(Widget parentWidget, UIBScrollable scrollbar)
	{
		m_parentWidget = parentWidget;
		m_scrollbar = scrollbar;
	}

	public void InitInput()
	{
		m_parentWidget.RegisterEventListener(HandleMouseEvents);
		m_scrollbar.SetScrollImmediate(0f);
		m_scrollbar.AddTouchScrollStartedListener(BindNoCard);
	}

	public void RemoveListeners()
	{
		m_parentWidget.RemoveEventListener(HandleMouseEvents);
		m_scrollbar.RemoveTouchScrollStartedListener(BindNoCard);
	}

	public void SetData(IEnumerable<CardTileDataModel> cardList, BoosterDbId boosterId)
	{
		BindDataModel(cardList, boosterId);
	}

	public void SetData(IEnumerable<DeckCardDbfRecord> cardList, BoosterDbId boosterId)
	{
		DefLoader loader = DefLoader.Get();
		List<CardTileDataModel> cards = (from cr in cardList
			group cr by cr.CardId into g
			select (loader.GetEntityDef(g.Key), g.Count()) into ed
			orderby ed.Item1.GetRarity() descending, ed.Item1.GetCost()
			select new CardTileDataModel
			{
				CardId = ed.Item1.GetCardId(),
				Count = ed.Item2,
				Premium = TAG_PREMIUM.NORMAL
			}).ToList();
		foreach (DeckCardDbfRecord card in cardList)
		{
			List<SideboardCardDbfRecord> sideboardCards = card.SideboardCards;
			if (sideboardCards.Count <= 0)
			{
				continue;
			}
			int cardIndex = cards.FindIndex((CardTileDataModel dataModel) => GameUtils.TranslateCardIdToDbId(dataModel.CardId) == card.CardRecord.ID);
			List<CardTileDataModel> sideboardDataModels = new List<CardTileDataModel>(sideboardCards.Count);
			foreach (SideboardCardDbfRecord item in sideboardCards)
			{
				string currentSideboardCardId = GameUtils.TranslateDbIdToCardId(item.SideboardCardId);
				if (!loader.GetEntityDef(currentSideboardCardId).HasTag(GAME_TAG.ZILLIAX_CUSTOMIZABLE_COSMETICMODULE))
				{
					sideboardDataModels.Add(new CardTileDataModel
					{
						CardId = currentSideboardCardId,
						Premium = TAG_PREMIUM.NORMAL
					});
				}
			}
			cards.InsertRange(cardIndex + 1, sideboardDataModels);
		}
		BindDataModel(cards, boosterId);
	}

	public void SetDataGhostNonCraftableCards(List<DeckCardDbfRecord> cardList, BoosterDbId boosterId, TAG_PREMIUM premium)
	{
		CollectionManager collectionManager = CollectionManager.Get();
		IEnumerable<CardTileDataModel> cards = from ed in (from cr in cardList
				group cr by cr.CardId).Select(delegate(IGrouping<int, DeckCardDbfRecord> g)
			{
				string cardID = GameUtils.TranslateDbIdToCardId(g.Key);
				CollectibleCard card = collectionManager.GetCard(cardID, premium);
				return (card.GetEntityDef(), g.Count(), IsCraftable: card.IsCraftable);
			})
			orderby (!ed.IsCraftable) ? 1 : 0, ed.Item1.GetRarity() descending, ed.Item1.GetCost()
			select new CardTileDataModel
			{
				CardId = ed.Item1.GetCardId(),
				Count = ed.Item2,
				Premium = (ed.IsCraftable ? premium : TAG_PREMIUM.NORMAL),
				ForceGhostDisplayStyle = ((!ed.IsCraftable) ? CollectionDeckTileActor.GhostedState.NOT_INCLUDED : CollectionDeckTileActor.GhostedState.NONE)
			};
		BindDataModel(cards, boosterId);
		m_dataModel.SelectedCard.Premium = premium;
		m_selectedCard.Premium = premium;
	}

	private void BindDataModel(IEnumerable<CardTileDataModel> cards, BoosterDbId boosterId)
	{
		DataModelList<CardTileDataModel> dataModelList = new DataModelList<CardTileDataModel>();
		dataModelList.AddRange(cards);
		m_dataModel = new MiniSetDetailsDataModel
		{
			CardTiles = dataModelList,
			Pack = new PackDataModel
			{
				Type = boosterId
			},
			SelectedCard = m_selectedCard
		};
		BindNoCard();
		m_parentWidget.BindDataModel(m_dataModel);
	}

	private void BindNoCard()
	{
		m_selectedCard.CardId = "";
		m_dataModel.SelectedCardNotIncluded = false;
		if (m_clickedTile != null)
		{
			m_clickedTile.Selected = false;
			m_clickedTile = null;
		}
	}

	private CardTileDataModel GetEventPayload()
	{
		return m_parentWidget.GetDataModel<EventDataModel>().Payload as CardTileDataModel;
	}

	private void HandleMouseEvents(string eventName)
	{
		if (m_scrollbar.IsTouchDragging())
		{
			BindNoCard();
			return;
		}
		switch (eventName)
		{
		case "TILE_OVER_code":
		{
			CardTileDataModel cardDataModel = GetEventPayload();
			CollectionManager collectionManager = CollectionManager.Get();
			m_selectedCard.CardId = cardDataModel.CardId;
			m_selectedCard.Premium = cardDataModel.Premium;
			if (cardDataModel.ForceGhostDisplayStyle == CollectionDeckTileActor.GhostedState.NOT_INCLUDED)
			{
				CollectibleCard collectibleCard = collectionManager.GetCard(cardDataModel.CardId, cardDataModel.Premium);
				m_dataModel.NotIncludedText = GameStrings.Format("GLUE_SHOP_CARD_INCLUDED_FREE", GameStrings.GetCardSetName(collectibleCard.Set));
				m_dataModel.SelectedCardNotIncluded = true;
			}
			else
			{
				m_dataModel.SelectedCardNotIncluded = false;
			}
			break;
		}
		case "TILE_OUT_code":
			BindNoCard();
			break;
		case "TILE_CLICKED_code":
		{
			CardTileDataModel tileDataModel = GetEventPayload();
			tileDataModel.Selected = true;
			m_clickedTile = tileDataModel;
			break;
		}
		case "TILE_RELEASED_code":
			GetEventPayload().Selected = false;
			m_clickedTile = null;
			break;
		}
	}
}
