using System;
using System.Collections.Generic;
using System.Text;
using Blizzard.T5.MaterialService.Extensions;
using UnityEngine;

namespace Hearthstone.UI;

public class RendererDynamicPropertyResolverProxy : IDynamicPropertyResolverProxy, IDynamicPropertyResolver
{
	private const string MaterialTexturePath = ".texture";

	private const string MaterialTextureTilingPath = ".textureTiling";

	private const string MaterialTextureOffsetPath = ".textureOffset";

	private const string MaterialTextureOffsetPathX = ".textureOffset_x";

	private const string MaterialTextureOffsetPathY = ".textureOffset_y";

	private const string MaterialColorPath = ".color";

	private static readonly int s_MainTexId = Shader.PropertyToID("_MainTex");

	private static readonly int s_ColorId = Shader.PropertyToID("_Color");

	private Renderer m_renderer;

	protected List<DynamicPropertyInfo> m_properties = new List<DynamicPropertyInfo>();

	public ICollection<DynamicPropertyInfo> DynamicProperties => m_properties;

	public virtual void SetTarget(object target)
	{
		m_renderer = (Renderer)target;
		m_properties.Clear();
		m_properties.Add(new DynamicPropertyInfo
		{
			Id = "enabled",
			Name = "Enabled",
			Type = typeof(bool),
			Value = m_renderer.enabled
		});
		m_properties.Add(new DynamicPropertyInfo
		{
			Id = "sharedMaterial",
			Name = "sharedMaterial",
			Type = typeof(Material),
			Value = m_renderer.GetSharedMaterial()
		});
		List<Material> sharedMaterials = m_renderer.GetSharedMaterials();
		StringBuilder localStringBuilder = new StringBuilder(13);
		for (int i = 0; i < sharedMaterials.Count; i++)
		{
			localStringBuilder.Clear();
			localStringBuilder.Append("Materials[");
			localStringBuilder.Append(i);
			localStringBuilder.Append("]");
			Material material = sharedMaterials[i];
			string initialRoot = localStringBuilder.ToString();
			initialRoot.AsSpan();
			m_properties.Add(new DynamicPropertyInfo
			{
				Id = initialRoot,
				Name = initialRoot,
				Type = typeof(Material),
				Value = material
			});
			if (material != null && material.HasProperty(s_MainTexId))
			{
				string materialTextureName = initialRoot + ".texture";
				m_properties.Add(new DynamicPropertyInfo
				{
					Id = materialTextureName,
					Name = materialTextureName,
					Type = typeof(Texture2D),
					Value = material.mainTexture
				});
				string materialTexturTilingeName = initialRoot + ".textureTiling";
				m_properties.Add(new DynamicPropertyInfo
				{
					Id = materialTexturTilingeName,
					Name = materialTexturTilingeName,
					Type = typeof(Vector2),
					Value = material.mainTextureScale
				});
				string materialTextureOffsetName = initialRoot + ".textureOffset";
				m_properties.Add(new DynamicPropertyInfo
				{
					Id = materialTextureOffsetName,
					Name = materialTextureOffsetName,
					Type = typeof(Vector2),
					Value = material.mainTextureOffset
				});
				string materialTextureOffsetXName = initialRoot + ".textureOffset_x";
				m_properties.Add(new DynamicPropertyInfo
				{
					Id = materialTextureOffsetXName,
					Name = materialTextureOffsetXName,
					Type = typeof(float),
					Value = material.mainTextureOffset.x
				});
				string materialTextureOffsetYName = initialRoot + ".textureOffset_y";
				m_properties.Add(new DynamicPropertyInfo
				{
					Id = materialTextureOffsetYName,
					Name = materialTextureOffsetYName,
					Type = typeof(float),
					Value = material.mainTextureOffset.y
				});
			}
			if (material != null && material.HasProperty(s_ColorId))
			{
				string materialColorName = initialRoot + ".color";
				m_properties.Add(new DynamicPropertyInfo
				{
					Id = materialColorName,
					Name = materialColorName,
					Type = typeof(Color),
					Value = material.color
				});
			}
		}
	}

