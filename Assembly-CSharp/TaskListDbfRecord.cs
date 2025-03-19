using System;
using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

[Serializable]
public class TaskListDbfRecord : DbfRecord
{
	[SerializeField]
	private int m_visitorTaskChainId;

	[SerializeField]
	private int m_taskId;

	[DbfField("VISITOR_TASK_CHAIN_ID")]
	public int VisitorTaskChainId => m_visitorTaskChainId;

	public VisitorTaskDbfRecord TaskRecord => GameDbf.VisitorTask.GetRecord(m_taskId);

	public override object GetVar(string name)
	{
		return name switch
		{
			"ID" => base.ID, 
			"VISITOR_TASK_CHAIN_ID" => m_visitorTaskChainId, 
			"TASK_ID" => m_taskId, 
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
		case "VISITOR_TASK_CHAIN_ID":
			m_visitorTaskChainId = (int)val;
			break;
		case "TASK_ID":
			m_taskId = (int)val;
			break;
		}
	}

	public override Type GetVarType(string name)
	{
		return name switch
		{
			"ID" => typeof(int), 
			"VISITOR_TASK_CHAIN_ID" => typeof(int), 
			"TASK_ID" => typeof(int), 
			_ => null, 
		};
	}

	public override IEnumerator<IAsyncJobResult> Job_LoadRecordsFromAssetAsync<T>(string resourcePath, Action<List<T>> resultHandler)
	{
		LoadTaskListDbfRecords loadRecords = new LoadTaskListDbfRecords(resourcePath);
		yield return loadRecords;
		resultHandler?.Invoke(loadRecords.GetRecords() as List<T>);
	}

	public override bool LoadRecordsFromAsset<T>(string resourcePath, out List<T> records)
	{
		TaskListDbfAsset dbfAsset = DbfShared.GetAssetBundle().LoadAsset(resourcePath, typeof(TaskListDbfAsset)) as TaskListDbfAsset;
		if (dbfAsset == null)
		{
			records = new List<T>();
			Debug.LogError($"TaskListDbfAsset.LoadRecordsFromAsset() - failed to load records from assetbundle: {resourcePath}");
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
