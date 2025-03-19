using System;
using System.Collections;
using System.Collections.Generic;
using Blizzard.T5.Core.Utils;
using HutongGames.PlayMaker;
using UnityEngine;
using UnityEngine.Serialization;

[ExecuteAlways]
public class BaconBoardSkinBehaviour : MonoBehaviour
{
	[Serializable]
	public class BaconBoardSkinCorners
	{
		public BaconBoardSkinCorner TL;

		public BaconBoardSkinCorner TR;

		public BaconBoardSkinCorner BL;

		public BaconBoardSkinCorner BR;
	}

	private const string FSM_STATE_NAMES_WIN_STREAK = "WIN_STREAK_{0}";

	private const string FSM_STATE_NAMES_LOSE_STREAK = "LOSE_STREAK_{0}";

	private const string FSM_STATE_NAMES_TOP_FOUR = "HERO_TOP_4";

	private const string FSM_STATE_NAMES_HERO_DEFEAT = "HERO_DEFEAT_ENEMY";

	private const string FSM_STATE_NAMES_HERO_DEFEATED = "HERO_DEFEATED_ENEMY";

	private const string FSM_STATE_NAMES_MINION_DEFEAT = "MINION_DEFEAT_{0}";

	private const string FSM_STATE_NAMES_MINION_DEFEATED = "MINION_DEFEATED_{0}";

	private const string FSM_STATE_NAMES_MINION_DEFEAT_COUNT = "MINION_DEFEAT_COUNT_{0}";

	private const string FSM_STATE_NAMES_MINION_DEFEATED_COUNT = "MINION_DEFEATED_COUNT_{0}";

	private const string FSM_STATE_NAMES_MINION_TRIBE_DEFEAT = "MINION_DEFEAT_TRIBE_{0}";

	private const string FSM_STATE_NAMES_MINION_TRIBE_DEFEATED = "MINION_DEFEATED_TRIBE_{0}";

	private const string FSM_STATE_NAMES_HEALTH_AT_OR_BELOW = "HEALTH_AT_OR_BELOW_{0}";

	private const string FSM_STATE_NAMES_HERO_HEAVY_HIT = "HEAVY_HIT";

	private const string FSM_STATE_NAMES_MINION_HEAVY_HIT = "MINION_HEAVY_HIT";

	public TAG_BOARD_VISUAL_STATE m_BoardType;

	[UnityEngine.Tooltip("If checked apply a default lighting transition using the color and timing variables below.")]
	public bool m_DefaultLightingEnabled = true;

	[SerializeField]
	[UnityEngine.Tooltip("If checked then this board has its own leaderboard frame and should hide the base one.")]
	private bool m_HasOwnLeaderboardFrame;

	[UnityEngine.Tooltip("If checked then this board has its own table top and should hide the base one.")]
	[SerializeField]
	private bool m_HasOwnTableTop;

	[UnityEngine.Tooltip("Minimum minion damage required before it's considered a 'heavy hit' (exposed for designer tweaking).")]
	public int m_MinMinionHeavyHitDamage = 100;

	public Color m_AmbientColor;

	[FormerlySerializedAs("m_CombatAmbientTransitionDelay")]
	public float m_AmbientTransitionDelay = 0.5f;

	[FormerlySerializedAs("m_CombatAmbientTransitionTime")]
	public float m_AmbientTransitionTime = 0.25f;

	[HideInInspector]
	public float m_CombatAmbientTransitionDelay = 0.5f;

	[HideInInspector]
	public float m_CombatAmbientTransitionTime = 0.25f;

	[UnityEngine.Tooltip("The number of seconds after the back-to-shop animation starts before this board skin object should be unloaded.")]
	public float m_UnloadDelay = 1.5f;

	public BaconBoardSkinCorners m_Corners;

	public List<PlayMakerFSM> m_BoardStateChangingObjects;

	public List<TextureTweenController> m_BoardTextureChangingObjects;

	public List<string> m_UniqueFsmTriggerEventOrder = new List<string>();

	private List<string> m_DeferredFsmTriggerRequests = new List<string>();

	protected TAG_BOARD_VISUAL_STATE GetActivatedState()
	{
		return m_BoardType;
	}

	private void TransitionToActivatedState(TAG_BOARD_VISUAL_STATE newBoardState)
	{
		SetLighting();
		m_DeferredFsmTriggerRequests.Add(EnumUtils.GetString(newBoardState));
		ProcessDeferredFsmTriggerRequests();
	}

	private void TransitionFromActivatedState(TAG_BOARD_VISUAL_STATE newBoardState)
	{
		SetStateOnFsms(EnumUtils.GetString(newBoardState));
	}

	private void StartAdditionalTransitions(TAG_BOARD_VISUAL_STATE newBoardState)
	{
		foreach (TextureTweenController tweenController in m_BoardTextureChangingObjects)
		{
			if (!(tweenController == null))
			{
				if (newBoardState == GetActivatedState())
				{
					tweenController.StartForwardTransition();
				}
				else
				{
					tweenController.StartReverseTransition();
				}
			}
		}
	}

