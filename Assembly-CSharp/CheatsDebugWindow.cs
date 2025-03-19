using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CheatsDebugWindow : DebuggerGuiWindow
{
	private class CheatEntry
	{
		public virtual string SearchString => Title.ToLowerInvariant();

		public string Title { get; protected set; }

		public CheatEntry(string title)
		{
			Title = title;
		}
	}

	private class CheatCategory : CheatEntry
	{
		public List<CheatEntry> children = new List<CheatEntry>();

		public override string SearchString => Path.ToLowerInvariant();

		public string Path { get; protected set; }

		public int Depth
		{
			get
			{
				int depth = 0;
				int idx = Path.IndexOf(':');
				while (idx > 0 && idx < Path.Length)
				{
					depth++;
					idx = Path.IndexOf(':', idx + 1);
				}
				return depth;
			}
		}

		public CheatCategory(string path)
			: base("")
		{
			Path = path;
			int end = path.LastIndexOf(':');
			base.Title = ((end > 0) ? path.Substring(end + 1) : path);
		}

		public static List<string> GetLineage(string fullPath)
		{
			List<string> lineage = new List<string>();
			for (int idx = fullPath.IndexOf(':'); idx > 0; idx = fullPath.IndexOf(':', idx + 1))
			{
				lineage.Add(fullPath.Substring(0, idx));
			}
			lineage.Add(fullPath);
			return lineage;
		}
	}

	private class CheatCommand : CheatEntry
	{
		public string example = "";

		public string description = "";

		public string args = "";

		public CheatCategory parent;

		public override string SearchString => (base.Title + " " + description).ToLowerInvariant();

		public CheatCommand(string name)
			: base(name)
		{
			CheatMgr cheatMgr = CheatMgr.Get();
			if (cheatMgr != null)
			{
				cheatMgr.cheatArgs.TryGetValue(name, out args);
				cheatMgr.cheatDesc.TryGetValue(name, out description);
				cheatMgr.cheatExamples.TryGetValue(name, out example);
			}
		}
	}

	private class CheatOption : CheatEntry
	{
		public Option option;

		public CheatCategory parent;

		public CheatOption(string title, Option option)
			: base(title)
		{
			this.option = option;
		}
	}

	private Vector2 m_GUISize;

	private CheatCommand m_currentlyDisplayedCheat;

	private string m_cheatSearchTerm = "";

	private Vector2 m_cheatScrollPosition;

	private Dictionary<string, CheatCategory> m_categories;

	private GUIStyle m_labelStyle;

	public CheatsDebugWindow(Vector2 guiSize)
		: base("Cheats", null)
	{
		m_GUISize = guiSize;
		m_OnGui = LayoutCheats;
		m_labelStyle = new GUIStyle("box")
		{
			alignment = TextAnchor.MiddleLeft,
			wordWrap = false,
			clipping = TextClipping.Clip,
			stretchWidth = false
		};
		Rect bounds = GetScaledScreen();
		ResizeToFit(bounds.width / 2f, bounds.height / 2f);
	}

	private void InitializeCheatsAsNecessary()
	{
		if (m_categories != null)
		{
			return;
		}
		CheatMgr cheatMgr = CheatMgr.Get();
		Options options = Options.Get();
		if (cheatMgr == null || options == null || cheatMgr.GetCheatCommands().Count() == 0)
		{
			return;
		}
		m_categories = new Dictionary<string, CheatCategory>();
		CheatCategory clientOptions = new CheatCategory("options");
		m_categories.Add(clientOptions.Path, clientOptions);
		foreach (KeyValuePair<Option, string> pair in options.GetClientOptions())
		{
			string value = pair.Value;
			Option option = pair.Key;
			string optionType = options.GetOptionType(option).ToString();
			if (optionType.StartsWith("System."))
			{
				optionType = optionType.Remove(0, 7);
			}
			string path = clientOptions.Path + ":" + optionType;
			CheatCategory category = null;
			if (!m_categories.TryGetValue(path, out category))
			{
				category = new CheatCategory(path);
				m_categories.Add(path, category);
			}
			CheatOption cheat = new CheatOption(value, option);
			cheat.parent = category;
			category.children.Add(cheat);
		}
		foreach (string cheat2 in cheatMgr.GetCheatCommands())
		{
			string path2 = cheatMgr.GetCheatCategory(cheat2);
			CheatCategory category2 = null;
			if (!m_categories.TryGetValue(path2, out category2) || category2 == null)
			{
				category2 = new CheatCategory(path2);
				m_categories.Add(path2, category2);
			}
			CheatCommand command = new CheatCommand(cheat2);
			command.parent = category2;
			category2.children.Add(command);
		}
	}

	private Rect LayoutCheats(Rect space)
	{
		InitializeCheatsAsNecessary();
		if (m_categories == null)
		{
			return space;
		}
		if (m_currentlyDisplayedCheat == null)
		{
			space = LayoutFilteredCheats(space);
		}
		else
		{
			if (GUI.Button(new Rect(space.x, space.y, m_GUISize.x, m_GUISize.y), "Back"))
			{
				m_currentlyDisplayedCheat = null;
				return space;
			}
			if (GUI.Button(new Rect(space.xMax - m_GUISize.x, space.y, m_GUISize.x, m_GUISize.y), "Hide Console"))
			{
				CheatMgr.Get().HideConsole();
				return space;
			}
			space.yMin += m_GUISize.y;
			string label = m_currentlyDisplayedCheat.Title;
			if (!string.IsNullOrEmpty(m_currentlyDisplayedCheat.args))
			{
				label += $" {m_currentlyDisplayedCheat.args}";
			}
			GUI.Box(new Rect(space.xMin, space.yMin, space.width, m_GUISize.y), label, m_labelStyle);
			space.yMin += 1.1f * m_GUISize.y;
			if (!string.IsNullOrEmpty(m_currentlyDisplayedCheat.description))
			{
				GUI.Box(new Rect(space.xMin, space.yMin, space.width, m_GUISize.y), m_currentlyDisplayedCheat.description, m_labelStyle);
				space.yMin += 1.1f * m_GUISize.y;
			}
			if (!string.IsNullOrEmpty(m_currentlyDisplayedCheat.example))
			{
				GUI.Box(new Rect(space.xMin, space.yMin, space.width, m_GUISize.y), $"Example: {m_currentlyDisplayedCheat.example}", m_labelStyle);
				space.yMin += 1.1f * m_GUISize.y;
			}
			GUI.Label(new Rect(space.min, m_GUISize), "History:");
			space.yMin += m_GUISize.y;
			string history = Options.Get().GetOption(Option.CHEAT_HISTORY).ToString();
			string cheatStart = ";" + m_currentlyDisplayedCheat.Title;
			for (int searchIdx = history.IndexOf(m_currentlyDisplayedCheat.Title); searchIdx >= 0; searchIdx = history.IndexOf(cheatStart, searchIdx + cheatStart.Length))
			{
				int startIdx = history.IndexOf(m_currentlyDisplayedCheat.Title, searchIdx);
				int endIdx = history.IndexOf(';', startIdx);
				string useCase = null;
				useCase = ((endIdx <= 0) ? history.Substring(startIdx) : history.Substring(startIdx, endIdx - startIdx));
				if (GUI.Button(new Rect(space.xMin, space.yMin, space.width, m_GUISize.y), useCase, m_labelStyle))
				{
					CheatMgr.Get().ShowConsole();
					UniversalInputManager.Get().SetInputText(useCase, moveCursorToEnd: true);
				}
				space.yMin += m_GUISize.y;
			}
		}
		return space;
	}

	private Rect LayoutFilteredCheats(Rect space)
	{
		float rowSize = m_GUISize.y;
		GUI.Label(new Rect(space.xMin + 10f, space.yMin + 5f, rowSize * 2f, rowSize), "Filter:");
		Rect textFieldRect = new Rect(space.xMin + rowSize * 2f, space.yMin, 0f, m_GUISize.y);
		textFieldRect.xMax = space.xMax;
		m_cheatSearchTerm = GUI.TextField(textFieldRect, m_cheatSearchTerm);
		space.yMin += rowSize;
		List<CheatEntry> entries = CollectCheats(m_cheatSearchTerm);
		Rect scrollPort = space;
		float margins = (PlatformSettings.IsMobile() ? 20f : 10f) - 5f;
		scrollPort.xMin += margins;
		scrollPort.xMax -= margins;
		scrollPort.yMax -= margins;
		Rect scrollView = new Rect(0f, 0f, scrollPort.width - 18f, (float)entries.Count * rowSize);
		m_cheatScrollPosition = GUI.BeginScrollView(scrollPort, m_cheatScrollPosition, scrollView, alwaysShowHorizontal: false, alwaysShowVertical: true);
		float top = 0f;
		foreach (CheatEntry entry in entries)
		{
			Rect rect = new Rect(0f, top, scrollView.width, rowSize);
			if (entry is CheatCategory)
			{
				CheatCategory category = entry as CheatCategory;
				rect.xMin += (float)category.Depth * 15f;
				GUI.Label(rect, category.Title);
			}
			else
			{
				int depth = 0;
				string buttonLabel = "";
				string description = "";
				Action buttonAction = null;
				if (entry is CheatOption)
				{
					CheatOption cheatOption = entry as CheatOption;
					string optionName = cheatOption.Title;
					object defaultValue = null;
					Option option = cheatOption.option;
					OptionDataTables.s_defaultsMap.TryGetValue(option, out defaultValue);
					depth = cheatOption.parent.Depth + 1;
					if (Options.Get().GetOptionType(option) == typeof(bool))
					{
						buttonLabel = string.Format("{0}={1}", optionName, Options.Get().GetBool(option) ? "1" : "0");
						buttonAction = delegate
						{
							Options.Get().SetBool(option, !Options.Get().GetBool(option));
						};
					}
					else
					{
						buttonLabel = optionName;
						buttonAction = delegate
						{
							CheatMgr.Get().ShowConsole();
							UniversalInputManager.Get().SetInputText($"set {optionName} ", moveCursorToEnd: true);
						};
					}
					object currentValue = Options.Get().GetOption(option);
					description = $"={currentValue}";
					if (defaultValue != null)
					{
						description += string.Format(" (default={1})", currentValue, defaultValue);
					}
				}
				else if (entry is CheatCommand)
				{
					CheatCommand command = entry as CheatCommand;
					depth = command.parent.Depth + 1;
					buttonLabel = command.Title;
					buttonAction = delegate
					{
						CheatMgr.Get().ShowConsole();
						UniversalInputManager.Get().SetInputText(command.Title, moveCursorToEnd: true);
						m_currentlyDisplayedCheat = command;
					};
					description = command.description;
				}
				rect.xMin += (float)depth * 15f;
				Rect buttonRect = new Rect(rect.xMin, rect.yMin, 200f, rowSize);
				if (GUI.Button(buttonRect, buttonLabel))
				{
					buttonAction?.Invoke();
				}
				if (!string.IsNullOrEmpty(description))
				{
					GUI.Box(new Rect(buttonRect.xMax, rect.yMin, rect.xMax - buttonRect.xMax, rowSize), description, m_labelStyle);
				}
			}
			top += rowSize;
		}
		GUI.EndScrollView();
		space.yMin = scrollPort.yMax;
		return space;
	}

	private List<CheatEntry> CollectCheats(string filter)
	{
		List<string> list = m_categories.Keys.ToList();
		list.Sort();
		List<CheatEntry> entries = new List<CheatEntry>();
		string[] searchTerms = filter.ToLowerInvariant().Split(' ');
		foreach (string path in list)
		{
			CheatCategory category = m_categories[path];
			bool addLineage = false;
			List<CheatEntry> addEntries = new List<CheatEntry>();
			if (CheatMatchesFilter(category, searchTerms))
			{
				addLineage = true;
				addEntries.AddRange(category.children);
			}
			else
			{
				foreach (CheatEntry entry in category.children)
				{
					if (CheatMatchesFilter(entry, searchTerms))
					{
						addLineage = true;
						addEntries.Add(entry);
					}
				}
			}
			if (addLineage)
			{
				foreach (string nodePath in CheatCategory.GetLineage(category.Path))
				{
					CheatCategory catNode = null;
					if (m_categories.TryGetValue(nodePath, out catNode) && !entries.Contains(catNode))
					{
						entries.Add(catNode);
					}
				}
			}
			foreach (CheatEntry entry2 in addEntries)
			{
				entries.Add(entry2);
			}
		}
		return entries;
	}

	private bool CheatMatchesFilter(CheatEntry cheat, string[] terms)
	{
		if (terms.Count() == 0)
		{
			return true;
		}
		string search = cheat?.SearchString;
		if (string.IsNullOrEmpty(search))
		{
			return false;
		}
		foreach (string term in terms)
		{
			if (!search.Contains(term))
			{
				return false;
			}
		}
		return true;
	}
}
