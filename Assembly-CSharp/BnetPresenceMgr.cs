using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Blizzard.GameService.SDK.Client.Integration;
using Blizzard.T5.Core;
using Blizzard.T5.Core.Utils;
using Blizzard.T5.Logging;
using Hearthstone;
using SpectatorProto;

public class BnetPresenceMgr
{
	public delegate void PlayersChangedCallback(BnetPlayerChangelist changelist, object userData);

	private class PlayersChangedListener : EventListener<PlayersChangedCallback>
	{
		public void Fire(BnetPlayerChangelist changelist)
		{
			m_callback(changelist, m_userData);
		}
	}

	private static BnetPresenceMgr s_instance;

	private Map<BnetAccountId, BnetAccount> m_accounts = new Map<BnetAccountId, BnetAccount>();

	private Map<BnetGameAccountId, BnetGameAccount> m_gameAccounts = new Map<BnetGameAccountId, BnetGameAccount>();

	private Map<BnetAccountId, BnetPlayer> m_players = new Map<BnetAccountId, BnetPlayer>();

	private BnetAccountId m_myBattleNetAccountId;

	private BnetGameAccountId m_myGameAccountId;

	private BnetPlayer m_myPlayer;

	private List<PlayersChangedListener> m_playersChangedListeners = new List<PlayersChangedListener>();

	public event Action<PresenceUpdate[]> OnGameAccountPresenceChange;

	public static BnetPresenceMgr Get()
	{
		if (s_instance == null)
		{
			s_instance = new BnetPresenceMgr();
			HearthstoneApplication hearthstoneApplication = HearthstoneApplication.Get();
			if (hearthstoneApplication != null)
			{
				hearthstoneApplication.WillReset += delegate
				{
					BnetPresenceMgr bnetPresenceMgr = s_instance;
					s_instance = new BnetPresenceMgr();
					s_instance.m_playersChangedListeners = bnetPresenceMgr.m_playersChangedListeners;
					s_instance.OnGameAccountPresenceChange = bnetPresenceMgr.OnGameAccountPresenceChange;
				};
			}
			else
			{
				Log.BattleNet.PrintWarning("BnetPresenceMgr.Get(): HearthstoneApplication.Get() returned null. Unable to subscribe to HearthstoneApplication.WillReset.");
			}
		}
		return s_instance;
	}

	public void Initialize()
	{
		Network.Get().SetPresenceHandler(OnPresenceUpdate);
		Network.Get().OnDisconnectedFromBattleNet += OnDisconnectedFromBattleNet;
		m_myGameAccountId = BattleNet.GetMyGameAccountId();
		m_myBattleNetAccountId = BattleNet.GetMyAccoundId();
	}

	public BnetAccountId GetMyAccountId()
	{
		return m_myBattleNetAccountId;
	}

	public BnetGameAccountId GetMyGameAccountId()
	{
		return m_myGameAccountId;
	}

	public BnetPlayer GetMyPlayer()
	{
		return m_myPlayer;
	}

	public BnetAccount GetAccount(BnetAccountId id)
	{
		if (id == null)
		{
			return null;
		}
		BnetAccount account = null;
		m_accounts.TryGetValue(id, out account);
		return account;
	}

	public BnetGameAccount GetGameAccount(BnetGameAccountId id)
	{
		if (id == null)
		{
			return null;
		}
		BnetGameAccount gameAccount = null;
		m_gameAccounts.TryGetValue(id, out gameAccount);
		return gameAccount;
	}

	public BnetPlayer GetPlayer(BnetAccountId id)
	{
		if (id == null)
		{
			return null;
		}
		BnetPlayer player = null;
		m_players.TryGetValue(id, out player);
		return player;
	}

	public BnetPlayer GetPlayer(BnetGameAccountId id)
	{
		BnetGameAccount gameAccount = GetGameAccount(id);
		if (gameAccount == null)
		{
			return null;
		}
		return GetPlayer(gameAccount.GetOwnerId());
	}

	public BnetPlayer RegisterPlayer(BnetPlayerSource source, BnetAccountId accountId, BnetGameAccountId gameAccountId = null, BnetProgramId programId = null)
	{
		BnetPlayer player = GetPlayer(accountId);
		if (player != null)
		{
			return player;
		}
		player = new BnetPlayer(source);
		player.SetAccountId(accountId);
		m_players[accountId] = player;
		BnetAccount account = new BnetAccount();
		m_accounts.Add(accountId, account);
		account.SetId(accountId);
		player.SetAccount(account);
		if (gameAccountId != null)
		{
			if (!m_gameAccounts.TryGetValue(gameAccountId, out var gameAccount))
			{
				gameAccount = new BnetGameAccount();
				gameAccount.SetId(gameAccountId);
				gameAccount.SetOwnerId(accountId);
				m_gameAccounts.Add(gameAccountId, gameAccount);
				if (programId != null)
				{
					gameAccount.SetProgramId(programId);
				}
			}
			player.AddGameAccount(gameAccount);
		}
		BnetPlayerChange change = new BnetPlayerChange();
		change.SetNewPlayer(player);
		BnetPlayerChangelist changelist = new BnetPlayerChangelist();
		changelist.AddChange(change);
		FirePlayersChangedEvent(changelist);
		return player;
	}

