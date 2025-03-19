using System;
using System.Collections.Generic;
using Assets;

public class LoginPopups : IDisposable
{
	private List<LoginPopupSequenceDbfRecord> m_loginPopupSequenceDbfRecords = new List<LoginPopupSequenceDbfRecord>();

	private List<long> m_saveKeyValues = new List<long>();

	private List<LoginPopupSequencePopupDbfRecord> m_popupRecords = new List<LoginPopupSequencePopupDbfRecord>();

	public void Dispose()
	{
		m_loginPopupSequenceDbfRecords = null;
		m_saveKeyValues = null;
		m_popupRecords = null;
	}

	public bool ShowLoginPopupSequence(bool suppressRewardPopupsForNewPlayer, bool shouldDisableNotificationOnLogin, CardPopups m_cardPopups)
	{
		if (suppressRewardPopupsForNewPlayer)
		{
			return false;
		}
		if (!UserAttentionManager.CanShowAttentionGrabber("ShowLoginPopupSequence"))
		{
			return false;
		}
		m_loginPopupSequenceDbfRecords.Clear();
		EventTimingManager eventTimingManager = EventTimingManager.Get();
		List<LoginPopupSequenceDbfRecord> sequences = GameDbf.LoginPopupSequence.GetRecords();
		int i = 0;
		for (int iMax = sequences.Count; i < iMax; i++)
		{
			LoginPopupSequenceDbfRecord loginPopupSequenceDbfRecord = sequences[i];
			if (eventTimingManager.IsEventActive(loginPopupSequenceDbfRecord.EventTiming))
			{
				m_loginPopupSequenceDbfRecords.Add(loginPopupSequenceDbfRecord);
			}
		}
		if (m_loginPopupSequenceDbfRecords.Count == 0)
		{
			return false;
		}
		if (shouldDisableNotificationOnLogin)
		{
			List<long> popSequenceIds = new List<long>();
			foreach (LoginPopupSequenceDbfRecord popupSequenceRecord in m_loginPopupSequenceDbfRecords)
			{
				popSequenceIds.Add(popupSequenceRecord.ID);
			}
			GameSaveDataManager.Get().SaveSubkey(new GameSaveDataManager.SubkeySaveRequest(GameSaveKeyId.PLAYER_OPTIONS, GameSaveKeySubkeyId.LOGIN_POPUP_SEQUENCE_SEEN_POPUPS, popSequenceIds.ToArray()));
			return false;
		}
		bool showingPopup = false;
		GameSaveDataManager.Get().GetSubkeyValue(GameSaveKeyId.PLAYER_OPTIONS, GameSaveKeySubkeyId.LOGIN_POPUP_SEQUENCE_SEEN_POPUPS, m_saveKeyValues);
		foreach (LoginPopupSequenceDbfRecord loginPopupSequenceDbfRecord3 in m_loginPopupSequenceDbfRecords)
		{
			int recordId = loginPopupSequenceDbfRecord3.ID;
			if (m_saveKeyValues.Contains(recordId))
			{
				continue;
			}
			m_popupRecords.Clear();
			List<LoginPopupSequencePopupDbfRecord> popupSequenceRecords = GameDbf.LoginPopupSequencePopup.GetRecords();
			int j = 0;
			for (int iMax2 = popupSequenceRecords.Count; j < iMax2; j++)
			{
				LoginPopupSequencePopupDbfRecord loginPopupSequenceDbfRecord2 = popupSequenceRecords[j];
				if (loginPopupSequenceDbfRecord2.LoginPopupSequenceId == recordId)
				{
					m_popupRecords.Add(loginPopupSequenceDbfRecord2);
				}
			}
			for (int k = 0; k < m_popupRecords.Count; k++)
			{
				LoginPopupSequencePopupDbfRecord popupRecord = m_popupRecords[k];
				Assets.LoginPopupSequencePopup.LoginPopupSequencePopupType popupType = popupRecord.PopupType;
				bool shouldShowPopup = true;
				if (popupRecord.RequiresWildUnlocked && !CollectionManager.Get().ShouldAccountSeeStandardWild())
				{
					shouldShowPopup = false;
				}
				else if (popupRecord.SuppressForReturningPlayer && ReturningPlayerMgr.Get().SuppressOldPopups)
				{
					shouldShowPopup = false;
				}
				bool lastPopupRecordInSequence = k == m_popupRecords.Count - 1;
				DialogBase.HideCallback onPopupDismissedCallback = null;
				if (lastPopupRecordInSequence)
				{
					if (!shouldShowPopup)
					{
						OnPopupSequenceDismissed(recordId);
						break;
					}
					onPopupDismissedCallback = CreateCallback(recordId);
				}
				else if (!shouldShowPopup)
				{
					continue;
				}
				if ((uint)popupType > 1u && popupType == Assets.LoginPopupSequencePopup.LoginPopupSequencePopupType.FEATURED_CARDS)
				{
					if (m_cardPopups.ShowFeaturedCards(DbfShared.GetEventMap().ConvertStringToSpecialEvent(popupRecord.FeaturedCardsEvent), popupRecord.HeaderText, onPopupDismissedCallback))
					{
						showingPopup = true;
					}
					else if (lastPopupRecordInSequence)
					{
						OnPopupSequenceDismissed(recordId);
					}
					continue;
				}
				LoginPopupSequencePopup.Info popupInfo = new LoginPopupSequencePopup.Info
				{
					m_headerText = popupRecord.HeaderText,
					m_bodyText = popupRecord.BodyText,
					m_buttonText = popupRecord.ButtonText,
					m_backgroundMaterialReference = new AssetReference(popupRecord.BackgroundMaterial),
					m_callbackOnHide = onPopupDismissedCallback,
					m_prefabAssetReference = popupRecord.PrefabOverride
				};
				if (popupRecord.CardId != 0)
				{
					TAG_PREMIUM premiumType = (TAG_PREMIUM)popupRecord.CardPremium;
					popupInfo.m_card = CollectionManager.Get().GetCard(GameUtils.TranslateDbIdToCardId(popupRecord.CardId), premiumType);
				}
				DialogManager.Get().ShowLoginPopupSequenceBasicPopup(UserAttentionBlocker.NONE, popupInfo);
				showingPopup = true;
			}
			m_popupRecords.Clear();
		}
		return showingPopup;
	}

	private DialogBase.HideCallback CreateCallback(int recordId)
	{
		return delegate
		{
			OnPopupSequenceDismissed(recordId);
		};
	}

	private void OnPopupSequenceDismissed(int popupSequenceId)
	{
		GameSaveDataManager.Get().GetSubkeyValue(GameSaveKeyId.PLAYER_OPTIONS, GameSaveKeySubkeyId.LOGIN_POPUP_SEQUENCE_SEEN_POPUPS, out List<long> seenPopupSequenceIds);
		if (seenPopupSequenceIds == null)
		{
			seenPopupSequenceIds = new List<long>();
		}
		seenPopupSequenceIds.Add(popupSequenceId);
		GameSaveDataManager.Get().SaveSubkey(new GameSaveDataManager.SubkeySaveRequest(GameSaveKeyId.PLAYER_OPTIONS, GameSaveKeySubkeyId.LOGIN_POPUP_SEQUENCE_SEEN_POPUPS, seenPopupSequenceIds.ToArray()));
	}
}
