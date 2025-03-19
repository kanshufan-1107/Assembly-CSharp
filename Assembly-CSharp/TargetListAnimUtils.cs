using System.Collections.Generic;
using Blizzard.T5.MaterialService.Extensions;
using UnityEngine;

public class TargetListAnimUtils : MonoBehaviour
{
	public List<GameObject> m_TargetList;

	public void PlayNewParticlesListInChildren()
	{
		foreach (GameObject targetObject in m_TargetList)
		{
			if (!(targetObject == null))
			{
				ParticleSystem[] componentsInChildren = targetObject.GetComponentsInChildren<ParticleSystem>();
				for (int i = 0; i < componentsInChildren.Length; i++)
				{
					componentsInChildren[i].Play();
				}
			}
		}
	}

	public void StopNewParticlesListInChildren()
	{
		foreach (GameObject targetObject in m_TargetList)
		{
			if (!(targetObject == null))
			{
				ParticleSystem[] componentsInChildren = targetObject.GetComponentsInChildren<ParticleSystem>();
				for (int i = 0; i < componentsInChildren.Length; i++)
				{
					componentsInChildren[i].Stop();
				}
			}
		}
	}

	public void PlayAnimationList()
	{
		foreach (GameObject targetObject in m_TargetList)
		{
			if (!(targetObject == null))
			{
				targetObject.GetComponent<Animation>().Play();
			}
		}
	}

	public void StopAnimationList()
	{
		foreach (GameObject targetObject in m_TargetList)
		{
			if (!(targetObject == null))
			{
				targetObject.GetComponent<Animation>().Stop();
			}
		}
	}

	public void PlayAnimationListInChildren()
	{
		foreach (GameObject targetObject in m_TargetList)
		{
			if (!(targetObject == null))
			{
				Animation[] componentsInChildren = targetObject.GetComponentsInChildren<Animation>();
				for (int i = 0; i < componentsInChildren.Length; i++)
				{
					componentsInChildren[i].Play();
				}
			}
		}
	}

	public void StopAnimationListInChildren()
	{
		foreach (GameObject targetObject in m_TargetList)
		{
			if (!(targetObject == null))
			{
				Animation[] componentsInChildren = targetObject.GetComponentsInChildren<Animation>();
				for (int i = 0; i < componentsInChildren.Length; i++)
				{
					componentsInChildren[i].Stop();
				}
			}
		}
	}

	public void ActivateHierarchyList()
	{
		foreach (GameObject targetObject in m_TargetList)
		{
			if (!(targetObject == null))
			{
				targetObject.SetActive(value: true);
			}
		}
	}

	public void DeactivateHierarchyList()
	{
		foreach (GameObject targetObject in m_TargetList)
		{
			if (!(targetObject == null))
			{
				targetObject.SetActive(value: false);
			}
		}
	}

	public void DestroyHierarchyList()
	{
		foreach (GameObject target in m_TargetList)
		{
			Object.Destroy(target);
		}
	}

	public void FadeInList(float FadeSec)
	{
		foreach (GameObject target in m_TargetList)
		{
			iTween.FadeTo(target, 1f, FadeSec);
		}
	}

	public void FadeOutList(float FadeSec)
	{
		foreach (GameObject target in m_TargetList)
		{
			iTween.FadeTo(target, 0f, FadeSec);
		}
	}

	public void SetAlphaHierarchyList(float alpha)
	{
		foreach (GameObject targetObject in m_TargetList)
		{
			if (targetObject == null)
			{
				continue;
			}
			Renderer[] componentsInChildren = targetObject.GetComponentsInChildren<Renderer>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				Material material = componentsInChildren[i].GetMaterial();
				if (material.HasProperty("_Color"))
				{
					Color color = material.color;
					color.a = alpha;
					material.color = color;
				}
			}
		}
	}
}
