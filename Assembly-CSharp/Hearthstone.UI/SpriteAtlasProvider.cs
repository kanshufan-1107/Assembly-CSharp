using System;
using System.Collections.Generic;
using Blizzard.T5.AssetManager;
using Blizzard.T5.Core;
using Blizzard.T5.Jobs;
using Blizzard.T5.Services;
using Hearthstone.UI.Internal;
using UnityEngine;
using UnityEngine.U2D;

namespace Hearthstone.UI;

public class SpriteAtlasProvider : IService
{
	private class SpriteAtlasStatus
	{
		public enum LoadState
		{
			NotStarted,
			Loading,
			Success,
			Failed
		}

		public string AtlasTag;

		public AssetHandle<SpriteAtlas> AssetHandle;

		public LoadState State;

		public HashSet<UnityEngine.Object> Dependents = new HashSet<UnityEngine.Object>();

		public Queue<Action<SpriteAtlas>> PendingCallbacksDuringLoad;

		public SpriteAtlasStatus(string atlasTag)
		{
			AtlasTag = atlasTag;
		}

		public void AddPendingCallback(Action<SpriteAtlas> callback)
		{
			if (PendingCallbacksDuringLoad == null)
			{
				PendingCallbacksDuringLoad = new Queue<Action<SpriteAtlas>>();
			}
			PendingCallbacksDuringLoad.Enqueue(callback);
		}

		public void DispatchToPendingCallbacks()
		{
			if (PendingCallbacksDuringLoad != null && PendingCallbacksDuringLoad.Count != 0)
			{
				while (PendingCallbacksDuringLoad.Count > 0)
				{
					PendingCallbacksDuringLoad.Dequeue()?.Invoke(AssetHandle?.Asset);
				}
			}
		}
	}

	private const string LogTag = "[SpriteAtlasProvider]";

	private static SpriteAtlasProvider s_instance;

	private static Queue<(string, Action<SpriteAtlas> atlasCallback)> s_pendingAtlasRequests;

	private static FlagStateTracker s_serviceReadyState;

	private Map<string, SpriteAtlasStatus> m_atlasStatusMap = new Map<string, SpriteAtlasStatus>();

	public static SpriteAtlasProvider Get()
	{
		return s_instance;
	}

	public static void RegisterReadyListener(Action<object> listener)
	{
		s_serviceReadyState.RegisterSetListener(listener, s_instance);
	}

	public static void RemoveReadyListener(Action<object> listener)
	{
		s_serviceReadyState.RemoveSetListener(listener);
	}

	private static void QueuePendingAtlasRequest(string atlasTag, Action<SpriteAtlas> atlasCallback)
	{
		if (!s_serviceReadyState.IsSet)
		{
			s_pendingAtlasRequests.Enqueue((atlasTag, atlasCallback));
			Log.Asset.PrintWarning(string.Format("{0} Service not ready. Queueing up Sprite Atlas request for {1}...(count {2})", "[SpriteAtlasProvider]", atlasTag, s_pendingAtlasRequests.Count));
		}
	}

	static SpriteAtlasProvider()
	{
		s_instance = null;
		s_pendingAtlasRequests = new Queue<(string, Action<SpriteAtlas>)>();
		SpriteAtlasManager.atlasRequested += QueuePendingAtlasRequest;
		SpriteAtlasManager.atlasRegistered += HandleAtlasRegistered;
	}

	public void RegisterSpriteComponent(string atlasTag, Component component)
	{
		SpriteAtlasStatus atlasStatus = GetOrCreateAtlasStatus(atlasTag);
		if (atlasStatus.Dependents.Contains(component))
		{
			Log.Asset.PrintWarning(string.Format("{0} Sprite component {1}{2} is already registered!", "[SpriteAtlasProvider]", component.name, component.GetInstanceID()));
		}
		else
		{
			atlasStatus.Dependents.Add(component);
		}
	}

	public void UnregisterSpriteComponent(string atlasTag, Component component)
	{
		if (!string.IsNullOrEmpty(atlasTag))
		{
			SpriteAtlasStatus atlasStatus = GetOrCreateAtlasStatus(atlasTag);
			if (!atlasStatus.Dependents.Contains(component))
			{
				Log.Asset.PrintWarning(string.Format("{0} Tried to unregister sprite component {1}{2} but there's no entry!", "[SpriteAtlasProvider]", component.name, component.GetInstanceID()));
				return;
			}
			atlasStatus.Dependents.Remove(component);
			UnloadUnusedResources(atlasStatus);
		}
	}

	public Type[] GetDependencies()
	{
		return new Type[2]
		{
			typeof(IAssetLoader),
			typeof(IAliasedAssetResolver)
		};
	}

	public IEnumerator<IAsyncJobResult> Initialize(ServiceLocator serviceLocator)
	{
		SpriteAtlasManager.atlasRequested += HandleAtlasRequest;
		while (s_pendingAtlasRequests.Count > 0)
		{
			(string, Action<SpriteAtlas>) request = s_pendingAtlasRequests.Dequeue();
			HandleAtlasRequest(request.Item1, request.Item2);
		}
		s_pendingAtlasRequests.Clear();
		s_instance = this;
		s_serviceReadyState.SetAndDispatch();
		yield break;
	}

