using System;
using System.Collections.Generic;
using Assets;
using Blizzard.GameService.SDK.Client.Integration;
using Blizzard.T5.Configuration;
using Blizzard.T5.Core;
using Blizzard.T5.Core.Utils;
using Blizzard.T5.Jobs;
using Blizzard.T5.Logging;
using Blizzard.T5.Services;
using Hearthstone;
using Hearthstone.Core;
using PegasusClient;
using PegasusGame;
using PegasusShared;
using PegasusUtil;
using SpectatorProto;
using UnityEngine;

public class GameMgr : IService
{
	public delegate bool FindGameCallback(FindGameEventData eventData, object userData);

	private class FindGameListener : EventListener<FindGameCallback>
	{
		public bool Fire(FindGameEventData eventData)
		{
			return m_callback(eventData, m_userData);
		}
	}

	private const string MATCHING_POPUP_PC_NAME = "MatchingPopup3D.prefab:4f4a40d14d907e94da1b81d97c18a44f";

	private const string MATCHING_POPUP_PHONE_NAME = "MatchingPopup3D_phone.prefab:a7a5cea6306a1fa4680a9782fd25be14";

	private const string LOADING_POPUP_NAME = "LoadingPopup.prefab:ff9266f7c55faa94b9cd0f1371df7168";

	private const int MINIMUM_SECONDS_TIL_TB_END_TO_RETURN_TO_TB_SCENE = 10;

	private PlatformDependentValue<string> MATCHING_POPUP_NAME;

	private readonly Map<string, Type> s_transitionPopupNameToType = new Map<string, Type>
	{
		{
			"MatchingPopup3D.prefab:4f4a40d14d907e94da1b81d97c18a44f",
			typeof(MatchingPopupDisplay)
		},
		{
			"MatchingPopup3D_phone.prefab:a7a5cea6306a1fa4680a9782fd25be14",
			typeof(MatchingPopupDisplay)
		},
		{
			"LoadingPopup.prefab:ff9266f7c55faa94b9cd0f1371df7168",
			typeof(LoadingPopupDisplay)
		}
	};

	private LastGameData m_lastGameData = new LastGameData();

	private GameConnectionInfo m_connectionInfoForGameConnectingTo;

	[StatePrinter.IncludeState]
	private GameType m_gameType;

	[StatePrinter.IncludeState]
	private GameType m_prevGameType;

	[StatePrinter.IncludeState]
	private GameType m_nextGameType;

	private FormatType m_formatType;

	private FormatType m_prevFormatType;

	private FormatType m_nextFormatType;

	private int m_missionId;

	private int m_prevMissionId;

	private int m_nextMissionId;

	private int m_brawlLibraryItemId;

	private int m_nextBrawlLibraryItemId;

	[StatePrinter.IncludeState]
	private GameReconnectType m_gameReconnectType;

	[StatePrinter.IncludeState]
	private GameReconnectType m_prevGameReconnectType;

	[StatePrinter.IncludeState]
	private GameReconnectType m_nextGameReconnectType;

	[StatePrinter.IncludeState]
	private bool m_readyToProcessGameConnections;

	[StatePrinter.IncludeState]
	private GameConnectionInfo m_deferredGameConnectionInfo;

	[StatePrinter.IncludeState]
	private bool m_spectator;

	[StatePrinter.IncludeState]
	private bool m_prevSpectator;

	[StatePrinter.IncludeState]
	private bool m_nextSpectator;

	private long? m_lastDeckId;

	private string m_lastAIDeck;

	private int? m_lastHeroCardDbId;

	private int? m_lastSeasonId;

	[StatePrinter.IncludeState]
	private int m_gameHandleId;

	[StatePrinter.IncludeState]
	private uint m_lastEnterGameError;

	[StatePrinter.IncludeState]
	private bool m_pendingAutoConcede;

	[StatePrinter.IncludeState]
	private FindGameState m_findGameState;

	private List<FindGameListener> m_findGameListeners = new List<FindGameListener>();

	private TransitionPopup m_transitionPopup;

	private Vector3 m_initialTransitionPopupPos;

	private Network.GameSetup m_gameSetup;

	private Map<int, string> m_lastDisplayedPlayerNames = new Map<int, string>();

	private static Map<QueueEvent.Type, FindGameState?> s_bnetToFindGameResultMap = new Map<QueueEvent.Type, FindGameState?>
	{
		{
			QueueEvent.Type.UNKNOWN,
			null
		},
		{
			QueueEvent.Type.QUEUE_ENTER,
			FindGameState.BNET_QUEUE_ENTERED
		},
		{
			QueueEvent.Type.QUEUE_LEAVE,
			null
		},
		{
			QueueEvent.Type.QUEUE_DELAY,
			FindGameState.BNET_QUEUE_DELAYED
		},
		{
			QueueEvent.Type.QUEUE_UPDATE,
			FindGameState.BNET_QUEUE_UPDATED
		},
		{
			QueueEvent.Type.QUEUE_DELAY_ERROR,
			FindGameState.BNET_ERROR
		},
		{
			QueueEvent.Type.QUEUE_AMM_ERROR,
			FindGameState.BNET_ERROR
		},
		{
			QueueEvent.Type.QUEUE_WAIT_END,
			null
		},
		{
			QueueEvent.Type.QUEUE_CANCEL,
			FindGameState.BNET_QUEUE_CANCELED
		},
		{
			QueueEvent.Type.QUEUE_GAME_STARTED,
			FindGameState.SERVER_GAME_CONNECTING
		},
		{
			QueueEvent.Type.ABORT_CLIENT_DROPPED,
			FindGameState.BNET_ERROR
		}
	};

	public const int NO_BRAWL_LIBRARY_ITEM_ID = 0;

	private static Blizzard.T5.Core.ILogger GameNetLogger => Network.Get().GameNetLogger;

	public long? LastDeckId => m_lastDeckId;

	public int? LastHeroCardDbId => m_lastHeroCardDbId;

	public LastGameData LastGameData => m_lastGameData;

	public event Action OnTransitionPopupShown;

	public IEnumerator<IAsyncJobResult> Initialize(ServiceLocator serviceLocator)
	{
		HearthstoneApplication.Get().WillReset += WillReset;
		MATCHING_POPUP_NAME = new PlatformDependentValue<string>(PlatformCategory.Screen)
		{
			PC = "MatchingPopup3D.prefab:4f4a40d14d907e94da1b81d97c18a44f",
			Phone = "MatchingPopup3D_phone.prefab:a7a5cea6306a1fa4680a9782fd25be14"
		};
		Network network = serviceLocator.Get<Network>();
		network.RegisterGameQueueHandler(OnGameQueueEvent);
		network.RegisterNetHandler(GameToConnectNotification.PacketID.ID, OnGameToJoinNotification);
		network.RegisterNetHandler(GameSetup.PacketID.ID, OnGameSetup);
		network.RegisterNetHandler(GameCanceled.PacketID.ID, OnGameCanceled);
		network.RegisterNetHandler(ServerResult.PacketID.ID, OnServerResult);
		network.AddBnetErrorListener(BnetFeature.Games, OnBnetError);
		FatalErrorMgr.Get().AddErrorListener(OnFatalError);
		yield break;
	}

	public Type[] GetDependencies()
	{
		return new Type[1] { typeof(Network) };
	}

	public void Shutdown()
	{
	}

	private void WillReset()
	{
		m_gameType = GameType.GT_UNKNOWN;
		m_prevGameType = GameType.GT_UNKNOWN;
		m_nextGameType = GameType.GT_UNKNOWN;
		m_formatType = FormatType.FT_UNKNOWN;
		m_prevFormatType = FormatType.FT_UNKNOWN;
		m_nextFormatType = FormatType.FT_UNKNOWN;
		m_missionId = 0;
		m_prevMissionId = 0;
		m_nextMissionId = 0;
		m_brawlLibraryItemId = 0;
		m_nextBrawlLibraryItemId = 0;
		m_gameReconnectType = GameReconnectType.INVALID;
		m_prevGameReconnectType = GameReconnectType.INVALID;
		m_nextGameReconnectType = GameReconnectType.INVALID;
		m_readyToProcessGameConnections = false;
		m_deferredGameConnectionInfo = null;
		m_spectator = false;
		m_prevSpectator = false;
		m_nextSpectator = false;
		m_lastEnterGameError = 0u;
		m_findGameState = FindGameState.INVALID;
		m_gameSetup = null;
		m_lastDisplayedPlayerNames.Clear();
		m_connectionInfoForGameConnectingTo = null;
		m_gameHandleId = 0;
		m_lastGameData.Clear();
	}

	public static GameMgr Get()
	{
		return ServiceManager.Get<GameMgr>();
	}

	public void OnLoggedIn()
	{
		SceneMgr.Get().RegisterSceneUnloadedEvent(OnSceneUnloaded);
		SceneMgr.Get().RegisterScenePreLoadEvent(OnScenePreLoad);
		ReconnectMgr.Get().AddGameTimeoutListener(OnReconnectTimeout);
	}

	public GameType GetGameType()
	{
		return m_gameType;
	}

	public GameType GetPreviousGameType()
	{
		return m_prevGameType;
	}

	public GameType GetNextGameType()
	{
		return m_nextGameType;
	}

	public FormatType GetFormatType()
	{
		return m_formatType;
	}

	public FormatType GetPreviousFormatType()
	{
		return m_prevFormatType;
	}

	public FormatType GetNextFormatType()
	{
		return m_nextFormatType;
	}

	public int GetMissionId()
	{
		return m_missionId;
	}

	public int GetPreviousMissionId()
	{
		return m_prevMissionId;
	}

	public int GetNextMissionId()
	{
		return m_nextMissionId;
	}

	public GameReconnectType GetReconnectType()
	{
		return m_gameReconnectType;
	}

	public GameReconnectType GetPreviousReconnectType()
	{
		return m_prevGameReconnectType;
	}

	public GameReconnectType GetNextReconnectType()
	{
		return m_nextGameReconnectType;
	}

	public bool IsReconnect()
	{
		return m_gameReconnectType != GameReconnectType.INVALID;
	}

	public bool IsPreviousReconnect()
	{
		return m_prevGameReconnectType != GameReconnectType.INVALID;
	}

	public bool IsNextReconnect()
	{
		return m_nextGameReconnectType != GameReconnectType.INVALID;
	}

	public bool IsSpectator()
	{
		return m_spectator;
	}

	public bool WasSpectator()
	{
		return m_prevSpectator;
	}

	public bool IsNextSpectator()
	{
		return m_nextSpectator;
	}

	public int GetGameHandle()
	{
		return m_gameHandleId;
	}

	public uint GetLastEnterGameError()
	{
		return m_lastEnterGameError;
	}

	public bool IsPendingAutoConcede()
	{
		return m_pendingAutoConcede;
	}

	public void SetPendingAutoConcede(bool pendingAutoConcede)
	{
		if (Network.Get().IsConnectedToGameServer())
		{
			m_pendingAutoConcede = pendingAutoConcede;
		}
	}

	public Network.GameSetup GetGameSetup()
	{
		return m_gameSetup;
	}

	public bool ConnectToGame(GameConnectionInfo info)
	{
		GameNetLogger.Log(Blizzard.T5.Core.LogLevel.Debug, "GameMgr.ConnectToGame()");
		if (info == null)
		{
			Log.GameMgr.PrintWarning("ConnectToGame() called with no GameConnectionInfo passed in!");
			GameNetLogger.Log(Blizzard.T5.Core.LogLevel.Debug, Network.Get().GetNetworkGameStateString());
			return false;
		}
		if (!m_readyToProcessGameConnections)
		{
			Log.GameMgr.Print("Received a GameConnectionInfo packet before the game is finished initializing; deferring it until later.");
			if (m_deferredGameConnectionInfo != null)
			{
				GameNetLogger.Log(Blizzard.T5.Core.LogLevel.Warning, "Another deferredGameConnectionInfo packet already exists.  Older packet GameType: {0}  Newer packet GameType: {1}", m_deferredGameConnectionInfo.GameType, info.GameType);
				GameNetLogger.Log(Blizzard.T5.Core.LogLevel.Warning, "Stomping over another deferred GameConnectionInfo packet.");
			}
			m_deferredGameConnectionInfo = info;
			return false;
		}
		FindGameState? result = s_bnetToFindGameResultMap[QueueEvent.Type.QUEUE_GAME_STARTED];
		GameServerInfo gameServer = new GameServerInfo();
		gameServer.Address = info.Address;
		gameServer.Port = (uint)info.Port;
		gameServer.GameHandle = (uint)info.GameHandle;
		gameServer.ClientHandle = info.ClientHandle;
		gameServer.AuroraPassword = info.AuroraPassword;
		gameServer.Mission = info.Scenario;
		m_nextGameType = info.GameType;
		m_nextFormatType = info.FormatType;
		m_nextMissionId = info.Scenario;
		m_connectionInfoForGameConnectingTo = info;
		gameServer.Version = BattleNet.GetVersion();
		QueueEvent queueEvent = new QueueEvent(QueueEvent.Type.QUEUE_GAME_STARTED, 0, 0, 0, gameServer);
		ChangeFindGameState(result.Value, queueEvent, queueEvent.GameServer, null);
		return true;
	}

	public bool ConnectToGameIfHaveDeferredConnectionPacket()
	{
		GameNetLogger.Log(Blizzard.T5.Core.LogLevel.Debug, "GameMgr.ConnectToGameIfHaveDeferredConnectionPacket()");
		m_readyToProcessGameConnections = true;
		if (m_deferredGameConnectionInfo != null)
		{
			bool result = ConnectToGame(m_deferredGameConnectionInfo);
			m_deferredGameConnectionInfo = null;
			return result;
		}
		return false;
	}

	public FindGameState GetFindGameState()
	{
		return m_findGameState;
	}

	public bool IsFindingGame()
	{
		return m_findGameState != FindGameState.INVALID;
	}

	public bool IsAboutToStopFindingGame()
	{
		switch (m_findGameState)
		{
		case FindGameState.CLIENT_CANCELED:
		case FindGameState.CLIENT_ERROR:
		case FindGameState.BNET_QUEUE_CANCELED:
		case FindGameState.BNET_ERROR:
		case FindGameState.SERVER_GAME_STARTED:
		case FindGameState.SERVER_GAME_CANCELED:
			return true;
		default:
			return false;
		}
	}

	public void RegisterFindGameEvent(FindGameCallback callback)
	{
		RegisterFindGameEvent(callback, null);
	}

	public void RegisterFindGameEvent(FindGameCallback callback, object userData)
	{
		FindGameListener listener = new FindGameListener();
		listener.SetCallback(callback);
		listener.SetUserData(userData);
		if (!m_findGameListeners.Contains(listener))
		{
			m_findGameListeners.Add(listener);
		}
	}

	public bool UnregisterFindGameEvent(FindGameCallback callback)
	{
		return UnregisterFindGameEvent(callback, null);
	}

	public bool UnregisterFindGameEvent(FindGameCallback callback, object userData)
	{
		FindGameListener listener = new FindGameListener();
		listener.SetCallback(callback);
		listener.SetUserData(userData);
		return m_findGameListeners.Remove(listener);
	}

	private void FindGameInternal(GameType gameType, FormatType formatType, int missionId, int brawlLibraryItemId, long deckId, string aiDeck, int heroCardDbId, int? seasonId, bool restoreSavedGameState, byte[] snapshot, int? lettuceMapNodeId, long lettuceTeamId, GameType progFilterOverride = GameType.GT_UNKNOWN, int deckTemplateId = 0)
	{
		m_lastEnterGameError = 0u;
		m_nextGameType = gameType;
		m_nextFormatType = formatType;
		m_nextMissionId = missionId;
		m_nextBrawlLibraryItemId = brawlLibraryItemId;
		m_lastDeckId = deckId;
		m_lastAIDeck = aiDeck;
		m_lastHeroCardDbId = heroCardDbId;
		m_lastSeasonId = seasonId;
		ChangeFindGameState(FindGameState.CLIENT_STARTED);
		Network.Get().FindGame(gameType, formatType, missionId, brawlLibraryItemId, deckId, aiDeck, heroCardDbId, seasonId, restoreSavedGameState, snapshot, lettuceMapNodeId, lettuceTeamId, progFilterOverride, deckTemplateId);
		UpdateSessionPresence(gameType);
	}

	public void FindGame(GameType gameType, FormatType formatType, int missionId, int brawlLibraryItemId = 0, long deckId = 0L, string aiDeck = null, int? seasonId = null, bool restoreSavedGameState = false, byte[] snapshot = null, int? lettuceMapNodeId = null, long lettuceTeamId = 0L, GameType progFilterOverride = GameType.GT_UNKNOWN, int deckTemplateId = 0)
	{
		FindGameInternal(gameType, formatType, missionId, brawlLibraryItemId, deckId, aiDeck, 0, seasonId, restoreSavedGameState, snapshot, lettuceMapNodeId, lettuceTeamId, progFilterOverride, deckTemplateId);
		if (!restoreSavedGameState)
		{
			string popupName = DetermineTransitionPopupForFindGame(gameType, missionId);
			if (popupName != null)
			{
				ShowTransitionPopup(popupName, missionId);
			}
		}
		CollectionManager collectionManager = CollectionManager.Get();
		if (collectionManager != null)
		{
			CollectionDeck collectionDeck = collectionManager.GetDeck(deckId);
			if (collectionDeck != null)
			{
				Log.Decks.PrintInfo("Finding Game With Deck:");
				collectionDeck.LogDeckStringInformation();
			}
		}
	}

	public void FindGameWithHero(GameType gameType, FormatType formatType, int missionId, int brawlLibraryItemId, int heroCardDbId, long deckid = 0L)
	{
		FindGameInternal(gameType, formatType, missionId, brawlLibraryItemId, deckid, null, heroCardDbId, null, restoreSavedGameState: false, null, null, 0L);
		string popupName = DetermineTransitionPopupForFindGame(gameType, missionId);
		if (popupName != null)
		{
			ShowTransitionPopup(popupName, missionId);
		}
		Log.Decks.PrintInfo("Finding Game With Hero: {0}", heroCardDbId);
	}

	public void Cheat_ShowTransitionPopup(GameType gameType, FormatType formatType, int missionId)
	{
		if (HearthstoneApplication.IsInternal())
		{
			m_nextMissionId = missionId;
			m_nextFormatType = formatType;
			string popupName = DetermineTransitionPopupForFindGame(gameType, missionId);
			if (popupName != null)
			{
				ShowTransitionPopup(popupName, missionId);
			}
		}
	}

	public void RestartGame()
	{
		FindGameInternal(m_gameType, m_formatType, m_missionId, m_brawlLibraryItemId, m_lastDeckId.GetValueOrDefault(), m_lastAIDeck, m_lastHeroCardDbId.GetValueOrDefault(), m_lastSeasonId, restoreSavedGameState: false, null, null, 0L);
	}

	public bool HasLastPlayedDeckId()
	{
		return m_lastDeckId.HasValue;
	}

	public void EnterFriendlyChallengeGameWithDecks(FormatType formatType, BrawlType brawlType, int missionId, int seasonId, int brawlLibraryItemId, BnetGameAccountId player2GameAccountId, DeckShareState player1DeckShareState, long player1DeckId, DeckShareState player2DeckShareState, long player2DeckId, long? player1RandomHeroCardDbId, long? player2RandomHeroCardDbId, long? player1CardBackId, long? player2CardBackId)
	{
		Network.Get().EnterFriendlyChallengeGame(formatType, brawlType, missionId, seasonId, brawlLibraryItemId, player2GameAccountId, player1DeckShareState, player1DeckId, player2DeckShareState, player2DeckId, null, null, player1RandomHeroCardDbId, player2RandomHeroCardDbId, player1CardBackId, player2CardBackId);
	}

