using System;
using System.Collections.Generic;
using Blizzard.T5.Core.Utils;
using Blizzard.T5.Jobs;
using Blizzard.T5.Services;
using PegasusGame;

public class LuaLogs : IService
{
	public enum ListenableScriptType
	{
		INVALID,
		QUEST,
		ACHIEVE,
		TASK
	}

	private enum ListenActionType
	{
		IDLE,
		LISTEN,
		CLEAR
	}

	private struct ListenAction
	{
		public ListenActionType Action;

		public int PlayerId;

		public int ScriptId;

		public ListenableScriptType ScriptType;

		public static ListenAction CreateEmpty()
		{
			return new ListenAction(ListenActionType.IDLE, 0, 0, ListenableScriptType.INVALID);
		}

		public ListenAction(ListenActionType action, int playerId, int scriptId, ListenableScriptType scriptType)
		{
			Action = action;
			PlayerId = playerId;
			ScriptId = scriptId;
			ScriptType = scriptType;
		}

		public void SetAsListen(int playerId, int scriptId, ListenableScriptType type)
		{
			Action = ListenActionType.LISTEN;
			PlayerId = playerId;
			ScriptId = scriptId;
			ScriptType = type;
		}

		public void SetAsClearListen(int playerId)
		{
			Action = ListenActionType.LISTEN;
			PlayerId = playerId;
			ScriptId = 0;
			ScriptType = ListenableScriptType.INVALID;
		}
	}

	private ListenAction m_currentAction;

	public IEnumerator<IAsyncJobResult> Initialize(ServiceLocator serviceLocator)
	{
		Network network = serviceLocator.Get<Network>();
		m_currentAction = ListenAction.CreateEmpty();
		network.RegisterNetHandler(GameSetup.PacketID.ID, OnGameSetup);
		yield break;
	}

	public Type[] GetDependencies()
	{
		return new Type[2]
		{
			typeof(Network),
			typeof(CheatMgr)
		};
	}

	public void Shutdown()
	{
		if (ServiceManager.TryGet<Network>(out var network))
		{
			network.RemoveNetHandler(GameSetup.PacketID.ID, OnGameSetup);
		}
	}

	private void OnGameSetup()
	{
		if (m_currentAction.Action == ListenActionType.LISTEN)
		{
			EnableListeningOnGameServer();
		}
		else if (m_currentAction.Action == ListenActionType.CLEAR)
		{
			ClearListenOnGameServer();
		}
	}

	private void ClearListenOnGameServer()
	{
		Network network = ServiceManager.Get<Network>();
		if (network != null && network.IsConnectedToGameServer())
		{
			CheatMgr.Get()?.RunCheatInternally($"cheat luaclearlisten {m_currentAction.PlayerId}");
		}
	}

	public void ClearListenOnGameServer(int playerId)
	{
		m_currentAction.SetAsClearListen(playerId);
		ClearListenOnGameServer();
	}

	public void ListenOnGameServer(int playerId, int scriptId, ListenableScriptType scriptType)
	{
		m_currentAction.SetAsListen(playerId, scriptId, scriptType);
		EnableListeningOnGameServer();
	}

	private void EnableListeningOnGameServer()
	{
		Network network = ServiceManager.Get<Network>();
		if (network != null && network.IsConnectedToGameServer())
		{
			CheatMgr cheatMgr = CheatMgr.Get();
			if (cheatMgr != null)
			{
				string cheatMessage = $"cheat lua{EnumUtils.GetString(m_currentAction.ScriptType).ToLower()}listen {m_currentAction.ScriptId} {m_currentAction.PlayerId}";
				cheatMgr.RunCheatInternally(cheatMessage);
			}
		}
	}
}
