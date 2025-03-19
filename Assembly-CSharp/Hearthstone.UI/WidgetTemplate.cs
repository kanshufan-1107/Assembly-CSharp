using System;
using System.Collections.Generic;
using System.Linq;
using Blizzard.T5.Core;
using Blizzard.T5.Core.Utils;
using Blizzard.T5.Jobs;
using Blizzard.T5.Services;
using Hearthstone.Core;
using Hearthstone.UI.Logging;
using Unity.Profiling;
using UnityEngine;

namespace Hearthstone.UI;

[ExecuteAlways]
[AddComponentMenu("")]
[DisallowMultipleComponent]
public class WidgetTemplate : Widget, ISerializationCallbackReceiver, IWidgetEventListener
{
	private enum DataChangeSource
	{
		Parent,
		Template,
		GameObjectBinding,
		Global
	}

	[Flags]
	public enum UpdateTargets
	{
		None = 0,
		Children = 2,
		Behaviors = 4,
		All = 6
	}

	private class GameObjectBinding
	{
		public delegate void DataChangedDelegate(IDataModel dataModel, GameObjectBinding binding);

		private DataContext m_dataContext;

		private HashSet<int> m_ownedDataModels = new HashSet<int>();

		private DataChangedDelegate m_onDataChanged;

		public DataContext DataContext => m_dataContext;

		public GameObjectBinding(DataContext dataContext)
		{
			m_dataContext = dataContext;
			m_dataContext.RegisterChangedListener(HandleDataContextChanged);
		}

		public void BindDataModel(IDataModel dataModel, bool owned)
		{
			if (owned)
			{
				m_ownedDataModels.Add(dataModel.DataModelId);
			}
			else
			{
				m_ownedDataModels.Remove(dataModel.DataModelId);
			}
			m_dataContext.BindDataModel(dataModel);
		}

		public void UnbindDataModel(int dataModelId)
		{
			m_dataContext.UnbindDataModel(dataModelId);
			m_ownedDataModels.Remove(dataModelId);
		}

		public bool HasDataModelInstance(IDataModel dataModel)
		{
			return m_dataContext.HasDataModelInstance(dataModel);
		}

		public bool OwnsDataModel(IDataModel dataModel)
		{
			return m_ownedDataModels.Contains(dataModel.DataModelId);
		}

		public void RegisterChangedListener(DataChangedDelegate listener)
		{
			m_onDataChanged = (DataChangedDelegate)Delegate.Remove(m_onDataChanged, listener);
			m_onDataChanged = (DataChangedDelegate)Delegate.Combine(m_onDataChanged, listener);
		}

		public void RemoveChangedListener(DataChangedDelegate listener)
		{
			m_onDataChanged = (DataChangedDelegate)Delegate.Remove(m_onDataChanged, listener);
		}

		private void HandleDataContextChanged(IDataModel dataModel)
		{
			m_onDataChanged?.Invoke(dataModel, this);
		}

		public void Deinitialize()
		{
			m_dataContext.RemoveChangedListener(HandleDataContextChanged);
		}
	}

	[Serializable]
	public struct KeyValuePair
	{
		public class Comparer : IEqualityComparer<KeyValuePair>
		{
			public bool Equals(KeyValuePair x, KeyValuePair y)
			{
				return x.Value == y.Value;
			}

			public int GetHashCode(KeyValuePair obj)
			{
				return obj.Key.GetHashCode();
			}
		}

		public long Key;

		public Component Value;
	}

	[HideInInspector]
	[SerializeField]
	private List<KeyValuePair> m_pairs;

	private HashSet<Component> m_componentsSet;

	private Map<long, Component> m_componentsById;

	private int m_numComponentsPendingInitialization;

	private DataContext m_dataContext = new DataContext();

	private List<WidgetBehavior> m_widgetBehaviors;

	private List<WidgetInstance> m_nestedInstances;

	private List<WidgetInstance> m_addedInstances;

	private List<VisualController> m_visualControllers;

	private List<IAsyncInitializationBehavior> m_deactivatedComponents;

	private Dictionary<GameObject, GameObjectBinding> m_gameObjectsToBindingsMap = new Dictionary<GameObject, GameObjectBinding>();

	private List<GameObject> m_newlyBoundGameObjects;

	private InitializationState m_initializationState;

	private WidgetTemplate m_parentWidgetTemplate;

	private float m_initializationStartTime;

	private bool m_waitForParentToShow = true;

	private UpdateTargets m_updateTargets = UpdateTargets.All;

	private HashSet<WidgetTemplate> m_widgetsPendingTick;

	private HashSet<WidgetTemplate> m_widgetsToTickThisIteration;

	private List<IStatefulWidgetComponent> m_componentsChangingStates = new List<IStatefulWidgetComponent>();

	private bool m_enabledInternally;

	private bool m_willTickWhileInactive;

	private bool m_changingStatesInternally;

	private Map<GameObject, int> m_prevLayersByObject;

	private HashSet<int> m_dataModelIdsBound = new HashSet<int>();

	private GameObject m_popupRootBoneParentGameObject;

	private bool m_hasPopupRootParent;

	private GameObject m_parentGameObject;

	private static ProfilerMarker s_tickChildren = new ProfilerMarker("WidgetTemplate.TickChildren");

	private static ProfilerMarker s_tickBehaviours = new ProfilerMarker("WidgetTemplate.TickBehaviours");

	private static ProfilerMarker s_handleComponentsReady = new ProfilerMarker("WidgetTemplate.HandleComponentsReady");

	[HideInInspector]
	[SerializeField]
	private List<int> m_dataModelHints_editorOnly;

	private static Pool<List<Component>> s_componentListPool = new Pool<List<Component>>((int _) => new List<Component>(), delegate
	{
	}, 1);

	private Map<Renderer, int> m_originalRendererLayers;

	private static ProfilerMarker s_getParentForDataModelMarker = new ProfilerMarker("WidgetTemplate.GetParentForDataModel");

	public List<int> DataModelHints_EditorOnly => m_dataModelHints_editorOnly;

	public bool WillTickWhileInactive => m_willTickWhileInactive;

	public bool WaitForParentToShow
	{
		get
		{
			if (m_waitForParentToShow && !m_willTickWhileInactive && m_parentWidgetTemplate != null)
			{
				return m_parentWidgetTemplate.GetInitializationState() != InitializationState.Done;
			}
			return false;
		}
		set
		{
			m_waitForParentToShow = value;
		}
	}

	public WidgetTemplate ParentWidgetTemplate
	{
		get
		{
			return m_parentWidgetTemplate;
		}
		set
		{
			m_parentWidgetTemplate = value;
		}
	}

	public DataContext DataContext => m_dataContext;

