using System;
using System.Collections.Generic;
using System.Diagnostics;
using Blizzard.T5.Core.Utils;
using Hearthstone.UI.Core;
using Hearthstone.UI.Logging;
using Unity.Profiling;
using UnityEngine;

namespace Hearthstone.UI;

[HelpURL("https://confluence.blizzard.com/x/GYiGJ")]
[DisallowMultipleComponent]
[ExecuteAlways]
[AddComponentMenu("")]
public class Clickable : WidgetBehavior, IBoundsDependent, ILayerOverridable
{
	public enum VisualState
	{
		Active,
		Inactive,
		Selected,
		Deselected,
		Clicked,
		Released,
		MouseOver,
		MouseOut,
		DoubleClicked,
		DragStarted,
		DragReleased,
		RightClicked
	}

	public enum ColliderType
	{
		Bounds,
		Geometry
	}

	private readonly string ACTIVE = "active";

	private readonly string INACTIVE = "inactive";

	private readonly string SELECTED = "selected";

	private readonly string DESELECTED = "deselected";

	private readonly string CLICKED = "clicked";

	private readonly string RELEASED = "released";

	private readonly string MOUSEOVER = "mouseover";

	private readonly string MOUSEOUT = "mouseout";

	private readonly string DOUBLECLICKED = "doubleclicked";

	private readonly string DRAG_STARTED = "dragstarted";

	private readonly string DRAG_RELEASED = "dragreleased";

	private readonly string RIGHTCLICKED = "rightclicked";

	[HideInInspector]
	[SerializeField]
	private ColliderType m_colliderType;

	[HideInInspector]
	[SerializeField]
	private GameObject[] m_geometryRoots;

	[SerializeField]
	[WidgetBehaviorStateEnum(typeof(VisualState), "")]
	private WidgetBehaviorStateCollection m_stateCollection;

	private List<Collider> m_geometryColliders;

	private Collider m_boundsCollider;

	private PegUIElement m_pegUiElement;

	private GameLayer? m_originalLayer;

	private GameLayer? m_overrideLayer;

	private bool m_active = true;

	private bool m_activeChanged = true;

	private bool m_hovered;

	private bool m_selected;

	private bool m_clicked;

	private bool m_dragged;

	private static ProfilerMarker s_onUpdateProfilerMarker = new ProfilerMarker("Clickable.OnUpdate");

	[Overridable]
	public bool Active
	{
		get
		{
			if (m_active)
			{
				return base.IsActive;
			}
			return false;
		}
		set
		{
			m_activeChanged = m_activeChanged || m_active != value;
			m_active = value;
			if (m_activeChanged && !m_active)
			{
				OnRelease(null);
				OnReleaseAll(null);
				OnDeselected(null);
				OnRollOut(null);
			}
			if (m_pegUiElement != null)
			{
				m_pegUiElement.SetEnabled(value);
			}
			if (m_boundsCollider != null)
			{
				m_boundsCollider.enabled = value && base.IsActive;
			}
			SetMeshCollidersEnabled(value && base.IsActive);
		}
	}

	public bool NeedsBounds => m_colliderType == ColliderType.Bounds;

	[Overridable]
	public GameLayer OverrideMaskLayer
	{
		get
		{
			return m_overrideLayer.Value;
		}
		set
		{
			if (m_overrideLayer != value)
			{
				m_overrideLayer = value;
			}
		}
	}

	public bool HandlesChildLayers => false;

	public override bool IsChangingStates
	{
		get
		{
			if (m_stateCollection != null)
			{
				return m_stateCollection.IsChangingStates;
			}
			return true;
		}
	}

	protected override void OnDisable()
	{
		if (m_boundsCollider != null)
		{
			m_boundsCollider.enabled = false;
			WidgetTransform widgetTransform = GetComponent<WidgetTransform>();
			if (widgetTransform != null)
			{
				widgetTransform.OnBoundsChanged -= OnBoundsChanged;
			}
		}
		SetMeshCollidersEnabled(enable: false);
		base.OnDisable();
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		if (m_boundsCollider != null)
		{
			m_boundsCollider.enabled = m_active;
			WidgetTransform widgetTransform = GetComponent<WidgetTransform>();
			if (widgetTransform != null)
			{
				widgetTransform.OnBoundsChanged -= OnBoundsChanged;
				widgetTransform.OnBoundsChanged += OnBoundsChanged;
			}
		}
		SetMeshCollidersEnabled(m_active);
	}

