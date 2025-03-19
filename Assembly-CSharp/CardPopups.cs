using System;
using System.Collections.Generic;
using System.Linq;
using Assets;

public class CardPopups : IDisposable
{
	private bool m_hasBeenInitialized;

	private Dictionary<(EventTimingType, bool), List<CardChangeDbfRecord>> m_cardChangesPerEventId = new Dictionary<(EventTimingType, bool), List<CardChangeDbfRecord>>();

	private List<(EventTimingType, bool)> m_cardChangeEventsToDisplayQueue;

	public void Dispose()
	{
	}

	public bool ShowChangedCards(bool shouldDisableNotificationOnLogin = false, UserAttentionBlocker ignoredAttentionBlockers = UserAttentionBlocker.NONE)
	{
		if (!m_hasBeenInitialized)
		{
			if (NetCache.Get().GetNetObject<NetCache.NetCacheHeroLevels>() == null)
			{
				return false;
			}
			InitializeCardChangeEvents(shouldDisableNotificationOnLogin);
			m_hasBeenInitialized = true;
		}
		if (m_cardChangeEventsToDisplayQueue == null || m_cardChangeEventsToDisplayQueue.Count == 0)
		{
			return false;
		}
		if (!UserAttentionManager.CanShowAttentionGrabber(ignoredAttentionBlockers, "ShowChangedCards"))
		{
			return false;
		}
		return ShowPopup(ignoredAttentionBlockers);
	}

	public bool ForceShowChangedCardEvent(EventTimingType eventToShowOverride = EventTimingType.UNKNOWN)
	{
		ClearCardChanges();
		InitializeCardChangeEvents(shouldDisableNotificationOnLogin: false, ignoreSeen: false, showOnlyActive: true, eventToShowOverride);
		if (m_cardChangeEventsToDisplayQueue == null || m_cardChangeEventsToDisplayQueue.Count == 0)
		{
			return false;
		}
		return ShowPopup();
	}

	public bool ShowFeaturedCards(EventTimingType featuredCardsEvent, string headerText, DialogBase.HideCallback callbackOnHide = null, UserAttentionBlocker ignoredAttentionBlockers = UserAttentionBlocker.NONE)
	{
		if (!UserAttentionManager.CanShowAttentionGrabber(ignoredAttentionBlockers, "ShowFeaturedCards"))
		{
			return false;
		}
		MultiPagePopup.Info popupInfo = new MultiPagePopup.Info
		{
			m_callbackOnHide = callbackOnHide,
			m_blurWhenShown = true
		};
		List<int> cardsToShow = (from r in GameDbf.GetIndex().GetCardsWithFeaturedCardsEvent()
			where r.FeaturedCardsEvent == featuredCardsEvent
			select r.ID).ToList();
		MultiPagePopup.PageInfo newCardsPage = new MultiPagePopup.PageInfo
		{
			m_pageType = MultiPagePopup.PageType.CARD_LIST,
			m_cards = cardsToShow,
			m_headerText = headerText
		};
		popupInfo.m_pages.Add(newCardsPage);
		DialogManager.Get().ShowMultiPagePopup(UserAttentionBlocker.NONE, popupInfo);
		return true;
	}

	public bool MarkCardChangeEventAsSeen(EventTimingType specialEventType)
	{
		GameSaveDataManager.Get().GetSubkeyValue(GameSaveKeyId.PLAYER_OPTIONS, GameSaveKeySubkeyId.LIST_OF_SEEN_CARD_CHANGES, out List<long> seenCardChangeEventIds);
		if (seenCardChangeEventIds == null)
		{
			seenCardChangeEventIds = new List<long>();
		}
		if (seenCardChangeEventIds.Count == 100)
		{
			seenCardChangeEventIds.RemoveAt(0);
		}
		long eventIdToAddToGSD = EventTimingManager.Get().GetEventIdFromEventName(specialEventType);
		seenCardChangeEventIds.Add(eventIdToAddToGSD);
		return GameSaveDataManager.Get().SaveSubkey(new GameSaveDataManager.SubkeySaveRequest(GameSaveKeyId.PLAYER_OPTIONS, GameSaveKeySubkeyId.LIST_OF_SEEN_CARD_CHANGES, seenCardChangeEventIds.ToArray()));
	}

	private bool ShowPopup(UserAttentionBlocker ignoredAttentionBlockers = UserAttentionBlocker.NONE)
	{
		(EventTimingType, bool) eventToShow = m_cardChangeEventsToDisplayQueue[0];
		m_cardChangeEventsToDisplayQueue.RemoveAt(0);
		Dictionary<int, List<CardChangeDbfRecord>> changes = (from r in m_cardChangesPerEventId[eventToShow]
			group r by r.CardId).ToDictionary((IGrouping<int, CardChangeDbfRecord> r) => r.Key, (IGrouping<int, CardChangeDbfRecord> r) => r.ToList());
		List<int> cardsToShow = changes.Keys.ToList();
		if (eventToShow.Item2)
		{
			CardListPopup.Info info = new CardListPopup.Info();
			info.m_description = ((cardsToShow.Count == 1) ? GameStrings.Get("GLUE_SINGLE_CARD_ADDED") : GameStrings.Format("GLUE_CARDS_ADDED", cardsToShow.Count));
			info.m_cards = cardsToShow;
			info.m_changes = changes;
			info.m_callbackOnHide = delegate
			{
				MarkCardChangeEventAsSeen(eventToShow.Item1);
			};
			CardListPopup.Info info2 = info;
			info2.m_useMultiLineDescription = info2.m_description.Contains('\n');
			DialogManager.Get().ShowCardListPopup(ignoredAttentionBlockers, info2);
		}
		else
		{
			CardListPopup.Info info3 = new CardListPopup.Info
			{
				m_description = GameStrings.Get((cardsToShow.Count == 1) ? "GLUE_SINGLE_CARD_UPDATED" : "GLUE_CARDS_UPDATED"),
				m_cards = cardsToShow,
				m_changes = changes,
				m_callbackOnHide = delegate
				{
					MarkCardChangeEventAsSeen(eventToShow.Item1);
				}
			};
			info3.m_useMultiLineDescription = info3.m_description.Contains('\n');
			DialogManager.Get().ShowCardListPopup(ignoredAttentionBlockers, info3);
		}
		return true;
	}

