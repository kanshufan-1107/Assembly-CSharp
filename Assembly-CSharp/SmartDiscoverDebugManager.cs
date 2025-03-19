using System.Collections.Generic;
using System.Text.RegularExpressions;
using Blizzard.T5.Logging;
using Hearthstone;
using UnityEngine;

public class SmartDiscoverDebugManager : MonoBehaviour
{
	private static SmartDiscoverDebugManager s_instance;

	private Regex m_fileOpenRegex = new Regex("beginsmartdiscoverreport");

	private Regex m_beginRegex = new Regex("beginsmartdiscovertest (?<testName>.+)");

	private Regex m_descriptionRegex = new Regex("smartdiscovertestdescription (?<testString>.+)");

	private Regex m_testExpectsOneResultRegex = new Regex("smartdiscovertestexpectresult (?<cardId1>[^\\s]+)");

	private Regex m_testExpectsTwoResultsRegex = new Regex("smartdiscovertestexpectresult (?<cardId1>[^\\s]+) (?<cardId2>[^\\s]+)");

	private Regex m_testExpectsThreeResultsRegex = new Regex("smartdiscovertestexpectresult (?<cardId1>[^\\s]+) (?<cardId2>[^\\s]+) (?<cardId3>[^\\s]+)");

	private Regex m_endRegex = new Regex("endsmartdiscovertest");

	private List<string> m_expectedResults = new List<string>();

	private string m_currentTestName = "";

	public static SmartDiscoverDebugManager Get()
	{
		if (s_instance == null)
		{
			GameObject obj = new GameObject();
			s_instance = obj.AddComponent<SmartDiscoverDebugManager>();
			obj.name = "SmartDiscoverDebugManager (Dynamically created)";
		}
		return s_instance;
	}

	public bool RequiresWaiting(string line)
	{
		return m_endRegex.Match(line).Success;
	}

	public bool PreprocessCommand(string line)
	{
		if (m_endRegex.Match(line).Success)
		{
			Network.Get().SendDebugConsoleCommand("spawncard XXX_56633 friendly play 0");
			return true;
		}
		return false;
	}

	public bool ParseCheatCommand(string line)
	{
		Match match = m_fileOpenRegex.Match(line);
		if (match.Success)
		{
			Log.SmartDiscover.PurgeFile();
			return true;
		}
		match = m_beginRegex.Match(line);
		if (match.Success)
		{
			Network.Get().SendDebugConsoleCommand("settag 1324 1 0");
			GameState.Get().GetGameEntity().SetTag(GAME_TAG.SMART_DISCOVER_DEBUG_TEST_COMPLETE, 0);
			string testNameString = $"Begin Smart Discover Test: {match.Groups[1].Value}";
			Log.SmartDiscover.ForceFilePrint(LogLevel.Info, testNameString);
			m_currentTestName = match.Groups[1].Value;
			m_expectedResults.Clear();
			return true;
		}
		match = m_descriptionRegex.Match(line);
		if (match.Success)
		{
			Log.SmartDiscover.ForceFilePrint(LogLevel.Info, match.Groups[1].Value);
			return true;
		}
		match = m_testExpectsThreeResultsRegex.Match(line);
		if (match.Success)
		{
			ParseExpectedResultsCommand(match, 3);
			return true;
		}
		match = m_testExpectsTwoResultsRegex.Match(line);
		if (match.Success)
		{
			ParseExpectedResultsCommand(match, 2);
			return true;
		}
		match = m_testExpectsOneResultRegex.Match(line);
		if (match.Success)
		{
			ParseExpectedResultsCommand(match, 1);
			return true;
		}
		match = m_endRegex.Match(line);
		if (match.Success)
		{
			ParseEndCommand(match);
			return true;
		}
		return false;
	}

	private void ParseExpectedResultsCommand(Match match, int expectedResultsCount)
	{
		m_expectedResults.Clear();
		List<string> cardNames = new List<string>();
		string expectedResultsString = "Expected results:";
		for (int i = 1; i <= expectedResultsCount; i++)
		{
			string cardID = match.Groups[i].Value;
			m_expectedResults.Add(cardID);
			EntityDef entityDef = DefLoader.Get().GetEntityDef(cardID);
			if (entityDef != null)
			{
				cardNames.Add(entityDef.GetName());
			}
			else
			{
				cardNames.Add($"UNRECOGNIZED CARD ID: {cardID}");
			}
			expectedResultsString = $"{expectedResultsString} {cardNames[cardNames.Count - 1]}";
		}
		Log.SmartDiscover.ForceFilePrint(LogLevel.Info, expectedResultsString);
	}

