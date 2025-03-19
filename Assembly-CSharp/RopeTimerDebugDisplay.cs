using Hearthstone;
using PegasusGame;
using UnityEngine;

public class RopeTimerDebugDisplay : MonoBehaviour
{
	private static RopeTimerDebugDisplay s_instance;

	private RopeTimerDebugInformation m_debugInformation;

	public bool m_isDisplayed;

	private const float MICROSECONDS_IN_SECOND = 1000000f;

	public static RopeTimerDebugDisplay Get()
	{
		if (s_instance == null)
		{
			GameObject obj = new GameObject();
			s_instance = obj.AddComponent<RopeTimerDebugDisplay>();
			obj.name = "RopeTimerDebugDisplay (Dynamically created)";
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
	}

	public bool EnableDebugDisplay(string func, string[] args, string rawArgs)
	{
		Network.Get().DebugRopeTimer();
		m_isDisplayed = true;
		return true;
	}

	public bool DisableDebugDisplay(string func, string[] args, string rawArgs)
	{
		Network.Get().DisableDebugRopeTimer();
		m_isDisplayed = false;
		return true;
	}

	private void Update()
	{
		if (!HearthstoneApplication.IsPublic() && GameState.Get() != null && m_isDisplayed)
		{
			UpdateDisplay();
		}
	}

	private string AppendLine(string inputString, string stringToAppend)
	{
		return $"{inputString}\n{stringToAppend}";
	}

	private void UpdateDisplay()
	{
		if (m_debugInformation != null)
		{
			float secondsRemaining = (float)m_debugInformation.MicrosecondsRemainingInTurn / 1000000f;
			float baseTurnTime = (float)m_debugInformation.BaseMicrosecondsInTurn / 1000000f;
			float slushTime = (float)m_debugInformation.SlushTimeInMicroseconds / 1000000f;
			float totalTurnTime = (float)m_debugInformation.TotalMicrosecondsInTurn / 1000000f;
			float slushTimeForOpponent = (float)m_debugInformation.OpponentSlushTimeInMicroseconds / 1000000f;
			float nextTurnSlushTime = (float)m_debugInformation.NextTurnSlushTimeInMicroseconds / 1000000f;
			string debugDisplayString = $"Rope Timer\n Time remaining in turn: {secondsRemaining:F1}\n Base turn time: {baseTurnTime:F1}\n SlushTime: {slushTime:F1}\n Total turn time: {totalTurnTime:F1}\nSlush time for opponent: {slushTimeForOpponent:F1}\nSlush time for next turn: {nextTurnSlushTime:F1}";
			Vector3 drawPos = new Vector3(Screen.width, Screen.height, 0f);
			DebugTextManager.Get().DrawDebugText(debugDisplayString, drawPos, 0f, screenSpace: true);
		}
	}

	public void OnRopeTimerDebugInformation(RopeTimerDebugInformation debugInfo)
	{
		m_debugInformation = debugInfo;
	}
}
