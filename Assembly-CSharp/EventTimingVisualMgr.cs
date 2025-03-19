using System.Collections.Generic;
using UnityEngine;

[CustomEditClass]
public class EventTimingVisualMgr : MonoBehaviour
{
	public List<EventTimingVisualDef> m_EventDefs = new List<EventTimingVisualDef>();

	public bool LoadEvent(EventTimingType eventType)
	{
		for (int i = 0; i < m_EventDefs.Count; i++)
		{
			EventTimingVisualDef def = m_EventDefs[i];
			if (def.m_EventType == eventType)
			{
				AssetLoader.Get().InstantiatePrefab(def.m_Prefab, null);
				return true;
			}
		}
		return false;
	}

	public bool UnloadEvent(EventTimingType eventType)
	{
		for (int i = 0; i < m_EventDefs.Count; i++)
		{
			if (m_EventDefs[i].m_EventType == eventType)
			{
				GameObject eventObj = GameObject.Find(base.name);
				if (eventObj != null)
				{
					Object.Destroy(eventObj);
				}
			}
		}
		return false;
	}

	private void OnEventFinished(Spell spell, object userData)
	{
		if (spell.GetActiveState() == SpellStateType.NONE)
		{
			Object.Destroy(spell.gameObject);
		}
	}
}
