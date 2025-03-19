using System;
using System.Collections.Generic;
using System.Diagnostics;
using Blizzard.T5.AssetManager;
using Blizzard.T5.MaterialService.Extensions;
using Hearthstone.UI.Core;
using Hearthstone.UI.Internal;
using Hearthstone.UI.Logging;
using Hearthstone.UI.Scripting;
using Unity.Profiling;
using UnityEngine;

namespace Hearthstone.UI;

[ExecuteAlways]
[AddComponentMenu("")]
[HelpURL("https://confluence.blizzard.com/x/URZVJg")]
public class Spawnable : WidgetBehavior, INestedReferenceResolver, IAsyncInitializationBehavior, IPopupRendering, ILayerOverridable, IVisibleWidgetComponent, IWidgetEventListener
{
	private const string SpawnedItemName = "Spawnable Library Item: {0}";

	[Tooltip("This script provides Spawnable with the ID or name of the spawnable item you wish to instantiate.")]
	[SerializeField]
	private ScriptString m_valueScript;

	[Tooltip("A reference the spawnable library asset that determines what objects this Spawnable can instantiate.")]
	[SerializeField]
	private SpawnableLibrary m_spawnableLibrary;

	[SerializeField]
	[Tooltip("An optional reference to a mesh to assign to the mesh filter on the loaded item.")]
	private Mesh m_intendedMesh;

	private AssetHandle<Material> m_materialHandle;

	private AssetHandle<Texture2D> m_textureHandle;

	private AssetHandle<Sprite> m_spriteHandle;

	private Material m_material;

	private Texture2D m_texture;

	private Sprite m_sprite;

	private GameLayer? m_originalLayer;

	private GameLayer? m_overrideLayer;

	private Renderer m_renderer;

	private WidgetInstance m_widget;

	private SpawnableLibrary.ItemData m_itemData;

	private int m_materialAsyncOperationId;

	private int m_textureAsyncOperationId;

	private int m_spriteAsyncOperationId;

	private int m_lastDataVerion;

	private int m_startedAssetLoadCount;

	private int m_failedAssetLoadCount;

	private bool m_hasOverride;

	private object m_overrideValue;

	private bool m_initialized;

	private bool m_isLoading;

	private bool m_isVisible;

	private IPopupRoot m_popupRoot;

	private HashSet<IPopupRendering> m_popupRenderingComponents = new HashSet<IPopupRendering>();

	private FlagStateTracker m_activatedState;

	private FlagStateTracker m_deactivatedState;

	private FlagStateTracker m_readyState;

	private Queue<(string eventName, TriggerEventParameters eventParams)> m_pendingEvents;

	private static ProfilerMarker s_onUpdateProfilerMarker = new ProfilerMarker("Spawnable.OnUpdate");

	public bool HandlesChildLayers => true;

	public bool IsDesiredHidden => base.Owner.IsDesiredHidden;

	public bool IsDesiredHiddenInHierarchy => base.Owner.IsDesiredHiddenInHierarchy;

	public bool HandlesChildVisibility => false;

	public WidgetTemplate OwningWidget => base.Owner;

	[Overridable]
	public bool ItemShouldUseScript
	{
		get
		{
			return m_hasOverride;
		}
		set
		{
			m_hasOverride = !value;
		}
	}

	[Overridable]
	public int ItemID
	{
		set
		{
			m_overrideValue = value;
			CreateItemByID(value);
			m_hasOverride = true;
		}
	}

	[Overridable]
	public string ItemName
	{
		set
		{
			m_overrideValue = value;
			CreateItemByName(value);
			m_hasOverride = true;
		}
	}

	public bool HasOverride => m_hasOverride;

	public object OverrideValue => m_overrideValue;

	public override bool IsChangingStates => m_isLoading;

	public bool IsReady
	{
		get
		{
			if (!m_readyState.IsSet)
			{
				return !m_initialized;
			}
			return true;
		}
	}

	public Behaviour Container => this;

