using System.Collections.Generic;
using Blizzard.T5.MaterialService.Extensions;
using UnityEngine;

public class AnimateTransitions : MonoBehaviour
{
	public List<GameObject> m_TargetList;

	public float amount;

	private List<Renderer> rend;

	public void StartTransitions()
	{
		foreach (Renderer item in rend)
		{
			item.GetMaterial().SetFloat("_Transistion", amount);
		}
	}

	private void Start()
	{
		rend = new List<Renderer>();
		foreach (GameObject targetObject in m_TargetList)
		{
			if (!(targetObject == null))
			{
				rend.Add(targetObject.GetComponent<Renderer>());
			}
		}
	}

	private void Update()
	{
		StartTransitions();
	}
}
