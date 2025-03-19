using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Rendering;

public class DiamondRenderToTextureAtlas : GreedyPacker
{
	public struct RegisteredTexture
	{
		public DiamondRenderToTexture DiamondRenderToTexture;

		public int DiamondRenderToTextureInstanceId;

		public RectInt AtlasPosition;
	}

	private const int RTT_DEPTH_BUFFER_SIZE = 16;

	private const float PADDING = 0.5f;

	private const float MARGIN = 1f;

	[CompilerGenerated]
	private readonly int _003CIndex_003Ek__BackingField;

	private readonly Vector2Int m_size;

	private readonly int m_totalPixelSpace;

	private int m_totalPixelUsed;

	private CommandBuffer m_opaqueCommandBuffer;

	private CommandBuffer m_transparentCommandBuffer;

	private List<RenderToTexturePostProcess> m_renderPostProcessList;

	public RenderTexture Texture { get; }

	public bool IsRealTime { get; private set; }

	public bool Dirty { get; set; }

	public List<RegisteredTexture> RegisteredTextures { get; private set; }

	public Color ClearColor { get; private set; } = Color.clear;

	public DiamondRenderToTextureAtlas(int index, int width, int height, RenderTextureFormat format = RenderTextureFormat.ARGB32)
		: base(width, height)
	{
		_003CIndex_003Ek__BackingField = index;
		Texture = RenderTextureTracker.Get().CreateNewTexture(width, height, 16, format);
		Texture.useMipMap = false;
		Texture.autoGenerateMips = false;
		m_size = new Vector2Int(width, height);
		m_totalPixelSpace = width * height;
		RegisteredTextures = new List<RegisteredTexture>();
		m_renderPostProcessList = new List<RenderToTexturePostProcess>();
	}

	public bool Insert(DiamondRenderToTexture r2t)
	{
		int freeSpace = GetFreeSpace();
		if (r2t.TextureSize.x * r2t.TextureSize.y > freeSpace)
		{
			return false;
		}
		bool isFirstTexture = RegisteredTextures.Count == 0;
		if (!isFirstTexture && !UsesSamePostProcess(r2t))
		{
			return false;
		}
		if (isFirstTexture)
		{
			ClearColor = r2t.m_ClearColor;
		}
		RectInt atlasPosition = Insert(r2t.TextureSize.x, r2t.TextureSize.y);
		if (atlasPosition.x == -1 && atlasPosition.y == -1)
		{
			return false;
		}
		RegisteredTextures.Add(new RegisteredTexture
		{
			DiamondRenderToTexture = r2t,
			DiamondRenderToTextureInstanceId = r2t.GetInstanceID(),
			AtlasPosition = atlasPosition
		});
		r2t.OnAddedToAtlas(Texture, new Rect((float)atlasPosition.xMin / (float)m_size.x, (float)atlasPosition.yMin / (float)m_size.y, (float)atlasPosition.width / (float)m_size.x, (float)atlasPosition.height / (float)m_size.y));
		BuildCommandBuffers();
		if (isFirstTexture)
		{
			SetupPostProcess(r2t);
		}
		m_totalPixelUsed += atlasPosition.width * atlasPosition.height;
		if (r2t.m_RealtimeRender)
		{
			IsRealTime = true;
		}
		Dirty = true;
		return true;
	}

	public bool Remove(int r2tInstanceId)
	{
		bool removed = false;
		for (int i = RegisteredTextures.Count - 1; i >= 0; i--)
		{
			RegisteredTexture registeredTexture = RegisteredTextures[i];
			if (!registeredTexture.DiamondRenderToTexture || registeredTexture.DiamondRenderToTextureInstanceId == r2tInstanceId)
			{
				RectInt atlasPosition = registeredTexture.AtlasPosition;
				m_totalPixelUsed -= atlasPosition.width * atlasPosition.height;
				Remove(registeredTexture.AtlasPosition);
				RegisteredTextures.RemoveAt(i);
				removed = true;
			}
		}
		if (removed)
		{
			BuildCommandBuffers();
			Dirty = true;
			return true;
		}
		return false;
	}

	public bool IsEmpty()
	{
		return RegisteredTextures.Count == 0;
	}

