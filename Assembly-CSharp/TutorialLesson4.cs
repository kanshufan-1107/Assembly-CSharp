using UnityEngine;

public class TutorialLesson4 : MonoBehaviour
{
	public UberText m_tauntDescriptionTitle;

	public UberText m_tauntDescription;

	public UberText m_taunt;

	private void Start()
	{
		m_tauntDescriptionTitle.SetText(GameStrings.Get("GLOBAL_TUTORIAL_TAUNT"));
		m_tauntDescription.SetText(GameStrings.Get("GLOBAL_TUTORIAL_TAUNT_DESCRIPTION"));
		m_taunt.SetText(GameStrings.Get("GLOBAL_TUTORIAL_TAUNT"));
	}
}
