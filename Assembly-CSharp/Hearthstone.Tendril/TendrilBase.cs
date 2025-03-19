using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Hearthstone.Tendril;

public abstract class TendrilBase : MonoBehaviour
{
	[Serializable]
	public class TendrilPoint
	{
		public Vector3 currentPosition;

		public Vector3 previousPosition;

		public float length;
	}

	public enum UpdateMode
	{
		Update,
		LateUpdate
	}

	public UpdateMode updateMode;

	public Transform target;

	public Transform pole;

	public List<Transform> bones = new List<Transform>();

	public List<Vector3> cachePositions = new List<Vector3>();

	public List<Quaternion> cacheRotations = new List<Quaternion>();

	public float weight = 1f;

	public float leafWeight = 1f;

	public float controlWeight = 1f;

	public float speed = 25f;

	public int chainLength = 2;

	public int priority = 4;

	public bool autonomous;

	public bool initialized;

	public bool showGizmos;

	protected Transform m_rootBone;

	private IEnumerator m_animateRoutine;

	private void OnEnable()
	{
		if (autonomous)
		{
			RenderPipelineManager.endFrameRendering += EndFrameRendering;
		}
	}

	private void OnDisable()
	{
		if (autonomous)
		{
			RenderPipelineManager.endFrameRendering -= EndFrameRendering;
		}
	}

	public void Initialize()
	{
		if (InitializeConditions())
		{
			bones = TendrilUtilities.CollectBones(base.transform, chainLength, out var root);
			for (int i = 0; i < bones.Count; i++)
			{
				cacheRotations.Add(bones[i].localRotation);
				cachePositions.Add(bones[i].position);
			}
			m_rootBone = root;
			InitializeTendril();
		}
	}

	protected virtual bool InitializeConditions()
	{
		return true;
	}

	protected virtual void InitializeTendril()
	{
		initialized = true;
	}

	public void Cache()
	{
		if (initialized)
		{
			CacheTendril();
		}
	}

	protected virtual void CacheTendril()
	{
		for (int i = 0; i < bones.Count; i++)
		{
			cacheRotations[i] = bones[i].localRotation;
			cachePositions[i] = bones[i].position;
		}
	}

	public void Resolve()
	{
		if (initialized)
		{
			ResolveTendril();
		}
		else
		{
			Initialize();
		}
	}

	protected virtual void ResolveTendril()
	{
	}

	public void PostRender()
	{
		if (initialized)
		{
			PostRenderTendril();
		}
	}

	protected virtual void PostRenderTendril()
	{
		for (int i = 0; i < bones.Count; i++)
		{
			bones[i].localRotation = cacheRotations[i];
		}
	}

	public void AnimateWeight(float targetWeight, float time, float delay = 0f)
	{
		if (m_animateRoutine != null)
		{
			StopCoroutine(m_animateRoutine);
		}
		m_animateRoutine = AnimateWeightRoutine(targetWeight, time, delay);
		StartCoroutine(m_animateRoutine);
	}

	public void AnimateWeight(AnimationCurve curve, float time, float delay = 0f)
	{
		if (m_animateRoutine != null)
		{
			StopCoroutine(m_animateRoutine);
		}
		m_animateRoutine = AnimateWeightRoutine(curve, time, delay);
		StartCoroutine(m_animateRoutine);
	}

	private IEnumerator AnimateWeightRoutine(float targetWeight, float time, float delay = 0f)
	{
		yield return new WaitForSeconds(delay);
		float countdown = 1f;
		float startWeight = controlWeight;
		while (countdown > 0f)
		{
			countdown -= Time.deltaTime / time;
			controlWeight = Mathf.Lerp(startWeight, targetWeight, 1f - countdown);
			yield return null;
		}
	}

	private IEnumerator AnimateWeightRoutine(AnimationCurve curve, float time, float delay = 0f)
	{
		yield return new WaitForSeconds(delay);
		float countdown = 1f;
		while (countdown > 0f)
		{
			countdown -= Time.deltaTime / time;
			controlWeight = curve.Evaluate(1f - countdown);
			yield return null;
		}
	}

	private void Update()
	{
		if (autonomous && updateMode == UpdateMode.Update)
		{
			PostRender();
			Cache();
			Resolve();
		}
	}

	protected void LateUpdate()
	{
		if (autonomous && updateMode == UpdateMode.LateUpdate)
		{
			Cache();
			Resolve();
		}
	}

	private void EndFrameRendering(ScriptableRenderContext context, Camera[] cameras)
	{
		if (autonomous && updateMode == UpdateMode.LateUpdate)
		{
			PostRender();
		}
	}

	protected virtual void TendrilGizmos()
	{
	}
}
