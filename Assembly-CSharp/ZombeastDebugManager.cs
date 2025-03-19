using Hearthstone;
using UnityEngine;

public class ZombeastDebugManager : MonoBehaviour
{
	private static ZombeastDebugManager s_instance;

	public static ZombeastDebugManager Get()
	{
		if (s_instance == null)
		{
			GameObject obj = new GameObject();
			s_instance = obj.AddComponent<ZombeastDebugManager>();
			obj.name = "ZombeastDebugManager (Dynamically created)";
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
		int currentZombeastDatabaseID = localPlayer.GetTag(GAME_TAG.ZOMBEAST_DEBUG_CURRENT_BEAST_DATABASE_ID);
		if (currentZombeastDatabaseID != 0)
		{
			EntityDef entityDef = DefLoader.Get().GetEntityDef(currentZombeastDatabaseID);
			string zombeastName = "Unknown";
			if (entityDef != null)
			{
				zombeastName = entityDef.GetName();
			}
			string debugDisplayString = $"Zombeast being generated: {zombeastName}\nGenerated: {localPlayer.GetTag(GAME_TAG.ZOMBEAST_DEBUG_CURRENT_ITERATION)}/{localPlayer.GetTag(GAME_TAG.ZOMBEAST_DEBUG_MAX_ITERATIONS)}";
			Vector3 drawPos = new Vector3(Screen.width, Screen.height, 0f);
			DebugTextManager.Get().DrawDebugText(debugDisplayString, drawPos, 0f, screenSpace: true);
		}
	}
}
