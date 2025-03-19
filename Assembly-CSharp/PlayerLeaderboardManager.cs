using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Blizzard.T5.Core;
using PegasusGame;
using UnityEngine;

public class PlayerLeaderboardManager : CardTileListDisplay
{
	public enum PlayerTileEvent
	{
		TRIPLE,
		WIN_STREAK,
		TECH_LEVEL,
		RECENT_COMBAT,
		KNOCK_OUT,
		RACES,
		BANANA,
		HERO_BUDDY,
		DOUBLE_HERO_BUDDY,
		QUEST_COMPLETE,
		QUEST_UPDATE,
		TRINKET_UPDATE,
		TRINKET_TAG_UPDATE
	}

	private readonly PlatformDependentValue<float> SPACE_BETWEEN_TILES = new PlatformDependentValue<float>(PlatformCategory.Screen)
	{
		PC = 0.15f,
		Phone = 0.1f
	};

	private readonly PlatformDependentValue<float> SPACE_BETWEEN_TILES_DUOS = new PlatformDependentValue<float>(PlatformCategory.Screen)
	{
		PC = 0.2f,
		Phone = 0.1f
	};

	private readonly PlatformDependentValue<float> LEADERBOARD_TILE_X_CORRECTION_FOR_PERSPECTIVE_MAX = new PlatformDependentValue<float>(PlatformCategory.Screen)
	{
		PC = -0.08f,
		Phone = -0.08f
	};

	private readonly PlatformDependentVector3 LEADERBOARD_TILE_SCALE = new PlatformDependentVector3(PlatformCategory.Screen)
	{
		PC = new Vector3(1f, 1f, 1f),
		Phone = new Vector3(1f, 1f, 1f)
	};

	private readonly PlatformDependentVector3 LEADERBOARD_TILE_CARD_SCALE = new PlatformDependentVector3(PlatformCategory.Screen)
	{
		PC = new Vector3(1.75f, 1.75f, 1.75f),
		Phone = new Vector3(1.3125f, 1.3125f, 1.3125f)
	};

	private readonly PlatformDependentVector3 LEADERBOARD_TILE_SCALE_DUOS = new PlatformDependentVector3(PlatformCategory.Screen)
	{
		PC = new Vector3(1f, 1f, 1f),
		Phone = new Vector3(0.75f, 0.75f, 0.75f)
	};

	private readonly PlatformDependentVector3 LEADERBOARD_TILE_CARD_SCALE_DUOS = new PlatformDependentVector3(PlatformCategory.Screen)
	{
		PC = new Vector3(1f, 1f, 1f),
		Phone = new Vector3(1f, 1f, 1f)
	};

	public readonly PlatformDependentValue<float> HIGHEST_MAIN_ACTOR_Z_WORLD = new PlatformDependentValue<float>(PlatformCategory.Screen)
	{
		PC = -1.5f,
		Phone = -2.5f,
		Tablet = -0.5f
	};

	public readonly PlatformDependentValue<float> LOWEST_MAIN_ACTOR_Z_WORLD = new PlatformDependentValue<float>(PlatformCategory.Screen)
	{
		PC = -3.4f,
		Phone = -2.5f,
		Tablet = -2.4f
	};

	private readonly string SOLO_TEAM_LEADERBOARD_CONTAINER_PREFAB = "PlayerLeaderboardTeamSolo.prefab:a09e9bf137c83104e8564db2abf1d38b";

	private readonly string DUOS_TEAM_LEADERBOARD_CONTAINER_PREFAB = "PlayerLeaderboardTeamDuo.prefab:69107c598787af14ea63ac8a7f316a4b";

	private static PlayerLeaderboardManager s_instance;

	private bool m_disabled;

	private List<PlayerLeaderboardTeam> m_teams = new List<PlayerLeaderboardTeam>();

	private PlayerLeaderboardCard m_currentlyMousedOverTile;

	private bool m_isMousedOver;

	private bool m_isNewMouseOver = true;

	private List<int> m_addedTileForPlayerId = new List<int>();

	private List<int> m_addedContainerForTeamId = new List<int>();

	private Map<int, List<PlayerLeaderboardRecentCombatsPanel.RecentCombatInfo>> m_combatHistory = new Map<int, List<PlayerLeaderboardRecentCombatsPanel.RecentCombatInfo>>();

	private Map<int, List<PlayerLeaderboardRecentCombatsPanel.RecentCombatInfo>> m_incomingHistory = new Map<int, List<PlayerLeaderboardRecentCombatsPanel.RecentCombatInfo>>();

	private Map<int, PlayerRealTimeBattlefieldRaces> m_pendingRaceCountUpdates = new Map<int, PlayerRealTimeBattlefieldRaces>();

	private Entity m_oddManOutOpponentHero;

	private const int NULL_PLAYER = 0;

	private bool m_allowFakePlayerTiles;

	private bool m_isLeaderboardDamageCapFXActive = true;

	private bool m_wasLeaderboardDamageCapFXActive;

	private Dictionary<int, int> m_currentWinStreak = new Dictionary<int, int>();

	private Dictionary<int, int> m_currentLoseStreak = new Dictionary<int, int>();

