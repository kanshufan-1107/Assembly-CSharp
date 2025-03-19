using System;
using System.Collections;
using System.Collections.Generic;
using Hearthstone.Core;
using Hearthstone.UI;
using UnityEngine;

public abstract class StaticWidgetCache<T> : StaticWidgetCacheBase where T : class, IDataModel
{
	private struct StaticWidgetData
	{
		public Widget widget;

		public StaticWidgetCacheLender currentOwner;
	}

	private struct RequestData
	{
		public T requestedData;

		public GameObject handler;

		public GameLayer overrideLayer;
	}

	[SerializeField]
	private GameObject m_cacheHolder;

	[SerializeField]
	private WeakAssetReference m_cachedWidget;

	private Dictionary<string, List<StaticWidgetData>> m_widgets = new Dictionary<string, List<StaticWidgetData>>();

	private Dictionary<string, Stack<Widget>> m_freeWidgets = new Dictionary<string, Stack<Widget>>();

	private Dictionary<StaticWidgetCacheLender, List<Widget>> m_lenders = new Dictionary<StaticWidgetCacheLender, List<Widget>>();

	private Dictionary<StaticWidgetCacheLender, List<RequestData>> m_lenderRequests = new Dictionary<StaticWidgetCacheLender, List<RequestData>>();

	private bool m_isPaused;

	private HashSet<string> m_pauseRequestIds = new HashSet<string>();

	private const float TARGET_PRELOAD_TIME = 25f;

	private Coroutine m_PreloadCoroutine;

	public override string GetUniqueIdentifier(IDataModel dataModelBase)
	{
		if (!(dataModelBase is T dataModel))
		{
			return null;
		}
		return GetUniqueIdentifier(dataModel);
	}

	public abstract string GetUniqueIdentifier(T dataModel);

	public override void RequestWidget(StaticWidgetCacheLender lender, IDataModel dataModelBase, GameObject handler = null, GameLayer overrideLayer = GameLayer.Default)
	{
		if (!(dataModelBase is T dataModelSource))
		{
			return;
		}
		T dataModel = dataModelSource.CloneDataModel();
		if (m_isPaused)
		{
			if (!m_lenderRequests.TryGetValue(lender, out var requestedData))
			{
				requestedData = new List<RequestData>();
				m_lenderRequests.Add(lender, requestedData);
			}
			bool found = false;
			foreach (RequestData item in requestedData)
			{
				if (GetUniqueIdentifier(item.requestedData) == GetUniqueIdentifier(dataModel))
				{
					found = true;
				}
			}
			if (!found)
			{
				requestedData.Add(new RequestData
				{
					requestedData = dataModel,
					handler = handler,
					overrideLayer = overrideLayer
				});
			}
		}
		else
		{
			if (!m_lenders.TryGetValue(lender, out var lentWidgets))
			{
				lentWidgets = new List<Widget>();
				m_lenders.Add(lender, lentWidgets);
			}
			string uniqueId = GetUniqueIdentifier(dataModel);
			Stack<Widget> freeWidgets;
			Widget lentWidget = ((!m_freeWidgets.TryGetValue(uniqueId, out freeWidgets) || freeWidgets.Count <= 0) ? GetNewWidgetInstance(dataModel) : freeWidgets.Pop());
			if (!(lentWidget == null))
			{
				UpdateWidgetList(uniqueId, new StaticWidgetData
				{
					widget = lentWidget,
					currentOwner = lender
				});
				lentWidgets.Add(lentWidget);
				lentWidget.Show();
				GameUtils.SetParent(lentWidget.gameObject, (handler != null) ? handler.gameObject : lender.gameObject, withRotation: true);
				lentWidget.SetLayerOverride(overrideLayer);
				lentWidget.transform.localPosition = Vector3.zero;
			}
		}
	}

	public override void ReturnWidgets(StaticWidgetCacheLender lender)
	{
		if (m_isPaused)
		{
			m_lenderRequests.Remove(lender);
		}
		if (!m_lenders.TryGetValue(lender, out var lentWidgets))
		{
			return;
		}
		foreach (WidgetInstance lentWidget in lentWidgets)
		{
			T dataModel = lentWidget.GetDataModel<T>();
			if (dataModel == null)
			{
				return;
			}
			string uniqueId = GetUniqueIdentifier(dataModel);
			if (!m_freeWidgets.TryGetValue(uniqueId, out var freeWidgets))
			{
				freeWidgets = new Stack<Widget>();
				m_freeWidgets[uniqueId] = freeWidgets;
			}
			UpdateWidgetList(uniqueId, new StaticWidgetData
			{
				widget = lentWidget,
				currentOwner = null
			});
			freeWidgets.Push(lentWidget);
			GameUtils.SetParent(lentWidget.gameObject, m_cacheHolder, withRotation: true);
			lentWidget.Hide();
		}
		lentWidgets.Clear();
	}

	public override void Preload(IEnumerable<IDataModel> dataModels, bool createNew = false)
	{
		m_PreloadCoroutine = Processor.RunCoroutine(PreloadCoro(dataModels, createNew));
	}

