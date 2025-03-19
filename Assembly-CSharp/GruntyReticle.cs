using UnityEngine;

public class GruntyReticle : MonoBehaviour
{
	public GameObject m_reticleGreen;

	public GameObject m_reticleRed;

	public void SetHoveringState(bool state)
	{
		m_reticleGreen.SetActive(!state);
		m_reticleRed.SetActive(state);
	}
}