	public int DataVersion { get; set; } = 1;

	public override bool IsInitialized => m_initializationState == InitializationState.Done;

	public override InitializationState InitState => m_initializationState;

	public override bool IsChangingStates
	{
		get
		{
			if (!base.IsActive)
			{
				return false;
			}
			if (m_initializationState <= InitializationState.InitializingWidget)
			{
				return true;
			}
			if (m_visualControllers != null)
			{
				foreach (VisualController visualController in m_visualControllers)
				{
					if (visualController.IsChangingStates)
					{
						return true;
					}
				}
			}
			if (m_componentsChangingStates != null)
			{
				foreach (IStatefulWidgetComponent statefulWidget in m_componentsChangingStates)
				{
					if (statefulWidget != this && statefulWidget.IsChangingStates)
					{
						return true;
					}
				}
			}
			return false;
		}
	}

	public override bool HasPendingActions
	{
		get
		{
			if (!base.IsActive)
			{
				return false;
			}
			if (m_visualControllers != null)
			{
				foreach (VisualController visualController in m_visualControllers)
				{
					if (visualController.HasPendingActions)
					{
						return true;
					}
				}
			}
			if (m_nestedInstances != null)
			{
				foreach (WidgetInstance nestedInstance in m_nestedInstances)
				{
					if (nestedInstance.HasPendingActions)
					{
						return true;
					}
				}
			}
			return false;
		}
	}

	public bool IsDesiredHiddenInHierarchy
	{
		get
		{
			if (IsDesiredHidden)
			{
				return true;
			}
			WidgetTemplate tpl = ParentWidgetTemplate;
			while (tpl != null)
			{
				if (tpl.IsDesiredHidden)
				{
					return true;
				}
				tpl = tpl.ParentWidgetTemplate;
			}
			return false;
		}
	}

	public bool IsDesiredHidden { get; private set; }

	private bool ListeningToNestedStateChanges
	{
		get
		{
			if (m_visualControllers == null || m_visualControllers.Count <= 0)
			{
				if (m_nestedInstances != null)
				{
					return m_nestedInstances.Count > 0;
				}
				return false;
			}
			return true;
		}
	}

	protected bool CanSendStateChanges
	{
		get
		{
			if (!m_enabledInternally)
			{
				return m_willTickWhileInactive;
			}
			return true;
		}
	}

	public WidgetTemplate OwningWidget => this;

	private event EventListenerDelegate m_eventListeners;

	public InitializationState GetInitializationState()
	{
		return m_initializationState;
	}

	public override bool GetIsChangingStates(Func<GameObject, bool> includeGameObject)
	{
		if (!base.IsActive)
		{
			return false;
		}
		if (!includeGameObject(base.gameObject))
		{
			return false;
		}
		if (m_initializationState <= InitializationState.InitializingWidget)
		{
			return true;
		}
		if (m_visualControllers != null)
		{
			foreach (VisualController visualController in m_visualControllers)
			{
				if (visualController.IsChangingStates)
				{
					return true;
				}
			}
		}
		if (m_componentsChangingStates != null)
		{
			foreach (IStatefulWidgetComponent componentsChangingState in m_componentsChangingStates)
			{
				if (componentsChangingState.GetIsChangingStates(includeGameObject))
				{
					return true;
				}
			}
		}
		return false;
	}

	public void OnInstantiated()
	{
		if (Application.IsPlaying(this))
		{
			ShowOrHide(show: false, recursive: false);
		}
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		m_enabledInternally = true;
		if (m_changingStatesInternally && !m_willTickWhileInactive && !m_startChangingStatesEvent.IsSet)
		{
			m_doneChangingStatesEvent.Clear();
			m_startChangingStatesEvent.SetAndDispatch();
		}
	}

	protected override void OnDisable()
	{
		m_enabledInternally = false;
		base.OnDisable();
		if (m_changingStatesInternally && !m_willTickWhileInactive && m_startChangingStatesEvent.IsSet)
		{
			m_startChangingStatesEvent.Clear();
			m_doneChangingStatesEvent.SetAndDispatch();
		}
	}

	private void Start()
	{
		Initialize();
		WidgetRunner.s_inactiveTemplates.Remove(this);
	}

	public void CleanDataModels()
	{
		GlobalDataContext.Get().RemoveChangedListener(HandleGlobalDataChanged);
		foreach (int model in m_dataModelIdsBound)
		{
			UnbindDataModel(model);
		}
		foreach (KeyValuePair<GameObject, GameObjectBinding> item in m_gameObjectsToBindingsMap)
		{
			item.Value.Deinitialize();
		}
	}

	public void Deinitialize()
	{
		if (m_dataContext != null)
		{
			m_dataContext.RemoveChangedListener(HandleDataChanged);
		}
		CleanDataModels();
		if (m_widgetBehaviors != null)
		{
			foreach (WidgetBehavior widgetBehavior in m_widgetBehaviors)
			{
				widgetBehavior.RemoveStartChangingStatesListener(HandleStartChangingStates);
				widgetBehavior.RemoveDoneChangingStatesListener(HandleDoneChangingStates);
			}
		}
		if (m_nestedInstances != null)
		{
			foreach (WidgetInstance nestedInstance in m_nestedInstances)
			{
				nestedInstance.ParentWidgetTemplate = this;
				nestedInstance.RemoveStartChangingStatesListener(HandleStartChangingStates);
				nestedInstance.RemoveDoneChangingStatesListener(HandleDoneChangingStates);
			}
		}
		if (ServiceManager.Get<WidgetRunner>() != null)
		{
			ServiceManager.Get<WidgetRunner>().UnregisterWidget(this);
		}
	}

	private void OnDestroy()
	{
		Deinitialize();
	}

	private void PreInitialize()
	{
		if (TryGetComponent<PopupRoot>(out var popupRoot))
		{
			SetPopupRoot(popupRoot);
		}
		Transform parent = base.transform.parent;
		if (parent != null)
		{
			m_parentGameObject = parent.gameObject;
		}
		if (m_dataContext != null)
		{
			m_dataContext.RegisterChangedListener(HandleDataChanged);
			GlobalDataContext.Get().RegisterChangedListener(HandleGlobalDataChanged);
		}
		if (m_widgetBehaviors != null)
		{
			foreach (WidgetBehavior widgetBehavior in m_widgetBehaviors)
			{
				widgetBehavior.RegisterStartChangingStatesListener(HandleStartChangingStates, widgetBehavior);
				widgetBehavior.RegisterDoneChangingStatesListener(HandleDoneChangingStates, widgetBehavior, callImmediatelyIfSet: false);
				widgetBehavior.PreInitialize(this);
			}
		}
		if (m_nestedInstances == null)
		{
			return;
		}
		foreach (WidgetInstance nestedInstance in m_nestedInstances)
		{
			nestedInstance.ParentWidgetTemplate = this;
			nestedInstance.RegisterStartChangingStatesListener(HandleStartChangingStates, nestedInstance);
			nestedInstance.RegisterDoneChangingStatesListener(HandleDoneChangingStates, nestedInstance, callImmediatelyIfSet: false);
			nestedInstance.PreInitialize();
		}
	}

