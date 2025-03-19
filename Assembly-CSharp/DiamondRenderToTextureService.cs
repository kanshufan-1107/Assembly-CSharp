using System;
using System.Collections.Generic;
using Blizzard.T5.Jobs;
using Blizzard.T5.Services;
using Hearthstone.Core;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Rendering;

public class DiamondRenderToTextureService : IService
{
	private struct TextureReference
	{
		public DiamondRenderToTexture Texture;

		public int TextureInstanceId;

		public DiamondRenderToTextureAtlas Atlas;

		public GameObject Container;

		public int RenderingObjectId;

		public bool Remove;
	}

	private const float MIN_OFFSET_DISTANCE = -4000f;

	private const float CAMERA_SIZE = 3.45f;

	private const float CAMERA_NEAR_CLIP = -0.3f;

	private const float CAMERA_FAR_CLIP = 15f;

	private const int LAYER_MASK = 23;

	private const int RENDER_TEXTURE_SIZE = 1024;

	private const float CAMERA_PIXEL_SIZE = 0.0067382813f;

	private static readonly Vector3 CAMERA_OFFSET = new Vector3(0f, 20f, 0f);

	private static readonly Vector3 OFFSCREEN_POS = new Vector3(-4000f, -4000f, -4000f);

	private static readonly Vector3 DEFAULT_ATLAS_POSITION = OFFSCREEN_POS - new Vector3(3.45f, 0f, 3.45f);

	private static readonly ProfilerMarker s_lateUpdateProfiler = new ProfilerMarker("DiamondRenderToTextureAtlas.LateUpdate");

	private static readonly ProfilerMarker s_removeUnusedProfiler = new ProfilerMarker("DiamondRenderToTextureAtlas.RemoveUnusedTextures");

	private static readonly ProfilerMarker s_renderAllAtlasesProfiler = new ProfilerMarker("DiamondRenderToTextureAtlas.RenderAllAtlases");

	private Dictionary<int, TextureReference> m_textures = new Dictionary<int, TextureReference>();

	private List<DiamondRenderToTextureAtlas> m_atlases = new List<DiamondRenderToTextureAtlas>();

	private GameObject m_containerObject;

	private GameObject m_itemsContainerObject;

	private Vector3 m_atlasOriginPosition;

	private int m_lastAddedAtlas;

	private Quaternion m_directionToCamera;

	private CommandBuffer m_atlasFilterCommandBuffer;

	private bool m_dirty;

	private List<DiamondRenderToTexture> m_texturesToRemove = new List<DiamondRenderToTexture>();

	public IEnumerator<IAsyncJobResult> Initialize(ServiceLocator serviceLocator)
	{
		m_lastAddedAtlas = 0;
		SetupObjects();
		Processor.RegisterLateUpdateDelegate(LateUpdate);
		yield break;
	}

	public Type[] GetDependencies()
	{
		return null;
	}

	public void Shutdown()
	{
	}

	private void LateUpdate()
	{
		if (NeedsUpdate())
		{
			RemoveUnusedTextures();
			RenderAllAtlases();
		}
	}

	public bool Register(DiamondRenderToTexture r2t)
	{
		if (!r2t.m_ObjectToRender)
		{
			return false;
		}
		int renderObjectInstanceId = r2t.m_ObjectToRender.GetInstanceID();
		int instanceId = r2t.GetInstanceID();
		if (m_textures.TryGetValue(instanceId, out var reference))
		{
			if (reference.RenderingObjectId == r2t.m_ObjectToRender.GetInstanceID())
			{
				if (reference.Remove)
				{
					reference.Remove = false;
					m_textures[instanceId] = reference;
				}
				return true;
			}
			RemoveTexture(reference);
		}
		if (!r2t.m_AllowRepetition)
		{
			foreach (KeyValuePair<int, TextureReference> texture2 in m_textures)
			{
				if (r2t.IsEqual(texture2.Value.Texture))
				{
					return false;
				}
			}
		}
		DiamondRenderToTextureAtlas atlas = AppendToAtlas(r2t);
		TextureReference textureReference = default(TextureReference);
		textureReference.Texture = r2t;
		textureReference.TextureInstanceId = instanceId;
		textureReference.Atlas = atlas;
		textureReference.RenderingObjectId = renderObjectInstanceId;
		TextureReference textureReference2 = textureReference;
		if (r2t.m_HideRenderObject)
		{
			GameObject containerGameObject = new GameObject("R2T_" + r2t.name);
			containerGameObject.transform.parent = m_itemsContainerObject.transform;
			r2t.transform.parent = containerGameObject.transform;
			r2t.m_ObjectToRender.transform.parent = containerGameObject.transform;
			textureReference2.Container = containerGameObject;
		}
		m_textures.Add(instanceId, textureReference2);
		m_dirty = true;
		return true;
	}

