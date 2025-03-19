using System;
using System.Collections.Generic;
using Blizzard.T5.AssetManager;
using Blizzard.T5.Core;
using Blizzard.T5.Core.Utils;
using Blizzard.T5.Services;
using Cysharp.Threading.Tasks;
using Unity.Profiling;
using UnityEngine;

public class PrefabInstantiator : IPrefabInstantiator
{
	private class PendingRequest
	{
		public object callerData;

		public PrefabInstantiatorCB<AssetHandle<GameObject>> callerCallback;
	}

	private readonly Vector3 SPAWN_POS_CAMERA_OFFSET = new Vector3(0f, 0f, -5000f);

	private readonly List<GameObject> m_waitingOnObjects = new List<GameObject>();

	private readonly AssetHandleCollection m_sharedPrefabHandles;

	private readonly Dictionary<string, GameObject> m_sharedPrefabInstances = new Dictionary<string, GameObject>();

	private readonly Dictionary<string, List<PendingRequest>> m_pendingSharedInstanceRequests = new Dictionary<string, List<PendingRequest>>();

	private readonly Blizzard.T5.Core.ILogger m_logger;

	private readonly ProfilerMarker s_releaseUnreferencedAssetsProfiler = new ProfilerMarker("PrefabInstantiator.ReleaseUnreferencedAssets");

	private readonly ProfilerMarker s_checkForDeadHandlesProfiler = new ProfilerMarker("PrefabInstantiator.CheckForDeadHandles");

	public event Action<string, string> OnSharedPrefabHandleOrphaned
	{
		add
		{
			m_sharedPrefabHandles.OnOrphanedHandleDetected += value;
		}
		remove
		{
			m_sharedPrefabHandles.OnOrphanedHandleDetected -= value;
		}
	}

	public PrefabInstantiator(Blizzard.T5.Core.ILogger logger)
	{
		m_logger = logger;
		m_sharedPrefabHandles = new AssetHandleCollection(logger);
		m_sharedPrefabHandles.OnLastHandleReleased += OnSharedPrefabReleased;
	}

	public GameObject InstantiatePrefab(AssetHandle<GameObject> prefabAsset, AssetLoadingOptions options)
	{
		if (!prefabAsset)
		{
			return null;
		}
		AssetLoader.Get()?.RecordInstantiatingPrefab(prefabAsset.Asset.GetFullPath());
		GameObject instance = (options.HasFlag(AssetLoadingOptions.IgnorePrefabPosition) ? ((GameObject)UnityEngine.Object.Instantiate((UnityEngine.Object)(GameObject)prefabAsset, NewGameObjectSpawnPosition(), prefabAsset.Asset.transform.rotation)) : ((GameObject)UnityEngine.Object.Instantiate((UnityEngine.Object)(GameObject)prefabAsset)));
		AssetHandle newHandle = prefabAsset.Share();
		ServiceManager.Get<DisposablesCleaner>()?.Attach(instance, newHandle);
		return instance;
	}

	public bool InstantiatePrefab(AssetHandle<GameObject> prefabAsset, PrefabInstantiatorCB<GameObject> callback, object callbackData, AssetLoadingOptions options)
	{
		if (!prefabAsset)
		{
			if (GeneralUtils.IsCallbackValid(callback))
			{
				callback(null, null, callbackData);
			}
			return false;
		}
		InstantiateAndWaitThenCallGameObjectCallback(prefabAsset.Share(), options, callback, callbackData).Forget();
		return true;
	}

	public AssetHandle<GameObject> GetOrInstantiateSharedPrefab(AssetHandle<GameObject> prefabAsset, AssetLoadingOptions options)
	{
		if (!prefabAsset)
		{
			return null;
		}
		if (!TryGetSharedInstance(prefabAsset.AssetAddress, out var instance))
		{
			instance = InstantiatePrefab(prefabAsset, options);
			if (instance != null)
			{
				m_sharedPrefabInstances.Add(prefabAsset.AssetAddress, instance);
			}
		}
		if (instance == null)
		{
			return null;
		}
		AssetHandle<GameObject> returnedHandle = new AssetHandle<GameObject>(prefabAsset.AssetAddress, instance);
		m_sharedPrefabHandles.StartTrackingHandle(returnedHandle);
		return returnedHandle;
	}

