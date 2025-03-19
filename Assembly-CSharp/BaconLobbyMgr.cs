using System;
using System.Collections.Generic;
using Blizzard.GameService.Protocol.V2.Client;
using Blizzard.GameService.SDK.Client.Integration;
using Blizzard.T5.Jobs;
using Blizzard.T5.Services;
using Hearthstone;
using PegasusShared;

public class BaconLobbyMgr : IService
{
	private class BattlegroundsGameModeChangedListener : EventListener<BattlegroundsGameModeChangedCallback>
	{
		public void Fire(string gameMode, bool showPartyUI)
		{
			m_callback(gameMode, showPartyUI, m_userData);
		}
	}

	public delegate void BattlegroundsGameModeChangedCallback(string gameMode, bool shouldShowPartyUI, object userData);

	private string m_selectedBattlegroundsGameMode = "solo";

	private bool m_showDuosPartyUI;

	private List<BattlegroundsGameModeChangedListener> m_battlegroundsGameModeChangedListeners = new List<BattlegroundsGameModeChangedListener>();

	public IEnumerator<IAsyncJobResult> Initialize(ServiceLocator serviceLocator)
	{
		PartyManager.Get().AddChangedListener(OnPartyChanged);
		PartyManager.Get().AddPartyAttributeChangedListener(OnPartyAttributeChanged);
		NetCache.Get().RegisterUpdatedListener(typeof(NetCache.NetCacheClientOptions), NetCache_OnNetCacheClientOptions);
		HearthstoneApplication.Get().WillReset += WillReset;
		yield break;
	}

	public Type[] GetDependencies()
	{
		return new Type[2]
		{
			typeof(PartyManager),
			typeof(NetCache)
		};
	}

	private void WillReset()
	{
		m_showDuosPartyUI = false;
	}

	public void Shutdown()
	{
		PartyManager.Get().RemoveChangedListener(OnPartyChanged);
		PartyManager.Get().RemovePartyAttributeChangedListener(OnPartyAttributeChanged);
		NetCache.Get().RemoveUpdatedListener(typeof(NetCache.NetCacheClientOptions), NetCache_OnNetCacheClientOptions);
		HearthstoneApplication.Get().WillReset -= WillReset;
	}

	public static BaconLobbyMgr Get()
	{
		return ServiceManager.Get<BaconLobbyMgr>();
	}

	private void OnPartyChanged(PartyManager.PartyInviteEvent inviteEvent, BnetGameAccountId playerGameAccountId, PartyManager.PartyData data, object userData)
	{
		bool gameModeChanged = PartyManager.Get().IsInBattlegroundsParty() && m_selectedBattlegroundsGameMode != PartyManager.Get().GetSelectedBattlegroundsGameMode();
		m_showDuosPartyUI = PartyManager.Get().IsInBattlegroundsParty();
		if (m_showDuosPartyUI && gameModeChanged)
		{
			SetBattlegroundsGameMode(PartyManager.Get().GetSelectedBattlegroundsGameMode());
		}
		else if (gameModeChanged)
		{
			FireBattlegroundsGameModeChangedEvent(m_selectedBattlegroundsGameMode, m_showDuosPartyUI);
		}
	}

	private void OnPartyAttributeChanged(Blizzard.GameService.Protocol.V2.Client.Attribute attribute, object userData)
	{
		if (PartyManager.Get().IsInBattlegroundsParty() && m_selectedBattlegroundsGameMode != PartyManager.Get().GetSelectedBattlegroundsGameMode())
		{
			SetBattlegroundsGameMode(PartyManager.Get().GetSelectedBattlegroundsGameMode());
		}
	}

	private void NetCache_OnNetCacheClientOptions()
	{
		if (Options.Get().HasOption(Option.BATTLEGROUNDS_PLAYING_DUOS_MODE) && Options.Get().GetBool(Option.BATTLEGROUNDS_PLAYING_DUOS_MODE))
		{
			m_selectedBattlegroundsGameMode = "duos";
		}
	}

	public void SetBattlegroundsGameMode(string gameMode)
	{
		m_selectedBattlegroundsGameMode = gameMode;
		Options.Get().SetBool(Option.BATTLEGROUNDS_PLAYING_DUOS_MODE, m_selectedBattlegroundsGameMode == "duos");
		FireBattlegroundsGameModeChangedEvent(m_selectedBattlegroundsGameMode, m_showDuosPartyUI);
	}

	public string GetBattlegroundsGameMode()
	{
		return m_selectedBattlegroundsGameMode;
	}

	public bool IsInDuosMode()
	{
		return GetBattlegroundsGameMode() == "duos";
	}

	public GameType GetBattlegroundsGameModeType()
	{
		if (m_selectedBattlegroundsGameMode == "duos")
		{
			return GameType.GT_BATTLEGROUNDS_DUO;
		}
		return GameType.GT_BATTLEGROUNDS;
	}

	public int GetBattlegroundsActiveGameModeRating()
	{
		NetCache.NetCacheBaconRatingInfo ratingInfo = NetCache.Get().GetNetObject<NetCache.NetCacheBaconRatingInfo>();
		if (ratingInfo == null)
		{
			return 0;
		}
		if (m_selectedBattlegroundsGameMode == "duos")
		{
			return ratingInfo.DuosRating;
		}
		return ratingInfo.Rating;
	}

	private void FireBattlegroundsGameModeChangedEvent(string gameMode, bool showPartyUI)
	{
		BattlegroundsGameModeChangedListener[] listeners = m_battlegroundsGameModeChangedListeners.ToArray();
		for (int i = 0; i < listeners.Length; i++)
		{
			listeners[i].Fire(gameMode, showPartyUI);
		}
	}

	public bool AddBattlegroundsGameModeChangedListener(BattlegroundsGameModeChangedCallback callback)
	{
		return AddBattlegroundsGameModeChangedListener(callback, null);
	}

	public bool AddBattlegroundsGameModeChangedListener(BattlegroundsGameModeChangedCallback callback, object userData)
	{
		BattlegroundsGameModeChangedListener listener = new BattlegroundsGameModeChangedListener();
		listener.SetCallback(callback);
		listener.SetUserData(userData);
		if (m_battlegroundsGameModeChangedListeners.Contains(listener))
		{
			return false;
		}
		m_battlegroundsGameModeChangedListeners.Add(listener);
		return true;
	}

	public bool RemoveBattlegroundsGameModeChangedListener(BattlegroundsGameModeChangedCallback callback)
	{
		return RemoveBattlegroundsGameModeChangedListener(callback, null);
	}

	public bool RemoveBattlegroundsGameModeChangedListener(BattlegroundsGameModeChangedCallback callback, object userData)
	{
		BattlegroundsGameModeChangedListener listener = new BattlegroundsGameModeChangedListener();
		listener.SetCallback(callback);
		listener.SetUserData(userData);
		return m_battlegroundsGameModeChangedListeners.Remove(listener);
	}
}