	public void RegisterBnetPlayer(BnetPlayer player)
	{
		if (player == null || player.GetAccount() == null || player.GetAccountId() == null)
		{
			return;
		}
		bool isNew = false;
		BnetAccountId accountId = player.GetAccountId();
		if (m_players.TryGetValue(accountId, out var existingPlayer))
		{
			if (existingPlayer != player)
			{
				isNew = true;
				Log.All.PrintWarning("Already registered BnetPlayer accountId={0} newSrc={1} - will overwrite.", accountId.Low, player.Source);
			}
		}
		else
		{
			isNew = true;
		}
		m_players[accountId] = player;
		if (m_accounts.TryGetValue(accountId, out var existingAccount))
		{
			if ((object)existingAccount != player.GetAccount())
			{
				isNew = true;
				Log.All.PrintWarning("Already registered BnetAccount accountId={0} newSrc={1} - will overwrite.", accountId.Low, player.Source);
			}
		}
		else
		{
			isNew = true;
		}
		m_accounts[accountId] = player.GetAccount();
		foreach (KeyValuePair<BnetGameAccountId, BnetGameAccount> kv in player.GetGameAccounts())
		{
			BnetGameAccountId gameAccountId = kv.Key;
			BnetGameAccount gameAccount = kv.Value;
			if (m_gameAccounts.TryGetValue(gameAccountId, out var existingGameAccount))
			{
				if ((object)existingGameAccount != gameAccount)
				{
					isNew = true;
					Log.All.PrintWarning("Already registered BnetAccount accountId={0} newSrc={1} - will overwrite.", accountId.Low, player.Source);
				}
			}
			else
			{
				isNew = true;
			}
			m_gameAccounts[gameAccountId] = gameAccount;
		}
		if (isNew)
		{
			BnetPlayerChange change = new BnetPlayerChange();
			change.SetNewPlayer(player);
			BnetPlayerChangelist changelist = new BnetPlayerChangelist();
			changelist.AddChange(change);
			FirePlayersChangedEvent(changelist);
		}
	}

	public bool IsSubscribedToPlayer(BnetGameAccountId id)
	{
		return BattleNet.IsSubscribedToEntity(id);
	}

	public void CheckSubscriptionsAndClearTransientStatus(BnetAccountId accountId)
	{
		if (!m_players.TryGetValue(accountId, out var player))
		{
			return;
		}
		foreach (KeyValuePair<BnetGameAccountId, BnetGameAccount> gameAccount in player.GetGameAccounts())
		{
			CheckSubscriptionsAndClearTransientStatus_Internal(gameAccount.Value);
		}
	}

	private void CheckSubscriptionsAndClearTransientStatus_Internal(BnetGameAccount gameAccount)
	{
		BnetGameAccountId gameAccountId = gameAccount.GetId();
		if (!IsSubscribedToPlayer(gameAccountId))
		{
			ClearTransientStatus(gameAccount);
			gameAccount.SetOnline(BnetNearbyPlayerMgr.Get().IsNearbyPlayer(gameAccount.GetId()));
			gameAccount.SetBusy(busy: false);
			gameAccount.SetAway(away: false);
			gameAccount.SetAwayTimeMicrosec(0L);
			gameAccount.SetRichPresence(null);
		}
	}

	private void ClearTransientStatus(BnetGameAccount gameAccount)
	{
		uint[] transientStatusFields = GamePresenceField.TransientStatusFields;
		foreach (uint fieldId in transientStatusFields)
		{
			gameAccount.SetGameField(fieldId, null);
		}
	}

	public bool SetGameField(uint fieldId, bool val)
	{
		if (!Network.ShouldBeConnectedToAurora())
		{
			Error.AddDevFatal("Caller should check for Battle.net connection before calling SetGameField {0}={1}", fieldId, val);
			return false;
		}
		if (!ShouldUpdateGameField(fieldId, val, out var hsGameAccount))
		{
			return false;
		}
		if (fieldId == 2)
		{
			hsGameAccount.SetBusy(val);
			int intVal = (val ? 1 : 0);
			BattleNet.SetPresenceInt(fieldId, intVal);
		}
		else
		{
			BattleNet.SetPresenceBool(fieldId, val);
		}
		BnetPlayerChangelist changelist = ChangeGameField(hsGameAccount, fieldId, val);
		switch (fieldId)
		{
		case 2u:
			if (val)
			{
				hsGameAccount.SetAway(away: false);
			}
			break;
		case 10u:
			if (val)
			{
				hsGameAccount.SetBusy(busy: false);
			}
			break;
		}
		FirePlayersChangedEvent(changelist);
		return true;
	}

