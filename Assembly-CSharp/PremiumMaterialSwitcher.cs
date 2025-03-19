using System.Collections.Generic;
using Blizzard.T5.MaterialService.Extensions;
using UnityEngine;

public class PremiumMaterialSwitcher : MonoBehaviour
{
	public Material[] m_PremiumMaterials;

	private List<Material> OrgMaterials;

	private Renderer m_renderer;

	private void Start()
	{
		m_renderer = GetComponent<Renderer>();
	}

	public void SetToPremium(int premium)
	{
		if (premium < 1)
		{
			List<Material> restoreMaterials = m_renderer.GetMaterials();
			if (restoreMaterials == null || OrgMaterials == null)
			{
				return;
			}
			for (int idx = 0; idx < m_PremiumMaterials.Length && idx < restoreMaterials.Count; idx++)
			{
				if (!(m_PremiumMaterials[idx] == null))
				{
					restoreMaterials[idx] = OrgMaterials[idx];
				}
			}
			m_renderer.SetMaterials(restoreMaterials);
			OrgMaterials = null;
		}
		else
		{
			if (m_PremiumMaterials.Length < 1)
			{
				return;
			}
			if (OrgMaterials == null)
			{
				OrgMaterials = m_renderer.GetMaterials();
			}
			List<Material> objectMaterials = m_renderer.GetMaterials();
			for (int i = 0; i < m_PremiumMaterials.Length && i < objectMaterials.Count; i++)
			{
				if (!(m_PremiumMaterials[i] == null))
				{
					objectMaterials[i] = m_PremiumMaterials[i];
				}
			}
			m_renderer.SetMaterials(objectMaterials);
		}
	}
}
