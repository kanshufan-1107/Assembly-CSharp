using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using Assets;
using Blizzard.T5.Core;
using Cysharp.Threading.Tasks;
using PegasusGame;
using UnityEngine;

public class GameState
{
	public enum ResponseMode
	{
		NONE,
		OPTION,
		SUB_OPTION,
		OPTION_TARGET,
		CHOICE
	}

	public enum CreateGamePhase
	{
		INVALID,
		CREATING,
		CREATED
	}

	public delegate void GameStateInitializedCallback(GameState instance, object userData);

	public delegate void GameStateShutdownCallback(GameState instance, object userData);

	public delegate void CreateGameCallback(CreateGamePhase phase, object userData);

	public delegate void OptionsReceivedCallback(object userData);

	public delegate void OptionsSentCallback(Network.Options.Option option, object userData);

	public delegate void OptionRejectedCallback(Network.Options.Option option, object userData);

	public delegate void EntityChoicesReceivedCallback(Network.EntityChoices choices, PowerTaskList preChoiceTaskList, object userData);

	public delegate bool EntitiesChosenReceivedCallback(Network.EntitiesChosen chosen, object userData);

	public delegate void CurrentPlayerChangedCallback(Player player, object userData);

	public delegate void TurnChangedCallback(int oldTurn, int newTurn, object userData);

	public delegate void FriendlyTurnStartedCallback(object userData);

	public delegate void TurnTimerUpdateCallback(TurnTimerUpdate update, object userData);

	public delegate void SpectatorNotifyEventCallback(SpectatorNotify notify, object userData);

	public delegate void GameOverCallback(TAG_PLAYSTATE playState, object userData);

	public delegate void HeroChangedCallback(Player player, object userData);

	public delegate void BusyStateChangedCallback(bool isBusy, object userData);

	public delegate void CantPlayCallback(Entity entity, object userData);

	public delegate void DamageCapChangedCallback(int oldValue, int newValue, object userData);

	public delegate void DiabloFightPlayerIDChangedCallback(int oldValue, int newValue, object userData);

	private delegate void AppendBlockingServerItemCallback<T>(StringBuilder builder, T item);

	private class SelectedOption
	{
		public int m_main = -1;

		public int m_sub = -1;

		public int m_target;

		public int m_position;

		public void Clear()
		{
			m_main = -1;
			m_sub = -1;
			m_target = 0;
			m_position = 0;
		}

		public void CopyFrom(SelectedOption original)
		{
			m_main = original.m_main;
			m_sub = original.m_sub;
			m_target = original.m_target;
			m_position = original.m_position;
		}
	}

	private class QueuedChoice
	{
		public enum PacketType
		{
			ENTITY_CHOICES,
			ENTITIES_CHOSEN
		}

		public PacketType m_type;

		public object m_packet;

		public object m_eventData;
	}

	private class GameStateInitializedListener : EventListener<GameStateInitializedCallback>
	{
		public void Fire(GameState instance)
		{
			m_callback(instance, m_userData);
		}
	}

	private class GameStateShutdownListener : EventListener<GameStateShutdownCallback>
	{
		public void Fire(GameState instance)
		{
			m_callback(instance, m_userData);
		}
	}

	private class CreateGameListener : EventListener<CreateGameCallback>
	{
		public void Fire(CreateGamePhase phase)
		{
			m_callback(phase, m_userData);
		}
	}

	private class OptionsReceivedListener : EventListener<OptionsReceivedCallback>
	{
		public void Fire()
		{
			m_callback(m_userData);
		}
	}

	private class OptionsSentListener : EventListener<OptionsSentCallback>
	{
		public void Fire(Network.Options.Option option)
		{
			m_callback(option, m_userData);
		}
	}

	private class OptionRejectedListener : EventListener<OptionRejectedCallback>
	{
		public void Fire(Network.Options.Option option)
		{
			m_callback(option, m_userData);
		}
	}

	private class EntityChoicesReceivedListener : EventListener<EntityChoicesReceivedCallback>
	{
		public void Fire(Network.EntityChoices choices, PowerTaskList preChoiceTaskList)
		{
			m_callback(choices, preChoiceTaskList, m_userData);
		}
	}

	private class EntitiesChosenReceivedListener : EventListener<EntitiesChosenReceivedCallback>
	{
		public bool Fire(Network.EntitiesChosen chosen)
		{
			return m_callback(chosen, m_userData);
		}
	}

	private class CurrentPlayerChangedListener : EventListener<CurrentPlayerChangedCallback>
	{
		public void Fire(Player player)
		{
			m_callback(player, m_userData);
		}
	}

	private class TurnChangedListener : EventListener<TurnChangedCallback>
	{
		public void Fire(int oldTurn, int newTurn)
		{
			m_callback(oldTurn, newTurn, m_userData);
		}
	}

	private class FriendlyTurnStartedListener : EventListener<FriendlyTurnStartedCallback>
	{
		public void Fire()
		{
			m_callback(m_userData);
		}
	}

	private class TurnTimerUpdateListener : EventListener<TurnTimerUpdateCallback>
	{
		public void Fire(TurnTimerUpdate update)
		{
			m_callback(update, m_userData);
		}
	}

	private class SpectatorNotifyListener : EventListener<SpectatorNotifyEventCallback>
	{
		public void Fire(SpectatorNotify notify)
		{
			m_callback(notify, m_userData);
		}
	}

	private class GameOverListener : EventListener<GameOverCallback>
	{
		public void Fire(TAG_PLAYSTATE playState)
		{
			m_callback(playState, m_userData);
		}
	}

	private class HeroChangedListener : EventListener<HeroChangedCallback>
	{
		public void Fire(Player player)
		{
			m_callback(player, m_userData);
		}
	}

	private class BusyStateChangedListener : EventListener<BusyStateChangedCallback>
	{
		public void Fire(bool isBusy)
		{
			m_callback(isBusy, m_userData);
		}
	}

	private class CantPlayListener : EventListener<CantPlayCallback>
	{
		public void Fire(Entity entity)
		{
			m_callback(entity, m_userData);
		}
	}

	private class DamageCapChangedListener : EventListener<DamageCapChangedCallback>
	{
		public void Fire(int oldValue, int newValue)
		{
			m_callback(oldValue, newValue, m_userData);
		}
	}

	private class DiabloFightPlayerIDChangedListener : EventListener<DiabloFightPlayerIDChangedCallback>
	{
		public void Fire(int oldValue, int newValue)
		{
			m_callback(oldValue, newValue, m_userData);
		}
	}

	public const int DEFAULT_SUBOPTION = -1;

	private const string INDENT = "    ";

	private const float BLOCK_REPORT_START_SEC = 10f;

	private const float BLOCK_REPORT_INTERVAL_SEC = 3f;

	private static GameState s_instance;

	private static List<GameStateInitializedListener> s_gameStateInitializedListeners;

	private static List<GameStateShutdownListener> s_gameStateShutdownListeners;

	private List<TAG_RACE> m_availableRacesInBattlegroundsExcludingAmalgam = new List<TAG_RACE>();

	private List<TAG_RACE> m_missingRacesInBattlegrounds = new List<TAG_RACE>();

	private List<TAG_RACE> m_inactiveRacesInBattlegrounds = new List<TAG_RACE>();

	private Map<int, Entity> m_entityMap = new Map<int, Entity>();

	private Map<int, Player> m_playerMap = new Map<int, Player>();

	private Map<int, SharedPlayerInfo> m_playerInfoMap = new Map<int, SharedPlayerInfo>();

	private GameEntity m_gameEntity;

	private Queue<Entity> m_removedFromGameEntities = new Queue<Entity>();

	private HashSet<int> m_removedFromGameEntityLog = new HashSet<int>();

	private CreateGamePhase m_createGamePhase;

	private Network.HistResetGame m_realTimeResetGame;

	private Network.HistTagChange m_realTimeGameOverTagChange;

	private bool m_gameOver;

	private bool m_concedeRequested;

	private bool m_restartRequested;

	private int m_maxSecretZoneSizePerPlayer;

	private int m_maxSecretsPerPlayer;

	private int m_maxQuestsPerPlayer;

	private int m_maxFriendlySlotsPerPlayer;

	private ResponseMode m_responseMode;

	private Map<int, Network.EntityChoices> m_choicesMap = new Map<int, Network.EntityChoices>();

	private Queue<QueuedChoice> m_queuedChoices = new Queue<QueuedChoice>();

	private List<Entity> m_chosenEntities = new List<Entity>();

	private Network.Options m_options;

	private SelectedOption m_selectedOption = new SelectedOption();

	private Network.Options m_lastOptions;

	private SelectedOption m_lastSelectedOption;

	private bool m_coinHasSpawned;

	private Card m_friendlyCardBeingDrawn;

	private Card m_opponentCardBeingDrawn;

	private int m_lastTurnRemindedOfFullHand;

	private bool m_usingFastActorTriggers;

	private List<CreateGameListener> m_createGameListeners = new List<CreateGameListener>();

	private List<OptionsReceivedListener> m_optionsReceivedListeners = new List<OptionsReceivedListener>();

	private List<OptionsSentListener> m_optionsSentListeners = new List<OptionsSentListener>();

	private List<OptionRejectedListener> m_optionRejectedListeners = new List<OptionRejectedListener>();

	private List<EntityChoicesReceivedListener> m_entityChoicesReceivedListeners = new List<EntityChoicesReceivedListener>();

	private List<EntitiesChosenReceivedListener> m_entitiesChosenReceivedListeners = new List<EntitiesChosenReceivedListener>();

	private List<CurrentPlayerChangedListener> m_currentPlayerChangedListeners = new List<CurrentPlayerChangedListener>();

	private List<FriendlyTurnStartedListener> m_friendlyTurnStartedListeners = new List<FriendlyTurnStartedListener>();

	private List<TurnChangedListener> m_turnChangedListeners = new List<TurnChangedListener>();

	private List<SpectatorNotifyListener> m_spectatorNotifyListeners = new List<SpectatorNotifyListener>();

	private List<GameOverListener> m_gameOverListeners = new List<GameOverListener>();

	private List<HeroChangedListener> m_heroChangedListeners = new List<HeroChangedListener>();

	private List<BusyStateChangedListener> m_busyStateChangedListeners = new List<BusyStateChangedListener>();

	private List<CantPlayListener> m_cantPlayListeners = new List<CantPlayListener>();

	private List<DamageCapChangedListener> m_damageCapChangedListeners = new List<DamageCapChangedListener>();

	private List<DiabloFightPlayerIDChangedListener> m_diabloFightPlayerIDChangedListeners = new List<DiabloFightPlayerIDChangedListener>();

	private PowerProcessor m_powerProcessor = new PowerProcessor();

	private float m_reconnectIfStuckTimer;

	private float m_lastBlockedReportTimestamp;

	private bool m_busy;

	private bool m_mulliganBusy;

	private List<Spell> m_serverBlockingSpells = new List<Spell>();

	private List<SpellController> m_serverBlockingSpellControllers = new List<SpellController>();

	private List<TurnTimerUpdateListener> m_turnTimerUpdateListeners = new List<TurnTimerUpdateListener>();

	private List<TurnTimerUpdateListener> m_mulliganTimerUpdateListeners = new List<TurnTimerUpdateListener>();

	private Map<int, TurnTimerUpdate> m_turnTimerUpdates = new Map<int, TurnTimerUpdate>();

	private AlertPopup m_waitForOpponentReconnectPopup;

	private AlertPopup.PopupInfo m_waitForOpponentReconnectPopupInfo;

	private int m_friendlyDrawCounter;

	private int m_opponentDrawCounter;

	private GameStateFrameTimeTracker m_lostFrameTimeTracker = CreateFrameTimeTracker();

	private GameStateSlushTimeTracker m_lostSlushTimeTracker = CreateSlushTimeTracker();

	private float m_clientLostTimeCatchUpThreshold;

	private bool m_useSlushTimeCatchUp;

	private bool m_restrictClientLostTimeCatchUpToLowEndDevices;

	private bool m_allowDeferredPowers = true;

	private bool m_allowBatchedPowers = true;

	private bool m_allowDiamondCards = true;

	private bool m_allowSignatureCards = true;

	private bool m_battlegroundAllowBuddies = true;

	private bool m_battlegroundsAllowQuestRewards = true;

	private bool m_battlegroundsAllowTrinkets = true;

	private string m_battlegroundMinionPool = "";

	private string m_battlegroundDenyList = "";

	private string m_battlegroundHeroArmorTierList = "";

	private string m_battlegroundsPlayerAnomaly = "";

	private bool m_printBattlegroundMinionPoolOnUpdate;

	private bool m_printBattlegroundDenyListOnUpdate;

	private bool m_printBattlegroundHeroArmorTierListUpdate;

	private bool m_printBattlegroundsAnomalyUpdate;

	private CornerSpellReplacementManager m_cornerSpellReplacementManager = new CornerSpellReplacementManager();

	public static GameState Get()
	{
		return s_instance;
	}

	public static GameState Initialize()
	{
		if (s_instance == null)
		{
			s_instance = new GameState();
			FireGameStateInitializedEvent();
			s_instance.m_powerProcessor.AddTaskEventListener(s_instance.HandleTaskTimeEvent);
		}
		return s_instance;
	}

	public static void Shutdown()
	{
		if (s_instance != null)
		{
			FireGameStateShutdownEvent();
			if (SoundManager.Get() != null)
			{
				SoundManager.Get().DestroyAll(Global.SoundCategory.FX);
				SoundManager.Get().DestroyAll(Global.SoundCategory.SPECIAL_CARD);
				SoundManager.Get().DestroyAll(Global.SoundCategory.SPECIAL_SFX);
			}
			s_instance.GetGameEntity()?.OnDecommissionGame();
			s_instance.ClearEntityMap();
			s_instance.HideWaitForOpponentReconnectPopup();
			s_instance.m_powerProcessor.RemoveTaskEventListener(s_instance.HandleTaskTimeEvent);
			s_instance = null;
		}
	}

	public void Update()
	{
		m_lostFrameTimeTracker.Update();
		m_lostSlushTimeTracker.Update();
		if (!CheckReconnectIfStuck())
		{
			m_powerProcessor.ProcessPowerQueue();
			m_lostFrameTimeTracker.AdjustAccruedLostTime(-0.016667f);
		}
	}

	public PowerProcessor GetPowerProcessor()
	{
		return m_powerProcessor;
	}

	public IGameStateTimeTracker GetTimeTracker()
	{
		if (m_useSlushTimeCatchUp)
		{
			return GetSlushTimeTracker();
		}
		return GetFrameTimeTracker();
	}

	public GameStateSlushTimeTracker GetSlushTimeTracker()
	{
		return m_lostSlushTimeTracker;
	}

	public GameStateFrameTimeTracker GetFrameTimeTracker()
	{
		return m_lostFrameTimeTracker;
	}

	public void HandleTaskTimeEvent(float diff)
	{
		m_lostSlushTimeTracker.AdjustAccruedLostTime(diff);
	}

	private static GameStateSlushTimeTracker CreateSlushTimeTracker()
	{
		return new GameStateSlushTimeTracker();
	}

	private static GameStateFrameTimeTracker CreateFrameTimeTracker()
	{
		return new GameStateFrameTimeTracker(15, 0.033333f);
	}

	public bool AreLostTimeGuardianConditionsMet()
	{
		if (m_clientLostTimeCatchUpThreshold > 0f)
		{
			if (m_restrictClientLostTimeCatchUpToLowEndDevices)
			{
				return PlatformSettings.Memory != MemoryCategory.High;
			}
			return true;
		}
		return false;
	}

	public bool AllowDeferredPowers()
	{
		return m_allowDeferredPowers;
	}

	public bool AllowBatchedPowers()
	{
		return m_allowBatchedPowers;
	}

	public bool AllowDiamondCards()
	{
		return m_allowDiamondCards;
	}

	public bool AllowSignatureCards()
	{
		return m_allowSignatureCards;
	}

	public bool BattlegroundAllowBuddies()
	{
		bool buddyEnabledGameTag = m_gameEntity == null || m_gameEntity.GetTag(GAME_TAG.BACON_BUDDY_ENABLED) != 0;
		return m_battlegroundAllowBuddies && buddyEnabledGameTag;
	}

	public bool BattlegroundsAllowQuests()
	{
		bool questEnabledGameTag = m_gameEntity == null || m_gameEntity.GetTag(GAME_TAG.BACON_QUESTS_ACTIVE) != 0;
		return m_battlegroundsAllowQuestRewards && questEnabledGameTag;
	}

	public bool BattlegroundsAllowTrinkets()
	{
		bool trinketsEnabledGameTag = m_gameEntity == null || m_gameEntity.GetTag(GAME_TAG.BACON_TRINKETS_ACTIVE) != 0;
		return m_battlegroundsAllowTrinkets && trinketsEnabledGameTag;
	}

	public bool PrintBattlegroundMinionPoolOnUpdate()
	{
		return m_printBattlegroundMinionPoolOnUpdate;
	}

	public bool PrintBattlegroundDenyListOnUpdate()
	{
		return m_printBattlegroundDenyListOnUpdate;
	}

	public void SetPrintBattlegroundMinionPoolOnUpdate(bool isPrinting)
	{
		m_printBattlegroundMinionPoolOnUpdate = isPrinting;
	}

	public void SetPrintBattlegroundDenyListOnUpdate(bool isPrinting)
	{
		m_printBattlegroundDenyListOnUpdate = isPrinting;
	}

	public void SetPrintBattlegroundHeroArmorTierListOnUpdate(bool isPrinting)
	{
		m_printBattlegroundHeroArmorTierListUpdate = isPrinting;
	}

	public void SetPrintBattlegroundAnomalyOnUpdate(bool isPrinting)
	{
		m_printBattlegroundsAnomalyUpdate = isPrinting;
	}

	public string BattlegroundDenyList()
	{
		return m_battlegroundDenyList;
	}

	public string BattlegroundMinionPool()
	{
		return m_battlegroundMinionPool;
	}

	public string BattlegroundHeroArmorTierList()
	{
		return m_battlegroundHeroArmorTierList;
	}

	public string BattlegroundsAnomaly()
	{
		return m_battlegroundHeroArmorTierList;
	}

	public bool HasPowersToProcess()
	{
		if (m_powerProcessor.GetCurrentTaskList() != null)
		{
			return true;
		}
		if (m_powerProcessor.GetPowerQueue().Count > 0)
		{
			return true;
		}
		return false;
	}

	public Entity GetEntity(int id)
	{
		m_entityMap.TryGetValue(id, out var entity);
		return entity;
	}

	public Player GetPlayer(int id)
	{
		m_playerMap.TryGetValue(id, out var player);
		return player;
	}

	public GameEntity GetGameEntity()
	{
		return m_gameEntity;
	}

	public bool GetBooleanGameOption(GameEntityOption option)
	{
		return (m_gameEntity?.GetGameOptions())?.GetBooleanOption(option) ?? false;
	}

	public string GetStringGameOption(GameEntityOption option)
	{
		return m_gameEntity?.GetGameOptions()?.GetStringOption(option);
	}

	[Conditional("UNITY_EDITOR")]
	public void DebugSetGameEntity(GameEntity gameEntity)
	{
		m_gameEntity = gameEntity;
	}

	public bool WasGameCreated()
	{
		return m_gameEntity != null;
	}

	public Player GetPlayerBySide(Player.Side playerSide)
	{
		foreach (Player player in m_playerMap.Values)
		{
			if (player.GetSide() == playerSide)
			{
				return player;
			}
		}
		return null;
	}

	public Player GetLocalSidePlayer()
	{
		bool isSpectating = SpectatorManager.Get().IsSpectatingOrWatching;
		foreach (Player player in m_playerMap.Values)
		{
			if (player.IsLocalUser())
			{
				return player;
			}
			if (isSpectating && player.GetGameAccountId() == SpectatorManager.Get().GetSpectateeFriendlySide())
			{
				return player;
			}
		}
		return null;
	}

	public List<Player> GetOpposingBackseatPlayers()
	{
		List<Player> players = new List<Player>();
		foreach (Player player in m_playerMap.Values)
		{
			if (player.GetSide() == Player.Side.OPPOSING && !player.IsTeamLeader())
			{
				players.Add(player);
			}
		}
		return players;
	}

	public List<Player> GetOpposingPlayers()
	{
		List<Player> players = new List<Player>();
		foreach (Player player in m_playerMap.Values)
		{
			if (player.GetSide() == Player.Side.OPPOSING)
			{
				players.Add(player);
			}
		}
		return players;
	}

	public int GetFriendlySideTeamId()
	{
		Player player = GetLocalSidePlayer();
		if (player != null)
		{
			int teamId = player.GetTeamId();
			if (teamId <= 0)
			{
				return player.GetPlayerId();
			}
			return teamId;
		}
		return 0;
	}

	public Player GetFriendlySidePlayer()
	{
		foreach (KeyValuePair<int, Player> item in m_playerMap)
		{
			Player player = item.Value;
			if (player.IsFriendlySide() && player.IsTeamLeader())
			{
				return player;
			}
		}
		return null;
	}

	public void HideZzzEffects()
	{
		Player myPlayer = GetFriendlySidePlayer();
		if (myPlayer != null)
		{
			ZonePlay zonePlay = myPlayer.GetBattlefieldZone();
			if (zonePlay != null)
			{
				zonePlay.HideCardZzzEffects();
			}
		}
		Player enemyPlayer = GetOpposingSidePlayer();
		if (enemyPlayer != null)
		{
			ZonePlay zonePlay2 = enemyPlayer.GetBattlefieldZone();
			if (zonePlay2 != null)
			{
				zonePlay2.HideCardZzzEffects();
			}
		}
	}