	public bool GetOrInstantiateSharedPrefab(AssetHandle<GameObject> prefabAsset, PrefabInstantiatorCB<AssetHandle<GameObject>> callback, object callbackData, AssetLoadingOptions options)
	{
		if (!prefabAsset)
		{
			return false;
		}
		if (TryGetSharedInstance(prefabAsset.AssetAddress, out var sharedInstance))
		{
			AssetHandle<GameObject> newHandle = new AssetHandle<GameObject>(prefabAsset.AssetAddress, sharedInstance);
			m_sharedPrefabHandles.StartTrackingHandle(newHandle);
			if (GeneralUtils.IsCallbackValid(callback))
			{
				callback(prefabAsset.AssetAddress, newHandle, callbackData);
			}
			return true;
		}
		PendingRequest newRequest = new PendingRequest
		{
			callerCallback = callback,
			callerData = callbackData
		};
		if (m_pendingSharedInstanceRequests.TryGetValue(prefabAsset.AssetAddress, out var pendingRequests))
		{
			pendingRequests.Add(newRequest);
			return true;
		}
		pendingRequests = new List<PendingRequest> { newRequest };
		m_pendingSharedInstanceRequests.Add(prefabAsset.AssetAddress, pendingRequests);
		return InstantiatePrefab(prefabAsset, OnSharedPrefabInstantiated, null, options);
	}

	public bool IsWaitingOnObject(GameObject go)
	{
		return m_waitingOnObjects.Contains(go);
	}

	public bool IsSharedPrefabInstance(GameObject go)
	{
		if (!go)
		{
			return false;
		}
		foreach (GameObject sharedInstance in m_sharedPrefabInstances.Values)
		{
			if (go == sharedInstance)
			{
				return true;
			}
		}
		return false;
	}

	public void ReleaseUnreferencedAssets()
	{
		m_sharedPrefabHandles.ReleaseUnreferencedAssets();
	}

	public bool CheckForDeadHandles(double maximumProcessingTimeMilleseconds = 1000.0)
	{
		using (s_checkForDeadHandlesProfiler.Auto())
		{
			return m_sharedPrefabHandles.CheckForDeadHandles(maximumProcessingTimeMilleseconds);
		}
	}

	private async UniTaskVoid InstantiateAndWaitThenCallGameObjectCallback(AssetHandle<GameObject> prefabHandle, AssetLoadingOptions options, PrefabInstantiatorCB<GameObject> callback, object callbackData)
	{
		using (prefabHandle)
		{
			GameObject instance = (options.HasFlag(AssetLoadingOptions.IgnorePrefabPosition) ? UnityEngine.Object.Instantiate(prefabHandle.Asset, NewGameObjectSpawnPosition(), prefabHandle.Asset.transform.rotation) : UnityEngine.Object.Instantiate(prefabHandle.Asset));
			ServiceManager.Get<DisposablesCleaner>()?.Attach(instance, prefabHandle.Share());
			m_waitingOnObjects.Add(instance);
			await UniTask.WaitForEndOfFrame();
			m_waitingOnObjects.Remove(instance);
			if (GeneralUtils.IsCallbackValid(callback))
			{
				callback(prefabHandle.AssetAddress, instance, callbackData);
			}
		}
	}

	private void OnSharedPrefabInstantiated(string prefabAddress, GameObject instance, object callbackData)
	{
		if (TryGetSharedInstance(prefabAddress, out var preExistingInstance))
		{
			UnityEngine.Object.Destroy(instance);
			instance = preExistingInstance;
		}
		else
		{
			m_sharedPrefabInstances.Add(prefabAddress, instance);
		}
		if (!m_pendingSharedInstanceRequests.TryGetValue(prefabAddress, out var _))
		{
			return;
		}
		foreach (PendingRequest pendingRequest in m_pendingSharedInstanceRequests[prefabAddress])
		{
			AssetHandle<GameObject> instanceHandle = new AssetHandle<GameObject>(prefabAddress, instance);
			m_sharedPrefabHandles.StartTrackingHandle(instanceHandle);
			if (GeneralUtils.IsCallbackValid(pendingRequest.callerCallback))
			{
				pendingRequest.callerCallback(prefabAddress, instanceHandle, pendingRequest.callerData);
			}
		}
		m_pendingSharedInstanceRequests.Remove(prefabAddress);
	}

	private void OnSharedPrefabReleased(string prefabAddress)
	{
		if (TryGetSharedInstance(prefabAddress, out var instance))
		{
			UnityEngine.Object.Destroy(instance);
			m_sharedPrefabInstances.Remove(prefabAddress);
		}
	}

	private bool TryGetSharedInstance(string prefabAddress, out GameObject instance)
	{
		if (m_sharedPrefabInstances.TryGetValue(prefabAddress, out instance))
		{
			if (!instance)
			{
				instance = null;
				m_logger.Log(LogLevel.Warning, "PrefabInstantiator found destroyed shared instance. This is unexpected.", prefabAddress);
				m_sharedPrefabInstances.Remove(prefabAddress);
				return false;
			}
			return true;
		}
		return false;
	}

	private Vector3 NewGameObjectSpawnPosition()
	{
		if (Camera.main == null)
		{
			return Vector3.zero;
		}
		return Camera.main.transform.position + SPAWN_POS_CAMERA_OFFSET;
	}
}
