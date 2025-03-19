using System;
using System.Collections.Generic;
using Blizzard.T5.MaterialService.Extensions;
using UnityEngine;

[CustomEditClass]
[ExecuteAlways]
public class ReplaceMaterials : MonoBehaviour
{
	[Serializable]
	public class MaterialData
	{
		[CustomEditField(T = EditType.SCENE_OBJECT)]
		public string GameObjectName;

		public int MaterialIndex;

		public Material NewMaterial;

		public bool ReplaceChildMaterials;

		public GameObject DisplayGameObject;
	}

	public List<MaterialData> m_Materials;

	private void Start()
	{
		foreach (MaterialData md in m_Materials)
		{
			if (md.NewMaterial == null)
			{
				continue;
			}
			GameObject go = FindGameObject(md.GameObjectName);
			if (go == null && !md.ReplaceChildMaterials)
			{
				Log.Graphics.Print("ReplaceMaterials failed to locate object: {0}", md.GameObjectName);
			}
			else if (md.ReplaceChildMaterials)
			{
				Renderer[] componentsInChildren = go.GetComponentsInChildren<Renderer>();
				foreach (Renderer rdr in componentsInChildren)
				{
					if (!(rdr == null))
					{
						rdr.SetMaterial(md.MaterialIndex, md.NewMaterial);
					}
				}
			}
			else
			{
				Renderer rdr2 = go.GetComponent<Renderer>();
				if (rdr2 == null)
				{
					Log.Graphics.Print("ReplaceMaterials failed to get Renderer: {0}", md.GameObjectName);
				}
				else
				{
					rdr2.SetMaterial(md.MaterialIndex, md.NewMaterial);
				}
			}
		}
	}

	private GameObject FindGameObject(string gameObjName)
	{
		if (gameObjName[0] != '/')
		{
			return GameObject.Find(gameObjName);
		}
		string[] array = gameObjName.Split('/');
		return GameObject.Find(array[array.Length - 1]);
	}
}
