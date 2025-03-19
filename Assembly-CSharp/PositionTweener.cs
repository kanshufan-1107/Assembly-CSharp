using System;
using System.Collections;
using UnityEngine;

[Serializable]
public class PositionTweener
{
	[SerializeField]
	private string m_name = "position";

	[SerializeField]
	private bool m_isLocal = true;

	[SerializeField]
	private Vector3 m_initialPosition = Vector3.zero;

	[SerializeField]
	private Vector3 m_finalPosition = Vector3.zero;

	[SerializeField]
	private float m_time = 1f;

	[SerializeField]
	private iTween.EaseType m_easeType = iTween.EaseType.easeOutQuad;

	[SerializeField]
	[Header("Callback")]
	private string m_onCompleteCallbackMethodName;

	[SerializeField]
	private GameObject m_onCompleteGameObject;

	public bool IsLocal => m_isLocal;

	public Vector3 InitialPosition => m_initialPosition;

	public Vector3 FinalPosition => m_finalPosition;

	public void Play(GameObject target, bool forward)
	{
		Hashtable args = iTweenManager.Get().GetTweenHashTable();
		args.Add("name", m_name);
		args.Add("position", forward ? m_finalPosition : m_initialPosition);
		args.Add("islocal", m_isLocal);
		args.Add("time", m_time);
		args.Add("easetype", m_easeType);
		if (!string.IsNullOrWhiteSpace(m_onCompleteCallbackMethodName) && m_onCompleteGameObject != null)
		{
			args.Add("oncomplete", m_onCompleteCallbackMethodName);
			args.Add("oncompletetarget", m_onCompleteGameObject);
		}
		iTween.MoveTo(target, args);
	}

	public PositionTweener SetInitialPosition(Vector3 initialPosition)
	{
		m_initialPosition = initialPosition;
		return this;
	}

	public PositionTweener SetFinalPosition(Vector3 targetPosition)
	{
		m_finalPosition = targetPosition;
		return this;
	}
}