	public bool HasOwnLeaderboardFrame()
	{
		return m_HasOwnLeaderboardFrame;
	}

	public bool HasOwnTableTop()
	{
		return m_HasOwnTableTop;
	}

	public void CopyCornersFromSkin(BaconBoardSkinBehaviour source)
	{
		m_Corners.TL.CopyToBackside(source.m_Corners.TL.m_TopContainer);
		m_Corners.TR.CopyToBackside(source.m_Corners.TR.m_TopContainer);
		m_Corners.BL.CopyToBackside(source.m_Corners.BL.m_TopContainer);
		m_Corners.BR.CopyToBackside(source.m_Corners.BR.m_TopContainer);
	}

	public void SetBoardState(TAG_BOARD_VISUAL_STATE newBoardState)
	{
		if (newBoardState == GetActivatedState())
		{
			TransitionToActivatedState(newBoardState);
		}
		else
		{
			TransitionFromActivatedState(newBoardState);
		}
		StartAdditionalTransitions(newBoardState);
	}

	private void ProcessDeferredFsmTriggerRequests()
	{
		int topEventIndex = -1;
		foreach (string deferredTriggerRequest in m_DeferredFsmTriggerRequests)
		{
			int eventIndex = m_UniqueFsmTriggerEventOrder.IndexOf(deferredTriggerRequest);
			if (eventIndex < 0)
			{
				SetStateOnFsms(deferredTriggerRequest);
			}
			else if (topEventIndex < 0 || eventIndex < topEventIndex)
			{
				topEventIndex = eventIndex;
			}
		}
		if (0 <= topEventIndex)
		{
			SetStateOnFsms(m_UniqueFsmTriggerEventOrder[topEventIndex]);
		}
		m_DeferredFsmTriggerRequests.Clear();
	}

	public void RequestWinStreak(int winStreak)
	{
		foreach (PlayMakerFSM fsm in m_BoardStateChangingObjects)
		{
			for (int i = winStreak; i > 0; i--)
			{
				string stateName = $"WIN_STREAK_{i}";
				if (FsmContainsState(fsm, stateName))
				{
					m_DeferredFsmTriggerRequests.Add(stateName);
					break;
				}
			}
		}
	}

	public void RequestLoseStreak(int loseStreak)
	{
		foreach (PlayMakerFSM fsm in m_BoardStateChangingObjects)
		{
			for (int i = loseStreak; i > 0; i--)
			{
				string stateName = $"LOSE_STREAK_{i}";
				if (FsmContainsState(fsm, stateName))
				{
					m_DeferredFsmTriggerRequests.Add(stateName);
					break;
				}
			}
		}
	}

	public void RequestTopFourPlacement()
	{
		m_DeferredFsmTriggerRequests.Add("HERO_TOP_4");
	}

	public void RequestFriendlyPlayerHealthAtOrBelow(int maxHealth, int currentHealth)
	{
		foreach (PlayMakerFSM fsm in m_BoardStateChangingObjects)
		{
			for (int i = currentHealth; i <= maxHealth; i++)
			{
				string stateName = $"HEALTH_AT_OR_BELOW_{i}";
				if (FsmContainsState(fsm, stateName))
				{
					m_DeferredFsmTriggerRequests.Add(stateName);
					break;
				}
			}
		}
	}

	public void RequestFriendlyPlayerHasDefeatedMinion(string minionCardID)
	{
		m_DeferredFsmTriggerRequests.Add($"MINION_DEFEATED_{minionCardID.ToUpper()}");
	}

	public void RequestFriendlyPlayerHasDefeatedRace(TAG_RACE race)
	{
		if (race == TAG_RACE.ALL)
		{
			string[] names = Enum.GetNames(typeof(TAG_RACE));
			foreach (string raceName in names)
			{
				m_DeferredFsmTriggerRequests.Add($"MINION_DEFEATED_TRIBE_{raceName}");
			}
		}
		else
		{
			m_DeferredFsmTriggerRequests.Add($"MINION_DEFEATED_TRIBE_{Enum.GetName(typeof(TAG_RACE), race)}");
		}
	}

	public void RequestOpponentMinionPreviouslyDefeatedCount(int defeatedCount)
	{
		foreach (PlayMakerFSM fsm in m_BoardStateChangingObjects)
		{
			for (int i = defeatedCount; i > 0; i--)
			{
				string stateName = $"MINION_DEFEATED_COUNT_{i}";
				if (FsmContainsState(fsm, stateName))
				{
					m_DeferredFsmTriggerRequests.Add(stateName);
					break;
				}
			}
		}
	}

	public void RequestHasFriendlyPlayerDefeatedOpponent()
	{
		m_DeferredFsmTriggerRequests.Add("HERO_DEFEATED_ENEMY");
	}

	public void PlayOpponentHeroDefeated()
	{
		SetStateOnFsms("HERO_DEFEAT_ENEMY");
	}

