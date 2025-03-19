using System.Collections.Generic;
using System.Linq;
using System.Text;
using Blizzard.T5.Logging;
using UnityEngine;

public class LoggerDebugWindow : DebuggerGuiWindow
{
	public class LogEntry
	{
		public LogLevel category;

		public string text;

		public override string ToString()
		{
			return $"[{category}] {text}";
		}
	}

	public LayoutGui CustomLayout;

	internal new static string SERIAL_ID = "[LOG]";

	private Vector2 m_GUISize;

	private QueueList<LogEntry> m_entries;

	private Dictionary<LogLevel, int> m_levels;

	private string m_title;

	private GUIStyle m_textStyle;

	private List<object> m_categories;

	private Dictionary<object, bool> m_categoryToggles;

	private Vector2 m_scrollPosition;

	private bool m_autoScroll = true;

	private int m_alertCount;

	public string FilterString { get; set; }

	public LoggerDebugWindow(string title, Vector2 guiSize, IEnumerable<object> categories)
		: base(title, null)
	{
		m_title = title;
		m_categories = categories.ToList();
		m_GUISize = guiSize;
		m_OnGui = LayoutMessages;
		m_categoryToggles = new Dictionary<object, bool>();
		m_entries = new QueueList<LogEntry>();
		m_levels = new Dictionary<LogLevel, int>();
		m_textStyle = new GUIStyle("box")
		{
			fontSize = 17,
			alignment = TextAnchor.UpperLeft,
			wordWrap = true,
			clipping = TextClipping.Clip,
			stretchWidth = true
		};
	}

	public void AddEntry(LogEntry entry, bool addAlert = false)
	{
		if (entry.text.Length > 8192)
		{
			entry.text = entry.text.Substring(0, 8192);
		}
		m_entries.Enqueue(entry);
		if (m_levels.ContainsKey(entry.category))
		{
			m_levels[entry.category]++;
		}
		else
		{
			m_levels.Add(entry.category, 1);
		}
		if (addAlert && AreLogsDisplayed(entry.category))
		{
			m_alertCount++;
			if (m_alertCount == 1)
			{
				base.IsShown = true;
				base.IsExpanded = true;
			}
		}
	}

	public void Clear()
	{
		m_entries.Clear();
		m_levels.Clear();
		m_alertCount = 0;
	}

	public int GetCount(LogLevel category)
	{
		m_levels.TryGetValue(category, out var value);
		return value;
	}

	public IEnumerable<LogEntry> GetEntries()
	{
		return m_entries;
	}

	public void ToggleLogsDisplay(object category, bool display)
	{
		m_categoryToggles[category] = display;
		InvokeOnChanged();
	}

	public bool AreLogsDisplayed(object category)
	{
		if (category == null || !m_categoryToggles.TryGetValue(category, out var isShown))
		{
			return true;
		}
		return isShown;
	}

	public Rect LayoutLog(Rect space)
	{
		GUI.skin.settings.selectionColor = Color.blue;
		Rect displaySpace = space;
		Rect textSpan = new Rect(0f, 0f, space.width - 20f, 0f);
		string[] terms = (string.IsNullOrEmpty(FilterString) ? null : FilterString.ToLowerInvariant().Split(' '));
		List<int> entryIndices = new List<int>();
		new StringBuilder();
		for (int i = 0; i < m_entries.Count; i++)
		{
			if (!AreLogsDisplayed(m_entries[i].category))
			{
				continue;
			}
			if (terms != null && terms.Count() > 0)
			{
				bool matches = true;
				string text = m_entries[i].text.ToLowerInvariant();
				string[] array = terms;
				foreach (string term in array)
				{
					if (!text.ToLowerInvariant().Contains(term))
					{
						matches = false;
						break;
					}
				}
				if (!matches)
				{
					continue;
				}
			}
			entryIndices.Add(i);
			textSpan.height += GetLogEntryHeight(m_entries[i], textSpan.width);
		}
		float scrollableHeight = textSpan.height - displaySpace.height;
		if (m_autoScroll)
		{
			m_scrollPosition.y = scrollableHeight;
		}
		m_scrollPosition = GUI.BeginScrollView(displaySpace, m_scrollPosition, textSpan, alwaysShowHorizontal: false, alwaysShowVertical: true);
		m_autoScroll = m_scrollPosition.y >= scrollableHeight;
		Rect guiRect = new Rect(0f, 0f, textSpan.width - 60f, 0f);
		for (int k = 0; k < entryIndices.Count; k++)
		{
			int idx = entryIndices[k];
			LogEntry entry = m_entries.ElementAtOrDefault(idx);
			guiRect.height = GetLogEntryHeight(entry, textSpan.width);
			GUI.TextArea(guiRect, entry.text, m_textStyle);
			if (GUI.Button(new Rect(textSpan.width - 55f, guiRect.y, 55f, guiRect.height), "COPY"))
			{
				string text2 = entry.text;
				string text3 = text2.Substring(text2.IndexOf(']') + 1);
				GUIUtility.systemCopyBuffer = text3.Substring(text3.IndexOf(']') + 2);
			}
			guiRect.yMin += guiRect.height;
		}
		GUI.EndScrollView();
		space.yMin = space.yMax;
		return space;
	}

	private Rect LayoutMessages(Rect space)
	{
		if (CustomLayout != null)
		{
			return CustomLayout(space);
		}
		return LayoutLog(space);
	}

	private float GetLogEntryHeight(LogEntry entry, float width)
	{
		return m_textStyle.CalcHeight(new GUIContent(entry.text), width) + 5f;
	}

	internal override string SerializeToString()
	{
		string str = SERIAL_ID;
		List<string> toggles = new List<string>();
		foreach (object category in m_categories)
		{
			toggles.Add(AreLogsDisplayed(category) ? "1" : "0");
		}
		str += string.Join(",", toggles.ToArray());
		return str + base.SerializeToString();
	}

	internal override void DeserializeFromString(string str)
	{
		int idx = str.IndexOf(SERIAL_ID);
		int baseClassIdx = str.IndexOf(DebuggerGuiWindow.SERIAL_ID);
		if (idx >= 0)
		{
			idx += SERIAL_ID.Length;
			List<string> toggles = str.Substring(idx, (baseClassIdx > idx) ? (baseClassIdx - idx) : (str.Length - idx)).Split(',').ToList();
			for (int categoryIdx = 0; categoryIdx < m_categories.Count; categoryIdx++)
			{
				string val = toggles.ElementAtOrDefault(categoryIdx);
				object category = m_categories[categoryIdx];
				m_categoryToggles[category] = val != "0";
			}
		}
		if (baseClassIdx >= 0)
		{
			base.DeserializeFromString(str.Substring(baseClassIdx));
		}
	}
}
