using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Blizzard.GameService.SDK.Client.Integration;
using Hearthstone;
using UnityEngine;

public class BnetNearbyPlayerMgr
{
	public delegate void ChangeCallback(BnetRecentOrNearbyPlayerChangelist changelist, object userData);

	private class ChangeListener : EventListener<ChangeCallback>
	{
		public void Fire(BnetRecentOrNearbyPlayerChangelist changelist)
		{
			m_callback(changelist, m_userData);
		}
	}

	private class NearbyPlayer : IEquatable<NearbyPlayer>
	{
		public float m_lastReceivedTime;

		public BnetPlayer m_bnetPlayer;

		public bool m_availability;

		public ulong m_sessionStartTime;

		public BnetPartyId m_partyId = BnetPartyId.Empty;

		public bool Equals(NearbyPlayer other)
		{
			if (other == null)
			{
				return false;
			}
			return GetGameAccountId() == other.GetGameAccountId();
		}

		public BnetAccountId GetAccountId()
		{
			return m_bnetPlayer.GetAccountId();
		}

		public BnetGameAccountId GetGameAccountId()
		{
			return m_bnetPlayer.GetHearthstoneGameAccountId();
		}

		public BnetGameAccount GetGameAccount()
		{
			return m_bnetPlayer.GetHearthstoneGameAccount();
		}

		public bool IsFriend()
		{
			BnetAccountId accountId = GetAccountId();
			return BnetFriendMgr.Get().IsFriend(accountId);
		}
	}

	private class UdpState
	{
		public UdpClient u;

		public IPEndPoint e;
	}

	private static BnetNearbyPlayerMgr s_instance;

	private bool m_enabled = true;

	private bool m_listening;

	private ulong m_myGameAccountLo;

	private string m_bnetVersion;

	private string m_bnetEnvironment;

	private string m_idString;

	private bool m_availability;

	private bool m_battlegroundsAvailability;

	private bool m_mercenariesAvailability;

	private BnetPartyId m_partyId = BnetPartyId.Empty;

	private UdpClient m_client;

	private int m_port;

	private float m_lastCallTime;

	private List<NearbyPlayer> m_nearbyPlayers = new List<NearbyPlayer>();

	private List<BnetPlayer> m_nearbyBnetPlayers = new List<BnetPlayer>();

	private List<BnetPlayer> m_nearbyFriends = new List<BnetPlayer>();

	private List<BnetPlayer> m_nearbyStrangers = new List<BnetPlayer>();

	private object m_mutex = new object();

	private object m_mutexClient = new object();

	private List<NearbyPlayer> m_nearbyAdds = new List<NearbyPlayer>();

	private List<NearbyPlayer> m_nearbyUpdates = new List<NearbyPlayer>();

	private List<ChangeListener> m_changeListeners = new List<ChangeListener>();

	private byte[] m_broadcastBuffer;

	private StringBuilder m_broadcastStringBuilder = new StringBuilder(128);

	private IPEndPoint m_broadcastEndpoint;

	private UdpClient m_broadcastSender;

	private bool m_isBroadcasting;

	private BnetRecentOrNearbyPlayerChangelist m_changelist = new BnetRecentOrNearbyPlayerChangelist();

	public static BnetNearbyPlayerMgr Get()
	{
		if (s_instance == null)
		{
			s_instance = new BnetNearbyPlayerMgr();
			HearthstoneApplication.Get().WillReset += s_instance.Clear;
		}
		return s_instance;
	}

	public void Initialize()
	{
		m_bnetVersion = BattleNet.GetVersion();
		m_bnetEnvironment = BattleNet.GetEnvironment();
		UpdateEnabled();
		Options.Get().RegisterChangedListener(Option.NEARBY_PLAYERS, OnEnabledOptionChanged);
		BnetFriendMgr.Get().AddChangeListener(OnFriendsChanged);
		Network.Get().OnDisconnectedFromBattleNet += OnDisconnectedFromBattleNet;
	}

	public void Shutdown()
	{
		StopListening();
		Options.Get().UnregisterChangedListener(Option.NEARBY_PLAYERS, OnEnabledOptionChanged);
		BnetFriendMgr.Get().RemoveChangeListener(OnFriendsChanged);
		Network.Get().OnDisconnectedFromBattleNet -= OnDisconnectedFromBattleNet;
	}

