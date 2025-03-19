using System;
using System.Collections.Generic;
using Blizzard.T5.Core.Utils;
using Blizzard.T5.Game.Spells;
using UnityEngine;

[Serializable]
public class SpellStateAnimObject : ISpellStateAnimObject
{
	public enum Target
	{
		AS_SPECIFIED,
		ACTOR,
		ROOT_OBJECT
	}

	public GameObject m_GameObject;

	public Target m_Target;

	public AnimationClip m_AnimClip;

	public int m_AnimLayer;

	public float m_AnimSpeed = 1f;

	public float m_CrossFadeSec;

	public bool m_ControlParticles;

	public bool m_EmitParticles;

	public string m_Comment;

	public bool m_Enabled = true;

	private bool m_prevParticleEmitValue;

	public bool Enabled => m_Enabled;

	public GameObject GameObject => m_GameObject;

	public int AnimLayer => m_AnimLayer;

	public void Init()
	{
		if (!(m_GameObject == null) && !(m_AnimClip == null))
		{
			SetupAnimation();
		}
	}

	private void SetupAnimation()
	{
		string clipName = m_AnimClip.name;
		if (!m_GameObject.TryGetComponent<Animation>(out var anim))
		{
			anim = m_GameObject.AddComponent<Animation>();
		}
		anim.playAutomatically = false;
		if (anim[clipName] == null)
		{
			anim.AddClip(m_AnimClip, clipName);
		}
		anim[clipName].layer = m_AnimLayer;
	}

	public void OnLoad(SpellState state)
	{
		if (m_Target == Target.AS_SPECIFIED)
		{
			if (m_GameObject == null)
			{
				Debug.LogError("Error: spell state anim target has a null game object after load");
			}
		}
		else if (m_Target == Target.ACTOR)
		{
			Actor actor = GameObjectUtils.FindComponentInParents<Actor>(state.transform);
			if (actor == null || actor.gameObject == null)
			{
				Debug.LogError("Error: spell state anim target has a null game object after load");
				return;
			}
			m_GameObject = actor.gameObject;
			SetupAnimation();
		}
		else if (m_Target == Target.ROOT_OBJECT)
		{
			Actor actor2 = GameObjectUtils.FindComponentInParents<Actor>(state.transform);
			if (actor2 == null || actor2.gameObject == null)
			{
				Debug.LogError("Error: spell state anim target has a null game object after load");
				return;
			}
			m_GameObject = actor2.GetRootObject();
			SetupAnimation();
		}
		else
		{
			Debug.LogWarning("Error: unimplemented spell anim target");
		}
	}

	public void Play()
	{
		if (!m_Enabled || m_GameObject == null)
		{
			return;
		}
		if (m_AnimClip != null)
		{
			Animation animation = m_GameObject.GetComponent<Animation>();
			string stateName = m_AnimClip.name;
			AnimationState animationState = animation[stateName];
			animationState.enabled = true;
			animationState.speed = m_AnimSpeed;
			if (Mathf.Approximately(m_CrossFadeSec, 0f))
			{
				if (!animation.Play(stateName))
				{
					Debug.LogWarning($"SpellStateAnimObject.PlayNow() - FAILED to play clip {stateName} on {m_GameObject}");
				}
			}
			else
			{
				animation.CrossFade(stateName, m_CrossFadeSec);
			}
		}
		if (m_ControlParticles)
		{
			ParticleSystem system = m_GameObject.GetComponent<ParticleSystem>();
			if (system != null)
			{
				system.Play();
			}
		}
	}

	public void Stop()
	{
		if (!m_Enabled || m_GameObject == null)
		{
			return;
		}
		if (m_AnimClip != null)
		{
			m_GameObject.GetComponent<Animation>().Stop(m_AnimClip.name);
		}
		if (m_ControlParticles)
		{
			ParticleSystem system = m_GameObject.GetComponent<ParticleSystem>();
			if (system != null)
			{
				system.Stop();
			}
		}
	}

	public void Stop(List<ISpellState> nextStateList)
	{
		if (m_GameObject == null)
		{
			return;
		}
		if (m_AnimClip != null)
		{
			bool foundMatch = false;
			int i = 0;
			while (!foundMatch && i < nextStateList.Count)
			{
				SpellState nextState = nextStateList[i] as SpellState;
				for (int j = 0; j < nextState.m_ExternalAnimatedObjects.Count; j++)
				{
					ISpellStateAnimObject nextAnimObject = nextState.m_ExternalAnimatedObjects[j];
					if (nextAnimObject.Enabled && m_GameObject == nextAnimObject.GameObject && m_AnimLayer == nextAnimObject.AnimLayer)
					{
						foundMatch = true;
						break;
					}
				}
				i++;
			}
			if (!foundMatch)
			{
				m_GameObject.GetComponent<Animation>().Stop(m_AnimClip.name);
			}
		}
		if (m_ControlParticles)
		{
			ParticleSystem system = m_GameObject.GetComponent<ParticleSystem>();
			if (system != null)
			{
				system.Stop();
			}
		}
	}
}
