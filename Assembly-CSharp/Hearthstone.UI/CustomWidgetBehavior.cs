using System;
using System.Collections.Generic;
using Blizzard.T5.Core;
using Blizzard.T5.Core.Utils;
using Hearthstone.UI.Core;
using Hearthstone.UI.Internal;
using Unity.Profiling;
using UnityEngine;

namespace Hearthstone.UI;

[ExecuteAlways]
[AddComponentMenu("")]
[SelectionBase]
[NestedReferenceScope(NestedReference.Scope.Children)]
public abstract class CustomWidgetBehavior : WidgetBehavior, IAsyncInitializationBehavior, IVisibleWidgetComponent, ILayerOverridable, IPopupRendering
{
	protected delegate void CreateObjectDelegate(IPreviewableObject previewableObject, Action<GameObject> createdCallback);

	protected delegate bool ShouldObjectBeRecreatedDelegate(IPreviewableObject previewableObject);

	protected interface IPreviewableObject
	{
		object Context { get; set; }
	}

	private class PreviewableObject : IPreviewableObject
	{
		public GameObject Object;

		public CreateObjectDelegate CreateObject;

		public ShouldObjectBeRecreatedDelegate ShouldObjectBeRecreated;

		public object Context { get; set; }

		public Exception FailureException { get; set; }

		public PreviewableObject(CreateObjectDelegate createObject, ShouldObjectBeRecreatedDelegate shouldObjectBeRecreated)
		{
			CreateObject = createObject;
			ShouldObjectBeRecreated = shouldObjectBeRecreated;
		}
	}

	private List<PreviewableObject> m_previewableObjects = new List<PreviewableObject>();

	private FlagStateTracker m_activatedEvent;

	private FlagStateTracker m_deactivatedEvent;

	private bool m_initialized;

	private GameLayer m_defaultLayer;

	private bool m_layerOverridden;

	private GameLayer m_layerOverride;

	private bool m_isVisibleInternally = true;

	private IPopupRoot m_popupRoot;

	private HashSet<IPopupRendering> m_popupRenderingComponents = new HashSet<IPopupRendering>();

	private Map<Renderer, int> m_originalRendererLayers;

	private static Pool<List<Component>> s_componentListPool = new Pool<List<Component>>((int _) => new List<Component>(), delegate
	{
	}, 1);

	private static ProfilerMarker s_onUpdateProfilerMarker = new ProfilerMarker("CustomWidgetBehavior.OnUpdate");

	private FlagStateTracker m_readyStateTracker;

	public override bool IsChangingStates => false;

	public bool HandlesChildLayers => true;

	public bool IsDesiredHidden => base.Owner.IsDesiredHidden;

	public bool IsDesiredHiddenInHierarchy
	{
		get
		{
			if (base.Owner.IsDesiredHiddenInHierarchy && !m_isVisibleInternally)
			{
				return true;
			}
			return false;
		}
	}

	public virtual bool HandlesChildVisibility => true;

	public bool IsReady
	{
		get
		{
			if (!m_readyStateTracker.IsSet)
			{
				return !m_initialized;
			}
			return true;
		}
	}

	public Behaviour Container => this;

	public override bool GetIsChangingStates(Func<GameObject, bool> includeGameObject)
	{
		return false;
	}

