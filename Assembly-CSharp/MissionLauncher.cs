using Hearthstone.UI;
using PegasusShared;
using UnityEngine;

public class MissionLauncher : MonoBehaviour
{
	public GameType GameType;

	public FormatType FormatType;

	public int MissionId;

	public Clickable Button;

	private void Awake()
	{
		Button?.AddEventListener(UIEventType.RELEASE, HandleClick);
	}

	private void HandleClick(UIEvent e)
	{
		ScenarioDbfRecord scenario = GameDbf.Scenario.GetRecord(MissionId);
		GameMgr.Get().FindGameWithHero(GameType, FormatType, scenario.ID, 0, scenario.Player1HeroCardId, scenario.Player1DeckId);
	}
}
