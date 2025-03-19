using System;
using UnityEngine;

public class Hero03ac_SparkleController : MonoBehaviour, ISerializationCallbackReceiver
{
	[Serializable]
	public class AnimState
	{
		public string StateName;

		[HideInInspector]
		public int FullPathHash;
	}

	private enum State
	{
		Waiting,
		Glinting
	}

	[Header("Game Objects")]
	public Transform[] AttachmentPoints;

	public Transform EffectRoot;

	[Header("Animation States")]
	public Animator Animator;

	public AnimState[] StateWhitelist;

	[Min(0.1f)]
	[Header("Glint Animation")]
	public float GlintTime;

	public float RotationRate;

	public float EffectScale;

	public AnimationCurve ScaleOverTime;

	[Header("Glint Delay")]
	[Min(0f)]
	public float MinDelayTime;

	[Min(0f)]
	public float MaxDelayTime;

	private State m_state;

	private float m_timer;

	private int m_currentIndex;

	private float m_rotationTimer;

	private void Awake()
	{
		float minDelayTime = Mathf.Min(MinDelayTime, MaxDelayTime);
		float maxDelayTime = Mathf.Max(MinDelayTime, MaxDelayTime, minDelayTime + 0.1f);
		MinDelayTime = minDelayTime;
		MaxDelayTime = maxDelayTime;
	}

	private void OnEnable()
	{
		m_currentIndex = int.MaxValue;
		Delay();
	}

	private void Update()
	{
		UpdateState();
		UpdateEffect();
	}

	private void Delay()
	{
		m_state = State.Waiting;
		m_timer = UnityEngine.Random.Range(MinDelayTime, MaxDelayTime);
		if (EffectRoot != null)
		{
			EffectRoot.gameObject.SetActive(value: false);
		}
	}

	private void PickNewAttachmentPoint()
	{
		m_state = State.Glinting;
		m_timer = 0f;
		m_rotationTimer = 0f;
		if (AttachmentPoints != null && AttachmentPoints.Length > 1)
		{
			int nextPoint = UnityEngine.Random.Range(0, AttachmentPoints.Length - 1);
			if (nextPoint >= m_currentIndex)
			{
				nextPoint++;
			}
			m_currentIndex = nextPoint;
		}
		if (m_currentIndex >= 0 && m_currentIndex < AttachmentPoints.Length && EffectRoot != null)
		{
			EffectRoot.SetParent(AttachmentPoints[m_currentIndex], worldPositionStays: false);
			EffectRoot.gameObject.SetActive(value: true);
		}
	}

	private void UpdateState()
	{
		if (m_state == State.Waiting)
		{
			m_timer -= Time.deltaTime;
			if (m_timer < 0f)
			{
				if (EffectAllowed())
				{
					PickNewAttachmentPoint();
				}
				else
				{
					Delay();
				}
			}
		}
		else if (m_state == State.Glinting)
		{
			m_timer += Time.deltaTime / GlintTime;
			if (m_timer > 1f)
			{
				Delay();
			}
		}
	}

	private bool EffectAllowed()
	{
		if (Animator != null && StateWhitelist != null)
		{
			AnimatorStateInfo stateInfo = Animator.GetCurrentAnimatorStateInfo(0);
			AnimState[] stateWhitelist = StateWhitelist;
			foreach (AnimState animState in stateWhitelist)
			{
				if (stateInfo.fullPathHash == animState.FullPathHash)
				{
					return true;
				}
			}
			return false;
		}
		return true;
	}

	private void UpdateEffect()
	{
		if (m_state != State.Glinting)
		{
			return;
		}
		m_rotationTimer += Time.deltaTime * RotationRate;
		m_rotationTimer %= 1f;
		if (EffectRoot != null)
		{
			EffectRoot.rotation = Quaternion.Euler(0f, 0f, m_rotationTimer * 360f);
			float scale = EffectScale;
			if (ScaleOverTime != null)
			{
				scale *= ScaleOverTime.Evaluate(m_timer);
			}
			EffectRoot.localScale = Vector3.one * scale;
		}
	}

	void ISerializationCallbackReceiver.OnBeforeSerialize()
	{
	}

	void ISerializationCallbackReceiver.OnAfterDeserialize()
	{
	}
}
