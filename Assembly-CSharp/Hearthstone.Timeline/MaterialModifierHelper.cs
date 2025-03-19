using System.Collections.Generic;
using UnityEngine;

namespace Hearthstone.Timeline;

public class MaterialModifierHelper : TimelineEffectHelper
{
	private Material m_material;

	private Material m_backupMaterial;

	private Renderer m_renderer;

	private int m_materialIndex;

	private MaterialModifierEntryCollection m_entries = new MaterialModifierEntryCollection();

	private bool m_isInitialized;

	private readonly List<Material> m_materials = new List<Material>();

	protected override void Initialize(params object[] values)
	{
		if (values == null || values.Length < 3)
		{
			return;
		}
		m_renderer = (Renderer)values[0];
		m_materialIndex = (int)values[1];
		m_entries = (MaterialModifierEntryCollection)values[2];
		if (m_renderer == null || m_entries == null)
		{
			return;
		}
		m_materials.Clear();
		m_renderer.GetMaterials(m_materials);
		if (m_materials.Count != 0)
		{
			m_materialIndex = Mathf.Clamp(m_materialIndex, 0, m_materials.Count - 1);
			m_material = m_materials[m_materialIndex];
			m_backupMaterial = new Material(m_material);
			for (int i = 0; i < m_entries.Count; i++)
			{
				m_entries.Get(i).randomValue = Random.value;
			}
			m_isInitialized = true;
			UpdateTarget(0f, includeTexture: true);
		}
	}

	protected override void CopyOriginalValuesFrom<T>(T _)
	{
	}

	protected override void OnKill(TimelineEffectKillCause _)
	{
	}

	protected override void ResetTarget(TimelineEffectResetCause cause)
	{
		if (m_isInitialized && cause == TimelineEffectResetCause.Kill)
		{
			TransferMaterialProperties(m_backupMaterial, m_material);
			Object.DestroyImmediate(m_backupMaterial);
			m_entries = null;
			m_isInitialized = false;
		}
	}

	protected override void UpdateTarget(float normalizedTime)
	{
		UpdateTarget(normalizedTime, includeTexture: false);
	}

	private void UpdateTarget(float normalizedTime, bool includeTexture)
	{
		if (m_material == null || m_entries == null)
		{
			return;
		}
		for (int i = 0; i < m_entries.Count; i++)
		{
			MaterialModifierEntry entry = m_entries.Get(i);
			if (!m_material.HasProperty(entry.key))
			{
				continue;
			}
			switch (entry.entryType)
			{
			case MaterialModifierEntry.EntryType.Float:
				m_material.SetFloat(entry.key, (float)entry.floatValue.Get(normalizedTime, entry.randomValue));
				break;
			case MaterialModifierEntry.EntryType.Vector:
				m_material.SetVector(entry.key, (Vector4)entry.vectorValue.Get(normalizedTime, entry.randomValue));
				break;
			case MaterialModifierEntry.EntryType.Color:
				m_material.SetColor(entry.key, (Color)entry.colorValue.Get(normalizedTime, entry.randomValue));
				break;
			case MaterialModifierEntry.EntryType.Texture:
				if (includeTexture && entry.texture != null)
				{
					m_material.SetTexture(entry.key, entry.texture);
				}
				break;
			case MaterialModifierEntry.EntryType.TextureProperties:
			{
				Vector4 v4 = (Vector4)entry.texturePropertiesValue.Get(normalizedTime, entry.randomValue);
				m_material.SetTextureScale(entry.key, new Vector2(v4.w, v4.z));
				m_material.SetTextureOffset(entry.key, new Vector2(v4.x, v4.y));
				break;
			}
			}
		}
	}

	private void TransferMaterialProperties(Material source, Material target)
	{
		if (source == null || target == null || m_entries == null)
		{
			return;
		}
		for (int i = 0; i < m_entries.Count; i++)
		{
			MaterialModifierEntry entry = m_entries.Get(i);
			if (source.HasProperty(entry.key) && target.HasProperty(entry.key))
			{
				switch (entry.entryType)
				{
				case MaterialModifierEntry.EntryType.Float:
					target.SetFloat(entry.key, source.GetFloat(entry.key));
					break;
				case MaterialModifierEntry.EntryType.Vector:
					target.SetVector(entry.key, source.GetVector(entry.key));
					break;
				case MaterialModifierEntry.EntryType.Color:
					target.SetColor(entry.key, source.GetColor(entry.key));
					break;
				case MaterialModifierEntry.EntryType.Texture:
					target.SetTexture(entry.key, source.GetTexture(entry.key));
					break;
				case MaterialModifierEntry.EntryType.TextureProperties:
					target.SetTextureScale(entry.key, source.GetTextureScale(entry.key));
					target.SetTextureOffset(entry.key, source.GetTextureOffset(entry.key));
					break;
				}
			}
		}
	}
}
