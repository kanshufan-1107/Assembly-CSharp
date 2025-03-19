using System.Collections.Generic;
using Blizzard.T5.MaterialService.Extensions;
using UnityEngine;

public class AnimateVars : MonoBehaviour
{
	public List<GameObject> m_objects;

	public float amount;

	public string varName;

	private List<Renderer> m_renderers;

	public void AnimateValue()
	{
		foreach (Renderer r in m_renderers)
		{
			if (r != null)
			{
				r.GetMaterial().SetFloat(varName, amount);
			}
		}
	}

	private void Start()
	{
		m_renderers = new List<Renderer>();
		foreach (GameObject targetObject in m_objects)
		{
			if (!(targetObject == null))
			{
				m_renderers.Add(targetObject.GetComponent<Renderer>());
			}
		}
	}

	private void Update()
	{
		AnimateValue();
	}
}