	public void Destroy()
	{
		RenderTextureTracker.Get().ReleaseRenderTexture(Texture);
		foreach (RenderToTexturePostProcess renderPostProcess in m_renderPostProcessList)
		{
			renderPostProcess.End();
		}
	}

	public void Render()
	{
		Camera.SetupCurrent(CameraUtils.GetMainCamera());
		Graphics.ExecuteCommandBuffer(m_opaqueCommandBuffer);
		if (m_renderPostProcessList.Count > 0)
		{
			foreach (RenderToTexturePostProcess renderPostProcess in m_renderPostProcessList)
			{
				renderPostProcess.AddCommandBuffers();
			}
		}
		Graphics.ExecuteCommandBuffer(m_transparentCommandBuffer);
	}

	private void BuildCommandBuffers()
	{
		m_opaqueCommandBuffer = new CommandBuffer
		{
			name = "AtlasOpaqueRender"
		};
		m_transparentCommandBuffer = new CommandBuffer
		{
			name = "AtlasTransparentRender"
		};
		Matrix4x4 ortho = Matrix4x4.Ortho(-3.45f, 3.45f, -3.45f, 3.45f, -0.3f, 15f);
		Vector3 cmdCameraPos = new Vector3(-4000f, -3990f, -4000f);
		Matrix4x4 view = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(1f, 1f, -1f)) * Matrix4x4.LookAt(cmdCameraPos, cmdCameraPos + Vector3.down, Vector3.forward).inverse;
		m_opaqueCommandBuffer.SetViewProjectionMatrices(view, ortho);
		m_transparentCommandBuffer.SetViewProjectionMatrices(view, ortho);
		m_opaqueCommandBuffer.SetRenderTarget(Texture);
		m_opaqueCommandBuffer.ClearRenderTarget(clearDepth: true, clearColor: true, ClearColor);
		m_transparentCommandBuffer.SetRenderTarget(Texture, RenderBufferLoadAction.Load, RenderBufferStoreAction.Store, RenderBufferLoadAction.Load, RenderBufferStoreAction.DontCare);
		foreach (RegisteredTexture texture in RegisteredTextures)
		{
			if (!texture.DiamondRenderToTexture || !texture.DiamondRenderToTexture.enabled)
			{
				continue;
			}
			RectInt atlasPosition = texture.AtlasPosition;
			Rect scissorRect = new Rect((float)atlasPosition.xMin + 0.5f, (float)atlasPosition.yMin + 0.5f, (float)atlasPosition.width - 1f, (float)atlasPosition.height - 1f);
			m_opaqueCommandBuffer.EnableScissorRect(scissorRect);
			m_transparentCommandBuffer.EnableScissorRect(scissorRect);
			foreach (RenderCommand command in texture.DiamondRenderToTexture.RenderCommands.OpaqueRenderCommands)
			{
				m_opaqueCommandBuffer.DrawRenderer(command.Renderer, command.Material, command.MeshIndex, command.passIndex);
			}
			foreach (RenderCommand command2 in texture.DiamondRenderToTexture.RenderCommands.TransparentRenderCommands)
			{
				m_transparentCommandBuffer.DrawRenderer(command2.Renderer, command2.Material, command2.MeshIndex, command2.passIndex);
			}
			m_opaqueCommandBuffer.DisableScissorRect();
			m_transparentCommandBuffer.DisableScissorRect();
		}
	}

	private int GetFreeSpace()
	{
		return m_totalPixelSpace - m_totalPixelUsed;
	}

	private bool UsesSamePostProcess(DiamondRenderToTexture r2t)
	{
		if (r2t.m_ClearColor != ClearColor)
		{
			return false;
		}
		foreach (RenderToTexturePostProcess renderPostProcess in m_renderPostProcessList)
		{
			if (!renderPostProcess.IsUsedBy(r2t))
			{
				return false;
			}
		}
		return true;
	}

	private void SetupPostProcess(DiamondRenderToTexture r2t)
	{
		if (r2t.m_OpaqueObjectAlphaFill)
		{
			RenderToTextureOpaqueAlphaFill opaqueAlphaFillPostProcess = new RenderToTextureOpaqueAlphaFill();
			opaqueAlphaFillPostProcess.Init(m_size.x, m_size.y, r2t.m_ClearColor, Texture);
			m_renderPostProcessList.Add(opaqueAlphaFillPostProcess);
		}
	}
}
