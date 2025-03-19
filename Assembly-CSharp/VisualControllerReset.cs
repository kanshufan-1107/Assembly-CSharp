using Hearthstone.UI;
using UnityEngine;

public class VisualControllerReset : MonoBehaviour
{
	[SerializeField]
	[Tooltip("Target controller to invoke")]
	private VisualController m_controller;

	[SerializeField]
	[Tooltip("Name of state to reset to")]
	private string m_defaultState;

	private void OnEnable()
	{
		m_controller.SetState(m_defaultState);
	}
}
