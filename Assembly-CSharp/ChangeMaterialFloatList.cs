using System.Collections.Generic;
using Blizzard.T5.MaterialService.Extensions;
using UnityEngine;

public class ChangeMaterialFloatList : MonoBehaviour
{
	public List<GameObject> m_TargetList;

	public string m_propertyName;

	public float m_propertyValue;

	private List<Renderer> rend;

	private int m_materialProperty;

	private Material m_mat;

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
		foreach (Renderer item in rend)
		{
			item.GetMaterial().SetFloat(m_propertyName, m_propertyValue);
		}
	}
}