	public void EnterFriendlyChallengeGameWithHeroes(FormatType formatType, BrawlType brawlType, int missionId, int seasonId, int brawlLibraryItemId, BnetGameAccountId player2GameAccountId, long player1HeroCardDbId, long player2HeroCardDbId, long? player1CardBackId, long? player2CardBackId)
	{
		Network.Get().EnterFriendlyChallengeGame(formatType, brawlType, missionId, seasonId, brawlLibraryItemId, player2GameAccountId, DeckShareState.NO_DECK_SHARE, 0L, DeckShareState.NO_DECK_SHARE, 0L, player1HeroCardDbId, player2HeroCardDbId, null, null, player1CardBackId, player2CardBackId);
	}

	public void WaitForFriendChallengeToStart(FormatType formatType, BrawlType brawlType, int missionId, int brawlLibraryItemId, PartyType partyType)
	{
		m_nextFormatType = formatType;
		m_nextMissionId = missionId;
		m_nextBrawlLibraryItemId = brawlLibraryItemId;
		m_lastEnterGameError = 0u;
		switch (partyType)
		{
		case PartyType.BATTLEGROUNDS_PARTY:
			if (!PartyManager.Get().IsInPrivateBattlegroundsParty())
			{
				m_nextGameType = GameType.GT_BATTLEGROUNDS;
				string battlegroundsMode = "solo";
				if (BattleNet.GetPartyAttribute(PartyManager.Get().GetCurrentPartyId(), "battlegrounds_mode", out battlegroundsMode))
				{
					m_nextGameType = ((battlegroundsMode == "duos") ? GameType.GT_BATTLEGROUNDS_DUO : GameType.GT_BATTLEGROUNDS);
				}
			}
			else
			{
				m_nextGameType = GameType.GT_BATTLEGROUNDS_FRIENDLY;
				string battlegroundsMode2 = "solo";
				if (BattleNet.GetPartyAttribute(PartyManager.Get().GetCurrentPartyId(), "battlegrounds_mode", out battlegroundsMode2))
				{
					m_nextGameType = ((battlegroundsMode2 == "duos") ? GameType.GT_BATTLEGROUNDS_DUO_FRIENDLY : GameType.GT_BATTLEGROUNDS_FRIENDLY);
				}
			}
			ChangeFindGameState(FindGameState.BNET_QUEUE_ENTERED);
			break;
		case PartyType.MERCENARIES_COOP_PARTY:
			m_nextGameType = GameType.GT_MERCENARIES_PVE_COOP;
			ChangeFindGameState(FindGameState.CLIENT_STARTED);
			break;
		case PartyType.MERCENARIES_FRIENDLY_CHALLENGE:
			m_nextGameType = GameType.GT_MERCENARIES_FRIENDLY;
			ChangeFindGameState(FindGameState.CLIENT_STARTED);
			break;
		default:
			m_nextGameType = GameType.GT_VS_FRIEND;
			ChangeFindGameState(FindGameState.CLIENT_STARTED);
			break;
		}
		string popupName = DetermineTransitionPopupForFindGame(m_nextGameType, missionId);
		if (popupName != null)
		{
			ShowTransitionPopup(popupName, missionId);
		}
		else
		{
			Debug.LogError("WaitForFriendChallengeToStart - No valid transition popup.");
		}
	}

	public void SpectateGame(JoinInfo joinInfo)
	{
		GameServerInfo serverInfo = new GameServerInfo();
		serverInfo.Address = joinInfo.ServerIpAddress;
		serverInfo.Port = joinInfo.ServerPort;
		serverInfo.GameHandle = (uint)joinInfo.GameHandle;
		serverInfo.SpectatorPassword = joinInfo.SecretKey;
		serverInfo.SpectatorMode = true;
		m_nextGameType = joinInfo.GameType;
		m_nextFormatType = joinInfo.FormatType;
		m_nextMissionId = joinInfo.MissionId;
		m_brawlLibraryItemId = joinInfo.BrawlLibraryItemId;
		m_nextSpectator = true;
		m_lastEnterGameError = 0u;
		ChangeFindGameState(FindGameState.CLIENT_STARTED);
		ShowTransitionPopup("LoadingPopup.prefab:ff9266f7c55faa94b9cd0f1371df7168", joinInfo.MissionId);
		ChangeFindGameState(FindGameState.SERVER_GAME_CONNECTING, serverInfo);
	}

	public void ReconnectGame(GameType gameType, FormatType formatType, GameReconnectType gameReconnectType, GameServerInfo serverInfo)
	{
		GameNetLogger.Log(Blizzard.T5.Core.LogLevel.Debug, $"GameMgr.ReconnectGame() - GameReconnectType:{gameReconnectType}");
		m_nextGameType = gameType;
		m_nextFormatType = formatType;
		m_nextMissionId = serverInfo.Mission;
		m_nextBrawlLibraryItemId = serverInfo.BrawlLibraryItemId;
		m_nextGameReconnectType = gameReconnectType;
		m_nextSpectator = serverInfo.SpectatorMode;
		m_lastEnterGameError = 0u;
		ChangeFindGameState(FindGameState.CLIENT_STARTED);
		ChangeFindGameState(FindGameState.SERVER_GAME_CONNECTING, serverInfo);
	}

	public bool CancelFindGame()
	{
		if (!GameUtils.IsMatchmadeGameType(m_nextGameType, null))
		{
			return false;
		}
		if (!Network.Get().IsFindingGame())
		{
			return false;
		}
		Network.Get().CancelFindGame();
		if (IsFindingGame())
		{
			ChangeFindGameState(FindGameState.CLIENT_CANCELED);
		}
		return true;
	}

	public void HideTransitionPopup()
	{
		if ((bool)m_transitionPopup)
		{
			m_transitionPopup.Hide();
		}
	}

