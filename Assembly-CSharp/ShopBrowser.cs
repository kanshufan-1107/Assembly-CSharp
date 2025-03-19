using System;
using System.Collections;
using System.Collections.Generic;
using Blizzard.T5.Services;
using Blizzard.Telemetry.WTCG.Client;
using Hearthstone.DataModels;
using Hearthstone.Store;
using Hearthstone.UI;
using Hearthstone.UI.Core;
using UnityEngine;
using UnityEngine.Serialization;

public class ShopBrowser : MonoBehaviour, IPopupRendering
{
	private struct BufferedSection
	{
		public ShopSection Section { get; set; }

		public ShopDivider Divider { get; set; }
	}

	[SerializeField]
	private Widget m_widget;

	[FormerlySerializedAs("m_stackingMargins")]
	[SerializeField]
	private float m_sectionMargins;

	[SerializeField]
	private float m_dividerMargins;

	[SerializeField]
	private bool m_loadWidgetInstancesSynchronously;

	[SerializeField]
	[Min(0f)]
	[Header("Sections Buffered Loading")]
	private float m_timeBeforeSpinnerShowsSeconds = 0.5f;

	[Min(0f)]
	[SerializeField]
	private float m_minTimeToDisplaySpinnerSeconds = 0.25f;

	[SerializeField]
	[Min(0f)]
	private int m_suppressUntilLoadedCount;

	private List<ProductTierDataModel> m_currentTiers = new List<ProductTierDataModel>();

	private bool m_dataDirty;

	private List<ShopSection> m_sections = new List<ShopSection>();

	private List<WidgetInstance> m_instances = new List<WidgetInstance>();

	private Queue<ShopDivider> m_unusedDividers = new Queue<ShopDivider>();

	private List<ShopDivider> m_activeDividers = new List<ShopDivider>();

	private List<WidgetInstance> m_dividerInstances = new List<WidgetInstance>();

	private Coroutine m_loadingCoroutine;

	private Coroutine m_loadingSpinnerDelayCoroutine;

	private bool m_isLoadingSuppressed;

	private HashSet<string> m_suppressLoadingIds = new HashSet<string>();

	private float m_timeWhenSpinnerWasRequested;

	private FrameTimer m_shopLoadTimer = new FrameTimer();

	private FrameTimer m_sectionLoadTimer = new FrameTimer();

	private IPopupRoot m_popupRoot;

	private Shop m_shopInstance;

	private IProductDataService m_dataService;

	private const string SECTION_WIDGET_PREFAB = "ShopSectionV3.prefab:fb6a6f5c2a481a94aa8e02931f58eea7";

	private const string DIVIDER_PREFAB = "ShopDivider.prefab:7b88d2046d189044fb32ee36b76a66e4";

	private const string HIDE_TIERS = "HIDE_TIERS";

	private const string TIER_POSITIONED_EVENT = "TIER_POSITIONED";

	private const string TIER_SLOTS_LOADED_EVENT = "TIER_SLOTS_LOADED";

	private const string START_LOADING_VISUALS_EVENT = "START_LOADING_VISUALS";

	private const string STOP_LOADING_VISUALS_EVENT = "STOP_LOADING_VISUALS";

	[Overridable]
	public string SurpressLoading
	{
		set
		{
			SuppressLoading(suppress: true, value);
		}
	}

	[Overridable]
	public string UnsupressLoading
	{
		set
		{
			SuppressLoading(suppress: false, value);
		}
	}

	public ShopBrowserDataModel ShopBrowserData { get; private set; } = new ShopBrowserDataModel();

	private Shop ShopInstance
	{
		get
		{
			if (m_shopInstance == null)
			{
				m_shopInstance = Shop.Get();
			}
			return m_shopInstance;
		}
	}

	private IProductDataService DataService
	{
		get
		{
			if (m_dataService == null && !ServiceManager.TryGet<IProductDataService>(out m_dataService))
			{
				m_dataService = null;
			}
			return m_dataService;
		}
	}

	private void Awake()
	{
		m_widget.BindDataModel(ShopBrowserData);
		m_widget.RegisterEventListener(HandleEvent);
		if (ShopInstance != null)
		{
			ShopInstance.OnCloseCompleted += OnShopCloseCompleted;
		}
	}

