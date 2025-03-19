using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using Blizzard.BlizzardErrorMobile;
using Blizzard.GameService.SDK.Client.Integration;
using Blizzard.T5.AssetManager;
using Blizzard.T5.Core.Utils;
using Blizzard.T5.Fonts;
using Blizzard.T5.Jobs;
using Blizzard.T5.Services;
using Blizzard.Telemetry.WTCG.Client;
using Cysharp.Threading.Tasks;
using Hearthstone;
using Hearthstone.Core;
using Hearthstone.Streaming;
using Hearthstone.Util;
using Unity.Profiling;
using UnityEngine;

public class AssetLoader : IAssetLoader, IService, IAssetBundleError
{
	private class AssetRecord<T>
	{
		private Queue<T> m_records = new Queue<T>(5);

		public List<T> GetList => m_records.ToList();

		public void Add(T asset)
		{
			if (m_records.Count >= 5)
			{
				m_records.Dequeue();
			}
			m_records.Enqueue(asset);
		}
	}

	private class InstantiatePrefabCallbackData<T>
	{
		public AssetReference requestedAssetRef;

		public AssetLoadingOptions callerOptions;

		public PrefabCallback<T> callerCallback;

		public object callerData;
	}

	private readonly Vector3 SPAWN_POS_CAMERA_OFFSET = new Vector3(0f, 0f, -5000f);

	private List<GameObject> m_waitingOnObjects = new List<GameObject>();

	private int m_framesSinceLastDeadHandlesCheck;

	private AssetRecord<string> m_loadingAssets = new AssetRecord<string>();

	private AssetRecord<string> m_instantiatingPrefabs = new AssetRecord<string>();

	private IAssetLocator m_assetLocator;

	private IAssetManager m_assetManager;

	private IAssetBank m_assetBank;

	private IGameDownloadManager m_gameDownloadManager;

	private IPrefabInstantiator m_prefabInstantiator;

	private ExceptionReporter m_exceptionReporter;

	private static readonly ProfilerMarker s_assetLoaderUpdateProfiler = new ProfilerMarker("AssetLoader.Update");

	[CompilerGenerated]
	private OnAssetLoadQueued AssetLoadQueued;

	[CompilerGenerated]
	private OnAssetLoaded AssetLoaded;

	[CompilerGenerated]
	private OnAssetUnreferenced AssetUnreferenced;

	[CompilerGenerated]
	private OnAssetBundleLoaded AssetBundleLoaded;

	[CompilerGenerated]
	private OnAssetBundleUnloaded AssetBundleUnloaded;

	private bool m_prefabInstantiatorCheckForDeadHandlesCompleted = true;

	private bool m_assetManagerForDeadHandlesCompleted = true;

	private bool UsingAssetBundles => true;

	public IEnumerator<IAsyncJobResult> Initialize(ServiceLocator serviceLocator)
	{
		m_exceptionReporter = ExceptionReporter.Get();
		if (serviceLocator.Exists(typeof(GameDownloadManager), includeUninitialized: true))
		{
			yield return new WaitForGameDownloadManagerAvailable();
			m_gameDownloadManager = GameDownloadManagerProvider.Get();
		}
		m_assetBank = CreateAppropriateAssetBank();
		m_assetManager = new AssetManager(Log.Asset, m_assetBank);
		m_assetManager.OnAssetHandleOrphaned += SendAssetHandleOrphanedTelemetry;
		m_prefabInstantiator = new PrefabInstantiator(Log.Asset);
		m_prefabInstantiator.OnSharedPrefabHandleOrphaned += OnSharedPrefabHandleOrphaned;
		serviceLocator.SetJobResultHandler<InstantiatePrefab>(OnInstantiatePrefabResultHandler);
		serviceLocator.SetJobResultHandler<LoadPrefab>(OnLoadAssetResultHandle<GameObject>);
		serviceLocator.SetJobResultHandler<LoadUIScreen>(OnInstantiatePrefabResultHandler);
		serviceLocator.SetJobResultHandler<LoadFontDef>(OnLoadAssetResultHandle<FontDefinition>);
		serviceLocator.SetJobResultHandler<LoadScriptableObject>(OnLoadAssetResultHandle<ScriptableObject>);
		SubscribeToAssetBankEvents(m_assetBank);
		Processor.RegisterUpdateDelegate(Update);
	}

	public Type[] GetDependencies()
	{
		return null;
	}

	public void Shutdown()
	{
		Processor.UnregisterUpdateDelegate(Update);
		UnsubscribeToAssetBankEvents(m_assetBank);
	}

