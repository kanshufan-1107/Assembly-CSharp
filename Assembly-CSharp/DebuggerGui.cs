using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DebuggerGui
{
	public delegate Rect LayoutGui(Rect space);

	protected bool m_canCollapse = true;

	protected bool m_isShown = true;

	protected bool m_isExpanded = true;

	protected LayoutGui m_OnGui;

	public const float HEADER_SIZE = 24f;

	private const char DOWN_ARROW = '▼';

	private const char RIGHT_ARROW = '▶';

	internal static string SERIAL_ID = "[DG]";

	public string Title { get; set; }

	public bool IsExpanded
	{
		get
		{
			return m_isExpanded;
		}
		set
		{
			if (m_isExpanded != value)
			{
				m_isExpanded = value;
				InvokeOnChanged();
			}
		}
	}

	public bool IsShown
	{
		get
		{
			return m_isShown;
		}
		set
		{
			if (m_isShown != value)
			{
				m_isShown = value;
				InvokeOnChanged();
			}
		}
	}

	public event Action OnChanged;

	public DebuggerGui(string title, LayoutGui onGui, bool canCollapse = true, bool drawWindow = false)
	{
		Title = title;
		m_canCollapse = canCollapse;
		m_OnGui = onGui;
	}

	public virtual Rect Layout(Rect space)
	{
		if (!m_isShown)
		{
			return space;
		}
		space = LayoutHeader(space);
		return LayoutInternal(space);
	}

	protected Rect LayoutHeader(Rect space)
	{
		Rect header = new Rect(space.x, space.y, space.width, 24f);
		if (m_canCollapse && GUI.Button(new Rect(header.xMin, header.yMin, header.height, header.height), m_isExpanded ? '▼'.ToString() : '▶'.ToString()))
		{
			IsExpanded = !m_isExpanded;
		}
		GUI.Label(new Rect(header.xMin + header.height + 5f, header.yMin, header.width - header.height * 2f - 5f, header.height), Title);
		space.yMin += header.height;
		return space;
	}

	protected Rect LayoutInternal(Rect space)
	{
		if (m_isExpanded && m_OnGui != null)
		{
			return m_OnGui(space);
		}
		return space;
	}

	protected void InvokeOnChanged()
	{
		if (this.OnChanged != null)
		{
			this.OnChanged();
		}
	}

	internal virtual string SerializeToString()
	{
		return string.Concat(SERIAL_ID + (IsShown ? "S" : "H"), IsExpanded ? "E" : "C");
	}

	internal virtual void DeserializeFromString(string str)
	{
		int idx = str.IndexOf(SERIAL_ID);
		if (idx >= 0)
		{
			idx += SERIAL_ID.Length;
			IsShown = str.ElementAtOrDefault(idx) == 'S';
			IsExpanded = str.ElementAtOrDefault(idx + 1) == 'E';
		}
	}

	public static void SaveConfig(List<DebuggerGui> guis)
	{
		List<string> configs = new List<string>();
		foreach (DebuggerGui gui in guis)
		{
			string serialized = gui.SerializeToString();
			configs.Add(serialized);
		}
		string configStr = string.Join(";", configs.ToArray());
		Options.Get().SetString(Option.HUD_CONFIG, configStr);
	}

	public static void LoadConfig(List<DebuggerGui> guis)
	{
		string configStr = Options.Get().GetString(Option.HUD_CONFIG);
		if (!string.IsNullOrEmpty(configStr))
		{
			List<string> configs = new List<string>();
			configs.AddRange(configStr.Split(';'));
			int i = 0;
			for (int n = Math.Min(guis.Count, configs.Count); i < n; i++)
			{
				guis[i].DeserializeFromString(configs[i]);
			}
		}
	}
}