	public bool IsEnabled()
	{
		if (TemporaryAccountManager.IsTemporaryAccount())
		{
			return false;
		}
		if (!Options.Get().GetBool(Option.NEARBY_PLAYERS))
		{
			return false;
		}
		if (!m_enabled)
		{
			return false;
		}
		return true;
	}

	public void SetEnabled(bool enabled)
	{
		m_enabled = enabled;
		UpdateEnabled();
	}

	public bool HasNearbyStrangers()
	{
		if (m_nearbyStrangers.Count > 0)
		{
			return m_nearbyStrangers.Any((BnetPlayer p) => p?.IsOnline() ?? false);
		}
		return false;
	}

	public List<BnetPlayer> GetNearbyPlayers()
	{
		return m_nearbyBnetPlayers;
	}

	public bool IsNearbyPlayer(BnetPlayer player)
	{
		return FindNearbyPlayer(player) != null;
	}

	public bool IsNearbyPlayer(BnetGameAccountId id)
	{
		return FindNearbyPlayer(id) != null;
	}

	public bool IsNearbyStranger(BnetPlayer player)
	{
		return FindNearbyStranger(player) != null;
	}

	public bool IsNearbyStranger(BnetGameAccountId id)
	{
		return FindNearbyStranger(id) != null;
	}

	public BnetPlayer FindNearbyPlayer(BnetPlayer player)
	{
		return FindNearbyPlayer(player, m_nearbyBnetPlayers);
	}

	public BnetPlayer FindNearbyPlayer(BnetGameAccountId id)
	{
		return FindNearbyPlayer(id, m_nearbyBnetPlayers);
	}

	public BnetPlayer FindNearbyStranger(BnetPlayer player)
	{
		return FindNearbyPlayer(player, m_nearbyStrangers);
	}

	public BnetPlayer FindNearbyStranger(BnetGameAccountId id)
	{
		return FindNearbyPlayer(id, m_nearbyStrangers);
	}

	public BnetPlayer FindNearbyStranger(BnetAccountId id)
	{
		return FindNearbyPlayer(id, m_nearbyStrangers);
	}

	public void SetAvailability(bool av)
	{
		m_availability = av;
		CreateBroadcastString();
	}

	public void SetBattlegroundsAvailability(bool av)
	{
		m_battlegroundsAvailability = av;
		CreateBroadcastString();
	}

	public void SetMercenariesAvailability(bool av)
	{
		m_mercenariesAvailability = av;
		CreateBroadcastString();
	}

	public void SetPartyId(BnetPartyId partyId)
	{
		m_partyId = partyId ?? BnetPartyId.Empty;
		CreateBroadcastString();
	}

	public bool AddChangeListener(ChangeCallback callback)
	{
		return AddChangeListener(callback, null);
	}

	public bool AddChangeListener(ChangeCallback callback, object userData)
	{
		ChangeListener listener = new ChangeListener();
		listener.SetCallback(callback);
		listener.SetUserData(userData);
		if (m_changeListeners.Contains(listener))
		{
			return false;
		}
		m_changeListeners.Add(listener);
		return true;
	}

	public bool RemoveChangeListener(ChangeCallback callback)
	{
		return RemoveChangeListener(callback, null);
	}

	private bool RemoveChangeListener(ChangeCallback callback, object userData)
	{
		ChangeListener listener = new ChangeListener();
		listener.SetCallback(callback);
		listener.SetUserData(userData);
		return m_changeListeners.Remove(listener);
	}

	public static bool RemoveChangeListenerFromInstance(ChangeCallback callback, object userData = null)
	{
		if (s_instance == null)
		{
			return false;
		}
		return s_instance.RemoveChangeListener(callback, userData);
	}

