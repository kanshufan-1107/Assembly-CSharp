using System;
using System.Collections.Generic;
using Blizzard.T5.AssetManager;
using Blizzard.T5.Jobs;
using Blizzard.T5.Services;
using Unity.Profiling;
using UnityEngine;

namespace Hearthstone.UI;

public class WidgetRunner : IService, IHasUpdate
{
	private const float MAX_FRAME_TIME_SECONDS = 5f;

	private const float CHECK_OBJECT_DISPOSED_TIME_SECONDS = 1f;

	private HashSet<WidgetTemplate> m_widgetTemplates = new HashSet<WidgetTemplate>();

	private HashSet<WidgetTemplate> m_widgetsPendingTick = new HashSet<WidgetTemplate>();

	public static HashSet<WidgetTemplate> s_inactiveTemplates = new HashSet<WidgetTemplate>();

	public static HashSet<WidgetInstance> s_inactiveInstances = new HashSet<WidgetInstance>();

	private List<WidgetTemplate> m_templatesToRemove = new List<WidgetTemplate>();

	private List<WidgetInstance> m_instancesToRemove = new List<WidgetInstance>();

	private List<(UnityEngine.Object, AssetHandle)> m_ownedAssetHandlePairs = new List<(UnityEngine.Object, AssetHandle)>();

	private float m_lastDisposeCheckTime;

	private static HashSet<WidgetTemplate> s_tempUpdateCache = new HashSet<WidgetTemplate>();

	private static ProfilerMarker s_updateAddToTempCache = new ProfilerMarker("WidgetRunner.Update.AddWidgetsToTempCache");

	private static ProfilerMarker s_tickWidgets = new ProfilerMarker("WidgetRunner.TickWidgets");

	private static ProfilerMarker s_tickPendingWidgets = new ProfilerMarker("WidgetRunner.TickWidgets.TickPendingWidgets");

	private static ProfilerMarker s_resetWidgets = new ProfilerMarker("WidgetRunner.ResetWidgets");

	private static ProfilerMarker s_cleanWidgets = new ProfilerMarker("WidgetRunner.CleanWidgets");

	public void Update()
	{
		foreach (WidgetTemplate widgetTemplate in m_widgetTemplates)
		{
			s_tempUpdateCache.Add(widgetTemplate);
		}
		float startTime = Time.realtimeSinceStartup;
		bool fullyResolved = false;
		int numIterations = 0;
		while (Time.realtimeSinceStartup - startTime < 5f)
		{
			foreach (WidgetTemplate widgetTemplate2 in m_widgetsPendingTick)
			{
				s_tempUpdateCache.Add(widgetTemplate2);
			}
			m_widgetsPendingTick.Clear();
			foreach (WidgetTemplate widget in s_tempUpdateCache)
			{
				if (widget != null)
				{
					widget.Tick();
				}
			}
			s_tempUpdateCache.Clear();
			if (m_widgetsPendingTick.Count == 0)
			{
				fullyResolved = true;
				break;
			}
			numIterations++;
		}
		foreach (WidgetTemplate widgetTemplate3 in m_widgetTemplates)
		{
			widgetTemplate3.ResetUpdateTargets();
		}
		if (Time.realtimeSinceStartup - m_lastDisposeCheckTime >= 1f)
		{
			CleanWidgets();
			HandleDisposeUnusedAssetHandles();
		}
		if (!fullyResolved)
		{
			Log.All.PrintWarning("Resolving widget visual states timed out at {0} seconds and {1} iterations!", 5f, numIterations);
		}
	}

	private void OnPreLoadNextScene(SceneMgr.Mode prevMode, SceneMgr.Mode mode, object userData)
	{
		CleanWidgets();
		HandleDisposeUnusedAssetHandles();
	}

	private bool IsZombie(object obj)
	{
		bool num = obj != null;
		bool isUnityObj = obj is UnityEngine.Object;
		UnityEngine.Object unityObj = obj as UnityEngine.Object;
		if (num && isUnityObj)
		{
			return unityObj == null;
		}
		return false;
	}

	private void CleanWidgets()
	{
		foreach (WidgetTemplate template in s_inactiveTemplates)
		{
			if (IsZombie(template))
			{
				m_templatesToRemove.Add(template);
				template.Deinitialize();
			}
		}
		foreach (WidgetInstance instance in s_inactiveInstances)
		{
			if (IsZombie(instance))
			{
				m_instancesToRemove.Add(instance);
				instance.Deinitialize();
			}
		}
		foreach (WidgetTemplate template2 in m_templatesToRemove)
		{
			s_inactiveTemplates.Remove(template2);
		}
		foreach (WidgetInstance instance2 in m_instancesToRemove)
		{
			s_inactiveInstances.Remove(instance2);
		}
		m_templatesToRemove.Clear();
		m_instancesToRemove.Clear();
	}

	private void HandleDisposeUnusedAssetHandles()
	{
		m_lastDisposeCheckTime = Time.realtimeSinceStartup;
		for (int i = m_ownedAssetHandlePairs.Count - 1; i >= 0; i--)
		{
			(UnityEngine.Object, AssetHandle) pair = m_ownedAssetHandlePairs[i];
			if (!(pair.Item1 != null))
			{
				pair.Item2?.Dispose();
				m_ownedAssetHandlePairs.RemoveAt(i);
			}
		}
	}

	public void RegisterWidget(WidgetTemplate widget)
	{
		m_widgetTemplates.Add(widget);
		m_widgetsPendingTick.Add(widget);
	}

	public void UnregisterWidget(WidgetTemplate widget)
	{
		m_widgetTemplates.Remove(widget);
		m_widgetsPendingTick.Remove(widget);
	}

	public void RegisterAssetHandle(UnityEngine.Object owner, AssetHandle assetHandle)
	{
		m_ownedAssetHandlePairs.Add((owner, assetHandle));
	}

	public void UnregisterAssetHandle(UnityEngine.Object owner, AssetHandle assetHandle)
	{
		m_ownedAssetHandlePairs.Remove((owner, assetHandle));
	}

	public void AddWidgetPendingTick(WidgetTemplate widget)
	{
		m_widgetsPendingTick.Add(widget);
	}

	public Type[] GetDependencies()
	{
		return null;
	}

	public IEnumerator<IAsyncJobResult> Initialize(ServiceLocator serviceLocator)
	{
		yield return new ServiceSoftDependency(typeof(SceneMgr), serviceLocator);
		if (serviceLocator.TryGetService<SceneMgr>(out var sceneMgr))
		{
			sceneMgr.RegisterScenePreLoadEvent(OnPreLoadNextScene);
		}
	}

	public void Shutdown()
	{
		if (ServiceManager.TryGet<SceneMgr>(out var sceneMgr))
		{
			sceneMgr.UnregisterScenePreLoadEvent(OnPreLoadNextScene);
		}
	}
}
