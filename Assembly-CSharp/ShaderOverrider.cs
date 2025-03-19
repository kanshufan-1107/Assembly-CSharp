using System;
using System.Collections.Generic;
using Blizzard.T5.MaterialService;
using Blizzard.T5.MaterialService.Extensions;
using Blizzard.T5.Services;
using Hearthstone.UI.Core;
using UnityEngine;

public class ShaderOverrider : MonoBehaviour
{
	[Serializable]
	private class ShaderTweak
	{
		public string parameter;

		public float value;
	}

	[SerializeField]
	private bool m_override;

	[SerializeField]
	private GameObject m_target;

	[SerializeField]
	protected Shader m_shaderOverride;

	[SerializeField]
	private List<ShaderTweak> m_tweaks = new List<ShaderTweak>();

	private Dictionary<Renderer, Material> m_rendererMapping = new Dictionary<Renderer, Material>();

	private Dictionary<Material, Material> m_materialOverrides = new Dictionary<Material, Material>();

	[Overridable]
	public bool Override
	{
		get
		{
			return m_override;
		}
		set
		{
			m_override = value;
			Apply(m_override);
		}
	}

	private void OnValidate()
	{
		if (m_target == null)
		{
			m_target = base.gameObject;
		}
		Apply(m_override);
	}

	private void OnDestroy()
	{
		DestroyInstancedMaterials();
	}

	private void Apply(bool applied)
	{
		if (applied)
		{
			InstantiateMaterials();
			ApplyShaderOverrides();
		}
		else
		{
			RestoreOriginalMaterials();
		}
	}

	private void InstantiateMaterials()
	{
		if (m_target == null)
		{
			return;
		}
		IMaterialService matService = ServiceManager.Get<IMaterialService>();
		Renderer[] componentsInChildren = m_target.GetComponentsInChildren<Renderer>(includeInactive: true);
		foreach (Renderer renderer in componentsInChildren)
		{
			if (m_rendererMapping.TryGetValue(renderer, out var baseMaterial))
			{
				continue;
			}
			baseMaterial = renderer.GetSharedMaterial();
			if (!(baseMaterial == null))
			{
				m_rendererMapping[renderer] = baseMaterial;
				if (!m_materialOverrides.TryGetValue(baseMaterial, out var overrideMaterial))
				{
					overrideMaterial = UnityEngine.Object.Instantiate(baseMaterial);
					matService.IgnoreMaterial(overrideMaterial);
					m_materialOverrides[baseMaterial] = overrideMaterial;
				}
				renderer.SetSharedMaterial(overrideMaterial);
			}
		}
	}

	private void ApplyShaderOverrides()
	{
		foreach (KeyValuePair<Material, Material> materialOverride2 in m_materialOverrides)
		{
			Material tweakedMaterial = materialOverride2.Value;
			if (tweakedMaterial.shader != m_shaderOverride && m_shaderOverride != null)
			{
				tweakedMaterial.shader = m_shaderOverride;
			}
			foreach (ShaderTweak tweak in m_tweaks)
			{
				if (!tweakedMaterial.HasProperty(tweak.parameter))
				{
					Debug.LogWarningFormat("Property '{0}' does not exist on shader '{1}'", tweak.parameter, tweakedMaterial.shader.name);
				}
				else
				{
					tweakedMaterial.SetFloat(tweak.parameter, tweak.value);
				}
			}
		}
	}

	private void RestoreOriginalMaterials()
	{
		foreach (KeyValuePair<Renderer, Material> mapping in m_rendererMapping)
		{
			Renderer key = mapping.Key;
			Material baseMaterial = mapping.Value;
			key.SetSharedMaterial(baseMaterial);
		}
		m_rendererMapping.Clear();
	}

	private void DestroyInstancedMaterials()
	{
		foreach (KeyValuePair<Material, Material> materialOverride in m_materialOverrides)
		{
			UnityEngine.Object.Destroy(materialOverride.Value);
		}
		m_materialOverrides.Clear();
	}
}
