using System;
using System.Collections.Generic;
using Assets;
using Blizzard.T5.Jobs;
using UnityEngine;

[Serializable]
public class QuestPoolDbfRecord : DbfRecord
{
	[SerializeField]
	private int m_grantDayOfWeek = -1;

	[SerializeField]
	private int m_grantHourOfDay;

	[SerializeField]
	private int m_numQuestsGranted = 1;

	[SerializeField]
	private int m_maxQuestsActive = 1;

	[SerializeField]
	private int m_maxQuestsBanked;

	[SerializeField]
	private int m_fallbackQuestPoolId;

	[SerializeField]
	private int m_rerollCountMax;

	[SerializeField]
	private QuestPool.QuestPoolType m_questPoolType = QuestPool.QuestPoolType.DAILY;

	[SerializeField]
	private QuestPool.RewardTrackType m_rewardTrackType;

	[DbfField("NUM_QUESTS_GRANTED")]
	public int NumQuestsGranted => m_numQuestsGranted;

	[DbfField("MAX_QUESTS_ACTIVE")]
	public int MaxQuestsActive => m_maxQuestsActive;

	[DbfField("FALLBACK_QUEST_POOL")]
	public int FallbackQuestPool => m_fallbackQuestPoolId;

	public QuestPoolDbfRecord FallbackQuestPoolRecord => GameDbf.QuestPool.GetRecord(m_fallbackQuestPoolId);

	[DbfField("QUEST_POOL_TYPE")]
	public QuestPool.QuestPoolType QuestPoolType => m_questPoolType;

	[DbfField("REWARD_TRACK_TYPE")]
	public QuestPool.RewardTrackType RewardTrackType => m_rewardTrackType;

	public override object GetVar(string name)
	{
		return name switch
		{
			"ID" => base.ID, 
			"GRANT_DAY_OF_WEEK" => m_grantDayOfWeek, 
			"GRANT_HOUR_OF_DAY" => m_grantHourOfDay, 
			"NUM_QUESTS_GRANTED" => m_numQuestsGranted, 
			"MAX_QUESTS_ACTIVE" => m_maxQuestsActive, 
			"MAX_QUESTS_BANKED" => m_maxQuestsBanked, 
			"FALLBACK_QUEST_POOL" => m_fallbackQuestPoolId, 
			"REROLL_COUNT_MAX" => m_rerollCountMax, 
			"QUEST_POOL_TYPE" => m_questPoolType, 
			"REWARD_TRACK_TYPE" => m_rewardTrackType, 
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
		case "GRANT_DAY_OF_WEEK":
			m_grantDayOfWeek = (int)val;
			break;
		case "GRANT_HOUR_OF_DAY":
			m_grantHourOfDay = (int)val;
			break;
		case "NUM_QUESTS_GRANTED":
			m_numQuestsGranted = (int)val;
			break;
		case "MAX_QUESTS_ACTIVE":
			m_maxQuestsActive = (int)val;
			break;
		case "MAX_QUESTS_BANKED":
			m_maxQuestsBanked = (int)val;
			break;
		case "FALLBACK_QUEST_POOL":
			m_fallbackQuestPoolId = (int)val;
			break;
		case "REROLL_COUNT_MAX":
			m_rerollCountMax = (int)val;
			break;
		case "QUEST_POOL_TYPE":
			if (val == null)
			{
				m_questPoolType = QuestPool.QuestPoolType.NONE;
			}
			else if (val is QuestPool.QuestPoolType || val is int)
			{
				m_questPoolType = (QuestPool.QuestPoolType)val;
			}
			else if (val is string)
			{
				m_questPoolType = QuestPool.ParseQuestPoolTypeValue((string)val);
			}
			break;
		case "REWARD_TRACK_TYPE":
			if (val == null)
			{
				m_rewardTrackType = QuestPool.RewardTrackType.NONE;
			}
			else if (val is QuestPool.RewardTrackType || val is int)
			{
				m_rewardTrackType = (QuestPool.RewardTrackType)val;
			}
			else if (val is string)
			{
				m_rewardTrackType = QuestPool.ParseRewardTrackTypeValue((string)val);
			}
			break;
		}
	}

	public override Type GetVarType(string name)
	{
		return name switch
		{
			"ID" => typeof(int), 
			"GRANT_DAY_OF_WEEK" => typeof(int), 
			"GRANT_HOUR_OF_DAY" => typeof(int), 
			"NUM_QUESTS_GRANTED" => typeof(int), 
			"MAX_QUESTS_ACTIVE" => typeof(int), 
			"MAX_QUESTS_BANKED" => typeof(int), 
			"FALLBACK_QUEST_POOL" => typeof(int), 
			"REROLL_COUNT_MAX" => typeof(int), 
			"QUEST_POOL_TYPE" => typeof(QuestPool.QuestPoolType), 
			"REWARD_TRACK_TYPE" => typeof(QuestPool.RewardTrackType), 
			_ => null, 
		};
	}

	public override IEnumerator<IAsyncJobResult> Job_LoadRecordsFromAssetAsync<T>(string resourcePath, Action<List<T>> resultHandler)
	{
		LoadQuestPoolDbfRecords loadRecords = new LoadQuestPoolDbfRecords(resourcePath);
		yield return loadRecords;
		resultHandler?.Invoke(loadRecords.GetRecords() as List<T>);
	}

	public override bool LoadRecordsFromAsset<T>(string resourcePath, out List<T> records)
	{
		QuestPoolDbfAsset dbfAsset = DbfShared.GetAssetBundle().LoadAsset(resourcePath, typeof(QuestPoolDbfAsset)) as QuestPoolDbfAsset;
		if (dbfAsset == null)
		{
			records = new List<T>();
			Debug.LogError($"QuestPoolDbfAsset.LoadRecordsFromAsset() - failed to load records from assetbundle: {resourcePath}");
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
