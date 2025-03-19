using System;

[Serializable]
public class BoxSpecialEvent
{
	public EventTimingType EventType;

	[CustomEditField(T = EditType.GAME_OBJECT)]
	public string Prefab;

	[CustomEditField(T = EditType.GAME_OBJECT)]
	public string PrefabPhoneOverride;

	[CustomEditField(T = EditType.TEXTURE)]
	public string BoxTexture;

	[CustomEditField(T = EditType.TEXTURE)]
	public string BoxTexturePhone;

	[CustomEditField(T = EditType.TEXTURE)]
	public string TableTexture;

	public bool showToReturningPlayers;
}
