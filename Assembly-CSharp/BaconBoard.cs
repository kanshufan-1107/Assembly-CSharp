using System;
using System.Collections;
using System.Collections.Generic;
using Blizzard.T5.AssetManager;
using Blizzard.T5.Core.Utils;
using UnityEngine;

public class BaconBoard : Board
{
	private class BoardSkinStatus
	{
		public GameObject m_CombatPrefab;

		public GameObject m_TavernPrefab;

		public GameObject m_TeammmateTavernPrefab;

		public AssetHandle<GameObject> m_AssetHandleCombat;

		public AssetHandle<GameObject> m_AssetHandleTavern;

		public AssetHandle<GameObject> m_AssetHandleTeammate;

		public BaconBoardSkinBehaviour m_CombatInstance;

		public BaconBoardSkinBehaviour m_TavernInstance;

		public BaconBoardSkinBehaviour m_TeammateTavernInstance;
	}

	private class BoardSkinStatusAndRound
	{
		public BoardSkinStatus m_Skin;

		public int m_Round;

		public BoardSkinStatusAndRound(BoardSkinStatus skin, int round)
		{
			m_Skin = skin;
			m_Round = round;
		}
	}

	public delegate void StateChangeCallback(TAG_BOARD_VISUAL_STATE newState);

	public float m_ShopAmbientTransitionDelay = 0.5f;

	public float m_ShopAmbientTransitionTime = 0.25f;

	public GameObject m_LeaderboardFrame;

	public GameObject m_TeammateLeaderboardFrame;

	public GameObject m_TableTop;

	public const int BOARD_SKIN_UNINITIALIZED = 0;

	public const int BOARD_SKIN_DEFAULT = 1;

	private readonly BoardSkinStatus m_FullBoardSkin = new BoardSkinStatus();

	private int m_ChosenBoardSkinId;

	private int m_ChosenCombatBoardSkinId;

	private int m_TeammateBoardSkinId;

	private bool m_InBattle;

	private int m_BattleRound;

	private int m_PendingLoads;

	private bool m_StartedLoadingChosenBoardThisRound;

	private TAG_BOARD_VISUAL_STATE m_currentBoardState;

	private HashSet<string> m_minionsDefeatedByPlayer = new HashSet<string>();

	private HashSet<TAG_RACE> m_racesDefeatedByPlayer = new HashSet<TAG_RACE>();

	private int m_minionsDefeatedCount;

	private static BaconBoard s_Instance;

	private int m_CheatWinstreak;

	private bool m_CheatHasDefeatedOpponent;

	private StateChangeCallback m_stateChangeCallback;

	private DateTime m_LastBoardVisualStateChangeDateTime;

	public void CheatSetWinstreak(int streak)
	{
		m_CheatWinstreak = streak;
	}

	public void CheatSetDefeatedMinionCount(int count)
	{
		m_minionsDefeatedCount = count;
	}

	public void CheatSetHasDefeatedOpponent()
	{
		m_CheatHasDefeatedOpponent = true;
	}

	public void CheatAddDefeatedRace(TAG_RACE race)
	{
		m_racesDefeatedByPlayer.Add(race);
	}

	public void CheatAddDefeatedMinion(string cardID)
	{
		m_minionsDefeatedByPlayer.Add(cardID);
	}

	public bool CheatTriggerDefeatedMinion(string cardID)
	{
		if (m_FullBoardSkin.m_CombatInstance != null)
		{
			m_FullBoardSkin.m_CombatInstance.CheatTriggerDefeatMinion(cardID);
			return true;
		}
		return false;
	}

	public bool CheatTriggerHeroHeavyHitEffects()
	{
		if (m_FullBoardSkin.m_CombatInstance != null)
		{
			m_FullBoardSkin.m_CombatInstance.CheatTriggerHeroHeavyHitBoardEffects();
			return true;
		}
		return false;
	}

	public bool CheatTriggerMinionHeavyHitEffects()
	{
		if (m_FullBoardSkin.m_CombatInstance != null)
		{
			m_FullBoardSkin.m_CombatInstance.CheatTriggerMinionHeavyHitBoardEffects();
			return true;
		}
		return false;
	}

