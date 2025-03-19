using UnityEngine;

public class Hero_06ah_EyeTracker : LegendaryHeroGenericEventHandler
{
	private enum State
	{
		Idle,
		TrackRemoteReticle,
		LookingAtAimPosition,
		LookAtTarget,
		FinishAiming
	}

	public float m_turnFromTime = 0.4f;

	public Vector3 m_lookOffset;

	public LegendaryHeroActorCache m_actorCache;

	public float m_remoteRotationDegPerSecond = 180f;

	public float m_remotePlayerTargetTimeout = 2f;

	public LegendaryHeroAnimController m_animController;

	private Quaternion m_desiredRotation;

	private float m_currentLookAtHeight = 2.5f;

	private Quaternion m_startRotation;

	private State m_state;

	private float m_timer;

	private bool m_isOpposingPlayer;

	private Actor m_owningActor;

	private bool m_isSpectator;

	private void Awake()
	{
		if (m_actorCache != null)
		{
			m_actorCache.ActorAttachedEvent += OnActorAttached;
		}
		else
		{
			base.gameObject.SetActive(value: false);
		}
		m_isSpectator = SpectatorManager.Get().IsSpectatingOrWatching;
	}

	private void OnDestroy()
	{
		if (m_animController != null)
		{
			m_animController.OnRequestAnimTransition -= OnOpponentRequestedAnimTransition;
		}
	}

	private void Update()
	{
		if (!(m_owningActor == null))
		{
			m_timer += Time.deltaTime;
			switch (m_state)
			{
			case State.Idle:
				UpdateIdleState();
				break;
			case State.TrackRemoteReticle:
				UpdateTrackRemoteReticle();
				break;
			case State.LookingAtAimPosition:
				UpdateLookingAtAimPos();
				break;
			case State.FinishAiming:
				UpdateLerpingFromDesiredPos();
				break;
			case State.LookAtTarget:
				break;
			}
		}
	}

	private void ChangeState(State state)
	{
		m_state = state;
		m_timer = 0f;
	}

	private void UpdateIdleState()
	{
		base.transform.rotation = (m_desiredRotation = m_startRotation);
		TargetReticleManager reticleManager = TargetReticleManager.Get();
		if (((m_isOpposingPlayer && reticleManager.IsEnemyArrowActive()) || (!m_isOpposingPlayer && m_isSpectator && reticleManager.IsLocalArrowActive())) && reticleManager.SourceEntityID == m_owningActor.GetEntity().GetEntityId())
		{
			m_animController.RaiseAnimEvent(LegendaryHeroAnimations.AttackBirth);
			m_animController.IgnoreAnim(LegendaryHeroAnimations.HeroPower, 1f);
			ChangeState(State.TrackRemoteReticle);
		}
	}

	private void UpdateTrackRemoteReticle()
	{
		if (GetReticlePos(out var reticlePos))
		{
			SetLookAt(reticlePos);
			m_timer = 0f;
		}
		else if ((m_isOpposingPlayer || m_isSpectator) && m_timer >= m_remotePlayerTargetTimeout)
		{
			m_animController.RaiseAnimEvent(LegendaryHeroAnimations.AttackCancel);
			ChangeState(State.FinishAiming);
			return;
		}
		UpdateLookAt();
	}

	private void UpdateLookingAtAimPos()
	{
		if (GetReticlePos(out var reticlePos))
		{
			SetLookAt(reticlePos);
		}
		UpdateLookAt();
	}

	private bool GetReticlePos(out Vector3 reticlePos)
	{
		TargetReticleManager reticleManager = TargetReticleManager.Get();
		reticlePos = Vector3.zero;
		if (m_isOpposingPlayer || m_isSpectator)
		{
			if ((m_isOpposingPlayer && !reticleManager.IsEnemyArrowActive()) || (!m_isOpposingPlayer && m_isSpectator && !reticleManager.IsLocalArrowActive()))
			{
				return false;
			}
			reticlePos = reticleManager.RemoteTargetArrowPosition;
		}
		else
		{
			if (!reticleManager.IsLocalArrowActive())
			{
				return false;
			}
			reticlePos = reticleManager.LocalTargetArrowPosition;
		}
		return true;
	}

	private void LookAtTarget(GameObject target)
	{
		Vector3 targetPos = target.transform.position;
		SetLookAt(new Vector3(targetPos.x, m_currentLookAtHeight, targetPos.z));
		base.transform.rotation = m_desiredRotation;
		ChangeState(State.LookAtTarget);
	}

	private void SetLookAt(Vector3 pos)
	{
		if (!(m_owningActor == null))
		{
			m_currentLookAtHeight = pos.y;
			Vector3 lookDirection = (pos + m_lookOffset - m_owningActor.transform.position).normalized;
			m_desiredRotation = Quaternion.LookRotation(lookDirection) * Quaternion.LookRotation(-Vector3.up);
		}
	}

	private void UpdateLookAt()
	{
		if (m_isOpposingPlayer || m_isSpectator)
		{
			base.transform.rotation = Quaternion.RotateTowards(base.transform.rotation, m_desiredRotation, Time.deltaTime * m_remoteRotationDegPerSecond);
		}
		else
		{
			base.transform.rotation = m_desiredRotation;
		}
	}

	private void UpdateLerpingFromDesiredPos()
	{
		base.transform.rotation = Quaternion.Lerp(m_desiredRotation, m_startRotation, m_timer / m_turnFromTime);
		if (m_timer >= m_turnFromTime)
		{
			ChangeState(State.Idle);
		}
	}

	public override void HandleEvent(string eventName, object eventData)
	{
		if (eventName == LegendaryHeroGenericEvents.Cthun_Start_Attack_Aim)
		{
			ChangeState(State.LookingAtAimPosition);
		}
		else if (eventName == LegendaryHeroGenericEvents.Cthun_Start_Attack)
		{
			LookAtTarget(eventData as GameObject);
		}
		else if (eventName == LegendaryHeroGenericEvents.Cthun_Stop_Attack)
		{
			ChangeState(State.FinishAiming);
		}
	}

	public void OnOpponentRequestedAnimTransition(LegendaryHeroAnimController.InternalState state)
	{
		if (m_state != 0 && state != LegendaryHeroAnimController.InternalState.AttackStart && state != LegendaryHeroAnimController.InternalState.AttackComplete && state != LegendaryHeroAnimController.InternalState.HeroPowerStart && state != LegendaryHeroAnimController.InternalState.HeroPowerComplete)
		{
			ChangeState(State.FinishAiming);
		}
	}

	private void OnActorAttached(Actor actor)
	{
		m_actorCache.ActorAttachedEvent -= OnActorAttached;
		Card heroCard = actor.GetCard();
		if (heroCard == null)
		{
			return;
		}
		if (heroCard.GetControllerSide() == Player.Side.OPPOSING)
		{
			base.transform.localEulerAngles = new Vector3(0f, 180f, 0f);
			base.transform.localScale = new Vector3(0f - base.transform.localScale.x, base.transform.localScale.y, base.transform.localScale.z);
			m_isOpposingPlayer = true;
			if (m_animController != null)
			{
				m_animController.OnRequestAnimTransition += OnOpponentRequestedAnimTransition;
			}
		}
		m_owningActor = actor;
		m_startRotation = base.transform.rotation;
	}
}