	public void Unregister(DiamondRenderToTexture r2t)
	{
		int instanceId = r2t.GetInstanceID();
		if (m_textures.TryGetValue(instanceId, out var textureReference))
		{
			textureReference.Remove = true;
			m_textures[instanceId] = textureReference;
			m_texturesToRemove.Add(r2t);
			m_dirty = true;
		}
	}

	private void SetupObjects()
	{
		m_containerObject = new GameObject("AtlasedRenderToTexture");
		m_containerObject.transform.position = OFFSCREEN_POS;
		m_itemsContainerObject = new GameObject("Items");
		m_itemsContainerObject.transform.parent = m_containerObject.transform;
		UnityEngine.Object.DontDestroyOnLoad(m_containerObject);
		m_directionToCamera = Quaternion.LookRotation(Vector3.up, Vector3.forward);
	}

	private bool NeedsUpdate()
	{
		return m_dirty;
	}

	private void RemoveUnusedTextures()
	{
		if (m_texturesToRemove.Count <= 0)
		{
			return;
		}
		foreach (DiamondRenderToTexture item in m_texturesToRemove)
		{
			int instanceId = item.GetInstanceID();
			if (m_textures.TryGetValue(instanceId, out var textureReference) && textureReference.Remove)
			{
				RemoveTexture(textureReference);
			}
		}
		CleanAtlases();
		m_texturesToRemove.Clear();
	}

	private void RemoveTexture(TextureReference reference)
	{
		reference.Atlas.Remove(reference.TextureInstanceId);
		m_textures.Remove(reference.TextureInstanceId);
		if ((bool)reference.Container)
		{
			reference.Texture.RestoreOriginalParents();
			UnityEngine.Object.Destroy(reference.Container);
			reference.Container = null;
		}
	}

	private DiamondRenderToTextureAtlas AppendToAtlas(DiamondRenderToTexture r2t)
	{
		foreach (DiamondRenderToTextureAtlas atlas in m_atlases)
		{
			if (atlas.Insert(r2t))
			{
				return atlas;
			}
		}
		m_atlases.Add(new DiamondRenderToTextureAtlas(m_lastAddedAtlas, 1024, 1024));
		m_lastAddedAtlas++;
		DiamondRenderToTextureAtlas diamondRenderToTextureAtlas = m_atlases[m_lastAddedAtlas - 1];
		diamondRenderToTextureAtlas.Insert(r2t);
		return diamondRenderToTextureAtlas;
	}

	private void RenderAllAtlases()
	{
		using (s_renderAllAtlasesProfiler.Auto())
		{
			bool hasRealtimeAtlas = false;
			m_atlasOriginPosition = DEFAULT_ATLAS_POSITION;
			foreach (DiamondRenderToTextureAtlas atlas in m_atlases)
			{
				if (atlas.Dirty || atlas.IsRealTime)
				{
					RenderAtlas(atlas, m_atlasOriginPosition);
				}
				hasRealtimeAtlas |= atlas.IsRealTime;
			}
			m_dirty = hasRealtimeAtlas;
		}
	}

	private void RenderAtlas(DiamondRenderToTextureAtlas atlas, Vector3 atlasOrigin)
	{
		foreach (DiamondRenderToTextureAtlas.RegisteredTexture texture in atlas.RegisteredTextures)
		{
			DiamondRenderToTexture atlasedTexture = texture.DiamondRenderToTexture;
			if ((bool)atlasedTexture)
			{
				atlasedTexture.PushTransform();
				if (atlasedTexture.m_HideRenderObject)
				{
					atlasedTexture.m_ObjectToRender.SetActive(value: true);
				}
				PositionObjectForAtlas(texture, atlasOrigin);
			}
		}
		atlas.Render();
		foreach (DiamondRenderToTextureAtlas.RegisteredTexture registeredTexture in atlas.RegisteredTextures)
		{
			DiamondRenderToTexture atlasedTexture2 = registeredTexture.DiamondRenderToTexture;
			if ((bool)atlasedTexture2 && !atlasedTexture2.m_HideRenderObject)
			{
				atlasedTexture2.HasAtlasPosition = false;
				atlasedTexture2.PopTransform();
			}
		}
		atlas.Dirty = false;
	}

