using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayAnimRandomStart : MonoBehaviour
{
	public List<GameObject> m_Bubbles;

	public float minWait;

	public float maxWait = 10f;

	public float MinSpeed = 0.2f;

	public float MaxSpeed = 1.1f;

	public string animName = "Bubble1";

	private void Start()
	{
		StartCoroutine(PlayRandomBubbles());
	}

	private IEnumerator PlayRandomBubbles()
	{
		while (true)
		{
			yield return new WaitForSeconds(Random.Range(minWait, maxWait));
			int bubbleIndex = Random.Range(0, m_Bubbles.Count);
			GameObject bubbles = m_Bubbles[bubbleIndex];
			if (!(bubbles == null))
			{
				Animation component = bubbles.GetComponent<Animation>();
				component.Play();
				component[animName].speed = Random.Range(MinSpeed, MaxSpeed);
			}
		}
	}
}
