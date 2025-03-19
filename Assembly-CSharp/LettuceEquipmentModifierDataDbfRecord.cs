using System;
using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

[Serializable]
public class LettuceEquipmentModifierDataDbfRecord : DbfRecord
{
	[SerializeField]
	private int m_lettuceEquipmentTierId;

	[SerializeField]
	private int m_mercenaryAttackChange;

	[SerializeField]
	private int m_mercenaryHealthChange;

	[DbfField("LETTUCE_EQUIPMENT_TIER_ID")]
	public int LettuceEquipmentTierId => m_lettuceEquipmentTierId;

	[DbfField("MERCENARY_ATTACK_CHANGE")]
	public int MercenaryAttackChange => m_mercenaryAttackChange;

	[DbfField("MERCENARY_HEALTH_CHANGE")]
	public int MercenaryHealthChange => m_mercenaryHealthChange;

	public List<ModifiedLettuceAbilityValueDbfRecord> ModifiedLettuceAbilityValues
	{
		get
		{
			int id = base.ID;
			List<ModifiedLettuceAbilityValueDbfRecord> returnRecords = new List<ModifiedLettuceAbilityValueDbfRecord>();
			List<ModifiedLettuceAbilityValueDbfRecord> records = GameDbf.ModifiedLettuceAbilityValue.GetRecords();
			int i = 0;
			for (int iMax = records.Count; i < iMax; i++)
			{
				ModifiedLettuceAbilityValueDbfRecord record = records[i];
				if (record.LettuceModifierDataId == id)
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
			"LETTUCE_EQUIPMENT_TIER_ID" => m_lettuceEquipmentTierId, 
			"MERCENARY_ATTACK_CHANGE" => m_mercenaryAttackChange, 
			"MERCENARY_HEALTH_CHANGE" => m_mercenaryHealthChange, 
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
		case "MERCENARY_ATTACK_CHANGE":
			m_mercenaryAttackChange = (int)val;
			break;
		case "MERCENARY_HEALTH_CHANGE":
			m_mercenaryHealthChange = (int)val;
			break;
		}
	}

	public override Type GetVarType(string name)
	{
		return name switch
		{
			"ID" => typeof(int), 
			"LETTUCE_EQUIPMENT_TIER_ID" => typeof(int), 
			"MERCENARY_ATTACK_CHANGE" => typeof(int), 
			"MERCENARY_HEALTH_CHANGE" => typeof(int), 
			_ => null, 
		};
	}

	public override IEnumerator<IAsyncJobResult> Job_LoadRecordsFromAssetAsync<T>(string resourcePath, Action<List<T>> resultHandler)
	{
		LoadLettuceEquipmentModifierDataDbfRecords loadRecords = new LoadLettuceEquipmentModifierDataDbfRecords(resourcePath);
		yield return loadRecords;
		resultHandler?.Invoke(loadRecords.GetRecords() as List<T>);
	}

	public override bool LoadRecordsFromAsset<T>(string resourcePath, out List<T> records)
	{
		LettuceEquipmentModifierDataDbfAsset dbfAsset = DbfShared.GetAssetBundle().LoadAsset(resourcePath, typeof(LettuceEquipmentModifierDataDbfAsset)) as LettuceEquipmentModifierDataDbfAsset;
		if (dbfAsset == null)
		{
			records = new List<T>();
			Debug.LogError($"LettuceEquipmentModifierDataDbfAsset.LoadRecordsFromAsset() - failed to load records from assetbundle: {resourcePath}");
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