	private void Update()
	{
		if (ShopInstance != null && ShopInstance.IsOpen())
		{
			if (DataService != null && DataService.TryRefreshStaleProductAvailability())
			{
				m_dataDirty = true;
			}
			if (m_dataDirty && !m_isLoadingSuppressed)
			{
				StartSectionLoading();
			}
		}
	}

	private void OnDestroy()
	{
		StopSectionLoading();
	}

	public bool IsReady()
	{
		if (m_loadingCoroutine != null)
		{
			return false;
		}
		foreach (ShopSection section in m_sections)
		{
			if (section != null && section.IsElementEnabled && !section.IsReady())
			{
				return false;
			}
		}
		return true;
	}

	public bool IsDirty()
	{
		return m_dataDirty;
	}

	public void SetData(IEnumerable<ProductTierDataModel> tiers)
	{
		if (tiers == null)
		{
			ClearData();
			Log.Store.PrintError("Cannot refresh shop browser with null contents");
		}
		else
		{
			m_currentTiers.Clear();
			m_currentTiers.AddRange(tiers);
			m_dataDirty = true;
		}
	}

	public void ForceRefresh()
	{
		m_dataDirty = true;
	}

	public void ClearData()
	{
		m_currentTiers.Clear();
		m_dataDirty = true;
	}

	public void ClearElements()
	{
		ClearInstances();
	}

	public List<ShopSection> GetActiveSections()
	{
		List<ShopSection> shopSections = new List<ShopSection>();
		foreach (ShopSection section in m_sections)
		{
			if (section != null && section.IsElementEnabled)
			{
				shopSections.Add(section);
			}
		}
		return shopSections;
	}

	public void SuppressLoading(bool suppress, string suppressId)
	{
		if (suppress)
		{
			if (m_suppressLoadingIds.Add(suppressId) && m_suppressLoadingIds.Count == 1)
			{
				OnSuppressLoadingChanged(isSuppressed: true);
			}
		}
		else if (m_suppressLoadingIds.Remove(suppressId) && m_suppressLoadingIds.Count == 0)
		{
			OnSuppressLoadingChanged(isSuppressed: false);
		}
	}

	public List<ShopCard> GetShopCardTelemetry()
	{
		List<ShopCard> shopCards = new List<ShopCard>();
		foreach (ShopSection activeSection in GetActiveSections())
		{
			foreach (ShopSlot slot in activeSection.GetActiveSlots())
			{
				shopCards.Add(slot.GetShopCardTelemetry());
			}
		}
		return shopCards;
	}

	private void HandleEvent(string e)
	{
		if (!(e == "HIDE_TIERS"))
		{
			return;
		}
		StopSectionLoading();
		foreach (WidgetInstance instance in m_instances)
		{
			instance.gameObject.SetActive(value: false);
		}
		foreach (WidgetInstance dividerInstance in m_dividerInstances)
		{
			dividerInstance.gameObject.SetActive(value: false);
		}
		foreach (ShopSection section in m_sections)
		{
			if (section != null)
			{
				section.SetScrollableState(isActive: false);
			}
		}
		ShopBrowserData.ShowEmptyShopMessage = false;
	}

	private void StartSectionLoading()
	{
		StopSectionLoading();
		if (m_currentTiers.Count == 0)
		{
			m_dataDirty = false;
			ClearInstances();
			RefreshBrowserData();
		}
		else
		{
			m_loadingCoroutine = StartCoroutine(LoadSectionsCoroutine());
		}
	}

	private void StopSectionLoading(string error = null)
	{
		SafeStopCoroutine(ref m_loadingSpinnerDelayCoroutine);
		if (m_loadingCoroutine != null)
		{
			StopCoroutine(m_loadingCoroutine);
			m_loadingCoroutine = null;
			if (!string.IsNullOrEmpty(error))
			{
				Log.Store.PrintError("Section loading was unexpectedly stopped - " + error);
			}
			RefreshBrowserData();
			m_shopLoadTimer.StopRecording();
			m_sectionLoadTimer.StopRecording();
		}
	}

