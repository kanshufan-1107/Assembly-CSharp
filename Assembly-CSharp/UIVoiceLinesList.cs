using System.Collections.Generic;
using UnityEngine;

[CustomEditClass]
[CreateAssetMenu(fileName = "Data", menuName = "UIVoiceLinesTool/UIVoiceLinesList", order = 1)]
public class UIVoiceLinesList : ScriptableObject
{
	public UIVoiceLinesManager.UIVoiceLineCategory m_Category;

	[CustomEditField(T = EditType.GAME_OBJECT)]
	public AssetReference m_ListName;

	public List<UIVoiceLineItem> m_DialogueItems;
}
