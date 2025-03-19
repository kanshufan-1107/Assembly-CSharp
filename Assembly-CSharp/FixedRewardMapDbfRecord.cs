using System;
using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

[Serializable]
public class FixedRewardMapDbfRecord : DbfRecord
{
	[SerializeField]
	private int m_actionId;

	[SerializeField]
	private int m_rewardId;

	[SerializeField]
	private int m_rewardCount = 1;

	[SerializeField]
	private string m_noteDesc;

	[SerializeField]
	private bool m_useQuestToast;

	[SerializeField]
	private bool m_suppressRewardToast;

	[SerializeField]
	private string m_rewardTiming = "immediate";

	[SerializeField]
	private DbfLocValue m_toastName;

	[SerializeField]
	private DbfLocValue m_toastDescription;

	[SerializeField]
	private int m_sortOrder;

	[DbfField("ACTION_ID")]
	public int ActionId => m_actionId;

	public FixedRewardActionDbfRecord ActionRecord => GameDbf.FixedRewardAction.GetRecord(m_actionId);

	[DbfField("REWARD_ID")]
	public int RewardId => m_rewardId;

	public FixedRewardDbfRecord RewardRecord => GameDbf.FixedReward.GetRecord(m_rewardId);

	[DbfField("REWARD_COUNT")]
	public int RewardCount => m_rewardCount;

	[DbfField("NOTE_DESC")]
	public string NoteDesc => m_noteDesc;

	[DbfField("USE_QUEST_TOAST")]
	public bool UseQuestToast => m_useQuestToast;

	[DbfField("SUPPRESS_REWARD_TOAST")]
	public bool SuppressRewardToast => m_suppressRewardToast;

	[DbfField("REWARD_TIMING")]
	public string RewardTiming => m_rewardTiming;

	[DbfField("TOAST_NAME")]
	public DbfLocValue ToastName => m_toastName;

	[DbfField("TOAST_DESCRIPTION")]
	public DbfLocValue ToastDescription => m_toastDescription;

	[DbfField("SORT_ORDER")]
	public int SortOrder => m_sortOrder;

	public override object GetVar(string name)
	{
		return name switch
		{
			"ID" => base.ID, 
			"ACTION_ID" => m_actionId, 
			"REWARD_ID" => m_rewardId, 
			"REWARD_COUNT" => m_rewardCount, 
			"NOTE_DESC" => m_noteDesc, 
			"USE_QUEST_TOAST" => m_useQuestToast, 
			"SUPPRESS_REWARD_TOAST" => m_suppressRewardToast, 
			"REWARD_TIMING" => m_rewardTiming, 
			"TOAST_NAME" => m_toastName, 
			"TOAST_DESCRIPTION" => m_toastDescription, 
			"SORT_ORDER" => m_sortOrder, 
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
		case "ACTION_ID":
			m_actionId = (int)val;
			break;
		case "REWARD_ID":
			m_rewardId = (int)val;
			break;
		case "REWARD_COUNT":
			m_rewardCount = (int)val;
			break;
		case "NOTE_DESC":
			m_noteDesc = (string)val;
			break;
		case "USE_QUEST_TOAST":
			m_useQuestToast = (bool)val;
			break;
		case "SUPPRESS_REWARD_TOAST":
			m_suppressRewardToast = (bool)val;
			break;
		case "REWARD_TIMING":
			m_rewardTiming = (string)val;
			break;
		case "TOAST_NAME":
			m_toastName = (DbfLocValue)val;
			break;
		case "TOAST_DESCRIPTION":
			m_toastDescription = (DbfLocValue)val;
			break;
		case "SORT_ORDER":
			m_sortOrder = (int)val;
			break;
		}
	}

	public override Type GetVarType(string name)
	{
		return name switch
		{
			"ID" => typeof(int), 
			"ACTION_ID" => typeof(int), 
			"REWARD_ID" => typeof(int), 
			"REWARD_COUNT" => typeof(int), 
			"NOTE_DESC" => typeof(string), 
			"USE_QUEST_TOAST" => typeof(bool), 
			"SUPPRESS_REWARD_TOAST" => typeof(bool), 
			"REWARD_TIMING" => typeof(string), 
			"TOAST_NAME" => typeof(DbfLocValue), 
			"TOAST_DESCRIPTION" => typeof(DbfLocValue), 
			"SORT_ORDER" => typeof(int), 
			_ => null, 
		};
	}

	public override IEnumerator<IAsyncJobResult> Job_LoadRecordsFromAssetAsync<T>(string resourcePath, Action<List<T>> resultHandler)
	{
		LoadFixedRewardMapDbfRecords loadRecords = new LoadFixedRewardMapDbfRecords(resourcePath);
		yield return loadRecords;
		resultHandler?.Invoke(loadRecords.GetRecords() as List<T>);
	}

	public override bool LoadRecordsFromAsset<T>(string resourcePath, out List<T> records)
	{
		FixedRewardMapDbfAsset dbfAsset = DbfShared.GetAssetBundle().LoadAsset(resourcePath, typeof(FixedRewardMapDbfAsset)) as FixedRewardMapDbfAsset;
		if (dbfAsset == null)
		{
			records = new List<T>();
			Debug.LogError($"FixedRewardMapDbfAsset.LoadRecordsFromAsset() - failed to load records from assetbundle: {resourcePath}");
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
		m_toastName.StripUnusedLocales();
		m_toastDescription.StripUnusedLocales();
	}
}
