using System;
using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

[Serializable]
public class QuestModifierDbfRecord : DbfRecord
{
	[SerializeField]
	private EventTimingType m_event = DbfShared.GetEventMap().ConvertStringToSpecialEvent("none");

	[SerializeField]
	private int m_quota;

	[SerializeField]
	private string m_description;

	[SerializeField]
	private string m_styleName;

	[SerializeField]
	private DbfLocValue m_questName;

	public override object GetVar(string name)
	{
		return name switch
		{
			"ID" => base.ID, 
			"EVENT" => m_event, 
			"QUOTA" => m_quota, 
			"DESCRIPTION" => m_description, 
			"STYLE_NAME" => m_styleName, 
			"QUEST_NAME" => m_questName, 
			_ => null, 
		};
	}

	public override void SetVar(string name, object val)
	{
		switch (name)
		{
		case "ID":
			SetID((int)val);
			break;
		case "EVENT":
			m_event = DbfShared.GetEventMap().ConvertStringToSpecialEvent((string)val);
			break;
		case "QUOTA":
			m_quota = (int)val;
			break;
		case "DESCRIPTION":
			m_description = (string)val;
			break;
		case "STYLE_NAME":
			m_styleName = (string)val;
			break;
		case "QUEST_NAME":
			m_questName = (DbfLocValue)val;
			break;
		}
	}

	public override Type GetVarType(string name)
	{
		return name switch
		{
			"ID" => typeof(int), 
			"EVENT" => typeof(string), 
			"QUOTA" => typeof(int), 
			"DESCRIPTION" => typeof(string), 
			"STYLE_NAME" => typeof(string), 
			"QUEST_NAME" => typeof(DbfLocValue), 
			_ => null, 
		};
	}

	public override IEnumerator<IAsyncJobResult> Job_LoadRecordsFromAssetAsync<T>(string resourcePath, Action<List<T>> resultHandler)
	{
		LoadQuestModifierDbfRecords loadRecords = new LoadQuestModifierDbfRecords(resourcePath);
		yield return loadRecords;
		resultHandler?.Invoke(loadRecords.GetRecords() as List<T>);
	}

	public override bool LoadRecordsFromAsset<T>(string resourcePath, out List<T> records)
	{
		QuestModifierDbfAsset dbfAsset = DbfShared.GetAssetBundle().LoadAsset(resourcePath, typeof(QuestModifierDbfAsset)) as QuestModifierDbfAsset;
		if (dbfAsset == null)
		{
			records = new List<T>();
			Debug.LogError($"QuestModifierDbfAsset.LoadRecordsFromAsset() - failed to load records from assetbundle: {resourcePath}");
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
		m_questName.StripUnusedLocales();
	}
}
