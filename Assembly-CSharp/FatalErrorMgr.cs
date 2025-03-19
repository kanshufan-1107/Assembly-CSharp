using System.Collections.Generic;
using System.Threading;
using Blizzard.T5.Core.Utils;
using Blizzard.T5.Jobs;
using Hearthstone;
using Hearthstone.Core;

public class FatalErrorMgr
{
	public delegate void ErrorCallback(FatalErrorMessage message, object userData);

	protected class ErrorListener : EventListener<ErrorCallback>
	{
		public void Fire(FatalErrorMessage message)
		{
			if (GeneralUtils.IsCallbackValid(m_callback))
			{
				m_callback(message, m_userData);
			}
		}
	}

	private const string UPDATE_GETSTATUS_JOB_NAME = "FatalErrorMgr.";

	private static FatalErrorMgr s_instance;

	private List<FatalErrorMessage> m_messages = new List<FatalErrorMessage>();

	private List<ErrorListener> m_errorListeners = new List<ErrorListener>();

	private string m_generatedErrorCode;

	private object m_lock = new object();

	public bool IsUnrecoverable { get; private set; }

	private int PosToFireMessage { get; set; } = -1;

	public static FatalErrorMgr Get()
	{
		if (s_instance == null)
		{
			s_instance = new FatalErrorMgr();
		}
		return s_instance;
	}

	public static bool IsInitialized()
	{
		return s_instance != null;
	}

	public void RunProcessJob()
	{
		Processor.QueueJobIfNotExist("FatalErrorMgr.", Job_ProcessMessages());
	}

	public void Add(FatalErrorMessage message)
	{
		RunProcessJob();
		lock (m_lock)
		{
			m_messages.Add(message);
			if (HearthstoneApplication.IsMainThread)
			{
				FireErrorListeners(message);
			}
			else if (PosToFireMessage == -1)
			{
				PosToFireMessage = m_messages.Count - 1;
			}
		}
	}

	public void SetErrorCode(string prefixSource, string errorSubset1, string errorSubset2 = null, string errorSubset3 = null)
	{
		m_generatedErrorCode = prefixSource + ":" + errorSubset1;
		if (errorSubset2 != null)
		{
			m_generatedErrorCode = m_generatedErrorCode + ":" + errorSubset2;
		}
		if (errorSubset3 != null)
		{
			m_generatedErrorCode = m_generatedErrorCode + ":" + errorSubset3;
		}
	}

	public void ClearAllErrors()
	{
		while (PosToFireMessage != -1)
		{
			Thread.Sleep(100);
		}
		lock (m_lock)
		{
			m_messages.Clear();
		}
		m_generatedErrorCode = null;
	}

	public bool AddErrorListener(ErrorCallback callback)
	{
		return AddErrorListener(callback, null);
	}

	public bool AddErrorListener(ErrorCallback callback, object userData)
	{
		ErrorListener listener = new ErrorListener();
		listener.SetCallback(callback);
		listener.SetUserData(userData);
		if (m_errorListeners.Contains(listener))
		{
			return false;
		}
		m_errorListeners.Add(listener);
		return true;
	}

	public bool RemoveErrorListener(ErrorCallback callback)
	{
		return RemoveErrorListener(callback, null);
	}

	public bool RemoveErrorListener(ErrorCallback callback, object userData)
	{
		ErrorListener listener = new ErrorListener();
		listener.SetCallback(callback);
		listener.SetUserData(userData);
		return m_errorListeners.Remove(listener);
	}

	public FatalErrorMessage[] GetMessages()
	{
		lock (m_lock)
		{
			return m_messages.ToArray();
		}
	}

	public string GetFormattedErrorCode()
	{
		return m_generatedErrorCode;
	}

	public bool HasError()
	{
		lock (m_lock)
		{
			return m_messages.Count > 0;
		}
	}

	public void NotifyExitPressed()
	{
		SendAcknowledgements();
		HearthstoneApplication.Get().Exit();
	}

	public static bool IsReconnectAllowedBasedOnFatalErrorReason(FatalErrorReason reason)
	{
		switch (reason)
		{
		case FatalErrorReason.LOGIN_FROM_ANOTHER_DEVICE:
		case FatalErrorReason.ADMIN_KICK_OR_BAN:
		case FatalErrorReason.ACCOUNT_SETUP_ERROR:
		case FatalErrorReason.MOBILE_GAME_SERVER_RPC_ERROR:
			return false;
		case FatalErrorReason.BREAKING_NEWS:
			if (SceneMgr.Get().GetMode() == SceneMgr.Mode.STARTUP)
			{
				return false;
			}
			break;
		}
		return true;
	}

	public void SetUnrecoverable(bool isUnrecoverable)
	{
		IsUnrecoverable = isUnrecoverable;
	}

	private void SendAcknowledgements()
	{
		FatalErrorMessage[] messages = GetMessages();
		foreach (FatalErrorMessage message in messages)
		{
			if (message.m_ackCallback != null)
			{
				message.m_ackCallback(message.m_ackUserData);
			}
		}
	}

	private IEnumerator<IAsyncJobResult> Job_ProcessMessages()
	{
		while (!HearthstoneApplication.Get().IsExiting())
		{
			if (PosToFireMessage == -1)
			{
				yield return new WaitForDurationForWorker(500.0);
			}
			lock (m_lock)
			{
				if (PosToFireMessage != -1)
				{
					FatalErrorMessage[] messages = GetMessages();
					for (int i = PosToFireMessage; i < messages.Length; i++)
					{
						FireErrorListeners(messages[i]);
					}
					PosToFireMessage = -1;
				}
			}
		}
	}

	protected void FireErrorListeners(FatalErrorMessage message)
	{
		ErrorListener[] listeners = m_errorListeners.ToArray();
		for (int i = 0; i < listeners.Length; i++)
		{
			listeners[i].Fire(message);
		}
	}
}
