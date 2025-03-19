using System;
using System.Collections.Generic;
using Assets;
using Blizzard.T5.Jobs;
using UnityEngine;

[Serializable]
public class PlayerExperimentDbfRecord : DbfRecord
{
	[SerializeField]
	private string m_displayName;

	[SerializeField]
	private string m_experimentKey;

	[SerializeField]
	private PlayerExperiment.TestFeature m_testFeature;

	public override object GetVar(string name)
	{
		return name switch
		{
			"ID" => base.ID, 
			"DISPLAY_NAME" => m_displayName, 
			"EXPERIMENT_KEY" => m_experimentKey, 
			"TEST_FEATURE" => m_testFeature, 
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
		case "DISPLAY_NAME":
			m_displayName = (string)val;
			break;
		case "EXPERIMENT_KEY":
			m_experimentKey = (string)val;
			break;
		case "TEST_FEATURE":
			if (val == null)
			{
				m_testFeature = PlayerExperiment.TestFeature.INVALID;
			}
			else if (val is PlayerExperiment.TestFeature || val is int)
			{
				m_testFeature = (PlayerExperiment.TestFeature)val;
			}
			else if (val is string)
			{
				m_testFeature = PlayerExperiment.ParseTestFeatureValue((string)val);
			}
			break;
		}
	}

	public override Type GetVarType(string name)
	{
		return name switch
		{
			"ID" => typeof(int), 
			"DISPLAY_NAME" => typeof(string), 
			"EXPERIMENT_KEY" => typeof(string), 
			"TEST_FEATURE" => typeof(PlayerExperiment.TestFeature), 
			_ => null, 
		};
	}

	public override IEnumerator<IAsyncJobResult> Job_LoadRecordsFromAssetAsync<T>(string resourcePath, Action<List<T>> resultHandler)
	{
		LoadPlayerExperimentDbfRecords loadRecords = new LoadPlayerExperimentDbfRecords(resourcePath);
		yield return loadRecords;
		resultHandler?.Invoke(loadRecords.GetRecords() as List<T>);
	}

	public override bool LoadRecordsFromAsset<T>(string resourcePath, out List<T> records)
	{
		PlayerExperimentDbfAsset dbfAsset = DbfShared.GetAssetBundle().LoadAsset(resourcePath, typeof(PlayerExperimentDbfAsset)) as PlayerExperimentDbfAsset;
		if (dbfAsset == null)
		{
			records = new List<T>();
			Debug.LogError($"PlayerExperimentDbfAsset.LoadRecordsFromAsset() - failed to load records from assetbundle: {resourcePath}");
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
