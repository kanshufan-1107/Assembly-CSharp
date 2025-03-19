using System;
using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

[Serializable]
public class QuestDialogDbfRecord : DbfRecord
{
	[SerializeField]
	private int m_onCompleteBannerId;

	[SerializeField]
	private string m_noteDesc;

	public override object GetVar(string name)
	{
		return name switch
		{
			"ID" => base.ID, 
			"ON_COMPLETE_BANNER_ID" => m_onCompleteBannerId, 
			"NOTE_DESC" => m_noteDesc, 
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
		case "ON_COMPLETE_BANNER_ID":
			m_onCompleteBannerId = (int)val;
			break;
		case "NOTE_DESC":
			m_noteDesc = (string)val;
			break;
		}
	}

	public override Type GetVarType(string name)
	{
		return name switch
		{
			"ID" => typeof(int), 
			"ON_COMPLETE_BANNER_ID" => typeof(int), 
			"NOTE_DESC" => typeof(string), 
			_ => null, 
		};
	}

	public override IEnumerator<IAsyncJobResult> Job_LoadRecordsFromAssetAsync<T>(string resourcePath, Action<List<T>> resultHandler)
	{
		LoadQuestDialogDbfRecords loadRecords = new LoadQuestDialogDbfRecords(resourcePath);
		yield return loadRecords;
		resultHandler?.Invoke(loadRecords.GetRecords() as List<T>);
	}

	public override bool LoadRecordsFromAsset<T>(string resourcePath, out List<T> records)
	{
		QuestDialogDbfAsset dbfAsset = DbfShared.GetAssetBundle().LoadAsset(resourcePath, typeof(QuestDialogDbfAsset)) as QuestDialogDbfAsset;
		if (dbfAsset == null)
		{
			records = new List<T>();
			Debug.LogError($"QuestDialogDbfAsset.LoadRecordsFromAsset() - failed to load records from assetbundle: {resourcePath}");
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
