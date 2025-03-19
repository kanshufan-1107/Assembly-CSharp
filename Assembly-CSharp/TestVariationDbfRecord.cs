using System;
using System.Collections.Generic;
using Assets;
using Blizzard.T5.Jobs;
using UnityEngine;

[Serializable]
public class TestVariationDbfRecord : DbfRecord
{
	[SerializeField]
	private int m_playerExperimentId;

	[SerializeField]
	private string m_variationKey;

	[SerializeField]
	private TestVariation.TestGroup m_testGroup;

	public override object GetVar(string name)
	{
		return name switch
		{
			"ID" => base.ID, 
			"PLAYER_EXPERIMENT_ID" => m_playerExperimentId, 
			"VARIATION_KEY" => m_variationKey, 
			"TEST_GROUP" => m_testGroup, 
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
		case "PLAYER_EXPERIMENT_ID":
			m_playerExperimentId = (int)val;
			break;
		case "VARIATION_KEY":
			m_variationKey = (string)val;
			break;
		case "TEST_GROUP":
			if (val == null)
			{
				m_testGroup = TestVariation.TestGroup.CONTROL_GROUP;
			}
			else if (val is TestVariation.TestGroup || val is int)
			{
				m_testGroup = (TestVariation.TestGroup)val;
			}
			else if (val is string)
			{
				m_testGroup = TestVariation.ParseTestGroupValue((string)val);
			}
			break;
		}
	}

	public override Type GetVarType(string name)
	{
		return name switch
		{
			"ID" => typeof(int), 
			"PLAYER_EXPERIMENT_ID" => typeof(int), 
			"VARIATION_KEY" => typeof(string), 
			"TEST_GROUP" => typeof(TestVariation.TestGroup), 
			_ => null, 
		};
	}

	public override IEnumerator<IAsyncJobResult> Job_LoadRecordsFromAssetAsync<T>(string resourcePath, Action<List<T>> resultHandler)
	{
		LoadTestVariationDbfRecords loadRecords = new LoadTestVariationDbfRecords(resourcePath);
		yield return loadRecords;
		resultHandler?.Invoke(loadRecords.GetRecords() as List<T>);
	}

	public override bool LoadRecordsFromAsset<T>(string resourcePath, out List<T> records)
	{
		TestVariationDbfAsset dbfAsset = DbfShared.GetAssetBundle().LoadAsset(resourcePath, typeof(TestVariationDbfAsset)) as TestVariationDbfAsset;
		if (dbfAsset == null)
		{
			records = new List<T>();
			Debug.LogError($"TestVariationDbfAsset.LoadRecordsFromAsset() - failed to load records from assetbundle: {resourcePath}");
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