	public virtual void SetVisibility(bool isVisible, bool isInternal)
	{
		if (isInternal)
		{
			m_isVisibleInternally = isVisible;
		}
		GameObjectUtils.WalkSelfAndChildren(base.transform, delegate(Transform current)
		{
			bool flag = false;
			List<Component> list = s_componentListPool.Acquire();
			current.GetComponents(list);
			Renderer renderer = null;
			PegUIElement pegUIElement = null;
			UberText uberText = null;
			IVisibleWidgetComponent visibleWidgetComponent = null;
			foreach (Component current2 in list)
			{
				if (current2 is Renderer)
				{
					renderer = (Renderer)current2;
				}
				else if (current2 is PegUIElement)
				{
					pegUIElement = (PegUIElement)current2;
				}
				else if (current2 is UberText)
				{
					uberText = (UberText)current2;
				}
				else if (current2 is IVisibleWidgetComponent)
				{
					visibleWidgetComponent = (IVisibleWidgetComponent)current2;
				}
			}
			list.Clear();
			s_componentListPool.Release(list);
			if (renderer != null)
			{
				RenderUtils.SetInvisibleRenderer(renderer, isVisible, ref m_originalRendererLayers);
			}
			if (pegUIElement != null && (base.InitializationStartTime > pegUIElement.SetEnabledLastCallTime || m_initialized))
			{
				pegUIElement.SetEnabled(isVisible, isInternal);
			}
			if (uberText != null)
			{
				if (isVisible)
				{
					uberText.Show();
				}
				else
				{
					uberText.Hide();
				}
				flag = true;
			}
			if (visibleWidgetComponent != null && (Component)visibleWidgetComponent != this)
			{
				visibleWidgetComponent.SetVisibility(isVisible && !visibleWidgetComponent.IsDesiredHidden, isInternal);
				flag = visibleWidgetComponent.HandlesChildVisibility;
				if (flag)
				{
					PopupRenderer[] componentsInChildren = current.GetComponentsInChildren<PopupRenderer>();
					for (int i = 0; i < componentsInChildren.Length; i++)
					{
						componentsInChildren[i].SetVisibility(isVisible && !visibleWidgetComponent.IsDesiredHidden, isInternal);
					}
				}
			}
			return !flag;
		});
	}

	protected override void OnInitialize()
	{
		CleanUp();
		m_initialized = true;
		m_readyStateTracker.SetAndDispatch();
	}

	private void CleanUp()
	{
		OwnedByWidgetBehavior[] componentsInChildren = GetComponentsInChildren<OwnedByWidgetBehavior>();
		foreach (OwnedByWidgetBehavior obj in componentsInChildren)
		{
			if (obj.Owner == this)
			{
				HandleDestroy(obj.gameObject);
			}
		}
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		m_activatedEvent.SetAndDispatch();
	}

	protected override void OnDisable()
	{
		m_deactivatedEvent.SetAndDispatch();
		base.OnDisable();
	}

	public void RegisterActivatedListener(Action<object> listener, object payload = null)
	{
		m_activatedEvent.RegisterSetListener(listener, payload, callImmediatelyIfSet: false);
	}

	public void RegisterDeactivatedListener(Action<object> listener, object payload = null)
	{
		m_deactivatedEvent.RegisterSetListener(listener, payload, callImmediatelyIfSet: false);
	}

	public override void OnUpdate()
	{
		using (s_onUpdateProfilerMarker.Auto())
		{
			base.OnUpdate();
			foreach (PreviewableObject previewableObject in m_previewableObjects)
			{
				if (previewableObject.FailureException != null)
				{
					continue;
				}
				try
				{
					if (!previewableObject.ShouldObjectBeRecreated(previewableObject))
					{
						continue;
					}
					m_readyStateTracker.Clear();
					if (previewableObject.Object != null)
					{
						HandleDestroy(previewableObject.Object);
					}
					PreviewableObject o = previewableObject;
					previewableObject.CreateObject(previewableObject, delegate(GameObject obj)
					{
						o.Object = obj;
						if (obj != null)
						{
							if (obj.transform.parent != base.transform)
							{
								obj.transform.SetParent(base.transform, worldPositionStays: true);
							}
							obj.AddComponent<OwnedByWidgetBehavior>().Owner = this;
							LayerUtils.SetLayer(obj, (int)m_layerOverride, 29);
							GameObjectUtils.WalkSelfAndChildren(obj.transform, delegate(Transform childTransform)
							{
								ILayerOverridable component = childTransform.GetComponent<ILayerOverridable>();
								if (base.gameObject.layer == 31)
								{
									childTransform.gameObject.layer = 31;
									component?.SetLayerOverride(GameLayer.CameraMask);
								}
								else
								{
									component?.SetLayerOverride(m_layerOverride);
								}
								return true;
							});
							if (m_popupRoot != null)
							{
								Transform parent = (obj.transform.IsChildOf(base.transform) ? base.transform : obj.transform);
								m_popupRoot.ApplyPopupRendering(parent, m_popupRenderingComponents, m_layerOverridden, (int)m_layerOverride, !IsDesiredHiddenInHierarchy);
							}
							if (IsDesiredHiddenInHierarchy)
							{
								SetVisibility(isVisible: false, isInternal: true);
							}
						}
						m_readyStateTracker.SetAndDispatch();
					});
				}
				catch (Exception ex)
				{
					Exception ex3 = (previewableObject.FailureException = ex);
					throw ex3;
				}
			}
		}
	}