	public GameEntity CreateGameEntity(List<Network.PowerHistory> powerList, Network.HistCreateGame createGame)
	{
		FlowPerformanceGame currentFlow = HearthstonePerformance.Get()?.GetCurrentPerformanceFlow<FlowPerformanceGame>();
		if (currentFlow != null)
		{
			currentFlow.GameUuid = createGame.Uuid;
		}
		GameEntity returnEntity = null;
		switch ((ScenarioDbId)m_missionId)
		{
		case ScenarioDbId.TUTORIAL_REXXAR:
			returnEntity = new Tutorial_Fight_001();
			break;
		case ScenarioDbId.TUTORIAL_GARROSH:
			returnEntity = new Tutorial_Fight_002();
			break;
		case ScenarioDbId.TUTORIAL_LICH_KING:
			returnEntity = new Tutorial_Fight_003();
			break;
		case ScenarioDbId.NAXX_ANUBREKHAN:
		case ScenarioDbId.NAXX_HEROIC_ANUBREKHAN:
			returnEntity = new NAX01_AnubRekhan();
			break;
		case ScenarioDbId.NAXX_FAERLINA:
		case ScenarioDbId.NAXX_CHALLENGE_DRUID_V_FAERLINA:
		case ScenarioDbId.NAXX_HEROIC_FAERLINA:
			returnEntity = new NAX02_Faerlina();
			break;
		case ScenarioDbId.NAXX_MAEXXNA:
		case ScenarioDbId.NAXX_CHALLENGE_ROGUE_V_MAEXXNA:
		case ScenarioDbId.NAXX_HEROIC_MAEXXNA:
			returnEntity = new NAX03_Maexxna();
			break;
		case ScenarioDbId.NAXX_NOTH:
		case ScenarioDbId.NAXX_HEROIC_NOTH:
			returnEntity = new NAX04_Noth();
			break;
		case ScenarioDbId.NAXX_HEIGAN:
		case ScenarioDbId.NAXX_CHALLENGE_MAGE_V_HEIGAN:
		case ScenarioDbId.NAXX_HEROIC_HEIGAN:
			returnEntity = new NAX05_Heigan();
			break;
		case ScenarioDbId.NAXX_LOATHEB:
		case ScenarioDbId.NAXX_CHALLENGE_HUNTER_V_LOATHEB:
		case ScenarioDbId.NAXX_HEROIC_LOATHEB:
			returnEntity = new NAX06_Loatheb();
			break;
		case ScenarioDbId.NAXX_RAZUVIOUS:
		case ScenarioDbId.NAXX_HEROIC_RAZUVIOUS:
			returnEntity = new NAX07_Razuvious();
			break;
		case ScenarioDbId.NAXX_GOTHIK:
		case ScenarioDbId.NAXX_CHALLENGE_SHAMAN_V_GOTHIK:
		case ScenarioDbId.NAXX_HEROIC_GOTHIK:
			returnEntity = new NAX08_Gothik();
			break;
		case ScenarioDbId.NAXX_HORSEMEN:
		case ScenarioDbId.NAXX_CHALLENGE_WARLOCK_V_HORSEMEN:
		case ScenarioDbId.NAXX_HEROIC_HORSEMEN:
			returnEntity = new NAX09_Horsemen();
			break;
		case ScenarioDbId.NAXX_PATCHWERK:
		case ScenarioDbId.NAXX_HEROIC_PATCHWERK:
			returnEntity = new NAX10_Patchwerk();
			break;
		case ScenarioDbId.NAXX_GROBBULUS:
		case ScenarioDbId.NAXX_CHALLENGE_WARRIOR_V_GROBBULUS:
		case ScenarioDbId.NAXX_HEROIC_GROBBULUS:
			returnEntity = new NAX11_Grobbulus();
			break;
		case ScenarioDbId.NAXX_GLUTH:
		case ScenarioDbId.NAXX_HEROIC_GLUTH:
			returnEntity = new NAX12_Gluth();
			break;
		case ScenarioDbId.NAXX_THADDIUS:
		case ScenarioDbId.NAXX_CHALLENGE_PRIEST_V_THADDIUS:
		case ScenarioDbId.NAXX_HEROIC_THADDIUS:
			returnEntity = new NAX13_Thaddius();
			break;
		case ScenarioDbId.NAXX_SAPPHIRON:
		case ScenarioDbId.NAXX_HEROIC_SAPPHIRON:
			returnEntity = new NAX14_Sapphiron();
			break;
		case ScenarioDbId.NAXX_KELTHUZAD:
		case ScenarioDbId.NAXX_CHALLENGE_PALADIN_V_KELTHUZAD:
		case ScenarioDbId.NAXX_HEROIC_KELTHUZAD:
			returnEntity = new NAX15_KelThuzad();
			break;
		case ScenarioDbId.BRM_GRIM_GUZZLER:
		case ScenarioDbId.BRM_HEROIC_GRIM_GUZZLER:
		case ScenarioDbId.BRM_CHALLENGE_HUNTER_V_GUZZLER:
			returnEntity = new BRM01_GrimGuzzler();
			break;
		case ScenarioDbId.BRM_DARK_IRON_ARENA:
		case ScenarioDbId.BRM_HEROIC_DARK_IRON_ARENA:
		case ScenarioDbId.BRM_CHALLENGE_MAGE_V_DARK_IRON_ARENA:
			returnEntity = new BRM02_DarkIronArena();
			break;
		case ScenarioDbId.BRM_THAURISSAN:
		case ScenarioDbId.BRM_HEROIC_THAURISSAN:
			returnEntity = new BRM03_Thaurissan();
			break;
		case ScenarioDbId.BRM_GARR:
		case ScenarioDbId.BRM_HEROIC_GARR:
		case ScenarioDbId.BRM_CHALLENGE_WARRIOR_V_GARR:
			returnEntity = new BRM04_Garr();
			break;
		case ScenarioDbId.BRM_BARON_GEDDON:
		case ScenarioDbId.BRM_HEROIC_BARON_GEDDON:
		case ScenarioDbId.BRM_CHALLENGE_SHAMAN_V_GEDDON:
			returnEntity = new BRM05_BaronGeddon();
			break;
		case ScenarioDbId.BRM_MAJORDOMO:
		case ScenarioDbId.BRM_HEROIC_MAJORDOMO:
			returnEntity = new BRM06_Majordomo();
			break;
		case ScenarioDbId.BRM_OMOKK:
		case ScenarioDbId.BRM_HEROIC_OMOKK:
			returnEntity = new BRM07_Omokk();
			break;
		case ScenarioDbId.BRM_DRAKKISATH:
		case ScenarioDbId.BRM_CHALLENGE_PRIEST_V_DRAKKISATH:
		case ScenarioDbId.BRM_HEROIC_DRAKKISATH:
			returnEntity = new BRM08_Drakkisath();
			break;
		case ScenarioDbId.BRM_REND_BLACKHAND:
		case ScenarioDbId.BRM_CHALLENGE_DRUID_V_BLACKHAND:
		case ScenarioDbId.BRM_HEROIC_REND_BLACKHAND:
			returnEntity = new BRM09_RendBlackhand();
			break;
		case ScenarioDbId.BRM_RAZORGORE:
		case ScenarioDbId.BRM_HEROIC_RAZORGORE:
		case ScenarioDbId.BRM_CHALLENGE_WARLOCK_V_RAZORGORE:
			returnEntity = new BRM10_Razorgore();
			break;
		case ScenarioDbId.BRM_VAELASTRASZ:
		case ScenarioDbId.BRM_HEROIC_VAELASTRASZ:
		case ScenarioDbId.BRM_CHALLENGE_ROGUE_V_VAELASTRASZ:
			returnEntity = new BRM11_Vaelastrasz();
			break;
		case ScenarioDbId.BRM_CHROMAGGUS:
		case ScenarioDbId.BRM_HEROIC_CHROMAGGUS:
			returnEntity = new BRM12_Chromaggus();
			break;
		case ScenarioDbId.BRM_NEFARIAN:
		case ScenarioDbId.BRM_HEROIC_NEFARIAN:
			returnEntity = new BRM13_Nefarian();
			break;
		case ScenarioDbId.BRM_MALORIAK:
		case ScenarioDbId.BRM_HEROIC_MALORIAK:
			returnEntity = new BRM15_Maloriak();
			break;
		case ScenarioDbId.BRM_OMNOTRON:
		case ScenarioDbId.BRM_HEROIC_OMNOTRON:
		case ScenarioDbId.BRM_CHALLENGE_PALADIN_V_OMNOTRON:
			returnEntity = new BRM14_Omnotron();
			break;
		case ScenarioDbId.BRM_ATRAMEDES:
		case ScenarioDbId.BRM_HEROIC_ATRAMEDES:
			returnEntity = new BRM16_Atramedes();
			break;
		case ScenarioDbId.BRM_ZOMBIE_NEF:
		case ScenarioDbId.BRM_HEROIC_ZOMBIE_NEF:
			returnEntity = new BRM17_ZombieNef();
			break;
		case ScenarioDbId.TB_RAG_V_NEF:
			returnEntity = new TB01_RagVsNef();
			break;
		case ScenarioDbId.TB_CO_OP_TEST:
		case ScenarioDbId.TB_CO_OP:
		case ScenarioDbId.TB_CO_OP_TEST2:
		case ScenarioDbId.TB_CO_OP_PRECON:
		case ScenarioDbId.TB_CO_OP_1P_TEST:
		case ScenarioDbId.TB_CO_OP_V2:
			returnEntity = new TB02_CoOp();
			break;
		case ScenarioDbId.TB_COOPV3_1P_TEST:
		case ScenarioDbId.TB_COOPV3:
		case ScenarioDbId.TB_COOPV3_Score_1P_TEST:
		case ScenarioDbId.TB_COOPV3_Score:
			returnEntity = new TB11_CoOpv3();
			break;
		case ScenarioDbId.TB_DECKBUILDING:
		case ScenarioDbId.TB_DECKBUILDING_1P_TEST:
			returnEntity = new TB04_DeckBuilding();
			break;
		case ScenarioDbId.TB_CHOOSEFATEBUILD_1P_TEST:
		case ScenarioDbId.TB_CHOOSEFATEBUILD:
			returnEntity = new TB_ChooseYourFateBuildaround();
			break;
		case ScenarioDbId.TB_CHOOSEFATERANDOM_1P_TEST:
		case ScenarioDbId.TB_CHOOSEFATERANDOM:
			returnEntity = new TB_ChooseYourFateRandom();
			break;
		case ScenarioDbId.TB_GIFTEXCHANGE_1P_TEST:
		case ScenarioDbId.TB_GIFTEXCHANGE:
			returnEntity = new TB05_GiftExchange();
			break;
		case ScenarioDbId.TB_SHADOWTOWERS_1P_TEST:
		case ScenarioDbId.TB_SHADOWTOWERS:
		case ScenarioDbId.TB_SHADOWTOWERS_TEST:
			returnEntity = new TB09_ShadowTowers();
			break;
		case ScenarioDbId.TB_DECKRECIPE_1P_TEST:
		case ScenarioDbId.TB_DECKRECIPE:
			returnEntity = new TB10_DeckRecipe();
			break;
		case ScenarioDbId.TB_DECKRECIPE_MSG_1P_TEST:
		case ScenarioDbId.TB_DECKRECIPE_MSG:
			returnEntity = new TB10_DeckRecipe();
			break;
		case ScenarioDbId.TB_KARAPORTALS_1P_TEST:
		case ScenarioDbId.TB_KARAPORTALS:
			returnEntity = new TB12_PartyPortals();
			break;
		case ScenarioDbId.TB_LETHALPUZZLES:
			returnEntity = new TB13_LethalPuzzles();
			break;
		case ScenarioDbId.TB_LETHALPUZZLES_RESTART:
			returnEntity = new TB13_LethalPuzzles_Restart();
			break;
		case ScenarioDbId.TB_DPROMO:
			returnEntity = new TB14_DPromo();
			break;
		case ScenarioDbId.TB_BATTLEROYALE_1P_TEST:
		case ScenarioDbId.TB_BATTLEROYALE:
			returnEntity = new TB15_BossBattleRoyale();
			break;
		case ScenarioDbId.LOE_GIANTFIN:
		case ScenarioDbId.LOE_CHALLENGE_SHAMAN_V_GIANTFIN:
		case ScenarioDbId.LOE_HEROIC_GIANTFIN:
			returnEntity = new LOE10_Giantfin();
			break;
		case ScenarioDbId.LOE_SLITHERSPEAR:
		case ScenarioDbId.LOE_CHALLENGE_HUNTER_V_SLITHERSPEAR:
		case ScenarioDbId.LOE_HEROIC_SLITHERSPEAR:
			returnEntity = new LOE09_LordSlitherspear();
			break;
		case ScenarioDbId.LOE_ZINAAR:
		case ScenarioDbId.LOE_CHALLENGE_WARRIOR_V_ZINAAR:
		case ScenarioDbId.LOE_HEROIC_ZINAAR:
			returnEntity = new LOE01_Zinaar();
			break;
		case ScenarioDbId.LOE_SUN_RAIDER_PHAERIX:
		case ScenarioDbId.LOE_CHALLENGE_WARLOCK_V_SUN_RAIDER:
		case ScenarioDbId.LOE_HEROIC_SUN_RAIDER_PHAERIX:
			returnEntity = new LOE02_Sun_Raider_Phaerix();
			break;
		case ScenarioDbId.LOE_SCARVASH:
		case ScenarioDbId.LOE_CHALLENGE_DRUID_V_SCARVASH:
		case ScenarioDbId.LOE_HEROIC_SCARVASH:
			returnEntity = new LOE04_Scarvash();
			break;
		case ScenarioDbId.LOE_ARCHAEDAS:
		case ScenarioDbId.LOE_CHALLENGE_PALADIN_V_ARCHAEDUS:
		case ScenarioDbId.LOE_HEROIC_ARCHAEDAS:
			returnEntity = new LOE08_Archaedas();
			break;
		case ScenarioDbId.LOE_STEEL_SENTINEL:
		case ScenarioDbId.LOE_CHALLENGE_MAGE_V_SENTINEL:
		case ScenarioDbId.LOE_HEROIC_STEEL_SENTINEL:
			returnEntity = new LOE14_Steel_Sentinel();
			break;
		case ScenarioDbId.LOE_TEMPLE_ESCAPE:
		case ScenarioDbId.LOE_HEROIC_TEMPLE_ESCAPE:
			returnEntity = new LOE03_AncientTemple();
			break;
		case ScenarioDbId.LOE_MINE_CART:
		case ScenarioDbId.LOE_HEROIC_MINE_CART:
			returnEntity = new LOE07_MineCart();
			break;
		case ScenarioDbId.LOE_LADY_NAZJAR:
		case ScenarioDbId.LOE_CHALLENGE_PRIEST_V_NAZJAR:
		case ScenarioDbId.LOE_HEROIC_LADY_NAZJAR:
			returnEntity = new LOE12_Naga();
			break;
		case ScenarioDbId.LOE_RAFAAM_1:
		case ScenarioDbId.LOE_HEROIC_RAFAAM_1:
			returnEntity = new LOE15_Boss1();
			break;
		case ScenarioDbId.LOE_RAFAAM_2:
		case ScenarioDbId.LOE_HEROIC_RAFAAM_2:
			returnEntity = new LOE16_Boss2();
			break;
		case ScenarioDbId.LOE_SKELESAURUS:
		case ScenarioDbId.LOE_CHALLENGE_ROGUE_V_SKELESAURUS:
		case ScenarioDbId.LOE_HEROIC_SKELESAURUS:
			returnEntity = new LOE13_Skelesaurus();
			break;
		case ScenarioDbId.TB_KELTHUZADRAFAAM:
		case ScenarioDbId.TB_KELTHUZADRAFAAM_1P:
			returnEntity = new TB_KelthuzadRafaam();
			break;
		case ScenarioDbId.KAR_PROLOGUE:
		case ScenarioDbId.KAR_HEROIC_PROLOGUE:
			returnEntity = new KAR00_Prologue();
			break;
		case ScenarioDbId.KAR_PANTRY:
		case ScenarioDbId.KAR_HEROIC_PANTRY:
		case ScenarioDbId.KAR_CHALLENGE_PRIEST_V_PANTRY:
			returnEntity = new KAR01_Pantry();
			break;
		case ScenarioDbId.KAR_MIRROR:
		case ScenarioDbId.KAR_HEROIC_MIRROR:
		case ScenarioDbId.KAR_CHALLENGE_SHAMAN_V_MIRROR:
			returnEntity = new KAR02_Mirror();
			break;
		case ScenarioDbId.KAR_CHESS:
		case ScenarioDbId.KAR_HEROIC_CHESS:
			returnEntity = new KAR03_Chess();
			break;
		case ScenarioDbId.KAR_JULIANNE:
		case ScenarioDbId.KAR_HEROIC_JULIANNE:
		case ScenarioDbId.KAR_CHALLENGE_WARLOCK_V_JULIANNE:
			returnEntity = new KAR04_Julianne();
			break;
		case ScenarioDbId.KAR_WOLF:
		case ScenarioDbId.KAR_HEROIC_WOLF:
		case ScenarioDbId.KAR_CHALLENGE_PALADIN_V_WOLF:
			returnEntity = new KAR05_Wolf();
			break;
		case ScenarioDbId.KAR_CRONE:
		case ScenarioDbId.KAR_HEROIC_CRONE:
			returnEntity = new KAR06_Crone();
			break;
		case ScenarioDbId.KAR_CURATOR:
		case ScenarioDbId.KAR_HEROIC_CURATOR:
		case ScenarioDbId.KAR_CHALLENGE_HUNTER_V_CURATOR:
			returnEntity = new KAR07_Curator();
			break;
		case ScenarioDbId.KAR_NIGHTBANE:
		case ScenarioDbId.KAR_HEROIC_NIGHTBANE:
		case ScenarioDbId.KAR_CHALLENGE_MAGE_V_NIGHTBANE:
			returnEntity = new KAR08_Nightbane();
			break;
		case ScenarioDbId.KAR_ILLHOOF:
		case ScenarioDbId.KAR_HEROIC_ILLHOOF:
		case ScenarioDbId.KAR_CHALLENGE_WARRIOR_V_ILLHOOF:
			returnEntity = new KAR09_Illhoof();
			break;
		case ScenarioDbId.KAR_ARAN:
		case ScenarioDbId.KAR_HEROIC_ARAN:
		case ScenarioDbId.KAR_CHALLENGE_ROGUE_V_ARAN:
			returnEntity = new KAR10_Aran();
			break;
		case ScenarioDbId.KAR_NETHERSPITE:
		case ScenarioDbId.KAR_HEROIC_NETHERSPITE:
		case ScenarioDbId.KAR_CHALLENGE_DRUID_V_NETHERSPITE:
			returnEntity = new KAR11_Netherspite();
			break;
		case ScenarioDbId.KAR_PORTALS:
		case ScenarioDbId.KAR_HEROIC_PORTALS:
			returnEntity = new KAR12_Portals();
			break;
		case ScenarioDbId.TB_BLIZZCON_2016_1P:
		case ScenarioDbId.TB_BLIZZCON_2016:
			returnEntity = new TB_Blizzcon_2016();
			break;
		case ScenarioDbId.TB_MAMMOTHPARTY_1P:
		case ScenarioDbId.TB_MAMMOTHPARTY:
		case ScenarioDbId.TB_MAMMOTHPARTY_ANYTIME:
			returnEntity = new TB_MammothParty();
			break;
		case ScenarioDbId.TB_MP_CROSSROADS_1P:
		case ScenarioDbId.TB_MP_CROSSROADS:
			returnEntity = new TB_MP_Crossroads();
			break;
		case ScenarioDbId.TB_MAMMOTHPARTY_STORMWIND:
			returnEntity = new TB16_MP_Stormwind();
			break;
		case ScenarioDbId.TB_100TH:
		case ScenarioDbId.TB_100TH_1P:
			returnEntity = new TB_100th();
			break;
		case ScenarioDbId.TB_FROSTFEST_1P:
		case ScenarioDbId.TB_FROSTFEST:
			returnEntity = new TB_FrostFestival();
			break;
		case ScenarioDbId.TB_FIREFEST_1P:
		case ScenarioDbId.TB_FIREFEST:
			returnEntity = new TB_FireFest();
			break;
		case ScenarioDbId.TB_JUGGERNAUT:
			returnEntity = new TB_Juggernaut();
			break;
		case ScenarioDbId.TB_LK_RAID:
			returnEntity = new TB_LichKingRaid();
			break;
		case ScenarioDbId.ICC_01_LICHKING:
			returnEntity = new ICC_01_LICHKING();
			break;
		case ScenarioDbId.ICC_03_SECRETS:
			returnEntity = new ICC_03_SECRETS();
			break;
		case ScenarioDbId.ICC_04_SINDRAGOSA:
			returnEntity = new ICC_04_Sindragosa();
			break;
		case ScenarioDbId.ICC_05_LANATHEL:
			returnEntity = new ICC_05_Lanathel();
			break;
		case ScenarioDbId.ICC_06_MARROWGAR:
			returnEntity = new ICC_06_Marrowgar();
			break;
		case ScenarioDbId.ICC_07_PUTRICIDE:
			returnEntity = new ICC_07_Putricide();
			break;
		case ScenarioDbId.ICC_08_FINALE:
			returnEntity = new ICC_08_Finale();
			break;
		case ScenarioDbId.ICC_09_SAURFANG:
			returnEntity = new ICC_09_Saurfang();
			break;
		case ScenarioDbId.ICC_10_DEATHWHISPER:
			returnEntity = new ICC_10_Deathwhisper();
			break;
		case ScenarioDbId.TB_HEADLESSHORSEMAN:
			returnEntity = new TB_HeadlessHorseman();
			break;
		case ScenarioDbId.TB_HEADLESSREDUX:
			returnEntity = new TB_HeadlessRedux();
			break;
		case ScenarioDbId.LOOT_DUNGEON:
			returnEntity = LOOT_Dungeon.InstantiateLootDungeonMissionEntityForBoss(powerList, createGame);
			break;
		case ScenarioDbId.GIL_DUNGEON:
			returnEntity = GIL_Dungeon.InstantiateGilDungeonMissionEntityForBoss(powerList, createGame);
			break;
		case ScenarioDbId.GIL_BONUS_CHALLENGE:
			returnEntity = GIL_Dungeon.InstantiateGilDungeonMissionEntityForBoss(powerList, createGame);
			break;
		case ScenarioDbId.BOTA_MIRROR_PUZZLE_1:
			returnEntity = new BOTA_Mirror_Puzzle_1();
			break;
		case ScenarioDbId.BOTA_MIRROR_PUZZLE_2:
			returnEntity = new BOTA_Mirror_Puzzle_2();
			break;
		case ScenarioDbId.BOTA_MIRROR_PUZZLE_3:
			returnEntity = new BOTA_Mirror_Puzzle_3();
			break;
		case ScenarioDbId.BOTA_MIRROR_PUZZLE_4:
			returnEntity = new BOTA_Mirror_Puzzle_4();
			break;
		case ScenarioDbId.BOTA_MIRROR_BOOM:
			returnEntity = new BOTA_Mirror_Boom();
			break;
		case ScenarioDbId.BOTA_SURVIVAL_PUZZLE_1:
			returnEntity = new BOTA_Survival_Puzzle_1();
			break;
		case ScenarioDbId.BOTA_SURVIVAL_PUZZLE_2:
			returnEntity = new BOTA_Survival_Puzzle_2();
			break;
		case ScenarioDbId.BOTA_SURVIVAL_PUZZLE_3:
			returnEntity = new BOTA_Survival_Puzzle_3();
			break;
		case ScenarioDbId.BOTA_SURVIVAL_PUZZLE_4:
			returnEntity = new BOTA_Survival_Puzzle_4();
			break;
		case ScenarioDbId.BOTA_SURVIVAL_BOOM:
			returnEntity = new BOTA_Survival_Boom();
			break;
		case ScenarioDbId.BOTA_LETHAL_PUZZLE_1:
			returnEntity = new BOTA_Lethal_Puzzle_1();
			break;
		case ScenarioDbId.BOTA_LETHAL_PUZZLE_2:
			returnEntity = new BOTA_Lethal_Puzzle_2();
			break;
		case ScenarioDbId.BOTA_LETHAL_PUZZLE_3:
			returnEntity = new BOTA_Lethal_Puzzle_3();
			break;
		case ScenarioDbId.BOTA_LETHAL_PUZZLE_4:
			returnEntity = new BOTA_Lethal_Puzzle_4();
			break;
		case ScenarioDbId.BOTA_LETHAL_BOOM:
			returnEntity = new BOTA_Lethal_Boom();
			break;
		case ScenarioDbId.BOTA_CLEAR_PUZZLE_1:
			returnEntity = new BOTA_Clear_Puzzle_1();
			break;
		case ScenarioDbId.BOTA_CLEAR_PUZZLE_2:
			returnEntity = new BOTA_Clear_Puzzle_2();
			break;
		case ScenarioDbId.BOTA_CLEAR_PUZZLE_3:
			returnEntity = new BOTA_Clear_Puzzle_3();
			break;
		case ScenarioDbId.BOTA_CLEAR_PUZZLE_4:
			returnEntity = new BOTA_Clear_Puzzle_4();
			break;
		case ScenarioDbId.BOTA_CLEAR_BOOM:
			returnEntity = new BOTA_Clear_Boom();
			break;
		case ScenarioDbId.TRL_DUNGEON:
			returnEntity = TRL_Dungeon.InstantiateTRLDungeonMissionEntityForBoss(powerList, createGame);
			break;
		case ScenarioDbId.DALA_01_BANK:
		case ScenarioDbId.DALA_02_VIOLET_HOLD:
		case ScenarioDbId.DALA_03_STREETS:
		case ScenarioDbId.DALA_04_UNDERBELLY:
		case ScenarioDbId.DALA_05_CITADEL:
		case ScenarioDbId.DALA_01_BANK_HEROIC:
		case ScenarioDbId.DALA_02_VIOLET_HOLD_HEROIC:
		case ScenarioDbId.DALA_03_STREETS_HEROIC:
		case ScenarioDbId.DALA_04_UNDERBELLY_HEROIC:
		case ScenarioDbId.DALA_05_CITADEL_HEROIC:
			returnEntity = DALA_Dungeon.InstantiateDALADungeonMissionEntityForBoss(powerList, createGame);
			break;
		case ScenarioDbId.DALA_TAVERN:
		case ScenarioDbId.DALA_TAVERN_HEROIC:
			returnEntity = new DALA_Tavern();
			break;
		case ScenarioDbId.ULDA_CITY:
		case ScenarioDbId.ULDA_DESERT:
		case ScenarioDbId.ULDA_TOMB:
		case ScenarioDbId.ULDA_HALLS:
		case ScenarioDbId.ULDA_SANCTUM:
		case ScenarioDbId.ULDA_CITY_HEROIC:
		case ScenarioDbId.ULDA_DESERT_HEROIC:
		case ScenarioDbId.ULDA_TOMB_HEROIC:
		case ScenarioDbId.ULDA_HALLS_HEROIC:
		case ScenarioDbId.ULDA_SANCTUM_HEROIC:
			returnEntity = ULDA_Dungeon.InstantiateULDADungeonMissionEntityForBoss(powerList, createGame);
			break;
		case ScenarioDbId.ULDA_TAVERN:
		case ScenarioDbId.ULDA_TAVERN_HEROIC:
			returnEntity = new ULDA_Tavern();
			break;
		case ScenarioDbId.DRGA_Good_01:
		case ScenarioDbId.DRGA_Good_02:
		case ScenarioDbId.DRGA_Good_03:
		case ScenarioDbId.DRGA_Good_04:
		case ScenarioDbId.DRGA_Good_05:
		case ScenarioDbId.DRGA_Good_06:
		case ScenarioDbId.DRGA_Good_07:
		case ScenarioDbId.DRGA_Good_08:
		case ScenarioDbId.DRGA_Good_09:
		case ScenarioDbId.DRGA_Good_10:
		case ScenarioDbId.DRGA_Good_11:
		case ScenarioDbId.DRGA_Good_12:
		case ScenarioDbId.DRGA_Evil_01:
		case ScenarioDbId.DRGA_Evil_02:
		case ScenarioDbId.DRGA_Evil_03:
		case ScenarioDbId.DRGA_Evil_04:
		case ScenarioDbId.DRGA_Evil_05:
		case ScenarioDbId.DRGA_Evil_06:
		case ScenarioDbId.DRGA_Evil_07:
		case ScenarioDbId.DRGA_Evil_08:
		case ScenarioDbId.DRGA_Evil_09:
		case ScenarioDbId.DRGA_Evil_10:
		case ScenarioDbId.DRGA_Evil_11:
		case ScenarioDbId.DRGA_Evil_12:
		case ScenarioDbId.DRGA_Good_01_Heroic:
		case ScenarioDbId.DRGA_Good_02_Heroic:
		case ScenarioDbId.DRGA_Good_03_Heroic:
		case ScenarioDbId.DRGA_Good_04_Heroic:
		case ScenarioDbId.DRGA_Good_05_Heroic:
		case ScenarioDbId.DRGA_Good_06_Heroic:
		case ScenarioDbId.DRGA_Good_07_Heroic:
		case ScenarioDbId.DRGA_Good_08_Heroic:
		case ScenarioDbId.DRGA_Good_09_Heroic:
		case ScenarioDbId.DRGA_Good_10_Heroic:
		case ScenarioDbId.DRGA_Good_11_Heroic:
		case ScenarioDbId.DRGA_Good_12_Heroic:
		case ScenarioDbId.DRGA_Evil_01_Heroic:
		case ScenarioDbId.DRGA_Evil_02_Heroic:
		case ScenarioDbId.DRGA_Evil_03_Heroic:
		case ScenarioDbId.DRGA_Evil_04_Heroic:
		case ScenarioDbId.DRGA_Evil_05_Heroic:
		case ScenarioDbId.DRGA_Evil_06_Heroic:
		case ScenarioDbId.DRGA_Evil_07_Heroic:
		case ScenarioDbId.DRGA_Evil_08_Heroic:
		case ScenarioDbId.DRGA_Evil_09_Heroic:
		case ScenarioDbId.DRGA_Evil_10_Heroic:
		case ScenarioDbId.DRGA_Evil_11_Heroic:
		case ScenarioDbId.DRGA_Evil_12_Heroic:
			returnEntity = DRGA_Dungeon.InstantiateDRGADungeonMissionEntityForBoss(powerList, createGame);
			break;
		case ScenarioDbId.BTA_01_INQUISITOR_DAKREL:
		case ScenarioDbId.BTA_02_XUR_GOTH:
		case ScenarioDbId.BTA_03_ZIXOR:
		case ScenarioDbId.BTA_04_BALTHARAK:
		case ScenarioDbId.BTA_05_KANRETHAD_PRIME:
		case ScenarioDbId.BTA_06_BURGRAK_CRUELCHAIN:
		case ScenarioDbId.BTA_07_FELSTORM_RUN:
		case ScenarioDbId.BTA_08_MOTHER_SHAHRAZ:
		case ScenarioDbId.BTA_09_SHAL_JA_OUTCAST:
		case ScenarioDbId.BTA_10_KARNUK_OUTCAST:
		case ScenarioDbId.BTA_11_JEK_HAZ:
		case ScenarioDbId.BTA_12_MAGTHERIDON_PRIME:
		case ScenarioDbId.BTA_13_GOK_AMOK:
		case ScenarioDbId.BTA_14_FLIKK:
		case ScenarioDbId.BTA_15_BADUU_CORRUPTED:
		case ScenarioDbId.BTA_16_MECHA_JARAXXUS:
		case ScenarioDbId.BTA_17_ILLIDAN_STORMRAGE:
			returnEntity = BTA_Dungeon.InstantiateBTADungeonMissionEntityForBoss(powerList, createGame);
			break;
		case ScenarioDbId.BTA_Heroic_KAZZAK:
		case ScenarioDbId.BTA_Heroic_GRUUL:
		case ScenarioDbId.BTA_Heroic_MAGTHERIDON:
		case ScenarioDbId.BTA_Heroic_SUPREMUS:
		case ScenarioDbId.BTA_Heroic_TERON_GOREFIEND:
		case ScenarioDbId.BTA_Heroic_MOTHER_SHARAZ:
		case ScenarioDbId.BTA_Heroic_LADY_VASHJ:
		case ScenarioDbId.BTA_Heroic_KAELTHAS:
		case ScenarioDbId.BTA_Heroic_ILLIDAN:
			returnEntity = BTA_Dungeon_Heroic.InstantiateBTADungeonMissionEntityForBoss(powerList, createGame);
			break;
		case ScenarioDbId.FB_ELOBRAWL:
			returnEntity = new FB_ELObrawl();
			break;
		case ScenarioDbId.TB_KOBOLDGIFTS:
			returnEntity = new TB_KoboldGifts();
			break;
		case ScenarioDbId.TB_MARIN:
			returnEntity = new TB_Marin();
			break;
		case ScenarioDbId.FB_CHAMPS:
		case ScenarioDbId.FB_CHAMPS_1P:
		case ScenarioDbId.TB_DARWIN_CHAMPS:
			returnEntity = new FB_Champs();
			break;
		case ScenarioDbId.FB_BUILDABRAWL_1P:
		case ScenarioDbId.FB_BUILDABRAWL:
			returnEntity = new FB_BuildABrawl();
			break;
		case ScenarioDbId.TB_FOXBLESSING:
		case ScenarioDbId.TB_FOXBLESSING_1P:
			returnEntity = new TB_NewYearRaven();
			break;
		case ScenarioDbId.TB_FIREFEST2_1P:
		case ScenarioDbId.TB_FIREFEST2:
			returnEntity = new TB_Firefest2();
			break;
		case ScenarioDbId.FB_DUELERSBRAWL_1P:
		case ScenarioDbId.FB_DUELERSBRAWL:
		case ScenarioDbId.FB_EXPANSIONDRAFT:
			returnEntity = new FB_DuelersBrawl();
			break;
		case ScenarioDbId.FB_TOKICOOP:
		case ScenarioDbId.FB_TOKICOOP_1P:
			returnEntity = new FB_TokiCoop();
			break;
		case ScenarioDbId.TB_TROLLSWEEK1_1P:
		case ScenarioDbId.TB_TROLLSWEEK1:
			returnEntity = new TB_TrollsWeek1();
			break;
		case ScenarioDbId.TB_HENCHMANIA_1P:
		case ScenarioDbId.TB_HENCHMANIA:
			returnEntity = new TB_Henchmania();
			break;
		case ScenarioDbId.TB_IGNOBLEGARDEN:
		case ScenarioDbId.TB_IGNOBLEGARDEN_1P:
			returnEntity = new TB_Ignoblegarden();
			break;
		case ScenarioDbId.TB_207TH:
		case ScenarioDbId.TB_207TH_1P:
			returnEntity = new TB_207();
			break;
		case ScenarioDbId.TB_AUTOBRAWL_1P:
		case ScenarioDbId.TB_AUTOBRAWL:
			returnEntity = new TB_AutoBrawl();
			break;
		case ScenarioDbId.TB_FIREFEST3_1P:
		case ScenarioDbId.TB_FIREFEST3:
			returnEntity = new TB_Firefest3();
			break;
		case ScenarioDbId.TB_RANDOM_DECK_KEEP_WINNER:
		case ScenarioDbId.TB_SEEDED_BRAWL:
		case ScenarioDbId.TB_CRAZY_DECK_KEEP_WINNER:
		case ScenarioDbId.TB_DUELS_DECK_KEEP_WINNER:
			returnEntity = new TB_RandomDeckKeepWinnerDeck();
			break;
		case ScenarioDbId.TB_EVILBRM_1:
		case ScenarioDbId.TB_EVILBRM_2:
		case ScenarioDbId.TB_EVILBRM_DEBUG:
			returnEntity = new TB_EVILBRM();
			break;
		case ScenarioDbId.FB_RAGRAID:
			returnEntity = new FB_RagRaidScript();
			break;
		case ScenarioDbId.TB_BACON_1P:
		case ScenarioDbId.TB_BACONSHOP_8P:
		case ScenarioDbId.TB_BACONSHOP_VS_AI:
			returnEntity = new TB_BaconShop();
			break;
		case ScenarioDbId.TB_BACONSHOP_DUOS_VS_AI:
		case ScenarioDbId.TB_BACONSHOP_DUOS:
		case ScenarioDbId.TB_BACONSHOP_DUOS_1_PLAYER_VS_AI:
			returnEntity = new TB_BaconShopDuos();
			break;
		case ScenarioDbId.TB_BACONSHOP_Tutorial:
			returnEntity = new TB_BaconShop_Tutorial();
			break;
		case ScenarioDbId.TB_BACONHAND_1P:
			returnEntity = new TB_BaconHand();
			break;
		case ScenarioDbId.TB_MARTINAUTOBRAWL:
			returnEntity = new TB_MartinAutoBrawl();
			break;
		case ScenarioDbId.TB_ARCHIVIST_1P:
		case ScenarioDbId.TB_ARCHIVIST:
			returnEntity = new TB_NoMulligan();
			break;
		case ScenarioDbId.TB_DRAWNDISOVERY:
			returnEntity = new TB_DrawnDiscovery();
			break;
		case ScenarioDbId.TB_LEAGUE_REVIVAL:
			returnEntity = new TB_LEAGUE_REVIVAL();
			break;
		case ScenarioDbId.TB_CAROUSEL_1P:
		case ScenarioDbId.TB_CAROUSEL:
			returnEntity = new TB_Carousel();
			break;
		case ScenarioDbId.TB_TEMPLEOUTRUN_1:
		case ScenarioDbId.TB_TEMPLEOUTRUN_2:
			returnEntity = ULDA_Dungeon.InstantiateULDADungeonMissionEntityForBoss(powerList, createGame);
			break;
		case ScenarioDbId.TB_ROAD_TO_NR1:
		case ScenarioDbId.TB_ROAD_TO_NR2:
		case ScenarioDbId.TB_ROAD_TO_NR3:
		case ScenarioDbId.TB_ROAD_TO_NR4:
		case ScenarioDbId.TB_ROAD_TO_NR5:
		case ScenarioDbId.TB_ROAD_TO_NR6:
		case ScenarioDbId.TB_ROAD_TO_NR7:
		case ScenarioDbId.TB_ROAD_TO_NR8:
			returnEntity = new TB_RoadToNR();
			break;
		case ScenarioDbId.TB_ROAD_TO_NR_TAVERN:
			returnEntity = new TB_RoadToNR_Tavern();
			break;
		case ScenarioDbId.TB_SPT_DALA_1P:
		case ScenarioDbId.TB_SPT_DALA:
			returnEntity = new TB_SPT_DALA();
			break;
		case ScenarioDbId.TB_MagicalGuardians_Fight_001:
			returnEntity = new TB_MagicalGuardians_Fight_001();
			break;
		case ScenarioDbId.ReturningPlayer_Challenge_1:
			returnEntity = new RP_Fight_01();
			break;
		case ScenarioDbId.ReturningPlayer_Challenge_2:
			returnEntity = new RP_Fight_02();
			break;
		case ScenarioDbId.ReturningPlayer_Challenge_3:
			returnEntity = new RP_Fight_03();
			break;
		case ScenarioDbId.BTP_01_AZZINOTH:
			returnEntity = new BTA_Prologue_Fight_01();
			break;
		case ScenarioDbId.BTP_02_XAVIUS:
			returnEntity = new BTA_Prologue_Fight_02();
			break;
		case ScenarioDbId.BTP_03_MANNOROTH:
			returnEntity = new BTA_Prologue_Fight_03();
			break;
		case ScenarioDbId.BTP_04_CENARIUS:
			returnEntity = new BTA_Prologue_Fight_04();
			break;
		case ScenarioDbId.TB_RumbleDome:
		case ScenarioDbId.TB_Rumbledome_1p:
			returnEntity = new TB_RumbleDome();
			break;
		case ScenarioDbId.BOH_JAINA_01:
			returnEntity = new BoH_Jaina_01();
			break;
		case ScenarioDbId.BOH_JAINA_02:
			returnEntity = new BoH_Jaina_02();
			break;
		case ScenarioDbId.BOH_JAINA_03:
			returnEntity = new BoH_Jaina_03();
			break;
		case ScenarioDbId.BOH_JAINA_04:
			returnEntity = new BoH_Jaina_04();
			break;
		case ScenarioDbId.BOH_JAINA_05:
			returnEntity = new BoH_Jaina_05();
			break;
		case ScenarioDbId.BOH_JAINA_06:
			returnEntity = new BoH_Jaina_06();
			break;
		case ScenarioDbId.BOH_JAINA_07:
			returnEntity = new BoH_Jaina_07();
			break;
		case ScenarioDbId.BOH_JAINA_08:
			returnEntity = new BoH_Jaina_08();
			break;
		case ScenarioDbId.BOH_REXXAR_01:
			returnEntity = new BoH_Rexxar_01();
			break;
		case ScenarioDbId.BOH_REXXAR_02:
			returnEntity = new BoH_Rexxar_02();
			break;
		case ScenarioDbId.BOH_REXXAR_03:
			returnEntity = new BoH_Rexxar_03();
			break;
		case ScenarioDbId.BOH_REXXAR_04:
			returnEntity = new BoH_Rexxar_04();
			break;
		case ScenarioDbId.BOH_REXXAR_05:
			returnEntity = new BoH_Rexxar_05();
			break;
		case ScenarioDbId.BOH_REXXAR_06:
			returnEntity = new BoH_Rexxar_06();
			break;
		case ScenarioDbId.BOH_REXXAR_07:
			returnEntity = new BoH_Rexxar_07();
			break;
		case ScenarioDbId.BOH_REXXAR_08:
			returnEntity = new BoH_Rexxar_08();
			break;
		case ScenarioDbId.BOH_GARROSH_01:
			returnEntity = new BoH_Garrosh_01();
			break;
		case ScenarioDbId.BOH_GARROSH_02:
			returnEntity = new BoH_Garrosh_02();
			break;
		case ScenarioDbId.BOH_GARROSH_03:
			returnEntity = new BoH_Garrosh_03();
			break;
		case ScenarioDbId.BOH_GARROSH_04:
			returnEntity = new BoH_Garrosh_04();
			break;
		case ScenarioDbId.BOH_GARROSH_05:
			returnEntity = new BoH_Garrosh_05();
			break;
		case ScenarioDbId.BOH_GARROSH_06:
			returnEntity = new BoH_Garrosh_06();
			break;
		case ScenarioDbId.BOH_GARROSH_07:
			returnEntity = new BoH_Garrosh_07();
			break;
		case ScenarioDbId.BOH_GARROSH_08:
			returnEntity = new BoH_Garrosh_08();
			break;
		case ScenarioDbId.BOH_UTHER_01:
			returnEntity = new BoH_Uther_01();
			break;
		case ScenarioDbId.BOH_UTHER_02:
			returnEntity = new BoH_Uther_02();
			break;
		case ScenarioDbId.BOH_UTHER_03:
			returnEntity = new BoH_Uther_03();
			break;
		case ScenarioDbId.BOH_UTHER_04:
			returnEntity = new BoH_Uther_04();
			break;
		case ScenarioDbId.BOH_UTHER_05:
			returnEntity = new BoH_Uther_05();
			break;
		case ScenarioDbId.BOH_UTHER_06:
			returnEntity = new BoH_Uther_06();
			break;
		case ScenarioDbId.BOH_UTHER_07:
			returnEntity = new BoH_Uther_07();
			break;
		case ScenarioDbId.BOH_UTHER_08:
			returnEntity = new BoH_Uther_08();
			break;
		case ScenarioDbId.PVPDR_Season_1:
			returnEntity = new WizardDuels();
			break;
		case ScenarioDbId.BOH_ANDUIN_01:
			returnEntity = new BoH_Anduin_01();
			break;
		case ScenarioDbId.BOH_ANDUIN_02:
			returnEntity = new BoH_Anduin_02();
			break;
		case ScenarioDbId.BOH_ANDUIN_03:
			returnEntity = new BoH_Anduin_03();
			break;
		case ScenarioDbId.BOH_ANDUIN_04:
			returnEntity = new BoH_Anduin_04();
			break;
		case ScenarioDbId.BOH_ANDUIN_05:
			returnEntity = new BoH_Anduin_05();
			break;
		case ScenarioDbId.BOH_ANDUIN_06:
			returnEntity = new BoH_Anduin_06();
			break;
		case ScenarioDbId.BOH_ANDUIN_07:
			returnEntity = new BoH_Anduin_07();
			break;
		case ScenarioDbId.BOH_ANDUIN_08:
			returnEntity = new BoH_Anduin_08();
			break;
		case ScenarioDbId.BOH_VALEERA_01:
			returnEntity = new BoH_Valeera_01();
			break;
		case ScenarioDbId.BOH_VALEERA_02:
			returnEntity = new BoH_Valeera_02();
			break;
		case ScenarioDbId.BOH_VALEERA_03:
			returnEntity = new BoH_Valeera_03();
			break;
		case ScenarioDbId.BOH_VALEERA_04:
			returnEntity = new BoH_Valeera_04();
			break;
		case ScenarioDbId.BOH_VALEERA_05:
			returnEntity = new BoH_Valeera_05();
			break;
		case ScenarioDbId.BOH_VALEERA_06:
			returnEntity = new BoH_Valeera_06();
			break;
		case ScenarioDbId.BOH_VALEERA_07:
			returnEntity = new BoH_Valeera_07();
			break;
		case ScenarioDbId.BOH_VALEERA_08:
			returnEntity = new BoH_Valeera_08();
			break;
		case ScenarioDbId.BOM_01_Rokara_01:
			returnEntity = new BOM_01_Rokara_01();
			break;
		case ScenarioDbId.BOM_01_Rokara_02:
			returnEntity = new BOM_01_Rokara_02();
			break;
		case ScenarioDbId.BOM_01_Rokara_03:
			returnEntity = new BOM_01_Rokara_03();
			break;
		case ScenarioDbId.BOM_01_Rokara_04:
			returnEntity = new BOM_01_Rokara_04();
			break;
		case ScenarioDbId.BOM_01_Rokara_05:
			returnEntity = new BOM_01_Rokara_05();
			break;
		case ScenarioDbId.BOM_01_Rokara_06:
			returnEntity = new BOM_01_Rokara_06();
			break;
		case ScenarioDbId.BOM_01_Rokara_07:
			returnEntity = new BOM_01_Rokara_07();
			break;
		case ScenarioDbId.BOM_01_Rokara_08:
			returnEntity = new BOM_01_Rokara_08();
			break;
		case ScenarioDbId.BOH_THRALL_01:
			returnEntity = new BoH_Thrall_01();
			break;
		case ScenarioDbId.BOH_THRALL_02:
			returnEntity = new BoH_Thrall_02();
			break;
		case ScenarioDbId.BOH_THRALL_03:
			returnEntity = new BoH_Thrall_03();
			break;
		case ScenarioDbId.BOH_THRALL_04:
			returnEntity = new BoH_Thrall_04();
			break;
		case ScenarioDbId.BOH_THRALL_05:
			returnEntity = new BoH_Thrall_05();
			break;
		case ScenarioDbId.BOH_THRALL_06:
			returnEntity = new BoH_Thrall_06();
			break;
		case ScenarioDbId.BOH_THRALL_07:
			returnEntity = new BoH_Thrall_07();
			break;
		case ScenarioDbId.BOH_THRALL_08:
			returnEntity = new BoH_Thrall_08();
			break;
		case ScenarioDbId.BOM_02_Xyrella_01:
			returnEntity = new BOM_02_Xyrella_Fight_01();
			break;
		case ScenarioDbId.BOM_02_Xyrella_02:
			returnEntity = new BOM_02_Xyrella_Fight_02();
			break;
		case ScenarioDbId.BOM_02_Xyrella_03:
			returnEntity = new BOM_02_Xyrella_Fight_03();
			break;
		case ScenarioDbId.BOM_02_Xyrella_04:
			returnEntity = new BOM_02_Xyrella_Fight_04();
			break;
		case ScenarioDbId.BOM_02_Xyrella_05:
			returnEntity = new BOM_02_Xyrella_Fight_05();
			break;
		case ScenarioDbId.BOM_02_Xyrella_06:
			returnEntity = new BOM_02_Xyrella_Fight_06();
			break;
		case ScenarioDbId.BOM_02_Xyrella_07:
			returnEntity = new BOM_02_Xyrella_Fight_07();
			break;
		case ScenarioDbId.BOM_02_Xyrella_08:
			returnEntity = new BOM_02_Xyrella_Fight_08();
			break;
		case ScenarioDbId.BOM_03_Guff_01:
			returnEntity = new BOM_03_Guff_Fight_01();
			break;
		case ScenarioDbId.BOM_03_Guff_02:
			returnEntity = new BOM_03_Guff_Fight_02();
			break;
		case ScenarioDbId.BOM_03_Guff_03:
			returnEntity = new BOM_03_Guff_Fight_03();
			break;
		case ScenarioDbId.BOM_03_Guff_04:
			returnEntity = new BOM_03_Guff_Fight_04();
			break;
		case ScenarioDbId.BOM_03_Guff_05:
			returnEntity = new BOM_03_Guff_Fight_05();
			break;
		case ScenarioDbId.BOM_03_Guff_06:
			returnEntity = new BOM_03_Guff_Fight_06();
			break;
		case ScenarioDbId.BOM_03_Guff_07:
			returnEntity = new BOM_03_Guff_Fight_07();
			break;
		case ScenarioDbId.BOM_03_Guff_08:
			returnEntity = new BOM_03_Guff_Fight_08();
			break;
		case ScenarioDbId.BOM_04_Kurtrus_01:
			returnEntity = new BOM_04_Kurtrus_Fight_01();
			break;
		case ScenarioDbId.BOM_04_Kurtrus_02:
			returnEntity = new BOM_04_Kurtrus_Fight_02();
			break;
		case ScenarioDbId.BOM_04_Kurtrus_03:
			returnEntity = new BOM_04_Kurtrus_Fight_03();
			break;
		case ScenarioDbId.BOM_04_Kurtrus_04:
			returnEntity = new BOM_04_Kurtrus_Fight_04();
			break;
		case ScenarioDbId.BOM_04_Kurtrus_05:
			returnEntity = new BOM_04_Kurtrus_Fight_05();
			break;
		case ScenarioDbId.BOM_04_Kurtrus_06:
			returnEntity = new BOM_04_Kurtrus_Fight_06();
			break;
		case ScenarioDbId.BOM_04_Kurtrus_07:
			returnEntity = new BOM_04_Kurtrus_Fight_07();
			break;
		case ScenarioDbId.BOM_04_Kurtrus_08:
			returnEntity = new BOM_04_Kurtrus_Fight_08();
			break;
		case ScenarioDbId.BOH_MALFURION_01:
			returnEntity = new BoH_Malfurion_01();
			break;
		case ScenarioDbId.BOH_MALFURION_02:
			returnEntity = new BoH_Malfurion_02();
			break;
		case ScenarioDbId.BOH_MALFURION_03:
			returnEntity = new BoH_Malfurion_03();
			break;
		case ScenarioDbId.BOH_MALFURION_04:
			returnEntity = new BoH_Malfurion_04();
			break;
		case ScenarioDbId.BOH_MALFURION_05:
			returnEntity = new BoH_Malfurion_05();
			break;
		case ScenarioDbId.BOH_MALFURION_06:
			returnEntity = new BoH_Malfurion_06();
			break;
		case ScenarioDbId.BOH_MALFURION_07:
			returnEntity = new BoH_Malfurion_07();
			break;
		case ScenarioDbId.BOH_MALFURION_08:
			returnEntity = new BoH_Malfurion_08();
			break;
		case ScenarioDbId.BOH_GULDAN_01:
			returnEntity = new BoH_Guldan_01();
			break;
		case ScenarioDbId.BOH_GULDAN_02:
			returnEntity = new BoH_Guldan_02();
			break;
		case ScenarioDbId.BOH_GULDAN_03:
			returnEntity = new BoH_Guldan_03();
			break;
		case ScenarioDbId.BOH_GULDAN_04:
			returnEntity = new BoH_Guldan_04();
			break;
		case ScenarioDbId.BOH_GULDAN_05:
			returnEntity = new BoH_Guldan_05();
			break;
		case ScenarioDbId.BOH_GULDAN_06:
			returnEntity = new BoH_Guldan_06();
			break;
		case ScenarioDbId.BOH_GULDAN_07:
			returnEntity = new BoH_Guldan_07();
			break;
		case ScenarioDbId.BOH_GULDAN_08:
			returnEntity = new BoH_Guldan_08();
			break;
		case ScenarioDbId.BOH_ILLIDAN_01:
			returnEntity = new BoH_Illidan_01();
			break;
		case ScenarioDbId.BOH_ILLIDAN_02:
			returnEntity = new BoH_Illidan_02();
			break;
		case ScenarioDbId.BOH_ILLIDAN_03:
			returnEntity = new BoH_Illidan_03();
			break;
		case ScenarioDbId.BOH_ILLIDAN_04:
			returnEntity = new BoH_Illidan_04();
			break;
		case ScenarioDbId.BOH_ILLIDAN_05:
			returnEntity = new BoH_Illidan_05();
			break;
		case ScenarioDbId.BOH_ILLIDAN_06:
			returnEntity = new BoH_Illidan_06();
			break;
		case ScenarioDbId.BOH_ILLIDAN_07:
			returnEntity = new BoH_Illidan_07();
			break;
		case ScenarioDbId.BOH_ILLIDAN_08:
			returnEntity = new BoH_Illidan_08();
			break;
		case ScenarioDbId.BOM_05_Tamsin_001:
			returnEntity = new BOM_05_Tamsin_Fight_001();
			break;
		case ScenarioDbId.BOM_05_Tamsin_002:
			returnEntity = new BOM_05_Tamsin_Fight_002();
			break;
		case ScenarioDbId.BOM_05_Tamsin_003:
			returnEntity = new BOM_05_Tamsin_Fight_003();
			break;
		case ScenarioDbId.BOM_05_Tamsin_004:
			returnEntity = new BOM_05_Tamsin_Fight_004();
			break;
		case ScenarioDbId.BOM_05_Tamsin_005:
			returnEntity = new BOM_05_Tamsin_Fight_005();
			break;
		case ScenarioDbId.BOM_05_Tamsin_006:
			returnEntity = new BOM_05_Tamsin_Fight_006();
			break;
		case ScenarioDbId.BOM_05_Tamsin_007:
			returnEntity = new BOM_05_Tamsin_Fight_007();
			break;
		case ScenarioDbId.BOM_05_Tamsin_008:
			returnEntity = new BOM_05_Tamsin_Fight_008();
			break;
		case ScenarioDbId.LETTUCE_1v1:
		case ScenarioDbId.LETTUCE_PVP_VS_AI:
			returnEntity = new LettucePvPMissionEntity();
			break;
		case ScenarioDbId.LETTUCE_MAP:
		case ScenarioDbId.LETTUCE_MAP_COOP:
			returnEntity = InstantiateLettuceBountyMissionEntityForBoss(powerList);
			break;
		case ScenarioDbId.LETTUCE_DEV_TEST_VS_AI:
		case ScenarioDbId.LETTUCE_DEV_TEST_COOP_VS_AI:
			returnEntity = new LettucePvEMissionEntity(skipTutorial: true);
			break;
		case ScenarioDbId.LETTUCE_TAVERN:
			returnEntity = new LettuceTavernMissionEntity();
			break;
		case ScenarioDbId.LETTUCE_PVE_TUTORIAL_1:
			returnEntity = new LettuceTutorialOneMissionEntity();
			break;
		case ScenarioDbId.LETTUCE_PVE_TUTORIAL_2:
			returnEntity = new LettuceTutorialTwoMissionEntity();
			break;
		case ScenarioDbId.LETTUCE_PVE_TUTORIAL_3:
			returnEntity = new LettuceTutorialThreeMissionEntity();
			break;
		case ScenarioDbId.LETTUCE_PVE_TUTORIAL_4:
			returnEntity = new LettuceTutorialFourMissionEntity();
			break;
		case ScenarioDbId.LETTUCE_PVE_TUTORIAL_BOSS:
			returnEntity = new LettuceTutorialBossMissionEntity();
			break;
		case ScenarioDbId.BOM_06_Cariel_001:
			returnEntity = new BOM_06_Cariel_Fight_001();
			break;
		case ScenarioDbId.BOM_06_Cariel_002:
			returnEntity = new BOM_06_Cariel_Fight_002();
			break;
		case ScenarioDbId.BOM_06_Cariel_003:
			returnEntity = new BOM_06_Cariel_Fight_003();
			break;
		case ScenarioDbId.BOM_06_Cariel_004:
			returnEntity = new BOM_06_Cariel_Fight_004();
			break;
		case ScenarioDbId.BOM_06_Cariel_005:
			returnEntity = new BOM_06_Cariel_Fight_005();
			break;
		case ScenarioDbId.BOM_06_Cariel_006:
			returnEntity = new BOM_06_Cariel_Fight_006();
			break;
		case ScenarioDbId.BOM_06_Cariel_007:
			returnEntity = new BOM_06_Cariel_Fight_007();
			break;
		case ScenarioDbId.BOM_06_Cariel_008:
			returnEntity = new BOM_06_Cariel_Fight_008();
			break;
		case ScenarioDbId.BOM_07_Scabbs_Fight_001:
			returnEntity = new BOM_07_Scabbs_Fight_001();
			break;
		case ScenarioDbId.BOM_07_Scabbs_Fight_002:
			returnEntity = new BOM_07_Scabbs_Fight_002();
			break;
		case ScenarioDbId.BOM_07_Scabbs_Fight_003:
			returnEntity = new BOM_07_Scabbs_Fight_003();
			break;
		case ScenarioDbId.BOM_07_Scabbs_Fight_004:
			returnEntity = new BOM_07_Scabbs_Fight_004();
			break;
		case ScenarioDbId.BOM_07_Scabbs_Fight_005:
			returnEntity = new BOM_07_Scabbs_Fight_005();
			break;
		case ScenarioDbId.BOM_07_Scabbs_Fight_006:
			returnEntity = new BOM_07_Scabbs_Fight_006();
			break;
		case ScenarioDbId.BOM_07_Scabbs_Fight_007:
			returnEntity = new BOM_07_Scabbs_Fight_007();
			break;
		case ScenarioDbId.BOM_07_Scabbs_Fight_008:
			returnEntity = new BOM_07_Scabbs_Fight_008();
			break;
		case ScenarioDbId.TB_01_BOOKOFMERCS:
			returnEntity = new TB_01_BOM_Mercs_Fight_001();
			break;
		case ScenarioDbId.BOM_08_Tavish_Fight_001:
			returnEntity = new BOM_08_Tavish_Fight_001();
			break;
		case ScenarioDbId.BOM_08_Tavish_Fight_002:
			returnEntity = new BOM_08_Tavish_Fight_002();
			break;
		case ScenarioDbId.BOM_08_Tavish_Fight_003:
			returnEntity = new BOM_08_Tavish_Fight_003();
			break;
		case ScenarioDbId.BOM_08_Tavish_Fight_004:
			returnEntity = new BOM_08_Tavish_Fight_004();
			break;
		case ScenarioDbId.BOM_08_Tavish_Fight_005:
			returnEntity = new BOM_08_Tavish_Fight_005();
			break;
		case ScenarioDbId.BOM_08_Tavish_Fight_006:
			returnEntity = new BOM_08_Tavish_Fight_006();
			break;
		case ScenarioDbId.BOM_08_Tavish_Fight_007:
			returnEntity = new BOM_08_Tavish_Fight_007();
			break;
		case ScenarioDbId.BOM_08_Tavish_Fight_008:
			returnEntity = new BOM_08_Tavish_Fight_008();
			break;
		case ScenarioDbId.BOM_09_Brukan_Fight_001:
			returnEntity = new BOM_09_Brukan_Fight_001();
			break;
		case ScenarioDbId.BOM_09_Brukan_Fight_002:
			returnEntity = new BOM_09_Brukan_Fight_002();
			break;
		case ScenarioDbId.BOM_09_Brukan_Fight_003:
			returnEntity = new BOM_09_Brukan_Fight_003();
			break;
		case ScenarioDbId.BOM_09_Brukan_Fight_004:
			returnEntity = new BOM_09_Brukan_Fight_004();
			break;
		case ScenarioDbId.BOM_09_Brukan_Fight_005:
			returnEntity = new BOM_09_Brukan_Fight_005();
			break;
		case ScenarioDbId.BOM_09_Brukan_Fight_006:
			returnEntity = new BOM_09_Brukan_Fight_006();
			break;
		case ScenarioDbId.BOM_09_Brukan_Fight_007:
			returnEntity = new BOM_09_Brukan_Fight_007();
			break;
		case ScenarioDbId.BOM_09_Brukan_Fight_008:
			returnEntity = new BOM_09_Brukan_Fight_008();
			break;
		case ScenarioDbId.BOM_10_Dawngrasp_Fight_001:
			returnEntity = new BOM_10_Dawngrasp_Fight_001();
			break;
		case ScenarioDbId.BOM_10_Dawngrasp_Fight_002:
			returnEntity = new BOM_10_Dawngrasp_Fight_002();
			break;
		case ScenarioDbId.BOM_10_Dawngrasp_Fight_003:
			returnEntity = new BOM_10_Dawngrasp_Fight_003();
			break;
		case ScenarioDbId.BOM_10_Dawngrasp_Fight_004:
			returnEntity = new BOM_10_Dawngrasp_Fight_004();
			break;
		case ScenarioDbId.BOM_10_Dawngrasp_Fight_005:
			returnEntity = new BOM_10_Dawngrasp_Fight_005();
			break;
		case ScenarioDbId.BOM_10_Dawngrasp_Fight_006:
			returnEntity = new BOM_10_Dawngrasp_Fight_006();
			break;
		case ScenarioDbId.BOM_10_Dawngrasp_Fight_007:
			returnEntity = new BOM_10_Dawngrasp_Fight_007();
			break;
		case ScenarioDbId.BOM_10_Dawngrasp_Fight_008:
			returnEntity = new BOM_10_Dawngrasp_Fight_008();
			break;
		case ScenarioDbId.BOH_FAELIN_STORY_PROLOGUE2:
			returnEntity = new BoH_Faelin_Story_Prologue2();
			break;
		case ScenarioDbId.BOH_FAELIN_01:
			returnEntity = new BoH_Faelin_01();
			break;
		case ScenarioDbId.BOH_FAELIN_02:
			returnEntity = new BoH_Faelin_02();
			break;
		case ScenarioDbId.BOH_FAELIN_03:
			returnEntity = new BoH_Faelin_03();
			break;
		case ScenarioDbId.BOH_FAELIN_04:
			returnEntity = new BoH_Faelin_04();
			break;
		case ScenarioDbId.BOH_FAELIN_05A:
			returnEntity = new BoH_Faelin_05A();
			break;
		case ScenarioDbId.BOH_FAELIN_05B:
			returnEntity = new BoH_Faelin_05B();
			break;
		case ScenarioDbId.BOH_FAELIN_06:
			returnEntity = new BoH_Faelin_06();
			break;
		case ScenarioDbId.BOH_FAELIN_07:
			returnEntity = new BoH_Faelin_07();
			break;
		case ScenarioDbId.BOH_FAELIN_08:
			returnEntity = new BoH_Faelin_08();
			break;
		case ScenarioDbId.BOH_FAELIN_09A:
			returnEntity = new BoH_Faelin_09A();
			break;
		case ScenarioDbId.BOH_FAELIN_09B:
			returnEntity = new BoH_Faelin_09B();
			break;
		case ScenarioDbId.BOH_FAELIN_10A:
			returnEntity = new BoH_Faelin_10A();
			break;
		case ScenarioDbId.BOH_FAELIN_10B:
			returnEntity = new BoH_Faelin_10B();
			break;
		case ScenarioDbId.BOH_FAELIN_11:
			returnEntity = new BoH_Faelin_11();
			break;
		case ScenarioDbId.BOH_FAELIN_12:
			returnEntity = new BoH_Faelin_12();
			break;
		case ScenarioDbId.BOH_FAELIN_13:
			returnEntity = new BoH_Faelin_13();
			break;
		case ScenarioDbId.BOH_FAELIN_14:
			returnEntity = new BoH_Faelin_14();
			break;
		case ScenarioDbId.BOH_FAELIN_15:
			returnEntity = new BoH_Faelin_15();
			break;
		case ScenarioDbId.BOH_FAELIN_16:
			returnEntity = new BoH_Faelin_16();
			break;
		case ScenarioDbId.RLK_PROLOGUE_01:
			returnEntity = new RLK_Prologue_Fight_001();
			break;
		case ScenarioDbId.RLK_PROLOGUE_02:
			returnEntity = new RLK_Prologue_Fight_002();
			break;
		case ScenarioDbId.RLK_PROLOGUE_03:
			returnEntity = new RLK_Prologue_Fight_003();
			break;
		case ScenarioDbId.RLK_PROLOGUE_04:
			returnEntity = new RLK_Prologue_Fight_004();
			break;
		case ScenarioDbId.TB_BotB_Mukla:
			returnEntity = new TB_BattleoftheBands_Mukla();
			break;
		case ScenarioDbId.TB_BotB_ZokFogsnout:
			returnEntity = new TB_BattleoftheBands_Zok();
			break;
		case ScenarioDbId.TB_BotB_Inzah:
			returnEntity = new TB_BattleoftheBands_Inzah();
			break;
		case ScenarioDbId.TB_BotB_Manastorm:
			returnEntity = new TB_BattleoftheBands_Manastorm();
			break;
		case ScenarioDbId.TB_BotB_Hedanis:
			returnEntity = new TB_BattleoftheBands_Hedanis();
			break;
		case ScenarioDbId.TB_BotB_Blingtron:
			returnEntity = new TB_BattleoftheBands_Blingtron();
			break;
		case ScenarioDbId.TB_BotB_Kangor:
			returnEntity = new TB_BattleoftheBands_Kangor();
			break;
		case ScenarioDbId.TB_BotB_Rin:
			returnEntity = new TB_BattleoftheBands_Rin();
			break;
		case ScenarioDbId.TB_BotB_CageHead:
			returnEntity = new TB_BattleoftheBands_CageHead();
			break;
		case ScenarioDbId.TB_BotB_Voone:
			returnEntity = new TB_BattleoftheBands_Voone();
			break;
		case ScenarioDbId.TB_BotB_Halveria:
			returnEntity = new TB_BattleoftheBands_Halveria();
			break;
		case ScenarioDbId.TB_BotB_ETC:
			returnEntity = new TB_BattleoftheBands_ETC();
			break;
		case ScenarioDbId.HM_Puzzle_01:
			returnEntity = new HM_Puzzle_01();
			break;
		case ScenarioDbId.HM_Puzzle_02:
			returnEntity = new HM_Puzzle_02();
			break;
		case ScenarioDbId.HM_Puzzle_04:
			returnEntity = new HM_Puzzle_04();
			break;
		default:
			returnEntity = new StandardGameEntity();
			break;
		}
		returnEntity.OnCreateGame();
		return returnEntity;
	}