	public void Initialize(bool shouldPreload = false)
	{
		if (m_initializationState == InitializationState.NotStarted)
		{
			m_willTickWhileInactive = shouldPreload;
			m_initializationState = InitializationState.InitializingWidget;
			WidgetRunner.s_inactiveTemplates.Add(this);
			PreInitialize();
			HandleStartChangingStates(this);
			ServiceManager.InitializeDynamicServicesIfEditor(out var dependencies, typeof(IAssetLoader), typeof(WidgetRunner));
			Processor.QueueJob(HearthstoneJobs.CreateJobFromAction("WidgetTemplate.Initialize", InitializeInternal, JobFlags.StartImmediately, dependencies));
		}
	}

	private void InitializeInternal()
	{
		if (m_parentWidgetTemplate == null)
		{
			ServiceManager.Get<WidgetRunner>().RegisterWidget(this);
		}
		m_initializationStartTime = Time.realtimeSinceStartup;
		List<IAsyncInitializationBehavior> componentsPendingInitialization = new List<IAsyncInitializationBehavior>();
		if (m_pairs != null)
		{
			foreach (KeyValuePair pair in m_pairs)
			{
				if (pair.Value is IAsyncInitializationBehavior asyncBehavior && asyncBehavior.Container != this && !asyncBehavior.IsReady && asyncBehavior.IsActive)
				{
					componentsPendingInitialization.Add(asyncBehavior);
				}
			}
		}
		if (m_nestedInstances != null)
		{
			foreach (WidgetInstance nestedInstance in m_nestedInstances)
			{
				nestedInstance.WillLoadSynchronously = nestedInstance.WillLoadSynchronously || base.WillLoadSynchronously;
				if (Application.IsPlaying(this) && nestedInstance.IsActive)
				{
					nestedInstance.DeferredWidgetBehaviorInitialization = true;
				}
			}
		}
		m_numComponentsPendingInitialization = componentsPendingInitialization.Count;
		if (m_numComponentsPendingInitialization > 0)
		{
			foreach (IAsyncInitializationBehavior asyncBehavior2 in componentsPendingInitialization)
			{
				asyncBehavior2.RegisterActivatedListener(HandleComponentActivated, asyncBehavior2);
				asyncBehavior2.RegisterDeactivatedListener(HandleComponentDeactivated, asyncBehavior2);
				asyncBehavior2.RegisterReadyListener(HandleComponentReady, asyncBehavior2);
			}
			return;
		}
		HandleAllAsyncBehaviorsReady();
	}

	private void HandleAllAsyncBehaviorsReady()
	{
		TriggerOnReady();
		if (!DeferredWidgetBehaviorInitialization)
		{
			InitializeWidgetBehaviors();
		}
	}

	public void ResetUpdateTargets()
	{
		m_updateTargets = UpdateTargets.All;
		if (m_nestedInstances == null)
		{
			return;
		}
		foreach (WidgetInstance instance in m_nestedInstances)
		{
			if (instance != null && instance.Widget != null)
			{
				instance.Widget.ResetUpdateTargets();
			}
		}
	}

	private void AddUpdateTarget(UpdateTargets flag)
	{
		m_updateTargets |= flag;
	}

	private void ClearUpdateTarget(UpdateTargets flag)
	{
		m_updateTargets &= ~flag;
	}

	private bool ShouldUpdateTarget(UpdateTargets flag)
	{
		return (m_updateTargets & flag) != 0;
	}

	public void Tick()
	{
		if (m_nestedInstances != null && ShouldUpdateTarget(UpdateTargets.Children))
		{
			ClearUpdateTarget(UpdateTargets.Children);
			if (m_widgetsToTickThisIteration == null)
			{
				m_widgetsToTickThisIteration = new HashSet<WidgetTemplate>();
			}
			foreach (WidgetInstance instance in m_nestedInstances)
			{
				if (instance != null && instance.Widget != null)
				{
					m_widgetsToTickThisIteration.Add(instance.Widget);
				}
			}
		}
		if (m_widgetsPendingTick != null)
		{
			if (m_widgetsToTickThisIteration == null)
			{
				m_widgetsToTickThisIteration = new HashSet<WidgetTemplate>();
			}
			foreach (WidgetTemplate widget in m_widgetsPendingTick)
			{
				if (widget != null)
				{
					m_widgetsToTickThisIteration.Add(widget);
				}
			}
			m_widgetsPendingTick.Clear();
		}
		if (m_widgetsToTickThisIteration != null)
		{
			foreach (WidgetTemplate widget2 in m_widgetsToTickThisIteration)
			{
				if (widget2 != null)
				{
					widget2.Tick();
				}
			}
		}
		if (ShouldUpdateTarget(UpdateTargets.Behaviors) && m_widgetBehaviors != null)
		{
			ClearUpdateTarget(UpdateTargets.Behaviors);
			foreach (WidgetBehavior behavior in m_widgetBehaviors)
			{
				if (behavior != null && behavior.CanTick)
				{
					behavior.OnUpdate();
				}
			}
		}
		if (m_deactivatedComponents != null && m_deactivatedComponents.Count > 0 && m_initializationState <= InitializationState.InitializingWidget)
		{
			for (int i = m_deactivatedComponents.Count - 1; i >= 0; i--)
			{
				IAsyncInitializationBehavior asyncBehavior = m_deactivatedComponents[i];
				HandleComponentReady(asyncBehavior);
				m_deactivatedComponents.RemoveAt(i);
			}
		}
		CheckIfDoneChangingStates();
	}

	private void CheckIfDoneChangingStates()
	{
		for (int i = m_componentsChangingStates.Count - 1; i >= 0; i--)
		{
			IStatefulWidgetComponent statefulWidgetComponent = m_componentsChangingStates[i];
			if (!statefulWidgetComponent.IsChangingStates)
			{
				Log.UIFramework.PrintWarning("WidgetTemplate " + Widget.GetObjectDebugName(this) + " did not receive HandleDoneChangingStates from " + Widget.GetObjectDebugName(statefulWidgetComponent));
				HandleDoneChangingStates(statefulWidgetComponent);
			}
		}
		if (m_componentsChangingStates.Count != 0)
		{
			return;
		}
		if (m_initializationState == InitializationState.InitializingWidgetBehaviors && !WaitForParentToShow)
		{
			FinalizeInitialization(!IsDesiredHiddenInHierarchy);
		}
		if (m_changingStatesInternally && (m_widgetsPendingTick == null || m_widgetsPendingTick.Count == 0))
		{
			m_changingStatesInternally = false;
			if (CanSendStateChanges && m_startChangingStatesEvent.IsSet)
			{
				m_startChangingStatesEvent.Clear();
				m_doneChangingStatesEvent.SetAndDispatch();
			}
		}
	}