	public bool SetGameField(uint fieldId, int val)
	{
		if (!Network.ShouldBeConnectedToAurora())
		{
			Error.AddDevFatal("Caller should check for Battle.net connection before calling SetGameField {0}={1}", fieldId, val);
			return false;
		}
		if (!ShouldUpdateGameField(fieldId, val, out var hsGameAccount))
		{
			return false;
		}
		BattleNet.SetPresenceInt(fieldId, val);
		BnetPlayerChangelist changelist = ChangeGameField(hsGameAccount, fieldId, val);
		FirePlayersChangedEvent(changelist);
		return true;
	}

	public bool SetGameField(uint fieldId, string val)
	{
		if (!Network.ShouldBeConnectedToAurora())
		{
			Error.AddDevFatal("Caller should check for Battle.net connection before calling SetGameField {0}={1}", fieldId, val);
			return false;
		}
		if (!ShouldUpdateGameField(fieldId, val, out var hsGameAccount))
		{
			return false;
		}
		BattleNet.SetPresenceString(fieldId, val);
		BnetPlayerChangelist changelist = ChangeGameField(hsGameAccount, fieldId, val);
		FirePlayersChangedEvent(changelist);
		return true;
	}

	public bool SetGameField(uint fieldId, byte[] val)
	{
		if (!Network.ShouldBeConnectedToAurora())
		{
			Error.AddDevFatal("Caller should check for Battle.net connection before calling SetGameField {0}=[{1}]", fieldId, (val == null) ? "" : val.Length.ToString());
			return false;
		}
		if (!ShouldUpdateGameFieldBlob(fieldId, val, out var hsGameAccount))
		{
			return false;
		}
		BattleNet.SetPresenceBlob(fieldId, val);
		BnetPlayerChangelist changelist = ChangeGameField(hsGameAccount, fieldId, val);
		FirePlayersChangedEvent(changelist);
		return true;
	}

	public bool SetGameFieldBlob(uint fieldId, IProtoBuf protoMessage)
	{
		if (fieldId == 21 || fieldId == 23)
		{
			SetPresenceSpectatorJoinInfo(protoMessage as JoinInfo);
			return true;
		}
		byte[] val = ((protoMessage == null) ? null : ProtobufUtil.ToByteArray(protoMessage));
		return SetGameField(fieldId, val);
	}

	public bool SetGameField(uint fieldId, BnetEntityId val)
	{
		if (!Network.ShouldBeConnectedToAurora())
		{
			Error.AddDevFatal("Caller should check for Battle.net connection before calling SetGameField {0}=[{1}]", fieldId, (val == null) ? "" : val.ToString());
			return false;
		}
		if (!ShouldUpdateGameField(fieldId, val, out var hsGameAccount))
		{
			return false;
		}
		BattleNet.SetPresenceEntityId(fieldId, val);
		BnetPlayerChangelist changelist = ChangeGameField(hsGameAccount, fieldId, val);
		FirePlayersChangedEvent(changelist);
		return true;
	}

	public void SetPresenceSpectatorJoinInfo(JoinInfo joinInfo)
	{
		byte[] joinInfoBytes = ((joinInfo == null) ? null : ProtobufUtil.ToByteArray(joinInfo));
		SetGameField(21u, joinInfoBytes);
		byte[] secretInfoBytes = null;
		SetGameField(23u, secretInfoBytes);
	}

	public bool AddPlayersChangedListener(PlayersChangedCallback callback)
	{
		return AddPlayersChangedListener(callback, null);
	}

	public bool AddPlayersChangedListener(PlayersChangedCallback callback, object userData)
	{
		PlayersChangedListener listener = new PlayersChangedListener();
		listener.SetCallback(callback);
		listener.SetUserData(userData);
		if (m_playersChangedListeners.Contains(listener))
		{
			return false;
		}
		m_playersChangedListeners.Add(listener);
		return true;
	}

	public bool RemovePlayersChangedListener(PlayersChangedCallback callback)
	{
		return RemovePlayersChangedListener(callback, null);
	}

	private bool RemovePlayersChangedListener(PlayersChangedCallback callback, object userData)
	{
		PlayersChangedListener listener = new PlayersChangedListener();
		listener.SetCallback(callback);
		listener.SetUserData(userData);
		return m_playersChangedListeners.Remove(listener);
	}

