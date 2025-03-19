using System;
using System.Collections.Generic;
using Assets;
using Blizzard.T5.Jobs;
using UnityEngine;

[Serializable]
public class LuckyDrawBoxDbfRecord : DbfRecord
{
	[SerializeField]
	private DbfLocValue m_name;

	[SerializeField]
	private EventTimingType m_event = EventTimingType.UNKNOWN;

	[SerializeField]
	private string m_theme;

	[SerializeField]
	private string m_layout;

	[SerializeField]
	private int m_accountLicenseId;

	[SerializeField]
	private int m_freeCount = 1;

	[SerializeField]
	private int m_bonusCount = 2;

	[SerializeField]
	private int m_earnCount = 1;

	[SerializeField]
	private LuckyDrawBox.LuckyDrawEarnHammerCondition m_earnCondition;

	[DbfField("NAME")]
	public DbfLocValue Name => m_name;

	[DbfField("EVENT")]
	public EventTimingType Event => m_event;

	[DbfField("THEME")]
	public string Theme => m_theme;

	[DbfField("LAYOUT")]
	public string Layout => m_layout;

	[DbfField("ACCOUNT_LICENSE_ID")]
	public int AccountLicenseId => m_accountLicenseId;

	public AccountLicenseDbfRecord AccountLicenseRecord => GameDbf.AccountLicense.GetRecord(m_accountLicenseId);

	public override object GetVar(string name)
	{
		return name switch
		{
			"ID" => base.ID, 
			"NAME" => m_name, 
			"EVENT" => m_event, 
			"THEME" => m_theme, 
			"LAYOUT" => m_layout, 
			"ACCOUNT_LICENSE_ID" => m_accountLicenseId, 
			"FREE_COUNT" => m_freeCount, 
			"BONUS_COUNT" => m_bonusCount, 
			"EARN_COUNT" => m_earnCount, 
			"EARN_CONDITION" => m_earnCondition, 
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
		case "EVENT":
			m_event = DbfShared.GetEventMap().ConvertStringToSpecialEvent((string)val);
			break;
		case "THEME":
			m_theme = (string)val;
			break;
		case "LAYOUT":
			m_layout = (string)val;
			break;
		case "ACCOUNT_LICENSE_ID":
			m_accountLicenseId = (int)val;
			break;
		case "FREE_COUNT":
			m_freeCount = (int)val;
			break;
		case "BONUS_COUNT":
			m_bonusCount = (int)val;
			break;
		case "EARN_COUNT":
			m_earnCount = (int)val;
			break;
		case "EARN_CONDITION":
			if (val == null)
			{
				m_earnCondition = LuckyDrawBox.LuckyDrawEarnHammerCondition.BATTLEGROUNDS_WIN;
			}
			else if (val is LuckyDrawBox.LuckyDrawEarnHammerCondition || val is int)
			{
				m_earnCondition = (LuckyDrawBox.LuckyDrawEarnHammerCondition)val;
			}
			else if (val is string)
			{
				m_earnCondition = LuckyDrawBox.ParseLuckyDrawEarnHammerConditionValue((string)val);
			}
			break;
		}
	}

	public override Type GetVarType(string name)
	{
		return name switch
		{
			"ID" => typeof(int), 
			"NAME" => typeof(DbfLocValue), 
			"EVENT" => typeof(string), 
			"THEME" => typeof(string), 
			"LAYOUT" => typeof(string), 
			"ACCOUNT_LICENSE_ID" => typeof(int), 
			"FREE_COUNT" => typeof(int), 
			"BONUS_COUNT" => typeof(int), 
			"EARN_COUNT" => typeof(int), 
			"EARN_CONDITION" => typeof(LuckyDrawBox.LuckyDrawEarnHammerCondition), 
			_ => null, 
		};
	}

	public override IEnumerator<IAsyncJobResult> Job_LoadRecordsFromAssetAsync<T>(string resourcePath, Action<List<T>> resultHandler)
	{
		LoadLuckyDrawBoxDbfRecords loadRecords = new LoadLuckyDrawBoxDbfRecords(resourcePath);
		yield return loadRecords;
		resultHandler?.Invoke(loadRecords.GetRecords() as List<T>);
	}

	public override bool LoadRecordsFromAsset<T>(string resourcePath, out List<T> records)
	{
		LuckyDrawBoxDbfAsset dbfAsset = DbfShared.GetAssetBundle().LoadAsset(resourcePath, typeof(LuckyDrawBoxDbfAsset)) as LuckyDrawBoxDbfAsset;
		if (dbfAsset == null)
		{
			records = new List<T>();
			Debug.LogError($"LuckyDrawBoxDbfAsset.LoadRecordsFromAsset() - failed to load records from assetbundle: {resourcePath}");
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
	}
}
