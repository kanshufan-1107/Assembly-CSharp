using System;
using System.Collections.Generic;
using Assets;
using Hearthstone.UI;
using PegasusLettuce;
using UnityEngine;

public static class LettuceTutorialUtils
{
	public static Dictionary<LettuceTutorialVo.LettuceTutorialEvent, int> SpecificEventMap = new Dictionary<LettuceTutorialVo.LettuceTutorialEvent, int>
	{
		{
			LettuceTutorialVo.LettuceTutorialEvent.INVALID,
			0
		},
		{
			LettuceTutorialVo.LettuceTutorialEvent.VILLAGE_TUTORIAL_END,
			34
		}
	};

	public static bool IsEventTypeComplete(LettuceTutorialVo.LettuceTutorialEvent tutorialEvent)
	{
		List<long> values;
		return IsEventTypeComplete(tutorialEvent, 0, 0, null, out values);
	}

	public static bool IsEventTypeComplete(LettuceTutorialVo.LettuceTutorialEvent tutorialEvent, int nodeTypeId, int bountyRecordId, LettuceTutorialVoDbfRecord vo, out List<long> values)
	{
		GameSaveDataManager.Get().GetSubkeyValue(GameSaveKeyId.MERCENARIES, GameSaveKeySubkeyId.MERCENARIES_TUTORIAL_EVENTS, out values);
		bool playableRecordFound = false;
		if (vo == null)
		{
			foreach (LettuceTutorialVoDbfRecord record in GameDbf.LettuceTutorialVo.GetRecords((LettuceTutorialVoDbfRecord r) => r.TutorialEvent == tutorialEvent))
			{
				if (CanRecordPlayUnderCurrentConditions(record, nodeTypeId, bountyRecordId))
				{
					playableRecordFound = true;
					List<long> obj = values;
					if (obj != null && obj.Contains(record.ID))
					{
						return true;
					}
				}
			}
			if (!playableRecordFound)
			{
				Log.Lettuce.PrintError($"unable to find playable VO Record for tutorial event {tutorialEvent}");
			}
			return false;
		}
		List<long> obj2 = values;
		if (obj2 != null && obj2.Contains(vo.ID))
		{
			return true;
		}
		return false;
	}

	public static bool IsSpecificEventComplete(LettuceTutorialVo.LettuceTutorialEvent eventType)
	{
		return IsSpecificEventComplete(SpecificEventMap[eventType]);
	}

	public static bool IsSpecificEventComplete(int tutorialId)
	{
		if (tutorialId <= 0)
		{
			return false;
		}
		GameSaveDataManager.Get().GetSubkeyValue(GameSaveKeyId.MERCENARIES, GameSaveKeySubkeyId.MERCENARIES_TUTORIAL_EVENTS, out List<long> values);
		if (values != null && values.Contains(tutorialId))
		{
			return true;
		}
		return false;
	}

	public static bool CanRecordPlayUnderCurrentConditions(LettuceTutorialVoDbfRecord vo, int nodeTypeId = 0, int bountyRecordId = 0)
	{
		if (vo.NodeTypeId != 0 && vo.NodeTypeId != nodeTypeId)
		{
			return false;
		}
		if (vo.RequiredActiveBounty != 0 && vo.RequiredActiveBounty != bountyRecordId)
		{
			return false;
		}
		if (vo.RequiredActiveTask != 0)
		{
			MercenariesTaskState match = LettuceVillageDataUtil.GetTaskStateByID(vo.RequiredActiveTask);
			if (match == null || match.Status_ == MercenariesTaskState.Status.COMPLETE || match.Status_ == MercenariesTaskState.Status.CLAIMED || match.Status_ == MercenariesTaskState.Status.INVALID)
			{
				return false;
			}
		}
		if (vo.RequiredActiveVisitor != 0 && LettuceVillageDataUtil.GetVisitorStateByID(vo.RequiredActiveVisitor) == null)
		{
			return false;
		}
		return true;
	}

