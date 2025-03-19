using System;
using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

[Serializable]
public class BonusBountyDropChanceDbfRecord : DbfRecord
{
	[SerializeField]
	private int m_lettuceEquipmentTierId;

	[SerializeField]
	private int m_lettuceBountyId;

	[DbfField("LETTUCE_EQUIPMENT_TIER_ID")]
	public int LettuceEquipmentTierId => m_lettuceEquipmentTierId;

	public LettuceBountyDbfRecord LettuceBountyRecord => GameDbf.LettuceBounty.GetRecord(m_lettuceBountyId);

	public override object GetVar(string name)
	{
		return name switch
		{
			"ID" => base.ID, 
			"LETTUCE_EQUIPMENT_TIER_ID" => m_lettuceEquipmentTierId, 
			"LETTUCE_BOUNTY_ID" => m_lettuceBountyId, 
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
		case "LETTUCE_EQUIPMENT_TIER_ID":
			m_lettuceEquipmentTierId = (int)val;
			break;
		case "LETTUCE_BOUNTY_ID":
			m_lettuceBountyId = (int)val;
			break;
		}
	}

	public override Type GetVarType(string name)
	{
		return name switch
		{
			"ID" => typeof(int), 
			"LETTUCE_EQUIPMENT_TIER_ID" => typeof(int), 
			"LETTUCE_BOUNTY_ID" => typeof(int), 
			_ => null, 
		};
	}

	public override IEnumerator<IAsyncJobResult> Job_LoadRecordsFromAssetAsync<T>(string resourcePath, Action<List<T>> resultHandler)
	{
		LoadBonusBountyDropChanceDbfRecords loadRecords = new LoadBonusBountyDropChanceDbfRecords(resourcePath);
		yield return loadRecords;
		resultHandler?.Invoke(loadRecords.GetRecords() as List<T>);
	}

	public override bool LoadRecordsFromAsset<T>(string resourcePath, out List<T> records)
	{
		BonusBountyDropChanceDbfAsset dbfAsset = DbfShared.GetAssetBundle().LoadAsset(resourcePath, typeof(BonusBountyDropChanceDbfAsset)) as BonusBountyDropChanceDbfAsset;
		if (dbfAsset == null)
		{
			records = new List<T>();
			Debug.LogError($"BonusBountyDropChanceDbfAsset.LoadRecordsFromAsset() - failed to load records from assetbundle: {resourcePath}");
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
