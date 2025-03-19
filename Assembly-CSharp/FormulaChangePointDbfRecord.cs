using System;
using System.Collections.Generic;
using Assets;
using Blizzard.T5.Jobs;
using UnityEngine;

[Serializable]
public class FormulaChangePointDbfRecord : DbfRecord
{
	[SerializeField]
	private int m_formulaId;

	[SerializeField]
	private int m_level;

	[SerializeField]
	private FormulaChangePoint.Function m_function;

	[SerializeField]
	private int m_baseValue = 100;

	[SerializeField]
	private int m_rate;

	[DbfField("FORMULA_ID")]
	public int FormulaId => m_formulaId;

	[DbfField("LEVEL")]
	public int Level => m_level;

	[DbfField("FUNCTION")]
	public FormulaChangePoint.Function Function => m_function;

	[DbfField("BASE_VALUE")]
	public int BaseValue => m_baseValue;

	[DbfField("RATE")]
	public int Rate => m_rate;

	public override object GetVar(string name)
	{
		return name switch
		{
			"ID" => base.ID, 
			"FORMULA_ID" => m_formulaId, 
			"LEVEL" => m_level, 
			"FUNCTION" => m_function, 
			"BASE_VALUE" => m_baseValue, 
			"RATE" => m_rate, 
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
		case "FORMULA_ID":
			m_formulaId = (int)val;
			break;
		case "LEVEL":
			m_level = (int)val;
			break;
		case "FUNCTION":
			if (val == null)
			{
				m_function = FormulaChangePoint.Function.NONE;
			}
			else if (val is FormulaChangePoint.Function || val is int)
			{
				m_function = (FormulaChangePoint.Function)val;
			}
			else if (val is string)
			{
				m_function = FormulaChangePoint.ParseFunctionValue((string)val);
			}
			break;
		case "BASE_VALUE":
			m_baseValue = (int)val;
			break;
		case "RATE":
			m_rate = (int)val;
			break;
		}
	}

	public override Type GetVarType(string name)
	{
		return name switch
		{
			"ID" => typeof(int), 
			"FORMULA_ID" => typeof(int), 
			"LEVEL" => typeof(int), 
			"FUNCTION" => typeof(FormulaChangePoint.Function), 
			"BASE_VALUE" => typeof(int), 
			"RATE" => typeof(int), 
			_ => null, 
		};
	}

	public override IEnumerator<IAsyncJobResult> Job_LoadRecordsFromAssetAsync<T>(string resourcePath, Action<List<T>> resultHandler)
	{
		LoadFormulaChangePointDbfRecords loadRecords = new LoadFormulaChangePointDbfRecords(resourcePath);
		yield return loadRecords;
		resultHandler?.Invoke(loadRecords.GetRecords() as List<T>);
	}

	public override bool LoadRecordsFromAsset<T>(string resourcePath, out List<T> records)
	{
		FormulaChangePointDbfAsset dbfAsset = DbfShared.GetAssetBundle().LoadAsset(resourcePath, typeof(FormulaChangePointDbfAsset)) as FormulaChangePointDbfAsset;
		if (dbfAsset == null)
		{
			records = new List<T>();
			Debug.LogError($"FormulaChangePointDbfAsset.LoadRecordsFromAsset() - failed to load records from assetbundle: {resourcePath}");
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