	private PegUIElement GetOrCreatePegUIElement()
	{
		if (m_pegUiElement == null)
		{
			m_pegUiElement = base.gameObject.GetComponent<PegUIElement>() ?? base.gameObject.AddComponent<PegUIElement>();
			m_pegUiElement.hideFlags = HideFlags.DontSave;
		}
		return m_pegUiElement;
	}

	protected override void OnInitialize()
	{
		if (m_stateCollection == null)
		{
			m_stateCollection = new WidgetBehaviorStateCollection();
		}
		if (!Application.IsPlaying(this))
		{
			return;
		}
		PegUIElement pegUi = GetOrCreatePegUIElement();
		pegUi.AddEventListener(UIEventType.RELEASE, OnRelease);
		pegUi.AddEventListener(UIEventType.RELEASEALL, OnReleaseAll);
		pegUi.AddEventListener(UIEventType.ROLLOVER, OnRollOver);
		pegUi.AddEventListener(UIEventType.ROLLOUT, OnRollOut);
		pegUi.AddEventListener(UIEventType.PRESS, OnClick);
		pegUi.AddEventListener(UIEventType.TAP, OnSelected);
		if (m_stateCollection.DoesStateExist(DOUBLECLICKED))
		{
			pegUi.AddEventListener(UIEventType.DOUBLECLICK, OnDoubleClick);
		}
		if (m_stateCollection.DoesStateExist(RIGHTCLICKED))
		{
			pegUi.AddEventListener(UIEventType.RIGHTCLICK, OnRightClick);
		}
		pegUi.AddEventListener(UIEventType.DRAG, OnDrag);
		pegUi.SetEnabled(m_active && !base.Owner.IsDesiredHiddenInHierarchy, isInternal: true);
		switch (m_colliderType)
		{
		case ColliderType.Bounds:
		{
			WidgetTransform widgetTransform = base.gameObject.GetComponent<WidgetTransform>();
			if (widgetTransform != null)
			{
				m_boundsCollider = widgetTransform.CreateBoxCollider(base.gameObject);
				m_boundsCollider.enabled = Active;
				widgetTransform.OnBoundsChanged -= OnBoundsChanged;
				widgetTransform.OnBoundsChanged += OnBoundsChanged;
			}
			break;
		}
		case ColliderType.Geometry:
		{
			GameObject[] geometryRoots = m_geometryRoots;
			foreach (GameObject geoRoot in geometryRoots)
			{
				if (!(geoRoot == null))
				{
					ApplyPegUIProxies(geoRoot.transform);
				}
			}
			break;
		}
		}
	}

	private void OnBoundsChanged()
	{
		BoxCollider boxCollider = m_boundsCollider as BoxCollider;
		if (boxCollider != null)
		{
			WidgetTransform widgetTransform = GetComponent<WidgetTransform>();
			if (widgetTransform != null)
			{
				widgetTransform.SetBoxColliderDimensionsToBounds(boxCollider);
			}
		}
	}

	private void ApplyPegUIProxies(object root)
	{
		if (this == null || root == null)
		{
			return;
		}
		GameObjectUtils.WalkSelfAndChildren((Transform)root, delegate(Transform child)
		{
			if (child == null)
			{
				return false;
			}
			Component[] components = child.GetComponents<Component>();
			foreach (Component component in components)
			{
				MeshFilter meshFilter = component as MeshFilter;
				if (meshFilter != null)
				{
					PopulateMeshesWithPegUIProxies(meshFilter);
				}
				else
				{
					Geometry geometry = component as Geometry;
					if (geometry != null)
					{
						geometry.RegisterReadyListener(delegate
						{
							PopulateGeometryMeshesWithPegUIProxies(geometry);
						});
					}
				}
			}
			List<IAsyncInitializationBehavior> asyncBehaviors = AsyncBehaviorUtils.GetAsyncBehaviors(child);
			if (asyncBehaviors != null)
			{
				foreach (IAsyncInitializationBehavior item in asyncBehaviors)
				{
					item.RegisterReadyListener(ApplyPegUIProxies, child, callImmediatelyIfReady: false);
				}
			}
			return true;
		});
	}

