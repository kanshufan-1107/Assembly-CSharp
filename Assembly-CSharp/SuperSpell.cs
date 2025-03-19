using System;
using System.Collections;
using System.Collections.Generic;
using Blizzard.T5.Core;
using Blizzard.T5.Core.Utils;
using Blizzard.T5.Game.Cutscene;
using Blizzard.T5.Game.Spells;
using Blizzard.T5.Game.Spells.SuperSpells;
using HutongGames.PlayMaker;
using PegasusGame;
using UnityEngine;

public class SuperSpell : Spell, ISuperSpell, ISpell, ICutsceneActionListener
{
	public bool m_MakeClones = true;

	[UnityEngine.Tooltip("If used as a subspell, setting this to true will skip auto-cleanup done by the SubspellController. Do not use unless you're sure you want this effect to stay around until the scene is cleaned up!")]
	public bool m_SkipAutoDestroyForSubspell;

	[SerializeField]
	private SpellTargetInfo m_TargetInfo = new SpellTargetInfo();

	[SerializeField]
	private SpellStartInfo m_StartInfo;

	[SerializeField]
	private SpellActionInfo m_ActionInfo;

	[SerializeField]
	private SpellMissileInfo m_MissileInfo;

	[SerializeField]
	private SpellImpactInfo m_ImpactInfo;

	[SerializeField]
	private SpellAreaEffectInfo m_FriendlyAreaEffectInfo;

	[SerializeField]
	private SpellAreaEffectInfo m_OpponentAreaEffectInfo;

	[HideInInspector]
	public SpellChainInfo m_ChainInfo;

	protected ISpell m_startSpell;

	protected List<GameObject> m_visualTargets = new List<GameObject>();

	protected int m_currentTargetIndex;

	protected int m_effectsPendingFinish;

	protected bool m_pendingNoneStateChange;

	protected bool m_pendingSpellFinish;

	protected List<Spell> m_activeClonedSpells = new List<Spell>();

	protected Map<int, int> m_visualToTargetIndexMap = new Map<int, int>();

	protected Map<int, int> m_targetToMetaDataMap = new Map<int, int>();

	protected bool m_settingUpAction;

	protected Spell m_activeAreaEffectSpell;

	private readonly List<int> m_createCardsInSetAside = new List<int>();

	public ISpellTargetInfo TargetInfo => m_TargetInfo;

	public ISpellStartInfo StartInfo => m_StartInfo;

	public ISpellMissileInfo MissileInfo => m_MissileInfo;

	public ISpellActionInfo ActionInfo => m_ActionInfo;

	public ISpellImpactInfo ImpactInfo => m_ImpactInfo;

	public ISpellAreaEffectInfo FriendlyAreaEffectInfo => m_FriendlyAreaEffectInfo;

	public ISpellAreaEffectInfo OpponentAreaEffectInfo => m_OpponentAreaEffectInfo;

	protected Action<Spell> OnBeforeActivateAreaEffectSpell { get; set; }

	public override List<GameObject> GetVisualTargets()
	{
		return m_visualTargets;
	}

	public override GameObject GetVisualTarget()
	{
		if (m_visualTargets.Count != 0)
		{
			return m_visualTargets[0];
		}
		return null;
	}

	public override void AddVisualTarget(GameObject go)
	{
		m_visualTargets.Add(go);
	}

	public override void AddVisualTargets(List<GameObject> targets)
	{
		m_visualTargets.AddRange(targets);
	}

	public override bool RemoveVisualTarget(GameObject go)
	{
		return m_visualTargets.Remove(go);
	}

	public override void RemoveAllVisualTargets()
	{
		m_visualTargets.Clear();
	}

	public override bool IsVisualTarget(GameObject go)
	{
		return m_visualTargets.Contains(go);
	}

	public override Card GetVisualTargetCard()
	{
		GameObject target = GetVisualTarget();
		if (target == null)
		{
			return null;
		}
		return target.GetComponent<Card>();
	}

	public virtual void RemoveImpactInfo()
	{
		m_ImpactInfo = null;
	}

	protected bool AddPowerTargetsInternal(bool fallbackToStartBlockTarget)
	{
		m_visualToTargetIndexMap.Clear();
		m_targetToMetaDataMap.Clear();
		if (!CanAddPowerTargets())
		{
			return false;
		}
		if (HasChain() && !AddPrimaryChainTarget())
		{
			return false;
		}
		if (!AddMultiplePowerTargets())
		{
			return false;
		}
		if (m_targets.Count > 0)
		{
			return true;
		}
		if (!fallbackToStartBlockTarget)
		{
			return true;
		}
		Network.HistBlockStart blockStart = m_taskList.GetBlockStart();
		if (blockStart == null || blockStart.Target == 0)
		{
			return true;
		}
		return AddSinglePowerTarget_FromBlockStart(blockStart);
	}

	public override bool AddPowerTargets()
	{
		return AddPowerTargetsInternal(fallbackToStartBlockTarget: true);
	}

	protected override void AddTargetFromMetaData(int metaDataIndex, Card targetCard)
	{
		int currTargetIndex = m_targets.Count;
		m_targetToMetaDataMap[currTargetIndex] = metaDataIndex;
		AddTarget(targetCard.gameObject);
	}

	protected override void OnBirth(SpellStateType prevStateType)
	{
		UpdatePosition();
		UpdateOrientation();
		m_currentTargetIndex = 0;
		if (HasStart())
		{
			SpawnStart();
			m_startSpell.SafeActivateState(SpellStateType.BIRTH);
			if (m_startSpell.GetActiveState() == SpellStateType.NONE)
			{
				m_startSpell = null;
			}
		}
		base.OnBirth(prevStateType);
	}

	protected override void OnAction(SpellStateType prevStateType)
	{
		m_settingUpAction = true;
		UpdateTargets();
		if (base.Location == SpellLocation.CHOSEN_TARGET)
		{
			m_positionDirty = true;
		}
		UpdatePosition();
		if (base.Facing == SpellFacing.TOWARDS_CHOSEN_TARGET)
		{
			m_orientationDirty = true;
		}
		UpdateOrientation();
		m_currentTargetIndex = GetPrimaryTargetIndex();
		UpdatePendingStateChangeFlags(SpellStateType.ACTION);
		DoAction();
		base.OnAction(prevStateType);
		m_settingUpAction = false;
		FinishIfPossible();
	}

	protected override void OnCancel(SpellStateType prevStateType)
	{
		UpdatePendingStateChangeFlags(SpellStateType.CANCEL);
		if (m_startSpell != null)
		{
			m_startSpell.SafeActivateState(SpellStateType.CANCEL);
			m_startSpell = null;
		}
		base.OnCancel(prevStateType);
		FinishIfPossible();
	}

	public override void OnStateFinished()
	{
		if (GuessNextStateType() == SpellStateType.NONE && AreEffectsActive())
		{
			m_pendingNoneStateChange = true;
		}
		else
		{
			base.OnStateFinished();
		}
	}