	public void UnhideZzzEffects()
	{
		Player myPlayer = GetFriendlySidePlayer();
		if (myPlayer != null)
		{
			ZonePlay zonePlay = myPlayer.GetBattlefieldZone();
			if (zonePlay != null)
			{
				zonePlay.UnhideCardZzzEffects();
			}
		}
		Player enemyPlayer = GetOpposingSidePlayer();
		if (enemyPlayer != null)
		{
			ZonePlay zonePlay2 = enemyPlayer.GetBattlefieldZone();
			if (zonePlay2 != null)
			{
				zonePlay2.UnhideCardZzzEffects();
			}
		}
	}

	public Player GetOpposingPlayer()
	{
		List<Player> backseatPlayers = GetOpposingBackseatPlayers();
		if (backseatPlayers.Count > 0)
		{
			return backseatPlayers[0];
		}
		return GetOpposingSidePlayer();
	}

	public Player GetOpposingSidePlayer()
	{
		foreach (KeyValuePair<int, Player> item in m_playerMap)
		{
			Player player = item.Value;
			if (player.IsOpposingSide() && player.IsTeamLeader())
			{
				return player;
			}
		}
		return null;
	}

	public int GetFriendlyPlayerId()
	{
		return GetFriendlySidePlayer()?.GetPlayerId() ?? 0;
	}

	public int GetOpposingPlayerId()
	{
		return GetOpposingSidePlayer()?.GetPlayerId() ?? 0;
	}

	public bool IsFriendlySidePlayerTurn()
	{
		return GetFriendlySidePlayer()?.IsCurrentPlayer() ?? false;
	}

	public bool IsLocalSidePlayerTurn()
	{
		return GetLocalSidePlayer()?.IsCurrentPlayer() ?? false;
	}

	public Player GetCurrentPlayer()
	{
		foreach (KeyValuePair<int, Player> item in m_playerMap)
		{
			Player player = item.Value;
			if (player.IsCurrentPlayer())
			{
				return player;
			}
		}
		return null;
	}

	public bool IsCurrentPlayerRevealed()
	{
		return GetCurrentPlayer()?.IsRevealed() ?? false;
	}

	public Player GetFirstOpponentPlayer(Player player)
	{
		foreach (KeyValuePair<int, Player> item in m_playerMap)
		{
			Player currPlayer = item.Value;
			if (currPlayer.GetSide() != player.GetSide())
			{
				return currPlayer;
			}
		}
		return null;
	}

	public int GetNumFriendlyMinionsInPlay(bool includeUntouchables)
	{
		return GetNumMinionsInPlay(GetFriendlySidePlayer(), includeUntouchables);
	}

	public int GetNumEnemyMinionsInPlay(bool includeUntouchables)
	{
		return GetNumMinionsInPlay(GetOpposingSidePlayer(), includeUntouchables);
	}

	private int GetNumMinionsInPlay(Player player, bool includeUntouchables)
	{
		if (player == null)
		{
			return 0;
		}
		int numMinionsInPlay = 0;
		foreach (Card card in player.GetBattlefieldZone().GetCards())
		{
			Entity cardEntity = card.GetEntity();
			if (cardEntity.GetController() == player && cardEntity.IsMinion() && (includeUntouchables || !cardEntity.HasTag(GAME_TAG.UNTOUCHABLE)))
			{
				numMinionsInPlay++;
			}
		}
		return numMinionsInPlay;
	}

	public int GetTurn()
	{
		if (m_gameEntity != null)
		{
			return m_gameEntity.GetTag(GAME_TAG.TURN);
		}
		return 0;
	}

	public bool IsTagBlockingInput()
	{
		if (m_gameEntity == null)
		{
			return false;
		}
		return m_gameEntity.HasTag(GAME_TAG.BLOCK_ALL_INPUT);
	}

	public bool IsResponsePacketBlocked()
	{
		if (IsMulliganManagerIntroActive())
		{
			return true;
		}
		if (m_gameEntity.IsMulliganActiveRealTime())
		{
			return false;
		}
		if (IsMulliganManagerActive())
		{
			return true;
		}
		if (!IsCurrentPlayerRevealed() && !IsLocalSidePlayerTurn())
		{
			return true;
		}
		if (!m_gameEntity.IsCurrentTurnRealTime())
		{
			return true;
		}
		if (!m_gameEntity.IsInputEnabled())
		{
			return true;
		}
		if (IsTurnStartManagerBlockingInput())
		{
			return true;
		}
		if (IsTagBlockingInput())
		{
			return true;
		}
		if (IsResetGamePending())
		{
			return false;
		}
		switch (m_responseMode)
		{
		case ResponseMode.NONE:
			return true;
		case ResponseMode.OPTION:
		case ResponseMode.SUB_OPTION:
		case ResponseMode.OPTION_TARGET:
			if (m_options == null)
			{
				return true;
			}
			break;
		case ResponseMode.CHOICE:
			if (GetFriendlyEntityChoices() == null)
			{
				return true;
			}
			break;
		default:
			Debug.LogWarning($"GameState.IsResponsePacketBlocked() - unhandled response mode {m_responseMode}");
			break;
		}
		return false;
	}

	public List<TAG_RACE> GetAvailableRacesInBattlegroundsExcludingAmalgam()
	{
		return m_availableRacesInBattlegroundsExcludingAmalgam;
	}

	public List<TAG_RACE> GetMissingRacesInBattlegrounds()
	{
		return m_missingRacesInBattlegrounds;
	}

	public List<TAG_RACE> GetInactiveRacesInBattlegrounds()
	{
		return m_inactiveRacesInBattlegrounds;
	}

	public Map<int, Entity> GetEntityMap()
	{
		return m_entityMap;
	}

	public Map<int, Player> GetPlayerMap()
	{
		return m_playerMap;
	}

	public Map<int, SharedPlayerInfo> GetPlayerInfoMap()
	{
		return m_playerInfoMap;
	}

	public AlertPopup GetWaitForOpponentReconnectPopup()
	{
		return m_waitForOpponentReconnectPopup;
	}

	public void AddPlayerInfo(SharedPlayerInfo playerInfo)
	{
		int playerID = playerInfo.GetPlayerId();
		if (m_playerInfoMap.ContainsKey(playerID))
		{
			Debug.LogWarning($"GameState.AddPlayerInfo() - playerInfo {playerInfo} has already been added");
		}
		else
		{
			m_playerInfoMap.Add(playerID, playerInfo);
		}
	}

	public void AddPlayer(Player player)
	{
		m_playerMap.Add(player.GetPlayerId(), player);
		m_entityMap.Add(player.GetEntityId(), player);
	}

	public void RemovePlayer(Player player)
	{
		player.Destroy();
		m_playerMap.Remove(player.GetPlayerId());
		m_entityMap.Remove(player.GetEntityId());
	}

	public int CountPlayersAlive()
	{
		int count = 0;
		foreach (SharedPlayerInfo sph in m_playerInfoMap.Values)
		{
			if (sph.GetPlayerHero() != null && sph.GetPlayerHero().GetRealTimeRemainingHP() > 0)
			{
				count++;
			}
		}
		return count;
	}

	public void AddEntity(Entity entity)
	{
		m_entityMap.Add(entity.GetEntityId(), entity);
	}

	public void RemoveEntity(Entity entity)
	{
		if (entity.IsPlayer())
		{
			RemovePlayer(entity as Player);
			return;
		}
		if (entity.IsGame())
		{
			m_gameEntity.OnDecommissionGame();
			m_gameEntity = null;
			return;
		}
		if (entity.IsAttached())
		{
			GetEntity(entity.GetAttached())?.RemoveAttachment(entity);
		}
		if (entity.IsHero())
		{
			Player player = GetPlayer(entity.GetControllerId());
			if (player != null && player.GetHero() == entity)
			{
				player.SetHero(null);
			}
		}
		else if (entity.IsHeroPower())
		{
			Player player2 = GetPlayer(entity.GetControllerId());
			if (player2 != null && player2.GetHeroPower() == entity)
			{
				player2.SetHeroPower(null);
			}
		}
		entity.Destroy();
		m_entityMap.Remove(entity.GetEntityId());
	}

	public void RemoveQueuedEntitiesFromGame()
	{
		if (m_removedFromGameEntities.Count == 0)
		{
			return;
		}
		bool removed = false;
		do
		{
			Entity entityToRemove = m_removedFromGameEntities.Peek();
			removed = AttemptRemovalOfQueuedEntity(entityToRemove);
			if (removed)
			{
				m_removedFromGameEntities.Dequeue();
				m_removedFromGameEntityLog.Add(entityToRemove.GetEntityId());
			}
		}
		while (removed && m_removedFromGameEntities.Count > 0);
	}

	public bool EntityRemovedFromGame(int entityId)
	{
		return m_removedFromGameEntityLog.Contains(entityId);
	}

	private bool AttemptRemovalOfQueuedEntity(Entity entity)
	{
		if (GetPowerProcessor().EntityHasPendingTasks(entity))
		{
			return false;
		}
		Get().RemoveEntity(entity);
		return true;
	}

	public int GetMaxSecretZoneSizePerPlayer()
	{
		return m_maxSecretZoneSizePerPlayer;
	}

	public int GetMaxSecretsPerPlayer()
	{
		return m_maxSecretsPerPlayer;
	}

	public int GetMaxQuestsPerPlayer()
	{
		return m_maxQuestsPerPlayer;
	}

	public int GetMaxFriendlySlotsPerPlayer(Entity source)
	{
		Entity player = null;
		if (source != null)
		{
			player = (source.IsPlayer() ? source : source.GetController());
		}
		if (player != null)
		{
			int maxSize = player.GetTag(GAME_TAG.MAX_SLOTS_PER_PLAYER_OVERRIDE);
			if (maxSize > 0)
			{
				return maxSize;
			}
		}
		if (GetGameEntity() != null)
		{
			int maxSize2 = GetGameEntity().GetTag(GAME_TAG.MAX_SLOTS_PER_PLAYER_OVERRIDE);
			if (maxSize2 != m_maxFriendlySlotsPerPlayer && maxSize2 > 0)
			{
				m_maxFriendlySlotsPerPlayer = maxSize2;
			}
		}
		return m_maxFriendlySlotsPerPlayer;
	}

	public bool IsBusy()
	{
		return m_busy;
	}

	public void SetBusy(bool busy)
	{
		if (m_busy != busy)
		{
			m_busy = busy;
			FireBusyStateChangedEvent(busy);
		}
	}

	public bool IsMulliganBusy()
	{
		return m_mulliganBusy;
	}

	public void SetMulliganBusy(bool busy)
	{
		m_mulliganBusy = busy;
	}

	public bool IsMulliganManagerActive()
	{
		if (MulliganManager.Get() == null)
		{
			return false;
		}
		return MulliganManager.Get().IsMulliganActive();
	}

	public bool IsMulliganManagerIntroActive()
	{
		if (MulliganManager.Get() == null)
		{
			return false;
		}
		return MulliganManager.Get().IsMulliganIntroActive();
	}

	public bool IsTurnStartManagerActive()
	{
		if (TurnStartManager.Get() == null)
		{
			return false;
		}
		return TurnStartManager.Get().IsListeningForTurnEvents();
	}

	public bool IsTurnStartManagerBlockingInput()
	{
		if (TurnStartManager.Get() == null)
		{
			return false;
		}
		return TurnStartManager.Get().IsBlockingInput();
	}

	public bool HasTheCoinBeenSpawned()
	{
		return m_coinHasSpawned;
	}

	public void NotifyOfCoinSpawn()
	{
		m_coinHasSpawned = true;
	}

	public bool IsActionStep()
	{
		if (m_gameEntity == null)
		{
			return false;
		}
		return m_gameEntity.GetTag<TAG_STEP>(GAME_TAG.STEP) == TAG_STEP.MAIN_ACTION;
	}

	public ACTION_STEP_TYPE GetActionStepType()
	{
		return (ACTION_STEP_TYPE)m_gameEntity.GetTag(GAME_TAG.ACTION_STEP_TYPE);
	}

	public bool IsFinalWrapupStep()
	{
		if (m_gameEntity == null)
		{
			return false;
		}
		return m_gameEntity.GetTag<TAG_STEP>(GAME_TAG.STEP) == TAG_STEP.FINAL_WRAPUP;
	}

	public bool IsBeginPhase()
	{
		if (m_gameEntity == null)
		{
			return false;
		}
		return GameUtils.IsBeginPhase(m_gameEntity.GetTag<TAG_STEP>(GAME_TAG.STEP));
	}

	public bool IsPastBeginPhase()
	{
		if (m_gameEntity == null)
		{
			return false;
		}
		return GameUtils.IsPastBeginPhase(m_gameEntity.GetTag<TAG_STEP>(GAME_TAG.STEP));
	}

	public bool IsMainPhase()
	{
		if (m_gameEntity == null)
		{
			return false;
		}
		return GameUtils.IsMainPhase((TAG_STEP)m_gameEntity.GetTag(GAME_TAG.STEP));
	}

	public bool IsMulliganPhase()
	{
		if (m_gameEntity == null)
		{
			return false;
		}
		return m_gameEntity.GetTag<TAG_STEP>(GAME_TAG.STEP) == TAG_STEP.BEGIN_MULLIGAN;
	}

	public bool IsMulliganPhasePending()
	{
		if (m_gameEntity == null)
		{
			return false;
		}
		if (m_gameEntity.GetTag<TAG_STEP>(GAME_TAG.NEXT_STEP) == TAG_STEP.BEGIN_MULLIGAN)
		{
			return true;
		}
		bool foundMulliganStep = false;
		int gameEntityId = m_gameEntity.GetEntityId();
		m_powerProcessor.ForEachTaskList(delegate(int queueIndex, PowerTaskList taskList)
		{
			List<PowerTask> taskList2 = taskList.GetTaskList();
			for (int i = 0; i < taskList2.Count; i++)
			{
				if (taskList2[i].GetPower() is Network.HistTagChange histTagChange && histTagChange.Entity == gameEntityId)
				{
					GAME_TAG tag = (GAME_TAG)histTagChange.Tag;
					if ((tag == GAME_TAG.STEP || tag == GAME_TAG.NEXT_STEP) && histTagChange.Value == 4)
					{
						foundMulliganStep = true;
						break;
					}
				}
			}
		});
		return foundMulliganStep;
	}

	public bool IsMulliganPhaseNowOrPending()
	{
		if (IsMulliganPhase())
		{
			return true;
		}
		if (IsMulliganPhasePending())
		{
			return true;
		}
		return false;
	}

	public bool IsResetGamePending()
	{
		return m_realTimeResetGame != null;
	}

	public CreateGamePhase GetCreateGamePhase()
	{
		return m_createGamePhase;
	}

	public bool IsGameCreating()
	{
		return m_createGamePhase == CreateGamePhase.CREATING;
	}

	public bool IsGameCreated()
	{
		return m_createGamePhase == CreateGamePhase.CREATED;
	}

	public bool IsGameCreatedOrCreating()
	{
		if (!IsGameCreated())
		{
			return IsGameCreating();
		}
		return true;
	}

	public bool WasConcedeRequested()
	{
		return m_concedeRequested;
	}

	public void Concede()
	{
		if (!m_concedeRequested)
		{
			m_concedeRequested = true;
			Network.Get().Concede();
		}
	}

	public bool WasRestartRequested()
	{
		return m_restartRequested;
	}

	public void Restart()
	{
		if (!m_restartRequested)
		{
			m_restartRequested = true;
			if (IsGameOverNowOrPending())
			{
				CheckRestartOnRealTimeGameOver();
			}
			else
			{
				Concede();
			}
		}
	}

	private void CheckRestartOnRealTimeGameOver()
	{
		if (WasRestartRequested())
		{
			m_gameOver = true;
			m_realTimeGameOverTagChange = null;
			Network.Get().DisconnectFromGameServer(Network.DisconnectReason.GameState_Restart);
			NotificationManager.Get().DestroyAllNotificationsNowWithNoAnim();
			ReconnectMgr.Get().SetBypassGameReconnect(shouldBypass: true);
			GameMgr.Get().RestartGame();
		}
	}

	public bool IsGameOver()
	{
		return m_gameOver;
	}

	public bool IsGameOverPending()
	{
		return m_realTimeGameOverTagChange != null;
	}

	public bool IsGameOverNowOrPending()
	{
		if (IsGameOver())
		{
			return true;
		}
		if (IsGameOverPending())
		{
			return true;
		}
		return false;
	}

	public Network.HistTagChange GetRealTimeGameOverTagChange()
	{
		return m_realTimeGameOverTagChange;
	}

	public void ShowEnemyTauntCharacters()
	{
		List<Zone> zones = ZoneMgr.Get().GetZones();
		for (int i = 0; i < zones.Count; i++)
		{
			Zone zone = zones[i];
			if (zone.m_ServerTag != TAG_ZONE.PLAY || zone.m_Side != Player.Side.OPPOSING)
			{
				continue;
			}
			List<Card> cards = zone.GetCards();
			for (int j = 0; j < cards.Count; j++)
			{
				Card card = cards[j];
				Entity entity = card.GetEntity();
				if (entity.HasTaunt() && !entity.IsStealthed())
				{
					card.DoTauntNotification();
				}
			}
		}
	}

	public void GetTauntCounts(Player player, out int minionCount, out int heroCount)
	{
		minionCount = 0;
		heroCount = 0;
		List<Zone> zones = ZoneMgr.Get().GetZones();
		for (int i = 0; i < zones.Count; i++)
		{
			Zone zone = zones[i];
			if (zone.m_ServerTag != TAG_ZONE.PLAY || player != zone.GetController())
			{
				continue;
			}
			List<Card> cards = zone.GetCards();
			for (int j = 0; j < cards.Count; j++)
			{
				Entity entity = cards[j].GetEntity();
				if (entity.HasTaunt() && !entity.IsStealthed())
				{
					switch (entity.GetCardType())
					{
					case TAG_CARDTYPE.MINION:
						minionCount++;
						break;
					case TAG_CARDTYPE.HERO:
						heroCount++;
						break;
					}
				}
			}
		}
	}

	public Card GetFriendlyCardBeingDrawn()
	{
		return m_friendlyCardBeingDrawn;
	}

	public void SetFriendlyCardBeingDrawn(Card card)
	{
		m_friendlyCardBeingDrawn = card;
	}

	public Card GetOpponentCardBeingDrawn()
	{
		return m_opponentCardBeingDrawn;
	}

	public void SetOpponentCardBeingDrawn(Card card)
	{
		m_opponentCardBeingDrawn = card;
	}

	public bool IsBeingDrawn(Card card)
	{
		if (card == m_friendlyCardBeingDrawn)
		{
			return true;
		}
		if (card == m_opponentCardBeingDrawn)
		{
			return true;
		}
		return false;
	}

	public bool ClearCardBeingDrawn(Card card)
	{
		if (card == m_friendlyCardBeingDrawn)
		{
			m_friendlyCardBeingDrawn = null;
			return true;
		}
		if (card == m_opponentCardBeingDrawn)
		{
			m_opponentCardBeingDrawn = null;
			return true;
		}
		return false;
	}

	public int GetLastTurnRemindedOfFullHand()
	{
		return m_lastTurnRemindedOfFullHand;
	}

	public void SetLastTurnRemindedOfFullHand(int turn)
	{
		m_lastTurnRemindedOfFullHand = turn;
	}

	public bool IsUsingFastActorTriggers()
	{
		GameEntity gameEntity = GetGameEntity();
		if (gameEntity != null && gameEntity.HasTag(GAME_TAG.ALWAYS_USE_FAST_ACTOR_TRIGGERS))
		{
			return true;
		}
		return m_usingFastActorTriggers;
	}

	public void SetUsingFastActorTriggers(bool enable)
	{
		m_usingFastActorTriggers = enable;
	}

