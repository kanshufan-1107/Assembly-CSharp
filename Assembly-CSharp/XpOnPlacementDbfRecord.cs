using System;
using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

[Serializable]
public class XpOnPlacementDbfRecord : DbfRecord
{
	[SerializeField]
	private int m_rewardTrackId;

	public override object GetVar(string name)
	{
		if (!(name == "ID"))
		{
			if (name == "REWARD_TRACK_ID")
			{
				return m_rewardTrackId;
			}
			return null;
		}
		return base.ID;
	}

	public override void SetVar(string name, object val)
	{
		if (!(name == "ID"))
		{
			if (name == "REWARD_TRACK_ID")
			{
				m_rewardTrackId = (int)val;
			}
		}
		else
		{
			SetID((int)val);
		}
	}

	public override Type GetVarType(string name)
	{
		if (!(name == "ID"))
		{
			if (name == "REWARD_TRACK_ID")
			{
				return typeof(int);
			}
			return null;
		}
		return typeof(int);
	}

	public override IEnumerator<IAsyncJobResult> Job_LoadRecordsFromAssetAsync<T>(string resourcePath, Action<List<T>> resultHandler)
	{
		LoadXpOnPlacementDbfRecords loadRecords = new LoadXpOnPlacementDbfRecords(resourcePath);
		yield return loadRecords;
		resultHandler?.Invoke(loadRecords.GetRecords() as List<T>);
	}

	public override bool LoadRecordsFromAsset<T>(string resourcePath, out List<T> records)
	{
		XpOnPlacementDbfAsset dbfAsset = DbfShared.GetAssetBundle().LoadAsset(resourcePath, typeof(XpOnPlacementDbfAsset)) as XpOnPlacementDbfAsset;
		if (dbfAsset == null)
		{
			records = new List<T>();
			Debug.LogError($"XpOnPlacementDbfAsset.LoadRecordsFromAsset() - failed to load records from assetbundle: {resourcePath}");
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
