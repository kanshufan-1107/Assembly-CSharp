using System;
using System.Collections.Generic;
using UnityEngine;

public class UIVoiceLinesManager : MonoBehaviour
{
	public enum TriggerType
	{
		NONE,
		BUTTON_PRESSED,
		DUNGEON_RUN_BOSS_REVEAL,
		BOSS_COIN_CLICKED,
		CARD_ADDED_TO_DECK,
		STARTED_EDITING_DEATH_KNIGHT_DECK,
		ADDED_TRIPLE_DEATH_KNIGHT_RUNES,
		REMOVED_THIRD_RUNE,
		CANNOT_ADD_RUNES
	}

	public enum UIVoiceLineCategory
	{
		ALL,
		ADVENTURE,
		EVENTS,
		DK_DECK_BUILDING_TUTORIAL,
		COLLECTION_EVENT
	}

	[SerializeField]
	private UIVoiceLinesList[] m_UIVoiceLineLists;

	private static UIVoiceLinesManager s_instance;

	public static event Action<UIVoiceLineItem> VoiceLineStarted;

	public static event Action<UIVoiceLineItem, bool> VoiceLineFinished;

	public static UIVoiceLinesManager Get()
	{
		return s_instance;
	}

	private void Awake()
	{
		if (s_instance != null)
		{
			Debug.LogWarning("UIVoiceLinesManager is supposed to be a singleton, but a second instance of it is being created!");
		}
		s_instance = this;
	}

	public void ExecuteTrigger(UIVoiceLineCategory category, TriggerType triggerType, int param1 = 0, string param2 = "")
	{
		if (m_UIVoiceLineLists.Length == 0)
		{
			return;
		}
		List<UIVoiceLineItem> voiceLinesToPlay = new List<UIVoiceLineItem>();
		for (int i = 0; i < m_UIVoiceLineLists.Length; i++)
		{
			UIVoiceLinesList list = m_UIVoiceLineLists[i];
			if (list.m_Category != 0 && list.m_Category != category)
			{
				continue;
			}
			if (list.m_DialogueItems.Count == 0)
			{
				Debug.LogError("No Voice Line Items available, List empty!");
				continue;
			}
			List<UIVoiceLineItem> dialogues = list.m_DialogueItems;
			for (int j = 0; j < dialogues.Count; j++)
			{
				UIVoiceLineItem item = dialogues[j];
				if (triggerType == item.m_VOTrigger.m_TriggerType && (item.m_VOTrigger.m_AdditonalStringParam == param2 || item.m_VOTrigger.m_AdditonalIntParam == param1))
				{
					voiceLinesToPlay.Add(item);
				}
			}
		}
		if (voiceLinesToPlay.Count == 1)
		{
			UIVoiceLineItem item2 = voiceLinesToPlay[0];
			UIVoiceLinesManager.VoiceLineStarted?.Invoke(item2);
			PlayVoiceLine(item2.m_SoundReference, item2.m_VisualAssetReference, item2.m_StringToLocalize, item2.m_AllowRepeatDuringSession, item2.m_BlockAllOtherInput, item2.m_AnchorPoint, item2.m_Position, delegate
			{
				UIVoiceLinesManager.VoiceLineFinished?.Invoke(item2, arg2: true);
			});
		}
		else if (voiceLinesToPlay.Count > 1)
		{
			PlayMultipleVoiceLines(0, voiceLinesToPlay);
		}
	}

	private void PlayVoiceLine(string soundRef, string visualReference, string stringToLocalize, bool allowRepeatDuringSession, bool blockAllOtherInput, CanvasAnchor anchorPoint, Vector3 position, Action<int> finishCallback = null)
	{
		NotificationManager.Get().CreateCharacterQuote(visualReference, position, GameStrings.Get(stringToLocalize), soundRef, allowRepeatDuringSession, 0f, finishCallback, anchorPoint, blockAllOtherInput);
	}

	private void PlayMultipleVoiceLines(int counter, List<UIVoiceLineItem> dialogues)
	{
		UIVoiceLineItem item = dialogues[counter];
		Action<int> cbNextLine = ((counter >= dialogues.Count - 1) ? ((Action<int>)delegate
		{
			UIVoiceLinesManager.VoiceLineFinished?.Invoke(item, arg2: true);
		}) : ((Action<int>)delegate
		{
			UIVoiceLinesManager.VoiceLineFinished?.Invoke(item, arg2: false);
			PlayMultipleVoiceLines(counter + 1, dialogues);
		}));
		UIVoiceLinesManager.VoiceLineStarted?.Invoke(item);
		PlayVoiceLine(item.m_SoundReference, item.m_VisualAssetReference, item.m_StringToLocalize, item.m_AllowRepeatDuringSession, item.m_BlockAllOtherInput, item.m_AnchorPoint, item.m_Position, cbNextLine);
	}
}
