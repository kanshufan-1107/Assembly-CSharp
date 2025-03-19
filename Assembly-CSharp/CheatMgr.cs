using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Blizzard.T5.Core;
using Blizzard.T5.Jobs;
using Blizzard.T5.Services;
using Hearthstone;
using UnityEngine;

public class CheatMgr : IService
{
	public delegate bool ProcessCheatCallback(string func, string[] args, string rawArgs);

	public delegate bool ProcessCheatAutofillCallback(string func, string[] args, string rawArgs, AutofillData autofillData);

	private Map<string, List<Delegate>> m_funcMap = new Map<string, List<Delegate>>();

	private Map<string, string> m_cheatAlias = new Map<string, string>();

	private Map<string, string> m_cheatDesc = new Map<string, string>();

	private Map<string, string> m_cheatArgs = new Map<string, string>();

	private Map<string, string> m_cheatExamples = new Map<string, string>();

	private Map<string, int> m_cheatCategoryIndex = new Map<string, int>();

	private List<string> m_categoryList = new List<string>();

	private int m_lastRegisteredCategoryIndex = -1;

	private List<string> m_cheatHistory;

	private int m_cheatHistoryIndex = -1;

	private string m_cheatTextBeforeScrollingThruHistory;

	private string m_cheatTextBeforeAutofill;

	private int m_autofillMatchIndex = -1;

	private string m_lastAutofillParamFunc;

	private string m_lastAutofillParamPrefix;

	private string m_lastAutofillParamMatch;

	private const string DEFAULT_CATEGORY = "other";

	private const int MAX_HISTORY_LINES = 25;

	private const float CHEAT_CONSOLE_PADDING = 1f;

	private const float CHEAT_CONSOLE_HEIGHT = 30f;

	private bool m_ignoreNextConsoleKeypress;

	private bool m_closingConsole;

	private GameObject m_sceneObject;

	private static CheatMgr s_instance;

	public Map<string, string> cheatDesc => m_cheatDesc;

	public Map<string, string> cheatArgs => m_cheatArgs;

	public Map<string, string> cheatExamples => m_cheatExamples;

	private GameObject SceneObject
	{
		get
		{
			if (m_sceneObject == null)
			{
				m_sceneObject = new GameObject("CheatMgrSceneObject", typeof(HSDontDestroyOnLoad));
			}
			return m_sceneObject;
		}
	}

	public IEnumerator<IAsyncJobResult> Initialize(ServiceLocator serviceLocator)
	{
		m_cheatHistory = new List<string>();
		yield break;
	}

	public Type[] GetDependencies()
	{
		return null;
	}

	public void Shutdown()
	{
		s_instance = null;
	}

	public static CheatMgr Get()
	{
		if (s_instance == null)
		{
			s_instance = ServiceManager.Get<CheatMgr>();
		}
		return s_instance;
	}

	public IEnumerable<string> GetCheatCommands()
	{
		return m_funcMap.Keys;
	}

	public bool HandleKeyboardInput()
	{
		m_ignoreNextConsoleKeypress = false;
		if (HearthstoneApplication.IsPublic())
		{
			return false;
		}
		if (!InputCollection.GetKeyUp(KeyCode.BackQuote))
		{
			return false;
		}
		if (m_closingConsole)
		{
			m_closingConsole = false;
			return true;
		}
		ShowConsole();
		m_ignoreNextConsoleKeypress = true;
		return true;
	}

	public void ShowConsole()
	{
		Rect cheatInputRect = new Rect(0f, 0f, 1f, 0.05f);
		m_cheatHistoryIndex = -1;
		m_cheatTextBeforeAutofill = null;
		m_autofillMatchIndex = -1;
		ReadCheatHistoryOption();
		m_cheatTextBeforeScrollingThruHistory = null;
		UniversalInputManager.TextInputParams inputParams = new UniversalInputManager.TextInputParams
		{
			m_owner = SceneObject,
			m_preprocessCallback = OnInputPreprocess,
			m_rect = cheatInputRect,
			m_color = Color.white,
			m_completedCallback = OnInputComplete,
			m_showBackground = true
		};
		UniversalInputManager.Get().UseTextInput(inputParams);
	}

	public void HideConsole()
	{
		UniversalInputManager.Get().CancelTextInput(SceneObject);
	}

