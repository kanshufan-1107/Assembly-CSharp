using System.Collections.Generic;
using Blizzard.T5.MaterialService.Extensions;
using Hearthstone.UI;
using UnityEngine;

[CustomEditClass]
public class AdventureBossCoin : PegUIElement
{
	private const string s_EventCoinFlip = "Flip";

	public GameObject m_Coin;

	public MeshRenderer m_PortraitRenderer;

	public int m_PortraitMaterialIndex = 1;

	public GameObject m_Connector;

	public StateEventTable m_CoinStateTable;

	public PegUIElement m_DisabledCollider;

	public VisualController m_visualControllerBroadcaster;

	private bool m_Enabled;

	private static bool neverRun;

	public void SetPortraitMaterial(AdventureBossDef bossDef)
	{
		List<Material> portraitMaterials = m_PortraitRenderer?.GetMaterials();
		if (portraitMaterials != null && m_PortraitMaterialIndex < portraitMaterials.Count)
		{
			Material material = bossDef.m_CoinPortraitMaterial.GetMaterial();
			portraitMaterials[m_PortraitMaterialIndex] = material;
			m_PortraitRenderer.SetMaterials(portraitMaterials);
		}
	}

	public void SetPortraitMaterial(Material material)
	{
		List<Material> portraitMaterials = m_PortraitRenderer?.GetMaterials();
		if (portraitMaterials != null && m_PortraitMaterialIndex < portraitMaterials.Count)
		{
			portraitMaterials[m_PortraitMaterialIndex] = material;
			m_PortraitRenderer.SetMaterials(portraitMaterials);
		}
	}

	public void ShowConnector(bool show)
	{
		if (m_Connector != null)
		{
			m_Connector.SetActive(show);
		}
	}

	public void Enable(bool flag, bool animate = true)
	{
		Collider collider = GetComponent<Collider>();
		if (collider != null)
		{
			collider.enabled = flag;
		}
		if (m_DisabledCollider != null)
		{
			m_DisabledCollider.gameObject.SetActive(!flag);
		}
		if (m_Enabled != flag)
		{
			m_Enabled = flag;
			if (animate && flag && m_CoinStateTable != null)
			{
				ShowCoin(show: false);
				m_CoinStateTable.TriggerState("Flip");
			}
			else
			{
				ShowCoin(flag);
			}
		}
	}

	public void Select(bool selected)
	{
		UIBHighlight highlight = GetComponent<UIBHighlight>();
		if (!(highlight == null))
		{
			highlight.AlwaysOver = selected;
			if (selected)
			{
				EnableFancyHighlight(enable: false);
			}
		}
	}

	public void HighlightOnce()
	{
		UIBHighlight highlight = GetComponent<UIBHighlight>();
		if (!(highlight == null))
		{
			highlight.HighlightOnce();
		}
	}

	public void ShowNewLookGlow()
	{
		EnableFancyHighlight(enable: true);
	}

	public VisualController GetVisualControllerBroadcaster()
	{
		return m_visualControllerBroadcaster;
	}

	private void EnableFancyHighlight(bool enable)
	{
		UIBHighlightStateControl hsc = GetComponent<UIBHighlightStateControl>();
		if (!(hsc == null))
		{
			hsc.Select(enable);
		}
	}

	private void ShowCoin(bool show)
	{
		if (!(m_Coin == null))
		{
			TransformUtil.SetEulerAngleZ(m_Coin, show ? 0f : (-180f));
		}
	}

	private void Update()
	{
		if (neverRun)
		{
			Debug.Log("TEST");
		}
	}
}
