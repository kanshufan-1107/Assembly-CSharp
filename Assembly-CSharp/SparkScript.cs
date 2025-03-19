using UnityEngine;

public class SparkScript : MonoBehaviour
{
	public AudioClip clip1;

	public AudioClip clip2;

	private void Awake()
	{
		AudioSource aud = GetComponent<AudioSource>();
		if (Random.value >= 0.5f)
		{
			aud.clip = clip1;
		}
		else
		{
			aud.clip = clip2;
		}
	}
}
