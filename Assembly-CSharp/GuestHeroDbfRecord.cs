using System;
using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

[Serializable]
public class GuestHeroDbfRecord : DbfRecord
{
	[SerializeField]
	private int m_cardId;

	[SerializeField]
	private DbfLocValue m_name;

	[SerializeField]
	private DbfLocValue m_shortName;

	[SerializeField]
	private DbfLocValue m_flavorText;

	[SerializeField]
	private EventTimingType m_unlockEvent = DbfShared.GetEventMap().ConvertStringToSpecialEvent("none");

	[DbfField("CARD_ID")]
	public int CardId => m_cardId;

	public CardDbfRecord CardRecord => GameDbf.Card.GetRecord(m_cardId);

	[DbfField("NAME")]
	public DbfLocValue Name => m_name;

	[DbfField("SHORT_NAME")]
	public DbfLocValue ShortName => m_shortName;

	[DbfField("FLAVOR_TEXT")]
	public DbfLocValue FlavorText => m_flavorText;

	[DbfField("UNLOCK_EVENT")]
	public EventTimingType UnlockEvent => m_unlockEvent;

	public void SetCardId(int v)
	{
		m_cardId = v;
	}

	public void SetName(DbfLocValue v)
	{
		m_name = v;
		v.SetDebugInfo(base.ID, "NAME");
	}

	public void SetShortName(DbfLocValue v)
	{
		m_shortName = v;
		v.SetDebugInfo(base.ID, "SHORT_NAME");
	}

	public void SetFlavorText(DbfLocValue v)
	{
		m_flavorText = v;
		v.SetDebugInfo(base.ID, "FLAVOR_TEXT");
	}

	public void SetUnlockEvent(EventTimingType v)
	{
		m_unlockEvent = v;
	}

	public override object GetVar(string name)
	{
		return name switch
		{
			"ID" => base.ID, 
			"CARD_ID" => m_cardId, 
			"NAME" => m_name, 
			"SHORT_NAME" => m_shortName, 
			"FLAVOR_TEXT" => m_flavorText, 
			"UNLOCK_EVENT" => m_unlockEvent, 
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
		case "CARD_ID":
			m_cardId = (int)val;
			break;
		case "NAME":
			m_name = (DbfLocValue)val;
			break;
		case "SHORT_NAME":
			m_shortName = (DbfLocValue)val;
			break;
		case "FLAVOR_TEXT":
			m_flavorText = (DbfLocValue)val;
			break;
		case "UNLOCK_EVENT":
			m_unlockEvent = DbfShared.GetEventMap().ConvertStringToSpecialEvent((string)val);
			break;
		}
	}

	public override Type GetVarType(string name)
	{
		return name switch
		{
			"ID" => typeof(int), 
			"CARD_ID" => typeof(int), 
			"NAME" => typeof(DbfLocValue), 
			"SHORT_NAME" => typeof(DbfLocValue), 
			"FLAVOR_TEXT" => typeof(DbfLocValue), 
			"UNLOCK_EVENT" => typeof(string), 
			_ => null, 
		};
	}

	public override IEnumerator<IAsyncJobResult> Job_LoadRecordsFromAssetAsync<T>(string resourcePath, Action<List<T>> resultHandler)
	{
		LoadGuestHeroDbfRecords loadRecords = new LoadGuestHeroDbfRecords(resourcePath);
		yield return loadRecords;
		resultHandler?.Invoke(loadRecords.GetRecords() as List<T>);
	}

	public override bool LoadRecordsFromAsset<T>(string resourcePath, out List<T> records)
	{
		GuestHeroDbfAsset dbfAsset = DbfShared.GetAssetBundle().LoadAsset(resourcePath, typeof(GuestHeroDbfAsset)) as GuestHeroDbfAsset;
		if (dbfAsset == null)
		{
			records = new List<T>();
			Debug.LogError($"GuestHeroDbfAsset.LoadRecordsFromAsset() - failed to load records from assetbundle: {resourcePath}");
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
		m_shortName.StripUnusedLocales();
		m_flavorText.StripUnusedLocales();
	}
}
