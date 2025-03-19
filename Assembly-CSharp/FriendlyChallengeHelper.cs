using System.Runtime.CompilerServices;
using Blizzard.GameService.SDK.Client.Integration;

public class FriendlyChallengeHelper
{
	private static FriendlyChallengeHelper s_instance;

	private AlertPopup m_friendChallengeWaitingPopup;

	private AlertPopup m_deckShareRequestWaitingPopup;

	private AlertPopup m_deckShareRequestDeclinedPopup;

	private AlertPopup m_deckShareRequestCanceledPopup;

	private AlertPopup m_deckShareRequestPopup;

	private AlertPopup m_deckShareErrorPopup;

	[CompilerGenerated]
	private BnetAccountId _003CActiveChallengeMenu_003Ek__BackingField;

	public BnetAccountId ActiveChallengeMenu
	{
		[CompilerGenerated]
		set
		{
			_003CActiveChallengeMenu_003Ek__BackingField = value;
		}
	}

	public static FriendlyChallengeHelper Get()
	{
		if (s_instance == null)
		{
			s_instance = new FriendlyChallengeHelper();
		}
		return s_instance;
	}

	public void StartChallengeOrWaitForOpponent(string waitingDialogText, AlertPopup.ResponseCallback waitingCallback)
	{
		if (!FriendChallengeMgr.Get().DidOpponentSelectDeckOrHero())
		{
			ShowFriendChallengeWaitingForOpponentDialog(waitingDialogText, waitingCallback);
		}
	}

	public void HideFriendChallengeWaitingForOpponentDialog()
	{
		if (!(m_friendChallengeWaitingPopup == null))
		{
			m_friendChallengeWaitingPopup.Hide();
			m_friendChallengeWaitingPopup = null;
		}
	}

	public void WaitForFriendChallengeToStart()
	{
		int brawlLibraryItemId = 0;
		if (FriendChallengeMgr.Get().IsChallengeTavernBrawl())
		{
			TavernBrawlMission mission = TavernBrawlManager.Get().CurrentMission();
			if (mission != null)
			{
				brawlLibraryItemId = mission.SelectedBrawlLibraryItemId;
			}
		}
		GameMgr.Get().WaitForFriendChallengeToStart(FriendChallengeMgr.Get().GetFormatType(), FriendChallengeMgr.Get().GetChallengeBrawlType(), FriendChallengeMgr.Get().GetScenarioId(), brawlLibraryItemId, (!FriendChallengeMgr.Get().IsChallengeBacon()) ? PartyType.FRIENDLY_CHALLENGE : PartyType.BATTLEGROUNDS_PARTY);
	}

	public void StopWaitingForFriendChallenge()
	{
		HideFriendChallengeWaitingForOpponentDialog();
	}

	public void HideAllDeckShareDialogs()
	{
		HideDeckShareRequestDialog();
		HideDeckShareRequestCanceledDialog();
		HideDeckShareRequestDeclinedDialog();
		HideDeckShareRequestWaitingDialog();
		HideDeckShareErrorDialog();
	}

	public void ShowDeckShareRequestCanceledDialog()
	{
		BnetPlayer opponent = FriendChallengeMgr.Get().GetMyOpponent();
		AlertPopup.PopupInfo info = new AlertPopup.PopupInfo();
		info.m_headerText = GameStrings.Get("GLOBAL_DECK_SHARE_HEADER");
		info.m_text = GameStrings.Format("GLOBAL_DECK_SHARE_REQUEST_CANCELED", opponent.GetBestName());
		info.m_showAlertIcon = false;
		info.m_responseDisplay = AlertPopup.ResponseDisplay.CONFIRM;
		DialogManager.DialogProcessCallback callback = delegate(DialogBase dialog, object userData)
		{
			if (!FriendChallengeMgr.Get().HasChallenge())
			{
				return false;
			}
			m_deckShareRequestCanceledPopup = (AlertPopup)dialog;
			return true;
		};
		DialogManager.Get().ShowPopup(info, callback);
	}

	public void HideDeckShareRequestCanceledDialog()
	{
		if (!(m_deckShareRequestCanceledPopup == null))
		{
			m_deckShareRequestCanceledPopup.Hide();
			m_deckShareRequestCanceledPopup = null;
		}
	}

	public void ShowDeckShareRequestDeclinedDialog()
	{
		BnetPlayer opponent = FriendChallengeMgr.Get().GetMyOpponent();
		AlertPopup.PopupInfo info = new AlertPopup.PopupInfo();
		info.m_headerText = GameStrings.Get("GLOBAL_DECK_SHARE_HEADER");
		info.m_text = GameStrings.Format("GLOBAL_DECK_SHARE_REQUEST_DECLINED", opponent.GetBestName());
		info.m_showAlertIcon = false;
		info.m_responseDisplay = AlertPopup.ResponseDisplay.CONFIRM;
		DialogManager.DialogProcessCallback callback = delegate(DialogBase dialog, object userData)
		{
			if (!FriendChallengeMgr.Get().HasChallenge())
			{
				return false;
			}
			m_deckShareRequestDeclinedPopup = (AlertPopup)dialog;
			return true;
		};
		DialogManager.Get().ShowPopup(info, callback);
	}

