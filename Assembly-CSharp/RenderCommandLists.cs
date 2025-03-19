using System.Collections.Generic;
using Blizzard.T5.MaterialService.Extensions;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Rendering;

public class RenderCommandLists
{
	public struct MaterialOveride
	{
		public int meshIndex;

		public Material materialToUse;

		public MaterialOveride(Material toUse, int meshIdx = -1)
		{
			materialToUse = toUse;
			meshIndex = meshIdx;
		}
	}

	public class MatOverrideDictionary : Dictionary<Renderer, List<MaterialOveride>>
	{
		public void Add(Renderer key, MaterialOveride matOverride)
		{
			if (TryGetValue(key, out var val))
			{
				val.Add(matOverride);
				return;
			}
			val = new List<MaterialOveride>(1);
			val.Add(matOverride);
			Add(key, val);
		}
	}

	public List<RenderCommand> OpaqueRenderCommands;

	public List<RenderCommand> TransparentRenderCommands;

	private int[] foundPasses = new int[3];

	private List<Material> matPool = new List<Material>(5);

	private static ShaderTagId s_lightModeTag = new ShaderTagId("LightMode");

	private static ShaderTagId s_defaultLightModeTag = new ShaderTagId("SRPDefaultUnlit");

	private static ShaderTagId[] s_lightModesToInclude = new ShaderTagId[3]
	{
		new ShaderTagId("SRPDefaultUnlit"),
		new ShaderTagId("UniversalForward"),
		new ShaderTagId("LightweightForward")
	};

	private static ProfilerMarker s_RTTCreateCommands = new ProfilerMarker("RTT_CreateCommands");

	public RenderCommandLists()
	{
		OpaqueRenderCommands = new List<RenderCommand>();
		TransparentRenderCommands = new List<RenderCommand>();
	}

	public void Clear()
	{
		OpaqueRenderCommands.Clear();
		TransparentRenderCommands.Clear();
	}

	private int SortRenderCommands(RenderCommand a, RenderCommand b)
	{
		if (a.Renderer.sortingOrder == b.Renderer.sortingOrder)
		{
			return a.Material.renderQueue - b.Material.renderQueue;
		}
		return a.Renderer.sortingOrder - b.Renderer.sortingOrder;
	}

	public void AppendRenderCommands(GameObject objectToDraw, bool includeInactiveRenderers = false, MatOverrideDictionary overrides = null)
	{
		Renderer[] toDraw = objectToDraw.GetComponentsInChildren<Renderer>(includeInactiveRenderers);
		AppendRenderCommands(toDraw, overrides);
	}

	public void AppendRenderCommands(Renderer[] toDraw, MatOverrideDictionary overrides = null)
	{
		foreach (Renderer renderer in toDraw)
		{
			List<MaterialOveride> matOverrides = null;
			bool fullRendererOverride = false;
			if (overrides != null)
			{
				overrides.TryGetValue(renderer, out matOverrides);
				if (matOverrides != null)
				{
					foreach (MaterialOveride item2 in matOverrides)
					{
						if (item2.meshIndex == -1)
						{
							fullRendererOverride = true;
							if (matOverrides.Count > 1)
							{
								Debug.LogError("Multiple overrides passed when a global override is active. This is not supported");
							}
							break;
						}
					}
				}
			}
			if (fullRendererOverride)
			{
				matPool.Add(matOverrides[0].materialToUse);
			}
			else if (renderer.HasCustomMaterials())
			{
				renderer.GetMaterialsToExistingList(matPool);
			}
			else
			{
				renderer.GetSharedMaterials(matPool);
				foreach (Material item3 in matPool)
				{
					if (item3 == null)
					{
						matPool.Clear();
						renderer.GetMaterialsToExistingList(matPool);
						break;
					}
				}
			}
			List<Material> sharedMaterials = matPool;
			MeshRenderer obj = renderer as MeshRenderer;
			int totalMeshCount = 1;
			if ((bool)obj)
			{
				bool isText = false;
				MeshFilter meshFilter = null;
				TextMesh textMesh = null;
				bool num = renderer.TryGetComponent<MeshFilter>(out meshFilter);
				if (!num)
				{
					isText = renderer.TryGetComponent<TextMesh>(out textMesh);
				}
				if (num && (bool)meshFilter.sharedMesh)
				{
					totalMeshCount = meshFilter.sharedMesh.subMeshCount;
				}
				else if (isText)
				{
					totalMeshCount = sharedMaterials.Count;
				}
			}
			else
			{
				SkinnedMeshRenderer skinnedMeshRenderer = renderer as SkinnedMeshRenderer;
				if ((bool)skinnedMeshRenderer)
				{
					totalMeshCount = skinnedMeshRenderer.sharedMesh.subMeshCount;
				}
			}
			for (int meshIndex = 0; meshIndex < totalMeshCount; meshIndex++)
			{
				int materialIndex = meshIndex;
				if (materialIndex >= sharedMaterials.Count)
				{
					materialIndex = 0;
				}
				Material material = sharedMaterials[materialIndex];
				if (!fullRendererOverride && matOverrides != null)
				{
					foreach (MaterialOveride item in matOverrides)
					{
						if (item.meshIndex == meshIndex)
						{
							material = item.materialToUse;
							break;
						}
					}
				}
				for (int j = 0; j < foundPasses.Length; j++)
				{
					foundPasses[j] = -1;
				}
				int numFoundPasses = 0;
				for (int passIdx = 0; passIdx < material.passCount; passIdx++)
				{
					ShaderTagId tagValue = material.shader.FindPassTagValue(passIdx, s_lightModeTag);
					if (tagValue == ShaderTagId.none)
					{
						tagValue = s_defaultLightModeTag;
					}
					for (int tagIdx = 0; tagIdx < s_lightModesToInclude.Length; tagIdx++)
					{
						if (tagValue == s_lightModesToInclude[tagIdx] && foundPasses[tagIdx] == -1)
						{
							foundPasses[tagIdx] = passIdx;
							numFoundPasses++;
							break;
						}
					}
					if (numFoundPasses == foundPasses.Length)
					{
						break;
					}
				}
				int[] array;
				if (material.renderQueue < 3000)
				{
					array = foundPasses;
					foreach (int passIdx2 in array)
					{
						if (passIdx2 != -1)
						{
							OpaqueRenderCommands.Add(new RenderCommand
							{
								Renderer = renderer,
								Material = material,
								MeshIndex = meshIndex,
								passIndex = passIdx2
							});
						}
					}
					continue;
				}
				array = foundPasses;
				foreach (int passIdx3 in array)
				{
					if (passIdx3 != -1)
					{
						TransparentRenderCommands.Add(new RenderCommand
						{
							Renderer = renderer,
							Material = material,
							MeshIndex = meshIndex,
							passIndex = passIdx3
						});
					}
				}
			}
			matPool.Clear();
		}
		OpaqueRenderCommands.Sort(SortRenderCommands);
		TransparentRenderCommands.Sort(SortRenderCommands);
	}
}
