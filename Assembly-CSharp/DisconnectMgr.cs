using System;
using System.Collections.Generic;
using Blizzard.T5.Core;
using Blizzard.T5.Jobs;
using Blizzard.T5.Services;

public class DisconnectMgr : IService
{
	private AlertPopup m_dialog;

	private static ILogger GameNetLogger => Network.Get().GameNetLogger;

	public IEnumerator<IAsyncJobResult> Initialize(ServiceLocator serviceLocator)
	{
		yield break;
	}

	public Type[] GetDependencies()
	{
		return new Type[2]
		{
			typeof(GameMgr),
			typeof(Network)
		};
	}

	public void Shutdown()
	{
		if (ServiceManager.TryGet<SceneMgr>(out var sceneMgr))
		{
			sceneMgr.UnregisterSceneLoadedEvent(OnSceneLoaded);
		}
	}

	public static DisconnectMgr Get()
	{
		return ServiceManager.Get<DisconnectMgr>();
	}

	public void DisconnectFromGameplay()
	{
		GameNetLogger.Log(LogLevel.Debug, "DisconnectMgr.DisconnectFromGameplay()");
		PerformanceAnalytics.Get()?.DisconnectEvent(SceneMgr.Get().GetMode().ToString());
		SceneMgr.Mode nextMode = GameMgr.Get().GetPostDisconnectSceneMode();
		GameMgr.Get().PreparePostGameSceneMode(nextMode);
		if (nextMode == SceneMgr.Mode.INVALID)
		{
			Network.Get().ShowBreakingNewsOrError("GLOBAL_ERROR_NETWORK_LOST_GAME_CONNECTION");
		}
		else if (Network.Get().WasDisconnectRequested())
		{
			SceneMgr.Get().SetNextMode(nextMode);
		}
		else
		{
			ShowGameplayDialog(nextMode);
		}
	}

	private void ShowGameplayDialog(SceneMgr.Mode nextMode)
	{
		GameNetLogger.Log(LogLevel.Debug, "DisconnectMgr.ShowGameplayDialog() - nextMode: " + nextMode);
		AlertPopup.PopupInfo info = new AlertPopup.PopupInfo();
		info.m_headerText = GameStrings.Get("GLOBAL_ERROR_NETWORK_TITLE");
		info.m_text = GameStrings.Get("GLOBAL_ERROR_NETWORK_LOST_GAME_CONNECTION");
		info.m_responseDisplay = AlertPopup.ResponseDisplay.NONE;
		info.m_layerToUse = GameLayer.UI;
		DialogManager.Get().ShowPopup(info, OnGameplayDialogProcessed, nextMode);
	}

	private bool OnGameplayDialogProcessed(DialogBase dialog, object userData)
	{
		GameNetLogger.Log(LogLevel.Debug, "DisconnectMgr.OnGameplayDialogProcessed()");
		m_dialog = (AlertPopup)dialog;
		SceneMgr.Mode nextMode = (SceneMgr.Mode)userData;
		SceneMgr.Get().SetNextMode(nextMode);
		SceneMgr.Get().RegisterSceneLoadedEvent(OnSceneLoaded);
		return true;
	}

	private void OnSceneLoaded(SceneMgr.Mode mode, PegasusScene scene, object userData)
	{
		SceneMgr.Get().UnregisterSceneLoadedEvent(OnSceneLoaded, userData);
		UpdateGameplayDialog();
	}

	private void UpdateGameplayDialog()
	{
		GameNetLogger.Log(LogLevel.Debug, "DisconnectMgr.UpdateGameplayDialog()");
		if (m_dialog != null)
		{
			AlertPopup.PopupInfo info = m_dialog.GetInfo();
			info.m_responseDisplay = AlertPopup.ResponseDisplay.OK;
			info.m_responseCallback = OnGameplayDialogResponse;
			m_dialog.UpdateInfo(info);
		}
	}

	private void OnGameplayDialogResponse(AlertPopup.Response response, object userData)
	{
		GameNetLogger.Log(LogLevel.Debug, "DisconnectMgr.OnGameplayDialogResponse()");
		m_dialog = null;
		if (!Network.IsLoggedIn())
		{
			Network.Get().ShowBreakingNewsOrError("GLOBAL_ERROR_NETWORK_LOST_GAME_CONNECTION");
		}
		else
		{
			SpectatorManager.Get().LeaveSpectatorMode();
		}
	}
}