	public override void OnSpellFinished()
	{
		if (AreEffectsActive())
		{
			m_pendingSpellFinish = true;
		}
		else
		{
			base.OnSpellFinished();
		}
	}

	public override void OnFsmStateStarted(FsmState state, SpellStateType stateType)
	{
		if (m_activeStateChange != stateType)
		{
			if (stateType == SpellStateType.NONE && AreEffectsActive())
			{
				m_pendingSpellFinish = true;
				m_pendingNoneStateChange = true;
			}
			else
			{
				base.OnFsmStateStarted(state, stateType);
			}
		}
	}

	public override bool CanPurge()
	{
		if (m_activeClonedSpells.Count > 0)
		{
			return false;
		}
		return base.CanPurge();
	}

	public void PurgeImmediate()
	{
		foreach (Spell target in m_activeClonedSpells)
		{
			if (target != null)
			{
				UnityEngine.Object.Destroy(target.gameObject);
			}
		}
	}

	public void ActivateFinisher(bool opponentFinisher = false)
	{
		m_ImpactInfo.AdjustRotation = opponentFinisher;
		m_StartInfo.AdjustRotation = opponentFinisher;
		Activate();
	}

	private void DoAction()
	{
		if (!CheckAndWaitForGameEventsThenDoAction() && !CheckAndWaitForStartDelayThenDoAction() && !CheckAndWaitForStartPrefabThenDoAction())
		{
			DoActionNow();
		}
	}

	private bool CheckAndWaitForGameEventsThenDoAction()
	{
		if (m_taskList == null)
		{
			return false;
		}
		if (m_ActionInfo.ShowSpellVisuals == SpellVisualShowTime.DURING_GAME_EVENTS)
		{
			return DoActionDuringGameEvents();
		}
		if (m_ActionInfo.ShowSpellVisuals == SpellVisualShowTime.AFTER_GAME_EVENTS)
		{
			DoActionAfterGameEvents();
			return true;
		}
		return false;
	}

	private bool DoActionDuringGameEvents()
	{
		m_taskList.DoAllTasks();
		if (m_taskList.IsComplete())
		{
			return false;
		}
		QueueList<PowerTask> tasks = DetermineTasksToWaitFor(0, m_taskList.GetTaskList().Count);
		if (tasks.Count == 0)
		{
			return false;
		}
		StartCoroutine(DoDelayedActionDuringGameEvents(tasks));
		return true;
	}

	private IEnumerator DoDelayedActionDuringGameEvents(QueueList<PowerTask> tasksToWaitFor)
	{
		m_effectsPendingFinish++;
		yield return StartCoroutine(WaitForTasks(tasksToWaitFor));
		m_effectsPendingFinish--;
		if (!CheckAndWaitForStartDelayThenDoAction() && !CheckAndWaitForStartPrefabThenDoAction())
		{
			DoActionNow();
		}
	}

	private Entity GetEntityFromZoneChangePowerTask(PowerTask task)
	{
		GetZoneChangeFromPowerTask(task, out var entity, out var _);
		return entity;
	}

	private bool GetZoneChangeFromPowerTask(PowerTask task, out Entity entity, out int zoneTag)
	{
		entity = null;
		zoneTag = 0;
		Entity tempEntity = null;
		Network.PowerHistory power = task.GetPower();
		switch (power.Type)
		{
		case Network.PowerType.FULL_ENTITY:
		{
			Network.HistFullEntity fullEntity = (Network.HistFullEntity)power;
			tempEntity = GameState.Get().GetEntity(fullEntity.Entity.ID);
			if (tempEntity == null || tempEntity.GetCard() == null)
			{
				return false;
			}
			foreach (Network.Entity.Tag tag in fullEntity.Entity.Tags)
			{
				if (tag.Name == 49)
				{
					entity = tempEntity;
					zoneTag = tag.Value;
					return true;
				}
			}
			break;
		}
		case Network.PowerType.SHOW_ENTITY:
		{
			Network.HistShowEntity showEntity = (Network.HistShowEntity)power;
			tempEntity = GameState.Get().GetEntity(showEntity.Entity.ID);
			if (tempEntity == null || tempEntity.GetCard() == null)
			{
				return false;
			}
			foreach (Network.Entity.Tag tag2 in showEntity.Entity.Tags)
			{
				if (tag2.Name == 49)
				{
					entity = tempEntity;
					zoneTag = tag2.Value;
					return true;
				}
			}
			break;
		}
		case Network.PowerType.TAG_CHANGE:
		{
			Network.HistTagChange tagChange = (Network.HistTagChange)power;
			tempEntity = GameState.Get().GetEntity(tagChange.Entity);
			if (tempEntity == null || tempEntity.GetCard() == null)
			{
				return false;
			}
			if (tagChange.Tag == 49)
			{
				entity = tempEntity;
				zoneTag = tagChange.Value;
				return true;
			}
			break;
		}
		}
		return false;
	}

	private void DoActionAfterGameEvents()
	{
		m_effectsPendingFinish++;
		PowerTaskList.CompleteCallback onTasksComplete = delegate
		{
			m_effectsPendingFinish--;
			if (!CheckAndWaitForStartDelayThenDoAction() && !CheckAndWaitForStartPrefabThenDoAction())
			{
				DoActionNow();
			}
		};
		m_taskList.DoAllTasks(onTasksComplete);
	}

	private bool CheckAndWaitForStartDelayThenDoAction()
	{
		if (Mathf.Min(m_ActionInfo.StartDelayMax, m_ActionInfo.StartDelayMin) <= Mathf.Epsilon)
		{
			return false;
		}
		m_effectsPendingFinish++;
		StartCoroutine(WaitForStartDelayThenDoAction());
		return true;
	}

	private IEnumerator WaitForStartDelayThenDoAction()
	{
		float delaySec = UnityEngine.Random.Range(m_ActionInfo.StartDelayMin, m_ActionInfo.StartDelayMax);
		yield return new WaitForSeconds(delaySec);
		m_effectsPendingFinish--;
		if (!CheckAndWaitForStartPrefabThenDoAction())
		{
			DoActionNow();
		}
	}

	private bool CheckAndWaitForStartPrefabThenDoAction()
	{
		if (!HasStart())
		{
			return false;
		}
		if (m_startSpell != null && m_startSpell.GetActiveState() == SpellStateType.IDLE)
		{
			return false;
		}
		if (m_startSpell == null)
		{
			SpawnStart();
		}
		if (m_startSpell is Spell spell)
		{
			spell.AddStateFinishedCallback(OnStartSpellBirthStateFinished);
		}
		if (m_startSpell.GetActiveState() != SpellStateType.BIRTH)
		{
			m_startSpell.SafeActivateState(SpellStateType.BIRTH);
			if (m_startSpell.GetActiveState() == SpellStateType.NONE)
			{
				m_startSpell = null;
				return false;
			}
		}
		return true;
	}