	public void EnablePopupRendering(IPopupRoot popupRoot)
	{
		m_popupRoot = popupRoot;
		PropagatePopupRendering(null);
		RegisterDoneChangingStatesListener(PropagatePopupRendering, null, callImmediatelyIfSet: false);
	}

	private void PropagatePopupRendering(object unused)
	{
		bool shouldOverrideLayer = m_overrideLayer.HasValue;
		int layerOverride = (int)m_overrideLayer.GetValueOrDefault();
		m_popupRoot.ApplyPopupRendering(base.transform, m_popupRenderingComponents, shouldOverrideLayer, layerOverride, m_isVisible);
	}

	public void DisablePopupRendering()
	{
		if (m_popupRoot != null)
		{
			m_popupRoot.CleanupPopupRendering(m_popupRenderingComponents);
		}
		RemoveDoneChangingStatesListener(PropagatePopupRendering);
		m_popupRoot = null;
		if (m_isVisible)
		{
			ClearLayerOverride();
		}
	}

	public bool HandlesChildPropagation()
	{
		return true;
	}

	public void SetVisibility(bool isVisible, bool isInternal)
	{
		m_isVisible = isVisible;
		if (isVisible)
		{
			if (isInternal && (m_overrideLayer.HasValue || m_originalLayer.HasValue))
			{
				base.gameObject.layer = (int)(m_overrideLayer ?? m_originalLayer).Value;
				if (m_widget != null)
				{
					m_widget.SetLayerOverride((GameLayer)base.gameObject.layer);
					m_widget.Show();
				}
				else if (m_renderer != null)
				{
					m_renderer.gameObject.layer = base.gameObject.layer;
					PopupRenderer popupRenderer = m_renderer.GetComponentInChildren<PopupRenderer>();
					if (popupRenderer != null)
					{
						popupRenderer.SetVisibility(isVisible, isInternal: false);
						popupRenderer.SetLayerOverride((GameLayer)base.gameObject.layer);
					}
				}
				m_overrideLayer = null;
				m_originalLayer = null;
			}
			else
			{
				ClearLayerOverride();
			}
		}
		else if (isInternal)
		{
			if (!m_originalLayer.HasValue)
			{
				m_originalLayer = (GameLayer)base.gameObject.layer;
			}
			base.gameObject.layer = 28;
			if (m_widget != null)
			{
				m_widget.SetLayerOverride((GameLayer)base.gameObject.layer);
				m_widget.Hide();
			}
			else if (m_renderer != null)
			{
				m_renderer.gameObject.layer = base.gameObject.layer;
			}
		}
		else
		{
			SetLayerOverride(GameLayer.InvisibleRender);
		}
	}

	public bool TryGetScriptValue(out object value)
	{
		value = null;
		ScriptContext.EvaluationResults results = new ScriptContext().Evaluate(m_valueScript.Script, this);
		if (results.ErrorCode == ScriptContext.ErrorCodes.Success)
		{
			value = results.Value;
			return true;
		}
		return false;
	}

	public WidgetEventListenerResponse EventReceived(string eventName, TriggerEventParameters eventParams)
	{
		WidgetEventListenerResponse response = default(WidgetEventListenerResponse);
		if (m_widget == null)
		{
			if (m_pendingEvents == null)
			{
				m_pendingEvents = new Queue<(string, TriggerEventParameters)>();
			}
			(string, TriggerEventParameters) eventContext = (eventName, eventParams);
			m_pendingEvents.Enqueue(eventContext);
			return response;
		}
		m_widget.TriggerEvent(eventName, eventParams);
		return default(WidgetEventListenerResponse);
	}

	protected override void OnInitialize()
	{
		m_initialized = true;
		if (m_spawnableLibrary == null || (m_overrideValue == null && GetLocalDataVersion() == 1))
		{
			m_readyState.SetAndDispatch();
		}
	}

	public override void OnUpdate()
	{
		using (s_onUpdateProfilerMarker.Auto())
		{
			int dataVersion = GetLocalDataVersion();
			if (m_lastDataVerion != dataVersion)
			{
				HandleDataChanged();
				m_lastDataVerion = dataVersion;
			}
		}
	}

