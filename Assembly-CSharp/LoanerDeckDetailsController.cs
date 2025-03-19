using System.Collections.Generic;
using Hearthstone.DataModels;
using Hearthstone.UI;
using UnityEngine;

public class LoanerDeckDetailsController : MonoBehaviour
{
	public const int MAX_LOANER_DECKS_DISPLAYED = 6;

	public ShopDeckPouchDisplay PouchDisplay;

	public UIBScrollable Scrollbar;

	private Widget m_detailsWidget;

	private ShopCardList m_cardList;

	[Header("Pagination FTUE")]
	private Notification m_paginationFTUENotification;

	[SerializeField]
	private Transform m_paginationFTUEDisplayBonePC;

	[SerializeField]
	private Transform m_paginationFTUEDisplayBoneMobile;

	[SerializeField]
	private float m_notificationPulseTime = 1f;

	[SerializeField]
	private float m_RemoveNotificationDelay = 8f;

	[SerializeField]
	private Notification.PopUpArrowDirection m_paginationFTUEArrowDirection = Notification.PopUpArrowDirection.Right;

	private bool shouldShowPaginationFTUE;

	private void Awake()
	{
		m_detailsWidget = GetComponent<WidgetTemplate>();
		m_cardList = new ShopCardList(m_detailsWidget, Scrollbar);
		m_cardList.InitInput();
	}

	public void ShowDeckChoiceDetails(DeckTemplateDbfRecord deckRecord)
	{
		if (PouchDisplay == null)
		{
			Log.Decks.PrintWarning(" Deck Details Widget is missing a ShopDeckPouchDisplay!");
			return;
		}
		PouchDisplay.SetDeckPouchData(m_detailsWidget, deckRecord);
		if (shouldShowPaginationFTUE)
		{
			ShowPaginationFTUENotification();
		}
	}

	public void ShowDeckCardList(DeckDbfRecord deckRecord)
	{
		if (m_cardList != null)
		{
			if (Scrollbar == null)
			{
				Log.Decks.PrintWarning(" Deck Details Widget is missing a Scrollbar!");
				return;
			}
			m_cardList.SetData(deckRecord.Cards, BoosterDbId.INVALID);
			Scrollbar.SetScrollImmediate(0f);
		}
	}

	public void SetShouldShowPaginationFTUE(bool showFTUE)
	{
		shouldShowPaginationFTUE = showFTUE;
	}

	public void ShowPaginationFTUENotification()
	{
		long hasSeenPaginationFTUE = 0L;
		GameSaveDataManager.Get().GetSubkeyValue(GameSaveKeyId.FTUE, GameSaveKeySubkeyId.FTUE_HAS_SEEN_PAGINATION_NOTIFICATION, out hasSeenPaginationFTUE);
		if (hasSeenPaginationFTUE > 0)
		{
			return;
		}
		NotificationManager notificationManager = NotificationManager.Get();
		if (!(notificationManager == null) && !(m_paginationFTUEDisplayBonePC == null) && !(m_paginationFTUEDisplayBoneMobile == null))
		{
			Transform displayBone = m_paginationFTUEDisplayBonePC;
			if ((bool)UniversalInputManager.UsePhoneUI)
			{
				displayBone = m_paginationFTUEDisplayBoneMobile;
			}
			m_paginationFTUENotification = notificationManager.CreatePopupText(UserAttentionBlocker.NONE, displayBone.transform.position, displayBone.transform.localScale, GameStrings.Get("GLUE_LOANER_DECK_FTUE_PAGINATION"));
			if (m_paginationFTUENotification != null)
			{
				m_paginationFTUENotification.PulseReminderEveryXSeconds(m_notificationPulseTime);
				m_paginationFTUENotification.ShowPopUpArrow(m_paginationFTUEArrowDirection);
				NotificationManager.Get()?.DestroyNotification(m_paginationFTUENotification, m_RemoveNotificationDelay);
				GameSaveDataManager.Get().SaveSubkey(new GameSaveDataManager.SubkeySaveRequest(GameSaveKeyId.FTUE, GameSaveKeySubkeyId.FTUE_HAS_SEEN_PAGINATION_NOTIFICATION, 1L));
			}
		}
	}

	public void ShowDeckCardList(IEnumerable<CardTileDataModel> cardList)
	{
		if (m_cardList != null)
		{
			if (Scrollbar == null)
			{
				Log.Decks.PrintWarning(" Deck Details Widget is missing a Scrollbar!");
				return;
			}
			m_cardList.SetData(cardList, BoosterDbId.INVALID);
			Scrollbar.SetScrollImmediate(0f);
		}
	}

	private void OnDestroy()
	{
		m_cardList.RemoveListeners();
		if (m_paginationFTUENotification != null)
		{
			NotificationManager.Get()?.DestroyNotificationNowWithNoAnim(m_paginationFTUENotification);
		}
	}
}