	private void ReadCheatHistoryOption()
	{
		string storedCheatHistory = Options.Get().GetString(Option.CHEAT_HISTORY);
		m_cheatHistory = new List<string>(storedCheatHistory.Split(';'));
	}

	private void WriteCheatHistoryOption()
	{
		Options.Get().SetString(Option.CHEAT_HISTORY, string.Join(";", m_cheatHistory.ToArray()));
	}

	private bool OnInputPreprocess()
	{
		if (m_ignoreNextConsoleKeypress)
		{
			m_ignoreNextConsoleKeypress = false;
		}
		else if (Input.GetKeyDown(KeyCode.BackQuote) && string.IsNullOrEmpty(UniversalInputManager.Get().GetInputText()))
		{
			m_closingConsole = true;
			UniversalInputManager.Get().CancelTextInput(SceneObject);
			return true;
		}
		if (m_cheatHistory.Count < 1)
		{
			return false;
		}
		if (Input.GetKeyDown(KeyCode.UpArrow))
		{
			if (m_cheatHistoryIndex >= m_cheatHistory.Count - 1)
			{
				return true;
			}
			string curText = UniversalInputManager.Get().GetInputText();
			if (m_cheatTextBeforeScrollingThruHistory == null)
			{
				m_cheatTextBeforeScrollingThruHistory = curText;
			}
			string text = m_cheatHistory[++m_cheatHistoryIndex];
			UniversalInputManager.Get().SetInputText(text, moveCursorToEnd: true);
			return true;
		}
		if (Input.GetKeyDown(KeyCode.DownArrow))
		{
			string text2;
			if (m_cheatHistoryIndex <= 0)
			{
				m_cheatHistoryIndex = -1;
				if (m_cheatTextBeforeScrollingThruHistory == null)
				{
					return false;
				}
				text2 = m_cheatTextBeforeScrollingThruHistory;
				m_cheatTextBeforeScrollingThruHistory = null;
			}
			else
			{
				text2 = m_cheatHistory[--m_cheatHistoryIndex];
			}
			UniversalInputManager.Get().SetInputText(text2);
			return true;
		}
		if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Backspace))
		{
			string text3 = UniversalInputManager.Get().GetInputText();
			if (text3[text3.Length - 1] == ' ')
			{
				text3.Trim();
			}
			text3 = ((!text3.Contains(" ")) ? "" : text3.Substring(0, text3.LastIndexOf(' ')));
			UniversalInputManager.Get().SetInputText(text3);
		}
		if (Input.GetKeyDown(KeyCode.Tab) && HearthstoneApplication.IsInternal())
		{
			string text4 = UniversalInputManager.Get().GetInputText();
			bool num = !text4.Contains(' ');
			bool shift = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
			if (num)
			{
				bool displayMatches = true;
				if (m_cheatTextBeforeAutofill != null)
				{
					text4 = m_cheatTextBeforeAutofill;
					displayMatches = false;
				}
				else
				{
					m_cheatTextBeforeAutofill = text4;
				}
				List<string> matches = m_funcMap.Keys.Where((string f) => f.StartsWith(text4, StringComparison.InvariantCultureIgnoreCase)).ToList();
				if (matches.Count > 0)
				{
					matches.Sort();
					int indexToUse = 0;
					m_autofillMatchIndex += ((!shift) ? 1 : (-1));
					if (m_autofillMatchIndex >= matches.Count)
					{
						m_autofillMatchIndex = 0;
					}
					else if (m_autofillMatchIndex < 0)
					{
						m_autofillMatchIndex = matches.Count - 1;
					}
					if (m_autofillMatchIndex >= 0 && m_autofillMatchIndex < matches.Count)
					{
						indexToUse = m_autofillMatchIndex;
					}
					text4 = matches[indexToUse];
					UniversalInputManager.Get().SetInputText(text4, moveCursorToEnd: true);
					if (displayMatches && matches.Count > 1)
					{
						float duration = 5f + Mathf.Max(0f, matches.Count - 3);
						duration *= Time.timeScale;
						UIStatus.Get().AddInfo("Available cheats:\n" + string.Join("   ", matches.ToArray()), duration);
					}
				}
			}
			else
			{
				string[] args;
				string rawArgs;
				string func = ParseFuncAndArgs(text4, out args, out rawArgs);
				if (func == null)
				{
					return false;
				}
				UIStatus.Get().AddInfo("", 0f);
				if (CallCheatCallback(func, args, rawArgs, isAutofill: true, shift))
				{
					string newArgs;
					if (string.IsNullOrEmpty(m_lastAutofillParamPrefix) && rawArgs.EndsWith(" "))
					{
						newArgs = rawArgs + m_lastAutofillParamMatch;
					}
					else
					{
						args[args.Length - 1] = m_lastAutofillParamMatch;
						newArgs = string.Join(" ", args);
					}
					UniversalInputManager.Get().SetInputText(func + " " + newArgs, moveCursorToEnd: true);
				}
			}
		}
		else
		{
			bool resetAutofillData = false;
			if (Input.GetKeyDown(KeyCode.None) || Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.Home) || Input.GetKeyDown(KeyCode.End) || Input.GetKeyDown(KeyCode.Insert) || Input.GetKeyDown(KeyCode.PageUp) || Input.GetKeyDown(KeyCode.PageDown) || Input.GetKeyDown(KeyCode.LeftAlt) || Input.GetKeyDown(KeyCode.RightAlt) || Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.RightControl) || Input.GetKeyDown(KeyCode.CapsLock) || Input.GetKeyDown(KeyCode.LeftWindows) || Input.GetKeyDown(KeyCode.RightWindows) || Input.GetKeyDown(KeyCode.LeftMeta) || Input.GetKeyDown(KeyCode.RightMeta) || Input.GetKeyDown(KeyCode.Menu) || Input.GetKeyDown(KeyCode.Backspace) || Input.GetKeyDown(KeyCode.Delete))
			{
				resetAutofillData = true;
			}
			if (resetAutofillData)
			{
				if (m_autofillMatchIndex != -1 || m_lastAutofillParamPrefix != null)
				{
					UIStatus.Get().AddInfo("", 0f);
				}
				m_cheatTextBeforeAutofill = null;
				m_autofillMatchIndex = -1;
				m_lastAutofillParamFunc = null;
				m_lastAutofillParamPrefix = null;
				m_lastAutofillParamMatch = null;
			}
		}
		return false;
	}

	public void RegisterCategory(string cat)
	{
		cat = cat.ToLowerInvariant();
		string catIter = cat;
		while (!string.IsNullOrEmpty(catIter))
		{
			if (m_categoryList.IndexOf(catIter) < 0)
			{
				m_categoryList.Count();
				m_categoryList.Add(catIter);
			}
			int parentCatLength = catIter.LastIndexOf(':');
			catIter = ((parentCatLength > 0) ? catIter.Substring(0, parentCatLength) : null);
		}
		m_lastRegisteredCategoryIndex = m_categoryList.IndexOf(cat);
	}

	public void DefaultCategory()
	{
		RegisterCategory("other");
	}

	public void RegisterCheatHandler(string func, ProcessCheatCallback callback, string desc = null, string argDesc = null, string exampleArgs = null)
	{
		RegisterCheatHandler_(func, callback);
		if (desc != null)
		{
			m_cheatDesc[func] = desc;
		}
		if (argDesc != null)
		{
			m_cheatArgs[func] = argDesc;
		}
		if (exampleArgs != null)
		{
			m_cheatExamples[func] = exampleArgs;
		}
	}

	public void RegisterCheatHandler(string func, ProcessCheatAutofillCallback callback, string desc = null, string argDesc = null, string exampleArgs = null)
	{
		RegisterCheatHandler_(func, callback);
		if (desc != null)
		{
			m_cheatDesc[func] = desc;
		}
		if (argDesc != null)
		{
			m_cheatArgs[func] = argDesc;
		}
		if (exampleArgs != null)
		{
			m_cheatExamples[func] = exampleArgs;
		}
	}

	public void RegisterCheatAlias(string func, params string[] aliases)
	{
		func = func?.Trim().ToLower();
		if (string.IsNullOrEmpty(func))
		{
			Debug.LogError("CheatMgr.RegisterCheatAlias() - cannot register aliases for a null func.");
			return;
		}
		if (!m_funcMap.ContainsKey(func))
		{
			Debug.LogError($"CheatMgr.RegisterCheatAlias() - cannot register aliases for func {func} because it does not exist");
			return;
		}
		foreach (string alias in aliases)
		{
			if (string.IsNullOrEmpty(alias))
			{
				Debug.LogWarning("CheatMgr.RegisterCheatAlias() - Skipping a null alias.");
			}
			else
			{
				m_cheatAlias[alias] = func.ToLower();
			}
		}
	}

	public void UnregisterCheatHandler(string func, ProcessCheatCallback callback)
	{
		UnregisterCheatHandler_(func, callback);
	}

	public string GetCheatCategory(string cheat)
	{
		if (m_cheatCategoryIndex.TryGetValue(cheat, out var categoryIndex) && categoryIndex >= 0)
		{
			return m_categoryList[categoryIndex];
		}
		return "other";
	}

	private void RegisterCheatHandler_(string func, Delegate callback)
	{
		func = func?.Trim().ToLower();
		if (string.IsNullOrEmpty(func))
		{
			Debug.LogError("CheatMgr.RegisterCheatHandler() - FAILED to register a null, empty, or all-whitespace function name");
			return;
		}
		if (m_funcMap.TryGetValue(func, out var callbacks))
		{
			if (!callbacks.Contains(callback))
			{
				callbacks.Add(callback);
			}
		}
		else
		{
			callbacks = new List<Delegate>();
			m_funcMap.Add(func, callbacks);
			callbacks.Add(callback);
		}
		m_cheatCategoryIndex[func] = m_lastRegisteredCategoryIndex;
	}

	private void UnregisterCheatHandler_(string func, Delegate callback)
	{
		if (m_funcMap.TryGetValue(func, out var callbacks))
		{
			callbacks.Remove(callback);
		}
	}

	private void OnInputComplete(string inputCommand)
	{
		inputCommand = inputCommand.TrimStart();
		if (!string.IsNullOrEmpty(inputCommand))
		{
			m_cheatTextBeforeAutofill = null;
			m_autofillMatchIndex = -1;
			string error = ProcessCheat(inputCommand);
			if (!string.IsNullOrEmpty(error))
			{
				UIStatus.Get().AddError(error, 4f);
			}
		}
	}

	private string ParseFuncAndArgs(string inputCommand, out string[] args, out string rawArgs)
	{
		rawArgs = null;
		args = null;
		string func = ExtractFunc(inputCommand);
		if (func == null)
		{
			return null;
		}
		int funcLength = func.Length;
		if (funcLength == inputCommand.Length)
		{
			rawArgs = "";
			args = new string[1];
			args[0] = "";
		}
		else
		{
			rawArgs = inputCommand.Remove(0, funcLength + 1);
			MatchCollection argMatches = Regex.Matches(rawArgs, "\\S+");
			if (argMatches.Count == 0)
			{
				args = new string[1];
				args[0] = "";
			}
			else
			{
				args = new string[argMatches.Count];
				for (int i = 0; i < argMatches.Count; i++)
				{
					args[i] = argMatches[i].Value;
				}
			}
		}
		return func;
	}

	public string RunCheatInternally(string inputCommand)
	{
		string[] args;
		string rawArgs;
		string func = ParseFuncAndArgs(inputCommand, out args, out rawArgs);
		if (func == null)
		{
			return "\"" + inputCommand.Split(' ')[0] + "\" cheat command not found!";
		}
		if (!CallCheatCallback(func, args, rawArgs, isAutofill: false, isShiftTab: false))
		{
			return "\"" + func + "\" cheat command executed, but failed!";
		}
		return null;
	}

	public string ProcessCheat(string inputCommand, bool doNotSaveToHistory = false)
	{
		if (!doNotSaveToHistory)
		{
			if (m_cheatHistory.Count < 1 || !m_cheatHistory[0].Equals(inputCommand))
			{
				m_cheatHistory.Remove(inputCommand);
				m_cheatHistory.Insert(0, inputCommand);
			}
			if (m_cheatHistory.Count > 25)
			{
				m_cheatHistory.RemoveRange(24, m_cheatHistory.Count - 25);
			}
			m_cheatHistoryIndex = -1;
			m_cheatTextBeforeScrollingThruHistory = null;
			WriteCheatHistoryOption();
		}
		string[] args;
		string rawArgs;
		string func = ParseFuncAndArgs(inputCommand, out args, out rawArgs);
		if (func == null)
		{
			return "\"" + inputCommand.Split(' ')[0] + "\" cheat command not found!";
		}
		UIStatus.Get().AddInfo("", 0f);
		if (!CallCheatCallback(func, args, rawArgs, isAutofill: false, isShiftTab: false))
		{
			return "\"" + func + "\" cheat command executed, but failed!";
		}
		return null;
	}

	private bool CallCheatCallback(string func, string[] args, string rawArgs, bool isAutofill, bool isShiftTab)
	{
		string originalFunc = GetOriginalFunc(func);
		List<Delegate> callbacks = m_funcMap[originalFunc];
		bool handled = false;
		for (int i = 0; i < callbacks.Count; i++)
		{
			Delegate cb = callbacks[i];
			if (cb is ProcessCheatCallback && !isAutofill)
			{
				handled = ((ProcessCheatCallback)cb)(func, args, rawArgs) || handled;
			}
			else if (cb is ProcessCheatAutofillCallback)
			{
				if (isAutofill && func != m_lastAutofillParamFunc)
				{
					m_lastAutofillParamMatch = null;
				}
				ProcessCheatAutofillCallback obj = (ProcessCheatAutofillCallback)cb;
				AutofillData autofillData = null;
				if (isAutofill)
				{
					autofillData = new AutofillData
					{
						m_isShiftTab = isShiftTab,
						m_lastAutofillParamPrefix = m_lastAutofillParamPrefix,
						m_lastAutofillParamMatch = m_lastAutofillParamMatch
					};
				}
				handled = obj(func, args, rawArgs, autofillData) || handled;
				if (isAutofill && handled)
				{
					m_lastAutofillParamFunc = func;
					m_lastAutofillParamPrefix = autofillData.m_lastAutofillParamPrefix;
					m_lastAutofillParamMatch = autofillData.m_lastAutofillParamMatch;
				}
			}
		}
		return handled;
	}

	private string ExtractFunc(string inputCommand)
	{
		inputCommand = inputCommand.TrimStart('/');
		inputCommand = inputCommand.Trim();
		int longestFuncIndex = 0;
		List<string> matches = new List<string>();
		foreach (string func in m_funcMap.Keys)
		{
			matches.Add(func);
			if (func.Length > matches[longestFuncIndex].Length)
			{
				longestFuncIndex = matches.Count - 1;
			}
		}
		foreach (string func2 in m_cheatAlias.Keys)
		{
			matches.Add(func2);
			if (func2.Length > matches[longestFuncIndex].Length)
			{
				longestFuncIndex = matches.Count - 1;
			}
		}
		int inputCommandIndex;
		for (inputCommandIndex = 0; inputCommandIndex < inputCommand.Length; inputCommandIndex++)
		{
			char inputCommandChar = inputCommand[inputCommandIndex];
			int i = 0;
			while (i < matches.Count)
			{
				string func3 = matches[i];
				if (inputCommandIndex == func3.Length)
				{
					if (char.IsWhiteSpace(inputCommandChar))
					{
						return func3;
					}
					matches.RemoveAt(i);
					if (i <= longestFuncIndex)
					{
						longestFuncIndex = ComputeLongestFuncIndex(matches);
					}
				}
				else if (char.ToLower(func3[inputCommandIndex]) != char.ToLower(inputCommandChar))
				{
					matches.RemoveAt(i);
					if (i <= longestFuncIndex)
					{
						longestFuncIndex = ComputeLongestFuncIndex(matches);
					}
				}
				else
				{
					i++;
				}
			}
			if (matches.Count == 0)
			{
				return null;
			}
		}
		if (matches.Count > 1)
		{
			foreach (string func4 in matches)
			{
				if (string.Equals(inputCommand, func4, StringComparison.OrdinalIgnoreCase))
				{
					return func4;
				}
			}
			return null;
		}
		string match = matches[0];
		if (inputCommandIndex < match.Length)
		{
			return null;
		}
		return match;
	}

	private int ComputeLongestFuncIndex(List<string> funcs)
	{
		int longestFuncIndex = 0;
		for (int i = 1; i < funcs.Count; i++)
		{
			if (funcs[i].Length > funcs[longestFuncIndex].Length)
			{
				longestFuncIndex = i;
			}
		}
		return longestFuncIndex;
	}

	private string GetOriginalFunc(string func)
	{
		if (!m_cheatAlias.TryGetValue(func, out var originalFunc))
		{
			return func;
		}
		return originalFunc;
	}
}