	public static bool RemovePlayersChangedListenerFromInstance(PlayersChangedCallback callback, object userData = null)
	{
		if (s_instance == null)
		{
			return false;
		}
		return s_instance.RemovePlayersChangedListener(callback, userData);
	}

	private void OnPresenceUpdate(PresenceUpdate[] updates)
	{
		BnetPlayerChangelist changelist = new BnetPlayerChangelist();
		foreach (PresenceUpdate update in updates.Where((PresenceUpdate u) => u.programId == BnetProgramId.BNET && u.groupId == 2 && u.fieldId == 7))
		{
			BnetGameAccountId gameAccountId = BnetGameAccountId.CreateFromBnetEntityId(update.entityId);
			BnetAccountId ownerId = BnetAccountId.CreateFromBnetEntityId(update.entityIdVal);
			if (!ownerId.IsEmpty())
			{
				if (GetAccount(ownerId) == null)
				{
					PresenceUpdate fakeAccountUpdate = default(PresenceUpdate);
					BnetPlayerChangelist ignoredAccountChangelist = new BnetPlayerChangelist();
					CreateAccount(ownerId, fakeAccountUpdate, ignoredAccountChangelist);
				}
				if (!gameAccountId.IsEmpty() && GetGameAccount(gameAccountId) == null)
				{
					CreateGameAccount(gameAccountId, update, changelist);
				}
			}
		}
		List<PresenceUpdate> hearthstonePresenceChanges = null;
		for (int i = 0; i < updates.Length; i++)
		{
			PresenceUpdate update2 = updates[i];
			if (update2.programId == BnetProgramId.BNET)
			{
				if (update2.groupId == 1)
				{
					OnAccountUpdate(update2, changelist);
				}
				else if (update2.groupId == 2)
				{
					OnGameAccountUpdate(update2, changelist);
				}
			}
			else if (update2.programId == BnetProgramId.HEARTHSTONE)
			{
				OnGameUpdate(update2, changelist);
			}
			if ((update2.programId == BnetProgramId.HEARTHSTONE || (update2.programId == BnetProgramId.BNET && update2.groupId == 2)) && this.OnGameAccountPresenceChange != null)
			{
				if (hearthstonePresenceChanges == null)
				{
					hearthstonePresenceChanges = new List<PresenceUpdate>();
				}
				hearthstonePresenceChanges.Add(update2);
			}
		}
		LogPresenceUpdates(updates);
		if (hearthstonePresenceChanges != null)
		{
			this.OnGameAccountPresenceChange(hearthstonePresenceChanges.ToArray());
		}
		FirePlayersChangedEvent(changelist);
	}

	private static void LogPresenceUpdates(PresenceUpdate[] updates)
	{
		Blizzard.T5.Logging.LogLevel presenceLogLevel = Blizzard.T5.Logging.LogLevel.Debug;
		bool presenceLogVerbosity = true;
		StringBuilder logLines = null;
		foreach (PresenceUpdate update in updates)
		{
			LogPresenceUpdate(ref logLines, presenceLogLevel, presenceLogVerbosity, update);
		}
		if (logLines != null)
		{
			Log.Presence.Print(presenceLogLevel, presenceLogVerbosity, logLines.ToString());
		}
	}

