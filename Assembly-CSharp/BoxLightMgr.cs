using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxLightMgr : MonoBehaviour
{
	public List<BoxLightState> m_States;

	private BoxLightStateType m_activeStateType = BoxLightStateType.DEFAULT;

	private void Start()
	{
		UpdateState();
	}

	public BoxLightStateType GetActiveState()
	{
		return m_activeStateType;
	}

	public void ChangeState(BoxLightStateType stateType)
	{
		if (stateType != 0 && m_activeStateType != stateType)
		{
			ChangeStateImpl(stateType);
		}
	}

	public void SetState(BoxLightStateType stateType)
	{
		if (m_activeStateType != stateType)
		{
			m_activeStateType = stateType;
			UpdateState();
		}
	}

	public void UpdateState()
	{
		BoxLightState state = FindState(m_activeStateType);
		if (state == null)
		{
			return;
		}
		state.m_Spell.ActivateState(SpellStateType.ACTION);
		iTween.Stop(base.gameObject);
		RenderSettings.ambientLight = state.m_AmbientColor;
		if (state.m_LightInfos == null)
		{
			return;
		}
		foreach (BoxLightInfo lightInfo in state.m_LightInfos)
		{
			iTween.Stop(lightInfo.m_Light.gameObject);
			lightInfo.m_Light.color = lightInfo.m_Color;
			lightInfo.m_Light.intensity = lightInfo.m_Intensity;
			LightType lightType = lightInfo.m_Light.type;
			if (lightType == LightType.Point || lightType == LightType.Spot)
			{
				lightInfo.m_Light.range = lightInfo.m_Range;
				if (lightType == LightType.Spot)
				{
					lightInfo.m_Light.spotAngle = lightInfo.m_SpotAngle;
				}
			}
		}
	}

	private BoxLightState FindState(BoxLightStateType stateType)
	{
		foreach (BoxLightState state in m_States)
		{
			if (state.m_Type == stateType)
			{
				return state;
			}
		}
		return null;
	}

	private void ChangeStateImpl(BoxLightStateType stateType)
	{
		m_activeStateType = stateType;
		BoxLightState state = FindState(stateType);
		if (state == null)
		{
			return;
		}
		iTween.Stop(base.gameObject);
		state.m_Spell.ActivateState(SpellStateType.BIRTH);
		ChangeAmbient(state);
		if (state.m_LightInfos == null)
		{
			return;
		}
		foreach (BoxLightInfo lightInfo in state.m_LightInfos)
		{
			ChangeLight(state, lightInfo);
		}
	}

	private void ChangeAmbient(BoxLightState state)
	{
		Color prevAmbientColor = RenderSettings.ambientLight;
		Action<object> OnAmbientColorUpdate = delegate(object amount)
		{
			RenderSettings.ambientLight = Color.Lerp(prevAmbientColor, state.m_AmbientColor, (float)amount);
		};
		Hashtable ambientArgs = iTweenManager.Get().GetTweenHashTable();
		ambientArgs.Add("from", 0f);
		ambientArgs.Add("to", 1f);
		ambientArgs.Add("delay", state.m_DelaySec);
		ambientArgs.Add("time", state.m_TransitionSec);
		ambientArgs.Add("easetype", state.m_TransitionEaseType);
		ambientArgs.Add("onupdate", OnAmbientColorUpdate);
		iTween.ValueTo(base.gameObject, ambientArgs);
	}

	private void ChangeLight(BoxLightState state, BoxLightInfo lightInfo)
	{
		iTween.Stop(lightInfo.m_Light.gameObject);
		Hashtable colorArgs = iTweenManager.Get().GetTweenHashTable();
		colorArgs.Add("color", lightInfo.m_Color);
		colorArgs.Add("delay", state.m_DelaySec);
		colorArgs.Add("time", state.m_TransitionSec);
		colorArgs.Add("easetype", state.m_TransitionEaseType);
		iTween.ColorTo(lightInfo.m_Light.gameObject, colorArgs);
		float intensity = lightInfo.m_Light.intensity;
		Action<object> OnIntensityUpdate = delegate(object amount)
		{
			lightInfo.m_Light.intensity = (float)amount;
		};
		Hashtable intensityArgs = iTweenManager.Get().GetTweenHashTable();
		intensityArgs.Add("from", intensity);
		intensityArgs.Add("to", lightInfo.m_Intensity);
		intensityArgs.Add("delay", state.m_DelaySec);
		intensityArgs.Add("time", state.m_TransitionSec);
		intensityArgs.Add("easetype", state.m_TransitionEaseType);
		intensityArgs.Add("onupdate", OnIntensityUpdate);
		iTween.ValueTo(lightInfo.m_Light.gameObject, intensityArgs);
		LightType lightType = lightInfo.m_Light.type;
		if (lightType != LightType.Point && lightType != 0)
		{
			return;
		}
		float range = lightInfo.m_Light.range;
		Action<object> OnRangeUpdate = delegate(object amount)
		{
			lightInfo.m_Light.range = (float)amount;
		};
		Hashtable rangeArgs = iTweenManager.Get().GetTweenHashTable();
		rangeArgs.Add("from", range);
		rangeArgs.Add("to", lightInfo.m_Range);
		rangeArgs.Add("delay", state.m_DelaySec);
		rangeArgs.Add("time", state.m_TransitionSec);
		rangeArgs.Add("easetype", state.m_TransitionEaseType);
		rangeArgs.Add("onupdate", OnRangeUpdate);
		iTween.ValueTo(lightInfo.m_Light.gameObject, rangeArgs);
		if (lightType == LightType.Spot)
		{
			float spotAngle = lightInfo.m_Light.spotAngle;
			Action<object> OnSpotAngleUpdate = delegate(object amount)
			{
				lightInfo.m_Light.spotAngle = (float)amount;
			};
			Hashtable spotAngleArgs = iTweenManager.Get().GetTweenHashTable();
			spotAngleArgs.Add("from", spotAngle);
			spotAngleArgs.Add("to", lightInfo.m_SpotAngle);
			spotAngleArgs.Add("delay", state.m_DelaySec);
			spotAngleArgs.Add("time", state.m_TransitionSec);
			spotAngleArgs.Add("easetype", state.m_TransitionEaseType);
			spotAngleArgs.Add("onupdate", OnSpotAngleUpdate);
			iTween.ValueTo(lightInfo.m_Light.gameObject, spotAngleArgs);
		}
	}
}
