using System.Collections.Generic;
using UnityEngine;

public class TutorialDeathKnightDeckBuilding : MonoBehaviour
{
	private static TutorialDeathKnightDeckBuilding s_instance;

	private const string EVENT_DATA_SHOW_RUNE_POPUP = "SHOW_RUNE_POPUP";

	private const string EVENT_DATA_SHOW_RUNE_INDICATOR_ARROW = "SHOW_RUNE_INDICATOR_ARROW";

	private static Queue<UIVoiceLinesManager.TriggerType> m_pendingTutorials = new Queue<UIVoiceLinesManager.TriggerType>();

	private static bool m_isTutorialPlaying = false;

	private void Awake()
	{
		if (!s_instance)
		{
			s_instance = this;
		}
		else if (s_instance != this)
		{
			Debug.LogWarning("TutorialDeathKnightDeckBuilding object should only be instantiated by the Initialize function.");
			Object.Destroy(base.gameObject);
		}
	}

	private void OnEnable()
	{
		UIVoiceLinesManager.VoiceLineStarted += OnVoiceLineStarted;
		UIVoiceLinesManager.VoiceLineFinished += OnVoiceLineFinished;
	}

	private void OnDisable()
	{
		UIVoiceLinesManager.VoiceLineStarted -= OnVoiceLineStarted;
		UIVoiceLinesManager.VoiceLineFinished -= OnVoiceLineFinished;
		m_isTutorialPlaying = false;
	}

	private static void Initialize()
	{
		if (!s_instance)
		{
			s_instance = new GameObject("DK Deck Building Tutorial").AddComponent<TutorialDeathKnightDeckBuilding>();
		}
	}

	private static void OnVoiceLineStarted(UIVoiceLineItem voiceLine)
	{
		if (voiceLine.m_VOTrigger.m_Category != UIVoiceLinesManager.UIVoiceLineCategory.DK_DECK_BUILDING_TUTORIAL)
		{
			return;
		}
		m_isTutorialPlaying = true;
		CollectionPageManager pageManager = GetPageManager();
		if (pageManager == null)
		{
			return;
		}
		string eventData = voiceLine.m_eventData;
		if (!(eventData == "SHOW_RUNE_POPUP"))
		{
			if (eventData == "SHOW_RUNE_INDICATOR_ARROW")
			{
				pageManager.ShowRuneIndicatorArrowForTutorial();
			}
		}
		else
		{
			pageManager.ShowRuneCardPopupForTutorial();
		}
	}

	private static void OnVoiceLineFinished(UIVoiceLineItem voiceLine, bool isFinalVoiceLine)
	{
		if (voiceLine.m_VOTrigger.m_Category != UIVoiceLinesManager.UIVoiceLineCategory.DK_DECK_BUILDING_TUTORIAL)
		{
			return;
		}
		CollectionPageManager pageManager = GetPageManager();
		if (pageManager == null)
		{
			m_isTutorialPlaying = false;
			return;
		}
		string eventData = voiceLine.m_eventData;
		if (!(eventData == "SHOW_RUNE_POPUP"))
		{
			if (eventData == "SHOW_RUNE_INDICATOR_ARROW")
			{
				pageManager.DismissRuneIndicatorArrowForTutorial();
			}
		}
		else
		{
			pageManager.DismissRuneCardPopupForTutorial();
		}
		if (!isFinalVoiceLine)
		{
			return;
		}
		m_isTutorialPlaying = false;
		GameSaveKeySubkeyId gsSubKey = GetGSDKeyForVoiceLineTriggerType(voiceLine.m_VOTrigger.m_TriggerType);
		if (gsSubKey != GameSaveKeySubkeyId.INVALID)
		{
			SetTutorialSeen(gsSubKey);
		}
		while (m_pendingTutorials.Count > 0)
		{
			UIVoiceLinesManager.TriggerType pendingTutorialTriggerType = m_pendingTutorials.Dequeue();
			if (!HasSeenTutorial(GetGSDKeyForVoiceLineTriggerType(pendingTutorialTriggerType)))
			{
				ShowTutorial(pendingTutorialTriggerType);
				break;
			}
		}
	}

	private static bool HasSeenTutorial(GameSaveKeySubkeyId tutorialSeenSubKey)
	{
		GameSaveDataManager.Get().GetSubkeyValue(GameSaveKeyId.FTUE, tutorialSeenSubKey, out long gskValue);
		return gskValue > 0;
	}

	private static void SetTutorialSeen(GameSaveKeySubkeyId tutorialSeenSubKey)
	{
		if (!HasSeenTutorial(tutorialSeenSubKey))
		{
			GameSaveDataManager.Get().SaveSubkey(new GameSaveDataManager.SubkeySaveRequest(GameSaveKeyId.FTUE, tutorialSeenSubKey, 1L));
		}
	}

	private static CollectionPageManager GetPageManager()
	{
		CollectionManager collectionManager = CollectionManager.Get();
		if (collectionManager == null)
		{
			return null;
		}
		CollectibleDisplay collectibleDisplay = collectionManager.GetCollectibleDisplay();
		if (collectibleDisplay == null)
		{
			return null;
		}
		CollectionPageManager pageManager = collectibleDisplay.GetPageManager() as CollectionPageManager;
		if (!(pageManager == null))
		{
			return pageManager;
		}
		return null;
	}

	private static GameSaveKeySubkeyId GetGSDKeyForVoiceLineTriggerType(UIVoiceLinesManager.TriggerType triggerType)
	{
		return triggerType switch
		{
			UIVoiceLinesManager.TriggerType.STARTED_EDITING_DEATH_KNIGHT_DECK => GameSaveKeySubkeyId.HAS_SEEN_DK_DECK_BUILDING_INTRO_TUTORIAL, 
			UIVoiceLinesManager.TriggerType.ADDED_TRIPLE_DEATH_KNIGHT_RUNES => GameSaveKeySubkeyId.HAS_SEEN_DK_DECK_BUILDING_TRIPLE_RUNES_POPUP, 
			UIVoiceLinesManager.TriggerType.REMOVED_THIRD_RUNE => GameSaveKeySubkeyId.HAS_SEEN_DK_DECK_BUILDING_RUNE_SLOT_AVAILABLE_POPUP, 
			UIVoiceLinesManager.TriggerType.CANNOT_ADD_RUNES => GameSaveKeySubkeyId.HAS_SEEN_DK_DECK_BUILDING_CANNOT_ADD_RUNES_POPUP, 
			_ => GameSaveKeySubkeyId.INVALID, 
		};
	}

	public static void ShowTutorial(UIVoiceLinesManager.TriggerType tutorialTrigger)
	{
		GameSaveKeySubkeyId gsSubKey = GetGSDKeyForVoiceLineTriggerType(tutorialTrigger);
		if (gsSubKey != GameSaveKeySubkeyId.INVALID && !HasSeenTutorial(gsSubKey))
		{
			if (m_isTutorialPlaying)
			{
				m_pendingTutorials.Enqueue(tutorialTrigger);
				return;
			}
			Initialize();
			UIVoiceLinesManager.Get().ExecuteTrigger(UIVoiceLinesManager.UIVoiceLineCategory.DK_DECK_BUILDING_TUTORIAL, tutorialTrigger);
		}
	}
}
