using System.Collections.Generic;
using Hearthstone;
using PegasusGame;
using UnityEngine;

public class ScriptDebugDisplay : MonoBehaviour
{
	private static ScriptDebugDisplay s_instance = null;

	private List<ScriptDebugInformation> m_debugInformation = new List<ScriptDebugInformation>();

	private static readonly int MAX_LINES = 70;

	public bool m_isDisplayed;

	private float m_currentDumpScrollBarValue = 1f;

	private float m_currentStatementScrollBarValue;

	public static ScriptDebugDisplay Get()
	{
		if (s_instance == null)
		{
			GameObject obj = new GameObject();
			s_instance = obj.AddComponent<ScriptDebugDisplay>();
			obj.name = "ScriptDebugDisplay (Dynamically created)";
		}
		return s_instance;
	}

	private void Start()
	{
		if (!HearthstoneApplication.IsPublic() && GameState.Get() != null)
		{
			GameState.Get().RegisterCreateGameListener(GameState_CreateGameEvent, null);
		}
	}

	private void GameState_CreateGameEvent(GameState.CreateGamePhase createGamePhase, object userData)
	{
		m_debugInformation.Clear();
	}

	public bool ToggleDebugDisplay(bool shouldDisplay)
	{
		m_isDisplayed = shouldDisplay;
		return true;
	}

	private void Update()
	{
		if (!HearthstoneApplication.IsPublic() && GameState.Get() != null && m_isDisplayed)
		{
			ScriptDebugInformation currentDebugInfo = null;
			if (m_debugInformation.Count > 0)
			{
				currentDebugInfo = m_debugInformation[GetCurrentDumpIndex()];
			}
			if (currentDebugInfo != null)
			{
				UpdateDisplay(currentDebugInfo);
			}
		}
	}

	private int GetCurrentDumpIndex()
	{
		int currentDumpIndex = (int)(m_currentDumpScrollBarValue * (float)m_debugInformation.Count);
		if (currentDumpIndex >= m_debugInformation.Count)
		{
			currentDumpIndex = m_debugInformation.Count - 1;
		}
		return currentDumpIndex;
	}

	private void UpdateDisplay(ScriptDebugInformation debugInfo)
	{
		string debugDisplayString = $"Script Debug: {debugInfo.EntityName} (ID{debugInfo.EntityID})\n";
		Vector3 drawPosTopRightCorner = new Vector3(Screen.width, Screen.height, 0f);
		Vector3 drawPosTopLeftCorner = new Vector3(0f, Screen.height, 0f);
		int currentHistoryIndex = (int)(m_currentStatementScrollBarValue * (float)debugInfo.Calls.Count);
		if (currentHistoryIndex >= debugInfo.Calls.Count)
		{
			currentHistoryIndex = debugInfo.Calls.Count - 1;
		}
		int currentIndex = 0;
		foreach (ScriptDebugCall call in debugInfo.Calls)
		{
			string stringToAppend = call.OpcodeName;
			if (currentIndex == currentHistoryIndex)
			{
				debugDisplayString = ((call.ErrorStrings.Count <= 0) ? AppendLine(debugDisplayString, $"<color=#00ff00ff>{stringToAppend}</color>") : AppendLine(debugDisplayString, $"<color=#ffff00ff>{stringToAppend}</color>"));
				string inspectString = "Inputs";
				int variableID = 0;
				foreach (ScriptDebugVariable input in call.Inputs)
				{
					inspectString = AppendVariable(inspectString, input, $"Input Variable {variableID}");
					variableID++;
				}
				if (call.ErrorStrings.Count > 0)
				{
					inspectString = AppendLine(inspectString, "\n<color=#ff0000ff>ERRORS</color>");
					foreach (string errorMessage in call.ErrorStrings)
					{
						string errorMessageInRed = $"<color=#ff0000ff>{errorMessage}</color>";
						inspectString = AppendLine(inspectString, errorMessageInRed);
					}
				}
				inspectString = AppendLine(inspectString, "\nOutput");
				if (call.Output.IntValue.Count > 0 || call.Output.StringValue.Count > 0)
				{
					inspectString = AppendVariable(inspectString, call.Output, $"Output Variable");
				}
				inspectString = AppendLine(inspectString, "\nOther variables");
				variableID = 0;
				foreach (ScriptDebugVariable variable in call.Variables)
				{
					inspectString = AppendVariable(inspectString, variable, $"Other Variable {variableID}");
					variableID++;
				}
				DebugTextManager.Get().DrawDebugText(inspectString, drawPosTopLeftCorner, 0f, screenSpace: true);
			}
			else
			{
				if (call.ErrorStrings.Count > 0)
				{
					stringToAppend = $"<color=#ff0000ff>{stringToAppend}</color>";
				}
				debugDisplayString = AppendLine(debugDisplayString, stringToAppend);
			}
			currentIndex++;
		}
		DebugTextManager.Get().DrawDebugText(debugDisplayString, drawPosTopRightCorner, 0f, screenSpace: true, "ScriptDebugDisplayCallLog");
	}

	private string AppendVariable(string inspectString, ScriptDebugVariable variable, string defaultVariableName)
	{
		string variableString = "";
		string variableName = variable.VariableName;
		if (variableName == "")
		{
			variableName = defaultVariableName;
		}
		if (variable.IntValue.Count == 1)
		{
			variableString = $"{variableName} ({variable.VariableType}): {variable.IntValue[0]}";
		}
		else if (variable.StringValue.Count == 1)
		{
			variableString = $"{variableName} ({variable.VariableType}): {variable.StringValue[0]}";
		}
		else if (variable.IntValue.Count > 1)
		{
			variableString = $"{variableName} ({variable.VariableType}): {variable.IntValue[0]}";
			for (int i = 1; i < variable.IntValue.Count; i++)
			{
				variableString = $"{variableString}, {variable.IntValue[i]}";
			}
		}
		else if (variable.StringValue.Count > 1)
		{
			variableString = $"{variableName} ({variable.VariableType}):";
			for (int j = 0; j < variable.StringValue.Count; j++)
			{
				variableString = $"{variableString}\n{variable.StringValue[j]}";
			}
		}
		if (variableString != "")
		{
			for (int x = MAX_LINES; x < variableString.Length; x += MAX_LINES)
			{
				variableString = variableString.Insert(x, "\n");
			}
			inspectString = AppendLine(inspectString, variableString);
		}
		return inspectString;
	}

	private string AppendLine(string inputString, string stringToAppend)
	{
		return $"{inputString}\n{stringToAppend}";
	}

	public void OnScriptDebugInfo(ScriptDebugInformation debugInfo)
	{
		m_debugInformation.Add(debugInfo);
	}
}
