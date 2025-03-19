using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TwistingNetherSpell : SuperSpell
{
	private enum VictimState
	{
		NONE,
		LIFTING,
		FLOATING,
		DRAINING,
		DEAD
	}

	private class Victim
	{
		public VictimState m_state;

		public Card m_card;
	}

	public float m_FinishTime;

	public TwistingNetherLiftInfo m_LiftInfo;

	public TwistingNetherFloatInfo m_FloatInfo;

	public TwistingNetherDrainInfo m_DrainInfo;

	public TwistingNetherSqueezeInfo m_SqueezeInfo;

	private static readonly Vector3 DEAD_SCALE = new Vector3(0.01f, 0.01f, 0.01f);

	private List<Victim> m_victims = new List<Victim>();

	protected override Card GetTargetCardFromPowerTask(int index, PowerTask task)
	{
		Network.PowerHistory power = task.GetPower();
		if (power.Type != Network.PowerType.TAG_CHANGE)
		{
			return null;
		}
		Network.HistTagChange tagChange = power as Network.HistTagChange;
		if (Gameplay.Get() != null)
		{
			if (tagChange.Tag != 360)
			{
				return null;
			}
			if (tagChange.Value != 1)
			{
				return null;
			}
		}
		Entity entity = GameState.Get().GetEntity(tagChange.Entity);
		if (entity == null)
		{
			Debug.LogWarning($"{this}.GetTargetCardFromPowerTask() - WARNING trying to target entity with id {tagChange.Entity} but there is no entity with that id");
			return null;
		}
		return entity.GetCard();
	}

	protected override void OnAction(SpellStateType prevStateType)
	{
		base.OnAction(prevStateType);
		if (!IsFinished())
		{
			Begin();
			if (CanFinish())
			{
				m_effectsPendingFinish--;
				FinishIfPossible();
			}
		}
	}

	protected override void CleanUp()
	{
		base.CleanUp();
		m_victims.Clear();
	}

	private void Begin()
	{
		m_effectsPendingFinish++;
		Action<object> OnDummyUpdate = delegate
		{
		};
		Hashtable args = iTweenManager.Get().GetTweenHashTable();
		args.Add("from", 0f);
		args.Add("to", 1f);
		args.Add("time", m_FinishTime);
		args.Add("onupdate", OnDummyUpdate);
		args.Add("oncomplete", "OnFinishTimeFinished");
		args.Add("oncompletetarget", base.gameObject);
		iTween.ValueTo(base.gameObject, args);
		Setup();
		Lift();
	}

	private void Setup()
	{
		foreach (GameObject target in GetTargets())
		{
			Card card = target.GetComponent<Card>();
			card.SetDoNotSort(on: true);
			Victim victim = new Victim();
			victim.m_state = VictimState.NONE;
			victim.m_card = card;
			m_victims.Add(victim);
		}
	}

	private void Lift()
	{
		foreach (Victim victim in m_victims)
		{
			victim.m_state = VictimState.LIFTING;
			Vector3 offset = TransformUtil.RandomVector3(m_LiftInfo.m_OffsetMin, m_LiftInfo.m_OffsetMax);
			Vector3 targetPoint = victim.m_card.transform.position + offset;
			float liftDelay = UnityEngine.Random.Range(m_LiftInfo.m_DelayMin, m_LiftInfo.m_DelayMax);
			float liftDuration = UnityEngine.Random.Range(m_LiftInfo.m_DurationMin, m_LiftInfo.m_DurationMax);
			Hashtable args = iTweenManager.Get().GetTweenHashTable();
			args.Add("position", targetPoint);
			args.Add("delay", liftDelay);
			args.Add("time", liftDuration);
			args.Add("easetype", m_LiftInfo.m_EaseType);
			args.Add("oncomplete", "OnLiftFinished");
			args.Add("oncompletetarget", base.gameObject);
			args.Add("oncompleteparams", victim);
			Vector3 targetRotation = new Vector3(UnityEngine.Random.Range(m_LiftInfo.m_RotationMin, m_LiftInfo.m_RotationMax), UnityEngine.Random.Range(m_LiftInfo.m_RotationMin, m_LiftInfo.m_RotationMax), UnityEngine.Random.Range(m_LiftInfo.m_RotationMin, m_LiftInfo.m_RotationMax));
			float rotateDelay = UnityEngine.Random.Range(m_LiftInfo.m_RotDelayMin, m_LiftInfo.m_RotDelayMax);
			float rotateDuration = UnityEngine.Random.Range(m_LiftInfo.m_RotDurationMin, m_LiftInfo.m_RotDurationMax);
			Hashtable args2 = iTweenManager.Get().GetTweenHashTable();
			args2.Add("rotation", targetRotation);
			args2.Add("delay", rotateDelay);
			args2.Add("time", rotateDuration);
			args2.Add("easetype", m_LiftInfo.m_EaseType);
			iTween.MoveTo(victim.m_card.gameObject, args);
			iTween.RotateTo(victim.m_card.gameObject, args2);
		}
	}

	private void OnLiftFinished(Victim victim)
	{
		Float(victim);
	}

	private void Float(Victim victim)
	{
		victim.m_state = VictimState.FLOATING;
		float duration = UnityEngine.Random.Range(m_FloatInfo.m_DurationMin, m_FloatInfo.m_DurationMax);
		Action<object> OnDummyUpdate = delegate
		{
		};
		Hashtable args = iTweenManager.Get().GetTweenHashTable();
		args.Add("from", 0f);
		args.Add("to", 1f);
		args.Add("time", duration);
		args.Add("onupdate", OnDummyUpdate);
		args.Add("oncomplete", "OnFloatFinished");
		args.Add("oncompletetarget", base.gameObject);
		args.Add("oncompleteparams", victim);
		iTween.ValueTo(victim.m_card.gameObject, args);
	}

	private void OnFloatFinished(Victim victim)
	{
		Drain(victim);
	}

	private void Drain(Victim victim)
	{
		victim.m_state = VictimState.LIFTING;
		float drainDelay = UnityEngine.Random.Range(m_DrainInfo.m_DelayMin, m_DrainInfo.m_DelayMax);
		float drainDuration = UnityEngine.Random.Range(m_DrainInfo.m_DurationMin, m_DrainInfo.m_DurationMax);
		Hashtable drainArgs = iTweenManager.Get().GetTweenHashTable();
		drainArgs.Add("position", base.transform.position);
		drainArgs.Add("delay", drainDelay);
		drainArgs.Add("time", drainDuration);
		drainArgs.Add("easetype", m_DrainInfo.m_EaseType);
		drainArgs.Add("oncomplete", "OnDrainFinished");
		drainArgs.Add("oncompletetarget", base.gameObject);
		drainArgs.Add("oncompleteparams", victim);
		iTween.MoveTo(victim.m_card.gameObject, drainArgs);
		float squeezeDelay = UnityEngine.Random.Range(m_SqueezeInfo.m_DelayMin, m_SqueezeInfo.m_DelayMax);
		float squeezeDuration = UnityEngine.Random.Range(m_SqueezeInfo.m_DurationMin, m_SqueezeInfo.m_DurationMax);
		Hashtable scaleArgs = iTweenManager.Get().GetTweenHashTable();
		scaleArgs.Add("scale", DEAD_SCALE);
		scaleArgs.Add("delay", squeezeDelay);
		scaleArgs.Add("time", squeezeDuration);
		scaleArgs.Add("easetype", m_SqueezeInfo.m_EaseType);
		iTween.ScaleTo(victim.m_card.gameObject, scaleArgs);
	}

	private void OnDrainFinished(Victim victim)
	{
		CleanUpVictim(victim);
	}

	private void OnFinishTimeFinished()
	{
		foreach (Victim victim in m_victims)
		{
			CleanUpVictim(victim);
		}
		m_effectsPendingFinish--;
		FinishIfPossible();
	}

	private void CleanUpVictim(Victim victim)
	{
		if (victim.m_state != VictimState.DEAD)
		{
			victim.m_state = VictimState.DEAD;
			victim.m_card.SetDoNotSort(on: false);
		}
	}

	private bool CanFinish()
	{
		foreach (Victim victim in m_victims)
		{
			if (victim.m_state != VictimState.DEAD)
			{
				return false;
			}
		}
		return true;
	}
}
