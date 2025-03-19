using UnityEngine;

public class PlayAnimator : MonoBehaviour
{
	public GameObject m_Target1;

	public string m_Target1State;

	public GameObject m_Target2;

	public string m_Target2State;

	public GameObject m_Target3;

	public string m_Target3State;

	public void PlayAnimator1()
	{
		if (!(m_Target1 == null))
		{
			Animator component = m_Target1.GetComponent<Animator>();
			component.enabled = true;
			component.Play(m_Target1State, -1, 0f);
		}
	}

	public void PlayAnimator2()
	{
		if (!(m_Target1 == null))
		{
			Animator component = m_Target2.GetComponent<Animator>();
			component.enabled = true;
			component.Play(m_Target2State, -1, 0f);
		}
	}

	public void PlayAnimator3()
	{
		if (!(m_Target1 == null))
		{
			Animator component = m_Target3.GetComponent<Animator>();
			component.enabled = true;
			component.Play(m_Target3State, -1, 0f);
		}
	}
}
