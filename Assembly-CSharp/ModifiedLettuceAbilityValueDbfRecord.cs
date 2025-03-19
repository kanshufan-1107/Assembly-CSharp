using System;
using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

[Serializable]
public class ModifiedLettuceAbilityValueDbfRecord : DbfRecord
{
	[SerializeField]
	private int m_lettuceModifierDataId;

	[SerializeField]
	private int m_lettuceAbilityId;

	[SerializeField]
	private int m_attackChange;

	[SerializeField]
	private int m_healthChange;

	[SerializeField]
	private int m_speedChange;

	[SerializeField]
	private int m_cooldownChange;

	[SerializeField]
	private int m_scriptDataNum1Change;

	[SerializeField]
	private int m_scriptDataNum2Change;

	[SerializeField]
	private int m_scriptDataNum3Change;

	[DbfField("LETTUCE_MODIFIER_DATA_ID")]
	public int LettuceModifierDataId => m_lettuceModifierDataId;

	[DbfField("LETTUCE_ABILITY_ID")]
	public int LettuceAbilityId => m_lettuceAbilityId;

	[DbfField("ATTACK_CHANGE")]
	public int AttackChange => m_attackChange;

	[DbfField("HEALTH_CHANGE")]
	public int HealthChange => m_healthChange;

	[DbfField("SPEED_CHANGE")]
	public int SpeedChange => m_speedChange;

	[DbfField("COOLDOWN_CHANGE")]
	public int CooldownChange => m_cooldownChange;

	[DbfField("SCRIPT_DATA_NUM_1_CHANGE")]
	public int ScriptDataNum1Change => m_scriptDataNum1Change;

	[DbfField("SCRIPT_DATA_NUM_2_CHANGE")]
	public int ScriptDataNum2Change => m_scriptDataNum2Change;

	[DbfField("SCRIPT_DATA_NUM_3_CHANGE")]
	public int ScriptDataNum3Change => m_scriptDataNum3Change;

	public List<ModifiedLettuceAbilityCardTagDbfRecord> Tags
	{
		get
		{
			int id = base.ID;
			List<ModifiedLettuceAbilityCardTagDbfRecord> returnRecords = new List<ModifiedLettuceAbilityCardTagDbfRecord>();
			List<ModifiedLettuceAbilityCardTagDbfRecord> records = GameDbf.ModifiedLettuceAbilityCardTag.GetRecords();
			int i = 0;
			for (int iMax = records.Count; i < iMax; i++)
			{
				ModifiedLettuceAbilityCardTagDbfRecord record = records[i];
				if (record.ModifiedLettuceAbilityValueId == id)
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
			"LETTUCE_MODIFIER_DATA_ID" => m_lettuceModifierDataId, 
			"LETTUCE_ABILITY_ID" => m_lettuceAbilityId, 
			"ATTACK_CHANGE" => m_attackChange, 
			"HEALTH_CHANGE" => m_healthChange, 
			"SPEED_CHANGE" => m_speedChange, 
			"COOLDOWN_CHANGE" => m_cooldownChange, 
			"SCRIPT_DATA_NUM_1_CHANGE" => m_scriptDataNum1Change, 
			"SCRIPT_DATA_NUM_2_CHANGE" => m_scriptDataNum2Change, 
			"SCRIPT_DATA_NUM_3_CHANGE" => m_scriptDataNum3Change, 
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
		case "LETTUCE_MODIFIER_DATA_ID":
			m_lettuceModifierDataId = (int)val;
			break;
		case "LETTUCE_ABILITY_ID":
			m_lettuceAbilityId = (int)val;
			break;
		case "ATTACK_CHANGE":
			m_attackChange = (int)val;
			break;
		case "HEALTH_CHANGE":
			m_healthChange = (int)val;
			break;
		case "SPEED_CHANGE":
			m_speedChange = (int)val;
			break;
		case "COOLDOWN_CHANGE":
			m_cooldownChange = (int)val;
			break;
		case "SCRIPT_DATA_NUM_1_CHANGE":
			m_scriptDataNum1Change = (int)val;
			break;
		case "SCRIPT_DATA_NUM_2_CHANGE":
			m_scriptDataNum2Change = (int)val;
			break;
		case "SCRIPT_DATA_NUM_3_CHANGE":
			m_scriptDataNum3Change = (int)val;
			break;
		}
	}

	public override Type GetVarType(string name)
	{
		return name switch
		{
			"ID" => typeof(int), 
			"LETTUCE_MODIFIER_DATA_ID" => typeof(int), 
			"LETTUCE_ABILITY_ID" => typeof(int), 
			"ATTACK_CHANGE" => typeof(int), 
			"HEALTH_CHANGE" => typeof(int), 
			"SPEED_CHANGE" => typeof(int), 
			"COOLDOWN_CHANGE" => typeof(int), 
			"SCRIPT_DATA_NUM_1_CHANGE" => typeof(int), 
			"SCRIPT_DATA_NUM_2_CHANGE" => typeof(int), 
			"SCRIPT_DATA_NUM_3_CHANGE" => typeof(int), 
			_ => null, 
		};
	}

	public override IEnumerator<IAsyncJobResult> Job_LoadRecordsFromAssetAsync<T>(string resourcePath, Action<List<T>> resultHandler)
	{
		LoadModifiedLettuceAbilityValueDbfRecords loadRecords = new LoadModifiedLettuceAbilityValueDbfRecords(resourcePath);
		yield return loadRecords;
		resultHandler?.Invoke(loadRecords.GetRecords() as List<T>);
	}

	public override bool LoadRecordsFromAsset<T>(string resourcePath, out List<T> records)
	{
		ModifiedLettuceAbilityValueDbfAsset dbfAsset = DbfShared.GetAssetBundle().LoadAsset(resourcePath, typeof(ModifiedLettuceAbilityValueDbfAsset)) as ModifiedLettuceAbilityValueDbfAsset;
		if (dbfAsset == null)
		{
			records = new List<T>();
			Debug.LogError($"ModifiedLettuceAbilityValueDbfAsset.LoadRecordsFromAsset() - failed to load records from assetbundle: {resourcePath}");
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
