using UnityEngine;

[CreateAssetMenu(fileName = "Trigger", menuName = "UIVoiceLinesTool/UIVoiceLinesTrigger", order = 2)]
public class UIVoiceLinesTrigger : ScriptableObject
{
	public UIVoiceLinesManager.TriggerType m_TriggerType;

	public UIVoiceLinesManager.UIVoiceLineCategory m_Category;

	public int m_AdditonalIntParam;

	public string m_AdditonalStringParam;
}
