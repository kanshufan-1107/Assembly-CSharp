using System;
using System.Collections;
using Blizzard.T5.MaterialService.Extensions;
using UnityEngine;

public class AnimationUtil : MonoBehaviour
{
	public delegate void DelOnShownWithPunch(object callbackData);

	public delegate void DelOnFade();

	private class PunchData
	{
		public GameObject m_gameObject;

		public Vector3 m_scale;

		public string m_callbackName;

		public GameObject m_callbackGameObject;

		public object m_callbackData;

		public DelOnShownWithPunch m_onShowPunchCallback;
	}

	public static void ShowWithPunch(GameObject go, Vector3 startScale, Vector3 punchScale, Vector3 afterPunchScale, string callbackName = "", bool noFade = false, GameObject callbackGO = null, object callbackData = null, DelOnShownWithPunch onShowPunchCallback = null)
	{
		if (!noFade)
		{
			iTween.FadeTo(go, 1f, 0.25f);
		}
		go.transform.localScale = startScale;
		iTween.ScaleTo(go, iTween.Hash("scale", punchScale, "time", 0.25f));
		iTween.MoveTo(go, iTween.Hash("position", go.transform.position + new Vector3(0.02f, 0.02f, 0.02f), "time", 1.5f));
		PunchData punchData = new PunchData
		{
			m_gameObject = go,
			m_scale = afterPunchScale,
			m_callbackName = callbackName,
			m_callbackGameObject = callbackGO,
			m_callbackData = callbackData,
			m_onShowPunchCallback = onShowPunchCallback
		};
		go.GetComponent<MonoBehaviour>().StartCoroutine(ShowPunchRoutine(punchData));
	}

	private static IEnumerator ShowPunchRoutine(PunchData callbackData)
	{
		yield return new WaitForSeconds(0.25f);
		ShowPunch(callbackData.m_gameObject, callbackData.m_scale, callbackData.m_callbackName, callbackData.m_callbackGameObject, callbackData.m_callbackData);
		if (callbackData.m_onShowPunchCallback != null)
		{
			callbackData.m_onShowPunchCallback(callbackData.m_callbackData);
		}
	}

	public static void ShowPunch(GameObject go, Vector3 scale, string callbackName = "", GameObject callbackGO = null, object callbackData = null)
	{
		if (string.IsNullOrEmpty(callbackName))
		{
			iTween.ScaleTo(go, scale, 0.15f);
			return;
		}
		if (callbackGO == null)
		{
			callbackGO = go;
		}
		if (callbackData == null)
		{
			callbackData = new object();
		}
		Hashtable scaleArgs = iTweenManager.Get().GetTweenHashTable();
		scaleArgs.Add("scale", scale);
		scaleArgs.Add("time", 0.15f);
		scaleArgs.Add("oncomplete", callbackName);
		scaleArgs.Add("oncompletetarget", callbackGO);
		scaleArgs.Add("oncompleteparams", callbackData);
		iTween.ScaleTo(go, scaleArgs);
	}

	public static void GrowThenDrift(GameObject go, Vector3 origin, Vector3 driftOffset)
	{
		iTween.ScaleFrom(go, iTween.Hash("scale", Vector3.one * 0.05f, "time", 0.15f, "easetype", iTween.EaseType.easeOutQuart));
		Hashtable args = iTweenManager.Get().GetTweenHashTable();
		args.Add("position", origin);
		args.Add("time", 0.15f);
		args.Add("easetype", iTween.EaseType.easeOutQuart);
		iTween.MoveFrom(go, args);
		go.GetComponent<MonoBehaviour>().StartCoroutine(DriftAfterTween(go, 0.15f, driftOffset));
	}

	public static void GrowThenDrift(GameObject go, Vector3 origin, float driftScale)
	{
		Vector3 obj = (PlatformSettings.IsTablet ? new Vector3(0f, 0.1f, 0.1f) : new Vector3(0.1f, 0.1f, 0.1f));
		Vector3 animOffsetInWorldSpace = Vector3.Scale(b: TransformUtil.ComputeWorldScale(go.transform.parent), a: obj * driftScale);
		GrowThenDrift(go, origin, animOffsetInWorldSpace);
	}

	private static IEnumerator DriftAfterTween(GameObject go, float delayTime, Vector3 driftOffset)
	{
		yield return new WaitForSeconds(delayTime);
		DriftObject(go, driftOffset);
	}