	public void Update()
	{
		m_assetManager.CheckPendingRequests();
		m_prefabInstantiator.ReleaseUnreferencedAssets();
		m_assetManager.ReleaseUnreferencedAssets();
		m_assetManager.CloseUnreferencedBundles();
		if (++m_framesSinceLastDeadHandlesCheck > 30)
		{
			if (m_prefabInstantiatorCheckForDeadHandlesCompleted)
			{
				m_prefabInstantiatorCheckForDeadHandlesCompleted = false;
				StartCheckForDeadHandlesWorker(m_prefabInstantiator.CheckForDeadHandles, 0.2, OnPrefabInstantiatorDeadHandleCheckComplete).Forget();
			}
			if (m_assetManagerForDeadHandlesCompleted)
			{
				m_assetManagerForDeadHandlesCompleted = false;
				StartCheckForDeadHandlesWorker(m_assetManager.CheckForDeadHandles, 0.2, OnAssetManagerDeadHandleCheckComplete).Forget();
			}
			m_framesSinceLastDeadHandlesCheck = 0;
		}
	}

	private async UniTaskVoid StartCheckForDeadHandlesWorker(Func<double, bool> checkForDeadHandles, double maxProcessingTime, Action completedCallback)
	{
		while (!checkForDeadHandles(maxProcessingTime))
		{
			await UniTask.NextFrame();
		}
		completedCallback();
	}

	private void OnPrefabInstantiatorDeadHandleCheckComplete()
	{
		m_prefabInstantiatorCheckForDeadHandlesCompleted = true;
	}

	private void OnAssetManagerDeadHandleCheckComplete()
	{
		m_assetManagerForDeadHandlesCompleted = true;
	}

	private void OnSharedPrefabHandleOrphaned(string asset, string owner)
	{
		Log.Asset.PrintWarning("OnSharedPrefabHandleOrphaned(asset: " + asset + ", owner: " + owner + ")");
		SendSharedPrefabHandleOrphanedTelemetry(asset, owner);
		AssetUnreferenced?.Invoke(asset, owner);
	}

	private void OnInstantiatePrefabResultHandler(IAsyncJobResult result)
	{
		if (result is InstantiatePrefab instantiatePrefab)
		{
			AssetLoadingOptions options = ((!instantiatePrefab.UsePrefabPosition) ? AssetLoadingOptions.IgnorePrefabPosition : AssetLoadingOptions.None);
			InstantiatePrefab(instantiatePrefab.AssetRef, instantiatePrefab.OnPrefabInstantiated, null, options);
		}
	}

	private void OnLoadAssetResultHandle<T>(IAsyncJobResult result) where T : UnityEngine.Object
	{
		if (result is LoadAsset<T> loadAsset)
		{
			LoadAsset<T>(loadAsset.AssetRef, loadAsset.OnAssetLoaded);
		}
	}

	private void OnAssetLoadQueued(string assetAddress, string assetPath)
	{
		AssetLoadQueued?.Invoke(assetAddress, assetPath);
	}

	private void OnAssetLoaded(string assetAddress, string assetPath)
	{
		AssetLoaded?.Invoke(assetAddress, assetPath);
	}

	private void OnAssetBundleLoaded(string assetBundleName)
	{
		AssetBundleLoaded?.Invoke(assetBundleName);
	}

	private void OnAssetBundleUnloaded(string assetBundleName)
	{
		AssetBundleUnloaded?.Invoke(assetBundleName);
	}

	public static IAssetLoader Get()
	{
		return ServiceManager.Get<IAssetLoader>();
	}

	public bool IsWaitingOnObject(GameObject go)
	{
		if (!m_waitingOnObjects.Contains(go))
		{
			return m_prefabInstantiator.IsWaitingOnObject(go);
		}
		return true;
	}

	public bool IsSharedPrefabInstance(GameObject go)
	{
		return m_prefabInstantiator.IsSharedPrefabInstance(go);
	}

	public bool LoadMaterial(AssetReference assetRef, ObjectCallback callback, object callbackData = null, bool disableLocalization = false)
	{
		return LoadObject<Material>(assetRef, callback, callbackData, disableLocalization);
	}

	public Material LoadMaterial(AssetReference assetRef, bool disableLocalization = false)
	{
		if (!TryValidateAndGetRuntimeVariant(assetRef, out var asset, AssetVariantTags.Quality.Normal, disableLocalization))
		{
			return null;
		}
		return LoadObjectImmediately<Material>(assetRef, asset);
	}