	public bool HasHandPlays()
	{
		if (m_options == null)
		{
			return false;
		}
		foreach (Network.Options.Option option in m_options.List)
		{
			if (option.Type != Network.Options.Option.OptionType.POWER)
			{
				continue;
			}
			Entity entity = GetEntity(option.Main.ID);
			if (entity != null)
			{
				Card card = entity.GetCard();
				if (!(card == null) && !(card.GetZone() as ZoneHand == null))
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool CanShowScoreScreen()
	{
		if (HasScoreLabels(m_gameEntity))
		{
			return true;
		}
		if (HasScoreLabels(GetFriendlySidePlayer()))
		{
			return true;
		}
		return false;
	}

	private bool HasScoreLabels(Entity entity)
	{
		if (entity.HasTag(GAME_TAG.SCORE_LABELID_1))
		{
			return true;
		}
		if (entity.HasTag(GAME_TAG.SCORE_LABELID_2))
		{
			return true;
		}
		if (entity.HasTag(GAME_TAG.SCORE_LABELID_3))
		{
			return true;
		}
		if (entity.HasTag(GAME_TAG.SCORE_FOOTERID))
		{
			return true;
		}
		return false;
	}

	public int GetFriendlyCardDrawCounter()
	{
		return m_friendlyDrawCounter;
	}

	public void IncrementFriendlyCardDrawCounter()
	{
		m_friendlyDrawCounter++;
	}

	public void ResetFriendlyCardDrawCounter()
	{
		m_friendlyDrawCounter = 0;
	}

	public int GetOpponentCardDrawCounter()
	{
		return m_opponentDrawCounter;
	}

	public void IncrementOpponentCardDrawCounter()
	{
		m_opponentDrawCounter++;
	}

	public void ResetOpponentCardDrawCounter()
	{
		m_opponentDrawCounter = 0;
	}

	private void PreprocessRealTimeTagChange(Entity entity, Network.HistTagChange change)
	{
		switch ((GAME_TAG)change.Tag)
		{
		case GAME_TAG.PLAYSTATE:
			if (GameUtils.IsGameOverTag(change.Entity, change.Tag, change.Value))
			{
				OnRealTimeGameOver(change);
			}
			break;
		case GAME_TAG.WAIT_FOR_PLAYER_RECONNECT_PERIOD:
			HandleWaitForOpponentReconnectPeriod(change.Value);
			break;
		case GAME_TAG.CANT_PLAY:
			if (change.Value > 0)
			{
				OnCantPlay(entity);
			}
			break;
		}
	}

	private void HandleWaitForOpponentReconnectPeriod(int periodInSeconds)
	{
		m_gameEntity.SetTag(GAME_TAG.WAIT_FOR_PLAYER_RECONNECT_PERIOD, periodInSeconds);
		if (periodInSeconds > 0)
		{
			ShowWaitForOpponentReconnectPopup(periodInSeconds);
			TurnTimerUpdate update = new TurnTimerUpdate();
			update.SetSecondsRemaining(float.PositiveInfinity);
			update.SetEndTimestamp(float.PositiveInfinity);
			update.SetShow(show: false);
			TriggerTurnTimerUpdate(update);
		}
		else
		{
			HideWaitForOpponentReconnectPopup();
		}
		GameMgr.Get().UpdatePresence();
	}

	private void ShowWaitForOpponentReconnectPopup(int periodInSeconds)
	{
		if (m_waitForOpponentReconnectPopupInfo == null)
		{
			m_waitForOpponentReconnectPopupInfo = new AlertPopup.PopupInfo();
			m_waitForOpponentReconnectPopupInfo.m_headerText = GameStrings.Get("GLOBAL_WAIT_FOR_OPPONENT_RECONNECT_HEADER");
			m_waitForOpponentReconnectPopupInfo.m_showAlertIcon = false;
			m_waitForOpponentReconnectPopupInfo.m_responseDisplay = AlertPopup.ResponseDisplay.NONE;
			m_waitForOpponentReconnectPopupInfo.m_responseUserData = periodInSeconds;
			m_waitForOpponentReconnectPopupInfo.m_alertTextAlignment = UberText.AlignmentOptions.Center;
			m_waitForOpponentReconnectPopupInfo.m_layerToUse = GameLayer.UI;
			DialogManager.Get().ShowPopup(m_waitForOpponentReconnectPopupInfo, OnWaitForOpponentReconnectPopupProcessed);
			if (Gameplay.Get() != null)
			{
				IncreaseWaitForOpponentReconnectPeriod(Gameplay.Get().WaitForOpponentToken).Forget();
			}
		}
		else
		{
			UpdateWaitForOpponentReconnectPopup(periodInSeconds);
		}
	}

	private bool OnWaitForOpponentReconnectPopupProcessed(DialogBase dialog, object userData)
	{
		m_waitForOpponentReconnectPopup = (AlertPopup)dialog;
		if (m_waitForOpponentReconnectPopupInfo != null)
		{
			UpdateWaitForOpponentReconnectPopup((int)m_waitForOpponentReconnectPopupInfo.m_responseUserData);
			return true;
		}
		return false;
	}

	private void HideWaitForOpponentReconnectPopup()
	{
		if (Gameplay.Get() != null)
		{
			Gameplay.Get().StopIncreaseWaitForOpponentReconnectPeriod();
		}
		if (m_waitForOpponentReconnectPopup != null)
		{
			m_waitForOpponentReconnectPopup.Hide();
		}
		m_waitForOpponentReconnectPopup = null;
		m_waitForOpponentReconnectPopupInfo = null;
	}

	private void UpdateWaitForOpponentReconnectPopup(int periodInSeconds)
	{
		m_waitForOpponentReconnectPopupInfo.m_responseUserData = periodInSeconds;
		int mins = periodInSeconds / 60;
		int secs = periodInSeconds % 60;
		string bodyTextKey = (GameMgr.Get().IsSpectator() ? "GLOBAL_WAIT_FOR_OPPONENT_RECONNECT_SPECTATOR" : "GLOBAL_WAIT_FOR_OPPONENT_RECONNECT");
		m_waitForOpponentReconnectPopupInfo.m_text = string.Format(GameStrings.Get(bodyTextKey), mins, secs);
		if (m_waitForOpponentReconnectPopup != null)
		{
			m_waitForOpponentReconnectPopup.UpdateInfo(m_waitForOpponentReconnectPopupInfo);
		}
	}

	private async UniTaskVoid IncreaseWaitForOpponentReconnectPeriod(CancellationToken token)
	{
		while (true)
		{
			await UniTask.Delay(TimeSpan.FromSeconds(1.0), ignoreTimeScale: false, PlayerLoopTiming.Update, token);
			if (m_waitForOpponentReconnectPopupInfo == null)
			{
				break;
			}
			int periodInSeconds = (int)m_waitForOpponentReconnectPopupInfo.m_responseUserData;
			UpdateWaitForOpponentReconnectPopup(periodInSeconds + 1);
		}
	}

	private void PreprocessTagChange(Entity entity, TagDelta change)
	{
		switch ((GAME_TAG)change.tag)
		{
		case GAME_TAG.CURRENT_PLAYER:
			if (change.newValue == 1)
			{
				Player player = (Player)entity;
				OnCurrentPlayerChanged(player);
			}
			break;
		case GAME_TAG.TURN:
			OnTurnChanged(change.oldValue, change.newValue);
			break;
		case GAME_TAG.BACON_COMBAT_DAMAGE_CAP:
			OnDamageCapChanged(change.oldValue, change.newValue);
			break;
		case GAME_TAG.BACON_DIABLO_FIGHT_DIABLO_PLAYER_ID:
			OnDiabloFightPlayerIDChanged(change.oldValue, change.newValue);
			break;
		case GAME_TAG.PLAYSTATE:
			if (GameUtils.IsGameOverTag((Player)entity, change.tag, change.newValue))
			{
				OnGameOver((TAG_PLAYSTATE)change.newValue);
			}
			break;
		}
	}

	private void PreprocessTagListChange(Entity entity, Network.HistTagListChange netChange)
	{
		if (entity != null && netChange != null && netChange.Tag == 3677 && entity.IsControlledByFriendlySidePlayer())
		{
			ZoneHand friendlyHand = GetFriendlySidePlayer().GetHandZone();
			if (!(friendlyHand == null))
			{
				friendlyHand.DirtyReplacementsWhenPlayedHintSpell();
			}
		}
	}

	private void PreprocessEarlyConcedeTagChange(Entity entity, TagDelta change)
	{
		if (change.tag == 17 && GameUtils.IsGameOverTag((Player)entity, change.tag, change.newValue))
		{
			OnGameOver((TAG_PLAYSTATE)change.newValue);
		}
	}

	private void ProcessEarlyConcedeTagChange(Entity entity, TagDelta change)
	{
		if (change.tag == 17)
		{
			entity.OnTagChanged(change);
		}
	}

	private void OnRealTimeGameOver(Network.HistTagChange change)
	{
		m_realTimeGameOverTagChange = change;
		if (Network.ShouldBeConnectedToAurora() && Network.IsLoggedIn())
		{
			BnetPresenceMgr.Get().SetPresenceSpectatorJoinInfo(null);
		}
		SpectatorManager.Get().OnRealTimeGameOver();
		CheckRestartOnRealTimeGameOver();
	}

	private void OnGameOver(TAG_PLAYSTATE playState)
	{
		m_gameOver = true;
		m_realTimeGameOverTagChange = null;
		m_gameEntity.NotifyOfGameOver(playState);
		FireGameOverEvent(playState);
		HideWaitForOpponentReconnectPopup();
		GameMgr.Get().LastGameData.GameResult = playState;
		if (GetFriendlySidePlayer() != null && GetFriendlySidePlayer().GetHero() != null)
		{
			GameMgr.Get().LastGameData.BattlegroundsLeaderboardPlace = GetFriendlySidePlayer().GetHero().GetRealTimePlayerLeaderboardPlace();
		}
	}

	public void FakeConceded()
	{
		OnGameOver(TAG_PLAYSTATE.LOST);
	}

	private void OnCurrentPlayerChanged(Player player)
	{
		FireCurrentPlayerChangedEvent(player);
	}

	private void OnTurnChanged(int oldTurn, int newTurn)
	{
		OnTurnChanged_TurnTimer(oldTurn, newTurn);
		FireTurnChangedEvent(oldTurn, newTurn);
	}

	private void OnDamageCapChanged(int oldValue, int newValue)
	{
		FireDamageCapChangedEvent(oldValue, newValue);
	}

	private void OnDiabloFightPlayerIDChanged(int oldValue, int newValue)
	{
		FireDiabloFightPlayerIDChangedEvent(oldValue, newValue);
	}

	public IEnumerator RejectUnresolvedChangesAfterDelay()
	{
		yield return new WaitForSecondsRealtime(1f);
		RejectUnresolvedOptions();
	}

	private void RejectUnresolvedOptions()
	{
		if (m_lastSelectedOption != null && m_lastOptions != null && ZoneMgr.Get().HasUnresolvedLocalChange())
		{
			Get().OnOptionRejected(m_lastOptions.ID);
		}
	}

	private void OnCantPlay(Entity entity)
	{
		FireCantPlayEvent(entity);
	}

	public void AddServerBlockingSpell(Spell spell)
	{
		if (!(spell == null) && !m_serverBlockingSpells.Contains(spell))
		{
			m_serverBlockingSpells.Add(spell);
		}
	}

	public bool RemoveServerBlockingSpell(Spell spell)
	{
		return m_serverBlockingSpells.Remove(spell);
	}

	public void AddServerBlockingSpellController(SpellController spellController)
	{
		if (!(spellController == null) && !m_serverBlockingSpellControllers.Contains(spellController))
		{
			m_serverBlockingSpellControllers.Add(spellController);
		}
	}

	public bool RemoveServerBlockingSpellController(SpellController spellController)
	{
		return m_serverBlockingSpellControllers.Remove(spellController);
	}

	public void DebugNukeServerBlocks()
	{
		while (m_serverBlockingSpells.Count > 0)
		{
			m_serverBlockingSpells[0].OnSpellFinished();
		}
		while (m_serverBlockingSpellControllers.Count > 0)
		{
			m_serverBlockingSpellControllers[0].ForceKill();
		}
		m_powerProcessor.ForceStopHistoryBlocking();
		m_busy = false;
	}

	private bool IsBlockingPowerProcessor()
	{
		if (m_serverBlockingSpells.Count > 0)
		{
			return true;
		}
		if (m_serverBlockingSpellControllers.Count > 0)
		{
			return true;
		}
		if (m_powerProcessor.IsHistoryBlocking())
		{
			return true;
		}
		return false;
	}

	private bool ShouldAdvanceReconnectIfStuckTimer()
	{
		m_serverBlockingSpells.RemoveAll((Spell item) => item == null);
		foreach (Spell serverBlockingSpell in m_serverBlockingSpells)
		{
			if (serverBlockingSpell.ShouldReconnectIfStuck())
			{
				return true;
			}
		}
		foreach (SpellController serverBlockingSpellController in m_serverBlockingSpellControllers)
		{
			if (serverBlockingSpellController.ShouldReconnectIfStuck())
			{
				return true;
			}
		}
		if (m_powerProcessor.IsHistoryBlocking())
		{
			return true;
		}
		return false;
	}

	public bool MustWaitForChoices()
	{
		if (!ChoiceCardMgr.Get().HasChoices())
		{
			return false;
		}
		PowerProcessor powerProcessor = Get().GetPowerProcessor();
		if (powerProcessor.HasGameOverTaskList())
		{
			return false;
		}
		foreach (int playerId in Get().GetPlayerMap().Keys)
		{
			PowerTaskList preChoiceTaskList = ChoiceCardMgr.Get().GetPreChoiceTaskList(playerId);
			if (preChoiceTaskList != null && !powerProcessor.HasTaskList(preChoiceTaskList))
			{
				return true;
			}
		}
		return false;
	}

	public bool CanProcessPowerQueue()
	{
		if (IsBlockingPowerProcessor())
		{
			return false;
		}
		if (IsBusy())
		{
			return false;
		}
		if (MustWaitForChoices())
		{
			return false;
		}
		if (m_powerProcessor.GetCurrentTaskList() != null)
		{
			return false;
		}
		if (m_powerProcessor.GetPowerQueue().Count == 0)
		{
			return false;
		}
		if (WasRestartRequested())
		{
			return false;
		}
		return true;
	}

	private bool CheckReconnectIfStuck()
	{
		if (!ShouldAdvanceReconnectIfStuckTimer())
		{
			m_reconnectIfStuckTimer = 0f;
			return false;
		}
		m_reconnectIfStuckTimer += Time.deltaTime;
		if (ReconnectIfStuck())
		{
			return true;
		}
		ReportStuck();
		return true;
	}

	private bool ReconnectIfStuck()
	{
		Network.GameSetup gameSetup = GameMgr.Get().GetGameSetup();
		if (gameSetup.DisconnectWhenStuckSeconds != 0 && m_reconnectIfStuckTimer < (float)gameSetup.DisconnectWhenStuckSeconds)
		{
			return false;
		}
		string timeString = TimeUtils.GetDevElapsedTimeString(m_reconnectIfStuckTimer);
		string causes = BuildServerBlockingCausesString();
		Log.Power.PrintWarning("GameState.ReconnectIfStuck() - Blocked more than {0}. Cause:\n{1}", timeString, causes);
		PerformanceAnalytics.Get()?.ReconnectStart("STUCK");
		Network.Get().DisconnectFromGameServer(Network.DisconnectReason.GameState_Reconnect);
		return true;
	}

	private void ReportStuck()
	{
		if (!(m_reconnectIfStuckTimer < 10f))
		{
			float timestamp = Time.realtimeSinceStartup;
			if (!(timestamp - m_lastBlockedReportTimestamp < 3f))
			{
				m_lastBlockedReportTimestamp = timestamp;
				string timeString = TimeUtils.GetDevElapsedTimeString(m_reconnectIfStuckTimer);
				string causes = BuildServerBlockingCausesString();
				Log.Power.PrintWarning("GameState.ReportStuck() - Stuck for {0}. {1}", timeString, causes);
			}
		}
	}

	private string BuildServerBlockingCausesString()
	{
		StringBuilder builder = new StringBuilder();
		int sectionCount = 0;
		AppendServerBlockingSection(builder, "Spells:", m_serverBlockingSpells, AppendServerBlockingSpell, ref sectionCount);
		AppendServerBlockingSection(builder, "SpellControllers:", m_serverBlockingSpellControllers, AppendServerBlockingSpellController, ref sectionCount);
		AppendServerBlockingHistory(builder, ref sectionCount);
		if (m_busy)
		{
			if (sectionCount > 0)
			{
				builder.Append(' ');
			}
			builder.Append("Busy=true");
			sectionCount++;
		}
		return builder.ToString();
	}

	private void AppendServerBlockingSection<T>(StringBuilder builder, string sectionPrefix, List<T> items, AppendBlockingServerItemCallback<T> itemCallback, ref int sectionCount) where T : Component
	{
		if (items.Count == 0)
		{
			return;
		}
		if (sectionCount > 0)
		{
			builder.Append(' ');
		}
		builder.Append('{');
		builder.Append(sectionPrefix);
		for (int i = 0; i < items.Count; i++)
		{
			builder.Append(' ');
			if (itemCallback == null)
			{
				builder.Append(items[i].name);
			}
			else
			{
				itemCallback(builder, items[i]);
			}
		}
		builder.Append('}');
		sectionCount++;
	}

	private void AppendServerBlockingSpell(StringBuilder builder, Spell spell)
	{
		if (spell == null)
		{
			builder.Append("[null Spell (The Spell object may have been destroyed prematurely)]");
			return;
		}
		builder.Append('[');
		builder.Append(spell.name);
		builder.Append(' ');
		builder.AppendFormat("Source: {0}", spell.GetSource());
		builder.Append(' ');
		builder.Append("Targets:");
		List<GameObject> targets = spell.GetTargets();
		if (targets.Count == 0)
		{
			builder.Append(' ');
			builder.Append("none");
		}
		else
		{
			for (int i = 0; i < targets.Count; i++)
			{
				builder.Append(' ');
				GameObject target = targets[i];
				builder.Append(target.ToString());
			}
		}
		builder.Append(']');
	}

	private void AppendServerBlockingSpellController(StringBuilder builder, SpellController spellController)
	{
		builder.Append('[');
		builder.Append(spellController.name);
		builder.Append(' ');
		builder.AppendFormat("Source: {0}", spellController.GetSource());
		builder.Append(' ');
		builder.Append("Targets:");
		List<Card> targets = spellController.GetTargets();
		if (targets.Count == 0)
		{
			builder.Append(' ');
			builder.Append("none");
		}
		else
		{
			for (int i = 0; i < targets.Count; i++)
			{
				builder.Append(' ');
				Card target = targets[i];
				builder.Append(target.ToString());
			}
		}
		builder.Append(']');
	}

	private void AppendServerBlockingHistory(StringBuilder builder, ref int sectionCount)
	{
		if (m_powerProcessor.IsHistoryBlocking())
		{
			Entity pendingBigCardEntity = HistoryManager.Get().GetPendingBigCardEntity();
			PowerTaskList historyBlockingTaskList = m_powerProcessor.GetHistoryBlockingTaskList();
			PowerTaskList currentTaskList = m_powerProcessor.GetCurrentTaskList();
			if (sectionCount > 0)
			{
				builder.Append(' ');
			}
			builder.Append("History: ");
			builder.Append('{');
			builder.AppendFormat("PendingBigCard: {0}", pendingBigCardEntity);
			builder.Append(' ');
			builder.AppendFormat("BlockingTaskList: ");
			PrintBlockingTaskList(builder, historyBlockingTaskList);
			builder.Append(' ');
			builder.AppendFormat("CurrentTaskList: ");
			PrintBlockingTaskList(builder, currentTaskList);
			builder.Append('}');
			sectionCount++;
		}
	}

	public static bool RegisterGameStateInitializedListener(GameStateInitializedCallback callback, object userData = null)
	{
		if (callback == null)
		{
			return false;
		}
		GameStateInitializedListener listener = new GameStateInitializedListener();
		listener.SetCallback(callback);
		listener.SetUserData(userData);
		if (s_gameStateInitializedListeners == null)
		{
			s_gameStateInitializedListeners = new List<GameStateInitializedListener>();
		}
		else if (s_gameStateInitializedListeners.Contains(listener))
		{
			return false;
		}
		s_gameStateInitializedListeners.Add(listener);
		return true;
	}

	public static bool UnregisterGameStateInitializedListener(GameStateInitializedCallback callback, object userData = null)
	{
		if (callback == null || s_gameStateInitializedListeners == null)
		{
			return false;
		}
		GameStateInitializedListener listener = new GameStateInitializedListener();
		listener.SetCallback(callback);
		listener.SetUserData(userData);
		return s_gameStateInitializedListeners.Remove(listener);
	}

	public static bool RegisterGameStateShutdownListener(GameStateShutdownCallback callback, object userData = null)
	{
		if (callback == null)
		{
			return false;
		}
		GameStateShutdownListener listener = new GameStateShutdownListener();
		listener.SetCallback(callback);
		listener.SetUserData(userData);
		if (s_gameStateShutdownListeners == null)
		{
			s_gameStateShutdownListeners = new List<GameStateShutdownListener>();
		}
		else if (s_gameStateShutdownListeners.Contains(listener))
		{
			return false;
		}
		s_gameStateShutdownListeners.Add(listener);
		return true;
	}

	public static bool UnregisterGameStateShutdownListener(GameStateShutdownCallback callback, object userData = null)
	{
		if (callback == null || s_gameStateShutdownListeners == null)
		{
			return false;
		}
		GameStateShutdownListener listener = new GameStateShutdownListener();
		listener.SetCallback(callback);
		listener.SetUserData(userData);
		return s_gameStateShutdownListeners.Remove(listener);
	}

	public bool RegisterCreateGameListener(CreateGameCallback callback)
	{
		return RegisterCreateGameListener(callback, null);
	}

	public bool RegisterCreateGameListener(CreateGameCallback callback, object userData)
	{
		CreateGameListener listener = new CreateGameListener();
		listener.SetCallback(callback);
		listener.SetUserData(userData);
		if (m_createGameListeners.Contains(listener))
		{
			return false;
		}
		m_createGameListeners.Add(listener);
		return true;
	}

	public bool UnregisterCreateGameListener(CreateGameCallback callback)
	{
		return UnregisterCreateGameListener(callback, null);
	}

	public bool UnregisterCreateGameListener(CreateGameCallback callback, object userData)
	{
		CreateGameListener listener = new CreateGameListener();
		listener.SetCallback(callback);
		listener.SetUserData(userData);
		return m_createGameListeners.Remove(listener);
	}

	public bool RegisterOptionsReceivedListener(OptionsReceivedCallback callback)
	{
		return RegisterOptionsReceivedListener(callback, null);
	}

	public bool RegisterOptionsReceivedListener(OptionsReceivedCallback callback, object userData)
	{
		OptionsReceivedListener listener = new OptionsReceivedListener();
		listener.SetCallback(callback);
		listener.SetUserData(userData);
		if (m_optionsReceivedListeners.Contains(listener))
		{
			return false;
		}
		m_optionsReceivedListeners.Add(listener);
		return true;
	}

	public bool UnregisterOptionsReceivedListener(OptionsReceivedCallback callback)
	{
		return UnregisterOptionsReceivedListener(callback, null);
	}

	public bool UnregisterOptionsReceivedListener(OptionsReceivedCallback callback, object userData)
	{
		OptionsReceivedListener listener = new OptionsReceivedListener();
		listener.SetCallback(callback);
		listener.SetUserData(userData);
		return m_optionsReceivedListeners.Remove(listener);
	}

	public bool RegisterOptionsSentListener(OptionsSentCallback callback, object userData = null)
	{
		OptionsSentListener listener = new OptionsSentListener();
		listener.SetCallback(callback);
		listener.SetUserData(userData);
		if (m_optionsSentListeners.Contains(listener))
		{
			return false;
		}
		m_optionsSentListeners.Add(listener);
		return true;
	}

	public bool UnregisterOptionsSentListener(OptionsSentCallback callback, object userData = null)
	{
		OptionsSentListener listener = new OptionsSentListener();
		listener.SetCallback(callback);
		listener.SetUserData(userData);
		return m_optionsSentListeners.Remove(listener);
	}

	public bool RegisterOptionRejectedListener(OptionRejectedCallback callback, object userData = null)
	{
		OptionRejectedListener listener = new OptionRejectedListener();
		listener.SetCallback(callback);
		listener.SetUserData(userData);
		if (m_optionRejectedListeners.Contains(listener))
		{
			return false;
		}
		m_optionRejectedListeners.Add(listener);
		return true;
	}

	public bool UnregisterOptionRejectedListener(OptionRejectedCallback callback, object userData = null)
	{
		OptionRejectedListener listener = new OptionRejectedListener();
		listener.SetCallback(callback);
		listener.SetUserData(userData);
		return m_optionRejectedListeners.Remove(listener);
	}

	public bool RegisterEntityChoicesReceivedListener(EntityChoicesReceivedCallback callback)
	{
		return RegisterEntityChoicesReceivedListener(callback, null);
	}

	public bool RegisterEntityChoicesReceivedListener(EntityChoicesReceivedCallback callback, object userData)
	{
		EntityChoicesReceivedListener listener = new EntityChoicesReceivedListener();
		listener.SetCallback(callback);
		listener.SetUserData(userData);
		if (m_entityChoicesReceivedListeners.Contains(listener))
		{
			return false;
		}
		m_entityChoicesReceivedListeners.Add(listener);
		return true;
	}

	public bool UnregisterEntityChoicesReceivedListener(EntityChoicesReceivedCallback callback)
	{
		return UnregisterEntityChoicesReceivedListener(callback, null);
	}

	public bool UnregisterEntityChoicesReceivedListener(EntityChoicesReceivedCallback callback, object userData)
	{
		EntityChoicesReceivedListener listener = new EntityChoicesReceivedListener();
		listener.SetCallback(callback);
		listener.SetUserData(userData);
		return m_entityChoicesReceivedListeners.Remove(listener);
	}

	public bool RegisterEntitiesChosenReceivedListener(EntitiesChosenReceivedCallback callback)
	{
		return RegisterEntitiesChosenReceivedListener(callback, null);
	}

	public bool RegisterEntitiesChosenReceivedListener(EntitiesChosenReceivedCallback callback, object userData)
	{
		EntitiesChosenReceivedListener listener = new EntitiesChosenReceivedListener();
		listener.SetCallback(callback);
		listener.SetUserData(userData);
		if (m_entitiesChosenReceivedListeners.Contains(listener))
		{
			return false;
		}
		m_entitiesChosenReceivedListeners.Add(listener);
		return true;
	}

	public bool UnregisterEntitiesChosenReceivedListener(EntitiesChosenReceivedCallback callback)
	{
		return UnregisterEntitiesChosenReceivedListener(callback, null);
	}

	public bool UnregisterEntitiesChosenReceivedListener(EntitiesChosenReceivedCallback callback, object userData)
	{
		EntitiesChosenReceivedListener listener = new EntitiesChosenReceivedListener();
		listener.SetCallback(callback);
		listener.SetUserData(userData);
		return m_entitiesChosenReceivedListeners.Remove(listener);
	}

	public bool RegisterCurrentPlayerChangedListener(CurrentPlayerChangedCallback callback)
	{
		return RegisterCurrentPlayerChangedListener(callback, null);
	}

	public bool RegisterCurrentPlayerChangedListener(CurrentPlayerChangedCallback callback, object userData)
	{
		CurrentPlayerChangedListener listener = new CurrentPlayerChangedListener();
		listener.SetCallback(callback);
		listener.SetUserData(userData);
		if (m_currentPlayerChangedListeners.Contains(listener))
		{
			return false;
		}
		m_currentPlayerChangedListeners.Add(listener);
		return true;
	}

	public bool UnregisterCurrentPlayerChangedListener(CurrentPlayerChangedCallback callback)
	{
		return UnregisterCurrentPlayerChangedListener(callback, null);
	}

	public bool UnregisterCurrentPlayerChangedListener(CurrentPlayerChangedCallback callback, object userData)
	{
		CurrentPlayerChangedListener listener = new CurrentPlayerChangedListener();
		listener.SetCallback(callback);
		listener.SetUserData(userData);
		return m_currentPlayerChangedListeners.Remove(listener);
	}

	public bool RegisterTurnChangedListener(TurnChangedCallback callback)
	{
		return RegisterTurnChangedListener(callback, null);
	}

	public bool RegisterTurnChangedListener(TurnChangedCallback callback, object userData)
	{
		TurnChangedListener listener = new TurnChangedListener();
		listener.SetCallback(callback);
		listener.SetUserData(userData);
		if (m_turnChangedListeners.Contains(listener))
		{
			return false;
		}
		m_turnChangedListeners.Add(listener);
		return true;
	}

	public bool UnregisterTurnChangedListener(TurnChangedCallback callback)
	{
		return UnregisterTurnChangedListener(callback, null);
	}

	public bool UnregisterTurnChangedListener(TurnChangedCallback callback, object userData)
	{
		TurnChangedListener listener = new TurnChangedListener();
		listener.SetCallback(callback);
		listener.SetUserData(userData);
		return m_turnChangedListeners.Remove(listener);
	}

	public bool RegisterDamageCapChangedListener(DamageCapChangedCallback callback)
	{
		return RegisterDamageCapChangedListener(callback, null);
	}

	public bool RegisterDamageCapChangedListener(DamageCapChangedCallback callback, object userData)
	{
		DamageCapChangedListener listener = new DamageCapChangedListener();
		listener.SetCallback(callback);
		listener.SetUserData(userData);
		if (m_damageCapChangedListeners.Contains(listener))
		{
			return false;
		}
		m_damageCapChangedListeners.Add(listener);
		return true;
	}

	public bool RegisterDiabloFightPlayerIDChangedListener(DiabloFightPlayerIDChangedCallback callback)
	{
		return RegisterDiabloFightPlayerIDChangedListener(callback, null);
	}

	public bool RegisterDiabloFightPlayerIDChangedListener(DiabloFightPlayerIDChangedCallback callback, object userData)
	{
		DiabloFightPlayerIDChangedListener listener = new DiabloFightPlayerIDChangedListener();
		listener.SetCallback(callback);
		listener.SetUserData(userData);
		if (m_diabloFightPlayerIDChangedListeners.Contains(listener))
		{
			return false;
		}
		m_diabloFightPlayerIDChangedListeners.Add(listener);
		return true;
	}

	public bool UnregisterDamageCapChangedListener(DamageCapChangedCallback callback)
	{
		return UnregisterDamageCapChangedListener(callback, null);
	}

	public bool UnregisterDamageCapChangedListener(DamageCapChangedCallback callback, object userData)
	{
		DamageCapChangedListener listener = new DamageCapChangedListener();
		listener.SetCallback(callback);
		listener.SetUserData(userData);
		return m_damageCapChangedListeners.Remove(listener);
	}

	public bool UnregisterDiabloFightPlayerIDChangedListener(DiabloFightPlayerIDChangedCallback callback)
	{
		return UnregisterDiabloFightPlayerIDChangedListener(callback, null);
	}

	public bool UnregisterDiabloFightPlayerIDChangedListener(DiabloFightPlayerIDChangedCallback callback, object userData)
	{
		DiabloFightPlayerIDChangedListener listener = new DiabloFightPlayerIDChangedListener();
		listener.SetCallback(callback);
		listener.SetUserData(userData);
		return m_diabloFightPlayerIDChangedListeners.Remove(listener);
	}

	public bool RegisterFriendlyTurnStartedListener(FriendlyTurnStartedCallback callback, object userData = null)
	{
		FriendlyTurnStartedListener listener = new FriendlyTurnStartedListener();
		listener.SetCallback(callback);
		listener.SetUserData(userData);
		if (m_friendlyTurnStartedListeners.Contains(listener))
		{
			return false;
		}
		m_friendlyTurnStartedListeners.Add(listener);
		return true;
	}

	public bool UnregisterFriendlyTurnStartedListener(FriendlyTurnStartedCallback callback, object userData = null)
	{
		FriendlyTurnStartedListener listener = new FriendlyTurnStartedListener();
		listener.SetCallback(callback);
		listener.SetUserData(userData);
		return m_friendlyTurnStartedListeners.Remove(listener);
	}

	public bool RegisterTurnTimerUpdateListener(TurnTimerUpdateCallback callback)
	{
		return RegisterTurnTimerUpdateListener(callback, null);
	}

	public bool RegisterTurnTimerUpdateListener(TurnTimerUpdateCallback callback, object userData)
	{
		TurnTimerUpdateListener listener = new TurnTimerUpdateListener();
		listener.SetCallback(callback);
		listener.SetUserData(userData);
		if (m_turnTimerUpdateListeners.Contains(listener))
		{
			return false;
		}
		m_turnTimerUpdateListeners.Add(listener);
		return true;
	}

	public bool UnregisterTurnTimerUpdateListener(TurnTimerUpdateCallback callback)
	{
		return UnregisterTurnTimerUpdateListener(callback, null);
	}

	public bool UnregisterTurnTimerUpdateListener(TurnTimerUpdateCallback callback, object userData)
	{
		TurnTimerUpdateListener listener = new TurnTimerUpdateListener();
		listener.SetCallback(callback);
		listener.SetUserData(userData);
		return m_turnTimerUpdateListeners.Remove(listener);
	}

	public bool RegisterMulliganTimerUpdateListener(TurnTimerUpdateCallback callback)
	{
		return RegisterMulliganTimerUpdateListener(callback, null);
	}

	public bool RegisterMulliganTimerUpdateListener(TurnTimerUpdateCallback callback, object userData)
	{
		TurnTimerUpdateListener listener = new TurnTimerUpdateListener();
		listener.SetCallback(callback);
		listener.SetUserData(userData);
		if (m_mulliganTimerUpdateListeners.Contains(listener))
		{
			return false;
		}
		m_mulliganTimerUpdateListeners.Add(listener);
		return true;
	}

	public bool UnregisterMulliganTimerUpdateListener(TurnTimerUpdateCallback callback)
	{
		return UnregisterMulliganTimerUpdateListener(callback, null);
	}

	public bool UnregisterMulliganTimerUpdateListener(TurnTimerUpdateCallback callback, object userData)
	{
		TurnTimerUpdateListener listener = new TurnTimerUpdateListener();
		listener.SetCallback(callback);
		listener.SetUserData(userData);
		return m_mulliganTimerUpdateListeners.Remove(listener);
	}

	public bool RegisterSpectatorNotifyListener(SpectatorNotifyEventCallback callback, object userData = null)
	{
		SpectatorNotifyListener listener = new SpectatorNotifyListener();
		listener.SetCallback(callback);
		listener.SetUserData(userData);
		if (m_spectatorNotifyListeners.Contains(listener))
		{
			return false;
		}
		m_spectatorNotifyListeners.Add(listener);
		return true;
	}

	public bool UnregisterSpectatorNotifyListener(SpectatorNotifyEventCallback callback, object userData = null)
	{
		SpectatorNotifyListener listener = new SpectatorNotifyListener();
		listener.SetCallback(callback);
		listener.SetUserData(userData);
		return m_spectatorNotifyListeners.Remove(listener);
	}

	public bool RegisterGameOverListener(GameOverCallback callback, object userData = null)
	{
		GameOverListener listener = new GameOverListener();
		listener.SetCallback(callback);
		listener.SetUserData(userData);
		if (m_gameOverListeners.Contains(listener))
		{
			return false;
		}
		m_gameOverListeners.Add(listener);
		return true;
	}

	public bool UnregisterGameOverListener(GameOverCallback callback, object userData = null)
	{
		GameOverListener listener = new GameOverListener();
		listener.SetCallback(callback);
		listener.SetUserData(userData);
		return m_gameOverListeners.Remove(listener);
	}

	public bool RegisterHeroChangedListener(HeroChangedCallback callback, object userData = null)
	{
		HeroChangedListener listener = new HeroChangedListener();
		listener.SetCallback(callback);
		listener.SetUserData(userData);
		if (m_heroChangedListeners.Contains(listener))
		{
			return false;
		}
		m_heroChangedListeners.Add(listener);
		return true;
	}

	public bool UnregisterHeroChangedListener(HeroChangedCallback callback, object userData = null)
	{
		HeroChangedListener listener = new HeroChangedListener();
		listener.SetCallback(callback);
		listener.SetUserData(userData);
		return m_heroChangedListeners.Remove(listener);
	}

	public bool RegisterBusyStateChangedListener(BusyStateChangedCallback callback, object userData = null)
	{
		BusyStateChangedListener listener = new BusyStateChangedListener();
		listener.SetCallback(callback);
		listener.SetUserData(userData);
		if (m_busyStateChangedListeners.Contains(listener))
		{
			return false;
		}
		m_busyStateChangedListeners.Add(listener);
		return true;
	}

	public bool UnregisterBusyStateChangedListener(BusyStateChangedCallback callback, object userData = null)
	{
		BusyStateChangedListener listener = new BusyStateChangedListener();
		listener.SetCallback(callback);
		listener.SetUserData(userData);
		return m_busyStateChangedListeners.Remove(listener);
	}

	public bool RegisterCantPlayListener(CantPlayCallback callback, object userData = null)
	{
		CantPlayListener listener = new CantPlayListener();
		listener.SetCallback(callback);
		listener.SetUserData(userData);
		if (m_cantPlayListeners.Contains(listener))
		{
			return false;
		}
		m_cantPlayListeners.Add(listener);
		return true;
	}

	public bool UnregisterCantPlayListener(CantPlayCallback callback, object userData = null)
	{
		CantPlayListener listener = new CantPlayListener();
		listener.SetCallback(callback);
		listener.SetUserData(userData);
		return m_cantPlayListeners.Remove(listener);
	}

	private static void FireGameStateInitializedEvent()
	{
		if (s_gameStateInitializedListeners != null)
		{
			GameStateInitializedListener[] array = s_gameStateInitializedListeners.ToArray();
			for (int i = 0; i < array.Length; i++)
			{
				array[i].Fire(s_instance);
			}
		}
	}

	private static void FireGameStateShutdownEvent()
	{
		if (s_gameStateShutdownListeners != null)
		{
			GameStateShutdownListener[] array = s_gameStateShutdownListeners.ToArray();
			for (int i = 0; i < array.Length; i++)
			{
				array[i].Fire(s_instance);
			}
		}
	}

	private void FireCreateGameEvent()
	{
		CreateGameListener[] array = m_createGameListeners.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Fire(m_createGamePhase);
		}
	}

	private void FireOptionsReceivedEvent()
	{
		OptionsReceivedListener[] array = m_optionsReceivedListeners.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Fire();
		}
	}

	private void FireOptionsSentEvent(Network.Options.Option option)
	{
		OptionsSentListener[] array = m_optionsSentListeners.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Fire(option);
		}
	}

