using System;
using System.Collections.Generic;
using Blizzard.T5.Core;
using UnityEngine;

public class PlayerLeaderboardRecentCombatsPanel : PlayerLeaderboardInformationPanel
{
	public struct RecentCombatInfo
	{
		public int ownerId;

		public int opponentId;

		public int damageTarget;

		public int damage;

		public int winStreak;

		public int loseStreak;

		public bool isDefeated;

		public int ownerTeammateId;

		public int opponentTeammateId;

		public bool ownerIsFirst;

		public bool opponentIsFirst;

		public bool friendlyIsDizzy;

		public bool enemyIsDizzy;

		public bool ownerIsOddManOut;

		public bool opponentIsOddManOut;
	}

	public uint m_maxDisplayItems = 2u;

	public List<GameObject> m_recentActionPlaceholders;

	public GameObject m_recentActionsParent;

	public static int NO_DAMAGE_TARGET = 100000;

	private QueueList<PlayerLeaderboardRecentCombatEntry> m_recentCombatEntries = new QueueList<PlayerLeaderboardRecentCombatEntry>();

	private const string RECENT_COMBAT_ENTRY_PREFAB = "Recent_Combat_Entry.prefab:74bf698d81967c9498554a64c9db51fc";

	private int m_triplesCount;

	private int m_winStreakCount;

	private int m_techLevelCount = 1;

	public PlayerLeaderboardIcon m_techLevel;

	public PlayerLeaderboardIcon m_winStreak;

	public PlayerLeaderboardIcon m_triples;

	public DamageCapPanel m_damageCap;

	private int m_damageCapValue;

	public List<GameObject> m_raceWrappers;

	public GameObject m_singleTribeWithCountWrapper;

	public UberText m_singleTribeWithCountName;

	public UberText m_singleTribeWithCountNumber;

	public GameObject m_singleTribeWithoutCountWrapper;

	public UberText m_singleTribeWithoutCountName;

	public void Awake()
	{
		for (int i = 0; i < m_recentActionPlaceholders.Count; i++)
		{
			PlayerLeaderboardRecentCombatEntry component = m_recentActionPlaceholders[i].GetComponent<PlayerLeaderboardRecentCombatEntry>();
			component.m_iconOpponentSwords.SetActive(value: false);
			component.m_iconOwnerSwords.SetActive(value: false);
			component.m_iconOpponentSplat.SetActive(value: false);
			component.m_iconOwnerSplat.SetActive(value: false);
			component.m_background.gameObject.SetActive(value: true);
			component.HideAllActors();
		}
		m_techLevel.ClearText();
		m_winStreak.ClearText();
		m_triples.ClearText();
		UpdateDamageCap();
	}

	public bool HasRecentCombats()
	{
		return m_recentCombatEntries.Count > 0;
	}

	public void ClearRecentCombats()
	{
		while (m_recentCombatEntries.Count > 0)
		{
			UnityEngine.Object.Destroy(m_recentCombatEntries.Dequeue().gameObject);
		}
	}

	public int GetTripleCount()
	{
		return m_triplesCount;
	}

	public void SetTriples(int triples)
	{
		m_triplesCount = triples;
		UpdateLayout();
	}

	public void SetTechLevel(int techLevel)
	{
		m_techLevelCount = techLevel;
		UpdateLayout();
	}

	public void AddRecentCombat(PlayerLeaderboardCard source, RecentCombatInfo recentCombatInfo)
	{
		GameObject actorObject = AssetLoader.Get().InstantiatePrefab("Recent_Combat_Entry.prefab:74bf698d81967c9498554a64c9db51fc");
		if (actorObject == null)
		{
			Debug.LogWarningFormat("PlayerLeaderboardRecentCombatsPanel.AddRecentCombat() - FAILED to load GameObject \"{0}\"", "Recent_Combat_Entry.prefab:74bf698d81967c9498554a64c9db51fc");
			return;
		}
		PlayerLeaderboardRecentCombatEntry recentEntry = actorObject.GetComponent<PlayerLeaderboardRecentCombatEntry>();
		if (recentEntry == null)
		{
			Debug.LogWarningFormat("PlayerLeaderboardRecentCombatsPanel.AddRecentCombat() - ERROR GameObject \"{0}\" has no PlayerLeaderboardRecentCombatEntry component", "Recent_Combat_Entry.prefab:74bf698d81967c9498554a64c9db51fc");
			return;
		}
		TransformUtil.Identity(recentEntry.transform);
		recentEntry.Load(source, recentCombatInfo);
		if (m_recentCombatEntries.Count == m_maxDisplayItems)
		{
			UnityEngine.Object.Destroy(m_recentCombatEntries.Dequeue().gameObject);
		}
		m_recentCombatEntries.Enqueue(recentEntry);
		m_winStreakCount = recentCombatInfo.winStreak;
		UpdateLayout();
	}

