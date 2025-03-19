using System.Collections.Generic;
using Blizzard.T5.Core.Utils;
using Blizzard.T5.MaterialService.Extensions;
using UnityEngine;

public class SpellImpl : Spell
{
	protected Actor m_actor;

	protected GameObject m_rootObject;

	protected MeshRenderer m_rootObjectRenderer;

	private static List<Renderer> s_cachedRenderers;

	protected void InitActorVariables()
	{
		m_actor = SpellUtils.GetParentActor(this);
		m_rootObject = SpellUtils.GetParentRootObject(this);
		m_rootObjectRenderer = SpellUtils.GetParentRootObjectMesh(this);
	}

	protected void SetActorVisibility(bool visible, bool ignoreSpells)
	{
		if (m_actor != null)
		{
			if (visible)
			{
				m_actor.Show(ignoreSpells);
			}
			else
			{
				m_actor.Hide(ignoreSpells);
			}
		}
	}

	protected void SetVisibility(GameObject go, bool visible)
	{
		go.GetComponent<Renderer>().enabled = visible;
	}

	protected void SetVisibilityRecursive(GameObject go, bool visible)
	{
		if (go == null)
		{
			return;
		}
		if (s_cachedRenderers == null)
		{
			s_cachedRenderers = new List<Renderer>();
		}
		go.GetComponentsInChildren(s_cachedRenderers);
		foreach (Renderer s_cachedRenderer in s_cachedRenderers)
		{
			s_cachedRenderer.enabled = visible;
		}
	}

	protected void SetAnimationSpeed(GameObject go, string animName, float speed)
	{
		if (!(go == null))
		{
			go.GetComponent<Animation>()[animName].speed = speed;
		}
	}

	protected void SetAnimationTime(GameObject go, string animName, float time)
	{
		if (!(go == null))
		{
			go.GetComponent<Animation>()[animName].time = time;
		}
	}

	protected void PlayAnimation(GameObject go, string animName, PlayMode playMode, float crossFade = 0f)
	{
		if (!(go == null))
		{
			Animation spellAnimation = go.GetComponent<Animation>();
			if (crossFade <= Mathf.Epsilon)
			{
				spellAnimation.Play(animName, playMode);
			}
			else
			{
				spellAnimation.CrossFade(animName, crossFade, playMode);
			}
		}
	}

	protected void PlayParticles(GameObject go, bool includeChildren)
	{
		if (!(go == null))
		{
			go.GetComponent<ParticleSystem>().Play(includeChildren);
		}
	}

	protected GameObject GetActorObject(string name)
	{
		if (m_actor == null)
		{
			return null;
		}
		return GameObjectUtils.FindChildBySubstring(m_actor.gameObject, name);
	}

	protected void SetMaterialColor(GameObject go, Material material, string colorName, Color color, int materialIndex = 0)
	{
		if (colorName == "")
		{
			colorName = "_Color";
		}
		if (material != null)
		{
			material.SetColor(colorName, color);
		}
		else
		{
			if (go == null)
			{
				return;
			}
			Renderer renderer = go.GetComponent<Renderer>();
			if (renderer == null)
			{
				return;
			}
			Material rendererMaterial = renderer.GetMaterial();
			if (!(rendererMaterial == null))
			{
				if (materialIndex == 0)
				{
					rendererMaterial.SetColor(colorName, color);
				}
				else if (renderer.GetMaterials().Count > materialIndex)
				{
					renderer.GetMaterial(materialIndex).SetColor(colorName, color);
				}
			}
		}
	}

	protected Material GetMaterial(GameObject go, Material material, bool getSharedMaterial = false, int materialIndex = 0)
	{
		if (go == null)
		{
			return null;
		}
		Renderer renderer = go.GetComponent<Renderer>();
		if (renderer == null)
		{
			return null;
		}
		if (materialIndex == 0 && !getSharedMaterial)
		{
			return renderer.GetMaterial();
		}
		if (materialIndex == 0 && getSharedMaterial)
		{
			return renderer.GetSharedMaterial();
		}
		if (renderer.GetMaterials().Count > materialIndex)
		{
			if (!getSharedMaterial)
			{
				return renderer.GetMaterial(materialIndex);
			}
			return renderer.GetSharedMaterial(materialIndex);
		}
		return null;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (s_cachedRenderers != null)
		{
			s_cachedRenderers.Clear();
		}
	}
}
