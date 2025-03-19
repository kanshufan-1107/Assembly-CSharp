using System.Collections.Generic;
using UnityEngine;

namespace Hearthstone.FX;

[DisallowMultipleComponent]
[RequireComponent(typeof(Animator))]
public class YoggWheelPointerController : MonoBehaviour
{
	[Tooltip("Expecting a BOOL parameter that triggers a looping animation.")]
	[SerializeField]
	[Header("Animator Parameters")]
	private string m_loopingBool = "Loop";

	[Tooltip("Expecting a TRIGGER parameter that triggers a one-shot animation.")]
	[SerializeField]
	private string m_oneShotTrigger = "Tick";

	[Header("Segments")]
	public Transform m_wheelRoot;

	[Tooltip("Angles around the wheel's perimeter where the pointer's movement should be triggered.")]
	[SerializeField]
	private List<float> m_wheelSegments = new List<float>();

	private Animator m_animator;

	private int m_currentTriggerAngle;

	private bool m_lookingForOneShots;

	private void Start()
	{
		m_animator = GetComponent<Animator>();
		m_animator.SetBool(m_loopingBool, value: false);
		m_animator.ResetTrigger(m_oneShotTrigger);
		m_lookingForOneShots = false;
	}

	private void Update()
	{
		if (!m_lookingForOneShots)
		{
			return;
		}
		float currentRotation = m_wheelRoot.rotation.eulerAngles.y;
		if (m_currentTriggerAngle >= m_wheelSegments.Count)
		{
			if (currentRotation < m_wheelSegments[m_currentTriggerAngle - 1])
			{
				m_currentTriggerAngle = 0;
			}
		}
		else if (currentRotation >= m_wheelSegments[m_currentTriggerAngle])
		{
			m_animator.SetTrigger(m_oneShotTrigger);
			m_currentTriggerAngle++;
		}
	}

	public void StartLoopingAnimation()
	{
		m_animator.SetBool(m_loopingBool, value: true);
		m_lookingForOneShots = false;
	}

	public void StartLookingForOneShots()
	{
		m_animator.SetBool(m_loopingBool, value: false);
		m_lookingForOneShots = true;
		SetCurrentRotationSlot();
	}

	public void StopPointer()
	{
		m_animator.SetBool(m_loopingBool, value: false);
		m_lookingForOneShots = false;
	}

	private void SetCurrentRotationSlot()
	{
		float currentRotation = m_wheelRoot.rotation.eulerAngles.y;
		for (int i = 0; i < m_wheelSegments.Count; i++)
		{
			if (currentRotation > m_wheelSegments[i])
			{
				m_currentTriggerAngle = i + 1;
				break;
			}
		}
	}
}
