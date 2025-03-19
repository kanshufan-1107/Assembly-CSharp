using System;
using Blizzard.GameService.SDK.Client.Integration;

public static class ChatUtils
{
	public static string GetMessage(BnetWhisper whisper)
	{
		return whisper.GetMessage();
	}

	public static bool TryGetFormattedDeckcodeMessage(string message, bool showHint, out string formattedDeckcodeMessage)
	{
		formattedDeckcodeMessage = string.Empty;
		if (message == null)
		{
			return false;
		}
		if (ShareableMercenariesTeam.ParseDeckCode(message, out var deckName) != null)
		{
			string newMessage = ((!string.IsNullOrWhiteSpace(deckName)) ? GameStrings.Format("GLOBAL_CHAT_MERCENARIES_PARTY_CODE_WITH_NAME_MESSAGE", deckName, string.Empty) : GameStrings.Format("GLOBAL_CHAT_MERCENARIES_PARTY_CODE_MESSAGE", string.Empty));
			formattedDeckcodeMessage = newMessage;
			return true;
		}
		ShareableDeck shareableDeck = ShareableDeck.ParseDeckCode(message, out deckName);
		if (shareableDeck != null)
		{
			TAG_CLASS heroClass = ShareableDeck.ExtractClassFromDeck(shareableDeck);
			if (heroClass != 0)
			{
				string className = GameStrings.GetClassName(heroClass);
				string newMessage2 = ((!string.IsNullOrWhiteSpace(deckName)) ? GameStrings.Format("GLOBAL_CHAT_DECK_CODE_WITH_NAME_MESSAGE", className, deckName, string.Empty) : GameStrings.Format("GLOBAL_CHAT_DECK_CODE_MESSAGE", className, string.Empty));
				formattedDeckcodeMessage = newMessage2;
				return true;
			}
		}
		return false;
	}

	public static bool TrySendDeckcodeFromClipboard(Action<string> onConfirmationCallback)
	{
		ShareableMercenariesTeam mercenariesDeck = ShareableMercenariesTeam.DeserializeFromClipboard();
		if (mercenariesDeck != null)
		{
			ShowDeckcodePopup(mercenariesDeck.Serialize(includeComments: false), mercenariesDeck.DeckName, onConfirmationCallback);
			return true;
		}
		ShareableDeck standardDeck = ShareableDeck.DeserializeFromClipboard();
		if (standardDeck != null)
		{
			ShowDeckcodePopup(standardDeck.Serialize(includeComments: false), standardDeck.DeckName, onConfirmationCallback);
			return true;
		}
		return false;
	}

	private static void ShowDeckcodePopup(string deckCode, string deckName, Action<string> onConfirmationCallback)
	{
		string deckCodeMessage = ShareableDeck.GenerateDeckCodeMessage(deckCode, deckName);
		AlertPopup.PopupInfo info = new AlertPopup.PopupInfo
		{
			m_headerText = GameStrings.Get("GLOBAL_CHAT_SEND_DECK_TITLE"),
			m_text = GameStrings.Get("GLOBAL_CHAT_SEND_DECK_MESSAGE"),
			m_showAlertIcon = false,
			m_attentionCategory = UserAttentionBlocker.NONE,
			m_responseDisplay = AlertPopup.ResponseDisplay.CONFIRM_CANCEL,
			m_responseCallback = delegate(AlertPopup.Response response, object userData)
			{
				if (response == AlertPopup.Response.CONFIRM)
				{
					onConfirmationCallback(deckCodeMessage);
				}
				ClipboardUtils.CopyToClipboard(string.Empty);
			}
		};
		DialogManager.Get().ShowPopup(info);
	}
}
