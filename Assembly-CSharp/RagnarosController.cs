using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RagnarosController : MonoBehaviour, IScriptEventHandler
{
	[Serializable]
	public class RagnarosAttackCurves
	{
		public AnimationCurve armCurve;

		public AnimationCurve spineCurve;
	}

	private enum ControllerState
	{
		Armed,
		Attacking,
		Finished
	}

	[Header("Ragnaros Settings")]
	[SerializeField]
	[Range(0f, 1f)]
	[Tooltip("Rotation weight during Attack Idle")]
	private float m_rotateAmount;

	[SerializeField]
	[Tooltip("Offset the spine LookAt target on the Y axis")]
	private float m_lookAtOffset;

	[Tooltip("The min/max distance a target can be from the controller on the X axis")]
	[SerializeField]
	private Vector2 m_horizontalDistance;

	[Tooltip("The min/max distance a target can be from the controller on the Z axis")]
	[SerializeField]
	private Vector2 m_verticalDistance;

	[Header("Sulfuras Settings")]
	[SerializeField]
	private Transform m_hammer;

	[SerializeField]
	private Vector2 m_hammerOffset;

	[Tooltip("IK animation length in seconds")]
	[Header("Attack IK Settings")]
	[Range(0f, 3f)]
	[SerializeField]
	private float m_animationLength;

	[Tooltip("Value dictating whether Attack or Attack Close animation is triggered relative to proximity to target")]
	[SerializeField]
	private float m_distanceDelta;

	[Tooltip("IK weight over time relative to Animation Length")]
	[SerializeField]
	private RagnarosAttackCurves m_attackCurves;

	[Tooltip("IK weight over time relative to Animation Length")]
	[SerializeField]
	private RagnarosAttackCurves m_attackCloseCurves;

	[SerializeField]
	private AnimationCurve m_distanceCurve;

	private IK_Humanoid m_IK;

	private IK_Humanoid.ChainData m_lookAt;

	private Animator m_animator;

	private Transform m_arm_target;

	private Transform m_spine_target;

	private Transform m_offset;

	private IEnumerator m_attackRoutine;

	private ControllerState m_state;

	private Vector3 m_hammerPosition;

	private Vector3 m_reticlePosition;

	private Quaternion m_startRotation;

	private const string m_summon = "Summon";

	private const string m_attack = "Attack";

	private const string m_attackClose = "AttackClose";

	private const string m_cancel = "Cancel";

	public Vector3 ReticlePosition
	{
		get
		{
			return m_reticlePosition;
		}
		set
		{
			m_reticlePosition = value;
		}
	}

	public void HandleEvent(string eventName)
	{
		switch (eventName)
		{
		case "ATTACK_BIRTH":
			m_state = ControllerState.Armed;
			Summon();
			break;
		case "ATTACK":
			m_state = ControllerState.Attacking;
			Attack();
			break;
		case "ATTACK_CANCEL":
			m_state = ControllerState.Finished;
			Cancel();
			break;
		case "ATTACK_STOP":
			m_state = ControllerState.Finished;
			Stop();
			break;
		}
	}

	private void Start()
	{
		m_hammerPosition = m_hammer.localPosition;
		m_animator = GetComponentInChildren<Animator>();
		m_offset = m_animator.transform;
		m_startRotation = m_offset.rotation;
		m_IK = GetComponentInChildren<IK_Humanoid>();
		foreach (IK_Humanoid.ChainData chainData in m_IK.chainData)
		{
			chainData.Chain.globalWeight = 0f;
			if (chainData.chainTag == IK_Humanoid.ChainData.ChainTag.LookAt)
			{
				m_spine_target = chainData.Chain.chainTarget;
				m_lookAt = chainData;
			}
			else
			{
				m_arm_target = chainData.Chain.chainTarget;
			}
		}
	}

	private void Summon()
	{
		m_animator.SetTrigger("Summon");
		m_lookAt.Chain.globalWeight = 1f;
	}

	private void Attack()
	{
		if (m_attackRoutine != null)
		{
			StopCoroutine(m_attackRoutine);
		}
		m_lookAt.Chain.globalWeight = 0f;
		Vector3 target = GetTarget();
		Rotate(target);
		m_arm_target.position = new Vector3(target.x, base.transform.position.y, target.z);
		m_arm_target.rotation = Quaternion.LookRotation(m_arm_target.position - base.transform.position);
		float distanceModifier = DistanceModifier(m_arm_target.localPosition);
		bool close = distanceModifier <= m_distanceDelta;
		m_animator.SetTrigger(close ? "AttackClose" : "Attack");
		RagnarosAttackCurves curves = (close ? m_attackCloseCurves : m_attackCurves);
		m_attackRoutine = AttackRoutine(distanceModifier, curves);
		StartCoroutine(m_attackRoutine);
	}

	private float DistanceModifier(Vector3 target)
	{
		float horizatonalSign = ((Mathf.Sign(target.x) < 0f) ? m_horizontalDistance.x : m_horizontalDistance.y);
		float horizontal = Mathf.InverseLerp(0f, Mathf.Abs(horizatonalSign), Mathf.Abs(target.x));
		float vertical = Mathf.InverseLerp(m_verticalDistance.x, m_verticalDistance.y, target.z);
		float evaluate = Mathf.Max(Mathf.Lerp(0f, 1f, horizontal + vertical / 4f), vertical);
		return m_distanceCurve.Evaluate(evaluate);
	}

	private IEnumerator AttackRoutine(float distanceModifier, RagnarosAttackCurves attackData)
	{
		float hammerZ = Mathf.Lerp(m_hammerOffset.x, m_hammerOffset.y, distanceModifier);
		Vector3 hammerTarget = new Vector3(m_hammerPosition.z, m_hammerPosition.y, hammerZ);
		List<IK_Humanoid.ChainData> chainData = m_IK.GetIKChains();
		float countdown = 1f;
		while (countdown > 0f)
		{
			countdown -= Time.deltaTime / m_animationLength;
			float armValue = attackData.armCurve.Evaluate(1f - countdown);
			float spineValue = attackData.spineCurve.Evaluate(1f - countdown);
			for (int i = 0; i < chainData.Count; i++)
			{
				bool spine = chainData[i].chainTag == IK_Humanoid.ChainData.ChainTag.Spine;
				chainData[i].Chain.globalWeight = (spine ? (spineValue * distanceModifier) : armValue);
			}
			m_hammer.localPosition = Vector3.Lerp(m_hammerPosition, hammerTarget, armValue);
			yield return null;
		}
	}

	private void Cancel()
	{
		m_animator.SetTrigger("Cancel");
	}

	private void Stop()
	{
	}

	private void Idle()
	{
		Vector3 reticleTarget = GetReticleTarget();
		SetLookAtTarget(reticleTarget);
		Rotate(reticleTarget);
	}

	private void SetLookAtTarget(Vector3 reticleTarget)
	{
		if (!(m_lookAt.Chain.leafBone == null))
		{
			float offset = m_lookAt.Chain.leafBone.position.y + m_lookAtOffset;
			Vector3 target = base.transform.InverseTransformPoint(new Vector3(reticleTarget.x, offset, reticleTarget.z));
			target.z = Mathf.Clamp(target.z, 0.5f, float.PositiveInfinity);
			m_spine_target.localPosition = target;
		}
	}

	private void Rotate(Vector3 reticleTarget)
	{
		reticleTarget.y = m_offset.position.y;
		Quaternion targetRotation = Quaternion.LookRotation(reticleTarget - m_offset.position);
		m_offset.rotation = Quaternion.Lerp(m_startRotation, targetRotation, m_rotateAmount);
	}

	private Vector3 GetTarget()
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
		return GetReticleTarget();
	}

	private Vector3 GetReticleTarget()
	{
		TargetReticleManager reticleManager = TargetReticleManager.Get();
		if (reticleManager != null)
		{
			if (reticleManager.IsLocalArrowActive())
			{
				m_reticlePosition = reticleManager.LocalTargetArrowPosition;
			}
			else if (reticleManager.IsEnemyArrowActive())
			{
				m_reticlePosition = reticleManager.RemoteTargetArrowPosition;
			}
		}
		return m_reticlePosition;
	}

	private void Update()
	{
		switch (m_state)
		{
		case ControllerState.Armed:
			Idle();
			break;
		case ControllerState.Attacking:
		case ControllerState.Finished:
			break;
		}
	}
}