	public bool LoadTexture(AssetReference assetRef, ObjectCallback callback, object callbackData = null, bool disableLocalization = false)
	{
		return LoadObject<Texture>(assetRef, callback, callbackData, disableLocalization);
	}

	public Texture LoadTexture(AssetReference assetRef, bool disableLocalization = false)
	{
		Log.Asset.PrintWarning("warning CS0618: `LoadTexture(Asset, bool, bool)' is obsolete: from now on, always use async loading instead (i.e. LoadTexture with callback).");
		if (!TryValidateAndGetRuntimeVariant(assetRef, out var asset, AssetVariantTags.Quality.Normal, disableLocalization))
		{
			return null;
		}
		return LoadObjectImmediately<Texture>(assetRef, asset);
	}

	public bool LoadMesh(AssetReference assetRef, ObjectCallback callback, object callbackData = null, bool disableLocalization = false)
	{
		ObjectCallback meshCallback = delegate(AssetReference meshAssetRef, UnityEngine.Object meshObj, object meshCallbackData)
		{
			GameObject gameObject = meshObj as GameObject;
			MeshFilter meshFilter = ((gameObject != null) ? gameObject.GetComponent<MeshFilter>() : null);
			Mesh obj = ((meshFilter != null) ? meshFilter.sharedMesh : null);
			callback(meshAssetRef, obj, meshCallbackData);
		};
		return LoadObject<GameObject>(assetRef, meshCallback, callbackData, disableLocalization);
	}

	public Mesh LoadMesh(AssetReference assetRef, bool disableLocalization = false)
	{
		if (!TryValidateAndGetRuntimeVariant(assetRef, out var asset, AssetVariantTags.Quality.Normal, disableLocalization))
		{
			return null;
		}
		GameObject go = LoadObjectImmediately<GameObject>(assetRef, asset);
		MeshFilter meshFilter = ((go != null) ? go.GetComponent<MeshFilter>() : null);
		if (!(meshFilter != null))
		{
			return null;
		}
		return meshFilter.sharedMesh;
	}

	public bool LoadGameObject(AssetReference assetRef, GameObjectCallback callback, object callbackData = null, bool autoInstantiateOnLoad = true, bool usePrefabPosition = true)
	{
		return LoadPrefab(assetRef, usePrefabPosition, callback, callbackData, autoInstantiateOnLoad);
	}

	[Obsolete("from now on, always use async loading instead (i.e. LoadUberAnimation with callback).")]
	public UberShaderAnimation LoadUberAnimation(AssetReference assetRef, bool usePrefabPosition = true, bool persistent = false)
	{
		if (!TryValidateAndGetRuntimeVariant(assetRef, out var asset))
		{
			return null;
		}
		return LoadObjectImmediately<UberShaderAnimation>(assetRef, asset);
	}

	public void RecordInstantiatingPrefab(string prefab)
	{
		m_instantiatingPrefabs.Add(prefab);
	}

	public string PrintRecordedAssets()
	{
		return "Loading assets:" + Environment.NewLine + "  " + string.Join(Environment.NewLine + "  ", m_loadingAssets.GetList) + Environment.NewLine + "Instantiating prefabs:" + Environment.NewLine + "  " + string.Join(Environment.NewLine + "  ", m_instantiatingPrefabs.GetList);
	}

	private void SubscribeToAssetBankEvents(IAssetBank assetBank)
	{
		assetBank.AssetLoadQueued += OnAssetLoadQueued;
		assetBank.AssetLoaded += OnAssetLoaded;
		if (assetBank is AssetBundleAssetBank assetBundleBank)
		{
			Log.Asset.PrintDebug("[AssetLoader.SubscribeToAssetBankEvents] Using AssetBundle bank.");
			assetBundleBank.AssetBundleLoaded += OnAssetBundleLoaded;
			assetBundleBank.AssetBundleUnloaded += OnAssetBundleUnloaded;
		}
		else
		{
			Log.Asset.PrintDebug("[AssetLoader.SubscribeToAssetBankEvents] Using prefab bank.");
		}
	}