	public void HideDeckShareRequestDeclinedDialog()
	{
		if (!(m_deckShareRequestDeclinedPopup == null))
		{
			m_deckShareRequestDeclinedPopup.Hide();
			m_deckShareRequestDeclinedPopup = null;
		}
	}

	public void ShowDeckShareRequestWaitingDialog(AlertPopup.ResponseCallback waitingCallback)
	{
		BnetPlayer opponent = FriendChallengeMgr.Get().GetMyOpponent();
		AlertPopup.PopupInfo info = new AlertPopup.PopupInfo();
		info.m_headerText = GameStrings.Get("GLOBAL_DECK_SHARE_HEADER");
		info.m_text = GameStrings.Format("GLOBAL_DECK_SHARE_REQUEST_WAITING_RESPONSE", opponent.GetBestName());
		info.m_showAlertIcon = false;
		info.m_responseDisplay = AlertPopup.ResponseDisplay.CANCEL;
		info.m_responseCallback = waitingCallback;
		DialogManager.DialogProcessCallback callback = delegate(DialogBase dialog, object userData)
		{
			if (!FriendChallengeMgr.Get().HasChallenge())
			{
				return false;
			}
			m_deckShareRequestWaitingPopup = (AlertPopup)dialog;
			return true;
		};
		DialogManager.Get().ShowPopup(info, callback);
	}

	public void HideDeckShareRequestWaitingDialog()
	{
		if (!(m_deckShareRequestWaitingPopup == null))
		{
			m_deckShareRequestWaitingPopup.Hide();
			m_deckShareRequestWaitingPopup = null;
		}
	}

	public void ShowDeckShareRequestDialog(AlertPopup.ResponseCallback waitingCallback)
	{
		BnetPlayer opponent = FriendChallengeMgr.Get().GetMyOpponent();
		AlertPopup.PopupInfo info = new AlertPopup.PopupInfo();
		info.m_headerText = GameStrings.Get("GLOBAL_DECK_SHARE_HEADER");
		info.m_text = GameStrings.Format("GLOBAL_DECK_SHARE_REQUESTED", opponent.GetBestName());
		info.m_showAlertIcon = false;
		info.m_responseDisplay = AlertPopup.ResponseDisplay.CONFIRM_CANCEL;
		info.m_responseCallback = waitingCallback;
		info.m_confirmText = GameStrings.Get("GLOBAL_DECK_SHARE_ACCEPT_REQUEST");
		info.m_cancelText = GameStrings.Get("GLOBAL_DECK_SHARE_DECLINE_REQUEST");
		DialogManager.DialogProcessCallback callback = delegate(DialogBase dialog, object userData)
		{
			if (!FriendChallengeMgr.Get().HasChallenge())
			{
				return false;
			}
			m_deckShareRequestPopup = (AlertPopup)dialog;
			return true;
		};
		DialogManager.Get().ShowPopup(info, callback);
	}

	public bool IsShowingDeckShareRequestDialog()
	{
		return m_deckShareRequestPopup != null;
	}

	public void HideDeckShareRequestDialog()
	{
		if (!(m_deckShareRequestPopup == null))
		{
			m_deckShareRequestPopup.Hide();
			m_deckShareRequestPopup = null;
		}
	}

	public void ShowDeckShareErrorDialog()
	{
		AlertPopup.PopupInfo info = new AlertPopup.PopupInfo();
		info.m_headerText = GameStrings.Get("GLOBAL_DECK_SHARE_HEADER");
		info.m_text = GameStrings.Get("GLOBAL_DECK_SHARE_ERROR");
		info.m_showAlertIcon = false;
		info.m_responseDisplay = AlertPopup.ResponseDisplay.CONFIRM;
		DialogManager.DialogProcessCallback callback = delegate(DialogBase dialog, object userData)
		{
			if (!FriendChallengeMgr.Get().HasChallenge())
			{
				return false;
			}
			m_deckShareErrorPopup = (AlertPopup)dialog;
			return true;
		};
		DialogManager.Get().ShowPopup(info, callback);
	}

	public void HideDeckShareErrorDialog()
	{
		if (!(m_deckShareErrorPopup == null))
		{
			m_deckShareErrorPopup.Hide();
			m_deckShareErrorPopup = null;
		}
	}

	private void ShowFriendChallengeWaitingForOpponentDialog(string dialogText, AlertPopup.ResponseCallback callback)
	{
		BnetPlayer opponent = FriendChallengeMgr.Get().GetMyOpponent();
		AlertPopup.PopupInfo info = new AlertPopup.PopupInfo();
		info.m_text = GameStrings.Format(dialogText, FriendUtils.GetUniqueName(opponent));
		info.m_showAlertIcon = false;
		info.m_responseDisplay = AlertPopup.ResponseDisplay.CANCEL;
		info.m_responseCallback = callback;
		DialogManager.Get().ShowPopup(info, OnFriendChallengeWaitingForOpponentDialogProcessed);
	}

	private bool OnFriendChallengeWaitingForOpponentDialogProcessed(DialogBase dialog, object userData)
	{
		if (!FriendChallengeMgr.Get().HasChallenge())
		{
			return false;
		}
		if (FriendChallengeMgr.Get().DidOpponentSelectDeckOrHero())
		{
			return false;
		}
		m_friendChallengeWaitingPopup = (AlertPopup)dialog;
		return true;
	}
}
