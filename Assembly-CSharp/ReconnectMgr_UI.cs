using System;
using System.Collections;
using Blizzard.T5.Core;
using Blizzard.T5.Jobs;
using Hearthstone;
using Hearthstone.Core;
using Hearthstone.Core.Streaming;
using Hearthstone.Streaming;
using PegasusShared;
using UnityEngine;

public class ReconnectMgr_UI
{
	private static AlertPopup m_gameplayReconnectDialog;

	private static Coroutine m_introPopupCoroutine;

	private static readonly Map<GameType, string> m_gameTypeNameKeys = new Map<GameType, string>
	{
		{
			GameType.GT_VS_FRIEND,
			"GLUE_RECONNECT_GAME_TYPE_FRIENDLY"
		},
		{
			GameType.GT_ARENA,
			"GLUE_RECONNECT_GAME_TYPE_ARENA"
		},
		{
			GameType.GT_CASUAL,
			"GLUE_RECONNECT_GAME_TYPE_UNRANKED"
		},
		{
			GameType.GT_RANKED,
			"GLUE_RECONNECT_GAME_TYPE_RANKED"
		},
		{
			GameType.GT_TAVERNBRAWL,
			"GLUE_RECONNECT_GAME_TYPE_TAVERN_BRAWL"
		},
		{
			GameType.GT_FSG_BRAWL_VS_FRIEND,
			"GLUE_RECONNECT_GAME_TYPE_FRIENDLY"
		},
		{
			GameType.GT_FSG_BRAWL,
			"GLUE_RECONNECT_GAME_TYPE_TAVERN_BRAWL"
		},
		{
			GameType.GT_FSG_BRAWL_1P_VS_AI,
			"GLUE_RECONNECT_GAME_TYPE_TAVERN_BRAWL"
		},
		{
			GameType.GT_FSG_BRAWL_2P_COOP,
			"GLUE_RECONNECT_GAME_TYPE_TAVERN_BRAWL"
		}
	};

	public static void Reset()
	{
		ClearDialog();
		StopIntroPopupCoroutineIfRunning();
	}

	public static void ClearDialog()
	{
		m_gameplayReconnectDialog = null;
	}

	public static bool ShowDisconnectedGameResult(NetCache.ProfileNoticeDisconnectedGame dcGame, Blizzard.T5.Core.ILogger GameNetLogger)
	{
		if (!GameUtils.IsMatchmadeGameType(dcGame.GameType, null))
		{
			return false;
		}
		TimeSpan timeSinceDisconnect = DateTime.UtcNow - DateTime.FromFileTimeUtc(dcGame.Date);
		GameNetLogger.Log(LogLevel.Information, "ReconnectMgr.ShowDisconnectedGameResult() - This user disconnected from his or her last game {0} minutes ago.", timeSinceDisconnect.TotalMinutes);
		if (timeSinceDisconnect.TotalHours > 24.0)
		{
			GameNetLogger.Log(LogLevel.Information, "ReconnectMgr.ShowDisconnectedGameResult() - Not showing the Disconnected Game Result because the game was disconnected from {0} hours ago.", timeSinceDisconnect.TotalHours);
			return false;
		}
		ProfileNoticeDisconnectedGameResult.GameResult gameResult = dcGame.GameResult;
		if ((uint)(gameResult - 2) > 1u)
		{
			GameNetLogger.Log(LogLevel.Error, $"ReconnectMgr.ShowDisconnectedGameResult() - unhandled game result {dcGame.GameResult}");
			return false;
		}
		if (dcGame.GameType == GameType.GT_UNKNOWN)
		{
			return false;
		}
		AlertPopup.PopupInfo info = new AlertPopup.PopupInfo();
		info.m_headerText = GameStrings.Get("GLUE_RECONNECT_RESULT_HEADER");
		string playerResultKey = null;
		if (dcGame.GameResult == ProfileNoticeDisconnectedGameResult.GameResult.GR_TIE)
		{
			playerResultKey = "GLUE_RECONNECT_RESULT_TIE";
		}
		else
		{
			switch (dcGame.YourResult)
			{
			case ProfileNoticeDisconnectedGameResult.PlayerResult.PR_WON:
				playerResultKey = "GLUE_RECONNECT_RESULT_WIN";
				break;
			case ProfileNoticeDisconnectedGameResult.PlayerResult.PR_LOST:
			case ProfileNoticeDisconnectedGameResult.PlayerResult.PR_QUIT:
				playerResultKey = "GLUE_RECONNECT_RESULT_LOSE";
				break;
			case ProfileNoticeDisconnectedGameResult.PlayerResult.PR_DISCONNECTED:
				playerResultKey = "GLUE_RECONNECT_RESULT_DISCONNECT";
				break;
			default:
				GameNetLogger.Log(LogLevel.Error, $"ReconnectMgr.ShowDisconnectedGameResult() - unhandled player result {dcGame.YourResult}");
				return false;
			}
		}
		info.m_text = GameStrings.Format(playerResultKey, GetGameTypeName(dcGame.GameType, dcGame.MissionId));
		info.m_responseDisplay = AlertPopup.ResponseDisplay.OK;
		info.m_showAlertIcon = true;
		DialogManager.Get().ShowPopup(info);
		return true;
	}