	private void RegisterChildPendingTick(WidgetTemplate nestedWidget)
	{
		if (m_widgetsPendingTick == null)
		{
			m_widgetsPendingTick = new HashSet<WidgetTemplate>();
		}
		m_widgetsPendingTick.Add(nestedWidget);
		if (m_parentWidgetTemplate == null)
		{
			ServiceManager.Get<WidgetRunner>()?.AddWidgetPendingTick(this);
		}
		else
		{
			m_parentWidgetTemplate.RegisterChildPendingTick(this);
		}
	}

	public void InitializeWidgetBehaviors()
	{
		if (m_initializationState != InitializationState.InitializingWidget)
		{
			return;
		}
		DeferredWidgetBehaviorInitialization = false;
		m_initializationState = InitializationState.InitializingWidgetBehaviors;
		if (m_nestedInstances != null)
		{
			foreach (WidgetInstance nestedInstance in m_nestedInstances)
			{
				if (nestedInstance != null)
				{
					nestedInstance.InitializeWidgetBehaviors();
				}
			}
		}
		if (m_widgetBehaviors != null)
		{
			foreach (WidgetBehavior widgetBehavior in m_widgetBehaviors)
			{
				if (widgetBehavior != null)
				{
					widgetBehavior.Initialize();
					if (widgetBehavior.CanTick)
					{
						widgetBehavior.OnUpdate();
					}
				}
			}
		}
		HandleDoneChangingStates(this);
		CheckIfDoneChangingStates();
	}

	public override void SetLayerOverride(GameLayer layerOverride)
	{
		if (layerOverride >= GameLayer.Default)
		{
			if (m_prevLayersByObject == null)
			{
				m_prevLayersByObject = new Map<GameObject, int>();
			}
			SetLayerOverrideForObject(layerOverride, base.gameObject, m_prevLayersByObject);
		}
	}

	public void SetLayerOverrideForObject(GameLayer layerOverride, GameObject go, Map<GameObject, int> originalLayers = null)
	{
		if (layerOverride < GameLayer.Default)
		{
			return;
		}
		GameObjectUtils.WalkSelfAndChildren(go.transform, delegate(Transform child)
		{
			bool result = true;
			ILayerOverridable[] components = child.GetComponents<ILayerOverridable>();
			if (components != null && components.Length != 0)
			{
				ILayerOverridable[] array = components;
				foreach (ILayerOverridable layerOverridable in array)
				{
					if (layerOverridable != this)
					{
						layerOverridable.SetLayerOverride(layerOverride);
						if (layerOverridable.HandlesChildLayers)
						{
							result = false;
							break;
						}
					}
				}
			}
			child.gameObject.layer = (int)layerOverride;
			if (originalLayers != null)
			{
				originalLayers[child.gameObject] = child.gameObject.layer;
			}
			return result;
		});
	}

	public override void ClearLayerOverride()
	{
		if (m_prevLayersByObject == null || !m_prevLayersByObject.ContainsKey(base.gameObject))
		{
			return;
		}
		int originalRootLayer = m_prevLayersByObject[base.gameObject];
		GameObjectUtils.WalkSelfAndChildren(base.transform, delegate(Transform child)
		{
			bool result = true;
			ILayerOverridable[] components = child.GetComponents<ILayerOverridable>();
			if (components != null && components.Length != 0)
			{
				ILayerOverridable[] array = components;
				foreach (ILayerOverridable obj in array)
				{
					obj.ClearLayerOverride();
					if (obj.HandlesChildLayers)
					{
						result = false;
						break;
					}
				}
			}
			if (m_prevLayersByObject.TryGetValue(child.gameObject, out var value))
			{
				child.gameObject.layer = value;
			}
			else
			{
				child.gameObject.layer = originalRootLayer;
				Log.UIFramework.PrintWarning("Couldn't find original layer for GameObject " + $"{child.name} ({child.gameObject.GetInstanceID()}) so setting it to widget owner's layer.");
			}
			return result;
		});
		m_prevLayersByObject = null;
	}

	private void HandleComponentActivated(object payload)
	{
		if (payload == null)
		{
			Log.UIFramework.PrintError($"WidgetTemplate {base.name} ({GetInstanceID()}) attempted to handle activated async component but it was null!");
			return;
		}
		IAsyncInitializationBehavior asyncBehavior = (IAsyncInitializationBehavior)payload;
		if (m_deactivatedComponents != null && m_deactivatedComponents.Contains(payload) && m_initializationState == InitializationState.InitializingWidget)
		{
			Widget widget = asyncBehavior as Widget;
			if (widget != null)
			{
				widget.DeferredWidgetBehaviorInitialization = true;
			}
			asyncBehavior.RegisterReadyListener(HandleComponentReady, payload);
			m_deactivatedComponents.Remove(asyncBehavior);
		}
	}

	private void HandleComponentDeactivated(object payload)
	{
		if (m_initializationState != InitializationState.InitializingWidget)
		{
			return;
		}
		if (payload == null)
		{
			Log.UIFramework.PrintError($"WidgetTemplate {base.name} ({GetInstanceID()}) attempted to handle deactivated async component but it was null!");
			return;
		}
		IAsyncInitializationBehavior asyncBehavior = (IAsyncInitializationBehavior)payload;
		if (m_deactivatedComponents == null)
		{
			m_deactivatedComponents = new List<IAsyncInitializationBehavior>();
		}
		Widget widget = asyncBehavior as Widget;
		if (widget != null)
		{
			widget.DeferredWidgetBehaviorInitialization = false;
		}
		if (!asyncBehavior.IsReady && !m_deactivatedComponents.Contains(asyncBehavior))
		{
			m_deactivatedComponents.Add(asyncBehavior);
			asyncBehavior.RemoveReadyListener(HandleComponentReady);
		}
	}

	private void HandleComponentReady(object asyncBehavior)
	{
		if (m_numComponentsPendingInitialization > 0)
		{
			m_numComponentsPendingInitialization--;
			if (m_numComponentsPendingInitialization == 0)
			{
				HandleAllAsyncBehaviorsReady();
			}
		}
	}