	private void OnStartSpellBirthStateFinished(Spell spell, SpellStateType prevStateType, object userData)
	{
		if (prevStateType == SpellStateType.BIRTH)
		{
			spell.RemoveStateFinishedCallback(OnStartSpellBirthStateFinished, userData);
			DoActionNow();
		}
	}

	protected virtual void DoActionNow()
	{
		ISpellAreaEffectInfo info = DetermineAreaEffectInfo();
		if (info != null)
		{
			SpawnAreaEffect(info);
		}
		bool hasMissile = HasMissile();
		bool hasImpact = HasImpact();
		bool hasChain = HasChain();
		if (GetVisualTargetCount() > 0 && (hasMissile || hasImpact || hasChain))
		{
			if (hasMissile)
			{
				if (hasChain)
				{
					SpawnChainMissile();
				}
				else if (m_MissileInfo.SpawnInSequence)
				{
					SpawnMissileInSequence();
				}
				else
				{
					SpawnAllMissiles();
				}
			}
			else
			{
				if (hasImpact)
				{
					if (hasChain)
					{
						SpawnImpact(m_currentTargetIndex);
					}
					else
					{
						SpawnAllImpacts();
					}
				}
				if (hasChain)
				{
					SpawnChain();
				}
				DoStartSpellAction();
			}
		}
		else
		{
			DoStartSpellAction();
		}
		FinishIfPossible();
	}

	private bool HasStart()
	{
		if (m_StartInfo != null && m_StartInfo.Enabled)
		{
			return m_StartInfo.Prefab != null;
		}
		return false;
	}

	private void SpawnStart()
	{
		m_effectsPendingFinish++;
		m_startSpell = CloneSpell(m_StartInfo.Prefab, null);
		m_startSpell.SetSource(GetSource());
		m_startSpell.AddTargets(GetTargets());
		if (m_StartInfo.UseSuperSpellLocation)
		{
			m_startSpell.SetPosition(base.transform.position);
		}
		if (m_StartInfo.AdjustRotation && m_StartInfo.StartRotationAdjustment != Vector3.zero)
		{
			m_startSpell.GameObject.transform.Rotate(m_StartInfo.StartRotationAdjustment);
			m_startSpell.UpdateOrientation();
		}
	}

	private void DoStartSpellAction()
	{
		if (m_startSpell == null)
		{
			return;
		}
		if (!m_startSpell.HasUsableState(SpellStateType.ACTION))
		{
			m_startSpell.UpdateTransform();
			m_startSpell.SafeActivateState(SpellStateType.DEATH);
		}
		else
		{
			if (m_startSpell is Spell spell)
			{
				spell.AddFinishedCallback(OnStartSpellActionFinished);
			}
			m_startSpell.ActivateState(SpellStateType.ACTION);
		}
		m_startSpell = null;
	}

	private void OnStartSpellActionFinished(Spell spell, object userData)
	{
		if (spell.GetActiveState() == SpellStateType.ACTION)
		{
			spell.SafeActivateState(SpellStateType.DEATH);
		}
	}

	private bool HasMissile()
	{
		if (m_MissileInfo != null && m_MissileInfo.Enabled)
		{
			if (m_MissileInfo.Prefab == null)
			{
				return m_MissileInfo.ReversePrefab != null;
			}
			return true;
		}
		return false;
	}

	private void SpawnChainMissile()
	{
		SpawnMissile(GetPrimaryTargetIndex());
		DoStartSpellAction();
	}

	private void SpawnMissileInSequence()
	{
		if (m_currentTargetIndex >= GetVisualTargetCount())
		{
			return;
		}
		SpawnMissile(m_currentTargetIndex);
		m_currentTargetIndex++;
		if (m_startSpell == null)
		{
			return;
		}
		if (m_StartInfo.DeathAfterAllMissilesFire)
		{
			if (m_currentTargetIndex < GetVisualTargetCount())
			{
				if (m_startSpell.HasUsableState(SpellStateType.ACTION))
				{
					m_startSpell.ActivateState(SpellStateType.ACTION);
				}
			}
			else
			{
				DoStartSpellAction();
			}
		}
		else
		{
			DoStartSpellAction();
		}
	}

	private void SpawnAllMissiles()
	{
		for (int i = 0; i < GetVisualTargetCount(); i++)
		{
			SpawnMissile(i);
		}
		DoStartSpellAction();
	}

	private void SpawnMissile(int targetIndex)
	{
		m_effectsPendingFinish++;
		StartCoroutine(WaitAndSpawnMissile(targetIndex));
	}

	private IEnumerator WaitAndSpawnMissile(int targetIndex)
	{
		float spawnDelaySec = UnityEngine.Random.Range(m_MissileInfo.SpawnDelaySecMin, m_MissileInfo.SpawnDelaySecMax);
		if (!m_MissileInfo.SpawnInSequence || targetIndex == 0)
		{
			yield return new WaitForSeconds(spawnDelaySec);
		}
		if (m_MissileInfo.SpawnOffset > 0f && targetIndex > 0)
		{
			yield return new WaitForSeconds(m_MissileInfo.SpawnOffset * (float)targetIndex);
		}
		int metaDataIndex = GetMetaDataIndexForTarget(targetIndex);
		if (ShouldCompleteTasksUntilMetaData(metaDataIndex))
		{
			yield return StartCoroutine(CompleteTasksUntilMetaData(metaDataIndex));
		}
		if (m_visualTargets.Count <= targetIndex || m_visualTargets[targetIndex] == null)
		{
			m_effectsPendingFinish--;
			yield break;
		}
		GameObject sourceObject = GetSource();
		GameObject targetObject = m_visualTargets[targetIndex];
		if (m_MissileInfo.Prefab != null)
		{
			ISpell missile;
			if (m_MissileInfo.UseSuperSpellLocation)
			{
				missile = CloneSpell(m_MissileInfo.Prefab, base.transform.position);
				missile.ClearPositionDirtyFlag();
			}
			else
			{
				missile = CloneSpell(m_MissileInfo.Prefab, null);
			}
			missile.SetSource(sourceObject);
			missile.AddTarget(targetObject);
			if (missile is Spell spell)
			{
				spell.AddStateFinishedCallback(OnMissileSpellStateFinished, targetIndex);
			}
			missile.ActivateState(SpellStateType.BIRTH);
		}
		else
		{
			m_effectsPendingFinish--;
		}
		if (m_MissileInfo.ReversePrefab != null)
		{
			m_effectsPendingFinish++;
			StartCoroutine(SpawnReverseMissile(m_MissileInfo.ReversePrefab, sourceObject, targetObject, m_MissileInfo.ReverseDelay));
		}
	}

	private IEnumerator SpawnReverseMissile(ISpell cloneSpell, GameObject sourceObject, GameObject targetObject, float delay)
	{
		yield return new WaitForSeconds(delay);
		ISpell spell = CloneSpell(cloneSpell, null);
		spell.SetSource(targetObject);
		spell.AddTarget(sourceObject);
		if (spell is Spell spell2)
		{
			spell2.AddStateFinishedCallback(OnMissileSpellStateFinished, -1);
		}
		spell.ActivateState(SpellStateType.BIRTH);
	}

