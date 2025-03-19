using System.Collections;
using UnityEngine;

public class AnimPositionResetter : MonoBehaviour
{
	private Vector3 m_initialPosition;

	private float m_endTimestamp;

	private float m_delay;

	private void Awake()
	{
		m_initialPosition = base.transform.position;
	}

	public static AnimPositionResetter OnAnimStarted(GameObject go, float animTime)
	{
		if (animTime <= 0f)
		{
			return null;
		}
		AnimPositionResetter animPositionResetter = RegisterResetter(go);
		animPositionResetter.OnAnimStarted(animTime);
		return animPositionResetter;
	}

	public Vector3 GetInitialPosition()
	{
		return m_initialPosition;
	}

	public float GetEndTimestamp()
	{
		return m_endTimestamp;
	}

	public float GetDelay()
	{
		return m_delay;
	}

	private static AnimPositionResetter RegisterResetter(GameObject go)
	{
		if (go == null)
		{
			return null;
		}
		AnimPositionResetter resetter = go.GetComponent<AnimPositionResetter>();
		if (resetter != null)
		{
			return resetter;
		}
		return go.AddComponent<AnimPositionResetter>();
	}

	private void OnAnimStarted(float animTime)
	{
		float endTimestamp = Time.realtimeSinceStartup + animTime;
		float addedDelay = endTimestamp - m_endTimestamp;
		if (!(addedDelay <= 0f))
		{
			m_delay = Mathf.Min(addedDelay, animTime);
			m_endTimestamp = endTimestamp;
			StopCoroutine("ResetPosition");
			StartCoroutine("ResetPosition");
		}
	}

	private IEnumerator ResetPosition()
	{
		yield return new WaitForSeconds(m_delay);
		base.transform.position = m_initialPosition;
		Object.Destroy(this);
	}
}