	private void UnsubscribeToAssetBankEvents(IAssetBank assetBank)
	{
		assetBank.AssetLoadQueued -= OnAssetLoadQueued;
		assetBank.AssetLoaded -= OnAssetLoaded;
		if (assetBank is AssetBundleAssetBank assetBundleBank)
		{
			Log.Asset.PrintDebug("[AssetLoader.UnsubscribeToAssetBankEvents] Using AssetBundle bank.");
			assetBundleBank.AssetBundleLoaded -= OnAssetBundleLoaded;
			assetBundleBank.AssetBundleUnloaded -= OnAssetBundleUnloaded;
		}
		else
		{
			Log.Asset.PrintDebug("[AssetLoader.UnsubscribeToAssetBankEvents] Using prefab bank.");
		}
	}

	private IAssetBank CreateAppropriateAssetBank()
	{
		return CreateAssetBundlesAssetBank();
	}

	private IAssetBank CreateAssetBundlesAssetBank()
	{
		string bundleWithDepGraph = AssetBundleInfo.GetAssetBundlePath(ScriptableAssetManifest.MainManifestBundleName);
		if (!AssetBundleInfo.Exists(bundleWithDepGraph))
		{
			Log.Asset.PrintError("Cannot find asset bundle for AssetBundleDependencyGraph '{0}', editor {1}, playing {2}", bundleWithDepGraph, Application.isEditor, Application.isPlaying);
			throw new ApplicationException("Could not initialize AssetLoader: missing AssetBundleDependencyGraph");
		}
		AssetBundle bundle = AssetBundle.LoadFromFile(bundleWithDepGraph);
		if (bundle == null)
		{
			Log.Asset.PrintError("Failed to load bundle for AssetBundleDependencyGraph '{0}', editor {1}, playing {2}", bundleWithDepGraph, Application.isEditor, Application.isPlaying);
			throw new ApplicationException("Could not initialize AssetLoader: failed to load bundle with AssetBundleDependencyGraph");
		}
		AssetBundleDependencyGraph bundleDeps = bundle.LoadAsset<AssetBundleDependencyGraph>(ScriptableAssetManifest.BundleDepsAssetPath);
		bundle.Unload(unloadAllLoadedObjects: false);
		if (bundleDeps == null)
		{
			Log.Asset.PrintError("Failed to load '{0}' from bundle '{1}'", ScriptableAssetManifest.BundleDepsAssetPath, bundleWithDepGraph);
			throw new ApplicationException("Could not initialize AssetLoader: failed to load AssetBundleDependencyGraph");
		}
		m_assetLocator = new AssetLocator(AssetManifest.Get());
		return new AssetBundleAssetBank(Log.Asset, this, m_assetLocator, bundleDeps);
	}

	private bool LoadObject<T>(AssetReference assetRef, ObjectCallback callback, object callbackData, bool disableLocalization = false) where T : UnityEngine.Object
	{
		if (!TryValidateAndGetRuntimeVariant(assetRef, out var asset, AssetVariantTags.Quality.Normal, disableLocalization))
		{
			return false;
		}
		m_loadingAssets.Add(assetRef.ToString() + ", Async");
		IAsyncAssetRequest<T> asyncAssetRequest = m_assetManager.LoadAsync<T>(asset?.GetGuid(), trackHandle: false);
		asyncAssetRequest.OnCompleted = (AssetLoadedCB<T>)Delegate.Combine(asyncAssetRequest.OnCompleted, (AssetLoadedCB<T>)delegate(IAsyncAssetRequest<T> completedRequest)
		{
			OnAssetLoaded(assetRef, asset?.GetGuid(), completedRequest.Result);
			callback(assetRef, completedRequest.Result.Asset, callbackData);
		});
		return true;
	}

	private bool LoadPrefab(AssetReference assetRef, bool usePrefabPosition, GameObjectCallback callback, object callbackData, bool autoInstantiateOnLoad = true)
	{
		if (!TryValidateAndGetRuntimeVariant(assetRef, out var asset))
		{
			return false;
		}
		return LoadPrefabInternal(asset, assetRef, usePrefabPosition, callback, callbackData, autoInstantiateOnLoad);
	}

	private GameObject TryGetAsGameObject(string guid, UnityEngine.Object obj)
	{
		GameObject go = obj as GameObject;
		if (!go)
		{
			string userErrorMessage = GameStrings.Format("GLOBAL_ERROR_ASSET_INCORRECT_DATA", guid);
			Debug.LogError($"AssetLoader.WaitThenCallGameObjectCallback() - {userErrorMessage} (prefab={obj})");
			Error.AddFatal(FatalErrorReason.ASSET_INCORRECT_DATA, userErrorMessage);
		}
		return go;
	}