	public static bool ForceCompleteEvent(LettuceTutorialVo.LettuceTutorialEvent tutorialEvent, int nodeTypeId = 0, int bountyRecordId = 0)
	{
		foreach (LettuceTutorialVoDbfRecord vo in GameDbf.LettuceTutorialVo.GetRecords((LettuceTutorialVoDbfRecord r) => r.TutorialEvent == tutorialEvent))
		{
			if (CanRecordPlayUnderCurrentConditions(vo, nodeTypeId, bountyRecordId))
			{
				if (IsEventTypeComplete(tutorialEvent, nodeTypeId, bountyRecordId, vo, out var gsdValues))
				{
					return true;
				}
				gsdValues = gsdValues ?? new List<long>();
				gsdValues.Add(vo.ID);
				return GameSaveDataManager.Get().SaveSubkey(new GameSaveDataManager.SubkeySaveRequest(GameSaveKeyId.MERCENARIES, GameSaveKeySubkeyId.MERCENARIES_TUTORIAL_EVENTS, gsdValues.ToArray()));
			}
		}
		Log.Lettuce.PrintError($"VO Record not found for tutorial event {tutorialEvent}");
		return false;
	}

	public static bool FireEvent(LettuceTutorialVo.LettuceTutorialEvent tutorialEvent, GameObject gameObject, int nodeTypeId = 0, int bountyRecordId = 0, Action onComplete = null)
	{
		foreach (LettuceTutorialVoDbfRecord vo in GameDbf.LettuceTutorialVo.GetRecords((LettuceTutorialVoDbfRecord r) => r.TutorialEvent == tutorialEvent))
		{
			if (IsEventTypeComplete(tutorialEvent, nodeTypeId, bountyRecordId, vo, out var gsdValues) && vo.OnlyShowOnce)
			{
				continue;
			}
			gsdValues = gsdValues ?? new List<long>();
			if (!CanRecordPlayUnderCurrentConditions(vo, nodeTypeId, bountyRecordId) || vo.ShowChance == 0 || (vo.ShowChance < 100 && UnityEngine.Random.Range(0, 100) > vo.ShowChance))
			{
				continue;
			}
			if (!string.IsNullOrWhiteSpace(vo.UiEvent))
			{
				SendEventUpwardStateAction.SendEventUpward(gameObject, vo.UiEvent);
			}
			if (!string.IsNullOrWhiteSpace(vo.Popup))
			{
				AssetReference assetReference = new AssetReference(vo.Popup);
				GameObject popup = AssetLoader.Get().InstantiatePrefab(assetReference);
				popup.transform.SetParent(gameObject.transform, worldPositionStays: true);
				if (onComplete != null)
				{
					TutorialNotification tutorialNotification = popup.GetComponent<TutorialNotification>();
					if (tutorialNotification == null || tutorialNotification.m_ButtonStart == null)
					{
						Log.Lettuce.PrintError($"Popup prefab for tutorial VO event {vo.ID} needs a root TutorialNotification component with valid ButtonStart reference.");
					}
					tutorialNotification.m_ButtonStart.AddEventListener(UIEventType.RELEASE, delegate
					{
						CompleteEvent(vo, gameObject, nodeTypeId, bountyRecordId, onComplete);
					});
				}
			}
			else if (vo.TutorialDialog > 0)
			{
				NarrativeManager.Get().PlayMercenariesTutorialDialogue(vo.TutorialDialog, delegate
				{
					CompleteEvent(vo, gameObject, nodeTypeId, bountyRecordId, onComplete);
				});
			}
			if (!gsdValues.Contains(vo.ID))
			{
				gsdValues.Add(vo.ID);
			}
			GameSaveDataManager.Get().SaveSubkey(new GameSaveDataManager.SubkeySaveRequest(GameSaveKeyId.MERCENARIES, GameSaveKeySubkeyId.MERCENARIES_TUTORIAL_EVENTS, gsdValues.ToArray()));
			return true;
		}
		onComplete?.Invoke();
		return false;
	}

	private static void CompleteEvent(LettuceTutorialVoDbfRecord record, GameObject gameObject, int mapNodeTypeId, int bountyRecordId, Action onComplete)
	{
		if (record.TriggerEventOnComplete != 0)
		{
			FireEvent(record.TriggerEventOnComplete, gameObject, mapNodeTypeId, bountyRecordId, onComplete);
		}
		else
		{
			onComplete?.Invoke();
		}
	}
}
