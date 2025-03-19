using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DebuggerGuiWindow : DebuggerGui
{
	public float? collapsedWidth;

	protected Vector2 m_pos;

	protected Vector2 m_size;

	protected Rect m_window;

	protected int m_windowId;

	protected bool m_spaceIsDirty;

	protected bool m_canClose;

	protected bool m_canResize;

	protected Vector2 m_resizingSide;

	protected Vector3 m_resizeClickStart;

	protected Rect m_resizeInitialWindow;

	public const float PADDING = 5f;

	protected const float RESIZE_HANDLE_SIZE = 10f;

	protected const float MOBILE_RESIZE_HANDLE_SIZE = 20f;

	protected static readonly Vector2 SIZE_PADDING = new Vector2(10f, 34f);

	private const float MIN_WINDOW_WIDTH = 100f;

	private const float MIN_WINDOW_HEIGHT = 48f;

	private const string CLOSE_SYMBOL = "✕";

	internal new static string SERIAL_ID = "[W]";

	public Vector2 Position
	{
		get
		{
			return m_pos;
		}
		set
		{
			if (m_pos != value)
			{
				m_pos = value;
				UpdateWindowSize();
				InvokeOnChanged();
			}
		}
	}

	private Vector2 Size
	{
		get
		{
			return m_size;
		}
		set
		{
			Vector2 screen = GetScaledScreen().size;
			value.x = Mathf.Clamp(value.x, 100f, screen.x);
			value.y = Mathf.Clamp(value.y, 48f, screen.y);
			if (m_size != value)
			{
				m_size = value;
				UpdateWindowSize();
				InvokeOnChanged();
			}
		}
	}

	public DebuggerGuiWindow(string title, LayoutGui onGui, bool canClose = true, bool canResize = true)
		: base(title, onGui)
	{
		m_windowId = title.GetHashCode();
		m_canClose = canClose;
		m_canResize = canResize;
		base.OnChanged += HandleChange;
	}

	public bool IsMouseOver()
	{
		Vector3 mousePos = GetScaledMouse();
		return m_window.Contains(mousePos);
	}

	public void ResizeToFit(Vector2 dims)
	{
		Size = dims + SIZE_PADDING;
	}

	public void ResizeToFit(float width, float height)
	{
		ResizeToFit(new Vector2(width, height));
	}

	public Rect GetHeaderRect()
	{
		return new Rect(5f, 5f, m_size.x - 10f, 24f);
	}

	public void Layout()
	{
		Layout(new Rect(Position, Size));
	}

	public override Rect Layout(Rect space)
	{
		if (!m_isShown)
		{
			return space;
		}
		Position = space.min;
		Size = space.size;
		UpdateWindowSize();
		m_window = GUI.Window(m_windowId, m_window, WindowFunction, "");
		ConstrainPosition(m_window);
		if (m_canResize && m_isExpanded)
		{
			Vector3 mousePos = GetScaledMouse();
			float resizeHandleSize = (PlatformSettings.IsMobile() ? 20f : 10f);
			Rect resizeExclusion = m_window;
			resizeExclusion.xMin += resizeHandleSize;
			resizeExclusion.yMin += 5f;
			resizeExclusion.xMax -= resizeHandleSize;
			resizeExclusion.yMax -= resizeHandleSize;
			if (Input.GetMouseButtonDown(0) && m_window.Contains(mousePos) && !resizeExclusion.Contains(mousePos))
			{
				m_resizingSide.x = ((mousePos.x >= resizeExclusion.xMax) ? 1 : 0) + ((mousePos.x <= resizeExclusion.xMin) ? (-1) : 0);
				m_resizingSide.y = ((mousePos.y >= resizeExclusion.yMax) ? 1 : 0) + ((mousePos.y <= resizeExclusion.yMin) ? (-1) : 0);
				m_resizeClickStart = mousePos;
				m_resizeInitialWindow = m_window;
			}
			else if (IsResizing())
			{
				if (Input.GetMouseButton(0))
				{
					Vector2 delta = mousePos - m_resizeClickStart;
					if (m_resizingSide.x < 0f)
					{
						m_window.xMin = Mathf.Min(m_resizeInitialWindow.xMin + delta.x, m_resizeInitialWindow.xMax - 100f);
					}
					else if (m_resizingSide.x > 0f)
					{
						m_window.xMax = Mathf.Max(m_resizeInitialWindow.xMax + delta.x, m_resizeInitialWindow.xMin + 100f);
					}
					if (m_resizingSide.y < 0f)
					{
						m_window.yMin = Mathf.Min(m_resizeInitialWindow.yMin + delta.y, m_resizeInitialWindow.yMax - 48f);
					}
					else if (m_resizingSide.y > 0f)
					{
						m_window.yMax = Mathf.Max(m_resizeInitialWindow.yMax + delta.y, m_resizeInitialWindow.yMin + 48f);
					}
					m_pos = m_window.min;
					m_size = m_window.size;
					InvokeOnChanged();
				}
				if (Input.GetMouseButtonUp(0))
				{
					m_resizingSide = new Vector2(0f, 0f);
				}
			}
		}
		return new Rect(m_window.xMin, m_window.yMax, space.width, space.height - m_window.height);
	}

	private void HandleChange()
	{
		Rect window = m_window;
		UpdateWindowSize();
		if (!window.Equals(m_window))
		{
			m_spaceIsDirty = true;
		}
	}

	private void UpdateWindowSize()
	{
		m_window = new Rect(m_pos.x, m_pos.y, (m_isExpanded || !collapsedWidth.HasValue) ? m_size.x : collapsedWidth.Value, m_isExpanded ? m_size.y : 34f);
	}

	private void WindowFunction(int windowId)
	{
		m_spaceIsDirty = false;
		Rect space = new Rect(5f, 5f, m_window.width - 10f, m_window.height - 10f);
		if (m_canClose && GUI.Button(new Rect(space.xMax - 24f, space.yMin, 24f, 24f), "✕"))
		{
			base.IsShown = false;
		}
		space = LayoutHeader(space);
		if (!m_spaceIsDirty)
		{
			space = LayoutInternal(space);
			if (!IsResizing())
			{
				GUI.DragWindow();
			}
		}
	}

	protected bool IsResizing()
	{
		return m_resizingSide.sqrMagnitude > 0f;
	}

	protected Rect GetScaledScreen()
	{
		return new Rect(0f, 0f, Math.Max(0f, (float)Screen.width / GUI.matrix.lossyScale.x), Math.Max(0f, (float)Screen.height / GUI.matrix.lossyScale.y));
	}

	protected Vector3 GetScaledMouse()
	{
		Vector3 mousePos = Input.mousePosition;
		mousePos.y = (float)Screen.height - mousePos.y;
		mousePos.x /= GUI.matrix.lossyScale.x;
		mousePos.y /= GUI.matrix.lossyScale.y;
		return mousePos;
	}

	private void ConstrainPosition(Rect window)
	{
		Vector2 margin = new Vector2(48f, 24f);
		Rect bounds = GetScaledScreen();
		Position = new Vector2(Mathf.Clamp(window.x, bounds.xMin - window.width + margin.x, bounds.xMax - margin.x), Mathf.Clamp(window.y, bounds.yMin - window.height + margin.y, bounds.yMax - margin.y));
	}

	internal override string SerializeToString()
	{
		string sERIAL_ID = SERIAL_ID;
		Vector2 pos = Position;
		Vector2 size = Size;
		return string.Concat(sERIAL_ID + string.Join("x", new List<string>
		{
			Mathf.RoundToInt(pos.x).ToString(),
			Mathf.RoundToInt(pos.y).ToString(),
			Mathf.RoundToInt(size.x).ToString(),
			Mathf.RoundToInt(size.y).ToString()
		}.ToArray()), base.SerializeToString());
	}

	internal override void DeserializeFromString(string str)
	{
		int idx = str.IndexOf(SERIAL_ID);
		int baseClassIdx = str.IndexOf(DebuggerGui.SERIAL_ID);
		if (idx >= 0)
		{
			idx += SERIAL_ID.Length;
			List<string> rect = new List<string>();
			string rectString = str.Substring(idx, (baseClassIdx > idx) ? (baseClassIdx - idx) : (str.Length - idx));
			rect.AddRange(rectString.Split('x'));
			Vector2 pos = Position;
			Vector2 size = Size;
			if (float.TryParse(rect.ElementAtOrDefault(0), out pos.x) && float.TryParse(rect.ElementAtOrDefault(1), out pos.y))
			{
				Position = pos;
			}
			if (m_canResize && float.TryParse(rect.ElementAtOrDefault(2), out size.x) && float.TryParse(rect.ElementAtOrDefault(3), out size.y))
			{
				Size = size;
			}
			ConstrainPosition(m_window);
		}
		if (baseClassIdx >= 0)
		{
			base.DeserializeFromString(str.Substring(baseClassIdx));
		}
	}
}