	private static LettucePvEMissionEntity InstantiateLettuceBountyMissionEntityForBoss(List<Network.PowerHistory> powerList)
	{
		return GetBossDesignCode(powerList) switch
		{
			"LETL_815H" => new LettucePvEMissionEntity(skipTutorial: false, new LettuceBoss_LETL_815H_VoHandler()), 
			"LETL_816H" => new LettucePvEMissionEntity(skipTutorial: false, new LettuceBoss_LETL_816H_VoHandler()), 
			"LETL_817H" => new LettucePvEMissionEntity(skipTutorial: false, new LettuceBoss_LETL_817H_VoHandler()), 
			"LETL_818H" => new LettucePvEMissionEntity(skipTutorial: false, new LettuceBoss_LETL_818H_VoHandler()), 
			"LETL_819H" => new LettucePvEMissionEntity(skipTutorial: false, new LettuceBoss_LETL_819H_VoHandler()), 
			"LETL_820H" => new LettucePvEMissionEntity(skipTutorial: false, new LettuceBoss_LETL_820H_VoHandler()), 
			"LETL_821H" => new LettucePvEMissionEntity(skipTutorial: false, new LettuceBoss_LETL_821H_VoHandler()), 
			"LETL_822H" => new LettucePvEMissionEntity(skipTutorial: false, new LettuceBoss_LETL_822H_VoHandler()), 
			"LETL_823H" => new LettucePvEMissionEntity(skipTutorial: false, new LettuceBoss_LETL_823H_VoHandler()), 
			"LETL_824H" => new LettucePvEMissionEntity(skipTutorial: false, new LettuceBoss_LETL_824H_VoHandler()), 
			"LETL_825H" => new LettucePvEMissionEntity(skipTutorial: false, new LettuceBoss_LETL_825H_VoHandler()), 
			"LETL_826H" => new LettucePvEMissionEntity(skipTutorial: false, new LettuceBoss_LETL_826H_VoHandler()), 
			"LETL_827H" => new LettucePvEMissionEntity(skipTutorial: false, new LettuceBoss_LETL_827H_VoHandler()), 
			"LETL_828H" => new LettucePvEMissionEntity(skipTutorial: false, new LettuceBoss_LETL_828H_VoHandler()), 
			"LETL_829H" => new LettucePvEMissionEntity(skipTutorial: false, new LettuceBoss_LETL_829H_VoHandler()), 
			"LETL_830H" => new LettucePvEMissionEntity(skipTutorial: false, new LettuceBoss_LETL_830H_VoHandler()), 
			"LETL_831H" => new LettucePvEMissionEntity(skipTutorial: false, new LettuceBoss_LETL_831H_VoHandler()), 
			"LETL_832H" => new LettucePvEMissionEntity(skipTutorial: false, new LettuceBoss_LETL_832H_VoHandler()), 
			"LETL_833H" => new LettucePvEMissionEntity(skipTutorial: false, new LettuceBoss_LETL_833H_VoHandler()), 
			"LETL_834H" => new LettucePvEMissionEntity(skipTutorial: false, new LettuceBoss_LETL_834H_VoHandler()), 
			"LETL_835H" => new LettucePvEMissionEntity(skipTutorial: false, new LettuceBoss_LETL_835H_VoHandler()), 
			"LETL_836H" => new LettucePvEMissionEntity(skipTutorial: false, new LettuceBoss_LETL_836H_VoHandler()), 
			"LETL_837H" => new LettucePvEMissionEntity(skipTutorial: false, new LettuceBoss_LETL_837H_VoHandler()), 
			"LETL_838H" => new LettucePvEMissionEntity(skipTutorial: false, new LettuceBoss_LETL_838H_VoHandler()), 
			"LETL_839H" => new LettucePvEMissionEntity(skipTutorial: false, new LettuceBoss_LETL_839H_VoHandler()), 
			"LETL_840H" => new LettucePvEMissionEntity(skipTutorial: false, new LettuceBoss_LETL_840H_VoHandler()), 
			"LETL_841H" => new LettucePvEMissionEntity(skipTutorial: false, new LettuceBoss_LETL_841H_VoHandler()), 
			"LETL_842H" => new LettucePvEMissionEntity(skipTutorial: false, new LettuceBoss_LETL_842H_VoHandler()), 
			"LETL_843H" => new LettucePvEMissionEntity(skipTutorial: false, new LettuceBoss_LETL_843H_VoHandler()), 
			"LETL_844H" => new LettucePvEMissionEntity(skipTutorial: false, new LettuceBoss_LETL_844H_VoHandler()), 
			"LETL_845H" => new LettucePvEMissionEntity(skipTutorial: false, new LettuceBoss_LETL_845H_VoHandler()), 
			"LETL_846H" => new LettucePvEMissionEntity(skipTutorial: false, new LettuceBoss_LETL_846H_VoHandler()), 
			"LETL_847H" => new LettucePvEMissionEntity(skipTutorial: false, new LettuceBoss_LETL_847H_VoHandler()), 
			"LETL_848H" => new LettucePvEMissionEntity(skipTutorial: false, new LettuceBoss_LETL_848H_VoHandler()), 
			"LETL_848H_Heroic" => new LettucePvEMissionEntity(skipTutorial: false, new LettuceBoss_LETL_848H_Heroic_VoHandler()), 
			"LETL_849H" => new LettucePvEMissionEntity(skipTutorial: false, new LettuceBoss_LETL_849H_VoHandler()), 
			"LETL_850H" => new LettucePvEMissionEntity(skipTutorial: false, new LettuceBoss_LETL_850H_VoHandler()), 
			"LETL_851H" => new LettucePvEMissionEntity(skipTutorial: false, new LettuceBoss_LETL_851H_VoHandler()), 
			"LETL_852H" => new LettucePvEMissionEntity(skipTutorial: false, new LettuceBoss_LETL_852H_VoHandler()), 
			"LETL_853H" => new LettucePvEMissionEntity(skipTutorial: false, new LettuceBoss_LETL_853H_VoHandler()), 
			"LETL_854H" => new LettucePvEMissionEntity(skipTutorial: false, new LettuceBoss_LETL_854H_VoHandler()), 
			"LETL_855H" => new LettucePvEMissionEntity(skipTutorial: false, new LettuceBoss_LETL_855H_VoHandler()), 
			"LETL_856H" => new LettucePvEMissionEntity(skipTutorial: false, new LettuceBoss_LETL_856H_VoHandler()), 
			"LETL_857H" => new LettucePvEMissionEntity(skipTutorial: false, new LettuceBoss_LETL_857H_VoHandler()), 
			"LETL_858H" => new LettucePvEMissionEntity(skipTutorial: false, new LettuceBoss_LETL_858H_VoHandler()), 
			"LETL_859H" => new LettucePvEMissionEntity(skipTutorial: false, new LettuceBoss_LETL_859H_VoHandler()), 
			"LETL_860H" => new LettucePvEMissionEntity(skipTutorial: false, new LettuceBoss_LETL_860H_VoHandler()), 
			"LETL_861H" => new LettucePvEMissionEntity(skipTutorial: false, new LettuceBoss_LETL_861H_VoHandler()), 
			"LETL_862H" => new LettucePvEMissionEntity(skipTutorial: false, new LettuceBoss_LETL_862H_VoHandler()), 
			"LETL_863H" => new LettucePvEMissionEntity(skipTutorial: false, new LettuceBoss_LETL_863H_VoHandler()), 
			"LETL_866H" => new LettucePvEMissionEntity(skipTutorial: false, new LettuceBoss_LETL_864H_VoHandler()), 
			"LETL_864H" => new LettucePvEMissionEntity(skipTutorial: false, new LettuceBoss_LETL_865H_VoHandler()), 
			"LETL_865H" => new LettucePvEMissionEntity(skipTutorial: false, new LettuceBoss_LETL_866H_VoHandler()), 
			"LT23_800H" => new LettucePvEMissionEntity(skipTutorial: false, new LettuceBoss_LT23_800H_VoHandler()), 
			"LT23_801H" => new LettucePvEMissionEntity(skipTutorial: false, new LettuceBoss_LT23_801H_VoHandler()), 
			"LT23_802H" => new LettucePvEMissionEntity(skipTutorial: false, new LettuceBoss_LT23_802H_VoHandler()), 
			"LT23_803H" => new LettucePvEMissionEntity(skipTutorial: false, new LettuceBoss_LT23_803H_VoHandler()), 
			"LT23_804H" => new LettucePvEMissionEntity(skipTutorial: false, new LettuceBoss_LT23_804H_VoHandler()), 
			"LT23_805H" => new LettucePvEMissionEntity(skipTutorial: false, new LettuceBoss_LT23_805H_VoHandler()), 
			"LT23_806H" => new LettucePvEMissionEntity(skipTutorial: false, new LettuceBoss_LT23_806H_VoHandler()), 
			"LT23_807H" => new LettucePvEMissionEntity(skipTutorial: false, new LettuceBoss_LT23_807H_VoHandler()), 
			"LT23_809H" => new LettucePvEMissionEntity(skipTutorial: false, new LettuceBoss_LT23_809H_VoHandler()), 
			"LT23_811H" => new LettucePvEMissionEntity(skipTutorial: false, new LettuceBoss_LT23_811H_VoHandler()), 
			"LT23_812H" => new LettucePvEMissionEntity(skipTutorial: false, new LettuceBoss_LT23_812H_VoHandler()), 
			"LT23_813H" => new LettucePvEMissionEntity(skipTutorial: false, new LettuceBoss_LT23_813H_VoHandler()), 
			"LT23_815H" => new LettucePvEMissionEntity(skipTutorial: false, new LettuceBoss_LT23_815H_VoHandler()), 
			"LT23_816H" => new LettucePvEMissionEntity(skipTutorial: false, new LettuceBoss_LT23_816H_VoHandler()), 
			"LT23_817H" => new LettucePvEMissionEntity(skipTutorial: false, new LettuceBoss_LT23_817H_VoHandler()), 
			"LT23_818H" => new LettucePvEMissionEntity(skipTutorial: false, new LettuceBoss_LT23_818H_VoHandler()), 
			"LT23_819H" => new LettucePvEMissionEntity(skipTutorial: false, new LettuceBoss_LT23_819H_VoHandler()), 
			"LT23_820H" => new LettucePvEMissionEntity(skipTutorial: false, new LettuceBoss_LT23_820H_VoHandler()), 
			"LT23_821H" => new LettucePvEMissionEntity(skipTutorial: false, new LettuceBoss_LT23_821H_VoHandler()), 
			"LT23_822H" => new LettucePvEMissionEntity(skipTutorial: false, new LettuceBoss_LT23_822H_VoHandler()), 
			"LT23_823H" => new LettucePvEMissionEntity(skipTutorial: false, new LettuceBoss_LT23_823H_VoHandler()), 
			"LT23_825H" => new LettucePvEMissionEntity(skipTutorial: false, new LettuceBoss_LT23_825H_VoHandler()), 
			"LT23_826H1" => new LettucePvEMissionEntity(skipTutorial: false, new LettuceBoss_LT23_826H1_VoHandler()), 
			"LT24_810H" => new LettucePvEMissionEntity(skipTutorial: false, new LettuceBoss_LT24_810H_VoHandler()), 
			"LT24_811H" => new LettucePvEMissionEntity(skipTutorial: false, new LettuceBoss_LT24_811H_VoHandler()), 
			"LT24_812H" => new LettucePvEMissionEntity(skipTutorial: false, new LettuceBoss_LT24_812H_VoHandler()), 
			"LT24_813H" => new LettucePvEMissionEntity(skipTutorial: false, new LettuceBoss_LT24_813H_VoHandler()), 
			"LT24_814H" => new LettucePvEMissionEntity(skipTutorial: false, new LettuceBoss_LT24_814H_VoHandler()), 
			"LT24_814H4" => new LettucePvEMissionEntity(skipTutorial: false, new LettuceBoss_LT24_814H4_VoHandler()), 
			"LT24_814H6" => new LettucePvEMissionEntity(skipTutorial: false, new LettuceBoss_LT24_814H6_VoHandler()), 
			"LT24_815H" => new LettucePvEMissionEntity(skipTutorial: false, new LettuceBoss_LT24_815H_VoHandler()), 
			"LT24_816H" => new LettucePvEMissionEntity(skipTutorial: false, new LettuceBoss_LT24_816H_VoHandler()), 
			"LT24_817H" => new LettucePvEMissionEntity(skipTutorial: false, new LettuceBoss_LT24_817H_VoHandler()), 
			"LT24_818H" => new LettucePvEMissionEntity(skipTutorial: false, new LettuceBoss_LT24_818H_VoHandler()), 
			"LT24_819H" => new LettucePvEMissionEntity(skipTutorial: false, new LettuceBoss_LT24_819H_VoHandler()), 
			"LT24_820H" => new LettucePvEMissionEntity(skipTutorial: false, new LettuceBoss_LT24_820H_VoHandler()), 
			"LT24_821H" => new LettucePvEMissionEntity(skipTutorial: false, new LettuceBoss_LT24_821H_VoHandler()), 
			"LT24_822H" => new LettucePvEMissionEntity(skipTutorial: false, new LettuceBoss_LT24_822H_VoHandler()), 
			_ => new LettucePvEMissionEntity(), 
		};
	}