	public override bool TryIncrementDataVersion(int id)
	{
		HashSet<int> dataModelIDs = null;
		dataModelIDs = m_valueScript.GetDataModelIDs();
		if (dataModelIDs == null || !dataModelIDs.Contains(id))
		{
			return false;
		}
		IncrementLocalDataVersion();
		return true;
	}

	public override bool GetIsChangingStates(Func<GameObject, bool> includeGameObject)
	{
		if (!includeGameObject(base.gameObject))
		{
			return false;
		}
		return IsChangingStates;
	}

	private void HandleDataChanged()
	{
		if (!m_hasOverride)
		{
			if (TryGetScriptValue(out var value))
			{
				HandleItemChanged(value);
			}
			else if (m_spawnableLibrary != null && m_spawnableLibrary.HasDefault)
			{
				CreateItem(m_spawnableLibrary.GetDefaultItemData());
			}
		}
	}

	private void HandleOverrideChange()
	{
		if (!m_hasOverride || m_overrideValue == null)
		{
			m_hasOverride = false;
		}
		else
		{
			HandleItemChanged(m_overrideValue);
		}
	}

	private void HandleItemChanged(object value)
	{
		if (value is string)
		{
			CreateItemByName((string)value);
		}
		else if (value is IConvertible)
		{
			CreateItemByID(Convert.ToInt32(value));
		}
		else if (value == null && m_spawnableLibrary != null && m_spawnableLibrary.HasDefault)
		{
			CreateItem(m_spawnableLibrary.GetDefaultItemData());
		}
	}

	private void SetupRenderer()
	{
		if (m_sprite != null && m_renderer is SpriteRenderer spriteRenderer)
		{
			bool customSize = spriteRenderer.size != Vector2.one;
			Vector2 setSize = spriteRenderer.size;
			spriteRenderer.sprite = m_sprite;
			if (customSize)
			{
				spriteRenderer.size = setSize;
			}
		}
		m_renderer.SetMaterial(m_material);
		m_renderer.enabled = true;
		m_failedAssetLoadCount = 0;
		m_startedAssetLoadCount = 0;
		m_isLoading = false;
		m_readyState.SetAndDispatch();
		HandleDoneChangingStates();
		if (m_popupRoot != null)
		{
			EnablePopupRendering(m_popupRoot);
		}
	}

	private void HandleTextureItemCleanUp()
	{
		m_material = null;
		AssetHandle.SafeDispose(ref m_materialHandle);
		m_texture = null;
		AssetHandle.SafeDispose(ref m_textureHandle);
		m_sprite = null;
		AssetHandle.SafeDispose(ref m_spriteHandle);
		if (m_renderer != null)
		{
			HandleGameObjectCleanUp(m_renderer.gameObject);
			m_renderer = null;
		}
		m_failedAssetLoadCount = 0;
		m_startedAssetLoadCount = 0;
	}

	private void HandleWidgetItemCleanUp()
	{
		if (!(m_widget == null))
		{
			base.Owner.RemoveNestedInstance(m_widget);
			HandleGameObjectCleanUp(m_widget.gameObject);
			m_widget = null;
		}
	}

	private void HandleGameObjectCleanUp(GameObject go)
	{
		UnityEngine.Object.Destroy(go);
	}

	private void HandleAssetItemError<T>(AssetHandle<T> assetHandle) where T : UnityEngine.Object
	{
		AssetHandle.SafeDispose(ref assetHandle);
		m_failedAssetLoadCount++;
		if (m_failedAssetLoadCount >= m_startedAssetLoadCount)
		{
			m_isLoading = false;
			m_readyState.SetAndDispatch();
			Debug.LogErrorFormat("Failed to load texture icon for {0} in {1}!", base.name, base.Owner.name);
			HandleTextureItemCleanUp();
			HandleDoneChangingStates();
		}
	}

