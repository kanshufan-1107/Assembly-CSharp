using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Blizzard.T5.Core;
using Blizzard.T5.Core.Utils;
using Blizzard.T5.Logging;
using Hearthstone;
using UnityEngine;

public class PegUIElement : MonoBehaviour
{
	public enum InteractionState
	{
		None,
		Out,
		Over,
		Down,
		Up,
		Disabled
	}

	public enum PegUILogLevel
	{
		NONE,
		PRESS,
		ALL_EVENTS,
		HIT_TEST
	}

	public delegate void UIElementPressAction(PegUIElement pressedElement);

	private MeshFilter m_meshFilter;

	private MeshRenderer m_renderer;

	private Map<UIEventType, List<UIEvent.Handler>> m_eventListeners = new Map<UIEventType, List<UIEvent.Handler>>();

	private bool m_enabled = true;

	private HashSet<UIEventType> m_disabledEventTypes = new HashSet<UIEventType>();

	private bool m_focused;

	private bool m_receiveReleaseWithoutMouseDown;

	private object m_data;

	private Vector3 m_originalLocalPosition;

	private InteractionState m_interactionState;

	private Vector3 m_dragTolerance = Vector3.one * 40f;

	private PegCursor.Mode m_cursorDownOverride = PegCursor.Mode.NONE;

	private PegCursor.Mode m_cursorOverOverride = PegCursor.Mode.NONE;

	public bool DoubleClickEnabled { get; private set; }

	public float SetEnabledLastCallTime { get; private set; }

	public static event UIElementPressAction OnUIElementPressed;

	protected virtual void Awake()
	{
		DoubleClickEnabled = DoubleClickEnabled || HasOverriddenDoubleClick();
	}

	protected virtual void OnDestroy()
	{
	}

	protected virtual void OnOver(InteractionState oldState)
	{
	}

	protected virtual void OnOut(InteractionState oldState)
	{
	}

	protected virtual void OnPress()
	{
	}

	protected virtual void OnTap()
	{
	}

	protected virtual void OnRelease()
	{
	}

	protected virtual void OnReleaseAll(bool mouseIsOver)
	{
	}

	protected virtual void OnDrag()
	{
	}

	protected virtual void OnDoubleClick()
	{
	}

	protected virtual void OnRightClick()
	{
	}

	protected virtual void OnHold()
	{
	}

	public void SetInteractionState(InteractionState state)
	{
		m_interactionState = state;
	}

	public virtual void TriggerOver()
	{
		if (m_enabled && !m_focused && IsEnabled(UIEventType.ROLLOVER))
		{
			PrintLog("OVER", PegUILogLevel.ALL_EVENTS);
			m_focused = true;
			InteractionState oldState = m_interactionState;
			m_interactionState = InteractionState.Over;
			OnOver(oldState);
			DispatchEvent(new UIEvent(UIEventType.ROLLOVER, this));
		}
	}

	public virtual void TriggerOut()
	{
		if (m_enabled && IsEnabled(UIEventType.ROLLOUT))
		{
			PrintLog("OUT", PegUILogLevel.ALL_EVENTS);
			m_focused = false;
			InteractionState oldState = m_interactionState;
			m_interactionState = InteractionState.Out;
			OnOut(oldState);
			DispatchEvent(new UIEvent(UIEventType.ROLLOUT, this));
		}
	}

	public virtual void TriggerPress()
	{
		if (m_enabled && IsEnabled(UIEventType.PRESS))
		{
			PrintLog("PRESS", PegUILogLevel.PRESS);
			m_focused = true;
			m_interactionState = InteractionState.Down;
			OnPress();
			if (PegUIElement.OnUIElementPressed != null)
			{
				PegUIElement.OnUIElementPressed(this);
			}
			DispatchEvent(new UIEvent(UIEventType.PRESS, this));
		}
	}

	public virtual void TriggerTap()
	{
		if (m_enabled && IsEnabled(UIEventType.TAP))
		{
			PrintLog("TAP", PegUILogLevel.ALL_EVENTS, printOnScreen: true);
			m_interactionState = InteractionState.Up;
			OnTap();
			DispatchEvent(new UIEvent(UIEventType.TAP, this));
		}
	}

