using System;
using System.Collections.Generic;
using UnityEngine;

public class StateDrivenAnimation : MonoBehaviour, ISerializationCallbackReceiver
{
	[Serializable]
	public class State
	{
		public string StateName;

		public AnimationCurve Weight;

		[HideInInspector]
		public int FullPathHash;
	}

	[Serializable]
	public class Channel
	{
		public string Name;

		public State[] States;
	}

	[Header("Animation Channels")]
	public Channel[] Channels;

	private Animator m_animator;

	private readonly Dictionary<string, float> m_channelValues = new Dictionary<string, float>();

	protected virtual void Awake()
	{
		m_animator = GetComponentInChildren<Animator>();
	}

	protected virtual void OnEnable()
	{
	}

	protected virtual void Update()
	{
		m_channelValues.Clear();
		if (Channels != null && m_animator != null)
		{
			AnimatorStateInfo stateInfo = m_animator.GetCurrentAnimatorStateInfo(0);
			Channel[] channels = Channels;
			foreach (Channel channel in channels)
			{
				float weight = 0f;
				if (channel.States != null)
				{
					State[] states = channel.States;
					foreach (State state in states)
					{
						if (stateInfo.fullPathHash == state.FullPathHash)
						{
							weight = ((state.Weight == null) ? 1f : state.Weight.Evaluate(stateInfo.normalizedTime));
							break;
						}
					}
				}
				m_channelValues.Add(channel.Name, weight);
			}
		}
		UpdateAnimation(in m_channelValues);
	}

	protected virtual void UpdateAnimation(in Dictionary<string, float> channelValues)
	{
	}

	void ISerializationCallbackReceiver.OnBeforeSerialize()
	{
	}

	void ISerializationCallbackReceiver.OnAfterDeserialize()
	{
	}
}