	private void OnSelected(UIEvent e)
	{
		if (!m_selected && Active)
		{
			m_selected = true;
			SetVisualState(VisualState.Selected);
		}
	}

	private void OnDeselected(UIEvent e)
	{
		if (m_selected)
		{
			m_selected = false;
			SetVisualState(VisualState.Deselected);
		}
	}

	private void OnRelease(UIEvent e)
	{
		if (m_dragged)
		{
			m_clicked = false;
			m_dragged = false;
			SetVisualState(VisualState.DragReleased);
		}
		else if (m_clicked)
		{
			m_clicked = false;
			SetVisualState(VisualState.Released);
		}
	}

	private void OnReleaseAll(UIEvent e)
	{
		m_clicked = false;
		if (m_dragged)
		{
			m_dragged = false;
			SetVisualState(VisualState.DragReleased);
		}
	}

	private void OnRollOut(UIEvent e)
	{
		if (m_hovered)
		{
			m_hovered = false;
			SetVisualState(VisualState.MouseOut);
		}
	}

	private void OnRollOver(UIEvent e)
	{
		if (!m_hovered && Active)
		{
			m_hovered = true;
			SetVisualState(VisualState.MouseOver);
		}
	}

	private void OnClick(UIEvent e)
	{
		if (!m_clicked && Active)
		{
			m_clicked = true;
			SetVisualState(VisualState.Clicked);
		}
	}

	private void OnDoubleClick(UIEvent e)
	{
		if (m_clicked && Active)
		{
			m_clicked = false;
			SetVisualState(VisualState.DoubleClicked);
		}
	}

	private void OnRightClick(UIEvent e)
	{
		if (Active)
		{
			m_clicked = false;
			SetVisualState(VisualState.RightClicked);
		}
	}

	private void OnDrag(UIEvent e)
	{
		if (!m_dragged && Active)
		{
			m_dragged = true;
			SetVisualState(VisualState.DragStarted);
		}
	}

	public override void OnUpdate()
	{
		using (s_onUpdateProfilerMarker.Auto())
		{
			if (m_activeChanged)
			{
				SetVisualState((!m_active) ? VisualState.Inactive : VisualState.Active);
				m_activeChanged = false;
			}
			if (m_stateCollection != null)
			{
				m_stateCollection.Update(this);
			}
		}
	}

	private bool SetVisualState(VisualState visualState)
	{
		m_stateCollection.IndependentStates = true;
		switch (visualState)
		{
		case VisualState.Active:
			m_stateCollection.AbortState(INACTIVE);
			return m_stateCollection.ActivateState(this, ACTIVE);
		case VisualState.Inactive:
			m_stateCollection.AbortState(ACTIVE);
			return m_stateCollection.ActivateState(this, INACTIVE);
		case VisualState.Selected:
			m_stateCollection.AbortState(DESELECTED);
			return m_stateCollection.ActivateState(this, SELECTED);
		case VisualState.Deselected:
			m_stateCollection.AbortState(SELECTED);
			return m_stateCollection.ActivateState(this, DESELECTED);
		case VisualState.Clicked:
			m_stateCollection.AbortState(RELEASED);
			return m_stateCollection.ActivateState(this, CLICKED);
		case VisualState.Released:
			m_stateCollection.AbortState(CLICKED);
			return m_stateCollection.ActivateState(this, RELEASED);
		case VisualState.MouseOver:
			m_stateCollection.AbortState(MOUSEOUT);
			return m_stateCollection.ActivateState(this, MOUSEOVER);
		case VisualState.MouseOut:
			m_stateCollection.AbortState(MOUSEOVER);
			return m_stateCollection.ActivateState(this, MOUSEOUT);
		case VisualState.DoubleClicked:
			m_stateCollection.AbortState(RELEASED);
			return m_stateCollection.ActivateState(this, DOUBLECLICKED);
		case VisualState.DragStarted:
			m_stateCollection.AbortState(DRAG_RELEASED);
			return m_stateCollection.ActivateState(this, DRAG_STARTED);
		case VisualState.DragReleased:
			m_stateCollection.AbortState(DRAG_STARTED);
			return m_stateCollection.ActivateState(this, DRAG_RELEASED);
		case VisualState.RightClicked:
			m_stateCollection.AbortState(RELEASED);
			return m_stateCollection.ActivateState(this, RIGHTCLICKED);
		default:
			return false;
		}
	}

