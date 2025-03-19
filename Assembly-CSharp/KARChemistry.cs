using System;
using System.Collections;
using UnityEngine;

[CustomEditClass]
public class KARChemistry : MonoBehaviour
{
	public PlayMakerFSM m_Lever;

	public PlayMakerFSM m_Knob;

	public Material m_BeakerMat;

	public Material m_CurlTubeMat;

	public Material m_SmallGlobeMat;

	public Material m_HeatGlowMat;

	public ParticleSystem m_BubbleFX;

	private bool m_isLeverOn;

	private bool m_isKnobOn;

	private void Start()
	{
		m_HeatGlowMat.SetFloat("_Intensity", 0f);
		SetEmissionRate(m_BubbleFX, 0f);
		m_CurlTubeMat.SetFloat("_UVOffsetSecondX", -0.8f);
		m_CurlTubeMat.SetFloat("_Intensity", 0f);
		m_SmallGlobeMat.SetFloat("_Transistion", 0f);
		m_BeakerMat.SetFloat("_Transistion", 0f);
	}

	private void Update()
	{
		HandleHits();
	}

	private void HandleHits()
	{
		if (InputCollection.GetMouseButtonUp(0) && IsOver(m_Lever.gameObject))
		{
			if (!m_isLeverOn)
			{
				LeverOnAnimations();
			}
			else
			{
				LeverOffAnimations();
			}
		}
		if (InputCollection.GetMouseButtonUp(0) && IsOver(m_Knob.gameObject))
		{
			if (!m_isKnobOn)
			{
				KnobOnAnimations();
			}
			else
			{
				KnobOffAnimations();
			}
		}
	}

	private void KnobOnAnimations()
	{
		if (!m_Knob.FsmVariables.GetFsmBool("knobAnimating").Value)
		{
			m_Knob.SendEvent("KnobTurnedOn");
			m_isKnobOn = true;
			iTween.Stop(base.gameObject);
			float curHeatGlowIntensity = m_HeatGlowMat.GetFloat("_Intensity");
			Hashtable args = iTweenManager.Get().GetTweenHashTable();
			args.Add("from", curHeatGlowIntensity);
			args.Add("to", 2f);
			args.Add("time", 2f * ((2f - curHeatGlowIntensity) / 2f));
			args.Add("easetype", iTween.EaseType.easeInOutCubic);
			args.Add("onupdate", (Action<object>)delegate(object newVal)
			{
				MaterialValueTo(m_HeatGlowMat, "_Intensity", (float)newVal);
			});
			iTween.ValueTo(base.gameObject, args);
			float curBubbleRate = GetEmissionRate(m_BubbleFX);
			Hashtable args2 = iTweenManager.Get().GetTweenHashTable();
			args2.Add("from", curBubbleRate);
			args2.Add("to", 50f);
			args2.Add("time", 3f * ((50f - curBubbleRate) / 50f));
			args2.Add("easetype", iTween.EaseType.easeInOutCubic);
			args2.Add("onupdate", (Action<object>)delegate(object newVal)
			{
				BubbleRate((float)newVal);
			});
			iTween.ValueTo(base.gameObject, args2);
		}
	}

