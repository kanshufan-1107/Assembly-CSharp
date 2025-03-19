using System;
using System.Collections;
using UnityEngine;

namespace Hearthstone.FX;

public class ObjectShaker : MonoBehaviour
{
	public enum TweenType
	{
		Linear,
		Sine,
		DoubleSine,
		Constant
	}

	private const int RANDOM_TRANFORMS_COUNT = 10;

	private const float HALF_PI = (float)Math.PI / 2f;

	private Vector3? m_originalPosition;

	private Vector3? m_originalRotation;

	private Space m_space = Space.Self;

	public static void Shake(GameObject target, Vector3 position, Vector3 rotation, Space space, AnimationCurve falloff, float duration, float interval, TweenType tweenType, bool destroyHelperOnComplete = false)
	{
		ObjectShaker helper = target.GetComponent<ObjectShaker>();
		if (helper != null)
		{
			helper.CancelShake();
		}
		else
		{
			helper = target.AddComponent<ObjectShaker>();
		}
		helper.StartCoroutine(helper.UpdateShake(position, rotation, space, falloff, duration, interval, tweenType, destroyHelperOnComplete));
	}

	public static void Cancel(GameObject target, bool andDestroy = false)
	{
		ObjectShaker helper = target.GetComponent<ObjectShaker>();
		if (helper != null)
		{
			helper.CancelShake();
			if (andDestroy)
			{
				UnityEngine.Object.Destroy(helper);
			}
		}
	}

	private IEnumerator UpdateShake(Vector3 position, Vector3 rotation, Space space, AnimationCurve falloff, float duration, float interval, TweenType tweenType, bool destroyHelperOnComplete)
	{
		Vector3[] positions = new Vector3[10];
		Vector3[] rotations = new Vector3[10];
		for (int i = 0; i < 10; i++)
		{
			positions[i] = new Vector3(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f)).normalized;
			positions[i].x *= position.x;
			positions[i].y *= position.y;
			positions[i].z *= position.z;
			rotations[i] = new Vector3
			{
				x = UnityEngine.Random.Range(0f - rotation.x, rotation.x),
				y = UnityEngine.Random.Range(0f - rotation.y, rotation.y),
				z = UnityEngine.Random.Range(0f - rotation.z, rotation.z)
			};
		}
		int randomTransformIndex = 0;
		float shakeStartTime = Time.time;
		float intervalStartTime = Time.time;
		float falloffEval = falloff.Evaluate(0f);
		m_space = space;
		m_originalPosition = ((m_space == Space.Self) ? base.transform.localPosition : base.transform.position);
		m_originalRotation = ((m_space == Space.Self) ? base.transform.localEulerAngles : base.transform.eulerAngles);
		Vector3 previousPosition = positions[positions.Length - 1] * falloffEval;
		Vector3 nextPosition = positions[0] * falloffEval;
		Vector3 previousRotation = rotations[rotations.Length - 1] * falloffEval;
		Vector3 nextRotation = rotations[0] * falloffEval;
		while (Time.time - shakeStartTime < duration)
		{
			float intervalTime = (Time.time - intervalStartTime) / interval;
			if (intervalTime >= 1f)
			{
				randomTransformIndex = (randomTransformIndex + 1) % 10;
				falloffEval = falloff.Evaluate((Time.time - shakeStartTime) / duration);
				previousPosition = nextPosition;
				nextPosition = positions[randomTransformIndex] * falloffEval;
				previousRotation = nextRotation;
				nextRotation = rotations[randomTransformIndex] * falloffEval;
				intervalTime = 0f;
				intervalStartTime = Time.time;
			}
			intervalTime = ConvertTween(intervalTime, tweenType);
			UpdateTransform(previousPosition, nextPosition, previousRotation, nextRotation, intervalTime);
			yield return new WaitForEndOfFrame();
		}
		previousPosition = nextPosition;
		nextPosition = Vector3.zero;
		previousRotation = nextRotation;
		nextRotation = Vector3.zero;
		intervalStartTime = Time.time;
		while (Time.time - intervalStartTime < interval)
		{
			float intervalTime = (Time.time - intervalStartTime) / interval;
			intervalTime = ConvertTween(intervalTime, tweenType);
			UpdateTransform(previousPosition, nextPosition, previousRotation, nextRotation, intervalTime);
			yield return new WaitForEndOfFrame();
		}
		SetPosition(m_originalPosition.Value);
		SetRotation(m_originalRotation.Value);
		m_originalPosition = null;
		m_originalRotation = null;
		if (destroyHelperOnComplete)
		{
			UnityEngine.Object.Destroy(this);
		}
	}

	private void CancelShake()
	{
		StopAllCoroutines();
		if (m_originalPosition.HasValue && m_originalRotation.HasValue)
		{
			SetPosition(m_originalPosition.Value);
			SetRotation(m_originalRotation.Value);
		}
	}

	private void UpdateTransform(Vector3 previousPosition, Vector3 nextPosition, Vector3 previousRotation, Vector3 nextRotation, float time)
	{
		if (m_originalPosition.HasValue && m_originalRotation.HasValue)
		{
			SetPosition(m_originalPosition.Value + Vector3.Lerp(previousPosition, nextPosition, time));
			SetRotation(m_originalRotation.Value + LerpVector3Angles(previousRotation, nextRotation, time));
		}
	}

	private void SetPosition(Vector3 position)
	{
		if (m_space == Space.Self)
		{
			base.transform.localPosition = position;
		}
		else
		{
			base.transform.position = position;
		}
	}

	private void SetRotation(Vector3 eulerAngles)
	{
		if (m_space == Space.Self)
		{
			base.transform.localEulerAngles = eulerAngles;
		}
		else
		{
			base.transform.eulerAngles = eulerAngles;
		}
	}

	private static Vector3 LerpVector3Angles(Vector3 a, Vector3 b, float t)
	{
		Vector3 result = default(Vector3);
		result.x = Mathf.LerpAngle(a.x, b.x, t);
		result.y = Mathf.LerpAngle(a.y, b.y, t);
		result.z = Mathf.LerpAngle(a.z, b.z, t);
		return result;
	}

	private static float ConvertTween(float time, TweenType tweenType)
	{
		switch (tweenType)
		{
		case TweenType.Constant:
			return 0f;
		case TweenType.Sine:
			return Mathf.Sin(time * (float)Math.PI - (float)Math.PI / 2f) * 0.5f + 0.5f;
		case TweenType.DoubleSine:
			time = Mathf.Sin(time * (float)Math.PI - (float)Math.PI / 2f) * 0.5f + 0.5f;
			return Mathf.Sin(time * (float)Math.PI - (float)Math.PI / 2f) * 0.5f + 0.5f;
		default:
			return time;
		}
	}
}