	private void CreateItemByName(string name)
	{
		if (string.IsNullOrEmpty(name) || m_spawnableLibrary == null)
		{
			return;
		}
		SpawnableLibrary.ItemData itemData = m_spawnableLibrary.GetItemDataByName(name);
		if (itemData == null)
		{
			if (m_spawnableLibrary.HasDefault)
			{
				CreateItem(m_spawnableLibrary.GetDefaultItemData());
			}
		}
		else
		{
			CreateItem(itemData);
		}
	}

	private void CreateItemByID(int id)
	{
		if (string.IsNullOrEmpty(base.name) || m_spawnableLibrary == null)
		{
			return;
		}
		SpawnableLibrary.ItemData itemData = m_spawnableLibrary.GetItemDataByID(id);
		if (itemData == null)
		{
			if (m_spawnableLibrary.HasDefault)
			{
				CreateItem(m_spawnableLibrary.GetDefaultItemData());
			}
		}
		else
		{
			CreateItem(itemData);
		}
	}

	private void CreateItem(SpawnableLibrary.ItemData itemData)
	{
		if (!string.IsNullOrEmpty(itemData.Reference) && itemData != m_itemData)
		{
			m_readyState.Clear();
			m_itemData = itemData;
			switch (itemData.ItemType)
			{
			case SpawnableLibrary.ItemType.Texture:
				CreateTextureItem(itemData);
				break;
			case SpawnableLibrary.ItemType.Sprite:
				CreateSpriteItem(itemData);
				break;
			case SpawnableLibrary.ItemType.Widget:
				CreateWidgetItem(itemData);
				break;
			case SpawnableLibrary.ItemType.Material:
				CreateMaterialItem(itemData);
				break;
			}
		}
	}

	private void CreateWidgetItem(SpawnableLibrary.ItemData itemData)
	{
		HandleWidgetItemCleanUp();
		HandleTextureItemCleanUp();
		m_isLoading = true;
		WidgetInstance widget = WidgetInstance.Create(itemData.Reference);
		widget.transform.SetParent(base.transform, worldPositionStays: false);
		widget.transform.localPosition = Vector3.zero;
		widget.transform.localRotation = Quaternion.identity;
		widget.transform.localScale = Vector3.one;
		widget.SetLayerOverride((GameLayer)base.gameObject.layer);
		widget.gameObject.hideFlags = HideFlags.DontSave | HideFlags.NotEditable;
		widget.name = $"Spawnable Library Item: {itemData.Name}";
		base.Owner.AddNestedInstance(widget, base.gameObject);
		HandleStartChangingStates();
		widget.RegisterReadyListener(delegate
		{
			m_readyState.SetAndDispatch();
		});
		widget.RegisterDoneChangingStatesListener(delegate
		{
			m_isLoading = false;
			HandleDoneChangingStates();
		}, null, callImmediatelyIfSet: true, doOnce: true);
		m_widget = widget;
		while (m_pendingEvents != null && m_pendingEvents.Count > 0)
		{
			(string, TriggerEventParameters) eventContext = m_pendingEvents.Dequeue();
			m_widget.TriggerEvent(eventContext.Item1, eventContext.Item2);
		}
	}

	private void CreateTextureItem(SpawnableLibrary.ItemData itemData)
	{
		m_isLoading = true;
		HandleStartChangingStates();
		HandleWidgetItemCleanUp();
		HandleTextureItemCleanUp();
		if (m_spawnableLibrary == null || string.IsNullOrEmpty(m_spawnableLibrary.BaseMaterial))
		{
			m_readyState.SetAndDispatch();
			return;
		}
		CreateRenderer(itemData);
		LoadAsset<Texture2D>(itemData.Reference, ++m_textureAsyncOperationId, HandleTextureLoaded);
		LoadAsset<Material>(m_spawnableLibrary.BaseMaterial, ++m_materialAsyncOperationId, HandleMaterialLoadedForInstancing);
	}