	private void OnMissileSpellStateFinished(Spell spell, SpellStateType prevStateType, object userData)
	{
		if (prevStateType == SpellStateType.BIRTH)
		{
			spell.RemoveStateFinishedCallback(OnMissileSpellStateFinished, userData);
			int targetIndex = (int)userData;
			bool reverse = targetIndex < 0;
			FireMissileOnPath(spell, targetIndex, reverse);
		}
	}

	private void FireMissileOnPath(Spell missile, int targetIndex, bool reverse)
	{
		Vector3[] path = GenerateMissilePath(missile);
		float durationSec = UnityEngine.Random.Range(m_MissileInfo.PathDurationMin, m_MissileInfo.PathDurationMax);
		Hashtable moveArgs = iTweenManager.Get().GetTweenHashTable();
		moveArgs.Add("path", path);
		moveArgs.Add("time", durationSec);
		moveArgs.Add("easetype", m_MissileInfo.PathEaseType);
		moveArgs.Add("oncompletetarget", base.gameObject);
		if (reverse)
		{
			moveArgs.Add("oncomplete", "OnReverseMissileTargetReached");
			moveArgs.Add("oncompleteparams", missile);
		}
		else
		{
			Hashtable completeArgs = new Hashtable(2);
			completeArgs.Add("missile", missile);
			completeArgs.Add("targetindex", targetIndex);
			moveArgs.Add("oncomplete", "OnMissileTargetReached");
			moveArgs.Add("oncompleteparams", completeArgs);
		}
		if (!object.Equals(path[0], path[2]))
		{
			moveArgs.Add("orienttopath", m_MissileInfo.OrientToPath);
		}
		if (m_MissileInfo.TargetJoint.Length > 0)
		{
			GameObject targetJoint = GameObjectUtils.FindChildBySubstring(missile.gameObject, m_MissileInfo.TargetJoint);
			if (targetJoint != null)
			{
				missile.transform.LookAt(missile.GetTarget().transform, m_MissileInfo.JointUpVector);
				path[2].y += m_MissileInfo.TargetHeightOffset;
				iTween.MoveTo(targetJoint, moveArgs);
				return;
			}
		}
		iTween.MoveTo(missile.gameObject, moveArgs);
	}

	private Vector3[] GenerateMissilePath(Spell missile)
	{
		Vector3[] path = new Vector3[3]
		{
			missile.transform.position,
			default(Vector3),
			default(Vector3)
		};
		Card targetCard = missile.GetTargetCard();
		if (targetCard != null && targetCard.GetZone() is ZoneHand && !m_MissileInfo.UseTargetCardPositionInsteadOfHandSlot)
		{
			ZoneHand handZone = targetCard.GetZone() as ZoneHand;
			path[2] = handZone.GetCardPosition(handZone.GetCardSlot(targetCard), -1);
		}
		else
		{
			path[2] = missile.GetTarget().transform.position;
		}
		path[1] = GenerateMissilePathCenterPoint(path);
		return path;
	}

	private Vector3 GenerateMissilePathCenterPoint(Vector3[] path)
	{
		Vector3 sp = path[0];
		Vector3 ep = path[2];
		Vector3 v = ep - sp;
		float magnitude = v.magnitude;
		Vector3 cp = sp;
		bool startIsEnd = magnitude <= Mathf.Epsilon;
		if (!startIsEnd)
		{
			cp = sp + v * (m_MissileInfo.CenterOffsetPercent * 0.01f);
		}
		float scaleFactor = magnitude / m_MissileInfo.DistanceScaleFactor;
		if (startIsEnd)
		{
			if (m_MissileInfo.CenterPointHeightMin <= Mathf.Epsilon && m_MissileInfo.CenterPointHeightMax <= Mathf.Epsilon)
			{
				cp.y += 2f;
			}
			else
			{
				cp.y += UnityEngine.Random.Range(m_MissileInfo.CenterPointHeightMin, m_MissileInfo.CenterPointHeightMax);
			}
		}
		else
		{
			cp.y += scaleFactor * UnityEngine.Random.Range(m_MissileInfo.CenterPointHeightMin, m_MissileInfo.CenterPointHeightMax);
		}
		float flipSides = 1f;
		if (sp.z > ep.z)
		{
			flipSides = -1f;
		}
		bool rightSide = GeneralUtils.RandomBool();
		if (m_MissileInfo.RightMin == 0f && m_MissileInfo.RightMax == 0f)
		{
			rightSide = false;
		}
		if (m_MissileInfo.LeftMin == 0f && m_MissileInfo.LeftMax == 0f)
		{
			rightSide = true;
		}
		if (rightSide)
		{
			if (m_MissileInfo.RightMin == m_MissileInfo.RightMax || m_MissileInfo.DebugForceMax)
			{
				cp.x += m_MissileInfo.RightMax * scaleFactor * flipSides;
			}
			else
			{
				cp.x += UnityEngine.Random.Range(m_MissileInfo.RightMin * scaleFactor, m_MissileInfo.RightMax * scaleFactor) * flipSides;
			}
		}
		else if (m_MissileInfo.LeftMin == m_MissileInfo.LeftMax || m_MissileInfo.DebugForceMax)
		{
			cp.x -= m_MissileInfo.LeftMax * scaleFactor * flipSides;
		}
		else
		{
			cp.x -= UnityEngine.Random.Range(m_MissileInfo.LeftMin * scaleFactor, m_MissileInfo.LeftMax * scaleFactor) * flipSides;
		}
		return cp;
	}

	private void OnMissileTargetReached(Hashtable args)
	{
		Spell obj = (Spell)args["missile"];
		int targetIndex = (int)args["targetindex"];
		if (HasImpact())
		{
			SpawnImpact(targetIndex);
		}
		if (HasChain())
		{
			SpawnChain();
		}
		else if (m_MissileInfo.SpawnInSequence)
		{
			SpawnMissileInSequence();
		}
		obj.ActivateState(SpellStateType.DEATH);
	}

	private void OnReverseMissileTargetReached(Spell missile)
	{
		missile.ActivateState(SpellStateType.DEATH);
	}

	private bool HasImpact()
	{
		if (m_ImpactInfo != null && m_ImpactInfo.Enabled)
		{
			if (m_ImpactInfo.Prefab == null)
			{
				return m_ImpactInfo.DamageAmountImpactSpells.Length != 0;
			}
			return true;
		}
		return false;
	}

	private void SpawnAllImpacts()
	{
		for (int i = 0; i < GetVisualTargetCount(); i++)
		{
			if (IsValidSpellTarget(m_visualTargets[i]))
			{
				SpawnImpact(i);
			}
		}
	}

