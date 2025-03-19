using Blizzard.T5.MaterialService.Extensions;
using UnityEngine;

public class WeaponUVWorldspace : MonoBehaviour
{
	public float xOffset;

	public float yOffset;

	public float scale = 0.7f;

	private Material m_material;

	private void Start()
	{
		m_material = base.gameObject.GetComponent<Renderer>().GetMaterial();
	}

	private void Update()
	{
		Vector3 pos = base.transform.position * scale;
		m_material.SetFloat("_OffsetX", 0f - pos.z - xOffset);
		m_material.SetFloat("_OffsetY", 0f - pos.x - yOffset);
	}
}
