using Blizzard.T5.AssetManager;
using UnityEngine;

[CreateAssetMenu(menuName = "Prototyping/Finisher Gameplay Settings")]
public class FinisherGameplaySettings : ScriptableObject
{
	private AssetHandle<FinisherGameplaySettings> FinisherHandle;

	[HideInInspector]
	public string SmallPrefab;

	[HideInInspector]
	public string SmallOpponentPrefab;

	[HideInInspector]
	public string LargePrefab;

	[HideInInspector]
	public string LargeOpponentPrefab;

	[HideInInspector]
	public string LethalPrefab;

	[HideInInspector]
	public string LethalOpponentPrefab;

	[HideInInspector]
	public string FirstPlaceVictoryPrefab;

	[HideInInspector]
	public string FirstPlaceVictoryOpponentPrefab;

	[HideInInspector]
	public string DestroyPlayerPrefab;

	[HideInInspector]
	public string DestroyOpponentPrefab;

	[HideInInspector]
	public string FirstPlaceVictoryDestroyPlayerPrefab;

	[HideInInspector]
	public string FirstPlaceVictoryDestroyOpponentPrefab;

	[Tooltip("Enable to show default impact effects (little sparks and stuff like you see in standard HS)")]
	public bool ShowImpactEffects;

	[Tooltip("Enable ONLY if knockout and victory spell prefabs contain frame shatter FX")]
	public bool IgnoreDestroyPrefabs;

	public static FinisherGameplaySettings GetFinisherGameplaySettings(Entity hero)
	{
		int favoriteFinisherId = hero.GetTag(GAME_TAG.BATTLEGROUNDS_FAVORITE_FINISHER);
		if (favoriteFinisherId <= 0)
		{
			Log.Spells.PrintError(hero.GetDebugName() + " has no tag BATTLEGROUNDS_FAVORITE_FINISHER. Using Default Finisher.");
			favoriteFinisherId = 1;
		}
		BattlegroundsFinisherDbfRecord finisherRecord = GameDbf.BattlegroundsFinisher.GetRecord(favoriteFinisherId);
		if (finisherRecord == null)
		{
			Log.Spells.PrintError($"No Finisher was found for Finisher ID {favoriteFinisherId}. Using default finisher.");
			finisherRecord = GameDbf.BattlegroundsFinisher.GetRecord(1);
		}
		AssetReference finisherReference = AssetReference.CreateFromAssetString(finisherRecord.GameplaySettings);
		AssetHandle<FinisherGameplaySettings> finisherHandle = ((finisherReference != null) ? AssetLoader.Get().LoadAsset<FinisherGameplaySettings>(finisherReference) : null);
		FinisherGameplaySettings finisherSettings = (finisherHandle ? finisherHandle.Asset : null);
		if (finisherSettings == null)
		{
			Log.Spells.PrintError($"Finisher ID {favoriteFinisherId} is missing its finisher settings entirely in HE2. Using default finisher.");
			finisherRecord = GameDbf.BattlegroundsFinisher.GetRecord(1);
			finisherReference = AssetReference.CreateFromAssetString(finisherRecord.GameplaySettings);
			finisherHandle = AssetLoader.Get().LoadAsset<FinisherGameplaySettings>(finisherReference);
			finisherSettings = finisherHandle.Asset;
		}
		finisherSettings.FinisherHandle = finisherHandle;
		return finisherSettings;
	}

	public void Dispose()
	{
		if (FinisherHandle != null)
		{
			FinisherHandle.Dispose();
			FinisherHandle = null;
		}
	}
}