	private void KnobOffAnimations()
	{
		if (!m_Knob.FsmVariables.GetFsmBool("knobAnimating").Value)
		{
			m_Knob.SendEvent("KnobTurnedOff");
			m_isKnobOn = false;
			iTween.Stop(base.gameObject);
			float curHeatGlowIntensity = m_HeatGlowMat.GetFloat("_Intensity");
			Hashtable args = iTweenManager.Get().GetTweenHashTable();
			args.Add("from", curHeatGlowIntensity);
			args.Add("to", 0f);
			args.Add("time", 2f * (curHeatGlowIntensity / 2f));
			args.Add("easetype", iTween.EaseType.easeInOutCubic);
			args.Add("onupdate", (Action<object>)delegate(object newVal)
			{
				MaterialValueTo(m_HeatGlowMat, "_Intensity", (float)newVal);
			});
			iTween.ValueTo(base.gameObject, args);
			float curBubbleRate = GetEmissionRate(m_BubbleFX);
			Hashtable args2 = iTweenManager.Get().GetTweenHashTable();
			args2.Add("from", curBubbleRate);
			args2.Add("to", 0f);
			args2.Add("time", 1f * (curBubbleRate / 50f));
			args2.Add("easetype", iTween.EaseType.easeInOutCubic);
			args2.Add("onupdate", (Action<object>)delegate(object newVal)
			{
				BubbleRate((float)newVal);
			});
			iTween.ValueTo(base.gameObject, args2);
		}
	}

	private void LeverOnAnimations()
	{
		if (!m_Lever.FsmVariables.GetFsmBool("leverAnimating").Value)
		{
			m_Lever.SendEvent("LeverTurnedOn");
			m_isLeverOn = true;
			iTween.Stop(base.gameObject);
			float curIntensity = m_CurlTubeMat.GetFloat("_Intensity");
			float curUV = m_CurlTubeMat.GetFloat("_UVOffsetSecondX");
			Hashtable args = iTweenManager.Get().GetTweenHashTable();
			args.Add("from", curIntensity);
			args.Add("to", 8f);
			args.Add("time", 1f * ((8f - curIntensity) / 8f));
			args.Add("easetype", iTween.EaseType.easeInOutCubic);
			args.Add("onupdate", (Action<object>)delegate(object newVal)
			{
				MaterialValueTo(m_CurlTubeMat, "_Intensity", (float)newVal);
			});
			iTween.ValueTo(base.gameObject, args);
			Hashtable args2 = iTweenManager.Get().GetTweenHashTable();
			args2.Add("from", curUV);
			args2.Add("to", 0.8f);
			args2.Add("time", 1f);
			args2.Add("easetype", iTween.EaseType.linear);
			args2.Add("onupdate", (Action<object>)delegate(object newVal)
			{
				MaterialValueTo(m_CurlTubeMat, "_UVOffsetSecondX", (float)newVal);
			});
			iTween.ValueTo(base.gameObject, args2);
			float curSmallGlobeTransition = m_SmallGlobeMat.GetFloat("_Transistion");
			Hashtable args3 = iTweenManager.Get().GetTweenHashTable();
			args3.Add("from", curSmallGlobeTransition);
			args3.Add("to", 1f);
			args3.Add("time", 3f * ((1f - curSmallGlobeTransition) / 1f));
			args3.Add("delay", 1f);
			args3.Add("easetype", iTween.EaseType.easeInOutCubic);
			args3.Add("onupdate", (Action<object>)delegate(object newVal)
			{
				MaterialValueTo(m_SmallGlobeMat, "_Transistion", (float)newVal);
			});
			iTween.ValueTo(base.gameObject, args3);
			float curBeakerTransition = m_BeakerMat.GetFloat("_Transistion");
			Hashtable args4 = iTweenManager.Get().GetTweenHashTable();
			args4.Add("from", curBeakerTransition);
			args4.Add("to", 1f);
			args4.Add("time", 3f * ((1f - curBeakerTransition) / 1f));
			args4.Add("delay", 1f);
			args4.Add("easetype", iTween.EaseType.easeInOutCubic);
			args4.Add("onupdate", (Action<object>)delegate(object newVal)
			{
				MaterialValueTo(m_BeakerMat, "_Transistion", (float)newVal);
			});
			iTween.ValueTo(base.gameObject, args4);
		}
	}

	public float GetEmissionRate(ParticleSystem particleSystem)
	{
		return particleSystem.emission.rateOverTime.constantMax;
	}

