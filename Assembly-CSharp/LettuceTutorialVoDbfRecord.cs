using System;
using System.Collections.Generic;
using Assets;
using Blizzard.T5.Jobs;
using UnityEngine;

[Serializable]
public class LettuceTutorialVoDbfRecord : DbfRecord
{
	[SerializeField]
	private LettuceTutorialVo.LettuceTutorialEvent m_tutorialEvent;

	[SerializeField]
	private LettuceTutorialVo.LettuceTutorialEvent m_triggerEventOnComplete;

	[SerializeField]
	private int m_nodeTypeId;

	[SerializeField]
	private int m_requiredActiveBountyId;

	[SerializeField]
	private int m_requiredActiveVisitorId;

	[SerializeField]
	private int m_requiredActiveTaskId;

	[SerializeField]
	private bool m_onlyShowOnce = true;

	[SerializeField]
	private int m_showChance = 100;

	[SerializeField]
	private string m_uiEvent;

	[SerializeField]
	private string m_popup;

	[SerializeField]
	private int m_tutorialDialogId;

	[DbfField("TUTORIAL_EVENT")]
	public LettuceTutorialVo.LettuceTutorialEvent TutorialEvent => m_tutorialEvent;

	[DbfField("TRIGGER_EVENT_ON_COMPLETE")]
	public LettuceTutorialVo.LettuceTutorialEvent TriggerEventOnComplete => m_triggerEventOnComplete;

	[DbfField("NODE_TYPE_ID")]
	public int NodeTypeId => m_nodeTypeId;

	[DbfField("REQUIRED_ACTIVE_BOUNTY")]
	public int RequiredActiveBounty => m_requiredActiveBountyId;

	[DbfField("REQUIRED_ACTIVE_VISITOR")]
	public int RequiredActiveVisitor => m_requiredActiveVisitorId;

	[DbfField("REQUIRED_ACTIVE_TASK")]
	public int RequiredActiveTask => m_requiredActiveTaskId;

	[DbfField("ONLY_SHOW_ONCE")]
	public bool OnlyShowOnce => m_onlyShowOnce;

	[DbfField("SHOW_CHANCE")]
	public int ShowChance => m_showChance;

	[DbfField("UI_EVENT")]
	public string UiEvent => m_uiEvent;

	[DbfField("POPUP")]
	public string Popup => m_popup;

	[DbfField("TUTORIAL_DIALOG")]
	public int TutorialDialog => m_tutorialDialogId;

	public override object GetVar(string name)
	{
		return name switch
		{
			"ID" => base.ID, 
			"TUTORIAL_EVENT" => m_tutorialEvent, 
			"TRIGGER_EVENT_ON_COMPLETE" => m_triggerEventOnComplete, 
			"NODE_TYPE_ID" => m_nodeTypeId, 
			"REQUIRED_ACTIVE_BOUNTY" => m_requiredActiveBountyId, 
			"REQUIRED_ACTIVE_VISITOR" => m_requiredActiveVisitorId, 
			"REQUIRED_ACTIVE_TASK" => m_requiredActiveTaskId, 
			"ONLY_SHOW_ONCE" => m_onlyShowOnce, 
			"SHOW_CHANCE" => m_showChance, 
			"UI_EVENT" => m_uiEvent, 
			"POPUP" => m_popup, 
			"TUTORIAL_DIALOG" => m_tutorialDialogId, 
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
		case "TUTORIAL_EVENT":
			if (val == null)
			{
				m_tutorialEvent = LettuceTutorialVo.LettuceTutorialEvent.INVALID;
			}
			else if (val is LettuceTutorialVo.LettuceTutorialEvent || val is int)
			{
				m_tutorialEvent = (LettuceTutorialVo.LettuceTutorialEvent)val;
			}
			else if (val is string)
			{
				m_tutorialEvent = LettuceTutorialVo.ParseLettuceTutorialEventValue((string)val);
			}
			break;
		case "TRIGGER_EVENT_ON_COMPLETE":
			if (val == null)
			{
				m_triggerEventOnComplete = LettuceTutorialVo.LettuceTutorialEvent.INVALID;
			}
			else if (val is LettuceTutorialVo.LettuceTutorialEvent || val is int)
			{
				m_triggerEventOnComplete = (LettuceTutorialVo.LettuceTutorialEvent)val;
			}
			else if (val is string)
			{
				m_triggerEventOnComplete = LettuceTutorialVo.ParseLettuceTutorialEventValue((string)val);
			}
			break;
		case "NODE_TYPE_ID":
			m_nodeTypeId = (int)val;
			break;
		case "REQUIRED_ACTIVE_BOUNTY":
			m_requiredActiveBountyId = (int)val;
			break;
		case "REQUIRED_ACTIVE_VISITOR":
			m_requiredActiveVisitorId = (int)val;
			break;
		case "REQUIRED_ACTIVE_TASK":
			m_requiredActiveTaskId = (int)val;
			break;
		case "ONLY_SHOW_ONCE":
			m_onlyShowOnce = (bool)val;
			break;
		case "SHOW_CHANCE":
			m_showChance = (int)val;
			break;
		case "UI_EVENT":
			m_uiEvent = (string)val;
			break;
		case "POPUP":
			m_popup = (string)val;
			break;
		case "TUTORIAL_DIALOG":
			m_tutorialDialogId = (int)val;
			break;
		}
	}

	public override Type GetVarType(string name)
	{
		return name switch
		{
			"ID" => typeof(int), 
			"TUTORIAL_EVENT" => typeof(LettuceTutorialVo.LettuceTutorialEvent), 
			"TRIGGER_EVENT_ON_COMPLETE" => typeof(LettuceTutorialVo.LettuceTutorialEvent), 
			"NODE_TYPE_ID" => typeof(int), 
			"REQUIRED_ACTIVE_BOUNTY" => typeof(int), 
			"REQUIRED_ACTIVE_VISITOR" => typeof(int), 
			"REQUIRED_ACTIVE_TASK" => typeof(int), 
			"ONLY_SHOW_ONCE" => typeof(bool), 
			"SHOW_CHANCE" => typeof(int), 
			"UI_EVENT" => typeof(string), 
			"POPUP" => typeof(string), 
			"TUTORIAL_DIALOG" => typeof(int), 
			_ => null, 
		};
	}

	public override IEnumerator<IAsyncJobResult> Job_LoadRecordsFromAssetAsync<T>(string resourcePath, Action<List<T>> resultHandler)
	{
		LoadLettuceTutorialVoDbfRecords loadRecords = new LoadLettuceTutorialVoDbfRecords(resourcePath);
		yield return loadRecords;
		resultHandler?.Invoke(loadRecords.GetRecords() as List<T>);
	}

	public override bool LoadRecordsFromAsset<T>(string resourcePath, out List<T> records)
	{
		LettuceTutorialVoDbfAsset dbfAsset = DbfShared.GetAssetBundle().LoadAsset(resourcePath, typeof(LettuceTutorialVoDbfAsset)) as LettuceTutorialVoDbfAsset;
		if (dbfAsset == null)
		{
			records = new List<T>();
			Debug.LogError($"LettuceTutorialVoDbfAsset.LoadRecordsFromAsset() - failed to load records from assetbundle: {resourcePath}");
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