	public bool CheatTriggerAllBoardEffects()
	{
		if (m_FullBoardSkin.m_CombatInstance != null)
		{
			m_FullBoardSkin.m_CombatInstance.CheatTriggerAllBoardEffects();
			return true;
		}
		return false;
	}

	public new static BaconBoard Get()
	{
		return s_Instance;
	}

	public override void Start()
	{
		base.Start();
		s_Instance = this;
		m_LastBoardVisualStateChangeDateTime = DateTime.Now;
		m_currentBoardState = TAG_BOARD_VISUAL_STATE.SHOP;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		s_Instance = null;
		m_InBattle = false;
		UnloadSkinAssets();
	}

	public void AddStateChangeCallback(StateChangeCallback newCallback)
	{
		m_stateChangeCallback = (StateChangeCallback)Delegate.Combine(m_stateChangeCallback, newCallback);
	}

	public void RemoveStateChangeCallback(StateChangeCallback oldCallback)
	{
		m_stateChangeCallback = (StateChangeCallback)Delegate.Remove(m_stateChangeCallback, oldCallback);
	}

	public override bool AreAllAssetsLoaded()
	{
		return m_PendingLoads <= 0;
	}

	public override void ChangeBoardVisualState(TAG_BOARD_VISUAL_STATE boardState)
	{
		SendBoardVisualStatChangeTelmetry(m_currentBoardState, boardState, m_LastBoardVisualStateChangeDateTime);
		m_LastBoardVisualStateChangeDateTime = DateTime.Now;
		m_currentBoardState = boardState;
		if (m_currentBoardState == TAG_BOARD_VISUAL_STATE.COMBAT)
		{
			OnTransitionToBattle();
		}
		if (m_currentBoardState == TAG_BOARD_VISUAL_STATE.SHOP)
		{
			OnTransitionToShop();
		}
	}

	public void LoadInitialTavernBoard(int chosenBoardSkinId)
	{
		m_ChosenBoardSkinId = chosenBoardSkinId;
		TryToLoadTavernPrefab(m_FullBoardSkin);
	}

	public void LoadTeammateTavernBoard(int chosenBoardSkinId)
	{
		m_TeammateBoardSkinId = chosenBoardSkinId;
		TryToLoadTeammateTavernPrefab(m_FullBoardSkin);
	}

	public void OnBoardSkinChosen(int chosenBoardSkinId)
	{
		m_ChosenCombatBoardSkinId = chosenBoardSkinId;
		TryLoadChosenBoardSkinPrefabs();
	}

	public void ProcessUnloadRequest(BaconBoardSkinBehaviour sourceBehavior)
	{
		UnloadSkinAssets();
	}

	public void NotifyOfMinionDied(Entity minion)
	{
		if (!m_InBattle || m_FullBoardSkin.m_CombatInstance == null || !minion.IsControlledByOpposingSidePlayer())
		{
			return;
		}
		m_minionsDefeatedCount++;
		EntityDef minionDef = minion.GetEntityDef();
		m_minionsDefeatedByPlayer.Add(minionDef.GetCardId());
		foreach (TAG_RACE race in minionDef.GetRaces())
		{
			m_racesDefeatedByPlayer.Add(race);
		}
		m_FullBoardSkin.m_CombatInstance.PlayOpponentMinionDefeatedCount(m_minionsDefeatedCount);
		m_FullBoardSkin.m_CombatInstance.PlayOpponentMinionDefeated(minionDef);
	}

	private void ToggleLeaderboardFrame(bool visible)
	{
		if (m_LeaderboardFrame != null)
		{
			m_LeaderboardFrame.SetActive(visible);
		}
	}

	private void ToggleTeammateLeaderboardFrame(bool visible)
	{
		if (m_TeammateLeaderboardFrame != null)
		{
			m_TeammateLeaderboardFrame.SetActive(visible);
		}
	}

	private void SetTeammateLeaderboardFrameOffset(Vector3 offset)
	{
		if (m_LeaderboardFrame != null && m_TeammateLeaderboardFrame != null)
		{
			m_TeammateLeaderboardFrame.transform.position = m_LeaderboardFrame.transform.position + offset;
		}
	}

