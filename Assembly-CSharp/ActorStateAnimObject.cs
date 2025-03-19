using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ActorStateAnimObject
{
	public bool m_Enabled = true;

	public GameObject m_GameObject;

	public AnimationClip m_AnimClip;

	public int m_AnimLayer;

	public float m_CrossFadeSec;

	public bool m_EmitParticles;

	public string m_Comment;

	private bool m_prevParticleEmitValue;

	public void Init()
	{
		if (!(m_GameObject == null) && !(m_AnimClip == null))
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
	}

	public void Play()
	{
		if (!m_Enabled || m_GameObject == null || !(m_AnimClip != null))
		{
			return;
		}
		string clipName = m_AnimClip.name;
		Animation actorAnimation = m_GameObject.GetComponent<Animation>();
		actorAnimation[clipName].enabled = true;
		if (Mathf.Approximately(m_CrossFadeSec, 0f))
		{
			if (!actorAnimation.Play(clipName))
			{
				Debug.LogWarning($"ActorStateAnimObject.PlayNow() - FAILED to play clip {clipName} on {m_GameObject}");
			}
		}
		else
		{
			actorAnimation.CrossFade(clipName, m_CrossFadeSec);
		}
	}

	public void Stop()
	{
		if (m_Enabled && !(m_GameObject == null) && m_AnimClip != null)
		{
			Animation component = m_GameObject.GetComponent<Animation>();
			component[m_AnimClip.name].time = 0f;
			component.Sample();
			component[m_AnimClip.name].enabled = false;
		}
	}

	public void Stop(List<ActorState> nextStateList)
	{
		if (!m_Enabled || m_GameObject == null || !(m_AnimClip != null))
		{
			return;
		}
		bool foundMatch = false;
		int i = 0;
		while (!foundMatch && i < nextStateList.Count)
		{
			ActorState nextState = nextStateList[i];
			for (int j = 0; j < nextState.m_ExternalAnimatedObjects.Count; j++)
			{
				ActorStateAnimObject nextAnimObject = nextState.m_ExternalAnimatedObjects[j];
				if (m_GameObject == nextAnimObject.m_GameObject && m_AnimLayer == nextAnimObject.m_AnimLayer)
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
}