	private WidgetInstance PreloadDataModel(IDataModel dataModelBase, bool createNew)
	{
		if (!(dataModelBase is T dataModel))
		{
			return null;
		}
		string uniqueId = GetUniqueIdentifier(dataModel);
		if (!createNew && m_widgets.TryGetValue(uniqueId, out var dataList) && dataList.Count > 0)
		{
			return null;
		}
		if (!m_freeWidgets.TryGetValue(uniqueId, out var freeWidgets))
		{
			freeWidgets = new Stack<Widget>();
			m_freeWidgets[uniqueId] = freeWidgets;
		}
		WidgetInstance preWidget = GetNewWidgetInstance(dataModel);
		UpdateWidgetList(uniqueId, new StaticWidgetData
		{
			widget = preWidget,
			currentOwner = null
		});
		freeWidgets.Push(preWidget);
		GameUtils.SetParent(preWidget.gameObject, m_cacheHolder, withRotation: true);
		preWidget.Hide();
		return preWidget;
	}

	private IEnumerator PreloadCoro(IEnumerable<IDataModel> dataModels, bool createNew = false)
	{
		DataModelList<IDataModel> dataModelList = dataModels.ToDataModelList();
		if (dataModelList.Count == 0)
		{
			yield break;
		}
		int preloadBatchSize = dataModelList.Count;
		int currentElement = 0;
		IDataModel dataModelBase = dataModelList[currentElement];
		int num = currentElement + 1;
		currentElement = num;
		float beforePreload = Time.unscaledTime;
		WidgetInstance testInstance = PreloadDataModel(dataModelBase, createNew);
		if (testInstance != null)
		{
			while (!testInstance.IsReady)
			{
				yield return null;
			}
			float timeDiff = Time.unscaledTime - beforePreload;
			if (timeDiff * (float)dataModelList.Count - 1f > 25f)
			{
				preloadBatchSize = Math.Max((int)(25f / timeDiff), 1);
			}
		}
		while (currentElement < dataModelList.Count)
		{
			int startElement = currentElement;
			List<WidgetInstance> preloadingElements = new List<WidgetInstance>();
			while (currentElement < startElement + preloadBatchSize && currentElement < dataModelList.Count)
			{
				IDataModel dataModelBase2 = dataModelList[currentElement];
				num = currentElement + 1;
				currentElement = num;
				WidgetInstance preWidget = PreloadDataModel(dataModelBase2, createNew);
				if (preWidget != null)
				{
					preloadingElements.Add(preWidget);
				}
			}
			if (preloadBatchSize == dataModelList.Count)
			{
				break;
			}
			Func<bool> elementsAllPreloaded = delegate
			{
				foreach (WidgetInstance item in preloadingElements)
				{
					if (!item.IsReady)
					{
						return false;
					}
				}
				return true;
			};
			while (!elementsAllPreloaded())
			{
				yield return null;
			}
		}
	}

	public override void Rebind(IDataModel dataModelBase)
	{
		if (!(dataModelBase is T dataModel))
		{
			return;
		}
		string uniqueId = GetUniqueIdentifier(dataModel);
		if (!m_widgets.TryGetValue(uniqueId, out var dataList))
		{
			return;
		}
		foreach (StaticWidgetData data in dataList)
		{
			if (data.widget != null)
			{
				data.widget.BindDataModel(dataModel);
			}
		}
	}

	public override void Pause(bool pause, string pauseRequestId)
	{
		if (pause)
		{
			m_pauseRequestIds.Add(pauseRequestId);
		}
		else
		{
			m_pauseRequestIds.Remove(pauseRequestId);
		}
		bool isPaused = m_isPaused;
		m_isPaused = m_pauseRequestIds.Count > 0;
		if (!isPaused || m_isPaused)
		{
			return;
		}
		foreach (KeyValuePair<StaticWidgetCacheLender, List<RequestData>> kvp in m_lenderRequests)
		{
			if (kvp.Key == null)
			{
				continue;
			}
			foreach (RequestData requestData in kvp.Value)
			{
				RequestWidget(kvp.Key, requestData.requestedData, requestData.handler, requestData.overrideLayer);
			}
		}
		m_lenderRequests.Clear();
	}

	private WidgetInstance GetNewWidgetInstance(T dataModel)
	{
		WidgetInstance widgetInstance = WidgetInstance.Create(m_cachedWidget.AssetString);
		widgetInstance.BindDataModel(dataModel);
		return widgetInstance;
	}

	private void UpdateWidgetList(string uniqueId, StaticWidgetData updateData)
	{
		if (m_widgets.TryGetValue(uniqueId, out var dataList))
		{
			bool found = false;
			for (int i = 0; i < dataList.Count; i++)
			{
				if (dataList[i].widget == updateData.widget)
				{
					dataList[i] = updateData;
					found = true;
					break;
				}
			}
			if (!found)
			{
				dataList.Add(updateData);
			}
		}
		else
		{
			m_widgets.Add(uniqueId, new List<StaticWidgetData> { updateData });
		}
	}
}