	private void ToggleTableTop(bool visible)
	{
		if (m_TableTop != null)
		{
			m_TableTop.SetActive(visible);
		}
	}

	private void OnTransitionToBattle()
	{
		m_InBattle = true;
		TryLoadChosenBoardSkinPrefabs();
	}

	private void OnTransitionToShop()
	{
		m_InBattle = false;
		m_BattleRound++;
		m_StartedLoadingChosenBoardThisRound = false;
		m_ChosenCombatBoardSkinId = 0;
		RunVisualStateAnimators(TAG_BOARD_VISUAL_STATE.SHOP);
		SetShopLighting();
		ToggleLeaderboardFrame(!m_FullBoardSkin.m_TavernInstance.HasOwnLeaderboardFrame());
		ToggleTableTop(!m_FullBoardSkin.m_TavernInstance.HasOwnTableTop());
		RunShopAnimation(m_FullBoardSkin);
	}

	public void ChangeBoardVisualStateForPreview(TAG_BOARD_VISUAL_STATE boardState, BaconBoardSkinBehaviour combatSkin, BaconBoardSkinBehaviour tavernSkin)
	{
		RunVisualStateAnimators(boardState);
		BaconBoardSkinBehaviour newActiveSkin = ((boardState == TAG_BOARD_VISUAL_STATE.COMBAT) ? combatSkin : tavernSkin);
		ToggleLeaderboardFrame(!newActiveSkin.HasOwnLeaderboardFrame());
		ToggleTableTop(!newActiveSkin.HasOwnTableTop());
		if (boardState == TAG_BOARD_VISUAL_STATE.COMBAT && combatSkin != null)
		{
			combatSkin.CopyCornersFromSkin(tavernSkin);
		}
		if (combatSkin != null)
		{
			combatSkin.SetBoardState(boardState);
		}
		if (tavernSkin != null)
		{
			tavernSkin.SetBoardState(boardState);
		}
	}

	public void SetShopLighting()
	{
		Action<object> ambientUpdate = delegate(object amount)
		{
			RenderSettings.ambientLight = (Color)amount;
		};
		Hashtable cArgs = iTweenManager.Get().GetTweenHashTable();
		cArgs.Add("from", RenderSettings.ambientLight);
		cArgs.Add("to", m_AmbientColor);
		cArgs.Add("delay", m_ShopAmbientTransitionDelay);
		cArgs.Add("time", m_ShopAmbientTransitionTime);
		cArgs.Add("easetype", iTween.EaseType.easeInOutQuad);
		cArgs.Add("onupdate", ambientUpdate);
		cArgs.Add("onupdatetarget", base.gameObject);
		iTween.ValueTo(base.gameObject, cArgs);
	}

	private void TryLoadChosenBoardSkinPrefabs()
	{
		if (m_InBattle && m_ChosenCombatBoardSkinId != 0 && !m_StartedLoadingChosenBoardThisRound)
		{
			m_StartedLoadingChosenBoardThisRound = true;
			TryToLoadPrefab(m_FullBoardSkin);
		}
	}

	private void TryToLoadPrefab(BoardSkinStatus skin)
	{
		if (m_ChosenCombatBoardSkinId == 0)
		{
			m_ChosenCombatBoardSkinId = 1;
		}
		BattlegroundsBoardSkinDbfRecord boardRecord = GameDbf.BattlegroundsBoardSkin.GetRecord(m_ChosenCombatBoardSkinId);
		if (boardRecord == null)
		{
			Debug.LogError($"BaconBoard.TryToLoadPrefab - Failed to get BG Board Skin Record for board skin id: {m_ChosenBoardSkinId}");
			return;
		}
		string boardAsset = ((PlatformSettings.Screen != ScreenCategory.Phone) ? boardRecord.FullBoardPrefab : boardRecord.FullBoardPrefabPhone);
		m_PendingLoads++;
		AssetLoader.Get().LoadAsset<GameObject>(boardAsset, OnSkinLoaded, new BoardSkinStatusAndRound(skin, m_BattleRound));
	}