	private bool LoadPrefabInternal(Asset assetToLoad, AssetReference requestedReference, bool usePrefabPosition, GameObjectCallback callback, object callbackData, bool autoInstantiateOnLoad)
	{
		string guid = assetToLoad?.GetGuid();
		if (guid == null)
		{
			return false;
		}
		m_loadingAssets.Add(assetToLoad.ToString() + ", Async");
		IAsyncAssetRequest<GameObject> asyncAssetRequest = m_assetManager.LoadAsync<GameObject>(assetToLoad.GetGuid(), trackHandle: false);
		asyncAssetRequest.OnCompleted = (AssetLoadedCB<GameObject>)Delegate.Combine(asyncAssetRequest.OnCompleted, (AssetLoadedCB<GameObject>)delegate(IAsyncAssetRequest<GameObject> completedRequest)
		{
			if (completedRequest.HasFailed)
			{
				LogAssetFailedToLoad(requestedReference, assetToLoad.GetGuid(), "GameObject");
			}
			else
			{
				OnAssetLoaded(requestedReference, guid, completedRequest.Result);
				GameObject asset = completedRequest.Result.Asset;
				GameObject gameObject = TryGetAsGameObject(guid, asset);
				if (autoInstantiateOnLoad)
				{
					Processor.RunCoroutine(InstantiateAndWaitThenCallGameObjectCallback(requestedReference, gameObject, usePrefabPosition, callback, callbackData));
				}
				else if (GeneralUtils.IsCallbackValid(callback))
				{
					callback(requestedReference, gameObject, callbackData);
				}
			}
		});
		return !asyncAssetRequest.HasFailed;
	}

	private void OnAssetLoaded<T>(AssetReference requestedAsset, string resolvedGuid, AssetHandle<T> loadedAsset) where T : UnityEngine.Object
	{
		if (loadedAsset == null)
		{
			LogAssetFailedToLoad(requestedAsset, resolvedGuid, typeof(T).Name);
		}
	}

	private bool TryValidateAndGetRuntimeVariant(AssetReference assetRef, out Asset assetToLoad, AssetLoadingOptions options)
	{
		AssetVariantTags.Quality quality = (options.HasFlag(AssetLoadingOptions.UseLowQuality) ? AssetVariantTags.Quality.Low : AssetVariantTags.Quality.Normal);
		bool disableLocalization = options.HasFlag(AssetLoadingOptions.DisableLocalization);
		return TryValidateAndGetRuntimeVariant(assetRef, out assetToLoad, quality, disableLocalization);
	}

	private bool TryValidateAndGetRuntimeVariant(AssetReference assetRef, out Asset assetToLoad, AssetVariantTags.Quality quality = AssetVariantTags.Quality.Normal, bool disableLocalization = false)
	{
		string bundleName = null;
		if (assetRef == null)
		{
			BadAssetAccess(null, bundleName, AssetBundleErrorReason.NullGuid, "assetRef is null");
			SendAssetErrorToExceptionDashboard("assetRef is null");
			assetToLoad = null;
			return false;
		}
		assetToLoad = GetRuntimeAssetVariant(assetRef, quality, disableLocalization);
		string assetPath;
		if (assetToLoad == null)
		{
			if (m_assetLocator != null)
			{
				m_assetLocator.TryLocateAsset(assetRef, out bundleName, out assetPath);
			}
			BadAssetAccess(assetRef.guid, bundleName, AssetBundleErrorReason.MissingRuntimeVariant, "Asset is missing from asset bundles");
			SendAssetErrorToExceptionDashboard($"Asset '{assetRef}' is missing from asset bundles");
			return false;
		}
		if (UsingAssetBundles && !m_assetLocator.TryLocateAsset(assetToLoad.GetGuid(), out bundleName, out assetPath))
		{
			BadAssetAccess(assetToLoad.GetGuid(), bundleName, AssetBundleErrorReason.MissingBundle, "Asset variant is missing from asset bundles");
			SendAssetErrorToExceptionDashboard($"Asset variant '{assetToLoad.GetGuid()}' of asset '{assetRef}' is missing from asset bundles");
			return false;
		}
		if (!IsAssetAvailable(assetToLoad.GetGuid()))
		{
			if (UsingAssetBundles)
			{
				if (m_gameDownloadManager.IsBundleShouldAvailable(bundleName))
				{
					BadAssetAccess(assetToLoad.GetGuid(), bundleName, AssetBundleErrorReason.NotDownloaded, "Asset used without bundle on disk");
					SendAssetErrorToExceptionDashboard("Asset '" + assetToLoad.GetGuid() + "' in bundle '" + bundleName + "' used without bundle on disk");
				}
			}
			else
			{
				SendAssetErrorToExceptionDashboard("Asset '" + assetToLoad.GetGuid() + "' couldn't be found");
			}
			return false;
		}
		return true;
	}