	private static string GetBossDesignCode(List<Network.PowerHistory> powerList)
	{
		foreach (Network.PowerHistory power in powerList)
		{
			if (power.Type != Network.PowerType.FULL_ENTITY)
			{
				continue;
			}
			Network.Entity ent = ((Network.HistFullEntity)power).Entity;
			foreach (Network.Entity.Tag tag in ent.Tags)
			{
				if (tag.Name == 2168 && tag.Value > 0)
				{
					return ent.CardID;
				}
			}
		}
		return string.Empty;
	}

	public bool IsAI()
	{
		return GameUtils.IsAIMission(m_missionId);
	}

	public bool WasAI()
	{
		return GameUtils.IsAIMission(m_prevMissionId);
	}

	public bool IsNextAI()
	{
		return GameUtils.IsAIMission(m_nextMissionId);
	}

	public bool IsTraditionalTutorial()
	{
		return GameUtils.IsTutorialMission(m_missionId);
	}

	public bool WasTutorial()
	{
		return GameUtils.IsTutorialMission(m_prevMissionId);
	}

	public bool IsNextTutorial()
	{
		return GameUtils.IsTutorialMission(m_nextMissionId);
	}

	public bool IsLettuceTutorial()
	{
		if (m_missionId != 3778 && m_missionId != 3900 && m_missionId != 3901)
		{
			return m_missionId == 3779;
		}
		return true;
	}

