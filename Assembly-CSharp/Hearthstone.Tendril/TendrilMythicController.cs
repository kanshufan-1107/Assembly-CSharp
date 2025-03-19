using System;
using System.Collections.Generic;
using UnityEngine;

namespace Hearthstone.Tendril;

public class TendrilMythicController : MonoBehaviour, IScriptEventHandler
{
	[SerializeField]
	private Transform m_inverKinematicTarget;

	[SerializeField]
	private Transform m_lookAtTarget;

	[SerializeField]
	private AnimationCurve m_attackCurve;

	[SerializeField]
	private float m_attackTime;

	[SerializeField]
	private float m_lookAtOffset;

	[SerializeField]
	private float m_lookAtDelay;

	private TendrilController m_tendrilController;

	private Animator m_animator;

	private bool m_initialized;

	protected virtual void Start()
	{
		m_animator = GetComponentInChildren<Animator>();
		m_tendrilController = GetComponent<TendrilController>();
		if (!(m_animator != null) || !(m_tendrilController != null))
		{
			return;
		}
		List<TendrilBase> tendrils = m_tendrilController.GetTendrils();
		for (int i = 0; i < tendrils.Count; i++)
		{
			if (tendrils[i].GetType() != typeof(TendrilSpring))
			{
				tendrils[i].controlWeight = 0f;
			}
		}
		m_initialized = true;
	}

	public virtual void HandleEvent(string eventName)
	{
		if (m_initialized)
		{
			switch (eventName)
			{
			case "ATTACK_BIRTH":
				Idle();
				break;
			case "ATTACK":
				Attack();
				break;
			case "ATTACK_CANCEL":
				AttackCancel();
				break;
			case "ATTACK_STOP":
				AttackComplete();
				break;
			}
		}
	}

	protected Vector3 GetReticle()
	{
		Vector3 reticlePosition = Vector3.zero;
		TargetReticleManager reticleManager = TargetReticleManager.Get();
		if (reticleManager != null)
		{
			if (reticleManager.IsLocalArrowActive())
			{
				reticlePosition = reticleManager.LocalTargetArrowPosition;
			}
			else if (reticleManager.IsEnemyArrowActive())
			{
				reticlePosition = reticleManager.RemoteTargetArrowPosition;
			}
		}
		return reticlePosition;
	}

	protected Vector3 GetTarget()
	{
		Transform parent = base.transform.parent;
		if (parent != null)
		{
			Spell attackSpell = parent.GetComponent<Spell>();
			if (attackSpell != null)
			{
				GameObject targetObject = attackSpell.GetTarget();
				if (targetObject != null)
				{
					return targetObject.transform.position;
				}
			}
		}
		return GetReticle();
	}

	protected virtual void AnimateWeights(Type type, float weight, float time, float delay = 0f)
	{
		if (m_tendrilController.GetTendrilsByType(type, out var tendrils))
		{
			for (int i = 0; i < tendrils.Count; i++)
			{
				tendrils[i].AnimateWeight(weight, time, delay);
			}
		}
	}

	protected virtual void AnimateWeights(Type type, AnimationCurve curve, float time, float delay = 0f)
	{
		if (m_tendrilController.GetTendrilsByType(type, out var tendrils))
		{
			for (int i = 0; i < tendrils.Count; i++)
			{
				tendrils[i].AnimateWeight(curve, time, delay);
			}
		}
	}

	protected virtual void Idle()
	{
		m_animator.SetTrigger("Summon");
		AnimateWeights(typeof(TendrilLookAt), 1f, 0.5f, m_lookAtDelay);
	}

	protected virtual void Attack()
	{
		SetInverseKinematicTarget(GetTarget());
		m_animator.SetTrigger("Attack");
		AnimateWeights(typeof(TendrilLookAt), 0f, 0.5f);
		AnimateWeights(typeof(TendrilInverseKinematic), m_attackCurve, m_attackTime);
	}

	protected virtual void AttackCancel()
	{
		m_animator.SetTrigger("Cancel");
	}

	protected virtual void AttackComplete()
	{
		AnimateWeights(typeof(TendrilLookAt), 0f, 0.25f);
		AnimateWeights(typeof(TendrilInverseKinematic), 0f, 0.25f);
	}

	private void SetLookAtTarget(Vector3 target)
	{
		float offset = base.transform.position.y + m_lookAtOffset;
		Vector3 lookAtTarget = base.transform.InverseTransformPoint(new Vector3(target.x, offset, target.z));
		lookAtTarget.z = Mathf.Clamp(lookAtTarget.z, 0.5f, float.PositiveInfinity);
		m_lookAtTarget.localPosition = lookAtTarget;
	}

	private void SetInverseKinematicTarget(Vector3 target)
	{
		m_inverKinematicTarget.position = new Vector3(target.x, base.transform.position.y, target.z);
		m_inverKinematicTarget.rotation = Quaternion.LookRotation(m_inverKinematicTarget.position - base.transform.position);
	}

	protected virtual void Update()
	{
		if (m_initialized)
		{
			SetLookAtTarget(GetReticle());
		}
	}
}