	private void HandleStartChangingStates(object context)
	{
		if (!(context is IStatefulWidgetComponent comp))
		{
			Log.UIFramework.PrintWarning("WidgetTemplate " + base.gameObject.name + " received HandleStartChangingStates with invalid context");
			return;
		}
		if (!m_componentsChangingStates.Contains(comp))
		{
			m_componentsChangingStates.Add(comp);
		}
		else
		{
			Log.UIFramework.PrintWarning("WidgetTemplate " + Widget.GetObjectDebugName(this) + " received HandleStartChangingStates more than once without a HandleDoneChangingStates for " + Widget.GetObjectDebugName(comp));
		}
		if (!m_changingStatesInternally)
		{
			m_changingStatesInternally = true;
			if (CanSendStateChanges && !m_startChangingStatesEvent.IsSet)
			{
				m_doneChangingStatesEvent.Clear();
				m_startChangingStatesEvent.SetAndDispatch();
			}
		}
	}

	private void HandleDoneChangingStates(object context)
	{
		if (!(context is IStatefulWidgetComponent comp))
		{
			Log.UIFramework.PrintWarning("WidgetTemplate " + Widget.GetObjectDebugName(this) + " received HandleDoneChangingStates with invalid context");
		}
		else if (!m_componentsChangingStates.Remove(comp))
		{
			Log.UIFramework.PrintWarning("WidgetTemplate " + Widget.GetObjectDebugName(this) + " received HandleDoneChangingStates without HandleStartChangingStates for " + Widget.GetObjectDebugName(context));
		}
	}

	private void FinalizeInitialization(bool tryShow)
	{
		if (tryShow && !IsDesiredHidden)
		{
			ShowOrHide(show: true, recursive: false);
		}
		if (m_nestedInstances != null)
		{
			foreach (WidgetInstance instance in m_nestedInstances)
			{
				if (!(instance.Widget != null) || !instance.Widget.WaitForParentToShow || !instance.Widget.IsActive || instance.WillPreload)
				{
					continue;
				}
				if (instance.Widget.m_initializationState != InitializationState.InitializingWidgetBehaviors)
				{
					if (Application.IsPlaying(this))
					{
						Log.UIFramework.PrintError("WidgetTemplate " + Widget.GetObjectDebugName(this) + " attempted to finalize and show child widget " + Widget.GetObjectDebugName(instance.Widget) + ", but child was not done with its init!");
					}
				}
				else
				{
					instance.Widget.FinalizeInitialization(tryShow && !IsDesiredHidden);
				}
			}
		}
		m_willTickWhileInactive = false;
		m_initializationState = InitializationState.Done;
	}

	public DataContext GetDataContextForGameObject(GameObject go)
	{
		if (m_gameObjectsToBindingsMap.TryGetValue(go, out var binding))
		{
			return binding.DataContext;
		}
		return m_dataContext;
	}

	private GameObjectBinding CreateCustomDataContextForGameObject(GameObject go, DataContext original)
	{
		DataContext customDataContext = new DataContext();
		GameObjectBinding binding = new GameObjectBinding(customDataContext);
		m_gameObjectsToBindingsMap[go] = binding;
		if (original != null && original != m_dataContext)
		{
			foreach (IDataModel model in original.GetDataModels())
			{
				customDataContext.BindDataModel(model);
				m_dataModelIdsBound.Add(model.DataModelId);
			}
		}
		return binding;
	}

	public WidgetEventListenerResponse EventReceived(string eventName, TriggerEventParameters parameters)
	{
		if (this.m_eventListeners != null)
		{
			this.m_eventListeners(eventName);
		}
		return default(WidgetEventListenerResponse);
	}

	public override void RegisterEventListener(EventListenerDelegate listener)
	{
		m_eventListeners -= listener;
		m_eventListeners += listener;
	}

	public override void RemoveEventListener(EventListenerDelegate listener)
	{
		m_eventListeners -= listener;
	}

	public override void Show()
	{
		IsDesiredHidden = false;
		if (m_initializationState == InitializationState.Done)
		{
			ShowOrHide(show: true, recursive: true);
		}
	}

	public override void Hide()
	{
		IsDesiredHidden = true;
		if (m_initializationState == InitializationState.Done)
		{
			ShowOrHide(show: false, recursive: true);
		}
	}

	private void ShowOrHide(bool show, bool recursive)
	{
		GameObjectUtils.WalkSelfAndChildren(base.transform, delegate(Transform current)
		{
			bool flag = false;
			List<Component> list = s_componentListPool.Acquire();
			current.GetComponents(list);
			WidgetTemplate widgetTemplate = null;
			int i = 0;
			for (int count = list.Count; i < count; i++)
			{
				Component component = list[i];
				if (!(component is Renderer renderer))
				{
					if (!(component is PegUIElement pegUIElement))
					{
						if (!(component is UberText uberText))
						{
							if (!(component is IVisibleWidgetComponent visibleWidgetComponent))
							{
								if (component is WidgetInstance widgetInstance)
								{
									if (recursive)
									{
										WidgetTemplate widget = widgetInstance.Widget;
										if (widget != null && !widget.IsDesiredHidden && widget.GetInitializationState() == InitializationState.Done)
										{
											widgetTemplate = widget;
										}
									}
									flag = true;
								}
							}
							else
							{
								visibleWidgetComponent.SetVisibility(show && !visibleWidgetComponent.IsDesiredHidden, isInternal: true);
								flag = flag || visibleWidgetComponent.HandlesChildVisibility;
							}
						}
						else
						{
							if (show)
							{
								uberText.Show();
							}
							else
							{
								uberText.Hide();
							}
							flag = true;
						}
					}
					else if (m_initializationStartTime > pegUIElement.SetEnabledLastCallTime || m_initializationState == InitializationState.Done)
					{
						pegUIElement.SetEnabled(show, isInternal: true);
					}
				}
				else
				{
					RenderUtils.SetInvisibleRenderer(renderer, show, ref m_originalRendererLayers);
				}
			}
			list.Clear();
			s_componentListPool.Release(list);
			if (widgetTemplate != null)
			{
				widgetTemplate.ShowOrHide(show, recursive: true);
			}
			return !flag;
		});
	}

	public override void UnbindDataModel(int id)
	{
		if (m_dataContext != null)
		{
			m_dataContext.UnbindDataModel(id);
		}
		foreach (KeyValuePair<GameObject, GameObjectBinding> item in m_gameObjectsToBindingsMap)
		{
			item.Value.UnbindDataModel(id);
		}
		if (m_nestedInstances == null)
		{
			return;
		}
		foreach (WidgetInstance nestedInstance in m_nestedInstances)
		{
			nestedInstance.UnbindDataModel(id);
		}
	}

