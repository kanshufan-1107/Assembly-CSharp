using System;
using System.Collections.Generic;
using System.Threading;
using Blizzard.T5.Core;
using Blizzard.T5.Core.Utils;
using Blizzard.T5.Game.Spells;
using Blizzard.T5.Game.Spells.SuperSpells;
using Cysharp.Threading.Tasks;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using PegasusGame;
using UnityEngine;

public class Spell : SpellBase, ISpellCallbackHandler<Spell>
{
	private class FinishedListener : EventListener<ISpellCallbackHandler<Spell>.FinishedCallback>
	{
		public void Fire(Spell spell)
		{
			m_callback(spell, m_userData);
		}
	}

	private class StateFinishedListener : EventListener<ISpellCallbackHandler<Spell>.StateFinishedCallback>
	{
		public void Fire(Spell spell, SpellStateType prevStateType)
		{
			m_callback(spell, prevStateType, m_userData);
		}
	}

	private class StateStartedListener : EventListener<ISpellCallbackHandler<Spell>.StateStartedCallback>
	{
		public void Fire(Spell spell, SpellStateType prevStateType)
		{
			m_callback(spell, prevStateType, m_userData);
		}
	}

	private class SpellEventListener : EventListener<ISpellCallbackHandler<Spell>.SpellEventCallback>
	{
		public void Fire(string eventName, object eventData)
		{
			m_callback(eventName, eventData, m_userData);
		}
	}

	private class SpellReleasedListener : EventListener<ISpellCallbackHandler<Spell>.SpellReleasedCallback>
	{
		public void Fire(Spell spell)
		{
			m_callback(spell);
		}
	}

	[UnityEngine.Tooltip("If checked, this spell will block power history processing when the spell leaves the None state.")]
	public bool m_BlockServerEvents;

	[UnityEngine.Tooltip("Additional configuration on when this spell should block power history processing")]
	public PowerProcessorBlockingBehavior m_BlockPowerProcessing;

	public GameObject m_ObjectContainer;

	public TARGET_RETICLE_TYPE m_TargetReticle;

	public List<SpellZoneTag> m_ZonesToDisable;

	[UnityEngine.Tooltip("Delay (in seconds) to wait before sorting a zone after processing entity death. This is often used in CustomDeath spells, in order to wait for the custom death animation to play through before sorting the Play zone.")]
	public float m_ZoneLayoutDelayForDeaths;

	public bool m_UseFastActorTriggers;

	public bool m_ExclusivelyUseMetadataForTargeting;

	private Map<SpellStateType, List<ISpellState>> m_spellStateMap;

	protected SpellStateType m_activeStateType;

	protected SpellStateType m_activeStateChange;

	private readonly List<FinishedListener> m_finishedListeners = new List<FinishedListener>();

	private readonly List<StateFinishedListener> m_stateFinishedListeners = new List<StateFinishedListener>();

	private readonly List<StateStartedListener> m_stateStartedListeners = new List<StateStartedListener>();

	private readonly List<SpellEventListener> m_spellEventListeners = new List<SpellEventListener>();

	private readonly List<SpellReleasedListener> m_spellReleasedListeners = new List<SpellReleasedListener>();

	protected List<GameObject> m_targets = new List<GameObject>();

	protected PowerTaskList m_taskList;

	protected bool m_shown = true;

	protected PlayMakerFSM m_fsm;

	private Map<SpellStateType, FsmState> m_fsmStateMap;

	private bool m_fsmSkippedFirstFrame;

	private bool m_fsmReady;

	protected CancellationTokenSource m_fsmTokenSource;

	protected bool m_positionDirty = true;

	protected bool m_orientationDirty = true;

	protected bool m_finished;

	private TransformProps m_defaultTransformProps;

	public bool IsDestroyed { get; set; }

	protected virtual void Awake()
	{
		BuildSpellStateMap();
		m_fsm = GetComponent<PlayMakerFSM>();
		if (!string.IsNullOrEmpty(base.LocationTransformName))
		{
			base.LocationTransformName = base.LocationTransformName.Trim();
		}
	}