	private void BeginListening()
	{
		if (!m_listening)
		{
			m_listening = true;
			IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, 1228);
			UdpClient client = new UdpClient();
			client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, optionValue: true);
			client.Client.Bind(endPoint);
			m_port = 1228;
			m_client = client;
			m_broadcastSender = new UdpClient();
			m_broadcastEndpoint = new IPEndPoint(IPAddress.Broadcast, m_port);
			UdpState state = new UdpState();
			state.e = endPoint;
			state.u = m_client;
			m_lastCallTime = Time.realtimeSinceStartup;
			m_client.BeginReceive(OnUdpReceive, state);
		}
	}

	private void OnUdpReceive(IAsyncResult ar)
	{
		lock (m_mutexClient)
		{
			if (!m_listening)
			{
				return;
			}
		}
		UdpClient client = ((UdpState)ar.AsyncState).u;
		IPEndPoint endPoint = ((UdpState)ar.AsyncState).e;
		if (client == null || client.Client == null || !client.Client.Connected || endPoint == null)
		{
			return;
		}
		byte[] responseBytes = null;
		try
		{
			responseBytes = client.EndReceive(ar, ref endPoint);
			client.BeginReceive(OnUdpReceive, ar.AsyncState);
		}
		catch
		{
			return;
		}
		if (responseBytes == null || responseBytes.Length == 0)
		{
			return;
		}
		string[] messageParts = Encoding.UTF8.GetString(responseBytes).Split(',');
		ulong aHi = 0uL;
		ulong aLo = 0uL;
		ulong gaHi = 0uL;
		ulong gaLo = 0uL;
		bool available = false;
		ulong sessionStartTime = 0uL;
		ulong partyIdHi = 0uL;
		ulong partyIdLo = 0uL;
		bool battlegroundsAvailable = false;
		bool mercenariesAvailable = false;
		bool traditionalTutorialComplete = false;
		int index = 0;
		if (index >= messageParts.Length || !ulong.TryParse(messageParts[index++], out aHi) || index >= messageParts.Length || !ulong.TryParse(messageParts[index++], out aLo) || index >= messageParts.Length || !ulong.TryParse(messageParts[index++], out gaHi) || index >= messageParts.Length || !ulong.TryParse(messageParts[index++], out gaLo) || m_myGameAccountLo == gaLo || index >= messageParts.Length)
		{
			return;
		}
		string btName = messageParts[index++];
		if (index >= messageParts.Length)
		{
			return;
		}
		string btNumber = messageParts[index++];
		if (index >= messageParts.Length)
		{
			return;
		}
		string version = messageParts[index++];
		if (string.IsNullOrEmpty(version) || version != m_bnetVersion || index >= messageParts.Length)
		{
			return;
		}
		string environment = messageParts[index++];
		if (string.IsNullOrEmpty(environment) || environment != m_bnetEnvironment || index >= messageParts.Length)
		{
			return;
		}
		string avStr = messageParts[index++];
		if (avStr == "1")
		{
			available = true;
		}
		else
		{
			if (!(avStr == "0"))
			{
				return;
			}
			available = false;
		}
		if (index >= messageParts.Length || !ulong.TryParse(messageParts[index++], out sessionStartTime) || index >= messageParts.Length || !ulong.TryParse(messageParts[index++], out partyIdHi) || index >= messageParts.Length || !ulong.TryParse(messageParts[index++], out partyIdLo) || index >= messageParts.Length)
		{
			return;
		}
		string bgAvStr = messageParts[index++];
		if (bgAvStr == "1")
		{
			battlegroundsAvailable = true;
		}
		else
		{
			if (!(bgAvStr == "0"))
			{
				return;
			}
			battlegroundsAvailable = false;
		}
		if (index >= messageParts.Length)
		{
			return;
		}
		string mercAvStr = messageParts[index++];
		if (mercAvStr == "1")
		{
			mercenariesAvailable = true;
		}
		else
		{
			if (!(mercAvStr == "0"))
			{
				return;
			}
			mercenariesAvailable = false;
		}
		if (index >= messageParts.Length)
		{
			return;
		}
		string traditionalTutorialCompleteStr = messageParts[index++];
		if (traditionalTutorialCompleteStr == "1")
		{
			traditionalTutorialComplete = true;
		}
		else
		{
			if (!(traditionalTutorialCompleteStr == "0"))
			{
				return;
			}
			traditionalTutorialComplete = false;
		}
		BnetBattleTag battleTag = new BnetBattleTag();
		battleTag.SetName(btName);
		battleTag.SetNumber(btNumber);
		BnetAccountId accountId = new BnetAccountId(aHi, aLo);
		BnetGameAccountId gameAccountId = new BnetGameAccountId(gaHi, gaLo);
		BnetPartyId partyId = new BnetPartyId(partyIdHi, partyIdLo);
		BnetPlayer bnetPlayer = BnetPresenceMgr.Get().GetPlayer(gameAccountId);
		if (bnetPlayer == null)
		{
			BnetAccount account = new BnetAccount();
			account.SetId(accountId);
			account.SetBattleTag(battleTag);
			account.SetAppearingOffline(appearingOffline: false);
			BnetGameAccount gameAccount = new BnetGameAccount();
			gameAccount.SetId(gameAccountId);
			gameAccount.SetOwnerId(accountId);
			gameAccount.SetBattleTag(battleTag);
			gameAccount.SetOnline(online: true);
			gameAccount.SetProgramId(BnetProgramId.HEARTHSTONE);
			gameAccount.SetGameField(1u, available);
			gameAccount.SetGameField(19u, version);
			gameAccount.SetGameField(20u, environment);
			gameAccount.SetGameField(26u, partyId.ToBnetEntityId());
			gameAccount.SetGameField(28u, battlegroundsAvailable ? 1 : 0);
			gameAccount.SetGameField(29u, mercenariesAvailable ? 1 : 0);
			gameAccount.SetGameField(15u, traditionalTutorialComplete ? 1 : 0);
			bnetPlayer = new BnetPlayer(BnetPlayerSource.NEARBY_PLAYER);
			bnetPlayer.SetAccount(account);
			bnetPlayer.AddGameAccount(gameAccount);
		}
		NearbyPlayer nearbyPlayer = new NearbyPlayer();
		nearbyPlayer.m_bnetPlayer = bnetPlayer;
		nearbyPlayer.m_availability = available;
		nearbyPlayer.m_partyId = partyId;
		nearbyPlayer.m_sessionStartTime = sessionStartTime;
		lock (m_mutex)
		{
			if (!m_listening)
			{
				return;
			}
			foreach (NearbyPlayer currNearbyPlayer in m_nearbyAdds)
			{
				if (currNearbyPlayer.Equals(nearbyPlayer))
				{
					UpdateNearbyPlayer(currNearbyPlayer, available, battlegroundsAvailable, mercenariesAvailable, traditionalTutorialComplete, sessionStartTime, partyId);
					return;
				}
			}
			foreach (NearbyPlayer currNearbyPlayer2 in m_nearbyUpdates)
			{
				if (currNearbyPlayer2.Equals(nearbyPlayer))
				{
					UpdateNearbyPlayer(currNearbyPlayer2, available, battlegroundsAvailable, mercenariesAvailable, traditionalTutorialComplete, sessionStartTime, partyId);
					return;
				}
			}
			foreach (NearbyPlayer currNearbyPlayer3 in m_nearbyPlayers)
			{
				if (currNearbyPlayer3.Equals(nearbyPlayer))
				{
					UpdateNearbyPlayer(currNearbyPlayer3, available, battlegroundsAvailable, mercenariesAvailable, traditionalTutorialComplete, sessionStartTime, partyId);
					m_nearbyUpdates.Add(currNearbyPlayer3);
					return;
				}
			}
			m_nearbyAdds.Add(nearbyPlayer);
		}
	}

	private void StopListening()
	{
		lock (m_mutexClient)
		{
			if (!m_listening)
			{
				return;
			}
			m_listening = false;
			m_client.Close();
		}
		BnetRecentOrNearbyPlayerChangelist changelist = new BnetRecentOrNearbyPlayerChangelist();
		lock (m_mutex)
		{
			foreach (BnetPlayer player in m_nearbyBnetPlayers)
			{
				changelist.AddRemovedPlayer(player);
			}
			foreach (BnetPlayer friend in m_nearbyFriends)
			{
				changelist.AddRemovedFriend(friend);
			}
			foreach (BnetPlayer stranger in m_nearbyStrangers)
			{
				changelist.AddRemovedStranger(stranger);
			}
			m_nearbyPlayers.Clear();
			m_nearbyBnetPlayers.Clear();
			m_nearbyFriends.Clear();
			m_nearbyStrangers.Clear();
			m_nearbyAdds.Clear();
			m_nearbyUpdates.Clear();
		}
		FireChangeEvent(changelist);
		m_broadcastSender.Close();
	}

	public void Update()
	{
		if (m_listening)
		{
			CacheMyAccountInfo();
			CheckIntervalAndBroadcast();
			ProcessPlayerChanges();
		}
	}

	private void Clear()
	{
		lock (m_mutex)
		{
			m_nearbyPlayers.Clear();
			m_nearbyBnetPlayers.Clear();
			m_nearbyFriends.Clear();
			m_nearbyStrangers.Clear();
			m_nearbyAdds.Clear();
			m_nearbyUpdates.Clear();
		}
	}

	private void OnDisconnectedFromBattleNet(BattleNetErrors error)
	{
		Clear();
	}

	private void UpdateEnabled()
	{
		bool enabled = IsEnabled();
		if (enabled != m_listening)
		{
			if (enabled)
			{
				BeginListening();
			}
			else
			{
				StopListening();
			}
		}
	}

	private void FireChangeEvent(BnetRecentOrNearbyPlayerChangelist changelist)
	{
		if (!changelist.IsEmpty())
		{
			ChangeListener[] listeners = m_changeListeners.ToArray();
			for (int i = 0; i < listeners.Length; i++)
			{
				listeners[i].Fire(changelist);
			}
		}
	}

	private void CacheMyAccountInfo()
	{
		if (m_idString != null)
		{
			return;
		}
		BnetGameAccountId myGameAccountId = BnetPresenceMgr.Get().GetMyGameAccountId();
		if (myGameAccountId == null)
		{
			return;
		}
		BnetPlayer myself = BnetPresenceMgr.Get().GetMyPlayer();
		if (myself == null)
		{
			return;
		}
		BnetAccountId myAccountId = myself.GetAccountId();
		if (!(myAccountId == null))
		{
			BnetBattleTag myBattleTag = myself.GetBattleTag();
			if (!(myBattleTag == null))
			{
				m_myGameAccountLo = myGameAccountId.Low;
				StringBuilder builder = new StringBuilder();
				builder.Append(myAccountId.High);
				builder.Append(',');
				builder.Append(myAccountId.Low);
				builder.Append(',');
				builder.Append(myGameAccountId.High);
				builder.Append(',');
				builder.Append(myGameAccountId.Low);
				builder.Append(',');
				builder.Append(myBattleTag.GetName());
				builder.Append(',');
				builder.Append(myBattleTag.GetNumber());
				builder.Append(',');
				builder.Append(BattleNet.GetVersion());
				builder.Append(',');
				builder.Append(BattleNet.GetEnvironment());
				m_idString = builder.ToString();
				CreateBroadcastString();
			}
		}
	}

	private void ProcessPlayerChanges()
	{
		m_changelist.Clear();
		lock (m_mutex)
		{
			ProcessAddedPlayers(m_changelist);
			ProcessUpdatedPlayers(m_changelist);
			RemoveInactivePlayers(m_changelist);
		}
		FireChangeEvent(m_changelist);
	}

	private void ProcessAddedPlayers(BnetRecentOrNearbyPlayerChangelist changelist)
	{
		if (m_nearbyAdds.Count == 0)
		{
			return;
		}
		for (int i = 0; i < m_nearbyAdds.Count; i++)
		{
			NearbyPlayer player = m_nearbyAdds[i];
			player.m_lastReceivedTime = Time.realtimeSinceStartup;
			BnetGameAccountId gameAccountId = player.GetGameAccount().GetId();
			if (BnetPresenceMgr.Get().GetPlayer(gameAccountId) == null)
			{
				BnetPresenceMgr.Get().RegisterBnetPlayer(player.m_bnetPlayer);
			}
			m_nearbyPlayers.Add(player);
			m_nearbyBnetPlayers.Add(player.m_bnetPlayer);
			changelist.AddAddedPlayer(player.m_bnetPlayer);
			if (player.IsFriend())
			{
				m_nearbyFriends.Add(player.m_bnetPlayer);
				changelist.AddAddedFriend(player.m_bnetPlayer);
			}
			else
			{
				m_nearbyStrangers.Add(player.m_bnetPlayer);
				changelist.AddAddedStranger(player.m_bnetPlayer);
			}
		}
		m_nearbyAdds.Clear();
	}

	private void ProcessUpdatedPlayers(BnetRecentOrNearbyPlayerChangelist changelist)
	{
		if (m_nearbyUpdates.Count == 0)
		{
			return;
		}
		for (int i = 0; i < m_nearbyUpdates.Count; i++)
		{
			NearbyPlayer player = m_nearbyUpdates[i];
			player.m_lastReceivedTime = Time.realtimeSinceStartup;
			changelist.AddUpdatedPlayer(player.m_bnetPlayer);
			if (player.IsFriend())
			{
				changelist.AddUpdatedFriend(player.m_bnetPlayer);
			}
			else
			{
				changelist.AddUpdatedStranger(player.m_bnetPlayer);
			}
		}
		m_nearbyUpdates.Clear();
	}

	private void RemoveInactivePlayers(BnetRecentOrNearbyPlayerChangelist changelist)
	{
		List<NearbyPlayer> inactivePlayers = null;
		for (int i = 0; i < m_nearbyPlayers.Count; i++)
		{
			NearbyPlayer player = m_nearbyPlayers[i];
			float sinceLastRcv = Time.realtimeSinceStartup - player.m_lastReceivedTime;
			if (!player.m_bnetPlayer.IsCheatPlayer && sinceLastRcv >= 60f)
			{
				if (inactivePlayers == null)
				{
					inactivePlayers = new List<NearbyPlayer>();
				}
				inactivePlayers.Add(player);
			}
		}
		if (inactivePlayers == null)
		{
			return;
		}
		foreach (NearbyPlayer player2 in inactivePlayers)
		{
			m_nearbyPlayers.Remove(player2);
			if (m_nearbyBnetPlayers.Remove(player2.m_bnetPlayer))
			{
				changelist.AddRemovedPlayer(player2.m_bnetPlayer);
			}
			if (m_nearbyFriends.Remove(player2.m_bnetPlayer))
			{
				changelist.AddRemovedFriend(player2.m_bnetPlayer);
			}
			if (m_nearbyStrangers.Remove(player2.m_bnetPlayer))
			{
				changelist.AddRemovedStranger(player2.m_bnetPlayer);
			}
		}
	}

	private bool CheckIntervalAndBroadcast()
	{
		if (!IsMyPlayerOnline())
		{
			return false;
		}
		if (Time.realtimeSinceStartup - m_lastCallTime < 12f)
		{
			return false;
		}
		m_lastCallTime = Time.realtimeSinceStartup;
		Broadcast();
		return true;
	}

	private async void Broadcast()
	{
		if (m_isBroadcasting)
		{
			return;
		}
		m_isBroadcasting = true;
		try
		{
			m_broadcastSender.EnableBroadcast = true;
			await m_broadcastSender.SendAsync(m_broadcastBuffer, m_broadcastBuffer.Length, m_broadcastEndpoint);
		}
		catch
		{
		}
		finally
		{
			m_isBroadcasting = false;
		}
	}

	private void CreateBroadcastString()
	{
		ulong sessionStartTime = HealthyGamingMgr.Get().GetSessionStartTime();
		BnetEntityId bnetPartyEntityId = m_partyId.ToBnetEntityId();
		m_broadcastStringBuilder.Clear();
		m_broadcastStringBuilder.Append(m_idString);
		m_broadcastStringBuilder.Append(',');
		m_broadcastStringBuilder.Append(m_availability ? "1" : "0");
		m_broadcastStringBuilder.Append(',');
		m_broadcastStringBuilder.Append(sessionStartTime);
		m_broadcastStringBuilder.Append(',');
		m_broadcastStringBuilder.Append(bnetPartyEntityId.High);
		m_broadcastStringBuilder.Append(',');
		m_broadcastStringBuilder.Append(bnetPartyEntityId.Low);
		m_broadcastStringBuilder.Append(',');
		m_broadcastStringBuilder.Append(m_battlegroundsAvailability ? "1" : "0");
		m_broadcastStringBuilder.Append(',');
		m_broadcastStringBuilder.Append(m_mercenariesAvailability ? "1" : "0");
		m_broadcastStringBuilder.Append(',');
		m_broadcastStringBuilder.Append(GameUtils.IsTraditionalTutorialComplete() ? "1" : "0");
		string broadcastString = m_broadcastStringBuilder.ToString();
		m_broadcastBuffer = Encoding.UTF8.GetBytes(broadcastString);
	}

	private int FindNearbyPlayerIndex(BnetPlayer bnetPlayer, List<BnetPlayer> bnetPlayers)
	{
		if (bnetPlayer == null)
		{
			return -1;
		}
		BnetAccountId accountId = bnetPlayer.GetAccountId();
		if (accountId != null)
		{
			return FindNearbyPlayerIndex(accountId, bnetPlayers);
		}
		BnetGameAccountId gameAccountId = bnetPlayer.GetHearthstoneGameAccountId();
		return FindNearbyPlayerIndex(gameAccountId, bnetPlayers);
	}

	private int FindNearbyPlayerIndex(BnetGameAccountId id, List<BnetPlayer> bnetPlayers)
	{
		if (id == null)
		{
			return -1;
		}
		for (int i = 0; i < bnetPlayers.Count; i++)
		{
			BnetPlayer bnetPlayer = bnetPlayers[i];
			if (id == bnetPlayer.GetHearthstoneGameAccountId())
			{
				return i;
			}
		}
		return -1;
	}

	private int FindNearbyPlayerIndex(BnetAccountId id, List<BnetPlayer> bnetPlayers)
	{
		if (id == null)
		{
			return -1;
		}
		for (int i = 0; i < bnetPlayers.Count; i++)
		{
			BnetPlayer bnetPlayer = bnetPlayers[i];
			if (id == bnetPlayer.GetAccountId())
			{
				return i;
			}
		}
		return -1;
	}

	private BnetPlayer FindNearbyPlayer(BnetPlayer bnetPlayer, List<BnetPlayer> bnetPlayers)
	{
		if (bnetPlayer == null)
		{
			return null;
		}
		BnetAccountId accountId = bnetPlayer.GetAccountId();
		if (accountId != null)
		{
			return FindNearbyPlayer(accountId, bnetPlayers);
		}
		BnetGameAccountId gameAccountId = bnetPlayer.GetHearthstoneGameAccountId();
		return FindNearbyPlayer(gameAccountId, bnetPlayers);
	}

	private BnetPlayer FindNearbyPlayer(BnetGameAccountId id, List<BnetPlayer> bnetPlayers)
	{
		int index = FindNearbyPlayerIndex(id, bnetPlayers);
		if (index < 0)
		{
			return null;
		}
		return bnetPlayers[index];
	}

	private BnetPlayer FindNearbyPlayer(BnetAccountId id, List<BnetPlayer> bnetPlayers)
	{
		int index = FindNearbyPlayerIndex(id, bnetPlayers);
		if (index < 0)
		{
			return null;
		}
		return bnetPlayers[index];
	}

	private void UpdateNearbyPlayer(NearbyPlayer player, bool available, bool battlegroundsAvailable, bool mercenariesAvailable, bool traditionalTutorialComplete, ulong sessionStartTime, BnetPartyId partyId)
	{
		BnetGameAccount gameAccount = player.GetGameAccount();
		bool num = BnetPresenceMgr.Get().IsSubscribedToPlayer(gameAccount.GetId());
		BnetPlayer playerTrackedByPresence = BnetPresenceMgr.Get().GetPlayer(gameAccount.GetId());
		if (num && playerTrackedByPresence != null)
		{
			player.m_bnetPlayer = playerTrackedByPresence;
		}
		else
		{
			gameAccount.SetGameField(1u, available);
			gameAccount.SetGameField(28u, battlegroundsAvailable ? 1 : 0);
			gameAccount.SetGameField(29u, mercenariesAvailable ? 1 : 0);
			gameAccount.SetGameField(15u, traditionalTutorialComplete ? 1 : 0);
			gameAccount.SetGameField(26u, partyId.ToBnetEntityId());
		}
		player.m_sessionStartTime = sessionStartTime;
	}

	private bool IsMyPlayerOnline()
	{
		BnetPlayer myself = BnetPresenceMgr.Get().GetMyPlayer();
		if (myself == null)
		{
			return false;
		}
		if (myself.IsOnline())
		{
			return !myself.IsAppearingOffline();
		}
		return false;
	}

	private void OnEnabledOptionChanged(Option option, object prevValue, bool existed, object userData)
	{
		UpdateEnabled();
	}

	private void OnFriendsChanged(BnetFriendChangelist friendChangelist, object userData)
	{
		List<BnetPlayer> addedFriends = friendChangelist.GetAddedFriends();
		List<BnetPlayer> removedFriends = friendChangelist.GetRemovedFriends();
		bool num = addedFriends != null && addedFriends.Count > 0;
		bool haveRemovals = removedFriends != null && removedFriends.Count > 0;
		if (!num && !haveRemovals)
		{
			return;
		}
		BnetRecentOrNearbyPlayerChangelist changelist = new BnetRecentOrNearbyPlayerChangelist();
		lock (m_mutex)
		{
			if (addedFriends != null)
			{
				foreach (BnetPlayer currFriend in addedFriends)
				{
					int index = FindNearbyPlayerIndex(currFriend, m_nearbyStrangers);
					if (index >= 0)
					{
						BnetPlayer stranger = m_nearbyStrangers[index];
						m_nearbyStrangers.RemoveAt(index);
						m_nearbyFriends.Add(stranger);
						changelist.AddAddedFriend(stranger);
						changelist.AddRemovedStranger(stranger);
					}
				}
			}
			if (removedFriends != null)
			{
				foreach (BnetPlayer currFriend2 in removedFriends)
				{
					int index2 = FindNearbyPlayerIndex(currFriend2, m_nearbyFriends);
					if (index2 >= 0)
					{
						BnetPlayer friend = m_nearbyFriends[index2];
						m_nearbyFriends.RemoveAt(index2);
						m_nearbyStrangers.Add(friend);
						changelist.AddAddedStranger(friend);
						changelist.AddRemovedFriend(friend);
					}
				}
			}
		}
		FireChangeEvent(changelist);
	}

	public BnetPlayer Cheat_CreateNearbyPlayer(string fullName, int leagueId, int starLevel, BnetProgramId programId, bool isFriend, bool isOnline)
	{
		BnetPlayer player = BnetFriendMgr.Get().Cheat_CreatePlayer(fullName, leagueId, starLevel, programId, isFriend, isOnline);
		BnetRecentOrNearbyPlayerChangelist changeList = new BnetRecentOrNearbyPlayerChangelist();
		changeList.AddAddedPlayer(player);
		if (isFriend)
		{
			changeList.AddAddedFriend(player);
		}
		else
		{
			changeList.AddAddedStranger(player);
		}
		NearbyPlayer nearbyPlayer = new NearbyPlayer();
		nearbyPlayer.m_bnetPlayer = player;
		nearbyPlayer.m_availability = true;
		nearbyPlayer.m_partyId = BnetPartyId.Empty;
		m_nearbyAdds.Add(nearbyPlayer);
		ProcessAddedPlayers(changeList);
		return player;
	}

	public int Cheat_RemoveCheatFriends()
	{
		int numRemoved = 0;
		BnetRecentOrNearbyPlayerChangelist changeList = new BnetRecentOrNearbyPlayerChangelist();
		for (int i = m_nearbyPlayers.Count - 1; i >= 0; i--)
		{
			NearbyPlayer player = m_nearbyPlayers[i];
			if (player.m_bnetPlayer.IsCheatPlayer)
			{
				player.m_lastReceivedTime = 0f;
				numRemoved++;
			}
		}
		RemoveInactivePlayers(changeList);
		FireChangeEvent(changeList);
		return numRemoved;
	}
}
