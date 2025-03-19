using System;
using System.Collections.Generic;
using Blizzard.T5.Core.Utils;
using UnityEngine;

[Serializable]
public class EventTimingMap : ScriptableObject
{
	public const string kAssetFullPath = "Assets/Game/DBF-Asset/EventMap.asset";

	[SerializeField]
	private int m_currentId = 10000000;

	[SerializeField]
	private bool m_mappingInit;

	[SerializeField]
	private List<string> m_Keys = new List<string>();

	[SerializeField]
	private List<int> m_Values = new List<int>();

	private Dictionary<string, EventTimingType> m_eventMap = new Dictionary<string, EventTimingType>();

	public int CurrentId => m_currentId;

	public List<string> Keys => m_Keys;

	public List<int> Values => m_Values;

	public void Reset()
	{
		m_currentId = 10000000;
		m_mappingInit = false;
		m_Keys.Clear();
		m_Values.Clear();
		m_eventMap.Clear();
	}

	public void Initialize()
	{
		for (int i = 0; i < m_Keys.Count; i++)
		{
			m_eventMap.Add(m_Keys[i], (EventTimingType)m_Values[i]);
		}
	}

	public EventTimingType ConvertStringToSpecialEvent(string eventName)
	{
		if (string.IsNullOrEmpty(eventName))
		{
			return EventTimingType.UNKNOWN;
		}
		if (!m_mappingInit)
		{
			m_mappingInit = true;
			foreach (EventTimingType enumVal in Enum.GetValues(typeof(EventTimingType)))
			{
				string enumDesc = EnumUtils.GetString(enumVal);
				m_eventMap.Add(enumDesc, enumVal);
				m_Keys.Add(enumDesc);
				m_Values.Add((int)enumVal);
			}
		}
		if (m_eventMap.TryGetValue(eventName, out var retValue))
		{
			return retValue;
		}
		m_currentId++;
		retValue = (EventTimingType)m_currentId;
		m_eventMap[eventName] = retValue;
		m_Keys.Add(eventName);
		m_Values.Add((int)retValue);
		return retValue;
	}
}