	private IEnumerator LoadSectionsCoroutine()
	{
		m_dataDirty = false;
		SafeStopCoroutine(ref m_loadingSpinnerDelayCoroutine);
		m_loadingSpinnerDelayCoroutine = StartCoroutine(LoadingSpinnerDelayTimer());
		Log.Store.PrintDebug($"Started loading {m_currentTiers.Count} sections");
		m_shopLoadTimer.StartRecording();
		SetupInstances(m_currentTiers.Count);
		foreach (WidgetInstance instance in m_instances)
		{
			instance.gameObject.SetActive(value: false);
		}
		foreach (WidgetInstance dividerInstance in m_dividerInstances)
		{
			dividerInstance.gameObject.SetActive(value: false);
		}
		Queue<BufferedSection> queue = new Queue<BufferedSection>();
		bool hasStoppedLoadingVis = false;
		ShopBrowserElement previousElement = null;
		for (int i = 0; i < m_currentTiers.Count; i++)
		{
			ProductTierDataModel currentDataModel = m_currentTiers[i];
			if (currentDataModel == null)
			{
				currentDataModel = ProductFactory.CreateEmptyProductTier();
			}
			ProductTierDataModel previousDataModel = null;
			if (i - 1 >= 0)
			{
				previousDataModel = m_currentTiers[i - 1];
			}
			ShopDivider divider = null;
			if (previousDataModel != null && (currentDataModel.DisplayDivder || previousDataModel.DisplayDivder))
			{
				string dividerError = string.Empty;
				yield return FetchDivider(currentDataModel, delegate(ShopDivider result)
				{
					divider = result;
				}, delegate(string outError)
				{
					dividerError = outError;
				});
				if (divider == null)
				{
					StopSectionLoading("DividerFailed: " + (string.IsNullOrEmpty(dividerError) ? "Unknown" : dividerError));
					yield break;
				}
			}
			ShopSection section = null;
			string sectionError = string.Empty;
			yield return LoadSection(currentDataModel, m_instances[i], delegate(ShopSection result)
			{
				section = result;
			}, delegate(string outError)
			{
				sectionError = outError;
			});
			if (section == null)
			{
				StopSectionLoading("SectionFailed: " + (string.IsNullOrEmpty(sectionError) ? "Unknown" : sectionError));
				yield break;
			}
			queue.Enqueue(new BufferedSection
			{
				Divider = divider,
				Section = section
			});
			if (m_suppressUntilLoadedCount > 0 && i + 1 < Mathf.Min(m_suppressUntilLoadedCount, m_currentTiers.Count))
			{
				continue;
			}
			if (!hasStoppedLoadingVis)
			{
				SafeStopCoroutine(ref m_loadingSpinnerDelayCoroutine);
				float spinnerRequestTimeDiff = Time.realtimeSinceStartup - m_timeWhenSpinnerWasRequested;
				if (m_timeWhenSpinnerWasRequested > 0f && spinnerRequestTimeDiff < m_minTimeToDisplaySpinnerSeconds)
				{
					yield return new WaitForSeconds(Math.Max(m_minTimeToDisplaySpinnerSeconds - spinnerRequestTimeDiff, 0f));
				}
				SendEventUpwardStateAction.SendEventUpward(base.gameObject, "STOP_LOADING_VISUALS");
				hasStoppedLoadingVis = true;
			}
			while (queue.Count > 0)
			{
				BufferedSection buffer = queue.Dequeue();
				if (buffer.Section.IsElementEnabled)
				{
					if (buffer.Divider != null)
					{
						PositionElement(buffer.Divider, previousElement, m_dividerMargins);
						previousElement = buffer.Divider;
					}
					PositionElement(buffer.Section, previousElement, m_sectionMargins);
					buffer.Section.SetScrollableState(isActive: true);
					buffer.Section.Widget.TriggerEvent("TIER_POSITIONED", TriggerEventParameters.Standard);
					previousElement = buffer.Section;
				}
			}
		}
		RefreshBrowserData();
		SafeStopCoroutine(ref m_loadingSpinnerDelayCoroutine);
		m_loadingCoroutine = null;
		m_shopLoadTimer.StopRecording();
		Log.Store.PrintDebug($"Completed loading {m_currentTiers.Count} sections - {m_shopLoadTimer.TimeTakenInfo}");
	}

	private IEnumerator LoadingSpinnerDelayTimer()
	{
		m_timeWhenSpinnerWasRequested = 0f;
		if (m_timeBeforeSpinnerShowsSeconds > 0f)
		{
			yield return new WaitForSeconds(m_timeBeforeSpinnerShowsSeconds);
		}
		m_loadingSpinnerDelayCoroutine = null;
		if (m_suppressUntilLoadedCount > 0 && m_loadingCoroutine != null)
		{
			m_timeWhenSpinnerWasRequested = Time.realtimeSinceStartup;
			SendEventUpwardStateAction.SendEventUpward(base.gameObject, "START_LOADING_VISUALS");
		}
	}

