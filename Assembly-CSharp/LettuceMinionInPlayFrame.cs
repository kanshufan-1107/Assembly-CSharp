using System;
using System.Collections.Generic;
using Blizzard.T5.Core;
using UnityEngine;

public class LettuceMinionInPlayFrame : MonoBehaviour
{
	[Serializable]
	public class GemAndScarfMapping
	{
		[SerializeField]
		public TAG_ROLE m_role;

		[SerializeField]
		public GameObject m_scarfAndGemParent;
	}

	public List<GemAndScarfMapping> m_gemAndScarfMappings;

	public GameObject[] m_attackBaubles = new GameObject[3];

	private Map<GameObject, Vector3> m_attackBaublesStartingScales = new Map<GameObject, Vector3>();

	public GameObject[] m_healthBaubles = new GameObject[3];

	private Map<GameObject, Vector3> m_healthBaublesStartingScales = new Map<GameObject, Vector3>();

	private void Awake()
	{
		InitializeInitialScaleMap(m_attackBaubles, m_attackBaublesStartingScales);
		InitializeInitialScaleMap(m_healthBaubles, m_healthBaublesStartingScales);
	}

	private void InitializeInitialScaleMap(GameObject[] baubles, Map<GameObject, Vector3> map)
	{
		if (baubles == null || map == null)
		{
			return;
		}
		foreach (GameObject bauble in baubles)
		{
			if (!map.ContainsKey(bauble))
			{
				map.Add(bauble, bauble.transform.localScale);
			}
		}
	}

	public void UpdateFrameType(TAG_ROLE role)
	{
		foreach (GemAndScarfMapping roleMapping in m_gemAndScarfMappings)
		{
			roleMapping.m_scarfAndGemParent.SetActive(roleMapping.m_role == role);
		}
	}

	public void EnlargeAttackBauble(float scaleFactor)
	{
		EnlargeBauble(m_attackBaubles, m_attackBaublesStartingScales, scaleFactor);
	}

	public void EnlargeHealthBauble(float scaleFactor)
	{
		EnlargeBauble(m_healthBaubles, m_healthBaublesStartingScales, scaleFactor);
	}

	private void EnlargeBauble(GameObject[] baubles, Map<GameObject, Vector3> startingScales, float scaleFactor)
	{
		if (baubles == null || startingScales == null)
		{
			return;
		}
		foreach (GameObject bauble in baubles)
		{
			if (!(bauble == null) && bauble.activeInHierarchy)
			{
				if (!startingScales.TryGetValue(bauble, out var startingScale))
				{
					startingScale = Vector3.one;
				}
				EnlargeBaubleTween(bauble, startingScale, scaleFactor);
			}
		}
	}

	private void EnlargeBaubleTween(GameObject bauble, Vector3 startingScale, float scaleFactor)
	{
		if (!(bauble == null) && bauble.activeInHierarchy)
		{
			iTween.Stop(bauble);
			iTween.ScaleTo(bauble, iTween.Hash("scale", new Vector3(startingScale.x * scaleFactor, startingScale.y * scaleFactor, startingScale.z * scaleFactor), "time", 0.5f, "easetype", iTween.EaseType.easeOutElastic));
		}
	}

	public void ShrinkAttackBauble()
	{
		ShrinkBauble(m_attackBaubles, m_attackBaublesStartingScales);
	}

	public void ShrinkHealthBauble()
	{
		ShrinkBauble(m_healthBaubles, m_healthBaublesStartingScales);
	}

	private void ShrinkBauble(GameObject[] baubles, Map<GameObject, Vector3> startingScales)
	{
		if (baubles == null || startingScales == null)
		{
			return;
		}
		foreach (GameObject bauble in baubles)
		{
			if (!(bauble == null) && bauble.activeInHierarchy)
			{
				if (!startingScales.TryGetValue(bauble, out var startingScale))
				{
					startingScale = Vector3.one;
				}
				iTween.ScaleTo(bauble, startingScale, 0.5f);
			}
		}
	}
}
