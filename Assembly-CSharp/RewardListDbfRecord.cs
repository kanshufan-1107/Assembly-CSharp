using System;
using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

[Serializable]
public class RewardListDbfRecord : DbfRecord
{
	[SerializeField]
	private DbfLocValue m_description;

	[SerializeField]
	private bool m_chooseOne;

	[SerializeField]
	private bool m_locked;

	[DbfField("DESCRIPTION")]
	public DbfLocValue Description => m_description;

	[DbfField("CHOOSE_ONE")]
	public bool ChooseOne => m_chooseOne;

	[DbfField("LOCKED")]
	public bool Locked => m_locked;

	public List<RewardItemDbfRecord> RewardItems
	{
		get
		{
			int id = base.ID;
			List<RewardItemDbfRecord> returnRecords = new List<RewardItemDbfRecord>();
			List<RewardItemDbfRecord> records = GameDbf.RewardItem.GetRecords();
			int i = 0;
			for (int iMax = records.Count; i < iMax; i++)
			{
				RewardItemDbfRecord record = records[i];
				if (record.RewardListId == id)
				{
					returnRecords.Add(record);
				}
			}
			return returnRecords;
		}
	}

	public override object GetVar(string name)
	{
		return name switch
		{
			"ID" => base.ID, 
			"DESCRIPTION" => m_description, 
			"CHOOSE_ONE" => m_chooseOne, 
			"LOCKED" => m_locked, 
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
		case "DESCRIPTION":
			m_description = (DbfLocValue)val;
			break;
		case "CHOOSE_ONE":
			m_chooseOne = (bool)val;
			break;
		case "LOCKED":
			m_locked = (bool)val;
			break;
		}
	}

	public override Type GetVarType(string name)
	{
		return name switch
		{
			"ID" => typeof(int), 
			"DESCRIPTION" => typeof(DbfLocValue), 
			"CHOOSE_ONE" => typeof(bool), 
			"LOCKED" => typeof(bool), 
			_ => null, 
		};
	}

	public override IEnumerator<IAsyncJobResult> Job_LoadRecordsFromAssetAsync<T>(string resourcePath, Action<List<T>> resultHandler)
	{
		LoadRewardListDbfRecords loadRecords = new LoadRewardListDbfRecords(resourcePath);
		yield return loadRecords;
		resultHandler?.Invoke(loadRecords.GetRecords() as List<T>);
	}

	public override bool LoadRecordsFromAsset<T>(string resourcePath, out List<T> records)
	{
		RewardListDbfAsset dbfAsset = DbfShared.GetAssetBundle().LoadAsset(resourcePath, typeof(RewardListDbfAsset)) as RewardListDbfAsset;
		if (dbfAsset == null)
		{
			records = new List<T>();
			Debug.LogError($"RewardListDbfAsset.LoadRecordsFromAsset() - failed to load records from assetbundle: {resourcePath}");
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
		m_description.StripUnusedLocales();
	}
}
