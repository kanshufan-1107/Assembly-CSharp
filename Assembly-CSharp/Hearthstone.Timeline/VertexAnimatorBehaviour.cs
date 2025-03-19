using System;
using UnityEngine;
using UnityEngine.Playables;

namespace Hearthstone.Timeline;

[Serializable]
public class VertexAnimatorBehaviour : PlayableBehaviour
{
	[Serializable]
	public struct SpeedKey
	{
		public float time;

		public float value;
	}

	[Serializable]
	public class SpeedCurve
	{
		public SpeedKey[] keys = new SpeedKey[0];

		public int Length => keys.Length;

		public SpeedCurve()
		{
			Clear();
			AddKey(0f, 1f);
		}

		public void Clear()
		{
			keys = new SpeedKey[0];
		}

		public void AddKey(float time, float value)
		{
			for (int i = 0; i < Length; i++)
			{
				if (keys[i].time > time)
				{
					InsertKey(i, new SpeedKey
					{
						time = time,
						value = value
					});
					return;
				}
			}
			AddKey(new SpeedKey
			{
				time = time,
				value = value
			});
		}

		private void InsertKey(int index, SpeedKey key)
		{
			SpeedKey[] altKeys = new SpeedKey[Length];
			for (int i = 0; i < altKeys.Length; i++)
			{
				altKeys[i] = keys[i];
			}
			keys = new SpeedKey[Length + 1];
			for (int j = 0; j < Length; j++)
			{
				if (j < index)
				{
					keys[j] = altKeys[j];
				}
				else if (j == index)
				{
					keys[j] = key;
				}
				else
				{
					keys[j] = altKeys[j - 1];
				}
			}
		}

		private void AddKey(SpeedKey key)
		{
			SpeedKey[] altKeys = new SpeedKey[Length];
			for (int i = 0; i < altKeys.Length; i++)
			{
				altKeys[i] = keys[i];
			}
			keys = new SpeedKey[Length + 1];
			for (int j = 0; j < altKeys.Length; j++)
			{
				keys[j] = altKeys[j];
			}
			keys[altKeys.Length] = key;
		}

		public float Evaluate(float time)
		{
			for (int i = Length - 1; i >= 0; i--)
			{
				if (time >= keys[i].time)
				{
					return keys[i].value;
				}
			}
			return 0f;
		}

		public float GetScaledClipPosFromClipPos(float clipPos)
		{
			float total = 0f;
			for (int i = 0; i < Length; i++)
			{
				SpeedKey currKey = keys[i];
				SpeedKey speedKey2;
				if (i <= 0)
				{
					SpeedKey speedKey = default(SpeedKey);
					speedKey.time = 0f;
					speedKey.value = currKey.value;
					speedKey2 = speedKey;
				}
				else
				{
					speedKey2 = keys[i - 1];
				}
				SpeedKey prevKey = speedKey2;
				if (clipPos < currKey.time)
				{
					total += GetScaledRange(prevKey.time, clipPos, prevKey.value);
					break;
				}
				total += GetScaledRange(prevKey.time, currKey.time, prevKey.value);
				if (i == Length - 1)
				{
					total += GetScaledRange(currKey.time, clipPos, currKey.value);
				}
			}
			return total;
		}

		private float GetScaledRange(float startTime, float endTime, float animationSpeed)
		{
			return (endTime - startTime) * animationSpeed;
		}
	}

	[Tooltip("The name of the animation to play.")]
	[SerializeField]
	private string m_animationName = "Idle";

	[Tooltip("The start of the clip equates to this point in the animation.\nTime is measured as a percentage of the animation's length; *not* in seconds.")]
	[Range(0f, 1f)]
	[SerializeField]
	private float m_startTime;

	[Tooltip("The speed at which the animation should play.")]
	[SerializeField]
	private SpeedCurve m_animationSpeed = new SpeedCurve();

	private VertexAnimation m_vertexAnimation;

	private bool m_firstFrameHappened;

	private float m_speed = -1f;

	public override void ProcessFrame(Playable playable, FrameData info, object playerData)
	{
		if (m_vertexAnimation == null)
		{
			m_vertexAnimation = playerData as VertexAnimation;
		}
		if (m_vertexAnimation == null)
		{
			return;
		}
		if (Application.isPlaying)
		{
			float clipTime = (float)playable.GetTime();
			float clipDuration = (float)playable.GetDuration();
			if (!m_firstFrameHappened)
			{
				m_firstFrameHappened = true;
				UpdateAnimationPosition(clipTime, clipDuration, m_vertexAnimation.GetAnimationLengthUnscaled(m_animationName));
			}
			float speed = m_animationSpeed.Evaluate(clipTime / clipDuration);
			if (!Mathf.Approximately(speed, m_speed))
			{
				UpdateAnimationSpeed(speed);
				m_speed = speed;
			}
		}
		else if (!m_firstFrameHappened)
		{
			m_firstFrameHappened = true;
			UpdateAnimationSpeed(1f);
		}
	}

	public override void OnBehaviourPause(Playable playable, FrameData info)
	{
		m_firstFrameHappened = false;
		if (!(m_vertexAnimation == null))
		{
			UpdateAnimationSpeed(1f);
			base.OnBehaviourPause(playable, info);
		}
	}

	public void ScrubFrame(ScriptPlayable<VertexAnimatorBehaviour> playable)
	{
		if (!(m_vertexAnimation == null))
		{
			float clipTime = (float)playable.GetTime();
			float clipDuration = (float)playable.GetDuration();
			float animPos = GetAnimPos(clipTime / clipDuration, clipDuration, m_vertexAnimation.GetAnimationLengthUnscaled(m_animationName));
			m_vertexAnimation.SetAnimationCompletionPercent(m_animationName, animPos);
		}
	}

	public float GetClipToAnimScalar(float clipDuration, float animDuration)
	{
		return clipDuration / animDuration;
	}

	public float GetAnimPos(float clipPos, float clipToAnimScalar)
	{
		return (m_animationSpeed.GetScaledClipPosFromClipPos(clipPos) * clipToAnimScalar + m_startTime) % 1f;
	}

	public float GetAnimPos(float clipPos, float clipDuration, float animDuration)
	{
		return GetAnimPos(clipPos, GetClipToAnimScalar(clipDuration, animDuration));
	}

	private void UpdateAnimationPosition(float clipTime, float clipDuration, float animDuration)
	{
		m_vertexAnimation.SetAnimationCompletionPercent(m_animationName, (m_startTime + clipTime / clipDuration * GetClipToAnimScalar(clipDuration, animDuration)) % 1f);
	}

	private void UpdateAnimationSpeed(float speed)
	{
		m_vertexAnimation.OverwriteAnimationSpeed(speed);
	}
}