	public void UnbindDataModel(int id, GameObject target)
	{
		if (target == null)
		{
			return;
		}
		if (target == base.gameObject)
		{
			UnbindDataModel(id);
			return;
		}
		GameObjectUtils.WalkSelfAndChildren(target.transform, delegate(Transform current)
		{
			bool result = true;
			if (m_gameObjectsToBindingsMap.TryGetValue(current.gameObject, out var value))
			{
				value.UnbindDataModel(id);
			}
			WidgetInstance component = current.GetComponent<WidgetInstance>();
			if (component != null)
			{
				result = false;
				component.UnbindDataModel(id);
			}
			return result;
		});
	}

	public override void BindDataModel(IDataModel dataModel, bool overrideChildren = false)
	{
		if (dataModel == null)
		{
			Log.UIFramework.PrintError("Attempted to bind a null datamodel on {0}", base.gameObject.name);
			return;
		}
		if (m_dataContext == null)
		{
			m_dataContext = new DataContext();
			m_dataContext.RegisterChangedListener(HandleDataChanged);
		}
		m_dataContext.BindDataModel(dataModel);
		m_dataModelIdsBound.Add(dataModel.DataModelId);
		if (!overrideChildren)
		{
			return;
		}
		if (m_nestedInstances != null)
		{
			foreach (WidgetInstance widgetInstance in m_nestedInstances)
			{
				if (widgetInstance == null)
				{
					Log.UIFramework.PrintError("Attempted to unbind a data model from a null widget instance. Parent: {0}", base.gameObject.name);
				}
				else
				{
					widgetInstance.UnbindDataModel(dataModel.DataModelId);
				}
			}
		}
		foreach (KeyValuePair<GameObject, GameObjectBinding> kv in m_gameObjectsToBindingsMap)
		{
			if (kv.Value == null)
			{
				Log.UIFramework.PrintError("Attempted to unbind a data model from a null binding map object. Parent: {0}", base.gameObject.name);
			}
			else
			{
				kv.Value.UnbindDataModel(dataModel.DataModelId);
			}
		}
	}

	public override bool BindDataModel(IDataModel dataModel, string targetName, bool propagateToChildren = true, bool overrideChildren = false)
	{
		if (dataModel == null)
		{
			Log.UIFramework.PrintError("Attempted to bind a null datamodel on {0}", base.gameObject.name);
			return false;
		}
		if (m_dataContext == null)
		{
			m_dataContext = new DataContext();
			m_dataContext.RegisterChangedListener(HandleDataChanged);
		}
		GameObject target = null;
		foreach (KeyValuePair kv in m_pairs)
		{
			if (kv.Value != null && kv.Value.name == targetName)
			{
				target = kv.Value.gameObject;
				break;
			}
		}
		return BindDataModel(dataModel, target, propagateToChildren, overrideChildren);
	}

	public bool BindDataModel(IDataModel dataModel, GameObject target, bool propagateToChildren = true, bool overrideChildren = false)
	{
		if (dataModel == null)
		{
			Log.UIFramework.PrintError("Attempted to bind a null datamodel on {0}", base.gameObject.name);
			return false;
		}
		if (target == null)
		{
			return false;
		}
		if (m_componentsSet == null)
		{
			m_componentsSet = new HashSet<Component>();
			foreach (KeyValuePair kv in m_pairs)
			{
				m_componentsSet.Add(kv.Value);
			}
			if (m_addedInstances != null)
			{
				foreach (WidgetInstance addedInstance in m_addedInstances)
				{
					m_componentsSet.Add(addedInstance.transform);
				}
			}
		}
		if (!m_componentsSet.Contains(target.transform))
		{
			Hearthstone.UI.Logging.Log.Get().AddMessage("Tried binding a data model to a game object that does not belong to this template", base.gameObject, Hearthstone.UI.Logging.LogLevel.Error);
			return false;
		}
		if (target == base.gameObject)
		{
			BindDataModel(dataModel, overrideChildren);
			return true;
		}
		BindDataModel_Recursive(dataModel, target, propagateToChildren, overrideChildren, target: true, m_dataContext);
		ProcessNewGameObjectBinds();
		return true;
	}

	private void BindDataModel_Recursive(IDataModel dataModel, GameObject current, bool propagateToChildren, bool overrideChildren, bool target, DataContext parentDataContext)
	{
		if (propagateToChildren)
		{
			WidgetInstance widgetInstance = current.GetComponent<WidgetInstance>();
			if (widgetInstance != null)
			{
				if (overrideChildren)
				{
					widgetInstance.UnbindDataModel(dataModel.DataModelId);
				}
				propagateToChildren = false;
			}
		}
		int lastDataVersion = 0;
		if (!m_gameObjectsToBindingsMap.TryGetValue(current, out var binding))
		{
			binding = CreateCustomDataContextForGameObject(current, parentDataContext);
		}
		else
		{
			lastDataVersion = binding.DataContext.GetLocalDataVersion();
		}
		IDataModel existingDataModel;
		if (target)
		{
			binding.BindDataModel(dataModel, owned: true);
			m_dataModelIdsBound.Add(dataModel.DataModelId);
		}
		else if (overrideChildren || !binding.DataContext.GetDataModel(dataModel.DataModelId, out existingDataModel) || !binding.OwnsDataModel(dataModel))
		{
			binding.BindDataModel(dataModel, owned: false);
			m_dataModelIdsBound.Add(dataModel.DataModelId);
		}
		if (lastDataVersion != binding.DataContext.GetLocalDataVersion())
		{
			if (m_newlyBoundGameObjects == null)
			{
				m_newlyBoundGameObjects = new List<GameObject>();
			}
			m_newlyBoundGameObjects.Add(current);
		}
		if (!propagateToChildren)
		{
			return;
		}
		Transform targetTransform = current.transform;
		for (int i = 0; i < targetTransform.childCount; i++)
		{
			Transform child = targetTransform.GetChild(i);
			if (m_componentsSet.Contains(child))
			{
				BindDataModel_Recursive(dataModel, child.gameObject, propagateToChildren: true, overrideChildren, target: false, binding.DataContext);
			}
		}
	}

	public override bool GetDataModel(int id, out IDataModel dataModel)
	{
		if (m_dataContext.GetDataModel(id, out dataModel))
		{
			return true;
		}
		GameObject parent = GetParentForDataModel();
		if (ParentWidgetTemplate != null && parent != null)
		{
			return ParentWidgetTemplate.GetDataModel(id, parent, out dataModel);
		}
		return false;
	}

	private GameObject GetParentForDataModel()
	{
		using (s_getParentForDataModelMarker.Auto())
		{
			if (m_hasPopupRootParent)
			{
				return m_popupRootBoneParentGameObject;
			}
			return m_parentGameObject;
		}
	}

