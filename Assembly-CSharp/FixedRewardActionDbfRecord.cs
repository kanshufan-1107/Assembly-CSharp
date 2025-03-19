using System;
using System.Collections.Generic;
using Assets;
using Blizzard.T5.Jobs;
using UnityEngine;

[Serializable]
public class FixedRewardActionDbfRecord : DbfRecord
{
	[SerializeField]
	private string m_noteDesc;

	[SerializeField]
	private FixedRewardAction.Type m_type = FixedRewardAction.ParseTypeValue("wing_progress");

	[SerializeField]
	private int m_wingId;

	[SerializeField]
	private int m_wingProgress;

	[SerializeField]
	private ulong m_wingFlags;

	[SerializeField]
	private int m_classId;

	[SerializeField]
	private int m_totalHeroLevel;

	[SerializeField]
	private int m_heroLevel;

	[SerializeField]
	private ulong m_metaActionFlags;

	[SerializeField]
	private int m_achieveId;

	[SerializeField]
	private long m_accountLicenseId;

	[SerializeField]
	private ulong m_accountLicenseFlags;

	[SerializeField]
	private EventTimingType m_activeEvent = DbfShared.GetEventMap().ConvertStringToSpecialEvent("always");

	[SerializeField]
	private int m_cardId;

	[DbfField("TYPE")]
	public FixedRewardAction.Type Type => m_type;

	[DbfField("WING_ID")]
	public int WingId => m_wingId;

	[DbfField("WING_PROGRESS")]
	public int WingProgress => m_wingProgress;

	[DbfField("WING_FLAGS")]
	public ulong WingFlags => m_wingFlags;

	[DbfField("CLASS_ID")]
	public int ClassId => m_classId;

	[DbfField("TOTAL_HERO_LEVEL")]
	public int TotalHeroLevel => m_totalHeroLevel;

	[DbfField("HERO_LEVEL")]
	public int HeroLevel => m_heroLevel;

	[DbfField("META_ACTION_FLAGS")]
	public ulong MetaActionFlags => m_metaActionFlags;

	[DbfField("ACHIEVE_ID")]
	public int AchieveId => m_achieveId;

	[DbfField("ACCOUNT_LICENSE_ID")]
	public long AccountLicenseId => m_accountLicenseId;

	[DbfField("ACCOUNT_LICENSE_FLAGS")]
	public ulong AccountLicenseFlags => m_accountLicenseFlags;

	[DbfField("ACTIVE_EVENT")]
	public EventTimingType ActiveEvent => m_activeEvent;

	public override object GetVar(string name)
	{
		return name switch
		{
			"ID" => base.ID, 
			"NOTE_DESC" => m_noteDesc, 
			"TYPE" => m_type, 
			"WING_ID" => m_wingId, 
			"WING_PROGRESS" => m_wingProgress, 
			"WING_FLAGS" => m_wingFlags, 
			"CLASS_ID" => m_classId, 
			"TOTAL_HERO_LEVEL" => m_totalHeroLevel, 
			"HERO_LEVEL" => m_heroLevel, 
			"META_ACTION_FLAGS" => m_metaActionFlags, 
			"ACHIEVE_ID" => m_achieveId, 
			"ACCOUNT_LICENSE_ID" => m_accountLicenseId, 
			"ACCOUNT_LICENSE_FLAGS" => m_accountLicenseFlags, 
			"ACTIVE_EVENT" => m_activeEvent, 
			"CARD_ID" => m_cardId, 
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
		case "NOTE_DESC":
			m_noteDesc = (string)val;
			break;
		case "TYPE":
			if (val == null)
			{
				m_type = FixedRewardAction.Type.WING_PROGRESS;
			}
			else if (val is FixedRewardAction.Type || val is int)
			{
				m_type = (FixedRewardAction.Type)val;
			}
			else if (val is string)
			{
				m_type = FixedRewardAction.ParseTypeValue((string)val);
			}
			break;
		case "WING_ID":
			m_wingId = (int)val;
			break;
		case "WING_PROGRESS":
			m_wingProgress = (int)val;
			break;
		case "WING_FLAGS":
			m_wingFlags = (ulong)val;
			break;
		case "CLASS_ID":
			m_classId = (int)val;
			break;
		case "TOTAL_HERO_LEVEL":
			m_totalHeroLevel = (int)val;
			break;
		case "HERO_LEVEL":
			m_heroLevel = (int)val;
			break;
		case "META_ACTION_FLAGS":
			m_metaActionFlags = (ulong)val;
			break;
		case "ACHIEVE_ID":
			m_achieveId = (int)val;
			break;
		case "ACCOUNT_LICENSE_ID":
			m_accountLicenseId = (long)val;
			break;
		case "ACCOUNT_LICENSE_FLAGS":
			m_accountLicenseFlags = (ulong)val;
			break;
		case "ACTIVE_EVENT":
			m_activeEvent = DbfShared.GetEventMap().ConvertStringToSpecialEvent((string)val);
			break;
		case "CARD_ID":
			m_cardId = (int)val;
			break;
		}
	}

	public override Type GetVarType(string name)
	{
		return name switch
		{
			"ID" => typeof(int), 
			"NOTE_DESC" => typeof(string), 
			"TYPE" => typeof(FixedRewardAction.Type), 
			"WING_ID" => typeof(int), 
			"WING_PROGRESS" => typeof(int), 
			"WING_FLAGS" => typeof(ulong), 
			"CLASS_ID" => typeof(int), 
			"TOTAL_HERO_LEVEL" => typeof(int), 
			"HERO_LEVEL" => typeof(int), 
			"META_ACTION_FLAGS" => typeof(ulong), 
			"ACHIEVE_ID" => typeof(int), 
			"ACCOUNT_LICENSE_ID" => typeof(long), 
			"ACCOUNT_LICENSE_FLAGS" => typeof(ulong), 
			"ACTIVE_EVENT" => typeof(string), 
			"CARD_ID" => typeof(int), 
			_ => null, 
		};
	}

	public override IEnumerator<IAsyncJobResult> Job_LoadRecordsFromAssetAsync<T>(string resourcePath, Action<List<T>> resultHandler)
	{
		LoadFixedRewardActionDbfRecords loadRecords = new LoadFixedRewardActionDbfRecords(resourcePath);
		yield return loadRecords;
		resultHandler?.Invoke(loadRecords.GetRecords() as List<T>);
	}

	public override bool LoadRecordsFromAsset<T>(string resourcePath, out List<T> records)
	{
		FixedRewardActionDbfAsset dbfAsset = DbfShared.GetAssetBundle().LoadAsset(resourcePath, typeof(FixedRewardActionDbfAsset)) as FixedRewardActionDbfAsset;
		if (dbfAsset == null)
		{
			records = new List<T>();
			Debug.LogError($"FixedRewardActionDbfAsset.LoadRecordsFromAsset() - failed to load records from assetbundle: {resourcePath}");
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