	public virtual bool GetDynamicPropertyValue(string id, out object value)
	{
		value = null;
		ReadOnlySpan<char> idAsSpan = id.AsSpan();
		if (!(id == "enabled"))
		{
			if (id == "sharedMaterial")
			{
				value = m_renderer.GetSharedMaterial();
				return true;
			}
			int index = -1;
			if (idAsSpan.Contains("Materials", StringComparison.InvariantCulture) && GetIndex(idAsSpan, out index))
			{
				Material material = (Material)(value = m_renderer.GetSharedMaterial(index));
				if (material != null)
				{
					string propertyName = null;
					if (idAsSpan.EndsWith(".color"))
					{
						value = material.color;
					}
					else if (idAsSpan.EndsWith(".texture"))
					{
						value = material.mainTexture;
					}
					else if (idAsSpan.EndsWith(".textureOffset"))
					{
						value = material.mainTextureOffset;
					}
					else if (idAsSpan.EndsWith(".textureOffset_x"))
					{
						value = material.mainTextureOffset.x;
					}
					else if (idAsSpan.EndsWith(".textureOffset_y"))
					{
						value = material.mainTextureOffset.y;
					}
					else if (idAsSpan.EndsWith(".textureTiling"))
					{
						value = material.mainTextureScale;
					}
					else if (!string.IsNullOrEmpty(propertyName = GetMaterialPropertyId(id)) && material.HasProperty(propertyName))
					{
						value = material.GetFloat(propertyName);
					}
				}
				return true;
			}
			return false;
		}
		value = m_renderer.enabled;
		return true;
	}

	public virtual bool SetDynamicPropertyValue(string id, object value)
	{
		if (!(id == "enabled"))
		{
			if (id == "sharedMaterial")
			{
				m_renderer.SetSharedMaterial((Material)value);
				return true;
			}
			int index = -1;
			ReadOnlySpan<char> idAsSpan = id.AsSpan();
			List<Material> sharedMaterials = m_renderer.GetSharedMaterials();
			if (idAsSpan.Contains("Materials", StringComparison.InvariantCulture) && GetIndex(idAsSpan, out index) && index < sharedMaterials.Count)
			{
				if (idAsSpan.EndsWith("]"))
				{
					List<Material> temp = sharedMaterials;
					Material material = (Material)value;
					temp[index] = material;
					m_renderer.SetSharedMaterials(temp);
				}
				else if (value != null)
				{
					Material mat = GetMaterialInstance(m_renderer, index);
					if (mat != null)
					{
						string propertyName;
						if (idAsSpan.EndsWith(".color"))
						{
							mat.color = (Color)value;
						}
						else if (idAsSpan.EndsWith(".texture"))
						{
							mat.mainTexture = value as Texture;
						}
						else if (idAsSpan.EndsWith(".textureOffset"))
						{
							mat.mainTextureOffset = (Vector4)value;
						}
						else if (idAsSpan.EndsWith(".textureOffset_x"))
						{
							mat.mainTextureOffset = new Vector2((float)value, mat.mainTextureOffset.y);
						}
						else if (idAsSpan.EndsWith(".textureOffset_y"))
						{
							mat.mainTextureOffset = new Vector2(mat.mainTextureOffset.x, (float)value);
						}
						else if (idAsSpan.EndsWith(".textureTiling"))
						{
							mat.mainTextureScale = (Vector4)value;
						}
						else if (!string.IsNullOrEmpty(propertyName = GetMaterialPropertyId(id)) && mat.HasProperty(propertyName))
						{
							mat.SetFloat(propertyName, (value is float) ? ((float)value) : ((value is double) ? ((float)(double)value) : 0f));
						}
					}
				}
				return true;
			}
			return false;
		}
		m_renderer.enabled = (bool)value;
		return true;
	}

	private Material GetMaterialInstance(Renderer renderer, int index)
	{
		Material originalMat = renderer.GetSharedMaterial(index);
		Material mat = originalMat;
		if (originalMat.name[0] != '?')
		{
			mat = new Material(originalMat);
			mat.name = "?" + mat.name;
			renderer.SetSharedMaterial(index, mat);
		}
		return mat;
	}

	private string GetMaterialPropertyId(ReadOnlySpan<char> idPath)
	{
		ReadOnlySpanExtensions.SplitEnumerator parts = idPath.Split(".");
		int index = 0;
		string retValue = "";
		ReadOnlySpanExtensions.SplitEnumerator enumerator = parts.GetEnumerator();
		while (enumerator.MoveNext())
		{
			ReadOnlySpan<char> span = enumerator.Current;
			if (index == 1)
			{
				retValue = span.ToString();
			}
			index++;
		}
		if (index != 2)
		{
			return "";
		}
		return retValue;
	}

	private bool GetIndex(ReadOnlySpan<char> id, out int index)
	{
		index = 0;
		int idx = id.IndexOf('[') + 1;
		if (idx == 0)
		{
			return false;
		}
		int idx2 = id.Slice(idx).IndexOf(']') - 1 + idx;
		if (idx2 < idx)
		{
			return false;
		}
		int radix = idx2 - idx;
		for (int i = idx; i <= idx2; i++)
		{
			int digit = id[i] - 48;
			index += digit * (int)Mathf.Pow(10f, radix);
			radix--;
		}
		return true;
	}
}