	public override bool GetDataModel(int id, string targetName, out IDataModel model)
	{
		GameObject target = null;
		foreach (KeyValuePair kv in m_pairs)
		{
			if (kv.Value != null && kv.Value.name == targetName)
			{
				target = kv.Value.gameObject;
				break;
			}
		}
		if (target == null)
		{
			model = null;
			return false;
		}
		return GetDataModel(id, target, out model);
	}

	public bool GetDataModel(int id, GameObject target, out IDataModel model)
	{
		DataContext dataContext = GetDataContextForGameObject(target);
		if (dataContext != null && dataContext.GetDataModel(id, out model))
		{
			return true;
		}
		return GetDataModel(id, out model);
	}

	public ICollection<IDataModel> GetDataModels()
	{
		return m_dataContext.GetDataModels();
	}

	private void TryHandleDataChanged(IDataModel dataModel, DataChangeSource changeType)
	{
		if (changeType == DataChangeSource.Parent && m_dataContext.HasDataModel(dataModel.DataModelId))
		{
			return;
		}
		bool hasChange = false;
		if (m_widgetBehaviors != null)
		{
			foreach (WidgetBehavior widgetBehavior in m_widgetBehaviors)
			{
				if (!(widgetBehavior == null))
				{
					bool hasBinding = HasGameObjectBinding(widgetBehavior.gameObject, dataModel.DataModelId);
					if ((changeType != DataChangeSource.GameObjectBinding || hasBinding) && !(changeType != DataChangeSource.GameObjectBinding && hasBinding) && widgetBehavior.TryIncrementDataVersion(dataModel.DataModelId))
					{
						hasChange = true;
					}
				}
			}
		}
		if (hasChange)
		{
			int dataVersion = DataVersion + 1;
			DataVersion = dataVersion;
			if (m_parentWidgetTemplate == null)
			{
				ServiceManager.Get<WidgetRunner>()?.AddWidgetPendingTick(this);
			}
			else
			{
				m_parentWidgetTemplate.RegisterChildPendingTick(this);
				AddUpdateTarget(UpdateTargets.Behaviors);
			}
		}
		if (changeType == DataChangeSource.Global || m_nestedInstances == null)
		{
			return;
		}
		foreach (WidgetInstance nestedInstance in m_nestedInstances)
		{
			if (!(nestedInstance == null))
			{
				bool hasBinding2 = HasGameObjectBinding(nestedInstance.gameObject, dataModel.DataModelId);
				if ((changeType != DataChangeSource.GameObjectBinding || hasBinding2) && !(changeType != DataChangeSource.GameObjectBinding && hasBinding2) && nestedInstance.Widget != null)
				{
					nestedInstance.Widget.TryHandleDataChanged(dataModel, DataChangeSource.Parent);
				}
			}
		}
	}

	private void HandleDataChanged(IDataModel dataModel)
	{
		TryHandleDataChanged(dataModel, DataChangeSource.Template);
	}

	private void HandleBindingDataChanged(IDataModel dataModel, GameObjectBinding binding)
	{
		if (binding.OwnsDataModel(dataModel) && binding.DataContext.HasDataModelInstance(dataModel))
		{
			TryHandleDataChanged(dataModel, DataChangeSource.GameObjectBinding);
		}
	}

	private void HandleGlobalDataChanged(IDataModel dataModel)
	{
		TryHandleDataChanged(dataModel, DataChangeSource.Global);
	}

	private void ProcessNewGameObjectBinds()
	{
		if (m_newlyBoundGameObjects != null)
		{
			for (int i = 0; i < m_newlyBoundGameObjects.Count; i++)
			{
				ProcessNewGameObjectBinding(m_newlyBoundGameObjects[i]);
			}
			m_newlyBoundGameObjects.Clear();
		}
	}

	private void ProcessNewGameObjectBinding(GameObject go)
	{
		GameObjectBinding binding = m_gameObjectsToBindingsMap[go];
		foreach (IDataModel model in binding.DataContext.GetDataModels())
		{
			HandleBindingDataChanged(model, binding);
		}
		binding.RegisterChangedListener(HandleBindingDataChanged);
	}

	private bool HasGameObjectBinding(GameObject go, int id)
	{
		GameObjectBinding binding = null;
		m_gameObjectsToBindingsMap.TryGetValue(go, out binding);
		return binding?.DataContext.HasDataModel(id) ?? false;
	}

	public bool OwnsDataModelInstance(IDataModel dataModel, GameObject target = null)
	{
		if (target == null)
		{
			return m_dataContext.HasDataModelInstance(dataModel);
		}
		if (!m_gameObjectsToBindingsMap.TryGetValue(target, out var binding))
		{
			return false;
		}
		if (binding.HasDataModelInstance(dataModel))
		{
			return binding.OwnsDataModel(dataModel);
		}
		return false;
	}

	public bool TryFindLocalDataModelOwner(IDataModel dataModel, out GameObject owner)
	{
		owner = null;
		if (m_dataContext.HasDataModelInstance(dataModel))
		{
			owner = base.gameObject;
			return true;
		}
		foreach (KeyValuePair<GameObject, GameObjectBinding> kvp in m_gameObjectsToBindingsMap)
		{
			GameObjectBinding binding = kvp.Value;
			if (binding.HasDataModelInstance(dataModel) && binding.OwnsDataModel(dataModel))
			{
				owner = kvp.Key;
				return true;
			}
		}
		return false;
	}

	public override Widget FindWidget(string childWidgetName)
	{
		return FindChildOfType<Widget>(childWidgetName);
	}

	public override T FindWidgetComponent<T>(params string[] path)
	{
		if (path == null || path.Length == 0)
		{
			return GetComponent<T>();
		}
		if (path.Length == 1)
		{
			return FindChildOfType<T>(path[0]);
		}
		Widget widget = this;
		int i;
		for (i = 0; i < path.Length - 1; i++)
		{
			if (!(widget != null))
			{
				break;
			}
			if ((widget = widget.FindWidget(path[i])) == null)
			{
				return null;
			}
		}
		if (!(widget != null))
		{
			return null;
		}
		return widget.FindWidgetComponent<T>(new string[1] { path[i] });
	}

	public override bool TriggerEvent(string eventName, TriggerEventParameters parameters = default(TriggerEventParameters))
	{
		return EventFunctions.TriggerEvent(base.transform, eventName, parameters);
	}

