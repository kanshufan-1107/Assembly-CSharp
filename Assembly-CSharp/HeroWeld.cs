using System.Collections;
using UnityEngine;

public class HeroWeld : MonoBehaviour
{
	private Light[] m_lights;

	public AudioSource m_weldInSound;

	public void DoAnim()
	{
		AudioSource audioSource = m_weldInSound.GetComponent<AudioSource>();
		if (SoundManager.Get() == null)
		{
			audioSource.Play();
		}
		else
		{
			SoundManager.Get().Play(audioSource);
		}
		base.gameObject.SetActive(value: true);
		m_lights = base.gameObject.GetComponentsInChildren<Light>();
		Light[] lights = m_lights;
		for (int i = 0; i < lights.Length; i++)
		{
			lights[i].enabled = true;
		}
		string ANIM_NAME = "HeroWeldIn";
		Animation component = base.gameObject.GetComponent<Animation>();
		component.Stop(ANIM_NAME);
		component.Play(ANIM_NAME);
		StartCoroutine(DestroyWhenFinished());
	}

	private IEnumerator DestroyWhenFinished()
	{
		yield return new WaitForSeconds(5f);
		Light[] lights = m_lights;
		for (int i = 0; i < lights.Length; i++)
		{
			lights[i].enabled = false;
		}
		Object.Destroy(base.gameObject);
	}
}
