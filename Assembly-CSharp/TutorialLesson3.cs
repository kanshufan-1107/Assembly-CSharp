using UnityEngine;

public class TutorialLesson3 : MonoBehaviour
{
	public UberText m_attacker;

	public UberText m_defender;

	private void Start()
	{
		m_attacker.SetText(GameStrings.Get(GameStrings.Get("GLOBAL_TUTORIAL_ATTACKER")));
		m_defender.SetText(GameStrings.Get(GameStrings.Get("GLOBAL_TUTORIAL_DEFENDER")));
	}
}