	public virtual void Hide()
	{
		if (m_initialized)
		{
			SetVisibility(isVisible: false, isInternal: false);
		}
	}

	public virtual void Show()
	{
		if (m_initialized)
		{
			SetVisibility(isVisible: true, isInternal: false);
		}
	}

	protected override void OnDestroy()
	{
		CleanUp();
		base.OnDestroy();
	}

	private void HandleDestroy(GameObject go)
	{
		Actor actor = go.GetComponent<Actor>();
		if (actor != null)
		{
			actor.Destroy();
		}
		else
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	protected IPreviewableObject CreatePreviewableObject(CreateObjectDelegate createObject, ShouldObjectBeRecreatedDelegate shouldObjectBeRecreated, object context = null)
	{
		PreviewableObject previewableObject = new PreviewableObject(createObject, shouldObjectBeRecreated);
		m_previewableObjects.Add(previewableObject);
		previewableObject.Context = context;
		return previewableObject;
	}

	public void RegisterReadyListener(Action<object> listener, object payload, bool callImmediatelyIfReady = true)
	{
		m_readyStateTracker.RegisterSetListener(listener, payload, callImmediatelyIfReady);
	}

	public void RemoveReadyListener(Action<object> listener)
	{
		m_readyStateTracker.RemoveSetListener(listener);
	}

	public void SetLayerOverride(GameLayer layer)
	{
		if (!m_layerOverridden)
		{
			m_defaultLayer = (GameLayer)base.gameObject.layer;
		}
		m_layerOverridden = true;
		m_layerOverride = layer;
		foreach (PreviewableObject previewableObject in m_previewableObjects)
		{
			if (!(previewableObject.Object == null))
			{
				GameObjectUtils.WalkSelfAndChildren(previewableObject.Object.transform, delegate(Transform child)
				{
					ILayerOverridable component = child.GetComponent<ILayerOverridable>();
					child.gameObject.layer = (int)m_layerOverride;
					component?.SetLayerOverride(m_layerOverride);
					return true;
				});
			}
		}
		LayerUtils.SetLayer(this, layer);
	}

	public void ClearLayerOverride()
	{
		m_layerOverride = m_defaultLayer;
		m_layerOverridden = false;
		foreach (PreviewableObject previewableObject in m_previewableObjects)
		{
			if (!(previewableObject.Object == null))
			{
				LayerUtils.SetLayer(previewableObject.Object, (int)m_defaultLayer, 29);
			}
		}
	}

	public void EnablePopupRendering(IPopupRoot popupRoot)
	{
		if (m_popupRoot != popupRoot)
		{
			popupRoot.ApplyPopupRendering(base.transform, m_popupRenderingComponents, m_layerOverridden, (int)m_layerOverride, m_isVisibleInternally);
			foreach (PreviewableObject preview in m_previewableObjects)
			{
				if (!(preview.Object == null))
				{
					popupRoot.ApplyPopupRendering(preview.Object.transform, m_popupRenderingComponents, m_layerOverridden, (int)m_layerOverride, m_isVisibleInternally);
				}
			}
		}
		m_popupRoot = popupRoot;
	}

	protected void ApplyPopupRenderingTo(Transform transform)
	{
		if (m_popupRoot != null)
		{
			m_popupRoot.ApplyPopupRendering(transform, m_popupRenderingComponents, m_layerOverridden, (int)m_layerOverride, m_isVisibleInternally);
		}
	}

	private void PropagatePopupRendering(object obj)
	{
		if (obj is IPopupRoot popupRoot)
		{
			popupRoot.ApplyPopupRendering(base.transform, m_popupRenderingComponents, m_layerOverridden, (int)m_layerOverride, m_isVisibleInternally);
		}
	}

	public void DisablePopupRendering()
	{
		if (m_popupRoot != null)
		{
			m_popupRoot.CleanupPopupRendering(m_popupRenderingComponents);
			m_popupRoot = null;
		}
	}

	public bool HandlesChildPropagation()
	{
		return true;
	}
}