	private Asset GetRuntimeAssetVariant(AssetReference assetRef, AssetLoadingOptions options)
	{
		AssetVariantTags.Quality quality = (options.HasFlag(AssetLoadingOptions.UseLowQuality) ? AssetVariantTags.Quality.Low : AssetVariantTags.Quality.Normal);
		bool disableLocalization = options.HasFlag(AssetLoadingOptions.DisableLocalization);
		return GetRuntimeAssetVariant(assetRef, quality, disableLocalization);
	}

	private Asset GetRuntimeAssetVariant(AssetReference assetRef, AssetVariantTags.Quality quality = AssetVariantTags.Quality.Normal, bool disableLocalization = false)
	{
		if (assetRef == null || assetRef.guid == null)
		{
			Log.Asset.PrintError("Invalid assetRef: {0} is null\n{1}", (assetRef == null) ? "assetRef" : "guid", new StackTrace());
			return null;
		}
		Locale locale = Locale.enUS;
		if (!disableLocalization)
		{
			locale = Localization.GetLocale();
			if (Network.IsRunning() && BattleNet.GetAccountCountry() == "CHN")
			{
				locale = Locale.zhCN;
			}
		}
		AssetVariantTags.Locale localeTag = AssetVariantTags.GetLocaleVariantTagForLocale(locale);
		AssetVariantTags.Platform platformTag = (UniversalInputManager.UsePhoneUI ? AssetVariantTags.Platform.Phone : AssetVariantTags.Platform.Any);
		AssetVariantTags.Region regionTag = AssetVariantTags.GetRegionVariantTagForRegion(RegionUtils.CurrentRegion);
		if (AssetManifest.Get() == null)
		{
			Log.Asset.PrintWarning("[AssetLoader.GetRuntimeAssetVariant] AssetManifest isn't initialized.");
			return null;
		}
		if (!AssetManifest.Get().TryResolveAsset(assetRef.guid, out var appropriateGuid, out var _, localeTag, quality, platformTag, regionTag))
		{
			Log.Asset.PrintWarning(string.Format("[AssetLoader.{0}] Asset variant for '{1}' wasn't found. Quality={2}, Locale={3} ({4}), Platform={5}", "GetRuntimeAssetVariant", 0, 1, 2, 3, 4), assetRef, quality, locale, localeTag, platformTag);
			return null;
		}
		return new Asset(appropriateGuid);
	}

	public bool IsRuntimeAssetVariantAvailable(AssetReference assetRef, AssetLoadingOptions options)
	{
		return IsAssetAvailable(GetRuntimeAssetVariant(assetRef, options)?.GetGuid());
	}

	public bool IsAssetAvailable(AssetReference assetRef)
	{
		if (assetRef == null)
		{
			return false;
		}
		return IsAssetAvailable(assetRef.guid);
	}

	private bool IsAssetAvailable(string assetGuid)
	{
		if (m_gameDownloadManager != null)
		{
			return m_gameDownloadManager.IsAssetDownloaded(assetGuid);
		}
		return false;
	}

	public AssetHandle<T> LoadAsset<T>(AssetReference requestedAsset, AssetLoadingOptions options = AssetLoadingOptions.None) where T : UnityEngine.Object
	{
		if (!TryValidateAndGetRuntimeVariant(requestedAsset, out var appropriateAsset, options))
		{
			return null;
		}
		m_loadingAssets.Add(requestedAsset.ToString());
		AssetHandle<T> loadedAsset = m_assetManager.Load<T>(appropriateAsset.GetGuid(), trackHandle: true);
		OnAssetLoaded(requestedAsset, appropriateAsset.GetGuid(), loadedAsset);
		return loadedAsset;
	}

	public bool LoadAsset<T>(ref AssetHandle<T> assetHandle, AssetReference assetRef, AssetLoadingOptions options = AssetLoadingOptions.None) where T : UnityEngine.Object
	{
		if (!TryValidateAndGetRuntimeVariant(assetRef, out var _, options))
		{
			return false;
		}
		AssetHandle<T> newHandle = LoadAsset<T>(assetRef, options);
		AssetHandle.SafeDispose(ref assetHandle);
		assetHandle = newHandle;
		return assetHandle;
	}