	private static string GetGameTypeName(GameType gameType, int missionId)
	{
		if (gameType == GameType.GT_BATTLEGROUNDS || gameType == GameType.GT_BATTLEGROUNDS_DUO || gameType == GameType.GT_BATTLEGROUNDS_FRIENDLY || gameType == GameType.GT_BATTLEGROUNDS_DUO_FRIENDLY)
		{
			return GameStrings.Get("GLUE_RECONNECT_GAME_TYPE_BATTLEGROUNDS");
		}
		AdventureDbfRecord adventureRec = GameUtils.GetAdventureRecordFromMissionId(missionId);
		if (adventureRec != null)
		{
			return adventureRec.ID switch
			{
				1 => GameStrings.Get("GLUE_RECONNECT_GAME_TYPE_TUTORIAL"), 
				2 => GameStrings.Get("GLUE_RECONNECT_GAME_TYPE_PRACTICE"), 
				3 => GameStrings.Get("GLUE_RECONNECT_GAME_TYPE_NAXXRAMAS"), 
				4 => GameStrings.Get("GLUE_RECONNECT_GAME_TYPE_BRM"), 
				7 => GameStrings.Get("GLUE_RECONNECT_GAME_TYPE_TAVERN_BRAWL"), 
				_ => adventureRec.Name, 
			};
		}
		if (m_gameTypeNameKeys.TryGetValue(gameType, out var key))
		{
			return GameStrings.Get(key);
		}
		Error.AddDevFatal("ReconnectMgr.GetGameTypeName() - no name for mission {0} gameType {1}", missionId, gameType);
		return string.Empty;
	}

	public static void ShowGameplayReconnectingDialog(GameReconnectType gameReconnectType)
	{
		BnetBar bnetBar = BnetBar.Get();
		if (bnetBar != null)
		{
			bnetBar.HideSkipTutorialButton();
		}
		AlertPopup.PopupInfo info = new AlertPopup.PopupInfo();
		info.m_headerText = GameStrings.Get("GLOBAL_RECONNECT_RECONNECTING_HEADER");
		if (gameReconnectType == GameReconnectType.LOGIN)
		{
			info.m_text = GameStrings.Get("GLOBAL_RECONNECT_RECONNECTING_LOGIN");
		}
		else
		{
			info.m_text = GameStrings.Get("GLOBAL_RECONNECT_RECONNECTING");
		}
		if ((bool)HearthstoneApplication.CanQuitGame)
		{
			info.m_responseDisplay = AlertPopup.ResponseDisplay.CANCEL;
			info.m_cancelText = GameStrings.Get("GLOBAL_RECONNECT_EXIT_BUTTON");
		}
		else
		{
			info.m_responseDisplay = AlertPopup.ResponseDisplay.NONE;
		}
		info.m_showAlertIcon = true;
		info.m_responseCallback = OnGameplayReconnectingDialogResponse;
		DialogManager.Get().ShowPopup(info, (DialogBase dialog, object userData) => OnGameplayReconnectingDialogProcessed(dialog, userData, gameReconnectType));
	}

	private static void OnGameplayReconnectingDialogResponse(AlertPopup.Response response, object userData)
	{
		m_gameplayReconnectDialog = null;
		HearthstoneApplication.Get().Exit();
	}

	private static bool OnGameplayReconnectingDialogProcessed(DialogBase dialog, object userData, GameReconnectType gameReconnectType)
	{
		if (gameReconnectType == GameReconnectType.INVALID)
		{
			return false;
		}
		m_gameplayReconnectDialog = (AlertPopup)dialog;
		if (GameMgr.Get().IsReconnect() && SceneMgr.Get().IsGoingToLoadGameplay())
		{
			ChangeGameplayDialogToReconnected(gameReconnectType);
		}
		return true;
	}

