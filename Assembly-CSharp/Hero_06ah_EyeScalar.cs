using UnityEngine;

public class Hero_06ah_EyeScalar : MonoBehaviour
{
	public enum State
	{
		Idle,
		AnimatingIn,
		AnimatedIn,
		AnimatingOut
	}

	public LegendaryHeroActorCache m_actorCache;

	public float m_scaleFactor = 0.75f;

	public float m_totalAnimTime = 0.5f;

	public State m_state;

	private Entity m_heroEntity;

	private Vector3 m_startScale;

	private float m_currentAnimTime;

	private void Awake()
	{
		if ((bool)m_actorCache)
		{
			m_actorCache.ActorAttachedEvent += OnActorAttached;
		}
		m_startScale = base.transform.localScale;
	}

	private void Update()
	{
		SetStateBasedOnEntityTags();
		UpdateState();
	}

	private void UpdateState()
	{
		float scaleAmount;
		if (m_state == State.Idle)
		{
			scaleAmount = 1f;
			m_currentAnimTime = 0f;
		}
		else if (m_state == State.AnimatedIn)
		{
			scaleAmount = m_scaleFactor;
			m_currentAnimTime = 0f;
		}
		else
		{
			m_currentAnimTime += Time.deltaTime;
			m_currentAnimTime = Mathf.Min(m_currentAnimTime, m_totalAnimTime);
			if (m_state == State.AnimatingIn)
			{
				scaleAmount = Mathf.Lerp(1f, m_scaleFactor, m_currentAnimTime / m_totalAnimTime);
				if (m_currentAnimTime >= m_totalAnimTime)
				{
					m_state = State.AnimatedIn;
				}
			}
			else
			{
				scaleAmount = Mathf.Lerp(m_scaleFactor, 1f, m_currentAnimTime / m_totalAnimTime);
				if (m_currentAnimTime >= m_totalAnimTime)
				{
					m_state = State.Idle;
				}
			}
		}
		base.transform.localScale = m_startScale * scaleAmount;
	}

	private void SetStateBasedOnEntityTags()
	{
		if (HasValidStatusEffect())
		{
			if (m_state == State.Idle || m_state == State.AnimatingOut)
			{
				m_state = State.AnimatingIn;
			}
		}
		else if (m_state == State.AnimatedIn || m_state == State.AnimatingIn)
		{
			m_state = State.AnimatingOut;
		}
	}

	private bool HasValidStatusEffect()
	{
		if (m_heroEntity == null)
		{
			return false;
		}
		if (!m_heroEntity.IsFrozen() && !m_heroEntity.IsImmune() && !m_heroEntity.HasDivineShield() && !m_heroEntity.IsStealthed() && !m_heroEntity.IsEnraged() && m_heroEntity.CanBeTargetedBySpells() && m_heroEntity.CanBeTargetedByHeroPowers() && !m_heroEntity.HasTag(GAME_TAG.HEAVILY_ARMORED))
		{
			return m_heroEntity.HasTag(GAME_TAG.UNTOUCHABLE);
		}
		return true;
	}

	private void OnActorAttached(Actor actor)
	{
		m_heroEntity = actor.GetEntity();
		m_actorCache.ActorAttachedEvent -= OnActorAttached;
	}
}
