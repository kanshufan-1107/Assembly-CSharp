using UnityEngine;

public class TutorialLesson2 : MonoBehaviour
{
	public UberText m_cost;

	public UberText m_yourMana;

	private void Start()
	{
		m_cost.SetText(GameStrings.Get("GLOBAL_TUTORIAL_COST"));
		m_yourMana.SetText(GameStrings.Get("GLOBAL_TUTORIAL_YOUR_MANA"));
	}
}