	public virtual void TriggerRelease()
	{
		if (m_enabled && IsEnabled(UIEventType.RELEASE))
		{
			PrintLog("RELEASE", PegUILogLevel.ALL_EVENTS);
			m_interactionState = InteractionState.Up;
			if (!IsScrolling())
			{
				OnRelease();
				DispatchEvent(new UIEvent(UIEventType.RELEASE, this));
			}
		}
	}

	public void TriggerReleaseAll(bool mouseIsOver)
	{
		if (m_enabled && IsEnabled(UIEventType.RELEASE))
		{
			m_interactionState = InteractionState.Up;
			OnReleaseAll(mouseIsOver);
			DispatchEvent(new UIReleaseAllEvent(mouseIsOver, this));
		}
	}

	public void TriggerDrag()
	{
		if (m_enabled && IsEnabled(UIEventType.DRAG))
		{
			PrintLog("DRAG", PegUILogLevel.ALL_EVENTS);
			m_interactionState = InteractionState.Down;
			OnDrag();
			DispatchEvent(new UIEvent(UIEventType.DRAG, this));
		}
	}

	public void TriggerHold()
	{
		if (m_enabled && IsEnabled(UIEventType.HOLD))
		{
			PrintLog("HOLD", PegUILogLevel.ALL_EVENTS);
			m_interactionState = InteractionState.Down;
			OnHold();
			DispatchEvent(new UIEvent(UIEventType.HOLD, this));
		}
	}

	public void TriggerDoubleClick()
	{
		if (m_enabled && IsEnabled(UIEventType.DOUBLECLICK))
		{
			PrintLog("DCLICK", PegUILogLevel.ALL_EVENTS);
			m_interactionState = InteractionState.Down;
			OnDoubleClick();
			DispatchEvent(new UIEvent(UIEventType.DOUBLECLICK, this));
		}
	}

	public void TriggerRightClick()
	{
		if (m_enabled && IsEnabled(UIEventType.RIGHTCLICK))
		{
			PrintLog("RCLICK", PegUILogLevel.ALL_EVENTS);
			OnRightClick();
			DispatchEvent(new UIEvent(UIEventType.RIGHTCLICK, this));
		}
	}

	public void SetDragTolerance(float newTolerance)
	{
		SetDragTolerance(Vector3.one * newTolerance);
	}

	public void SetDragTolerance(Vector3 newTolerance)
	{
		m_dragTolerance = newTolerance;
	}

	public Vector3 GetDragTolerance()
	{
		return m_dragTolerance;
	}

	public virtual bool AddEventListener(UIEventType type, UIEvent.Handler handler)
	{
		DoubleClickEnabled |= type == UIEventType.DOUBLECLICK;
		if (!m_eventListeners.TryGetValue(type, out var listeners))
		{
			listeners = new List<UIEvent.Handler>();
			m_eventListeners.Add(type, listeners);
			listeners.Add(handler);
			return true;
		}
		if (listeners.Contains(handler))
		{
			return false;
		}
		listeners.Add(handler);
		return true;
	}

	public virtual bool RemoveEventListener(UIEventType type, UIEvent.Handler handler)
	{
		if (!m_eventListeners.TryGetValue(type, out var listeners))
		{
			return false;
		}
		return listeners.Remove(handler);
	}

	public void ClearEventListeners()
	{
		m_eventListeners.Clear();
	}

	public bool HasEventListener(UIEventType type)
	{
		if (!m_eventListeners.TryGetValue(type, out var listeners))
		{
			return false;
		}
		return listeners.Count > 0;
	}