	public void PlayOpponentMinionDefeated(EntityDef minion)
	{
		SetStateOnFsms($"MINION_DEFEAT_{minion.GetCardId().ToUpper()}");
		if (minion.GetRaces().Contains(TAG_RACE.ALL))
		{
			string[] names = Enum.GetNames(typeof(TAG_RACE));
			foreach (string raceName in names)
			{
				SetStateOnFsms($"MINION_DEFEAT_TRIBE_{raceName}");
			}
			return;
		}
		foreach (TAG_RACE race in minion.GetRaces())
		{
			SetStateOnFsms($"MINION_DEFEAT_TRIBE_{Enum.GetName(typeof(TAG_RACE), race)}");
		}
	}

	public void PlayOpponentMinionDefeatedCount(int defeatedCount)
	{
		foreach (PlayMakerFSM fsm in m_BoardStateChangingObjects)
		{
			for (int i = defeatedCount; i > 0; i--)
			{
				string stateName = $"MINION_DEFEAT_COUNT_{i}";
				if (FsmContainsState(fsm, stateName))
				{
					fsm.SetState(stateName);
					break;
				}
			}
		}
	}

	public void CheckForHeroHeavyHitBoardEffects(Card sourceCard, Card targetCard)
	{
		if (IsHeavyHit(sourceCard))
		{
			if (targetCard == targetCard.GetHeroCard())
			{
				SetStateOnFsms("HEAVY_HIT");
			}
			else if (sourceCard.GetEntity().IsControlledByFriendlySidePlayer())
			{
				SetStateOnFsms("MINION_HEAVY_HIT");
			}
		}
	}

	public void CheatTriggerHeroHeavyHitBoardEffects()
	{
		SetStateOnFsms("HEAVY_HIT");
	}

	public void CheatTriggerMinionHeavyHitBoardEffects()
	{
		SetStateOnFsms("MINION_HEAVY_HIT");
	}

	public void CheatTriggerAllBoardEffects()
	{
		foreach (PlayMakerFSM fsm in m_BoardStateChangingObjects)
		{
			if (fsm == null)
			{
				continue;
			}
			FsmState[] fsmStates = fsm.FsmStates;
			foreach (FsmState state in fsmStates)
			{
				if (state.Name != "SHOP" && state.Name != "COMBAT")
				{
					fsm.SetState(state.Name);
				}
			}
		}
	}

	public void CheatTriggerDefeatMinion(string cardID)
	{
		SetStateOnFsms($"MINION_DEFEAT_{cardID.ToUpper()}");
	}

	public void DebugTriggerFSMState(string stateName)
	{
		SetStateOnFsms(stateName);
	}

	private bool IsHeavyHit(Card sourceCard)
	{
		if (sourceCard.GetEntity().IsControlledByOpposingSidePlayer())
		{
			return false;
		}
		if (sourceCard.GetEntity().GetATK() >= m_MinMinionHeavyHitDamage)
		{
			return true;
		}
		return false;
	}

	private bool FsmContainsState(PlayMakerFSM fsm, string stateName)
	{
		FsmState[] fsmStates = fsm.FsmStates;
		foreach (FsmState state in fsmStates)
		{
			if (stateName.Equals(state.Name))
			{
				return true;
			}
		}
		return false;
	}

	private void SetStateOnFsms(string stateName)
	{
		foreach (PlayMakerFSM fsm in m_BoardStateChangingObjects)
		{
			if (FsmContainsState(fsm, stateName))
			{
				fsm.SetState(stateName);
			}
		}
	}

	public void SetLighting()
	{
		if (m_DefaultLightingEnabled)
		{
			Action<object> ambientUpdate = delegate(object amount)
			{
				RenderSettings.ambientLight = (Color)amount;
			};
			Hashtable cArgs = iTweenManager.Get().GetTweenHashTable();
			cArgs.Add("from", RenderSettings.ambientLight);
			cArgs.Add("to", m_AmbientColor);
			cArgs.Add("delay", m_AmbientTransitionDelay);
			cArgs.Add("time", m_AmbientTransitionTime);
			cArgs.Add("easetype", iTween.EaseType.easeInOutQuad);
			cArgs.Add("onupdate", ambientUpdate);
			cArgs.Add("onupdatetarget", base.gameObject);
			iTween.ValueTo(base.gameObject, cArgs);
		}
	}

	public void QueueToUnload(BaconBoard unloadTarget)
	{
		if (m_BoardType == TAG_BOARD_VISUAL_STATE.COMBAT)
		{
			StartCoroutine(QueueToUnloadCoroutine(unloadTarget));
		}
	}

	private IEnumerator QueueToUnloadCoroutine(BaconBoard unloadTarget)
	{
		yield return new WaitForSeconds(m_UnloadDelay);
		unloadTarget.ProcessUnloadRequest(this);
	}

	public void OnValidate()
	{
		if (base.gameObject.activeInHierarchy)
		{
			RenderSettings.ambientLight = m_AmbientColor;
		}
	}

	public void OnEnable()
	{
		if (!Application.isPlaying)
		{
			RenderSettings.ambientLight = m_AmbientColor;
		}
	}
}
