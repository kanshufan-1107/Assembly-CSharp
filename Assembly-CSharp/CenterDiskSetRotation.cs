using Blizzard.T5.MaterialService.Extensions;
using UnityEngine;

public class CenterDiskSetRotation : MonoBehaviour
{
	[SerializeField]
	private GameObject m_diskMesh;

	[SerializeField]
	private GameObject m_buttonMesh;

	public void ApplyBoxDressingMaterials(EventBoxDressing.BoxDressingMaterials materials)
	{
		if (materials != null && !(materials.BoxMaterial == null) && !(materials.SetRotationButtonMaterial == null))
		{
			Renderer diskRenderer = m_diskMesh?.GetComponent<Renderer>();
			if (m_diskMesh != null && diskRenderer != null)
			{
				diskRenderer.SetMaterial(0, materials.BoxMaterial);
				diskRenderer.SetMaterial(2, materials.SetRotationButtonMaterial);
			}
			Renderer buttonRenderer = m_buttonMesh?.GetComponent<Renderer>();
			if (m_buttonMesh != null && buttonRenderer != null)
			{
				buttonRenderer.SetMaterial(materials.SetRotationButtonMaterial);
			}
		}
	}
}
