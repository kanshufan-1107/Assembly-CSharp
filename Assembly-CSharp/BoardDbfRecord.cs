using System;
using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

[Serializable]
public class BoardDbfRecord : DbfRecord
{
	[SerializeField]
	private string m_noteDesc;

	[SerializeField]
	private string m_prefab;

	[DbfField("NOTE_DESC")]
	public string NoteDesc => m_noteDesc;

	[DbfField("PREFAB")]
	public string Prefab => m_prefab;

	public override object GetVar(string name)
	{
		return name switch
		{
			"ID" => base.ID, 
			"NOTE_DESC" => m_noteDesc, 
			"PREFAB" => m_prefab, 
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
		case "NOTE_DESC":
			m_noteDesc = (string)val;
			break;
		case "PREFAB":
			m_prefab = (string)val;
			break;
		}
	}

	public override Type GetVarType(string name)
	{
		return name switch
		{
			"ID" => typeof(int), 
			"NOTE_DESC" => typeof(string), 
			"PREFAB" => typeof(string), 
			_ => null, 
		};
	}

	public override IEnumerator<IAsyncJobResult> Job_LoadRecordsFromAssetAsync<T>(string resourcePath, Action<List<T>> resultHandler)
	{
		LoadBoardDbfRecords loadRecords = new LoadBoardDbfRecords(resourcePath);
		yield return loadRecords;
		resultHandler?.Invoke(loadRecords.GetRecords() as List<T>);
	}

	public override bool LoadRecordsFromAsset<T>(string resourcePath, out List<T> records)
	{
		BoardDbfAsset dbfAsset = DbfShared.GetAssetBundle().LoadAsset(resourcePath, typeof(BoardDbfAsset)) as BoardDbfAsset;
		if (dbfAsset == null)
		{
			records = new List<T>();
			Debug.LogError($"BoardDbfAsset.LoadRecordsFromAsset() - failed to load records from assetbundle: {resourcePath}");
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