	public static void ChangeGameplayDialogToReconnected(GameReconnectType gameReconnectType)
	{
		if (!(m_gameplayReconnectDialog == null))
		{
			AlertPopup.PopupInfo info = new AlertPopup.PopupInfo();
			info.m_headerText = GameStrings.Get("GLOBAL_RECONNECT_RECONNECTED_HEADER");
			if (gameReconnectType == GameReconnectType.LOGIN)
			{
				info.m_text = GameStrings.Get("GLOBAL_RECONNECT_RECONNECTED_LOGIN");
			}
			else
			{
				info.m_text = GameStrings.Get("GLOBAL_RECONNECT_RECONNECTED");
			}
			info.m_responseDisplay = AlertPopup.ResponseDisplay.NONE;
			info.m_showAlertIcon = true;
			m_gameplayReconnectDialog.UpdateInfo(info);
			LoadingScreen.Get().RegisterPreviousSceneDestroyedListener(OnPreviousSceneDestroyed);
		}
	}

	private static void OnPreviousSceneDestroyed(object userData)
	{
		LoadingScreen.Get().UnregisterPreviousSceneDestroyedListener(OnPreviousSceneDestroyed);
		HideGameplayReconnectDialog();
	}

	public static void HideGameplayReconnectDialog()
	{
		if (!(m_gameplayReconnectDialog == null))
		{
			m_gameplayReconnectDialog.Hide();
			ClearDialog();
		}
	}

	public static void ShowModeNotAvailableWarning(DownloadTags.Content tag)
	{
		AlertPopup.PopupInfo info = new AlertPopup.PopupInfo();
		info.m_headerText = GameStrings.Get("GLOBAL_RECONNECT_RECONNECTING_HEADER");
		string mode = DownloadUtils.GetGameModeName(tag);
		info.m_text = GameStrings.Format("GLOBAL_RECONNECT_FAIL_MODE_NOT_AVAILABLE", mode);
		info.m_responseDisplay = AlertPopup.ResponseDisplay.OK;
		info.m_showAlertIcon = true;
		info.m_responseCallback = delegate
		{
			ClearDialog();
		};
		DialogManager.Get().ShowPopup(info, delegate(DialogBase dialog, object userdata)
		{
			m_gameplayReconnectDialog = (AlertPopup)dialog;
			return true;
		});
	}

	public static void ChangeGameplayDialogToTimeout()
	{
		if (!(m_gameplayReconnectDialog == null))
		{
			AlertPopup.PopupInfo info = new AlertPopup.PopupInfo();
			info.m_headerText = GameStrings.Get("GLOBAL_RECONNECT_TIMEOUT_HEADER");
			info.m_text = GameStrings.Get("GLOBAL_RECONNECT_TIMEOUT");
			info.m_responseDisplay = AlertPopup.ResponseDisplay.OK;
			info.m_showAlertIcon = true;
			info.m_responseCallback = OnTimeoutGameplayDialogResponse;
			m_gameplayReconnectDialog.UpdateInfo(info);
		}
	}

	private static void OnTimeoutGameplayDialogResponse(AlertPopup.Response response, object userData)
	{
		ClearDialog();
		if (!Network.IsLoggedIn())
		{
			if ((bool)HearthstoneApplication.AllowResetFromFatalError)
			{
				HearthstoneApplication.Get().Reset();
			}
			else
			{
				HearthstoneApplication.Get().Exit();
			}
		}
	}

	public static void ShowIntroPopups()
	{
		StopIntroPopupCoroutineIfRunning();
		m_introPopupCoroutine = Processor.RunCoroutine(ShowIntroPopupsCoroutine());
	}

	private static void StopIntroPopupCoroutineIfRunning()
	{
		if (m_introPopupCoroutine != null)
		{
			Processor.CancelCoroutine(m_introPopupCoroutine);
			m_introPopupCoroutine = null;
		}
	}

	private static IEnumerator ShowIntroPopupsCoroutine()
	{
		while (DialogManager.Get().ShowingDialog())
		{
			yield return new WaitForEndOfFrame();
		}
		while (CollectionManager.Get() == null || !CollectionManager.Get().IsFullyLoaded())
		{
			yield return new WaitForEndOfFrame();
		}
		JobDefinition waitForIntroPopups = Processor.QueueJob("LoginManager.ShowIntroPopups", LoginManager.Get().ShowIntroPopups());
		Processor.QueueJob("LoginManager.CompleteLoginFlow", LoginManager.Get().CompleteLoginFlow(), waitForIntroPopups.CreateDependency());
		m_introPopupCoroutine = null;
	}
}