	public bool LoadAsset<T>(AssetReference assetRef, AssetHandleCallback<T> callback, object callbackData = null, AssetLoadingOptions options = AssetLoadingOptions.None) where T : UnityEngine.Object
	{
		if (!TryValidateAndGetRuntimeVariant(assetRef, out var appropriateAsset))
		{
			return false;
		}
		m_loadingAssets.Add(assetRef.ToString() + ", Async");
		IAsyncAssetRequest<T> asyncAssetRequest = m_assetManager.LoadAsync<T>(appropriateAsset.GetGuid(), trackHandle: true);
		asyncAssetRequest.OnCompleted = (AssetLoadedCB<T>)Delegate.Combine(asyncAssetRequest.OnCompleted, (AssetLoadedCB<T>)delegate(IAsyncAssetRequest<T> completedRequest)
		{
			OnAssetLoaded(assetRef, appropriateAsset.GetGuid(), completedRequest.Result);
			if (GeneralUtils.IsCallbackValid(callback))
			{
				callback(assetRef, completedRequest.Result, callbackData);
			}
			else
			{
				completedRequest.Result?.Dispose();
			}
		});
		return true;
	}

	public GameObject InstantiatePrefab(AssetReference assetRef, AssetLoadingOptions options)
	{
		using AssetHandle<GameObject> prefabHandle = LoadAsset<GameObject>(assetRef, options);
		if (prefabHandle == null)
		{
			return null;
		}
		return m_prefabInstantiator.InstantiatePrefab(prefabHandle, options);
	}

	public bool InstantiatePrefab(AssetReference assetRef, PrefabCallback<GameObject> callback, object callbackData, AssetLoadingOptions options)
	{
		InstantiatePrefabCallbackData<GameObject> internalCbData = new InstantiatePrefabCallbackData<GameObject>
		{
			callerCallback = callback,
			callerData = callbackData,
			callerOptions = options,
			requestedAssetRef = assetRef
		};
		return LoadAsset<GameObject>(assetRef, OnPrefabLoaded, internalCbData, options);
	}

	public AssetHandle<GameObject> GetOrInstantiateSharedPrefab(AssetReference assetRef, AssetLoadingOptions options = AssetLoadingOptions.None)
	{
		if (!TryValidateAndGetRuntimeVariant(assetRef, out var _, options))
		{
			return null;
		}
		using AssetHandle<GameObject> prefabHandle = LoadAsset<GameObject>(assetRef, options);
		return m_prefabInstantiator.GetOrInstantiateSharedPrefab(prefabHandle, options);
	}

	public bool GetOrInstantiateSharedPrefab(AssetReference assetRef, PrefabCallback<AssetHandle<GameObject>> callback, object callbackData = null, AssetLoadingOptions options = AssetLoadingOptions.None)
	{
		InstantiatePrefabCallbackData<AssetHandle<GameObject>> internalCbData = new InstantiatePrefabCallbackData<AssetHandle<GameObject>>
		{
			callerCallback = callback,
			callerData = callbackData,
			callerOptions = options,
			requestedAssetRef = assetRef
		};
		return LoadAsset<GameObject>(assetRef, OnSharedPrefabLoaded, internalCbData, options);
	}

	private void OnPrefabLoaded(AssetReference prefabRef, AssetHandle<GameObject> prefabHandle, object callbackData)
	{
		using (prefabHandle)
		{
			InstantiatePrefabCallbackData<GameObject> internalCbData = callbackData as InstantiatePrefabCallbackData<GameObject>;
			if (!prefabHandle)
			{
				if (GeneralUtils.IsCallbackValid(internalCbData.callerCallback))
				{
					internalCbData.callerCallback(prefabRef, null, internalCbData.callerData);
				}
			}
			else
			{
				m_prefabInstantiator.InstantiatePrefab(prefabHandle, OnPrefabInstantiated, internalCbData, internalCbData.callerOptions);
			}
		}
	}

	private void OnSharedPrefabLoaded(AssetReference prefabRef, AssetHandle<GameObject> prefabHandle, object callbackData)
	{
		InstantiatePrefabCallbackData<AssetHandle<GameObject>> internalCbData = callbackData as InstantiatePrefabCallbackData<AssetHandle<GameObject>>;
		if (!prefabHandle)
		{
			if (GeneralUtils.IsCallbackValid(internalCbData.callerCallback))
			{
				internalCbData.callerCallback(prefabRef, null, internalCbData.callerData);
			}
		}
		else
		{
			m_prefabInstantiator.GetOrInstantiateSharedPrefab(prefabHandle, OnPrefabInstantiated, internalCbData, internalCbData.callerOptions);
		}
	}