	private void UpdateDamageCap()
	{
		if (m_damageCap != null)
		{
			m_damageCapValue = GameState.Get().GetGameEntity().GetTag(GAME_TAG.BACON_COMBAT_DAMAGE_CAP);
			m_damageCap.gameObject.SetActive(GameState.Get().GetGameEntity().GetRealtimeBaconDamageCapEnabled());
			m_damageCap.SetText(m_damageCapValue.ToString());
		}
	}

	private void UpdateTechLevelPlaymaker()
	{
		PlayMakerFSM fsm = m_techLevel.GetComponent<PlayMakerFSM>();
		if (fsm == null)
		{
			Log.Gameplay.PrintError("No playmaker attached to tech level icon.");
			return;
		}
		fsm.FsmVariables.GetFsmInt("TechLevel").Value = m_techLevelCount;
		fsm.SendEvent("Action");
	}

	private void UpdateLayout()
	{
		if (m_triples != null)
		{
			m_triples.SetText(m_triplesCount.ToString());
		}
		if (m_winStreak != null)
		{
			m_winStreak.SetText(m_winStreakCount.ToString());
		}
		if (m_techLevel != null)
		{
			UpdateTechLevelPlaymaker();
		}
		UpdateDamageCap();
		if (m_recentActionPlaceholders == null)
		{
			return;
		}
		for (int i = 0; i < m_recentActionPlaceholders.Count; i++)
		{
			if (m_recentCombatEntries.Count > i)
			{
				int targetPlaceholderIndex = Math.Min(m_recentActionPlaceholders.Count, m_recentCombatEntries.Count) - (1 + i);
				GameObject targetPlaceholder = m_recentActionPlaceholders[targetPlaceholderIndex];
				GameObject obj = m_recentCombatEntries[i].gameObject;
				obj.transform.parent = targetPlaceholder.transform.parent;
				TransformUtil.CopyLocal(obj, targetPlaceholder);
				targetPlaceholder.SetActive(value: false);
			}
			else
			{
				m_recentActionPlaceholders[i].SetActive(value: true);
			}
		}
	}

	internal bool SetRaces(Map<TAG_RACE, int> raceCounts)
	{
		int allTypeAdjustment = 0;
		if (raceCounts.ContainsKey(TAG_RACE.ALL))
		{
			allTypeAdjustment = raceCounts[TAG_RACE.ALL];
		}
		TAG_RACE mostMinionRace = TAG_RACE.ALL;
		int mostMinionCount = allTypeAdjustment;
		int secondMostMinionCount = 0;
		foreach (KeyValuePair<TAG_RACE, int> entry in raceCounts)
		{
			if (entry.Key != TAG_RACE.ALL)
			{
				int adjustedCount = entry.Value + allTypeAdjustment;
				if (adjustedCount >= mostMinionCount && adjustedCount > 0)
				{
					secondMostMinionCount = mostMinionCount;
					mostMinionCount = adjustedCount;
					mostMinionRace = entry.Key;
				}
				else if (adjustedCount >= secondMostMinionCount && adjustedCount > 0)
				{
					secondMostMinionCount = adjustedCount;
					_ = entry.Key;
				}
			}
		}
		if (mostMinionRace == TAG_RACE.ALL || mostMinionCount == secondMostMinionCount)
		{
			if (mostMinionCount == 0)
			{
				m_singleTribeWithoutCountWrapper.SetActive(value: false);
				m_singleTribeWithCountWrapper.SetActive(value: false);
			}
			else
			{
				m_singleTribeWithoutCountWrapper.SetActive(value: true);
				m_singleTribeWithCountWrapper.SetActive(value: false);
			}
		}
		else
		{
			m_singleTribeWithoutCountWrapper.SetActive(value: false);
			m_singleTribeWithCountWrapper.SetActive(value: true);
			m_singleTribeWithCountNumber.Text = mostMinionCount.ToString();
			m_singleTribeWithCountName.Text = GameStrings.GetRaceNameBattlegrounds(mostMinionRace);
		}
		return true;
	}
}
