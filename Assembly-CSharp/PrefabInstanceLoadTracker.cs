using System;
using System.Collections.Generic;
using Blizzard.T5.Core.Utils;
using Blizzard.T5.Jobs;
using Blizzard.T5.Services;
using UnityEngine;

public class PrefabInstanceLoadTracker : IService
{
	public class Context
	{
		public bool Active { get; private set; } = true;

		public void MarkDestroyed()
		{
			Active = false;
		}
	}

	private class InstantiatePrefabCallbackData<T>
	{
		public Context context;

		public AssetReference requestedAssetRef;

		public PrefabCallback<T> callerCallback;

		public object callerData;
	}

	private Dictionary<Context, List<GameObject>> m_TrackedPrefabs = new Dictionary<Context, List<GameObject>>();

	public IEnumerator<IAsyncJobResult> Initialize(ServiceLocator serviceLocator)
	{
		yield break;
	}

	public Type[] GetDependencies()
	{
		return null;
	}

	public void Shutdown()
	{
	}

	public static PrefabInstanceLoadTracker Get()
	{
		return ServiceManager.Get<PrefabInstanceLoadTracker>();
	}

	public GameObject InstantiatePrefab(Context context, AssetReference assetRef, AssetLoadingOptions options = AssetLoadingOptions.None)
	{
		GameObject instance = AssetLoader.Get().InstantiatePrefab(assetRef, options);
		if (instance != null)
		{
			AddInstantiatedPrefabToContext(context, instance);
		}
		return instance;
	}

	public bool InstantiatePrefab(Context context, AssetReference assetRef, PrefabCallback<GameObject> callback, object callbackData = null, AssetLoadingOptions options = AssetLoadingOptions.None)
	{
		InstantiatePrefabCallbackData<GameObject> internalCbData = new InstantiatePrefabCallbackData<GameObject>
		{
			context = context,
			callerCallback = callback,
			callerData = callbackData,
			requestedAssetRef = assetRef
		};
		return AssetLoader.Get().InstantiatePrefab(assetRef, OnPrefabInstantiated, internalCbData, options);
	}

	public void DestroyContext(Context context)
	{
		if (!context.Active)
		{
			return;
		}
		context.MarkDestroyed();
		if (m_TrackedPrefabs.TryGetValue(context, out var instanceList))
		{
			foreach (GameObject item in instanceList)
			{
				UnityEngine.Object.Destroy(item);
			}
			m_TrackedPrefabs.Clear();
		}
		m_TrackedPrefabs.Remove(context);
	}

	private void OnPrefabInstantiated(AssetReference assetRef, GameObject instance, object callbackData)
	{
		InstantiatePrefabCallbackData<GameObject> internalCbData = callbackData as InstantiatePrefabCallbackData<GameObject>;
		if (internalCbData.context != null && !internalCbData.context.Active)
		{
			if (instance != null)
			{
				UnityEngine.Object.Destroy(instance);
			}
			return;
		}
		AddInstantiatedPrefabToContext(internalCbData.context, instance);
		if (GeneralUtils.IsCallbackValid(internalCbData.callerCallback))
		{
			internalCbData.callerCallback(internalCbData.requestedAssetRef, instance, internalCbData.callerData);
		}
	}

	private void AddInstantiatedPrefabToContext(Context context, GameObject instance)
	{
		if (context != null && instance != null)
		{
			if (!m_TrackedPrefabs.TryGetValue(context, out var instanceList))
			{
				instanceList = new List<GameObject>();
				m_TrackedPrefabs.Add(context, instanceList);
			}
			instanceList.Add(instance);
		}
	}
}
