using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TouristCollectionCardEventHandler : CollectionCardEventHandler
{
	private HashSet<TAG_CLASS> m_currentGlowingTouristClasses = new HashSet<TAG_CLASS>();

	private Notification m_collectionNotification;

	private Notification m_classTabNotification;

	private bool m_hasShownCollectionNotification;

	private bool m_hasShownClassTabNotification;

	private Coroutine m_delayedCollectionFTUETooltip;

	private TAG_CLASS m_touristClass;

	private const float FTUE_PULSE_REMINDER_SECONDS = 3f;

	public override void OnCardAdded(CollectionDeckTray collectionDeckTray, CollectionDeck deck, EntityDef entityDef, TAG_PREMIUM __, Actor ____)
	{
		CollectionManager cm = CollectionManager.Get();
		if (cm == null)
		{
			return;
		}
		CollectibleDisplay collectibleDisplay = cm.GetCollectibleDisplay();
		if (collectibleDisplay == null)
		{
			return;
		}
		TAG_CLASS touristClass = (m_touristClass = (TAG_CLASS)entityDef.GetTag(GAME_TAG.TOURIST));
		CollectionPageManager collectionPageManager = collectibleDisplay.GetPageManager() as CollectionPageManager;
		if (collectionPageManager != null)
		{
			List<TAG_CLASS> filterClasses = deck.GetClasses();
			filterClasses.Add(TAG_CLASS.NEUTRAL);
			collectionPageManager.FilterByClasses(filterClasses);
			if (filterClasses.Contains(touristClass) && !collectionPageManager.HasSearchTextFilter() && !collectionDeckTray.IsAutoAddingCardsWithTiming)
			{
				BookPageManager.DelOnPageTransitionComplete tabChangedCallback = delegate
				{
					collectionPageManager.OnCurrentClassChanged -= OnCurrentCollectionClassChanged;
					collectionPageManager.OnCurrentClassChanged += OnCurrentCollectionClassChanged;
				};
				collectionPageManager.JumpToCollectionClassPage(touristClass, tabChangedCallback);
			}
			else
			{
				collectionPageManager.UpdateVisibleTabs();
				collectionPageManager.OnCurrentClassChanged -= OnCurrentCollectionClassChanged;
				collectionPageManager.OnCurrentClassChanged += OnCurrentCollectionClassChanged;
			}
			collectionPageManager.UpdatePageGhostingForInvalidTourists(deck);
			if (collectionPageManager.HasClassCardsAvailable(touristClass))
			{
				collectionPageManager.PlaySpawnVFXOnClassTab(touristClass);
			}
			else
			{
				collectionPageManager.OnVisibleTabsUpdated -= TryShowClassTabSparkleFX;
				collectionPageManager.OnVisibleTabsUpdated += TryShowClassTabSparkleFX;
			}
			collectionPageManager.SetClassTabShouldShowPersistentGlow(touristClass, shouldShowGlow: true);
			collectionPageManager.OnCollectionClassTabHovered -= OnClassTabHovered;
			collectionPageManager.OnCollectionClassTabHovered += OnClassTabHovered;
			m_currentGlowingTouristClasses.Add(touristClass);
			cm.RemoveEditedDeckChanged(OnEditedDeckChanged);
			cm.RegisterEditedDeckChanged(OnEditedDeckChanged);
			if (collectionPageManager.m_classFilterHeader != null)
			{
				collectionPageManager.m_classFilterHeader.SetShouldShowPersistentClassGlow(shouldShowGlow: true);
				collectionPageManager.m_classFilterHeader.OnPressed -= OnClassFilterHeaderPressed;
				collectionPageManager.m_classFilterHeader.OnPressed += OnClassFilterHeaderPressed;
			}
		}
		collectionDeckTray.UpdateRuneIndicatorVisual(deck);
		if (!collectionDeckTray.IsAutoAddingCardsWithTiming)
		{
			GameSaveDataManager.Get().GetSubkeyValue(GameSaveKeyId.FTUE, GameSaveKeySubkeyId.HAS_SEEN_TOURIST_FTUE, out long hasSeenTouristFTUE);
			if (hasSeenTouristFTUE <= 0)
			{
				StartFTUE(touristClass);
			}
		}
	}

	public override void OnCardRemoved(CollectionDeckTray collectionDeckTray, CollectionDeck deck)
	{
		CollectionPageManager collectionPageManager = CollectionManager.Get().GetCollectibleDisplay().GetPageManager() as CollectionPageManager;
		if (collectionPageManager != null)
		{
			List<TAG_CLASS> filterClasses = deck.GetClasses();
			filterClasses.Add(TAG_CLASS.NEUTRAL);
			collectionPageManager.FilterByClasses(filterClasses);
			if (!filterClasses.Contains(collectionPageManager.GetCurrentClassContextClassTag()))
			{
				collectionPageManager.JumpToCollectionClassPage(filterClasses[0]);
			}
			else
			{
				collectionPageManager.UpdateVisibleTabs();
			}
			collectionPageManager.RemoveGhostingEffectForTouristCards();
			collectionPageManager.UpdatePageGhostingForInvalidTourists(deck);
			List<TAG_CLASS> currentDeckTouristClasses = deck.GetTouristClasses();
			foreach (TAG_CLASS tagClass in new List<TAG_CLASS>(m_currentGlowingTouristClasses))
			{
				if (!currentDeckTouristClasses.Contains(tagClass))
				{
					EndClassTabGlow(tagClass);
				}
			}
			if (m_currentGlowingTouristClasses.Count == 0 && collectionPageManager.m_classFilterHeader != null)
			{
				collectionPageManager.m_classFilterHeader.SetShouldShowPersistentClassGlow(shouldShowGlow: false);
				collectionPageManager.m_classFilterHeader.OnPressed -= OnClassFilterHeaderPressed;
			}
		}
		CollectionDeckTray.Get().UpdateRuneIndicatorVisual(deck);
		DismissAllFTUE(shouldDismissIfFTUEHasntShown: true);
	}

	private void OnEditedDeckChanged(CollectionDeck newDeck, CollectionDeck oldDeck, object callbackData)
	{
		if (newDeck == null)
		{
			CollectionManager cm = CollectionManager.Get();
			if (cm != null)
			{
				CollectibleDisplay collectibleDisplay = cm.GetCollectibleDisplay();
				if (collectibleDisplay != null)
				{
					CollectionPageManager collectionPageManager = collectibleDisplay.GetPageManager() as CollectionPageManager;
					if (collectionPageManager != null)
					{
						ClassFilterHeaderButton classFilterHeaderButton = collectionPageManager.m_classFilterHeader;
						foreach (TAG_CLASS touristClass in m_currentGlowingTouristClasses)
						{
							collectionPageManager.SetClassTabShouldShowPersistentGlow(touristClass, shouldShowGlow: false);
							if (classFilterHeaderButton != null)
							{
								classFilterHeaderButton.SetShouldShowPersistentClassGlow(shouldShowGlow: false);
								classFilterHeaderButton.OnPressed -= OnClassFilterHeaderPressed;
							}
						}
					}
				}
			}
			m_currentGlowingTouristClasses.Clear();
		}
		CleanupEventListeners();
		DismissAllFTUE(shouldDismissIfFTUEHasntShown: true);
	}

	private void EndClassTabGlow(TAG_CLASS tagClass)
	{
		CollectionPageManager collectionPageManager = CollectionManager.Get().GetCollectibleDisplay().GetPageManager() as CollectionPageManager;
		if (m_currentGlowingTouristClasses.Contains(tagClass))
		{
			if (collectionPageManager != null)
			{
				collectionPageManager.SetClassTabShouldShowPersistentGlow(tagClass, shouldShowGlow: false);
			}
			m_currentGlowingTouristClasses.Remove(tagClass);
			CleanupEventListeners();
		}
	}

	private void OnClassTabHovered(TAG_CLASS tagClass)
	{
		EndClassTabGlow(tagClass);
	}

	private void OnCurrentCollectionClassChanged(TAG_CLASS tagClass)
	{
		EndClassTabGlow(tagClass);
	}

	private void OnClassFilterHeaderPressed()
	{
		CollectionPageManager collectionPageManager = CollectionManager.Get().GetCollectibleDisplay().GetPageManager() as CollectionPageManager;
		if (collectionPageManager != null)
		{
			ClassFilterHeaderButton classFilterHeaderButton = collectionPageManager.m_classFilterHeader;
			if (classFilterHeaderButton != null)
			{
				classFilterHeaderButton.SetShouldShowPersistentClassGlow(shouldShowGlow: false);
				classFilterHeaderButton.OnPressed -= OnClassFilterHeaderPressed;
			}
		}
	}

	private void ShowFTUETooltipOnClassFilterHeaderPressedCallback()
	{
		CollectionPageManager collectionPageManager = CollectionManager.Get().GetCollectibleDisplay().GetPageManager() as CollectionPageManager;
		if (collectionPageManager != null)
		{
			ClassFilterHeaderButton classFilterHeaderButton = collectionPageManager.m_classFilterHeader;
			if (classFilterHeaderButton != null)
			{
				classFilterHeaderButton.OnPressed -= ShowFTUETooltipOnClassFilterHeaderPressedCallback;
				classFilterHeaderButton.m_classFilterTray.OnTransitionComplete -= ShowCollectionFTUETooltipAfterClassFilterShown;
				classFilterHeaderButton.m_classFilterTray.OnTransitionComplete += ShowCollectionFTUETooltipAfterClassFilterShown;
			}
		}
	}

	private void ShowCollectionFTUETooltipAfterClassFilterShown()
	{
		CollectionPageManager collectionPageManager = CollectionManager.Get().GetCollectibleDisplay().GetPageManager() as CollectionPageManager;
		if (collectionPageManager != null)
		{
			ClassFilterHeaderButton classFilterHeaderButton = collectionPageManager.m_classFilterHeader;
			if (classFilterHeaderButton != null)
			{
				classFilterHeaderButton.m_classFilterTray.OnTransitionComplete -= ShowCollectionFTUETooltipAfterClassFilterShown;
				classFilterHeaderButton.m_classFilterTray.OnTransitionComplete -= DismissFTUEOnTransitiionComplete;
				classFilterHeaderButton.m_classFilterTray.OnTransitionComplete += DismissFTUEOnTransitiionComplete;
				ShowCollectionFTUENotification(null);
			}
		}
	}

	private void CleanupEventListeners()
	{
		if (m_currentGlowingTouristClasses.Count != 0)
		{
			return;
		}
		CollectionManager cm = CollectionManager.Get();
		if (cm == null)
		{
			return;
		}
		cm.RemoveEditedDeckChanged(OnEditedDeckChanged);
		CollectibleDisplay collectibleDisplay = cm.GetCollectibleDisplay();
		if (!(collectibleDisplay != null))
		{
			return;
		}
		CollectionPageManager collectionPageManager = collectibleDisplay.GetPageManager() as CollectionPageManager;
		if (!(collectionPageManager != null))
		{
			return;
		}
		collectionPageManager.OnCollectionClassTabHovered -= OnClassTabHovered;
		collectionPageManager.OnCurrentClassChanged -= OnCurrentCollectionClassChanged;
		collectionPageManager.OnVisibleTabsUpdated -= TryShowClassTabSparkleFX;
		ClassFilterHeaderButton classFilterHeaderButton = collectionPageManager.m_classFilterHeader;
		if (classFilterHeaderButton != null)
		{
			ClassFilterButtonContainer classFilterButtonContainer = classFilterHeaderButton.m_container;
			if (classFilterButtonContainer != null)
			{
				classFilterButtonContainer.OnClassFilterButtonPressed -= OnClassFilterButtonPressed;
			}
		}
	}

	private void StartFTUE(TAG_CLASS touristClass)
	{
		CollectionManager cm = CollectionManager.Get();
		if (cm == null)
		{
			return;
		}
		CollectionManagerDisplay collectionManagerDisplay = cm.GetCollectibleDisplay() as CollectionManagerDisplay;
		if (collectionManagerDisplay == null)
		{
			return;
		}
		CollectionPageManager collectionPageManager = collectionManagerDisplay.GetPageManager() as CollectionPageManager;
		if (!(collectionPageManager == null))
		{
			NotificationManager notificationManager = NotificationManager.Get();
			if (collectionPageManager.HasClassCardsAvailable(touristClass))
			{
				m_classTabNotification = notificationManager.CreatePopupText(UserAttentionBlocker.NONE, collectionManagerDisplay.m_touristClassTabTutorialBone.transform.position, collectionManagerDisplay.m_touristClassTabTutorialBone.transform.localScale, GameStrings.Get("GLUE_TOURIST_FTUE_CLASS_TAB_TOOLTIP"));
				m_classTabNotification.PulseReminderEveryXSeconds(3f);
				m_hasShownClassTabNotification = true;
				GameSaveDataManager.Get().SaveSubkey(new GameSaveDataManager.SubkeySaveRequest(GameSaveKeyId.FTUE, GameSaveKeySubkeyId.HAS_SEEN_TOURIST_FTUE, 1L));
			}
			else
			{
				collectionPageManager.OnVisibleTabsUpdated -= TryShowClassFTUETooltip;
				collectionPageManager.OnVisibleTabsUpdated += TryShowClassFTUETooltip;
			}
			if (collectionPageManager.GetCurrentClassContextClassTag() == m_touristClass)
			{
				ShowCollectionFTUETooltip();
			}
			else
			{
				collectionPageManager.OnCurrentClassChanged -= ShowCollectionFTUETooltipOnClassChanged;
				collectionPageManager.OnCurrentClassChanged += ShowCollectionFTUETooltipOnClassChanged;
			}
			collectionPageManager.SuppressTouristTooltip = true;
			CollectionDeckTray.OnCardAddedEvent -= OnAnyCardAdded;
			CollectionDeckTray.OnCardAddedEvent += OnAnyCardAdded;
		}
	}

	private void TryShowClassTabSparkleFX()
	{
		CollectionManager cm = CollectionManager.Get();
		if (cm == null)
		{
			return;
		}
		CollectionManagerDisplay collectionManagerDisplay = cm.GetCollectibleDisplay() as CollectionManagerDisplay;
		if (!(collectionManagerDisplay == null))
		{
			CollectionPageManager collectionPageManager = collectionManagerDisplay.GetPageManager() as CollectionPageManager;
			if (!(collectionPageManager == null) && collectionPageManager.HasClassCardsAvailable(m_touristClass))
			{
				collectionPageManager.OnVisibleTabsUpdated -= TryShowClassTabSparkleFX;
				collectionPageManager.PlaySpawnVFXOnClassTab(m_touristClass);
			}
		}
	}

	private void ShowCollectionFTUETooltipOnClassChanged(TAG_CLASS newClass)
	{
		if (newClass != m_touristClass)
		{
			return;
		}
		CollectionManager cm = CollectionManager.Get();
		if (cm == null)
		{
			return;
		}
		CollectionManagerDisplay collectionManagerDisplay = cm.GetCollectibleDisplay() as CollectionManagerDisplay;
		if (!(collectionManagerDisplay == null))
		{
			CollectionPageManager collectionPageManager = collectionManagerDisplay.GetPageManager() as CollectionPageManager;
			if (!(collectionPageManager == null))
			{
				collectionPageManager.OnCurrentClassChanged -= ShowCollectionFTUETooltipOnClassChanged;
				ShowCollectionFTUETooltip();
			}
		}
	}

	private void ShowCollectionFTUETooltip()
	{
		CollectionManager cm = CollectionManager.Get();
		if (cm == null)
		{
			return;
		}
		CollectionManagerDisplay collectionManagerDisplay = cm.GetCollectibleDisplay() as CollectionManagerDisplay;
		if (collectionManagerDisplay == null)
		{
			return;
		}
		CollectionPageManager collectionPageManager = collectionManagerDisplay.GetPageManager() as CollectionPageManager;
		if (collectionPageManager == null)
		{
			return;
		}
		if (!UniversalInputManager.UsePhoneUI)
		{
			m_delayedCollectionFTUETooltip = StartCoroutine(ShowCollectionFTUETooltipOnDelayCoroutine(2f, Notification.PopUpArrowDirection.Left));
			collectionPageManager.PageTurnStart -= DismissFTUEOnPageTurnStartCallback;
			collectionPageManager.PageTurnStart += DismissFTUEOnPageTurnStartCallback;
			return;
		}
		m_classTabNotification.ShowPopUpArrow(Notification.PopUpArrowDirection.Up);
		ClassFilterHeaderButton classFilterHeaderButton = collectionPageManager.m_classFilterHeader;
		if (classFilterHeaderButton != null)
		{
			classFilterHeaderButton.OnPressed += ShowFTUETooltipOnClassFilterHeaderPressedCallback;
			ClassFilterButtonContainer classFilterButtonContainer = classFilterHeaderButton.m_container;
			if (classFilterButtonContainer != null)
			{
				classFilterButtonContainer.OnClassFilterButtonPressed -= OnClassFilterButtonPressed;
				classFilterButtonContainer.OnClassFilterButtonPressed += OnClassFilterButtonPressed;
			}
		}
	}

	private void OnAnyCardAdded(EntityDef entityDef)
	{
		if (!entityDef.HasTag(GAME_TAG.TOURIST))
		{
			DismissAllFTUE(shouldDismissIfFTUEHasntShown: false);
		}
	}

	private void ShowCollectionFTUENotification(Notification.PopUpArrowDirection? popupArrowDirection)
	{
		CollectionManager cm = CollectionManager.Get();
		if (cm == null)
		{
			return;
		}
		CollectionManagerDisplay collectionManagerDisplay = cm.GetCollectibleDisplay() as CollectionManagerDisplay;
		if (!(collectionManagerDisplay == null))
		{
			NotificationManager notificationManager = NotificationManager.Get();
			m_collectionNotification = notificationManager.CreatePopupText(UserAttentionBlocker.NONE, collectionManagerDisplay.m_touristCollectionTutorialBone.transform.position, collectionManagerDisplay.m_touristCollectionTutorialBone.transform.localScale, GameStrings.Get("GLUE_TOURIST_FTUE_COLLECTION_TOOLTIP"));
			m_collectionNotification.PulseReminderEveryXSeconds(3f);
			m_hasShownCollectionNotification = true;
			if (popupArrowDirection.HasValue)
			{
				m_collectionNotification.ShowPopUpArrow(popupArrowDirection.Value);
			}
			GameSaveDataManager.Get().SaveSubkey(new GameSaveDataManager.SubkeySaveRequest(GameSaveKeyId.FTUE, GameSaveKeySubkeyId.HAS_SEEN_TOURIST_FTUE, 1L));
		}
	}

	private IEnumerator ShowCollectionFTUETooltipOnDelayCoroutine(float seconds, Notification.PopUpArrowDirection? popupArrowDirection)
	{
		yield return new WaitForSeconds(seconds);
		m_delayedCollectionFTUETooltip = null;
		ShowCollectionFTUENotification(popupArrowDirection);
	}

	private void TryShowClassFTUETooltip()
	{
		CollectionManager cm = CollectionManager.Get();
		if (cm == null)
		{
			return;
		}
		CollectionManagerDisplay collectionManagerDisplay = cm.GetCollectibleDisplay() as CollectionManagerDisplay;
		if (!(collectionManagerDisplay == null))
		{
			CollectionPageManager collectionPageManager = collectionManagerDisplay.GetPageManager() as CollectionPageManager;
			if (!(collectionPageManager == null) && collectionPageManager.HasClassCardsAvailable(m_touristClass))
			{
				NotificationManager notificationManager = NotificationManager.Get();
				m_classTabNotification = notificationManager.CreatePopupText(UserAttentionBlocker.NONE, collectionManagerDisplay.m_touristClassTabTutorialBone.transform.position, collectionManagerDisplay.m_touristClassTabTutorialBone.transform.localScale, GameStrings.Get("GLUE_TOURIST_FTUE_CLASS_TAB_TOOLTIP"));
				m_classTabNotification.PulseReminderEveryXSeconds(3f);
				m_hasShownClassTabNotification = true;
				collectionPageManager.OnVisibleTabsUpdated -= TryShowClassFTUETooltip;
				GameSaveDataManager.Get().SaveSubkey(new GameSaveDataManager.SubkeySaveRequest(GameSaveKeyId.FTUE, GameSaveKeySubkeyId.HAS_SEEN_TOURIST_FTUE, 1L));
			}
		}
	}

	private void OnClassFilterButtonPressed(TAG_CLASS tagClass)
	{
		EndClassTabGlow(tagClass);
	}

	private void DismissFTUEOnPageTurnStartCallback(BookPageManager.PageTransitionType transitionType)
	{
		DismissAllFTUE(shouldDismissIfFTUEHasntShown: false);
	}

	private void DismissFTUEOnTransitiionComplete()
	{
		DismissAllFTUE(shouldDismissIfFTUEHasntShown: false);
	}

	private void DismissAllFTUE(bool shouldDismissIfFTUEHasntShown)
	{
		if (!((m_hasShownClassTabNotification && m_hasShownCollectionNotification) || shouldDismissIfFTUEHasntShown))
		{
			return;
		}
		NotificationManager notificationManager = NotificationManager.Get();
		if (m_collectionNotification != null)
		{
			if (notificationManager != null)
			{
				notificationManager.DestroyNotification(m_collectionNotification, 0f);
			}
			m_collectionNotification = null;
		}
		if (m_classTabNotification != null)
		{
			if (notificationManager != null)
			{
				notificationManager.DestroyNotification(m_classTabNotification, 0f);
			}
			m_classTabNotification = null;
		}
		if (m_delayedCollectionFTUETooltip != null)
		{
			StopCoroutine(m_delayedCollectionFTUETooltip);
			m_delayedCollectionFTUETooltip = null;
		}
		CollectionDeckTray.OnCardAddedEvent -= OnAnyCardAdded;
		CollectionManager cm = CollectionManager.Get();
		if (cm == null)
		{
			return;
		}
		CollectionManagerDisplay collectionManagerDisplay = cm.GetCollectibleDisplay() as CollectionManagerDisplay;
		if (!(collectionManagerDisplay != null))
		{
			return;
		}
		CollectionPageManager collectionPageManager = collectionManagerDisplay.GetPageManager() as CollectionPageManager;
		if (collectionPageManager != null)
		{
			collectionPageManager.PageTurnStart -= DismissFTUEOnPageTurnStartCallback;
			ClassFilterHeaderButton classFilterHeaderButton = collectionPageManager.m_classFilterHeader;
			if (classFilterHeaderButton != null)
			{
				classFilterHeaderButton.OnPressed -= OnClassFilterHeaderPressed;
			}
			collectionPageManager.SuppressTouristTooltip = false;
			collectionPageManager.OnVisibleTabsUpdated -= TryShowClassFTUETooltip;
			collectionPageManager.OnCurrentClassChanged -= ShowCollectionFTUETooltipOnClassChanged;
		}
	}
}
