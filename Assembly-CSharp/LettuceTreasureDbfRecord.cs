using System;
using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

[Serializable]
public class LettuceTreasureDbfRecord : DbfRecord
{
	[SerializeField]
	private int m_cardId;

	[SerializeField]
	private int m_requiredAbilityId;

	[DbfField("CARD_ID")]
	public int CardId => m_cardId;

	public List<ScalingTreasureCardTagDbfRecord> Tags
	{
		get
		{
			int id = base.ID;
			List<ScalingTreasureCardTagDbfRecord> returnRecords = new List<ScalingTreasureCardTagDbfRecord>();
			List<ScalingTreasureCardTagDbfRecord> records = GameDbf.ScalingTreasureCardTag.GetRecords();
			int i = 0;
			for (int iMax = records.Count; i < iMax; i++)
			{
				ScalingTreasureCardTagDbfRecord record = records[i];
				if (record.LettuceTreasureId == id)
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
			"CARD_ID" => m_cardId, 
			"REQUIRED_ABILITY" => m_requiredAbilityId, 
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
		case "CARD_ID":
			m_cardId = (int)val;
			break;
		case "REQUIRED_ABILITY":
			m_requiredAbilityId = (int)val;
			break;
		}
	}

	public override Type GetVarType(string name)
	{
		return name switch
		{
			"ID" => typeof(int), 
			"CARD_ID" => typeof(int), 
			"REQUIRED_ABILITY" => typeof(int), 
			_ => null, 
		};
	}

	public override IEnumerator<IAsyncJobResult> Job_LoadRecordsFromAssetAsync<T>(string resourcePath, Action<List<T>> resultHandler)
	{
		LoadLettuceTreasureDbfRecords loadRecords = new LoadLettuceTreasureDbfRecords(resourcePath);
		yield return loadRecords;
		resultHandler?.Invoke(loadRecords.GetRecords() as List<T>);
	}

	public override bool LoadRecordsFromAsset<T>(string resourcePath, out List<T> records)
	{
		LettuceTreasureDbfAsset dbfAsset = DbfShared.GetAssetBundle().LoadAsset(resourcePath, typeof(LettuceTreasureDbfAsset)) as LettuceTreasureDbfAsset;
		if (dbfAsset == null)
		{
			records = new List<T>();
			Debug.LogError($"LettuceTreasureDbfAsset.LoadRecordsFromAsset() - failed to load records from assetbundle: {resourcePath}");
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
