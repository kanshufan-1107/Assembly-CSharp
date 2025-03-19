using System;
using Assets;
using Hearthstone.DataModels;
using Hearthstone.Progression;

[Serializable]
public class CardDefSpecialEvent
{
	public enum EventSceneMode
	{
		All,
		Arena,
		Gameplay,
		CollectionManager,
		TavernBrawl
	}

	public SpecialEvent.SpecialEventType EventType;

	public EventSceneMode m_SceneMode;

	public ScenarioDbId m_Scenario;

	[CustomEditField(Sections = "Portrait", T = EditType.CARD_TEXTURE)]
	public string m_PortraitTextureOverride;

	[CustomEditField(Sections = "Portrait", T = EditType.MATERIAL)]
	public string m_PremiumPortraitMaterialOverride;

	[CustomEditField(Sections = "Portrait", T = EditType.UBERANIMATION)]
	public string m_PremiumUberShaderAnimationOverride;

	[CustomEditField(Sections = "Portrait", T = EditType.CARD_TEXTURE)]
	public string m_PremiumPortraitTextureOverride;

	public static CardDefSpecialEvent FindActiveEvent(CardDef cardDef)
	{
		foreach (CardDefSpecialEvent specialEvent in cardDef.m_SpecialEvents)
		{
			SpecialEventDataModel eventDataModel = SpecialEventManager.Get().GetEventDataModelForCurrentEvent();
			if (eventDataModel != null && eventDataModel.SpecialEventType == specialEvent.EventType)
			{
				SceneMgr.Mode sceneMode = SceneMgr.Mode.INVALID;
				switch (specialEvent.m_SceneMode)
				{
				case EventSceneMode.Arena:
					sceneMode = SceneMgr.Mode.DRAFT;
					break;
				case EventSceneMode.Gameplay:
					sceneMode = SceneMgr.Mode.GAMEPLAY;
					break;
				case EventSceneMode.CollectionManager:
					sceneMode = SceneMgr.Mode.COLLECTIONMANAGER;
					break;
				case EventSceneMode.TavernBrawl:
					sceneMode = SceneMgr.Mode.TAVERN_BRAWL;
					break;
				}
				if (SceneMgr.Get().GetMode() == sceneMode || sceneMode == SceneMgr.Mode.INVALID)
				{
					return specialEvent;
				}
				if (GameMgr.Get().GetMissionId() == (int)specialEvent.m_Scenario)
				{
					return specialEvent;
				}
			}
		}
		return null;
	}
}