	private void OnPrefabInstantiated<T>(string prefabAddress, T instance, object callbackData)
	{
		InstantiatePrefabCallbackData<T> internalCbData = callbackData as InstantiatePrefabCallbackData<T>;
		if (GeneralUtils.IsCallbackValid(internalCbData.callerCallback))
		{
			internalCbData.callerCallback(internalCbData.requestedAssetRef, instance, internalCbData.callerData);
		}
	}

	private T LoadObjectImmediately<T>(AssetReference requestedAsset, Asset resolvedAsset) where T : UnityEngine.Object
	{
		if (!TryValidateAndGetRuntimeVariant(requestedAsset, out var _))
		{
			return null;
		}
		string resolvedGuid = resolvedAsset?.GetGuid();
		if (string.IsNullOrEmpty(resolvedGuid))
		{
			return null;
		}
		m_loadingAssets.Add(requestedAsset.ToString());
		AssetHandle<T> loadedAsset = m_assetManager.Load<T>(resolvedGuid, trackHandle: false);
		OnAssetLoaded(requestedAsset, resolvedGuid, loadedAsset);
		return loadedAsset.Asset;
	}

	private IEnumerator InstantiateAndWaitThenCallGameObjectCallback(AssetReference assetRef, GameObject prefab, bool usePrefabPosition, GameObjectCallback callback, object callbackData)
	{
		if (prefab == null)
		{
			if (GeneralUtils.IsCallbackValid(callback))
			{
				callback(assetRef, null, callbackData);
			}
			yield break;
		}
		GameObject instance = (usePrefabPosition ? UnityEngine.Object.Instantiate(prefab) : UnityEngine.Object.Instantiate(prefab, NewGameObjectSpawnPosition(), prefab.transform.rotation));
		m_waitingOnObjects.Add(instance);
		yield return new WaitForEndOfFrame();
		m_waitingOnObjects.Remove(instance);
		if (GeneralUtils.IsCallbackValid(callback))
		{
			callback(assetRef, instance, callbackData);
		}
	}

	private Vector3 NewGameObjectSpawnPosition()
	{
		if (Camera.main == null)
		{
			return Vector3.zero;
		}
		return Camera.main.transform.position + SPAWN_POS_CAMERA_OFFSET;
	}

	public void BadAssetAccess(string assetAddress, string bundleName, AssetBundleErrorReason reason, string detail)
	{
		TelemetryManager.Client().SendAssetLoaderError(assetAddress, bundleName, (AssetLoaderError.AssetBundleErrorReason)reason, detail);
	}

	private void LogAssetFailedToLoad(AssetReference requestedAsset, string resolvedGuid, string assetType)
	{
		SendMissingAssetTelemetry(requestedAsset, resolvedGuid, assetType);
		string errorMessage = $"Asset {requestedAsset} of type {assetType} failed to load.";
		Log.MissingAssets.PrintError(errorMessage);
		SendAssetErrorToExceptionDashboard(errorMessage);
	}

	private void SendAssetErrorToExceptionDashboard(string assertionMessage)
	{
		Log.Asset.PrintError(assertionMessage);
	}

	private static void SendMissingAssetTelemetry(AssetReference requestedAsset, string resolvedGuid, string assetType)
	{
		if (string.IsNullOrEmpty(requestedAsset?.guid))
		{
			Log.Telemetry.Print("Missing asset was found, but there was not way to identify it. No telemetry will be sent.");
		}
		else if (Application.isEditor)
		{
			Log.Telemetry.Print("Missing asset found in editor - not sending missing asset telemetry for requestedGuid={0}, resolvedGuid={1}, name={2}", requestedAsset.guid, resolvedGuid, requestedAsset.GetLegacyAssetName());
		}
		else
		{
			TelemetryManager.Client().SendAssetNotFound(assetType, requestedAsset.guid, resolvedGuid, requestedAsset.GetLegacyAssetName());
		}
	}

	private static void SendSharedPrefabHandleOrphanedTelemetry(string asset, string owner)
	{
	}

	private static void SendAssetHandleOrphanedTelemetry(string asset, string owner)
	{
	}

	public void PopulateDebugStats(AssetManagerDebugStats stats, AssetManagerDebugStats.DataFields requestedFields)
	{
		m_assetManager.PopulateDebugStats(stats, requestedFields);
	}
}