	private static void LogPresenceUpdate(ref StringBuilder buffer, Blizzard.T5.Logging.LogLevel level, bool verbosity, PresenceUpdate update)
	{
		if (HearthstoneApplication.IsPublic() || !Log.Presence.CanPrint(level, verbosity))
		{
			return;
		}
		BnetAccountId bnetAccountId = BnetAccountId.CreateFromBnetEntityId(update.entityId);
		BnetGameAccountId gameAccountId = BnetGameAccountId.CreateFromBnetEntityId(update.entityId);
		bool num = bnetAccountId == BattleNet.GetMyAccoundId() || gameAccountId == BattleNet.GetMyGameAccountId();
		BnetPlayer player = Get().GetPlayer(gameAccountId);
		if (player == null)
		{
			player = Get().GetPlayer(bnetAccountId);
		}
		bool isFriend = !num && BnetFriendMgr.Get().IsFriend(player);
		bool isOpponentPlayer = !num && GameState.Get() != null && (GameMgr.Get() == null || !GameMgr.Get().IsSpectator()) && GameState.Get().GetOpposingSidePlayer() != null && GameState.Get().GetOpposingSidePlayer().GetGameAccountId() == gameAccountId;
		string personDesc = (num ? "myself" : (isOpponentPlayer ? "opponent" : (isFriend ? "friend" : string.Empty)));
		if (!num && !isOpponentPlayer && !isFriend)
		{
			if (BnetNearbyPlayerMgr.Get().IsNearbyPlayer(player))
			{
				personDesc = "nearbyplayer";
			}
			else if (BnetParty.GetJoinedParties().Any((PartyInfo p) => p.Type == PartyType.SPECTATOR_PARTY && BnetParty.IsMember(p.Id, gameAccountId)))
			{
				personDesc = "fellowspecator";
			}
		}
		string battleTag = ((player == null || player.GetBattleTag() == null) ? "" : player.GetBattleTag().ToString());
		if (string.IsNullOrEmpty(battleTag) && update.programId == BnetProgramId.BNET && ((update.groupId == 1 && update.fieldId == 4) || (update.groupId == 2 && update.fieldId == 5)))
		{
			battleTag = update.stringVal;
		}
		personDesc = ((!string.IsNullOrEmpty(battleTag) || !string.IsNullOrEmpty(personDesc)) ? $"{battleTag}{((string.IsNullOrEmpty(battleTag) || string.IsNullOrEmpty(personDesc)) ? personDesc : $"({personDesc})")}" : "someone");
		BnetProgramId programId = new BnetProgramId(update.programId);
		string groupName;
		string fieldName;
		if (programId == BnetProgramId.BNET)
		{
			groupName = BnetPresenceField.GetGroupName(update.groupId);
			fieldName = BnetPresenceField.GetFieldName(update.groupId, update.fieldId);
		}
		else if (programId == BnetProgramId.HEARTHSTONE)
		{
			groupName = "GameAccount";
			fieldName = GamePresenceField.GetFieldName(update.fieldId);
		}
		else
		{
			groupName = update.groupId.ToString();
			fieldName = update.fieldId.ToString();
		}
		string fieldValue = GetFieldValue(update);
		if (buffer == null)
		{
			buffer = new StringBuilder();
		}
		else
		{
			buffer.Append("\n");
		}
		buffer.AppendFormat("Update entity={0} who={1} {2}.{3}.{4}={5}", $"{{hi:{update.entityId?.High} lo:{update.entityId?.Low}}}", personDesc, programId, groupName, fieldName, fieldValue);
	}

	private static string GetFieldValue(PresenceUpdate update)
	{
		if (update.programId == BnetProgramId.HEARTHSTONE)
		{
			return GamePresenceField.GetFieldValue(update);
		}
		return BnetPresenceField.GetFieldValue(update);
	}

	private void OnDisconnectedFromBattleNet(BattleNetErrors error)
	{
		m_accounts.Clear();
		m_gameAccounts.Clear();
		m_players.Clear();
	}

	private void OnAccountUpdate(PresenceUpdate update, BnetPlayerChangelist changelist)
	{
		BnetAccountId id = BnetAccountId.CreateFromBnetEntityId(update.entityId);
		BnetAccount account = null;
		if (!m_accounts.TryGetValue(id, out account))
		{
			CreateAccount(id, update, changelist);
		}
		else
		{
			UpdateAccount(account, update, changelist);
		}
	}

	private void CreateAccount(BnetAccountId id, PresenceUpdate update, BnetPlayerChangelist changelist)
	{
		BnetAccount account = new BnetAccount();
		m_accounts.Add(id, account);
		account.SetId(id);
		BnetPlayer player = null;
		if (!m_players.TryGetValue(id, out player))
		{
			player = new BnetPlayer(BnetPlayerSource.PRESENCE_UPDATE);
			m_players.Add(id, player);
			BnetPlayerChange change = new BnetPlayerChange();
			change.SetNewPlayer(player);
			changelist.AddChange(change);
		}
		player.SetAccount(account);
		UpdateAccount(account, update, changelist);
	}

