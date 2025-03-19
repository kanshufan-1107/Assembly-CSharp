using Hearthstone;
using UnityEngine;

public class DrustvarHorrorDebugManager : MonoBehaviour
{
	private static DrustvarHorrorDebugManager s_instance;

	public static DrustvarHorrorDebugManager Get()
	{
		if (s_instance == null)
		{
			GameObject obj = new GameObject();
			s_instance = obj.AddComponent<DrustvarHorrorDebugManager>();
			obj.name = "DrustvarHorrorDebugManager (Dynamically created)";
		}
		return s_instance;
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
		int currentSpellDatabaseID = localPlayer.GetTag(GAME_TAG.DRUSTVAR_HORROR_DEBUG_CURRENT_SPELL_DATABASE_ID);
		if (currentSpellDatabaseID != 0)
		{
			EntityDef entityDef = DefLoader.Get().GetEntityDef(currentSpellDatabaseID);
			string spellName = "Unknown";
			if (entityDef != null)
			{
				spellName = entityDef.GetName();
			}
			string debugDisplayString = $"Horror being generated: {spellName}\nGenerated: {localPlayer.GetTag(GAME_TAG.DRUSTVAR_HORROR_DEBUG_CURRENT_ITERATION)}/{localPlayer.GetTag(GAME_TAG.DRUSTVAR_HORROR_DEBUG_MAX_ITERATIONS)}";
			Vector3 drawPos = new Vector3(Screen.width, Screen.height, 0f);
			DebugTextManager.Get().DrawDebugText(debugDisplayString, drawPos, 0f, screenSpace: true);
		}
	}
}
