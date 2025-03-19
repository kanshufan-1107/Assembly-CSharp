using System;
using System.Collections.Generic;
using Blizzard.T5.MaterialService.Extensions;
using UnityEngine;

public class LegendaryShaderPropertyController : MonoBehaviour
{
	[Serializable]
	public class RendererInfo
	{
		public Renderer Renderer;

		public int MaterialIndex;

		private Material m_Material;

		public Material Mat
		{
			get
			{
				if (m_Material != null)
				{
					return m_Material;
				}
				m_Material = Renderer.GetSharedMaterial(MaterialIndex);
				return m_Material;
			}
		}
	}

	public List<RendererInfo> m_Renderers;

	private MaterialPropertyBlock m_propertyBlock;

	private LegendaryHeroShaderPropertyGroup[] m_propertyGroups;

	public bool EditMode { get; set; }

	public void ResetProperties()
	{
		m_propertyBlock = new MaterialPropertyBlock();
		foreach (RendererInfo rendererInfo in m_Renderers)
		{
			LegendaryHeroShaderPropertyGroup[] propertyGroups = m_propertyGroups;
			for (int i = 0; i < propertyGroups.Length; i++)
			{
				propertyGroups[i].Reset();
			}
			rendererInfo.Renderer.SetPropertyBlock(m_propertyBlock, rendererInfo.MaterialIndex);
		}
	}

	private void Awake()
	{
		m_propertyGroups = base.gameObject.GetComponentsInChildren<LegendaryHeroShaderPropertyGroup>();
	}

	private void LateUpdate()
	{
		UpdateProperties();
	}

	private void UpdateProperties()
	{
		if (m_propertyBlock == null)
		{
			m_propertyBlock = new MaterialPropertyBlock();
		}
		foreach (RendererInfo rendererInfo in m_Renderers)
		{
			if (rendererInfo.Renderer == null)
			{
				break;
			}
			rendererInfo.Renderer.GetPropertyBlock(m_propertyBlock, rendererInfo.MaterialIndex);
			Material mat = rendererInfo.Mat;
			LegendaryHeroShaderPropertyGroup[] propertyGroups = m_propertyGroups;
			for (int i = 0; i < propertyGroups.Length; i++)
			{
				propertyGroups[i].SetProperties(mat, m_propertyBlock, EditMode);
			}
			rendererInfo.Renderer.SetPropertyBlock(m_propertyBlock, rendererInfo.MaterialIndex);
		}
	}
}