	private IEnumerator LoadSection(ProductTierDataModel dataModel, WidgetInstance inst, Action<ShopSection> success, Action<string> error)
	{
		m_sectionLoadTimer.StartRecording();
		Log.Store.PrintDebug("Started loading section " + dataModel.GetDebugName());
		inst.BindDataModel(dataModel);
		inst.gameObject.SetActive(value: true);
		if (inst.WillLoadSynchronously)
		{
			inst.Initialize();
		}
		while (!inst.IsReady)
		{
			yield return null;
			if (inst == null)
			{
				error?.Invoke("Initialized instance is null");
				yield break;
			}
		}
		if (inst.Widget == null)
		{
			error?.Invoke("Instance failed to load template");
			yield break;
		}
		ShopSection section = inst.Widget.GetComponent<ShopSection>();
		if (section == null)
		{
			error?.Invoke("Instance has no ShopSection element component");
			yield break;
		}
		if (!m_sections.Contains(section))
		{
			m_sections.Add(section);
		}
		section.RefreshData();
		if (!section.IsElementEnabled)
		{
			m_sectionLoadTimer.StopRecording();
			Log.Store.PrintDebug("Cancelled loading section " + dataModel.GetDebugName() + " - " + m_sectionLoadTimer.TimeTakenInfo);
			success?.Invoke(section);
			yield break;
		}
		while (section.Widget.GetIsChangingStates((GameObject go) => go.GetComponent<ShopSlot>() == null))
		{
			yield return null;
			if (inst == null)
			{
				error?.Invoke("Instance was destroyed during load");
				yield break;
			}
		}
		section.RefreshContent();
		while (!section.IsReady())
		{
			yield return null;
			if (section == null)
			{
				error?.Invoke("Section was destroyed during load");
				yield break;
			}
			if (!section.IsElementEnabled)
			{
				m_sectionLoadTimer.StopRecording();
				Log.Store.PrintDebug("Cancelled loading section " + dataModel.GetDebugName() + " - " + m_sectionLoadTimer.TimeTakenInfo);
				success?.Invoke(section);
				yield break;
			}
		}
		section.Widget.TriggerEvent("TIER_SLOTS_LOADED", TriggerEventParameters.Standard);
		if (m_popupRoot != null)
		{
			((IPopupRendering)section).EnablePopupRendering(m_popupRoot);
		}
		m_sectionLoadTimer.StopRecording();
		Log.Store.PrintDebug("Finished loading section " + dataModel.GetDebugName() + " - " + m_sectionLoadTimer.TimeTakenInfo);
		success?.Invoke(section);
	}

	private IEnumerator FetchDivider(ProductTierDataModel dataModel, Action<ShopDivider> success, Action<string> error)
	{
		ShopDivider divider;
		if (m_unusedDividers.Count > 0)
		{
			divider = m_unusedDividers.Dequeue();
			WidgetInstance instance = divider.Instance;
			instance.SetLayerOverride((GameLayer)base.gameObject.layer);
			instance.transform.localPosition = new Vector3(0f, 0f, 1000f);
			instance.BindDataModel(dataModel);
			instance.gameObject.SetActive(value: true);
		}
		else
		{
			WidgetInstance inst = WidgetInstance.Create("ShopDivider.prefab:7b88d2046d189044fb32ee36b76a66e4");
			inst.SetLayerOverride((GameLayer)base.gameObject.layer);
			inst.transform.localPosition = new Vector3(0f, 0f, 1000f);
			inst.transform.SetParent(base.transform, worldPositionStays: false);
			inst.name = string.Format($"Divider {m_dividerInstances.Count}");
			inst.WillLoadSynchronously = m_loadWidgetInstancesSynchronously;
			m_dividerInstances.Add(inst);
			inst.BindDataModel(dataModel);
			inst.gameObject.SetActive(value: true);
			inst.Initialize();
			while (!inst.IsReady)
			{
				yield return null;
				if (inst == null)
				{
					error?.Invoke("Initialized instance is null");
					yield break;
				}
			}
			if (inst.Widget == null)
			{
				error?.Invoke("Instance failed to load template");
				yield break;
			}
			divider = inst.Widget.GetComponent<ShopDivider>();
			if (divider == null)
			{
				error?.Invoke("Instance has no ShopDivider element component");
				yield break;
			}
		}
		bool dividerReady = false;
		divider.Widget.RegisterDoneChangingStatesListener(delegate
		{
			dividerReady = true;
		}, null, callImmediatelyIfSet: true, doOnce: true);
		while (!dividerReady)
		{
			yield return null;
		}
		if (m_popupRoot != null)
		{
			((IPopupRendering)divider).EnablePopupRendering(m_popupRoot);
		}
		divider.gameObject.SetActive(value: true);
		m_activeDividers.Add(divider);
		success?.Invoke(divider);
	}

