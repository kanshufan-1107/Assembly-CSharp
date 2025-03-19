using System;
using UnityEngine;

[Serializable]
public class UIVoiceLineItem
{
	public UIVoiceLinesTrigger m_VOTrigger;

	[CustomEditField(T = EditType.GAME_OBJECT)]
	public string m_SoundReference;

	[CustomEditField(T = EditType.GAME_OBJECT)]
	public string m_VisualAssetReference;

	public string m_StringToLocalize = "";

	public Vector3 m_Position;

	public bool m_AllowRepeatDuringSession = true;

	public bool m_BlockAllOtherInput;

	public CanvasAnchor m_AnchorPoint = CanvasAnchor.BOTTOM_LEFT;

	public string m_eventData;
}
