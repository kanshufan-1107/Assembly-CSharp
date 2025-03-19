using System;

[Serializable]
public class EventTimingVisualDef
{
	[CustomEditField]
	public EventTimingType m_EventType;

	[CustomEditField(T = EditType.GAME_OBJECT)]
	public string m_Prefab;
}
