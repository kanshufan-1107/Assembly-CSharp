using UnityEngine;

public class BaconDeckCapToggle : MonoBehaviour
{
	public GameObject[] deckCapObjects;

	private void Awake()
	{
		GameState gameState = GameState.Get();
		if (gameState != null)
		{
			if (!gameState.IsGameCreated())
			{
				gameState.RegisterCreateGameListener(OnGameCreated);
			}
			else
			{
				UpdateVisibility();
			}
		}
	}

	private void OnGameCreated(GameState.CreateGamePhase phase, object userData)
	{
		UpdateVisibility();
	}

	private void UpdateVisibility()
	{
		bool darkmoonPrizesActive = GameState.Get().GetGameEntity().GetTag(GAME_TAG.DARKMOON_FAIRE_PRIZES_ACTIVE) == 1;
		GameObject[] array = deckCapObjects;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetActive(!darkmoonPrizesActive);
		}
	}

	private void OnDestroy()
	{
		GameState.Get()?.UnregisterCreateGameListener(OnGameCreated);
	}
}
