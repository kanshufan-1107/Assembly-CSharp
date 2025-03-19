using System;
using System.Collections.Generic;
using Blizzard.T5.MaterialService.Extensions;
using Hearthstone.UI.Internal;
using UnityEngine;

namespace Hearthstone.UI;

[ExecuteAlways]
[AddComponentMenu("")]
[SelectionBase]
[NestedReferenceScope(NestedReference.Scope.Children)]
public abstract class Geometry : WidgetBehavior, INestedReferenceResolver, IAsyncInitializationBehavior
{
	protected PrefabInstance[] m_prefabInstances;

	private FlagStateTracker m_readyState;

	private FlagStateTracker m_activatedEvent;

	private FlagStateTracker m_deactivatedEvent;

	private int m_instancesPendingInitialization;

	protected abstract WeakAssetReference[] ModelReferences { get; }

	public ICollection<DynamicPropertyInfo> DynamicProperties
	{
		get
		{
			List<DynamicPropertyInfo> properties = new List<DynamicPropertyInfo>();
			ForEachProperty(delegate(string id, Renderer renderer)
			{
				properties.Add(new DynamicPropertyInfo
				{
					Id = id,
					Name = id,
					Type = typeof(Material),
					Value = renderer.GetSharedMaterial()
				});
			});
			return properties;
		}
	}

	public bool IsReady => m_readyState.IsSet;

	public Behaviour Container => this;

	public override bool IsChangingStates => !IsReady;

	protected override void Awake()
	{
		base.Awake();
	}

	protected override void OnDestroy()
	{
		if (m_prefabInstances != null)
		{
			PrefabInstance[] prefabInstances = m_prefabInstances;
			for (int i = 0; i < prefabInstances.Length; i++)
			{
				prefabInstances[i].Destroy();
			}
		}
		base.OnDestroy();
	}

	protected override void OnInitialize()
	{
		InstantiateModels();
	}

	private void InstantiateModels()
	{
		WeakAssetReference[] modelReferences = ModelReferences;
		if (m_prefabInstances == null)
		{
			m_prefabInstances = new PrefabInstance[modelReferences.Length];
		}
		m_instancesPendingInitialization = m_prefabInstances.Length;
		for (int i = 0; i < m_prefabInstances.Length; i++)
		{
			PrefabInstance inst = m_prefabInstances[i];
			if (inst == null)
			{
				inst = new PrefabInstance(base.gameObject);
				m_prefabInstances[i] = inst;
			}
			if (!inst.IsInstanceReady)
			{
				m_readyState.Clear();
			}
			inst.LoadPrefab(modelReferences[i], base.WillLoadSynchronously);
			inst.InstantiateWhenReady();
			inst.RegisterInstanceReadyListener(HandleInstanceReady);
		}
	}

	private void HandleInstanceReady(object unused)
	{
		m_instancesPendingInitialization--;
		if (m_instancesPendingInitialization == 0)
		{
			OnInstancesReady(m_prefabInstances);
		}
	}

	protected virtual void OnInstancesReady(PrefabInstance[] instances)
	{
		m_readyState.SetAndDispatch();
	}

	private void ForEachProperty(Action<string, Renderer> callback)
	{
		for (int i = 0; i < m_prefabInstances.Length; i++)
		{
			PrefabInstance inst = m_prefabInstances[i];
			if (inst.IsInstanceReady && inst.Instance != null)
			{
				Renderer[] componentsInChildren = inst.Instance.GetComponentsInChildren<Renderer>(includeInactive: true);
				foreach (Renderer r in componentsInChildren)
				{
					string id = $"{i}.{r.name}";
					callback(id, r);
				}
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
		Component[] componentsInChildren = GetComponentsInChildren<Component>(includeInactive: true);
		foreach (Component component in componentsInChildren)
		{
			if (component.ToString().GetHashCode() == id)
			{
				return new NestedReferenceComponentInfo(component);
			}
		}
		return new NestedReferenceComponentInfo(null);
	}

	public bool GetComponentId(Component component, out long id)
	{
		id = component.ToString().GetHashCode();
		return true;
	}

	public string GetPathToObject()
	{
		return DebugUtils.GetHierarchyPath(base.transform, '/');
	}

	public override bool GetIsChangingStates(Func<GameObject, bool> includeGameObject)
	{
		if (!includeGameObject(base.gameObject))
		{
			return false;
		}
		return IsChangingStates;
	}
}
