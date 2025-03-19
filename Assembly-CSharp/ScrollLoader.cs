using System;
using System.Collections;
using System.Collections.Generic;
using Hearthstone.UI;
using UnityEngine;

public class ScrollLoader : MonoBehaviour
{
	[SerializeField]
	private AsyncReference m_scrollableReference;

	private UIBScrollable m_scrollable;

	private bool m_isScrollableReady;

	[SerializeField]
	private AsyncReference m_listReference;

	private Listable m_list;

	private bool m_isListReady;

	[Min(0f)]
	[SerializeField]
	private float m_itemHeight;

	[Min(0f)]
	[SerializeField]
	private int m_itemBuffer;

	[SerializeField]
	private string m_showItemEvent;

	[SerializeField]
	private string m_hideItemEvent;

	private bool m_isReady;

	private bool m_isPaused;

	private bool m_hasChangedStatesOnce;

	private bool _isUpdatingData;

	private Dictionary<string, WidgetInstance> m_visibleAffectedObjects = new Dictionary<string, WidgetInstance>();

	private List<Action> m_startChangingStatesListeners = new List<Action>();

	private List<Action> m_doneChangingStatesListeners = new List<Action>();

	private List<Action<bool>> m_onPausedListeners = new List<Action<bool>>();

	private Coroutine m_waitUntilReadyCoroutine;

	private Coroutine m_refreshDelayCoroutine;

	public bool IsChangingState { get; private set; }

	private bool IsUpdatingData
	{
		get
		{
			return _isUpdatingData;
		}
		set
		{
			_isUpdatingData = value;
			RefreshIsChangingState();
		}
	}

	private void Awake()
	{
		m_scrollableReference.RegisterReadyListener<UIBScrollable>(OnScrollableReady);
		m_listReference.RegisterReadyListener<Listable>(OnListReady);
		m_waitUntilReadyCoroutine = StartCoroutine(WaitUntilReady());
	}

	private void OnDestroy()
	{
		if (m_waitUntilReadyCoroutine != null)
		{
			StopCoroutine(m_waitUntilReadyCoroutine);
			m_waitUntilReadyCoroutine = null;
		}
	}

	private IEnumerator WaitUntilReady()
	{
		while (!m_isScrollableReady)
		{
			yield return null;
		}
		while (!m_isListReady)
		{
			yield return null;
		}
		m_isReady = true;
		m_list.RegisterDataChangedListener(OnListDataChanged);
		m_list.RegisterDoneChangingStatesListener(OnListDoneChangingStates);
		RefreshVisibleAffectedObjects();
		m_waitUntilReadyCoroutine = null;
	}

	public void Pause(bool isPaused)
	{
		bool wasPaused = m_isPaused;
		m_isPaused = isPaused;
		if (!isPaused && wasPaused)
		{
			RefreshCurrentVisibleStates();
			{
				foreach (Action<bool> onPausedListener in m_onPausedListeners)
				{
					onPausedListener?.Invoke(obj: false);
				}
				return;
			}
		}
		if (!isPaused || wasPaused)
		{
			return;
		}
		foreach (Action<bool> onPausedListener2 in m_onPausedListeners)
		{
			onPausedListener2?.Invoke(obj: true);
		}
	}

	private void RefreshVisibleAffectedObjects()
	{
		if (!m_isReady)
		{
			return;
		}
		m_scrollable.ClearVisibleAffectObjects();
		m_visibleAffectedObjects.Clear();
		if (m_list.WidgetItemsCount <= 0)
		{
			return;
		}
		float itemHeightWithBuffer = m_itemHeight + m_itemHeight * (float)m_itemBuffer;
		Vector3 extents = new Vector3(0f, 0f, itemHeightWithBuffer);
		foreach (WidgetInstance item in m_list.WidgetItems)
		{
			m_scrollable.AddVisibleAffectedObject(item.gameObject, extents, m_scrollable.IsObjectVisibleInScrollArea(item.gameObject, extents), UpdateVisibleState);
			m_visibleAffectedObjects.Add(item.gameObject.name, item);
		}
	}

	private void RefreshCurrentVisibleStates()
	{
		if (!m_isReady || m_isPaused)
		{
			return;
		}
		foreach (UIBScrollable.VisibleAffectedObject visualObject in m_scrollable.GetVisibleAffectedObjects())
		{
			UpdateVisibleState(visualObject.Obj, visualObject.Visible);
		}
	}

	private void UpdateVisibleState(GameObject obj, bool visible)
	{
		if (m_isReady && !m_isPaused && m_visibleAffectedObjects.TryGetValue(obj.name, out var widget))
		{
			if (visible)
			{
				widget.TriggerEvent(m_showItemEvent);
			}
			else
			{
				widget.TriggerEvent(m_hideItemEvent);
			}
		}
	}

	private void OnListDataChanged()
	{
		IsUpdatingData = true;
		m_scrollable.SetScrollImmediate(0f);
		RefreshVisibleAffectedObjects();
		m_list.UpdatePositions();
	}

	private void OnListDoneChangingStates(object _)
	{
		if (IsUpdatingData)
		{
			IsUpdatingData = false;
			RefreshCurrentVisibleStates();
		}
	}

	private void OnScrollableReady(UIBScrollable scrollable)
	{
		m_isScrollableReady = true;
		m_scrollable = scrollable;
	}

	private void OnListReady(Listable list)
	{
		m_isListReady = true;
		m_list = list;
	}

	public void RefreshIsChangingState()
	{
		bool isChangingState = IsChangingState;
		IsChangingState = IsUpdatingData;
		if (isChangingState != IsChangingState || m_hasChangedStatesOnce)
		{
			m_hasChangedStatesOnce = true;
			if (m_refreshDelayCoroutine != null)
			{
				StopCoroutine(m_refreshDelayCoroutine);
			}
			if (base.gameObject.activeInHierarchy)
			{
				m_refreshDelayCoroutine = StartCoroutine(RefreshDelay());
			}
			else if (IsChangingState)
			{
				OnStartChangingStates();
			}
			else
			{
				OnDoneChangingStates();
			}
		}
	}

	private IEnumerator RefreshDelay()
	{
		if (IsChangingState)
		{
			OnStartChangingStates();
		}
		else
		{
			int frameCount = 2;
			while (frameCount > 0)
			{
				m_list.UpdatePositions();
				yield return null;
				int num = frameCount - 1;
				frameCount = num;
			}
			OnDoneChangingStates();
		}
		m_refreshDelayCoroutine = null;
	}

	private void OnStartChangingStates()
	{
		foreach (Action startChangingStatesListener in m_startChangingStatesListeners)
		{
			startChangingStatesListener?.Invoke();
		}
	}

	private void OnDoneChangingStates()
	{
		foreach (Action doneChangingStatesListener in m_doneChangingStatesListeners)
		{
			doneChangingStatesListener?.Invoke();
		}
		RefreshCurrentVisibleStates();
	}

	public void RegisterStartChangingState(Action del)
	{
		m_startChangingStatesListeners.Add(del);
	}

	public void UnregisterStartChangingState(Action del)
	{
		m_startChangingStatesListeners.Remove(del);
	}

	public void RegisterDoneChangingState(Action del)
	{
		m_doneChangingStatesListeners.Add(del);
	}

	public void UnregisterDoneChangingState(Action del)
	{
		m_doneChangingStatesListeners.Remove(del);
	}

	public void RegisterOnPausedChanged(Action<bool> del)
	{
		m_onPausedListeners.Add(del);
	}

	public void UnregisterOnPausedChanged(Action<bool> del)
	{
		m_onPausedListeners.Remove(del);
	}
}