	private void UpdateAccount(BnetAccount account, PresenceUpdate update, BnetPlayerChangelist changelist)
	{
		BnetPlayer player = m_players[account.GetId()];
		if (update.fieldId == 7)
		{
			bool away = update.boolVal;
			if (away != account.IsAway())
			{
				AddChangedPlayer(player, changelist);
				account.SetAway(away);
				if (away)
				{
					account.SetBusy(busy: false);
				}
			}
		}
		else if (update.fieldId == 8)
		{
			long awayTimeMicrosec = update.intVal;
			if (awayTimeMicrosec != account.GetAwayTimeMicrosec())
			{
				AddChangedPlayer(player, changelist);
				account.SetAwayTimeMicrosec(awayTimeMicrosec);
			}
		}
		else if (update.fieldId == 11)
		{
			bool busy = update.boolVal;
			if (busy != account.IsBusy())
			{
				AddChangedPlayer(player, changelist);
				account.SetBusy(busy);
				if (busy)
				{
					account.SetAway(away: false);
				}
			}
		}
		else if (update.fieldId == 4)
		{
			BnetBattleTag battleTag = BnetBattleTag.CreateFromString(update.stringVal);
			if (battleTag == null)
			{
				Log.All.Print("Failed to parse BattleTag={0} for account={1}", update.stringVal, update.entityId?.Low);
			}
			if (!(battleTag == account.GetBattleTag()))
			{
				AddChangedPlayer(player, changelist);
				account.SetBattleTag(battleTag);
			}
		}
		else if (update.fieldId == 1)
		{
			string name = update.stringVal;
			if (name == null)
			{
				Error.AddDevFatal("BnetPresenceMgr.UpdateAccount() - Failed to convert full name to native string for {0}.", account);
			}
			else if (!(name == account.GetFullName()))
			{
				if (name == string.Empty && update.valCleared)
				{
					name = null;
				}
				AddChangedPlayer(player, changelist);
				account.SetFullName(name);
			}
		}
		else if (update.fieldId == 6)
		{
			long lastOnlineMicrosec = update.intVal;
			if (lastOnlineMicrosec != account.GetLastOnlineMicrosec())
			{
				AddChangedPlayer(player, changelist);
				account.SetLastOnlineMicrosec(lastOnlineMicrosec);
			}
		}
		else if (update.fieldId != 3 && update.fieldId == 12)
		{
			bool appearOffline = update.boolVal;
			if (appearOffline != account.IsAppearingOffline())
			{
				AddChangedPlayer(player, changelist);
				account.SetAppearingOffline(appearOffline);
			}
		}
	}

	private void OnGameAccountUpdate(PresenceUpdate update, BnetPlayerChangelist changelist)
	{
		BnetGameAccountId id = BnetGameAccountId.CreateFromBnetEntityId(update.entityId);
		BnetGameAccount gameAccount = null;
		if (!m_gameAccounts.TryGetValue(id, out gameAccount))
		{
			CreateGameAccount(id, update, changelist);
		}
		else
		{
			UpdateGameAccount(gameAccount, update, changelist);
		}
	}

	private void CreateGameAccount(BnetGameAccountId id, PresenceUpdate update, BnetPlayerChangelist changelist)
	{
		BnetGameAccount gameAccount = new BnetGameAccount();
		m_gameAccounts.Add(id, gameAccount);
		gameAccount.SetId(id);
		UpdateGameAccount(gameAccount, update, changelist);
	}

	private void UpdateGameAccount(BnetGameAccount gameAccount, PresenceUpdate update, BnetPlayerChangelist changelist)
	{
		BnetPlayer player = null;
		BnetAccountId playerId = gameAccount.GetOwnerId();
		if (playerId != null)
		{
			m_players.TryGetValue(playerId, out player);
		}
		if (update.fieldId == 2)
		{
			int oldBusyInt = (gameAccount.IsBusy() ? 1 : 0);
			int newBusyInt = (int)update.intVal;
			if (newBusyInt != oldBusyInt)
			{
				AddChangedPlayer(player, changelist);
				bool busy = newBusyInt == 1;
				gameAccount.SetBusy(busy);
				if (busy)
				{
					gameAccount.SetAway(away: false);
				}
				HandleGameAccountChange(player, update);
			}
		}
		else if (update.fieldId == 10)
		{
			bool away = update.boolVal;
			if (away != gameAccount.IsAway())
			{
				AddChangedPlayer(player, changelist);
				gameAccount.SetAway(away);
				if (away)
				{
					gameAccount.SetBusy(busy: false);
				}
				HandleGameAccountChange(player, update);
			}
		}
		else if (update.fieldId == 11)
		{
			long awayTimeMicrosec = update.intVal;
			if (awayTimeMicrosec != gameAccount.GetAwayTimeMicrosec())
			{
				AddChangedPlayer(player, changelist);
				gameAccount.SetAwayTimeMicrosec(awayTimeMicrosec);
				HandleGameAccountChange(player, update);
			}
		}
		else if (update.fieldId == 5)
		{
			BnetBattleTag battleTag = BnetBattleTag.CreateFromString(update.stringVal);
			if (battleTag == null)
			{
				Log.All.Print("Failed to parse BattleTag={0} for gameAccount={1}", update.stringVal, update.entityId?.Low);
			}
			if (!(battleTag == gameAccount.GetBattleTag()))
			{
				AddChangedPlayer(player, changelist);
				gameAccount.SetBattleTag(battleTag);
				HandleGameAccountChange(player, update);
			}
		}
		else if (update.fieldId == 1)
		{
			bool online = update.boolVal;
			if (online != gameAccount.IsOnline())
			{
				AddChangedPlayer(player, changelist);
				gameAccount.SetOnline(online);
				if (!online)
				{
					ClearTransientStatus(gameAccount);
				}
				HandleGameAccountChange(player, update);
			}
		}
		else if (update.fieldId == 3)
		{
			BnetProgramId programId = new BnetProgramId(update.stringVal);
			if (!(programId == gameAccount.GetProgramId()))
			{
				AddChangedPlayer(player, changelist);
				gameAccount.SetProgramId(programId);
				HandleGameAccountChange(player, update);
			}
		}
		else if (update.fieldId == 4)
		{
			long lastOnlineMicrosec = update.intVal;
			if (lastOnlineMicrosec != gameAccount.GetLastOnlineMicrosec())
			{
				AddChangedPlayer(player, changelist);
				gameAccount.SetLastOnlineMicrosec(lastOnlineMicrosec);
				HandleGameAccountChange(player, update);
			}
		}
		else if (update.fieldId == 7)
		{
			BnetAccountId ownerId = BnetAccountId.CreateFromBnetEntityId(update.entityIdVal);
			if (!(ownerId == gameAccount.GetOwnerId()))
			{
				UpdateGameAccountOwner(ownerId, gameAccount, changelist);
			}
		}
		else if (update.fieldId == 9)
		{
			if (update.valCleared && gameAccount.GetRichPresence() != null)
			{
				AddChangedPlayer(player, changelist);
				gameAccount.SetRichPresence(null);
				HandleGameAccountChange(player, update);
			}
		}
		else if (update.fieldId == 1000)
		{
			string richPresence = update.stringVal;
			if (richPresence == null)
			{
				richPresence = "";
			}
			if (!(richPresence == gameAccount.GetRichPresence()))
			{
				AddChangedPlayer(player, changelist);
				gameAccount.SetRichPresence(richPresence);
				HandleGameAccountChange(player, update);
			}
		}
	}

