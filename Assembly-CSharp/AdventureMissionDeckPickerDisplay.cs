using UnityEngine;

[CustomEditClass]
public class AdventureMissionDeckPickerDisplay : MonoBehaviour
{
	public GameObject m_deckPickerTrayContainer;

	private DeckPickerTrayDisplay m_deckPickerTray;

	private void Awake()
	{
		GameObject go = AssetLoader.Get().InstantiatePrefab(UniversalInputManager.UsePhoneUI ? "DeckPickerTray_phone.prefab:a30124f640b5b92459bf820a4e3b1ca7" : "DeckPickerTray.prefab:3e13b59cdca14074bbce2b7d903ed895", AssetLoadingOptions.IgnorePrefabPosition);
		if (go == null)
		{
			Debug.LogError("Unable to load DeckPickerTray.");
			return;
		}
		m_deckPickerTray = go.GetComponent<DeckPickerTrayDisplay>();
		if (m_deckPickerTray == null)
		{
			Debug.LogError("DeckPickerTrayDisplay component not found in DeckPickerTray object.");
			return;
		}
		if (m_deckPickerTrayContainer != null)
		{
			GameUtils.SetParent(m_deckPickerTray, m_deckPickerTrayContainer);
		}
		m_deckPickerTray.AddDeckTrayLoadedListener(OnTrayLoaded);
		m_deckPickerTray.InitAssets();
		m_deckPickerTray.SetPlayButtonText(GameStrings.Get("GLOBAL_PLAY"));
		AdventureConfig adventureConfig = AdventureConfig.Get();
		AdventureDbId selectedAdv = adventureConfig.GetSelectedAdventure();
		AdventureModeDbId selectedMode = adventureConfig.GetSelectedMode();
		AdventureDataDbfRecord dataRecord = GameUtils.GetAdventureDataRecord((int)selectedAdv, (int)selectedMode);
		m_deckPickerTray.SetHeaderText(dataRecord.Name);
	}

	private void OnTrayLoaded()
	{
		AdventureSubScene subscene = GetComponent<AdventureSubScene>();
		if (subscene != null)
		{
			subscene.SetIsLoaded(loaded: true);
		}
	}

	public DeckPickerTrayDisplay GetDeckPickerTrayDisplay()
	{
		return m_deckPickerTray;
	}
}
