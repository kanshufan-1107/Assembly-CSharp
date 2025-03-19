using System;
using System.Collections.Generic;
using Blizzard.BlizzardErrorMobile;
using Blizzard.T5.AssetManager;
using Blizzard.T5.Jobs;
using Blizzard.T5.Services;
using Hearthstone.Core;
using Hearthstone.UI.Internal;
using UnityEngine;

namespace Hearthstone.UI;

[Serializable]
public class PrefabInstance
{
	private AssetHandle<GameObject> m_prefab;

	private GameObject m_owner;

	private WeakAssetReference m_assetReference;

	private GameObject m_cachedInstance;

	private FlagStateTracker m_instanceReadyState;

	private bool m_instantiateWhenPrefabReady;

	private bool m_isLoading;

	private bool m_wasDestroyed;

	public GameObject Owner
	{
		get
		{
			return m_owner;
		}
		set
		{
			m_owner = value;
		}
	}

	public AssetHandle<GameObject> Prefab => m_prefab;

	public GameObject Instance => m_cachedInstance;

	public bool IsInstanceReady => m_cachedInstance != null;

	private event Action m_loadFailed;

	public PrefabInstance(GameObject owner)
	{
		m_owner = owner;
	}

	public void RegisterInstanceReadyListener(Action<object> listener)
	{
		m_instanceReadyState.RegisterSetListener(listener);
	}

	public void RegisterPrefabLoadFailedListener(Action listener)
	{
		m_loadFailed += listener;
	}

	public bool LoadPrefab(WeakAssetReference assetReference, bool loadSynchronously)
	{
		if (m_assetReference.AssetString == assetReference.AssetString && m_prefab != null)
		{
			return false;
		}
		if (m_cachedInstance != null)
		{
			UnityEngine.Object.DestroyImmediate(m_cachedInstance);
		}
		m_assetReference = assetReference;
		AssetReference assetRef = AssetReference.CreateFromAssetString(assetReference.AssetString);
		if (assetRef == null)
		{
			AssetHandle.SafeDispose(ref m_prefab);
			return false;
		}
		m_wasDestroyed = false;
		if (!m_isLoading)
		{
			m_isLoading = true;
			ServiceManager.InitializeDynamicServicesIfEditor(out var dependencies, typeof(IAssetLoader), typeof(WidgetRunner));
			Processor.QueueJob(new JobDefinition("PrefabInstance.LoadGameObject", Job_LoadGameObject(assetRef, loadSynchronously), JobFlags.StartImmediately, dependencies));
		}
		return true;
	}

	private IEnumerator<IAsyncJobResult> Job_LoadGameObject(AssetReference assetReference, bool loadSynchronously)
	{
		if (loadSynchronously)
		{
			IAssetLoader loader = AssetLoader.Get();
			HandlePrefabReady(assetReference, loader.LoadAsset<GameObject>(assetReference), null);
		}
		else
		{
			LoadPrefab loadPrefab = new LoadPrefab(assetReference);
			yield return loadPrefab;
			HandlePrefabReady(assetReference, loadPrefab.loadedAsset, null);
		}
	}

	public void InstantiateWhenReady()
	{
		m_instantiateWhenPrefabReady = true;
		DestroyInstance();
		if (m_prefab != null && !(m_owner == null))
		{
			ExceptionReporter.Get().UnhandledExceptionContext = m_prefab.Asset.GetFullPath();
			AssetLoader.Get()?.RecordInstantiatingPrefab(m_prefab.Asset.GetFullPath());
			GameObject instance = null;
			try
			{
				instance = UnityEngine.Object.Instantiate(m_prefab.Asset, m_owner.transform);
			}
			catch (Exception ex)
			{
				Debug.LogError($"GameObject Instantiation failed in PrefabInstance.InstantiateWhenReady(), Error:{ex}, " + "Asset.Name:" + ((m_prefab.Asset != null) ? m_prefab.Asset.name : string.Empty) + ", AssetAddress:" + m_prefab.AssetAddress);
				ExceptionReporter.Get().ReportCaughtException(ex);
				this.m_loadFailed?.Invoke();
				ExceptionReporter.Get().UnhandledExceptionContext = null;
				return;
			}
			ExceptionReporter.Get().UnhandledExceptionContext = null;
			instance.transform.SetParent(m_owner.transform);
			instance.transform.localPosition = Vector3.zero;
			instance.name = m_prefab.Asset.name;
			m_cachedInstance = instance;
			m_instanceReadyState.SetAndDispatch();
		}
	}

	private void HandlePrefabReady(AssetReference assetRef, AssetHandle<GameObject> prefab, object unused)
	{
		m_isLoading = false;
		if (m_wasDestroyed)
		{
			Destroy();
			return;
		}
		AssetHandle.Take(ref m_prefab, prefab);
		if (m_prefab == null)
		{
			this.m_loadFailed?.Invoke();
		}
		else if (m_instantiateWhenPrefabReady)
		{
			InstantiateWhenReady();
		}
	}

	public void Destroy()
	{
		DestroyInstance();
		UnloadPrefab();
		m_wasDestroyed = true;
		m_instantiateWhenPrefabReady = false;
	}

	private void DestroyInstance()
	{
		if (m_cachedInstance != null)
		{
			m_instanceReadyState.Clear();
			UnityEngine.Object.DestroyImmediate(m_cachedInstance.gameObject);
		}
	}

	private void UnloadPrefab()
	{
		AssetHandle.SafeDispose(ref m_prefab);
	}
}
