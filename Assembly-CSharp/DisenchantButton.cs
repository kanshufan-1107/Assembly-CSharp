using System.Collections.Generic;
using Assets;
using PegasusShared;
using UnityEngine;

public class DisenchantButton : CraftingButton
{
	private string m_lastwarnedCard;

	private List<AlertPopup.PopupInfo> PendingDisenchantWarnings = new List<AlertPopup.PopupInfo>();

	public override void EnableButton()
	{
		if (CraftingManager.Get().GetPendingClientTransaction().GetLastTransactionWasCrafting())
		{
			base.EnterUndoMode();
			return;
		}
		SetCraftingState(CraftingState.Disenchant);
		SetLabelText(GameStrings.Get("GLUE_CRAFTING_DISENCHANT"));
		base.EnableButton();
	}

	protected override void OnRelease()
	{
		if (!Network.IsLoggedIn())
		{
			CollectionManager.ShowFeatureDisabledWhileOfflinePopup();
		}
		else if (CraftingManager.Get().GetPendingServerTransaction() == null)
		{
			if ((bool)UniversalInputManager.UsePhoneUI)
			{
				GetComponent<Animation>().Play("CardExchange_ButtonPress1_phone");
			}
			else
			{
				GetComponent<Animation>().Play("CardExchange_ButtonPress1");
			}
			if (CraftingManager.Get().GetPendingClientTransaction().GetLastTransactionWasCrafting())
			{
				DoDisenchant();
			}
			else
			{
				CollectionManager.Get().RequestDeckContentsForDecksWithoutContentsLoaded(OnReadyToStartDisenchant);
			}
		}
	}

	private void OnReadyToStartDisenchant()
	{
		if (!CraftingManager.Get().IsCardShowing())
		{
			return;
		}
		string cardID = CraftingManager.Get().GetShownActor().GetEntityDef()
			.GetCardId();
		List<string> affectedDeckNames = GetPostDisenchantInvalidDeckNames();
		bool hasExtraCopies = CraftingManager.Get().GetNumOwnedIncludePending(null) > CollectionManager.Get().GetCard(cardID, TAG_PREMIUM.NORMAL).DefaultMaxCopiesPerDeck;
		if (affectedDeckNames.Count == 0)
		{
			if (CraftingManager.Get().GetNumClientTransactions() <= 0 && m_lastwarnedCard != cardID && !hasExtraCopies)
			{
				m_lastwarnedCard = cardID;
				AlertPopup.PopupInfo dInfo = new AlertPopup.PopupInfo();
				dInfo.m_headerText = GameStrings.Get("GLUE_CRAFTING_DISENCHANT_CONFIRM_HEADER");
				dInfo.m_text = GameStrings.Get("GLUE_CRAFTING_DISENCHANT_CONFIRM2_DESC");
				dInfo.m_showAlertIcon = true;
				dInfo.m_responseDisplay = AlertPopup.ResponseDisplay.CONFIRM_CANCEL;
				dInfo.m_responseCallback = OnConfirmDisenchantResponse;
				PendingDisenchantWarnings.Add(dInfo);
			}
		}
		else
		{
			string confirmDesc = GameStrings.Get("GLUE_CRAFTING_DISENCHANT_CONFIRM_DESC");
			foreach (string deckName in affectedDeckNames)
			{
				confirmDesc = confirmDesc + "\n" + deckName;
			}
			AlertPopup.PopupInfo info = new AlertPopup.PopupInfo();
			info.m_headerText = GameStrings.Get("GLUE_CRAFTING_DISENCHANT_CONFIRM_HEADER");
			info.m_text = confirmDesc;
			info.m_showAlertIcon = false;
			info.m_responseDisplay = AlertPopup.ResponseDisplay.CONFIRM_CANCEL;
			info.m_responseCallback = OnConfirmDisenchantResponse;
			PendingDisenchantWarnings.Add(info);
		}
		CraftingManager.GetCardValue(cardID, CraftingManager.Get().GetShownActor().GetPremium(), out var cardValueDbfRecord);
		if (cardValueDbfRecord != null && cardValueDbfRecord.SellState == CardValue.SellState.PERMANENT_OVERRIDE_USE_CUSTOM_VALUE)
		{
			AlertPopup.PopupInfo info2 = new AlertPopup.PopupInfo();
			info2.m_headerText = GameStrings.Get("GLUE_CRAFTING_DISENCHANT_CONFIRM_HEADER");
			info2.m_text = GameStrings.Get("GLUE_CRAFTING_DISENCHANT_CONFIRM4_DESC");
			info2.m_showAlertIcon = true;
			info2.m_responseDisplay = AlertPopup.ResponseDisplay.CONFIRM_CANCEL;
			info2.m_responseCallback = OnConfirmDisenchantResponse;
			PendingDisenchantWarnings.Add(info2);
		}
		if (PendingDisenchantWarnings.Count > 0)
		{
			ShowNextDisenchantWarning();
		}
		else
		{
			DoDisenchant();
		}
	}

	private void ShowNextDisenchantWarning()
	{
		if (PendingDisenchantWarnings.Count != 0)
		{
			AlertPopup.PopupInfo currentWarning = PendingDisenchantWarnings[0];
			PendingDisenchantWarnings.RemoveAt(0);
			DialogManager.Get().ShowPopup(currentWarning);
		}
	}

	private void OnConfirmDisenchantResponse(AlertPopup.Response response, object userData)
	{
		if (response == AlertPopup.Response.CANCEL)
		{
			PendingDisenchantWarnings.Clear();
		}
		else if (PendingDisenchantWarnings.Count > 0)
		{
			ShowNextDisenchantWarning();
		}
		else
		{
			DoDisenchant();
		}
	}

	private void DoDisenchant()
	{
		CraftingManager.Get().DisenchantButtonPressed();
	}

	private List<string> GetPostDisenchantInvalidDeckNames()
	{
		Actor shownActor = CraftingManager.Get().GetShownActor();
		TAG_PREMIUM premium = shownActor.GetPremium();
		string cardID = shownActor.GetEntityDef().GetCardId();
		int counterpartCardId = GameUtils.GetFixedRewardCounterpartCardID(GameUtils.TranslateCardIdToDbId(cardID));
		if (counterpartCardId != 0 && GameUtils.IsClassicCard(counterpartCardId))
		{
			cardID = GameUtils.TranslateDbIdToCardId(counterpartCardId);
		}
		int totalNumCopiesInCollection = CollectionManager.Get().GetTotalNumCopiesInCollection(cardID);
		int numCopiesInCollection = CollectionManager.Get().GetNumCopiesInCollection(cardID, premium);
		int numCopiesOwnedPending = CraftingManager.Get().GetNumOwnedIncludePending(premium);
		if (numCopiesOwnedPending > 0)
		{
			numCopiesOwnedPending--;
		}
		int numCopiesBeingDisenchanted = numCopiesInCollection - numCopiesOwnedPending;
		int totalCopiesOwnedAfterDisenchant = totalNumCopiesInCollection - numCopiesBeingDisenchanted;
		List<string> affectedDeckNames = new List<string>();
		foreach (CollectionDeck deck in CollectionManager.Get().GetDecks(DeckType.NORMAL_DECK))
		{
			if (deck.GetOwnedCardCountInDeck(cardID, premium) > totalCopiesOwnedAfterDisenchant)
			{
				affectedDeckNames.Add(deck.Name);
			}
		}
		return affectedDeckNames;
	}
}
