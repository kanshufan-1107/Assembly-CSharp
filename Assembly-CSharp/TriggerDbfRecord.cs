using System;
using System.Collections.Generic;
using Assets;
using Blizzard.T5.Jobs;
using UnityEngine;

[Serializable]
public class TriggerDbfRecord : DbfRecord
{
	[SerializeField]
	private Trigger.Triggertype m_triggerType = Trigger.ParseTriggertypeValue("lua");

	public override object GetVar(string name)
	{
		if (!(name == "ID"))
		{
			if (name == "TRIGGER_TYPE")
			{
				return m_triggerType;
			}
			return null;
		}
		return base.ID;
	}

	public override void SetVar(string name, object val)
	{
		if (!(name == "ID"))
		{
			if (name == "TRIGGER_TYPE")
			{
				if (val == null)
				{
					m_triggerType = Trigger.Triggertype.LUA;
				}
				else if (val is Trigger.Triggertype || val is int)
				{
					m_triggerType = (Trigger.Triggertype)val;
				}
				else if (val is string)
				{
					m_triggerType = Trigger.ParseTriggertypeValue((string)val);
				}
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
			if (name == "TRIGGER_TYPE")
			{
				return typeof(Trigger.Triggertype);
			}
			return null;
		}
		return typeof(int);
	}

	public override IEnumerator<IAsyncJobResult> Job_LoadRecordsFromAssetAsync<T>(string resourcePath, Action<List<T>> resultHandler)
	{
		LoadTriggerDbfRecords loadRecords = new LoadTriggerDbfRecords(resourcePath);
		yield return loadRecords;
		resultHandler?.Invoke(loadRecords.GetRecords() as List<T>);
	}

	public override bool LoadRecordsFromAsset<T>(string resourcePath, out List<T> records)
	{
		TriggerDbfAsset dbfAsset = DbfShared.GetAssetBundle().LoadAsset(resourcePath, typeof(TriggerDbfAsset)) as TriggerDbfAsset;
		if (dbfAsset == null)
		{
			records = new List<T>();
			Debug.LogError($"TriggerDbfAsset.LoadRecordsFromAsset() - failed to load records from assetbundle: {resourcePath}");
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