	private void HandleTextureLoaded(AssetReference assetRef, AssetHandle<Texture2D> assetHandle, object asyncOperationId)
	{
		if (assetHandle == null)
		{
			HandleAssetItemError(assetHandle);
			return;
		}
		if (m_textureAsyncOperationId != (int)asyncOperationId)
		{
			AssetHandle.SafeDispose(ref assetHandle);
			return;
		}
		m_texture = assetHandle.Asset;
		m_textureHandle = assetHandle;
		if (m_material != null)
		{
			m_material.mainTexture = m_texture;
			SetupRenderer();
		}
	}

	private void CreateSpriteItem(SpawnableLibrary.ItemData itemData)
	{
		m_isLoading = true;
		HandleStartChangingStates();
		HandleWidgetItemCleanUp();
		HandleTextureItemCleanUp();
		if (m_spawnableLibrary == null)
		{
			m_readyState.SetAndDispatch();
			return;
		}
		CreateRenderer(itemData);
		LoadAsset<Sprite>(itemData.Reference, ++m_spriteAsyncOperationId, HandleSpriteLoaded);
		if (itemData.Parameters is SpriteItemParameters parameters && !string.IsNullOrEmpty(parameters.MaterialReference.AssetString))
		{
			LoadAsset<Material>(parameters.MaterialReference.AssetString, ++m_materialAsyncOperationId, HandleMaterialLoaded);
		}
		else
		{
			LoadAsset<Material>(m_spawnableLibrary.BaseMaterial, ++m_materialAsyncOperationId, HandleMaterialLoaded);
		}
	}

	private void HandleSpriteLoaded(AssetReference assetRef, AssetHandle<Sprite> assetHandle, object asyncOperationId)
	{
		if (assetHandle == null)
		{
			HandleAssetItemError(assetHandle);
			return;
		}
		if (m_spriteAsyncOperationId != (int)asyncOperationId)
		{
			AssetHandle.SafeDispose(ref assetHandle);
			return;
		}
		m_sprite = assetHandle.Asset;
		m_spriteHandle = assetHandle;
		if (m_material != null)
		{
			SetupRenderer();
		}
	}

	private void CreateMaterialItem(SpawnableLibrary.ItemData itemData)
	{
		m_isLoading = true;
		HandleStartChangingStates();
		HandleWidgetItemCleanUp();
		HandleTextureItemCleanUp();
		if (m_spawnableLibrary == null)
		{
			m_readyState.SetAndDispatch();
			return;
		}
		CreateRenderer(itemData);
		LoadAsset<Material>(itemData.Reference, ++m_materialAsyncOperationId, HandleMaterialLoaded);
	}

	private void HandleMaterialLoaded(AssetReference assetRef, AssetHandle<Material> assetHandle, object asyncOperationId)
	{
		if (assetHandle == null)
		{
			HandleAssetItemError(assetHandle);
			return;
		}
		if (m_materialAsyncOperationId != (int)asyncOperationId)
		{
			AssetHandle.SafeDispose(ref assetHandle);
			return;
		}
		m_material = assetHandle.Asset;
		m_materialHandle = assetHandle;
		SetupRenderer();
	}

	private void HandleMaterialLoadedForInstancing(AssetReference assetRef, AssetHandle<Material> assetHandle, object asyncOperationId)
	{
		if (assetHandle == null)
		{
			HandleAssetItemError(assetHandle);
			return;
		}
		if (m_materialAsyncOperationId != (int)asyncOperationId)
		{
			AssetHandle.SafeDispose(ref assetHandle);
			return;
		}
		m_material = new Material(assetHandle.Asset);
		m_material.name = "?" + m_material.name;
		m_materialHandle = assetHandle;
		if (m_texture != null)
		{
			m_material.mainTexture = m_texture;
			SetupRenderer();
		}
	}

