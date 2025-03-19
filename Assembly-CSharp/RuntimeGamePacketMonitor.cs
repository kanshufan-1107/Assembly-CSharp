using System;
using Blizzard.GameService.SDK.Client.Integration;
using Blizzard.T5.Core;
using PegasusGame;
using UnityEngine;

public class RuntimeGamePacketMonitor : IDisposable
{
	private IDispatcherListener m_dispatcherListener;

	private int m_previousOptionsID = -1;

	public RuntimeGamePacketMonitor(IDispatcherListener dispatcherListener)
	{
		m_dispatcherListener = dispatcherListener;
		IDispatcherListener dispatcherListener2 = m_dispatcherListener;
		dispatcherListener2.OnGameServerConnect = (Action<BattleNetErrors>)Delegate.Combine(dispatcherListener2.OnGameServerConnect, new Action<BattleNetErrors>(OnGameServerConnect));
		IDispatcherListener dispatcherListener3 = m_dispatcherListener;
		dispatcherListener3.OnGamePacketReceived = (Action<PegasusPacket>)Delegate.Combine(dispatcherListener3.OnGamePacketReceived, new Action<PegasusPacket>(OnGamePacketReceived));
	}

	private void OnGameServerConnect(BattleNetErrors errors)
	{
		m_previousOptionsID = -1;
	}

	private void OnGamePacketReceived(PegasusPacket packet)
	{
		if (packet.Type == 14)
		{
			CheckAllOptionsValidity(packet);
		}
	}

	private void CheckAllOptionsValidity(PegasusPacket decodedPacket)
	{
		if (!GameMgr.Get().IsBattlegrounds())
		{
			return;
		}
		NetCache.NetCacheFeatures features = NetCache.Get()?.GetNetObject<NetCache.NetCacheFeatures>();
		if (features != null && !features.EnableAllOptionsIDCheck)
		{
			return;
		}
		AllOptions options = (AllOptions)decodedPacket.Body;
		if (options == null)
		{
			return;
		}
		if (m_previousOptionsID != -1 && options.Id != m_previousOptionsID + 1)
		{
			if (Network.Get() != null)
			{
				Network.Get().GameNetLogger.Log(Blizzard.T5.Core.LogLevel.Error, $"Recevied AllOptions packet with ID {options.Id} but previous packet had ID {m_previousOptionsID}");
			}
			else
			{
				Debug.LogError($"Recevied AllOptions packet with ID {options.Id} but previous packet had ID {m_previousOptionsID}");
			}
		}
		m_previousOptionsID = options.Id;
	}

	public void Dispose()
	{
		IDispatcherListener dispatcherListener = m_dispatcherListener;
		dispatcherListener.OnGameServerConnect = (Action<BattleNetErrors>)Delegate.Remove(dispatcherListener.OnGameServerConnect, new Action<BattleNetErrors>(OnGameServerConnect));
		IDispatcherListener dispatcherListener2 = m_dispatcherListener;
		dispatcherListener2.OnGamePacketReceived = (Action<PegasusPacket>)Delegate.Remove(dispatcherListener2.OnGamePacketReceived, new Action<PegasusPacket>(OnGamePacketReceived));
	}
}