	private void SpawnImpact(int targetIndex)
	{
		m_effectsPendingFinish++;
		StartCoroutine(WaitAndSpawnImpact(targetIndex));
	}

	private IEnumerator WaitAndSpawnImpact(int targetIndex)
	{
		float spawnDelaySec = UnityEngine.Random.Range(m_ImpactInfo.SpawnDelaySecMin, m_ImpactInfo.SpawnDelaySecMax);
		yield return new WaitForSeconds(spawnDelaySec);
		if (m_ImpactInfo.SpawnOffset > 0f && targetIndex > 0)
		{
			yield return new WaitForSeconds(m_ImpactInfo.SpawnOffset * (float)targetIndex);
		}
		int metaDataIndex = GetMetaDataIndexForTarget(targetIndex);
		if (metaDataIndex >= 0)
		{
			if (ShouldCompleteTasksUntilMetaData(metaDataIndex))
			{
				yield return StartCoroutine(CompleteTasksUntilMetaData(metaDataIndex));
			}
			float gameDelaySec = UnityEngine.Random.Range(m_ImpactInfo.GameDelaySecMin, m_ImpactInfo.GameDelaySecMax);
			StartCoroutine(CompleteTasksFromMetaData(metaDataIndex, gameDelaySec));
		}
		if (m_visualTargets.Count <= targetIndex || m_visualTargets[targetIndex] == null)
		{
			m_effectsPendingFinish--;
			yield break;
		}
		GameObject sourceObject = GetSource();
		GameObject targetObject = m_visualTargets[targetIndex];
		ISpell impactPrefab = DetermineImpactPrefab(targetObject);
		ISpell impact = CloneSpell(impactPrefab, null);
		impact.SetSource(sourceObject);
		impact.AddTarget(targetObject);
		if (m_ImpactInfo.UseSuperSpellLocation)
		{
			impact.SetPosition(base.transform.position);
		}
		else
		{
			if (IsMakingClones())
			{
				impact.Location = m_ImpactInfo.Location;
				impact.SetParentToLocation = m_ImpactInfo.SetParentToLocation;
			}
			impact.UpdatePosition();
			if (m_ImpactInfo.AdjustRotation && m_ImpactInfo.ImpactRotationAdjustment != Vector3.zero)
			{
				impact.GameObject.transform.Rotate(m_ImpactInfo.ImpactRotationAdjustment);
			}
		}
		impact.UpdateOrientation();
		impact.Activate();
	}

	private ISpell DetermineImpactPrefab(GameObject targetObject)
	{
		if (m_ImpactInfo.DamageAmountImpactSpells.Length == 0)
		{
			return m_ImpactInfo.Prefab;
		}
		ISpell impactPrefab = m_ImpactInfo.Prefab;
		if (m_taskList == null)
		{
			return impactPrefab;
		}
		Card targetCard = targetObject.GetComponent<Card>();
		if (targetCard == null)
		{
			return impactPrefab;
		}
		PowerTaskList.DamageInfo damageInfo = m_taskList.GetDamageInfo(targetCard.GetEntity());
		if (damageInfo == null)
		{
			return impactPrefab;
		}
		ISpellValueRange prefabToUse = SpellUtils.GetAppropriateElementAccordingToRanges(m_ImpactInfo.DamageAmountImpactSpells, (ISpellValueRange x) => x.Range, damageInfo.m_damage);
		if (prefabToUse != null && prefabToUse.SpellPrefab != null)
		{
			impactPrefab = prefabToUse.SpellPrefab;
		}
		return impactPrefab;
	}

	private bool HasChain()
	{
		if (m_ChainInfo != null && m_ChainInfo.Enabled)
		{
			return m_ChainInfo.Prefab != null;
		}
		return false;
	}

	private void SpawnChain()
	{
		if (GetVisualTargetCount() > 1)
		{
			m_effectsPendingFinish++;
			StartCoroutine(WaitAndSpawnChain());
		}
	}

	private IEnumerator WaitAndSpawnChain()
	{
		float spawnDelaySec = UnityEngine.Random.Range(m_ChainInfo.SpawnDelayMin, m_ChainInfo.SpawnDelayMax);
		yield return new WaitForSeconds(spawnDelaySec);
		ISpell chain = CloneSpell(m_ChainInfo.Prefab, null);
		GameObject sourceObject = GetPrimaryTarget();
		chain.SetSource(sourceObject);
		foreach (GameObject targetObject in m_visualTargets)
		{
			if (!(targetObject == sourceObject))
			{
				chain.AddTarget(targetObject);
			}
		}
		chain.ActivateState(SpellStateType.ACTION);
	}

	public Spell GetActiveAreaEffectSpell()
	{
		return m_activeAreaEffectSpell;
	}

	private ISpellAreaEffectInfo DetermineAreaEffectInfo()
	{
		Card sourceCard = GetSourceCard();
		if (sourceCard != null)
		{
			Player sourcePlayer = sourceCard.GetController();
			if (sourcePlayer != null)
			{
				if (sourcePlayer.IsFriendlySide() && HasFriendlyAreaEffect())
				{
					return m_FriendlyAreaEffectInfo;
				}
				if (!sourcePlayer.IsFriendlySide() && HasOpponentAreaEffect())
				{
					return m_OpponentAreaEffectInfo;
				}
			}
		}
		if (HasFriendlyAreaEffect())
		{
			return m_FriendlyAreaEffectInfo;
		}
		if (HasOpponentAreaEffect())
		{
			return m_OpponentAreaEffectInfo;
		}
		return null;
	}

	private bool HasAreaEffect()
	{
		if (!HasFriendlyAreaEffect())
		{
			return HasOpponentAreaEffect();
		}
		return true;
	}

	private bool HasFriendlyAreaEffect()
	{
		if (m_FriendlyAreaEffectInfo != null && m_FriendlyAreaEffectInfo.Enabled)
		{
			return m_FriendlyAreaEffectInfo.Prefab != null;
		}
		return false;
	}

	private bool HasOpponentAreaEffect()
	{
		if (m_OpponentAreaEffectInfo != null && m_OpponentAreaEffectInfo.Enabled)
		{
			return m_OpponentAreaEffectInfo.Prefab != null;
		}
		return false;
	}

	private void SpawnAreaEffect(ISpellAreaEffectInfo info)
	{
		m_effectsPendingFinish++;
		StartCoroutine(WaitAndSpawnAreaEffect(info));
	}