	private void ParseEndCommand(Match match)
	{
		bool testPassed = true;
		Player friendlyPlayer = GameState.Get().GetFriendlySidePlayer();
		string cardName1 = "CHOICE_1_INVALID";
		string cardName2 = "CHOICE_2_INVALID";
		string cardName3 = "CHOICE_3_INVALID";
		EntityDef entityDef = DefLoader.Get().GetEntityDef(friendlyPlayer.GetTag(GAME_TAG.SMART_DISCOVER_DEBUG_ENTITY_1), displayError: false);
		if (entityDef != null)
		{
			cardName1 = entityDef.GetName();
		}
		entityDef = DefLoader.Get().GetEntityDef(friendlyPlayer.GetTag(GAME_TAG.SMART_DISCOVER_DEBUG_ENTITY_2), displayError: false);
		if (entityDef != null)
		{
			cardName2 = entityDef.GetName();
		}
		entityDef = DefLoader.Get().GetEntityDef(friendlyPlayer.GetTag(GAME_TAG.SMART_DISCOVER_DEBUG_ENTITY_3), displayError: false);
		if (entityDef != null)
		{
			cardName3 = entityDef.GetName();
		}
		string resultsString = $"Received results: {cardName1}, {cardName2}, {cardName3}";
		Log.SmartDiscover.ForceFilePrint(LogLevel.Info, resultsString);
		foreach (string expectedResult in m_expectedResults)
		{
			int databaseID = GameUtils.TranslateCardIdToDbId(expectedResult);
			if (databaseID == 0)
			{
				testPassed = false;
				break;
			}
			if (databaseID != friendlyPlayer.GetTag(GAME_TAG.SMART_DISCOVER_DEBUG_ENTITY_1) && databaseID != friendlyPlayer.GetTag(GAME_TAG.SMART_DISCOVER_DEBUG_ENTITY_2) && databaseID != friendlyPlayer.GetTag(GAME_TAG.SMART_DISCOVER_DEBUG_ENTITY_3))
			{
				testPassed = false;
				break;
			}
		}
		string conclusionString = string.Format("Test {0} {1}\n", m_currentTestName, testPassed ? "passed" : "FAILED");
		Log.SmartDiscover.ForceFilePrint(LogLevel.Info, conclusionString);
	}

	private void Update()
	{
		if (HearthstoneApplication.IsPublic())
		{
			return;
		}
		GameState currentGame = GameState.Get();
		if (currentGame == null)
		{
			return;
		}
		Player localPlayer = currentGame.GetFriendlySidePlayer();
		if (localPlayer == null)
		{
			return;
		}
		int cardDatabaseID = localPlayer.GetTag(GAME_TAG.SMART_DISCOVER_DEBUG_ENTITY_1);
		if (cardDatabaseID != 0)
		{
			EntityDef entityDef = DefLoader.Get().GetEntityDef(cardDatabaseID);
			string cardName1 = "Unknown";
			if (entityDef != null)
			{
				cardName1 = entityDef.GetName();
			}
			cardDatabaseID = localPlayer.GetTag(GAME_TAG.SMART_DISCOVER_DEBUG_ENTITY_2);
			entityDef = DefLoader.Get().GetEntityDef(cardDatabaseID);
			string cardName2 = "Unknown";
			if (entityDef != null)
			{
				cardName2 = entityDef.GetName();
			}
			cardDatabaseID = localPlayer.GetTag(GAME_TAG.SMART_DISCOVER_DEBUG_ENTITY_3);
			entityDef = DefLoader.Get().GetEntityDef(cardDatabaseID);
			string cardName3 = "Unknown";
			if (entityDef != null)
			{
				cardName3 = entityDef.GetName();
			}
			string debugDisplayString = $"Results:\n1. {cardName1}\n2. {cardName2}\n3. {cardName3}";
			Vector3 drawPos = new Vector3(Screen.width, Screen.height, 0f);
			DebugTextManager.Get().DrawDebugText(debugDisplayString, drawPos, 0f, screenSpace: true);
		}
		string passiveText = GetStringForPassiveResults(localPlayer);
		if (passiveText == "")
		{
			passiveText = GetStringForPassiveResults(currentGame.GetOpposingSidePlayer());
		}
		else
		{
			string theirText = GetStringForPassiveResults(currentGame.GetOpposingSidePlayer());
			if (theirText != "")
			{
				passiveText = $"{passiveText}\n\n{theirText}";
			}
		}
		if (passiveText != "")
		{
			Vector3 drawPos2 = new Vector3(Screen.width, 0f, 0f);
			DebugTextManager.Get().DrawDebugText(passiveText, drawPos2, 0f, screenSpace: true);
		}
	}

	private string GetStringForPassiveResults(Player player)
	{
		if (GameState.Get() == null)
		{
			return "";
		}
		if (player == null)
		{
			return "";
		}
		int cardDatabaseID = player.GetTag(GAME_TAG.SMART_DISCOVER_DEBUG_PASSIVE_EVAL_RESULT_1);
		if (cardDatabaseID == 0)
		{
			return "";
		}
		EntityDef entityDef = DefLoader.Get().GetEntityDef(cardDatabaseID);
		string cardName1 = "Unknown";
		if (entityDef != null)
		{
			cardName1 = entityDef.GetName();
		}
		cardDatabaseID = player.GetTag(GAME_TAG.SMART_DISCOVER_DEBUG_PASSIVE_EVAL_RESULT_2);
		entityDef = DefLoader.Get().GetEntityDef(cardDatabaseID);
		string cardName2 = "Unknown";
		if (entityDef != null)
		{
			cardName2 = entityDef.GetName();
		}
		cardDatabaseID = player.GetTag(GAME_TAG.SMART_DISCOVER_DEBUG_PASSIVE_EVAL_RESULT_3);
		entityDef = DefLoader.Get().GetEntityDef(cardDatabaseID);
		string cardName3 = "Unknown";
		if (entityDef != null)
		{
			cardName3 = entityDef.GetName();
		}
		return $"Passive Results for {player.GetName()}:\n1. {cardName1}\n2. {cardName2}\n3. {cardName3}";
	}
}
