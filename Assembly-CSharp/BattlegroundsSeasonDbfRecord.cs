using System;
using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

[Serializable]
public class BattlegroundsSeasonDbfRecord : DbfRecord
{
	[SerializeField]
	private EventTimingType m_event = EventTimingType.UNKNOWN;

	public override object GetVar(string name)
	{
		if (name == "EVENT")
		{
			return m_event;
		}
		return null;
	}

	public override void SetVar(string name, object val)
	{
		if (name == "EVENT")
		{
			m_event = DbfShared.GetEventMap().ConvertStringToSpecialEvent((string)val);
		}
	}

	public override Type GetVarType(string name)
	{
		if (name == "EVENT")
		{
			return typeof(string);
		}
		return null;
	}

	public override IEnumerator<IAsyncJobResult> Job_LoadRecordsFromAssetAsync<T>(string resourcePath, Action<List<T>> resultHandler)
	{
		LoadBattlegroundsSeasonDbfRecords loadRecords = new LoadBattlegroundsSeasonDbfRecords(resourcePath);
		yield return loadRecords;
		resultHandler?.Invoke(loadRecords.GetRecords() as List<T>);
	}

	public override bool LoadRecordsFromAsset<T>(string resourcePath, out List<T> records)
	{
		BattlegroundsSeasonDbfAsset dbfAsset = DbfShared.GetAssetBundle().LoadAsset(resourcePath, typeof(BattlegroundsSeasonDbfAsset)) as BattlegroundsSeasonDbfAsset;
		if (dbfAsset == null)
		{
			records = new List<T>();
			Debug.LogError($"BattlegroundsSeasonDbfAsset.LoadRecordsFromAsset() - failed to load records from assetbundle: {resourcePath}");
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