	private void TryToLoadTavernPrefab(BoardSkinStatus skin)
	{
		if (m_ChosenBoardSkinId == 0)
		{
			m_ChosenBoardSkinId = 1;
		}
		BattlegroundsBoardSkinDbfRecord boardRecord = GameDbf.BattlegroundsBoardSkin.GetRecord(m_ChosenBoardSkinId);
		if (boardRecord == null)
		{
			Debug.LogError($"BaconBoard.TryToLoadPrefab - Failed to get BG Board Skin Record for board skin id: {m_ChosenBoardSkinId}");
			return;
		}
		string boardAsset = ((PlatformSettings.Screen != ScreenCategory.Phone) ? boardRecord.FullTavernBoardPrefab : boardRecord.FullTavernBoardPrefabPhone);
		m_PendingLoads++;
		if (!AssetLoader.Get().LoadAsset<GameObject>(boardAsset, OnTavernSkinLoadAttempted, skin))
		{
			OnTavernSkinLoadAttempted(boardAsset, null, skin);
		}
	}

	private void OnTavernSkinLoadAttempted(AssetReference assetRef, AssetHandle<GameObject> asset, object callbackData)
	{
		m_PendingLoads--;
		BoardSkinStatus skin = (BoardSkinStatus)callbackData;
		if (skin == null || asset == null)
		{
			Debug.LogError("OnTavernSkinLoadAttempted - skin or asset is null");
			return;
		}
		skin.m_AssetHandleTavern = asset;
		skin.m_TavernPrefab = asset.Asset;
		if (!AreAllAssetsLoaded())
		{
			return;
		}
		GameObject playerInstance = UnityEngine.Object.Instantiate(skin.m_TavernPrefab);
		if (playerInstance == null || !playerInstance.TryGetComponent<BaconBoardSkinBehaviour>(out skin.m_TavernInstance))
		{
			Debug.LogError("Attempting to get component BaconBoardSkinBehaviour but not found on " + playerInstance);
			return;
		}
		ToggleLeaderboardFrame(!skin.m_TavernInstance.HasOwnLeaderboardFrame());
		ToggleTableTop(!skin.m_TavernInstance.HasOwnTableTop());
		if (m_AllAssetsLoadedCallback != null)
		{
			m_AllAssetsLoadedCallback();
		}
	}

	private void TryToLoadTeammateTavernPrefab(BoardSkinStatus skin)
	{
		if (m_TeammateBoardSkinId == 0)
		{
			m_TeammateBoardSkinId = 1;
		}
		BattlegroundsBoardSkinDbfRecord boardRecord = GameDbf.BattlegroundsBoardSkin.GetRecord(m_TeammateBoardSkinId);
		if (boardRecord == null)
		{
			Debug.LogError($"BaconBoard.TryToLoadPrefab - Failed to get BG Board Skin Record for board skin id: {m_TeammateBoardSkinId}");
			return;
		}
		string boardAsset = ((PlatformSettings.Screen != ScreenCategory.Phone) ? boardRecord.FullTavernBoardPrefab : boardRecord.FullTavernBoardPrefabPhone);
		AssetLoader.Get().LoadAsset<GameObject>(boardAsset, OnTeammateTavernSkinLoaded, skin);
	}

	private void OnTeammateTavernSkinLoaded(AssetReference assetRef, AssetHandle<GameObject> asset, object callbackData)
	{
		BoardSkinStatus skin = (BoardSkinStatus)callbackData;
		if (skin == null || asset == null)
		{
			Debug.LogError("OnTeammateTavernSkinLoaded - skin or asset is null");
			return;
		}
		skin.m_AssetHandleTeammate = asset;
		skin.m_TeammmateTavernPrefab = asset.Asset;
		GameObject playerInstance = UnityEngine.Object.Instantiate(skin.m_TeammmateTavernPrefab);
		if (playerInstance == null || !playerInstance.TryGetComponent<BaconBoardSkinBehaviour>(out skin.m_TeammateTavernInstance))
		{
			Debug.LogError("Attempting to get component BaconBoardSkinBehaviour but not found on " + playerInstance);
			return;
		}
		Vector3 teammateBoardPos = Board.Get().FindBone("TeammateBoardPosition").position;
		skin.m_TeammateTavernInstance.transform.position = teammateBoardPos;
		ToggleTeammateLeaderboardFrame(!skin.m_TeammateTavernInstance.HasOwnLeaderboardFrame());
		SetTeammateLeaderboardFrameOffset(teammateBoardPos);
	}

