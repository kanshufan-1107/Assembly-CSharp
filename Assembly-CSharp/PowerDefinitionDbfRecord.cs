using System;
using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

[Serializable]
public class PowerDefinitionDbfRecord : DbfRecord
{
	[SerializeField]
	private string m_notes;

	public override object GetVar(string name)
	{
		if (name == "NOTES")
		{
			return m_notes;
		}
		return null;
	}

	public override void SetVar(string name, object val)
	{
		if (name == "NOTES")
		{
			m_notes = (string)val;
		}
	}

	public override Type GetVarType(string name)
	{
		if (name == "NOTES")
		{
			return typeof(string);
		}
		return null;
	}

	public override IEnumerator<IAsyncJobResult> Job_LoadRecordsFromAssetAsync<T>(string resourcePath, Action<List<T>> resultHandler)
	{
		LoadPowerDefinitionDbfRecords loadRecords = new LoadPowerDefinitionDbfRecords(resourcePath);
		yield return loadRecords;
		resultHandler?.Invoke(loadRecords.GetRecords() as List<T>);
	}

	public override bool LoadRecordsFromAsset<T>(string resourcePath, out List<T> records)
	{
		PowerDefinitionDbfAsset dbfAsset = DbfShared.GetAssetBundle().LoadAsset(resourcePath, typeof(PowerDefinitionDbfAsset)) as PowerDefinitionDbfAsset;
		if (dbfAsset == null)
		{
			records = new List<T>();
			Debug.LogError($"PowerDefinitionDbfAsset.LoadRecordsFromAsset() - failed to load records from assetbundle: {resourcePath}");
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
