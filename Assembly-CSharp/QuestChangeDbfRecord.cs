using System;
using System.Collections.Generic;
using Assets;
using Blizzard.T5.Jobs;
using UnityEngine;

[Serializable]
public class QuestChangeDbfRecord : DbfRecord
{
	[SerializeField]
	private int m_questId;

	[SerializeField]
	private QuestChange.ChangeAttribute m_changeAttribute = QuestChange.ParseChangeAttributeValue("Invalid");

	[SerializeField]
	private QuestChange.ChangeType m_changeType = QuestChange.ParseChangeTypeValue("Invalid");

	[SerializeField]
	private EventTimingType m_event = DbfShared.GetEventMap().ConvertStringToSpecialEvent("never");

	[DbfField("QUEST_ID")]
	public int QuestId => m_questId;

	[DbfField("CHANGE_ATTRIBUTE")]
	public QuestChange.ChangeAttribute ChangeAttribute => m_changeAttribute;

	[DbfField("CHANGE_TYPE")]
	public QuestChange.ChangeType ChangeType => m_changeType;

	[DbfField("EVENT")]
	public EventTimingType Event => m_event;

	public override object GetVar(string name)
	{
		return name switch
		{
			"QUEST_ID" => m_questId, 
			"CHANGE_ATTRIBUTE" => m_changeAttribute, 
			"CHANGE_TYPE" => m_changeType, 
			"EVENT" => m_event, 
			_ => null, 
		};
	}

	public override void SetVar(string name, object val)
	{
		switch (name)
		{
		case "QUEST_ID":
			m_questId = (int)val;
			break;
		case "CHANGE_ATTRIBUTE":
			if (val == null)
			{
				m_changeAttribute = QuestChange.ChangeAttribute.INVALID;
			}
			else if (val is QuestChange.ChangeAttribute || val is int)
			{
				m_changeAttribute = (QuestChange.ChangeAttribute)val;
			}
			else if (val is string)
			{
				m_changeAttribute = QuestChange.ParseChangeAttributeValue((string)val);
			}
			break;
		case "CHANGE_TYPE":
			if (val == null)
			{
				m_changeType = QuestChange.ChangeType.INVALID;
			}
			else if (val is QuestChange.ChangeType || val is int)
			{
				m_changeType = (QuestChange.ChangeType)val;
			}
			else if (val is string)
			{
				m_changeType = QuestChange.ParseChangeTypeValue((string)val);
			}
			break;
		case "EVENT":
			m_event = DbfShared.GetEventMap().ConvertStringToSpecialEvent((string)val);
			break;
		}
	}

	public override Type GetVarType(string name)
	{
		return name switch
		{
			"QUEST_ID" => typeof(int), 
			"CHANGE_ATTRIBUTE" => typeof(QuestChange.ChangeAttribute), 
			"CHANGE_TYPE" => typeof(QuestChange.ChangeType), 
			"EVENT" => typeof(string), 
			_ => null, 
		};
	}

	public override IEnumerator<IAsyncJobResult> Job_LoadRecordsFromAssetAsync<T>(string resourcePath, Action<List<T>> resultHandler)
	{
		LoadQuestChangeDbfRecords loadRecords = new LoadQuestChangeDbfRecords(resourcePath);
		yield return loadRecords;
		resultHandler?.Invoke(loadRecords.GetRecords() as List<T>);
	}

	public override bool LoadRecordsFromAsset<T>(string resourcePath, out List<T> records)
	{
		QuestChangeDbfAsset dbfAsset = DbfShared.GetAssetBundle().LoadAsset(resourcePath, typeof(QuestChangeDbfAsset)) as QuestChangeDbfAsset;
		if (dbfAsset == null)
		{
			records = new List<T>();
			Debug.LogError($"QuestChangeDbfAsset.LoadRecordsFromAsset() - failed to load records from assetbundle: {resourcePath}");
			return false;
		}
		for (int i = 0; i < dbfAsset.Records.Count; i++)
		{
			dbfAsset.Records[i].StripUnusedLocales();
		}
		records = dbfAsset.Records as List<T>;
		return true;
	}

	public override bool SaveRecordsToAsset<T>(string assetPath, List<T> records)
	{
		return false;
	}

	public override void StripUnusedLocales()
	{
	}
}
