using Blizzard.T5.MaterialService.Extensions;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class LegendaryHeroMagicEffect : MonoBehaviour
{
	public LegendaryHeroMagicEffectMaterial EffectConfig;

	private LegendarySkin m_skin;

	private MeshFilter m_meshFilter;

	private Material m_material;

	private LegendaryHeroMagicEffectState m_state;

	private void Start()
	{
		if (EffectConfig == null)
		{
			base.enabled = false;
			return;
		}
		m_skin = GetComponentInParent<LegendarySkin>();
		m_meshFilter = GetComponent<MeshFilter>();
		if (m_meshFilter != null)
		{
			m_meshFilter.sharedMesh = EffectConfig.Mesh;
		}
		Shader shader = EffectConfig.Shader;
		if (shader == null)
		{
			shader = Shader.Find("Unlit/Color");
		}
		m_material = new Material(shader);
		EffectConfig.InitialiseMaterial(m_material);
		EffectConfig.UpdateMaterialState(m_material, in m_state);
		MeshRenderer renderer = GetComponent<MeshRenderer>();
		if (renderer != null)
		{
			renderer.SetSharedMaterial(m_material);
		}
		if (m_skin != null)
		{
			m_skin.SetDirty();
		}
	}

	private void OnEnable()
	{
		m_state = default(LegendaryHeroMagicEffectState);
	}

	private void Update()
	{
		if (!(EffectConfig == null))
		{
			m_state = EffectConfig.UpdateState(Time.deltaTime, in m_state);
			if (m_material != null)
			{
				EffectConfig.UpdateMaterialState(m_material, in m_state);
			}
		}
	}

	private void OnDrawGizmos()
	{
		if (EffectConfig != null)
		{
			Gizmos.DrawMesh(EffectConfig.Mesh, base.transform.position, base.transform.rotation, base.transform.lossyScale);
		}
	}
}
