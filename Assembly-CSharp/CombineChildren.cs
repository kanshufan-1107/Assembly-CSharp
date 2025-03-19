using System;
using System.Collections;
using System.Collections.Generic;
using Blizzard.T5.MaterialService.Extensions;
using UnityEngine;

[AddComponentMenu("Mesh/Combine Children")]
public class CombineChildren : MonoBehaviour
{
	public bool generateTriangleStrips = true;

	private void Start()
	{
		Component[] filters = GetComponentsInChildren(typeof(MeshFilter));
		Matrix4x4 myTransform = base.transform.worldToLocalMatrix;
		Hashtable materialToMesh = new Hashtable();
		for (int i = 0; i < filters.Length; i++)
		{
			MeshFilter filter = (MeshFilter)filters[i];
			Renderer curRenderer = filters[i].GetComponent<Renderer>();
			MeshCombineUtility.MeshInstance instance = default(MeshCombineUtility.MeshInstance);
			instance.mesh = filter.sharedMesh;
			if (!(curRenderer != null) || !curRenderer.enabled || !(instance.mesh != null))
			{
				continue;
			}
			instance.transform = myTransform * filter.transform.localToWorldMatrix;
			List<Material> materials = curRenderer.GetSharedMaterials();
			for (int m = 0; m < materials.Count; m++)
			{
				instance.subMeshIndex = Math.Min(m, instance.mesh.subMeshCount - 1);
				ArrayList objects = (ArrayList)materialToMesh[materials[m]];
				if (objects != null)
				{
					objects.Add(instance);
					continue;
				}
				objects = new ArrayList();
				objects.Add(instance);
				materialToMesh.Add(materials[m], objects);
			}
			curRenderer.enabled = false;
		}
		foreach (DictionaryEntry de in materialToMesh)
		{
			MeshCombineUtility.MeshInstance[] instances = (MeshCombineUtility.MeshInstance[])((ArrayList)de.Value).ToArray(typeof(MeshCombineUtility.MeshInstance));
			if (materialToMesh.Count == 1)
			{
				if (GetComponent(typeof(MeshFilter)) == null)
				{
					base.gameObject.AddComponent(typeof(MeshFilter));
				}
				if (!GetComponent("MeshRenderer"))
				{
					base.gameObject.AddComponent<MeshRenderer>();
				}
				((MeshFilter)GetComponent(typeof(MeshFilter))).mesh = MeshCombineUtility.Combine(instances, generateTriangleStrips);
				Renderer component = GetComponent<Renderer>();
				component.SetMaterial((Material)de.Key);
				component.enabled = true;
			}
			else
			{
				GameObject obj = new GameObject("Combined mesh");
				obj.transform.parent = base.transform;
				obj.transform.localScale = Vector3.one;
				obj.transform.localRotation = Quaternion.identity;
				obj.transform.localPosition = Vector3.zero;
				obj.AddComponent(typeof(MeshFilter));
				obj.AddComponent<MeshRenderer>();
				obj.GetComponent<Renderer>().SetMaterial((Material)de.Key);
				((MeshFilter)obj.GetComponent(typeof(MeshFilter))).mesh = MeshCombineUtility.Combine(instances, generateTriangleStrips);
			}
		}
	}
}