	private IEnumerator WaitAndSpawnAreaEffect(ISpellAreaEffectInfo info)
	{
		float spawnDelaySec = UnityEngine.Random.Range(info.SpawnDelaySecMin, info.SpawnDelaySecMax);
		if (spawnDelaySec > 0f)
		{
			yield return new WaitForSeconds(spawnDelaySec);
		}
		Spell areaEffect = CloneSpell(info.Prefab, null) as Spell;
		areaEffect.SetSource(GetSource());
		if (m_taskList != null && (bool)areaEffect)
		{
			areaEffect.AttachPowerTaskList(m_taskList);
		}
		if (info.UseSuperSpellLocation)
		{
			areaEffect.SetPosition(base.transform.position);
		}
		else if (IsMakingClones() && info.Location != 0)
		{
			areaEffect.Location = info.Location;
			areaEffect.SetParentToLocation = info.SetParentToLocation;
			areaEffect.UpdatePosition();
		}
		if (IsMakingClones() && info.Facing != 0)
		{
			areaEffect.Facing = info.Facing;
			areaEffect.FacingOptions = info.FacingOptions;
			areaEffect.UpdateOrientation();
		}
		if (OnBeforeActivateAreaEffectSpell != null)
		{
			OnBeforeActivateAreaEffectSpell(areaEffect);
		}
		areaEffect.Activate();
		m_activeAreaEffectSpell = areaEffect;
	}

	private bool AddPrimaryChainTarget()
	{
		Network.HistBlockStart blockStart = m_taskList.GetBlockStart();
		if (blockStart == null)
		{
			return false;
		}
		return AddSinglePowerTarget_FromBlockStart(blockStart);
	}

	private int GetPrimaryTargetIndex()
	{
		return 0;
	}

	private GameObject GetPrimaryTarget()
	{
		return m_visualTargets[GetPrimaryTargetIndex()];
	}

	protected virtual void UpdateTargets()
	{
		UpdateVisualTargets();
		SuppressPlaySoundsOnVisualTargets();
	}

	private int GetVisualTargetCount()
	{
		if (IsMakingClones())
		{
			return m_visualTargets.Count;
		}
		return Mathf.Min(1, m_visualTargets.Count);
	}

	protected virtual void UpdateVisualTargets()
	{
		switch (m_TargetInfo.Behavior)
		{
		case SpellTargetBehavior.FRIENDLY_PLAY_ZONE_CENTER:
		{
			ZonePlay zonePlay2 = SpellUtils.FindFriendlyPlayZone(this);
			AddVisualTarget(zonePlay2.gameObject);
			return;
		}
		case SpellTargetBehavior.FRIENDLY_PLAY_ZONE_RANDOM:
		{
			ZonePlay zonePlay = SpellUtils.FindFriendlyPlayZone(this);
			GenerateRandomPlayZoneVisualTargets(zonePlay);
			return;
		}
		case SpellTargetBehavior.OPPONENT_PLAY_ZONE_CENTER:
		{
			ZonePlay zonePlay4 = SpellUtils.FindOpponentPlayZone(this);
			AddVisualTarget(zonePlay4.gameObject);
			return;
		}
		case SpellTargetBehavior.OPPONENT_PLAY_ZONE_RANDOM:
		{
			ZonePlay zonePlay3 = SpellUtils.FindOpponentPlayZone(this);
			GenerateRandomPlayZoneVisualTargets(zonePlay3);
			return;
		}
		case SpellTargetBehavior.BOARD_CENTER:
		{
			Board board = Board.Get();
			AddVisualTarget(board.FindBone("CenterPointBone").gameObject);
			return;
		}
		case SpellTargetBehavior.UNTARGETED:
			AddVisualTarget(GetSource());
			return;
		case SpellTargetBehavior.CHOSEN_TARGET_ONLY:
			AddChosenTargetAsVisualTarget();
			return;
		case SpellTargetBehavior.BOARD_RANDOM:
			GenerateRandomBoardVisualTargets();
			return;
		case SpellTargetBehavior.TARGET_ZONE_CENTER:
		{
			Zone zone = SpellUtils.FindTargetZone(this);
			AddVisualTarget(zone.gameObject);
			return;
		}
		case SpellTargetBehavior.NEW_CREATED_CARDS:
			GenerateCreatedCardsTargets();
			return;
		case SpellTargetBehavior.MINIONS_NEWLY_ENTERING_PLAY:
			GenerateCreatedCardsTargets(TAG_ZONE.PLAY, includeZoneChange: true, onlyMinions: true);
			return;
		case SpellTargetBehavior.NEW_CREATED_CARDS_IN_PLAY:
			GenerateCreatedCardsTargets(TAG_ZONE.PLAY);
			return;
		}
		AddAllTargetsAsVisualTargets();
		if (GetVisualTargets().Count == 1 && m_MissileInfo.TimesToHitSameTarget > 1)
		{
			AddSameTargetForAdditionalMissiles();
		}
	}

	protected void GenerateRandomBoardVisualTargets()
	{
		ZonePlay friendlyZonePlay = SpellUtils.FindFriendlyPlayZone(this);
		ZonePlay zonePlay = SpellUtils.FindOpponentPlayZone(this);
		Bounds friendlyBounds = friendlyZonePlay.GetComponent<Collider>().bounds;
		Bounds opponentBounds = zonePlay.GetComponent<Collider>().bounds;
		Vector3 min = Vector3.Min(friendlyBounds.min, opponentBounds.min);
		Vector3 max = Vector3.Max(friendlyBounds.max, opponentBounds.max);
		Vector3 center = 0.5f * (max + min);
		Vector3 minToMax = max - min;
		Vector3 size = new Vector3(Mathf.Abs(minToMax.x), Mathf.Abs(minToMax.y), Mathf.Abs(minToMax.z));
		Bounds bounds = new Bounds(center, size);
		GenerateRandomVisualTargets(bounds);
	}

	protected void GenerateRandomPlayZoneVisualTargets(ZonePlay zonePlay)
	{
		GenerateRandomVisualTargets(zonePlay.GetComponent<Collider>().bounds);
	}

	private void GenerateRandomVisualTargets(Bounds bounds)
	{
		int targetCount = UnityEngine.Random.Range(m_TargetInfo.RandomTargetCountMin, m_TargetInfo.RandomTargetCountMax + 1);
		if (targetCount == 0)
		{
			return;
		}
		float boundsLeft = bounds.min.x;
		float boundsTop = bounds.max.z;
		float boundsBottom = bounds.min.z;
		float boxWidth = bounds.size.x / (float)targetCount;
		int[] boxUsageCounts = new int[targetCount];
		int[] pickableBoxIndexes = new int[targetCount];
		for (int i = 0; i < targetCount; i++)
		{
			boxUsageCounts[i] = 0;
			pickableBoxIndexes[i] = -1;
		}
		for (int j = 0; j < targetCount; j++)
		{
			float roll = UnityEngine.Random.Range(0f, 1f);
			int pickableCount = 0;
			for (int k = 0; k < targetCount; k++)
			{
				if (ComputeBoxPickChance(boxUsageCounts, k) >= roll)
				{
					pickableBoxIndexes[pickableCount++] = k;
				}
			}
			int boxIndex = pickableBoxIndexes[UnityEngine.Random.Range(0, pickableCount)];
			boxUsageCounts[boxIndex]++;
			float boxLeft = boundsLeft + (float)boxIndex * boxWidth;
			float boxRight = boxLeft + boxWidth;
			Vector3 position = default(Vector3);
			position.x = UnityEngine.Random.Range(boxLeft, boxRight);
			position.y = bounds.center.y;
			position.z = UnityEngine.Random.Range(boundsBottom, boundsTop);
			GenerateVisualTarget(position, j, boxIndex);
		}
	}