	private void UpdateGameAccountOwner(BnetAccountId ownerId, BnetGameAccount gameAccount, BnetPlayerChangelist changelist)
	{
		BnetPlayer oldOwner = null;
		BnetAccountId oldOwnerId = gameAccount.GetOwnerId();
		if (oldOwnerId != null && m_players.TryGetValue(oldOwnerId, out oldOwner))
		{
			oldOwner.RemoveGameAccount(gameAccount.GetId());
			AddChangedPlayer(oldOwner, changelist);
		}
		BnetPlayer owner = null;
		if (m_players.TryGetValue(ownerId, out owner))
		{
			AddChangedPlayer(owner, changelist);
		}
		else
		{
			owner = new BnetPlayer(BnetPlayerSource.PRESENCE_UPDATE);
			m_players.Add(ownerId, owner);
			BnetPlayerChange change = new BnetPlayerChange();
			change.SetNewPlayer(owner);
			changelist.AddChange(change);
		}
		gameAccount.SetOwnerId(ownerId);
		owner.AddGameAccount(gameAccount);
		CacheMyself(gameAccount, owner);
	}

	private void HandleGameAccountChange(BnetPlayer player, PresenceUpdate update)
	{
		player?.OnGameAccountChanged(update.fieldId);
	}

	private void OnGameUpdate(PresenceUpdate update, BnetPlayerChangelist changelist)
	{
		BnetGameAccountId id = BnetGameAccountId.CreateFromBnetEntityId(update.entityId);
		BnetGameAccount gameAccount = null;
		if (!m_gameAccounts.TryGetValue(id, out gameAccount))
		{
			CreateGameInfo(id, update, changelist);
		}
		else
		{
			UpdateGameInfo(gameAccount, update, changelist);
		}
	}

	private void CreateGameInfo(BnetGameAccountId id, PresenceUpdate update, BnetPlayerChangelist changelist)
	{
		BnetGameAccount gameAccount = new BnetGameAccount();
		m_gameAccounts.Add(id, gameAccount);
		gameAccount.SetId(id);
		UpdateGameInfo(gameAccount, update, changelist);
	}