	public bool IsPractice()
	{
		return GameUtils.IsPracticeMission(m_missionId);
	}

	public bool WasPractice()
	{
		return GameUtils.IsPracticeMission(m_prevMissionId);
	}

	public bool IsNextPractice()
	{
		return GameUtils.IsPracticeMission(m_nextMissionId);
	}

	public bool IsClassChallengeMission()
	{
		return GameUtils.IsClassChallengeMission(m_missionId);
	}

	public bool IsHeroicMission()
	{
		return GameUtils.IsHeroicAdventureMission(m_missionId);
	}

	public bool IsExpansionMission()
	{
		return GameUtils.IsExpansionMission(m_missionId);
	}

	public bool WasExpansionMission()
	{
		return GameUtils.IsExpansionMission(m_prevMissionId);
	}

	public bool IsNextExpansionMission()
	{
		return GameUtils.IsExpansionMission(m_nextMissionId);
	}

	public bool IsDungeonCrawlMission()
	{
		return GameUtils.IsDungeonCrawlMission(m_missionId);
	}

	public bool WasDungeonCrawlMission()
	{
		return GameUtils.IsDungeonCrawlMission(m_prevMissionId);
	}

	public bool IsNextDungeonCrawlMission()
	{
		return GameUtils.IsDungeonCrawlMission(m_nextMissionId);
	}

	public bool IsPlay()
	{
		if (!IsRankedPlay())
		{
			return IsUnrankedPlay();
		}
		return true;
	}

	public bool WasPlay()
	{
		if (!WasRankedPlay())
		{
			return WasUnrankedPlay();
		}
		return true;
	}

	public bool IsNextPlay()
	{
		if (!IsNextRankedPlay())
		{
			return IsNextUnrankedPlay();
		}
		return true;
	}

	public bool IsRankedPlay()
	{
		return m_gameType == GameType.GT_RANKED;
	}

	public bool WasRankedPlay()
	{
		return m_prevGameType == GameType.GT_RANKED;
	}

	public bool IsNextRankedPlay()
	{
		return m_nextGameType == GameType.GT_RANKED;
	}

	public bool IsUnrankedPlay()
	{
		return m_gameType == GameType.GT_CASUAL;
	}

	public bool WasUnrankedPlay()
	{
		return m_prevGameType == GameType.GT_CASUAL;
	}

	public bool IsNextUnrankedPlay()
	{
		return m_nextGameType == GameType.GT_CASUAL;
	}

	public bool IsArena()
	{
		return m_gameType == GameType.GT_ARENA;
	}

	public bool WasArena()
	{
		return m_prevGameType == GameType.GT_ARENA;
	}

	public bool IsNextArena()
	{
		return m_nextGameType == GameType.GT_ARENA;
	}

	public bool IsFriendly()
	{
		return m_gameType == GameType.GT_VS_FRIEND;
	}

	public bool WasFriendly()
	{
		return m_prevGameType == GameType.GT_VS_FRIEND;
	}

	public bool IsNextFriendly()
	{
		return m_nextGameType == GameType.GT_VS_FRIEND;
	}

	public bool WasTavernBrawl()
	{
		if (GameUtils.IsTavernBrawlGameType(m_prevGameType))
		{
			return !WasFriendly();
		}
		return false;
	}

	public bool IsTavernBrawl()
	{
		if (GameUtils.IsTavernBrawlGameType(m_gameType))
		{
			return !IsFriendly();
		}
		return false;
	}

	public bool IsNextTavernBrawl()
	{
		if (GameUtils.IsTavernBrawlGameType(m_nextGameType))
		{
			return !IsNextFriendly();
		}
		return false;
	}

	public bool IsBattlegrounds()
	{
		return GameUtils.IsBattlegroundsGameType(m_gameType);
	}

	public bool WasBattlegrounds()
	{
		return GameUtils.IsBattlegroundsGameType(m_prevGameType);
	}

	public bool IsBattlegroundsTutorial()
	{
		if (m_gameType == GameType.GT_VS_AI)
		{
			return m_missionId == 3539;
		}
		return false;
	}

	public bool IsBattlegroundsMatchOrTutorial()
	{
		if (!IsBattlegrounds())
		{
			return IsBattlegroundsTutorial();
		}
		return true;
	}

	public bool IsBattlegroundVsAIGame()
	{
		if (m_gameType != GameType.GT_BATTLEGROUNDS_PLAYER_VS_AI && m_gameType != GameType.GT_BATTLEGROUNDS_DUO_VS_AI)
		{
			return m_gameType == GameType.GT_BATTLEGROUNDS_DUO_1_PLAYER_VS_AI;
		}
		return true;
	}

	public bool IsBattlegroundDuoGame()
	{
		if (m_gameType != GameType.GT_BATTLEGROUNDS_DUO && m_gameType != GameType.GT_BATTLEGROUNDS_DUO_VS_AI && m_gameType != GameType.GT_BATTLEGROUNDS_DUO_FRIENDLY)
		{
			return m_gameType == GameType.GT_BATTLEGROUNDS_DUO_1_PLAYER_VS_AI;
		}
		return true;
	}

	public bool IsRankedBattlegroundsDuoGame()
	{
		return m_gameType == GameType.GT_BATTLEGROUNDS_DUO;
	}

	public bool IsBattlegroundSoloGame()
	{
		if (m_gameType != GameType.GT_BATTLEGROUNDS && m_gameType != GameType.GT_BATTLEGROUNDS_FRIENDLY && m_gameType != GameType.GT_BATTLEGROUNDS_PLAYER_VS_AI)
		{
			return m_gameType == GameType.GT_BATTLEGROUNDS_AI_VS_AI;
		}
		return true;
	}

	public bool IsFriendlyBattlegrounds()
	{
		return m_gameType == GameType.GT_BATTLEGROUNDS_FRIENDLY;
	}

	public bool IsStandardFormatType()
	{
		return m_formatType == FormatType.FT_STANDARD;
	}

	public bool IsWildFormatType()
	{
		return m_formatType == FormatType.FT_WILD;
	}

	public bool IsClassicFormatType()
	{
		return m_formatType == FormatType.FT_CLASSIC;
	}

	public bool IsTwistFormatType()
	{
		return m_formatType == FormatType.FT_TWIST;
	}

	public bool IsNextWildFormatType()
	{
		return m_nextFormatType == FormatType.FT_WILD;
	}

	public bool IsDuels()
	{
		if (m_gameType != GameType.GT_PVPDR)
		{
			return m_gameType == GameType.GT_PVPDR_PAID;
		}
		return true;
	}

	public bool WasDuels()
	{
		if (m_prevGameType != GameType.GT_PVPDR)
		{
			return m_prevGameType == GameType.GT_PVPDR_PAID;
		}
		return true;
	}

	public bool IsMercenaries()
	{
		if (m_gameType != GameType.GT_MERCENARIES_PVP && m_gameType != GameType.GT_MERCENARIES_PVE && m_gameType != GameType.GT_MERCENARIES_PVE_COOP)
		{
			return m_gameType == GameType.GT_MERCENARIES_FRIENDLY;
		}
		return true;
	}

	private SceneMgr.Mode GetSpectatorPostGameSceneMode()
	{
		if (PartyManager.Get().IsInBattlegroundsParty())
		{
			return SceneMgr.Mode.BACON;
		}
		if (GameUtils.IsAnyTutorialComplete())
		{
			return SceneMgr.Mode.HUB;
		}
		if (Network.ShouldBeConnectedToAurora())
		{
			return SceneMgr.Mode.INVALID;
		}
		return SceneMgr.Mode.HUB;
	}

	public SceneMgr.Mode GetPostGameSceneMode()
	{
		if (IsSpectator())
		{
			return GetSpectatorPostGameSceneMode();
		}
		SceneMgr.Mode sceneMode = SceneMgr.Mode.HUB;
		switch (m_gameType)
		{
		case GameType.GT_RANKED:
		case GameType.GT_CASUAL:
			sceneMode = SceneMgr.Mode.TOURNAMENT;
			break;
		case GameType.GT_VS_AI:
			if (m_missionId == 3539)
			{
				BnetPresenceMgr.Get().SetGameField(28u, 1);
				sceneMode = SceneMgr.Mode.BACON;
			}
			else if (m_missionId == 3790)
			{
				sceneMode = SceneMgr.Mode.LETTUCE_MAP;
			}
			else
			{
				TavernBrawlMission currentMission = TavernBrawlManager.Get().CurrentMission();
				sceneMode = ((currentMission == null || currentMission.missionId != m_missionId) ? SceneMgr.Mode.ADVENTURE : SceneMgr.Mode.TAVERN_BRAWL);
			}
			break;
		case GameType.GT_ARENA:
			sceneMode = SceneMgr.Mode.DRAFT;
			break;
		case GameType.GT_VS_FRIEND:
			sceneMode = (FriendChallengeMgr.Get().HasChallenge() ? ((!FriendChallengeMgr.Get().IsChallengeTavernBrawl()) ? SceneMgr.Mode.FRIENDLY : SceneMgr.Mode.TAVERN_BRAWL) : SceneMgr.Mode.HUB);
			break;
		case GameType.GT_TAVERNBRAWL:
			sceneMode = SceneMgr.Mode.TAVERN_BRAWL;
			if (TavernBrawlManager.Get().CurrentTavernBrawlSeasonEndInSeconds < 10)
			{
				sceneMode = SceneMgr.Mode.HUB;
			}
			break;
		case GameType.GT_BATTLEGROUNDS:
		case GameType.GT_BATTLEGROUNDS_FRIENDLY:
		case GameType.GT_BATTLEGROUNDS_AI_VS_AI:
		case GameType.GT_BATTLEGROUNDS_PLAYER_VS_AI:
		case GameType.GT_BATTLEGROUNDS_DUO:
		case GameType.GT_BATTLEGROUNDS_DUO_VS_AI:
		case GameType.GT_BATTLEGROUNDS_DUO_FRIENDLY:
		case GameType.GT_BATTLEGROUNDS_DUO_1_PLAYER_VS_AI:
			sceneMode = SceneMgr.Mode.BACON;
			break;
		case GameType.GT_PVPDR_PAID:
		case GameType.GT_PVPDR:
			sceneMode = SceneMgr.Mode.PVP_DUNGEON_RUN;
			break;
		case GameType.GT_MERCENARIES_PVP:
			sceneMode = SceneMgr.Mode.LETTUCE_PLAY;
			break;
		case GameType.GT_MERCENARIES_PVE:
			sceneMode = SceneMgr.Mode.LETTUCE_MAP;
			break;
		case GameType.GT_MERCENARIES_PVE_COOP:
			sceneMode = SceneMgr.Mode.LETTUCE_MAP;
			break;
		case GameType.GT_MERCENARIES_FRIENDLY:
			sceneMode = SceneMgr.Mode.LETTUCE_FRIENDLY;
			break;
		}
		return sceneMode;
	}

