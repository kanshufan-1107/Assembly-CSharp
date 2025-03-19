using System;
using System.Collections.Generic;
using Assets;
using Blizzard.T5.Jobs;
using UnityEngine;

[Serializable]
public class VisitorTaskDbfRecord : DbfRecord
{
	[SerializeField]
	private int m_mercenaryVisitorId;

	[SerializeField]
	private int m_mercenaryOverrideId;

	[SerializeField]
	private DbfLocValue m_taskTitle;

	[SerializeField]
	private DbfLocValue m_taskDescription;

	[SerializeField]
	private DbfLocValue m_mercenaryQuote;

	[SerializeField]
	private string m_mercenaryTaskBarkVo;

	[SerializeField]
	private int m_onAssignedDialogId;

	[SerializeField]
	private int m_onCompleteDialogId;

	[SerializeField]
	private int m_quota = 1;

	[SerializeField]
	private VisitorTask.VillageTutorialServerEvent m_tutorialEventType;

	[SerializeField]
	private int m_tutorialEventValue;

	[SerializeField]
	private int m_rewardListId;

	[DbfField("MERCENARY_VISITOR_ID")]
	public int MercenaryVisitorId => m_mercenaryVisitorId;

	[DbfField("MERCENARY_OVERRIDE")]
	public int MercenaryOverride => m_mercenaryOverrideId;

	[DbfField("TASK_TITLE")]
	public DbfLocValue TaskTitle => m_taskTitle;

	[DbfField("TASK_DESCRIPTION")]
	public DbfLocValue TaskDescription => m_taskDescription;

	[DbfField("MERCENARY_QUOTE")]
	public DbfLocValue MercenaryQuote => m_mercenaryQuote;

	[DbfField("MERCENARY_TASK_BARK_VO")]
	public string MercenaryTaskBarkVo => m_mercenaryTaskBarkVo;

	[DbfField("ON_ASSIGNED_DIALOG")]
	public int OnAssignedDialog => m_onAssignedDialogId;

	[DbfField("ON_COMPLETE_DIALOG")]
	public int OnCompleteDialog => m_onCompleteDialogId;

	[DbfField("QUOTA")]
	public int Quota => m_quota;

	public RewardListDbfRecord RewardListRecord => GameDbf.RewardList.GetRecord(m_rewardListId);

	public override object GetVar(string name)
	{
		return name switch
		{
			"ID" => base.ID, 
			"MERCENARY_VISITOR_ID" => m_mercenaryVisitorId, 
			"MERCENARY_OVERRIDE" => m_mercenaryOverrideId, 
			"TASK_TITLE" => m_taskTitle, 
			"TASK_DESCRIPTION" => m_taskDescription, 
			"MERCENARY_QUOTE" => m_mercenaryQuote, 
			"MERCENARY_TASK_BARK_VO" => m_mercenaryTaskBarkVo, 
			"ON_ASSIGNED_DIALOG" => m_onAssignedDialogId, 
			"ON_COMPLETE_DIALOG" => m_onCompleteDialogId, 
			"QUOTA" => m_quota, 
			"TUTORIAL_EVENT_TYPE" => m_tutorialEventType, 
			"TUTORIAL_EVENT_VALUE" => m_tutorialEventValue, 
			"REWARD_LIST" => m_rewardListId, 
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
		case "MERCENARY_VISITOR_ID":
			m_mercenaryVisitorId = (int)val;
			break;
		case "MERCENARY_OVERRIDE":
			m_mercenaryOverrideId = (int)val;
			break;
		case "TASK_TITLE":
			m_taskTitle = (DbfLocValue)val;
			break;
		case "TASK_DESCRIPTION":
			m_taskDescription = (DbfLocValue)val;
			break;
		case "MERCENARY_QUOTE":
			m_mercenaryQuote = (DbfLocValue)val;
			break;
		case "MERCENARY_TASK_BARK_VO":
			m_mercenaryTaskBarkVo = (string)val;
			break;
		case "ON_ASSIGNED_DIALOG":
			m_onAssignedDialogId = (int)val;
			break;
		case "ON_COMPLETE_DIALOG":
			m_onCompleteDialogId = (int)val;
			break;
		case "QUOTA":
			m_quota = (int)val;
			break;
		case "TUTORIAL_EVENT_TYPE":
			if (val == null)
			{
				m_tutorialEventType = VisitorTask.VillageTutorialServerEvent.NONE;
			}
			else if (val is VisitorTask.VillageTutorialServerEvent || val is int)
			{
				m_tutorialEventType = (VisitorTask.VillageTutorialServerEvent)val;
			}
			else if (val is string)
			{
				m_tutorialEventType = VisitorTask.ParseVillageTutorialServerEventValue((string)val);
			}
			break;
		case "TUTORIAL_EVENT_VALUE":
			m_tutorialEventValue = (int)val;
			break;
		case "REWARD_LIST":
			m_rewardListId = (int)val;
			break;
		}
	}

	public override Type GetVarType(string name)
	{
		return name switch
		{
			"ID" => typeof(int), 
			"MERCENARY_VISITOR_ID" => typeof(int), 
			"MERCENARY_OVERRIDE" => typeof(int), 
			"TASK_TITLE" => typeof(DbfLocValue), 
			"TASK_DESCRIPTION" => typeof(DbfLocValue), 
			"MERCENARY_QUOTE" => typeof(DbfLocValue), 
			"MERCENARY_TASK_BARK_VO" => typeof(string), 
			"ON_ASSIGNED_DIALOG" => typeof(int), 
			"ON_COMPLETE_DIALOG" => typeof(int), 
			"QUOTA" => typeof(int), 
			"TUTORIAL_EVENT_TYPE" => typeof(VisitorTask.VillageTutorialServerEvent), 
			"TUTORIAL_EVENT_VALUE" => typeof(int), 
			"REWARD_LIST" => typeof(int), 
			_ => null, 
		};
	}

	public override IEnumerator<IAsyncJobResult> Job_LoadRecordsFromAssetAsync<T>(string resourcePath, Action<List<T>> resultHandler)
	{
		LoadVisitorTaskDbfRecords loadRecords = new LoadVisitorTaskDbfRecords(resourcePath);
		yield return loadRecords;
		resultHandler?.Invoke(loadRecords.GetRecords() as List<T>);
	}

	public override bool LoadRecordsFromAsset<T>(string resourcePath, out List<T> records)
	{
		VisitorTaskDbfAsset dbfAsset = DbfShared.GetAssetBundle().LoadAsset(resourcePath, typeof(VisitorTaskDbfAsset)) as VisitorTaskDbfAsset;
		if (dbfAsset == null)
		{
			records = new List<T>();
			Debug.LogError($"VisitorTaskDbfAsset.LoadRecordsFromAsset() - failed to load records from assetbundle: {resourcePath}");
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
		m_taskTitle.StripUnusedLocales();
		m_taskDescription.StripUnusedLocales();
		m_mercenaryQuote.StripUnusedLocales();
	}
}