	protected virtual void OnDestroy()
	{
		if (m_ObjectContainer != null)
		{
			UnityEngine.Object.Destroy(m_ObjectContainer);
			m_ObjectContainer = null;
		}
		if (base.gameObject != null)
		{
			StopAllAsyncs();
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	protected virtual void Start()
	{
		if (m_activeStateType == SpellStateType.NONE)
		{
			ActivateObjectContainer(enable: false);
		}
		else if (m_shown)
		{
			ShowImpl();
		}
		else
		{
			HideImpl();
		}
	}

	private void Update()
	{
		if (!m_fsmReady)
		{
			if (m_fsm == null)
			{
				m_fsmReady = true;
			}
			else if (!m_fsmSkippedFirstFrame)
			{
				m_fsmSkippedFirstFrame = true;
			}
			else if (m_fsm.enabled)
			{
				BuildFsmStateMap();
				m_fsmReady = true;
			}
		}
	}

	public PlayMakerFSM GetPlayMaker()
	{
		return m_fsm;
	}

	public SpellType GetSpellType()
	{
		return base.SpellType;
	}

	public void SetSpellType(SpellType spellType)
	{
		base.SpellType = spellType;
	}

	public bool DoesBlockServerEvents()
	{
		if (GameState.Get() == null)
		{
			return false;
		}
		return m_BlockServerEvents;
	}

	public ISuperSpell GetSuperSpellParent()
	{
		if (base.transform.parent == null)
		{
			return null;
		}
		return base.transform.parent.GetComponent<ISuperSpell>();
	}

	public PowerTaskList GetPowerTaskList()
	{
		return m_taskList;
	}

	public Entity GetPowerSource()
	{
		if (m_taskList == null)
		{
			return null;
		}
		return m_taskList.GetSourceEntity();
	}

	public Card GetPowerSourceCard()
	{
		return GetPowerSource()?.GetCard();
	}

	public Entity GetPowerTarget()
	{
		if (m_taskList == null)
		{
			return null;
		}
		return m_taskList.GetTargetEntity();
	}

	public Card GetPowerTargetCard()
	{
		return GetPowerTarget()?.GetCard();
	}

	public virtual bool CanPurge()
	{
		return !IsActive();
	}

	public virtual bool ShouldReconnectIfStuck()
	{
		return true;
	}

	public void UpdateParentActorComponents()
	{
		Transform parent = base.transform.parent;
		if (parent == null)
		{
			return;
		}
		Actor ancestorActor = parent.GetComponent<Actor>();
		if (ancestorActor == null)
		{
			Transform grandparent = parent.parent;
			if (grandparent != null)
			{
				ancestorActor = grandparent.GetComponent<Actor>();
			}
		}
		if (ancestorActor != null)
		{
			ancestorActor.UpdateAllComponents();
		}
	}

	public SpellLocation GetLocation()
	{
		return base.Location;
	}

	public string GetLocationTransformName()
	{
		return base.LocationTransformName;
	}

	public SpellFacing GetFacing()
	{
		return base.Facing;
	}

	public SpellFacingOptions GetFacingOptions()
	{
		return base.FacingOptions;
	}

	public override void ClearPositionDirtyFlag()
	{
		m_positionDirty = false;
	}

	public override void SetPosition(Vector3 position)
	{
		base.transform.position = position;
		m_positionDirty = false;
	}

	public override void SetLocalPosition(Vector3 position)
	{
		base.transform.localPosition = position;
		m_positionDirty = false;
	}

	public override void SetOrientation(Quaternion orientation)
	{
		base.transform.rotation = orientation;
		m_orientationDirty = false;
	}

	public override void SetLocalOrientation(Quaternion orientation)
	{
		base.transform.localRotation = orientation;
		m_orientationDirty = false;
	}

	public override void ForceUpdateTransform()
	{
		m_positionDirty = true;
		UpdateTransform();
	}

	public override void UpdateTransform()
	{
		UpdatePosition();
		UpdateOrientation();
	}

	public override void UpdatePosition()
	{
		if (m_positionDirty)
		{
			SpellUtils.SetPositionFromLocation(this, base.SetParentToLocation);
			m_positionDirty = false;
		}
	}

	public override void UpdateOrientation()
	{
		if (m_orientationDirty)
		{
			SpellUtils.SetOrientationFromFacing(this);
			m_orientationDirty = false;
		}
	}

	public GameObject GetSource()
	{
		return Source;
	}

	public override void SetSource(GameObject go)
	{
		Source = go;
	}

	public override void RemoveSource()
	{
		Source = null;
	}

	public override bool IsSource(GameObject go)
	{
		return Source == go;
	}

	public Card GetSourceCard()
	{
		if (Source == null)
		{
			return null;
		}
		return Source.GetComponent<Card>();
	}

	private void ResetLocalPosition()
	{
		base.transform.localPosition = m_defaultTransformProps.position;
		base.transform.localScale = m_defaultTransformProps.scale;
		base.transform.localRotation = m_defaultTransformProps.rotation;
	}

	public void ReleaseSpell()
	{
		FinishIfNecessary();
		FireSpellReleasedCallbacks();
		ResetSpellStates();
		RemoveSource();
		ClearAllListeners();
		RemoveAllTargets();
		ForceDeactivate();
		ResetLocalPosition();
	}

	private void ResetSpellStates()
	{
		if (m_spellStateMap == null)
		{
			return;
		}
		foreach (KeyValuePair<SpellStateType, List<ISpellState>> item in m_spellStateMap)
		{
			foreach (ISpellState item2 in item.Value)
			{
				item2.Reset();
			}
		}
	}

	public override List<GameObject> GetTargets()
	{
		return m_targets;
	}

	public override GameObject GetTarget()
	{
		if (m_targets.Count != 0)
		{
			return m_targets[0];
		}
		return null;
	}

	public override void AddTarget(GameObject go)
	{
		m_targets.Add(go);
	}

	public override void AddTargets(List<GameObject> targets)
	{
		m_targets.AddRange(targets);
	}

	public override bool RemoveTarget(GameObject go)
	{
		return m_targets.Remove(go);
	}

	public override void RemoveAllTargets()
	{
		m_targets.Clear();
	}

	public override bool IsTarget(GameObject go)
	{
		return m_targets.Contains(go);
	}

	public Card GetTargetCard()
	{
		GameObject target = GetTarget();
		if (target == null)
		{
			return null;
		}
		return target.GetComponent<Card>();
	}

	public override List<GameObject> GetVisualTargets()
	{
		return GetTargets();
	}

	public override GameObject GetVisualTarget()
	{
		return GetTarget();
	}

	public override void AddVisualTarget(GameObject go)
	{
		AddTarget(go);
	}

	public override void AddVisualTargets(List<GameObject> targets)
	{
		AddTargets(targets);
	}

	public override bool RemoveVisualTarget(GameObject go)
	{
		return RemoveTarget(go);
	}

	public override void RemoveAllVisualTargets()
	{
		RemoveAllTargets();
	}

	public override bool IsVisualTarget(GameObject go)
	{
		return IsTarget(go);
	}

	public virtual Card GetVisualTargetCard()
	{
		return GetTargetCard();
	}

	public virtual bool IsValidSpellTarget(Entity ent)
	{
		return !ent.IsEnchantment();
	}

	public bool IsValidSpellTarget(GameObject go)
	{
		if (!go.TryGetComponent<Card>(out var card))
		{
			return false;
		}
		if (card.GetEntity() == null)
		{
			return false;
		}
		return IsValidSpellTarget(go.GetComponent<Card>().GetEntity());
	}

	public override bool IsShown()
	{
		return m_shown;
	}

	public override void Show()
	{
		if (!m_shown)
		{
			m_shown = true;
			if (m_activeStateType != 0)
			{
				OnExitedNoneState();
			}
			ShowImpl();
		}
	}

	public override void Hide()
	{
		if (m_shown)
		{
			m_shown = false;
			HideImpl();
			if (m_activeStateType != 0)
			{
				OnEnteredNoneState();
			}
		}
	}

	public override void ActivateObjectContainer(bool enable)
	{
		if (!(m_ObjectContainer == null))
		{
			RenderUtils.EnableRenderers(m_ObjectContainer, enable);
		}
	}

	public override bool IsActive()
	{
		return m_activeStateType != SpellStateType.NONE;
	}

	public override void Activate()
	{
		SpellStateType stateType = GuessNextStateType();
		if (stateType == SpellStateType.NONE)
		{
			Deactivate();
		}
		else
		{
			ChangeState(stateType);
		}
	}

	public override void Reactivate()
	{
		SpellStateType stateType = GuessNextStateType(SpellStateType.NONE);
		if (stateType == SpellStateType.NONE)
		{
			Deactivate();
		}
		else
		{
			ChangeState(stateType);
		}
	}

	public override void Deactivate()
	{
		if (m_activeStateType != 0)
		{
			ForceDeactivate();
		}
	}

	public override void ForceDeactivate()
	{
		ChangeState(SpellStateType.NONE);
	}

	public override void ActivateState(SpellStateType stateType)
	{
		if (!HasUsableState(stateType))
		{
			Deactivate();
		}
		else
		{
			ChangeState(stateType);
		}
	}

	public override void SafeActivateState(SpellStateType stateType)
	{
		if (!HasUsableState(stateType))
		{
			ForceDeactivate();
		}
		else
		{
			ChangeState(stateType);
		}
	}

	public override bool HasUsableState(SpellStateType stateType)
	{
		if (stateType == SpellStateType.NONE)
		{
			return false;
		}
		if (HasStateContent(stateType))
		{
			return true;
		}
		if (HasOverriddenStateMethod(stateType))
		{
			return true;
		}
		if (m_activeStateType == SpellStateType.NONE && m_ZonesToDisable != null && m_ZonesToDisable.Count > 0)
		{
			return true;
		}
		return false;
	}

	public override SpellStateType GetActiveState()
	{
		return m_activeStateType;
	}

	public List<ISpellState> GetSpellStates(SpellStateType stateType)
	{
		if (m_spellStateMap == null)
		{
			return null;
		}
		if (!m_spellStateMap.TryGetValue(stateType, out var stateList))
		{
			return null;
		}
		return stateList;
	}

	public override List<ISpellState> GetActiveStateList()
	{
		if (m_spellStateMap == null)
		{
			return null;
		}
		if (!m_spellStateMap.TryGetValue(m_activeStateType, out var stateList))
		{
			return null;
		}
		return stateList;
	}

	public override bool IsFinished()
	{
		return m_finished;
	}

	public void AddFinishedCallback(ISpellCallbackHandler<Spell>.FinishedCallback callback)
	{
		AddFinishedCallback(callback, null);
	}

	public void AddFinishedCallback(ISpellCallbackHandler<Spell>.FinishedCallback callback, object userData)
	{
		FinishedListener listener = new FinishedListener();
		listener.SetCallback(callback);
		listener.SetUserData(userData);
		if (!m_finishedListeners.Contains(listener))
		{
			m_finishedListeners.Add(listener);
		}
	}

	public bool RemoveFinishedCallback(ISpellCallbackHandler<Spell>.FinishedCallback callback)
	{
		return RemoveFinishedCallback(callback, null);
	}

	public bool RemoveFinishedCallback(ISpellCallbackHandler<Spell>.FinishedCallback callback, object userData)
	{
		FinishedListener listener = new FinishedListener();
		listener.SetCallback(callback);
		listener.SetUserData(userData);
		return m_finishedListeners.Remove(listener);
	}

	public void AddStateFinishedCallback(ISpellCallbackHandler<Spell>.StateFinishedCallback callback)
	{
		AddStateFinishedCallback(callback, null);
	}

	public void AddStateFinishedCallback(ISpellCallbackHandler<Spell>.StateFinishedCallback callback, object userData)
	{
		StateFinishedListener listener = new StateFinishedListener();
		listener.SetCallback(callback);
		listener.SetUserData(userData);
		if (!m_stateFinishedListeners.Contains(listener))
		{
			m_stateFinishedListeners.Add(listener);
		}
	}

	public bool RemoveStateFinishedCallback(ISpellCallbackHandler<Spell>.StateFinishedCallback callback)
	{
		return RemoveStateFinishedCallback(callback, null);
	}

	public bool RemoveStateFinishedCallback(ISpellCallbackHandler<Spell>.StateFinishedCallback callback, object userData)
	{
		StateFinishedListener listener = new StateFinishedListener();
		listener.SetCallback(callback);
		listener.SetUserData(userData);
		return m_stateFinishedListeners.Remove(listener);
	}

	public void AddStateStartedCallback(ISpellCallbackHandler<Spell>.StateStartedCallback callback)
	{
		AddStateStartedCallback(callback, null);
	}

	public void AddStateStartedCallback(ISpellCallbackHandler<Spell>.StateStartedCallback callback, object userData)
	{
		StateStartedListener listener = new StateStartedListener();
		listener.SetCallback(callback);
		listener.SetUserData(userData);
		if (!m_stateStartedListeners.Contains(listener))
		{
			m_stateStartedListeners.Add(listener);
		}
	}

	public bool RemoveStateStartedCallback(ISpellCallbackHandler<Spell>.StateStartedCallback callback)
	{
		return RemoveStateStartedCallback(callback, null);
	}

	public bool RemoveStateStartedCallback(ISpellCallbackHandler<Spell>.StateStartedCallback callback, object userData)
	{
		StateStartedListener listener = new StateStartedListener();
		listener.SetCallback(callback);
		listener.SetUserData(userData);
		return m_stateStartedListeners.Remove(listener);
	}

	public void AddSpellEventCallback(ISpellCallbackHandler<Spell>.SpellEventCallback callback)
	{
		AddSpellEventCallback(callback, null);
	}

	public void AddSpellEventCallback(ISpellCallbackHandler<Spell>.SpellEventCallback callback, object userData)
	{
		SpellEventListener listener = new SpellEventListener();
		listener.SetCallback(callback);
		listener.SetUserData(userData);
		if (!m_spellEventListeners.Contains(listener))
		{
			m_spellEventListeners.Add(listener);
		}
	}

	public bool RemoveSpellEventCallback(ISpellCallbackHandler<Spell>.SpellEventCallback callback)
	{
		return RemoveSpellEventCallback(callback, null);
	}

	public bool RemoveSpellEventCallback(ISpellCallbackHandler<Spell>.SpellEventCallback callback, object userData)
	{
		SpellEventListener listener = new SpellEventListener();
		listener.SetCallback(callback);
		listener.SetUserData(userData);
		return m_spellEventListeners.Remove(listener);
	}

	public void AddSpellReleasedCallback(ISpellCallbackHandler<Spell>.SpellReleasedCallback callback)
	{
		SpellReleasedListener listener = new SpellReleasedListener();
		listener.SetCallback(callback);
		if (!m_spellReleasedListeners.Contains(listener))
		{
			m_spellReleasedListeners.Add(listener);
		}
	}

	public bool RemoveSpellReleasedCallback(ISpellCallbackHandler<Spell>.SpellReleasedCallback callback)
	{
		SpellReleasedListener listener = new SpellReleasedListener();
		listener.SetCallback(callback);
		return m_spellReleasedListeners.Remove(listener);
	}

	private void ClearAllListeners()
	{
		m_finishedListeners.Clear();
		m_stateFinishedListeners.Clear();
		m_stateStartedListeners.Clear();
		m_spellEventListeners.Clear();
		m_spellReleasedListeners.Clear();
	}

	public override void ChangeState(SpellStateType stateType)
	{
		ChangeStateImpl(stateType);
		if (m_activeStateType == stateType)
		{
			ChangeFsmState(stateType);
		}
	}

	public override SpellStateType GuessNextStateType()
	{
		return GuessNextStateType(m_activeStateType);
	}

	public override SpellStateType GuessNextStateType(SpellStateType stateType)
	{
		switch (stateType)
		{
		case SpellStateType.NONE:
			if (HasUsableState(SpellStateType.BIRTH))
			{
				return SpellStateType.BIRTH;
			}
			if (HasUsableState(SpellStateType.IDLE))
			{
				return SpellStateType.IDLE;
			}
			if (HasUsableState(SpellStateType.ACTION))
			{
				return SpellStateType.ACTION;
			}
			if (HasUsableState(SpellStateType.DEATH))
			{
				return SpellStateType.DEATH;
			}
			if (HasUsableState(SpellStateType.CANCEL))
			{
				return SpellStateType.CANCEL;
			}
			break;
		case SpellStateType.BIRTH:
			if (HasUsableState(SpellStateType.IDLE))
			{
				return SpellStateType.IDLE;
			}
			break;
		case SpellStateType.IDLE:
			if (HasUsableState(SpellStateType.ACTION))
			{
				return SpellStateType.ACTION;
			}
			break;
		case SpellStateType.ACTION:
			if (HasUsableState(SpellStateType.DEATH))
			{
				return SpellStateType.DEATH;
			}
			break;
		}
		return SpellStateType.NONE;
	}

	public virtual bool AttachPowerTaskList(PowerTaskList taskList)
	{
		PowerTaskList oldTaskList = m_taskList;
		m_taskList = taskList;
		RemoveAllTargets();
		if (!AddPowerTargets())
		{
			m_taskList = oldTaskList;
			return false;
		}
		OnAttachPowerTaskList();
		return true;
	}

	public virtual bool AddPowerTargets()
	{
		if (!CanAddPowerTargets())
		{
			return false;
		}
		return AddMultiplePowerTargets();
	}

	public PowerTaskList GetLastHandledTaskList(PowerTaskList taskList)
	{
		if (taskList == null)
		{
			return null;
		}
		Spell clone = UnityEngine.Object.Instantiate(this);
		clone.SetSource(GetSource());
		PowerTaskList lastTaskList = null;
		for (PowerTaskList currTaskList = taskList.GetLast(); currTaskList != null; currTaskList = currTaskList.GetPrevious())
		{
			clone.m_taskList = currTaskList;
			clone.RemoveAllTargets();
			if (clone.AddPowerTargets())
			{
				lastTaskList = currTaskList;
				break;
			}
		}
		UnityEngine.Object.Destroy(clone);
		return lastTaskList;
	}

	public bool IsHandlingLastTaskList()
	{
		return GetLastHandledTaskList(m_taskList) == m_taskList;
	}

	public virtual void OnStateFinished()
	{
		SpellStateType stateType = GuessNextStateType();
		ChangeState(stateType);
	}

	public virtual void OnSpellFinished()
	{
		m_finished = true;
		if (GameState.Get() != null)
		{
			GameState.Get().RemoveServerBlockingSpell(this);
		}
		BlockZones(block: false);
		if (m_UseFastActorTriggers && GameState.Get() != null && IsHandlingLastTaskList())
		{
			GameState.Get().SetUsingFastActorTriggers(enable: false);
		}
		FireFinishedCallbacks();
	}

	public virtual void OnSpellEvent(string eventName, object eventData)
	{
		FireSpellEventCallbacks(eventName, eventData);
	}

	public virtual void OnFsmStateStarted(FsmState state, SpellStateType stateType)
	{
		if (m_activeStateChange != stateType)
		{
			ChangeStateImpl(stateType);
		}
	}

	protected virtual void OnAttachPowerTaskList()
	{
		if (m_UseFastActorTriggers && m_taskList.IsStartOfBlock())
		{
			GameState.Get().SetUsingFastActorTriggers(enable: true);
		}
	}

	protected virtual void OnBirth(SpellStateType prevStateType)
	{
		UpdateTransform();
		FireStateStartedCallbacks(prevStateType);
	}

	protected virtual void OnIdle(SpellStateType prevStateType)
	{
		FireStateStartedCallbacks(prevStateType);
	}

	protected virtual void OnAction(SpellStateType prevStateType)
	{
		UpdateTransform();
		FireStateStartedCallbacks(prevStateType);
	}

	protected virtual void OnCancel(SpellStateType prevStateType)
	{
		FireStateStartedCallbacks(prevStateType);
	}

	protected virtual void OnDeath(SpellStateType prevStateType)
	{
		FireStateStartedCallbacks(prevStateType);
	}

	protected virtual void OnNone(SpellStateType prevStateType)
	{
		FireStateStartedCallbacks(prevStateType);
	}

	private void BuildSpellStateMap()
	{
		foreach (Transform item in base.transform)
		{
			ISpellState spellState = item.gameObject.GetComponent<SpellState>();
			if (spellState == null)
			{
				continue;
			}
			SpellStateType stateType = spellState.Type;
			if (stateType != 0)
			{
				if (m_spellStateMap == null)
				{
					m_spellStateMap = new Map<SpellStateType, List<ISpellState>>();
				}
				if (!m_spellStateMap.TryGetValue(stateType, out var stateList))
				{
					stateList = new List<ISpellState>();
					m_spellStateMap.Add(stateType, stateList);
				}
				stateList.Add(spellState);
			}
		}
	}

	private void BuildFsmStateMap()
	{
		if (m_fsm == null)
		{
			return;
		}
		List<FsmState> spellStates = GenerateSpellFsmStateList();
		if (spellStates.Count > 0)
		{
			m_fsmStateMap = new Map<SpellStateType, FsmState>();
		}
		Map<SpellStateType, int> stateCounts = new Map<SpellStateType, int>();
		foreach (SpellStateType stateType in Enum.GetValues(typeof(SpellStateType)))
		{
			stateCounts[stateType] = 0;
		}
		Map<SpellStateType, int> transitionCounts = new Map<SpellStateType, int>();
		foreach (SpellStateType stateType2 in Enum.GetValues(typeof(SpellStateType)))
		{
			transitionCounts[stateType2] = 0;
		}
		FsmTransition[] fsmGlobalTransitions = m_fsm.FsmGlobalTransitions;
		foreach (FsmTransition transition in fsmGlobalTransitions)
		{
			SpellStateType stateType3;
			try
			{
				stateType3 = EnumUtils.GetEnum<SpellStateType>(transition.EventName);
			}
			catch (ArgumentException)
			{
				continue;
			}
			SpellStateType key = stateType3;
			int value = transitionCounts[key] + 1;
			transitionCounts[key] = value;
			foreach (FsmState state in spellStates)
			{
				if (transition.ToState.Equals(state.Name))
				{
					key = stateType3;
					value = stateCounts[key] + 1;
					stateCounts[key] = value;
					if (!m_fsmStateMap.ContainsKey(stateType3))
					{
						m_fsmStateMap.Add(stateType3, state);
					}
				}
			}
		}
		foreach (KeyValuePair<SpellStateType, int> pair in stateCounts)
		{
			if (pair.Value > 1)
			{
				Debug.LogWarning($"{this}.BuildFsmStateMap() - Found {pair.Value} states for SpellStateType {pair.Key}. There should be 1.");
			}
		}
		foreach (KeyValuePair<SpellStateType, int> pair2 in transitionCounts)
		{
			if (pair2.Value > 1)
			{
				Debug.LogWarning($"{this}.BuildFsmStateMap() - Found {pair2.Value} transitions for SpellStateType {pair2.Key}. There should be 1.");
			}
			if (pair2.Value > 0 && stateCounts[pair2.Key] == 0)
			{
				Debug.LogWarning($"{this}.BuildFsmStateMap() - SpellStateType {pair2.Key} is missing a SpellStateAction.");
			}
		}
		if (m_fsmStateMap != null && m_fsmStateMap.Values.Count == 0)
		{
			m_fsmStateMap = null;
		}
	}

	private List<FsmState> GenerateSpellFsmStateList()
	{
		List<FsmState> states = new List<FsmState>();
		FsmState[] fsmStates = m_fsm.FsmStates;
		foreach (FsmState state in fsmStates)
		{
			SpellStateAction spellStateAction = null;
			int spellStateActionCount = 0;
			for (int j = 0; j < state.Actions.Length; j++)
			{
				if (state.Actions[j] is SpellStateAction currSpellStateAction)
				{
					spellStateActionCount++;
					if (spellStateAction == null)
					{
						spellStateAction = currSpellStateAction;
					}
				}
			}
			if (spellStateAction != null)
			{
				states.Add(state);
			}
			if (spellStateActionCount > 1)
			{
				Debug.LogWarning($"{this}.GenerateSpellFsmStateList() - State \"{state.Name}\" has {spellStateActionCount} SpellStateActions. There should be 1.");
			}
		}
		return states;
	}

	protected void ChangeStateImpl(SpellStateType stateType)
	{
		m_activeStateChange = stateType;
		SpellStateType prevStateType = m_activeStateType;
		m_activeStateType = stateType;
		if (stateType == SpellStateType.NONE)
		{
			FinishIfNecessary();
		}
		List<ISpellState> spellStateList = null;
		if (m_spellStateMap != null)
		{
			m_spellStateMap.TryGetValue(stateType, out spellStateList);
		}
		if (prevStateType != 0)
		{
			if (m_spellStateMap != null && m_spellStateMap.TryGetValue(prevStateType, out var prevSpellStateList))
			{
				foreach (SpellState item in prevSpellStateList)
				{
					item.Stop(spellStateList);
				}
			}
			FireStateFinishedCallbacks(prevStateType);
		}
		else if (stateType != 0)
		{
			m_finished = false;
			OnExitedNoneState();
		}
		if (spellStateList != null)
		{
			foreach (SpellState item2 in spellStateList)
			{
				item2.Play();
			}
		}
		CallStateFunction(prevStateType, stateType);
		if (prevStateType != 0 && stateType == SpellStateType.NONE)
		{
			OnEnteredNoneState();
		}
	}

	protected void ChangeFsmState(SpellStateType stateType)
	{
		if (m_fsm == null)
		{
			return;
		}
		if (!base.gameObject.activeInHierarchy)
		{
			Log.Spells.PrintWarning("Spell.ChangeFsmState() - WARNING gameObject {0} wants to go into state {1} but is inactive!", base.gameObject, stateType);
			return;
		}
		if (m_fsmTokenSource == null)
		{
			m_fsmTokenSource = new CancellationTokenSource();
		}
		WaitThenChangeFsmState(stateType, m_fsmTokenSource.Token).Forget();
	}

	private async UniTaskVoid WaitThenChangeFsmState(SpellStateType stateType, CancellationToken token = default(CancellationToken))
	{
		while (!m_fsmReady)
		{
			await UniTask.Yield(PlayerLoopTiming.Update, token);
		}
		if (m_activeStateType == stateType)
		{
			ChangeFsmStateNow(stateType);
		}
	}

	protected virtual void StopAllAsyncs()
	{
		if (m_fsmTokenSource != null)
		{
			m_fsmTokenSource.Cancel();
			m_fsmTokenSource.Dispose();
			m_fsmTokenSource = null;
		}
	}

	private void ChangeFsmStateNow(SpellStateType stateType)
	{
		if (m_fsmStateMap == null)
		{
			Debug.LogError($"Spell.ChangeFsmStateNow() - stateType {stateType}  was requested but the m_fsmStateMap for {m_fsm.name} is null");
			return;
		}
		FsmState state = null;
		if (m_fsmStateMap.TryGetValue(stateType, out state))
		{
			m_fsm.SendEvent(EnumUtils.GetString(stateType));
		}
	}

	protected void FinishIfNecessary()
	{
		if (!m_finished)
		{
			OnSpellFinished();
		}
	}

	protected void CallStateFunction(SpellStateType prevStateType, SpellStateType stateType)
	{
		switch (stateType)
		{
		case SpellStateType.BIRTH:
			OnBirth(prevStateType);
			break;
		case SpellStateType.IDLE:
			OnIdle(prevStateType);
			break;
		case SpellStateType.ACTION:
			OnAction(prevStateType);
			break;
		case SpellStateType.CANCEL:
			OnCancel(prevStateType);
			break;
		case SpellStateType.DEATH:
			if (m_BlockPowerProcessing.m_OnEnterDeathState)
			{
				GameState.Get().AddServerBlockingSpell(this);
			}
			OnDeath(prevStateType);
			break;
		default:
			OnNone(prevStateType);
			break;
		}
	}

	protected void FireFinishedCallbacks()
	{
		for (int i = m_finishedListeners.Count - 1; i >= 0; i--)
		{
			m_finishedListeners[i].Fire(this);
		}
		m_finishedListeners.Clear();
	}

	protected void FireStateFinishedCallbacks(SpellStateType prevStateType)
	{
		for (int i = m_stateFinishedListeners.Count - 1; i >= 0; i--)
		{
			if (i < m_stateFinishedListeners.Count)
			{
				m_stateFinishedListeners[i].Fire(this, prevStateType);
			}
		}
		if (m_activeStateType == SpellStateType.NONE)
		{
			m_stateFinishedListeners.Clear();
		}
	}

	protected void FireStateStartedCallbacks(SpellStateType prevStateType)
	{
		for (int i = m_stateStartedListeners.Count - 1; i >= 0; i--)
		{
			m_stateStartedListeners[i].Fire(this, prevStateType);
		}
		if (m_activeStateType == SpellStateType.NONE)
		{
			m_stateStartedListeners.Clear();
		}
	}

	protected void FireSpellEventCallbacks(string eventName, object eventData)
	{
		for (int i = m_spellEventListeners.Count - 1; i >= 0; i--)
		{
			m_spellEventListeners[i].Fire(eventName, eventData);
		}
	}

	protected void FireSpellReleasedCallbacks()
	{
		for (int i = m_spellReleasedListeners.Count - 1; i >= 0; i--)
		{
			m_spellReleasedListeners[i].Fire(this);
		}
	}

	protected bool HasStateContent(SpellStateType stateType)
	{
		if (m_spellStateMap != null && m_spellStateMap.ContainsKey(stateType))
		{
			return true;
		}
		if (!m_fsmReady)
		{
			if (m_fsm != null && m_fsm.Fsm.HasEvent(EnumUtils.GetString(stateType)))
			{
				return true;
			}
		}
		else if (m_fsmStateMap != null && m_fsmStateMap.ContainsKey(stateType))
		{
			return true;
		}
		return false;
	}

	protected bool HasOverriddenStateMethod(SpellStateType stateType)
	{
		string methodName = GetStateMethodName(stateType);
		if (methodName == null)
		{
			return false;
		}
		Type type = GetType();
		Type topType = typeof(Spell);
		return GeneralUtils.IsOverriddenMethod(type, topType, methodName);
	}

	protected string GetStateMethodName(SpellStateType stateType)
	{
		return stateType switch
		{
			SpellStateType.BIRTH => "OnBirth", 
			SpellStateType.IDLE => "OnIdle", 
			SpellStateType.ACTION => "OnAction", 
			SpellStateType.CANCEL => "OnCancel", 
			SpellStateType.DEATH => "OnDeath", 
			_ => null, 
		};
	}

	protected bool CanAddPowerTargets()
	{
		return SpellUtils.CanAddPowerTargets(m_taskList);
	}

	protected bool AddSinglePowerTarget()
	{
		Card sourceCard = GetSourceCard();
		if (sourceCard == null)
		{
			Log.Power.PrintWarning("{0}.AddSinglePowerTarget() - a source card was never added", this);
			return false;
		}
		Network.HistBlockStart blockStart = m_taskList.GetBlockStart();
		if (blockStart == null)
		{
			Log.Power.PrintError("{0}.AddSinglePowerTarget() - got a task list with no block start", this);
			return false;
		}
		List<PowerTask> tasks = m_taskList.GetTaskList();
		if (AddSinglePowerTarget_FromBlockStart(blockStart))
		{
			return true;
		}
		if (AddSinglePowerTarget_FromMetaData(tasks))
		{
			return true;
		}
		if (AddSinglePowerTarget_FromAnyPower(sourceCard, tasks))
		{
			return true;
		}
		return false;
	}

	protected bool AddSinglePowerTarget_FromBlockStart(Network.HistBlockStart blockStart)
	{
		Entity targetEntity = GameState.Get().GetEntity(blockStart.Target);
		if (targetEntity == null)
		{
			return false;
		}
		Card targetCard = targetEntity.GetCard();
		if (targetCard == null)
		{
			Log.Power.Print("{0}.AddSinglePowerTarget_FromSourceAction() - FAILED Target {1} in blockStart has no Card", this, blockStart.Target);
			return false;
		}
		AddTarget(targetCard.gameObject);
		return true;
	}

	protected bool AddSinglePowerTarget_FromMetaData(List<PowerTask> tasks)
	{
		GameState state = GameState.Get();
		for (int i = 0; i < tasks.Count; i++)
		{
			Network.PowerHistory power = tasks[i].GetPower();
			if (power.Type != Network.PowerType.META_DATA)
			{
				continue;
			}
			Network.HistMetaData metaData = (Network.HistMetaData)power;
			if (metaData.MetaType != 0)
			{
				continue;
			}
			if (metaData.Info == null || metaData.Info.Count == 0)
			{
				Debug.LogError($"{this}.AddSinglePowerTarget_FromMetaData() - META_DATA at index {i} has no Info");
				continue;
			}
			for (int j = 0; j < metaData.Info.Count; j++)
			{
				Entity targetEntity = state.GetEntity(metaData.Info[j]);
				if (targetEntity == null)
				{
					Debug.LogError($"{this}.AddSinglePowerTarget_FromMetaData() - Entity is null for META_DATA at index {i} Info index {j}");
					continue;
				}
				Card targetCard = targetEntity.GetCard();
				AddTargetFromMetaData(i, targetCard);
				return true;
			}
		}
		return false;
	}

	protected bool AddSinglePowerTarget_FromAnyPower(Card sourceCard, List<PowerTask> tasks)
	{
		for (int i = 0; i < tasks.Count; i++)
		{
			PowerTask task = tasks[i];
			Card targetCard = GetTargetCardFromPowerTask(i, task);
			if (!(targetCard == null) && !(sourceCard == targetCard) && IsValidSpellTarget(targetCard.GetEntity()))
			{
				AddTarget(targetCard.gameObject);
				return true;
			}
		}
		return false;
	}

	protected bool AddMultiplePowerTargets()
	{
		List<PowerTask> tasks = m_taskList.GetTaskList();
		if (AddMultiplePowerTargets_FromMetaData(tasks) || m_ExclusivelyUseMetadataForTargeting)
		{
			return true;
		}
		Card sourceCard = GetSourceCard();
		AddMultiplePowerTargets_FromAnyPower(sourceCard, tasks);
		return true;
	}

	protected bool AddMultiplePowerTargets_FromMetaData(List<PowerTask> tasks)
	{
		int startingTargetCount = m_targets.Count;
		GameState state = GameState.Get();
		for (int i = 0; i < tasks.Count; i++)
		{
			Network.PowerHistory power = tasks[i].GetPower();
			if (power.Type != Network.PowerType.META_DATA)
			{
				continue;
			}
			Network.HistMetaData metaData = (Network.HistMetaData)power;
			if (metaData.MetaType != 0)
			{
				continue;
			}
			if (metaData.Info == null || metaData.Info.Count == 0)
			{
				Debug.LogError($"{this}.AddMultiplePowerTargets_FromMetaData() - META_DATA at index {i} has no Info");
				continue;
			}
			int source = metaData.Data;
			if (source != 0 && (GetSourceCard() == null || GetSourceCard().GetEntity() == null || source != GetSourceCard().GetEntity().GetEntityId()))
			{
				continue;
			}
			for (int j = 0; j < metaData.Info.Count; j++)
			{
				Entity targetEntity = state.GetEntity(metaData.Info[j]);
				if (targetEntity == null)
				{
					Debug.LogError($"{this}.AddMultiplePowerTargets_FromMetaData() - Entity is null for META_DATA at index {i} Info index {j}");
					continue;
				}
				Card targetCard = targetEntity.GetCard();
				AddTargetFromMetaData(i, targetCard);
			}
		}
		return m_targets.Count != startingTargetCount;
	}

	protected void AddMultiplePowerTargets_FromAnyPower(Card sourceCard, List<PowerTask> tasks)
	{
		for (int i = 0; i < tasks.Count; i++)
		{
			PowerTask task = tasks[i];
			Card targetCard = GetTargetCardFromPowerTask(i, task);
			if (!(targetCard == null) && !(sourceCard == targetCard) && !IsTarget(targetCard.gameObject) && IsValidSpellTarget(targetCard.GetEntity()))
			{
				AddTarget(targetCard.gameObject);
			}
		}
	}

	protected virtual Card GetTargetCardFromPowerTask(int index, PowerTask task)
	{
		Network.PowerHistory power = task.GetPower();
		if (power.Type != Network.PowerType.TAG_CHANGE)
		{
			return null;
		}
		Network.HistTagChange tagChange = power as Network.HistTagChange;
		Entity entity = GameState.Get().GetEntity(tagChange.Entity);
		if (entity == null)
		{
			Debug.LogWarning($"{this}.GetTargetCardFromPowerTask() - WARNING trying to target entity with id {tagChange.Entity} but there is no entity with that id");
			return null;
		}
		return entity.GetCard();
	}

	protected virtual void AddTargetFromMetaData(int metaDataIndex, Card targetCard)
	{
		AddTarget(targetCard.gameObject);
	}

	protected bool CompleteMetaDataTasks(int metaDataIndex)
	{
		return CompleteMetaDataTasks(metaDataIndex, null, null);
	}

	protected bool CompleteMetaDataTasks(int metaDataIndex, PowerTaskList.CompleteCallback completeCallback)
	{
		return CompleteMetaDataTasks(metaDataIndex, completeCallback, null);
	}

	protected bool CompleteMetaDataTasks(int metaDataIndex, PowerTaskList.CompleteCallback completeCallback, object callbackData)
	{
		List<PowerTask> tasks = m_taskList.GetTaskList();
		int count = 1;
		for (int i = metaDataIndex + 1; i < tasks.Count; i++)
		{
			Network.PowerHistory power = tasks[i].GetPower();
			if (power.Type == Network.PowerType.META_DATA && ((Network.HistMetaData)power).MetaType == HistoryMeta.Type.TARGET)
			{
				break;
			}
			count++;
		}
		if (count == 0)
		{
			Debug.LogError($"{this}.CompleteMetaDataTasks() - there are no tasks to complete for meta data {metaDataIndex}");
			return false;
		}
		m_taskList.DoTasks(metaDataIndex, count, completeCallback, callbackData);
		return true;
	}

	protected virtual void ShowImpl()
	{
		List<ISpellState> activeStateList = GetActiveStateList();
		if (activeStateList == null)
		{
			return;
		}
		foreach (ISpellState item in activeStateList)
		{
			item.ShowState();
		}
	}

	protected virtual void HideImpl()
	{
		List<ISpellState> activeStateList = GetActiveStateList();
		if (activeStateList == null)
		{
			return;
		}
		foreach (ISpellState item in activeStateList)
		{
			item.HideState();
		}
	}

	protected void OnExitedNoneState()
	{
		if (DoesBlockServerEvents())
		{
			GameState.Get().AddServerBlockingSpell(this);
		}
		ActivateObjectContainer(enable: true);
		BlockZones(block: true);
		if (ZoneMgr.Get() != null)
		{
			ZoneMgr.Get().RequestNextDeathBlockLayoutDelaySec(m_ZoneLayoutDelayForDeaths);
		}
	}

	protected void OnEnteredNoneState()
	{
		if (GameState.Get() != null)
		{
			GameState.Get().RemoveServerBlockingSpell(this);
		}
		ActivateObjectContainer(enable: false);
	}

	protected void BlockZones(bool block)
	{
		if (m_ZonesToDisable == null)
		{
			return;
		}
		foreach (SpellZoneTag item in m_ZonesToDisable)
		{
			List<Zone> zones = SpellUtils.FindZonesFromTag(item);
			if (zones == null)
			{
				continue;
			}
			foreach (Zone item2 in zones)
			{
				item2.BlockInput(block);
			}
		}
	}

	public void OnLoad()
	{
		foreach (Transform item in base.transform)
		{
			SpellState spellState = item.gameObject.GetComponent<SpellState>();
			if (!(spellState == null))
			{
				spellState.OnLoad();
			}
		}
	}
}