	private void CreateRenderer(SpawnableLibrary.ItemData itemData)
	{
		GameObject go = ((itemData.ItemType != SpawnableLibrary.ItemType.Sprite) ? GameObject.CreatePrimitive(PrimitiveType.Quad) : CreateSpriteRendererObject(itemData));
		go.name = $"Spawnable Library Item: {itemData.Name}";
		Quaternion desiredRotation;
		if (itemData.ItemType != SpawnableLibrary.ItemType.Sprite && m_intendedMesh != null)
		{
			go.GetComponent<MeshFilter>().mesh = m_intendedMesh;
			desiredRotation = Quaternion.identity;
		}
		else
		{
			WidgetTransform wt = GetComponent<WidgetTransform>();
			desiredRotation = WidgetTransformUtils.GetRotationFromZNegativeToDesiredFacing((wt != null) ? wt.Facing : FacingDirection.YPositive);
		}
		go.transform.SetParent(base.transform, worldPositionStays: false);
		go.transform.localPosition = Vector3.zero;
		go.transform.localRotation = desiredRotation;
		go.transform.localScale = Vector3.one;
		go.layer = base.gameObject.layer;
		go.hideFlags = HideFlags.DontSave | HideFlags.NotEditable;
		m_renderer = go.GetComponent<Renderer>();
		m_renderer.enabled = false;
	}

	private GameObject CreateSpriteRendererObject(SpawnableLibrary.ItemData itemData)
	{
		AssetReference atlasRef = null;
		GameObject go = new GameObject();
		go.SetActive(value: false);
		ManagedSprite managedSprite = ManagedSprite.Create(null, null, go);
		if (!(itemData.Parameters is SpriteItemParameters parameters))
		{
			global::Log.UIFramework.PrintError("[Spawnable] Failed to get sprite parameters for {0} (instance {1}). This sprite may not be configured correctly!", base.name, base.gameObject.GetInstanceID());
			return go;
		}
		managedSprite.UsesSpriteAtlas = true;
		atlasRef = AssetReference.CreateFromAssetString(parameters.SpriteAtlasReference.AssetString);
		if (atlasRef == null)
		{
			global::Log.UIFramework.PrintWarning("[Spawnable] Sprite {0} for renderer {1} (instance {2}) doesn't have a clear atlas associated with it. If there is an atlas for it, this could lead to missing assets in-game or the inability to track assets' lifetime!", itemData.Reference, base.name, base.gameObject.GetInstanceID());
			managedSprite.UsesSpriteAtlas = false;
		}
		managedSprite.SpriteAtlas = atlasRef;
		managedSprite.SpriteRenderer.color = parameters.Color;
		managedSprite.SpriteRenderer.flipX = parameters.FlipX;
		managedSprite.SpriteRenderer.flipY = parameters.FlipY;
		managedSprite.SpriteRenderer.drawMode = parameters.DrawMode;
		managedSprite.SpriteRenderer.size = parameters.Size;
		managedSprite.SpriteRenderer.tileMode = parameters.TileMode;
		managedSprite.SpriteRenderer.adaptiveModeThreshold = parameters.TileStretchValue;
		managedSprite.SpriteRenderer.maskInteraction = parameters.MaskInteraction;
		managedSprite.SpriteRenderer.spriteSortPoint = parameters.SortPoint;
		managedSprite.SpriteRenderer.sortingOrder = parameters.SortingOrder;
		go.SetActive(value: true);
		return go;
	}

	private void LoadAsset<T>(AssetReference assetRef, int asyncOperationId, AssetHandleCallback<T> assetLoadedHandler) where T : UnityEngine.Object
	{
		m_startedAssetLoadCount++;
		IAssetLoader assetLoader = AssetLoader.Get();
		if (assetLoader != null)
		{
			if (!assetLoader.LoadAsset(assetRef, assetLoadedHandler, asyncOperationId))
			{
				assetLoadedHandler(assetRef, null, asyncOperationId);
			}
			return;
		}
		Type assetType = typeof(T);
		if (!(assetType == typeof(Material)) && !(assetType == typeof(Texture2D)))
		{
			_ = assetType == typeof(Sprite);
		}
	}

	public void ClearLayerOverride()
	{
		if (m_originalLayer.HasValue)
		{
			base.gameObject.layer = (int)m_originalLayer.Value;
			if (m_widget != null)
			{
				m_widget.SetLayerOverride(m_originalLayer.Value);
			}
			else if (m_renderer != null)
			{
				m_renderer.gameObject.layer = base.gameObject.layer;
			}
			m_originalLayer = null;
			m_overrideLayer = null;
		}
	}

