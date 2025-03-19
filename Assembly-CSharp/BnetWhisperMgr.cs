using System.Collections.Generic;
using Blizzard.GameService.SDK.Client.Integration;
using Blizzard.T5.Core;
using Hearthstone;

public class BnetWhisperMgr
{
	public delegate void WhisperCallback(BnetWhisper whisper, object userData);

	private class WhisperListener : EventListener<WhisperCallback>
	{
		public void Fire(BnetWhisper whisper)
		{
			m_callback(whisper, m_userData);
		}
	}

	private static BnetWhisperMgr s_instance;

	private List<BnetWhisper> m_whispers = new List<BnetWhisper>();

	private Map<BnetAccountId, List<BnetWhisper>> m_whisperMap = new Map<BnetAccountId, List<BnetWhisper>>();

	private int m_firstPendingWhisperIndex = -1;

	private List<WhisperListener> m_whisperListeners = new List<WhisperListener>();

	public static BnetWhisperMgr Get()
	{
		if (s_instance == null)
		{
			s_instance = new BnetWhisperMgr();
			HearthstoneApplication.Get().WillReset += delegate
			{
				s_instance.m_whispers.Clear();
				s_instance.m_whisperMap.Clear();
				s_instance.m_firstPendingWhisperIndex = -1;
				BnetPresenceMgr.Get().RemovePlayersChangedListener(Get().OnPlayersChanged);
			};
		}
		return s_instance;
	}

	public void Initialize()
	{
		Network.Get().SetWhisperHandler(OnWhispers);
		Network.Get().AddBnetErrorListener(BnetFeature.Whisper, OnBnetError);
	}

	public List<BnetWhisper> GetWhispersWithPlayer(BnetPlayer player)
	{
		if (player == null)
		{
			return null;
		}
		List<BnetWhisper> playerWhispers = new List<BnetWhisper>();
		BnetAccountId accountId = player.GetAccountId();
		if (m_whisperMap.TryGetValue(accountId, out var whispers))
		{
			playerWhispers.AddRange(whispers);
		}
		if (playerWhispers.Count == 0)
		{
			return null;
		}
		playerWhispers.Sort(delegate(BnetWhisper a, BnetWhisper b)
		{
			ulong timestampMicrosec = a.GetTimestampMicrosec();
			ulong timestampMicrosec2 = b.GetTimestampMicrosec();
			if (timestampMicrosec < timestampMicrosec2)
			{
				return -1;
			}
			return (timestampMicrosec > timestampMicrosec2) ? 1 : 0;
		});
		return playerWhispers;
	}

	public bool SendWhisper(BnetPlayer player, string message)
	{
		if (player == null)
		{
			return false;
		}
		BnetPlayer myself = BnetPresenceMgr.Get().GetMyPlayer();
		if (myself == null || !myself.IsOnline())
		{
			return false;
		}
		BnetAccountId accountId = player.GetAccountId();
		if (accountId == null)
		{
			return false;
		}
		Network.SendWhisper(accountId, message);
		return true;
	}

	public bool HavePendingWhispers()
	{
		return m_firstPendingWhisperIndex >= 0;
	}

	public bool AddWhisperListener(WhisperCallback callback)
	{
		return AddWhisperListener(callback, null);
	}

	public bool AddWhisperListener(WhisperCallback callback, object userData)
	{
		WhisperListener listener = new WhisperListener();
		listener.SetCallback(callback);
		listener.SetUserData(userData);
		if (m_whisperListeners.Contains(listener))
		{
			return false;
		}
		m_whisperListeners.Add(listener);
		return true;
	}

	public bool RemoveWhisperListener(WhisperCallback callback)
	{
		return RemoveWhisperListener(callback, null);
	}

	public bool RemoveWhisperListener(WhisperCallback callback, object userData)
	{
		WhisperListener listener = new WhisperListener();
		listener.SetCallback(callback);
		listener.SetUserData(userData);
		return m_whisperListeners.Remove(listener);
	}

	private void OnWhispers(BnetWhisper[] whispers)
	{
		for (int i = 0; i < whispers.Length; i++)
		{
			BnetWhisper whisper = whispers[i];
			m_whispers.Add(whisper);
			if (!HavePendingWhispers())
			{
				if (WhisperUtil.IsDisplayable(whisper))
				{
					ProcessWhisper(m_whispers.Count - 1);
					continue;
				}
				m_firstPendingWhisperIndex = i;
				BnetPresenceMgr.Get().AddPlayersChangedListener(OnPlayersChanged);
			}
		}
	}

	private bool OnBnetError(BnetErrorInfo info, object userData)
	{
		return true;
	}

	private void OnPlayersChanged(BnetPlayerChangelist changelist, object userData)
	{
		if (CanProcessPendingWhispers())
		{
			BnetPresenceMgr.Get().RemovePlayersChangedListener(OnPlayersChanged);
			ProcessPendingWhispers();
		}
	}

	private void FireWhisperEvent(BnetWhisper whisper)
	{
		WhisperListener[] listeners = m_whisperListeners.ToArray();
		for (int i = 0; i < listeners.Length; i++)
		{
			listeners[i].Fire(whisper);
		}
	}

	private bool CanProcessPendingWhispers()
	{
		if (m_firstPendingWhisperIndex < 0)
		{
			return true;
		}
		for (int i = m_firstPendingWhisperIndex; i < m_whispers.Count; i++)
		{
			if (!WhisperUtil.IsDisplayable(m_whispers[i]))
			{
				return false;
			}
		}
		return true;
	}

	private void ProcessPendingWhispers()
	{
		if (m_firstPendingWhisperIndex >= 0)
		{
			for (int i = m_firstPendingWhisperIndex; i < m_whispers.Count; i++)
			{
				ProcessWhisper(i);
			}
			m_firstPendingWhisperIndex = -1;
		}
	}

	private void ProcessWhisper(int index)
	{
		BnetWhisper whisper = m_whispers[index];
		BnetAccountId theirId = WhisperUtil.GetTheirAccountId(whisper);
		if (theirId == null || !BnetUtils.CanReceiveWhisperFrom(theirId))
		{
			m_whispers.RemoveAt(index);
			return;
		}
		if (!m_whisperMap.TryGetValue(theirId, out var whispers))
		{
			whispers = new List<BnetWhisper>();
			m_whisperMap.Add(theirId, whispers);
		}
		else if (whispers.Count == 100)
		{
			RemoveOldestWhisper(whispers);
		}
		whispers.Add(whisper);
		FireWhisperEvent(whisper);
	}

	private void RemoveOldestWhisper(List<BnetWhisper> whispers)
	{
		BnetWhisper oldestWhisper = whispers[0];
		whispers.RemoveAt(0);
		m_whispers.Remove(oldestWhisper);
	}
}
