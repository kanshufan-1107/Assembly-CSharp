using Unity.Profiling;
using UnityEngine;
using UnityEngine.Rendering;

public class RenderToTextureUtils
{
	public class RenderCommandListPool
	{
		private static ObjectPool<RenderCommandLists> s_commandListPool = new ObjectPool<RenderCommandLists>(null, delegate(RenderCommandLists x)
		{
			x.Clear();
		});

		public static RenderCommandLists Get()
		{
			return s_commandListPool.Get();
		}

		public static RenderCommandLists Get(Renderer[] toDraw, RenderCommandLists.MatOverrideDictionary overrides = null)
		{
			RenderCommandLists renderCommandLists = Get();
			renderCommandLists.AppendRenderCommands(toDraw, overrides);
			return renderCommandLists;
		}

		public static RenderCommandLists Get(GameObject objectToDraw, bool includeInactiveRenderers = false, RenderCommandLists.MatOverrideDictionary overrides = null)
		{
			RenderCommandLists renderCommandLists = Get();
			renderCommandLists.AppendRenderCommands(objectToDraw, includeInactiveRenderers, overrides);
			return renderCommandLists;
		}

		public static void Release(RenderCommandLists list)
		{
			if (list != null)
			{
				s_commandListPool.Release(list);
			}
		}
	}

	public struct LightWeightCamera
	{
		public Color backgroundColor;

		public LayerMask cullingMask;

		public float aspectRatio;

		public Matrix4x4 worldToCameraMatrix { get; set; }

		public Matrix4x4 projectionMatrix { get; set; }

		public LightWeightCamera(LightWeightCamera rhs)
		{
			worldToCameraMatrix = rhs.worldToCameraMatrix;
			projectionMatrix = rhs.projectionMatrix;
			backgroundColor = rhs.backgroundColor;
			cullingMask = rhs.cullingMask;
			aspectRatio = rhs.aspectRatio;
		}

		public void SetOrthoProjectionMatrix(float orthographicSize, float nearClip, float farClip)
		{
			projectionMatrix = Matrix4x4.Ortho((0f - orthographicSize) * aspectRatio, orthographicSize * aspectRatio, 0f - orthographicSize, orthographicSize, nearClip, farClip);
		}

		public void SetWorldToCameraMatrix(Transform obj)
		{
			worldToCameraMatrix = Matrix4x4.Inverse(Matrix4x4.TRS(obj.position, obj.rotation, new Vector3(1f, 1f, -1f)));
		}
	}

	private static ProfilerMarker s_RTTRenderCamera = new ProfilerMarker("RTT_RenderCamera");

	public static Bounds CalcRendererBounds(Renderer[] toInclude)
	{
		Bounds toReturn = default(Bounds);
		foreach (Renderer render in toInclude)
		{
			if (toReturn.size == Vector3.zero)
			{
				toReturn = render.bounds;
			}
			else
			{
				toReturn.Encapsulate(render.bounds);
			}
		}
		return toReturn;
	}

	private static void DrawCommand(CommandBuffer cmd, LayerMask cullingMask, RenderCommand command)
	{
		int objectLayer = 1 << command.Renderer.gameObject.layer;
		if (((int)cullingMask & objectLayer) != 0 && command.Renderer.enabled)
		{
			cmd.DrawRenderer(command.Renderer, command.Material, command.MeshIndex, command.passIndex);
		}
	}

	private static void DrawCommandReplacement(CommandBuffer cmd, LayerMask cullingMask, RenderCommand command, Material replacementShaderMaterial, string replacementTag, string replacementVal)
	{
		int objectLayer = 1 << command.Renderer.gameObject.layer;
		if (((int)cullingMask & objectLayer) != 0 && command.Renderer.enabled && command.Material.GetTag(replacementTag, searchFallbacks: false) == replacementVal)
		{
			cmd.DrawRenderer(command.Renderer, replacementShaderMaterial, command.MeshIndex);
		}
	}

	public static void RenderCamera(CommandBuffer cmd, RenderTexture rt, LightWeightCamera camera, RenderCommandLists renderCommands, Shader replacementShader = null, string replacementTag = "")
	{
		cmd.SetRenderTarget(rt);
		cmd.SetViewProjectionMatrices(camera.worldToCameraMatrix, camera.projectionMatrix);
		cmd.ClearRenderTarget(clearDepth: true, clearColor: true, camera.backgroundColor);
		if ((bool)replacementShader)
		{
			Material tempMat = new Material(replacementShader);
			string replacementVal = tempMat.GetTag(replacementTag, searchFallbacks: false);
			foreach (RenderCommand command in renderCommands.OpaqueRenderCommands)
			{
				DrawCommandReplacement(cmd, camera.cullingMask, command, tempMat, replacementTag, replacementVal);
			}
			{
				foreach (RenderCommand command2 in renderCommands.TransparentRenderCommands)
				{
					DrawCommandReplacement(cmd, camera.cullingMask, command2, tempMat, replacementTag, replacementVal);
				}
				return;
			}
		}
		foreach (RenderCommand command3 in renderCommands.OpaqueRenderCommands)
		{
			DrawCommand(cmd, camera.cullingMask, command3);
		}
		foreach (RenderCommand command4 in renderCommands.TransparentRenderCommands)
		{
			DrawCommand(cmd, camera.cullingMask, command4);
		}
	}
}