	public SceneMgr.Mode GetPostDisconnectSceneMode()
	{
		if (IsSpectator())
		{
			return GetSpectatorPostGameSceneMode();
		}
		if (IsTraditionalTutorial())
		{
			return SceneMgr.Mode.INVALID;
		}
		return GetPostGameSceneMode();
	}

	public void PreparePostGameSceneMode(SceneMgr.Mode mode)
	{
		if (mode != SceneMgr.Mode.ADVENTURE || AdventureConfig.Get().CurrentSubScene != 0)
		{
			return;
		}
		ScenarioDbfRecord scenarioRecord = GameDbf.Scenario.GetRecord(m_missionId);
		if (scenarioRecord == null)
		{
			return;
		}
		int adventureId = scenarioRecord.AdventureId;
		if (adventureId != 0)
		{
			int modeId = scenarioRecord.ModeId;
			if (modeId != 0)
			{
				AdventureConfig.Get().SetSelectedAdventureMode((AdventureDbId)adventureId, (AdventureModeDbId)modeId);
				AdventureConfig.Get().ChangeSubSceneToSelectedAdventure();
				AdventureConfig.Get().SetMission((ScenarioDbId)m_missionId, showDetails: false);
			}
		}
	}

	public bool IsTransitionPopupShown()
	{
		if (m_transitionPopup == null)
		{
			return false;
		}
		return m_transitionPopup.IsShown();
	}

	public TransitionPopup GetTransitionPopup()
	{
		return m_transitionPopup;
	}

	public void UpdatePresence()
	{
		if (!Network.ShouldBeConnectedToAurora() || !Network.IsLoggedIn())
		{
			return;
		}
		if (IsSpectator())
		{
			PresenceMgr presenceMgr = PresenceMgr.Get();
			if (IsTraditionalTutorial())
			{
				presenceMgr.SetStatus(Global.PresenceStatus.SPECTATING_GAME_TUTORIAL);
			}
			else if (IsBattlegrounds() || m_missionId == 3539)
			{
				PresenceMgr.Get().SetStatus(Global.PresenceStatus.SPECTATING_GAME_BATTLEGROUNDS);
			}
			else if (IsPractice())
			{
				presenceMgr.SetStatus(Global.PresenceStatus.SPECTATING_GAME_PRACTICE);
			}
			else if (IsPlay())
			{
				if (IsRankedPlay())
				{
					if (IsStandardFormatType())
					{
						presenceMgr.SetStatus(Global.PresenceStatus.SPECTATING_GAME_PLAY_RANKED_STANDARD);
					}
					else if (IsWildFormatType())
					{
						presenceMgr.SetStatus(Global.PresenceStatus.SPECTATING_GAME_PLAY_RANKED_WILD);
					}
					else if (IsClassicFormatType())
					{
						presenceMgr.SetStatus(Global.PresenceStatus.SPECTATING_GAME_PLAY_RANKED_CLASSIC);
					}
					else if (IsTwistFormatType())
					{
						presenceMgr.SetStatus(Global.PresenceStatus.SPECTATING_GAME_PLAY_RANKED_TWIST);
					}
					else
					{
						presenceMgr.SetStatus(Global.PresenceStatus.SPECTATING_GAME_PLAY);
					}
				}
				else if (IsStandardFormatType())
				{
					presenceMgr.SetStatus(Global.PresenceStatus.SPECTATING_GAME_PLAY_CASUAL_STANDARD);
				}
				else if (IsWildFormatType())
				{
					presenceMgr.SetStatus(Global.PresenceStatus.SPECTATING_GAME_PLAY_CASUAL_WILD);
				}
				else if (IsClassicFormatType())
				{
					presenceMgr.SetStatus(Global.PresenceStatus.SPECTATING_GAME_PLAY_CASUAL_CLASSIC);
				}
				else if (IsTwistFormatType())
				{
					presenceMgr.SetStatus(Global.PresenceStatus.SPECTATING_GAME_PLAY_CASUAL_TWIST);
				}
				else
				{
					presenceMgr.SetStatus(Global.PresenceStatus.SPECTATING_GAME_PLAY);
				}
			}
			else if (IsArena())
			{
				presenceMgr.SetStatus(Global.PresenceStatus.SPECTATING_GAME_ARENA);
			}
			else if (IsFriendly())
			{
				presenceMgr.SetStatus(Global.PresenceStatus.SPECTATING_GAME_FRIENDLY);
			}
			else if (IsTavernBrawl())
			{
				presenceMgr.SetStatus(Global.PresenceStatus.SPECTATING_GAME_TAVERN_BRAWL);
			}
			else if (IsMercenaries())
			{
				presenceMgr.SetStatus(Global.PresenceStatus.SPECTATING_GAME_MERCENARIES);
			}
			else if (IsExpansionMission())
			{
				ScenarioDbId missionId = (ScenarioDbId)m_missionId;
				presenceMgr.SetStatus_SpectatingMission(missionId);
			}
			SpectatorManager.Get().UpdateMySpectatorInfo();
			return;
		}
		if (IsTraditionalTutorial())
		{
			switch ((ScenarioDbId)m_missionId)
			{
			case ScenarioDbId.TUTORIAL_REXXAR:
				PresenceMgr.Get().SetStatus(Global.PresenceStatus.TUTORIAL_GAME, PresenceTutorial.REXXAR);
				break;
			case ScenarioDbId.TUTORIAL_GARROSH:
				PresenceMgr.Get().SetStatus(Global.PresenceStatus.TUTORIAL_GAME, PresenceTutorial.GARROSH);
				break;
			case ScenarioDbId.TUTORIAL_LICH_KING:
				PresenceMgr.Get().SetStatus(Global.PresenceStatus.TUTORIAL_GAME, PresenceTutorial.LICH_KING);
				break;
			}
		}
		else if (IsBattlegroundDuoGame())
		{
			PresenceMgr.Get().SetStatus(Global.PresenceStatus.PLAY_BATTLEGROUNDS_DUOS);
		}
		else if (IsBattlegrounds() || m_missionId == 3539)
		{
			PresenceMgr.Get().SetStatus(Global.PresenceStatus.BATTLEGROUNDS_GAME);
		}
		else if (IsMercenaries())
		{
			if (m_gameType == GameType.GT_MERCENARIES_FRIENDLY)
			{
				PresenceMgr.Get().SetStatus(Global.PresenceStatus.MERCENARIES_FRIENDLY_GAME);
			}
			else
			{
				PresenceMgr.Get().SetStatus(Global.PresenceStatus.MERCENARIES_GAME);
			}
		}
		else if (IsPractice())
		{
			PresenceMgr.Get().SetStatus(Global.PresenceStatus.PRACTICE_GAME);
		}
		else if (IsPlay())
		{
			if (IsRankedPlay())
			{
				if (IsStandardFormatType())
				{
					PresenceMgr.Get().SetStatus(Global.PresenceStatus.PLAY_RANKED_STANDARD);
				}
				else if (IsWildFormatType())
				{
					PresenceMgr.Get().SetStatus(Global.PresenceStatus.PLAY_RANKED_WILD);
				}
				else if (IsClassicFormatType())
				{
					PresenceMgr.Get().SetStatus(Global.PresenceStatus.PLAY_RANKED_CLASSIC);
				}
				else if (IsTwistFormatType())
				{
					PresenceMgr.Get().SetStatus(Global.PresenceStatus.PLAY_RANKED_TWIST);
				}
				else
				{
					PresenceMgr.Get().SetStatus(Global.PresenceStatus.PLAY_GAME);
				}
			}
			else if (IsStandardFormatType())
			{
				PresenceMgr.Get().SetStatus(Global.PresenceStatus.PLAY_CASUAL_STANDARD);
			}
			else if (IsWildFormatType())
			{
				PresenceMgr.Get().SetStatus(Global.PresenceStatus.PLAY_CASUAL_WILD);
			}
			else if (IsClassicFormatType())
			{
				PresenceMgr.Get().SetStatus(Global.PresenceStatus.PLAY_CASUAL_CLASSIC);
			}
			else if (IsTwistFormatType())
			{
				PresenceMgr.Get().SetStatus(Global.PresenceStatus.PLAY_CASUAL_TWIST);
			}
			else
			{
				PresenceMgr.Get().SetStatus(Global.PresenceStatus.PLAY_GAME);
			}
		}
		else if (IsArena())
		{
			PresenceMgr.Get().SetStatus(Global.PresenceStatus.ARENA_GAME);
		}
		else if (IsFriendly())
		{
			Global.PresenceStatus status = Global.PresenceStatus.FRIENDLY_GAME;
			if (GameUtils.IsWaitingForOpponentReconnect())
			{
				status = Global.PresenceStatus.WAIT_FOR_OPPONENT_RECONNECT;
			}
			else if (FriendChallengeMgr.Get().IsChallengeTavernBrawl())
			{
				status = Global.PresenceStatus.TAVERN_BRAWL_FRIENDLY_GAME;
			}
			PresenceMgr.Get().SetStatus(status);
		}
		else if (IsTavernBrawl())
		{
			PresenceMgr.Get().SetStatus(Global.PresenceStatus.TAVERN_BRAWL_GAME);
		}
		else if (IsExpansionMission())
		{
			ScenarioDbId missionId2 = (ScenarioDbId)m_missionId;
			PresenceMgr.Get().SetStatus_PlayingMission(missionId2);
		}
		SpectatorManager.Get().UpdateMySpectatorInfo();
	}

	public void UpdateSessionPresence(GameType gameType)
	{
		if (gameType == GameType.GT_ARENA)
		{
			int wins = DraftManager.Get().GetWins();
			int losses = DraftManager.Get().GetLosses();
			SessionRecord sessionRecord = new SessionRecord();
			sessionRecord.Wins = (uint)wins;
			sessionRecord.Losses = (uint)losses;
			sessionRecord.RunFinished = false;
			sessionRecord.SessionRecordType = SessionRecordType.ARENA;
			BnetPresenceMgr.Get().SetGameFieldBlob(22u, sessionRecord);
		}
		else if (GameUtils.IsTavernBrawlGameType(gameType) && TavernBrawlManager.Get().IsCurrentSeasonSessionBased)
		{
			int wins2 = TavernBrawlManager.Get().GamesWon;
			int losses2 = TavernBrawlManager.Get().GamesLost;
			SessionRecord sessionRecord2 = new SessionRecord();
			sessionRecord2.Wins = (uint)wins2;
			sessionRecord2.Losses = (uint)losses2;
			sessionRecord2.RunFinished = false;
			sessionRecord2.SessionRecordType = ((TavernBrawlManager.Get().CurrentSeasonBrawlMode != 0) ? SessionRecordType.HEROIC_BRAWL : SessionRecordType.TAVERN_BRAWL);
			BnetPresenceMgr.Get().SetGameFieldBlob(22u, sessionRecord2);
		}
	}

	public void SetLastDisplayedPlayerName(int playerId, string name)
	{
		m_lastDisplayedPlayerNames[playerId] = name;
	}

	public string GetLastDisplayedPlayerName(int playerId)
	{
		m_lastDisplayedPlayerNames.TryGetValue(playerId, out var name);
		return name;
	}

	private void OnSceneUnloaded(SceneMgr.Mode prevMode, PegasusScene prevScene, object userData)
	{
		if (prevMode == SceneMgr.Mode.GAMEPLAY && SceneMgr.Get().GetMode() != SceneMgr.Mode.GAMEPLAY)
		{
			OnGameEnded();
		}
	}

	private void OnScenePreLoad(SceneMgr.Mode prevMode, SceneMgr.Mode mode, object userData)
	{
		PreloadTransitionPopup();
		if (SceneMgr.Get().GetMode() == SceneMgr.Mode.HUB)
		{
			DestroyTransitionPopup();
		}
	}

	private void OnServerResult()
	{
		if (IsFindingGame())
		{
			ServerResult msg = Network.Get().GetServerResult();
			GameNetLogger.Log(Blizzard.T5.Core.LogLevel.Debug, $"GameMgr.OnServerResult() -  result: {msg.ResultCode}");
			if (msg.ResultCode == 1)
			{
				float secondsToWait = Mathf.Max(msg.HasRetryDelaySeconds ? msg.RetryDelaySeconds : 2f, 0.5f);
				Processor.CancelScheduledCallback(OnServerResult_Retry);
				Processor.ScheduleCallback(secondsToWait, realTime: true, OnServerResult_Retry);
			}
			else if (msg.ResultCode == 2)
			{
				OnGameCanceled();
			}
		}
	}

	private void OnServerResult_Retry(object userData)
	{
		Network.Get().RetryGotoGameServer();
	}

	private void ChangeBoardIfNecessary()
	{
		int boardId = m_gameSetup.Board;
		if (DemoMgr.Get().IsExpoDemo())
		{
			string boardName = Vars.Key("Demo.ForceBoard").GetStr(null);
			if (boardName != null)
			{
				boardId = GameUtils.GetBoardIdFromAssetName(boardName);
			}
		}
		m_gameSetup.Board = boardId;
	}

	private void PreloadTransitionPopup()
	{
		switch (SceneMgr.Get().GetMode())
		{
		case SceneMgr.Mode.TOURNAMENT:
		case SceneMgr.Mode.DRAFT:
		case SceneMgr.Mode.TAVERN_BRAWL:
			LoadTransitionPopup(MATCHING_POPUP_NAME);
			break;
		case SceneMgr.Mode.FRIENDLY:
		case SceneMgr.Mode.ADVENTURE:
			LoadTransitionPopup("LoadingPopup.prefab:ff9266f7c55faa94b9cd0f1371df7168");
			break;
		case SceneMgr.Mode.FATAL_ERROR:
		case SceneMgr.Mode.CREDITS:
		case SceneMgr.Mode.RESET:
			break;
		}
	}

	private string DetermineTransitionPopupForFindGame(GameType gameType, int missionId)
	{
		if (gameType == GameType.GT_TUTORIAL)
		{
			return null;
		}
		if (GameUtils.IsMatchmadeGameType(gameType, missionId))
		{
			return MATCHING_POPUP_NAME;
		}
		return "LoadingPopup.prefab:ff9266f7c55faa94b9cd0f1371df7168";
	}

	private void LoadTransitionPopup(string prefabPath)
	{
		GameObject go = AssetLoader.Get().InstantiatePrefab(prefabPath, AssetLoadingOptions.IgnorePrefabPosition);
		if (go == null)
		{
			Error.AddDevFatal("GameMgr.LoadTransitionPopup() - Failed to load {0}", prefabPath);
			return;
		}
		if (m_transitionPopup != null)
		{
			UnityEngine.Object.Destroy(m_transitionPopup.gameObject);
		}
		m_transitionPopup = go.GetComponent<TransitionPopup>();
		m_transitionPopup.OnPopupDestroyed += OnHandlePopupDestroyed;
		m_initialTransitionPopupPos = m_transitionPopup.transform.position;
		m_transitionPopup.RegisterMatchCanceledEvent(OnTransitionPopupCanceled);
		LayerUtils.SetLayer(m_transitionPopup, GameLayer.IgnoreFullScreenEffects);
	}

	private void OnHandlePopupDestroyed()
	{
		m_transitionPopup = null;
	}

	private void ShowTransitionPopup(string popupName, int scenarioId)
	{
		Type popupType = s_transitionPopupNameToType[popupName];
		if (!m_transitionPopup || m_transitionPopup.GetType() != popupType)
		{
			DestroyTransitionPopup();
			LoadTransitionPopup(popupName);
		}
		if (!m_transitionPopup.IsShown())
		{
			if (Box.Get() != null && Box.Get().GetState() != Box.State.OPEN)
			{
				Vector3 diff = Box.Get().m_Camera.GetCameraPosition(BoxCamera.State.OPENED) - m_initialTransitionPopupPos;
				Vector3 targetPosition = CameraUtils.GetMainCamera().transform.position - diff;
				m_transitionPopup.transform.position = targetPosition;
			}
			AdventureDbId adventureId = GameUtils.GetAdventureId(m_nextMissionId);
			m_transitionPopup.SetAdventureId(adventureId);
			m_transitionPopup.SetFormatType(m_nextFormatType);
			m_transitionPopup.SetGameType(m_nextGameType);
			m_transitionPopup.SetDeckId(m_lastDeckId);
			m_transitionPopup.SetScenarioId(scenarioId);
			m_transitionPopup.Show();
			if (this.OnTransitionPopupShown != null)
			{
				this.OnTransitionPopupShown();
			}
		}
	}

	private void OnTransitionPopupCanceled()
	{
		bool num = Network.Get().IsFindingGame();
		if (num)
		{
			Network.Get().CancelFindGame();
		}
		ChangeFindGameState(FindGameState.CLIENT_CANCELED);
		if (!num)
		{
			ChangeFindGameState(FindGameState.INVALID);
		}
	}

	private void DestroyTransitionPopup()
	{
		if ((bool)m_transitionPopup)
		{
			UnityEngine.Object.Destroy(m_transitionPopup.gameObject);
			m_transitionPopup = null;
		}
	}

