using Blizzard.T5.Core.Utils;
using Blizzard.T5.MaterialService.Extensions;
using UnityEngine;

public class AssignActorPortraitToChildren : MonoBehaviour
{
	private Actor m_Actor;

	private void Start()
	{
		m_Actor = GameObjectUtils.FindComponentInThisOrParents<Actor>(base.gameObject);
	}

	public void AssignPortraitToAllChildren()
	{
		if (!m_Actor || m_Actor.m_portraitMesh == null)
		{
			return;
		}
		Texture portraitTex = m_Actor.GetCard().GetPreferredActorPortraitTexture();
		if (portraitTex == null)
		{
			Debug.LogWarning($"AssignPortraitToAllChildren could not find a preferred portrait for {m_Actor}");
			return;
		}
		Renderer[] componentsInChildren = GetComponentsInChildren<Renderer>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			foreach (Material mat in componentsInChildren[i].GetMaterials())
			{
				if (mat.name.Contains("portrait"))
				{
					mat.mainTexture = portraitTex;
				}
			}
		}
	}
}
