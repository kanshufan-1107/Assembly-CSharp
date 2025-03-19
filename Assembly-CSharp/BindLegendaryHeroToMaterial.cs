using System.Collections;
using System.Collections.Generic;
using Blizzard.T5.MaterialService.Extensions;
using Blizzard.T5.Services;
using UnityEngine;

[CustomEditClass]
public class BindLegendaryHeroToMaterial : MonoBehaviour
{
	[CustomEditField(Sections = "Setup", T = EditType.DEFAULT)]
	public bool ShouldBindOnStart = true;

	[CustomEditField(Sections = "Hero Prefab", T = EditType.GAME_OBJECT)]
	public string LegendaryHeroPrefab;

	[CustomEditField(Sections = "Hero Prefab")]
	public Player.Side PlayerSide;

	[CustomEditField(Sections = "Target Material")]
	public Renderer PortraitRenderer;

	[CustomEditField(Sections = "Target Material")]
	public int MaterialIndex;

	private Coroutine m_bindingCoroutine;

	private ILegendaryHeroPortrait m_legendaryHeroPortrait;

	private bool m_hasBoundMaterial;

	private string m_boundAssetPath;

	private void Start()
	{
		if (ShouldBindOnStart)
		{
			BindMaterial();
		}
	}

	private void OnDestroy()
	{
		Cleanup();
	}

	public void BindMaterial()
	{
		if (!string.IsNullOrEmpty(m_boundAssetPath) && LegendaryHeroPrefab != m_boundAssetPath)
		{
			Cleanup();
		}
		if (!m_hasBoundMaterial && m_bindingCoroutine == null)
		{
			m_bindingCoroutine = StartCoroutine(BindMaterialAsync());
		}
	}

	private IEnumerator BindMaterialAsync()
	{
		if (PortraitRenderer != null && !string.IsNullOrEmpty(LegendaryHeroPrefab))
		{
			m_boundAssetPath = LegendaryHeroPrefab;
			LegendaryHeroRenderToTextureService service;
			while (!ServiceManager.TryGet<LegendaryHeroRenderToTextureService>(out service))
			{
				yield return null;
			}
			if (service != null)
			{
				m_legendaryHeroPortrait = service.CreatePortrait(m_boundAssetPath, PlayerSide);
				Texture texture = m_legendaryHeroPortrait?.PortraitTexture;
				if (texture != null)
				{
					List<Material> materials = new List<Material>();
					PortraitRenderer.GetSharedMaterials(materials);
					if (MaterialIndex < materials.Count)
					{
						Material material = materials[MaterialIndex];
						if ((bool)material)
						{
							material = Object.Instantiate(material);
							material.mainTexture = texture;
							materials[MaterialIndex] = material;
							PortraitRenderer.SetSharedMaterials(materials.ToArray());
							LegendarySkinDynamicResController dynamicResController = GetComponentInChildren<LegendarySkinDynamicResController>();
							if ((bool)dynamicResController)
							{
								dynamicResController.CacheMaterialProperties(material);
								dynamicResController.Renderer = PortraitRenderer;
								dynamicResController.MaterialIdx = MaterialIndex;
								m_legendaryHeroPortrait.ConnectDynamicResolutionController(dynamicResController);
							}
							m_hasBoundMaterial = true;
							m_bindingCoroutine = null;
							yield break;
						}
					}
				}
			}
		}
		Cleanup();
	}

	public void Cleanup()
	{
		m_hasBoundMaterial = false;
		m_boundAssetPath = string.Empty;
		if (m_bindingCoroutine != null)
		{
			StopCoroutine(m_bindingCoroutine);
			m_bindingCoroutine = null;
		}
		m_legendaryHeroPortrait?.Dispose();
		m_legendaryHeroPortrait = null;
	}
}