	private void GenerateVisualTarget(Vector3 position, int index, int boxIndex)
	{
		GameObject targetObject = new GameObject();
		targetObject.name = $"{this} Target {index} (box {boxIndex})";
		targetObject.transform.position = position;
		targetObject.AddComponent<SpellGeneratedTarget>();
		AddVisualTarget(targetObject);
	}

	private float ComputeBoxPickChance(int[] boxUsageCounts, int index)
	{
		int num = boxUsageCounts[index];
		float maxBoxUsage = (float)boxUsageCounts.Length * 0.25f;
		float usage = (float)num / maxBoxUsage;
		return 1f - usage;
	}

	private void GenerateCreatedCardsTargets(TAG_ZONE onlyAffectZone = TAG_ZONE.INVALID, bool includeZoneChange = false, bool onlyMinions = false)
	{
		if (m_taskList == null)
		{
			return;
		}
		if (m_taskList.IsStartOfBlock())
		{
			m_createCardsInSetAside.Clear();
		}
		foreach (PowerTask task in m_taskList.GetTaskList())
		{
			Network.PowerHistory power = task.GetPower();
			switch (power.Type)
			{
			case Network.PowerType.FULL_ENTITY:
			{
				int entID2 = ((Network.HistFullEntity)power).Entity.ID;
				Entity entity2 = GameState.Get().GetEntity(entID2);
				if (entity2 == null)
				{
					Debug.LogWarning($"{this}.GenerateCreatedCardsTargets() - WARNING trying to target entity with id {entID2} but there is no entity with that id");
					break;
				}
				TAG_ZONE zone2 = entity2.GetTag<TAG_ZONE>(GAME_TAG.ZONE);
				if (onlyAffectZone != 0 && zone2 != onlyAffectZone)
				{
					break;
				}
				switch (zone2)
				{
				case TAG_ZONE.SETASIDE:
					m_createCardsInSetAside.Add(entID2);
					break;
				default:
					if (!onlyMinions || entity2.GetCardType() == TAG_CARDTYPE.MINION)
					{
						Card card2 = entity2.GetCard();
						if (card2 == null)
						{
							Debug.LogWarning($"{this}.GenerateCreatedCardsTargets() - WARNING trying to target entity.GetCard() with id {entID2} but there is no card with that id");
						}
						else
						{
							m_visualTargets.Add(card2.gameObject);
						}
					}
					break;
				case TAG_ZONE.LETTUCE_ABILITY:
					break;
				}
				break;
			}
			case Network.PowerType.SHOW_ENTITY:
			{
				Network.HistShowEntity showEntity = (Network.HistShowEntity)power;
				int entID3 = showEntity.Entity.ID;
				Entity entity3 = GameState.Get().GetEntity(entID3);
				if (!m_createCardsInSetAside.Contains(entID3))
				{
					break;
				}
				TAG_ZONE zone3 = TAG_ZONE.INVALID;
				foreach (Network.Entity.Tag tag in showEntity.Entity.Tags)
				{
					if (tag.Name == 49)
					{
						zone3 = (TAG_ZONE)tag.Value;
					}
				}
				if (zone3 != 0 && zone3 != TAG_ZONE.LETTUCE_ABILITY && zone3 != TAG_ZONE.SETASIDE && (onlyAffectZone == TAG_ZONE.INVALID || zone3 == onlyAffectZone) && (!onlyMinions || entity3.GetCardType() == TAG_CARDTYPE.MINION))
				{
					Card card3 = GameState.Get().GetEntity(entID3)?.GetCard();
					if (card3 == null)
					{
						Debug.LogWarning($"{this}.GenerateCreatedCardsTargets() - WARNING trying to target entity.GetCard() with id {entID3} but there is no card with that id");
						break;
					}
					m_createCardsInSetAside.Remove(entID3);
					m_visualTargets.Add(card3.gameObject);
				}
				break;
			}
			case Network.PowerType.TAG_CHANGE:
			{
				Network.HistTagChange tagChange = (Network.HistTagChange)power;
				if (tagChange.Tag != 49)
				{
					break;
				}
				TAG_ZONE zone = (TAG_ZONE)tagChange.Value;
				if (zone == TAG_ZONE.LETTUCE_ABILITY || zone == TAG_ZONE.SETASIDE || (onlyAffectZone != 0 && zone != onlyAffectZone))
				{
					break;
				}
				int entID = tagChange.Entity;
				Entity entity = GameState.Get().GetEntity(entID);
				if ((!m_createCardsInSetAside.Contains(entID) && !includeZoneChange) || (onlyMinions && entity.GetCardType() != TAG_CARDTYPE.MINION))
				{
					break;
				}
				Card card = GameState.Get().GetEntity(entID)?.GetCard();
				if (card == null)
				{
					Debug.LogWarning($"{this}.GenerateCreatedCardsTargets() - WARNING trying to target entity.GetCard() with id {entID} but there is no card with that id");
					break;
				}
				if (m_createCardsInSetAside.Contains(entID))
				{
					m_createCardsInSetAside.Remove(entID);
				}
				m_visualTargets.Add(card.gameObject);
				break;
			}
			}
		}
	}

	private void AddChosenTargetAsVisualTarget()
	{
		Card targetCard = GetPowerTargetCard();
		if (targetCard == null)
		{
			Debug.LogWarning($"{this}.AddChosenTargetAsVisualTarget() - there is no chosen target");
		}
		else
		{
			AddVisualTarget(targetCard.gameObject);
		}
	}

	private void AddAllTargetsAsVisualTargets()
	{
		for (int i = 0; i < m_targets.Count; i++)
		{
			int visualTargetIndex = m_visualTargets.Count;
			m_visualToTargetIndexMap[visualTargetIndex] = i;
			AddVisualTarget(m_targets[i]);
		}
	}

	private void AddSameTargetForAdditionalMissiles()
	{
		for (int i = 1; i < m_MissileInfo.TimesToHitSameTarget; i++)
		{
			int visualTargetIndex = GetVisualTargets().Count;
			m_visualToTargetIndexMap[visualTargetIndex] = i;
			AddVisualTarget(GetVisualTargets()[0]);
		}
	}

	private void SuppressPlaySoundsOnVisualTargets()
	{
		if (!m_TargetInfo.SuppressPlaySounds)
		{
			return;
		}
		for (int i = 0; i < m_visualTargets.Count; i++)
		{
			Card targetCard = m_visualTargets[i].GetComponent<Card>();
			if (!(targetCard == null))
			{
				if (targetCard.GetEntity().GetTag(GAME_TAG.DONT_SUPPRESS_SUMMON_VO) == 1)
				{
					break;
				}
				targetCard.SuppressPlaySounds(suppress: true);
			}
		}
	}

