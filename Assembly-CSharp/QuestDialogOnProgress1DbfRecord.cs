using System;
using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

[Serializable]
public class QuestDialogOnProgress1DbfRecord : DbfRecord
{
	[SerializeField]
	private int m_questDialogId;

	[SerializeField]
	private int m_playOrder;

	[SerializeField]
	private string m_prefabName;

	[SerializeField]
	private string m_audioName;

	[SerializeField]
	private bool m_altBubblePosition;

	[SerializeField]
	private double m_waitBefore;

	[SerializeField]
	private double m_waitAfter;

	[SerializeField]
	private bool m_persistPrefab;

	public override object GetVar(string name)
	{
		return name switch
		{
			"ID" => base.ID, 
			"QUEST_DIALOG_ID" => m_questDialogId, 
			"PLAY_ORDER" => m_playOrder, 
			"PREFAB_NAME" => m_prefabName, 
			"AUDIO_NAME" => m_audioName, 
			"ALT_BUBBLE_POSITION" => m_altBubblePosition, 
			"WAIT_BEFORE" => m_waitBefore, 
			"WAIT_AFTER" => m_waitAfter, 
			"PERSIST_PREFAB" => m_persistPrefab, 
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
		case "QUEST_DIALOG_ID":
			m_questDialogId = (int)val;
			break;
		case "PLAY_ORDER":
			m_playOrder = (int)val;
			break;
		case "PREFAB_NAME":
			m_prefabName = (string)val;
			break;
		case "AUDIO_NAME":
			m_audioName = (string)val;
			break;
		case "ALT_BUBBLE_POSITION":
			m_altBubblePosition = (bool)val;
			break;
		case "WAIT_BEFORE":
			m_waitBefore = (double)val;
			break;
		case "WAIT_AFTER":
			m_waitAfter = (double)val;
			break;
		case "PERSIST_PREFAB":
			m_persistPrefab = (bool)val;
			break;
		}
	}

	public override Type GetVarType(string name)
	{
		return name switch
		{
			"ID" => typeof(int), 
			"QUEST_DIALOG_ID" => typeof(int), 
			"PLAY_ORDER" => typeof(int), 
			"PREFAB_NAME" => typeof(string), 
			"AUDIO_NAME" => typeof(string), 
			"ALT_BUBBLE_POSITION" => typeof(bool), 
			"WAIT_BEFORE" => typeof(double), 
			"WAIT_AFTER" => typeof(double), 
			"PERSIST_PREFAB" => typeof(bool), 
			_ => null, 
		};
	}

	public override IEnumerator<IAsyncJobResult> Job_LoadRecordsFromAssetAsync<T>(string resourcePath, Action<List<T>> resultHandler)
	{
		LoadQuestDialogOnProgress1DbfRecords loadRecords = new LoadQuestDialogOnProgress1DbfRecords(resourcePath);
		yield return loadRecords;
		resultHandler?.Invoke(loadRecords.GetRecords() as List<T>);
	}

	public override bool LoadRecordsFromAsset<T>(string resourcePath, out List<T> records)
	{
		QuestDialogOnProgress1DbfAsset dbfAsset = DbfShared.GetAssetBundle().LoadAsset(resourcePath, typeof(QuestDialogOnProgress1DbfAsset)) as QuestDialogOnProgress1DbfAsset;
		if (dbfAsset == null)
		{
			records = new List<T>();
			Debug.LogError($"QuestDialogOnProgress1DbfAsset.LoadRecordsFromAsset() - failed to load records from assetbundle: {resourcePath}");
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