	private void PositionObjectForAtlas(DiamondRenderToTextureAtlas.RegisteredTexture texture, Vector3 atlasPosition)
	{
		DiamondRenderToTexture atlasedTexture = texture.DiamondRenderToTexture;
		if (atlasedTexture.m_HideRenderObject && atlasedTexture.HasAtlasPosition && atlasedTexture.MaintainsAtlasPosition())
		{
			atlasedTexture.transform.hasChanged = false;
			return;
		}
		if (!atlasedTexture.m_ObjectToRender.activeInHierarchy)
		{
			atlasedTexture.transform.hasChanged = false;
			return;
		}
		atlasedTexture.ResetTransform(atlasPosition);
		if (atlasedTexture.HasAtlasPosition)
		{
			atlasedTexture.RestoreAtlasPosition();
		}
		else
		{
			float scale = ScaleObjectToAtlasPosition(texture);
			Quaternion rotation = RotateTowardsCamera(texture);
			MoveToAtlasPosition(texture, atlasPosition, scale, rotation);
		}
		atlasedTexture.CaptureAtlasPosition();
		atlasedTexture.RestoreParents();
		atlasedTexture.transform.hasChanged = false;
	}

	private float ScaleObjectToAtlasPosition(DiamondRenderToTextureAtlas.RegisteredTexture texture)
	{
		DiamondRenderToTexture atlasedTexture = texture.DiamondRenderToTexture;
		float objectScaleInY = 0.0067382813f * (float)texture.AtlasPosition.height / atlasedTexture.WorldBounds.x;
		float packObjectScale = Mathf.Max(0.0067382813f * (float)texture.AtlasPosition.width / atlasedTexture.WorldBounds.y, objectScaleInY);
		Vector3 desiredScale = Vector3.one * packObjectScale;
		Transform objectToRenderTransform = atlasedTexture.m_ObjectToRender.transform;
		Vector3 objectToRenderTransformLocalScale = objectToRenderTransform.localScale;
		if (objectToRenderTransformLocalScale == desiredScale)
		{
			return 1f;
		}
		objectToRenderTransformLocalScale *= packObjectScale;
		objectToRenderTransform.localScale = objectToRenderTransformLocalScale;
		atlasedTexture.transform.localScale *= packObjectScale;
		return packObjectScale;
	}

	private void MoveToAtlasPosition(DiamondRenderToTextureAtlas.RegisteredTexture texture, Vector3 atlasOrigin, float scaleApplied, Quaternion rotationApplied)
	{
		DiamondRenderToTexture atlasedTexture = texture.DiamondRenderToTexture;
		Transform transform = atlasedTexture.transform;
		Vector3 atlasOffset = 0.0067382813f * new Vector3(texture.AtlasPosition.x, 0f, texture.AtlasPosition.y);
		atlasedTexture.m_ObjectToRender.transform.position = atlasOrigin + atlasOffset - atlasedTexture.WorldPivotOffset;
		transform.position = atlasOrigin + atlasOffset - atlasedTexture.WorldPivotOffset;
	}

	private Quaternion RotateTowardsCamera(DiamondRenderToTextureAtlas.RegisteredTexture texture)
	{
		DiamondRenderToTexture diamondRenderToTexture = texture.DiamondRenderToTexture;
		Transform objectToRenderTransform = diamondRenderToTexture.m_ObjectToRender.transform;
		Transform transform = diamondRenderToTexture.transform;
		Quaternion desiredRotation = Quaternion.RotateTowards(transform.rotation, m_directionToCamera, 360f);
		transform.rotation = desiredRotation;
		objectToRenderTransform.eulerAngles = Vector3.zero;
		return Quaternion.identity;
	}

	private void CleanAtlases()
	{
		for (int i = m_atlases.Count - 1; i >= 0; i--)
		{
			DiamondRenderToTextureAtlas atlas = m_atlases[i];
			if (atlas.IsEmpty())
			{
				atlas.Destroy();
				m_atlases.RemoveAt(i);
				m_lastAddedAtlas--;
			}
		}
	}
}