	public void AddNestedInstance(WidgetInstance nestedInstance, GameObject parent = null)
	{
		if (m_addedInstances == null)
		{
			m_addedInstances = new List<WidgetInstance>();
		}
		m_addedInstances.Add(nestedInstance);
		if (m_nestedInstances == null)
		{
			m_nestedInstances = new List<WidgetInstance>();
		}
		m_nestedInstances.Add(nestedInstance);
		if (m_componentsSet != null)
		{
			m_componentsSet.Add(nestedInstance.transform);
		}
		nestedInstance.ParentWidgetTemplate = this;
		nestedInstance.RegisterStartChangingStatesListener(HandleStartChangingStates, nestedInstance);
		nestedInstance.RegisterDoneChangingStatesListener(HandleDoneChangingStates, nestedInstance, callImmediatelyIfSet: false);
		nestedInstance.PreInitialize();
		if (parent != null)
		{
			DataContext dataContext = GetDataContextForGameObject(parent);
			if (dataContext != null && m_dataContext != dataContext)
			{
				CreateCustomDataContextForGameObject(nestedInstance.gameObject, dataContext);
				ProcessNewGameObjectBinding(nestedInstance.gameObject);
			}
		}
	}

	public void RemoveNestedInstance(WidgetInstance nestedInstance)
	{
		nestedInstance.RemoveStartChangingStatesListener(HandleStartChangingStates);
		nestedInstance.RemoveDoneChangingStatesListener(HandleDoneChangingStates);
		if (nestedInstance.StartedChangingStates)
		{
			HandleDoneChangingStates(nestedInstance);
		}
		if (m_addedInstances != null)
		{
			m_addedInstances.Remove(nestedInstance);
		}
		if (m_nestedInstances != null)
		{
			m_nestedInstances.Remove(nestedInstance);
		}
		if (nestedInstance.ParentWidgetTemplate == this)
		{
			nestedInstance.ParentWidgetTemplate = null;
		}
		m_gameObjectsToBindingsMap.Remove(nestedInstance.gameObject);
	}

	public Component GetComponentById(long id)
	{
		if (id == 0L)
		{
			return base.transform;
		}
		m_componentsById.TryGetValue(id, out var comp);
		return comp;
	}

	public bool GetComponentId(Component component, out long id)
	{
		if (component == base.transform)
		{
			id = 0L;
			return true;
		}
		foreach (KeyValuePair kv in m_pairs)
		{
			if (kv.Value == component)
			{
				id = kv.Key;
				return true;
			}
		}
		id = -1L;
		return false;
	}

	public void OnBeforeSerialize()
	{
	}

	public void OnAfterDeserialize()
	{
		BuildComponentMap();
	}

	private void BuildComponentMap()
	{
		m_nestedInstances?.Clear();
		m_visualControllers?.Clear();
		m_widgetBehaviors?.Clear();
		if (m_addedInstances != null)
		{
			if (m_nestedInstances == null)
			{
				m_nestedInstances = new List<WidgetInstance>();
			}
			foreach (WidgetInstance addedInstance in m_addedInstances)
			{
				if (addedInstance != null)
				{
					m_nestedInstances.Add(addedInstance);
				}
			}
		}
		int pairCount = m_pairs.Count;
		m_componentsById = new Map<long, Component>(pairCount);
		for (int i = 0; i < pairCount; i++)
		{
			KeyValuePair kv = m_pairs[i];
			if (kv.Key != 0L)
			{
				m_componentsById.Add(kv.Key, kv.Value);
			}
			if (kv.Value is WidgetInstance widgetInstance)
			{
				if (m_nestedInstances == null)
				{
					m_nestedInstances = new List<WidgetInstance>();
				}
				m_nestedInstances.Add(widgetInstance);
			}
			if (kv.Value is WidgetBehavior widgetBehavior)
			{
				if (m_widgetBehaviors == null)
				{
					m_widgetBehaviors = new List<WidgetBehavior>();
				}
				m_widgetBehaviors.Add(widgetBehavior);
			}
			if (kv.Value is VisualController visualController)
			{
				if (m_visualControllers == null)
				{
					m_visualControllers = new List<VisualController>();
				}
				m_visualControllers.Add(visualController);
			}
		}
		ReconcileDataContextMap();
	}

	private void ReconcileDataContextMap()
	{
		if (m_gameObjectsToBindingsMap.Count <= 0)
		{
			return;
		}
		HashSet<GameObject> storedGameObjects = new HashSet<GameObject>();
		foreach (KeyValuePair pair in m_pairs)
		{
			storedGameObjects.Add(pair.Value.gameObject);
		}
		if (m_addedInstances != null)
		{
			foreach (WidgetInstance addedInstance in m_addedInstances)
			{
				if (!(addedInstance == null))
				{
					storedGameObjects.Add(addedInstance.gameObject);
				}
			}
		}
		Dictionary<GameObject, GameObjectBinding> originalMap = new Dictionary<GameObject, GameObjectBinding>(m_gameObjectsToBindingsMap);
		m_gameObjectsToBindingsMap.Clear();
		foreach (KeyValuePair<GameObject, GameObjectBinding> kv in originalMap)
		{
			GameObjectBinding binding = null;
			if (storedGameObjects.Contains(kv.Key) && originalMap.TryGetValue(kv.Key, out binding))
			{
				m_gameObjectsToBindingsMap.Add(kv.Key, binding);
			}
		}
	}

	private T FindChildOfType<T>(string childName) where T : Component
	{
		Queue<Transform> queue = new Queue<Transform>();
		queue.Enqueue(base.transform);
		while (queue.Count > 0)
		{
			Transform candidate = queue.Dequeue();
			if (candidate.name.Equals(childName, StringComparison.InvariantCultureIgnoreCase))
			{
				return candidate.GetComponent<T>();
			}
			if (candidate.GetComponent<WidgetInstance>() == null)
			{
				for (int i = 0; i < candidate.childCount; i++)
				{
					queue.Enqueue(candidate.GetChild(i));
				}
			}
		}
		return null;
	}

	public void SetPopupRoot(PopupRoot newPopupRoot)
	{
		if (newPopupRoot.PopupBone == base.transform.parent)
		{
			m_hasPopupRootParent = true;
			m_popupRootBoneParentGameObject = newPopupRoot.PopupBone.parent.gameObject;
			newPopupRoot.OnDestroyed += ParentPopupRootOnDestroy;
		}
	}

	private void ParentPopupRootOnDestroy(PopupRoot oldPopupRoot)
	{
		oldPopupRoot.OnDestroyed -= ParentPopupRootOnDestroy;
		m_hasPopupRootParent = false;
		m_popupRootBoneParentGameObject = null;
	}

	private void OnTransformParentChanged()
	{
		Transform parent = base.transform.parent;
		if (parent != null)
		{
			m_parentGameObject = parent.gameObject;
		}
		else
		{
			m_parentGameObject = null;
		}
	}
}