	public virtual void SetEnabled(bool enabled, bool isInternal = false)
	{
		if (!isInternal)
		{
			SetEnabledLastCallTime = Time.realtimeSinceStartup;
		}
		if (enabled)
		{
			PrintLog("ENABLE", PegUILogLevel.ALL_EVENTS);
		}
		else
		{
			PrintLog("DISABLE", PegUILogLevel.ALL_EVENTS);
		}
		bool enabledChanged = m_enabled != enabled;
		if (!enabled && enabledChanged)
		{
			DispatchEvent(new UIEvent(UIEventType.DISABLE, this));
		}
		m_enabled = enabled;
		if (enabled && enabledChanged)
		{
			DispatchEvent(new UIEvent(UIEventType.ENABLE, this));
		}
		if (!m_enabled)
		{
			m_focused = false;
		}
	}

	public bool IsEnabled()
	{
		return m_enabled;
	}

	public void SetEnabled(UIEventType eventType, bool enabled)
	{
		if (enabled)
		{
			m_disabledEventTypes.Remove(eventType);
		}
		else
		{
			m_disabledEventTypes.Add(eventType);
		}
	}

	public bool IsEnabled(UIEventType eventType)
	{
		return !m_disabledEventTypes.Contains(eventType);
	}

	public void SetReceiveReleaseWithoutMouseDown(bool receiveReleaseWithoutMouseDown)
	{
		m_receiveReleaseWithoutMouseDown = receiveReleaseWithoutMouseDown;
	}

	public bool GetReceiveReleaseWithoutMouseDown()
	{
		return m_receiveReleaseWithoutMouseDown;
	}

	public bool GetReceiveOverWithMouseDown()
	{
		if (!UniversalInputManager.Get().IsTouchMode())
		{
			return false;
		}
		return true;
	}

	public InteractionState GetInteractionState()
	{
		return m_interactionState;
	}

	public void SetData(object data)
	{
		m_data = data;
	}

	public object GetData()
	{
		return m_data;
	}

	public void SetOriginalLocalPosition()
	{
		SetOriginalLocalPosition(base.transform.localPosition);
	}

	public void SetOriginalLocalPosition(Vector3 pos)
	{
		m_originalLocalPosition = pos;
	}

	public Vector3 GetOriginalLocalPosition()
	{
		return m_originalLocalPosition;
	}

	public void SetCursorDown(PegCursor.Mode mode)
	{
		m_cursorDownOverride = mode;
	}

	public PegCursor.Mode GetCursorDown()
	{
		return m_cursorDownOverride;
	}

	public void SetCursorOver(PegCursor.Mode mode)
	{
		m_cursorOverOverride = mode;
	}

	public PegCursor.Mode GetCursorOver()
	{
		return m_cursorOverOverride;
	}

	private void DispatchEvent(UIEvent e)
	{
		if (m_eventListeners.TryGetValue(e.GetEventType(), out var listeners))
		{
			UIEvent.Handler[] array = listeners.ToArray();
			for (int i = 0; i < array.Length; i++)
			{
				array[i](e);
			}
		}
	}

	private bool HasOverriddenDoubleClick()
	{
		Type type = GetType();
		Type topType = typeof(PegUIElement);
		topType.GetMethod("OnDoubleClick", BindingFlags.Instance | BindingFlags.NonPublic);
		type.GetMethod("OnDoubleClick", BindingFlags.Instance | BindingFlags.NonPublic);
		return GeneralUtils.IsOverriddenMethod(type, topType, "OnDoubleClick");
	}

	private void PrintLog(string evt, PegUILogLevel logLevel, bool printOnScreen = false)
	{
		if (!(this == null) && !(base.gameObject == null) && HearthstoneApplication.IsInternal() && Options.Get().GetInt(Option.PEGUI_DEBUG) >= (int)logLevel)
		{
			string hierarchyPath = DebugUtils.GetHierarchyPath(base.gameObject, '/');
			string message = string.Format("{0,-7} {1}", evt + ":", hierarchyPath);
			Log.All.PrintInfo(message);
			if (printOnScreen)
			{
				Log.All.ForceScreenPrint(Blizzard.T5.Logging.LogLevel.Info, verbose: true, message);
			}
		}
	}

	private bool IsScrolling()
	{
		return GetComponentsInParent<UIBScrollable.IContent>().Any((UIBScrollable.IContent scrollable) => scrollable.Scrollable.IsTouchDragging());
	}
}