	private void FireOptionRejectedEvent(Network.Options.Option option)
	{
		OptionRejectedListener[] array = m_optionRejectedListeners.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Fire(option);
		}
	}

	private void FireEntityChoicesReceivedEvent(Network.EntityChoices choices, PowerTaskList preChoiceTaskList)
	{
		EntityChoicesReceivedListener[] array = m_entityChoicesReceivedListeners.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Fire(choices, preChoiceTaskList);
		}
	}

	private bool FireEntitiesChosenReceivedEvent(Network.EntitiesChosen chosen)
	{
		EntitiesChosenReceivedListener[] array = m_entitiesChosenReceivedListeners.ToArray();
		bool handled = false;
		EntitiesChosenReceivedListener[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			handled = array2[i].Fire(chosen) || handled;
		}
		return handled;
	}

	private void FireTurnChangedEvent(int oldTurn, int newTurn)
	{
		TurnChangedListener[] array = m_turnChangedListeners.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Fire(oldTurn, newTurn);
		}
	}

	private void FireDamageCapChangedEvent(int oldValue, int newValue)
	{
		DamageCapChangedListener[] array = m_damageCapChangedListeners.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Fire(oldValue, newValue);
		}
	}

	private void FireDiabloFightPlayerIDChangedEvent(int oldValue, int newValue)
	{
		DiabloFightPlayerIDChangedListener[] array = m_diabloFightPlayerIDChangedListeners.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Fire(oldValue, newValue);
		}
	}

	public void FireFriendlyTurnStartedEvent()
	{
		m_gameEntity.NotifyOfStartOfTurnEventsFinished();
		FriendlyTurnStartedListener[] array = m_friendlyTurnStartedListeners.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Fire();
		}
	}

	private void FireTurnTimerUpdateEvent(TurnTimerUpdate update)
	{
		TurnTimerUpdateListener[] listeners = null;
		if (GetGameEntity() == null)
		{
			Debug.LogWarning("FireTurnTimerUpdateEvent - Turn timer update received before game entity created.");
			return;
		}
		listeners = ((!GetGameEntity().IsMulliganActiveRealTime()) ? m_turnTimerUpdateListeners.ToArray() : m_mulliganTimerUpdateListeners.ToArray());
		TurnTimerUpdateListener[] array = listeners;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Fire(update);
		}
	}

	private void FireCantPlayEvent(Entity entity)
	{
		CantPlayListener[] array = m_cantPlayListeners.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Fire(entity);
		}
	}

	private void FireCurrentPlayerChangedEvent(Player player)
	{
		CurrentPlayerChangedListener[] array = m_currentPlayerChangedListeners.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Fire(player);
		}
	}

	private void FireSpectatorNotifyEvent(SpectatorNotify notify)
	{
		SpectatorNotifyListener[] array = m_spectatorNotifyListeners.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Fire(notify);
		}
	}

	private void FireGameOverEvent(TAG_PLAYSTATE playState)
	{
		GameOverListener[] array = m_gameOverListeners.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Fire(playState);
		}
	}

	public void FireHeroChangedEvent(Player player)
	{
		HeroChangedListener[] array = m_heroChangedListeners.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Fire(player);
		}
	}

	private void FireBusyStateChangedEvent(bool isBusy)
	{
		BusyStateChangedListener[] array = m_busyStateChangedListeners.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Fire(isBusy);
		}
	}

	public ResponseMode GetResponseMode()
	{
		return m_responseMode;
	}

	public Network.EntityChoices GetFriendlyEntityChoices()
	{
		int playerId = GetFriendlyPlayerId();
		return GetEntityChoices(playerId);
	}

	public Network.EntityChoices GetOpponentEntityChoices()
	{
		int playerId = GetOpposingPlayerId();
		return GetEntityChoices(playerId);
	}

	public Network.EntityChoices GetEntityChoices(int playerId)
	{
		m_choicesMap.TryGetValue(playerId, out var choices);
		return choices;
	}

	public Map<int, Network.EntityChoices> GetEntityChoicesMap()
	{
		return m_choicesMap;
	}

	public bool IsChoosableEntity(Entity entity)
	{
		return GetFriendlyEntityChoices()?.Entities.Contains(entity.GetEntityId()) ?? false;
	}

	public bool IsChosenEntity(Entity entity)
	{
		if (GetFriendlyEntityChoices() == null)
		{
			return false;
		}
		return m_chosenEntities.Contains(entity);
	}

	public bool AddChosenEntity(Entity entity)
	{
		if (m_chosenEntities.Contains(entity))
		{
			return false;
		}
		m_chosenEntities.Add(entity);
		ChoiceCardMgr.Get().OnChosenEntityAdded(entity);
		Card card = entity.GetCard();
		if (card != null)
		{
			card.UpdateActorState();
		}
		return true;
	}

	public bool RemoveChosenEntity(Entity entity)
	{
		if (!m_chosenEntities.Remove(entity))
		{
			return false;
		}
		ChoiceCardMgr.Get().OnChosenEntityRemoved(entity);
		Card card = entity.GetCard();
		if (card != null)
		{
			card.UpdateActorState();
		}
		return true;
	}

	public List<Entity> GetChosenEntities()
	{
		return m_chosenEntities;
	}

	public Network.Options GetOptionsPacket()
	{
		return m_options;
	}

	public void EnterChoiceMode()
	{
		m_responseMode = ResponseMode.CHOICE;
		UpdateOptionHighlights();
		UpdateChoiceHighlights();
	}

	public void EnterMainOptionMode()
	{
		ResponseMode prevResponseMode = m_responseMode;
		m_responseMode = ResponseMode.OPTION;
		if (m_selectedOption.m_main >= 0 && m_selectedOption.m_main < m_options.List.Count)
		{
			switch (prevResponseMode)
			{
			case ResponseMode.SUB_OPTION:
			{
				Network.Options.Option option2 = m_options.List[m_selectedOption.m_main];
				UpdateSubOptionHighlights(option2);
				break;
			}
			case ResponseMode.OPTION_TARGET:
			{
				Network.Options.Option option = m_options.List[m_selectedOption.m_main];
				UpdateTargetHighlights(option.Main);
				if (m_selectedOption.m_sub != -1)
				{
					Network.Options.Option.SubOption subOption = option.Subs[m_selectedOption.m_sub];
					UpdateTargetHighlights(subOption);
				}
				break;
			}
			}
		}
		UpdateOptionHighlights(m_lastOptions);
		UpdateOptionHighlights();
		m_selectedOption.Clear();
	}

	public void EnterSubOptionMode()
	{
		Network.Options.Option option = m_options.List[m_selectedOption.m_main];
		if (m_responseMode == ResponseMode.OPTION)
		{
			m_responseMode = ResponseMode.SUB_OPTION;
			UpdateOptionHighlights();
		}
		else if (m_responseMode == ResponseMode.OPTION_TARGET)
		{
			m_responseMode = ResponseMode.SUB_OPTION;
			Network.Options.Option.SubOption subOption = option.Subs[m_selectedOption.m_sub];
			UpdateTargetHighlights(subOption);
		}
		UpdateSubOptionHighlights(option);
	}

	public void EnterOptionTargetMode(Entity entity)
	{
		if (m_responseMode == ResponseMode.OPTION)
		{
			m_responseMode = ResponseMode.OPTION_TARGET;
			UpdateOptionHighlights();
			Network.Options.Option option = m_options.List[m_selectedOption.m_main];
			UpdateTargetHighlights(option.Main);
		}
		else if (m_responseMode == ResponseMode.SUB_OPTION)
		{
			m_responseMode = ResponseMode.OPTION_TARGET;
			Network.Options.Option option2 = m_options.List[m_selectedOption.m_main];
			UpdateSubOptionHighlights(option2);
			Network.Options.Option.SubOption subOption = option2.Subs[m_selectedOption.m_sub];
			UpdateTargetHighlights(subOption);
		}
		if (IsInTargetMode())
		{
			GetGameEntity()?.NotifyOfTargetModeStarted(entity);
		}
	}

	public void EnterMoveMinionMode(Entity heldEntity, bool suppressGlow = false)
	{
		ActivateMoveMinionTargets(heldEntity, suppressGlow);
	}

	public void ExitMoveMinionMode()
	{
		DeactivateMoveMinionTargetHighlights();
	}

	public void CancelCurrentOptionMode()
	{
		if (IsInTargetMode())
		{
			GetGameEntity().NotifyOfTargetModeCancelled();
		}
		CancelSelectedOptionProposedMana();
		EnterMainOptionMode();
	}

	public bool IsInMainOptionMode()
	{
		return m_responseMode == ResponseMode.OPTION;
	}

	public bool IsInSubOptionMode()
	{
		return m_responseMode == ResponseMode.SUB_OPTION;
	}

	public bool IsInTargetMode()
	{
		return m_responseMode == ResponseMode.OPTION_TARGET;
	}

	public bool IsInChoiceMode()
	{
		return m_responseMode == ResponseMode.CHOICE;
	}

	public void SetSelectedOption(ChooseOption packet)
	{
		m_selectedOption.m_main = packet.Index;
		m_selectedOption.m_sub = packet.SubOption;
		m_selectedOption.m_target = packet.Target;
		m_selectedOption.m_position = packet.Position;
	}

	public void SetChosenEntities(ChooseEntities packet)
	{
		m_chosenEntities.Clear();
		foreach (int entityId in packet.Entities)
		{
			Entity entity = GetEntity(entityId);
			if (entity != null)
			{
				m_chosenEntities.Add(entity);
			}
		}
	}

	public void SetSelectedOption(int index)
	{
		m_selectedOption.m_main = index;
	}

	public int GetSelectedOption()
	{
		return m_selectedOption.m_main;
	}

	public void SetSelectedSubOption(int index)
	{
		m_selectedOption.m_sub = index;
	}

	public int GetSelectedSubOption()
	{
		return m_selectedOption.m_sub;
	}

	public void SetSelectedOptionTarget(int target)
	{
		m_selectedOption.m_target = target;
	}

	public int GetSelectedOptionTarget()
	{
		return m_selectedOption.m_target;
	}

	public bool IsSelectedOptionFriendlyHero()
	{
		Entity friendlyHero = GetFriendlySidePlayer().GetHero();
		if (friendlyHero == null)
		{
			return false;
		}
		Network.Options.Option option = GetSelectedNetworkOption();
		if (option != null)
		{
			return option.Main.ID == friendlyHero.GetEntityId();
		}
		return false;
	}

	public bool IsSelectedOptionFriendlyHeroPower()
	{
		Entity friendlyHeroPower = GetFriendlySidePlayer().GetHeroPower();
		if (friendlyHeroPower == null)
		{
			return false;
		}
		Network.Options.Option option = GetSelectedNetworkOption();
		if (option != null)
		{
			return option.Main.ID == friendlyHeroPower.GetEntityId();
		}
		return false;
	}

	public bool IsSelectedOptionMercenariesAbility()
	{
		Network.Options.Option option = GetSelectedNetworkOption();
		if (option == null)
		{
			return false;
		}
		return GetEntity(option.Main.ID)?.IsLettuceAbility() ?? false;
	}

	public void SetSelectedOptionPosition(int position)
	{
		m_selectedOption.m_position = position;
	}

	public int GetSelectedOptionPosition()
	{
		return m_selectedOption.m_position;
	}

	public Network.Options.Option GetSelectedNetworkOption()
	{
		if (m_selectedOption.m_main < 0)
		{
			return null;
		}
		return m_options.List[m_selectedOption.m_main];
	}

	public Network.Options.Option.SubOption GetSelectedNetworkSubOption()
	{
		if (m_selectedOption.m_main < 0)
		{
			return null;
		}
		Network.Options.Option option = m_options.List[m_selectedOption.m_main];
		if (m_selectedOption.m_sub == -1)
		{
			return option.Main;
		}
		return option.Subs[m_selectedOption.m_sub];
	}

	public bool EntityHasSubOptions(Entity entity)
	{
		int entityId = entity.GetEntityId();
		Network.Options optionsPacket = GetOptionsPacket();
		if (optionsPacket == null)
		{
			return false;
		}
		for (int i = 0; i < optionsPacket.List.Count; i++)
		{
			Network.Options.Option option = optionsPacket.List[i];
			if (option.Type == Network.Options.Option.OptionType.POWER && option.Main.ID == entityId)
			{
				if (option.Subs != null)
				{
					return option.Subs.Count > 0;
				}
				return false;
			}
		}
		return false;
	}

	public bool EntityHasTargets(Entity entity)
	{
		return EntityHasTargets(entity, isSubEntity: false);
	}

	public bool SubEntityHasTargets(Entity subEntity)
	{
		return EntityHasTargets(subEntity, isSubEntity: true);
	}

	public bool EntityOnlyTrades(Entity entity)
	{
		int entityId = entity.GetEntityId();
		Network.Options optionsPacket = GetOptionsPacket();
		if (optionsPacket == null)
		{
			return false;
		}
		bool foundOne = false;
		for (int i = 0; i < optionsPacket.List.Count; i++)
		{
			Network.Options.Option option = optionsPacket.List[i];
			if (option.Type == Network.Options.Option.OptionType.POWER && option.Main.ID == entityId)
			{
				if (option.Main.IsDeckActionOption())
				{
					foundOne = true;
				}
				else if (option.Main.HasValidTarget())
				{
					return false;
				}
			}
		}
		return foundOne;
	}

	public bool HasSubOptions(Entity entity)
	{
		if (!IsEntityInputEnabled(entity))
		{
			return false;
		}
		int entityId = entity.GetEntityId();
		Network.Options optionsPacket = GetOptionsPacket();
		for (int i = 0; i < optionsPacket.List.Count; i++)
		{
			Network.Options.Option option = optionsPacket.List[i];
			if (option.Type == Network.Options.Option.OptionType.POWER && option.Main.ID == entityId)
			{
				return option.Subs.Count > 0;
			}
		}
		return false;
	}

	public int? GetErrorParam(Entity entity)
	{
		Network.Options optionsPacket = GetOptionsPacket();
		if (optionsPacket == null || entity == null)
		{
			return null;
		}
		switch (GetResponseMode())
		{
		case ResponseMode.OPTION:
		{
			Network.Options.Option option = optionsPacket.GetOptionFromEntityID(entity.GetEntityId());
			if (option != null && option.Type == Network.Options.Option.OptionType.POWER)
			{
				return option.Main.PlayErrorInfo.PlayErrorParam;
			}
			break;
		}
		case ResponseMode.SUB_OPTION:
		{
			Network.Options.Option.SubOption subOption = GetSelectedNetworkOption().GetSubOptionFromEntityID(entity.GetEntityId());
			if (subOption != null)
			{
				return subOption.PlayErrorInfo.PlayErrorParam;
			}
			break;
		}
		case ResponseMode.OPTION_TARGET:
			return GetSelectedNetworkSubOption().GetErrorParamForTarget(entity.GetEntityId());
		}
		return null;
	}

	public PlayErrors.ErrorType GetErrorType(Entity entity)
	{
		Network.Options optionsPacket = GetOptionsPacket();
		if (optionsPacket == null || !Get().IsFriendlySidePlayerTurn())
		{
			return PlayErrors.ErrorType.REQ_YOUR_TURN;
		}
		switch (GetResponseMode())
		{
		case ResponseMode.OPTION:
		{
			Network.Options.Option option = optionsPacket.GetOptionFromEntityID(entity.GetEntityId());
			if (option != null && option.Type == Network.Options.Option.OptionType.POWER)
			{
				return option.Main.PlayErrorInfo.PlayError;
			}
			break;
		}
		case ResponseMode.SUB_OPTION:
		{
			Network.Options.Option.SubOption subOption = GetSelectedNetworkOption().GetSubOptionFromEntityID(entity.GetEntityId());
			if (subOption != null)
			{
				return subOption.PlayErrorInfo.PlayError;
			}
			break;
		}
		case ResponseMode.OPTION_TARGET:
			return GetSelectedNetworkSubOption().GetErrorForTarget(entity.GetEntityId());
		}
		return PlayErrors.ErrorType.INVALID;
	}

	public bool HasResponse(Entity entity, bool? wantTradeOption = null)
	{
		return GetResponseMode() switch
		{
			ResponseMode.CHOICE => IsChoice(entity), 
			ResponseMode.OPTION => IsValidOption(entity, wantTradeOption), 
			ResponseMode.SUB_OPTION => IsValidSubOption(entity), 
			ResponseMode.OPTION_TARGET => IsValidOptionTarget(entity, checkInputEnabled: true), 
			_ => false, 
		};
	}

	public bool IsChoice(Entity entity)
	{
		if (!IsEntityInputEnabled(entity))
		{
			return false;
		}
		if (!IsChoosableEntity(entity))
		{
			return false;
		}
		if (IsChosenEntity(entity))
		{
			return false;
		}
		return true;
	}

	public bool IsValidOption(Entity entity, bool? wantDeckActionOption = null)
	{
		if (!IsEntityInputEnabled(entity))
		{
			return false;
		}
		int entityId = entity.GetEntityId();
		Network.Options optionsPacket = GetOptionsPacket();
		if (optionsPacket == null)
		{
			return false;
		}
		for (int i = 0; i < optionsPacket.List.Count; i++)
		{
			Network.Options.Option option = optionsPacket.List[i];
			if (option.Type == Network.Options.Option.OptionType.POWER && option.Main.PlayErrorInfo.IsValid() && option.Main.ID == entityId && (!wantDeckActionOption.HasValue || option.Main.IsDeckActionOption() == wantDeckActionOption))
			{
				return true;
			}
		}
		return false;
	}

	public bool IsValidSubOption(Entity entity)
	{
		if (!IsEntityInputEnabled(entity))
		{
			return false;
		}
		int entityId = entity.GetEntityId();
		Network.Options.Option option = GetSelectedNetworkOption();
		for (int i = 0; i < option.Subs.Count; i++)
		{
			Network.Options.Option.SubOption subOption = option.Subs[i];
			if (subOption.ID == entityId)
			{
				if (!subOption.PlayErrorInfo.IsValid())
				{
					return false;
				}
				return true;
			}
		}
		return false;
	}

	public bool IsValidOptionTarget(Entity entity, bool checkInputEnabled)
	{
		if (checkInputEnabled && !IsEntityInputEnabled(entity))
		{
			return false;
		}
		return GetSelectedNetworkSubOption()?.IsValidTarget(entity.GetEntityId()) ?? false;
	}

	public bool IsValidPotentialOptionTarget(Entity source, Entity target)
	{
		if (m_options == null)
		{
			return false;
		}
		int sourceEntityId = source.GetEntityId();
		foreach (Network.Options.Option option in m_options.List)
		{
			if (option.Type == Network.Options.Option.OptionType.POWER && option.Main.ID == sourceEntityId)
			{
				if (option.Subs != null && option.Subs.Count > 0)
				{
					return false;
				}
				return option.Main.IsValidTarget(target.GetEntityId());
			}
		}
		return false;
	}

	public bool IsEntityInputEnabled(Entity entity)
	{
		if (IsResponsePacketBlocked())
		{
			return false;
		}
		if (entity.IsBusy())
		{
			return false;
		}
		Card card = entity.GetCard();
		if (card != null)
		{
			if (!card.IsInputEnabled())
			{
				return false;
			}
			Zone zone = card.GetZone();
			if (zone != null && !zone.IsInputEnabled())
			{
				return false;
			}
		}
		return true;
	}

	private bool EntityHasTargets(Entity entity, bool isSubEntity)
	{
		int entityId = entity.GetEntityId();
		Network.Options optionsPacket = GetOptionsPacket();
		if (optionsPacket == null)
		{
			return false;
		}
		for (int i = 0; i < optionsPacket.List.Count; i++)
		{
			Network.Options.Option option = optionsPacket.List[i];
			if (option.Type != Network.Options.Option.OptionType.POWER)
			{
				continue;
			}
			if (isSubEntity)
			{
				if (option.Subs == null)
				{
					continue;
				}
				for (int subIdx = 0; subIdx < option.Subs.Count; subIdx++)
				{
					Network.Options.Option.SubOption subOption = option.Subs[subIdx];
					if (subOption.ID == entityId)
					{
						return subOption.HasValidTarget();
					}
				}
			}
			else if (option.Main.ID == entityId)
			{
				return option.Main.HasValidTarget();
			}
		}
		return false;
	}

	private void CancelSelectedOptionProposedMana()
	{
		Network.Options.Option selectedOption = GetSelectedNetworkOption();
		if (selectedOption != null)
		{
			GetFriendlySidePlayer().CancelAllProposedMana(GetEntity(selectedOption.Main.ID));
		}
	}

	public void ClearResponseMode()
	{
		Log.Hand.Print("ClearResponseMode");
		m_responseMode = ResponseMode.NONE;
		ZoneMgr.Get().DismissMercenariesAbilityTray();
		RemoteActionHandler.Get().NotifyOpponentOfSelection(0);
		if (m_options != null)
		{
			for (int i = 0; i < m_options.List.Count; i++)
			{
				Network.Options.Option option = m_options.List[i];
				if (option.Type == Network.Options.Option.OptionType.POWER)
				{
					GetEntity(option.Main.ID)?.ClearBattlecryFlag();
				}
			}
			UpdateHighlightsBasedOnSelection();
			UpdateOptionHighlights(m_options);
		}
		else if (GetFriendlyEntityChoices() != null)
		{
			UpdateChoiceHighlights();
		}
	}

	public void UpdateChoiceHighlights()
	{
		foreach (Network.EntityChoices choices in m_choicesMap.Values)
		{
			Entity sourceEntity = GetEntity(choices.Source);
			if (sourceEntity != null)
			{
				Card sourceCard = sourceEntity.GetCard();
				if (sourceCard != null)
				{
					sourceCard.UpdateActorState();
				}
			}
			foreach (int entityId in choices.Entities)
			{
				Entity entity = GetEntity(entityId);
				if (entity != null)
				{
					Card card = entity.GetCard();
					if (!(card == null))
					{
						card.UpdateActorState();
					}
				}
			}
		}
		foreach (Entity chosenEntity in m_chosenEntities)
		{
			Card card2 = chosenEntity.GetCard();
			if (!(card2 == null))
			{
				card2.UpdateActorState();
			}
		}
	}

	private void UpdateHighlightsBasedOnSelection()
	{
		if (m_selectedOption.m_target != 0)
		{
			Network.Options.Option.SubOption selectedSubOption = GetSelectedNetworkSubOption();
			if (selectedSubOption != null)
			{
				UpdateTargetHighlights(selectedSubOption);
			}
		}
		else if (m_selectedOption.m_sub >= 0)
		{
			Network.Options.Option selectedOption = GetSelectedNetworkOption();
			UpdateSubOptionHighlights(selectedOption);
		}
	}

	public void UpdateOptionHighlights()
	{
		UpdateOptionHighlights(m_options);
	}

	public void UpdateOptionHighlights(Network.Options options)
	{
		if (options == null || options.List == null)
		{
			return;
		}
		for (int i = 0; i < options.List.Count; i++)
		{
			Network.Options.Option option = options.List[i];
			if (option.Type != Network.Options.Option.OptionType.POWER)
			{
				continue;
			}
			Entity entity = GetEntity(option.Main.ID);
			if (entity != null)
			{
				Card card = entity.GetCard();
				if (!(card == null))
				{
					card.UpdateActorState();
				}
			}
		}
	}

	private void UpdateSubOptionHighlights(Network.Options.Option option)
	{
		Entity entity = GetEntity(option.Main.ID);
		if (entity != null)
		{
			Card card = entity.GetCard();
			if (card != null)
			{
				card.UpdateActorState();
			}
		}
		foreach (Network.Options.Option.SubOption subOption in option.Subs)
		{
			Entity entity2 = GetEntity(subOption.ID);
			if (entity2 != null)
			{
				Card card2 = entity2.GetCard();
				if (!(card2 == null))
				{
					card2.UpdateActorState();
				}
			}
		}
	}

	private void UpdateTargetHighlights(Network.Options.Option.SubOption subOption)
	{
		Entity entity = GetEntity(subOption.ID);
		if (entity != null)
		{
			Card card = entity.GetCard();
			if (card != null)
			{
				card.UpdateActorState();
			}
		}
		foreach (Network.Options.Option.TargetOption targetOption in subOption.Targets)
		{
			if (!targetOption.PlayErrorInfo.IsValid())
			{
				continue;
			}
			int targetID = targetOption.ID;
			Entity entity2 = GetEntity(targetID);
			if (entity2 != null)
			{
				Card card2 = entity2.GetCard();
				if (!(card2 == null))
				{
					card2.UpdateActorState();
				}
			}
		}
	}

	public void DisableOptionHighlights(Network.Options options)
	{
		if (options == null || options.List == null)
		{
			return;
		}
		for (int i = 0; i < options.List.Count; i++)
		{
			Network.Options.Option option = options.List[i];
			if (option.Type != Network.Options.Option.OptionType.POWER)
			{
				continue;
			}
			Entity entity = GetEntity(option.Main.ID);
			if (entity == null)
			{
				continue;
			}
			Card card = entity.GetCard();
			if (!(card == null))
			{
				Actor actor = card.GetActor();
				if (!(actor == null))
				{
					actor.SetActorState(ActorStateType.CARD_IDLE);
				}
			}
		}
	}

	public bool HasValidHoverTargetForMovedMinion(Entity movedEntity, out PlayErrors.ErrorType mainOptionPlayError)
	{
		mainOptionPlayError = PlayErrors.ErrorType.INVALID;
		List<Card> moveMinionHoverTargets = GetMoveMinionHoverTargetsInPlay();
		if (!moveMinionHoverTargets.Any())
		{
			return false;
		}
		if (m_options == null || m_options.List == null)
		{
			return false;
		}
		foreach (Network.Options.Option option in m_options.List)
		{
			if (moveMinionHoverTargets.FirstOrDefault((Card t) => t.GetEntity().GetEntityId() == option.Main.ID) == null)
			{
				continue;
			}
			if (!option.Main.PlayErrorInfo.IsValid())
			{
				if (option.Main.PlayErrorInfo.PlayError != PlayErrors.ErrorType.INVALID)
				{
					mainOptionPlayError = option.Main.PlayErrorInfo.PlayError;
				}
			}
			else if (option.Main.IsValidTarget(movedEntity.GetEntityId()))
			{
				return true;
			}
		}
		if (movedEntity.IsDormant())
		{
			mainOptionPlayError = PlayErrors.ErrorType.REQ_TARGET_NOT_UNTOUCHABLE;
		}
		return false;
	}

	private void ActivateMoveMinionTargets(Entity movedEntity, bool suppressGlow = false)
	{
		if (movedEntity == null)
		{
			return;
		}
		DisableOptionHighlights(m_options);
		List<Card> moveMinionHoverTargets = GetMoveMinionHoverTargetsInPlay();
		if (!moveMinionHoverTargets.Any() || m_options == null || m_options.List == null)
		{
			return;
		}
		foreach (Network.Options.Option option in m_options.List)
		{
			Card card = moveMinionHoverTargets.FirstOrDefault((Card t) => t.GetEntity().GetEntityId() == option.Main.ID);
			if (card == null || !card.HasCardDef)
			{
				continue;
			}
			PlayMakerFSM hoverTargetCardDefPlaymaker = card.GetCardDefComponent<PlayMakerFSM>();
			if (hoverTargetCardDefPlaymaker == null)
			{
				continue;
			}
			bool isValidTarget = option.Main.IsValidTarget(movedEntity.GetEntityId());
			hoverTargetCardDefPlaymaker.Fsm.GetFsmGameObject("HoverTargetCard").Value = card.gameObject;
			hoverTargetCardDefPlaymaker.Fsm.GetFsmBool("SuppressGlow").Value = suppressGlow || !isValidTarget;
			SetCustomMoveMinionTargetSpellVariables(hoverTargetCardDefPlaymaker, movedEntity);
			hoverTargetCardDefPlaymaker.SendEvent("Action");
			if (isValidTarget)
			{
				if (!movedEntity.HasTag(GAME_TAG.BACON_COSTS_HEALTH_TO_BUY))
				{
					ManaCrystalMgr.Get().ProposeManaCrystalUsage(card.GetEntity());
				}
				PlayCustomMoveMinionTargetSpell(movedEntity, card);
			}
		}
	}

	private void DeactivateMoveMinionTargetHighlights()
	{
		List<Card> moveMinionHoverTargets = GetMoveMinionHoverTargetsInPlay();
		if (!moveMinionHoverTargets.Any())
		{
			return;
		}
		foreach (Card targetCard in moveMinionHoverTargets)
		{
			if (targetCard.HasCardDef)
			{
				PlayMakerFSM hoverTargetCardDefPlaymaker = targetCard.GetCardDefComponent<PlayMakerFSM>();
				if (!(hoverTargetCardDefPlaymaker == null))
				{
					hoverTargetCardDefPlaymaker.SendEvent("Death");
					ManaCrystalMgr.Get().CancelAllProposedMana(targetCard.GetEntity());
					DisableCustomMoveMinionTagetSpells(targetCard);
				}
			}
		}
		UpdateOptionHighlights();
	}

	private void PlayCustomMoveMinionTargetSpell(Entity movedEntity, Card moveMinionTarget)
	{
		if (movedEntity.HasTag(GAME_TAG.BACON_COSTS_HEALTH_TO_BUY) && !movedEntity.IsControlledByFriendlySidePlayer())
		{
			Spell spell = moveMinionTarget.GetHero().GetCard().GetActor()
				.GetSpell(SpellType.COST_HEALTH_TO_BUY);
			if (spell != null)
			{
				spell.GetComponent<PlayMakerFSM>().FsmVariables.GetFsmString("DamageAmount").Value = Convert.ToString(moveMinionTarget.GetEntity().GetCost() * -1);
				spell.ActivateState(SpellStateType.BIRTH);
			}
		}
	}

	private void SetCustomMoveMinionTargetSpellVariables(PlayMakerFSM hoverTargetCardDefPlaymaker, Entity movedEntity)
	{
		if (hoverTargetCardDefPlaymaker.Fsm.GetFsmString("SellPrice") != null)
		{
			int price = movedEntity.GetTag(GAME_TAG.BACON_SELL_VALUE);
			if (price == 0)
			{
				price = 1;
			}
			hoverTargetCardDefPlaymaker.Fsm.GetFsmString("SellPrice").Value = price.ToString();
		}
	}

	private void DisableCustomMoveMinionTagetSpells(Card moveMinionTarget)
	{
		moveMinionTarget.GetHero().GetCard().GetActor()
			.ActivateSpellDeathState(SpellType.COST_HEALTH_TO_BUY);
	}

	public bool HasEnoughManaForMoveMinionHoverTarget(Entity heldEntity)
	{
		Player friendlyPlayer = GetFriendlySidePlayer();
		List<Card> moveMinionHoverTargets = GetMoveMinionHoverTargetsInPlay();
		foreach (Network.Options.Option option in m_options.List)
		{
			if (!option.Main.IsValidTarget(heldEntity.GetEntityId()))
			{
				continue;
			}
			Card card = moveMinionHoverTargets.FirstOrDefault((Card t) => t.GetEntity().GetEntityId() == option.Main.ID);
			if (!(card == null))
			{
				if (heldEntity.HasTag(GAME_TAG.BACON_COSTS_HEALTH_TO_BUY))
				{
					return true;
				}
				if (friendlyPlayer.GetNumAvailableResources() >= card.GetEntity().GetCost())
				{
					return true;
				}
			}
		}
		return moveMinionHoverTargets.Count <= 0;
	}

	private List<Card> GetMoveMinionHoverTargetsInPlay()
	{
		List<ZoneMoveMinionHoverTarget> list = ZoneMgr.Get().FindZonesOfType<ZoneMoveMinionHoverTarget>(Player.Side.FRIENDLY);
		List<Card> moveMinionHoverTargets = new List<Card>();
		list.ForEach(delegate(ZoneMoveMinionHoverTarget z)
		{
			moveMinionHoverTargets.AddRange(z.GetCards());
		});
		return moveMinionHoverTargets;
	}

	public Network.Options GetLastOptions()
	{
		return m_lastOptions;
	}

	public bool FriendlyHeroIsTargetable()
	{
		if (m_responseMode == ResponseMode.OPTION_TARGET)
		{
			Network.Options.Option option = m_options.List[m_selectedOption.m_main];
			foreach (Network.Options.Option.TargetOption targetOption in ((m_selectedOption.m_sub != -1) ? option.Subs[m_selectedOption.m_sub] : option.Main).Targets)
			{
				if (targetOption.PlayErrorInfo.IsValid())
				{
					int targetID = targetOption.ID;
					Entity entity = GetEntity(targetID);
					if (entity != null && !(entity.GetCard() == null) && entity.IsHero() && entity.IsControlledByFriendlySidePlayer())
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	private void ClearLastOptions()
	{
		m_lastOptions = null;
		m_lastSelectedOption = null;
	}

	private void ClearOptions()
	{
		m_options = null;
		m_selectedOption.Clear();
	}

	public void ClearFriendlyChoicesList()
	{
		m_chosenEntities.Clear();
	}

	private void ClearFriendlyChoices()
	{
		m_chosenEntities.Clear();
		int friendlyPlayerId = GetFriendlyPlayerId();
		m_choicesMap.Remove(friendlyPlayerId);
	}

	private void OnSelectedOptionsSent()
	{
		ClearResponseMode();
		m_lastOptions = new Network.Options();
		m_lastOptions.CopyFrom(m_options);
		m_lastSelectedOption = new SelectedOption();
		m_lastSelectedOption.CopyFrom(m_selectedOption);
		ClearOptions();
	}

	private void OnTimeout()
	{
		if (m_responseMode != 0)
		{
			ClearResponseMode();
			ClearLastOptions();
			ClearOptions();
		}
	}

	private void ClearEntityMap()
	{
		Entity[] entitiesToDestroy = m_entityMap.Values.ToArray();
		for (int i = 0; i < entitiesToDestroy.Length; i++)
		{
			entitiesToDestroy[i].Destroy();
		}
		m_entityMap.Clear();
	}

	private void CleanGameState()
	{
		foreach (Zone zone in ZoneMgr.Get().GetZones())
		{
			zone.Reset();
		}
		ManaCrystalMgr.Get().Reset();
		foreach (Entity value in m_entityMap.Values)
		{
			Card card = value.GetCard();
			if (card != null)
			{
				card.DeactivatePlaySpell();
				card.CancelActiveSpells();
				card.CancelCustomSpells();
			}
		}
		foreach (Entity value2 in m_entityMap.Values)
		{
			Card card2 = value2.GetCard();
			if (card2 != null)
			{
				card2.Destroy();
			}
		}
		m_playerMap.Clear();
		m_entityMap.Clear();
		m_removedFromGameEntities.Clear();
		m_removedFromGameEntityLog.Clear();
	}

	private void CreateGameEntity(List<Network.PowerHistory> powerList, Network.HistCreateGame createGame)
	{
		m_gameEntity = GameMgr.Get().CreateGameEntity(powerList, createGame);
		m_gameEntity.Uuid = createGame.Uuid;
		m_gameEntity.SetTags(createGame.Game.Tags);
		m_gameEntity.InitRealTimeValues(createGame.Game.Tags);
		AddEntity(m_gameEntity);
		m_gameEntity.OnCreate();
		m_gameEntity.OnLoadActions.AddRange(createGame.ActionInfos);
	}

	public void OnRealTimeCreateGame(List<Network.PowerHistory> powerList, int index, Network.HistCreateGame createGame)
	{
		if (m_gameEntity != null)
		{
			Log.Power.PrintError("{0}.OnRealTimeCreateGame(): there is already a game entity!", this);
			m_gameEntity.OnDecommissionGame();
			CleanGameState();
		}
		if (powerList.Count == 1)
		{
			string telemetryString = "Game Created without entries:";
			telemetryString += $" BuildNumber={216423}";
			telemetryString += $" GameType={GameMgr.Get().GetGameType()}";
			telemetryString += $" FormatType={GameMgr.Get().GetFormatType()}";
			telemetryString += $" ScenarioID={GameMgr.Get().GetMissionId()}";
			telemetryString += $" IsReconnect={GameMgr.Get().IsReconnect()}";
			if (GameMgr.Get().IsReconnect())
			{
				telemetryString += $" ReconnectType={GameMgr.Get().GetReconnectType()}";
			}
			Log.Power.Print(telemetryString);
			TelemetryManager.Client().SendLiveIssue("Gameplay_GameState", telemetryString);
		}
		CreateGameEntity(powerList, createGame);
		foreach (Network.HistCreateGame.PlayerData netPlayer in createGame.Players)
		{
			Player player = new Player();
			player.InitPlayer(netPlayer);
			AddPlayer(player);
		}
		int friendlySideTeamId = GetFriendlySideTeamId();
		foreach (Player value in m_playerMap.Values)
		{
			value.UpdateSide(friendlySideTeamId);
		}
		foreach (Network.HistCreateGame.SharedPlayerInfo netPlayerInfo in createGame.PlayerInfos)
		{
			SharedPlayerInfo playerInfo = new SharedPlayerInfo();
			playerInfo.InitPlayerInfo(netPlayerInfo);
			AddPlayerInfo(playerInfo);
		}
		m_createGamePhase = CreateGamePhase.CREATING;
		FireCreateGameEvent();
		if (m_gameEntity.HasTag(GAME_TAG.WAIT_FOR_PLAYER_RECONNECT_PERIOD))
		{
			HandleWaitForOpponentReconnectPeriod(m_gameEntity.GetTag(GAME_TAG.WAIT_FOR_PLAYER_RECONNECT_PERIOD));
		}
		DebugPrintGame();
	}

	public bool OnRealTimeFullEntity(Network.HistFullEntity fullEntity)
	{
		Entity entity = new Entity();
		entity.OnRealTimeFullEntity(fullEntity);
		AddEntity(entity);
		return true;
	}

	public bool OnFullEntity(Network.HistFullEntity fullEntity)
	{
		Network.Entity netEnt = fullEntity.Entity;
		Entity entity = GetEntity(netEnt.ID);
		if (entity == null)
		{
			Log.Power.PrintWarning("GameState.OnFullEntity() - WARNING entity {0} DOES NOT EXIST!", netEnt.ID);
			return false;
		}
		entity.OnFullEntity(fullEntity);
		return true;
	}

	public bool OnRealTimeShowEntity(Network.HistShowEntity showEntity)
	{
		if (EntityRemovedFromGame(showEntity.Entity.ID))
		{
			return false;
		}
		Network.Entity netEnt = showEntity.Entity;
		Entity entity = GetEntity(netEnt.ID);
		if (entity == null)
		{
			Log.Power.PrintWarning("GameState.OnRealTimeShowEntity() - WARNING entity {0} DOES NOT EXIST!", netEnt.ID);
			return false;
		}
		entity.OnRealTimeShowEntity(showEntity);
		return true;
	}

	public bool OnShowEntity(Network.HistShowEntity showEntity)
	{
		if (EntityRemovedFromGame(showEntity.Entity.ID))
		{
			return false;
		}
		Network.Entity netEnt = showEntity.Entity;
		Entity entity = GetEntity(netEnt.ID);
		if (entity == null)
		{
			Log.Power.PrintWarning("GameState.OnShowEntity() - WARNING entity {0} DOES NOT EXIST!", netEnt.ID);
			return false;
		}
		entity.OnShowEntity(showEntity);
		return true;
	}

	public bool OnEarlyConcedeShowEntity(Network.HistShowEntity showEntity)
	{
		if (EntityRemovedFromGame(showEntity.Entity.ID))
		{
			return false;
		}
		Network.Entity netEnt = showEntity.Entity;
		Entity entity = GetEntity(netEnt.ID);
		if (entity == null)
		{
			Log.Power.PrintWarning("GameState.OnEarlyConcedeShowEntity() - WARNING entity {0} DOES NOT EXIST!", netEnt.ID);
			return false;
		}
		entity.SetTags(netEnt.Tags);
		return true;
	}

	public bool OnHideEntity(Network.HistHideEntity hideEntity)
	{
		if (EntityRemovedFromGame(hideEntity.Entity))
		{
			return false;
		}
		Entity entity = GetEntity(hideEntity.Entity);
		if (entity == null)
		{
			Log.Power.PrintWarning("GameState.OnHideEntity() - WARNING entity {0} DOES NOT EXIST! zone={1}", hideEntity.Entity, hideEntity.Zone);
			return false;
		}
		entity.OnHideEntity(hideEntity);
		return true;
	}

	public bool OnEarlyConcedeHideEntity(Network.HistHideEntity hideEntity)
	{
		if (EntityRemovedFromGame(hideEntity.Entity))
		{
			return false;
		}
		Entity entity = GetEntity(hideEntity.Entity);
		if (entity == null)
		{
			Log.Power.PrintWarning("GameState.OnEarlyConcedeHideEntity() - WARNING entity {0} DOES NOT EXIST! zone={1}", hideEntity.Entity, hideEntity.Zone);
			return false;
		}
		entity.SetTag(GAME_TAG.ZONE, hideEntity.Zone);
		return true;
	}

	public bool OnRealTimeChangeEntity(List<Network.PowerHistory> powerList, int index, Network.HistChangeEntity changeEntity)
	{
		if (EntityRemovedFromGame(changeEntity.Entity.ID))
		{
			return false;
		}
		Network.Entity netEnt = changeEntity.Entity;
		Entity entity = GetEntity(netEnt.ID);
		if (entity == null)
		{
			Log.Power.PrintWarning("GameState.OnRealTimeChangeEntity() - WARNING entity {0} DOES NOT EXIST!", netEnt.ID);
			return false;
		}
		entity.OnRealTimeChangeEntity(powerList, index, changeEntity);
		return true;
	}

	public bool OnChangeEntity(Network.HistChangeEntity changeEntity)
	{
		if (EntityRemovedFromGame(changeEntity.Entity.ID))
		{
			return false;
		}
		Network.Entity netEnt = changeEntity.Entity;
		Entity entity = GetEntity(netEnt.ID);
		if (entity == null)
		{
			Log.Power.PrintWarning("GameState.OnChangeEntity() - WARNING entity {0} DOES NOT EXIST!", netEnt.ID);
			return false;
		}
		entity.OnChangeEntity(changeEntity);
		return true;
	}

	public bool OnEarlyConcedeChangeEntity(Network.HistChangeEntity changeEntity)
	{
		if (EntityRemovedFromGame(changeEntity.Entity.ID))
		{
			return false;
		}
		Network.Entity netEnt = changeEntity.Entity;
		Entity entity = GetEntity(netEnt.ID);
		if (entity == null)
		{
			Log.Power.PrintWarning("GameState.OnEarlyConcedeChangeEntity() - WARNING entity {0} DOES NOT EXIST!", netEnt.ID);
			return false;
		}
		entity.SetTags(netEnt.Tags);
		return true;
	}

	public bool OnRealTimeTagChange(Network.HistTagChange change)
	{
		if (change == null)
		{
			Log.Power.PrintError("GameState.OnRealTimeTagChange() - ERROR HistTagChange is NULL");
			return false;
		}
		if (EntityRemovedFromGame(change.Entity))
		{
			return false;
		}
		Entity entity = null;
		if (!m_entityMap.TryGetValue(change.Entity, out entity))
		{
			Log.Power.PrintWarning($"GameState.OnRealTimeTagChange() - WARNING Entity {change.Entity} does not exist");
			return false;
		}
		if (entity == null)
		{
			Log.Power.PrintWarning($"GameState.OnRealTimeTagChange() - WARNING Entity {change.Entity} is mapped to a NULL Entity");
			return false;
		}
		if (change.ChangeDef)
		{
			return false;
		}
		PreprocessRealTimeTagChange(entity, change);
		if (m_gameEntity == null)
		{
			Log.Power.PrintWarning("GameState.OnRealTimeTagChange() - WARNING GameEntity has been removed during RealTimeTagChange");
			return false;
		}
		m_gameEntity.NotifyOfRealTimeTagChange(entity, change);
		entity.OnRealTimeTagChanged(change);
		return true;
	}

	public bool OnTagChange(Network.HistTagChange netChange)
	{
		if (EntityRemovedFromGame(netChange.Entity))
		{
			return false;
		}
		Entity entity = GetEntity(netChange.Entity);
		if (entity == null)
		{
			Debug.LogWarningFormat("GameState.OnTagChange() - WARNING Entity {0} does not exist", netChange.Entity);
			return false;
		}
		TagDelta change = new TagDelta();
		change.tag = netChange.Tag;
		change.oldValue = entity.GetTag(netChange.Tag);
		change.newValue = netChange.Value;
		if (netChange.ChangeDef)
		{
			entity.GetOrCreateDynamicDefinition().SetTag(change.tag, change.newValue);
		}
		else
		{
			entity.SetTag(change.tag, change.newValue);
		}
		PreprocessTagChange(entity, change);
		entity.OnTagChanged(change);
		return true;
	}

	public bool OnTagListChange(Network.HistTagListChange netChange)
	{
		if (EntityRemovedFromGame(netChange.Entity))
		{
			return false;
		}
		Entity entity = GetEntity(netChange.Entity);
		if (entity == null)
		{
			Debug.LogWarningFormat("GameState.OnTagListChange() - WARNING Entity {0} does not exist", netChange.Entity);
			return false;
		}
		if (netChange.ChangeDef)
		{
			entity.GetOrCreateDynamicDefinition().SetTagList(netChange.Tag, netChange.Values);
		}
		else
		{
			entity.SetTagList(netChange.Tag, netChange.Values);
		}
		PreprocessTagListChange(entity, netChange);
		entity.OnTagListChanged(netChange.Tag, netChange.Values);
		return true;
	}

	public void OnRealTimeVoSpell(Network.HistVoSpell voSpell)
	{
		if (voSpell != null)
		{
			SoundLoader.LoadSound(new AssetReference(voSpell.SpellPrefabGUID), OnSoundLoaded, voSpell, SoundManager.Get().GetPlaceholderSound());
		}
	}

	public bool OnCachedTagForDormantChange(Network.HistCachedTagForDormantChange netChange)
	{
		if (EntityRemovedFromGame(netChange.Entity))
		{
			return false;
		}
		Entity entity = GetEntity(netChange.Entity);
		if (entity == null)
		{
			Debug.LogWarningFormat("GameState.OnCachedTagForDormantChange() - WARNING Entity {0} does not exist", netChange.Entity);
			return false;
		}
		TagDelta change = new TagDelta();
		change.tag = netChange.Tag;
		change.oldValue = entity.GetTag(netChange.Tag);
		change.newValue = netChange.Value;
		entity.OnCachedTagForDormantChanged(change);
		return true;
	}

	public bool OnShuffleDeck(Network.HistShuffleDeck shuffleDeck)
	{
		Player player = GetPlayer(shuffleDeck.PlayerID);
		if (player == null)
		{
			Debug.LogWarningFormat("GameState.OnShuffleDeck() - WARNING Player for ID {0} does not exist", shuffleDeck.PlayerID);
			return false;
		}
		if (EntityRemovedFromGame(player.GetEntityId()))
		{
			return false;
		}
		player.OnShuffleDeck();
		return true;
	}

	private void OnSoundLoaded(AssetReference assetRef, GameObject go, object callbackData)
	{
		if (go == null)
		{
			Debug.LogWarning($"{MethodBase.GetCurrentMethod().Name} - FAILED to load \"{assetRef}\"");
			return;
		}
		AudioSource source = go.GetComponent<AudioSource>();
		if (source == null)
		{
			Debug.LogWarning(string.Format("{0} - ERROR \"{1}\" has no {2} component", assetRef, MethodBase.GetCurrentMethod().Name, "AudioSource"));
		}
		else if (callbackData is Network.HistVoSpell voSpell)
		{
			voSpell.m_audioSource = source;
			voSpell.m_ableToLoad = true;
		}
	}

	public bool OnVoSpell(Network.HistVoSpell voSpell)
	{
		if (voSpell == null)
		{
			return false;
		}
		if (!voSpell.m_ableToLoad)
		{
			return false;
		}
		AudioSource sound = voSpell.m_audioSource;
		if (sound == null || sound.clip == null)
		{
			return false;
		}
		float num = voSpell.AdditionalDelayMs;
		float soundLength = sound.clip.length * 1000f;
		float totalPauseTimeMs = num;
		if (voSpell.Blocking)
		{
			totalPauseTimeMs += soundLength;
		}
		if (totalPauseTimeMs > 0f)
		{
			if (Gameplay.Get() != null)
			{
				m_powerProcessor.ArtificiallyPausePowerProcessor(totalPauseTimeMs, Gameplay.Get().PausePowerToken).Forget();
			}
			if (m_gameEntity is MissionEntity)
			{
				(m_gameEntity as MissionEntity).SetBlockVo(shouldBlock: true, totalPauseTimeMs / 1000f);
			}
		}
		string[] prefabNameWithGuid = voSpell.SpellPrefabGUID.Split(':');
		if (prefabNameWithGuid.Length != 2)
		{
			return false;
		}
		string prefabNameOnly = prefabNameWithGuid[0];
		if (!prefabNameOnly.EndsWith(".prefab"))
		{
			return false;
		}
		string localizedTextKey = prefabNameOnly.Substring(0, prefabNameOnly.Length - ".prefab".Length);
		if (voSpell.Speaker != 0)
		{
			Actor speakingActor = GetEntity(voSpell.Speaker)?.GetCard()?.GetActor();
			if (speakingActor != null)
			{
				CharacterInPlaySpeak(voSpell, speakingActor, localizedTextKey, totalPauseTimeMs);
			}
		}
		else if (!string.IsNullOrEmpty(voSpell.BrassRingGUID))
		{
			BrassRingCharacterSpeak(voSpell, localizedTextKey, totalPauseTimeMs);
		}
		return true;
	}

	private void CharacterInPlaySpeak(Network.HistVoSpell voSpell, Actor speakingActor, string localizedTextKey, float totalPauseTimeMs)
	{
		if (voSpell != null && !(speakingActor == null) && !string.IsNullOrEmpty(localizedTextKey) && !(totalPauseTimeMs < 0f))
		{
			if (voSpell.m_audioSource != null)
			{
				SoundManager.Get().PlayPreloaded(voSpell.m_audioSource);
			}
			Notification.SpeechBubbleDirection bubbleDirection = Notification.SpeechBubbleDirection.None;
			Entity speakingEntity = speakingActor.GetEntity();
			bubbleDirection = (speakingEntity.IsControlledByFriendlySidePlayer() ? Notification.SpeechBubbleDirection.BottomLeft : ((!speakingEntity.IsMinion()) ? Notification.SpeechBubbleDirection.TopLeft : Notification.SpeechBubbleDirection.BottomLeft));
			if (totalPauseTimeMs > 0f && bubbleDirection != 0)
			{
				NotificationManager notificationManager = NotificationManager.Get();
				Notification notification = notificationManager.CreateSpeechBubble(parentToActor: !(speakingActor.GetCard() != null) || speakingActor.GetCard().GetEntity() == null || !speakingActor.GetCard().GetEntity().IsHeroPower(), speechText: GameStrings.Get(localizedTextKey), direction: bubbleDirection, actor: speakingActor, bDestroyWhenNewCreated: false);
				notificationManager.DestroyNotification(notification, totalPauseTimeMs / 1000f);
			}
		}
	}

	private void BrassRingCharacterSpeak(Network.HistVoSpell voSpell, string localizedTextKey, float soundLengthMs)
	{
		if (voSpell != null && !string.IsNullOrEmpty(localizedTextKey) && !(soundLengthMs <= 0f))
		{
			NotificationManager notificationMgr = NotificationManager.Get();
			if (!(notificationMgr == null))
			{
				Vector3 position = Vector3.zero;
				Notification.SpeechBubbleDirection bubbleDirection = Notification.SpeechBubbleDirection.None;
				notificationMgr.CreateBigCharacterQuoteWithGameString(voSpell.BrassRingGUID, position, voSpell.SpellPrefabGUID, localizedTextKey, allowRepeatDuringSession: true, soundLengthMs / 1000f, null, useOverlayUI: false, bubbleDirection);
			}
		}
	}

	public bool OnVoBanter(Network.HistVoBanter voBanter)
	{
		if (voBanter == null)
		{
			return false;
		}
		if (voBanter.EmoteEvent == PowerHistoryVoBanter.ClientEmoteEvent.INVALID)
		{
			return false;
		}
		if (m_gameEntity is LettuceMissionEntity letlMissionEntity)
		{
			if (voBanter.Speaker != 0)
			{
				letlMissionEntity.OnVoBanter_OneSpeaker(voBanter.Speaker, voBanter.EmoteEvent);
				return true;
			}
			if (voBanter.Teams != null && voBanter.Teams.Count > 0)
			{
				letlMissionEntity.OnVoBanter_TeamDialogue(voBanter.Teams, voBanter.EmoteEvent);
				return true;
			}
		}
		return false;
	}

	public bool OnEarlyConcedeTagChange(Network.HistTagChange netChange)
	{
		if (EntityRemovedFromGame(netChange.Entity))
		{
			return false;
		}
		Entity entity = GetEntity(netChange.Entity);
		if (entity == null)
		{
			Debug.LogWarningFormat("GameState.OnEarlyConcedeTagChange() - WARNING Entity {0} does not exist", netChange.Entity);
			return false;
		}
		TagDelta change = new TagDelta();
		change.tag = netChange.Tag;
		change.oldValue = entity.GetTag(netChange.Tag);
		change.newValue = netChange.Value;
		entity.SetTag(change.tag, change.newValue);
		PreprocessEarlyConcedeTagChange(entity, change);
		ProcessEarlyConcedeTagChange(entity, change);
		return true;
	}

	public bool OnRealTimeResetGame(Network.HistResetGame resetGame)
	{
		if (m_realTimeResetGame != null)
		{
			Log.Gameplay.PrintError("{0}.OnRealTimeResetGame: There is already a ResetGame task we're waiting to execute!", this);
		}
		m_realTimeResetGame = resetGame;
		foreach (Zone zone in ZoneMgr.Get().GetZones())
		{
			zone.AddInputBlocker();
		}
		return true;
	}

	public bool OnResetGame(Network.HistResetGame resetGame)
	{
		if (m_realTimeResetGame != resetGame)
		{
			Log.Power.PrintError("{0}.OnResetGame(): Passed ResetGame Task {0} does not match the expected ResetGame Task {1}!", this, resetGame, m_realTimeResetGame);
		}
		if (m_gameEntity != null)
		{
			m_gameEntity.OnDecommissionGame();
			CleanGameState();
		}
		List<Network.PowerHistory> powerList = new List<Network.PowerHistory>();
		foreach (PowerTask task in m_powerProcessor.GetCurrentTaskList().GetTaskList())
		{
			powerList.Add(task.GetPower());
		}
		CreateGameEntity(powerList, resetGame.CreateGame);
		foreach (Network.HistCreateGame.PlayerData netPlayer in resetGame.CreateGame.Players)
		{
			Player player = new Player();
			player.InitPlayer(netPlayer);
			AddPlayer(player);
		}
		int friendlySideTeamId = GetFriendlySideTeamId();
		foreach (Player value in m_playerMap.Values)
		{
			value.UpdateSide(friendlySideTeamId);
			value.OnBoardLoaded();
		}
		m_realTimeResetGame = null;
		m_powerProcessor.FlushDelayedRealTimeTasks();
		return true;
	}

	public bool OnMetaData(Network.HistMetaData metaData)
	{
		m_powerProcessor.OnMetaData(metaData);
		HistoryMeta.Type metaType = metaData.MetaType;
		if (metaType == HistoryMeta.Type.SHOW_BIG_CARD || metaType == HistoryMeta.Type.CONTROLLER_AND_ZONE_CHANGE)
		{
			if (metaData.Info.Count == 0)
			{
				return false;
			}
			int entityID = metaData.Info[0];
			Entity entity = GetEntity(entityID);
			if (entity == null)
			{
				if (!EntityRemovedFromGame(entityID))
				{
					Debug.LogWarning($"GameState.OnMetaData() - WARNING Entity {entityID} does not exist");
				}
				return false;
			}
			entity.OnMetaData(metaData);
		}
		else
		{
			foreach (int entityID2 in metaData.Info)
			{
				Entity entity2 = GetEntity(entityID2);
				if (entity2 == null)
				{
					if (!EntityRemovedFromGame(entityID2))
					{
						Debug.LogWarning($"GameState.OnMetaData() - WARNING Entity {entityID2} does not exist");
					}
					return false;
				}
				entity2.OnMetaData(metaData);
			}
		}
		return true;
	}

	public void OnTaskListEnded(PowerTaskList taskList)
	{
		if (taskList == null)
		{
			return;
		}
		foreach (PowerTask task in taskList.GetTaskList())
		{
			if (task.GetPower().Type == Network.PowerType.CREATE_GAME)
			{
				m_createGamePhase = CreateGamePhase.CREATED;
				FireCreateGameEvent();
				m_createGameListeners.Clear();
			}
		}
		RemoveQueuedEntitiesFromGame();
	}

	public void OnPowerHistory(List<Network.PowerHistory> powerList)
	{
		DebugPrintPowerList(powerList);
		bool num = m_powerProcessor.HasEarlyConcedeTaskList();
		m_powerProcessor.OnPowerHistory(powerList);
		ProcessAllQueuedChoices();
		bool hasEarlyConcede = m_powerProcessor.HasEarlyConcedeTaskList();
		if (!num && hasEarlyConcede)
		{
			OnReceivedEarlyConcede();
		}
	}

	private void OnReceivedEarlyConcede()
	{
		ClearResponseMode();
		ClearLastOptions();
		ClearOptions();
	}

	public void OnAllOptions(Network.Options options)
	{
		ResponseMode previousResponseMode = m_responseMode;
		if (IsInTargetMode() && InputManager.Get() != null)
		{
			InputManager.Get().PauseTargetMode();
		}
		m_responseMode = ResponseMode.OPTION;
		m_chosenEntities.Clear();
		if (m_options != null && (m_lastOptions == null || m_lastOptions.ID < m_options.ID))
		{
			m_lastOptions = new Network.Options();
			m_lastOptions.CopyFrom(m_options);
		}
		m_options = options;
		foreach (Network.Options.Option option in m_options.List)
		{
			if (option.Type == Network.Options.Option.OptionType.POWER)
			{
				Entity entity = GetEntity(option.Main.ID);
				if (entity != null && option.Main.Targets != null && option.Main.Targets.Count > 0)
				{
					entity.UpdateUseBattlecryFlag(fromGameState: true);
				}
			}
		}
		if (GameScenarioAllowsPowerPrinting())
		{
			DebugPrintOptions(Log.Power);
		}
		if (previousResponseMode == ResponseMode.OPTION_TARGET && InputManager.Get() != null && InputManager.Get().IsPaused())
		{
			m_responseMode = ResponseMode.OPTION_TARGET;
			if (!ResolveExistingTargetWithNewOptions())
			{
				InputManager.Get().CancelTargetMode();
				EnterMainOptionMode();
			}
			else
			{
				if (m_responseMode == ResponseMode.OPTION_TARGET)
				{
					Network.Options.Option option2 = m_options.List[m_selectedOption.m_main];
					UpdateTargetHighlights(option2.Main);
					if (m_selectedOption.m_sub != -1)
					{
						Network.Options.Option.SubOption subOption = option2.Subs[m_selectedOption.m_sub];
						UpdateTargetHighlights(subOption);
					}
				}
				UpdateOptionHighlights(m_lastOptions);
				UpdateOptionHighlights();
			}
		}
		else if (previousResponseMode == ResponseMode.SUB_OPTION)
		{
			m_responseMode = ResponseMode.SUB_OPTION;
			if (!ResolveExistingTargetWithNewOptions())
			{
				if (InputManager.Get() != null)
				{
					InputManager.Get().CancelSubOptionMode();
				}
				EnterMainOptionMode();
			}
			else
			{
				m_responseMode = ResponseMode.OPTION;
				EnterSubOptionMode();
			}
		}
		else
		{
			EnterMainOptionMode();
		}
		FireOptionsReceivedEvent();
	}

	private bool ResolveExistingTargetWithNewOptions()
	{
		if (m_selectedOption == null || m_selectedOption.m_main == -1)
		{
			return false;
		}
		if (m_lastOptions == null)
		{
			return false;
		}
		int lastSelectedOptionEntityId = 0;
		if (m_selectedOption.m_main < m_lastOptions.List.Count)
		{
			lastSelectedOptionEntityId = m_lastOptions.List[m_selectedOption.m_main].Main.ID;
		}
		if (lastSelectedOptionEntityId == 0)
		{
			return false;
		}
		for (int i = 0; i < m_options.List.Count; i++)
		{
			if (GameScenarioAllowsPowerPrinting())
			{
				DebugPrintOptions(Log.Power);
			}
			if (m_options.List[i].Main.ID == lastSelectedOptionEntityId)
			{
				m_selectedOption.m_main = i;
				return true;
			}
		}
		return false;
	}

	public void OnEntityChoices(Network.EntityChoices choices)
	{
		PowerTaskList preChoiceTaskList = m_powerProcessor.GetLastTaskList();
		if (!CanProcessEntityChoices(choices))
		{
			if (GameScenarioAllowsPowerPrinting())
			{
				Log.Power.Print("GameState.OnEntityChoices() - id={0} playerId={1} queued", choices.ID, choices.PlayerId);
			}
			QueuedChoice queuedChoice = new QueuedChoice
			{
				m_type = QueuedChoice.PacketType.ENTITY_CHOICES,
				m_packet = choices,
				m_eventData = preChoiceTaskList
			};
			m_queuedChoices.Enqueue(queuedChoice);
		}
		else
		{
			ProcessEntityChoices(choices, preChoiceTaskList);
		}
	}

	public void OnEntitiesChosen(Network.EntitiesChosen chosen)
	{
		if (!CanProcessEntitiesChosen(chosen))
		{
			if (GameScenarioAllowsPowerPrinting())
			{
				Log.Power.Print("GameState.OnEntitiesChosen() - id={0} playerId={1} queued", chosen.ID, chosen.PlayerId);
			}
			QueuedChoice queuedChoice = new QueuedChoice
			{
				m_type = QueuedChoice.PacketType.ENTITIES_CHOSEN,
				m_packet = chosen
			};
			m_queuedChoices.Enqueue(queuedChoice);
		}
		else
		{
			ProcessEntitiesChosen(chosen);
		}
	}

	public float GetClientLostTimeCatchUpThreshold()
	{
		return m_clientLostTimeCatchUpThreshold;
	}

	public bool ShouldUseSlushTimeTracker()
	{
		return m_useSlushTimeCatchUp;
	}

	public bool ShoudRestrictLostTimeCatchUpToLowEndDevices()
	{
		return m_restrictClientLostTimeCatchUpToLowEndDevices;
	}

	public void SetBattlegroundAllowBuddies(bool value)
	{
		bool num = m_battlegroundAllowBuddies != value;
		m_battlegroundAllowBuddies = value;
		if (num)
		{
			PlayerLeaderboardManager.Get()?.NotifyBattlegroundHeroBuddyEnabledDirty();
		}
	}

	public void SetBattlegroundsAllowQuestRewards(bool value)
	{
		if (m_battlegroundsAllowQuestRewards != value)
		{
			m_battlegroundsAllowQuestRewards = value;
			PlayerLeaderboardManager.Get()?.NotifyBattlegroundsQuestRewardEnabledDirty();
		}
	}

	public void SetBattlegroundsAllowTrinkets(bool value)
	{
		if (m_battlegroundsAllowTrinkets != value)
		{
			m_battlegroundsAllowTrinkets = value;
			PlayerLeaderboardManager.Get()?.NotifyBattlegroundsTrinketEnabledDirty();
		}
	}

	public void UpdateGameGuardianVars(GameGuardianVars gameGuardianVars)
	{
		m_clientLostTimeCatchUpThreshold = (gameGuardianVars.HasClientLostFrameTimeCatchUpThreshold ? gameGuardianVars.ClientLostFrameTimeCatchUpThreshold : 0f);
		m_useSlushTimeCatchUp = gameGuardianVars.HasClientLostFrameTimeCatchUpUseSlush && gameGuardianVars.ClientLostFrameTimeCatchUpUseSlush;
		m_restrictClientLostTimeCatchUpToLowEndDevices = gameGuardianVars.HasClientLostFrameTimeCatchUpLowEndOnly && gameGuardianVars.ClientLostFrameTimeCatchUpLowEndOnly;
		m_allowDeferredPowers = !gameGuardianVars.HasGameAllowDeferredPowers || gameGuardianVars.GameAllowDeferredPowers;
		m_allowBatchedPowers = !gameGuardianVars.HasGameAllowBatchedPowers || gameGuardianVars.GameAllowBatchedPowers;
		m_allowDiamondCards = !gameGuardianVars.HasGameAllowDiamondCards || gameGuardianVars.GameAllowDiamondCards;
		m_allowSignatureCards = !gameGuardianVars.HasGameAllowSignatureCards || gameGuardianVars.GameAllowSignatureCards;
		SetBattlegroundAllowBuddies(!gameGuardianVars.HasBattlegroundAllowBuddies || gameGuardianVars.BattlegroundAllowBuddies);
		SetBattlegroundsAllowQuestRewards(!gameGuardianVars.HasBattlegroundsAllowQuestRewards || gameGuardianVars.BattlegroundsAllowQuestRewards);
	}

	public void UpdateBattlegroundInfo(UpdateBattlegroundInfo battlegroundMinionPoolDenyList)
	{
		m_battlegroundMinionPool = (battlegroundMinionPoolDenyList.HasBattlegroundMinionPool ? battlegroundMinionPoolDenyList.BattlegroundMinionPool : "Battleground minion pool not available");
		if (m_printBattlegroundMinionPoolOnUpdate)
		{
			Log.All.Print(m_battlegroundMinionPool);
			m_printBattlegroundMinionPoolOnUpdate = false;
		}
		m_battlegroundDenyList = (battlegroundMinionPoolDenyList.HasBattlegroundDenyList ? battlegroundMinionPoolDenyList.BattlegroundDenyList : "Battle ground deny list not available");
		if (m_printBattlegroundDenyListOnUpdate)
		{
			Log.All.Print(m_battlegroundDenyList);
			m_printBattlegroundDenyListOnUpdate = false;
		}
	}

	public void UpdateBattlegroundArmorTierList(GetBattlegroundHeroArmorTierList battlegroundHeroArmorTierList)
	{
		m_battlegroundHeroArmorTierList = (battlegroundHeroArmorTierList.HasBattlegroundHeroArmorTierList ? battlegroundHeroArmorTierList.BattlegroundHeroArmorTierList : "Battle ground hero armor tier list not available");
		if (m_printBattlegroundHeroArmorTierListUpdate)
		{
			Log.All.Print(m_battlegroundHeroArmorTierList);
			m_printBattlegroundHeroArmorTierListUpdate = false;
		}
	}

	public void UpdateBattlegroundsPlayerAnomaly(GetBattlegroundsPlayerAnomaly battlegroundsPlayerAnomaly)
	{
		m_battlegroundsPlayerAnomaly = (battlegroundsPlayerAnomaly.HasBattlegroundsAnomalyList ? battlegroundsPlayerAnomaly.BattlegroundsAnomalyList : "Battlegrounds anomaly list not available");
		if (m_printBattlegroundsAnomalyUpdate)
		{
			Log.All.Print(m_battlegroundsPlayerAnomaly);
			m_printBattlegroundsAnomalyUpdate = false;
		}
	}

	private bool CanProcessEntityChoices(Network.EntityChoices choices)
	{
		int playerId = choices.PlayerId;
		if (!m_playerMap.ContainsKey(playerId))
		{
			return false;
		}
		foreach (int entityId in choices.Entities)
		{
			if (!m_entityMap.ContainsKey(entityId))
			{
				return false;
			}
		}
		if (m_choicesMap.ContainsKey(playerId))
		{
			return false;
		}
		return true;
	}

	private bool CanProcessEntitiesChosen(Network.EntitiesChosen chosen)
	{
		int playerId = chosen.PlayerId;
		if (!m_playerMap.ContainsKey(playerId))
		{
			return false;
		}
		foreach (int entityId in chosen.Entities)
		{
			if (!m_entityMap.ContainsKey(entityId))
			{
				return false;
			}
		}
		if (m_choicesMap.TryGetValue(playerId, out var choice) && choice.ID != chosen.ID)
		{
			return false;
		}
		return true;
	}

	private void ProcessAllQueuedChoices()
	{
		while (m_queuedChoices.Count > 0)
		{
			QueuedChoice queuedChoice = m_queuedChoices.Peek();
			switch (queuedChoice.m_type)
			{
			case QueuedChoice.PacketType.ENTITY_CHOICES:
			{
				Network.EntityChoices choices = (Network.EntityChoices)queuedChoice.m_packet;
				if (!CanProcessEntityChoices(choices))
				{
					return;
				}
				m_queuedChoices.Dequeue();
				PowerTaskList preChoiceTaskList = (PowerTaskList)queuedChoice.m_eventData;
				ProcessEntityChoices(choices, preChoiceTaskList);
				break;
			}
			case QueuedChoice.PacketType.ENTITIES_CHOSEN:
			{
				Network.EntitiesChosen chosen = (Network.EntitiesChosen)queuedChoice.m_packet;
				if (!CanProcessEntitiesChosen(chosen))
				{
					return;
				}
				m_queuedChoices.Dequeue();
				ProcessEntitiesChosen(chosen);
				break;
			}
			}
		}
	}

	private void ProcessEntityChoices(Network.EntityChoices choices, PowerTaskList preChoiceTaskList)
	{
		DebugPrintEntityChoices(choices, preChoiceTaskList);
		if (!m_powerProcessor.HasEarlyConcedeTaskList())
		{
			int playerId = choices.PlayerId;
			m_choicesMap[playerId] = choices;
			int friendlyPlayerId = GetFriendlyPlayerId();
			if (playerId == friendlyPlayerId)
			{
				m_responseMode = ResponseMode.CHOICE;
				m_chosenEntities.Clear();
				EnterChoiceMode();
			}
			FireEntityChoicesReceivedEvent(choices, preChoiceTaskList);
		}
	}

	private void ProcessEntitiesChosen(Network.EntitiesChosen chosen)
	{
		DebugPrintEntitiesChosen(chosen);
		if (!m_powerProcessor.HasEarlyConcedeTaskList() && !FireEntitiesChosenReceivedEvent(chosen))
		{
			OnEntitiesChosenProcessed(chosen);
		}
	}

	public void OnGameSetup(Network.GameSetup setup)
	{
		m_maxSecretZoneSizePerPlayer = setup.MaxSecretZoneSizePerPlayer;
		m_maxSecretsPerPlayer = setup.MaxSecretsPerPlayer;
		m_maxQuestsPerPlayer = setup.MaxQuestsPerPlayer;
		m_maxFriendlySlotsPerPlayer = setup.MaxFriendlyMinionsPerPlayer;
	}

	public void QueueEntityForRemoval(Entity entity)
	{
		m_removedFromGameEntities.Enqueue(entity);
	}

	public void OnOptionRejected(int optionId)
	{
		if (m_lastSelectedOption == null)
		{
			Debug.LogError("GameState.OnOptionRejected() - got an option rejection without a last selected option");
		}
		else if (m_lastOptions.ID != optionId)
		{
			Debug.LogErrorFormat("GameState.OnOptionRejected() - rejected option id ({0}) does not match last option id ({1})", optionId, m_lastOptions.ID);
		}
		else
		{
			Network.Options.Option selectedOption = m_lastOptions.List[m_lastSelectedOption.m_main];
			FireOptionRejectedEvent(selectedOption);
			ClearLastOptions();
		}
	}

	public void OnTurnTimerUpdate(Network.TurnTimerInfo info)
	{
		TurnTimerUpdate update = new TurnTimerUpdate();
		update.SetSecondsRemaining(info.Seconds);
		update.SetEndTimestamp(Time.realtimeSinceStartup + info.Seconds);
		update.SetShow(info.Show);
		if (IsMulliganManagerActive() && m_gameEntity != null && GetBooleanGameOption(GameEntityOption.ALWAYS_SHOW_MULLIGAN_TIMER))
		{
			update.SetShow(show: true);
		}
		int currentTurn = GetTurn();
		if (info.Turn > currentTurn)
		{
			m_turnTimerUpdates[info.Turn] = update;
		}
		else
		{
			TriggerTurnTimerUpdate(update);
		}
	}

	public void TriggerTurnTimerUpdateForTurn(int turn)
	{
		OnTurnChanged_TurnTimer(GetTurn(), turn);
	}

	public void OnSpectatorNotifyEvent(SpectatorNotify notify)
	{
		FireSpectatorNotifyEvent(notify);
	}

	public void SendChoices()
	{
		if (m_responseMode != ResponseMode.CHOICE)
		{
			return;
		}
		Network.EntityChoices choices = GetFriendlyEntityChoices();
		if (choices == null || m_chosenEntities.Count < choices.CountMin || m_chosenEntities.Count > choices.CountMax)
		{
			return;
		}
		ChoiceCardMgr.Get().OnSendChoices(choices, m_chosenEntities);
		bool canPrintPowers = GameScenarioAllowsPowerPrinting();
		if (canPrintPowers)
		{
			Log.Power.Print("GameState.SendChoices() - id={0} ChoiceType={1}", choices.ID, choices.ChoiceType);
		}
		List<int> chosenEntities = new List<int>();
		for (int i = 0; i < m_chosenEntities.Count; i++)
		{
			Entity entity = m_chosenEntities[i];
			int entityId = entity.GetEntityId();
			if (canPrintPowers)
			{
				Log.Power.Print("GameState.SendChoices() -   m_chosenEntities[{0}]={1}", i, entity);
			}
			chosenEntities.Add(entityId);
		}
		if (!GameMgr.Get().IsSpectator())
		{
			Network.Get().SendChoices(choices.ID, chosenEntities);
		}
		ClearResponseMode();
	}

	public void OnEntitiesChosenProcessed(Network.EntitiesChosen chosen)
	{
		int playerId = chosen.PlayerId;
		int friendlyPlayerId = GetFriendlyPlayerId();
		if (playerId == friendlyPlayerId)
		{
			if (m_responseMode == ResponseMode.CHOICE)
			{
				ClearResponseMode();
			}
			ClearFriendlyChoices();
		}
		else
		{
			m_choicesMap.Remove(playerId);
		}
		ProcessAllQueuedChoices();
	}

	public void SendOption()
	{
		if (!GameMgr.Get().IsSpectator())
		{
			Network.Get().SendOption(m_options.ID, m_selectedOption.m_main, m_selectedOption.m_target, m_selectedOption.m_sub, m_selectedOption.m_position);
			if (GameScenarioAllowsPowerPrinting())
			{
				Log.Power.Print("GameState.SendOption() - selectedOption={0} selectedSubOption={1} selectedTarget={2} selectedPosition={3}", m_selectedOption.m_main, m_selectedOption.m_sub, m_selectedOption.m_target, m_selectedOption.m_position);
			}
		}
		OnSelectedOptionsSent();
		Network.Options.Option selectedOption = m_lastOptions.List[m_lastSelectedOption.m_main];
		FireOptionsSentEvent(selectedOption);
	}

	private void OnTurnChanged_TurnTimer(int oldTurn, int newTurn)
	{
		if (m_turnTimerUpdates.Count != 0 && m_turnTimerUpdates.TryGetValue(newTurn, out var update))
		{
			float now = Time.realtimeSinceStartup;
			float end = update.GetEndTimestamp();
			float secondsRemaining = Mathf.Max(0f, end - now);
			update.SetSecondsRemaining(secondsRemaining);
			TriggerTurnTimerUpdate(update);
			m_turnTimerUpdates.Remove(newTurn);
		}
	}

	private void TriggerTurnTimerUpdate(TurnTimerUpdate update)
	{
		FireTurnTimerUpdateEvent(update);
		if (!(update.GetSecondsRemaining() > Mathf.Epsilon))
		{
			OnTimeout();
		}
	}

	private void DebugPrintGame()
	{
		if (!Log.Power.CanPrint() || !GameScenarioAllowsPowerPrinting())
		{
			return;
		}
		Log.Power.Print($"GameState.DebugPrintGame() - BuildNumber={216423}");
		Log.Power.Print($"GameState.DebugPrintGame() - GameType={GameMgr.Get().GetGameType()}");
		Log.Power.Print($"GameState.DebugPrintGame() - FormatType={GameMgr.Get().GetFormatType()}");
		Log.Power.Print($"GameState.DebugPrintGame() - ScenarioID={GameMgr.Get().GetMissionId()}");
		foreach (Player player in m_playerMap.Values)
		{
			Log.Power.Print($"GameState.DebugPrintGame() - PlayerID={player.GetPlayerId()}, PlayerName={GetEntityLogName(player.GetEntityId())}");
		}
	}

	private void DebugPrintPowerList(List<Network.PowerHistory> powerList)
	{
		if (Log.Power.CanPrint() && GameScenarioAllowsPowerPrinting())
		{
			string indentation = "";
			Log.Power.Print($"GameState.DebugPrintPowerList() - Count={powerList.Count}");
			for (int i = 0; i < powerList.Count; i++)
			{
				Network.PowerHistory power = powerList[i];
				DebugPrintPower(Log.Power, "GameState", power, ref indentation);
			}
		}
	}

	public bool GameScenarioAllowsPowerPrinting()
	{
		if (m_gameEntity == null)
		{
			return true;
		}
		GameEntityOptions options = m_gameEntity.GetGameOptions();
		if (options == null)
		{
			return true;
		}
		return !options.GetBooleanOption(GameEntityOption.DISABLE_POWER_LOGGING);
	}

	public void DebugPrintPower(Logger logger, string callerName, Network.PowerHistory power)
	{
		string indentation = string.Empty;
		DebugPrintPower(logger, callerName, power, ref indentation);
	}

	public void DebugPrintPower(Logger logger, string callerName, Network.PowerHistory power, ref string indentation)
	{
		if (!Log.Power.CanPrint() || !GameScenarioAllowsPowerPrinting())
		{
			return;
		}
		switch (power.Type)
		{
		case Network.PowerType.BLOCK_START:
		{
			Network.HistBlockStart blockStart = (Network.HistBlockStart)power;
			string triggerKeywordDebug = string.Empty;
			if (blockStart.BlockType == HistoryBlock.Type.TRIGGER)
			{
				string keywordName = ((GAME_TAG)blockStart.TriggerKeyword/*cast due to .constrained prefix*/).ToString();
				triggerKeywordDebug = $"TriggerKeyword={keywordName}";
			}
			logger.Print("{0}.DebugPrintPower() - {1}BLOCK_START BlockType={2} Entity={3} EffectCardId={4} EffectIndex={5} Target={6} SubOption={7} {8}", callerName, indentation, blockStart.BlockType, GetEntitiesLogNames(blockStart.Entities), blockStart.EffectCardId, blockStart.EffectIndex, GetEntityLogName(blockStart.Target), blockStart.SubOption, triggerKeywordDebug);
			indentation += "    ";
			break;
		}
		case Network.PowerType.BLOCK_END:
			if (indentation.Length >= "    ".Length)
			{
				indentation = indentation.Remove(indentation.Length - "    ".Length);
			}
			logger.Print("{0}.DebugPrintPower() - {1}BLOCK_END", callerName, indentation);
			break;
		case Network.PowerType.FULL_ENTITY:
		{
			Network.Entity netEntity = ((Network.HistFullEntity)power).Entity;
			Entity entity = GetEntity(netEntity.ID);
			if (entity == null)
			{
				logger.Print("{0}.DebugPrintPower() - {1}FULL_ENTITY - Creating ID={2} CardID={3}", callerName, indentation, netEntity.ID, netEntity.CardID);
			}
			else
			{
				logger.Print("{0}.DebugPrintPower() - {1}FULL_ENTITY - Updating {2} CardID={3}", callerName, indentation, entity, netEntity.CardID);
			}
			DebugPrintTags(logger, callerName, indentation, netEntity);
			break;
		}
		case Network.PowerType.TAG_CHANGE:
		{
			Network.HistTagChange tagChange = (Network.HistTagChange)power;
			logger.Print("{0}.DebugPrintPower() - {1}TAG_CHANGE Entity={2} {3} {4}", callerName, indentation, GetEntityLogName(tagChange.Entity), Tags.DebugTag(tagChange.Tag, tagChange.Value), tagChange.ChangeDef ? "DEF CHANGE" : "");
			break;
		}
		case Network.PowerType.CREATE_GAME:
		{
			Network.HistCreateGame createGame = (Network.HistCreateGame)power;
			logger.Print("{0}.DebugPrintPower() - {1}CREATE_GAME", callerName, indentation);
			indentation += "    ";
			logger.Print("{0}.DebugPrintPower() - {1}GameEntity EntityID={2}", callerName, indentation, createGame.Game.ID);
			DebugPrintTags(logger, callerName, indentation, createGame.Game);
			foreach (Network.HistCreateGame.PlayerData netPlayer in createGame.Players)
			{
				logger.Print("{0}.DebugPrintPower() - {1}Player EntityID={2} PlayerID={3} GameAccountId={4}", callerName, indentation, netPlayer.Player.ID, netPlayer.ID, netPlayer.GameAccountId);
				DebugPrintTags(logger, callerName, indentation, netPlayer.Player);
			}
			indentation = indentation.Remove(indentation.Length - "    ".Length);
			break;
		}
		case Network.PowerType.SHOW_ENTITY:
		{
			Network.Entity netEntity2 = ((Network.HistShowEntity)power).Entity;
			logger.Print("{0}.DebugPrintPower() - {1}SHOW_ENTITY - Updating Entity={2} CardID={3}", callerName, indentation, GetEntityLogName(netEntity2.ID), netEntity2.CardID);
			DebugPrintTags(logger, callerName, indentation, netEntity2);
			break;
		}
		case Network.PowerType.HIDE_ENTITY:
		{
			Network.HistHideEntity hideEntity = (Network.HistHideEntity)power;
			logger.Print("{0}.DebugPrintPower() - {1}HIDE_ENTITY - Entity={2} {3}", callerName, indentation, GetEntityLogName(hideEntity.Entity), Tags.DebugTag(49, hideEntity.Zone));
			break;
		}
		case Network.PowerType.CHANGE_ENTITY:
		{
			Network.Entity netEntity3 = ((Network.HistChangeEntity)power).Entity;
			logger.Print("{0}.DebugPrintPower() - {1}CHANGE_ENTITY - Updating Entity={2} CardID={3}", callerName, indentation, GetEntityLogName(netEntity3.ID), netEntity3.CardID);
			DebugPrintTags(logger, callerName, indentation, netEntity3);
			break;
		}
		case Network.PowerType.META_DATA:
		{
			Network.HistMetaData metaData = (Network.HistMetaData)power;
			string dataObjName = metaData.Data.ToString();
			if (metaData.MetaType == HistoryMeta.Type.JOUST)
			{
				dataObjName = GetEntityLogName(metaData.Data);
			}
			logger.Print("{0}.DebugPrintPower() - {1}META_DATA - Meta={2} Data={3} InfoCount={4}", callerName, indentation, metaData.MetaType, dataObjName, metaData.Info.Count);
			if (metaData.Info.Count > 0 && logger.IsVerbose())
			{
				indentation += "    ";
				for (int j = 0; j < metaData.Info.Count; j++)
				{
					int info = metaData.Info[j];
					logger.Print(true, "{0}.DebugPrintPower() - {1}        Info[{2}] = {3}", callerName, indentation, j, GetEntityLogName(info));
				}
				indentation = indentation.Remove(indentation.Length - "    ".Length);
			}
			break;
		}
		case Network.PowerType.RESET_GAME:
			logger.Print("{0}.DebugPrintPower() - {1}RESET_GAME", callerName, indentation);
			break;
		case Network.PowerType.SUB_SPELL_START:
		{
			Network.HistSubSpellStart subSpellStart = power as Network.HistSubSpellStart;
			logger.Print("{0}.DebugPrintPower() - {1}SUB_SPELL_START - SpellPrefabGUID={2} Source={3} TargetCount={4}", callerName, indentation, subSpellStart.SpellPrefabGUID, subSpellStart.SourceEntityID, subSpellStart.TargetEntityIDS.Count);
			if (logger.IsVerbose())
			{
				if (subSpellStart.SourceEntityID != 0)
				{
					logger.Print(true, "{0}.DebugPrintPower() - {1}                  Source = {2}", callerName, indentation, GetEntityLogName(subSpellStart.SourceEntityID));
				}
				for (int i = 0; i < subSpellStart.TargetEntityIDS.Count; i++)
				{
					int target = subSpellStart.TargetEntityIDS[i];
					logger.Print(true, "{0}.DebugPrintPower() - {1}                  Targets[{2}] = {3}", callerName, indentation, i, GetEntityLogName(target));
				}
			}
			indentation += "    ";
			break;
		}
		case Network.PowerType.SUB_SPELL_END:
			if (indentation.Length >= "    ".Length)
			{
				indentation = indentation.Remove(indentation.Length - "    ".Length);
			}
			logger.Print("{0}.DebugPrintPower() - {1}SUB_SPELL_END", callerName, indentation);
			break;
		case Network.PowerType.VO_SPELL:
		{
			Network.HistVoSpell voSpell = power as Network.HistVoSpell;
			logger.Print("{0}.DebugPrintPower() - {1}VO_SPELL - BrassRingGuid={2} - VoSpellPrefabGUID={3} - Blocking={4} - AdditionalDelayInMs={5}", callerName, indentation, voSpell.SpellPrefabGUID, voSpell.BrassRingGUID, voSpell.Blocking, voSpell.AdditionalDelayMs);
			break;
		}
		case Network.PowerType.CACHED_TAG_FOR_DORMANT_CHANGE:
		{
			Network.HistCachedTagForDormantChange cachedTagForDormantChange = (Network.HistCachedTagForDormantChange)power;
			logger.Print("{0}.DebugPrintPower() - {1}CACHED_TAG_FOR_DORMANT_CHANGE Entity={2} {3}", callerName, indentation, GetEntityLogName(cachedTagForDormantChange.Entity), Tags.DebugTag(cachedTagForDormantChange.Tag, cachedTagForDormantChange.Value));
			break;
		}
		case Network.PowerType.SHUFFLE_DECK:
		{
			Network.HistShuffleDeck shuffleDeck = (Network.HistShuffleDeck)power;
			logger.Print("{0}.DebugPrintPower() - {1}SHUFFLE_DECK PlayerID={2}", callerName, indentation, shuffleDeck.PlayerID);
			break;
		}
		default:
			logger.Print("{0}.DebugPrintPower() - ERROR: unhandled PowType {1}", callerName, power.Type);
			break;
		}
	}

	private void DebugPrintTags(Logger logger, string callerName, string indentation, Network.Entity netEntity)
	{
		if (Log.Power.CanPrint() && GameScenarioAllowsPowerPrinting())
		{
			if (indentation != null)
			{
				indentation += "    ";
			}
			for (int i = 0; i < netEntity.Tags.Count; i++)
			{
				Network.Entity.Tag tag = netEntity.Tags[i];
				logger.Print("{0}.DebugPrintPower() - {1}{2}", callerName, indentation, Tags.DebugTag(tag.Name, tag.Value));
			}
		}
	}

	private void DebugPrintOptions(Logger logger)
	{
		if (!logger.CanPrint())
		{
			return;
		}
		logger.Print("GameState.DebugPrintOptions() - id={0}", m_options.ID);
		for (int i = 0; i < m_options.List.Count; i++)
		{
			Network.Options.Option option = m_options.List[i];
			Entity mainEntity = GetEntity(option.Main.ID);
			logger.Print("GameState.DebugPrintOptions() -   option {0} type={1} mainEntity={2} error={3} errorParam={4}", i, option.Type, mainEntity, option.Main.PlayErrorInfo.PlayError, option.Main.PlayErrorInfo.PlayErrorParam);
			if (option.Main.Targets != null)
			{
				for (int j = 0; j < option.Main.Targets.Count; j++)
				{
					Network.Options.Option.TargetOption targetOption = option.Main.Targets[j];
					Entity targetEntity = GetEntity(targetOption.ID);
					logger.Print("GameState.DebugPrintOptions() -     target {0} entity={1} error={2} errorParam={3}", j, targetEntity, targetOption.PlayErrorInfo.PlayError, targetOption.PlayErrorInfo.PlayErrorParam);
				}
			}
			for (int k = 0; k < option.Subs.Count; k++)
			{
				Network.Options.Option.SubOption subOption = option.Subs[k];
				Entity subEntity = GetEntity(subOption.ID);
				logger.Print("GameState.DebugPrintOptions() -     subOption {0} entity={1} error={2} errorParam={3}", k, subEntity, subOption.PlayErrorInfo.PlayError, subOption.PlayErrorInfo.PlayErrorParam);
				if (subOption.Targets != null)
				{
					for (int l = 0; l < subOption.Targets.Count; l++)
					{
						Network.Options.Option.TargetOption targetOption2 = subOption.Targets[l];
						Entity targetEntity2 = GetEntity(targetOption2.ID);
						logger.Print("GameState.DebugPrintOptions() -       target {0} entity={1} error={2} errorParam={3}", l, targetEntity2, targetOption2.PlayErrorInfo.PlayError, targetOption2.PlayErrorInfo.PlayErrorParam);
					}
				}
			}
		}
	}

	private void DebugPrintEntityChoices(Network.EntityChoices choices, PowerTaskList preChoiceTaskList)
	{
		if (Log.Power.CanPrint() && GameScenarioAllowsPowerPrinting())
		{
			Player player = GetPlayer(choices.PlayerId);
			object preChoiceTaskListObj = null;
			if (preChoiceTaskList != null)
			{
				preChoiceTaskListObj = preChoiceTaskList.GetId();
			}
			Log.Power.Print("GameState.DebugPrintEntityChoices() - id={0} Player={1} TaskList={2} ChoiceType={3} CountMin={4} CountMax={5}", choices.ID, GetEntityLogName(player.GetEntityId()), preChoiceTaskListObj, choices.ChoiceType, choices.CountMin, choices.CountMax);
			Log.Power.Print("GameState.DebugPrintEntityChoices() -   Source={0}", GetEntityLogName(choices.Source));
			for (int i = 0; i < choices.Entities.Count; i++)
			{
				Log.Power.Print("GameState.DebugPrintEntityChoices() -   Entities[{0}]={1}", i, GetEntityLogName(choices.Entities[i]));
			}
		}
	}

	private void DebugPrintEntitiesChosen(Network.EntitiesChosen chosen)
	{
		if (Log.Power.CanPrint() && GameScenarioAllowsPowerPrinting())
		{
			Player player = GetPlayer(chosen.PlayerId);
			Log.Power.Print("GameState.DebugPrintEntitiesChosen() - id={0} Player={1} EntitiesCount={2}", chosen.ID, GetEntityLogName(player.GetEntityId()), chosen.Entities.Count);
			for (int i = 0; i < chosen.Entities.Count; i++)
			{
				Log.Power.Print("GameState.DebugPrintEntitiesChosen() -   Entities[{0}]={1}", i, GetEntityLogName(chosen.Entities[i]));
			}
		}
	}

	private string GetEntityLogName(int id)
	{
		Entity entity = GetEntity(id);
		if (entity == null)
		{
			return id.ToString();
		}
		if (entity.IsPlayer())
		{
			BnetPlayer bnetPlayer = (entity as Player).GetBnetPlayer();
			if (bnetPlayer != null && bnetPlayer.GetBattleTag() != null)
			{
				return $"{bnetPlayer.GetBattleTag().GetName()}#{bnetPlayer.GetBattleTag().GetNumber()}";
			}
		}
		return entity.ToString();
	}

	private string GetEntitiesLogNames(List<int> ids)
	{
		StringBuilder sb = new StringBuilder();
		foreach (int id in ids)
		{
			if (sb.Length > 0)
			{
				sb.Append(",");
			}
			sb.Append(GetEntityLogName(id));
		}
		return sb.ToString();
	}

	private void PrintBlockingTaskList(StringBuilder builder, PowerTaskList taskList)
	{
		if (taskList == null)
		{
			builder.Append("null");
			return;
		}
		builder.AppendFormat("ID={0} ", taskList.GetId());
		builder.Append("Source=[");
		Network.HistBlockStart blockStart = taskList.GetBlockStart();
		if (blockStart == null)
		{
			builder.Append("null");
		}
		else
		{
			builder.AppendFormat("BlockType={0}", blockStart.BlockType);
			builder.Append(' ');
			builder.AppendFormat("Entities={0}", GetEntitiesLogNames(blockStart.Entities));
			builder.Append(' ');
			builder.AppendFormat("Target={0}", GetEntityLogName(blockStart.Target));
		}
		builder.Append(']');
		builder.AppendFormat(" Tasks={0}", taskList.GetTaskList().Count);
	}

	private void QuickGameFlipHeroesCheat(List<Network.PowerHistory> powerList)
	{
	}

	public void UpdateCornerReplacements()
	{
		if (m_cornerSpellReplacementManager != null)
		{
			m_cornerSpellReplacementManager.UpdateCornerReplacements();
		}
	}

	public void ForceUpdateAllSubcards()
	{
		foreach (Entity entity in m_entityMap.Values)
		{
			if (entity == null)
			{
				continue;
			}
			int parentId = entity.GetTag(GAME_TAG.PARENT_CARD);
			if (parentId != 0)
			{
				Entity parent = GetEntity(parentId);
				if (parent != null && parent.AddSubCard(entity))
				{
					Log.Gameplay.PrintError("Adding a missing subcard entity={0} parent={1} ", entity.ToString(), parent.ToString());
				}
			}
		}
	}
}
