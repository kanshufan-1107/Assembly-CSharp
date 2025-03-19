using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SpecialEventsList", menuName = "ScriptableObjects/BoxSpecialEventScriptableObj", order = 1)]
[CustomEditClass]
public class BoxSpecialEventScriptableObj : ScriptableObject
{
	public List<BoxSpecialEvent> m_specialEvents;
}