	private void OnSkinLoaded(AssetReference assetRef, AssetHandle<GameObject> asset, object callbackData)
	{
		m_PendingLoads--;
		BoardSkinStatusAndRound obj = (BoardSkinStatusAndRound)callbackData;
		BoardSkinStatus skin = obj?.m_Skin;
		if (obj == null || skin == null)
		{
			Log.All.PrintWarning($"[BaconBoard.OnSkinLoaded] skin or skinWithRound is null, assetRef:{assetRef}");
		}
		if (obj.m_Round != m_BattleRound)
		{
			asset.Dispose();
			return;
		}
		skin.m_AssetHandleCombat = asset;
		skin.m_CombatPrefab = asset.Asset;
		if (AreAllAssetsLoaded())
		{
			TryStartBattleTransitionAnimations();
			if (m_AllAssetsLoadedCallback != null)
			{
				m_AllAssetsLoadedCallback();
			}
		}
	}

	private void RunShopAnimation(BoardSkinStatus skin)
	{
		if (skin.m_CombatInstance != null)
		{
			skin.m_CombatInstance.SetBoardState(TAG_BOARD_VISUAL_STATE.SHOP);
			skin.m_CombatInstance.QueueToUnload(this);
		}
		if (skin.m_TavernInstance != null)
		{
			skin.m_TavernInstance.SetBoardState(TAG_BOARD_VISUAL_STATE.SHOP);
		}
	}

	public void FriendlyPlayerFinisherCalled()
	{
		Entity opponentEntity = GameState.Get().GetOpposingSidePlayer().GetHero();
		if (m_currentBoardState == TAG_BOARD_VISUAL_STATE.COMBAT && opponentEntity.HasTag(GAME_TAG.TRANSIENT_ENTITY) && opponentEntity.GetCurrentHealth() < 0)
		{
			SetOpponentHeroDefeated();
		}
	}

	public bool SetOpponentHeroDefeated()
	{
		if (m_FullBoardSkin.m_CombatInstance != null)
		{
			m_FullBoardSkin.m_CombatInstance.PlayOpponentHeroDefeated();
			return true;
		}
		return false;
	}

	private void TryStartBattleTransitionAnimations()
	{
		if (m_InBattle && !(m_FullBoardSkin.m_CombatInstance != null))
		{
			StartBattleTransitionAnimation(m_FullBoardSkin);
			ToggleLeaderboardFrame(!m_FullBoardSkin.m_CombatInstance.HasOwnLeaderboardFrame());
			ToggleTableTop(!m_FullBoardSkin.m_CombatInstance.HasOwnTableTop());
			RunVisualStateAnimators(TAG_BOARD_VISUAL_STATE.COMBAT);
		}
	}