	public void SetLayerOverride(GameLayer layer)
	{
		if (!m_originalLayer.HasValue || base.gameObject.layer != 28)
		{
			m_originalLayer = (GameLayer)base.gameObject.layer;
		}
		m_overrideLayer = layer;
		base.gameObject.layer = (int)m_overrideLayer.Value;
		if (m_widget != null)
		{
			m_widget.SetLayerOverride(layer);
		}
		else if (m_renderer != null)
		{
			m_renderer.gameObject.layer = (int)layer;
		}
	}

	public void RegisterActivatedListener(Action<object> listener, object payload = null)
	{
		m_activatedState.RegisterSetListener(listener, payload);
	}

	public void RegisterDeactivatedListener(Action<object> listener, object payload = null)
	{
		m_deactivatedState.RegisterSetListener(listener, payload);
	}

	public void RegisterReadyListener(Action<object> listener, object payload = null, bool callImmediatelyIfReady = true)
	{
		m_readyState.RegisterSetListener(listener, payload, callImmediatelyIfReady);
	}

	public void RemoveReadyListener(Action<object> listener)
	{
		m_readyState.RemoveSetListener(listener);
	}

	public NestedReferenceComponentInfo GetComponentInfoById(long id)
	{
		NestedReferenceComponentInfo info = new NestedReferenceComponentInfo(null, componentsChecked: false);
		if (m_spawnableLibrary == null)
		{
			info.CheckedAllComponents = true;
			return info;
		}
		if (m_itemData == null)
		{
			return info;
		}
		bool isTransform = false;
		if (m_itemData.ID != id)
		{
			if (GetSpawnedItemRootTransformId(m_itemData) != id)
			{
				return info;
			}
			isTransform = true;
		}
		if (!IsReady)
		{
			return info;
		}
		switch (m_itemData.ItemType)
		{
		case SpawnableLibrary.ItemType.Texture:
		case SpawnableLibrary.ItemType.Material:
			info.FoundComponent = (isTransform ? ((Component)m_renderer.transform) : ((Component)m_renderer));
			info.CheckedAllComponents = true;
			break;
		case SpawnableLibrary.ItemType.Widget:
			info.FoundComponent = ((isTransform && m_widget != null) ? ((Component)m_widget.transform) : ((Component)m_widget));
			info.CheckedAllComponents = m_widget == null || m_widget.Widget != null;
			break;
		}
		return info;
	}

	public bool GetComponentId(Component component, out long id)
	{
		id = -1L;
		if (m_spawnableLibrary == null || m_itemData == null)
		{
			return false;
		}
		if ((component is WidgetInstance instance && m_widget != null && instance == m_widget) || (component is Renderer targetRenderer && targetRenderer == m_renderer))
		{
			id = m_itemData.ID;
			return true;
		}
		if (component is Transform)
		{
			id = GetSpawnedItemRootTransformId(m_itemData);
			return true;
		}
		return false;
	}

	private long GetSpawnedItemRootTransformId(SpawnableLibrary.ItemData itemData)
	{
		return $"{m_itemData.Name}_Transform".GetHashCode();
	}

	public string GetPathToObject()
	{
		return DebugUtils.GetHierarchyPath(base.transform, '/');
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		m_deactivatedState.Clear();
		m_activatedState.SetAndDispatch();
	}

	protected override void OnDisable()
	{
		m_activatedState.Clear();
		m_deactivatedState.SetAndDispatch();
		base.OnDisable();
	}

	protected override void OnDestroy()
	{
		HandleTextureItemCleanUp();
		m_textureAsyncOperationId++;
		m_materialAsyncOperationId++;
		base.OnDestroy();
	}

	[Conditional("UNITY_EDITOR")]
	private void Log(string message, string type)
	{
		Hearthstone.UI.Logging.Log.Get().AddMessage(message, this, LogLevel.Info, type);
	}
}
