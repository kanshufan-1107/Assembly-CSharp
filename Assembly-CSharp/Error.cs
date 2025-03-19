using System.Collections.Generic;
using Blizzard.GameService.SDK.Client.Integration;
using Hearthstone;
using UnityEngine;

public static class Error
{
	public static readonly PlatformDependentValue<bool> HAS_APP_STORE = new PlatformDependentValue<bool>(PlatformCategory.OS)
	{
		PC = false,
		Mac = false,
		iOS = true,
		Android = true
	};

	private static bool s_hasShownNonRepeatingDevWarning = false;

	private static List<FatalErrorReason> s_telemSentReasons = new List<FatalErrorReason>();

	public static void AddWarning(string header, string message, params object[] messageArgs)
	{
		AddWarning(new ErrorParams
		{
			m_header = header,
			m_message = string.Format(message, messageArgs)
		});
	}

	public static void AddWarningLoc(string headerKey, string messageKey, params object[] messageArgs)
	{
		AddWarning(new ErrorParams
		{
			m_header = GameStrings.Get(headerKey),
			m_message = GameStrings.Format(messageKey, messageArgs)
		});
	}

	public static void AddWarning(ErrorParams parms)
	{
		if (!DialogManager.Get())
		{
			parms.m_reason = FatalErrorReason.UNAVAILAVLE_DIALOGMANAGER_FOR_WARNING;
			AddFatal(parms);
			return;
		}
		Debug.LogWarning($"Error.AddWarning() - header={parms.m_header} message={parms.m_message}");
		if (UniversalInputManager.Get() != null)
		{
			UniversalInputManager.Get().CancelTextInput(null, force: true);
		}
		ShowWarningDialog(parms);
	}

	public static void AddDevWarning(string header, string message, params object[] messageArgs)
	{
		string formattedMessage = string.Format(message, messageArgs);
		if (!Debug.isDebugBuild)
		{
			Debug.LogWarning($"Error.AddDevWarning() - header={header} message={formattedMessage}");
			return;
		}
		AddWarning(new ErrorParams
		{
			m_header = header,
			m_message = formattedMessage
		});
	}

	public static void AddDevWarningNonRepeating(string header, string message, params object[] messageArgs)
	{
		if (!s_hasShownNonRepeatingDevWarning)
		{
			s_hasShownNonRepeatingDevWarning = true;
			AddDevWarning(header, message, messageArgs);
		}
		else
		{
			string formattedMessage = string.Format(message, messageArgs);
			Debug.LogWarning($"Error.AddDevWarningNonRepeating() - header={header} message={formattedMessage}");
		}
	}

	public static void AddFatal(FatalErrorReason reason, string messageKey, params object[] messageArgs)
	{
		AddFatal(new ErrorParams
		{
			m_message = GameStrings.Format(messageKey, messageArgs),
			m_reason = reason
		});
	}

	private static void SendFatalErrorTelemetry(FatalErrorReason reason)
	{
		if (!s_telemSentReasons.Contains(reason))
		{
			TelemetryManager.Client().SendFatalError(reason.ToString(), BattleNet.IsInitialized() ? BattleNet.GetDataVersion() : 0);
			s_telemSentReasons.Add(reason);
		}
	}

	public static void AddFatal(ErrorParams parms)
	{
		Debug.LogError($"Error.AddFatal() - message={parms.m_message}");
		SendFatalErrorTelemetry(parms.m_reason);
		if (UniversalInputManager.Get() != null)
		{
			UniversalInputManager.Get().CancelTextInput(null, force: true);
		}
		if (ShouldUseWarningDialogForFatalError())
		{
			if (string.IsNullOrEmpty(parms.m_header))
			{
				parms.m_header = "Fatal Error as Warning";
			}
			ShowWarningDialog(parms);
			return;
		}
		parms.m_type = ErrorType.FATAL;
		FatalErrorMessage message = new FatalErrorMessage();
		message.m_id = (parms.m_header ?? string.Empty) + parms.m_message;
		message.m_text = parms.m_message;
		message.m_ackCallback = parms.m_ackCallback;
		message.m_ackUserData = parms.m_ackUserData;
		message.m_allowClick = parms.m_allowClick;
		message.m_redirectToStore = parms.m_redirectToStore;
		message.m_delayBeforeNextReset = parms.m_delayBeforeNextReset;
		message.m_reason = parms.m_reason;
		FatalErrorMgr.Get().Add(message);
	}

	public static void AddDevFatal(string message, params object[] messageArgs)
	{
		string formattedMessage = string.Format(message, messageArgs);
		if (!HearthstoneApplication.IsInternal())
		{
			Debug.LogError($"Error.AddDevFatal() - message={formattedMessage}");
			return;
		}
		Debug.LogError(formattedMessage);
		if (SceneDebugger.Get() != null)
		{
			SceneDebugger.Get().AddErrorMessage(formattedMessage);
		}
	}

	public static void AddDevFatalUnlessWorkarounds(string message, params object[] messageArgs)
	{
		string formattedMessage = string.Format(message, messageArgs);
		if (HearthstoneApplication.UseDevWorkarounds())
		{
			Debug.LogError(formattedMessage);
		}
		else
		{
			AddDevFatal(formattedMessage);
		}
	}

	private static bool ShouldUseWarningDialogForFatalError()
	{
		if (HearthstoneApplication.IsPublic())
		{
			return false;
		}
		if (!DialogManager.Get())
		{
			return false;
		}
		return !Options.Get().GetBool(Option.ERROR_SCREEN);
	}

	private static void ShowWarningDialog(ErrorParams parms)
	{
		parms.m_type = ErrorType.WARNING;
		AlertPopup.PopupInfo info = new AlertPopup.PopupInfo();
		info.m_id = parms.m_header + parms.m_message;
		info.m_headerText = parms.m_header;
		info.m_text = parms.m_message;
		info.m_responseCallback = OnWarningPopupResponse;
		info.m_responseUserData = parms;
		info.m_showAlertIcon = true;
		DialogManager.Get().ShowPopup(info);
	}

	private static void OnWarningPopupResponse(AlertPopup.Response response, object userData)
	{
		ErrorParams parms = (ErrorParams)userData;
		if (parms.m_ackCallback != null)
		{
			parms.m_ackCallback(parms.m_ackUserData);
		}
	}
}