	private void StartBattleTransitionAnimation(BoardSkinStatus skin)
	{
		GameObject playerInstance = UnityEngine.Object.Instantiate(skin.m_CombatPrefab);
		if (!playerInstance.TryGetComponent<BaconBoardSkinBehaviour>(out skin.m_CombatInstance))
		{
			Debug.LogError("Attempting to get component BaconBoardSkinBehaviour but not found on " + playerInstance);
			return;
		}
		PlayerLeaderboardManager leaderboardManager = PlayerLeaderboardManager.Get();
		int friendlyPlayerId = GameState.Get().GetFriendlyPlayerId();
		int currentWinStreak = ((m_CheatWinstreak > 0) ? m_CheatWinstreak : leaderboardManager.GetLatestWinStreakForPlayer(friendlyPlayerId));
		if (currentWinStreak > 0)
		{
			skin.m_CombatInstance.RequestWinStreak(currentWinStreak);
		}
		int currentLoseStreak = leaderboardManager.GetLatestLoseStreakForPlayer(friendlyPlayerId);
		if (currentLoseStreak > 0)
		{
			skin.m_CombatInstance.RequestLoseStreak(currentLoseStreak);
		}
		if (m_minionsDefeatedCount > 0)
		{
			skin.m_CombatInstance.RequestOpponentMinionPreviouslyDefeatedCount(m_minionsDefeatedCount);
		}
		foreach (string minion in m_minionsDefeatedByPlayer)
		{
			skin.m_CombatInstance.RequestFriendlyPlayerHasDefeatedMinion(minion);
		}
		foreach (TAG_RACE race in m_racesDefeatedByPlayer)
		{
			skin.m_CombatInstance.RequestFriendlyPlayerHasDefeatedRace(race);
		}
		if (GameState.Get().GetFriendlySidePlayer().GetHero()
			.GetRealTimePlayerLeaderboardPlace() <= 4)
		{
			skin.m_CombatInstance.RequestTopFourPlacement();
		}
		if (m_CheatHasDefeatedOpponent)
		{
			skin.m_CombatInstance.RequestHasFriendlyPlayerDefeatedOpponent();
		}
		else
		{
			List<PlayerLeaderboardRecentCombatsPanel.RecentCombatInfo> recentCombats = leaderboardManager.GetRecentCombatHistoryForPlayer(friendlyPlayerId);
			if (recentCombats != null)
			{
				foreach (PlayerLeaderboardRecentCombatsPanel.RecentCombatInfo item in recentCombats)
				{
					if (item.isDefeated)
					{
						skin.m_CombatInstance.RequestHasFriendlyPlayerDefeatedOpponent();
						break;
					}
				}
			}
		}
		Entity friendlyPlayer = GameState.Get().GetFriendlySidePlayer().GetHero();
		if (friendlyPlayer != null)
		{
			skin.m_CombatInstance.RequestFriendlyPlayerHealthAtOrBelow(friendlyPlayer.GetDefHealth(), friendlyPlayer.GetCurrentHealth());
		}
		if (skin.m_TavernInstance != null)
		{
			skin.m_CombatInstance.CopyCornersFromSkin(skin.m_TavernInstance);
			skin.m_TavernInstance.SetBoardState(TAG_BOARD_VISUAL_STATE.COMBAT);
		}
		skin.m_CombatInstance.SetBoardState(TAG_BOARD_VISUAL_STATE.COMBAT);
	}

	public void CheckForHeroHeavyHitBoardEffects(Card sourceCard, Card targetCard)
	{
		if (m_FullBoardSkin.m_CombatInstance != null)
		{
			m_FullBoardSkin.m_CombatInstance.CheckForHeroHeavyHitBoardEffects(sourceCard, targetCard);
		}
	}

	private void UnloadSkinAssets()
	{
		m_ChosenCombatBoardSkinId = 0;
		UnloadSkinAsset(m_FullBoardSkin);
	}

	private void UnloadSkinAsset(BoardSkinStatus skin)
	{
		if (skin.m_CombatInstance != null)
		{
			UnityEngine.Object.Destroy(skin.m_CombatInstance.gameObject);
			skin.m_CombatInstance = null;
		}
		skin.m_CombatPrefab = null;
		if (skin.m_AssetHandleCombat != null)
		{
			skin.m_AssetHandleCombat.Dispose();
			skin.m_AssetHandleCombat = null;
		}
	}

	public void RunVisualStateAnimators(TAG_BOARD_VISUAL_STATE boardState)
	{
		if (m_stateChangeCallback != null)
		{
			m_stateChangeCallback(boardState);
		}
		if (m_BoardStateChangingObjects == null || m_BoardStateChangingObjects.Count == 0)
		{
			return;
		}
		foreach (PlayMakerFSM boardStateChangingObject in m_BoardStateChangingObjects)
		{
			boardStateChangingObject.SetState(EnumUtils.GetString(boardState));
		}
	}

	protected void SendBoardVisualStatChangeTelmetry(TAG_BOARD_VISUAL_STATE fromBoardState, TAG_BOARD_VISUAL_STATE toBoardState, DateTime lastBoardStateChangeDateTime)
	{
		int numberOfSeconds = (int)(DateTime.Now - lastBoardStateChangeDateTime).TotalSeconds;
		TelemetryManager.Client().SendBoardVisualStateChanged(fromBoardState.ToString(), toBoardState.ToString(), numberOfSeconds);
	}
}
