using System.Collections.Generic;
using Hearthstone.DataModels;
using Hearthstone.UI;
using PegasusShared;
using UnityEngine;

public class SideboardDeck : CollectionDeck
{
	public int OwnerCardDbId { get; }

	public DeckSideboardDataModel DataModel { get; }

	public CollectionDeck MainDeck { get; }

	public override DeckType Type => MainDeck.Type;

	public override FormatType FormatType => MainDeck.FormatType;

	public override string HeroCardID => MainDeck.HeroCardID;

	public TAG_SIDEBOARD_TYPE SideboardType { get; }

	public List<IDataModel> UIDataModels { get; }

	public SideboardDeck(string ownerCardId, TAG_PREMIUM premium, CollectionDeck mainDeck)
	{
		if (mainDeck == null)
		{
			Debug.LogError("SideboardDeck: mainDeck should not be null.");
			return;
		}
		MainDeck = mainDeck;
		base.Name = mainDeck.Name + ": Sideboard(" + ownerCardId + ")";
		OwnerCardDbId = GameUtils.TranslateCardIdToDbId(ownerCardId);
		SetRuneOrder(mainDeck.GetRuneOrder());
		int maxCards = 0;
		int minCards = 0;
		EntityDef entityDef = DefLoader.Get().GetEntityDef(OwnerCardDbId);
		if (entityDef != null)
		{
			maxCards = entityDef.GetTag(GAME_TAG.MAX_SIDEBOARD_CARDS);
			minCards = entityDef.GetTag(GAME_TAG.MIN_SIDEBOARD_CARDS);
		}
		SideboardType = entityDef.GetTag<TAG_SIDEBOARD_TYPE>(GAME_TAG.SIDEBOARD_TYPE);
		DataModel = new DeckSideboardDataModel
		{
			OwnerCardId = ownerCardId,
			MaxCards = maxCards,
			MinCards = minCards,
			Premium = premium,
			ButtonLabelText = GetSideboardButtonLabelForSideboardType(SideboardType)
		};
		UIDataModels = new List<IDataModel>();
		UIDataModels.Add(DataModel);
		DataModel.HeroClasses.AddRange(mainDeck.GetClasses());
	}

	public void UpdateInvalidCardsData(bool requiresValidation = true)
	{
		if (!requiresValidation)
		{
			DataModel.HasInvalidCards = false;
		}
		else
		{
			DataModel.HasInvalidCards = GetTotalInvalidCardCount(null) > 0 || GetTotalInvalidRuneCardCount() > 0;
		}
	}

	public override bool AddCard(string cardID, TAG_PREMIUM premium, bool allowInvalid = false, EntityDef entityDef = null, params DeckRule.RuleType[] ignoreRules)
	{
		if (GetTotalCardCountExcludingUnownedOfCardId(cardID) == DataModel.MaxCards)
		{
			Debug.LogWarning($"SideboardDeck.AddCard: Failed to add card. Cannot add more cards than the max of {DataModel.MaxCards}");
			return false;
		}
		bool result = base.AddCard(cardID, premium, allowInvalid, entityDef, ignoreRules);
		UpdateCardCount();
		return result;
	}

	public override List<TAG_CLASS> GetClasses()
	{
		return MainDeck.GetClasses();
	}

	public override List<TAG_CLASS> GetTouristClasses()
	{
		return MainDeck.GetTouristClasses();
	}

	public virtual void AutoFillSideboard()
	{
	}

	public override bool GetCardCountRange(out int min, out int max)
	{
		if (DataModel != null)
		{
			min = DataModel.MinCards;
			max = DataModel.MaxCards;
			return true;
		}
		Debug.LogError("GetMaxCardCount() - unable to get correct count, DataModel was unavailable");
		min = 0;
		max = 0;
		return false;
	}

	public virtual void InitCardCount()
	{
		UpdateCardCount();
	}

	protected void UpdateCardCount()
	{
		DataModel.CardCount = GetTotalCardCount();
	}

	private static string GetSideboardButtonLabelForSideboardType(TAG_SIDEBOARD_TYPE sideboardType)
	{
		return sideboardType switch
		{
			TAG_SIDEBOARD_TYPE.ETC => GameStrings.Get("GLUE_ETC_THE_BAND"), 
			TAG_SIDEBOARD_TYPE.ZILLIAX => GameStrings.Get("GLUE_ZILLIAX_SIDEBOARD_BUTTON"), 
			_ => "", 
		};
	}
}