	private bool GetFriendlyErrorMessage(int errorCode, ref string headerKey, ref string messageKey, ref object[] messageParams)
	{
		switch (errorCode)
		{
		case 1000500:
			headerKey = "GLOBAL_ERROR_GENERIC_HEADER";
			messageKey = "GLOBAL_ERROR_FIND_GAME_SCENARIO_INCORRECT_NUM_PLAYERS";
			return true;
		case 1000501:
			headerKey = "GLOBAL_ERROR_GENERIC_HEADER";
			messageKey = "GLOBAL_ERROR_FIND_GAME_SCENARIO_NO_DECK_SPECIFIED";
			return true;
		case 1000502:
		case 1002008:
			headerKey = "GLOBAL_ERROR_GENERIC_HEADER";
			messageKey = "GLOBAL_ERROR_FIND_GAME_SCENARIO_MISCONFIGURED";
			return true;
		case 1001001:
			headerKey = "GLOBAL_TAVERN_BRAWL";
			messageKey = "GLOBAL_TAVERN_BRAWL_ERROR_NOT_ACTIVE";
			TavernBrawlManager.Get().RefreshServerData();
			return true;
		case 1002009:
			headerKey = "GLOBAL_ERROR_GENERIC_HEADER";
			messageKey = "GLUE_ERROR_DECK_RULESET_RULE_VIOLATION";
			return true;
		case 1002012:
			headerKey = "GLOBAL_ERROR_GENERIC_HEADER";
			messageKey = "GLUE_ERROR_DECK_RULESET_RULE_VIOLATION_BANNED_SIDEBOARD_CARD";
			return true;
		case 1002013:
			headerKey = "GLOBAL_ERROR_GENERIC_HEADER";
			messageKey = "GLUE_ERROR_DECK_RULESET_RULE_VIOLATION_TOO_FEW_CARDS_IN_SIDEBOARD";
			return true;
		case 1002014:
			headerKey = "GLOBAL_ERROR_GENERIC_HEADER";
			messageKey = "GLUE_ERROR_DECK_RULESET_RULE_VIOLATION_TOO_MANY_CARDS_IN_SIDEBOARD";
			return true;
		case 1001000:
			headerKey = "GLOBAL_TAVERN_BRAWL";
			messageKey = "GLOBAL_TAVERN_BRAWL_ERROR_SEASON_INCREMENTED";
			TavernBrawlManager.Get().RefreshServerData();
			return true;
		case 1003005:
			if (m_nextGameType != GameType.GT_ARENA)
			{
				break;
			}
			headerKey = "GLOBAL_ERROR_GENERIC_HEADER";
			messageKey = "GLOBAL_ARENA_SEASON_ERROR_NOT_ACTIVE";
			DraftManager.Get().RefreshCurrentSeasonFromServer();
			if (SceneMgr.Get().GetMode() == SceneMgr.Mode.DRAFT)
			{
				Processor.ScheduleCallback(0f, realTime: false, delegate
				{
					Navigation.GoBack();
				});
			}
			return true;
		case 1002007:
			headerKey = "GLOBAL_ERROR_GENERIC_HEADER";
			messageKey = "GLUE_ERROR_DECK_VALIDATION_WRONG_FORMAT";
			return true;
		case 1002002:
		{
			GameType gameType = GetGameType();
			if (gameType == GameType.GT_UNKNOWN)
			{
				gameType = GetNextGameType();
			}
			if (!GameUtils.IsMatchmadeGameType(gameType, null))
			{
				headerKey = "GLOBAL_ERROR_GENERIC_HEADER";
				messageKey = "GLUE_ERROR_DECK_RULESET_RULE_VIOLATION";
				return true;
			}
			break;
		}
		case 1003015:
			headerKey = "GLOBAL_ERROR_GENERIC_HEADER";
			messageKey = "GLUE_ERROR_PLAY_GAME_PARTY_NOT_ALLOWED";
			return true;
		}
		return false;
	}

	private void OnGameQueueEvent(QueueEvent queueEvent)
	{
		GameNetLogger.Log(Blizzard.T5.Core.LogLevel.Debug, $"GameMgr.OnGameQueueEvent() - QueueEvent: {queueEvent.EventType}");
		FindGameState? result = null;
		s_bnetToFindGameResultMap.TryGetValue(queueEvent.EventType, out result);
		if (queueEvent.BnetError != 0)
		{
			m_lastEnterGameError = (uint)queueEvent.BnetError;
		}
		if (!result.HasValue)
		{
			return;
		}
		if (queueEvent.EventType == QueueEvent.Type.QUEUE_DELAY_ERROR)
		{
			if (queueEvent.BnetError == 25017)
			{
				return;
			}
			string headerKey = "";
			string messageKey = null;
			object[] messageArgs = new object[0];
			if (GetFriendlyErrorMessage(queueEvent.BnetError, ref headerKey, ref messageKey, ref messageArgs))
			{
				Error.AddWarningLoc(headerKey, messageKey, messageArgs);
				result = FindGameState.BNET_QUEUE_CANCELED;
				ResetNextGameState();
				Network.Get().ClearLastGameServerJoined();
			}
		}
		if (queueEvent.BnetError != 0)
		{
			string errorName = string.Empty;
			if (Enum.IsDefined(typeof(BattleNetErrors), (BattleNetErrors)queueEvent.BnetError))
			{
				errorName = ((BattleNetErrors)queueEvent.BnetError/*cast due to .constrained prefix*/).ToString();
			}
			else if (Enum.IsDefined(typeof(ErrorCode), (ErrorCode)queueEvent.BnetError))
			{
				errorName = ((ErrorCode)queueEvent.BnetError/*cast due to .constrained prefix*/).ToString();
			}
			string msg = $"OnGameQueueEvent error={queueEvent.BnetError} {errorName}";
			if (HearthstoneApplication.IsInternal())
			{
				Error.AddDevWarning("OnGameQueueEvent", msg);
			}
			else
			{
				Log.BattleNet.PrintWarning(msg);
				GameNetLogger.Log(Blizzard.T5.Core.LogLevel.Warning, "GameMgr.OnGameQueueEvent() - message: " + msg);
				GameNetLogger.Log(Blizzard.T5.Core.LogLevel.Debug, Network.Get().GetNetworkGameStateString());
			}
		}
		if (queueEvent.EventType == QueueEvent.Type.QUEUE_GAME_STARTED)
		{
			queueEvent.GameServer.Mission = m_nextMissionId;
			ChangeFindGameState(result.Value, queueEvent, queueEvent.GameServer, null);
		}
		else if (m_findGameState != 0 || (result.Value != FindGameState.BNET_QUEUE_UPDATED && result.Value != FindGameState.BNET_QUEUE_DELAYED))
		{
			ChangeFindGameState(result.Value, queueEvent);
		}
	}

	private void OnGameToJoinNotification()
	{
		GameNetLogger.Log(Blizzard.T5.Core.LogLevel.Debug, "GameMgr.OnGameToJoinNotification()");
		GameToConnectNotification notification = Network.Get().GetGameToConnectNotification();
		ConnectToGame(notification.Info);
	}

	private void OnGameSetup()
	{
		GameNetLogger.Log(Blizzard.T5.Core.LogLevel.Information, "GameMgr.OnGameSetup()");
		if (SpectatorManager.Get().IsSpectatingOpposingSide() && m_gameSetup != null)
		{
			return;
		}
		m_gameSetup = Network.Get().GetGameSetupInfo();
		ChangeBoardIfNecessary();
		if (m_findGameState == FindGameState.INVALID && m_gameType == GameType.GT_UNKNOWN)
		{
			Log.BattleNet.PrintError($"GameMgr.OnGameSetup() - Received {GameSetup.PacketID.ID} packet even though we're not looking for a game.");
			GameNetLogger.Log(Blizzard.T5.Core.LogLevel.Error, $"GameMgr.OnGameSetup() - Received {GameSetup.PacketID.ID} packet even though we're not looking for a game.");
			GameNetLogger.Log(Blizzard.T5.Core.LogLevel.Debug, Network.Get().GetNetworkGameStateString());
			return;
		}
		m_lastGameData.Clear();
		m_lastGameData.GameConnectionInfo = m_connectionInfoForGameConnectingTo;
		m_connectionInfoForGameConnectingTo = null;
		m_prevGameType = m_gameType;
		m_gameType = m_nextGameType;
		m_nextGameType = GameType.GT_UNKNOWN;
		m_prevFormatType = m_formatType;
		m_formatType = m_nextFormatType;
		m_nextFormatType = FormatType.FT_UNKNOWN;
		m_prevMissionId = m_missionId;
		m_missionId = m_nextMissionId;
		m_nextMissionId = 0;
		m_brawlLibraryItemId = m_nextBrawlLibraryItemId;
		m_nextBrawlLibraryItemId = 0;
		m_prevGameReconnectType = m_gameReconnectType;
		m_gameReconnectType = m_nextGameReconnectType;
		m_nextGameReconnectType = GameReconnectType.INVALID;
		m_prevSpectator = m_spectator;
		m_spectator = m_nextSpectator;
		m_nextSpectator = false;
		if (!m_spectator)
		{
			HearthstonePerformance.Get()?.StartPerformanceFlow(new FlowPerformanceGame.GameSetupConfig
			{
				GameType = m_gameType,
				BoardId = m_gameSetup.Board,
				ScenarioId = m_missionId,
				FormatType = m_formatType
			});
		}
		ChangeFindGameState(FindGameState.SERVER_GAME_STARTED);
	}

	public void OnGameCanceled()
	{
		GameNetLogger.Log(Blizzard.T5.Core.LogLevel.Debug, "GameMgr.OnGameCanceled()");
		ResetNextGameState();
		Network.Get().ClearLastGameServerJoined();
		Network network = Network.Get();
		Network.GameCancelInfo cancelInfo = network.GetGameCancelInfo();
		network.DisconnectFromGameServer(Network.DisconnectReason.GameCanceled);
		ChangeFindGameState(FindGameState.SERVER_GAME_CANCELED, cancelInfo);
	}

	public bool OnBnetError(BnetErrorInfo info, object userData)
	{
		if (info.GetFeature() == BnetFeature.Games)
		{
			BattleNetErrors error = (BattleNetErrors)(m_lastEnterGameError = (uint)info.GetError());
			string errName = null;
			bool handled = false;
			FindGameState nextState = FindGameState.BNET_ERROR;
			if (error == BattleNetErrors.ERROR_GAME_MASTER_INVALID_FACTORY || error == BattleNetErrors.ERROR_GAME_MASTER_NO_GAME_SERVER || error == BattleNetErrors.ERROR_GAME_MASTER_NO_FACTORY)
			{
				errName = error.ToString();
				handled = true;
			}
			if (!handled)
			{
				string headerKey = "";
				string messageKey = null;
				object[] messageArgs = new object[0];
				ReconnectMgr reconnectMgr = ReconnectMgr.Get();
				if (GetFriendlyErrorMessage((int)m_lastEnterGameError, ref headerKey, ref messageKey, ref messageArgs) && !reconnectMgr.IsReconnectingToGame() && !reconnectMgr.IsRestoringGameStateFromDatabase())
				{
					Error.AddWarningLoc(headerKey, messageKey, messageArgs);
					ErrorCode lastEnterGameError = (ErrorCode)m_lastEnterGameError;
					errName = lastEnterGameError.ToString();
					nextState = FindGameState.BNET_QUEUE_CANCELED;
					handled = true;
				}
			}
			if (!handled && info.GetFeatureEvent() == BnetFeatureEvent.Games_OnFindGame)
			{
				handled = true;
			}
			if (handled)
			{
				string errString = $"GameMgr.OnBnetError() - received error {m_lastEnterGameError} {errName}";
				Log.BattleNet.PrintError(errString);
				GameNetLogger.Log(Blizzard.T5.Core.LogLevel.Error, errString);
				GameNetLogger.Log(Blizzard.T5.Core.LogLevel.Debug, Network.Get().GetNetworkGameStateString());
				if (!Log.BattleNet.CanPrint(LogTarget.CONSOLE, Blizzard.T5.Logging.LogLevel.Error, verbose: false))
				{
					Debug.LogError(string.Format("[{0}] {1}", "BattleNet", errString));
				}
				ResetNextGameState();
				Network.Get().ClearLastGameServerJoined();
				ChangeFindGameState(nextState);
				return true;
			}
		}
		return false;
	}

	private void ResetNextGameState()
	{
		m_nextGameType = GameType.GT_UNKNOWN;
		m_nextFormatType = FormatType.FT_UNKNOWN;
		m_nextMissionId = 0;
		m_nextBrawlLibraryItemId = 0;
		m_nextGameReconnectType = GameReconnectType.INVALID;
		m_nextSpectator = false;
	}

	private bool OnReconnectTimeout(object userData)
	{
		ResetNextGameState();
		Network.Get().ClearLastGameServerJoined();
		ChangeFindGameState(FindGameState.CLIENT_CANCELED);
		ChangeFindGameState(FindGameState.INVALID);
		return false;
	}

	private void OnFatalError(FatalErrorMessage message, object userData)
	{
		if (IsFindingGame())
		{
			ChangeFindGameState(FindGameState.CLIENT_CANCELED);
			ChangeFindGameState(FindGameState.INVALID);
			if (message.m_reason != FatalErrorReason.MOBILE_GAME_SERVER_RPC_ERROR)
			{
				DialogManager.Get().ShowReconnectHelperDialog();
			}
		}
	}

	private bool ChangeFindGameState(FindGameState state)
	{
		return ChangeFindGameState(state, null, null, null);
	}

	private bool ChangeFindGameState(FindGameState state, QueueEvent queueEvent)
	{
		return ChangeFindGameState(state, queueEvent, null, null);
	}

	private bool ChangeFindGameState(FindGameState state, GameServerInfo serverInfo)
	{
		return ChangeFindGameState(state, null, serverInfo, null);
	}

	private bool ChangeFindGameState(FindGameState state, Network.GameCancelInfo cancelInfo)
	{
		return ChangeFindGameState(state, null, null, cancelInfo);
	}

	private bool ChangeFindGameState(FindGameState state, QueueEvent queueEvent, GameServerInfo serverInfo, Network.GameCancelInfo cancelInfo)
	{
		GameNetLogger.Log(Blizzard.T5.Core.LogLevel.Information, $"GameMgr.ChangeFindGameState() - state: {state}, previous state: {m_findGameState}");
		FindGameState prevState = m_findGameState;
		uint lastEnterGameError = m_lastEnterGameError;
		m_findGameState = state;
		if (serverInfo != null)
		{
			m_gameHandleId = (int)serverInfo.GameHandle;
		}
		FindGameEventData eventData = new FindGameEventData();
		eventData.m_state = state;
		eventData.m_gameServer = serverInfo;
		eventData.m_cancelInfo = cancelInfo;
		if (queueEvent != null)
		{
			eventData.m_queueMinSeconds = queueEvent.MinSeconds;
			eventData.m_queueMaxSeconds = queueEvent.MaxSeconds;
		}
		bool num = FireFindGameEvent(eventData);
		if (!num)
		{
			DoDefaultFindGameEventBehavior(eventData);
		}
		FinalizeState(eventData);
		if (prevState != state)
		{
			Network.Get().OnFindGameStateChanged(prevState, state, lastEnterGameError);
		}
		return num;
	}

	private bool FireFindGameEvent(FindGameEventData eventData)
	{
		bool handled = false;
		FindGameListener[] listeners = m_findGameListeners.ToArray();
		for (int i = 0; i < listeners.Length; i++)
		{
			handled = listeners[i].Fire(eventData) || handled;
		}
		return handled;
	}

	private void DoDefaultFindGameEventBehavior(FindGameEventData eventData)
	{
		GameNetLogger.Log(Blizzard.T5.Core.LogLevel.Debug, $"GameMgr.DoDefaultFindGameEventBehavior() -  FindGameEventData: {eventData.m_state}");
		switch (eventData.m_state)
		{
		case FindGameState.CLIENT_ERROR:
		case FindGameState.BNET_ERROR:
		{
			ReconnectMgr reconnectMgr = ReconnectMgr.Get();
			if (!reconnectMgr.IsReconnectingToGame() && !reconnectMgr.IsRestoringGameStateFromDatabase())
			{
				Error.AddWarningLoc("GLOBAL_ERROR_GENERIC_HEADER", "GLOBAL_ERROR_GAME_DENIED");
			}
			HideTransitionPopup();
			break;
		}
		case FindGameState.BNET_QUEUE_CANCELED:
			HideTransitionPopup();
			break;
		case FindGameState.SERVER_GAME_CONNECTING:
			Network.Get().GotoGameServer(eventData.m_gameServer, IsNextReconnect());
			break;
		case FindGameState.SERVER_GAME_STARTED:
			if (Box.Get() != null)
			{
				LoadingScreen.Get().SetFreezeFrameCamera(Box.Get().GetCamera());
				LoadingScreen.Get().SetTransitionAudioListener(Box.Get().GetAudioListener());
			}
			if (SceneMgr.Get().GetMode() == SceneMgr.Mode.GAMEPLAY)
			{
				if (!SpectatorManager.Get().IsSpectatingOpposingSide())
				{
					Debug.Log("SERVER_GAME_STARTED event - Reloading Gameplay Scene");
					GameNetLogger.Log(Blizzard.T5.Core.LogLevel.Debug, "GameMgr.DoDefaultFindGameEventBehavior() - SERVER_GAME_STARTED event - Reloading Gameplay Scene");
					GameNetLogger.Log(Blizzard.T5.Core.LogLevel.Debug, Network.Get().GetNetworkGameStateString());
					SceneMgr.Get().ReloadMode();
				}
			}
			else
			{
				Debug.Log("SERVER_GAME_STARTED event - Loading Gameplay Scene");
				GameNetLogger.Log(Blizzard.T5.Core.LogLevel.Debug, "GameMgr.DoDefaultFindGameEventBehavior() - SERVER_GAME_STARTED event - Loading Gameplay Scene");
				GameNetLogger.Log(Blizzard.T5.Core.LogLevel.Debug, Network.Get().GetNetworkGameStateString());
				SceneMgr.Get().SetNextMode(SceneMgr.Mode.GAMEPLAY);
			}
			break;
		case FindGameState.SERVER_GAME_CANCELED:
			if (eventData.m_cancelInfo != null)
			{
				Network.GameCancelInfo.Reason cancelReason = eventData.m_cancelInfo.CancelReason;
				if ((uint)(cancelReason - 1) <= 2u)
				{
					Error.AddWarningLoc("GLOBAL_ERROR_GENERIC_HEADER", "GLOBAL_ERROR_GAME_OPPONENT_TIMEOUT");
					GameNetLogger.Log(Blizzard.T5.Core.LogLevel.Debug, Network.Get().GetNetworkGameStateString());
				}
				else
				{
					Error.AddDevWarning("GAME ERROR", "The Game Server canceled the game. Error: {0}", eventData.m_cancelInfo.CancelReason);
					GameNetLogger.Log(Blizzard.T5.Core.LogLevel.Debug, Network.Get().GetNetworkGameStateString());
				}
			}
			HideTransitionPopup();
			break;
		case FindGameState.CLIENT_CANCELED:
			HideTransitionPopup();
			break;
		case FindGameState.BNET_QUEUE_ENTERED:
		case FindGameState.BNET_QUEUE_DELAYED:
		case FindGameState.BNET_QUEUE_UPDATED:
			break;
		}
	}

	private void FinalizeState(FindGameEventData eventData)
	{
		switch (eventData.m_state)
		{
		case FindGameState.CLIENT_ERROR:
		case FindGameState.BNET_QUEUE_CANCELED:
		case FindGameState.BNET_ERROR:
		case FindGameState.SERVER_GAME_STARTED:
		case FindGameState.SERVER_GAME_CANCELED:
			ChangeFindGameState(FindGameState.INVALID);
			break;
		case FindGameState.BNET_QUEUE_ENTERED:
		case FindGameState.BNET_QUEUE_DELAYED:
		case FindGameState.BNET_QUEUE_UPDATED:
		case FindGameState.SERVER_GAME_CONNECTING:
			break;
		}
	}

	private void OnGameEnded()
	{
		GameNetLogger.Log(Blizzard.T5.Core.LogLevel.Debug, "GameMgr.OnGameEnded()");
		if (!m_spectator)
		{
			HearthstonePerformance.Get()?.StopCurrentFlow();
		}
		m_prevGameType = m_gameType;
		m_gameType = GameType.GT_UNKNOWN;
		m_prevFormatType = m_formatType;
		m_formatType = FormatType.FT_UNKNOWN;
		m_prevMissionId = m_missionId;
		m_missionId = 0;
		m_brawlLibraryItemId = 0;
		m_prevGameReconnectType = m_gameReconnectType;
		m_gameReconnectType = GameReconnectType.INVALID;
		m_prevSpectator = m_spectator;
		m_spectator = false;
		m_lastEnterGameError = 0u;
		m_pendingAutoConcede = false;
		m_gameSetup = null;
		m_lastDisplayedPlayerNames.Clear();
	}
}