	public static void FloatyPosition(GameObject go, Vector3 startPos, float localRadius, float loopTime)
	{
		Vector3[] path = new Vector3[5]
		{
			startPos,
			startPos + new Vector3(localRadius, 0f, localRadius),
			startPos + new Vector3(localRadius * 2f, 0f, 0f),
			startPos + new Vector3(localRadius, 0f, 0f - localRadius),
			startPos + Vector3.zero
		};
		iTween.StopByName("DriftingTween");
		iTween.MoveTo(go, iTween.Hash("name", "DriftingTween", "path", path, "time", loopTime, "islocal", true, "easetype", iTween.EaseType.linear, "looptype", iTween.LoopType.loop, "movetopath", false));
	}

	public static void FloatyPosition(GameObject go, float radius, float loopTime)
	{
		FloatyPosition(go, go.transform.localPosition, radius, loopTime);
	}

	public static void ScaleFade(GameObject go, Vector3 scale)
	{
		ScaleFade(go, scale, null);
	}

	public static void ScaleFade(GameObject go, Vector3 scale, string callbackName)
	{
		iTween.FadeTo(go, 0f, 0.25f);
		Hashtable scaleArgs;
		if (string.IsNullOrEmpty(callbackName))
		{
			scaleArgs = iTweenManager.Get().GetTweenHashTable();
			scaleArgs.Add("scale", scale);
			scaleArgs.Add("time", 0.25f);
		}
		else
		{
			scaleArgs = iTweenManager.Get().GetTweenHashTable();
			scaleArgs.Add("scale", scale);
			scaleArgs.Add("time", 0.25f);
			scaleArgs.Add("oncomplete", callbackName);
			scaleArgs.Add("oncompletetarget", go);
		}
		iTween.ScaleTo(go, scaleArgs);
	}

	public static int GetLayerIndexFromName(Animator animator, string layerName)
	{
		if (layerName == null)
		{
			return -1;
		}
		layerName = layerName.Trim();
		for (int i = 0; i < animator.layerCount; i++)
		{
			string currLayerName = animator.GetLayerName(i);
			if (currLayerName != null)
			{
				currLayerName = currLayerName.Trim();
				if (currLayerName.Equals(layerName, StringComparison.OrdinalIgnoreCase))
				{
					return i;
				}
			}
		}
		return -1;
	}

	public static void DriftObject(GameObject go, Vector3 driftOffset)
	{
		iTween.StopByName(go, "DRIFT_MOVE_OBJECT_ITWEEN");
		iTween.MoveBy(go, iTween.Hash("amount", driftOffset, "time", 10f, "name", "DRIFT_MOVE_OBJECT_ITWEEN", "easetype", iTween.EaseType.easeOutQuart));
	}

	public static void FadeTexture(MeshRenderer mesh, float fromAlpha, float toAlpha, float fadeTime, float delay, DelOnFade onCompleteCallback = null)
	{
		iTween.StopByName(mesh.gameObject, "FADE_TEXTURE");
		Material logoMaterial = mesh.GetMaterial();
		Color currentColor = logoMaterial.GetColor("_Color");
		currentColor.a = fromAlpha;
		logoMaterial.SetColor("_Color", currentColor);
		Hashtable tweenArgs = iTweenManager.Get().GetTweenHashTable();
		tweenArgs.Add("from", fromAlpha);
		tweenArgs.Add("to", toAlpha);
		tweenArgs.Add("time", fadeTime);
		tweenArgs.Add("onupdate", (Action<object>)delegate(object val)
		{
			currentColor.a = (float)val;
			logoMaterial.SetColor("_Color", currentColor);
		});
		tweenArgs.Add("name", "FADE_TEXTURE");
		if (delay > 0f)
		{
			tweenArgs.Add("delay", delay);
		}
		if (onCompleteCallback != null)
		{
			tweenArgs.Add("oncomplete", (Action<object>)delegate
			{
				onCompleteCallback();
			});
		}
		iTween.ValueTo(mesh.gameObject, tweenArgs);
	}

	public static void DelayedActivate(GameObject go, float time, bool activate)
	{
		go.GetComponent<MonoBehaviour>().StartCoroutine(DelayedActivation(go, time, activate));
	}

	private static IEnumerator DelayedActivation(GameObject go, float time, bool activate)
	{
		yield return new WaitForSeconds(time);
		go.SetActive(activate);
	}
}