	private void UpdateGameInfo(BnetGameAccount gameAccount, PresenceUpdate update, BnetPlayerChangelist changelist)
	{
		BnetPlayer player = null;
		BnetAccountId playerId = gameAccount.GetOwnerId();
		if (playerId != null)
		{
			m_players.TryGetValue(playerId, out player);
		}
		if (update.valCleared)
		{
			if (gameAccount.HasGameField(update.fieldId))
			{
				AddChangedPlayer(player, changelist);
				gameAccount.RemoveGameField(update.fieldId);
				HandleGameAccountChange(player, update);
			}
			return;
		}
		switch (update.fieldId)
		{
		case 1u:
			if (update.boolVal != gameAccount.GetGameFieldBool(update.fieldId))
			{
				AddChangedPlayer(player, changelist);
				gameAccount.SetGameField(update.fieldId, update.boolVal);
				HandleGameAccountChange(player, update);
			}
			break;
		case 5u:
		case 6u:
		case 7u:
		case 8u:
		case 9u:
		case 10u:
		case 11u:
		case 12u:
		case 13u:
		case 14u:
		case 15u:
		case 16u:
		case 27u:
		case 28u:
		case 29u:
		case 30u:
		case 31u:
			if ((int)update.intVal != gameAccount.GetGameFieldInt(update.fieldId))
			{
				AddChangedPlayer(player, changelist);
				gameAccount.SetGameField(update.fieldId, (int)update.intVal);
				HandleGameAccountChange(player, update);
			}
			break;
		case 2u:
		case 4u:
		case 19u:
		case 20u:
			if (!(update.stringVal == gameAccount.GetGameFieldString(update.fieldId)))
			{
				AddChangedPlayer(player, changelist);
				gameAccount.SetGameField(update.fieldId, update.stringVal);
				HandleGameAccountChange(player, update);
			}
			break;
		case 17u:
		case 18u:
		case 21u:
		case 22u:
		case 23u:
		case 24u:
			if (!GeneralUtils.AreBytesEqual(update.blobVal, gameAccount.GetGameFieldBytes(update.fieldId)))
			{
				AddChangedPlayer(player, changelist);
				gameAccount.SetGameField(update.fieldId, update.blobVal);
				HandleGameAccountChange(player, update);
			}
			break;
		case 26u:
			if (!(update.entityIdVal == gameAccount.GetGameFieldEntityId(update.fieldId)))
			{
				AddChangedPlayer(player, changelist);
				gameAccount.SetGameField(update.fieldId, update.entityIdVal);
				HandleGameAccountChange(player, update);
			}
			break;
		default:
			Log.Presence.PrintWarning("Unknown HS game account fieldId={0} - not saved into presence cache.", update.fieldId);
			break;
		}
	}

	private void CacheMyself(BnetGameAccount gameAccount, BnetPlayer player)
	{
		if (player != m_myPlayer && !(gameAccount.GetId() != m_myGameAccountId))
		{
			m_myPlayer = player;
		}
	}

	private void AddChangedPlayer(BnetPlayer player, BnetPlayerChangelist changelist)
	{
		if (player != null && !changelist.HasChange(player))
		{
			BnetPlayerChange change = new BnetPlayerChange();
			change.SetOldPlayer(player.Clone());
			change.SetNewPlayer(player);
			changelist.AddChange(change);
		}
	}

	private void FirePlayersChangedEvent(BnetPlayerChangelist changelist)
	{
		if (changelist != null && changelist.GetChanges().Count != 0)
		{
			PlayersChangedListener[] listeners = m_playersChangedListeners.ToArray();
			for (int i = 0; i < listeners.Length; i++)
			{
				listeners[i].Fire(changelist);
			}
		}
	}

	private bool ShouldUpdateGameField(uint fieldId, object val, out BnetGameAccount hsGameAccount)
	{
		hsGameAccount = null;
		if (m_myPlayer == null)
		{
			return true;
		}
		hsGameAccount = m_myPlayer.GetHearthstoneGameAccount();
		if (hsGameAccount == null)
		{
			return true;
		}
		if (hsGameAccount.TryGetGameField(fieldId, out var curVal) && val.Equals(curVal))
		{
			return false;
		}
		return true;
	}

	private bool ShouldUpdateGameFieldBlob(uint fieldId, byte[] val, out BnetGameAccount hsGameAccount)
	{
		hsGameAccount = null;
		if (m_myPlayer == null)
		{
			return true;
		}
		hsGameAccount = m_myPlayer.GetHearthstoneGameAccount();
		if (hsGameAccount == null)
		{
			return true;
		}
		if (hsGameAccount.TryGetGameFieldBytes(fieldId, out var curVal) && GeneralUtils.AreArraysEqual(val, curVal))
		{
			return false;
		}
		return true;
	}

	private BnetPlayerChangelist ChangeGameField(BnetGameAccount hsGameAccount, uint fieldId, object val)
	{
		if (hsGameAccount == null)
		{
			return null;
		}
		BnetPlayerChange change = new BnetPlayerChange();
		change.SetOldPlayer(m_myPlayer.Clone());
		change.SetNewPlayer(m_myPlayer);
		hsGameAccount.SetGameField(fieldId, val);
		BnetPlayerChangelist bnetPlayerChangelist = new BnetPlayerChangelist();
		bnetPlayerChangelist.AddChange(change);
		return bnetPlayerChangelist;
	}
}
