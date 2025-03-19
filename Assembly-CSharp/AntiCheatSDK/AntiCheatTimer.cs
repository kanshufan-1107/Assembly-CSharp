using System;
using System.Timers;
using Blizzard.T5.Core;

namespace AntiCheatSDK;

internal class AntiCheatTimer : IDisposable
{
	private bool m_disposed;

	private bool m_isCalledStart;

	private Action m_callback;

	public ILogger m_logger;

	private Timer CallSDKTimer { get; set; }

	private int Interval
	{
		get
		{
			return Options.Get().GetInt(Option.CALLSDK_INTERVAL, 300);
		}
		set
		{
			Options.Get().SetInt(Option.CALLSDK_INTERVAL, value);
		}
	}

	private void OnTimedEvent(object source, ElapsedEventArgs e)
	{
		m_logger.Log(LogLevel.Debug, "hb called");
		m_callback?.Invoke();
	}

	private void OnFeaturesUpdated()
	{
		NetCache.NetCacheFeatures features = NetCache.Get().GetNetObject<NetCache.NetCacheFeatures>();
		UpdateInterval(features.CallSDKInterval);
	}

	private void CreateTimer()
	{
		if (CallSDKTimer == null)
		{
			CallSDKTimer = new Timer(Interval * 1000);
			CallSDKTimer.Elapsed += OnTimedEvent;
			CallSDKTimer.AutoReset = true;
			CallSDKTimer.Enabled = true;
		}
	}

	public AntiCheatTimer(ILogger logger, Action callback)
	{
		m_logger = logger;
		m_callback = callback;
		NetCache.Get().RegisterUpdatedListener(typeof(NetCache.NetCacheFeatures), OnFeaturesUpdated);
	}

	~AntiCheatTimer()
	{
		Dispose();
	}

	public void Dispose()
	{
		if (!m_disposed)
		{
			CallSDKTimer.Elapsed -= OnTimedEvent;
			CallSDKTimer.Stop();
			CallSDKTimer.Dispose();
			CallSDKTimer = null;
			m_disposed = true;
		}
	}

	public void Start()
	{
		if (CallSDKTimer == null && Interval > 0)
		{
			CreateTimer();
			CallSDKTimer.Start();
		}
		m_isCalledStart = true;
	}

	public void UpdateInterval(int interval)
	{
		if (interval < 0)
		{
			m_logger.Log(LogLevel.Information, $"Ignored invalid interval value: {interval}");
			return;
		}
		if (interval == Interval)
		{
			m_logger.Log(LogLevel.Debug, $"Same interval, Ignored: {interval}");
			return;
		}
		Interval = interval;
		if (CallSDKTimer == null)
		{
			if (!m_isCalledStart)
			{
				m_logger.Log(LogLevel.Debug, "Timer is not ready yet.");
				return;
			}
			CreateTimer();
		}
		if (interval == 0)
		{
			CallSDKTimer.Stop();
		}
		else
		{
			CallSDKTimer.Interval = interval * 1000;
		}
	}
}