	private void InitializeCardChangeEvents(bool shouldDisableNotificationOnLogin = false, bool ignoreSeen = true, bool showOnlyActive = true, EventTimingType eventToShowOverride = EventTimingType.UNKNOWN)
	{
		bool suppressPopup = ReturningPlayerMgr.Get().SuppressOldPopups || !GameUtils.HasCompletedApprentice() || shouldDisableNotificationOnLogin;
		if (eventToShowOverride == EventTimingType.UNKNOWN)
		{
			PopulateActiveEvents(ignoreSeen, showOnlyActive, suppressPopup);
		}
		else
		{
			PopulateSpecificEvent(eventToShowOverride);
		}
		if (suppressPopup)
		{
			return;
		}
		Dictionary<TAG_CLASS, int> orderedClasses = new Dictionary<TAG_CLASS, int>();
		for (int i = 0; i < GameUtils.ORDERED_HERO_CLASSES.Length; i++)
		{
			orderedClasses.Add(GameUtils.ORDERED_HERO_CLASSES[i], i);
		}
		orderedClasses.Add(TAG_CLASS.NEUTRAL, GameUtils.ORDERED_HERO_CLASSES.Length + 1);
		foreach (var key in m_cardChangesPerEventId.Keys.ToList())
		{
			if (m_cardChangesPerEventId.TryGetValue(key, out var listOfChanges))
			{
				m_cardChangesPerEventId[key] = (from r in listOfChanges
					orderby r.SortOrder, orderedClasses[DefLoader.Get().GetEntityDef(r.CardId).GetClass()], DefLoader.Get().GetEntityDef(r.CardId).GetRarity() descending
					select r).ToList();
			}
		}
		m_cardChangeEventsToDisplayQueue = m_cardChangesPerEventId.Keys.OrderBy(((EventTimingType, bool) k) => EventTimingManager.Get().GetEventStartTimeUtc(k.Item1)).ToList();
	}

	private HashSet<long> GetSeenEvents()
	{
		GameSaveDataManager.Get().GetSubkeyValue(GameSaveKeyId.PLAYER_OPTIONS, GameSaveKeySubkeyId.LIST_OF_SEEN_CARD_CHANGES, out List<long> seenCardChangeEventIds);
		if (seenCardChangeEventIds == null)
		{
			seenCardChangeEventIds = new List<long>();
		}
		return new HashSet<long>(seenCardChangeEventIds);
	}

	private void PopulateActiveEvents(bool ignoreSeen, bool showOnlyActive, bool suppressPopup)
	{
		HashSet<long> seenEventSet = new HashSet<long>();
		if (ignoreSeen)
		{
			seenEventSet = GetSeenEvents();
		}
		foreach (CardChangeDbfRecord cardChangeRecord in GameDbf.CardChange.GetRecords())
		{
			long eventId = EventTimingManager.Get().GetEventIdFromEventName(cardChangeRecord.Event);
			if ((!ignoreSeen || !seenEventSet.Contains(eventId)) && (!showOnlyActive || EventTimingManager.Get().IsEventActive(cardChangeRecord.Event)))
			{
				if (suppressPopup)
				{
					MarkCardChangeEventAsSeen(cardChangeRecord.Event);
				}
				else
				{
					AddCardChangeRecordToDictionary(cardChangeRecord);
				}
			}
		}
	}

	private void PopulateSpecificEvent(EventTimingType eventToShow)
	{
		foreach (CardChangeDbfRecord cardChangeRecord in GameDbf.CardChange.GetRecords())
		{
			if (cardChangeRecord.Event == eventToShow)
			{
				AddCardChangeRecordToDictionary(cardChangeRecord);
			}
		}
	}

	private void AddCardChangeRecordToDictionary(CardChangeDbfRecord cardChangeRecord)
	{
		(EventTimingType, bool) key = (cardChangeRecord.Event, cardChangeRecord.ChangeType == CardChange.ChangeType.ADDITION);
		if (m_cardChangesPerEventId.TryGetValue(key, out var listOfChanges))
		{
			listOfChanges.Add(cardChangeRecord);
			return;
		}
		List<CardChangeDbfRecord> newList = new List<CardChangeDbfRecord>();
		newList.Add(cardChangeRecord);
		m_cardChangesPerEventId[key] = newList;
	}

	private void ClearCardChanges()
	{
		m_cardChangesPerEventId.Clear();
		m_cardChangeEventsToDisplayQueue.Clear();
	}
}
