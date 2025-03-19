using System.Collections.Generic;
using Blizzard.T5.MaterialService.Extensions;
using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class SetRenderQue : MonoBehaviour
{
	public int queue = 1;

	public bool includeChildren;

	public int[] queues;

	private Renderer m_Renderer;

	private void Awake()
	{
		m_Renderer = GetComponent<Renderer>();
	}

	private void Start()
	{
		if (includeChildren)
		{
			Renderer[] componentsInChildren = GetComponentsInChildren<Renderer>();
			foreach (Renderer childRenderer in componentsInChildren)
			{
				if (!(childRenderer == null))
				{
					childRenderer.sortingOrder += queue;
				}
			}
		}
		else
		{
			if (m_Renderer == null)
			{
				return;
			}
			m_Renderer.sortingOrder += queue;
		}
		Run();
	}

	public void Run()
	{
		if (queues == null || m_Renderer == null)
		{
			return;
		}
		List<Material> materials = m_Renderer.GetSharedMaterials();
		if (materials == null)
		{
			return;
		}
		int matCount = materials.Count;
		for (int idx = 0; idx < queues.Length && idx < matCount; idx++)
		{
			int specificQueue = queues[idx];
			if (specificQueue == 0 || specificQueue == queue)
			{
				continue;
			}
			Material mat = m_Renderer.GetSharedMaterial(idx);
			if (!(mat == null))
			{
				if (specificQueue < 0)
				{
					Debug.LogWarning($"WARNING: Using negative renderQueue for {base.transform.root.name}'s {base.gameObject.name} (renderQueue = {queues[idx]})");
				}
				Material newMaterial = new Material(mat);
				newMaterial.renderQueue += specificQueue;
				m_Renderer.SetMaterial(idx, newMaterial);
			}
		}
	}
}
