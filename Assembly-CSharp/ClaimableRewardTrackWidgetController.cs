using System.Collections.Generic;
using Hearthstone.UI;
using UnityEngine;

public class ClaimableRewardTrackWidgetController : MonoBehaviour
{
	public float m_HoveredColorIntensity;

	public float m_DefaultColorIntensity;

	private Widget m_widget;

	private bool m_isCurrentlyHovered;

	private MeshRenderer m_currentMeshRenderer;

	private const string ON_HOVER = "CLAIMABLE_HOVER";

	private const string ON_HOVER_OFF = "CLAIMABLE_NO_HOVER";

	private void Start()
	{
		Spawnable spawnable = base.gameObject.GetComponent<Spawnable>();
		if (spawnable != null)
		{
			m_widget = spawnable.OwningWidget;
			if (m_widget != null)
			{
				m_widget.RegisterDoneChangingStatesListener(OnCurrentMeshRendererChanged);
				m_widget.RegisterEventListener(HandleEvent);
			}
		}
	}

	private void OnCurrentMeshRendererChanged(object payload)
	{
		m_currentMeshRenderer = base.gameObject.GetComponentInChildren<MeshRenderer>();
		UpdateMeshRendererColorIntensity();
	}

	private void HandleEvent(string eventName)
	{
		if (!(eventName == "CLAIMABLE_HOVER"))
		{
			if (eventName == "CLAIMABLE_NO_HOVER")
			{
				m_isCurrentlyHovered = false;
				UpdateMeshRendererColorIntensity();
			}
		}
		else
		{
			m_isCurrentlyHovered = true;
			UpdateMeshRendererColorIntensity();
		}
	}

	private void UpdateMeshRendererColorIntensity()
	{
		if (m_currentMeshRenderer != null)
		{
			List<Material> materials = new List<Material>();
			m_currentMeshRenderer.GetMaterials(materials);
			Material materialToModify = ((materials.Count > 0) ? materials[0] : null);
			if (materialToModify != null)
			{
				materialToModify.SetFloat("_ColorIntensity", m_isCurrentlyHovered ? m_HoveredColorIntensity : m_DefaultColorIntensity);
			}
		}
	}
}