	public void SetLayerOverride(GameLayer layer)
	{
		if (m_overrideLayer.HasValue)
		{
			base.gameObject.layer = (int)m_overrideLayer.Value;
		}
		else if (!m_originalLayer.HasValue)
		{
			GameObject o = base.gameObject;
			m_originalLayer = (GameLayer)o.layer;
			o.layer = (int)layer;
		}
	}

	public void ClearLayerOverride()
	{
		if (m_originalLayer.HasValue)
		{
			base.gameObject.layer = (int)m_originalLayer.Value;
			m_originalLayer = null;
		}
	}

	public virtual bool AddEventListener(UIEventType type, UIEvent.Handler handler)
	{
		return GetOrCreatePegUIElement().AddEventListener(type, handler);
	}

	public virtual bool RemoveEventListener(UIEventType type, UIEvent.Handler handler)
	{
		if (m_pegUiElement != null)
		{
			return m_pegUiElement.RemoveEventListener(type, handler);
		}
		return false;
	}

	public virtual object GetData()
	{
		return GetOrCreatePegUIElement().GetData();
	}

	public virtual void SetData(object data)
	{
		GetOrCreatePegUIElement().SetData(data);
	}

	private void PopulateGeometryMeshesWithPegUIProxies(Geometry geo)
	{
		MeshFilter[] componentsInChildren = geo.GetComponentsInChildren<MeshFilter>(includeInactive: true);
		foreach (MeshFilter meshFilter in componentsInChildren)
		{
			PopulateMeshesWithPegUIProxies(meshFilter);
		}
	}

	private void PopulateMeshesWithPegUIProxies(MeshFilter meshFilter)
	{
		if (!meshFilter.sharedMesh.isReadable)
		{
			Log.UIFramework.PrintError("You can't add a MeshCollider to this object as the Mesh is not readable : " + meshFilter.sharedMesh.name + ". Consider using Bounds setting on Clickable component as it is much more efficient for CPU & Memory!");
			return;
		}
		if (m_geometryColliders == null)
		{
			m_geometryColliders = new List<Collider>();
		}
		MeshCollider meshCollider = meshFilter.gameObject.AddComponent<MeshCollider>();
		meshCollider.hideFlags = HideFlags.DontSave;
		meshCollider.sharedMesh = meshFilter.sharedMesh;
		meshCollider.enabled = Active;
		m_geometryColliders.Add(meshCollider);
		meshCollider.gameObject.AddComponent<PegUIElementProxy>().Owner = m_pegUiElement;
	}

	private void SetMeshCollidersEnabled(bool enable)
	{
		if (m_geometryColliders == null)
		{
			return;
		}
		foreach (Collider collider in m_geometryColliders)
		{
			if (!(collider == null))
			{
				collider.enabled = enable;
			}
		}
	}

	public override bool GetIsChangingStates(Func<GameObject, bool> includeGameObject)
	{
		if (!includeGameObject(base.gameObject))
		{
			return false;
		}
		return IsChangingStates;
	}

	[Conditional("UNITY_EDITOR")]
	private void LogMessage(string message, string type)
	{
		Hearthstone.UI.Logging.Log.Get().AddMessage(message, this, LogLevel.Info, type);
	}
}
