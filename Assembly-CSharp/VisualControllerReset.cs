using Hearthstone.UI;
using UnityEngine;

public class VisualControllerReset : MonoBehaviour
{
	[SerializeField]
	[Tooltip("Target controller to invoke")]
	private VisualController m_controller;

	[Tooltip("Name of state to reset to")]
	[SerializeField]
	private string m_defaultState;

	private void OnEnable()
	{
		m_controller.SetState(m_defaultState);
	}
}
