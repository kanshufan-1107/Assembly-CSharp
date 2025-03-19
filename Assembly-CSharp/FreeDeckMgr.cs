using System;
using System.Collections.Generic;
using Assets;
using Blizzard.T5.Jobs;
using Blizzard.T5.Services;
using Hearthstone;
using PegasusUtil;

public class FreeDeckMgr : IService
{
	public enum FreeDeckStatus
	{
		UNKNOWN,
		AVAILABLE,
		CLAIMED,
		EXPIRED,
		TRIAL_PERIOD
	}

	private Dictionary<int, CollectionDeck> m_collectionLoanerDecks;

	private Dictionary<int, DeckTemplateDbfRecord> m_loanerDeckTemplateMap;

	public FreeDeckStatus Status { get; private set; }

	public DateTime? TrialPeriodEndTime { get; private set; }

	public IEnumerator<IAsyncJobResult> Initialize(ServiceLocator serviceLocator)
	{
		HearthstoneApplication.Get().WillReset += WillReset;
		Network network = serviceLocator.Get<Network>();
		network.RegisterNetHandler(FreeDeckStateUpdate.PacketID.ID, OnFreeDeckStateUpdate);
		network.RegisterNetHandler(FreeDeckChoiceResponse.PacketID.ID, OnFreeDeckChoiceResponse);
		m_collectionLoanerDecks = new Dictionary<int, CollectionDeck>();
		m_loanerDeckTemplateMap = new Dictionary<int, DeckTemplateDbfRecord>();
		yield break;
	}

	public Type[] GetDependencies()
	{
		return new Type[2]
		{
			typeof(CheatMgr),
			typeof(Network)
		};
	}

	public void Shutdown()
	{
	}

	public static FreeDeckMgr Get()
	{
		return ServiceManager.Get<FreeDeckMgr>();
	}

	private void WillReset()
	{
		Status = FreeDeckStatus.UNKNOWN;
		TrialPeriodEndTime = null;
		m_collectionLoanerDecks = new Dictionary<int, CollectionDeck>();
		m_loanerDeckTemplateMap = new Dictionary<int, DeckTemplateDbfRecord>();
	}

	public bool IsLoanerDeckAvailableToClaim()
	{
		if (Status == FreeDeckStatus.AVAILABLE)
		{
			return true;
		}
		if (Status == FreeDeckStatus.TRIAL_PERIOD && TrialPeriodEndTime.HasValue)
		{
			return DateTime.Now >= TrialPeriodEndTime.Value;
		}
		return false;
	}

	public CollectionDeck GetLoanerDeckFromDeckTemplateId(int deckTemplateId)
	{
		if (Status == FreeDeckStatus.CLAIMED || Status == FreeDeckStatus.UNKNOWN)
		{
			return null;
		}
		if (m_collectionLoanerDecks == null || m_collectionLoanerDecks.Count == 0)
		{
			return null;
		}
		if (m_collectionLoanerDecks.ContainsKey(deckTemplateId))
		{
			return m_collectionLoanerDecks[deckTemplateId];
		}
		return null;
	}

	public Dictionary<int, CollectionDeck> GetLoanerDecksAsMap()
	{
		if (Status == FreeDeckStatus.CLAIMED || Status == FreeDeckStatus.UNKNOWN || Status == FreeDeckStatus.EXPIRED)
		{
			return null;
		}
		if (m_collectionLoanerDecks.Count == 0)
		{
			InitializeLoanerDeckData();
		}
		return m_collectionLoanerDecks;
	}

	public Dictionary<int, DeckTemplateDbfRecord> GetLoanerDeckTemplateMap()
	{
		if (m_loanerDeckTemplateMap.Count == 0)
		{
			InitializeLoanerDeckData();
		}
		return m_loanerDeckTemplateMap;
	}

	public int GetLoanerDecksCount()
	{
		return m_collectionLoanerDecks.Count;
	}

	private void OnFreeDeckStateUpdate()
	{
		FreeDeckStateUpdate packet = Network.Get().GetFreeDeckStateUpdate();
		if (packet != null)
		{
			Status = (FreeDeckStatus)packet.Status;
			if (packet.HasTrialPeriodSecondsRemaining)
			{
				TrialPeriodEndTime = DateTime.Now.AddSeconds(packet.TrialPeriodSecondsRemaining);
			}
			else
			{
				TrialPeriodEndTime = null;
			}
		}
	}

	private void OnFreeDeckChoiceResponse()
	{
		if (!Network.Get().GetFreeDeckChoiceResponse().Success)
		{
			AlertPopup.PopupInfo info = new AlertPopup.PopupInfo
			{
				m_headerText = GameStrings.Get("GLUE_FREE_DECK_ERROR_HEADER"),
				m_text = GameStrings.Get("GLUE_FREE_DECK_ERROR_TEXT"),
				m_showAlertIcon = false,
				m_alertTextAlignment = UberText.AlignmentOptions.Center,
				m_responseDisplay = AlertPopup.ResponseDisplay.OK
			};
			DialogManager.Get().ShowPopup(info);
		}
	}

	private void InitializeLoanerDeckData()
	{
		foreach (DeckTemplateDbfRecord deck in GameDbf.DeckTemplate.GetRecords())
		{
			if (deck.IsFreeReward && EventTimingManager.Get().IsEventActive(deck.Event))
			{
				m_collectionLoanerDecks.Add(deck.ID, CollectionDeck.Create(deck, DeckTemplate.SourceType.LOANER));
				m_loanerDeckTemplateMap.Add(deck.ID, deck);
			}
		}
	}
}