	public void Shutdown()
	{
		SpriteAtlasManager.atlasRequested -= HandleAtlasRequest;
		UnloadAllUnusedResources();
		s_serviceReadyState.Clear();
		s_instance = null;
	}

	private SpriteAtlasStatus GetOrCreateAtlasStatus(string atlasTag)
	{
		if (!m_atlasStatusMap.TryGetValue(atlasTag, out var atlasStatus))
		{
			atlasStatus = new SpriteAtlasStatus(atlasTag);
			m_atlasStatusMap[atlasStatus.AtlasTag] = atlasStatus;
		}
		return atlasStatus;
	}

	private void HandleAtlasRequest(string atlasTag, Action<SpriteAtlas> atlasCallback)
	{
		Log.Asset.PrintInfo("[SpriteAtlasProvider] Request for SpriteAtlas " + atlasTag);
		SpriteAtlasStatus atlasStatus = GetOrCreateAtlasStatus(atlasTag);
		if (atlasStatus.State == SpriteAtlasStatus.LoadState.Failed)
		{
			Log.Asset.PrintError("[SpriteAtlasProvider] Previously failed to load " + atlasTag + ". Returning nothing...");
			atlasCallback(null);
			return;
		}
		if (atlasStatus.AssetHandle != null && atlasStatus.AssetHandle.Asset != null)
		{
			atlasCallback(atlasStatus.AssetHandle.Asset);
			return;
		}
		if (atlasStatus.State == SpriteAtlasStatus.LoadState.Loading)
		{
			atlasStatus.AddPendingCallback(atlasCallback);
			return;
		}
		AssetReference spriteAtlasRef = ServiceManager.Get<IAliasedAssetResolver>().GetSpriteAtlasAssetRefFromTag(atlasTag);
		if (spriteAtlasRef == null)
		{
			Log.Asset.PrintError("[SpriteAtlasProvider] Unable to find asset reference for SpriteAtlas " + atlasTag);
		}
		atlasStatus.AddPendingCallback(atlasCallback);
		atlasStatus.State = SpriteAtlasStatus.LoadState.Loading;
		AssetHandleCallback<SpriteAtlas> callback = delegate(AssetReference assetRef, AssetHandle<SpriteAtlas> assetHandle, object asyncOperationId)
		{
			AssetHandle.Take(ref atlasStatus.AssetHandle, assetHandle);
			atlasStatus.AssetHandle = assetHandle;
			if (assetHandle == null)
			{
				Log.Asset.PrintError("[SpriteAtlasProvider] Failed to get asset handle for SpriteAtlas " + atlasTag);
				atlasStatus.State = SpriteAtlasStatus.LoadState.Failed;
				atlasStatus.DispatchToPendingCallbacks();
			}
			else if (assetHandle.Asset == null)
			{
				Log.Asset.PrintError("[SpriteAtlasProvider] Failed to load SpriteAtlas " + atlasTag);
				atlasStatus.State = SpriteAtlasStatus.LoadState.Failed;
				atlasStatus.DispatchToPendingCallbacks();
			}
			else
			{
				if (atlasStatus.AssetHandle.Asset.spriteCount > 0)
				{
					Sprite[] array = new Sprite[atlasStatus.AssetHandle.Asset.spriteCount];
					atlasStatus.AssetHandle.Asset.GetSprites(array);
					Sprite sprite = array[0];
					Log.Asset.Print("[SpriteAtlasProvider] Loaded SpriteAtlas " + atlasStatus.AtlasTag + " using texture " + sprite.texture.name);
				}
				atlasStatus.State = SpriteAtlasStatus.LoadState.Success;
				atlasStatus.DispatchToPendingCallbacks();
			}
		};
		if (!AssetLoader.Get().LoadAsset(spriteAtlasRef, callback, null, AssetLoadingOptions.DisableLocalization))
		{
			callback(spriteAtlasRef, null, null);
		}
	}

	private void UnloadUnusedResources(SpriteAtlasStatus atlasStatus)
	{
		if (atlasStatus.Dependents.Count == 0)
		{
			Log.Asset.PrintInfo("[SpriteAtlasProvider] " + atlasStatus.AtlasTag + " reached 0 dependent components. Releasing sprite atlas...");
			AssetHandle.SafeDispose(ref atlasStatus.AssetHandle);
			m_atlasStatusMap.Remove(atlasStatus.AtlasTag);
		}
	}

	private void UnloadAllUnusedResources(bool forceUnload = false)
	{
		foreach (KeyValuePair<string, SpriteAtlasStatus> item in m_atlasStatusMap)
		{
			SpriteAtlasStatus atlasStatus = item.Value;
			if (atlasStatus.Dependents.Count == 0 || forceUnload)
			{
				Log.Asset.PrintInfo("[SpriteAtlasProvider] Releasing handle for sprite atlas " + atlasStatus.AtlasTag);
				atlasStatus.AssetHandle.Dispose();
				m_atlasStatusMap.Remove(atlasStatus.AtlasTag);
			}
		}
	}

	private static void HandleAtlasRegistered(SpriteAtlas spriteAtlas)
	{
		Log.Asset.Print("Successfully delivered and registered SpriteAtlas " + spriteAtlas.name);
	}
}