	public void SetEmissionRate(ParticleSystem particleSystem, float emissionRate)
	{
		ParticleSystem.EmissionModule emission = particleSystem.emission;
		ParticleSystem.MinMaxCurve rate = emission.rateOverTime;
		rate.constantMax = emissionRate;
		emission.rateOverTime = rate;
	}

	private void LeverOffAnimations(bool hasScience = true)
	{
		if (m_Lever.FsmVariables.GetFsmBool("leverAnimating").Value)
		{
			return;
		}
		if (m_isKnobOn && m_isLeverOn && m_BeakerMat.GetFloat("_Transistion") == 1f && hasScience)
		{
			BlindMeWithScience();
		}
		else
		{
			m_Lever.SendEvent("LeverTurnedOff");
			iTween.Stop(base.gameObject);
			float curIntensity = m_CurlTubeMat.GetFloat("_Intensity");
			float curUV = m_CurlTubeMat.GetFloat("_UVOffsetSecondX");
			Hashtable args = iTweenManager.Get().GetTweenHashTable();
			args.Add("from", curIntensity);
			args.Add("to", 0f);
			args.Add("time", 1f * (curIntensity / 8f));
			args.Add("easetype", iTween.EaseType.easeInOutCubic);
			args.Add("onupdate", (Action<object>)delegate(object newVal)
			{
				MaterialValueTo(m_CurlTubeMat, "_Intensity", (float)newVal);
			});
			iTween.ValueTo(base.gameObject, args);
			Hashtable args2 = iTweenManager.Get().GetTweenHashTable();
			args2.Add("from", curUV);
			args2.Add("to", -0.8f);
			args2.Add("time", 1f);
			args2.Add("easetype", iTween.EaseType.linear);
			args2.Add("onupdate", (Action<object>)delegate(object newVal)
			{
				MaterialValueTo(m_CurlTubeMat, "_UVOffsetSecondX", (float)newVal);
			});
			iTween.ValueTo(base.gameObject, args2);
			float curSmallGlobeTransition = m_SmallGlobeMat.GetFloat("_Transistion");
			Hashtable args3 = iTweenManager.Get().GetTweenHashTable();
			args3.Add("from", curSmallGlobeTransition);
			args3.Add("to", 0f);
			args3.Add("time", 4f * (curSmallGlobeTransition / 1f));
			args3.Add("easetype", iTween.EaseType.easeInOutCubic);
			args3.Add("onupdate", (Action<object>)delegate(object newVal)
			{
				MaterialValueTo(m_SmallGlobeMat, "_Transistion", (float)newVal);
			});
			iTween.ValueTo(base.gameObject, args3);
			float curBeakerTransition = m_BeakerMat.GetFloat("_Transistion");
			Hashtable args4 = iTweenManager.Get().GetTweenHashTable();
			args4.Add("from", curBeakerTransition);
			args4.Add("to", 0f);
			args4.Add("time", 4f * (curBeakerTransition / 1f));
			args4.Add("easetype", iTween.EaseType.easeInOutCubic);
			args4.Add("onupdate", (Action<object>)delegate(object newVal)
			{
				MaterialValueTo(m_BeakerMat, "_Transistion", (float)newVal);
			});
			iTween.ValueTo(base.gameObject, args4);
		}
		m_isLeverOn = false;
	}

	private void BlindMeWithScience()
	{
		m_Lever.FsmVariables.GetFsmBool("doPoof").Value = true;
		LeverOffAnimations(hasScience: false);
	}

	private void MaterialValueTo(Material mat, string property, float newVal)
	{
		mat.SetFloat(property, newVal);
	}

	private void BubbleRate(float newVal)
	{
		SetEmissionRate(m_BubbleFX, newVal);
	}

	private bool IsOver(GameObject go)
	{
		if (!go)
		{
			return false;
		}
		if (!InputUtil.IsPlayMakerMouseInputAllowed(go))
		{
			return false;
		}
		if (!UniversalInputManager.Get().InputIsOver(go))
		{
			return false;
		}
		return true;
	}
}