	private static HistoryTileInitInfo CreateHistoryTileInitInfo(Entity entity)
	{
		HistoryTileInitInfo initInfo = new HistoryTileInitInfo();
		initInfo.m_entity = entity;
		initInfo.m_cardDef = entity.ShareDisposableCardDef();
		using (initInfo.m_cardDef)
		{
			if (initInfo.m_cardDef?.CardDef != null)
			{
				TAG_PREMIUM premium = entity.GetPremiumType();
				initInfo.m_portraitTexture = initInfo.m_cardDef.CardDef.GetPortraitTexture(premium);
				initInfo.m_portraitGoldenMaterial = initInfo.m_cardDef.CardDef.GetPremiumPortraitMaterial();
				if (initInfo.m_cardDef.CardDef.GetLeaderboardTileFullPortrait() != null)
				{
					initInfo.m_fullTileMaterial = initInfo.m_cardDef.CardDef.GetLeaderboardTileFullPortrait();
				}
				else
				{
					initInfo.m_cardDef.CardDef.TryGetHistoryTileFullPortrait(premium, out initInfo.m_fullTileMaterial);
				}
				initInfo.m_cardDef.CardDef.TryGetHistoryTileHalfPortrait(premium, out initInfo.m_halfTileMaterial);
			}
			return initInfo;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		s_instance = this;
		base.transform.position = new Vector3(base.transform.position.x, base.transform.position.y + 0.15f, base.transform.position.z);
		SetEnabled(enabled: false);
		GameState gameState = GameState.Get();
		if (gameState == null)
		{
			Debug.LogWarning("PlayerLeaderboardManager.Awake() - GameState was not Initialized. Initializing now...");
			gameState = GameState.Initialize();
		}
		gameState.RegisterTurnChangedListener(OnTurnChanged);
		gameState.RegisterCreateGameListener(OnCreateGame);
		gameState.RegisterDamageCapChangedListener(OnDamageCapChanged);
		gameState.RegisterDiabloFightPlayerIDChangedListener(OnDiabloFightPlayerIDChanged);
	}

	protected override void OnDestroy()
	{
		s_instance = null;
		if (GameState.Get() != null)
		{
			GameState.Get().UnregisterTurnChangedListener(OnTurnChanged);
			GameState.Get().UnregisterCreateGameListener(OnCreateGame);
			GameState.Get().UnregisterDamageCapChangedListener(OnDamageCapChanged);
			GameState.Get().UnregisterDamageCapChangedListener(OnDamageCapChanged);
		}
		base.OnDestroy();
	}

	public static PlayerLeaderboardManager Get()
	{
		return s_instance;
	}

	public void SetEnabled(bool enabled)
	{
		m_disabled = !enabled;
		GetComponent<Collider>().enabled = enabled;
	}

	public bool IsEnabled()
	{
		return !m_disabled;
	}

	public void SetAllowFakePlayers(bool enabled)
	{
		m_allowFakePlayerTiles = enabled;
	}

	public void CreatePlayerTile(Entity playerHero)
	{
		if (m_disabled)
		{
			return;
		}
		int playerHeroId = playerHero.GetTag(GAME_TAG.PLAYER_ID);
		if (playerHeroId == 0)
		{
			playerHeroId = playerHero.GetTag(GAME_TAG.CONTROLLER);
		}
		if (!GameState.Get().GetPlayerInfoMap().ContainsKey(playerHeroId))
		{
			if (!m_allowFakePlayerTiles)
			{
				Log.Gameplay.PrintError($"PlayerLeaderboardManager.CreatePlayerTile() - Attempt to add player id {playerHeroId} to leaderboard, but that is not a valid id.");
				return;
			}
			SharedPlayerInfo playerInfo = new SharedPlayerInfo();
			playerInfo.SetPlayerId(playerHeroId);
			GameState.Get().AddPlayerInfo(playerInfo);
		}
		if (m_addedTileForPlayerId.Any((int t) => t == playerHeroId))
		{
			return;
		}
		m_addedTileForPlayerId.Add(playerHeroId);
		int playerTeamId = 0;
		string teamPrefab = "";
		if (GameMgr.Get().IsBattlegroundDuoGame())
		{
			playerTeamId = playerHero.GetTag(GAME_TAG.BACON_DUO_TEAM_ID);
			if (playerTeamId == 0)
			{
				playerTeamId = playerHero.GetController().GetTag(GAME_TAG.BACON_DUO_TEAM_ID);
			}
			teamPrefab = DUOS_TEAM_LEADERBOARD_CONTAINER_PREFAB;
		}
		else
		{
			playerTeamId = playerHero.GetTag(GAME_TAG.PLAYER_ID);
			teamPrefab = SOLO_TEAM_LEADERBOARD_CONTAINER_PREFAB;
		}
		if (!m_addedContainerForTeamId.Any((int t) => t == playerTeamId))
		{
			m_addedContainerForTeamId.Add(playerTeamId);
			AssetLoader.Get().InstantiatePrefab(teamPrefab, TeamContainerLoadedCallback, playerHero, AssetLoadingOptions.IgnorePrefabPosition);
		}
		else
		{
			StartCoroutine(AddPlayerToTeamWhenTeamIsReady(playerTeamId, playerHero));
		}
	}

	private IEnumerator AddPlayerToTeamWhenTeamIsReady(int teamId, Entity playerHero)
	{
		if (playerHero != null)
		{
			while (playerHero.GetLoadState() == Entity.LoadState.LOADING)
			{
				yield return null;
			}
			PlayerLeaderboardTeam team = GetTeamForTeamId(teamId);
			while (team == null)
			{
				yield return null;
				team = GetTeamForTeamId(teamId);
			}
			HistoryTileInitInfo initInfo = CreateHistoryTileInitInfo(playerHero);
			team.AddMember(initInfo);
		}
	}

	public void UpdatePlayerTileHeroPower(Entity hero, int newHeroPowerId)
	{
		int playerId = hero.GetTag(GAME_TAG.PLAYER_ID);
		PlayerLeaderboardCard tile = GetTileForPlayerId(playerId);
		if (tile != null)
		{
			tile.SetHeroPower(hero);
		}
	}

	public void SetPlayerTileAdditionalHeroPowerDirty(Entity hero)
	{
		int playerId = hero.GetTag(GAME_TAG.PLAYER_ID);
		PlayerLeaderboardCard tile = GetTileForPlayerId(playerId);
		if (tile != null)
		{
			tile.SetAdditionalHeroPowersDirty();
		}
	}

	public List<PlayerLeaderboardCard> GetAllTiles()
	{
		List<PlayerLeaderboardCard> allTiles = new List<PlayerLeaderboardCard>();
		foreach (PlayerLeaderboardTeam team in m_teams)
		{
			foreach (PlayerLeaderboardCard tile in team.Members)
			{
				allTiles.Add(tile);
			}
		}
		return allTiles;
	}

	public void NotifyBattlegroundHeroBuddyEnabledDirty()
	{
		foreach (PlayerLeaderboardCard allTile in GetAllTiles())
		{
			allTile.SetBattlegroundHeroBuddyEnabledDirty();
		}
	}

	public void NotifyBattlegroundsQuestRewardEnabledDirty()
	{
		foreach (PlayerLeaderboardCard allTile in GetAllTiles())
		{
			allTile.SetBGQuestRewardDirty();
		}
	}

	public void NotifyBattlegroundsTrinketEnabledDirty()
	{
		foreach (PlayerLeaderboardCard allTile in GetAllTiles())
		{
			allTile.SetBGTrinketDirty();
		}
	}

	public void NotifyPlayerTileEvent(int playerId, PlayerTileEvent tileEvent)
	{
		if (!m_addedTileForPlayerId.Contains(playerId))
		{
			return;
		}
		EmoteType desiredEmoteType = EmoteType.INVALID;
		PlayerLeaderboardCard playerTile = GetTileForPlayerId(playerId);
		switch (tileEvent)
		{
		default:
			return;
		case PlayerTileEvent.TECH_LEVEL:
		{
			int desiredTechLevel = 1;
			if (GameState.Get().GetPlayerInfoMap().ContainsKey(playerId) && GameState.Get().GetPlayerInfoMap()[playerId].GetPlayerHero() != null)
			{
				desiredTechLevel = GameState.Get().GetPlayerInfoMap()[playerId].GetPlayerHero().GetRealTimePlayerTechLevel();
			}
			desiredTechLevel = Mathf.Clamp(desiredTechLevel, 1, 7);
			if (playerTile != null)
			{
				playerTile.SetTechLevelDirty();
			}
			switch (desiredTechLevel)
			{
			default:
				return;
			case 2:
				desiredEmoteType = EmoteType.BATTLEGROUNDS_VISUAL_TECH_UP_02;
				break;
			case 3:
				desiredEmoteType = EmoteType.BATTLEGROUNDS_VISUAL_TECH_UP_03;
				break;
			case 4:
				desiredEmoteType = EmoteType.BATTLEGROUNDS_VISUAL_TECH_UP_04;
				break;
			case 5:
				desiredEmoteType = EmoteType.BATTLEGROUNDS_VISUAL_TECH_UP_05;
				break;
			case 6:
				desiredEmoteType = EmoteType.BATTLEGROUNDS_VISUAL_TECH_UP_06;
				break;
			case 7:
				desiredEmoteType = EmoteType.BATTLEGROUNDS_VISUAL_TECH_UP_07;
				break;
			}
			break;
		}
		case PlayerTileEvent.TRIPLE:
			if (playerTile != null)
			{
				playerTile.SetTriplesDirty();
			}
			desiredEmoteType = EmoteType.BATTLEGROUNDS_VISUAL_TRIPLE;
			break;
		case PlayerTileEvent.WIN_STREAK:
			desiredEmoteType = EmoteType.BATTLEGROUNDS_VISUAL_HOT_STREAK;
			break;
		case PlayerTileEvent.BANANA:
			desiredEmoteType = EmoteType.BATTLEGROUNDS_VISUAL_BANANA;
			break;
		case PlayerTileEvent.QUEST_COMPLETE:
			desiredEmoteType = EmoteType.BATTLEGROUNDS_VISUAL_QUEST_COMPLETE;
			break;
		case PlayerTileEvent.HERO_BUDDY:
			desiredEmoteType = EmoteType.BATTLEGROUNDS_VISUAL_HERO_BUDDY;
			break;
		case PlayerTileEvent.DOUBLE_HERO_BUDDY:
			desiredEmoteType = EmoteType.BATTLEGROUNDS_VISUAL_DOUBLE_HERO_BUDDY;
			break;
		case PlayerTileEvent.KNOCK_OUT:
			return;
		case PlayerTileEvent.RECENT_COMBAT:
			if (playerTile != null)
			{
				playerTile.SetRecentCombatsDirty();
			}
			return;
		case PlayerTileEvent.RACES:
			if (playerTile != null)
			{
				playerTile.SetRacesDirty();
			}
			return;
		case PlayerTileEvent.QUEST_UPDATE:
			if (playerTile != null)
			{
				playerTile.SetBGQuestRewardDirty();
			}
			return;
		case PlayerTileEvent.TRINKET_UPDATE:
			if (playerTile != null)
			{
				playerTile.SetBGTrinketDirty();
			}
			return;
		case PlayerTileEvent.TRINKET_TAG_UPDATE:
			if (playerTile != null)
			{
				playerTile.UpdateTrinketTags();
			}
			return;
		}
		GameState.Get().GetGameEntity().PlayAlternateEnemyEmote(playerId, desiredEmoteType);
	}

	public void NotifyOfInput(Vector3 hitPoint)
	{
		m_isMousedOver = true;
		if (m_teams.Count == 0)
		{
			CheckForMouseOff();
			return;
		}
		float lowestBottom = 1000f;
		float highestTop = -1000f;
		float closestTileDistance = 1000f;
		PlayerLeaderboardCard closestTile = null;
		foreach (PlayerLeaderboardCard card in GetAllTiles())
		{
			if (!card.HasBeenShown())
			{
				continue;
			}
			Collider tileCollider = card.GetTileCollider();
			if (!(tileCollider == null))
			{
				float bottom = tileCollider.bounds.center.z - tileCollider.bounds.extents.z;
				float top = tileCollider.bounds.center.z + tileCollider.bounds.extents.z;
				if (bottom < lowestBottom)
				{
					lowestBottom = bottom;
				}
				if (top > highestTop)
				{
					highestTop = top;
				}
				if (bottom < hitPoint.z && top > hitPoint.z)
				{
					closestTile = card;
					break;
				}
				float distanceToBottom = Mathf.Abs(hitPoint.z - bottom);
				if (distanceToBottom < closestTileDistance)
				{
					closestTileDistance = distanceToBottom;
					closestTile = card;
				}
				float distanceToTop = Mathf.Abs(hitPoint.z - top);
				if (distanceToTop < closestTileDistance)
				{
					closestTileDistance = distanceToTop;
					closestTile = card;
				}
			}
		}
		if (hitPoint.z < lowestBottom || hitPoint.z > highestTop)
		{
			CheckForMouseOff();
			return;
		}
		if (closestTile == null)
		{
			CheckForMouseOff();
			return;
		}
		Collider rootCollider = base.gameObject.GetComponent<BoxCollider>();
		Collider closestTileCollider = closestTile.GetTileCollider();
		float offsetX = 0f;
		if (closestTile.GetNextOpponentState())
		{
			offsetX = closestTile.GetPoppedOutBoneX() * closestTile.m_tileActor.transform.localScale.x;
		}
		if (hitPoint.x < rootCollider.bounds.center.x - closestTileCollider.bounds.extents.x || hitPoint.x > rootCollider.bounds.center.x + closestTileCollider.bounds.extents.x + offsetX)
		{
			CheckForMouseOff();
		}
		else if (!(closestTile == m_currentlyMousedOverTile))
		{
			if (m_currentlyMousedOverTile != null)
			{
				m_currentlyMousedOverTile.NotifyMousedOut();
			}
			else
			{
				FadeVignetteIn();
			}
			m_currentlyMousedOverTile = closestTile;
			closestTile.NotifyMousedOver();
			m_isNewMouseOver = false;
		}
	}

	public void NotifyOfMouseOff()
	{
		CheckForMouseOff();
	}

	public void SetNextOpponent(int opponentPlayerId)
	{
		if (opponentPlayerId == 0)
		{
			return;
		}
		foreach (PlayerLeaderboardTeam team in m_teams)
		{
			team.SetNextOpponentState(team.IsPlayerOnTeam(opponentPlayerId), opponentPlayerId);
		}
	}

	public void SetCurrentOpponent(int opponentPlayerId)
	{
		foreach (PlayerLeaderboardTeam team in m_teams)
		{
			bool isNextOpponent = team.IsPlayerOnTeam(opponentPlayerId);
			team.SetSwordsIconActive(isNextOpponent, opponentPlayerId);
			if (!isNextOpponent)
			{
				continue;
			}
			foreach (PlayerLeaderboardCard card in team.Members)
			{
				if (GameState.Get().GetPlayerInfoMap().TryGetValue(card.Entity.GetTag(GAME_TAG.PLAYER_ID), out var opponentInfo) && opponentInfo != null)
				{
					BnetRecentPlayerMgr.Get().AddRecentPlayer(opponentInfo.GetBnetPlayer(), BnetRecentPlayerMgr.RecentReason.CURRENT_OPPONENT);
				}
			}
		}
	}

	private void HideAllSwordsDisplay()
	{
		foreach (PlayerLeaderboardTeam team in m_teams)
		{
			team.SetSwordsIconActive(active: false, -1);
		}
		foreach (PlayerLeaderboardTeam team2 in m_teams)
		{
			team2.DeactivateDizzyEffects();
		}
	}

	public void UpdateSwordsDisplay()
	{
		if (!GameState.Get().GetGameEntity().IsInBattlegroundsCombatPhase())
		{
			HideAllSwordsDisplay();
			return;
		}
		int friendlyCombatPlayerId = GameState.Get().GetFriendlySidePlayer().GetTag(GAME_TAG.BACON_CURRENT_COMBAT_PLAYER_ID);
		int opposingCombatPlayerId = GameState.Get().GetOpposingSidePlayer().GetTag(GAME_TAG.BACON_CURRENT_COMBAT_PLAYER_ID);
		foreach (PlayerLeaderboardTeam team in m_teams)
		{
			bool isNextOpponent = team.IsPlayerOnTeam(opposingCombatPlayerId);
			bool isSelf = team.IsPlayerOnTeam(friendlyCombatPlayerId);
			if (isNextOpponent)
			{
				team.SetSwordsIconActive(isNextOpponent, opposingCombatPlayerId);
			}
			else if (isSelf)
			{
				team.SetSwordsIconActive(isSelf, friendlyCombatPlayerId);
			}
			else
			{
				team.SetSwordsIconActive(active: false, -1);
			}
		}
		int nextOpponentPlayerId = GameState.Get().GetFriendlySidePlayer().GetTag(GAME_TAG.NEXT_OPPONENT_PLAYER_ID);
		int friendlyPlayerId = GameState.Get().GetFriendlySidePlayer().GetPlayerId();
		bool friendlyPlayerGoingFirst = GameState.Get().GetFriendlySidePlayer().HasTag(GAME_TAG.BACON_DUO_PLAYER_FIGHTS_FIRST_NEXT_COMBAT);
		bool opponentDizzy = opposingCombatPlayerId != nextOpponentPlayerId;
		bool friendlyDizzy = (friendlyCombatPlayerId != friendlyPlayerId && friendlyPlayerGoingFirst) || (friendlyCombatPlayerId == friendlyPlayerId && !friendlyPlayerGoingFirst);
		foreach (PlayerLeaderboardTeam team2 in m_teams)
		{
			bool isOpponentTeam = team2.IsPlayerOnTeam(opposingCombatPlayerId);
			bool isFriendlyTeam = team2.IsPlayerOnTeam(friendlyCombatPlayerId);
			if (opponentDizzy && isOpponentTeam)
			{
				team2.ActivateDizzyEffectOnPlayerTeammate(opposingCombatPlayerId);
			}
			else if (friendlyDizzy && isFriendlyTeam)
			{
				team2.ActivateDizzyEffectOnPlayerTeammate(friendlyCombatPlayerId);
			}
			else
			{
				team2.DeactivateDizzyEffects();
			}
		}
	}

	public void ApplyEntityReplacement(int playerID, Entity replacementEntity)
	{
		foreach (PlayerLeaderboardCard playerCard in GetAllTiles())
		{
			if (playerCard.Entity.GetTag(GAME_TAG.PLAYER_ID) == playerID)
			{
				HistoryTileInitInfo initInfo = CreateHistoryTileInitInfo(replacementEntity);
				playerCard.ReplaceHero(replacementEntity, initInfo);
			}
			playerCard.RefreshRecentCombats();
		}
		if (m_oddManOutOpponentHero != null && m_oddManOutOpponentHero.GetTag(GAME_TAG.PLAYER_ID) == playerID)
		{
			m_oddManOutOpponentHero = replacementEntity;
		}
	}

	private void OnTurnChanged(int oldTurn, int newTurn, object userdata)
	{
		int nextOpponentId = GameState.Get().GetFriendlySidePlayer().GetTag(GAME_TAG.NEXT_OPPONENT_PLAYER_ID);
		if (GameState.Get().GetCurrentPlayer().IsFriendlySide())
		{
			SetNextOpponent(nextOpponentId);
			SetCurrentOpponent(-1);
			ApplyIncomingCombatHistory();
			return;
		}
		SetCurrentOpponent(nextOpponentId);
		foreach (PlayerLeaderboardCard allTile in GetAllTiles())
		{
			allTile.PauseHealthUpdates();
		}
	}

	private void OnDamageCapChanged(int oldValue, int newValue, object userdata)
	{
		StartCoroutine(UpdateDamageCapFX(oldValue, newValue));
	}

	private void OnDiabloFightPlayerIDChanged(int oldValue, int newValue, object userdata)
	{
		foreach (PlayerLeaderboardCard allTile in GetAllTiles())
		{
			allTile.UpdateDiabloPlayerFightFX(oldValue, newValue);
		}
	}

	public void EnableDamageCapFX(bool enable)
	{
		m_isLeaderboardDamageCapFXActive = enable;
		StartCoroutine(UpdateDamageCapFX(-1, -1, forceUpdate: true));
	}

	public void UpdateDamageCap()
	{
		StartCoroutine(UpdateDamageCapFX());
	}

	public IEnumerator UpdateDamageCapFX(int oldValue = -1, int newValue = -1, bool forceUpdate = false)
	{
		GameEntity gameEntity = GameState.Get().GetGameEntity();
		if (GameState.Get() == null || gameEntity == null)
		{
			Debug.Log("[PlayerLeaderboardManager::UpdateDamageCapFX] - Game State/Game Entity is null");
			yield return null;
		}
		bool enabled = GameState.Get().GetGameEntity().HasTag(GAME_TAG.BACON_COMBAT_DAMAGE_CAP_ENABLED);
		Spell damageCapSpell = Board.Get().m_leaderboardDamageCapFX;
		if (!m_isLeaderboardDamageCapFXActive)
		{
			if (damageCapSpell != null)
			{
				m_wasLeaderboardDamageCapFXActive = false;
				while (gameEntity.IsInBattlegroundsCombatPhase() || gameEntity.IsStateChangePopupVisible())
				{
					yield return new WaitForSeconds(0.1f);
				}
				SpellUtils.ActivateDeathIfNecessary(damageCapSpell);
			}
			yield return null;
		}
		if (newValue == -1)
		{
			newValue = GameState.Get().GetGameEntity().GetTag(GAME_TAG.BACON_COMBAT_DAMAGE_CAP);
		}
		if (!forceUpdate && oldValue == newValue && m_wasLeaderboardDamageCapFXActive == enabled)
		{
			yield return null;
		}
		if (!(damageCapSpell != null))
		{
			yield break;
		}
		while (gameEntity.IsInBattlegroundsCombatPhase() || gameEntity.IsStateChangePopupVisible())
		{
			yield return new WaitForSeconds(0.1f);
		}
		if (enabled)
		{
			damageCapSpell.gameObject.SetActive(value: true);
			m_wasLeaderboardDamageCapFXActive = true;
			if (!SpellUtils.ActivateBirthIfNecessary(damageCapSpell) && oldValue != newValue && oldValue != -1)
			{
				SpellUtils.ActivateStateIfNecessary(damageCapSpell, SpellStateType.ACTION);
			}
		}
		else
		{
			m_wasLeaderboardDamageCapFXActive = false;
			SpellUtils.ActivateDeathIfNecessary(damageCapSpell);
		}
	}

	private void OnCreateGame(GameState.CreateGamePhase phase, object userData)
	{
		UpdateLayout(animate: false);
		ApplyIncomingCombatHistory(suppressNotifications: true);
		StartCoroutine(UpdateDamageCapFX());
	}

	public bool IsMousedOver()
	{
		return m_isMousedOver;
	}

	public bool IsNewlyMousedOver()
	{
		if (m_isMousedOver)
		{
			return m_isNewMouseOver;
		}
		return false;
	}

	private void CheckForMouseOff()
	{
		if (!m_isMousedOver)
		{
			return;
		}
		m_isMousedOver = false;
		m_isNewMouseOver = true;
		foreach (PlayerLeaderboardCard allTile in GetAllTiles())
		{
			allTile.NotifyMousedOut();
		}
		if (m_currentlyMousedOverTile != null)
		{
			FadeVignetteOut();
		}
		m_currentlyMousedOverTile = null;
	}

	private void FadeVignetteIn()
	{
		foreach (PlayerLeaderboardCard item in GetAllTiles())
		{
			if (!(item.m_parent == null))
			{
				LayerUtils.SetLayer(item.m_parent.gameObject, GameLayer.Tooltip);
			}
		}
		LayerUtils.SetLayer(base.gameObject, GameLayer.IgnoreFullScreenEffects);
		AnimateBlurVignetteIn();
	}

	private void FadeVignetteOut()
	{
		foreach (PlayerLeaderboardCard item in GetAllTiles())
		{
			if (!(item.m_parent == null))
			{
				LayerUtils.SetLayer(item.m_parent.gameObject, GameLayer.Default);
			}
		}
		LayerUtils.SetLayer(base.gameObject, GameLayer.CardRaycast);
		AnimateBlurVignetteOut();
	}

	protected override void OnFullScreenEffectOutFinished()
	{
		foreach (PlayerLeaderboardCard card in GetAllTiles())
		{
			if (!(card.m_tileActor == null))
			{
				LayerUtils.SetLayer(card.m_tileActor.gameObject, GameLayer.Default);
			}
		}
	}

	private void TeamContainerLoadedCallback(AssetReference assetReference, GameObject go, object callbackData)
	{
		Entity entity = (Entity)callbackData;
		PlayerLeaderboardTeam team = go.GetComponent<PlayerLeaderboardTeam>();
		go.transform.localScale = (GameMgr.Get().IsBattlegroundDuoGame() ? LEADERBOARD_TILE_SCALE_DUOS : LEADERBOARD_TILE_SCALE);
		PlayerLeaderboardCard[] componentsInChildren = go.GetComponentsInChildren<PlayerLeaderboardCard>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].gameObject.transform.localScale = (GameMgr.Get().IsBattlegroundDuoGame() ? LEADERBOARD_TILE_CARD_SCALE_DUOS : LEADERBOARD_TILE_CARD_SCALE);
		}
		if (GameMgr.Get().IsBattlegroundDuoGame())
		{
			team.TeamId = entity.GetTag(GAME_TAG.BACON_DUO_TEAM_ID);
			if (team.TeamId == 0)
			{
				team.TeamId = entity.GetController().GetTag(GAME_TAG.BACON_DUO_TEAM_ID);
			}
		}
		else
		{
			team.TeamId = entity.GetTag(GAME_TAG.PLAYER_ID);
		}
		if (GameState.Get().IsMulliganManagerActive())
		{
			team.SetRevealed(revealed: false, isNextOpponent: false);
		}
		m_teams.Add(team);
		StartCoroutine(AddPlayerToTeamWhenTeamIsReady(team.TeamId, entity));
	}

	public PlayerLeaderboardTeam GetTeamForTeamId(int teamId)
	{
		foreach (PlayerLeaderboardTeam team in m_teams)
		{
			if (team.TeamId == teamId)
			{
				return team;
			}
		}
		return null;
	}

	public PlayerLeaderboardCard GetTileForPlayerId(int playerId)
	{
		foreach (PlayerLeaderboardTeam team in m_teams)
		{
			foreach (PlayerLeaderboardCard tile in team.Members)
			{
				if (tile != null && tile.Entity != null && tile.Entity.GetTag(GAME_TAG.PLAYER_ID) == playerId)
				{
					return tile;
				}
			}
		}
		return null;
	}

	public Vector3 GetTopTilePosition()
	{
		float x_offset = 0.05f;
		if (m_teams != null && m_teams.Count > 0 && m_teams[0] is PlayerLeaderboardDuosTeam)
		{
			x_offset = 0.05f;
		}
		return new Vector3(base.transform.position.x + x_offset, base.transform.position.y - 0.15f, base.transform.position.z);
	}

	public void UpdatePlayerCombatOrder(bool animate = true)
	{
		foreach (PlayerLeaderboardTeam team in m_teams)
		{
			team.UpdatePlayerOrder(animate);
		}
	}

	public void UpdateLayout(bool animate = true)
	{
		SortPlayers();
		UpdateHealthTotals();
		HideAllSwordsDisplay();
		AnimateTeamsToPositions(animate);
		UpdatePlayerCombatOrder(animate);
	}

	public void AnimateTeamsToPositions(bool animate)
	{
		float spaceBetweenTiles = (GameMgr.Get().IsBattlegroundDuoGame() ? SPACE_BETWEEN_TILES_DUOS : SPACE_BETWEEN_TILES);
		float zOffset = 0f;
		float x_correction = 0f;
		Vector3 topPosition = GetTopTilePosition();
		for (int i = 0; i < m_teams.Count; i++)
		{
			PlayerLeaderboardTeam team = m_teams[i];
			Collider tileCollider = team.GetTileCollider();
			Vector3 newPosition = new Vector3(topPosition.x + x_correction, topPosition.y, topPosition.z - zOffset);
			if (animate)
			{
				iTween.MoveTo(team.gameObject, newPosition, 1f);
			}
			else
			{
				team.gameObject.transform.position = newPosition;
			}
			int nextOpponentPlayerId = GameState.Get().GetFriendlySidePlayer().GetTag(GAME_TAG.NEXT_OPPONENT_PLAYER_ID);
			bool isNextOpponent = team.IsPlayerOnTeam(nextOpponentPlayerId);
			if (!team.HasBeenShown() && GameState.Get().IsMulliganManagerActive())
			{
				team.SetRevealed(revealed: true, isNextOpponent);
			}
			team.MarkAsShown();
			team.UpdateOddPlayerOutFx(isNextOpponent, nextOpponentPlayerId);
			if (tileCollider != null)
			{
				zOffset += tileCollider.bounds.size.z + spaceBetweenTiles;
			}
			x_correction += (float)LEADERBOARD_TILE_X_CORRECTION_FOR_PERSPECTIVE_MAX / (float)m_teams.Count;
		}
	}

	public void UpdateRoundHistory(GameRoundHistory gameRoundHistory)
	{
		m_incomingHistory.Clear();
		for (int i = 0; i < gameRoundHistory.Rounds.Count; i++)
		{
			GameRoundHistoryEntry gameRound = gameRoundHistory.Rounds[i];
			AddCombatRound(gameRound);
		}
	}

	public void UpdatePlayerRaces(PlayerRealTimeBattlefieldRaces realTimeBattlefieldRaces)
	{
		PlayerLeaderboardCard playerTile = GetTileForPlayerId(realTimeBattlefieldRaces.PlayerId);
		if (playerTile != null)
		{
			playerTile.UpdateRacesCount(realTimeBattlefieldRaces.Races);
		}
		else if (!m_pendingRaceCountUpdates.ContainsKey(realTimeBattlefieldRaces.PlayerId))
		{
			m_pendingRaceCountUpdates.Add(realTimeBattlefieldRaces.PlayerId, realTimeBattlefieldRaces);
		}
		else
		{
			m_pendingRaceCountUpdates[realTimeBattlefieldRaces.PlayerId] = realTimeBattlefieldRaces;
		}
	}

	private void ApplyIncomingCombatHistory(bool suppressNotifications = false)
	{
		m_combatHistory.Clear();
		m_combatHistory = new Map<int, List<PlayerLeaderboardRecentCombatsPanel.RecentCombatInfo>>(m_incomingHistory);
		int roundCount = 0;
		foreach (KeyValuePair<int, List<PlayerLeaderboardRecentCombatsPanel.RecentCombatInfo>> entry in m_combatHistory)
		{
			if (entry.Value != null && entry.Value.Count != 0)
			{
				roundCount = Math.Max(roundCount, entry.Value.Count);
			}
		}
		foreach (KeyValuePair<int, List<PlayerLeaderboardRecentCombatsPanel.RecentCombatInfo>> entry2 in m_combatHistory)
		{
			if (entry2.Value == null || entry2.Value.Count == 0)
			{
				continue;
			}
			PlayerLeaderboardRecentCombatsPanel.RecentCombatInfo previousCombat = default(PlayerLeaderboardRecentCombatsPanel.RecentCombatInfo);
			PlayerLeaderboardRecentCombatsPanel.RecentCombatInfo currentCombat = entry2.Value.LastOrDefault();
			if (entry2.Value.Count > 1)
			{
				previousCombat = entry2.Value[entry2.Value.Count - 2];
			}
			if (entry2.Value.Count == roundCount)
			{
				int lastTurnWinStreak = 0;
				if (m_currentWinStreak.ContainsKey(entry2.Key))
				{
					lastTurnWinStreak = m_currentWinStreak[entry2.Key];
				}
				m_currentWinStreak[entry2.Key] = currentCombat.winStreak;
				m_currentLoseStreak[entry2.Key] = currentCombat.loseStreak;
				if (currentCombat.winStreak > 1 && currentCombat.winStreak > previousCombat.winStreak && !suppressNotifications && lastTurnWinStreak != currentCombat.winStreak)
				{
					NotifyPlayerTileEvent(entry2.Key, PlayerTileEvent.WIN_STREAK);
				}
			}
			NotifyPlayerTileEvent(entry2.Key, PlayerTileEvent.RECENT_COMBAT);
		}
	}

	private void AddCombatRound(GameRoundHistoryEntry gameRound)
	{
		Dictionary<int, GameRoundHistoryPlayerEntry> combatRoundLookup = gameRound.Combats.ToDictionary((GameRoundHistoryPlayerEntry combat) => combat.PlayerId, (GameRoundHistoryPlayerEntry combat) => combat);
		foreach (KeyValuePair<int, GameRoundHistoryPlayerEntry> playerCombatEntry in combatRoundLookup)
		{
			int playerId = playerCombatEntry.Key;
			if (playerId != 0 && (!playerCombatEntry.Value.PlayerIsDead || playerCombatEntry.Value.PlayerDiedThisRound))
			{
				AddPlayerToCombatHistoryIfNeeded(playerId);
				GameRoundHistoryPlayerEntry playerEntry = playerCombatEntry.Value;
				GameRoundHistoryPlayerEntry opponentEntry = combatRoundLookup[playerEntry.PlayerOpponentId];
				m_incomingHistory[playerId].Add(ConvertGameRoundHistoryToRecentCombatInfo(playerEntry, opponentEntry));
			}
		}
	}

	private PlayerLeaderboardRecentCombatsPanel.RecentCombatInfo ConvertGameRoundHistoryToRecentCombatInfo(GameRoundHistoryPlayerEntry playerEntry, GameRoundHistoryPlayerEntry opponentEntry)
	{
		PlayerLeaderboardRecentCombatsPanel.RecentCombatInfo playerCombatInfo = default(PlayerLeaderboardRecentCombatsPanel.RecentCombatInfo);
		playerCombatInfo.ownerId = playerEntry.PlayerId;
		playerCombatInfo.opponentId = opponentEntry.PlayerId;
		playerCombatInfo.damage = ((playerEntry.PlayerDamageTaken != 0) ? playerEntry.PlayerDamageTaken : opponentEntry.PlayerDamageTaken);
		playerCombatInfo.isDefeated = playerEntry.PlayerIsDead || opponentEntry.PlayerIsDead;
		playerCombatInfo.friendlyIsDizzy = playerEntry.PlayerIsDizzy;
		playerCombatInfo.enemyIsDizzy = playerEntry.PlayerOpponentIsDizzy;
		playerCombatInfo.ownerTeammateId = playerEntry.PlayerTeammateId;
		playerCombatInfo.opponentTeammateId = playerEntry.PlayerOpponentTeammateId;
		playerCombatInfo.ownerIsFirst = playerEntry.PlayerIsFirst;
		playerCombatInfo.opponentIsFirst = playerEntry.PlayerOpponentIsFirst;
		playerCombatInfo.ownerIsOddManOut = playerEntry.PlayerIsOddManOut;
		playerCombatInfo.opponentIsOddManOut = playerEntry.OpponentIsOddManOut;
		if (playerEntry.PlayerDamageTaken != 0)
		{
			playerCombatInfo.damageTarget = playerEntry.PlayerId;
		}
		else if (opponentEntry.PlayerDamageTaken != 0)
		{
			playerCombatInfo.damageTarget = opponentEntry.PlayerId;
		}
		else
		{
			playerCombatInfo.damageTarget = PlayerLeaderboardRecentCombatsPanel.NO_DAMAGE_TARGET;
		}
		if (m_incomingHistory[playerEntry.PlayerId].Count > 0)
		{
			PlayerLeaderboardRecentCombatsPanel.RecentCombatInfo previousCombat = m_incomingHistory[playerEntry.PlayerId].Last();
			playerCombatInfo.loseStreak = previousCombat.loseStreak;
			playerCombatInfo.winStreak = previousCombat.winStreak;
		}
		if (playerCombatInfo.damageTarget == PlayerLeaderboardRecentCombatsPanel.NO_DAMAGE_TARGET)
		{
			return playerCombatInfo;
		}
		if (playerCombatInfo.damageTarget == playerEntry.PlayerId && (playerCombatInfo.damage > 0 || playerCombatInfo.isDefeated))
		{
			playerCombatInfo.winStreak = 0;
			playerCombatInfo.loseStreak++;
		}
		else
		{
			playerCombatInfo.winStreak++;
			playerCombatInfo.loseStreak = 0;
		}
		return playerCombatInfo;
	}

	private void AddPlayerToCombatHistoryIfNeeded(int playerId)
	{
		if (playerId != 0 && !m_incomingHistory.ContainsKey(playerId))
		{
			m_incomingHistory.Add(playerId, new List<PlayerLeaderboardRecentCombatsPanel.RecentCombatInfo>());
		}
	}

	public List<PlayerLeaderboardRecentCombatsPanel.RecentCombatInfo> GetRecentCombatHistoryForPlayer(int playerId)
	{
		if (m_combatHistory.ContainsKey(playerId))
		{
			return m_combatHistory[playerId];
		}
		return null;
	}

	public int GetLatestWinStreakForPlayer(int playerId)
	{
		if (!m_currentWinStreak.TryGetValue(playerId, out var value))
		{
			return 0;
		}
		return value;
	}

	public int GetLatestLoseStreakForPlayer(int playerId)
	{
		if (!m_currentLoseStreak.TryGetValue(playerId, out var value))
		{
			return 0;
		}
		return value;
	}

	private void SortPlayers()
	{
		m_teams = m_teams.OrderBy((PlayerLeaderboardTeam t) => t.GetBestPlacement()).ToList();
		for (int i = 0; i < m_teams.Count; i++)
		{
			m_teams[i].SetPlaceIcon(i + 1);
		}
	}

	private void UpdateHealthTotals()
	{
		foreach (PlayerLeaderboardTeam team in m_teams)
		{
			team.UpdateTileHealth();
		}
	}

	public void SetOddManOutOpponentHero(Entity entity)
	{
		m_oddManOutOpponentHero = entity;
	}

	public Entity GetOddManOutOpponentHero()
	{
		return m_oddManOutOpponentHero;
	}

	public int GetBattlegroundsLeaderboardMaxArmor()
	{
		if (GameState.Get() == null)
		{
			return 0;
		}
		if (GameState.Get().GetGameEntity() == null)
		{
			return 0;
		}
		return GameState.Get().GetGameEntity().GetTag(GAME_TAG.BACON_MAX_LEADERBOARD_ARMOR);
	}
}
