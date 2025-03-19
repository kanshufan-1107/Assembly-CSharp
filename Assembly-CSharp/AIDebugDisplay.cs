using System.Collections.Generic;
using System.Linq;
using Hearthstone;
using PegasusGame;
using UnityEngine;

public class AIDebugDisplay : MonoBehaviour
{
	private static AIDebugDisplay s_instance;

	private List<List<List<AIDebugInformation>>> m_debugInformation = new List<List<List<AIDebugInformation>>>();

	public bool m_isDisplayed;

	private float m_currentHistoryScrollBarValue = 1f;

	private float m_currentIterationScrollBarValue = 1f;

	private float m_currentDepthScrollBarValue;

	private bool m_showIterationScrollBar;

	private bool m_showDepthScrollBar;

	public static AIDebugDisplay Get()
	{
		if (s_instance == null)
		{
			GameObject obj = new GameObject();
			s_instance = obj.AddComponent<AIDebugDisplay>();
			obj.name = "AIDebugDisplay (Dynamically created)";
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

	public bool ToggleDebugDisplay(string func, string[] args, string rawArgs)
	{
		m_isDisplayed = !m_isDisplayed;
		return true;
	}

	private void Update()
	{
		if (HearthstoneApplication.IsPublic())
		{
			return;
		}
		GameState currentGame = GameState.Get();
		if (currentGame == null || !m_isDisplayed)
		{
			return;
		}
		AIDebugInformation currentDebugInfo = null;
		if (!currentGame.IsFriendlySidePlayerTurn())
		{
			int currentTurn = currentGame.GetGameEntity().GetTag(GAME_TAG.TURN);
			int moveID = currentGame.GetOpposingSidePlayer().GetTag(GAME_TAG.NUM_OPTIONS_PLAYED_THIS_TURN) + 1;
			List<List<AIDebugInformation>> moveList = m_debugInformation.Find((List<List<AIDebugInformation>> x) => x.Count > 0 && x[0].Count > 0 && x[0][0].MoveID == moveID && x[0][0].TurnID == currentTurn);
			if (moveList != null)
			{
				List<AIDebugInformation> iterationList = moveList[moveList.Count - 1];
				if (iterationList[0].DebugIteration == 0)
				{
					currentDebugInfo = iterationList[0];
				}
			}
			m_currentHistoryScrollBarValue = 1f;
			m_currentIterationScrollBarValue = 1f;
			m_currentDepthScrollBarValue = 0f;
			m_showIterationScrollBar = (m_showDepthScrollBar = moveList != null && moveList.Count > 1);
		}
		else if (m_debugInformation.Count > 0)
		{
			int currentHistoryIndex = (int)(m_currentHistoryScrollBarValue * (float)m_debugInformation.Count);
			if (currentHistoryIndex >= m_debugInformation.Count)
			{
				currentHistoryIndex = m_debugInformation.Count - 1;
			}
			List<List<AIDebugInformation>> moveList2 = m_debugInformation[currentHistoryIndex];
			m_showIterationScrollBar = (m_showDepthScrollBar = moveList2 != null && moveList2.Count > 1);
			int currentIterationIndex = (int)(m_currentIterationScrollBarValue * (float)moveList2.Count);
			if (currentIterationIndex >= moveList2.Count)
			{
				currentIterationIndex = moveList2.Count - 1;
			}
			List<AIDebugInformation> iterationList2 = moveList2[currentIterationIndex];
			int currentDepthIndex = (int)(m_currentDepthScrollBarValue * (float)iterationList2.Count);
			if (currentDepthIndex >= iterationList2.Count)
			{
				currentDepthIndex = iterationList2.Count - 1;
			}
			currentDebugInfo = iterationList2[currentDepthIndex];
		}
		if (currentDebugInfo == null && m_debugInformation.Count > 0)
		{
			List<List<AIDebugInformation>> moveList3 = m_debugInformation.FindLast((List<List<AIDebugInformation>> x) => x.Count > 0 && x[x.Count - 1].Count > 0 && x[x.Count - 1][0].DebugIteration == 0);
			if (moveList3 != null)
			{
				currentDebugInfo = moveList3[moveList3.Count - 1][0];
				m_showIterationScrollBar = (m_showDepthScrollBar = moveList3 != null && moveList3.Count > 1);
			}
		}
		UpdateDisplay(currentDebugInfo);
	}

	private string AppendLine(string inputString, string stringToAppend)
	{
		return $"{inputString}\n{stringToAppend}";
	}

	private string FormatOptionName(AIEvaluation evaluation)
	{
		string formattedString = "";
		formattedString = ((!evaluation.OptionChosen) ? $"{evaluation.OptionName} (ID{evaluation.EntityID})" : $"AI CHOSE: {evaluation.OptionName} (ID{evaluation.EntityID})");
		if (evaluation.TargetScores.Count >= 1)
		{
			AITarget bestTarget = evaluation.TargetScores.Find((AITarget x) => x.TargetChosen);
			if (bestTarget != null)
			{
				formattedString = $"{formattedString} targeting {bestTarget.EntityName} (ID{bestTarget.EntityID})";
			}
		}
		return formattedString;
	}

	private int GetOverallScore(AIEvaluation evaluation)
	{
		return evaluation.BaseScore + evaluation.BonusScore + evaluation.ContextualScore.Sum((AIContextualValue x) => x.ContextualScore) + evaluation.EdgeCount;
	}

	private void UpdateDisplay(AIDebugInformation debugInfo)
	{
		string debugDisplayString = "";
		Vector3 drawPos = new Vector3(Screen.width, Screen.height, 0f);
		if (GameState.Get() != null && GameState.Get().GetGameEntity() != null)
		{
			debugDisplayString = $"Uuid: {GameState.Get().GetGameEntity().Uuid}\n";
		}
		if (debugInfo == null)
		{
			DebugTextManager.Get().DrawDebugText(debugDisplayString, drawPos, 0f, screenSpace: true);
			return;
		}
		if (debugInfo.ModelVersion != 0L)
		{
			debugDisplayString += $"Model Version: {debugInfo.ModelVersion}\n";
		}
		debugDisplayString += $"AI Debug Turn {debugInfo.TurnID}, Move {debugInfo.MoveID}";
		string debug = "";
		if (debugInfo.DebugIteration != 0)
		{
			debug = debug + "Iteration: " + debugInfo.DebugIteration;
		}
		if (debugInfo.DebugDepth != 0)
		{
			debug = debug + " Depth: " + debugInfo.DebugDepth;
		}
		if (!string.IsNullOrEmpty(debug))
		{
			debugDisplayString = AppendLine(debugDisplayString, debug);
		}
		string values = "";
		if (debugInfo.InferenceValue != 0f)
		{
			values = values + "Inference: " + debugInfo.InferenceValue.ToString(".000");
		}
		if (debugInfo.HeuristicValue != 0f)
		{
			values = values + " Heuristic: " + debugInfo.HeuristicValue.ToString(".000");
		}
		if (debugInfo.SubtreeValue != 0f)
		{
			values = values + " Subtree: " + debugInfo.SubtreeValue.ToString(".000");
		}
		if (!string.IsNullOrEmpty(values))
		{
			debugDisplayString = AppendLine(debugDisplayString, values);
		}
		if (debugInfo.TotalVisits > 0)
		{
			debugDisplayString = AppendLine(debugDisplayString, "Total Visits: " + debugInfo.TotalVisits);
		}
		if (debugInfo.UniqueNodes > 0)
		{
			debugDisplayString = AppendLine(debugDisplayString, "Unique Nodes: " + debugInfo.UniqueNodes);
		}
		if (debugInfo.SubtreeDepth > 0)
		{
			debugDisplayString = AppendLine(debugDisplayString, "SubTree Depth: " + debugInfo.SubtreeDepth);
		}
		List<AIEvaluation> list = new List<AIEvaluation>();
		list.AddRange(debugInfo.Evaluations);
		debugInfo.Evaluations = debugInfo.Evaluations.OrderByDescending((AIEvaluation x) => x.OptionChosen ? 9999999 : GetOverallScore(x)).ToList();
		foreach (AIEvaluation evaluation in list)
		{
			debugDisplayString = AppendLine(debugDisplayString, "---");
			debugDisplayString = AppendLine(debugDisplayString, FormatOptionName(evaluation));
			if (evaluation.BaseScore > 0)
			{
				debugDisplayString = AppendLine(debugDisplayString, "Total Option Score: " + GetOverallScore(evaluation));
			}
			int totalContextualScore = 0;
			foreach (AIContextualValue score in evaluation.ContextualScore)
			{
				totalContextualScore += score.ContextualScore;
			}
			if (evaluation.BonusScore != 0 || totalContextualScore != 0)
			{
				debugDisplayString = AppendLine(debugDisplayString, "Base Score: " + evaluation.BaseScore);
			}
			if (evaluation.BonusScore != 0)
			{
				debugDisplayString = AppendLine(debugDisplayString, "Bonus Score: " + evaluation.BonusScore);
			}
			if (evaluation.ContextualScore.Count > 0)
			{
				debugDisplayString = AppendLine(debugDisplayString, "Contextual Score from: ");
				foreach (AIContextualValue score2 in evaluation.ContextualScore)
				{
					debugDisplayString = AppendLine(debugDisplayString, $"{score2.EntityName} (ID{score2.EntityID}): {score2.ContextualScore}");
				}
			}
			if (evaluation.PriorProbability != 0f)
			{
				debugDisplayString = AppendLine(debugDisplayString, "Prior Probability: " + evaluation.PriorProbability.ToString(".000"));
			}
			if (evaluation.PuctValue != 0f && debugInfo.TotalVisits > 1)
			{
				debugDisplayString = AppendLine(debugDisplayString, "Puct Value: " + evaluation.PuctValue.ToString(".000"));
			}
			if (evaluation.FinalVisitCount > 0)
			{
				debugDisplayString = AppendLine(debugDisplayString, "Visit Count: " + evaluation.FinalVisitCount + " (" + evaluation.EdgeCount + ")");
			}
			if (evaluation.SubtreeDepth > 0)
			{
				debugDisplayString = AppendLine(debugDisplayString, "Subtree Depth: " + evaluation.SubtreeDepth);
			}
			if (evaluation.FinalQValue != 0f)
			{
				debugDisplayString = AppendLine(debugDisplayString, "Q Value: " + evaluation.FinalQValue.ToString(".000"));
			}
			string values2 = "";
			if (evaluation.InferenceValue != 0f)
			{
				values2 = values2 + "Inference: " + evaluation.InferenceValue.ToString(".000");
			}
			if (evaluation.HeuristicValue != 0f)
			{
				values2 = values2 + " Heuristic: " + evaluation.HeuristicValue.ToString(".000");
			}
			if (evaluation.SubtreeValue != 0f)
			{
				values2 = values2 + " Subtree: " + evaluation.SubtreeValue.ToString(".000");
			}
			if (!string.IsNullOrEmpty(values2))
			{
				debugDisplayString = AppendLine(debugDisplayString, values2);
			}
			if (evaluation.TargetScores.Count >= 1)
			{
				debugDisplayString = AppendLine(debugDisplayString, "Target scores: ");
				foreach (AITarget score3 in evaluation.TargetScores)
				{
					if (score3.TargetScore > 0)
					{
						debugDisplayString = AppendLine(debugDisplayString, $"{score3.EntityName} (ID{score3.EntityID}): {score3.TargetScore}");
					}
					else if (score3.PriorProbability > 0f)
					{
						string values3 = "";
						if (score3.InferenceValue != 0f)
						{
							values3 = values3 + ", Inf: " + score3.InferenceValue.ToString(".000");
						}
						if (score3.HeuristicValue != 0f)
						{
							values3 = values3 + ", Heur: " + score3.HeuristicValue.ToString(".000");
						}
						if (score3.SubtreeValue != 0f)
						{
							values3 = values3 + ", Sub: " + score3.SubtreeValue.ToString(".000");
						}
						string subtreeString = "";
						if (debugInfo.TotalVisits > 1)
						{
							subtreeString = $", PUCT {score3.PuctValue:.000}, Visit {score3.FinalVisitCount} ({score3.EdgeCount}), Value {score3.FinalQValue:.000}{values3}";
						}
						debugDisplayString = AppendLine(debugDisplayString, $"{score3.EntityName} (ID{score3.EntityID}): Prior {score3.PriorProbability:.000}{subtreeString}");
					}
				}
			}
			if (evaluation.PositionScores.Count < 2)
			{
				continue;
			}
			debugDisplayString = AppendLine(debugDisplayString, "Position scores: ");
			foreach (AIPosition score4 in evaluation.PositionScores)
			{
				if (score4.PriorProbability > 0f)
				{
					string values4 = "";
					if (score4.InferenceValue != 0f)
					{
						values4 = values4 + ", Inf: " + score4.InferenceValue.ToString(".000");
					}
					if (score4.HeuristicValue != 0f)
					{
						values4 = values4 + ", Heur: " + score4.HeuristicValue.ToString(".000");
					}
					if (score4.SubtreeValue != 0f)
					{
						values4 = values4 + ", Sub: " + score4.SubtreeValue.ToString(".000");
					}
					debugDisplayString = AppendLine(debugDisplayString, $"Pos {((score4.Position > 0) ? score4.Position : evaluation.PositionScores.Count)}: Prior {score4.PriorProbability:.000}, PUCT {score4.PuctValue:.000}, Visit {score4.FinalVisitCount} ({score4.EdgeCount}), Value {score4.FinalQValue:.000}{values4}");
				}
			}
		}
		DebugTextManager.Get().DrawDebugText(debugDisplayString, drawPos, 0f, screenSpace: true);
	}

	public void OnAIDebugInformation(AIDebugInformation debugInfo)
	{
		int index = m_debugInformation.FindIndex((List<List<AIDebugInformation>> x) => x.Count > 0 && x[0].Count > 0 && x[0][0].MoveID == debugInfo.MoveID && x[0][0].TurnID == debugInfo.TurnID);
		if (index == -1)
		{
			index = m_debugInformation.Count;
			m_debugInformation.Add(new List<List<AIDebugInformation>>());
		}
		int subIndex = m_debugInformation[index].FindIndex((List<AIDebugInformation> x) => x.Count > 0 && x[0].DebugIteration == debugInfo.DebugIteration);
		if (subIndex == -1)
		{
			subIndex = m_debugInformation[index].Count;
			m_debugInformation[index].Add(new List<AIDebugInformation>());
		}
		int depthIndex = m_debugInformation[index][subIndex].FindIndex((AIDebugInformation x) => x.DebugDepth == debugInfo.DebugDepth);
		if (depthIndex == -1)
		{
			m_debugInformation[index][subIndex].Add(debugInfo);
		}
		else
		{
			m_debugInformation[index][subIndex][depthIndex] = debugInfo;
		}
	}
}
