using System;
using System.Collections.Generic;
using Assets;
using Blizzard.T5.Jobs;
using UnityEngine;

[Serializable]
public class QuestDbfRecord : DbfRecord
{
	[SerializeField]
	private DbfLocValue m_name;

	[SerializeField]
	private DbfLocValue m_description;

	[SerializeField]
	private DbfLocValue m_toastDescription;

	[SerializeField]
	private string m_icon;

	[SerializeField]
	private int m_quota;

	[SerializeField]
	private EventTimingType m_event = EventTimingType.UNKNOWN;

	[SerializeField]
	private int m_nextInChainId;

	[SerializeField]
	private bool m_canAbandon;

	[SerializeField]
	private string m_deepLink;

	[SerializeField]
	private int m_questPoolId;

	[SerializeField]
	private bool m_poolGuaranteed;

	[SerializeField]
	private int m_poolInstantGrantDay;

	[SerializeField]
	private int m_rewardTrackXp;

	[SerializeField]
	private int m_rewardListId;

	[SerializeField]
	private Quest.RewardTrackType m_rewardTrackType;

	[SerializeField]
	private int m_proxyForLegacyId;

	[DbfField("NAME")]
	public DbfLocValue Name => m_name;

	[DbfField("DESCRIPTION")]
	public DbfLocValue Description => m_description;

	[DbfField("TOAST_DESCRIPTION")]
	public DbfLocValue ToastDescription => m_toastDescription;

	[DbfField("ICON")]
	public string Icon => m_icon;

	[DbfField("QUOTA")]
	public int Quota => m_quota;

	[DbfField("EVENT")]
	public EventTimingType Event => m_event;

	[DbfField("NEXT_IN_CHAIN")]
	public int NextInChain => m_nextInChainId;

	[DbfField("CAN_ABANDON")]
	public bool CanAbandon => m_canAbandon;

	[DbfField("DEEP_LINK")]
	public string DeepLink => m_deepLink;

	[DbfField("QUEST_POOL")]
	public int QuestPool => m_questPoolId;

	public QuestPoolDbfRecord QuestPoolRecord => GameDbf.QuestPool.GetRecord(m_questPoolId);

	[DbfField("REWARD_TRACK_XP")]
	public int RewardTrackXp => m_rewardTrackXp;

	[DbfField("REWARD_LIST")]
	public int RewardList => m_rewardListId;

	[DbfField("REWARD_TRACK_TYPE")]
	public Quest.RewardTrackType RewardTrackType => m_rewardTrackType;

	[DbfField("PROXY_FOR_LEGACY_ID")]
	public int ProxyForLegacyId => m_proxyForLegacyId;

	public override object GetVar(string name)
	{
		return name switch
		{
			"ID" => base.ID, 
			"NAME" => m_name, 
			"DESCRIPTION" => m_description, 
			"TOAST_DESCRIPTION" => m_toastDescription, 
			"ICON" => m_icon, 
			"QUOTA" => m_quota, 
			"EVENT" => m_event, 
			"NEXT_IN_CHAIN" => m_nextInChainId, 
			"CAN_ABANDON" => m_canAbandon, 
			"DEEP_LINK" => m_deepLink, 
			"QUEST_POOL" => m_questPoolId, 
			"POOL_GUARANTEED" => m_poolGuaranteed, 
			"POOL_INSTANT_GRANT_DAY" => m_poolInstantGrantDay, 
			"REWARD_TRACK_XP" => m_rewardTrackXp, 
			"REWARD_LIST" => m_rewardListId, 
			"REWARD_TRACK_TYPE" => m_rewardTrackType, 
			"PROXY_FOR_LEGACY_ID" => m_proxyForLegacyId, 
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
		case "NAME":
			m_name = (DbfLocValue)val;
			break;
		case "DESCRIPTION":
			m_description = (DbfLocValue)val;
			break;
		case "TOAST_DESCRIPTION":
			m_toastDescription = (DbfLocValue)val;
			break;
		case "ICON":
			m_icon = (string)val;
			break;
		case "QUOTA":
			m_quota = (int)val;
			break;
		case "EVENT":
			m_event = DbfShared.GetEventMap().ConvertStringToSpecialEvent((string)val);
			break;
		case "NEXT_IN_CHAIN":
			m_nextInChainId = (int)val;
			break;
		case "CAN_ABANDON":
			m_canAbandon = (bool)val;
			break;
		case "DEEP_LINK":
			m_deepLink = (string)val;
			break;
		case "QUEST_POOL":
			m_questPoolId = (int)val;
			break;
		case "POOL_GUARANTEED":
			m_poolGuaranteed = (bool)val;
			break;
		case "POOL_INSTANT_GRANT_DAY":
			m_poolInstantGrantDay = (int)val;
			break;
		case "REWARD_TRACK_XP":
			m_rewardTrackXp = (int)val;
			break;
		case "REWARD_LIST":
			m_rewardListId = (int)val;
			break;
		case "REWARD_TRACK_TYPE":
			if (val == null)
			{
				m_rewardTrackType = Quest.RewardTrackType.NONE;
			}
			else if (val is Quest.RewardTrackType || val is int)
			{
				m_rewardTrackType = (Quest.RewardTrackType)val;
			}
			else if (val is string)
			{
				m_rewardTrackType = Quest.ParseRewardTrackTypeValue((string)val);
			}
			break;
		case "PROXY_FOR_LEGACY_ID":
			m_proxyForLegacyId = (int)val;
			break;
		}
	}

	public override Type GetVarType(string name)
	{
		return name switch
		{
			"ID" => typeof(int), 
			"NAME" => typeof(DbfLocValue), 
			"DESCRIPTION" => typeof(DbfLocValue), 
			"TOAST_DESCRIPTION" => typeof(DbfLocValue), 
			"ICON" => typeof(string), 
			"QUOTA" => typeof(int), 
			"EVENT" => typeof(string), 
			"NEXT_IN_CHAIN" => typeof(int), 
			"CAN_ABANDON" => typeof(bool), 
			"DEEP_LINK" => typeof(string), 
			"QUEST_POOL" => typeof(int), 
			"POOL_GUARANTEED" => typeof(bool), 
			"POOL_INSTANT_GRANT_DAY" => typeof(int), 
			"REWARD_TRACK_XP" => typeof(int), 
			"REWARD_LIST" => typeof(int), 
			"REWARD_TRACK_TYPE" => typeof(Quest.RewardTrackType), 
			"PROXY_FOR_LEGACY_ID" => typeof(int), 
			_ => null, 
		};
	}

	public override IEnumerator<IAsyncJobResult> Job_LoadRecordsFromAssetAsync<T>(string resourcePath, Action<List<T>> resultHandler)
	{
		LoadQuestDbfRecords loadRecords = new LoadQuestDbfRecords(resourcePath);
		yield return loadRecords;
		resultHandler?.Invoke(loadRecords.GetRecords() as List<T>);
	}

	public override bool LoadRecordsFromAsset<T>(string resourcePath, out List<T> records)
	{
		QuestDbfAsset dbfAsset = DbfShared.GetAssetBundle().LoadAsset(resourcePath, typeof(QuestDbfAsset)) as QuestDbfAsset;
		if (dbfAsset == null)
		{
			records = new List<T>();
			Debug.LogError($"QuestDbfAsset.LoadRecordsFromAsset() - failed to load records from assetbundle: {resourcePath}");
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
		m_name.StripUnusedLocales();
		m_description.StripUnusedLocales();
		m_toastDescription.StripUnusedLocales();
	}
}