	private void PositionElement(ShopBrowserElement element, ShopBrowserElement previousElement, float margin)
	{
		float offset = ((!(previousElement != null)) ? 0f : (previousElement.ElementLocalPosition.z + previousElement.WidgetTransform.Bottom - margin));
		Vector3 currentLocalPosition = element.ElementLocalPosition;
		element.ElementLocalPosition = new Vector3(currentLocalPosition.x, currentLocalPosition.y, offset - element.WidgetTransform.Top);
	}

	private void SetupInstances(int targetCount)
	{
		if (targetCount <= 0)
		{
			ClearInstances();
			return;
		}
		targetCount = Mathf.Max(targetCount, 1);
		while (m_instances.Count > targetCount)
		{
			int lastIndex = m_instances.Count - 1;
			if (lastIndex < 0)
			{
				continue;
			}
			WidgetInstance instance = m_instances[lastIndex];
			m_instances.RemoveAt(lastIndex);
			if (instance != null)
			{
				m_sections.RemoveAll((ShopSection s) => s == null || s.transform.IsChildOf(instance.transform));
				UnityEngine.Object.Destroy(instance.gameObject);
			}
		}
		while (m_instances.Count < targetCount)
		{
			WidgetInstance inst = WidgetInstance.Create("ShopSectionV3.prefab:fb6a6f5c2a481a94aa8e02931f58eea7");
			inst.SetLayerOverride((GameLayer)base.gameObject.layer);
			inst.transform.SetParent(base.transform, worldPositionStays: false);
			inst.transform.localPosition = new Vector3(0f, 0f, 1000f);
			inst.name = string.Format($"Section {m_instances.Count}");
			inst.WillLoadSynchronously = m_loadWidgetInstancesSynchronously;
			m_instances.Add(inst);
		}
		foreach (ShopDivider divider in m_activeDividers)
		{
			m_unusedDividers.Enqueue(divider);
		}
		m_activeDividers.Clear();
		foreach (ShopDivider unusedDivider in m_unusedDividers)
		{
			unusedDivider.gameObject.SetActive(value: false);
		}
	}

	private void ClearInstances()
	{
		foreach (WidgetInstance inst in m_instances)
		{
			if (inst != null)
			{
				UnityEngine.Object.Destroy(inst.gameObject);
			}
		}
		m_instances.Clear();
		m_sections.Clear();
		foreach (WidgetInstance inst2 in m_dividerInstances)
		{
			if (inst2 != null)
			{
				UnityEngine.Object.Destroy(inst2.gameObject);
			}
		}
		m_dividerInstances.Clear();
		m_activeDividers.Clear();
		m_unusedDividers.Clear();
	}

	private void RefreshBrowserData()
	{
		bool isShowingSection = false;
		foreach (ShopSection section in GetActiveSections())
		{
			if (section != null && section.IsElementEnabled && section.GetActiveSlots().Count > 0)
			{
				isShowingSection = true;
			}
		}
		ShopBrowserData.ShowEmptyShopMessage = !isShowingSection;
		ShopBrowserData.EmptyShopMessage = "GLUE_STORE_UNAVAILABLE_REASON_UNKNOWN";
	}

	private void OnSuppressLoadingChanged(bool isSuppressed)
	{
		m_isLoadingSuppressed = isSuppressed;
	}

	private void OnShopCloseCompleted()
	{
		StopSectionLoading();
		m_currentTiers.Clear();
		ClearInstances();
	}

	void IPopupRendering.EnablePopupRendering(IPopupRoot popupRoot)
	{
		m_popupRoot = popupRoot;
	}

	void IPopupRendering.DisablePopupRendering()
	{
		m_popupRoot = null;
	}

	bool IPopupRendering.HandlesChildPropagation()
	{
		return true;
	}

	private void SafeStopCoroutine(ref Coroutine coroutine)
	{
		if (coroutine != null)
		{
			StopCoroutine(coroutine);
			coroutine = null;
		}
	}
}