	protected virtual void CleanUp()
	{
		foreach (GameObject targetObject in m_visualTargets)
		{
			if (targetObject == null)
			{
				Debug.LogWarning($"{this}.CleanUp() - found a null GameObject in m_visualTargets");
			}
			else if (!(targetObject.GetComponent<SpellGeneratedTarget>() == null))
			{
				UnityEngine.Object.Destroy(targetObject);
			}
		}
		m_visualTargets.Clear();
	}

	protected bool HasMetaDataTargets()
	{
		return m_targetToMetaDataMap.Count > 0;
	}

	protected int GetMetaDataIndexForTarget(int visualTargetIndex)
	{
		if (!m_visualToTargetIndexMap.TryGetValue(visualTargetIndex, out var targetIndex))
		{
			return -1;
		}
		if (!m_targetToMetaDataMap.TryGetValue(targetIndex, out var metaDataIndex))
		{
			return -1;
		}
		return metaDataIndex;
	}

	protected bool ShouldCompleteTasksUntilMetaData(int metaDataIndex)
	{
		if (m_taskList == null || IsBatchedTargetInfo(metaDataIndex) || !m_taskList.HasEarlierIncompleteTask(metaDataIndex))
		{
			return false;
		}
		return true;
	}

	private bool IsBatchedTargetInfo(int metaDataIndex)
	{
		if (m_taskList.GetTaskList().Count >= metaDataIndex)
		{
			return false;
		}
		if (m_taskList.GetTaskList()[metaDataIndex].GetPower() is Network.HistMetaData { MetaType: HistoryMeta.Type.TARGET, Data: not 0 })
		{
			return true;
		}
		return false;
	}

	protected IEnumerator CompleteTasksUntilMetaData(int metaDataIndex)
	{
		m_effectsPendingFinish++;
		m_taskList.DoTasks(0, metaDataIndex);
		QueueList<PowerTask> tasks = DetermineTasksToWaitFor(0, metaDataIndex);
		if (tasks != null && tasks.Count > 0)
		{
			yield return StartCoroutine(WaitForTasks(tasks));
		}
		m_effectsPendingFinish--;
	}

	protected QueueList<PowerTask> DetermineTasksToWaitFor(int startIndex, int count)
	{
		if (count == 0)
		{
			return null;
		}
		int endIndex = startIndex + count;
		QueueList<PowerTask> tasksToWaitFor = new QueueList<PowerTask>();
		List<PowerTask> tasks = m_taskList.GetTaskList();
		for (int i = startIndex; i < endIndex; i++)
		{
			PowerTask task = tasks[i];
			Entity entity = GetEntityFromZoneChangePowerTask(task);
			if (entity == null || m_visualTargets.Find(delegate(GameObject currTargetObject)
			{
				Card component = currTargetObject.GetComponent<Card>();
				return entity.GetCard() == component;
			}) == null)
			{
				continue;
			}
			for (int j = 0; j < tasksToWaitFor.Count; j++)
			{
				PowerTask pastTask = tasksToWaitFor[j];
				Entity pastEntity = GetEntityFromZoneChangePowerTask(pastTask);
				if (entity == pastEntity)
				{
					tasksToWaitFor.RemoveAt(j);
					break;
				}
			}
			tasksToWaitFor.Enqueue(task);
		}
		return tasksToWaitFor;
	}

	protected IEnumerator WaitForTasks(QueueList<PowerTask> tasksToWaitFor)
	{
		while (tasksToWaitFor.Count > 0)
		{
			PowerTask task = tasksToWaitFor.Peek();
			if (!task.IsCompleted())
			{
				yield return null;
				continue;
			}
			GetZoneChangeFromPowerTask(task, out var entity, out var zoneTag);
			Card card = entity.GetCard();
			Zone zone = ZoneMgr.Get().FindZoneForEntityAndZoneTag(entity, (TAG_ZONE)zoneTag);
			while (card.GetZone() != zone)
			{
				yield return null;
			}
			while (card.IsActorLoading())
			{
				yield return null;
			}
			tasksToWaitFor.Dequeue();
		}
	}

	protected IEnumerator CompleteTasksFromMetaData(int metaDataIndex, float delaySec)
	{
		m_effectsPendingFinish++;
		yield return new WaitForSeconds(delaySec);
		CompleteMetaDataTasks(metaDataIndex, OnMetaDataTasksComplete);
	}

	protected void OnMetaDataTasksComplete(PowerTaskList taskList, int startIndex, int count, object userData)
	{
		m_effectsPendingFinish--;
		FinishIfPossible();
	}

	protected bool IsMakingClones()
	{
		return true;
	}

	protected bool AreEffectsActive()
	{
		return m_effectsPendingFinish > 0;
	}

	public ISpell CloneSpell(ISpell prefab, Vector3? position = null)
	{
		if (!(prefab is Spell template))
		{
			return null;
		}
		Spell spell;
		if (IsMakingClones())
		{
			if (position.HasValue)
			{
				spell = SpellManager.Get().GetSpell(template);
				spell.transform.position = position.Value;
			}
			else
			{
				spell = SpellManager.Get().GetSpell(template);
			}
			spell.AddStateStartedCallback(OnCloneSpellStateStarted);
			spell.transform.parent = base.transform;
			m_activeClonedSpells.Add(spell);
		}
		else
		{
			spell = template;
			spell.RemoveAllTargets();
		}
		spell.AddFinishedCallback(OnCloneSpellFinished);
		return spell;
	}

	private void OnCloneSpellFinished(Spell spell, object userData)
	{
		m_effectsPendingFinish--;
		FinishIfPossible();
	}

	private void OnCloneSpellStateStarted(Spell spell, SpellStateType prevStateType, object userData)
	{
		if (spell.GetActiveState() == SpellStateType.NONE)
		{
			m_activeClonedSpells.Remove(spell);
			SpellManager.Get().ReleaseSpell(spell);
		}
	}

	private void UpdatePendingStateChangeFlags(SpellStateType stateType)
	{
		if (!HasStateContent(stateType))
		{
			m_pendingNoneStateChange = true;
			m_pendingSpellFinish = true;
		}
		else
		{
			m_pendingNoneStateChange = false;
			m_pendingSpellFinish = false;
		}
	}

	protected void FinishIfPossible()
	{
		if (!m_settingUpAction && !AreEffectsActive())
		{
			if (m_pendingSpellFinish)
			{
				OnSpellFinished();
				m_pendingSpellFinish = false;
			}
			if (m_pendingNoneStateChange)
			{
				OnStateFinished();
				m_pendingNoneStateChange = false;
			}
			CleanUp();
		}
	}

	void ICutsceneActionListener.Play()
	{
		m_pendingSpellFinish = true;
		DoStartSpellAction();
		DoActionNow();
	}

	void ICutsceneActionListener.Stop()
	{
		FinishIfPossible();
	}
}
